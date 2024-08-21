#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Notification;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.Notification
{
    /// <summary>
    /// Génére des instructions de confirmations d'un trade
    /// <para>Alimente la table MCO sans généré le message</para>
    /// </summary>
    public class ConfInstrGenProcess : ProcessTradeBase
    {
        #region Members
        /// <summary>
        /// Message Queue Déclencheur
        /// </summary>
        private readonly ConfirmationInstrGenMQueue confInstrGenMQueue;
        
        /// <summary>
        /// Représente le trade 
        /// </summary>
        private EFS_TradeLibrary tradeLibrary;
        
        /// <summary>
        /// class qui détient les listes des évènements disponibles vis à vis de processTuning  
        /// </summary>
        private RestrictionElement restrictionEvent;
        /// <summary>
        /// Represente les directives pour la messagerie positionnées sur le trade
        /// </summary>
        private TradeNotification tradeNotification;
        /// <summary>
        /// Représente les caractéristiques nécessaires à la messagerie du trade 
        /// </summary>
        private TradeInfo _tradeInfo;
        #endregion Members
        #region accessors
        /// <summary>
        /// Obtient lorsque le traitement s'applique à tous les évènements postérieurs à la date de traitement
        /// </summary>
        public bool IsToEnd
        {
            get { return confInstrGenMQueue.GetBoolValueParameterById(ConfirmationInstrGenMQueue.PARAM_ISTOEND); }
        }

        /// <summary>
        /// Obtient la date de traitement
        /// </summary>
        public DateTime DtStart
        {
            get { return confInstrGenMQueue.GetMasterDate(); }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// Génération des instructions de confirmations
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public ConfInstrGenProcess(ConfirmationInstrGenMQueue pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            confInstrGenMQueue = pMQueue;
        }
        #endregion Constructor
        #region Methods
        /// <summary>
        /// Appel au traitement
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;

            if (ProcessCall == ProcessCallEnum.Master)
            {
                
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 3000), 0,
                    new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
            }

            
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 1,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                new LogParam(MQueue.GetStringValueIdInfoByKey("GPRODUCT"))));

            tradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);
            SetTradeInfo();
            tradeNotification = new TradeNotification();
            tradeNotification.InitializeFromTrade(Cs, CurrentId);

            DeleteMCO();

            ConfirmationChain[] confirmChain = LoadConfirmationChain();
            Boolean isContinue = ArrFunc.IsFilled(confirmChain);
            if (false == isContinue)
                errLevel = Cst.ErrLevel.NOTHINGTODO;

            #region Instruction Generation for Chain
            if (isContinue)
            {
                //Génération des instructions de confirmation
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3007), 1,
                    new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));

                //init ConfirmationChainProcess
                ConfirmationChainProcess[] confirmChainProcess = new ConfirmationChainProcess[ArrFunc.Count(confirmChain)];
                for (int i = 0; i < ArrFunc.Count(confirmChain); i++)
                {
                    confirmChainProcess[i] = new ConfirmationChainProcess(confirmChain[i]);
                    confirmChainProcess[i].InitLogAddProcessLogInfoDelegate(this.ProcessState.SetErrorWarning);
                }

                foreach (ConfirmationChainProcess item in confirmChainProcess)
                {
                    try
                    {
                        string sendCheck = item.CheckConfirmationChain(SendEnum.SendBy);
                        if (StrFunc.IsFilled(sendCheck))
                        {
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-03302",
                                  new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.ABORTED),
                                  LogTools.IdentifierAndId(MQueue.Identifier, CurrentId), "SendBy", sendCheck);
                        }

                        sendCheck = item.CheckConfirmationChain(SendEnum.SendTo);
                        if (StrFunc.IsFilled(sendCheck))
                        {
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-03302",
                                  new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.ABORTED),
                                  LogTools.IdentifierAndId(MQueue.Identifier, CurrentId), "SendTo", sendCheck);
                        }


                        List<string> sendBy = item.GetDisplay(Cs, SendEnum.SendBy);
                        List<string> sendTo = item.GetDisplay(Cs, SendEnum.SendTo);

                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3008), 2,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(sendBy[0]),
                            new LogParam(sendBy[1]),
                            new LogParam(sendBy[2]),
                            new LogParam(sendTo[0]),
                            new LogParam(sendTo[1]),
                            new LogParam(sendTo[2])));

                        Cst.ErrLevel errLevelItem = ExecuteConfirmationChain(item);

                        if (errLevel == Cst.ErrLevel.UNDEFINED)
                            errLevel = errLevelItem;
                        else if (errLevelItem == Cst.ErrLevel.SUCCESS) // ds qu'il existe au minimum 1 succès le traitement est en succès
                            errLevel = errLevelItem;
                    }
                    finally
                    {
                        
                        
                        Logger.Write();
                    }
                }
            }
            #endregion

            return errLevel;
        }

        /// <summary>
        /// Chargement des chaines de confirmation à partir du trade
        /// </summary>
        /// <returns></returns>
        private ConfirmationChain[] LoadConfirmationChain()
        {

            #region Loading confirmation chains
            // Chargement des chaines de confirmation
            // Par Exemple si Spheres® est installé chez un broker et que l'opération est entre une CTR client et une CTR Externe
            // Il y a 2 chaînes (Emetteur "BROKER" Recepteur "CTR" et Emetteur "BROKER" Recepteur CTR Externe)

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3002), 1,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));

            //FI 20120215 Il n'y a pas nécessairement à émettre de la messagerie sur les Trades Risk
            //Ex un CashBalance entre l'entité et une chambre ne donnent lieu à aucune messagerie 
            //Il n'y a aucune pas de chaîne de confirmation ds ce cas
            // EG 20220523 [WI639] Réduction des parties (Buyer/Seller) sur les instruments de facturation
            List<int> partyRestrict = new List<int>();
            if (tradeLibrary.DataDocument.CurrentProduct.IsADM)
                partyRestrict.AddRange(new int[] { _tradeInfo.idA_Buyer, _tradeInfo.idA_Seller });

            ConfirmationChain[] ret = ConfirmationTools.GetConfirmationChain2(CSTools.SetCacheOn(Cs), tradeLibrary.DataDocument, partyRestrict);
            bool isContinue = null != ret;
            // EG 20250126 [WI828] AssetDebtSecurity: La demande de confirmation est en erreur
            if (false == isContinue)
            {
                if (ConfirmationTools.IsTradeOptionalMessage(CSTools.SetCacheOn(Cs), _tradeInfo.idI, _tradeInfo.statusBusiness))
                {
                    //FI 20120315 [17724] Certains trades ne génère pas nécessairement de la messagerie
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3003), 2,
                        new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
                }
                else
                {
                    //Exception s'il n'existe aucune chaine de confirmation => ce n'est pas normal
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-03000",
                        LogTools.IdentifierAndId(MQueue.Identifier, CurrentId));
                }
            }
            #endregion Loading confirmation chains

            #region Exclusions de certaines chaines de confirmation
            if (isContinue)
            {
                //Spheres® exclue les confirmations chain où l'acteur ne possède pas de contact office     
                //De fait cela veut dire que la contrepartie ne veut pas recevoir de message
                List<ConfirmationChain> al = new List<ConfirmationChain>();
                foreach (ConfirmationChain item in ret)
                {
                    if (item.IsExistSendToContactOffice)
                    {
                        al.Add(item);
                    }
                    else
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3004), 2,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(LogTools.IdentifierAndId(item[SendEnum.SendTo].sqlActor.Identifier, item[SendEnum.SendTo].IdActor))));
                    }
                }
                ret = al.ToArray();
                isContinue = ArrFunc.IsFilled(ret);
            }

            if (isContinue)
            {
                // Spheres® exclue les confirmations chain où aucune case n'est cochée sur une partie     
                // De fait cela veut dire que la contrepartie ne veut doit pas recevoir de message pour ce trade

                // RD 20140723 [20173] Bug dans le cas où aucune case n'est cochée sur une partie
                List<ConfirmationChain> al = new List<ConfirmationChain>();
                foreach (ConfirmationChain item in ret)
                {
                    int idASendTo = item[SendEnum.SendTo].IdActor;
                    ActorNotification actorNotification = tradeNotification.GetActorNotification(idASendTo);
                    if (null == actorNotification)
                        throw new NullReferenceException(StrFunc.AppendFormat("Actor Notification is null for actor : {0} ",
                            LogTools.IdentifierAndId(item[SendEnum.SendTo].sqlActor.Identifier, item[SendEnum.SendTo].IdActor)));

                    if (actorNotification.IsConfirmSpecified)
                    {
                        al.Add(item);
                    }
                    else
                    {
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3005), 2,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(LogTools.IdentifierAndId(item[SendEnum.SendTo].sqlActor.Identifier, item[SendEnum.SendTo].IdActor))));
                    }
                }
                ret = al.ToArray();
            }
            #endregion Exclusions de certaines chaines de confirmation

            return ret;
        }



        /// <summary>
        /// Retourne la description de l'évènement {pIdE}
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public override string GetInfoIdE(int pIdE)
        {
            string ret = string.Empty;
            SQL_Event sqlEvent = new SQL_Event(Cs, pIdE);
            sqlEvent.LoadTable(new string[] { "IDE", "EVENTCODE", "EVENTTYPE" });
            if (sqlEvent.IsLoaded)
            {
                StringBuilder sb = new StringBuilder().AppendFormat("Event[Id:{0}, Code:{1}, Type:{2}]", sqlEvent.Id, sqlEvent.EventCode, sqlEvent.EventType);
                ret = sb.ToString();
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ordre SQL de sélection des évènements déclencheurs
        /// </summary>
        /// <param name="pCnfMessage"></param>
        /// <returns></returns>
        /// FI 20150427 [20987] Add
        public QueryParameters GetQueryTriggerEvent(CnfMessage pCnfMessage)
        {
            // FI 20180616 [24718] pIsUseDtEVENTForced = true;
            return ConfirmationTools.GetQueryTriggerEventOfTrade(Cs, CurrentId, pCnfMessage, DtStart, null, IsToEnd, true);
        }


        /// <summary>
        ///  Génère les lignes MCO pour une chaîne de confirmation donnée
        ///  <para>Retourne NOTHINGTODO ou SUCCESS </para>
        /// </summary>
        /// <param name="pConfirmationChainProcess">Représente la chaîne de confirmation</param>
        /// FI 20170913 [23417] Modify
        /// EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel ExecuteConfirmationChain(ConfirmationChainProcess pConfirmationChainProcess)
        {

            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region Recherche des s'il existe des indicateurs (au niveaux Book, ou Entity, etc ...) qui annule l'émission
            bool isOk = pConfirmationChainProcess.IsGenerateMessage(CSTools.SetCacheOn(Cs), NotificationClassEnum.MONOTRADE);
            if (false == isOk)
                ret = Cst.ErrLevel.NOTHINGTODO;
            #endregion

            #region Recherche des messages qui peuvent potentiellement être envoyés
            if (isOk)
            {
                NotificationStepLifeEnum[] stepLife = tradeNotification.GetStepLifeWithActiveConfirmation(pConfirmationChainProcess[SendEnum.SendTo].IdActor);
                NotificationClassEnum[] cnfClass = new NotificationClassEnum[] { NotificationClassEnum.MONOTRADE };
                // FI 20170913 [23417] use _tradeInfo.contractId
                LoadMessageSettings settings = new LoadMessageSettings(cnfClass, null, _tradeInfo.idI, _tradeInfo.idM, _tradeInfo.contractId,
                    _tradeInfo.statusBusiness, _tradeInfo.statusMatch, _tradeInfo.statusCheck,
                    stepLife, null);
                pConfirmationChainProcess.LoadMessages(CSTools.SetCacheOn(Cs), settings);

                //FI 20120502 Add test sur isNcsMatching
                //Test Message non compatible avec au minimum 1 NCS
                if (ArrFunc.IsFilled(pConfirmationChainProcess.cnfMessages.cnfMessage))
                {
                    foreach (CnfMessage item in pConfirmationChainProcess.cnfMessages.cnfMessage.Where(x => (false == x.IsNcsMatching())))
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3360), 2,
                            new LogParam(LogTools.IdentifierAndId(item.identifier, item.idCnfMessage))));
                    }
                }
                pConfirmationChainProcess.cnfMessages.RemoveMessageWithoutNcsMatching();

                isOk = (pConfirmationChainProcess.cnfMessages.Count > 0);
                if (false == isOk)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3020), 2,
                        new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));

                    ret = Cst.ErrLevel.NOTHINGTODO;
                }
            }
            #endregion Load Messages

            #region Load Trade events dates
            if (isOk)
            {
                //Chargement des messages pour lesquels des évènements déclencheurs sont présents
                pConfirmationChainProcess.LoadCnfMessageToSend(Cs, CurrentId, tradeLibrary.DataDocument.CurrentProduct.ProductBase, DtStart, IsToEnd);
                //
                isOk = ArrFunc.IsFilled(pConfirmationChainProcess.cnfMessageToSend);
                if (false == isOk)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3021), 2,
                        new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));

                    ret = Cst.ErrLevel.NOTHINGTODO;
                }
            }
            #endregion Load Trade events dates

            #region Set Ncs and Inci
            if (isOk)
            {
                // Recherche des ICs pour chaque message susceptible d'être envoyé (message qui match avec un des évènements retenus)  
                // Un message peut être envoyé à travers plusieurs NCS
                pConfirmationChainProcess.SetNcsInciChain(CSTools.SetCacheOn(Cs), _tradeInfo);

                // On poursuit s'il existe au moins un message avec des IC complètes de part et d'autres
                isOk = pConfirmationChainProcess.IsExistMessageWithInstruction;
                if (false == isOk)
                    ret = Cst.ErrLevel.NOTHINGTODO;
            }
            #endregion Set Ncs and Inci

            #region Set MCO for each: Message, NCS and date
            if (isOk)
            {
                // Pour chaque Message injection des MCO pour chaque NCS retenu et pour chaque date retenu 
                foreach (CnfMessageToSend cnfMessageToSend  in 
                    pConfirmationChainProcess.cnfMessageToSend.Where(x => ArrFunc.IsFilled(x.NcsInciChain)))
                {
                    SetRestrictionEvent((CnfMessage)cnfMessageToSend);
                    SetMCO(pConfirmationChainProcess, cnfMessageToSend);
                }
            }
            #endregion
            return ret;
        }

        /// <summary>
        /// Recherche des évènements compatibles vis à vis de processTuning 
        /// <para>Alimente l'élément restrictionEvent</para>
        /// </summary>
        /// <exception cref="SpheresException2[DATANOTFOUND] si aucun évènement compatible avec processTuning"></exception>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SetRestrictionEvent(CnfMessage pCnfMessage)
        {
            //Add Restriction on EVENT 
            CnfMessageEventRestriction processEventRestriction = new CnfMessageEventRestriction(this, pCnfMessage);
            //
            restrictionEvent = new RestrictionElement(processEventRestriction);
            restrictionEvent.Initialize(this.Cs);
            RestrictionItem[] item = restrictionEvent.GetRestrictItem();
            //
            if (ArrFunc.IsFilled(item))
            {
                for (int i = 0; i < item.Length; i++)
                {
                    if (false == item[i].IsEnabled)
                    {
                        Pair<int, Pair<string, string>> evt = GetInfoEvent(item[i].id);
                        if (null != evt)
                        {
                            ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.LOG, 3010), 0,
                                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                                new LogParam(evt.First),
                                new LogParam(evt.Second.First),
                                new LogParam(evt.Second.Second)));
                        }
                    }
                }
            }
            //
            if (ArrFunc.IsEmpty(restrictionEvent.GetRestrictItemEnabled()))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-03004",
                        new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnDataNotFoundEnum),
                        LogTools.IdentifierAndId(MQueue.Identifier, CurrentId));
            }
        }

        /// <summary>
        /// Alimentation de la table MCO avec les intructions retenues sur chaque NCS
        /// </summary>
        /// <param name="pCnfMessageProcess"></param>
        /// FI 20150427 [20987] Modify
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SetMCO(ConfirmationChainProcess pCnfChainProcess, CnfMessageToSend pCnfMessageToSend)
        {
            IDbTransaction dbTransaction = null;

            bool isOk = true;
            try
            {
                DataSetEventTrade dsEvent = null;
                //
                if (ProcessTuningSpecified)
                {
                    int[] idE = restrictionEvent.GetIdEnabled();
                    if (ArrFunc.IsFilled(idE))
                    {
                        // EG 20150612 [20665] Chargement direct
                        dsEvent = new DataSetEventTrade(Cs, idE);
                    }
                }

                dbTransaction = DataHelper.BeginTran(Cs);

                
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3009), 2,
                    new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                    new LogParam(LogTools.IdentifierAndId(pCnfMessageToSend.identifier, pCnfMessageToSend.idCnfMessage))));

                //Add MCO
                ArrayList lds = new ArrayList();
                for (int i = 0; i < ArrFunc.Count(pCnfMessageToSend.DateInfo); i++)
                {
                    DatasetConfirmationMessageProcess ds = new DatasetConfirmationMessageProcess(0, pCnfMessageToSend.DateInfo[i].dateEvent, IsModeSimul);
                    ds.LoadDs(Cs, dbTransaction, CurrentId, pCnfMessageToSend.DateInfo[i].dateToSend);
                    //
                    lds.Add(ds);
                    //AddRowMCO pour chaque instruction (une instruction par NCS)
                    //exemple Un message envoyé par FAX et par EMAIL => 2 instructions
                    for (int k = 0; k < ArrFunc.Count(pCnfMessageToSend.NcsInciChain); k++)
                    {
                        bool isNewRow = (!ConfirmationTools.IsMessageGenerated(Cs, CurrentId, pCnfMessageToSend.DateInfo[i].dateEvent,
                                                                (CnfMessage)pCnfMessageToSend,
                                                                pCnfMessageToSend.NcsInciChain[k].ncs.idNcs,
                                                                (ConfirmationChain)pCnfChainProcess));
                        if (isNewRow)
                        {
                            ds.AddRowMCOConfirm(Cs, 0, CurrentId, pCnfMessageToSend.DateInfo[i].dateToSend,
                                        (CnfMessage)pCnfMessageToSend,
                                         (ConfirmationChain)pCnfChainProcess,
                                         pCnfMessageToSend.NcsInciChain[k].ncs.idNcs,
                                         pCnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendBy].idInci,
                                         pCnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendTo].idInci, Session.IdA);
                        }
                        else
                        {

                            string msg = StrFunc.AppendFormat("message:{0},ncs:{1},date:{2} already generated",
                                pCnfMessageToSend.identifier,
                                pCnfMessageToSend.NcsInciChain[k].ncs.identifier,
                                DtFunc.DateTimeToStringDateISO(pCnfMessageToSend.DateInfo[i].dateEvent));

                            ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, msg));
                        }
                    }
                }

                //Add MCODET
                DatasetConfirmationMessageProcess[] ads = (DatasetConfirmationMessageProcess[])lds.ToArray(typeof(DatasetConfirmationMessageProcess));
                for (int i = 0; i < ArrFunc.Count(ads); i++)
                {
                    ads[i].SetIdMCO(Cs, dbTransaction, false);
                    DataTable dt = ads[i].DtMCO.GetChanges(DataRowState.Added);
                    if (null != dt)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            using (IDataReader dr = GetEventMco(Cs, (CnfMessage)pCnfMessageToSend, ads[i].dtEvent, CurrentId))
                            {
                                while (dr.Read())
                                {
                                    int idE = Convert.ToInt32(dr["ID"]);
                                    int idMco = Convert.ToInt32(row["ID"]);
                                    ads[i].AddRowMCODET(Cs, idMco, idE, Session.IdA);
                                }
                            }

                            try
                            {
                                EventProcess eventProcess = new EventProcess(Cs);
                                int idMco = Convert.ToInt32(row["ID"]);
                                int[] idE = ads[i].GetIdEventsInMCODET(idMco);
                                for (int j = 0; j < ArrFunc.Count(idE); j++)
                                {
                                    // FI 20200820 [25468] dates systemes en UTC
                                    eventProcess.Write(dbTransaction, idE[j], MQueue.ProcessType, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(Cs), Tracker.IdTRK_L, idMco);
                                    if (null != dsEvent)
                                    {
                                        // FI 20200820 [25468] dates systemes en UTC
                                        dsEvent.SetEventStatus(idE[j], ProcessTuning.GetProcessTuningOutput(Tuning.TuningOutputTypeEnum.OES), Session.IdA, OTCmlHelper.GetDateSysUTC(Cs));
                                    }
                                }
                            }
                            catch (Exception) { throw; }
                        }
                    }
                    //
                    ads[i].ExecuteDataAdapterMCO(Cs, dbTransaction);
                    ads[i].ExecuteDataAdapterMCODET(Cs, dbTransaction);
                }
                if (null != dsEvent)
                {
                    dsEvent.Update(dbTransaction);
                }
                //PL 20151229 Use DataHelper.CommitTran()
                //dbTransaction. Commit();
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception) { isOk = false; throw; }
            finally
            {
                if (null != dbTransaction)
                {
                    if (false == isOk)
                    {
                        //PL 20151229 Use DataHelper.RollbackTran()
                        //dbTransaction. Rollback();
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Retourne les évènements déclencheurs du message
        /// </summary>
        /// FI 20150427 [20987] Modify
        private static IDataReader GetEventMco(string pCs, CnfMessage pCnfMessage, DateTime pDateEvent, int pIdT)
        {
            // FI 20180616 [24718] isUseDtEVENTForced ;
            bool isUseDtEVENTForced;
            switch (ConfirmationTools.MCOmode)
            {
                case ConfirmationTools.MCOModeEnum.DTEVENT:
                    isUseDtEVENTForced = false;
                    break;
                case ConfirmationTools.MCOModeEnum.DTEVENTFORCED:
                    isUseDtEVENTForced = true;
                    break;
                default:
                    throw new InvalidProgramException(StrFunc.AppendFormat("{0} is not supported", ConfirmationTools.MCOmode.ToString()));
            }
            QueryParameters queryParameters = ConfirmationTools.GetQueryTriggerEventOfTrade(pCs, pIdT, pCnfMessage, pDateEvent, null, false, isUseDtEVENTForced);
            IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return dr;
        }

        /// <summary>
        /// Suppression des MCO existants pour lesquels il n'existe pas encore de flux XML généré
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void DeleteMCO()
        {
            //Purge des anciennes instructions non encore valorisées (dont le flux xml est non calculé)

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3001), 1,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(Cs, "IDT", DbType.Int32), CurrentId);
            if (DtFunc.IsDateTimeFilled(DtStart))
                dataParameters.Add(new DataParameter(Cs, "DTSTART", DbType.Date), DtStart); // FI 20201006 [XXXXX] DbType.Date
            //
            StrBuilder query = new StrBuilder();
            query += SQLCst.DELETE_DBO + Cst.OTCml_TBL.MCO + Cst.CrLf;
            query += SQLCst.WHERE + SQLCst.EXISTS + Cst.CrLf;
            query += "(select 1 " + SQLCst.FROM_DBO + Cst.OTCml_TBL.MCODET + " mcodet " + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e on e.IDE = mcodet.IDE " + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + " ec on ec.IDE = e.IDE " + Cst.CrLf;
            query += SQLCst.WHERE + " mcodet.IDMCO=MCO.IDMCO and MCO.CNFMSGXML is null and e.IDT=@IDT";

            if (dataParameters.Contains("DTSTART"))
            {
                if (IsToEnd)
                    query += SQLCst.AND + "ec.DTEVENTFORCED >= @DTSTART" + Cst.CrLf;
                else
                    query += SQLCst.AND + "ec.DTEVENTFORCED = @DTSTART" + Cst.CrLf;
            }
            query += ")";

            QueryParameters qryParameters = new QueryParameters(Cs, query.ToString(), dataParameters);

            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Alimente _tradeInfo
        /// </summary>
        /// FI 20140808 [20275] Modify
        /// FI 20170913 [23417] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void SetTradeInfo()
        {
            if (null == tradeLibrary)
                throw new InvalidOperationException("tradeLibrary is null");


            ProductContainer productContainer = tradeLibrary.DataDocument.CurrentProduct;
            int idI = productContainer.IdI;
            int idIUnderlyer = (int)((null == productContainer.GetUnderlyingAssetIdI()) ? 0 : productContainer.GetUnderlyingAssetIdI());

            string idC = string.Empty;
            if (null != tradeLibrary.DataDocument.GetMainCurrencyAmount(CSTools.SetCacheOn(Cs)))
                idC = Tools.GetIdC(Cs, tradeLibrary.DataDocument.GetMainCurrencyAmount(Cs).Currency);

            Pair<Cst.ContractCategory, int> contractId = null;
            int idM = 0;
            // EG 20220523 [WI639] Inutile pour les trades ADM
            if (false ==productContainer.IsADM)
            {
                tradeLibrary.DataDocument.CurrentProduct.GetContract(Cs, null, SQL_Table.ScanDataDtEnabledEnum.No, DateTime.MinValue, out Pair<Cst.ContractCategory, SQL_TableWithID> contract);
                if (null != contract)
                    contractId = new Pair<Cst.ContractCategory, int>(contract.First, contract.Second.Id);

                tradeLibrary.DataDocument.CurrentProduct.GetMarket(CSTools.SetCacheOn(Cs), null, out SQL_Market sqlMarket);
                if (null != sqlMarket)
                    idM = sqlMarket.Id;
            }

            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(Cs, CurrentId);
            sqlTrade.LoadTable(new string[] { "TRADE.IDT, IDENTIFIER, IDSTBUSINESS, IDA_BUYER, IDA_SELLER" });

            _tradeInfo = new TradeInfo
            {
                idT = sqlTrade.Id,
                identifier = sqlTrade.Identifier,
                statusBusiness = sqlTrade.IdStBusinessEnum,
                idM = idM,
                idI = idI,
                idIUnderlyer = idIUnderlyer,
                idC = idC,
                contractId = contractId,
                // EG 20220523 [WI639] Réduction des parties (Buyer/Seller) sur les instruments de facturation
                idA_Buyer = sqlTrade.IdA_Buyer.Value,
                idA_Seller = sqlTrade.IdA_Seller.Value,
            };
            

            TradeStatus tradeSt = new TradeStatus();
            tradeSt.InitializeStUsers(Cs, _tradeInfo.idT);
            _tradeInfo.statusMatch = tradeSt.GetTickedTradeStUser(StatusEnum.StatusMatch, "}{");
            _tradeInfo.statusCheck = tradeSt.GetTickedTradeStUser(StatusEnum.StatusCheck, "}{");


        }
        #endregion
    }


    /// <summary>
    /// Génération des message de confirmation
    /// </summary>
    public class ConfMsgGenProcess : ProcessBase
    {
        #region Members
        public ConfirmationMsgGenMQueue confMsgGenMQueue;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Obtient true si regénération des messages existants
        /// </summary>
        protected virtual bool IsModeRegenerate
        {
            get
            {
                return confMsgGenMQueue.GetBoolValueParameterById(ConfirmationMsgGenMQueue.PARAM_ISMODEREGENERATE);
            }
        }

        /// <summary>
        ///  Obtient true si envoi du message 
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsWithIO
        {
            get { return confMsgGenMQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISWITHIO); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.MCO;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.MCO.ToString();
            }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ConfMsgGenProcess() : base() { }
        /// <summary>
        /// Génération des messages de confirmation
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public ConfMsgGenProcess(ConfirmationMsgGenMQueue pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            confMsgGenMQueue = pMQueue;
        }
        #endregion Constructor
        #region Method

        /// <summary>
        /// Recherche des enregistrements MCO (IDMCO) candidats à génération de confirmation en fonction des paramètres du Mqueue
        /// </summary>
        /// FI 20240522 [WI937] Refactoring
        protected override void SelectDatas()
        {
            QueryParameters queryParamers = GetQueryMCO();

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += @"mco.IDMCO As IDDATA" + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + Cst.CrLf;
            sqlSelect += "(" + queryParamers.Query + ") mco";

            QueryParameters queryParamers2 = new QueryParameters(Cs, sqlSelect.ToString(), queryParamers.Parameters);

            DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, queryParamers2.Query, queryParamers2.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Retourne la query de selection des enregistrements MCO candidats à génération de confirmation à partir des paramètres du Mqueue
        /// </summary>
        /// <returns></returns>
        /// EG 20100310 [16881]
        /// FI 20240522 [WI937] Refactoring
        private QueryParameters GetQueryMCO()
        {
            DataParameters parameters = GetMQueueDataParameters();

            String sqlSelect = @"select mco.IDMCO,mco.DTMCO,mco.DTMCOFORCED,
mco.IDCNFMESSAGE,mco.IDA_NCS,
mco.IDA_SENDBYPARTY,mco.IDA_SENDBYOFFICE,mco.IDINCI_SENDBY,
mco.IDA_SENDTOPARTY,mco.IDA_SENDTOOFFICE,mco.IDINCI_SENDTO,
mco.IDT
from dbo.MCO mco";

            SQLWhere sqlWhere = GetMQueueSqlWhere();

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += sqlSelect + Cst.CrLf + sqlWhere.ToString() + Cst.CrLf;

            QueryParameters queryParameters = new QueryParameters(Cs, sqlQuery.ToString(), parameters);

            return queryParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();
            //
            if (false == IsProcessObserver)
            {
                #region ProcessTuning => Initialisation from Trade
                int[] idT = GetIdT();
                //
                if (1 == ArrFunc.Count(idT))
                {
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(Cs, idT[0]);
                    if (sqlTrade.LoadTable(new string[] { "IDT", "IDI" }))
                    {
                        ProcessTuning = new ProcessTuning(Cs, sqlTrade.IdI, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                    }
                }
                else
                {
                    ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                }
                //
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    
                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
                #endregion
            }
        }

        /// <summary>
        /// Verification ultime avant execution du traitement 
        /// <para>- Mise en place d'un lock</para>
        /// <para>- Verification du respect par rapport à processTuning</para>
        /// </summary>
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();

            if (false == IsProcessObserver)
            {
                // Lock 
                //  Lock du message MCO 
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                    ProcessState.CodeReturn = LockCurrentObjectId();

                // Lock des trades en rapport avec Message 
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    int[] idT = GetIdT();
                    for (int i = 0; i < ArrFunc.Count(idT); i++)
                    {
                        ProcessState.CodeReturn = ScanCompatibility_Trade(idT[i]);
                        if (Cst.ErrLevel.SUCCESS != ProcessState.CodeReturn)
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            // RD 20161031 [22511] Load obsolete report             
            DatasetConfirmationMessage dsMco = GetDsMCO(Cs, IsModeSimul, false, CurrentId);

            if (ArrFunc.IsEmpty(dsMco.DtMCO.Rows))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03466",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), CurrentId.ToString());
            }
            //RD 20161031 [22511] Display Log message for obsolete report
            else if (ArrFunc.IsEmpty(dsMco.DtMCO.Select("DTOBSOLETE is null")))
            {
                DataRow rowIdMCO = dsMco.GetRowIdMco(CurrentId);
                SQL_Actor sendToActor = new SQL_Actor(Cs, Convert.ToInt32(rowIdMCO["IDA_SENDTOOFFICE"]));
                string sendToIdentifier;
                if (sendToActor.LoadTable(new string[] { "IDA", "IDENTIFIER" }))
                    sendToIdentifier = LogTools.IdentifierAndId(sendToActor.Identifier, sendToActor.Id);
                else
                    sendToIdentifier = LogTools.IdentifierAndId(string.Empty, string.Empty);

                SQL_ConfirmationMessage cnfMessage = new SQL_ConfirmationMessage(Cs, Convert.ToInt32(rowIdMCO["IDCNFMESSAGE"]));
                string cnfMessageIdentifier;
                if (cnfMessage.LoadTable(new string[] { "IDCNFMESSAGE", "IDENTIFIER" }))
                    cnfMessageIdentifier = LogTools.IdentifierAndId(cnfMessage.Identifier, cnfMessage.Id);
                else
                    cnfMessageIdentifier = LogTools.IdentifierAndId(string.Empty, string.Empty);

                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03473",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                    LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowIdMCO["DTMCO"])), CurrentId),
                    DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowIdMCO["DTMCOFORCED"])),
                    DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowIdMCO["DTOBSOLETE"])),
                    sendToIdentifier,
                    cnfMessageIdentifier);
            }

            DatasetConfirmationMessageManager mcoMng = new DatasetConfirmationMessageManager(dsMco, this);
            mcoMng.InitLogAddProcessLogInfoDelegate(LogLevelDetail.LEVEL3);

            if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn))
            {
                BuildMessage(mcoMng);

                if (IsWithIO && (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                    mcoMng.ExportationMessage(Cs, CurrentId);
            }
            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            int[] idT = GetIdT();
            //
            for (int i = 0; i < ArrFunc.Count(idT); i++)
                SetTradeStatus(idT[i], ProcessState.Status);
        }

        /// <summary>
        ///  Retourne un nouveau DatasetConfirmationMessage 
        /// </summary>
        /// <returns></returns>
        // RD 20161031 [22511] Add param pWithoutObsolete        
        protected static DatasetConfirmationMessage GetDsMCO(string pCS, bool pIsModeProcessSimul, bool pWithoutObsolete, int pIdMCO)
        {
            DatasetConfirmationMessage ds = new DatasetConfirmationMessage(pIsModeProcessSimul);
            ds.LoadDs(pCS, null, pIdMCO, pWithoutObsolete);
            return ds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected DataParameters GetMQueueDataParameters()
        {

            DataParameters dataParameters = new DataParameters();
            //
            if (false == (confMsgGenMQueue.idSpecified))
            {
                if (confMsgGenMQueue.parametersSpecified)
                {
                    if (null != confMsgGenMQueue.GetObjectValueParameterById(ConfirmationMsgGenMQueue.PARAM_DATE1))
                    {
                        dataParameters.Add(new DataParameter(Cs, "DATE1", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                        dataParameters["DATE1"].Value = Convert.ToDateTime(confMsgGenMQueue.GetDateTimeValueParameterById(ConfirmationMsgGenMQueue.PARAM_DATE1));
                    }
                    if (null != confMsgGenMQueue.GetObjectValueParameterById(ConfirmationMsgGenMQueue.PARAM_IDT))
                    {
                        dataParameters.Add(new DataParameter(Cs, "IDT", DbType.Int32));
                        dataParameters["IDT"].Value = Convert.ToInt32(confMsgGenMQueue.GetIntValueParameterById(ConfirmationMsgGenMQueue.PARAM_IDT));
                    }
                }
            }
            else
            {
                dataParameters.Add(new DataParameter(Cs, "IDMCO", DbType.Int32), confMsgGenMQueue.id);
            }
            //
            return dataParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20240522 [WI937] add DTOBSOLETE is null
        private SQLWhere GetMQueueSqlWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();

            DataParameters parameters = GetMQueueDataParameters();
            if (parameters.Contains("DATE1"))
                sqlWhere.Append("mco.DTMCOFORCED=@DATE1");

            if (parameters.Contains("IDT"))
                sqlWhere.Append("mco.IDT=@IDT");

            if (parameters.Contains("IDMCO"))
                sqlWhere.Append("mco.IDMCO=@IDMCO");
            
            sqlWhere.Append("mco.CNFMSGXML is null");

            // FI 20190515 [23912]  Ne pas considérer les message Obsolete
            sqlWhere.Append("mco.DTOBSOLETE is null");

            return sqlWhere;
        }

        /// <summary>
        ///  Retourne les trade d'un MCO
        /// </summary>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        private int[] GetIdT()
        {
            int[] ret = null;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "IDMCO", DbType.Int32), CurrentId);

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT_DISTINCT + "e.IDT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MCO + " mco " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MCODET + " mcodet on mcodet.IDMCO = mco.IDMCO " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " e on e.IDE = mcodet.IDE " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "mco.IDMCO=@IDMCO";

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter()))
            {
                ArrayList al = new ArrayList();
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr.GetValue(0)));
                ret = (int[])al.ToArray(typeof(int));
            }
            return ret;
        }

        /// <summary>
        /// Génère ou Regénère potentiellement un message existant
        /// </summary>
        /// FI 20160624 [22286] Modify
        private void BuildMessage(DatasetConfirmationMessageManager mcoMng)
        {
            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(Cs);
                // FI 20160624 [22286]  pIsLoadPrevious = false
                // RD 20160905 [21961][22286]  pIsLoadPrevious = true
                mcoMng.GenerateMessage(Cs, dbTransaction, CurrentId, IsModeRegenerate, true);
                //PL 20151229 Use DataHelper.CommitTran()
                //dbTransaction. Commit();
                DataHelper.CommitTran(dbTransaction);
            }
            catch
            {
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);

                // FI 20160701 [22069] Add throw
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
        }
        #endregion
    }

}

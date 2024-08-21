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
using EFS.SpheresService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Notification;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
//
using Tz = EFS.TimeZone;
using EfsML.v30.MiFIDII_Extended;
#endregion Using Directives

namespace EFS.Process.Notification
{
    /// <summary>
    /// Représente une chaine de confirmation et la liste des messages à envoyer pour cette chaîne de comfirmation
    /// </summary>
    internal class ConfirmationChainProcess : ConfirmationChain
    {
        #region Members
        /// <summary>
        /// Représente les messages de confirmation compatibles avec la chaîne de confirmation
        /// </summary>
        public CnfMessages cnfMessages;
        /// <summary>
        /// Représente les messages de confirmation compatibles avec la chaîne de confirmation qui sont à envoyer 
        /// </summary>
        public CnfMessageToSend[] cnfMessageToSend;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        private SetErrorWarning _SetErrorWarning = null;


        #endregion Members
        #region accessor
        /// <summary>
        /// Obtient true s'il au minimum 1 message à envoyer pour la chaîne de confirmation
        /// </summary>
        public bool IsExistMessageWithInstruction
        {
            get
            {
                bool ret = false;

                if (ArrFunc.IsFilled(cnfMessageToSend))
                {
                    var exist = (from item in cnfMessageToSend.Where(x => ArrFunc.IsFilled(x.NcsInciChain))
                                 select item).FirstOrDefault();

                    ret = (exist != null);
                }

                return ret;
            }
        }
        #endregion
        #region constructor
        public ConfirmationChainProcess(ConfirmationChain pConfirmationChain)
            : base()
        {
            this[SendEnum.SendBy].sqlActor = pConfirmationChain[SendEnum.SendBy].sqlActor;
            if (null != pConfirmationChain[SendEnum.SendBy].sqlBook)
                this[SendEnum.SendBy].sqlBook = pConfirmationChain[SendEnum.SendBy].sqlBook;
            this[SendEnum.SendBy].sqlContactOffice = pConfirmationChain[SendEnum.SendBy].sqlContactOffice;
            if (null != pConfirmationChain[SendEnum.SendBy].sqlTrader)
                this[SendEnum.SendBy].sqlTrader = pConfirmationChain[SendEnum.SendBy].sqlTrader;
            this.IsSendByActor_Entity = pConfirmationChain.IsSendByActor_Entity;

            this[SendEnum.SendTo].sqlActor = pConfirmationChain[SendEnum.SendTo].sqlActor;
            if (null != pConfirmationChain[SendEnum.SendTo].sqlBook)
                this[SendEnum.SendTo].sqlBook = pConfirmationChain[SendEnum.SendTo].sqlBook;
            this[SendEnum.SendTo].sqlContactOffice = pConfirmationChain[SendEnum.SendTo].sqlContactOffice;
            if (null != pConfirmationChain[SendEnum.SendTo].sqlTrader)
                this[SendEnum.SendTo].sqlTrader = pConfirmationChain[SendEnum.SendTo].sqlTrader;
            this.IsSendTo_Broker = pConfirmationChain.IsSendTo_Broker;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retourne un message d'erreur si la chaîne de confirmation n'est pas correcte
        /// </summary>
        /// <returns></returns>
        public string CheckConfirmationChain(SendEnum pSend)
        {

            string ret = string.Empty;
            if (null == this[pSend].sqlActor || (false == this[pSend].sqlActor.IsLoaded))
                ret += " Actor";
            if (null == this[pSend].sqlContactOffice || (false == this[pSend].sqlContactOffice.IsLoaded))
                ret += " Contact office";
            return ret;
        }

        /// <summary>
        /// Retourne true s'il n'existe pas de contre-indication (au niveau Book, Entity...) concernant l'envoi et la réception de message de Notifications/Confirmations.
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pNotificationClass">Type de messagerie (Edition simple, consolidée ou confirmation)</param>
        /// <returns></returns>
        /// FI 20120418 [17752] Refactoring
        /// RD 20151008 [21139] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        public bool IsGenerateMessage(string pCs, NotificationClassEnum pNotificationClass)
        {
            bool ret = true;
            #region -- SendTo (Destinataire) ------------------------------------------------------------------------------------------------------
            if (null != this[SendEnum.SendTo].sqlBook)
            {
                // Contrôle du book du destinataire pour savoir s'il est paramétré pour recevoir des Notifications/Confirmations.
                SQL_Book sqlBook = this[SendEnum.SendTo].sqlBook;
                ret = (sqlBook.IsReceiveNcMsg);
                if (false == ret)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3023), 2,
                        new LogParam(LogTools.IdentifierAndId(sqlBook.Identifier, sqlBook.Id))));
                }
            }
            #endregion

            #region -- SendBy (Emetteur) ----------------------------------------------------------------------------------------------------------
            //PL 20111128 Ajout du test sur: ret=true
            if (ret)
            {
                int idAEntity;
                if (IsSendByActor_Entity)
                {
                    // Cas où l'emission est pilotée par l'entité, celle-ci se trouve alors dans sqlActor
                    // ex. 
                    //     - Emission d'une confirmation à un Client depuis un Trade Client vs Entity
                    //     - Emission d'une confirmation à une Contrepartie externe depuis un Trade Client vs Contrepartie externe
                    idAEntity = this[SendEnum.SendBy].sqlActor.Id;
                }
                else
                {
                    // Cas où l'emission est pilotée par la partie elle même, on obtient alors son entité depuis sqlBook
                    // ex. 
                    //     - Emission d'une confirmation à une Contrepartie externe depuis un Trade Entity vs Contrepartie externe
                    //     - Emission d'une confirmation à une Entity depuis un Trade Entity E1 vs Entity E2
                    idAEntity = this[SendEnum.SendBy].sqlBook.IdA_Entity;
                }
                if (idAEntity == 0)
                    throw new Exception("Entity not Found");

                SQL_Entity sqlEntity = new SQL_Entity(pCs, idAEntity);
                // FI/CC 20121126 Ticket 18285 ajout colonne IDA + modification du status (passage de StatusErrorEnum à StatusWarningEnum)
                bool isEntityParameterFound = sqlEntity.LoadTable(new string[] { sqlEntity.AliasActorTable + ".IDA", "ISSENDNCMSG_CLIENT", "ISSENDNCMSG_ENTITY", "ISSENDNCMSG_EXT" });

                if (isEntityParameterFound)
                {
                    NotificationSendToClass sendToClass = this.SendTo(pCs, pNotificationClass);
                    switch (sendToClass)
                    {
                        case NotificationSendToClass.Client:
                            ret = sqlEntity.IsSendNcMsgClient;
                            if (false == ret)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3024), 2,
                                    new LogParam(LogTools.IdentifierAndId(sqlEntity.Identifier, sqlEntity.Id))));
                            }
                            break;
                        case NotificationSendToClass.Entity:
                            ret = sqlEntity.IsSendNcMsgHouse;
                            if (false == ret)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3025), 2,
                                    new LogParam(LogTools.IdentifierAndId(sqlEntity.Identifier, sqlEntity.Id))));
                            }
                            break;
                        case NotificationSendToClass.External:
                            ret = sqlEntity.IsSendNcMsgExt;
                            if (false == ret)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3026), 2,
                                    new LogParam(LogTools.IdentifierAndId(sqlEntity.Identifier, sqlEntity.Id))));
                            }
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", sendToClass.ToString()));
                    }
                }
            }
            #endregion
            return ret;
        }
        /// <summary>
        /// Charge les messages compatibles avec la chaîne de confirmation et plusieurs directives supplémentaires
        /// <para></para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pSettings">Représente les directives pour charger les messages</param>
        public void LoadMessages(string pCs, LoadMessageSettings pSettings)
        {
            cnfMessages = LoadCnfMessage(pCs, pSettings);
        }

        /// <summary>
        /// Charge les messages à envoyer parmi les messages compatibles. 
        /// <para>Ceux qui sont à envoyer sont ceux où s'il existe au moin un évènement déclencheur</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pDateStart">Date initiale de recherche des évènements</param>
        /// <param name="pIsToEnd">true pour scruter tous les évènements postérieurs à la date initiale</param>
        public void LoadCnfMessageToSend(string pCs, int pIdT, IProductBase pProductBase, DateTime pDateStart, bool pIsToEnd)
        {
            ArrayList al = new ArrayList();

            if ((null != cnfMessages) && (cnfMessages.Count > 0))
            {
                for (int i = 0; i < cnfMessages.Count; i++)
                {
                    CnfMessageToSend item = new CnfMessageToSend(cnfMessages[i]);
                    
                    ArrayList alDates = new ArrayList();
                    Pair<DateTime,DateTime>[] date = item.GetTriggerEventDate(pCs, pIdT);
                    for (int j = 0; j < ArrFunc.Count(date); j++)
                    {
                        bool isToAdd;
                        if (pIsToEnd)
                            isToAdd = date[j].Second.CompareTo(pDateStart) >= 0;
                        else
                            isToAdd = (date[j].Second.CompareTo(pDateStart) == 0);
                        
                        if (isToAdd)
                        {
                            // FI 20180616 [24718] Alimentation de dtEvent en fonction du mode 
                            DateTime dtEvent;
                            switch (ConfirmationTools.MCOmode)
                            {
                                case ConfirmationTools.MCOModeEnum.DTEVENT:
                                    dtEvent = date[j].First;
                                    break;
                                case ConfirmationTools.MCOModeEnum.DTEVENTFORCED:
                                    dtEvent = date[j].Second;
                                    break;
                                default:
                                    throw new InvalidProgramException(StrFunc.AppendFormat("{0} is not supported", ConfirmationTools.MCOmode.ToString()));
                            }
                            DateTime dtToSend = item.GetDateToSend(pCs, dtEvent, this, pProductBase);
                            DateTime dtToSendForced = OTCmlHelper.GetAnticipatedDate(pCs, dtToSend);
                            alDates.Add(new NotificationSendDateInfo(dtEvent, dtToSend, dtToSendForced));
                        }
                    }
                    if (ArrFunc.IsFilled(alDates))
                        item.DateInfo = (NotificationSendDateInfo[])alDates.ToArray(typeof(NotificationSendDateInfo));
                    
                    if (ArrFunc.IsFilled(item.DateInfo))
                        al.Add(item);
                }
            }

            if (ArrFunc.Count(al) > 0)
                cnfMessageToSend = (CnfMessageToSend[])al.ToArray(typeof(CnfMessageToSend));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20120131 [] La liste doit nécessairement contenir 3 items
        public List<string> GetDisplay(string pCS, SendEnum pSend)
        {
            List<string> ret = new List<string>();

            //Item 1 
            if (null != this[pSend].sqlContactOffice && this[pSend].sqlContactOffice.IsLoaded)
                ret.Add("[Contact Office: " + LogTools.IdentifierAndId(this[pSend].sqlContactOffice.Identifier, this[pSend].sqlContactOffice.Id) + "]");
            else
                ret.Add("[Contact Office: <Unknown>]");

            if ((pSend == SendEnum.SendBy) && (IsSendTo_Client(pCS) || IsSendTo_Broker))
            {
                //Item 2 
                if (null != this[pSend].sqlActor && this[pSend].sqlActor.IsLoaded)
                    ret.Add("[Entity: " + LogTools.IdentifierAndId(this[pSend].sqlActor.Identifier, this[pSend].sqlActor.Id) + "]");
                else
                    ret.Add("");

                //Item 3
                if (null != this[SendEnum.SendTo].sqlBook && this[SendEnum.SendTo].sqlBook.IsLoaded)
                    ret.Add("[Entity of book: " + LogTools.IdentifierAndId(this[SendEnum.SendTo].sqlBook.Identifier, this[SendEnum.SendTo].sqlBook.Id) + "]");
                else
                    ret.Add("");
            }
            else if ((pSend == SendEnum.SendTo) && IsSendTo_Broker)
            {
                //Item 2
                if (null != this[pSend].sqlActor && this[pSend].sqlActor.IsLoaded)
                    ret.Add("[Broker of party: " + LogTools.IdentifierAndId(this[pSend].sqlActor.Identifier, this[pSend].sqlActor.Id) + "]");
                else
                    ret.Add("");

                //Item 3
                ret.Add("");
            }
            else
            {
                //Item 2
                if (null != this[pSend].sqlActor && this[pSend].sqlActor.IsLoaded)
                    ret.Add("[Party: " + LogTools.IdentifierAndId(this[pSend].sqlActor.Identifier, this[pSend].sqlActor.Id) + "]");
                else
                    ret.Add("");

                //Item 3
                if (null != this[pSend].sqlBook && this[pSend].sqlBook.IsLoaded)
                    ret.Add("[Book: " + LogTools.IdentifierAndId(this[pSend].sqlBook.Identifier, this[pSend].sqlBook.Id) + "]");
                else
                    ret.Add("");
            }
            return ret;
        }

        /// <summary>
        /// Pour chaque message à envoyer
        /// <para>
        /// Recherche,en fonction des instructions du contact office du destinatiare (prioritaire) et du contact office de l'émetteur, 
        /// les NCS (système de notification) sur lequel le message doit être envoyé   
        /// </para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdIUnderlyer"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pTradeStBusiness"></param>
        /// <example>
        /// Ex soit un message A
        /// Le Contact office destinataire a les ICs suivantes
        /// recevoir un email (aux adresses xxx) + recevoir un fax (à l'adresse yyy)
        /// Le Contact office emetteur a les ICs suivantes
        /// Envoyé les email et/ou les fax
        /// 
        /// Si le message est compatible Email et Fax
        /// Spheres génère le message à travers les 2 ncs  
        /// Chaque couple (CnfMessage,NCs) possèdent les 2 instructions INCIs  (=> matérialisé par InciChain)
        /// </example>
        /// FI 20170913 [23417] Modify
        internal bool SetNcsInciChain(string pCs, TradeInfo tradeInfo)
        {
            bool ret = true;
            //
            for (int i = 0; i < ArrFunc.Count(cnfMessageToSend); i++)
            {
                // FI 20170913 [23417] use tradeInfo.contractId
                CnfMessageToSend.SetNcsInciChainErrLevel errLevel = cnfMessageToSend[i].SetNcsInciChain(pCs, this,
                    tradeInfo.idM,
                    tradeInfo.idI, tradeInfo.idIUnderlyer, tradeInfo.idC, tradeInfo.contractId,
                    tradeInfo.statusBusiness.ToString(), tradeInfo.statusMatch, tradeInfo.statusCheck);

                if (CnfMessageToSend.SetNcsInciChainErrLevel.Succes != errLevel)
                {
                    switch (errLevel)
                    {
                        case CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendTo:
                            //Il n'existe aucune instruction de reception compatible avec le couple (Trade,Message)
                            //Remarque Spheres® considère ici tous les NCS compatibles avec les messages

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3350), 0,
                                new LogParam(tradeInfo.identifier),
                                new LogParam(cnfMessageToSend[i].identifier),
                                new LogParam(this[SendEnum.SendTo].sqlContactOffice.Identifier)));

                            break;
                        case CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy:

                            // FI 20200623 [XXXXX] SetErrorWarning
                            // FI 20240208 [WI842] Spheres® ne génère plus une erreur mais un simple Warning
                            _SetErrorWarning.Invoke(ProcessStateTools.StatusWarningEnum);

                            // FI 20240208 [WI842] ajoute un wwaring 
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 3351), 0,
                                new LogParam(tradeInfo.identifier),
                                new LogParam(cnfMessageToSend[i].identifier),
                                new LogParam(this[SendEnum.SendTo].sqlContactOffice.Identifier),
                                new LogParam(this[SendEnum.SendBy].sqlContactOffice.Identifier)));

                            ret = false; // ERROR FATAL , ils existent des instructions de reception que l'émetteur ne sait satisfaire
                            break;
                        case CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToClient:
                        case CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToEntity:
                        case CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToExternalCtr:
                            if (IsContactOfficesIdentical)
                            {
                                SysMsgCode code;
                                if (IsSendTo_Client(pCs))
                                {
                                    code = new SysMsgCode(SysCodeEnum.LOG, 3352);
                                }
                                else if (IsSendTo_Entity(pCs, cnfMessageToSend[i].NotificationClass))
                                {
                                    code = new SysMsgCode(SysCodeEnum.LOG, 3353);
                                }
                                else
                                {
                                    throw new Exception();
                                }

                                Logger.Log(new LoggerData(LogLevelEnum.Info, code, 0,
                                    new LogParam(tradeInfo.identifier),
                                    new LogParam(cnfMessageToSend[i].identifier),
                                    new LogParam(this[SendEnum.SendBy].sqlContactOffice.Identifier)));
                            }
                            else
                            {
                                SysMsgCode code;
                                if (IsSendTo_Client(pCs))
                                {
                                    //code = "LOG-03354";
                                    code = new SysMsgCode(SysCodeEnum.LOG, 3354);
                                }
                                else if (IsSendTo_Entity(pCs, cnfMessageToSend[i].NotificationClass))
                                {
                                    //code = "LOG-03355";
                                    code = new SysMsgCode(SysCodeEnum.LOG, 3355);
                                }
                                else
                                {
                                    //code = "LOG-03356";
                                    code = new SysMsgCode(SysCodeEnum.LOG, 3356);
                                }
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, code, 0,
                                    new LogParam(tradeInfo.identifier),
                                    new LogParam(cnfMessageToSend[i].identifier),
                                    new LogParam(this[SendEnum.SendTo].sqlContactOffice.Identifier),
                                    new LogParam(this[SendEnum.SendBy].sqlContactOffice.Identifier)));
                            }

                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("errLevel:{0} is  not implemented", errLevel.ToString()));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Init log method
        /// </summary>
        /// <param name="pSetErrorWarning">delegate reference</param>
        public void InitLogAddProcessLogInfoDelegate(SetErrorWarning pSetErrorWarning)
        {
            this._SetErrorWarning = pSetErrorWarning;
        }
        #endregion
    }

    /// <summary>
    /// Restriction des évènements candidats pour un message donné 
    /// <para>fonction des paramètres du process (PROCESSTUNING)</para>
    /// </summary>
    internal class CnfMessageEventRestriction : IRestrictionElement
    {
        #region Membres
        readonly ConfInstrGenProcess _process;
        readonly CnfMessage _cnfMessage;
        #endregion
        
        #region Constructor
        public CnfMessageEventRestriction(ConfInstrGenProcess pProcess, CnfMessage pCnfMessage)
        {
            _process = pProcess;
            _cnfMessage = pCnfMessage;
        }
        #endregion
        
        #region Membres de IRestrictionElement
        /// <summary>
        /// 
        /// </summary>
        public string Class
        {
            get { return Cst.OTCml_TBL.EVENT.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20150427 [20987] Modify
        public QueryParameters GetQueryRestrictionElement()
        {
            // FI 20150427 [20987]
            QueryParameters qryParameters = _process.GetQueryTriggerEvent(_cnfMessage);

            return qryParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdItem"></param>
        /// <returns></returns>
        public bool IsItemEnabled(int pIdItem)
        {
            bool ret = (Cst.ErrLevel.SUCCESS == _process.ScanCompatibility_Event(pIdItem));
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal class DatasetConfirmationMessageProcess : DatasetConfirmationMessage
    {
        #region Members
        /// <summary>
        /// Date de l'évènement à l'origine du message
        /// </summary>
        public DateTime dtEvent;
        /// <summary>
        /// 
        /// </summary>
        public int idB;
        #endregion Members

        #region constructor
        public DatasetConfirmationMessageProcess(int pIdB, DateTime pDtEvent, bool pIsModeSimul)
            : base(pIsModeSimul)
        {
            dtEvent = pDtEvent;
            idB = pIdB;
        }
        #endregion constructor
    }

    /// <summary>
    /// classe qui alimente et exporte le message d'un enregistrement MCO
    /// </summary>
    internal class DatasetConfirmationMessageManager
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly DatasetConfirmationMessage _dsMCO;

        /// <summary>
        /// process qui lance le mananager
        /// </summary>
        private readonly ProcessBase _process;

        /// <summary>
        /// 
        /// </summary>
        private McoInput _mcoInput;

        

        /// <summary>
        /// Représente le niveau d'écriture dans le log
        /// </summary>
        LogLevelDetail _levelStart = LogLevelDetail.LEVEL3;
        #endregion Members
        #region constructor
        public DatasetConfirmationMessageManager(DatasetConfirmationMessage pDsMCO, ProcessBase pProcess)
        {
            _dsMCO = pDsMCO;
            _process = pProcess;
        }
        #endregion constructor
        #region Method
        /// <summary>
        /// Génère le message et enregistre le résultat dans MCO 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMCO"></param>
        /// <param name="pIsRebuildMessage">
        /// <para>si true, le flux de messagerie est créé s'il n'existe pas ou écrasé s'il existe</para>
        /// <para>si false, le flux de messagerie est créé uniquement s'il n'existe pas</para>
        /// </param>
        /// <param name="pIsLoadPrevious"></param>
        /// FI 20150520 [XXXXX] Modify
        /// FI 20160624 [22286] Modify
        /// FI 20171120 [23580] Modify
        public void GenerateMessage(string pCS, IDbTransaction pDbTransaction, int pIdMCO, bool pIsRebuildMessage, Boolean pIsLoadPrevious)
        {
            DataRow rowIdMCO = _dsMCO.GetRowIdMco(pIdMCO);
            if (null == rowIdMCO)
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03462", new ProcessState(ProcessStateTools.StatusErrorEnum), pIdMCO.ToString());

            bool isToGenerate = true;
            if (false == pIsRebuildMessage)
                isToGenerate = (rowIdMCO["CNFMSGXML"] == Convert.DBNull);

            if (isToGenerate)
                isToGenerate = CheckProcessTuning(pIdMCO);

            if (isToGenerate)
            {
                CnfMessage cnfMessage = LoadCNFMessage(CSTools.SetCacheOn(pCS), rowIdMCO);

                int[] idT = _dsMCO.GetIdTInMCO(pCS, _process.Session.SessionId, pIdMCO);
                if (ArrFunc.IsEmpty(idT))
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03467", new ProcessState(ProcessStateTools.StatusErrorEnum), pIdMCO.ToString());
                else if (cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE && (ArrFunc.Count(idT) > 1))
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03468", new ProcessState(ProcessStateTools.StatusErrorEnum), pIdMCO.ToString());

                //Chargement du datadocument si message MONOTRADE
                DataDocumentContainer dataDocument = null;
                IProductBase productBase = Tools.GetNewProductBase();
                if (cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
                {
                    EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(pCS, null, idT[0]);
                    dataDocument = tradeLibrary.DataDocument;
                    productBase = (IProductBase)dataDocument.CurrentProduct.Product;
                }

                SetMcoInput(CSTools.SetCacheOn(pCS), cnfMessage, rowIdMCO, dataDocument);

                //Recherche des destinatiares des messages 
                InciItems inciItems = ConfirmationTools.GetInciItems(CSTools.SetCacheOn(pCS), _mcoInput.cnfMessage, dataDocument, _mcoInput.cnfChain, _mcoInput.idInci_SendBy, _mcoInput.idInci_SendTo);

                #region BuildMessage
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3370), 0,
                    new LogParam(LogTools.IdentifierAndId(_mcoInput.cnfMessage.identifier, _mcoInput.cnfMessage.idCnfMessage)),
                    new LogParam(LogTools.IdentifierAndId(_mcoInput.ncs.identifier, _mcoInput.ncs.idNcs)),
                    new LogParam(pIdMCO)));

                NotificationBuilder cnfMessageBuilder = null;
                try
                {
                    cnfMessageBuilder = new NotificationBuilder(_process.Session, _mcoInput.cnfMessage);

                    // FI 20150520 [XXXXX] call InitializeDelegate
                    cnfMessageBuilder.InitializeDelegate(LogTools.AddAttachedDoc, _process.ProcessState.SetErrorWarning);

                    // FI 20200520 [XXXXX] suppression du cache On sur l'appel à cette méthode
                    // Il est inutile de mettre toutes les requêtes en cache.
                    // La mise en cache est effectuée au cas par cas là où cela a un intérêt
                    cnfMessageBuilder.SetNotificationDocument(pCS, _mcoInput.dtMco, _mcoInput.dtMco2, pIdMCO, _mcoInput.ncs,
                        _mcoInput.idInci_SendBy, _mcoInput.cnfChain, inciItems, idT, productBase);

                    // FI 20160624 [22286] Chgt des précédentes versions
                    if (pIsLoadPrevious)
                        SetPrevious(pCS, cnfMessageBuilder.NotificationDocument, idT);

                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03469",
                        new ProcessState(ProcessStateTools.StatusErrorEnum), ex,
                        LogTools.IdentifierAndId(_mcoInput.cnfMessage.identifier, _mcoInput.cnfMessage.idCnfMessage),
                        pIdMCO.ToString(),
                        LogTools.IdentifierAndId(_mcoInput.ncs.identifier, _mcoInput.ncs.idNcs),
                        DtFunc.DateTimeToStringDateISO(_mcoInput.dtMco));
                }
                #endregion BuildMessage

                #region Serialisation
                try
                {
                    cnfMessageBuilder.SerializeDoc();
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03470",
                        new ProcessState(ProcessStateTools.StatusErrorEnum), ex,
                        LogTools.IdentifierAndId(_mcoInput.cnfMessage.identifier, _mcoInput.cnfMessage.idCnfMessage),
                        pIdMCO.ToString(),
                        LogTools.IdentifierAndId(_mcoInput.ncs.identifier, _mcoInput.ncs.idNcs),
                        DtFunc.DateTimeToStringDateISO(_mcoInput.dtMco));
                }
                #endregion serialize

                #region Transformation
                try
                {
                    if (cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
                        cnfMessageBuilder.TransForm(pCS, productBase, idT[0], _process.IdProcess);
                    else
                        cnfMessageBuilder.TransForm(pCS, null, null, _process.IdProcess);
                }
                catch (FileNotFoundException ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03471",
                        new ProcessState(ProcessStateTools.StatusErrorEnum), ex,
                        ex.FileName,
                        LogTools.IdentifierAndId(_mcoInput.cnfMessage.identifier, _mcoInput.cnfMessage.idCnfMessage),
                        pIdMCO.ToString(),
                        LogTools.IdentifierAndId(_mcoInput.ncs.identifier, _mcoInput.ncs.idNcs),
                        DtFunc.DateTimeToStringDateISO(_mcoInput.dtMco));

                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03472",
                        new ProcessState(ProcessStateTools.StatusErrorEnum), ex,
                        LogTools.IdentifierAndId(_mcoInput.cnfMessage.identifier, _mcoInput.cnfMessage.idCnfMessage),
                        pIdMCO.ToString(),
                        LogTools.IdentifierAndId(_mcoInput.ncs.identifier, _mcoInput.ncs.idNcs),
                        DtFunc.DateTimeToStringDateISO(_mcoInput.dtMco));
                }
                #endregion Transformation

                
                //AL 20240328 [WI440] New parameter logLevel
                _dsMCO.UpdRowMCO(pCS, pIdMCO, cnfMessageBuilder, _process.Session.IdA, _process.ProcessTuning.LogDetailEnum);
                _dsMCO.ExecuteDataAdapterMCO(pCS, pDbTransaction);
                _dsMCO.ExecuteDataAdapterMCODET(pCS, pDbTransaction);

                UpdateEventProcessAndEventStatus(pCS, pDbTransaction, pIdMCO);
            }
        }

        /// <summary>
        ///  Lance la tâche d'exportation
        ///  <para>Génération d'un message Queue vers le service IO</para>
        /// </summary>
        /// <returns></returns>
        public void ExportationMessage(string pCS, int pIdMCO)
        {
            DataRow rowIdMCO = _dsMCO.GetRowIdMco(pIdMCO);
            if (null == rowIdMCO)
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03462",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), pIdMCO.ToString());

            CnfMessage cnfMessage = LoadCNFMessage(pCS, rowIdMCO);
            NotificationConfirmationSystem ncs = LoadNCS(pCS, rowIdMCO);

            //Ecriture Log
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 3381), 0,
                new LogParam(LogTools.IdentifierAndId(cnfMessage.identifier, cnfMessage.idCnfMessage)),
                new LogParam(LogTools.IdentifierAndId(ncs.identifier, ncs.idNcs)),
                new LogParam(pIdMCO)));

            //20090220 FI [16512] Recherche de la tâche d'export associée au NCS, Exception si non trouvée 
            int idTask = ncs.idIoTask;
            SQL_IOTask sqltask = null;
            if (0 < idTask)
            {
                sqltask = new SQL_IOTask(pCS, idTask);
                sqltask.LoadTable(new string[] { "IDIOTASK", "IDENTIFIER" });
            }
            if ((null == sqltask) || (false == sqltask.IsLoaded))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03481",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), ncs.identifier);
            }


            #region Send Message
            int idMCo = Convert.ToInt32(rowIdMCO["ID"]);
            //Search paramètre IDMCO
            SQL_IOTaskParams taskParams = new SQL_IOTaskParams(pCS, idTask);
            DataRow[] drParams = taskParams.Select();
            
            MQueueparameters parameters = null;
            if (ArrFunc.IsFilled(drParams))
            {
                parameters = new MQueueparameters();
                foreach (DataRow drParam in drParams)
                {
                    // EG 20121012 AddParameter
                    MQueueparameter parameter = new MQueueparameter(drParam["IDIOPARAMDET"].ToString(),
                                                    drParam["DISPLAYNAME"].ToString(),
                                                    drParam["DISPLAYNAME"].ToString(),
                                                    (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum),
                                                    drParam["DATATYPE"].ToString(), true))
                    {
                        direction = drParam["DIRECTION"].ToString()
                    };

                    if (DataHelper.IsParamDirectionOutput(parameter.direction) || DataHelper.IsParamDirectionInputOutput(parameter.direction))
                    {
                        if (StrFunc.IsFilled(drParam["RETURNTYPE"].ToString()))
                            parameter.ReturnType = (Cst.ReturnSPParamTypeEnum)Enum.Parse(typeof(Cst.ReturnSPParamTypeEnum),
                                drParam["RETURNTYPE"].ToString(), true);
                    }
                    string inputValue;

                    #region Set parameter Value
                    if (drParam["DISPLAYNAME"].ToString() == "IDMCO")
                        inputValue = idMCo.ToString();
                    else
                        inputValue = Convert.ToString(drParam["DEFAULTVALUE"]);

                    try
                    {
                        switch (parameter.dataType)
                        {
                            case TypeData.TypeDataEnum.@bool:
                                parameter.SetValue(BoolFunc.IsTrue(inputValue));
                                break;
                            case TypeData.TypeDataEnum.date:
                            case TypeData.TypeDataEnum.datetime:
                            case TypeData.TypeDataEnum.time:
                                DateTime dtValue = new DtFunc().StringToDateTime(inputValue);
                                if (DtFunc.IsDateTimeFilled(dtValue))
                                    parameter.SetValue(dtValue);
                                break;
                            case TypeData.TypeDataEnum.integer:
                                parameter.SetValue(Convert.ToInt32(inputValue));
                                break;
                            case TypeData.TypeDataEnum.@decimal:
                                parameter.SetValue(Convert.ToDecimal(inputValue));
                                break;
                            default:
                                parameter.SetValue(inputValue);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // Fonctions qui seront interprétés dans les process
                        if (StrFunc.ContainsIn(inputValue.ToUpper(), "SPHERESLIB") || StrFunc.ContainsIn(inputValue.ToUpper(), "SQL"))
                            parameter.Value = inputValue;
                    }
                    #endregion Set parameter Value

                    parameters.Add(parameter);
                }
            }

            if ((null == parameters) || null == parameters["IDMCO"])
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03482",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), ncs.identifier);
            }

            IdInfo idInfo = new IdInfo()
            {
                id = idTask,
                idInfos = new DictionaryEntry[]{
                                                    new DictionaryEntry("ident", "IOTASK"),
                                                    new DictionaryEntry("identifier", sqltask.Identifier) }
            };

            //Post Mqeue vers IO
            MQueueSendInfo sendInfo = ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.IO, _process.AppInstance);
            if ((null == sendInfo) || (!sendInfo.IsInfoValid))
                throw new Exception("Probably " + Cst.Process.GetService(Cst.ProcessTypeEnum.IO) + " service is not yet installed");

            _process.Tracker.AddPostedSubMsg(1, _process.Session);
            MQueueTools.Send(Cst.ProcessTypeEnum.IO, pCS, idInfo, parameters, _process.MQueue.header.requester, sendInfo);

            #endregion
        }

        /// <summary>
        /// Init log method
        /// </summary>
        /// <param name="pLevelStart"></param>
        public void InitLogAddProcessLogInfoDelegate(LogLevelDetail pLevelStart)
        {
            _levelStart = pLevelStart;
        }

        /// <summary>
        /// Alimente _mcoInput à partir d'un enregistrement MCO
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRowMCO"></param>
        /// <param name="pDataDoc"></param>
        /// FI 20120829 [18048] alimentation de _mcoInput.dtMco2 
        /// EG 20150706 [21021] Nullable int for idB
        /// FI 20160624 [22286] Modify
        private void SetMcoInput(string pCS, CnfMessage pCnfMsg, DataRow pRowMCO, DataDocumentContainer pDataDoc)
        {
            _mcoInput = new McoInput
            {
                dtMco = Convert.ToDateTime(pRowMCO["DTMCO"]),
                cnfMessage = pCnfMsg,
                ncs = LoadNCS(pCS, pRowMCO),
                cnfChain = new ConfirmationChain()
            };
            if (false == Convert.IsDBNull(pRowMCO["DTMCO2"]))
                _mcoInput.dtMco2 = Convert.ToDateTime(pRowMCO["DTMCO2"]);
            _mcoInput.cnfChain[SendEnum.SendBy].LoadActor(pCS, Convert.ToInt32(pRowMCO["IDA_SENDBYPARTY"]));
            _mcoInput.cnfChain[SendEnum.SendBy].LoadContactOffice(pCS, Convert.ToInt32(pRowMCO["IDA_SENDBYOFFICE"]));

            // EG 20150706 [21021]
            Nullable<int> idB = null;
            if (false == Convert.IsDBNull(pRowMCO["IDB_SENDBYPARTY"]))
                idB = Convert.ToInt32(pRowMCO["IDB_SENDBYPARTY"]);
            _mcoInput.cnfChain[SendEnum.SendBy].LoadBook(pCS, idB);

            _mcoInput.cnfChain[SendEnum.SendTo].LoadActor(pCS, Convert.ToInt32(pRowMCO["IDA_SENDTOPARTY"]));
            _mcoInput.cnfChain[SendEnum.SendTo].LoadContactOffice(pCS, Convert.ToInt32(pRowMCO["IDA_SENDTOOFFICE"]));

            // EG 20150706 [21021]
            idB = null;
            if (false == Convert.IsDBNull(pRowMCO["IDB_SENDTOPARTY"]))
                idB = Convert.ToInt32(pRowMCO["IDB_SENDTOPARTY"]);
            _mcoInput.cnfChain[SendEnum.SendTo].LoadBook(pCS, idB);

            
            _mcoInput.idInci_SendBy = Convert.ToInt32(pRowMCO["IDINCI_SENDBY"]);
            _mcoInput.idInci_SendTo = 0;
            if (Convert.DBNull != pRowMCO["IDINCI_SENDTO"])
                _mcoInput.idInci_SendTo = Convert.ToInt32(pRowMCO["IDINCI_SENDTO"]);
            //
            //GLOP FI EVENTTRIGER ce code est à supprimer
            //Il reste là tant que le script de reprise n'est pas branché
            if (_mcoInput.cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
            {
                if (_mcoInput.cnfChain[SendEnum.SendBy].IdBook == 0)
                {
                    IParty party = pDataDoc.GetParty(_mcoInput.cnfChain[SendEnum.SendBy].IdActor.ToString(), PartyInfoEnum.OTCmlId);
                    if (null != party)
                    {
                        // EG 20150706 [21021]
                        _mcoInput.cnfChain[SendEnum.SendBy].LoadBook(pCS, pDataDoc.GetOTCmlId_Book(party.Id));
                    }
                }
                
                if (_mcoInput.cnfChain[SendEnum.SendTo].IdBook == 0)
                {
                    IParty party = pDataDoc.GetParty(_mcoInput.cnfChain[SendEnum.SendTo].IdActor.ToString(), PartyInfoEnum.OTCmlId);
                    if (null != party)
                    {
                        // EG 20150706 [21021]
                        _mcoInput.cnfChain[SendEnum.SendTo].LoadBook(pCS, pDataDoc.GetOTCmlId_Book(party.Id));
                    }
                }
            }
            //    
            //FI 20120528 L'alimentation de la classe confirmationChain était incomplète
            //Il faut valoriser les properties isSendByActor_Entity et isSendTo_Broker
            if (_mcoInput.cnfMessage.NotificationClass == NotificationClassEnum.MULTITRADES
                ||
                _mcoInput.cnfMessage.NotificationClass == NotificationClassEnum.MULTIPARTIES)
            {
                //Pour la messagerie de type Edition (simple ou consolidée)
                //La messagerie est tjs envoyé par l'entité à destination soit d'un dealer maison soit d'un client
                _mcoInput.cnfChain.IsSendByActor_Entity = true;
                _mcoInput.cnfChain.IsSendTo_Broker = false;
            }
            else if (_mcoInput.cnfMessage.NotificationClass == NotificationClassEnum.MONOTRADE)
            {
                _mcoInput.cnfChain.IsSendTo_Broker = false;
                if (pDataDoc.IsExternalCounterpartyInContextNamesGiveUp(pCS, _mcoInput.cnfChain[SendEnum.SendTo].IdActor))
                    _mcoInput.cnfChain.IsSendTo_Broker = pDataDoc.IsPartyBroker(_mcoInput.cnfChain[SendEnum.SendTo].IdAContactOffice);

                _mcoInput.cnfChain.IsSendByActor_Entity = false;
                if (null != _mcoInput.cnfChain[SendEnum.SendTo].sqlBook)
                    _mcoInput.cnfChain.IsSendByActor_Entity = BookTools.IsBookClient(pCS, _mcoInput.cnfChain[SendEnum.SendTo].IdActor, 
                        _mcoInput.cnfChain[SendEnum.SendTo].IdBook, _mcoInput.cnfChain[SendEnum.SendBy].IdActor);
            }

            // FI 20160624 [22286] add scope
            _mcoInput.scope = null;
            if (Convert.DBNull != pRowMCO["SCOPE"])
                _mcoInput.scope = Convert.ToString(pRowMCO["SCOPE"]);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRowMCO"></param>
        /// <exception cref="SpheresException2 si message non chargé"></exception> 
        private static CnfMessage LoadCNFMessage(string pCS, DataRow pRowMCO)
        {
            int idCnfMessage = Convert.ToInt32(pRowMCO["IDCNFMESSAGE"]);
            CnfMessage ret = ConfirmationTools.LoadCnfMessage(pCS, SQL_TableWithID.IDType.Id, idCnfMessage.ToString(), SQL_Table.ScanDataDtEnabledEnum.No);
            if (null == ret)
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03464",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), idCnfMessage.ToString());
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRowMCO"></param>
        /// <returns></returns>
        private static NotificationConfirmationSystem LoadNCS(string pCS, DataRow pRowMCO)
        {
            int idNcs = Convert.ToInt32(pRowMCO["IDA_NCS"]);
            NotificationConfirmationSystem ret = ConfirmationTools.LoadNotificationConfirmationSystem(CSTools.SetCacheOn(pCS), SQL_TableWithID.IDType.Id, idNcs.ToString());
            if (null == ret)
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03465",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), idNcs.ToString());
            return ret;
        }

        

        /// <summary>
        ///  Chargement des versions précédentes 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMcoInpt"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCulture"></param>
        /// <param name="pScope"></param>
        /// <param name="pDefaultOffset"></param>
        /// FI 20160624 [22286] Add 
        /// FI 20171120 [23580] Modify (variables de retour List<PreviousDateTimeUTC> )
        // EG 20180425 Analyse du code Correction [CA2202]
        private List<PreviousDateTimeUTC> LoadPrevious(string pCS, McoInput pMcoInpt, int[] pIdT, string pCulture, string pScope, TimeSpan pDefaultOffset)
        {

            List<PreviousDateTimeUTC> ret = new List<PreviousDateTimeUTC>();
            _ = pDefaultOffset;
            string restrictDtMco2 = pMcoInpt.dtMco2.HasValue ? "DTMCO2=@DTMCO2" : "DTMCO2 is null";
            string restrictIdT = ArrFunc.Count(pIdT) == 1 ? "IDT=@IDT" : "IDT is null";
            string restrictSendByIdb = (pMcoInpt.cnfChain[SendEnum.SendBy].IdBook > 0) ? "IDB_SENDBYPARTY=@IDB_SENDBYPARTY" : "IDB_SENDBYPARTY is null";

            string restrictSendToIdInci = (pMcoInpt.idInci_SendTo > 0) ? "IDINCI_SENDTO=@IDINCI_SENDTO" : "IDINCI_SENDTO is null";
            string restrictSendToIdb = (pMcoInpt.cnfChain[SendEnum.SendTo].IdBook > 0) ? "IDB_SENDTOPARTY=@IDB_SENDTOPARTY" : "IDB_SENDTOPARTY is null";
            // RD 20160831 [21961][22286] Correction
            string restrictScope = StrFunc.IsFilled(pScope) ? "SCOPE=@SCOPE" : "SCOPE is null";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "DTMCO", DbType.Date), pMcoInpt.dtMco);
            if (restrictDtMco2.Contains("@DTMCO2"))
                dp.Add(new DataParameter(pCS, "DTMCO2", DbType.Date), pMcoInpt.dtMco2.Value);

            if (restrictIdT.Contains("@IDT"))
                dp.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT[0]);
            dp.Add(new DataParameter(pCS, "IDCNFMESSAGE", DbType.Int32), pMcoInpt.cnfMessage.idCnfMessage);

            dp.Add(new DataParameter(pCS, "IDA_NCS", DbType.Int32), pMcoInpt.ncs.idNcs);
            //SENDBY
            dp.Add(new DataParameter(pCS, "IDINCI_SENDBY", DbType.Int32), pMcoInpt.idInci_SendBy);
            dp.Add(new DataParameter(pCS, "IDA_SENDBYOFFICE", DbType.Int32), pMcoInpt.cnfChain[SendEnum.SendBy].IdAContactOffice);
            dp.Add(new DataParameter(pCS, "IDA_SENDBYPARTY", DbType.Int32), pMcoInpt.cnfChain[SendEnum.SendBy].IdActor);
            if (restrictSendByIdb.Contains("@IDB_SENDBYPARTY"))
                dp.Add(new DataParameter(pCS, "IDB_SENDBYPARTY", DbType.Int32), pMcoInpt.cnfChain[SendEnum.SendBy].IdBook);
            //SENDTO
            if (restrictSendToIdInci.Contains("@IDINCI_SENDTO"))
                dp.Add(new DataParameter(pCS, "IDINCI_SENDTO", DbType.Int32), pMcoInpt.idInci_SendTo);
            dp.Add(new DataParameter(pCS, "IDA_SENDTOOFFICE", DbType.Int32), pMcoInpt.cnfChain[SendEnum.SendTo].IdAContactOffice);
            dp.Add(new DataParameter(pCS, "IDA_SENDTOPARTY", DbType.Int32), pMcoInpt.cnfChain[SendEnum.SendTo].IdActor);
            if (restrictSendToIdb.Contains("@IDB_SENDTOPARTY"))
                dp.Add(new DataParameter(pCS, "IDB_SENDTOPARTY", DbType.Int32), pMcoInpt.cnfChain[SendEnum.SendTo].IdBook);

            dp.Add(new DataParameter(pCS, "CULTURE", DbType.AnsiString), pCulture);
            if (restrictScope.Contains("@SCOPE"))
                dp.Add(new DataParameter(pCS, "SCOPE", DbType.AnsiString), pScope);
            /* 
             pas de script de reprise sur les messages sur les messages existants 
             seuls les messages tel que DTCREATE is not null sont considérés
             */
            string sql = StrFunc.AppendFormat(@"
select DTCREATE, TZ_SENDBY,  case when DTOBSOLETE is null then 0 else 1 end as OBSOLETE
from dbo.MCO
where DTMCO=@DTMCO and {0}
  and IDA_NCS=@IDA_NCS and {1}
  and IDCNFMESSAGE=@IDCNFMESSAGE 
  and IDINCI_SENDBY=@IDINCI_SENDBY and IDA_SENDBYOFFICE=@IDA_SENDBYOFFICE and IDA_SENDBYPARTY=@IDA_SENDBYPARTY and {2}
  and {3} and IDA_SENDTOOFFICE=@IDA_SENDTOOFFICE and IDA_SENDTOPARTY=@IDA_SENDTOPARTY and {4}
  and CULTURE=@CULTURE 
  and {5} 
  and DTCREATE is not null
order by DTCREATE desc
", restrictDtMco2, restrictIdT, restrictSendByIdb, restrictSendToIdInci, restrictSendToIdb, restrictScope);

            QueryParameters qry = new QueryParameters(pCS, sql, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    // FI 20171120 [23580] Alimentation d'une classe PreviousDateTimeUTC
                    PreviousDateTimeUTC item = new PreviousDateTimeUTC()
                    {
                        tzdbIdSpecified = (dr["TZ_SENDBY"] != Convert.DBNull),
                        tzdbId = (dr["TZ_SENDBY"] == Convert.DBNull) ? Tz.Tools.UniversalTimeZone : Convert.ToString(dr["TZ_SENDBY"]),

                        DateTimeValueUTC = Convert.ToDateTime(dr["DTCREATE"]),
                        DateTimeValue = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(dr["DTCREATE"]),
                                            Tz.Tools.GetTimeZoneInfoFromTzdbId((dr["TZ_SENDBY"] == Convert.DBNull) ? Tz.Tools.UniversalTimeZone : Convert.ToString(dr["TZ_SENDBY"]))),
                        obsolete = Convert.ToBoolean(dr["OBSOLETE"])
                    };

                    ret.Add(item);
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne True, si ProcessTuning autorise la génération pour l'ensemble des événements déclencheurs (MCODET) 
        /// </summary>
        /// <param name="pIdMCO"></param>
        /// FI 20160624 [22286] Add Method
        private Boolean CheckProcessTuning(int pIdMCO)
        {
            Boolean isToGenerate = true;

            int[] idE = _dsMCO.GetIdEventsInMCODET(pIdMCO);
            for (int i = 0; i < ArrFunc.Count(idE); i++)
            {
                if (Cst.ErrLevel.SUCCESS != _process.ScanCompatibility_Event(idE[i]))
                {
                    isToGenerate = false;

                    Pair<int, Pair<string, string>> evt = _process.GetInfoEvent(idE[i]);
                    if (null != evt)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.LOG, 3010), 0,
                            new LogParam(LogTools.IdentifierAndId(_process.MQueue.Identifier, _process.CurrentId)),
                            new LogParam(evt.First),
                            new LogParam(evt.Second.First),
                            new LogParam(evt.Second.Second)));
                    }
                    break;
                }
            }

            return isToGenerate;
        }

        /// <summary>
        ///  ECriture dans EVENTPROCESS et EVENTSTATUS pour tous les évènements déclencheurs en fonction de PROCESSTUNING
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMCO"></param>
        private void UpdateEventProcessAndEventStatus(string pCS, IDbTransaction pDbTransaction, int pIdMCO)
        {
            // Obtenir la liste des Events qui sont impliqués dans le MCO
            int[] idEvents = _dsMCO.GetIdEventsInMCODET(pIdMCO);
            
            EventProcess eventProcess = new EventProcess(pCS);
            DataSetEventTrade dsEvent = new DataSetEventTrade(pCS, pDbTransaction, idEvents);
            
            for (int i = 0; i < ArrFunc.Count(idEvents); i++)
            {
                //
                // Pour chaque Event, ecrire une ligne dans EventProcess, pour dire que l'Event a subit un Traitement avec Success
                // FI 20200820 [25468] dates systemes en UTC
                eventProcess.Write(pDbTransaction, idEvents[i], _process.MQueue.ProcessType, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(pCS), _process.Tracker.IdTRK_L, pIdMCO); ;
                //
                // Pour chaque Event, Appliquer les statuts du Traitement avec Success : IDSTACTIVATION, StCheck et StMatch
                if (_process.ProcessTuningSpecified) //ProcessTuning peut être null "avis d'opérés"
                {
                    // FI 20200820 [25468] dates systemes en UTC
                    dsEvent.SetEventStatus(idEvents[i], _process.ProcessTuning.GetProcessTuningOutput(Tuning.TuningOutputTypeEnum.OES), _process.Session.IdA, OTCmlHelper.GetDateSysUTC(pCS));
                }
            }
            
            dsEvent.Update(pDbTransaction);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="cnfMessageBuilder"></param>
        /// <param name="pIdT"></param>
        private void SetPrevious(string pCS, NotificationDocumentContainer notificationDocument, int[] pIdT)
        {

            INotificationHeader header = notificationDocument.NotificationDocument.Header;
            DateTimeOffset dtCreation = DtFunc.ConvertToDateTimeOffset(header.CreationTimestamp.DateTimeValue, header.CreationTimestamp.offset);

            // FI 20171120 [23580] LoadPrevious retourne une liste de PreviousDateTimeUTC 
            List<PreviousDateTimeUTC> previous = LoadPrevious(pCS, _mcoInput, pIdT, notificationDocument.Culture, _mcoInput.scope, dtCreation.Offset);
            if (previous.Count > 0)
            {
                header.PreviousCreationTimestampSpecified = true;
                header.PreviousCreationTimestamp = previous.ToArray();

                // FI 20160624 [22286] Mise à jour de settings.headerFooter.hCancelMsg avec les données présentes sous  header.previousCreationTimestamp[0] (le plus récent)
                // FI 20171120 [23580] Add PREVIOUS_TZ
                if (notificationDocument.NotificationDocument.Header.ReportSettingsSpecified)
                {
                    ReportSettings settings = notificationDocument.NotificationDocument.Header.ReportSettings;
                    if (settings.headerFooterSpecified && settings.headerFooter.hCancelMsgSpecified)
                    {
                        CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(notificationDocument.Culture);
                        MsgStyle cancelMsg = settings.headerFooter.hCancelMsg;
                        cancelMsg.msg = StrFuncExtended.ReplaceObjectField(cancelMsg.msg, new
                        {
                            TITLE = settings.headerFooter.hTitle,
                            PREVIOUS_DATETIME = DtFunc.DateTimeToString(header.PreviousCreationTimestamp[0].DateValue, DtFunc.FmtDateTime),
                            PREVIOUS_DATE = DtFunc.DateTimeToString(header.PreviousCreationTimestamp[0].DateValue, DtFunc.FmtShortDate),
                            PREVIOUS_TIME = DtFunc.DateTimeToString(header.PreviousCreationTimestamp[0].DateValue, DtFunc.FmtShortTime),
                            PREVIOUS_TZ = header.PreviousCreationTimestamp[0].tzdbId
                        }, cultureInfo);
                    }
                }
            }

        }

        #endregion Method
    }

    /// <summary>
    /// Représente un enregistrement MCO
    /// </summary>
    internal class McoInput
    {
        #region members
        /// <summary>
        /// Représente la date du Message
        /// <para>Représente la date "from" sur un message "extrait de compte"</para>
        /// </summary>
        internal DateTime dtMco;

        /// <summary>
        /// Représente la date "to" sur un message "extrait de compte"
        /// </summary>
        internal Nullable<DateTime> dtMco2;

        /// <summary>
        /// Représente le message
        /// </summary>
        internal CnfMessage cnfMessage;
        /// <summary>
        /// Représente la chaîne de confirmation
        /// </summary>
        internal ConfirmationChain cnfChain;
        /// <summary>
        /// Représente le système de messagerie 
        /// </summary>
        internal NotificationConfirmationSystem ncs;
        /// <summary>
        /// Représente l'instruction Emetteur
        /// </summary>
        internal int idInci_SendBy;
        /// <summary>
        /// Représente l'instruction Destinataire
        /// </summary>
        internal int idInci_SendTo;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        internal string scope;

        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        internal McoInput()
        {
            cnfMessage = new CnfMessage();
            cnfChain = new ConfirmationChain();
            ncs = new NotificationConfirmationSystem();
        }
        #endregion
    }

    /// <summary>
    ///  Représente les informations issues d'un trade nécessaires à la messagerie
    /// </summary>
    /// FI 20140808 [20275] Modify
    /// FI 20170913 [23417] Modify
    internal struct TradeInfo
    {
        #region Members
        /// <summary>
        /// IdT
        /// </summary>
        internal int idT;

        /// <summary>
        /// identifier du trade
        /// </summary>
        internal string identifier;

        /// <summary>
        /// Instrument
        /// </summary>
        internal int idI;

        /// <summary>
        ///  Marché
        ///  <para>0 si trade de type cash Balance</para>
        /// </summary>
        /// FI 20140808 [20275] add
        internal int idM;

     
        /// <summary>
        ///  contractId 
        ///  <para>null si trade sans contract (ni LSD, ni commodity)</para>
        /// </summary>
        internal Pair<Cst.ContractCategory, int> contractId;


        /// <summary>
        /// Buyer / Seller
        /// </summary>
        // EG 20220523 [WI639] Réduction des parties (Buyer/Seller) sur les instruments de facturation
        internal int idA_Buyer;
        internal int idA_Seller;


        /// <summary>
        ///  Devise du trade
        /// </summary>
        internal string idC;

        /// <summary>
        /// Statut business
        /// </summary>
        internal Cst.StatusBusiness statusBusiness;

        /// PL 20140710 [20179]
        /// <summary>
        /// Statut User - Match
        /// </summary>
        internal string statusMatch;

        /// <summary>
        /// Statut User - Check
        /// </summary>
        internal string statusCheck;

        /// <summary>
        /// 
        /// </summary>
        internal int idIUnderlyer;

        /// <summary>
        /// Représente les évènements déclencheurs
        /// </summary>
        internal List<Int32> idE;
        #endregion
    }
}

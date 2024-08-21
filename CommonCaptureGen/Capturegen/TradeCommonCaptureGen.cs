#region Using Directives
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.CCI;
using EFS.Permission;
using EFS.Tuning;
using EFS.Status;
//
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// 
    /// </summary>
    public abstract partial class TradeCommonCaptureGen : CommonCaptureGen, ICustomCaptureInfos
    {
        #region Membre
        // EG 20190308 New (Utilisation en multithreading sur Delete EVENTS pour eviter DeadLock)
        private static readonly object m_RecordLock = new object();
        /// <summary>
        /// Message Détail en cas de succès lors de l'enregistrement d'un trade
        /// </summary>
        private string _msgDet;
        #endregion

        #region Enums
        /// <summary>
        /// Liste des erreurs dans CheckAndRecord
        /// </summary>
        /// FI 202120301 [18465] add IDENTIFIER_DUPLICATE
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public enum ErrorLevel
        {
            SUCCESS = -1,
            UNDEFINED = 0,
            //
            LICENSEE_ERROR = 5,
            ENVIRONNEMENT_UNDEFINED = 10,
            STCHECK_ERROR = 15,
            STMATCH_ERROR = 16,
            STATUS_ERROR = 17,
            //
            IDENTIFIER_ERROR = 20,
            IDENTIFIER_NOTFOUND = 21,
            IDENTIFIER_DUPLICATE = 22,
            //
            SPECIFICCHECK_ERROR = 25,
            SECURITYSUSE_ERROR = 26,
            SECURITYSUSE_ERROR2 = 27,
            SECURITYIDENTIFIER_DUPLICATE = 28,
            //
            LOCK_ERROR = 30,
            LOCKPROCESS_ERROR = 32,
            //
            ROWVERSION_ERROR = 35,
            //
            UPD_DOCREADER_ERROR = 40,
            SERIALIZE_ERROR = 50,
            BEGINTRANSACTION_ERROR = 60,
            DELETE_ERROR = 65,
            //
            EDIT_TRADE_ERROR = 70,
            EDIT_TRADESTSYS_ERROR = 71,
            EDIT_TRADESTCHECK_ERROR = 72,
            EDIT_TRADESTMATCH_ERROR = 73,
            EDIT_EVENT_ERROR = 74,
            EDIT_TRADESTATUS_ERROR = 75,
            //
            LOAD_LINKTABLE_ERROR = 80,
            LOAD_TRADEINSTRUMENTorSTREAM_ERROR = 81,
            LOAD_TRADELINK_ERROR = 82,
            LOAD_TRADEASSET_ERROR = 83,
            //
            ADD_ENTITYMARKET = 84,
            ADD_ATTACHEDDOC_ERROR = 85,
            ADD_NOTEPAD_ERROR = 86,
            //
            ADD_TRADETRAIL = 87,
            ADD_POSUTI = 88,
            //
            COMMIT_ERROR = 90,
            AFTER_RECORD_ERROR = 91, //20091016 FI [Rebuild identification] add AFTER_RECORD_ERROR
            //
            FAILURE = 100,
            XMLDOCUMENT_NOTCONFORM = 200,
            VALIDATIONRULE_ERROR = 300,
            XSLDOCUMENT_ERROR = 400,
            //
            REMOVEINVOICESIMUL_ERROR = 500,
            GET_TRADEIDENTIFIER_ERROR = 510,
            INVOICETRADE_NOTFOUND = 520,
            MULTIINVOICETRADE_FOUND = 530,
            UPD_TRADEIDENTIFIER_ERROR = 540,
            //
            SAVEUNDERLYING_ERROR = 600,
            CHECKUNDERLYING_ERROR = 650,
            //
            SETDEFAULTVALUE = 700,
        }

        #endregion Enums

        #region Accessors

        /// <summary>
        /// Obtient ou définit le message détail Obtenu en cas de succès lors de l'enregistrement d'un trade
        /// </summary>
        protected string MsgDet
        {
            get { return _msgDet; }
            set { _msgDet = value; }
        }

        /// <summary>
        /// Retourne le type de trade (TRADEADMIN, TRADEDEBTSEC, TRADERISK, TRADE)
        /// </summary>
        public virtual string DataIdent
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        ///  Représente le trade
        /// </summary>
        public virtual TradeCommonInput TradeCommonInput
        {
            get
            {
                return null;
                // FI 20130204 il faudrait une exception, juste avant sortie de release je laisse en l'état
                //throw new Exception("Propertie TradeCommonInput must be override");
            }
            set
            {
                throw new Exception("Propertie TradeCommonInput must be override");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        CustomCaptureInfosBase ICustomCaptureInfos.CcisBase
        {
            get { return (CustomCaptureInfosBase)TradeCommonInput.CustomCaptureInfos; }
        }

        /// <summary>
        ///  Obtient les directives utilisées par CheckAndRecord
        /// </summary>
        protected TradeRecordSettings RecordSettings
        {
            get;
            private set;
        }



        #endregion Accessors

        #region Constructor
        public TradeCommonCaptureGen() : base() { }
        #endregion Constructors

        #region Methods
        // EG 20180514 [23812] Report
        public virtual bool RecordFxOptionEarlyTermination(string pCS, string pIdScreen)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMenu">Menu utilise pour contôler les permissions</param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pRecordSettings"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdT"></param>
        /// <param name="oUnderlying">Liste des assets créés pendant l'enregistrement du trade (null si aucun asset créé)</param>
        /// <param name="oTrader">Liste des traders créés pendant l'enregistrement du trade (null si aucun trader créé)</param>
        /// <exception cref="TradeCommonCaptureGenException"></exception>
        /// FI 20131213 [19337] Mise en place d'une transaction indépendante pour la création des assets ss-jacent
        /// FI 20140206 [19564] Refactoring et Mise à jour des UIs lorsque non renseignés
        /// FI 20170404 [23039] Modification de sinature ( oTrader, oUnderlying) 
        /// EG 20171115 [23509] Upd ProcessCheckValidationRule
        /// EG 20180205 [23769] Use pDbTransaction  
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20200519 [XXXXX] Refactoring for performance
        /// EG 20201006 [XXXXX] Déplacement de CheckConformity après ProcessUpdPartyTradeIdentifier et ProcessUpdPartyTradeInformation
        /// EG 20211014 [XXXXX] Les IDs IDTRADEXML_P et IDTRADE_P n'étaient pas mis à jour dans TRADETRAIL lors de la modification d'un trade. (Ordre des MAJ de tables)
        /// FI 20231227 [WI791] Suppression de l'alimentation d'un ProcessLog (jamais utilisé)
        /// FI 20231227 [WI791] Le pIdTRK_L est également supprimé, Cette information se trouve sous pSessionInfo
        public virtual void CheckAndRecord(string pCS, IDbTransaction pDbTransaction,
                                         string pIdMenu,
                                         Cst.Capture.ModeEnum pCaptureMode,
                                         CaptureSessionInfo pSessionInfo,
                                         TradeRecordSettings pRecordSettings,
                                         ref string pIdentifier, ref int pIdT,
                                         out Pair<int, string>[] oUnderlying,
                                         out Pair<int, string>[] oTrader)
        {
            this.RecordSettings = pRecordSettings;

            ErrorLevel errLevel = ErrorLevel.SUCCESS;

            MsgDet = string.Empty;
            int idT = 0;
            // FI 20170404 [23039] Utilisation de underlying et trader
            Pair<int, string>[] underlying = null;
            Pair<int, string>[] trader = null;

            //Date système pour alimentation des colonnes DTUPD et DTINS ou equivalentes
            // FI 20200820 [25468] Dates systèmes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);

            Boolean isAutoTransaction = (null == pDbTransaction); // Retourne true lorsque CheckAndRecord génère sa propre transaction
            IDbTransaction dbTransaction = pDbTransaction;

            SpheresIdentification tradeIdentification = new SpheresIdentification(pIdentifier, RecordSettings.displayName, RecordSettings.description, RecordSettings.extLink);

            TradeCommonCaptureGenException errExc = null;
            LockObject lockTrade = null;
            //LockObject lockOrder = null;
            try
            {

                ProcessCheckLicensee(pSessionInfo);

                lockTrade = ProcessLockTrade(pCS, dbTransaction, Cst.FeeScopeEnum.Trade, pCaptureMode, pSessionInfo, tradeIdentification);

                ProcessCheckValidationLock(pCS, dbTransaction, pCaptureMode, pSessionInfo.session);

                ProcessCheckRowVersion(pCS, dbTransaction, pCaptureMode);

                ProcessCheckActionTuning(pCS, dbTransaction, pCaptureMode, pSessionInfo, pIdMenu);

                ProcessCheckSpecific(pCS, dbTransaction, pCaptureMode);

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20191210 [25099] In progress... GLOP25099 TO REMOVE
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //if (this.RecordSettings.isExistsFeeScope_OrderId)
                //{
                //    SpheresIdentification orderIdentification = new SpheresIdentification(this.RecordSettings.OrderId, this.RecordSettings.OrderId, "Order relative to " + tradeIdentification.identifier, null);
                //    lockOrder = ProcessLockTrade(pCS, dbTransaction, Cst.FeeScopeEnum.OrderId, pCaptureMode, pSessionInfo, orderIdentification, dtSys);
                //}
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                ProcessSetDefault(pCS, dbTransaction, pCaptureMode);

                // EG 20200519 [XXXXX] Refactoring for performance
                // Execution de ces méthodes hors transaction
                ProcessSaveUnderlyingAsset(pCS, dbTransaction, pCaptureMode, pSessionInfo, dtSys, out underlying);
                ProcessCheckUnderlyingAsset(pCS, dbTransaction, pCaptureMode);

                // FI 20210308 [XXXXX] Appel à ProcessUpdPartyTradeInformation avant ProcessCheckValidationRule puisque ProcessUpdPartyTradeInformation peut ajouter un acteur (Trader) en base de données
                ProcessUpdPartyTradeInformation(pCS, dbTransaction, pCaptureMode, pSessionInfo, tradeIdentification, dtSys, out trader);

                ProcessCheckValidationRule(pCS, dbTransaction, pCaptureMode, pSessionInfo);

                if (isAutoTransaction)
                {
                    //Ouverture d'une d'une transaction SQL, s'il en existe pas 
                    dbTransaction = ProcessStartDbTransaction(pCS);
                }

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20191210 [25099] In progress... GLOP25099 TO REMOVE
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //if (this.RecordSettings.isExistsFeeScope_OrderId)
                //{
                //    ProcessFee_OrderId(pCS, dbTransaction, pCaptureMode, pSessionInfo); 
                //}
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                tradeIdentification = ProcessCalcTradeIdentification(pCS, dbTransaction, pCaptureMode, pSessionInfo, tradeIdentification);



                //ProcessCheckConformity(pCaptureMode, pSessionInfo);


                idT = ProcessSaveTrade(pCS, dbTransaction, pCaptureMode, tradeIdentification, pSessionInfo, dtSys);

                // FI 20140217 [19618][19640] Sauvegarde de l'IdT dans tradeIdentification
                tradeIdentification.OTCmlId = idT;

                ProcessDelete(pCS, dbTransaction, pCaptureMode, idT);

                // EG 20200519 [XXXXX] Refactoring for performance
                switch (pRecordSettings.recordMode)
                {
                    case 0: // Toutes les méthodes sont exécutées dans la transaction

                        ProcessUpdPartyTradeIdentifier(pCS, dbTransaction, pCaptureMode, tradeIdentification);
                        // FI 20210308 [XXXXX] Appel effectué plus haut
                        //ProcessUpdPartyTradeInformation(pCS, dbTransaction, pCaptureMode, pSessionInfo, tradeIdentification, dtSys, out trader);

                        ProcessCheckConformity(pCaptureMode, pSessionInfo);

                        ProcessTradeLink(pCS, dbTransaction, pCaptureMode, tradeIdentification.Identifier);
                        ProcessUpdUTIPartyTradeIdentifier(pCS, dbTransaction, pCaptureMode, tradeIdentification);

                        ProcessSaveTradeOtherStatus(pCS, dbTransaction, pCaptureMode, idT, pSessionInfo, dtSys);
                        // EG 20211014 Sauvegarde TRADEXML avant TRADETRAIL (pour mise à jour des IDTRADE_P et IDTRADEXML_P dans TRADETRAIL)
                        ProcessSaveTradeXML(pCS, dbTransaction, pCaptureMode, idT);
                        ProcessSaveTradeTrail(pCS, dbTransaction, pCaptureMode, idT, pSessionInfo, dtSys);
                        ProcessSaveTradeActorTradeIdLinkId(pCS, dbTransaction, pCaptureMode, idT);
                        ProcessSaveTradeAsset(pCS, dbTransaction, pCaptureMode, idT);
                        ProcessSaveInstrumentAndStream(pCS, dbTransaction, pCaptureMode, idT);
                        ProcesSaveTradeLink(pCS, dbTransaction, pCaptureMode, idT, tradeIdentification.Identifier);
                        ProcessSavePositionUTI(pCS, dbTransaction, pCaptureMode, tradeIdentification, pSessionInfo, dtSys, isAutoTransaction);
                        ProcessDeleteEvent(pCS, dbTransaction, pCaptureMode, idT, pSessionInfo, dtSys);
                        ProcessCopyAttachedDoc(pCS, dbTransaction, pCaptureMode, idT);
                        ProcessCopyNotepad(pCS, dbTransaction, pCaptureMode, idT);

                        ProcessSaveENTITYMARKET(pCS, dbTransaction, pCaptureMode, tradeIdentification, pSessionInfo, dtSys);

                        //Le commit est effectué uniquement si le beginTran a été généré par CheckAndRecord
                        if (isAutoTransaction)
                            ProcessCommitTran(dbTransaction);

                        break;
                    case 1:
                        // Le trade est enregistré dans une transaction courte
                        // Le reste hors transaction et en séquentiel

                        //Le commit est effectué uniquement si le beginTran a été généré par CheckAndRecord
                        if (isAutoTransaction)
                            ProcessCommitTran(dbTransaction);

                        ProcessUpdPartyTradeIdentifier(pCS, null, pCaptureMode, tradeIdentification);
                        // FI 20210308 [XXXXX] Appel effectué plus haut
                        //ProcessUpdPartyTradeInformation(pCS, null, pCaptureMode, pSessionInfo, tradeIdentification, dtSys, out trader);

                        ProcessCheckConformity(pCaptureMode, pSessionInfo);

                        ProcessTradeLink(pCS, null, pCaptureMode, tradeIdentification.Identifier);
                        ProcessUpdUTIPartyTradeIdentifier(pCS, null, pCaptureMode, tradeIdentification);

                        ProcessSaveTradeOtherStatus(pCS, null, pCaptureMode, idT, pSessionInfo, dtSys);
                        // EG 20211014 Sauvegarde TRADEXML avant TRADETRAIL (pour mise à jour des IDTRADE_P et IDTRADEXML_P dans TRADETRAIL)
                        ProcessSaveTradeXML(pCS, null, pCaptureMode, idT);
                        ProcessSaveTradeTrail(pCS, null, pCaptureMode, idT, pSessionInfo, dtSys);
                        ProcessSaveTradeActorTradeIdLinkId(pCS, null, pCaptureMode, idT);
                        ProcessSaveTradeAsset(pCS, null, pCaptureMode, idT);
                        ProcessSaveInstrumentAndStream(pCS, null, pCaptureMode, idT);
                        ProcesSaveTradeLink(pCS, null, pCaptureMode, idT, tradeIdentification.Identifier);
                        ProcessSavePositionUTI(pCS, null, pCaptureMode, tradeIdentification, pSessionInfo, dtSys, isAutoTransaction);
                        ProcessDeleteEvent(pCS, null, pCaptureMode, idT, pSessionInfo, dtSys);
                        ProcessCopyAttachedDoc(pCS, null, pCaptureMode, idT);
                        ProcessCopyNotepad(pCS, null, pCaptureMode, idT);
                        ProcessSaveENTITYMARKET(pCS, null, pCaptureMode, tradeIdentification, pSessionInfo, dtSys);

                        break;
                    case 2:

                        // Le trade est enregistré dans une transaction courte

                        //Le commit est effectué uniquement si le beginTran a été généré par CheckAndRecord
                        if (isAutoTransaction)
                            ProcessCommitTran(dbTransaction);

                        // Le reste hors transaction et en séquentiel
                        ProcessUpdPartyTradeIdentifier(pCS, null, pCaptureMode, tradeIdentification);
                        // FI 20210308 [XXXXX] Appel effectué plus haut
                        //ProcessUpdPartyTradeInformation(pCS, null, pCaptureMode, pSessionInfo, tradeIdentification, dtSys, out trader);

                        ProcessCheckConformity(pCaptureMode, pSessionInfo);

                        ProcessTradeLink(pCS, null, pCaptureMode, tradeIdentification.Identifier);
                        ProcessUpdUTIPartyTradeIdentifier(pCS, null, pCaptureMode, tradeIdentification);

                        // Le reste hors transaction et en multitâches
                        List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>
                        {
                            System.Threading.Tasks.Task.Run(() => ProcessSaveTradeOtherStatus(pCS, null, pCaptureMode, idT, pSessionInfo, dtSys)),
                            // EG 20211014 Sauvegarde TRADETRAIL en fin d'exécution des tâches (pour mise à jour des IDTRADE_P et IDTRADEXML_P dans TRADETRAIL)
                            //tasks.Add(
                            //    System.Threading.Tasks.Task.Run(() => ProcessSaveTradeTrail(pCS, null, pCaptureMode, idT, pIdTRK_L, pSessionInfo, dtSys))
                            //    );
                            System.Threading.Tasks.Task.Run(() => ProcessSaveTradeXML(pCS, null, pCaptureMode, idT)),
                            System.Threading.Tasks.Task.Run(() => ProcessSaveTradeActorTradeIdLinkId(pCS, null, pCaptureMode, idT)),
                            System.Threading.Tasks.Task.Run(() => ProcessSaveTradeAsset(pCS, null, pCaptureMode, idT)),
                            System.Threading.Tasks.Task.Run(() => ProcessSaveInstrumentAndStream(pCS, null, pCaptureMode, idT)),
                            // EG 20220330 Sauvegarde TRADELINK en fin d'exécution des tâches (pour mise à jour des liens de factures sur Règlement)
                            //tasks.Add(
                            //    System.Threading.Tasks.Task.Run(() => ProcesSaveTradeLink(pCS, null, pCaptureMode, idT, tradeIdentification.identifier))
                            //    );
                            System.Threading.Tasks.Task.Run(() => ProcessSavePositionUTI(pCS, null, pCaptureMode, tradeIdentification, pSessionInfo, dtSys, isAutoTransaction)),
                            System.Threading.Tasks.Task.Run(() => ProcessCopyAttachedDoc(pCS, null, pCaptureMode, idT)),
                            System.Threading.Tasks.Task.Run(() => ProcessCopyNotepad(pCS, null, pCaptureMode, idT)),
                            System.Threading.Tasks.Task.Run(() => ProcessDeleteEvent(pCS, null, pCaptureMode, idT, pSessionInfo, dtSys)),
                            System.Threading.Tasks.Task.Run(() => ProcessSaveENTITYMARKET(pCS, null, pCaptureMode, tradeIdentification, pSessionInfo, dtSys))
                        };

                        try
                        {
                            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                            // EG 20211014 Sauvegarde TRADETRAIL en fin d'exécution des tâches (pour mise à jour des IDTRADE_P et IDTRADEXML_P dans TRADETRAIL)
                            ProcessSaveTradeTrail(pCS, null, pCaptureMode, idT, pSessionInfo, dtSys);
                            // EG 20220330 Sauvegarde TRADELINK en fin d'exécution des tâches (pour mise à jour des liens de factures sur Règlement)
                            ProcesSaveTradeLink(pCS, null, pCaptureMode, idT, tradeIdentification.Identifier);
                        }
                        catch (AggregateException ae)
                        {
                            throw ae.Flatten();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        break;
                }
            }
            catch (TradeCommonCaptureGenException ex)
            {
                errExc = ex;
                errLevel = ex.ErrLevel;
            }
            catch (Exception ex)
            {
                errLevel = ErrorLevel.FAILURE;
                errExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, errLevel);
            }
            finally
            {
                #region finally
                if (ErrorLevel.SUCCESS != errLevel)
                {
                    //FI 20100331 L'enregistrement n'est pas en succes, un Rollback va se produire
                    //On supprime le message détail associé 
                    //Par exemple : Enregistrement d'un trade ETD avec création de l'asset 
                    //Même si l'asset est correctement créé, si une erreur se produit après sa création un Rollback générale annule la création de cette asset
                    //Il faut donc dupprimer le message détail qui indique la création de l'asset xxx
                    _msgDet = string.Empty;
                    //
                    try
                    {
                        if (isAutoTransaction)
                        {
                            if (isAutoTransaction && (null != dbTransaction) && DataHelper.IsTransactionValid(dbTransaction)) 
                            {
                                DataHelper.RollbackTran(dbTransaction);
                            }
                            else if ((pRecordSettings.recordMode == 1) || (pRecordSettings.recordMode == 2) && idT > 0)
                            {
                                // FI 20240618 [WI974] Application du statut DEACTIV 
                                StatusTools.UpdateTradeStatusActivation(pCS, idT, Cst.StatusActivation.DEACTIV, pSessionInfo.session.IdA, dtSys);
                            }
                        }
                        
                        //En cas d'erreur 
                        //le roolback supprime les titres créé avec succès, il faut donc repositionner l'OTCmlId à zéro
                        //Attention si saisie avec la full alors isNewAssetSpecified = false puisque cette donnée n'est pas sérialisée dans le datadocument
                        //=> Donc si saisie Full avec Erreur lors de la sauvegarde de trade alors que la sauvegarde du titre s'est bien déroulé
                        // securityAsset[*].OTCmlId resteront valorisés (alors q'un rollback s'est produit)
                        // Lors de la prochaine tentative de sauvegarde les titres ne seront pas générés à tort
                        //

                        //ISecurityAsset[] securityAsset = TradeCommonInput.DataDocument.GetSecurityAsset();
                        // EG 20100208 
                        ISecurityAsset[] securityAsset = TradeCommonInput.DataDocument.GetSecurityAsset();
                        for (int i = 0; i < ArrFunc.Count(securityAsset); i++)
                        {
                            if (securityAsset[i].IsNewAssetSpecified && securityAsset[i].IsNewAsset)
                                securityAsset[i].OTCmlId = 0;
                        }
                    }
                    catch { }
                }
                try
                {
                    if (null != lockTrade)
                        LockTools.UnLock(pCS, pDbTransaction, lockTrade, pSessionInfo.session.SessionId);

                }
                catch
                {
                    //Si plantage, le lock sera alors supprimer lors de la fin de la session.
                }

                //
                //20091016 FI [Rebuild identification] alimentation de pIdentifier
                pIdT = idT;
                pIdentifier = tradeIdentification.Identifier;
                // FI 20170404 [23039] Alimentation de oUnderlying et oTrader 
                oUnderlying = underlying;
                oTrader = trader;

                //pas d'action Finale si la sauvegarde est en erreur
                if (null != errExc)
                {
                    throw errExc;
                }
                //
                #endregion finally
            }
            //
            #region action Finale
            //20091016 FI [Rebuild identification] appel à AfterCommit
            //=> si une erreur se produit Spheres génère une exception TradeCommonCaptureGenException
            // cette dernière a dans sa propriété errLevel "ErrorLevel.AFTER_RECORD_ERROR" 
            // le trade s'est bien enregistré le commit a eu lieu
            errExc = null;
            try
            {
                AfterRecord(pCS, idT, pCaptureMode, pSessionInfo);
            }
            catch (TradeCommonCaptureGenException ex)
            {
                errLevel = ErrorLevel.AFTER_RECORD_ERROR;
                errExc = ex;
            }
            catch (Exception ex)
            {
                errLevel = ErrorLevel.AFTER_RECORD_ERROR;
                errExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, errLevel);
            }


            if (null != errExc)
                throw errExc;

            #endregion
        }

        /// <summary>
        /// Contrôle la validité de identifier de trade
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdentifier"></param>
        protected virtual void CheckIdentifier(string pCS, Cst.Capture.ModeEnum pCaptureMode, string pIdentifier)
        {
        }

        ///<summary>
        ///Contrôle des validationRules
        ///<para></para>
        ///</summary>
        ///<param name="pCS"></param>
        ///<param name="pDbTransaction"></param>
        ///<param name="pCaptureMode"></param>
        ///<param name="pCheckMode"></param>
        ///<param name="pUser"></param>
        // EG 20171115 [23509] Add CaptureSessionInfo pSessionInfo
        public virtual void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CheckTradeValidationRuleBase.CheckModeEnum pCheckMode, User pUser)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="session"></param>
        public virtual void CheckValidationLock(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, AppSession session)
        {
        }


        /// <summary>
        /// Charge le dataDocument et initialie les ccis qui se trouvent dans TradeCommonInput
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSetNewCustomcapturesInfos"></param>
        /// <returns></returns>
        public virtual bool Load(string pCS, IDbTransaction pDbTransaction, int pIdT, Cst.Capture.ModeEnum pCaptureMode,
                                User pUser, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {
            return Load(pCS, pDbTransaction, pIdT.ToString(), SQL_TableWithID.IDType.Id, pCaptureMode, pUser, pSessionId, pIsSetNewCustomcapturesInfos);
        }

        /// <summary>
        /// Charge le dataDocument et initialie les ccis qui se trouvent dans TradeCommonInput
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSetNewCustomcapturesInfos"></param>
        /// <returns></returns>
        public virtual bool Load(string pCS, IDbTransaction pDbTransaction, string pIdentifier, Cst.Capture.ModeEnum pCaptureMode,
                                User pUser, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {
            return Load(pCS, pDbTransaction, pIdentifier, SQL_TableWithID.IDType.Identifier, pCaptureMode, pUser, pSessionId, pIsSetNewCustomcapturesInfos);
        }

        /// <summary>
        /// Charge le dataDocument et initialie les ccis qui se trouvent dans TradeCommonInput
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pCaptureMode">Type de saisie</param>
        /// <param name="pUser">User connecté (nécessaire à la gestion des accès aux trades[SESSIONRESTRICT])</param>
        /// <param name="pSessionId">Identifiant de la session</param>
        /// <param name="pIsSetNewCustomcapturesInfos"></param>
        /// <returns></returns>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public virtual bool Load(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, Cst.Capture.ModeEnum pCaptureMode,
                User pUser, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {

            //Chargement du dataDocument
            bool isOk = TradeCommonInput.SearchAndDeserialize(pCS, pDbTransaction, pId, pIdType, pCaptureMode, pUser, pSessionId);

            if (Cst.Capture.IsModeAction(pCaptureMode))
                TradeCommonInput.InitializeForAction(pCS, pCaptureMode);

            if (Cst.Capture.IsModeUpdateFeesUninvoiced(pCaptureMode))
                TradeCommonInput.InitializeInvoicedFees(pCS);

            if (pIsSetNewCustomcapturesInfos)
            {
                //Nouvelle instance de la collection CustomCaptureInfos, elle est purgée au passage
                TradeCommonInput.InitializeCustomCaptureInfos(pCS, pUser, pSessionId, Cst.Capture.IsModeNew(pCaptureMode));
            }
            else
            {
                //Spheres conserve les items de la collection
                //Spheres synchronize les pointeurs des CciContainers 
                //Spheres reinit les newValue et les lastValue
                if (null != TradeCommonInput.CustomCaptureInfos)
                {
                    TradeCommonInput.CustomCaptureInfos.InitializeCciContainer();
                    TradeCommonInput.CustomCaptureInfos.ClearAllValue();
                }
            }

            return isOk;
        }

        /// <summary>
        /// Alimente le repository de Spheres® avec des nouveaux assets en fonction des données saisies dans le trade
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsGlobalTransaction"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pTemplateIdentifierUnderLyingref"></param>
        /// <param name="pIsCheckLicense"></param>
        /// <param name="oUnderlying">Retourne la laiste  des assets créés (null si aucun asset)</param>
        /// FI 20170404 [23039] Modification signature (utilisation paramètre oUnderlying)
        protected virtual void SaveUnderlyingAsset(string pCS, IDbTransaction pDbTransaction, Boolean pIsGlobalTransaction, CaptureSessionInfo pCaptureSessionInfo, DateTime pDtSys,
            string pTemplateIdentifierUnderLyingref, Boolean pIsCheckLicense,
            out Pair<int, string>[] oUnderlying)
        {
            oUnderlying = null;
        }

        /// <summary>
        /// Vérifie les assets présents dans le trade
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pTemplateIdentifierUnderLyingref"></param>
        /// <param name="pIdentifierUnderLying"></param>
        /// <param name="pIdTUnderlying"></param>
        protected virtual void CheckUnderlyingAsset(string pCS, IDbTransaction pDbTransaction)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSysBusiness"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20220210 [25939] Correction RESERVED sur CaptureMode
        protected virtual void UpdateTradeSourceStUsedBy(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            string lblStUsedBy = Cst.ProcessTypeEnum.TRADEACTGEN.ToString() + ":" + pCaptureMode.ToString();
            if (Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode) || (pCaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination))
            {
                TradeCommonCaptureGen.UpdateTradeStSysUsedBy(pCS, pDbTransaction, TradeCommonInput.IdT,
                    pSessionInfo, pDtSys, Cst.StatusUsedBy.RESERVED, lblStUsedBy);
            }
            else if (Cst.Capture.IsModeRemoveReplace(pCaptureMode))
            {
                TradeCommonCaptureGen.UpdateTradeStSysUsedBy(pCS, pDbTransaction, TradeCommonInput.IdT,
                    pSessionInfo, pDtSys, Cst.StatusUsedBy.RESERVED, lblStUsedBy);
                TradeCommonCaptureGen.UpdateTradeStSysUsedBy(pCS, pDbTransaction, TradeCommonInput.RemoveTrade.idTCancel,
                    pSessionInfo, pDtSys, Cst.StatusUsedBy.RESERVED, lblStUsedBy);
            }
        }

        /// <summary>
        /// Serialize le trade (disponible TradeCommonInput), Ajoute les informations de validation XSD puis génère l'ensemble dans le folder Temporary de l'application
        /// </summary>
        /// <param name="pSession">Application</param>
        /// <param name="pFolder">Folder généré</param>
        /// <param name="pFileName">Fichier généré</param>
        public void WriteTradeXMLOnTemporary(AppSession pSession, out string pFolder, out string pFileName)
        {
            pFolder = pSession.MapTemporaryPath("Trade_xml", AppSession.AddFolderSessionId.True);
            SystemIOTools.CreateDirectory(pFolder);

            pFileName = FileTools.GetUniqueName("Trade", TradeCommonInput.Identifier) + ".xml";
            // FI 20200409 [XXXXX] add call WriteTradeXMLOnTemporary
            WriteTradeXMLOnTemporary(pSession, pFolder, pFileName);
        }

        /// <summary>
        /// Serialize le trade (disponible TradeCommonInput), Ajoute les informations de validation XSD. 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="pFolder"></param>
        /// <param name="pFileName"></param>
        /// FI 20200409 [XXXXX] add
        public void WriteTradeXMLOnTemporary(AppSession session, string pFolder, string pFileName)
        {

            //Serialize
            EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(TradeCommonInput.FpMLDataDocReader, session.AppInstance.MapPath("BusinessSchemas/"))
            {
                Source = TradeCommonInput.ProductSource()
            };
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, Encoding.UTF8);
            //
            //Write File with xsd validation info
            XSDValidation xsd = new XSDValidation(serializeInfo.Schemas);
            xsd.WriteFile(sb.ToString(), pFolder + @"\" + pFileName);
        }

        /// <summary>
        /// Obtient true si le type de saisie (Modification, Suppression, etc..) est autorisée via à vis de ACTIONTUNING
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="msgControl"></param>
        /// <returns></returns>
        /// FI 20151203 [21613] Modify demo RBS
        public bool CheckCaptureModeAllowed(string pCS, InputUser pInputUser, out string pMsgControl)
        {
            bool isOk = true;
            pMsgControl = string.Empty;

            if (((Cst.Capture.IsModeNewCapture(pInputUser.CaptureMode) && (false == Cst.Capture.IsModeNew(pInputUser.CaptureMode))) ||
                   Cst.Capture.IsModeUpdateGen(pInputUser.CaptureMode)))
            {
                // FI 20151203 [21613] Mise en commentaire
                //pInputUser.InitActionTuning(CS, TradeCommonInput.Product.idI);
                if (pInputUser.ActionTuning.DrSpecified)
                {
                    //Ctrl de cohérence avec l'éventuel paramétrage présent dans ACTIONTUNING 
                    isOk = TuningTools.IsActionAllowed(pCS, TradeCommonInput.IdT, pInputUser.ActionTuning,
                        TradeCommonInput.TradeStatus, TradeCommonInput.SQLLastTradeLog.IdA, out pMsgControl);
                }
            }

            return isOk;
        }


        /// <summary>
        /// Retourne un message suite à l'appel de CheckAndRecord
        /// </summary>
        /// <param name="pEx">Représente l'exception rencontrée lors de la sauvegarde, null possible</param>
        /// <param name="pMode">Représente le type de saisie</param>
        /// <param name="pNewIdentifier">Représente l'lidentifier du trade</param>
        /// <param name="pUnderlying">Représente les assets sous-jacents injectés, null possible</param>
        /// <param name="pTrader">Représente les traders injectés, null possible</param>
        /// <param name="pIsAddinnerExceptionInMsgDet">si true, ajoute le message détail de l'exception non gérée à l'origine de l'exception {pEx}.
        /// Remarque : Les exception de type SQL sont ajoutées dans le message détail même si {pIsAddinnerExceptionInMsgDet} = false</param>
        /// <param name="pMsgDet">retourne le message détail associé</param>
        /// <returns></returns>
        /// FI 20140415 [XXXXX] Add Cas TradeCommonCaptureGen.ErrorLevel.DELETE_ERROR
        /// FI 20160517 [22148] Modify 
        /// FI 20170404 [23039] Modify (Modification de signature pUnderlying, pTrader) 
        public virtual string GetResultMsgAfterCheckAndRecord(string pCS, TradeCommonCaptureGenException pEx, Cst.Capture.ModeEnum pMode, string pNewIdentifier,
                            Pair<int, string>[] pUnderlying, Pair<int, string>[] pTrader, bool pIsAddinnerExceptionInMsgDet, out string pMsgDet)
        {
            pMsgDet = string.Empty;
            string ret = string.Empty;

            TradeCommonCaptureGen.ErrorLevel retSav = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            if (null != pEx)
                retSav = pEx.ErrLevel;

            bool IsRecordInSuccess = TradeCommonCaptureGen.IsRecordInSuccess(retSav);

            //20091016 FI [Rebuild identification] use IsRecordInSuccess
            if (TradeCommonCaptureGen.IsRecordInSuccess(retSav))
            {
                if (Cst.Capture.IsModeUpdateGen(pMode) || Cst.Capture.IsModeCorrection(pMode))
                    ret = Ressource.GetString("Msg_ConfirmTradeSaved");
                else if (Cst.Capture.IsModeRemoveOnlyAll(pMode))
                    ret = Ressource.GetString("Msg_ConfirmTradeRemoved");
                else if (Cst.Capture.IsModeRemoveReplace(pMode))
                    ret = Ressource.GetString("Msg_ConfirmTradeReplace");
                else if (Cst.Capture.IsModePositionTransfer(pMode))
                    ret = Ressource.GetString("Msg_ConfirmTradeTransfer");
                else
                    ret = Ressource.GetString("Msg_ConfirmTradeCreated");
                //
                if (TradeCommonCaptureGen.IsErrorAfterRecord(retSav))
                    pMsgDet = Ressource.GetString("Msg_ErrorAfterTradeSaved") + Cst.CrLf;
            }
            else
            {
                switch (retSav)
                {
                    case TradeCommonCaptureGen.ErrorLevel.ENVIRONNEMENT_UNDEFINED:
                        ret = Ressource.GetString("Msg_StEnvironmentError");
                        ret = ret.Replace("{0}", TradeCommonInput.TradeStatus.stEnvironment.NewSt);
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.IDENTIFIER_ERROR:
                        ret = Ressource.GetString("Msg_IdentifierError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.IDENTIFIER_NOTFOUND:
                        ret = Ressource.GetString("Msg_IdentifierNotFound");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.IDENTIFIER_DUPLICATE:
                        ret = Ressource.GetString("Msg_IdentifierDuplicate");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.XMLDOCUMENT_NOTCONFORM:
                        ret = Ressource.GetString("Msg_TradeNotConformity");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.VALIDATIONRULE_ERROR:
                        // FI 20160517 [22148] si action Annulation => Pas de ss-titre pour ne pas alourdir 
                        // => il y aura toute de même du détail dans le message final (variable pMsgDet sera renseignée)  
                        if (false == (Cst.Capture.IsModeRemoveOnlyAll(pMode)))
                            ret = Ressource.GetString("Msg_TradeValidationRuleError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.LOCK_ERROR:
                        ret = Ressource.GetString("Msg_TradeLockError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.LOCKPROCESS_ERROR:
                        ret = Ressource.GetString("Msg_ProcessLockError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.STATUS_ERROR:
                        ret = Ressource.GetString("Msg_TradeStatusError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.ROWVERSION_ERROR:
                        ret = Ressource.GetString(DataHelper.GetSQLErrorMessage(SQLErrorEnum.Concurrency));
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.LICENSEE_ERROR:
                        ret = Ressource.GetString("Msg_LicenseeError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.SAVEUNDERLYING_ERROR:
                        ret = Ressource.GetString("Msg_SaveUnderlyingError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.CHECKUNDERLYING_ERROR:
                        ret = Ressource.GetString("Msg_CheckUnderlyingError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.SECURITYSUSE_ERROR:
                        ret = Ressource.GetString("Msg_SecurityUseError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.SECURITYSUSE_ERROR2:
                        ret = Ressource.GetString("Msg_SecurityUseError2");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.SECURITYIDENTIFIER_DUPLICATE:
                        ret = Ressource.GetString("Msg_SecurityIdentifierDuplicate");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.SPECIFICCHECK_ERROR:
                        ret = Ressource.GetString("Msg_TradeValidationRuleError"); // Glop: Todo RD,FI 20100330: Création d'une ressource adaptée
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.EDIT_TRADE_ERROR:
                    case TradeCommonCaptureGen.ErrorLevel.EDIT_EVENT_ERROR:
                        ret = Ressource.GetString("Msg_EditDatabaseError");
                        break;
                    case TradeCommonCaptureGen.ErrorLevel.DELETE_ERROR:
                        // FI 20140415 [XXXXX] affichage du message présent dans DataHelperException
                        if ((pEx.InnerException != null) && pEx.InnerException.GetType().Equals(typeof(DataHelperException)))
                            ret = pEx.InnerException.Message;
                        break;
                    default:
                        ret = retSav.ToString();
                        break;
                }
                //
                if (TradeCommonCaptureGen.ErrorLevel.SUCCESS != retSav)
                {
                    string res = "Msg_ConfirmTradeSavedError";
                    if (Cst.Capture.IsModeRemoveOnlyAll(pMode)) //FI 20160517 [22172] Message plus adaptée si action annulation
                        res = "Msg_ConfirmRemoveTradeError";

                    //FI 20160517 [22148] add test StrFunc.IsFilled(ret)
                    ret = Ressource.GetString(res) + (StrFunc.IsFilled(ret) ? Cst.CrLf2 + ret : string.Empty);
                }
            }
            //
            if (StrFunc.IsFilled(ret))
            {
                // 20081216 RD 16416
                if ((IsRecordInSuccess) && (Cst.Capture.IsModeUpdateGen(pMode)) &&
                    (pNewIdentifier != TradeCommonInput.SQLTrade.Identifier))
                {
                    string msgIdentifierChanged = Ressource.GetString("Msg_IdentifierChanged");
                    msgIdentifierChanged = msgIdentifierChanged.Replace("{0}", pNewIdentifier);

                    ret = ret.Replace("{0}", TradeCommonInput.SQLTrade.Identifier);
                    ret += Cst.CrLf + msgIdentifierChanged;
                }
                else
                    ret = ret.Replace("{0}", pNewIdentifier);
                //
                if (pMode == Cst.Capture.ModeEnum.RemoveReplace)
                    ret = ret.Replace("{1}", TradeCommonInput.RemoveTrade.idTCancelIdentifier);

                //FI 20100331 si l'enregistrement n'est pas en succes, un Rollback s'est produit
                //les sous jacents n'ont pas été créé 
                if (IsRecordInSuccess)
                {
                    // FI 20170404 [23039] call GetMsgDetAutoCreate
                    if (ArrFunc.IsFilled(pUnderlying))
                    {
                        if (StrFunc.IsFilled(pMsgDet))
                            pMsgDet = $"{pMsgDet}{Cst.CrLf2}";
                        pMsgDet = $"{pMsgDet}{GetMsgDetAutoCreate("Underlying", pUnderlying)}";
                    }

                    if (ArrFunc.IsFilled(pTrader))
                    {
                        if (StrFunc.IsFilled(pMsgDet))
                            pMsgDet = $"{pMsgDet}{Cst.CrLf2}";
                        pMsgDet = $"{pMsgDet}{GetMsgDetAutoCreate("Trader", pTrader)}";
                    }
                }
            }
            //
            //20090416 PL Astuce "temporaire" ...
            if (TradeCommonInput.SQLProduct.IsAssetProduct)
                ret = Ressource.GetTrade(ret);

            if (null != pEx)
            {
                string sqlMessageError = string.Empty;

                // FI 20140415 [XXXXX] Mise en place de la méthode ExceptionTools.GetFirstRDBMSException
                bool isSqlError = false;
                Exception RDBMSException = ExceptionTools.GetFirstRDBMSException(pEx);
                if (null != RDBMSException)
                    isSqlError = DataHelper.AnalyseSQLException(pCS, RDBMSException, out sqlMessageError, out SQLErrorEnum _);

                if (isSqlError)
                    ret += $"{Cst.CrLf2}{Ressource.GetString("Msg_AfterSqlError")}";

                //Alimentation du message detail 
                //Alimentation  
                //- si exception gérée (false == pEx.IsInnerException)
                //- si exception SQL 
                //- si exception non gérée et pIsAddinnerExceptionInMsgDet
                bool isAddMsgDet = ((false == pEx.IsInnerException) || isSqlError || (pEx.IsInnerException && pIsAddinnerExceptionInMsgDet));
                if (isAddMsgDet)
                {
                    string messageDet = string.Empty;
                    if (false == pEx.IsInnerException)
                    {
                        messageDet = pEx.Message;
                    }
                    else if (isSqlError)
                    {
                        messageDet = sqlMessageError;
                    }
                    else if (pEx.IsInnerException && pIsAddinnerExceptionInMsgDet)
                    {
                        messageDet = pEx.InnerException.Message;
                    }
                    if (StrFunc.IsFilled(messageDet))
                    {
                        if (StrFunc.IsFilled(pMsgDet))
                            pMsgDet = $"{pMsgDet}{Cst.CrLf2}";
                        pMsgDet += messageDet;
                    }
                }
            }

            if (TradeCommonCaptureGen.IsRecordInSuccess(retSav) && StrFunc.IsFilled(_msgDet))
            {
                if (StrFunc.IsFilled(pMsgDet))
                    pMsgDet = $"{pMsgDet}{Cst.CrLf2}";
                pMsgDet += _msgDet;
            }

            return ret;

        }
        /// <summary>
        /// Retourne un message qui indique la création de {pListNewElement}
        /// </summary>
        /// <param name="pNewElement">Représente le type d'élément (ex Underlying,Trader) </param>
        /// <param name="pListNewElement">Représente les éléments créés (Id + identifier) </param>
        /// <returns></returns>
        /// FI 20170404 [23039] Add pUnderlying et pTrader
        private static string GetMsgDetAutoCreate(string pNewElement, Pair<int, string>[] pListNewElement)
        {
            string ret = string.Empty;

            if (ArrFunc.IsFilled(pListNewElement))
            {
                string resource = (pListNewElement.Length == 1) ? "Msg_New{0}" : "Msg_New{0}s";
                resource = StrFunc.AppendFormat(resource, pNewElement);

                ret = Ressource.GetString(resource);
                ret = ret.Replace("{0}", ArrFunc.GetStringList((from item in pListNewElement
                                                                select StrFunc.AppendFormat("{0} (id:{1})", item.Second, item.First)).ToArray(), Cst.CrLf));

            }
            return ret;
        }


        /// <summary>
        /// Initialisation au préalable avant de rentrer dans le type de saisie (Mofication, Suppression, etc..)
        /// </summary>
        /// <param name="pInputUser"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Use dbTransaction  
        public virtual void InitBeforeCaptureMode(string pCS, IDbTransaction pDbTransaction, InputUser pInputUser, CaptureSessionInfo pSessionInfo)
        {
            #region Set non serializable attribut
            if (Cst.Capture.IsModeDuplicateOrReflect(pInputUser.CaptureMode) ||
                Cst.Capture.IsModeUpdateGen(pInputUser.CaptureMode))
            {
                ISecurityAsset[] securityAsset = TradeCommonInput.DataDocument.GetSecurityAsset();
                for (int i = 0; i < ArrFunc.Count(securityAsset); i++)
                {
                    securityAsset[i].IsNewAssetSpecified = true;
                    securityAsset[i].IsNewAsset = (0 == securityAsset[i].OTCmlId);
                    securityAsset[i].IdTTemplateSpecified = false;
                }
            }
            #endregion


            //
            #region reinit des identifiants
            //20091016 FI [Rebuild identification] 
            ///Lorsque les identifiants du trade ont été construits par le XSL on les supprime, ils seront recalculés lors de la sauvegarde 
            if (Cst.Capture.IsModeUpdateGen(pInputUser.CaptureMode))
            {
                SpheresIdentification currentIdentification = TradeCommonInput.Identification;

                string defaultvalue = TradeIdentificationBuilder.ConstDefault;
                SpheresIdentification initialValue = new SpheresIdentification(defaultvalue, defaultvalue, defaultvalue, defaultvalue);

                TradeIdentificationBuilder tradeIdentBuilder = new TradeIdentificationBuilder(null, TradeCommonInput, initialValue);
                SpheresIdentification newIdentification = tradeIdentBuilder.BuildIdentification(pCS, pDbTransaction, pInputUser.CaptureMode, pSessionInfo);

                if (newIdentification.Identifier == currentIdentification.Identifier)
                {
                    // RD 20150807 Save identifier
                    currentIdentification.LastIdentifier = currentIdentification.Identifier;
                    currentIdentification.Identifier = string.Empty;
                }
                if (newIdentification.Displayname == currentIdentification.Displayname)
                    currentIdentification.Displayname = string.Empty;
                if (newIdentification.Description == currentIdentification.Description)
                    currentIdentification.Description = string.Empty;
                if (newIdentification.Extllink == currentIdentification.Extllink)
                    currentIdentification.Extllink = string.Empty;
            }
            #endregion
        }

        /// <summary>
        /// Génère les actions en cascade à effectuer après la sauvegarde du trade 
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        protected virtual void AfterRecord(string pCS, int pIdT, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo)
        {
        }


        /// <summary>
        /// Methode à overrider pour des contrôles spécifiques
        /// Ex Pour les titres, afin de contrôler qu'un titre n'est pas déjà utilisé par un trade classique
        /// </summary>
        /// <returns></returns>
        protected virtual void CheckSpecific(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, DateTime pDtRefForDtEnabled)
        {
        }

        ///<summary>
        ///Serialize le dataDocument et vérifie qu'il respecte le xsd 
        ///</summary>
        ///<param name="pSessionInfo"></param>
        private void CheckXMLConformity(CaptureSessionInfo pSessionInfo)
        {
            EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(TradeCommonInput.FpMLDataDocReader,
                                                                            pSessionInfo.session.AppInstance.MapPath(@"BusinessSchemas\"))
            {
                Source = TradeCommonInput.ProductSource()
            };
            CacheSerializer.Conformity(serializeInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string SerializeDataDocument()
        {
            EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(TradeCommonInput.FpMLDocReader)
            {
                Source = TradeCommonInput.ProductSource()
            };
            StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
            return sb.ToString();
        }

        /// <summary>
        /// Vérifie que la trade n'a pas déjà subit en modification en base par rapport au dernier chargement
        /// <para>Les vérifications s'effectue sur la table TRADE</para>
        /// </summary>
        ///<exception cref="TradeCommonCaptureGenException[ROWVERSION_ERROR] si le trade a été modifé"/>
        // EG 20180205 [23769] Use dbTransaction  
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected void CheckRowVersion(string pCS, IDbTransaction pDbTransaction)
        {
            SQL_TradeCommon sqltrade = new SQL_TradeCommon(pCS, TradeCommonInput.IdT)
            {
                DbTransaction = pDbTransaction,
                IsAddRowVersion = true
            };
            sqltrade.LoadTable(new string[] { "TRADE.IDT" });
            //
            if (sqltrade.RowVersion != TradeCommonInput.SQLTrade.RowVersion)
                throw new TradeCommonCaptureGenException("CheckRowVersion", string.Empty, ErrorLevel.ROWVERSION_ERROR);
        }

        /// <summary>
        /// Pose un lock sur le trade (utilisé pour la modification  par exemple)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pScope"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pTradeIdentification"></param>
        ///<exception cref="TradeCommonCaptureGenException[LOCK_ERROR] si le lock n'a pu être posé"/>
        // EG 20180205 [23769] Use dbTransaction  
        protected LockObject LockTrade(string pCS, IDbTransaction pDbTransaction,
                                       Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo,
                                       SpheresIdentification pTradeIdentification)
        {
            return LockTrade(pCS, pDbTransaction, Cst.FeeScopeEnum.Trade, pCaptureMode, pSessionInfo, pTradeIdentification);
        }
        /// <summary>
        /// Pose un lock sur le trade (utilisé pour la modification par exemple) ou pose un lock sur l'ordre relatif au trade (utilisé pour le calcul des frais par ordre)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pScope"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pDtSys">Date début procédure d'enregistrement</param>
        /// <returns></returns>
        //PL 20191210 [25099]
        protected LockObject LockTrade(string pCS, IDbTransaction pDbTransaction,
                                       Cst.FeeScopeEnum pScope, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo,
                                       SpheresIdentification pTradeIdentification)
        {
            int lockMode = 1; //Mode sans pause entre les différentes tentatives
            string objectId = TradeCommonInput.IdT.ToString();

            switch (pScope)
            {
                case Cst.FeeScopeEnum.OrderId:
                case Cst.FeeScopeEnum.FolderId:
                    lockMode = 2; //Mode avec pause entre les différentes tentatives
                    objectId = pScope.ToString() + ":" + pTradeIdentification.Identifier;
                    break;
            }

            LockObject lockTrade = new LockObject(TypeLockEnum.TRADE, objectId, pTradeIdentification.Identifier, LockTools.Exclusive);
            Lock lck = new Lock(pCS, pDbTransaction, lockTrade, pSessionInfo.session, Cst.Capture.GetLabel(pCaptureMode));
            bool isLockSuccessful = (lockMode == 1 ? LockTools.LockMode1(lck, out Lock lckExisting) : LockTools.LockMode2(lck, out lckExisting));
            if (false == isLockSuccessful)
            {
                if (null != lckExisting)
                    throw new TradeCommonCaptureGenException("CheckAndRecord", lckExisting.ToString(), ErrorLevel.LOCK_ERROR);
            }
            return lockTrade;
        }

        /// <summary>
        /// Retourne true lorsque le code Retour pErrLevel exprime un enregistrement de trade réalisé avec succes
        /// </summary>
        /// <param name="pErrLevel"></param>
        /// <returns></returns>
        public static bool IsRecordInSuccess(TradeCommonCaptureGen.ErrorLevel pErrLevel)
        {
            return (pErrLevel == ErrorLevel.SUCCESS) || IsErrorAfterRecord(pErrLevel);
        }

        /// <summary>
        /// Retourne true lorsque le code Retour pErrLevel exprime un enregistrement de trade réalisé avec succes 
        /// et que des actions menées après le l'enregistrement sont en erreur
        /// </summary>
        /// <param name="pErrLevel"></param>
        /// <returns></returns>
        public static bool IsErrorAfterRecord(TradeCommonCaptureGen.ErrorLevel pErrLevel)
        {
            return (pErrLevel == ErrorLevel.AFTER_RECORD_ERROR);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys">Date début procédure d'enregistrement</param>
        /// <param name="pStUsedBy"></param>
        /// <param name="pLibStUsedBy"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected static void UpdateTradeStSysUsedBy(string pCS, IDbTransaction pDbTransaction, int pIdT,
            CaptureSessionInfo pSessionInfo, DateTime pDtSys,
            Cst.StatusUsedBy pStUsedBy, string pLibStUsedBy)
        {

            SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(pCS, pIdT);
            sqlTradeStSys.UpdateTradeStUsedBy(pDbTransaction, pSessionInfo.session.IdA, pStUsedBy, pLibStUsedBy, pDtSys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureMode"></param>
        ///<param name="pSessionInfo"></param>
        /// <param name="pDtSys">Date début procédure d'enregistrement</param>
        /// <param name="pIsNewRow"></param>
        // EG 20180514 [23812] Report
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected virtual void SaveTradeOthersStatus(string pCS, IDbTransaction pDbTransaction, int pIdT,
                                Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            UpdateTradeSourceStUsedBy(pCS, pDbTransaction, pCaptureMode, pSessionInfo, pDtSys);
            TradeCommonInput.TradeStatus.UpdateStUser(pCS, pDbTransaction, Status.Mode.Trade, pIdT, pSessionInfo.user.IdA, pDtSys);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetTradeTransfered()
        {
            return null;
        }

        /// <summary>
        /// Retourne true si le trade peut-être sauvegardé alors qu'il est incomplet 
        /// </summary>
        /// FI 20140708 [20179] Modify: Gestion du mode IsModeMatch
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public virtual bool IsInputIncompleteAllow(Cst.Capture.ModeEnum pCaptureMode)
        {
            // FI 20140708 [20179] Mise en place d'une exception en mode IsModeMatch ou IsModeConsult Spheres® ne doit pas appeler cette méthode
#if DEBUG
            if (Cst.Capture.IsModeMatch(pCaptureMode) || Cst.Capture.IsModeConsult(pCaptureMode))
                throw new ArgumentException(StrFunc.AppendFormat("CaptureMode {0}) is invalid", pCaptureMode.ToString()));
#endif
            bool ret = false;

            //La saisie de la remplaçante doit être complete (Il y a des infos ds tradeInput nécessaires le send du message)
            //La saisie d'un exercice, d'une correction, et... doit être complet
            if (Cst.Capture.IsModeAction(pCaptureMode))
                ret = false;
            else if (Cst.Capture.IsModeUpdateFeesUninvoiced(pCaptureMode))
                ret = true;
            else
                ret = TradeCommonInput.TradeStatus.IsStEnvironment_Template ||
                      TradeCommonInput.TradeStatus.IsStActivation_Missing;

            return ret;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // FI 20140703 [20161] add column TRADE.TIMING
        // EG 20171025 [23509] Add DTORDERENTERED, DTEXECUTION, TZFACILITY
        // EG 20180906 PERF Add DTOUT (Alloc ETD only)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20240227 [WI855][WI858] Trade input : Add New Columns TVTIC| TRDID
        private static string[] GetTradeColumns()
        {
            List<string> cols = new List<string>();
            cols.AddRange(new string[]{
            "TRADE.IDT","IDI","IDENTIFIER","DISPLAYNAME","DESCRIPTION",
            "DTTRADE","DTTIMESTAMP","DTORDERENTERED","DTEXECUTION","DTBUSINESS","DTSYS","DTOUT",
            "IDT_SOURCE","IDT_TEMPLATE","SOURCE","IDINVOICINGRULES","TIMING","TZFACILITY","EXTLLINK",
            "IDSTENVIRONMENT","DTSTENVIRONMENT","IDASTENVIRONMENT",
            "IDSTACTIVATION","DTSTACTIVATION","IDASTACTIVATION",
            "IDSTPRIORITY","DTSTPRIORITY","IDASTPRIORITY",
            "IDSTUSEDBY","LIBSTUSEDBY","DTSTUSEDBY","IDASTUSEDBY",
            "IDSTBUSINESS","DTSTBUSINESS","IDASTBUSINESS",
            "IDM","IDM_FACILITY","ASSETCATEGORY", "IDASSET",
            "SIDE","QTY", "UNITQTY","PRICE","UNITPRICE", "TYPEPRICE",
            "STRIKEPRICE","UNITSTRIKEPRICE", "ACCINTRATE",
            "EXECUTIONID","TRDTYPE", "TRDSUBTYPE","SECONDARYTRDTYPE","ORDERID", "ORDERTYPE",
            "IDA_DEALER","IDB_DEALER", "IDA_CLEARER","IDB_CLEARER",
            "IDA_BUYER", "IDB_BUYER","IDA_SELLER","IDB_SELLER",
            "IDA_RISK", "IDB_RISK","IDA_ENTITY","IDA_CSSCUSTODIAN",
            "DTINUNADJ", "DTINADJ","DTOUTUNADJ","DTOUTADJ",
            "DTSETTLT", "DTDLVYSTART","DTDLVYEND","TZDLVY",
            "POSITIONEFFECT", "RELTDPOSID","INPUTSOURCE", "TVTIC", "TRDID"

            });
            return cols.ToArray();
        }


        /// <summary>
        /// Mise à jour du partyTradeIdentifier du datadocument du trade lié 
        /// <para>Le trade lié est TradeLink.TradeLink.IdT_B</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeIdentifier">Représente le trade source</param>
        /// <param name="pIdTLinkedTrade">Représente le trade lié</param>
        /// <param name="pSchemeForLink">Type de lien</param>
        /// <param name="pLinkedTradeIdentifier">Retourne l'identier du trade lié</param>
        /// FI 20131210 [19320] Add method
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // FI 20230711 [XXXXX] Refactoring puisque plantage systématique suites aux évolutuions précédentes (EG 20191115 [25077] RDBMS)
        private static void UpdateLinkedTrade(string pCS, IDbTransaction pDbTransaction, string pTradeIdentifier, int pIdTLinkedTrade, string pSchemeForLink, out string pLinkedTradeIdentifier)
        {

            pLinkedTradeIdentifier = string.Empty;

            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(pCS, pIdTLinkedTrade)
            {
                IsWithTradeXML = true,
                DbTransaction = pDbTransaction
            };

            if (false == sqlTrade.LoadTable(new string[] { "TRADE.IDT", "TRADE.IDENTIFIER", "trx.TRADEXML" }))
                throw new InvalidOperationException($" Trade (IdT:{pIdTLinkedTrade}) doesn't exist.");

            pLinkedTradeIdentifier = sqlTrade.Identifier;

            if (StrFunc.IsFilled(sqlTrade.TradeXml))
            {
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                IDataDocument dataDocument = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);

                IPartyTradeIdentifier[] partyTradeIdentifier = dataDocument.FirstTrade.TradeHeader.PartyTradeIdentifier;
                if (CaptureTools.SetLinkIdToPartyTradeIdentifier(partyTradeIdentifier, pSchemeForLink, pTradeIdentifier))
                {
                    DataParameters parameters = new DataParameters();
                    parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdTLinkedTrade);
                    parameters.Add(new DataParameter(pCS, "TRADEXML", DbType.Xml), CacheSerializer.Serialize(new EFS_SerializeInfo(dataDocument)).ToString());
                    string sqlQuery = @"update dbo.TRADEXML set TRADEXML = @TRADEXML where (IDT = @IDT)";
                    QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);

                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
            }
        }

        /// <summary>
        /// Mise à jour des tradeId sous partyTradeIdentifier
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        /// FI 20140206 [19564] add virtual Method
        /// FI 20140307 [19689] add pDbTransaction
        // EG 20180205 [23769] use dbTransaction
        protected void UpdatePartyTradeIdentifier(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification)
        {
            // Mise à jour des UTI
            UpdateUTIPartyTradeIdentifier(pCS, pDbTransaction, pCaptureMode, pTradeIdentification);


            if (ArrFunc.IsFilled(TradeCommonInput.CurrentTrade.TradeHeader.PartyTradeIdentifier))
            {
                for (int i = 0; i < TradeCommonInput.CurrentTrade.TradeHeader.PartyTradeIdentifier.Length; i++)
                {
                    IPartyTradeIdentifier partyTradeIdentifier = TradeCommonInput.CurrentTrade.TradeHeader.PartyTradeIdentifier[i];
                    bool addTradeIdentifier = CaptureTools.IsDocumentElementValid(partyTradeIdentifier.PartyReference);
                    //
                    if (addTradeIdentifier)
                    {
                        ISchemeId tradeId = partyTradeIdentifier.GetTradeIdFromScheme(Cst.OTCml_TradeIdScheme);
                        if (null == tradeId)
                            tradeId = partyTradeIdentifier.GetTradeIdWithNoScheme();
                        //
                        if (null == tradeId)
                        {
                            ReflectionTools.AddItemInArray(partyTradeIdentifier, partyTradeIdentifier.GetTradeIdMemberName(), 0);
                            tradeId = partyTradeIdentifier.TradeId[partyTradeIdentifier.TradeId.Length - 1];
                        }
                        //
                        if (null != tradeId)
                        {
                            partyTradeIdentifier.TradeIdSpecified = true;
                            tradeId.Scheme = Cst.OTCml_TradeIdScheme;
                            if (TradeCommonInput.SQLProduct.IsAssetProduct)
                            {
                                tradeId.Value = pTradeIdentification.Identifier;
                            }
                            else // TRADE classique et facture
                            {
                                bool isTradeidToFill = false;
                                SQL_Book sqlbook = null;
                                // FI 20130702 [18798] add test sur bookId.OTCmlId > 0 => garde fou
                                // on peut avoir partyTradeIdentifier.bookIdSpecified et partyTradeIdentifier.bookId.OTCmlId ==0 
                                // voir DumpBook_ToDocument
                                if (partyTradeIdentifier.BookIdSpecified && partyTradeIdentifier.BookId.OTCmlId > 0)
                                {
                                    sqlbook = new SQL_Book(CSTools.SetCacheOn(pCS), partyTradeIdentifier.BookId.OTCmlId, SQL_Table.ScanDataDtEnabledEnum.Yes)
                                    {
                                        DbTransaction = pDbTransaction,
                                        IsUseTable = true
                                    };
                                }

                                if ((null != sqlbook) && sqlbook.LoadTable(new string[] { "IDA_ENTITY" }) && sqlbook.IdA_Entity > 0)
                                {
                                    isTradeidToFill = true;
                                }
                                else if (Cst.Capture.IsModeUpdateOrUpdatePostEvts(pCaptureMode)
                                        || Cst.Capture.IsModeDuplicateOrReflect(pCaptureMode))
                                {
                                    // FI 20130702 [18798] add test sur bookId.OTCmlId > 0 => garde fou
                                    // on peut avoir partyTradeIdentifier.bookIdSpecified et partyTradeIdentifier.bookId.OTCmlId ==0 
                                    // voir DumpBook_ToDocument
                                    if (partyTradeIdentifier.BookIdSpecified && partyTradeIdentifier.BookId.OTCmlId > 0)
                                    {
                                        // RD 20120403 [17737] 
                                        // En mode Modification et Duplication, alimenter tradeId, même si le Book est désactivé ou non géré
                                        if ((null != sqlbook) && (false == sqlbook.IsLoaded))
                                        {
                                            sqlbook = new SQL_Book(CSTools.SetCacheOn(pCS), partyTradeIdentifier.BookId.OTCmlId, SQL_Table.ScanDataDtEnabledEnum.No)
                                            {
                                                DbTransaction = pDbTransaction,
                                                IsUseTable = true
                                            };
                                        }
                                    }
                                    isTradeidToFill = ((null != sqlbook) && sqlbook.IsFound);
                                }

                                if (isTradeidToFill)
                                    tradeId.Value = pTradeIdentification.Identifier;
                                else
                                {
                                    // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
                                    // Suppression du tradeId sur la venue inutile et alimenté à 0 (pas de book) depuis l'arrivée du TVTIC
                                    tradeId.Value = "0";
                                    IParty partyFacility = TradeCommonInput.DataDocument.GetPartyFacility();
                                    if ((null != partyFacility) && (partyFacility.Id == partyTradeIdentifier.PartyReference.HRef))
                                        partyTradeIdentifier.RemoveTradeId(Cst.OTCml_TradeIdScheme);
                                }
                            }
                        }
                    }
                    else
                    {
                        // EG 20240227 [WI855] Refactoring
                        partyTradeIdentifier.RemoveTradeId(Cst.OTCml_TradeIdScheme);
                    }
                }
            }
            //Mise a jour de TRADESIDE => notamment si enregistrement depuis la Full 
            //Mise a jour sauf si trade est un asset (Ex Titre)
            //FI 20110908 [] les Trades Risk alimente le Bloc Trade Side (les trades CashBalance génère de la messagerie)
            if (this.DataIdent != Cst.OTCml_TBL.TRADEDEBTSEC.ToString())
            {
                if (false == TradeCommonInput.DataDocument.CurrentTrade.TradeSideSpecified)
                    TradeCommonInput.DataDocument.SetTradeSide(CSTools.SetCacheOn(pCS), pDbTransaction);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pDtSys"></param>
        /// <param name="oTrader"></param>
        /// FI 20170404 [23039] Add Method
        protected virtual void UpdatePartyTradeInformation(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, SpheresIdentification pTradeIdentification, DateTime pDtSys, out Pair<int, string>[] oTrader)
        {
            oTrader = null;
        }



        /// <summary>
        /// Procédure de mise à jour des tables POSUTI et POSUTIDET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// FI 20140206 [19564] add POSUTI
        protected void ProcessSavePositionUTI(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification, CaptureSessionInfo pSessionInfo, DateTime pDtSys, Boolean pIsAutoTransaction)
        {
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                    (Cst.Capture.IsModeNewCapture(pCaptureMode) || Cst.Capture.IsModeUpdate(pCaptureMode)))
                {
                    try
                    {
                        SavePositionUTI(pCS, pDbTransaction, pTradeIdentification, pSessionInfo.session.IdA, pDtSys, pIsAutoTransaction);
                    }
                    catch (Exception ex)
                    {
                        TradeCommonCaptureGenException stepExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.ADD_POSUTI);
                        throw stepExc;
                    }

                }
            }
        }

        /// <summary>
        /// Procédure de mise à jour des tradeId sous partyTradeIdentifier
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        /// FI 20140206 [19564] add Method
        /// FI 20140307 [19689] add pDbTransaction
        protected void ProcessUpdPartyTradeIdentifier(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification)
        {
            if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {
                try
                {
                    // FI 20140326 [19793] la transaction n'est pas tjs passée
                    UpdatePartyTradeIdentifier(pCS, RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction, pCaptureMode, pTradeIdentification);
                }
                catch (Exception ex)
                {
                    TradeCommonCaptureGenException stepExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.UPD_DOCREADER_ERROR);
                    throw stepExc;
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pDtSys">date système de creéation du trade</param>
        /// <param name="oTrader">Retourne la liste des trades créés (null si aucun trader créé)</param>
        /// FI 20170404 [23039] Add
        protected void ProcessUpdPartyTradeInformation(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, SpheresIdentification pTradeIdentification, DateTime pDtSys, out Pair<int, string>[] oTrader)
        {
            oTrader = null;

            if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {

                try
                {
                    UpdatePartyTradeInformation(pCS, pDbTransaction, pCaptureMode, pSessionInfo, pTradeIdentification, pDtSys, out oTrader);
                }
                catch (Exception ex)
                {
                    TradeCommonCaptureGenException stepExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.UPD_DOCREADER_ERROR);
                    throw stepExc;
                }
            }
        }


        /// <summary>
        /// Procédure de mise à jour de partyTradeIdentifier avec les UTI
        /// <para>Seuls les UTI non renseignés sont calculés</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        /// FI 20140206 [19564] add  Method
        /// FI 20140307 [19689] add pDbTransaction
        protected void ProcessUpdUTIPartyTradeIdentifier(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification)
        {
            if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {
                try
                {
                    // FI 20140326 [19793] la transaction n'est pas tjs passée
                    UpdateUTIPartyTradeIdentifier(pCS, RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction, pCaptureMode, pTradeIdentification);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.UPD_DOCREADER_ERROR);
                }
            }
        }

        /// <summary>
        /// Mise à jour du datadocument avec les UTIs lorsqu'ils sont non renseignés
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        protected virtual void UpdateUTIPartyTradeIdentifier(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification)
        {
        }




        /// <summary>
        /// Procédure de serialization du dataDocument
        /// <para>Retourne le résultat de la serialization</para>
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        /// FI 20140206 [19564] add  Method
        // EG 20180423 Analyse du code Correction [CA2200]
        protected string ProcessSerializeDataDocument(Cst.Capture.ModeEnum pCaptureMode)
        {
            string data = null;
            //FI 20130424 [18601] les modes IsModeRemoveOnlyAll sont exclus 
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                TradeCommonCaptureGenException stepExc;
                try
                {
                    data = SerializeDataDocument();
                }
                catch (TradeCommonCaptureGenException ex) { stepExc = ex; throw; }
                catch (Exception ex)
                {
                    stepExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.SERIALIZE_ERROR);
                    throw stepExc;
                }
            }
            return data;
        }

        /// <summary>
        /// Procédure d'alimentation des tables TRADEACTOR/TRADEID/LINKID
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// FI 20140206 [19564] add  Method
        protected void ProcessSaveTradeActorTradeIdLinkId(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT)
        {
            // RD 20120322 / Intégration de trade "Incomplet"
            // Alimenter les tables LINKID/TRADEACTOR pour les Trades "Incomplet"
            bool isOk = (((false == IsInputIncompleteAllow(pCaptureMode)) || (TradeCommonInput.TradeStatus.IsStActivation_Missing)) &&
                (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)));
            if (pCaptureMode != Cst.Capture.ModeEnum.Duplicate)
                isOk &= (false == TradeCommonInput.IsTradeInInvoice());

            if (isOk)
            {
                try
                {
                    SaveTradeActor(pCS, pDbTransaction, pIdT);

                    SaveTradeIdAndLinkId(pCS, pDbTransaction, pIdT);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LOAD_LINKTABLE_ERROR);
                }
            }
        }

        /// <summary>
        /// Procédure d'alimentation de la table TRADEASSET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIsUpdateOnly_TradeStream"></param>
        /// <param name="pidT"></param>
        /// FI 20140206 [19564] add  Method
        protected void ProcessSaveTradeAsset(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pidT)
        {
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {

                try
                {
                    SaveTradeAsset(pCS, pDbTransaction, pidT);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LOAD_TRADEASSET_ERROR);
                }
            }
        }

        /// <summary>
        /// Procédure de suppression des tables LINKID, TRADEID, TRADELINK, TRADEINSTRUMENT, TRADESTREAM, TRADEACTOR, TRADEASSET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pidT"></param>
        /// <param name="pIsDelTRADEINSTRUMENTAndTRADESTREAM">conditionne la suppression de TRADESTREAM et TRADEINSTRUMENT</param>
        /// FI 20140206 [19564] add Method
        /// FI 20140415 [XXXXX] Delete si modification totale ou partielle et si le trade n'est pas impliqué dans une facture
        /// FI 20160907 [21831] Modify
        protected void ProcessDelete(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pidT)
        {
            // RD 20120322 / Intégration de trade "Incomplet"
            // Deleter les tables LINKID,TRADELINK,TRADEINSTRUMENT,TRADESTREAM,TRADEACTOR pour les Trades "Incomplet"
            //if (((false == IsInputIncompleteAllow(pCaptureMode)) || (TradeCommonInput.TradeStatus.IsStActivation_Missing)) &&
            //     (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            //   )
            // FI 20140415 [XXXXX] Delete si modification totale ou partielle et si le trade n'est pas impliqué dans une facture 
            if (Cst.Capture.IsModeUpdateGen(pCaptureMode) && (false == TradeCommonInput.IsTradeInInvoice()))
            {
                try
                {
                    // FI 20160907 [21831] isDelEvent
                    //Boolean isDelTRADEINSTRUMENTAndTRADESTREAM = true;
                    //DeleteTradeTables(pDbTransaction, pidT, isDelTRADEINSTRUMENTAndTRADESTREAM);

                    Boolean isDelEvent = IsDeleteEvent(pCaptureMode);
                    DeleteTradeTables(pCS, pDbTransaction, pidT, isDelEvent);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.DELETE_ERROR);
                }
            }
        }

        /// <summary>
        /// Procédure d'alimentation de la table TRADETRAIL
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pidT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// FI 20140206 [19564] add Method
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected void ProcessSaveTradeTrail(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pidT, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {

            try
            {
                string screen = RecordSettings.idScreen;
                SaveTradeTrail(pCS, pDbTransaction, pidT, pSessionInfo, screen, pCaptureMode, pDtSys);
                if (Cst.Capture.IsModeRemoveReplace(pCaptureMode))
                    SaveTradeTrail(pCS, pDbTransaction, TradeCommonInput.RemoveTrade.idTCancel, pSessionInfo, screen, Cst.Capture.ModeEnum.RemoveOnly, pDtSys);
            }
            catch (Exception ex)
            {
                throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.ADD_TRADETRAIL);
            }
        }

        /// <summary>
        /// Procédure d'alimentation de la table TRADELINK
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pidT"></param>
        /// <param name="pTradeIdentifier"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcesSaveTradeLink(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, string pTradeIdentifier)
        {
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {

                try
                {
                    SaveTradelink(pCS, pDbTransaction, pCaptureMode, pIdT, pTradeIdentifier);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LOAD_TRADELINK_ERROR);
                }
            }
        }

        /// <summary>
        /// Procédure d'alimentation des autres statuts dans la table TRADE
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pidT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// FI 20140206 [19564] add Method
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected void ProcessSaveTradeOtherStatus(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {

            try
            {
                SaveTradeOthersStatus(pCS, pDbTransaction, pIdT, pCaptureMode, pSessionInfo, pDtSys);
            }
            catch (Exception ex)
            {
                throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.EDIT_TRADESTATUS_ERROR);
            }
        }

        /// <summary>
        /// Procédure d'alimentation de la table TRADE
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pData"></param>
        /// <param name="tradeIdentification"></param>
        /// <param name="?"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        /// FI 20140206 [19564] add Method
        protected int ProcessSaveTrade(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            int idT = TradeCommonInput.Identification.OTCmlId;
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {

                try
                {
                    idT = SaveTrade(pCS, pDbTransaction, pCaptureMode, pTradeIdentification, pSessionInfo, pDtSys);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.EDIT_TRADE_ERROR);
                }
            }
            return idT;
        }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected void ProcessSaveTradeXML(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT)
        {
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                try
                {
                    string data = ProcessSerializeDataDocument(pCaptureMode);
                    SaveTradeXML(pCS, pDbTransaction, pIdT, data, pCaptureMode);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.EDIT_TRADE_ERROR);
                }
            }
        }

        /// <summary>
        /// Procédure de suppression des enregistrements présents dans les tables EVENT, EVENTSI, EVENTCLASS, MCO, EAR, POSREQUEST
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// FI 20140415 [XXXXX] Mise en place d'une DataHelperException en cas d'erreur
        /// FI 20140206 [19564] add Method
        /// FI 20160816 [22146] Modification signature add pCaptureSessionInfo et pDateSys
        /// FI 20160907 [21831] Modify
        // EG 20190308 New m_RecordLock (Utilisation en multithreading sur Delete EVENTS pour eviter DeadLock)
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected void ProcessDeleteEvent(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, CaptureSessionInfo pCaptureSessionInfo, DateTime pDateSys)

        {
            // RD 20091231 [16814] Modification of Trade included in Invoice => ne pas supprimer les Evenements

            // FI 20160907 [21831]
            Boolean isExistDel = IsDeleteEvent(pCaptureMode) || IsDeleteEventFee(pCaptureMode) || IsDeleteEventFeeUninvoiced(pCaptureMode);

            if (isExistDel)
            {

                try
                {
                    if (IsDeleteEvent(pCaptureMode))
                    {
                        lock (m_RecordLock)
                        {
                            DeleteEvent(pCS, pDbTransaction, pIdT, pCaptureSessionInfo, pDateSys);
                        }
                        DeletePosRequest(pCS, pDbTransaction, pIdT);
                    }
                    else if (IsDeleteEventFee(pCaptureMode))
                    {
                        // FI 20160907 [21831] 
                        DeleteFeeEvent(pCS, pDbTransaction, pIdT, pCaptureSessionInfo, pDateSys);
                    }
                    else if (IsDeleteEventFeeUninvoiced(pCaptureMode))
                    {
                        DeleteFeeEventUninvoiced(pCS, pDbTransaction, pIdT);
                    }
                    else
                    {
                        throw new NotImplementedException("Not NotImplemented condition else");
                    }
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.DELETE_ERROR);
                }

            }
        }

        /// <summary>
        ///  Procédure d'alimentation de la table ENTITYMARKET
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessSaveENTITYMARKET(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification, CaptureSessionInfo pCaptureSessionInfo, DateTime pDtSys)
        {
            // FI/PM 20130205 [] En mode modification Spheres® alimente ENTITYMARKET si nécessaire 
            // cas rencontré
            // importation d'un trade incomplet puis complété dans le cadre d'une modification de trade
            // Il est décidé de conserver false == IsInputIncompleteAllow(pCaptureMode) au  cas où la date de compensation ne serait pas renseignée
            if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                (Cst.Capture.IsModeNewCapture(pCaptureMode) || Cst.Capture.IsModeUpdate(pCaptureMode)))
            {
                try
                {
                    SaveEntityMarket(pCS, pDbTransaction, pTradeIdentification, pCaptureSessionInfo.session.IdA, pDtSys);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.ADD_ENTITYMARKET);
                }

            }
        }

        /// <summary>
        ///  Procédure de contôle des validation Rule
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pCheckMode"></param>
        /// FI 20140206 [19564] add Method
        /// FI 20160517 [22148] Modify
        // EG 20171115 [23509] Add CaptureSessionInfo pSessionInfo
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void ProcessCheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo)
        {
            //20100311 PL-StatusBusiness
            //if ((false == TradeCommonInput.IsStEnvironment_Template) && (false == TradeCommonInput.IsStEnvironment_PreTrade) &&
            //if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
            //    (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))

            // FI 20160517 [22148] ValidationRules appliquées si mode Remove
            if (false == IsInputIncompleteAllow(pCaptureMode))
            {
                try
                {
                    CheckValidationRuleBase.CheckModeEnum checkMode = CheckValidationRuleBase.CheckModeEnum.Error;
                    if (false == RecordSettings.isCheckValidationRules)
                        checkMode = CheckValidationRuleBase.CheckModeEnum.ErrorCritical;

                    // FI 20140326 [19793] la transaction n'est pas tjs passée 
                    CheckValidationRule(pCS, RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction, pCaptureMode, checkMode, pSessionInfo.user);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.VALIDATIONRULE_ERROR); }

            }
        }

        /// <summary>
        /// Procédure de validation XSD
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessCheckConformity(Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo)
        {
            if (RecordSettings.isCheckValidationXSD)
            {
                //FI 20130424 [18601] les modes IsModeRemoveOnlyAll sont exclus 
                if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
                {
                    if (false == TradeCommonInput.SQLInstrument.IsOpen)
                    {
                        if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                            (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
                        {
                            try
                            {
                                CheckXMLConformity(pSessionInfo);
                            }
                            catch (Exception ex)
                            {
                                throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.XMLDOCUMENT_NOTCONFORM);

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Procédure de contrôle de la licence
        /// </summary>
        /// <param name="pSessionInfo"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessCheckLicensee(CaptureSessionInfo pSessionInfo)
        {
            if (RecordSettings.isCheckLicense)
            {
                try
                {
                    if (false == pSessionInfo.licence.IsLicProductAuthorised_Add(TradeCommonInput.SQLProduct.Identifier))
                        throw new TradeCommonCaptureGenException("CheckAndRecord", string.Empty, ErrorLevel.LICENSEE_ERROR);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LICENSEE_ERROR);

                }
            }
        }


        /// <summary>
        /// Procédure de mise en place d'un lock sur TRADE
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pScope"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        // FI 20140206 [19564] add Method
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20181127 Post RTM Passage à nouveau de pDbTransaction sur LockTrade
        protected LockObject ProcessLockTrade(string pCS, IDbTransaction pDbTransaction,
                                              Cst.FeeScopeEnum pScope, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo,
                                              SpheresIdentification pTradeIdentification)
        {
            LockObject ret = null;
            if (Cst.Capture.IsModeUpdateGen(pCaptureMode) || Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                try
                {
                    ret = LockTrade(pCS, pDbTransaction, pScope, pCaptureMode, pSessionInfo, pTradeIdentification);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LOCK_ERROR);
                }
            }
            return ret;
        }

        /// <summary>
        /// Procédure de contrôle de modification simultanée d'un même trade
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessCheckRowVersion(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode)
        {

            //FI 20130424 [18601] use IsModeRemoveOnlyAll à la place de IsModeRemoveOnly
            if (Cst.Capture.IsModeUpdateGen(pCaptureMode) || Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                try
                {
                    CheckRowVersion(pCS, pDbTransaction);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.ROWVERSION_ERROR);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pIdMenu"></param>
        /// FI 20140206 [19564] add Method
        // EG 20180205 [23769] Add dbTransaction  
        protected void ProcessCheckActionTuning(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, string pIdMenu)
        {
            if (RecordSettings.isCheckActionTuning)
            {
                if (Cst.Capture.IsModeUpdateGen(pCaptureMode))
                {
                    try
                    {
                        //
                        string msg = string.Empty;
                        int idPermission = PermissionTools.GetIdPermission(CSTools.SetCacheOn(pCS), pDbTransaction, pIdMenu, PermissionTools.GetPermission(pCaptureMode));
                        if (false == TuningTools.IsActionAllowed(pCS, pDbTransaction, TradeCommonInput.IdT, TradeCommonInput.Product.IdI, idPermission, pSessionInfo.user.IdA, pSessionInfo.user.ActorAncestor, out msg))
                            throw new TradeCommonCaptureGenException("CheckAndRecord", msg, ErrorLevel.STATUS_ERROR);
                    }
                    catch (TradeCommonCaptureGenException) { throw; }
                    catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.STATUS_ERROR); }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// FI 20140206 [19564] add Method
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void ProcessCheckSpecific(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode)
        {
            //FI 20130424 [18601] les modes IsModeRemoveOnlyAll sont exclus 
            if ((false == IsInputIncompleteAllow(pCaptureMode)) && (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {
                try
                {
                    DateTime dtRefForDtEnabled = RecordSettings.dtRefForDtEnabled;
                    CheckSpecific(pCS, pDbTransaction, pCaptureMode, TradeCommonInput.IdT, dtRefForDtEnabled);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.SPECIFICCHECK_ERROR); }
            }
        }

        /// <summary>
        /// 
        /// </summary>

        /// <returns></returns>
        /// FI 20140206 [19564] add Method
        protected IDbTransaction ProcessStartDbTransaction(string pCS)
        {

            IDbTransaction ret;
            try
            {
                ret = DataHelper.BeginTran(pCS);
            }
            catch (Exception ex)
            {

                throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.BEGINTRANSACTION_ERROR);
            }
            return ret;
        }

        /// <summary>
        ///  Procédure de mise en place des valeurs default dans le trade 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessSetDefault(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode)
        {
            //PL 20130213 WARNING: Cette région "SetDefaultValue" est remontée au dessus de la région "GetIdentifier", cette dernière exploitant potentiellement les données dates.
            if ((false == IsInputIncompleteAllow(pCaptureMode)) && (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {
                try
                {
                    TradeCommonInput.SetDefaultValue(pCS, pDbTransaction);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.SETDEFAULTVALUE);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="session"></param>
        /// FI 20140206 [19564] add Method
        /// FI 20140523 [19997] le contrôle n'est plus effectué si le trade a le statut template ou missing
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void ProcessCheckValidationLock(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, AppSession session)
        {
            if (RecordSettings.isCheckValidationLock)
            {
                //FI 20140523 [19997] add if
                if (false == IsInputIncompleteAllow(pCaptureMode))
                {
                    try
                    {
                        CheckValidationLock(pCS, pDbTransaction, pCaptureMode, session);
                    }
                    catch (TradeCommonCaptureGenException) { throw; }
                    catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LOCKPROCESS_ERROR); }
                }
            }
        }

        /// <summary>
        /// Procédure de contrôle de l'asset ss-jacent
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessCheckUnderlyingAsset(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode)
        {
            if ((false == IsInputIncompleteAllow(pCaptureMode)) &&
                (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {
                try
                {
                    // FI 20140326 [19793] la transaction n'est pas tjs passée
                    CheckUnderlyingAsset(pCS, RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.CHECKUNDERLYING_ERROR); }

            }
        }

        /// <summary>
        /// Procédure d'alimentation de NOTEPAD 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessCopyNotepad(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT)
        {
            if (RecordSettings.isCopyNotePad)
            {
                // Copy Notepad from template to new Trade
                if (Cst.Capture.IsModeNew(pCaptureMode))
                {
                    try
                    {
                        CopyNotepad(pCS, pDbTransaction, TradeCommonInput.IdT, pIdT);
                    }
                    catch (Exception ex)
                    {
                        throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.ADD_NOTEPAD_ERROR);
                    }
                }
            }
        }

        /// <summary>
        /// Procédure d'alimentation de ATTACHEDDOC 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// FI 20140206 [19564] add Method
        protected void ProcessCopyAttachedDoc(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT)
        {
            if (RecordSettings.isCopyAttachedDoc)
            {
                // RD 20150323 [20824] En cas de modification, ne pas dupliquer AttchedDoc pour le même trade            
                if (Cst.Capture.IsModeNew(pCaptureMode))
                {
                    // Copy AttachedDoc from template to new Trade
                    try
                    {
                        CopyAttachedDoc(pCS, pDbTransaction, TradeCommonInput.IdT, pIdT);
                    }
                    catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.ADD_ATTACHEDDOC_ERROR); }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// <param name="oUnderlying">Retourne la liste des assets créés (null si aucun asset)</param>
        /// FI 20140206 [19564] add Method
        /// FI 20170404 [23039] Modify (Chg de signateure paramètre oUnderlying) 
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void ProcessSaveUnderlyingAsset(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, DateTime pDtSys, out Pair<int, string>[] oUnderlying)
        {
            oUnderlying = null;

            //FI 20130131 [] En mode IsInputIncompleteAllow Spheres® cherche à généré l'asset
            //Si la génaration plante Spheres® poursuit,cela est sans incidence car le trade est créé sans échéancier
            //FI 20131213 [19337] mise en place d'une transaction indépendante pour la sauvegarde su ssJacent
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                TradeCommonCaptureGenException stepExc = null;

                IDbTransaction dbTransaction2;
                if (RecordSettings.isSaveUnderlyingInParticularTransaction)
                    dbTransaction2 = DataHelper.BeginTran(pCS);
                else
                    dbTransaction2 = pDbTransaction;

                try
                {
                    SaveUnderlyingAsset(pCS, dbTransaction2, (false == RecordSettings.isSaveUnderlyingInParticularTransaction),
                        pSessionInfo, pDtSys, string.Empty, RecordSettings.isCheckLicense, out oUnderlying);

                    if (RecordSettings.isSaveUnderlyingInParticularTransaction)
                        DataHelper.CommitTran(dbTransaction2);
                }
                catch (TradeCommonCaptureGenException ex)
                {
                    stepExc = ex;
                    if (false == IsInputIncompleteAllow(pCaptureMode))
                        throw;
                }
                catch (Exception ex)
                {
                    stepExc = new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.SAVEUNDERLYING_ERROR);
                    if (false == IsInputIncompleteAllow(pCaptureMode))
                        throw stepExc;
                }
                finally
                {
                    if (null != stepExc)
                    {

                        if (RecordSettings.isSaveUnderlyingInParticularTransaction)
                            DataHelper.RollbackTran(dbTransaction2);

                        // FI 20230118 [XXXXX] Suppression des données mise en cache en cas d'exception
                        // ces données peuvent ne pas être correcte puisqu'un Rollback est exécuté
                        // Cela conserne les tables impactées par l'ajour d'un asset
                        Cst.OTCml_TBL[] table = new Cst.OTCml_TBL[] { Cst.OTCml_TBL.MATURITY, Cst.OTCml_TBL.DERIVATIVEATTRIB, Cst.OTCml_TBL.ASSET_ETD, Cst.OTCml_TBL.VW_TRADEDEBTSEC };
                        DataHelper.queryCache.Remove(ArrFunc.Map<Cst.OTCml_TBL, string>(table, (x) => { return x.ToString(); }), pCS, true);
                        DataEnabledHelper.ClearCache(pCS, table);
                    }
                }
            }
        }

        /// <summary>
        /// Procédure de calcul de l'identification du trade
        /// <para>La procédure détermine l'identifier, le displayname, la desciption et l'exttlink</para>
        /// <para>L'identification est calculé dans tous les cas sauf si annulation de trade</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <returns></returns>
        /// FI 20160503 [XXXXX] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        protected SpheresIdentification ProcessCalcTradeIdentification(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, SpheresIdentification pInitialValue)
        {
            SpheresIdentification ret = pInitialValue;

            //FI 20130424 [18601] les modes IsModeRemoveOnlyAll sont exclus 
            if (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
            {
                //20091016 FI [Rebuild identification] Get Identifier en commentaire
                //#region Get Identifier
                //GetTradeIdentifier(dbTransaction,pCaptureMode,pRecordSettings,ref identifier);
                //#endregion Get Identifier
                //20091016 FI [Rebuild identification] appel à TradeIdentificationBuilder
                //PL 20130213 WARNING: Cette région "GetIdentifier" est descendue au dessous de la région "SetDefaultValue", suite à exploitation potentielle des données dates.

                try
                {
                    // FI 20160503 lecture de cla clé CheckAndRecord_TradeIdentifier pour ne pas faire appel à getId (Test performamnce)
                    if (SystemSettings.GetAppSettings("CheckAndRecord_TradeIdentifier") == "extLink")
                    {
                        ret.Identifier = RecordSettings.extLink;
                    }
                    else
                    {
                        //PL 20130422
                        //TradeIdentificationBuilder identBuilder = new TradeIdentificationBuilder(pRecordSettings.isGetNewIdForIdentifier, TradeCommonInput, tradeIdentification);
                        TradeIdentificationBuilder identBuilder = new TradeIdentificationBuilder(RecordSettings, TradeCommonInput, ret);
                        ret = identBuilder.BuildIdentification(pCS, pDbTransaction, pCaptureMode, pSessionInfo);

                        if ((StrFunc.IsEmpty(ret.Identifier) || (ret.Identifier.ToLower() == TradeIdentificationBuilder.ConstDefault)) && identBuilder.IsXslFind)
                            throw new TradeCommonCaptureGenException("CheckAndRecord", Ressource.GetString("Msg_IdentifierNotFoundInXSL"), ErrorLevel.IDENTIFIER_NOTFOUND);
                    }

                    if (StrFunc.IsEmpty(ret.Identifier))
                        throw new Exception("Identifier is empty");

                    // RD 20100601 [17008] / Control of duplicated Security identifier
                    // FI 20120301 [18465] test l'identifier d'un trade lorsqu'il n'est pas généré par UP_GETID
                    if (RecordSettings.isGetNewIdForIdentifier == false)
                    {
                        CheckIdentifier(pCS, pCaptureMode, ret.Identifier);
                    }
                }
                catch (TradeCommonCaptureGenException) { throw; } // RD 20100601
                catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.IDENTIFIER_ERROR); }

            }

            return ret;
        }

        /// <summary>
        /// Procédure chargé du commit de la transaction 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        protected void ProcessCommitTran(IDbTransaction pDbTransaction)
        {

            try
            {
                DataHelper.CommitTran(pDbTransaction);
            }
            catch (Exception ex)
            {
                throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.COMMIT_ERROR);
            }

        }
        /// <summary>
        /// Ppd FpMLDocReader of Linked and Link to Trades
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentifier"></param>
		// RD 20161117 Add Reflect Link
        protected void ProcessTradeLink(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, string pTradeIdentifier)
        {
            // EG 20131210 New Use by CB process
            if (RecordSettings.isUpdateTradeXMLWithTradeLink)
            {
                // RD 20111024
                // Utilisation du nouvel objet TradeCommonInput.TradeLink pour lier le Trade en cours ssur d'autres Trades
                List<EFS.TradeLink.TradeLink> tradeLinkList = new List<TradeLink.TradeLink>();

                if (Cst.Capture.IsModeRemoveReplace(pCaptureMode))
                {
                    tradeLinkList.Add(
                        new TradeLink.TradeLink(0, TradeCommonInput.RemoveTrade.idTCancel, TradeLink.TradeLinkType.Replace,
                            null, null, null, null));
                }
                if (Cst.Capture.IsModeReflect(pCaptureMode))
                {
                    tradeLinkList.Add(
                        new TradeLink.TradeLink(0, TradeCommonInput.IdT, TradeLink.TradeLinkType.Reflect,
                            null, null, null, null));
                }
                //
                // RD 20120516
                // Ne pas considérer les liens TradeLinkType.ExchangeTradedDerivativeInCashBalance
                // Car un Trade Cash-Balance est potentiellement constitué de plusieurs centaines de trade Alloc, 
                // ce qui pose un problème pour Oracle: ORA-01000: maximum open cursors exceeded
                // 
                if (ArrFunc.IsFilled(TradeCommonInput.TradeLink))
                    tradeLinkList.AddRange(TradeCommonInput.TradeLink.FindAll(link => link.Link != EFS.TradeLink.TradeLinkType.ExchangeTradedDerivativeInCashBalance));
                //
                if (ArrFunc.IsFilled(tradeLinkList))
                {
                    try
                    {
                        foreach (TradeLink.TradeLink link in tradeLinkList)
                        {
                            // FI 20131210 [19320] appel à la méthode UpdateLinkedTrade
                            UpdateLinkedTrade(pCS, pDbTransaction, pTradeIdentifier, link.IdT_B, link.LinkScheme_B, out string linkedTradeIdentifier);

                            CaptureTools.SetLinkIdToPartyTradeIdentifier(TradeCommonInput.CurrentTrade.TradeHeader.PartyTradeIdentifier,
                                link.LinkScheme_A, linkedTradeIdentifier);
                        }
                    }
                    catch (Exception ex) { throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.UPD_DOCREADER_ERROR); }

                }
            }
        }


        /// <summary>
        ///  Retourne true si tous les évènements du trade existant doivent être supprimés
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        /// FI 20140206 [19564] add Method
        /// FI 20160907 [21831] Modify
        private Boolean IsDeleteEvent(Cst.Capture.ModeEnum pCaptureMode)
        {
            bool ret = Cst.Capture.IsModeUpdate(pCaptureMode);

            if (ret)
            {
                if (TradeCommonInput.IsTradeInInvoice())
                {
                    // RD 20091231 [16814]
                    // Si le trade est déjà facturé => pas de suppression des évènements
                    ret = false;
                }
                else if (IsDeleteEventFee(pCaptureMode))
                {
                    // FI 20160907 [21831] 
                    // Si Modification de frais uniquement => pas de suppression des évènements (seuls les évènements de frais seront supprimés)
                    ret = false;
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne true si seuls les évènements de frais du trade existant doivent être supprimés
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        /// FI 20160907 [21831] Add Method
        private Boolean IsDeleteEventFee(Cst.Capture.ModeEnum pCaptureMode)
        {
            return Cst.Capture.IsModeUpdate(pCaptureMode) && this.RecordSettings.isUpdateFeesOnly;
        }
        /// <summary>
        ///  Retourne true si seuls les évènements de frais non facturés 
        ///  du trade existant doivent être supprimés
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        private Boolean IsDeleteEventFeeUninvoiced(Cst.Capture.ModeEnum pCaptureMode)
        {
            return Cst.Capture.IsModeUpdateFeesUninvoiced(pCaptureMode);
        }


        /// <summary>
        /// Alimente ENTITYMARKET
        /// </summary>
        protected virtual void SaveEntityMarket(string pCS, IDbTransaction pDbTransaction, SpheresIdentification pTradeIdentification, int pIdA, DateTime pDtSys)
        {


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        protected void ProcessSaveInstrumentAndStream(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT)
        {
            // RD 20120322 / Intégration de trade "Incomplet"
            // Alimenter les tables TRADEINSTRUMENT/TRADESTREAM pour les Trades "Incomplet"
            if (((false == IsInputIncompleteAllow(pCaptureMode)) || (TradeCommonInput.TradeStatus.IsStActivation_Missing)) &&
                (false == Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode)))
            {


                try
                {
                    SaveTradeInstrumentAndStream(pCS, pDbTransaction, pIdT, pCaptureMode);
                }
                catch (Exception ex)
                {
                    throw new TradeCommonCaptureGenException("CheckAndRecord", ex, ErrorLevel.LOAD_TRADEINSTRUMENTorSTREAM_ERROR);
                }
            }
        }
        #endregion Methods
    }
}
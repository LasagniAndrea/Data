#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Common.Web.Menu;
using EFS.GUI.Interface;
using EFS.Process;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;
using FixML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
#endregion Using Directives
namespace EfsML.Business
{


    [DataContract(Name = DataHelper<AssetMinMaturityDate>.DATASETROWNAME,
        Namespace = DataHelper<AssetMinMaturityDate>.DATASETNAMESPACE)]
    internal sealed class AssetMinMaturityDate
    {

        /// <summary>
        /// Asset id
        /// </summary>
        [DataMember(Name = "ASSETID", Order = 1)]
        public int AssetId
        { get; set; }

    }

    [DataContract(Name = DataHelper<MinMaturityDate>.DATASETROWNAME,
        Namespace = DataHelper<MinMaturityDate>.DATASETNAMESPACE)]
    internal sealed class MinMaturityDate
    {

        /// <summary>
        /// Derivative contract internal id
        /// </summary>
        [DataMember(Name = "CONTRACTID", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Min Maturity date
        /// </summary>
        [DataMember(Name = "MATURITY", Order = 2)]
        public DateTime Maturity
        { get; set; }

    }


    /// <summary>
    /// Classe scellée utilisée pour les demandes de traitements sur ETD 
    /// Constitution des messages postés au service de tenue de position
    /// une demande d'action via alimentation de la table POSREQUEST 
    /// </summary>
    #region PosKeepingTools
    public sealed partial class PosKeepingTools
    {
        /// <summary>
        /// action qui impacye un trade ou une position
        /// </summary>
        public enum PosActionType
        {
            /// <summary>
            /// Action qui impacte sur la quantité attribué à un trade
            /// </summary>
            Trade,
            /// <summary>
            /// Action qui impacte sur la quantité attribué à une position
            /// </summary>
            Position
        }

        #region Constructors
        public PosKeepingTools() { }
        #endregion Constructors
        #region Methods
        #region AddPosRequestClosingDay
        /// <summary>
        /// CLOTURE DE JOURNEE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = CLOSINGDAY)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDataTable">Datatable contenant les lignes ENTITE/CSS sélectionnées pour la demande</param>
        /// <param name="pParameters">Unused</param>
        /// <param name="pIdA">Id user</param>
        /// <param name="pMsgDatas">Unused</param>
        /// <returns>Code retour + Message</returns>
        public static Cst.ErrLevelMessage AddPosRequestClosingDay(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            return AddPosRequestCommonEndOfDay(pCS, Cst.PosRequestTypeEnum.ClosingDay, pDataTable, pParameters, pMsgDatas);
        }
        #endregion AddPosRequestClosingDay
        #region AddPosRequestEndOfDay
        /// <summary>
        /// TRAITEMENT DE FIN DE JOURNEE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = EOD)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDataTable">Datatable contenant les lignes ENTITE/CSS sélectionnées pour la demande</param>
        /// <param name="pParameters">Unused</param>
        /// <param name="pIdA">Id user</param>
        /// <param name="pMsgDatas">Unused</param>
        /// <returns>Code retour + Message</returns>
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin
        public static Cst.ErrLevelMessage AddPosRequestEndOfDay(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.PosRequestTypeEnum posRequestType = Cst.PosRequestTypeEnum.EndOfDay;
            if ((null != pParameters) && (null != pParameters["ISEOD_WOIM"]) && BoolFunc.IsTrue(Convert.ToBoolean(pParameters["ISEOD_WOIM"].Value)))
                posRequestType = Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin;
            return AddPosRequestCommonEndOfDay(pCS, posRequestType, pDataTable, pParameters, pMsgDatas);
        }
        #endregion AddPosRequestEndOfDay
        #region AddPosRequestRemoveEndOfDay
        /// <summary>
        /// ANNULATION D'UN TRAITEMENT DE FIN DE JOURNEE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = REMOVEEOD)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDataTable">Datatable contenant les lignes ENTITE/CSS sélectionnées pour la demande</param>
        /// <param name="pParameters">Unused</param>
        /// <param name="pIdA">Id user</param>
        /// <param name="pMsgDatas">Unused</param>
        /// <returns>Code retour + Message</returns>
        /// EG 20130607 [18740] Add RemoveEndOfDay
        public static Cst.ErrLevelMessage AddPosRequestRemoveEndOfDay(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            return AddPosRequestCommonEndOfDay(pCS, Cst.PosRequestTypeEnum.RemoveEndOfDay, pDataTable, pParameters, pMsgDatas);
        }
        #endregion AddPosRequestRemoveEndOfDay
        #region AddPosRequestCommonEndOfDay
        /// <summary>
        /// TRAITEMENT DE FIN DE JOURNEE / CLOTURE DE JOURNEE / ANNULATION TRAITEMENT DE FIN DE JOURNEE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = EOD/CLOSINGDAY/REMOVEEOD)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pDataTable">Datatable contenant les lignes ENTITE/CSS sélectionnées pour la demande</param>
        /// <param name="pParameters">Unused</param>
        /// <param name="pMsgDatas">Unused</param>
        /// <returns>Code retour + Message</returns>
        /// EG 20150317 [POC] Test présence pIdEM (cas Marché OTC = -1) sur EOD et CLOSINGDAY
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        public static Cst.ErrLevelMessage AddPosRequestCommonEndOfDay(string pCS, Cst.PosRequestTypeEnum pRequestType, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            Cst.ErrLevelMessage finalMessage;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    IPosRequest posRequest = null;
                    IProductBase product = Tools.GetNewProductBase();
                    foreach (DataRow dr in pDataTable.Rows)
                    {
                        // Insert POSREQUEST demande de clôture/Traitemant de fin de journée pour chaque couple ENTITE/CSSCUSTODIAN sélectionné
                        int idA_Entity = Convert.ToInt32(dr["IDA_ENTITY"]);
                        string entityIdentifier = dr["ENTITY_IDENTIFIER"].ToString();
                        int idA_CssCustodian = Convert.ToInt32(dr["IDA_CSSCUSTODIAN"]);
                        string cssCustodianIdentifier = dr["CSSCUSTODIAN_IDENTIFIER"].ToString();
                        //PM 20150515 [20575] Gestion DTENTITY
                        //DateTime dtBusiness = Convert.ToDateTime(dr["DTMARKET"]);
                        DateTime dtBusiness = Convert.ToDateTime(dr["DTENTITY"]);
                        bool isCustodian = (dr["GPRODUCT"].ToString() != Cst.ProductGProduct_FUT);

                        // EG 20130531 Test Existence IDEM/IDM (sur REMOVEEOD)
                        Nullable<int> idEM = null;
                        if (dr.Table.Columns.Contains("IDEM") && (false == Convert.IsDBNull(dr["IDEM"])))
                            idEM = Convert.ToInt32(dr["IDEM"]);
                        Nullable<int> idM = null;
                        string marketIdentifier = string.Empty;
                        if (dr.Table.Columns.Contains("IDM"))
                        {
                            if (false == Convert.IsDBNull(dr["IDM"]))
                            {
                                idM = Convert.ToInt32(dr["IDM"]);
                                marketIdentifier = dr["SHORT_ACRONYM"].ToString();
                            }
                        }
                        // EG 20130527 Test Lock sur ENTITYMARKET
                        string lockMsg = string.Empty;
                        if (idEM.HasValue)
                        {
                            // EG 20151102 [21465] ObjectId = string
                            LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, idEM.Value,
                                entityIdentifier + " - " + marketIdentifier + " - " + cssCustodianIdentifier, LockTools.Exclusive);
                            lockMsg = LockTools.SearchProcessLocks(pCS, null, lockEntityMarket, SessionTools.AppSession);
                            if (StrFunc.IsFilled(lockMsg))
                            {
                                errLevel = Cst.ErrLevel.FAILURE;
                                errorMessage = lockMsg + Cst.HTMLHorizontalLine;
                            }
                        }
                        else
                        {
                            DataSet dsEntity = GetEntityMarkets(pCS, idA_Entity, idA_CssCustodian, isCustodian);
                            if ((null != dsEntity) && ArrFunc.IsFilled(dsEntity.Tables[0].Rows))
                            {
                                foreach (DataRow row in dsEntity.Tables[0].Rows)
                                {
                                    // EG 20151102 [21465] ObjectId = string
                                    LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, Convert.ToInt32(row["IDEM"]),
                                        entityIdentifier + " - " + row["SHORT_ACRONYM"].ToString() + " - " + row["CSSCUSTODIAN_IDENTIFIER"].ToString(), LockTools.Exclusive);
                                    lockMsg = LockTools.SearchProcessLocks(pCS, null, lockEntityMarket, SessionTools.AppSession);
                                    if (StrFunc.IsFilled(lockMsg))
                                    {
                                        errLevel = Cst.ErrLevel.FAILURE;
                                        errorMessage = lockMsg + Cst.HTMLHorizontalLine;
                                    }
                                }
                            }
                        }
                        if (Cst.ErrLevel.SUCCESS == errLevel)
                        {
                            switch (pRequestType)
                            {
                                // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
                                case Cst.PosRequestTypeEnum.EndOfDay:
                                case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                                    // EG 20150317 [POC] Add IdEM
                                    posRequest = product.CreatePosRequestEOD(idA_Entity, idA_CssCustodian, dtBusiness, idEM, isCustodian, pRequestType);
                                    break;
                                case Cst.PosRequestTypeEnum.ClosingDay:
                                    // EG 20150317 [POC] Add IdEM
                                    posRequest = product.CreatePosRequestClosingDAY(idA_Entity, idA_CssCustodian, dtBusiness, idEM, isCustodian);
                                    break;
                                case Cst.PosRequestTypeEnum.RemoveEndOfDay:
                                    if (idEM.HasValue)
                                    {
                                        posRequest = product.CreatePosRequestREMOVEEOD(idA_Entity, idA_CssCustodian, dtBusiness, idEM, isCustodian);
                                        posRequest.IdMSpecified = idM.HasValue;
                                        if (posRequest.IdMSpecified)
                                            posRequest.IdM = idM.Value;
                                        // EG 20150706 Cas du MTM
                                        else if (dr.Table.Columns.Contains("GPRODUCT") && (false == Convert.IsDBNull(dr["GPRODUCT"])) &&
                                                (dr["GPRODUCT"].ToString() == Cst.ProductGProduct_MTM))
                                        {
                                            posRequest.IdMSpecified = true;
                                            posRequest.IdM = -1;
                                        }
                                    }
                                    else
                                    {
                                        posRequest = product.CreatePosRequestREMOVEEOD(idA_Entity, idA_CssCustodian, dtBusiness, isCustodian);
                                    }
                                    break;
                            }

                            posRequest.SetIdentifiers(entityIdentifier, cssCustodianIdentifier, marketIdentifier);
                            posRequest.StatusSpecified = true;
                            posRequest.Status = ProcessStateTools.StatusPendingEnum;

                            if (null == pParameters["ENTITY"])
                                pParameters.Add(new MQueueparameter("ENTITY", TypeData.TypeDataEnum.integer));
                            pParameters["ENTITY"].SetValue(idA_Entity, entityIdentifier);

                            if (null == pParameters["DTBUSINESS"])
                                pParameters.Add(new MQueueparameter("DTBUSINESS", TypeData.TypeDataEnum.date));
                            pParameters["DTBUSINESS"].SetValue(dtBusiness);

                            if (null == pParameters["CSSCUSTODIAN"])
                                pParameters.Add(new MQueueparameter("CSSCUSTODIAN", TypeData.TypeDataEnum.integer));
                            pParameters["CSSCUSTODIAN"].SetValue(idA_CssCustodian, cssCustodianIdentifier);

                            if (idEM.HasValue)
                            {
                                if (null == pParameters["ENTITYMARKET"])
                                    pParameters.Add(new MQueueparameter("ENTITYMARKET", TypeData.TypeDataEnum.integer));
                                pParameters["ENTITYMARKET"].SetValue(idEM.Value);
                            }

                            if (idM.HasValue)
                            {
                                if (null == pParameters["MARKET"])
                                    pParameters.Add(new MQueueparameter("MARKET", TypeData.TypeDataEnum.integer));
                                pParameters["MARKET"].SetValue(idM.Value, marketIdentifier);
                            }


                            Cst.ErrLevelMessage _errMessage = AddPosRequest(pCS, null, posRequest, SessionTools.AppSession, pParameters, dr);
                            if (Cst.ErrLevel.SUCCESS != _errMessage.ErrLevel)
                            {
                                errLevel = Cst.ErrLevel.FAILURE;
                                returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                                errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                // PL 20220614 Add error message 
                                errorMessage += _errMessage.Message;
                            }
                            else
                            {
                                succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                            }
                        }
                    }
                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                returnMessage = Ressource.GetString2(returnMessage, pDataTable.Rows.Count.ToString(), errorMessage + succesMessage);
                // PL 20220614 Add error message on Failure
                if (errLevel == Cst.ErrLevel.FAILURE)
                    returnMessage += Cst.HTMLHorizontalLine + errorMessage;

                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }

        #endregion AddPosRequestCommonEndOfDay
        #region AddPosRequestUnclearing
        /// <summary>
        /// TRAITEMENT DE DECOMPENSATION UNITAIRE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = UNCLEARING)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDataTable">Datatable contenant les lignes clôturées sélectionnées pour la demande de decompensation</param>
        /// <param name="pIdA">Id User</param>
        /// <returns>Code retour + Message</returns>
        /// FI 20130318 [18467] use  SessionTools.NewAppInstance()
        // EG 20180307 [23769] Gestion dbTransaction
        public static Cst.ErrLevelMessage AddPosRequestUnclearing(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            int nbMessageSendToService = 0;
            Cst.ErrLevelMessage finalMessage;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    IPosRequest posRequest = null;
                    IProductBase pProduct = Tools.GetNewProductBase();
                    nbMessageSendToService = pDataTable.Rows.Count;
                    foreach (DataRow dr in pDataTable.Rows)
                    {
                        int idT = Convert.ToInt32(dr["IDT"]);
                        string tradeIdentifier = Convert.ToString(dr["TRADE_IDENTIFIER"]);
                        // EG 20151102 [21465] ObjectId = string
                        Lock lckExisting = LockTools.SearchLock(pCS, null, new LockObject(TypeLockEnum.TRADE, idT, tradeIdentifier, LockTools.Exclusive));
                        if (null == lckExisting)
                        {
                            posRequest = SetPosRequestUnclearing(pProduct, dr);
                            if (null != posRequest)
                            {
                                // Insert POSREQUEST
                                posRequest.StatusSpecified = true;
                                posRequest.Status = ProcessStateTools.StatusPendingEnum;
                                posRequest.IdPR_PosRequestSpecified = true;
                                posRequest.IdPR_PosRequest = ((IPosRequestDetUnclearing)posRequest.DetailBase).IdPR;
                                Cst.ErrLevelMessage _errMessage = AddPosRequest(pCS, null, posRequest, SessionTools.AppSession, pParameters, dr);
                                if (Cst.ErrLevel.SUCCESS != _errMessage.ErrLevel)
                                {
                                    errLevel = Cst.ErrLevel.FAILURE;
                                    returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                                    returnMessage += _errMessage.Message + Cst.CrLf;
                                    errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                }
                                else
                                {
                                    succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                }
                            }
                            else
                                nbMessageSendToService--;
                        }
                        else
                            errorMessage += lckExisting.ToString() + Cst.HTMLHorizontalLine;
                    }
                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                returnMessage = Ressource.GetString2(returnMessage, nbMessageSendToService.ToString(), errorMessage + succesMessage);
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }
        #endregion AddPosRequestUnclearing
        #region AddPosRequestUpdateEntry
        /// <summary>
        /// TRAITEMENT DE MISE A JOUR DES CLOTURES
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = UPDENTRY)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pDataTable">Datatable contenant les séries (clé de position) nécessitant une mise à jour de leurs positions</param>
        /// <param name="pIdA">Id User</param>
        /// <returns>Code retour + Message</returns>
        /// FI 20130318 [18467] use SessionTools.NewAppInstance()
        public static Cst.ErrLevelMessage AddPosRequestUpdateEntry(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            Cst.ErrLevelMessage finalMessage;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    IPosRequest posRequest = null;
                    IProductBase pProduct = Tools.GetNewProductBase();
                    foreach (DataRow dr in pDataTable.Rows)
                    {
                        // Insert POSREQUEST
                        posRequest = SetPosRequestUpdateEntry(pProduct, SettlSessIDEnum.Intraday, dr);
                        posRequest.StatusSpecified = true;
                        posRequest.Status = ProcessStateTools.StatusPendingEnum;
                        Cst.ErrLevelMessage _errMessage = AddPosRequest(pCS, null, posRequest, SessionTools.AppSession, pParameters, dr);
                        if (Cst.ErrLevel.SUCCESS != _errMessage.ErrLevel)
                        {
                            errLevel = Cst.ErrLevel.FAILURE;
                            returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                            errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                        }
                        else
                        {
                            succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                        }
                    }
                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                returnMessage = Ressource.GetString2(returnMessage, pDataTable.Rows.Count.ToString(), errorMessage + succesMessage);
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }
        #endregion AddPosRequestUpdateEntry
        #region AddPosKeepingKeyParameters
        /// <summary>
        /// Setting des paramètres utilisés dans une requête d'insertion dans la table POSREQUEST
        /// <para>───────────────────────────────────────────────────────────────────────────────</para>
        /// <para >● Alimentation des paramètres optionnels en fonction des caractèristiques </para>
        /// <para >de la demande (IDT ou clé de position...).</para>
        /// <para >● Sérialisation des caractéristiques du détail de POSREQUEST lorsqu'il existe.</para>
        /// <para >Appelée par la méthode AddPosRequest</para>
        /// <para>───────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pParameters">Collection de DataParameters</param>
        /// <param name="pPosRequest">Demande de traitement de type PosRequest</param>
        /// FI 201303 [18467] les ccolonnes idA_Dealer et idB_Dealer peuvznt être à zéro
        /// Cela concerne notamment les assignations
        /// EG 20130607 [18740] Add RemoveCAExecuted
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        private static Cst.ErrLevel AddPosKeepingKeyParameters(string pCS, DataParameters pParameters, IPosRequest pPosRequest)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            Nullable<int> idI = null;
            Nullable<int> idAsset = null;
            Nullable<int> idA_Dealer = null;
            Nullable<int> idB_Dealer = null;
            Nullable<int> idA_Clearer = null;
            Nullable<int> idB_Clearer = null;
            Nullable<int> idT = null;
            // EG 20170127 Qty Long To Decimal
            Nullable<decimal> qty = null;
            string detail = string.Empty;

            #region Elements de clé de position
            if (pPosRequest.PosKeepingKeySpecified)
            {
                idI = pPosRequest.PosKeepingKey.IdI;
                idAsset = pPosRequest.PosKeepingKey.IdAsset;
                if (pPosRequest.PosKeepingKey.IdA_Dealer > 0)
                    idA_Dealer = pPosRequest.PosKeepingKey.IdA_Dealer;
                if (pPosRequest.PosKeepingKey.IdB_Dealer > 0)
                    idB_Dealer = pPosRequest.PosKeepingKey.IdB_Dealer;
                idA_Clearer = pPosRequest.PosKeepingKey.IdA_Clearer;
                if (pPosRequest.PosKeepingKey.IdB_Clearer > 0)
                    idB_Clearer = pPosRequest.PosKeepingKey.IdB_Clearer;
                if (pPosRequest.IdTSpecified)
                    idT = pPosRequest.IdT;
            }
            else if (pPosRequest.IdTSpecified)
            {
                idT = pPosRequest.IdT;
            }
            /// EG 20130607 [18740] Add RemoveCAExecuted
            else if ((pPosRequest.RequestType != Cst.PosRequestTypeEnum.EndOfDay) &&
                     (pPosRequest.RequestType != Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin) &&
                     (null == pPosRequest.GetType().GetInterface("IPosRequestGroupLevel")) &&
                     (pPosRequest.RequestType != Cst.PosRequestTypeEnum.RemoveEndOfDay) &&
                     (pPosRequest.RequestType != Cst.PosRequestTypeEnum.RemoveCAExecuted) &&
                     (pPosRequest.RequestType != Cst.PosRequestTypeEnum.ClosingDay))
            {
                errLevel = Cst.ErrLevel.DATAREJECTED;
            }
            #endregion Elements de clé de position

            #region Qty
            if (pPosRequest.QtySpecified)
                qty = pPosRequest.Qty;
            #endregion Qty

            #region Serialisation du Detail (XML)
            if (null != pPosRequest.DetailBase)
                detail = SerializePosRequestDetail(pPosRequest);
            #endregion Serialisation du Detail (XML)


            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                pParameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pPosRequest.DtBusiness);
                pParameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), idI ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), idAsset ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), idA_Dealer ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), idB_Dealer ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), idA_Clearer ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), idB_Clearer ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), idT ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "QTY", DbType.Decimal), qty ?? Convert.DBNull);
                pParameters.Add(new DataParameter(pCS, "REQUESTDETAIL", DbType.Xml), StrFunc.IsFilled(detail) ? detail : Convert.DBNull);
            }
            return errLevel;
        }
        #endregion AddPosKeepingKeyParameters
        #region AddPosRequest
        /// <summary>
        /// INSERTION / MISE A JOUR DE LA TABLE POSREQUEST 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Ecriture de la demande dans la table POSREQUEST ou </para> 
        /// <para>  mise à jour si demande déjà insérée (voir GetExistingIDPosRequest)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (REQUESTMODE = INTRADAY)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pdbTransaction">Transaction</param>
        /// <param name="pPosRequest">Représente la demande</param>
        /// <param name="pSession">Application /Id User</param>
        /// <returns>Code retour + Message</returns>
        /// EG 20141230 (20587]
        /// EG 20150317 [POC] use pPosRequest.GProduct
        /// EG 20170223 use pPosRequest.groupProductValue
        public static Cst.ErrLevelMessage AddPosRequest(string pCS, IDbTransaction pdbTransaction, IPosRequest pPosRequest, AppSession pSession,
            MQueueparameters pParameters, DataRow pDr)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevelMessage errMessage;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            try
            {
                errLevel = FillPosRequest(pCS, pdbTransaction, pPosRequest, pSession);
                if (Cst.ErrLevel.SUCCESS == errLevel)
                {
                    string requestMessage = pPosRequest.RequestMessage;
                    if (pPosRequest.RequestMode == SettlSessIDEnum.Intraday)
                    {
                        //string _gProduct = GetFungibiliyProduct(pDr);
                        PosKeepingRequestMQueue requestMQueue = BuildPosKeepingRequestMQueue(pCS, pPosRequest, pParameters);
                        // EG 20141230 (20587] New parameter = No ThreadPool for SendMessage
                        // EG 20150317 [POC] use pPosRequest.GProduct
                        // PL 20220614 En cas d'anomalie (ex. FilWatcher unavailable) SendMessage génère maintenant une erreur.
                        errLevel = SendMessage(pCS, requestMQueue, pDr, pPosRequest.GroupProductValue);
                    }
                    else
                    {
                        requestMessage += Cst.HTMLHorizontalLine + Ressource.GetString2("Msg_PROCESS_REQUESTMODE", pPosRequest.RequestMode.ToString());
                    }
                    returnMessage = Ressource.GetString2(returnMessage, "1", requestMessage);
                }
            }
            catch (Exception ex)
            {
                errLevel = Cst.ErrLevel.FAILURE;
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage += ExceptionTools.GetMessageExtended(ex);
            }
            finally
            {
                errMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            //
            return errMessage;
        }
        #endregion AddPosRequest
        #region AdditionalPosRequestParameters
        // EG 20150716 [21103] Add Safekeeping
        private static void AdditionalPosRequestParameters(IPosRequest pPosRequest, MQueueparameters pQueueParameters)
        {
            if (null == pQueueParameters["DTBUSINESS"])
                pQueueParameters.Add(new MQueueparameter("DTBUSINESS", TypeData.TypeDataEnum.date));

            pQueueParameters["DTBUSINESS"].SetValue(pPosRequest.DtBusiness);
            if (pPosRequest.IdTSpecified)
            {
                if (null == pQueueParameters["IDT"])
                    pQueueParameters.Add(new MQueueparameter("IDT", TypeData.TypeDataEnum.integer));
                pQueueParameters["IDT"].SetValue(pPosRequest.IdT);
                if (null == pQueueParameters["TRADE_IDENTIFIER"])
                    pQueueParameters.Add(new MQueueparameter("TRADE_IDENTIFIER", TypeData.TypeDataEnum.@string));
                if (pPosRequest.IdentifiersSpecified)
                    pQueueParameters["TRADE_IDENTIFIER"].SetValue(pPosRequest.Identifiers.Trade);
            }
            if (pPosRequest.QtySpecified)
            {
                // EG 20170127 Qty Long To Decimal
                if (null == pQueueParameters["QTY"])
                    pQueueParameters.Add(new MQueueparameter("QTY", TypeData.TypeDataEnum.@decimal));
                pQueueParameters["QTY"].SetValue(pPosRequest.Qty);
            }
            Type tPosRequest = pPosRequest.GetType();
            if (tPosRequest.Equals(typeof(PosRequestOption)))
            {
                IPosRequestOption option = (IPosRequestOption)pPosRequest;
                if ((Cst.PosRequestTypeEnum.OptionAssignment == option.RequestType) ||
                    (Cst.PosRequestTypeEnum.OptionExercise == option.RequestType))
                {
                    if (null == pQueueParameters["FEES"])
                        pQueueParameters.Add(new MQueueparameter("FEES", TypeData.TypeDataEnum.@bool));
                    pQueueParameters["FEES"].SetValue(option.Detail.PaymentFeesSpecified);
                }
            }
            else if (tPosRequest.Equals(typeof(PosRequestCorrection)))
            {
                IPosRequestCorrection correction = (IPosRequestCorrection)pPosRequest;
                if (null == pQueueParameters["FEES"])
                    pQueueParameters.Add(new MQueueparameter("FEES", TypeData.TypeDataEnum.@bool));
                pQueueParameters["FEES"].SetValue(correction.Detail.PaymentFeesSpecified);
                if (null == pQueueParameters["SAFEKEEPING"])
                    pQueueParameters.Add(new MQueueparameter("SAFEKEEPING", TypeData.TypeDataEnum.@bool));
                // EG 20150716 [21103]
                pQueueParameters["SAFEKEEPING"].SetValue(correction.Detail.IsReversalSafekeepingSpecified && correction.Detail.IsReversalSafekeeping);
            }
            else if (tPosRequest.Equals(typeof(PosRequestTransfer)))
            {
                IPosRequestTransfer transfer = (IPosRequestTransfer)pPosRequest;
                if (transfer.Detail.IdTReplaceSpecified)
                {
                    #region Transfert de position unitaire
                    if (null == pQueueParameters["TRADETARGET"])
                        pQueueParameters.Add(new MQueueparameter("TRADETARGET", TypeData.TypeDataEnum.integer));
                    if (transfer.Detail.Trade_IdentifierReplaceSpecified)
                        pQueueParameters["TRADETARGET"].SetValue(transfer.Detail.IdTReplace, transfer.Detail.Trade_IdentifierReplace);
                    else
                        pQueueParameters["TRADETARGET"].SetValue(transfer.Detail.IdTReplace, "N/A");

                    if (null == pQueueParameters["FEES"])
                        pQueueParameters.Add(new MQueueparameter("FEES", TypeData.TypeDataEnum.@bool));
                    pQueueParameters["FEES"].SetValue(transfer.Detail.PaymentFeesSpecified);
                    #endregion Transfert de position unitaire
                }
                else
                {
                    #region Transfert de masse
                    if (null == pQueueParameters["ISREVERSALFEES"])
                        pQueueParameters.Add(new MQueueparameter("ISREVERSALFEES", TypeData.TypeDataEnum.@bool));
                    pQueueParameters["ISREVERSALFEES"].SetValue(transfer.Detail.IsReversalFees);
                    if (null == pQueueParameters["ISCALCNEWFEES"])
                        pQueueParameters.Add(new MQueueparameter("ISCALCNEWFEES", TypeData.TypeDataEnum.@bool));
                    pQueueParameters["ISCALCNEWFEES"].SetValue(transfer.Detail.IsCalcNewFees);
                    #endregion Transfert de masse
                }
                // EG 20150716 [21103]
                if (null == pQueueParameters["ISREVERSALSAFEKEEPING"])
                    pQueueParameters.Add(new MQueueparameter("ISREVERSALSAFEKEEPING", TypeData.TypeDataEnum.@bool));
                pQueueParameters["ISREVERSALSAFEKEEPING"].SetValue(transfer.Detail.IsReversalSafekeepingSpecified && transfer.Detail.IsReversalSafekeeping);


                if (transfer.Detail.DealerTargetSpecified)
                {
                    if (null == pQueueParameters["DEALER"])
                        pQueueParameters.Add(new MQueueparameter("DEALER", TypeData.TypeDataEnum.@string));
                    pQueueParameters["DEALER"].SetValue(transfer.Detail.DealerTarget.PartyId);
                }
                if (transfer.Detail.DealerBookIdTargetSpecified)
                {
                    if (null == pQueueParameters["BOOKDEALER"])
                        pQueueParameters.Add(new MQueueparameter("BOOKDEALER", TypeData.TypeDataEnum.@string));
                    pQueueParameters["BOOKDEALER"].SetValue(transfer.Detail.DealerBookIdTarget.Value);
                }

                if (transfer.Detail.ClearerTargetSpecified)
                {
                    if (null == pQueueParameters["CLEARER"])
                        pQueueParameters.Add(new MQueueparameter("CLEARER", TypeData.TypeDataEnum.@string));
                    pQueueParameters["CLEARER"].SetValue(transfer.Detail.ClearerTarget.PartyId);
                }
                if (transfer.Detail.ClearerBookIdTargetSpecified)
                {
                    if (null == pQueueParameters["BOOKCLEARER"])
                        pQueueParameters.Add(new MQueueparameter("BOOKCLEARER", TypeData.TypeDataEnum.@string));
                    pQueueParameters["BOOKCLEARER"].SetValue(transfer.Detail.ClearerBookIdTarget.Value);
                }

            }
        }
        #endregion AdditionalPosRequestParameters
        #region AddPosRequestClearing
        /// <summary>
        /// TRAITEMENT DE COMPENSATION 
        /// appelée pour traiter la compensation globale (BULK) ou automatique (EOD)
        /// <para>────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = CLEARBULK/CLEAREOD)</para> 
        /// <para>  Pour chaque ligne sélectionnée (caractérisée par une clé de position)</para> 
        /// <para>  avec position acheteuse, position vendeuse et la quantité à compenser</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pDataTable">Datatable contenant les séries (clé de position) sélectionnées pour compensation globale</param>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pParameters">Unused</param>
        /// <param name="pIdA">Id User</param>
        /// <returns>Code retour + Message</returns>
        /// FI 20130318 [] use SessionTools.NewAppInstance()
        private static Cst.ErrLevelMessage AddPosRequestClearing(string pCS, DataTable pDataTable, Cst.PosRequestTypeEnum pRequestType, MQueueparameters pParameters)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            int nbMessageSendToService = 0;
            Cst.ErrLevelMessage finalMessage;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    IPosRequest posRequest = null;
                    IProductBase product = Tools.GetNewProductBase();
                    nbMessageSendToService = pDataTable.Rows.Count;
                    foreach (DataRow dr in pDataTable.Rows)
                    {
                        // Insert POSREQUEST
                        posRequest = SetPosRequestClearing(product, pRequestType, SettlSessIDEnum.Intraday, dr);
                        if (null != posRequest)
                        {
                            posRequest.StatusSpecified = true;
                            posRequest.Status = ProcessStateTools.StatusPendingEnum;
                            Cst.ErrLevelMessage _errMessage = AddPosRequest(pCS, null, posRequest, SessionTools.AppSession, pParameters, dr);
                            if (Cst.ErrLevel.SUCCESS != _errMessage.ErrLevel)
                            {
                                errLevel = Cst.ErrLevel.FAILURE;
                                returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                                errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                            }
                            else
                            {
                                succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                            }
                        }
                        else
                            nbMessageSendToService--;
                    }
                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                returnMessage = Ressource.GetString2(returnMessage, nbMessageSendToService.ToString(), errorMessage + succesMessage);
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }
        #endregion AddPosRequestClearing
        #region AddPosRequestClearingBLK
        /// <summary>
        /// TRAITEMENT DE COMPENSATION GLOBALE (MANUELLE)
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = CLEARBULK)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pDataTable">Datatable contenant les séries (clé de position) sélectionnées pour compensation globale</param>
        /// <param name="pIdA">Id User</param>
        /// <returns>Code retour + Message</returns>
        public static Cst.ErrLevelMessage AddPosRequestClearingBLK(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            return AddPosRequestClearing(pCS, pDataTable, Cst.PosRequestTypeEnum.ClearingBulk, pParameters);
        }
        #endregion AddPosRequestClearingBLK
        #region AddPosRequestClearingSPEC
        /// <summary>
        /// TRAITEMENT DE CLOTURE SPECIFIQUE UNITAIRE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = CLEARSPEC)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pDataTable">Datatable contenant les trades sélectionnés pour clôture spécifique face au trade source</param>
        /// <param name="pIdA">Id User</param>
        /// <returns>Code retour + Message</returns>
        /// EG 20150907 [21317] Refactoring for EquitySecurityTransaction|DebtSecurityTransaction
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        // RD 20170315 [22967] Modify
        // EG 20171025 [23509] Upd dtExecution replace dtTimestamp
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public static Cst.ErrLevelMessage AddPosRequestClearingSPEC(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            ArrayList aTradesTarget = new ArrayList();
            IDbTransaction dbTransaction = null;
            Cst.ErrLevelMessage finalMessage;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    IPosRequest posRequest = null;
                    IProductBase product = Tools.GetNewProductBase();
                    int nbMessageSendToService = pDataTable.Rows.Count;
                    int idTSource = Convert.ToInt32(pParameters["IDT"].Value);
                    string identifier = pParameters["IDENTIFIER"].Value;

                    // EG 20150907 [21317] 
                    //int idA_Css = Convert.ToInt32(pParameters["CLEARINGHOUSE"].Value);
                    int idA_CssCustodian = Convert.ToInt32(pParameters["CSSCUSTODIAN"].Value);
                    Nullable<int> idA_Custodian = null;
                    if (pDataTable.Columns.Contains("ISCUSTODIAN") && Convert.ToBoolean(pDataTable.Rows[0]["ISCUSTODIAN"]))
                        idA_Custodian = idA_CssCustodian;

                    // EG 20150920 [21314] 
                    // EG 20170127 Qty Long To Decimal
                    decimal sourceQty = Convert.ToDecimal(pParameters["AVAILABLEQTY"].Value);
                    int ida_Entity = Convert.ToInt32(pParameters["ENTITY"].Value);
                    DateTime dtBusiness = Convert.ToDateTime(pParameters["DTBUSINESS"].Value);
                    // EG 20121127 Change IDA_CSS to IDM
                    int idM = Convert.ToInt32(pParameters["MARKET"].Value);
                    SQL_EntityMarket entityMarket = new SQL_EntityMarket(pCS, null, ida_Entity, idM, idA_Custodian);
                    int idEM = entityMarket.IdEM;
                    // EG 20150920 [21314] 
                    // EG 20170127 Qty Long To Decimal
                    decimal totalClosableQty = 0;
                    IPosKeepingClearingTrade tradeTarget = null;
                    foreach (DataRow dr in pDataTable.Rows)
                    {
                        // EG 20150920 [21314]
                        // EG 20170127 Qty Long To Decimal
                        decimal qtyTarget = Convert.ToDecimal(dr["AVAILABLEQTY"]);
                        int idTTarget = Convert.ToInt32(dr["IDT"]);
                        // EG 20150920 [21314]
                        decimal closableQty = Convert.ToDecimal(dr["CLOSABLEQTY"]);
                        string identifierTarget = dr["TRADE_IDENTIFIER"].ToString();
                        if (0 < closableQty)
                        {
                            if (closableQty > sourceQty)
                            {
                                errLevel = Cst.ErrLevel.FAILURE;
                                returnMessage = Ressource.GetString2("Msg_ClosingSpecErrQtyTarget", identifierTarget,
                                    closableQty.ToString(), identifier, sourceQty.ToString()) + Cst.CrLf;
                                break;
                            }
                            totalClosableQty += closableQty;
                            if (totalClosableQty > sourceQty)
                            {
                                errLevel = Cst.ErrLevel.FAILURE;
                                returnMessage = Ressource.GetString2("Msg_ClosingSpecErrTotalQty",
                                    totalClosableQty.ToString(), identifier, sourceQty.ToString()) + Cst.CrLf;
                                break;
                            }
                            tradeTarget = product.CreatePosKeepingClearingTrade();
                            tradeTarget.IdT = Convert.ToInt32(dr["IDT"]);
                            tradeTarget.Identifier = dr["TRADE_IDENTIFIER"].ToString();
                            // EG 20150920 [21314] 
                            // EG 20170127 Qty Long To Decimal
                            tradeTarget.AvailableQty = Convert.ToDecimal(dr["AVAILABLEQTY"]);
                            tradeTarget.ClosableQty = closableQty;
                            tradeTarget.DtBusiness = Convert.ToDateTime(dr["DTBUSINESS"]);
                            tradeTarget.DtExecution = Convert.ToDateTime(dr["DTEXECUTION"]);
                            aTradesTarget.Add(tradeTarget);
                        }
                    }
                    // EG 20130305 Add Test aTradesTarget.Count
                    if ((Cst.ErrLevel.SUCCESS == errLevel) && (0 < aTradesTarget.Count))
                    {
                        dbTransaction = DataHelper.BeginTran(pCS);
                        IPosKeepingClearingTrade[] tradesTarget = (IPosKeepingClearingTrade[])aTradesTarget.ToArray(typeof(IPosKeepingClearingTrade));
                        posRequest = product.CreatePosRequestClearingSPEC(SettlSessIDEnum.Intraday, dtBusiness,
                            idTSource, totalClosableQty, tradesTarget);

                        if (null != posRequest)
                        {
                            posRequest.StatusSpecified = true;
                            posRequest.Status = ProcessStateTools.StatusPendingEnum;
                            posRequest.IdentifiersSpecified = true;
                            posRequest.Identifiers = product.CreatePosRequestKeyIdentifier();
                            posRequest.Identifiers.Trade = identifier;
                            posRequest.IdA_Entity = ida_Entity;
                            // EG 20150907 [21317] Refactoring
                            posRequest.IdA_CssCustodian = idA_CssCustodian;
                            posRequest.IdA_CustodianSpecified = idA_Custodian.HasValue;
                            if (posRequest.IdA_CustodianSpecified)
                                posRequest.IdA_Custodian = idA_Custodian.Value;
                            posRequest.IdEMSpecified = true;
                            posRequest.IdEM = entityMarket.IdEM;
                            // RD 20170315 [22967] Valoriser "posRequest.groupProduct"
                            posRequest.GroupProductSpecified = pDataTable.Columns.Contains("GPRODUCT");
                            if (posRequest.GroupProductSpecified)
                                posRequest.GroupProduct = (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), pDataTable.Rows[0]["GPRODUCT"].ToString());

                            // All trades are RESERVED
                            //
                            AppSession session = SessionTools.AppSession;
                            // FI 20200820 [25468] Dates systemes en UTC
                            DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);
                            SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(pCS, idTSource);
                            sqlTradeStSys.UpdateTradeStUsedBy(dbTransaction, session.IdA, Cst.StatusUsedBy.RESERVED, Cst.ProcessTypeEnum.CLOSINGGEN.ToString(), dtSys);
                            foreach (IPosKeepingClearingTrade item in tradesTarget)
                            {
                                sqlTradeStSys = new SQL_TradeStSys(pCS, item.IdT);
                                sqlTradeStSys.UpdateTradeStUsedBy(dbTransaction, session.IdA, Cst.StatusUsedBy.RESERVED, Cst.ProcessTypeEnum.CLOSINGGEN.ToString(), dtSys);
                            }
                            // EG 20170127 Qty Long To Decimal
                            pParameters.Add(new MQueueparameter("QTY", TypeData.TypeDataEnum.dec));
                            pParameters["QTY"].SetValue(totalClosableQty);

                            Cst.ErrLevelMessage _errMessage = AddPosRequest(pCS, dbTransaction, posRequest, session, pParameters, null);
                            if (Cst.ErrLevel.SUCCESS != _errMessage.ErrLevel)
                            {
                                errLevel = Cst.ErrLevel.FAILURE;
                                returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                                errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                            }
                            else
                            {
                                succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                            }
                        }
                        else
                            returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                    }
                }
                else
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
            }
            catch (Exception ex)
            {
                errLevel = Cst.ErrLevel.FAILURE;
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
            }
            finally
            {
                returnMessage = Ressource.GetString2(returnMessage, "1", errorMessage + succesMessage);
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
                if (null != dbTransaction)
                {
                    if (Cst.ErrLevel.FAILURE == errLevel)
                        DataHelper.RollbackTran(dbTransaction);
                    else
                        DataHelper.CommitTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }
            return finalMessage;
        }
        #endregion AddPosRequestClearingSPEC
        #region AddPosRequestPositionTransferBLK
        /// <summary>
        /// TRAITEMENT DE TRANSFERT DE POSITION GLOBAL 
        /// appelée pour traiter le transfert de position global (BULK) 
        /// <para>────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = POT)</para> 
        /// <para>  Pour chaque ligne sélectionnée (caractérisée par une clé de position)</para> 
        /// <para>  avec quantité initiale, quantité dispo et quantité à transférer</para> 
        /// <para>  et ACtor/Book destinataire</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pDataTable">Datatable contenant les séries (clé de position) sélectionnées pour transfert global</param>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pParameters">Unused</param>
        /// <param name="pIdA">Id User</param>
        /// <returns>Code retour + Message</returns>
        /// <param name="pCS"></param>
        /// <param name="pDataTable"></param>
        /// <param name="pParameters"></param>
        /// <param name="pMsgDatas"></param>
        /// <returns></returns>
        /// EG 20141210 [20554] New : Envoi des messages de transfert de masse avec 1 ligne de tracker par type (OTC | ETD)
        /// // EG 20150716 [21103] Add SafeKeeping reversal
        // EG 20170315 [22967] Gestion de la colonne GPRODUCT + une seule liste listMQueue remplace listMQueue_OTC et listMQueue_ETD
        public static Cst.ErrLevelMessage AddPosRequestPositionTransferBLK(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevelMessage finalMessage = null;
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            // EG 20170315 [22967]
            //List<MQueueBase> listMQueue_OTC = new List<MQueueBase>();
            //List<MQueueBase> listMQueue_ETD = new List<MQueueBase>();
            List<MQueueBase> listMQueue = new List<MQueueBase>();
            int nbMessageSendToService = 0;
            int nbMessageNotSendToService = 0;

            string headerMessage = string.Empty;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    #region Contrôles des nouveaux acteurs (DEALER|CLEARER) et paramètres pour frais (EXTPOURNE|RECALCUL)
                    Pair<IParty, IBookId> transferDealer = null;
                    Pair<IParty, IBookId> transferClearer = null;
                    Pair<bool, bool> transferInfo = null;
                    // EG 20150716 [21103]
                    EFS_Boolean isReversalSafekeeping = null;
                    int idA_Entity = Convert.ToInt32(pParameters["ENTITY"].Value);
                    finalMessage = ControlForTransferBulk(pCS, idA_Entity, pMsgDatas, ref transferDealer, ref transferClearer, ref transferInfo, ref isReversalSafekeeping);
                    #endregion Contrôles des nouveaux acteurs (DEALER|CLEARER) et paramètres pour frais (EXTPOURNE|RECALCUL)

                    if (Cst.ErrLevel.SUCCESS == finalMessage.ErrLevel)
                    {

                        IPosRequest posRequest = null;
                        IProductBase product = Tools.GetNewProductBase();
                        headerMessage = finalMessage.Message + Cst.CrLf;

                        string caller = IdMenu.GetIdMenu(IdMenu.Menu.POSKEEPING_TRANSFERBULK);
                        AppSession appSession = SessionTools.AppSession;
                        foreach (DataRow dr in pDataTable.Rows)
                        {
                            // Insert POSREQUEST
                            // EG 20150716 [21103] Add pIsReversalSafekeeping
                            posRequest = SetPosRequestPositionTransfer(pCS, product, dr, transferDealer, transferClearer, transferInfo, isReversalSafekeeping);
                            if (null != posRequest)
                            {
                                posRequest.StatusSpecified = true;
                                posRequest.Status = ProcessStateTools.StatusPendingEnum;

                                if (Cst.ErrLevel.SUCCESS != AddNewPosRequest(pCS, null, out int newIdPR, posRequest, appSession, null, null))
                                {
                                    errLevel = Cst.ErrLevel.FAILURE;
                                    returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                                    errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                }
                                else
                                {
                                    posRequest.IdPR = newIdPR;
                                    PosKeepingRequestMQueue requestMQueue = BuildPosKeepingRequestMQueue(pCS, posRequest, pParameters);
                                    // EG 20170315 [22967]
                                    //if (posRequest.idA_CustodianSpecified)
                                    //    listMQueue_OTC.Add(requestMQueue);
                                    //else
                                    //    listMQueue_ETD.Add(requestMQueue);
                                    listMQueue.Add(requestMQueue);
                                    succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                }
                            }
                        }
                        // EG 20170315 [22967]
                        //nbMessageSendToService = ArrFunc.Count(listMQueue_OTC) + ArrFunc.Count(listMQueue_ETD);
                        nbMessageSendToService = ArrFunc.Count(listMQueue);
                        nbMessageNotSendToService = pDataTable.Rows.Count - nbMessageSendToService;
                        // EG 20170315 [22967]
                        if (ArrFunc.IsFilled(listMQueue))
                            MQueueTaskInfo.SetAndSendMultipleThreadPool(pCS, Cst.ProcessTypeEnum.POSKEEPREQUEST, null, caller, appSession, listMQueue);

                    }
                    else
                    {
                        returnMessage = finalMessage.Message + Cst.CrLf;
                    }

                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                //Envoi des messages Mqueue générés
                if (nbMessageSendToService > 0 && nbMessageNotSendToService > 0)
                {
                    errLevel = Cst.ErrLevel.SUCCESS;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_GENERATE_NOGENERATE_MULTI", nbMessageSendToService.ToString(),
                        returnMessage, nbMessageNotSendToService.ToString(), headerMessage + errorMessage + succesMessage);
                }
                else if (nbMessageSendToService > 0)
                {
                    errLevel = Cst.ErrLevel.SUCCESS;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_GENERATE_DATA", nbMessageSendToService.ToString(), headerMessage + succesMessage);
                }
                else if (nbMessageNotSendToService > 0)
                {
                    errLevel = Cst.ErrLevel.FAILURE;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_NOGENERATE_DATA", nbMessageNotSendToService.ToString(), headerMessage + errorMessage);
                }
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }
        #endregion AddPosRequestPositionTransferBLK
        #region ControlActorForTransferBulk
        private static Cst.ErrLevelMessage ControlActorForTransferBulk(string pCS, int pIdA_Entity, string pActor, string pBook, List<RoleActor> pRole,
            ref Pair<IParty, IBookId> pTransferActor)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string returnMessage = string.Empty;

            #region Contrôle Actor / Book / Entity
            SQL_Actor sql_Actor = new SQL_Actor(pCS, pActor);
            sql_Actor.SetRoleRange(pRole.ToArray());
            if (sql_Actor.IsLoaded && (1 == sql_Actor.RowsCount))
            {
                SQL_Book sql_Book = new SQL_Book(pCS, SQL_TableWithID.IDType.Identifier, pBook, SQL_Table.ScanDataDtEnabledEnum.Yes, sql_Actor.Id);
                if (sql_Book.IsLoaded && (1 == sql_Book.RowsCount))
                {
                    // Le book spécifié à la bonne entité et gère la position
                    if (pIdA_Entity == sql_Book.IdA_Entity)
                    {
                        IProductBase product = Tools.GetNewProductBase();
                        // ACTOR
                        IParty party = product.CreateParty();
                        Tools.SetParty(party, sql_Actor);
                        // BOOK
                        IBookId bookId = product.CreateBookId();
                        Tools.SetBookId(bookId, sql_Book);

                        pTransferActor = new Pair<IParty, IBookId>(party, bookId); ;
                        if (pRole.Contains(RoleActor.COUNTERPARTY))
                        {
                            if (false == sql_Book.IsPosKeeping)
                            {
                                // Le book ne gère pas la tenue de position
                                errLevel = Cst.ErrLevel.FAILURE;
                                returnMessage = sql_Book.Identifier + Ressource.GetString("Msg_TradeActionDisabledReason_NoPosKeepingDealer");
                            }
                        }
                    }
                    else
                    {
                        // Entité du book incompatible
                        errLevel = Cst.ErrLevel.FAILURE;
                        returnMessage = Ressource.GetString("Msg_BookNotFoundForActor") + " : " + pActor;
                    }
                }
                else
                {
                    // Book non trouvé pour l'acteur spécifié
                    errLevel = Cst.ErrLevel.FAILURE;
                    returnMessage = Ressource.GetString("Msg_BookNotFoundForActor") + " : " + pActor;
                }
            }
            else
            {
                // Acteur non trouvé
                errLevel = Cst.ErrLevel.FAILURE;
                returnMessage = Ressource.GetString("Msg_ActorNotFound") + " : " + pActor;
            }

            #endregion Contrôle Acteur / Book / Entity
            return new Cst.ErrLevelMessage(errLevel, returnMessage);
        }
        #endregion ControlActorForTransferBulk
        #region SetActorMessageForTransferBulk
        private static string SetActorMessageForTransferBulk<T>(T pInfo)
        {
            string message = string.Empty;
            if (pInfo is IParty)
            {
                IParty _party = pInfo as IParty;
                message = LogTools.IdentifierAndId(_party.PartyName + " / " + _party.PartyId, _party.OtcmlId);
            }
            else if (pInfo is IBookId)
            {
                IBookId _bookId = pInfo as IBookId;
                message = LogTools.IdentifierAndId(_bookId.BookName + " / " + _bookId.Value, _bookId.OtcmlId);
            }
            return message;
        }
        #endregion SetActorMessageForTransferBulk
        #region ControlForTransferBulk
        // EG 20150716 [21103] Add Safekeeping
        private static Cst.ErrLevelMessage ControlForTransferBulk(string pCS, int pIdA_Entity, string[] pDealerArguments,
            ref Pair<IParty, IBookId> pTransferDealer,
            ref Pair<IParty, IBookId> pTransferClearer,
            ref Pair<bool, bool> pTransferInfo,
            ref EFS_Boolean pReversalSafeKeeping)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            string returnMessage = string.Empty;

            if (ArrFunc.IsFilled(pDealerArguments))
            {
                // Donneur d'ordre et book cibles du transfert
                string dealer = string.Empty;
                string bookDealer = string.Empty;
                string clearer = string.Empty;
                string bookClearer = string.Empty;
                bool isReversalFees = false;
                bool isReversalSafekeeping = false;
                bool isCalcNewFees = false;
                #region Récupération des paramètres du transfert ACTOR, BOOK (DEALER|CLEARER) et infos restitution/calcul de frais
                string[] args = pDealerArguments[1].Split('|');
                if (ArrFunc.IsFilled(args))
                {
                    foreach (string arg in args)
                    {
                        string[] value = arg.Split('=');
                        if (ArrFunc.IsFilled(value) && (2 == value.Length))
                        {
                            switch (value[0])
                            {
                                case "DEALER":
                                    dealer = value[1];
                                    break;
                                case "BOOKDEALER":
                                    bookDealer = value[1];
                                    break;
                                case "CLEARER":
                                    clearer = value[1];
                                    break;
                                case "BOOKCLEARER":
                                    bookClearer = value[1];
                                    break;
                                case "ISREVERSALFEES":
                                    isReversalFees = Convert.ToBoolean(value[1]);
                                    break;
                                case "ISCALCNEWFEES":
                                    isCalcNewFees = Convert.ToBoolean(value[1]);
                                    break;
                                // EG 20150716 [21103]
                                case "ISREVERSALSAFEKEEPING":
                                    isReversalSafekeeping = Convert.ToBoolean(value[1]);
                                    break;

                            }
                        }
                    }
                }
                #endregion Récupération des paramètres du transfert ACTOR, BOOK (DEALER|CLEARER) et infos restitution/calcul de frais

                _ = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);
                if ((StrFunc.IsFilled(dealer) && StrFunc.IsFilled(bookDealer)) || (StrFunc.IsFilled(clearer) && StrFunc.IsFilled(bookClearer)))
                {
                    Cst.ErrLevelMessage retActor;
                    if (StrFunc.IsFilled(dealer) && StrFunc.IsFilled(bookDealer))
                    {
                        #region Contrôle Dealer
                        retActor = ControlActorForTransferBulk(pCS, pIdA_Entity, dealer, bookDealer,
                            new List<RoleActor>() { RoleActor.COUNTERPARTY }, ref pTransferDealer);
                        if (Cst.ErrLevel.SUCCESS != retActor.ErrLevel)
                        {
                            errLevel = Cst.ErrLevel.FAILURE;
                            returnMessage += retActor.Message;
                        }
                        #endregion Contrôle Dealer
                    }
                    if (StrFunc.IsFilled(clearer) && StrFunc.IsFilled(bookClearer))
                    {
                        #region Contrôle Clearer
                        // RD 20210521 [25756] Add CSS Role
                        retActor = ControlActorForTransferBulk(pCS, pIdA_Entity, clearer, bookClearer,
                            new List<RoleActor>() { RoleActor.CLEARER, RoleActor.CUSTODIAN, RoleActor.CSS }, ref pTransferClearer);
                        if (Cst.ErrLevel.SUCCESS != retActor.ErrLevel)
                        {
                            errLevel = Cst.ErrLevel.FAILURE;
                            returnMessage += retActor.Message;
                        }
                        #endregion Contrôle Clearer
                    }
                }
                else
                {
                    // Acteur et Book non renseigné
                    errLevel = Cst.ErrLevel.FAILURE;
                    returnMessage = "Acteur et Book non renseigné";
                }

                if (Cst.ErrLevel.SUCCESS == errLevel)
                {
                    pTransferInfo = new Pair<bool, bool>(isReversalFees, isCalcNewFees);
                    // EG 20150716 [21103]
                    pReversalSafeKeeping = new EFS_Boolean(isReversalSafekeeping);

                    // Caractéristiques du transfert de masse
                    // Nouvel acteur DO
                    // ● Identifiant de l'acteur: XXXXXXXXXX(999)|Inchangé
                    // ● Identifiant du book: XXXXXXXXXX(999)|Inchangé
                    //
                    // Nouvel acteur CLEARER ou CUSTODIAN
                    // ● Identifiant de l'acteur: XXXXXXXXXX(999)|Inchangé
                    // ● Identifiant du book: XXXXXXXXXX(999)|Inchangé
                    //
                    // Frais
                    // ● Restitution des frais sur la négociation source : true|false
                    // ● Calcul des frais sur la nouvelle négociation : true|false
                    string unchanged = Ressource.GetString("Unchanged", true);
                    returnMessage = Ressource.GetString("Msg_PosRequestPositiontransferBulk_Header");
                    // EG 20150716 [21103]
                    returnMessage = String.Format(returnMessage,
                        (null != pTransferDealer) ? SetActorMessageForTransferBulk(pTransferDealer.First) : unchanged,
                        (null != pTransferDealer) ? SetActorMessageForTransferBulk(pTransferDealer.Second) : unchanged,
                        (null != pTransferClearer) ? SetActorMessageForTransferBulk(pTransferClearer.First) : unchanged,
                        (null != pTransferClearer) ? SetActorMessageForTransferBulk(pTransferClearer.Second) : unchanged,
                        Ressource.GetString("isReversalFees"), isReversalFees.ToString(),
                        Ressource.GetString("isNewCalculationFees"), isCalcNewFees.ToString(),
                        Ressource.GetString("isReversalSafekeeping"), isReversalSafekeeping.ToString());
                    returnMessage += Cst.HTMLHorizontalLine;
                }
            }
            else
            {
                // Dealer et Book non renseignés
                errLevel = Cst.ErrLevel.FAILURE;
                returnMessage = "Dealer et Book non renseignés";
            }
            return new Cst.ErrLevelMessage(errLevel, returnMessage);
        }
        #endregion ControlForTransferBulk

        #region AddPosRequestDenouementBLK
        /// <summary>
        /// Génération des demande (POSREQUEST) pour Abandon ou Assignations/Exercices de masse
        /// </summary>
        // EG 20151019 [21495] New
        // EG 20180307 [23769] Gestion dbTransaction
        public static Cst.ErrLevelMessage AddPosRequestDenouementBLK(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevelMessage finalMessage = null;
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            List<MQueueBase> listMQueue = new List<MQueueBase>();
            int nbMessageSendToService = 0;
            int nbMessageNotSendToService = 0;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            try
            {
                // ASSEXE ou ABN
                //string caller = IdMenu.GetIdMenu(pIdMenu);
                string idMenuSys = pMsgDatas[1];
                Menu mnu = SessionTools.Menus.SelectByID(idMenuSys);
                string caller = mnu.IdMenu;
                // Détermination du RequestMode en fonction du menu parent associé : EOD ou ITD
                Menu mnuParent = SessionTools.Menus.GetMenuParentById(idMenuSys);
                string[] mnuParentSplit = mnuParent.IdMenu.Split(new char[] { '_' });

                Nullable<SettlSessIDEnum> requestMode = null;
                if (ArrFunc.IsFilled(mnuParentSplit))
                    requestMode = (SettlSessIDEnum)ReflectionTools.EnumParse(new SettlSessIDEnum(), mnuParentSplit.Last());

                if ((0 < pDataTable.Rows.Count) && requestMode.HasValue)
                {
                    IPosRequest posRequest = null;
                    IProductBase product = Tools.GetNewProductBase();
                    AppSession session = SessionTools.AppSession;

                    // Dénouement de la quantité restante en position ou non
                    // Si remainingQty = TRUE  alors création d'une nouvelle demande (new IDPR)
                    // Si remainingQty = FALSE alors mise à jour de la demande existante (upd IDPR)
                    Cst.DenOptionActionType denOptionActionType = Cst.DenOptionActionType.@new;
                    if (null != pParameters["DENOPTIONACTIONTYPE"])
                        denOptionActionType = (Cst.DenOptionActionType)ReflectionTools.EnumParse(new Cst.DenOptionActionType(), pParameters["DENOPTIONACTIONTYPE"].Value);

                    // Application du barême sur Dénouement
                    Nullable<bool> isFeeCalculation = null;
                    if (null != pParameters["ISIGNOREFEECALCULATION"])
                        isFeeCalculation = !Convert.ToBoolean(pParameters["ISIGNOREFEECALCULATION"].Value);

                    // Contrôle Lock ENTITYMARKET
                    string lockMsg = string.Empty;
                    Nullable<int> idEM = null;
                    if ((0 < Convert.ToInt32(pParameters["ENTITY"].Value)) && (0 < Convert.ToInt32(pParameters["MARKET"].Value)))
                    {
                        idEM = Convert.ToInt32(pDataTable.Rows[0]["IDEM"]);
                        // EG 20151102 [21465] ObjectId = string
                        LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, idEM.Value, pParameters["ENTITY"].ExValue + " - " + pParameters["MARKET"].ExValue, LockTools.Exclusive);
                        lockMsg = LockTools.SearchProcessLocks(pCS, null, lockEntityMarket, SessionTools.AppSession);
                    }
                    else
                    {
                        lockMsg = LockTools.SearchProcessLocks(pCS, null, TypeLockEnum.ENTITYMARKET, LockTools.Exclusive, SessionTools.AppSession);
                    }

                    if (StrFunc.IsFilled(lockMsg))
                    {
                        errLevel = Cst.ErrLevel.FAILURE;
                        errorMessage += lockMsg + Cst.HTMLHorizontalLine;
                    }
                    else
                    {
                        foreach (DataRow dr in pDataTable.Rows)
                        {
                            // Insert POSREQUEST
                            posRequest = SetPosRequestDenouement(pCS, product, dr, requestMode.Value, isFeeCalculation);
                            if (null != posRequest)
                            {
                                posRequest.StatusSpecified = true;
                                posRequest.Status = ProcessStateTools.StatusPendingEnum;
                                if (0 < posRequest.IdPR)
                                {
                                    posRequest.SetSource(session.AppInstance);
                                    //pPosRequest.source = pAppInstance.AppNameVersion;
                                    UpdatePosRequest(pCS, null, posRequest.IdPR, posRequest, session.IdA, null, null);
                                }
                                else
                                {
                                    // La demande n'existe pas => on la créée
                                    errLevel = AddNewPosRequest(pCS, null, out int newIdPR, posRequest, session, null, null);
                                    posRequest.IdPR = newIdPR;
                                }

                                if (Cst.ErrLevel.SUCCESS != errLevel)
                                {
                                    errLevel = Cst.ErrLevel.FAILURE;
                                    returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                                    errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                }
                                else
                                {
                                    PosKeepingRequestMQueue requestMQueue = BuildPosKeepingRequestMQueue(pCS, posRequest, pParameters);
                                    listMQueue.Add(requestMQueue);
                                    succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                                }
                            }
                        }
                    }
                    nbMessageSendToService = ArrFunc.Count(listMQueue);
                    nbMessageNotSendToService = pDataTable.Rows.Count - nbMessageSendToService;

                    if (ArrFunc.IsFilled(listMQueue) && requestMode.HasValue && (requestMode.Value == SettlSessIDEnum.Intraday))
                        MQueueTaskInfo.SetAndSendMultipleThreadPool(pCS, Cst.ProcessTypeEnum.POSKEEPREQUEST, Cst.ProductGProduct_FUT, caller, session, listMQueue);

                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                //Envoi des messages Mqueue générés
                if (nbMessageSendToService > 0 && nbMessageNotSendToService > 0)
                {
                    errLevel = Cst.ErrLevel.SUCCESS;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_GENERATE_NOGENERATE_MULTI", nbMessageSendToService.ToString(),
                        returnMessage, nbMessageNotSendToService.ToString(), errorMessage + succesMessage);
                }
                else if (nbMessageSendToService > 0)
                {
                    errLevel = Cst.ErrLevel.SUCCESS;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_GENERATE_DATA", nbMessageSendToService.ToString(), succesMessage);
                }
                else if (nbMessageNotSendToService > 0)
                {
                    errLevel = Cst.ErrLevel.FAILURE;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_NOGENERATE_DATA", nbMessageNotSendToService.ToString(), errorMessage);
                }
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }
        #endregion AddPosRequestDenouementBLK

        #region AddPosRequestOption
        /// <summary>
        /// TRAITEMENT DU DENOUEMENT MANUEL D'OPTION 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = AAB/AAS/AEX/ABN/NEX/NAS/ASS/EXE)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pProduct">Product</parparam>
        /// <param name="pPosRequestType">Type de demande</parparam>
        /// <param name="pIdT">Identifiant de l'option</param>
        /// <param name="pIdentifier">Identifier de l'option</param>/// 
        /// <param name="pIdEM">Identifiant du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness">Date de journée</param>
        /// <param name="pQty">Quantité dénouée</param>
        /// <param name="pAvailableQty">Quantité disponible</param>
        /// <param name="pStrikePrice">Strike</param>
        /// <param name="pIdAsset">Identifiant de l'asset du sous-jacent</param>
        /// <param name="pPrice">Prix</param>
        /// <param name="pDtPrice">Date du prix</param>
        /// <param name="pSource">Source du prix</param>
        /// <param name="pPaymentFees">Frais liés au dénouement</param>
        /// <param name="pNotes">Commentaires</param>
        /// <param name="pAppInstance">Application / Id User</param>
        /// <returns>Code retour + Message</returns>
        /// FI 20130318 [18467] add pAppInstance
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151019 [21465] Add Null instead to new pIsFeeCalculation parameter
        // EG 20170127 Qty Long To Decimal
        // EG 2017023 Add SQLProduct.GroupProduct
        public static Cst.ErrLevelMessage AddPosRequestOption(string pCS, IProductBase pProduct, Cst.PosRequestTypeEnum pRequestType,
            SettlSessIDEnum pRequestMode, int pIdT, string pIdentifier,
            int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness, decimal pQty, decimal pAvailableQty, decimal pStrikePrice,
            Cst.UnderlyingAsset pAssetCategory, int pIdAsset, string pAssetIdentifier, QuoteTimingEnum pQuotetiming,
            decimal pPrice, DateTime pDtPrice, string pSource, IPayment[] pPaymentFees, string pNotes, AppSession  pAppSession, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {

            IPosRequest posRequest = CreatePosRequestOption(pCS, pProduct, pRequestType, pRequestMode, pIdT, pIdentifier,
                pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM,
                pDtBusiness, pQty, pAvailableQty, pStrikePrice, pAssetCategory, pIdAsset, pAssetIdentifier, pQuotetiming, pPrice, pDtPrice, pSource, 
                pPaymentFees, pNotes, false, pGroupProduct);

            posRequest.StatusSpecified = true;
            posRequest.Status = ProcessStateTools.StatusPendingEnum;

            MQueueparameters parameters = new MQueueparameters();
            // EG 20170127 Qty Long To Decimal
            parameters.Add(new MQueueparameter("QTY", TypeData.TypeDataEnum.@decimal));
            parameters["QTY"].SetValue(pQty);

            return AddPosRequest(pCS, null, posRequest, pAppSession, parameters, null);
        }
        #endregion AddPosRequestOption

        #region AddPosRequestRemoveCAExecuted
        /// <summary>
        /// ANNULATION d'un TRAITEMENT DE CORPORATE ACTION
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = RMVCAEXECUTED)</para> 
        /// <para>► Postage d'un message PosKeepingRequestMQueue (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// EG 20130607 [18740] Add RemoveCAExecuted
        public static Cst.ErrLevelMessage AddPosRequestRemoveCAExecuted(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pMsgDatas)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            Cst.ErrLevelMessage finalMessage;
            try
            {
                if (0 < pDataTable.Rows.Count)
                {
                    IPosRequest posRequest = null;
                    IProductBase product = Tools.GetNewProductBase();
                    foreach (DataRow dr in pDataTable.Rows)
                    {
                        int idA_CssCustodian = Convert.ToInt32(dr["IDA_CSSCUSTODIAN"]);
                        string cssCustodianIdentifier = dr["CSSCUSTODIAN_IDENTIFIER"].ToString();
                        int idM = Convert.ToInt32(dr["IDM"]);
                        string marketIdentifier = dr["SHORT_ACRONYM"].ToString();
                        int idCA = Convert.ToInt32(dr["IDCA"]);
                        string caIdentifier = dr["CA_IDENTIFIER"].ToString();
                        int idCE = Convert.ToInt32(dr["IDCE"]);
                        DateTime dtBusiness = Convert.ToDateTime(dr["EFFECTIVEDATE"]);
                        bool isCustodian = false;

                        posRequest = product.CreatePosRequestRemoveCAExecuted(idA_CssCustodian, idM, idCE, dtBusiness, isCustodian);
                        posRequest.SetIdentifiers(null, cssCustodianIdentifier, marketIdentifier);
                        posRequest.Identifiers.CorporateAction = caIdentifier;
                        posRequest.RequestMode = SettlSessIDEnum.Intraday;
                        posRequest.StatusSpecified = true;
                        posRequest.Status = ProcessStateTools.StatusPendingEnum;

                        if (null == pParameters["CSSCUSTODIAN"])
                            pParameters.Add(new MQueueparameter("CSSCUSTODIAN", TypeData.TypeDataEnum.integer));
                        pParameters["CSSCUSTODIAN"].SetValue(idA_CssCustodian, cssCustodianIdentifier);
                        if (null == pParameters["MARKET"])
                            pParameters.Add(new MQueueparameter("MARKET", TypeData.TypeDataEnum.integer));
                        pParameters["MARKET"].SetValue(idM, marketIdentifier);
                        if (null == pParameters["CORPORATEACTION"])
                            pParameters.Add(new MQueueparameter("CORPORATEACTION", TypeData.TypeDataEnum.integer));
                        pParameters["CORPORATEACTION"].SetValue(idCA, caIdentifier);
                        if (null == pParameters["CORPORATEEVENT"])
                            pParameters.Add(new MQueueparameter("CORPORATEEVENT", TypeData.TypeDataEnum.integer));
                        if (null == pParameters["DTBUSINESS"])
                            pParameters.Add(new MQueueparameter("DTBUSINESS", TypeData.TypeDataEnum.date));
                        pParameters["DTBUSINESS"].SetValue(dtBusiness);

                        Cst.ErrLevelMessage _errMessage = AddPosRequest(pCS, null, posRequest, SessionTools.AppSession, pParameters, dr);
                        if (Cst.ErrLevel.SUCCESS != _errMessage.ErrLevel)
                        {
                            errLevel = Cst.ErrLevel.FAILURE;
                            returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                            errorMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                        }
                        else
                        {
                            succesMessage += posRequest.RequestMessage + Cst.HTMLHorizontalLine;
                        }
                    }
                }
                else
                {
                    returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] Appel à ExceptionTools.GetMessageExtended
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                errLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                returnMessage = Ressource.GetString2(returnMessage, pDataTable.Rows.Count.ToString(), errorMessage + succesMessage);
                finalMessage = new Cst.ErrLevelMessage(errLevel, returnMessage);
            }
            return finalMessage;
        }
        #endregion AddPosRequestRemoveCAExecuted

        #region GetUnderLyingAssetRelativeToInstrument
        // EG 20180326 [23768] Gestion de la source de type MapDataReaderRow
        public static Nullable<Cst.UnderlyingAsset> GetUnderLyingAssetRelativeToInstrument<T>(T pSource)
        {
            Nullable<Cst.UnderlyingAsset> _underlyingAsset = null;
            string _temp = string.Empty;
            if (pSource is DataRow)
            {
                DataRow _row = pSource as DataRow;
                if (_row.Table.Columns.Contains("ASSETCATEGORY") && (false == Convert.IsDBNull(_row["ASSETCATEGORY"])))
                    _temp = Convert.ToString(_row["ASSETCATEGORY"].ToString());
            }
            else if (pSource is IDataReader)
            {
                IDataReader _row = pSource as IDataReader;
                if (ReaderContainsColumn(_row, "ASSETCATEGORY") && (false == Convert.IsDBNull(_row["ASSETCATEGORY"])))
                    _temp = Convert.ToString(_row["ASSETCATEGORY"].ToString());
            }
            else if (pSource is MapDataReaderRow)
            {
                MapDataReaderRow _row = pSource as MapDataReaderRow;
                if ((null != _row["ASSETCATEGORY"]) && (false == Convert.IsDBNull(_row["ASSETCATEGORY"])))
                    _temp = Convert.ToString(_row["ASSETCATEGORY"].Value.ToString());
            }
            else if (pSource is string)
            {
                _temp = pSource as string;
            }
            if (StrFunc.IsFilled(_temp))
            {
                if (System.Enum.IsDefined(typeof(Cst.UnderlyingAsset), _temp))
                    _underlyingAsset = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), _temp);
            }
            return _underlyingAsset;
        }
        #endregion GetUnderLyingAssetRelativeToInstrument
        #region CreateScheduledPosRequest
        /// <summary>
        /// CREATION D'UNE LIGNE DANS POSREQUEST DANS LE CAS D'UN MESSAGE SCHEDULE
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (en fonction REQUESTTYPE)</para> 
        /// <para>► Retourne l'interface POSREQUEST de la ligne insérée (IDPR)</para> 
        /// <para>────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pQueue"></param>
        /// <param name="pProduct"></param>
        /// <param name="pAppSession"></param>
        /// <param ></param>
        /// <returns></returns>
        /// FI 20130318 [18467] add pAppInstance parameter
        /// FI 20130403 [] call PosKeepingTools.AddNewPosRequest
        /// EG 20150317 [POC] New parameter for CreatePosRequestEOD|CreatePosRequestClosingDAY
        /// RD 20151127 [21596] Modify
        /// FI 20170327 [23004] Modify
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Use pRequestType to CreatePosRequestEOD)
        public static IPosRequest CreateScheduledPosRequest(string pCS, IDbTransaction pDbTransaction, PosKeepingRequestMQueue pQueue, IProductBase pProduct, AppSession pAppSession)
        {

            int idA_Entity = Convert.ToInt32(pQueue.parameters["ENTITY"].Value);
            string entityIdentifier = pQueue.parameters["ENTITY"].ExValue;
            
            int idA_CssCustodian = Convert.ToInt32(pQueue.parameters["CSSCUSTODIAN"].Value);
            string cssCustodianIdentifier = pQueue.parameters["CSSCUSTODIAN"].ExValue;
            
            DateTime dtBusiness = Convert.ToDateTime(pQueue.parameters["DTBUSINESS"].Value);
            // RD 20151127 [21596] Valorize isCustodian according to GPRODUCT (see methode AddPosRequestCommonEndOfDay)
            //bool isCustodian = false;
            // FI 20170327 [23004] Alimentation de gProduct
            string gProduct = pQueue.GetStringValueIdInfoByKey("GPRODUCT");
            bool isCustodian = (gProduct != Cst.ProductGProduct_FUT);
            
            string marketIdentifier = string.Empty;


            IPosRequest posRequest = null;
            switch (pQueue.requestType)
            {
                case Cst.PosRequestTypeEnum.EndOfDay:
                case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                    // EG 20150317 [POC] 
                    posRequest = pProduct.CreatePosRequestEOD(idA_Entity, idA_CssCustodian, dtBusiness, null, isCustodian, pQueue.requestType);
                    posRequest.SetIdentifiers(entityIdentifier, cssCustodianIdentifier);
                    break;
                case Cst.PosRequestTypeEnum.ClosingDay:
                    // EG 20150317 [POC] 
                    posRequest = pProduct.CreatePosRequestClosingDAY(idA_Entity, idA_CssCustodian, dtBusiness, null, isCustodian);
                    posRequest.SetIdentifiers(entityIdentifier, cssCustodianIdentifier);
                    break;
                case Cst.PosRequestTypeEnum.RemoveEndOfDay:
                    if (null != pQueue.parameters["ENTITYMARKET"])
                    {
                        int idEM = Convert.ToInt32(pQueue.parameters["ENTITYMARKET"].Value);
                        posRequest = pProduct.CreatePosRequestREMOVEEOD(idA_Entity, idA_CssCustodian, dtBusiness, idEM, isCustodian);
                    }
                    else
                        posRequest = pProduct.CreatePosRequestREMOVEEOD(idA_Entity, idA_CssCustodian, dtBusiness, isCustodian);

                    if (null != pQueue.parameters["MARKET"])
                    {
                        _ = Convert.ToInt32(pQueue.parameters["MARKET"].Value);
                        marketIdentifier = pQueue.parameters["MARKET"].ExValue;
                    }
                    posRequest.SetIdentifiers(entityIdentifier, cssCustodianIdentifier, marketIdentifier);
                    break;

                case Cst.PosRequestTypeEnum.UpdateEntry: // FI 20170327 [23004] Add UpdateEntry
                    posRequest = pProduct.CreatePosRequestUpdateEntry(SettlSessIDEnum.Intraday, dtBusiness);
                    posRequest.SetPosKey(Convert.ToInt32(pQueue.GetIdInfoEntry("IDI").Value),
                                        null, Convert.ToInt32(pQueue.GetIdInfoEntry("IDASSET").Value),
                                        Convert.ToInt32(pQueue.GetIdInfoEntry("IDA_DEALER").Value),
                                        Convert.ToInt32(pQueue.GetIdInfoEntry("IDB_DEALER").Value),
                                        Convert.ToInt32(pQueue.GetIdInfoEntry("IDA_CLEARER").Value),
                                        Convert.ToInt32(pQueue.GetIdInfoEntry("IDB_CLEARER").Value));
                    posRequest.SetAdditionalInfo(Convert.ToInt32(pQueue.GetIdInfoEntry("IDA_ENTITY").Value),
                                                 Convert.ToInt32(pQueue.GetIdInfoEntry("IDA_ENTITYCLEARER").Value));


                    posRequest.IdTSpecified = true;
                    posRequest.IdT = Convert.ToInt32(pQueue.GetIdInfoEntry("IDT").Value);
                    
                    posRequest.IdA_Entity = Convert.ToInt32(pQueue.GetIdInfoEntry("IDA_ENTITY").Value);
                    posRequest.SetIdA_CssCustodian(Convert.ToInt32(pQueue.GetIdInfoEntry("IDA_CSSCUSTODIAN").Value), isCustodian);
                    posRequest.IdEMSpecified = true;
                    posRequest.IdEM = Convert.ToInt32(pQueue.GetIdInfoEntry("IDEM").Value);

                    marketIdentifier = Convert.ToString(pQueue.GetIdInfoEntry("MARKET").Value);
                    posRequest.SetIdentifiers(entityIdentifier, cssCustodianIdentifier, marketIdentifier);

                    posRequest.GroupProductSpecified = true; 
                    posRequest.GroupProduct = ProductTools.GroupProductEnum.ExchangeTradedDerivative;
                    
                    break;

            }
            if (null != posRequest)
            {
                posRequest.StatusSpecified = true;
                posRequest.Status = ProcessStateTools.StatusPendingEnum;

                PosKeepingTools.AddNewPosRequest(pCS, pDbTransaction, out int newIdPR, posRequest, pAppSession, null, null);
                posRequest.IdPR = newIdPR;
            }
            return posRequest;
        }
        #endregion CreateScheduledPosRequest

        #region CreatePosRequestCorrection
        /// <summary>
        /// CORRECTION DE POSITION
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = POC)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pProduct">ProductBase</param>
        /// <param name="pIdT">Id de l'allocation</param>
        /// <param name="pIdA_Entity">Id de l'entité</param>
        /// <param name="pIdA_CSS">Id de la chambre</param>
        /// <param name="pIdEM">Identifiant du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness">date de correction</param>
        /// <param name="pQty">Quantité supprimée</param>
        /// <param name="pAvailableQty">Quantité avant correction</param>
        /// <param name="pPaymentFees">Frais liés à la correction (extourne?)</param>
        /// <param name="pNotes">Commentaires</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        // EG 2017023 Add pGroupProduct
        public static IPosRequest CreatePosRequestCorrection(IProductBase pProduct, SettlSessIDEnum pRequestMode,
            int pIdT, string pIdentifier,
            int pIdA_Entity, Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            decimal pInitialQty, decimal pPositionQty, decimal pQty,
            IPayment[] pPaymentFees, EFS_Boolean pIsSafekeepingReversal, string pNotes, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            IPosRequestCorrection posRequest = pProduct.CreatePosRequestCorrection(pRequestMode, pDtBusiness, pQty);
            posRequest.IdA_Entity = pIdA_Entity;
            posRequest.SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            posRequest.IdEM = pIdEM;
            posRequest.IdEMSpecified = (0 < pIdEM);
            posRequest.PosKeepingKeySpecified = false;
            posRequest.IdTSpecified = true;
            posRequest.IdT = pIdT;
            posRequest.StatusSpecified = true;
            posRequest.Status = ProcessStateTools.StatusPendingEnum;
            posRequest.SetNotes(pNotes);
            posRequest.SetDetail(pInitialQty, pPositionQty, pPaymentFees, pIsSafekeepingReversal);
            posRequest.SetIdentifiers(pIdentifier);
            // EG 2017023 New
            posRequest.GroupProductSpecified = pGroupProduct.HasValue;
            if (posRequest.GroupProductSpecified)
                posRequest.GroupProduct = pGroupProduct.Value;

            return posRequest;
        }
        #endregion CreatePosRequestCorrection
        #region CreatePosRequestTransfer
        /// <summary>
        /// TRANSFERT UNITAIRE DE POSITION
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = POT)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pProduct">ProductBase</param>
        /// <param name="pIdT">Id du trade dont une quantité est transférée</param>
        /// <param name="pIdentifier">Identifiant du trade dont une quantité est transférée</param>
        /// <param name="pIdA_Entity">Id de l'entité</param>
        /// <param name="pIdA_CSS">Id de la chambre</param>
        /// <param name="pIdEM">Id du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness">date de transfert</param>
        /// <param name="pInitialQty">Quantité initiale de l'allocation transféré</param>
        /// <param name="pPositionQty">Quantité en position avant transfert</param>
        /// <param name="pQty">Quantité à transferer</param>
        /// <param name="pPaymentFees">Frais liés au transfert(extourne)</param>
        /// <param name="pNotes">Commentaires</param>
        /// <param name="pIdTTarget">Id du trade cible du transfer</param>
        /// EG 20150716 [21103] Add pIsReversalSafekeeping
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        // EG 2017023 Add pGroupProduct
        public static IPosRequest CreatePosRequestTransfer(string pCS, IProductBase pProduct,
            int pIdT, string pIdentifier,
            int pIdA_Entity, Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            decimal pInitialQty, decimal pPositionQty, decimal pQty,
            IPayment[] pPaymentFees, string pNotes, int pIdTTarget, EFS_Boolean pIsReversalSafekeeping, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            IPosRequestTransfer posRequest = pProduct.CreatePosRequestTransfer(pDtBusiness, pQty);
            posRequest.IdA_Entity = pIdA_Entity;
            posRequest.SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            posRequest.IdEM = pIdEM;
            posRequest.IdEMSpecified = (0 < pIdEM);
            posRequest.PosKeepingKeySpecified = false;
            posRequest.IdTSpecified = true;
            posRequest.IdT = pIdT;
            posRequest.StatusSpecified = true;
            posRequest.Status = ProcessStateTools.StatusPendingEnum;
            posRequest.SetNotes(pNotes);
            /// EG 20150716 [21103]
            posRequest.SetDetail(pCS, pInitialQty, pPositionQty, pPaymentFees, pIdTTarget, pIsReversalSafekeeping);
            posRequest.SetIdentifiers(pIdentifier);
            // EG 2017023 New
            posRequest.GroupProductSpecified = pGroupProduct.HasValue;
            if (posRequest.GroupProductSpecified)
                posRequest.GroupProduct = pGroupProduct.Value;

            return posRequest;
        }
        #endregion CreatePosRequestTransfert
        #region CreatePosRequestOption
        /// <summary>
        /// DENOUEMENT MANUEL D'OPTION
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = AAB/AAS/AEX/ABN/NEX/NAS/ASS/EXE)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// Alimentation de la classe POSREQUEST dans le cas du dénouement d'option
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pIdT">Id du trade option</param>
        /// <param name="pIdA_Entity">Id de l'entité</param>
        /// <param name="pIdA_CSS">Id de la chambre</param>
        /// <param name="pIdEM">Id du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pQty">Quantité dénouée</param>
        /// <param name="pAvailableQty">Quantité disponible</param>
        /// <param name="pStrikePrice">Strike</param>
        /// <param name="pIdAsset">Identifiant de l'asset du sous-jacent</param>
        /// <param name="pPrice">Prix</param>
        /// <param name="pDtPrice">Date du prix</param>
        /// <param name="pSource">Source du prix</param>
        /// <param name="pPaymentFees">Frais liés au dénouement</param>
        /// <param name="pNotes">Commentaires</param>
        // EG 20150920 [21314] Int (int32) to Long (Int64)
        // EG 20151019 [21465] Add Nullable on parameters pQuoteTiming|pPrice|pDtPrice, Add Nullable<bool> pIsFeeCalculation
        // EG 20170127 Qty Long To Decimal
        // EG 2017023 Add pGroupProduct
        public static IPosRequest CreatePosRequestOption(string pCS, IProductBase pProduct, Cst.PosRequestTypeEnum pRequestType,
            SettlSessIDEnum pRequestMode, int pIdT, string pIdentifier,
            int pIdA_Entity, Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness, decimal pQty, decimal pAvailableQty, decimal pStrikePrice,
            Cst.UnderlyingAsset pAssetCategory,
            int pIdAsset, string pAssetIdentifier, Nullable<QuoteTimingEnum> pQuoteTiming, Nullable<decimal> pPrice, Nullable<DateTime> pDtPrice, string pSource,
            IPayment[] pPaymentFees, string pNotes, Nullable<bool> pIsFeeCalculation, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            IPosRequestOption posRequest = pProduct.CreatePosRequestOption(pRequestType, pRequestMode, pDtBusiness, pQty);
            posRequest.IdA_Entity = pIdA_Entity;
            posRequest.SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            posRequest.IdEM = pIdEM;
            posRequest.IdEMSpecified = (0 < pIdEM);
            posRequest.PosKeepingKeySpecified = false;
            posRequest.IdTSpecified = true;
            posRequest.IdT = pIdT;
            posRequest.StatusSpecified = true;
            posRequest.Status = ProcessStateTools.StatusPendingEnum;
            posRequest.SetNotes(pNotes);
            posRequest.SetDetail(pCS, pAvailableQty, pStrikePrice, pAssetCategory, pIdAsset, pAssetIdentifier, pQuoteTiming, pPrice, pDtPrice, pSource, pPaymentFees, pIsFeeCalculation);
            posRequest.SetIdentifiers(pIdentifier);
            // EG 2017023 New
            posRequest.GroupProductSpecified = pGroupProduct.HasValue;
            if (posRequest.GroupProductSpecified)
                posRequest.GroupProduct = pGroupProduct.Value;
            return posRequest;
        }
        #endregion CreatePosRequestOption

        #region CreatePosRequestRemoveAlloc
        /// <summary>
        /// ANNULATION D'ALLOCATION
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = RMVALLOC)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pProduct">ProductBase</param>
        /// <param name="pIdT">Id du trade annulé</param>
        /// <param name="pIdentifier">Identifiant du trade annulé</param>
        /// <param name="pIdA_Entity">Id de l'entité</param>
        /// <param name="pIdA_CSS">Id de la chambre</param>
        /// <param name="pIdEM">Id du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness">date de l'annulation</param>
        /// <param name="pInitialQty">Quantité initiale de l'allocation annulée</param>
        /// <param name="pPositionQty">Quantité en position avant annulation</param>
        /// <param name="pNotes">Commentaires</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        // EG 2017023 Add pGroupProduct
        public static IPosRequest CreatePosRequestRemoveAlloc(string pCS, IProductBase pProduct,
            int pIdT, string pIdentifier,
            int pIdA_Entity, Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            decimal pInitialQty, decimal pPositionQty, string pNotes, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            IPosRequestRemoveAlloc posRequest = pProduct.CreatePosRequestRemoveAlloc(pDtBusiness, pInitialQty);
            posRequest.IdA_Entity = pIdA_Entity;
            posRequest.SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            posRequest.IdEM = pIdEM;
            posRequest.IdEMSpecified = (0 < pIdEM);
            posRequest.PosKeepingKeySpecified = false;
            posRequest.IdTSpecified = true;
            posRequest.IdT = pIdT;
            posRequest.StatusSpecified = true;
            posRequest.Status = ProcessStateTools.StatusPendingEnum;
            posRequest.SetNotes(pNotes);
            posRequest.SetDetail(pCS, pInitialQty, pPositionQty);
            posRequest.SetIdentifiers(pIdentifier);

            // EG 2017023 New
            posRequest.GroupProductSpecified = pGroupProduct.HasValue;
            if (posRequest.GroupProductSpecified)
                posRequest.GroupProduct = pGroupProduct.Value;

            return posRequest;
        }
        #endregion CreatePosRequestRemoveAlloc
        #region CreatePosRequestSplit
        /// <summary>
        /// TRADESPLITTING
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = TRADESPLITTING)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pProduct">ProductBase</param>
        /// <param name="pIdT">Id du trade annulé</param>
        /// <param name="pIdentifier">Identifiant du trade annulé</param>
        /// <param name="pIdA_Entity">Id de l'entité</param>
        /// <param name="pIdA_CSS">Id de la chambre</param>
        /// <param name="pIdEM">Id du couple ENTITE/MARCHE</param>
        /// <param name="pDtBusiness">date de compensation</param>
        /// <param name="pQty">Quantité du trade Splitée</param>
        /// <param name="pNotes">Commentaires</param>
        /// <param name="pNewTrades">New trades caractéristiques (Dealer, Qty)</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 2017023 Add pGroupProduct
        public static IPosRequest CreatePosRequestSplit(string pCS, IProductBase pProduct,
            int pIdT, string pIdentifier,
            int pIdA_Entity, Nullable<int> pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            decimal pQty, string pNotes, ArrayList pNewTrades, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            IPosRequestSplit posRequest = pProduct.CreatePosRequestSplit(pDtBusiness, pQty);
            posRequest.IdA_Entity = pIdA_Entity;
            posRequest.SetIdA_CssCustodian(pIdA_Css, pIdA_Custodian);
            posRequest.IdEM = pIdEM;
            posRequest.IdEMSpecified = (0 < pIdEM);
            posRequest.PosKeepingKeySpecified = false;
            posRequest.IdTSpecified = true;
            posRequest.IdT = pIdT;
            posRequest.StatusSpecified = true;
            posRequest.Status = ProcessStateTools.StatusPendingEnum;
            posRequest.SetNotes(pNotes);
            posRequest.SetDetail(pCS, pNewTrades);
            posRequest.SetIdentifiers(pIdentifier);

            // EG 2017023 New
            posRequest.GroupProductSpecified = pGroupProduct.HasValue;
            if (posRequest.GroupProductSpecified)
                posRequest.GroupProduct = pGroupProduct.Value;

            return posRequest;
        }
        #endregion CreatePosRequestSplit
        #region CreatePosRequestRemoveCAExecuted
        /*
        /// <summary>
        /// ANNULATION D'ALLOCATION
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Alimentation de la table POSREQUEST (REQUESTTYPE = RMVCAEXECUTED)</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// EG 20130607 [18740] Add RemoveCAExecuted
        public static IPosRequest CreatePosRequestRemoveCAExecuted(string pCS, IProductBase pProduct, int pIdA_CssCustodian, int pIdM, int pIdCE, DateTime pDtBusiness, bool )
        {
            IPosRequestRemoveCAExecuted posRequest = null;
            posRequest = (IPosRequestRemoveCAExecuted)pProduct.CreatePosRequestRemoveCAExecuted(pIdA_CssCustodian, pIdM, pIdCE, pDtBusiness);
            posRequest.SetIdA_CssCustodian(pIdA_CssCustodian);
            posRequest.idM = pIdM;
            posRequest.idMSpecified = (0 < pIdM);
            posRequest.idCE = pIdCE;
            posRequest.idCESpecified = (0 < pIdCE);
            posRequest.posKeepingKeySpecified = false;
            posRequest.statusSpecified = true;
            posRequest.status = ProcessStateTools.StatusPendingEnum;
            return posRequest;
        }
        */
        #endregion CreatePosRequestRemoveCAExecuted


        #region DeletePosRequest
        /// <summary>
        /// Suppression d'un POSREQUEST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pdbTransaction"></param>
        /// <param name="pIdPR"></param>
        /// <returns></returns>
        /// EG 20130613 [18751]
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public static Cst.ErrLevel DeletePosRequest(string pCS, IDbTransaction pDbTransaction, int pIdPR)
        {
            if (StrFunc.IsEmpty(pCS) && (null != pDbTransaction))
                pCS = pDbTransaction.Connection.ConnectionString;

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDPR", DbType.Int32), pIdPR);

            string sqlQuery = @"delete from dbo.POSREQUEST where (IDPR = @IDPR)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            _ = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, ret.Query, ret.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeletePosRequest


        #region DeletePosRequestUnderlyerDelivery
        /// <summary>
        /// Suppression d'un POSREQUEST UNLDLVR
        /// </summary>
        // EG 20170412 [23081] Décompensation des dénouement automatiques avant mise à jour des clôtures (si ADD|UPD|DEL de trades post EOD) 
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public static Cst.ErrLevel DeletePosRequestUnderlyerDelivery(string pCS, IDbTransaction pDbTransaction,
            int pIdTUnderlyer, DateTime pDtBusiness, int pIdTOption)
        {
            if (StrFunc.IsEmpty(pCS) && (null != pDbTransaction))
                pCS = pDbTransaction.Connection.ConnectionString;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdTUnderlyer);
            parameters.Add(new DataParameter(pCS, "IDTOPTION", DbType.Int32), pIdTOption);
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.UnderlyerDelivery));

            string sqlDelete = @"Delete 
                                from dbo.POSREQUEST
                                where (IDT = @IDT) and (REQUESTTYPE = @REQUESTTYPE) and (DTBUSINESS = @DTBUSINESS) and (IDTOPTION = @IDTOPTION)";
            _ = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlDelete, parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeletePosRequest



               
        #region GetQueryProcesEndOfDayInSucces
        /// <summary>
        /// Retourne le dernier ds POSREQUEST de type (REQUESTTYPE = EODMARKET) en succès ou waring
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate">Date de traitement</param>
        /// <param name="pIdEM">id IDEM (entity+Market)</param>
        /// <returns></returns>
        /// EG 20141224 [20566] DTBUSINESS replace DTPOS
        /// FI 20161021 [22152] Modify (Chgt signature et de nome method)
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        public static DataSet GetQueryProcesEndOfDayInSucces(string pCS, IDbTransaction pDbTransaction, DateTime pDate, int pIdEM)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel));
                

            string sqlSelect = GetQueryProcesEndOfDayInSucces();

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);

            // EG 20140204 [19586] Paramètres xxxHint pour gestion instruction /* Spheres:Hint NOPARAMS etc...
            return DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.QueryHint, qryParameters.GetArrayDbParameterHint());
        }
        #endregion GetQueryProcesEndOfDayInSucces

        #region FillPosRequest
        /// <summary>
        /// INSERTION / MISE A JOUR DE LA TABLE POSREQUEST 
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pPosRequest">Représente la demande</param>
        /// <param name="pAppSession">Application à l'origine de la demande</param>
        /// <returns>Code retour</returns>
        public static Cst.ErrLevel FillPosRequest(string pCS, IDbTransaction pDbTransaction, IPosRequest pPosRequest, AppSession pAppSession)
        {
            return FillPosRequest(pCS, pDbTransaction, pPosRequest, pAppSession, null, null);
        }
        /// <summary>
        /// INSERTION / MISE A JOUR DE LA TABLE POSREQUEST 
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pPosRequest">Représente la demande</param>
        /// <param name="pAppSession">Application à l'origine de la demande</param>
        /// <param name="pIdProcess">Id process en cours (utilisé côté service)</param>
        /// <param name="pIdPR_Parent">Id Posrequest parent (utilisé côté service)</param>
        /// <returns>Code retour</returns>
        // EG 20151102 [21465][20979] Refactoring Nullable<int> for newIdPR
        public static Cst.ErrLevel FillPosRequest(string pCS, IDbTransaction pDbTransaction, IPosRequest pPosRequest, AppSession pAppSession,
            Nullable<int> pIdProcess, Nullable<int> pIdPR_Parent)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

            if (StrFunc.IsEmpty(pCS) && (null != pDbTransaction))
                pCS = pDbTransaction.Connection.ConnectionString;
            //
            Nullable<int> newIdPR = GetExistingKeyPosRequest(pCS, pDbTransaction, pPosRequest);

            if (newIdPR.HasValue)
            {
                pPosRequest.SetSource(pAppSession.AppInstance);
                UpdatePosRequest(pCS, pDbTransaction, newIdPR.Value, pPosRequest, pAppSession.IdA, pIdProcess, pIdPR_Parent);
                pPosRequest.IdPR = newIdPR.Value;

            }
            else
            {
                // La demande n'existe pas => on la créée
                // EG 20151102 [21465]
                errLevel = AddNewPosRequest(pCS, pDbTransaction, out int idPR, pPosRequest, pAppSession, pIdProcess, pIdPR_Parent);
                if (Cst.ErrLevel.SUCCESS == errLevel)
                    pPosRequest.IdPR = idPR;
            }
            return errLevel;
        }
        #endregion FillPosRequest

        #region GetAvailableQuantity
        /// <summary>
        /// Retourne la quantité disponible en position sur le trade {pIdT} à une date {pDtPos}
        /// </summary>
        /// <param name="pIdT">Identifiant du trade</param>
        /// <param name="pDtPos">Date de position</param>
        // EG 20150920 [21314] Int (int32) to Long (Int64)
        // EG 20151102 [21465] Refactoring
        // EG 20170127 Qty Long To Decimal
        // EG 20180307 [23769] Gestion dbTransaction
        public static decimal GetAvailableQuantity(string pCS, DateTime pDtBusiness, int pIdT)
        {
            return GetAvailableQuantity(pCS, null, pDtBusiness, pIdT, false, null);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public static decimal GetAvailableQuantity(string pCS, IDbTransaction pDbTransaction, DateTime pDtBusiness, int pIdT)
        {
            return GetAvailableQuantity(pCS, pDbTransaction, pDtBusiness, pIdT, false, null);
        }
        // EG 20150920 [21314] Int (int32) to Long (Int64)
        // EG 20151102 [21465] Refactoring
        // EG 20170127 Qty Long To Decimal
        // EG 20180307 [23769] Gestion dbTransaction
        public static decimal GetAvailableQuantity(string pCS, DateTime pDtBusiness, int pIdT, Nullable<int> pIdPR)
        {
            return GetAvailableQuantity(pCS, null, pDtBusiness, pIdT, false, pIdPR);
        }
        // EG 20151102 [21465] New
        // EG 20170127 Qty Long To Decimal
        // EG 20180307 [23769] Gestion dbTransaction
        //  EG 20180614 [XXXXX] Upd
        public static decimal GetPreviousAvailableQuantity(string pCS, DateTime pDtBusiness, int pIdT)
        {
            return GetPreviousAvailableQuantity(pCS, null, pDtBusiness, pIdT);
        }
        //  EG 20180614 [XXXXX] New
        public static decimal GetPreviousAvailableQuantity(string pCS, IDbTransaction pDbTransaction, DateTime pDtBusiness, int pIdT)
        {
            return GetAvailableQuantity(pCS, pDbTransaction, pDtBusiness, pIdT, true, null);
        }
        // EG 20151102 [21465] Refactoring
        // EG 20170127 Qty Long To Decimal
        // EG 20170412 [23081] Appel à GetQryPosAction_Trade_BySide
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static decimal GetAvailableQuantity(string pCS, IDbTransaction pDbTransaction, DateTime pDtBusiness, int pIdT, bool pIsPrevious, Nullable<int> pIdPR)
        {
            IDbTransaction dbTransaction = pDbTransaction;
            decimal ret = 0;
            bool isException = false;
            bool isExceptCurrentIdPR = pIdPR.HasValue;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness); // FI 20201006 [XXXXX] DbType.Date
                if (isExceptCurrentIdPR)
                    parameters.Add(new DataParameter(pCS, "IDPR", DbType.Int32), pIdPR.Value);

                string sqlSelect = @"select (isnull(tr.QTY,0)  - isnull(pab.QTY,0) - isnull(pas.QTY,0)) as QTY 
                from dbo.TRADE tr 
                left outer join (  " + GetQryPosAction_Trade_BySide(BuyerSellerEnum.BUYER, pIsPrevious, false, isExceptCurrentIdPR) + @") pab  on (pab.IDT = tr.IDT)   
                left outer join (  " + GetQryPosAction_Trade_BySide(BuyerSellerEnum.SELLER, pIsPrevious, false, isExceptCurrentIdPR) + @") pas  on (pas.IDT = tr.IDT)   
                where (tr.IDT = @IDT)";


                if (null == pDbTransaction)
                {
                    // PL 20180312 WARNING: Use Read Commited !
                    //dbTransaction = DataHelper.BeginTran(pCS, IsolationLevel.ReadUncommitted);
                    dbTransaction = DataHelper.BeginTran(pCS, IsolationLevel.ReadCommitted);
                }
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                object obj = DataHelper.ExecuteScalar(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (null != obj)
                    ret = Convert.ToDecimal(obj);

                if (null == pDbTransaction)
                    DataHelper.CommitTran(dbTransaction);
                return ret;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null == pDbTransaction))
                {
                    if (isException) { DataHelper.RollbackTran(dbTransaction); }
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion GetAvailableQuantity
        #region GetAvailableQuantityDtSettlement
        // EG 20190401 [MIGRATIONVCL] New Used By EventsValESE (Quantité en position en Date règlement
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static decimal GetAvailableQuantityDtSettlement(string pCS, IDbTransaction pDbTransaction, DateTime pDtBusiness, int pIdT)
        {
            IDbTransaction dbTransaction = pDbTransaction;
            decimal ret = 0;
            bool isException = false;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness); // FI 20201006 [XXXXX] DbType.Date

                string sqlSelect = @"select (isnull(tr.QTY,0)  - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) as QTY 
                from dbo.TRADE tr 
                left outer join
                (
	                select pad.IDT_BUY as IDT,
                    sum(case when isnull(tr.DTSETTLT,@DTBUSINESS) <= @DTBUSINESS then isnull(pad.QTY,0) else 0 end) as QTY_BUY, 0 as QTY_SELL
	                from dbo.TRADE alloc 
	                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                    left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_SELL) 
                    where ((pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)))
                    and (alloc.IDT = @IDT)
                    group by pad.IDT_BUY

	                union all

	                select pad.IDT_SELL as IDT,
	                0 as QTY_BUY, sum(case when isnull(tr.DTSETTLT,@DTBUSINESS) <= @DTBUSINESS  then isnull(pad.QTY,0) else 0 end) as QTY_SELL
	                from dbo.TRADE alloc 
	                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                    left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_BUY) 
                    where ((pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)))
                    and (alloc.IDT = @IDT) 
                    group by pad.IDT_SELL

                ) pos on (pos.IDT = tr.IDT)
                where (tr.IDT = @IDT)";

                if (null == pDbTransaction)
                {
                    dbTransaction = DataHelper.BeginTran(pCS, IsolationLevel.ReadCommitted);
                }
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                object obj = DataHelper.ExecuteScalar(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (null != obj)
                    ret = Convert.ToDecimal(obj);

                if (null == pDbTransaction)
                    DataHelper.CommitTran(dbTransaction);
                return ret;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null == pDbTransaction))
                {
                    if (isException) { DataHelper.RollbackTran(dbTransaction); }
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion GetAvailableQuantityDtSettlement
        #region GetEntityMarkets
        // EG 20150317 [POC] New Gestion OTC non fongible (IDEM spécifié avec Marché = -1.
        public static DataSet GetEntityMarkets(string pCS, IPosRequest pPosRequest)
        {
            if (pPosRequest.IdEMSpecified)
                return GetEntityMarkets(pCS, pPosRequest.IdEM);
            else if (pPosRequest.IdA_CustodianSpecified)
                return GetEntityMarkets(pCS, pPosRequest.IdA_Entity, null, pPosRequest.IdA_CssCustodian);
            else
                return GetEntityMarkets(pCS, pPosRequest.IdA_Entity, pPosRequest.IdA_CssCustodian, null);
        }
        public static DataSet GetEntityMarkets(string pCS, int? pIdA_Entity, int pIdA_CssCustodian, bool pIsCustodian)
        {
            if (pIsCustodian)
                return GetEntityMarkets(pCS, pIdA_Entity, null, pIdA_CssCustodian);
            else
                return GetEntityMarkets(pCS, pIdA_Entity, pIdA_CssCustodian, null);

        }
        /// EG 20150317 [POC] Add IDEM <> -1
        /// PM 20150422 [20575] Add ENTITYMARKET.DTENTITY, ENTITY.IDBCACCOUNT
        /// EG 20240605 [XXXXX] Exclusion des marchés désactivés
        private static DataSet GetEntityMarkets(string pCS, int? pIdA_Entity, Nullable<int> pIdA_CSS, Nullable<int> pIdA_Custodian)
        {
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), pIdA_Entity ?? Convert.DBNull);
            string sqlSelect = @"select em.IDEM, em.IDM, em.IDA as IDA_ENTITY, em.DTMARKET, em.DTENTITY, em.IDA_CUSTODIAN, mk.IDBC, mk.IDENTIFIER, mk.SHORT_ACRONYM, 
            ac.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER, mk.IDA as IDA_CSS, isnull(e.IDBCACCOUNT, mk.IDBC) as IDBCENTITY, ae.IDENTIFIER as ENTITY_IDENTIFIER
            from dbo.ENTITYMARKET em
            inner join dbo.ENTITY e on (e.IDA = em.IDA)
            inner join dbo.ACTOR ae on (ae.IDA = e.IDA)
            inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = em.IDM)" + Cst.CrLf;

            if (pIdA_CSS.HasValue)
            {
                parameters.Add(new DataParameter(pCS, "IDA_CSS", DbType.Int32), pIdA_CSS.Value);
                sqlSelect += @"and (mk.IDA = @IDA_CSS)
                inner join dbo.ACTOR ac on (ac.IDA = mk.IDA)
                where (em.IDA_CUSTODIAN is null)";
            }
            else if (pIdA_Custodian.HasValue)
            {
                parameters.Add(new DataParameter(pCS, "IDA_CUSTODIAN", DbType.Int32), pIdA_Custodian.Value);
                sqlSelect += @"inner join dbo.ACTOR ac on (ac.IDA = em.IDA_CUSTODIAN)
                where (em.IDA_CUSTODIAN = @IDA_CUSTODIAN)";
            }
            sqlSelect += "and (em.IDA = case when @IDA_ENTITY is null then em.IDA else @IDA_ENTITY end)" + Cst.CrLf;
            // EG 20150317 [POC] New
            sqlSelect += "and (em.IDM != -1) and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk") + Cst.CrLf;
            sqlSelect += "order by ac.IDENTIFIER, mk.SHORT_ACRONYM" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
            // PL 20180312 WARNING: Use Read Commited !
            //return OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadUncommitted, qryParameters, null);
            return OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParameters, null);
        }

        /// EG 20150317 [POC] New
        /// PM 20150422 [20575] Add Add ENTITYMARKET.DTENTITY, ENTITY.IDBCACCOUNT
        /// EG 20240605 [XXXXX] Exclusion des marchés désactivés
        private static DataSet GetEntityMarkets(string pCS, Nullable<int> pIdEM)
        {
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            string sqlSelect = @"select em.IDEM, em.IDM, em.IDA as IDA_ENTITY, em.DTMARKET, em.DTENTITY, em.IDA_CUSTODIAN, mk.IDBC, mk.IDENTIFIER, mk.SHORT_ACRONYM, 
            ac.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER, mk.IDA as IDA_CSS, isnull(e.IDBCACCOUNT, mk.IDBC) as IDBCENTITY, ae.IDENTIFIER as ENTITY_IDENTIFIER
            from dbo.ENTITYMARKET em
            inner join dbo.ENTITY e on (e.IDA = em.IDA)
            inner join dbo.ACTOR ae on (ae.IDA = e.IDA)
            inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = em.IDM)
            inner join dbo.ACTOR ac on (ac.IDA = case when IDA_CUSTODIAN is null then mk.IDA else em.IDA_CUSTODIAN end)
            where (em.IDEM = @IDEM)  and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mk") + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
            // PL 20180312 WARNING: Use Read Commited !
            //return OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadUncommitted, qryParameters, null);
            return OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParameters, null);
        }
        #endregion GetEntityMarkets

        // COMMENTS OK jusque là 

        #region GetDataClosingDayCashFlowsControl
        // EG 20140205 [19572] Ajout de la colonne STATUS (plutôt que 1) dans le select pour utilisation dans traitement CLOSINGDAY
        // EG 20140225 [19575][19666]
        public static DataSet GetDataClosingDayCashFlowsControl(string pCS, DateTime pDate, int pIdEM)
        {
            DataSet ds = null;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTPOS), pDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel));

            string sqlSubSelect = @"select max(prmax.DTUPD)
            from dbo.POSREQUEST prmax
            where (prmax.REQUESTTYPE = @REQUESTTYPE) and (prmax.IDEM = @IDEM) and (prmax.DTBUSINESS = @DTPOS)";

            string sqlSelect = @"select pr.STATUS
            from dbo.POSREQUEST pr
            where (pr.DTBUSINESS = @DTPOS) and (pr.IDEM = @IDEM) and (pr.REQUESTTYPE = @REQUESTTYPE) and
            (pr.STATUS in ('SUCCESS','WARNING')) and (pr.DTUPD = (" + sqlSubSelect + "))" + Cst.CrLf;

            if (StrFunc.IsFilled(sqlSelect))
            {
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                // PL 20180312 WARNING: Use Read Commited !
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParameters, null);
            }
            return ds;
        }
        #endregion GetDataClosingDayCashFlowsControl
        #region GetDataInitialMarginControl
        // EG 20141208 [20439] Add IsolationLevel = ReadUncommitted et réécriture Query CONTROL STEP 3 (suppression Récursivité)
        // RD 20140317 [20832] Modify
        // EG 20170412 [23081] Appel à GetQryPosAction_BySide
        public static DataSet GetDataInitialMarginControl(string pCS, int pStepNumber, int pIdA_Entity, int pIdA_CssCustodian, DateTime pDate)
        {
            DataSet ds = null;
            DataParameters parameters = new DataParameters();
            // RD 20140317 [20832] Renommer le paramètre DTPOS en DTBUSINESS 
            //parameters.Add(new DataParameter(pCS, "DTPOS", DbType.DateTime), pDate);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), pIdA_Entity);
            parameters.Add(new DataParameter(pCS, "IDA_CSSCUSTODIAN", DbType.Int32), pIdA_CssCustodian);

            string sqlSelect = string.Empty;
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            switch (pStepNumber)
            {
                case 0:
                    #region CONTROL STEP 1
                    // Retourne les lignes IMREQUEST autres que SUCCESS/NONE/NA pour un couple  ENTITE/DTBUSINESS
                    // EG 20130627 Retourne les lignes IMREQUEST autres que SUCCESS/WARNING/NONE/NA pour un couple  ENTITE/DTBUSINESS
                    parameters.Add(new DataParameter(pCS, "TIMING", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.EndOfDay));
                    parameters.Add(new DataParameter(pCS, "STSUCCESS", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusSuccess);
                    parameters.Add(new DataParameter(pCS, "STWARNING", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusWarning);
                    parameters.Add(new DataParameter(pCS, "STNONE", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusNone);
                    parameters.Add(new DataParameter(pCS, "STNA", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusUnknown);

                    // RD 20140317 [20832] Renommer le paramètre DTPOS en DTBUSINESS 
                    sqlSelect = @"select 1
                    from dbo.IMREQUEST rq
                    where (rq.IDA_ENTITY = @IDA_ENTITY) and (rq.IDA_CSS = @IDA_CSSCUSTODIAN) and (rq.DTBUSINESS = @DTBUSINESS) " + Cst.CrLf;

                    if (DbSvrType.dbORA == serverType)
                        sqlSelect += @"and (@TIMING = @TIMING) and (@STSUCCESS = @STSUCCESS) and (@STWARNING = @STWARNING) and (@STNONE = @STNONE) and (@STNA = @STNA);" + Cst.CrLf;

                    sqlSelect += @"select rq.IDA_ENTITY, rq.DTBUSINESS, rq.STATUS, rq.IDT
                    from dbo.IMREQUEST rq
                    where (rq.IDA_ENTITY = @IDA_ENTITY) and (rq.IDA_CSS = @IDA_CSSCUSTODIAN) and (rq.DTBUSINESS = @DTBUSINESS) and 
                    (rq.IMTIMING = @TIMING) and (rq.STATUS not in ( @STSUCCESS, @STWARNING, @STNONE, @STNA))" + Cst.CrLf;
                    #endregion CONTROL STEP1
                    break;
                case 1:
                    #region CONTROL STEP 2
                    // Retourne les séries en position en DATE DTBUSINESS
                    // RD 20140317 [20832] Ajouter des parathèses ouvrantes avant les appels à la méthode GetQueryPositionActionBySide
                    sqlSelect = @"select count(tr.IDT)
                    from dbo.VW_TRADE_POSETD tr
                    left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER) + @") pab on (pab.IDT = tr.IDT) 
                    left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER) + @") pas on (pas.IDT = tr.IDT) 
                    where (tr.POSKEEPBOOK_DEALER = 1) and (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN)
                    group by tr.IDM, tr.DTMARKET, tr.IDI, tr.IDASSET, tr.IDA_DEALER, tr.IDB_DEALER,tr.IDA_ENTITYDEALER,
                    tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.IDEM 
                    having 
                    (sum(case when tr.SIDE = 1 then (tr.QTY - isnull(pab.QTY,0)) else 0 end) != 0)
                    or
                    (sum(case when tr.SIDE = 2 then (tr.QTY - isnull(pas.QTY,0)) else 0 end) != 0)";
                    #endregion CONTROL STEP 2
                    break;
                case 2:
                    #region CONTROL STEP 3
                    // EG 20141208 [20439] Réécriture Query CONTROL STEP 3 (suppression Récursivité)
                    // Retourne si un TRT de tenue de position a eu lieu après calcul de DEPOSITS EPOSITS 
                    parameters.Add(new DataParameter(pCS, "TIMING", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.EndOfDay));
                    parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusSuccess);

                    // RD 20140317 [20832] Renommer le paramètre DTPOS en DTBUSINESS 
                    sqlSelect = @"select rq.IDA_ENTITY, rq.IDA_CSS, rq.DTBUSINESS, 'ERROR', rq.IDT, pr.REQUESTTYPE, rq.IDPR, pr.IDPR_POSREQUEST, pr.IDPR
                    from dbo.IMREQUEST rq
                    inner join dbo.POSREQUEST pr on (pr.IDA_ENTITY = rq.IDA_ENTITY) and (pr.IDA_CSSCUSTODIAN = rq.IDA_CSS) and (pr.IDPR_POSREQUEST is null) and
                    (pr.REQUESTTYPE != 'CLOSINGDAY') and (pr.DTBUSINESS = rq.DTBUSINESS)
                    where (rq.DTBUSINESS = @DTBUSINESS) and (rq.IMTIMING = @TIMING) and (rq.STATUS = @STATUS) and (rq.IDA_ENTITY = @IDA_ENTITY) and (rq.IDA_CSS = @IDA_CSSCUSTODIAN) and 
                    (rq.IDA_MRO is null) and (pr.DTINS >= isnull(rq.DTUPD,rq.DTINS))" + Cst.CrLf;

                    #endregion CONTROL STEP 3
                    break;
            }
            if (StrFunc.IsFilled(sqlSelect))
            {
                // EG 20141208 [20439] Add IsolationLevel = ReadUncommitted
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                // PL 20180312 WARNING: Use Read Commited !
                //ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadUncommitted, qryParameters, null);
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParameters, null);
            }
            return ds;
        }
        #endregion GetDataInitialMarginControl
        #region GetDataCashBalanceControl
        public static DataSet GetDataCashBalanceControl(string pCS, DateTime pDate, int pIdEM)
        {
            DataSet ds = null;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTPOS), pDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            parameters.Add(new DataParameter(pCS, "TIMING", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.EndOfDay));
            parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusSuccess);

            string sqlJoinEntityMarket = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ENTITYMARKET + " em " + SQLCst.ON + "(em.IDA = rq.IDA_ENTITY)" + SQLCst.AND + "(em.IDEM = @IDEM)";
            string sqlWhere = SQLCst.WHERE + "(rq.DTBUSINESS = @DTPOS)" + Cst.CrLf;
            sqlWhere += SQLCst.AND + "(rq.CBTIMING = @TIMING)" + Cst.CrLf;


            string sqlSelect = SQLCst.SELECT;
            // Il existe des lignes CBREQUEST pour un couple  ENTITE/DTBUSINESS
            sqlSelect += "1" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBREQUEST + " rq" + Cst.CrLf;
            sqlSelect += sqlJoinEntityMarket + Cst.CrLf + sqlWhere + Cst.CrLf;
            // Il existe des lignes CBREQUEST autre que SUCCESS (donc ERROR) pour un couple  ENTITE/DTBUSINESS
            sqlSelect += SQLCst.SELECT + "rq.IDA_ENTITY, rq.DTBUSINESS, rq.STATUS, rq.IDT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBREQUEST + " rq" + Cst.CrLf;
            sqlSelect += sqlJoinEntityMarket + Cst.CrLf + sqlWhere + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(rq.STATUS != @STATUS)" + Cst.CrLf;

            if (StrFunc.IsFilled(sqlSelect))
                ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
            return ds;
        }
        #endregion GetDataCashBalanceControl

        #region GetStatusEndOfDayProcessControl
        // EG 20170206 [22787]
        public static DataSet GetStatusEndOfDayProcessControl(string pCS, Cst.PosRequestTypeEnum pPosRequestType, DateTime pDate, int pIdEM)
        {
            DataSet ds = null;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTPOS), pDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequestType));

            string sqlSubSelect = @"select max(prmax.DTUPD)
            from dbo.POSREQUEST prmax 
            where (prmax.REQUESTTYPE = @REQUESTTYPE) and (prmax.IDEM = @IDEM) and (prmax.DTBUSINESS = @DTPOS)";

            string sqlSelect = @"select pr.STATUS 
            from dbo.POSREQUEST pr 
            where (pr.DTBUSINESS = @DTPOS) and (pr.IDEM = @IDEM) and (pr.REQUESTTYPE = @REQUESTTYPE) and 
            (pr.STATUS in ('ERROR', 'WARNING')) and pr.DTUPD = (" + sqlSubSelect + ")" + Cst.CrLf;

            if (StrFunc.IsFilled(sqlSelect))
            {
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                // PL 20180312 WARNING: Use Read Commited !
                //ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadUncommitted, qryParameters, null);
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParameters, null);
            }
            return ds;
        }
        #endregion GetStatusEndOfDayProcessControl

        #region GetExistingKeyPosRequest
        /// <summary>
        /// Requête qui retourne l'existence d'une demande dans POSREQUEST (mode Trade)
        /// . CONTRAINTE = Même type de demande + même journée + même pIdT (voir IDPADET pour UNCLEARING)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRequestType">Type de la demande</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pDtBusiness"></param>
        /// FI 20120312 [18467] add pDbTransaction
        // EG 20151102 [21465] Refactoring
        // EG 20170127 Qty Long To Decimal
        // EG 20230929 [WI715][26497] Dénouement manuel + automatique à l'échéance : Passage paramètre RequestType de la source appelante
        public static Nullable<int> GetExistingKeyPosRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, int pIdT, DateTime pDtBusiness)
        {
            // EG 20170127 Qty Long To Decimal
            return GetExistingKeyPosRequest(pCS, pDbTransaction, pRequestType, pIdT, pDtBusiness, out _, out _, out _, out _, null);
        }
        /// <summary>
        /// Requête qui retourne l'existence d'une demande dans POSREQUEST (mode Trade)
        /// . CONTRAINTE = Même type de demande + même journée + même pIdT (voir IDPADET pour UNCLEARING)
        /// RequestTypeSource correspond au requestType de la source appelante (par exemple Exercise|AutomaticExercise|etc... lors de la création d'un POSREQUEST 
        /// pour la livraison du sous-jacent
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRequestType"></param>
        /// <param name="pIdT"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="opIdT"></param>
        /// <param name="opQuantity"></param>
        /// <param name="opNotes"></param>
        /// <param name="opRequestMode"></param>
        /// <param name="pRequestTypeSource">RequestType de la source appelante</param>
        /// <returns></returns>
        // FI 20120312 [18467] add pDbTransaction
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151125 [20465][20979] Refactoring Nullable<int> for returnValue
        // EG 20170127 Qty Long To Decimal
        // EG 20230929 [WI715][26497] Dénouement manuel + automatique à l'échéance : Passage paramètre RequestType de la source appelante
        public static Nullable<int> GetExistingKeyPosRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, int pIdT, DateTime pDtBusiness,
            out Nullable<int> opIdT, out Nullable<decimal> opQuantity, out string opNotes, out string opRequestMode, Nullable<Cst.PosRequestTypeEnum> pRequestTypeSource)
        {
            IProductBase product = Tools.GetNewProductBase();
            IPosRequest posRequest = product.CreatePosRequest();
            posRequest.RequestType = pRequestType;
            posRequest.IdTSpecified = true;
            posRequest.IdT = pIdT;
            posRequest.DtBusiness = pDtBusiness;
            return GetExistingKeyPosRequest(pCS, pDbTransaction, posRequest, out opIdT, out opQuantity, out opNotes, out opRequestMode, pRequestTypeSource);
        }
        /// <summary>
        /// Recherche de l'existance de la demande dans la table POSREQUEST
        /// Utilisé sur les cas de tenue de position en mode UNITAIRE 
        /// (Dénouement d'options, Correction, Décompensation...)
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest">Demande</param>
        /// <returns>IdPR (si trouvé)</returns>
        /// FI 20120312 [18467] add pDbTransaction
        // EG 20151125 [20465][20979] Refactoring Nullable<int> for returnValue
        // EG 20170127 Qty Long To Decimal
        // EG 20230929 [WI715][26497] Dénouement manuel + automatique à l'échéance : Passage paramètre RequestType de la source appelante
        public static Nullable<int> GetExistingKeyPosRequest(string pCS, IDbTransaction pDbTransaction, IPosRequest pPosRequest)
        {
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            return GetExistingKeyPosRequest(pCS, pDbTransaction, pPosRequest, out _, out _, out _, out _, null);
        }
        /// <summary>
        /// Recherche de l'existance de la demande dans la table POSREQUEST
        /// Utilisé sur les cas de tenue de position en mode UNITAIRE 
        /// (Dénouement d'options, Correction, Décompensation...)
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest">Demande</param>
        /// <returns>IdPR (si trouvé)</returns>
        /// FI 20130312 [18467] add pDbTransaction
        /// FI 20130328 [18467] modification defaut pour gérer les cas où IDT is null
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151125 [20465][20979] Refactoring Nullable<int> for returnValue
        // EG 20160121 [21805] POC-MUREX Add IDTOPTION 
        // EG 20170127 Qty Long To Decimal
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180426 Analyse du code Correction [CA2202]
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        // EG 20230929 [WI715][26497] Dénouement manuel + automatique à l'échéance : Passage paramètre RequestType de la source appelante
        // EG 20230929 [WI715][26497] Changement du nom de paramètre (PB XPATH et TransformQuery2 avec Oracle)
        public static Nullable<int> GetExistingKeyPosRequest(string pCS, IDbTransaction pDbTransaction, IPosRequest pPosRequest,
            out Nullable<int> opIdT, out Nullable<decimal> opQuantity, out string opNotes, out string opRequestMode, Nullable<Cst.PosRequestTypeEnum> pRequestTypeSource)
        {
            Nullable<int> idPR = null;
            opIdT = null;
            opQuantity = null;
            opNotes = string.Empty;
            opRequestMode = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.EndOfDay); 
                

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequest.RequestType)); 
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pPosRequest.DtBusiness);

            string sqlSelect = string.Empty;
            switch (pPosRequest.RequestType)
            {
                case Cst.PosRequestTypeEnum.UnClearing:

                    IPosRequestUnclearing posRequest = (IPosRequestUnclearing)pPosRequest;
                    parameters.Add(new DataParameter(pCS, "IDPADET", DbType.Int32), posRequest.Detail.IdPADET);

                    sqlSelect = @"select pr.IDPR, pr.IDT, pr.QTY, pr.NOTES, pr.REQUESTMODE 
                    from dbo.POSREQUEST pr where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.DTBUSINESS = @DTBUSINESS) and " + Cst.CrLf;
                    sqlSelect += DataHelper.GetSQLXQuery_ExistsNode(pCS, "REQUESTDETAIL", "pr", @"//efs:unclearing/efs:idPADET[text()=sql:variable(""@IDPADET"")]", 
                        OTCmlHelper.GetXMLNamespace_3_0(pCS));
                    break;
                case Cst.PosRequestTypeEnum.ClearingSpecific:
                case Cst.PosRequestTypeEnum.PositionTransfer:
                    // EG/FI 20120504 on génère toujours un nouveau POSREQUEST pour ces demandes
                    sqlSelect = string.Empty;
                    break;
                case Cst.PosRequestTypeEnum.UnderlyerDelivery:
                    parameters.Add(new DataParameter(pCS, "IDTOPTION", DbType.Int32), pPosRequest.IdT);
                    sqlSelect = @"select pr.IDPR, pr.IDT, pr.QTY, pr.NOTES, pr.REQUESTMODE, pr.IDTOPTION
                    from dbo.POSREQUEST pr
                    where (pr.REQUESTTYPE=@REQUESTTYPE) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.IDTOPTION = @IDTOPTION)" + Cst.CrLf;
                    // EG 20230929 [WI715][26497] Add
                    // EG 20230929 [WI715][26497] Changement du nom de paramètre (PB XPATH et TransformQuery2 avec Oracle)
                    if (pRequestTypeSource.HasValue)
                    {
                        parameters.Add(new DataParameter(pCS, "REQUESTTYPESRC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pRequestTypeSource.Value));
                        sqlSelect += " and " + Cst.CrLf + DataHelper.GetSQLXQuery_ExistsNode(pCS, "REQUESTDETAIL", "pr", @"//efs:option/efs:underlyer[@requestTypeSource=sql:variable(""@REQUESTTYPESRC"")]", 
                        OTCmlHelper.GetXMLNamespace_3_0(pCS));
                    }
                    break;
                case Cst.PosRequestTypeEnum.OptionAbandon:
                case Cst.PosRequestTypeEnum.OptionNotExercised:
                case Cst.PosRequestTypeEnum.OptionNotAssigned:
                case Cst.PosRequestTypeEnum.OptionAssignment:
                case Cst.PosRequestTypeEnum.OptionExercise:
                    if (pPosRequest.IdTSpecified)
                    {
                        parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pPosRequest.IdT);
                        // EG 20151019 [20465][21112] Add restriction sur STATUS != ERROR|WARNING
                        sqlSelect = @"select pr.IDPR, pr.IDT, pr.QTY, pr.NOTES, pr.REQUESTMODE 
                        from dbo.POSREQUEST pr where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.DTBUSINESS = @DTBUSINESS) and 
                        (pr.STATUS not in ('ERROR','WARNING')) and (0 < pr.QTY) and (isnull(IDT,-1) = @IDT)" + Cst.CrLf;
                    }
                    else if (pPosRequest.PosKeepingKeySpecified)
                    {
                        QueryParameters qryParameters = PosKeepingTools.GetQueryExistingKeyPosRequest(pCS, pPosRequest);
                        sqlSelect = qryParameters.Query;
                        parameters = qryParameters.Parameters;
                    }
                    break;

                default:
                    if (pPosRequest.IdTSpecified)
                    {
                        parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pPosRequest.IdT);

                        sqlSelect = @"select pr.IDPR, pr.IDT, pr.QTY, pr.NOTES, pr.REQUESTMODE 
                        from dbo.POSREQUEST pr where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.DTBUSINESS = @DTBUSINESS) and (isnull(IDT,-1) = @IDT)" + Cst.CrLf;
                    }
                    else if (pPosRequest.PosKeepingKeySpecified)
                    {
                        QueryParameters qryParameters = PosKeepingTools.GetQueryExistingKeyPosRequest(pCS, pPosRequest);
                        sqlSelect = qryParameters.Query;
                        parameters = qryParameters.Parameters;
                    }
                    break;
            }

            if (StrFunc.IsFilled(sqlSelect))
            {
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);

                using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (null != dr && dr.Read())
                    {
                        idPR = Convert.ToInt32(dr["IDPR"]);
                        opIdT = Convert.IsDBNull(dr["IDT"]) ? 0 : Convert.ToInt32(dr["IDT"]);
                        // EG 20150920 [21314]
                        // EG 20170127 Qty Long To Decimal
                        opQuantity = Convert.ToDecimal(dr["QTY"]);
                        opNotes = Convert.ToString(dr["NOTES"]);
                        opRequestMode = Convert.ToString(dr["REQUESTMODE"]);
                    }
                }
            }
            return idPR;
        }
        #endregion GetExistingKeyPosRequest

        #region GetFungibiliyProduct
        public static string GetFungibiliyProduct<T>(T pSource)
        {
            string _gProduct = string.Empty;
            if (pSource is DataRow _row)
            {
                if (_row.Table.Columns.Contains("GPRODUCT"))
                {
                    if (false == Convert.IsDBNull(_row["GPRODUCT"]))
                        _gProduct = _row["GPRODUCT"].ToString();
                }
                else if (_row.Table.Columns.Contains("IDA_CUSTODIAN"))
                {
                    if (Convert.IsDBNull(_row["IDA_CUSTODIAN"]))
                        _gProduct = Cst.ProductGProduct_FUT;
                }
                else
                {
                    _gProduct = Cst.ProductGProduct_FUT;
                }
            }
            else if (pSource is string)
            {
                _gProduct = pSource as string;
            }
            return _gProduct;
        }
        #endregion GetFungibiliyProduct
        #region GetIdAssetNextMaturity
        /// <summary>
        /// Get the id of the asset relative to the closest maturity 
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pIdDC">derivative contract reference, defining the asset search perimeter</param>
        /// <param name="pBusinessDate">date business, the returned maturity is the closest maturity available to that date</param>
        /// <returns></returns>
        public static int GetIdAssetNextMaturity(string pCS, int pIdDC, DateTime pBusinessDate)
        {
            int idAsset = 0;

            using (IDbConnection pConnection = DataHelper.OpenConnection(pCS))
            {

                string sqlrequest = DataContractHelper.GetQuery(DataContractResultSets.GETMINMATURITY);
                CommandType sqlrequesttype = DataContractHelper.GetType(DataContractResultSets.GETMINMATURITY);

                Dictionary<string, object> parameterValues = new Dictionary<string, object>
                {
                    { "IDDC", pIdDC },
                    { "DTBUSINESS", pBusinessDate }
                };


                List<MinMaturityDate> minMaturities =
                    DataHelper<MinMaturityDate>.ExecuteDataSet(pConnection, sqlrequesttype, sqlrequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.GETMINMATURITY, parameterValues));

                if (minMaturities.Count > 0)
                {

                    DateTime minMaturity = (from maturity in minMaturities select maturity.Maturity).FirstOrDefault();

                    sqlrequest = DataContractHelper.GetQuery(DataContractResultSets.GETASSETMINMATURITY);
                    sqlrequesttype = DataContractHelper.GetType(DataContractResultSets.GETASSETMINMATURITY);

                    parameterValues.Clear();
                    parameterValues.Add("IDDC", pIdDC);
                    parameterValues.Add("MINMATURITYDATE", minMaturity);

                    List<AssetMinMaturityDate> assets =
                        DataHelper<AssetMinMaturityDate>.ExecuteDataSet(pConnection, sqlrequesttype, sqlrequest,
                        DataContractHelper.GetDbDataParameters(DataContractResultSets.GETASSETMINMATURITY, parameterValues));

                    idAsset = (from asset in assets select asset.AssetId).FirstOrDefault();
                }
            }

            return idAsset;
        }
        #endregion GetIdAssetNextMaturity


        #region GetQueryOffSettingAndUnclearingDayBySide
        /// <summary>
        /// Requête qui donne les clôtures/compensations du jour unclearées le jour même
        /// </summary>
        /// EG 20130516 New
        public static string GetQueryOffSettingAndUnclearingDayBySide(string pCS, BuyerSellerEnum pBuyerSeller)
        {
            return GetQueryOffSettingAndUnclearingDayBySide(pCS, pBuyerSeller, 0);
        }
        public static string GetQueryOffSettingAndUnclearingDayBySide(string pCS, BuyerSellerEnum pBuyerSeller, int pIdPR)
        {


            string columnIDT = "pad.IDT_" + (pBuyerSeller == BuyerSellerEnum.BUYER ? "BUY" : "SELL");
            //
            string sqlSelect = SQLCst.SELECT + columnIDT + SQLCst.AS + "IDT" + ", sum(" + DataHelper.SQLIsNull(pCS, "pad.QTY", "0") + ") as QTY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSACTION + " pa" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSACTIONDET + " pad" + SQLCst.ON + "(pad.IDPA = pa.IDPA)" + SQLCst.AND + "(pad.DTCAN = @DTBUSINESS)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(pa.DTBUSINESS = @DTBUSINESS)" + Cst.CrLf;

            if (pIdPR > 0)
                sqlSelect += SQLCst.AND + "(" + DataHelper.SQLIsNull(pCS, "IDPR", "0") + "<> @IDPR)";
            sqlSelect += SQLCst.GROUPBY + columnIDT + Cst.CrLf;
            return sqlSelect;

        }
        #endregion GetQueryOffSettingAndUnclearingDayBySide


        #region GetQueryQuantityUncleared
        public static string GetQueryQuantityUncleared(string pCS)
        {
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "ev.IDT, sum(" + DataHelper.SQLIsNull(pCS, "ev.VALORISATION", "0") + ") as QTY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + SQLCst.ON + "(ec.IDE = ev.IDE)" + SQLCst.AND + "(ec.DTEVENT=@DTBUSINESS)";
            sqlSelect += SQLCst.WHERE + "(ev.IDSTACTIVATION=" + DataHelper.SQLString(Cst.StatusActivation.REGULAR.ToString()) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ev.EVENTCODE = " + DataHelper.SQLString(EventCodeFunc.UnclearingOffsetting) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "ev.IDT";
            return sqlSelect.ToString();
        }
        #endregion GetQueryQuantityUncleared
        #region GetQueryQuantityCorrected
        public static string GetQueryQuantityCorrected(string pCS)
        {
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "ev.IDT, sum(" + DataHelper.SQLIsNull(pCS, "ev.VALORISATION", "0") + ") as QTY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + SQLCst.ON + "(ec.IDE = ev.IDE)" + SQLCst.AND + "(ec.DTEVENT=@DTBUSINESS)";
            sqlSelect += SQLCst.WHERE + "(ev.IDSTACTIVATION=" + DataHelper.SQLString(Cst.StatusActivation.REGULAR.ToString()) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ev.EVENTCODE" + SQLCst.IN + "(";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.PositionCancelation) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.PositionTransfer) + "))" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "ev.IDT";
            return sqlSelect.ToString();
        }
        #endregion GetQueryQuantityCorrected
        #region GetQueryQuantityCorrectedAndUnclearing
        public static string GetQueryQuantityCorrectedAndUnclearing(string pCS)
        {
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "ev.IDT, sum(case when ev.EVENTCODE = 'UOF' then -1 else 1 end * " + DataHelper.SQLIsNull(pCS, "ev.VALORISATION", "0") + ") as QTY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + SQLCst.ON + "(ec.IDE = ev.IDE)" + SQLCst.AND + "(ec.DTEVENT=@DTBUSINESS)";
            sqlSelect += SQLCst.WHERE + "(ev.IDSTACTIVATION=" + DataHelper.SQLString(Cst.StatusActivation.REGULAR.ToString()) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ev.EVENTCODE" + SQLCst.IN + "(";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.PositionCancelation) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.PositionTransfer) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.UnclearingOffsetting) + "))" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "ev.IDT";
            return sqlSelect.ToString();
        }
        #endregion GetQueryQuantityCorrectedAndUnclearing

        #region GetQueryPositionActionBySide
        
        





        /// <summary>
        /// get a query string in order to load the posactions minor or equals to a given date (the date value is related to @DTBUSINESS/@DTBUSINESSINVARIANT)
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pBuyerSeller">buyer seller flag, 
        /// when "BUYER" we load the posactions related to the trades where the dealer has a long position</param>
        /// <param name="pIdPR">optional parameter, 
        /// optional filter to get just the posactions for a specific request Id; "-1" to get them all</param>
        /// <param name="pSettlSess">"IntraDay" or "EndOfDay", 
        /// when "IntraDay" all the posactions of the day will be loaded</param>
        /// <param name="pIntraDayExeAssCtrlActivated">Used just for the "IntraDay" mode, 
        /// when true some posactions types (exe/ass) could be excluded when they occured during the current business day (depending on @DTPOS/@DTPOSINVARIANT values)</param>
        /// <returns></returns>
        // EG 20141224 [20566] Refactoring replace DEPRECATED_GetQueryPositionActionBySide
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public static string GetQueryPositionActionBySide(string pCS, BuyerSellerEnum pBuyerSeller, Nullable<int> pIdPR, SettlSessIDEnum pSettlSess, bool pIntraDayExeAssCtrlActivated)
        {
            string sqlSelect = @"select pad.{0} as IDT, sum(isnull(pad.QTY,0)) as QTY  
            from dbo.POSACTIONDET pad
            inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)" + Cst.CrLf;

            // 1 get the pos actions for a specific datetime...

            // 1.1 IntraDay including exe/ass for the given @DTPOS ( @DTPOS, with or without hours and minutes)
            if (pSettlSess == SettlSessIDEnum.Intraday && !pIntraDayExeAssCtrlActivated)
            {
                sqlSelect += @"where ({1} <= @DTBUSINESS)";
            }
            // 1.2 IntraDay excluding exe/ass for the given @DTPOS ( @DTPOS, with or without hours and minutes)
            else if (pSettlSess == SettlSessIDEnum.Intraday && pIntraDayExeAssCtrlActivated)
            {
                sqlSelect += @"where 
                (pr.REQUESTTYPE not in ('ABN', 'NEX', 'NAS', 'ASS', 'EXE', 'MOF', 'AUTOABN', 'AUTOASS', 'AUTOEXE') and ({1} <= @DTBUSINESSINVARIANT))
                or 
                (pr.REQUESTTYPE in ('ABN', 'NEX', 'NAS', 'ASS', 'EXE', 'MOF', 'AUTOABN', 'AUTOASS', 'AUTOEXE') and ({1} <= @DTBUSINESS))";
            }
            // 1.3 EndOfDay
            else
            {
                // EG 20150107 Replace {1} by pa.DTBUSINESS
                sqlSelect += @"where (pa.DTBUSINESS <= @DTBUSINESS)";
            }

            if (pIdPR.HasValue && pIdPR.Value > 0)
                sqlSelect += "and (isnull(pa.IDPR, 0) <> @IDPR)";

            sqlSelect = String.Format(sqlSelect + Cst.CrLf + "group by pad.{0}",
                (pBuyerSeller == BuyerSellerEnum.BUYER ? "IDT_BUY" : "IDT_SELL"),
                DataHelper.SQLAddTime(pCS, "pa.DTBUSINESS", "pad.DTINS"));

            return sqlSelect;

        }
        #endregion GetQueryPositionActionBySide

        #region GetQueryPositionActionBySideForDtEntity
        /// <summary>
        /// Retourne la requête de sélection des quantités réellement cloturées (Acheteuses ou Vendeuses) 
        /// . pour un asset donné
        /// . à la date ENTITYMARKET.DTENTITY 
        /// . non annulée        
        /// La jointure sur VW_TRADE_POSETD permet de récupérer la date ENTITYMARKET.DTENTITY
        /// Actuellement cette requête est utilisée uniquement dans le contexte de modification du Multiplier sur le DC ou l'ASSET_ETD 
        /// </summary>
        /// <returns></returns>
        /// PM 20151027 [21491] New : Copie de GetQueryPositionActionBySideForDtMarket en utilisant DTENTITY au lieu de DTMARKET
        public static string GetQueryPositionActionBySideForDtEntity(string pCS, BuyerSellerEnum pBuyerSeller)
        {
            return GetQueryPositionActionBySideForDtEntity(pCS, pBuyerSeller, -1, SettlSessIDEnum.EndOfDay);
        }
        public static string GetQueryPositionActionBySideForDtEntity(string pCS, BuyerSellerEnum pBuyerSeller, int pIdPR, SettlSessIDEnum pSettlSess)
        {
            string columnIDT = "pad.IDT_" + (pBuyerSeller == BuyerSellerEnum.BUYER ? "BUY" : "SELL");
            //
            string sqlSelect = SQLCst.SELECT + columnIDT + SQLCst.AS + "IDT" + ", sum(" + DataHelper.SQLIsNull(pCS, "pad.QTY", "0") + ") as QTY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSACTION + " pa" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSACTIONDET + " pad" + SQLCst.ON + "(pad.IDPA = pa.IDPA)";
            // Pour accéder à la date ENTITYMARKET.DTENTITY
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_TRADE_POSETD + " tr" + SQLCst.ON + "(tr.IDT = " + columnIDT + ")";
            //
            if (pSettlSess == SettlSessIDEnum.EndOfDay)

                sqlSelect += SQLCst.WHERE + "(pa.DTBUSINESS <= tr.DTENTITY)" + Cst.CrLf;

            else

                sqlSelect += SQLCst.WHERE +
                    String.Format("({0} <= tr.DTENTITY)", DataHelper.SQLAddTime(pCS, "pa.DTBUSINESS", "pad.DTINS"))
                    + Cst.CrLf;
            //
            if (pIdPR > 0)
                sqlSelect += SQLCst.AND + "(" + DataHelper.SQLIsNull(pCS, "IDPR", "0") + "<> @IDPR)";
            //
            sqlSelect += SQLCst.AND + "((pad.DTCAN is null)" + SQLCst.OR + "(pad.DTCAN > tr.DTENTITY))" + Cst.CrLf;
            //
            sqlSelect += SQLCst.GROUPBY + columnIDT + Cst.CrLf;
            //
            return sqlSelect;
        }
        #endregion GetQueryPositionActionBySideForDtEntity
        #region GetQueryProcesEndOfDayInSucces
        
        /// <summary>
        /// Query qui retourne le dernier status d'un traitemenent EOD (paramèters de la requête @REQUESTTYPE, @IDEM, @DTBUSINESS)
        /// <para>La requête considère uniquement les traitements EOD terminés en SUCCESS ou WARNING</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20140120 Report 3.7
        /// EG 20141224 [20566] Refactoring
        /// EG 20190812 [24825] Test Aucune activité sur CSS/CUSTODIAN
        public static string GetQueryProcesEndOfDayInSucces()
        {
            string sqlSelect = @"select pr.STATUS
            from dbo.POSREQUEST pr
            where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.STATUS in ('SUCCESS','WARNING','NONE')) and  
            (pr.DTUPD = (select max(prmax.DTUPD)
                         from dbo.POSREQUEST prmax
                         where (prmax.REQUESTTYPE = @REQUESTTYPE) and (prmax.IDEM = @IDEM) and (prmax.DTBUSINESS = @DTBUSINESS)))";
            return sqlSelect;
        }
        #endregion GetQueryProcesEndOfDayInSucces

        #region GetPosRequest
        /// <summary>
        /// retourne une instance de type POSREQUEST en fonction d'un IDPR donné
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct">Product</param>
        /// <param name="pIdPR">Identifiant POSREQUEST</param>
        public static IPosRequest GetPosRequest(string pCS, IProductBase pProduct, int pIdPR)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                // PL 20180312 WARNING: Use Read Commited !
                //dbTransaction = DataHelper.BeginTran(pCS, IsolationLevel.ReadUncommitted);
                dbTransaction = DataHelper.BeginTran(pCS, IsolationLevel.ReadCommitted);
                IPosRequest posRequest = GetPosRequest(pCS, dbTransaction, pProduct, pIdPR);
                //PL 20151229 Use DataHelper.CommitTran()
                //dbTransaction. Commit();
                DataHelper.CommitTran(dbTransaction);
                return posRequest;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException) { DataHelper.RollbackTran(dbTransaction); }
                    dbTransaction.Dispose();
                }
            }
            //return GetPosRequest(pCS, null, pProduct, pIdPR);
        }

        /// <summary>
        /// retourne une instance de type POSREQUEST en fonction d'un IDPR donné
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct">Product</param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPR">Identifiant POSREQUEST</param>
        /// PM 20130218 [18414] Add Cst.PosRequestTypeEnum.Cascading et Cst.PosRequestTypeEnum.Shifting en utilisant CreatePosRequestMaturityOffsetting
        /// FI 20130312 [18467] Add pDbTransaction parameter (parce que Spheres® insère désormais dans POSREQUEST dans le cadre d'une transaction SQL)
        /// FI 20130305 [18467] Add CreatePosRequestPositionOption
        /// FI 20130318 [18467] IDA_DEALER,IDB_DEALER peuvent être null dans POSREQUEST (cas des assignations par position, c'est uniquement une fois les trades identifiés que Spheres® sera en mesure de connaître le dealer)
        /// FI 20130319 [18467] Alimentation de POSREQUEST.SOURCE
        /// FI 20130327 [18467] Alimentation de POSREQUEST.EXTLLINK
        /// FI 20130327 [18467] Alimentation de POSREQUEST.IDPROCESS_L_SOURCE
        /// EG 20130607 [18740] Add RemoveCAExecuted
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20190926 [Maturity Redemption] Add MaturityRedemptionOffsettingDebtSecurity
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        public static IPosRequest GetPosRequest(string pCS, IDbTransaction pDbTransaction, IProductBase pProduct, int pIdPR)
        {
            IPosRequest posRequest = null;
            QueryParameters sqlSelect = GetQueryPosRequest(pCS, pIdPR);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, sqlSelect.Query, sqlSelect.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    bool isIdT = (false == Convert.IsDBNull(dr["IDT"]));

                    Cst.PosRequestTypeEnum requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(dr["REQUESTTYPE"].ToString());
                    switch (requestType)
                    {
                        case Cst.PosRequestTypeEnum.ClosingDay:
                            posRequest = pProduct.CreatePosRequestClosingDAY();
                            break;
                        case Cst.PosRequestTypeEnum.ClearingEndOfDay:
                            posRequest = pProduct.CreatePosRequestClearingEOD();
                            break;
                        case Cst.PosRequestTypeEnum.ClearingBulk:
                            posRequest = pProduct.CreatePosRequestClearingBLK();
                            break;
                        case Cst.PosRequestTypeEnum.ClearingSpecific:
                            posRequest = pProduct.CreatePosRequestClearingSPEC();
                            break;
                        case Cst.PosRequestTypeEnum.Closure:
                            break;
                        case Cst.PosRequestTypeEnum.EndOfDay:
                        case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                            posRequest = pProduct.CreatePosRequestEOD();
                            break;
                        case Cst.PosRequestTypeEnum.RemoveEndOfDay:
                            posRequest = pProduct.CreatePosRequestREMOVEEOD();
                            break;
                        case Cst.PosRequestTypeEnum.Cascading:
                        case Cst.PosRequestTypeEnum.Shifting:
                        case Cst.PosRequestTypeEnum.MaturityOffsettingFuture:
                        case Cst.PosRequestTypeEnum.MaturityOffsettingOption:
                        case Cst.PosRequestTypeEnum.MaturityRedemptionOffsettingDebtSecurity:
                            posRequest = pProduct.CreatePosRequestMaturityOffsetting();
                            break;
                        case Cst.PosRequestTypeEnum.OptionAbandon:
                        case Cst.PosRequestTypeEnum.OptionNotExercised:
                        case Cst.PosRequestTypeEnum.OptionNotAssigned:
                        case Cst.PosRequestTypeEnum.OptionAssignment:
                        case Cst.PosRequestTypeEnum.OptionExercise:
                            if (isIdT)
                                posRequest = pProduct.CreatePosRequestOption();
                            else
                                posRequest = pProduct.CreatePosRequestPositionOption();
                            break;

                        case Cst.PosRequestTypeEnum.UnderlyerDelivery:
                        case Cst.PosRequestTypeEnum.AutomaticOptionAbandon:
                        case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                        case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                            posRequest = pProduct.CreatePosRequestOption();
                            break;

                        case Cst.PosRequestTypeEnum.PositionCancelation:
                            posRequest = pProduct.CreatePosRequestCorrection();
                            break;
                        case Cst.PosRequestTypeEnum.PositionInsertion:
                            break;
                        case Cst.PosRequestTypeEnum.PositionTransfer:
                            posRequest = pProduct.CreatePosRequestTransfer();
                            break;
                        case Cst.PosRequestTypeEnum.RemoveAllocation:
                            posRequest = pProduct.CreatePosRequestRemoveAlloc();
                            break;
                        case Cst.PosRequestTypeEnum.RemoveCAExecuted:
                            posRequest = pProduct.CreatePosRequestRemoveCAExecuted();
                            break;
                        case Cst.PosRequestTypeEnum.UpdateEntry:
                            posRequest = pProduct.CreatePosRequestUpdateEntry();
                            break;
                        case Cst.PosRequestTypeEnum.UnClearing:
                            posRequest = pProduct.CreatePosRequestUnclearing();
                            break;
                        case Cst.PosRequestTypeEnum.TradeSplitting:
                            posRequest = pProduct.CreatePosRequestSplit();
                            break;
                    }

                    if (null != posRequest)
                    {
                        posRequest.IdPR = pIdPR;
                        posRequest.RequestType = requestType;
                        posRequest.RequestMode = (SettlSessIDEnum)ReflectionTools.EnumParse(new SettlSessIDEnum(), dr["REQUESTMODE"].ToString());

                        posRequest.IdA_Entity = Convert.ToInt32(dr["IDA_ENTITY"]);
                        posRequest.IdA_CssCustodian = Convert.ToInt32(dr["IDA_CSSCUSTODIAN"]);

                        posRequest.IdA_CssSpecified = (false == Convert.IsDBNull(dr["IDA_CSS"]));
                        if (posRequest.IdA_CssSpecified)
                            posRequest.IdA_Css = Convert.ToInt32(dr["IDA_CSS"]);

                        posRequest.IdA_CustodianSpecified = (false == Convert.IsDBNull(dr["IDA_CUSTODIAN"]));
                        if (posRequest.IdA_CustodianSpecified)
                            posRequest.IdA_Custodian = Convert.ToInt32(dr["IDA_CUSTODIAN"]);

                        posRequest.IdEMSpecified = (false == Convert.IsDBNull(dr["IDEM"]));
                        if (posRequest.IdEMSpecified)
                            posRequest.IdEM = Convert.ToInt32(dr["IDEM"]);

                        // FI 20130917 [18953] alimentation de idM 
                        posRequest.IdMSpecified = (false == Convert.IsDBNull(dr["IDM"]));
                        if (posRequest.IdMSpecified)
                            posRequest.IdM = Convert.ToInt32(dr["IDM"]);

                        posRequest.IdCESpecified = (false == Convert.IsDBNull(dr["IDCE"]));
                        if (posRequest.IdCESpecified)
                            posRequest.IdCE = Convert.ToInt32(dr["IDCE"]);

                        posRequest.DtBusiness = Convert.ToDateTime(dr["DTBUSINESS"]);

                        posRequest.IdPR_PosRequestSpecified = (false == Convert.IsDBNull(dr["IDPR_POSREQUEST"]));
                        if (posRequest.IdPR_PosRequestSpecified)
                            posRequest.IdPR_PosRequest = Convert.ToInt32(dr["IDPR_POSREQUEST"]);

                        posRequest.PosKeepingKeySpecified = (false == Convert.IsDBNull(dr["IDASSET"]));
                        if (posRequest.PosKeepingKeySpecified)
                        {
                            int idI = Convert.ToInt32(dr["IDI"]);
                            Nullable<Cst.UnderlyingAsset> _underlyingAsset = GetUnderLyingAssetRelativeToInstrument(dr);

                            int idAsset = Convert.ToInt32(dr["IDASSET"]);

                            //Dealer
                            int idA_Dealer = 0;
                            if (false == Convert.IsDBNull(dr["IDA_DEALER"]))
                                idA_Dealer = Convert.ToInt32(dr["IDA_DEALER"]);
                            int idB_Dealer = 0;
                            if (false == Convert.IsDBNull(dr["IDB_DEALER"]))
                                idB_Dealer = Convert.ToInt32(dr["IDB_DEALER"]);
                            int idA_EntityDealer = 0;
                            if (false == Convert.IsDBNull(dr["IDA_ENTITYDEALER"]))
                                idA_EntityDealer = Convert.ToInt32(dr["IDA_ENTITYDEALER"]);

                            //Clearer
                            int idA_Clearer = Convert.ToInt32(dr["IDA_CLEARER"]);
                            int idB_Clearer = Convert.ToInt32(dr["IDB_CLEARER"]);
                            int idA_EntityClearer = 0;
                            if (false == Convert.IsDBNull(dr["IDA_ENTITYCLEARER"]))
                                idA_EntityClearer = Convert.ToInt32(dr["IDA_ENTITYCLEARER"]);

                            posRequest.SetPosKey(idI, _underlyingAsset, idAsset, idA_Dealer, idB_Dealer, idA_Clearer, idB_Clearer);
                            posRequest.SetAdditionalInfo(idA_EntityDealer, idA_EntityClearer);
                        }

                        posRequest.IdTSpecified = (false == Convert.IsDBNull(dr["IDT"]));
                        if (posRequest.IdTSpecified)
                            posRequest.IdT = Convert.ToInt32(dr["IDT"]);

                        posRequest.QtySpecified = (false == Convert.IsDBNull(dr["QTY"]));
                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        if (posRequest.QtySpecified)
                            posRequest.Qty = Convert.ToDecimal(dr["QTY"]);

                        posRequest.NotesSpecified = (false == Convert.IsDBNull(dr["NOTES"]));
                        if (posRequest.NotesSpecified)
                            posRequest.Notes = dr["NOTES"].ToString();

                        posRequest.IdAIns = Convert.ToInt32(dr["IDAINS"]);
                        // FI 20200820 [25468] Dates systemes en UTC
                        posRequest.DtIns = DateTime.SpecifyKind(Convert.ToDateTime(dr["DTINS"] ), DateTimeKind.Utc);
                        if (false == Convert.IsDBNull(dr["IDAUPD"]))
                            posRequest.IdAUpd = Convert.ToInt32(dr["IDAUPD"]);
                        if (false == Convert.IsDBNull(dr["DTUPD"]))
                        {
                            // FI 20200820 [25468] Dates systemes en UTC
                            posRequest.DtUpd = DateTime.SpecifyKind(Convert.ToDateTime(dr["DTUPD"]), DateTimeKind.Utc);
                        }

                        posRequest.IdProcessLSpecified = (false == Convert.IsDBNull(dr["IDPROCESS_L"]));
                        if (posRequest.IdProcessLSpecified)
                            posRequest.IdProcessL = Convert.ToInt32(dr["IDPROCESS_L"]);

                        posRequest.SourceSpecified = (false == Convert.IsDBNull(dr["SOURCE"]));
                        if (posRequest.SourceSpecified)
                            posRequest.Source = dr["SOURCE"].ToString();

                        posRequest.SourceIdProcessLSpecified = (false == Convert.IsDBNull(dr["IDPROCESS_L_SOURCE"]));
                        if (posRequest.SourceIdProcessLSpecified)
                            posRequest.SourceIdProcessL = Convert.ToInt32(dr["IDPROCESS_L_SOURCE"]);

                        posRequest.ExtlLinkSpecified = (false == Convert.IsDBNull(dr["EXTLLINK"]));
                        if (posRequest.ExtlLinkSpecified)
                            posRequest.ExtlLink = dr["EXTLLINK"].ToString();

                        if (false == Convert.IsDBNull(dr["REQUESTDETAIL"]))
                        {
                            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(posRequest.DetailType, dr["REQUESTDETAIL"].ToString());
                            serializeInfo.SetPosRequestTradeInfo(EfsMLDocumentVersionEnum.Version30);
                            serializeInfo.AddNameSerializerNamespaces();
                            object obj = CacheSerializer.Deserialize(serializeInfo);
                            posRequest.DetailBase = obj;
                        }
                    }
                }
            }
            return posRequest;
        }
        #endregion GetPosRequest
        #region GetQueryPosRequest
        /// <summary>
        /// Retourne la query nécessaire à l'alimentation d'un IPOSREQUEST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdPR"></param>
        /// <returns></returns>
        /// FI 20130319 [18467] add column SOURCE
        /// FI 20130327 [18467] add column EXTLLINK (identification externe d'une ASS,EXE,ABN sur position)
        /// FI 20130327 [18467] add column IDPROCESS_L_SOURCE (log du POSREQUEST importé) 
        /// FI 20130328 [18467] add isnull sur la colonne entity (sur les assignations il n'y a pas de book dealer)
        /// FI 20130408 [18467] add column IDPROCESS_L
        /// EG 20140225 [19575][19666]
        private static QueryParameters GetQueryPosRequest(string pCS, int pIdPR)
        {
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "pr.IDPR, pr.IDA_ENTITY, pr.IDA_CSSCUSTODIAN, pr.IDA_CSS, pr.IDA_CUSTODIAN, pr.IDEM, pr.IDCE, pr.REQUESTTYPE, pr.REQUESTMODE, pr.DTBUSINESS," + Cst.CrLf;
            sqlSelect += "pr.IDI, pr.IDASSET, pr.IDA_DEALER, pr.IDB_DEALER, pr.IDA_CLEARER, pr.IDB_CLEARER, " + Cst.CrLf;
            sqlSelect += "pr.IDT, pr.QTY, pr.REQUESTDETAIL, pr.IDPROCESS_L, pr.DTINS, pr.IDAINS, " + Cst.CrLf;
            sqlSelect += "pr.IDAUPD, pr.DTUPD, pr.REQUESTDETAIL, pr.NOTES, pr.STATUS, pr.IDPR_POSREQUEST," + Cst.CrLf;
            sqlSelect += "pr.EXTLLINK," + Cst.CrLf;
            sqlSelect += "pr.IDPROCESS_L," + Cst.CrLf;
            sqlSelect += "pr.SOURCE,pr.IDPROCESS_L_SOURCE," + Cst.CrLf;
            sqlSelect += "isnull(bd.IDA_ENTITY,pr.IDA_ENTITY) as IDA_ENTITYDEALER," + Cst.CrLf;
            sqlSelect += "bc.IDA_ENTITY as IDA_ENTITYCLEARER, em.IDM, ns.ASSETCATEGORY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSREQUEST + " pr" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.VW_INSTR_PRODUCT + " ns on (ns.IDI = pr.IDI)" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ENTITYMARKET + " em on (em.IDEM = pr.IDEM)" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK + " bd" + SQLCst.ON + "(bd.IDB = pr.IDB_DEALER)" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK + " bc" + SQLCst.ON + "(bc.IDB = pr.IDB_CLEARER)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(pr.IDPR = @IDPR)" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDPR), pIdPR);
            QueryParameters ret = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            return ret;
        }
        #endregion GetQueryPosRequest
        #region GetMoneyPosition
        // EG 20151102 [21465] New
        public static string GetMoneyPosition(PutOrCallEnum pPutOrCall, decimal pStrike, decimal pQuote)
        {
            return GetMoneyPosition(GetMoneyPositionEnum(pPutOrCall, pStrike, pQuote));
        }
        public static string GetMoneyPosition(MoneyPositionEnum pMoneyPosition)
        {
            string moneyPosition = System.Enum.GetName(typeof(MoneyPositionEnum), pMoneyPosition);
            Regex regexMoneyPosition = new Regex("(In|(Out)(Of)|At){1}(The){1}(Money){1}", RegexOptions.IgnoreCase);
            Match match = regexMoneyPosition.Match(moneyPosition);
            if (match.Success)
            {
                if (match.Groups[2].Value == String.Empty)
                    moneyPosition = regexMoneyPosition.Replace(moneyPosition, "$1 $4 $5");
                else
                    moneyPosition = regexMoneyPosition.Replace(moneyPosition, "$2 $3 $4 $5");
            }
            return moneyPosition;
        }
        #endregion GetMoneyPosition
        #region GetMoneyPositionEnum
        public static MoneyPositionEnum GetMoneyPositionEnum(PutOrCallEnum pPutOrCall, decimal pStrike, decimal pQuote)
        {
            MoneyPositionEnum ret = MoneyPositionEnum.AtTheMoney;
            switch (pPutOrCall)
            {
                // Put
                case PutOrCallEnum.Put:

                    if (pStrike > pQuote)
                        ret = MoneyPositionEnum.InTheMoney;
                    else if (pStrike < pQuote)
                        ret = MoneyPositionEnum.OutOfTheMoney;
                    break;

                // Call
                case PutOrCallEnum.Call:

                    if (pStrike < pQuote)
                        ret = MoneyPositionEnum.InTheMoney;
                    else if (pStrike > pQuote)
                        ret = MoneyPositionEnum.OutOfTheMoney;
                    break;

                default:
                    ret = MoneyPositionEnum.Unknown;
                    break;
            }
            return ret;
        }
        #endregion GetMoneyPositionEnum

        #region IsInTheMoney
        public static bool IsInTheMoney(decimal pSpot, decimal pStrike, decimal pQuote, decimal pTick)
        {
            throw new NotImplementedException();
        }
        #endregion IsInTheMoney
        #region InsertPosRequest
        /// <summary>
        /// Insertion d'une demande de traitement dans POSREQUEST
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIdPR">Identifiant de la demande POSREQUEST</param>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pAppInstance">Application /Acteur à l'origine de la demande</param>
        /// FI 20130318 [18467] parameter pAppInstance
        public static Cst.ErrLevel InsertPosRequest(string pCS, int pIdPR, IPosRequest pPosRequest, AppSession appSession)
        {
            return InsertPosRequest(pCS, null, pIdPR, pPosRequest, appSession, null, null);
        }
        /// <summary>
        /// Insertion d'une demande de traitement dans POSREQUEST
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdPR">Identifiant de la demande POSREQUEST</param>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pAppInstance">Application /Acteur à l'origine de la demande</param>
        /// <param name="pIdProcess_L">Identifiant du PROCESS</param>
        /// <param name="pIdPR_Parent">Identifiant de la demande POSREQUEST parente</param>
        /// FI 20130318 [18467] parameter pAppInstance 
        /// FI 20130327 [18467] add colum IDPROCESS_L_SOURCE, EXTLLINK
        /// EG 20160121 [21805] POC-MUREX Add IDTOPTION 
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public static Cst.ErrLevel InsertPosRequest(string pCS, IDbTransaction pDbTransaction, int pIdPR, IPosRequest pPosRequest, AppSession appSession,
            Nullable<int> pIdProcess_L, Nullable<int> pIdPR_Parent)
        {
            if (StrFunc.IsEmpty(pCS) && (null != pDbTransaction))
                pCS = pDbTransaction.Connection.ConnectionString;

            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.POSREQUEST.ToString();
            sqlQuery += "(IDPR, IDA_ENTITY, IDA_CSSCUSTODIAN, IDA_CSS, IDA_CUSTODIAN, IDEM, IDCE, REQUESTTYPE, REQUESTMODE, DTBUSINESS, IDI, IDASSET, IDA_DEALER, IDB_DEALER, ";
            sqlQuery += "IDA_CLEARER, IDB_CLEARER, IDT, QTY, REQUESTDETAIL, DTINS, IDAINS, NOTES, IDPROCESS_L, IDPR_POSREQUEST, STATUS, SOURCE, EXTLLINK, IDPROCESS_L_SOURCE, IDTOPTION, GPRODUCT )";
            sqlQuery += "values (@IDPR, @IDA_ENTITY, @IDA_CSSCUSTODIAN, @IDA_CSS, @IDA_CUSTODIAN, @IDEM, @IDCE, @REQUESTTYPE, @REQUESTMODE, @DTBUSINESS, @IDI, @IDASSET, @IDA_DEALER, @IDB_DEALER, ";
            sqlQuery += "@IDA_CLEARER, @IDB_CLEARER, @IDT, @QTY, @REQUESTDETAIL, @DTINS, @IDAINS, @NOTES, @IDPROCESS_L, @IDPR_POSREQUEST, @STATUS, @SOURCE, @EXTLLINK, @IDPROCESS_L_SOURCE, @IDTOPTION, @GPRODUCT)";

            DataParameters parameters = new DataParameters();
            Cst.ErrLevel errLevel = AddPosKeepingKeyParameters(pCS, parameters, pPosRequest);
            Nullable<int> idEM = null;
            if (pPosRequest.IdEMSpecified && (0 != pPosRequest.IdEM))
                idEM = pPosRequest.IdEM;

            Nullable<int> idA_Css = null;
            if (pPosRequest.IdA_CssSpecified && (0 != pPosRequest.IdA_Css))
                idA_Css = pPosRequest.IdA_Css;

            Nullable<int> idA_Custodian = null;
            if (pPosRequest.IdA_CustodianSpecified && (0 != pPosRequest.IdA_Custodian))
                idA_Custodian = pPosRequest.IdA_Custodian;

            Nullable<int> idCE = null;
            if (pPosRequest.IdCESpecified && (0 != pPosRequest.IdCE))
                idCE = pPosRequest.IdCE;

            pPosRequest.DtIns = OTCmlHelper.GetDateSysUTC(pCS);
            pPosRequest.SourceSpecified = true;

            // FI 20140401 [19804] utilisation de la property serviceName
            string source = appSession.AppInstance.AppNameVersion;
            if (appSession.AppInstance.GetType().Equals(typeof(AppInstanceService)))
                source = ((AppInstanceService)appSession.AppInstance).ServiceName;
            pPosRequest.Source = source;

            Nullable<int> idTOption = null;
            if ((pPosRequest is IPosRequestOption) && pPosRequest.IdTSpecified &&
                (pPosRequest.RequestType == Cst.PosRequestTypeEnum.UnderlyerDelivery))
            {
                idTOption = pPosRequest.IdT;
            }

            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                parameters.Add(new DataParameter(pCS, "IDPR", DbType.Int32), pIdPR);
                parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), pPosRequest.IdA_Entity);
                parameters.Add(new DataParameter(pCS, "IDA_CSSCUSTODIAN", DbType.Int32), pPosRequest.IdA_CssCustodian);
                parameters.Add(new DataParameter(pCS, "IDA_CSS", DbType.Int32), idA_Css ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_CUSTODIAN", DbType.Int32), idA_Custodian ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), idEM ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDCE", DbType.Int32), idCE ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequest.RequestType));
                parameters.Add(new DataParameter(pCS, "REQUESTMODE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pPosRequest.RequestMode));
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), pPosRequest.DtIns);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), appSession.IdA);
                parameters.Add(new DataParameter(pCS, "NOTES", DbType.AnsiString), pPosRequest.NotesSpecified ? pPosRequest.Notes : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDPROCESS_L", DbType.Int32), pIdProcess_L ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDPR_POSREQUEST", DbType.Int32), pIdPR_Parent ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), pPosRequest.StatusSpecified ? pPosRequest.Status : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "SOURCE", DbType.String, SQLCst.UT_ENUM_OPTIONAL_LEN), pPosRequest.Source);
                parameters.Add(new DataParameter(pCS, "EXTLLINK", DbType.String, SQLCst.UT_EXTLINK_LEN), pPosRequest.ExtlLinkSpecified ? pPosRequest.ExtlLink : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDPROCESS_L_SOURCE", DbType.Int32), pPosRequest.SourceIdProcessLSpecified ? pPosRequest.SourceIdProcessL : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDTOPTION", DbType.Int32), idTOption ?? Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "GPRODUCT", DbType.String, SQLCst.UT_ENUM_OPTIONAL_LEN), pPosRequest.GroupProductSpecified ? pPosRequest.GroupProductValue : Convert.DBNull);
            }

            _ = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());

            return errLevel;
        }
        #endregion InsertPosRequest
        #region GetPendingKeyPosRequest
        /// <summary>
        /// Recherche dans POSREQUEST un enregistrement en attente ou en cours de traitement qui pourrait être l'équivalent de {PosRequest}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        public static Nullable<int> GetPendingKeyPosRequest(string pCS, IDbTransaction pDbTransaction, IPosRequest pPosRequest)
        {
            IDbTransaction dbTransaction = pDbTransaction;
            bool isException = false;
            Nullable<int> idPR = null;
            try
            {
                DataParameters parameters = null;
                string sqlSelect = string.Empty;

                if (StrFunc.IsEmpty(pCS) && (null != pDbTransaction))
                    pCS = pDbTransaction.Connection.ConnectionString;

                if (pPosRequest.IdTSpecified)
                {
                    parameters = new DataParameters();
                    parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequest.RequestType));
                    parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pPosRequest.DtBusiness);
                    parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pPosRequest.IdT);

                    sqlSelect = @"select pr.IDPR
                    from dbo.POSREQUEST pr
                    where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.DTBUSINESS = @DTBUSINESS) and (isnull(pr.IDT, -1) = @IDT)" + Cst.CrLf;
                }
                else if (pPosRequest.PosKeepingKeySpecified)
                {
                    QueryParameters qryParameters2 = PosKeepingTools.GetQueryExistingKeyPosRequest(pCS, pPosRequest);
                    sqlSelect = qryParameters2.Query;
                    parameters = qryParameters2.Parameters;
                }
                sqlSelect += @"and ((pr.STATUS is null) or (pr.STATUS = 'PENDING'))" + Cst.CrLf;

                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);

                if (null == pDbTransaction)
                {
                    // PL 20180312 WARNING: Use Read Commited !
                    //dbTransaction = DataHelper.BeginTran(CSTools.SetMaxTimeOut(pCS, 480), IsolationLevel.ReadUncommitted);
                    dbTransaction = DataHelper.BeginTran(CSTools.SetMaxTimeOut(pCS, 480), IsolationLevel.ReadCommitted);
                }
                using (IDataReader dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (null != dr && dr.Read())
                        idPR = Convert.ToInt32(dr["IDPR"]);
                }
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null == pDbTransaction))
                {
                    if (isException) { DataHelper.RollbackTran(dbTransaction); }
                    dbTransaction.Dispose();
                }
            }
            return idPR;
        }
        #endregion
        #region IsPosKeepingBookDealer
        /// <summary>
        /// Obtient True si la tenue de position est gérée pour le Book du Dealer sur le trade {pIdT} 
        /// Le Dealer est la Partie avec FIXPARTYROLE = '27' 
        /// </summary>
        /// <param name="pCS">Chaine de connexion</parparam>
        /// <param name="pIdT">Identifiant du trade</param>
        /// <returns></returns>
        /// 
        // EG 20190613 [24683] Use dbTransaction
        public static bool IsPosKeepingBookDealer(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            return IsPosKeepingBookDealer(pCS, pDbTransaction, pIdT, Cst.ProductGProduct_FUT);
        }
        public static bool IsPosKeepingBookDealer(string pCS, IDbTransaction pDbTransaction, int pIdT, string pGProduct)
        {
            bool ret = false;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "tr.POSKEEPBOOK_DEALER ";
            // EG 20170322 [22919] New
            switch (pGProduct)
            {
                case Cst.ProductGProduct_FUT:
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE_POSETD + " tr" + Cst.CrLf;
                    break;
                case Cst.ProductGProduct_SEC:
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE_POSSEC + " tr" + Cst.CrLf;
                    break;
                case Cst.ProductGProduct_OTC:
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE_POSOTC + " tr" + Cst.CrLf;
                    break;
                case Cst.ProductGProduct_COM:
                    sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE_POSCOM + " tr" + Cst.CrLf;
                    break;
            }
            sqlSelect += SQLCst.WHERE + "tr.IDT=@IDT";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);
            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToBoolean(obj);
            return ret;
        }
        #endregion IsPosKeepingBookDealer
        #region IsPosKeepingKeyChanged
        public static bool IsPosKeepingKeyChanged(IPosKeepingKey pPosKeepingKey, DataRow pDr)
        {
            bool ret = (pPosKeepingKey.IdI != Convert.ToInt32(pDr["IDI"]));
            ret |= (pPosKeepingKey.IdAsset != Convert.ToInt32(pDr["IDASSET"]));
            ret |= (pPosKeepingKey.IdA_Dealer != Convert.ToInt32(pDr["IDA_DEALER"]));
            ret |= (pPosKeepingKey.IdB_Dealer != Convert.ToInt32(pDr["IDB_DEALER"]));
            ret |= (pPosKeepingKey.IdA_Clearer != Convert.ToInt32(pDr["IDA_CLEARER"]));
            ret |= (pPosKeepingKey.IdB_Clearer != Convert.ToInt32(pDr["IDB_CLEARER"]));
            return ret;
        }
        #endregion IsPosKeepingKeyChanged

        #region ReaderContainsColumn
        public static bool ReaderContainsColumn(IDataReader pReader, string pColumnName)
        {
            for (int i = 0; i < pReader.FieldCount; i++)
            {
                if (pReader.GetName(i).Equals(pColumnName, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }
        #endregion ReaderContainsColumn


        #region SendMessage
        
        /// <summary>
        /// Postage du message queue à partir de l'application web (avec insertion dans TRACKER_L)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQueue"></param>
        /// <param name="pDr"></param>
        /// <param name="pGProduct"></param>
        /// <returns></returns>
        private static Cst.ErrLevel SendMessage(string pCS, PosKeepingRequestMQueue pQueue, DataRow pDr, string pGProduct)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            try
            {
                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    connectionString = pCS,
                    Session = SessionTools.AppSession,
                    mQueue = new MQueueBase[] { pQueue },
                    process = pQueue.ProcessType,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = pQueue.ProcessType,
                        gProduct = pGProduct,
                        caller = pQueue.requestType.ToString(),
                        info = SetDataTracker(pQueue, pDr)
                    }
                };
                taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);

                var (isOk, errMsg) = MQueueTaskInfo.SendMultiple(taskInfo);
                if (!isOk)
                    throw new SpheresException2("MQueueTaskInfo.SendMultiple", errMsg);

            }
            catch (SpheresException2 ex)
            {
                //PL 20220614 Newness
                errLevel = Cst.ErrLevel.FAILURE;
                throw ex;
            }
            catch (Exception)
            {
                errLevel = Cst.ErrLevel.FAILURE;
            }
            return errLevel;
        }
        #endregion SendMessage
        #region SerializePosRequestDetail
        /// <summary>
        /// Serialisation du détail d'une demande POSREQUEST
        /// </summary>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        public static string SerializePosRequestDetail(IPosRequest pPosRequest)
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(pPosRequest.DetailBase.GetType(), pPosRequest.DetailBase);
            serializeInfo.SetPosRequestTradeInfo(((IPosRequestDetail)pPosRequest.DetailBase).EfsMLversion);
            // FI 20230103 [26204] Encoding.Unicode (puisque accepté par Oracle et sqlServer)
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, Encoding.Unicode);
            return sb.ToString();
        }
        #endregion UpdatePosRequest
        /// <summary>
        ///  Retourne un identifiant de l'asset
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        /// FI 20170327 [23004] public Method
        public static string GetContractName(DataRow pDr)
        {
            string contractFullName = string.Empty;
            if (null != pDr)
            {
                if (pDr.Table.Columns.Contains("CONTRACTIDENTIFIER") && (false == Convert.IsDBNull(pDr["CONTRACTIDENTIFIER"])))
                    contractFullName += Convert.ToString(pDr["CONTRACTIDENTIFIER"]);
                if (pDr.Table.Columns.Contains("MATFMT_MMMYY") && (false == Convert.IsDBNull(pDr["MATFMT_MMMYY"])))
                    contractFullName += " " + Convert.ToString(pDr["MATFMT_MMMYY"]);
                if (pDr.Table.Columns.Contains("PUTCALL") && (false == Convert.IsDBNull(pDr["PUTCALL"])))
                    contractFullName += " " + Convert.ToString(pDr["PUTCALL"]);
                if (pDr.Table.Columns.Contains("STRIKE") && (false == Convert.IsDBNull(pDr["STRIKE"])))
                    contractFullName += " " + StrFunc.FmtDecimalToInvariantCulture2(Convert.ToString(pDr["STRIKE"]));
            }
            return contractFullName;
        }

        #region SetDataTracker
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPosKeepingMQueue"></param>
        /// <param name="pDr"></param>
        /// <returns></returns>
        /// EG 20130607 [18740] Add RemoveEndOfDay, RemoveCAExecuted
        /// EG 20130807 [18870] Add Split
        /// EG 20190215 Tracker Donnée DTBUSINESS au format ISO sans heures
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        private static List<DictionaryEntry> SetDataTracker(PosKeepingRequestMQueue pPosKeepingMQueue, DataRow pDr)
        {
            List<DictionaryEntry> lstData = new List<DictionaryEntry>();
            switch (pPosKeepingMQueue.requestType)
            {
                case Cst.PosRequestTypeEnum.OptionAssignment:
                case Cst.PosRequestTypeEnum.OptionExercise:
                case Cst.PosRequestTypeEnum.PositionCancelation:
                case Cst.PosRequestTypeEnum.PositionTransfer:
                case Cst.PosRequestTypeEnum.OptionAbandon:
                case Cst.PosRequestTypeEnum.OptionNotExercised:
                case Cst.PosRequestTypeEnum.OptionNotAssigned:
                case Cst.PosRequestTypeEnum.RemoveAllocation:
                case Cst.PosRequestTypeEnum.ClearingSpecific:
                case Cst.PosRequestTypeEnum.TradeSplitting:
                    lstData.Add(new DictionaryEntry("IDDATA", pPosKeepingMQueue.parameters["IDT"].Value));
                    lstData.Add(new DictionaryEntry("IDDATAIDENT", "TRADE"));
                    lstData.Add(new DictionaryEntry("IDDATAIDENTIFIER", pPosKeepingMQueue.parameters["TRADE_IDENTIFIER"].Value));

                    switch (pPosKeepingMQueue.requestType)
                    {
                        case Cst.PosRequestTypeEnum.OptionAssignment:
                        case Cst.PosRequestTypeEnum.OptionExercise:
                        case Cst.PosRequestTypeEnum.PositionCancelation:
                            lstData.Add(new DictionaryEntry("DATA1", pPosKeepingMQueue.parameters["DTBUSINESS"].Value));
                            lstData.Add(new DictionaryEntry("DATA2", pPosKeepingMQueue.parameters["QTY"].Value));
                            lstData.Add(new DictionaryEntry("DATA3", pPosKeepingMQueue.parameters["FEES"].Value));
                            break;
                        case Cst.PosRequestTypeEnum.PositionTransfer:
                            lstData.Add(new DictionaryEntry("DATA1", pPosKeepingMQueue.parameters["DTBUSINESS"].Value));
                            if (null != pPosKeepingMQueue.parameters["TRADETARGET"])
                            {
                                lstData.Add(new DictionaryEntry("DATA2", pPosKeepingMQueue.parameters["TRADETARGET"].ExValue));
                                lstData.Add(new DictionaryEntry("DATA3", pPosKeepingMQueue.parameters["QTY"].Value));
                                lstData.Add(new DictionaryEntry("DATA4", pPosKeepingMQueue.parameters["DEALER"].Value + "/" + pPosKeepingMQueue.parameters["BOOKDEALER"].Value));
                                if (null != pPosKeepingMQueue.parameters["BOOKCLEARER"])
                                    lstData.Add(new DictionaryEntry("DATA5", pPosKeepingMQueue.parameters["CLEARER"].Value + "/" + pPosKeepingMQueue.parameters["BOOKCLEARER"].Value));
                                else if (null != pPosKeepingMQueue.parameters["CLEARER"])
                                    lstData.Add(new DictionaryEntry("DATA5", pPosKeepingMQueue.parameters["CLEARER"].Value));
                                else
                                    lstData.Add(new DictionaryEntry("DATA5", "N/A"));
                            }
                            else
                            {
                                lstData.Add(new DictionaryEntry("DATA2", "N/A"));
                                lstData.Add(new DictionaryEntry("DATA3", pPosKeepingMQueue.parameters["QTY"].Value));
                                lstData.Add(new DictionaryEntry("DATA4", pPosKeepingMQueue.parameters["DEALER"].Value + "/" + pPosKeepingMQueue.parameters["BOOKDEALER"].Value));
                                lstData.Add(new DictionaryEntry("DATA5", "N/A"));
                            }
                            break;
                        case Cst.PosRequestTypeEnum.OptionAbandon:
                        case Cst.PosRequestTypeEnum.OptionNotExercised:
                        case Cst.PosRequestTypeEnum.OptionNotAssigned:
                        case Cst.PosRequestTypeEnum.RemoveAllocation:
                            lstData.Add(new DictionaryEntry("DATA1", pPosKeepingMQueue.parameters["DTBUSINESS"].Value));
                            lstData.Add(new DictionaryEntry("DATA2", pPosKeepingMQueue.parameters["QTY"].Value));
                            break;
                        case Cst.PosRequestTypeEnum.ClearingSpecific:
                            lstData.Add(new DictionaryEntry("DATA1", pPosKeepingMQueue.parameters["ENTITY"].ExValue));
                            lstData.Add(new DictionaryEntry("DATA2", pPosKeepingMQueue.parameters["ASSET"].Value));
                            lstData.Add(new DictionaryEntry("DATA3", pPosKeepingMQueue.parameters["IDENTIFIER"].Value));
                            lstData.Add(new DictionaryEntry("DATA4", pPosKeepingMQueue.parameters["DTBUSINESS"].Value));
                            lstData.Add(new DictionaryEntry("DATA5", pPosKeepingMQueue.parameters["QTY"].Value));
                            break;
                        case Cst.PosRequestTypeEnum.TradeSplitting:
                            lstData.Add(new DictionaryEntry("DATA1", pPosKeepingMQueue.parameters["DTBUSINESS"].Value));
                            break;
                    }
                    break;
                case Cst.PosRequestTypeEnum.UnClearing:
                    lstData.Add(new DictionaryEntry("IDDATA", Convert.ToString(pDr["IDT"])));
                    lstData.Add(new DictionaryEntry("IDDATAIDENT", "TRADE"));
                    lstData.Add(new DictionaryEntry("IDDATAIDENTIFIER", Convert.ToString(pDr["TRADE_IDENTIFIER"])));
                    if (null != pPosKeepingMQueue.parameters["DATE1"])
                        lstData.Add(new DictionaryEntry("DATA1", pPosKeepingMQueue.parameters["DATE1"].Value));
                    else
                        lstData.Add(new DictionaryEntry("DATA1", Convert.ToDateTime(pDr["DTBUSINESS"])));
                    lstData.Add(new DictionaryEntry("DATA2", GetContractName(pDr)));
                    lstData.Add(new DictionaryEntry("DATA3", Convert.ToString(pDr["TRADE_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA4", Convert.ToString(pDr["UNCLEARINGQTY"])));
                    lstData.Add(new DictionaryEntry("DATA5", Convert.ToString(pDr["TR_CLOSING_IDENTIFIER"])));
                    break;
                case Cst.PosRequestTypeEnum.ClearingBulk:
                    lstData.Add(new DictionaryEntry("DATA1", Convert.ToString(pDr["ENTITY_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA2", Convert.ToString(pDr["MARKET"])));
                    lstData.Add(new DictionaryEntry("DATA3", GetContractName(pDr)));
                    //PM 20150601 [20575] Gestion DTENTITY
                    //lstData.Add(new DictionaryEntry("DATA4", DtFunc.DateTimeToStringISO(Convert.ToDateTime(pDr["DTMARKET"]))));
                    lstData.Add(new DictionaryEntry("DATA4", DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(pDr["DTENTITY"]))));
                    // EG 20170127 Qty Long To Decimal
                    lstData.Add(new DictionaryEntry("DATA5", StrFunc.FmtDecimalToInvariantCulture2(pDr["CLEARINGQTY"].ToString())));
                    break;
                case Cst.PosRequestTypeEnum.UpdateEntry:
                    lstData.Add(new DictionaryEntry("DATA1", Convert.ToString(pDr["ENTITY_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA2", Convert.ToString(pDr["CSSCUSTODIAN_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA3", Convert.ToString(pDr["MARKET"])));
                    lstData.Add(new DictionaryEntry("DATA4", GetContractName(pDr)));
                    //PM 20150601 [20575] Gestion DTENTITY
                    //lstData.Add(new DictionaryEntry("DATA5", DtFunc.DateTimeToStringISO(Convert.ToDateTime(pDr["DTMARKET"]))));
                    lstData.Add(new DictionaryEntry("DATA5", DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(pDr["DTENTITY"]))));
                    break;
                case Cst.PosRequestTypeEnum.ClosingDay:
                case Cst.PosRequestTypeEnum.EndOfDay:
                case Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin:
                    lstData.Add(new DictionaryEntry("IDDATA", pPosKeepingMQueue.id.ToString()));
                    lstData.Add(new DictionaryEntry("IDDATAIDENT", "POSREQUEST"));
                    lstData.Add(new DictionaryEntry("DATA1", Convert.ToString(pDr["ENTITY_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA2", Convert.ToString(pDr["CSSCUSTODIAN_IDENTIFIER"])));
                    //PM 20150519 [20575] Gestion DTENTITY
                    //lstData.Add(new DictionaryEntry("DATA3", DtFunc.DateTimeToStringISO(Convert.ToDateTime(pDr["DTMARKET"]))));
                    lstData.Add(new DictionaryEntry("DATA3", DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(pDr["DTENTITY"]))));
                    break;
                case Cst.PosRequestTypeEnum.RemoveEndOfDay:
                    lstData.Add(new DictionaryEntry("IDDATA", pPosKeepingMQueue.id.ToString()));
                    lstData.Add(new DictionaryEntry("IDDATAIDENT", "POSREQUEST"));
                    lstData.Add(new DictionaryEntry("DATA1", Convert.ToString(pDr["ENTITY_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA2", Convert.ToString(pDr["CSSCUSTODIAN_IDENTIFIER"])));
                    if (null != pPosKeepingMQueue.parameters["MARKET"])
                        lstData.Add(new DictionaryEntry("DATA3", pPosKeepingMQueue.parameters["MARKET"].ExValue));
                    else
                        lstData.Add(new DictionaryEntry("DATA3", "All"));
                    //PM 20150520 [20575] Gestion DTENTITY
                    //lstData.Add(new DictionaryEntry("DATA4", DtFunc.DateTimeToStringISO(Convert.ToDateTime(pDr["DTMARKET"]))));
                    lstData.Add(new DictionaryEntry("DATA4", DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(pDr["DTENTITY"]))));
                    if (null != pPosKeepingMQueue.parameters["ISSIMUL"])
                        lstData.Add(new DictionaryEntry("DATA5", pPosKeepingMQueue.parameters["ISSIMUL"].Value));
                    if (null != pPosKeepingMQueue.parameters["ISKEEPHISTORY"])
                        lstData.Add(new DictionaryEntry("DATA6", pPosKeepingMQueue.parameters["ISKEEPHISTORY"].Value));
                    break;
                case Cst.PosRequestTypeEnum.RemoveCAExecuted:
                    lstData.Add(new DictionaryEntry("IDDATA", pPosKeepingMQueue.id.ToString()));
                    lstData.Add(new DictionaryEntry("IDDATAIDENT", "POSREQUEST"));
                    lstData.Add(new DictionaryEntry("DATA1", Convert.ToString(pDr["CSSCUSTODIAN_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA2", Convert.ToString(pDr["SHORT_ACRONYM"])));
                    lstData.Add(new DictionaryEntry("DATA3", Convert.ToString(pDr["CA_IDENTIFIER"])));
                    lstData.Add(new DictionaryEntry("DATA4", DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(pDr["EFFECTIVEDATE"]))));
                    if (null != pPosKeepingMQueue.parameters["ISSIMUL"])
                        lstData.Add(new DictionaryEntry("DATA5", pPosKeepingMQueue.parameters["ISSIMUL"].Value));
                    if (null != pPosKeepingMQueue.parameters["ISKEEPHISTORY"])
                        lstData.Add(new DictionaryEntry("DATA6", pPosKeepingMQueue.parameters["ISKEEPHISTORY"].Value));
                    break;
                default:
                    break;
            }
            return lstData;
        }
        #endregion SetDataTracker
        #region SetPosKeepingKey
        public static IPosKeepingKey SetPosKeepingKey(IProductBase pProduct, DataRow pDr)
        {
            IPosKeepingKey posKeepingKey = pProduct.CreatePosKeepingKey();
            posKeepingKey.IdI = Convert.ToInt32(pDr["IDI"]);
            posKeepingKey.UnderlyingAsset = PosKeepingTools.GetUnderLyingAssetRelativeToInstrument(pDr);
            posKeepingKey.IdAsset = Convert.ToInt32(pDr["IDASSET"]);
            posKeepingKey.IdA_Dealer = Convert.ToInt32(pDr["IDA_DEALER"]);
            posKeepingKey.IdB_Dealer = Convert.ToInt32(pDr["IDB_DEALER"]);
            posKeepingKey.IdA_Clearer = Convert.ToInt32(pDr["IDA_CLEARER"]);
            posKeepingKey.IdB_Clearer = Convert.ToInt32(pDr["IDB_CLEARER"]);
            posKeepingKey.IdA_EntityDealer = Convert.ToInt32(pDr["IDA_ENTITYDEALER"]);
            if (false == Convert.IsDBNull(pDr["IDA_ENTITYCLEARER"]))
                posKeepingKey.IdA_EntityClearer = Convert.ToInt32(pDr["IDA_ENTITYCLEARER"]);
            return posKeepingKey;
        }
        public static IPosKeepingKey SetPosKeepingKey(IProductBase pProduct, IPosKeepingKey pPosKeepingKey)
        {
            IPosKeepingKey posKeepingKey = pProduct.CreatePosKeepingKey();
            posKeepingKey.IdI = pPosKeepingKey.IdI;
            posKeepingKey.UnderlyingAsset = pPosKeepingKey.UnderlyingAsset;
            posKeepingKey.IdAsset = pPosKeepingKey.IdAsset;
            posKeepingKey.IdA_Dealer = pPosKeepingKey.IdA_Dealer;
            posKeepingKey.IdB_Dealer = pPosKeepingKey.IdB_Dealer;
            posKeepingKey.IdA_Clearer = pPosKeepingKey.IdA_Clearer;
            posKeepingKey.IdB_Clearer = pPosKeepingKey.IdB_Clearer;
            posKeepingKey.IdA_EntityDealer = pPosKeepingKey.IdA_EntityDealer;
            posKeepingKey.IdA_EntityClearer = pPosKeepingKey.IdA_EntityClearer;
            return posKeepingKey;
        }
        #endregion SetPosKeepingKey
        #region SetPosRequestMainInfo
        // EG 20170317 [22967] Gestion de GroupProduct
        private static void SetPosRequestMainInfo(IPosRequest pPosRequest, DataRow pDr)
        {
            int idA_Entity = Convert.ToInt32(pDr["IDA_ENTITY"]);
            pPosRequest.IdA_Entity = idA_Entity;

            // EG 20170317 [22967]
            if ((false == pPosRequest.GroupProductSpecified)  && pDr.Table.Columns.Contains("GPRODUCT"))
            {
                pPosRequest.GroupProductSpecified = true;
                pPosRequest.GroupProduct = (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), pDr["GPRODUCT"].ToString());
            }

            if (pDr.Table.Columns.Contains("IDA_CSSCUSTODIAN") && pDr.Table.Columns.Contains("ISCUSTODIAN"))
            {
                int _idA_CssCustodian = Convert.ToInt32(pDr["IDA_CSSCUSTODIAN"]);
                bool _isCustodian = Convert.ToBoolean(pDr["ISCUSTODIAN"]);
                pPosRequest.SetIdA_CssCustodian(_idA_CssCustodian, _isCustodian);
            }
            else if (pDr.Table.Columns.Contains("IDA_CSS"))
            {
                // RD 20150915 [21354]
                //Nullable<int> _idA_Css = Convert.ToInt32(pDr["IDA_CSS"]);
                Nullable<int> _idA_Css = null;
                if (false == Convert.IsDBNull(pDr["IDA_CSS"]))
                    _idA_Css = Convert.ToInt32(pDr["IDA_CSS"]);
                Nullable<int> _idA_Custodian = null;
                if (pDr.Table.Columns.Contains("IDA_CUSTODIAN") && (false == Convert.IsDBNull(pDr["IDA_CUSTODIAN"])))
                    _idA_Custodian = Convert.ToInt32(pDr["IDA_CUSTODIAN"]);
                pPosRequest.SetIdA_CssCustodian(_idA_Css, _idA_Custodian);
            }

            int idI = Convert.ToInt32(pDr["IDI"]);
            Nullable<Cst.UnderlyingAsset> _underlyingAsset = GetUnderLyingAssetRelativeToInstrument(pDr);

            int idAsset = Convert.ToInt32(pDr["IDASSET"]);
            int idA_Dealer = Convert.ToInt32(pDr["IDA_DEALER"]);
            int idB_Dealer = Convert.ToInt32(pDr["IDB_DEALER"]);
            int idA_Clearer = Convert.ToInt32(pDr["IDA_CLEARER"]);
            int idB_Clearer = Convert.ToInt32(pDr["IDB_CLEARER"]);
            int idA_EntityClearer = 0;
            if (pDr.Table.Columns.Contains("IDA_ENTITYCLEARER") && (false == Convert.IsDBNull(pDr["IDA_ENTITYCLEARER"])))
                idA_EntityClearer = Convert.ToInt32(pDr["IDA_ENTITYCLEARER"]);


            if (pDr.Table.Columns.Contains("IDEM"))
            {
                pPosRequest.IdEMSpecified = (false == Convert.IsDBNull(pDr["IDEM"]));
                if (pPosRequest.IdEMSpecified)
                    pPosRequest.IdEM = Convert.ToInt32(pDr["IDEM"]);
            }
            pPosRequest.SetPosKey(idI, _underlyingAsset, idAsset, idA_Dealer, idB_Dealer, idA_Clearer, idB_Clearer);
            pPosRequest.SetAdditionalInfo(idA_Entity, idA_EntityClearer);

            if (pDr.Table.Columns.Contains("IDT"))
            {
                pPosRequest.IdTSpecified = (false == Convert.IsDBNull(pDr["IDT"]));
                if (pPosRequest.IdTSpecified)
                    pPosRequest.IdT = Convert.ToInt32(pDr["IDT"]);
            }

            if (pDr.Table.Columns.Contains("MARKET"))
            {
                string market = pDr["MARKET"].ToString();
                string instrument = pDr["NS_IDENTIFIER"].ToString();
                string asset = pDr["ASSET_IDENTIFIER"].ToString();
                string dealer = pDr["DEALER_IDENTIFIER"].ToString();
                string bookDealer = pDr["DEALER_BOOKIDENTIFIER"].ToString();
                string clearer = pDr["CLEARER_IDENTIFIER"].ToString();
                string bookClearer = pDr["CLEARER_BOOKIDENTIFIER"].ToString();
                pPosRequest.SetIdentifiers(market, instrument, asset, dealer, bookDealer, clearer, bookClearer);
                // EG 20160224 Test Contains
                if (pDr.Table.Columns.Contains("TRADE_IDENTIFIER") && (false == Convert.IsDBNull(pDr["TRADE_IDENTIFIER"])))
                    pPosRequest.Identifiers.Trade = pDr["TRADE_IDENTIFIER"].ToString();
                if (false == Convert.IsDBNull(pDr["ENTITY_IDENTIFIER"]))
                    pPosRequest.Identifiers.Entity = pDr["ENTITY_IDENTIFIER"].ToString();
                if (false == Convert.IsDBNull(pDr["CSSCUSTODIAN_IDENTIFIER"]))
                    pPosRequest.Identifiers.CssCustodian = pDr["CSSCUSTODIAN_IDENTIFIER"].ToString();
            }
            // EG 20151102 [21465] New
            else if (pDr.Table.Columns.Contains("TRADE_IDENTIFIER") && (false == Convert.IsDBNull(pDr["TRADE_IDENTIFIER"])))
            {
                pPosRequest.SetIdentifiers(pDr["TRADE_IDENTIFIER"].ToString());
            }
        }
        #endregion SetPosRequestMainInfo
        #region SetPosRequestMainInfoIdentifiers
        /// <summary>
        /// Alimentation des identifiants
        /// </summary>
        /// <param name="pPosRequest">Demande</param>
        /// <param name="pDr">DataRow source</param>
        private static void SetPosRequestMainInfoIdentifiers(IPosRequest pPosRequest, DataRow pDr)
        {
            string entity = pDr["ENTITY_IDENTIFIER"].ToString();
            string cssCustodian = pDr["CSSCUSTODIAN_IDENTIFIER"].ToString();
            string market = pDr["MARKET"].ToString();
            string instrument = pDr["NS_IDENTIFIER"].ToString();
            string asset = pDr["ASSET_IDENTIFIER"].ToString();
            string dealer = pDr["DEALER_IDENTIFIER"].ToString();
            string bookDealer = pDr["DEALER_BOOKIDENTIFIER"].ToString();
            string clearer = pDr["CLEARER_IDENTIFIER"].ToString();
            string bookClearer = pDr["CLEARER_BOOKIDENTIFIER"].ToString();
            pPosRequest.SetIdentifiers(entity, cssCustodian, market, instrument, asset, dealer, bookDealer, clearer, bookClearer);
            if (false == Convert.IsDBNull(pDr["TRADE_IDENTIFIER"]))
                pPosRequest.Identifiers.Trade = pDr["TRADE_IDENTIFIER"].ToString();
        }
        #endregion SetPosRequestMainInfoIdentifiers

        #region SetPosRequestDenouement
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type ABANDON/ASSIGNMENT/EXERCISE GLOBAL
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pDr">Caractéristiques de la demande</param>
        /// EG 20151019 [21465] New
        public static IPosRequest SetPosRequestDenouement(string pCS, IProductBase pProduct, DataRow pDr, SettlSessIDEnum pRequestMode, Nullable<bool> pIsFeeCalculation)
        {
            // EG 20170127 Qty Long To Decimal
            decimal availableQty = Convert.ToDecimal(pDr["AVAILABLEQTY"]);
            _ = Convert.ToDecimal(pDr["POSQTY"]);
            decimal qty = Convert.ToDecimal(pDr["CLEARINGQTY"]);
            _ = Convert.ToDecimal(pDr["DENQTY"]);
            Cst.PosRequestTypeEnum requestType = (Cst.PosRequestTypeEnum)ReflectionTools.EnumParse(new Cst.PosRequestTypeEnum(), pDr["REQUESTTYPE_VALUE"].ToString());

            int idT = Convert.ToInt32(pDr["IDT"]);
            string identifier = string.Empty;
            int idA_Entity = Convert.ToInt32(pDr["IDA_ENTITY"]);
            int idEM = Convert.ToInt32(pDr["IDEM"]);
            int idA_CSS = Convert.ToInt32(pDr["IDA_CSS"]);
            DateTime dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);

            int idAsset = Convert.ToInt32(pDr["IDASSET_UNL"]);
            Cst.UnderlyingAsset assetCategory = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), pDr["ASSETCATEGORY_UNL"].ToString());
            string assetIdentifier = pDr["ASSET_IDENTIFIER"].ToString();
            Nullable<QuoteTimingEnum> quoteTiming = null;
            if (false == Convert.IsDBNull(pDr["QUOTETIMING"]))
                quoteTiming = (QuoteTimingEnum)ReflectionTools.EnumParse(new QuoteTimingEnum(), pDr["QUOTETIMING"].ToString());
            decimal strikePrice = Convert.ToDecimal(pDr["STRIKEPRICE"]);

            Nullable<decimal> price = null;
            if (false == Convert.IsDBNull(pDr["VALUE"]))
                price = Convert.ToDecimal(pDr["VALUE"]);

            Nullable<DateTime> dtPrice = null;
            if (false == Convert.IsDBNull(pDr["TIME"]))
                dtPrice = Convert.ToDateTime(pDr["TIME"]);
            string source = null;
            if (false == Convert.IsDBNull(pDr["SOURCE"]))
                source = pDr["SOURCE"].ToString();


            IPosRequest posRequest = CreatePosRequestOption(pCS, pProduct, requestType, pRequestMode, idT, identifier, idA_Entity, idA_CSS, null,
            idEM, dtBusiness, qty, availableQty, strikePrice, assetCategory, idAsset, assetIdentifier, quoteTiming, price, dtPrice,
            source, null, null, pIsFeeCalculation, ProductTools.GroupProductEnum.ExchangeTradedDerivative);
            SetPosRequestMainInfo(posRequest, pDr);

            // EG 20151102 [21465]
            if (false == Convert.IsDBNull(pDr["IDPR"]))
                posRequest.IdPR = Convert.ToInt32(pDr["IDPR"]);
            return posRequest;
        }
        #endregion SetPosRequestDenouement

        #region SetPosRequestCorporateAction
        /// <summary>
        /// Clone le position request source en y ajoutant les informations supplémentaires reçues en paramètres
        /// </summary>
        /// <param name="pPosRequest">Position Request source</param>
        /// <param name="pIdDCDest">Id du DC Ex ajusté</param>
        /// <param name="pIdentifierDCDest">Identifiant du DC Ex ajusté</param>
        /// <param name="pEffectiveDate">DtBusiness du trade post CA</param>
        /// <returns>Une nouvelle demande (position request)</returns>
        // EG 20130417
        // EG 20140516 [19816] Ajout pContractSymbolDCEx (Mise jour du symbole DCEX dans TRADEXML)
        public static IPosRequest SetPosRequestCorporateAction(IPosRequestCorporateAction pPosRequest, int pIdDCEx, string pIdentifierDCEx, string pContractSymbolDCEx, DateTime pEffectiveDate)
        {
            IPosRequestCorporateAction posRequest = (IPosRequestCorporateAction)pPosRequest.Clone();
            posRequest.SetDetail(pIdDCEx, pIdentifierDCEx, pContractSymbolDCEx, pEffectiveDate);
            return posRequest;
        }
        #endregion SetPosRequestCorporateAction

        #region SetPosRequestCascadingShifting
        /// <summary>
        /// Clone le position request source en y ajoutant les informations supplémentaires reçues en paramètres
        /// </summary>
        /// <param name="pPosRequest">Position Request source</param>
        /// <param name="pIdDCDest">Id du DC destination</param>
        /// <param name="pIdentifierDCDest">Identifiant du DC Destination</param>
        /// <param name="pMaturityMonthYearDest">Maturity destination</param>
        /// <param name="pTradeTimeStamping">TimeStamp du trade post cascading/shifting</param>
        /// <returns>Une nouvelle demande (position request)</returns>
        // PM 20130219 [18414] & PM 20130307 [18434]
        public static IPosRequest SetPosRequestCascadingShifting(IPosRequestCascadingShifting pPosRequest, int pIdDCDest, string pIdentifierDCDest, string pMaturityMonthYearDest, DateTime pDtExecution)
        {
            IPosRequestCascadingShifting posRequest = (IPosRequestCascadingShifting)pPosRequest.Clone();
            posRequest.SetDetail(pIdDCDest, pIdentifierDCDest, pMaturityMonthYearDest, pDtExecution);
            return posRequest;
        }
        #endregion SetPosRequestCascadingShifting
        #region SetPosRequestClearing
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type COMPENSATION
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pRequestMode">Mode de la demande (INTRADAY / EOD)</param>
        /// <param name="pDr">Caractéristiques de la demande</param>
        // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public static IPosRequest SetPosRequestClearing(IProductBase pProduct, Cst.PosRequestTypeEnum pRequestType,
            SettlSessIDEnum pRequestMode, DataRow pDr)
        {
            IPosRequest posRequest = null;
            decimal availableQtyBuy = Convert.ToDecimal(pDr["QTY_BUY"]);
            decimal availableQtySell = Convert.ToDecimal(pDr["QTY_SELL"]);
            decimal qty;
            if (pDr.Table.Columns.Contains("CLEARINGQTY"))
                qty = Convert.ToDecimal(pDr["CLEARINGQTY"]);
            else
                qty = Math.Min(availableQtyBuy, availableQtySell);

            if (0 < qty)
            {
                // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
                //DateTime dtBusiness = Convert.ToDateTime(pDr["DTMARKET"]);
                DateTime dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
                if (Cst.PosRequestTypeEnum.ClearingEndOfDay == pRequestType)
                    posRequest = pProduct.CreatePosRequestClearingEOD(pRequestMode, dtBusiness, qty);
                else if (Cst.PosRequestTypeEnum.ClearingBulk == pRequestType)
                    posRequest = pProduct.CreatePosRequestClearingBLK(pRequestMode, dtBusiness, qty);

                ((IPosRequestClearingBLK)posRequest).SetDetail(availableQtyBuy, availableQtySell);
                SetPosRequestMainInfo(posRequest, pDr);
            }

            return posRequest;
        }
        #endregion SetPosRequestClearing
        #region SetPosRequestCorporateAction
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type CorporateAction (TRADE)
        /// </summary>
        /// <param name="pProduct"></param>
        /// <param name="pRequestType"></param>
        /// <param name="pRequestMode"></param>
        /// <param name="pDr"></param>
        /// <returns></returns>
        // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public static IPosRequest SetPosRequestCorporateAction(IProductBase pProduct, Cst.PosRequestTypeEnum pRequestType, DataRow pDr)
        {
            int _idA_Entity = Convert.ToInt32(pDr["IDA_ENTITY"]);
            int _idEM = Convert.ToInt32(pDr["IDEM"]);
            int _idA_Css = Convert.ToInt32(pDr["IDA_CSSCUSTODIAN"]);
            decimal _qty = Convert.ToDecimal(pDr["QTY"]);
            // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
            //DateTime _dtBusiness = Convert.ToDateTime(pDr["DTMARKET"]);
            DateTime _dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
            IPosRequest posRequest = pProduct.CreatePosRequestCorporateAction(pRequestType, _idA_Entity, _idA_Css, null, _idEM, _dtBusiness);
            SetPosRequestMainInfo(posRequest, pDr);
            posRequest.QtySpecified = (0 < _qty);
            posRequest.Qty = _qty;

            if (pDr.Table.Columns.Contains("ASSET_IDENTIFIER") && pDr.Table.Columns.Contains("IDT"))
                SetPosRequestMainInfoIdentifiers(posRequest, pDr);

            return posRequest;
        }
        #endregion SetPosRequestCorporateAction

        #region SetPosRequestMaturityOffsetting
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type MaturityOffsetting (Option/Future)
        /// ou Cascading Shifting
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pRequestMode">Timming de la demande</param>
        /// <param name="pDr">Caractéristiques de la demande</param>
        // PM 20130218 [18414] Ajout appel à CreatePosRequestCascadingShifting
        // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        public static IPosRequest SetPosRequestMaturityOffsetting(IProductBase pProduct, Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DataRow pDr)
        {
            // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
            DateTime dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);

            IPosRequest posRequest;
            if ((pRequestType == Cst.PosRequestTypeEnum.Cascading)
                || (pRequestType == Cst.PosRequestTypeEnum.Shifting))
            {
                posRequest = pProduct.CreatePosRequestCascadingShifting(pRequestType, pRequestMode, dtBusiness);
            }
            else
            {
                posRequest = pProduct.CreatePosRequestMaturityOffsetting(pRequestType, pRequestMode, dtBusiness);
            }

            SetPosRequestMainInfo(posRequest, pDr);

            if (pDr.Table.Columns.Contains("ASSET_IDENTIFIER") && pDr.Table.Columns.Contains("IDT"))
                SetPosRequestMainInfoIdentifiers(posRequest, pDr);

            return posRequest;
        }
        #endregion SetPosRequestMaturityOffsetting
        #region SetPosRequestPhysicalPeriodicDelivery
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type PhysicalPeriodicDelivery (Future)
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pDr">Caractéristiques de la demande</param>
        // EG 20170206 [22787] New 
        public static IPosRequest SetPosRequestPhysicalPeriodicDelivery(IProductBase pProduct, DataRow pDr)
        {
            DateTime dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
            IPosRequest posRequest = pProduct.CreatePosRequestPhysicalPeriodicDelivery(dtBusiness);

            SetPosRequestMainInfo(posRequest, pDr);

            if (pDr.Table.Columns.Contains("ASSET_IDENTIFIER") && pDr.Table.Columns.Contains("IDT"))
                SetPosRequestMainInfoIdentifiers(posRequest, pDr);

            return posRequest;
        }
        #endregion SetPosRequestPhysicalPeriodicDelivery

        #region SetPosRequestPositionTransfer
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type TRANSFERT DE POSITION GLOBAL
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pDr">Caractéristiques de la demande</param>
        /// EG 20141210 [20554] New
        /// EG 20150716 [21103] Add pIsReversalSafekeeping
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public static IPosRequest SetPosRequestPositionTransfer(string pCS, IProductBase pProduct, DataRow pDr,
            Pair<IParty, IBookId> pTransferDealer, Pair<IParty, IBookId> pTransferClearer, Pair<bool, bool> pTransferInfo, EFS_Boolean pIsReversalSafekeeping)
        {
            IPosRequest posRequest = null;
            decimal initialQty = Convert.ToDecimal(pDr["INITIALQTY"]);
            decimal availableQty = Convert.ToDecimal(pDr["AVAILABLEQTY"]);
            decimal qty = Convert.ToDecimal(pDr["CLEARINGQTY"]);

            if (0 < qty)
            {
                DateTime dtBusiness = Convert.ToDateTime(pDr["DTBUSINESS"]);
                posRequest = pProduct.CreatePosRequestTransfer(dtBusiness, qty);

                // EG 20150716 [21103] Add pIsReversalSafekeeping
                ((IPosRequestTransfer)posRequest).SetDetail(pCS, initialQty, availableQty, pTransferDealer, pTransferClearer, pTransferInfo, pIsReversalSafekeeping);
                SetPosRequestMainInfo(posRequest, pDr);
            }

            return posRequest;
        }
        #endregion SetPosRequestPositionTransfer
        #region SetPosRequestMaturityRedemptionOffsetting
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type MaturityRedemptionOffsetting (DebtSecurity)
        /// </summary>
        // EG 20190926 [Maturity Redemption]  New 
        public static IPosRequest SetPosRequestMaturityRedemptionOffsetting(IProductBase pProduct, Cst.PosRequestTypeEnum pRequestType, SettlSessIDEnum pRequestMode, DataRow pDr)
        {
            DateTime dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
            IPosRequest posRequest = pProduct.CreatePosRequestMaturityOffsetting(pRequestType, pRequestMode, dtBusiness);
            SetPosRequestMainInfo(posRequest, pDr);
            if (pDr.Table.Columns.Contains("ASSET_IDENTIFIER") && pDr.Table.Columns.Contains("IDT"))
                SetPosRequestMainInfoIdentifiers(posRequest, pDr);
            return posRequest;
        }
        #endregion SetPosRequestMaturityRedemptionOffsetting
        #region SetPosRequestUnclearing
        /// <summary>
        /// Retourne un IPOSREQUEST de type UNCLEARING si UNCLEARINGQTY >0 sinon retourne null
        /// </summary>
        /// <param name="pProduct">Product</param>
        /// <param name="pDr">Caractéristiques de la demande</param>
        /// PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// EG 20170127 Qty Long To Decimal
        public static IPosRequest SetPosRequestUnclearing(IProductBase pProduct, DataRow pDr)
        {
            IPosRequestUnclearing posRequest = null;

            decimal qty = Convert.ToDecimal(pDr["UNCLEARINGQTY"]);
            if (0 < qty)
            {
                decimal closingQty = Convert.ToDecimal(pDr["QTY"]);
                int idPR = Convert.ToInt32(pDr["IDPR"]);
                int idPADET = Convert.ToInt32(pDr["IDPADET"]);

                int idT_Closing = Convert.ToInt32(pDr["IDT_CLOSING"]);
                //FI 20120113 Oracle ne connait uniquement des alias en majuscule
                string identifier_Closing = pDr["TR_CLOSING_IDENTIFIER"].ToString();

                // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
                //DateTime dtMarket = Convert.ToDateTime(pDr["DTMARKET"]);
                DateTime dtMarket = Convert.ToDateTime(pDr["DTENTITY"]);
                DateTime dtBusiness = Convert.ToDateTime(pDr["DTBUSINESS"]);
                Cst.PosRequestTypeEnum requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(pDr["REQUESTTYPE_VALUE"].ToString()); 

                posRequest = pProduct.CreatePosRequestUnclearing(dtMarket, idPR, idPADET, requestType, qty, dtBusiness, idT_Closing, identifier_Closing, closingQty);

                // EG 20130604 Cas des annulation de dénouement automatique sur Options avec Livraison Future
                if (pDr.Table.Columns.Contains("IDT_DELIVERY"))
                {
                    posRequest.Detail.IdT_DeliverySpecified = (false == Convert.IsDBNull(pDr["IDT_DELIVERY"]));
                    posRequest.Detail.Delivery_IdentifierSpecified = (false == Convert.IsDBNull(pDr["TR_DLVRY_IDENTIFIER"]));
                    if (posRequest.Detail.IdT_DeliverySpecified)
                        posRequest.Detail.IdT_Delivery = Convert.ToInt32(pDr["IDT_DELIVERY"]);
                    if (posRequest.Detail.Delivery_IdentifierSpecified)
                        posRequest.Detail.Delivery_Identifier = pDr["TR_DLVRY_IDENTIFIER"].ToString();
                }
                SetPosRequestMainInfo(posRequest, pDr);

                if (pDr.Table.Columns.Contains("ASSET_IDENTIFIER") && pDr.Table.Columns.Contains("IDT"))
                    SetPosRequestMainInfoIdentifiers(posRequest, pDr);
            }

            return posRequest;
        }
        #endregion SetPosRequestUnclearing
        #region SetPosRequestUpdateEntry
        /// <summary>
        /// Alimentation d'une demande POSREQUEST de type UpdateEntry
        /// </summary>
        /// <param name="pProduct">Product</parparam>
        /// <param name="pPosRequestType">Type de demande</parparam>
        /// <param name="pDr">Caractéristiques de la demande</parparam>
        // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        public static IPosRequest SetPosRequestUpdateEntry(IProductBase pProduct, SettlSessIDEnum pRequestMode, DataRow pDr)
        {
            // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
            DateTime dtBusiness = Convert.ToDateTime(pDr["DTENTITY"]);
            IPosRequest posRequest = pProduct.CreatePosRequestUpdateEntry(pRequestMode, dtBusiness);
            SetPosRequestMainInfo(posRequest, pDr);
            if (pDr.Table.Columns.Contains("ASSET_IDENTIFIER") && pDr.Table.Columns.Contains("IDT"))
                SetPosRequestMainInfoIdentifiers(posRequest, pDr);
            return posRequest;
        }
        #endregion SetPosRequestUpdateEntry

        #region UpdatePosRequest
        /// <summary>
        /// Update d'une demande de traitement déjà existante dans POSREQUEST
        /// <para>Le statut du POSREQUEST est alimenté avec pPosRequest.status</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIdPR">Identifiant de la demande POSREQUEST</param>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pIdAUpd">Application/Acteur qui applique l'update</param>
        public static void UpdatePosRequest(string pCS, int pIdPR, IPosRequest pPosRequest, int pIdAUpd)
        {
            UpdatePosRequest(pCS, null, pIdPR, pPosRequest, pIdAUpd, null, null);
        }
        /// <summary>
        /// Update d'une demande de traitement déjà existante dans POSREQUEST
        /// <para>Le statut du POSREQUEST est alimenté avec pPosRequest.status</para>
        /// <para>La source du POSREQUEST est alimentée avec pPosRequest.source</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdPR">Identifiant de la demande POSREQUEST</param>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pIdAUpd">Application/Acteur qui applique l'update</param>
        /// <param name="pIdProcess_L">IdLog du traitement à l'origine de la mise à jour du POSREQUEST</param> 
        /// <param name="pIdPR_Parent">Identifiant de la demande POSREQUEST parent</param>
        /// FI 20130318 [18467] méthode void
        /// FI 20130319 [18467] Alimentation de SOURCE
        /// FI 20130327 [18467] Alimentation de EXTLLINK
        /// FI 20130327 [18467] Alimentation de IDPROCESS_L_SOURCE
        /// EG 20130613 [18751] Alimentation de REQUESTMODE
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        // EG 20180307 [23769] Gestion dbTransaction
        public static void UpdatePosRequest(string pCS, IDbTransaction pDbTransaction, int pIdPR, IPosRequest pPosRequest, int pIdAUpd,
            Nullable<int> pIdProcess_L, Nullable<int> pIdPR_Parent)
        {
            if (StrFunc.IsEmpty(pCS) && (null != pDbTransaction))
                pCS = pDbTransaction.Connection.ConnectionString;

            string sqlQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.POSREQUEST.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "REQUESTMODE = @REQUESTMODE, REQUESTDETAIL = @REQUESTDETAIL, DTUPD = @DTUPD, IDAUPD = @IDAUPD, " + Cst.CrLf;
            sqlQuery += "IDEM = @IDEM, IDCE = @IDCE, IDT = @IDT, QTY = @QTY, "  + Cst.CrLf;
            sqlQuery += "NOTES = @NOTES, IDPROCESS_L = @IDPROCESS_L, IDPR_POSREQUEST = @IDPR_POSREQUEST, " + Cst.CrLf;
            if (DtFunc.IsDateTimeFilled(pPosRequest.DtIns))
                sqlQuery += "DTINS = @DTINS, " + Cst.CrLf;
            sqlQuery += "SOURCE = @SOURCE, EXTLLINK = @EXTLLINK," + Cst.CrLf;
            sqlQuery += "IDPROCESS_L_SOURCE = @IDPROCESS_L_SOURCE," + Cst.CrLf;
            sqlQuery += "STATUS = @STATUS" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "IDPR = @IDPR";

            string detail = null;
            if (pPosRequest.DetailBase != null)
                detail = SerializePosRequestDetail(pPosRequest);

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDPR", DbType.Int32), pIdPR);
            parameters.Add(new DataParameter(pCS, "REQUESTDETAIL", DbType.Xml), StrFunc.IsFilled(detail) ? detail : Convert.DBNull);
            if (DtFunc.IsDateTimeFilled(pPosRequest.DtIns))
                parameters.Add(DataParameter.GetParameter(pCS,DataParameter.ParameterEnum.DTINS), pPosRequest.DtIns);
            // FI 20200820 [25468] Dates systemes en UTC
            parameters.Add(DataParameter.GetParameter(pCS,DataParameter.ParameterEnum.DTUPD), OTCmlHelper.GetDateSysUTC(pCS));
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), pIdAUpd);
            parameters.Add(new DataParameter(pCS, "SOURCE", DbType.String, SQLCst.UT_ENUM_OPTIONAL_LEN), pPosRequest.SourceSpecified ? pPosRequest.Source : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "REQUESTMODE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pPosRequest.RequestMode));

            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pPosRequest.IdEMSpecified ? pPosRequest.IdEM : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDCE", DbType.Int32), pPosRequest.IdCESpecified ? pPosRequest.IdCE : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pPosRequest.IdTSpecified ? pPosRequest.IdT : Convert.DBNull);
            // EG 20150920 [21374] Int (int32) to Long (Int64)
            // EG 20170127 Qty Long To Decimal
            parameters.Add(new DataParameter(pCS, "QTY", DbType.Decimal), pPosRequest.QtySpecified ? pPosRequest.Qty : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "NOTES", DbType.AnsiString), pPosRequest.NotesSpecified ? pPosRequest.Notes : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDPROCESS_L", DbType.Int32), pIdProcess_L ?? Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDPR_POSREQUEST", DbType.Int32), pIdPR_Parent ?? Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), pPosRequest.StatusSpecified ? pPosRequest.Status.ToString() : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "EXTLLINK", DbType.String, SQLCst.UT_EXTLINK_LEN), pPosRequest.ExtlLinkSpecified ? pPosRequest.ExtlLink.ToString() : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDPROCESS_L_SOURCE", DbType.Int32), pPosRequest.SourceIdProcessLSpecified ? pPosRequest.SourceIdProcessL : Convert.DBNull);
            _ = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
        }
        /// <summary>
        /// Méthode la plus exhaustive de mise à jour, tous les champs de la table POSREQUEST sont considérés et valorisés en fonction de {pPosRequest}
        /// <para>IPosRequest IDPR doit être renseigné obligatoirement</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest"></param>
        /// FI 20130408 [18467] méthode utilisée pour la mise à jour d'un POSREQUEST 
        /// EG 20150920 [21374] Int (int32) to Long (Int64)  
        /// EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public static void UpdatePosRequest(string pCS, IDbTransaction pDbTransaction, IPosRequest pPosRequest)
        {
            // FI 20181025 [24279] Corrections diverses

            if (pPosRequest.IdPR == 0)
                throw new ArgumentException("0 is not a valid value for pPosRequest.IDPR");

            string sqlQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.POSREQUEST.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.SET + "REQUESTMODE = @REQUESTMODE, REQUESTTYPE = @REQUESTTYPE, DTBUSINESS = @DTBUSINESS," + Cst.CrLf;
            sqlQuery += "REQUESTDETAIL = @REQUESTDETAIL," + Cst.CrLf;
            sqlQuery += "IDEM = @IDEM, IDCE = @IDCE, QTY = @QTY, NOTES = @NOTES, IDPR_POSREQUEST= @IDPR_POSREQUEST, " + Cst.CrLf;
            sqlQuery += "SOURCE = @SOURCE, EXTLLINK = @EXTLLINK," + Cst.CrLf;
            sqlQuery += "IDPROCESS_L = @IDPROCESS_L, IDPROCESS_L_SOURCE = @IDPROCESS_L_SOURCE," + Cst.CrLf;
            sqlQuery += "IDT = @IDT," + Cst.CrLf;
            sqlQuery += "IDI = @IDI, IDASSET = @IDASSET, " + Cst.CrLf;
            sqlQuery += "IDA_DEALER = @IDA_DEALER, IDB_DEALER = @IDB_DEALER, IDA_ENTITY = @IDA_ENTITY," + Cst.CrLf;
            sqlQuery += "IDA_CLEARER = @IDA_CLEARER, IDB_CLEARER = @IDB_CLEARER, " + Cst.CrLf;
            sqlQuery += "IDA_CSS = @IDA_CSS, IDA_CUSTODIAN = @IDA_CUSTODIAN, " + Cst.CrLf;
            sqlQuery += "STATUS = @STATUS," + Cst.CrLf;
            sqlQuery += "IDTOPTION = @IDTOPTION, GPRODUCT = @GPRODUCT," + Cst.CrLf;
            sqlQuery += "DTUPD = @DTUPD, IDAUPD = @IDAUPD" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "IDPR = @IDPR";

            string detail = null;
            if (pPosRequest.DetailBase != null)
                detail = SerializePosRequestDetail(pPosRequest);

            Nullable<int> idTOption = null;
            if ((pPosRequest is IPosRequestOption) && pPosRequest.IdTSpecified &&
                (pPosRequest.RequestType == Cst.PosRequestTypeEnum.UnderlyerDelivery))
            {
                idTOption = pPosRequest.IdT;
            }


            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDPR", DbType.Int32), pPosRequest.IdPR);
            parameters.Add(new DataParameter(pCS, "REQUESTMODE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pPosRequest.RequestMode));
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequest.RequestType));
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pPosRequest.DtBusiness);
            parameters.Add(new DataParameter(pCS, "REQUESTDETAIL", DbType.Xml), StrFunc.IsFilled(detail) ? detail : Convert.DBNull);

            parameters.Add(new DataParameter(pCS, "SOURCE", DbType.String, SQLCst.UT_ENUM_OPTIONAL_LEN), pPosRequest.SourceSpecified ? pPosRequest.Source : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pPosRequest.IdEMSpecified ? pPosRequest.IdEM : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDCE", DbType.Int32), pPosRequest.IdCESpecified ? pPosRequest.IdCE : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pPosRequest.IdTSpecified ? pPosRequest.IdT : Convert.DBNull);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            parameters.Add(new DataParameter(pCS, "QTY", DbType.Decimal), pPosRequest.QtySpecified ? pPosRequest.Qty : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "NOTES", DbType.AnsiString), pPosRequest.NotesSpecified ? pPosRequest.Notes : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDPR_POSREQUEST", DbType.Int32), pPosRequest.IdPR_PosRequestSpecified ? pPosRequest.IdPR_PosRequest : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "EXTLLINK", DbType.String, SQLCst.UT_EXTLINK_LEN), pPosRequest.ExtlLinkSpecified ? pPosRequest.ExtlLink.ToString() : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDPROCESS_L", DbType.Int32), pPosRequest.IdProcessLSpecified ? pPosRequest.IdProcessL : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDPROCESS_L_SOURCE", DbType.Int32), pPosRequest.SourceIdProcessLSpecified ? pPosRequest.SourceIdProcessL : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDA_CSS", DbType.Int32), pPosRequest.IdA_CssSpecified ? pPosRequest.IdA_Css : Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "IDA_CUSTODIAN", DbType.Int32), pPosRequest.IdA_CustodianSpecified ? pPosRequest.IdA_Custodian : Convert.DBNull);

            parameters.Add(new DataParameter(pCS, "IDTOPTION", DbType.Int32), idTOption ?? Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "GPRODUCT", DbType.String, SQLCst.UT_ENUM_OPTIONAL_LEN), pPosRequest.GroupProductSpecified ? pPosRequest.GroupProductValue : Convert.DBNull);

            // FI 20200820 [25468] Dates systemes en UTC
            parameters.Add(DataParameter.GetParameter(pCS,DataParameter.ParameterEnum.DTUPD), OTCmlHelper.GetDateSysUTC(pCS));
            parameters.Add(DataParameter.GetParameter(pCS,DataParameter.ParameterEnum.IDAUPD), pPosRequest.IdAUpd);
            parameters.Add(new DataParameter(pCS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), pPosRequest.StatusSpecified ? pPosRequest.Status.ToString() : Convert.DBNull);

            if (pPosRequest.PosKeepingKeySpecified)
            {
                IPosKeepingKey key = pPosRequest.PosKeepingKey;
                parameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), key.IdI);
                parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), key.IdAsset);
                parameters.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), key.IdA_Dealer > 0 ? key.IdA_Dealer : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), key.IdB_Dealer > 0 ? key.IdB_Dealer : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), key.IdA_Clearer > 0 ? key.IdA_Clearer : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), key.IdB_Clearer > 0 ? key.IdB_Clearer : Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), key.IdA_EntityDealer > 0 ? key.IdA_EntityDealer : Convert.DBNull);
            }
            else
            {
                parameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), Convert.DBNull);
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);

            _ = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter());
        }
        #endregion UpdatePosRequest
        #region UpdatePosRequestGroupLevel
        /// <summary>
        /// Mise à jour du status d'un POSREQUEST en fonction de ses POSREQUEST enfants
        /// </summary>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        // EG 20140120 Report 3.7
        // EG 20180307 [23769] Gestion List<IPosRequest>
        public static void UpdatePosRequestGroupLevel(string pCS, IPosRequest pPosRequest, List<IPosRequest> pLstSubPosRequest, AppSession pSession, int pIdProcess)
        {
            UpdatePosRequestGroupLevel(pCS, pPosRequest, pLstSubPosRequest, ProcessStateTools.StatusNoneEnum, pSession, pIdProcess);
        }
        /// <summary>
        /// Mise à jour du status d'un POSREQUEST en fonction de ses POSREQUEST enfants
        /// </summary>
        /// <param name="pPosRequest">Demande POSREQUEST</param>
        /// <param name="pStatus">Statut par défaut</param>
        // EG 20140120 Report 3.7
        // EG 20180307 [23769] Gestion List<IPosRequest>
        public static void UpdatePosRequestGroupLevel(string pCS, IPosRequest pPosRequest, List<IPosRequest> pLstSubPosRequest, ProcessStateTools.StatusEnum pStatus, AppSession pAppSession, int pIdProcess)
        {
            pPosRequest.Status = GetStatusGroupLevel(pLstSubPosRequest, pPosRequest.IdPR, pStatus);
            pPosRequest.StatusSpecified = true;
            PosKeepingTools.UpdatePosRequest(pCS, null, pPosRequest.IdPR, pPosRequest, pAppSession.IdA, pIdProcess, pPosRequest.IdPR_PosRequest);
        }
        #endregion UpdatePosRequestGroupLevel
        #region UpdateIRQPosRequestGroupLevel
        // EG 20180525 [23979] IRQ Processing
        public static void UpdateIRQPosRequestGroupLevel(string pCS, IPosRequest pPosRequest, AppSession pSession, int pIdProcess)
        {
            pPosRequest.Status = ProcessStateTools.StatusInterruptEnum;
            pPosRequest.StatusSpecified = true;
            PosKeepingTools.UpdatePosRequest(pCS, null, pPosRequest.IdPR, pPosRequest, pSession.IdA, pIdProcess, pPosRequest.IdPR_PosRequest);
        }
        #endregion UpdateIRQPosRequestGroupLevel
        #region GetStatusGroupLevel
        /// <summary>
        /// Détermine le statut le plus fort sur la base des POSREQUEST de même PARENT (IDPR_POSZREQUEST)
        /// </summary>
        /// <param name="pIdPR"></param>
        /// <param name="pDefaultStatus">statut par défaut</param>
        /// <returns></returns>
        // EG 20180221 Upd Signature pSubPosRequest (List<IPosRequest>)
        // EG 20180307 [23769] Refactoring + Lock
        // EG 20180312 Ciomment Test StatusNone
        // EG 20180525 [23979] IRQ Processing
        public static ProcessStateTools.StatusEnum GetStatusGroupLevel(List<IPosRequest> pSubPosRequest, int pIdPR, ProcessStateTools.StatusEnum pDefaultStatus)
        {
            ProcessStateTools.StatusEnum status = pDefaultStatus;

            // FI 20181219 [24399] Add test valeur null avant de faire le lock
            // Lock de la collection des posRequests childs
            if (null != pSubPosRequest)
            {
                object spinLock = ((ICollection)pSubPosRequest).SyncRoot;
                lock (spinLock)
                {
                    List<IPosRequest> result = (from item in pSubPosRequest
                                                where item.IdPR_PosRequestSpecified && (item.IdPR_PosRequest == pIdPR) && item.StatusSpecified
                                                select item).ToList();

                    if (result.Exists(item => ProcessStateTools.IsStatusInterrupt(item.Status)))
                        status = ProcessStateTools.StatusInterruptEnum;
                    else if (result.Exists(item => ProcessStateTools.IsStatusError(item.Status)))
                        status = ProcessStateTools.StatusErrorEnum;
                    else if (result.Exists(item => ProcessStateTools.IsStatusWarning(item.Status)))
                        status = ProcessStateTools.StatusWarningEnum;
                    else
                    {
                        if (result.Exists(item => ProcessStateTools.IsStatusPending(item.Status)))
                            status = ProcessStateTools.StatusPendingEnum;
                        else if (result.Exists(item => ProcessStateTools.IsStatusProgress(item.Status)))
                            status = ProcessStateTools.StatusProgressEnum;
                        else if (result.Exists(item => ProcessStateTools.IsStatusSuccess(item.Status)))
                            status = ProcessStateTools.StatusSuccessEnum;
                        //CC/PL/EG 20180312 
                        //else if (result.Exists(item => ProcessStateTools.IsStatusNone(item.status)))
                        //    status = ProcessStateTools.StatusNoneEnum;
                    }
                }
            }
            return status;
        }
        #endregion GetStatusGroupLevel

        #region IsShiftingDayForEntityMarket
        // PM 20130301 [18434] Added for position Shifting
        /// <summary>
        /// Vérifie s'il existe au moins une échéance de contrats en Shifting dont le dernier jour de négociation est <paramref name="pBusinessDate"/>
        /// <para>pour les marchés du couple Entity Market dont l'Id est <paramref name="pIdEM"/>, et uniquement si <paramref name="pBusinessDate"/> et la date courante pour ce couple EntityMarket</para>
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <param name="pIdEM">Id du couple EntitéMarket pour lequel effectuer le test</param>
        /// <param name="pBusinessDate">Date de bourse pour laquelle effectuer le test</param>
        /// <returns>bool</returns>
        public static bool IsShiftingDayForEntityMarket(string pCS, int pIdEM, DateTime pBusinessDate)
        {
            bool ret = false;
            //
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pBusinessDate); // FI 20201006 [XXXXX] DbType.Date

            // PM 20150601 [20575] Utilisation de DTENTITY à la place de DTMARKET
            // RD 20180302 [23757] Chercher les DC destinataires d'un shifting avec LASTTRADINGDAY à la date business 
            string sqlSelect = @"select count(*) 
            from dbo.ENTITYMARKET em
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDM = em.IDM)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.LASTTRADINGDAY = em.DTENTITY) and (ma.LASTTRADINGDAY = @DTBUSINESS)
            left outer join dbo.DERIVATIVECONTRACT dcshift on (dcshift.IDDC_SHIFT = dc.IDDC)
            where (em.IDEM = @IDEM)
            and ((dc.IDDC_SHIFT is not null) or (dcshift.IDDC is not null))";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToBoolean(obj);
            return ret;
        }
        #endregion IsShiftingDayForEntityMarket

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPR"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pAppInstance">Application qui insère la ligne</param>
        /// <param name="pIdProcess"></param>
        /// <param name="pIdPR_Parent"></param>
        /// <returns></returns>
        public static Cst.ErrLevel AddNewPosRequest(string pCS, IDbTransaction pDbTransaction, out int pIdPR, IPosRequest pPosRequest, AppSession appSession,
            Nullable<int> pIdProcess, Nullable<int> pIdPR_Parent)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

            if (null != pDbTransaction)
                SQLUP.GetId(out pIdPR, pDbTransaction, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
            else
                SQLUP.GetId(out pIdPR, pCS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);

            if (Cst.ErrLevel.SUCCESS == errLevel)
                errLevel = InsertPosRequest(pCS, pDbTransaction, pIdPR, pPosRequest, appSession, pIdProcess, pIdPR_Parent);

            return errLevel;
        }

        /// <summary>
        /// Retourne un clone de {pPosRequest}
        /// </summary>
        /// <param name="pPosRequest"></param>
        /// FI 20130311 [18467] new method
        public static IPosRequest ClonePosRequest(IPosRequest pPosRequest)
        {
            return (IPosRequest)ReflectionTools.Clone(pPosRequest, ReflectionTools.CloneStyle.CloneField);
        }


        /// <summary>
        /// Génère un PosKeepingRequestMQueue à partir d'un POSREQUEST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pParameters"></param>
        /// <returns></returns>
        /// FI 20130422 [18601]  add Method 
        /// EG 20150317 [POC] pPosRequest.GetMQueueIdInfo sans parametre
        public static PosKeepingRequestMQueue BuildPosKeepingRequestMQueue(string pCS, IPosRequest pPosRequest, MQueueparameters pParameters)
        {
            // SendMessage POSREQUESTMQUEUE
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = pCS,
                id = pPosRequest.IdPR,
                idInfo = pPosRequest.GetMQueueIdInfo(),
                parameters = pParameters
            };

            if (null == mQueueAttributes.parameters)
                mQueueAttributes.parameters = new MQueueparameters();

            PosKeepingTools.AdditionalPosRequestParameters(pPosRequest, mQueueAttributes.parameters);

            PosKeepingRequestMQueue requestMQueue = new PosKeepingRequestMQueue(pPosRequest.RequestType, mQueueAttributes);

            return requestMQueue;
        }


        #endregion Methods
    }

    #endregion PosKeepingTools
}

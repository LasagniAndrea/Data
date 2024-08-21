#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeInformation;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;
//
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    // EG 20150708 [21103] Partialisation de PokKeeingGen_OTC pour les frais de garde (voir fichier PosKeepingGen_SafeKeeping.cs)
    public partial class PosKeepingGen_OTC : PosKeepingGenProcessBase
    {
        #region Accessors
        #region StoreProcedure_RemoveEOD
        protected override string StoreProcedure_RemoveEOD
        {
            get { return "UP_REMOVE_POSACTION_OTC"; }
        }
        #endregion StoreProcedure_RemoveEOD
        #region VW_TRADE_POS
        protected override string VW_TRADE_POS
        {
            get { return Cst.OTCml_TBL.VW_TRADE_POSOTC.ToString(); }
        }
        #endregion VW_TRADE_POS
        #region VW_TRADE_FUNGIBLE_LIGHT
        protected override string VW_TRADE_FUNGIBLE_LIGHT
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_LIGHT_OTC.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE_LIGHT
        #region VW_TRADE_FUNGIBLE
        protected override string VW_TRADE_FUNGIBLE
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_OTC.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE
        #region RESTRICT_EVENT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENT_POS
        {
            get { return "EVENTCODE in ('TRL','PRL','DRL')"; }
        }
        #endregion RESTRICT_EVENT_POS
        #region RESTRICT_TRDTYPE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override string RESTRICT_TRDTYPE_POS
        {
            //PL 20170403 [23015]
            get { return "isnull(tr.TRDTYPE,'0') not in (" + Cst.TrdType_ExcludedValuesForFees_OTC + ")"; }
        }
        #endregion RESTRICT_TRDTYPE_POS
        #region RESTRICT_GPRODUCT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_GPRODUCT_POS
        {
            get { return "p.GPRODUCT = '" + GPRODUCT_VALUE + "'"; }
        }
        #endregion RESTRICT_GPRODUCT_POS
        #region RESTRICT_ISCUSTODIAN
        // EG 20181010 PERF EOD New
        protected override string RESTRICT_ISCUSTODIAN
        {
            get { return "1"; }
        }
        #endregion RESTRICT_ISCUSTODIAN
        #region RESTRICT_EMCUSTODIAN_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EMCUSTODIAN_POS
        {
            get { return "em.IDA_CUSTODIAN = tr.IDA_CSSCUSTODIAN"; }
        }
        #endregion RESTRICT_EMCUSTODIAN_POS
        #region RESTRICT_ASSET_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_ASSET_POS
        {
            get { return "inner join VW_ASSET ass on (ass.IDASSET = tr.IDASSET) and (ass.ASSETCATEGORY = tr.ASSETCATEGORY)"; }
        }
        #endregion RESTRICT_ASSET_POS
        #region RESTRICT_EVENTMERGE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENTMERGE_POS
        {
            get { return "left outer join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = 'AIN') and (ev.EVENTCODE = 'LPP')"; }
        }
        #endregion RESTRICT_EVENTMERGE_POS
        #region RESTRICT_COLMERGE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_COLMERGE_POS
        {
            get { return "ev.VALORISATION as ACCRUEDINTEREST"; }
        }
        #endregion RESTRICT_COLMERGE_POS
        #region RESTRICT_PARTYROLE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_PARTYROLE_POS
        {
            get
            {
                return ReflectionTools.ConvertEnumToString<PartyRoleEnum>(PartyRoleEnum.Custodian); 
            }
        }
        #endregion RESTRICT_PARTYROLE_POS
        #region GPRODUCT_VALUE
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        public override string GPRODUCT_VALUE
        {
            get
            {
                return ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.OverTheCounter); 
            }
        }
        #endregion GPRODUCT_VALUE


        #endregion Accesors
        #region Constructors
        public PosKeepingGen_OTC(PosKeepingGenProcess pPosKeepingGenProcess) : base(pPosKeepingGenProcess)
        {
        }
		#endregion Constructors

        #region Methods

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // EOD/CLOSINGDAY OVERRIDE METHODS (VIRTUAL SUR PosKeepingGenProcessBase)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region EOD_MarketGen
        /// <summary>
        /// Méthode principale de traitement de tenue de position de fin de journée par ENTITE/MARCHE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Traitement séquentiel</para>
        ///<para>  1 ● Contrôles (Trades incomplets, sans événements</para>
        ///<para>  2 ● Merge de trades</para>
        ///<para>  3 ● Mise à jour des clôture candidates (par clé de position)</para>
        ///<para>  5 ● Dénouement manuel d'options (Lecture dans POSREQUEST des demandes)</para>
        ///<para>  6 ● Dénouement automatique d'options</para>
        ///<para>  7 ● Livraison future (Lecture dans POSREQUEST des demandes)</para>
        ///<para>  8 ● Clôture des futures à l'échéance</para>
        ///<para>  9 ● Compensation automatique (par clé de position)</para>
        ///<para> 10 ● Calcul des Cash-Flows (ex. Marges)</para>
        ///<para> 11 ● Calcul des frais</para>
        ///<para> 12 ● Corporate actions</para>
        ///<para> 13 ● Calcul des UTI/PUTI</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>/// </summary>
        /// <param name="pIdPR">Id de la ligne de regroupement</param>
        /// <param name="pIdEM">Id ENTITE/MARCHE</param>
        /// <returns>Cst.ErrLevel</returns>
        // PM 20130212 [18414] Add Position Cascading
        // EG 20130701 AllocMissing refactoring
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180221 [23769] Upd Signature
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Add ClosingReopeningPosition
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        public override Cst.ErrLevel EOD_MarketGen(int pIdPR, int pIdEM, string pMarketShortAcronym, int pIdM, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                m_LstSubPosRequest = new List<IPosRequest>();
                m_TemplateDataDocumentTrade = new Hashtable();

                // STEP 0 : INITIALISATION POSREQUEST REGROUPEMENT
                m_MarketPosRequest = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel, pIdPR, m_MasterPosRequest, pIdEM,
                    ProcessStateTools.StatusProgressEnum, m_MasterPosRequest.IdPR, m_LstSubPosRequest, pGroupProduct);

                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5000), 0,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(pMarketShortAcronym, pIdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));
                Logger.Write();

                if (0 < m_LstSubPosRequest.Count)
                    AddSubPosRequest(m_LstMarketSubPosRequest, m_LstSubPosRequest[0]);

                m_MarketPosRequest.IdentifiersSpecified = true;
                m_MarketPosRequest.Identifiers = m_Product.CreatePosRequestKeyIdentifier();
                m_MarketPosRequest.Identifiers.Entity = Queue.GetStringValueIdInfoByKey("ENTITY");
                m_MarketPosRequest.Identifiers.CssCustodian = Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN");
                m_MarketPosRequest.Identifiers.Market = pMarketShortAcronym;

                m_MarketPosRequest.IdMSpecified = true;
                m_MarketPosRequest.IdM = pIdM;

                // CONTROLS
                if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                {
                    codeReturn = EOD_ControlGen();
                }
                // STEP : MERGE (MERGE)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_MergeGen();
                }
                // STEP : MISE A JOUR DES CLOTURES (UPDENTRY)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_UpdateEntryGen();
                }
                // STEP : COMPENSATION AUTOMATIQUE
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ClearingEndOfDay();
                }
                // STEP : CALCUL DES CASH-FLOWS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CashFlowsGen();
                }
                // STEP : CALCUL DES FRAIS DE GARDE
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_Safekeeping();
                }
                // STEP : CALCUL DES FRAIS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_FeesGen();
                }
                // STEP : CLOSING/REOPENING ACTIONS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ClosingReopeningActionsGen();
                }
                // STEP : CORPORATE ACTIONS (OST)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CorporateActionsGen(pIdM);
                }
                // STEP : CALCUL DES UTI
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_UTICalculation();
                }

                if ((Cst.ErrLevel.FAILURE != codeReturn) && (Cst.ErrLevel.IRQ_EXECUTED != codeReturn))
                {
                    if (IsSubRequest_Error(Cst.PosRequestTypeEnum.RequestTypeAll))
                        codeReturn = Cst.ErrLevel.FAILURE;
                    else
                    {
                        codeReturn = Cst.ErrLevel.SUCCESS;
                        ProcessStateTools.StatusEnum _status = ProcessStateTools.StatusEnum.ERROR;
                        if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                            _status |= ProcessStateTools.StatusEnum.WARNING;
                        if (IsSubRequest_Status(Cst.PosRequestTypeEnum.EOD_CorporateActionGroupLevel, _status))
                            codeReturn = Cst.ErrLevel.FAILURE;
                    }
                }
                return codeReturn;
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                // Update POSREQUEST GROUP
                // EG 20130620 Gestion Status
                if ((Cst.ErrLevel.FAILURE == codeReturn) || (Cst.ErrLevel.IRQ_EXECUTED == codeReturn))
                    UpdatePosRequest(codeReturn, m_MarketPosRequest, m_MarketPosRequest.IdPR_PosRequest);
                else
                {
                    UpdatePosRequestGroupLevel(m_MarketPosRequest, ProcessStateTools.StatusSuccessEnum);
                }
            }
        }

        #endregion EOD_MarketGen

        #region CLOSINGDAY_EODControl
        /// <summary>
        /// Controle du traitement EOD (exclusivement lors de la clôture de journée)
        /// </summary>
        /// <param name="pIdA_Entity"></param>
        /// <param name="pIdA_Css"></param>
        /// <param name="pIdA_Custodian"></param>
        /// <param name="pIdEM"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdPR_Parent"></param>
        /// FI 20160819 [22364] Modify
        // EG 20171128 [23331] Upd
        // EG 20180221 [23769] Upd Signature
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        protected override Cst.ErrLevel CLOSINGDAY_EODControl(int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness, 
            int pIdPR_Parent, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            int newIDPR = pIdPR_Parent;
            #region EOD PROCESS exist and his status is SUCCESS
            // EG 20151202 GetDataRequestWithIsolationLevel
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.ClosingDay, pDtBusiness, pIdEM, null, null, null);
            if (null != ds)
            {
                newIDPR++;
                codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                    Cst.PosRequestTypeEnum.ClosingDay_EndOfDayGroupLevel, pIdPR_Parent, pGroupProduct);
            }
            #endregion EOD PROCESS exist and his status is SUCCESS

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region NEW ALLOC AFTER LAST EOD PROCESS
                // EG 20151202 GetDataRequestWithIsolationLevel
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.Entry, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.ClosingDay_EntryGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion NEW ALLOC AFTER LAST EOD PROCESS
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region NEW REMOVE ALLOC AFTER LAST EOD PROCESS
                // FI 20160819 [22364] Add
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.RemoveAllocation, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.ClosingDay_RemoveAllocationGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion NEW REMOVE ALLOC AFTER LAST EOD PROCESS
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region EVENT (MISSING)
                // EG 20151202 GetDataRequestWithIsolationLevel
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EventMissing, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EventMissing, pIdPR_Parent, pGroupProduct);
                }
                #endregion EVENT (MISSING)
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region AUTOMATIC CLEARING BOOK
                // EG 20151202 GetDataRequestWithIsolationLevel
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.ClearingEndOfDay, pDtBusiness, pIdEM, 480, null, null);
                if (null != ds)
                {
                    // EG 20131223 Test Trade candidat à Compensation (lecture paramètres de BOOKPOSEFCT)
                    IPosRequest posRequestGroupLevel = null;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                            (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn)))
                        {
                            IPosRequest _posRequest = PosKeepingTools.SetPosRequestClearing(m_Product, Cst.PosRequestTypeEnum.ClearingEndOfDay, SettlSessIDEnum.EndOfDay, dr);
                            IPosKeepingData m_PosKeepingData = m_Product.CreatePosKeepingData();
                            m_PosKeepingData.SetPosKey(CS, null, _posRequest.PosKeepingKey.IdI, _posRequest.PosKeepingKey.UnderlyingAsset, _posRequest.PosKeepingKey.IdAsset,
                                                       _posRequest.PosKeepingKey.IdA_Dealer, _posRequest.PosKeepingKey.IdB_Dealer,
                                                       _posRequest.PosKeepingKey.IdA_Clearer, _posRequest.PosKeepingKey.IdB_Clearer);
                            m_PosKeepingData.SetAdditionalInfo(_posRequest.PosKeepingKey.IdA_EntityDealer, _posRequest.PosKeepingKey.IdA_EntityClearer);
                            m_PosKeepingData.SetBookDealerInfo(CS, null, _posRequest.PosKeepingKey.UnderlyingAsset, pDtBusiness);

                            bool isOk = m_PosKeepingData.IsIgnorePositionEffect_BookDealer && ExchangeTradedDerivativeTools.IsPositionEffect_Open(m_PosKeepingData.PositionEffect_BookDealer);
                            // EG 20140403 [19734][19702][19814] Compensation automatique EOD non autorisée par référentiel - Prise en compte des paramètrages sur BOOK/BOOKPOSEFCT
                            isOk |= (false == m_PosKeepingData.IsClearingEOD_BookDealer);

                            if (false == isOk)
                            {
                                // EG 20140102 [19428] Insertion de la ligne Titre - EOD_ClearingEndOfDayGroupLevel - une seule fois 
                                if (null == posRequestGroupLevel)
                                {
                                    newIDPR++;
                                    posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel, newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness,
                                        ProcessStateTools.StatusErrorEnum, pIdPR_Parent, m_LstSubPosRequest, pGroupProduct);
                                }
                                //Insertion ligne Compensation en erreur
                                SQLUP.GetId(out int _idPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                                _posRequest = m_Product.CreatePosRequestClosingDayControl(Cst.PosRequestTypeEnum.ClearingEndOfDay, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness);
                                _posRequest.IdPR = _idPR;
                                // EG 20140113
                                _posRequest.Status = ProcessStateTools.StatusErrorEnum;
                                _posRequest.StatusSpecified = true;
                                _posRequest.IdPR_PosRequest = newIDPR;
                                _posRequest.IdPR_PosRequestSpecified = true;
                                // EG 20171128 [23331]
                                _posRequest.PosKeepingKeySpecified = true;
                                _posRequest.IdTSpecified = false;
                                // EG 20170109 New currentGroupProduct
                                _posRequest.GroupProductSpecified = (pGroupProduct.HasValue);
                                if (_posRequest.GroupProductSpecified)
                                    _posRequest.GroupProduct = pGroupProduct.Value;

                                _posRequest.SetPosKey(m_PosKeepingData.IdI, m_PosKeepingData.UnderlyingAsset, m_PosKeepingData.IdAsset, m_PosKeepingData.IdA_Dealer, m_PosKeepingData.IdB_Dealer,
                                    m_PosKeepingData.IdA_Clearer, m_PosKeepingData.IdB_Clearer);
                                
                                //PosKeepingTools.InsertPosRequest(CS, null, _idPR, _posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, posRequestGroupLevel.idPR);
                                PosKeepingTools.InsertPosRequest(CS, null, _idPR, _posRequest, m_PKGenProcess.Session, IdProcess, posRequestGroupLevel.IdPR);
                                // EG 20171128 [23333]
                                AddSubPosRequest(m_LstSubPosRequest, _posRequest);
                            }
                        }
                    }
                    if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                    {
                        if (null == posRequestGroupLevel)
                        {
                            newIDPR++;
                            InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel, newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness,
                                ProcessStateTools.StatusSuccessEnum, pIdPR_Parent, m_LstSubPosRequest, pGroupProduct);
                        }
                    }
                }
                #endregion AUTOMATIC CLEARING BOOK
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region CASH FLOWS CALCULATION
                ds = PosKeepingTools.GetDataClosingDayCashFlowsControl(CS, pDtBusiness, pIdEM);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion CASH FLOWS CALCULATION
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region SAFEKEEPING CALCULATION
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion SAFEKEEPING CALCULATION
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region FEES CALCULATION
                // EG 20151202 GetDataRequestWithIsolationLevel
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_FeesGroupLevel, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_FeesGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion FEES CALCULATION
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region UTI CALCULATION
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion UTI CALCULATION
            }
            return codeReturn;
        }
        #endregion CLOSINGDAY_EODControl
        #region CLOSINGDAY_EODMarketGen
        /// <summary>
        /// Méthode principale de traitement de tenue de position de cloture de journée par ENTITE/MARCHE
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Traitement séquentiel</para>
        /// <para>  1 ● Shifting de position</para>
        /// <para>────────────────────────────────────────────────────────────────────────────────────────────────</para>/// </summary>
        /// <param name="pIdPR">Id de la ligne de regroupement</param>
        /// <param name="pIdEM">Id ENTITE/MARCHE</param>
        /// <param name="pMarketIdentifier">Identifiant du marché</param>
        /// <param name="pIdM">Id du marché</param>
        /// <param name="pIdM">Id du business center du marché</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Add ClosingReopeningPosition
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        public override Cst.ErrLevel CLOSINGDAY_EODMarketGen(int pIdPR, int pIdEM, string pMarketShortAcronym, int pIdM, string pIdBC, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                m_LstSubPosRequest = new List<IPosRequest>();
                m_TemplateDataDocumentTrade = new Hashtable();

                // STEP 0 : INITIALISATION POSREQUEST REGROUPEMENT MARKET
                m_MarketPosRequest = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingDay_EOD_MarketGroupLevel, pIdPR, m_MasterPosRequest, pIdEM,
                    ProcessStateTools.StatusProgressEnum, m_MasterPosRequest.IdPR, m_LstSubPosRequest, pGroupProduct);

                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5000), 0,
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(pMarketShortAcronym, pIdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));

                if (0 < m_LstSubPosRequest.Count)
                    AddSubPosRequest(m_LstMarketSubPosRequest, m_LstSubPosRequest[0]);

                m_MarketPosRequest.IdentifiersSpecified = true;
                m_MarketPosRequest.Identifiers = m_Product.CreatePosRequestKeyIdentifier();
                m_MarketPosRequest.Identifiers.Entity = Queue.GetStringValueIdInfoByKey("ENTITY");
                m_MarketPosRequest.Identifiers.CssCustodian = Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN");
                m_MarketPosRequest.Identifiers.Market = pMarketShortAcronym;

                m_MarketPosRequest.IdMSpecified = true;
                m_MarketPosRequest.IdM = pIdM;

                // STEP 1 : TRADES INCOMPLETS
                codeReturn = EOD_AllocMissing(m_MarketPosRequest);
                // STEP 2 : CLOSING/REOPENING ACTIONS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ClosingReopeningActionsGen();
                }
                // STEP 3 : CORPORATE ACTION (SOD)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CorporateActionsGen(pIdM);
                }

                if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                    codeReturn = IsSubRequest_Error(Cst.PosRequestTypeEnum.RequestTypeAll | Cst.PosRequestTypeEnum.CorporateAction) ? Cst.ErrLevel.FAILURE : Cst.ErrLevel.SUCCESS;
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw;
            }
            finally
            {
                // Update POSREQUEST GROUP
                if (Cst.ErrLevel.FAILURE == codeReturn)
                    UpdatePosRequest(codeReturn, m_MarketPosRequest, m_MarketPosRequest.IdPR_PosRequest);
                else if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                    UpdatePosRequestGroupLevel(m_MarketPosRequest, ProcessStateTools.StatusSuccessEnum);
                else
                    UpdateIRQPosRequestGroupLevel(m_MarketPosRequest);
            }
            return codeReturn;
        }
        #endregion CLOSINGDAY_EODMarketGen

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // OVERRIDE METHODS (VIRTUAL SUR PosKeepingGenProcessBase)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region DeserializeTrade
        /// <summary>
        /// Alimente m_TradeLibrary et  m_RptSideProductContainer
        /// </summary>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// FI 20141218 [XXXXX] Modify
        protected override void DeserializeTrade(IDbTransaction pDbTransaction, int pIdT)
        {
            base.DeserializeTrade(pDbTransaction, pIdT);
            if (m_TradeLibrary.Product.ProductBase.IsReturnSwap)
                m_RptSideProductContainer = new ReturnSwapContainer(CS, pDbTransaction, (IReturnSwap)m_TradeLibrary.Product, m_TradeLibrary.DataDocument);
        }
        #endregion DeserializeTrade

        #region GetQueueForEODCashFlows
        // EG 20150113 [20501]
        // EG 20150302 Call AddTimeRateSourceToDate to DtQuiote (CFD FOREX)
        /// EG 20170510 [23153] Add eodComplement
        // EG 20180205 [23769] Upd GetQuoteLock 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd Use GetQuoteLockWithKeyQuote
        protected override EventsValMQueue GetQueueForEODCashFlows(DataRow pRow)
        {
            EventsValMQueue mQueue = null;
            bool isEOD = (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);

            string _tradeIdentifier = pRow["IDENTIFIER"].ToString();
            int _idT = Convert.ToInt32(pRow["IDT"]);
            int _idAsset = Convert.ToInt32(pRow["IDASSET"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal _qty = Convert.ToDecimal(pRow["QTY"]);
            string _assetIdentifier = pRow["ASSET_IDENTIFIER"].ToString();
            DateTime _dtQuote = m_MasterPosRequest.DtBusiness;
            Cst.UnderlyingAsset _underlyingAsset = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), pRow["ASSETCATEGORY"].ToString());
            // EG 20150302 New
            if (_underlyingAsset == Cst.UnderlyingAsset.FxRateAsset)
            {
                _dtQuote = AssetTools.AddTimeRateSourceToDate(CSTools.SetCacheOn(CS, 1, null), _underlyingAsset, _idAsset, _dtQuote);
            }

            SystemMSGInfo errReadOfficialClose = null;
            Quote quote = GetQuoteLockWithKeyQuote(_idAsset, _dtQuote, _assetIdentifier, _underlyingAsset, ref errReadOfficialClose) as Quote;
            // EG 20150113 [20501] On va dans EVENTSVAL même si pas de quotation
            if ((null != errReadOfficialClose) && isEOD && (0 <_qty))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, errReadOfficialClose.SysMsgCode, 0, errReadOfficialClose.LogParamDatas));
            }
            // EG 20150617 Add (_underlyingAsset != Cst.UnderlyingAsset.Bond)
            if ((false == quote.valueSpecified) && (_underlyingAsset != Cst.UnderlyingAsset.Bond) && isEOD && (0 < _qty))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                // EG 20150218 Change 5161 to 5164
                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5164), 0, new LogParam(LogTools.IdentifierAndId(_tradeIdentifier, _idT))));
            }

            if (null != quote)
            {
                quote.time = m_MasterPosRequest.DtBusiness;
                quote.action = DataRowState.Added.ToString();
                quote.isEODSpecified = true;
                quote.isEOD = isEOD;

                // EG 20170510 [23153] New
                if (isEOD)
                {
                    quote.eodComplementSpecified = isEOD;
                    quote.eodComplement = new Quote_EODComplement
                    {
                        isPosKeeping_BookDealer = true,
                        posQuantityPrevAndActionsDay = _qty,
                        idAEntity = m_MasterPosRequest.IdA_Entity
                    };
                }

                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString = CS,
                    id = _idT,
                    requester = Queue.header.requester
                };
                mQueue = new EventsValMQueue(mQueueAttributes, quote)
                {
                    idInfo = new IdInfo
                    {
                        id = _idT,
                        // EG 20170510 [23153] Add Cst.ProductGProduct_OTC
                        idInfos = new DictionaryEntry[]{
                                new DictionaryEntry("ident", "TRADE"),
                                new DictionaryEntry("identifier", _tradeIdentifier),
                                new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_OTC)}
                    },
                    idInfoSpecified = true
                };
            }
            return mQueue;
        }
        #endregion GetQueueForEODCashFlows

        #region InsertPosKeepingEvents
        /// <summary>
        /// <para>Insertion d'un package Evénement : OFFSETTING/POSITIONCORRECTION/POSITIONTRANSFER</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>OFFSETTING          OFS     PAR/TOT     GRP</para>
        /// <para>ou</para>
        /// <para>POSITION CORRECTION POC     PAR/TOT     GRP</para>
        /// <para>ou</para>
        /// <para>POSITION TRANSFER   POT     PAR/TOT     GRP</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>NOMINAL             NOM     INT/TER     REC</para>
        /// <para>QUANTITY            QTY     INT/TER     REC</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>REALIZED MARGIN     RMG     LPC         REC</para>
        /// <para>VARIATION           VMG     LPC         REC</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>FEE                 OPP     ttt         </para>
        /// </summary>
        /// <param name="pDbTransaction">Current Transaction</param>
        /// <param name="pRow">ClosedTrade or ClosingTrade)</param>
        /// <param name="pRow2">if pRow = ClosedTrade then pRow2 = ClosingTrade else ClosedTrade</param>
        /// <param name="pActionQuantity"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        // EG 20120327 Méthode InitPaymentForEvent déplacée dans Trade.cs de Process
        // EG 20170127 Qty Long To Decimal
        protected override Cst.ErrLevel InsertPosKeepingEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRow2,
            DataRow pRowPosActionDet, int pIdPADET, ref int pIdE, bool pIsClosed)
        {
            #region Variables
            int idT = Convert.ToInt32(pRow["IDT"]);
            int idE = pIdE;
            int idE_Event = Convert.ToInt32(pRow["IDE_EVENT"]);
            // EG 20170127 Qty Long To Decimal
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            decimal remainQuantity;
            if (pIsClosed)
                remainQuantity = Convert.ToDecimal(pRowPosActionDet["ORIGINALQTY_CLOSED"]) - qty;
            else
                remainQuantity = Convert.ToDecimal(pRowPosActionDet["ORIGINALQTY_CLOSING"]) - qty;

            bool isDealerBuyer = IsTradeBuyer(pRow["SIDE"].ToString());
            #endregion Variables

            #region EVENT OFS (OffSetting) / POC (Position Cancelation)
            string eventCode = PosKeepingEventCode;
            string eventType = (remainQuantity == 0) ? EventTypeFunc.Total : EventTypeFunc.Partiel;
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, idT, idE, idE_Event, null, 1, 1, null, null, null, null,
    eventCode, eventType, DtBusiness, DtBusiness, DtBusiness, DtBusiness, qty, null, UnitTypeEnum.Qty.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.GroupLevel, DtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != m_PosRequest) && m_PosRequest.NotesSpecified)
                codeReturn = m_EventQuery.InsertEventDet_Notes(pDbTransaction, idE, m_PosRequest.Notes);
            #endregion EVENT OFS (OffSetting) / POC (Position Cancelation)

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region EVENT NOM/QTY (Nominal/Quantité)
                idE_Event = idE;
                idE++;
                codeReturn = InsertNominalQuantityEvent(pDbTransaction, idT, ref idE, idE_Event, DtBusiness, isDealerBuyer, qty, remainQuantity, pIdPADET);
                #endregion EVENT NOM/QTY (Nominal/Quantité)
            }

            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != pRow) && (null != pRow2))
            {
                if (PosKeepingData.Product.IsRTS)
                {
                    #region EVENT RMG (Realized margin)
                    idE++;
                    #region Realized margin calculation
                    decimal price = 0;
                    decimal closingPrice = 0;
                    if (pIsClosed)
                    {
                        if (false == Convert.IsDBNull(pRow["PRICE"]))
                            price = Convert.ToDecimal(pRow["PRICE"]);
                        if (false == Convert.IsDBNull(pRow2["PRICE"]))
                            closingPrice = Convert.ToDecimal(pRow2["PRICE"]);
                        isDealerBuyer = IsTradeBuyer(pRow["SIDE"].ToString());
                    }
                    else
                    {
                        if (false == Convert.IsDBNull(pRow2["PRICE"]))
                            price = Convert.ToDecimal(pRow2["PRICE"]);
                        if (false == Convert.IsDBNull(pRow["PRICE"]))
                            closingPrice = Convert.ToDecimal(pRow["PRICE"]);
                        isDealerBuyer = IsTradeBuyer(pRow2["SIDE"].ToString());
                    }
                    #endregion Realized margin calculation

                    codeReturn = InsertRealizedMarginEvent(pDbTransaction, idT, ref idE, idE_Event, isDealerBuyer, price, closingPrice, qty, pIdPADET, pIsClosed);
                    #endregion EVENT RMG (Realized margin)
                }
            }

            // Insertion des restitutions
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (IsRequestPositionCancelation || IsRequestPositionTransfer))
            {
                 if (Cst.ErrLevel.SUCCESS == codeReturn)
                 {
                    #region EVENT OPP (Restitution)
                     codeReturn = InsertReversalFeesEvents(pDbTransaction, pRow, pIdPADET, idE_Event, ref idE);
                    #endregion EVENT OPP (Restitution)
                 }

                // EG 20150716 [21103] New 
                // Restitution des frais de garde (SKP) si demandé et DTBUSINESS >= DTSETTLT
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                 {
                    #region EVENT SKP (Restitution)
                     codeReturn = InsertReversalSafekeepingEvents(pDbTransaction, idT, idE_Event, qty, pIdPADET);
                    #endregion EVENT SKP (Restitution)
                 }
            }

            pIdE = idE;
            return codeReturn;
        }
        #endregion InsertPosKeepingEvents

        #region InsertNominalQuantityEvent
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Add PosKeepingData parameter
        protected override Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            return InsertNominalQuantityEvent(pDbTransaction, PosKeepingData, pIdT, ref pIdE, pIdE_Event, pDtBusiness, pIsDealerBuyer, pQty, pRemainQty, pIdPADET);
        }
        // EG 20190613 [24683] Add PosKeepingData parameter
        protected override Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {

            #region EVENT NOM/QTY
            Nullable<int> idA_Payer;
            Nullable<int> idB_Payer;
            Nullable<int> idA_Receiver;
            Nullable<int> idB_Receiver;
            if (pIsDealerBuyer)
            {
                idA_Payer = pPosKeepingData.IdA_Clearer;
                idB_Payer = pPosKeepingData.IdB_Clearer;
                idA_Receiver = pPosKeepingData.IdA_Dealer;
                idB_Receiver = pPosKeepingData.IdB_Dealer;
            }
            else
            {
                idA_Payer = pPosKeepingData.IdA_Dealer;
                idB_Payer = pPosKeepingData.IdB_Dealer;
                idA_Receiver = pPosKeepingData.IdA_Clearer;
                idB_Receiver = pPosKeepingData.IdB_Clearer;
            }

            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;
            string eventType = EventTypeFunc.Quantity;
            //return Cst.ErrLevel.SUCCESS;
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
    eventCode, eventType, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness, pQty, null, UnitTypeEnum.Qty.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            #endregion EVENT NOM/QTY

            return codeReturn;

        }
        #endregion InsertNominalQuantityEvent

        #region SetPaymentFeesForEvent
        /// <summary>
        /// Remplacement de la quantité d'origine du trade par la quantité dénouée ou corrigée
        /// </summary>
        /// <param name="pIdT">Id du trade clôturant</param>
        /// <param name="pQty">Quantité clôturée</param>
        /// <param name="pPaymentFees">Frais (pour restitution)</param>
        /// <param name="pDetail">PosRequestDetail</param>
        // EG 20150716 [21103] (New - Override) + Gestion de la restitution des frais de garde
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override void SetPaymentFeesForEvent(int pIdT, decimal pQty, IPayment[] pPaymentFees, IPosRequestDetail pDetail)
        {
            // Restitution des frais initiaux (OPP)
            if (ArrFunc.IsFilled(pPaymentFees))
            {
                EventQuery.InitPaymentForEvent(CS, pPaymentFees, m_TradeLibrary.DataDocument, out int nbEvent);
                pDetail.NbAdditionalEvents += nbEvent;
            }
        }
        #endregion SetPaymentFeesForEvent
        #region SetPosKeepingAsset
        /// <summary>
        ///  Retourne l'asset (IPosKeepingAsset)
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        // EG 20180221 [23769] Public pour gestion Asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        public override PosKeepingAsset SetPosKeepingAsset(IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, MapDataReaderRow pMapDr)
        {
            PosKeepingAsset asset = base.SetPosKeepingAsset(pDbTransaction, pUnderlyingAsset, pMapDr);
            if (null != asset)
            {
                bool isExistPriceCurrency = StrFunc.IsFilled(asset.priceCurrency);

                if (null != pMapDr["IDA_ISSUER"])
                    asset.IdA_Issuer = Convert.IsDBNull(pMapDr["IDA_ISSUER"].Value) ? 0 : Convert.ToInt32(pMapDr["IDA_ISSUER"].Value);
                if (null != pMapDr["IDB_ISSUER"])
                    asset.IdB_Issuer = Convert.IsDBNull(pMapDr["IDB_ISSUER"].Value) ? 0 : Convert.ToInt32(pMapDr["IDB_ISSUER"].Value);

                //--------------------------------------------------------------
                //PL 20150202 Gestion du cas GBX --> GBP [TRIM 20762]
                //            A l'image de ce qui est opéré sur les ETD, on utilise ici le "contractMultiplier" pour y stocker le FACTOR.
                //            Cette approche permet de conserver les prix en devise d'origine (ex. GBX) et donc de les stocker tels quels (EVENTDET). 
                //--------------------------------------------------------------
                Tools.GetQuotedCurrency(CS, pDbTransaction, SQL_Currency.IDType.IdC, (isExistPriceCurrency ? asset.priceCurrency : asset.currency),
                                        out string quotedCurrency, out int? quotedCurrencyFactor);
                if (quotedCurrencyFactor.HasValue)
                {
                    asset.contractMultiplier /= quotedCurrencyFactor.Value;
                    asset.currency = quotedCurrency;
                }
                //--------------------------------------------------------------
            }
            return asset;
        }
        #endregion SetPosKeepingAsset

        #region SetTradeMerge
        // EG 20150723 New
        protected override void SetTradeMerge(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocumentContainer, TradeMergeRule pTmr,
            List<TradeCandidate> pLstTradeMerge, TradeCandidate pSourceTradeBase)
        {
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal totalQty = pLstTradeMerge.Sum(trade => trade.Qty);

            if (pDataDocumentContainer.CurrentProduct.IsReturnSwap)
            {

                IReturnSwap returnSwap = pDataDocumentContainer.CurrentProduct.Product as IReturnSwap;
                ReturnSwapContainer returnSwapContainer = new ReturnSwapContainer(CS, pDbTransaction, returnSwap, pDataDocumentContainer);

                // Prix (du ReturnLeg)
                IReturnLegValuationPrice _price = returnSwapContainer.ReturnLeg.RateOfReturn.InitialPrice;
                if (_price.NetPriceSpecified && (_price.NetPrice.PriceExpression == PriceExpressionEnum.AbsoluteTerms))
                {
                    if (pTmr.Context.PriceValue.HasValue)
                        // Prix identique (contexte = IDENTICAL)
                        _price.NetPrice.Amount.DecValue = pSourceTradeBase.Price;
                    else
                    {
                        // Prix moyen pondéré (contexte = IDENTICAL)
                        decimal _lastPx = pLstTradeMerge.Sum(trade => trade.Price * trade.Qty) / totalQty;

                        Nullable<Cst.RoundingDirectionSQL> direction = Cst.RoundingDirectionSQL.N;
                        // On prend le plus grand nombre de décimales des prix des trades sources
                        Nullable<int> precision = pLstTradeMerge.Max(trade => trade.RemainderPriceLength);
                        EFS_Round _round = new EFS_Round(direction.Value, precision.Value, _lastPx);
                        _price.NetPrice.Amount.DecValue  = _round.AmountRounded;
                    }
                }
                // Recalcul du notionalAmount
                if (returnSwapContainer.MainReturnLeg.First.Notional.NotionalAmountSpecified)
                {
                    decimal multiplier = 1;
                    if (returnSwapContainer.ReturnLeg.Underlyer.UnderlyerSingleSpecified &&
                        returnSwapContainer.ReturnLeg.Underlyer.UnderlyerSingle.NotionalBaseSpecified)
                        multiplier = returnSwapContainer.ReturnLeg.Underlyer.UnderlyerSingle.NotionalBase.Amount.DecValue / returnSwapContainer.MainOpenUnits.Value;
                    returnSwapContainer.MainReturnLeg.First.Notional.NotionalAmount.Amount.DecValue = totalQty * returnSwapContainer.MainInitialNetPrice.Value * multiplier;
                }
                returnSwapContainer.SetMainOpenUnits(totalQty);
            }
        }
        #endregion SetTradeMerge

        #endregion Methods

    }
}

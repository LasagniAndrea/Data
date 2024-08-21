#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
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
    public partial class PosKeepingGen_SEC : PosKeepingGenProcessBase
    {
        #region Accessors
        #region StoreProcedure_RemoveEOD
        protected override string StoreProcedure_RemoveEOD
        {
            get { return "UP_REMOVE_POSACTION_SEC"; }
        }
        #endregion StoreProcedure_RemoveEOD
        #region VW_TRADE_POS
        protected override string VW_TRADE_POS
        {
            get { return Cst.OTCml_TBL.VW_TRADE_POSSEC.ToString(); }
        }
        #endregion VW_TRADE_POS
        #region VW_TRADE_FUNGIBLE_LIGHT
        protected override string VW_TRADE_FUNGIBLE_LIGHT
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_LIGHT_SEC.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE_LIGHT
        #region VW_TRADE_FUNGIBLE
        protected override string VW_TRADE_FUNGIBLE
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_SEC.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE
        #region RESTRICT_EVENT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENT_POS
        {
            get { return "EVENTCODE in ('EST', 'DST')"; }
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
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
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
            get { return ReflectionTools.ConvertEnumToString<PartyRoleEnum>(PartyRoleEnum.Custodian); }
        }
        #endregion RESTRICT_PARTYROLE_POS
        #region GPRODUCT_VALUE
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        public override string GPRODUCT_VALUE
        {
            get
            {
                return ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.Security); 
            }
        }
        #endregion GPRODUCT_VALUE
        #region Asset
        // EG 20190926 [Maturity redemption] New
        protected PosKeepingAsset_BOND AssetBond
        {
            get { return (PosKeepingAsset_BOND)m_PosKeepingData.Asset; }
        }
        #endregion Asset
        #endregion Accesors

        #region Constructors
        public PosKeepingGen_SEC(PosKeepingGenProcess pPosKeepingGenProcess) : base(pPosKeepingGenProcess)
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
        ///<para>  4 ● Compensation automatique (par clé de position)</para>
        ///<para>  5 ● Calcul des Cash-Flows (ex. Marges)</para>
        ///<para>  6 ● Frais de garde</para>
        ///<para>  7 ● Calcul des frais</para>
        ///<para>  8 ● Corporate actions</para>
        ///<para>  9 ● Calcul des UTI/PUTI</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>/// </summary>
        /// <param name="pIdPR">Id de la ligne de regroupement</param>
        /// <param name="pIdEM">Id ENTITE/MARCHE</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Add ClosingReopeningPosition
        // EG 20190926 [Maturity redemption] Upd
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
                // STEP : MATURITY REDEMPTION ON DEBTSECURITY
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_MaturityRedemptionDebtSecurityGen();
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
                if ((Cst.ErrLevel.FAILURE == codeReturn) || (Cst.ErrLevel.IRQ_EXECUTED == codeReturn))
                    UpdatePosRequest(codeReturn, m_MarketPosRequest, m_MarketPosRequest.IdPR_PosRequest);
                else
                    UpdatePosRequestGroupLevel(m_MarketPosRequest, ProcessStateTools.StatusSuccessEnum);
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
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

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
                            // EG 20171128 [23331] Upd
                            AddSubPosRequest(m_LstSubPosRequest, _posRequest);
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
        // EG 20190918 Upd (PosRequtTypeEnum refactoring)
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
            if (m_TradeLibrary.Product.ProductBase.IsESE)
                m_RptSideProductContainer = new EquitySecurityTransactionContainer(CS, pDbTransaction, (IEquitySecurityTransaction)m_TradeLibrary.Product, m_TradeLibrary.DataDocument);
            else if (m_TradeLibrary.Product.ProductBase.IsDebtSecurityTransaction)
                m_RptSideProductContainer = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)m_TradeLibrary.Product, m_TradeLibrary.DataDocument);
        }
        #endregion DeserializeTrade

        #region GetQueueForEODCashFlows
        // EG 20150113 [20501]
        // EG 20150302 Call AddTimeRateSourceToDate to DtQuiote (CFD FOREX)
        // EG 20180205 [23769] Upd GetQuoteLock
        // EG 20180713 Set eodComplement
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
            Cst.UnderlyingAsset _underlyingAsset = ReflectionTools.ConvertStringToEnum<Cst.UnderlyingAsset>(pRow["ASSETCATEGORY"].ToString());

            SystemMSGInfo errReadOfficialClose = null;
            Quote quote = GetQuoteLockWithKeyQuote(_idAsset, _dtQuote, _assetIdentifier, _underlyingAsset, ref errReadOfficialClose) as Quote;
            // EG 20150113 [20501] On va dans EVENTSVAL même si pas de quotation
            if ((null != errReadOfficialClose) && isEOD && (0 < _qty))
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
                    idInfo = new IdInfo()
                    {
                        id = _idT,
                        idInfos = new DictionaryEntry[]{
                                new DictionaryEntry("ident", "TRADE"),
                                new DictionaryEntry("identifier", _tradeIdentifier),
                                new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_SEC)}
                    },
                    idInfoSpecified = true
                };
            }
            return mQueue;
        }
        #endregion GetQueueForEODCashFlows

        #region GetQuoteLockWithKeyQuote
        // EG 20190716 [VCL : New FixedIncome] New
        // EG 20190730 Upd (parameter pQuoteNotFoundIsLog)
        public override Quote GetQuoteLockWithKeyQuote(int pIdAsset, DateTime pDate, string pAssetIdentifier,
            Cst.UnderlyingAsset pUnderlyingAsset, ref SystemMSGInfo pErrReadOfficialClose)
        {
            Quote quote;
            if (Cst.UnderlyingAsset.Bond == pUnderlyingAsset)
            {
                quote = m_PKGenProcess.ProcessCacheContainer.GetQuoteLock(pIdAsset, pDate, pAssetIdentifier,
                 QuotationSideEnum.OfficialClose, pUnderlyingAsset, new KeyQuoteAdditional(AssetMeasureEnum.CleanPrice, false),
                 ref pErrReadOfficialClose) as Quote;
                if ((null == quote) || (false == quote.valueSpecified))
                    quote = m_PKGenProcess.ProcessCacheContainer.GetQuoteLock(pIdAsset, pDate, pAssetIdentifier,
                        QuotationSideEnum.OfficialClose, pUnderlyingAsset, new KeyQuoteAdditional(AssetMeasureEnum.DirtyPrice),
                        ref pErrReadOfficialClose) as Quote;
            }
            else
            {
                quote = m_PKGenProcess.ProcessCacheContainer.GetQuoteLock(pIdAsset, pDate, pAssetIdentifier,
                 QuotationSideEnum.OfficialClose, pUnderlyingAsset, new KeyQuoteAdditional(), ref pErrReadOfficialClose) as Quote;
            }
            return quote;
        }
        #endregion GetQuoteLockWithKeyQuote


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
        /// <para>GROSS AMOUNT        LPC     GAM         </para>
        /// <para>FEE                 OPP     ttt         </para>
        /// </summary>
        /// <param name="pDbTransaction">Current Transaction</param>
        /// <param name="pRow">ClosedTrade or ClosingTrade)</param>
        /// <param name="pRow2">if pRow = ClosedTrade then pRow2 = ClosingTrade else ClosedTrade</param>
        /// <param name="pActionQuantity"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        // EG 20120327 Méthode InitPaymentForEvent déplacée dans Trade.cs de Process
        // EG 20190730 Upd RealizedMargin sur DST
        protected override Cst.ErrLevel InsertPosKeepingEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRow2,
            DataRow pRowPosActionDet, int pIdPADET, ref int pIdE, bool pIsClosed)
        {
            #region Variables
            int idT = Convert.ToInt32(pRow["IDT"]);
            int idE = pIdE;
            int idE_Event = Convert.ToInt32(pRow["IDE_EVENT"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal remainQuantity;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
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
                // EG 20150624 [21151] Alimentation des caractéristiques de l'asset DebtSecurity lié au DebtSecurityTransaction
                // EG 20151201 New Add EquitySecurityTransaction
                if ((PosKeepingData.Product.IsDebtSecurityTransaction || PosKeepingData.Product.IsEquitySecurityTransaction) && 
                    ((IsRequestPositionCancelation || IsRequestPositionTransfer)))
                    PosKeepingData.Asset = GetAsset(pDbTransaction, PosKeepingData.UnderlyingAsset, PosKeepingData.IdAsset, Cst.PosRequestAssetQuoteEnum.Asset);


                #region EVENT NOM/QTY (Nominal/Quantité)
                idE_Event = idE;
                idE++;
                codeReturn = InsertNominalQuantityEvent(pDbTransaction, idT, ref idE, idE_Event, DtBusiness, isDealerBuyer, qty, remainQuantity, pIdPADET);
                #endregion EVENT NOM/QTY (Nominal/Quantité)
            }

            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != pRow) && (null != pRow2))
            {
                if (PosKeepingData.Product.IsEquitySecurityTransaction || PosKeepingData.Product.IsDebtSecurityTransaction)
                {
                    #region EVENT RMG (Realized margin)
                    idE++;
                    #region Realized margin calculation
                    decimal price = 0;
                    decimal closingPrice = 0;
                    Nullable<decimal> closingAccIntRate = null;
                    Nullable<AssetMeasureEnum> typePrice = null;
                    Nullable<AssetMeasureEnum> typeClosingPrice = null;
                    if (pIsClosed)
                    {
                        if (false == Convert.IsDBNull(pRow["TYPEPRICE"]))
                            typePrice = ReflectionTools.ConvertStringToEnum<AssetMeasureEnum>(pRow["TYPEPRICE"].ToString());
                        if (false == Convert.IsDBNull(pRow["PRICE"]))
                            price = Convert.ToDecimal(pRow["PRICE"]);

                        if (false == Convert.IsDBNull(pRow2["TYPEPRICE"]))
                            typeClosingPrice = ReflectionTools.ConvertStringToEnum<AssetMeasureEnum>(pRow2["TYPEPRICE"].ToString());
                        if (false == Convert.IsDBNull(pRow2["PRICE"]))
                            closingPrice = Convert.ToDecimal(pRow2["PRICE"]);

                        if (false == Convert.IsDBNull(pRow2["ACCINTRATE"]))
                            closingAccIntRate = Convert.ToDecimal(pRow2["ACCINTRATE"]);

                        isDealerBuyer = IsTradeBuyer(pRow["SIDE"].ToString());
                    }
                    else
                    {
                        if (false == Convert.IsDBNull(pRow2["TYPEPRICE"]))
                            typePrice = ReflectionTools.ConvertStringToEnum<AssetMeasureEnum>(pRow2["TYPEPRICE"].ToString());
                        if (false == Convert.IsDBNull(pRow2["PRICE"]))
                            price = Convert.ToDecimal(pRow2["PRICE"]);

                        if (false == Convert.IsDBNull(pRow["TYPEPRICE"]))
                            typeClosingPrice = ReflectionTools.ConvertStringToEnum<AssetMeasureEnum>(pRow["TYPEPRICE"].ToString());
                        if (false == Convert.IsDBNull(pRow["PRICE"]))
                            closingPrice = Convert.ToDecimal(pRow["PRICE"]);

                        if (false == Convert.IsDBNull(pRow["ACCINTRATE"]))
                            closingAccIntRate = Convert.ToDecimal(pRow["ACCINTRATE"]);

                        isDealerBuyer = IsTradeBuyer(pRow2["SIDE"].ToString());
                    }
                    #endregion Realized margin calculation

                    bool isCanBeCalculated = true;
                    if (PosKeepingData.Product.IsDebtSecurityTransaction)
                    {
                        if (typeClosingPrice.GetValueOrDefault(AssetMeasureEnum.CleanPrice) != typePrice.GetValueOrDefault(AssetMeasureEnum.CleanPrice))
                        {
                            // Les prix doivent être sur la même mesure
                            isCanBeCalculated = closingAccIntRate.HasValue;
                            closingPrice += (closingAccIntRate.Value * ((typePrice.Value == AssetMeasureEnum.CleanPrice)?-1:1));
                        }
                    }

                    if (isCanBeCalculated)
                        codeReturn = InsertRealizedMarginEvent(pDbTransaction, idT, ref idE, idE_Event, isDealerBuyer, 
                            typePrice.GetValueOrDefault(AssetMeasureEnum.CleanPrice), price, closingPrice, qty, pIdPADET, pIsClosed);
                    #endregion EVENT RMG (Realized margin)
                }
            }

            // Insertion des restitutions
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (IsRequestPositionCancelation || IsRequestPositionTransfer))
            {
                #region EVENT GAM (Restitution)
                // EG 20150624 [21151] New
                // EG 20150920 [21374] Int (int32) to Long (Int64)
                // EG 20170127 Qty Long To Decimal
                decimal initialQty = 0;
                if (IsRequestPositionCancelation)
                    initialQty = ((IPosRequestDetCorrection)m_PosRequest.DetailBase).InitialQty;
                else if (IsRequestPositionTransfer)
                    initialQty = ((IPosRequestDetTransfer)m_PosRequest.DetailBase).InitialQty;
                if (PosKeepingData.Product.IsEquitySecurityTransaction || PosKeepingData.Product.IsDebtSecurityTransaction)
                    codeReturn = InsertReversalEvents(pDbTransaction, idT, ref idE, idE_Event, m_TradeLibrary.DataDocument, initialQty, qty, pIdPADET);
                #endregion EVENT GAM (Restitution)

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

        #region InsertReversalEvents
        /// <summary>
        /// Insertion des événements représentatifs de restitution en cas de TRANSFER|CORRECTION sur EquitySecurityTransaction/DebtSecurityTransaction
        /// <para>sont considérés les GAM, AIN et PAM </para>
        /// <para>REC : DtBusiness</para>
        /// <para>VAL : DtSettlement si DTBusiness est inférieure DtSettlement, DtBusiness sinon</para>
        /// </summary>
        /// EG 20150624 [21151] New
        /// EG 20150907 [21317] Refactoring
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// FI 20151228 [21660] Modify (Gestion du principalAmountRestitution et ISPAYMENT=false sur PAM et AIN) 
        // EG 20170127 Qty Long To Decimal
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        protected Cst.ErrLevel InsertReversalEvents(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
            DataDocumentContainer pDataDocument, decimal pInitialQty, decimal pQty, int pIdPADET)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            IPayment grossAmount = null;

            IMoney grossAmountRestitution = null;
            IMoney accruedInterestRestitution = null;
            IMoney principalAmountRestitution = null;

            decimal prorata = NotionalProrata(pInitialQty, pQty);
            if (pDataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                IEquitySecurityTransaction est = pDataDocument.CurrentProduct.Product as IEquitySecurityTransaction;
                // GAM Source
                grossAmount = est.GrossAmount;
                // GAM proraté
                grossAmountRestitution = GrossAmountProrataCalculation(pDataDocument, prorata);
            }
            else if (pDataDocument.CurrentProduct.IsDebtSecurityTransaction)
            {
                // FI 20230628 [XXXXX] use DebtSecurityTransactionContainer for ResolveSecurityAsset
                IDebtSecurityTransaction dst = new DebtSecurityTransactionContainer(pDataDocument.CurrentProduct.Product as IDebtSecurityTransaction, pDataDocument).DebtSecurityTransaction;
                // GAM Source
                grossAmount = dst.GrossAmount;
                // GrossAmount prorata
                grossAmountRestitution = GrossAmountProrataCalculation(pDataDocument, prorata);
                // AccruedInterest prorata
                accruedInterestRestitution = AccruedInterestProrataCalculation(pDataDocument, prorata);
                // PrincipalAmount prorata
                principalAmountRestitution = PrincipalAmountProrataCalculation(pDataDocument, pInitialQty, prorata);
            }

            if ((null != grossAmountRestitution) || (null != accruedInterestRestitution) || (null != principalAmountRestitution))
            {
                // SettlementDate GAM|AIN|PAM
                DateTime dtSettlement = DtBusiness;

                EFS_Payment payment = new EFS_Payment(m_PKGenProcess.Cs, pDbTransaction, grossAmount, pDataDocument);
                // La date Business courante est inférieure à la date de règlement originelle, on conserve la date de règlement sinon DtBusiness
                if (0 > DtBusiness.CompareTo(payment.AdjustedPaymentDate.DateValue))
                    dtSettlement = payment.AdjustedPaymentDate.DateValue;

                // Alimentation du sens originel Payer|Receiver ( il y aura inversion dans l'événement)
                IParty payer = pDataDocument.GetParty(payment.payerPartyReference.HRef);
                IParty receiver = pDataDocument.GetParty(payment.receiverPartyReference.HRef);
                Nullable<int> idA_Payer = payer.OTCmlId;
                Nullable<int> idB_Payer = pDataDocument.GetOTCmlId_Book(payment.payerPartyReference.HRef);
                Nullable<int> idA_Receiver = receiver.OTCmlId;
                Nullable<int> idB_Receiver = pDataDocument.GetOTCmlId_Book(payment.receiverPartyReference.HRef);

                if (null != grossAmountRestitution)
                {
                    pIdE++;
                    #region Insert GAM
                    // Insertion Evénement de restitution
                    codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                        EventCodeFunc.LinkedProductClosing, EventTypeFunc.GrossAmount, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                        grossAmountRestitution.Amount.DecValue, grossAmountRestitution.Currency, UnitTypeEnum.Currency.ToString(), null, null);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, dtSettlement, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, dtSettlement, true);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
                    #endregion Insert GAM
                }
                if (null != accruedInterestRestitution)
                {
                    pIdE++;
                    #region Insert AIN
                    // Insertion Evénement de restitution
                    codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                        EventCodeFunc.LinkedProductClosing, EventTypeFunc.AccruedInterestAmount, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                        accruedInterestRestitution.Amount.DecValue, accruedInterestRestitution.Currency, UnitTypeEnum.Currency.ToString(), null, null);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, dtSettlement, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        // FI 20151228 [21660] IsPAYMENT = false
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, dtSettlement, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
                    #endregion Insert GAM
                }


                if (null != principalAmountRestitution)
                {
                    pIdE++;
                    #region Insert PAM
                    // Insertion Evénement de restitution
                    codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                        EventCodeFunc.LinkedProductClosing, EventTypeFunc.PrincipalAmount, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                        principalAmountRestitution.Amount.DecValue, principalAmountRestitution.Currency, UnitTypeEnum.Currency.ToString(), null, null);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, dtSettlement, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, dtSettlement, false);

                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
                    #endregion Insert PAM
                }
            }

            return codeReturn;
        }
        #endregion InsertReversalEvents

        #region AccruedInterestProrataCalculation
        /// <summary>
        /// Calcul du montant des intérêts courus sur (POC|POT) par prorata quantité (corrigée|transférée) / quantité initiale
        /// </summary>
        /// <param name="pPosRequestTransfer"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pDebtSecurityContainer"></param>
        /// <returns></returns>
        /// EG 20150907 [21317] New
        protected override IMoney AccruedInterestProrataCalculation(DataDocumentContainer pDataDocument, decimal pProrata)
        {
            IMoney ret = null;

            // FI 20230628 [XXXXX] use DebtSecurityTransactionContainer for ResolveSecurityAsset
            IDebtSecurityTransaction dst = new DebtSecurityTransactionContainer(pDataDocument.CurrentProduct.Product as IDebtSecurityTransaction, pDataDocument).DebtSecurityTransaction;

            if (null != dst && dst.Price.AccruedInterestAmountSpecified)
            {
                ret = pDataDocument.CurrentProduct.Product.ProductBase.CreateMoney(0, dst.Price.AccruedInterestAmount.Currency);
                ret.Amount.DecValue = dst.Price.AccruedInterestAmount.Amount.DecValue * pProrata;
                // Application règles d'arrondi spécifiées sur DebtSecurity
                if (dst.DebtSecurity.Security.CalculationRulesSpecified &&
                    dst.DebtSecurity.Security.CalculationRules.AccruedInterestCalculationRulesSpecified &&
                    dst.DebtSecurity.Security.CalculationRules.AccruedInterestCalculationRules.RoundingSpecified)
                {
                    EFS_Round round = new EFS_Round(dst.DebtSecurity.Security.CalculationRules.AccruedInterestCalculationRules.Rounding, ret.Amount.DecValue);
                    ret.Amount.DecValue = round.AmountRounded;
                }
                
                else
                {
                    // Application règles d'arrondi spécifiées sur devise
                    EFS_Cash round = new EFS_Cash(CS, ret.Amount.DecValue, dst.Price.AccruedInterestAmount.Currency);
                    ret.Amount.DecValue = round.AmountRounded;
                }
            }
            return ret;
        }
        #endregion AccruedInterestProrataCalculation
        #region GrossAmountProrataCalculation
        /// <summary>
        /// Calcul du montant du GrossAmount sur (POC|POT) par prorata quantité (corrigée|transférée) / quantité initiale
        /// </summary>
        /// <param name="pPosRequestTransfer"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pDebtSecurityContainer"></param>
        /// <returns></returns>
        /// EG 20150907 [21317] New
        // EG 20180601 [23978] Correction Currency (Lecture de la devise du GrossAmount en lieu et place de l'actif)
        protected override IMoney GrossAmountProrataCalculation(DataDocumentContainer pDataDocument, decimal pProrata)
        {
            IProduct product = pDataDocument.CurrentProduct.Product;

            IMoney grossAmount = null;
            if (product is IDebtSecurityTransaction)
            {
                IDebtSecurityTransaction dst = product as IDebtSecurityTransaction;
                grossAmount = product.ProductBase.CreateMoney(dst.GrossAmount.PaymentAmount.Amount.DecValue * pProrata, dst.GrossAmount.PaymentCurrency);
            }
            else if (product is IEquitySecurityTransaction)
            {
                IEquitySecurityTransaction est = product as IEquitySecurityTransaction;
                grossAmount = product.ProductBase.CreateMoney(est.GrossAmount.PaymentAmount.Amount.DecValue * pProrata, est.GrossAmount.PaymentCurrency);
            }

            if (null != grossAmount)
            {
                EFS_Cash round = new EFS_Cash(CS, grossAmount.Amount.DecValue, grossAmount.Currency);
                grossAmount.Amount.DecValue = round.AmountRounded;
            }
            
            return grossAmount;
        }
        #endregion GrossAmountProrataCalculation
        #region PrincipalAmountProrataCalculation
        /// <summary>
        /// Calcul du montant du principal amount sur (POC|POT) par prorata quantité (corrigée|transférée) / quantité initiale
        /// </summary>
        /// <param name="pDataDocument"></param>
        /// <param name="pProrata"></param>
        /// <returns></returns>
        /// FI 20151228 [21660] Add method
        // EG 20170127 Qty Long To Decimal
        protected IMoney PrincipalAmountProrataCalculation(DataDocumentContainer pDataDocument, decimal pInitialQty, decimal pProrata)
        {
            IMoney ret = null;
            if (pDataDocument.CurrentProduct.Product is IDebtSecurityTransaction dst && dst.Price.CleanPriceSpecified)
            {
                DebtSecurityTransactionContainer dstContainer = new DebtSecurityTransactionContainer(dst, pDataDocument);

                //Recherche du nominal du titre
                DebtSecurityContainer debtSecurityContainer = new DebtSecurityContainer(dstContainer.DebtSecurityTransaction.DebtSecurity);
                IMoney securityNominal = debtSecurityContainer.GetNominal(dstContainer.ProductBase);

                //Sauvegarde de la qté déjà présente dans le datadocument qui est la quantité assciée à l'action 
                IOrderQuantity savOrderQuantity = (IOrderQuantity)ReflectionTools.Clone(dstContainer.DebtSecurityTransaction.Quantity, ReflectionTools.CloneStyle.CloneField);

                //Mise à jour de la  qté présente dans le datadocument
                IOrderQuantity orderQuantity = dstContainer.DebtSecurityTransaction.Quantity;
                orderQuantity.NumberOfUnitsSpecified = true;
                // EG 20170127 Qty Long To Decimal
                orderQuantity.NumberOfUnits = new EFS_Decimal(pInitialQty);
                orderQuantity.NotionalAmount = dstContainer.ProductBase.CreateMoney(orderQuantity.NumberOfUnits.DecValue * securityNominal.Amount.DecValue, securityNominal.Currency);

                ret = dstContainer.CalcPrincipalAmount();
                ret.Amount.DecValue = ret.Amount.DecValue * pProrata;

                // Application règles d'arrondi spécifiées sur devise
                EFS_Cash round = new EFS_Cash(CS, ret.Amount.DecValue, ret.Currency);
                ret.Amount.DecValue = round.AmountRounded;

                //Restitution de la valeur sauvegardée
                dstContainer.DebtSecurityTransaction.Quantity = savOrderQuantity;
            }
            return ret;
        }
        #endregion

        #region InsertNominalQuantityEvent
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Add PoskeepingData
        protected override Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            return InsertNominalQuantityEvent(pDbTransaction, PosKeepingData, pIdT, ref pIdE, pIdE_Event, pDtBusiness, pIsDealerBuyer, pQty, pRemainQty, pIdPADET);
        }
        // EG 20190613 [24683] Add PoskeepingData
        // EG 20190730 Upd Le custodian remplace l'emetteur
        protected override Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            #region EVENT NOM/QTY
            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;
            string eventType = EventTypeFunc.Nominal;

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


            Cst.ErrLevel codeReturn;
            //if (pPosKeepingData.product.IsDebtSecurityTransaction && EventCodeFunc.IsTermination(eventCode))
            //{
            //    // Issuer est le Payer sur le TER/NOM
            //    idA_Payer = pPosKeepingData.IdA_Issuer;
            //    // EG 20150907 Set null
            //    idB_Payer = null;
            //    if (0 < pPosKeepingData.IdB_Issuer)
            //        idB_Payer = pPosKeepingData.IdB_Issuer;
            //}

            if (false == pPosKeepingData.Product.IsEquitySecurityTransaction)
            {
                Nullable<decimal> nominalValue = pPosKeepingData.NominalValue(pQty);
                codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                    eventCode, eventType, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness,
                    nominalValue, pPosKeepingData.Asset.nominalCurrency, UnitTypeEnum.Currency.ToString(), null, null);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
                pIdE++;
            }
            eventType = EventTypeFunc.Quantity;
            codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                eventCode, eventType, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness, pQty, null, UnitTypeEnum.Qty.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            #endregion EVENT NOM/QTY

            return codeReturn;

        }
        #endregion InsertNominalQuantityEvent
        #region InsertRedemptionAmountEvent
        /// <summary>
        /// Insertion de lévénement RAM matérialisant le remboursement à l'échéance par application du pourcentage spécifié sur le titre.
        /// soit QTY en position * NOMINAL AU PAIR * POURCENTAGE DE REMBOURSEMENT
        /// </summary>
        // EG 20190926 [Maturity redemption] New
        protected Cst.ErrLevel InsertRedemptionAmountEvent(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;

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

            Nullable<decimal> nominalValue = pPosKeepingData.NominalValue(pQty);
            if (nominalValue.HasValue)
                nominalValue *= AssetBond.RedemptionPricePercentage;

            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
    eventCode, EventTypeFunc.RedemptionAmount, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness,
    nominalValue, pPosKeepingData.Asset.nominalCurrency, UnitTypeEnum.Currency.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, pDtBusiness, true);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
            pIdE++;
            return codeReturn;
        }
        #endregion InsertRedemptionAmountEvent
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
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190730 Upd (parameter StatusBusiness)
        public override void SetPaymentFeesForEvent(int pIdT, decimal pQty, IPayment[] pPaymentFees, IPosRequestDetail pDetail)
        {
            if (null != m_RptSideProductContainer) 
            {
                if (m_TradeLibrary.Product.ProductBase.IsESE)
                {
                    EquitySecurityTransactionContainer estContainer = m_RptSideProductContainer as EquitySecurityTransactionContainer;
                    estContainer.TradeCaptureReport.LastQty = new EFS_Decimal(pQty);
                    estContainer.Efs_EquitySecurityTransaction = new EFS_EquitySecurityTransaction(CS, estContainer.EquitySecurityTransaction, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC);
                }
                else if (m_TradeLibrary.Product.ProductBase.IsDebtSecurityTransaction)
                {
                    DebtSecurityTransactionContainer dstContainer = m_RptSideProductContainer as DebtSecurityTransactionContainer;
                    dstContainer.DebtSecurityTransaction.Efs_DebtSecurityTransactionAmounts = new EFS_DebtSecurityTransactionAmounts(CS, dstContainer.DebtSecurityTransaction, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC);
                    dstContainer.DebtSecurityTransaction.Efs_DebtSecurityTransactionStream = new EFS_DebtSecurityTransactionStream(CS, dstContainer.DebtSecurityTransaction, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC);
                    dstContainer.SetOrderQuantityForAction(pQty);
                }
                // Restitution des frais initiaux (OPP)
                if (ArrFunc.IsFilled(pPaymentFees))
                {
                    EventQuery.InitPaymentForEvent(CS, pPaymentFees, m_TradeLibrary.DataDocument, out int nbEvent);
                    pDetail.NbAdditionalEvents += nbEvent;
                }
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
        // EG 20181205 [24360] Refactoring
        // EG 20190926 [Maturity redemption] Upd (Nouvelles colonnes ISSUEPRICEPCT et REDEMPTIONPRICEPCT)
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

                if (null != pMapDr["IDA_ISSUER"])
                    asset.IdA_Issuer = Convert.IsDBNull(pMapDr["IDA_ISSUER"].Value) ? 0 : Convert.ToInt32(pMapDr["IDA_ISSUER"].Value);

                if (null != pMapDr["ISSUEPRICEPCT"])
                    asset.IssuePricePercentage = Convert.IsDBNull(pMapDr["ISSUEPRICEPCT"].Value) ? 1 : Convert.ToDecimal(pMapDr["ISSUEPRICEPCT"].Value);

                if (null != pMapDr["REDEMPTIONPRICEPCT"])
                    asset.RedemptionPricePercentage = Convert.IsDBNull(pMapDr["REDEMPTIONPRICEPCT"].Value) ? 1 : Convert.ToDecimal(pMapDr["REDEMPTIONPRICEPCT"].Value);
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
        // EG 20170127 Qty Long To Decimal
        protected override void SetTradeMerge(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocumentContainer, TradeMergeRule pTmr,
            List<TradeCandidate> pLstTradeMerge, TradeCandidate pSourceTradeBase)
        {
            // EG 20170127 Qty Long To Decimal
            decimal totalQty = pLstTradeMerge.Sum(trade => trade.Qty);

            if (pDataDocumentContainer.CurrentProduct.IsEquitySecurityTransaction)
            {
                #region EquitySecurityTransaction
                IEquitySecurityTransaction _mergeTrade = (IEquitySecurityTransaction)pDataDocumentContainer.CurrentProduct.Product;
                EquitySecurityTransactionContainer estContainer = new EquitySecurityTransactionContainer(CS, pDbTransaction, _mergeTrade, pDataDocumentContainer);
                IFixTrdCapRptSideGrp _exTradeRptSide = (IFixTrdCapRptSideGrp)_mergeTrade.TradeCaptureReport.TrdCapRptSideGrp[0];

                if (_mergeTrade.TradeCaptureReport.TrdTypeSpecified)
                {
                    int _trdTypeValue = Convert.ToInt32(ReflectionTools.ConvertEnumToString(_mergeTrade.TradeCaptureReport.TrdType));
                    _mergeTrade.TradeCaptureReport.SecondaryTrdTypeSpecified = (_trdTypeValue < 1000);
                    if (_mergeTrade.TradeCaptureReport.SecondaryTrdTypeSpecified)
                        _mergeTrade.TradeCaptureReport.SecondaryTrdType = (SecondaryTrdTypeEnum)((int)_mergeTrade.TradeCaptureReport.TrdType);
                }
                _mergeTrade.TradeCaptureReport.TrdTypeSpecified = true;
                _mergeTrade.TradeCaptureReport.TrdType = TrdTypeEnum.MergedTrade;
                _mergeTrade.TradeCaptureReport.LastQtySpecified = true;
                _mergeTrade.TradeCaptureReport.LastQty = new EFS_Decimal
                {
                    // Quantité = Somme des quantité des trade sources
                    DecValue = totalQty
                };
                _mergeTrade.TradeCaptureReport.LastPxSpecified = true;
                if (pTmr.Context.PriceValue.HasValue)
                    // Prix identique (contexte = IDENTICAL)
                    _mergeTrade.TradeCaptureReport.LastPx.DecValue = pSourceTradeBase.Price;
                else
                {
                    // Prix moyen pondéré
                    decimal _lastPx = pLstTradeMerge.Sum(trade => trade.Price * trade.Qty) / _mergeTrade.TradeCaptureReport.LastQty.DecValue;

                    Cst.RoundingDirectionSQL? direction = Cst.RoundingDirectionSQL.N;
                    // On prend le plus grand nombre de décimales des prix des trades sources
                    Nullable<int> precision = pLstTradeMerge.Max(trade => trade.RemainderPriceLength);
                    EFS_Round _round = new EFS_Round(direction.Value, precision.Value, _lastPx);
                    _mergeTrade.TradeCaptureReport.LastPx.DecValue = _round.AmountRounded;
                }

                // Recalcul du GrossAmount sur la base de la somme des QTY des trades sources et du prix (identique|pondéré)
                estContainer.SetAssetEquity(CS);
                estContainer.GrossAmount.PaymentAmount.Amount.DecValue = estContainer.CalcGrossAmount(CS, pDbTransaction).Amount.DecValue;
                #endregion EquitySecurityTransaction
            }
            else if (pDataDocumentContainer.CurrentProduct.IsDebtSecurityTransaction)
            {
                #region DebtSecurityTransaction
                IDebtSecurityTransaction _mergeTrade = (IDebtSecurityTransaction)pDataDocumentContainer.CurrentProduct.Product;
                DebtSecurityTransactionContainer debtSecurityTransaction = new DebtSecurityTransactionContainer(_mergeTrade, pDataDocumentContainer);
                DebtSecurityContainer debtSecurity = new DebtSecurityContainer(_mergeTrade.DebtSecurity);
                // Qty
                if (_mergeTrade.Quantity.NumberOfUnitsSpecified)
                {
                    // EG 20170127 Qty Long To Decimal
                    _mergeTrade.Quantity.NumberOfUnits = new EFS_Decimal
                    {
                        DecValue = totalQty
                    };
                }
                // Nominal
                _mergeTrade.Quantity.NotionalAmount.Amount.DecValue = debtSecurity.GetNominal(debtSecurityTransaction.ProductBase).Amount.DecValue * totalQty;
                // Price
                if (pTmr.Context.PriceValue.HasValue)
                {
                    // Prix identique (contexte = IDENTICAL) TO DOOOOOOOOOOOOOOOOOOOOOO
                    if (_mergeTrade.Price.CleanPriceSpecified)
                        _mergeTrade.Price.CleanPrice.DecValue = pSourceTradeBase.Price;
                    else if (_mergeTrade.Price.DirtyPriceSpecified)
                        _mergeTrade.Price.DirtyPrice.DecValue = pSourceTradeBase.Price;
                }
                else
                {
                    // Prix moyen pondéré (contexte = IDENTICAL) TO DOOOOOOOOOOOOOOOOOOOOOO
                    decimal _price = pLstTradeMerge.Sum(trade => trade.Price * trade.Qty) / totalQty;

                    Nullable<Cst.RoundingDirectionSQL> direction = Cst.RoundingDirectionSQL.N;
                    // On prend le plus grand nombre de décimales des prix des trades sources
                    Nullable<int> precision = pLstTradeMerge.Max(trade => trade.RemainderPriceLength);
                    EFS_Round _round = new EFS_Round(direction.Value, precision.Value, _price);
                    if (_mergeTrade.Price.CleanPriceSpecified)
                        _mergeTrade.Price.CleanPrice.DecValue = _round.AmountRounded;
                    else if (_mergeTrade.Price.DirtyPriceSpecified)
                        _mergeTrade.Price.DirtyPrice.DecValue = _round.AmountRounded;
                }

                // Recalcul du GrossAmount sur la base de la somme des QTY des trades sources et du prix (identique|pondéré)
                debtSecurityTransaction.SetAssetDebtSecurity(CS, null);
                // AccruedInterest
                if (_mergeTrade.Price.AccruedInterestAmountSpecified)
                    _mergeTrade.Price.AccruedInterestAmount.Amount.DecValue = pLstTradeMerge.Sum(trade => trade.AccruedInterest);
                // GrossAmount
                _mergeTrade.GrossAmount.PaymentAmount.Amount.DecValue = debtSecurityTransaction.CalcGrossAmount(CS).Amount.DecValue;
                #endregion DebtSecurityTransaction
            }
        }
        #endregion SetTradeMerge
        #region EOD_MaturityRedemptionDebtSecurityGen
        /// <summary>
        /// Traitement du remboursement des transaction sur titre à l'échéance (en date BUSINESS)
        /// CLEARBULK si possible
        /// OFFSETTING pour clôturer la position
        /// -> QTY et NOM (TER)
        /// -> RAM (pour REDEMPTION AMOUNT) avec isPayment  1
        /// </summary>
        // EG 20190926 [Maturity redemption] New
        protected Cst.ErrLevel EOD_MaturityRedemptionDebtSecurityGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;

            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5028), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.MaturityRedemptionOffsettingDebtSecurity,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_MaturityRedemptionOffsettingDebtSecurityGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    IPosRequest posRequestMOD = null;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

                        try
                        {
                            // Insertion POSREQUEST (MOD)    
                            posRequestMOD = AddPosRequestMaturityRedemptionOffsetting(Cst.PosRequestTypeEnum.MaturityRedemptionOffsettingDebtSecurity, dr, out int newIdPR_MOD, posRequestGroupLevel.IdPR);
                            if (null != posRequestMOD)
                            {
                                // EG 20170206 [22787] RequestOnContractCanBeExecuted instead of RequestRODCanBeExecuted
                                if (posRequestMOD.PosKeepingKeySpecified &&
                                    RequestCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestMOD.IdEM, posRequestMOD.PosKeepingKey) &&
                                    RequestOnDebtSecurityCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestMOD.IdEM, posRequestMOD.PosKeepingKey))
                                {
                                    #region Compensation manuelle avant remboursement
                                    m_PosRequest = PosKeepingTools.SetPosRequestClearing(m_Product, Cst.PosRequestTypeEnum.ClearingBulk, SettlSessIDEnum.EndOfDay, dr);
                                    if (null != m_PosRequest)
                                    {
                                        SQLUP.GetId(out newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                                        InitStatusPosRequest(m_PosRequest, newIdPR, newIdPR_MOD, ProcessStateTools.StatusProgressEnum, m_LstSubPosRequest);

                                        m_PosRequest.GroupProductSpecified = (posRequestGroupLevel.GroupProductSpecified);
                                        if (m_PosRequest.GroupProductSpecified)
                                            m_PosRequest.GroupProduct = posRequestGroupLevel.GroupProduct;

                                        codeReturn = PosKeepingTools.InsertPosRequest(CS, newIdPR, m_PosRequest, m_PKGenProcess.Session);
                                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                                        {
                                            codeReturn = ClearingBLKGen(false);
                                            m_PosRequest.Status = (ProcessStateTools.IsCodeReturnSuccess(codeReturn) || ProcessStateTools.IsCodeReturnNothingToDo(codeReturn) ?
                                                ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum);
                                            m_PosRequest.StatusSpecified = true;
                                            
                                            //PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, m_PosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, newIdPR_MOD);
                                            PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, m_PosRequest, m_PKGenProcess.Session.IdA, IdProcess, newIdPR_MOD);
                                        }
                                    }
                                    m_PosRequest = posRequestMOD;
                                    #endregion Compensation manuelle avant liquidation

                                    // Remboursement du future à l'échance
                                    codeReturn = MaturityRedemptionOffsettingDebtSecurityGen(posRequestMOD);
                                }
                                else
                                {
                                    codeReturn = Cst.ErrLevel.DATAREJECTED;
                                }
                            }
                            else
                            {
                                codeReturn = Cst.ErrLevel.DATAREJECTED;
                            }
                        }
                        catch (Exception ex)
                        {
                            codeReturn = Cst.ErrLevel.FAILURE;
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            // FI 20200623 [XXXXX] AddCriticalException
                            m_PKGenProcess.ProcessState.AddCriticalException(ex);

                            
                            Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                            isException = true;
                        }
                        finally
                        {
                            //Update POSREQUEST (ROD)
                            UpdatePosRequest(codeReturn, posRequestMOD, posRequestGroupLevel.IdPR);
                        }
                    }
                    // Update POSREQUEST GROUP
                    if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                    {
                        
                        //PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, posRequestGroupLevel, m_PKGenProcess.appInstance, LogHeader.IdProcess);
                        PosKeepingTools.UpdateIRQPosRequestGroupLevel(CS, posRequestGroupLevel, m_PKGenProcess.Session, IdProcess);
                    }
                    else
                    {
                        UpdatePosRequestGroupLevel(posRequestGroupLevel);
                    }
                }
            }
            

            
            Logger.Write();

            return (isException ? Cst.ErrLevel.FAILURE : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_RepaymentDebtSecurityGen
        #endregion Methods

        #region AddPosRequestMaturityRedemptionOffsetting
        /// <summary>
        /// Insertion d'un POSREQUEST d'une ligne de regroupement
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► REQUESTTYPE = MOD (concerne les remboursements sur DebtSecurity</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pDr">DataRow (Caractéristiques de la clé de position)</param>
        /// <param name="pNewIdPR">Id de la nouvelle ligne à insérer</param>
        /// <param name="pIdPR_Parent">Id parent</param>
        /// <returns></returns>
        // EG 20190926 [Maturity redemption] New
        private IPosRequest AddPosRequestMaturityRedemptionOffsetting(Cst.PosRequestTypeEnum pRequestType, DataRow pDr, out int pNewIdPR, int pIdPR_Parent)
        {
            int newIdPR = 0;
            IPosKeepingKey posKeepingKey = PosKeepingTools.SetPosKeepingKey(m_Product, pDr);
            Cst.ErrLevel codeReturn = InitPosKeepingData(null, posKeepingKey, m_MarketPosRequest.IdEM, Cst.PosRequestAssetQuoteEnum.Asset, false);
            IPosRequest posRequest = GetSubRequest(pRequestType, Convert.ToInt32(m_MarketPosRequest.IdEM), posKeepingKey);
            if (null == posRequest)
            {
                // Insertion POSREQUEST (Maturité à l'échéance : MOD)    
                posRequest = PosKeepingTools.SetPosRequestMaturityRedemptionOffsetting(m_Product, pRequestType, SettlSessIDEnum.EndOfDay, pDr);
                posRequest.IdTSpecified = false;
                posRequest.GroupProductSpecified = (m_MarketPosRequest.GroupProductSpecified);
                if (posRequest.GroupProductSpecified)
                    posRequest.GroupProduct = m_MarketPosRequest.GroupProduct;
                InitStatusPosRequest(posRequest, newIdPR, pIdPR_Parent, ProcessStateTools.StatusProgressEnum, m_LstSubPosRequest);

                
                //PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, pIdPR_Parent);
                PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, posRequest, m_PKGenProcess.Session, IdProcess, pIdPR_Parent);
                posRequest.IdPR = newIdPR;
                if (Cst.ErrLevel.SUCCESS != codeReturn)
                    posRequest.SetStatus(codeReturn);
            }
            pNewIdPR = newIdPR;
            return posRequest;
        }
        #endregion AddPosRequestMaturityRedemptionOffsetting
        #region MaturityRedemptionOffsettingDebtSecurityGen
        /// <summary>
        /// REMBOURSEMENT AUTOMATIQUE DES TITRES ARRIVANT A ECHEANCE (POUR UNE CLE DE POSITION)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        protected Cst.ErrLevel MaturityRedemptionOffsettingDebtSecurityGen(IPosRequest pPosRequestMOD)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                QueryParameters qryParameters = GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity(pPosRequestMOD.DtBusiness, pPosRequestMOD.PosKeepingKey);
                DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);

                if (null != ds && ArrFunc.IsFilled(ds.Tables[0].Rows))
                {
                    List<EOD_Trade> lstTrades = new List<EOD_Trade>();
                    int nbRow = ds.Tables[0].Rows.Count;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        EOD_Trade trade = new EOD_Trade();
                        #region Initialisation Variables
                        trade.IdT = Convert.ToInt32(dr["IDT"]);
                        // EG 20170127 Qty Long To Decimal
                        trade.Qty = Convert.ToDecimal(dr["QTY"]);
                        trade.IdE_Event = Convert.ToInt32(dr["IDE_EVENT"]);
                        trade.IsDealerBuyer = IsTradeBuyer(dr["SIDE"].ToString());
                        trade.Price = Convert.ToDecimal(dr["PRICE"]);
                        trade.Currency = Convert.ToString(dr["IDC_PARVALUE"]); //PL 20170413 Add for DVA
                        if (trade.IsDealerBuyer)
                            trade.IdT_Buy = trade.IdT;
                        else
                            trade.IdT_Sell = trade.IdT;
                        #endregion Initialisation Variables

                        #region Calcul des éventuels frais de remboursement
                        //trade.fees = CalcMaturityOffsettingFuture(CS, trade.idT, trade.qty, m_MasterPosRequest.dtBusiness, m_PKGenProcess.appInstance);
                        #endregion

                        trade.NbEvent = pPosRequestMOD.NbTokenIdE;

                        if (ArrFunc.IsFilled(trade.Fees))
                        {
                            DeserializeTrade(trade.IdT);

                            EventQuery.InitPaymentForEvent(CS, trade.Fees, m_TradeLibrary.DataDocument, out int nbFeesEvent);
                            trade.NbEvent += nbFeesEvent;
                        }

                        lstTrades.Add(trade);
                    }

                    if (0 < lstTrades.Count)
                    {
                        dbTransaction = DataHelper.BeginTran(CS);

                        #region GetId of POSACTION/POSACTIONDET
                        SQLUP.GetId(out int newIdPA, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);

                        int newIdPADET = newIdPA;

                        #endregion GetId of POSACTION/POSACTIONDET
                        #region Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS
                        #endregion GetId of POSACTION/POSACTIONDET

                        #region Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS
                        SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, lstTrades.Sum(trade => trade.NbEvent));

                        #region Insertion dans POSACTION
                        InsertPosAction(dbTransaction, newIdPA, DtBusiness, pPosRequestMOD.IdPR);
                        #endregion Insertion dans POSACTION

                        lstTrades.ForEach(trade =>
                        {
                            if (false == isException)
                            {
                                #region POSACTIONDET
                                // EG 20141205 null for PositionEffect
                                InsertPosActionDet(dbTransaction, newIdPA, newIdPADET, trade.IdT_Buy, trade.IdT_Sell, trade.IdT, trade.Qty, null);
                                #endregion POSACTIONDET

                                #region EVENT MOD|TOT (Maturity OffSetting)

                                int idE_Parent = trade.IdE_Event;
                                int idE_RepaymentOffSetting = newIdE;

                                //Event MOD|TOT
                                m_EventQuery.InsertEvent(dbTransaction, trade.IdT, idE_RepaymentOffSetting, idE_Parent, null, 1, 1, null, null, null, null,
                                        EventCodeFunc.MaturityRedemptionOffsettingDebtSecurity, EventTypeFunc.Total,
                                        DtBusiness, DtBusiness, DtBusiness, DtBusiness, trade.Qty, null, UnitTypeEnum.Qty.ToString(), null, null);

                                //EventClass
                                m_EventQuery.InsertEventClass(dbTransaction, idE_RepaymentOffSetting, EventClassFunc.CashSettlement, DtBusiness, false);

                                //PosActionDet
                                EventQuery.InsertEventPosActionDet(dbTransaction, newIdPADET, idE_RepaymentOffSetting);

                                InsertEventDet(dbTransaction, PosKeepingData, idE_RepaymentOffSetting, trade.Qty, AssetBond.contractMultiplier, null, trade.Price, null);

                                newIdE++;

                                #endregion EVENT MOD|TOT (Maturity OffSetting)

                                #region EVENT NOM/QTY (Nominal/Quantité)
                                codeReturn = InsertNominalQuantityEvent(dbTransaction, trade.IdT, ref newIdE, idE_RepaymentOffSetting, DtBusiness, trade.IsDealerBuyer, trade.Qty, 0, newIdPADET);
                                newIdE++;
                                #endregion EVENT NOM/QTY (Nominal/Quantité)

                                #region EVENT RAM (Redemption amount)
                                codeReturn = InsertRedemptionAmountEvent(dbTransaction,PosKeepingData, trade.IdT, ref newIdE, idE_RepaymentOffSetting, DtBusiness, trade.IsDealerBuyer, trade.Qty, 0,newIdPADET);
                                #endregion EVENT RAM (Redemption amount)

                                #region Insert des éventuels frais de remboursement

                                if (ArrFunc.IsFilled(trade.Fees))
                                {
                                    foreach (IPayment payment in trade.Fees)
                                    {
                                        newIdE++;
                                        int savIdE = newIdE;
                                        codeReturn = m_EventQuery.InsertPaymentEvents(dbTransaction, m_TradeLibrary.DataDocument, trade.IdT, payment, DtBusiness, 1, 1, ref newIdE, idE_RepaymentOffSetting);
                                        // EG 20120511 Add EVENTPOSACTIONDET pour toutes les lignes de FRAIS
                                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                                        {
                                            for (int i = savIdE; i <= newIdE; i++)
                                            {
                                                codeReturn = EventQuery.InsertEventPosActionDet(dbTransaction, newIdPADET, i);
                                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                                    break;
                                            }
                                        }
                                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                                            break;
                                    }
                                }
                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                {
                                    isException = true;
                                }

                                newIdE++;

                                #endregion Insert des éventuels frais de remboursement


                                //newIdPADET++;
                                SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                            }
                        });
                        #endregion Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS

                        if ((null != dbTransaction) && (false == isException))
                            DataHelper.CommitTran(dbTransaction);
                    }
                }
                return codeReturn;
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
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }
        }
        #endregion MaturityRedemptionOffsettingDebtSecurityGen
        #region RequestOnDebtSecurityCanBeExecuted
        /// <summary>
        /// Retourne si une demande peut-être exécutée par lecture des STATUT des
        /// traitements EOD matchant avec la liste passé en paramètre
        /// avec l'entité/marché ainsi que pour toutes les clé de position 
        /// constituées via l'IDASSET (DEBTSECURITY) passé en paramètre
        /// (stockés dans m_LstSubPosRequest)
        /// </summary>
        /// <param name="pListRequestType">RequestType (List)</param>
        /// <param name="pIdEM">Id Entité/Marché</param>
        /// <param name="pPosKeepingKey">Clé de position</param>
        /// <returns>boolean</returns>
        private bool RequestOnDebtSecurityCanBeExecuted(Cst.PosRequestTypeEnum pListRequestType, int pIdEM, IPosKeepingKey pPosKeepingKey)
        {
            bool ret = true;
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pPosKeepingKey.IdAsset);

            string sqlSelect = @"select ast.IDT as IDASSET, ast.IDI
            from dbo.VW_TRADEDEBTSEC ast
            where (ast.IDT = @IDASSET)" + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    IPosKeepingKey posKeepingKey = m_Product.CreatePosKeepingKey();
                    posKeepingKey.IdI = Convert.ToInt32(dr["IDI"]);
                    posKeepingKey.IdAsset = Convert.ToInt32(dr["IDASSET"]);
                    posKeepingKey.IdA_Dealer = pPosKeepingKey.IdA_Dealer;
                    posKeepingKey.IdB_Dealer = pPosKeepingKey.IdB_Dealer;
                    posKeepingKey.IdA_Clearer = pPosKeepingKey.IdA_Clearer;
                    posKeepingKey.IdB_Clearer = pPosKeepingKey.IdB_Clearer;
                    posKeepingKey.IdA_EntityDealer = pPosKeepingKey.IdA_EntityDealer;
                    ret = RequestCanBeExecuted(pListRequestType, pIdEM, posKeepingKey);
                    if (false == ret)
                        break;
                }
            }
            return ret;
        }
        #endregion RequestOnDebtSecurityCanBeExecuted
    }
}

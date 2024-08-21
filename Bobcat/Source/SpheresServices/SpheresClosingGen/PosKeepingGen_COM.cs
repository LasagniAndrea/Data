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
    public partial class PosKeepingGen_COM : PosKeepingGenProcessBase
    {
        #region Accessors

        #region StoreProcedure_RemoveEOD
        protected override string StoreProcedure_RemoveEOD
        {
            get { return "UP_REMOVE_POSACTION_COMD"; }
        }
        #endregion StoreProcedure_RemoveEOD
        #region VW_TRADE_POS
        protected override string VW_TRADE_POS
        {
            get { return Cst.OTCml_TBL.VW_TRADE_POSCOM.ToString(); }
        }
        #endregion VW_TRADE_POS
        #region VW_TRADE_FUNGIBLE_LIGHT
        protected override string VW_TRADE_FUNGIBLE_LIGHT
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_LIGHT_COM.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE_LIGHT
        #region VW_TRADE_FUNGIBLE
        protected override string VW_TRADE_FUNGIBLE
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_COM.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE
        #region RESTRICT_EVENT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENT_POS
        {
            get { return "EVENTCODE = 'PHL'"; }
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
        #region RESTRICT_PARTYROLE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_PARTYROLE_POS
        {
            get
            {
                return ReflectionTools.ConvertEnumToString<PartyRoleEnum>(PartyRoleEnum.BuyerSellerReceiverDeliverer); }
        }
        #endregion RESTRICT_PARTYROLE_POS
        #region RESTRICT_ISCUSTODIAN
        // EG 20181010 PERF EOD New
        protected override string RESTRICT_ISCUSTODIAN
        {
            get { return "0"; }
        }
        #endregion RESTRICT_ISCUSTODIAN
        #region RESTRICT_EMCUSTODIAN_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EMCUSTODIAN_POS
        {
            get { return "em.IDA_CUSTODIAN is null"; }
        }
        #endregion RESTRICT_EMCUSTODIAN_POS
        #region GPRODUCT_VALUE
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        public override string GPRODUCT_VALUE
        {
            get { return ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.Commodity); }
        }
        #endregion GPRODUCT_VALUE


        #endregion Accesors
        #region Constructors
        public PosKeepingGen_COM(PosKeepingGenProcess pPosKeepingGenProcess) : base(pPosKeepingGenProcess)
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
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
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
                // STEP : CALCUL DES FRAIS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_FeesGen();
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
                    UpdatePosRequestGroupLevel(m_MarketPosRequest, ProcessStateTools.StatusSuccessEnum);
            }
        }
        #endregion EOD_MarketGen

        #region CLOSINGDAY_EODControl
        /// <summary>
        /// Controle du traitement EOD (exclusivement lors de la clôture de journée)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pIdA_Entity"></param>
        /// <param name="pIdA_Css"></param>
        /// <param name="pIdA_Custodian"></param>
        /// <param name="pIdEM"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pAppInstance"></param>
        /// <param name="pIdProcess"></param>
        /// <param name="pIdPR_Parent"></param>
        /// <param name="pSubPosRequest"></param>
        /// FI 20160819 [22364] Modify
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180307 [23769] Refactoring
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
                else if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
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
            m_TradeLibrary = new EFS_TradeLibrary(CS, pDbTransaction, pIdT);
            if (m_TradeLibrary.Product.ProductBase.IsCommoditySpot)
                m_RptSideProductContainer = new CommoditySpotContainer(CS, pDbTransaction, (ICommoditySpot)m_TradeLibrary.Product, m_TradeLibrary.DataDocument);
        }
        #endregion DeserializeTrade

        #region InsertEventDet
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Add PosKeepingData parameter
        // EG 20190730 Add TypePrice parameter
        protected override Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdE, decimal pQty,
            decimal pContractMultiplier, Nullable<AssetMeasureEnum> pTypePrice, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet


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
        protected override Cst.ErrLevel InsertPosKeepingEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRow2,
            DataRow pRowPosActionDet, int pIdPADET, ref int pIdE, bool pIsClosed)
        {
            return Cst.ErrLevel.SUCCESS;
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
        protected override Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {
            return Cst.ErrLevel.SUCCESS;
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
            if (null != m_RptSideProductContainer) 
            {
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
        // EG 20180221 [23769] Public pour gestion Asynchrone)
        // EG 20181205 [24360] Refactoring
        public override PosKeepingAsset SetPosKeepingAsset(IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, MapDataReaderRow pMapDr)
        {
            PosKeepingAsset asset = m_Product.CreatePosKeepingAsset(pUnderlyingAsset);
            if (null != asset)
            {
                int idAsset = Convert.ToInt32(pMapDr["IDASSET"].Value);
                string identifier = pMapDr["ASSET_IDENTIFIER"].Value.ToString();

                asset.idAsset = idAsset;
                asset.identifier = identifier;
                asset.contractMultiplier = 1;
                if (null != pMapDr["IDC"])
                    asset.currency = pMapDr["IDC"].Value.ToString();

                if (null != pMapDr["IDBC"])
                    asset.idBC = pMapDr["IDBC"].Value.ToString();

            }
            return asset;
        }
        #endregion SetPosKeepingAsset

        #region SetTradeMerge
        // EG 20150723 TO DO 
        protected override void SetTradeMerge(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocumentContainer, TradeMergeRule pTmr,
            List<TradeCandidate> pLstTradeMerge, TradeCandidate pSourceTradeBase)
        {
            // EG 20170127 Qty Long To Decimal
            decimal totalQty = pLstTradeMerge.Sum(trade => trade.Qty);

            if (pDataDocumentContainer.CurrentProduct.IsCommoditySpot)
            {
                ICommoditySpot _mergeTrade = (ICommoditySpot)pDataDocumentContainer.CurrentProduct.Product;
                CommoditySpotContainer commoditySpotContainer = new CommoditySpotContainer(CS, pDbTransaction, _mergeTrade, pDataDocumentContainer);
                IFixedPrice price = commoditySpotContainer.FixedLeg.FixedPrice;
            }
        }
        #endregion SetTradeMerge

        #endregion Methods

    }
}

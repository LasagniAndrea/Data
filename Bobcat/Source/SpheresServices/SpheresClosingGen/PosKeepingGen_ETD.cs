#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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
//
using SpheresClosingGen.Properties;
//
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    public partial class PosKeepingGen_ETD : PosKeepingGenProcessBase
    {
        #region Accessors

        #region ExchangeTradedDerivativeContainer
        protected ExchangeTradedDerivativeContainer ExchangeTradedDerivativeContainer
        {
            get { return m_RptSideProductContainer as ExchangeTradedDerivativeContainer; }
        }
        #endregion ExchangeTradedDerivativeContainer

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // OVERRIDE ACCESSORS (VIRTUAL SUR PosKeepingGenProcessBase)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region ClearingBusinessDate
        protected override DateTime ClearingBusinessDate
        {
            get { return m_RptSideProductContainer.ClearingBusinessDate; }
        }
        #endregion ClearingBusinessDate

        #region DtSettlement
        /// <summary>
        /// Obtient la date de règlement (DtBusiness + 1JO)
        /// </summary>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected override DateTime DtSettlement
        {
            get
            {
                return Tools.ApplyOffset(CS, m_Product, DtBusiness, 1, DayTypeEnum.ExchangeBusiness, Asset.idBC, null);
            }
        }
        #endregion DtSettlement

        #region IsFuturesStyleMarkToMarket
        // EG 20140711 (New - Override)
        protected override bool IsFuturesStyleMarkToMarket
        {
            get { return ExchangeTradedDerivativeContainer.IsFuturesStyleMarkToMarket; }
        }
        #endregion IsFuturesStyleMarkToMarket
        #region IsOption
        // EG 20140711 (New - Override)
        protected override bool IsOption
        {
            get { return ExchangeTradedDerivativeContainer.IsOption; }
        }
        #endregion IsOption
        #region IsPremiumStyle
        // EG 20140711 (New - Override)
        protected override bool IsPremiumStyle
        {
            get {return ExchangeTradedDerivativeContainer.IsPremiumStyle;}
        }
        #endregion IsPremiumStyle
        #region IsVariationMarginAuthorized
        // EG 20140711 (See PosKeepingGen_XXX - Override)
        protected override bool IsVariationMarginAuthorized
        {
            get 
            {
                return (Asset.valuationMethod == FuturesValuationMethodEnum.FuturesStyleMarkToMarket) &&
                        Asset.exerciseStyleSpecified && (Asset.category == CfiCodeCategoryEnum.Option) && (Asset.maturityDateSys == DtBusiness);
            }
        }
        #endregion IsVariationMarginAuthorized

        #region MaturityDate
        protected override DateTime MaturityDate
        {
            get { return Asset.maturityDate; }
        }
        #endregion MaturityDate
        #region MaturityDateSys
        protected override DateTime MaturityDateSys
        {
            get { return ExchangeTradedDerivativeContainer.MaturityDateSys; }
        }
        #endregion MaturityDateSys

        #region StoreProcedure_RemoveEOD
        // EG 20150708 [RemoveEOD] Rename procedure
        protected override string StoreProcedure_RemoveEOD
        {
            get { return "UP_REMOVE_POSACTION_ETD"; }
        }
        #endregion StoreProcedure_RemoveEOD
        #region VW_TRADE_POS
        protected override string VW_TRADE_POS
        {
            get { return Cst.OTCml_TBL.VW_TRADE_POSETD.ToString(); }
        }
        #endregion VW_TRADE_POS
        #region VW_TRADE_FUNGIBLE_LIGHT
        protected override string VW_TRADE_FUNGIBLE_LIGHT
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_LIGHT_ETD.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE_LIGHT
        #region VW_TRADE_FUNGIBLE
        protected override string VW_TRADE_FUNGIBLE
        {
            get { return Cst.OTCml_TBL.VW_TRADE_FUNGIBLE_ETD.ToString(); }
        }
        #endregion VW_TRADE_FUNGIBLE
        #region RESTRICT_EVENT_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENT_POS
        {
            get { return "EVENTTYPE in ('FUT','CAL','PUT')"; }
        }
        #endregion RESTRICT_EVENT_POS
        #region RESTRICT_EVENTCODE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENTCODE_POS
        {
            get { return "EVENTCODE = 'LPC'"; }
        }
        #endregion RESTRICT_EVENTCODE_POS
        #region RESTRICT_EVENTTYPE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_EVENTTYPE_POS
        {
            get { return "EVENTTYPE = 'AMT'"; }
        }
        #endregion RESTRICT_EVENTTYPE_POS
        #region RESTRICT_TRDTYPE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        // RD 20170327 [23003] Add restriction for Cascading(1001), Shifting (1002)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override string RESTRICT_TRDTYPE_POS
        {
            get { return "isnull(tr.TRDTYPE,'0') not in (" + Cst.TrdType_ExcludedValuesForFees_ETD + ")"; }
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
        #region RESTRICT_ASSET_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_ASSET_POS
        {
            get { return @"inner join dbo.ASSET_ETD ass on (ass.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)"; }
        }
        #endregion RESTRICT_ASSET_POS
        #region RESTRICT_COLMERGE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_COLMERGE_POS
        {
            get { return "da.IDDC"; }
        }
        #endregion RESTRICT_COLMERGE_POS
        #region RESTRICT_PARTYROLE_POS
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        protected override string RESTRICT_PARTYROLE_POS
        {
            get { return ReflectionTools.ConvertEnumToString<PartyRoleEnum>(PartyRoleEnum.BuyerSellerReceiverDeliverer); }
        }
        #endregion RESTRICT_PARTYROLE_POS
        #region GPRODUCT_VALUE
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        public override string GPRODUCT_VALUE
        {
            get
            {
                return ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.ExchangeTradedDerivative); 
            }
        }
        #endregion GPRODUCT_VALUE

        #region Asset
        protected PosKeepingAsset_ETD Asset
        {
            get { return (PosKeepingAsset_ETD)m_PosKeepingData.Asset; }
        }
        #endregion Asset

        #endregion Accessors

        #region Constructors
        public PosKeepingGen_ETD(PosKeepingGenProcess pPosKeepingGenProcess) : base(pPosKeepingGenProcess)
		{
		}
		#endregion Constructors

        #region Methods

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // EOD/CLOSINGDAY OVERRIDE METHODS (VIRTUAL SUR PosKeepingGenProcessBase)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region EOD_CascadingGen
        /// <summary>
        /// REALISE LE CASCADING AUTOMATIQUE DES POSITIONS ARRIVANT A ECHEANCE (EOD)
        /// <para>[Pour les contrats pour lesquels est défini un cascading]</para>
        ///<para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.FAILURE (si exception) ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// EG 20140130 [19586] IsolationLevel
        /// EG 20140326 [19775] 
        /// EG 20170221 [22879] 
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue 
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        protected override Cst.ErrLevel EOD_CascadingGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;

            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5023), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.Cascading, m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.Cascading, m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_CascadingGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

                        IPosRequest posRequestCAS = null;
                        try
                        {
                            // Insertion POSREQUEST (CAS)    
                            posRequestCAS = AddPosRequestMaturityOffsetting(Cst.PosRequestTypeEnum.Cascading, dr, out int newIdPR_CAS, posRequestGroupLevel.IdPR);
                            if (null != posRequestCAS)
                            {
                                /// EG 20131218 [19353/19355]
                                if (IsQuoteOk)
                                {
                                    if (posRequestCAS.PosKeepingKeySpecified &&
                                        RequestCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestCAS.IdEM, posRequestCAS.PosKeepingKey))
                                    {
                                        // Cascading de la position à l'échance
                                        // EG 20170206 [22879]
                                        codeReturn = CascadingOrShiftingGen((IPosRequestCascadingShifting)posRequestCAS, DtBusiness);
                                    }
                                    else
                                    {
                                        codeReturn = Cst.ErrLevel.DATAREJECTED;
                                    }
                                }
                                else
                                {
                                    codeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                                }
                            }
                            else
                            {
                                codeReturn = Cst.ErrLevel.DATAREJECTED;
                            }
                        }
                        catch (Exception ex)
                        {
                            /// EG 20140326 [19775] 
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
                            //Update POSREQUEST (CAS)
                            if (null != posRequestCAS)
                                UpdatePosRequest(codeReturn, posRequestCAS, posRequestGroupLevel.IdPR);
                        }
                    }

                    if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                        UpdateIRQPosRequestGroupLevel(posRequestGroupLevel);
                    else
                        UpdatePosRequestGroupLevel(posRequestGroupLevel);
                }
            }

            
            Logger.Write();

            /// EG 20140326 [19775] 
            return (isException ? Cst.ErrLevel.FAILURE : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_CascadingGen
        #region EOD_MarketGen
        /// <summary>
        /// Méthode principale de traitement de tenue de position de fin de journée par ENTITE/MARCHE
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Traitement séquentiel</para>
        ///<para>  1 ● Contrôles (Trades incomplets, sans événements)</para>
        ///<para>  2 ● Merge de trades</para>
        ///<para>  3 ● Mise à jour des clôture candidates (par clé de position)</para>
        ///<para>  4 ● Cascading de position</para>
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
        // EG 20180307 [23769] Gestion Asynchrone
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
                    ProcessStateTools.StatusProgressEnum, m_MasterPosRequest.IdPR, m_LstMarketSubPosRequest, pGroupProduct);

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
                // STEP 2 : MERGE (MERGE)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_MergeGen();
                }
                // STEP 3 : MISE A JOUR DES CLOTURES (UPDENTRY)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_UpdateEntryGen();
                }
                // STEP 4 : CASCADING DE POSITION
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CascadingGen();
                }
                // STEP 5 : DENOUEMENT MANUEL D'OPTIONS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_OptionGen();
                }
                // STEP 6 : DENOUEMENT AUTOMATIQUE D'OPTIONS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_AutomaticOptionGenThreading();
                }
                // STEP 7 : LIVRAISON SOUS-JACENT
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_UnderlyerDeliveryGenThreading();
                }
                // STEP 8 : CLOTURE AUTOMATIQUE FUTURE A ECHEANCE
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_MaturityOffsettingFutureGen();
                }
                // STEP 9 : COMPENSATION AUTOMATIQUE
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ClearingEndOfDay();
                }
                // STEP 10 : PHYSICAL PERIODIC DELIVERY
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_PhysicalPeriodicDeliveryGen();
                }
                // STEP 11 : CALCUL DES CASH-FLOWS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CashFlowsGen();
                }
                // STEP 12 : CALCUL DES FRAIS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_FeesGen();
                }
                // STEP 13 : CLOSING/REOPENING ACTIONS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ClosingReopeningActionsGen();
                }
                // STEP 14 : CORPORATE ACTIONS (OST)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CorporateActionsGen(pIdM);
                }
                // STEP 15 : CALCUL DES UTI
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
                        if (IsSubRequest_Status(Cst.PosRequestTypeEnum.EOD_CorporateActionGroupLevel, ProcessStateTools.StatusEnum.ERROR))
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

        #region EOD_MaturityOffsettingFutureGen
        /// <summary>
        /// CLOTURE AUTOMATIQUE DES FUTURES ARRIVANT A ECHEANCE (EOD)
        /// <para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.FAILURE (si exception) ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// EG 20140204 [19586] IsolationLevel
        /// EG 20140326 [19775] 
        /// EG 20160302 (21969] New
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue 
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20181231 [24413] Test CodeReturn and réinitialisation m_PosRequest après Clearing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190918 Refactoring PosRequestTypeEnum (Maturity Redemption DEBTSECURITY)
        protected Cst.ErrLevel EOD_MaturityOffsettingFutureGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;

            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG

            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5006), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.MaturityOffsettingFuture,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.MaturityOffsettingFuture,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_MaturityOffsettingFutureGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    IPosRequest posRequestMOF = null;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

                        try
                        {
                            // Insertion POSREQUEST (MOF)    
                            posRequestMOF = AddPosRequestMaturityOffsetting(Cst.PosRequestTypeEnum.MaturityOffsettingFuture, dr, out int newIdPR_MOF, posRequestGroupLevel.IdPR);
                            if (null != posRequestMOF)
                            {
                                // EG 201100708 TEST EXISTENCE COURS
                                // EG 20160302 [21969] CodeReturn
                                codeReturn = SetPosKeepingQuote(posRequestMOF);
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    if (IsQuoteOfficialCloseExist)
                                    {

                                        // EG 20170206 [22787] RequestOnContractCanBeExecuted instead of RequestMOFCanBeExecuted
                                        if (posRequestMOF.PosKeepingKeySpecified &&
                                            RequestCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestMOF.IdEM, posRequestMOF.PosKeepingKey) &&
                                            RequestOnContractCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestMOF.IdEM, posRequestMOF.PosKeepingKey))
                                        {
                                            #region Compensation manuelle avant liquidation
                                            m_PosRequest = PosKeepingTools.SetPosRequestClearing(m_Product, Cst.PosRequestTypeEnum.ClearingBulk, SettlSessIDEnum.EndOfDay, dr);
                                            if (null != m_PosRequest)
                                            {
                                                SQLUP.GetId(out newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                                                InitStatusPosRequest(m_PosRequest, newIdPR, newIdPR_MOF, ProcessStateTools.StatusProgressEnum, m_LstSubPosRequest);

                                                // EG 20170109 New currentGroupProduct
                                                m_PosRequest.GroupProductSpecified = (posRequestGroupLevel.GroupProductSpecified);
                                                if (m_PosRequest.GroupProductSpecified)
                                                    m_PosRequest.GroupProduct = posRequestGroupLevel.GroupProduct;

                                                codeReturn = PosKeepingTools.InsertPosRequest(CS, newIdPR, m_PosRequest, m_PKGenProcess.Session);
                                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                                {
                                                    codeReturn = ClearingBLKGen(false);
                                                    // EG [24143] Add Test Status NothingToDo
                                                    m_PosRequest.Status = (ProcessStateTools.IsCodeReturnSuccess(codeReturn) || ProcessStateTools.IsCodeReturnNothingToDo(codeReturn) ?
                                                        ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum);
                                                    m_PosRequest.StatusSpecified = true;
                                                    
                                                    //PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, m_PosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, newIdPR_MOF);
                                                    PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, m_PosRequest, m_PKGenProcess.Session.IdA, IdProcess, newIdPR_MOF);
                                                }
                                            }
                                            m_PosRequest = posRequestMOF;
                                            #endregion Compensation manuelle avant liquidation

                                            // Liquidation du future à l'échance
                                            codeReturn = MaturityOffsettingFutureGen(posRequestMOF);
                                        }
                                        else
                                        {
                                            codeReturn = Cst.ErrLevel.DATAREJECTED;
                                        }
                                    }
                                    else
                                    {
                                        codeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                                    }
                                }
                            }
                            else
                            {
                                codeReturn = Cst.ErrLevel.DATAREJECTED;
                            }
                        }
                        catch (Exception ex)
                        {
                            /// EG 20140326 [19775] 
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
                            //Update POSREQUEST (MOF)
                            UpdatePosRequest(codeReturn, posRequestMOF, posRequestGroupLevel.IdPR);
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
        #endregion PosKeep_EOD_MaturityOffsettingFutureGen
        #region EOD_PhysicalPeriodicDeliveryGen
        /// <summary>
        /// Gestion des periodes de livraison
        /// Production de nouveaux événements destinés à matérialiser :
        /// 1. la quantité d'énergie livrée, avec théoriquement les dates/heures de début et fin (sur EVENTDET).
        /// 2. le montant du règlement à opérer (relatif à la QTY d'énergie livrée) et ce sur la base du Final Settlement Price observé sur le Last Trading Day (LTD).
        /// <para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.FAILURE (si exception) ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20170206 [22787] (See PosKeepingGen_XXX - Override)
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        protected override Cst.ErrLevel EOD_PhysicalPeriodicDeliveryGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;

            m_PosRequest = RestoreMarketRequest;

            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5017), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.PhysicalPeriodicDelivery,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.PhysicalPeriodicDelivery,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_PhysicalPeriodicDelivery, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    IPosRequest posRequestPDP = null;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                            break;

                        try
                        {
                            // Insertion POSREQUEST (DLYPERIODIC)    
                            posRequestPDP = AddPosRequestPhysicalPeriodicDelivery(dr, out int newIdPR_PDP, posRequestGroupLevel.IdPR);
                            if (null != posRequestPDP)
                            {
                                codeReturn = SetPosKeepingQuote(posRequestPDP);
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    // EG 20170424 [23064]
                                    if (IsQuoteOk)
                                    {
                                        if (posRequestPDP.PosKeepingKeySpecified &&
                                            RequestCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestPDP.IdEM, posRequestPDP.PosKeepingKey) &&
                                            RequestOnContractCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestPDP.IdEM, posRequestPDP.PosKeepingKey))
                                        {
                                            // Traitement Trades
                                            codeReturn = PhysicalPeriodicDeliveryGen(posRequestPDP);
                                        }
                                        else
                                        {
                                            codeReturn = Cst.ErrLevel.DATAREJECTED;
                                        }
                                    }
                                    else
                                    {
                                        codeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                                    }
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
                            
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            m_PKGenProcess.ProcessState.AddCriticalException(ex);

                            
                            Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                            isException = true;
                        }
                        finally
                        {
                            //Update POSREQUEST (DLYPAYMENT)
                            UpdatePosRequest(codeReturn, posRequestPDP, posRequestGroupLevel.IdPR);
                        }
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
            
            Logger.Write();

            return (isException ? Cst.ErrLevel.FAILURE : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? codeReturn : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_PhysicalPeriodicDeliveryGen

        #region EOD_ShiftingGen
        /// <summary>
        /// REALISE LE SHIFTING AUTOMATIQUE DES POSITIONS LE LAST TRADING DAY (EOD)
        /// <para>[Pour les contrats pour lesquels est défini un shifting]</para>
        /// <para>[Attention: le Shifting à lieu le dernier jour de négociation et non à la date d'expiration]</para>
        /// </summary>
        /// <param name="pIdM">Id du business center du marché</param>
        /// <returns>Cst.ErrLevel</returns>
        ///EG 20140130 [19586] IsolationLevel
        /// EG 20140326 [19775] 
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        protected override Cst.ErrLevel EOD_ShiftingGen(string pIdBC)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;

            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5024), 2,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

            // Insert POSREQUEST REGROUPEMENT
            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);

            // Vérifier s'il y a des échéances de contrats en Shifting dont le dernier jour de négociation est le jour du traitement
            if (PosKeepingTools.IsShiftingDayForEntityMarket(CS, m_PosRequest.IdEM, m_MasterPosRequest.DtBusiness))
            {
                // PL 20180312 WARNING: Use Read Commited !                
                //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.Shifting,
                //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM);
                DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.Shifting,
                    m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM);

                if (null != ds)
                {
                    int nbRow = ds.Tables[0].Rows.Count;
                    // Insert POSREQUEST REGROUPEMENT
                    ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                    IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ShiftingGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                        status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);


                    if (0 < nbRow)
                    {
                        IOffset offset = m_Product.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness);
                        IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, pIdBC);
                        DateTime newTradeDtBusiness = Tools.ApplyOffset(CS, m_PosRequest.DtBusiness, offset, bda, null);

                        IPosRequest posRequestSHI = null;
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                break;

                            try
                            {
                                // Insertion POSREQUEST (SHI)    
                                posRequestSHI = AddPosRequestMaturityOffsetting(Cst.PosRequestTypeEnum.Shifting, dr, out int newIdPR_SHI, posRequestGroupLevel.IdPR);
                                if (null != posRequestSHI)
                                {
                                    if (IsQuoteOk)
                                    {
                                        if (posRequestSHI.PosKeepingKeySpecified &&
                                            RequestCanBeExecuted(Cst.PosRequestTypeEnum.RequestTypeMarket, posRequestSHI.IdEM, posRequestSHI.PosKeepingKey))
                                        {
                                            // Shifting de la position
                                            // EG 20170206 [22879]
                                            codeReturn = CascadingOrShiftingGen((IPosRequestCascadingShifting)posRequestSHI, newTradeDtBusiness);
                                        }
                                        else
                                        {
                                            codeReturn = Cst.ErrLevel.DATAREJECTED;
                                        }
                                    }
                                    else
                                    {
                                        codeReturn = Cst.ErrLevel.QUOTENOTFOUND;
                                    }
                                }
                                else
                                {
                                    codeReturn = Cst.ErrLevel.DATAREJECTED;
                                }
                            }
                            catch (Exception ex)
                            {
                                /// EG 20140326 [19775] 
                                codeReturn = Cst.ErrLevel.FAILURE;

                                m_PKGenProcess.ProcessState.AddCriticalException(ex);
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                
                                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                                isException = true;
                            }
                            finally
                            {
                                //Update POSREQUEST (SHI)
                                UpdatePosRequest(codeReturn, posRequestSHI, posRequestGroupLevel.IdPR);
                            }
                        }
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
            }
            else
            {
                _ = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ShiftingGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    ProcessStateTools.StatusNoneEnum, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);
            }

            
            Logger.Write();

            return (isException ? Cst.ErrLevel.FAILURE : (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? Cst.ErrLevel.IRQ_EXECUTED : Cst.ErrLevel.SUCCESS);
        }
        #endregion EOD_ShiftingGen
        #region EOD_RecalculationAmountGen
        /// <summary>
        /// CALCUL DES MONTANTS (VMG|SCU) sur ASS/EXE (IDSTCALCUL = TOCALC)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20150128 [20726]
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        protected override Cst.ErrLevel EOD_RecalculationAmountGen()
        {
            IDbTransaction dbTransaction = null;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                m_PosRequest = RestoreMarketRequest;

                // INSERTION LOG
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5012), 1,
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                    new LogParam(m_PosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                string sqlSelect = @"select pr.IDPR, ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDE_EVENT, ev.VALORISATION, evdet.DAILYQUANTITY as QTY, evdet.STRIKEPRICE, 
                tr.SIDE, ass.IDENTIFIER as ASSET_IDENTIFIER
                from dbo.POSREQUEST pr
                inner join dbo.VW_TRADE_POSETD tr on (tr.IDT = pr.IDT)
                inner join dbo.ASSET_ETD ass on (ass.IDASSET = tr.IDASSET) and ((ass.DTDISABLED is null) or (ass.DTDISABLED > @DTBUSINESS))
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'O')
                inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (isnull(ma.MATURITYDATE, ma.MATURITYDATESYS) >= @DTBUSINESS)
                inner join dbo.EVENT ev  on (ev.IDT = tr.IDT) and (ev.IDSTACTIVATION = 'REGULAR') and (ev.EVENTTYPE in ('TOT', 'PAR', 'SCU', 'VMG')) and (ev.IDSTCALCUL = 'TOCALC')
                inner join dbo.EVENT evp  on (evp.IDE = ev.IDE_EVENT) and (evp.EVENTCODE in ('ASS', 'EXE'))
                inner join dbo.EVENTDET evdet on (evdet.IDE = ev.IDE)
                where (pr.REQUESTTYPE in ('ASS','EXE')) and (pr.REQUESTMODE = 'ITD') and (pr.DTBUSINESS=@DTBUSINESS) and (pr.IDEM = @IDEM)";

                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), m_MasterPosRequest.DtBusiness); // FI 20201006 [XXXXX] DbType.Date
                parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_PosRequest.IdEM);
                QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
                // PL 20180312 WARNING: Use Read Commited !                
                //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
                DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);

                if (null != ds)
                {
                    int nbRow = ds.Tables[0].Rows.Count;
                    if (0 < nbRow)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            int idPR = Convert.ToInt32(row["IDPR"]);
                            int idT = Convert.ToInt32(row["IDT"]);
                            int idE = Convert.ToInt32(row["IDE"]);
                            int idE_Event = Convert.ToInt32(row["IDE_EVENT"]);
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            decimal qty = Convert.ToDecimal(row["QTY"]);
                            bool isDealerBuyer = IsTradeBuyer(row["SIDE"].ToString());
                            string eventType = row["EVENTTYPE"].ToString();

                            m_PosRequest = PosKeepingTools.GetPosRequest(CS, m_Product, idPR);
                            InitPosKeepingData(idT, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset, true);
                            if (IsQuoteOk)
                            {
                                dbTransaction = DataHelper.BeginTran(m_PKGenProcess.Cs);
                                PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(PosKeepingData, eventType, isDealerBuyer);
                                Nullable<decimal> _amount = null;

                                Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
                                if (EventTypeFunc.IsVariationMargin(eventType))
                                {
                                    // VMG de ASS|EXE
                                    DateTime dtQuote = PosKeepingData.Asset.GetOfficialCloseQuoteTime(DtBusiness);
                                    SystemMSGInfo errReadOfficialClose = null;
                                    if (Asset.maturityDateSys <= DtBusiness)
                                    {
                                        // Lecture en Date veille de dtQuote (MaturityDateSys)
                                        dtQuote = Tools.ApplyOffset(CS, m_Product, dtQuote, PeriodEnum.D, -1, DayTypeEnum.ExchangeBusiness, PosKeepingData.Asset.idBC, BusinessDayConventionEnum.PRECEDING, null);
                                    }
                                    Quote_ETDAsset quote = m_PKGenProcess.ProcessCacheContainer.GetQuote(PosKeepingData.IdAsset, dtQuote, PosKeepingData.Asset.identifier,
                                        QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote_ETDAsset;
                                    if ((null != quote) || quote.valueSpecified)
                                    {
                                        _amount = PosKeepingData.VariationMargin(quote.value, 0, qty);
                                        _payrec.SetCloseInfo(PosKeepingData, quote);
                                        _payrec.SetPayerReceiver(_amount);
                                        codeReturn = EventQuery.UpdateEvent_Amount(CS, dbTransaction, idE, _payrec.Amount, _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver, _payrec.IdStCalcul);
                                        codeReturn = EventQuery.UpdateEventDet_Closing(CS, dbTransaction, idE, _payrec.QuotePrice, _payrec.QuotePrice100, _payrec.QuoteDelta);
                                    }
                                }
                                else
                                {
                                    // ASS|EXE
                                    IPosKeepingQuote quote = Asset.GetSettlementPrice(DtBusiness, ref errLevel);
                                    // EG 20151001 [21414] 
                                    if (null != quote)
                                        _payrec.SetSettlementInfo(PosKeepingData, quote);
                                    decimal strikePrice = Convert.ToDecimal(row["STRIKEPRICE"]);
                                    if (EventTypeFunc.IsSettlementCurrency(eventType))
                                    {
                                        // SCU de ASS|EXE
                                        // EG 20151001 [21414] 
                                        if (null != quote)
                                            _amount = PosKeepingData.CashSettlement(strikePrice, quote.QuotePrice, qty);
                                        _payrec.SetPayerReceiver(_amount);
                                        codeReturn = EventQuery.UpdateEvent_Amount(CS, dbTransaction, idE, _payrec.Amount, _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver, _payrec.IdStCalcul);
                                    }
                                    codeReturn = EventQuery.UpdateEventDet_Denouement(CS, dbTransaction, idE, strikePrice, _payrec.QuoteSide, _payrec.QuoteTiming, _payrec.DtSettlementPrice, _payrec.SettlementPrice, _payrec.SettlementPrice100);
                                }
                                FinalizeTransaction(dbTransaction, codeReturn);
                            }
                        }
                    }
                }
                return codeReturn;
            }
            catch (Exception)
            {
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
        }
        #endregion EOD_RecalculationAmountGen

        #region EOD_UnclearingMaturityOffsettingGen
        /// <summary>
        /// Override de la méthode présente sur PosKeepingGenProcessBase
        /// pour la gestion des suppressions des dénouements automatiques et de leurs livraison
        /// dans la cas d'arrivée de nouvelles négociations impactant les positions options clôturées
        /// par dénouement.
        /// </summary>
        /// <returns></returns>
        // EG 20170412 [23081] Décompensation des dénouement automatiques avant mise à jour des clôtures (si ADD|UPD|DEL de trades post EOD)
        // EG 20170424 [23064] Rename EOD_UnclearingMaturityOffsettingGen + Gestion des liquidations automatiques
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override Cst.ErrLevel EOD_UnclearingMaturityOffsettingGen(int pIdPR_Parent)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_PosRequest = RestoreMarketRequest;

            // ------------------------------------------------------------------------------------------------------------------
            // Contrôle existence de dénouements automatiques opérés par un traitement EOD précédent 
            // pour la même journée et la même ENTITE et le même MARCHE (IDEM)
            // ------------------------------------------------------------------------------------------------------------------
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), m_PosRequest.DtBusiness); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_PosRequest.IdEM);

            // EG 20170424 [23064]  add 'MOF'
            string sqlQuery = @"select 1
            from dbo.POSACTION pa
            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA)
            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and 
            (pr.REQUESTTYPE in ('AUTOABN', 'AUTOASS', 'AUTOEXE', 'MOF'))
            where (pa.DTBUSINESS = @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))";

            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
            object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
            {
                IDbTransaction dbTransaction = null;
                IDataReader dr = null;
                bool isException = false;
                try
                {
                    DbSvrType serverType = DataHelper.GetDbSvrType(CS);

                    // ------------------------------------------------------------------------------------------------------------------
                    // Il existe des dénouements automatiques : 
                    // ---------------------------------------
                    // Existe-t-il des trades saisis, modifiés ou annulés post-traitement EOD ?
                    // Chargement des clés de position concernées par agrégat
                    // ------------------------------------------------------------------------------------------------------------------

                    // EG 20170424 [23064]  Comment => inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE in ('ASD','EXD'))
                    sqlQuery = @"select tr.SIDE, tr.IDASSET, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, isnull(tr.IDB_CLEARER, 0) as IDB_CLEARER
                    from dbo.VW_TRADE_POSETD tr
                    /*inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE in ('ASD','EXD'))*/
                    inner join dbo.TRADETRAIL tr_l on (tr_l.IDT = tr.IDT) and ({0})
                    inner join 
                    (
    	                select pr.DTUPD, pr.DTINS, pr.IDEM,pr.DTBUSINESS
                        from dbo.POSREQUEST pr
                        where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) 
                        /*and (pr.STATUS = @STATUS) */
                        and (pr.DTUPD = (select max(prmax.DTUPD) 
				        from dbo.POSREQUEST prmax 
                        where (prmax.REQUESTTYPE = @REQUESTTYPE) and (prmax.IDEM = @IDEM) 
                        /*and (prmax.STATUS = @STATUS) */
                        and (prmax.DTBUSINESS = @DTBUSINESS)))
                    ) pr on (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.DTINS < tr_l.DTSYS)
                    where (tr.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS)
                    group by tr.SIDE, tr.IDA_CSSCUSTODIAN, tr.IDASSET, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER";

                    if (DbSvrType.dbSQL == serverType)
                        sqlQuery = String.Format(sqlQuery, @"charindex('SpheresClosingGen', tr_l.APPNAME) = 0");
                    else if (DbSvrType.dbORA == serverType)
                        sqlQuery = String.Format(sqlQuery, @"instr(tr_l.APPNAME,'SpheresClosingGen') = 0");



                    parameters.Add(new DataParameter(CS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),
                        ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.EOD_MarketGroupLevel));
                    //parameters.Add(new DataParameter(CS, "STATUS", DbType.String, SQLCst.UT_STATUS_LEN), ProcessStateTools.StatusSuccess);

                    qryParameters = new QueryParameters(CS, sqlQuery, parameters);
                    dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    if (null != dr)
                    {
                        m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarket(m_PosRequest.IdA_Entity, m_PosRequest.IdM, m_PosRequest.IdA_CssCustodian);

                        parameters = new DataParameters();
                        parameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.DateTime), m_PosRequest.DtBusiness);
                        parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_PosRequest.IdEM);
                        parameters.Add(new DataParameter(CS, "DTENTITY", DbType.Date), m_EntityMarketInfo.DtEntity); // FI 20201006 [XXXXX] DbType.Date
                        parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), Convert.DBNull);
                        parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), Convert.DBNull);
                        parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), Convert.DBNull);
                        parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), Convert.DBNull);
                        parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), Convert.DBNull);
                        parameters.Add(new DataParameter(CS, "SIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);

                        // ------------------------------------------------------------------------------------------------------------------
                        // IMPORTANT !!!! Le nom des étiquettes est en phase avec la query du traitement UNCLEARING_MOO lancé via l'interface
                        // (voir : POSKEEPING_UNCLEARING_MOO.XML)
                        // ------------------------------------------------------------------------------------------------------------------

                        // EG 20170424 [23064]  add MOF
                        sqlQuery = @"select 
                            /* Used by SetPosRequestUnclearing */
                            tr.DTBUSINESS, tr.IDT, tr.IDENTIFIER as TRADE_IDENTIFIER, pad.IDT_CLOSING, tr.IDENTIFIER as TR_CLOSING_IDENTIFIER, 
                            tl.IDT_A as IDT_DELIVERY, tr_ssj.IDENTIFIER as TR_DLVRY_IDENTIFIER ,
                            pad.IDPADET, pad.QTY as QTY, pad.QTY as UNCLEARINGQTY, 
                            pr.IDPR, pr.REQUESTTYPE as REQUESTTYPE_VALUE, 
                            tr.IDI, tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, 0 as ISCUSTODIAN, 
                            tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_DEALER, tr.IDB_DEALER, 'FUT' as GPRODUCT, 
                            @IDEM as IDEM, @DTENTITY as DTENTITY
                            from dbo.POSACTION pa
                            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) 
                            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and 
                            (pr.REQUESTTYPE in ('AUTOABN', 'AUTOASS', 'AUTOEXE', 'MOF'))
                            inner join dbo.TRADE tr on (tr.IDT = pad.IDT_CLOSING)
                            left outer join dbo.TRADELINK tl on (tl.IDT_B = pad.IDT_CLOSING) and (tl.LINK like '%AutomaticOption%')
                            left outer join dbo.TRADE tr_ssj on (tr_ssj.IDT = tl.IDT_A)
                            where (pa.DTBUSINESS = @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)) and 
                            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and 
                            (tr.IDASSET = @IDASSET) and (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER, 0) = @IDB_CLEARER) and 
                            (tr.IDA_DEALER = @IDA_DEALER) and (tr.IDB_DEALER = @IDB_DEALER) and (tr.SIDE <> @SIDE) and (isnull(tr_ssj.IDSTACTIVATION,'REGULAR') = 'REGULAR')";


                        dbTransaction = DataHelper.BeginTran(CS);

                        while (dr.Read())
                        {
                            // ------------------------------------------------------------------------------------------------------------------
                            // Pour chaque clé de position trouvée : 
                            // ---------------------------------------
                            // Recherche des trade ayant été dénoué automatiquement pour effectuer une décompensation
                            // ------------------------------------------------------------------------------------------------------------------

                            parameters["IDASSET"].Value = Convert.ToInt32(dr["IDASSET"]);
                            parameters["IDA_DEALER"].Value = Convert.ToInt32(dr["IDA_DEALER"]);
                            parameters["IDB_DEALER"].Value = Convert.ToInt32(dr["IDB_DEALER"]);
                            parameters["IDA_CLEARER"].Value = Convert.ToInt32(dr["IDA_CLEARER"]);
                            parameters["IDB_CLEARER"].Value = Convert.ToInt32(dr["IDB_CLEARER"]);
                            parameters["SIDE"].Value = Convert.ToString(dr["SIDE"]);

                            qryParameters = new QueryParameters(CS, sqlQuery, parameters);
                            // PL 20180312 WARNING: Use Read Commited !                
                            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
                            DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);

                            if (null != ds)
                            {
                                int nbRows = ds.Tables[0].Rows.Count;
                                if (0 < nbRows)
                                {
                                    // Traitement Unclearing de dénouement automatique pour chaque trade trouvé
                                    // Insert POSREQUEST REGROUPEMENT
                                    SQLUP.GetId(out int newIdPR, dbTransaction, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRows);

                                    foreach (DataRow row in ds.Tables[0].Rows)
                                    {
                                        int idT = Convert.ToInt32(row["IDT"]);
                                        DeserializeTrade(idT);
                                        m_PosRequest = PosKeepingTools.SetPosRequestUnclearing(m_Product, row);
                                        m_PosRequest.RequestMode = SettlSessIDEnum.EndOfDay;
                                        m_PosRequest.IdPR = newIdPR;
                                        m_PosRequest.Status = ProcessStateTools.StatusPendingEnum;
                                        m_PosRequest.SourceSpecified = true;
                                        m_PosRequest.Source = m_PKGenProcess.AppInstance.ServiceName;

                                        IPosRequest savPosRequest = PosKeepingTools.ClonePosRequest(m_PosRequest);
                                        
                                        
                                        //PosKeepingTools.InsertPosRequest(CS, null, newIdPR, m_PosRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, pIdPR_Parent);
                                        PosKeepingTools.InsertPosRequest(CS, null, newIdPR, m_PosRequest, m_PKGenProcess.Session, IdProcess, pIdPR_Parent);
                                        
                                        codeReturn = UnclearingGen(dbTransaction, true, m_PosRequest.IdPR);
                                        // Update POSREQUEST (UNCLEARING)    
                                        UpdatePosRequest(codeReturn, savPosRequest, pIdPR_Parent);

                                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                                        {
                                            isException = true;
                                            break;
                                        }
                                        newIdPR++;
                                    }
                                }
                            }

                            if (isException)
                                break;
                        }

                        if ((null != dbTransaction) && (false == isException))
                            DataHelper.CommitTran(dbTransaction);
                    }

                }
                catch (Exception)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    isException = true;
                    throw;
                }
                finally
                {
                    if (null != dr)
                        dr.Close();

                    if (null != dbTransaction)
                    {
                        if (isException) { DataHelper.RollbackTran(dbTransaction); }
                        dbTransaction.Dispose();
                    }
                    m_PosRequest = RestoreMarketRequest;
                }
            }
            return codeReturn;
        }
        #endregion EOD_UnclearingMaturityOffsettingGen

        #region CLOSINGDAY_EODControl
        /// <summary>
        /// Controle du traitement EOD (exclusivement lors de la clôture de journée)
        /// </summary>
        /// <param name="pIdA_Entity">ID Entité</param>
        /// <param name="pIdA_CSS">ID Chambre de compensation</param>
        /// <param name="pIdEM">ID EntityMarket</param>
        /// <param name="pDtBusiness">Date de traitement (journée de bourse courante)</param>
        /// <param name="pIdPR_Parent">ID PosRequest parent</param>
        /// <returns></returns>
        /// EG 20140120 Report 3.7  New
        /// EG 20141208 [20439] Add IsolationLevel = ReadUncommitted
        /// FI 20160819 [22364] Modify
        // EG 20171128 [23331] Upd
        // EG 20180221 [23769] Upd Signature && Used collection List<IposRequest> m_LstMarketSubPosRequest|m_LstSubPosRequest
        // EG 20180525 [23979] IRQ Processing
        // [26063][WI397] Gestion du marquage d'un traitement de clôture de journée précédemment en erreur
        // pour pouvoir débloquer le changement de journée.
        protected override Cst.ErrLevel CLOSINGDAY_EODControl(int pIdA_Entity, int pIdA_Css, Nullable<int> pIdA_Custodian, int pIdEM, DateTime pDtBusiness,
            int pIdPR_Parent, Nullable<ProductTools.GroupProductEnum> pGroupProduct)
        {
            bool isControlMustBeExecuted = false;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            int newIDPR = pIdPR_Parent;
            #region EOD PROCESS exist and his status is SUCCESS
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.ClosingDay, pDtBusiness, pIdEM, null, null, null);
            if (null != ds)
            {
                newIDPR++;
                codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                    Cst.PosRequestTypeEnum.ClosingDay_EndOfDayGroupLevel, pIdPR_Parent, pGroupProduct);
                // [26063][WI397] New
                isControlMustBeExecuted = (false == IsSomeClosingDayControlWillBeSkipped(IsolationLevel.ReadUncommitted, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness));
            }
            #endregion EOD PROCESS exist and his status is SUCCESS

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region NEW ALLOC AFTER LAST EOD PROCESS
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
                #region CASCADING OF POSITION
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.Cascading, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_CascadingGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion CASCADING OF POSITION
            }

            // [26063][WI397] Upd
            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) && isControlMustBeExecuted)
            {
                #region MANUAL ACTIONS ON OPTIONS
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.OptionExercise, pDtBusiness, pIdEM, null,
                    PosKeepingTools.PosActionType.Trade, SettlSessIDEnum.EndOfDay);

                if (null != ds)
                {
                    DataSet ds2 = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.OptionExercise, pDtBusiness, pIdEM, null,
                        PosKeepingTools.PosActionType.Position, SettlSessIDEnum.EndOfDay);
                    if (null != ds2)
                    {
                        foreach (DataRow row in ds2.Tables[0].Rows)
                        {
                            ds.Tables[0].LoadDataRow(row.ItemArray, false);
                        }
                    }
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_ManualOptionGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion MANUAL ACTIONS ON OPTIONS
            }

            // [26063][WI397] Upd
            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) && isControlMustBeExecuted)
            {
                #region AUTOMATIC ACTIONS ON OPTIONS
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.AutomaticOptionExercise, pDtBusiness, pIdEM, 480, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_AutomaticOptionGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion AUTOMATIC ACTIONS ON OPTIONS
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region UNDERLYER DELIVERY TRADE INSERTION
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.UnderlyerDelivery, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_UnderlyerDeliveryGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion UNDERLYER DELIVERY TRADE INSERTION
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region MATURITY OFFSETTING ON FUTURE
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.MaturityOffsettingFuture, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_MaturityOffsettingFutureGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion MATURITY OFFSETTING ON FUTURE
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region AUTOMATIC CLEARING BOOK
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
                                posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel, newIDPR,
                                    pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ProcessStateTools.StatusErrorEnum, pIdPR_Parent, m_LstSubPosRequest, pGroupProduct);
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

                            // EG 20171128 [23331]  
                            AddSubPosRequest(m_LstSubPosRequest, _posRequest);
                        }
                    }

                    if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
                    {
                        if (null == posRequestGroupLevel)
                        {
                            newIDPR++;
                            InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel, newIDPR,
                                pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ProcessStateTools.StatusSuccessEnum, pIdPR_Parent, m_LstSubPosRequest, pGroupProduct);

                        }
                    }
                }
                #endregion AUTOMATIC CLEARING BOOK
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region PHYSICAL PERIODIC DELIVERY ON FUTURE
                // EG 20170206 [22787]
                ds = PosKeepingTools.GetStatusEndOfDayProcessControl(CS, Cst.PosRequestTypeEnum.EOD_PhysicalPeriodicDelivery, pDtBusiness, pIdEM);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.EOD_PhysicalPeriodicDelivery, pIdPR_Parent, pGroupProduct);
                }
                #endregion PHYSICAL PERIODIC DELIVERY ON FUTURE
            }

            if (Cst.ErrLevel.IRQ_EXECUTED != codeReturn)
            {
                #region CASH FLOWS CALCULATION
                // EG 20170206 [22787]
                //ds = PosKeepingTools.GetDataClosingDayCashFlowsControl(pCS, Cst.PosRequestTypeEnum.ClearingEndOfDay, pDtBusiness, pIdEM);
                ds = PosKeepingTools.GetStatusEndOfDayProcessControl(CS, Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel, pDtBusiness, pIdEM);
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
                #region FEES CALCULATION
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
        /// <param name="pIdBC">Id du business center du marché</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20180525 [23979] IRQ Processing
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
                    new LogParam(m_PosRequest.GroupProductValue),
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
                // STEP 2 : SHIFTING DE POSITION
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ShiftingGen(pIdBC);
                }
                // STEP 3 : CLOSING/REOPENING ACTIONS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_ClosingReopeningActionsGen();
                }
                // STEP 4 : CORPORATE ACTION (SOD)
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
        // EOD/CLOSINGDAY PRIVATE METHODS 
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_OptionGen
        /// <summary>
        /// DENOUEMENT MANUEL D'OPTIONS (EOD)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► REQUESTMODE = EOD (EndOfDay)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>Retourne Cst.ErrLevel.IRQ_EXECUTED ou Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140204 [19586] IsolationLevel 
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        /// EG 20180307 [23769] Gestion List de IPosRequest
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel EOD_OptionGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5003), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            // Gestion des denouement manuel directement appliqué à des trades (saisie manuelle)
            // PL 20180312 WARNING: Use Read Commited !                
            //DataSet dsTrade = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.OptionAbandon,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, PosKeepingTools.PosActionType.Trade, SettlSessIDEnum.EndOfDay);
            DataSet dsTrade = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.OptionAbandon,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, PosKeepingTools.PosActionType.Trade, SettlSessIDEnum.EndOfDay);

            // Gestion des denouement manuel appliqué sur des positions (issu en générale d'importation)
            // PL 20180312 WARNING: Use Read Commited !                
            //DataSet dsPosition = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.OptionAbandon,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, PosKeepingTools.PosActionType.Position, SettlSessIDEnum.EndOfDay);
            DataSet dsPosition = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.OptionAbandon,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, PosKeepingTools.PosActionType.Position, SettlSessIDEnum.EndOfDay);

            if (null != dsTrade || null != dsPosition)
            {
                int nbTrade = 0;
                if (null != dsTrade)
                    nbTrade = dsTrade.Tables[0].Rows.Count;

                int nbPosition = 0;
                if (null != dsPosition)
                    nbPosition = dsPosition.Tables[0].Rows.Count;

                int nbRow = nbTrade + nbPosition;

                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);

                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_ManualOptionGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    // EG 20140113 Gestion du CodeReturn/Status sur posRequestGroupLevel
                    if (0 < nbTrade)
                    {
                        DataRow[] rows = GetRowsInSpheresOrder(dsTrade.Tables[0]);
                        foreach (DataRow dr in rows)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                break;

                            Cst.ErrLevel codeReturnItem = PosRequestTradeOptionGen(null, dr, posRequestGroupLevel.IdPR);
                            if (codeReturnItem != Cst.ErrLevel.SUCCESS)
                                codeReturn = codeReturnItem;
                        }
                    }

                    // Les exe,abn,ass sur position sont effectuées après  les exe,abn,ass saisis manuellement sur les trades
                    // Cette étape n'est exécuté que lorsque l'étape précédente est ok
                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        if (nbPosition > 0)
                        {
                            DataRow[] rows = GetRowsInSpheresOrder(dsPosition.Tables[0]);
                            foreach (DataRow dr in rows)
                            {
                                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                                    break;

                                PositionOptionGen(Convert.ToInt32(dr["IDPR"]), posRequestGroupLevel.IdPR);
                            }
                        }
                    }
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

            return (Cst.ErrLevel.IRQ_EXECUTED == codeReturn) ? codeReturn : Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_OptionGen

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // OVERRIDE METHODS (VIRTUAL SUR PosKeepingGenProcessBase)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region DeserializeTrade
        /// <summary>
        /// Alimente m_TradeLibrary et  m_ExchangeTradedDerivativeContainer
        /// </summary>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        protected override void DeserializeTrade(IDbTransaction pDbTransaction, int pIdT)
        {
            base.DeserializeTrade(pDbTransaction, pIdT);
            IExchangeTradedDerivative etd = (IExchangeTradedDerivative)m_TradeLibrary.Product;
            m_RptSideProductContainer = new ExchangeTradedDerivativeContainer(CS, pDbTransaction, etd);
        }
        #endregion DeserializeTrade

        #region GetQueryDataRequest

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRequestType"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdEM"></param>
        /// <param name="pPosActionType"></param>
        /// <param name="pSettlSessIDEnum"></param>
        /// <returns></returns>
        /// EG 20141224 [20566]
        /// FI 20160819 [22364] Modify
        /// EG 20160106 Global Refactoring
        // EG 20170206 [22787] Add Test on Cst.PosRequestTypeEnum.PhysicalDeliveryPayment
        // EG 20181010 PERF EOD sur MOF|MOO
        // EG 20201028 [XXXXX] Refactoring sur requête de Cascading
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected override Pair<string, DataParameters> GetQueryDataRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, DateTime pDate, int pIdEM,
            Nullable<PosKeepingTools.PosActionType> pPosActionType, Nullable<SettlSessIDEnum> pSettlSessIDEnum)
        {
            Pair<string, DataParameters> _query = base.GetQueryDataRequest(pCS, pDbTransaction, pRequestType, pDate, pIdEM, pPosActionType, pSettlSessIDEnum);

            if (null == _query)
            {
                string sqlSelect = string.Empty;
                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
                parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
                switch (pRequestType)
                {
                    case Cst.PosRequestTypeEnum.AutomaticOptionAbandon:
                    case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                    case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                        parameters.Clear();
                        parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
                        sqlSelect = GetQueryCandidatesToMaturityOffsettingOption(pCS);
                        break;
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                    case Cst.PosRequestTypeEnum.OptionExercise:
                        if (false == pPosActionType.HasValue)
                            throw new ArgumentException("pPosActionType is null");
                        sqlSelect = GetQueryCandidatesToManualDenouement(pPosActionType.Value, pSettlSessIDEnum.Value);
                        break;
                    case Cst.PosRequestTypeEnum.UnderlyerDelivery:
                        sqlSelect = GetQueryCandidatesToUnderlyerDelivery();
                        break;
                    case Cst.PosRequestTypeEnum.MaturityOffsettingFuture:
                        parameters.Clear();
                        parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
                        sqlSelect = GetQueryCandidatesToMaturityOffsettingFuture(pCS);
                        break;
                    case Cst.PosRequestTypeEnum.Cascading:
                        parameters.Clear();
                        parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
                        sqlSelect = GetQueryCandidatesToPositionCascading(pCS);
                        break;
                    case Cst.PosRequestTypeEnum.Shifting:
                        sqlSelect = GetQueryCandidatesToPositionShifting(pCS);
                        break;
                    // EG 20170206 [22787] New
                    case Cst.PosRequestTypeEnum.PhysicalPeriodicDelivery:
                        sqlSelect = GetQueryCandidatesToPhysicalPeriodicDelivery(pCS);
                        break;
                }

                if (StrFunc.IsFilled(sqlSelect))
                {
                    _query = new Pair<string, DataParameters>(sqlSelect, parameters);
                }
            }
            return _query;
        }
        #endregion GetQueryDataRequest
        #region GetQueueForEODCashFlows
        // EG 20150113 [20501]
        // EG 20170324 [22991] Set eodComplement
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
            string _assetIdentifier = pRow["ASSET_IDENTIFIER"].ToString();
            DateTime _maturityDateSys = m_MasterPosRequest.DtBusiness;
            if (false == Convert.IsDBNull(pRow["MATURITYDATESYS"]))
                _maturityDateSys = Convert.ToDateTime(pRow["MATURITYDATESYS"]);

            DateTime _dtQuote = m_MasterPosRequest.DtBusiness;
            if (DtFunc.IsDateTimeFilled(_maturityDateSys) && (_dtQuote >= _maturityDateSys))
                _dtQuote = _maturityDateSys;

            SystemMSGInfo errReadOfficialClose = null;
            Quote_ETDAsset quote = GetQuoteLockWithKeyQuote(_idAsset, _dtQuote, _assetIdentifier, Cst.UnderlyingAsset.ExchangeTradedContract, ref errReadOfficialClose) as Quote_ETDAsset;
            // EG 20150113 [20501] On va dans EVENTSVAL même si pas de quotation
            // Test sur errReadOfficialClose parce que valorisé uniquement lorsqu'il y a lecture dans la base de donnée (donc sera affiché 1 seul fois dans le log pour les n trades)
            if ((null != errReadOfficialClose) && isEOD)
            {
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, errReadOfficialClose.SysMsgCode, 0, errReadOfficialClose.LogParamDatas));
            }
            if ((false == quote.valueSpecified) && isEOD)
            {
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5161), 0, new LogParam(LogTools.IdentifierAndId(_tradeIdentifier, _idT))));
            }

            if (null != quote)
            {
                quote = SerializationHelper.DeepClone(quote) as Quote_ETDAsset;
                // EG 20140210 la date est écrasée par la date Business (les événements sont en DTBUSINESS)
                quote.time = m_MasterPosRequest.DtBusiness;
                quote.action = DataRowState.Added.ToString();
                quote.isEODSpecified = true;
                quote.isEOD = isEOD;

                // EG 20170324 [22991] new (pour éviter une nouvelle execution de query par trade dans EventsVal
                if (isEOD)
                {
                    quote.eodComplementSpecified = isEOD;
                    quote.eodComplement = new Quote_EODComplement
                    {
                        isPosKeeping_BookDealer = true,
                        posQuantityPrevAndActionsDay = Convert.ToDecimal(pRow["QTY"]),
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
                                new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_FUT)}
                    },
                    idInfoSpecified = true
                };
            }
            return mQueue;
        }
        #endregion GetQueueForEODCashFlows

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
            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;
            string eventType = EventTypeFunc.Nominal;

            int idA_Payer;
            int idB_Payer;
            int idA_Receiver;
            int idB_Receiver;
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
            //FI 20110816  [17548] Sur l'évènement de Nominal Spheres® insère la devise du nominal  
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                eventCode, eventType, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness,
                nominalValue, pPosKeepingData.Asset.nominalCurrency, UnitTypeEnum.Currency.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            pIdE++;
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
        #region InsertOptionEvents
        /// <summary>
        /// <para>Insertion d'un package Evénement : Dénouement d'options</para>
        /// <para>Cas 1 : EXERCISE</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>TOTAL EXERCISE      EXE     TOT         GRP</para>
        /// <para>CASH SETTLEMENT     TER     SCU         REC/STL</para>
        /// <para>─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ </para>
        /// <para>PARTIAL EXERCISE    EXE     PAR         GRP</para>
        /// <para>CASH SETTLEMENT     INT     SCU         REC/STL</para>
        /// <para>PARTIAL ABANDON     ABN     PAR         GRP (si AbandonRemainingQty = true)</para>
        /// <para></para>
        /// <para>Cas 2 : ASSIGNMENT</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>TOTAL ASSIGNMENT    ASS     TOT         GRP</para>
        /// <para>CASH SETTLEMENT     TER     SCU         REC/STL</para>
        /// <para>─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ </para>
        /// <para>PARTIAL ASSIGNMENT  ASS     PAR         GRP</para>
        /// <para>CASH SETTLEMENT     INT     SCU         REC/STL</para>
        /// <para>PARTIAL ABANDON     ABN     PAR         GRP (si AbandonRemainingQty = true)</para>
        /// <para>Cas 3 : ABANDON</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>TOTAL ABANDON       ABN     TOT         GRP</para>
        /// <para>─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ </para>
        /// <para>PARTIAL ABANDON     ABN     PAR         GRP</para>
        /// </summary>
        /// <param name="pDbTransaction">Current Transaction</param>
        /// <param name="pRow">ClosingTrade</param>
        /// <param name="pActionQuantity"></param>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        // EG 20140711 (New - Override)
        // EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
        protected override Cst.ErrLevel InsertOptionEvents(IDbTransaction pDbTransaction, DataRow pRow, DataRow pRowPosActionDet, int pIdPADET, ref int pIdE)
        {
            #region Variables
            int idT = Convert.ToInt32(pRow["IDT"]);
            int idE = pIdE;
            int idE_Event = Convert.ToInt32(pRow["IDE_EVENT"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal originalClosingQuantity = Convert.ToDecimal(pRowPosActionDet["ORIGINALQTY_CLOSING"]);
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            decimal remainQuantity = originalClosingQuantity - qty;
            bool isDealerBuyer = IsTradeBuyer(pRow["SIDE"].ToString());
            string eventClass = pRow["EVENTCLASS"].ToString();
            decimal price = Convert.ToDecimal(pRow["PRICE"]);
            #endregion Variables

            #region EVENT ABANDON/ASSIGNMENT/EXERCISE
            /// EG 20140121 Homologuation
            IPosRequestDetOption detail = (IPosRequestDetOption)m_PosRequest.DetailBase;

            StatusCalculEnum idStCalc = IsQuoteOk ? StatusCalculEnum.CALC : StatusCalculEnum.TOCALC;

            string eventCode = OptionEventCode;
            string eventType = remainQuantity == 0 ? EventTypeFunc.Total : EventTypeFunc.Partiel;

            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, idT, idE, idE_Event, null, 1, 1, null, null, null, null,
                eventCode, eventType, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                qty, null, UnitTypeEnum.Qty.ToString(), idStCalc, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, eventClass, DtBusiness, false);

            // Insertion d'une Date DLY sur Exercise/Assignment
            if ((Cst.ErrLevel.SUCCESS == codeReturn) &&
                (false == EventCodeFunc.IsAbandon(eventCode)) &&
                (false == EventCodeFunc.IsAutomaticAbandon(eventCode)))
            {
                if (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
                {
                    DateTime deliveryDate = GetDeliveryDate(DtBusiness);
                    codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.DeliveryDelay, deliveryDate, false);
                }
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                /// EG 20131218 [19353/19355]
                /// EG 20140123 Homologuation
                Nullable<QuoteTimingEnum> quoteTiming = null;
                Nullable<QuotationSideEnum> quoteSide = null;
                Nullable<decimal> settlementPrice = null;
                Nullable<decimal> settlementPrice100 = null;
                Nullable<DateTime> dtQuote = null;

                // EG 20140115 [19456] Si nous sommes le jour de l'échéance alors lecture du FinalSettlementPrice
                _ = GetQuoteOfAsset(ref dtQuote, ref quoteTiming, ref quoteSide, ref settlementPrice, ref settlementPrice100);
                if (IsQuoteOk)
                {
                    // EG 20140326 [19771]
                    if (dtQuote.HasValue && (dtQuote.Value != DateTime.MinValue))
                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.Fixing, dtQuote.Value, false);
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    codeReturn = m_EventQuery.InsertEventDet_Denouement(pDbTransaction, idE, qty, null,
                        detail.StrikePrice, quoteSide, quoteTiming, dtQuote, settlementPrice, settlementPrice100, m_PosRequest.Notes);

                }
            }

            /// EG 20140121 Homologuation (Add EVENTASSET)
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region Insertion EVENTASSET avec info du sous-jacent
                // EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
                if ((Cst.ErrLevel.SUCCESS == codeReturn) && (detail.UnderlyerSpecified))
                {
                    if ((0 == detail.Underlyer.IdAsset) && Asset.idAsset_UnderlyerSpecified)
                    {
                        detail.Underlyer.IdAsset = Asset.idAsset_Underlyer;
                        detail.Underlyer.Identifier = Asset.identifier_Underlyer;
                        detail.Underlyer.AssetCategory = Asset.assetCategory_Underlyer;
                    }
                    EFS_Asset efs_Asset = detail.Underlyer.GetCharacteristics(CS, pDbTransaction);
                    codeReturn = EventQuery.InsertEventAsset(pDbTransaction, idE, efs_Asset);
                }
                #endregion Insertion EVENTASSET avec info du sous-jacent
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);

            #endregion EVENT ABANDON/ASSIGNMENT/EXERCISE

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region EVENT NOM/QTY (Nominal/Quantité)
                idE_Event = idE;
                idE++;
                codeReturn = InsertNominalQuantityEvent(pDbTransaction, idT, ref idE, idE_Event, DtBusiness, isDealerBuyer, qty, remainQuantity, pIdPADET);
                #endregion EVENT NOM/QTY (Nominal/Quantité)
            }


            if ((Cst.ErrLevel.SUCCESS == codeReturn) && ExchangeTradedDerivativeContainer.IsFuturesStyleMarkToMarket)
            {
                #region VMG
                idE++;
                codeReturn = InsertVariationMarginEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, price, qty, m_RptSideProductContainer.ClearingBusinessDate);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);
                #endregion VMG
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region RMG
                idE++;
                // SI RMG >=0 Le payeur est l'acheteur de l'opération dénouée
                // SI RMG <0  Le payeur est le vendeur de l'opération dénouée
                codeReturn = InsertRealizedMarginEvent(pDbTransaction, idT, ref idE, idE_Event, isDealerBuyer, price, 0, qty, pIdPADET, true);
                #endregion RMG
            }


            if ((Cst.ErrLevel.SUCCESS == codeReturn) &&
                (false == EventCodeFunc.IsAbandon(eventCode)) &&
                (false == EventCodeFunc.IsAutomaticAbandon(eventCode)) &&
                EventClassFunc.IsCashSettlement(eventClass))
            {
                #region EVENT SCU (Cash Settlement sur Exercice/Assignation)
                // EG 20140121 Homologuation
                idE++;
                // EG 20130529 Passe paramètre pIdPADET
                codeReturn = InsertSettlementCurrencyEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, qty, remainQuantity, detail, pIdPADET);


                #endregion EVENT SCU (Cash Settlement)
            }
            pIdE = idE;
            return codeReturn;
        }
        #endregion InsertOptionEvents

        #region InsertPhysicalDeliveryAmountEvent
        // EG 20170424 [23064] (TER|DVA et EVENTPOSACTIONDET)
        protected Cst.ErrLevel InsertPhysicalDeliveryAmountEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, 
            DateTime pDeliveryDate, decimal pSettlementQty, string pUnitOfMeasure, string pCurrency, bool pIsDealerBuyer, bool pIsPayment, 
            Nullable<DateTime> pDtQuote, Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming, Nullable<decimal> pSettltPrice,
            Nullable<decimal> pSettltPrice100, Nullable<int> pIdPADET)
        {
            return InsertPhysicalDeliveryAmountEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, EventCodeFunc.Termination, EventTypeFunc.DeliveryAmount,
            pDeliveryDate, pDeliveryDate, pDeliveryDate, pDeliveryDate, pDeliveryDate, Tz.Tools.UniversalTimeZone, 
            pSettlementQty, pUnitOfMeasure, pCurrency, pIsDealerBuyer, pIsPayment, true,
            pDtQuote, pQuoteSide, pQuoteTiming, pSettltPrice, pSettltPrice100, pIdPADET);
        }
        // EG 20170424 [23064] (TER|DVA et EVENTPOSACTIONDET)
        // EG 20190129 [24361] Settlement Date toujours +1JO sur la base de Value Date
        protected Cst.ErrLevel InsertPhysicalDeliveryAmountEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            string pEventCode, string pEventType,
            DateTime pDeliveryDate, DateTime pStartPeriod, DateTime pEndPeriod, DateTime pDeliveryDateStart, DateTime pDeliveryDateEnd, string pDeliveryTimeZone,
            decimal pSettlementQty, string pUnitOfMeasure, string pCurrency, bool pIsDealerBuyer, bool pIsPayment, bool pIsMinusOneDayValueDate,
            Nullable<DateTime> pDtQuote, Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming, Nullable<decimal> pSettltPrice, 
            Nullable<decimal> pSettltPrice100, Nullable<int> pIdPADET)
        {
            //DealerSeller --> Receiver de DVA
            int idA_Payer = PosKeepingData.IdA_Clearer;
            int idB_Payer = PosKeepingData.IdB_Clearer;
            int idA_Receiver = PosKeepingData.IdA_Dealer;
            int idB_Receiver = PosKeepingData.IdB_Dealer;
            if (pIsDealerBuyer)
            {
                //DealerBuyer --> Payer de DVA
                idA_Payer = PosKeepingData.IdA_Dealer;
                idB_Payer = PosKeepingData.IdB_Dealer;
                idA_Receiver = PosKeepingData.IdA_Clearer;
                idB_Receiver = PosKeepingData.IdB_Clearer;
            }

            m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                    pEventCode, pEventType, pStartPeriod, pStartPeriod, pEndPeriod, pEndPeriod,
                    pSettlementQty * pSettltPrice100.Value, pCurrency, UnitTypeEnum.Currency.ToString(), null, null);

            //EventClass (REC|VAL|STL)
            m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);
            //WARNING: dtVAL=deliveryDate-1JO afin que ce flux soit pris en compte dans le Cash-Balance précédent celui du jour de livraion (Demande AKROS).
            // EG 20190129 [24361] Settlement Date toujours +1JO sur la base de Value Date
            DateTime valueDate = pDeliveryDate;
            DateTime settlementDate = pDeliveryDate;
            if (pIsMinusOneDayValueDate)
                valueDate = GetValueDateOnDVA(pDeliveryDate);
            else
                settlementDate = GetSettlementDateOnDVA(pDeliveryDate);

            m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, valueDate, false);
            m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, settlementDate, pIsPayment);

            //EventDet
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEventDet_Delivery(pDbTransaction, pIdE, pSettlementQty, pUnitOfMeasure,
                pQuoteSide, pQuoteTiming, pDtQuote, pSettltPrice, pSettltPrice100, pDeliveryDateStart, pDeliveryDateEnd, pDeliveryTimeZone, null);

            // EventPosActionDet
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && pIdPADET.HasValue)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET.Value, pIdE);

            return codeReturn;
        }
        #endregion InsertPhysicalDeliveryAmountEvent
        #region InsertPhysicalDeliveryEvent
        // EG 20170424 [23064] (DLV|PHY et EVENTPOSACTIONDET)
        // EG 20170615 Conversion explicite en DateTimeOffset
        protected Cst.ErrLevel InsertPhysicalDeliveryEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            DateTime pDeliveryDate, decimal pQty, decimal pTotalDeliveryQty, string pUnitOfMeasure,
            Nullable<DateTime> pDtQuote, Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming,
            Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100, Nullable<int> pIdPADET)
        {
            // EG 20170615
            DateTimeOffset deliveryDateOffset = new DateTimeOffset(pDeliveryDate, TimeSpan.Zero);
            return InsertPhysicalDeliveryEvent(pDbTransaction, pIdT, pIdE, pIdE_Event,
            pDeliveryDate, pDeliveryDate, deliveryDateOffset.UtcDateTime, deliveryDateOffset.UtcDateTime, Tz.Tools.UniversalTimeZone, 
            pQty, pTotalDeliveryQty, pUnitOfMeasure,
            pDtQuote, pQuoteSide, pQuoteTiming, pSettltPrice, pSettltPrice100, pIdPADET);
        }
        protected Cst.ErrLevel InsertPhysicalDeliveryEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            DateTime pFirstDeliveryDate, DateTime pLastDeliveryDate, DateTime pDeliveryDateStart, DateTime pDeliveryDateEnd, string pDeliveryTimeZone, 
            decimal pQty, decimal pTotalDeliveryQty, string pUnitOfMeasure,
            Nullable<DateTime> pDtQuote, Nullable<QuotationSideEnum> pQuoteSide, Nullable<QuoteTimingEnum> pQuoteTiming, 
            Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100, Nullable<int> pIdPADET)
        {
            m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, null, null, null, null,
                    EventCodeFunc.PhysicalDelivery, EventTypeFunc.PhysicalSettlement, pFirstDeliveryDate, pFirstDeliveryDate, pLastDeliveryDate, pLastDeliveryDate,
                    pTotalDeliveryQty, pUnitOfMeasure, UnitTypeEnum.Qty.ToString(), null, null);

            //EventClass (GRP)
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.GroupLevel, DtBusiness, false);
            if (pDtQuote.HasValue && (pDtQuote.Value != DateTime.MinValue))
            {
                //EventClass (FXG)
                m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Fixing, pDtQuote.Value, false);
                //EventDet (FXG)
                codeReturn = m_EventQuery.InsertEventDet_Delivery(pDbTransaction, pIdE, pQty, null,
                    pQuoteSide, pQuoteTiming, pDtQuote, pSettltPrice, pSettltPrice100, pDeliveryDateStart, pDeliveryDateEnd, pDeliveryTimeZone, null);
            }

            // EventPosActionDet
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && pIdPADET.HasValue)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET.Value, pIdE);

            return codeReturn;
        }
        #endregion InsertPhysicalDeliveryEvent
        #region InsertPhysicalDeliveryQuantityEvent
        // EG 20170424 [23064] (TER|QTY et EVENTPOSACTIONDET)
        protected Cst.ErrLevel InsertPhysicalDeliveryQuantityEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            DateTime pDeliveryDate, decimal pDeliveryQty, string pUnitOfMeasure, bool pIsDealerBuyer, Nullable<int> pIdPADET)
        {
            return InsertPhysicalDeliveryQuantityEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, EventCodeFunc.Termination,
            pDeliveryDate, pDeliveryDate, pDeliveryDate, pDeliveryDate, pDeliveryDate, Tz.Tools.UniversalTimeZone, pDeliveryQty, pUnitOfMeasure, pIsDealerBuyer, pIdPADET);
        }
        protected Cst.ErrLevel InsertPhysicalDeliveryQuantityEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, string pEventCode,
            DateTime pDeliveryDate, DateTime pStartPeriod, DateTime pEndPeriod, DateTime pDeliveryDateStart, DateTime pDeliveryDateEnd, string pDeliveryTimeZone, 
            decimal pDeliveryQty, string pUnitOfMeasure, bool pIsDealerBuyer, Nullable<int> pIdPADET)
        {
            //DealerSeller --> Receiver de DVA
            int idA_Payer = PosKeepingData.IdA_Clearer;
            int idB_Payer = PosKeepingData.IdB_Clearer;
            int idA_Receiver = PosKeepingData.IdA_Dealer;
            int idB_Receiver = PosKeepingData.IdB_Dealer;
            if (pIsDealerBuyer)
            {
                //DealerBuyer --> Payer de DVA
                idA_Payer = PosKeepingData.IdA_Dealer;
                idB_Payer = PosKeepingData.IdB_Dealer;
                idA_Receiver = PosKeepingData.IdA_Clearer;
                idB_Receiver = PosKeepingData.IdB_Clearer;
            }

            m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                    pEventCode, EventTypeFunc.Quantity, pStartPeriod, pStartPeriod, pEndPeriod, pEndPeriod, pDeliveryQty, pUnitOfMeasure, UnitTypeEnum.Qty.ToString(), null, null);

            //EventClass (DLY)
            m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);
            m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, pDeliveryDate, false);

            //EventDet
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEventDet_Delivery(pDbTransaction, pIdE, pDeliveryQty, pUnitOfMeasure,
                null, null, null, null, null, pDeliveryDateStart, pDeliveryDateEnd, pDeliveryTimeZone,  null);

            // EventPosActionDet
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && pIdPADET.HasValue)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET.Value, pIdE);

            return codeReturn;
        }
        #endregion InsertPhysicalDeliveryQuantityEvent
        #region InsertPosKeepingEvents
        /// <summary>
        /// <para>Insertion d'un package Evénement : OFFSETTING/POSITIONCORRECTION</para>
        /// <para>Evénement           Code    Type        Class</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>OFFSETTING          OFS     PAR/TOT     GRP</para>
        /// <para>ou</para>
        /// <para>POSITION CORRECTION POC     PAR/TOT     GRP</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>NOMINAL             NOM     INT/TER     REC</para>
        /// <para>QUANTITY            QTY     INT/TER     REC</para>
        /// <para>─────────────────────────────────────────────</para>
        /// <para>REALIZED MARGIN     RMG     LPC         REC</para>
        /// <para>VARIATION           VMG     LPC         REC</para>
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
            #region Variables
            int idT = Convert.ToInt32(pRow["IDT"]);
            int idE = pIdE;
            int idE_Event = Convert.ToInt32(pRow["IDE_EVENT"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
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

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                if ((IsRequestPositionCancelation || IsRequestPositionTransfer))
                {
                    #region EVENT PRM/VMG (Restitution)
                    idE++;
                    // EG 20130730 [18754] Add parameter (pIsPaymentSettlement)
                    if (IsPremiumStyle)
                        codeReturn = InsertPremiumRestitutionEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, qty, true);
                    else
                    {
                        // EG 20130723 [18754]
                        // EG/PM 20130808 Add IsOption Test 
                        if (IsFuturesStyleMarkToMarket && IsOption)
                        {
                            codeReturn = InsertPremiumRestitutionEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, qty, false);
                            // EG 20131205 [19302] Incrément jeton + (voir property NbTokenIdE sur les posRequests Cancelation / Transfer )
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);
                            idE++;
                        }
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            codeReturn = InsertVariationMarginEvent(pDbTransaction, idT, idE, idE_Event, isDealerBuyer, Convert.ToDecimal(pRow["PRICE"]), qty, ClearingBusinessDate);
                        }
                    }
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);
                    #endregion EVENT PRM/VMG (Restitution)

                    // Insertion des frais
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        #region EVENT OPP (Restitution)
                        codeReturn = InsertReversalFeesEvents(pDbTransaction, pRow, pIdPADET, idE_Event, ref idE);
                        #endregion EVENT OPP (Restitution)
                    }
                }
                else
                {
                    // EG 20140409 [19807] 
                    // PM 20140603 [20036][20007] Ajout vérification qu'il s'agit d'une option
                    if (IsVariationMarginAuthorized)
                    {
                        idE++;
                        codeReturn = InsertVariationMarginEvent(pDbTransaction, idT, idE, idE_Event, IsTradeBuyer(pRow["SIDE"].ToString()),
                            Convert.ToDecimal(pRow["PRICE"]), qty, Convert.ToDateTime(pRow["DTBUSINESS"]));
                        // EG 20210413 [25584] Ajout insertion dans EVENTPOSACTIONDET
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                            codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, idE);
                    }
                }

            }

            pIdE = idE;
            return codeReturn;
        }
        #endregion InsertPosKeepingEvents
        #region InsertPremiumRestitutionEvent
        // EG 20130730 [18754] Add parameter (pIsPaymentSettlement)
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190730 Add TypePrice parameter
        protected override Cst.ErrLevel InsertPremiumRestitutionEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, 
            bool pIsDealerBuyer, decimal pQty, bool pIsPaymentSettlement)
        {

            #region Premium restitution calculation
            IExchangeTradedDerivative etd = (IExchangeTradedDerivative)m_TradeLibrary.Product;
            EFS_ETDPremium premium = etd.Efs_ExchangeTradedDerivative.premium;
            // EG 20140401 Prorata sur la base de la quantité de la restitution de la prime
            decimal amount = premium.RestitutionPremiumAmount(pQty);
            // Restitution de prime : le payeur est le seller
            int idA_Payer = (pIsDealerBuyer ? PosKeepingData.IdA_Clearer : PosKeepingData.IdA_Dealer);
            int idB_Payer = (pIsDealerBuyer ? PosKeepingData.IdB_Clearer : PosKeepingData.IdB_Dealer);
            int idA_Receiver = (pIsDealerBuyer ? PosKeepingData.IdA_Dealer : PosKeepingData.IdA_Clearer);
            int idB_Receiver = (pIsDealerBuyer ? PosKeepingData.IdB_Dealer : PosKeepingData.IdB_Clearer);
            #endregion Premium restitution calculation

            // EG 20121009 Replace LPP by LPC
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                EventCodeFunc.LinkedProductClosing, EventTypeFunc.Premium, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                amount, premium.PremiumCurrency, UnitTypeEnum.Currency.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

            // EG 20150616 [21124]
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, DtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, DtSettlement, pIsPaymentSettlement);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                decimal price = etd.TradeCaptureReport.LastPx.DecValue;
                decimal price100 = PosKeepingData.ToBase100(price);
                decimal contractMultiplier = PosKeepingData.Asset.contractMultiplier;
                // PM 20130807 [18876] for Variable Tick Value
                price100 = PosKeepingData.VariableContractValue(price100);
                codeReturn = m_EventQuery.InsertEventDet_Closing(pDbTransaction, pIdE, pQty, contractMultiplier, null, price, price100, null, null);
            }
            return codeReturn; ;
        }
        #endregion InsertPremiumRestitutionEvent
        #region InsertVariationMarginEvent
        /// <summary>
        /// Calcul et insertion d'un VMG dans un traitement EOD
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// ► Dénouement d'options
        ///   ● VMG = QTY DENOUEE * CONTRACTMULTIPLIER * COURS COMPENS[J]
        /// ► Correction de position / Transfert de position
        ///   ● VMG = QTY * CONTRACTMULTIPLIER * (COURS NEGO - COURS COMPENS[J-1])
        /// ► Décompensation
        ///   ● VMG = QTY * CONTRACTMULTIPLIER * (COURS OFFSETTING - COURS COMPENS[J-1])
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">IdT</param>
        /// <param name="pIdE">IdE</param>
        /// <param name="pIdE_Event">IdE parent</param>
        /// <param name="pIsDealerBuyer">Le dealer est le buyer</param>
        /// <param name="pPrice">Prix de négo</param>
        /// <param name="pQty">Quantité</param>
        /// <param name="pDtControl">Date offsetting (cas unclearing) ou ClearingBusinessDate (cas mise à jour de cloture à l'échéance)</param>
        /// <returns></returns>
        //PM 20140514 [19970][19259] Utilisation de PosKeepingData.asset.currency au lieu de PosKeepingData.asset.priceCurrency
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150128 [20726]
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        // EG 20190730 Add TypePrice parameter
        protected override Cst.ErrLevel InsertVariationMarginEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, bool pIsDealerBuyer,
            decimal pPrice, decimal pQty, DateTime pDtControl)
        {
            // EG 20141128 [20520] Nullable<decimal>
            Nullable<decimal> variationMargin = null;
            Quote_ETDAsset quote = null;
            SystemMSGInfo errReadOfficialClose = null;
            Quote_ETDAsset quoteVeil = null;
            SystemMSGInfo errReadOfficialCloseVeil = null;

            bool isEOD = (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);

            if (IsRequestPositionCancelation || IsRequestPositionTransfer || IsRequestUnclearing)
            {
                // EG 20140113 [19419] dtPreviousBusiness = DTMARKETPREV 
                // Cours de compensation VEILLE (QuotationSideEnum.OfficialClose)
                //DateTime dtPreviousBusiness = Tools.ApplyOffset(CS, m_Product, DtBusiness, PeriodEnum.D, -1, DayTypeEnum.ExchangeBusiness,
                //    PosKeepingData.asset.idBC, BusinessDayConventionEnum.PRECEDING);
                DateTime dtPreviousBusiness;

                #region PositionCancelation  - PositionTransfer - Unclearing
                if (PosKeepingData.MarketSpecified)
                    dtPreviousBusiness = PosKeepingData.Market.DtMarketPrev;
                else
                    dtPreviousBusiness = Tools.ApplyOffset(CS, m_Product, DtBusiness, PeriodEnum.D, -1, DayTypeEnum.ExchangeBusiness,
                        PosKeepingData.Asset.idBC, BusinessDayConventionEnum.PRECEDING, null);

                quoteVeil = m_PKGenProcess.ProcessCacheContainer.GetQuote(PosKeepingData.IdAsset, dtPreviousBusiness, PosKeepingData.Asset.identifier,
                    QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional(), ref errReadOfficialCloseVeil) as Quote_ETDAsset;
                if ((null == quoteVeil) || (false == quoteVeil.valueSpecified))
                {
                    if (null != errReadOfficialCloseVeil)
                    {
                        m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialCloseVeil.processState.Status);
                        
                        Logger.Log(errReadOfficialCloseVeil.ToLoggerData(0));
                    }
                }

                if ((null != quoteVeil) && quoteVeil.valueSpecified)
                {
                    if (IsRequestPositionCancelation || IsRequestPositionTransfer)
                    {
                        // VMG quantité corrigée/transférée entre le cours de clôture veille et le prix.
                        // Formule: QTY * MULT * (QUOTEveille - PRICE)
                        // PM 20130930 [19012] Invertion de la formule suite à erreur entre Buyer et Seller certainement due à la correction pour le ticket [18759]
                        // Formule: QTY * MULT * (PRICE - QUOTEveille)
                        //variationMargin = PosKeepingData.VariationMargin(pPrice, quoteVeil.value, pQty);
                        variationMargin = PosKeepingData.VariationMargin(quoteVeil.value, pPrice, pQty);
                    }
                    else if (IsRequestUnclearing)
                    {
                        // VMG quantité corrigée/transférée entre le cours de clôture veille et le cours de clôture de la compensation(clôture ) initiale.
                        quote = m_PKGenProcess.ProcessCacheContainer.GetQuote(PosKeepingData.IdAsset, pDtControl, PosKeepingData.Asset.identifier,
                            QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote_ETDAsset;
                        if ((null == quote) || (false == quote.valueSpecified))
                        {
                            if (null != errReadOfficialClose)
                            {
                                m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);
                                
                                Logger.Log(errReadOfficialClose.ToLoggerData(0));
                            }
                        }
                        if ((null != quote) && quote.valueSpecified)
                        {
                            // EG 20130615 [18759] Inversion quotation sur formule 
                            // Formule: QTY * MULT * (QUOTEveille - QUOTEClearing)
                            variationMargin = PosKeepingData.VariationMargin(quote.value, quoteVeil.value, pQty);
                        }
                    }
                }
                #endregion PositionCancelation  - PositionTransfer - Unclearing
            }
            else
            {
                // EG 20140204 [19587] Détermination de la date de lecture du cours
                //Pour les DC dont l’échéance B.O. est décalée à JREAL_MATURITY = JREAL_MATURITY + N
                // -	A partir de JREAL_MATURITY (inclus) jusqu’à JREAL_MATURITY (inclus) il n’y aura donc plus d’événement LPP\VMG de généré
                // -	A JREAL_MATURITY il y aura un événement LPC\VMG généré, incluant dernier VMG et VMG correctif donc calculé entre ZERO et « cours veille », 
                //      avec cours veille = cours JREAL_MATURTY - 1
                //Pour les DC dont l’échéance B.O. n’est pas décalée à 
                // -	A JREAL_MATURITY il y aura un événement LPC\VMG généré, incluant dernier VMG et VMG correctif donc calculé entre ZERO et « cours veille »
                // Cas particulier Négociation le jour de l'échéance (Cours négo se substitue à Cours veille)
                // EG 20141128 [20520] Nullable<decimal>
                Nullable<decimal> quoteValue = null;
                // EG 20140409 [19807] 
                if (((pDtControl.Date >= Asset.maturityDateSys)) && (Asset.maturityDateSys <= DtBusiness))
                {
                    // Cas particulier jour de négociation à l'échéance (Cours négo - 0)
                    variationMargin = PosKeepingData.VariationMargin(pPrice, 0, pQty);
                }
                else
                {
                    // EG 20140211 Pas de test sur trade jour
                    DateTime dtQuote = PosKeepingData.Asset.GetOfficialCloseQuoteTime(DtBusiness);
                    // EG 20120605 [19807] New
                    //if (m_ExchangeTradedDerivativeContainer.MaturityDateSys <= DtBusiness)
                    if (Asset.maturityDateSys <= DtBusiness)
                    {
                        // Lecture en Date veille de dtQuote (MaturityDateSys)
                        dtQuote = Tools.ApplyOffset(CS, m_Product, dtQuote, PeriodEnum.D, -1, DayTypeEnum.ExchangeBusiness, PosKeepingData.Asset.idBC, BusinessDayConventionEnum.PRECEDING, null);
                    }
                    quote = m_PKGenProcess.ProcessCacheContainer.GetQuote(PosKeepingData.IdAsset, dtQuote, PosKeepingData.Asset.identifier,
                        QuotationSideEnum.OfficialClose, Cst.UnderlyingAsset.ExchangeTradedContract, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote_ETDAsset;
                    if ((null == quote) || (false == quote.valueSpecified))
                    {
                        if ((null != errReadOfficialClose) && isEOD)
                        {
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, errReadOfficialClose.SysMsgCode, 0, errReadOfficialClose.LogParamDatas));
                        }
                    }
                    else
                        quoteValue = quote.value;

                    variationMargin = PosKeepingData.VariationMargin(quoteValue, 0, pQty);
                }
            }
            // EG 20150129 [20726]
            // EG 20141128 [20520] Nullable<decimal>
            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(PosKeepingData, EventTypeFunc.VariationMargin, pIsDealerBuyer);
            _payrec.SetPayerReceiver(variationMargin);
            // RD 20150311 [20831] Remplacer le deuxième argument _payrec.IdA_Receiver par _payrec.IdB_Receiver
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1,
                _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver,
                EventCodeFunc.LinkedProductClosing, EventTypeFunc.VariationMargin, DtBusiness, DtBusiness, DtBusiness, DtBusiness,
                variationMargin, PosKeepingData.Asset.currency, UnitTypeEnum.Currency.ToString(),
                variationMargin.HasValue ? StatusCalculEnum.CALC : StatusCalculEnum.TOCALC, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

            // EG 20150616 [21124]
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, DtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, DtSettlement, true);
            // RD 20120821 [18087] Add QUOTEDELTA
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                Nullable<decimal> quoteValue = null;
                Nullable<decimal> quoteDelta = null;
                if (null != quote)
                {
                    quoteValue = quote.value;
                    quoteDelta = quote.delta;
                }

                Nullable<decimal> quoteValueVeil = null;
                if (null != quoteVeil)
                    quoteValueVeil = quoteVeil.value;

                decimal price100 = PosKeepingData.ToBase100(pPrice);
                decimal? quoteValue100 = PosKeepingData.ToBase100(quoteValue);
                decimal? quoteValueVeil100 = PosKeepingData.ToBase100(quoteValueVeil);
                price100 = PosKeepingData.VariableContractValue(price100);
                quoteValue100 = PosKeepingData.VariableContractValue(quoteValue100);
                quoteValueVeil100 = PosKeepingData.VariableContractValue(quoteValueVeil100);

                codeReturn = m_EventQuery.InsertEventDet_Closing(pDbTransaction, pIdE, pQty, PosKeepingData.Asset.contractMultiplier,
                    null, null, null, pPrice, price100, null, quoteValue, quoteValue100, quoteDelta, quoteValueVeil, quoteValueVeil100, null, null);
            }
            return codeReturn;
        }
        #endregion InsertVariationMarginEvent

        #region MandatoryPosKeep_Calc
        /// <summary>
        /// Détermine si le recalcul de la tenue de position est nécessaire à l'arrivée d'un nouveau trade
        /// 
        /// L'entrée du trade est à traiter, car elle peut avoir un impact sur la tenue de position, si : 
        ///   ● Le trade est en CLOSE
        ///   ou
        ///   ● La méthode de tenue de position du BOOK est HILO
        ///   ou
        ///   ● Le trade n’a pas pris part à une clôture/compensation valide et postérieure à son entrée en portefeuille. 
        ///   ● Et le trade est en OPEN et il existe en portefeuille un trade de sens inverse en CLOSE et postérieure.
        /// </summary>
        /// <returns></returns>
        /// EG 20171016 [23509] DTEXECUTION remplace DTTIMESTAMP
        protected override bool MandatoryPosKeep_Calc(int pIdT)
        {
            //bool isMandatoryPosKeep_Calc = ("C" == m_PosKeepingData.trade.positionEffect);
            bool isMandatoryPosKeep_Calc = (!ExchangeTradedDerivativeTools.IsPositionEffect_Open(m_PosKeepingData.Trade.UltimatelyPositionEffect))
                || (ExchangeTradedDerivativeTools.IsPositionEffect_HILO(m_PosKeepingData.Trade.PositionEffect_BookDealer));

            if (!isMandatoryPosKeep_Calc)
            {
                isMandatoryPosKeep_Calc = base.MandatoryPosKeep_Calc(pIdT);

                //PL 20130319 Sauf erreur, le test "O" est déjà opéré implicitement ci-dessus
                //if (isMandatoryPosKeep_Calc && ("O" == m_PosKeepingData.trade.positionEffect))
                if (isMandatoryPosKeep_Calc)
                {
                    if (m_PosKeepingData.Trade.IsIgnorePositionEffect_BookDealer && !ExchangeTradedDerivativeTools.IsPositionEffect_Open(m_PosKeepingData.Trade.PositionEffect_BookDealer))
                    {
                        //Tous les trades sont à considérer en tant que "Close" --> De fait le recalcul de la tenue de position s'impose.
                        isMandatoryPosKeep_Calc = true;
                    }
                    else
                    {
                        DataParameters parameters = new DataParameters(new DataParameter[] { });
                        parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
                        parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), m_PosKeepingData.IdI);
                        parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), m_PosKeepingData.IdAsset);
                        parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), m_PosKeepingData.IdA_Dealer);
                        parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), m_PosKeepingData.IdB_Dealer);
                        parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), m_PosKeepingData.IdA_Clearer);
                        parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), m_PosKeepingData.IdB_Clearer);
                        parameters.Add(new DataParameter(CS, "SIDE", DbType.Int32), m_PosKeepingData.Trade.Side);
                        // EG 20171016 [23509] DTEXECUTION remplace DTTIMESTAMP
                        parameters.Add(new DataParameter(CS, "DTEXECUTION", DbType.DateTime2), m_PosKeepingData.Trade.DtExecution);

                        string sqlSelect = GetQueryMandatoryPosKeep_Calc_Cond2();
                        object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
                        isMandatoryPosKeep_Calc = (null != obj) && BoolFunc.IsTrue(obj);
                    }
                }
            }

            return isMandatoryPosKeep_Calc;
        }
        #endregion MandatoryPosKeep_Calc
        #region MaturityOffsettingFutureGen
        /// <summary>
        /// CLOTURE AUTOMATIQUE DES FUTURES ARRIVANT A ECHEANCE (POUR UNE CLE DE POSITION)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20120327 Méthode InitPaymentForEvent déplacée dans Trade.cs de Process
        // EG 20140225 [19575][19666]
        // EG 20140711 (New - Override)
        // EG 20160106 [21679][POC-MUREX]
        // EG 20160208 [POC-MUREX] Refactoring (Alimentation List<MOF_Trade> lstTrades)
        // EG 20170206 [22879] Replace List<MOF_Trade> by List<EOD_Trade>
        // PL 20170413 [23064] Modify 
        // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
        // EG 20190730 Add TypePrice parameter
        // EG 20190926 Refactoring Cst.PosRequestTypeEnum
        protected override Cst.ErrLevel MaturityOffsettingFutureGen(IPosRequest pPosRequestMOF)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                QueryParameters qryParameters = GetQueryTradeCandidatesToMaturityOffsettingFuture(pPosRequestMOF.DtBusiness, pPosRequestMOF.PosKeepingKey);
                // PL 20180312 WARNING: Use Read Commited !                
                //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
                DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);

                if (null != ds && ArrFunc.IsFilled(ds.Tables[0].Rows))
                {
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    //PL 20170413 [23064]
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    string unitOfMeasure = string.Empty;
                    bool isPayment = false;
                    bool isGenerateDVA = false; 
                    bool isGenerateDLV = (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
                                      && (Asset.physicalSettlementAmount != PhysicalSettlementAmountEnum.NA);
                    if (isGenerateDLV)
                    {
                        isGenerateDVA = GenerateDVA(ref isPayment);
                        unitOfMeasure = GetUnitOfMeasure();
                    }
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+                   
                    
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
                        trade.Currency = Convert.ToString(dr["PRICECURRENCY"]); //PL 20170413 Add for DVA
                        if (trade.IsDealerBuyer)
                            trade.IdT_Buy = trade.IdT;
                        else
                            trade.IdT_Sell = trade.IdT;
                        #endregion Initialisation Variables

                        #region Calcul des éventuels frais de liquidation
                        trade.Fees = CalcMaturityOffsettingFuture(CS, trade.IdT, trade.Qty, m_MasterPosRequest.DtBusiness, m_PKGenProcess.Session);
                        #endregion

                        trade.NbEvent = pPosRequestMOF.NbTokenIdE;

                        if (ArrFunc.IsFilled(trade.Fees))
                        {
                            DeserializeTrade(trade.IdT);

                            ExchangeTradedDerivativeContainer.Efs_ExchangeTradedDerivative =
                                new EFS_ExchangeTradedDerivative(CS, ExchangeTradedDerivativeContainer.ExchangeTradedDerivative, m_TradeLibrary.DataDocument,
                                    Cst.StatusBusiness.ALLOC, trade.IdT);

                            EventQuery.InitPaymentForEvent(CS, trade.Fees, m_TradeLibrary.DataDocument, out int nbFeesEvent);
                            trade.NbEvent += nbFeesEvent;
                        }

                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        //PL 20170413 [23064]
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        if (isGenerateDLV)
                        {
                            trade.NbEvent += (isGenerateDVA ? 3 : 2);
                        }
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        
                        lstTrades.Add(trade);
                    }

                    if (0 < lstTrades.Count)
                    {
                        Nullable<QuoteTimingEnum> quoteTiming = null;
                        Nullable<QuotationSideEnum> quoteSide = null;
                        Nullable<decimal> settlementPrice = null;
                        Nullable<decimal> settlementPrice100 = null;
                        Nullable<DateTime> dtQuote = null;

                        if (isGenerateDLV)
                        {
                            // SettlementPrice
                            Cst.ErrLevel errLevel = GetQuoteOfAsset(ref dtQuote, ref quoteTiming, ref quoteSide, ref settlementPrice, ref settlementPrice100);
                        }

                        DateTime deliveryDate = GetDeliveryDate(DtBusiness);
                        Nullable<decimal> closingPrice = PosKeepingData.Asset.quote.quotePrice;

                        dbTransaction = DataHelper.BeginTran(CS);

                        #region GetId of POSACTION/POSACTIONDET
                        // EG 20160106 [21679][POC-MUREX]
                        SQLUP.GetId(out int newIdPA, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);

                        // EG 20160106 [21679][POC-MUREX]
                        //int newIdPADET = 0;
                        //SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTIONDET, SQLUP.PosRetGetId.First, lstTrades.Count);
                        int newIdPADET = newIdPA;

                        #endregion GetId of POSACTION/POSACTIONDET
                        #region Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS
                        #endregion GetId of POSACTION/POSACTIONDET

                        #region Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS
                        SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, lstTrades.Sum(trade => trade.NbEvent));

                        #region Insertion dans POSACTION
                        InsertPosAction(dbTransaction, newIdPA, DtBusiness, pPosRequestMOF.IdPR);
                        #endregion Insertion dans POSACTION

                        lstTrades.ForEach(trade =>
                        {
                            if (false == isException)
                            {
                                #region POSACTIONDET
                                // EG 20141205 null for PositionEffect
                                InsertPosActionDet(dbTransaction, newIdPA, newIdPADET, trade.IdT_Buy, trade.IdT_Sell, trade.IdT, trade.Qty, null);
                                #endregion POSACTIONDET

                                #region EVENT MOF|TOT (Maturity OffSetting)

                                int idE_Parent = trade.IdE_Event;
                                int idE_MaturityOffSetting = newIdE;

                                //Event MOF|TOT
                                m_EventQuery.InsertEvent(dbTransaction, trade.IdT, idE_MaturityOffSetting, idE_Parent, null, 1, 1, null, null, null, null,
                                        EventCodeFunc.MaturityOffsettingFuture, EventTypeFunc.Total,
                                        DtBusiness, DtBusiness, deliveryDate, deliveryDate, trade.Qty, null, UnitTypeEnum.Qty.ToString(), null, null);

                                //EventClass
                                m_EventQuery.InsertEventClass(dbTransaction, idE_MaturityOffSetting,
                                        (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement) ? EventClassFunc.PhysicalSettlement : EventClassFunc.CashSettlement, DtBusiness, false);

                                // Insertion d'une Date DLV sur MOF
                                // EG 20110901 Ticket 17428 
                                if (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
                                    codeReturn = m_EventQuery.InsertEventClass(dbTransaction, idE_MaturityOffSetting, EventClassFunc.DeliveryDelay, deliveryDate, false);

                                //PosActionDet
                                EventQuery.InsertEventPosActionDet(dbTransaction, newIdPADET, idE_MaturityOffSetting);

                                InsertEventDet(dbTransaction, PosKeepingData, idE_MaturityOffSetting, trade.Qty, Asset.contractMultiplier, null, trade.Price, closingPrice);

                                newIdE++;

                                #endregion EVENT MOF|TOT (Maturity OffSetting)

                                #region EVENT NOM/QTY (Nominal/Quantité)
                                codeReturn = InsertNominalQuantityEvent(dbTransaction, trade.IdT, ref newIdE, idE_MaturityOffSetting, DtBusiness, trade.IsDealerBuyer, trade.Qty, 0, newIdPADET);
                                newIdE++;
                                #endregion EVENT NOM/QTY (Nominal/Quantité)

                                #region EVENT RMG (Realized margin)
                                codeReturn = InsertRealizedMarginEvent(dbTransaction, trade.IdT, ref newIdE, idE_MaturityOffSetting, trade.IsDealerBuyer, trade.Price, closingPrice, trade.Qty, newIdPADET, true);
                                #endregion EVENT RMG (Realized margin)

                                #region Insert des éventuels frais de liquidation
                                
                                if (ArrFunc.IsFilled(trade.Fees))
                                {
                                    foreach (IPayment payment in trade.Fees)
                                    {
                                        newIdE++;
                                        int savIdE = newIdE;
                                        codeReturn = m_EventQuery.InsertPaymentEvents(dbTransaction, m_TradeLibrary.DataDocument, trade.IdT, payment, DtBusiness, 1, 1, ref newIdE, idE_MaturityOffSetting);
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

                                #endregion Insert des éventuels frais de liquidation

                                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                //PL 20170413 [23064]
                                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                // EG 20170424 [23064] Add IsQuoteOk
                                if (isGenerateDLV && IsQuoteOk)
                                {
                                    #region EVENT DLV|PHY (Physical Delivery)

                                    // Event DLV|PHY 
                                    decimal totalDeliveryQty = trade.Qty * Asset.unitOfMeasureQty;
                                    idE_Parent = trade.IdE_Event;
                                    int idE_Delivery = newIdE;

                                    codeReturn = InsertPhysicalDeliveryEvent(dbTransaction, trade.IdT, idE_Delivery, idE_Parent, deliveryDate, 
                                        trade.Qty, totalDeliveryQty, unitOfMeasure, dtQuote, quoteSide, quoteTiming, settlementPrice, settlementPrice100, newIdPADET);

                                    newIdE++;

                                    #endregion EVENT DLV|PHY  (Physical Delivery)

                                    #region EVENT TER|DVA|REC-STL
                                    // Event (DVA)
                                    decimal settlementQty = trade.Qty * Asset.unitOfMeasureQty;
                                    idE_Parent = idE_Delivery;
                                    int idE_Settlement = newIdE;

                                    if (isGenerateDVA)
                                    {
                                        codeReturn = InsertPhysicalDeliveryAmountEvent(dbTransaction, trade.IdT, idE_Settlement, idE_Parent, deliveryDate, 
                                            settlementQty, unitOfMeasure, trade.Currency, trade.IsDealerBuyer, isPayment, dtQuote, quoteSide, quoteTiming, settlementPrice,settlementPrice100, newIdPADET);
                                        newIdE++;
                                    }

                                    #endregion EVENT TER|DVA|REC-STL

                                    #region EVENT TER|QTY|REC-STL

                                    // Event (QTY)
                                    decimal deliveryQuantity = trade.Qty * Asset.unitOfMeasureQty;
                                    idE_Parent = (isGenerateDVA ? idE_Settlement : idE_Delivery);
                                    int idE_Quantity = newIdE;

                                    codeReturn = InsertPhysicalDeliveryQuantityEvent(dbTransaction, trade.IdT, idE_Quantity, idE_Parent, deliveryDate, deliveryQuantity, unitOfMeasure,
                                         trade.IsDealerBuyer, newIdPADET);

                                    newIdE++;

                                    #endregion EVENT TER|QTY|REC-STL
                                }

                                //newIdPADET++;
                                SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);

                                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
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
        #endregion MaturityOffsettingFutureGen

        #region PhysicalPeriodicDeliveryGen
        /// <summary>
        /// Calcul des périodes de livraison et paiements relatives à un contrat Future
        /// . Calcul des périodes (livraison / Paiement) par DC
        /// . Génération des événements relatifs à ces périodes par trade en position
        /// </summary>
        // EG 20170206 [22787] New 
        // PL 20170411 [23064] Modify 
        // EG 20170424 [23064] Gestion des livraisons périodiques sur Late Trade, Open position ou post mise à jour de clôture.
        protected override Cst.ErrLevel PhysicalPeriodicDeliveryGen(IPosRequest pPosRequestPDP)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                QueryParameters qryParameters = GetQueryTradeCandidatesToPhysicalPeriodicDelivery(pPosRequestPDP.DtBusiness, pPosRequestPDP.PosKeepingKey);
                // PL 20180312 WARNING: Use Read Commited !                
                //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
                DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);

                if (null != ds && ArrFunc.IsFilled(ds.Tables[0].Rows))
                {
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    //PL 20170411 [23064]
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    bool isPayment = false;
                    bool isGenerateDVA = GenerateDVA(ref isPayment); 
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    string unitOfMeasure = GetUnitOfMeasure(); 

                    List<EOD_Trade> lstTrades = new List<EOD_Trade>();
                    int nbRow = ds.Tables[0].Rows.Count;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        EOD_Trade trade = new EOD_Trade();
                        #region Initialisation Variables
                        trade.IdT = Convert.ToInt32(dr["IDT"]);
                        trade.Qty = Convert.ToDecimal(dr["QTY"]);
                        trade.IdE_Event = Convert.ToInt32(dr["IDE_EVENT"]);
                        trade.IsDealerBuyer = IsTradeBuyer(dr["SIDE"].ToString());
                        trade.Currency = Convert.ToString(dr["PRICECURRENCY"]);
                        trade.NbEvent = pPosRequestPDP.NbTokenIdE;

                        trade.DeliveryQtySpecified = (false == Convert.IsDBNull(dr["DELIVERYQTY"]));
                        if (trade.DeliveryQtySpecified)
                            trade.DeliveryQty = Convert.ToDecimal(dr["DELIVERYQTY"]);
                        #endregion Initialisation Variables

                        lstTrades.Add(trade);
                    }

                    // EG 20170424 [23064] Filtre sur les trades candidats :
                    // soit : Pas encore de livraison périodique générée ou livraison déjà opérée pour la même quantité.
                    List<EOD_Trade> lstTradesCandidates = new List<EOD_Trade>();
                    if (0 < lstTrades.Count)
                        lstTradesCandidates = lstTrades.Where(trade => (false == trade.DeliveryQtySpecified) || (trade.DeliveryQty != trade.Qty)).ToList();

                    if (0 < lstTradesCandidates.Count)
                    {

                        Nullable<QuoteTimingEnum> quoteTiming = null;
                        Nullable<QuotationSideEnum> quoteSide = null;
                        Nullable<decimal> settlementPrice = null;
                        Nullable<decimal> settlementPrice100 = null;
                        Nullable<DateTime> dtQuote = null;

                        // Calcul des périodes de livraison par contrat
                        Dictionary<DateTime, List<EFS_DeliveryPeriod>> dicDeliveryPeriods = PosKeepingData.Asset.GetDeliveryPeriods(CS, m_Product, Asset.isApplySummertime);
                        if ((null != dicDeliveryPeriods) && (0 < dicDeliveryPeriods.Keys.Count))
                        {
                            bool isOneDayPeriod = (Asset.deliveryPeriodFrequency.Interval.Period == PeriodEnum.D) &&
                                (Asset.deliveryPeriodFrequency.Interval.PeriodMultiplier.IntValue == 1);

                            // Détermination de la période totale de livraison
                            string deliveryTimeZone = dicDeliveryPeriods.Values.First().First().timeZone;
                            DateTime fisrtDeliveryDate = dicDeliveryPeriods.Values.First().First().deliveryDateStart.Date;
                            DateTime lastDeliveryDate = dicDeliveryPeriods.Values.Last().Last().deliveryDateEnd.Date;
                            if (isOneDayPeriod)
                                lastDeliveryDate = dicDeliveryPeriods.Values.Last().Last().deliveryDateStart.Date;

                            DateTimeOffset deliveryDateStart = dicDeliveryPeriods.Values.First().First().deliveryDateStart;
                            DateTimeOffset deliveryDateEnd = dicDeliveryPeriods.Values.Last().Last().deliveryDateEnd;
                            dbTransaction = DataHelper.BeginTran(CS);

                            //int nbToken = lstTrades.Count + (lstTrades.Sum(trade => trade.nbEvent) * dicDeliveryPeriods.Keys.Count);
                            //PL 20170412 TBD: Exploiter plus bas isGenerateDVA=false pour essayer ici de restreindre le nombre de jeton demandé...
                            int nbToken = lstTradesCandidates.Count + (lstTradesCandidates.Sum(trade => trade.NbEvent) * dicDeliveryPeriods.Values.Sum(period => period.Count));
                            SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbToken);

                            lstTradesCandidates.ForEach(trade =>
                            {
                                if (false == isException)
                                {
                                    // Suppression des événements générés dans un précédent traitement
                                    // EG 20170424 Test sur trade.deliveryQtySpecified
                                    if (trade.DeliveryQtySpecified)
                                        DeleteEventPhysicalDeliveryPayment(dbTransaction, trade.IdT);

                                    Cst.ErrLevel errLevel = GetQuoteOfAsset(ref dtQuote, ref quoteTiming, ref quoteSide, ref settlementPrice, ref settlementPrice100, pPosRequestPDP);
                                    if (IsQuoteOk)
                                    {
                                        #region EVENT DLV|PHY (Physical Delivery)

                                        // Event Period = first and lastDeliveryDate
                                        // Total heures sur les périodes
                                        decimal totalDeliveryQty = trade.Qty * dicDeliveryPeriods.Values.Sum(lstPeriod => lstPeriod.Sum(period => period.deliveryHours));
                                        totalDeliveryQty *= Asset.unitOfMeasureQty;

                                        // Event DLV|PHY 
                                        int idE_Parent = trade.IdE_Event;
                                        int idE_Delivery = newIdE;

                                        codeReturn = InsertPhysicalDeliveryEvent(dbTransaction, trade.IdT, idE_Delivery, idE_Parent,
                                            fisrtDeliveryDate, fisrtDeliveryDate, lastDeliveryDate, lastDeliveryDate, deliveryTimeZone,
                                            trade.Qty, totalDeliveryQty, unitOfMeasure,
                                            dtQuote, quoteSide, quoteTiming, settlementPrice, settlementPrice100, null);

                                        newIdE++;

                                        #endregion EVENT DLV|PHY  (Physical Delivery)

                                        //DealerSeller --> Receiver de DVA
                                        DateTime startPeriod = DateTime.MinValue;
                                        DateTime endPeriod = DateTime.MaxValue;

                                        dicDeliveryPeriods.ToList().ForEach(settlement =>
                                        {
                                            startPeriod = settlement.Value.First().StartPeriod;
                                            endPeriod = settlement.Value.Last().EndPeriod;
                                            deliveryDateStart = settlement.Value.First().deliveryDateStart;
                                            deliveryDateEnd = settlement.Value.Last().deliveryDateEnd;

                                            //NB: si mono-période on retiend TER
                                            string eventCode = (startPeriod == lastDeliveryDate ? EventCodeFunc.Termination :
                                                                            (startPeriod == fisrtDeliveryDate ? EventCodeFunc.Start : EventCodeFunc.Intermediary));

                                            #region EVENT STA/INT/TER|DVA|REC-STL

                                            // Event (DVA)
                                            decimal settlementQty = settlement.Value.Sum(period => period.deliveryHours * trade.Qty * Asset.unitOfMeasureQty);
                                            idE_Parent = idE_Delivery;
                                            int idE_Settlement = newIdE;

                                            if (isGenerateDVA)
                                            {
                                                string eventType = EventTypeFunc.DeliveryAmount;
                                                if (DtBusiness > settlement.Key)
                                                    eventType = EventTypeFunc.HistoricalDeliveryAmount;

                                                codeReturn = InsertPhysicalDeliveryAmountEvent(dbTransaction, trade.IdT, idE_Settlement, idE_Parent,
                                                    eventCode, eventType,
                                                    settlement.Key, startPeriod, endPeriod, deliveryDateStart.UtcDateTime, deliveryDateEnd.UtcDateTime,
                                                    deliveryTimeZone, settlementQty, unitOfMeasure, trade.Currency, trade.IsDealerBuyer, isPayment, false,
                                                    dtQuote, quoteSide, quoteTiming, settlementPrice, settlementPrice100, null);

                                                newIdE++;
                                            }

                                            #endregion EVENT STA/INT/TER|DVA|REC-STL

                                            settlement.Value.ForEach(period =>
                                            {
                                                decimal deliveryQuantity = (trade.Qty * period.deliveryHours * Asset.unitOfMeasureQty);

                                                #region EVENT STA/INT/TER|QTY|REC-STL

                                                // Event (QTY)
                                                idE_Parent = (isGenerateDVA ? idE_Settlement : idE_Delivery);
                                                int idE_Quantity = newIdE;

                                                codeReturn = InsertPhysicalDeliveryQuantityEvent(dbTransaction, trade.IdT, idE_Quantity, idE_Parent,
                                                    eventCode, period.StartPeriod, period.StartPeriod, period.EndPeriod,
                                                    period.deliveryDateStart.UtcDateTime, period.deliveryDateEnd.UtcDateTime, period.timeZone, 
                                                    deliveryQuantity, unitOfMeasure, trade.IsDealerBuyer, null);

                                                newIdE++;

                                                #endregion EVENT STA/INT/TER|QTY|REC-STL
                                            });
                                        });
                                    }
                                }
                            });

                            if ((null != dbTransaction) && (false == isException))
                                DataHelper.CommitTran(dbTransaction);
                        }
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
        #endregion PhysicalPeriodicDeliveryGen

        #region RequestPositionOptionGen
        /// <summary>
        /// Traitement d'un denoument d'option manuel d'une position activé via un message Mqueue
        /// </summary>
        /// <returns></returns>
        /// FI 20130917 [18953] Add Method
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel RequestPositionOptionGen()
        {
            PosRequestPositionSetIdentifiers(CS, m_PosRequest);

            // RD 20210908 [25803] Le message LOG-05063 ne s'attend pas au paramètre m_PosRequest.groupProductValue, en plus il est vide.
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5063), 0,
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(m_PosRequest.RequestType),
                new LogParam(m_PosRequest.RequestMode),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Dealer, m_PosRequest.PosKeepingKey.IdA_Dealer)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.BookDealer, m_PosRequest.PosKeepingKey.IdB_Dealer)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Clearer, m_PosRequest.PosKeepingKey.IdA_Clearer)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.BookClearer, m_PosRequest.PosKeepingKey.IdB_Clearer)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Asset, m_PosRequest.PosKeepingKey.IdAsset)),
                new LogParam(m_PosRequest.Qty)));

            // RD 20150924 [21296] Initialisation de m_MarketPosRequest 
            if (m_MarketPosRequest == null)
            {
                m_MarketPosRequest = m_Product.CreatePosRequest();
                m_MarketPosRequest.IdentifiersSpecified = true;
                m_MarketPosRequest.Identifiers = m_Product.CreatePosRequestKeyIdentifier();
                m_MarketPosRequest.Identifiers.Entity = Queue.GetStringValueIdInfoByKey("ENTITY");
                m_MarketPosRequest.Identifiers.CssCustodian = Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN");
                m_MarketPosRequest.Identifiers.Market = m_PosRequest.Identifiers.Market;
                m_MarketPosRequest.IdMSpecified = true;
                m_MarketPosRequest.IdM = m_PosRequest.IdM;
            }

            Cst.ErrLevel codeReturn = PositionOptionGen(m_PosRequest.IdPR, null);
            return codeReturn;
        }
        #endregion RequestPositionOptionGen
        #region RequestTradeOptionGen
        /// <summary>
        /// Traitement d'un denoument d'option manuel d'un trade activé via un message Mqueue
        /// </summary>
        /// FI 20130917 [18953] Add Method
        protected override Cst.ErrLevel RequestTradeOptionGen()
        {
            return TradeOptionGen(null, true);
        }
        #endregion RequestTradeOptionGen

        #region SetPaymentFeesForEvent
        /// <summary>
        /// Remplacement de la quantité d'origine du trade par la quantité dénouée ou corrigée (UTILISE pour la restitution de prime)
        /// </summary>
        /// <param name="pIdT">Id du trade clôturant</param>
        /// <param name="pQty">Quantité clôturée</param>
        /// <param name="pPaymentFees">Frais (pour restitution)</param>
        /// <param name="pDetail">PosRequestDetail</param>
        // EG 20140711 (New - Override)
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public override void SetPaymentFeesForEvent(int pIdT, decimal pQty, IPayment[] pPaymentFees, IPosRequestDetail pDetail)
        {
            if ((null != ExchangeTradedDerivativeContainer) && (null != ExchangeTradedDerivativeContainer.TradeCaptureReport))
            {
                ExchangeTradedDerivativeContainer.TradeCaptureReport.LastQty = new EFS_Decimal(pQty);
                ExchangeTradedDerivativeContainer.Efs_ExchangeTradedDerivative = new EFS_ExchangeTradedDerivative(CS,
                    ExchangeTradedDerivativeContainer.ExchangeTradedDerivative, m_TradeLibrary.DataDocument,
                    Cst.StatusBusiness.ALLOC, pIdT);

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
        /// EG 20141013 Gstion des tous les type d'asset
        /// EG 20141014 PosKeepingAsset remplace PosKeepingAsset_ETD
        /// EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
        /// PM 20141120 [20508] Ajout gestion de cashFlowCalculationMethod
        /// EG 20170206 [22787] Add gestion FIRSTDELIVERYDATE, LASTDELIVERYDATE, etc.
        /// FI 20170303 [22916] Modify 
        /// PL 20170411 [23064] Add column PHYSETTLTAMOUNT
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180221 [23769] Public pour gestion asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20181203 [24360] Add parameter pIsCA (= true pour le traitement des CA (pas de dbTransaction sur Sql_Table - un dataReader est déjà ouvert sur la transaction propre à la CA)
        // EG 20181205 [24360] Refactoring
        public override PosKeepingAsset SetPosKeepingAsset(IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, MapDataReaderRow pMapDr)
        {
            PosKeepingAsset asset = base.SetPosKeepingAsset(pDbTransaction, pUnderlyingAsset, pMapDr);
            if (null != asset)
            {
                if (asset is PosKeepingAsset_ETD)
                {
                    PosKeepingAsset_ETD assetETD = asset as PosKeepingAsset_ETD;
                    assetETD.maturityDate = Convert.IsDBNull(pMapDr["MATURITYDATE"].Value) ? DateTime.MaxValue.Date : Convert.ToDateTime(pMapDr["MATURITYDATE"].Value);
                    assetETD.strikePrice = Convert.IsDBNull(pMapDr["STRIKEPRICE"].Value) ? 0 : Convert.ToDecimal(pMapDr["STRIKEPRICE"].Value);
                    assetETD.instrumentNum = Convert.ToInt32(pMapDr["INSTRUMENTNUM"].Value);
                    assetETD.instrumentDen = Convert.ToInt32(pMapDr["INSTRUMENTDEN"].Value);
                    assetETD.putCallSpecified = (false == Convert.IsDBNull(pMapDr["PUTCALL"].Value));
                    if (assetETD.putCallSpecified)
                        assetETD.putCall = (PutOrCallEnum)ReflectionTools.EnumParse(assetETD.putCall, pMapDr["PUTCALL"].Value.ToString());

                    assetETD.assetCategorySpecified = (false == Convert.IsDBNull(pMapDr["ASSETCATEGORY"].Value));
                    if (assetETD.assetCategorySpecified)
                        assetETD.assetCategory = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), pMapDr["ASSETCATEGORY"].Value.ToString());
                    assetETD.factor = Convert.IsDBNull(pMapDr["FACTOR"].Value) ? 0 : Convert.ToDecimal(pMapDr["FACTOR"].Value);

                    // SettltMethod
                    assetETD.settlMethod = (SettlMethodEnum)ReflectionTools.EnumParse(assetETD.settlMethod, pMapDr["SETTLTMETHOD"].Value.ToString());
                    assetETD.physicalSettlementAmount = (PhysicalSettlementAmountEnum)ReflectionTools.EnumParse(assetETD.physicalSettlementAmount, pMapDr["PHYSETTLTAMOUNT"].Value.ToString());

                    // DeliveryDate
                    assetETD.deliveryDateSpecified = (false == Convert.IsDBNull(pMapDr["DELIVERYDATE"].Value));
                    if (assetETD.deliveryDateSpecified)
                        assetETD.deliveryDate = Convert.ToDateTime(pMapDr["DELIVERYDATE"].Value);
                    // DeliveryOffset
                    assetETD.deliveryDelayOffsetSpecified = (false == Convert.IsDBNull(pMapDr["PERIODMLTPDELIVERYDATEOFFSET"].Value));
                    assetETD.deliveryDelayOffsetSpecified &= (false == Convert.IsDBNull(pMapDr["PERIODDELIVERYDATEOFFSET"].Value));
                    assetETD.deliveryDelayOffsetSpecified &= (false == Convert.IsDBNull(pMapDr["DAYTYPEDELIVERYDATEOFFSET"].Value));
                    if (assetETD.deliveryDelayOffsetSpecified)
                    {
                        PeriodEnum period = StringToEnum.Period(Convert.ToString(pMapDr["PERIODDELIVERYDATEOFFSET"].Value));
                        int multiplier = Convert.ToInt32(pMapDr["PERIODMLTPDELIVERYDATEOFFSET"].Value);
                        DayTypeEnum dayType = StringToEnum.DayType(Convert.ToString(pMapDr["DAYTYPEDELIVERYDATEOFFSET"].Value));
                        assetETD.deliveryDelayOffset = m_Product.CreateOffset(period, multiplier, dayType);
                    }

                    // EG 20130603 FinalSettlementPrice
                    assetETD.lastTradingDay = Convert.IsDBNull(pMapDr["LASTTRADINGDAY"].Value) ? assetETD.maturityDate : Convert.ToDateTime(pMapDr["LASTTRADINGDAY"].Value);
                    assetETD.finalSettlementPrice = FinalSettlementPriceEnum.ExpiryDate;
                    if (false == Convert.IsDBNull(pMapDr["FINALSETTLTPRICE"].Value))
                        assetETD.finalSettlementPrice = (FinalSettlementPriceEnum)Enum.Parse(typeof(FinalSettlementPriceEnum), pMapDr["FINALSETTLTPRICE"].Value.ToString());

                    // PM 20130807 [18876] PriceQuoteMethod for Variable Tick Value
                    assetETD.priceQuoteMethod = (PriceQuoteMethodEnum)ReflectionTools.EnumParse(assetETD.priceQuoteMethod, pMapDr["PRICEQUOTEMETHOD"].Value.ToString());

                    /// EG 20140115 [19456] Add MATURITYDATESYS, EXERCISESTYLE, FINALSETTLTSIDE, FINALSETTLTTIME
                    /// EG 20140121 Homologuation
                    assetETD.maturityDateSys = Convert.IsDBNull(pMapDr["MATURITYDATESYS"].Value) ? assetETD.maturityDate : Convert.ToDateTime(pMapDr["MATURITYDATESYS"].Value);
                    assetETD.category = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(assetETD.category, pMapDr["CATEGORY"].Value.ToString());
                    assetETD.exerciseStyleSpecified = (false == Convert.IsDBNull(pMapDr["EXERCISESTYLE"].Value));
                    if (assetETD.exerciseStyleSpecified)
                        assetETD.exerciseStyle = (DerivativeExerciseStyleEnum)Enum.Parse(typeof(DerivativeExerciseStyleEnum), pMapDr["EXERCISESTYLE"].Value.ToString());

                    // FI 20170303 [22916] affectation de inTheMoneyCondition
                    assetETD.inTheMoneyCondition = ITMConditionEnum.OTM;
                    if (false == Convert.IsDBNull(pMapDr["ITMCONDITION"].Value))
                        assetETD.inTheMoneyCondition = (ITMConditionEnum)ReflectionTools.EnumParse(assetETD.inTheMoneyCondition, pMapDr["ITMCONDITION"].Value.ToString());
                    
                    assetETD.finalSettlementSideSpecified = (false == Convert.IsDBNull(pMapDr["FINALSETTLTSIDE"].Value));
                    if (assetETD.finalSettlementSideSpecified)
                        assetETD.finalSettlementSide = (QuotationSideEnum)Enum.Parse(typeof(QuotationSideEnum), pMapDr["FINALSETTLTSIDE"].Value.ToString());
                    assetETD.finalSettlementTimeSpecified = (false == Convert.IsDBNull(pMapDr["FINALSETTLTTIME"].Value));
                    if (assetETD.finalSettlementTimeSpecified)
                        assetETD.finalSettlementTime = pMapDr["FINALSETTLTTIME"].Value.ToString();


                    // EG 20140326 [19771][19785] 
                    assetETD.precedingMaturityDateSysSpecified = assetETD.exerciseStyleSpecified && (assetETD.exerciseStyle == DerivativeExerciseStyleEnum.European);
                    if (assetETD.precedingMaturityDateSysSpecified)
                    {
                        IBusinessDayAdjustments _bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, asset.idBC);
                        IOffset _offset = m_Product.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.Business);
                        assetETD.precedingMaturityDateSys = Tools.ApplyOffset(CS, assetETD.maturityDate, _offset, _bda, null);
                    }

                    // EG 20120605 [19807] New 
                    assetETD.valuationMethod = (FuturesValuationMethodEnum)ReflectionTools.EnumParse(assetETD.valuationMethod, pMapDr["FUTVALUATIONMETHOD"].Value.ToString());

                    // Underlyer
                    if (null != pMapDr["IDASSET_UNDERLYER"])
                    {
                        // Id UNL
                        assetETD.idAsset_UnderlyerSpecified = (false == Convert.IsDBNull(pMapDr["IDASSET_UNDERLYER"].Value));
                        if (assetETD.idAsset_UnderlyerSpecified)
                            assetETD.idAsset_Underlyer = Convert.ToInt32(pMapDr["IDASSET_UNDERLYER"].Value);
                        // Identifier UNL
                        assetETD.identifier_UnderlyerSpecified = (false == Convert.IsDBNull(pMapDr["IDENTIFIER_UNDERLYER"].Value));
                        if (assetETD.identifier_UnderlyerSpecified)
                            assetETD.identifier_Underlyer = pMapDr["IDENTIFIER_UNDERLYER"].Value.ToString();
                        // Id DC UNL
                        assetETD.idDC_UnderlyerSpecified = (false == Convert.IsDBNull(pMapDr["IDDC_UNL"].Value));
                        if (assetETD.idDC_UnderlyerSpecified)
                            assetETD.idDC_Underlyer = Convert.ToInt32(pMapDr["IDDC_UNL"].Value);

                        assetETD.assetCategory_UnderlyerSpecified = (false == Convert.IsDBNull(pMapDr["ASSETCATEGORY_UNDERLYER"].Value));
                        if (assetETD.assetCategory_UnderlyerSpecified)
                            assetETD.assetCategory_Underlyer = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), pMapDr["ASSETCATEGORY_UNDERLYER"].Value.ToString());

                        if (assetETD.idDC_UnderlyerSpecified && assetETD.assetCategory_UnderlyerSpecified &&
                            ((assetETD.assetCategory_Underlyer == Cst.UnderlyingAsset.Future) || (assetETD.assetCategory_Underlyer == Cst.UnderlyingAsset.ExchangeTradedContract)))
                        {
                            SQL_AssetETD sqlAssetETDUnderlyer = SetUnderlyingAssetETD(CS, pDbTransaction, asset.idAsset);
                            if (null != sqlAssetETDUnderlyer)
                            {
                                assetETD.idAsset_UnderlyerSpecified = true;
                                assetETD.idAsset_Underlyer = sqlAssetETDUnderlyer.Id;
                                assetETD.identifier_Underlyer = sqlAssetETDUnderlyer.Identifier;
                                assetETD.instrumentNum_Underlyer = Convert.ToInt32(sqlAssetETDUnderlyer.GetFirstRowColumnValue("INSTRUMENTNUM"));
                                assetETD.instrumentDen_Underlyer = Convert.ToInt32(sqlAssetETDUnderlyer.GetFirstRowColumnValue("INSTRUMENTDEN"));
                            }
                        }
                    }
                    //PM 20141120 [20508] Ajout gestion de cashFlowCalculationMethod
                    assetETD.cashFlowCalculationMethod = CashFlowCalculationMethodEnum.OVERALL;
                    if ((null != pMapDr["CASHFLOWCALCMETHOD"]) && (false == Convert.IsDBNull(pMapDr["CASHFLOWCALCMETHOD"].Value)))
                    {
                        string cashFlowCalculationMethod = Convert.ToString(pMapDr["CASHFLOWCALCMETHOD"].Value);
                        assetETD.cashFlowCalculationMethod = (CashFlowCalculationMethodEnum)StringToEnum.Parse(cashFlowCalculationMethod, assetETD.cashFlowCalculationMethod);
                    }
                    //PM 20140807 [20273][20106] Ajout informations d'arrondi
                    SQL_Currency currencyInfo = new SQL_Currency(CS, asset.currency)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (currencyInfo.LoadTable())
                    {
                        //PM 20141120 [20508] Prendre les informations d'arrondi de la devise quotée (devise de flux) si differente de la devise de prix
                        if (StrFunc.IsFilled(currencyInfo.IdCQuoted) && (currencyInfo.IdCQuoted != currencyInfo.IdC))
                        {
                            currencyInfo = new SQL_Currency(CS, currencyInfo.IdCQuoted)
                            {
                                DbTransaction = pDbTransaction
                            };
                            if (currencyInfo.LoadTable(new string[] { "ROUNDDIR", "ROUNDPREC" }))
                            {
                                assetETD.roundDir = currencyInfo.RoundDir;
                                assetETD.roundPrec = currencyInfo.RoundPrec;
                            }
                        }
                        else
                        {
                            assetETD.roundDir = currencyInfo.RoundDir;
                            assetETD.roundPrec = currencyInfo.RoundPrec;
                        }
                    }

                    // EG 20170206 [22787]
                    assetETD.unitOfMeasureSpecified = (null != pMapDr["UNITOFMEASURE"]) && (false == Convert.IsDBNull(pMapDr["UNITOFMEASURE"].Value));
                    if (assetETD.unitOfMeasureSpecified)
                        assetETD.unitOfMeasure = Convert.ToString(pMapDr["UNITOFMEASURE"].Value);

                    if ((null != pMapDr["UNITOFMEASUREQTY"]) && (false == Convert.IsDBNull(pMapDr["UNITOFMEASUREQTY"].Value)))
                        assetETD.unitOfMeasureQty = Convert.ToDecimal(pMapDr["UNITOFMEASUREQTY"].Value);

                    // EG 20170206 [22787]
                    assetETD.firstDeliveryDateSpecified = (null != pMapDr["FIRSTDELIVERYDATE"]) && (false == Convert.IsDBNull(pMapDr["FIRSTDELIVERYDATE"].Value));
                    if (assetETD.firstDeliveryDateSpecified)
                        assetETD.firstDeliveryDate = Convert.ToDateTime(pMapDr["FIRSTDELIVERYDATE"].Value);
                    assetETD.lastDeliveryDateSpecified = (null != pMapDr["LASTDELIVERYDATE"]) && (false == Convert.IsDBNull(pMapDr["LASTDELIVERYDATE"].Value));
                    if (assetETD.lastDeliveryDateSpecified)
                        assetETD.lastDeliveryDate = Convert.ToDateTime(pMapDr["LASTDELIVERYDATE"].Value);

                    string deliveryTimezone = string.Empty;
                    if ((null != pMapDr["DELIVERYTIMEZONE"]) && (false == Convert.IsDBNull(pMapDr["DELIVERYTIMEZONE"].Value)))
                        deliveryTimezone = Convert.ToString(pMapDr["DELIVERYTIMEZONE"].Value);

                    assetETD.deliveryTimeStartSpecified = (null != pMapDr["DELIVERYTIMESTART"]) && (false == Convert.IsDBNull(pMapDr["DELIVERYTIMESTART"].Value));
                    if (assetETD.deliveryTimeStartSpecified)
                    {
                        assetETD.deliveryTimeStart = m_Product.CreatePrevailingTime();
                        assetETD.deliveryTimeStart.HourMinuteTime.Value = Convert.ToString(pMapDr["DELIVERYTIMESTART"].Value) + ":00";
                        assetETD.deliveryTimeStart.Location.Value = deliveryTimezone;
                    }

                    assetETD.deliveryTimeEndSpecified = (null != pMapDr["DELIVERYTIMEEND"]) && (false == Convert.IsDBNull(pMapDr["DELIVERYTIMEEND"].Value));
                    if (assetETD.deliveryTimeEndSpecified)
                    {
                        assetETD.deliveryTimeEnd = m_Product.CreatePrevailingTime();
                        assetETD.deliveryTimeEnd.HourMinuteTime.Value = Convert.ToString(pMapDr["DELIVERYTIMEEND"].Value) + ":00";
                        assetETD.deliveryTimeEnd.Location.Value = deliveryTimezone;
                    }

                    if (null != pMapDr["ISAPPLYSUMMERTIME"])
                        assetETD.isApplySummertime = Convert.ToBoolean(pMapDr["ISAPPLYSUMMERTIME"].Value);

                    assetETD.deliveryPeriodFrequencySpecified = (null != pMapDr["PERIODMLTPDELIVERY"]) && (false == Convert.IsDBNull(pMapDr["PERIODMLTPDELIVERY"].Value));
                    assetETD.deliveryPeriodFrequencySpecified &= (null != pMapDr["PERIODDELIVERY"]) && (false == Convert.IsDBNull(pMapDr["PERIODDELIVERY"].Value));
                    assetETD.deliveryPeriodFrequencySpecified &= (null != pMapDr["DAYTYPEDELIVERY"]) && (false == Convert.IsDBNull(pMapDr["DAYTYPEDELIVERY"].Value));
                    assetETD.deliveryPeriodFrequencySpecified &= (null != pMapDr["ROLLCONVDELIVERY"]) && (false == Convert.IsDBNull(pMapDr["ROLLCONVDELIVERY"].Value));
                    if (assetETD.deliveryPeriodFrequencySpecified)
                    {
                        assetETD.deliveryPeriodFrequency = m_Product.CreateCalculationPeriodFrequency();
                        assetETD.deliveryPeriodFrequency.Period = StringToEnum.Period(Convert.ToString(pMapDr["PERIODDELIVERY"].Value));
                        assetETD.deliveryPeriodFrequency.PeriodMultiplier = new EFS_Integer(Convert.ToString(pMapDr["PERIODMLTPDELIVERY"].Value));
                        assetETD.deliveryPeriodFrequency.RollConvention = StringToEnum.RollConvention(Convert.ToString(pMapDr["ROLLCONVDELIVERY"].Value)); 
                    }

                    assetETD.dayTypeDeliverySpecified = (null != pMapDr["DAYTYPEDELIVERY"]) && (false == Convert.IsDBNull(pMapDr["DAYTYPEDELIVERY"].Value));
                    if (assetETD.dayTypeDeliverySpecified)
                        assetETD.dayTypeDelivery = StringToEnum.DayType(Convert.ToString(pMapDr["DAYTYPEDELIVERY"].Value));

                    assetETD.deliverySettlementOffsetSpecified = (null != pMapDr["PERIODMLTPDLVSETTLTOFFSET"]) && (false == Convert.IsDBNull(pMapDr["PERIODMLTPDLVSETTLTOFFSET"].Value));
                    assetETD.deliverySettlementOffsetSpecified &= (null != pMapDr["PERIODDLVSETTLTOFFSET"]) && (false == Convert.IsDBNull(pMapDr["PERIODDLVSETTLTOFFSET"].Value));
                    assetETD.deliverySettlementOffsetSpecified &= (null != pMapDr["DAYTYPEDLVSETTLTOFFSET"]) && (false == Convert.IsDBNull(pMapDr["DAYTYPEDLVSETTLTOFFSET"].Value));
                    if (assetETD.deliverySettlementOffsetSpecified)
                    {
                        PeriodEnum period = StringToEnum.Period(Convert.ToString(pMapDr["PERIODDLVSETTLTOFFSET"].Value));
                        int multiplier = Convert.ToInt32(pMapDr["PERIODMLTPDLVSETTLTOFFSET"].Value);
                        DayTypeEnum dayType = StringToEnum.DayType(Convert.ToString(pMapDr["DAYTYPEDLVSETTLTOFFSET"].Value));
                        assetETD.deliverySettlementOffset = m_Product.CreateOffset(period, multiplier, dayType);
                    }

                    assetETD.firstDeliverySettlementDateSpecified = (null != pMapDr["FIRSTDLVSETTLTDATE"]) && (false == Convert.IsDBNull(pMapDr["FIRSTDLVSETTLTDATE"].Value));
                    if (assetETD.firstDeliverySettlementDateSpecified)
                        assetETD.firstDeliverySettlementDate = Convert.ToDateTime(pMapDr["FIRSTDLVSETTLTDATE"].Value);
                    assetETD.lastDeliverySettlementDateSpecified = (null != pMapDr["LASTDLVSETTLTDATE"]) && (false == Convert.IsDBNull(pMapDr["LASTDLVSETTLTDATE"].Value));
                    if (assetETD.lastDeliverySettlementDateSpecified)
                        assetETD.lastDeliverySettlementDate = Convert.ToDateTime(pMapDr["LASTDLVSETTLTDATE"].Value);

                    assetETD.settlementOfHolidayDeliveryConventionSpecified = (null != pMapDr["SETTLTOFHOLIDAYDLVCONVENTION"]) && (false == Convert.IsDBNull(pMapDr["SETTLTOFHOLIDAYDLVCONVENTION"].Value));
                    if (assetETD.settlementOfHolidayDeliveryConventionSpecified)
                        assetETD.settlementOfHolidayDeliveryConvention = (SettlementOfHolidayDeliveryConventionEnum) ReflectionTools.EnumParse(new SettlementOfHolidayDeliveryConventionEnum(), 
                            Convert.ToString(pMapDr["SETTLTOFHOLIDAYDLVCONVENTION"].Value));

                }
            }
            return asset;
        }
        #endregion SetPosKeepingAsset

        #region SetPosKeepingQuote
        /// <summary>
        /// <para>
        /// Recherche du prix de settlement de préférence puis du prix de clôture de l'asset ss-jacent ou de l'asset principale
        /// </para>
        /// <para>Alimentation du log (Statut Warning ou Error) en cas de défaillance de cotation</para>
        /// <para>Cst.ErrLevel.SUCCES ou</para>
        /// <para>Cst.ErrLevel.DATANOTFOUND (si ss-jacent manquant (cas possible avec les options))</para>
        /// </summary>
        /// <returns>Cst.ErrLevel.SUCCES</returns>
        /// <returns>Cst.ErrLevel.DATANOTFOUND (si ss-jacent manquant (cas possible avec les options))</returns>
        /// FI 20130314 [18467] Passage à void puisque la méthode retourne tjs succes
        /// EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
        /// EG 20160302 (21969] Cst.ErrLevel
        /// EG 20170206 [22787]
        /// EG 20170424 [23064] 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190716 [VCL : New FixedIncome] Upd GetQuote
        protected override Cst.ErrLevel SetPosKeepingQuote(IPosRequest pPosRequest)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            int idAsset = Asset.idAsset;
            string identifier = Asset.identifier;
            Cst.UnderlyingAsset assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;

            SystemMSGInfo errReadOfficialSettlement = null;
            SystemMSGInfo errReadOfficialClose = null;
            // EG 20160302 (21969]
            // EG 20170206 [22787]
            if ((pPosRequest is PosRequestMaturityOffsetting) || 
                (pPosRequest is PosRequestOption) ||
                (pPosRequest is PosRequestPhysicalPeriodicDelivery))
            {
                // Lecture du sous-jacent exclusivement sur option
                if (Asset.assetCategory_UnderlyerSpecified)
                    assetCategory = Asset.assetCategory_Underlyer;

                if (Asset.idAsset_UnderlyerSpecified && Asset.putCallSpecified)
                {
                    idAsset = Asset.idAsset_Underlyer;
                    identifier = Asset.identifier_Underlyer;
                }
                else if (Asset.putCallSpecified)
                {
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    // EG 20160302 (21969] Erreur Sous-jacent non défini sur le contrat
                    if (pPosRequest.IdTSpecified)
                    {
                        string tradeIdentifier = PosKeepingData.TradeSpecified ? LogTools.IdentifierAndId(PosKeepingData.Trade.Identifier, pPosRequest.IdT) : pPosRequest.IdT.ToString();
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5109), 0, new LogParam(LogTools.IdentifierAndId(identifier, idAsset)), new LogParam(assetCategory.ToString()), new LogParam(tradeIdentifier)));
                    }
                    else
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5108), 0, new LogParam(LogTools.IdentifierAndId(identifier, idAsset)), new LogParam(assetCategory.ToString())));
                    }
                    errLevel = Cst.ErrLevel.DATANOTFOUND;
                }
            }

            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                bool isEOD = (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay);
                // EG 20140115 [19456] 
                // EG 20170424 [23064]
                Asset.SetFlagQuoteMandatory(DtBusiness, pPosRequest);
                /// EG 20140204 Rename GetQuoteTime en GetSettlementQuoteTime
                DateTime dtQuote = Asset.GetSettlementQuoteTime(DtBusiness, pPosRequest);
                Quote quote = m_PKGenProcess.ProcessCacheContainer.GetQuote(idAsset, dtQuote, identifier,
                    QuotationSideEnum.OfficialSettlement, assetCategory, new KeyQuoteAdditional(), ref errReadOfficialSettlement);
                if (null != quote)
                {
                    PosKeepingData.SetQuoteReference((Cst.UnderlyingAsset)Asset.assetCategory, quote, string.Empty);
                }

                quote = m_PKGenProcess.ProcessCacheContainer.GetQuote(idAsset, dtQuote, identifier,
                    QuotationSideEnum.OfficialClose, assetCategory, new KeyQuoteAdditional(), ref errReadOfficialClose) as Quote;

                if (null != quote)
                {
                    PosKeepingData.SetQuote((Cst.UnderlyingAsset)Asset.assetCategory, quote, string.Empty);
                }

                // EG 20140120 Gestion status (ITD/EOD)
                if (Asset.isOfficialSettlementMandatory && (null != errReadOfficialSettlement))
                {
                    if (false == isEOD)
                        errReadOfficialSettlement.processState.Status = ProcessStateTools.StatusWarningEnum;
                    
                    m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);

                    
                    Logger.Log(errReadOfficialSettlement.ToLoggerData(0));
                }
                else if (Asset.isOfficialCloseMandatory && (null != errReadOfficialClose))
                {
                    if (false == isEOD)
                        errReadOfficialClose.processState.Status = ProcessStateTools.StatusWarningEnum;

                    m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);

                    
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }
                else if ((null != errReadOfficialSettlement) && (null != errReadOfficialClose))
                {
                    if (false == isEOD)
                    {
                        errReadOfficialSettlement.processState.Status = ProcessStateTools.StatusWarningEnum;
                        errReadOfficialClose.processState.Status = ProcessStateTools.StatusWarningEnum;
                    }
                    m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialSettlement.processState.Status);
                    m_PKGenProcess.ProcessState.SetErrorWarning(errReadOfficialClose.processState.Status);

                    
                    Logger.Log(errReadOfficialSettlement.ToLoggerData(0));
                    Logger.Log(errReadOfficialClose.ToLoggerData(0));
                }
            }
            return errLevel;
        }
        #endregion SetPosKeepingQuote

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // PRIVATE METHODS
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region AddChildPosRequestPositionOptionGen
        /// <summary>
        ///  Alimentation de POSREQUEST afin de générer des denouements d'option par trade
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        // FI 20130917 [18953] Gestion du mode Intraday
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AddChildPosRequestPositionOptionGen(IDbTransaction pDbTransaction)
        {
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                // Chargement des trades candidats
                // RD 20220615 [26059] Add dbTransaction parameter
                List<Pair<int,Pair<IPosRequest,IPosRequest>>> tradeCandidates = GetExeAbnAssLoadPositionListTrade(pDbTransaction);
                if (0 == tradeCandidates.Count)
                {
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5185), 0,
                        new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(LogTools.IdentifierAndId(PosKeepingData.Asset.identifier, PosKeepingData.Asset.idAsset)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                        new LogParam(m_PosRequest.Qty)));

                    if (false == ((IPosRequestDetPositionOption)m_PosRequest.DetailBase).PartialExecutionAllowed)
                    {
                        //// Le traitement termine en erreur, le statut dans POSREQUEST vaut alors ERROR
                        codeReturn = Cst.ErrLevel.FAILUREWARNING;
                    }
                    else
                    {
                        // Le traitement n'effectue aucune action, le statut dans POSREQUEST vaut alors NA
                        codeReturn = Cst.ErrLevel.NOTHINGTODO;
                    }
                }

                if (codeReturn == Cst.ErrLevel.SUCCESS)
                {
                    Cst.PosRequestTypeEnum requestType = m_PosRequest.RequestType;
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    decimal qtyToClose = m_PosRequest.Qty;

                    if (!(m_PosRequest.DetailBase is IPosRequestDetPositionOption posRequestDetail))
                        throw new Exception("Type of Detail is not IPosRequestDetPositionOption");

                    bool isExecutionComplete = false;
                    for (int i = 0; i < tradeCandidates.Count; i++)
                    {
                        Pair<int, Pair<IPosRequest,IPosRequest>> tradeCandidate = tradeCandidates[i];

                        Cst.Capture.ModeEnum captureMode = ExeAssAbnCaptureMode(m_PosRequest.RequestType, posRequestDetail.AbandonRemainingQty, isExecutionComplete);
                        // EG 20170127 Qty Long To Decimal
                        decimal qtyToCloseByTrade = qtyToClose;
                        // Si exercice avec abandon des quantités restantes => Spheres abandonne tous les trades dès que la quantité demandée a été exercée (qtyToCloseByTrade vaut alors -1)
                        if (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionExercise && posRequestDetail.AbandonRemainingQty && isExecutionComplete)
                            qtyToCloseByTrade = -1;

                        TradeInput tradeInput = InitTradeInputDenOption(CS, pDbTransaction, tradeCandidate.First, m_MasterPosRequest.DtBusiness, qtyToCloseByTrade, captureMode,
                            ProcessBase.Session.IdA);
                        TradeDenOption tradeDenOption = tradeInput.tradeDenOption;
                        // FI 20130917 [18953] 
                        tradeDenOption.PosRequestMode = m_PosRequest.RequestMode;
                        //
                        if (qtyToClose > 0)
                        {
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            qtyToClose = System.Math.Max(qtyToClose - tradeDenOption.quantity.DecValue, 0);
                            if (qtyToClose == 0)
                            {
                                switch (captureMode)
                                {
                                    case Cst.Capture.ModeEnum.OptionExercise:
                                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                        // EG 20170127 Qty Long To Decimal
                                        if (tradeDenOption.availableQuantity.DecValue > tradeDenOption.quantity.DecValue)
                                            tradeDenOption.abandonRemaining.BoolValue = posRequestDetail.AbandonRemainingQty;
                                        break;
                                    case Cst.Capture.ModeEnum.OptionAbandon:
                                    case Cst.Capture.ModeEnum.OptionAssignment:
                                        //nothing
                                        break;
                                    default:
                                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", captureMode));
                                }
                            }
                        }

                        if (false == tradeDenOption.IsActionCanBePerformed)
                        {
                            codeReturn = Cst.ErrLevel.FAILURE;

                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5186), 0,
                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(LogTools.IdentifierAndId(PosKeepingData.Asset.identifier, PosKeepingData.Asset.idAsset)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                                new LogParam(LogTools.IdentifierAndId(tradeInput.Identifier, tradeInput.IdT)),
                                new LogParam(m_PosRequest.Qty)));
                        }

                        if (codeReturn == Cst.ErrLevel.SUCCESS)
                        {
                            // RD 20200120 [25114] Use posRequestDetail.feeCalculationSpecified
                            //if (posRequestDetail.feeCalculation)
                            if (posRequestDetail.FeeCalculationSpecified && posRequestDetail.FeeCalculation) 
                                DenOptionCalcFee(CSTools.SetCacheOn(CS), pDbTransaction, tradeInput, captureMode, true);

                            List<IPosRequest> lstPosRequestOptionTrade = tradeInput.NewPostRequest(CS, pDbTransaction, tradeDenOption.posRequestType, true);
                            IPosRequest posRequestOptionTrade = lstPosRequestOptionTrade.First();
                            posRequestOptionTrade.GroupProductSpecified = (m_MarketPosRequest.GroupProductSpecified);
                            if (posRequestOptionTrade.GroupProductSpecified)
                                posRequestOptionTrade.GroupProduct = m_MarketPosRequest.GroupProduct;

                            int idPRChild;
                            //PosKeepingTools.AddNewPosRequest(CS, dbTransaction, out idPRChild, posRequestOptionTrade, AppInstance, LogHeader.IdProcess, m_PosRequest.idPR);
                            // EG 20151019 [21112] on utilise la ligne POSREQUEST si elle existe sinon on créé une nouvelle ligne dans POSREQUEST
                            if (null != tradeCandidate.Second.First)
                            {
                                idPRChild = tradeCandidate.Second.First.IdPR;
                                posRequestOptionTrade.SetSource(m_PKGenProcess.AppInstance);
                                
                                //PosKeepingTools.UpdatePosRequest(CS, pDbTransaction, idPRChild, posRequestOptionTrade, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, m_PosRequest.idPR);
                                PosKeepingTools.UpdatePosRequest(CS, pDbTransaction, idPRChild, posRequestOptionTrade, m_PKGenProcess.Session.IdA, IdProcess, m_PosRequest.IdPR);
                            }
                            else
                            {
                                
                                //PosKeepingTools.AddNewPosRequest(CS, pDbTransaction, out idPRChild, posRequestOptionTrade, m_PKGenProcess.appInstance, LogHeader.IdProcess, m_PosRequest.idPR);
                                PosKeepingTools.AddNewPosRequest(CS, pDbTransaction, out idPRChild, posRequestOptionTrade, m_PKGenProcess.Session, IdProcess, m_PosRequest.IdPR);
                            }

                            if (captureMode == Cst.Capture.ModeEnum.OptionExercise && tradeDenOption.abandonRemaining.BoolValue)
                            {
                                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                // EG 20170127 Qty Long To Decimal
                                decimal remainQty = tradeDenOption.availableQuantity.DecValue - tradeDenOption.quantity.DecValue;
                                if (0 < remainQty)
                                {
                                    // Il existe déjà un abandon ce jour
                                    if (null != tradeCandidate.Second.Second)
                                    {
                                        if (Cst.Capture.ModeEnum.New == posRequestDetail.CaptureMode)
                                            remainQty += tradeCandidate.Second.Second.Qty;

                                        remainQty += tradeCandidate.Second.Second.Qty;
                                        idPRChild = tradeCandidate.Second.Second.IdPR;
                                    }

                                    tradeDenOption.posRequestType = Cst.PosRequestTypeEnum.OptionAbandon;
                                    tradeDenOption.quantity = new EFS_Decimal(remainQty);
                                    tradeDenOption.availableQuantity = tradeDenOption.quantity;
                                    tradeDenOption.abandonRemaining.BoolValue = false;

                                    IPosRequest posRequestOptionAbnTrade = tradeInput.NewPostRequest(CS , pDbTransaction,  Cst.Capture.ModeEnum.OptionAbandon);

                                    // Il existe déjà un abandon ce jour
                                    if (null != tradeCandidate.Second.Second)
                                    {
                                        posRequestOptionAbnTrade.SourceSpecified = true;
                                        posRequestOptionAbnTrade.Source = m_PKGenProcess.AppInstance.ServiceName;
                                        
                                        //PosKeepingTools.UpdatePosRequest(CS, pDbTransaction, idPRChild, posRequestOptionAbnTrade, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, m_PosRequest.idPR);
                                        PosKeepingTools.UpdatePosRequest(CS, pDbTransaction, idPRChild, posRequestOptionAbnTrade, m_PKGenProcess.Session.IdA, IdProcess, m_PosRequest.IdPR);
                                    }
                                    else
                                    {
                                        
                                        //PosKeepingTools.AddNewPosRequest(CS, pDbTransaction, out idPRChild, posRequestOptionAbnTrade, m_PKGenProcess.appInstance, LogHeader.IdProcess, m_PosRequest.idPR);
                                        PosKeepingTools.AddNewPosRequest(CS, pDbTransaction, out idPRChild, posRequestOptionAbnTrade, m_PKGenProcess.Session, IdProcess, m_PosRequest.IdPR);
                                    }
                                }
                            }
                        }

                        if (qtyToClose == 0)
                        {
                            isExecutionComplete = true;

                            bool isBreak = true;
                            if (m_PosRequest.RequestType == Cst.PosRequestTypeEnum.OptionExercise && posRequestDetail.AbandonRemainingQty)
                                isBreak = false;

                            if (isBreak)
                                break;
                        }

                        if (codeReturn != Cst.ErrLevel.SUCCESS)
                            break;
                    }

                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        if (qtyToClose > 0 && (false == posRequestDetail.PartialExecutionAllowed))
                        {
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            decimal qtyDispo = m_PosRequest.Qty - qtyToClose;

                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5187), 0,
                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(LogTools.IdentifierAndId(PosKeepingData.Asset.identifier, PosKeepingData.Asset.idAsset)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                                new LogParam(qtyDispo),
                                new LogParam(m_PosRequest.Qty)));

                            codeReturn = Cst.ErrLevel.FAILUREWARNING;
                        }
                    }
                }
                return codeReturn;
            }
            catch (Exception ex)
            {
                throw new Exception("exception occurred when adding new child POSREQUEST", ex);
            }
        }
        #endregion AddChildPosRequestPositionOptionGen
        #region AddPosRequestMaturityOffsetting
        /// <summary>
        /// Insertion d'un POSREQUEST d'une ligne de regroupement
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► REQUESTTYPE = MOO (concerne les dénouements automatiques d'options)</para>
        ///<para> ► REQUESTTYPE = MOF (concerne la liquidation des futures arrivés à échéance)</para>
        ///<para> ► REQUESTTYPE = CAS (concerne le cascading de position à échéance)</para>
        ///<para> ► REQUESTTYPE = SHI (concerne le shifting de position à échéance)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pDr">DataRow (Caractéristiques de la clé de position)</param>
        /// <param name="pNewIdPR">Id de la nouvelle ligne à insérer</param>
        /// <param name="pIdPR_Parent">Id parent</param>
        /// <returns></returns>
        // PM 20130218 [18414] Add CAS et SHI
        // EG 20130701 Plus de lecture du cours pour un MOF
        /// EG 20160302 [21969] 
        // EG 20180307 [23769] Gestion List<IPosRequest>
        private IPosRequest AddPosRequestMaturityOffsetting(Cst.PosRequestTypeEnum pRequestType, DataRow pDr, out int pNewIdPR, int pIdPR_Parent)
        {
            int newIdPR = 0;
            IPosKeepingKey posKeepingKey = PosKeepingTools.SetPosKeepingKey(m_Product, pDr);
            Cst.PosRequestAssetQuoteEnum posRequestAssetQuote =
                (pRequestType == Cst.PosRequestTypeEnum.MaturityOffsettingOption) ? Cst.PosRequestAssetQuoteEnum.UnderlyerAsset : Cst.PosRequestAssetQuoteEnum.Asset;
            // EG 20130701 Sur un MOF, la lecture de la cotation n'est plus opérée ici (cas du MOF en dès le car MaturityDate non renseignée)
            //InitPosKeepingData(posKeepingKey, m_MarketPosRequest.idEM, posRequestAssetQuote, true);
            // EG 20160302 (21969] CodeReturn
            Cst.ErrLevel codeReturn = InitPosKeepingData(null, posKeepingKey, m_MarketPosRequest.IdEM, posRequestAssetQuote, (pRequestType != Cst.PosRequestTypeEnum.MaturityOffsettingFuture));
            IPosRequest posRequest = GetSubRequest(pRequestType, Convert.ToInt32(m_MarketPosRequest.IdEM), posKeepingKey);
            if (null == posRequest)
            {
                // Insertion POSREQUEST (Maturité à l'échéance : MOO / MOF / CAS / SHI)    
                posRequest = PosKeepingTools.SetPosRequestMaturityOffsetting(m_Product, pRequestType, SettlSessIDEnum.EndOfDay, pDr);
                posRequest.IdTSpecified = false;
                posRequest.GroupProductSpecified = (m_MarketPosRequest.GroupProductSpecified);
                if (posRequest.GroupProductSpecified)
                    posRequest.GroupProduct = m_MarketPosRequest.GroupProduct;
                InitStatusPosRequest(posRequest, newIdPR, pIdPR_Parent, ProcessStateTools.StatusProgressEnum, m_LstSubPosRequest);

                
                //PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, pIdPR_Parent);
                PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, posRequest, m_PKGenProcess.Session, IdProcess, pIdPR_Parent);
                // PM 20130410 Mise à jour de posRequest.idPR qui était passé à 0 à la PosKeepingTools.InitStatusPosRequest
                posRequest.IdPR = newIdPR;
                // EG 20160301 [21969] 
                if (Cst.ErrLevel.SUCCESS != codeReturn)
                    posRequest.SetStatus(codeReturn);
            }
            pNewIdPR = newIdPR;
            return posRequest;
        }
        #endregion AddPosRequestMaturityOffsetting
        #region AddPosRequestPhysicalPeriodicDelivery
        /// <summary>
        /// Insertion d'un POSREQUEST d'une ligne de regroupement
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para> ► REQUESTTYPE = PDP (concerne les paiements de livraison physique des futures avec sous-jacent Commoditities)</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pRequestType">Type de demande</param>
        /// <param name="pDr">DataRow (Caractéristiques de la clé de position)</param>
        /// <param name="pNewIdPR">Id de la nouvelle ligne à insérer</param>
        /// <param name="pIdPR_Parent">Id parent</param>
        /// <returns></returns>
        // EG 20170206 [22787] New 
        // EG 20180307 [23769] Gestion List<IPosRequest>
        private IPosRequest AddPosRequestPhysicalPeriodicDelivery(DataRow pDr, out int pNewIdPR, int pIdPR_Parent)
        {
            int newIdPR = 0;
            IPosKeepingKey posKeepingKey = PosKeepingTools.SetPosKeepingKey(m_Product, pDr);
            Cst.ErrLevel codeReturn = InitPosKeepingData(null, posKeepingKey, m_MarketPosRequest.IdEM, Cst.PosRequestAssetQuoteEnum.Asset, false);
            IPosRequest posRequest = GetSubRequest(Cst.PosRequestTypeEnum.PhysicalPeriodicDelivery, Convert.ToInt32(m_MarketPosRequest.IdEM), posKeepingKey);
            if (null == posRequest)
            {
                // Insertion POSREQUEST (PDP)    
                posRequest = PosKeepingTools.SetPosRequestPhysicalPeriodicDelivery(m_Product, pDr);
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
        #endregion AddPosRequestPhysicalPeriodicDelivery

        #region CalFeesByQuantity
        /// <summary>
        /// Calcul des frais associés au cascading
        /// </summary>
        /// EG 20170221 [22879] New
        private IPayment[] CalFeesByQuantity(string pCS, IdMenu.Menu pIdMenu, int pIdT, decimal pQty)
        {
            IPayment[] payment = null;
            string action = IdMenu.GetIdMenu(pIdMenu);
            User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);
            TradeInput tradeInput = new TradeInput();
            tradeInput.SearchAndDeserializeShortForm(pCS, null, pIdT.ToString(), SQL_TableWithID.IDType.Id, user, m_PKGenProcess.Session.SessionId);

            //Spheres® supprime les frais existants
            tradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;
            tradeInput.CurrentTrade.OtherPartyPayment = null;

            IExchangeTradedDerivative etd = (IExchangeTradedDerivative)tradeInput.CurrentTrade.Product;
            etd.TradeCaptureReport.ClearingBusinessDate = new EFS_Date
            {
                DateValue = DtBusiness
            };

            FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(pCS), null,tradeInput, action, Cst.Capture.ModeEnum.Update,
                new string[] { Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity),
                               Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.QuantityContractMultiplier) }, 
                    pQty, "");
            FeeProcessing fees = new FeeProcessing(feeRequest);
            fees.Calc(CSTools.SetCacheOn(pCS), null);

            bool isExistFeeCalculated = ArrFunc.IsFilled(fees.FeeResponse);
            if (isExistFeeCalculated)
            {
                tradeInput.SetFee(TradeInput.FeeTarget.trade, fees.FeeResponse);
                tradeInput.ProcessFeeTax(pCS, null, TradeInput.FeeTarget.trade, feeRequest.DtReference);
                payment = tradeInput.CurrentTrade.OtherPartyPayment;
            }
            return payment;
        }
        #endregion CalFeesByQuantity
        #region CalcMaturityOffsettingFuture
        /// <summary>
        /// Calcul des frais associés aux liquidations de FUTURE
        /// <param name="pCS"></param>
        /// <param name="pIdT">Identifiant du trade Future</param>
        /// <param name="pQty">Qté disponible</param>
        /// <param name="pDtBusiness"></param>
        /// <param name="session"></param>
        /// </summary>
        /// <remarks>
        /// Spheres® charge le trade, modifie la date Business et calcule les frais comme si le trade était en saisie
        /// Spheres® passe la quantité disponible
        /// </remarks>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        private static IPayment[] CalcMaturityOffsettingFuture(string pCS, int pIdT, decimal pQty, DateTime pDtBusiness, AppSession session)
        {
            IPayment[] ret = null;
            //
            User user = new User(session.IdA, null, RoleActor.SYSADMIN);
            //
            TradeInput tradeInput = new TradeInput();
            tradeInput.SearchAndDeserializeShortForm(pCS, null, pIdT.ToString(), SQL_TableWithID.IDType.Id, user, session.SessionId);
            
            //Spheres® ecrase la date du trade
            //DateTime dtBusiness = m_MasterPosRequest.dtBusiness;
            DateTime dtBusiness = pDtBusiness;
            IExchangeTradedDerivative etd = (IExchangeTradedDerivative)tradeInput.CurrentTrade.Product;
            etd.TradeCaptureReport.ClearingBusinessDate = new EFS_Date
            {
                DateValue = dtBusiness
            };

            //Spheres® supprime les frais existants
            tradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;
            tradeInput.CurrentTrade.OtherPartyPayment = null;

            //PL 20200217 [25207] 
            //string action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_DLV); //Retourne "OTC_INP_TRD_DLV"
            string action = "OTC_INP_TRD_MOF";

            //PL 20130909 On considère ici ModeEnum.Update nécessaire au constructor FeeRequest
            // RD 20170208 [22815]
            //FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(pCS), tradeInput, action, Cst.Capture.ModeEnum.Update,
            //               Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity), Convert.ToDecimal(pQty), "");
            FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(pCS), null, tradeInput, action, Cst.Capture.ModeEnum.Update,
                new string[] { Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity),
                               Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.QuantityContractMultiplier)},
                    Convert.ToDecimal(pQty), "");
         
            FeeProcessing fees = new FeeProcessing(feeRequest);
            fees.Calc(CSTools.SetCacheOn(pCS), null);
         
            bool isExistFeeCalculated = ArrFunc.IsFilled(fees.FeeResponse);
            if (isExistFeeCalculated)
            {
                tradeInput.SetFee(TradeInput.FeeTarget.trade, fees.FeeResponse);
                // RD 20130205 [18389] Utiliser la bonne date de référence, selon qu'il s'agisse d'un ETD ou pas, d'une action sur trade ou pas.
                tradeInput.ProcessFeeTax(pCS, null, TradeInput.FeeTarget.trade, feeRequest.DtReference);
                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                    ret = (IPayment[])tradeInput.CurrentTrade.OtherPartyPayment;
            }

            return ret;
        }
        #endregion CalcMaturityOffsettingFuture
        #region CalcMaturityOffsettingOptionsPayment
        /// <summary>
        /// Calcul des frais associés aux denouement d'option
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        /// FI 20130307 [18467] appel à la méthode ConvertExeAssAbnRequestTypeToCaptureMode
        /// // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20180307 [23769] Gestion dbTransaction
        private IPayment[] CalcMaturityOffsettingOptionsPayment(string pCS, IDbTransaction pDbTransaction, int pIdT, Cst.PosRequestTypeEnum pRequestType, decimal pQty, DateTime pDtBusiness, AppSession session)
        {
            // FI 20130307 [18467] appel à ConvertExeAssAbnRequestTypeToCaptureMode
            Cst.Capture.ModeEnum mode = ConvertExeAssAbnRequestTypeToCaptureMode(pRequestType);
            User user = new User(session.IdA, null, RoleActor.SYSADMIN);
            TradeInput tradeInput = new TradeInput();
            tradeInput.SearchAndDeserializeShortForm(pCS,pDbTransaction, pIdT.ToString(), SQL_TableWithID.IDType.Id, user, session.SessionId);
            tradeInput.tradeDenOption = new TradeDenOption();
            tradeInput.tradeDenOption.InitShortForm(pDtBusiness, pQty);
            // FI 20130307 [18467] appel à ExeAssAbnOptionCalcFee
            IPayment[] opp = DenOptionCalcFee(CSTools.SetCacheOn(pCS),pDbTransaction, tradeInput, mode, true);
            return opp;
        }
        #endregion CalcMaturityOffsettingOptionsPayment
        #region CheckPosRequestPositionOptionGen
        /// <summary>
        ///  Retourne Cst.ErrLevel.SUCCESS si le traitement des denouements manuel d'option par position peut être exécuté
        /// </summary>
        /// <returns></returns>
        /// FI 20130917 [18953] suppression de l'appel à InitPosKeepingData
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CheckPosRequestPositionOptionGen()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (false == RequestCanBeExecuted(Cst.PosRequestTypeEnum.UpdateEntry, m_PosRequest.IdEM, m_PosRequest.PosKeepingKey))
                codeReturn = Cst.ErrLevel.DATAREJECTED;

            if (false == m_PosRequest.PosKeepingKeySpecified)
            {
                codeReturn = Cst.ErrLevel.DATAREJECTED;

                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // GLOP SYSTEMMSG
                string msg = "key is not specified [POSREQUEST (id:{0})]";

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, msg, 0, new LogParam(m_PosRequest.IdPR)));
            }
            return codeReturn;
        }
        #endregion CheckPosRequestPositionOptionGen
        #region ConvertExeAssAbnCaptureModeToFeeAction
        /// <summary>
        /// Convertie le type de denoument en Menu/Action reconnu par le calcul des frais
        /// </summary>
        /// <param name="pmode">Type de denouement</param>
        /// <param name="pIsAutomatic">true, si denouenement automatique</param>
        /// <returns></returns>
        private static string ConvertExeAssAbnCaptureModeToFeeAction(Cst.Capture.ModeEnum pMode, Boolean pIsAutomatic)
        {
            string action;
            switch (pMode)
            {
                case Cst.Capture.ModeEnum.OptionExercise:
                    if (pIsAutomatic)
                        action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE_AUTOMATIC);
                    else
                        action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE);
                    break;

                case Cst.Capture.ModeEnum.OptionAbandon:
                    if (pIsAutomatic)
                        action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ABN_AUTOMATIC);
                    else
                        action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ABN);
                    break;

                case Cst.Capture.ModeEnum.OptionAssignment:
                    if (pIsAutomatic)
                        action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ASS_AUTOMATIC);
                    else
                        action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ASS);
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Mode[{0}] is not implemented", pMode.ToString()));

            }

            return action;
        }
        #endregion ConvertExeAssAbnCaptureModeToFeeAction
        #region ConvertExeAssAbnRequestTypeToCaptureMode
        /// <summary>
        /// Convertie le type de denoument en type de saisie de denouement
        /// </summary>
        /// <param name="pRequestType"></param>
        /// <returns></returns>
        // EG 20180221 [23769] Public pour gestion asynchrone
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public Cst.Capture.ModeEnum ConvertExeAssAbnRequestTypeToCaptureMode(Cst.PosRequestTypeEnum pRequestType)
        {
            Cst.Capture.ModeEnum captureMode;
            switch (pRequestType)
            {
                case Cst.PosRequestTypeEnum.AutomaticOptionExercise:
                case Cst.PosRequestTypeEnum.OptionExercise:
                    captureMode = Cst.Capture.ModeEnum.OptionExercise;
                    break;
                case Cst.PosRequestTypeEnum.AutomaticOptionAbandon:
                case Cst.PosRequestTypeEnum.OptionAbandon:
                case Cst.PosRequestTypeEnum.OptionNotExercised:
                case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    captureMode = Cst.Capture.ModeEnum.OptionAbandon;
                    break;
                case Cst.PosRequestTypeEnum.AutomaticOptionAssignment:
                case Cst.PosRequestTypeEnum.OptionAssignment:
                    captureMode = Cst.Capture.ModeEnum.OptionAssignment;
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RequestType[{0}] is not implemented", pRequestType.ToString()));

            }
            return captureMode;
        }
        #endregion ConvertExeAssAbnRequestTypeToCaptureMode
        #region CreateCascadingTrades
        /// <summary>
        /// Génération des trades issus d'un cascading
        /// </summary>
        /// <param name="pPosRequest">Demande de cascading</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20170221 [22879] Add pDbTransaction and pTradeResult
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20220816 [XXXXX] [WI396] Cascading (idem Split) : Passage pDbTransaction en paramètre
        private Cst.ErrLevel CreateCascadingTrades(IDbTransaction pDbTransaction, IPosRequestCascadingShifting pPosRequest, ref List<Pair<int, string>> pTradeResult)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IDataReader dr = null;
            List<Pair<int, string>> tradeResult = new List<Pair<int, string>>();
            try
            {
                int idAsset = pPosRequest.PosKeepingKey.IdAsset;
                if ((pPosRequest != default(IPosRequestCascadingShifting))
                    && pPosRequest.IdTSpecified
                    && (pPosRequest.IdT > 0)
                    && (idAsset > 0))
                {
                    // Charger le trade dont réaliser le Cascading
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, pPosRequest.IdT)
                    {
                        IsWithTradeXML = true,
                        DbTransaction = pDbTransaction,
                        IsAddRowVersion = true
                    };
                    // RD 20210304 Add "trx."            
                    if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML", "DTTRADE", "DTBUSINESS", "DTEXECUTION", "DTORDERENTERED", "DTTIMESTAMP", "TZFACILITY" }))
                    {
                        int idT = 0;
                        EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                        IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                        DataDocumentContainer dataDocContainer = new DataDocumentContainer(dataDoc);

                        // PM 20191230 [25138] Ajout party Facility dans le document XML pour les trades pour lesquels il est manquant
                        // EG 20240531 [WI926] Add Parameter pIsTemplate
                        dataDocContainer.UpdateMissingTimestampAndFacility(CS, pDbTransaction, sqlTrade, false);

                        // Lire les Dérivative Contracts sur lesquels réaliser le Cascading de position
                        DataParameters parameters = new DataParameters(new DataParameter[] { });
                        parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), idAsset);
                        string sqlSelect = GetQueryCascadingContract();
                        QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
                        //dr = DataHelper.ExecuteReader(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        while (dr.Read())
                        {
                            string maturityMonthYear = Convert.ToString(dr["MATURITYMONTHYEAR"]);
                            int maturityMonth = Convert.ToInt32(dr["MATURITYMONTH"]);

                            // Vérifier que le Mois de Cascading correspond au mois de l'échéance
                            if (StrFunc.IsFilled(maturityMonthYear) && (IntFunc.IntValue(maturityMonthYear.Substring(4)) == maturityMonth))
                            {
                                int newMaturityMonth = Convert.ToInt32(dr["CASCMATURITYMONTH"]);
                                bool isYearIncrement = Convert.ToBoolean(dr["ISYEARINCREMENT"]);
                                int newMaturityYear = IntFunc.IntValue(maturityMonthYear.Substring(0, 4)) + (isYearIncrement ? 1 : 0);

                                int idDCDest = Convert.ToInt32(dr["IDDC_CASC"]);
                                string identifierDCDest = Convert.ToString(dr["IDENTIFIER"]);

                                // Convertion de l'échéance
                                string maturityMonthYearDest = newMaturityYear.ToString("D4") + newMaturityMonth.ToString("D2");

                                // Création d'un nouveau PosRequest fictif avec les informations détaillées
                                IPosRequestCascadingShifting pPosRequestTrade =
                                    (IPosRequestCascadingShifting)PosKeepingTools.SetPosRequestCascadingShifting(pPosRequest, idDCDest, identifierDCDest,
                                    maturityMonthYearDest, DtBusiness.Add(new TimeSpan(23, 59, 59)));

                                // Lire le template du trade à créer
                                // EG 20220816 [XXXXX] [WI396] Passage pDbTransaction en paramètre
                                codeReturn = SetAdditionalInfoFromTrade(pDbTransaction, pPosRequestTrade);

                                // Création du DataDocument du nouveau trade
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    // Copie du datadocument du trade initiale comme base pour du trade généré
                                    pPosRequestTrade.Detail.DataDocument = (DataDocumentContainer)dataDocContainer.Clone();
                                    codeReturn = SetDataDocumentCascadingShifting(pPosRequestTrade);
                                }

                                // Enregistrement du trade
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    codeReturn = RecordCascadingShiftingTrade(pDbTransaction, pPosRequestTrade, out idT);
                                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                                    {
                                        string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(CS, pDbTransaction, idT);
                                        tradeResult.Add(new Pair<int, string>(idT, tradeIdentifier));
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                // Ajout des IDT pour générations des événements de tous les Trades en fin de traitement
                if (0 < tradeResult.Count)
                    pTradeResult.AddRange(tradeResult);

                if (null != dr)
                    dr.Close();
            }
            return codeReturn;
        }
        #endregion CreateCascadingTrades
        #region CreateShiftingTrade
        /// <summary>
        /// Génération du trade issu d'un shifting
        /// </summary>
        /// <param name="pPosRequest">Requête de shifting</param>
        /// <param name="pNewTradeDtBusiness">Date de bourse dans laquelle générer le trade</param>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20170221 [22879] Add pDbTransaction and pTradeResult
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20220816 [XXXXX] [WI396] Shifting (idem Split) : Passage pDbTransaction en paramètre
        private Cst.ErrLevel CreateShiftingTrade(IDbTransaction pDbTransaction, IPosRequestCascadingShifting pPosRequest, DateTime pNewTradeDtBusiness, ref List<Pair<int,string>> pTradesResult)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IDataReader dr = null;
            Pair<int, string> tradeResult = null;
            try
            {
                int idAsset = pPosRequest.PosKeepingKey.IdAsset;
                if ((pPosRequest != default(IPosRequestCascadingShifting))
                    && pPosRequest.IdTSpecified
                    && (pPosRequest.IdT > 0)
                    && (idAsset > 0))
                {
                    // Charger le trade dont réaliser le Shifting
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, pPosRequest.IdT)
                    {
                        IsWithTradeXML = true,
                        DbTransaction = pDbTransaction,
                        IsAddRowVersion = true
                    };
                    if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML" }))
                    {
                        int idT = 0;
                        EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                        IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                        DataDocumentContainer dataDocContainer = new DataDocumentContainer(dataDoc);

                        // Lire le Dérivative Contract sur lesquel réaliser le Shifting de position
                        DataParameters parameters = new DataParameters(new DataParameter[] { });
                        parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), idAsset);
                        string sqlSelect = GetQueryShiftingContract();
                        QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
                        dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        if (dr.Read())
                        {
                            int idDCDest = Convert.ToInt32(dr["IDDC_SHIFT"]);
                            string identifierDCDest = Convert.ToString(dr["IDENTIFIER"]);

                            // Création d'un nouveau PosRequest fictif avec les informations détaillées
                            IPosRequestCascadingShifting pPosRequestTrade =
                                (IPosRequestCascadingShifting)PosKeepingTools.SetPosRequestCascadingShifting(pPosRequest, idDCDest, 
                                identifierDCDest, null, pNewTradeDtBusiness.Add(new TimeSpan(0, 0, 1)));

                            // Lire le template du trade à créer
                            // EG 20220816 [XXXXX] [WI396] Passage pDbTransaction en paramètre
                            codeReturn = SetAdditionalInfoFromTrade(pDbTransaction, pPosRequestTrade);

                            // Création du DataDocument du nouveau trade
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                // Copie du datadocument du trade initiale comme base pour du trade généré
                                pPosRequestTrade.Detail.DataDocument = (DataDocumentContainer)dataDocContainer.Clone();
                                codeReturn = SetDataDocumentCascadingShifting(pPosRequestTrade);
                            }

                            // Enregistrement du trade
                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                codeReturn = RecordCascadingShiftingTrade(pDbTransaction,pPosRequestTrade, out idT);
                                if (codeReturn == Cst.ErrLevel.SUCCESS)
                                {
                                    string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(CS, pDbTransaction, idT);
                                    tradeResult = new Pair<int, string>(idT, tradeIdentifier);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                // Ajout de l'IDT pour générations des événements du trade résultant du Shifting en fin de traitement
                if (null != tradeResult)
                    pTradesResult.Add(tradeResult);

                if (null != dr)
                    dr.Close();
            }
            return codeReturn;
        }
        #endregion CreateShiftingTrade

        #region DeleteEventPhysicalDeliveryPayment
        /// <summary>
        /// Suppression des Evénements liés à la livraison  dans le cadre de EOD_UpdateEntryGen / EOD_ClearingEndOfDay
        /// </summary>
        // EG 20170206 [22787] new
        protected override Cst.ErrLevel DeleteEventPhysicalPeriodicDelivery(IDbTransaction pDbTransaction, DataRow pRowClosing, DataRow pRowClosed)
        {
            if (Asset.firstDeliverySettlementDateSpecified && (DtBusiness == Asset.firstDeliverySettlementDate))
            {
                if (null != pRowClosing)
                    DeleteEventPhysicalDeliveryPayment(pDbTransaction, Convert.ToInt32(pRowClosing["IDT"]));
                if (null != pRowClosed)
                    DeleteEventPhysicalDeliveryPayment(pDbTransaction, Convert.ToInt32(pRowClosed["IDT"]));
            }
            return Cst.ErrLevel.SUCCESS;
        }
        /// <summary>
        /// Suppression des Evénements liés à la livraison 
        /// </summary>
        // EG 20170206 [22787] new
        // EG 20170424 [23064]
        protected Cst.ErrLevel DeleteEventPhysicalDeliveryPayment(IDbTransaction pDbTransaction, int pIdT)
        {
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);

            string sqlQuery = string.Empty;
            if (DataHelper.IsDbSqlServer(CS))
            {
                sqlQuery += @"delete ev
                from dbo.EVENT ev
                inner join dbo.EVENT evdva on (evdva.IDE = ev.IDE_EVENT)
                where (ev.IDT = @IDT)  and (evdva.EVENTCODE in ('STA', 'INT', 'TER')) and (evdva.EVENTTYPE = 'DVA');
                delete ev
                from dbo.EVENT ev
                inner join dbo.EVENT evdlv on (evdlv.IDE = ev.IDE_EVENT)
                where (ev.IDT = @IDT)  and (evdlv.EVENTCODE = 'DLV') and (evdlv.EVENTTYPE = 'PHY');
                delete from dbo.EVENT
                from dbo.EVENT ev
                where (ev.IDT = @IDT)  and (ev.EVENTCODE = 'DLV') and (ev.EVENTTYPE = 'PHY')";

            }
            else if (DataHelper.IsDbOracle(CS))
            {
                sqlQuery += @"delete from dbo.EVENT
                where IDE in 
                (
                    select ev.IDE 
                    from dbo.EVENT ev
                    inner join dbo.EVENT evdva on (evdva.IDE = ev.IDE_EVENT)
                    where (ev.IDT = @IDT)  and (evdva.EVENTCODE in ('STA', 'INT', 'TER')) and (evdva.EVENTTYPE = 'DVA');
                );
                delete from dbo.EVENT
                where IDE in 
                (
                    select ev.IDE 
                    from dbo.EVENT ev
                    inner join dbo.EVENT evdlv on (evdlv.IDE = ev.IDE_EVENT)
                    where (ev.IDT = @IDT)  and (evdlv.EVENTCODE = 'DLV') and (evdlv.EVENTTYPE = 'PHY');
                );                
                delete from dbo.EVENT
                where IDE in 
                (
                    select ev.IDE 
                    from dbo.EVENT ev
                    where (ev.IDT = @IDT)  and (ev.EVENTCODE = 'DLV') and (ev.EVENTTYPE = 'PHY')
                )";
            }
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion DeleteEventPhysicalDeliveryPaiement

        #region ExeAssAbnCaptureMode
        /// <summary>
        /// Retourne l'action (Cst.Capture.ModeEnum) en fonction de l'action sur position
        /// </summary>
        /// <param name="pRequestType"></param>
        /// <param name="pIsAbandonRemainingQty">true si exercice avec "abandon de la quantité restante"</param>
        /// <param name="pIsExerciceExecutionComplete">true si exercice et que les quantités demandés est déjà exerciées</param>
        /// <returns></returns>
        /// FI 20130403 [18467] new Method
        /// En cas d'exercice d'une position avec "abandon de la quantité restante", Spheres abandonne tous les trades avec quantité restante >0
        /// L'abandon s'effectue uniquement une fois que la totalité de l'exercice a été appliqué
        private Cst.Capture.ModeEnum ExeAssAbnCaptureMode(Cst.PosRequestTypeEnum pRequestType, bool pIsExerciceAbandonRemainingQty, bool pIsExerciceExecutionComplete)
        {
            Cst.Capture.ModeEnum ret = ConvertExeAssAbnRequestTypeToCaptureMode(pRequestType);
            if ((ret == Cst.Capture.ModeEnum.OptionExercise) && pIsExerciceAbandonRemainingQty)
            {
                if (pIsExerciceExecutionComplete)
                    ret = Cst.Capture.ModeEnum.OptionAbandon;
            }
            return ret;
        }
        #endregion ExeAssAbnCaptureMode
        #region ExeAssAbnOptionCalcFee
        /// <summary>
        ///  Retourne les frais appliqués en cas de denouement d'option
        ///  <para>Les frais appliqués sont stockés dans le trade</para>
        /// </summary>
        /// <param name="tradeInput">Représente un trade</param>
        /// <param name="pCaptureMode">Représente un denouenement (Exercice, Abandon, Assignation)</param>
        /// <param name="pIsAutomatic">true, si denouenement automatique</param>
        /// <returns></returns>
        // EG 20180221 [23769] Public pour gestion asynchrone
        public static IPayment[] DenOptionCalcFee(string pCS, IDbTransaction pDbTransaction, TradeInput tradeInput, Cst.Capture.ModeEnum mode, Boolean pIsAutomatic)
        {
            IPayment[] ret = null;

            string action = ConvertExeAssAbnCaptureModeToFeeAction(mode, pIsAutomatic);
            // RD 20170208 [22815]
            //FeeRequest feeRequest = new FeeRequest(pCS, tradeInput, action, mode,
            //               Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity),
            //               tradeInput.tradeDenOption.quantity.DecValue, "");
            FeeRequest feeRequest = new FeeRequest(pCS, pDbTransaction, tradeInput, action, mode,
                new string[] { Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity), 
                               Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.QuantityContractMultiplier) },
                    tradeInput.tradeDenOption.quantity.DecValue, "");

            FeeProcessing fees = new FeeProcessing(feeRequest);
            fees.Calc(pCS, pDbTransaction);

            bool isExistFeeCalculated = ArrFunc.IsFilled(fees.FeeResponse);
            if (isExistFeeCalculated)
            {
                // RD 20130205 [18389] Utiliser la bonne date de référence, selon s'il s'agit d'un ETD ou pas, d'une action sur trade ou pas.
                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                tradeInput.SetFee(TradeInput.FeeTarget.denOption, fees.FeeResponse);
                tradeInput.ProcessFeeTax(pCS, pDbTransaction, TradeInput.FeeTarget.denOption, feeRequest.DtReference);
                if (tradeInput.tradeDenOption.otherPartyPaymentSpecified)
                    ret = (IPayment[])tradeInput.tradeDenOption.otherPartyPayment;
            }
            return ret;
        }
        #endregion ExeAssAbnOptionCalcFee
        // Ajout de l'éventuel posrequest du dénouement du jour 
        // EG 20151019 [21112] New
        private IPosRequest GetExistingPosRequest(string pCS, int pIdT, Cst.PosRequestTypeEnum pPosRequestType, DataTable pDtPosAction)
        {
            IPosRequest lastDen = null;
            DataRow[] drPosaction = pDtPosAction.Select("IDT_CLOSING = " + pIdT.ToString());
            if (ArrFunc.IsFilled(drPosaction))
            {
                foreach (DataRow row in drPosaction)
                {
                    int idPR = Convert.ToInt32(row["IDPR"]);
                    IPosRequest posRequest = PosKeepingTools.GetPosRequest(pCS, Tools.GetNewProductBase(), idPR);
                    if (null == posRequest)
                        throw new Exception(StrFunc.AppendFormat("POSREQUEST not found (Id:{0})", idPR.ToString()));

                    if ((posRequest.RequestType == pPosRequestType) && (posRequest.IdPR_PosRequest == m_PosRequest.IdPR))
                    {
                        lastDen = posRequest;
                        break;
                    }
                }
            }
            return lastDen;
        }
        #region ExeAbnAssLoadPositionTrade
        /// <summary>
        /// Charge les trades relatifs au dénouement d'option, par position. 
        /// <para>Les trades exercés, assignés ou abandonnés manuellement (via saisie manuelle depuis le trade) sont exclus.</para>
        /// <para>Rappel important: Spheres n’autorise, par trade et par date business, qu'un seul exercice, assignation ou abandon.</para>
        /// </summary>
        /// <param name="pPosRequesType">type d'action sur option</param>
        /// <returns></returns>
        // EG 20151019 [21112] New
        /// EG 20171016 [23509] Upd  DTEXECUTION remplace DTTIMESTAMP sur le tri
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        // RD 20220615 [26059] Add pDbTransaction parameter
        private List<Pair<int, Pair<IPosRequest, IPosRequest>>> GetExeAbnAssLoadPositionListTrade(IDbTransaction pDbTransaction)
        {
            List<Pair<int,Pair<IPosRequest,IPosRequest>>> lstTrades = new List<Pair<int,Pair<IPosRequest,IPosRequest>>>();
            //Règle issu du Ticket 18473 EFS thread Item 3
            //Sphères applique la méthode FIFO basée sur la date et heure d'exécution
            //Dès lors qu'il y a plus d'un trade avec la même date et heure d'exécution, Spheres considère comme trade FIRST-IN celui ayant l'identifiant système inférieur
            string filterExpression;
            string sort;
            switch (m_PosRequest.RequestType)
            {
                case Cst.PosRequestTypeEnum.OptionExercise:
                case Cst.PosRequestTypeEnum.OptionAbandon:
                case Cst.PosRequestTypeEnum.OptionNotExercised:
                    filterExpression = "SIDE = '1'";
                    // EG 20171016 [23509]
                    sort = "DTEXECUTION, IDT"; //FIFO
                    break;
                case Cst.PosRequestTypeEnum.OptionAssignment:
                case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    filterExpression = "SIDE = '2'";
                    // EG 20171016 [23509]
                    sort = "DTEXECUTION, IDT"; //FIFO
                    break;
                default:
                    throw new ArgumentException(StrFunc.AppendFormat("Argument {0} is not valid ", m_PosRequest.RequestType.ToString()));
            }
            // RD 20220615 [26059] Add pDbTransaction parameter
            DataSet ds = GetPositionDenOption(pDbTransaction, DtBusiness);
            DataTable dtTrade = ds.Tables[0];
            // rowTrade liste des trades candidats
            DataRow[] rowTrade = dtTrade.Select(filterExpression, sort, DataViewRowState.CurrentRows);
            if (ArrFunc.IsFilled(rowTrade))
            {
                DataTable dtPosAction = ds.Tables[1];
                foreach (DataRow row in rowTrade)
                {
                    int idT = Convert.ToInt32(row["IDT"]);
                    Pair<int, Pair<IPosRequest, IPosRequest>> tradeCandidate = new Pair<int, Pair<IPosRequest, IPosRequest>>
                    {
                        First = Convert.ToInt32(row["IDT"]),
                        Second = new Pair<IPosRequest, IPosRequest>
                        {
                            // PosRequest du dénouement (éventuel) du jour
                            First = GetExistingPosRequest(CS, idT, m_PosRequest.RequestType, dtPosAction)
                        }
                    };
                    // PosRequest de l'abandon (éventuel) du jour
                    if (((IPosRequestDetPositionOption)m_PosRequest.DetailBase).AbandonRemainingQty)
                        tradeCandidate.Second.Second = GetExistingPosRequest(CS, idT, Cst.PosRequestTypeEnum.OptionAbandon, dtPosAction);
                    lstTrades.Add(tradeCandidate);
                }
            }
            return lstTrades;
        }
        #endregion ExeAbnAssLoadPositionTrade
        #region ExecChildPosRequestPositionOptionGen
        /// <summary>
        ///  Traitement des denouements d'option par trade issus du denouement d'option par position courant
        /// </summary>
        /// <param name="pDbTransaction"></param>
        ///FI 20130917 [18953] Gestion du mode intraday
        private Cst.ErrLevel ExecChildPosRequestPositionOptionGen(IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            //FI 20130917 [18953] Mode ITD/EOD en fonction du POSREQUEST parent
            // Gestion des denouements d'option générés précédemment
            DataSet ds = GetDataRequest(CS, pDbTransaction, Cst.PosRequestTypeEnum.OptionAbandon, m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM,
                                                                    PosKeepingTools.PosActionType.Trade, m_PosRequest.RequestMode);

            if ((ds == null) || ArrFunc.IsEmpty(ds.Tables[0].Rows))
                throw new Exception("Dataset ChildPosRequest is empty"); //Err grave => cela ne peut se produire

            //sauvegarde de m_PosRequest (m_PosRequest va être affecté ds le traitement unitaire des denouements d'option) 
            IPosRequest savPosRequest = PosKeepingTools.ClonePosRequest(m_PosRequest);
            try
            {
                foreach (DataRow dRow in ds.Tables[0].Rows)
                {
                    if ((false == dRow["IDPR_POSREQUEST"] is System.DBNull) &&
                            Convert.ToInt32(dRow["IDPR_POSREQUEST"]) == savPosRequest.IdPR)
                    {
                        codeReturn = PosRequestTradeOptionGen(pDbTransaction, dRow, savPosRequest.IdPR);
                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("exception occurred when executing child POSREQUEST", ex);
            }
            finally
            {
                RestorePosRequest(savPosRequest);
            }
            return codeReturn;
        }
        #endregion ExecChildPosRequestPositionOptionGen
        #region ExecPosRequestPositionOptionGen
        /// <summary>
        /// Traitement d'un denoument d'option manuel qui s'applique sur une position
        /// </summary>
        /// <returns></returns>
        // EG 20151019 [21112] Add dbTransaction parameter
        // EG 20151019 [21112] New
        private Cst.ErrLevel ExecPosRequestPositionOptionGen()
        {
            IDbTransaction dbTransaction = null;
            try
            {
                Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
                dbTransaction = DataHelper.BeginTran(m_PKGenProcess.Cs);
                // Si l'action n'est pas un ajout alors on supprime les actions précédentes 
                if (Cst.Capture.ModeEnum.New != ((IPosRequestDetPositionOption)m_PosRequest.DetailBase).CaptureMode)
                    codeReturn = RemoveChildPosRequestPositionOptionGen(dbTransaction);

                if (codeReturn == Cst.ErrLevel.SUCCESS)
                {
                    if (m_PosRequest.Qty > 0)
                    {
                        // EG 20151019 [21112] Add dbTransaction parameter
                        codeReturn = AddChildPosRequestPositionOptionGen(dbTransaction);
                        if (codeReturn == Cst.ErrLevel.SUCCESS)
                            // EG 20151019 [21112] Add dbTransaction parameter
                            codeReturn = ExecChildPosRequestPositionOptionGen(dbTransaction);
                    }
                }
                FinalizeTransaction(dbTransaction, codeReturn);
                return codeReturn;
            }
            catch (Exception)
            {
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
        }
        #endregion ExecPosRequestPositionOptionGen

        #region FinalizeTransaction
        /// <summary>
        ///  Termine une transaction
        /// <para>
        ///  Effectue un commit de la transaction si pErrLevel = Cst.ErrLevel.SUCCESS
        ///  </para>
        ///  <para>
        ///  Effectue un Rollback de la transaction si pErrLevel != Cst.ErrLevel.SUCCESS
        /// </para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pErrLevel"></param>
        private void FinalizeTransaction(IDbTransaction pDbTransaction, Cst.ErrLevel pErrLevel)
        {
            if (null != pDbTransaction)
            {
                if (pErrLevel == Cst.ErrLevel.SUCCESS)
                {
                    //PL 20151229 Use DataHelper.CommitTran()
                    //pDbTransaction. Commit();
                    DataHelper.CommitTran(pDbTransaction);
                }
                else
                {
                    //PL 20151229 Use DataHelper.RollbackTran()
                    //pDbTransaction. Rollback();
                    DataHelper.RollbackTran(pDbTransaction);
                }
            }
        }
        #endregion FinalizeTransaction

        #region GenerateDVA
        private bool GenerateDVA(ref bool opIsPayment)
        {
            bool isGenerateDVA = (Asset.physicalSettlementAmount == PhysicalSettlementAmountEnum.Unsettled)
                                 ||
                                 (Asset.physicalSettlementAmount == PhysicalSettlementAmountEnum.Settled);
            
            opIsPayment = isGenerateDVA && (Asset.physicalSettlementAmount == PhysicalSettlementAmountEnum.Settled); 

            return isGenerateDVA;
        }
        #endregion GenerateDVA
        #region GetUnitOfMeasure
        private string GetUnitOfMeasure()
        {
            // Unité de mesure de la quantité
            string unitOfMeasure = string.Empty;
            if (Asset.unitOfMeasureSpecified)
            {
                //ExtendEnum enums = ExtendEnumsTools.ListEnumsSchemes["UnitOfMeasureEnum"];
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                ExtendEnum enums = DataEnabledEnumHelper.GetDataEnum(CS, "UnitOfMeasureEnum");
                ExtendEnumValue enumValue = enums.GetExtendEnumValueByValue(Asset.unitOfMeasure);
                if (null != enumValue)
                    unitOfMeasure = enumValue.CustomValue;
            }
            return unitOfMeasure;
        }
        #endregion GetUnitOfMeasure
        #region GetQuoteOfAsset
        // EG 20170424 [23064] Add pPosRequestReference
        // EG 20240115 [WI808] Update to public
        public Cst.ErrLevel GetQuoteOfAsset(ref Nullable<DateTime> opDtQuote, ref Nullable<QuoteTimingEnum> opQuoteTiming, ref Nullable<QuotationSideEnum> opQuoteSide, 
                                             ref Nullable<decimal> opSettlementPrice, ref Nullable<decimal> opSettlementPrice100)
        {
            return GetQuoteOfAsset(ref opDtQuote, ref opQuoteTiming, ref opQuoteSide, ref opSettlementPrice, ref opSettlementPrice100, null);
        }
        private Cst.ErrLevel GetQuoteOfAsset(ref Nullable<DateTime> opDtQuote, ref Nullable<QuoteTimingEnum> opQuoteTiming, ref Nullable<QuotationSideEnum> opQuoteSide,
                                             ref Nullable<decimal> opSettlementPrice, ref Nullable<decimal> opSettlementPrice100, IPosRequest pPosRequestReference)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            
            // SettlementPrice
            if (IsQuoteOk)
            {
                // EG 20170424 [23064]
                IPosKeepingQuote quote = Asset.GetSettlementPrice(DtBusiness, ref errLevel, pPosRequestReference);
                if (null != quote)
                {
                    opSettlementPrice = quote.QuotePrice;
                    opQuoteSide = quote.QuoteSide;
                    opQuoteTiming = quote.QuoteTiming;
                    opDtQuote = quote.QuoteTime;
                }

                if (opSettlementPrice.HasValue)
                {
                    opSettlementPrice100 = PosKeepingData.ToBase100_UNL(opSettlementPrice.Value);
                    opSettlementPrice100 = PosKeepingData.VariableContractValue_UNL(opSettlementPrice100);
                }
            }

            return errLevel ;
        }
        #endregion GetQuoteOfAsset

        #region GetDeliveryDate
        /// <summary>
        /// Insertion d'une Date DLY sur Exercise/Assignment
        /// DATE SOURCE (de BASE) : DTBUSINESS
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// ► Cas 1 : Dénouement ou Liquidation à l'échéance (DTBUSINESS = MATURITY.MATURITYDATE)
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        ///   ● EGALE A la date de livraison (DLY) de cette échéance (MATURITY.DELIVERYDATE) 
        ///   ou si celle-ci n'est pas renseignée
        ///   ● CALCULE PAR application de l'offset spécifié dans la règle d'échéance associée 
        ///     (maturityRule.PERIODMLTPDELIVERYDATEOFFSET, maturityRule.PERIODDELIVERYDATEOFFSET, 
        ///     maturityRule.DAYTYPEDELIVERYDATEOFFSET)
        ///   ou si celui-ci n'est pas renseigné
        ///   ● EGALE A la date d'échéance (MATURITY.MATURITYDATE ou DTBUSINESS)
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        /// ► Cas 2 : Dénouement anticipé (manuel)(DTBUSINESS inférieur ou égale MATURITY.MATURITYDATE)
        /// ─────────────────────────────────────────────────────────────────────────────────────────
        ///   ● CALCULE PAR application de l'offset spécifié dans la règle d'échéance associée 
        ///     (maturityRule.PERIODMLTPDELIVERYDATEOFFSET, maturityRule.PERIODDELIVERYDATEOFFSET, 
        ///     maturityRule.DAYTYPEDELIVERYDATEOFFSET)
        ///   ou si celui-ci n'est pas renseigné
        ///   ● EGALE A la date de dénouement (DTBUSINESS)
        ///   
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private DateTime GetDeliveryDate(DateTime pDtBusiness)
        {
            DateTime deliveryDate = pDtBusiness;
            if (Asset.settlMethod == SettlMethodEnum.PhysicalSettlement)
            {
                if ((DtBusiness == Asset.maturityDate) && Asset.deliveryDateSpecified)
                {
                    deliveryDate = Asset.deliveryDate;
                }
                else if (Asset.deliveryDelayOffsetSpecified)
                {
                    IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, Asset.idBC);
                    deliveryDate = Tools.ApplyOffset(CS, deliveryDate, Asset.deliveryDelayOffset, bda, null);
                }
            }
            return deliveryDate;
        }
        #endregion GetDeliveryDate
        #region GetValueDateOnDVA
        /// <summary>
        /// Obtient la date de valeur relative à un montant de livraison (DtDelivery - 1JO)
        /// </summary>
        // PL 20170413 [23064] New method
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private DateTime GetValueDateOnDVA(DateTime pDeliveryDate)
        {
            return Tools.ApplyOffset(CS, m_Product, pDeliveryDate, -1, DayTypeEnum.ExchangeBusiness, Asset.idBC, null);
        }
        #endregion GetValueDateOnDVA
        #region GetSettlementDateOnDVA
        /// <summary>
        /// Obtient la date de règlement relative à un montant de livraison (DtDelivery + 1JO)
        /// </summary>
        // EG 20190129 [24361] New
        private DateTime GetSettlementDateOnDVA(DateTime pDeliveryDate)
        {
            return Tools.ApplyOffset(CS, m_Product, pDeliveryDate, +1, DayTypeEnum.ExchangeBusiness, Asset.idBC, null);
        }
        #endregion GetSettlementDateOnDVA
        #region GetPositionDenOption
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // RD 20220615 [26059] Add pDbTransaction parameter
        private DataSet GetPositionDenOption(IDbTransaction pDbTransaction, DateTime pDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), PosKeepingData.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), PosKeepingData.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), PosKeepingData.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), PosKeepingData.IdB_Clearer);
            parameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), m_PosRequest.IdPR);
            AddParameterDealer(parameters);

            string sqlSelect = GetQueryPositionDenOption();
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            // PL 20180312 WARNING: Use Read Commited !                
            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, 480);
            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, 480);
            // RD 20220615 [26059] Use pDbTransaction parameter
            DataSet ds;
            if (null != pDbTransaction)
                ds = DataHelper.ExecuteDataset(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            else
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, 480);

            // EG 20150920 [21314] ORIGINALQTY_CLOSED|ORIGINALQTY_CLOSING added after Dataset construction
            // EG 20170127 Qty Long To Decimal
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSED", typeof(decimal));
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSING", typeof(decimal));
            return ds;
        }
        #endregion GetPositionDenOption
        #region GetPositionTradeOption
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20130312 [18467] add pDbTransaction parameter (parce Spheres® insère désormais dans POSREQUEST dans le cadre d'une transaction SQL)
        /// EG 20140204 [19586] IsolationLevel 
        // EG 20141224 [20566] DTPOS unused
        private DataSet GetPositionTradeOption(string pCS, IDbTransaction pDbTransaction, DateTime pDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), PosKeepingData.Trade.IdT);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            Nullable<int> idPR = GetIdPR();
            if (idPR.HasValue)
                parameters.Add(new DataParameter(CS, "IDPR", DbType.Int32), idPR.Value);

            // EG 20151125 [20979] Refactoring
            string sqlSelect = GetQueryPositionTradeOptionAndAction(idPR.HasValue);
            QueryParameters qryParam = new QueryParameters(pCS, sqlSelect, parameters);

            DataSet ds;
            if (null != pDbTransaction)
            {
                ds = DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
            }
            else
            {
                // PL 20180312 WARNING: Use Read Commited !                
                //ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadUncommitted, qryParam, 480);
                ds = OTCmlHelper.GetDataSetWithIsolationLevel(pCS, IsolationLevel.ReadCommitted, qryParam, 480);
            }
            // EG 20150920 [21314] ORIGINALQTY_CLOSED|ORIGINALQTY_CLOSING added after Dataset construction
            // EG 20170127 Qty Long To Decimal
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSED", typeof(decimal));
            ds.Tables[1].Columns.Add("ORIGINALQTY_CLOSING", typeof(decimal));
            return ds;
        }
        #endregion GetPositionTradeOption
        #region GetRowsInSpheresOrder
        /// <summary>
        /// Retourne les enregistrements du datatable dans un ordre spécifique tel que les enregistrements saisies mauellement dans l'application web de spheres 
        /// sont prioritaires par rapport aux autres enregistrements
        /// <para>Le datatable doit posséder une colonne SOURCE </para> 
        /// <para>En sortie le datatable contient une nouvelle colonne nommée "ORDER"</para>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataRow[] GetRowsInSpheresOrder(DataTable pDataTable)
        {
            DataColumn column = pDataTable.Columns["SOURCE"];
            if (null == column)
                throw new ArgumentException("datatable doesn't constains SOURCE column");

            column = pDataTable.Columns["ORDER"];
            if (null != column)
                throw new ArgumentException("datatable already contains ORDER column");

            pDataTable.Columns.Add(new DataColumn("ORDER", typeof(int)));
            foreach (DataRow dr in pDataTable.Rows)
            {
                bool isSpheresWebInput = false;
                string source = Convert.ToString(dr["SOURCE"]);
                if (StrFunc.IsFilled(source))
                {
                    isSpheresWebInput = EFS.Common.AppInstance.IsSpheresWebApp(Convert.ToString(dr["SOURCE"]));
                }
                if (isSpheresWebInput)
                    dr["ORDER"] = 0;
                else
                    dr["ORDER"] = 1;
            }
            DataRow[] rows = pDataTable.Select(string.Empty, "ORDER", DataViewRowState.CurrentRows);
            return rows;
        }
        #endregion GetRowsInSpheresOrder

        #region InitTradeInputDenOption
        /// <summary>
        ///  Initialisation TradeInput pour préparer un exercice,un abandon ou une assignation 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Représente le trade</param>
        /// <param name="pDtBusiness">Journée de bourse de l'action</param>
        /// <param name="qtyToClose">Quantité à Exercer, Adandonner, Assigner<para>-1 pour considérer toutes les quantités restantes</para></param>
        /// <param name="pCaptureMode">Représente le type d'action</param>
        /// <param name="pIdA">Acteur de l'appInstance</param>
        /// <returns></returns>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20151019 [21112] Add pDbTransaction parameter
        // RD 20161031 [22567] Modify
        // EG 20170127 Qty Long To Decimal
        private TradeInput InitTradeInputDenOption(string pCS, IDbTransaction pDbTransaction, int pIdT, DateTime pDtBusiness, decimal qtyToClose, Cst.Capture.ModeEnum pCaptureMode, int pIdA)
        {
            User user = new User(pIdA, null, RoleActor.SYSADMIN);

            TradeInput tradeInput = new TradeInput();
            tradeInput.SearchAndDeserializeShortForm(pCS, pDbTransaction, pIdT.ToString(), SQL_TableWithID.IDType.Id, user, string.Empty);

            tradeInput.tradeDenOption = new TradeDenOption();
            TradeDenOption tradeDenOption = tradeInput.tradeDenOption;
            tradeDenOption.Initialized(pCS, pDbTransaction, pIdT, pDtBusiness, (IExchangeTradedDerivative)tradeInput.Product.Product,  
                pCaptureMode, tradeInput.DataDocument, Cst.DenOptionActionType.@new);

            if (qtyToClose != -1)
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // RD 20161031 [22567] Use positionQuantity instead availableQuantity
                // EG 20170127 Qty Long To Decimal
				tradeDenOption.quantity.DecValue = System.Math.Min(qtyToClose, tradeDenOption.positionQuantity.DecValue);
            }
            else
            {
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // RD 20161031 [22567] Use positionQuantity instead availableQuantity
                // EG 20170127 Qty Long To Decimal
				tradeDenOption.quantity.DecValue = tradeDenOption.positionQuantity.DecValue;
            }

            tradeDenOption.abandonRemaining.BoolValue = false;

            return tradeInput;
        }
        #endregion InitTradeInputDenOption
        #region InsertEventDet
        // EG 20141128 [20520] Nullable<decimal>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190613 [24683] Add PosKeepingData parameter
        // EG 20190730 Add TypePrice parameter
        protected override Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, int pIdE, decimal pQty, 
            decimal pContractMultiplier, Nullable<AssetMeasureEnum> pTypePrice, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice)
        {
            // EG 20141128 [20520] Nullable<decimal>
            Nullable<decimal> price100 = null;
            Nullable<decimal> closingPrice100 = null;

            if (pPrice.HasValue)
            {
                price100 = pPosKeepingData.ToBase100(pPrice.Value);
                price100 = pPosKeepingData.VariableContractValue(price100);
            }
            if (pClosingPrice.HasValue)
            {
                closingPrice100 = pPosKeepingData.ToBase100(pClosingPrice);
                closingPrice100 = pPosKeepingData.VariableContractValue(closingPrice100);
            }
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEventDet_Closing(pDbTransaction, pIdE, pQty, pContractMultiplier, pTypePrice, pPrice, price100, pClosingPrice, closingPrice100);
            return codeReturn;
        }
        #endregion InsertEventDet
        #region InsertPosActionDetAndOptionLinkedEvent
        /// <summary>
        /// Insertion des éléments matérialisant la clôture dans POSACTIONDET/EVENTPOSACTIONDET/EVENT
        /// pour les dénouement d'options
        /// ATTENTION la colonne IDT_CLOSING = Négociation traitée soit:
        /// (IDT_CLOSING = IDT_BUY et IDT_SELL = null) ou (IDT_CLOSING = IDT_SELL et IDT_BUY = null)
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdPA">Identifiant POSACTION</param>
        /// <param name="pIdPADET">Identifiant POSACTIONDET</param>
        /// <param name="pIdE">Identifiant EVENT</param>
        /// <param name="pRowPosActionDet">Nouvelles données de clôture</param>
        /// <param name="pRowClosing">Données de la négociation clôturante</param>
        /// <returns></returns>
        // EG 20141205 PositionEffect
        // EG 20180221 [23769] Public pour gestion asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        public Cst.ErrLevel InsertPosActionDetAndOptionLinkedEvent(IDbTransaction pDbTransaction, int pIdPA, int pIdPADET, ref int pIdE,
                                                                DataRow pRowPosActionDet, DataRow pRowClosing)
        {
            // POSACTIONDET
            Nullable<int> idT_Buy = null;
            if (false == Convert.IsDBNull(pRowPosActionDet["IDT_BUY"]))
                idT_Buy = Convert.ToInt32(pRowPosActionDet["IDT_BUY"]);
            Nullable<int> idT_Sell = null;
            if (false == Convert.IsDBNull(pRowPosActionDet["IDT_SELL"]))
                idT_Sell = Convert.ToInt32(pRowPosActionDet["IDT_SELL"]);
            int idT_Closing = Convert.ToInt32(pRowPosActionDet["IDT_CLOSING"]);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = Convert.ToDecimal(pRowPosActionDet["QTY"]);
            string positionEffect = String.Empty;
            // EG 20141205 PositionEffect
            if (false == Convert.IsDBNull(pRowPosActionDet["POSITIONEFFECT"]))
                positionEffect = pRowPosActionDet["POSITIONEFFECT"].ToString();

            Cst.ErrLevel codeReturn = InsertPosActionDet(pDbTransaction, pIdPA, pIdPADET, idT_Buy, idT_Sell, idT_Closing, qty, positionEffect);

            // EVENT CLOTURANTE
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = InsertPosKeepingOptionEvents(pDbTransaction, pRowClosing, pRowPosActionDet, pIdPADET, ref pIdE);
            return codeReturn;
        }
        #endregion InsertPosActionDetAndOptionLinkedEvent
        #region InsertPosKeepingOptionEvents
        /// <summary>
        /// Application de la clôture sur les événements suite à dénouement d'options
        /// </summary>
        /// <returns></returns>
        // EG 20120327 Méthode InitPaymentForEvent déplacée dans Trade.cs de Process
        // EG 20151019 [21465] New Test feeCalculationSpecifiedSpecified|feeCalculationSpecified
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel InsertPosKeepingOptionEvents(IDbTransaction pDbTransaction, DataRow pRow,
            DataRow pRowPosActionDet, int pIdPADET, ref int pIdE)
        {
            int idEParent = pIdE;
            #region EVENT (ABN/ASS/EXE)
            Cst.ErrLevel codeReturn = InsertOptionEvents(pDbTransaction, pRow, pRowPosActionDet, pIdPADET, ref pIdE);
            #endregion EVENT (ABN/ASS/EXE)

            // Insertion des frais
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                #region EVENT (OPP)
                IPosRequestDetail detail = (IPosRequestDetail)m_PosRequest.DetailBase;
                // EG 20151019 [21465] New Application des barèmes de frais (MODE BULK)
                if (detail.FeeCalculationSpecified && detail.FeeCalculation)
                {

                    Cst.Capture.ModeEnum mode = ConvertExeAssAbnRequestTypeToCaptureMode(m_PosRequest.RequestType);
                    User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);
                    TradeInput tradeInput = new TradeInput();
                    tradeInput.SearchAndDeserializeShortForm(CS, pDbTransaction, m_PosRequest.IdT.ToString(), SQL_TableWithID.IDType.Id, user, m_PKGenProcess.Session.SessionId);
                    tradeInput.tradeDenOption = new TradeDenOption();
                    tradeInput.tradeDenOption.InitShortForm(m_PosRequest.DtBusiness, m_PosRequest.Qty);
                    IPayment[] opp = DenOptionCalcFee(CSTools.SetCacheOn(CS), pDbTransaction, tradeInput, mode, false);
                    detail.PaymentFeesSpecified = ArrFunc.IsFilled(opp);
                    if (ArrFunc.IsFilled(opp))
                    {
                        IExchangeTradedDerivative etd = (IExchangeTradedDerivative)tradeInput.CurrentTrade.Product;
                        ExchangeTradedDerivativeContainer etdContainer = new ExchangeTradedDerivativeContainer(etd, tradeInput.DataDocument);
                        etdContainer.Efs_ExchangeTradedDerivative = new EFS_ExchangeTradedDerivative(CS, pDbTransaction, etd, etdContainer.DataDocument, Cst.StatusBusiness.ALLOC,null);
                        detail.PaymentFeesSpecified = true;
                        detail.PaymentFees = opp;
                        foreach (IPayment payment in detail.PaymentFees)
                        {
                            if (null == payment.Efs_Payment)
                                payment.Efs_Payment = new EFS_Payment(CS, pDbTransaction,  payment, tradeInput.Product.Product, etdContainer.DataDocument);
                        }
                    }
                }
                if (detail.PaymentFeesSpecified)
                {
                    foreach (IPayment payment in detail.PaymentFees)
                    {
                        pIdE++;
                        int savIdE = pIdE;
                        codeReturn = m_EventQuery.InsertPaymentEvents(pDbTransaction, m_TradeLibrary.DataDocument, Convert.ToInt32(pRow["IDT"]),
                        payment, DtBusiness, 1, 1, ref pIdE, idEParent);


                        // EG 20120511 Add EVENTPOSACTIONDET pour toutes les lignes de FRAIS
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            for (int i = savIdE; i <= pIdE; i++)
                            {
                                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, i);
                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    break;
                            }
                        }
                        if (Cst.ErrLevel.SUCCESS != codeReturn)
                            break;
                    }
                }
                #endregion EVENT (OPP)
            }
            return codeReturn;
        }
        #endregion InsertPosKeepingOptionEvents
        #region InsertSettlementCurrencyEvent
        /// <summary>
        /// Le montant SCU suite à dénouement d'option est calculé directement avec le cours du sous-jacent
        /// Dans le cas d'un dénouement INTRADAY ce cours peut être absent. L'événement SCU et ses composants sera malgré tout
        /// créé avec un montant à zéro et sans Payer/Receiver
        /// A charge au traitement EOD final (via Calcul des Cash-Flows) de le compléter
        /// </summary>
        /// EG 20140116 [19456] 
        //PM 20140514 [19970][19259] Utilisation de PosKeepingData.asset.currency au lieu de PosKeepingData.asset.priceCurrency
        // EG 20150128 [20726] 
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20180307 [23769] Gestion dbTransaction
        private Cst.ErrLevel InsertSettlementCurrencyEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event,
            bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, IPosRequestDetOption pPosRequestDetOption, int pIdPADET)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;

            #region EVENT SCU (Cash Settlement sur Exercice/Assignation)
            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;
            string eventType = EventTypeFunc.SettlementCurrency;
            decimal strikePrice = pPosRequestDetOption.StrikePrice;

            // EG 20140325 [19671][19766] 
            EFS_Asset efs_Asset = null;
            if (pPosRequestDetOption.UnderlyerSpecified)
            {
                efs_Asset = pPosRequestDetOption.Underlyer.GetCharacteristics(CS, pDbTransaction);
                strikePrice = ExchangeTradedDerivativeTools.ConvertStrikeToUnderlyerBase(strikePrice, pPosRequestDetOption.Underlyer.AssetCategory,
                    efs_Asset.instrumentNum, efs_Asset.instrumentDen);
            }

            #region Insertion EVENT / EVENTCLASS

            // EG 20150128 [20726] 
            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(PosKeepingData, EventTypeFunc.SettlementCurrency, pIsDealerBuyer);
            IPosKeepingQuote quote = Asset.GetSettlementPrice(DtBusiness, ref codeReturn);
            // EG 20151001 [21414]
            Nullable<decimal> cashSettlementAmount = null;
            if (null != quote)
            {
                _payrec.SetSettlementInfo(PosKeepingData, quote);
                cashSettlementAmount = PosKeepingData.CashSettlement(strikePrice, quote.QuotePrice, pQty);
            }
            _payrec.SetPayerReceiver(cashSettlementAmount);

            codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver,
                eventCode, eventType, DtBusiness, DtBusiness, DtBusiness, DtBusiness, _payrec.Amount, Asset.currency, UnitTypeEnum.Currency.ToString(), _payrec.Amount.HasValue ? StatusCalculEnum.CALC : StatusCalculEnum.TOCALC, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, DtBusiness, false);

            // EG 20150616 [21124]
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, DtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, DtSettlement, true);

            #endregion Insertion EVENT / EVENTCLASS
            #region Insertion EVENTASSET avec info du sous-jacent
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (pPosRequestDetOption.UnderlyerSpecified))
            {
                if ((0 == pPosRequestDetOption.Underlyer.IdAsset) && Asset.idAsset_UnderlyerSpecified)
                {
                    pPosRequestDetOption.Underlyer.IdAsset = Asset.idAsset_Underlyer;
                    pPosRequestDetOption.Underlyer.Identifier = Asset.identifier_Underlyer;
                    pPosRequestDetOption.Underlyer.AssetCategory = Asset.assetCategory_Underlyer;
                }
                codeReturn = EventQuery.InsertEventAsset(pDbTransaction, pIdE, efs_Asset);
            }
            #endregion Insertion EVENTASSET avec info du sous-jacent

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                codeReturn = m_EventQuery.InsertEventDet_Denouement(pDbTransaction, pIdE, pQty, Asset.contractMultiplier,
                    strikePrice, _payrec.QuoteSide, _payrec.QuoteTiming, _payrec.DtSettlementPrice, _payrec.SettlementPrice, _payrec.SettlementPrice100, null);
            }

            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (0 < pIdPADET))
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            #endregion EVENT SCU (Cash Settlement)

            return codeReturn;
        }
        #endregion InsertSettlementCurrencyEvent

        #region OptionUpdating
        // EG 20130613 [18751] Purge de l'éventuelle demande de LIVRAISON (UNLDLVR) suite à annulation de dénouement manuel (Qty = 0)
        // EG 20151125 [20979] Refactoring
        // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
        // EG 20230929 [WI715][26497] Call PosKeepingTools.SetPosRequestUnderlyerDelivery instead of AddPosRequestUnderlyerDelivery
        // EG 20230929 [WI715][26497] Call PosKeepingTools.DeletePosRequestUnderlyerDelivery instead of DeletePosRequestUnderlyerDelivery
        private Cst.ErrLevel OptionUpdating(IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = pDbTransaction;
            bool isDbTransactionSelfCommit = (null == pDbTransaction);
            bool isException = false;
            try
            {
                if (isDbTransactionSelfCommit)
                    dbTransaction = DataHelper.BeginTran(CS);

                // Delete POSACTION
                // EG 20151125 [20979] Refactoring
                // EG 20160121 [21805][POC-MUREX]
                //Nullable<int> idPR = GetIdPR();
                //if (idPR.HasValue)
                //    codeReturn = DeleteAllPosActionAndLinkedEventByRequest(dbTransaction, idPR.Value);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    IPosRequestOption _posRequestOption = m_PosRequest as IPosRequestOption;
                    // PM 20150225 [POC] Ajout EquityAsset
                    bool isUnderlyerDelivery = (_posRequestOption.Detail.UnderlyerSpecified && (false == IsRequestOptionAbandon) &&
                        ((_posRequestOption.Detail.Underlyer.AssetCategory == Cst.UnderlyingAsset.Future) ||
                        (_posRequestOption.Detail.Underlyer.AssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ||
                        (_posRequestOption.Detail.Underlyer.AssetCategory == Cst.UnderlyingAsset.EquityAsset)));
                    DataTable dtChanged = m_DtPosActionDet.GetChanges();
                    if (null != dtChanged)
                    {
                        m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                        DataRow rowTradeClosing = null;
                        #region GetId of POSACTION/POSACTIONDET/EVENT
                        int nbOfTokenIDPADET = dtChanged.Rows.Count;
                        // IDE = 9 EVTs potentiels
                        // . ABN-ASS-EXE/TOT-PAR
                        // . TER-INT/NOM
                        // . TER-INT/QTY
                        // . TER-INT/SCU
                        // . LPP/VMG
                        // . LPP/RMG
                        // . ABN/PAR si ASS-EXE/PAR et abandon de la quantité restante
                        // . TER/NOM si ASS-EXE/PAR et abandon de la quantité restante
                        // . TER/QTY si ASS-EXE/PAR et abandon de la quantité restante
                        int newIdPADET = 0;
                        int newIdE = 0;
                        codeReturn = SQLUP.GetId(out int newIdPA, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                        //if (Cst.ErrLevel.SUCCESS == codeReturn)
                        //    codeReturn = SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTIONDET, SQLUP.PosRetGetId.First, nbOfTokenIDPADET);
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            newIdPADET = newIdPA;
                            codeReturn = SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbOfTokenIDPADET * m_PosRequest.NbTokenIdE);
                        }
                        #endregion GetId of POSACTION/POSACTIONDET/EVENT

                        // EG 20151125 [20979] Refactoring
                        Nullable<int> idPR = GetIdPR();
                        if (idPR.HasValue)
                            codeReturn = DeleteAllPosActionAndLinkedEventByRequest(dbTransaction, idPR.Value);

                        #region Insertion dans POSACTION
                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            InsertPosAction(dbTransaction, newIdPA, DtBusiness, m_PosRequest.IdPR);
                        }
                        #endregion Insertion dans POSACTION

                        if (Cst.ErrLevel.SUCCESS == codeReturn)
                        {
                            #region Lecture des lignes de POSACTIONDET post traitement
                            foreach (DataRow row in dtChanged.Rows)
                            {
                                // On récupère le DataRow de la négociation clôturante
                                if (row.RowState == DataRowState.Added)
                                {
                                    // Nouvelle clôture 
                                    rowTradeClosing = m_DtPosition.Rows.Find(row["IDT_CLOSING", DataRowVersion.Current]);
                                    codeReturn = InsertPosActionDetAndOptionLinkedEvent(dbTransaction, newIdPA, newIdPADET, ref newIdE, row, rowTradeClosing);
                                    // Insertion POSREQUEST pour INSERTION TRADE sur SOUS-JACENT
                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                    {
                                        // EG 20130513 Plus d'insertion si le sous-jacent n'est pas un future
                                        // PM 20150225 [POC] ou un EquityAsset
                                        if (isUnderlyerDelivery)
                                        {
                                            string eventClass = rowTradeClosing["EVENTCLASS"].ToString();
                                            if (EventClassFunc.IsPhysicalSettlement(eventClass))
                                            {
                                                // EG 20160127 [21805] Test autorisé sur produit sous-jacent
                                                //codeReturn = AddPosRequestUnderlyerDelivery(dbTransaction);
                                                IPosRequestDetOption requestOption = (IPosRequestDetOption)((IPosRequestOption)m_PosRequest).Detail;
                                                if (requestOption.UnderlyerSpecified)
                                                {
                                                    SQL_Instrument sqlInstrument = new SQL_Instrument(CS, requestOption.Underlyer.IdI)
                                                    {
                                                        DbTransaction = dbTransaction
                                                    };
                                                    if (sqlInstrument.IsLoaded && sqlInstrument.IsEnabled)
                                                    {
                                                        // EG 20230929 [WI715][26497] 
                                                        //codeReturn = AddPosRequestUnderlyerDelivery(dbTransaction);
                                                        codeReturn = PosKeepingTools.SetPosRequestUnderlyerDelivery(CS, dbTransaction, m_Product, m_PosRequest, m_PKGenProcess.Session);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                                //newIdPADET++;
                                //newIdE++;
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    codeReturn = SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                                    newIdE++;
                                }
                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                {
                                    isException = true;
                                    break;
                                }
                            }
                            #endregion Lecture des lignes de POSACTIONDET post traitement
                        }
                    }
                    else
                    {
                        // EG 20151125 [20979] Refactoring
                        Nullable<int> idPR = GetIdPR();
                        if (idPR.HasValue)
                            codeReturn = DeleteAllPosActionAndLinkedEventByRequest(dbTransaction, idPR.Value);

                        /// EG 20130613 [18751] Purge de l'éventuelle demande de LIVRAISON dans le cas où
                        // nous sommes en présence d'une annulation de dénouement manuel (Qty = 0)
                        if (isUnderlyerDelivery && (0 == _posRequestOption.Qty))
                        {
                            m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                            DataRow rowTradeClosing2 = m_DtPosition.Rows.Find(_posRequestOption.IdT);
                            if (null != rowTradeClosing2)
                            {
                                string eventClass = rowTradeClosing2["EVENTCLASS"].ToString();
                                if (EventClassFunc.IsPhysicalSettlement(eventClass))
                                    // EG 20230929 [WI715][26497]
                                    //codeReturn = DeletePosRequestUnderlyerDelivery(dbTransaction);
                                    codeReturn = PosKeepingTools.DeletePosRequestUnderlyerDelivery(CS, dbTransaction, m_Product, m_PosRequest, m_PKGenProcess.Session.IdA);
                            }
                        }
                    }
                }
                else
                {
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }

                if (isDbTransactionSelfCommit)
                {
                    if (false == isException)
                        DataHelper.CommitTran(dbTransaction);
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
                if (isDbTransactionSelfCommit)
                {
                    if (null != dbTransaction)
                    {
                        if (isException)
                            DataHelper.RollbackTran(dbTransaction);
                        dbTransaction.Dispose();
                    }
                }
            }
        }
        #endregion OptionUpdating

        #region PositionOptionGen
        /// <summary>
        /// Traitement d'un denoument d'option manuel qui s'applique sur une position
        /// <para>
        /// - Il s'agit de créer un enregistrement ds POSREQUEST par trade impliqué 
        /// </para>
        /// <para>
        /// - Il s'agit ensuite de traiter les POSREQUEST précédemment créés
        /// </para>
        /// </summary>
        /// <param name="pIdPR">Représente un denoument d'option manuel qui s'applique sur une position (Enregistrement POSREQUEST où IDT is null)</param>
        /// <param name="pIdPRGrpLevel">Représente le POSREQUEST de regroupement 'Traitement des denouements manuel d'otion' (généré par le traitement EOD)</param>
        /// FI 20130917 [18953] Modification de la signature de la fonction, rename de la méthode en PositionOptionGen
        // EG 20180307 [23769] Gestion List<IPosRequest>
        private Cst.ErrLevel PositionOptionGen(int pIdPR, Nullable<int> pIdPRGrpLevel)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            m_PosRequest = PosKeepingTools.GetPosRequest(CS, m_Product, pIdPR);

            Boolean isModeEOD = (m_PosRequest.RequestMode == SettlSessIDEnum.EndOfDay);
            Boolean isModeITD = (m_PosRequest.RequestMode == SettlSessIDEnum.Intraday);

            try
            {

                m_PosRequest.IdAUpdSpecified = true;
                m_PosRequest.IdAUpd = m_PKGenProcess.Session.IdA;
                InitStatusPosRequest(m_PosRequest, m_PosRequest.IdPR, pIdPRGrpLevel, ProcessStateTools.StatusProgressEnum, m_LstSubPosRequest);

                codeReturn = CheckPosRequestPositionOptionGen();


                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = InitPosKeepingData(null, m_PosRequest.PosKeepingKey, m_PosRequest.IdEM, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset, false);

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = ExecPosRequestPositionOptionGen();

            }
            catch (Exception ex)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw new Exception(StrFunc.AppendFormat("Exception on POSREQUEST (id:{0})", m_PosRequest.IdPR), ex);
            }
            finally
            {
                UpdatePosRequest(codeReturn, m_PosRequest, pIdPRGrpLevel);
                // FI 20181219 [24399] add test valeur null 
                // FI 20181213 [24393] Code "verrue" pour mettre à jour le status 
                // m_PosRequest étant clôner dans ExecChildPosRequestPositionOptionGen la mise a jour ne se fait pas dans la liste
                // GLOP A revoir pour faire autrement 
                if (null != m_LstSubPosRequest)
                {
                    IPosRequest posrequest = (from item in m_LstSubPosRequest.Where(x => x.IdPR == m_PosRequest.IdPR)
                                              select item).FirstOrDefault();
                    if (null != posrequest)
                    {
                        posrequest.StatusSpecified = m_PosRequest.StatusSpecified;
                        if (posrequest.StatusSpecified)
                            posrequest.Status = m_PosRequest.Status;
                    }
                }
            }

            return codeReturn;
        }
        #endregion PositionOptionGen
        #region PosRequestTradeOptionGen
        /// <summary>
        /// Traitement d'un denouments d'option saisi manuellement qui s'applique à un trade 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="dr">Représente un enregistrement de POSREQUEST</param>
        /// <param name="pIdPRGrpLevel">Represente le POSREQUEST parent"</param>
        /// FI 20130917 [18953] en Mode Intraday le prix du ssjacent n'est pas nécessaire
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel PosRequestTradeOptionGen(IDbTransaction pDbTransaction, DataRow dr, int pIdPRGrpLevel)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            int idPR = Convert.ToInt32(dr["IDPR"]);
            m_PosRequest = PosKeepingTools.GetPosRequest(CS, pDbTransaction, m_Product, idPR);

            try
            {
                bool isReadQuote = (m_PosRequest.Qty > 0);
                codeReturn = InitPosKeepingData(m_PosRequest.IdT, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset, isReadQuote);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    m_PosRequest.PosKeepingKey = PosKeepingTools.SetPosKeepingKey(m_Product, (IPosKeepingKey)PosKeepingData);
                    m_PosRequest.PosKeepingKeySpecified = true;
                    if (RequestCanBeExecuted(Cst.PosRequestTypeEnum.UpdateEntry, m_PosRequest.IdEM, m_PosRequest.PosKeepingKey))
                    {
                        m_PosRequest.IdAUpdSpecified = true;
                        m_PosRequest.IdAUpd = m_PKGenProcess.Session.IdA;
                        InitStatusPosRequest(m_PosRequest, m_PosRequest.IdPR, pIdPRGrpLevel, ProcessStateTools.StatusProgressEnum, m_LstSubPosRequest);

                        if ((m_PosRequest.Qty == 0) || (m_PosRequest.RequestMode == SettlSessIDEnum.Intraday))
                        {
                            // FI 20130917 [18953]
                            // Spheres® poursuit le traitement 
                            // Si m_PosRequest.qty ==0 (Annulation d'un POSREQUEST)
                            // => Il n'est pas nécessaire de connaître la cotation du ss-jacent
                            // Si Mode intraday
                            // => Il n'est pas nécessaire de connaître la cotation du ss-jacent
                            codeReturn = Cst.ErrLevel.SUCCESS;
                        }
                        else if (IsQuoteOk)
                        {
                            // FI/EG 20130917 [18953]
                            // Spheres® poursuit le traitement uniquement si la cotation du ss-jacent est connue
                            // C'est peut-être abusif (En mode EOD, le prix sera de toute façon présent)
                            codeReturn = Cst.ErrLevel.SUCCESS;
                        }
                        else
                        {
                            codeReturn = Cst.ErrLevel.QUOTENOTFOUND;

                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5188), 0,
                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(LogTools.IdentifierAndId(PosKeepingData.Asset.identifier, PosKeepingData.Asset.idAsset)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness)),
                                new LogParam(LogTools.IdentifierAndId(PosKeepingData.Trade.Identifier, m_PosRequest.IdT)),
                                new LogParam(m_PosRequest.Qty),
                                new LogParam(Asset.GetLogQuoteInformationRelativeTo(m_PosRequest))));
                        }

                        if (codeReturn == Cst.ErrLevel.SUCCESS)
                        {
                            DeserializeTrade(m_PosRequest.IdT);
                            codeReturn = TradeOptionGen(pDbTransaction, false);
                        }
                    }
                    else
                    {
                        codeReturn = Cst.ErrLevel.DATAREJECTED;
                    }
                }
            }
            catch (Exception ex)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                throw new Exception(StrFunc.AppendFormat("Exception on POSREQUEST (id:{0})", m_PosRequest.IdPR), ex);
            }
            finally
            {
                UpdatePosRequest(pDbTransaction, codeReturn, m_PosRequest, pIdPRGrpLevel);
            }

            return codeReturn;
        }
        #endregion PosRequestTradeOptionGen

        #region RecordCascadingShiftingTrade
        /// <summary>
        /// Génération d'un trade post cascading ou shifting.
        /// </summary>
        /// <param name="pPosRequest">Demande de cascading ou de Shifting</param>
        /// <param name="pIdT">Id en retour du trade généré</param>
        /// <returns>Cst.ErrLevel</returns>
        /// FI 20161206 [22092] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Add identifier
        private Cst.ErrLevel RecordCascadingShiftingTrade(IDbTransaction pDbTransaction, IPosRequestCascadingShifting pPosRequest, out int pIdT)
        {
            try
            {
                IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[pPosRequest.PosKeepingKey.IdI];
                List<Pair<int, string>> TradeLinkInfo = new List<Pair<int, string>>
                {
                    new Pair<int, string>(pPosRequest.IdT, pPosRequest.Identifiers.Trade)
                };
                Cst.ErrLevel codeReturn = RecordTrade(pDbTransaction, pPosRequest.Detail.DataDocument, additionalInfo, null, pPosRequest.RequestType, TradeLinkInfo, 
                    out int idT, out string identifier);
                pIdT = idT;
                return codeReturn;
            }
            catch (Exception)
            {
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5180), 0,
                    new LogParam(GetPosRequestLogValue(pPosRequest.RequestType)),
                    new LogParam(LogTools.IdentifierAndId(pPosRequest.Identifiers.Trade, pPosRequest.IdT)),
                    new LogParam(LogTools.IdentifierAndId(pPosRequest.Detail.IdentifierDCDest, pPosRequest.Detail.IdDCDest)),
                    new LogParam(pPosRequest.Detail.MaturityMonthYearDest)));

                throw;
            }
        }
        #endregion RecordCascadingShiftingTrade
        #region RecordUnderlyingEvent
        /// <summary>
        /// Génération des événements du trade sous-jacent inséré.
        /// Attention le message Queue est ici un POSREQUESTMQUEUE et non un EVENTSGENMQUEUE. 
        /// L'accesseur currentId de cette queue est alimenté par l'identifiant du trade retourné par l'insertion du sous-jacent.
        /// (NB: L'identifiant d'origine était celui de l'option source du dénouement)
        /// </summary>
        /// <param name="pUnderlyer">Information de sous-jacent</param>
        /// <param name="pIdT">Id du trade sous-jacent inséré</param>
        /// <returns></returns>
        /// EG 20141103 Add Test on Cst.UnderlyingAsset.ExchangeTradedContract
        // EG 20180221 [23769] Public pour gestion asynchrone
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel RecordUnderlyingEvent(IPosRequestDetUnderlyer pUnderlyer, int pIdT)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            // PM 20150304 [POC] Ajout EquityAsset
            if ((pUnderlyer.AssetCategory == Cst.UnderlyingAsset.Future) ||
                (pUnderlyer.AssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ||
                (pUnderlyer.AssetCategory == Cst.UnderlyingAsset.EquityAsset))
            {
                string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(CS, pIdT);
                codeReturn = RecordTradeEvent(pIdT, tradeIdentifier);
                if (false == ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 5162), 0,
                        new LogParam(LogTools.IdentifierAndId(tradeIdentifier, pIdT)),
                        new LogParam(LogTools.IdentifierAndId(pUnderlyer.Identifier, pUnderlyer.IdAsset)),
                        new LogParam(pUnderlyer.AssetCategory)));
                }
            }
            return codeReturn;
        }
        #endregion RecordUnderlyingEvent
        #region RecordUnderlyingTrade
        /// <summary>
        ///  Création du trade matérialisant la livraison du sous-jacent
        /// </summary>
        /// <param name="pRequestOption"></param>
        /// <param name="pIdT">Retourne l'IdT du trade généré</param>
        /// <returns></returns>
        /// FI 20161206 [22092] Modify
        // EG 20180221 [23769] Public pour gestion asynchrone
        // EG 20190613 [24683] Add identifier
        public Cst.ErrLevel RecordUnderlyingTrade(IPosRequestOption pRequestOption, out int pIdT)
        {
            IPosRequestDetUnderlyer underlyer = pRequestOption.Detail.Underlyer;
            try
            {
                IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[underlyer.IdI];


                // FI 20161206 [22092] Modify calcul des frais => Mise à jour de underlyer.dataDocument 
                TradeInput tradeInput = new TradeInput
                {
                    SQLProduct = additionalInfo.SqlProduct,
                    SQLTrade = additionalInfo.SqlTemplateTrade,
                    DataDocument = underlyer.DataDocument
                };
                tradeInput.TradeStatus.Initialize(ProcessBase.Cs, null, additionalInfo.SqlTemplateTrade.IdT);
                //PL 20200217 [25207] 
                //tradeInput.RecalculFeeAndTax(CS, null, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_DLV));
                string action = "OTC_INP_TRD_OPTEXE_OPTASS";
                if (tradeInput.Product.GetTrdSubType() == TrdSubTypeEnum.TransactionFromExercise)
                    action = "OTC_INP_TRD_OPTEXE";
                else if (tradeInput.Product.GetTrdSubType() == TrdSubTypeEnum.TransactionFromAssignment)
                    action = "OTC_INP_TRD_OPTASS";
                tradeInput.RecalculFeeAndTax(CS, null, action);

                List<Pair<int, string>> tradeLinkInfo = new List<Pair<int, string>>
                {
                    new Pair<int, string>(m_PosRequest.IdT, PosKeepingData.Trade.Identifier)
                };

                Cst.ErrLevel codeReturn = RecordTrade(null, underlyer.DataDocument, additionalInfo, pRequestOption.Detail.Underlyer.GetIdPRSource(),
        pRequestOption.Detail.Underlyer.RequestTypeSource, tradeLinkInfo, out int idT, out string identifier);

                pIdT = idT;

                return codeReturn;
            }
            catch (Exception)
            {
                string sourceIdentifier = string.Empty;
                if (PosKeepingData.TradeSpecified)
                    sourceIdentifier = PosKeepingData.Trade.Identifier;

                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5159), 0,
                    new LogParam(LogTools.IdentifierAndId(sourceIdentifier, PosKeepingData.Trade.IdT)),
                    new LogParam(LogTools.IdentifierAndId(pRequestOption.Detail.Underlyer.Identifier, pRequestOption.Detail.Underlyer.IdAsset)),
                    new LogParam(pRequestOption.Detail.Underlyer.AssetCategory)));

                throw;
            }
        }
        #endregion RecordUnderlyingTrade
        #region RemoveChildPosRequestPositionOptionGen
        /// <summary>
        /// Purge des denouement d'option potentiellement réalisés lors d'un traitement précédent
        /// <para>Il s'agit de mettre à zéro la quantité sur les POSREQUEST des denouement d'option par trade associés</para>
        /// <para>Il s'agit d'exécuter les POSREQUEST précédemment mis à zéro et de les supprimer ensuite</para>
        /// </summary>
        // EG 20151019 [21112] Add pDbTransaction parameter
        private Cst.ErrLevel RemoveChildPosRequestPositionOptionGen(IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                string note = "Remove of previous action";

                string sqlUpdate = @"update dbo.POSREQUEST 
                set STATUS = 'PENDING', QTY = 0, IDAUPD = @IDA , DTUPD = @DTUPD , NOTES = @NOTE, IDPROCESS_L = @IDPROCESS_L  
                where IDPR_POSREQUEST=@IDPR";

                DataParameters dp = new DataParameters();
                AddParameter_IDPR(dp);
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDA), m_PKGenProcess.Session.IdA);
                
                //dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDPROCESS_L), LogHeader.IdProcess);
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDPROCESS_L), IdProcess);
                // FI 20200820 [25468] dates systemes en UTC
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTUPD), OTCmlHelper.GetDateSysUTC(CS));
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.NOTE), note);

                QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, dp);
                int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (0 < rowAffected)
                    codeReturn = ExecChildPosRequestPositionOptionGen(pDbTransaction);

                if (Cst.ErrLevel.SUCCESS == codeReturn && rowAffected > 0)
                {
                    string sqlDelete = @"delete from dbo.POSREQUEST where IDPR_POSREQUEST=@IDPR";
                    dp = new DataParameters();
                    AddParameter_IDPR(dp);
                    qryParameters = new QueryParameters(CS, sqlDelete, dp);
                    DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occurred when removing previous execution", ex);
            }
            return codeReturn;
        }
        #endregion RemoveChildPosRequestPositionOptionGen
        #region RequestOnContractCanBeExecuted
        /// <summary>
        /// Retourne si une demande peut-être exécutée par lecture des STATUT des
        /// traitements EOD matchant avec la liste passé en paramètre
        /// avec l'entité/marché ainsi que pour toutes les clé de position 
        /// constituées via l'IDASSET passé en paramètre
        /// (stockés dans m_LstSubPosRequest)
        /// </summary>
        /// <param name="pListRequestType">RequestType (List)</param>
        /// <param name="pIdEM">Id Entité/Marché</param>
        /// <param name="pPosKeepingKey">Clé de position</param>
        /// <returns>boolean</returns>
        // EG 20170206 [22787] RequestOnContractCanBeExecuted instead of RequestMOFCanBeExecuted
        // EG 20180425 Analyse du code Correction [CA2202]
        private bool RequestOnContractCanBeExecuted(Cst.PosRequestTypeEnum pListRequestType, int pIdEM, IPosKeepingKey pPosKeepingKey)
        {
                bool ret = true;
                DataParameters parameters = new DataParameters(new DataParameter[] { });
                parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pPosKeepingKey.IdAsset);

                string sqlSelect = SQLCst.SELECT + "ast.IDASSET, dc.IDI" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " ast" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB + " da" + SQLCst.ON + "(da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + SQLCst.ON + "(dc.IDDC = da.IDDC)" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(ast.DA_IDASSET = @IDASSET)" + Cst.CrLf;

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
        #endregion RequestOnContractCanBeExecuted

        #region SetDataDocument
        /// <summary>
        ///  Alimente pRequestOption.detail.underlyer.dataDocument (trade ss-jacent) 
        /// </summary>
        /// <param name="pRequestOption"></param>
        /// <param name="pPrice"></param>
        /// <returns></returns>
        /// PM 20150207 [POC] Nouvelle méthode en remplacement de celle exitante afin de traiter ExchangeTradedDerivative et EquitySecurityTransaction
        // EG 20180221 [23769] Public pour gestion asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        public Cst.ErrLevel SetDataDocument(IDbTransaction pDbTransaction, IPosRequestOption pRequestOption, decimal pPrice)
        {
            IPosRequestDetUnderlyer underlyer = pRequestOption.Detail.Underlyer;
            IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[underlyer.IdI];
            
            // Désérialisation du trade option à l'origine du trade sous-jacent à créer
            DeserializeTrade(pDbTransaction, underlyer.IdTSource);
            
            // Recopie du dataDocument template du sous-jacent
            underlyer.DataDocument = (DataDocumentContainer)additionalInfo.TemplateDataDocument.Clone();
            
            DataDocumentContainer dataDocUnl = underlyer.DataDocument;

            Cst.ErrLevel codeReturn;
            if (dataDocUnl.CurrentProduct.IsExchangeTradedDerivative)
            {
                codeReturn = SetDataDocumentEtd(pDbTransaction, pRequestOption, pPrice, dataDocUnl);
            }
            else if (dataDocUnl.CurrentProduct.IsEquitySecurityTransaction)
            {
                codeReturn = SetDataDocumentEquityTransaction(pDbTransaction, pRequestOption, pPrice, dataDocUnl);
            }
            else
            {
                codeReturn = Cst.ErrLevel.DATAREJECTED;
            }
            return codeReturn;
        }
        #endregion SetDataDocument

        #region SetDataDocumentEtd
        /// <summary>
        ///  Complete le DataDocument dans le cas d'un ExchangeTradedDerivative
        /// </summary>
        /// <param name="pRequestOption"></param>
        /// <param name="pPrice"></param>
        /// <param name="pDataDocument"></param>
        /// <returns></returns>
        // PM 20150227 [POC] Nouvelle méthode
        // EG 20171107 [23509] Upd
        // EG 20180307 [23769] Gestion dbTransaction
        private Cst.ErrLevel SetDataDocumentEtd(IDbTransaction pDbTransaction, IPosRequestOption pRequestOption, decimal pPrice, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (pDataDocument.CurrentProduct.IsExchangeTradedDerivative)
            {
                // Désérialisation du trade option à l'origine du trade sous-jacent à créer
                DataDocumentContainer dataDocOption = m_TradeLibrary.DataDocument;
                IExchangeTradedDerivative etdOption = (IExchangeTradedDerivative)dataDocOption.CurrentProduct.Product;
                IFixTrdCapRptSideGrp rptSideOption = (IFixTrdCapRptSideGrp)etdOption.TradeCaptureReport.TrdCapRptSideGrp[0];
                //
                IExchangeTradedDerivative etdFuture = (IExchangeTradedDerivative)pDataDocument.CurrentProduct.Product;
                IFixTradeCaptureReport trdCapRptUnl = (IFixTradeCaptureReport)etdFuture.TradeCaptureReport;
                IFixTrdCapRptSideGrp rptSideUnl = (IFixTrdCapRptSideGrp)etdFuture.TradeCaptureReport.TrdCapRptSideGrp[0];
                //
                pDataDocument.PartyTradeIdentifier = dataDocOption.PartyTradeIdentifier;
                pDataDocument.PartyTradeInformation = dataDocOption.PartyTradeInformation;
                pDataDocument.PartyTradeInformationSpecified = dataDocOption.PartyTradeInformationSpecified;
                pDataDocument.Party = dataDocOption.Party;
                pDataDocument.BrokerPartyReferenceSpecified = dataDocOption.BrokerPartyReferenceSpecified;
                pDataDocument.BrokerPartyReference = dataDocOption.BrokerPartyReference;
                pDataDocument.GoverningLaw = dataDocOption.GoverningLaw;
                pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;
                pDataDocument.CalculationAgent = dataDocOption.CalculationAgent;
                pDataDocument.CalculationAgentSpecified = dataDocOption.CalculationAgentSpecified;
                pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;
                //
                pDataDocument.OtherPartyPaymentSpecified = false;

                // Mise à jour de l'asset sous jacent
                SQL_AssetETD assetETD = new SQL_AssetETD(CS, pRequestOption.Detail.Underlyer.IdAsset)
                {
                    DbTransaction = pDbTransaction
                };
                if (assetETD.IsLoaded)
                {
                    ExchangeTradedDerivativeTools.SetFixInstrumentFromETDAsset(CS, pDbTransaction, assetETD, CfiCodeCategoryEnum.Future,
                        trdCapRptUnl.Instrument, null);
                }
                // Mise à jour des données (Prix du sous-jacent, quantié, ...)
                trdCapRptUnl.LastPx = new EFS_Decimal(pPrice);
                trdCapRptUnl.LastPxSpecified = true;
                trdCapRptUnl.LastQty = new EFS_Decimal(pRequestOption.Qty);
                trdCapRptUnl.LastQtySpecified = true;

                trdCapRptUnl.TrdType = TrdTypeEnum.OptionExercise;
                trdCapRptUnl.TrdTypeSpecified = true;
                if ((pRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.OptionExercise) ||
                    (pRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.AutomaticOptionExercise))
                {
                    etdFuture.TradeCaptureReport.TrdSubType = TrdSubTypeEnum.TransactionFromExercise;
                }
                else
                {
                    etdFuture.TradeCaptureReport.TrdSubType = TrdSubTypeEnum.TransactionFromAssignment;
                }
                trdCapRptUnl.TrdSubTypeSpecified = true;
                trdCapRptUnl.ExecType = ExecTypeEnum.TriggeredOrActivatedBySystem;

                rptSideUnl.Account = rptSideOption.Account;
                rptSideUnl.AccountSpecified = rptSideOption.AccountSpecified;
                rptSideUnl.AccountType = rptSideOption.AccountType;
                rptSideUnl.AccountTypeSpecified = rptSideOption.AccountTypeSpecified;
                rptSideUnl.AcctIdSource = AcctIDSourceEnum.Other;
                rptSideUnl.AcctIdSourceSpecified = rptSideOption.AcctIdSourceSpecified;
                rptSideUnl.ClrInstGrp = null;
                rptSideUnl.ExecRefIdSpecified = false;
                rptSideUnl.OrderIdSpecified = false;
                rptSideUnl.OrderInputDeviceSpecified = false;
                rptSideUnl.OrdTypeSpecified = rptSideOption.OrdTypeSpecified;
                rptSideUnl.Parties = rptSideOption.Parties;
                rptSideUnl.PositionEffect = FixML.v50SP1.Enum.PositionEffectEnum.Open;
                rptSideUnl.PositionEffectSpecified = true;
                // le dealer est Acheteur de CALL (exercice)  alors il est Acheteur du Future
                // le dealer est Vendeur de PUT (assignation) alors il est Acheteur du Future
                if (((etdOption.TradeCaptureReport.Instrument.PutOrCall == PutOrCallEnum.Call) &&
                    (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromExercise)) ||
                    ((etdOption.TradeCaptureReport.Instrument.PutOrCall == PutOrCallEnum.Put) &&
                    (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromAssignment)))
                {
                    rptSideUnl.Side = SideEnum.Buy;
                }
                else
                {
                    rptSideUnl.Side = SideEnum.Sell;
                }
                rptSideUnl.SideSpecified = true;
                rptSideUnl.Text = "Delivery - Future";
                rptSideUnl.TextSpecified = true;

                // EG 20171107 [23509]
                DateTime tradeDate = pRequestOption.DtBusiness;
                if (0 < DateTime.Compare(tradeDate, assetETD.Maturity_LastTradingDay))
                    tradeDate = assetETD.Maturity_LastTradingDay;

                SetMarketFacility(pDbTransaction, pDataDocument);

                // RD 20171222 [23671] Business date is pRequestOption.dtBusiness
                //SetTradeDates(dataDocOption, pDataDocument, trdCapRptUnl, tradeDate, tradeDate);
                SetTradeDates(pDbTransaction, pDataDocument, trdCapRptUnl, tradeDate, pRequestOption.DtBusiness);

            }
            return codeReturn;
        }
        #endregion SetDataDocumentEtd
        #region SetDataDocumentEquityTransaction
        /// <summary>
        /// Complete le DataDocument dans le cas d'un EquitySecurityTransaction
        /// </summary>
        /// <param name="pRequestOption"></param>
        /// <param name="pPrice"></param>
        /// <param name="pDataDocument"></param>
        /// <returns></returns>
        // PM 20150227 [POC] Nouvelle méthode
        // EG 20171107 [23509] Upd
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetDataDocumentEquityTransaction(IDbTransaction pDbTransaction, IPosRequestOption pRequestOption, decimal pPrice, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (pDataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                DateTime dtBusiness = pRequestOption.DtBusiness;
                string CSCacheOn = CSTools.SetCacheOn(CS);
                // Désérialisation du trade option à l'origine du trade sous-jacent à créer
                DataDocumentContainer dataDocOption = m_TradeLibrary.DataDocument;
                IExchangeTradedDerivative etdOption = (IExchangeTradedDerivative)dataDocOption.CurrentProduct.Product;
                IFixTradeCaptureReport trdCapRptOption = (IFixTradeCaptureReport)etdOption.TradeCaptureReport;
                IFixTrdCapRptSideGrp rptSideOption = (IFixTrdCapRptSideGrp)etdOption.TradeCaptureReport.TrdCapRptSideGrp[0];
                IFixParty fixPartyDealer = RptSideTools.GetParty(rptSideOption, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                //
                IEquitySecurityTransaction equityTransaction = (IEquitySecurityTransaction)pDataDocument.CurrentProduct.Product;
                IFixTradeCaptureReport trdCapRptUnl = (IFixTradeCaptureReport)equityTransaction.TradeCaptureReport;
                IFixTrdCapRptSideGrp rptSideUnl = (IFixTrdCapRptSideGrp)trdCapRptUnl.TrdCapRptSideGrp[0];
                //
                IParty partyDealer = dataDocOption.Party.FirstOrDefault(p => p.Id == fixPartyDealer.PartyId.href);
                if (partyDealer != default(IParty))
                {
                    pDataDocument.RemoveParty();
                    SQL_Actor sqlActor = new SQL_Actor(CSCacheOn, partyDealer.PartyId)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (null != sqlActor)
                        pDataDocument.AddParty(sqlActor);
                    else
                        pDataDocument.AddParty(partyDealer);
                }

                IPartyTradeIdentifier partyIdentifierDealer = dataDocOption.PartyTradeIdentifier.FirstOrDefault(p => p.PartyReference.HRef == fixPartyDealer.PartyId.href);
                if (partyIdentifierDealer != default(IPartyTradeIdentifier))
                {
                    IPartyTradeIdentifier newPartyIdentifier = pDataDocument.AddPartyTradeIndentifier(fixPartyDealer.PartyId.href);
                    int posNewPartyIdentifier = Array.IndexOf(pDataDocument.PartyTradeIdentifier, newPartyIdentifier);
                    pDataDocument.PartyTradeIdentifier[posNewPartyIdentifier] = partyIdentifierDealer;
                }
                IPartyTradeInformation partyInformationDealer = dataDocOption.PartyTradeInformation.FirstOrDefault(p => p.PartyReference == fixPartyDealer.PartyId.href);
                if (partyInformationDealer != default(IPartyTradeInformation))
                {
                    IPartyTradeInformation newPartyInformation = pDataDocument.AddPartyTradeInformation(fixPartyDealer.PartyId.href);
                    int posNewPartyInformation = Array.IndexOf(pDataDocument.PartyTradeInformation, newPartyInformation);
                    pDataDocument.PartyTradeInformation[posNewPartyInformation] = partyInformationDealer;
                    pDataDocument.PartyTradeInformationSpecified = (pDataDocument.PartyTradeInformation.Count() != 0);
                }

                pDataDocument.BrokerPartyReferenceSpecified = dataDocOption.BrokerPartyReferenceSpecified;
                pDataDocument.BrokerPartyReference = dataDocOption.BrokerPartyReference;
                pDataDocument.GoverningLaw = dataDocOption.GoverningLaw;
                pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;
                pDataDocument.CalculationAgent = dataDocOption.CalculationAgent;
                pDataDocument.CalculationAgentSpecified = dataDocOption.CalculationAgentSpecified;
                pDataDocument.GoverningLawSpecified = dataDocOption.GoverningLawSpecified;

                pDataDocument.OtherPartyPaymentSpecified = false;

                // Mise à jour de l'asset sous jacent
                SQL_AssetEquity assetUnl = new SQL_AssetEquity(CSCacheOn, pRequestOption.Detail.Underlyer.IdAsset)
                {
                    DbTransaction = pDbTransaction
                };
                if (assetUnl.IsLoaded)
                {
                    EquitySecurityTransactionTools.SetFixInstrumentFromEquityAsset(CS, assetUnl, trdCapRptUnl.Instrument, trdCapRptUnl, pDataDocument);
                }
                // Mise à jour des données (prix, quantité, ...)
                trdCapRptUnl.LastPx = new EFS_Decimal(pPrice);
                trdCapRptUnl.LastPxSpecified = true;
                decimal quantity = pRequestOption.Qty;
                if ((Asset != null) && (Asset.factor != 0))
                {
                    quantity *= Asset.factor;
                }
                trdCapRptUnl.LastQty = new EFS_Decimal(quantity);
                trdCapRptUnl.LastQtySpecified = true;

                trdCapRptUnl.TrdType = TrdTypeEnum.OptionExercise;
                trdCapRptUnl.TrdTypeSpecified = true;
                if ((pRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.OptionExercise) ||
                    (pRequestOption.Detail.Underlyer.RequestTypeSource == Cst.PosRequestTypeEnum.AutomaticOptionExercise))
                {
                    trdCapRptUnl.TrdSubType = TrdSubTypeEnum.TransactionFromExercise;
                }
                else
                {
                    trdCapRptUnl.TrdSubType = TrdSubTypeEnum.TransactionFromAssignment;
                }
                trdCapRptUnl.TrdSubTypeSpecified = true;
                trdCapRptUnl.ExecType = ExecTypeEnum.TriggeredOrActivatedBySystem;

                rptSideUnl.Account = rptSideOption.Account;
                rptSideUnl.AccountSpecified = rptSideOption.AccountSpecified;
                rptSideUnl.AccountType = rptSideOption.AccountType;
                rptSideUnl.AccountTypeSpecified = rptSideOption.AccountTypeSpecified;
                rptSideUnl.AcctIdSource = AcctIDSourceEnum.Other;
                rptSideUnl.AcctIdSourceSpecified = rptSideOption.AcctIdSourceSpecified;
                rptSideUnl.ClrInstGrp = null;
                rptSideUnl.ExecRefIdSpecified = false;
                rptSideUnl.OrderIdSpecified = false;
                rptSideUnl.OrderInputDeviceSpecified = false;
                rptSideUnl.OrdTypeSpecified = rptSideOption.OrdTypeSpecified;
                // rptSideUnl.Parties
                if (rptSideUnl.Parties.Count() == 0)
                {
                    RptSideTools.AddParty(rptSideUnl);
                }
                rptSideUnl.Parties[0] = fixPartyDealer;
                IFixParty fixPartyCustodian = RptSideTools.AddParty(rptSideUnl);
                fixPartyCustodian.PartyRole = PartyRoleEnum.Custodian;
                fixPartyCustodian.PartyId.href = null;
                // rptSideUnl.PositionEffect
                SQL_Instrument sqlInstrument = new SQL_Instrument(CSCacheOn, pRequestOption.Detail.Underlyer.IdI)
                {
                    DbTransaction = pDbTransaction
                };
                if (sqlInstrument.FungibilityMode == FungibilityModeEnum.CLOSE)
                {
                    rptSideUnl.PositionEffect = FixML.v50SP1.Enum.PositionEffectEnum.Close;
                }
                else
                {
                    rptSideUnl.PositionEffect = FixML.v50SP1.Enum.PositionEffectEnum.Open;
                }
                rptSideUnl.PositionEffectSpecified = true;
                // le dealer est Acheteur de CALL (exercice)  alors il est Acheteur du sous-jacent
                // le dealer est Vendeur de PUT (assignation) alors il est Acheteur du sous-jacent
                if (((trdCapRptOption.Instrument.PutOrCall == PutOrCallEnum.Call) &&
                    (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromExercise)) ||
                    ((trdCapRptOption.Instrument.PutOrCall == PutOrCallEnum.Put) &&
                    (trdCapRptUnl.TrdSubType == TrdSubTypeEnum.TransactionFromAssignment)))
                {
                    rptSideUnl.Side = SideEnum.Buy;
                }
                else
                {
                    rptSideUnl.Side = SideEnum.Sell;
                }
                rptSideUnl.SideSpecified = true;
                rptSideUnl.Text = "Delivery - EquitySecurityTransaction";
                rptSideUnl.TextSpecified = true;
                // Recherche du Custodian
                EquitySecurityTransactionContainer equityTransactionContainer = new EquitySecurityTransactionContainer(equityTransaction, pDataDocument);
                ClearingTemplates clearingTemplates = new ClearingTemplates();
                clearingTemplates.Load(CSCacheOn, pDataDocument, equityTransactionContainer, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (ArrFunc.IsFilled(clearingTemplates.clearingTemplate))
                {
                    ClearingTemplate clearingTemplateFind = clearingTemplates.clearingTemplate[0];
                    if (clearingTemplateFind.idAClearerSpecified)
                    {
                        string bookClearerIdentifier = clearingTemplateFind.bookClearerIdentifier;
                        SQL_Actor sqlActor = new SQL_Actor(CSCacheOn, clearingTemplateFind.idAClearer)
                        {
                            DbTransaction = pDbTransaction
                        };
                        string custodianXmlId = sqlActor.XmlId;
                        pDataDocument.AddParty(sqlActor);
                        IPartyTradeIdentifier partyIdentifierCustodian = pDataDocument.AddPartyTradeIndentifier(custodianXmlId);
                        if (StrFunc.IsFilled(bookClearerIdentifier))
                        {
                            partyIdentifierCustodian.BookId.Value = bookClearerIdentifier;
                            partyIdentifierCustodian.BookIdSpecified = true;
                            if (clearingTemplateFind.bookClearerDisplayNameSpecified)
                            {
                                partyIdentifierCustodian.BookId.BookName = clearingTemplateFind.bookClearerDisplayName;
                            }
                            if (clearingTemplateFind.idBClearerSpecified)
                            {
                                partyIdentifierCustodian.BookId.OTCmlId = clearingTemplateFind.idBClearer;
                            }
                        }
                        RptSideTools.SetParty(fixPartyCustodian, custodianXmlId, PartyRoleEnum.Custodian);

                        // EG 20150622 On prend la date Business courante de la table ENTITYMARKET
                        DateTime dtSysBusiness = Tools.GetDateBusiness(CS, pDataDocument);
                        if (1 == dtSysBusiness.CompareTo(pRequestOption.DtBusiness))
                            dtBusiness = dtSysBusiness;
                    }
                }

                // EG 20171106 [23509]
                SetMarketFacility(pDbTransaction, pDataDocument);
                SetTradeDates(pDbTransaction, pDataDocument, trdCapRptUnl, pRequestOption.DtBusiness, dtBusiness);

                if (ArrFunc.IsEmpty(clearingTemplates.clearingTemplate) || (false == clearingTemplates.clearingTemplate[0].idAClearerSpecified))
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5165), 0,
                        new LogParam(LogTools.IdentifierAndId(assetUnl.Identifier, assetUnl.IdAsset)),
                        new LogParam(LogTools.IdentifierAndId(sqlInstrument.Identifier, sqlInstrument.IdI)),
                        new LogParam(LogTools.IdentifierAndId(partyDealer.PartyName, partyDealer.OTCmlId))));
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    #region GrossAmount
                    IPayment grossAmount = pDataDocument.CurrentProduct.ProductBase.CreatePayment();
                    //Payer\Receiver
                    if (rptSideUnl.Side == SideEnum.Buy)
                    {
                        grossAmount.PayerPartyReference.HRef = fixPartyDealer.PartyId.href;
                        grossAmount.ReceiverPartyReference.HRef = fixPartyCustodian.PartyId.href;
                    }
                    else
                    {
                        grossAmount.PayerPartyReference.HRef = fixPartyCustodian.PartyId.href;
                        grossAmount.ReceiverPartyReference.HRef = fixPartyDealer.PartyId.href;
                    }
                    //paymentAmount
                    Pair<Nullable<decimal>, string> priceGrossAmount = Tools.ConvertToQuotedCurrency(CSCacheOn, pDbTransaction, new Pair<Nullable<decimal>, string>(pPrice, assetUnl.IdC));

                    IMoney grossCalcAmount = pDataDocument.CurrentProduct.ProductBase.CreateMoney(quantity * priceGrossAmount.First.Value, priceGrossAmount.Second);
                    grossAmount.PaymentAmount.Amount.DecValue = grossCalcAmount.Amount.DecValue;
                    grossAmount.PaymentAmount.Currency = grossCalcAmount.Currency;
                    //Payement en date de livraison
                    // EG 20150622 Replace pRequestOption.dtBusiness by trdCapRptUnl.ClearingBusinessDate.DateValue
                    grossAmount.PaymentDate.UnadjustedDate.DateValue = GetDeliveryDate(trdCapRptUnl.ClearingBusinessDate.DateValue);
                    grossAmount.PaymentDate.DateAdjustments.BusinessDayConvention = BusinessDayConventionEnum.FOLLOWING;
                    grossAmount.PaymentDateSpecified = true;
                    equityTransaction.GrossAmount = grossAmount;
                    #endregion GrossAmount
                }
            }
            return codeReturn;
        }
        #endregion SetDataDocumentEquityTransaction
        #region SetMarketFacility
        // EG 20180307 [23769] Gestion dbTransaction
        public void SetMarketFacility(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument)
        {
            string CSCacheOn = CSTools.SetCacheOn(CS);
            string marketFacility = pDataDocument.GetFacilityFromMarket(CSCacheOn, pDbTransaction);
            if (StrFunc.IsFilled(marketFacility))
            {
                SQL_Market sql_Market = new SQL_Market(CSCacheOn, SQL_TableWithID.IDType.FIXML_SecurityExchange, marketFacility, SQL_Table.ScanDataDtEnabledEnum.No)
                {
                    DbTransaction = pDbTransaction
                };
                if (sql_Market.LoadTable())
                {
                    IParty party = pDataDocument.AddParty(sql_Market.XmlId);
                    Tools.SetParty(party, sql_Market);
                }
            }

            IParty partyFacility = pDataDocument.GetPartyFacility();
            if (null != partyFacility)
            {
                IPartyTradeInformation partyInformationFacility = pDataDocument.GetPartyTradeInformationFacility();
                if (null == partyInformationFacility)
                    pDataDocument.AddPartyTradeInformation(partyFacility.Id);
                pDataDocument.SetRelatedMarketVenue();
            }
        }
        #endregion SetMarketFacility
        #region SetTradeDates
        // EG 20171004 [23452] New
        // EG 20171016 [23509] Upd
        // EG 20171106 [23509] Upd
        // EG 20180307 [23769] Gestion dbTransaction
        public void SetTradeDates(IDbTransaction pDbTransaction, DataDocumentContainer pDestination, IFixTradeCaptureReport pTrdCapRpt, DateTime pTradeDate, DateTime pDtBusiness)
        {
            pDestination.TradeDate = pTradeDate;
            pTrdCapRpt.TradeDate = new EFS_Date
            {
                DateValue = pTradeDate
            };
            pTrdCapRpt.TradeDateSpecified = true;

            pTrdCapRpt.ClearingBusinessDate = new EFS_Date
            {
                DateValue = pDtBusiness
            };
            pTrdCapRpt.ClearingBusinessDateSpecified = true;
            pDestination.TradeHeader.ClearedDateSpecified = true;
            pDestination.TradeHeader.ClearedDate = pDestination.CurrentProduct.ProductBase.CreateAdjustedDate(pDtBusiness);


            IPartyTradeInformation partyTradeInformation = pDestination.GetPartyTradeInformationFacility();


            DateTime dtSys = OTCmlHelper.GetDateBusiness(CS, pDbTransaction);
            Nullable<DateTimeOffset> dtOrderEntered = Tz.Tools.FromTimeZone(dtSys, Tz.Tools.UniversalTimeZone);
            string orderEnteredValue = Tz.Tools.ToString(dtOrderEntered);

            DateTime time = new DateTime(new TimeSpan(23, 59, 59).Ticks);
            string executionDateTimeValue = Tz.Tools.AddTimeToDateReturnString(pTradeDate, time);

            if (Tz.Tools.IsDateFilled(orderEnteredValue))
            {
                partyTradeInformation.Timestamps = pDestination.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();
                partyTradeInformation.Timestamps.OrderEnteredSpecified = true;
                partyTradeInformation.Timestamps.OrderEntered = orderEnteredValue;
                partyTradeInformation.TimestampsSpecified = true;

                pTrdCapRpt.TransactTimeSpecified = true;
                pTrdCapRpt.TransactTime = new EFS_DateTimeOffset(orderEnteredValue);
                pTrdCapRpt.LastUpdateTimeSpecified = true;
                pTrdCapRpt.LastUpdateTime = new EFS_DateTimeOffset(orderEnteredValue);
                pTrdCapRpt.SetTradeRegulatoryTimeStamp(TrdRegTimestampTypeEnum.TimeIn, orderEnteredValue);
            }

            
            if (Tz.Tools.IsDateFilled(executionDateTimeValue))
            {
                partyTradeInformation.ExecutionDateTimeSpecified = true;
                partyTradeInformation.ExecutionDateTime = new SchemeData() { value = executionDateTimeValue, scheme = "http://www.iana.org/time-zones" };
                pTrdCapRpt.SetTradeRegulatoryTimeStamp(TrdRegTimestampTypeEnum.ExecutionTime, executionDateTimeValue);
            }

            pTrdCapRpt.TradedRegulatoryTimestampsSpecified = ArrFunc.IsFilled(pTrdCapRpt.TradedRegulatoryTimestamps);
        }
        #endregion SetTradeDates

        #region SetDataDocumentCascadingShifting
        /// <summary>
        /// Mise à jour du DataDocument de la demande de cascading ou de shifting
        /// </summary>
        /// <param name="pPosRequest">Demande de cascading ou de shifting</param>
        /// <returns>Cst.ErrLevel</returns>
        private Cst.ErrLevel SetDataDocumentCascadingShifting(IPosRequestCascadingShifting pPosRequest)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            DataDocumentContainer dataDoc = pPosRequest.Detail.DataDocument;
            IExchangeTradedDerivative etdCascading = (IExchangeTradedDerivative)dataDoc.CurrentProduct.Product;
            IFixTrdCapRptSideGrp rptSideCascading = (IFixTrdCapRptSideGrp)etdCascading.TradeCaptureReport.TrdCapRptSideGrp[0];

            dataDoc.OtherPartyPaymentSpecified = false;
            // Nouvelle date de compensation
            etdCascading.TradeCaptureReport.ClearingBusinessDate = new EFS_Date
            {
                DateValue = pPosRequest.Detail.DtExecution.Date
            };
            etdCascading.TradeCaptureReport.ClearingBusinessDateSpecified = true;

            // RD 20170127 [22795]
            etdCascading.TradeCaptureReport.LastQty = new EFS_Decimal(pPosRequest.Qty);
            etdCascading.TradeCaptureReport.LastQtySpecified = true;

            if (pPosRequest.RequestType == Cst.PosRequestTypeEnum.Cascading)
            {
                etdCascading.TradeCaptureReport.TrdType = TrdTypeEnum.Cascading;
                etdCascading.TradeCaptureReport.TrdTypeSpecified = true;
            }
            else if (pPosRequest.RequestType == Cst.PosRequestTypeEnum.Shifting)
            {
                etdCascading.TradeCaptureReport.TrdType = TrdTypeEnum.Shifting;
                etdCascading.TradeCaptureReport.TrdTypeSpecified = true;
            }
            etdCascading.TradeCaptureReport.ExecType = ExecTypeEnum.TriggeredOrActivatedBySystem;
            //
            rptSideCascading.Text = pPosRequest.RequestType.ToString();
            rptSideCascading.TextSpecified = true;
            //
            // Nouvelles charactéristiques de l'Asset
            etdCascading.TradeCaptureReport.Instrument.Symbol = pPosRequest.Detail.IdentifierDCDest;
            if (pPosRequest.RequestType == Cst.PosRequestTypeEnum.Cascading)
            {
                etdCascading.TradeCaptureReport.Instrument.MaturityMonthYear = pPosRequest.Detail.MaturityMonthYearDest;
            }
            // Supprimer la référence à l'ancien asset
            etdCascading.TradeCaptureReport.Instrument.SecurityId = null;
            return codeReturn;
        }
        #endregion SetDataDocumentCascadingShifting
        #region SetUnderlyingAssetETD
        /// <summary>
        /// Retourne l'asset future sous-jacent d'une option sur future, après le cas échéant l'avoir créé. 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAsset_Option">IDASSET de l'option</param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // PL 20180523 New feature (suite) [23968]
        private SQL_AssetETD SetUnderlyingAssetETD(string pCS, IDbTransaction pDbTransaction, int pIdAsset_Option)
        {
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);

            SQL_AssetETD sqlAsset_Future = null;

            SQL_AssetETD sqlAsset_Option = new SQL_AssetETD(pCS, pIdAsset_Option)
            {
                DbTransaction = pDbTransaction
            };
            sqlAsset_Option.LoadTable();

            //PL 20130613 Use DrvAttrib_IdAssetUnl
            if (sqlAsset_Option.DrvAttrib_IdAssetUnl != 0)
            {
                #region Contrat Future Sous-jacent paramétré sur l'échéance du contrat Option --> Lecture
                try
                {
                    sqlAsset_Future = new SQL_AssetETD(pCS, sqlAsset_Option.DrvAttrib_IdAssetUnl)
                    {
                        DbTransaction = pDbTransaction
                    };
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion
            }
            else
            {
                #region Contrat Future Sous-Jacent NON paramétré sur l'échéance Option --> Calcul et Création de cet Underlying Future, puis Lecture 
                SQL_DerivativeContract sqlFut_DC = new SQL_DerivativeContract(pCS, sqlAsset_Option.DrvContract_IdDerivativeContractUnl)
                {
                    DbTransaction = pDbTransaction
                };

                IExchangeTradedDerivative exchangeTradedDerivative = m_Product.CreateExchangeTradedDerivative();
                IProductBase productBase = (IProductBase)exchangeTradedDerivative;

                // PL 20180523 New feature (suite) [23968]
                #region Until v7.1
                //string maturityFuture = ExchangeTradedDerivativeTools.GetMaturityFutureFromMaturityOption(pCS, pDbTransaction, productBase, sqlAsset_Option.Maturity_MaturityMonthYear, 
                //                                                                                          sqlAsset_Option.DrvContract_ExerciseRule, sqlFut_DC);
                //if (StrFunc.IsFilled(maturityFuture))
                //{
                //    //PL 20130613 Below use m_MasterPosRequest.dtBusiness (2 times)
                //    SQL_Market sqlMarketFuture = new SQL_Market(pCS, sqlFut_DC.IdMarket);
                //    sqlMarketFuture.dbTransaction = pDbTransaction;
                //    FixInstrumentContainer fixInstrumentFuture = new FixInstrumentContainer(exchangeTradedDerivative.CreateFixInstrument());
                //    int idIFuture = sqlFut_DC.IdI;

                //    fixInstrumentFuture.SecurityExchange = sqlMarketFuture.Identifier;
                //    fixInstrumentFuture.Symbol = sqlFut_DC.Identifier;
                //    fixInstrumentFuture.MaturityMonthYear = maturityFuture;
                //    fixInstrumentFuture.PutOrCallSpecified = false;
                //    fixInstrumentFuture.StrikePriceSpecified = false;

                //    IDbTransaction dbTransaction_ = pDbTransaction;
                //    bool isException_ = false;
                //    try
                //    {
                //        if (null == dbTransaction_)
                //            dbTransaction_ = DataHelper.BeginTran(pCS);

                //        sqlAsset_Future = AssetTools.LoadAssetETD(pCS, dbTransaction_, idIFuture, CfiCodeCategoryEnum.Future, fixInstrumentFuture, m_MasterPosRequest.dtBusiness);
                //        if ((null == sqlAsset_Future) && (sqlFut_DC.IsAutoCreateAsset))
                //        {
                //            //Ajout de l'asset Future s'il n'existe pas
                //            string validationMsg_ = string.Empty; //Message de warning associé à la création de l'asset
                //            string infoMsg_ = string.Empty; //Message d'info des actions menées
                //            sqlAsset_Future = AssetTools.CreateAssetETD(pCS, dbTransaction_, productBase,
                //                                        fixInstrumentFuture.fixInstrument,
                //                                        sqlFut_DC.IdI, CfiCodeCategoryEnum.Future,
                //                                        this.ProcessBase.appInstance.IdA, m_MasterPosRequest.dtBusiness, out validationMsg_, out infoMsg_);
                //        }

                //        if (null != sqlAsset_Future)
                //        {
                //            //Mise à jour de l'échéance de l'asset option => update de l'échéance option avec l'asset future rattaché
                //            AssetTools.SetUnderlyingAssetETD(pCS, dbTransaction_, sqlAsset_Option.IdDerivativeAttrib, sqlAsset_Future.Id);
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        isException_ = true;
                //        throw;
                //    }
                //    finally
                //    {
                //        if ((null != dbTransaction_) && (null == pDbTransaction))
                //        {
                //            if (isException_)
                //                DataHelper.RollbackTran(dbTransaction_);
                //            else
                //                DataHelper.CommitTran(dbTransaction_);
                //        }
                //    }
                //}
                #endregion Until v7.1
                #region Onwards v7.2
                bool isException = false; 
                bool isLocalTransaction = (null == pDbTransaction);
                IDbTransaction dbTransaction = pDbTransaction;
                try
                {
                    if (isLocalTransaction)
                    {
                        dbTransaction = DataHelper.BeginTran(pCS);
                        sqlFut_DC.DbTransaction = dbTransaction; //NB: alimentation de la property "dbTransaction", car celle-ci est utilisée par la méthode "GetAssetFutureRelativeToMaturityOption()" pour y récupérer l'objet "Transaction".
                    }

                    //FI [20220629] call AssetTools.GetAssetFutureRelativeToMaturityOption à la place de ExchangeTradedDerivativeTools.GetAssetFutureRelativeToMaturityOption(
                    sqlAsset_Future = AssetTools.GetAssetFutureRelativeToMaturityOption(pCS, dbTransaction, productBase,  sqlAsset_Option,
                                                                                                            m_MasterPosRequest.DtBusiness, 
                                                                                                            this.ProcessBase.Session.IdA, dtSys,
                                                                                                            out _, out _, out bool _);
                    if (null != sqlAsset_Future)
                    {
                        //Mise à jour de l'échéance de l'asset Option --> On y renseigne l'échéance de l'asset Future sous-jacent qui s'y rapporte (qui donnera lieu à livraison)
                        AssetTools.SetUnderlyingAssetETD(pCS, dbTransaction, sqlAsset_Option.IdDerivativeAttrib, sqlAsset_Future.Id, this.ProcessBase.Session.IdA, dtSys);
                    }
                }
                catch (Exception)
                {
                    isException = true;
                    throw;
                }
                finally
                {
                    if (isLocalTransaction && (null != dbTransaction))
                    {
                        if (isException)
                            DataHelper.RollbackTran(dbTransaction);
                        else
                            DataHelper.CommitTran(dbTransaction);
                    }
                }
                #endregion Onwards v7.2
                #endregion
            }
            return sqlAsset_Future;
        }
        #endregion SetUnderlyingAssetETD
        #region SetTemplateDataDocument
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUnderlyer"></param>
        /// <returns></returns>
        /// EG 20141103 Add Test on ExchangeTradedContract
        /// PM 20150227 [POC] Add Test on EquityAsset
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public Cst.ErrLevel SetTemplateDataDocument(IDbTransaction pDbTransaction, IPosRequestDetUnderlyer pUnderlyer)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                string logMessage = string.Empty;
                if (false == m_TemplateDataDocumentTrade.ContainsKey(pUnderlyer.IdI))
                {
                    if ((pUnderlyer.AssetCategory == Cst.UnderlyingAsset.Future) || 
                        (pUnderlyer.AssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ||
                        (pUnderlyer.AssetCategory == Cst.UnderlyingAsset.EquityAsset))
                    {
                        SQL_Instrument sqlInstrument = new SQL_Instrument(CS, pUnderlyer.IdI)
                        {
                            DbTransaction = pDbTransaction
                        };
                        if (sqlInstrument.IsLoaded)
                        {
                            // PM 20150306 [POC] Ajout vérification que l'instrument est enable
                            if (sqlInstrument.IsEnabled)
                            {
                                string underlyerTemplateIdentifier = string.Empty;
                                string screenName = string.Empty;
                                using (IDataReader dr = GetTemplate(CS, pDbTransaction, pUnderlyer.IdI))
                                {
                                if (dr.Read())
                                {
                                        underlyerTemplateIdentifier = dr.GetValue(0).ToString();
                                        screenName = dr.GetValue(1).ToString();
                                    }
                                    else
                                    {
                                        codeReturn = Cst.ErrLevel.FAILURE;

                                        m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                        
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5155), 0,
                                            new LogParam(LogTools.IdentifierAndId(pUnderlyer.Identifier, pUnderlyer.IdAsset)),
                                            new LogParam(LogTools.IdentifierAndId(sqlInstrument.Identifier, sqlInstrument.IdI))));
                                    }
                                }

                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, underlyerTemplateIdentifier)
                                    {
                                        IsWithTradeXML = true,
                                        DbTransaction = pDbTransaction,
                                        IsAddRowVersion = true
                                    };
                                    if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML" }))
                                    {
                                        EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                                        IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                                        DataDocumentContainer dataDocContainer = new DataDocumentContainer(dataDoc);
                                        SQL_Product sqlProduct = new SQL_Product(CS, sqlInstrument.IdP)
                                        {
                                            DbTransaction = pDbTransaction
                                        };
                                        if (sqlProduct.IsLoaded)
                                        {
                                            IPosRequestTradeAdditionalInfo additionalInfo = pUnderlyer.CreateAdditionalInfo(dataDocContainer,
                                                sqlTrade, sqlProduct, sqlInstrument, screenName);
                                            m_TemplateDataDocumentTrade.Add(pUnderlyer.IdI, additionalInfo);
                                        }
                                    }

                                }
                                }
                            else
                            {
                                codeReturn = Cst.ErrLevel.DATAIGNORE;
                                
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 5157), 0,
                                    new LogParam(pUnderlyer.Identifier),
                                    new LogParam(pUnderlyer.AssetCategory)));
                            }
                        }
                        else
                        {
                            codeReturn = Cst.ErrLevel.FAILURE;
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5156), 0,
                                new LogParam(LogTools.IdentifierAndId(pUnderlyer.Identifier, pUnderlyer.IdAsset)),
                                new LogParam(pUnderlyer.IdI)));
                        }
                    }
                    else
                    {
                        codeReturn = Cst.ErrLevel.DATAIGNORE;
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 5157), 0,
                            new LogParam(pUnderlyer.Identifier),
                            new LogParam(pUnderlyer.AssetCategory)));
                    }
                }
            }
            catch (Exception)
            {
                codeReturn = Cst.ErrLevel.FAILURE;
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5155), 0, new LogParam(pUnderlyer.Identifier)));
            }
            return codeReturn;
        }
        #endregion SetTemplateDataDocument
        #region SetTradeMerge
        // EG 20150723 New
        protected override void SetTradeMerge(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocumentContainer, TradeMergeRule pTmr,
            List<TradeCandidate> pLstTradeMerge, TradeCandidate pSourceTradeBase)
        {
            IExchangeTradedDerivative _mergeTrade = (IExchangeTradedDerivative)pDataDocumentContainer.CurrentProduct.Product;
            IFixTrdCapRptSideGrp _exTradeRptSide = (IFixTrdCapRptSideGrp)_mergeTrade.TradeCaptureReport.TrdCapRptSideGrp[0];
            if (_mergeTrade.TradeCaptureReport.TrdTypeSpecified)
            {
                //Sauvegarde du TrdType du trade source de base dans SecondaryTrdType
                // EG 20130808 Secondary n'autorise pas de valeur > 1000 donc on ne copie pas le TrdType dans ce cas
                int _trdTypeValue = Convert.ToInt32(ReflectionTools.ConvertEnumToString<TrdTypeEnum>(_mergeTrade.TradeCaptureReport.TrdType));
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
                DecValue = pLstTradeMerge.Sum(trade => trade.Qty)
            };
            _mergeTrade.TradeCaptureReport.LastPxSpecified = true;
            if (pTmr.Context.PriceValue.HasValue)
                // Prix identique (contexte = IDENTICAL)
                _mergeTrade.TradeCaptureReport.LastPx.DecValue = pSourceTradeBase.Price;
            else
            {
                // Prix moyen pondéré
                decimal _lastPx = pLstTradeMerge.Sum(trade => trade.Price * trade.Qty) / _mergeTrade.TradeCaptureReport.LastQty.DecValue;

                Nullable<Cst.RoundingDirectionSQL> direction = null;
                Nullable<int> precision = null;

                SQL_DerivativeContract _sqlDC = new SQL_DerivativeContract(CS, pSourceTradeBase.IdDC.Value);
                if (_sqlDC.IsLoaded)
                {
                    direction = _sqlDC.RoundingDirectionSQLWeightAVGPrice;
                    precision = _sqlDC.RoundPrecWeightAveragePrice;
                }
                if (false == direction.HasValue)
                    direction = Cst.RoundingDirectionSQL.N;
                if (false == precision.HasValue)
                {
                    // On prend le plus grand nombre de décimales des prix des trades sources
                    precision = pLstTradeMerge.Max(trade => trade.RemainderPriceLength);
                }
                EFS_Round _round = new EFS_Round(direction.Value, precision.Value, _lastPx);
                _mergeTrade.TradeCaptureReport.LastPx.DecValue = _round.AmountRounded;
            }
        }
        #endregion SetTradeMerge

        #region TradeOptionGen
        /// <summary>
        /// DENOUEMENT D'OPTION (MANUEL / AUTOMATIQUE)
        /// Demande utilisateur (ITD/EOD) et/ou dénouement automatique (via EOD)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Message de type PosKeepingRequestMQueue</para>
        ///<para>  ● REQUESTTYPE = AAB, ABN,NEX,NAS, AAS, ASS, AEX ou EXE</para>
        ///<para>  ● REQUESTMODE = EOD/ITD</para>
        ///<para>  ● Pour un trade donné et une quantité donnée</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <param name="pIsResetData">Boolean</param>
        /// <returns>Cst.ErrLevel</returns>
        /// FI 20130917 [18953] Rename de la fonction en TradeOptionGen (cette méthode s'applique uniquement s'il existe un IdT)
        /// EG 20221212 [WI496] New
        // EG 20240115 [WI808] New : Harmonisation et réunification des méthodes
        private Cst.ErrLevel TradeOptionGen(IDbTransaction pDbTransaction, bool pIsResetData)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            //
            if (pIsResetData)
                codeReturn = InitPosKeepingData(m_PosRequest.IdT, Cst.PosRequestAssetQuoteEnum.UnderlyerAsset, true);

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5067), 0,
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(m_PosRequest.RequestType),
                new LogParam(m_PosRequest.RequestMode),
                new LogParam(LogTools.IdentifierAndId(PosKeepingData.Trade.Identifier, m_PosRequest.IdT)),
                new LogParam(m_PosRequest.Qty.ToString())));

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                DataSet ds = GetPositionTradeOption(CS, pDbTransaction, DtBusiness);
                if (null != ds)
                {
                    m_DtPosition = ds.Tables[0];
                    m_DtPosActionDet = ds.Tables[1];
                    m_DtPosition.PrimaryKey = new DataColumn[] { m_DtPosition.Columns["IDT"] };
                    TradeOptionData tradeData = new TradeOptionData(this, m_PosRequest, PosKeepingData, (m_TradeLibrary, m_DtPosition, m_DtPosActionDet))
                    {
                        ExchangeTradedDerivativeContainer = new ExchangeTradedDerivativeContainer(CS, pDbTransaction, (IExchangeTradedDerivative)m_TradeLibrary.Product)
                    };
                    tradeData.PosActionDetCalculation(pDbTransaction);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = tradeData.TradeOptionUpdating(pDbTransaction);
                }
            }
            return codeReturn;
        }
        #endregion TradeOptionGen
        #endregion Methods


        #region CascadingOrShiftingGen
        /// <summary>
        /// REALISE LE CASCADING AUTOMATIQUE DES POSITIONS ARRIVANT A ECHEANCE (POUR UNE CLE DE POSITION)
        /// <para>[Pour les contrats qui sont en cascading]</para>
        /// 
        /// REALISE LE SHIFTING AUTOMATIQUE DES POSITIONS LE LAST TRADING DAY (POUR UNE CLE DE POSITION)
        /// <para>[Pour les contrats pour lesquels est défini un shifting]</para>
        /// <para>[Attention: le Shifting à lieu le dernier jour de négociation et non à la date d'expiration]</para>
        /// </summary>
        /// <param name="pPosRequest">Requete de cascading|shifting</param>
        /// <param name="pNewTradeDtBusiness">Date du trade généré par le shifting</param>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20140225 [19575]
        // EG 20170221 [22879] Fusion between Cascading and Shifting in one method
        // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CascadingOrShiftingGen(IPosRequestCascadingShifting pPosRequest, DateTime pNewDtBusiness)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                bool isCascading = (pPosRequest.RequestType == Cst.PosRequestTypeEnum.Cascading);

                QueryParameters qryParameters = GetQueryTradeCandidatesToCascadingOrShifting(pPosRequest);
                // PL 20180312 WARNING: Use Read Commited !                
                //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
                DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);

                if (null != ds && ArrFunc.IsFilled(ds.Tables[0].Rows))
                {
                    List<EOD_Trade> lstTrades = new List<EOD_Trade>();

                    #region Alimentation de la liste des trades candidats
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        EOD_Trade trade = new EOD_Trade
                        {
                            IdT = Convert.ToInt32(dr["IDT"]),
                            Trade_Identifier = Convert.ToString(dr["IDENTIFIER"]),
                            Asset_Identifier = Convert.ToString(dr["ASSET_IDENTIFIER"]),
                            Qty = Convert.ToDecimal(dr["QTY"]),
                            IdE_Event = Convert.ToInt32(dr["IDE_EVENT"]),
                            IsDealerBuyer = IsTradeBuyer(Convert.ToString(dr["SIDE"])),
                            NbEvent = pPosRequest.NbTokenIdE
                        };
                        if (trade.IsDealerBuyer)
                            trade.IdT_Buy = trade.IdT;
                        else
                            trade.IdT_Sell = trade.IdT;

                        if (isCascading)
                        {

                            #region Calcul des frais liés au cascading
                            //string action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_DLV);
                            trade.Fees = CalFeesByQuantity(CS, IdMenu.Menu.InputTrade_CASC, trade.IdT, trade.Qty);

                            // Calcul du nombre d'événements pour les frais (avec taxes ou non)

                            if (ArrFunc.IsFilled(trade.Fees))
                            {
                                // Désérialisation du trade
                                DeserializeTrade(trade.IdT);

                                ExchangeTradedDerivativeContainer.Efs_ExchangeTradedDerivative =
                                    new EFS_ExchangeTradedDerivative(CS, ExchangeTradedDerivativeContainer.ExchangeTradedDerivative, m_TradeLibrary.DataDocument,
                                        Cst.StatusBusiness.ALLOC, trade.IdT);

                                EventQuery.InitPaymentForEvent(CS, trade.Fees, m_TradeLibrary.DataDocument, out int nbFeesEvent);
                                trade.NbEvent += nbFeesEvent;
                            }
                            #endregion Calcul des frais liés au cascading
                        }

                        lstTrades.Add(trade);

                    }
                    pPosRequest.IdTSpecified = false;
                    #endregion Alimentation de la liste des trades candidats

                    if ((Cst.ErrLevel.SUCCESS == codeReturn) && (0 < lstTrades.Count))
                    {
                        dbTransaction = DataHelper.BeginTran(CS);

                        #region GetId of POSACTION/POSACTIONDET

                        SQLUP.GetId(out int newIdPA, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);

                        int newIdPADET = newIdPA;
                        #endregion GetId of POSACTION/POSACTIONDET

                        #region Insertion dans POSACTION
                        InsertPosAction(dbTransaction, newIdPA, DtBusiness, pPosRequest.IdPR);
                        #endregion Insertion dans POSACTION


                        #region Insertion TRADE/POSACTIONDET/EVENTPOSACTIONDET/EVENTS
                        // 1. dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS pour les trades cascadés|shiftés
                        // 2. dans TRADE et EVENT issus du cascading|shifting

                        List<Pair<int, string>> lstTradesResult = new List<Pair<int, string>>();
                        lstTrades.ForEach(trade =>
                        {
                            if (false == isException)
                            {
                                pPosRequest.SetIdentifiers(trade.Trade_Identifier);
                                pPosRequest.Identifiers.Asset = trade.Asset_Identifier;
                                pPosRequest.IdT = trade.IdT;
                                pPosRequest.IdTSpecified = false;
                                pPosRequest.Qty = trade.Qty;
                                pPosRequest.QtySpecified = true;

                                #region EVENT et POSACTIONDET
                                SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, trade.NbEvent);

                                #region POSACTIONDET
                                // EG 20141205 null for PositionEffect
                                InsertPosActionDet(dbTransaction, newIdPA, newIdPADET, trade.IdT_Buy, trade.IdT_Sell, trade.IdT, trade.Qty, null);
                                #endregion POSACTIONDET

                                #region EVENT CAS|SHI
                                //Event
                                m_EventQuery.InsertEvent(dbTransaction, trade.IdT, newIdE, trade.IdE_Event, null, 1, 1, null, null, null, null,
                                    isCascading ? EventCodeFunc.Cascading:EventCodeFunc.Shifting, EventTypeFunc.Total, 
                                    DtBusiness, DtBusiness, DtBusiness, DtBusiness, trade.Qty, null, UnitTypeEnum.Qty.ToString(), null, null);

                                //EventClass
                                m_EventQuery.InsertEventClass(dbTransaction, newIdE, EventClassFunc.GroupLevel, DtBusiness, false);

                                //PosActionDet
                                EventQuery.InsertEventPosActionDet(dbTransaction, newIdPADET, newIdE);

                                #endregion EVENT CAS|SHI

                                //idE_Event Contient L'evènement CAS|SHI
                                trade.IdE_Event = newIdE;

                                #region EVENT NOM/QTY (Nominal/Quantité)
                                newIdE++;
                                codeReturn = InsertNominalQuantityEvent(dbTransaction, trade.IdT, ref newIdE, trade.IdE_Event, DtBusiness, trade.IsDealerBuyer, trade.Qty, 0, newIdPADET);
                                #endregion EVENT NOM/QTY (Nominal/Quantité)

                                #region EVENT FEES (CUM TRADE - Cascading)
                                if (ArrFunc.IsFilled(trade.Fees))
                                {
                                    foreach (IPayment payment in trade.Fees)
                                    {
                                        newIdE++;
                                        int savIdE = newIdE;
                                        codeReturn = m_EventQuery.InsertPaymentEvents(dbTransaction, m_TradeLibrary.DataDocument, trade.IdT, payment, DtBusiness, 1, 1, ref newIdE, trade.IdE_Event);
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
                                #endregion EVENT FEES (CUM TRADE - Cascading)

                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    #region Création des trades cascading|shifting
                                    pPosRequest.IdTSpecified = true;
                                    if (isCascading)
                                        codeReturn = CreateCascadingTrades(dbTransaction, pPosRequest, ref lstTradesResult);
                                    else
                                        codeReturn = CreateShiftingTrade(dbTransaction, pPosRequest, pNewDtBusiness, ref lstTradesResult);
                                    #endregion Création des trades cascading|shifting
                                }

                                if (Cst.ErrLevel.SUCCESS != codeReturn)
                                    isException = true;

                                newIdE++;
                                //newIdPADET++;
                                SQLUP.GetId(out newIdPADET, dbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);

                                #endregion EVENT et POSACTIONDET

                            }
                        });
                        #endregion Insertion TRADE/POSACTIONDET/EVENTPOSACTIONDET/EVENTS

                        if (false == isException)
                        {
                            if (null != dbTransaction)
                                DataHelper.CommitTran(dbTransaction);

                            lstTradesResult.ForEach(tradeResult =>
                            {
                                if (Cst.ErrLevel.SUCCESS != RecordTradeEvent(tradeResult.First, tradeResult.Second))
                                {

                                    m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                    
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5179), 0,
                                        new LogParam(GetPosRequestLogValue(isCascading ? Cst.PosRequestTypeEnum.Cascading : Cst.PosRequestTypeEnum.Shifting)),
                                        new LogParam(LogTools.IdentifierAndId(tradeResult.Second, tradeResult.First))));
                                }
                            });
                        }
                    }
                }
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
            return codeReturn;
        }
        #endregion CascadingOrShiftingGen


    }
}

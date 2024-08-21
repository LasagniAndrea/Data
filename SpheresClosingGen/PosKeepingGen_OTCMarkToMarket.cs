#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.MarkToMarket;
//
using EfsML;
using EfsML.Business;
using EfsML.Interface;
using EfsML.v30.PosRequest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
//
//
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    public class CrossMarginTradeInfo
    {
        public string Identifier {get;set;}
        public Cst.MarginingMode MarginingMode {get;set;}
        public string GProduct  { get; set; }
    }

    // EG 20150317 [POC] New 
    public partial class PosKeepingGen_MTM : PosKeepingGenProcessBase
    {
        #region Accessors

        #region StoreProcedure_RemoveEOD
        protected override string StoreProcedure_RemoveEOD
        {
            get { return "UP_REMOVE_POSACTION_MTM"; }
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
        #region GPRODUCT_VALUE
        // EG 20160106 (See PosKeepingGen_XXX - Override)
        public override string GPRODUCT_VALUE
        {
            get { return ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.MarkToMarket);}
        }
        #endregion GPRODUCT_VALUE


        #endregion Accessors
        #region Constructors
        public PosKeepingGen_MTM(PosKeepingGenProcess pPosKeepingGenProcess)
            : base(pPosKeepingGenProcess)
        {
        }
		#endregion Constructors

        #region Methods

        //────────────────────────────────────────────────────────────────────────────────────────────────
        // EOD/CLOSINGDAY OVERRIDE METHODS (VIRTUAL SUR PosKeepingGenProcessBase)
        //────────────────────────────────────────────────────────────────────────────────────────────────

        #region EOD_CashFlowsGen
        /// <summary>
        /// CALCUL DES MARGES des NEOGICATIONS EN POSITIONS (EOD)
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Calcul/Recalcul des événements de marges des trades jour ou en position veille, à savoir:</para>
        ///<para>  ● VMG : Variation margin</para>
        ///<para>  ● LOV : Liquidative option value</para>
        ///<para>  ● UMG : Unrealized margin</para>
        ///<para>────────────────────────────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140204 [19586] IsolationLevel
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel EOD_CashFlowsGen()
        {
            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5008), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel,
            //    m_MasterPosRequest.dtBusiness, m_PosRequest.idEM, 480, null, null);
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    // INSERTION LOG
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5011), 1,
                        new LogParam(nbRow),
                        new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                        new LogParam(m_PosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));

                    MarkToMarketGenProcess mtmGenProcess = null;
                    ProcessState _processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);


                    Dictionary<Pair<int, CrossMarginTradeInfo>, List<Pair<int, decimal>>> crossMarginMatrix = CrossMarginMatrixGen(ds.Tables[0]);

                    foreach (Pair<int, CrossMarginTradeInfo> key in crossMarginMatrix.Keys)
                    {
                        MtMValuation(key, crossMarginMatrix[key], mtmGenProcess, _processState);
                    }

                    if (null != mtmGenProcess)
                    {
                        mtmGenProcess.ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.End;
                        mtmGenProcess.ProcessSlave();
                        mtmGenProcess.MtmGenProcessBase.SlaveCallDispose();
                    }
                    UpdatePosRequestGroupLevel(posRequestGroupLevel, _processState.Status);
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_CashFlowsGen


        #region CrossMarginMatrixGen
        /// <summary>
        /// Construction de la matrice de CrossMargin pour le calcul du MGR à savoir :
        /// pour un IDT donné ( à concurrence de son ReferenceAmount)
        /// - Recherche des trades de sens opposé et de même caractéristiques avec une date de VALEUR supérieure ou égale
        /// </summary>
        /// <returns></returns>
        /// EG 20171016 [23509] Upd DTEXECUTION replace TIMESTAMP to Sort and Filter 
        protected Dictionary<Pair<int, CrossMarginTradeInfo>, List<Pair<int, decimal>>> CrossMarginMatrixGen(DataTable pDtPosition)
        {
            Dictionary<Pair<int, CrossMarginTradeInfo>, List<Pair<int, decimal>>> crossMarginMatrix = null;
            m_DtPosActionDet_Working = pDtPosition.Clone();
            // EG 20171016 [23509]
            string sort = "IDI, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDC, IDC2, DTOUTUNADJ, DTEXECUTION";
            // EG 20171016 [23509]
            string filter = @"SIDE <> '{0}' and DTEXECUTION >= '{1}' 
            and IDI = {2} and IDA_DEALER = {3} and IDB_DEALER = {4} and IDA_CLEARER = {5} and IDB_CLEARER = {6} and IDC = '{7}' and IDC2 = '{8}'";
            DataRow[] rowTrades = pDtPosition.Select(null, sort, DataViewRowState.CurrentRows);
            if (ArrFunc.IsFilled(rowTrades))
            {

                foreach (DataRow rowTrade in rowTrades)
                {
                    // IdT et AmountReference du Trade qui subira le CrossMargin
                    int idT = Convert.ToInt32(rowTrade["IDT"]);
                    decimal amountTrade = Convert.ToDecimal(rowTrade["NOTIONALREFERENCE"]);
                    Cst.MarginingMode marginingMode = (Cst.MarginingMode)ReflectionTools.EnumParse(new Cst.MarginingMode(), rowTrade["MARGININGMODE"].ToString());

                    CrossMarginTradeInfo crossMarginTradeInfo = new CrossMarginTradeInfo
                    {
                        Identifier = rowTrade["IDENTIFIER"].ToString(),
                        GProduct = rowTrade["GPRODUCT"].ToString(),
                        MarginingMode = (Cst.MarginingMode)ReflectionTools.EnumParse(new Cst.MarginingMode(), rowTrade["MARGININGMODE"].ToString())
                    };

                    Pair<int, CrossMarginTradeInfo> key = new Pair<int, CrossMarginTradeInfo>(idT, crossMarginTradeInfo);
                    Pair<int, decimal> crossMarginItem = null;

                    if (0 < amountTrade)
                    {
                        // filtre sur les trades de sens inverse de même caractèristiques avec une date de VALEUR supérieure ou égale à celle du trade source
                        DataRow[] rowCrossTrades = pDtPosition.Select(String.Format(filter,
                            rowTrade["SIDE"].ToString(),
                            // EG 20171016 [23509]
                            DtFunc.DateTimeOffsetUTCToStringISO(Convert.ToDateTime(rowTrade["DTEXECUTION"])),
                            Convert.ToInt32(rowTrade["IDI"]),
                            Convert.ToInt32(rowTrade["IDA_DEALER"]),
                            Convert.ToInt32(rowTrade["IDB_DEALER"]),
                            Convert.ToInt32(rowTrade["IDA_CLEARER"]),
                            Convert.ToInt32(rowTrade["IDB_CLEARER"]),
                            rowTrade["IDC"].ToString(),
                            rowTrade["IDC2"].ToString()), sort, DataViewRowState.CurrentRows);


                        if (null == crossMarginMatrix)
                        {
                            // Initialisation du dictionnaire
                            crossMarginMatrix = new Dictionary<Pair<int, CrossMarginTradeInfo>, List<Pair<int, decimal>>>();
                        }

                        foreach (DataRow rowCrossTrade in rowCrossTrades)
                        {
                            // Sortie de boucle lorsque le montant disponible = 0
                            if (0 == amountTrade)
                            {
                                rowTrade["NOTIONALREFERENCE"] = Math.Max(0, amountTrade);
                                break;
                            }

                            // IdT et AmountReference du Trade qui participera au CrossMargin
                            int idTCrossMargin = Convert.ToInt32(rowCrossTrade["IDT"]);
                            decimal amountCrossTrade = Convert.ToDecimal(rowCrossTrade["NOTIONALREFERENCE"]);
                            decimal amount = Math.Min(amountTrade, amountCrossTrade);

                            crossMarginItem = new Pair<int, decimal>(idTCrossMargin, amount);

                            // Insertion du trade qui subira le CrossMargin dans le dictionnaire

                            if (false == crossMarginMatrix.ContainsKey(key))
                            {
                                // Insertion du trade qui SUBIRA le CrossMargin dans le dictionnaire
                                List<Pair<int, decimal>> lstCrossMarginItem = new List<Pair<int, decimal>>
                                {
                                    crossMarginItem
                                };
                                crossMarginMatrix.Add(key, lstCrossMarginItem);
                            }
                            else if (false == crossMarginMatrix[key].Exists(item => item.First == idTCrossMargin))
                            {
                                // Insertion du trade qui PARTICIPERA au CrossMargin dans le dictionnaire
                                crossMarginMatrix[key].Add(crossMarginItem);
                            }

                            rowCrossTrade["NOTIONALREFERENCE"] = Math.Max(0, amountCrossTrade - amount);
                            amountTrade -= amount;
                        }

                        // Si 0 < ReferenceAmount alors on insère une ligne pour gérer le montant résiduel
                        if (0 < amountTrade)
                        {
                            crossMarginItem = new Pair<int, decimal>(idT, amountTrade);
                            if (false == crossMarginMatrix.ContainsKey(key))
                            {
                                List<Pair<int, decimal>> lstCrossMarginItem = new List<Pair<int, decimal>>
                                {
                                    crossMarginItem
                                };
                                crossMarginMatrix.Add(key, lstCrossMarginItem);
                            }
                            else
                            {
                                crossMarginMatrix[key].Add(crossMarginItem);
                            }
                        }
                    }
                    else
                    {
                        // EG 20150408 A REVOIR !!!!!!!
                        // Pas de MGR sur ce trade car totalement CrossMarginé
                        // Par contre génération UMG donc on l'insére dans la liste (ReferenceAmount = 0 sera utilisé dans le test pour le traitement)
                        crossMarginItem = new Pair<int, decimal>(idT, amountTrade);
                        if (false == crossMarginMatrix.ContainsKey(key))
                        {
                            List<Pair<int, decimal>> lstCrossMarginItem = new List<Pair<int, decimal>>
                            {
                                crossMarginItem
                            };
                            crossMarginMatrix.Add(key, lstCrossMarginItem);
                        }
                        else
                        {
                            crossMarginMatrix[key].Add(crossMarginItem);
                        }

                    }
                }
            }
            return crossMarginMatrix;
        }
        #endregion CrossMarginMatrixGen
        #region MtMValuation
        // EG 20190613 [24683] Set Tracker to Slave
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void MtMValuation(Pair<int, CrossMarginTradeInfo> pKey, List<Pair<int, decimal>> pCrossMarginTrades, MarkToMarketGenProcess pMtmGenProcess, ProcessState pProcessState)
        {
            try
            {
                MarkToMarketGenMQueue mtmMQueue = GetQueueForEODCashFlows(pKey, pCrossMarginTrades);
                if (null != mtmMQueue)
                {
                    if (null == pMtmGenProcess)
                    {
                        pMtmGenProcess = new MarkToMarketGenProcess(mtmMQueue, m_PKGenProcess.AppInstance)
                        {
                            ProcessCacheContainer = m_PKGenProcess.ProcessCacheContainer,
                            
                            IsDataSetTrade_AllTable = false,
                            TradeTable = TradeTableEnum.NONE,
                            ProcessCall = ProcessBase.ProcessCallEnum.Slave,
                            ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.Start,
                            IsSendMessage_PostProcess = false,
                            
                            IdProcess = ProcessBase.IdProcess,
                            LogDetailEnum = m_PKGenProcess.LogDetailEnum,
                            NoLockCurrentId = true,
                            Tracker = m_PKGenProcess.Tracker
                        };
                        pMtmGenProcess.ProcessSlave();
                    }
                    pMtmGenProcess.MQueue = mtmMQueue;
                    pMtmGenProcess.ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.Execute;
                    pMtmGenProcess.ProcessState = new ProcessState(ProcessStateTools.StatusEnum.NONE, Cst.ErrLevel.SUCCESS);
                    pMtmGenProcess.ProcessSlave();

                    // EG 20130620 Valorisation CodeReturn global
                    if (pMtmGenProcess.ProcessState.CodeReturn != Cst.ErrLevel.SUCCESS)
                        pProcessState.SetErrorWarning(pMtmGenProcess.ProcessState.Status, pMtmGenProcess.ProcessState.CodeReturn);
                    else
                        pProcessState.SetErrorWarning(pMtmGenProcess.ProcessState.Status);

                    pMtmGenProcess.MtmGenProcessBase.SlaveCallDispose();
                }
                else
                    pProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.QUOTENOTFOUND);
            }
            catch (Exception)
            {
                if (null != pMtmGenProcess)
                {
                    pMtmGenProcess.ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.End;
                    pMtmGenProcess.ProcessSlave();
                    pMtmGenProcess.MtmGenProcessBase.SlaveCallDispose();
                }
                throw;
            }
        }
        #endregion MtmValuation
        #region EOD_MarketGen
        // EG 20130313 [POC]
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

                // STEP : CALCUL DES CASH-FLOWS
                if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = EOD_CashFlowsGen();

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
                if ((Cst.ErrLevel.FAILURE == codeReturn) || (Cst.ErrLevel.IRQ_EXECUTED == codeReturn))
                    UpdatePosRequest(codeReturn, m_MarketPosRequest, m_MarketPosRequest.IdPR_PosRequest);
                else
                    UpdatePosRequestGroupLevel(m_MarketPosRequest, ProcessStateTools.StatusSuccessEnum);
            }
        }

        #endregion EOD_MarketGen

        #region CLOSINGDAY_EODControl
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
                #region NEW ALLOC AFTER EOD PROCESS
                // EG 20151202 GetDataRequestWithIsolationLevel
                ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, Cst.PosRequestTypeEnum.Entry, pDtBusiness, pIdEM, null, null, null);
                if (null != ds)
                {
                    newIDPR++;
                    codeReturn = ClosingDayControlDetail(newIDPR, pIdA_Entity, pIdA_Css, pIdA_Custodian, pIdEM, pDtBusiness, ds.Tables[0].Rows,
                        Cst.PosRequestTypeEnum.ClosingDay_EntryGroupLevel, pIdPR_Parent, pGroupProduct);
                }
                #endregion NEW ALLOC AFTER EOD PROCESS
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
                {
                    AddSubPosRequest(m_LstMarketSubPosRequest, m_LstSubPosRequest[0]);
                }

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
        /// Alimente m_TradeLibrary 
        /// </summary>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// FI 20141218 [XXXXX] Modify
        protected override void DeserializeTrade(IDbTransaction pDbTransaction, int pIdT)
        {
            base.DeserializeTrade(pDbTransaction, pIdT);
        }
        #endregion DeserializeTrade

        #region GetQueueForEODCashFlows
        protected MarkToMarketGenMQueue GetQueueForEODCashFlows(Pair<int, CrossMarginTradeInfo> pKey, List<Pair<int, decimal>> pCrossMarginTrades)
        {

            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = CS,
                id = pKey.First,
                requester = Queue.header.requester
            };

            MarkToMarketGenMQueue mQueue = new MarkToMarketGenMQueue(mQueueAttributes, pKey.Second.MarginingMode, pCrossMarginTrades, (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay))
            {
                dateSpecified = true,
                date = DtBusiness,
                idInfo = new IdInfo()
                {
                    id = pKey.First,
                    idInfos = new DictionaryEntry[]{
                            new DictionaryEntry("ident", "TRADE"),
                            new DictionaryEntry("identifier", pKey.Second.Identifier),
                            new DictionaryEntry("GPRODUCT", pKey.Second.GProduct)}
                },
                idInfoSpecified = true
            };
            return mQueue;
        }
        #endregion GetQueueForEODCashFlows

        #region LoadTradeMergeRules
        protected override Cst.ErrLevel LoadTradeMergeRules()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion LoadTradeMergeRules

        #region SetPosKeepingAsset
        /// <summary>
        ///  Retourne l'asset (IPosKeepingAsset)
        /// </summary>
        /// <param name="pDr"></param>
        /// <returns></returns>
        // EG 20180221 [23769] Public pour gestion asynchrone
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20181205 [24360] Refactoring
        public override PosKeepingAsset SetPosKeepingAsset(IDbTransaction pDbTransaction, Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, MapDataReaderRow pMapDr)
        {
            PosKeepingAsset asset = m_Product.CreatePosKeepingAsset(pUnderlyingAsset);
            if (null != asset)
            {
                bool isExistPriceCurrency = false;

                int idAsset = Convert.ToInt32(pMapDr["IDASSET"].Value);
                string identifier = pMapDr["ASSET_IDENTIFIER"].Value.ToString();

                asset.idAsset = idAsset;
                asset.identifier = identifier;
                asset.contractMultiplier = 1;
                if (null != pMapDr["NOMINALVALUE"])
                    asset.nominal = Convert.IsDBNull(pMapDr["NOMINALVALUE"].Value) ? 0 : Convert.ToDecimal(pMapDr["NOMINALVALUE"].Value);
                if (null != pMapDr["NOMINALCURRENCY"])
                    asset.nominalCurrency = pMapDr["NOMINALCURRENCY"].Value.ToString();
                if (null != pMapDr["IDC"])
                    asset.currency = pMapDr["IDC"].Value.ToString();
                if (null != pMapDr["PRICECURRENCY"])
                {
                    isExistPriceCurrency = true;
                    asset.priceCurrency = pMapDr["PRICECURRENCY"].Value.ToString();
                }
                if (null != pMapDr["CONTRACTMULTIPLIER"])
                    asset.contractMultiplier = Convert.IsDBNull(pMapDr["CONTRACTMULTIPLIER"].Value) ? 1 : Convert.ToDecimal(pMapDr["CONTRACTMULTIPLIER"].Value);
                else
                    asset.contractMultiplier = 1;

                if (null != pMapDr["IDBC"])
                    asset.idBC = pMapDr["IDBC"].Value.ToString();

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

        #endregion Methods

    }
}

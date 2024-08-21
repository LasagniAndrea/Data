#region Using Directives
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
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
using EfsML.Interface;
//
//
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // B A S E   
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // RD 20171012 [23502] new
    public partial class PosKeepingGenProcessBase
    {
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // SAFE KEEPING  (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_Safekeeping
        /// <summary>
        /// Safekeeping (Frais de garde) calculation
        /// </summary>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Calcul des frais de garde du jour</para>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20150708 [21103] New
        // EG 20160208 [POC-MUREX] Refactoring (Alimentation List<SKP_Trade> lstTrades)
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20180307 [23769] Gestion List<IPosRequest>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        protected virtual Cst.ErrLevel EOD_Safekeeping()
        {
            bool isParallelCalculation = ProcessBase.IsParallelProcess(ParallelProcess.SafeKeepingCalculation);
            bool isParallelWriting = ProcessBase.IsParallelProcess(ParallelProcess.SafeKeepingWriting);

            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);

            m_PosRequest = RestoreMarketRequest;

            m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarket(m_PosRequest.IdA_Entity, m_PosRequest.IdM, m_PosRequest.IdA_CssCustodian);

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5014), 0,
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                new LogParam(isParallelCalculation ? "YES" : "NO"),
                new LogParam(isParallelCalculation ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.SafeKeepingCalculation)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.SafeKeepingCalculation)) : "-"),
                new LogParam(isParallelWriting ? "YES" : "NO"),
                new LogParam(isParallelWriting ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.SafeKeepingWriting)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.SafeKeepingWriting)) : "-")));
            Logger.Write();

            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel,
                m_MasterPosRequest.DtBusiness, m_PosRequest.IdEM, 480, null, null);

            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                // Insert POSREQUEST REGROUPEMENT
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

                if (0 < nbRow)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, "Preparation...", 1));
                    Logger.Write();

                    m_TradeCandidatesForSafeKeeping = new Dictionary<int, TradeSafeKeepingCalculation>();

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        if (IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref processState))
                            break;
                        int idT = Convert.ToInt32(dr["IDT"]);
                        if (false == m_TradeCandidatesForSafeKeeping.ContainsKey(idT))
                            m_TradeCandidatesForSafeKeeping.Add(idT, SetTradeCandidateForSafekeeping(dr));
                    }

                    bool isContinue = (0 < m_TradeCandidatesForSafeKeeping.Count) && (false == ProcessStateTools.IsStatusInterrupt(processState.Status));

                    if (isContinue)
                    {
                        // INSERTION LOG
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5011), 1,
                            new LogParam(m_TradeCandidatesForSafeKeeping.Count),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                            new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                            new LogParam(m_MarketPosRequest.GroupProductValue),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                        Logger.Write();

                        User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);

                        //---------------------------------------------------------
                        // CALCUL DES FRAIS DE GARDE
                        //---------------------------------------------------------
                        if (isParallelCalculation)
                    {
                            // Calcul des frais de garde (ASYNCHRONE)
                            processState = CommonGenThreading(ParallelProcess.SafeKeepingCalculation, m_TradeCandidatesForSafeKeeping.Values.ToList(), user);

                            }
                        else
                        {
                            // Calcul des frais de garde (SYNCHRONE)
                            m_TradeCandidatesForSafeKeeping.ToList().ForEach(trade =>
                            {
                                ProcessState processStateItem = SafeKeepingCalculationGenByTrade(trade.Value, user);
                                processState.SetErrorWarning(processStateItem.Status, processStateItem.CodeReturn);
                                        });
                                    }

                        //---------------------------------------------------------
                        // ECRITURE (INSERTION/SUPPRESSION DES FRAIS DE GARDE
                        //---------------------------------------------------------
                        isContinue = (0 < m_TradeCandidatesForSafeKeeping.Count) &&
                            m_TradeCandidatesForSafeKeeping.ToList().Exists(trade => trade.Value.IsDeleted || trade.Value.IsInserted) && 
                            (false == ProcessStateTools.IsStatusInterrupt(processState.Status));

                        if (isContinue)
        {
                            if (isParallelWriting)
            {
                                // Ecriture des frais de garde (ASYNCHRONE)
                                processState = CommonGenThreading(ParallelProcess.SafeKeepingWriting, m_TradeCandidatesForSafeKeeping.Values.ToList(), user);
                            }
                            else
                {
                                // Ecriture des frais de garde (SYNCHRONE)
                                m_TradeCandidatesForSafeKeeping.ToList().ForEach(trade =>
                    {
                                    ProcessState processStateItem = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
                                    processStateItem = SafeKeepingWritingGenByTrade(trade.Value);
                                });
                            }
                    }

                    }
                    UpdatePosRequestGroupLevel(posRequestGroupLevel, processState.Status);
                }
            }
            return processState.CodeReturn;
        }
        #endregion EOD_Safekeeping
        
        #region SetTradeCandidateForSafekeeping
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        // EG 20190716 [VCL : New FixedIncome] Upd Add SetDataToMarketValueCalculation
        private TradeSafeKeepingCalculation SetTradeCandidateForSafekeeping(DataRow pDr)
        {
            TradeSafeKeepingCalculation trade = new TradeSafeKeepingCalculation
            {
                idT = Convert.ToInt32(pDr["IDT"]),
                qty = Convert.ToDecimal(pDr["QTY"]),
                // Il existe une ligne de frais de garde (SKP/ttt)
                IsDeleted = (false == Convert.IsDBNull(pDr["IDE_SKP"])),
                IdAsset = Convert.ToInt32(pDr["IDASSET"]),
                AssetIdentifier = pDr["ASSET_IDENTIFIER"].ToString(),
                identifier = pDr["TRADE_IDENTIFIER"].ToString(),
                DtQuote = DtBusiness,
                UnderlyingAsset = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), pDr["ASSETCATEGORY"].ToString())
            };
            // Il existe une ligne de regroupement (LPC/AMT)
            if (false == Convert.IsDBNull(pDr["IDE_EVENT"]))
                trade.idE_Event = Convert.ToInt32(pDr["IDE_EVENT"]);
            if (trade.UnderlyingAsset == Cst.UnderlyingAsset.FxRateAsset)
                trade.DtQuote = AssetTools.AddTimeRateSourceToDate(CSTools.SetCacheOn(CS, 1, null), trade.UnderlyingAsset, trade.IdAsset, trade.DtQuote);

            // ------------------------------------------------------------------------------------------------------
            // EG 20190403 [MIGRATION VCL] 
            // Le MKV est toujours calculé par le traitement SKP (car le montant MKV dans EVENT est en DTBUSINESS)
            // Pour les SKP il faut calculer un MKV en DTSETTLEMENT
            // Dans le cas des DST les éléments de calcul du MKV (CleanPrice, DirtyPrice et Taux du CC) sont récupérés pour évaluer le MKV
            // ------------------------------------------------------------------------------------------------------
            SetDataToMarketValueCalculation(trade, pDr);

            return trade;
        }
        #endregion SetSafekeepingTradeCandidate
        #region SetDataToMarketValueCalculation
        /// <summary>
        /// On aliment les données servant au calcul du MKV pour le SKP
        /// Lecture EVENTDET sur EVT MKV de la journée en cours
        /// </summary>
        // EG 20190716 [VCL : New FixedIncome] New
        private void SetDataToMarketValueCalculation(TradeSafeKeepingCalculation pTrade, DataRow pDr)
        {
            pTrade.MarketValue = null;
            pTrade.MarketValueData = null;
            if (pDr.Table.Columns.Contains("ASSETMEASURE_MKV") && (false == Convert.IsDBNull(pDr["ASSETMEASURE_MKV"])))
            {
                Nullable<AssetMeasureEnum> assetMeasure = ReflectionTools.ConvertStringToEnumOrNullable<AssetMeasureEnum>(pDr["ASSETMEASURE_MKV"].ToString());
                if (null != assetMeasure)
                {
                    pTrade.MarketValueData = new MarketValueData();
                    switch (assetMeasure.Value)
                    {
                        case AssetMeasureEnum.CleanPrice:
                            pTrade.MarketValueData.cleanPrice = Convert.ToDecimal(pDr["QUOTEPRICE_MKV"]);
                            pTrade.MarketValueData.dirtyPrice = Convert.ToDecimal(pDr["QUOTEPRICE100_MKV"]);
                            break;
                        case AssetMeasureEnum.DirtyPrice:
                            pTrade.MarketValueData.dirtyPrice = Convert.ToDecimal(pDr["QUOTEPRICE_MKV"]);
                            pTrade.MarketValueData.cleanPrice = Convert.ToDecimal(pDr["QUOTEPRICE100_MKV"]);
                            break;
                    }
                    pTrade.MarketValueData.accruedRate = Convert.ToDecimal(pDr["ACCRUEDRATE_MKV"]);
                    pTrade.MarketValueData.unitNotional = Convert.ToDecimal(pDr["BIZNOTIONALREF_MKV"]);
                    pTrade.MarketValueData.currency = pDr["BIZCUR_MKV"].ToString();
                }
            }
            else if (pDr.Table.Columns.Contains("BIZAMOUNT_MKV") && (false == Convert.IsDBNull(pDr["BIZAMOUNT_MKV"])) && (false == Convert.IsDBNull(pDr["BIZCUR_MKV"])))
            {
                pTrade.MarketValue = m_Product.CreateMoney(Convert.ToDecimal(pDr["BIZAMOUNT_MKV"]), pDr["BIZCUR_MKV"].ToString());
                pTrade.MarketValueQty = Convert.ToDecimal(pDr["BIZQTY_MKV"]);
            }

        }
        #endregion SetDataToMarketValueCalculation
    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // O T C : ReturnSwap (Cfd)
    //────────────────────────────────────────────────────────────────────────────────────────────────
    public partial class PosKeepingGen_OTC : PosKeepingGenProcessBase
    {
        // RD 20171012 [23502] Move EOD_Safekeeping methods to partial class "PosKeepingGenProcessBase"    
        #region Override Methods
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // SAFE KEEPING  (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_Safekeeping
        // See PosKeepingGenProcessBase.EOD_Safekeeping
        #endregion EOD_Safekeeping
        #endregion Override Methods
    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // S E C : EquitySecurityTransaction | DebtSecurityTransaction
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // RD 20171012 [23502] new
    public partial class PosKeepingGen_SEC : PosKeepingGenProcessBase
    {
        #region Override Methods
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // SAFE KEEPING  (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_Safekeeping
        // See PosKeepingGenProcessBase.EOD_Safekeeping
        #endregion EOD_Safekeeping
        #endregion Override Methods
    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // M T M : MarkToMarket
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // RD 20171012 [23502] new
    public partial class PosKeepingGen_MTM : PosKeepingGenProcessBase
    {
        #region Override Methods        
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // SAFE KEEPING  (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_Safekeeping
        // No calculation of SAFE KEEPING for M T M : MarkToMarket
        protected override Cst.ErrLevel EOD_Safekeeping()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_Safekeeping
        #endregion Override Methods
    }
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // C O M
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // RD 20171012 [23502] new
    public partial class PosKeepingGen_COM : PosKeepingGenProcessBase
    {
        #region Override Methods
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // SAFE KEEPING  (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_Safekeeping
        // No calculation of SAFE KEEPING for C O M : Commodity
        protected override Cst.ErrLevel EOD_Safekeeping()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_Safekeeping
        #endregion Override Methods
    }
    
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // E T D
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // RD 20171012 [23502] new
    public partial class PosKeepingGen_ETD : PosKeepingGenProcessBase
    {
        #region Override Methods
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // SAFE KEEPING  (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_Safekeeping
        // No calculation of SAFE KEEPING for E T D : ExchangeTradedDerivative
        protected override Cst.ErrLevel EOD_Safekeeping()
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_Safekeeping
        #endregion Override Methods
    }
}

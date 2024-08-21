using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
//
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace EFS.Process
{
    /// <summary>
    /// Génération des évènements de frais
    /// </summary>
    /// FI 20160907 [21831] Add
    public class TradeActionGenFeesEventGen : TradeActionGenProcessBase
    {
        #region Constructors
        /// <summary>
        /// Génération des évènements de frais sur un trade
        /// </summary>
        /// <param name="pTradeActionGenProcess">process courant</param>
        /// <param name="pDsTrade">Dataset Trade</param>
        /// <param name="pTradeLibrary">Represente le trade (contient les frais pour lesquels les événements seront générés)</param>
        /// <param name="pTradeAction">Message Queue</param>
        /// <exception cref="SpheresException2 si des frais existent déjà sur le trade"></exception>
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public TradeActionGenFeesEventGen(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
        {
            // PM 20210121 [XXXXX] Passage du message au niveau de log None
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7020), 0,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenMQueue.identifier, m_TradeActionGenMQueue.id))));
        }
        #endregion Constructors

        /// <summary>
        /// 
        /// </summary>
        public override void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction, m_DsTrade.IdT);
            m_DsEvents.Load(EventTableEnum.None, null, null);
        }

   

        /// <summary>
        /// Retourne les évènements tels que EVENTCODE = 'OPP' enfant de TRD/DAT
        /// </summary>
        /// <returns></returns>
        private DataRow[] GetRowOPP()
        {
            int idE_Event = GetIdE_TRDDAT();
            DataRow[] rowOpp = DtEvent.Select(StrFunc.AppendFormat("EVENTCODE = 'OPP' and IDE_EVENT = {0}", idE_Event.ToString()));

            DataRow[] ret = null;
            if (ArrFunc.IsFilled(rowOpp))
                ret = rowOpp;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180502 Analyse du code Correction [CA2214]
        // PL 20200115 [25099] Refactoring and Enrichment
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (null == GetRowOPP())
            {
                if (m_tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    bool isTodoUpdateTradeXML = false;

                    //GLOPXXX See EG
                    if (m_tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified
                        && (m_tradeLibrary.CurrentTrade.OtherPartyPayment[0].Efs_Payment == null))
                    {
                        ret = PaymentTools.CalcPayments(m_CS, m_TradeActionGenProcess.SlaveDbTransaction, m_tradeLibrary.Product,
                            m_tradeLibrary.CurrentTrade.OtherPartyPayment, m_tradeLibrary.DataDocument);
                    }

                    bool existsPayment_OnFeeScopeOrderId = false;
                    bool existsPayment_OnFeeScopeFolderId = false;
                    // PL 20200115 [25099] New
                    if (PaymentTools.IsExistsPayment_OnFeeScopeOrderIdOrFolderId_WithMinMax(m_tradeLibrary.CurrentTrade.OtherPartyPayment,
                                                                                            ref existsPayment_OnFeeScopeOrderId, ref existsPayment_OnFeeScopeFolderId))
                    {

                        #region Lock de l'ordre et/ou du dossier
                        if (existsPayment_OnFeeScopeOrderId)
                        {
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            string orderId = m_tradeLibrary.DataDocument.GetOrderId();
                            ret = m_TradeActionGenProcess.LockObjectId(TypeLockEnum.TRADE, Cst.FeeScopeEnum.OrderId.ToString() + ":" + orderId, orderId, m_TradeActionGenMQueue.ProcessType.ToString(), LockTools.Exclusive.ToString());
                        }
                        if (existsPayment_OnFeeScopeFolderId && ProcessStateTools.IsCodeReturnSuccess(ret))
                        {
                            string folderId = "";
                            _ = m_TradeActionGenProcess.LockObjectId(TypeLockEnum.TRADE, Cst.FeeScopeEnum.FolderId.ToString() + ":" + folderId, folderId, m_TradeActionGenMQueue.ProcessType.ToString(), LockTools.Exclusive.ToString());
                            ret = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                        }
                        #endregion Lock de l'ordre et/ou du dossier

                        if (ProcessStateTools.IsCodeReturnSuccess(ret))
                        {
                            //Pour chaque payment OnFeeScopeOrderIdOrFolderId_WithMinMax opérer un recalcul 
                            //- si le résultat est identique --> RAS
                            //- si le résultat est différent --> MAJ du payment et MAJ du TRADEXML
                            if (existsPayment_OnFeeScopeOrderId)
                                ret = EFS.Process.EventsGen.New_EventsGenAPI.FeeCalculation_OrderId(m_CS, m_TradeActionGenProcess,
                                                    m_tradeLibrary.DataDocument, new KeyValuePair<int, string>(DsTrade.IdT, DsTrade.Identifier),
                                                    ref isTodoUpdateTradeXML);
                        }
                    }

                    #region MAJ des tables SQL (EVENT et TRADE)
                    ArrayList alPayments = new System.Collections.ArrayList();
                    alPayments.AddRange(m_tradeLibrary.CurrentTrade.OtherPartyPayment);

                    IProduct product = m_tradeLibrary.CurrentTrade.Product;

                    int idE_Event = GetIdE_TRDDAT();
                    DateTime businessDate = new ProductContainer(product, m_tradeLibrary.DataDocument).GetBusinessDate2();

                    //Prepare Payments
                    int nbEvent = 0;
                    IPayment[] payments_Prepared = EventQuery.PrepareFeeEvents(m_CS, product, m_tradeLibrary.DataDocument, DsTrade.IdT, alPayments, ref nbEvent);

                    IDbTransaction dbTransaction = null;
                    try
                    {
                        dbTransaction = DataHelper.BeginTran(m_CS);

                        //Insert EVENT
                        SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);
                        EventQuery eventQuery = new EventQuery(m_Process.Session, m_Process.ProcessType, m_Process.Tracker.IdTRK_L);
                        eventQuery.InsertFeeEvents(m_CS, dbTransaction, m_tradeLibrary.DataDocument, DsTrade.IdT, businessDate, idE_Event, payments_Prepared, newIdE);

                        //Update TRADE
                        if (isTodoUpdateTradeXML)
                        {
                            EventQuery.UpdateTradeXMLForFees(dbTransaction, DsTrade.IdT, m_tradeLibrary.DataDocument, (IPayment[])alPayments.ToArray(typeof(IPayment)), OTCmlHelper.GetDateSysUTC(m_CS), Process.UserId,
                                            Process.Session, Process.Tracker.IdTRK_L, Process.IdProcess);
                        }

                        DataHelper.CommitTran(dbTransaction);

                        // LOG-07021 => Evénements de frais générés avec succès.
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7021), 0));
                        
                        m_TradeActionGenProcess.AddLogFeeInformation(m_tradeLibrary.DataDocument, (IPayment[])alPayments.ToArray(typeof(IPayment)), LogLevelDetail.LEVEL3, 1);
                    }
                    catch (Exception)
                    {
                        ret = Cst.ErrLevel.FAILURE;
                        if (null != dbTransaction)
                            DataHelper.RollbackTran(dbTransaction);
                        throw;
                    }
                    finally
                    {
                        if (null != dbTransaction)
                            dbTransaction.Dispose();
                    }
                    #endregion MAJ des tables SQL (EVENT et TRADE)
                }
            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Fees already exists on trade (Identifier: {0})", m_DsTrade.Identifier),
                    new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnAbortedEnum));
            }

            return ret;
        }
    }
}
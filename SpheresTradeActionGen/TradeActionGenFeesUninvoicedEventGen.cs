using EFS.ACommon;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML;
using EfsML.Business;
using System.Collections.Generic;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using System;
using System.Data;
using System.Linq;
using FpML.Interface;
using System.Collections;
using EFS.Common;

namespace EFS.Process
{
    /// <summary>
    /// Génération des évènements de frais non facturés après modification (via GUI)
    /// </summary>
    /// EG 20240123 [WI818] Trade input: Modification of periodic fees uninvoiced on a trade - Processing

    public class TradeActionGenFeesUninvoicedEventGen : TradeActionGenProcessBase
    {

        public TradeActionGenFeesUninvoicedEventGen(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
        {
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7020), 0,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenMQueue.identifier, m_TradeActionGenMQueue.id))));
        }
        /// <summary>
        /// Chargement du Dataset (TRADE et tables EVENT)
        /// </summary>
        public override void InitializeDataSetEvent()
        {
            m_DsEvents = new DataSetEventTrade(m_CS, SlaveDbTransaction, m_DsTrade.IdT);
            m_DsEvents.Load(EventTableEnum.None, null, null);
        }
        /// <summary>
        /// Création des événements de frais non facturés suite à modification des frais
        /// Les événements de frais non facturés ont été au préalable supprimés (et pas marqués RMV) 
        /// à la validation de la modification avec mode = (CaptureMode = UpdateFeesUnvoiced )
        /// </summary>
        /// <returns></returns>
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            EFS.TradeInformation.TradeInput tradeInput = new EFS.TradeInformation.TradeInput();
            tradeInput.InitializeInvoicedFees(m_CS, DsTrade.IdT);
            tradeInput.DataDocument = m_tradeLibrary.DataDocument;

            // Contrôle que seuls les événements de frais déjà facturés sont présents dans la table EVENT
            IEnumerable<int> eventOPP = (from item in DtEvent.AsEnumerable()
                                         where item["EVENTCODE"].ToString() == "OPP"
                                         select Convert.ToInt32(item["IDE"])).Except(from x in tradeInput.feesAlreadyInvoiced select x.idE);


            if (0 == eventOPP.Count())
            {
                // Insertion des nouveaux d'événements OPP post modification
                if (m_tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    IProduct product = m_tradeLibrary.CurrentTrade.Product;
                    DateTime businessDate = new ProductContainer(product, m_tradeLibrary.DataDocument).GetBusinessDate2();

                    // Récupération de l'IDE racine des OPP (TRD/DAT)
                    int idE_Event = (from item in DtEvent.AsEnumerable()
                                     where item["EVENTCODE"].ToString() == "TRD" && item["EVENTTYPE"].ToString() == "DAT"
                                     select Convert.ToInt32(item["IDE"])).First();

                    // Extraction des OPP candidats à écriture dans EVENT (les OPP déjà facturés sont exclus)
                    IPayment[] paymentCandidates = (from item in m_tradeLibrary.CurrentTrade.OtherPartyPayment
                                                              where tradeInput.PaymentIsUninvoiced(item as IPayment)
                                                              select item).ToArray();

                    //Preparation des événements Payments
                    int nbEvent = 0;
                    IPayment[] paymentsResults = EventQuery.PrepareFeeEvents(m_CS, product, m_tradeLibrary.DataDocument, DsTrade.IdT, paymentCandidates, ref nbEvent);

                    IDbTransaction dbTransaction = null;
                    try
                    {
                        dbTransaction = DataHelper.BeginTran(m_CS);

                        //Insertion dans EVENT
                        SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);
                        EventQuery eventQuery = new EventQuery(m_Process.Session, m_Process.ProcessType, m_Process.Tracker.IdTRK_L);
                        eventQuery.InsertFeeEvents(m_CS, dbTransaction, m_tradeLibrary.DataDocument, DsTrade.IdT, businessDate, idE_Event, paymentsResults, newIdE);

                        DataHelper.CommitTran(dbTransaction);

                        // LOG-07021 => Evénements de frais générés avec succès.
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7021), 0));

                        m_TradeActionGenProcess.AddLogFeeInformation(m_tradeLibrary.DataDocument, paymentsResults, LogLevelDetail.LEVEL3, 1);
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
                }
            }
            else
            {
                ret = Cst.ErrLevel.FAILURE;
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 7010), 0,
                    new LogParam(LogTools.IdentifierAndId(m_TradeActionGenMQueue.identifier, m_TradeActionGenMQueue.id))));
            }
            return ret;
        }
    }
}

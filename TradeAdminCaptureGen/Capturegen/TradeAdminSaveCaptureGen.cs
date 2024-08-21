#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.Business;
using EfsML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// 
    /// </summary>
    /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
    public sealed partial class TradeAdminCaptureGen
    {
        /// <summary>
        /// Alimente seulement la table TRADESTREAM pour un trade ADMIN
        /// </summary>
        /// <param name="pCS">Chaine de connection</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pTradeProduct">product ADMIN</param>
        /// <param name="pIsUpdateOnly_TradeInstrument">Mise à jour de TRADEINSTRUMENT uniquement</param>
        /// RD 20130109 [18314] update only asset if trade included on invoice or partial modification mode
        protected override void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct, bool pIsUpdateOnly_TradeInstrument)
        {
            if (false == pIsUpdateOnly_TradeInstrument)
                InsertTradeStream(pCS, pDbTransaction, pIdT, pProduct);
        }

        #region Insert TradeStream
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void InsertTradeStream(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct)
        {
            int streamNo = 1;
            int instrumentNo = Convert.ToInt32(pProduct.ProductBase.Id.Replace(Cst.FpML_InstrumentNo, string.Empty));

            TradeAdminStreamInfo tradeAdminStreamInfo;
            if (pProduct.ProductBase.IsInvoice || pProduct.ProductBase.IsAdditionalInvoice || pProduct.ProductBase.IsCreditNote)
            {
                #region Invoice
                IInvoice invoice = (IInvoice)pProduct.Product;
                tradeAdminStreamInfo = new TradeAdminStreamInfo(pIdT, instrumentNo, streamNo)
                {
                    IdC1 = invoice.NetTurnOverAmount.Currency,
                    IdC2 = invoice.NetTurnOverIssueAmount.Currency
                };
                tradeAdminStreamInfo.SetIdParty(TradeCommonInput, invoice.PayerPartyReference.HRef, invoice.ReceiverPartyReference.HRef);
                tradeAdminStreamInfo.Insert(pCS, pDbTransaction);
                #endregion Invoice
            }
            else if (pProduct.ProductBase.IsInvoiceSettlement)
            {
                #region Invoice
                IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)pProduct.Product;
                tradeAdminStreamInfo = new TradeAdminStreamInfo(pIdT, instrumentNo, streamNo)
                {
                    IdC1 = invoiceSettlement.NetCashAmount.Currency,
                    IdC2 = invoiceSettlement.SettlementAmount.Currency
                };
                tradeAdminStreamInfo.SetIdParty(TradeCommonInput, invoiceSettlement.PayerPartyReference.HRef, invoiceSettlement.ReceiverPartyReference.HRef);
                tradeAdminStreamInfo.Insert(pCS, pDbTransaction);
                #endregion Invoice
            }
            else
            {
                throw new Exception("Error, Current product is not managed");
            }
        }
        #endregion Insert TradeStream



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        protected override void SaveTradelink(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, string pIdentifier)
        {
            base.SaveTradelink(pCS, pDbTransaction, pCaptureMode, pIdT, pIdentifier);

            if (TradeCommonInput.Product.IsInvoiceSettlement)
            {
                IInvoiceSettlement settlement = (IInvoiceSettlement)TradeCommonInput.Product.Product;
                if (settlement.AllocatedInvoiceSpecified)
                {
                    bool isNewAllocation = true;
                    foreach (IAllocatedInvoice allocatedInvoice in settlement.AllocatedInvoice)
                    {
                        if (Cst.Capture.IsModeUpdateAllocatedInvoice(pCaptureMode))
                            isNewAllocation = allocatedInvoice.Id.StartsWith("NEW_");

                        if (isNewAllocation)
                        {
                            TradeLink.TradeLink TradeLink = new TradeLink.TradeLink(
                            pIdT, allocatedInvoice.OTCmlId,
                            EFS.TradeLink.TradeLinkType.StlInvoice, null, null,
                            new string[2] { pIdentifier, allocatedInvoice.Identifier.Value },
                            new string[2] { EFS.TradeLink.TradeLinkDataIdentification.InvoiceSettlementIdentifier.ToString(),
                                EFS.TradeLink.TradeLinkDataIdentification.InvoiceIdentifier.ToString() });
                            TradeLink.Insert(pCS, pDbTransaction);
                        }
                    }
                }
            }
        }


        #region UpdateTradeSourceStUsedBy
        // EG 20100208 add productContainer
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override void UpdateTradeSourceStUsedBy(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            base.UpdateTradeSourceStUsedBy(pCS, pDbTransaction, pCaptureMode, pSessionInfo, pDtSys);
            if (Input.Product.IsInvoiceSettlement)
            {
                IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)Input.Product.Product;
                if (invoiceSettlement.AllocatedInvoiceSpecified && (0 < invoiceSettlement.AllocatedInvoice.Length))
                {
                    Cst.StatusUsedBy stUsedBy = Cst.StatusUsedBy.RESERVED;
                    string libStUsedBy = Cst.ProcessTypeEnum.EVENTSGEN.ToString();
                    foreach (IAllocatedInvoice allocated in invoiceSettlement.AllocatedInvoice)
                    {
                        if (allocated.Id.StartsWith("NEW_"))
                            UpdateTradeStSysUsedBy(pCS, pDbTransaction, allocated.OTCmlId, pSessionInfo, pDtSys, stUsedBy, libStUsedBy);
                    }
                }
            }
        }
        #endregion UpdateTradeSourceStUsedBy

    }

    /// <summary>
    /// 
    /// </summary>
    public class TradeAdminStreamInfo : TradeStreamInfoShortForm
    {
        #region Constructors
        public TradeAdminStreamInfo(int pIdT, int pInstrumentNo, int pStreamNo)
            : base(pIdT, pInstrumentNo, pStreamNo)
        {
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected override QueryParameters GetQueryInsert(string pCS)
        {
            return base.GetQueryInsert(pCS);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected override QueryParameters GetQueryUpdate(string pCS)
        {
            StrBuilder update = new StrBuilder();
            update += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRADESTREAM + SQLCst.SET + Cst.CrLf;
            update += "IDC2=@IDC2" + Cst.CrLf;
            update += SQLCst.WHERE + "IDT=@IDT and INSTRUMENTNO=@INSTRUMENTNO and STREAMNO=@STREAMNO";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), m_IdT);
            dataParameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), m_InstrumentNo);
            dataParameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), m_StreamNo);
            dataParameters.Add(new DataParameter(pCS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC2());

            QueryParameters ret = new QueryParameters(pCS, update.ToString(), dataParameters);
            return ret;
        }
        #endregion Methods

    }

}

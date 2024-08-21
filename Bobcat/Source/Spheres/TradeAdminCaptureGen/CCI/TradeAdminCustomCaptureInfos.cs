#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region TradeAdminCustomCaptureInfos
    /// <summary>
    /// Ccis Facturation
    /// </summary>
    public sealed class TradeAdminCustomCaptureInfos : TradeCommonCustomCaptureInfos, ICloneable
    {
        #region CCst
        public new sealed class CCst
        {
            public CCst() { }
            #region Constants
            public const string Prefix_AdditionalInvoice = "additionalInvoice";
            public const string Prefix_InvoiceAllocatedAmount = "allocatedAmount";
            public const string Prefix_CreditNote = "creditNote";
            public const string Prefix_Invoice = "invoice";
            public const string Prefix_InvoiceSettlement = "invoiceSettlement";
            public const string Prefix_InvoiceSettlementHeader = "invoiceSettlementHeader";
            public const string Prefix_AllocatedInvoice = "allocatedInvoice_body";
            public const string Prefix_AvailableInvoice = "availableInvoice_body";
            public const string Prefix_InvoiceTrade = "invoiceTrade";
            public const string Prefix_InvoiceFee = "invoiceFee";
            public const string Prefix_InvoiceRebateConditions = "rebateConditions";
            public const string Prefix_InvoiceRebateCapConditions = "cap";
            public const string Prefix_InvoiceRebateBracketConditions = "bracket";
            public const string Prefix_InvoiceRebateBracketCalculation = "calculation";
            public const string PAYMENT_BUSINESS_CENTERS_REFERENCE = "paymentBusinessCenters";
            public const string INVOICEDATE_REFERENCE = "invoiceDate";
            public const string RECEPTIONDATE_REFERENCE = "receptionDate";
            public const string INVOICEPAYMENTDATE_REFERENCE = "invoicePaymentDate";
            #endregion Constants
        }
        #endregion CCst

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public CciTradeAdminBase CciTradeAdmin
        {
            get { return (CciTradeAdminBase)CciContainer; }
            set { CciContainer = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInput TradeCommonInput
        {
            get { return (TradeCommonInput)Obj; }
        }

        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeCommonInput"></param>
        public TradeAdminCustomCaptureInfos(string pCS, TradeCommonInput pTradeCommonInput) :
            this(pCS, pTradeCommonInput, null, string.Empty, true) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="User"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Modifcation de signature
        public TradeAdminCustomCaptureInfos(string pCS, TradeCommonInput pTradeCommonInput, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
            : base(pCS,pTradeCommonInput, pUser, pSessionId, pIsGetDefaultOnInitializeCci) { }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// 1/ Déversement des CCI sur l'IHM
        /// 2/ Mise à Disabled de certains contrôles
        /// 3/ Reload de certaines DDL
        /// </summary>
        /// <param name="pPage"></param>
        public override void Dump_ToGUI(CciPageBase pPage)
        {
            base.Dump_ToGUI(pPage);
            CciTradeAdmin.DumpSpecific_ToGUI(pPage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        public override void InitializeCci(CustomCaptureInfo pCci, SQL_Table pSql_Table, string pData)
        {
            base.InitializeCci(pCci, pSql_Table, pData);
        }

        /// <summary>
        /// Synchronize les différents pointeurs du dataDocument existants dans les cciContainers  
        /// </summary>
        public override void InitializeCciContainer()
        {
            //[NewProduct] Code événtuellement à enrichir lors de l'ajout d'un nouveau produit
            CciTradeAdmin = null;
            DataDocumentContainer document = TradeCommonInput.DataDocument;
            ProductContainer product = TradeCommonInput.Product;
            //
            if (null != document)
            {
                if (product.IsInvoice)
                    CciTradeAdmin = new CciInvoice(this);
                else if (product.IsAdditionalInvoice || product.IsCreditNote)
                    CciTradeAdmin = new CciInvoiceSupplement(this);
                else if (product.IsInvoiceSettlement)
                    CciTradeAdmin = new CciInvoiceSettlement(this);

                if (null != CciTradeAdmin)
                    CciTradeAdmin.Initialize(CciTradeAdmin.PrefixHeader);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pModeCapture"></param>
        public override void InitializeDocument(Cst.Capture.ModeEnum pModeCapture)
        {

            if (CciContainerSpecified)
            {
                if ((false == IsPreserveData) && Cst.Capture.IsModeNewOrDuplicate(pModeCapture))
                    CciTradeAdmin.Initialize_Document();
                //
                base.InitializeDocument(pModeCapture);
            }

        }

        #endregion Methods

        // EG 20230526 [WI640] New override Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
        protected override void SetCssClassOnPartiesFromSide(PlaceHolder plh)
        {
            Boolean isModeCapture = Cst.Capture.IsModeConsult(this.CaptureMode);

            for (int i = 0; i < CciTradeCommon.PartyLength; i++)
            {
                CustomCaptureInfo cciSide = CciTradeCommon.cciParty[i].Cci(CciTradeParty.CciEnum.side);
                if (null != cciSide)
                {
                    if (plh.FindControl("divtradeAdminHeader_party" + (i + 1).ToString() + "_party") is WCTogglePanel pnlParty)
                    {
                        if (pnlParty.CssClass.EndsWith("invoicing") ||
                            pnlParty.CssClass.EndsWith("blue") ||
                            pnlParty.CssClass.EndsWith("red") ||
                            pnlParty.CssClass.EndsWith("gray") ||
                            pnlParty.CssClass.EndsWith("green"))
                        {
                            if (cciSide.ListRetrieval == "predef:invpayrec")
                            {
                                if (cciSide.NewValue == "BUY")
                                {
                                    //Debit
                                    pnlParty.CssClass = pnlParty.CssClass.Replace("invoicing", "red").Replace("blue", "red").Replace("gray", "red");
                                }
                                else if (cciSide.NewValue == "SELL")
                                {
                                    //Credit
                                    pnlParty.CssClass = pnlParty.CssClass.Replace("invoicing", "blue").Replace("red", "blue").Replace("gray", "blue");
                                }
                            }
                        }
                        Control pnlPartyHeader = plh.FindControl("tradeAdminHeader_party" + (i + 1).ToString() + "_ptyHeader");
                        if (null != pnlPartyHeader)
                            SetChildSideColorParty(pnlPartyHeader, cciSide, isModeCapture);
                    }
                }
            }
        }


        #region ICloneable Members
        public object Clone()
        {
            return Clone(CustomCaptureInfo.CloneMode.CciAll);
        }
        public object Clone(CustomCaptureInfo.CloneMode pCloneMode)
        {
            TradeAdminCustomCaptureInfos clone = new TradeAdminCustomCaptureInfos(CS, TradeCommonInput, User, SessionId, IsGetDefaultOnInitializeCci);
            foreach (CustomCaptureInfo cci in this)
            {
                clone.Add((CustomCaptureInfo)cci.Clone(pCloneMode));
            }
            return clone;
        }
        #endregion ICloneable Members
    }
    #endregion TradeAdminCustomCaptureInfos
}

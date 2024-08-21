using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Permission;
using EFS.TradeInformation;
using EFS.TradeLink;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{

    /// <summary>
    /// Description résumée de TradeAdminHeaderBanner.
    /// </summary>
    public class TradeAdminHeaderBanner : TradeCommonHeaderBanner
    {
        #region Members
        private readonly TradeAdminInput m_TradeAdminInput;
        private readonly TradeAdminInputGUI m_TradeAdminInputGUI;
        #endregion Members
        #region Accessors
        #region TradeCommonInputGUI
        protected override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)m_TradeAdminInputGUI; }
        }
        #endregion TradeCommonInputGUI
        #region TradeCommonInput
        protected override TradeCommonInput TradeCommonInput
        {
            get { return (TradeCommonInput)m_TradeAdminInput; }
        }
        #endregion TradeCommonInput
        #region isIdentifierReadOnly
        protected override bool IsIdentifierReadOnly
        {
            get { return true; }
        }
        #endregion isIdentifierReadOnly

        #endregion Accessors

        #region Constructors
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public TradeAdminHeaderBanner(PageBase pParentControl, string pGUID, Control pCtrlContainer,
            TradeAdminInput pTradeAdminInput, TradeAdminInputGUI pTradeAdminInputGUI, bool pIsWithStatusButton)
            : base(pParentControl, pGUID, pCtrlContainer, pIsWithStatusButton, pTradeAdminInputGUI.MainMenuClassName)
        {
            m_TradeAdminInput = pTradeAdminInput;
            m_TradeAdminInputGUI = pTradeAdminInputGUI;
            NbDisplayProcess = 5;
        }
        #endregion Constructors
        #region Methods
        protected override string GetSelectState()
        {
            string sqlSelect = string.Empty;
            // ProcessState : EVENT
            sqlSelect += GetSelectState_Event(1, "Event");
            // ProcessState : CONF
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(2, Cst.ProcessTypeEnum.CMGEN, "Conf.");
            // ProcessState : EAR
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(3, Cst.ProcessTypeEnum.EARGEN, "EAR");
            // ProcessState : ACCT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(4, Cst.ProcessTypeEnum.ACCOUNTGEN, "Acct.");

            sqlSelect += SQLCst.ORDERBY + "SORT1, SORT2" + Cst.CrLf;
            return sqlSelect;
        }
        // EG 20150618 New
        protected override DataParameter GetDtBusinessState(string pCS)
        {
            return null;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsRefresh"></param>
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        // EG 20200903 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        // EG 20200921 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override void DisplayTradeLink(bool pIsRefresh)
        {
            string cssClass = CstCSS.LblError;

            Table tblTradeLink = m_Page.FindControl(ctrlTblTradelink) as Table;
            bool isDisplay = Cst.Capture.IsModeConsult(TradeCommonInputGUI.CaptureMode) && (IsTradeLink || IsTradeRemoved);
            if (IsTradeRemoved)
            {
                #region IsTradeRemoved

                TableRow tr = new TableRow
                {
                    CssClass = cssClass
                };
                TableCell td = new TableCell();
                // EG 20170125 [Refactoring URL]
                Panel pnl = new Panel { CssClass = "fa-icon fas fa-circle red"};
                td.Controls.Add(pnl);
                tr.Cells.Add(td);

                tr.Cells.Add(td);

                //
                td = new TableCell
                {
                    HorizontalAlign = HorizontalAlign.Left,
                    Wrap = false,
                    // FI 20200820 [25468] dates systemes en UTC. Affichage selon le profile avec précision à la minute
                    //Text = DtFunc.DateTimeToString(dtDisplay, DtFunc.FmtShortDate) + Cst.HTMLSpace + DtFunc.DateTimeToString(dtDisplay, DtFunc.FmtShortTime) + Cst.HTMLSpace
                    Text = DtFuncExtended.DisplayTimestampUTC(TradeCommonInput.SQLLastTradeLog.DtSys, new AuditTimestampInfo()
                    {
                        Collaborator = SessionTools.Collaborator,
                        TimestampZone = SessionTools.AuditTimestampZone,
                        Precision = Cst.AuditTimestampPrecision.Minute
                    })
                };
                tr.Cells.Add(td);
                //
                td = new TableCell
                {
                    Wrap = false,
                    HorizontalAlign = HorizontalAlign.Left
                };
                td.Font.Bold = true;
                td.Text = Ressource.GetString("TradeCanceled").Replace(@" ", Cst.HTMLSpace) + Cst.HTMLSpace;
                tr.Cells.Add(td);
                //
                td = new TableCell
                {
                    HorizontalAlign = HorizontalAlign.Left,
                    Wrap = false,
                    Text = GetLastTradeLogOperator()
                };
                tr.Cells.Add(td);
                //
                td = new TableCell
                {
                    Text = Cst.HTMLSpace,
                    Width = Unit.Percentage(100)
                };
                //
                tr.Cells.Add(td);
                tblTradeLink.Rows.Add(tr);
                #endregion
            }
            else if (IsTradeFound && IsTradeLink && pIsRefresh && (null != tblTradeLink))
                isDisplay &= ConstructTradeLinkInfo(tblTradeLink);

            if (m_Page.FindControl(ctrlToggleTradelink) is WCTogglePanel pnlTradeLink)
            {
                if (isDisplay)
                {
                    if (IsTradeRemoved)
                        pnlTradeLink.CssClass = m_Page.CSSMode + " red";
                    ControlsTools.RemoveStyleDisplay(pnlTradeLink);
                }
                else
                    pnlTradeLink.Style[HtmlTextWriterStyle.Display] = "none!important";
            }
        }


        /// <summary>
        /// Recherche et construction des lignes TRADELINK à afficher pour les TRADEADMIN
        /// </summary>
        /// <param name="pTblTradeLink">HTMLTable réceptrice</param>
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override bool ConstructTradeLinkInfo(Table pTblTradeLink)
        {
            
            string action = string.Empty;
            string identifier = string.Empty;
            string led = "red";
            string cssClass = CstCSS.LblError;
            int id = 0;
            DataRow[] rows;
            if (TradeCommonInput.Product.IsInvoice)
                rows = TradeCommonInput.SQLTradeLink.Dt.Select("IDT_B=" + TradeCommonInput.IdT);
            else
                rows = TradeCommonInput.SQLTradeLink.Dt.Select();

            #region foreach
            foreach (DataRow row in rows)
            {
                TradeLinkType tradelinkType = TradeLinkType.NA;
                if (Enum.IsDefined(typeof(TradeLinkType), row["LINK"].ToString()))
                    tradelinkType = (TradeLinkType)Enum.Parse(typeof(TradeLinkType), row["LINK"].ToString(), true);

                int idT_A = Convert.ToInt32(row["IDT_A"]);
                int idT_B = Convert.ToInt32(row["IDT_B"]);
                if (idT_A == TradeCommonInput.Identification.OTCmlId)
                {
                    #region
                    PermissionEnum perm_A = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), row["ACTION_A"].ToString(), true);
                    action = PermissionTools.GetRessource(perm_A, false) + " ( " + row["LINK"].ToString() + " - " + row["MESSAGE"].ToString() + " )";
                    identifier = row["IDENTIFIER_B"].ToString();
                    id = idT_B;
                    switch (tradelinkType)
                    {
                        case TradeLinkType.AddInvoice:
                        case TradeLinkType.CreditNote:
                        case TradeLinkType.StlInvoice:
                            action = Ressource.GetString("LinkedInvoice");
                            identifier = row["DATA2"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            break;
                        case TradeLinkType.RemoveStlInvoice:
                            action = Ressource.GetString("RemoveAmountLinkedInvoice");
                            identifier = row["DATA2"].ToString();
                            led = "orange";
                            cssClass = CstCSS.LblWarning;
                            break;
                        case TradeLinkType.RemoveInvoice:
                            action = Ressource.GetString("RemoveLinkedInvoice");
                            if (EFS.TradeLink.TradeLinkDataIdentification.CreditNoteIdentifier.ToString() == row["DATA2IDENT"].ToString())
                                action = Ressource.GetString("RemoveLinkedCreditNote");
                            identifier = row["DATA2"].ToString();
                            led = "orange";
                            cssClass = CstCSS.LblWarning;
                            break;
                        case TradeLinkType.NewIdentifier:
                            action = Ressource.GetString("TradeNewIdentifier");
                            identifier = row["DATA1"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            break;
                    }
                    #endregion
                }
                else if (idT_B == TradeCommonInput.Identification.OTCmlId)
                {
                    #region
                    identifier = row["IDENTIFIER_A"].ToString();
                    id = idT_A;
                    //
                    switch (tradelinkType)
                    {
                        case TradeLinkType.AddInvoice:
                        case TradeLinkType.CreditNote:
                        case TradeLinkType.Invoice:
                        case TradeLinkType.StlInvoice:
                        case TradeLinkType.RemoveStlInvoice:
                        case TradeLinkType.RemoveInvoice:
                            if (TradeLinkType.AddInvoice == tradelinkType)
                                action = Ressource.GetString("AdditionalInvoice");
                            else if (TradeLinkType.CreditNote == tradelinkType)
                                action = Ressource.GetString("CreditNote");
                            else if (TradeLinkType.Invoice == tradelinkType)
                            {
                                if (Convert.IsDBNull(row["DATA3"]))
                                    action = Ressource.GetString("LinkedInvoice");
                                else
                                    action = Ressource.GetString2("LinkedInvoiceValidated", row["DATA3"].ToString());
                            }
                            else if (TradeLinkType.StlInvoice == tradelinkType)
                                action = Ressource.GetString("LinkedSettlementInvoice");
                            else if (TradeLinkType.RemoveStlInvoice == tradelinkType)
                                action = Ressource.GetString("LinkedRemoveSettlementInvoice");
                            else if (TradeLinkType.RemoveInvoice == tradelinkType)
                                action = Ressource.GetString("LinkedRemoveInvoice");

                            identifier = row["DATA1"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            break;
                    }
                    #endregion
                }
                // EG 20170125 [Refactoring URL]
                //string url = SpheresURL.GetUrlFormTradeAdmin("IDT", id.ToString(), SessionTools.Menus);
                string url = SpheresURL.GetURL(IdMenu.Menu.InputTradeAdmin, id.ToString());
                
                FillTradeLinkInfoRow(pTblTradeLink, DateTime.SpecifyKind(Convert.ToDateTime(row["DTSYS"]), DateTimeKind.Utc), cssClass, led, action, url, identifier, id);
            }
            return (null != rows) && (0 < rows.Length);
            #endregion foreach
        }
        #endregion Methods
    }

}

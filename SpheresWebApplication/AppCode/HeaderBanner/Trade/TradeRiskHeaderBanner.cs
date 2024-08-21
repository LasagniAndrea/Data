using System;
using System.Drawing;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Linq; 


using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.Status;
using EFS.Permission;
using EFS.TradeInformation;
using EFS.ApplicationBlocks.Data;
using EFS.TradeLink;
// EG 20150618 New
using EfsML.Enum;
using EfsML.Enum.Tools;
// EG 20150618 New
using EfsML.Business;

namespace EFS.Spheres
{

    /// <summary>
    /// Description résumée de TradeRiskHeaderBanner.
    /// </summary>
    public class TradeRiskHeaderBanner : TradeCommonHeaderBanner
    {
        #region Members
        private readonly TradeRiskInput _input;
        private readonly TradeRiskInputGUI _inputGUI;
        #endregion Members

        #region Accessors
        #region TradeCommonInputGUI
        protected override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)_inputGUI; }
        }
        #endregion TradeCommonInputGUI
        #region TradeCommonInput
        protected override TradeCommonInput TradeCommonInput
        {
            get { return (TradeCommonInput)_input; }
        }
        #endregion TradeCommonInput
        #region isWithNotification
        protected override bool IsWithNotification
        {
            get { return false; }
        }
        #endregion
        #region isIdentifierReadOnly
        protected override bool IsIdentifierReadOnly
        {
            get { return false; }
        }
        #endregion
        #endregion Accessors

        #region Constructors
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public TradeRiskHeaderBanner(PageBase pParentControl, string pGUID, Control pCtrlContainer,
            TradeRiskInput pInput, TradeRiskInputGUI pInputGUI, bool pIsWithStatusButton)
            : base(pParentControl, pGUID, pCtrlContainer, pIsWithStatusButton, pInputGUI.MainMenuClassName)
        {
            _input = pInput;
            _inputGUI = pInputGUI;
            NbDisplayProcess = 3;
        }
        #endregion Constructors
        #region Methods
        protected override string GetSelectState()
        {
            string sqlSelect = string.Empty;
            // ProcessState : EVENT
            sqlSelect += GetSelectState_Event(1, "Event");
            // ProcessState : EAR
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(2, Cst.ProcessTypeEnum.EARGEN, "EAR");
            // ProcessState : ACCT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(3, Cst.ProcessTypeEnum.ACCOUNTGEN, "Acct.");
            sqlSelect += SQLCst.ORDERBY + "SORT1, SORT2" + Cst.CrLf;
            return sqlSelect;
        }
        // EG 20150618 New
        protected override DataParameter GetDtBusinessState(string pCS)
        {
            return null;
        }

        /// <summary>
        /// Recherche et construction des lignes TRADELINK à afficher pour les TRADES RISKMANAGEMENT
        /// </summary>
        /// <param name="pTblTradeLink">HTMLTable réceptrice</param>
        // EG 20170125 [Refactoring URL]
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override bool ConstructTradeLinkInfo(Table pTblTradeLink)
        {
            
            string action = string.Empty;
            string identifier = string.Empty;
            string led = "red";
            string cssClass = CstCSS.LblError;
            string url = string.Empty;
            int id = 0;
            DataRow[] rows = TradeCommonInput.SQLTradeLink.Dt.Select("GPRODUCT_A='" + Cst.ProductGProduct_RISK +
                                                                     "' and GPRODUCT_B='" + Cst.ProductGProduct_RISK + "'");

            #region foreach
            foreach (DataRow row in rows)
            {
                TradeLinkType tradelinkType = TradeLinkType.NA;
                if (Enum.IsDefined(typeof(TradeLinkType), row["LINK"].ToString()))
                    tradelinkType = (TradeLinkType)Enum.Parse(typeof(TradeLinkType), row["LINK"].ToString(), true);

                int idT_A = Convert.ToInt32(row["IDT_A"]);
                int idT_B = Convert.ToInt32(row["IDT_B"]);
                PermissionEnum perm_A = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), row["ACTION_A"].ToString(), true);
                // EG 20170125 [Refactoring URL]
                IdMenu.Menu idMenu = InputTradeMenu;
                //------------------------------------------------------
                // IDT = IDT_A
                //------------------------------------------------------
                if (idT_A == TradeCommonInput.Identification.OTCmlId)
                {
                    #region
                    action = PermissionTools.GetRessource(perm_A, false) + " ( " + row["LINK"].ToString() + " - " + row["MESSAGE"].ToString() + " )";
                    identifier = row["IDENTIFIER_B"].ToString();
                    id = idT_B;
                    switch (tradelinkType)
                    {
                        case TradeLinkType.NewIdentifier:
                            action = Ressource.GetString("TradeNewIdentifier");
                            identifier = row["DATA1"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            url = string.Empty; // SpheresURL.GetUrlFormTradeAdmin("IDT", id.ToString(), SessionTools.Menus);
                            break;
                        case TradeLinkType.Replace:
                            if (PermissionEnum.Create == perm_A)
                                action = Ressource.GetString("TradeReplaces");
                            led = "orange";
                            cssClass = CstCSS.LblWarning;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(idMenu, id.ToString());
                            break;
                        case TradeLinkType.Reflect:
                            action = Ressource.GetString("TradeIsReflectionOf");
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(idMenu, id.ToString());
                            
                            break;
                        case TradeLinkType.PrevCashBalance:
                        case TradeLinkType.MarginRequirementInCashBalance:
                        case TradeLinkType.CashPaymentInCashBalance:
                            // EG 20170125 [Refactoring URL]
                            if (TradeLinkType.PrevCashBalance == tradelinkType)
                            {
                                action = Ressource.GetString("PrevCashBalance");
                                idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                            }
                            else if (TradeLinkType.MarginRequirementInCashBalance == tradelinkType)
                            {
                                action = Ressource.GetString("LinkedMarginRequiremnt");
                                idMenu = IdMenu.Menu.InputTradeRisk_InitialMargin;
                            }
                            else if (TradeLinkType.CashPaymentInCashBalance == tradelinkType)
                            {
                                action = Ressource.GetString("LinkedPayment");
                                idMenu = IdMenu.Menu.InputTradeRisk_CashPayment;
                            }

                            identifier = row["DATA2"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;

                            //PL 20140930 TBD
                            //FI 20140930 [XXXXX] TODO, il serait mieux de donner la famille du trade risk
                            //url = SpheresURL.GetUrlFormTradeRisk("IDT", id.ToString(), SessionTools.Menus);
                            // EG 20170125 [Refactoring URL]
                            //url = SpheresURL.GetUrlFormTradeRisk("IDT", id.ToString(), null, SessionTools.Menus);
                            url = SpheresURL.GetURL(idMenu, id.ToString());
                            break;
                    }
                    #endregion
                }
                //------------------------------------------------------
                // IDT = IDT_B
                //------------------------------------------------------
                else if (idT_B == TradeCommonInput.Identification.OTCmlId)
                {
                    #region
                    identifier = row["IDENTIFIER_A"].ToString();
                    id = idT_A;
                    //
                    switch (tradelinkType)
                    {
                        case TradeLinkType.Replace:
                            if (PermissionEnum.Create == perm_A)
                                action = Ressource.GetString("TradeIsReplacedBy");
                            led = "red";
                            cssClass = CstCSS.LblError;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTradeRisk, id.ToString());
                            break;
                        case TradeLinkType.Reflect:
                            action = Ressource.GetString("TradeIsReflectedBy");
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTradeRisk, id.ToString());
                            break;
                        case TradeLinkType.PrevCashBalance:
                        case TradeLinkType.MarginRequirementInCashBalance:
                        case TradeLinkType.CashPaymentInCashBalance:
                            // EG 20170125 [Refactoring URL]
                            if (TradeLinkType.PrevCashBalance == tradelinkType)
                            {
                                action = Ressource.GetString("NextCashBalance");
                                idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                            }
                            else if (TradeLinkType.MarginRequirementInCashBalance == tradelinkType)
                            {
                                action = Ressource.GetString("LinkedCashBalance");
                                idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                            }
                            else if (TradeLinkType.CashPaymentInCashBalance == tradelinkType)
                            {
                                action = Ressource.GetString("LinkedCashBalance");
                                idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                            }
                            identifier = row["DATA1"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            //PL 20140930 TBD
                            //FI 20140930 [XXXXX] TODO, il serait mieux de donner la famille du trade risk
                            //url = SpheresURL.GetUrlFormTradeRisk("IDT", id.ToString(), SessionTools.Menus);
                            // EG 20170125 [Refactoring URL]
                            //url = SpheresURL.GetUrlFormTradeRisk("IDT", id.ToString(), null, SessionTools.Menus);
                            url = SpheresURL.GetURL(idMenu, id.ToString());
                            break;
                    }
                    #endregion
                }
                // FI 20200820 [25468] dates systemes en UTC
                FillTradeLinkInfoRow(pTblTradeLink, DateTime.SpecifyKind(Convert.ToDateTime(row["DTSYS"]), DateTimeKind.Utc), cssClass, led, action, url, identifier, id);
            }
            return (null != rows) && (0 < rows.Length);
            #endregion foreach
        }

        #endregion Methods
    }
}
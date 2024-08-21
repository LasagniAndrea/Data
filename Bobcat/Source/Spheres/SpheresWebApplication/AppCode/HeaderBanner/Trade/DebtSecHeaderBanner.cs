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
    /// Description résumée de DebtSecHeaderBanner.
    /// </summary>
    public class DebtSecHeaderBanner : TradeCommonHeaderBanner
    {
        #region Members
        private readonly DebtSecInput _input;
        private readonly DebtSecInputGUI _inputGUI;
        #endregion Members
        //
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
        //
        #region Constructors
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public DebtSecHeaderBanner(PageBase pParentControl, string pGUID, Control pCtrlContainer,
            DebtSecInput pInput, DebtSecInputGUI pInputGUI, bool pIsWithStatusButton)
            : base(pParentControl, pGUID, pCtrlContainer, pIsWithStatusButton, pInputGUI.MainMenuClassName)
        {
            _input = pInput;
            _inputGUI = pInputGUI;
            //m_NbDisplayProcess = 5;
            NbDisplayProcess = 1;
        }
        #endregion Constructors

        #region Methods
        protected override string GetSelectState()
        {
            string sqlSelect = string.Empty;
            // ProcessState : EVENT
            sqlSelect += GetSelectState_Event(1, "Event");
            /*
            // ProcessState : CONF
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(2, Cst.ProcessTypeEnum.CMGEN, "Conf.");
            // ProcessState : SIGEN
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(3, Cst.ProcessTypeEnum.SIGEN, "Settlt.");
            // ProcessState : EAR
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(4, Cst.ProcessTypeEnum.EARGEN, "EAR");
            // ProcessState : ACCT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(5, Cst.ProcessTypeEnum.ACCOUNTGEN, "Acct.");
            */
            sqlSelect += SQLCst.ORDERBY + "SORT1, SORT2" + Cst.CrLf;
            return sqlSelect;
        }
        // EG 20150618 New
        protected override DataParameter GetDtBusinessState(string pCS)
        {
            return null;
        }

        /// <summary>
        /// Recherche et construction des lignes TRADELINK à afficher pour les TRADES DEBTSECURITY
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
            DataRow[] rows = TradeCommonInput.SQLTradeLink.Dt.Select();

            #region foreach
            foreach (DataRow row in rows)
            {
                TradeLinkType tradelinkType = TradeLinkType.NA;
                if (Enum.IsDefined(typeof(TradeLinkType), row["LINK"].ToString()))
                    tradelinkType = (TradeLinkType)Enum.Parse(typeof(TradeLinkType), row["LINK"].ToString(), true);

                int idT_A = Convert.ToInt32(row["IDT_A"]);
                int idT_B = Convert.ToInt32(row["IDT_B"]);
                PermissionEnum perm_A = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), row["ACTION_A"].ToString(), true);
                if (idT_A == TradeCommonInput.Identification.OTCmlId)
                {
                    #region IDT = IDT_A
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
                            url = string.Empty; // SpheresURL.GetUrlFormTradeDebtSecurity("IDT", id.ToString());
                            break;
                        case TradeLinkType.Replace:
                            if (PermissionEnum.Create == perm_A)
                                action = Ressource.GetString("TradeReplaces");
                            led = "orange";
                            cssClass = CstCSS.LblWarning;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(IdMenu.Menu.InputDebtSec, id.ToString());
                            break;
                        case TradeLinkType.Reflect:
                            action = Ressource.GetString("TradeIsReflectionOf");
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(IdMenu.Menu.InputDebtSec, id.ToString());
                            break;
                    }
                    #endregion IDT = IDT_A
                }
                else if (idT_B == TradeCommonInput.Identification.OTCmlId)
                {
                    #region IDT = IDT_B
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
                            url = SpheresURL.GetURL(IdMenu.Menu.InputDebtSec, id.ToString());
                            break;
                        case TradeLinkType.Reflect:
                            action = Ressource.GetString("TradeIsReflectedBy");
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            // EG 20170125 [Refactoring URL]
                            url = SpheresURL.GetURL(IdMenu.Menu.InputDebtSec, id.ToString());
                            break;
                    }
                    #endregion IDT = IDT_B
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
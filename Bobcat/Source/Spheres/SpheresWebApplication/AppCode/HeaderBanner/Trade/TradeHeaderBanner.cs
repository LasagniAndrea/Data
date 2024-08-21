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
    /// En tête Trade 
    /// </summary>
    public class TradeHeaderBanner : TradeCommonHeaderBanner
    {
        #region Members
        private readonly TradeInput _input;
        private readonly TradeInputGUI _inputGUI;
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

        #region isIdentifierReadOnly
        protected override bool IsIdentifierReadOnly
        {
            get { return base.IsIdentifierReadOnly; }
        }
        #endregion isIdentifierReadOnly

        #endregion Accessors

        #region Constructors
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public TradeHeaderBanner(PageBase pParentControl, string pGUID, Control pCtrlContainer,
            TradeInput pInput, TradeInputGUI pInputGUI, bool pIsWithStatusButton)
            : base(pParentControl, pGUID, pCtrlContainer, pIsWithStatusButton, pInputGUI.MainMenuClassName)
        {
            _input = pInput;
            _inputGUI = pInputGUI;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Recherche et construction des lignes TRADELINK à afficher pour les TRADES de MARCHE
        /// </summary>
        /// <param name="pTblTradeLink">HTMLTable réceptrice</param>
        // EG 20170412 [23081] Gestion des livraisons de sous-jacent non REGULAR
        // EG 20190613 [24683] Upd (Add ClosingPosition and ClosingReopeningPosition)
        // EG 20231030 [WI725] New Closing/Reopening : Use module to manage closing suspens after GiveUp cleared / Input
        protected override bool ConstructTradeLinkInfo(Table pTblTradeLink)
        {

            string action = string.Empty;
            string identifier = string.Empty;
            string led = "red";
            string cssClass = CstCSS.LblError;
            string url = string.Empty;
            int id = 0;
            int nbLinkRows = 0;

            DataRow[] rows = TradeCommonInput.SQLTradeLink.Dt.Select("GPRODUCT_A<>'" + Cst.ProductGProduct_RISK +
                                                         "' and GPRODUCT_B<>'" + Cst.ProductGProduct_RISK + "'");
            //PM 20150312 [POC] Ajout comptage des rows
            if (null != rows)
            {
                nbLinkRows = rows.Length;
            }
            #region foreach
            foreach (DataRow row in rows)
            {
                //PM 20150312 [POC] Ajout isLinkToDisplay pour n'afficher le lien Folder que lorsque le trade correspond à IDT_A
                bool isLinkToDisplay = true;
                //
                TradeLinkType tradelinkType = TradeLinkType.NA;
                if (Enum.IsDefined(typeof(TradeLinkType), row["LINK"].ToString()))
                    tradelinkType = (TradeLinkType)Enum.Parse(typeof(TradeLinkType), row["LINK"].ToString(), true);

                int idT_A = Convert.ToInt32(row["IDT_A"]);
                int idT_B = Convert.ToInt32(row["IDT_B"]);

                Cst.StatusActivation idStActivation_A = (Cst.StatusActivation)System.Enum.Parse(typeof(Cst.StatusActivation), row["IDSTACTIVATION_A"].ToString());
                Cst.StatusActivation idStActivation_B = (Cst.StatusActivation)System.Enum.Parse(typeof(Cst.StatusActivation), row["IDSTACTIVATION_B"].ToString());

                PermissionEnum perm_A = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), row["ACTION_A"].ToString(), true);
                if (idT_A == TradeCommonInput.Identification.OTCmlId)
                {
                    #region IDT = IDT_A
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
                            //url = SpheresURL.GetUrlFormTradeAdmin("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTradeAdmin, id.ToString());
                            break;
                        case TradeLinkType.Invoice:
                            action = Ressource.GetString("LinkedTrade");
                            identifier = row["DATA2"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
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
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        case TradeLinkType.Reflect:
                            action = Ressource.GetString("TradeIsReflectionOf");
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        case TradeLinkType.UnderlyerDeliveryAfterOptionAssignment:
                        case TradeLinkType.UnderlyerDeliveryAfterAutomaticOptionAssignment:
                        case TradeLinkType.UnderlyerDeliveryAfterAutomaticOptionExercise:
                        case TradeLinkType.UnderlyerDeliveryAfterOptionExercise:
                            action = Ressource.GetString("LinkedTrade");
                            if (idStActivation_B != Cst.StatusActivation.REGULAR)
                                action += " [" + idStActivation_A + "]";
                            identifier = row["DATA1"].ToString();

                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            switch (idStActivation_B)
                            {
                                case Cst.StatusActivation.DEACTIV:
                                case Cst.StatusActivation.REMOVED:
                                    led = "red";
                                    cssClass = CstCSS.LblError;
                                    break;
                                case Cst.StatusActivation.LOCKED:
                                    led = "gray";
                                    cssClass = CstCSS.LblUnknown;
                                    break;
                                case Cst.StatusActivation.MISSING:
                                    led = "orange";
                                    cssClass = CstCSS.LblWarning;
                                    break;
                            }
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        case TradeLinkType.PositionTransfert:
                        case TradeLinkType.PositionAfterCorporateAction:
                        case TradeLinkType.SplitTrade:
                        case TradeLinkType.MergedTrade:
                        case TradeLinkType.PositionAfterCascading:
                        case TradeLinkType.PositionAfterShifting:
                        case TradeLinkType.ClosingPosition:
                        case TradeLinkType.ReopeningPosition:
                        case TradeLinkType.GiveUpSuspens:
                            if (TradeLinkType.PositionTransfert == tradelinkType)
                                action = "TradeTransferFrom";
                            else if (TradeLinkType.PositionAfterCorporateAction == tradelinkType)
                                action = "TradeCumPostCA";
                            else if (TradeLinkType.SplitTrade == tradelinkType)
                                action = "TradeSplitOf";
                            else if (TradeLinkType.MergedTrade == tradelinkType)
                                action = "TradeMergeOf";
                            else if (TradeLinkType.PositionAfterCascading == tradelinkType)
                                action = "TradeCascadingFrom";
                            else if (TradeLinkType.PositionAfterShifting == tradelinkType)
                                action = "TradeShiftingFrom";
                            else if (TradeLinkType.ClosingPosition == tradelinkType)
                                action = "TradeClosingPositionFrom";
                            else if (TradeLinkType.ReopeningPosition == tradelinkType)
                                action = "TradeReopeningPositionFrom";
                            else if (TradeLinkType.GiveUpSuspens == tradelinkType)
                                action = "GiveUpSuspensFrom";
                            action = Ressource.GetString(action);
                            cssClass = CstCSS.LblInfo;
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;

                        // PM 20150312 [POC] Add Folder
                        case TradeLinkType.Folder:
                            // Afficher uniquement pour SysAdmin, SysOper et User non descendant d'un Client
                            if ((SessionTools.IsSessionSysAdmin || SessionTools.IsSessionSysOper) || (SessionTools.User.ActorAncestor.GetFirstRelation(EFS.Actor.RoleActor.CLIENT) == 0))
                            {
                                action = Ressource.GetString("LinkedTrade");
                                identifier = row["DATA1"].ToString();
                                led = "blue";
                                cssClass = CstCSS.LblInfo;
                                //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                                url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            }
                            else
                            {
                                isLinkToDisplay = false;
                                nbLinkRows -= 1;
                            }
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
                        case TradeLinkType.Invoice:
                            if (Convert.IsDBNull(row["DATA3"]))
                                action = Ressource.GetString("LinkedInvoice");
                            else
                                action = Ressource.GetString2("LinkedInvoiceValidated", row["DATA3"].ToString());
                            identifier = row["DATA1"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTradeAdmin, id.ToString());
                            break;
                        case TradeLinkType.Replace:
                            if (PermissionEnum.Create == perm_A)
                                action = Ressource.GetString("TradeIsReplacedBy");
                            led = "blue";
                            cssClass = CstCSS.LblError;
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        case TradeLinkType.Reflect:
                            action = Ressource.GetString("TradeIsReflectedBy");
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        case TradeLinkType.UnderlyerDeliveryAfterOptionAssignment:
                        case TradeLinkType.UnderlyerDeliveryAfterAutomaticOptionAssignment:
                        case TradeLinkType.UnderlyerDeliveryAfterAutomaticOptionExercise:
                        case TradeLinkType.UnderlyerDeliveryAfterOptionExercise:
                            string[] splitAction = tradelinkType.ToString().Replace("After", "Info;").Split(';');
                            if (ArrFunc.IsFilled(splitAction))
                                action = Ressource.GetString2(splitAction[0], Ressource.GetString(splitAction[1]));
                            identifier = row["DATA2"].ToString();
                            led = "blue";
                            cssClass = CstCSS.LblInfo;
                            if (idStActivation_A != Cst.StatusActivation.REGULAR)
                                action += " [" + idStActivation_A + "]";
                            switch (idStActivation_A)
                            {
                                case Cst.StatusActivation.DEACTIV:
                                case Cst.StatusActivation.REMOVED:

                                    led = "red";
                                    cssClass = CstCSS.LblError;
                                    break;
                                case Cst.StatusActivation.LOCKED:
                                    led = "gray";
                                    cssClass = CstCSS.LblUnknown;
                                    break;
                                case Cst.StatusActivation.MISSING:
                                    led = "orange";
                                    cssClass = CstCSS.LblWarning;
                                    break;
                            }
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        case TradeLinkType.PositionTransfert:
                        case TradeLinkType.PositionAfterCorporateAction:
                        case TradeLinkType.SplitTrade:
                        case TradeLinkType.MergedTrade:
                        case TradeLinkType.PositionAfterCascading:
                        case TradeLinkType.PositionAfterShifting:
                        case TradeLinkType.ClosingPosition:
                        case TradeLinkType.ReopeningPosition:
                        case TradeLinkType.GiveUpSuspens:
                            if (TradeLinkType.PositionTransfert == tradelinkType)
                                action = "TradeTransferTo";
                            else if (TradeLinkType.PositionAfterCorporateAction == tradelinkType)
                                action = "TradeExPostCA";
                            else if (TradeLinkType.SplitTrade == tradelinkType)
                                action = "TradeSplitIn";
                            else if (TradeLinkType.MergedTrade == tradelinkType)
                                action = "TradeMergeIn";
                            else if (TradeLinkType.PositionAfterCascading == tradelinkType)
                                action = "TradeCascadingTo";
                            else if (TradeLinkType.PositionAfterShifting == tradelinkType)
                                action = "TradeShiftingTo";
                            else if (TradeLinkType.ClosingPosition == tradelinkType)
                                action = "TradeClosingPositionTo";
                            else if (TradeLinkType.ReopeningPosition == tradelinkType)
                                action = "TradeReopeningPositionTo";
                            else if (TradeLinkType.GiveUpSuspens == tradelinkType)
                                action = "GiveUpSuspensTo";
                            action = Ressource.GetString(action);
                            cssClass = CstCSS.LblInfo;
                            //url = SpheresURL.GetUrlFormTrade("IDT", id.ToString(), SessionTools.Menus);
                            url = SpheresURL.GetURL(IdMenu.Menu.InputTrade, id.ToString());
                            break;
                        // PM 20150312 [POC] Add Folder
                        case TradeLinkType.Folder:
                            isLinkToDisplay = false;
                            nbLinkRows -= 1;
                            break;
                    }
                    #endregion IDT = IDT_B
                }
                // PM 20150312 [POC] Ajout test sur isLinkToDisplay
                if (isLinkToDisplay)
                {
                    // FI 20200820 [25468] dates systemes en UTC
                    FillTradeLinkInfoRow(pTblTradeLink, DateTime.SpecifyKind(Convert.ToDateTime(row["DTSYS"]), DateTimeKind.Utc), cssClass, led, action, url, identifier, id);
                }
            }
            // PM 20150312 [POC] Nombre de rows affichés pouvant être différent du nombre de rows réél
            //return (null != rows) && (0 < rows.Length);
            return (0 < nbLinkRows);
            #endregion foreach
        }
        #endregion Methods
    }
}

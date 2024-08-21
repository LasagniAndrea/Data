using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Permission;
using EFS.Status;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de TradeCommonHeaderBanner.
    /// </summary>
    public class TradeCommonHeaderBanner : HeaderBannerBase
    {

        /// <summary>
        /// 
        /// </summary>
        protected bool m_IsReset_IdentifierDisplaynameDescription;

        
        #region Members
        public string ctrlToggleTradelink;
        public string ctrlTblTradelink;
        public string ctrlLblActionTradelink;
        public string ctrlDspIdentifierTradelink;
        public string ctrlLblIdSysTradelink;
        public string ctrlDspIdSysTradelink;
        public string ctrlLblDtTradelink;
        public string ctrlDspDtTradelink;
        #endregion Members

        #region Accessors
        #region InputTradeMenu
        // EG 20170125 [Refactoring URL] New
        protected virtual IdMenu.Menu InputTradeMenu
        {
            get
            {
                IdMenu.Menu idMenu = IdMenu.Menu.InputTrade;

                switch (TradeCommonInput.SQLProduct.GroupProduct)
                {
                    case ProductTools.GroupProductEnum.Administrative:
                        idMenu = IdMenu.Menu.InputTradeAdmin;
                        break;
                    case ProductTools.GroupProductEnum.Asset:
                        idMenu = IdMenu.Menu.InputDebtSec;
                        break;
                    case ProductTools.GroupProductEnum.Risk:
                        switch (TradeCommonInput.SQLProduct.FamilyProduct)
                        {
                            case ProductTools.FamilyEnum.CashBalance:
                                idMenu = IdMenu.Menu.InputTradeRisk_CashBalance;
                                break;
                            case ProductTools.FamilyEnum.CashInterest:
                                idMenu = IdMenu.Menu.InputTradeRisk_CashInterest;
                                break;
                            case ProductTools.FamilyEnum.CashPayment:
                                idMenu = IdMenu.Menu.InputTradeRisk_CashPayment;
                                break;
                            case ProductTools.FamilyEnum.Margin:
                                idMenu = IdMenu.Menu.InputTradeRisk_InitialMargin;
                                break;
                            default:
                                idMenu = IdMenu.Menu.InputTradeRisk;
                                break;
                        }
                        break;
                }
                return idMenu;
            }
        }
        #endregion InputTradeMenu

        #region FamilyProduct
        protected virtual ProductTools.FamilyEnum FamilyProduct
        {
            get 
            {
                return TradeCommonInput.SQLProduct.FamilyProduct; 
            }
        }
        #endregion FamilyProduct
        #region TradeCommonInputGUI
        protected virtual TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return null; }
        }
        #endregion TradeCommonInputGUI
        #region TradeCommonInput
        protected virtual TradeCommonInput TradeCommonInput
        {
            get { return null; }
        }
        #endregion TradeCommonInput
        #region isWithNotification
        protected virtual bool IsWithNotification
        {
            get
            {
                return true;
            }
        }
        #endregion
        #region Identifier
        /// <summary>
        /// Obtient true si le l'identifier du trade doit être verrouillé.
        /// NB: Indique entre autre qu'il n'est pas saisissable, et donc calculé par le système.
        /// </summary>
        protected virtual bool IsIdentifierReadOnly
        {
            get { return !TradeCommonInput.TradeStatus.IsStEnvironment_Template; }
        }
        /// <summary>
        /// Obtient true si l'initialiation (reset) du banner doit positionner l'identifier à "Empty"
        /// Exemple: C'est le cas en création de trade
        /// </summary>
        protected bool IsIdentifierReset
        {
            get
            {
                bool ret = false;
                //
                if (!TradeCommonInput.TradeStatus.IsStEnvironment_Template)
                    ret = Cst.Capture.IsModeNewCapture(TradeCommonInputGUI.CaptureMode);
                return ret;
            }
        }
        #endregion

        #region IsTradeFound
        protected bool IsTradeFound
        {
            get
            {
                return TradeCommonInput.IsTradeFound;
            }
        }
        #endregion
        #region IsTradeLink
        protected bool IsTradeLink
        {
            get
            {
                return (null != TradeCommonInput.SQLTradeLink) && TradeCommonInput.SQLTradeLink.IsFound;
            }
        }
        #endregion
        #region IsTradeRemoved
        protected bool IsTradeRemoved
        {
            get
            {
                return TradeCommonInput.IsTradeRemoved;
            }
        }
        #endregion
        #endregion Accessors

        #region Constructors
        // EG 20180514 [23812] Report
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public TradeCommonHeaderBanner(PageBase pPage, string pGUID, Control pControlContainer, bool pIsStatusButtonVisible, string pMainMenuClassName)
            : base(pPage, pGUID, pControlContainer,  pIsStatusButtonVisible, pMainMenuClassName)
        {
            #region Trade Common Controls Banner
            ctrlLblActionTradelink = Cst.LBL + "ActionTradeLink";
            ctrlDspIdentifierTradelink = Cst.DSP + "IdentifierTradeLink";
            ctrlLblIdSysTradelink = Cst.LBL + "IdSysTradeLink";
            ctrlDspIdSysTradelink = Cst.DSP + "IdSysTradeLink";
            ctrlLblDtTradelink = Cst.LBL + "DtTradeLink";
            ctrlDspDtTradelink = Cst.DSP + "DtTradeLink";
            ctrlTblTradelink = "tblTradeLink";
            ctrlToggleTradelink = "divtradelink";
            //
            m_IsStatusButtonVisible = pIsStatusButtonVisible;
            NbDisplayProcess = 11;
            #endregion Trade Common Controls Banner
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        ///  Création des contôles web 
        /// </summary>
        public override void AddControls()
        {
            AddStatusPanel(Mode.Trade);
            AddDateSystemOperatorPanel();
            AddProcessPanel();
            AddTradeLinkPanel();
            AddDisplaynameDescriptionPanel();
        }


        /// <summary>
        ///  Création des contôles web spécifiques au panel LINK
        /// </summary>
        protected void AddTradeLinkPanel()
        {
            WCTogglePanel pnlTradeLink = new WCTogglePanel(Color.Transparent, Ressource.GetString("LinkInformation"), "size4", true)
            {
                ID = ctrlToggleTradelink,
                CssClass = m_Page.CSSMode + " blue"
            };

            Table tableTradeLink = new Table
            {
                ID = ctrlTblTradelink,
                CellPadding = 0,
                CellSpacing = 1,
                Width = Unit.Percentage(98)
            };

            Panel pnl = new Panel
            {
                CssClass = "tradelink"
            };
            pnl.Controls.Add(tableTradeLink);
            pnlTradeLink.AddContent(pnl);

            m_HeaderPanelContainer.Controls.Add(pnlTradeLink);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsRefresh"></param>
        private void DisplayDateSys(bool pIsRefresh)
        {
            if (m_Page.FindControl(ctrlLblDtSys) is Label lbl)
            {
                if (IsTradeLink || (false == IsTradeRemoved))
                    ControlsTools.RemoveStyleDisplay(lbl);
                else
                    lbl.Style[HtmlTextWriterStyle.Display] = "none";

                if (pIsRefresh)
                {
                    // 20081222 RD 16099
                    if (Cst.Capture.IsModeNewCapture(TradeCommonInputGUI.CaptureMode))
                        lbl.Text = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.New);
                    else
                    {
                        if (IsTradeFound)
                            lbl.Text = GetLastTradeLogAction();
                    }
                }
            }
            /// Charge la date de saisie ou dernière action d'un trade
            if (m_Page.FindControl(ctrlDspDtSys) is Label dsp)
            {
                Control ctrlParent = dsp.Parent as Control;
                if (IsTradeLink || (false == IsTradeRemoved))
                    ControlsTools.RemoveStyleDisplay(dsp);
                else
                    dsp.Style[HtmlTextWriterStyle.Display] = "none";
                if (pIsRefresh)
                {
                    string dtDisplay = string.Empty;
                    if (Cst.Capture.IsModeNewCapture(TradeCommonInputGUI.CaptureMode))
                    {
                        dtDisplay = new DtFunc().GetDateString(DtFunc.TODAY);
                    }
                    else
                    {
                        if (IsTradeFound)
                            dtDisplay = GetLastTradeLogDtSys(ctrlParent, dsp);
                    }
                    dsp.Text = dtDisplay;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        /// <param name="pIsLoadFromDb"></param>
        /// EG 20160119 Refactoring Header
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public override void DisplayHeader(bool pIsReadOnly, bool pIsRefresh)
        {
            //20090731 FI ADD pIsRefresh (ce paramètre est un true en mode LoadTrade)  
            //Si l'utilisateur click sur POST par exemple pIsRefresh est à false (il n'est pas nécessaire de rejouer les requêtes)
            pIsRefresh = true; // Bizarrement les contrôle perdent leur données (pb de viewState??? (voir avec PL)  pIsRefresh = true systématiquement

            if (m_Page.FindControl("divdescandstatus") is Panel ctrl)
            {
                if (IsTradeFound)
                    ControlsTools.RemoveStyleDisplay(ctrl);
                else
                    ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            DisplayOperator(pIsRefresh);
            DisplayProcessState(pIsRefresh);
            DisplayDateSys(pIsRefresh);
            DisplayIdSys();
            DisplayTradeLink(pIsRefresh);

            if (IsTradeFound)
                DisplayStatus();

            DisplayIdentifierDisplayNameDescription(pIsReadOnly);
        }

        /// <summary>
        /// 
        /// </summary>
        private void DisplayIdSys()
        {
            // EG 20190328 [MIGRATION VCL] Visibilité IdSystem sur Trade annulé
            // FI 20190405 [XXXXX] visibilité dès que IsTradeFound
            bool isDisplay = Cst.Capture.IsModeConsult(TradeCommonInputGUI.CaptureMode) && IsTradeFound ;
            if (m_Page.FindControl(ctrlLblIdSys) is Label lbl)
            {
                if (isDisplay)
                    ControlsTools.RemoveStyleDisplay(lbl);
                else
                    lbl.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if (m_Page.FindControl(ctrlDspIdSys) is Label dsp)
            {
                if (isDisplay)
                    ControlsTools.RemoveStyleDisplay(dsp);
                else
                    dsp.Style[HtmlTextWriterStyle.Display] = "none";
                dsp.Text = TradeCommonInput.IdT.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsRefresh"></param>
        private void DisplayOperator(bool pIsRefresh)
        {
            /// Charge l'opérateur de saisie d'un trade
            if (m_Page.FindControl(ctrlLblOperator) is Label lbl)
            {
                if (IsTradeLink || (false == IsTradeRemoved))
                    ControlsTools.RemoveStyleDisplay(lbl);
                else
                    lbl.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if (m_Page.FindControl(ctrlDspOperator) is Label dsp)
            {
                if (IsTradeLink || (false == IsTradeRemoved))
                    ControlsTools.RemoveStyleDisplay(dsp);
                else
                    dsp.Style[HtmlTextWriterStyle.Display] = "none";

                if (pIsRefresh)
                {
                    string operator_displayname = string.Empty;
                    if (Cst.Capture.IsModeNewCapture(TradeCommonInputGUI.CaptureMode))
                        operator_displayname = SessionTools.Collaborator_DISPLAYNAME;
                    else if (IsTradeFound)
                        operator_displayname = GetLastTradeLogOperator();
                    dsp.Text = operator_displayname;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsRefresh"></param>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements 
        /// EG 20150618 Refactoring Add DTBUSINESS parameter for GetSelectState_PosKeeping
        /// FI 20161124 [22634] Modify
        private void DisplayProcessState(bool pIsRefresh)
        {
            if (m_Page.FindControl(ctrlPnlProcess) is Panel pnl)
            {
                if ((false == Cst.Capture.IsModeNewOrDuplicateOrReflect(TradeCommonInputGUI.CaptureMode)) && (IsTradeLink || (false == IsTradeRemoved)))
                    ControlsTools.RemoveStyleDisplay(pnl);
                else
                    pnl.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if (m_Page.FindControl(ctrlLblProcess) is Label lbl)
            {
                if ((false == Cst.Capture.IsModeNewOrDuplicate(TradeCommonInputGUI.CaptureMode)) && (IsTradeLink || (false == IsTradeRemoved)))
                    ControlsTools.RemoveStyleDisplay(lbl);
                else
                    lbl.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if (!Cst.Capture.IsModeNewOrDuplicate(TradeCommonInputGUI.CaptureMode) && pIsRefresh)
            {
                if (IsTradeFound)
                {
                    //PL 20110915 New rules for "Ofs"
                    // EG 20120619 Démutualisation de l'affichage des statuts de traitements
                    string sqlSelect = GetSelectState();
                    string fromDual = DataHelper.SQLFromDual(SessionTools.CS);
                    if (StrFunc.IsFilled(fromDual))
                        sqlSelect = sqlSelect.Replace(") tbl", fromDual + ") tbl");
                    
                    DataParameters parameters = new DataParameters();
                    parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), TradeCommonInput.IdT);

                    DataParameter parameterDtBusiness = GetDtBusinessState(SessionTools.CS);
                    if (null != parameterDtBusiness)
                        parameters.Add(parameterDtBusiness);

                    QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect, parameters);
                    // FI 20161124 [22634] call SetDspProcess
                    SetDspProcess(qryParameters);

                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements
        // EG 20180514 [23812] Report 
        protected virtual string GetSelectState()
        {
            string sqlSelect = string.Empty;
            // ProcessState : EVENT
            sqlSelect += GetSelectState_Event(1, "Event");
            // ProcessState : CONF
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(2, Cst.ProcessTypeEnum.CMGEN, "Conf.");
            // ProcessState : SIGEN
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(3, Cst.ProcessTypeEnum.MSOGEN, "Settlt.");
            // ProcessState : EAR
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(4, Cst.ProcessTypeEnum.EARGEN, "EAR");
            // ProcessState : ACCT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(5, Cst.ProcessTypeEnum.ACCOUNTGEN, "Acct.");
            // ProcessState : EXERCISE
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_AssignmentExercise(6, EventCodeFunc.Exercise, "Exe.");
            // ProcessState : ASSIGNEMENT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_AssignmentExercise(7, EventCodeFunc.Assignment, "Ass.");
            // ProcessState : ABANDON
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Abandon(8, "Abn.");
            // ProcessState : OUT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Out(9, "Out");
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            // ProcessState : PROVISIONS (EXC|EXM|EXO|EXS|EXX)
            sqlSelect += GetSelectState_Provisions(10, "Prov.");
            // ProcessState : POSITION / TRANSFERT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_PosKeeping(11);

            sqlSelect += SQLCst.ORDERBY + "SORT1, SORT2" + Cst.CrLf;
            return sqlSelect;
        }

        /// <summary>
        /// Query ProcessState for Event
        /// </summary>
        /// <returns></returns>
        // EG 20120619 Démutualisation de l'affichage des statuts de traitements
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual string GetSelectState_Event(int pOrderState, string pLabel)
        {
            string sqlSelect = SQLCst.SELECT_DISTINCT + pOrderState.ToString() + " as SORT1, SORT2," + DataHelper.SQLString(pLabel) + " as CODE, COLOR";
            sqlSelect += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "1 as SORT2, 'green' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.Trade) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "99 as SORT2, 'black' as COLOR" + Cst.CrLf;
            sqlSelect += ") tbl" + Cst.CrLf;
            return sqlSelect;
        }
        /// <summary>
        /// Query ProcessState for Assignment / Exercise
        /// </summary>
        /// <returns></returns>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements
        // EG 20170412 [23081] Ne plus voir les dénouements du jour désactivés
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        // EG 20231006 [XXXXX] Trade input - Statut : Corrections ordre de tri sur statuts des traitements d'un trade (DénouementTOTAL prioritaire à PARTIEL)        
        protected virtual string GetSelectState_AssignmentExercise(int pOrderState, string pEventCode, string pLabel)
        {
            string eventCodeFilter = string.Empty;
            if (EventCodeFunc.IsExercise(pEventCode))
                eventCodeFilter += DataHelper.SQLString(EventCodeFunc.AutomaticExercise) + "," + DataHelper.SQLString(EventCodeFunc.Exercise);
            else
                eventCodeFilter += DataHelper.SQLString(EventCodeFunc.AutomaticAssignment) + "," + DataHelper.SQLString(EventCodeFunc.Assignment);

            string sqlSelect = SQLCst.SELECT_DISTINCT + pOrderState.ToString() + " as SORT1, SORT2," + DataHelper.SQLString(pLabel) + " as CODE, COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "1 as SORT2, 'green' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTTYPE = " + DataHelper.SQLString(EventTypeFunc.Total) + ")" + SQLCst.AND;
            sqlSelect += "(e.EVENTCODE in (" + eventCodeFilter + "))" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(e.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "2 as SORT2, 'blue' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTTYPE not in ('TOT','CSH','PHY'))" + SQLCst.AND + "(e.EVENTCODE in (" + eventCodeFilter + "))";
            sqlSelect += SQLCst.AND + "(e.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "98 as SORT2, 'gray' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i" + SQLCst.ON + "(i.IDI = t.IDI)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p" + SQLCst.ON + "(p.IDP = i.IDP)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(case p.GPRODUCT when " + DataHelper.SQLString(Cst.ProductGProduct_FX) + " then p.IDENTIFIER else i.IDENTIFIER end not like '%Option')" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(t.IDT = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "98 as SORT2, 'gray' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTCODE in (";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.AutomaticAbandon) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.Abandon) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.Out) + "))" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(e.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "99 as SORT2, 'black' as COLOR" + Cst.CrLf;
            sqlSelect += ") tbl" + Cst.CrLf;
            return sqlSelect;
        }
        /// <summary>
        /// Query ProcessState for Abandon
        /// </summary>
        /// <returns></returns>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements
        // EG 20170412 [23081] Ne plus voir les abandons du jour désactivés
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual string GetSelectState_Abandon(int pOrderState, string pLabel)
        {
            string sqlSelect = SQLCst.SELECT_DISTINCT + pOrderState.ToString() + " as SORT1, SORT2," + DataHelper.SQLString(pLabel) + " as CODE, COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "1 as SORT2, 'red' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTCODE in (";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.Abandon) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.AutomaticAbandon) + "))";
            sqlSelect += SQLCst.AND + "(e.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "98 as SORT2, 'gray' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i" + SQLCst.ON + "(i.IDI = t.IDI)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p" + SQLCst.ON + "(p.IDP = i.IDP)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(case p.GPRODUCT when " + DataHelper.SQLString(Cst.ProductGProduct_FX) + " then p.IDENTIFIER else i.IDENTIFIER end not like '%Option')" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(t.IDT = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "98 as SORT2, 'gray' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTTYPE=" + DataHelper.SQLString(EventTypeFunc.Total) + ")";
            sqlSelect += SQLCst.AND + "(e.EVENTCODE in (";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.AutomaticAssignment) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.AutomaticExercise) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.Assignment) + ",";
            sqlSelect += DataHelper.SQLString(EventCodeFunc.Exercise) + "))" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(e.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "99 as SORT2, 'black' as COLOR" + Cst.CrLf;
            sqlSelect += ") tbl" + Cst.CrLf;
            return sqlSelect;
        }
        /// <summary>
        /// Query ProcessState for Out
        /// </summary>
        /// <returns></returns>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual string GetSelectState_Out(int pOrderState, string pLabel)
        {
            string sqlSelect = SQLCst.SELECT_DISTINCT + pOrderState.ToString() + " as SORT1, SORT2," + DataHelper.SQLString(pLabel) + " as CODE, COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "1 as SORT2, 'red' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + SQLCst.AND + "(e.EVENTCODE =" + DataHelper.SQLString(EventCodeFunc.Out) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "98 as SORT2, 'gray' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i" + SQLCst.ON + "(i.IDI = t.IDI)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p" + SQLCst.ON + "(p.IDP = i.IDP)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(p.IDENTIFIER not like '%Option')" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(t.IDT = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "98 as SORT2, 'gray' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " e" + SQLCst.ON + "(e.IDT = t.IDT)";
            sqlSelect += SQLCst.AND + "(e.EVENTCODE = " + DataHelper.SQLString(EventCodeFunc.Barrier) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(e.IDT = @IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "99 as SORT2, 'black' as COLOR" + Cst.CrLf;
            sqlSelect += ") tbl" + Cst.CrLf;
            return sqlSelect;
        }
        /// <summary>
        /// Query ProcessState for PosKeeping
        /// </summary>
        /// <returns></returns>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements
        /// EG 20150618 Refactoring Use PosKeepingTools.GetQueryPositionActionBySide
        /// EG 20151102 [20979] Refactoring GetQueryPositionActionBySide
        // EG 20170412 [23081] Appel à GetQryPosAction_BySide
        // EG 20200323 [25077] RDBMS Suppression jointure TRADEINSTRUMENT
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240520 [WI930] Upd Usage de la table TRADE à la place de VW_TRADE_FUNGIBLE
        protected virtual string GetSelectState_PosKeeping(int pOrderState)
        {
            string sqlSelect = $@"select distinct {pOrderState} as SORT1, SORT2,
            case SORT2 when 1 then 'Trans.' when 3 then 'Trans.' else 'Pos.' end as CODE, COLOR
            from 
            (
              select case tr.QTY - isnull(pot.QTY,0) when 0 then 1 else 3 end  as SORT2,
                     case tr.QTY - isnull(pot.QTY,0) when 0 then 'red' else 'orange' end as COLOR
              from dbo.TRADE tr
              inner join
              (
	            select sum(pad.QTY) as QTY, pad.IDT_CLOSING as IDT
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR) and (pr.REQUESTTYPE = 'POT')
                where (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_CLOSING
              ) pot on (pot.IDT = @IDT)
              where (tr.IDT = @IDT) and (tr.IDSTBUSINESS = 'ALLOC') and (tr.IDSTACTIVATION = 'REGULAR')

              union all

              select case isnull(pab.QTY,0) + isnull(pas.QTY, 0) when 0 then 5 
		             else case tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0) when 0 then 2 else 4 end end as SORT2,
		             case isnull(pab.QTY,0) + isnull(pas.QTY, 0) when 0 then 'green'
		             else case tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0) when 0 then 'red' else 'orange' end end as COLOR
               from dbo.TRADE tr
              left outer join ({PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER)}) pab on (pab.IDT = tr.IDT)
              left outer join ({PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER)}) pas on (pas.IDT = tr.IDT)
              where (tr.IDT = @IDT) and (tr.IDSTBUSINESS = 'ALLOC') and (tr.IDSTACTIVATION = 'REGULAR')
              
              union all

              select 5 as SORT2, case when b.ISPOSKEEPING = 0 then 'gray' else 'green' end as COLOR
              from dbo.TRADE tr
              inner join dbo.BOOK b on (b.IDB = tr.IDB_DEALER)
              where (tr.IDT = @IDT)

              union all  

              select 98 as SORT2, 'gray' as COLOR
            ) tbl" + Cst.CrLf;
            return sqlSelect;
        }

        // EG 20180514 [23812] Report
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual string GetSelectState_Provisions(int pOrderState, string pLabel)
        {
            string eventCodeProvision =
                DataHelper.SQLString(EventCodeFunc.ExerciseCancelable) + "," +
                DataHelper.SQLString(EventCodeFunc.ExerciseExtendible) + "," +
                DataHelper.SQLString(EventCodeFunc.ExerciseMandatoryEarlyTermination) + "," +
                DataHelper.SQLString(EventCodeFunc.ExerciseOptionalEarlyTermination) + "," +
                DataHelper.SQLString(EventCodeFunc.ExerciseStepUp);

            string sqlSelect = @"select distinct {0} as SORT1, SORT2, '{1}' as CODE, COLOR
            from (
                select 1 as SORT2, 'orange' as COLOR
                from dbo.EVENT ev where (ev.IDT = @IDT) and (ev.EVENTCODE in ({2}))
                union all
                select 98 as SORT2, 'black' as COLOR
                from dbo.EVENT ev where (ev.IDT = @IDT) and (ev.EVENTCODE = '{3}')
                union all
                select 99 as SORT2, 'gray' as COLOR
            ) tbl";

            return String.Format(sqlSelect, pOrderState.ToString(), pLabel, eventCodeProvision, EventCodeFunc.Provision);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// EG 20150619 New
        protected virtual DataParameter GetDtBusinessState(string pCS)
        {
            DateTime dtBusiness = TradeCommonInput.CurrentBusinessDate;
            if (DtFuncML.IsDateTimeEmpty(dtBusiness))
                dtBusiness = DateTime.MaxValue;
            DataParameter parameter = new DataParameter(pCS, "DTBUSINESS", DbType.Date)
            {
                Value = dtBusiness
            };
            return parameter;
        }
        /// <summary>
        /// Query ProcessState for Process (EARGEN, SIGEN, CMGEN, ACCOUNTGEN)
        /// </summary>
        /// <returns></returns>
        /// <param name="pOrderState"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pLabel"></param>
        /// <returns></returns>
        /// EG 20120619 Démutualisation de l'affichage des statuts de traitements
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected string GetSelectState_Process(int pOrderState, Cst.ProcessTypeEnum pProcessType, string pLabel)
        {
            string sqlSelect = SQLCst.SELECT_DISTINCT + pOrderState.ToString() + " as SORT1, SORT2, " + DataHelper.SQLString(pLabel) + " as CODE, COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "1 as SORT2, 'red' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTPROCESS + " ep" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " e" + SQLCst.ON + "(e.IDE = ep.IDE)" + SQLCst.AND + "(e.IDT=@IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(ep.PROCESS=" + DataHelper.SQLString(pProcessType.ToString()) + ")";
            sqlSelect += SQLCst.AND + "(ep.IDSTPROCESS!='SUCCESS')" + SQLCst.AND + "(ep.DTSTPROCESS=";
            sqlSelect += "(" + SQLCst.SELECT + SQLCst.MAX + "(ep2.DTSTPROCESS)" + SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTPROCESS + " ep2" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(ep2.IDE = ep.IDE)))";
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "2 as SORT2, 'green' as COLOR" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTPROCESS + " ep" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " e" + SQLCst.ON + "(e.IDE = ep.IDE)" + SQLCst.AND + "(e.IDT=@IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(ep.PROCESS=" + DataHelper.SQLString(pProcessType.ToString()) + ")";
            sqlSelect += SQLCst.AND + "(ep.IDSTPROCESS='SUCCESS')" + Cst.CrLf;
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "99 as SORT2, 'black' as COLOR" + Cst.CrLf;
            sqlSelect += ") tbl" + Cst.CrLf;
            return sqlSelect;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        /// FI 20140708 [XXXXX] Modification
        /// EG 20160119 Refactoring Header Status
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        private void DisplayStatus()
        {
            // FI 20200820 [25468] dates systemes en UTC, Tooltip affichés selon le profil avec précison à la seconde
            AuditTimestampInfo auditTimestampInfo = new AuditTimestampInfo()
            {
                Collaborator = SessionTools.Collaborator,
                TimestampZone = SessionTools.AuditTimestampZone,
                Precision = Cst.AuditTimestampPrecision.Second
            };

            TradeCommonInput.TradeStatus.stEnvironment.SetTooltip(SessionTools.CS, auditTimestampInfo);
            TradeCommonInput.TradeStatus.stBusiness.SetTooltip(SessionTools.CS, auditTimestampInfo);
            TradeCommonInput.TradeStatus.stActivation.SetTooltip(SessionTools.CS, auditTimestampInfo);
            TradeCommonInput.TradeStatus.stPriority.SetTooltip(SessionTools.CS, auditTimestampInfo);
            TradeCommonInput.TradeStatus.stUsedBy.SetTooltip(SessionTools.CS, auditTimestampInfo);

            if (m_Page.FindControl("lblStEnvironment") is WCTooltipLabel stenv)
                SetToHtmlControl(TradeCommonInput.TradeStatus.stEnvironment, stenv);

            if (m_Page.FindControl("lblStBusiness") is WCTooltipLabel stbus)
                SetToHtmlControl(TradeCommonInput.TradeStatus.stBusiness, stbus);

            if (m_Page.FindControl("lblStActivation") is WCTooltipLabel stact)
                SetToHtmlControl(TradeCommonInput.TradeStatus.stActivation, stact);

            if (m_Page.FindControl("lblStPriority") is WCTooltipLabel stprio)
                SetToHtmlControl(TradeCommonInput.TradeStatus.stPriority, stprio);

            if (m_Page.FindControl("lblStUsedBy") is WCTooltipLabel stused)
                SetToHtmlControl(TradeCommonInput.TradeStatus.stUsedBy, stused);

            foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                    .Where(x => StatusTools.IsStatusUser(x)))
            {
                if (m_Page.FindControl("lbl" + statusEnum.ToString()) is WCTooltipLabel lbl)
                    SetToHtmlControl(SessionTools.CS, TradeCommonInput.TradeStatus.GetStUsersCollection(statusEnum).Status, lbl);
            }

            #region imgStatus
            //20090923 FI imgStatus est de type ImageButton (il rentre dans le circuit tabulaire et est accessible via accessKey)
            LinkButton imgStatus = (LinkButton)m_Page.FindControl("imgStatus");
            if (null != imgStatus)
            {
                imgStatus.CssClass = "fa-icon input";
                imgStatus.Text = @"<i class='fas fa-ellipsis-h'></i>";

                string url = StrFunc.AppendFormat("Status.aspx?GUID={0}&F=CustomCapture&Mode={1}", m_GUID, Mode.Trade.ToString());  
                url = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_Status);
                //FI 20110415 [17402] Spheres effectue désormais une publication de la page pour mettre à jour le dataDocument avec le contenu des contrôles qui ne sont pas autoPostack
                //Ceci est fait uniquement en mode saisie
                if (Cst.Capture.IsModeConsult(TradeCommonInputGUI.CaptureMode))
                {
                    url += "return false;";
                }
                else
                {
                    //FI 20120124 publication de la page sous forme d'un DoPostBack
                    // EG 20160404 Migration vs2013
                    //url += this.m_Page.GetPostBackEventReference(imgStatus) + ";return false;";
                    url += m_Page.ClientScript.GetPostBackEventReference(imgStatus, null) + ";return false;";
                }
                imgStatus.OnClientClick = url;
            }
            #endregion imgStatus
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsRefresh"></param>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20200921 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected virtual void DisplayTradeLink(bool pIsRefresh)
        {
            // Charge les éventuels liens entre négociation

            Table tblTradeLink = m_Page.FindControl(ctrlTblTradelink) as Table;
            bool isDisplay = Cst.Capture.IsModeConsult(TradeCommonInputGUI.CaptureMode) && (IsTradeLink || IsTradeRemoved);
            if (IsTradeRemoved)
            {
                #region IsTradeRemoved

                TableRow tr = new TableRow
                {
                    CssClass = CstCSS.LblError
                };
                TableCell td = new TableCell();
                Panel pnl = new Panel
                {
                    CssClass = "fa-icon fas fa-circle red"
                };
                td.Controls.Add(pnl);
                tr.Cells.Add(td);

                td = new TableCell
                {
                    HorizontalAlign = HorizontalAlign.Left,
                    Wrap = false,
                    //Text = DtFunc.DateTimeToString(dtDisplay, DtFunc.FmtShortDate) + Cst.HTMLSpace + DtFunc.DateTimeToString(dtDisplay, DtFunc.FmtShortTime) + Cst.HTMLSpace
                    // FI 20200820 [25468] // FI 20200820 [25468] dates systemes en UTC, Affichage selon le profil avec précison à la minute
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
                        pnlTradeLink.ResetColorHeader("red");
                    ControlsTools.RemoveStyleDisplay(pnlTradeLink);
                }
                else
                    pnlTradeLink.Style[HtmlTextWriterStyle.Display] = "none!important";
            }
        }
        protected virtual bool ConstructTradeLinkInfo(Table pTblTradeLink)
        {
            return true;
        }

        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Désactivation du tabIndex
        protected virtual void FillTradeLinkInfoRow(Table pTblTradeLink, DateTime pDtsys,
            string pCssClass, string pLed, string pAction, string pUrl, string pIdentifier, int pIdT)
        {
            TableRow tr = new TableRow
            {
                CssClass = pCssClass
            };
            TableCell td = new TableCell();
            Panel pnl = new Panel
            {
                CssClass = String.Format("fa-icon fa fa-circle {0}", pLed)
            };
            td.Controls.Add(pnl);
            tr.Cells.Add(td);

            td = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false,
                //Text = DtFunc.DateTimeToString(pDtsys, DtFunc.FmtShortDate) + Cst.HTMLSpace + DtFunc.DateTimeToString(pDtsys, DtFunc.FmtShortTime) + Cst.HTMLSpace
                Text = DtFuncExtended.DisplayTimestampUTC(pDtsys, new AuditTimestampInfo()
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
            td.Text = pAction + Cst.HTMLSpace;
            tr.Cells.Add(td);
            //
            td = new TableCell
            {
                Wrap = false,
                HorizontalAlign = HorizontalAlign.Left
            };
            if (StrFunc.IsFilled(pUrl))
            {
                HyperLink lnk = new HyperLink
                {
                    TabIndex = -1,
                    NavigateUrl = pUrl,
                    Text = pIdentifier
                };
                td.Controls.Add(lnk);
            }
            else
            {
                td.Text = pIdentifier;
            }
            tr.Cells.Add(td);
            //
            td = new TableCell
            {
                Wrap = false,
                HorizontalAlign = HorizontalAlign.Right,
                Text = Cst.HTMLSpace + "(Id: " + pIdT.ToString() + ")"
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
            pTblTradeLink.Rows.Add(tr);
        }

        // EG 201200621 Unused for the moment
        protected virtual void FillTradeLinkInfoDefil(Panel pPnlTradeLink, DateTime pDtsys,
            string pCssClass, string pLed, string pAction, string pUrl, string pIdentifier, int pIdT)
        {
            //
            if (0 == pPnlTradeLink.Controls.Count)
            {
                HtmlGenericControl ul = new HtmlGenericControl("ul")
                {
                    ID = "ulTradeLinkInfo"
                };
                pPnlTradeLink.Controls.Add(ul);
            }
            HtmlGenericControl li = new HtmlGenericControl("li");
            Label lbl = new Label
            {
                Text = DtFuncExtended.DisplayTimestampUTC(pDtsys, new AuditTimestampInfo()
                {
                    Collaborator = SessionTools.Collaborator,
                    TimestampZone = SessionTools.AuditTimestampZone,
                    Precision = Cst.AuditTimestampPrecision.Minute
                })
            };

            li.Controls.Add(lbl);
            HtmlAnchor anchor = new HtmlAnchor
            {
                InnerText = pIdentifier,
                HRef = pUrl,
                Target = "blank"
            };
            li.Controls.Add(anchor);
            lbl = new Label
            {
                Text = pAction
            };
            anchor.Controls.Add(lbl);
            lbl = new Label
            {
                Text = "(Id: " + pIdT.ToString() + ")"
            };
            anchor.Controls.Add(lbl);

            pPnlTradeLink.Controls[0].Controls.Add(li);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string GetLastTradeLogAction()
        {
            string action;
            if (StrFunc.IsFilled(TradeCommonInput.SQLLastTradeLog.Action))
            {
                PermissionEnum perm = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), TradeCommonInput.SQLLastTradeLog.Action, true);
                action = PermissionTools.GetRessource(perm, false);
            }
            else
                action = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.Consult);
            return action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCellParent"></param>
        /// <param name="pLbl"></param>
        /// <param name="pIsTradeFound"></param>
        /// <param name="pIsTradeLink"></param>
        /// <param name="pIsTradeRemoved"></param>
        /// <returns></returns>
        /// EG 20160119 Control pControlParent replace TableCell pCellParent
        protected string GetLastTradeLogDtSys(Control pControlParent, Label pLbl)
        {
            DateTime dtSysUTC = TradeCommonInput.SQLLastTradeLog.DtSys;
            
            DateTime dtTransac = TradeCommonInput.CurrentTrade.TradeHeader.TradeDate.DateValue;
            DateTime dtSysTzEntity = DtFuncExtended.ConvertTimeToTz(new DateTimeTz(TradeCommonInput.SQLLastTradeLog.DtSys, "Etc/UTC"), 
                TradeCommonInput.DataDocument.GetTimeZoneEntity(CSTools.SetCacheOn(SessionTools.CS), "Etc/UTC")).Date.Date;
            
            bool isDesLe = dtSysTzEntity.CompareTo(dtTransac) > 0;
            pLbl.ToolTip = string.Empty;
            if (isDesLe)
            {
                if (!(m_Page.FindControl(ctrlDspDtSys + "DesLe") is WCTooltipLabel lblDesLe))
                {
                    lblDesLe = new WCTooltipLabel
                    {
                        ID = ctrlDspDtSys + "DesLe",
                        Text = "!",
                        ForeColor = Color.Red
                    };
                    lblDesLe.Font.Bold = true;
                    lblDesLe.Pty.TooltipContent = Ressource.GetString("InfoDesLe");
                    lblDesLe.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                    pControlParent.Controls.Add(lblDesLe);
                }
                if (IsTradeLink || (false == IsTradeRemoved))
                    ControlsTools.RemoveStyleDisplay(lblDesLe);
                else
                    lblDesLe.Style[HtmlTextWriterStyle.Display] = "none";
            }
            string ret = DtFuncExtended.DisplayTimestampUTC(dtSysUTC, new AuditTimestampInfo
            {
                Collaborator = SessionTools.Collaborator,
                TimestampZone = SessionTools.AuditTimestampZone,
                Precision = Cst.AuditTimestampPrecision.Minute
            });
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string GetLastTradeLogOperator()
        {
            string ret = string.Empty;
            SQL_Actor oper = new SQL_Actor(CSTools.SetCacheOn(SessionTools.CS), TradeCommonInput.SQLLastTradeLog.IdA);
            if (oper.LoadTable(new string[] { "IDA", "DISPLAYNAME" }))
                ret = oper.DisplayName;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        private void DisplayIdentifierDisplayNameDescription(bool pIsReadOnly)
        {

            bool isReadOnlyDefault = Cst.Capture.IsModeConsult(TradeCommonInputGUI.CaptureMode)
                                    || Cst.Capture.IsModeRemoveOnlyAll(TradeCommonInputGUI.CaptureMode)
                                    || Cst.Capture.IsModeCorrection(TradeCommonInputGUI.CaptureMode);
            //
            bool isReadOnly = pIsReadOnly;
            if (!isReadOnly)
                isReadOnly = isReadOnlyDefault;
            //
            TextBox txt;
            if (IsTradeFound)
            {
                #region IsReset_IdentifierDisplaynameDescription=true
                if (m_IsReset_IdentifierDisplaynameDescription)
                {
                    m_IsReset_IdentifierDisplaynameDescription = false;
                    //Identifier
                    txt = m_Page.FindControl(ctrlTxtIdentifier) as TextBox;
                    if (null != txt)
                    {
                        if (IsIdentifierReset)
                            txt.Text = string.Empty;
                        else
                            txt.Text = TradeCommonInput.Identifier;
                        txt.ToolTip = txt.Text;
                        txt.ReadOnly = isReadOnly;
                    }
                    //DisplayName
                    txt = m_Page.FindControl(ctrlTxtDisplayName) as TextBox;
                    if (null != txt)
                    {
                        //20091016 FI [Rebuild identification] use identification
                        txt.Text = TradeCommonInput.Identification.Displayname;
                        txt.ToolTip = txt.Text;
                        txt.ReadOnly = isReadOnly;
                    }
                    //Description
                    txt = m_Page.FindControl(ctrlTxtDescription) as TextBox;
                    if (null != txt)
                    {
                        //20091016 FI [Rebuild identification] use identification
                        txt.Text = TradeCommonInput.Identification.Description;
                        txt.ToolTip = txt.Text;
                        txt.ReadOnly = isReadOnly;
                    }
                    //ExtLink
                    txt = m_Page.FindControl(ctrlTxtExtLink) as WCTextBox2;
                    if (null != txt)
                    {
                        //20091016 FI [Rebuild identification] use identification
                        txt.Text = TradeCommonInput.Identification.Extllink;
                        txt.ToolTip = txt.Text;
                        txt.ReadOnly = isReadOnly;
                        txt.CssClass = EFSCssClass.GetCssClass(false, false, false, isReadOnly);
                    }
                }
                #endregion Trade DisplayName / Description
            }
            //
            #region ReadOnly on Identifier
            if (IsIdentifierReadOnly)
            {
                txt = m_Page.FindControl(ctrlTxtIdentifier) as TextBox;
                if (null != txt)
                    txt.ReadOnly = true;
            }
            #endregion

        }


        /// <summary>
        /// 
        /// </summary>
        public void ResetIdentifierDisplaynameDescriptionExtlLink()
        {
            m_IsReset_IdentifierDisplaynameDescription = true;

            #region Identifier
            if (m_Page.FindControl(ctrlTxtIdentifier) is TextBox identifier)
                identifier.Text = string.Empty;
            #endregion Identifier
            #region DisplayName
            if (m_Page.FindControl(ctrlTxtDisplayName) is TextBox dsn)
            {
                dsn.Text = string.Empty;
                dsn.ToolTip = string.Empty;
            }
            #endregion DisplayName
            #region Description
            if (m_Page.FindControl(ctrlTxtDescription) is TextBox description)
            {
                description.Text = string.Empty;
                description.ToolTip = string.Empty;
            }

            #endregion Description
            #region ExtLink
            if (m_Page.FindControl(ctrlTxtExtLink) is WCTextBox2 extLink)
            {
                extLink.Text = string.Empty;
                extLink.ToolTip = string.Empty;
            }
            #endregion ExtLink

        }

        
        
        #endregion Methods
    }
}
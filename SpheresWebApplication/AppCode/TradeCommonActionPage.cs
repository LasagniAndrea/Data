#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Permission;
using EFS.Process;
using EFS.Referential;
using EFS.TradeInformation;
using EFS.Tuning;
using EfsML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#endregion using directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de TradeCommonActionPage
    /// </summary>
    /// 
    public abstract class TradeCommonActionPage : PageBase
    {
        #region Members
        protected TradeActionMode.TradeActionModeEnum m_TradeActionMode;
        protected TradeActionType.TradeActionTypeEnum m_TradeActionType;
        protected string m_EventArgument;
        protected string m_EventTarget;
        protected int m_CurrentIdE;
        protected string m_XmlRefEvent;
        protected string m_XmlRefEventClass;
        protected string m_XmlRefEventAsset;
        protected string m_XmlRefEventDet;
        protected string m_ParentGUID;
        protected InputGUI m_InputActionGUI;
        #endregion Members
        #region Accessors
        #region CurrentActionEvent
        /// EG 20150513 [20513] TradeCommonActionEvent devient TradeActionEventBase
        public virtual TradeActionEventBase CurrentActionEvent
        {
            get { return null; }
        }
        #endregion CurrentActionEvent
        #region CurrentIdE
        protected string CurrentIdE
        {
            get
            {
                string idE = "0";
                Control ctrl = FindControl("__IDE");
                if (null != ctrl)
                {
                    idE = ((HtmlInputHidden)ctrl).Value;
                    int i = idE.IndexOf("-");
                    if (-1 < i)
                        idE = idE.Substring(i);
                    else
                    {
                        i = idE.IndexOf("_");
                        if (-1 < i)
                            idE = idE.Substring(i + 1);
                    }
                }
                return idE;
            }
            set
            {
                Control ctrl = FindControl("__IDE");
                if (null != ctrl)
                    ((HtmlInputHidden)ctrl).Value = value;
            }
        }
        #endregion CurrentIdE
        #region ActionRootName
        protected virtual string ActionRootName
        {
            get { return null; }
        }
        #endregion ActionRootName
        #region InputActionGUISessionID
        protected string InputActionGUISessionID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("GUI");
            }
        }
        #endregion InputActionGUISessionID
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }
        
        #region IsActionPermitted
        private bool IsActionPermitted()
        {
            return TradeCommonInputGUI.IsCreateAuthorised;
        }
        #endregion IsActionPermitted
        #region IsActionAllowed
        protected bool IsActionAllowed(out string msg)
        {
            int idPermission = m_InputActionGUI.Permission.GetIdPermission(PermissionEnum.Create);
            bool ret = TuningTools.IsActionAllowed(SessionTools.CS, TradeCommonInput.IdT, TradeCommonInput.Product.IdI, idPermission, SessionTools.Collaborator_IDA, SessionTools.ActorAncestor, out msg);
            return ret;
        }
        #endregion IsActionAllowed
        #region IsModeConsult
        // ****** Don't touch this property. Use by Full FpML Interface
        public bool IsModeConsult
        {
            get { return false; }
        }
        #endregion IsModeConsult
        #region IsScreenFullCapture
        // ****** Don't touch this property. Use by Full FpML Interface
        public bool IsScreenFullCapture
        {
            get { return false; }
        }
        #endregion IsScreenFullCapture
        #region IsStEnvironment_Template
        //20081128 FI [ticket 16435] IsStEnvironment_Template doit être une property public
        public bool IsStEnvironment_Template
        {
            get { return TradeCommonInput.TradeStatus.IsStEnvironment_Template; }
        }
        #endregion IsStEnvironment_Template
        #region IsModeUpdatePostEvts
        // ****** Don't touch this property. Use by Full FpML Interface
        public bool IsModeUpdatePostEvts
        {
            get { return false; }
        }
        #endregion IsModeUpdatePostEvts
        // ****** Don't touch this property. Use by Full FpML Interface
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public bool IsModeUpdateFeesUninvoiced
        {
            get { return false; }
        }

        #region IsZoomOnFull
        //FI 20091118 [16744] Add IsZoomOnFull
        /// <summary>
        /// Obtient false pour indiquer que la page en cours n'est pas un Zoom vers la saisie Full
        /// </summary>
        public bool IsZoomOnFull
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region PlaceHolderTradeActionFooter
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected Control PlaceHolderTradeActionFooter
        {
            get { return FindControl("plhTradeActionFooter"); }
        }
        #endregion PlaceHolderTradeActionFooter

        #region PlaceHolderTradeAction
        protected Control PlaceHolderTradeAction
        {
            get { return FindControl("plhTradeAction"); }
        }
        #endregion PlaceHolderTradeAction
        #region PlaceHolderTradeActionEvents
        protected Control PlaceHolderTradeActionEvents
        {
            get { return FindControl("plhTradeActionEvents"); }
        }
        #endregion PlaceHolderTradeActionEvents
        #region TradeActionEventsSessionID
        protected string TradeActionEventsSessionID
        {
            get { return m_ParentGUID + "_InputActionEvents"; }
        }
        #endregion TradeActionEventsSessionID
        #region TradeCommonActionEvents
        /// EG 20150513 [20513] TradeCommonActionEvent devient TradeActionEventBase
        protected virtual TradeActionEventsBase TradeCommonActionEvents
        {
            set { ; }
            get { return null; }
        }
        #endregion TradeCommonActionEvents
        #region TradeCommonInput
        public virtual TradeCommonInput TradeCommonInput
        {
            get { return null; }
        }
        #endregion TradeCommonInput
        #region TradeCommonInputGUI
        public virtual TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return null; }
        }
        #endregion TradeCommonInputGUI
        #endregion Accessors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            m_EventArgument = Page.Request.Params["__EVENTARGUMENT"];
            m_EventTarget = Page.Request.Params["__EVENTTARGET"];
            m_ParentGUID = Request.QueryString["GUID"];
            
            //if (StrFunc.IsEmpty(m_ParentGUID))
            //    throw new ArgumentException("Argument GUID expected");

            m_TradeActionType = TradeActionType.GetTradeActionTypeEnum(ObjFunc.NullToEmpty(Request.QueryString["Action"]));

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(System.EventArgs e)
        {
            // FI 20200518 [XXXXX] Use DataCache
            //Session[InputActionGUISessionID] = m_InputActionGUI;
            DataCache.SetData<InputGUI>(InputActionGUISessionID, m_InputActionGUI);

            if (TradeActionMode.IsEditPlus(m_TradeActionMode))
                JavaScript.ScriptOnStartUp(this, "DisableValidators();", "DisableValidators");

            if (TradeActionMode.IsCalcul(m_TradeActionMode))
                ExecuteAction(m_TradeActionMode, m_CurrentIdE);

            base.OnPreRender(e);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20210222 [XXXXX] Appel self.Close() et suppression inscription function AutoClose
        // EG 20210623 [XXXXX] Ajout instruction JS Block
        protected override void OnLoad(System.EventArgs e)
        {
            bool isOk = true;
            if (false == IsPostBack)
            {
                if (isOk)
                {
                    isOk = IsActionPermitted();
                    if (false == isOk)
                        DisplayErrMsg(Ressource.GetString2("Msg_TradeActionNotAuthorized", TradeCommonInput.Identifier));
                }
                if (isOk)
                {
                    isOk = IsActionAllowed(out string msg);
                    if (false == isOk)
                        DisplayErrMsg(msg);
                }
            }
            //
            if (isOk)
            {
                if (StrFunc.IsFilled(m_EventArgument))
                {
                    m_TradeActionMode = TradeActionMode.SetTradeActionMode(m_EventArgument);
                    if ((TradeActionMode.IsIdSpecified(m_TradeActionMode)) && (StrFunc.IsFilled(m_EventTarget)))
                        CurrentIdE = m_EventTarget;
                }

                m_CurrentIdE = Convert.ToInt32(CurrentIdE);

                if ((false == IsPostBack) || TradeActionMode.IsRefresh(m_TradeActionMode))
                {
                    // FI 20200518 [XXXXX] Mis en commentaire 
                    // Session[TradeActionEventsSessionID] = null;
                    GetSelectEvents();
                }

                SetTradeActionEvents();

                if (TradeActionMode.IsClose(m_TradeActionMode))
                {
                    JavaScript.CallFunction(this, "self.close();");
                }
                else if (TradeActionMode.IsValidate(m_TradeActionMode))
                {
                    #region Final Validation
                    if (false == ExecuteAction(TradeActionMode.TradeActionModeEnum.Save))
                    {
                        m_TradeActionMode = TradeActionMode.SetTradeActionMode(TradeActionMode.TradeActionModeEnum.EditErrorValidationRules.ToString());
                        DisplayEvents();
                    }
                    else
                    {
                        ValidateAction();
                        JavaScript.CallFunction(this, "self.close();");
                    }
                    #endregion Final Validation
                }
                else
                {
                    if (TradeActionMode.IsSave(m_TradeActionMode))
                    {
                        #region Action Validation
                        if (false == ExecuteAction(m_TradeActionMode, m_CurrentIdE))
                            m_TradeActionMode = TradeActionMode.SetTradeActionMode(TradeActionMode.TradeActionModeEnum.EditErrorValidationRules.ToString());
                        #endregion Action Validation
                    }
                    else if (TradeActionMode.IsRelease(m_TradeActionMode))
                    {
                        #region Release last action Validation
                        ReleaseCurrentAction();
                        #endregion Release last action Validation
                    }
                    DisplayEvents();
                }

                ActiveButtons("btnPostToService");
                HideButtons("btnPostToService");
                ActiveButtons("btnRefresh");

                Control btn = FindControl("btnClose2");
                if ((null != btn) && ((null == TradeCommonActionEvents) || (false == TradeCommonActionEvents.IsEditable)))
                {
                    // Close form only without confirm message
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("document.forms[0].__EVENTARGUMENT.value='{0}';", TradeActionMode.TradeActionModeEnum.Close.ToString());
                    sb.Append("return true;");
                    //((Button)btn).Attributes["onclick"] = sb.ToString();
                    ((ImageButton)btn).Attributes["onclick"] = sb.ToString();
                }
            }

            // EG 20210623 New
            JQuery.Block block = new JQuery.Block((PageBase)this.Page)
            {
                Timeout = SystemSettings.GetTimeoutJQueryBlock("TAE")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);

            base.OnLoad(e);

        }

        #endregion Events

        #region ActiveButtons
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void ActiveButtons(string pButtonId)
        {
            if (FindControl(pButtonId) is WebControl btn)
            {
                btn.Enabled = (false == TradeActionMode.IsEditPlus(m_TradeActionMode));
                if (false == btn.Enabled)
                    btn.Attributes.Remove("onclick");
            }
        }
        #endregion ActiveButtons
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20210623 [XXXXX] Désactivation des hRef sur boutons
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + TradeCommonInputGUI.MainMenuClassName };

            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Attributes.Add("onclick", GetScriptConfirmBox(btnRefresh, false, TradeActionMode.TradeActionModeEnum.Refresh));
            btnRefresh.Attributes["href"] = "#";
            pnlToolBar.Controls.Add(btnRefresh);

            WCToolTipLinkButton btnPostToService = ControlsTools.GetAwesomeButtonAction("btnPostToService", "fa fa-caret-square-right", true);
            btnPostToService.Attributes.Add("onclick", GetScriptConfirmBox(btnPostToService, false, TradeActionMode.TradeActionModeEnum.Validate));
            btnPostToService.Attributes["href"] = "#";
            pnlToolBar.Controls.Add(btnPostToService);

            WCToolTipLinkButton btnPrint = ControlsTools.GetAwesomeButtonAction("btnPrint", "fas fa-print", false);
            btnPrint.Attributes.Add("onclick", "CallPrint('divdescandstatus','tblTradeActionEvents');return false;");
            btnPrint.Attributes["href"] = "#";
            pnlToolBar.Controls.Add(btnPrint);

            CellForm.Controls.Add(pnlToolBar);
        }
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divDtg", CssClass = CSSMode + " " + TradeCommonInputGUI.MainMenuClassName };
            Table table = new Table() {
                ID = "tblTradeActionEvents",
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
                CellPadding = 0,
                CellSpacing = 0
            };
            TableRow tr = new TableRow
            {
                ID = "trTradeActionEvents"
            };
            TableCell td = new TableCell
            {
                ID = "tdTradeActionEvents"
            };

            PlaceHolder ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "plhTradeActionEvents"
            };
            td.Controls.Add(ph);
            tr.Cells.Add(td);
            table.Rows.Add(tr);

            pnlBody.Controls.Add(table);
            CellForm.Controls.Add(pnlBody);

            Panel pnlBodyAction = new Panel() { ID = "divDtgbodyaction", CssClass = CSSMode + " " + TradeCommonInputGUI.MainMenuClassName };
            table = new Table() {
                ID = "tblTradeAction",
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
                CellPadding = 0,
                CellSpacing = 0
            };

            tr = new TableRow
            {
                ID = "trTradeAction"
            };
            td = new TableCell
            {
                ID = "tdTradeAction"
            };

            ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "plhTradeAction"
            };
            td.Controls.Add(ph);
            tr.Cells.Add(td);
            table.Rows.Add(tr);

            pnlBodyAction.Controls.Add(table);
            CellForm.Controls.Add(pnlBodyAction);

            ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "plhTradeActionFooter"
            };
            CellForm.Controls.Add(ph);
        }

        #region CreateAndLoadPlaceHolder
        protected void CreateAndLoadPlaceHolder()
        {
            Table table = new Table
            {
                ID = "tblTradeActionEvents",
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
                BorderColor = Color.CornflowerBlue,
                CellPadding = 0,
                CellSpacing = 0
            };

            TableRow tr = new TableRow
            {
                ID = "trTradeActionEvents"
            };
            TableCell td = new TableCell
            {
                ID = "tdTradeActionEvents"
            };

            PlaceHolder ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "plhTradeActionEvents"
            };
            td.Controls.Add(ph);
            tr.Cells.Add(td);
            table.Rows.Add(tr);

            tr = new TableRow
            {
                ID = "trTradeAction"
            };
            td = new TableCell
            {
                ID = "tdTradeAction"
            };

            ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "plhTradeAction"
            };
            td.Controls.Add(ph);
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            CellForm.Controls.Add(table);
        }
        #endregion CreateAndLoadPlaceHolder
        #region CreateChildControls
        // EG 20210222 [XXXXX] Suppression Validators.js (fonctions déplacées dans PageBase.js)
        // EG 20210222 [XXXXX] Suppression JavaScript.ConfirmInputTrade (présent dans Trade.js)
        // EG 20210224 [XXXXX] Minification Trade.js
        protected override void CreateChildControls()
        {
            //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/validators.js"));
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/Trade.min.js"));
            //JavaScript.ConfirmInputTrade(this);
            base.CreateChildControls();
        }
        #endregion CreateChildControls
        #region CreateCurrentAction
        /// EG 20150513 [20513] Upd
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public void CreateCurrentAction()
        {
            Control ctrl = PlaceHolderTradeAction;
            if (null != ctrl)
            {
                if (TradeActionMode.IsEditPlus(m_TradeActionMode))
                {
                    #region Controls created in Edit Mode
                    TradeActionEventBase events = CurrentActionEvent;
                    if (null != events)
                    {
                        ctrl.Controls.Add(new LiteralControl("<br/>"));
                        ctrl.Controls.Add(events.CreateControlCurrentAction);
                        if (TradeActionMode.IsEditErrorValidationRules(m_TradeActionMode))
                            ctrl.Controls.Add(events.DisplayValidationRulesMessage);
                    }
                    #endregion Controls created in Edit Mode
                }
            }

            ctrl = PlaceHolderTradeActionFooter;
            if (null != ctrl)
            {

                #region Buttons action

                if (TradeActionMode.IsEditPlus(m_TradeActionMode))
                {
                    Panel pnlButton = new Panel() { ID="divfooter", Width = Unit.Percentage(50)};
                    pnlButton.Style.Add(HtmlTextWriterStyle.Margin, "0% 25%");

                    WCToolTipLinkButton btnValidate = ControlsTools.GetAwesomeButtonAction("btnValidate", "fas fa-save", true);
                    btnValidate.CausesValidation = true;
                    btnValidate.OnClientClick = GetScriptConfirmBox(btnValidate, true, TradeActionMode.TradeActionModeEnum.Save);
                    btnValidate.Attributes["href"] = "#";
                    pnlButton.Controls.Add(btnValidate);

                    WCToolTipLinkButton btnCancel = ControlsTools.GetAwesomeButtonAction("btnCancel", "fas fa-times", true);
                    btnCancel.CausesValidation = false;
                    btnCancel.Attributes.Add("onclick", GetScriptConfirmBox(btnCancel, false, TradeActionMode.TradeActionModeEnum.Annul));
                    btnCancel.Attributes["href"] = "#";
                    pnlButton.Controls.Add(btnCancel);

                    WCToolTipLinkButton btnReinit = ControlsTools.GetAwesomeButtonAction("btnReinit", "fa fa-redo", true);
                    btnReinit.CausesValidation = false;
                    btnReinit.Attributes.Add("onclick", GetScriptConfirmBox(btnReinit, false, TradeActionMode.TradeActionModeEnum.Release));
                    btnReinit.Attributes["href"] = "#";
                    pnlButton.Controls.Add(btnReinit);

                    ctrl.Controls.Add(pnlButton);
                }
                #endregion Buttons Action
            }
        }
        #endregion CreateCurrentAction
        #region DeserializeEvents
        public virtual Cst.ErrLevel DeserializeEvents(DataSet pDsEvents) { return Cst.ErrLevel.FAILURE; }
        #endregion DeserializeEvents
        #region DisplayErrMsg
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void DisplayErrMsg(string pErrMsg)
        {
            Control ctrl = PlaceHolderTradeActionEvents;
            if (null != ctrl)
            {
                PlaceHolder plh = (PlaceHolder)ctrl;
                Label lbl = new Label
                {
                    Text = pErrMsg,
                    CssClass = EFSCssClass.Msg_Alert
                };
                plh.Controls.AddAt(0, lbl);
                plh.Controls.AddAt(0, new LiteralControl(Cst.HTMLBreakLine));
                HideButtons("btnPostToService");
                HideButtons("btnRefresh");
            }
        }
        #endregion DisplayErrMsg
        #region DisplayEvents
        public virtual void DisplayEvents() { }
        #endregion DisplayEvents
        #region ExecuteAction
        /// EG 20150513 [20513] Upd
        protected bool ExecuteAction(TradeActionMode.TradeActionModeEnum pTradeActionMode)
        {
            bool isOk = true;
            TradeActionEventBase events = TradeCommonActionEvents.GetEventEditable();
            while (isOk && (null != events))
            {
                isOk = ExecuteAction(pTradeActionMode, events.idE);
                if (false == isOk)
                {
                    CurrentIdE = events.idE.ToString();
                    m_CurrentIdE = Convert.ToInt32(CurrentIdE);
                    return isOk;
                }
                if (null != events.Events)
                    events = events.GetEventEditable();
                else
                    return isOk;
            }
            return isOk;
        }
        /// EG 20150513 [20513] Upd
        // EG 20180423 Analyse du code Correction [CA2200]
        protected bool ExecuteAction(TradeActionMode.TradeActionModeEnum pTradeActionMode, int pIdE)
        {
            bool isOk = true;

            TradeActionEventBase events = GetActionEvent(pIdE);
            if ((null != events) && (null != events.m_Action))
            {
                Type tAction = events.m_Action.GetType();
                MethodInfo method = tAction.GetMethod(pTradeActionMode.ToString());
                if (null != method)
                {
                    ParameterInfo[] pi = method.GetParameters();
                    if (0 < pi.Length)
                    {
                        object[] _params = new object[] { this.Page };
                        string[] _names = new String[] { pi[0].Name };

                        if (TradeActionMode.IsCalcul(m_TradeActionMode))
                        {
                            _params = new object[] { this.Page, m_EventTarget };
                            _names = new String[] { pi[0].Name, pi[1].Name };
                        }
                        object result = tAction.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, events.m_Action, _params, null, null, _names);
                        events.isModified = true;
                        if ((null != result) && (result.GetType().Equals(typeof(System.Boolean))))
                            isOk = (bool)result;
                    }
                }
            }
            return isOk;
        }
        #endregion ExecuteAction
        #region GenerateHtmlForm
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmTradeAction";
            HtmlInputHidden hdn = new HtmlInputHidden
            {
                ID = "__IDE",
                Value = "0"
            };
            CellForm.Controls.Add(hdn);
            AddToolBar();
        }
        #endregion GenerateHtmlForm
        #region GetActionEvent
        /// EG 20150513 [20513] Upd
        public virtual TradeActionEventBase GetActionEvent(int pIdE)
        {
            return null;
        }
        #endregion GetActionEvent
        #region GetScriptConfirmBox
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20210623 Appel à nouvelle fonction ConfirmInputTradeWithArgs (Identique à ConfirmInputTrade)
        // mais les arguments associés aux boutons OK/CANCEL de la fenêtre de validation sont passés en paramètre :
        // - OK = pTradeActionMode.ToString()
        // - CANCEL = FALSE
        // ces paramètres sont passés dans le PostBack du control {pControl} et interprété par la méthode OnLoad
        protected string GetScriptConfirmBox(Control pControl, bool pIsCausesValidation, TradeActionMode.TradeActionModeEnum pTradeActionMode)
        {
            StringBuilder sb = new StringBuilder();
            string msgConfirm = Ressource.GetStringForJS("Msg_" + pTradeActionMode.ToString() + "Va");
            if (pIsCausesValidation)
            {
                if (TradeActionMode.IsEditPlus(m_TradeActionMode))
                    sb.Append(@"EnableValidators();");
                sb.Append(@"if (Page_ClientValidate()) {");
                sb.AppendFormat("document.forms[0].__EVENTARGUMENT.value='{0}';", pTradeActionMode.ToString());
                sb.AppendFormat("ConfirmInputTradeWithArgs('{0}','{1}','{2}','{3}','{4}','{5}');", PageTitle, pControl.ID, msgConfirm, TradeCommonInput.Identifier, pTradeActionMode.ToString(), "FALSE");
                sb.Append("} return false;");
            }
            else
            {
                if (TradeActionMode.IsEditPlus(m_TradeActionMode))
                    sb.Append(@"DisableValidators();");
                sb.AppendFormat("document.forms[0].__EVENTARGUMENT.value='{0}';", pTradeActionMode.ToString());
                sb.AppendFormat("ConfirmInputTradeWithArgs('{0}','{1}','{2}','{3}','{4}','{5}');", PageTitle, pControl.ID, msgConfirm, TradeCommonInput.Identifier, pTradeActionMode.ToString(), "FALSE");
                sb.Append(" return false;");
            }
            return sb.ToString();
        }
        #endregion GetScriptConfirmBox
        #region GetSelectEvents
        // EG 20150116 [20700] Change alias e.IDT by ev.IDT
        // EG 20190412 Tri sur IDE de la table EVENT pour Nested correct
        protected Cst.ErrLevel GetSelectEvents()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            //ArrayList aTemp, aTemp2;
            //bool isTemp;
            SQLReferentialData.SQLSelectParameters sqlSelectParameters;
            StrBuilder sqlQuery = new StrBuilder();
            string SQLWhere = @"IDT = @IDT
            order by IDE" + Cst.CrLf;

            #region Event
            if (StrFunc.IsEmpty(m_XmlRefEvent))
                m_XmlRefEvent = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "VW_EVENT");
            ReferentialTools.DeserializeXML_ForModeRO(m_XmlRefEvent, out ReferentialsReferential refEvent);
            //sqlQuery += SQLReferentialData.GetSQLSelect(CS, refEvent, SQLWhere, false, false, out aTemp, out aTemp2).GetQueryReplaceParameters(false);
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEvent, SQLWhere);
            sqlQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false);
            sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            #endregion Event
            #region EventClass
            if (StrFunc.IsEmpty(m_XmlRefEventClass))
                m_XmlRefEventClass = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTCLASS");
            ReferentialTools.DeserializeXML_ForModeRO(m_XmlRefEventClass, out ReferentialsReferential refEventClass);
            if (null != refEventClass)
            {
                // EG 20150116 [20700] Change alias e.IDT by ev.IDT
                sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventClass, "ev.IDT = @IDT");
                sqlQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false);
                sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            }

            #endregion EventClass
            #region EventAsset
            if (StrFunc.IsEmpty(m_XmlRefEventAsset))
                m_XmlRefEventAsset = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTASSET");
            ReferentialTools.DeserializeXML_ForModeRO(m_XmlRefEventAsset, out ReferentialsReferential refEventAsset);
            if (null != refEventAsset)
            {
                // EG 20150116 [20700] Change alias e.IDT by ev.IDT
                sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventAsset, "ev.IDT = @IDT");
                sqlQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false);
                sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            }

            #endregion EventAsset
            #region EventDet
            if (StrFunc.IsEmpty(m_XmlRefEventDet))
                m_XmlRefEventDet = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTDET");
            ReferentialTools.DeserializeXML_ForModeRO(m_XmlRefEventDet, out ReferentialsReferential refEventDet);
            if (null != refEventDet)
            {
                // EG 20150116 [20700] Change alias e.IDT by ev.IDT
                sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventDet, "ev.IDT = @IDT");
                sqlQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false);
                sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            }
            #endregion EventDet

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(SessionTools.CS, "IDT", DbType.Int32), TradeCommonInput.IdT);
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlQuery.ToString(), parameters.GetArrayDbParameter());
            #region Relations
            ds.DataSetName = ActionRootName;
            DataTable dtEvent = ds.Tables[0];
            dtEvent.TableName = "Event";
            DataTable dtEventClass = ds.Tables[1];
            dtEventClass.TableName = "EventClass";
            DataTable dtEventAsset = ds.Tables[2];
            dtEventAsset.TableName = "Asset";
            DataTable dtEventDet = ds.Tables[3];
            dtEventDet.TableName = "Details";
            DataRelation relEvent = new DataRelation(dtEvent.TableName, dtEvent.Columns["IDE"], dtEvent.Columns["IDE_EVENT"], false);
            DataRelation relEventClass = new DataRelation(dtEventClass.TableName, dtEvent.Columns["IDE"], dtEventClass.Columns["IDE"], false);
            DataRelation relEventAsset = new DataRelation(dtEventAsset.TableName, dtEvent.Columns["IDE"], dtEventAsset.Columns["IDE"], false);
            DataRelation relEventDet = new DataRelation(dtEventDet.TableName, dtEvent.Columns["IDE"], dtEventDet.Columns["IDE"], false);

            relEvent.Nested = true;
            relEventClass.Nested = true;
            relEventAsset.Nested = true;
            relEventDet.Nested = true;

            ds.Relations.Add(relEvent);
            ds.Relations.Add(relEventClass);
            ds.Relations.Add(relEventAsset);
            ds.Relations.Add(relEventDet);
            #endregion Relations

            if ((null != ds) && (0 < dtEvent.Rows.Count))
            {
                _ = DeserializeEvents(ds);
                ret = Cst.ErrLevel.SUCCESS;
            }

            return ret;
        }
        #endregion GetSelectEvents
        #region HideButtons
        // EG 202009014 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void HideButtons(string pButtonId)
        {
            if (FindControl(pButtonId) is WebControl btn)
            {
                btn.Visible = (null != TradeCommonActionEvents) && TradeCommonActionEvents.IsEditable;
            }
        }
        #endregion HideButtons
        #region PageConstruction
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void PageConstruction()
        {
            AbortRessource = true;
            string title = Ressource.GetString(m_TradeActionType.ToString());
            string subTitle = Ressource.GetString2("TradeActions", TradeCommonInput.Identifier);
            HtmlPageTitle titleLeft = new HtmlPageTitle(title, subTitle);
            PageTitle = title + Cst.Space + subTitle;
            GenerateHtmlForm();
            //
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), string.Empty, TradeCommonInputGUI.IdMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, TradeCommonInputGUI.CssMode, false, string.Empty, TradeCommonInputGUI.IdMenu);
        }
        #endregion PageConstruction
        #region ReleaseCurrentAction
        /// EG 20150513 [20513] Upd
        private void ReleaseCurrentAction()
        {
            TradeActionEventBase events = GetActionEvent(m_CurrentIdE);
            TradeActionEventBase eventParent = GetActionEvent(events.idEParent);
            if ((null != events) && (null != events.m_Action))
            {
                events.InitializeAction(eventParent);
                events.isModified = false;
            }
        }
        #endregion ReleaseCurrentAction
        #region SetActionMQueue
        protected virtual MQueueBase SetActionMQueue(SortedList pSortedAction, IdInfo pIdInfo)
        {
            return null;
        }
        #endregion SetActionMQueue
        #region SetTradeActionEvents
        protected virtual void SetTradeActionEvents()
        {
        }
        #endregion SetTradeActionEvents
        #region UpdateTradeStSys
        /// <summary>
        /// Mise à jour des statuts STUSED => RESERVED
        /// </summary>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)-->
        private Cst.ErrLevel UpdateTradeStSys()
        {
            IDbTransaction dbTransaction = null;
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            try
            {
                dbTransaction = DataHelper.BeginTran(SessionTools.CS);
                
                #region Select Trade Status USEDBY
                SQL_TradeCommon sqlTradeStSys = new SQL_TradeCommon(SessionTools.CS, TradeCommonInput.IdT);
                string SQLQuery = sqlTradeStSys.GetQueryParameters(new string[] { "TRADE.IDT", "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY" }).QueryReplaceParameters;

                DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, SQLQuery);
                DataTable dt = ds.Tables[0];
                DataRow dr = dt.Rows[0];
                #endregion Select Trade Status USEDBY

                #region Update Trade Status USEDBY
                dr.BeginEdit();
                dr["IDSTUSEDBY"] = Cst.StatusUsedBy.RESERVED.ToString();
                dr["LIBSTUSEDBY"] = Cst.ProcessTypeEnum.TRADEACTGEN.ToString();
                // FI 20200820 [25468] dates systemes en UTC
                dr["DTSTUSEDBY"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                dr["IDASTUSEDBY"] = SessionTools.AppSession.IdA;
                dr.EndEdit();
                //
                DataHelper.ExecuteDataAdapter(dbTransaction, SQLQuery, dt);
                DataHelper.CommitTran(dbTransaction);
                #endregion Update Trade Status USEDBY
            }
            catch (Exception)
            {
                DataHelper.RollbackTran(dbTransaction);
                throw;
            }

            return ret;
        }
        #endregion UpdateTradeStSys
        #region ValidateAction
        /// <summary>
        /// Validate of the actions
        /// <para>Modifications sending to the <b>VariousAction windows service</b> via MQueue type</para>
        /// </summary>
        /// EG 20150513 [20513] Upd
        private void ValidateAction()
        {
            ArrayList lstIdE = TradeCommonActionEvents.GetChanges;
            if (null != lstIdE)
            {
                SortedList sortAction = new SortedList();
                object valueAction = null;
                foreach (int idE in lstIdE)
                {
                    /// EG 20150513 [20513] Upd
                    TradeActionEventBase events = GetActionEvent(idE);
                    TradeActionEventBase eventParent = GetActionEvent(events.idEParent);
                    string keyAction = events.idEParent + "_" + eventParent.idEParent + "_";
                    if ((null != events) && (null != events.m_Action))
                    {
                        if (events.IsEventChanged)
                        {
                            Type tAction = events.m_Action.GetType();
                            MethodInfo method = tAction.GetMethod("PostedAction");
                            if (null != method)
                            {
                                object[] _params = null;
                                string[] _names = null;
                                ParameterInfo[] pi = method.GetParameters();
                                if (0 < pi.Length)
                                {
                                    _params = new object[] { keyAction };
                                    _names = new String[] { pi[0].Name };
                                }
                                valueAction = tAction.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, events.m_Action, _params, null, null, _names);
                            }
                            if (null != valueAction)
                            {
                                /// EG 20150513 [20513] Upd
                                keyAction = ((ActionMsgBase)valueAction).keyAction;
                                if (sortAction.ContainsKey(keyAction))
                                    ((ArrayList)sortAction[keyAction]).Add(valueAction);
                                else
                                {
                                    ArrayList aAction = new ArrayList
                                    {
                                        valueAction
                                    };
                                    sortAction.Add(keyAction, aAction);
                                }
                            }
                        }
                    }
                }
                if (0 < sortAction.Count)
                {
                    SendMessageAction(sortAction);
                    UpdateTradeStSys();
                }
            }
        }
        #endregion New_ValidateAction
        #region SendMessageAction
        /// <summary>
        /// Validate of the actions
        /// <para>Modifications sending to the <b>VariousAction windows service</b> via MQueue type</para>
        /// </summary>
        /// EG 20150513 [20513] Upd
        private void SendMessageAction(SortedList pSortAction)
        {
            
            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                connectionString = SessionTools.CS,
                Session = SessionTools.AppSession
            };

            IdInfo idInfo = new IdInfo()
            {
                id = TradeCommonInput.IdT,
                idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", TradeCommonInput.SQLProduct.GProduct) }
            };
            taskInfo.idInfo = new IdInfo[1] { idInfo };

            MQueueBase taMQueue = SetActionMQueue(pSortAction, idInfo);
            taMQueue.identifierSpecified = true;
            taMQueue.identifier = TradeCommonInput.Identifier;
            taskInfo.mQueue = new MQueueBase[] { taMQueue };
            taskInfo.process = taMQueue.ProcessType;

            taskInfo.trackerAttrib = new TrackerAttributes()
            {
                process = taMQueue.ProcessType,
                gProduct  = TradeCommonInput.SQLProduct.GProduct,
                caller = TradeActionType.GetMenuActionType(m_TradeActionType),
                info = new List<DictionaryEntry>()
            };
            string idDataIdent = Cst.OTCml_TBL.TRADE.ToString();
            if (TradeCommonInput.SQLProduct.IsRiskProduct)
                idDataIdent = Cst.OTCml_TBL.TRADERISK.ToString();
            else if (TradeCommonInput.SQLProduct.IsAssetProduct)
                idDataIdent = Cst.OTCml_TBL.TRADEDEBTSEC.ToString();
            else if (TradeCommonInput.SQLProduct.IsAdministrativeProduct)
                idDataIdent = Cst.OTCml_TBL.TRADEADMIN.ToString();
            taskInfo.trackerAttrib.info.Add(new DictionaryEntry("IDDATA", TradeCommonInput.IdT));
            taskInfo.trackerAttrib.info.Add(new DictionaryEntry("IDDATAIDENT", idDataIdent));
            taskInfo.trackerAttrib.info.Add(new DictionaryEntry("IDDATAIDENTIFIER", TradeCommonInput.Identifier));
            taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.idInfo[0]);
            

            
            int idTRK_L = 0;
            MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);

            SQL_LastTrade_L sqlLastTradeLog = TradeCommonInput.SQLLastTradeLog;
            for (int i = 0; i < taskInfo.mQueue.GetLength(0); i++)
            {
                taMQueue = taskInfo.mQueue[i];
                /// EG 20150513 [20513] Upd
                TradeActionBaseMQueue[] tradeActionItem = taMQueue.TradeActionItem;
                for (int j = 0; j < tradeActionItem.GetLength(0); j++)
                {
                    string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADETRAIL + Cst.CrLf;
                    sqlQuery += "(IDT, IDA, DTSYS, SCREENNAME,ACTION, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDTRADE_P, IDTRK_L)" + Cst.CrLf;
                    sqlQuery += "values (";
                    sqlQuery += taMQueue.id.ToString();
                    sqlQuery += ", " + taskInfo.Session.IdA.ToString();
                    sqlQuery += ", " + DataHelper.SQLGetDate(taskInfo.connectionString);
                    sqlQuery += ", " + DataHelper.SQLString(sqlLastTradeLog.ScreenName);
                    sqlQuery += ", " + DataHelper.SQLString(tradeActionItem[j].tradeActionCode.ToString());
                    sqlQuery += ", " + DataHelper.SQLString(taskInfo.Session.AppInstance.HostName);
                    sqlQuery += ", " + DataHelper.SQLString(taskInfo.Session.AppInstance.AppName);
                    sqlQuery += ", " + DataHelper.SQLString(taskInfo.Session.AppInstance.AppVersion);
                    sqlQuery += ", " + DataHelper.SQLString(taskInfo.Session.BrowserInfo);
                    sqlQuery += ", " + SQLCst.NULL;
                    sqlQuery += ", " + idTRK_L.ToString();
                    sqlQuery += ")";
                    DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
                }
            }
        }
        #endregion SendMessageAction
    }
}
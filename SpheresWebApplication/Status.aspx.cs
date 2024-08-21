using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Permission;
using EFS.Restriction;
using EFS.Status;
using EFS.TradeInformation;
using EFS.Tuning;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    /// <summary>
    /// Modification et consultation des Status d'un trade
    /// </summary>
    /// FI 20161124 [22634] Refactoring (Gestion de TRADESTATUS et EVENTSTATUS)
    public partial class StatusCapturePage : PageBase
    {
        #region Membres
        
        /// <summary>
        /// Event ou Trade
        /// </summary>
        private Mode modeEnum;

        /// <summary>
        /// Représente l'identifiant unique de la page opener
        /// </summary>
        private string m_ParentGUID;
        /// <summary>
        /// <para>Attention (m_InputGUI.IsModeConsult == true) signifie utlisateur est en consult sur la Page TradeCapturePage.aspx ou  EventCapturePage.apx </para>
        /// </summary>
        private InputGUI m_InputGUI;
        /// <summary>
        /// Représente le trade ou l'événement
        /// </summary>
        private CommonInput m_CommonInput;
        
        /// <summary>
        ///  Liste des status paramétrés par type de status
        ///  <para>La clé est de de type StatusEnum</para>
        /// </summary>
        private readonly Hashtable htstUser = new Hashtable() ;

        #endregion Members

        #region Accessors

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


        /// <summary>
        /// Obtient true si la modification est autorisée
        /// </summary>
        private bool IsButtonOkEnabled
        {
            get
            {
                RestrictionPermission permission = m_InputGUI.Permission;

                bool ret = false;
                ret = ret || (permission.IsPermissionEnabled(PermissionEnum.StatusEnvironment));
                ret = ret || (permission.IsPermissionEnabled(PermissionEnum.StatusBusiness));
                ret = ret || (permission.IsPermissionEnabled(PermissionEnum.StatusActivation));
                ret = ret || (permission.IsPermissionEnabled(PermissionEnum.StatusPriority));
                ret = ret || (permission.IsPermissionEnabled(PermissionEnum.StatusCheck));
                ret = ret || (permission.IsPermissionEnabled(PermissionEnum.StatusMatch));
                return ret;
            }
        }
             

        /// <summary>
        /// Obtient true si les variables session sont disponibles
        /// <remarks>Lorsque les variables session sont non disponibles cette page s'autoclose</remarks>
        /// </summary>
        protected bool IsSessionVariableAvailable
        {

            get { return (null != m_CommonInput); }
        }

        #endregion Accessors
        
        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20161124 [22634] Refactoring (gestion des status sur EVENT)
        private void OnButtonActionClick(object sender, EventArgs e)
        {
            int tradeLastIda;
            int idI;
            int idT;
            SpheresIdentification tradeIdentification;
            CommonStatus inputStatus;
            CommonStatus cloneInputStatus;
            switch (this.modeEnum)
            {
                case Mode.Trade:
                    TradeCommonInput tradeCommonInput = ((TradeCommonInput)m_CommonInput);
                    tradeIdentification = tradeCommonInput.Identification;

                    inputStatus = tradeCommonInput.TradeStatus as CommonStatus;
                    tradeLastIda = tradeCommonInput.SQLLastTradeLog.IdA;
                    idT = tradeCommonInput.IdT;
                    idI = tradeCommonInput.SQLInstrument.IdI;

                    cloneInputStatus = tradeCommonInput.TradeStatus.Clone() as CommonStatus;

                    break;
                case Mode.Event:
                    EventInput eventInput = ((EventInput)m_CommonInput);
                    tradeIdentification = eventInput.TradeIdentification;

                    inputStatus = eventInput.CurrentEventStatus as CommonStatus;
                    // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                    SQL_LastTrade_L SQLLastTradeLog = new SQL_LastTrade_L(SessionTools.CS, eventInput.IdT,
                        new PermissionEnum[] { PermissionEnum.Create, PermissionEnum.Modify, PermissionEnum.ModifyPostEvts, PermissionEnum.ModifyFeesUninvoiced, PermissionEnum.Remove });
                    SQLLastTradeLog.LoadTable(new string[] { "IDA" });
                    tradeLastIda = SQLLastTradeLog.IdA;
                    idT = eventInput.IdT;
                    idI = eventInput.SQLInstrument.IdI;

                    cloneInputStatus = eventInput.CurrentEventStatus.Clone() as CommonStatus;

                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", modeEnum.ToString()));
            }


            //Date système pour alimentation des colonnes DTUPD et DTINS ou equivalentes
            // FI 20200820 [25468] Dates systèmes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(SessionTools.CS);

            // Validation
            UpdateStatusFromControls(cloneInputStatus);

            bool isReloadOpener = true;

            string msg = string.Empty;
            bool isOk = true;
            LockObject lockTrade = null;
            try
            {
                if (isOk)
                {
                    // FI 20190524 [23912] Modification du fait du chgt de signature des méthodes IsStatusCheckAllowed et IsStatusMatchAllowed
                    
                    string msgCheck = string.Empty;
                    isOk = TuningTools.IsStatusCheckAllowed(SessionTools.CS, idT, idI, cloneInputStatus, tradeLastIda,
                        SessionTools.Collaborator_IDA, SessionTools.ActorAncestor, out msgCheck);
                    msg += msgCheck;

                    string msgMatch = string.Empty;
                    isOk = isOk && TuningTools.IsStatusMatchAllowed(SessionTools.CS, idT, idI, cloneInputStatus, tradeLastIda,
                        SessionTools.Collaborator_IDA, SessionTools.ActorAncestor, out msgMatch);
                    if (StrFunc.IsFilled(msg))
                        msg += Cst.CrLf;
                    msg += msgMatch;
                }

                if (isOk)
                {
                    if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode))
                    {
                        UpdateStatusFromControls(inputStatus);

                        if (isOk)
                        {
                            lockTrade = new LockObject(TypeLockEnum.TRADE, tradeIdentification.OTCmlId,
                                                                            tradeIdentification.Identifier, LockTools.Exclusive);
                            Lock lck = new Lock(SessionTools.CS, lockTrade, SessionTools.AppSession, PermissionEnum.Modify.ToString());
                            isOk = LockTools.LockMode1(lck, out Lock lckExisting);
                            if (false == isOk)
                                msg = lckExisting.ToString();
                        }

                        if (isOk)
                        {
                            switch (this.modeEnum)
                            {
                                case Mode.Trade:
                                    TradeCommonInput tradeCommonInput = ((TradeCommonInput)m_CommonInput);
                                    SQL_TradeStSys sqltradeStSys = new SQL_TradeStSys(SessionTools.CS, tradeCommonInput.IdT)
                                    {
                                        IsAddRowVersion = true
                                    };
                                    sqltradeStSys.LoadTable(new string[] { "IDT" });
                                    isOk = (tradeCommonInput.TradeStatus.sqlTradeStSysSource.RowVersion == sqltradeStSys.RowVersion);
                                    // RD 20100115 [16792]
                                    if (false == isOk)
                                        msg = Ressource.GetString(DataHelper.GetSQLErrorMessage(SQLErrorEnum.Concurrency));
                                    break;
                                case Mode.Event:
                                    break;
                            }
                        }

                        if (isOk)
                        {
                            switch (this.modeEnum)
                            {
                                case Mode.Trade:
                                    TradeCommonInput tradeCommonInput = ((TradeCommonInput)m_CommonInput);
                                    isOk = tradeCommonInput.TradeStatus.UpdateStatus(SessionTools.CS, null, tradeIdentification.OTCmlId, SessionTools.User.IdA, dtSys);
                                    break;
                                case Mode.Event:
                                    EventInput EventInput = ((EventInput)m_CommonInput);
                                    isOk = EventInput.CurrentEventStatus.UpdateStatus(SessionTools.CS, null, EventInput.CurrentEvent.idE, SessionTools.User.IdA, dtSys);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        UpdateStatusFromControls(inputStatus);
                    }
                }

                if (isOk)
                {
                    switch (this.modeEnum)
                    {
                        case Mode.Trade:
                            // RD 20100831 [] En cas d'Allocation pour ETD, les trois cases de cycle de vie de la deuxième partie sont décochées
                            TradeCommonInput tradeCommonInput = ((TradeCommonInput)m_CommonInput);
                            tradeCommonInput.TradeNotification.PartyNotification[1].SetConfirmation((false == tradeCommonInput.IsAllocation));
                            break;
                        case Mode.Event:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                isOk = false;
            }
            finally
            {
                try
                {
                    if (null != lockTrade)
                        LockTools.UnLock(SessionTools.CS, lockTrade, SessionTools.AppSession.SessionId);
                }
                catch
                {
                    //Si ça plante RAS, le lock sera supprimé en fin de session 
                }
                string msgReturn = null;
                string resource = StrFunc.AppendFormat("Msg_Confirm{0}StSaved", this.modeEnum.ToString());
                string identifier = string.Empty;
                switch (modeEnum)
                {
                    case Mode.Trade:
                        identifier = ((TradeCommonInput)m_CommonInput).Identification.Identifier;
                        break;
                    case Mode.Event:
                        identifier = ((EventInput)m_CommonInput).CurrentEvent.idE.ToString();
                        break;
                }

                if (isOk)
                {
                    if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode))
                        msgReturn = Ressource.GetString2(resource, identifier);
                }
                else
                {
                    isReloadOpener = false;
                    msgReturn = Ressource.GetString2(resource, identifier);
                    msgReturn += Cst.CrLf + msg;
                }

                if (StrFunc.IsFilled(msgReturn))
                    //JavaScript.AlertStartUpImmediate((PageBase)this, msgReturn, isOk);
                    JavaScript.DialogStartUpImmediate(this, msgReturn, isOk, isOk ? ProcessStateTools.StatusEnum.NONE : ProcessStateTools.StatusEnum.ERROR);
            }

            if (isReloadOpener) 
            {
                switch (this.modeEnum)
                {
                    case Mode.Trade:
                        if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode))
                        {
                            JavaScript.SubmitOpenerAndSelfClose("SaveAndClose", (PageBase)this, "tblMenu$mnuConsult", Cst.Capture.MenuConsultEnum.LoadTrade.ToString());
                        }
                        else
                        {
                            //Spheres redessine l'écran pour prendre en considération l'éventuel nouveau statut Business
                            JavaScript.SubmitOpenerAndSelfClose("SaveAndClose", (PageBase)this, "tblMenu$mnuConsult", Cst.Capture.MenuConsultEnum.SetScreen.ToString());
                        }
                        break;

                    case Mode.Event:
                        if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode))
                        {
                            JavaScript.SubmitOpenerAndSelfClose("SaveAndClose", (PageBase)this, "tblMenu$mnuConsult", Cst.Capture.MenuConsultEnum.LoadEvent.ToString());
                        }
                        else
                        {
                            JavaScript.SubmitOpenerAndSelfClose("SaveAndClose", (PageBase)this, "tblMenu$mnuConsult", Cst.Capture.MenuConsultEnum.SetScreen.ToString());
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20140728 [20255] Modify
        /// FI 20161124 [22634] Refactoring (gestion des status sur EVENT)
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            string mode = Request.QueryString["Mode"];
            if (StrFunc.IsEmpty(mode))
                throw new ArgumentException("Argument Mode expected. (eg Mode=Event or Mode=Trdade)");

            try
            {
                modeEnum = (Mode)Enum.Parse(typeof(Mode), mode);
            }
            catch
            {
                throw new NotSupportedException(StrFunc.AppendFormat("value:{0} is not valid. (eg Mode=Event or Mode=Trade)", mode));
            }

            //FI 20140728 [20255] => AbortRessource = true
            AbortRessource = true;

            //FI 20140728 [20255] => call PageTools.SetHead pour inscrire le jquery
            PageTools.SetHead(this, "TradeStatus", null, null);

            //20090923 FI affectation de Form
            Form = frmStatus;
            AntiForgeryControl();

            AddInputHiddenAutoPostback();

            InitializeComponent();

            base.OnInit(e);

            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 20200518 [XXXXX] Use DataCache
            //m_InputGUI = (InputGUI)Session[InputGUISessionID];
            //m_CommonInput = (CommonInput)Session[InputSessionID];

            m_InputGUI = DataCache.GetData<InputGUI>(ParentInputGUISessionID);
            m_CommonInput = DataCache.GetData<CommonInput>(ParentInputSessionID);

            if (IsSessionVariableAvailable)
            {
                SetPanelHeader();
                AddToolBar();
                AddBody();
            }
            else if (false == IsSessionVariableAvailable)
            {
                MsgForAlertImmediate = Ressource.GetString("Msg_SessionVariableParentNotAvailable");
                CloseAfterAlertImmediate = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140728 [20255] Modify
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void OnLoad(object sender, System.EventArgs e)
        {
            if (IsSessionVariableAvailable)
            {
                CommonStatus inputStatus = null;
                switch (this.modeEnum)
                {
                    case Mode.Trade:
                        inputStatus = ((TradeCommonInput)m_CommonInput).TradeStatus as CommonStatus;
                        break;
                    case Mode.Event:
                        inputStatus = ((EventInput)m_CommonInput).CurrentEventStatus as CommonStatus;
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", modeEnum.ToString()));
                }

                #region Chargement des DLL (Status system)
                IEnumerable status = from item in StatusTools.GetAvailableStatus(modeEnum)
                                                        .Where(x => (false == Status.StatusTools.IsStatusUser(x) && x != StatusEnum.StatusUsedBy))
                                     select item;

                //Sauvegarde des valeurs courantes si Postback 
                Hashtable sav = new Hashtable();
                if (IsPostBack)
                {
                    foreach (StatusEnum item in status)
                    {
                        string controlId = StrFunc.AppendFormat("htmlddl{0}", item.ToString().Replace("Status", "St"));
                        if (!(FindControl(controlId) is DropDownList ddl))
                            throw new NullReferenceException(StrFunc.AppendFormat("Control : {0} not found", controlId));
                        sav[item] = ddl.SelectedValue;
                    }
                }

                // chgt DDLs effectué à chaque post pour restitution des couleurs de chaque item (Etat non maintenu en natif par asp)
                LoadDDLs();

                if (IsPostBack)
                {
                    // Restitution des valeurs courantes si Postback 
                    foreach (StatusEnum item in status)
                    {
                        string key = item.ToString().Replace("Status", "st");
                        Status.Status st = (Status.Status)ReflectionTools.GetElementByName(inputStatus, key);

                        string key2 = item.ToString().Replace("Status", "St");
                        string controlId = StrFunc.AppendFormat("htmlddl{0}", key);
                        if (!(FindControl(controlId) is DropDownList ddl))
                            throw new NullReferenceException(StrFunc.AppendFormat("Control : {0} not found", controlId));
                        ddl.SelectedValue = sav[item] as string;
                    }
                }
                else if (!IsPostBack)
                {
                    foreach (StatusEnum item in status)
                    {
                        string key = item.ToString().Replace("Status", "st");
                        Status.Status stItem = (Status.Status)ReflectionTools.GetElementByName(inputStatus, key);

                        string key2 = item.ToString().Replace("Status", "St");
                        string controlId = $"htmlddl{key2}";
                        if (!(FindControl(controlId) is DropDownList ddl))
                            throw new NullReferenceException(StrFunc.AppendFormat("Control : {0} not found", controlId));
                        ddl.SelectedValue = stItem.NewSt;

                        controlId = $"txt{key2}";
                        if (!(FindControl(controlId) is Label lbl))
                            throw new NullReferenceException(StrFunc.AppendFormat("Control : {0} not found", controlId));
                        lbl.Text = stItem.CurrentStExtend.ExtValue;
                        lbl.ForeColor = Color.FromName(stItem.CurrentStExtend.ForeColor);
                        lbl.BackColor = Color.FromName(stItem.CurrentStExtend.BackColor);
                    }
                }
                #endregion

                #region Chargement des DLL (Status USER)
                if (!IsPostBack)
                {
                    //FI 20140728 [20255] utilisation des contrôles check dynamic
                    StatusCollection stChecks = inputStatus.GetStUsersCollection(StatusEnum.StatusCheck);
                    InitControlStatusCollection(stChecks);

                    StatusCollection stMatchs = inputStatus.GetStUsersCollection(StatusEnum.StatusMatch);
                    InitControlStatusCollection(stMatchs);

                }
                #endregion


            }
        }
        #endregion Events

        #region Methods
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " repository" };
            string btnID = string.Empty;
            string btnClass = string.Empty;
            string js = string.Empty;
            if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode))
            {
                btnID = "btnRecord";
                btnClass = "fas fa-save";
                js = "return confirm(" + JavaScript.JSString(Ressource.GetString("Msg_RecordData")) + ");";
            }
            else
            {
                btnID = "btnOk";
                btnClass = "fa fa-check";
            }

            WCToolTipLinkButton btnOkRecord = ControlsTools.GetAwesomeButtonAction(btnID, btnClass, true);
            btnOkRecord.Click += new EventHandler(OnButtonActionClick);
            btnOkRecord.OnClientClick = js;
            btnOkRecord.Enabled = IsButtonOkEnabled;
            pnlToolBar.Controls.Add(btnOkRecord);

            AddInputHiddenDeFaultControlOnEnter();
            HiddenFieldDeFaultControlOnEnter.Value = btnOkRecord.ClientID;

            if (false == IsPostBack)
            {
                try {SetFocus(btnOkRecord.ClientID);}
                catch{ /*Inutile de planter si le focus génère une erreur*/ };
            }

            WCToolTipLinkButton btnCancel = ControlsTools.GetAwesomeButtonCancel(false);
            pnlToolBar.Controls.Add(btnCancel);

            plhToolBar.Controls.Add(pnlToolBar);
        }


        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + m_InputGUI.MainMenuClassName };
            pnlBody.Controls.Add(AddStatusSystem());
            pnlBody.Controls.Add(AddStatusUser());
            plhBody.Controls.Add(pnlBody);
        }
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected WCTogglePanel AddStatusSystem()
        {
            WCTogglePanel togglePanel = new WCTogglePanel() {ID="divstsys", CssClass = CSSMode + " " + m_InputGUI.MainMenuClassName };
            togglePanel.SetHeaderTitle(Ressource.GetString("lblStSys"));
            Panel pnlStSys = new Panel() { ID = "divstdet_sys"};

            Panel pnlStSysDetail = new Panel() { ID = "divstdet_sys_hdr"};
            pnlStSysDetail.Controls.Add(new LiteralControl("<span style='width:80px;'>&nbsp;</span>"));
            WCTooltipLabel lbl1 = new WCTooltipLabel() { ID = "lbl1", Text = Ressource.GetString("lblCurrent"), Width = Unit.Pixel(190) }; pnlStSysDetail.Controls.Add(lbl1);
            WCTooltipLabel lbl2 = new WCTooltipLabel() { ID = "lbl2", Text = Ressource.GetString("lblNew"), Width = Unit.Pixel(190) }; pnlStSysDetail.Controls.Add(lbl2);
            pnlStSys.Controls.Add(pnlStSysDetail);
            RestrictionPermission permission = m_InputGUI.Permission;

            IEnumerable status = from item in StatusTools.GetAvailableStatus(modeEnum).Where(x => (false == Status.StatusTools.IsStatusUser(x) && x != StatusEnum.StatusUsedBy))
                                 select item;
            foreach (StatusEnum item in status)
            {
                pnlStSysDetail = new Panel();
                string key = item.ToString().Replace("Status", "St");
                // Label du statut
                Label lbl = new Label()
                {
                    ID = StrFunc.AppendFormat("lbl{0}", key),
                    Text = item.ToString().Replace("Status", string.Empty),
                    Width = Unit.Pixel(80)
                };
                pnlStSysDetail.Controls.Add(lbl);

                // Valeur actuelle du statut
                lbl = new Label()
                {
                    ID = StrFunc.AppendFormat("txt{0}", key),
                    Text = item.ToString().Replace("Status", string.Empty),
                    Width = Unit.Pixel(190)
                };
                pnlStSysDetail.Controls.Add(lbl);

                // Nouvelle valeur du statut
                PermissionEnum perm = (PermissionEnum)Enum.Parse(typeof(PermissionEnum), item.ToString());
                DropDownList ddl = new DropDownList()
                {
                    ID = StrFunc.AppendFormat("htmlddl{0}", key),
                    CssClass = "ddlCapture",
                    Enabled = permission.IsPermissionEnabled(perm),
                    Width = Unit.Pixel(190)
                };
                pnlStSysDetail.Controls.Add(ddl);

                pnlStSys.Controls.Add(pnlStSysDetail);
            }

            togglePanel.AddContent(pnlStSys);
            return togglePanel;
        }
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected WCTogglePanel AddStatusUser()
        {
            WCTogglePanel togglePanel = new WCTogglePanel() { ID = "divstuser", CssClass = CSSMode + " " + m_InputGUI.MainMenuClassName };
            togglePanel.SetHeaderTitle(Ressource.GetString("lblStUser"));
            Panel pnlStUser = new Panel() { ID = "divstdet_user" };

            Panel pnlStUserDetail = new Panel() { ID = "divstdet_user_hdr" };
            pnlStUserDetail.Controls.Add(new LiteralControl("<span style='width:80px;'>&nbsp;</span>"));
            WCTooltipLabel lbl1 = new WCTooltipLabel() { ID = "lbl4", Text = Ressource.GetString("lblCurrent"), Width = Unit.Pixel(150) }; pnlStUserDetail.Controls.Add(lbl1);
            WCTooltipLabel lbl2 = new WCTooltipLabel() { ID = "lbl5", Text = Ressource.GetString("lblNew"), Width = Unit.Pixel(150) }; pnlStUserDetail.Controls.Add(lbl2);
            WCTooltipLabel lbl3 = new WCTooltipLabel() { ID = "lbl6", Text = Ressource.GetString("RelevantDate"), Width = Unit.Pixel(100) }; pnlStUserDetail.Controls.Add(lbl3);
            WCTooltipLabel lbl4 = new WCTooltipLabel() { ID = "lbl7", Text = Ressource.GetString("NOTE"), Width = Unit.Pixel(420) }; pnlStUserDetail.Controls.Add(lbl4);
            pnlStUser.Controls.Add(pnlStUserDetail);

            AddDivStatusUserList(pnlStUser,StatusEnum.StatusCheck, (false == m_InputGUI.Permission.IsPermissionEnabled(PermissionEnum.StatusCheck)));
            pnlStUser.Controls.Add(new LiteralControl("<hr/>"));
            AddDivStatusUserList(pnlStUser, StatusEnum.StatusMatch, (false == m_InputGUI.Permission.IsPermissionEnabled(PermissionEnum.StatusMatch)));

            togglePanel.AddContent(pnlStUser);
            return togglePanel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161124 [22634] modify
        private void DDLLoad_StatusActivation()
        {
            Status.Status stActivation;
            switch (this.modeEnum)
            {
                case Mode.Trade:
                    stActivation = ((TradeCommonInput)m_CommonInput).TradeStatus.stActivation;
                    break;
                case Mode.Event:
                    stActivation = ((EventInput)m_CommonInput).CurrentEventStatus.stActivation;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", modeEnum.ToString()));
            }

            ArrayList alForcedValue = new ArrayList();
            bool isExcludeForcedValue = false;
            //
            if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeUpdateGen(m_InputGUI.CaptureMode))
            {
                string currentStActivation = stActivation.CurrentSt;

                // RD 20120328 

                // En mode Consultation:
                // ---------------------
                //  Si le trade est Incomplet, donc en statut d'activation "Missing", on pourra:
                //      - soit le laisser en Incomplet: le statut d'activation "Missing" sera affiché dans la DDL
                //      - soit le désactiver: le statut d'activation "Deactiv" sera affiché dans la DDL
                //  Si le trade est Désactivé, donc en statut d'activation "Deactiv", on pourra:
                //      - uniquement le laisser désactivé: uniquement le statut d'activation "Deactiv" qui sera affiché dans la DDL (comme c'est déjà le cas)
                //  Si le trade n'est ni Désactivé ni Incomplet, on ne pourra:
                //      - ni le désactiver: le statut d'activation à "Deactiv" ne sera pas affiché dans la DDL
                //      - ni le mettre en Incomplet: le statut d'activation à "Missing" ne sera pas affiché dans la DDL
                //
                // En mode Modification: comme c'est déjà le cas
                // ---------------------
                //  Si le trade est Désactivé, donc en statut d'activation "Deactiv", on pourra:
                //      - uniquement le laisser désactivé: uniquement le statut d'activation "Deactiv" qui sera affiché dans la DDL
                //  Sinon, :
                //      - tous les statut d'activation seront affichés dans la DDL, 
                //      - sauf le le statut d'activation "Deactiv" on ne pourra pas donc le désactiver
                if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode))
                {
                    if (Cst.StatusActivation.MISSING.ToString() == currentStActivation)
                    {
                        alForcedValue.Add(Cst.StatusActivation.DEACTIV.ToString());
                        alForcedValue.Add(Cst.StatusActivation.MISSING.ToString());
                        isExcludeForcedValue = false;
                    }
                    else if (Cst.StatusActivation.DEACTIV.ToString() == currentStActivation)
                    {
                        alForcedValue.Add(Cst.StatusActivation.DEACTIV.ToString());
                        isExcludeForcedValue = false;
                    }
                    else
                    {
                        alForcedValue.Add(Cst.StatusActivation.DEACTIV.ToString());
                        alForcedValue.Add(Cst.StatusActivation.MISSING.ToString());
                        isExcludeForcedValue = true;
                    }
                }
                else
                {
                    alForcedValue.Add(Cst.StatusActivation.DEACTIV.ToString());
                    if (Cst.StatusActivation.DEACTIV.ToString() == currentStActivation)
                        isExcludeForcedValue = false;
                    else
                        isExcludeForcedValue = true;
                }
            }

            DropDownList htmlddlStActivation = this.FindControl("htmlddlStActivation") as DropDownList;
            ControlsTools.DDLLoad_StatusActivation(CSTools.SetCacheOn(SessionTools.CS), htmlddlStActivation, false, alForcedValue, isExcludeForcedValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20140705 Lecture de Sql_Instr.FUNGIBILITYMODE pour ACCES au STATUS ALLOC
        private void DDLLoad_StatusBusiness()
        {
            RestrictionPermission permission = m_InputGUI.Permission;
            TradeCommonInput TradeCommonInput = (TradeCommonInput)m_CommonInput;
            TradeStatus tradestatus = ((TradeCommonInput)m_CommonInput).TradeStatus;


            DropDownList htmlddlStBusiness = this.FindControl("htmlddlStBusiness") as DropDownList;

            ArrayList alForcedValue = new ArrayList();

            // En création:
            //		- Possibilite de choisir n'importe quel statut business
            // En modification:
            //		- Si PRETRADE   => Le trade peut être EXECUTED ou INTERMED
            //		- Si ALLOC      => Le trade reste ALLOC
            //		- Si EXECUTED   => Le trade reste EXECUTED
            //		- Si INTERMED   => Le trade reste INTERMED
            // NB: Il est toutefois nécessaire de disposer des droits...
            if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeCorrection(m_InputGUI.CaptureMode))
            {
                htmlddlStBusiness.Enabled = false;
                alForcedValue.Add(tradestatus.stBusiness.CurrentSt);
            }
            else if (Cst.Capture.IsModeUpdateGen(m_InputGUI.CaptureMode))
            {
                alForcedValue.Add(tradestatus.stBusiness.CurrentSt);
                if (tradestatus.IsCurrentStBusiness_PreTrade)
                {
                    if (permission.IsPermissionEnabled(PermissionEnum.Executed))
                        alForcedValue.Add(Cst.StatusBusiness.EXECUTED.ToString());
                    if (permission.IsPermissionEnabled(PermissionEnum.Intermed))
                        alForcedValue.Add(Cst.StatusBusiness.INTERMED.ToString());
                }
            }
            else if (Cst.Capture.IsModeNewCapture(m_InputGUI.CaptureMode))
            {
                if (permission.IsPermissionEnabled(PermissionEnum.PreTrade))
                    alForcedValue.Add(Cst.StatusBusiness.PRETRADE.ToString());
                if (permission.IsPermissionEnabled(PermissionEnum.Executed))
                    alForcedValue.Add(Cst.StatusBusiness.EXECUTED.ToString());
                if (permission.IsPermissionEnabled(PermissionEnum.Intermed))
                    alForcedValue.Add(Cst.StatusBusiness.INTERMED.ToString());

                // EG 20150331 (POC]
                if ((TradeCommonInput.SQLInstrument.FungibilityMode != EfsML.Enum.FungibilityModeEnum.NONE) || TradeCommonInput.SQLInstrument.IsMargining)
                {
                    if (permission.IsPermissionEnabled(PermissionEnum.Alloc))
                        alForcedValue.Add(Cst.StatusBusiness.ALLOC.ToString());
                }
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not Implemented", m_InputGUI.CaptureMode.ToString()));
            }

            
            ControlsTools.DDLLoad_StatusBusiness(CSTools.SetCacheOn(SessionTools.CS), htmlddlStBusiness, false, alForcedValue, false);

        }

        /// <summary>
        /// Chargement de la DDL StatusEnvironment 
        /// </summary>
        private void DDLLoad_StatusEnvironment()
        {
            RestrictionPermission permission = m_InputGUI.Permission;
            TradeStatus tradeStatus = ((TradeCommonInput)m_CommonInput).TradeStatus;

            DropDownList htmlddlStEnvironment = this.FindControl("htmlddlStEnvironment") as DropDownList;

            ArrayList alForcedValue = new ArrayList();
            // En création:
            //		- Possibilite de choisir n'importe quel statut d'Environnement
            // En modification:
            //		- Si REGULAR    => Le trade reste REGULAR
            //		- [DEPRECATED] Si PRETRADE   => Le trade peut être REGULAR ou SIMUL
            //		- Si SIMULATION => Le trade peut PRETRADE ou REGULAR
            // NB: Il est toutefois nécessaire de disposer des droits...
            //
            // Ticket 15637 
            if (Cst.Capture.IsModeConsult(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeCorrection(m_InputGUI.CaptureMode)) //En mode correction On ne change pas les statuts
            {
                htmlddlStEnvironment.Enabled = false;
                alForcedValue.Add(tradeStatus.stEnvironment.CurrentSt);
            }
            else if (Cst.Capture.IsModeUpdateGen(m_InputGUI.CaptureMode))
            {
                alForcedValue.Add(tradeStatus.stEnvironment.CurrentSt);
                //En Modification, on passe de PRE-TRADE à SIMUL ou l'inverse=> pas le droit de passer en REGULAR 
                //Pour passer en regular ou simul 
                //20100311 PL-StatusBusiness
                //if (TradeStatus.IsCurrentStEnvironment_PreTrade)
                //{
                //    if ((m_TradeCommonInputGUI.IsModeUpdateGen) && Permission.IsPermissionEnabled(PermissionEnum.Regular))
                //        alForcedValue.Add(Cst.StatusEnvironment.REGULAR.ToString());

                //    if (Permission.IsPermissionEnabled(PermissionEnum.Simul))
                //        alForcedValue.Add(Cst.StatusEnvironment.SIMUL.ToString());
                //}
                if (tradeStatus.IsCurrentStEnvironment_Simul)
                {
                    if (permission.IsPermissionEnabled(PermissionEnum.Regular))
                        alForcedValue.Add(Cst.StatusEnvironment.REGULAR.ToString());
                    //20100311 PL-StatusBusiness
                    //if (Permission.IsPermissionEnabled(PermissionEnum.PreTrade))
                    //    alForcedValue.Add(Cst.StatusEnvironment.PRETRADE.ToString());
                }
            }
            else if (Cst.Capture.IsModeNewOrDuplicateOrReflect(m_InputGUI.CaptureMode))
            {
                if (permission.IsPermissionEnabled(PermissionEnum.Template))
                    alForcedValue.Add(Cst.StatusEnvironment.TEMPLATE.ToString());
                //20100311 PL-StatusBusiness
                //if (Permission.IsPermissionEnabled(PermissionEnum.System))
                //    alForcedValue.Add(Cst.StatusEnvironment.SYSTEM.ToString());
                //if (Permission.IsPermissionEnabled(PermissionEnum.PreTrade))
                //    alForcedValue.Add(Cst.StatusEnvironment.PRETRADE.ToString());
                if (permission.IsPermissionEnabled(PermissionEnum.Simul))
                    alForcedValue.Add(Cst.StatusEnvironment.SIMUL.ToString());
                if (permission.IsPermissionEnabled(PermissionEnum.Regular))
                    alForcedValue.Add(Cst.StatusEnvironment.REGULAR.ToString());
            }
            else if (Cst.Capture.IsModePositionTransfer(m_InputGUI.CaptureMode))
            {
                if (permission.IsPermissionEnabled(PermissionEnum.Regular))
                    alForcedValue.Add(Cst.StatusEnvironment.REGULAR.ToString());
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", m_InputGUI.CaptureMode.ToString()));
            }

            
            ControlsTools.DDLLoad_StatusEnvironment(CSTools.SetCacheOn(SessionTools.CS), htmlddlStEnvironment, false, alForcedValue, false);
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.OnLoad);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161124 [22634] modify
        private void LoadDDLs()
        {

            IEnumerable status = from item in StatusTools.GetAvailableStatus(modeEnum)
                                                        .Where(x => (false == Status.StatusTools.IsStatusUser(x) && x != StatusEnum.StatusUsedBy))
                                 select item;

            foreach (StatusEnum item in status)
            {
                switch (item)
                {
                    case StatusEnum.StatusEnvironment:
                        DDLLoad_StatusEnvironment();
                        break;
                    case StatusEnum.StatusBusiness:
                        DDLLoad_StatusBusiness();
                        break;
                    case StatusEnum.StatusActivation:
                        DDLLoad_StatusActivation();
                        break;
                    case StatusEnum.StatusPriority:
                        DropDownList  htmlddlStPriority =  this.FindControl("htmlddlStPriority") as DropDownList;
                        ControlsTools.DDLLoad_StatusPriority(CSTools.SetCacheOn(SessionTools.CS), htmlddlStPriority, false);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", item.ToString()));

                }
            }
        }

        /// <summary>
        ///  Mise à jour des status {pInputStatus} via lecture des contrôles web
        /// </summary>
        /// <param name="pInputStatus"></param>
        /// FI 20161124 [22634] Refactoring (gestion des status sur EVENT)
        private void UpdateStatusFromControls(CommonStatus pInputStatus)
        {

            IEnumerable status = from item in StatusTools.GetAvailableStatus(modeEnum)
                                        .Where(x => (false == Status.StatusTools.IsStatusUser(x) && x != StatusEnum.StatusUsedBy))
                                 select item;

            foreach (StatusEnum item in status)
            {
                string controlId = StrFunc.AppendFormat("htmlddl{0}", item.ToString().Replace("Status", "St"));
                if (!(FindControl(controlId) is DropDownList ddl))
                    throw new NullReferenceException(StrFunc.AppendFormat("Control : {0} not found", controlId));

                string key = item.ToString().Replace("Status", "st");
                Status.Status stItem = (Status.Status)ReflectionTools.GetElementByName(pInputStatus, key);

                stItem.NewSt = ddl.SelectedValue;
            }

            StatusCollection stChecks = pInputStatus.GetStUsersCollection(StatusEnum.StatusCheck);
            UpdateStatusCollectionFromControl(stChecks);

            StatusCollection stMatchs = pInputStatus.GetStUsersCollection(StatusEnum.StatusMatch);
            UpdateStatusCollectionFromControl(stMatchs);
        }

        /// <summary>
        /// Ajoute les contrôles dynamiques associés aux statuts Check et aux statuts Match
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="isReadOnly"></param>
        /// FI 20140728 [20255] add Method
        /// FI 20161123 [22630] Add
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddDivStatusUserList(Panel pPnlParent, StatusEnum pStatus, Boolean isReadOnly)
        {
            string query = GetQueryStatusUser(CSTools.SetCacheOn(SessionTools.CS), pStatus);
            string column = StatusTools.GetColumnNameStatusUser(pStatus);
            DataTable dt = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(SessionTools.CS), query);

            htstUser[pStatus] = dt;

            for (int i = 0; i < ArrFunc.Count(dt.Rows); i++)
            {
                DataRow dr = dt.Rows[i];
                string suffix = pStatus + dr[StatusTools.GetColumnNameStatusUser(pStatus)].ToString();

                Panel pnlStUserDetail = new Panel();
                if (i == 0)
                {
                    Label lbl = new Label()
                    {
                        Text = StatusTools.LibStatusType(pStatus),
                        Width = Unit.Pixel(80)
                    };
                    pnlStUserDetail.Controls.Add(lbl);
                }
                else
                {
                    pnlStUserDetail.Controls.Add(new LiteralControl("<span style='width:80px;'>&nbsp;</span>"));
                }

                WCCheckBox2 chkCurrent = new WCCheckBox2()
                {
                    ID = "chkCurrent" + suffix,
                    IsReadOnly = true,
                    Text = dr["DISPLAYNAME_WEIGHT"].ToString(),
                    Width = Unit.Pixel(150)
                };
                SetCheckStyle(chkCurrent, dr["FORECOLOR"] != Convert.DBNull ? dr["FORECOLOR"].ToString() : null,
                                        dr["BACKCOLOR"] != Convert.DBNull ? dr["BACKCOLOR"].ToString() : null);
                pnlStUserDetail.Controls.Add(chkCurrent);

                WCCheckBox2 chkNew = new WCCheckBox2()
                {
                    ID = "chkNew" + suffix,
                    IsReadOnly = isReadOnly,
                    Text = dr["DISPLAYNAME"].ToString(),
                    Width = Unit.Pixel(150)
                };

                if (BoolFunc.IsTrue(System.Configuration.ConfigurationManager.AppSettings["isUseCustomStatus"]))
                {
                    //Si la valeur du statut impactent d'autre statut => PostBack
                    var existsCustomReference = from item in dt.Rows.Cast<DataRow>().
                                   Where(x => x["CUSTOMVALUE"] != Convert.DBNull &&
                                              (x["CUSTOMVALUE"].ToString().Contains(dr[column].ToString()) ||
                                               x["CUSTOMVALUE"].ToString().Contains(StrFunc.FirstUpperCase(dr[column].ToString().ToLower())))
                                              )
                                              select item;

                    if (existsCustomReference.Count() > 0)
                    {
                        chkNew.AutoPostBack = true;
                        chkNew.CheckedChanged += new System.EventHandler(OnCheckedChanged);
                    }
                }

                SetCheckStyle(chkNew, dr["FORECOLOR"] != Convert.DBNull ? dr["FORECOLOR"].ToString() : null,
                                         dr["BACKCOLOR"] != Convert.DBNull ? dr["BACKCOLOR"].ToString() : null);

                pnlStUserDetail.Controls.Add(chkNew);

                // Date d'effet
                WCTextBox2 txtDtEffect = GetControlDate("txtDtEffect" + suffix, isReadOnly);
                pnlStUserDetail.Controls.Add(txtDtEffect);

                // Note
                WCTextBox2 txtNote = GetControlNote("txtNote" + suffix, isReadOnly);
                pnlStUserDetail.Controls.Add(txtNote);

                pPnlParent.Controls.Add(pnlStUserDetail);
            }
        }

        /// <summary>
        ///  pré-propostion des autres status en fonction de l'état d'un contôle web  (checkbox) représentant un staus
        /// </summary>
        /// <param name="sender">Contôle web de type checkbox</param>
        /// <param name="e"></param>
        /// FI 20161123 [22630] Add
        private void OnCheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is WCCheckBox2 chk))
                throw new NullReferenceException("chk is null");

            if (chk.Checked)
            {
                StatusEnum status = (from item in Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                                    .Where(x => StatusTools.IsStatusUser(x) &&
                                    chk.ClientID.Contains(x.ToString()))
                                     select item).FirstOrDefault();

                string stValue = chk.ClientID.Replace("chkNew" + status.ToString(), string.Empty);
                
                DataTable dt = (DataTable)this.htstUser[status];

                for (int i = 0; i < 2; i++)
                {
                    bool isUntickedWhen = (i == 0);
                    string search1 = isUntickedWhen ? StrFunc.AppendFormat("UntickedWhen{0}", stValue) : StrFunc.AppendFormat("TickedWhen{0}", stValue);
                    string search2 = isUntickedWhen ? StrFunc.AppendFormat("UntickedWhen{0}", StrFunc.FirstUpperCase(stValue.ToLower())) : StrFunc.AppendFormat("TickedWhen{0}", StrFunc.FirstUpperCase(stValue.ToLower()));

                    IEnumerable statusLst = dt.Rows.Cast<DataRow>()
                    .Where(x => x["CUSTOMVALUE"].ToString().Contains(search1) ||
                                x["CUSTOMVALUE"].ToString().Contains(search2));

                    foreach (DataRow item in statusLst)
                    {
                        string value = item[StatusTools.GetColumnNameStatusUser(status)].ToString();
                        string ctrlName1 = StrFunc.AppendFormat("chkNew{0}{1}", status.ToString(), value);
                        string ctrlName2 = StrFunc.AppendFormat("chkNew{0}{1}", status.ToString(), StrFunc.FirstUpperCase(value.ToLower()));

                        if (!(FindControl(ctrlName1) is WCCheckBox2 chkItem))
                            chkItem = FindControl(ctrlName2) as WCCheckBox2;

                        if (null != chkItem)
                            chkItem.Checked = !isUntickedWhen;
                    }
                }
            }
        }

        /// <summary>
        ///  Applique un style au contrôle checkBox
        /// </summary>
        /// <param name="checkBox"></param>
        /// <param name="pForeColor"></param>
        /// <param name="pBackColor"></param>
        /// FI 20140728 [20255] add Method
        // EG 20200826 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Attributes sur Label
        private static void SetCheckStyle(WCCheckBox2 checkBox, string pForeColor, string pBackColor)
        {
            string style = "white-space:nowrap";
            if (StrFunc.IsFilled(pForeColor))
                style += ";color:" + pForeColor + "!important";
            if (StrFunc.IsFilled(pBackColor))
                style += ";background-color:" + pBackColor + "!important";
            checkBox.LabelAttributes.Add("style", style);
            checkBox.Attributes.Add("style", style);
        }

        /// <summary>
        /// Retourne la requête de chargement des status Check ou match
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        /// FI 20140728 [20255] add Method
        private static string GetQueryStatusUser(string pCS, StatusEnum pStatus)
        {

            string column = StatusTools.GetColumnNameStatusUser(pStatus);

            string ret = SQLCst.SELECT + column + ",";
            ret += DataHelper.SQLConcat(pCS,
                "DISPLAYNAME",
                "' ('",
                DataHelper.SQLString(Ressource.GetString("WEIGHT") + ": "),
                DataHelper.SQLNumberToChar(pCS, "WEIGHT"),
                "')'");
            ret += " as DISPLAYNAME_WEIGHT,";
            ret += "DISPLAYNAME,";

            ret += "FORECOLOR, BACKCOLOR, CUSTOMVALUE" + Cst.CrLf;
            ret += SQLCst.FROM_DBO + StatusTools.GetTableNameStatusUser(pStatus) + Cst.CrLf;
            ret += SQLCst.ORDERBY + "WEIGHT" + SQLCst.ASC + ",DISPLAYNAME";

            return ret;
        }

        /// <summary>
        /// Retourne un contrôlte TextBox2 pour saisir une date 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        /// FI 20140728 [20255] add Method
        private static WCTextBox2 GetControlDate(string pClientId, Boolean isReadOnly)
        {

            #region Validator
            Validator[] validators = new Validator[4];
            if (false == isReadOnly)
            {
                #region RequireField
                Validator validatorRequireField = null;
                validators[0] = validatorRequireField;
                #endregion RequireField

                #region Regular Expression
                Validator validatorRegEx = null;
                validators[1] = validatorRegEx;
                #endregion Regular Expression

                #region ValidationDataType
                Validator validatorDataType = new Validator(ValidationDataType.Date, "[" + Ressource.GetString("InvalidData") + "]", true, false);
                validators[2] = validatorDataType;
                #endregion ValidationDataType

                #region CustomValidator
                Validator validatorCustom = null;
                validators[3] = validatorCustom;
                #endregion CustomValidator
            }
            #endregion Validator

            string cssClass = EFSCssClass.Capture;

            WCTextBox2 textBox = new WCTextBox2(pClientId, isReadOnly,
                false, false, cssClass, validators);

            //className DtPicker for Date JQuery UI DatePicker (type Date)
            if (false == isReadOnly)
                textBox.CssClass = "DtPicker " + textBox.CssClass;

            textBox.Enabled = (false == isReadOnly);

            if (isReadOnly)
                textBox.TabIndex = -1;

            textBox.AutoPostBack = false;
            textBox.Style.Add(HtmlTextWriterStyle.Width, "80px");

            return textBox;
        }

        /// <summary>
        /// Retourne un contrôlte TextBox2 pour saisir une note
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        /// FI 20140728 [20255] add Method
        private static WCTextBox2 GetControlNote(string pClientId, Boolean isReadOnly)
        {
            string cssClass = EFSCssClass.Capture;

            WCTextBox2 textBox = new WCTextBox2(pClientId, isReadOnly, false, false, cssClass, null)
            {
                Enabled = (false == isReadOnly)
            };
            if (isReadOnly)
                textBox.TabIndex = -1;

            textBox.AutoPostBack = false;
            textBox.Style.Add(HtmlTextWriterStyle.Width, "420px");

            return textBox;
        }

        /// <summary>
        /// 
        /// </summary>
        ///FI 20140728 [20255] add Method
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200821 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetPanelHeader()
        {
            string title;
            switch (this.modeEnum)
            {
                case Mode.Trade:
                    if (Cst.Capture.IsModeNewCapture(m_InputGUI.CaptureMode))
                        title = Ressource.GetString("NewTrade", true);
                    else
                        title = Ressource.GetString("Trade", true) + "&nbsp;" + ((TradeCommonInput)m_CommonInput).Identification.Identifier;
                    break;
                case Mode.Event:
                    if (Cst.Capture.IsModeNewCapture(m_InputGUI.CaptureMode))
                        title = Ressource.GetString("NewEvent", true);
                    else
                        title = Ressource.GetString("Event", true) + "&nbsp;" + ((EventInput)m_CommonInput).CurrentEventIdentifier + "&nbsp;" + ((EventInput)m_CommonInput).CurrentEvent.GetDisplayName();
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", modeEnum.ToString()));
            }

            PageTitle = title;
            HtmlPageTitle titleLeft = new HtmlPageTitle(Ressource.GetString("Statut"), title);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, m_InputGUI.IdMenu));
            plhHeader.Controls.Add(pnlHeader);
        }

        /// <summary>
        /// Alimente les contrôles à partir de la collection de statuts 
        /// </summary>
        /// <param name="pStatusCol"></param>
        /// FI 20140728 [20255] add method
        private void InitControlStatusCollection(StatusCollection pStatusCol)
        {
            for (int i = 0; i < ArrFunc.Count(pStatusCol.Status); i++)
            {
                string suffix = pStatusCol.StatusEnum.ToString() + pStatusCol.Status[i].CurrentSt;

                if (FindControl("chkCurrent" + suffix) is WCCheckBox2 chkCurrent)
                    chkCurrent.Checked = (pStatusCol.Status[i].CurrentValue == 1);

                if (FindControl("chkNew" + suffix) is WCCheckBox2 chkNew)
                    chkNew.Checked = (pStatusCol.Status[i].NewValue == 1);

                if (FindControl("txtDtEffect" + suffix) is WCTextBox2 txtDtEffect)
                {
                    Nullable<DateTime> dtEffect = pStatusCol.Status[i].NewDtEffect;
                    txtDtEffect.Text = (dtEffect.HasValue) ? DtFunc.DateTimeToString(dtEffect.Value, DtFunc.FmtShortDate) : string.Empty;
                }

                if (FindControl("txtNote" + suffix) is WCTextBox2 txtNote)
                    txtNote.Text = pStatusCol.Status[i].NewNote;
            }
        }
        
        /// <summary>
        /// Mise à jour de la collection de statuts à partir des données présentes dans les contrôles 
        /// </summary>
        /// <param name="pStatusCol"></param>
        /// FI 20140728 [20255] add method
        private  void UpdateStatusCollectionFromControl(StatusCollection pStatusCol)
        {
            for (int i = 0; i < ArrFunc.Count(pStatusCol.Status); i++)
            {
                string suffix = pStatusCol.StatusEnum.ToString() + pStatusCol.Status[i].CurrentSt;

                if (FindControl("chkNew" + suffix) is WCCheckBox2 chk)
                    pStatusCol[i].NewValue = Convert.ToInt32(chk.Checked);

                if ((FindControl("txtDtEffect" + suffix) is WCTextBox2 txtDtEffect) && StrFunc.IsFilled(txtDtEffect.Text))
                    pStatusCol[i].NewDtEffect = new DtFunc().StringToDateTime(txtDtEffect.Text);

                if ((FindControl("txtNote" + suffix) is WCTextBox2 txtNote) && StrFunc.IsFilled(txtNote.Text))
                    pStatusCol[i].NewNote = txtNote.Text;
            }
        }


        #endregion Methods
    }
}

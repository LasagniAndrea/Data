#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EFS.Status;
using EFS.TradeInformation;
using EfsML;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
//
//20071212 FI Ticket 16012 => Migration Asp2.0
#endregion Using Directives

namespace EFS.Spheres
{
    public partial class EventCapturePage : CapturePageBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private EventCaptureGen m_CaptureGen;

        /// <summary>
        /// 
        /// </summary>
        private EventInputGUI m_InputGUI;

        /// <summary>
        /// 
        /// </summary>
        protected string m_ParentGUID;

        /// <summary>
        /// 
        /// </summary>
        private EventHeaderBanner  m_EventHeaderBanner;

        #endregion Members

        #region Accessor
        /// <summary>
        ///  Obtient le contrôle txtEvent
        /// </summary>
        private Control CtrlEvent
        {
            get
            {
                Control ret = null;
                Control ctrlContainer = FindControl("tblMenu");
                if (null != ctrlContainer)
                    ret = ctrlContainer.FindControl("txtEvent");
                return ret;
            }
        }

        /// <summary>
        ///  Obtient le contrôle caché hihEventId
        /// </summary>
        private HtmlInputHidden CtrlEventId
        {
            get
            {
                Control ctrlContainer = FindControl("tblMenu");
                HtmlInputHidden ctrlEventId = null;
                if (null != ctrlContainer)
                    ctrlEventId = (HtmlInputHidden)ctrlContainer.FindControl("hihEventId");
                return ctrlEventId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected LockObject GetLockTrade()
        {
            LockObject lockTrade = null;
            if (EventInput.IsTradeFound)
                lockTrade = new LockObject(TypeLockEnum.TRADE, this.TradeIdT, this.TradeIdentifier, LockTools.Exclusive);
            return lockTrade;

        }

        /// <summary>
        /// Obtient ou définit la valeur Text du contrôle qui contient l'identifier de l'évènement (IdE)
        /// </summary>
        private string EventIdentifier
        {
            get
            {
                Control ctrl = CtrlEvent;
                if (null == CtrlEvent)
                    throw new NullReferenceException("CtrlEvent is null");

                return ((TextBox)ctrl).Text.Trim();
            }
            set
            {
                Control ctrl = CtrlEvent;
                if (null == CtrlEvent)
                    throw new NullReferenceException("CtrlEvent is null");
                ((TextBox)ctrl).Text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string EventIdentifierCode
        {
            get
            {
                string ret = string.Empty;
                if (null != EventInput.CurrentEvent)
                    ret = EventInput.CurrentEvent.GetDisplayName();
                return ret;
            }
        }

        /// <summary>
        ///  Alimente ou lit le contrôle caché CtrlEventId
        /// </summary>
        private int EventId
        {
            get
            {
                HtmlInputHidden ctrl = CtrlEventId;
                try { return (Convert.ToInt32(ctrl.Value)); }
                catch { return 0; }
            }
            set
            {
                HtmlInputHidden ctrl = CtrlEventId;
                if (null != ctrl)
                    ctrl.Value = value.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventInput EventInput
        {
            get { return m_CaptureGen.Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string FolderName
        {
            get { return "Event"; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override InputGUI InputGUI
        {
            get { return (InputGUI)m_InputGUI; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputTradeSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsEventFound
        {
            get
            {
                //20090104 FI Ne pas ecrire en 1 seule ligne car si  EventInput.IsEventFound = false, EventInput.SQLProduct peut être null
                bool ret = EventInput.IsEventFilled;
                if (ret)
                    ret = SessionTools.License.IsLicProductAuthorised(EventInput.SQLProduct.Identifier);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// ****** Don't touch this property. Use by Full FpML Interface
        public new bool IsModeConsult
        {
            get { return Cst.Capture.IsModeConsult(InputGUI.CaptureMode); }
        }

        /// <summary>
        /// 
        /// </summary>
        // ****** Don't touch this property. Use by Full FpML Interface
        public new bool IsModeUpdatePostEvts
        {
            get { return Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override ICustomCaptureInfos Object
        {
            get { return m_CaptureGen.Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161123 [22629] Modify
        protected override string ScreenName
        {
            get
            {
                if (Cst.Capture.IsModeNewCapture(this.InputGUI.CaptureMode))
                {
                    // FI 20161123 [22629] Modify Utilisation de l'écran DailyClosingTradeEvent en création d'évènement 
                    InputGUI.CurrentIdScreen = "DailyClosingTradeEvent";
                }
                else
                {
                    Event currentEvent = EventInput.CurrentEvent;
                    if (EventCodeFunc.IsTrade(currentEvent.eventCode))
                        InputGUI.CurrentIdScreen = "RootTradeEvent";
                    else if (LevelEventFunc.IsDailyClosingTradeEvent(currentEvent.eventCode))
                        InputGUI.CurrentIdScreen = "DailyClosingTradeEvent";
                    else if (LevelEventFunc.IsAdministrationTradeEvent(currentEvent.eventCode, currentEvent.eventType))
                        InputGUI.CurrentIdScreen = "AdministrationTradeEvent";
                    else if (LevelEventFunc.IsCalculationTradeEvent(currentEvent.eventCode, currentEvent.eventType))
                    {
                        if (EventCodeFunc.IsCalculationPeriod(currentEvent.eventCode) &&
                            (EventTypeFunc.IsFixedRate(currentEvent.eventType) || EventTypeFunc.IsFloatingRate(currentEvent.eventType)))
                            InputGUI.CurrentIdScreen = "CalculationPeriodTradeEvent";
                        else
                        {
                            if (EventTypeFunc.IsFxRatePlus(currentEvent.eventType))
                                InputGUI.CurrentIdScreen = "CalculationFxTradeEvent";
                            else
                                InputGUI.CurrentIdScreen = "CalculationTradeEvent";
                        }
                    }
                    else if (LevelEventFunc.IsDescriptionTradeEvent(currentEvent.eventCode, currentEvent.eventType))
                    {
                        if (EventCodeFunc.IsTrigger(currentEvent.eventCode) || EventCodeFunc.IsBarrier(currentEvent.eventCode))
                            InputGUI.CurrentIdScreen = "DescriptionTriggerTradeEvent";
                        else
                            InputGUI.CurrentIdScreen = "DescriptionTradeEvent";
                    }
                    else if (LevelEventFunc.IsGroupTradeEvent(currentEvent.eventCode, currentEvent.eventType))
                    {
                        if ((EventCodeFunc.IsNominalStep(currentEvent.eventCode) || EventCodeFunc.IsNominalQuantityStep(currentEvent.eventCode)) &&
                            (EventTypeFunc.IsNominal(currentEvent.eventType) || EventTypeFunc.IsQuantity(currentEvent.eventType)))
                            InputGUI.CurrentIdScreen = "GroupNominalStepTradeEvent";
                        else
                            InputGUI.CurrentIdScreen = "GroupTradeEvent";
                    }
                    else
                        InputGUI.CurrentIdScreen = "StandardEvent";
                }


                return InputGUI.CurrentIdScreen;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string SubTitleLeft
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160804 [Migration TFS]
        protected override string XML_FilesPath
        {
            //get { return @"XML_Files\CustomEvent"; }
            get { return @"CCIML\CustomEvent"; }
        }
        #endregion Accessor

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20161123 [22629] Modify (Gestiton de la saisie d'un évènement)
        protected override void OnConsult(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            Control ctrlPlaceHolder = FindControl(NamePlaceHolder);
            Cst.Capture.MenuConsultEnum captureMenuConsult = (Cst.Capture.MenuConsultEnum)System.Enum.Parse(typeof(Cst.Capture.MenuConsultEnum), e.CommandName);
            switch (captureMenuConsult)
            {
                case Cst.Capture.MenuConsultEnum.SetTemplate:
                case Cst.Capture.MenuConsultEnum.GoUpdate:
                case Cst.Capture.MenuConsultEnum.LoadEvent:
                case Cst.Capture.MenuConsultEnum.SetScreen:
                case Cst.Capture.MenuConsultEnum.GoEvent:
                    if (null != ctrlPlaceHolder)
                    {
                        // FI 20161123 [22629] 
                        //Chargement de l'évènement lorsque click sur création (SetTemplate == captureMenuConsult)
                        bool isLoadEvent = (Cst.Capture.MenuConsultEnum.LoadEvent == captureMenuConsult) ||
                                           (Cst.Capture.MenuConsultEnum.GoUpdate == captureMenuConsult) ||
                                           (Cst.Capture.MenuConsultEnum.SetTemplate == captureMenuConsult);

                        bool isControlSuccessful = true;
                        string msgControl = null;
                        
                        ClearPlaceHolder();
                        
                        if (isLoadEvent)
                            LoadEvent();
                        else if (captureMenuConsult == Cst.Capture.MenuConsultEnum.GoEvent)
                        {
                            // FI 20161124 [22634]  Call LoadCurrentEventStatus (Il faut charger les status de l'évènement car non présens dans  Input.DataSetEvent)
                            this.m_CaptureGen.Input.LoadCurrentEventStatus(SessionTools.CS);
                        }

                        #region Lock Control
                        if (EventInput.IsEventFilled)
                        {
                            if (Cst.Capture.MenuConsultEnum.GoUpdate == captureMenuConsult)
                            {
                                Lock lckExisting = LockTools.SearchLock(SessionTools.CS, GetLockTrade());
                                isControlSuccessful = (null == lckExisting);
                                if (false == isControlSuccessful)
                                    msgControl = lckExisting.ToString();
                            }
                        }
                        #endregion Lock Control
                        
                        #region ActionTuning Control
                        if (EventInput.IsTradeFound && isControlSuccessful)
                        {
                            if ((Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode) ||
                                (Cst.Capture.IsModeUpdate(InputGUI.CaptureMode))))
                            {
                                // EG 20160308 Migration vs2013
                                // #warning Action Tuning EG
                            }
                        }
                        #endregion ActionTuning Control

                        if (isControlSuccessful)
                        {
                            #region Display
                            DisplayEvent();
                            #endregion Display

                            SavePlaceHolder();
                            // FI 20200518 [XXXXX] Utilisation de DataCache
                            //Session[InputSessionID] = EventInput;
                            DataCache.SetData<EventInput>(InputSessionID, EventInput);
                        }
                        else
                        {
                            if (StrFunc.IsFilled(msgControl))
                                MsgForAlertImmediate = msgControl;

                            //Contrôle invalide --> Annulation de l'action en cours
                            OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                        }
                    }
                    break;
                case Cst.Capture.MenuConsultEnum.FirstEvent:
                case Cst.Capture.MenuConsultEnum.PreviousEvent:
                case Cst.Capture.MenuConsultEnum.NextEvent:
                case Cst.Capture.MenuConsultEnum.EndEvent:
                    Search(captureMenuConsult);
                    OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.GoEvent.ToString()));
                    EventIdentifier = EventInput.CurrentEventIdentifier;
                    break;
            }
            //
            if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                CtrlTrade.Focus();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200903 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            m_ParentGUID = Request.QueryString["GUID"];

            // FI 20161124 [22634] Attention code hyper dangereux , je laisse pour l'instant
            // tradeCommonInput est issu d'une variable session ...
            // Si l'utilisateur change de trade (Modification du contrôle Text tradeIdentifier) , TradeCommonInput pointe toujours sur un trade différent
            // FI 20200518 [XXXXX] Utilisation de DataCache
            TradeCommonInput tradeCommonInput = DataCache.GetData<TradeCommonInput>(ParentInputTradeSessionID);
            m_CaptureGen = new EventCaptureGen(tradeCommonInput);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {

            if (false == IsPostBack)
            {
                string idMenu = Request.QueryString["IDMenu"];
                if (StrFunc.IsEmpty(idMenu))
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputEvent);

                // Request.ServerVariables["APPL_PHYSICAL_PATH"]
                m_InputGUI = new EventInputGUI(idMenu, SessionTools.User, XML_FilesPath);
                m_InputGUI.InitializeFromMenu(CSTools.SetCacheOn(SessionTools.CS));
            }
            else
            {
                // FI 20200518 [XXXX] Utilisation de DataCache
                //m_InputGUI = Session[InputGUISessionID] as EventInputGUI;
                //m_CaptureGen.Input = Session[InputSessionID] as EventInput;
                //m_CaptureGen.TradeCommonInput = Session[InputTradeSessionID] as TradeCommonInput;
                m_InputGUI = DataCache.GetData<EventInputGUI>(InputGUISessionID);
                m_CaptureGen.Input = DataCache.GetData<EventInput>(InputSessionID);
                m_CaptureGen.TradeCommonInput = DataCache.GetData<TradeCommonInput>(ParentInputTradeSessionID);
            }

            // FI 20201022 [XXXXX] add test (null != m_CaptureGen.TradeCommonInput) car cette form peut être ouverte de manière indépendate (Il faut dans ce cas saisir le trade pour visualiser l'évènement) 
            if ((null != m_CaptureGen.TradeCommonInput) && m_CaptureGen.TradeCommonInput.Product.IsADM)
                m_InputGUI.MainMenuClassName = "invoicing";


            PageConstruction();
            PageTitle = Ressource.GetString(m_InputGUI.IdMenu);

            if (false == IsPostBack)
            {
                #region Call By Viewer Event
                try
                {
                    if (StrFunc.IsFilled(Request.QueryString["FKV"]))
                    {
                        string idT = Request.QueryString["FKV"];
                        TradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(SessionTools.CS, Convert.ToInt32(idT));
                    }
                    else if (StrFunc.IsFilled(Request.QueryString["FKVIDENTIFIER"]))
                    {
                        TradeIdentifier = Request.QueryString["FKVIDENTIFIER"];
                    }
                    if (StrFunc.IsFilled(Request.QueryString["PKV"]))
                    {
                        string idE = Request.QueryString["PKV"];
                        EventIdentifier = idE;
                    }
                    InputGUI.CaptureMode = Cst.Capture.ModeEnum.Consult;
                }
                catch { };
                #endregion Call By Viewer menu

                if (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode))
                    OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.SetTemplate.ToString()));
                else if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                {
                    if ((StrFunc.IsFilled(TradeIdentifier) && ("0" != TradeIdentifier)) ||
                       ((StrFunc.IsFilled(EventIdentifier) && ("0" != EventIdentifier))))
                        OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadEvent.ToString()));
                }
            }
            else
            {
                RestorePlaceHolder(); // Restore ( +  mise à jour du Document Fpml en mode Full)
                SavePlaceHolder();
            }


            base.OnLoad(e);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMode(object sender, skmMenu.MenuItemClickEventArgs e)
        {

            InputGUI.CaptureMode = (Cst.Capture.ModeEnum)System.Enum.Parse(typeof(Cst.Capture.ModeEnum), e.CommandName);
            // EG 20150923 Unused
            //if (null != sender)
            //    InputGUI.CaptureModeImage = GetMenuImageUrl(((skmMenu.Menu)sender).Items, e);
            //                
            skmMenu.MenuItemClickEventArgs argOnConsult = null;
            if (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode))
            {
                if (Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                    argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.SetTemplate.ToString(), e.Argument);
                else
                    argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadEvent.ToString(), e.Argument);
            }
            else if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadEvent.ToString(), e.Argument);
            else if (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode))
                argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.GoUpdate.ToString(), e.Argument);

            if (null != argOnConsult)
                OnConsult(null, argOnConsult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20161124 [22634] Modify
        protected override void OnPreRender(EventArgs e)
        {
            // FI 20161124 [22634] call DisplayHeader
            m_EventHeaderBanner.DisplayHeader(true, true);

            string eventTarget = Request.Params["__EVENTTARGET"];
            if (IsPostBack)
            {
                if (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType)
                {
                    if (null != EventInput.CustomCaptureInfos && (Cst.Capture.IsModeInput(m_InputGUI.CaptureMode)))
                        EventInput.CustomCaptureInfos.UpdCaptureAndDisplay(this);
                    //
                    //Bouton Add Or Delete Item
                    if ("OnAddOrDeleteItem" == eventTarget)
                        OnAddOrDeleteItem();
                }

                if (Cst.OTCml_ScreenBox == eventTarget)
                    OnZoomScreenBox();

            }

            // FI 20200518 [XXXXX] Utilisation DataCache
            //Session[InputGUISessionID] = m_InputGUI;
            //Session[InputSessionID] = EventInput;
            DataCache.SetData(InputSessionID, EventInput);
            DataCache.SetData(InputGUISessionID, m_InputGUI);
            
            base.OnPreRender(e);
            //
            // A faire absolument apres base.OnPreRender 
            #region PageSubTitle
            string subTitle = TitleLeft;
            if (StrFunc.IsFilled(SubTitleLeft))
                subTitle += " - " + SubTitleLeft;
            if ("0" != EventIdentifier)
                subTitle += " - " + EventIdentifier;
            PageTitle = subTitle;
            #endregion PageSubTitle

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20161114 [RATP]
        // EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
        protected override void OnValidate(object sender, CommandEventArgs e)
        {
            bool isValidateOk = (null == e.CommandArgument) || Convert.ToString(e.CommandArgument) == "TRUE";

            if (isValidateOk)
            {
                Control ctrlContainer = FindControl("tblMenu");
                if (null != ctrlContainer)
                {
                    Control ctrl = ctrlContainer.FindControl("mnuMode");
                    Cst.Capture.MenuValidateEnum captureMenuValidateEnum =
                        (Cst.Capture.MenuValidateEnum)System.Enum.Parse(typeof(Cst.Capture.MenuValidateEnum), e.CommandName);

                    switch (captureMenuValidateEnum)
                    {
                        case Cst.Capture.MenuValidateEnum.Close:
                            ClosePage();
                            break;
                        case Cst.Capture.MenuValidateEnum.Record:
                            if (Cst.Capture.IsModeMatch(m_InputGUI.CaptureMode))
                            {
                                OnValidateMatch(sender, e);
                            }
                            else
                            {
                                Page.Validate();
                                if (Page.IsValid)
                                {
                                    EventInput.CustomCaptureInfos.SaveCapture(Page);
                                    EventCaptureGen.ErrorLevel lRet = SaveEvent();
                                    //
                                    if (EventCaptureGen.ErrorLevel.SUCCESS == lRet)
                                    {

                                        OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                                        // FI 20161123 [22629] Mise en commentaire (pour faire comme la saisie des trades...)
                                        //JavaScript.SubmitOpenerOnly(this, "btnRefresh", null);
                                    }
                                    else
                                    {
                                        // En cas d'erreur
                                        // En mode creation ou duplication, on efface pas l'écran (= On conserve les données avant enregistrement) 
                                        // En mode Modification, on efface pas l'écran sauf si Pb de ROWVERSION
                                        if ((false == Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode)) && (lRet == EventCaptureGen.ErrorLevel.ROWVERSION_ERROR))
                                            OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                                    }
                                }
                            }

                            break;

                        case Cst.Capture.MenuValidateEnum.Annul:
                            #region Cancelling CaptureEvent
                            if (null != ctrl)
                            {
                                string eArgs;
                                // 20161123 [22629] Add IsModeNew
                                if (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeDuplicate(InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                                    eArgs = Cst.Capture.ModeEnum.Consult.ToString();
                                else
                                    eArgs = Cst.Capture.ModeEnum.New.ToString();
                                //
                                OnMode(ctrl, new skmMenu.MenuItemClickEventArgs(eArgs));
                            }
                            #endregion Cancelling CaptureEvent
                            break;
                    }
                }
            }
        }

        #endregion Events

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// EG 20160119 Refactoring Footer
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void AddToolBarFooter()
        {
            Panel pnlFooter = new Panel() { ID = "divfooter", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };
            pnlFooter.Controls.Add(GetButtonFooter_POST());
            CellForm.Controls.Add(pnlFooter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void AddToolBarConsult(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "divtrade", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };
            FpMLTextBox txtTrade = new FpMLTextBox(null, "0", 300, "Trade", false, "txtTrade", null, new Validator(),
                new Validator(EFSRegex.TypeRegex.RegexString, "N° trade", true));
            txtTrade.Attributes.Add("onblur", "OnTradeChanged('tblMenu_txtTrade');");
            txtTrade.Attributes.Add("onkeydown", "PostBackOnKeyEnter(event,'tblMenu$mnu" + Cst.Capture.MenuEnum.Consult.ToString() + "','"
                + Cst.Capture.MenuConsultEnum.LoadEvent.ToString() + "');");

            HtmlInputHidden hihTradeIdT = new HtmlInputHidden
            {
                ID = "hihTradeIdT"
            };
            pnl.Controls.Add(txtTrade);
            pnl.Controls.Add(hihTradeIdT);

            FpMLTextBox txtEvent = new FpMLTextBox(null, string.Empty, 100, "Event", false, "txtEvent", null, new Validator(),
                new Validator(EFSRegex.TypeRegex.RegexString, "N° event", true));
            txtEvent.Attributes.Add("onblur", "OnEventChanged('tblMenu_txtEvent');");
            txtEvent.Attributes.Add("onkeydown", "PostBackOnKeyEnter(event,'tblMenu$mnu" + Cst.Capture.MenuEnum.Consult.ToString() + "','"
                + Cst.Capture.MenuConsultEnum.LoadEvent.ToString() + "');");

            HtmlInputHidden hihEventId = new HtmlInputHidden
            {
                ID = "hihEventId"
            };
            pnl.Controls.Add(txtEvent);
            pnl.Controls.Add(hihEventId);

            pPnlParent.Controls.Add(pnl);

            base.AddToolBarConsult(pPnlParent);

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20210222 [XXXXX] Suppression JavaScript.ConfirmInputEvent (présent dans Trade.js)
        // EG 20210224 [XXXXX] Minification Trade.js
        protected override void CreateChildControls()
        {
            //JavaScript.ConfirmInputEvent(this);
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/Trade.min.js"));
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180425 Analyse du code Correction [CA2235]
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayEvent()
        {
            PlaceHolder plh = new PlaceHolder();
            bool isAddPlaceHolder = true;
            Control ctrlPlaceHolder = FindControl(NamePlaceHolder);

            #region  Display Global
            if (FindControl("tblMenu$txtTrade") is TextBox txtTradeId)
                txtTradeId.Enabled = (false == Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode));
            if (FindControl("tblMenu$txtEvent") is TextBox txtEventId)
                txtEventId.Enabled = (false == Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode));
            // EG 20160119 Refactoring Footer
            Control ctrlContainerFooter = FindControl("divfooter");
            if (null != ctrlContainerFooter)
                ctrlContainerFooter.Visible = IsEventFound;
            #endregion  Display Global

            if (IsEventFound)
            {
                //
                switch (InputGUI.CaptureType)
                {
                    case Cst.Capture.TypeEnum.Customised:
                        EventInput.CustomCaptureInfos = new EventCustomCaptureInfos(SessionTools.CS, m_CaptureGen.Input, SessionTools.User, SessionTools.SessionID, Cst.Capture.IsModeNew(InputGUI.CaptureMode));
                        EventInput.CustomCaptureInfos.InitializeCciContainer();
                        //
                        bool existCustomScreen = LoadScreen();
                        isAddPlaceHolder = (false == existCustomScreen);
                        if (existCustomScreen)
                        {
                            EventInput.CustomCaptureInfos.LoadDocument(InputGUI.CaptureMode, (CciPageBase)this);
                        }
                        else
                            InputGUI.CurrentIdScreen = string.Empty;
                        break;

                    default:
                        break;
                }
            }
            else
            {
                #region TradeIdentifier or EventId unknown
                string msg = Ressource.GetString2("Msg_EventConsult");
                string css = EFSCssClass.Msg_Information;
                bool isUnknown = true;
                //
                if (IsModeConsult)
                {
                    //Aucun message si "0" ou Empty et load d'un trade nUNDEFINEDon template (on est ici sur le switch en mode Consult)
                    isUnknown = StrFunc.IsFilled(TradeIdentifier) && ("0" != TradeIdentifier);
                }
                //	
                if (isUnknown)
                {
                    //
                    msg = Ressource.GetString2("Msg_EventNotFound", TradeIdentifier);
                    css = EFSCssClass.Msg_Alert;
                    //
                    if ((null != EventInput.SQLProduct) &&
                        (false == SessionTools.License.IsLicProductAuthorised(EventInput.SQLProduct.Identifier)))
                    {
                        css = EFSCssClass.Msg_Alert;
                        msg = Ressource.GetString("Msg_LicProduct");
                    }
                }
                //
                Label lbl = new Label
                {
                    Text = msg,
                    CssClass = css
                };
                plh.Controls.AddAt(0, lbl);
                #endregion TradeIdentifier or EventId unknown
            }
            //
            if (isAddPlaceHolder)
                ctrlPlaceHolder.Controls.Add(plh);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        /// FI 20161124 [22634] Modify
        protected override string GetDisplayKey(DisplayKeyEnum pKey)
        {
            string ret;

            switch (pKey)
            {
                case DisplayKeyEnum.DisplayKey_Event:
                    // FI 20161124 [22634] nouvelle alimentation de DisplayKey_Event
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                    {
                        ret = Ressource.GetString(ScreenName);
                        ret += "&nbsp;" + "&nbsp;";
                        ret += StrFunc.AppendFormat("({0}/{1})", m_CaptureGen.Input.CurrentEventIndex + 1, m_CaptureGen.Input.DataSetEvent.DtEvent.Rows.Count);
                    }
                    else
                    {
                        ret = Ressource.GetString("Characteristics");
                    }
                    break;
                default:
                    ret = string.Empty;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void InitFromProductInstrument()
        {
            if (EventInput.IsEventFilled)
            {
                m_InputGUI.IdP = EventInput.SQLProduct.Id;
                m_InputGUI.IdI = EventInput.SQLInstrument.Id;
                m_InputGUI.ProductCss = AspTools.GetCssColorDefault(EventInput.SQLInstrument);
                m_InputGUI.CssColor = m_InputGUI.ProductCss;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        protected override void InitializeCustomObjetInstanceObj(CustomObject pCo, XmlNode pNode)
        {
            base.InitializeCustomObjetInstanceObj(pCo, pNode);
            //
            if (EventCustomCaptureInfos.CCst.Prefix_eventDet == pCo.ClientId)
            {
                XmlNode nodeObject = GetNodeObject(pNode.Name);
                bool isOk = true;
                ArrayList aNodeToRemove = new ArrayList();
                while (isOk)
                {
                    if (nodeObject.HasChildNodes)
                    {
                        foreach (XmlNode nodeChild in nodeObject.ChildNodes)
                        {
                            if (ScreenNodeTypeEnum.objet == GetNodeType(nodeChild.Name, out XmlNode xmlNode))
                            {
                                #region Reading
                                //XmlNode nodeChildObject = GetNodeObject(nodeChild.Name);
                                // FI 20121204 [18224] la méthode GetNodeType retourne désormais le node Object
                                XmlNode nodeChildObject = xmlNode;

                                if (nodeChildObject.HasChildNodes && (ScreenNodeTypeEnum.table == GetNodeType(nodeChildObject.FirstChild.Name)))
                                {
                                    #region Recherche du type d'EVENTDET à conserver en fonction de EventCode et EventType
                                    XmlNode nodeTable = nodeChildObject.FirstChild;
                                    string clientId = XMLTools.GetNodeAttribute(nodeChild, "clientid");
                                    bool isRemovable = false;
                                    string eventCode = EventInput.CurrentEvent.eventCode;
                                    string eventType = EventInput.CurrentEvent.eventType;
                                    string family = EventInput.CurrentEvent.family;
                                    string fungibilityMode = EventInput.CurrentEvent.fungibilityMode;

                                    string eventCodeParent = string.Empty;
                                    string eventTypeParent = string.Empty;

                                    //20090527 PL Add Try/Catch au cas où...
                                    try
                                    {
                                        if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_currencyPair.ToString())
                                        {
                                            #region CurrencyPair details
                                            isRemovable = (Cst.ProductFamily_FX != family) ||
                                                (false == LevelEventFunc.IsEventDet_CurrencyPair(eventCode, eventType));
                                            #endregion CurrencyPair details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_dayCountFraction.ToString())
                                        {
                                            #region DayCountFraction details
                                            isRemovable = (false == LevelEventFunc.IsEventDet_DayCountFraction(eventCode, eventType));
                                            #endregion DayCountFraction details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_capfloorSchedule.ToString())
                                        {
                                            #region CapFloorSchedule details
                                            isRemovable = (false == LevelEventFunc.IsEventDet_CapFloorSchedule(eventCode, eventType));
                                            #endregion CapFloorSchedule details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_exchangeRate.ToString())
                                        {
                                            #region ExchangeRate details
                                            isRemovable = (Cst.ProductFamily_FX != family) ||
                                                (false == LevelEventFunc.IsEventDet_ExchangeRate(eventCode, eventType));
                                            #endregion ExchangeRate details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_strikePrice.ToString())
                                        {
                                            #region StrikePrice details
                                            isRemovable = (Cst.ProductFamily_FX != family) ||
                                                (false == LevelEventFunc.IsEventDet_StrikePrice(eventCode, eventType));
                                            #endregion StrikePrice details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_exchangeRatePremium.ToString())
                                        {
                                            #region ExchangeRatePremium details
                                            bool premiumQuoteSpecified = EventInput.CurrentEvent.detailsSpecified &&
                                                                         EventInput.CurrentEvent.details.premiumQuoteBasisSpecified;
                                            isRemovable = (Cst.ProductFamily_FX != family) || (premiumQuoteSpecified) ||
                                                (false == LevelEventFunc.IsEventDet_ExchangeRatePremium(eventCode, eventType));
                                            #endregion ExchangeRatePremium details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_fixedRate.ToString())
                                        {
                                            #region FixedRate details
                                            isRemovable = true;
                                            #endregion FixedRate details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_fixingRate.ToString())
                                        {
                                            #region FixingRate details
                                            isRemovable = (false == LevelEventFunc.IsEventDet_FixingRate(eventCode, eventType));
                                            #endregion FixingRate details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_paymentQuote.ToString())
                                        {
                                            #region PaymentQuote details
                                            isRemovable = true;
                                            #endregion PaymentQuote details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_premiumQuote.ToString())
                                        {
                                            #region PremiumQuote details
                                            isRemovable = (Cst.ProductFamily_FX != family) ||
                                                (false == LevelEventFunc.IsEventDet_PremiumQuote(eventCode, eventType));
                                            #endregion PremiumQuote details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_settlementRate.ToString())
                                        {
                                            #region SettlementRate details
                                            isRemovable = (Cst.ProductFamily_FX != family) ||
                                                (false == LevelEventFunc.IsEventDet_SettlementRate(eventCode, eventType));
                                            #endregion SettlementRate details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_sideRate.ToString())
                                        {
                                            #region SideRate details
                                            isRemovable = (Cst.ProductFamily_FX != family) ||
                                                (false == LevelEventFunc.IsEventDet_SideRate(eventCode, eventType));
                                            #endregion SideRate details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_triggerRate.ToString())
                                        {
                                            #region TriggerRate details
                                            isRemovable = (false == LevelEventFunc.IsEventDet_TriggerRate(eventCode));
                                            #endregion TriggerRate details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_notes.ToString())
                                        {
                                            #region Notes details
                                            isRemovable = LevelEventFunc.IsEventDet_MarkToMarket(eventType) || LevelEventFunc.IsEventDet_Fungible(eventType);
                                            #endregion Notes details
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_pricingFx.ToString())
                                        {
                                            #region Pricing (for MarkToMarket)
                                            isRemovable = (false == LevelEventFunc.IsEventDet_MarkToMarket(eventType));
                                            if (false == isRemovable)
                                            {
                                                Event eventParent = EventInput.GetEventGrandParent;
                                                //20090527 PL Add if() suuite à un bug lié à des MTM rattaché à TRD/DAT
                                                if (eventParent != null)
                                                    isRemovable = (false == EventCodeFunc.IsFxForward(eventParent.eventCode));
                                            }
                                            #endregion Pricing (for MarkToMarket)
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_pricingFxOption.ToString())
                                        {
                                            #region Pricing (for MarkToMarket)
                                            isRemovable = (false == LevelEventFunc.IsEventDet_MarkToMarket(eventType));
                                            if (false == isRemovable)
                                            {
                                                Event eventParent = EventInput.GetEventGrandParent;
                                                //20090527 PL Add if() suuite à un bug lié à des MTM rattaché à TRD/DAT
                                                if (eventParent != null)
                                                    isRemovable = (false == EventCodeFunc.IsFxOption(eventParent.eventCode));
                                            }
                                            #endregion Pricing (for MarkToMarket)
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_pricingIRD.ToString())
                                        {
                                            #region Pricing (for MarkToMarket)
                                            isRemovable = (false == LevelEventFunc.IsEventDet_MarkToMarket(eventType));
                                            if (false == isRemovable)
                                                isRemovable = (Cst.ProductFamily_IRD != family);
                                            #endregion Pricing (for MarkToMarket)
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventChild.ToString())
                                        {
                                            #region Children
                                            if (LevelEventFunc.IsResetTradeEvent(eventCode))
                                                isRemovable = (null == EventInput.CurrentEventChilds);
                                            else
                                                isRemovable = true;
                                            #endregion Children
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventAsset.ToString())
                                        {
                                            #region Asset 
                                            isRemovable = (false == EventInput.CurrentEvent.assetSpecified);
                                            #endregion Asset 
                                        }
                                        else if (clientId == EventCustomCaptureInfos.CCst.Prefix_eventDet_Closing.ToString())
                                        {
                                            #region EventDET
                                            isRemovable = (FungibilityModeEnum.NONE.ToString() == fungibilityMode) ||
                                                (false == LevelEventFunc.IsEventDet_Fungible(eventType));
                                            #endregion EventDET
                                        }
                                    }
                                    catch
                                    {
                                        isRemovable = false;
                                    }
                                    if (isRemovable)
                                        aNodeToRemove.Add(nodeChild);
                                    #endregion Recherche cohérence Type EventDet compatible
                                }
                                #endregion Reading
                                isOk = false;
                            }
                            else if (ScreenNodeTypeEnum.table == GetNodeType(nodeChild.Name))
                            {
                                nodeObject = nodeChild;
                                break;
                            }
                            else
                            {
                                isOk = false;
                                break;
                            }
                        }
                    }
                    if (0 < aNodeToRemove.Count)
                    {
                        for (int i = 0; i < aNodeToRemove.Count; i++)
                        {
                            nodeObject.RemoveChild((XmlNode)aNodeToRemove[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
        protected override int InitializeNodeObjectArray(CustomObject pCo, XmlNode pNode, string pParentClientId, int pParentOccurs)
        {
            bool isArrayManaged = (EventCustomCaptureInfos.CCst.Prefix_eventClass == pCo.ClientId) && ArrFunc.IsFilled(EventInput.CurrentEvent.eventClass);
            isArrayManaged = isArrayManaged || (EventCustomCaptureInfos.CCst.Prefix_eventAsset == pCo.ClientId) && ArrFunc.IsFilled(EventInput.CurrentEvent.asset);
            isArrayManaged = isArrayManaged || (EventCustomCaptureInfos.CCst.Prefix_eventProcess == pCo.ClientId) && ArrFunc.IsFilled(EventInput.CurrentEvent.process);
            isArrayManaged = isArrayManaged || (EventCustomCaptureInfos.CCst.Prefix_eventChild == pCo.ClientId) && ArrFunc.IsFilled(EventInput.CurrentEventChilds);
            //
            int screenOccurs = int.Parse(XMLTools.GetNodeAttribute(pNode, "occurs"));
            int occurs = screenOccurs;
            //		
            if (isArrayManaged)
            {
                int docOccurs = 0;
                if (EventCustomCaptureInfos.CCst.Prefix_eventClass == pCo.ClientId)
                    docOccurs = ArrFunc.Count(EventInput.CurrentEvent.eventClass);
                else if (EventCustomCaptureInfos.CCst.Prefix_eventProcess == pCo.ClientId)
                    docOccurs = ArrFunc.Count(EventInput.CurrentEvent.process);
                else if (EventCustomCaptureInfos.CCst.Prefix_eventAsset == pCo.ClientId)
                    docOccurs = ArrFunc.Count(EventInput.CurrentEvent.asset);
                else if (EventCustomCaptureInfos.CCst.Prefix_eventChild == pCo.ClientId)
                    docOccurs = ArrFunc.Count(EventInput.CurrentEventChilds);

                if (-1 != docOccurs)
                {
                    occurs = System.Math.Max(docOccurs, occurs);
                    //
                    if ("OnAddOrDeleteItem" == Request.Params["__EVENTTARGET"])
                    {
                        if ((null != m_AddRemoveNode) && (m_AddRemoveNode.Name == pNode.Name))
                        {
                            string prefix = pParentClientId;
                            //
                            if (pParentOccurs > 0)
                                prefix += pParentOccurs.ToString();
                            //
                            prefix += (StrFunc.IsFilled(prefix) ? CustomObject.KEY_SEPARATOR.ToString() : string.Empty) + pCo.ClientId;
                            //
                            if (m_AddRemovePrefix.EndsWith(prefix))
                            {
                                if (m_AddRemoveOperatorType == Cst.OperatorType.add)
                                    occurs++;
                                else if (m_AddRemoveOperatorType == Cst.OperatorType.substract)
                                {
                                    if (occurs > 1 && occurs > screenOccurs)
                                    {
                                        occurs = System.Math.Max(1, occurs - 1);
                                        /*
                                        if (m_CaptureGen.Ccis.CciEvent.RemoveLastItemInArray(pCo.ClientId, occurs, pParentClientId, pParentOccurs))
                                            occurs = System.Math.Max(1, occurs - 1);
                                        else
                                            JavaScript.AlertImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_ItemNotEmpty"), false);
                                        */
                                    }
                                    else
                                        //JavaScript.AlertImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_LastItem"), false);
                                        JavaScript.DialogImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_LastItem"), false);
                                }
                            }
                        }
                    }
                }
                //
                //if (IsControlModeConsult(pCo.ClientId))
                if (IsControlModeConsult(pCo))
                    XMLTools.SetNodeAttribute(pNode, "addsuboperator", "false");
                return occurs;
            }
            //
            return occurs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161123 [22629] Modify
        private void LoadEvent()
        {
            if (StrFunc.IsFilled(EventIdentifier))
            {
                // Chargement des évènements du trade, l'évènement courant (EventInput.CurrentEvent) est l'évènement  IdE= EventIdentifier
                m_CaptureGen.LoadEvent(SessionTools.CS, TradeIdentifier, EventIdentifier, SessionTools.User, SessionTools.SessionID);

                // FI 20161123 [22629] ajour d'un évènement
                if (Cst.Capture.IsModeNew(InputGUI.CaptureMode) || Cst.Capture.IsModeDuplicate(InputGUI.CaptureMode))
                    AddNewEvent(Cst.Capture.IsModeNew(InputGUI.CaptureMode));
            }
            else if (StrFunc.IsFilled(TradeIdentifier))
            {
                // Chargement des évènements du trade, l'évènement courant (EventInput.CurrentEvent) est le 1er évènement du trade (TRD,DAT)
                m_CaptureGen.LoadEvents(SessionTools.CS, TradeIdentifier, SessionTools.User, SessionTools.SessionID);
            }

            if (EventInput.IsEventFilled)
            {
                InitFromProductInstrument();
                TradeIdT = EventInput.IdT;
                TradeIdentifier = EventInput.TradeIdentifier;
                EventIdentifier = EventInput.CurrentEventIdentifier;
                EventId = Convert.ToInt32(EventInput.CurrentEventIdentifier);
            }
        }
        
        
        
        /// <summary>
        /// Ajout d'un nouvel évènement 
        /// </summary>
        /// <param name="pIsChildEvent">
        /// <para>si true: => Nouvel évènement enfant de l'évènement courrant (pre-proposition avec CLO/MTM)</para>
        /// <para>si true: => Nouvel évènement frère l'évènement courrant avec caractéristiques identiques</para>
        /// </param>
        /// FI 20161123 [22629] Add method
        // EG 20180425 Analyse du code Correction [CA2235]
        private void AddNewEvent(Boolean pIsChildEvent)
        {
            EventInput input = m_CaptureGen.Input;

            DateTime dtsys = OTCmlHelper.GetDateSys(SessionTools.CS, out DateTime dtsysUTC);
            string sDate = DtFunc.DateTimeToString(dtsys, DtFunc.FmtISODate);

            Event currentEvent = input.CurrentEvent;

            input.CloneCurrentEvent();
            Event newEvent = input.CurrentEventClone;

            newEvent.idE = -1; //Nouvel élément
            if (pIsChildEvent)
                newEvent.idEParent = currentEvent.idE;

            newEvent.sourceSpecified = true;
            newEvent.source = "Manual Input";

            newEvent.idAStActivation = SessionTools.User.IdA;
            newEvent.idStActivation = Cst.STATUSREGULAR;
            newEvent.dtStActivationSpecified = true;
            // FI 20200820 [25468] dates systemes en UTC
            newEvent.dtStActivation = new EFS_Date
            {
                DateTimeValue = dtsysUTC
            };

            newEvent.idStCalculSpecified = true;
            newEvent.idStCalcul = "CALC";

            if (pIsChildEvent)
            {
                string eventCode = EventCodeEnum.CLO.ToString();
                string eventType = EventTypeEnum.MTM.ToString();
                string eventclass = EventClassEnum.FRP.ToString();

                /* Définition des codes en fonction des familles d'instrument */
                switch (this.EventInput.SQLProduct.Family)
                {
                    case Cst.ProductFamily_IRD:
                        break;
                    case Cst.ProductFamily_FX:
                        break;
                }

                newEvent.eventCode = eventCode;
                newEvent.eventType = eventType;
                newEvent.dtStartPeriodSpecified = true;
                newEvent.dtStartPeriod = new EFS_Date(sDate);
                newEvent.dtEndPeriodSpecified = true;
                newEvent.dtEndPeriod = new EFS_Date(sDate);

                newEvent.streamNo = currentEvent.streamNo;
                newEvent.instrumentNo = currentEvent.instrumentNo;

                //Payer
                newEvent.idPayer = currentEvent.idPayer;
                newEvent.idPayerBook = currentEvent.idPayerBook;
                if (newEvent.idPayer == 0)
                {
                    TradeInput tradeInput = ((TradeInput)m_CaptureGen.TradeCommonInput);
                    IParty party = tradeInput.DataDocument.GetPartyBuyer();
                    newEvent.idPayer = (null != party) ? party.OTCmlId : 0;
                    newEvent.idPayerBook = 0;
                    if (newEvent.idPayer > 0)
                    {
                        int? bookId = tradeInput.DataDocument.GetOTCmlId_Book(party.Id);
                        newEvent.idPayerBook = bookId ?? 0;
                    }
                }
                newEvent.idPayerSpecified = (newEvent.idPayer > 0);
                newEvent.idPayerBookSpecified = (newEvent.idPayerBook > 0);

                //Rceiver
                newEvent.idReceiver = currentEvent.idReceiver;
                newEvent.idReceiver = currentEvent.idReceiverBook;
                if (newEvent.idReceiver == 0)
                {
                    TradeInput tradeInput = ((TradeInput)m_CaptureGen.TradeCommonInput);
                    IParty party = tradeInput.DataDocument.GetPartySeller();
                    newEvent.idReceiver = (null != party) ? party.OTCmlId : 0;
                    newEvent.idReceiverBook = 0;
                    if (newEvent.idReceiver > 0)
                    {
                        int? bookId = tradeInput.DataDocument.GetOTCmlId_Book(party.Id);
                        newEvent.idReceiverBook = bookId ?? 0;
                    }
                }
                newEvent.idReceiverSpecified = (newEvent.idReceiver > 0);
                newEvent.idReceiverBookSpecified = (newEvent.idReceiverBook > 0);


                newEvent.eventClassSpecified = true;
                newEvent.eventClass = new EventClass[] { new EventClass() };
                newEvent.eventClass[0].codeSpecified = true;
                newEvent.eventClass[0].code = eventclass;
                newEvent.eventClass[0].dtEvent = new EFS_Date(sDate);
                newEvent.eventClass[0].dtEventSpecified = true;
                newEvent.eventClass[0].idE = newEvent.idE;
            }

            newEvent.siSpecified = false;
            newEvent.si = new EventSi[] { };

            newEvent.processSpecified = false;
            newEvent.process = new EventProcess[] { };

            newEvent.pricing2Specified = false;
            newEvent.pricing2 = new EventPricing2[] { };

            newEvent.pricingSpecified = false;
            newEvent.pricing = new EventPricing { };

            newEvent.detailsSpecified = false;
            newEvent.details = new EventDetails();

            newEvent.asset = new EventAsset[] { };
            newEvent.assetSpecified = false;

            // add new event in  input.EventDocReader.@event
            input.EventDocReader.@event = ((from item in input.EventDocReader.@event
                                            select item).Union(new Event[] { newEvent })).ToArray();

            input.CurrentEventIndex = ArrFunc.Count(input.EventDocReader.@event) - 1;

            input.CurrentEventStatus = new EventStatus();
            input.CurrentEventStatus.Initialize(SessionTools.CS, -1);

            input.CustomCaptureInfos = new EventCustomCaptureInfos(SessionTools.CS, input, SessionTools.User, SessionTools.SessionID, true);
            input.CustomCaptureInfos.InitializeCciContainer();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200903 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override void PageConstruction()
        {
            
            HtmlPageTitle titleLeft = new HtmlPageTitle(TitleLeft, SubTitleLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle(TitleRight);
            this.PageTitle = TitleLeft;
            
            GenerateHtmlForm();
            // FI 20201022 [XXXXX] add test (null != m_CaptureGen.TradeCommonInput) car cette form peut être ouverte de manière indépendate (Il faut dans ce cas saisir le trade pour visualiser l'évènement)
            string mainMenuName = ((null != m_CaptureGen.TradeCommonInput) && m_CaptureGen.TradeCommonInput.Product.IsADM) ? "invoicing" : "input";
            FormTools.AddBanniere(this, Form, titleLeft, titleRight, InputGUI.IdMenu, mainMenuName);
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, InputGUI.IdMenu);
            // FI 20201022 [XXXXX] add test (null != m_CaptureGen.TradeCommonInput) car cette form peut être ouverte de manière indépendate (Il faut dans ce cas saisir le trade pour visualiser l'évènement)
            if ((null != m_CaptureGen.TradeCommonInput) && m_CaptureGen.TradeCommonInput.Product.IsADM)
                PageTools.SetHeaderLinkIcon(this, IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin));
            
            //Binding des ToolBars
            ToolBarBinding();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161124 [22634]
        protected override void AddHeader()
        {
            
            m_EventHeaderBanner = new EventHeaderBanner(this, GUID, CellForm,
                                        m_CaptureGen.Input,
                                        m_InputGUI,
                                        (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)));
            m_EventHeaderBanner.AddControls();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
        protected override void RefreshButtonsValidate()
        {
            #region Header
            string tmpSubTitleLeft = string.Empty;
            Control ctrlContainer = FindControl("tblHeader");
            WebControl ctrl;
            if (null != ctrlContainer)
            {
                ctrl = ctrlContainer.FindControl("titleLeft") as WebControl;
                if (null != ctrl)
                    ((TableCell)ctrl).Text = TitleLeft;

                ctrl = ctrlContainer.FindControl("subtitleLeft") as WebControl;
                if (null != ctrl)
                {
                    tmpSubTitleLeft = SubTitleLeft;
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && (StrFunc.IsEmpty(EventIdentifier)))
                        tmpSubTitleLeft = string.Empty;
                    ((TableCell)ctrl).Text = tmpSubTitleLeft;
                }
                //
                ctrl = ctrlContainer.FindControl("titleRight") as WebControl;
                // EG 20151217 GUI Reduce Tag
                if (null != ctrl)
                {
                    if (ctrl is TableCell cell)
                        cell.Text = TitleRight;
                    else if (ctrl is Label label)
                        label.Text = TitleRight;
                }
            }
            #endregion Header
            //
            //20060216 PL Add next row for Title
            //Ne fonctionne pas ??? le tag title ne semble pas encore créer, à suivre...
            PageTitle = TitleLeft + (StrFunc.IsFilled(tmpSubTitleLeft) ? " - " + tmpSubTitleLeft : string.Empty);
            //
            #region Update Menu Buttons Record and Cancel
            string msgConfirm = string.Empty;
            ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                #region Record Button
                ctrl = ctrlContainer.FindControl("btnRecord") as WebControl;
                if (null != ctrl)
                {
                    ctrl.Attributes["href"] = "#";
                    string tmpPrefix = (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode) ? "Msg_Record" : "Msg_Modify");
                    if (StrFunc.IsFilled(msgConfirm))
                        msgConfirm += Cst.CrLf + Cst.CrLf;
                    msgConfirm += Ressource.GetString(tmpPrefix + "Event");
                    //
                    //alimentation attribute onclick
                    string data = (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode) ? string.Empty : this.EventIdentifier + this.EventIdentifierCode);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"EnableValidators(); ");
                    sb.Append(@"if (typeof(Page_ClientValidate) == 'function')");
                    sb.AppendFormat("if (Page_ClientValidate()) return ConfirmInputEvent('{0}','{1}', {2}, '{3}','{4}'); DisableValidators();", this.TitleLeft, "tblMenu$btnRecord", JavaScript.HTMLString(msgConfirm), this.TradeIdentifier, data);
                    ctrl.Attributes.Add("onclick", sb.ToString());

                }
                #endregion Record Button

                #region Cancel Button
                ctrl = FindControl("btnCancel") as WebControl;
                if (null != ctrl)
                {
                    ctrl.Attributes["href"] = "#";
                    string data = string.Empty;
                    msgConfirm = Ressource.GetStringForJS("Msg_AbortData");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat(@"DisableValidators();return ConfirmInputEvent('{0}','{1}', '{2}', '{3}');", this.TitleLeft, "tblMenu$btnCancel", msgConfirm, data);
                    ctrl.Attributes.Add("onclick", sb.ToString());
                }
                #endregion Cancel Button

            }
            #endregion Update Menu Buttons Record and Cancel
            //

        }

        /// <summary>
        /// Retourne le contenu de la toolbar Mode (création, consultation, Duplication, Modification)
        /// </summary>
        /// <returns></returns>
        /// FI 20161124 [22634] Modify
        /// FI 20170621 [XXXXX] Modify
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuMode()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(4);

            mnu[0] = GetMenuCreation();
            // FI 20161124 [22634] Voir explication sur (m_CaptureGen.TradeCommonInput.Identifier) == TradeIdentifier dans le OnInit
            mnu[0].Enabled = mnu[0].Enabled && (null != m_CaptureGen.TradeCommonInput) && (m_CaptureGen.TradeCommonInput.Identifier) == TradeIdentifier;

            mnu[1] = GetMenuConsult();
            mnu[2] = GetMenuDuplicate();
            mnu[3] = GetMenuModify();

            // FI 20170621 [XXXXX] Alimentation de MnuModeModify
            MnuModeModify = mnu[3];

            return mnu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuConsult()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(5);
            mnu[0] = new skmMenu.MenuItemParent(0)
            {
                aID = "btnLoad",
                eImageUrl = "far fa-icon fa-play-circle",
                aToolTip = Ressource.GetString("btnLoad"),
                eCommandName = Cst.Capture.MenuConsultEnum.LoadEvent.ToString()
            };
            mnu[1] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-double-left",
                aToolTip = Ressource.GetString("btnFirst"),
                eCommandName = Cst.Capture.MenuConsultEnum.FirstEvent.ToString()
            };
            mnu[2] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-left",
                aToolTip = Ressource.GetString("btnPrevious"),
                eCommandName = Cst.Capture.MenuConsultEnum.PreviousEvent.ToString()
            };
            mnu[3] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-right",
                aToolTip = Ressource.GetString("btnNext"),
                eCommandName = Cst.Capture.MenuConsultEnum.NextEvent.ToString()
            };
            mnu[4] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-double-right",
                aToolTip = Ressource.GetString("btnLast"),
                eCommandName = Cst.Capture.MenuConsultEnum.EndEvent.ToString()
            };
            return mnu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        private EventCaptureGen.ErrorLevel SaveEvent()
        {
            EventCaptureGen.ErrorLevel lRet = EventCaptureGen.ErrorLevel.SUCCESS;
            ErrLevelForAlertImmediate = ProcessStateTools.StatusNoneEnum;
            int idE = EventId;
            EventCaptureGenException errExc = null;

            try
            {
                CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo()
                {
                    user = SessionTools.User,
                    session = SessionTools.AppSession,
                    licence = SessionTools.License
                };

                m_CaptureGen.SaveCurrentEvent(SessionTools.CS, InputGUI.CaptureMode, captureSessionInfo, ref idE);
            }
            catch (EventCaptureGenException ex)
            {
                //Erreur reconnue
                errExc = ex;
                lRet = errExc.ErrLevel;
            }
            catch (Exception) { throw; }//Error non gérée

            string msg;
            switch (lRet)
            {
                case EventCaptureGen.ErrorLevel.SUCCESS:
                    if (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode))
                        msg = Ressource.GetString("Msg_ConfirmEventSaved");
                    else
                        msg = Ressource.GetString("Msg_ConfirmEventCreated");
                    break;
                case EventCaptureGen.ErrorLevel.ROWVERSION_ERROR:
                    msg = Ressource.GetString(DataHelper.GetSQLErrorMessage(SQLErrorEnum.Concurrency));
                    break;
                default:
                    msg = lRet.ToString();
                    break;
            }
            //
            #region Set Return Msg
            if (EventCaptureGen.ErrorLevel.SUCCESS != lRet)
            {
                // Msg enrichi si autre Que BUG => Car Ds ce cas le message est trop long et sans intérêt pour le user 
                // Le tracker est de toute façon alimenté ds ce cas
                if ((false == errExc.IsInnerException) && StrFunc.IsFilled(errExc.Message))
                    msg += Cst.CrLf + Cst.CrLf + errExc.Message;
                msg = Ressource.GetString("Msg_ConfirmEventSavedError") + Cst.CrLf + Cst.CrLf + msg;
            }
            //
            if (StrFunc.IsFilled(msg))
            {
                if (lRet == EventCaptureGen.ErrorLevel.LOCKPROCESS_ERROR)
                    msg = msg.Replace(lRet.ToString(), Ressource.GetString("Msg_ProcessLockError"));
                msg = msg.Replace("{0}", TradeIdentifier);
                msg = msg.Replace("{1}", EventIdentifier + EventIdentifierCode);
                MsgForAlertImmediate = msg;
                if (EventCaptureGen.ErrorLevel.SUCCESS != lRet)
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
            }
            #endregion Set Return Msg
            //
            #region Reload eventCaptureGen  => Reload de l'instance eventCaptureGen pour eviter tout déphasage avec la database
            if (EventCaptureGen.ErrorLevel.SUCCESS == lRet)
            {
                // 20161123 [22629] Alimentation EventIdentifier avec l'évènement qui d'être inséré
                EventIdentifier = idE.ToString();
                m_CaptureGen.LoadEvent(SessionTools.CS, TradeIdentifier, EventIdentifier, SessionTools.User, SessionTools.SessionID);
            }
            #endregion Reload eventCaptureGen
           

            return lRet;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void SetToolBarsStyle()
        {
            Control ctrlContainer = FindControl("tblMenu");
            WebControl ctrl;

            bool isVisible;
            if (null != ctrlContainer)
            {
                #region Validate
                #region BtnRecord && BtnCancel
                ctrl = ctrlContainer.FindControl("BtnRecord") as WebControl;
                if (null != ctrl)
                {
                    isVisible = (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode)) ||
                                (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode));
                    bool isRecordVisible = isVisible && IsEventFound;
                    if (isRecordVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";

                    if (isRecordVisible)
                        ctrl.Enabled = SessionTools.License.IsLicProductAuthorised_Add(EventInput.SQLProduct.Identifier);
                    HiddenFieldDeFaultControlOnEnter.Value = string.Empty;
                    if (isRecordVisible)
                        HiddenFieldDeFaultControlOnEnter.Value = ctrl.ClientID;

                }
                //
                ctrl = ctrlContainer.FindControl("BtnCancel") as WebControl;
                if (null != ctrl)
                {
                    isVisible = (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode)) ||
                                    (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode));
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion BtnRecord && BtnCancel
                #endregion Validate

                #region Template
                ctrl = ctrlContainer.FindControl("tdTbrTemplate") as WebControl;
                if (null != ctrl)
                {
                    isVisible = false;
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ((TableCell)ctrl).Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion Template

                #region Consult
                ctrl = ctrlContainer.FindControl("divtrade") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";

                }
                ctrl = CtrlTrade as WebControl;
                if (null != ctrl)
                    ((FpMLTextBox)ctrl).IsLocked = Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);



                ctrl = ctrlContainer.FindControl("divevent") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                ctrl = CtrlEvent as WebControl;
                if (null != ctrl)
                    ((FpMLTextBox)ctrl).IsLocked = Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);


                ctrl = ctrlContainer.FindControl("tbrConsult") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion Consult

                #region Mode
                ctrl = ctrlContainer.FindControl("btnCreation") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                //
                ctrl = ctrlContainer.FindControl("btnConsult") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                //
                ctrl = ctrlContainer.FindControl("btnModification") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && IsPlaceHolderLoaded && IsEventFound;
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                //
                ctrl = ctrlContainer.FindControl("btnDuplication") as WebControl;
                if (null != ctrl)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && IsPlaceHolderLoaded && IsEventFound;
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion Mode

                #region Screen
                ctrl = ctrlContainer.FindControl("tdTbrScreen") as WebControl;
                if (null != ctrl)
                {
                    isVisible = false;
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion Screen

                #region Action
                ctrl = ctrlContainer.FindControl("tdtbrAction") as WebControl;
                if (null != ctrl)
                {
                    isVisible = false;
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion Action

                #region ResultAction
                ctrl = ctrlContainer.FindControl("tdtbrResultAction") as WebControl;
                if (null != ctrl)
                {
                    ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                #endregion Screen
            }

            #region tblMainDescriptionAndStatus
            isVisible = (IsPlaceHolderLoaded);
            ctrl = FindControl("tblMainDescription") as WebControl;
            if (null != ctrl)
            {
                if (isVisible)
                    ControlsTools.RemoveStyleDisplay(ctrl);
                else
                    ctrl.Style[HtmlTextWriterStyle.Display] = "none";
            }
            #endregion tblMainDescriptionAndStatus

            #region ValidationSummary
            ctrl = FindControl("ValidationSummary") as WebControl;
            if (null != ctrl)
                ((ValidationSummary)ctrl).ShowSummary = false;
            #endregion ValidationSummary


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void AddToolBarReport(Panel pPnlParent)
        {
            //pas de Toolbar report
        }

        /// <summary>
        ///  Recherche l'évènement selon {pCaptureMenuConsult}  et affecte CurrentEventIndex 
        /// </summary>
        /// <returns></returns>
        public void Search(Cst.Capture.MenuConsultEnum pCaptureMenuConsult)
        {
            EventInput input = m_CaptureGen.Input;

            if ((null != input.EventDocReader) && (null != input.EventDocReader.@event) && (0 < input.EventDocReader.@event.Length))
            {
                switch (pCaptureMenuConsult)
                {
                    case Cst.Capture.MenuConsultEnum.FirstEvent:
                        input.CurrentEventIndex = 0;
                        break;
                    case Cst.Capture.MenuConsultEnum.EndEvent:
                        input.CurrentEventIndex = input.EventDocReader.@event.Length - 1;
                        break;
                    case Cst.Capture.MenuConsultEnum.PreviousEvent:
                        input.CurrentEventIndex = Math.Max(0, input.CurrentEventIndex - 1);
                        break;
                    case Cst.Capture.MenuConsultEnum.NextEvent:
                        input.CurrentEventIndex = Math.Min(input.EventDocReader.@event.Length - 1, input.CurrentEventIndex + 1);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion Methods
    }
}

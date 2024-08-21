#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.Enum.Tools;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion Using Directives
namespace EFS.Spheres
{

    // EG 20190419 [EventHistoric Settings] DtHisto
    public partial class TradeCommonEventsPage : PageBase
    {
        #region Members

        protected PanelEvent pnlEvents;
        protected string xmlRefEvent;
        protected string xmlRefEventAsset;
        protected string xmlRefEventDet;
        protected string xmlRefEventClass;
        protected string xmlRefEventPricing;
        protected string xmlRefEventFee;
        protected ExtendEnum m_EventCodeEnum;
        protected ExtendEnum m_EventTypeEnum;
        protected string m_ParentGUID;
        protected int m_IdE;
        protected Nullable<DateTime> m_DtHisto;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected EventGrid DtgEvents
        {
            get { return pnlEvents.dgEvent; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsModeMainEvents
        {
            get { return (false == IsModeSubEvents); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsModeSubEvents
        {
            get { return (0 < m_IdE); }
        }

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

        // EG 20170419 [23019] New
        public bool IsHiddenDeactivEvent
        {
            get
            {
                CheckBox chkHiddenDeactivEvent = (CheckBox)FindControl(Cst.CHK + "HIDDENDEACTIVEVENT");
                return (null != chkHiddenDeactivEvent) && chkHiddenDeactivEvent.Checked;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsWithComplementaryInfo
        {
            get
            {
                CheckBox chkComplementaryInfo = (CheckBox)FindControl(Cst.CHK + "COMPLEMENTARYINFO");
                return (null != chkComplementaryInfo) && chkComplementaryInfo.Checked;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsWithoutDailyClosingLevel
        {
            get
            {
                CheckBox chkDailyClosingLevel = (CheckBox)FindControl(Cst.CHK + "DAILYCLOSING");
                return (null != chkDailyClosingLevel) && (false == chkDailyClosingLevel.Checked);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsWithOnlyDailyClosingAccountingLevel
        {
            get
            {
                CheckBox chkDailyClosingLevel = (CheckBox)FindControl(Cst.CHK + "DAILYCLOSINGACCOUNTING");
                return (null != chkDailyClosingLevel) && (true == chkDailyClosingLevel.Checked);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsWithoutCalculationLevel
        {
            get
            {
                CheckBox chkCalculationLevel = (CheckBox)FindControl(Cst.CHK + "CALCULATION");
                return (null != chkCalculationLevel) && (false == chkCalculationLevel.Checked);
            }
        }

        // EG 20190419 [EventHistoric Settings] New
        public bool IsHistoRestriction
        {
            get
            {
                return IsModeSubEvents && (m_DtHisto.HasValue);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonInput TradeCommonInput
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return null; }
        }

        /// <summary>
        /// Obtient true si les variables session sont disponibles
        /// <remarks>Lorsque les variables session sont non disponibles cette page s'autoclose</remarks>
        /// </summary>
        protected bool IsSessionVariableAvailable
        {

            get { return (null != TradeCommonInput); }
        }


        #endregion Accessors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            //ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
            m_EventCodeEnum = extendEnums["EventCode"];
            m_EventTypeEnum = extendEnums["EventType"];
            */

            m_EventCodeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventCode");
            m_EventTypeEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, "EventType");

            //
            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            if (StrFunc.IsFilled(Request.QueryString["IdE"]))
                m_IdE = Convert.ToInt32(Request.QueryString["IdE"]);
            //
            PageConstruction();
            //
            if (false == IsSessionVariableAvailable)
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
        //EG 20120613 BlockUI New
        // EG 20230222 [26270][WI588] Correction Double Refresh sur Postback via bouton "btnRefresh"
        protected void OnLoad(object sender, System.EventArgs e)
        {
            if (IsSessionVariableAvailable)
            {
                if (false == IsPostBack)
                {
                    OnRefreshClick(sender, e);
                }
                else if (IsPostBack)
                {
                    CheckBox chkDailyClosingLevel = (CheckBox)FindControl(Cst.CHK + "DAILYCLOSING");
                    CheckBox chkDailyClosingAccountingLevel = (CheckBox)FindControl(Cst.CHK + "DAILYCLOSINGACCOUNTING");
                    if ((null != chkDailyClosingLevel) && (null != chkDailyClosingAccountingLevel))
                    {
                        chkDailyClosingAccountingLevel.Enabled = chkDailyClosingLevel.Checked;
                        if (false == chkDailyClosingAccountingLevel.Enabled)
                            chkDailyClosingAccountingLevel.Checked = false;
                    }
                }
                // EG 20130725 Timeout sur Block
                JQuery.Block block = new JQuery.Block((PageBase)this.Page)
                {
                    Timeout = SystemSettings.GetTimeoutJQueryBlock(TradeCommonInputGUI.IdMenu)
                };
                JQuery.UI.WriteInitialisationScripts(this, block);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRefreshClick(object sender, EventArgs e)
        {
            DatagridEvents();
            BindData();
            DtgEvents.RowExpand.ExpandAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (IsSessionVariableAvailable)
                TradeCommonHeaderBanner.DisplayHeader(true, true);
            //
            base.Render(writer);
        }
        #endregion Events

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddPanelCheckBox(Panel pPnlContainer)
        {
            if (IsModeMainEvents)
            {
                Panel pnlMainEvents = new Panel() { ID = "divtbevt2" };
                #region DailyClosing level CheckBox
                Panel pnlDailyClosing = new Panel();
                CheckBox chkDailyClosingLevel = new CheckBox
                {
                    ID = Cst.CHK + "DAILYCLOSING",
                    Text = Ressource.GetString("WithDailyClosingLevel"),
                    Checked = false,
                    Enabled = true,
                    EnableViewState = true,
                    AutoPostBack = true
                };
                pnlDailyClosing.Controls.Add(chkDailyClosingLevel);

                chkDailyClosingLevel = new CheckBox
                {
                    ID = Cst.CHK + "DAILYCLOSINGACCOUNTING",
                    Text = Ressource.GetString("WithDailyClosingAccountingLevel"),
                    Checked = false,
                    Enabled = false,
                    EnableViewState = true,
                    AutoPostBack = true
                };
                pnlDailyClosing.Controls.Add(chkDailyClosingLevel);

                pnlMainEvents.Controls.Add(pnlDailyClosing);
                #endregion DailyClosing level CheckBox

                #region Calculation level CheckBox
                Panel pnlCalculationLevel = new Panel();
                CheckBox chkCalculationLevel = new CheckBox
                {
                    ID = Cst.CHK + "CALCULATION",
                    Text = Ressource.GetString("WithCalculationLevel"),
                    Checked = false,
                    EnableViewState = true,
                    AutoPostBack = true
                };
                pnlCalculationLevel.Controls.Add(chkCalculationLevel);
                pnlMainEvents.Controls.Add(pnlCalculationLevel);
                #endregion Calculation level CheckBox

                pPnlContainer.Controls.Add(pnlMainEvents);
            }

            #region Complementary information CheckBox
            Panel pnlComplementary = new Panel() { ID = "divtbevt3" };
            CheckBox chkComplementaryInfo = new CheckBox
            {
                ID = Cst.CHK + "COMPLEMENTARYINFO",
                Text = Ressource.GetString("WithComplementaryInfo"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = true
            };
            pnlComplementary.Controls.Add(chkComplementaryInfo);
            #endregion Complementary information CheckBox

            pnlComplementary.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            #region Deactiv Event CheckBox
            CheckBox chkHiddenDeactivEvent = new CheckBox
            {
                ID = Cst.CHK + "HIDDENDEACTIVEVENT",
                Text = Ressource.GetString("HiddenDeactivEvent"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = true
            };
            pnlComplementary.Controls.Add(chkHiddenDeactivEvent);
            #endregion Deactiv Event CheckBox

            pPnlContainer.Controls.Add(pnlComplementary);
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20190419 [EventHistoric Settings] Upd
        // EG 20190611 Upd (Bouton Font Awesome) Refresh, Print, InfoLegend
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + TradeCommonInputGUI.MainMenuClassName };
            pnlToolBar.Controls.Add(AddPanelButton());
            AddPanelCheckBox(pnlToolBar);
            pnlToolBar.Controls.Add(AddPanelHistoric());
            pnlToolBar.Controls.Add(new Panel() { ID = "divtbevt4" });

            CellForm.Controls.Add(pnlToolBar);
        }

        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Panel AddPanelButton()
        {
            Panel pnl = new Panel() { ID = "divtbevt1" }; 
            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Click += new EventHandler(OnRefreshClick);
            pnl.Controls.Add(btnRefresh);

            WCToolTipLinkButton btnExpandCollapse = ControlsTools.GetAwesomeButtonExpandCollapse(false);
            btnExpandCollapse.OnClientClick = "javascript:HierarchicalGrid_ExpandCollapseAll(this);return false;";
            pnl.Controls.Add(btnExpandCollapse);

            pnl.Controls.Add(ControlsTools.GetToolTipLinkButtonPrint("divbody"));

            WCToolTipLinkButton btnLegend = ControlsTools.GetToolTipLinkButtonInfo("btnLegend");
            btnLegend.Attributes.Add("onclick", "OpenCodeEvent();return false;");
            pnl.Controls.Add(btnLegend);
            return pnl;
        }
        #region AddPanelHistoric
        // EG 20190419 [EventHistoric Settings] New
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Panel AddPanelHistoric()
        {

            string hisToricValue = SessionTools.EventHistoric;
            //WCTogglePanel pnlHistoric = new WCTogglePanel() { ID = "divtbevt5" };
            //pnlHistoric.SetHeaderTitle(Ressource.GetString("EventHistoric"));

            Panel pnlCheckValue = new Panel() { ID = "divtbevt5" };
            Label lbl = new Label
            {
                Text = Ressource.GetString("EventHistoric")
            };
            pnlCheckValue.Controls.Add(lbl);
            pnlCheckValue.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            RadioButton chkToday = new RadioButton()
            {
                ID = "chkHisto0D",
                Text = Ressource.GetString("EventToDay"),
                GroupName = "historic",
                AutoPostBack = true
            };
            chkToday.Checked = ("0D" == hisToricValue);

            pnlCheckValue.Controls.Add(chkToday);

            RadioButton chk1d = new RadioButton()
            {
                ID = "chkHisto1D",
                Text = Ressource.GetString("Event1D"),
                GroupName = "historic",
                AutoPostBack = true
            };
            chk1d.Checked = ("1D" == hisToricValue);
            pnlCheckValue.Controls.Add(chk1d);

            RadioButton chk7d = new RadioButton()
            {
                Text = Ressource.GetString("Event7D"),
                ID = "chkHisto7D",
                GroupName = "historic",
                AutoPostBack = true
            };
            chk7d.Checked = ("7D" == hisToricValue);
            pnlCheckValue.Controls.Add(chk7d);

            RadioButton chk1m = new RadioButton()
            {
                Text = Ressource.GetString("Event1M"),
                ID = "chkHisto1M",
                GroupName = "historic",
                AutoPostBack = true
            };
            chk1m.Checked = ("1M" == hisToricValue);
            pnlCheckValue.Controls.Add(chk1m);

            RadioButton chk3m = new RadioButton()
            {
                Text = Ressource.GetString("Event3M"),
                ID = "chkHisto3M",
                GroupName = "historic",
                AutoPostBack = true
            };
            chk3m.Checked = ("3M" == hisToricValue);
            pnlCheckValue.Controls.Add(chk3m);

            RadioButton chkBeyond = new RadioButton()
            {
                Text = Ressource.GetString("EventBeyond"),
                ID = "chkHistoBeyond",
                GroupName = "historic",
                AutoPostBack = true
            };
            chkBeyond.Checked = ("Beyond" == hisToricValue);
            pnlCheckValue.Controls.Add(chkBeyond);
            //pnlHistoric.AddContent(pnlCheckValue);
            //return pnlHistoric;
            return pnlCheckValue;
        }
        #endregion AddPanelHistoric

        /// <summary>
        /// 
        /// </summary>
        protected void BindData()
        {
            DtgEvents.DataSource = (DataView)Cache["DtEvent"];
            DtgEvents.DataMember = "Event";
            DtgEvents.RelationName = "Event";
            DtgEvents.DataBind();
            DtgEvents.RowExpand[0] = true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            JavaScript.OpenEventSi(this, m_ParentGUID);
            JavaScript.OpenCodeEvent(this, m_ParentGUID);
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = TradeCommonInputGUI.MainMenuClassName };

            #region Header banner
            InitHeaderBanner(pnlBody);
            TradeCommonHeaderBanner.ResetIdentifierDisplaynameDescriptionExtlLink();
            #endregion Header banner

            PlaceHolder ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "phEvents"
            };
            pnlEvents = new PanelEvent(TradeCommonInput.Product.Product)
            {
                ID = "divDtg",
                CssClass = TradeCommonInputGUI.MainMenuClassName
            };
            ph.Controls.Add(pnlEvents);
            pnlBody.Controls.Add(ph);
            CellForm.Controls.Add(pnlBody);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190419 [EventHistoric Settings] Upd
        protected void DatagridEvents()
        {
            // EG 20170419 [23019] New
            CookieData[] cookiedata = new CookieData[6];
            string controlValue;

            #region DailyClosingLevel
            CheckBox chkDailyClosingLevel = (CheckBox)FindControl(Cst.CHK + "DAILYCLOSING");
            if (null != chkDailyClosingLevel)
            {
                if (false == IsPostBack)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEvents, chkDailyClosingLevel.ID, out controlValue);
                    chkDailyClosingLevel.Checked = (controlValue == "1");
                }
                bool isDailyClosingLevel = (chkDailyClosingLevel.Checked);

                if (IsPostBack)
                    cookiedata[0] = new CookieData(Cst.SQLCookieGrpElement.TradeEvents, chkDailyClosingLevel.ID, (isDailyClosingLevel ? "1" : "0"));
            }

            #endregion DailyClosingLevel
            #region DailyClosingAccountingLevel
            CheckBox chkDailyClosingAccountingLevel = (CheckBox)FindControl(Cst.CHK + "DAILYCLOSINGACCOUNTING");
            if (null != chkDailyClosingAccountingLevel)
            {
                if (null != chkDailyClosingLevel)
                    chkDailyClosingAccountingLevel.Enabled = chkDailyClosingLevel.Checked;
                if (false == IsPostBack)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEvents, chkDailyClosingAccountingLevel.ID, out controlValue);
                    chkDailyClosingAccountingLevel.Checked = (controlValue == "1");
                }
                bool isDailyClosingAccountingLevel = (chkDailyClosingAccountingLevel.Checked);
                //
                if (IsPostBack)
                    cookiedata[1] = new CookieData(Cst.SQLCookieGrpElement.TradeEvents, chkDailyClosingAccountingLevel.ID, (isDailyClosingAccountingLevel ? "1" : "0"));
            }

            #endregion DailyClosingAccountingLevel
            #region ComplementaryInfo
            CheckBox chkComplementaryInfo = (CheckBox)FindControl(Cst.CHK + "COMPLEMENTARYINFO");
            if (null != chkComplementaryInfo)
            {
                if (false == IsPostBack)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEvents, chkComplementaryInfo.ID, out controlValue);
                    chkComplementaryInfo.Checked = (controlValue == "1");
                }
                bool isComplementaryInfo = (chkComplementaryInfo.Checked);
                if (IsPostBack)
                    cookiedata[2] = new CookieData(Cst.SQLCookieGrpElement.TradeEvents, chkComplementaryInfo.ID, (isComplementaryInfo ? "1" : "0"));
            }

            #endregion ComplementaryInfo
            #region CalculationLevel
            CheckBox chkCalculationLevel = (CheckBox)FindControl(Cst.CHK + "CALCULATION");
            if (null != chkCalculationLevel)
            {
                if (false == IsPostBack)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEvents, chkCalculationLevel.ID, out controlValue);
                    chkCalculationLevel.Checked = (controlValue == "1");
                }
                bool isCalculationLevel = (chkCalculationLevel.Checked);
                //
                if (IsPostBack)
                    cookiedata[3] = new CookieData(Cst.SQLCookieGrpElement.TradeEvents, chkCalculationLevel.ID, (isCalculationLevel ? "1" : "0"));
            }

            #endregion CalculationLevel
            #region Deactiv Event CheckBox
            CheckBox chkHiddenDeactivEvent = (CheckBox)FindControl(Cst.CHK + "HIDDENDEACTIVEVENT");
            if (null != chkHiddenDeactivEvent)
            {
                if (false == IsPostBack)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEvents, chkHiddenDeactivEvent.ID, out controlValue);
                    chkHiddenDeactivEvent.Checked = (controlValue == "1");
                }
                // EG 20170419 [23019] New
                bool isHiddenDeactivEvent = (chkHiddenDeactivEvent.Checked);
                if (IsPostBack)
                    cookiedata[4] = new CookieData(Cst.SQLCookieGrpElement.TradeEvents, chkHiddenDeactivEvent.ID, (isHiddenDeactivEvent ? "1" : "0"));
            }
            #endregion Deactiv Event CheckBox

            #region Historic
            SaveHistoValue();
            #endregion Historic


            DataSet dsEvents = GetDsSelectEvents();

            #region Table
            DataTable dtEvent = dsEvents.Tables[0];
            dtEvent.TableName = "Event";
            DataTable dtEventClass = dsEvents.Tables[1];
            dtEventClass.TableName = "EventClass";
            #endregion Table

            #region Relations Setting
            DataColumn dcIdE = dtEvent.Columns["IDE"];
            DataColumn dcIdE_Event = dtEvent.Columns["IDE_EVENT"];
            DataColumn dcIdE_EventClass = dtEventClass.Columns["IDE"];
            DataRelation relEvent = new DataRelation("Event", dcIdE, dcIdE_Event, false);
            DataRelation relEventClass = new DataRelation("EventClass", dcIdE, dcIdE_EventClass, false);
            relEventClass.ExtendedProperties.Add("Container", true);
            dsEvents.Relations.Add(relEventClass);
            if (IsWithComplementaryInfo)
            {
                DataTable dtEventAsset = dsEvents.Tables[2];
                dtEventAsset.TableName = "EventAsset";
                DataTable dtEventDet = dsEvents.Tables[3];
                dtEventDet.TableName = "EventDet";
                DataTable dtEventPricing = dsEvents.Tables[4];
                dtEventPricing.TableName = "EventPricing";
                DataTable dtEventFee = dsEvents.Tables[5];
                dtEventFee.TableName = "EventFee";
                DataRelation relEventAsset = new DataRelation("EventAsset", dcIdE, dtEventAsset.Columns["IDE"], false);
                DataRelation relEventDet = new DataRelation("EventDet", dcIdE, dtEventDet.Columns["IDE"], false);
                DataRelation relEventPricing = new DataRelation("EventPricing", dcIdE, dtEventPricing.Columns["IDE"], false);
                DataRelation relEventFee = new DataRelation("EventFee", dcIdE, dtEventFee.Columns["IDE"], false);
                relEventAsset.ExtendedProperties.Add("Container", true);
                relEventDet.ExtendedProperties.Add("Container", true);
                relEventPricing.ExtendedProperties.Add("Container", true);
                relEventFee.ExtendedProperties.Add("Container", true);
                dsEvents.Relations.Add(relEventAsset);
                dsEvents.Relations.Add(relEventDet);
                dsEvents.Relations.Add(relEventPricing);
                dsEvents.Relations.Add(relEventFee);
            }
            dsEvents.Relations.Add(relEvent);
            #endregion Relations Setting
            if (IsModeSubEvents)
                dtEvent.DefaultView.RowFilter = "IDE=" + m_IdE.ToString();
            else
                dtEvent.DefaultView.RowFilter = "EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.Trade);
            dtEvent.DefaultView.Sort = "IDE" + SQLCst.ASC;
            Cache["DtEvent"] = dtEvent.DefaultView;

            if (IsPostBack)
                AspTools.WriteSQLCookie(cookiedata);


        }
        // EG 20190419 [EventHistoric Settings] new
        /// <summary>
        /// Sauvegarde de la date historique de consultation des événements
        /// Ne concerne que les événement en mode ZOOM
        /// </summary>
        /// <param name="pCookieData"></param>
        // EG 20190419 [EventHistoric Settings] New
        // EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        private void SaveHistoValue()
        {
            bool isChecked = IsEventHistoChecked("0D");
            if (false == isChecked)
                isChecked = IsEventHistoChecked("1D");
            if (false == isChecked)
                isChecked = IsEventHistoChecked("7D");
            if (false == isChecked)
                isChecked = IsEventHistoChecked("1M");
            if (false == isChecked)
                isChecked = IsEventHistoChecked("3M");
            if (false == isChecked)
                _ = IsEventHistoChecked("Beyond");

            m_DtHisto = null;
            string histo = SessionTools.EventHistoric;
            if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
            {
                DtFuncML dtFuncML = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null);
                m_DtHisto = dtFuncML.StringToDateTime("BUSINESS-" + histo);
            }

        }
        private bool IsEventHistoChecked(string pId)
        {
            CheckBox chk = (CheckBox)FindControl("chkHisto" + pId);
            bool isChecked = (null != chk) && chk.Checked;
            if (isChecked)
                SessionTools.EventHistoric = pId;

            if (false == IsPostBack)
            {
                AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEvents, Cst.SQLCookieElement.EventHistoric.ToString(), out string controlValue);
                chk.Checked = (controlValue == pId);
            }
            return isChecked;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            if (IsSessionVariableAvailable)
            {
                AddToolBar();
                AddBody();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual string GetWhereCalculationLevel()
        {
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20170116 [21916] Modify
        // EG 20190419 [EventHistoric Settings] Upd
        // EG 20230222 [26270][WI588] Refactoring requête sur la consultation des événement d'un trade
        protected DataSet GetDsSelectEvents()
        {
            string CS = SessionTools.CS;
            DbSvrType svrType = DataHelper.GetDbSvrType(CS);

            SQLReferentialData.SQLSelectParameters sqlSelectParameters;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDT), TradeCommonInput.IdT);
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDE), m_IdE);
            if (IsModeMainEvents)
            {
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.EVENTCODE), EventCodeFunc.DailyClosing);
            }
            else if (IsHistoRestriction)
            {
                dp.Add(new DataParameter(CS, "DTHISTO", DbType.Date), m_DtHisto.Value.Date);// FI 20201006 [XXXXX] DbType.Date
            }

            StrBuilder sQuery = new StrBuilder();
            sQuery.AppendFormat(GetSQLSelectEvents() + SQLCst.SEPARATOR_MULTISELECT);

            string sqlWhere = "(ev.IDT = @IDT) and (@IDE = @IDE)";
            if (IsModeMainEvents && (svrType == DbSvrType.dbORA))
                sqlWhere += " and (@EVENTCODE = @EVENTCODE)";

            #region EventClass
            if (StrFunc.IsEmpty(xmlRefEventClass))
                xmlRefEventClass = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTCLASS");
            ReferentialTools.DeserializeXML_ForModeRO(xmlRefEventClass, out ReferentialsReferential refEventClass);
            if (null != refEventClass)
            {
                sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEventClass, sqlWhere);
                sQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false) + Cst.CrLf;
                sQuery += SQLCst.SEPARATOR_MULTISELECT;
            }
            #endregion EventClass

            if (IsWithComplementaryInfo)
            {
                #region EventAsset
                if (StrFunc.IsEmpty(xmlRefEventAsset))
                    xmlRefEventAsset = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTASSET");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEventAsset, out ReferentialsReferential refEventAsset);
                if (null != refEventAsset)
                {
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEventAsset, sqlWhere);
                    sQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false) + Cst.CrLf;
                    sQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EventAsset
                #region EventDet
                if (StrFunc.IsEmpty(xmlRefEventDet))
                {
                    xmlRefEventDet = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTDET");
                }
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEventDet, out ReferentialsReferential refEventDet);
                if (null != refEventDet)
                {
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEventDet, sqlWhere);
                    sQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false) + Cst.CrLf;
                    sQuery += SQLCst.SEPARATOR_MULTISELECT;
                }
                #endregion EventDet
                #region EventPricing
                if (StrFunc.IsEmpty(xmlRefEventPricing))
                    xmlRefEventPricing = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTPRICING");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEventPricing, out ReferentialsReferential refEventPricing);
                if (null != refEventPricing)
                {
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEventPricing, sqlWhere);
                    sQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false) + Cst.CrLf;
                    sQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EventPricing
                #region EventFee
                if (StrFunc.IsEmpty(xmlRefEventFee))
                    xmlRefEventFee = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTFEE");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEventFee, out ReferentialsReferential refEventEventFee);
                if (null != refEventEventFee)
                {
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEventEventFee, sqlWhere);
                    sQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).GetQueryReplaceParameters(false) + Cst.CrLf;
                    sQuery += SQLCst.SEPARATOR_MULTISELECT;
                }
                #endregion EventFee
            }

            QueryParameters qp = new QueryParameters(CS, sQuery.ToString(), dp);
            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, qp.Query, qp.Parameters.GetArrayDbParameter());
            ds.DataSetName = "Events";
            return ds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableCell"></param>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void InitHeaderBanner(Control pCtrlContainer)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        protected override void PageConstruction()
        {
            AbortRessource = true;

            string title = string.Empty;
            string idMenu = string.Empty;
            string cssMode = string.Empty;
            if (IsSessionVariableAvailable)
            {
                title = Ressource.GetString2("EventsOfTrade", TradeCommonInput.Identifier);
                idMenu = TradeCommonInputGUI.IdMenu;
                cssMode = TradeCommonInputGUI.CssMode;
            }
            HtmlPageTitle titleLeft = new HtmlPageTitle(title);
            PageTitle = title;

            GenerateHtmlForm();

            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), string.Empty, idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, cssMode, false, string.Empty, idMenu);
        }



        /// <summary>
        /// Requête avec récursivité
        ///  Lecture de la vue VW_ALLTRADEINSTRUMENT (TRADEINSTRUMENT union TRADE)  pour récupérer les IDI (1 à n) du trade
        ///  - TRADEINSTRUMENT est alimenté exclusivement pour les stratégies (donc INSTRUMENTNO > 1)
        ///  - Reste ensuite  à lire la table EVENT avec le résultat de la récursivité pour retourner le résultat.
        ///    Jointure avec la table EVENTGROUP pour constituer à travers elle l'ordre d'affichage des événements sur la
        ///    base des inforamtion FAMILY, EVENTCODE et/ou EVENTTYPE de chaque ligne d'événement.
        ///    de LPC/AMT ne sont pas visibles sur l'affichage principal des événements mais dans une nouvelle fenêtre par clique sur l'EVENTTYPE de l'événement)
        /// </summary>
        /// <returns></returns>
        // EG 20190419 [EventHistoric Settings] Upd
        // EG 20230222 [26270][WI588] Refactoring requête sur la consultation des événement d'un trade
        // EG 20220519 [WI637] Remplacement jointure VW_EVENT_EVENTGROUP par EVENT/TRADE sur Trades sources
        protected string GetSQLSelectEvents()
        {
            string recursivePredicate = (IsModeMainEvents ? "(ev.IDE_EVENT = @IDE)" : "(ev.IDE = @IDE)") + Cst.CrLf;
            string hidePredicate = string.Empty;

            string selectPredicate = string.Empty;
            // Application de prédicate en fonction des valeurs spécifiées sur l'interface de consultation des événments
            // - Evénements d'arrétés, événements de calcul, arrêtés comptables ... (OTC/F&O)
            if (IsModeMainEvents)
            {
                hidePredicate = " and (cte.HIDE = 0)";

                #region Display Or Not DailyClosing Level
                if (IsWithoutDailyClosingLevel)
                    selectPredicate += " and (ev.EVENTCODE <> @EVENTCODE)" + Cst.CrLf;
                else if (IsWithOnlyDailyClosingAccountingLevel)
                    selectPredicate += @" and (ev.EVENTCODE <> @EVENTCODE) or 
                    exists (select 1 from dbo.EVENTCLASS ec
                    where (ec.IDE = ev.IDE) and (ec.EVENTCLASS in (select EVENTCLASS from dbo.ACCOUNTING)))" + Cst.CrLf;
                else // RD 20150520 [21022] Ajouter la paramètre @EVENTCODE
                    selectPredicate += " and (@EVENTCODE = @EVENTCODE)" + Cst.CrLf;
                #endregion Display Or Not DailyClosing Level
                if (IsHiddenDeactivEvent)
                    selectPredicate += @" and ((ev.IDSTACTIVATION <> 'DEACTIV') or (ev.EVENTCODE = 'TRD'))";

                selectPredicate += Cst.CrLf;
            }
            // - 1er select TRADE RACINE (TRD/DAT pour la page principale ou l'événement cliqué pour les pages secondaires))
            // - 2ème select les enfants du TRADE RACINE
            // Select du résultat de la récursivité avec le tri adéquat calculé

            string sqlSelect = $@"with EVENT_CTE
            (IDT, IDE, IDE_EVENT, IDI, ZOOM, SORTSTREAM, SORTINSTRUMENT, SORTORDER, IDENTIFIER_TRADE,
            IDENTIFIER_INSTR, DISPLAYNAME_INSTR, IDP, IDENTIFIER_PRODUCT, FAMILY, GPRODUCT, FUNGIBILITYMODE, HIDE) as
            (
                select ev.IDT, ev.IDE, ev.IDE_EVENT, ev.IDI, ev.ZOOM, 
                ev.SORTSTREAM, ev.SORTINSTRUMENT, ev.SORTORDER, 
                ev.IDENTIFIER_TRADE, ev.IDENTIFIER_INSTR, ev.DISPLAYNAME_INSTR,
                ev.IDP, ev.IDENTIFIER_PRODUCT, ev.FAMILY, ev.GPRODUCT,ev.FUNGIBILITYMODE,
                0 as HIDE
                from dbo.VW_EVENT_EVENTGROUP ev
                where (ev.IDT = @IDT) and {recursivePredicate}

                union all

                select ev.IDT, ev.IDE, ev.IDE_EVENT, ev.IDI, ev.ZOOM,
                ev.SORTSTREAM, ev.SORTINSTRUMENT, ev.SORTORDER, 
                ev.IDENTIFIER_TRADE, ev.IDENTIFIER_INSTR, ev.DISPLAYNAME_INSTR,
                ev.IDP, ev.IDENTIFIER_PRODUCT, ev.FAMILY, ev.GPRODUCT,ev.FUNGIBILITYMODE,
                case when cte.ZOOM is not null then 1 when cte.HIDE = 1 then 1 else 0 end as HIDE 
                from dbo.VW_EVENT_EVENTGROUP ev
                inner join EVENT_CTE cte on (cte.IDE = ev.IDE_EVENT)
                where(ev.IDT = @IDT)
            )
            select ev.*, cte.IDI, cte.ZOOM, cte.SORTSTREAM, cte.SORTINSTRUMENT, cte.SORTORDER, 
            cte.IDENTIFIER_TRADE, cte.IDENTIFIER_INSTR, cte.DISPLAYNAME_INSTR, cte.IDP, cte.IDENTIFIER_PRODUCT, 
            cte.FAMILY, cte.GPRODUCT, cte.FUNGIBILITYMODE, cte.HIDE,
            --es.IDENTIFIER_TRADE as ES_IDENTIFIER_TRADE,
            trs.IDENTIFIER as ES_IDENTIFIER_TRADE,
            ac1.IDENTIFIER as AC1_IDENTIFIER, bk1.IDENTIFIER as BK1_IDENTIFIER,
            ac2.IDENTIFIER as AC2_IDENTIFIER, bk2.IDENTIFIER as BK2_IDENTIFIER
            from EVENT_CTE cte
            inner join dbo.EVENT ev on (ev.IDE = cte.IDE) 
            --left join dbo.VW_EVENT_EVENTGROUP es on (es.IDE = ev.IDE_SOURCE)
			left join dbo.EVENT es on (es.IDE = ev.IDE_SOURCE)
			left join dbo.TRADE trs on (trs.IDT = es.IDT)
            left join dbo.ACTOR ac1 on (ac1.IDA = ev.IDA_PAY)
            left join dbo.BOOK  bk1 on (bk1.IDB = ev.IDB_PAY)
            left join dbo.ACTOR ac2 on (ac2.IDA = ev.IDA_REC)
            left join dbo.BOOK  bk2 on (bk2.IDB = ev.IDB_REC)
            where (ev.IDT=@IDT) {hidePredicate} {selectPredicate}
            order by cte.SORTINSTRUMENT, cte.SORTSTREAM, ev.IDE_EVENT, cte.SORTORDER , ev.DTSTARTADJ, ev.DTENDADJ";

            return sqlSelect;
        }
        #endregion Methods

    }

}

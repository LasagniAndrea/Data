#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.TradeInformation;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
#endregion Using Directives
namespace EFS.Spheres
{
    public partial class EarPage : PageBase
    {
        #region Members

        private readonly bool isNewVersion = true;
        protected string xmlRefEar;
        protected string xmlRefEarDet;
        protected string xmlRefEarResult;
        protected string xmlRefEarClass;
        protected string xmlRefEarDay;
        protected string xmlRefEarCommon;
        protected string xmlRefEarNom;
        protected string xmlRefEarCalc;
        protected string xmlRefEarMoney;
        protected string xsltFile;
        protected string xsltFile_V211;
        private string m_ParentGUID;
        private TradeCommonInputGUI m_TradeCommonInputGUI;
        private TradeCommonInput m_TradeCommonInput;
        private XmlDocument m_DocEar;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected string EarResultSessionID
        {
            get
            {
                // FI 20200518 [XXXXX] Utilisation de BuildDataCacheKey
                return BuildDataCacheKey("EarResult");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string EarSessionID
        {
            get
            {
                // FI 20200518 [XXXXX] Utilisation de BuildDataCacheKey
                return BuildDataCacheKey("Ear");
            }
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

        /// <summary>
        /// 
        /// </summary>
        public TradeCommonInput TradeCommonInput
        {
            get { return m_TradeCommonInput; }
        }

        /// <summary>
        /// Obtient true si les variables session sont disponibles
        /// <remarks>Lorsque les variables session sont non disponibles cette page s'autoclose</remarks>
        /// </summary>
        protected bool IsSessionVariableAvailable
        {

            get { return (null != TradeCommonInput); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161114 [RATP] Add pour ne pas planter systématiquement dans la méthode MethodsGUI.IsModeConsult
        public bool IsModeConsult
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161114 [RATP] Add pour ne pas planter systématiquement dans la méthode MethodsGUI.IsModeConsult
        public bool IsScreenFullCapture
        {
            get { return false; }
        }


        #endregion Accessors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 202000518 [XXXXX] Utilisation de DataCache
            //m_TradeCommonInputGUI = (TradeCommonInputGUI)Session[InputGUISessionID];
            //m_TradeCommonInput = (TradeCommonInput)Session[InputSessionID];
            m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
            m_TradeCommonInput = DataCache.GetData<TradeCommonInput>(ParentInputSessionID);

            PageConstruction();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        protected void OnLoad(object sender, System.EventArgs e)
        {
            if (IsSessionVariableAvailable)
            {
                RadioButton butDailyEar = (RadioButton)FindControl(Cst.BUT + "DAILYEAR");
                RadioButton butAllEar = (RadioButton)FindControl(Cst.BUT + "ALLEAR");
                RadioButton butSelectEar = (RadioButton)FindControl(Cst.BUT + "SELECTEAR");
                TextBox txtDtStart = (TextBox)FindControl(Cst.TXT + "DTSTARTEAR");
                TextBox txtDtEnd = (TextBox)FindControl(Cst.TXT + "DTENDEAR");
                WebControl pnlEventDate = FindControl("divtbear3") as WebControl;
                bool isDailyEar = false;
                bool isAllEar = false;

                if (null != butDailyEar && null != butAllEar && null != butSelectEar)
                {
                    isDailyEar = (butDailyEar.Checked);
                    isAllEar = (butAllEar.Checked);
                    _ = (butSelectEar.Checked);
                }
                if (isDailyEar || isAllEar)
                    pnlEventDate.Style.Add(HtmlTextWriterStyle.Display, "none");
                else
                {
                    ControlsTools.RemoveStyleDisplay(pnlEventDate);
                    //				
                    if ((null != txtDtStart) && (null != txtDtEnd))
                    {
                        DateTime dtStart = new DtFunc().StringToDateTime(txtDtStart.Text);
                        DateTime dtEnd = new DtFunc().StringToDateTime(txtDtEnd.Text);
                        if (-1 == dtEnd.CompareTo(dtStart))
                            txtDtEnd.Text = txtDtStart.Text;
                    }
                }
                if (false == IsPostBack)
                    OnRefreshClick(sender, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRefreshClick(object sender, EventArgs e)
        {
            DisplayEars();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20200518 [XXXXX] Rafctoring (Utilisation de DataCache) 
        private void OnXmlClick(object sender, EventArgs e)
        {
            
            if (null != DataCache.GetData<XmlDocument>(EarSessionID))
            {
                m_DocEar = (XmlDocument)DataCache.GetData<XmlDocument>(EarSessionID);
                DisplayXml("Ear_XML", "Ear_" + m_TradeCommonInput.Identifier, m_DocEar.InnerXml);
            }

            Control ctrl = FindControl("phEars");
            if ((null != ctrl) && (null != DataCache.GetData<LiteralControl>(EarResultSessionID)))
            {
                ctrl.Controls.Clear();
                ctrl.Controls.Add(DataCache.GetData<LiteralControl>(EarResultSessionID));
            }
        }
        #endregion Events

        #region Methods
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            pnlToolBar.Controls.Add(AddPanelButton());
            AddPanelCriteria(pnlToolBar);
            pnlToolBar.Controls.Add(new Panel() { ID = "divtbevt6" });

            CellForm.Controls.Add(pnlToolBar);
        }
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Panel AddPanelButton()
        {
            Panel pnl = new Panel() { ID = "divtbear1" };
            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Click += new EventHandler(OnRefreshClick);
            pnl.Controls.Add(btnRefresh);

            WCToolTipLinkButton btnExpandCollapse = ControlsTools.GetAwesomeButtonExpandCollapse(false);
            btnExpandCollapse.OnClientClick = "javascript:ExpandCollapseAll(this);return false;";
            pnl.Controls.Add(btnExpandCollapse);

            pnl.Controls.Add(ControlsTools.GetToolTipLinkButtonPrint("divbody"));

            return pnl;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddPanelCriteria(Panel pPnlContainer)
        {
            Panel pnlDailyEar = new Panel() { ID = "divtbear2" };
            RadioButton rbDailyEar = new RadioButton() 
            {
                ID = Cst.BUT + "DAILYEAR",
                GroupName = "EarDisplaySelection",
                Text = Ressource.GetString("DailyEar"),
                Checked = true,
                EnableViewState = true,
                AutoPostBack = false
            };
            rbDailyEar.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlDailyEar.Controls.Add(rbDailyEar);
            pnlDailyEar.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            RadioButton rbAllEar = new RadioButton()
            {
                ID = Cst.BUT + "ALLEAR",
                GroupName = "EarDisplaySelection",
                Text = Ressource.GetString("AllEar"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = false
            };
            rbAllEar.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlDailyEar.Controls.Add(rbAllEar);
            pnlDailyEar.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            RadioButton rbSelectEar = new RadioButton()
            {
                ID = Cst.BUT + "SELECTEAR",
                GroupName = "EarDisplaySelection",
                Text = Ressource.GetString("SelectEar"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = false,
            };
            rbSelectEar.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlDailyEar.Controls.Add(rbSelectEar);

            pPnlContainer.Controls.Add(pnlDailyEar);

            Panel pnlEventDates = new Panel() { ID = "divtbear3" };

            Label lblDtEvent = new Label
            {
                ID = Cst.LBL + "DTEVENT",
                Text = Ressource.GetString("DtEarEvent")
            };
            pnlEventDates.Controls.Add(lblDtEvent);
            pnlEventDates.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            // FI 20200811 [XXXXX] use OTCmlHelper.GetDateBusiness puisque GetRDBMSDateSys est obsolete
            //DateTime dtTmp;
            //DateTime dt = OTCmlHelper.GetRDBMSDateSys(SessionTools.CS, null, true, false, out dtTmp);
            DateTime dt = OTCmlHelper.GetDateBusiness(CSTools.SetCacheOn(SessionTools.CS));
            CreateControlEarDate(pnlEventDates, dt.Date, "DtStartEar");
            pnlEventDates.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
            CreateControlEarDate(pnlEventDates, dt.Date, "DtEndEar");
            pPnlContainer.Controls.Add(pnlEventDates);

            Panel pnlDisplay = new Panel() { ID = "divtbear4" };
            CheckBox chkDisplayAllFlows2 = new CheckBox()
            {
                ID = Cst.CHK + "DISPLAYALLFLOWS",
                Text = Ressource.GetString("EarDisplayAllFlows"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = false,
            };
            chkDisplayAllFlows2.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlDisplay.Controls.Add(chkDisplayAllFlows2);
            pnlDisplay.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            CheckBox chkDisplayAllClosing2 = new CheckBox()
            {
                ID = Cst.CHK + "DISPLAYALLCLOSING",
                Text = Ressource.GetString("EarDisplayAllClosing"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = false
            };
            chkDisplayAllClosing2.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlDisplay.Controls.Add(chkDisplayAllClosing2);
            pnlDisplay.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));

            CheckBox chkDisplayAllExchange2 = new CheckBox()
            {
                ID = Cst.CHK + "DISPLAYALLEXCHANGE",
                Text = Ressource.GetString("EarDisplayAllExchange"),
                Checked = false,
                EnableViewState = true,
                AutoPostBack = false
            };
            chkDisplayAllExchange2.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlDisplay.Controls.Add(chkDisplayAllExchange2);
            pPnlContainer.Controls.Add(pnlDisplay);

            Panel pnlWithLabel = new Panel() { ID = "divtbear5" };
            CheckBox chkDisplayLabel = new CheckBox()
            {
                ID = Cst.CHK + "DISPLAYLABEL",
                Text = Ressource.GetString("EarDisplayLabel"),
                Checked = true,
                EnableViewState = true,
                AutoPostBack = false,
                Visible = false
            };
            chkDisplayLabel.Attributes.Add("onclick", "GUI_ShowHide();");
            pnlWithLabel.Controls.Add(chkDisplayLabel);
            pPnlContainer.Controls.Add(pnlWithLabel);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correction et compléments
        private void AddFooter()
        {
            Panel pnlFooter = new Panel()
            {
                ID = "divfooter",
                CssClass = CSSMode + " ear " + m_TradeCommonInputGUI.MainMenuClassName
            };
            WCToolTipLinkButton btn = new WCToolTipLinkButton()
            {
                ID = "btnXML",
                CssClass = "fa-icon",
                AccessKey = "X",
                CausesValidation = false,
                Text = "<i class='fas fa-external-link-alt'></i> XML"
            };
            btn.Click += new EventHandler(OnXmlClick);
            pnlFooter.Controls.Add(btn);
            CellForm.Controls.Add(pnlFooter);

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = m_TradeCommonInputGUI.MainMenuClassName };

            PlaceHolder ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "phEars"
            };

            pnlBody.Controls.Add(ph);
            CellForm.Controls.Add(pnlBody);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/EAR.js"));
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pId"></param>
        /// <returns></returns>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        private void CreateControlEarDate(Panel pPnlContainer, DateTime pDate, string pId)
        {
            WCTooltipLabel lblDate = new WCTooltipLabel()
            {
                ID = "Lbl" + pId,
                Width = Unit.Pixel(65),
                Text = Ressource.GetString(pId)
            };
            pPnlContainer.Controls.Add(lblDate);

            WCTextBox2 txtDate = new WCTextBox2(Cst.TXT + pId.ToUpper(), 
                new Validator("EarDate", true),
                new Validator(EFSRegex.TypeRegex.RegexDate, "EarDate", true));
            if (0 != pDate.CompareTo(DateTime.MinValue))
                txtDate.Text = DtFunc.DateTimeToString(pDate, DtFunc.FmtShortDate);
            txtDate.CssClass = "DtPicker";
            txtDate.Width = Unit.Pixel(65);
            txtDate.EnableViewState = true;
            txtDate.AutoPostBack = false;
            txtDate.Attributes.Add("onchange", "GUI_ShowHide();");
            pPnlContainer.Controls.Add(txtDate);

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160804 [Migration TFS] Modify
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        private void DisplayEars()
        {
            //20071212 FI Ticket 16012 => Migration Asp2.0 : utilisation de XSLTTools.TransformXml
            CookieData[] cookiedata = new CookieData[5];
            string controlValue;
            #region Daily Ear
            bool isDailyEar = false;
            bool isAllEar = false;
            bool isSelectEar = false;
            //
            RadioButton butDailyEar = (RadioButton)FindControl(Cst.BUT + "DAILYEAR");
            RadioButton butAllEar = (RadioButton)FindControl(Cst.BUT + "ALLEAR");
            RadioButton butSelectEar = (RadioButton)FindControl(Cst.BUT + "SELECTEAR");
            //
            WebControl pnlEventDate = FindControl("divtbear3") as WebControl;
            //
            if (null != butDailyEar && null != butAllEar && null != butSelectEar)
            {
                if (false == IsPostBack)
                {
                    ReadCookie(butDailyEar.GroupName, out controlValue);
                    //
                    if (StrFunc.IsFilled(controlValue))
                    {
                        butDailyEar.Checked = (controlValue == butDailyEar.ID);
                        butAllEar.Checked = (controlValue == butAllEar.ID);
                        butSelectEar.Checked = (controlValue == butSelectEar.ID);
                    }
                    else
                        butDailyEar.Checked = true;
                }
                //
                isDailyEar = (butDailyEar.Checked);
                isAllEar = (butAllEar.Checked);
                isSelectEar = (butSelectEar.Checked);
                //
                if (IsPostBack)
                    cookiedata[0] = new CookieData(Cst.SQLCookieGrpElement.TradeEars, butDailyEar.GroupName,
                        (butDailyEar.Checked ? butDailyEar.ID : (butAllEar.Checked ? butAllEar.ID : butSelectEar.ID)));

            }

            #endregion Daily Ear
            #region Date sélection
            //
            #endregion Daily Ear
            #region Date sélection

            DateTime dtStart = Convert.ToDateTime(null);
            DateTime dtEnd = Convert.ToDateTime(null);

            if (isAllEar || isDailyEar)
            {
                pnlEventDate.Style.Add(HtmlTextWriterStyle.Display, "none");

                // FI 20200811 [XXXXX] use OTCmlHelper.GetDateBusiness puisque GetRDBMSDateSys est obsolete
                //DateTime dtTmp = DateTime.MinValue;
                //dtStart = OTCmlHelper.GetRDBMSDateSys(SessionTools.CS, null, true, false, out dtTmp);

                dtStart = OTCmlHelper.GetDateBusiness(CSTools.SetCacheOn(SessionTools.CS));
                dtEnd = dtStart;
            }
            else
            {
                ControlsTools.RemoveStyleDisplay(pnlEventDate);
                if ((FindControl(Cst.TXT + "DTSTARTEAR") is WCTextBox2 txtDtStart) && (FindControl(Cst.TXT + "DTENDEAR") is WCTextBox2 txtDtEnd))
                {
                    dtStart = new DtFunc().StringToDateTime(txtDtStart.Text);
                    dtEnd = new DtFunc().StringToDateTime(txtDtEnd.Text);
                }
            }
            #endregion Date sélection
            #region EarWithLabel
            bool isDisplayLabel = false;
            CheckBox chkDisplayLabel = (CheckBox)FindControl(Cst.CHK + "DISPLAYLABEL");
            if (null != chkDisplayLabel)
            {
                if (false == IsPostBack)
                {
                    ReadCookie(chkDisplayLabel.ID, out controlValue);
                    chkDisplayLabel.Checked = (controlValue == "1" || StrFunc.IsEmpty(controlValue));
                }
                isDisplayLabel = (chkDisplayLabel.Checked);
                //
                if (IsPostBack)
                    cookiedata[1] = new CookieData(Cst.SQLCookieGrpElement.TradeEars, chkDisplayLabel.ID, (isDisplayLabel ? "1" : "0"));
            }
            //
            #endregion EarWithLabel
            #region DisplayAllExchange
            bool isDisplayAllExchange = true;
            CheckBox chkDisplayAllExchange = (CheckBox)FindControl(Cst.CHK + "DISPLAYALLEXCHANGE");
            if (null != chkDisplayAllExchange)
            {
                if (false == IsPostBack)
                {
                    ReadCookie(chkDisplayAllExchange.ID, out controlValue);
                    chkDisplayAllExchange.Checked = (controlValue == "1");
                }
                isDisplayAllExchange = chkDisplayAllExchange.Checked;
                //
                if (IsPostBack)
                    cookiedata[2] = new CookieData(Cst.SQLCookieGrpElement.TradeEars, chkDisplayAllExchange.ID, (isDisplayAllExchange ? "1" : "0"));
            }
            //
            #endregion DisplayAllExchange
            #region DisplayAllClosing
            bool isDisplayAllClosing = true;
            CheckBox chkDisplayAllClosing = (CheckBox)FindControl(Cst.CHK + "DISPLAYALLCLOSING");
            if (null != chkDisplayAllClosing)
            {
                if (false == IsPostBack)
                {
                    ReadCookie(chkDisplayAllClosing.ID, out controlValue);
                    chkDisplayAllClosing.Checked = (controlValue == "1");
                }
                isDisplayAllClosing = chkDisplayAllClosing.Checked;
                //
                if (IsPostBack)
                    cookiedata[3] = new CookieData(Cst.SQLCookieGrpElement.TradeEars, chkDisplayAllClosing.ID, (isDisplayAllClosing ? "1" : "0"));
            }
            //
            #endregion DisplayAllClosing
            #region DisplayAllFlows
            bool isDisplayAllFlows = true;
            CheckBox chkDisplayAllFlows = (CheckBox)FindControl(Cst.CHK + "DISPLAYALLFLOWS");
            if (null != chkDisplayAllFlows)
            {
                if (false == IsPostBack)
                {
                    ReadCookie(chkDisplayAllFlows.ID, out controlValue);
                    chkDisplayAllFlows.Checked = (controlValue == "1");
                }
                isDisplayAllFlows = chkDisplayAllFlows.Checked;
                //
                if (IsPostBack)
                    cookiedata[4] = new CookieData(Cst.SQLCookieGrpElement.TradeEars, chkDisplayAllFlows.ID, (isDisplayAllFlows ? "1" : "0"));
            }
            //
            #endregion DisplayAllFlows
            DataSet dsEars =   GetSelectEars(isDailyEar, isSelectEar, dtStart, dtEnd);

            dsEars.DataSetName = "EARS";
            if (isNewVersion)
            {
                #region Tables
                DataTable dtEar = dsEars.Tables[0];
                dtEar.TableName = "EARBOOK";
                DataTable dtEarDet = dsEars.Tables[1];
                dtEarDet.TableName = "EARDET";
                //
                //DataTable dtEarResult = dsEars.Tables[2];
                //dtEarResult.TableName = "EAR";
                //
                DataTable dtEarClass = dsEars.Tables[2];
                dtEarClass.TableName = "EARCLASS";
                DataTable dtEarDay = dsEars.Tables[3];
                dtEarDay.TableName = "EARDAY";
                DataTable dtEarCommon = dsEars.Tables[4];
                dtEarCommon.TableName = "EARCOMMON";
                DataTable dtEarNom = dsEars.Tables[5];
                dtEarNom.TableName = "EARNOM";
                DataTable dtEarCalc = dsEars.Tables[6];
                dtEarCalc.TableName = "EARCALC";
                DataTable dtEarDayAmount = dsEars.Tables[7];
                dtEarDayAmount.TableName = "EARDAYAMOUNT";
                DataTable dtEarCommonAmount = dsEars.Tables[8];
                dtEarCommonAmount.TableName = "EARCOMMONAMOUNT";
                DataTable dtEarNomAmount = dsEars.Tables[9];
                dtEarNomAmount.TableName = "EARNOMAMOUNT";
                DataTable dtEarCalcAmount = dsEars.Tables[10];
                dtEarCalcAmount.TableName = "EARCALCAMOUNT";
                #endregion Tables
                #region Columns Mapping
                dtEar.Columns["IDCACCOUNT"].ColumnMapping = MappingType.Hidden;
                dtEar.Columns["BOOK_DISPLAYNAME"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["ENTITY_DISPLAYNAME"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDEAR"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDB"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDT"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["TRADE_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["DTEAR"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["DTEVENT"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["BOOK_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDA_ENTITY"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["ENTITY_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDSTACTIVATION"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;

                dtEarDet.Columns["DTEAR"].ColumnMapping = MappingType.Hidden;
                dtEarDet.Columns["IDEAR"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["IDT"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["IDI"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["INSTR_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["INSTR_DISPLAYNAME"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["STREAMNO"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //
                //dtEarResult.Columns["IDEAR"].ColumnMapping = MappingType.Attribute;
                //dtEarResult.Columns["IDT"].ColumnMapping = MappingType.Attribute;
                //dtEarResult.Columns["DTACCOUNT"].ColumnMapping = MappingType.Attribute;
                //dtEarResult.Columns["IDSTACTIVATION"].ColumnMapping = MappingType.Attribute;

                dtEarClass.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarClass.Columns["CODE"].ColumnMapping = MappingType.Attribute;
                dtEarClass.Columns["CLASS"].ColumnMapping = MappingType.Attribute;
                dtEarClass.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Attribute;
                dtEarClass.Columns["STREAMNO"].ColumnMapping = MappingType.Attribute;
                dtEarClass.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //
                dtEarDay.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarDay.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarDay.Columns["CODE"].ColumnMapping = MappingType.Hidden;
                dtEarDay.Columns["CLASS"].ColumnMapping = MappingType.Hidden;
                dtEarDay.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Hidden;
                dtEarDay.Columns["STREAMNO"].ColumnMapping = MappingType.Hidden;
                dtEarDay.Columns["IDEARDAY"].ColumnMapping = MappingType.Attribute;
                dtEarDay.Columns["TYPE"].ColumnMapping = MappingType.Attribute;
                dtEarDay.Columns["SIDE"].ColumnMapping = MappingType.Attribute;
                dtEarDay.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //
                dtEarCommon.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarCommon.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarCommon.Columns["CODE"].ColumnMapping = MappingType.Hidden;
                dtEarCommon.Columns["CLASS"].ColumnMapping = MappingType.Hidden;
                dtEarCommon.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Hidden;
                dtEarCommon.Columns["STREAMNO"].ColumnMapping = MappingType.Hidden;
                dtEarCommon.Columns["IDEARDAY"].ColumnMapping = MappingType.Attribute;
                dtEarCommon.Columns["IDEARCOMMON"].ColumnMapping = MappingType.Attribute;
                dtEarCommon.Columns["TYPE"].ColumnMapping = MappingType.Attribute;
                dtEarCommon.Columns["SIDE"].ColumnMapping = MappingType.Attribute;
                dtEarCommon.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //
                dtEarNom.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarNom.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarNom.Columns["CODE"].ColumnMapping = MappingType.Hidden;
                dtEarNom.Columns["CLASS"].ColumnMapping = MappingType.Hidden;
                dtEarNom.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Hidden;
                dtEarNom.Columns["STREAMNO"].ColumnMapping = MappingType.Hidden;
                dtEarNom.Columns["IDEARNOM"].ColumnMapping = MappingType.Attribute;
                dtEarNom.Columns["TYPE"].ColumnMapping = MappingType.Attribute;
                dtEarNom.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //
                dtEarCalc.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarCalc.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarCalc.Columns["CODE"].ColumnMapping = MappingType.Hidden;
                dtEarCalc.Columns["CLASS"].ColumnMapping = MappingType.Hidden;
                dtEarCalc.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Hidden;
                dtEarCalc.Columns["STREAMNO"].ColumnMapping = MappingType.Hidden;
                dtEarCalc.Columns["IDEARCALC"].ColumnMapping = MappingType.Attribute;
                dtEarCalc.Columns["TYPE"].ColumnMapping = MappingType.Attribute;
                dtEarCalc.Columns["SIDE"].ColumnMapping = MappingType.Attribute;
                dtEarCalc.Columns["AGFUNC"].ColumnMapping = MappingType.Attribute;
                dtEarCalc.Columns["AGAMOUNTS"].ColumnMapping = MappingType.Attribute;
                dtEarCalc.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //
                dtEarDayAmount.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarDayAmount.Columns["IDEARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarDayAmount.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarDayAmount.Columns["EXCHANGETYPE"].ColumnMapping = MappingType.Attribute;
                dtEarDayAmount.Columns["CURRENCY"].ColumnMapping = MappingType.Attribute;
                dtEarDayAmount.Columns["DTACCOUNT"].ColumnMapping = MappingType.Attribute;
                dtEarDayAmount.Columns["DTACCOUNT"].ColumnName = "DATE";
                dtEarDayAmount.Columns["STATUS"].ColumnMapping = MappingType.Attribute;
                dtEarDayAmount.Columns["VALUE"].ColumnMapping = MappingType.Attribute;
                dtEarDayAmount.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //dtEarDayAmount.Columns["AMOUNT"].ColumnMapping = MappingType.SimpleContent;
                //
                dtEarCommonAmount.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarCommonAmount.Columns["IDEARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarCommonAmount.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarCommonAmount.Columns["EXCHANGETYPE"].ColumnMapping = MappingType.Attribute;
                dtEarCommonAmount.Columns["CURRENCY"].ColumnMapping = MappingType.Attribute;
                dtEarCommonAmount.Columns["DTACCOUNT"].ColumnMapping = MappingType.Attribute;
                dtEarCommonAmount.Columns["DTACCOUNT"].ColumnName = "DATE";
                dtEarCommonAmount.Columns["STATUS"].ColumnMapping = MappingType.Attribute;
                dtEarCommonAmount.Columns["VALUE"].ColumnMapping = MappingType.Attribute;
                dtEarCommonAmount.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //dtEarCommonAmount.Columns["AMOUNT"].ColumnMapping = MappingType.SimpleContent;
                //
                dtEarNomAmount.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarNomAmount.Columns["IDEARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarNomAmount.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarNomAmount.Columns["EXCHANGETYPE"].ColumnMapping = MappingType.Attribute;
                dtEarNomAmount.Columns["CURRENCY"].ColumnMapping = MappingType.Attribute;
                dtEarNomAmount.Columns["DTACCOUNT"].ColumnMapping = MappingType.Attribute;
                dtEarNomAmount.Columns["DTACCOUNT"].ColumnName = "DATE";
                dtEarNomAmount.Columns["STATUS"].ColumnMapping = MappingType.Attribute;
                dtEarNomAmount.Columns["VALUE"].ColumnMapping = MappingType.Attribute;
                dtEarNomAmount.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //dtEarNomAmount.Columns["AMOUNT"].ColumnMapping = MappingType.SimpleContent;
                //
                dtEarCalcAmount.Columns["IDEAR"].ColumnMapping = MappingType.Hidden;
                dtEarCalcAmount.Columns["IDEARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarCalcAmount.Columns["EARTYPE"].ColumnMapping = MappingType.Hidden;
                dtEarCalcAmount.Columns["EXCHANGETYPE"].ColumnMapping = MappingType.Attribute;
                dtEarCalcAmount.Columns["CURRENCY"].ColumnMapping = MappingType.Attribute;
                dtEarCalcAmount.Columns["DTACCOUNT"].ColumnMapping = MappingType.Attribute;
                dtEarCalcAmount.Columns["DTACCOUNT"].ColumnName = "DATE";
                dtEarCalcAmount.Columns["STATUS"].ColumnMapping = MappingType.Attribute;
                dtEarCalcAmount.Columns["VALUE"].ColumnMapping = MappingType.Attribute;
                dtEarCalcAmount.Columns["ISSELECTED"].ColumnMapping = MappingType.Hidden;
                //dtEarCalcAmount.Columns["AMOUNT"].ColumnMapping = MappingType.SimpleContent;

                #endregion Columns Mapping
                #region Relations Setting
                DataRelation relEarDet = new DataRelation("EarDet", dtEar.Columns["IDEAR"], dtEarDet.Columns["IDEAR"], false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarDet);
                //
                //DataRelation relEarResult = new DataRelation("EarResult", dtEar.Columns["IDEAR"], dtEarResult.Columns["IDEAR"], false);
                //relEarResult.Nested = true;
                //dsEars.Relations.Add(relEarResult);
                //
                DataRelation relEarClass = new DataRelation("EarClass", dtEar.Columns["IDEAR"], dtEarClass.Columns["IDEAR"], false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarClass);
                //
                DataColumn[] parentColumns = new DataColumn[5] { dtEarClass.Columns["IDEAR"], dtEarClass.Columns["INSTRUMENTNO"], dtEarClass.Columns["STREAMNO"], dtEarClass.Columns["CODE"], dtEarClass.Columns["CLASS"] };
                DataColumn[] childColumns = new DataColumn[5] { dtEarDay.Columns["IDEAR"], dtEarDay.Columns["INSTRUMENTNO"], dtEarDay.Columns["STREAMNO"], dtEarDay.Columns["CODE"], dtEarDay.Columns["CLASS"] };
                DataRelation relEarDay = new DataRelation("EarDay", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarDay);
                //
                childColumns = new DataColumn[5] { dtEarCommon.Columns["IDEAR"], dtEarCommon.Columns["INSTRUMENTNO"], dtEarCommon.Columns["STREAMNO"], dtEarCommon.Columns["CODE"], dtEarCommon.Columns["CLASS"] };
                DataRelation relEarCommon = new DataRelation("EarCommon", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarCommon);
                //
                childColumns = new DataColumn[5] { dtEarNom.Columns["IDEAR"], dtEarNom.Columns["INSTRUMENTNO"], dtEarNom.Columns["STREAMNO"], dtEarNom.Columns["CODE"], dtEarNom.Columns["CLASS"] };
                DataRelation relEarNom = new DataRelation("EarNom", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarNom);
                //
                childColumns = new DataColumn[5] { dtEarCalc.Columns["IDEAR"], dtEarCalc.Columns["INSTRUMENTNO"], dtEarCalc.Columns["STREAMNO"], dtEarCalc.Columns["CODE"], dtEarCalc.Columns["CLASS"] };
                DataRelation relEarCalc = new DataRelation("EarCalc", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarCalc);
                //
                parentColumns = new DataColumn[2] { dtEarDay.Columns["IDEAR"], dtEarDay.Columns["IDEARDAY"] };
                childColumns = new DataColumn[2] { dtEarDayAmount.Columns["IDEAR"], dtEarDayAmount.Columns["IDEARTYPE"] };
                DataRelation relEarDayAmount = new DataRelation("EarDayAmount", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarDayAmount);
                //
                parentColumns = new DataColumn[2] { dtEarCommon.Columns["IDEAR"], dtEarCommon.Columns["IDEARCOMMON"] };
                childColumns = new DataColumn[2] { dtEarCommonAmount.Columns["IDEAR"], dtEarCommonAmount.Columns["IDEARTYPE"] };
                DataRelation relEarCommonAmount = new DataRelation("EarCommonMoney", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarCommonAmount);
                //
                parentColumns = new DataColumn[2] { dtEarNom.Columns["IDEAR"], dtEarNom.Columns["IDEARNOM"] };
                childColumns = new DataColumn[2] { dtEarNomAmount.Columns["IDEAR"], dtEarNomAmount.Columns["IDEARTYPE"] };
                DataRelation relEarNomAmount = new DataRelation("EarNomMoney", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarNomAmount);
                //
                parentColumns = new DataColumn[2] { dtEarCalc.Columns["IDEAR"], dtEarCalc.Columns["IDEARCALC"] };
                childColumns = new DataColumn[2] { dtEarCalcAmount.Columns["IDEAR"], dtEarCalcAmount.Columns["IDEARTYPE"] };
                DataRelation relEarCalcAmount = new DataRelation("EarCalcMoney", parentColumns, childColumns, false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarCalcAmount);
                #endregion Relations Setting
            }
            else
            {
                #region Tables
                DataTable dtEar = dsEars.Tables[0];
                dtEar.TableName = "EARBOOK";
                DataTable dtEarDet = dsEars.Tables[1];
                dtEarDet.TableName = "EARDET";
                //                    
                DataTable dtEarResult = dsEars.Tables[2];
                dtEarResult.TableName = "EAR";
                #endregion Tables
                #region Columns Mapping
                dtEar.Columns["IDCACCOUNT"].ColumnMapping = MappingType.Hidden;
                dtEar.Columns["BOOK_DISPLAYNAME"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["ENTITY_DISPLAYNAME"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDEAR"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDB"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDT"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["TRADE_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["DTEAR"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["DTEVENT"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["BOOK_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDA_ENTITY"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["ENTITY_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEar.Columns["IDSTACTIVATION"].ColumnMapping = MappingType.Attribute;

                dtEarDet.Columns["DTEAR"].ColumnMapping = MappingType.Hidden;
                dtEarDet.Columns["IDEAR"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["IDT"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["IDI"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["INSTR_IDENTIFIER"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["INSTR_DISPLAYNAME"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["INSTRUMENTNO"].ColumnMapping = MappingType.Attribute;
                dtEarDet.Columns["STREAMNO"].ColumnMapping = MappingType.Attribute;
                //
                dtEarResult.Columns["IDEAR"].ColumnMapping = MappingType.Attribute;
                dtEarResult.Columns["IDT"].ColumnMapping = MappingType.Attribute;
                dtEarResult.Columns["DTACCOUNT"].ColumnMapping = MappingType.Attribute;
                dtEarResult.Columns["IDSTACTIVATION"].ColumnMapping = MappingType.Attribute;
                #endregion Columns Mapping
                #region Relations Setting
                DataRelation relEarDet = new DataRelation("EarDet", dtEar.Columns["IDEAR"], dtEarDet.Columns["IDEAR"], false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarDet);
                //
                DataRelation relEarResult = new DataRelation("EarResult", dtEar.Columns["IDEAR"], dtEarResult.Columns["IDEAR"], false)
                {
                    Nested = true
                };
                dsEars.Relations.Add(relEarResult);
                #endregion Relations Setting
            }

            #region Ear XML Construction
            StringBuilder sb = new StringBuilder();
            TextWriter writer = new StringWriter(sb);
            XmlTextWriter xw = new XmlTextWriter(writer);
            dsEars.WriteXml(xw);
            writer.Close();
            #endregion Ear XML Construction
            //
            StringBuilder sb2 = new StringBuilder();
            sb2.Append(sb.ToString().Replace("&lt;", "<").Replace("&gt;", ">"));
            m_DocEar = new XmlDocument();
            m_DocEar.LoadXml(sb2.ToString());
            // FI 2020518 [XXXXX] Utilisation de DataCache
            //Session[EarSessionID] = m_DocEar;
            DataCache.SetData(EarSessionID, m_DocEar);

            
            string xslt;
            if (isNewVersion)
            {
                // FI 20160804 [Migration TFS] Utilisation de la méthode SearchFile 
                //xslt = ReferentialTools.GetObjectXSLFile(Cst.ListType.EAR, "Ear");
                xslt = StrFunc.AppendFormat(@"~\GUIOutput\{0}\{1}.XSLT", Cst.ListType.EAR.ToString(), "Ear");
            }
            else
            {
                // FI 20160804 [Migration TFS] Utilisation de la méthode SearchFile 
                //xslt = ReferentialTools.GetObjectXSLFile(Cst.ListType.EAR, "Ear_v211");
                xslt = StrFunc.AppendFormat(@"~\GUIOutput\{0}\{1}.XSLT", Cst.ListType.EAR.ToString(), "Ear_v211");
            }
            SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, xslt, ref xslt); ;

            Hashtable param = new Hashtable
            {
                { "pWithTitle", decimal.Zero },
                { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name },
                { "pWithLabel", Convert.ToInt32(isDisplayLabel) },
                { "pWithAllExchange", Convert.ToInt32(isDisplayAllExchange) },
                { "pWithAllClosing", Convert.ToInt32(isDisplayAllClosing) },
                { "pWithAllFlows", Convert.ToInt32(isDisplayAllFlows) }
            };
            //
            string retTransForm = XSLTTools.TransformXml(sb2, xslt, param, null).ToString();
            //
            #region Add html transformation to placeHolder
            Control ctrl = FindControl("phEars");
            if (null != ctrl)
            {
                ctrl.Controls.Clear();
                LiteralControl lit = new LiteralControl(retTransForm);
                ctrl.Controls.Add(lit);
                // FI 20200518 [XXXXX] Utilisation de DataCache 
                //Session[EarResultSessionID] = lit;
                DataCache.SetData<LiteralControl>(EarResultSessionID, lit);
            }
            #endregion Add html transformation to placeHolder
            //
            if (IsPostBack)
                AspTools.WriteSQLCookie(cookiedata);

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmEars";
            if (IsSessionVariableAvailable)
            {
                AddToolBar();
                AddBody();
                AddFooter();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsDailyEar"></param>
        /// <param name="pIsSelectEar"></param>
        /// <param name="pDtStart"></param>
        /// <param name="pDtEnd"></param>
        /// <returns></returns>
        /// EG [XXXXX][WI417] Coquille DTSTART vs DTSART
        private DataSet GetSelectEars(bool pIsDailyEar, bool pIsSelectEar, DateTime pDtStart, DateTime pDtEnd)
        {
            string CS = SessionTools.CS;
            //ArrayList aTemp, aTemp2;
            DataParameters dp = new DataParameters();
            //
            string SQLWhere = "IDT=@IDT";
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDT), m_TradeCommonInput.IdT);
            //
            if (pIsDailyEar)
            {
                SQLWhere += SQLCst.AND + "DTEAR=@DTSTART";
                dp.Add(new DataParameter(CS, "DTSTART", DbType.Date), pDtStart);
            }
            else if (pIsSelectEar)
            {
                SQLWhere += SQLCst.AND + " DTEVENT>=@DTSTART";
                SQLWhere += SQLCst.AND + " DTEVENT<=@DTEND";
                //
                dp.Add(new DataParameter(CS, "DTSTART", DbType.Date), pDtStart);
                dp.Add(new DataParameter(CS, "DTEND", DbType.Date), pDtEnd);
            }
            //
            string SQLQuery = string.Empty;
            SQLReferentialData.SQLSelectParameters sqlSelectParameters;
            #region Ear
            if (StrFunc.IsEmpty(xmlRefEar))
                xmlRefEar = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARBOOK");
            ReferentialTools.DeserializeXML_ForModeRO(xmlRefEar, out ReferentialsReferential refEar);
            if (null != refEar)
            {
                sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEar, SQLWhere);
                SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
            }

            #endregion Ear
            #region EarDet
            if (StrFunc.IsEmpty(xmlRefEarDet))
                xmlRefEarDet = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARDET");
            ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarDet, out ReferentialsReferential refEarDet);
            if (null != refEarDet)
            {
                //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarDet, SQLWhere, false, false, out aTemp, out aTemp2).Query;
                sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarDet, SQLWhere);
                SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
            }
            #endregion EarDet
            //
            if (isNewVersion)
            {
                #region EarClass
                if (StrFunc.IsEmpty(xmlRefEarClass))
                    xmlRefEarClass = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARCLASS");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarClass, out ReferentialsReferential refEarClass);
                if (null != refEarClass)
                {
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarClass, SQLWhere, false, false, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarClass, SQLWhere);
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EarClass
                #region EarDay
                if (StrFunc.IsEmpty(xmlRefEarDay))
                    xmlRefEarDay = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARDAY");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarDay, out ReferentialsReferential refEarDay);
                if (null != refEarDay)
                {
                    //20090805 FI GLOP FI PAGER (pourkoi select distinct)
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarDay, SQLWhere, false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarDay, SQLWhere)
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EarDay
                #region EarCommon
                if (StrFunc.IsEmpty(xmlRefEarCommon))
                    xmlRefEarCommon = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARCOMMON");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarCommon, out ReferentialsReferential refEarCommon);
                if (null != refEarCommon)
                {
                    //20090805 FI GLOP FI PAGER (pourkoi select distinct)
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarCommon, SQLWhere, false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarCommon, SQLWhere)
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EarCommon
                #region EarNom
                if (StrFunc.IsEmpty(xmlRefEarNom))
                    xmlRefEarNom = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARNOM");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarNom, out ReferentialsReferential refEarNom);
                if (null != refEarNom)
                {
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarNom, SQLWhere, false, false, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarNom, SQLWhere);
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EarNom
                #region EarCalc
                if (StrFunc.IsEmpty(xmlRefEarCalc))
                    xmlRefEarCalc = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARCALC");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarCalc, out ReferentialsReferential refEarCalc);
                if (null != refEarCalc)
                {
                    //20090805 FI GLOP FI PAGER (pourkoi select distinct)
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarCalc, SQLWhere, false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarCalc, SQLWhere)
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }

                #endregion EarCalc
                #region EarDayMoney
                if (StrFunc.IsEmpty(xmlRefEarMoney))
                    xmlRefEarMoney = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARMONEY");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarMoney, out ReferentialsReferential refEarDayMoney);
                if (null != refEarDayMoney)
                {
                    //GLOP FI PAGER (pourkoi select distinct ??)
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARDAY"), false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARDAY"))
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                    //
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARCOMMON"), false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARCOMMON"))
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                    //
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARNOM"), false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARNOM"))
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                    //
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARCALC"), false, true, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarDayMoney, SQLWhere + SQLCst.AND + " EARTYPE=" + DataHelper.SQLString("EARCALC"))
                    {
                        isSelectDistinct = true
                    };
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }
                #endregion EarDayMoney
            }
            else
            {
                #region EarResult
                if (StrFunc.IsEmpty(xmlRefEarResult))
                    xmlRefEarResult = ReferentialTools.GetObjectXMLFile(Cst.ListType.EAR, "EARRESULT");
                ReferentialTools.DeserializeXML_ForModeRO(xmlRefEarResult, out ReferentialsReferential refEarResult);
                if (null != refEarDet)
                {
                    //SQLQuery += SQLReferentialData.GetSQLSelect(CS, refEarResult, SQLWhere, false, false, out aTemp, out aTemp2).Query;
                    sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, refEarResult, SQLWhere);
                    SQLQuery += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
                    SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                }
                #endregion EarResult
            }
            //
            QueryParameters qryParameters = new QueryParameters(CS, SQLQuery, dp);
            DataSet ret = DataHelper.ExecuteDataset(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(OnLoad);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void PageConstruction()
        {
            AbortRessource = true;
            string title = string.Empty;
            string idMenu =string.Empty ;
            string cssMode = string.Empty;
            //
            if (IsSessionVariableAvailable)
            {
                title = Ressource.GetString2("Ears", m_TradeCommonInput.Identifier);
                idMenu = m_TradeCommonInputGUI.IdMenu;
                cssMode = m_TradeCommonInputGUI.CssMode;
            }
            //
            HtmlPageTitle titleLeft = new HtmlPageTitle(title);
            PageTitle = title;
            //
            GenerateHtmlForm();
            //
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), string.Empty, idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, cssMode , false, string.Empty, idMenu);
            //
            HtmlGenericControl body = PageTools.SearchBodyControl(this);
            body.Controls.AddAt(0, new LiteralControl(@"<a name=""toppage""></a>"));
            body.Controls.Add(new LiteralControl(@"<a name=""bottom""></a>"));
            //
            if (false == IsSessionVariableAvailable)
            {
                MsgForAlertImmediate = Ressource.GetString("Msg_SessionVariableParentNotAvailable");
                CloseAfterAlertImmediate = true;
            }

        }

        /// <summary>
        /// Renvoie la valeur du control dans la table cookie
        /// </summary>
        /// <param name="pPage">Page: page appelante</param>
        /// <param name="pControlId">string: Id du control </param>
        /// <param name="pControlValue">string: valeur</param>
        private void ReadCookie(string pControlId, out string pControlValue)
        {
            AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.TradeEars, pControlId, out pControlValue);
        }
        #endregion Methods
    }
}

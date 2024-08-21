using EFS.ACommon;
using EFS.Common.Web;
using EfsML.Business;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    public abstract partial class RunCommonPage : PageBase
    {
        #region ParameterRow
        public class ParameterRow
        {
            #region Members
            private readonly PageBase _PageBase = null;
            private TypeData.TypeDataEnum _dataType = TypeData.TypeDataEnum.unknown;
            private int _rowNum = 0;
            private bool _isEnabled = true;
            private bool _dataIsMandatory = true;
            private string _dataId = string.Empty;
            private string _dataDisplayName = string.Empty;
            private string _infoValue = string.Empty;
            private string _dataValue = string.Empty;
            private string _dataListRet = string.Empty;
            private string _dataRegEx = string.Empty;
            private string _dataNote = string.Empty;
            #endregion Members
            #region Accessors
            public bool IsDataMandatory
            {
                get { return _dataIsMandatory; }
                set { _dataIsMandatory = value; }
            }
            public bool IsEnabled
            {
                get { return _isEnabled; }
                set { _isEnabled = value; }
            }
            public int RowNumber
            {
                get { return _rowNum; }
                set { _rowNum = value; if (StrFunc.IsEmpty(DataId)) { DataId = "DATA" + _rowNum.ToString(); } }
            }
            public string DataId
            {
                get { return _dataId; }
                set { _dataId = value; }
            }
            public TypeData.TypeDataEnum DataType
            {
                get { return _dataType; }
                set { _dataType = value; }
            }
            public string DisplayName
            {
                get { return _dataDisplayName; }
                set { _dataDisplayName = value; }
            }
            public string InfoValue
            {
                get { return _infoValue; }
                set { _infoValue = value; }
            }
            public string DataValue
            {
                get { return _dataValue; }
                set { _dataValue = value; }
            }
            public string DataListRet
            {
                get { return _dataListRet; }
                set { _dataListRet = value; }
            }
            public string DataRegEx
            {
                get { return _dataRegEx; }
                set { _dataRegEx = value; }
            }
            public string DataNote
            {
                get { return _dataNote; }
                set { _dataNote = value; }
            }
            #endregion Accessors
            #region Constructors
            public ParameterRow(PageBase pPageBase)
            {
                _PageBase = pPageBase;
            }
            #endregion Constructors
            #region Methods
            #region ConstructParameterRow
            /// EG 20210419 [XXXXX] Upd Nouveau paramètre - Usage du businessCenter de l'entité
            public TableRow ConstructParameterRow(bool pSetData)
            {
                TableRow tr = new TableRow();
                TableRow trDet = new TableRow();
                TableCell td;

                Unit spaceWidth = Unit.Pixel(3);
                Unit imageWidth = Unit.Pixel(15);

                // Espace
                tr.Controls.Add(HtmlTools.NewControlInCell(spaceWidth));
                // DISPLAYNAME
                td = HtmlTools.NewLabelInCell(Ressource.GetString(_dataDisplayName, _dataDisplayName, true));
                td.Wrap = false;
                td.Style.Add(HtmlTextWriterStyle.PaddingTop, "3px");
                td.Enabled = _isEnabled;
                tr.Controls.Add(td);
                // Espace
                tr.Controls.Add(HtmlTools.NewControlInCell(spaceWidth));

                // Css
                DtFuncML dtFunc = null;
                string dataCss = EFSCssClass.GetCssClass(TypeData.IsTypeNumeric(_dataType), _dataIsMandatory, false, (false == _isEnabled));
                string original_dataValue = _dataValue;

                Unit dataWidth;
                if (TypeData.IsTypeDate(_dataType))
                {
                    #region Date
                    // Width
                    dataWidth = Cst.WidthDate;
                    
                    dtFunc = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null)
                    {
                        FourDigitReading = DtFunc.FourDigitReadingEnum.FourDigitHasYYYY
                    };

                    _dataValue = dtFunc.GetDateString(_dataValue);
                    //}

                    // Regular expression
                    _dataRegEx = EFSRegex.TypeRegex.RegexDate.ToString();
                    // CSS
                    if (_isEnabled)
                        dataCss = "DtPicker " + dataCss;
                    #endregion Date
                }
                else if (TypeData.IsTypeDateTime(_dataType))
                {
                    #region DateTime
                    // Width
                    dataWidth = Cst.WidthDateTime;
                    // Default value
                    if (StrFunc.IsFilled(_dataValue))
                        _dataValue = new DtFunc().GetDateTimeStringFromIso(_dataValue);
                    // Regular expression
                    _dataRegEx = EFSRegex.TypeRegex.RegexDateTime.ToString();
                    // CSS
                    if (_isEnabled)
                        dataCss = "DtTimePicker " + dataCss;
                    #endregion DateTime
                }
                else if (TypeData.IsTypeTime(_dataType))
                {
                    #region Time
                    // Width
                    dataWidth = Cst.WidthTime;
                    // Default value
                    if (StrFunc.IsFilled(_dataValue))
                        _dataValue = new DtFunc().GetShortTimeString(_dataValue);
                    // Regular expression
                    _dataRegEx = EFSRegex.TypeRegex.RegexShortTime.ToString();
                    if (_isEnabled)
                        dataCss = "TimePicker " + dataCss;
                    #endregion Time
                }
                else if (TypeData.IsTypeNumeric(_dataType))
                {
                    #region Numeric
                    // Width
                    dataWidth = Unit.Pixel(150);
                    // Regular expression
                    if (StrFunc.IsEmpty(_dataRegEx))
                    {
                        if (TypeData.IsTypeInt(_dataType))
                            _dataRegEx = EFSRegex.TypeRegex.RegexInteger.ToString();
                        else
                            _dataRegEx = EFSRegex.TypeRegex.RegexDecimal.ToString();
                    }
                    #endregion Numeric
                }
                else if (TypeData.IsTypeBool(_dataType))
                    dataWidth = Unit.Pixel(1);
                else if (StrFunc.IsFilled(_dataListRet))
                    dataWidth = Unit.Pixel(1);
                else
                {
                    if (StrFunc.IsFilled(_infoValue))
                        dataWidth = Unit.Percentage(50);
                    else
                        dataWidth = Unit.Percentage(100);
                }

                //PL 20190227 Newness
                // Data
                bool isDataList = StrFunc.IsFilled(_dataListRet);
                if (isDataList)
                {
                    string dataCssList = EFSCssClass.DropDownListCapture;
                    td = HtmlTools.NewDataListInCell(_dataListRet, true, _dataValue, _dataId, dataCssList, dataWidth, _dataIsMandatory, _isEnabled);

                    if (original_dataValue == "LISTRETRIEVAL")
                    {
                        //TIP: Initialisation d'une VALEUR PAR DEFAUT à partir d'un ordre SQL 
                        isDataList = false;
                        try
                        {
                            string sql_datavalue = ((WCDropDownList2)td.Controls[0]).Items[_dataIsMandatory ? 0 : 1].Value;
                            if (TypeData.IsTypeDate(_dataType))
                                _dataValue = dtFunc.GetDateString(sql_datavalue);
                            else
                                _dataValue = sql_datavalue;
                        }
                        catch
                        {
                            _dataValue = string.Empty;
                        }
                    }
                }

                if (isDataList)
                {
                    td.Style.Add(HtmlTextWriterStyle.PaddingTop, "3px");
                    td.Wrap = false;
                    trDet.Controls.Add(td);
                }
                else
                {
                    if (TypeData.IsTypeBool(_dataType))
                    {
                        td = HtmlTools.NewCheckBoxInCell(string.Empty, _dataValue, _dataId, null, string.Empty, pSetData, dataWidth, _isEnabled);
                        td.Style.Add(HtmlTextWriterStyle.PaddingTop, "3px");
                        td.Wrap = false;
                        trDet.Controls.Add(td);
                    }
                    else
                    {
                        td = HtmlTools.NewTextBoxInCell(_dataValue, _dataId, dataCss, string.Empty, pSetData, dataWidth, _dataIsMandatory, _dataRegEx, _isEnabled);
                        td.Style.Add(HtmlTextWriterStyle.PaddingTop, "3px");
                        td.Wrap = false;
                        trDet.Controls.Add(td);
                    }
                }

                #region InformationControls
                if (StrFunc.IsFilled(_infoValue) || StrFunc.IsFilled(_dataNote))
                {
                    WebControl[] InformationControls = CreateInfo(Cst.TypeInformationMessage.Information, _dataNote, _infoValue, string.Empty);
                    if (ArrFunc.IsFilled(InformationControls))
                    {
                        // Espace
                        trDet.Controls.Add(HtmlTools.NewControlInCell(spaceWidth));

                        if (StrFunc.IsFilled(_infoValue))
                        {
                            // Image
                            td = HtmlTools.NewControlInCell(InformationControls[0], imageWidth);
                            trDet.Controls.Add(td);
                            // Espace
                            trDet.Controls.Add(HtmlTools.NewControlInCell(spaceWidth));
                            // Info
                            td = HtmlTools.NewControlInCell(InformationControls[1]);
                            trDet.Controls.Add(td);
                        }
                        else
                        {
                            // Image
                            td = HtmlTools.NewControlInCell(InformationControls[0]);
                            trDet.Controls.Add(td);
                        }
                    }
                }
                #endregion InformationControls

                td = HtmlTools.NewRowInCell(trDet, Unit.Percentage(100));
                tr.Controls.Add(td);

                return tr;
            }
            #endregion ConstructParameterRow
            #region CreateInfo
            // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
            private WebControl[] CreateInfo(string pType, string pToolTipMessage, string pLabelMessage, string pPopupMessage)
            {
                WebControl[] retControls = null;

                bool isToolTipMessage = StrFunc.IsFilled(pToolTipMessage);
                bool isLabelMessage = StrFunc.IsFilled(pLabelMessage);
                bool isPopupMessage = StrFunc.IsFilled(pPopupMessage);
                if (isToolTipMessage || isLabelMessage || isPopupMessage)
                {
                    string color;
                    string cssClass;
                    switch (pType)
                    {
                        case Cst.TypeInformationMessage.Warning:
                            cssClass = EFSCssClass.Msg_Warning;
                            color = "orange";
                            break;
                        case Cst.TypeInformationMessage.Alert:
                            cssClass = EFSCssClass.Msg_Alert;
                            color = "red";
                            break;
                        case Cst.TypeInformationMessage.Success:
                            cssClass = EFSCssClass.Msg_Success;
                            color = "green";
                            break;
                        case Cst.TypeInformationMessage.Information:
                        default:
                            cssClass = EFSCssClass.Msg_Information;
                            if (isToolTipMessage)
                                color = "blue";
                            else
                                color = "gray";
                            break;
                    }
                    string toolTipMessage = string.Empty;
                    string labelMessage = string.Empty;
                    string popupMessage = string.Empty;

                    if (isToolTipMessage)
                        toolTipMessage = pToolTipMessage;
                    if (isLabelMessage)
                        labelMessage = pLabelMessage;
                    if (isPopupMessage)
                        popupMessage = pPopupMessage;

                    WCToolTipPanel btnMessage = new WCToolTipPanel
                    {
                        CssClass = String.Format("fa-icon fas fa-info-circle {0}", color)
                    };

                    if (isToolTipMessage)
                        btnMessage.Pty.TooltipContent = toolTipMessage;

                    if (isPopupMessage)
                    {
                        string scriptForClick = string.Empty;
                        scriptForClick += "{";
                        scriptForClick += "alert(" + JavaScript.JSString(popupMessage) + ");";
                        scriptForClick += "}";
                        btnMessage.Attributes.Add("onclick", scriptForClick);
                    }

                    WCTooltipLabel lblMessage = null;
                    if (StrFunc.IsFilled(labelMessage))
                    {
                        lblMessage = new WCTooltipLabel
                        {
                            CssClass = cssClass,
                            Text = labelMessage
                        };

                        if (isToolTipMessage)
                            lblMessage.Pty.TooltipContent = toolTipMessage;
                    }

                    retControls = new WebControl[2];
                    retControls[0] = btnMessage;
                    retControls[1] = lblMessage;
                }
                return retControls;
            }
            #endregion CreateInfo
            #endregion Methods
        }
        #endregion ParameterRow

        #region Members
        protected string _title;
        protected string _mainMenuClassName;
        #endregion Members

        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageConstruction();
        }
        #endregion OnInit
        #region GenerateHtmlForm
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            AddToolBar();
            AddBody();
        }
        #endregion GenerateHtmlForm

        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + _mainMenuClassName };
            WCToolTipLinkButton btnProcess = ControlsTools.GetAwesomeButtonAction("btnProcess", "fa fa-caret-square-right", true);
            btnProcess.Click += new EventHandler(this.OnProcessClick);
            pnlToolBar.Controls.Add(btnProcess);
            CellForm.Controls.Add(pnlToolBar);
        }

        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + _mainMenuClassName };
            AddDetailControls(pnlBody);
            AddDetailParameters(pnlBody);
            CellForm.Controls.Add(pnlBody);
        }

        #region PageConstruction
        // EG 20160404 Migration vs2013
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void PageConstruction()
        {
            // RD 20130321 / Ne pas afficher les ressources sur les CheckBox
            AbortRessource = true;
            string idMenu = Request.QueryString["IDMenu"];
            _mainMenuClassName = ControlsTools.MainMenuName(idMenu);

            string _titleLeft = Ressource.GetMenu_Fullname(idMenu, idMenu);
            string _titleRight = string.Empty;
            string _footerRight = string.Empty;

            HtmlPageTitle titleLeft = new HtmlPageTitle(_titleLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle(_titleRight, null, _footerRight);
            PageTitle = Ressource.GetString("ProcessBase", null, true) + ": " + _titleLeft;
            GenerateHtmlForm();

            FormTools.AddBanniere(this, Form, titleLeft, titleRight, string.Empty, idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null);
        }
        #endregion PageConstruction

        #region OnProcessClick
        protected virtual void OnProcessClick(object sender, System.EventArgs e)
        {
        }
        #endregion OnProcessClick
        #region OnCloseClick
        protected void OnCloseClick(object sender, System.EventArgs e)
        {
            ClosePage();
        }
        #endregion OnCloseClick

        #region AddDetailControls
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected virtual void AddDetailControls(Panel pPanelParent)
        {
        }
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected virtual void AddDetailParameters(Panel pPanelParent)
        {
            WCTogglePanel togglePanel = new WCTogglePanel()
            {
                ID = "divParameters",
                CssClass = CSSMode + " " + _mainMenuClassName,
                Visible = false
            };
            togglePanel.SetHeaderTitle(Ressource.GetString("OTC_ADM_TOOL_IO_TASK_PARAM"));
            Table tblParameters = HtmlTools.CreateTable();
            tblParameters.ID = "tblParameters";
            togglePanel.AddContent(tblParameters);

            pPanelParent.Controls.Add(togglePanel);
        }
        #endregion AddDetailControls


        #region LoadInfoDatas
        protected void LoadInfoDatas(string pKey)
        {
            LoadInfoDatas(pKey, true, true);
        }
        protected virtual void LoadInfoDatas(string pKey, bool pSetData, bool pEnableViewState)
        {
            ResetInfoDatas(pEnableViewState);

            if (StrFunc.IsFilled(pKey))
            {
                if (this.FindControl("btnProcess") is Button btnProcess)
                    btnProcess.Enabled = true;
            }
        }
        #endregion LoadInfoDatas
        #region ResetInfoDatas
        /// <summary>
        /// Réinitialiser les données de la dernière sélection
        /// </summary>
        /// <param name="pSetData"></param>
        /// <param name="pEnableViewState"></param>
        protected void ResetInfoDatas(bool pEnableViewState)
        {
            if (this.FindControl("tblParameters") is Table tblParameters)
            {
                tblParameters.Controls.Clear();
                tblParameters.EnableViewState = pEnableViewState;
            }
            if (this.FindControl("lblInfoDesc") is WCTooltipLabel lblInfoDesc)
            {
                lblInfoDesc.Text = string.Empty;
                lblInfoDesc.Pty.TooltipContent = string.Empty;
            }
            if (this.FindControl("btnProcess") is Button btnProcess)
                btnProcess.Enabled = false;

            HttpSessionStateTools.Set(SessionTools.SessionState, "Parameters", null);
        }
        #endregion ResetInfoDatas

    }
}

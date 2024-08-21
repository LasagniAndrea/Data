using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Drawing;
using System.Text.RegularExpressions;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

/// EG 20170918 [23342] Suppression Appel WCToolTipImageCalendar
namespace EFS.Common.Web
{
    public partial class CustomObjects
    {
        #region Members
        [System.Xml.Serialization.XmlAttribute()]
        private string bsGrid;
        #endregion Members

        #region property
        #region BsGrid
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsBsGrid
        {
            get { return (!this.BsGrid.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("bs-grid")]
        public string BsGrid
        {
            set { bsGrid = GetValidValue(value); }
            get { return bsGrid; }
        }
        #endregion BsGrid
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IntBsGrid
        {
            get 
            {
                int ret = 0;
                string[] _value = bsGrid.Split('-');
                if (ArrFunc.IsFilled(_value) && (3 == _value.Length))
                    ret = Convert.ToInt32(_value[2]);
                return ret;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string RemainBsGrid
        {
            get { return "col-sm-" + (12 - IntBsGrid).ToString(); }
        }
        #endregion property

        public CustomObjects()
        {
            BsGrid = string.Empty;
        }

        #region Methods
        protected static string GetValidValue(string pValue)
        {
            return GetValidValue(pValue, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pbToLower"></param>
        /// <returns></returns>
        protected static string GetValidValue(string pValue, bool pbToLower)
        {
            string ret = string.Empty;

            if (StrFunc.IsFilled(pValue))
            {
                if (pbToLower)
                    ret = pValue.ToLower();
                else
                    ret = pValue;
            }
            else
            {
                ret = "undefined";
            }
            return ret;
        }

        public Panel CreatePanel(PageBase pPage)
        {
            Panel pnl = new Panel();
            List<CustomObject> lstCustomObjects = customObject.Cast<CustomObject>().ToList();
            if (null != lstCustomObjects)
            {
                Panel pnlRow = null;
                string formGroupClass = "form-group form-group-xs";
                lstCustomObjects.ForEach(item =>
                {
                    if (item.ContainsBsRow)
                    {
                        if (null != pnlRow)
                            pnl.Controls.Add(pnlRow);

                        pnlRow = new Panel();
                        pnlRow.CssClass = "row";
                    }

                    //if (item.BsRow)
                    Panel pnlFormGroup = null;
                    Control ctrl = null;
                    // Le CustomObject a un label 
                    // => si <> de CustomObjectLabel et CustomObject alors création du Label (cela évite à définir un CustomObjectLabel dans le fichier GUI
                    if (item.ContainsCaption)
                    {
                        if ((item.Control != CustomObject.ControlEnum.label) && (item.Control != CustomObject.ControlEnum.checkbox))
                        {
                            CustomObject co = new CustomObjectLabel();
                            co.ClientId = item.ClientId;
                            co.Caption = item.Caption;
                            ctrl = co.WriteControl(pPage, false);
                            if (null != ctrl)
                            {
                                pnlFormGroup = new Panel();
                                pnlFormGroup.CssClass = formGroupClass;
                                pnlFormGroup.Controls.Add(ctrl);
                                //pnl.Controls.Add(pnlFormGroup);

                                if (null != pnlRow)
                                    pnlRow.Controls.Add(pnlFormGroup);
                                else
                                    pnl.Controls.Add(pnlFormGroup);
                            }
                        }
                    }

                    ctrl = item.WriteControl(pPage, false);
                    if (null != ctrl)
                    {
                        pnlFormGroup = new Panel();
                        pnlFormGroup.CssClass = formGroupClass;
                        pnlFormGroup.Controls.Add(ctrl);
                        //pnl.Controls.Add(pnlFormGroup);
                        if (null != pnlRow)
                            pnlRow.Controls.Add(pnlFormGroup);
                        else
                            pnl.Controls.Add(pnlFormGroup);

                    }

                });
                if (null != pnlRow)
                    pnl.Controls.Add(pnlRow);

            }
            return pnl;
        }
        #endregion Methods
    }

    public partial class CustomObject
    {
        #region Members
        [System.Xml.Serialization.XmlAttribute()]
        private string row;
        [System.Xml.Serialization.XmlAttribute()]
        private string col;

        [System.Xml.Serialization.XmlAttribute()]
        private string typeAheadTable;
        [System.Xml.Serialization.XmlAttribute()]
        private string typeAheadValues;

        #endregion Members

        #region property
        #region Row
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsBsRow
        {
            get { return ((null != Row) && (Row.Equals("1"))); }
        }
        [System.Xml.Serialization.XmlAttribute("row")]
        public string Row
        {
            set { row = GetValidValue(value); }
            get { return row; }
        }
        #endregion Row
        #region Col
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsCol
        {
            get { return ((null != Col) && (false == Col.Equals("undefined"))); }
        }
        [System.Xml.Serialization.XmlAttribute("sm")]
        public string Col
        {
            set { col = GetValidValue(value); }
            get { return col; }
        }
        #endregion Col
        #region TypeAheadTable
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsTypeAheadTable
        {
            get { return ((null != TypeAheadTable) && (false == TypeAheadTable.Equals("undefined"))); }
        }
        [System.Xml.Serialization.XmlAttribute("ta-tbl")]
        public string TypeAheadTable
        {
            set { typeAheadTable = GetValidValue(value); }
            get { return typeAheadTable; }
        }
        #endregion TypeAheadTable
        #region TypeAheadValues
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsTypeAheadValues
        {
            get { return ((null != TypeAheadValues) && (false == TypeAheadValues.Equals("undefined"))); }
        }
        [System.Xml.Serialization.XmlAttribute("ta-val")]
        public string TypeAheadValues
        {
            set { typeAheadValues = GetValidValue(value); }
            get { return typeAheadValues; }
        }
        #endregion TypeAheadValues
        #endregion property

        #region Methods
        public virtual Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {

            Control ctrl = null;
            switch (m_control)
            {
                case ControlEnum.checkbox:
                    ctrl = WriteControlClassicCheckBox(pPage, pIsControlModeConsult);
                    break;
                case ControlEnum.htmlcheckbox:
                    ctrl = WriteControlContentCheckBox(pPage, pIsControlModeConsult);
                    break;
                case ControlEnum.hr:
                    ctrl = WriteHR();
                    break;
            }
            return ctrl;
        }
        private Control WriteControlClassicCheckBox(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteClassicCheckBox() " + clientId);

            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            BS_ClassicCheckBox ctrl = new BS_ClassicCheckBox(IsEnabled, Resource, IsAutoPostBack);
            ctrl.ID = clientId;

            if (isReadOnly)
                ctrl.chk.TabIndex = -1;

            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;

                    ctrl.chk.Attributes.Add(GetPostBackEvent(), "if (Page_ClientValidate()) " + GetPostBackMethod() + "('" + pPage.ContentPlaceHolder_ClientID + ctrl.chk.ClientID + "','" + arg + "');");
                }
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                ctrl.chk.Attributes.Add("onclick", "__doPostBack('" + pPage.ContentPlaceHolder_ClientID + ctrl.chk.ClientID + "','match');");
                SetLookMatchDefaultValue(ctrl.chk);
            }

            if (ContainsToolTip)
                ctrl.chk.ToolTip = ResourceToolTip;

            if (ContainsStyle)
                ControlsTools.SetStyleList(ctrl.chk.Style, Style);

            CustomObjectTools.SetVisibilityStyle(ctrl.chk, this, pIsControlModeConsult);

            if (ContainsWidth)
            {
                ctrl.chk.Width = UnitWidth;
                ctrl.Width = UnitWidth;
            }

            if (ContainsHeight)
                ctrl.chk.Height = UnitHeight;

            if (pPage.isDebugClientId)
                ctrl.chk.ToolTip = ctrl.chk.ID;

            if (ContainsDefaultValue)
                ctrl.chk.Checked = BoolFunc.IsTrue(DefaultValue);

            return ctrl;
        }
        private Control WriteControlCheckBox(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteCheckBox() " + clientId);

            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            BS_CheckBox chk = new BS_CheckBox();
            chk.isReadOnly = isReadOnly;
            chk.ID = clientId;
            chk.CssClass = "switch";
            chk.Width = Unit.Percentage(100);
            chk.Text = Resource;
            chk.Enabled = IsEnabled;

            if (isReadOnly)
                chk.TabIndex = -1;

            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;

                    chk.Attributes.Add(GetPostBackEvent(), "if (Page_ClientValidate()) " + GetPostBackMethod() + "('" + pPage.ContentPlaceHolder_ClientID + chk.ClientID + "','" + arg + "');");
                }
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                chk.Attributes.Add("onclick", "__doPostBack('" + pPage.ContentPlaceHolder_ClientID + chk.ClientID + "','match');");
                SetLookMatchDefaultValue(chk);
            }

            if (ContainsToolTip)
                chk.ToolTip = ResourceToolTip;

            if (ContainsStyle)
                ControlsTools.SetStyleList(chk.Style, Style);

            CustomObjectTools.SetVisibilityStyle(chk, this, pIsControlModeConsult);

            if (ContainsWidth)
                chk.Width = UnitWidth;

            if (ContainsHeight)
                chk.Height = UnitHeight;

            if (pPage.isDebugClientId)
                chk.ToolTip = chk.ID;

            if (ContainsDefaultValue)
                chk.Checked = BoolFunc.IsTrue(DefaultValue);

            return chk;
        }
        private Control WriteControlContentCheckBox(PageBase pPage, bool pIsControlModeConsult)
        {
            Control chk = WriteControlHtmlInputCheckBox(pPage, pIsControlModeConsult);
            BS_ContentCheckBox contentCheckBox = new BS_ContentCheckBox(chk);
            return contentCheckBox;
        }
        private Control WriteControlHtmlInputCheckBox(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("HtmlInputCheckBox() " + clientId);

            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);
            bool isWithLabel = GetMiscValue("label", false);

            int lblWidth = 0;
            if ((isWithLabel) && (ContainsWidth))
                lblWidth = Convert.ToInt32(UnitWidth.Value) - 40;
            BS_HtmlCheckBox chk = new BS_HtmlCheckBox(isWithLabel, lblWidth);
            chk.ID = clientId;
            chk.isReadOnly = isReadOnly;
            chk.CssClass = "switch";
            chk.Text = Resource;
            chk.Enabled = IsEnabled;

            if (ContainsStyle)
                ControlsTools.SetStyleList(chk.Style, Style);

            CustomObjectTools.SetVisibilityStyle(chk, this, pIsControlModeConsult);

            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;

                    chk.Attributes.Add(GetPostBackEvent(), GetPostBackMethod() + "('" + pPage.ContentPlaceHolder_ClientID + chk.ClientID + "','" + arg + "');");
                }
            }

            if (ContainsToolTip)
                chk.ToolTip = ResourceToolTip;

            if (pPage.isDebugClientId)
                chk.ToolTip = chk.ID;

            return chk;
        }
        private Control WriteControlHR()
        {
            Panel pnl = new Panel();
            pnl.CssClass = "col-sm-12";
            LiteralControl hr = new LiteralControl("<hr/>");
            pnl.Style.Add(HtmlTextWriterStyle.Height, "1px");
            pnl.Controls.Add(hr);
            return pnl;
        }


        #region GetValidators
        public List<Validator> GetValidators()
        {
            List<Validator> lstValidators = new List<Validator>();

            #region RequireField
            if (IsMandatory)
                lstValidators.Add(new Validator(Ressource.GetString("MissingData"), true, false));
            #endregion RequireField

            #region Regular Expression
            if (ContainsRegEx && (EFSRegex.TypeRegex.None != RegexValue))
                lstValidators.Add(new Validator(RegexValue, Ressource.GetString("InvalidData"), true, false));
            #endregion Regular Expression

            #region ValidationDataType
            if ((false == ContainsRegEx) && GetMiscValue("isSetValidatorType", true))
            {
                ValidationDataType validationType = ValidationDataType.String;
                if (TypeData.IsTypeDate(DataType))
                    validationType = ValidationDataType.Date;
                else if (TypeData.IsTypeDec(DataType))
                    validationType = ValidationDataType.Double;
                else if (TypeData.IsTypeInt(DataType))
                    validationType = ValidationDataType.Integer;
                else if (TypeData.IsTypeString(DataType))
                    validationType = ValidationDataType.String;
                lstValidators.Add(new Validator(validationType, Ressource.GetString("InvalidData"), true, false));
            }
            #endregion ValidationDataType

            #region CustomValidator
            if (TypeData.IsTypeDate(DataType))
                lstValidators.Add(Validator.GetValidatorDateRange(Ressource.GetString("Msg_InvalidDate"), Resource));
            #endregion CustomValidator

            return lstValidators;
        }
        #endregion GetValidators
        #endregion Method
    }

    public partial class CustomObjectLabel
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteLabel() " + CtrlClientId);
            return WriteControlLabel(pPage, pIsControlModeConsult);
        }
        private Control WriteControlLabel(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;

            BS_Label lbl = new BS_Label();
            lbl.ID = clientId;
            lbl.Text = Resource;
            if (ContainsCssClass)
                lbl.CssClass = CssClass;
            lbl.Enabled = IsEnabled;
            if (ContainsWidth)
                lbl.Width = UnitWidth;
            if (ContainsHeight)
                lbl.Height = UnitHeight;

            if (ContainsStyle)
                ControlsTools.SetStyleList(lbl.Style, Style);

            CustomObjectTools.SetVisibilityStyle(lbl, this, pIsControlModeConsult);

            // ToolTip
            if (pPage.isDebugClientId)
                lbl.ToolTip = lbl.ClientID;
            else if (ContainsToolTip)
                lbl.ToolTip = ResourceToolTip;
            return lbl;
        }        
        #endregion Methods
    }

    public partial class CustomObjectTextBox : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteControlTextBox() " + CtrlClientId);
            return WriteControlTextBox(pPage, pIsControlModeConsult);
        }
        private Control WriteControlTextBox(PageBase pPage, bool pIsControlModeConsult)
        {
            string dataType = DataType;
            bool isTextBoxDate = (TypeData.IsTypeDateOrDateTime(dataType) || TypeData.IsTypeTime(dataType));
            bool isReadOnly = pIsControlModeConsult || GetMiscValue("IsReadOnly", pIsControlModeConsult);

            #region Validator
            List<Validator> lstValidators = null;
            if (false == isReadOnly)
            {
                lstValidators = GetValidators();
            }
            #endregion Validator

            #region TextBox
            BS_TextBoxDate txtBoxDate = null;
            BS_TextBox txt = null;
            if (isTextBoxDate)
            {
                txtBoxDate = new BS_TextBoxDate(CtrlClientId, isReadOnly, dataType, lstValidators);
                txt = txtBoxDate.Date;
            }
            else
            {
                txt = new BS_TextBox(CtrlClientId, isReadOnly, lstValidators);
            }

            txt.Enabled = IsEnabled;

            if (ContainsHeight)
                txt.Height = UnitHeight;

            if (this.ContainsAccessKey)
                txt.AccessKey = (AccessKey == "NextBlock") ? SystemSettings.GetAppSettings("Spheres_NextBlock_AccessKey", "N") : AccessKey;

            if (isReadOnly)
                txt.TabIndex = -1;

            if (ContainsWidth)
                txt.Width = UnitWidth;

            if (ContainsHeight)
                txt.Width = UnitHeight;

            if (ContainsToolTip)
                txt.ToolTip = ResourceToolTip;

            string fullClientID = pPage.ContentPlaceHolder_ClientID + txt.ClientID;

            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;
                    txt.Attributes.Add(GetPostBackEvent(), "if (Page_ClientValidate())" + GetPostBackMethod() + "('" + fullClientID + "','" + arg + "');");
                }
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                txt.Attributes.Add("onClick", "__doPostBack('" + fullClientID + "','match');");
                SetLookMatchDefaultValue(txt);
            }

            if (ContainsStyle)
                ControlsTools.SetStyleList(txt.Style, Style);

            if (ContainsAttributes)
                ControlsTools.SetAttributesList(txt.Attributes, Attributes);

            #endregion TextBox

            return (isTextBoxDate? (Control)txtBoxDate : txt);

        }
        #endregion Methods
    }

    public partial class CustomObjectDropDown : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {

            Control ctrl = null;
            switch (Control)
            {
                case ControlEnum.htmlselect:
                case ControlEnum.dropdown:
                case ControlEnum.ddlbanner:
                case ControlEnum.optgroupdropdown:
                    ctrl = WriteControlDropDown(pPage, pIsControlModeConsult);
                    break;
                default:
                    ctrl = null;
                    break;
            }
            return ctrl;

        }
        private Control WriteControlDropDown(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteControlDropDown() " + clientId);

            string cssClass = EFSCssClass.DropDownListCapture;
            if (isModeConsult)
                cssClass = EFSCssClass.CaptureConsult;
            if (ContainsCssClass)
                cssClass = CssClass;

            BS_DropDownList ddl = new BS_DropDownList(isReadOnly);
            ddl.ID = CtrlClientId;
            ddl.Enabled = IsEnabled;

            if (isReadOnly)
                ddl.TabIndex = -1;

            if (ContainsHeight)
                ddl.Height = UnitHeight;

            if (ContainsWidth)
                ddl.Width = UnitWidth;

            ddl.IsSetTextOnTitle = true;

            SetDropDownLoaded(isReadOnly);

            if (isDropDownLoaded)
            {
                ControlsTools.DDLLoad_FromListRetrieval(pPage, ddl, CSTools.SetCacheOn(pPage.CS), ListRetrieval, !IsMandatory, ContainsMisc ? Misc : string.Empty);
            }

            string fullClientID = pPage.ContentPlaceHolder_ClientID + ddl.ClientID;
            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;
                    ddl.Attributes.Add(GetPostBackEvent(), GetPostBackMethod() + "('" + fullClientID + "','" + arg + "');");
                }
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                ddl.txtViewer.Attributes.Add("onClick", "__doPostBack('" + fullClientID + "','match');");
                SetLookMatchDefaultValue(ddl.txtViewer);
            }


            if (ContainsAttributes)
                ControlsTools.SetAttributesList(ddl.Attributes, Attributes);

            if (ContainsStyle)
                ControlsTools.SetStyleList(ddl.Style, Style);

            CustomObjectTools.SetVisibilityStyle(ddl, this, isModeConsult);

            ddl.SelectedIndex = 0;
            return ddl;
        }
        #endregion Methods
    }

    public partial class CustomObjectTypeAhead : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            return WriteControlTypeAhead(pPage, pIsControlModeConsult);
        }
        private Control WriteControlTypeAhead(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;

            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteControlTypeAhead() " + clientId);

            PlaceHolder plhTypeAhead = new PlaceHolder();

            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            BS_TextBox txt = new BS_TextBox();
            txt.ID = CtrlClientId;

            // TO DO
            plhTypeAhead.Controls.Add(txt);
            return plhTypeAhead;
        }
        #endregion Methods
    }

    public partial class CustomObjectButton : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteControlButton() " + CtrlClientId);

            return WriteControlButton(pPage, pIsControlModeConsult);
        }
        private Control WriteControlButton(PageBase pPage, bool pIsControlModeConsult)
        {
            WCToolTipButton btn = new WCToolTipButton();
            if (CSS.IsCssClassMain(CssClass))
                btn.Text = string.Empty;
            btn.CausesValidation = false;
            btn.OnClientClick = "return false;";
            btn.ID = CtrlClientId;
            btn.CssClass = CssClass;

            if (ContainsWidth)
                btn.Width = UnitWidth;
            if (ContainsHeight)
                btn.Height = UnitHeight;

            if (ContainsToolTip)
                btn.Pty.TooltipContent = ResourceToolTip;

            if (ContainsToolTipTitle)
                btn.Pty.TooltipTitle = ResourceToolTipTitle;

            if (ContainsStyle)
                ControlsTools.SetStyleList(btn.Style, Style);

            CustomObjectTools.SetVisibilityStyle(btn, this, pIsControlModeConsult);
            btn.TabIndex = -1;

            return btn;
        }
        #endregion Methods
    }

    public partial class CustomObjectPanel : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteButton() " + CtrlClientId);

            return WriteControlPanel(pPage, pIsControlModeConsult);
        }
        private Control WriteControlPanel(PageBase pPage, bool pIsControlModeConsult)
        {
            WCToolTipPanel pnl = new WCToolTipPanel();
            pnl.ID = CtrlClientId;
            pnl.CssClass = CssClass;

            if (ContainsToolTip)
                pnl.Pty.TooltipContent = ResourceToolTip;

            if (ContainsToolTipTitle)
                pnl.Pty.TooltipTitle = ResourceToolTipTitle;

            if (ContainsStyle)
                ControlsTools.SetStyleList(pnl.Style, Style);

            if (ContainsWidth)
                pnl.Width = UnitWidth;
            if (ContainsHeight)
                pnl.Height = UnitHeight;

            CustomObjectTools.SetVisibilityStyle(pnl, this, pIsControlModeConsult);
            pnl.TabIndex = -1;
            pnl.Style.Add("outline", "transparent");
            return pnl;

        }
        #endregion Methods
    }

    public partial class CustomObjectImage : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteImage() " + CtrlClientId);
            return WriteControlImage(pPage, pIsControlModeConsult);
        }
        private Control WriteControlImage(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            bool isModeConsult = pIsControlModeConsult;

            System.Web.UI.WebControls.Image img = new WCToolTipImage();
            img.ID = clientId;
            img.ImageUrl = Source;
            img.ImageAlign = ImageAlign.Left;
            ((WCToolTipImage)img).Pty.TooltipContent = ContainsToolTip ? ResourceToolTip : img.AlternateText;
            if (ContainsToolTipTitle)
                ((WCToolTipImage)img).Pty.TooltipTitle = ResourceToolTipTitle;

            if (ContainsWidth)
                img.Width = UnitWidth;

            if (ContainsHeight)
                img.Height = UnitHeight;

            if (ContainsStyle)
                ControlsTools.SetStyleList(img.Style, Style);

            CustomObjectTools.SetVisibilityStyle(img, this, pIsControlModeConsult);
            img.TabIndex = -1;

            return img;
        }
        #endregion Methods
    }

    public partial class CustomObjectDDLBanner : CustomObjectDropDown, ICustomObjectOpenBanner
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            return WriteControlDDLBanner(pPage, pIsControlModeConsult);
        }
        private Control WriteControlDDLBanner(PageBase pPage, bool pIsControlModeConsult)
        {
            Control ctrl = base.WriteControl(pPage, pIsControlModeConsult);
            ControlsTools.SetAttributesList(((WebControl)ctrl).Attributes, "isddlbanner:true");
            return ctrl;
        }
        #endregion Methods
    }

    public partial class CustomObjectHyperLink : CustomObject
    {
        #region Methods
        public override Control WriteControl(PageBase pPage, bool pIsControlModeConsult)
        {
            if (pPage.isDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteControlHyperLink() " + CtrlClientId);
            return WriteControlHyperLink(pPage, pIsControlModeConsult);
        }
        private Control WriteControlHyperLink(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            bool isModeConsult = pIsControlModeConsult;

            WCToolTipHyperlink lnk = new WCToolTipHyperlink();
            lnk.ID = CtrlClientId;
            lnk.Text = Resource;
            lnk.NavigateUrl = NavigateUrl;
            lnk.ImageUrl = ImageUrl;
            lnk.Enabled = IsEnabled;

            if (ContainsToolTip)
            {
                lnk.Pty.TooltipTitle = Resource;
                lnk.Pty.TooltipContent = ResourceToolTip;
            }

            if (ContainsCssClass)
                lnk.CssClass = CssClass;

            if (ContainsStyle)
                ControlsTools.SetStyleList(lnk.Style, Style);

            lnk.Visible = true;

            if (pPage.isDebugClientId)
                lnk.Pty.TooltipContent = lnk.ID;

            lnk.Width = Unit.Pixel(10);
            if (ContainsWidth)
                lnk.Width = UnitWidth;

            if (ContainsHeight)
                lnk.Height = UnitHeight;

            return lnk;
        }
        #endregion Methods
    }
}


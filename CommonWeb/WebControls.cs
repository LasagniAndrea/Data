using EFS.ACommon;
using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Tz = EFS.TimeZone;

// EG 20170918 [23342] Suppression WCToolTipImageCalendar et WCImgCalendar
// EG 20234429 [WI756] Spheres Core : Refactoring Code Analysis
namespace EFS.Common.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class WebControlType
    {
        /// <summary>
        /// 
        /// </summary>
        public enum WebControlTypeEnum
        {
            TextBox,
            CheckBox,
            DropDown,
            TableBegin,
            TableEnd,
            Banner,
            DDLBanner,
            Br,
            Hr,
            Space,
            Unknown
        }
    }

    #region Enum for WCRoundedTable With IMage
    public enum RoundedTableColorEnum
    {
        Blue,
        Green,
        Red,
        Yellow,
    }
    #endregion Enum for WCRoundedTable With IMage

    /// <summary>
    /// 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCTable : Table, INamingContainer
    {
    }

    /// <summary>
    /// 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCCheckBox2 : CheckBox
    {
        #region Members
        private bool _isReadOnly;
        #endregion

        #region Accessor
        #region isReadOnly
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        #endregion
        #endregion

        #region constructor
        public WCCheckBox2()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            //FI 20091106 [16722] Lorsque la check est initialement activé (enabled=true), on rentre dans ce code afin de lire les données existantes sur le client
            //Interrogation de form.request
            //Si le control est _isReadOnly=true cad Enabled=false
            //=> il ne faut pas tenter de lire les données du client car elles ne sont pas disponibles puisque sur le client le contrôle est disabled
            //
            if (!_isReadOnly)
                return base.LoadPostData(postDataKey, postCollection);
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            //
            //Attributes.Add("isReadOnly",_isReadOnly.ToString());
            //FI 20091106 [16722]
            //if (_isReadOnly)
            //    Attributes.Add("readonly", "readonly");
            //
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            Enabled = Enabled && (false == _isReadOnly);
            //
            base.Render(writer);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }
        #endregion

    }
    
    /// <summary>
    /// 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public class WCHtmlCheckBox2 : HtmlInputCheckBox
    {
        #region Member
        private readonly Label _label;
        private bool _isReadOnly;
        #endregion
        //
        #region Accessor
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        public bool ExistLabel
        {
            get { return (null != _label); }
        }
        public override string ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = value;
                if (this.ExistLabel)
                    _label.ID = value.Replace(Cst.HCK, Cst.LBL);
            }
        }
        public string Text
        {
            set { if (this.ExistLabel) _label.Text = value; }
            get { if (this.ExistLabel) return _label.Text; else	return string.Empty; }
        }

        public string CssClass
        {
            set { this.Attributes.Add("class", value); }
            get { return this.Attributes["class"].ToString(); }
        }

        public Boolean Enabled
        {
            set { this.Disabled = !value; }
            get { return (!this.Disabled); }
        }

        public string ToolTip
        {
            set { if (ExistLabel) _label.ToolTip = value; }
            get { if (ExistLabel) return (_label.ToolTip); else return string.Empty; }
        }
        #endregion
        
        #region Constructor
        public WCHtmlCheckBox2() : this(false, 0) { }
        public WCHtmlCheckBox2(bool pWithLabel) : this(pWithLabel, 0) { }
        public WCHtmlCheckBox2(bool pWithLabel, int pLblWidth)
        {
            if (pWithLabel)
            {
                _label = new Label();
                if (0 != pLblWidth)
                    _label.Width = Unit.Pixel(pLblWidth);
            }
            EnsureChildControls();
            //FI 20091106 [16722]
            //_isReadOnly = pIsReadOnly;
        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            if (this.ExistLabel)
            {
                _label.Text = this.Text + Cst.HTMLSpace;
                _label.Visible = true;
            }
            //FI 20091106 [16722]
            //Attributes.Add("isReadOnly",_isReadOnly.ToString());
            //if (_isReadOnly)
            //    Attributes.Add("readonly", "readonly");
            //
            base.CreateChildControls();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.ExistLabel)
                _label.RenderControl(writer);
            //
            Enabled = Enabled && (false == _isReadOnly);
            //
            base.Render(writer);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            //FI 20091106 [16722] Lorsque la check est initialement activé (enabled=true), on rentre dans ce code afin de lire les données existantes sur le client
            //Interrogation de form.request
            //Si le control est _isReadOnly=true => il ne faut pas lire les données du client car elles ne sont pas disponibles
            if (!_isReadOnly)
                return base.LoadPostData(postDataKey, postCollection);
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }
        #endregion
    }
    
    /// <summary>
    /// TextBox and Validator Controls
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCTextBox : TextBox
    {

        #region Members
        public string regularExpression = string.Empty;
        private readonly bool showSummary = false;
        private readonly string typeRange = string.Empty;
        private readonly string minValue;
        private readonly string maxValue;
        private readonly string customFunction = string.Empty;
        private readonly string msgErrRequiredField = string.Empty;
        private readonly string msgErr = string.Empty;
        #endregion

        #region Constructeur
        public WCTextBox() { }
        public WCTextBox(string pID,
            string pMsgErrRequiredField, string pRegularExpression, bool pShowSummary, string pMsgErr)
        {
            ID = pID;
            msgErrRequiredField = pMsgErrRequiredField;
            regularExpression = pRegularExpression;
            showSummary = pShowSummary;
            msgErr = pMsgErr;
        }
        #endregion

        #region protected override CreateChildControls
        // EG 20201008 [XXXXX] Ajour CssClass pour Gestion de l'affichage des validateurs lorsqu'ils sont activés (Sous la contrôle associé)
        protected override void CreateChildControls()
        {
            if (StrFunc.IsFilled(msgErrRequiredField))
            {
                RequiredFieldValidator rqv = new RequiredFieldValidator
                {
                    ControlToValidate = this.ID,
                    CssClass = "vtor",
                    ErrorMessage = msgErrRequiredField
                };
                if (showSummary)
                    rqv.Display = ValidatorDisplay.Dynamic;
                this.Controls.Add(rqv);
            }
            if (StrFunc.IsFilled(regularExpression))
            {
                RegularExpressionValidator rev = new RegularExpressionValidator
                {
                    ControlToValidate = this.ID,
                    CssClass = "vtor",
                    ErrorMessage = msgErr,
                    ValidationExpression = regularExpression
                };
                if (showSummary)
                    rev.Display = ValidatorDisplay.Dynamic;
                this.Controls.Add(rev);
            }
            if (StrFunc.IsFilled(typeRange))
            {
                RangeValidator rv = new RangeValidator
                {
                    ControlToValidate = this.ID,
                    CssClass = "vtor",
                    ErrorMessage = msgErr,
                    MinimumValue = minValue,
                    MaximumValue = maxValue,
                };
                //TODO type pris en compte pour RangeValidator ( Integer, String et Date ) 
                if (typeRange.Equals("integer"))
                    rv.Type = ValidationDataType.Double;
                if (typeRange.Equals("string"))
                    rv.Type = ValidationDataType.String;
                if (typeRange.Equals("date"))
                    rv.Type = ValidationDataType.Date;
                if (showSummary)
                    rv.Display = ValidatorDisplay.None;
                this.Controls.Add(rv);
            }
            if (StrFunc.IsFilled(customFunction))
            {
                CustomValidator cv = new CustomValidator
                {
                    ControlToValidate = this.ID,
                    CssClass = "vtor",
                    ErrorMessage = msgErr
                };

                if (showSummary)
                    cv.Display = ValidatorDisplay.None;
                this.Controls.Add(cv);
            }
        }

        #endregion
        #region protected override void OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        #endregion
        #region protected override LoadPostData
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return base.LoadPostData(postDataKey, postCollection);
        }
        #endregion
        #region protected override LoadViewState
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }
        #endregion

        #region protected override Render
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            RenderChildren(writer);
        }
        #endregion
    }
    
    /// <summary>
    /// Représente une zone de saisie (TextBox)
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCTextBox2 : TextBox
    {
        
        #region Members
        private readonly ArrayList aValidator;
        #endregion

        #region Constructors
        public WCTextBox2() { }
        public WCTextBox2(string pId, params Validator[] pValidator) :
            this(pId, false, false, false, EFSCssClass.Capture, pValidator) { }
        //
        public WCTextBox2(string pId, bool pIsReadOnly, bool pIsMandatory, bool pIsNumeric, params Validator[] pValidator) :
            this(pId,  pIsReadOnly, pIsMandatory, pIsNumeric, EFSCssClass.Capture, pValidator) { }
        
        public WCTextBox2(string pId, bool pIsReadOnly, bool pIsMandatory,bool pIsNumeric, string pCssClass, params Validator[] pValidator)
        {
            
            ID = pId;
            CssClass = EFSCssClass.GetCssClass(pIsNumeric, pIsMandatory, false, pIsReadOnly, pCssClass);
            ReadOnly = pIsReadOnly;

            aValidator = new ArrayList();
            if (ArrFunc.IsFilled(pValidator))
            {
                for (int i = 0; i < pValidator.Length; i++)
                    AddValidator(pValidator[i]);
            }
            
            EnsureChildControls();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Attributes.Add("oldvalue", string.Empty);
            if (ArrFunc.IsFilled(aValidator))
            {
                for (int i = 0; i < aValidator.Count; i++)
                {
                    Validator validator = (Validator)aValidator[i];
                    this.Controls.Add(validator.CreateControl(this.ID));
                }
            }
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            //20071212 FI Ticket 16012 => Migration Asp2.0
            foreach (WebControl ctrl in this.Controls)
                ((BaseValidator)ctrl).ControlToValidate = this.ID;
            
            base.OnPreRender(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return base.LoadPostData(postDataKey, postCollection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            
            base.Render(writer);
            //
            //20071212 FI Ticket 16012 => Migration Asp2.0
            //foreach (WebControl ctrl in this.Controls)
            //	((BaseValidator) ctrl).ControlToValidate=this.ID;
            //            
            RenderChildren(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value + string.Empty;
                Attributes["oldvalue"] = this.Text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValidator"></param>
        public void AddValidator(Validator pValidator)
        {
            if (null != pValidator)
                aValidator.Add(pValidator);
        }
        #endregion

    }

    /// <summary>
    /// Représente une zone de saisie (TextBox (Date) + TextBox (Time) )
    /// </summary>
    /// EG 20170822 [23342] New
    /// EG 20170926 [22374] Upd
    /// EG 20170929 [23450][22374] Upd
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCZonedDateTime : TextBox
    {
        #region Members
        private readonly string dataType;
        private readonly string cssClass;
        private Label label;
        private readonly bool isLabelTop;
        private readonly bool isAltTime;
        private readonly bool isToolTipZone;
        private readonly bool isFreeZone;
        private string labelText;
        private readonly ArrayList aValidator;
        private Tz.ZonedDateTimeExtended zdt;

        private TextBox txtTime;
        private readonly TextBox txtOffset;
        private readonly DropDownList ddlZone;
        private Label lblZone;

        private WCTooltipCommonProperties m_Pty;

        private Pair<string, string> postedZonedDateTimeValue;
        #endregion Members

        #region Accessors
        /// <summary>
        /// css Class applied to controls when it is not on ReadOnly mode (used by Jquery)  
        /// </summary>
        /// FI 20240528 Add 
        public string CssClassEdit
        {
            get;
            private set;
        }

        // EG 20171025 [23509] New
        public string LabelText
        {
            get { return labelText; }
            set { labelText = value; }
        }
        /// <summary>
        /// ZonedDateTime (NodaTime)
        /// </summary>
        public Tz.ZonedDateTimeExtended Zdt
        {
            get { return zdt; }
        }

        /// <summary>
        /// Label de la date
        /// </summary>
        public Label Label
        {
            get { return label; }
        }
        /// <summary>
        /// Heure si séparée de la date
        /// </summary>
        public TextBox Time
        {
            get { return txtTime; }
        }
        /// <summary>
        /// Heure si séparée de la date
        /// </summary>
        public TextBox Offset
        {
            get { return txtOffset; }
        }
        /// <summary>
        /// TimeZone
        /// </summary>
        public DropDownList Zone
        {
            get { return ddlZone; }
        }
        /// <summary>
        /// Indique si Date et heure sont sur des contrôles séparés
        /// </summary>
        public bool IsAltTime
        {
            get { return isAltTime; }
        }
        /// <summary>
        /// Indique si Affichage des conversion de zone (UTC, ISO8601 et Locale)
        /// </summary>
        public bool IsToolTipZone
        {
            get { return isToolTipZone; }
        }
        /// <summary>
        /// Indique si le timeZone est libre.
        /// Non dépendant du marché, de la party etc.
        /// Si Free alors DDL visible et enabled
        /// Sinon affichage de la zone dans un label.
        /// </summary>
        public bool IsFreeZone
        {
            get { return isFreeZone; }
        }
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Accessors

        #region Constructors
        public WCZonedDateTime() { }
        public WCZonedDateTime(string pId, string pDatatype, bool pIsAltTime, bool pIsFreeZone, bool pIsToolTipZone, bool pIsReadOnly, bool pIsMandatory, params Validator[] pValidator) :
            this(pId, string.Empty, false, pDatatype, pIsAltTime, pIsFreeZone, pIsToolTipZone, pIsReadOnly, pIsMandatory, EFSCssClass.Capture, pValidator) { }
        public WCZonedDateTime(string pId, string pDatatype, bool pIsAltTime, bool pIsFreeZone, bool pIsToolTipZone, bool pIsReadOnly, bool pIsMandatory, string pCssClass, params Validator[] pValidator) :
            this(pId, string.Empty, false, pDatatype, pIsAltTime, pIsFreeZone, pIsToolTipZone, pIsReadOnly, pIsMandatory, pCssClass, pValidator) { }
        public WCZonedDateTime(string pId, string pLabelText, bool pIsLabelTop, string pDatatype, bool pIsAltTime, bool pIsFreeZone, bool pIsToolTipZone, bool pIsReadOnly, bool pIsMandatory, Validator[] pValidator) :
            this(pId, pLabelText, pIsLabelTop, pDatatype, pIsAltTime, pIsFreeZone, pIsToolTipZone, pIsReadOnly, pIsMandatory, EFSCssClass.Capture, pValidator) { }
        // EG 20171031 [23509] Upd
        public WCZonedDateTime(string pId, string pLabelText, bool pIsLabelTop, string pDatatype, bool pIsAltTime, bool pIsFreeZone, bool pIsToolTipZone, bool pIsReadOnly, bool pIsMandatory, string pCssClass, Validator[] pValidator)
        {
            //this.AutoPostBack = true;
            aValidator = new ArrayList();
            ID = pId;
            cssClass = EFSCssClass.GetCssClass(false, pIsMandatory, false, pIsReadOnly, pCssClass);
            labelText = pLabelText;
            isLabelTop = pIsLabelTop;
            isAltTime = pIsAltTime;
            isToolTipZone = pIsToolTipZone;
            isFreeZone = pIsFreeZone;
            ReadOnly = pIsReadOnly;
            dataType = pDatatype;
            CssClass = cssClass;
            if (ArrFunc.IsFilled(pValidator))
            {
                for (int i = 0; i < pValidator.Length; i++)
                    AddValidator(pValidator[i]);
            }

            zdt = new Tz.ZonedDateTimeExtended();

            txtOffset = new TextBox
            {
                ID = this.ID + "_Offset",
                Width = Unit.Pixel(32),
                CssClass = "picker-ro",
                ReadOnly = true,
                TabIndex = -1
            };

            ddlZone = new DropDownList
            {
                ID = this.ID + "_Zone",
                CssClass = "timezone " + EFSCssClass.DropDownListCapture
            };
            // FI 20220921 [XXXXX] Ne pas charger la DLL si elle n'est pas saisissable
            if (!(this.ReadOnly || (false == isFreeZone)))
            {
                ddlZone.Attributes.Add("onchange", "__doPostBack('" + ddlZone.UniqueID + "','');");
                Tz.Web.LoadTzdbIdByWindowsIdToListControl(ddlZone);
            }
            EnsureChildControls();
        }
        #endregion Constructors

        #region Events
        #region OnPreRender
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// EG 20170929 [23450][22374] Upd
        protected override void OnPreRender(EventArgs e)
        {
            foreach (WebControl ctrl in this.Controls)
                ((BaseValidator)ctrl).ControlToValidate = this.ID;

            ApplyPostedValue();
            SetZonedDateTime();

            base.OnPreRender(e);
        }
        #endregion OnPreRender
        #region Render
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (null != label)
                label.RenderControl(writer);

            base.Render(writer);

            if (null != txtTime)
                txtTime.RenderControl(writer);

            if (this.zdt.IsDateFilled)
            {
                if (isToolTipZone)
                {
                    m_Pty = new WCTooltipCommonProperties
                    {
                        TooltipTitle = "Time zones",
                        TooltipContent = "ISO8601 : " + this.Zdt.ISO8601DateTime + Cst.HTMLBreakLine
                    };
                    m_Pty.TooltipContent += "UTC     : " + this.Zdt.UniversalDateTime + Cst.HTMLBreakLine;
                    m_Pty.TooltipContent += "Local   : " + this.Zdt.LocaleDateTime;
                    Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                    m_Pty.Render(writer);
                }
            }

            if (null != ddlZone)
                ddlZone.RenderControl(writer);
            if (null != lblZone)
                lblZone.RenderControl(writer);

            bool isOffset = StrFunc.IsFilled(txtOffset.Text);
            if (null != txtOffset)
            {
                if (isOffset)
                    writer.Write(" (");
                txtOffset.RenderControl(writer);
                if (isOffset)
                    writer.Write(")"); 
            }

            RenderChildren(writer);
        }
        #endregion Render
        #endregion Events

        #region Methods
        #region AddValidator
        /// <summary>
        /// Validateurs du contrôle
        /// </summary>
        /// <param name="pValidator"></param>
        public void AddValidator(Validator pValidator)
        {
            if (null != pValidator)
                aValidator.Add(pValidator);
        }
        #endregion AddValidator
        /// EG 20170929 [23450][22374] New
        /// EG 20171009 [23452] Upd
        public void ApplyPostedValue()
        {
            if (null != postedZonedDateTimeValue)
            {
                if (Tz.Tools.IsDateFilled(postedZonedDateTimeValue.First))
                    Zdt.Parse = postedZonedDateTimeValue.First;
                if (StrFunc.IsFilled(postedZonedDateTimeValue.Second))
                    Zdt.ChangeZone(postedZonedDateTimeValue.Second);
                postedZonedDateTimeValue = null;
            }
        }

        #region CreateChildControls
        /// <summary>
        /// Création de la hierarchie des contrôles pour restituer WCZonedDateTime
        /// </summary>
        // EG 20171031 [23509] Upd 
        protected override void CreateChildControls()
        {
            if (0 < labelText.Length)
            {
                this.label = new Label
                {
                    Text = labelText,
                    Visible = true
                };
                this.label.Text += isLabelTop ? Cst.HTMLBreakLine : Cst.HTMLSpace;
            }

            if (isAltTime)
            {
                this.Width = Unit.Pixel(60);

                txtTime = new TextBox
                {
                    Width = Unit.Pixel(73),
                    ID = this.ID + "_Time",
                    CssClass = "picker-ro",
                    ReadOnly = true,
                    TabIndex = -1
                };

                if (false == this.ReadOnly)
                {
                    CssClassEdit = "DtTimeOffsetPickerAlt";
                    CssClass = $"{CssClassEdit} {cssClass}";
                }
            }
            else
            {
                if (TypeData.IsTypeDateTimeOffset(dataType))
                {
                    if (false == this.ReadOnly)
                    {
                        CssClassEdit = "DtTimeOffsetPicker";
                        CssClass = $"{CssClassEdit} {cssClass}";
                    }
                    this.Width = Unit.Pixel(150);
                }
                else if (TypeData.IsTypeTimeOffset(dataType))
                {
                    if (false == this.ReadOnly)
                    {
                        CssClassEdit = "TimeOffsetPicker";
                        CssClass = $"{CssClassEdit} {cssClass}";
                    }
                    this.Width = Unit.Pixel(77);
                }
            }


            if (this.ReadOnly || (false == isFreeZone))
            {
                // FI 20220921 [XXXXX] ddlZone.Visible = false à la place de display none
                // ddlZone sera asente du flux HTML
                //ddlZone.Style.Add(HtmlTextWriterStyle.Display, "none");
                ddlZone.Visible = false;
                lblZone = new Label
                {
                    ID = Cst.LBL + ID.Substring(Cst.TMS.Length) + "_Zone",
                    CssClass = "picker-ro",
                    Width = ddlZone.Width
                };
            }

            for (int i = 0; i < aValidator.Count; i++)
            {
                Validator validator = (Validator)aValidator[i];
                this.Controls.Add(validator.CreateControl(this.ID));
            }
            Attributes.Add("oldvalue", string.Empty);
            base.CreateChildControls();
        }
        #endregion CreateChildControls
        #region LoadPostData
        /// <summary>
        /// Traitement des données de publication (si modification alors mise à jour dals le OnPreRender de
        /// la variable Zdt (ZonedDateTime) et des contrôles (Date / Timme / Offset et Zone)
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        /// EG 20171009 [23452] Upd
        // EG 20171031 [23509] Upd 
        // EG 20171226 Upd (Add OffsetValue to presentValue and postedValue)
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            postedZonedDateTimeValue = null;
            string presentValue = Text;
            string postedValue = postCollection[this.ClientID] ?? string.Empty;

            // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
            // Lecture par Reflection de la Property : IsStEnvironment_Template
            // La saisie des dates est obligatoire si le trade n'est pas un TEMPLATE (DTORDERENTERD, DTEXECUTION)
            (bool isMandatoryPresentData, bool isMandatoryPostData) = IsMandatoryPresentAndPostData(presentValue, postedValue);

            string offsetID = this.ClientID + "_Offset";
            string presentOffsetValue = txtOffset.Text;
            string postedOffsetValue = postCollection[offsetID] ?? string.Empty;

            string zoneID = this.ClientID + "_Zone";
            string presentZoneValue = ddlZone.SelectedValue;
            string postedZoneValue = postCollection[zoneID] ?? string.Empty;

            if (IsAltTime)
            {
                string timeID = this.ClientID + "_Time";
                string presentTimeValue = txtTime.Text;
                string postedTimeValue = postCollection[timeID] ?? string.Empty;

                // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
                if (isMandatoryPresentData)
                    presentValue += " " + presentTimeValue;
                if (isMandatoryPostData)
                    postedValue += " " + postedTimeValue;
            }
            // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
            if (isMandatoryPresentData)
                presentValue += " " + presentOffsetValue;
            if (isMandatoryPostData) 
                postedValue += " " + postedOffsetValue;

            //System.Diagnostics.Debug.WriteLineIf(StrFunc.IsEmpty(postedValue), "posted " + postDataKey + ":" + presentValue);

            if (presentValue == null || !presentValue.Equals(postedValue) || (presentZoneValue == null) || !presentZoneValue.Equals(postedZoneValue))
            {
                postedZonedDateTimeValue = new Pair<string, string>();
                if (presentValue == null || !presentValue.Equals(postedValue))
                    postedZonedDateTimeValue.First = postedValue;
                if ((presentZoneValue == null) || !presentZoneValue.Equals(postedZoneValue))
                    postedZonedDateTimeValue.Second = postedZoneValue;
                // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
                // Indicateur d'une date vierge (Cas TRADE TEMPLATE)
                Zdt.DateIsReset = String.IsNullOrEmpty(postedZonedDateTimeValue.First);
                ApplyPostedValue();
                return true;
            }
            else
            {
                // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
                Zdt.DateIsReset = String.IsNullOrEmpty(postedValue);
            }
            return false;
        }
        #endregion LoadPostData
        /// <summary>
        /// Lecture par Reflection de la Property : IsStEnvironment_Template
        /// La saisie des dates est obligatoire si le trade n'est pas un TEMPLATE (DTORDERENTERD, DTEXECUTION)
        /// </summary>
        /// <param name="pPresentValue">Valeur précédente du contrôle (= PresentValue)</param>
        /// <param name="pPostedValue">Valeur posté du contrôle (= PostedValue)</param>
        /// <returns></returns>
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        private (bool,bool) IsMandatoryPresentAndPostData(string pPresentValue, string pPostedValue)
        {
            bool ret = false;
            PropertyInfo pty = this.Page.GetType().GetProperty("IsStEnvironment_Template",typeof(Boolean));
            if (null != pty)
                ret = (bool) pty.GetValue(this.Page);

            return (StrFunc.IsFilled(pPresentValue) || (false == ret), StrFunc.IsFilled(pPostedValue) || (false == ret));
        }
        #region DateTimeValue
        /// <summary>
        /// Date + Time
        /// </summary>
        public string DateTimeValue
        {
            get
            {
                string _value = base.Text;
                if (IsAltTime)
                    _value += " " + txtTime.Text;
                return _value.Trim();
            }
        }
        #endregion DateTimeValue

        #region OffsetDateTimeValue
        /// <summary>
        /// Date + Time + Offset
        /// </summary>
        public string OffsetDateTimeValue
        {
            get
            {
                string _value = base.Text;
                if (IsAltTime)
                    _value += " " + txtTime.Text;
                _value += " " + txtOffset.Text;
                return _value.Trim();
            }
        }
        #endregion OffsetDateTimeValue

        #region SetZonedDateTime
        /// <summary>
        /// Mise à jour des controles (Date + Time + Offset + Zone)
        /// </summary>
        public void SetZonedDateTime()
        {
            SetDateTime();
            SetOffsetAndZone();
        }
        #endregion SetZonedDateTime
        #region SetDateTime
        /// <summary>
        /// Mise à jour des controles (Date + Time)
        /// </summary>
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public void SetDateTime()
        {
            if (Zdt.IsDateFilled)
            {
                // Date vierge autorisée (TRADETEMPLATE only)
                if (Zdt.DateIsReset)
                {
                    this.Text = string.Empty;
                    if (IsAltTime)
                        Time.Text = string.Empty;
                }
                else
                {
                    if (IsAltTime)
                    {
                        this.Text = Zdt.DateToString();
                        Time.Text = Zdt.TimeToString();
                    }
                    else
                    {
                        this.Text = Zdt.DateToString() + " " + Zdt.TimeToString();
                    }
                }
            }
        }
        #endregion SetDateTime
        #region SetOffsetAndZone
        /// <summary>
        /// Mise à jour des controles (Offset + Zone)
        /// </summary>
        // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public void SetOffsetAndZone()
        {
            this.ddlZone.SelectedValue = Zdt.TzDbId;
            // Date vierge autorisée (TRADETEMPLATE only)
            if (Zdt.DateIsReset)
            {
                if (this.ReadOnly || (false == isFreeZone))
                    lblZone.Text = string.Empty;
                this.txtOffset.Text = string.Empty;
            }
            else
            {
                if (this.ReadOnly || (false == isFreeZone))
                    lblZone.Text = Zdt.TzDbId;
                if (Zdt.IsDateFilled)
                    this.txtOffset.Text = Zdt.OffsetToString();
            }
        }
        #endregion SetOffsetAndZone

        #region ZonedDateTimeValue
        /// <summary>
        /// Retourne la zone
        /// </summary>
        public string ZonedDateTimeValue
        {
            get
            {
                return Zone.SelectedValue.Trim();
            }
        }
        #endregion ZonedDateTimeValue
        #region Text
        /// <summary>
        /// Set de la date
        /// </summary>
        public override string Text
        {
            set
            {
                base.Text = value;
                Attributes["oldvalue"] = value;
            }
        }
        #endregion Text
        #endregion Methods
    }


    /// <summary>
    /// DropDownList
    /// </summary>
    [Serializable]
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCDropDownList2 : DropDownList
    {
        #region Members
        private readonly string _cssClass;
        //private TextBox  txtViewer;
        [NonSerialized]
        private readonly Label _lblViewer;
        private bool _hasViewer;
        //_isSetTextOnTitle = true 
        //=> Tooltip des Items = Text des items 
        //=> Tooltip de la DDL Text de selectIndex
        private bool _isSetTextOnTitle;
        [NonSerialized]
        protected Label label;
        protected bool isLabelTop;
        protected string labelClass;
        protected string labelText;
        #endregion Members

        #region accessor
        /// <summary>
        ///  Obtient ou Définit la couleur d'arrière plan
        /// </summary>
        /// FI 20140708 [20179] add property
        public override Color BackColor
        {
            get
            {
                if (null != LblViewer)
                    return LblViewer.BackColor;
                else
                    return base.BackColor;
            }
            set
            {
                if (null != LblViewer)
                    LblViewer.BackColor = value;
                else
                    base.BackColor = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSetTextOnTitle
        {
            get { return _isSetTextOnTitle; }
            set { _isSetTextOnTitle = value; }
        }
        public bool HasViewer
        {
            get { return _hasViewer; }
            set { _hasViewer = true; }
        }
        public Label LblViewer
        {
            get { return _lblViewer; }
        }
        /// <summary>
        ///  Si true, la css class appliquée à la DropDownList sera appliquée au lblViewer
        ///  <para>Si False, le lblViewer conserve la css Class appliquée dans le constructor (Fonctionnement par défaut) </para>
        /// </summary>
        /// FI 20191128 [XXXXX] Add 
        public Boolean IsSynchonizeLblViewerCssClass
        {
            get;
            set;
        }
        #endregion Members

        #region constructor
        public WCDropDownList2() : this(false, EFSCssClass.DropDownListCapture) { }
        public WCDropDownList2(bool phasViewer) : this(phasViewer, EFSCssClass.CaptureConsult) { }
        public WCDropDownList2(bool phasViewer, string pCssClass)
        {
            _hasViewer = phasViewer;
            _lblViewer = new Label();
            _isSetTextOnTitle = false;
            _cssClass = pCssClass;
            this.CssClass = _cssClass;
            labelText = string.Empty;
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            //
            if (0 < labelText.Length)
            {
                this.label = new Label
                {
                    Text = labelText,
                    Visible =true,
                };
                this.label.Text += isLabelTop ? Cst.HTMLBreakLine : Cst.HTMLSpace;
                if (StrFunc.IsFilled(labelClass))
                    this.label.CssClass = labelClass;
            }
            Attributes.Add("oldvalue", string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {

            if (_hasViewer)
            {
                Style.Add(HtmlTextWriterStyle.Display, "none");
                _lblViewer.Text = string.Empty;
                if (null != SelectedItem)
                {
                    _lblViewer.Text = SelectedItem.Text;
                    _lblViewer.ToolTip = SelectedItem.Text;
                }
                _lblViewer.Width = Width;
                //
                #region Style
                //20080929 PL Add Code de gestion du style pour les DDL avec couleurs
                if (-1 < this.SelectedIndex)
                {
                    string attributes = this.Items[this.SelectedIndex].Attributes["style"];
                    if (null != attributes)
                    {
                        string[] s = attributes.Split(';');
                        for (int i = 0; i < s.Length; i++)
                        {
                            string[] nameValue = s[i].Split(':');
                            if (nameValue.Length >= 2)
                            {
                                _lblViewer.Style.Add(nameValue[0], nameValue[1]);
                            }
                        }
                    }
                }
                #endregion Style
            }
            else
            {
                if (IsSetTextOnTitle)
                {
                    // EG 20161122
                    ScriptManager sM = ((PageBase)Page).ScriptManager;
                    bool isFind = false;
                    if (null != sM)
                    {
                        for (int i = 0; i < ArrFunc.Count(sM.Scripts); i++)
                        {
                            if (sM.Scripts[i].Path == "~/Javascript/ControlsTools.js")
                                isFind = true;
                            if (isFind)
                                break;
                        }
                    }
                    if (false == isFind)
                        throw new Exception("script Reference ~/Javascript/ControlsTools.js not found");
                    ControlsTools.SetAttributesList(this.Attributes, "onchange:DDLSetTextOnToolTip(this.event)");
                }
            }
            //
            if (null != SelectedItem)
                Attributes["oldvalue"] = SelectedItem.Text;
            //
            base.OnPreRender(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (_isSetTextOnTitle)
                ToolTip = this.Items[SelectedIndex].Text;
            //            
            base.OnSelectedIndexChanged(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {

            if (null != label)
                label.RenderControl(writer);
            //
            base.Render(writer);
            //
            if (_hasViewer)
            {
                if (null != this.ID)
                {
                    //txtViewer.ID = Cst.TXT + ID.Substring(Cst.DDL.Length) + "Viewer";
                    //txtViewer.CssClass = EFSCssClass.CaptureConsult;
                    //txtViewer.RenderControl(writer);

                    _lblViewer.ID = Cst.LBL + ID.Substring(Cst.DDL.Length) + "Viewer";
                    // FI 20191128 [XXXXX] gestion de isSynchonizeLblViewerCssClass
                    if (IsSynchonizeLblViewerCssClass)
                        _lblViewer.CssClass = this.CssClass;
                    else
                        _lblViewer.CssClass = _cssClass;
                    _lblViewer.RenderControl(writer);
                    // FI 20121107 [] certains label sont parfois trop courts, le tooltip est alimenté  
                    _lblViewer.ToolTip = _lblViewer.Text; 
                }
            }
            else
            {
                if (_isSetTextOnTitle)
                    ControlsTools.DDLItemsSetTextOnTitle(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return base.LoadPostData(postDataKey, postCollection);
        }
    
        #endregion
    }
    
    /// <summary>
    /// HtmlSelect
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCHtmlSelect : HtmlSelect
    {
        readonly Label lblViewer;
        readonly bool _hasViewer = false;

        public WCHtmlSelect()
        { }

        /// <summary>
        /// DDL avec son Viewer
        /// </summary>
        /// <param name="str">Viewer de la DDl</param>
        public WCHtmlSelect(bool hasViewer)
        {
            _hasViewer = hasViewer;
            lblViewer = new Label();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if ((-1 != this.SelectedIndex) && (_hasViewer))
            {
                this.Style.Add(HtmlTextWriterStyle.Display, "none");
                lblViewer.Text = this.Items[this.SelectedIndex].Text;
                string attributes = this.Items[this.SelectedIndex].Attributes["style"];
                if (null != attributes)
                {
                    string[] s = attributes.Split(';');
                    for (int i = 0; i < s.Length; i++)
                    {
                        string[] nameValue = s[i].Split(':');
                        lblViewer.Style.Add(nameValue[0], nameValue[1]);
                    }
                }
            }
            base.OnPreRender(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (((null != this.ID) && (_hasViewer)))
            {
                lblViewer.ID = Cst.LBL + this.ID.Substring(Cst.DDL.Length) + "Viewer";
                lblViewer.CssClass = EFSCssClass.CaptureConsult;
                lblViewer.RenderControl(writer);
            }
        }
    }

    /// <summary>
    /// LinkButton d'ouverture d'un TogglePanel (avec Font awesome) se substitue à WCImageButtonOpenBanner
    /// </summary>
    /// EG 20150923 Refactoring
    /// EG 20180423 Analyse du code Correction [CA1405]
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    [ComVisible(false)]
    public class WCLinkButtonOpenBanner : LinkButton
    {
        #region Members

        /// <summary>
        /// Id du WebControl lié
        /// </summary>
        private readonly string _linkControlId;
        /// <summary>
        /// Container (Page, PlaceHolder, etc..) qui permet d'accéder au WebControl lié via FindControl
        /// </summary>
        private readonly Control _controlContainer;
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControlContainer">Contrôle qui contient le webControl lié</param>
        /// <param name="pLinkControlId">Id du webControl lié</param>
        /// <param name="pStartDisplay">Mode d'affichage du webControl lié : collapse sinon expand</param>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Désactivation du tabIndex
        public WCLinkButtonOpenBanner(Control pControlContainer, string pLinkControlId, string pStartDisplay)
        {

            _linkControlId = pLinkControlId;
            _controlContainer = pControlContainer;
            this.ID = "IMG" + _linkControlId;
            this.TabIndex = -1;
            this.CssClass = "fa-icon";
            // FI 20200121 [XXXXX] 
            // call des méhodes HideLinkControl et ShowLinkControl
            // Exception si paramètre incorrect
            if (pStartDisplay.ToLower() == "collapse")
                HideLinkControl();
            else if (pStartDisplay.ToLower() == "expand")
                ShowLinkControl();
            else
                throw new ArgumentException(StrFunc.AppendFormat("value:{0} is not expected", pStartDisplay));

            //this.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
            this.CausesValidation = false;
            // FI 20200121 [XXXXX] Appel de PostBack car elle contient SaveActiElement
            //Le focus est conservé sur l'image lorsque l'utilisateur appuie sur space clavier
            this.OnClientClick = "PostBack('" + this.UniqueID + "','');return false;";
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// Affiche/Cache le WebControl lié à ImageButtonOpenBanner 
        /// <para>Cette méthode doit être appelée uniquement dans le OnPreRender</para>
        /// </summary>
        private void SetLinkControlDisplay()
        {
            // FI 20200121 [XXXXX] Exception en mode DEBUG
#if DEBUG
            //if (null == webControl)
            //    throw new NullReferenceException(StrFunc.AppendFormat("linkControl not found"));
#endif
            if (_controlContainer.FindControl(_linkControlId) is WebControl webControl)
            {
                // FI 20200120 [XXXXX] block n'as pas tjs approprié 
                // FI 20200121 [XXXXX] Usage de la propriété visible et de la méthode IsLinkControlShow
                //webControl.Style[HtmlTextWriterStyle.Display] = this.CssClass.EndsWith(CSS.Main.collapse.ToString()) ? "block" : "none";
                // FI 20200317 [XXXXX] L'usage de la proriété .Visible est supprimée 
                // car s'il existe des contrôles de type CheckBox présents sous le webControl on perd leur valeur lors de la publication de la page
                // Désormais on applique le style "display:none" (pour ne pas afficher webControl) et pas attribut display (pour afficher webControl)
                //webControl.Visible = IsLinkControlShow;
                // 
                if (IsLinkControlShow)
                    ControlsTools.RemoveStyleDisplay(webControl);
                else
                    webControl.Style[HtmlTextWriterStyle.Display] = "none";
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public void SetEventHandler()
        {
            this.Click += new EventHandler(OnLinkButtonClick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20200121 [XXXXX] Add Method
        private void OnLinkButtonClick(object sender, EventArgs e)
        {
            if (IsLinkControlHide)
                ShowLinkControl();
            else
                HideLinkControl();
        }

        /// <summary>
        /// Applique une directive pour afficher le WebControl lié 
        /// <para>La directive sera appliquée dans le OnPreRender</para>
        /// </summary>
        /// FI 20200121 [XXXXX] Add Method
        public void ShowLinkControl()
        {
            this.Text = "<i class='far fa-minus-square'></i>";
        }

        /// <summary>
        /// Retourne true s'il existe une directive qui affiche le WebControl lié
        /// <para>La directive sera appliquée dans le OnPreRender</para>
        /// </summary>
        /// <returns></returns>
        public Boolean IsLinkControlShow
        {
            get
            {
                return this.Text.Contains("minus-square");
            }
        }

        /// <summary>
        /// Applique une directive pour cacher le WebControl lié 
        /// <para>La directive sera appliquée dans le OnPreRender</para>
        /// </summary>
        /// FI 20200121 [XXXXX] Add Method
        public void HideLinkControl()
        {
            this.Text = "<i class='far fa-plus-square'></i>";
        }

        /// <summary>
        /// Retourne true s'il existe une directive qui cache le WebControl lié
        /// <para>La directive sera appliquée dans le OnPreRender</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20200121 [XXXXX] Add Method
        public Boolean IsLinkControlHide
        {
            get
            {
                return this.Text.Contains("plus-square");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200121 [XXXXX] Add 
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            // FI 20200121 [XXXXX] Appel à SetLinkControlDisplay
            SetLinkControlDisplay();
            base.OnPreRender(e);
        }
        #endregion
    }

    #region WCToolTipPanel
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCToolTipPanel : Panel
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipPanel()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// EG 20200107 [XXXXX] Les tooltip (de type info) sans contenu ont un "i" informatif les autres un "i" cerclé + bouton close
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            else
                this.CssClass = this.CssClass.Replace("fa-info-circle", "fa-info");
            m_Pty.SetButton(this.CssClass);
            m_Pty.Render(writer);
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipPanel
    #region WCToolTipButton
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCToolTipButton : Button
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipButton()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.SetButton(this.CssClass);
            m_Pty.Render(writer);
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipButton
    #region WCToolTipLinkButton
    // EG 20190411 [ExportFromCSV]  New LinkButton (used Font Awesome CSS)
    [ComVisible(false)]
    public class WCToolTipLinkButton : LinkButton
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipLinkButton()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.SetButton(this.CssClass);
            m_Pty.Render(writer);
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipLinkButton
    #region WCToolTipCell
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCToolTipCell : TableCell
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipCell()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.Render(writer);
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipCell
    #region WCToolTipHyperlink
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCToolTipHyperlink : HyperLink
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipHyperlink()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.Render(writer);
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipHyperlink
    #region WCToolTipLabel
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCTooltipLabel : Label
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCTooltipLabel()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.Render(writer);
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipLabel
    #region WCToolTipImage
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCToolTipImage : System.Web.UI.WebControls.Image
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipImage()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.SetButton(this.ImageUrl);
            m_Pty.Render(writer);
            if (StrFunc.IsEmpty(this.ImageUrl))
                this.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), @"images\PNG\BannerHelp-Gray.png");
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipImage
    #region WCToolTipImageButton
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCToolTipImageButton : ImageButton
    {
        #region Members
        private readonly WCTooltipCommonProperties m_Pty;
        #endregion Members
        #region Accessors
        #region Pty
        public WCTooltipCommonProperties Pty
        {
            get { return this.m_Pty; }
        }
        #endregion Pty
        #endregion Accessors
        #region Constructors
        public WCToolTipImageButton()
        {
            m_Pty = new WCTooltipCommonProperties();
        }
        #endregion Constructors
        #region Events
        #region Render
        /// <summary>
        /// Renders the control to the specified HTML writer.
        protected override void Render(HtmlTextWriter writer)
        {
            if (StrFunc.IsFilled(m_Pty.TooltipTitle) || StrFunc.IsFilled(m_Pty.TooltipContent))
                Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            m_Pty.SetButton(this.ImageUrl);
            m_Pty.Render(writer);
            if (StrFunc.IsEmpty(this.ImageUrl))
                this.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), @"images\PNG\BannerHelp-Gray.png");
            base.Render(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion WCToolTipImageButton

    #region WCTooltipCommonProperties
    // EG 20240619 [WI945] Security : Update outdated components (QTip2)
    public class WCTooltipCommonProperties
    {
        #region Members
        private string m_Title;
        private string m_Content;
        private string m_QTipButton;
        private string m_Style;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Title. Background color is automatically the same as the border color.
        /// </summary>
        public string TooltipTitle
        {
            get { return this.m_Title; }
            set { this.m_Title = value; }
        }
        /// <summary>
        ///  The content will be displayed in the body of the tooltip and can be plain text or HTML.
        /// </summary>
        public string TooltipContent
        {
            get { return this.m_Content; }
            set { this.m_Content = value; }
        }
        /// <summary>
        /// Style of the Qtip
        /// </summary>
        public string TooltipStyle
        {
            get { return this.m_Style; }
            set { this.m_Style = value; }
        }
        #endregion Accessors

        #region Constructors
        public WCTooltipCommonProperties()
        {
            m_QTipButton = string.Empty;
            m_Style = "qtip-blue";
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20240619 [WI945] Security : Update outdated components (QTip2)
        public void SetButton(string pValue)
        {
            if (pValue.Contains("btnclose") ||  (pValue.Contains("fa-info-circle blue") && (StrFunc.IsFilled(TooltipTitle) || StrFunc.IsFilled(TooltipContent))))
                m_QTipButton = "X";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        // EG 20240619 [WI945] Security : Update outdated components (QTip2)
        public void Render(HtmlTextWriter writer)
        {
            string title = null;
            string content = null;
            if (StrFunc.IsFilled(m_Title))
            {
                string[] _title = m_Title.Split(';');
                if (ArrFunc.IsFilled(_title))
                    title = _title[0].Replace(Cst.HTMLBreakLine, Cst.HTMLSpace);
            }
            if (StrFunc.IsFilled(m_Content))
            {
                content = JavaScript.HTMLStringCrLf(m_Content);
                content = content.Replace("&", "&amp;");
            }

            if (StrFunc.IsFilled(content) && StrFunc.IsFilled(title) && (false == content.Equals(title)))
            {
                if (35 < title.Length)
                    title = title.Substring(0, 35) + "...";
                writer.AddAttribute("qtip-alt", content, true);
                writer.AddAttribute("title", title, true);
                writer.AddAttribute("qtip-style", m_Style, true);
                if (StrFunc.IsFilled(m_QTipButton))
                    writer.AddAttribute("qtip-button", m_QTipButton, true);
            }
            else if (StrFunc.IsFilled(content))
            {
                writer.AddAttribute("qtip-alt", content, true);
                writer.AddAttribute("qtip-style", m_Style, true);
            }
            else if (StrFunc.IsFilled(title))
            {
                writer.AddAttribute("qtip-alt", title, true);
                writer.AddAttribute("qtip-style", m_Style, true);
            }
        }
        #endregion Methods
    }
    #endregion WCToolTipLabel

    /// <summary>
    /// 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public class WCRoundedTable : Panel
    {
        #region Members
        private readonly string m_CssClass;
        private readonly Color m_BackgroundColor;
        private readonly Color m_BorderColor;
        #endregion Members

        #region Constructors
        public WCRoundedTable(string pCssClass) : this(pCssClass, Color.Empty, Color.Empty) { }
        public WCRoundedTable(Color pBackColor, Color pBorderColor)
            : this(null, pBackColor, pBorderColor ) { }
        public WCRoundedTable(string pCssClass, Color pBackColor, Color pBorderColor)
        {
            m_CssClass = pCssClass;
            m_BackgroundColor = pBackColor;
            m_BorderColor = pBorderColor;
            if (StrFunc.IsFilled(m_CssClass))
                AddBodyCss();
            else
                AddBodyStyle();
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        public void AddContent(WebControl pControl)
        {
            pControl.BackColor = Color.Transparent;
            pControl.Style.Add(HtmlTextWriterStyle.ZIndex, "6");
            this.Controls[0].Controls[0].Controls.Add(pControl);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void AddBodyCss()
        {
            Panel pnlBody = new Panel
            {
                CssClass = "contentb " + m_CssClass
            };
            Panel pnlBody2 = new Panel();
            pnlBody.Controls.Add(pnlBody2);
            this.Controls.Add(pnlBody);
        }
        
       
        /// <summary>
        /// 
        /// </summary>
        public void AddBodyStyle()
        {
            Panel pnlBody = new Panel
            {
                CssClass = "contentb ",
                BackColor = m_BackgroundColor,
                BorderColor = m_BorderColor
            };
            Panel pnlBody2 = new Panel();
            pnlBody.Controls.Add(pnlBody2);
            this.Controls.Add(pnlBody);
        }
        #endregion Methods
    }
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public class WCBodyPanel : Panel
    {
        #region Constructors
        public WCBodyPanel(string pCssClass):this(pCssClass, Color.Transparent,Color.Transparent){}
        public WCBodyPanel(string pCssClass, Color pBackColor, Color pBorderColor)
        {
            this.CssClass = pCssClass;
            Panel pnlBody = new Panel
            {
                CssClass = "contentb "
            };
            if (Color.Transparent != pBackColor)
                pnlBody.BackColor = pBackColor;
            if (Color.Transparent != pBorderColor)
                pnlBody.BorderColor = pBorderColor;
            this.Controls.Add(pnlBody);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        public void AddContent(WebControl pControl)
        {
            pControl.BackColor = Color.Transparent;
            pControl.Style.Add(HtmlTextWriterStyle.ZIndex, "6");
            this.Controls[0].Controls.Add(pControl);
        }
        #endregion Methods
    }

    /// <summary>
    /// TogglePanel (Remplace WCTableH et WCRoundedTable)
    /// Panel constitué d'un panel dit "Header" et d'un panel dit "Body"
    /// </summary>
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    [ComVisible(false)]
    public class WCTogglePanel : Panel
    {
        /// <summary>
        /// classe css des labels présents dans le header,et qui sont cachés lorsque le body est fermé via un bouton WCImageButtonOpenBanner
        /// </summary>
        /// FI 20200121 [XXXXX] Add
        public const string CssHeadLinkInfo = "headLinkInfo"; /* Link pour info lié au contrôle lié au bouton WCImageButtonOpenBanner */
        /// <summary>
        /// classe css des labels présents dans le header,et visible en permanence 
        /// </summary>
        /// FI 20200121 [XXXXX] Add 
        public const string CssHeadAddInfo = "headAddInfo"; /* Add pour additional info */

        #region Members
        private Color backgroundColor;
        private string cssClass;
        private string title;
        private bool isReverse;
        private string _linkHeaderControlId;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Obtient le panel dit "Header"
        /// <para>Ce panel contient un label comme 1er control enfant</para>
        /// </summary>
        public Panel ControlHeader
        {
            get
            {
                return Controls[0] as Panel;
            }
        }

        /// <summary>
        /// Obtient le panel dit "Body"
        /// <para>Ce panel contient lui-même un panel comme 1er control enfant</para>
        /// </summary>
        public Panel ControlBody
        {
            get
            {
                return Controls[1] as Panel;
            }
        }

        public Panel ControlFooter
        {
            get
            {
                return Controls[2] as Panel;
            }
        }

        /// <summary>
        /// Obtient le control label chargé de recevoir le titre 
        /// <para>Obtient null s'il n'existe pas</para>
        /// </summary>
        public Label ControlHeaderTitle
        {
            get
            {
                Label lbl = ControlHeader.Controls[0] as Label;
                if ((null == lbl) && (1 < ControlHeader.Controls.Count))
                    lbl = ControlHeader.Controls[1] as Label;
                return lbl;
            }
        }

        /// <summary>
        /// Obtient true si le Header possède un WCLinkButtonOpenBanner (Dans ce cas c'est le 1er contrôle présent sous le Header)
        /// </summary>
        /// FI 20200120 [XXXXX] Add
        public Boolean HasLinkButtonOpenBanner
        {
            get
            {
                return ControlHeader.Controls[0].GetType().Equals(typeof(WCLinkButtonOpenBanner));
            }
        }
        /// <summary>
        /// Obtient le LinkButtonOpenBanner présent sur le header (lorsqu'il en existe)
        /// <para>Retour null sinon</para>
        /// </summary>
        /// FI 20200120 [XXXXX] Add
        public WCLinkButtonOpenBanner LinkButtonOpenBanner
        {
            get
            {
                WCLinkButtonOpenBanner ret = null;
                if (HasLinkButtonOpenBanner)
                    ret = ControlHeader.Controls[0] as WCLinkButtonOpenBanner;
                return ret;
            }
        }
        #endregion Accessors

        #region Constructors
        public WCTogglePanel()
        {
            this.CssClass = "referential";
            Build();
        }
        public WCTogglePanel(Color pBackgroundColor, string pTitle, string pCssClass, bool pIsReverse)
        {
            Build(pBackgroundColor, pTitle, pCssClass, pIsReverse);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Construction du panel
        /// </summary>
        private void Build()
        {
            BuildHeader();
            BuildBody();
        }
        private void Build(Color pBackgroundColor, string pTitle, string pCssClass, bool pIsReverse)
        {
            backgroundColor = pBackgroundColor;
            title = pTitle;
            cssClass = pCssClass;
            isReverse = pIsReverse && (pBackgroundColor == Color.Transparent);

            BuildHeader();
            BuildBody();
        }

        /// <summary>
        /// Ajoute un panel dit "Header" contenant un label
        /// </summary>
        private void BuildHeader()
        {
            Panel pnlHeader = new Panel
            {
                CssClass = "headh"
            };
            if (isReverse)
                pnlHeader.CssClass = "headhR";

            if (Color.Transparent != backgroundColor)
                pnlHeader.BackColor = backgroundColor;
            // Ajout du contrôle Titre
            Label lbl = new Label
            {
                Text = title,
                CssClass = "title"
            };
            if (StrFunc.IsFilled(cssClass))
                lbl.CssClass = cssClass;
            pnlHeader.Controls.Add(lbl);
            this.Controls.Add(pnlHeader);
        }

        /// <summary>
        /// Ajoute un panel dit "Body" contenant lui même un panel
        /// </summary>
        private void BuildBody()
        {
            Panel pnlBody = new Panel
            {
                CssClass = "contenth"
            };
            pnlBody.Controls.Add(new Panel());
            this.Controls.Add(pnlBody);
        }

        /// <summary>
        /// Ajoute {pControl} dans le Body
        /// </summary>
        /// <param name="pControl"></param>
        public void AddContent(WebControl pControl)
        {
            pControl.Style.Add(HtmlTextWriterStyle.ZIndex, "6");
            ControlBody.Controls[0].Controls.Add(pControl);
        }

        /// <summary>
        /// Ajoute {pControl} dans le Body
        /// </summary>
        /// <param name="pControl"></param>
        public void AddContent(PlaceHolder pControl)
        {
            ControlBody.Controls[0].Controls.Add(pControl);
        }

        /// <summary>
        /// Ajoute un contrôle dans le panel dit "Header" à l'index 0
        /// </summary>
        /// <param name="pControl"></param>
        public void AddContentHeader(Control pControl)
        {
            AddContentHeader(pControl, false);
        }
        /// <summary>
        /// Ajoute un contrôle dans le panel dit "Header" 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pIsAfter">Si false, {pControl} sera placé à l'index 0</param>
        public void AddContentHeader(Control pControl, bool pIsAfter)
        {
            if (pIsAfter)
                ControlHeader.Controls.Add(pControl);
            else
                ControlHeader.Controls.AddAt(0, pControl);
        }

        /// <summary>
        /// Ajouter un WCLinkButtonOpenBanner dans le Header, il est ajouté comme 1er control (index 0)
        /// <para>le contrôle lié devra se trouver dans le ControlBody</para>
        /// </summary>
        /// <param name="pLinkControlId">id du WebControl lié au WCLinkButtonOpenBanner (Ce contrôle doit être présent dans ControlBody)</param>
        /// <param name="pStartDisplay">Mode d'affichage du webControl lié : collapse sinon expand</param>
        /// FI 20200120 [XXXXX] Add Method
        public void AddLinkButtonOpenBanner(string pLinkControlId, string pStartDisplay)
        {
            WCLinkButtonOpenBanner but = new WCLinkButtonOpenBanner(this.ControlBody, pLinkControlId, pStartDisplay);
            AddContentHeader(but);
        }

        public void ResetColorHeader(string pCssClass)
        {
            ControlHeader.CssClass = pCssClass;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeaderTitle"></param>
        public void SetHeaderTitle(string pHeaderTitle)
        {
            Label lbl = ControlHeaderTitle;
            if (null == lbl)
                throw new Exception("Control Label doesn't exist");
            lbl.Text = pHeaderTitle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pHelpSchema"></param>
        /// <param name="pHelpElement"></param>
        public void SetHelpToHeaderTitle(PageBase pPage, string pHelpSchema, string pHelpElement)
        {
            if (StrFunc.IsFilled(pHelpElement))
            {
                string helpUrl = pPage.GetCompleteHelpUrl(pHelpSchema, pHelpElement);
                if (StrFunc.IsFilled(helpUrl))
                {
                    ControlHeaderTitle.Style.Add(HtmlTextWriterStyle.Cursor, "help");
                    pPage.SetQtipHelpOnLine(ControlHeaderTitle, ControlHeaderTitle.Text, helpUrl);
                }
            }
        }
        public void SetHeaderControlId(string pLinkHeaderControlId)
        {
            _linkHeaderControlId = pLinkHeaderControlId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>

        protected override void OnPreRender(EventArgs e)
        {
            Label lbl = ControlHeaderTitle;
            if ((null != lbl) && StrFunc.IsEmpty(lbl.Text))
                lbl.Visible = false;

            // FI 20200121 [XXXXX] call Method
            SetDisplayHeadLinkInfo();

            if (StrFunc.IsFilled(_linkHeaderControlId))
            {
                // FI 20200120 [XXXXX] Utilisation de ControlHeader 
                if (ControlHeader.FindControl(_linkHeaderControlId) is WebControl headerLink)
                {
                    headerLink.Parent.Controls.Remove(headerLink);

                    Panel pnlHeaderContainer = new Panel
                    {
                        CssClass = cssClass
                    };
                    pnlHeaderContainer.Controls.Add(headerLink);
                    AddContentHeader(pnlHeaderContainer);
                }
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Ajoute un label dédié à l'affichage d'une donnée présente dans le WebControl lié à ImageButtonOpenBanner
        /// <para>La donné est affichée uniquement lorsque le WebControl est caché</para>
        /// </summary>
        /// <param name="pLabel"></param>
        /// FI 20200121 [XXXXX] Add Method
        public void AddContentHeaderLinkInfo(Label pLabel)
        {
            pLabel.CssClass = StrFunc.IsFilled(pLabel.CssClass) ? pLabel.CssClass + Cst.Space + CssHeadLinkInfo : CssHeadLinkInfo;
            AddContentHeader(pLabel, true);
        }

        /// <summary>
        /// Ajoute un label dédié à l'affichage d'une propriété étendue
        /// </summary>
        /// <param name="pLabel"></param>
        /// FI 20200121 [XXXXX] Add Method
        public void AddContentHeaderAddInfo(Label pLabel)
        {
            pLabel.CssClass = StrFunc.IsFilled(pLabel.CssClass) ? pLabel.CssClass + Cst.Space + CssHeadAddInfo : CssHeadAddInfo;
            AddContentHeader(pLabel, true);
        }

        /// <summary>
        /// Affiche/Cache les labels dédiés à l'affichage de données présentes dans le WebControl lié à ImageButtonOpenBanner 
        /// <para>Les données sont affichées uniquement lorsque le WebControl est caché</para>
        /// <para>Cette méthode doit être appelée uniquement dans le OnPreRender</para>
        /// </summary>
        /// FI 20200121 [XXXXX] Add Method
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        private void SetDisplayHeadLinkInfo()
        {
            if (this.HasLinkButtonOpenBanner)
            {
                int indexStart = (null != ControlHeaderTitle) ? 2 : 1;
                for (int i = indexStart; i < ControlHeader.Controls.Count; i++)
                {
                    if (ControlHeader.Controls[i] is Label lbl && lbl.CssClass.EndsWith(CssHeadLinkInfo))
                        lbl.Visible = StrFunc.IsFilled(lbl.Text);// && LinkButtonOpenBanner.IsLinkControlHide;
                }
            }
        }
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        public void SetCssContentFullMargin()
        {
            ControlBody.CssClass = "contenthfullmargin";
        }
        #endregion Methods
    }


    #region WCMathRoundedTable
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class WCMathRoundedTable : Panel
    {
        #region Members
        private readonly Color m_BackgroundColor;
        private readonly Color m_BorderColor;
        private readonly double m_X;
        private readonly double m_Y;
        #endregion Members
        #region Constructors
        public WCMathRoundedTable(Color pBackColor, Color pBorderColor) : this(pBackColor, pBorderColor, Unit.Pixel(30), Unit.Pixel(30)) { }
        public WCMathRoundedTable(Color pBackColor, Color pBorderColor, Unit pX, Unit pY)
        {
            m_BackgroundColor = pBackColor;
            m_BorderColor = pBorderColor;
            m_X = pX.Value;
            m_Y = pY.Value;
            AddBorder();
            AddBody();
            AddBorder(true);
        }
        #endregion Constructors
        #region Methods
        #region AddBorder
        public void AddBorder()
        {
            AddBorder(false);
        }
        public void AddBorder(bool pIsBottom)
        {
            Panel pnlTop = new Panel
            {
                CssClass = "rounded"
            };
            pnlTop.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#" + m_BackgroundColor.Name.Substring(2));
            this.Controls.Add(pnlTop);
            Panel[] pnlTop1 = new Panel[Convert.ToInt32(m_Y)];
            double lastArc = 0;
            for (int i = 1; i <= m_Y; i++)
            {
                double arc = Math.Sqrt(1.0 - Math.Pow(1.0 - Convert.ToDouble(i) / m_Y, 2.0)) * m_X;
                double n_Bg = m_X - Math.Ceiling(arc);
                double n_Fg = Math.Floor(lastArc);
                double n_aa = m_X - n_Bg - n_Fg;

                pnlTop1[i - 1] = new Panel();
                pnlTop1[i - 1].Style.Add(HtmlTextWriterStyle.Margin, "0px " + n_Bg + "px");
                Panel[] pnl2 = new Panel[Convert.ToInt32(n_aa)];
                for (int j = 1; j <= n_aa; j++)
                {
                    pnl2[j - 1] = new Panel();
                    pnl2[j - 1].Style.Add(HtmlTextWriterStyle.Margin, "0px 1px");
                    double arc2;
                    double coverage;
                    if (1 == j)
                    {
                        if (j == n_aa)
                        {
                            coverage = ((arc + lastArc) * .5) - n_Fg;
                            pnl2[j - 1].Style.Add(HtmlTextWriterStyle.BackgroundColor, "#" + m_BorderColor.Name.Substring(2));
                        }
                        else
                        {
                            arc2 = Math.Sqrt(1.0 - Math.Pow(1.0 - (n_Bg + 1.0) / m_X, 2.0)) * m_Y;
                            coverage = (arc2 - (m_Y - Convert.ToDouble(i))) * (arc - n_Fg - n_aa + 1.0) * .5;
                        }
                    }
                    else if (j == n_aa)
                    {
                        arc2 = Math.Sqrt(1.0 - Math.Pow((m_X - n_Bg - Convert.ToDouble(j) + 1.0) / m_X, 2.0)) * m_Y;
                        coverage = 1.0 - (1.0 - (arc2 - (m_Y - Convert.ToDouble(i)))) * (1.0 - (lastArc - n_Fg)) * .5;
                        pnl2[j - 1].Style.Add(HtmlTextWriterStyle.BackgroundColor, "#" + m_BorderColor.Name.Substring(2));
                    }
                    else
                    {
                        double arc3 = Math.Sqrt(1.0 - Math.Pow((m_X - n_Bg - Convert.ToDouble(j)) / m_X, 2.0)) * m_Y;
                        arc2 = Math.Sqrt(1.0 - Math.Pow((m_X - n_Bg - Convert.ToDouble(j) + 1.0) / m_X, 2.0)) * m_Y;
                        coverage = ((arc2 + arc3) * .5) - (m_Y - Convert.ToDouble(i));
                    }

                    Color newBgColor = Blend(coverage);

                    if (1 == j)
                    {
                        pnlTop1[i - 1].Style.Add(HtmlTextWriterStyle.BackgroundColor, "#" + newBgColor.Name.Substring(2));
                        if (pIsBottom)
                            pnlTop1[i - 1].Controls.AddAt(0, pnl2[j - 1]);
                        else
                            pnlTop1[i - 1].Controls.Add(pnl2[j - 1]);
                    }
                    else
                    {
                        pnl2[j - 2].Style.Add(HtmlTextWriterStyle.BackgroundColor, "#" + newBgColor.Name.Substring(2));
                        if (pIsBottom)
                            pnl2[j - 2].Controls.AddAt(0, pnl2[j - 1]);
                        else
                            pnl2[j - 2].Controls.Add(pnl2[j - 1]);
                    }
                }
                if (pIsBottom)
                    pnlTop.Controls.AddAt(0, pnlTop1[i - 1]);
                else
                    pnlTop.Controls.Add(pnlTop1[i - 1]);
                lastArc = arc;
            }

        }
        #endregion AddBorder
        #region AddContent
        public void AddContent(WebControl pControl)
        {

            pControl.BackColor = Color.Transparent;
            this.Controls[1].Controls.Add(pControl);

        }
        #endregion AddContent
        #region AddBody
        public void AddBody()
        {
            Panel pnlBody = new Panel();
            pnlBody.Style.Add(HtmlTextWriterStyle.BackgroundColor, "#" + m_BorderColor.Name.Substring(2));
            this.Controls.Add(pnlBody);
        }
        #endregion AddBody
        #region Blend
        private Color Blend(double pAlpha)
        {
            double bgR = m_BackgroundColor.R;
            double bgG = m_BackgroundColor.G;
            double bgB = m_BackgroundColor.B;

            double bR = m_BorderColor.R;
            double bG = m_BorderColor.G;
            double bB = m_BorderColor.B;

            int r = Convert.ToInt32(Math.Round(bgR + (bR - bgR) * pAlpha));
            int g = Convert.ToInt32(Math.Round(bgG + (bG - bgG) * pAlpha));
            int b = Convert.ToInt32(Math.Round(bgB + (bB - bgB) * pAlpha));
            return Color.FromArgb(r, g, b);
        }
        #endregion Blend
        #endregion Methods
    }
    #endregion WCRoundedTable
}
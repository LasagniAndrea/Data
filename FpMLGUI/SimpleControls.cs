#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.GUI.SimpleControls
{
	public delegate void PasteChoiceSelectedEventHandler(object sender, PasteChoiceSelectedEventArgs e);

	#region public enum CountryNameEnum
	public enum CountryNameEnum
	{
		country,
		ctry,
		issuctry,
	}
	#endregion CountryNameEnum
	#region public enum CurrencyNameEnum
	public enum CurrencyNameEnum
	{
		agmtccy,
        allocsettlccy,
		contamtcurr,
		curr,
		currency,
		ccy,
		identifiedcurrency,
		settlccy,
		strkccy,
	}
	#endregion CurrencyNameEnum
	#region public enum MarketNameEnum
	public enum MarketNameEnum
	{
		exch,
		lastmkt,
	}
	#endregion MarketNameEnum

    #region public class FpMLCalendarBox
    /// EG 20170918 [23342] Upd
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLCalendarBox : TextBox
	{
		#region Members
		private readonly ControlGUI m_Control;
		private readonly string m_LabelCssClass;
		private readonly ArrayList m_aValidator;
		private readonly string m_FpMLKey;
		private readonly string	m_HelpUrl;
		private readonly FullConstructor m_FullCtor;
		#endregion
		#region Members Controls
		private Label              m_Label;
		private LiteralControl     m_LiteralControl;
		private PlaceHolder        m_PlaceHolder1;
		private PlaceHolder        m_PlaceHolder2;
		#endregion Members Controls
		#region Accessors
		public EFSRegex.TypeRegex Regex {get{return m_Control.Regex;}}
		public string FpMLKey {get{return m_FpMLKey;}}
		public Label       Label {get{return m_Label;}}
		#endregion Accessors
		#region Constructors
		public FpMLCalendarBox(FullConstructor pFullCtor,DateTime pDtValue,string pFpMLKey,ControlGUI pControlGUI,string pHelpUrl,params Validator[] pValidator)
			:this(pFullCtor,pDtValue,pFpMLKey,pControlGUI,pHelpUrl,string.Empty,pValidator){}
        // EG 20170918 [23342] Refactoring
		public FpMLCalendarBox(FullConstructor pFullCtor,DateTime pDtValue,string pFpMLKey,ControlGUI pControlGUI,string pHelpUrl,string pID,params Validator[] pValidator)
		{
            bool isDateTime = false;
            string fmtDate = DtFunc.FmtShortDate;
            m_LabelCssClass = "lblCaptureTitle";
            m_aValidator = new ArrayList();

            m_Control = pControlGUI;
            m_FpMLKey = pFpMLKey;
            m_HelpUrl = pHelpUrl;
            m_FullCtor = pFullCtor;

            if (0 != pDtValue.CompareTo(DateTime.MinValue))
                this.Text = DtFunc.DateTimeToString(pDtValue, DtFunc.FmtShortDate);
            ID = pID;

            for (int i = 0; i < pValidator.Length; i++)
            {
                m_aValidator.Add(pValidator[i]);
                Validator validator = (Validator)m_aValidator[i];
                if (EFSRegex.TypeRegex.RegexDateTime == validator.RegexTemplate)
                {
                    isDateTime = true;
                    fmtDate = DtFunc.FmtDateLongTime;
                }
            }

            if (isDateTime)
            {
                CssClass = "DtTimePicker " + EFSCssClass.Capture;
                this.Width = Unit.Pixel(155);
            }
            else
            {
                CssClass = "DtPicker " + EFSCssClass.Capture;
                this.Width = Unit.Pixel(85);
            }
            if (0 != pDtValue.CompareTo(DateTime.MinValue))
                this.Text = DtFunc.DateTimeToString(pDtValue, fmtDate);

            EnsureChildControls();
        }
		#endregion Constructors

		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		protected override void CreateChildControls()
		{
			if (m_Control.IsLabel)
			{
                m_Label = new Label
                {
                    CssClass = m_LabelCssClass
                };
                if (MethodsGUI.IsFixedOrNoneLevel(m_Control.Level))
					m_Label.Text  = Cst.HTMLSpace + m_Control.Name.ToString().Trim() + Cst.HTMLSpace;
				if (0 != m_Control.LblWidth) 
					m_Label.Width = Unit.Pixel(m_Control.LblWidth);

				MethodsGUI.SetHelpUrlLink(m_Label,m_HelpUrl);
			}
			//
			for (int i=0;i<m_aValidator.Count;i++)
			{
				Validator validator = (Validator) m_aValidator[i];
				Controls.Add(validator.CreateControl(this.ID));
			}
			this.Style.Add(HtmlTextWriterStyle.Overflow,"hidden");
            m_LiteralControl = new LiteralControl
            {
                Text = Cst.HTMLSpace
            };
            // 20100901 EG Datepicker substitution
            if (MethodsGUI.IsFixedLevel(m_Control.Level))
			{
				m_PlaceHolder1 = MethodsGUI.MakeDiv(m_FullCtor,new OpenDivGUI(m_Control.Level,m_Control.Name + Cst.HTMLSpace,
					m_Control.IsDisplay,string.Empty,m_Control.LblWidth),true);
				this.BackColor = Color.Transparent;
				m_PlaceHolder2 = MethodsGUI.MakeDiv(m_FullCtor,new CloseDivGUI(m_Control.Level));
			}
			else
			{
				m_PlaceHolder1 = new PlaceHolder();
				if (m_Control.IsLabel) 
					m_PlaceHolder1.Controls.Add(m_Label);
				m_PlaceHolder1.Controls.Add(new LiteralControl(Cst.HTMLSpace));
			}
			base.CreateChildControls();
		}

		#endregion CreateChildControls
		#endregion
		#region Events
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            //20071212 FI Ticket 16012 => Migration Asp2.0
            foreach (WebControl ctrl in this.Controls)
                ((BaseValidator)ctrl).ControlToValidate = this.ID;
            base.OnPreRender(e);
        }
        #endregion
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            bool isConsult;
            try
            {
                isConsult = MethodsGUI.IsModeConsult(this.Page);
            }
            catch (NotImplementedException) { isConsult = false; }

            if (null != m_PlaceHolder1)
                m_PlaceHolder1.RenderControl(writer);

            ReadOnly = isConsult;

            base.Render(writer);

            m_LiteralControl.RenderControl(writer);
            if (null != m_PlaceHolder2)
                m_PlaceHolder2.RenderControl(writer);

            RenderChildren(writer);
        }
        #endregion Render
        #endregion Events
	}
	#endregion FpMLCalendarBox

    #region FpMLTimestampBox
    /// EG 20170918 [23342] new
    /// EG 20170926 [22374] Upd
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class FpMLTimestampBox : WebControl
    {
        #region Members
        private readonly ControlGUI m_Control;
        private readonly string m_FpMLKey;
        private readonly string m_HelpUrl;
        private readonly FullConstructor m_FullCtor;
        #endregion
        #region Members Controls
        private readonly WCZonedDateTime m_DatePicker;
        private readonly string m_DtValue;
        private readonly string m_TimeZone;
        private PlaceHolder m_PlaceHolder1;
        #endregion Members Controls
        #region Accessors
        public EFSRegex.TypeRegex Regex { get { return m_Control.Regex; } }
        public string FpMLKey { get { return m_FpMLKey; } }
        public WCZonedDateTime ControlDatePicker { get { return m_DatePicker; } }
        #endregion Accessors
        #region Constructors
        public FpMLTimestampBox(FullConstructor pFullCtor, string pDtValue, string pFpMLKey, ControlGUI pControlGUI, string pHelpUrl)
        {
            m_Control = pControlGUI;
            m_FpMLKey = pFpMLKey;
            m_HelpUrl = pHelpUrl;
            m_FullCtor = pFullCtor;

            string labelText = string.Empty;
            if (m_Control.IsLabel)
            {
                if (MethodsGUI.IsFixedOrNoneLevel(m_Control.Level))
                    labelText = Cst.HTMLSpace + m_Control.Name.ToString().Trim() + Cst.HTMLSpace;
            }

            m_DatePicker = new WCZonedDateTime(m_FullCtor.GetUniqueID(), labelText, false, "datetimeoffset", pControlGUI.IsAltTime, false, false, false, false, null)
            {
                CausesValidation = true
            };
            m_TimeZone = Tz.Tools.UniversalTimeZone;
            m_DtValue = pDtValue;
            m_DatePicker.Zdt.SetZone(m_TimeZone);

            EnsureChildControls();
        }
        #endregion Constructors
        //
        #region Methods
        #region CreateChildControls
        protected override void CreateChildControls()
        {
            if (m_Control.IsLabel && (null != m_DatePicker.Label))
            {
                MethodsGUI.SetHelpUrlLink(m_DatePicker.Label, m_HelpUrl);
                if (0 != m_Control.LblWidth)
                    m_DatePicker.Label.Width = Unit.Pixel(m_Control.LblWidth);
            }

            this.Style.Add(HtmlTextWriterStyle.Overflow, "hidden");

            if (MethodsGUI.IsFixedLevel(m_Control.Level))
            {
                m_PlaceHolder1 = MethodsGUI.MakeDiv(m_FullCtor, new OpenDivGUI(m_Control.Level, m_Control.Name + Cst.HTMLSpace,
                    m_Control.IsDisplay, string.Empty, m_Control.LblWidth), true);
                this.BackColor = Color.Transparent;
                _ = MethodsGUI.MakeDiv(m_FullCtor, new CloseDivGUI(m_Control.Level));
            }
            else
            {
                m_PlaceHolder1 = new PlaceHolder();
                m_PlaceHolder1.Controls.Add(new LiteralControl(Cst.HTMLSpace));
            }
            m_PlaceHolder1.Controls.Add(m_DatePicker);
            base.CreateChildControls();
        }

        #endregion CreateChildControls
        #endregion
        #region Events
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            if (Tz.Tools.IsDateFilled(m_DtValue))
            {
                m_DatePicker.Zdt.Parse = m_DtValue;
                m_DatePicker.SetZonedDateTime();
            }
            base.OnPreRender(e);
        }
        #endregion
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            bool isConsult;
            try { isConsult = MethodsGUI.IsModeConsult(this.Page); }
            catch (NotImplementedException) { isConsult = false; }
            m_DatePicker.ReadOnly = isConsult;
            m_PlaceHolder1.RenderControl(writer);

            base.Render(writer);

            RenderChildren(writer);
        }
        #endregion Render
        #endregion Events
    }
    #endregion FpMLTimestampBox

    #region public class FpMLCheckBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLCheckBox : CheckBox
	{
		#region Members
		private readonly ControlGUI m_Control;
		private readonly string m_CheckBoxCssClass = "full-title";
		private readonly string m_FpMLKey;
		private readonly FullConstructor m_FullCtor;
		#endregion
		#region Members Controls
		private PlaceHolder m_PlaceHolder1;
		private PlaceHolder m_PlaceHolder2;
		#endregion
		#region Accessors
		public string FpMLKey {get{return m_FpMLKey;}}
		#endregion Accessors
		#region Constructors
		public FpMLCheckBox(FullConstructor pFullCtor,bool pIsChecked,TextAlign pTextAlign,string pFpMLKey,ControlGUI pControlGUI)
		{
			m_Control         = pControlGUI;
			m_FpMLKey         = pFpMLKey;
			m_FullCtor        = pFullCtor;
            //
			Checked      = pIsChecked;
			TextAlign    = pTextAlign;
			CssClass     = m_CheckBoxCssClass;
			BackColor    = Color.Transparent;
			AutoPostBack = true;
			//
            if (MethodsGUI.IsNoneLevel(m_Control.Level))
			{
				Text      = Cst.HTMLSpace + m_Control.Name.Trim();
				ToolTip   = m_Control.Name.Trim();
			}
			if (0 != m_Control.LblWidth) 
				Width = Unit.Pixel(m_Control.LblWidth);
			//
            EnsureChildControls();
		}
		#endregion Constructors
		#region Methods
		#region CreateChildControls
		protected override void CreateChildControls()
		{

			if (MethodsGUI.IsFixedLevel(m_Control.Level))
			{
				m_PlaceHolder1 = MethodsGUI.MakeDiv(m_FullCtor,new OpenDivGUI(m_Control.Level,m_Control.Name,
					m_Control.IsDisplay,string.Empty,m_Control.LblWidth),true);
				m_PlaceHolder2 = MethodsGUI.MakeDiv(m_FullCtor,new CloseDivGUI(m_Control.Level));
			}
			base.CreateChildControls();
		}
		#endregion CreateChildControls
		#endregion Methods
		#region Events
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			if (null != m_PlaceHolder1)
				m_PlaceHolder1.RenderControl(writer);
            if (false == MethodsGUI.IsModeConsult(this.Page))
    			base.Render(writer);
			if (null != m_PlaceHolder2)
				m_PlaceHolder2.RenderControl(writer);
		}
		#endregion Render
		#endregion Events
	}
	#endregion FpMLCheckBox
	#region public class FpMLCopyPasteButton
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
	public class FpMLCopyPasteButton : Panel, IPostBackEventHandler
	{
		#region Members
		private readonly string m_Label;
		private readonly string m_ObjectName;
		private readonly string m_FieldName;
        private WCToolTipLinkButton m_CopyButton;
        private WCToolTipLinkButton m_PasteButton;
        private WCToolTipLinkButton m_PasteChoiceButton;
        private WCTooltipLabel m_LabelItem;
		private TextBox m_TextItemIdentifier;
		private readonly bool m_IsCauseValidation;
		private LiteralControl m_LiteralControl;
		private readonly int m_ItemArray;

		#endregion Members
		#region Accessors
		#region IsItemArray
		private bool IsItemArray
		{
			get {return 0<=m_ItemArray;}
		}
		#endregion IsItemArray
		#region Visible
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
				foreach (Control ctrl in base.Controls)
					ctrl.Visible = value;
			}
		}
		#endregion Visible
		#endregion Accessors
		#region Constructor
		public FpMLCopyPasteButton(string pId,string pGUIName,string pObjectName,string pFieldName,bool pIsCausesValidation,bool pIsDisplayPanel)
			: this (pId,pGUIName,pObjectName,pFieldName,pIsCausesValidation,pIsDisplayPanel,-1){}
        public FpMLCopyPasteButton(string pId, string pGUIName, string pObjectName, string pFieldName, bool pIsCausesValidation, bool pIsDisplayPanel, int pItemArray)
		{
			m_Label              = pFieldName;
			m_ObjectName         = pObjectName;
			m_FieldName          = pFieldName;
			m_IsCauseValidation  = pIsCausesValidation;
			m_ItemArray          = pItemArray;
            //
            ID                   = pId;  
            Visible              = pIsDisplayPanel;
			EnableViewState      = true;
			CssClass             = "pnlCaptureItemToCopyPaste";
			HorizontalAlign      = HorizontalAlign.Center;
			Wrap                 = false;
			if (StrFunc.IsFilled(pGUIName))
				m_Label = pGUIName.Replace(Cst.HTMLSpace,Cst.Space);
            //
            EnsureChildControls();
            // EG 20160404 Migration vs2013
            //SetEventHandler();
		}
		#endregion Constructor
		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		protected override void CreateChildControls()
		{
            #region Copy button
            m_CopyButton = new WCToolTipLinkButton
            {
                CausesValidation = m_IsCauseValidation,
                CommandName = (IsItemArray ? Cst.OperatorType.copyitem.ToString() : Cst.OperatorType.copy.ToString()),
                TabIndex = -1,
				CssClass = "fa-icon",
				Text = @"<i class='fas fa-copy'></i>",
			};
            m_CopyButton.ToolTip            = m_CopyButton.Pty.TooltipContent;
            m_CopyButton.ID                 = ID + Cst.BUT + "Copy";
			m_CopyButton.Pty.TooltipContent = "Copy this current object";

            #endregion Copy button
            //
            #region Paste button
            m_PasteButton = new WCToolTipLinkButton
            {
                CausesValidation = m_IsCauseValidation,
                CommandName = (IsItemArray ? Cst.OperatorType.pasteitem.ToString() : Cst.OperatorType.paste.ToString()),
				CssClass = "fa-icon",
				TabIndex = -1,
				Text = @"<i class='fas fa-paste'></i>",
            };
            m_PasteButton.ToolTip = m_PasteButton.Pty.TooltipContent;
			m_PasteButton.Pty.TooltipContent = "Paste the last copy of a same object type";
			m_PasteButton.ID = ID + Cst.BUT + "Paste";

            #endregion Paste button
            //			
            #region Paste Choice button
            m_PasteChoiceButton = new WCToolTipLinkButton
            {
                CausesValidation = m_IsCauseValidation,
                CommandName = (IsItemArray ? Cst.OperatorType.pastechoiceitem.ToString() : Cst.OperatorType.pastechoice.ToString()),
                TabIndex = -1,
                CssClass = "fa-icon",
                Text = @"<i class='fa fa-clipboard'></i>"
            };
            m_PasteChoiceButton.Pty.TooltipContent = "Paste a copy of your choice of a same object type";
			m_PasteChoiceButton.ToolTip = m_PasteChoiceButton.Pty.TooltipContent;
			m_PasteChoiceButton.ID = ID + Cst.BUT + "PasteChoice";
            #endregion Paste Choice Button
            //
            #region Item label control text
            m_LabelItem = new WCTooltipLabel
            {
                Height = Unit.Pixel(16),
                Width = Unit.Empty,
                CssClass = "lblCaptureItemToCopyPaste",
                Text = m_Label + " : ",
                ID = ID + Cst.LBL + "Item"
            };
            #endregion Item label control text

            #region Item label control text
            m_TextItemIdentifier = new TextBox
            {
                Wrap = false,
                CssClass = "txtCaptureItemToCopyPaste",
                ReadOnly = false,
                Text = string.Empty,
                ID = ID + Cst.TXT + "Item"
            };
            #endregion Item label control text

            m_LiteralControl = new LiteralControl(Cst.HTMLSpace);
			Controls.Add(m_CopyButton);
            Controls.Add(new LiteralControl(Cst.HTMLSpace));
			Controls.Add(m_PasteButton);
            Controls.Add(new LiteralControl(Cst.HTMLSpace));
			Controls.Add(m_PasteChoiceButton);
            Controls.Add(new LiteralControl(Cst.HTMLSpace));
            Controls.Add(m_LabelItem);
            Controls.Add(m_LiteralControl);
			Controls.Add(m_TextItemIdentifier);

            // EG 20160404 Migration vs2013
            SetEventHandler();
            base.CreateChildControls();
		}
		#endregion CreateChildControls
        #region SetEventHandler
        public void SetEventHandler()
        {
            // EG 20160404 Migration vs2013
            //this.PasteChoiceSelected += new PasteChoiceSelectedEventHandler(OnPasteChoiceSelected);
            m_CopyButton.Command += new CommandEventHandler(OnCopyPasteArray);
            m_PasteButton.Command += new CommandEventHandler(OnCopyPasteArray);
        }
        #endregion

        #endregion Methods
        #region Events
        #region OnCopyPasteArray
        protected void OnCopyPasteArray(object sender,CommandEventArgs e) 
		{
			if (System.Enum.IsDefined(typeof(Cst.OperatorType),e.CommandName))
			{
				Cst.OperatorType operatorType = (Cst.OperatorType) System.Enum.Parse(typeof(Cst.OperatorType),e.CommandName,true);
				int idClipBoard = 0;
				//
                if (StrFunc.IsFilled(e.CommandArgument.ToString()))
					idClipBoard = Convert.ToInt32(e.CommandArgument);

				if (IsItemArray)
					((ControlBase)NamingContainer).CopyPasteItem(operatorType,idClipBoard,m_ItemArray,m_TextItemIdentifier.Text);
				else
					((ControlBase)NamingContainer).CopyPaste(operatorType,idClipBoard,m_TextItemIdentifier.Text);
			}
		}
		#endregion OnCopyPasteArray
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			bool isNotConsult = (false == MethodsGUI.IsModeConsult(this.Page));

			if (isNotConsult)
			{
				#region Copy/Paste button attributes
				string guid           = ((PageBase)this.Page).GUID;
				string attributeValue = "FpmlCopyPaste('" + guid + "','" + this.UniqueID + "','";
				attributeValue       += m_ObjectName + "','" + m_FieldName + "','";
				attributeValue       += m_Label + "');return false;";
				m_PasteChoiceButton.Attributes.Add("onclick",attributeValue);
				#endregion Copy/Paste button attributes
			}
			m_PasteButton.Visible       = isNotConsult;
			m_PasteChoiceButton.Visible = isNotConsult;
			base.Render(writer);
		}
		#endregion Render
		#endregion Events
		#region Raise PasteChoice Selected
        // EG 20160404 Migration vs2013
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        //public void RaisePostBackEvent(string eventArgument)
        {
            Cst.OperatorType operatorType = IsItemArray ? Cst.OperatorType.pastechoiceitem : Cst.OperatorType.pastechoice;
            //OnPasteChoiceSelected(new PasteChoiceSelectedEventArgs(operatorType.ToString(), eventArgument));
            OnCopyPasteArray(this, new CommandEventArgs(operatorType.ToString(), eventArgument));
        }
        // EG 20160404 Migration vs2013
        //protected void OnPasteChoiceSelected(object sender, PasteChoiceSelectedEventArgs e)
        //{
        //    OnCopyPasteArray(sender,new CommandEventArgs(e.CommandName,e.Argument));
        //}
        protected void OnPasteChoiceSelected(PasteChoiceSelectedEventArgs e)
        {
            OnCopyPasteArray(this, new CommandEventArgs(e.CommandName, e.Argument));
        }
		#endregion Raise PasteChoice Selected

	}
	#endregion FpMLCopyPasteButton

	#region public class FpMLDropDownList
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLDropDownList : DropDownList
	{
		#region Members
        private readonly ControlGUI m_Control;
        private readonly string m_LabelCssClass;
        private readonly string m_DdlCssClass;
        private string m_SelectedValue;
		private readonly FullConstructor m_FullCtor;
        private string m_FpMLKey;
        private readonly string m_HelpUrl;
        private readonly Dictionary<string, string> m_LstValues;
		#endregion
        #region Members Controls
        private Label m_Label;
		private PlaceHolder m_PlaceHolder1;
		private PlaceHolder m_PlaceHolder2;
		#endregion Variables Controls
		#region Accessors
		public string FpMLKey 
		{
            set { m_FpMLKey = value; }
            get { return m_FpMLKey; }
		}
        public string SetSelectedValue { set { m_SelectedValue = value; } }
		#endregion Accessors
		#region Constructors
        public FpMLDropDownList(FullConstructor pFullCtor, string pSelectedValue, int pWidth, string pFpMLKey, ControlGUI pControlGUI, string pHelpUrl, Dictionary<string, string> pLstValues)
            : this(pFullCtor, pSelectedValue, pWidth, pFpMLKey, pControlGUI, pHelpUrl)
		{
            m_LabelCssClass = "lblCaptureTitle";
            m_DdlCssClass = "ddlCapture";
            m_LstValues = pLstValues;
            //
            EnsureChildControls();
		}
        public FpMLDropDownList(FullConstructor pFullCtor, string pSelectedValue, string pId, int pWidth, ControlGUI pControlGUI, Dictionary<string, string> pLstValues)
		{
            m_LabelCssClass = "lblCaptureTitle";
            m_DdlCssClass = "ddlCapture";
            m_SelectedValue = pSelectedValue;
            m_Control = pControlGUI;
            m_FullCtor = pFullCtor;

            CssClass = m_DdlCssClass;
            ID = pId;
            EnableViewState = true;
            m_LstValues = pLstValues;
            m_FpMLKey = string.Empty;
            if (pWidth != 0)
                Width = Unit.Pixel(pWidth);

            EnsureChildControls();
		}

        public FpMLDropDownList(FullConstructor pFullCtor, string pSelectedValue, int pWidth, string pFpMLKey, string pCaption, string pHelpUrl)
            : this(pFullCtor, pSelectedValue, pWidth, pFpMLKey, new ControlGUI(StrFunc.IsFilled(pCaption), pCaption), pHelpUrl) { }

        public FpMLDropDownList(FullConstructor pFullCtor, string pSelectedValue, int pWidth, string pFpMLKey, ControlGUI pControlGUI, string pHelpUrl)
		{
            m_LabelCssClass = "lblCaptureTitle";
            m_DdlCssClass = "ddlCapture";
            m_SelectedValue = pSelectedValue;
            m_Control = pControlGUI;
            m_FpMLKey = pFpMLKey;
            m_HelpUrl = pHelpUrl;
            m_FullCtor = pFullCtor;
            CssClass = m_DdlCssClass;
            ID = string.Empty;
            EnableViewState = true;
            if (pWidth != 0)
                Width = Unit.Pixel(pWidth);

            EnsureChildControls();
		}
		#endregion Constructors
		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		protected override void CreateChildControls()
		{
			if (m_Control.IsLabel)
			{
                m_Label = new Label
                {
                    CssClass = m_LabelCssClass,
                    Text = m_Control.Name.ToString().TrimEnd(null)
                };
                if (m_Control.IsSpaceBefore)
                    m_Label.Text = Cst.HTMLSpace + m_Label.Text;

				if (0 != m_Control.LblWidth)
					m_Label.Width = Unit.Pixel(m_Control.LblWidth);

                MethodsGUI.SetHelpUrlLink(m_Label, m_HelpUrl);
			}
			if (MethodsGUI.IsFixedLevel(m_Control.Level))
			{
                m_PlaceHolder1 = MethodsGUI.MakeDiv(m_FullCtor, new OpenDivGUI(m_Control.Level, m_Control.Name,
                    m_Control.IsDisplay, string.Empty, m_Control.LblWidth), true);
                m_PlaceHolder2 = MethodsGUI.MakeDiv(m_FullCtor, new CloseDivGUI(m_Control.Level));
			}
			else
			{
				m_PlaceHolder1 = new PlaceHolder();
				if (m_Control.IsLabel)
					m_PlaceHolder1.Controls.Add(m_Label);
				m_PlaceHolder1.Controls.Add(new LiteralControl(Cst.HTMLSpace));
			}
			//
            base.CreateChildControls();
		}
		#endregion CreateChildControls
		#region DisplayControlByMode
		protected void DisplayControlByMode(HtmlTextWriter writer)
		{
			if (MethodsGUI.IsModeConsult(Page))
			{
                TextBox txt = new TextBox
                {
                    ReadOnly = true,
                    Enabled = true,
					CssClass = EFSCssClass.Capture
				};
                if (0 != Width) 
					txt.Width = this.Width;
				if (0 < Items.Count)
					txt.Text = SelectedItem.Text;

                txt.Style.Add(HtmlTextWriterStyle.Overflow, "visible");
				txt.RenderControl(writer);
			}
			else
				base.Render(writer);
		}
		#endregion DisplayControlByMode
		#region LoadFpMLItemReference
		protected void LoadFpMLItemReference()
		{
			if ((null != m_FullCtor) && (null != m_FullCtor.ListFpMLReference))
			{
                GUITools.LoadDDLFromListFpMLReference(this, m_FpMLKey, m_FullCtor.ListFpMLReference);
			}
		}
		#endregion LoadFpMLItemReference
		#region LoadListEnumScheme
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180131 [23753] Modify
        protected void LoadListEnumScheme()
        {
            string key = m_FpMLKey.ToLower();
            // 20070814 EG -> Gestion Dropdown optionelle dans le cas des templates/pretrades
            bool isWithEmptyItem = MethodsGUI.IsStEnvironment_Template(this.Page);
            Items.Clear();

            if (System.Enum.IsDefined(typeof(CountryNameEnum), key))
                ControlsTools.DDLLoad_Country(CSTools.SetCacheOn(SessionTools.CS), this, isWithEmptyItem);
            else if (System.Enum.IsDefined(typeof(CurrencyNameEnum), key))
                ControlsTools.DDLLoad_Currency(CSTools.SetCacheOn(SessionTools.CS), this, isWithEmptyItem);
            else if (System.Enum.IsDefined(typeof(MarketNameEnum), key))
                // FI 20180131 [23753] use of DDLLoad_Market2
                ControlsTools.DDLLoad_Market2(CSTools.SetCacheOn(SessionTools.CS), this, isWithEmptyItem, false, false, false, string.Empty);
            else if ("businesscenter" == key)
                ControlsTools.DDLLoad_BusinessCenter(CSTools.SetCacheOn(SessionTools.CS), this, isWithEmptyItem);
            else
            {
                // 20070814 EG -> Gestion Dropdown optionelle dans le cas des templates/pretrades
                if (isWithEmptyItem)
                    this.Items.Add(new ListItem(string.Empty, string.Empty));
				
				// FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
				//ExtendEnum extendEnum = ExtendEnumsTools.ListEnumsSchemes[this.FpMLKey];
				ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, this.FpMLKey);
				if (extendEnum != null)
                {
                    if (extendEnum.item != null)
                    {
                        ExtendEnumValue[] item = extendEnum.Sort("ExtValue");
                        foreach (ExtendEnumValue i in item)
                        {
                            if (i != null)
                            {
                                this.Items.Add(i.ExtValue);
                                this.Items[this.Items.Count - 1].Value = i.Value;
                            }
                        }
                    }
                }
                else
                    this.Items.Add(m_SelectedValue);
            }
        }
		#endregion LoadListEnumScheme
		#endregion Methods
		#region Events
		#region OnInit
		protected override void OnInit(EventArgs e)
		{
			if ((null != m_LstValues) && (0 < m_LstValues.Count))
			{
                SortedList sortedList = new SortedList((IDictionary)m_LstValues);
                foreach (DictionaryEntry item in sortedList)
                {
                    Items.Add(item.Value.ToString());
                    Items[this.Items.Count - 1].Value = item.Key.ToString();
                }
            }
			else if (false == m_FpMLKey.EndsWith(Cst.FpML_ClassReference))
				LoadListEnumScheme();
		}
		#endregion OnInit
		#region OnLoad
        // EG 20100210 Search by Text if by Value not Found (when Enum value <> EnumAttribute serialzation value)
		protected override void OnLoad(System.EventArgs e)
		{
            //if (Cst.FpML_ClassPartyReference == m_FpMLKey)
            //    ControlsTools.DDLSelectByText(this,m_SelectedValue);
            //else
            //    ControlsTools.DDLSelectByValue(this,m_SelectedValue);

            if (Cst.FpML_ClassPartyReference == m_FpMLKey)
                ControlsTools.DDLSelectByText(this, m_SelectedValue);
            else if (false == ControlsTools.DDLSelectByValue(this, m_SelectedValue))
                ControlsTools.DDLSelectByText(this, m_SelectedValue);

		}
		#endregion OnLoad
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			if (m_FpMLKey.EndsWith(Cst.FpML_ClassReference))
			{
				this.LoadFpMLItemReference();
				if (Cst.FpML_ClassPartyReference == m_FpMLKey)
                    ControlsTools.DDLSelectByText(this, m_SelectedValue);
				else
                    ControlsTools.DDLSelectByValue(this, m_SelectedValue);
			}
			if (null != this.m_PlaceHolder1) 
				m_PlaceHolder1.RenderControl(writer);
			DisplayControlByMode(writer);
			if (null != this.m_PlaceHolder2) 
				m_PlaceHolder2.RenderControl(writer);
			RenderChildren(writer);
		}
		#endregion Render
		#endregion Events
	}
	#endregion FpMLDropDownList
	#region public class FpMLIncrButton
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
	public class FpMLIncrButton : Panel
    {
        #region Members
        private readonly Cst.OperatorType m_Operatortype;
        private readonly string m_Name;
        private readonly bool m_IsCausesValidation;
        private int m_TextItem;

        //private WCToolTipImageButton m_ButtonItem;
        private WCToolTipLinkButton m_ButtonItem;
        private Label m_ItemLabel;
        private TextBox m_TextItemNumber;
        private TextBox m_TextItemPosition;
        #endregion Members
        //
        #region Accessors
        #region Visible
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
                foreach (Control ctrl in base.Controls)
                {
                    ctrl.Visible = value;
                    foreach (Control ctrl2 in ctrl.Controls)
                        ctrl2.Visible = value;
                }
            }
        }

        #endregion Visible
        #region ItemNumber
        public int ItemNumber
        {
            set { m_TextItemNumber.Text = value.ToString(); }
            get { return Convert.ToInt32(m_TextItemNumber.Text); }
        }
        #endregion ItemNumber
        #region ItemPosition
        public int ItemPosition
        {
            set { m_TextItemPosition.Text = value.ToString(); }
            get { return Convert.ToInt32(m_TextItemPosition.Text); }
        }
        #endregion ItemPosition
        #region TextItem
        public int TextItem
        {
            set
            {
                m_TextItem = value;
                SetTextValue();
            }
        }
        #endregion TextItem
        #endregion Accessors
        //		
        #region Constructor
        public FpMLIncrButton(string pId,string pName, Cst.OperatorType pOperatortype, int pTextItem, bool pIsCausesValidation, bool pIsDisplayButton)
        {
            ID = pId;
            m_Name = pName;
            m_Operatortype = pOperatortype;
            m_TextItem = pTextItem;
            m_IsCausesValidation = pIsCausesValidation;

            Height = Unit.Pixel(20);
            HorizontalAlign = HorizontalAlign.Center;
            Visible = pIsDisplayButton;

            EnsureChildControls();
        }
		#endregion Constructor
		//
		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		protected override void CreateChildControls()
        {
            string lblClassName = string.Empty;
            string txtClassName = string.Empty;
            string pnlClassName = string.Empty;
            string tooltip = string.Empty;
            //
            if (Cst.OperatorType.clone == m_Operatortype)
            {
                lblClassName = "lblCaptureItemToClone";
                txtClassName = "txtCaptureItemToClone";
                pnlClassName = "pnlCaptureItemToClone";
                tooltip = "Add a new " + m_Name + " : specify the item to be copied and the start position to be inserted";
            }
            else if (Cst.OperatorType.add == m_Operatortype)
            {
                lblClassName = "lblCaptureItemToAdd";
                txtClassName = "txtCaptureItemToAdd";
                pnlClassName = "pnlCaptureItemToAdd";
                tooltip = "Add one or more item(s) of " + m_Name + " : specify the number of items and the start position to be inserted";
            }
            else if (Cst.OperatorType.substract == m_Operatortype)
            {
                lblClassName = "lblCaptureItemToSub";
                txtClassName = "txtCaptureItemToSub";
                pnlClassName = "pnlCaptureItemToSub";
                tooltip = "Delete one or more existing item(s) of " + m_Name + " :specify the number of items and the start position to be deleted";
            }
            CssClass = pnlClassName;
            m_ButtonItem = new WCToolTipLinkButton
            {
                CausesValidation = m_IsCausesValidation,
                ToolTip = tooltip,
                CssClass = "fa-icon",
                ID = ID + Cst.BUT,
                CommandName = m_Operatortype.ToString(),
                TabIndex = -1
            };
            m_ButtonItem.Pty.TooltipContent = tooltip;

			if (m_Operatortype == Cst.OperatorType.clone)
				m_ButtonItem.Text = @"<i class='fas fa-clone'></i>";
			else if (m_Operatortype == Cst.OperatorType.add)
				m_ButtonItem.Text = @"<i class='fas fa-plus-circle'></i>";
			else if (m_Operatortype == Cst.OperatorType.substract)
				m_ButtonItem.Text = @"<i class='fas fa-trash-alt'></i>";

            m_ButtonItem.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");

            Controls.Add(m_ButtonItem);

            Controls.Add(new LiteralControl(Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace));

            #region Item number control text
            m_TextItemNumber = new TextBox
            {
                Width = Unit.Pixel(16),
                CssClass = txtClassName,
                ID = ID + Cst.TXT + "ItemNumber"
            };
            Controls.Add(m_TextItemNumber);
            #endregion Item number control text
            //
            #region Item label control text
            m_ItemLabel = new Label
            {
                Text = " at ",
                Width = Unit.Pixel(16),
                Height = Unit.Pixel(16),
                CssClass = lblClassName,
                ID = ID + Cst.LBL + "ItemAt"
            };
            Controls.Add(m_ItemLabel);
            #endregion Item label control text
            //
            #region Item position control text
            m_TextItemPosition = new TextBox
            {
                Width = Unit.Pixel(16),
                CssClass = txtClassName,
                ID = ID + Cst.TXT + "ItemPosition"
            };
            Controls.Add(m_TextItemPosition);
            #endregion Item position control text
            //
            SetTextValue();
            SetEventHandler();
            //
            base.CreateChildControls();
        }
        #endregion CreateChildControls
        #region SetEventHandler
        public void SetEventHandler()
        {
            m_ButtonItem.Command += new CommandEventHandler(OnAddOrDeleteItem);
        }
        #endregion
        #region SetTextValue
        private void SetTextValue()
        {
            m_TextItemNumber.Text = Convert.ToString((Cst.OperatorType.clone == m_Operatortype) ? m_TextItem : 1);
            m_TextItemPosition.Text = Convert.ToString((Cst.OperatorType.substract == m_Operatortype) ? m_TextItem : m_TextItem + 1);
        }
        #endregion SetTextValue
        #endregion Methods
        //
        #region Events
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            if (false == MethodsGUI.IsModeConsult(Page))
                base.Render(writer);
        }
        #endregion Render
        #region OnAddOrDeleteItem
        private void OnAddOrDeleteItem(object sender, CommandEventArgs e)
        {
            ((ComplexControls.ObjectArray)Parent).OnAddOrDeleteItem(sender, e);
        }
        #endregion OnAddOrDeleteItem
        #endregion Events
    }
	#endregion FpMLIncrButton
	#region public class FpMLLabel
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLLabel : Label
	{
		#region Members
		private readonly ControlGUI m_ControlGUI;
		private readonly string m_LabelCssClass;
		private readonly string m_FpMLKey;
		#endregion
		#region Members Controls
		private Label      m_Label;
		#endregion
		#region Accessors
		public string FpMLKey {get{return m_FpMLKey;}}
		#endregion Accessors
		#region Constructors
		public FpMLLabel(string pText,int pWidth,string pFpMLKey,ControlGUI pControlGUI)
			:this(pText,pWidth,pFpMLKey,pControlGUI,true){}
		public FpMLLabel(string pText,int pWidth,string pFpMLKey,ControlGUI pControlGUI,bool pIsVisible)
		{
            m_LabelCssClass = "lblCaptureTitle";
            m_ControlGUI  = pControlGUI;
			m_FpMLKey     = pFpMLKey;
			//
            Text     = pText;
			Width    = Unit.Pixel(pWidth);
			Visible  = pIsVisible;
			CssClass = "lblCaptureId";
            //
			EnsureChildControls();
		}
		#endregion Constructors
		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		protected override void CreateChildControls()
		{
			if (m_ControlGUI.IsLabel)
			{
                m_Label = new Label
                {
                    CssClass = m_LabelCssClass,
                    Text = Cst.HTMLSpace + m_ControlGUI.Name.ToString().Trim() + Cst.HTMLSpace
                };
                if (0 != m_ControlGUI.LblWidth)
					m_Label.Width = Unit.Pixel(m_ControlGUI.LblWidth);
			}
		}
		#endregion CreateChildControls
		#endregion Methods
		#region Events
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			if (null != m_ControlGUI)
			{
				if (m_ControlGUI.IsLabel) 
					m_Label.RenderControl(writer); 
			}
			base.Render(writer);
		}
		#endregion Render
		#endregion Events
	}
	#endregion FpMLLabel
	#region public class FpMLLabelOnly
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLLabelOnly : Label
	{
		#region Members
		private readonly string m_LabelCssClass;
		private readonly string m_HelpUrl;
		#endregion
		#region Constructors
		public FpMLLabelOnly(ControlGUI pControlGUI,string pHelpUrl)
		{
            m_LabelCssClass = "lblCaptureTitle";
			m_HelpUrl      = pHelpUrl;
			
            Text      = pControlGUI.Name;
            CssClass  = m_LabelCssClass;
			if (pControlGUI.IsSpaceBefore)
				Text  = Cst.HTMLSpace + this.Text;
			if (pControlGUI.LblWidth!=0) 
				Width = Unit.Pixel(pControlGUI.LblWidth);
		}
		#endregion Constructors
		#region Events
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			MethodsGUI.SetHelpUrlLink(this,m_HelpUrl);
			base.Render(writer);
		}
		#endregion Render
		#endregion Events
	}
	#endregion FpMLLabelOnly
	#region public class FpMLRadioButton
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLRadioButton : RadioButtonList
	{
		#region Members
		private readonly ControlGUI m_Control;
        private readonly string m_LabelCssClass = "full-title";
		private readonly string m_FpMLKey;
		private readonly FullConstructor m_FullCtor;
		#endregion
        #region  Members Controls
        private PlaceHolder m_PlaceHolder1;
		private PlaceHolder m_PlaceHolder2;
		#endregion
		#region Accessors
		public string FpMLKey {get{return m_FpMLKey;}}
		#endregion Accessors
		#region Constructors
		public FpMLRadioButton(FullConstructor pFullCtor,ControlGUI pControlGUI,string pFpMLKey,int pWidth,params string[] pItems)
		{
			m_Control             = pControlGUI;
			m_FpMLKey             = pFpMLKey;
			m_FullCtor            = pFullCtor;
			//
            CssClass         = m_LabelCssClass;
			this.RepeatLayout	= RepeatLayout.Flow;
			this.RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Horizontal;
			AutoPostBack     = true;
			if (pWidth!=0) 
				Width = Unit.Pixel(pWidth);
			for (int i=0;i<pItems.Length;i++)
				Items.Add(pItems[i].ToString());
			//
            EnsureChildControls();
		}
		#endregion Constructors
		#region Methods
		#region CreateChildControls
		protected override void CreateChildControls()
		{
			if (MethodsGUI.IsFixedLevel(m_Control.Level))
			{
				m_PlaceHolder1 = MethodsGUI.MakeDiv(m_FullCtor,new OpenDivGUI(m_Control.Level,m_Control.Name,
					m_Control.IsDisplay,string.Empty,m_Control.LblWidth),true);
				m_PlaceHolder2 = MethodsGUI.MakeDiv(m_FullCtor,new CloseDivGUI(m_Control.Level));
			}
			base.CreateChildControls();
		}
		#endregion CreateChildControls
		#endregion Methods
		#region Events
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
            if (null != m_PlaceHolder1)
                m_PlaceHolder1.RenderControl(writer);

            Enabled = (false == MethodsGUI.IsModeConsult(this.Page));

            if (false == Enabled)
            {
                Label lbl;
                RadioButtonList rbl = (RadioButtonList)this;
                for (int i = 0; i < rbl.Items.Count; i++)
                {
                    if (rbl.Items[i].Selected)
                    {
                        lbl = new Label
                        {
                            CssClass = CssClass,
                            Text = "[" + Items[i].Text + "]"
                        };
                        PlaceHolder placeHolder3 = new PlaceHolder();
                        placeHolder3.Controls.Add(lbl);
                        placeHolder3.RenderControl(writer);
                        break;
                    }
                }
            }
            else
                base.Render(writer);

			if (null != m_PlaceHolder2) 
				m_PlaceHolder2.RenderControl(writer);
		}
		#endregion Render
		#endregion Events
	}
	#endregion FpMLRadioButton
	#region public class FpMLSchemeBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLSchemeBox : TextBox
	{
		#region Members
		private readonly ControlGUI m_ControlGUI;
		private readonly string m_LabelCssClass;
		private readonly ArrayList m_aValidator;
		private readonly string m_FpMLKey;
		private readonly string m_HelpUrl;
		//
		private Label           m_Label;
		private LiteralControl  m_LiteralControl;
        private WCToolTipLinkButton m_Button;
		#endregion
		//
		#region Accessors
		public string FpMLKey {get{return m_FpMLKey;}}
		#endregion Accessors
		//
		#region Constructors
		public FpMLSchemeBox(string pText,int pWidth,string pFpMLKey,string pCaption,bool pIsAutoPostBack,
			string pHelpUrl,params Validator[] pValidator)
			:this(pText,pWidth,pFpMLKey,new ControlGUI(StrFunc.IsFilled(pCaption),pCaption),pIsAutoPostBack,pHelpUrl,pValidator){}
		public FpMLSchemeBox(string pText,int pWidth,string pFpMLKey,ControlGUI pControlGUI,bool pIsAutoPostBack,string pHelpUrl,
			params Validator[] pValidator)
		{
			m_aValidator    = new ArrayList();
			m_LabelCssClass = "lblCaptureTitle";
			//
			m_ControlGUI = pControlGUI;
			m_FpMLKey    = pFpMLKey;
			m_HelpUrl    = pHelpUrl;
			Text         = pText;
			Width        = Unit.Pixel(pWidth);
			AutoPostBack = pIsAutoPostBack;
			//
            Attributes.Add("oldvalue", pText + string.Empty);
			//
			for (int i=0;i<pValidator.Length;i++)
				m_aValidator.Add(pValidator[i]);
            //			
            EnsureChildControls();
		}
		#endregion Constructors
		//
		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		protected override void CreateChildControls()
		{
			if (m_ControlGUI.IsLabel)
			{
                m_Label = new Label
                {
                    CssClass = m_LabelCssClass,
                    Text = Cst.HTMLSpace + m_ControlGUI.Name.ToString().Trim() + Cst.HTMLSpace
                };
                if (0 != m_ControlGUI.LblWidth)
					m_Label.Width = Unit.Pixel(m_ControlGUI.LblWidth);

				MethodsGUI.SetHelpUrlLink(m_Label,m_HelpUrl);
			}
			for (int i=0;i<m_aValidator.Count;i++)
			{
				Validator validator = (Validator) m_aValidator[i];
				Controls.Add(validator.CreateControl(ID));
			}
			ID       = string.Empty;
			CssClass = EFSCssClass.Capture;
            Style.Add(HtmlTextWriterStyle.Overflow, "hidden");

            m_LiteralControl = new LiteralControl
            {
                Text = Cst.HTMLSpace
            };

            m_Button = new WCToolTipLinkButton() 
			{
				CssClass = "fa-icon",
				Text = "<i class='fas fa-ellipsis-h'></i>",
				TabIndex = -1
			};
			m_Button.Pty.TooltipContent = "Open " + m_FpMLKey + " list";

			base.CreateChildControls();
		}
		#endregion CreateChildControls
		#region GetKeyReferentiel
		private string GetKeyReferentiel()
		{
			switch (m_FpMLKey)
			{
				case "floatingRateIndex":
					return Cst.OTCml_TBL.RATEINDEX.ToString();
				case "bookId":
					return Cst.OTCml_TBL.BOOK.ToString();
				case "cssCriteriaCss":
					return Cst.OTCml_TBL.CSS.ToString();
				case "Party":
					return Cst.OTCml_TBL.ACTOR.ToString();
				case "assetFxRateId":
					return Cst.OTCml_TBL.ASSET_FXRATE.ToString();
				default:
					return m_FpMLKey;
			}
		}
		#endregion GetKeyReferentiel
		#endregion Methods
		//
        #region Events
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            //20071212 FI Ticket 16012 => Migration Asp2.0
            foreach (WebControl ctrl in this.Controls)
                ((BaseValidator)ctrl).ControlToValidate = this.ID;
            //
            base.OnPreRender(e);
        }
        #endregion
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            bool isConsult = MethodsGUI.IsModeConsult(this.Page);
            if (null != m_ControlGUI)
            {
                if (m_ControlGUI.IsLabel)
                    m_Label.RenderControl(writer);
            }
            this.Attributes.Add("onblur", "OnCtrlChanged('" + this.UniqueID + "','');return false");

            m_LiteralControl.RenderControl(writer);
            this.ReadOnly = isConsult;
            base.Render(writer);

            //20071212 FI Ticket 16012 => Migration Asp2.0
            //foreach (WebControl ctrl in this.Controls)
            //	((BaseValidator) ctrl).ControlToValidate=this.ID;

            RenderChildren(writer);

            if (!isConsult)
            {
                m_LiteralControl.RenderControl(writer);

                // impossible to get IDISDA, without adding these two fields
                // because PK est automatically retrieved for first control,
                // must specify IDISDA for 2nd.
                string clientIdForDumpKeyField = this.UniqueID;
                string sqlColumn = null;
                string clientIdForDumpSqlColumn = null;
                if (Cst.OTCml_TBL.RATEINDEX.ToString() == GetKeyReferentiel())
                {
                    sqlColumn = "IDISDA";
                    clientIdForDumpSqlColumn = this.UniqueID;
                }
                else if (Cst.OTCml_TBL.CSS.ToString() == GetKeyReferentiel())
                {
                    sqlColumn = "a1_IDENTIFIER";
                    clientIdForDumpSqlColumn = this.UniqueID;
                }

                ControlsTools.SetOnClientClick_ButtonReferential(m_Button, GetKeyReferentiel(), null,
                    Cst.ListType.Repository,
                    false,
                    null, clientIdForDumpKeyField,
                    sqlColumn, clientIdForDumpSqlColumn,
                    null, null, null,null);
                m_Button.RenderControl(writer);
            }
        }
        #endregion Render
        #endregion Events
	}
	#endregion FpMLSchemeBox
	#region public class FpMLTextBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLTextBox : TextBox
	{
		#region Members
		private readonly ControlGUI m_Control;
		private readonly string m_LabelCssClass;
		private readonly string m_Caption;
		private readonly string m_HelpUrl;
		private readonly FullConstructor m_FullCtor;
		private readonly ArrayList m_aValidator;
		private readonly string m_FpMLKey;
		private bool m_IsMultilineMode;
		private bool m_IsLocked;
		#endregion
		//
		#region Members Controls
		private Label       m_Label;
		private Label       m_Label2;
		private PlaceHolder m_PlaceHolder1;
		private PlaceHolder m_PlaceHolder2;
		#endregion
		//
		#region Accessors
		public EFSRegex.TypeRegex Regex {get{return m_Control.Regex;}}
		public string FpMLKey {get{return m_FpMLKey;}}
		public bool IsMultilineMode {
			set { m_IsMultilineMode = value; }
			get { return m_IsMultilineMode; } 
		}
		public bool IsLocked
		{
			set {m_IsLocked = value;}
			get {return m_IsLocked;}
		}
		// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		public Label AssociatedLabel
		{
			get { return m_Label;}
		}
		#endregion Accessors
		//
		#region Constructors
		public FpMLTextBox(FullConstructor pFullCtor,string pCssClass,string pText,int pWidth,string pFpMLKey,ControlGUI pControlGUI,string pHelpUrl,
			params Validator[] pValidator)
			:this(pFullCtor,pCssClass,pText,pWidth,pFpMLKey,pControlGUI,null,false,string.Empty,pHelpUrl,pValidator){}
		public FpMLTextBox(FullConstructor pFullCtor,string pText,int pWidth,string pFpMLKey,string pCaption,string pCaption2,bool pIsLocked,string pId,
			string pHelpUrl,params Validator[] pValidator)
			:this(pFullCtor,EFSCssClass.Capture,pText,pWidth,pFpMLKey, new ControlGUI(StrFunc.IsFilled(pCaption),pCaption),
			pCaption2,pIsLocked,(StrFunc.IsEmpty(pId)?string.Empty:pId),pHelpUrl,pValidator){}

		public FpMLTextBox(FullConstructor pFullCtor,string pText,int pWidth,string pFpMLKey,ControlGUI pControlGUI,
			string pCaption,bool pIsLocked,string pId,string pHelpUrl,params Validator[] pValidator)
			:this(pFullCtor,EFSCssClass.Capture,pText,pWidth,pFpMLKey, pControlGUI,pCaption,pIsLocked,
			(StrFunc.IsEmpty(pId)?string.Empty:pId),pHelpUrl,pValidator){}
		public FpMLTextBox(FullConstructor pFullCtor,string pText,int pWidth,string pCaption,bool pIsLocked,string pId,string pHelpUrl,params Validator[] pValidator)
			:this(pFullCtor,EFSCssClass.Capture,pText,pWidth,null, new ControlGUI(StrFunc.IsFilled(pCaption),pCaption),null,pIsLocked,pId,
			pHelpUrl,pValidator){}
		public FpMLTextBox(FullConstructor pFullCtor,string pCssClass,string pText,int pWidth,string pFpMLKey,ControlGUI pControlGUI,
			string pCaption,bool pIsLocked,string pId,string pHelpUrl,Validator[] pValidator)
		{
			
            m_aValidator  = new ArrayList();
			m_LabelCssClass = "lblCaptureTitle";
			//
			m_Control     = pControlGUI;
			m_FpMLKey     = pFpMLKey;
			m_Caption     = pCaption;
			m_HelpUrl     = pHelpUrl;
			m_FullCtor    = pFullCtor;
			//
			Text          = pText;
			Width         = Unit.Pixel(pWidth);
			m_IsLocked    = pIsLocked;
			ID            = pId;
			CssClass      = pCssClass;
			//
			if (null != pValidator)
			{
				for (int i=0;i<pValidator.Length;i++)
					m_aValidator.Add(pValidator[i]);
			}
			//
            Style.Add(HtmlTextWriterStyle.Overflow, "hidden");
			//
			EnsureChildControls();
		}
		#endregion Constructors
		//
		#region Methods
		#region CreateChildControls
		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		protected override void CreateChildControls()
		{
			if (m_Control.IsLabel)
			{
                m_Label = new Label
                {
                    CssClass = m_LabelCssClass,
                    Text = m_Control.Name.ToString().Trim() + Cst.HTMLSpace,
                    Height = this.Height
                };
                if (m_Control.IsSpaceBefore)
					m_Label.Text  = Cst.HTMLSpace + m_Label.Text;
				if (0 != m_Control.LblWidth)
					m_Label.Width = Unit.Pixel(m_Control.LblWidth);

				MethodsGUI.SetHelpUrlLink(m_Label,m_HelpUrl);
			}
			if (StrFunc.IsFilled(m_Caption))
			{
                m_Label2 = new Label
                {
                    CssClass = m_LabelCssClass,
                    Text = m_Caption + Cst.HTMLSpace
                };
            }

			if (MethodsGUI.IsFixedLevel(m_Control.Level))
			{
				m_PlaceHolder1 = MethodsGUI.MakeDiv(m_FullCtor,new OpenDivGUI(m_Control.Level,m_Control.Name+Cst.HTMLSpace,
					m_Control.IsDisplay,string.Empty,m_Control.LblWidth),true);
				this.BackColor=Color.Transparent;
				m_PlaceHolder2 = MethodsGUI.MakeDiv(new CloseDivGUI(m_Control.Level));
			}
			else
			{
				m_PlaceHolder1 = new PlaceHolder();
				if (m_Control.IsLabel)
				{
					m_PlaceHolder1.Controls.Add(m_Label);
					m_PlaceHolder1.Controls.Add(new LiteralControl(Cst.HTMLSpace));

				}
				if (StrFunc.IsFilled(m_Caption))
				{
					m_PlaceHolder1.Controls.Add(m_Label2);
					m_PlaceHolder1.Controls.Add(new LiteralControl(Cst.HTMLSpace));
				}
			}
			for (int i=0;i<m_aValidator.Count;i++)
			{
				Validator validator = (Validator) m_aValidator[i];
				Controls.Add(validator.CreateControl(this.ID));
				if (EFSRegex.IsNumber(validator.RegexTemplate))
					Style.Add(HtmlTextWriterStyle.TextAlign,"right");
			}
			base.CreateChildControls();
		}
		#endregion CreateChildControls
		#region SetMultilineAttribute
		public void SetMultilineAttribute()
		{
			SetMultilineAttribute(50,450);
		}
		public void SetMultilineAttribute(int pHeight,int pWidth)
		{
			this.TextMode          = TextBoxMode.MultiLine;
			this.CssClass          = EFSCssClass.CaptureMultiline;
			this.Height            = Unit.Pixel(pHeight);
			this.Width             = Unit.Pixel(pWidth);
			this.Style["overflow"] = "auto"; 
			m_IsMultilineMode      = true;
			if (m_Control.IsLabel)
				m_Label.Height = Unit.Pixel(pHeight);

		}
		#endregion SetMultilineAttribute
		#endregion Methods
        //
        #region Events
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            //20071212 FI Ticket 16012 => Migration Asp2.0
            foreach (WebControl ctrl in this.Controls)
                ((BaseValidator)ctrl).ControlToValidate = this.ID;
            //
            base.OnPreRender(e);
        }
        #endregion
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            if (null != m_PlaceHolder1)
                m_PlaceHolder1.RenderControl(writer);
            //
            ReadOnly = (MethodsGUI.IsModeConsult(this.Page) && ("txtTrade" != this.ID) && ("txtEvent" != this.ID)) || m_IsLocked;
            //
            base.Render(writer);
            //
            if (null != m_PlaceHolder2)
                m_PlaceHolder2.RenderControl(writer);
            //
            //20071212 FI Ticket 16012 => Migration Asp2.0
            //foreach (WebControl ctrl in this.Controls)
            //	((BaseValidator) ctrl).ControlToValidate=this.ID;
            //
            RenderChildren(writer);
        }
        #endregion Render
        #endregion Events
	}
	#endregion FpMLTextBox
	#region public class FpMLTextBoxForId
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class FpMLTextBoxForId : TextBox
	{
		#region Members 
		private readonly string m_ContainerIdName;
		private readonly bool m_IsLocked;
		#endregion
        #region Members Controls
        private WCTooltipLabel m_Label;
		#endregion
		#region Constructor
		public FpMLTextBoxForId(string pText) : this(pText, string.Empty,false){}
		public FpMLTextBoxForId(string pText,bool pIsLocked) : this(pText, string.Empty,pIsLocked){}
		public FpMLTextBoxForId(string pText,string pContainerIdName) : this(pText, pContainerIdName,false){}
		public FpMLTextBoxForId(string pText, string pContainerIdName,bool pIsLocked)
		{
			m_ContainerIdName = pContainerIdName;
            m_IsLocked        = pIsLocked;

            Text = pText;
            CssClass     = "txtCaptureId";
			AutoPostBack = true;
            Attributes.Add("oldvalue", pText);
			EnsureChildControls();
		}
		#endregion Constructor
		#region Methods
		#region CreateChildControls
		protected override void CreateChildControls()
		{
            m_Label = new WCTooltipLabel
            {
                Text = "id",
                Width = Unit.Pixel(15),
                CssClass = "lblCaptureId"
            };
            if (StrFunc.IsFilled(m_ContainerIdName))
                m_Label.Pty.TooltipContent = m_ContainerIdName;
			base.CreateChildControls();
		}
		#endregion CreateChildControls
        #endregion Methods
        #region Events
        #region Render
        protected override void Render(HtmlTextWriter writer)
		{
            m_Label.RenderControl(writer);
			ReadOnly = MethodsGUI.IsModeConsult(this.Page) || m_IsLocked;
			base.Render(writer);
			RenderChildren(writer);
		}
		#endregion Render
		#endregion Events
	}
	#endregion FpMLTextBoxForId
	//
	#region public class GUITools
	public class GUITools
	{
		#region public static void LoadDDLFromListFpMLReference
		public static void LoadDDLFromListFpMLReference(DropDownList pDDL,string pKey, ArrayList pListReference)
		{
			LoadDDLFromListFpMLReference(pDDL,pKey,pListReference,true); //MethodsGUI.IsStEnvironment_Template(pDDL.Page));
		}
		public static void LoadDDLFromListFpMLReference(DropDownList pDDL,string pKey, ArrayList pListReference,bool pIsWithEmpty)
		{
			pDDL.Items.Clear();
			if (ArrFunc.IsFilled( pListReference))
			{
				for (int i=0;i<pListReference.Count;i++)
				{
					FpMLItemReference itemReference = (FpMLItemReference) pListReference[i];
					if (pKey.ToUpper() == itemReference.code.ToUpper())
					{
						pDDL.Items.Add(itemReference.ExtValue);
						pDDL.Items[pDDL.Items.Count-1].Value = itemReference.Value;
					}
				}
			}
			// 20070814 EG -> Gestion Dropdown optionelle
			if (pIsWithEmpty)
				pDDL.Items.Add(new ListItem(string.Empty, string.Empty));
		}
		#endregion
	}
	#endregion
	//

	#region PasteChoiceSelectedEventArgs
	public class PasteChoiceSelectedEventArgs : EventArgs
	{
		#region Members
		private readonly string m_CommandName;
		private readonly string m_Argument;
		#endregion Members
		#region Accessors
		public string CommandName
		{
			get {return m_CommandName;}
		}
		public string Argument
		{
			get {return m_Argument;}
		}
		#endregion Accessors
		#region Constructors
		public PasteChoiceSelectedEventArgs(string pCommandName,string pArgument)
		{
			m_CommandName = pCommandName;
			m_Argument    = pArgument;
		}
		#endregion Constructors
	}
	#endregion PasteChoiceSelectedEventArgs

}

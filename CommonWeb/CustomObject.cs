using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

// EG 20170918 [23342] Suppression Appel WCToolTipImageCalendar
// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{


    /// <summary>
    /// Représente une liste de CustomObject
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customObjects", IsNullable = false)]
    public partial class CustomObjects
    {
        #region Members
        [System.Xml.Serialization.XmlIgnore()]
        public bool styleSpecified;
        [System.Xml.Serialization.XmlAttribute()]
        public string style;
        [System.Xml.Serialization.XmlElement()]
        public CustomObject[] customObject;
        #endregion Members


        /// <summary>
        /// Génère une HTML table. Chaque cellule est pilotée par le CustomObject.
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControDataEnabled">Si true force les contrôles de saisie à être Enabled</param>
        /// <returns></returns>
        /// FI 20200602 [25370] Add pIsControDataEnabled
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Appel CreateTooglePanel et CreateBodyPanel via ControlsTools
        public Panel CreateTable(PageBase pPage, Boolean pIsControDataEnabled, string pMainMenuClass)
        {
            Table tbl = new Table()
            {
                ID = "tblCustomObjects",
                CellPadding = 0,
                CellSpacing = 0
            };

            TableRow tr = new TableRow();
            for (int i = 0; i < ArrFunc.Count(customObject); i++)
            {
                TableCell td = customObject[i].WriteCell(pPage, false);
                
                // FI 20200602 [25370] 
                if (pIsControDataEnabled && customObject[i].IsControlData)
                    ((WebControl)td.Controls[0]).Enabled = true;

                if (customObject[i].Control == CustomObject.ControlEnum.hr ||
                    customObject[i].Control == CustomObject.ControlEnum.hline)
                {
                    tbl.Controls.Add(tr);
                    tr = new TableRow();
                    tr.Controls.Add(td);
                    tbl.Controls.Add(tr);
                    tr = new TableRow();
                }
                else
                {
                    tr.Controls.Add(td);
                }
            }
            tbl.Controls.Add(tr);

            if (styleSpecified)
            {
                ControlsTools.SetStyleList(tbl.Style, style);
                if (BoolFunc.IsTrue(tbl.Style["istableH"]))
                {
                    #region TableHeader
                    WCTogglePanel togglePanel = ControlsTools.CreateTogglePanel(tbl, pPage.CSSModeEnum, pMainMenuClass);
                    togglePanel.AddContent(tbl);
                    TableTools.RemoveStyleTableHeader(tbl);
                    return togglePanel;
                    #endregion TableHeader
                }
                else
                {
                    WCBodyPanel bodyPanel = ControlsTools.CreateBodyPanel(tbl, pPage.CSSModeEnum, pMainMenuClass);
                    bodyPanel.AddContent(tbl);
                    return bodyPanel;
                }
            }
            else
            {
                WCBodyPanel bodyPanel = ControlsTools.CreateBodyPanel(tbl, pPage.CSSModeEnum, pMainMenuClass);
                bodyPanel.AddContent(tbl);
                return bodyPanel;
            }
        }
    }



    /// <summary>
    /// Classe chargée de piloter les contrôles disponibles sur une form
    /// </summary>
    /// FI 20140708 [20179] add match
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectButton))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectLabel))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectDisplay))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectTextBox))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectImage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectDropDown))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectHyperLink))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectBanner))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectDDLBanner))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectOpenBanner))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectRowHeader))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectRowFooter))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectPanel))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CustomObjectTimestamp))]
    [System.Xml.Serialization.XmlRootAttribute("customObject", IsNullable = false)]
    /// EG 20170918 [23342] CustomObjectTimestamp 
    public partial class CustomObject
    {
        #region Constants
        /// <summary>
        /// 
        /// </summary>
        public const char KEY_SEPARATOR = '_';
        #endregion Constants
        #region ControlEnum
        /// <summary>
        /// Liste des différents type de contrôle
        /// </summary>
        /// EG 20170918 [23342] Add timestamp 
        public enum ControlEnum
        {
            // Look
            banner,
            buttonbanner,
            ddlbanner,
            openbanner,
            rowheader,
            rowfooter,
            br,
            fill,
            hr,
            hline,
            space,
            vline,
            //
            button,
            checkbox,
            htmlcheckbox,
            dropdown,
            optgroupdropdown,
            htmlselect,
            cellheader,
            cellfooter,
            display,
            hiddenbox,
            hyperlink,
            image,
            label,
            textbox,
            quickinput,

            panel,

            timestamp, 
            Unknown,
        }
        #endregion Enum ControlEnum

        #region LockedEnum
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public enum LockedEnum
        {
            create,
            modif,
            modifypostevts,
            modifyfeesuninvoiced,
            allocatedinvoice,
            tradeRiskCorrection,
        }
        #endregion Enum LockedEnum

        #region Members
        protected string m_clientid;
        protected string m_accesskey;
        protected string m_postback;
        protected string m_postbackEvent;    //"onclick","onblur", etc.....
        protected string m_postbackMethod;   //
        protected string m_postbackArgument; //
        protected string m_required;
        //
        protected string m_lockedCreate;
        protected string m_lockedModify;
        protected string m_lockedModifyPostEvts;
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected string m_lockedModifyFeesUninvoiced;
        protected string m_lockedAllocatedInvoice;
        //
        protected DictionaryEntry[] m_locked;

        protected string m_helpUrl;
        protected string m_helpSchema;
        protected string m_helpElement;
        protected string m_keepInQuickInput;
        //
        private string m_width;
        private string m_height;
        private string m_regEx;
        private string m_datatype;
        private string m_style;
        private string m_enabled;
        private string m_cssClass;
        private string m_colspan;
        private string m_rowspan;
        private string m_caption;
        private string m_toolTip;
        private string m_toolTipTitle;
        private string m_defaultValue;
        private ControlEnum m_control;
        private string m_occurs;
        /// <summary>
        /// Permet de positionner des infos diverses separees de ; => ie Misc="AA:00;BB:11"
        /// </summary>
        private string m_misc;       
        /// <summary>
        /// Permet de positionner des attributs divers separees de ; => ie attib="AA:00;BB:11" donne
        /// </summary>
        private string m_attributes; 
        private string m_fixedCol;
        
        /// <summary>
        /// Obtient ou Définit les propriétés du contrôle lorsque la saisie est en mode "Match"
        /// </summary>
        /// FI 20140708 [20179] add  
        private string m_match;
        // FI 20191227 [XXXXX] Add
        private string m_autocomplete;
        /// <summary>
        /// Déclare l'id du contrôle associé pour l'affichage dans un header de panel
        /// </summary>
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        private string m_headlinkinfo;
        #endregion variables locales

        #region property
        #region public ClientId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsClientId
        {
            get { return (!this.ClientId.Equals("undefined")); }
        }
        /// <summary>
        /// Obtient ou définie l'identifiant unique 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("clientid")]
        public string ClientId
        {
            set { m_clientid = GetValidValue(value); }
            get { return m_clientid; }
        }
        #endregion ClientId
        #region public AccessKey
        /// <summary>
        /// Obtient ou définit la touche d'accès rapide qui vous permet de naviguer rapidement vers le contrôle 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("accesskey")]
        public string AccessKey
        {
            set { m_accesskey = GetValidValue(value); }
            get { return m_accesskey; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsAccessKey
        {
            get { return (!this.AccessKey.Equals("undefined")); }
        }
        #endregion AccessKey
        #region public postback
        [System.Xml.Serialization.XmlAttribute("postback")]
        public string PostBack
        {
            set
            {
                m_postback = GetValidValue(value, true);
                if ("undefined" == m_postback)
                    m_postback = "no";
                else if (BoolFunc.IsTrue(m_postback))
                    m_postback = "yes";
                else
                    m_postback = "no";
            }
            get { return m_postback; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAutoPostBack
        {
            get { return (PostBack == "yes"); }
        }
        #endregion postback
        #region public PostBackEvent
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsPostBackEvent
        {
            get { return (!this.PostBackEvent.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("postbackevent")]
        public string PostBackEvent
        {
            set { m_postbackEvent = GetValidValue(value, true); }
            get { return m_postbackEvent; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string DefaultPostBackEvent
        {
            get { return "onclick"; }
        }
        #endregion ContainsPostBackEvent
        //
        #region PostBackMethod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsPostBackMethod
        {
            get { return (!this.PostBackMethod.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("postbackmethod")]
        public string PostBackMethod
        {
            set { m_postbackMethod = GetValidValue(value, false); }
            get { return m_postbackMethod; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string DefaultPostBackMethod
        {
            get { return "OnCtrlChanged"; }
        }
        #endregion PostBackArgument
        //
        #region PostBackArgument
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsPostBackArgument
        {
            get { return (!this.PostBackArgument.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("postbackargument")]
        public string PostBackArgument
        {
            set { m_postbackArgument = GetValidValue(value, true); }
            get { return m_postbackArgument; }
        }
        #endregion PostBackArgument
        //
        #region public Required
        [System.Xml.Serialization.XmlAttribute("required")]
        public string Required
        {
            set
            {
                m_required = GetValidValue(value, true);
                if ("undefined" == m_required)
                    m_required = "yes";
                else if (BoolFunc.IsTrue(m_required))
                    m_required = "yes";
                else
                    m_required = "no";
            }
            get { return m_required; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsMandatory
        {
            get { return (Required == "yes"); }
        }
        #endregion Required
        //
        #region public Locked
        [System.Xml.Serialization.XmlAttribute("locked")]
        public string Locked
        {
            set
            {
                string locked = GetValidValue(value, true);
                if ("undefined" != locked)
                {
                    string[] aLocked = locked.Split(';');
                    if (ArrFunc.IsFilled(aLocked))
                    {
                        m_locked = new DictionaryEntry[aLocked.Length];

                        for (int i = 0; i < aLocked.Length; i++)
                        {
                            if (StrFunc.IsFilled(aLocked[i]))
                            {
                                string[] aLockedValue = aLocked[i].Split(':');
                                if (ArrFunc.IsFilled(aLockedValue) && (2 == aLockedValue.Length))
                                    m_locked[i] = new DictionaryEntry(aLockedValue[0], aLockedValue[1] == "yes");
                            }
                        }
                    }
                }
            }
            //get { return "create:" + m_lockedCreate + ";modify:" + m_lockedModify + ";modifypostevts:" + m_lockedModifyPostEvts; }
        }
        #region IsLockedCreate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLockedCreate
        {
            get { return IsLocked(LockedEnum.create.ToString()); }
            set { SetLocked(LockedEnum.create.ToString(), value); }
            //set { m_lockedCreate = (value ? "yes" : "no"); }
        }
        #endregion IsLockedCreate
        #region IsLockedModify
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLockedModify
        {
            get { return IsLocked(LockedEnum.modif.ToString()); }
            set { SetLocked(LockedEnum.modif.ToString(), value); }
        }
        #endregion IsLockedModify
        #region IsLockedModifyPostEvts
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLockedModifyPostEvts
        {
            get
            {
                DictionaryEntry item = GetLocked(LockedEnum.modifypostevts.ToString());
                return (null == item.Value) || (bool)item.Value;
            }
            set { SetLocked(LockedEnum.modifypostevts.ToString(), value); }
        }
        #endregion IsLockedModifyPostEvts
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public bool IsLockedModifyFeesUninvoiced
        {
            get
            {
                DictionaryEntry item = GetLocked(LockedEnum.modifyfeesuninvoiced.ToString());
                return (null == item.Value) || (bool)item.Value;
            }
            set { SetLocked(LockedEnum.modifyfeesuninvoiced.ToString(), value); }
        }
        #region IsLockedAllocatedInvoice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLockedAllocatedInvoice
        {
            get
            {
                DictionaryEntry item = GetLocked(LockedEnum.allocatedinvoice.ToString());
                return (null == item.Value) || (bool)item.Value;
            }
            set
            {
                SetLocked(LockedEnum.allocatedinvoice.ToString(), value);
            }
        }
        #endregion IsLockedAllocatedInvoice
        #region IsLockedSpecified
        public bool IsLockedSpecified
        {
            get { return (null != m_locked) && ArrFunc.IsFilled(m_locked); }
        }
        #endregion IsLockedSpecified
        #endregion Locked

        #region public HelpUrl
        [System.Xml.Serialization.XmlAttribute("helpurl")]
        public string HelpUrl
        {
            set { m_helpUrl = GetValidValue(value); }
            get { return m_helpUrl; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsHelpUrl
        {
            get { return (!HelpUrl.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHelpUrlNone
        {
            get { return (HelpUrl.Equals("none")); }
        }
        #endregion HelpUrl
        #region public HelpSchema
        [System.Xml.Serialization.XmlAttribute("helpschema")]
        public string HelpSchema
        {
            set { m_helpSchema = GetValidValue(value); }
            get { return m_helpSchema; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsHelpSchema
        {
            get { return (!HelpSchema.Equals("undefined")); }
        }
        #endregion
        #region public HelpElement
        [System.Xml.Serialization.XmlAttribute("helpelement")]
        public string HelpElement
        {
            set { m_helpElement = GetValidValue(value); }
            get { return m_helpElement; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsHelpElement
        {
            get { return (!HelpElement.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHelpElementNone
        {
            get { return (HelpElement.Equals("none")); }
        }
        #endregion
        //
        #region public KeepInQuick
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsInQuickInput
        {
            get { return ("yes" == KeepInQuickInput); }
        }
        [System.Xml.Serialization.XmlAttribute("keepinquickinput")]
        public string KeepInQuickInput
        {
            set
            {
                m_keepInQuickInput = GetValidValue(value, true);
                //
                if (BoolFunc.IsTrue(m_keepInQuickInput))
                    m_keepInQuickInput = "yes";
                else
                    m_keepInQuickInput = "no";

            }
            get { return m_keepInQuickInput; }
        }
        #endregion KeepInQuick
        //
        #region public Enabled
        [System.Xml.Serialization.XmlAttribute("enabled")]
        public string Enabled
        {
            set
            {
                m_enabled = GetValidValue(value, true);
                if ("undefined" == m_enabled)
                    m_enabled = "yes";
                else if (BoolFunc.IsTrue(m_enabled))
                    m_enabled = "yes";
                else
                    m_enabled = "no";
            }
            get { return m_enabled; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsEnabled
        {
            get { return (Enabled == "yes"); }
        }
        #endregion Enabled

        #region public Style
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsStyle
        {
            get { return (!this.Style.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("style")]
        public string Style
        {
            set { m_style = GetValidValue(value); }
            get { return m_style; }
        }
        #endregion style
        #region public Cssclass
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsCssClass
        {
            get { return (!this.CssClass.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("cssclass")]
        public string CssClass
        {
            set { m_cssClass = GetValidValue(value); }
            get { return m_cssClass; }
        }
        #endregion style
        #region public RegEx
        [System.Xml.Serialization.XmlAttribute("regex")]
        public string RegEx
        {
            set { m_regEx = GetValidValue(value); }
            get { return m_regEx; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsRegEx
        {
            get { return (!this.RegEx.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFSRegex.TypeRegex RegexValue
        {
            get
            {
                EFSRegex.TypeRegex typeRegex = EFSRegex.TypeRegex.None;
                if (System.Enum.IsDefined(typeof(EFSRegex.TypeRegex), this.RegEx))
                    typeRegex = (EFSRegex.TypeRegex)System.Enum.Parse(typeof(EFSRegex.TypeRegex), this.RegEx);
                return typeRegex;
            }
        }
        #endregion RegEx
        #region public Datatype
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TypeData.TypeDataEnum DataTypeEnum
        {
            get { return TypeData.GetTypeDataEnum(m_datatype, true); }
        }
        [System.Xml.Serialization.XmlAttribute("datatype")]
        public string DataType
        {
            get { return m_datatype; }
            set { m_datatype = GetValidValue(value, true); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsDataType
        {
            get { return (!this.DataType.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNumeric
        {
            get { return (TypeData.IsTypeNumeric(this.DataType)); }
        }
        #endregion Datatype
        #region public Width
        [System.Xml.Serialization.XmlAttribute("width")]
        public string Width
        {
            get { return m_width; }
            set { m_width = GetValidValue(value); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsWidth
        {
            get { return (!this.Width.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Unit WidthInStyle
        {
            get
            {
                Unit widthInStyle = Unit.Empty;
                bool isContainsWidthInStyle = ContainsWidth;
                if (false == isContainsWidthInStyle)
                {
                    if (ContainsStyle)
                    {
                        string[] splitStyle = m_style.Split(';');
                        foreach (string s in splitStyle)
                        {
                            string[] splitStyleKey = s.Split(':');
                            if (ArrFunc.IsFilled(splitStyleKey))
                            {
                                if (splitStyleKey[0].Trim().ToLower() == "width")
                                {
                                    if (-1 < splitStyleKey[1].IndexOf("%"))
                                        widthInStyle = Unit.Percentage(Convert.ToDouble(splitStyleKey[1].Replace("%", string.Empty)));
                                    else if (-1 < splitStyleKey[1].IndexOf("pt"))
                                        widthInStyle = Unit.Point(Convert.ToInt32(splitStyleKey[1].Replace("pt", string.Empty)));
                                    else if ((splitStyleKey[1] != "initial") && (splitStyleKey[1] != "inherit"))
                                        widthInStyle = Unit.Pixel(Convert.ToInt32(splitStyleKey[1].Replace("px", string.Empty)));
                                    break;
                                }
                            }
                        }

                    }
                }
                return widthInStyle;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Unit UnitWidth
        {
            get
            {
                bool isOk = false;
                Unit ret = Unit.Pixel(140);
                //
                if (this.ContainsWidth)
                {
                    try
                    {
                        if ((m_width != "initial") && (m_width != "inherit"))
                            ret = Unit.Parse(m_width);
                        else
                            ret = Unit.Empty;
                        isOk = true;
                    }
                    catch { isOk = false; };
                }
                else if (this.ContainsStyle)
                {
                    ret = WidthInStyle;
                    isOk = (Unit.Empty != ret);
                }
                //
                if (false == isOk)
                {
                    if (ControlEnum.quickinput == this.Control)
                        ret = Unit.Percentage(100);
                    else if (ControlEnum.htmlselect == this.Control)
                        ret = Unit.Empty;
                    else if (TypeData.IsTypeDate(this.DataType))
                        ret = Unit.Pixel(80);
                    else if (TypeData.IsTypeTime(this.DataType))
                        ret = Unit.Pixel(40);
                    else
                        ret = Unit.Pixel(140);
                }
                return ret;
            }
        }
        #endregion Width
        #region Height
        // RD 20100120 Add height property
        [System.Xml.Serialization.XmlAttribute("height")]
        public string Height
        {
            get { return m_height; }
            set { m_height = GetValidValue(value); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsHeight
        {
            get { return (!this.Height.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Unit HeightInStyle
        {
            get
            {
                Unit heightInStyle = Unit.Empty;
                bool isContainsHeightInStyle = ContainsHeight;
                if (false == isContainsHeightInStyle)
                {
                    if (ContainsStyle)
                    {
                        string[] splitStyle = m_style.Split(';');
                        foreach (string s in splitStyle)
                        {
                            string[] splitStyleKey = s.Split(':');
                            if (ArrFunc.IsFilled(splitStyleKey))
                            {
                                if (splitStyleKey[0].Trim().ToLower() == "height")
                                {
                                    if (-1 < splitStyleKey[1].IndexOf("%"))
                                        heightInStyle = Unit.Percentage(Convert.ToDouble(splitStyleKey[1].Replace("%", string.Empty)));
                                    else if (-1 < splitStyleKey[1].IndexOf("pt"))
                                        heightInStyle = Unit.Point(Convert.ToInt32(splitStyleKey[1].Replace("pt", string.Empty)));
                                    else
                                        heightInStyle = Unit.Pixel(Convert.ToInt32(splitStyleKey[1].Replace("px", string.Empty)));
                                    break;
                                }
                            }
                        }

                    }
                }
                return heightInStyle;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual Unit UnitHeight
        {
            get
            {
                Unit ret = Unit.Empty;
                //
                if (this.ContainsHeight)
                    ret = Unit.Parse(m_height);
                //                
                else if (this.ContainsStyle)
                    ret = HeightInStyle;
                //
                return ret;
            }
        }
        #endregion Height
        #region public Colspan
        [System.Xml.Serialization.XmlAttribute("colspan")]
        public string Colspan
        {
            set { m_colspan = GetValidValue(value); }
            get { return m_colspan; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsColspan
        {
            get { return (!this.Colspan.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int ColspanIntValue
        {
            get
            {
                int ret = 1;
                if (this.ContainsColspan)
                {
                    ret = IntFunc.IntValue2(this.Colspan);
                    if (-1 == ret)
                        ret = 999;
                }
                return ret;
            }
        }
        #endregion Colspan
        #region public Rowspan
        [System.Xml.Serialization.XmlAttribute("rowspan")]
        public string Rowspan
        {
            set { m_rowspan = GetValidValue(value); }
            get { return m_rowspan; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsRowspan
        {
            get { return (!this.Colspan.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int RowspanIntValue
        {
            get
            {
                int ret = 1;
                if (this.ContainsRowspan)
                {
                    ret = IntFunc.IntValue2(this.Rowspan);
                    if (-1 == ret)
                        ret = 999;
                }
                return ret;
            }
        }
        #endregion Rowspan
        
        #region public Caption
        [System.Xml.Serialization.XmlAttribute("caption")]
        public string Caption
        {
            set { m_caption = GetValidValue(value); }
            get { return m_caption; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsCaption
        {
            get { return (!this.Caption.Equals("undefined")); }
        }
        /// <summary>
        /// Obtient la Resource du contrôle
        /// </summary>
        /// FI 20140708 [20179] Modify
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Resource
        {
            get
            {
                //FI 20140708 [20179] Appel à la méthode
                return GetResource(System.Globalization.CultureInfo.CurrentCulture);
            }
        }
        #endregion caption

        #region public ToolTip
        [System.Xml.Serialization.XmlAttribute("tooltip")]
        public string ToolTip
        {
            set { m_toolTip = GetValidValue(value); }
            get { return m_toolTip; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsToolTip
        {
            get { return (!this.ToolTip.Equals("undefined")); }
        }
        #endregion ToolTip
        #region public ResourceToolTip
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ResourceToolTip
        {
            get
            {
                string ret = string.Empty;
                //
                if (this.ContainsToolTip)
                {
                    ret = Ressource.GetString(this.ToolTip, "ResError");
                    if (ret.Equals("ResError"))
                        ret = this.ToolTip;
                }
                return ret;
            }
        }
        #endregion ResourceToolTip
        
        #region public ToolTipTitle
        [System.Xml.Serialization.XmlAttribute("tooltiptitle")]
        public string ToolTipTitle
        {
            set { m_toolTipTitle = GetValidValue(value); }
            get { return m_toolTipTitle; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsToolTipTitle
        {
            get { return (!this.ToolTipTitle.Equals("undefined")); }
        }
        #endregion ToolTip
        #region public ResourceToolTipTitle
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ResourceToolTipTitle
        {
            get
            {
                string ret = string.Empty;
                //
                if (this.ContainsToolTipTitle)
                {
                    ret = Ressource.GetString(this.ToolTipTitle, "ResError");
                    //PL 20091230 Add ! et 
                    if (ret.Equals("ResError"))
                    {
                        //ret = this.ToolTipTitle;
                        ret = Ressource.GetString("Information_Title");
                    }
                }
                return ret;
            }
        }
        #endregion ResourceToolTipTitle
        /// <summary>
        /// 
        /// </summary>
        /// FI 20191227 [XXXXX] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsAutocomplete
        {
            get { return (!this.Autocomplete.Equals("undefined")); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20191227 [XXXXX] Add
        [System.Xml.Serialization.XmlAttribute("autocomplete")]
        public string Autocomplete
        {
            set
            {
                m_autocomplete = GetValidValue(value, true);
                if ("undefined" == m_autocomplete)
                    m_autocomplete = "no";
                else if (BoolFunc.IsTrue(m_autocomplete))
                    m_autocomplete = "yes";
                else
                    m_autocomplete = "no";
            }
            get { return m_autocomplete; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20191227 [XXXXX] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAutocomplete
        {
            get { return (Autocomplete == "yes"); }
        }

        // EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsHeadLinkInfo
        {
            get { return (!HeadLinkInfo.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("headlinkinfo")]
        // EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        public string HeadLinkInfo
        {
            set
            {
                if (StrFunc.IsFilled(value))
                {
                    m_headlinkinfo = string.Empty;
                }
                m_headlinkinfo = GetValidValue(value);
            }
            get { return m_headlinkinfo; }
        }

        /// <summary>
        /// Obtient true si le contrôle est un de type saisisable (exemple htmlselect, textbox, dropdown etc...)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20170918 [23342] add timestamp 
        public bool IsControlData
        {
            get
            {
                bool bret;
                bret =
                    (CustomObject.ControlEnum.htmlselect == m_control) ||
                    (CustomObject.ControlEnum.dropdown == m_control) ||
                    (CustomObject.ControlEnum.optgroupdropdown == m_control) ||
                    (CustomObject.ControlEnum.htmlcheckbox == m_control) ||
                    (CustomObject.ControlEnum.checkbox == m_control) ||
                    (CustomObject.ControlEnum.textbox == m_control) ||
                    (CustomObject.ControlEnum.timestamp == m_control) ||
                    (CustomObject.ControlEnum.quickinput == m_control) ||
                    (CustomObject.ControlEnum.ddlbanner == m_control) ||
                    (CustomObject.ControlEnum.hyperlink == m_control); //??Ne devrait pas être là??? Voir avec EG
                return bret;
            }
        }

        /// <summary>
        /// Obtient l'identifiant unique du contrôle
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CtrlClientId
        {
            get
            {
                //return   (GetCstCtrl(this.Control) + this.ClientId); 
                return GetCtrlClientId(this.Control);
            }
        }
        
        #region public Control
        /// <summary>
        /// Obtient ou définie le type de contrôle
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("control")]
        public ControlEnum Control
        {
            set { m_control = value; }
            get { return m_control; }
        }
        #endregion
        
        #region public Occurs
        [System.Xml.Serialization.XmlAttribute("occurs")]
        public string Occurs
        {
            set { m_occurs = GetValidValue(value); }
            get { return m_occurs; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsOccurs
        {
            get { return (!this.Occurs.Equals("undefined")); }
        }
        #endregion Occurs
        
        #region public Misc
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsMisc
        {
            get { return (!Misc.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("misc")]
        public string Misc
        {
            set
            {
                if (StrFunc.IsFilled(value))
                {
                    m_misc = string.Empty;
                }

                m_misc = GetValidValue(value);
            }
            get { return m_misc; }
        }
        #endregion Misc
        #region public Attributes
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsAttributes
        {
            get { return (!this.Attributes.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("attributes")]
        public string Attributes
        {
            set { m_attributes = GetValidValue(value); }
            get { return m_attributes; }
        }
        #endregion Attributes

        #region DefaultValue
        [System.Xml.Serialization.XmlAttribute("defaultvalue")]
        public string DefaultValue
        {
            set { m_defaultValue = GetValidValue(value); }
            get { return m_defaultValue; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsDefaultValue
        {
            get { return (!this.DefaultValue.Equals("undefined")); }
        }

        #endregion DefaultValue
        #region FixedCol
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsFixedCol
        {
            get { return (!this.FixedCol.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("fixedCol")]
        public string FixedCol
        {
            set { m_fixedCol = GetValidValue(value); }
            get { return m_fixedCol; }
        }
        #endregion FixedCol

        #region public Match
        /// <summary>
        /// Obtient true si le contrôle associé doit être pris en considération lorsque la saisie est en mode "Matching"
        /// </summary>
        /// FI 20140708 [20179] add 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsToMatch
        {
            //get { return GetAttributeValue(Match, "matchMode") == "match"; }
            get 
            {
                if (String.IsNullOrEmpty(Match) || (!this.ContainsMatch))
                {
                    return false;
                }
                else
                {
                    string[] split = Match.Split(';');
                    return BoolFunc.IsTrue(split[0]);                     
                }
            }
        }
        /// <summary>
        /// Obtient true si la propriété Match est renseignée
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsMatch
        {
            get { return (!Match.Equals("undefined")); }
        }
        /// <summary>
        /// Obtient ou Définit les propriétés du contrôle lorsque la saisie est en mode "Match"
        /// <para>modèle => true|false;default=match|unmatch|NA</para>
        /// <para>Ex1=> "true", Déclarer une zone de saisie comme étant à "matcher"</para>
        /// <para>Ex2 =>"false", Déclarer une zone de saisie comme n'étant pas à "matcher", (Equivalent à match non renseigné)</para>
        /// <para>Ex3 =>"true;default=unmatch", Déclarer une zone de saisie comme étant à "matcher", avec une valeur par défaut</para>
        /// </summary>
        /// FI 20140708 [20179] add  
        [System.Xml.Serialization.XmlAttribute("match")]
        public string Match
        {
            set { m_match = GetValidValue(value, false); }
            get { return m_match; }
        }
        /// <summary>
        /// Valeur du statut Match par défaut  
        /// <para>valuers possibles 'match' ou 'unmatch'</para>
        /// </summary>
        /// FI 20140708 [20179] add  
        [System.Xml.Serialization.XmlIgnore]
        public string MatchDefaultValue
        {
            //get { return GetAttributeValue(Match, "defaultValue"); }
            get { return GetAttributeValue(Match, "default"); }
        }
        #endregion Match
        #endregion property

        #region Constructor
        public CustomObject()
            : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }
        public CustomObject(string pClientId)
            : this(pClientId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }
        /// <summary>
        /// Contructeur 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pRequired"></param>
        /// <param name="pPostBack"></param>
        /// <param name="pHelpSchema"></param>
        /// <param name="pHelpElement"></param>
        /// <param name="pHelpUrl"></param>
        /// <param name="pkeepInQuick"></param>
        /// FI 20140708 [20179] Modify: Gestion de match 
        public CustomObject(string pClientId, string pRequired, string pPostBack, string pHelpSchema, string pHelpElement, string pHelpUrl, string pkeepInQuick)
        {
            m_control = ControlEnum.Unknown;
            ClientId = pClientId;
            AccessKey = string.Empty;
            PostBack = pPostBack;
            PostBackMethod = string.Empty;
            PostBackEvent = string.Empty;
            PostBackArgument = string.Empty;
            Required = pRequired;
            HelpSchema = pHelpSchema;
            HelpElement = pHelpElement;
            HelpUrl = pHelpUrl;
            KeepInQuickInput = pkeepInQuick;
            //
            Width = string.Empty;
            // RD 20100120 Add height property
            Height = string.Empty;
            RegEx = string.Empty;
            DataType = string.Empty;
            Style = string.Empty;
            CssClass = string.Empty;
            Colspan = string.Empty;
            Caption = string.Empty;
            ToolTip = string.Empty;
            ToolTipTitle = string.Empty;
            Misc = string.Empty;
            Attributes = string.Empty;
            Occurs = string.Empty;
            Enabled = string.Empty;
            DefaultValue = string.Empty;
            FixedCol = string.Empty;
            //		
            Locked = "undefined";
            Match = string.Empty;
            // FI 20211224 [XXXXX] initialisation à Empty
            Autocomplete = string.Empty;
            HeadLinkInfo = string.Empty;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pModeLocked"></param>
        /// <returns></returns>
        public DictionaryEntry GetLocked(string pModeLocked)
        {
            DictionaryEntry item = new DictionaryEntry();
            if (IsLockedSpecified)
            {
                foreach (DictionaryEntry de in m_locked)
                {
                    if (pModeLocked == de.Key.ToString())
                    {
                        item = de;
                        break;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pModeLocked"></param>
        /// <returns></returns>
        public bool IsLocked(string pModeLocked)
        {
            bool isLocked = false;
            DictionaryEntry item = GetLocked(pModeLocked);
            if (null != item.Value)
                isLocked = (bool)item.Value;
            return isLocked;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pModeLocked"></param>
        /// <param name="pValue"></param>
        public void SetLocked(string pModeLocked, bool pValue)
        {
            DictionaryEntry item = GetLocked(pModeLocked);
            if (null == item.Value)
            {
                ArrayList aDictionaryEntry = new ArrayList();
                if (IsLockedSpecified)
                    aDictionaryEntry.AddRange(m_locked);
                aDictionaryEntry.Add(new DictionaryEntry(pModeLocked, pValue));

                //item = new DictionaryEntry(pModeLocked, pValue);
                m_locked = (DictionaryEntry[])aDictionaryEntry.ToArray(typeof(DictionaryEntry));
            }
            else
                item.Value = pValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcontrol"></param>
        /// <returns></returns>
        public string GetCtrlClientId(CustomObject.ControlEnum pcontrol)
        {
            return (GetCstCtrl(pcontrol) + this.ClientId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMiscKey"></param>
        /// <param name="pMiscDefaultValue"></param>
        /// <returns></returns>
        public bool GetMiscValue(string pMiscKey, bool pMiscDefaultValue)
        {
            string miscValue = GetMiscValue(pMiscKey);
            if (StrFunc.IsEmpty(miscValue))
                return pMiscDefaultValue;
            else
                return BoolFunc.IsTrue(miscValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMiscKey"></param>
        /// <param name="pMiscDefaultValue"></param>
        /// <returns></returns>
        public string GetMiscValue(string pMiscKey, string pMiscDefaultValue)
        {
            string miscValue = GetMiscValue(pMiscKey);
            if (StrFunc.IsEmpty(miscValue))
                return pMiscDefaultValue;
            else
                return miscValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMiscKey"></param>
        /// <returns></returns>
        public string GetMiscValue(string pMiscKey)
        {
            string miscValue = string.Empty;
            if (ContainsMisc)
                miscValue = GetAttributeValue(m_misc, pMiscKey);
            return miscValue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
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
            string ret;
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


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPostBackMethod()
        {
            if (ContainsPostBackMethod)
                return PostBackMethod;
            else
                return DefaultPostBackMethod;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPostBackEvent()
        {
            if (ContainsPostBackEvent)
                return PostBackEvent;
            else
                return DefaultPostBackEvent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControlModeConsult"></param>
        /// <returns></returns>
        public virtual TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            TableCell ret = null;
            //
            switch (m_control)
            {
                case ControlEnum.checkbox:
                    ret = WriteCellCheckBox(pPage, pIsControlModeConsult);
                    break;
                case ControlEnum.htmlcheckbox:
                    ret = WriteHtmlInputCheckBox(pPage, pIsControlModeConsult);
                    break;
                case ControlEnum.cellheader:
                case ControlEnum.cellfooter:
                    ret = WriteCellHeaderFooter(pPage);
                    break;
                case ControlEnum.fill:
                case ControlEnum.space:
                    if (m_control == ControlEnum.fill)
                        Width = "100%";
                    ret = WriteSpace();
                    break;
                case ControlEnum.vline:
                    ret = ControlsTools.WebTdSeparator();
                    break;
                case ControlEnum.hr:
                    ret = WriteHR();
                    break;
            }
         
            if (null != ret)
            {
                if (ContainsColspan)
                    ret.ColumnSpan = ColspanIntValue;

                if (ContainsRowspan)
                    ret.RowSpan = RowspanIntValue;

                SetFixedCol(ret);
            }
            return ret;

        }


        /// <summary>
        ///  Génère un TD contenant un DIV, ce dernier contenant un HR 
        /// </summary>
        /// <returns></returns>
        /// FI 20180424 [23871] Refactoring 
        /// => possibilité de définir un style sur le Div
        /// => possibilité de définir une hauteur de cellule
        private TableCell WriteHR()
        {

            LiteralControl hr = new LiteralControl("<hr/>");

            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "hr");
            if (ContainsStyle)
                ControlsTools.SetStyleList(div.Style, Style);
            else 
                div.Style.Add(HtmlTextWriterStyle.Height, "1px");
            div.Controls.Add(hr);

            TableCell cell = new TableCell();
            if (ContainsColspan)
                cell.ColumnSpan = ColspanIntValue;
            if (ContainsHeight)
                cell.Height = UnitHeight;
            cell.Width = Unit.Percentage(100);
            cell.Style.Add(HtmlTextWriterStyle.Padding, "5px");
            cell.Controls.Add(div);

            return cell;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCtrl"></param>
        /// <returns></returns>
        /// EG 20170918 [23342] add timestamp 
        private string GetCstCtrl(ControlEnum pCtrl)
        {
            string ret = string.Empty;
            //
            switch (pCtrl)
            {
                case ControlEnum.banner:
                case ControlEnum.openbanner:
                case ControlEnum.br:
                case ControlEnum.fill:
                case ControlEnum.hr:
                case ControlEnum.hline:
                case ControlEnum.space:
                case ControlEnum.vline:
                    break;
                case ControlEnum.hyperlink:
                    ret = Cst.LNK;
                    break;
                case ControlEnum.button:
                    ret = Cst.BUT;
                    break;
                case ControlEnum.checkbox:
                    ret = Cst.CHK;
                    break;
                case ControlEnum.htmlcheckbox:
                    ret = Cst.HCK;
                    break;
                case ControlEnum.htmlselect:
                case ControlEnum.dropdown:
                case ControlEnum.optgroupdropdown:
                case ControlEnum.ddlbanner:
                    ret = Cst.DDL;
                    break;
                //case ControlEnum.htmlselect :	 	
                //    ret = Cst.HSL;
                //    break;
                case ControlEnum.display:
                    ret = Cst.DSP;
                    break;
                case ControlEnum.cellheader:
                    ret = Cst.CHEAD;
                    break;
                case ControlEnum.cellfooter:
                    ret = Cst.CFOOT;
                    break;
                case ControlEnum.hiddenbox:
                    ret = Cst.HDN;
                    break;

                case ControlEnum.image:
                    ret = Cst.IMG;
                    break;
                case ControlEnum.label:
                    ret = Cst.LBL;
                    break;
                case ControlEnum.textbox:
                    ret = Cst.TXT;
                    break;
                case ControlEnum.timestamp:
                    ret = Cst.TMS;
                    break;
                case ControlEnum.quickinput:
                    ret = Cst.QKI;
                    break;
                case ControlEnum.panel:
                    ret = Cst.PNL;
                    break;
            }
            return ret;
        }


        /// <summary>
        /// Write CheckBox webcontrol to HTML.
        /// </summary>
        /// <newpara>Description : Write CheckBox webcontrol to HTML.</newpara>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <returns>Returns nothing.</returns>
        ///FI 20140708 [20179] Modify : Gestion de l'attribut match
        private TableCell WriteCellCheckBox(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteCheckBox() " + clientId);
            //
            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false,
                Width = UnitWidth
            };
            // RD 20100120 Add height property
            if (ContainsHeight)
                cell.Height = UnitHeight;
            #endregion cell
            //            
            #region CheckBox
            //FI 20091106 [16722] use WCHtmlCheckBox2
            WCCheckBox2 checkBox = new WCCheckBox2
            {
                IsReadOnly = isReadOnly,
                ID = clientId,
                CssClass = EFSCssClass.CaptureCheck,
                Text = Resource,
                Enabled = IsEnabled
            };
            if (isReadOnly)
                checkBox.TabIndex = -1;
            // EG 20160404 Migration vs2013
            //#warning RD 20091230 Il faudrait faire évoluer dans une prochaine version le code pour pouvoir spécifier le TabIndex sur le descriptif XML de l'écran
            // RD 201001040 [16809] Pour sortir les CheckBox du crcuit tabulaire
            if (clientId.EndsWith("ISNCMINI") || clientId.EndsWith("ISNCMINT") || clientId.EndsWith("ISNCMFIN"))
                checkBox.TabIndex = -1;
            //
            // FI 20140708 [20179] pas de PostBack, pas de SetNoAutoPostbackCtrlChanged  si le contrôle est isReadOnly 
            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;
                    // FI 20200813 [XXXXX] Il n'y a pas de validator sur un contrôle CheckBox. L'appel à Page_ClientValidate si sur la page il n'existe aucun contôle avec validator
                    //checkBox.Attributes.Add(GetPostBackEvent(), "if (Page_ClientValidate()) " + GetPostBackMethod() + "('" + checkBox.UniqueID + "','" + arg + "');");
                    // FI 20210325 [XXXXX] setTimeout pour meilleur gestion des focus (en relation avec activeElement) 
                    //checkBox.Attributes.Add(GetPostBackEvent(), GetPostBackMethod() + "('" + checkBox.UniqueID + "','" + arg + "');");
                    checkBox.Attributes.Add(GetPostBackEvent(), $"setTimeout('{GetPostBackMethod()}(\"{checkBox.UniqueID}\",\"{arg}\")',0);");
                }
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                checkBox.Attributes.Add("onClick", "__doPostBack('" + checkBox.UniqueID + "','match');");
                SetLookMatchDefaultValue(checkBox);
            }

            if (ContainsToolTip)
                checkBox.ToolTip = ResourceToolTip;

            if (ContainsStyle)
                ControlsTools.SetStyleList(checkBox.Style, Style);

            CustomObjectTools.SetVisibilityStyle(checkBox, this, pIsControlModeConsult);

            if (ContainsWidth)
                checkBox.Width = UnitWidth;

            // RD 20100120 Add height property
            if (ContainsHeight)
                checkBox.Height = UnitHeight;

            if (pPage.IsDebugClientId)
                checkBox.ToolTip = checkBox.ID;

            if (ContainsDefaultValue)
                checkBox.Checked = BoolFunc.IsTrue(DefaultValue);

            #endregion CheckBox

            cell.Controls.Add(checkBox);
            SetFixedCol(cell);
            return cell;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <returns></returns>
        private TableCell WriteCellHeaderFooter(PageBase pPage)
        {

            string clientId = CtrlClientId;
            //
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteCellHeaderFooter() " + clientId);
            //
            #region Cell
            TableCell cell = new TableCell
            {
                ID = clientId,
                HorizontalAlign = HorizontalAlign.Left,
                Visible = true,
                Wrap = false
            };
            //
            Label lbl = new Label
            {
                Text = Resource
            };
            if (ContainsCssClass)
                lbl.CssClass = CssClass;
            cell.Controls.Add(lbl);
            if (ContainsWidth)
                cell.Width = UnitWidth;
            // RD 20100120 Add height property
            if (ContainsHeight)
                cell.Height = UnitHeight;
            if (ContainsCssClass)
                cell.CssClass = CssClass;
            if (ContainsStyle)
                ControlsTools.SetStyleList(cell.Style, Style);
            //ToolTip
            if (pPage.IsDebugClientId)
                cell.ToolTip = cell.ClientID;
            else if (ContainsToolTip)
                cell.ToolTip = ResourceToolTip;
            //Help
            string helpUrl = string.Empty;
            if (ContainsHelpElement && ContainsHelpSchema && (!IsHelpElementNone) && pPage.ContainsHelpUrlPath)
                helpUrl = pPage.GetCompleteHelpUrl(HelpSchema, HelpElement);
            else if (ContainsHelpUrl && (!IsHelpUrlNone) && pPage.ContainsHelpUrlPath)
                helpUrl = pPage.GetCompleteHelpUrl(HelpUrl);
            //
            if (StrFunc.IsFilled(helpUrl))
            {
                cell.Style.Add(HtmlTextWriterStyle.Cursor, "help");
                pPage.SetQtipHelpOnLine(cell, cell.ToolTip, helpUrl);
            }
            //
            SetFixedCol(cell);
            #endregion Cell
            //
            return cell;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControlModeConsult"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] Modify
        /// Attention les HtmlInputCheckBox ne sont pas gérés en mode match (il n'existe pas de propriété BackColor)
        private TableCell WriteHtmlInputCheckBox(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("HtmlInputCheckBox() " + clientId);

            bool isWithLabel = GetMiscValue("label", false);

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false
            };
            if (isWithLabel)
                cell.Width = UnitWidth;
            else if (ContainsWidth)
                cell.Width = UnitWidth;
            #endregion cell

            #region CheckBox
            int lblWidth = 0;
            if ((isWithLabel) && (!UnitWidth.IsEmpty))
                lblWidth = Convert.ToInt32(UnitWidth.Value) - 40;
            //
            //FI 20091106 [16722] use WCHtmlCheckBox2
            WCHtmlCheckBox2 checkBox = new WCHtmlCheckBox2(isWithLabel, lblWidth)
            {
                ID = clientId,
                IsReadOnly = isReadOnly,
                CssClass = EFSCssClass.CaptureCheck,
                Text = Resource,
                Enabled = IsEnabled
            };
            // EG 20160404 Migration vs2013
            //#warning RD 20100104 Il faudrait faire évoluer dans une prochaine version le code pour pouvoir spécifier le TabIndex sur le descriptif XML de l'écran
            // RD 20091230 [16809] Pour sortir les CheckBox du crcuit tabulaire
            if (clientId.EndsWith("ISNCMINI") || clientId.EndsWith("ISNCMINT") || clientId.EndsWith("ISNCMFIN"))
                checkBox.Attributes.Add("tabIndex", "-1");

            if (ContainsStyle)
                ControlsTools.SetStyleList(checkBox.Style, Style);

            CustomObjectTools.SetVisibilityStyle(checkBox, this, pIsControlModeConsult);

            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;

                    // FI 20210325 [XXXXX] setTimeout pour meilleur gestion des focus (en relation avec activeElement) 
                    //checkBox.Attributes.Add(GetPostBackEvent(), GetPostBackMethod() + "('" + checkBox.UniqueID + "','" + arg + "');");
                    checkBox.Attributes.Add(GetPostBackEvent(), $"setTimeout('{GetPostBackMethod()}(\"{checkBox.UniqueID}\",\"{arg}\")',0);");
                }
            }

            if (ContainsToolTip)
                checkBox.ToolTip = ResourceToolTip;

            if (pPage.IsDebugClientId)
                checkBox.ToolTip = checkBox.ID;
            #endregion CheckBox

            cell.Controls.Add(checkBox);

            SetFixedCol(cell);

            return cell;
        }

        private const string SMALL_SPACE = "3", MEDIUM_SPACE = "5";

        /// <summary>
        /// Retourne une cellule avec la valeur Cst.HTMLSpace
        /// </summary>
        /// <returns></returns>
        private TableCell WriteSpace()
        {
            string width = null;
            string colspan = null;
            //
            if (ContainsColspan)
                colspan = Colspan;
            if (ContainsWidth)
                width = Width;
            //
            if ((null != colspan) && (null != width))
                return WriteSpace(width, ColspanIntValue);
            else if ((null == colspan) && (null != width))
                return WriteSpace(width);
            else if ((null != colspan) && (null == width))
                return WriteSpace(ColspanIntValue);
            else
                return WriteSpace(MEDIUM_SPACE);

        }
        /// <summary>
        /// Retourne une cellule avec la valeur Cst.HTMLSpace
        /// </summary>
        /// <param name="pColspan"></param>
        /// <returns></returns>
        private TableCell WriteSpace(int pColspan)
        {
            return WriteSpace(MEDIUM_SPACE, pColspan);
        }

        /// <summary>
        /// Retourne une cellule avec la valeur Cst.HTMLSpace
        /// </summary>
        /// <param name="pWidth"></param>
        /// <returns></returns>
        private TableCell WriteSpace(string pWidth)
        {
            return WriteSpace(pWidth, 1);
        }

        /// <summary>
        /// Retourne une cellule avec la valeur Cst.HTMLSpace
        /// </summary>
        /// <param name="pWidth"></param>
        /// <param name="pColspan"></param>
        /// <returns></returns>
        private TableCell WriteSpace(string pWidth, int pColspan)
        {
            TableCell cell = new TableCell();
            //
            if (StrFunc.IsEmpty(pWidth))
                cell.Width = Unit.Pixel(Convert.ToInt32(SMALL_SPACE));
            else
            {
                try { cell.Width = Unit.Parse(pWidth); }
                catch { cell.Width = Unit.Parse(SMALL_SPACE); }
            }
            if (pColspan > 1)
                cell.ColumnSpan = pColspan;
            //	
            cell.Controls.Add(new LiteralControl(Cst.HTMLSpace));
            SetFixedCol(cell);
            return cell;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCell"></param>
        protected void SetFixedCol(TableCell pCell)
        {
            string fixedCol = GetMiscValue("FixedCol");
            if (StrFunc.IsFilled(fixedCol))
                ControlsTools.SetFixedCol(pCell, fixedCol);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        private static string GetRessourceData(string pClientId)
        {
            string ret = string.Empty;
            string[] list = pClientId.Split(KEY_SEPARATOR);

            foreach (string s in list)
                ret = ret + StrFunc.PutOffSuffixNumeric(s) + KEY_SEPARATOR.ToString();

            ret = ret.Substring(0, ret.Length - KEY_SEPARATOR.ToString().Length);

            return ret;
        }

        /// <summary>
        /// Retourne la valeur associé à {pkey} lorsque pdata est formaté comme suit
        /// "key1:value1;key2:value2;key3:value3"
        /// <para></para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        protected static string GetAttributeValue(string pData, string pKey)
        {
            string ret = null;
            string[] split = pData.Split(';');
            foreach (string s in split)
            {
                string[] splitKey = s.Split(':');
                if (ArrFunc.IsFilled(splitKey))
                {
                    if (splitKey[0].Trim().ToUpper() == pKey.ToUpper())
                    {
                        ret = splitKey[1].Trim();
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Applique un look spécifique au contôle {pControl} en fonction de la valeur par défaut MatchDefaultValue 
        /// </summary>
        /// <param name="pControl"></param>
        protected void SetLookMatchDefaultValue(WebControl pControl)
        {
            Nullable<Cst.MatchEnum> statut = null;
            if (StrFunc.IsFilled(MatchDefaultValue) && (Enum.IsDefined(typeof(Cst.MatchEnum), MatchDefaultValue)))
            {
                statut = (Cst.MatchEnum)Enum.Parse(typeof(Cst.MatchEnum), MatchDefaultValue, true);
            }
            ControlsTools.SetLookMatch(pControl, statut);
        }

        /// <summary>
        /// Retourne la resource dans la culture {pCultureInfo}
        /// </summary>
        /// <param name="pCultureInfo"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] add Method
        public string GetResource(System.Globalization.CultureInfo pCultureInfo)
        {
            string ret;
            if (ContainsCaption)
            {
                //PL 20140610 Newness isRemoveBreakRow
                bool isRemoveBreakRow = Caption.EndsWith("!br");
                if (isRemoveBreakRow)
                    Caption = Caption.Replace("!br", string.Empty);

                ret = Ressource.GetMulti(Caption, 1, "ResError");
                if (ret.Equals("ResError"))
                    ret = Caption;
                else if (isRemoveBreakRow)
                    ret = ret.Replace(Cst.HTMLBreakLine, Cst.HTMLSpace).Replace(Cst.HTMLBreakLine2, Cst.HTMLSpace);
            }
            else
            {
                //20100128 Recherche multiples dans le fichier ressources (voir Exemple)
                //Exemple : clientId = fra_notional_amount
                //Alors Spheres recherche une ressource pour =>fra_notional_amount
                //Si elle n'existe pas alors recherche d'une ressource pour =>notional_amount
                //Si elle n'existe pas alors recherche d'une ressource pour =>amount
                string key = GetRessourceData(ClientId);
                ret = Ressource.GetString(key, "ResError", pCultureInfo);
                bool isOk = (ret != "ResError");
                //
                if (false == isOk)
                {
                    string[] s = key.Split(KEY_SEPARATOR);
                    int i = 0;
                    while (false == isOk)
                    {
                        i++;
                        if (i == ArrFunc.Count(s))
                            break;
                        
                        key = string.Empty;
                        for (int j = i; j < ArrFunc.Count(s); j++)
                            key += s[j] + KEY_SEPARATOR.ToString();
                        key = key.Substring(0, key.Length - KEY_SEPARATOR.ToString().Length);
                        
                        ret = Ressource.GetString(key, "ResError", pCultureInfo);
                        isOk = (ret != "ResError");
                    }
                }

                if (false == isOk)
                    ret = ClientId;
            }

            return ret;
        }


        #endregion Methods
    
    }


    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customObjectLabel", IsNullable = false)]
    public partial class CustomObjectLabel : CustomObject
    {
        #region Membres
        private string m_SpaceBefore;
        private string m_SpaceAfter;
        #endregion Membres

        #region Constructor
        public CustomObjectLabel()
        {
            this.SpaceBefore = string.Empty;
            this.SpaceAfter = string.Empty;
        }
        #endregion

        #region Accessors
        #region SpaceBefore
        public string SpaceBefore
        {
            set { m_SpaceBefore = GetValidValue(value, false); }
            get { return m_SpaceBefore; }
        }
        public bool ContainsSpaceBefore
        {
            get { return (!this.SpaceBefore.Equals("undefined")); }
        }
        #endregion SpaceBefore
        #region SpaceAfter
        public string SpaceAfter
        {
            set { m_SpaceAfter = GetValidValue(value, false); }
            get { return m_SpaceAfter; }
        }
        public bool ContainsSpaceAfter
        {
            get { return (!this.SpaceAfter.Equals("undefined")); }
        }
        #endregion SpaceAfter

        #endregion Accessors

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControlModeConsult"></param>
        /// <returns></returns>
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {
            return WriteCellLabel(pPage, pIsControlModeConsult);
        }

        /// <summary>
        /// Write Label webcontrol to HTML.
        /// </summary>
        /// <newpara>Description : Write Label webcontrol to HTML.</newpara>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <returns>Returns nothing.</returns>
        /// 
        private TableCell WriteCellLabel(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            //
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteLabel() " + clientId);
            //
            #region Cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Visible = true,
                Wrap = false
            };
            if (ContainsWidth)
                cell.Width = UnitWidth;
            // RD 20100120 Add height property
            if (ContainsHeight)
                cell.Height = UnitHeight;
            #endregion

            #region label
            Label lbl = new Label
            {
                ID = clientId,
                Text = Resource
            };
            if (ContainsCssClass)
                lbl.CssClass = CssClass;
            else
                // FI 20191128 [XXXXX] Ajout de LabelDisplayCustomObject
                lbl.CssClass = ((ControlEnum.display == this.Control) ?
                    StrFunc.AppendFormat("{0} {1}", EFSCssClass.LabelDisplay, EFSCssClass.LabelDisplayCustomObject) : EFSCssClass.LabelCapture);
            //
            lbl.Enabled = IsEnabled;
            //
            if (ContainsStyle)
                ControlsTools.SetStyleList(lbl.Style, Style);
            //
            CustomObjectTools.SetVisibilityStyle(lbl, this, pIsControlModeConsult);
            //
            //ToolTip
            if (pPage.IsDebugClientId)
                lbl.ToolTip = lbl.ClientID;
            else if (ContainsToolTip)
                lbl.ToolTip = ResourceToolTip;
            //
            //Help
            string helpUrl = string.Empty;
            if (ContainsHelpElement && ContainsHelpSchema && (!IsHelpElementNone) && pPage.ContainsHelpUrlPath)
                helpUrl = pPage.GetCompleteHelpUrl(HelpSchema, HelpElement);
            else if (ContainsHelpUrl && (!IsHelpUrlNone) && pPage.ContainsHelpUrlPath)
                helpUrl = pPage.GetCompleteHelpUrl(HelpUrl);
            //
            if (StrFunc.IsFilled(helpUrl))
            {
                lbl.Style.Add(HtmlTextWriterStyle.Cursor, "help");
                pPage.SetQtipHelpOnLine(lbl, Resource, helpUrl);
            }
            #endregion label
            //
            // Add Space before
            if (this.ContainsSpaceBefore && BoolFunc.IsTrue(this.SpaceBefore))
            {
                TableTools.WriteSpaceInCell(cell);
            }
            else if (false == this.ContainsSpaceBefore) // traitement par défaut
            {
                if (false == (ControlEnum.display == this.Control))
                    TableTools.WriteSpaceInCell(cell);
            }
            else
            {
                //Aucun espcace
                //Nickel
            }
            //
            //Add Label
            cell.Controls.Add(lbl);
            //
            // Add Space After
            if (this.ContainsSpaceAfter && BoolFunc.IsTrue(this.SpaceAfter))
            {
                TableTools.WriteSpaceInCell(cell);
            }
            else if (false == this.ContainsSpaceAfter) // traitement par défaut
            {
                TableTools.WriteSpaceInCell(cell);
            }
            else
            {
                //Aucun espcace
                //Nickel
            }
            //
            SetFixedCol(cell);
            return cell;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customObjectDisplay", IsNullable = false)]
    public partial class CustomObjectDisplay : CustomObjectLabel
    {
        public CustomObjectDisplay() : base() { }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customObjectTextBox", IsNullable = false)]
    public partial class CustomObjectTextBox : CustomObject
    {
        #region Membres
        private string m_TextMode;
        private string m_Rows;
        #endregion Membres
        //
        #region Accessors
        #region TextMode
        public string TextMode
        {
            set { m_TextMode = GetValidValue(value, false); }
            get { return m_TextMode; }
        }
        public bool ContainsTextMode
        {
            get { return (!this.TextMode.Equals("undefined")); }
        }
        #endregion TextMode
        #region Rows
        public string Rows
        {
            set
            {
                if (IntFunc.IsPositiveInteger(value))
                {
                    m_Rows = value;
                }
                else
                {
                    m_Rows = "2"; //Default value
                }
            }
            get { return m_Rows; }
        }
        #endregion TextMode
        #endregion Accessors
        //
        #region Constructor
        public CustomObjectTextBox()
            : base()
        {
            TextMode = string.Empty;
        }
        #endregion Constructor
        //
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override string DefaultPostBackEvent
        {
            get { return "onblur"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControlModeConsult"></param>
        /// <returns></returns>
        ///FI 20140708 [20179] Modify : Gestion de l'attribut match
        /// EG 20201117 [25556] Corrections diverses - CSS
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            string dataType = DataType;
            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteTextBox() " + clientId);

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false,
            };
            if (ContainsHeight)
                cell.Height = UnitHeight;
            SetFixedCol(cell);
            #endregion cell

            #region TextBox

            #region Validator
            Validator[] validators = new Validator[4];
            // 20091110 EG
            if (false == isReadOnly)
            {
                #region RequireField
                Validator validatorRequireField = null;
                if (IsMandatory)
                    validatorRequireField = new Validator(Resource + Cst.Space + "[" + Ressource.GetString("MissingData") + "]", false, true);
                validators[0] = validatorRequireField;
                #endregion RequireField

                #region Regular Expression
                bool isExistRegEx = (ContainsRegEx);
                if (isExistRegEx && (EFSRegex.TypeRegex.None != RegexValue))
                    validators[1] = new Validator(RegexValue, Resource + Cst.Space + "[" + Ressource.GetString("InvalidData") + "]", false, true);
                #endregion Regular Expression

                #region ValidationDataType
                Validator validatorDataType = null;
                if ((!isExistRegEx) && GetMiscValue("isSetValidatorType", true))
                {
                    ValidationDataType validationType = ValidationDataType.String;
                    //
                    if (TypeData.IsTypeDate(dataType))
                        validationType = ValidationDataType.Date;
                    else if (TypeData.IsTypeDec(dataType))
                        validationType = ValidationDataType.Double;
                    else if (TypeData.IsTypeInt(dataType))
                        validationType = ValidationDataType.Integer;
                    else if (TypeData.IsTypeString(dataType))
                        validationType = ValidationDataType.String;
                    //
                    validatorDataType = new Validator(validationType, Resource + Cst.Space + "[" + Ressource.GetString("InvalidData") + "]", false, true);
                }
                validators[2] = validatorDataType;
                #endregion ValidationDataType

                #region CustomValidator
                Validator validatorCustom = null;
                if (TypeData.IsTypeDate(dataType))
                {
                    validatorCustom = Validator.GetValidatorDateRange(Resource + Cst.Space + "[" + Ressource.GetString("Msg_InvalidDate") + "]", Resource);
                    validatorCustom.IsSummaryOnly = true;
                    validatorCustom.IsShowMessage = false;
                }
                validators[3] = validatorCustom;
                #endregion CustomValidator
            }
            #endregion Validator

            string cssClass = EFSCssClass.Capture;
            if (ContainsCssClass)
                cssClass = CssClass;
            
            WCTextBox2 textBox = new WCTextBox2(clientId, isReadOnly,
                IsMandatory, IsNumeric, cssClass, validators);
            
            // 20100901 EG Add className DtPicker for Date JQuery UI DatePicker (type Date)
            // 20100901 EG Add className DtTimePicker for Date JQuery UI DatePicker (type DateTime)
            // 20100901 EG Add className TimePicker for Date JQuery UI TimePicker (type Time)
            if (false == isReadOnly)
            {
                if (TypeData.IsTypeDate(dataType))
                    textBox.CssClass = "DtPicker " + textBox.CssClass;
                else if (TypeData.IsTypeDateTime(dataType))
                    textBox.CssClass = "DtTimePicker " + textBox.CssClass;
                else if (TypeData.IsTypeTime(dataType))
                    textBox.CssClass = "TimePicker " + textBox.CssClass;
            }

            // FI 20191227 [XXXXX] Add co-autocomplete class
            // FI 20200107 [XXXXX] Mod or-autocomplete class
            if ((false == isReadOnly) && (IsAutocomplete))
                textBox.CssClass = "or-autocomplete " + textBox.CssClass; // or pour OpenReferential

            if ((false == isReadOnly) && ContainsHeadLinkInfo)
                textBox.Attributes.Add("onchange", "SetLinkIdToHeaderToggle(this,'" + HeadLinkInfo + "');return false;");

            if (this.ContainsAccessKey)
            {
                if (AccessKey == "NextBlock")
                    textBox.AccessKey = SystemSettings.GetAppSettings("Spheres_NextBlock_AccessKey", "N");
                else
                    textBox.AccessKey = AccessKey;
            }
            textBox.Enabled = IsEnabled;

            if (isReadOnly)
                textBox.TabIndex = -1;

            if (ContainsTextMode)
            {
                textBox.TextMode = (TextBoxMode)System.Enum.Parse(typeof(TextBoxMode), TextMode);
                textBox.Rows = Convert.ToInt32(Rows);
                // EG 20091110 
                textBox.CssClass = EFSCssClass.GetCssClass(IsNumeric, IsMandatory, textBox.TextMode == TextBoxMode.MultiLine, isReadOnly, cssClass);
            }
            //20061124 PL ASP.Net 2.0 
            //	if (SystemTools.IsFramework1())
            // EG Firefox/IE8
            //textBox.Width = Unit.Percentage(100);
            // RD 20100119 Problème sur la non prise en compte de la taille parametrée 
            //if (false == cell.Width.IsEmpty)
            //    textBox.Width = cell.Width;
            Unit width = UnitWidth;
            if (false == width.IsEmpty)
                textBox.Width = width;

            // RD 20100120 Add height property
            if (false == cell.Height.IsEmpty)
                textBox.Height = cell.Height;

            if (ContainsToolTip)
                textBox.ToolTip = ResourceToolTip;

            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;

                    string methodjsArg = StrFunc.AppendFormat("'{0}','{1}'", textBox.UniqueID, arg);
                    // FI 20200102 [XXXXX] post de la page si touche entrée actionnée
                    // FI 20200401 [XXXXX] supp post de la page si touche entrée actionnée
                    /*if (IsAutocomplete)
                        textBox.Attributes.Add("onkeydown", "if (Page_ClientValidate()){PostBackOnKeyEnter(event," + methodjsArg + ")};");
                    else*/
                    // FI 20210325 [XXXXX] setTimeout pour meilleur gestion des focus (en relation avec activeElement) 
                    //textBox.Attributes.Add(GetPostBackEvent(), "if (Page_ClientValidate()){" + GetPostBackMethod() + "(" + methodjsArg + ")};");
                    textBox.Attributes.Add(GetPostBackEvent(), "setTimeout(function() {if (Page_ClientValidate()){" + GetPostBackMethod() + "(" + methodjsArg + ")};},0);");
                }
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                textBox.Attributes.Add("onClick", "__doPostBack('" + textBox.UniqueID + "','match');");
                SetLookMatchDefaultValue(textBox);
            }
            // EG 20201117 Suppression style width en double
            if (ContainsStyle)
            {
                ControlsTools.SetStyleList(textBox.Style, Style);
                if (Unit.Empty != WidthInStyle)
                    textBox.Style.Remove("width");
            }

            if (ContainsAttributes)
                ControlsTools.SetAttributesList(textBox.Attributes, Attributes);

            #endregion TextBox

            cell.Controls.Add(textBox);

            if (ContainsColspan)
                cell.ColumnSpan = ColspanIntValue;

            if (ContainsRowspan)
                cell.RowSpan = RowspanIntValue;

            SetFixedCol(cell);

            return cell;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customObjectTimestamp", IsNullable = false)]
    /// EG 20170918 [23342] New
    /// EG 20170926 [22374] Add ToolTipZone
    public partial class CustomObjectTimestamp : CustomObject
    {
        #region Membres
        private string m_AltTime;
        private string m_TooltipZone;
        private string m_FreeZone;
        #endregion Membres

        #region Accessors
        #region FreeZone
        [System.Xml.Serialization.XmlAttribute("freezone")]
        public string FreeZone
        {
            set
            {
                m_FreeZone = GetValidValue(value, true);
                if ("undefined" == m_FreeZone)
                    m_FreeZone = "no";
                else if (BoolFunc.IsTrue(m_FreeZone))
                    m_FreeZone = "yes";
                else
                    m_FreeZone = "no";
            }
            get { return m_FreeZone; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFreeZone
        {
            get { return (FreeZone == "yes"); }
        }
        #endregion FreeZone
        #region AltTime
        [System.Xml.Serialization.XmlAttribute("alttime")]
        public string AltTime
        {
            set
            {
                m_AltTime = GetValidValue(value, true);
                if ("undefined" == m_AltTime)
                    m_AltTime = "no";
                else if (BoolFunc.IsTrue(m_AltTime))
                    m_AltTime = "yes";
                else
                    m_AltTime = "no";
            }
            get { return m_AltTime; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAltTime
        {
            get { return (AltTime == "yes"); }
        }
        #endregion AltTime
        #region ToolTipZone
        [System.Xml.Serialization.XmlAttribute("ttipzone")]
        /// EG 20170926 [22374] Affichage dans un tooltip de la date zonée convertie :
        public string ToolTipZone
        {
            set
            {
                m_TooltipZone = GetValidValue(value, true);
                if ("undefined" == m_TooltipZone)
                    m_TooltipZone = "no";
                else if (BoolFunc.IsTrue(m_TooltipZone))
                    m_TooltipZone = "yes";
                else
                    m_TooltipZone = "no";
            }
            get { return m_TooltipZone; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsToolTipZone
        {
            get { return (ToolTipZone == "yes"); }
        }
        #endregion ToolTipZone
        #endregion Accessors

        #region Constructor
        public CustomObjectTimestamp()
            : base()
        {
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override string DefaultPostBackEvent
        {
            get { return "onblur"; }
            //get { return "onchange"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControlModeConsult"></param>
        /// <returns></returns>
        ///FI 20140708 [20179] Modify : Gestion de l'attribut match
        // EG 20171031 [23509] Upd
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            string dataType = DataType;
            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);

            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteTimeStamp() " + clientId);

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left
            };
            if (ContainsHeight)
                cell.Height = UnitHeight;
            cell.Wrap = false;
            SetFixedCol(cell);
            #endregion cell

            #region TextBox

            #region Validator
            Validator[] validators = new Validator[4];
            if (false == isReadOnly)
            {
                #region RequireField
                Validator validatorRequireField = null;
                if (IsMandatory)
                    validatorRequireField = new Validator(Resource + Cst.Space + "[" + Ressource.GetString("MissingData") + "]", false, true);
                validators[0] = validatorRequireField;
                #endregion RequireField

                #region Regular Expression
                bool isExistRegEx = (ContainsRegEx);
                if (isExistRegEx && (EFSRegex.TypeRegex.None != RegexValue))
                    validators[1] = new Validator(RegexValue, Resource + Cst.Space + "[" + Ressource.GetString("InvalidData") + "]", false, true);
                #endregion Regular Expression

                #region ValidationDataType
                Validator validatorDataType = null;
                if ((!isExistRegEx) && GetMiscValue("isSetValidatorType", true))
                {
                    ValidationDataType validationType = ValidationDataType.Date;
                    validatorDataType = new Validator(validationType, Resource + Cst.Space + "[" + Ressource.GetString("InvalidData") + "]", false, true);
                }
                validators[2] = validatorDataType;
                #endregion ValidationDataType

                #region CustomValidator
                Validator validatorCustom = null;
                if (TypeData.IsTypeDate(dataType))
                {
                    validatorCustom = Validator.GetValidatorDateRange(Resource + Cst.Space + "[" + Ressource.GetString("Msg_InvalidDate") + "]", Resource);
                    validatorCustom.IsSummaryOnly = true;
                    validatorCustom.IsShowMessage = false;
                }
                validators[3] = validatorCustom;
                #endregion CustomValidator
            }
            #endregion Validator

            string cssClass = EFSCssClass.Capture;
            if (ContainsCssClass)
                cssClass = CssClass;

            WCZonedDateTime textBox = new WCZonedDateTime(clientId, dataType, IsAltTime, IsFreeZone, IsToolTipZone, isReadOnly, IsMandatory, cssClass, validators);
            if (this.ContainsAccessKey)
            {
                if (AccessKey == "NextBlock")
                    textBox.AccessKey = SystemSettings.GetAppSettings("Spheres_NextBlock_AccessKey", "N");
                else
                    textBox.AccessKey = AccessKey;
            }
            textBox.Enabled = IsEnabled;

            if (isReadOnly)
                textBox.TabIndex = -1;

            if (ContainsToolTip)
                textBox.ToolTip = ResourceToolTip;

            if (false == isReadOnly)
            {
                textBox.AutoPostBack = IsAutoPostBack;
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                textBox.Attributes.Add("onClick", "__doPostBack('" + textBox.UniqueID + "','match');");
                SetLookMatchDefaultValue(textBox);
            }

            if (ContainsStyle)
                ControlsTools.SetStyleList(textBox.Style, Style);

            if (ContainsAttributes)
                ControlsTools.SetAttributesList(textBox.Attributes, Attributes);

            #endregion TextBox

            cell.Controls.Add(textBox);

            if (ContainsColspan)
                cell.ColumnSpan = ColspanIntValue;

            if (ContainsRowspan)
                cell.RowSpan = RowspanIntValue;

            SetFixedCol(cell);

            return cell;
        }
        #endregion
    }



    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("customObject", IsNullable = false)]
    public partial class CustomObjectDropDown : CustomObject
    {
        #region Membres
        private string m_listRetrieval;
        private string m_relativeTo; //Utilisation pour des control qui font référence à des Ids
        private string m_readOnlyMode;
        #endregion Membres

        #region properties
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("listretrieval")]
        public string ListRetrieval
        {
            set
            {
                //20090624 PL Test sur SQL:
                //20091008 PL Test sur DELIMLIST:
                bool isToLower = true;
                string[] array = value.Split(":".ToCharArray(), 2);
                if ((array.Length >= 2) && Enum.IsDefined(typeof(Cst.ListRetrievalEnum), array[0].ToUpper()))
                {
                    Cst.ListRetrievalEnum listRetrieval = (Cst.ListRetrievalEnum)Enum.Parse(typeof(Cst.ListRetrievalEnum), array[0], true);
                    if ((listRetrieval == Cst.ListRetrievalEnum.SQL) || (listRetrieval == Cst.ListRetrievalEnum.DELIMLIST))
                        isToLower = false;
                }
                m_listRetrieval = GetValidValue(value, isToLower);
            }
            get { return m_listRetrieval; }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsListRetrieval
        {
            get { return (!this.ListRetrieval.Equals("undefined")); }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsRelativeTo
        {
            get { return (!this.RelativeTo.Equals("undefined")); }
        }
        /// <summary>
        /// Obyeint l'Id 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("relativeto")]
        public string RelativeTo
        {
            set { m_relativeTo = GetValidValue(value); }
            get { return m_relativeTo; }
        }

        /// <summary>
        /// Obtient la largeur du contrôle
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override Unit UnitWidth
        {
            get
            {
                Unit ret;
                if (!this.ContainsWidth && this.ContainsRelativeTo)
                    ret = Unit.Pixel(200);
                else
                    ret = base.UnitWidth;
                return ret;
            }
        }

        /// <summary>
        /// Obtient l'évèmement sur lequel le PostBack est déclenché 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string DefaultPostBackEvent
        {
            get { return "onblur"; }
        }

        /// <summary>
        /// Obtient true si la propriété ReadOnlyMode est renseignée
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsReadOnlyMode
        {
            get { return (!ReadOnlyMode.Equals("undefined")); }
        }
        /// <summary>
        /// Représente les attributs du mode readonly
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("readonlymode")]
        public string ReadOnlyMode
        {
            set { m_readOnlyMode = GetValidValue(value); }
            get { return m_readOnlyMode; }
        }
        /// <summary>
        /// Obtient un indicateur qui indique si le contrôle DDL associé a été chargée à l'inscription du contrôle dans la page (méthode WriteCell)
        /// </summary>
        /// FI 20130102 [18224] nouvelle property
        /// Certains DDL ne sont pas chargée (Exemple les DDL Enum lorsqu'elles sont en mode ReadOnly)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDropDownLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient true lorsque ListRetrieval contient %% (ListRetrieval est dynamique)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDynamicListRetrieval
        {
            get
            {
                return (StrFunc.IsFilled(ListRetrieval) && ListRetrieval.Contains(Cst.DA_START2));
            }
        }


        /// <summary>
        /// readonlymode est constitué suit
        /// <para>"key1:value1;key2:value2;key3:value3"</para>
        /// <para>Cette méthode permet de récupérer la valeur associée à une clef</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pDefaultValue">Valeur par défaut lorsque la clef {pKey} est non renseignée</param>
        /// <returns></returns>
        public string GetReadOnlyModeValue(string pKey, string pDefaultValue)
        {
            string ret = GetReadOnlyModeValue(pKey);
            if (StrFunc.IsEmpty(ret))
                ret = pDefaultValue;
            return ret;
        }
        /// <summary>
        /// readonlymode est constitué suit
        /// <para>"key1:value1;key2:value2;key3:value3"</para>
        /// <para>Cette méthode permet de récupérer la valeur associée à une clef</para>
        /// </summary>
        /// <returns></returns>
        public string GetReadOnlyModeValue(string pKey)
        {
            string ret = string.Empty;
            if (ContainsReadOnlyMode)
                ret = GetAttributeValue(ReadOnlyMode, pKey);
            return ret;
        }




        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public CustomObjectDropDown()
            : base()
        {
            ListRetrieval = string.Empty;
            RelativeTo = string.Empty;
            m_readOnlyMode = string.Empty;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Write DropDown webcontrol to HTML.
        /// </summary>
        /// <newpara>Description : Write DropDown webcontrol to HTML.</newpara>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <returns>Returns nothing.</returns>
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            TableCell ret;
            switch (Control)
            {
                case ControlEnum.htmlselect:
                case ControlEnum.dropdown:
                case ControlEnum.ddlbanner:
                    ret = WriteDropDown(pPage, pIsControlModeConsult, false);
                    break;
                case ControlEnum.optgroupdropdown:
                    ret = WriteDropDown(pPage, pIsControlModeConsult, true);
                    break;
                default:
                    ret = null;
                    break;
            }
            if (null != ret)
            {
                if (ContainsColspan)
                    ret.ColumnSpan = ColspanIntValue;

                if (ContainsRowspan)
                    ret.RowSpan = RowspanIntValue;

                SetFixedCol(ret);
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pIsControlModeConsult"></param>
        /// <param name="pIsOptionGroup"></param>
        /// <returns></returns>
        /// FI 20121107 [18224] Les DropDowns ne sont plus chargées lorsqu'elles sont en mode Consultation/ReadOnly
        /// FI 20170928 [23452] Modify
        /// EG 20211220 [XXXXX] Ajout événement OnChange sur présence de l'attribut customObject (headlinkinfo)
        private TableCell WriteDropDown(PageBase pPage, bool pIsControlModeConsult, bool pIsOptionGroup)
        {
            string clientId = CtrlClientId;
            bool isModeConsult = pIsControlModeConsult;
            bool isReadOnly = isModeConsult || GetMiscValue("IsReadOnly", isModeConsult);
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteDropDow() " + clientId);

            //FI 20170928 [23452] with Specific lorsque le controle est en ReadOnly
            Nullable<Unit> withOnReadOnly = null;
            if (isReadOnly)
            {
                string with = GetReadOnlyModeValue("width");
                if (StrFunc.IsFilled(with))
                    withOnReadOnly = (with == "Empty") ? Unit.Empty : Unit.Parse(with);
            }

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false,

                Width = UnitWidth
            };
            if (null != withOnReadOnly) //FI 20170928 [23452]
                cell.Width = withOnReadOnly.Value; 
            
            if (ContainsHeight)
                cell.Height = UnitHeight;
            cell.Style.Add(HtmlTextWriterStyle.Overflow, "hidden");
            #endregion cell

            #region DropDown

            string cssClass = EFSCssClass.DropDownListCapture;
            if (isModeConsult)
                cssClass = EFSCssClass.CaptureConsult;
            if (ContainsCssClass)
                cssClass = CssClass;

            // 20090910 RD / OptGroupDropDown
            WCDropDownList2 dropDown;
            if (pIsOptionGroup)
            {
                OptionGroupDropDownList optionGroupDropDown = new OptionGroupDropDownList(isReadOnly, cssClass);
                dropDown = (WCDropDownList2)optionGroupDropDown;
            }
            else
                dropDown = new WCDropDownList2(isReadOnly, cssClass);

            dropDown.ID = clientId;
            dropDown.Enabled = IsEnabled;
            if (isReadOnly)
                dropDown.TabIndex = -1;

            

            if (false == cell.Width.IsEmpty)
                dropDown.Width = cell.Width;

            if (false == cell.Height.IsEmpty)
                dropDown.Height = cell.Height;

            dropDown.IsSetTextOnTitle = (false == ContainsHeadLinkInfo);

            SetDropDownLoaded(isReadOnly);

            if (IsDropDownLoaded)
            {
                ControlsTools.DDLLoad_FromListRetrieval(pPage, dropDown, CSTools.SetCacheOn(SessionTools.CS), ListRetrieval, !IsMandatory, ContainsMisc ? Misc : string.Empty);
            }
            //
            // FI 20121107 [18224] add (false == isReadOnly) => pas nécessaire d'ajouter des attributs PostBack en mode readonly
            if (false == isReadOnly)
            {
                if (IsAutoPostBack)
                {
                    string arg = string.Empty;
                    if (ContainsPostBackArgument)
                        arg = PostBackArgument;
                    // FI 20210325 [XXXXX] setTimeout pour meilleur gestion des focus (en relation avec activeElement) 
                    //dropDown.Attributes.Add(GetPostBackEvent(), GetPostBackMethod() + "('" + dropDown.UniqueID + "','" + arg + "');");
                    dropDown.Attributes.Add(GetPostBackEvent(), $"setTimeout('{GetPostBackMethod()}(\"{dropDown.UniqueID}\",\"{arg}\")',0);");
                }
                if (ContainsHeadLinkInfo)
                    dropDown.Attributes.Add("onchange", "SetLinkIdToHeaderToggle(this,'" + HeadLinkInfo + "');return false;");
            }
            else if ((this.ContainsMatch) && this.IsToMatch)
            {
                dropDown.LblViewer.Attributes.Add("onClick", "__doPostBack('" + dropDown.UniqueID + "','match');");
                // FI 20200124 [XXXXX]  on passe la dropDown
                SetLookMatchDefaultValue(dropDown);
            }

            if (ContainsAttributes)
                ControlsTools.SetAttributesList(dropDown.Attributes, Attributes);

            if (ContainsStyle)
                ControlsTools.SetStyleList(dropDown.Style, Style);


            if (null != withOnReadOnly) //FI 20170928 [23452]
            {
                dropDown.Style.Remove(HtmlTextWriterStyle.Width);
                if (withOnReadOnly.Value != Unit.Empty)
                    dropDown.Style.Add(HtmlTextWriterStyle.Width, withOnReadOnly.Value.ToString());
            }

            CustomObjectTools.SetVisibilityStyle(dropDown, this, pIsControlModeConsult);

            dropDown.SelectedIndex = 0;
            #endregion DropDown

            cell.Controls.Add(dropDown);
            SetFixedCol(cell);
            return cell;
        }
        /// <summary>
        /// Obtient true si le contrôle est chargé via un Enum ou les MapZones (NODA)
        /// </summary>
        /// <returns></returns>
        public bool IsLoadWithEnum
        {
            get
            {
                bool ret = false;
                if (ControlsTools.DecomposeListRetrieval(ListRetrieval, out Cst.ListRetrievalEnum listRetrievalType, out string listRetrievalData))
                    ret = (listRetrievalType == Cst.ListRetrievalEnum.PREDEF) && Cst.IsDDLTypeEnum(listRetrievalData);
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si le contrôle est chargé via les MapZones (NODA)
        /// </summary>
        /// <returns></returns>
        /// EG 20170918 [23452] New
        public bool IsLoadWithMapZone
        {
            get
            {
                bool ret = false;
                if (ControlsTools.DecomposeListRetrieval(ListRetrieval, out Cst.ListRetrievalEnum listRetrievalType, out string listRetrievalData))
                    ret = (listRetrievalType == Cst.ListRetrievalEnum.PREDEF) && Cst.IsDDLTypeMapZone(listRetrievalData);
                return ret;
            }
        }

        /// <summary>
        /// Alimente la propriété isDropDownLoaded
        /// <para>Cette propriété active oui/non le chargement de la DDL</para>
        /// </summary>
        /// <param name="isReadOnly"></param>
        /// FI 20130102 [18224] alimentation de isDropDownLoaded
        /// EG 20170918 [23452] Upd
        private void SetDropDownLoaded(bool isReadOnly)
        {
            IsDropDownLoaded = ContainsListRetrieval;
            if (IsDropDownLoaded)
                IsDropDownLoaded = (false == IsDynamicListRetrieval);

            if (IsDropDownLoaded && isReadOnly)
            {
                if (ContainsReadOnlyMode)
                {
                    IsDropDownLoaded = BoolFunc.IsTrue(GetReadOnlyModeValue("isload"));
                }
                else
                {
                    //FI 20130102 [18224]
                    //Spheres ne charge pas les combos en ReadOnly qui sont alimentées via un enum|mapZone
                    IsDropDownLoaded = (false == IsLoadWithEnum) && (false == IsLoadWithMapZone);
                }
            }
        }


        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// EG 20120613 BlockUI New
    /// EG 20120620 DisplayMode attribute New        
    public partial class CustomObjectButton : CustomObject
    {
        #region Members
        private string m_title;
        private string m_imageUrl;
        private string m_onclick;
        private string m_IsBlockUI;
        private string m_DisplayMode;
        #endregion Members
        //
        #region Accessors
        #region Title
        public bool ContainsTitle
        {
            get { return (!this.Title.Equals("undefined")); }
        }
        public string Title
        {
            set { m_title = GetValidValue(value); }
            get { return m_title; }
        }
        #endregion
        #region Image
        public bool ContainsImageUrl
        {
            get { return (StrFunc.IsFilled(ImageUrl)); }
        }
        public string ImageUrl
        {
            set { m_imageUrl = value; }
            get { return m_imageUrl; }
        }
        #endregion Image
        #region OnClick
        public bool ContainsOnClick
        {
            get { return (StrFunc.IsFilled(OnClick)); }
        }
        public string OnClick
        {
            set { m_onclick = value; }
            get { return m_onclick; }
        }
        #endregion OnClick
        #region BlockUI
        //EG 20120613 BlockUI New
        [System.Xml.Serialization.XmlAttribute("blockUI")]
        public string BlockUI
        {
            set
            {
                m_IsBlockUI = GetValidValue(value, true);
                if ("undefined" == m_IsBlockUI)
                    m_IsBlockUI = "no";
                else if (BoolFunc.IsTrue(m_required))
                    m_IsBlockUI = "yes";
                else
                    m_IsBlockUI = "no";
            }
            get { return m_IsBlockUI; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsBlockUIAttached
        {
            get { return (BlockUI == "yes"); }
        }
        #endregion BlockUI
        #region DisplayMode
        /// EG 20120620 DisplayMode attribute New        
        public bool ContainsDisplayMode
        {
            get { return (StrFunc.IsFilled(DisplayMode)); }
        }
        public string DisplayMode
        {
            set { m_DisplayMode = value; }
            get { return m_DisplayMode; }
        }
        public Nullable<Cst.Capture.ModeEnum> DisplayModeValue
        {
            get
            {
                if (Enum.IsDefined(typeof(Cst.Capture.ModeEnum), m_DisplayMode))
                    return (Cst.Capture.ModeEnum)Enum.Parse(typeof(Cst.Capture.ModeEnum), m_DisplayMode);
                return null;
            }
        }
        #endregion DisplayMode



        #endregion
        //
        #region constructor
        /// EG 20120620 DisplayMode attribute New        
        public CustomObjectButton()
            : base()
        {
            Title = string.Empty;
            ImageUrl = string.Empty;
            OnClick = string.Empty;
            BlockUI = string.Empty;
            DisplayMode = string.Empty;
        }
        #endregion

        #region Method
        /// <summary>
        /// Write Button webcontrol to HTML.
        /// </summary>
        /// <newpara>Description : Write Image webcontrol to HTML.</newpara>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <returns>Returns nothing.</returns>
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteButton() " + clientId);

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false
            };
            if (ContainsWidth)
                cell.Width = UnitWidth;
            if (ContainsHeight)
                cell.Height = UnitHeight;
            #endregion

            WCToolTipButton btn = new WCToolTipButton();
            if (CSS.IsCssClassMain(CssClass))
                btn.Text   = string.Empty;
            btn.CausesValidation = false;
            btn.OnClientClick = "return false;";
            btn.ID = clientId;
            btn.CssClass = CssClass;
            if (ContainsToolTip)
                btn.Pty.TooltipContent = ResourceToolTip;
            if (ContainsToolTipTitle)
                btn.Pty.TooltipTitle = ResourceToolTipTitle;

            if (ContainsStyle)
                ControlsTools.SetStyleList(btn.Style, Style);

            CustomObjectTools.SetVisibilityStyle(btn, this, pIsControlModeConsult);
            btn.TabIndex = -1;
            cell.Controls.Add(btn);
            SetFixedCol(cell);
            return cell;

        }
        #endregion

    }


    /// EG 20151006 New        
    public partial class CustomObjectPanel : CustomObject
    {
        #region Members
        private string m_title;
        private string m_onclick;
        private string m_IsBlockUI;
        private string m_DisplayMode;
        #endregion Members
        //
        #region Accessors
        #region Title
        public bool ContainsTitle
        {
            get { return (!this.Title.Equals("undefined")); }
        }
        public string Title
        {
            set { m_title = GetValidValue(value); }
            get { return m_title; }
        }
        #endregion
        #region OnClick
        public bool ContainsOnClick
        {
            get { return (StrFunc.IsFilled(OnClick)); }
        }
        public string OnClick
        {
            set { m_onclick = value; }
            get { return m_onclick; }
        }
        #endregion OnClick
        #region BlockUI
        //EG 20120613 BlockUI New
        [System.Xml.Serialization.XmlAttribute("blockUI")]
        public string BlockUI
        {
            set
            {
                m_IsBlockUI = GetValidValue(value, true);
                if ("undefined" == m_IsBlockUI)
                    m_IsBlockUI = "no";
                else if (BoolFunc.IsTrue(m_required))
                    m_IsBlockUI = "yes";
                else
                    m_IsBlockUI = "no";
            }
            get { return m_IsBlockUI; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsBlockUIAttached
        {
            get { return (BlockUI == "yes"); }
        }
        #endregion BlockUI
        #region DisplayMode
        /// EG 20120620 DisplayMode attribute New        
        public bool ContainsDisplayMode
        {
            get { return (StrFunc.IsFilled(DisplayMode)); }
        }
        public string DisplayMode
        {
            set { m_DisplayMode = value; }
            get { return m_DisplayMode; }
        }
        public Nullable<Cst.Capture.ModeEnum> DisplayModeValue
        {
            get
            {
                if (Enum.IsDefined(typeof(Cst.Capture.ModeEnum), m_DisplayMode))
                    return (Cst.Capture.ModeEnum)Enum.Parse(typeof(Cst.Capture.ModeEnum), m_DisplayMode);
                return null;
            }
        }
        #endregion DisplayMode
        #endregion
        //
        #region constructor
        /// EG 20120620 DisplayMode attribute New        
        public CustomObjectPanel()
            : base()
        {
            Title = string.Empty;
            OnClick = string.Empty;
            BlockUI = string.Empty;
            DisplayMode = string.Empty;
        }
        #endregion

        #region Method
        /// <summary>
        /// Write Button webcontrol to HTML.
        /// </summary>
        /// <newpara>Description : Write Image webcontrol to HTML.</newpara>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <returns>Returns nothing.</returns>
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WritePanel() " + clientId);

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false
            };
            if (ContainsWidth)
                cell.Width = UnitWidth;
            if (ContainsHeight)
                cell.Height = UnitHeight;
            #endregion

            WCToolTipPanel pnl = new WCToolTipPanel
            {
                ID = clientId,
                CssClass = CssClass
            };
            if (ContainsToolTip)
                pnl.Pty.TooltipContent = ResourceToolTip;
            if (ContainsToolTipTitle)
                pnl.Pty.TooltipTitle = ResourceToolTipTitle;

            if (ContainsStyle)
                ControlsTools.SetStyleList(pnl.Style, Style);

            CustomObjectTools.SetVisibilityStyle(pnl, this, pIsControlModeConsult);
            pnl.TabIndex = -1;
            pnl.Style.Add("outline", "transparent");
            cell.Controls.Add(pnl);
            SetFixedCol(cell);
            return cell;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CustomObjectButtonInputMenu : CustomObjectButton
    {
        #region Members
        private string m_Menu;
        private string m_PK;
        private string m_PKV;
        private string m_FKV;

        #endregion Members

        #region Accessors
        #region ContainsFKV
        public bool ContainsFKV
        {
            get { return (false == FKV.Equals("undefined")); }
        }
        #endregion ContainsFKV
        #region ContainsMenu
        public bool ContainsMenu
        {
            get { return (false == Menu.Equals("undefined")); }
        }
        #endregion ContainsMenu
        #region ContainsPK
        public bool ContainsPK
        {
            get { return (false == PK.Equals("undefined")); }
        }
        #endregion ContainsPK
        #region ContainsPKV
        public bool ContainsPKV
        {
            get { return (false == PKV.Equals("undefined")); }
        }
        #endregion ContainsPKV
        #region FKV
        public string FKV
        {
            set { m_FKV = GetValidValue(value); }
            get { return m_FKV; }
        }
        #endregion PKV
        #region Menu
        public string Menu
        {
            set { m_Menu = GetValidValue(value); }
            get { return m_Menu; }
        }
        #endregion Menu
        #region PK
        public string PK
        {
            set { m_PK = GetValidValue(value); }
            get { return m_PK; }
        }
        #endregion PK
        #region PKV
        public string PKV
        {
            set { m_PKV = GetValidValue(value); }
            get { return m_PKV; }
        }
        #endregion PKV
        #endregion Accessors

        #region Constructors
        public CustomObjectButtonInputMenu()
            : base()
        {
            Menu = string.Empty;
            PK = string.Empty;
            PKV = string.Empty;
            FKV = string.Empty;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public bool IsOk
        {
            get { return this.ContainsMenu; }
        }

        #endregion Methods
    }

    /// <summary>
    /// Classe de gestion d'ouverture d'un bouton Referential
    /// </summary>
    public partial class CustomObjectButtonReferential : CustomObjectButton
    {

        #region Members
        public OpenReferentialArguments args;
        #endregion
        //
        #region Constructor
        /// EG 20120620 invoicing attribute New
        public CustomObjectButtonReferential()
            : base()
        {
            args = new OpenReferentialArguments();
            Referential = string.Empty;
            Invoicing = string.Empty;
            Condition = string.Empty;
            SqlColumn = string.Empty;
            ClientIdForSqlColumn = string.Empty;
            Fk = string.Empty;
            Title = string.Empty;
            // FI 20180502 [23926] Alimentation de FilteredWithSqlColumnOrKeyField
            FilteredWithSqlColumnOrKeyField = string.Empty;
        }
        #endregion Constructor

        
        /// EG 20120620 invoicing attribute New
        public bool ContainsReferential
        {
            get
            {
                return (StrFunc.IsFilled(Referential) && !Referential.Equals("undefined")) ||
                    (StrFunc.IsFilled(Invoicing) && !Invoicing.Equals("undefined")) ||
                (StrFunc.IsFilled(Consultation) && !Consultation.Equals("undefined"));
            }
        }
        public string Referential
        {
            set { args.referential = GetValidValue(value); }
            get { return args.referential; }
        }
        /// EG 20120620 invoicing attribute New
        public string Invoicing
        {
            set { args.invoicing = GetValidValue(value); }
            get { return args.invoicing; }
        }
        public string Consultation
        {
            set { args.consultation = GetValidValue(value); }
            get { return args.consultation; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180502 [23926] add 
        public bool ContainsFilteredWithSqlColumnOrKeyField
        {
            get { return (!FilteredWithSqlColumnOrKeyField.Equals("undefined")); }
        }
        /// <summary>
        ///  
        /// </summary>
        /// FI 20180502 [23926] add 
        public String FilteredWithSqlColumnOrKeyField
        {
            set
            {
                string result = GetValidValue(value);
                args.isFilteredWithSqlColumnOrKeyFieldSpecified = (result != "undefined");
                if (args.isFilteredWithSqlColumnOrKeyFieldSpecified)
                    args.isFilteredWithSqlColumnOrKeyField = BoolFunc.IsTrue(result);
            }
            get
            {
                string ret = "undefined";
                if (args.isFilteredWithSqlColumnOrKeyFieldSpecified)
                    ret = args.isFilteredWithSqlColumnOrKeyField.ToString();
                return ret;
            }
        }
        

        #region Condition
        public bool ContainsCondition
        {
            get { return (!Condition.Equals("undefined")); }
        }
        public string Condition
        {
            set { args.condition = GetValidValue(value); }
            get { return args.condition; }
        }
        #endregion Condition

        #region ClientIdForSqlColumn
        public bool ContainsClientIdForSqlColumn
        {
            get { return (!ClientIdForSqlColumn.Equals("undefined")); }
        }
        public string ClientIdForSqlColumn
        {
            set { args.clientIdForSqlColumn = GetValidValue(value); }
            get { return args.clientIdForSqlColumn; }
        }
        #endregion clientIdForDumpSqlColumn

        #region SqlColumn
        public bool ContainsSqlColumn
        {
            get { return (!SqlColumn.Equals("undefined")); }
        }
        public string SqlColumn
        {
            set { args.sqlColumn = GetValidValue(value); }
            get { return args.sqlColumn; }
        }
        #endregion SqlSpeciField

        #region Fk
        public bool ContainsFK
        {
            get { return (!Fk.Equals("undefined")); }
        }
        public string Fk
        {
            set { args.fk = GetValidValue(value); }
            get { return args.fk; }
        }
        #endregion SqlSpeciField

        #region Param
        public bool ContainsParam
        {
            get { return (ArrFunc.IsFilled(args.param)); }
        }
        public string[] Param
        {
            set { args.param = value; }
            get { return args.param; }
        }
        #endregion


        #region DynamicArgument
        /// <summary>
        /// Obtient true s'il existe des Dynamic Argument
        /// <para>Ils seront passés dans l'URL avec l'argument DA</para>
        /// </summary>
        public bool ContainsDynamicArgument
        {
            get { return (ArrFunc.IsFilled(args.DynamicArgument)); }
        }
        /// <summary>
        /// Obtient ou définit la liste des Arguments Dynamiques 
        /// <para>Ils seront passés dans l'URL avec l'argument DA</para>
        /// <para>Chaque element du tableau est le résultat de la sérialization d'un StringDynamicData </para>
        /// </summary>
        public string[] DynamicArgument
        {
            set { args.DynamicArgument = value; }
            get { return args.DynamicArgument; }
        }
        #endregion

        
        #region isOK
        public bool IsOk
        {
            get { return this.ContainsReferential; }
        }
        #endregion isOK
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CustomObjectButtonFpmlObject : CustomObjectButton
    {
        #region Members
        private string m_object;
        private string m_objectIndex;
        private string m_Element;
        private string m_occurence;
        private string m_copyTo;
        private string m_IsZoomOnModeReadOnly;
        #endregion Members
        //
        #region accessor
        //
        #region Object
        public bool ContainsObject
        {
            get { return (!this.Object.Equals("undefined")); }
        }
        public string Object
        {
            set { m_object = GetValidValue(value); }
            get { return m_object; }
        }
        #endregion Referential
        //
        #region ObjectIndex
        public bool ContainsObjectIndex
        {
            get { return (!this.ObjectIndex.Equals("undefined")); }
        }
        public string ObjectIndex
        {
            set { m_objectIndex = GetValidValue(value); }
            get { return m_objectIndex; }
        }
        public int ObjectIndexValue
        {
            set { m_objectIndex = value.ToString(); }
            get
            {
                int ret;
                try
                {
                    ret = int.Parse(this.ObjectIndex);
                }
                catch { ret = 0; }
                return ret;

            }
        }
        #endregion Index
        //
        #region Element
        public bool ContainsElement
        {
            get { return (!this.Element.Equals("undefined")); }
        }
        public string Element
        {
            set { m_Element = GetValidValue(value); }
            get { return m_Element; }
        }
        #endregion Element
        //
        #region Occurence
        public bool ContainsOccurence
        {
            get { return (!this.Occurence.Equals("undefined")); }
        }
        public string Occurence
        {
            set { m_occurence = GetValidValue(value); }
            get { return m_occurence; }
        }
        public int OccurenceValue
        {
            set { m_occurence = value.ToString(); }
            get
            {
                int ret;
                try
                {
                    ret = int.Parse(this.Occurence);
                }
                catch { ret = 0; }
                return ret;

            }
        }
        #endregion Occurence
        //
        #region CopyTo
        public bool ContainsCopyTo
        {
            get { return (!this.CopyTo.Equals("undefined")); }
        }
        public string CopyTo
        {
            set { m_copyTo = GetValidValue(value); }
            get { return m_copyTo; }
        }
        #endregion CopyTo
        //
        #region isOK
        public bool IsOk
        {
            get { return this.ContainsElement || this.ContainsObject; }
        }
        #endregion isOK
        //	
        #region IsZoomOnModeReadOnly
        public bool ContainsIsZoomOnModeReadOnly
        {
            get { return (!this.IsZoomOnModeReadOnly.Equals("undefined")); }
        }
        public string IsZoomOnModeReadOnly
        {
            set { m_IsZoomOnModeReadOnly = GetValidValue(value); }
            get { return m_IsZoomOnModeReadOnly; }
        }
        #endregion isOK
        #endregion accessor
        //
        #region Constructor
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        public CustomObjectButtonFpmlObject()
            : base()
        {
            Object = string.Empty;
            Element = string.Empty;
            ObjectIndex = string.Empty;
            Occurence = string.Empty;
            CopyTo = string.Empty;
            IsZoomOnModeReadOnly = string.Empty;
            CssClass = "fa-icon";
        }
        #endregion Constructor
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CustomObjectButtonItemArray : CustomObjectButton
    {
        #region Membres
        private string m_xPath;
        private string m_operatortype;
        private string m_prefix;
        #endregion Membres
        //
        #region accessors
        #region OperatorType
        public bool ContainsOperatorType
        {
            get { return (!this.OperatorType.Equals("undefined")); }
        }
        public string OperatorType
        {
            set { m_operatortype = GetValidValue(value); }
            get { return m_operatortype; }
        }
        public Cst.OperatorType OperatorTypeValue
        {
            get
            {
                Cst.OperatorType type;
                try
                {
                    type = (Cst.OperatorType)Enum.Parse(typeof(Cst.OperatorType), this.OperatorType, true);
                }
                catch { type = Cst.OperatorType.add; }
                return type;
            }
        }

        #endregion
        //
        #region XPath
        public bool ContainsXPath
        {
            get { return (!this.XPath.Equals("undefined")); }
        }
        public string XPath
        {
            set { m_xPath = GetValidValue(value); }
            get { return m_xPath; }
        }
        #endregion
        //
        #region Prefix
        public bool ContainsPrefix
        {
            get { return (!this.Prefix.Equals("undefined")); }
        }
        public string Prefix
        {
            set { m_prefix = GetValidValue(value); }
            get { return m_prefix; }
        }
        #endregion
        //
        #region isOK
        public bool IsOk
        {
            get { return StrFunc.IsFilled(this.XPath) && StrFunc.IsFilled(this.OperatorType); }
        }
        #endregion isOK
        #endregion
        //
        #region Constructor
        public CustomObjectButtonItemArray()
            : base()
        {
            Prefix = string.Empty;
            XPath = string.Empty;
            OperatorType = string.Empty;
        }
        #endregion Constructor
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CustomObjectButtonScreenBox : CustomObjectButton
    {
        #region Members
        private string m_screenBox;
        private string m_dialogStyle;
        #endregion Members

        #region Accessors
        #region DialogStyle
        public bool ContainsDialogStyle
        {
            get { return (StrFunc.IsFilled(DialogStyle)); }
        }
        public string DialogStyle
        {
            set { m_dialogStyle = value; }
            get { return m_dialogStyle; }
        }
        #endregion DialogStyle
        #region ScreenBox
        public bool ContainsScreenBox
        {
            get { return (!this.ScreenBox.Equals("undefined")); }
        }
        public string ScreenBox
        {
            set { m_screenBox = GetValidValue(value); }
            get { return m_screenBox; }
        }
        #endregion ScreenBox
        #region isOK
        public bool IsOk
        {
            get { return this.ContainsScreenBox; }
        }
        #endregion isOK
        #endregion Accessors

        #region Constructor
        public CustomObjectButtonScreenBox()
            : base()
        {
            ScreenBox = string.Empty;
            DialogStyle = "fullscreen=no,resizable=yes,location=no,scrollbars=yes,status=yes";
        }
        #endregion Constructor
    }

    /// <summary>
    /// 
    /// </summary>
    // EG 20200914 [XXXXX] Suppression de méthodes obsolètes
    public partial class CustomObjectImage : CustomObject
    {

        #region Membres
        private string m_source;
        #endregion Membres

        #region source
        [System.Xml.Serialization.XmlAttribute("source")]
        public string Source
        {
            set { m_source = GetValidValue(value, true); }
            get { return m_source; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsSource
        {
            get { return (!this.Source.Equals("undefined")); }
        }
        #endregion source

        #region Constructor
        public CustomObjectImage()
            : base()
        {
            Source = string.Empty;
        }
        #endregion Constructor

        #region Method
        /// <summary>
        /// Write Image webcontrol to HTML.
        /// </summary>
        /// <newpara>Description : Write Image webcontrol to HTML.</newpara>
        /// <param name="pNode">Node of XML that describes object.</param>
        /// <returns>Returns nothing.</returns>
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            string clientId = CtrlClientId;
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteImage() " + clientId);

            System.Web.UI.WebControls.Image img;

            #region cell
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false
            };
            if (ContainsWidth)
                cell.Width = UnitWidth;
            // RD 20100120 Add height property
            if (ContainsHeight)
                cell.Height = UnitHeight;
            #endregion

            img = new WCToolTipImage
            {
                ID = clientId,
                ImageUrl = Source,
                ImageAlign = ImageAlign.Left
            };

            ((WCToolTipImage)img).Pty.TooltipContent = ContainsToolTip ? ResourceToolTip : img.AlternateText;
            if (ContainsToolTipTitle)
                ((WCToolTipImage)img).Pty.TooltipTitle = ResourceToolTipTitle;

            if (ContainsStyle)
                ControlsTools.SetStyleList(img.Style, Style);

            CustomObjectTools.SetVisibilityStyle(img, this, pIsControlModeConsult);
            img.TabIndex = -1;
            cell.Controls.Add(img);
            SetFixedCol(cell);
            return cell;

        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CustomObjectBanner : CustomObject
    {
        #region Membres
        private string m_level;
        #endregion Membres

        #region accessors
        #region level
        public bool ContainsLevel
        {
            get { return (!this.Level.Equals("undefined")); }
        }
        public string Level
        {
            set { m_level = GetValidValue(value); }
            get { return m_level; }
        }
        public int LevelIntValue
        {
            get
            {
                int ret = 1;
                if (this.ContainsLevel)
                {
                    try { ret = int.Parse(this.Level); }
                    catch { }
                }
                return ret;
            }
        }
        #endregion level
        #endregion accessors

        #region Constructor
        public CustomObjectBanner()
            : base()
        {
            Level = string.Empty;
        }
        #endregion Constructor
    }



    #region CustomObjectButtonBanner
    public class CustomObjectButtonBanner : CustomObjectButton, ICustomObjectOpenBanner
    {
        #region Membres
        private string m_StartDisplay;
        private string m_TableLinkId;
        private string m_level;
        private int m_ItemOccurs;
        #endregion Membres

        #region ICustomObjectOpenBanner Membres
        #region ItemOccurs
        public int ItemOccurs
        {
            set { m_ItemOccurs = value; }
            get { return m_ItemOccurs; }
        }
        #endregion ItemOccurs
        #region TableLinkId
        public bool ContainsTableLinkId
        {
            get { return (!this.TableLinkId.Equals("undefined")); }
        }
        public string TableLinkId
        {
            set { m_TableLinkId = GetValidValue(value); }
            get { return m_TableLinkId; }
        }
        #endregion TableLinkId
        #region StartDisplay
        public bool ContainsStartDisplay
        {
            get { return (!this.StartDisplay.Equals("undefined")); }
        }
        public string StartDisplay
        {
            set { m_StartDisplay = GetValidValue(value); }
            get { return m_StartDisplay; }
        }
        #endregion StartDisplay
        #endregion

        #region ICustomObjectBanner Membres
        #region level
        public bool ContainsLevel
        {
            get { return (!this.Level.Equals("undefined")); }
        }
        public string Level
        {
            set { m_level = GetValidValue(value); }
            get { return m_level; }
        }
        public int LevelIntValue
        {
            get
            {
                int ret = 1;
                if (this.ContainsLevel)
                {
                    try { ret = int.Parse(this.Level); }
                    catch { }
                }
                return ret;
            }
        }
        #endregion level
        #endregion

        #region Constructor
        public CustomObjectButtonBanner()
            : base()
        {
            Level = string.Empty;
        }
        #endregion Constructor
    }
    #endregion

    #region CustomObjectDLLBanner
    public partial class CustomObjectDDLBanner : CustomObjectDropDown, ICustomObjectOpenBanner
    {
        #region Membres
        private string m_level;
        private string m_StartDisplay;
        private string m_TableLinkId;
        private int m_ItemOccurs;
        #endregion Membres
        //
        #region ICustomObjectBanner Membres
        #region level
        public bool ContainsLevel
        {
            get { return (!this.Level.Equals("undefined")); }
        }
        public string Level
        {
            set { m_level = GetValidValue(value); }
            get { return m_level; }
        }
        public int LevelIntValue
        {
            get
            {
                int ret = 1;
                if (this.ContainsLevel)
                {
                    try { ret = int.Parse(this.Level); }
                    catch { }
                }
                return ret;
            }
        }
        #endregion level
        #endregion
        //
        #region ICustomObjectOpenBanner Membres
        #region ItemOccurs
        public int ItemOccurs
        {
            set { m_ItemOccurs = value; }
            get { return m_ItemOccurs; }
        }
        #endregion ItemOccurs
        #region TableLinkId
        public bool ContainsTableLinkId
        {
            get { return (!this.TableLinkId.Equals("undefined")); }
        }
        public string TableLinkId
        {
            set { m_TableLinkId = GetValidValue(value); }
            get { return m_TableLinkId; }
        }
        #endregion TableLinkId
        #region StartDisplay
        public bool ContainsStartDisplay
        {
            get { return (!this.StartDisplay.Equals("undefined")); }
        }
        public string StartDisplay
        {
            set { m_StartDisplay = GetValidValue(value); }
            get { return m_StartDisplay; }
        }
        #endregion StartDisplay
        #endregion
        //
        #region Constructor
        public CustomObjectDDLBanner()
            : base()
        {
            Level = string.Empty;
        }
        #endregion Constructor
        //
        #region public override WriteCell
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {

            TableCell cell = base.WriteCell(pPage, pIsControlModeConsult);
            DropDownList ddl = (DropDownList)(cell.Controls[0]);
            ControlsTools.SetAttributesList(ddl.Attributes, "isddlbanner:true");
            return cell;

        }
        #endregion
    }

    #endregion CustomObjectBanner

    #region CustomObjectOpenBanner
    public class CustomObjectOpenBanner : CustomObjectBanner, ICustomObjectOpenBanner
    {
        #region Membres
        private string m_StartDisplay;
        private string m_TableLinkId;
        private int m_ItemOccurs;
        #endregion Membres
        //
        #region Accessors
        #region ItemOccurs
        public int ItemOccurs
        {
            set { m_ItemOccurs = value; }
            get { return m_ItemOccurs; }
        }
        #endregion ItemOccurs
        #region TableLinkId
        public bool ContainsTableLinkId
        {
            get { return (!this.TableLinkId.Equals("undefined")); }
        }
        public string TableLinkId
        {
            set { m_TableLinkId = GetValidValue(value); }
            get { return m_TableLinkId; }
        }
        #endregion TableLinkId
        #region StartDisplay
        public bool ContainsStartDisplay
        {
            get { return (!this.StartDisplay.Equals("undefined")); }
        }
        public string StartDisplay
        {
            set { m_StartDisplay = GetValidValue(value); }
            get { return m_StartDisplay; }
        }
        #endregion StartDisplay
        #endregion Accessors
        //
        #region Constructor
        public CustomObjectOpenBanner()
            : base()
        {
            Level = string.Empty;
            TableLinkId = string.Empty;
        }
        #endregion Constructor
    }

    #endregion CustomObjectOpenBanner

    #region CustomObjectHyperLink
    public partial class CustomObjectHyperLink : CustomObject
    {

        #region Members
        private string m_navigateUrl;
        private string m_imageUrl;
        private string m_arguments;
        #endregion Members
        //
        #region Accessors
        #region Arguments
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsArguments
        {
            get { return (!this.Arguments.Equals("undefined")); }
        }
        [System.Xml.Serialization.XmlAttribute("arguments")]
        public string Arguments
        {
            set { m_arguments = GetValidValue(value); }
            get { return m_arguments; }
        }
        #endregion Arguments
        #region ImageUrl
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsImageUrl
        {
            get { return StrFunc.IsFilled(this.ImageUrl); }
        }
        [System.Xml.Serialization.XmlAttribute("imageUrl")]
        public string ImageUrl
        {
            set { m_imageUrl = value; }
            get { return m_imageUrl; }
        }
        #endregion ImageUrl
        #region NavigateUrl
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContainsNavigateUrl
        {
            get { return StrFunc.IsFilled(this.NavigateUrl); }
        }
        [System.Xml.Serialization.XmlAttribute("navigateUrl")]
        public string NavigateUrl
        {
            set { m_navigateUrl = value; }
            get { return m_navigateUrl; }
        }
        #endregion NavigateUrl
        #endregion Accessors
        //
        #region Constructor
        public CustomObjectHyperLink()
            : base()
        {
            NavigateUrl = string.Empty;
            ImageUrl = string.Empty;
            Arguments = string.Empty;
        }
        #endregion Constructor
        //
        #region Methods
        #region public override WriteCell
        public override TableCell WriteCell(PageBase pPage, bool pIsControlModeConsult)
        {
            string clientId = CtrlClientId;
            //
            if (pPage.IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteHyperlink() " + clientId);
            //
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false
            };
            //
            WCToolTipHyperlink hLnk = new WCToolTipHyperlink
            {
                ID = clientId,
                Text = Resource,
                NavigateUrl = NavigateUrl,
                ImageUrl = ImageUrl,
                Enabled = IsEnabled
            };
            //
            if (ContainsToolTip)
            {
                hLnk.Pty.TooltipTitle = Resource;
                hLnk.Pty.TooltipContent = ResourceToolTip;
            }
            //
            if (ContainsCssClass)
                hLnk.CssClass = CssClass;
            //
            if (ContainsStyle)
                ControlsTools.SetStyleList(hLnk.Style, Style);
            //
            hLnk.Visible = true;// isModeConsult;
            //
            if (pPage.IsDebugClientId)
                hLnk.Pty.TooltipContent = hLnk.ID;
            //
            cell.Width = Unit.Pixel(10);
            if (ContainsWidth)
                cell.Width = UnitWidth;
            // RD 20100120 Add height property
            if (ContainsHeight)
                cell.Height = UnitHeight;
            ///
            cell.Controls.Add(hLnk);
            //
            if (ContainsColspan)
                cell.ColumnSpan = ColspanIntValue;
            //
            if (ContainsRowspan)
                cell.RowSpan = RowspanIntValue;
            //
            SetFixedCol(cell);
            //
            return cell;

        }
        #endregion public override WriteCell
        #endregion

    }
    #endregion CustomObjectHyperLink

    #region CustomObjectRowHeader
    public class CustomObjectRowHeader : CustomObject
    {
        #region Membres
        #endregion Membres
        #region Accessors
        #endregion Accessors
        #region Constructor
        public CustomObjectRowHeader()
            : base()
        {
        }
        #endregion Constructor
    }
    #endregion CustomObjectRowHeader
    #region CustomObjectRowFooter
    public class CustomObjectRowFooter : CustomObject
    {
        #region Membres
        #endregion Membres
        #region Accessors
        #endregion Accessors
        #region Constructor
        public CustomObjectRowFooter()
            : base()
        {
        }
        #endregion Constructor
    }
    #endregion CustomObjectRowFooter
    #region public ICustomObjectOpenBanner
    public interface ICustomObjectOpenBanner : ICustomObjectBanner
    {
        //
        bool ContainsTableLinkId { get; }
        string TableLinkId { set; get; }
        //
        bool ContainsStartDisplay { get; }
        string StartDisplay { set; get; }
        //
        int ItemOccurs { set; get; }
        //
    }
    #endregion

    #region public ICustomObjectBanner
    public interface ICustomObjectBanner
    {
        bool ContainsLevel { get; }
        string Level { set; get; }
        int LevelIntValue { get; }
    }
    #endregion

    #region public CustomObjectTools
    public class CustomObjectTools
    {
        #region CustomObjectTools
        /// <summary>
        /// positionne  sur le control l'attribut style à visibility:visible|hidden en fonction de l'attribut misc="visibleonconsult:true|false"  du CustomObject
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pCustomObject"></param>
        /// <param name="pIsControlModeConsult"></param>
        public static void SetVisibilityStyle(System.Web.UI.HtmlControls.HtmlControl pControl, CustomObject pCustomObject, bool pIsControlModeConsult)
        {
            if (pCustomObject.ContainsMisc)
            {
                if (pIsControlModeConsult)
                {
                    bool isVisible = pCustomObject.GetMiscValue("visibleonconsult", true);
                    if (isVisible)
                        ControlsTools.SetStyleList(pControl.Style, "visibility:visible");
                    else
                        ControlsTools.SetStyleList(pControl.Style, "visibility:hidden");
                }
            }

        }
        /// <summary>
        /// positionne  sur le control l'attribut style à visibility:visible|hidden en fonction de l'attribut misc="visibleonconsult:true|false"  du CustomObject
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pCustomObject"></param>
        /// <param name="pIsControlModeConsult"></param>
        public static void SetVisibilityStyle(WebControl pControl, CustomObject pCustomObject, bool pIsControlModeConsult)
        {
            if (pCustomObject.ContainsMisc)
            {
                if (pIsControlModeConsult)
                {
                    bool isVisible = pCustomObject.GetMiscValue("visibleonconsult", true);
                    if (isVisible)
                        ControlsTools.SetStyleList(pControl.Style, "visibility:visible");
                    else
                        ControlsTools.SetStyleList(pControl.Style, "visibility:hidden");
                }
            }

        }
        #endregion
    }
    #endregion

}


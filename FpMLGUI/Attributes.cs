#region Using Directives
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Drawing;
using System.Xml;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.Interface;
#endregion Using Directives

namespace EFS.GUI.Attributes
{
	#region ArrayDivGUI
	/// <summary>
	/// <newpara><b>Description :</b>Array DIV attributes to an Array FpML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML classes used to an array</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class)]
	public sealed class ArrayDivGUI : Attribute
	{
		#region Members
		private string               m_Name             = string.Empty;
		private string               m_HelpURL          = string.Empty;
		private MethodsGUI.LevelEnum m_Level            = MethodsGUI.LevelEnum.None;
		private int                  m_MinItem          = 1;
		private int                  m_MaxItem; 
		private bool                 m_IsClonable;
		private bool                 m_IsMaster;
		private bool                 m_IsMasterVisible;
		private bool                 m_IsChild;
		private bool                 m_IsChildVisible;
		private bool                 m_IsVariableArray  = true;
		private bool                 m_IsProduct;
		private bool                 m_IsCopyPaste;
		private bool                 m_IsCopyPasteItem;
        private MethodsGUI.ColorEnum m_Color;
		#endregion Members
		#region Accessors
		#region HelpURL
		public string HelpURL
		{
			get {return(m_HelpURL);}
			set {m_HelpURL = value;}
		}
		#endregion HelpURL
		#region IsChild
		public bool IsChild
		{
			get {return(m_IsChild);}
			set {m_IsChild = value;}
		}
		#endregion IsChild
		#region IsChildVisible
		public bool IsChildVisible
		{
			get {return(m_IsChildVisible);}
			set {m_IsChildVisible = value;}
		}
		#endregion IsChildVisible
		#region IsClonable
		public bool IsClonable
		{
			get {return(m_IsClonable);}
			set {m_IsClonable = value;}
		}
		#endregion IsClonable
		#region IsCopyPaste
		public bool IsCopyPaste
		{
			get {return(m_IsCopyPaste);}
			set {m_IsCopyPaste = value;}
		}
		#endregion IsCopyPaste
		#region IsCopyPasteItem
		public bool IsCopyPasteItem
		{
			get {return(m_IsCopyPasteItem);}
			set {m_IsCopyPasteItem = value;}
		}
		#endregion IsCopyPasteItem
		#region IsMaster
		public bool IsMaster
		{
			get {return(m_IsMaster);}
			set {m_IsMaster = value;}
		}
		#endregion IsMaster
		#region IsMasterVisible
		public bool IsMasterVisible
		{
			get {return(m_IsMasterVisible);}
			set {m_IsMasterVisible = value;}
		}
		#endregion IsMasterVisible
		#region IsProduct
		public bool IsProduct
		{
			get {return(m_IsProduct);}
			set {m_IsProduct = value;}
		}
		#endregion IsProduct
		#region IsVariableArray
		public bool IsVariableArray
		{
			get {return(m_IsVariableArray);}
			set {m_IsVariableArray = value;}
		}
		#endregion IsVariableArray
		#region Level
		public MethodsGUI.LevelEnum Level
		{
			get {return(m_Level);}
			set {m_Level = value;}
		}
		#endregion Level
		#region MaxItem
		public int MaxItem
		{
			get {return(m_MaxItem);}
			set {m_MaxItem = value;}
		}
		#endregion MaxItem
		#region MinItem
		public int MinItem
		{
			get {return(m_MinItem);}
			set {m_MinItem = value;}
		}
		#endregion MinItem
		#region Name
		public string Name
		{
			get {return(m_Name);}
			set {m_Name = value;}
		}
		#endregion Name
        #region Color
        public MethodsGUI.ColorEnum Color
        {
            get { return (m_Color); }
            set { m_Color = value; }
        }
        #endregion Color
		#endregion Accessors
		#region Constructors
		public ArrayDivGUI(){}
		#endregion Constructors
	}
	#endregion ArrayDivGUI
	#region BookMarkGUI
	/// <summary>
	/// <newpara><b>Description :</b>Set a bookMark to an FpML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class)]
    public sealed class BookMarkGUI : Attribute
	{
		#region Variables
		private string m_Name;
		private bool   m_isVisible;
		#endregion Variables
		#region Accessors
		public string Name
		{
			get {return(m_Name);}
			set {m_Name = value;}
		}
		public bool IsVisible
		{
			get {return(m_isVisible);}
			set {m_isVisible = value;}
		}
		#endregion Accessors
	}
	#endregion BookMarkGUI
	#region CloseDivGUI
	/// <summary>
	/// <newpara><b>Description :</b>Close DIV attributes to an FpML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class,AllowMultiple = true)]
    public sealed class CloseDivGUI : Attribute
	{
		#region Variables
		private MethodsGUI.LevelEnum m_Level;
		private string               m_Name;
		#endregion Variables
		#region Accessors
		public MethodsGUI.LevelEnum Level
		{
			get {return(m_Level);}
			set {m_Level = value;}
		}
		public string Name
		{
			get {return(m_Name);}
			set {m_Name = value;}
		}
		#endregion Accessors
		#region Constructors
		public CloseDivGUI(){}
		public CloseDivGUI(MethodsGUI.LevelEnum pLevel):this(pLevel,string.Empty){}
		public CloseDivGUI(MethodsGUI.LevelEnum pLevel, string pName)
		{
			m_Level = pLevel;
			m_Name  = pName;
		}
		#endregion Constructors
	}
	#endregion CloseDivGUI
	#region ControlGUI
	/// <summary>
	/// <newpara><b>Description :</b>Control attributes associated an FpML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class,AllowMultiple = true)]
    /// EG 20170918 [23342] Add m_IsDatePickerAlt
    /// EG 20170926 [22374] Upd
    public sealed class ControlGUI : Attribute
	{
        #region Members
        private string m_Name = string.Empty;
        private string m_HelpURL = string.Empty;
        private MethodsGUI.LevelEnum m_Level = MethodsGUI.LevelEnum.None;
        private int m_LblWidth;
        private int m_Width;
        private MethodsGUI.LineFeedEnum m_LineFeed = MethodsGUI.LineFeedEnum.None;
        private bool m_IsDisplay;
        private bool m_IsLabel = true;
        private bool m_IsPrimary = true;
        private bool m_IsSpaceBefore = true;
        private bool m_IsLocked;
        private EFSRegex.TypeRegex m_Regex = EFSRegex.TypeRegex.None;
        private bool m_IsCopyPaste;
        private MethodsGUI.ColorEnum m_Color = MethodsGUI.ColorEnum.None;
        private bool m_IsAltTime = true;
        #endregion Members
        #region Accessors
		#region HelpURL
		public string HelpURL
		{
			get {return(m_HelpURL);}
			set {m_HelpURL = value;}
		}
		#endregion HelpURL
		#region IsCopyPaste
		public bool IsCopyPaste
		{
			get {return(m_IsCopyPaste);}
			set {m_IsCopyPaste = value;}
		}
		#endregion IsCopyPaste
		#region IsDisplay
		public bool IsDisplay
		{
			get {return(m_IsDisplay);}
			set {m_IsDisplay = value;}
		}
		#endregion IsDisplay
		#region IsLabel
		public bool IsLabel
		{
			get {return(m_IsLabel);}
			set {m_IsLabel = value;}
		}
		#endregion IsLabel
		#region IsLocked
		public bool IsLocked
		{
			get {return(m_IsLocked);}
			set {m_IsLocked = value;}
		}
		#endregion IsLocked
		#region IsPrimary
		public bool IsPrimary
		{
			get {return(m_IsPrimary);}
			set {m_IsPrimary = value;}
		}
		#endregion IsPrimary
		#region IsSpaceBefore
		public bool IsSpaceBefore
		{
			get {return(m_IsSpaceBefore);}
			set {m_IsSpaceBefore = value;}
		}
		#endregion IsSpaceBefore
		#region LblWidth
		public int LblWidth
		{
			get {return(m_LblWidth);}
			set {m_LblWidth = value;}
		}
		#endregion LblWidth
		#region Level
		public MethodsGUI.LevelEnum Level
		{
			get {return(m_Level);}
			set {m_Level = value;}
		}
		#endregion Level
		#region LineFeed
		public MethodsGUI.LineFeedEnum LineFeed
		{
			get {return(m_LineFeed);}
			set {m_LineFeed = value;}
		}
		#endregion LineFeed
		#region Name
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public string Name
		{
			get {return(m_Name);}
			set { m_Name = (value?.Replace(" ", Cst.HTMLSpace)); }
		}
		#endregion Name
		#region Regex
		public EFSRegex.TypeRegex Regex
		{
			get {return(m_Regex);}
			set {m_Regex = value;}
		}
		#endregion Regex
		#region Width
		public int Width
		{
			get {return(m_Width);}
			set {m_Width = value;}
		}
		#endregion Width
        #region Color
        public MethodsGUI.ColorEnum Color
        {
            get { return (m_Color); }
            set { m_Color = value; }
        }
        #endregion Color
        #region IsAltTime
        // EG 20170918 [23342] New
        public bool IsAltTime
        {
            get { return (m_IsAltTime); }
            set { m_IsAltTime = value; }
        }
        #endregion IsAltTime
        #endregion Accessors
        #region Constructors
        public ControlGUI(){}
		public ControlGUI(bool pIsLabel,string pName)
		{
			m_IsLabel = pIsLabel;
			m_Name    = pName;
		}
        // EG 20170918 [23342] New
        public ControlGUI(bool pIsLabel, string pName, bool pIsAltTime)
            : this(pIsLabel, pName)
        {
            m_IsAltTime = pIsAltTime;
        }
		#endregion Constructors
	}
	#endregion ControlGUI
	#region CreateControlGUI
	/// <summary>
	/// <newpara><b>Description :</b>Return GUI method to apply</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class)]
    public sealed class CreateControlGUI : Attribute
	{
		#region Members
		private MethodsGUI.CreateControlEnum m_Declare;
		#endregion Members
		#region Accessors
		public MethodsGUI.CreateControlEnum Declare
		{
			get {return m_Declare;}
			set {m_Declare = value;}
		}
		#endregion Accessors
		#region Constructors
		public CreateControlGUI(){m_Declare = MethodsGUI.CreateControlEnum.Mandatory;}
		#endregion Constructors
	}
	#endregion CreateControlGUI
	#region OpenDivGUI
	/// <summary>
	/// <newpara><b>Description :</b>Open DIV attributes to an FpML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class,AllowMultiple = true)]
    public sealed class OpenDivGUI : Attribute
	{
		#region Members
		private string               m_Name;
		private int                  m_LabelWidth;
		private string               m_HelpURL;
		private MethodsGUI.LevelEnum m_Level;
		private bool                 m_IsVisible;
		private bool                 m_IsGroup;
		private bool                 m_IsProduct;
		private string               m_UniqueId    = string.Empty;
		private bool                 m_IsCopyPaste;
        private MethodsGUI.ColorEnum m_Color;
        private bool m_IsAutoClose = false;
        #endregion Members
        #region Accessors
        public int LabelWidth
		{
			get {return(m_LabelWidth);}
			set {m_LabelWidth = value;}
		}
		public string Name
		{
			get {return(m_Name);}
			set {m_Name = value;}
		}
		public string HelpURL
		{
			get {return(m_HelpURL);}
			set {m_HelpURL = value;}
		}
		public MethodsGUI.LevelEnum Level
		{
			get {return(m_Level);}
			set {m_Level = value;}
		}
		public bool IsVisible
		{
			get {return(m_IsVisible);}
			set {m_IsVisible = value;}
		}
		public bool IsGroup
		{
			get {return(m_IsGroup);}
			set {m_IsGroup = value;}
		}
		public bool IsProduct
		{
			get {return(m_IsProduct);}
			set {m_IsProduct = value;}
		}
		public string UniqueId
		{
			get {return(m_UniqueId);}
			set {m_UniqueId = value;}
		}
		public bool IsCopyPaste
		{
			get {return(m_IsCopyPaste);}
			set {m_IsCopyPaste = value;}
		}
        public MethodsGUI.ColorEnum Color
        {
            get { return (m_Color); }
            set { m_Color = value; }
        }
        public bool IsAutoClose
        {
            get { return (m_IsAutoClose); }
            set { m_IsAutoClose = value; }
        }
		#endregion Accessors
		#region Constructors
		public OpenDivGUI(){}
		public OpenDivGUI(MethodsGUI.LevelEnum pLevel, string pName,bool pIsVisible)
			:this(pLevel,pName,pIsVisible,string.Empty,0,MethodsGUI.ColorEnum.None){}
		public OpenDivGUI(MethodsGUI.LevelEnum pLevel, string pName,bool pIsVisible,string pHelpURL)
			:this(pLevel,pName,pIsVisible,pHelpURL,0,MethodsGUI.ColorEnum.None){}
        public OpenDivGUI(MethodsGUI.LevelEnum pLevel, string pName, bool pIsVisible, string pHelpURL,int pLabelWidth)
            : this(pLevel, pName, pIsVisible, pHelpURL, pLabelWidth, MethodsGUI.ColorEnum.None) { }
        public OpenDivGUI(MethodsGUI.LevelEnum pLevel, string pName, bool pIsVisible, string pHelpURL, int pLabelWidth, MethodsGUI.ColorEnum pColor)
		{
			m_Level      = pLevel;
			m_Name       = pName;
			m_IsVisible  = pIsVisible;
			m_HelpURL    = pHelpURL;
			m_LabelWidth = pLabelWidth;
            m_Color = pColor;
		}
		#endregion Constructors
	}
	#endregion OpenDivGUI
	#region ReferenceGUI
	/// <summary>
	/// <newpara><b>Description :</b>Set FpML/FixML Reference group to FpML/FixML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FpML/FixML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class,AllowMultiple = true)]
    public sealed class ReferenceGUI : Attribute
	{
		#region Variables
		private MethodsGUI.ReferenceEnum m_Reference;
		#endregion Variables
		#region Accessors
		public MethodsGUI.ReferenceEnum Reference
		{
			get {return(m_Reference);}
			set {m_Reference = value;}
		}
		#endregion Accessors
	}
	#endregion ReferenceGUI
	#region DictionaryGUI
	/// <summary>
	/// <newpara><b>Description :</b>Set FixML specifications help Link for an FixML object</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b>All FixML classes</newpara>
	///</remarks>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class,AllowMultiple = true)]
    public sealed class DictionaryGUI : Attribute
	{
		#region Variables
		private string                    m_Page;
		private string                    m_Anchor;
        private string                    m_Tag;
        #endregion Variables
		#region Accessors
		public string Page
		{
			get {return(m_Page);}
			set {m_Page = value;}
		}
		public string Anchor
		{
			get {return(m_Anchor);}
			set {m_Anchor = value;}
		}
        public string Tag
        {
            get { return (m_Tag); }
            set { m_Tag = value; }
        }

		#endregion Accessors
	}
	#endregion DictionaryGUI

    #region MainTitleGUI
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MainTitleGUI : Attribute
    {
        #region Members
        private string m_Title;
        #endregion Members
        #region Accessors
        public string Title
        {
            get { return (m_Title); }
            set { m_Title = value; }
        }
        #endregion MainTitleGUI
    }
    #endregion BookMarkGUI


	#region MethodsGUI
	/// <summary>
	/// <newpara><b>Description :</b>List of GUI enums and static methods</newpara>
	/// <newpara><b>Contents :</b></newpara>
	///</summary>
	public class MethodsGUI
	{
		#region Enums
		#region CreateControl - Enum
		public enum CreateControlEnum
		{
			Mandatory,
			None,
			Optional,
			ValidatorOptional,
		}
		#endregion CreateControl - Enum
		#region LineFeed - Enum
		public enum LineFeedEnum
		{
			None           = 0,
			Before         = 1,
			After          = 2, 
			BeforeAndAfter = 3,
		}
		#endregion LineFeed - Enum
		#region OpenDiv - Enum
		public enum LevelEnum
		{
			None            = 0,
			First           = -1,
			Intermediary    = -2,
			End             = -3,
			FirstKey        = -10,
			IntermediaryKey = -20,
			EndKey          = -30,
			Fixed           = 50, // balise qui ne s'ouvre pas ni se ferme
			HiddenKey       = 100,

		}
		#endregion OpenDiv - Enum
		#region Reference - Enum
        // EG 20140526 New build FpML4.4 (ValuationDates)
        /// EG 20161122 New Commodity Derivative
        /// EG 20170918 [23342] Add BusinessUnit
		public enum ReferenceEnum
		{
			Account,
            AdjustabledDates,
			AssetReference,
			BusinessDayAdjustments,
			BusinessCenter,
            BusinessUnit,
			BusinessCenters,
            CalculationPeriods,
			CalculationPeriodDates,
            CalculationPeriodsSchedule,
            CommodityLeg,
			Currency,
            DeliveryQuantity,
			PaymentQuote,
			DateRelativeTo,
			Entity,
			Formula,
            FxRate,
			Index,
			Notional,
			Party,
			PartyPortFolio,
			PayerReceiver,
			Payment,
			PaymentDates,
			PortFolio,
			Product,
			QueryParameterId,
			QueryParameterOperator,
			Rate,
			RateIndex,
			ResetDates,
			SecurityCode,
            SecurityLeg,
            SettlementPeriods,
			Trade,
			TradeSide,
            ValuationDates,
			// EG 20231127 [WI753] Implementation Return Swap : New
			PaymentCurrency,
		}
		#endregion Reference - Enum
		#region HelpSource - Enum
		public enum HelpSourceEnum
		{
			FpML,
			FixML,
			EfsML,
		}
		#endregion HelpSource - Enum
        #region ColorEnum
        public enum ColorEnum
        {
            None=0,
            Orange=1,
            Green=2,
            Violet=3,
            Marron=4,
            Red=5,
            Rose=6,
            Gray=7,
            Blue=8,
            BlueLight=9,
            Yellow=10,
			Brown = 11,
		}
		#endregion ColorEnum
		#endregion Enums

		#region Constructor
		private MethodsGUI() { }
        #endregion Constructor
        #region Methods
        #region BookMarkGUI - Methods
        #region GetBookMark
        public static string GetBookMark(FieldInfo pFldCapture)
		{
			object[] attributes = pFldCapture.GetCustomAttributes(typeof(BookMarkGUI),true);
			if (0 != attributes.GetLength(0))
			{
				if ("?" == ((BookMarkGUI) attributes[0]).Name)
					((BookMarkGUI) attributes[0]).Name = pFldCapture.Name;
				return ((BookMarkGUI) attributes[0]).Name;
			}
			else
				return string.Empty;
		}
		#endregion GetBookMark
		#region WriteBookMark
		public static void WriteBookMark(FieldInfo pFldCapture,int pIndex,FullConstructor pFullCtor)
		{
			if (null != pFldCapture) 
			{
				object[] attributes = pFldCapture.GetCustomAttributes(typeof(BookMarkGUI),true);
				if (0 != attributes.GetLength(0))
				{
					BookMarkGUI bookMarkGUI     = (BookMarkGUI)attributes[0];
					string name                 = StrFunc.IsFilled(bookMarkGUI.Name)?bookMarkGUI.Name:pFldCapture.Name;
					HistoryBalise historyBalise = new HistoryBalise(name,bookMarkGUI.IsVisible,pIndex);
					pFullCtor.HistoryBalise.Add(historyBalise);
				}
			}
		}
		#endregion GetBookMark
		#endregion BookMarkGUI - Methods
		#region ControlGUI - Methods
		#region GetControl
		public static ControlGUI GetControl(FieldInfo pFldCapture){return GetControl(pFldCapture,true);}
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static ControlGUI GetControl(FieldInfo pFldCapture,bool pIsPrimary)
		{
			ControlGUI controlGUI = new ControlGUI
			{
				IsLabel = false
			};
			object[] attributes   = pFldCapture.GetCustomAttributes(typeof(ControlGUI),true);
			for (int i=0;i<attributes.GetLength(0);i++)
			{
				controlGUI = (ControlGUI) attributes[i];
				if (controlGUI.IsPrimary == pIsPrimary)
					break;
			}
			return controlGUI;
		}
		#endregion GetControl
		#region IsLineFeedAfter
		public static bool IsLineFeedAfter(LineFeedEnum pLineFeed)
		{
			return IsLineFeedAfterOnly(pLineFeed) || IsLineFeedBeforeAndAfter(pLineFeed);
		}
		#endregion IsLineFeedAfter
		#region IsLineFeedAfterOnly
		public static bool IsLineFeedAfterOnly(LineFeedEnum pLineFeed)
		{
			return (LineFeedEnum.After == pLineFeed);
		}
		#endregion IsLineFeedAfterOnly
		#region IsLineFeedBefore
		public static bool IsLineFeedBefore(LineFeedEnum pLineFeed)
		{
			return IsLineFeedBeforeOnly(pLineFeed) || IsLineFeedBeforeAndAfter(pLineFeed);
		}
		#endregion IsLineFeedBefore
		#region IsLineFeedBeforeOnly
		public static bool IsLineFeedBeforeOnly(LineFeedEnum pLineFeed)
		{
			return (LineFeedEnum.Before == pLineFeed);
		}
		#endregion IsLineFeedBeforeOnly
		#region IsLineFeedBeforeAndAfter
		public static bool IsLineFeedBeforeAndAfter(LineFeedEnum pLineFeed)
		{
			return (LineFeedEnum.BeforeAndAfter == pLineFeed);
		}
		#endregion IsLineFeedBeforeAndAfter
		#endregion ControlGUI - Methods
		#region CreateControlGUI - Methods
		#region GetCreateControl
		public static MethodsGUI.CreateControlEnum GetCreateControl(FieldInfo pFldCapture)
		{
			object[] attributes = pFldCapture.GetCustomAttributes(typeof(CreateControlGUI),true);
			if (0 == attributes.GetLength(0))
				return CreateControlEnum.Mandatory;
			else
				return ((CreateControlGUI) attributes[0]).Declare;
		}
		#endregion GetCreateControl
		#region GetRealCreateControl
		public static MethodsGUI.CreateControlEnum GetCreateControl(FieldInfo pFldCapture,bool pIsStep)
		{
			MethodsGUI.CreateControlEnum createControl = GetCreateControl(pFldCapture);
			if (pIsStep && IsNoneControl(createControl))
				return CreateControlEnum.Mandatory;
			else
				return createControl;
		}
		#endregion GetRealCreateControl
		#region IsDefineOptionalControl
		public static bool IsDefineOptionalControl(FieldInfo pFieldInfo)
		{
			return CreateControlEnum.Optional == GetCreateControl(pFieldInfo);
		}
		#endregion IsDefineOptionalControl
		#region IsMandatoryControl
		public static bool IsMandatoryControl(FieldInfo pFieldInfo)
		{
			return CreateControlEnum.Mandatory == GetCreateControl(pFieldInfo);
		}
		#endregion IsMandatoryControl
		#region IsNoneControl
		public static bool IsNoneControl(FieldInfo pFieldInfo)
		{
			return CreateControlEnum.None == GetCreateControl(pFieldInfo);
		}
		public static bool IsNoneControl(CreateControlEnum pCreateControl)
		{
			return CreateControlEnum.None == pCreateControl;
		}
		#endregion IsNoneControl
		#region IsOptionalControl
		public static bool IsOptionalControl(object pObject,FieldInfo pFieldInfo)
		{
			return IsDefineOptionalControl(pFieldInfo) || 
				(null != pObject.GetType().GetField(pFieldInfo.Name + Cst.FpML_SerializeKeySpecified));
		}
		#endregion IsOptionalControl
		#region IsValidatorOptional
		public static bool IsValidatorOptionalControl(FieldInfo pFieldInfo)
		{
			return CreateControlEnum.ValidatorOptional == GetCreateControl(pFieldInfo);
		}
		public static bool IsValidatorOptionalControl(CreateControlEnum pCreateControl)
		{
			return CreateControlEnum.ValidatorOptional == pCreateControl;
		}
		#endregion IsValidatorOptional
		#endregion CreateControlGUI - Methods
		#region OpenDivGUI/CloseDivGUI - Methods
		#region CreateHelpImage
		// EG 20200908 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		private static Panel CreateHelpImage(string pHelpUrl)
        {
			Panel pnl = new Panel
			{
				CssClass = "fa-icon fas fa-question-circle",
				TabIndex = -1,
			};
			pnl.Attributes.Add("onclick", "window.open('" + pHelpUrl + "','_blank','location=no,scrollbars=yes');return false;");
            pnl.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
			pnl.Style.Add(HtmlTextWriterStyle.MarginLeft, "140px");
            return pnl;
        }
		#endregion CreateHelpImage

		#region CreateHiddenKeyText
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		private static HtmlInputHidden CreateHiddenKeyText(FullConstructor pFullCtor,bool pIsVisible,string pCurrentStream,
			string[] FpMLObject,bool pIsVirtualBalise)
		{
			HtmlInputHidden txt = new HtmlInputHidden
			{
				EnableViewState = true,
				Value = pIsVisible ? "1" : "0",
				ID = Cst.TXT + (pFullCtor == null ? string.Empty : pFullCtor.NbBalise.ToString())
			};
			// Attributes
			if (StrFunc.IsFilled(pCurrentStream))
				txt.Attributes.Add("stream",pCurrentStream);

			if (1 < FpMLObject.Length)
			{
				txt.Attributes.Add("parent",FpMLObject[0]);
				txt.Attributes.Add("current",FpMLObject[1]);
			}
			else
				txt.Attributes.Add("current",FpMLObject[0]);
			txt.Attributes.Add("virtual",pIsVirtualBalise?"1":"0");
			return txt;
		}
		#endregion CreateHiddenKeyText
		#region CreateUniqueID
		public static string CreateUniqueID(FullConstructor pFullCtor,object pParent,FieldInfo pFldParent,object pCapture,FieldInfo pFldCapture,int pItemCurrent)
		{

			#region ParentId
			string parentId = Cst.OTCml_Name;
			if (null != pFldParent)
			{
				if (pFldParent.DeclaringType.Name.Equals("Strategy"))
					parentId = pFldParent.DeclaringType.BaseType.Name.ToLower();
				else
					parentId = pFldParent.Name;

				if (pFldParent.FieldType.IsArray && (null != pParent))
				{
					object obj = pFldParent.GetValue(pParent);
					int item   = Array.IndexOf((System.Array) obj, pCapture);
					item++;

					//if (pFldParent.DeclaringType.Equals(typeof(FpML.Strategy)))
					if (pFldParent.DeclaringType.Name.Equals("Strategy"))
						parentId = pCapture.GetType().Name;
					else
						parentId = parentId + "_" + item.ToString();
				}
			}
			#endregion ParentId
			#region Id
			string id = string.Empty;
			if (null != pFldCapture)
			{
				if (pFldCapture.DeclaringType.Name.Equals("Strategy"))
				{
					if (pFldCapture.FieldType.IsArray)
					{
						object obj = ((System.Array)pFldCapture.GetValue(pCapture)).GetValue(pItemCurrent - 1);
						id         = obj.GetType().Name;
					}
				}
				else
				{
					id = pFldCapture.Name;
					if (0 < pItemCurrent)
						id += "_" + pItemCurrent.ToString();
				}
			}
			#endregion Id

			#region UniqueID
			if (parentId.Equals(id))
			{
				int index = pFullCtor.KeyBaliseName.Count-1;
                if (0 <= index)
				    return pFullCtor.KeyBaliseName[index].ToString();
			}
			return parentId + ":" + id;
			#endregion UniqueID
		}
		#endregion CreateUniqueID
		#region GetLevelKey
		public static LevelEnum GetLevelKey(LevelEnum pLevel)
		{
			if (LevelEnum.First == pLevel)
				return LevelEnum.FirstKey;
			else if (LevelEnum.Intermediary == pLevel)
				return LevelEnum.IntermediaryKey;
			else if (LevelEnum.End == pLevel)
				return LevelEnum.EndKey;
			else
				return LevelEnum.None;
		}
		#endregion GetLevelKey
        #region IsFullColor
        public static bool IsFullColor(ColorEnum pColor)
        {
            return (ColorEnum.None != pColor);
        }
        #endregion IsFullColor

        #region IsExtendibleLevel
        public static bool IsExtendibleLevel(LevelEnum pLevel)
		{
			return (LevelEnum.HiddenKey != pLevel) && (LevelEnum.Fixed != pLevel) && (LevelEnum.None != pLevel);
		}
		#endregion IsExtendibleLevel
		#region IsFixedLevel
		public static bool IsFixedLevel(LevelEnum pLevel)
		{
			return (LevelEnum.HiddenKey == pLevel) || (LevelEnum.Fixed == pLevel);
		}
		#endregion IsFixedLevel
		#region IsFixedOrNoneLevel
		public static bool IsFixedOrNoneLevel(LevelEnum pLevel)
		{
			return IsFixedLevel(pLevel) || IsNoneLevel(pLevel);
		}
		#endregion IsFixedOrNoneLevel
		#region IsNoneLevel
		public static bool IsNoneLevel(LevelEnum pLevel)
		{
			return (LevelEnum.None == pLevel);
		}
		#endregion IsNoneLevel
		#region MakeCloseDiv
		private static PlaceHolder MakeCloseDiv(LevelEnum pLevel)
		{
			PlaceHolder plh              = new PlaceHolder();
			System.Text.StringBuilder sb = new StringBuilder();
			switch (pLevel)
			{
				case LevelEnum.None:
				case LevelEnum.HiddenKey:
					sb.AppendFormat(string.Empty);
					break;
				default:
					sb.AppendFormat("</div></div>");
					break;
			}
			plh.Controls.Add(new LiteralControl(sb.ToString()));
			return plh;
		}
		#endregion MakeCloseDiv
		#region MakeDiv
		public static PlaceHolder MakeDiv(object pAttributeGUI){return MakeDiv(null,pAttributeGUI,false);}
		public static PlaceHolder MakeDiv(FullConstructor pFullCtor,object pAttributeGUI){return MakeDiv(pFullCtor,pAttributeGUI,false);}
		public static PlaceHolder MakeDiv(FullConstructor pFullCtor,object pAttributeGUI,bool pIsChildControl)
		{
			PlaceHolder plh = new PlaceHolder();
			if (pAttributeGUI.GetType().Equals(typeof(OpenDivGUI)))
			{
				OpenDivGUI openDiv = (OpenDivGUI) pAttributeGUI;
				plh = MethodsGUI.MakeOpenDiv(pFullCtor,openDiv.Level,openDiv.IsVisible,openDiv.IsGroup,openDiv.Name,
					openDiv.UniqueId,openDiv.HelpURL,openDiv.LabelWidth,openDiv.Color);
				if ((IsExtendibleLevel(openDiv.Level)) && (false == pIsChildControl))
					plh.Controls.Add(MethodsGUI.MakeOpenDiv(pFullCtor,MethodsGUI.GetLevelKey(openDiv.Level),
						openDiv.IsVisible,openDiv.IsGroup,openDiv.Name,openDiv.UniqueId,openDiv.HelpURL,openDiv.LabelWidth,openDiv.Color));
				else if (IsFixedLevel(openDiv.Level) && (false == pIsChildControl))
					plh.Controls.Add(MethodsGUI.MakeCloseDiv(openDiv.Level));
			}
			else if (pAttributeGUI.GetType().Equals(typeof(CloseDivGUI)))
			{
				CloseDivGUI closeDiv = (CloseDivGUI) pAttributeGUI;
				plh = MethodsGUI.MakeCloseDiv(closeDiv.Level);
			}
			return plh;
		}
		#endregion MakeDiv
		#region MakeOpenDiv

		// EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
		// EG 20211217 [XXXXX] Gestion icon (Font Awesome) ouverture/Fermeture FpML (identique à un toggle)
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		private static PlaceHolder MakeOpenDiv(FullConstructor pFullCtor, LevelEnum pLevel, bool pIsVisible, bool pIsGroup, string pCaption,
			string pUniqueId, string pHelpUrl, int pLabelWidth, ColorEnum pColor)
		{
			PlaceHolder plh = new PlaceHolder
			{
				EnableViewState = true,
				ID = "B",
			};
			string currentStream = string.Empty;
			if (null != pFullCtor)
			{
				plh.ID += pFullCtor.NbBalise.ToString();
				if (ArrFunc.IsFilled(pFullCtor.HistoryBalise))
				{
					HistoryBalise histo = (HistoryBalise)pFullCtor.HistoryBalise[pFullCtor.HistoryBalise.Count - 1];
					currentStream = histo.Name;
				}
			}
			// 20071023 EG Add next line
			//
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string[] FpMLObject = pUniqueId.Split(new char[] { ':' });

			switch (pLevel)
			{
				case LevelEnum.End:
				case LevelEnum.Intermediary:
				case LevelEnum.First:
					#region LevelEnum.First
					// Entête
					int i = Math.Abs(Int32.Parse(Enum.Format(typeof(LevelEnum), pLevel, "d")));
					string level = i.ToString();
					sb.AppendFormat("<div class=\"full-margin\">");
					sb.AppendFormat("<div level=\"{0}\" color=\"{1}\" class=\"full-capture full-header\">", level, pColor.ToString().ToLower());
					sb.AppendFormat("<div level=\"{0}\" class=\"fa-icon fa fa-angle-{1}\"></div>", level, pIsVisible ? "down" : "right");
					sb.AppendFormat("<span level=\"{0}\" class=\"full-btn\">{1}</span>", level, pCaption);
					break;
				#endregion LevelEnum.First
				case LevelEnum.EndKey:
				case LevelEnum.IntermediaryKey:
				case LevelEnum.FirstKey:
					#region LevelEnum.FirstKey, LevelEnum.IntermediaryKey, LevelEnum.EndKey
					plh.Controls.Add(new LiteralControl(@"<span id=""" + plh.ID + "_" + FpMLObject[FpMLObject.Length > 1 ? 1 : 0] + @""" class=""full-capture-textsupplement"">"));
					// Key Balise Parent/Child
					plh.Controls.Add(CreateHiddenKeyText(pFullCtor, pIsVisible, currentStream, FpMLObject, false));
					if (StrFunc.IsFilled(pHelpUrl) && (false == pIsGroup))
						plh.Controls.Add(CreateHelpImage(pHelpUrl));
					plh.Controls.Add(new LiteralControl(@"</span>"));
					sb.AppendFormat("</div>");

					if ((true == pIsGroup) && (1 < FpMLObject.Length))
						pUniqueId = FpMLObject[0] + ":" + FpMLObject[0];
					sb.AppendFormat(@"<div id=""{1}_b"" class=""full-region"" style=""display:{0}"">", (pIsVisible ? "block" : "none"), plh.ID + "_" + pUniqueId);
					break;
				#endregion LevelEnum.FirstKey, LevelEnum.IntermediaryKey, LevelEnum.EndKey
				case LevelEnum.Fixed:
					#region LevelEnum.Fixed
					sb.AppendFormat(@"<div class=""full-margin"">");
					sb.AppendFormat(@"<div class=""full-fixed"">");
					// pLabelWidth
					string style = string.Empty;
					if (0 != pLabelWidth)
						style = "style='width:" + pLabelWidth.ToString() + "px;'";
					sb.AppendFormat(@"<span class=""full-fixedtext"" {1}>{0}</span>", Cst.HTMLSpace + pCaption + Cst.HTMLSpace, style);
					if (StrFunc.IsFilled(pHelpUrl) && (false == pIsGroup))
						plh.Controls.Add(CreateHelpImage(pHelpUrl));
					break;
				#endregion LevelEnum.Fixed
				case LevelEnum.HiddenKey:
					#region LevelEnum.HiddenKey
					// Entête
					plh.Controls.Add(new LiteralControl(@"<span id=""" + plh.ID + "_" + FpMLObject[FpMLObject.Length > 1 ? 1 : 0] +
						@""" class=""full-hiddentext"">"));
					// Key Balise Parent/Child
					plh.Controls.Add(CreateHiddenKeyText(pFullCtor, pIsVisible, currentStream, FpMLObject, true));

					if (StrFunc.IsFilled(pHelpUrl) && (false == pIsGroup))
						plh.Controls.Add(CreateHelpImage(pHelpUrl));

					plh.Controls.Add(new LiteralControl(@"</span>"));
					break;
					#endregion LevelEnum.HiddenKey
			}
			if (null != pFullCtor)
				pFullCtor.NbBalise++;

			plh.Controls.Add(new LiteralControl(sb.ToString()));
			return plh;
		}
		#endregion MakeOpenDiv
		#endregion OpenDivGUI/CloseDivGUI - Methods

		#region ConvertColor
		public static Color ConvertColor(ColorEnum pColor, string pDefColor)
        {
            string color;
            switch (pColor)
            {
                case MethodsGUI.ColorEnum.Blue:
                    color = CstCSSColor.blue;
                    break;
                case MethodsGUI.ColorEnum.BlueLight:
                    color = CstCSSColor.blueLight;
                    break;
                case MethodsGUI.ColorEnum.Gray:
                    color = CstCSSColor.gray;
                    break;
                case MethodsGUI.ColorEnum.Green:
                    color = CstCSSColor.green;
                    break;
                case MethodsGUI.ColorEnum.Marron:
				case MethodsGUI.ColorEnum.Brown:
                    color = CstCSSColor.marron;
                    break;
                case MethodsGUI.ColorEnum.Orange:
                    color = CstCSSColor.orange;
                    break;
                case MethodsGUI.ColorEnum.Red:
                    color = CstCSSColor.red;
                    break;
                case MethodsGUI.ColorEnum.Rose:
                    color = CstCSSColor.rose;
                    break;
                case MethodsGUI.ColorEnum.Violet:
                    color = CstCSSColor.violet;
                    break;
                case MethodsGUI.ColorEnum.Yellow:
                    color = CstCSSColor.yellow;
                    break;
                case MethodsGUI.ColorEnum.None:
                default:
                    color = pDefColor;
                    break;

            }
            return Color.FromName(color);
        }
        #endregion ConvertColor

        #region public GetReference
        public static string GetReference(FieldInfo pFldCapture)
		{
			if (pFldCapture.IsDefined(typeof(ReferenceGUI),true))
			{
				object[] attributes = pFldCapture.GetCustomAttributes(typeof(ReferenceGUI),true);
				return ((ReferenceGUI) attributes[0]).Reference.ToString() + Cst.FpML_ClassReference;
			}
			else if ("href" == pFldCapture.Name)
				return pFldCapture.DeclaringType.Name;
			else
				return pFldCapture.FieldType.Name;

		}
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static string[] GetReference(object pCapture,FieldInfo pFldCapture)
		{
			string[] keyReference;
			if (pFldCapture.IsDefined(typeof(ReferenceGUI),true))
			{
				object[] attributes = pFldCapture.GetCustomAttributes(typeof(ReferenceGUI),true);
				keyReference        = new String[attributes.Length];
				for (int i=0;i<attributes.Length;i++)
					keyReference[i] = ((ReferenceGUI) attributes[i]).Reference + Cst.FpML_ClassReference;
			}
			else
				keyReference = new String[1] {pCapture.GetType().Name + Cst.FpML_ClassReference};
			return keyReference;
		}
		#endregion GetReference
		#region public GetInstrumentName
		public static string GetInstrumentName(object pCapture)
		{
			string productName  = string.Empty;
			if (null != pCapture)
			{
				bool isNewKeyWord   = false;
				char[] aProductName = pCapture.GetType().Name.ToCharArray();
				for (int i=0;i<aProductName.Length;i++)
				{
					if (0 < i) 
						isNewKeyWord = (!Char.IsUpper(aProductName[i-1]));
					if (Char.IsUpper(aProductName[i]) && isNewKeyWord) 
						productName += " ";
					productName += aProductName[i].ToString();
				}
				FieldInfo fld = pCapture.GetType().GetField("productType");
				if (null != fld)
				{
					object productType = fld.GetValue(pCapture);
					fld                = productType.GetType().GetField("Value");
					if (null != fld)
					{
						productName += " [" + fld.GetValue(productType).ToString() + "]";
					}
				}
			}
			return productName;

		}
		public static string GetInstrumentName(object pCapture,FieldInfo pFldCapture)
		{
			return GetInstrumentName(pFldCapture.GetValue(pCapture));
		}
		#endregion GetInstrumentName
		#region public GetHelpLink
		/// <summary>
		/// <newpara><b>Description :</b>Constitution of the URL for the on-line help </newpara>
		///</summary>
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static string GetHelpLink(object pObject,FieldInfo pField)
		{
            string schema = pField.FieldType.Namespace;
            if (("EFS.GUI.Interface" == schema) || (-1 < schema.IndexOf("System")))
                schema = pField.DeclaringType.Namespace;
            else if (pField.FieldType.Name.Equals("Product"))
            {
                object obj = pField.GetValue(pObject);
                schema = obj.GetType().FullName;
            }
			string url;
            if (-1 < schema.IndexOf(Cst.FixML_Name.ToUpper()))
                url = GetFixMLHelpLink( pField);
            else
            {
                bool isEFSmLHelp = (bool)SystemSettings.GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
                if (isEFSmLHelp)
                {
                    string element = GetEFSmLElementHelpLink(pObject, pField);
                    url = GetEFSmLHelpUrl(schema, element);
                }
                else
                    url = GetFpMLHelpLink(pObject, pField);
            }
            return url;
		}
		#endregion GetHelpLink
        #region public GetEFSmLHelpUrl
        // 20090519 EG Add FIXml help link
        public static string GetEFSmLHelpUrl(string pSchema,string pElement)
        {
            string url = string.Empty;

            XmlDocument indexSchemas = null;
            if (null != System.Web.HttpContext.Current.Application["EFSmL_HelpIndexSchemas"])
                indexSchemas = (XmlDocument)System.Web.HttpContext.Current.Application["EFSmL_HelpIndexSchemas"];

            if (null != indexSchemas)
            {
                string xPathValue = "//Elements[@category='{0}']";
                string category = "All Items";
                if (StrFunc.IsFilled(pSchema))
                {
                    if (pSchema.ToLower().StartsWith("fpml"))
                        category = Cst.FpML_Name;
                    else if (pSchema.ToLower().StartsWith("efsml"))
                        category = "EFSmL";
                    else if (pSchema.ToLower().StartsWith("fixml"))
                        category = "FixML";
                    else if (pSchema.ToLower().StartsWith("all"))
                        category = "All Items";
                }
                XmlNode xmlNode = indexSchemas.SelectSingleNode(xPathValue.Replace("{0}", category));
                if (null != xmlNode)
                {
                    xmlNode = xmlNode.SelectSingleNode(pElement + "/text()");
                    if (null != xmlNode)
                    {
                        url = SystemSettings.GetAppSettings("EFSmLHelpSchemasUrl");
                        url += "/Docs/" + xmlNode.Value + ".html"; ;
                    }
                }
                if (StrFunc.IsEmpty(url))
                    System.Diagnostics.Debug.Write(pElement);
            }
            return url;
        }
		#endregion
		#region public GetEFSmLHelpLink
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static string GetEFSmLElementHelpLink(object pObject, FieldInfo pField)
		{
			Type tElement  = pField.FieldType;
			if (null != pField.GetValue(pObject))
			{
				Type tObject = pField.GetValue(pObject).GetType();
				if (tObject.BaseType.Equals(tElement))
					tElement = tObject;
			}
			string element;
			if ("EFS.GUI.Interface" == tElement.Namespace)
                element = pObject.GetType().Name;
			else
			{
				if (tElement.IsArray)
					element = tElement.GetElementType().Name.Replace("Array",string.Empty);
				else
					element = tElement.Name;
			}
            return element;
		}
		#endregion GetFpMLHelpLink
		#region public GetFpMLHelpLink
		/// <summary>
		/// <newpara><b>Description :</b>Set Url on-line help for FpML</newpara>
		///</summary>
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static string GetFpMLHelpLink(object pObject, FieldInfo pField)
        {
            Type tElement = pField.FieldType;
            bool isOldMethode = IsFpmlHelpVersionBefore4_4();
            if (null != pField.GetValue(pObject))
            {
                Type tObject = pField.GetValue(pObject).GetType();
                if (tObject.BaseType.Equals(tElement))
                    tElement = tObject;
            }

			string element;
			string schema;
			if ("EFS.GUI.Interface" == tElement.Namespace)
            {
                schema = pField.DeclaringType.Namespace;
                if (isOldMethode)
                    element = "Complex_" + pObject.GetType().Name;
                else
                    element = pObject.GetType().Name;
            }
            else
            {
                schema = tElement.Namespace;
                //
                if (tElement.IsArray)
                    element = tElement.GetElementType().Name.Replace("Array", string.Empty);
                else
                    element = tElement.Name;
                //
                if (isOldMethode)
                    element = (tElement.IsEnum ? "Simple_" : "Complex_") + element;
            }
            //
            schema = schema.Replace(".", "-").ToLower();
            //
            // 20080305 FI Ticket 16119 => suppression de n° de version Exemple "-v42-" donne "-" 
            Regex rexEx = new Regex(@"^\w+-v\d+-\w+$");
            if (rexEx.IsMatch(schema))
                schema = Regex.Replace(schema, @"-v\d+-", "-");
            //
            if (Cst.FpML_Name == schema)
                schema += "-doc";
            //
            string url = GetFpMLHelpUrl(schema, element);
            return url;
        }
        #endregion GetFpMLHelpLink
		#region public GetFpMLHelpUrl
		public static  string GetFpMLHelpUrl(string pSchema,string pElement)
		{
            string url = SystemSettings.GetAppSettings("FpMLHelpUrl");	
            string version = SystemSettings.GetAppSettings("FpMLHelpVersion");
			if (IsFpmlHelpVersionBefore4_4())
			{
				url += pSchema + "-" +  version + ".html#" + pElement;
                //20060731 PL Add ToLower() suite à error 404 depuis la saisie Full
				url  = url.ToLower();
			}
			else
			{
				url += "schemaRef/";
				url += pSchema + "-" +  version + ".xsd." + "html_type_" + pElement + ".html";
			}
			return url;
		}
		#endregion 
		#region public GetFixMLHelpLink
		/// <summary>
		/// <newpara><b>Description :</b>Set Url on-line help for FixML</newpara>
		///</summary>
		public static string GetFixMLHelpLink(FieldInfo pField)
		{
			if (pField.IsDefined(typeof(DictionaryGUI),true))
			{
				object[] attributes = pField.GetCustomAttributes(typeof(DictionaryGUI),true);
				if (0 < attributes.Length)
				{
					string url               = SystemSettings.GetAppSettings("FixMLHelpUrl");
					string version           = SystemSettings.GetAppSettings("FixMLHelpVersion");
					DictionaryGUI dictionary = (DictionaryGUI) attributes[0];
                    string page = StrFunc.IsEmpty(dictionary.Tag) ? "fields_sorted_by_name" : "tag" + dictionary.Tag + ".html";
                    return url + "FIX" + version + "/" + page;
				}
			}
			return string.Empty;
		}
		#endregion GetFixMLHelpLink
		#region public GetValidationSummaryMessage
		public static string GetValidationSummaryMessage(FieldInfo pFldCapture, FieldInfo pFldParent)
		{
			string name = string.Empty;
			ControlGUI controlGUI = GetControl(pFldCapture,false);
			if (StrFunc.IsFilled(controlGUI.Name))
				name = controlGUI.Name + " [";

			object[] attributes;
			if (null != pFldParent)
			{
				attributes = pFldParent.GetCustomAttributes(typeof(OpenDivGUI),true);
				if (0 < attributes.GetLength(0))
				{
					if (((OpenDivGUI) attributes[0]).Level!=LevelEnum.HiddenKey)
						name += ((OpenDivGUI) attributes[0]).Name;
					else
						name += pFldParent.Name;
				}
				else
				{
					controlGUI = GetControl(pFldParent,false);
					if (StrFunc.IsFilled(controlGUI.Name))
						name += controlGUI.Name;
					else
						name += pFldParent.Name;
				}
			}
			else
			{
				attributes = pFldCapture.GetCustomAttributes(typeof(OpenDivGUI),true);
				if (0 < attributes.GetLength(0))
					if (((OpenDivGUI) attributes[0]).Level!=LevelEnum.HiddenKey)
						name += ((OpenDivGUI) attributes[0]).Name;
					else
						name += pFldCapture.DeclaringType.Name;
			}
			if (-1 < name.IndexOf("["))
				name += "]";
			return name;
		}
		#endregion GetValidationSummaryMessage
		#region public IsModeConsult
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static bool IsModeConsult(object pPage)
        {
            Type tForm = pPage.GetType();
            PropertyInfo pty = tForm.GetProperty("IsModeConsult", typeof(Boolean));
			bool isModeconsult;
			if (null != pty)
                isModeconsult = (bool)tForm.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pPage, null);
            else
                throw new NotImplementedException(" property 'IsModeConsult' not implemented on The Page");

            // 20090107 RD 16099
            if (false == isModeconsult)
            {
                bool isScreenFullCapture;
                PropertyInfo ptyFull = tForm.GetProperty("IsScreenFullCapture", typeof(Boolean));
                if (null != ptyFull)
                    isScreenFullCapture = (bool)tForm.InvokeMember(ptyFull.Name, BindingFlags.GetProperty, null, pPage, null);
                else
                    throw new NotImplementedException(" property 'IsScreenFullCapture' not implemented on The Page");

                isModeconsult = isScreenFullCapture && IsModeUpdatePostEvts(pPage);
            }
            return isModeconsult;
        }
		#endregion IsModeConsult
		#region public IsModeUpdatePostEvts
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static bool IsModeUpdatePostEvts(object pPage)
        {
            Type tForm = pPage.GetType();
            PropertyInfo pty = tForm.GetProperty("IsModeUpdatePostEvts", typeof(Boolean));
			bool isModeUpdatePostEvts;
			if (null != pty)
                isModeUpdatePostEvts = (bool)tForm.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pPage, null);
            else
                throw new NotImplementedException(" property 'IsModeUpdatePostEvts' not implemented on The Page");

            return isModeUpdatePostEvts;
        }
		#endregion IsModeUpdatePostEvts
		// EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
		public static bool IsModeUpdateFeesUninvoiced(object pPage)
		{
			Type tForm = pPage.GetType();
			PropertyInfo pty = tForm.GetProperty("IsModeUpdateFeesUninvoiced", typeof(Boolean));
			bool isModeUpdateFeesUninvoiced;
			if (null != pty)
				isModeUpdateFeesUninvoiced = (bool)tForm.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pPage, null);
			else
				throw new NotImplementedException(" property 'IsModeUpdateFeesUninvoiced' not implemented on The Page");

			return isModeUpdateFeesUninvoiced;
		}
		#region public IsStEnvironment_Template
		// EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
		public static bool IsStEnvironment_Template(object pPage)
        {
            Type tForm = pPage.GetType();
            PropertyInfo pty = tForm.GetProperty("IsStEnvironment_Template", typeof(Boolean));
			bool ret;
			if (null != pty)
                ret = (bool)tForm.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pPage, null);
            else
                throw new NotImplementedException(" property 'IsStEnvironment_Template' not implemented on The Page");

            return ret;
        }
        #endregion IsStEnvironment_Template
		#region public IsFpmlHelpVersionBefore4_4
		public static bool IsFpmlHelpVersionBefore4_4()
		{
			bool ret = false;
			string version = SystemSettings.GetAppSettings("FpMLHelpVersion");
			//
			string[] tmp= version.Split("-".ToCharArray()) ;
			if (tmp.GetLength(0)>0)
			{
				int major = Convert.ToInt32(tmp[0].Trim()); 
				int minor = Convert.ToInt32(tmp[1].Trim()); 
				ret  = (major<=4) && (minor<4); 
			}
			return ret;
		}
		#endregion 
        #region public SetHelpUrlLink
		/// <summary>
		/// <newpara><b>Description :</b>Set events to label for linkedHelp</newpara>
		///</summary>
		public static void SetHelpUrlLink(Label pLabel,string pHelpUrl)
		{
			if (StrFunc.IsFilled(pHelpUrl))
			{
				pLabel.Attributes.Add("onmouseout","this.className='lblCaptureTitle';");
				pLabel.Attributes.Add("onmouseover","this.className='lblCaptureTitleHighLevel';");
				pLabel.Attributes.Add("onclick","window.open('"+ pHelpUrl + "','_blank','location=no,scrollbars=yes');return false;");
                pLabel.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
			}
		}
		#endregion SetHelpUrlLink
        #region public SetEventHandler
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void SetEventHandler(ControlCollection pCtrlcollection)
        {
            if (null != pCtrlcollection)
            {
                foreach (Control ctrl in pCtrlcollection)
                {
                    //
                    try
                    {
                        Type t = ctrl.GetType();
                        MethodInfo m = t.GetMethod("SetEventHandler");
                        if (m != null)
                            t.InvokeMember(m.Name, BindingFlags.InvokeMethod, null, ctrl, null, null, null, null);
                    }
                    catch (Exception) { throw; }
                    //
                    if (null != ctrl.Controls)
                        SetEventHandler(ctrl.Controls);
                }
            }
        }
        #endregion
        #region public IsZoomOnFull
        //FI 20091118 [16744] Add IsZoomOnFull
        /// <summary>
        /// Retourne true si la page est celle utilisée pour faire un Zoom depuis la saisie light  
        /// </summary>
        /// <param name="pPage"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static bool IsZoomOnFull(object pPage)
        {
            try
            {
                bool ret = false;
                Type tForm = pPage.GetType();
                PropertyInfo pty = tForm.GetProperty("IsZoomOnFull", typeof(Boolean));
                if (null != pty)
                    ret = (bool)tForm.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pPage, null);
                else
                    throw new NotImplementedException(" property 'IsZoomOnFull' not implemented on The Page");

                return ret;
            }
            catch (NotImplementedException) { throw; }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion IsZoomOnFull

        #endregion Methods
    }
	#endregion MethodsGUI

    // EG 20160404 Migration vs2013
    // #warning TO BE DEFINE and REPLACE XmlEnumAttribute in All CciEnum
	#region CciEnumInstance
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Class,AllowMultiple = true)]
    public sealed class CciEnumInstance : Attribute
	{
		#region Variables
		private string m_Name;
		#endregion Variables
		#region Accessors
		public string Name
		{
			get {return(m_Name);}
			set {m_Name = value;}
		}
		#endregion Accessors
		#region Constructors
		public CciEnumInstance():this(string.Empty){}
		public CciEnumInstance(string pName)
		{
			m_Name    = pName;
		}
		#endregion Constructors
	}
	#endregion CciInstance
}

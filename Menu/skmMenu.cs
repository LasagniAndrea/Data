#region Using Directives
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Xml;
using System.Runtime.InteropServices;


using EFS.Common;
using EFS.ACommon;
using EFS.Common.Web;

#endregion Using Directives


namespace skmMenu
{
	#region public class MenuItemParent
    //EG 20120613 BlockUI New
	public class MenuItemParent
	{
		#region Members
		public string aID;				    // attribut ID
		public string aEnabled = "true";	// attribut Enabled
		public string aHidden  = "false";	// attribut Hidden
		public string aToolTip;    	        // attribut ToolTip
		public string eText;			    // elem text
		public string eUrl;				    // elem url
		public string eImageUrl;    	    // elem imageurl
		public string eCommandName;		    // elem commandName
		public string eArgument;		    // elem argument
		public string eLayout="Horizontal"; // elem layout
        public string eBlockUIMessage;	    // elem Message BLockUI


		public MenuItemParent[] subItems;
		
		private long _Root;
		// EG 20240123 [WI816] Update private to public
		public long _Childs;
		
		private string _mainNode;
		private string _subNode;
		private string _itemNode;
		private readonly string _textNode        ="text";
		private readonly string _urlNode         ="url";
		private readonly string _imageUrlNode    ="imageUrl";
		private readonly string _commandNameNode ="commandName";
		private readonly string _argumentNode    ="argument";
		private readonly string _layoutNode      ="layout";
        private readonly string _blockUIMessageNode = "blockUIMessage";

		#endregion Members

		#region public accessors
		public bool HasChilds
		{
			get
			{ return _Childs > 0;}
		}

		#region Enabled
		public bool Enabled
		{
			set {aEnabled = value.ToString().ToLower();}
			get {return Convert.ToBoolean(aEnabled);}
		}
		#endregion Enabled
		#region Hidden
		public bool Hidden
		{
			set {aHidden = value.ToString().ToLower();}
			get {return Convert.ToBoolean(aHidden);}
		}
		#endregion Hidden
		#endregion

		#region constructor
		// EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
		public MenuItemParent()
		{
		}
		// EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
		public MenuItemParent(string pText, bool pIsEnabled, Cst.Capture.ModeEnum pMode)
		{
			eText = Ressource.GetString(pText);
			Enabled = pIsEnabled;
			eCommandName = pMode.ToString();
			eBlockUIMessage = eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
		}
		public MenuItemParent(long nbChilds)
		{
			if (nbChilds > 0)
			{
				subItems = new MenuItemParent[nbChilds];
				for (int i=0;i<nbChilds;i++)
					subItems[i]= new MenuItemParent(0);
			}
			else
			{
				nbChilds = 0;
			}
			_Childs = nbChilds;

		}
        /// <summary>
        /// Accès au  menu enfant
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
		public MenuItemParent this[long l]
		{
			get 
			{	
				return subItems[l];
			}
			set
			{
				subItems[l] = value;
			}
		}
		#endregion constructor
		
		#region public setRoot
		public void SetRoot()
		{
			_Root = 1;
		}
		#endregion

		#region public initXmlWriter
		public XmlDocument InitXmlWriter()
		{
			return InitXmlWriter("menu","subMenu","menuItem");
		}
		public XmlDocument InitXmlWriter(string mainNode, string subNode, string itemNode)
		{
			SetRoot();
			_mainNode=(mainNode ?? "menu");
			_subNode=(subNode ?? "subMenu");
			_itemNode=(itemNode ?? "menuItem");
			XmlDocument doc = new XmlDocument ();
			XmlAddChilds(doc, null);
			// ---pour debug----
			/*
			XmlTextWriter writer = new XmlTextWriter(@"c:\jkrtestdata.xml",null);
			writer.Formatting = Formatting.Indented;
			doc.Save(writer);
			writer.Close();*/
			
			return doc;
		}
		#endregion public initXmlWriter
		
		#region public XmlAddChilds
		//EG 20120613 BlockUI New
        public long XmlAddChilds(XmlDocument doc, XmlNode parentNode)
		{
			long returnChilds = 0;
			if (_Root != 1)
			{
				if (this.aHidden.ToLower() == "false")
				{
					XmlNode newValue; 
					long countChilds        = 0;
					XmlNode newNode         = doc.CreateElement(_itemNode);
					XmlAttribute newID      = doc.CreateAttribute("ID");
					XmlAttribute newToolTip = doc.CreateAttribute("ToolTip");
					XmlAttribute newEnabled = doc.CreateAttribute("Enabled");
					if (this.aID!=null)
					{
						newID.Value = this.aID;
						newNode.Attributes.Append(newID);
					}
					if (this.aToolTip!=null)
					{
						newToolTip.Value = this.aToolTip;
						newNode.Attributes.Append(newToolTip);
					}

					newValue = doc.CreateElement(_textNode);
					newValue.InnerText=this.eText;
					newNode.AppendChild(newValue);

					if (this.aEnabled!=null)
					{
						newEnabled.Value = this.aEnabled;
						newNode.Attributes.Append(newEnabled);
					}

					if (this.aEnabled.ToLower() == "true")
					{
						if (this.eUrl!=null)
						{
							newValue = doc.CreateElement(_urlNode);
							newValue.InnerText=this.eUrl;
							newNode.AppendChild(newValue);
						}

						if (this.eCommandName!=null)
						{
							newValue = doc.CreateElement(_commandNameNode);
							newValue.InnerText=this.eCommandName;
							newNode.AppendChild(newValue);
						}

						if (this.eArgument!=null)
						{
							newValue = doc.CreateElement(_argumentNode);
							newValue.InnerText=this.eArgument;
							newNode.AppendChild(newValue);
						}

                        if (this.eBlockUIMessage != null)
                        {
                            newValue = doc.CreateElement(_blockUIMessageNode);
                            newValue.InnerText = this.eBlockUIMessage;
                            newNode.AppendChild(newValue);
                        }

					}

					if (this.eImageUrl!=null)
					{
						newValue = doc.CreateElement(_imageUrlNode);
						newValue.InnerText=this.eImageUrl;
						newNode.AppendChild(newValue);
					}

					newValue = doc.CreateElement(_layoutNode);
					if (this.eLayout!=null)
						newValue.InnerText=this.eLayout;
					else
						newValue.InnerText="Horizontal";
					newNode.AppendChild(newValue);

					if (this.HasChilds)
					{
						newValue = doc.CreateElement(_subNode);
						
						for (int i=0;i<this._Childs;i++)
						{
							this[i]._mainNode=this._mainNode;
							this[i]._subNode=this._subNode;
							this[i]._itemNode=this._itemNode;
							countChilds += this[i].XmlAddChilds(doc, newValue);
						}
						if (countChilds > 0)
						{
							returnChilds ++;
							newNode.AppendChild(newValue);
						}
						if (returnChilds > 0 )
							parentNode.AppendChild(newNode);
					}
					else
					{
						parentNode.AppendChild(newNode);
						returnChilds ++;
					}
				}
			}
			else
			{
				XmlNode newValue = doc.CreateElement(_mainNode);
				if (this.HasChilds)
				{
					for (int i=0;i<this._Childs;i++)
					{
						this[i]._mainNode=this._mainNode;
						this[i]._subNode=this._subNode;
						this[i]._itemNode=this._itemNode;
						this[i].XmlAddChilds(doc, newValue);

					}
				}
				returnChilds ++;
				doc.AppendChild(newValue);
			}
			return returnChilds;
		}
		#endregion

        #region AlternativeAction

        /// <summary>
        /// Evaluate the alternative action part of the URL
        /// </summary>
        /// <remarks>
        /// the URL value is stocked in the eUrl instance field
        /// </remarks>
        /// <param name="pKeyValuesCollection">
        /// Collection containing all the parameter values
        /// <value>
        /// The first line of the array contains a subset of the Parameters array.
        /// The second line contains the relative values.
        /// </value> 
        /// </param>
        /// <param name="pArgument"></param>
        /// <returns>true if the URL contains an alternative action</returns>
        public bool EvaluateURLAlternativeAction(string[][] pKeyValuesCollection)
        {
            bool parsed = URLAlternativeActionParser.Parse(this.eUrl, out _, out _, out string[] dependsOnParameters, out string[] optionalParameters);

            if (parsed)
            {
                string[] parameters = ArrFunc.ConcatArray<string>(dependsOnParameters, optionalParameters);

                this.eUrl = URLAlternativeActionParser.Evaluate(this.eUrl, parameters, pKeyValuesCollection);
            }

            return parsed;
        }

        #endregion AlternativeAction

    }
	#endregion
	
	public delegate void MenuItemClickedEventHandler(object sender, MenuItemClickEventArgs e);
	
	/// <summary>
	/// The MenuLayout enumeration specifies the layout settings for the menu
	/// </summary> 
	public enum MenuLayout { Horizontal, Vertical }
	public enum MenuLayoutDOWN { DOWN, UP }

	/// <summary>
	/// The menu class is an ASP.NET server control that displays a client-side menu utilizing
	/// CSS and DHTML.
	/// </summary>
	[
	DefaultProperty("ID"),
	ToolboxData("<{0}:Menu runat=server></{0}:Menu>"),
	Designer("skmMenu.Design.MenuDesigner"),
	ParseChildren(true),
	PersistChildren(false),
	DefaultEvent("MenuItemClick")
	]
	
	#region public class Menu
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
	public class Menu : Control, IPostBackEventHandler
	{
		// Specify the clicked event
		public event MenuItemClickedEventHandler MenuItemClick;
		
		#region Members
		private readonly string subMenuKey                     = "-sm";
		private int    mnuNum;
		private string cssClassMnuMasterTable = "BannerMenu";
        private string cssClassMnuTable = "BannerMenu";
        private string cssClassMnuItem = "BannerMenuItem";
        private string cssClassMnuItemSelected = "BannerMenuItemSelected";
        private string cssClassMnuItemDisabled = "BannerMenuItemDisabled";

		// styles for the Menu, and unselected & selected menu items...
		private readonly TableItemStyle unselectedMenuItemStyle = new TableItemStyle();
		private readonly TableItemStyle selectedMenuItemStyle   = new TableItemStyle();
		private readonly TableItemStyle disabledMenuItemStyle   = new TableItemStyle();
		private readonly TableStyle menuMasterStyle             = new TableStyle();
		private readonly TableStyle menuStyle                   = new TableStyle();

		private readonly ArrayList subItemsIds    = new ArrayList();		     // the list of submenu ids
		private object dataSource;//        = null;					 // the menu's datasource - used for databinding
		private MenuItemCollection items = new MenuItemCollection(); // the top-level menu		

		#endregion Members

		#region Accessors
		#region DataSource
		/// <summary>
		/// Sets or gets the name of the XML file that is the datasource for the menu.
		/// </summary>
		public object DataSource
		{
			get
			{
				return this.dataSource;
			}
			set
			{
				if (value is string || value is XmlDocument)
					this.dataSource = value;
				else
					throw new ArgumentException("DataSource must be a string or XmlDocument instance.");
			}
		}
		#endregion DataSource
		#region Items
		/// <summary>
		/// Returns the menu items for the menu.
		/// </summary>
		[
		Browsable(false)
		]
		public MenuItemCollection Items
		{
			get
			{				
				if (this.IsTrackingViewState)
					((IStateManager) items).TrackViewState();

				return this.items;
			}
		}
		#endregion Items
		#region Layout
		/// <summary>
		/// Sets or Gets the menu's layout direction.
		/// </summary>
		[
		Category("Appearance"),
		Description("Specifies the menu layout direction.")
		]
		public MenuLayout Layout
		{
			get
			{
				object o = ViewState["MenuLayout"];
				if (o == null)
					return MenuLayout.Vertical;
				else
					return (MenuLayout) o;
			}
			set
			{
				ViewState["MenuLayout"] = value;
			}
		}
		#endregion Layout
		#region LayoutDOWN
		[
		Category("Appearance"),
		Description("Specifies the menu layout direction.")
		]
		public MenuLayoutDOWN LayoutDOWN
		{
			get
			{
				object o = ViewState["MenuLayoutDOWN"];
				if (o == null)
					return MenuLayoutDOWN.DOWN;
				else
					return (MenuLayoutDOWN) o;
			}
			set
			{
				ViewState["MenuLayoutDOWN"] = value;
			}
		}
		#endregion LayoutDOWN
		#region MenuMasterStyle
		[
		Category("Appearance"),
		PersistenceMode(PersistenceMode.InnerProperty),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		Description("Specifies the style for the master menus.")
		]
		public TableStyle MenuMasterStyle
		{
			get
			{
				if (this.IsTrackingViewState)
					((IStateManager) this.menuMasterStyle).TrackViewState();

				return this.menuMasterStyle;
			}
		}

		#endregion MenuMasterStyle
		#region MnuNum
		public int MnuNum
		{
			get{return mnuNum;}
			set{mnuNum = value;}
		}
		#endregion MnuNum
		#region MenuStyle
		[ 
		Category("Appearance"),
		PersistenceMode(PersistenceMode.InnerProperty),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		Description("Specifies the style for the menus.")
		]
		public TableStyle MenuStyle
		{
			get
			{
				if (this.IsTrackingViewState)
					((IStateManager) this.menuStyle).TrackViewState();

				return this.menuStyle;
			}
		}
		#endregion MenuStyle
		#region NamingContainerID
		public string NamingContainerID
		{
			get 
			{
                //20071212 FI Ticket 16012 => Migration Asp2.0 Add ("__Page" != this.NamingContainer.ClientID)
                if ((null != NamingContainer) && (null != NamingContainer.ClientID) && ("__Page" != this.NamingContainer.ClientID))
					return NamingContainer.ClientID + "_";
				else
					return string.Empty;
			}
		}
		#endregion NamingContainerID
		#region SelectedMenuItemStyle	
		[ 
		Category("Appearance"),
		PersistenceMode(PersistenceMode.InnerProperty),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		Description("Specifies the style for the selected menu item.")
		]
		public TableItemStyle SelectedMenuItemStyle
		{
			get
			{
				if (this.IsTrackingViewState)
					((IStateManager) this.selectedMenuItemStyle).TrackViewState();

				return this.selectedMenuItemStyle;
			}
		}
		#endregion SelectedMenuItemStyle	
		#region DisabledMenuItemStyle	
		[ 
		Category("Appearance"),
		PersistenceMode(PersistenceMode.InnerProperty),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		Description("Specifies the style for the disabled menu item.")
		]
		public TableItemStyle DisabledMenuItemStyle
		{
			get
			{
				if (this.IsTrackingViewState)
					((IStateManager) this.disabledMenuItemStyle).TrackViewState();

				return this.disabledMenuItemStyle;
			}
		}
		#endregion DisabledMenuItemStyle	
		#region UnselectedMenuItemStyle	
		[ 
		Category("Appearance"),
		PersistenceMode(PersistenceMode.InnerProperty),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		Description("Specifies the style for the unselected menu items.")
		]
		public TableItemStyle UnselectedMenuItemStyle
		{
			get
			{
				if (this.IsTrackingViewState)
					((IStateManager) this.unselectedMenuItemStyle).TrackViewState();

				return this.unselectedMenuItemStyle;
			}
		}

		#endregion UnselectedMenuItemStyle
		#endregion Accessors
			
		#region Constructors
		public Menu()
		{
			MnuNum = 0;
			SetStyles(null,null,null,null,null);
		}
		public Menu(int pMnuNum)
		{
			MnuNum = pMnuNum;
			SetStyles(null,null,null,null,null);
		}
		public Menu(int pMnuNum, string pCssClassMasterMenu, string pCssClassMenu,string pCssClassItemMenu,string pCssClassItemSelectedMenu,string pCssClassItemDisabledMenu)
		{
			MnuNum = pMnuNum;
			SetStyles(pCssClassMasterMenu,pCssClassMenu,pCssClassItemMenu,pCssClassItemSelectedMenu,pCssClassItemDisabledMenu);
		}
		public Menu(int pMnuNum, string pID, string pCssClassMasterMenu,string pCssClassMenu,string pCssClassItemMenu,string pCssClassItemSelectedMenu,string pCssClassItemDisabledMenu)
		{
			this.ID = pID;
			MnuNum = pMnuNum;
			SetStyles(pCssClassMasterMenu,pCssClassMenu,pCssClassItemMenu,pCssClassItemSelectedMenu,pCssClassItemDisabledMenu);
		}
		#endregion Constructors

		
		#region protected virtual OnMenuItemClick
		protected virtual void OnMenuItemClick(MenuItemClickEventArgs e)
		{
            MenuItemClick?.Invoke(this, e);
        }
		#endregion
		#region protected override OnDataBinding
		/// <summary>
		/// This method runs when the DataBind() method is called.  Essentially, it clears out the
		/// current state and builds up the menu from the specified DataSource.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDataBinding(EventArgs e)
		{
			// Start by resetting the Control's state
			this.Controls.Clear();			
			if (HasChildViewState)
				ClearChildViewState();

			// load the datasource either as a string or XmlDocuemnt
			XmlDocument xmlDoc = new XmlDocument();

			if (this.DataSource is String)
				// Load the XML document specified by DataSource as a filepath			
				xmlDoc.Load((string) this.DataSource);
			else if (this.DataSource is XmlDocument)
				xmlDoc = (XmlDocument) DataSource;
			else
				throw new ArgumentException("DataSource either null or not of the correct type.");

			// Clear out the MenuItems and build them according to the XmlDocument
			this.items.Clear();
			this.items = GatherMenuItems(xmlDoc.SelectSingleNode("/menu"), this.ClientID);
			BuildMenu();

			this.ChildControlsCreated = true;

			if (!IsTrackingViewState)
				TrackViewState();
		}
		#endregion OnDataBinding
		#region protected override OnPreRender
		/// <summary>
		/// Prerender generates the client-side JavaScript.  Note that a script block is created for
		/// each skmMenu on a single ASP.NET Web page.
		/// 
		/// (As of 9/11/2003, the JavaScript has yet to be fine tuned to allow for multiple skmMenus to
		/// work with closing submenus...)
		/// 
		/// For more information on adding client-side script via an ASP.NET server control, refer to:
		/// 
		///		Injecting Client-Side Script from an ASP.NET Server Control
		///		http://msdn.microsoft.com/asp.net/default.aspx?pull=/library/en-us/dnaspp/html/aspnet-injectclientsidesc.asp
		/// </summary>
		/// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            #region Menu Javascript registration
            if (StrFunc.IsFilled(this.ClientID))
            {
				string _mnuNum = this.mnuNum.ToString();

				JavaScript.RegisterManifestResource(this.Page, "skmMenuDeclareVariables.js", "SkmMenuDeclareVariables", false);

				string script = string.Empty;
				for (int i = 0; i < this.subItemsIds.Count; i++)
                    script += "subMenuIDs[" + _mnuNum + "][" + i.ToString() + "] = '" + this.subItemsIds[i].ToString() + "';\n";

                JavaScript.RegisterManifestResource(this.Page, "skmMenuVariables.js", "SkmMenu-" + this.ClientID, true,
                    "mnuNum", _mnuNum,
                    "ClientID", this.ClientID,
                    "cssClassMnuItem", this.cssClassMnuItem,
                    "cssClassMnuItemSelected", this.cssClassMnuItemSelected,
                    "cssClassMnuItemDisabled", this.cssClassMnuItemDisabled,
                    "subItemsIds", this.subItemsIds.Count.ToString(),
                    "subItemsIdsChilds", script);

				//FI 20100803 => RegisterStartUp à false
				//Ainsi les fonctions sont présentes en début de page.
				//Il n'y a plus d'erreur Java (du type fonction not defined)
				JavaScript.RegisterManifestResource(this.Page, "skmMenu.js", "SkmMenuFunctions", false);
			}
            #endregion Menu Javascript registration
        }
		#endregion OnPreRender
		#region protected override Render
		/// <summary>
		/// The Render method is responsible for generating the HTML markup.
		/// Here, we check to ensure that the menu control is placed inside a Web Form,
		/// as it must be for a menu item to be able to have a postback occur when clicked.
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm(this);
			base.Render (writer);
		}
		#endregion Render
		#region protected override CreateChildControls
		/// <summary>
		/// This method is called from base.Render(), and starts the build menu process.
		/// </summary>
		protected override void CreateChildControls()
		{
			BuildMenu();
		}
		#endregion CreateChildControls
		#region protected override TrackViewState
		/// <summary>
		/// TrackViewState informs all of the menus complex properties that they, too, need to
		/// track their viewstate changes.
		/// </summary>
		protected override void TrackViewState()
		{
			base.TrackViewState ();
			
			if (this.items != null)
				((IStateManager) items).TrackViewState();
			
			if (this.selectedMenuItemStyle != null)
				((IStateManager) this.selectedMenuItemStyle).TrackViewState();

			if (this.unselectedMenuItemStyle != null)
				((IStateManager) this.unselectedMenuItemStyle).TrackViewState();

			if (this.disabledMenuItemStyle != null)
				((IStateManager) this.disabledMenuItemStyle).TrackViewState();

			if (this.menuStyle != null)
				((IStateManager) this.menuStyle).TrackViewState();

			if (this.menuMasterStyle != null)
				((IStateManager) this.menuMasterStyle).TrackViewState();
		}
		#endregion TrackViewState
		#region protected override LoadViewState
		/// <summary>
		/// Loads the state from the passed in saveState object.  This method runs during the
		/// page life-cycle, and is required for the menu to work across postbacks.
		/// </summary>
		/// <param name="savedState">The state persisted by SaveViewState() in the previous life-cycle.</param>
		protected override void LoadViewState(object savedState)
		{
            if (savedState != null)
			{
                object[] state = (object[])savedState;

                base.LoadViewState(state[0]);
				((IStateManager) this.selectedMenuItemStyle).LoadViewState(state[1]);
				((IStateManager) this.unselectedMenuItemStyle).LoadViewState(state[2]);
				((IStateManager) this.disabledMenuItemStyle).LoadViewState(state[3]);
				((IStateManager) this.menuMasterStyle).LoadViewState(state[4]);
				((IStateManager) this.menuStyle).LoadViewState(state[5]);
				((IStateManager) this.items).LoadViewState(state[6]);
			}
		}
		#endregion LoadViewState
		#region protected override SaveViewState
		/// <summary>
		/// SaveViewState saves the state of the menu into an object (specifically, an object array
		/// with 5 indices).  This is required to have the state persisted across postbacks.
		/// </summary>
		/// <returns>A five-element object array representing the menu's state.</returns>
		protected override object SaveViewState()
		{
			// Create a Triplet to store the base view state and the two styles' view states
			Object [] state = new object[7];
			state[0] = base.SaveViewState();
			state[1] = ((IStateManager) this.selectedMenuItemStyle).SaveViewState();
			state[2] = ((IStateManager) this.unselectedMenuItemStyle).SaveViewState();
			state[3] = ((IStateManager) this.disabledMenuItemStyle).SaveViewState();
			state[4] = ((IStateManager) this.menuMasterStyle).SaveViewState();
			state[5] = ((IStateManager) this.menuStyle).SaveViewState();
			state[6] = ((IStateManager) this.items).SaveViewState();

			return state;
		}
		#endregion SaveViewState

        #region public SetStyles
        public void SetStyles(string pCssClassMnuMasterTable,string pCssClassMnuTable , 
			string pCssClassMnuItem, string pCssClassMnuItemSelected, string pCssClassMnuItemDisabled)
		{
			cssClassMnuMasterTable  = (StrFunc.IsFilled(pCssClassMnuMasterTable)? pCssClassMnuMasterTable : cssClassMnuMasterTable) ;
			cssClassMnuTable        = (StrFunc.IsFilled(pCssClassMnuTable)? pCssClassMnuTable : cssClassMnuTable) ;
			cssClassMnuItem         = (StrFunc.IsFilled(pCssClassMnuItem)? pCssClassMnuItem : cssClassMnuItem) ;
			cssClassMnuItemSelected = (StrFunc.IsFilled(pCssClassMnuItemSelected)? pCssClassMnuItemSelected : cssClassMnuItemSelected) ;
			cssClassMnuItemDisabled = (StrFunc.IsFilled(pCssClassMnuItemSelected)? pCssClassMnuItemDisabled : cssClassMnuItemDisabled) ;
			//
			menuMasterStyle.CssClass         = cssClassMnuMasterTable;
			menuStyle.CssClass               = cssClassMnuTable;
			unselectedMenuItemStyle.CssClass = cssClassMnuItem;
			selectedMenuItemStyle.CssClass   = cssClassMnuItemSelected;
			disabledMenuItemStyle.CssClass   = cssClassMnuItemDisabled;

		}
		#endregion SetStyles

		#region RaisePostBackEvent
		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) 
		{
			MenuItemClickEventArgs itemClickEventArgs = null;
			//20071114 FI Passage de argument voir AddTableCellEvent
			char[] splitChar = new char[] {';'};
			String[] aEventArg = eventArgument.Split(splitChar);
			//
			if (ArrFunc.IsFilled(aEventArg))
			{
				if (aEventArg.Length == 2)
				{
					itemClickEventArgs = new MenuItemClickEventArgs(aEventArg[0],aEventArg[1]);
				}
				else
				{
					itemClickEventArgs = new MenuItemClickEventArgs(aEventArg[0]);
				}
			}
			//
			if (null != itemClickEventArgs)
				OnMenuItemClick(itemClickEventArgs);
		}
		#endregion RaisePostBackEvent

		#region private AddMenu
		/// <summary>
		/// AddMenu is called recusively, doing a depth-first traversal of the menu hierarchy and building
		/// up the HTML elements from the object model.
		/// </summary>
		/// <param name="menuID">The ID of the parent menu.</param>
		/// <param name="myItems">The collection of menu items.</param>
		// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		private void AddMenu(string pMenuID, MenuItemCollection pMyItems)
		{
            // iterate through the Items
            Table mnuTable = new Table
            {
                ID = pMenuID,
                CellPadding = 0,
                CellSpacing = 0,
                CssClass = this.cssClassMnuTable
            };
            mnuTable.Attributes.Add("style", "display:none;");

			// Iterate through the menuItem's subMenu...
			for (int i = 0; i < pMyItems.Count; i++)
			{
				MenuItem mi  = pMyItems[i];
				TableRow tr  = new TableRow();
				if (mi.IsEnabled)
					tr.CssClass  = this.cssClassMnuItem;
				else
					tr.CssClass  = this.cssClassMnuItemDisabled;

                #region Image
                TableCell td = new TableCell
                {
                    Height = Unit.Pixel(18)
                };
                if (StrFunc.IsFilled(mi.ImageUrl))
				{
                    WCToolTipPanel pnl = new WCToolTipPanel();
                    if (mi.ImageUrl.Contains("fa-mnu") || mi.ImageUrl.Contains("fa-icon"))
                        pnl.CssClass = mi.ImageUrl;
                    else
                        pnl.CssClass = CSS.SetCssClass(mi.ImageUrl);
                    pnl.Enabled = mi.IsEnabled;
                    if (StrFunc.IsFilled(mi.ToolTip))
                        pnl.Pty.TooltipContent = mi.ToolTip;
                    if (StrFunc.IsFilled(mi.Text))
                        pnl.Pty.TooltipTitle = mi.Text;
                    td.Controls.Add(pnl);
				}
				else
					td.Text = Cst.HTMLSpace;

				AddTableCellEvent(td,mi,NamingContainerID + pMenuID,"img");
				tr.Cells.Add(td);
				#endregion Image
				#region Text

				if (StrFunc.IsFilled(mi.Text))
				{
					WCToolTipCell ttcell = new WCToolTipCell();
					ttcell.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
					ttcell.Height = Unit.Pixel(18);

					if (StrFunc.IsFilled(mi.ToolTip))
					{
                        WCTooltipLabel lbl = new WCTooltipLabel
                        {
                            Text = mi.Text
                        };
                        lbl.Pty.TooltipContent = mi.ToolTip;
						lbl.Pty.TooltipTitle = mi.Text;
						ttcell.Wrap = false;
						ttcell.Controls.Add(lbl);
					}
					else
					{
						ttcell.Text = (StrFunc.IsEmpty(mi.Text) ? Cst.HTMLSpace : Cst.HTMLSpace + mi.Text + Cst.HTMLSpace);
						ttcell.Wrap = false;
						ttcell.Pty.TooltipContent = mi.ToolTip;
					}

					AddTableCellEvent(ttcell, mi, NamingContainerID + pMenuID, "td");
					tr.Cells.Add(ttcell);
				}

                #endregion Text
                #region DropDown image (if subitems)
                td = new TableCell
                {
                    HorizontalAlign = HorizontalAlign.Right,
                    Height = Unit.Pixel(18)
                };
                LinkButton btn = new LinkButton
                {
                    CausesValidation = false
                };
                if (mi.SubItems.Count>0)
				{
					btn.CssClass = "fa-icon";
					btn.Text = @"<i class='fas fa-angle-right'></i>";
				}
                td.Controls.Add(btn);

				AddTableCellEvent(td,mi,NamingContainerID + pMenuID,"ddimg");
				tr.Cells.Add(td);
				#endregion DropDown image (if subitems)
				mnuTable.Rows.Add(tr);

				#region (Recursively) Add the subitems for this menu, if needed
				if (mi.SubItems.Count > 0)
				{
					this.subItemsIds.Add(NamingContainerID + mi.ID + subMenuKey);
					AddMenu(mi.ID + subMenuKey, mi.SubItems);
				}
				#endregion (Recursively) Add the subitems for this menu, if needed
			}

			Controls.Add(mnuTable);
		}

		#endregion AddMenu
        #region private AddTableCellEvent
        //EG 20120613 BlockUI New
        private void AddTableCellEvent(TableCell pTableCell, MenuItem pMenuItem, string pMenuID,string pPrefix)
        {
            pTableCell.Attributes.Add("id",pPrefix + "-X-" + NamingContainerID + pMenuItem.ID);
            if (StrFunc.IsFilled(pMenuItem.Url))
            {
                if (URLAlternativeActionParser.IsAlternativeActionToPerform(pMenuItem.Url, out string userAction, out URLAlternativeActionParser.ScriptToBePerformed script))
                {
                    AddTableCellAlternativeEvent(pTableCell, pMenuItem.Argument, pMenuItem.BlockUIMessage, userAction, script);
                }
                else if (StrFunc.ContainsIn(pMenuItem.Url, "javascript:"))
                    pTableCell.Attributes.Add("onclick", pMenuItem.Url);
                else if ((pMenuItem.Url.Length > 12) && (pMenuItem.Url.Substring(0, 12) == "window.open("))
                    pTableCell.Attributes.Add("onclick", pMenuItem.Url);
                else if (pMenuItem.Url.Substring(0, 4) == "Open")
                    pTableCell.Attributes.Add("onclick", pMenuItem.Url);
                else
                    pTableCell.Attributes.Add("onclick", "javascript:location.href='" + pMenuItem.Url + "'");
            }
            else if (StrFunc.IsFilled(pMenuItem.CommandName))
            {
                Regex regex = new Regex(pPrefix + "-X-" + "tblMenu_tblMenu_mnuScreen-m00-sm-m0\\d");
                if (regex.IsMatch(pPrefix + "-X-" + NamingContainerID + pMenuItem.ID))
                    pTableCell.Attributes.Add("onclick", "OnSubmit('" + this.UniqueID.Replace(":", "$") + "','" + pMenuItem.CommandName + "')");
                else
                {
                    //20071114 FI Passage de argument voir RaisePostBackEvent
                    string arg = pMenuItem.CommandName;
                    if (StrFunc.IsFilled(pMenuItem.Argument))
                        arg = arg + ";" + pMenuItem.Argument;

                    AddOnClickWithPostBackClient(pTableCell, arg, pMenuItem.BlockUIMessage);
                }
            }
            //
            pTableCell.Attributes.Add("onmouseover", "javascript:MousedOverMenu(this, document.getElementById('" + pMenuID + "')," +
                (pMenuItem.Layout == MenuLayout.Vertical.ToString() ? "true" : "false") + " , " +
                (LayoutDOWN == MenuLayoutDOWN.DOWN ? "true" : "false") + " , " + this.mnuNum + " , " + pMenuItem.Enabled + ");");
            //
            pTableCell.Attributes.Add("onmouseout", "javascript:MousedOutMenu(this, " + this.mnuNum + " , " + pMenuItem.Enabled + ");");
        }

        /// <summary>
        /// Register the alternative action (alternative event) on the cell passed as input parameter
        /// </summary>
        /// <remarks>
        /// Add new cases in case of need
        /// </remarks>
        /// <param name="pTableCell">Cell where we register the alternative actiont</param>
        /// <param name="pArgument">Arguments of the alternative action</param>
        /// <param name="userAction">Script type of the alternative action</param>
        /// <param name="script"></param>
        //EG 20120613 BlockUI New
        private void AddTableCellAlternativeEvent(TableCell pTableCell, string pArgument, string pBlockUIMessage, string userAction, 
            URLAlternativeActionParser.ScriptToBePerformed script)
        {
            switch (script)
            {
                case URLAlternativeActionParser.ScriptToBePerformed.PostBackClient:
                    AddOnClickWithPostBackClient(pTableCell, String.Concat(userAction, ";", pArgument), pBlockUIMessage);
                    break;

                case URLAlternativeActionParser.ScriptToBePerformed.Default:
                default:
                    break;
            }
        }
        //EG 20120613 BlockUI New
        private void AddOnClickWithPostBackClient(TableCell pTableCell, string arg, string pBlockUIMessage)
        {
            string onclick = string.Empty;
            if (StrFunc.IsFilled(pBlockUIMessage))
                onclick = "Block(" + JavaScript.HTMLBlockUIMessage(this.Page, pBlockUIMessage) + ");";
            onclick += Page.ClientScript.GetPostBackClientHyperlink(this, arg);
            pTableCell.Attributes.Add("onclick", onclick);
        }

		#endregion AddTableCellEvent
		#region private BuildMenu
		/// <summary>
		/// BuildMenu builds the top-level menu.  It is called from the OnDataBinding method as well
		/// as from CreateChildControls().  It has code to check if the top-level menu should be
		/// laid out horizontally or vertically.
		/// </summary>
		// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
		private void BuildMenu()
        {
            // iterate through the Items
            Table mnuTable = new Table
            {
                CellPadding = 0,
                CellSpacing = 0,
                CssClass = this.cssClassMnuMasterTable,
                BorderColor = Color.Transparent
            };
            mnuTable.Attributes.Add("id", this.ClientID);
            TableRow trMaster = new TableRow();

            Table menuItem = null;
            // Iterate through the top-level menu's menu items, and add a <td> tag for each menuItem
            for (int i = 0; i < this.items.Count; i++)
            {
                MenuItem mi = this.items[i];
                TableRow tr = new TableRow();
                if (mi.IsEnabled)
                    tr.CssClass = this.cssClassMnuItem;
                else
                    tr.CssClass = this.cssClassMnuItemDisabled;

                TableCell td;
                #region Image
                if (StrFunc.IsFilled(mi.ImageUrl))
                {
                    td = new TableCell();
                    WCToolTipPanel pnl = new WCToolTipPanel();
                    if (mi.ImageUrl.Contains("fa-mnu") || mi.ImageUrl.Contains("fa-icon"))
                        pnl.CssClass = mi.ImageUrl;
                    else
                        pnl.CssClass = CSS.SetCssClass(mi.ImageUrl);
                    pnl.Enabled = mi.IsEnabled;
                    if (StrFunc.IsFilled(mi.ToolTip))
                        pnl.Pty.TooltipContent = mi.ToolTip;
                    if (StrFunc.IsFilled(mi.Text))
                        pnl.Pty.TooltipTitle = mi.Text;
                    td.Controls.Add(pnl);

                    AddTableCellEvent(td, mi, this.ClientID, "img");
                    tr.Cells.Add(td);

                }
                #endregion Image
                #region Text
                if (StrFunc.IsFilled(mi.Text))
				{
                    WCToolTipCell ttcell = new WCToolTipCell
                    {
                        Height = Unit.Pixel(18),
                        Text = (StrFunc.IsEmpty(mi.Text) ? Cst.HTMLSpace : Cst.HTMLSpace + mi.Text + Cst.HTMLSpace)
                    };
                    ttcell.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
					ttcell.Wrap = false;
					ttcell.Pty.TooltipContent = mi.ToolTip;

					AddTableCellEvent(ttcell, mi, this.ClientID, "td");
					tr.Cells.Add(ttcell);
				}
                #endregion Text
                #region DropDown image (if subitems)
                if (0 < mi.SubItems.Count)
                {
                    td = new TableCell
                    {
                        HorizontalAlign = HorizontalAlign.Left,
                        Height = Unit.Pixel(18)
                    };
                    LinkButton btn = new LinkButton
                    {
                        CausesValidation = false
                    };
                    if (mi.SubItems.Count > 0)
					{
						btn.CssClass = "fa-icon";
						btn.Text = @"<i class='fas fa-angle-down'></i>";
					}
                    td.Controls.Add(btn);
                    AddTableCellEvent(td, mi, this.ClientID, "ddimg");
                    tr.Cells.Add(td);
                }
                #endregion DropDown image (if subitems)

                if (mi.Layout == MenuLayout.Vertical.ToString())
                {
                    mnuTable.Rows.Add(tr);
                }
                else
                {
                    menuItem = new Table
                    {
                        ID = mi.ID,
                        CellPadding = 0,
                        CellSpacing = 0
                    };
                    menuItem.Rows.Add(tr);
                }

                #region Add the subitems for this menu, if needed
                if (0 < mi.SubItems.Count)
                {
                    this.subItemsIds.Add(NamingContainerID + mi.ID + subMenuKey);
                    AddMenu(mi.ID + subMenuKey, mi.SubItems);
                }
                #endregion Add the subitems for this menu, if needed
                if (mi.Layout == MenuLayout.Horizontal.ToString())
                {
                    td = new TableCell
                    {
                        Height = Unit.Pixel(18)
                    };
                    td.Controls.Add(menuItem);
                    if (mi.IsEnabled)
                        trMaster.CssClass = this.cssClassMnuItem;
                    else
                        trMaster.CssClass = this.cssClassMnuItemDisabled;
                    trMaster.Cells.Add(td);
                }
            }

            if (Layout == MenuLayout.Horizontal)
                mnuTable.Rows.Add(trMaster);

            Controls.Add(mnuTable);
        }
		#endregion BuildMenu
		#region private BuildMenuItem
		/// <summary>
		/// This method creates a single MenuItem and is called repeatedly from GatherMenuItems().
		/// </summary>
		/// <param name="menuItem">The menuItem XmlNode.</param>
		/// <param name="parentID">The parent menuItem's ID.</param>
		/// <param name="indexValue">The ordinal index of the menuItem in the set of menuItems.</param>
		/// <returns></returns>
		//EG 20120613 BlockUI New
        private MenuItem BuildMenuItem(XmlNode pMenuItem, string pParentID, int pIndexValue)
		{
            MenuItem menuItem = new MenuItem
            {
                ToolTip = string.Empty
            };

            XmlNode textNode        = pMenuItem.SelectSingleNode("text/text()");
			XmlNode urlNode         = pMenuItem.SelectSingleNode("url/text()");
			XmlNode imageUrlNode    = pMenuItem.SelectSingleNode("imageUrl/text()");
			XmlNode commandNameNode = pMenuItem.SelectSingleNode("commandName/text()");
			XmlNode argumentNode    = pMenuItem.SelectSingleNode("argument/text()");
			XmlNode layoutNode      = pMenuItem.SelectSingleNode("layout/text()");
            XmlNode blockUIMessageNode = pMenuItem.SelectSingleNode("blockUIMessage/text()");
			
			// Format the indexValue so its three-digits (allows for 1,00 menu items per (sub)menu
			if (0 < pMenuItem.Attributes.Count)
			{
				XmlAttribute attr = (XmlAttribute) pMenuItem.Attributes.GetNamedItem("ID");
				if (attr!=null && attr.Specified)
					menuItem.ID = attr.Value;
				else
					menuItem.ID = pParentID + "-m" + pIndexValue.ToString("d2");
				attr = (XmlAttribute) pMenuItem.Attributes.GetNamedItem("ToolTip");
				if (attr!=null && attr.Specified)
					menuItem.ToolTip = attr.Value;
				attr = (XmlAttribute) pMenuItem.Attributes.GetNamedItem("Enabled");
				if (attr!=null && attr.Specified)
					menuItem.Enabled = attr.Value;
			}
			else
				menuItem.ID = pParentID + "-m" + pIndexValue.ToString("d2");
			
			if (textNode == null)
			{
				string sError = "The XML data for the Menu control is in an invalid format: missing the <text> element for a menuItem>.";
				throw new ArgumentException(sError); 
			}
			
			menuItem.Text = textNode.Value;
			
			if (urlNode != null)
				menuItem.Url = urlNode.Value;

			if (imageUrlNode != null)
				menuItem.ImageUrl = imageUrlNode.Value;

			if (commandNameNode != null)
				menuItem.CommandName = commandNameNode.Value;

			if (argumentNode != null)
				menuItem.Argument = argumentNode.Value;

			if (layoutNode != null)
				menuItem.Layout = layoutNode.Value;

            if (blockUIMessageNode != null)
                menuItem.BlockUIMessage = blockUIMessageNode.Value;

			// see if there is a submenu
			XmlNode subMenu = pMenuItem.SelectSingleNode("subMenu");
			if (subMenu != null)
			{
				// Recursively processes the <menuItem>'s <subMenu> node, if present
				menuItem.SubItems.AddRange(GatherMenuItems(subMenu, menuItem.ID + subMenuKey));
			}
			return menuItem;
		}
		#endregion BuildMenuItem
		#region private GatherMenuItems
		/// <summary>
		/// This method is used from the OnDataBinding method; it traverses the XML document,
		/// building up the object model.
		/// </summary>
		/// <param name="itemsNode">The current menuItem XmlNode</param>
		/// <param name="parentID">The ID of the parent menuItem XmlNode</param>
		/// <returns>A set of MenuItems for this menu.</returns>
		private MenuItemCollection GatherMenuItems(XmlNode itemsNode, string parentID)
		{
			// Make sure we have an XmlNode instance - it should never be null, else the
			// XML document does not have the expected structure
			if (itemsNode == null)
				throw new ArgumentException("The XML data for the Menu control is in an invalid format.");

			MenuItemCollection myItems = new MenuItemCollection();
			if (IsTrackingViewState)
				((IStateManager) myItems).TrackViewState();
				
			// iterate through each MenuItem
			XmlNodeList menuItems = itemsNode.SelectNodes("menuItem");
			for (int i = 0; i < menuItems.Count; i++)
			{
				XmlNode menuItem = menuItems[i];
				
				// Create the menu item
				myItems.Add(BuildMenuItem(menuItem, parentID, i));
			}

			return myItems;
		}
		#endregion GatherMenuItems

    }
	#endregion public class Menu

    #region UrlAlternativeAction

    /// <summary>
    /// Helper class to parse and evaluate an alternative action property (identified by the tag "AlternativeAction") 
    /// stocked inside of the Spheres MENU.URL column. 
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <listheader>An alternative action property is identified by:</listheader>
    /// <item>
    /// <term>UserAction</term>
    /// <description>The action demanded by the user to be executed</description>
    /// </item>
    /// <item>
    /// <term>ScriptToBePerformed</term>
    /// <description>The alternative action to perform when at least one of the conditions is satisfied</description>
    /// </item> 
    /// <item>
    /// <term>DependsOn</term>
    /// <description>The list of the conditions, at least one of the listed conditions must be true to perform the alternative action</description>
    /// </item>
    /// <item>
    /// <term>ParametersToEvaluate</term>
    /// <description>Supplementary parameters</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <listheader>MENU.URL samples</listheader>
    /// <item>
    /// <term>Redirect to a Spheres page</term>
    /// <description>Unavailable.aspx</description>
    /// </item>
    /// <item>
    /// <term>Javascript code call (when GUID is specified)</term>
    /// <description>OpenTradeAction('GUID','CalculationAgentSettlementRateEvents'); return false;</description>
    /// </item>
    /// <item>
    /// <term>Redirect to a Spheres with parameters</term>
    /// <description>List.aspx?Referential=MODELDATA&InputMode=2&P1=MODELPERMISSION</description>
    /// </item>
    /// <item>
    /// <term>Client postback, passing a Spheres command</term>
    /// <description>RemoveReplace</description>
    /// </item>
    /// <item>
    /// <term>Javascript code call, including an alternative action</term>
    /// <description>OpenTradeAction('GUID','ExerciseEvents','AlternativeAction=>UserAction{ExerciseEvents};ScriptToBePerformed{PostBackClient};DependsOn{IsOption[VALUE];IsFuture[VALUE]};OptionalParameters{ProductType[VALUE];IsOption[true];}'); return false;</description>
    /// </item>
    /// </list>
    /// <br/>
    /// Alternative action format:
    /// <br/>
    /// <code>
    /// ALTERNATIVEACTION=>
    /// USERACTION{context_name}
    /// ;SCRIPTTOBEPERFORMED{script_name}
    /// ;DEPENDSON{[parameter1_name[VALUE|value][;parameter2_name[VALUE|value][...]]]]}
    /// ;OPTIONALPARAMETERS{[parameter1_name[VALUE|value][;parameter2_name[VALUE|value][...]]]]}
    /// [;]
    /// </code>
    /// <br/>
    /// Alternative action sample: 
    /// <br/>
    /// <code>
    /// AlternativeAction=>
    /// UserAction{ExerciseEvents};
    /// ScriptToBePerformed{PostBackClient};DependsOn{IsOption[VALUE];IsFuture[true]};
    /// OptionalParameters{ProductType[VALUE];IsOption[VALUE];}
    /// </code>
    /// <br/>
    /// That alternative action has been defined to distinguish between the exercise of an OTC trade and the exercise of an ETD trade.
    /// The script stocked in the ScriptToBePerformed field will be performed when at least one of the conditions in the DependsOn field is satisfied.
    /// In this case a client postback (IOW: a submit) will be performed instead of the default action 
    /// when the product of the trade is type of Option or Future.
    /// </example>
    public static class URLAlternativeActionParser
    {
        /// <summary>
        /// Possible alternative actions
        /// </summary>
        /// <remarks>
        /// this enumeration can be enriched in case of need
        /// </remarks>
        public enum ScriptToBePerformed
        {
            /// <summary>
            /// Execute the default behavior, IOW it performs the default action/command saved in the MENU.URL field
            /// </summary>
            /// <example>
            /// OpenTradeAction('GUID','ExerciseEvents','AlternativeAction=>UserAction{ExerciseEvents};ScriptToBePerformed{Default};') => OpenTradeAction('GUID','ExerciseEvents')
            /// </example>
            Default,
            /// <summary>
            /// Perform a PostBackClient driven by the parameters in the "DependsOn" conditions list.
            /// </summary>
            /// <example>
            /// OpenTradeAction('GUID','ExerciseEvents',
            /// 'AlternativeAction=>UserAction{ExerciseEvents};ScriptToBePerformed{PostBackClient};DependsOn{IsOption[VALUE];IsFuture[VALUE]};OptionalParameters{ProductType[VALUE];IsOption[VALUE];}'); 
            /// return false;.
            /// </example>
            PostBackClient,
        }

        /// <summary>
        /// Parameters names recognized inside of the DependsOn and the ParametersToEvaluate fields
        /// </summary>
        /// <remarks>
        /// this list can be enriched in case of need
        /// </remarks>
        public static readonly string[] Parameters = new string[] { "ProductType", "IsOption", "IsFuture", "TestConstant" };

        /// <summary>
        /// Get the pattern to parse the alternative action
        /// <list type="bullet">
        /// <listheader>Format</listheader>
        /// <item>
        /// <term>AlternativeAction=></term>
        /// <description>constant prefix identifying the existence of an laternative action</description>
        /// </item>
        /// <item>
        /// <term>UserAction{(\w+)}</term>
        /// <description>First capturing group, containing the demanded user action</description>
        /// </item> 
        /// <item>
        /// <term>ScriptToBePerformed{(ScriptToBePerformed){1}}</term>
        /// <description>Second capturing group, containing the alternative action script</description>
        /// </item>
        /// <item>
        /// <term>DependsOn{([{parameters list}]+\[[\w\.]+\];{0,1})*}</term>
        /// <description>Third capturing group, containing the parameters collection of the DependsOn field</description>
        /// </item>
        /// <item>
        /// <term>OptionalParameters{([{parameters list}]+\[[\w\.]+\];{0,1})*}</term>
        /// <description>Fourth capturing group, containing the parameters collection of the ParametersToEvaluate field</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <example>
        /// AlternativeAction=>UserAction{(\w+)}\s*;\s*ScriptToBePerformed{(PostBackClient|Default){1}}\s*;\s*DependsOn{([IsOption|IsFuture|ProductType]+\[[\w\.]+\];{0,1})*}\s*;\s*OptionalParameters{([IsOption|IsFuture|ProductType]+\[[\w\.]+\];{0,1})*}
        /// </example>
        public static readonly string ParsingPattern = GetParsingPattern();

        static string GetParsingPattern()
        {

            string pattern = @"AlternativeAction=>UserAction{{(\w+)}}\s*;\s*ScriptToBePerformed{{({0}){{1}}}}\s*;\s*DependsOn{{([{1}]+\[[\w\.]+\];{{0,1}})*}}\s*;\s*OptionalParameters{{([{1}]+\[[\w\.]+\];{{0,1}})*}}";

            string[] scripts = Enum.GetNames(typeof(ScriptToBePerformed));

            string scriptsAlternated = String.Empty;

            if (!ArrFunc.IsEmpty(scripts))
                foreach (string script in scripts)
                    scriptsAlternated = String.Concat(scriptsAlternated, script, "|");

            string parametersAlternated = String.Empty;

            if (!ArrFunc.IsEmpty(Parameters))
                foreach (string parameter in Parameters)
                    parametersAlternated = String.Concat(parametersAlternated, parameter, "|");

            return String.Format(pattern, scriptsAlternated, parametersAlternated);
      
        }

        /// <summary>
        /// Expression to parse a single rule parameter:
        /// 1. "({parameter name}){1}" => First capturing group, containing the rule parameter name; 
        /// 2. "(({parameter name}){1}\[VALUE\]){1}" => Second capturing group, containing the whole rule parameter (name+value pattern) 
        /// </summary>
        /// <example>
        /// ((IsOption|IsFuture|ProductType){1}\[VALUE\]){1};{0,1}
        /// </example>
        public static readonly string ParameterToEvaluateParsingPattern = GetParameterToEvaluateParsingPattern();

        static string GetParameterToEvaluateParsingPattern()
        {

            string pattern = @"(({0}){{1}}\[VALUE\]){{1}};{{0,1}}";

            string parametersAlternated = String.Empty;

            if (!ArrFunc.IsEmpty(Parameters))
                foreach (string parameter in Parameters)
                    parametersAlternated = String.Concat(parametersAlternated, parameter, "|");

            return String.Format(pattern, parametersAlternated);

        }

        /// <summary>
        /// Expression to parse a single rule parameter:
        /// 1. "({parameter name}){1}" => First capturing group, containing the rule parameter name; 
        /// 2. "(true|false)" => Second capturing group, containing the boolean parameter value
        /// </summary>
        /// <example>
        /// (IsOption|IsFuture|ProductType){1}\[(true|false)\];{0,1}
        /// </example>
        public static readonly string BooleanParameterParsingPattern = GetBooleanParameterParsingPattern();

        static string GetBooleanParameterParsingPattern()
        {

            string pattern = @"({0}){{1}}\[(true|false)\]{{1}};{{0,1}}";

            string parametersAlternated = String.Empty;

            if (!ArrFunc.IsEmpty(Parameters))
                foreach (string parameter in Parameters)
                    parametersAlternated = String.Concat(parametersAlternated, parameter, "|");

            return String.Format(pattern, parametersAlternated);

        }

        static readonly Regex _parsingExpression = new Regex(ParsingPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        static readonly Regex _parameterToEvaluateParsingExp = new Regex(ParameterToEvaluateParsingPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        static readonly Regex _booleanParameterParsingExp = new Regex(BooleanParameterParsingPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Parse the pUrl input parameter, and get the action type rule ("ActionType:") with its own parameters 
        /// </summary>
        /// <param name="poUrlActionTypeRule">the parsed url action type</param>
        /// <param name="poUrlActionTypeRuleParameters">the parsed url action type parameters</param>
        /// <returns>true if the URL contains one action type rule</returns>
        static public bool Parse(
            string pUrl,
            out string opUserAction, out ScriptToBePerformed opScriptToBePerformed,
            out string[] opDependsOnParameters, out string[] opOptionalParameters)
        {
            bool parsed = false;

            opUserAction = String.Empty;
            opScriptToBePerformed = default;
            opDependsOnParameters = null;
            opOptionalParameters = null;

            if (!String.IsNullOrEmpty(pUrl))
            {
                Match singleMatch = _parsingExpression.Match(pUrl);

                if (singleMatch.Success)
                {
                    parsed = true;
                    
                    opUserAction = singleMatch.Groups[1].Value;

                    opScriptToBePerformed = (ScriptToBePerformed)Enum.Parse(typeof(ScriptToBePerformed), singleMatch.Groups[2].Value);

                    switch (opScriptToBePerformed)
                    {
                        case ScriptToBePerformed.PostBackClient:
                            
                            if (singleMatch.Groups[3].Value != null && singleMatch.Groups[3].Captures.Count > 0)
                            {
                                opDependsOnParameters = new string[singleMatch.Groups[3].Captures.Count];

                                for (int idxCapture = 0; idxCapture < singleMatch.Groups[3].Captures.Count; idxCapture++)
                                    opDependsOnParameters[idxCapture] = singleMatch.Groups[3].Captures[idxCapture].Value;
                            }

                            if (singleMatch.Groups[4].Value != null && singleMatch.Groups[4].Captures.Count > 0)
                            {
                                opOptionalParameters = new string[singleMatch.Groups[4].Captures.Count];

                                for (int idxCapture = 0; idxCapture < singleMatch.Groups[4].Captures.Count; idxCapture++)
                                    opOptionalParameters[idxCapture] = singleMatch.Groups[4].Captures[idxCapture].Value;
                            }

                            break;

                        case ScriptToBePerformed.Default:
                        default:
                            // nothing to do...
                            break;
                    }
                }
            }

            return parsed;
        }

        /// <summary>
        /// Evaluate the pURL input parameter, 
        /// using the values provided by the pKeyValuesCollection input parameter
        /// </summary>
        /// <param name="pURL">the input URL</param>
        /// <param name="pDependsOnParameters">Parameters list</param>
        /// <param name="pParametersToEvaluate">Parameters list</param>
        /// <param name="pKeyValuesCollection">
        /// Collection containing all the parameter values
        /// <value>
        /// The first line of the array contains a subset of the Parameters array.
        /// The second line contains the relative values.
        /// </value>
        /// </param>
        /// <returns>The evaluated URL</returns>
        /// <remarks>The parameter evaluation of a Spheres URL is performed in the CreateChildsUserActionMenu method</remarks>
        public static string Evaluate(string pURL, string[] pParametersToEvaluate, string[][] pKeyValuesCollection)
        {
            string evaluatedUrl = pURL;

            if (ArrFunc.IsEmpty(pKeyValuesCollection) ||
                ArrFunc.IsEmpty(pKeyValuesCollection[0]) ||
                ArrFunc.IsEmpty(pKeyValuesCollection[1]) ||
                pKeyValuesCollection[0].Length != pKeyValuesCollection[1].Length)
                throw new ArgumentException(
                "The matrix is not well formed, the matrix is null, empty or the keys/values collections does not have the same length.",
                "pKeyValuesCollection");


            if (!ArrFunc.IsEmpty(pParametersToEvaluate))
                foreach (string parameter in pParametersToEvaluate)
                {
                    bool evaluated = EvaluateSingleParameter(parameter, pKeyValuesCollection, out string evaluatedParameter);
                    if (evaluated)
                        evaluatedUrl = evaluatedUrl.Replace(parameter, evaluatedParameter);
                }

            return evaluatedUrl;
        }

        /// <summary>
        /// Evaluate the pActionParameter value
        /// </summary>
        /// <param name="pURL"></param>
        /// <param name="pActionParameter"></param>
        /// <param name="pKeyValuesCollection"></param>
        /// <param name="opEvaluatedActionParameter"></param>
        /// <returns>the evaluated parameter</returns>
        private static bool EvaluateSingleParameter(string pActionParameter, string[][] pKeyValuesCollection, out string opEvaluatedActionParameter)
        {
            bool evaluated = false;

            string[] keys = pKeyValuesCollection[0];
            string[] values = pKeyValuesCollection[1];

            opEvaluatedActionParameter = String.Empty;

            Match match = _parameterToEvaluateParsingExp.Match(pActionParameter);

            if (match.Success)
            {
                //                throw new NotSupportedException(
                //                    String.Format(@"The ""{0}"" URL action parameter does not have the right pattern.
                //                                            Right pattern: ""{1}"".
                //                                            Source Url: ""{2}""",
                //                                    pActionParameter, ParameterToEvaluateParsingPattern, pURL));

                string parameterName = match.Groups[2].Value;

                for (int idxKey = 0; idxKey < keys.Length; idxKey++)
                    if (keys[idxKey] == parameterName)
                    {
                        opEvaluatedActionParameter = _parameterToEvaluateParsingExp.Replace(pActionParameter,
                            String.Format("$2[{0}];", values[idxKey]));

                        evaluated = true;
                    }

            }

            return evaluated;
        }

        public static bool IsAlternativeActionToPerform(string pUrl, out string opUserAction, out ScriptToBePerformed opScriptToBePerformed)
        {
            bool toPerform = false;
            _ = Parse(pUrl, out opUserAction, out opScriptToBePerformed, out string[] dependsOnParameters, out _);

            switch (opScriptToBePerformed)
            {
                case ScriptToBePerformed.PostBackClient:
                    if (!ArrFunc.IsEmpty(dependsOnParameters))
                        foreach (string parameter in dependsOnParameters)
                        {
                            Match match = _booleanParameterParsingExp.Match(parameter);

                            if (match.Success)
                                toPerform = Boolean.Parse(match.Groups[2].Value);

                            if (toPerform)
                                break;
                        }
                    else
                        toPerform = true;
                    break;

                case ScriptToBePerformed.Default:
                default:
                    break;
            }
            
            return toPerform;
        }
    }

    #endregion UrlAlternativeAction
}

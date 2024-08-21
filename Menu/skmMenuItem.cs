using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;

namespace skmMenu
{
	/// <summary>
	/// A MenuItem represents a single item in a menu.  Menu items have the following properties:
	///		Text - specifies the text to display for the menu item (can contain HTML, such as IMG tags)
	///		Url (optional) - if present, specifies the URL the user is taken to upon clicking the menu item
	///		CommandName (optional) - if Url is NOT specified and CommandName is, then the menu item,
	///			when clicked, causes the Web Form to postback and raises the Menu control's MenuItemClick event
	///		
	///		(Note: if NEITHER Url nor CommandName are specified, the menu item is not clickable.)
	///		
	///	MenuItems also have an ID property, but this should not be set manually; let the DataBind()
	///	method set ID for you.
	/// </summary>
	//EG 20120613 BlockUI New
    public class MenuItem  : IStateManager
	{
		#region Private Member Variables
		// private member variables
		private MenuItemCollection subItems = new MenuItemCollection();

		private string id          = string.Empty;
		private string text        = string.Empty;
		private string imageUrl    = string.Empty;
		private string url         = string.Empty;
		private string commandName = string.Empty;
		private string argument    = string.Empty;
		private string layout      = string.Empty;
		private string toolTip     = string.Empty;
		private string enabled     = string.Empty;
		private string hidden      = string.Empty;
        // EG 201200613 New
        private string blockUIMessage = string.Empty;

		private bool isTrackingViewState;
        private bool idChanged;
        private bool textChanged;
        private bool imageUrlChanged;
        private bool urlChanged;
        private bool commandNameChanged;
        private bool argumentChanged;
        private bool layoutChanged;
        private bool toolTipChanged;
        private bool enabledChanged;
        private bool hiddenChanged;
        // EG 201200613 New
        private bool blockUIMessageChanged;
        #endregion

		public MenuItem() {}		// empty, default constructor

        public MenuItem(string pItemID, string pItemText, string pItemUrl, string pItemImageUrl, string pItemLayout,
            string pItemToolTip, string pEnabled, string pHidden):this(pItemID, pItemText, pItemUrl, pItemImageUrl, pItemLayout,
            pItemToolTip, pEnabled, pHidden, string.Empty){}

        //EG 20120613 BlockUI New
		public MenuItem(string pItemID, string pItemText, string pItemUrl,string pItemImageUrl,string pItemLayout,
            string pItemToolTip,string pEnabled,string pHidden,string pBlockUIMessage)
		{
			ID       = pItemID;
			Text     = pItemText;
			Url      = pItemUrl;
			ImageUrl = pItemImageUrl;
			Layout   = pItemLayout;
			ToolTip  = pItemToolTip;
			Enabled  = pEnabled;
			Hidden   = pHidden;
            blockUIMessage = pBlockUIMessage;
		}


		#region IStateManager Implementation
        //EG 20120613 BlockUI New
		object IStateManager.SaveViewState()
		{
			bool isAllNulls = true;
			object [] state = new object[12];
			for (int i = 0; i < 12; i++)
				state[i] = null;

			if (this.textChanged)
			{
				state[0] = text;
				isAllNulls = false;
			}
			if (this.urlChanged)
			{
				state[1] = url;
				isAllNulls = false;
			}
			if (this.commandNameChanged)
			{
				state[2] = commandName;
				isAllNulls = false;
			}
			if (this.argumentChanged)
			{
				state[3] = argument;
				isAllNulls = false;
			}
			if (this.idChanged)
			{
				state[4] = id;
				isAllNulls = false;
			}

			if (this.subItems != null)
			{
				state[5] = ((IStateManager) subItems).SaveViewState();
				isAllNulls = false;
			}
			if (this.imageUrlChanged)
			{
				state[6] = imageUrl;
				isAllNulls = false;
			}

			if (this.layoutChanged)
			{
				state[7] = layout;
				isAllNulls = false;
			}

			if (this.toolTipChanged)
			{
				state[8] = toolTip;
				isAllNulls = false;
			}

			if (this.enabledChanged)
			{
				state[9] = enabled;
				isAllNulls = false;
			}

			if (this.hiddenChanged)
			{
				state[10] = hidden;
				isAllNulls = false;
			}

            if (this.blockUIMessageChanged)
            {
                state[11] = blockUIMessage;
                isAllNulls = false;
            }

			if (isAllNulls)
				return null;
			else
				return state;
		}
        //EG 20120613 BlockUI New
		void IStateManager.LoadViewState(object savedState)
		{
			if (savedState != null)
			{
				object [] state = (object[]) savedState;
				if (state[0] != null)
					this.text = (string) state[0];
				if (state[1] != null)
					this.url = (string) state[1];
				if (state[2] != null)
					this.commandName = (string) state[2];
				if (state[3] != null)
					this.argument = (string) state[3];
				if (state[4] != null)
					this.id = (string) state[4];
				if (state[5] != null)
				{
					if (subItems == null)
						subItems = new MenuItemCollection();
					((IStateManager) subItems).LoadViewState(state[5]);
				}
				if (state[6] != null)
					this.imageUrl = (string) state[6];
				if (state[7] != null)
					this.layout = (string) state[7];
				if (state[8] != null)
					this.toolTip = (string) state[8];
				if (state[9] != null)
					this.enabled = (string) state[9];
				if (state[10] != null)
					this.hidden = (string) state[10];
                if (state[11] != null)
                    this.blockUIMessage = (string)state[11];
            }
		}

		void IStateManager.TrackViewState()
		{
			isTrackingViewState = true;

			if (subItems != null)
				((IStateManager) subItems).TrackViewState();
		}
		#endregion


		#region MenuItem Properties
		/// <summary>
		/// Specifies if the MenuItem is tracking ViewState.  Required, since MenuItem
		/// implements the IStateManager interface.
		/// </summary>
		bool IStateManager.IsTrackingViewState
		{
			get 
			{
				return this.isTrackingViewState;
			}
		}


		/// <summary>
		/// Gets or sets the menu item's text content.
		/// </summary>
		public string Text
		{
			get {return text;}
			set
			{
				text = value;
				if (this.isTrackingViewState)
					this.textChanged = true;
			}
		}

		/// <summary>
		/// Gets or sets the menu item's Url
		/// </summary>
		public string Url
		{
			get {return url;}
			set
			{
				url = value;
				if (this.isTrackingViewState)
					this.urlChanged = true;
			}
		}

		/// <summary>
		/// Retrieves the menu item's set of subitems.
		/// </summary>
		public MenuItemCollection SubItems
		{
			get
			{
				if (this.isTrackingViewState)
					((IStateManager) subItems).TrackViewState();

				return this.subItems;
			}
		}

		/// <summary>
		/// Gets or sets the menu items ID
		/// </summary>
		public string ID
		{
			get {return this.id;}
			set
			{
				this.id = value;
				if (this.isTrackingViewState)
					this.idChanged = true;
			}
		}

		/// <summary>
		/// Gets or Sets the Argument property
		/// </summary>
		public string Argument
		{
			get {return argument;}
			set
			{
				this.argument = value;

				if (this.isTrackingViewState)
					this.argumentChanged = true;
			}
		}

		/// <summary>
		/// Gets or Sets the CommandName property
		/// </summary>
		public string CommandName
		{
			get {return commandName;}
			set
			{
				this.commandName = value;

				if (this.isTrackingViewState)
					this.commandNameChanged = true;
			}
		}

		/// <summary>
		/// Gets or sets the menu item's imageUrl
		/// </summary>
		public string ImageUrl
		{
			get {return imageUrl;}
			set
			{
				imageUrl = value;
				if (this.isTrackingViewState)
					this.imageUrlChanged = true;
			}
		}

		/// <summary>
		/// The layout settings for the menu
		/// </summary>
		public string Layout
		{
			get {return layout;}
			set
			{
				layout = value;
				if (this.isTrackingViewState)
					this.layoutChanged = true;
			}
		}

		/// <summary>
		/// Gets or sets the menu item's ToolTip
		/// </summary>
		public string ToolTip
		{
			get {return toolTip;}
			set
			{
				toolTip = value;
				if (this.isTrackingViewState)
					this.toolTipChanged = true;
			}
		}

		/// <summary>
		/// Gets or sets the menu item's Enabled
		/// </summary>
		public string Enabled
		{
			get {return enabled;}
			set
			{
				enabled = value;
				if (this.isTrackingViewState)
					this.enabledChanged = true;
			}
		}

		public bool IsEnabled
		{
			get {return Enabled.ToLower()=="true";}
		}

		/// <summary>
		/// Gets or sets the menu item's Hidden
		/// </summary>
		public string Hidden
		{
			get {return hidden;}
			set
			{
				hidden = value;
				if (this.isTrackingViewState)
					this.hiddenChanged = true;
			}
		}

		public bool IsHidden
		{
			get {return Hidden.ToLower()=="true";}
		}

        /// <summary>
        /// Gets or sets the menu item's Hidden
        /// </summary>
        //EG 20120613 BlockUI New
        public string BlockUIMessage
        {
            get { return blockUIMessage; }
            set
            {
                blockUIMessage = value;
                if (this.isTrackingViewState)
                    this.blockUIMessageChanged = true;
            }
        }
        //EG 20120613 BlockUI New
        public bool IsBlockUIMessage
        {
            get { return BlockUIMessage != string.Empty; }
        }

		/// <summary>
		/// Sets or gets the MenuItem's dirty status
		/// </summary>
        //EG 20120613 BlockUI New
		[ Browsable(false) ]
		internal bool Dirty
		{
            /*
			get
			{
				return	this.idChanged       || this.textChanged     || this.urlChanged    || this.commandNameChanged || 
						this.argumentChanged || this.imageUrlChanged || this.layoutChanged || this.toolTipChanged     || 
					    this.enabledChanged  || this.hiddenChanged || this.blockUIMessageChanged;
			}
            */
            set
			{
				this.idChanged          = true;
				this.textChanged        = true;
				this.urlChanged         = true;
				this.imageUrlChanged    = true;
				this.commandNameChanged = true;
				this.argumentChanged    = true;
				this.layoutChanged      = true;
				this.toolTipChanged     = true;
				this.enabledChanged     = true;
				this.hiddenChanged      = true;
                this.blockUIMessageChanged = true;
			}
		}
		#endregion
	}
}

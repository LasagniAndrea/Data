using EFS.ACommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{
    #region OptionGroupList
    public class OptionGroupList
    {
        #region Members
        private readonly Control _listControl;
        private readonly bool _isDropDownList;
        private readonly bool _isListBox;
        private readonly bool _isCheckBoxList;
        private readonly DropDownList _optionGroupDropDownList;
        private readonly ListBox _optionGroupListBox;
        private readonly CheckBoxList _optionGroupCheckBoxList;
        private ExtendedListItemCollection _extendedItems;
        internal const string _optGroupAttributeKey = "optgroup";
        internal const string _separator = "#";
        bool _optGroupStarted = false;
        bool _selected = false;
        #endregion Members
        //
        #region Accessors
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }
        #region OptionGroupList
        #endregion ExtendedItems
        #region ExtendedItems
        [Category("Default"), Description("The items in a grouped manner."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ExtendedListItemCollection ExtendedItems
        {
            get
            {
                // get a new collection if neccesary and 
                // hook the drop down list items collection
                if (null == this._extendedItems)
                {
                    if (_isDropDownList)
                        this._extendedItems = new ExtendedListItemCollection(_optionGroupDropDownList.Items);
                    else if (_isListBox)
                        this._extendedItems = new ExtendedListItemCollection(_optionGroupListBox.Items);
                    else if (_isCheckBoxList)
                        this._extendedItems = new ExtendedListItemCollection(_optionGroupCheckBoxList.Items);
                }
                //
                return this._extendedItems;
            }
        }
        #endregion ExtendedItems
        #region Items
        /// <summary>
        /// Gets the collection of items in the list control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItemCollection"/> that represents the items within the list. The default is an empty list.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public /*new*/ ListItemCollection Items
        {
            get
            {
                return ExtendedItems._wrappedCollection;
            }

        }
        #endregion Items
        #region SelectedItemExtended
        /// <summary>
        /// Gets the selected item extended.
        /// </summary>
        /// <value>The selected item extended.</value>
        public ExtendedListItem SelectedItemExtended
        {
            get
            {
                ListItem selectedItem = null;
                if (null == this._extendedItems)
                {
                    if (_isDropDownList)
                        selectedItem = _optionGroupDropDownList.SelectedItem;
                    else if (_isListBox)
                        selectedItem = _optionGroupListBox.SelectedItem;
                    else if (_isCheckBoxList)
                        selectedItem = _optionGroupCheckBoxList.SelectedItem;
                }
                return new ExtendedListItem(selectedItem);
            }
        }
        #endregion SelectedItemExtended
        #region SelectedGroup
        public string SelectedGroup
        {
            get
            {
                //get selected item
                ExtendedListItem item = this.SelectedItemExtended;

                if (item.GroupingType == ListItemGroupingTypeEnum.New)
                    return item.GroupingText;
                else if (item.GroupingType == ListItemGroupingTypeEnum.None)
                    return string.Empty;
                else
                {
                    // find index...
                    // go through prev items, to find group text
                    int tmpIdx = this.ExtendedItems.IndexOf(item) - 1;

                    while (tmpIdx >= 0)
                    {
                        if (this.ExtendedItems[tmpIdx].GroupingType == ListItemGroupingTypeEnum.New)
                        {
                            return this.ExtendedItems[tmpIdx].GroupingText;
                        }

                        tmpIdx--;
                    }

                    //item was not found...
                    return string.Empty;
                }
            }
        }
        #endregion SelectedGroup
        #endregion Accessors
        //
        #region Constructor
        public OptionGroupList(Control pOptionGroupList)
        {
            _listControl = pOptionGroupList;
            //
            _isDropDownList = (_listControl.GetType() == typeof(DropDownList)) ||
                              (_listControl.GetType() == typeof(OptionGroupDropDownList)) ||
                              (_listControl.GetType() == typeof(WCDropDownList2));
            //
            _isListBox = (_listControl.GetType() == typeof(ListBox)) ||
                          (_listControl.GetType() == typeof(OptionGroupListBox));
            //
            _isCheckBoxList = (_listControl.GetType() == typeof(CheckBoxList)) ||
                          (_listControl.GetType() == typeof(OptionGroupCheckBoxList));
            if (_isDropDownList)
                _optionGroupDropDownList = (DropDownList)_listControl;
            else if (_isListBox)
                _optionGroupListBox = (ListBox)_listControl;
            else if (_isCheckBoxList)
                _optionGroupCheckBoxList = (CheckBoxList)_listControl;
        }
        #endregion
        //
        #region Methods
        #region protected SaveViewState
        /// <summary>
        /// Saves the state of the view.
        /// </summary>
        public object SaveViewState(object pBaseState)
        {
            // Create an object array with one element for the CheckBoxList's
            // ViewState contents, and one element for each ListItem in skmCheckBoxList
            object[] state = new object[this.Items.Count + 1];

            //object baseState = _optionGroupList.SaveViewState();
            state[0] = pBaseState;

            // Now, see if we even need to save the view state
            bool itemHasAttributes = false;
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.Items[i].Attributes.Count > 0)
                {
                    itemHasAttributes = true;

                    // Create an array of the item's Attribute's keys and values
                    object[] attribKV = new object[this.Items[i].Attributes.Count * 2];
                    int k = 0;
                    foreach (string key in this.Items[i].Attributes.Keys)
                    {
                        attribKV[k++] = key;
                        attribKV[k++] = this.Items[i].Attributes[key];
                    }

                    state[i + 1] = attribKV;
                }
            }

            // return either baseState or state, depending on whether or not
            // any ListItems had attributes
            if (itemHasAttributes)
                return state;
            else
                return pBaseState;
        }
        #endregion SaveViewState
        #region protected ClearSelection
        /// <summary>
        ///Annule la sélection de liste et affecte la valeur False à la propriété System.Web.UI.WebControls.ListItem.Selected
        ///de tous les éléments.
        /// </summary>
        public void ClearSelection()
        {
            for (int i = 0; i < this.ExtendedItems.Count; i++)
                this.ExtendedItems[i].Selected = false;
            //
            if (_isDropDownList)
                _optionGroupDropDownList.ClearSelection();
            else if (_isListBox)
                _optionGroupListBox.ClearSelection();
            else if (_isCheckBoxList)
                _optionGroupCheckBoxList.ClearSelection();
            //
            _selected = false;
        }
        #endregion ClearSelection
        #region protected LoadViewState
        /// <summary>
        /// Loads the state of the view.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        public void LoadViewState(object[] pState)
        {
            for (int i = 1; i < pState.Length; i++)
            {
                if (pState[i] != null)
                {
                    // Load back in the attributes
                    object[] attribKV = (object[])pState[i];
                    for (int k = 0; k < attribKV.Length; k += 2)
                    {
                        // FI 202309029 Add test puisque suite au postback le DDL peut être vide (ou avoir moins d'items) 
                        if ((i - 1) < this.Items.Count)
                            this.Items[i - 1].Attributes.Add(attribKV[k].ToString(), attribKV[k + 1].ToString());
                    }
                }
            }
        }
        #endregion LoadViewState
        #region protected RenderContents
        /// <summary>
        /// Renders the items in the <see cref="T:System.Web.UI.WebControls.ListControl"/> control.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream used to write content to a Web page.</param>
        public void RenderContents(HtmlTextWriter writer)
        {
            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.ExtendedItems.Count; i++)
                {
                    ExtendedListItem item = this.ExtendedItems[i];
                    //
                    if (item.GroupingType == ListItemGroupingTypeEnum.New) //.Attributes[_optGroupAttributeKey] != null)
                    {
                        WriteOptionGroup(item, writer);
                    }
                    else if (item.GroupingType == ListItemGroupingTypeEnum.Inherit)
                    {
                        WriteOption(item, writer);
                    }
                    else // ListItemGroupingType.None
                    {
                        if (_optGroupStarted)
                            writer.WriteEndTag("optgroup");

                        _optGroupStarted = false;

                        WriteOption(item, writer);
                    }
                }

                if (_optGroupStarted)
                    writer.WriteEndTag("optgroup");
            }
        }
        #endregion RenderContents
        #region private WriteOptionGroup
        /// <summary>
        /// Writes the option group.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="writer">The writer.</param>
        private void WriteOptionGroup(ExtendedListItem item, HtmlTextWriter writer)
        {
            if (_optGroupStarted)
                writer.WriteEndTag("optgroup");
            //
            writer.WriteBeginTag("optgroup");
            writer.WriteAttribute("label", item.GroupingText);
            if (!item.Enabled)
                writer.WriteAttribute("disabled", "disabled");
            if (!string.IsNullOrEmpty(item.GroupCssClass))
                writer.WriteAttribute("class", item.GroupCssClass);
            writer.Write('>');
            // writer.WriteLine();
            _optGroupStarted = true;
        }
        #endregion WriteOptionGroup
        #region private WriteOption
        /// <summary>
        /// Writes the option.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="writer">The writer.</param>
        private void WriteOption(ExtendedListItem item, HtmlTextWriter writer)
        {
            writer.WriteBeginTag("option");
            //
            if (item.Selected)
                writer.WriteAttribute("selected", "selected");
            //
            if (!string.IsNullOrEmpty(item.CssClass))
                writer.WriteAttribute("class", item.CssClass);
            //
            writer.WriteAttribute("value", item.Value, true);
            //
            if (item.Attributes.Count > 0)
            {
                StateBag bag = new StateBag();
                foreach (string attrKey in item.Attributes.Keys)
                {
                    if (attrKey.IndexOf(ExtendedListItem._attrPrefix) == -1)
                        bag.Add(attrKey, item.Attributes[attrKey]);
                }
                //
                System.Web.UI.AttributeCollection coll = new System.Web.UI.AttributeCollection(bag);
                coll.Render(writer);
            }
            //
            if (_listControl.Page != null)
            {
                _listControl.Page.ClientScript.RegisterForEventValidation(_listControl.UniqueID, item.Value);
            }
            //
            writer.Write('>');
            HttpUtility.HtmlEncode(item.Text, writer);
            writer.WriteEndTag("option");
            writer.WriteLine();
        }
        #endregion WriteOption
        #endregion Methods
    }
    #endregion OptionGroupList
    //
    #region OptionGroupDropDownList
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)] 
    public class OptionGroupDropDownList : WCDropDownList2
    {
        #region Members
        private readonly bool isWithLabel;
        private readonly OptionGroupList _optionGroupList;
        #endregion Members
        //
        #region Accessors
        #region LabelClass
        public string LabelClass
        {
            get { return labelClass; }
            set { labelClass = value; }
        }
        #endregion LabelClass
        #region ExtendedItems
        [Category("Default"), Description("The items in a grouped manner."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ExtendedListItemCollection ExtendedItems
        {
            get
            {
                return _optionGroupList.ExtendedItems;
            }
        }
        #endregion ExtendedItems
        #region Items
        /// <summary>
        /// Gets the collection of items in the list control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItemCollection"/> that represents the items within the list. The default is an empty list.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ListItemCollection Items
        {
            get
            {
                return _optionGroupList.Items;
            }
        }
        #endregion Items
        #region SelectedItemExtended
        /// <summary>
        /// Gets the selected item extended.
        /// </summary>
        /// <value>The selected item extended.</value>
        public ExtendedListItem SelectedItemExtended
        {
            get { return _optionGroupList.SelectedItemExtended; }
        }
        #endregion SelectedItemExtended
        #region SelectedGroup
        public string SelectedGroup
        {
            get
            {
                return _optionGroupList.SelectedGroup;
            }
        }
        #endregion SelectedGroup
        #endregion Accessors
        //
        #region Constructor
        public OptionGroupDropDownList() : this(false, false, null, false, null) { }
        public OptionGroupDropDownList(bool phasViewer, string pViewerClass) :
            this(false, false, null, phasViewer, pViewerClass) { }
        public OptionGroupDropDownList(bool pIsWithLabel, bool pIsLabelTop, string pLabelClass) : 
            this(pIsWithLabel, pIsLabelTop, pLabelClass, false, null) { }
        public OptionGroupDropDownList(bool pIsWithLabel, bool pIsLabelTop, string pLabelClass, bool phasViewer, string pViewerClass)
            : base(phasViewer, pViewerClass)
        {
            isWithLabel = pIsWithLabel;
            isLabelTop = pIsLabelTop;
            labelClass = pLabelClass;
            _optionGroupList = new OptionGroupList(this);
        }
        #endregion
        //
        #region Methods
        #region protected SaveViewState
        /// <summary>
        /// Saves the state of the view.
        /// </summary>
        protected override object SaveViewState()
        {
            object baseViewState = base.SaveViewState();
            return _optionGroupList.SaveViewState(baseViewState);
        }
        #endregion SaveViewState
        #region protected ClearSelection
        /// <summary>
        ///Annule la sélection de liste et affecte la valeur False à la propriété System.Web.UI.WebControls.ListItem.Selected
        ///de tous les éléments.
        /// </summary>
        public new void ClearSelection()
        {
            _optionGroupList.ClearSelection();
        }
        #endregion ClearSelection
        #region protected LoadViewState
        /// <summary>
        /// Loads the state of the view.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState == null) return;
            // we have an array of items with attributes

            // see if savedState is an object or object array
            if (savedState is object[] state)
            {
                base.LoadViewState(state[0]);   // load the base state
                _optionGroupList.LoadViewState(state);
            }
            else
                // we have just the base state
                base.LoadViewState(savedState);
        }
        #endregion LoadViewState
        #region CreateChildControls
        protected override void CreateChildControls()
        {
            if (isWithLabel && (_optionGroupList.Items.Count > 0))
            {
                string groupName = Ressource.GetString("CriteriaSelect");
                for (int i = 0; i < _optionGroupList.ExtendedItems.Count; i++)
                {
                    ExtendedListItem item = _optionGroupList.ExtendedItems[i];
                    //
                    if (item.GroupingType == ListItemGroupingTypeEnum.New)
                    {
                        groupName = item.GroupingText;
                    }
                    else if (item.GroupingType == ListItemGroupingTypeEnum.Inherit)
                    {
                        if (item.Selected)
                            break;
                    }
                    else
                    {
                        if (item.Selected)
                            break;
                    }
                }
                labelText = groupName;
            }
            base.CreateChildControls();
        }
        #endregion CreateChildControls
        #region protected RenderContents
        /// <summary>
        /// Renders the items in the <see cref="T:System.Web.UI.WebControls.ListControl"/> control.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream used to write content to a Web page.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            _optionGroupList.RenderContents(writer);
        }
        #endregion RenderContents
        #endregion Methods
    }
    #endregion OptionGroupDropDownList
    #region OptionGroupCheckBoxList
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class OptionGroupCheckBoxList : CheckBoxList, IRepeatInfoUser
    {
        #region Members
        private readonly CheckBox controlToRepeat;
        private readonly OptionGroupList _optionGroupList;
        #endregion Members
        #region Constructor
        public OptionGroupCheckBoxList() : base()
        {
            this.controlToRepeat = new CheckBox
            {
                ID = "0",
                EnableViewState = false
            };
            this.Controls.Add(this.controlToRepeat);

            _optionGroupList = new OptionGroupList(this);
        }
        #endregion
        #region Methods
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            this.controlToRepeat.AutoPostBack = this.AutoPostBack;
            if (null != this.Page)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    this.controlToRepeat.ID = i.ToString(NumberFormatInfo.InvariantInfo);
                    this.Page.RegisterRequiresPostBack(this.controlToRepeat);
                }
            }
        }
        #endregion OnPreRender
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            RepeatInfo _repeatInfo = new RepeatInfo();
            Style _style = base.ControlStyleCreated ? base.ControlStyle : null;
            short tabIndex = this.TabIndex;
            bool flag = false;
            this.controlToRepeat.TabIndex = tabIndex;
            if (0 != tabIndex)
            {
                if (false == this.ViewState.IsItemDirty("TabIndex"))
                {
                    flag = true;
                }
                this.TabIndex = 0;
            }
            _repeatInfo.RepeatColumns = this.RepeatColumns;
            _repeatInfo.RepeatDirection = this.RepeatDirection;
            _repeatInfo.RepeatLayout = this.RepeatLayout;
            _repeatInfo.RenderRepeater(writer, this, _style, this);
            if (0 != tabIndex)
                this.TabIndex = tabIndex;

            if (flag)
                this.ViewState.SetItemDirty("TabIndex", false);
        }
        #endregion Render
        #region IRepeatInfoUser Members
        public new bool HasHeader
        {
            get {return false;}
        }

        public new bool HasSeparators
        {
            get {return false;}
        }

        public new bool HasFooter
        {
            get {return false;}
        }

        public new void RenderItem(System.Web.UI.WebControls.ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
        {
            ExtendedListItem item = this.ExtendedItems[repeatIndex];
            if (null != item)
            {
                if (item.GroupingType != ListItemGroupingTypeEnum.None)
                {
                    if (!string.IsNullOrEmpty(item.GroupingClass))
                    {
                        writer.WriteBeginTag("div");
                        writer.WriteAttribute("class", item.GroupingClass);
                        writer.Write('>');
                        writer.WriteEndTag("div");
                    }
                    writer.WriteBeginTag("span");
                    writer.WriteAttribute("label", item.GroupingText);
                    if (!item.Enabled)
                        writer.WriteAttribute("disabled", "disabled");
                    if (!string.IsNullOrEmpty(item.GroupCssClass))
                        writer.WriteAttribute("class", item.GroupCssClass);
                    writer.Write('>');
                    writer.Write(item.GroupingText);
                    writer.WriteEndTag("span");
                }
                else
                {
                    this.controlToRepeat.ID = repeatIndex.ToString(NumberFormatInfo.InvariantInfo);
                    this.controlToRepeat.Text = this.Items[repeatIndex].Text;
                    this.controlToRepeat.TextAlign = this.TextAlign;
                    this.controlToRepeat.Checked = this.Items[repeatIndex].Selected;
                    this.controlToRepeat.Enabled = this.Enabled;
                    this.controlToRepeat.Attributes.Clear();
                    foreach (string key in this.Items[repeatIndex].Attributes.Keys)
                        this.controlToRepeat.Attributes.Add(key, this.Items[repeatIndex].Attributes[key]);
                    this.controlToRepeat.RenderControl(writer);
                }
            }
        }
        public new Style GetItemStyle(System.Web.UI.WebControls.ListItemType itemType, int repeatIndex)
        {
            return null;
        }

        public new int RepeatedItemCount
        {
            get {return this.Items.Count;}
        }
        #endregion




        #region protected SaveViewState
        /// <summary>
        /// Saves the state of the view.
        /// </summary>
        protected override object SaveViewState()
        {
            object baseViewState = base.SaveViewState();
            return _optionGroupList.SaveViewState(baseViewState);
        }
        #endregion SaveViewState
        #region protected LoadViewState
        /// <summary>
        /// Loads the state of the view.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState == null) return;
            // we have an array of items with attributes

            // see if savedState is an object or object array
            if (savedState is object[] state)
            {
                base.LoadViewState(state[0]);   // load the base state
                //
                _optionGroupList.LoadViewState(state);
            }
            else
                // we have just the base state
                base.LoadViewState(savedState);
        }
        #endregion LoadViewState

        #region protected ClearSelection
        /// <summary>
        ///Annule la sélection de liste et affecte la valeur False à la propriété System.Web.UI.WebControls.ListItem.Selected
        ///de tous les éléments.
        /// </summary>
        public new void ClearSelection()
        {
            _optionGroupList.ClearSelection();
        }
        #endregion ClearSelection
        #endregion Methods
        #region Accessors
        #region ExtendedItems
        [Category("Default"), Description("The items in a grouped manner."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ExtendedListItemCollection ExtendedItems
        {
            get
            {
                return _optionGroupList.ExtendedItems;
            }
        }
        #endregion ExtendedItems
        #region Items
        /// <summary>
        /// Gets the collection of items in the list control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItemCollection"/> that represents the items within the list. The default is an empty list.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ListItemCollection Items
        {
            get
            {
                return _optionGroupList.Items;
            }
        }
        #endregion Items
        #region SelectedItemExtended
        /// <summary>
        /// Gets the selected item extended.
        /// </summary>
        /// <value>The selected item extended.</value>
        public ExtendedListItem SelectedItemExtended
        {
            get { return _optionGroupList.SelectedItemExtended; }
        }
        #endregion SelectedItemExtended
        #region SelectedGroup
        public string SelectedGroup
        {
            get
            {
                return _optionGroupList.SelectedGroup;
            }
        }
        #endregion SelectedGroup
        #endregion Accessors

    }
    #endregion OptionGroupCheckBoxList
    #region OptionGroupListBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class OptionGroupListBox : ListBox
    {
        #region Members
        private readonly OptionGroupList _optionGroupList;
        #endregion Members
        #region Constructor
        public OptionGroupListBox()
            : base()
        {
            _optionGroupList = new OptionGroupList(this);
        }
        #endregion
        #region Methods
        #region protected SaveViewState
        /// <summary>
        /// Saves the state of the view.
        /// </summary>
        protected override object SaveViewState()
        {
            object baseViewState = base.SaveViewState();
            return _optionGroupList.SaveViewState(baseViewState);
        }
        #endregion SaveViewState
        #region protected LoadViewState
        /// <summary>
        /// Loads the state of the view.
        /// </summary>
        /// <param name="savedState">State of the saved.</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState == null) return;
            // we have an array of items with attributes

            // see if savedState is an object or object array
            if (savedState is object[] state)
            {
                base.LoadViewState(state[0]);   // load the base state
                _optionGroupList.LoadViewState(state);
            }
            else
                // we have just the base state
                base.LoadViewState(savedState);
        }
        #endregion LoadViewState
        #region protected RenderContents
        /// <summary>
        /// Renders the items in the <see cref="T:System.Web.UI.WebControls.ListControl"/> control.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream used to write content to a Web page.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            for (int i = 0; i < this.ExtendedItems.Count; i++)
            {
                ExtendedListItem item = this.ExtendedItems[i];
                if (item.Selected)
                {
                    if (_optionGroupList.Selected && this.Items.Count > 1)
                        this.VerifyMultiSelect();

                    _optionGroupList.Selected = true;
                }
            }
            _optionGroupList.RenderContents(writer);
        }
        #endregion RenderContents
        #region protected ClearSelection
        /// <summary>
        ///Annule la sélection de liste et affecte la valeur False à la propriété System.Web.UI.WebControls.ListItem.Selected
        ///de tous les éléments.
        /// </summary>
        public new void ClearSelection()
        {
            _optionGroupList.ClearSelection();
        }
        #endregion ClearSelection
        #endregion Methods
        #region Accessors
        #region ExtendedItems
        [Category("Default"), Description("The items in a grouped manner."), DefaultValue((string)null), MergableProperty(false), NotifyParentProperty(true)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ExtendedListItemCollection ExtendedItems
        {
            get
            {
                return _optionGroupList.ExtendedItems;
            }
        }
        #endregion ExtendedItems
        #region Items
        /// <summary>
        /// Gets the collection of items in the list control.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItemCollection"/> that represents the items within the list. The default is an empty list.</returns>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ListItemCollection Items
        {
            get
            {
                return _optionGroupList.Items;
            }
        }
        #endregion Items
        #region SelectedItemExtended
        /// <summary>
        /// Gets the selected item extended.
        /// </summary>
        /// <value>The selected item extended.</value>
        public ExtendedListItem SelectedItemExtended
        {
            get { return _optionGroupList.SelectedItemExtended; }
        }
        #endregion SelectedItemExtended
        #region SelectedGroup
        public string SelectedGroup
        {
            get
            {
                return _optionGroupList.SelectedGroup;
            }
        }
        #endregion SelectedGroup
        #endregion Accessors
    }
    #endregion OptionGroupListBox
    #region ExtendedListItemCollection
    public sealed class ExtendedListItemCollection : IList, ICollection, IEnumerable, IStateManager
    {
        #region Members
        internal readonly ListItemCollection _wrappedCollection;
        #endregion Members
        #region Constructors
        public ExtendedListItemCollection(ListItemCollection wrappedCollection)
        {
            this._wrappedCollection = wrappedCollection ?? throw new ArgumentNullException("wrappedCollection");
        }
        public ExtendedListItemCollection()
        {
            this._wrappedCollection = new ListItemCollection();
        }
        #endregion Constructors
        #region Methods
        public void Add(string item)
        {
            this._wrappedCollection.Add(item);
        }
        public void Add(ExtendedListItem item)
        {
            this._wrappedCollection.Add(this.GetSafeWrappedItem(item));
        }
        public void AddRange(ExtendedListItem[] items)
        {
            if (null != items)
            {
                ListItem[] listItems = new ListItem[items.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    listItems[i] = items[i]._listItem;
                }
                this._wrappedCollection.AddRange(listItems);
            }
        }
        public void Clear()
        {
            this._wrappedCollection.Clear();
        }
        public bool Contains(ExtendedListItem item)
        {
            return this._wrappedCollection.Contains(this.GetSafeWrappedItem(item));
        }
        public void CopyTo(Array array, int index)
        {
            List<ExtendedListItem> list = this.ToList();
            Array.Copy(list.ToArray(), 0, array, index, list.Count);
        }
        public IEnumerator GetEnumerator()
        {
            return this.ToList().GetEnumerator();
        }
        public int IndexOf(ExtendedListItem item)
        {
            return this._wrappedCollection.IndexOf(this.GetSafeWrappedItem(item));
        }
        public void Insert(int index, string item)
        {
            this._wrappedCollection.Insert(index, item);
        }
        public void Insert(int index, ExtendedListItem item)
        {
            this._wrappedCollection.Insert(index, this.GetSafeWrappedItem(item));
        }
        internal void LoadViewState(object state)
        {
            if (null != state)
            {
                object[] s = (object[])state;

                // first obj is the wrapped state

                ((IStateManager)this._wrappedCollection).LoadViewState(s[0]);

                // second is a list of grouping types

                // third is a list of grouping texts

                // restore grouping type and text
                if ((s[1] is IList<ListItemGroupingTypeEnum> types) && (types.Count == this._wrappedCollection.Count) && (s[2] is IList<string> texts) && (texts.Count == this._wrappedCollection.Count))
                {
                    for (int i = 0; i < this._wrappedCollection.Count; i++)
                    {
                        this[i].GroupingType = types[i];
                        this[i].GroupingText = texts[i];
                    }
                }
            }
        }
        internal object SaveViewState()
        {
            object[] state = new object[3];

            // first obj is the wrapped state
            object wrappedViewState = this.SaveViewState();

            IList<ExtendedListItem> items = this.ToList();

            // second is a list of grouping types
            IList<ListItemGroupingTypeEnum> listItemGroupingTypes = new List<ListItemGroupingTypeEnum>();
            foreach (ExtendedListItem item in items)
            {
                listItemGroupingTypes.Add(item.GroupingType);
            }

            // third is a list of grouping texts
            IList<string> listItemGroupingTexts = new List<string>();
            foreach (ExtendedListItem item in items)
            {
                listItemGroupingTexts.Add(item.GroupingText);
            }

            state[0] = wrappedViewState;
            state[1] = listItemGroupingTypes;
            state[2] = listItemGroupingTexts;

            return state;
        }
        internal void TrackViewState()
        {
            ((IStateManager)this._wrappedCollection).TrackViewState();
        }
        public void Remove(string item)
        {
            this._wrappedCollection.Remove(item);
        }
        public void Remove(ExtendedListItem item)
        {
            this._wrappedCollection.Remove(this.GetSafeWrappedItem(item));
        }
        public void RemoveAt(int index)
        {
            this._wrappedCollection.RemoveAt(index);
        }
        int IList.Add(object item)
        {
            return ((IList)this._wrappedCollection).Add((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }
        bool IList.Contains(object item)
        {
            return ((IList)this._wrappedCollection).Contains((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }
        int IList.IndexOf(object item)
        {
            return ((IList)this._wrappedCollection).IndexOf((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }
        void IList.Insert(int index, object item)
        {
            ((IList)this._wrappedCollection).Insert(index, (object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }
        void IList.Remove(object item)
        {
            ((IList)this).Remove((object)this.GetSafeWrappedItem((ExtendedListItem)item));
        }
        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }
        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }
        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }
        private List<ExtendedListItem> ToList()
        {
            List<ExtendedListItem> list = new List<ExtendedListItem>();
            foreach (ListItem wrappedItem in this._wrappedCollection)
            {
                list.Add(new ExtendedListItem(wrappedItem));
            }
            return list;
        }
        #endregion Methods
        #region Accessors
        public int Capacity
        {
            get { return this._wrappedCollection.Capacity; }
            set { }
        }
        public int Count
        {
            get { return this._wrappedCollection.Count; }
        }
        public bool IsReadOnly
        {
            get { return this._wrappedCollection.IsReadOnly; }
        }
        public bool IsSynchronized
        {
            get { return this._wrappedCollection.IsSynchronized; }
        }
        public ExtendedListItem this[int index]
        {
            get { return new ExtendedListItem(this._wrappedCollection[index]); }
        }
        public object SyncRoot
        {
            get { return this._wrappedCollection.SyncRoot; }
        }
        bool IList.IsFixedSize
        {
            get { return ((IList)this._wrappedCollection).IsFixedSize; }
        }
        object IList.this[int index]
        {
            get { return ((IList)this._wrappedCollection)[index]; }
            set { }
        }
        bool IStateManager.IsTrackingViewState
        {
            get { return ((IStateManager)this._wrappedCollection).IsTrackingViewState; }
        }
        private ListItem GetSafeWrappedItem(ExtendedListItem item)
        {
            return item?._listItem;
        }
        #endregion Accessors
    }
    #endregion ExtendedListItemCollection
    #region ExtendedListItem
    /// <summary>
    /// Wrapper over ListItem exposing the optgroup option.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter)),
     ControlBuilder(typeof(ListItemControlBuilder)),
     ParseChildren(true, "Text"),
     AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class ExtendedListItem : IParserAccessor, IAttributeAccessor
    {
        #region Private members
        internal static string _attrPrefix = "extended_";
        internal ListItem _listItem;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        public ExtendedListItem() : this("") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public ExtendedListItem(string text) : this(text, "") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        public ExtendedListItem(string text, string value) : this(text, value, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public ExtendedListItem(string text, string value, bool enabled) : this(text, value, enabled, ListItemGroupingTypeEnum.None, "") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="groupingType">Type of the grouping.</param>
        public ExtendedListItem(string text, string value, ListItemGroupingTypeEnum groupingType) : this(text, value, true, groupingType, "") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptGroupListItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="enabled">if set to <c>true</c> the item is enabled.</param>
        /// <param name="groupingType">The opt-grouping type.</param>
        /// <param name="groupingText">The opt-grouping text.</param>
        public ExtendedListItem(string text, string value, bool enabled, ListItemGroupingTypeEnum groupingType, string groupingText)
        {
            this._listItem = new ListItem(text, value, enabled);
            this.GroupingType = groupingType;
            this.GroupingText = groupingText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptGroupListItem"/> class.
        /// </summary>
        /// <param name="item">The wrapped item.</param>
        internal ExtendedListItem(ListItem item)
        {
            this._listItem = item ?? throw new ArgumentNullException("item");
        }
        #endregion
        #region Public Properties
        /// <summary>
        /// Gets a collection of attribute name and value pairs for the <see cref="OptGroupListItem"/> that are not directly supported by the class.
        /// </summary>
        /// <value>A System.Web.UI.AttributeCollection that contains a collection of name and value pairs.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Web.UI.AttributeCollection Attributes
        {
            get { return this._listItem.Attributes; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OptGroupListItem"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if the optgroup list item is enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        [Category("Behaviour")]
        public bool Enabled
        {
            get { return this._listItem.Enabled; }
            set { this._listItem.Enabled = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OptGroupListItem"/> is selected.
        /// </summary>
        /// <value><c>true</c> if the item is selected; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        [Category("Behaviour")]
        public bool Selected
        {
            get { return this._listItem.Selected; }
            set { this._listItem.Selected = value; }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty), DefaultValue(""), Localizable(true)]
        public string Text
        {
            get { return this._listItem.Text; }
            set { this._listItem.Text = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [Localizable(true), DefaultValue("")]
        public string Value
        {
            get { return this._listItem.Value; }
            set { this._listItem.Value = value; }
        }

        /// <summary>
        /// Gets or sets the opt-grouping type.
        /// </summary>
        /// <value>The opt-grouping type.</value>
        [Browsable(true), Description("The grouping type."), Category("OptGrouping"), DefaultValue(ListItemGroupingTypeEnum.None), NotifyParentProperty(true)]
        public ListItemGroupingTypeEnum GroupingType
        {
            get
            {
                if (null == this._listItem.Attributes[_attrPrefix + "GroupingType"])
                {
                    this._listItem.Attributes[_attrPrefix + "GroupingType"] = ListItemGroupingTypeEnum.None.ToString();
                }
                return (ListItemGroupingTypeEnum)Enum.Parse(typeof(ListItemGroupingTypeEnum), this._listItem.Attributes[_attrPrefix + "GroupingType"]);
            }
            set { this._listItem.Attributes[_attrPrefix + "GroupingType"] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the opt-grouping text.
        /// </summary>
        /// <value>The opt-grouping text.</value>
        [Description("The grouping text."), Category("OptGrouping"), DefaultValue(""), NotifyParentProperty(true)]
        public string GroupingText
        {
            get
            {
                if (null == this._listItem.Attributes[_attrPrefix + "GroupingText"])
                {
                    this._listItem.Attributes[_attrPrefix + "GroupingText"] = "";
                }
                return (string)this._listItem.Attributes[_attrPrefix + "GroupingText"];
            }
            set { this._listItem.Attributes[_attrPrefix + "GroupingText"] = value; }
        }

        /// <summary>
        /// Gets or sets the group CSS Sprites class.
        /// </summary>
        /// <value>The group CSS sprite class.</value>
        [Description("The optgroup element css sprite class."), Category("OptGrouping"), NotifyParentProperty(true), DefaultValue("")]
        public string GroupingClass
        {
            get { return _listItem.Attributes[_attrPrefix + "GroupingClass"]; }
            set { _listItem.Attributes[_attrPrefix + "GroupingClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the group CSS class.
        /// </summary>
        /// <value>The group CSS class.</value>
        [Description("The optgroup element css class."), Category("OptGrouping"), NotifyParentProperty(true), DefaultValue("")]
        public string GroupCssClass
        {
            get { return _listItem.Attributes[_attrPrefix + "GroupCssClass"]; }
            set { _listItem.Attributes[_attrPrefix + "GroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the css class.
        /// </summary>
        [Description("The option element css class."), Category("OptGrouping"), NotifyParentProperty(true), DefaultValue("")]
        public string CssClass
        {
            get { return _listItem.Attributes[_attrPrefix + "CssClass"]; }
            set { _listItem.Attributes[_attrPrefix + "CssClass"] = value; }
        }
        #endregion
        #region Internal behaviour
        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="o">The System.Object to compare with the current System.Object.</param>
        /// <returns>true if the specified System.Object is equal to the current System.Object; otherwise, false.</returns>
        public override bool Equals(object o)
        {
            if ((null == o) || !(o is ExtendedListItem))
            {
                return false;
            }

            return this._listItem.Equals(o) && (this.GroupingType == ((ExtendedListItem)o).GroupingType) && (this.GroupingText == ((ExtendedListItem)o).GroupingText);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this._listItem.GetHashCode();
        }

        /// <summary>
        /// Renders the attributes.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal void RenderAttributes(HtmlTextWriter writer)
        {
            this._listItem.Attributes.AddAttributes(writer);
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        string IAttributeAccessor.GetAttribute(string name)
        {
            return this._listItem.Attributes[name];
        }

        /// <summary>
        /// Sets the attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void IAttributeAccessor.SetAttribute(string name, string value)
        {
            this._listItem.Attributes[name] = value;
        }

        /// <summary>
        /// When implemented by an ASP.NET server control, notifies the server control that an element, either XML or HTML, was parsed.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> that was parsed.</param>
        void IParserAccessor.AddParsedSubObject(object obj)
        {
            ((IParserAccessor)this._listItem).AddParsedSubObject(obj);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            switch (this.GroupingType)
            {
                case ListItemGroupingTypeEnum.None:
                    {
                        return !string.IsNullOrEmpty(this.Text) ? this.Text : "[Text]";
                    }
                case ListItemGroupingTypeEnum.New:
                    {
                        return string.Concat(!string.IsNullOrEmpty(this.GroupingText) ? this.GroupingText : "[Group]", ".", !string.IsNullOrEmpty(this.Text) ? this.Text : "[Text]");
                    }
                case ListItemGroupingTypeEnum.Inherit:
                    {
                        return string.Concat(" ", ".", !string.IsNullOrEmpty(this.Text) ? this.Text : "[Text]");
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }
        #endregion
        #region Nested Types
        private enum DirtyFlags
        {
            None = 0,
            GroupingType = 1,
            GroupingText = 2,
            All = GroupingType | GroupingText
        }
        #endregion
    }
    #endregion ExtendedListItem
    #region ListItemGroupingTypeEnum
    public enum ListItemGroupingTypeEnum
    {
        None,
        New,
        Inherit
    }
    #endregion ListItemGroupingTypeEnum
}

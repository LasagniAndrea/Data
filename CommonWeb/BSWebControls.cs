#region using directives
using EFS.ACommon;
using EfsML.Enum;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.Common.Web
{
    #region ListFilterTools
    public sealed class ListFilterTools
    {
        public const string CHK_ACTIVE = "chkActive";
        public const string BTN_GROUP = "btnGrp";
        public const string DDL_GROUP = "ddlGrp";
        public const string DDL_COLUMN = "ddlCol";
        public const string DDL_OPERAND = "ddlOperand";
        public const string TXT_DATA = "txtData";
        public const string TXT_ID = "txtIDData";
        public const string BTN_REF = "btnRef";
        public const string SEP = ";";

        public const string BS_GROUPCOLUMN = "bsGrpCol";
        public const string BS_OPERAND = "bsOperand";
        public const string BS_DATA = "bsData";

        public class ListColumnRelation
        {
            public string table { set; get; }
            public string columnRelation { set; get; }
            public string columnSelect { set; get; }
            public string type { set; get; }
        }
        #region ListColumn
        public class ListColumn
        {
            public string group { set; get; }
            public bool isMandatory { set; get; }
            public bool isHideCriteria { set; get; }
            public string alias { set; get; }
            public string aliasDisplayName { set; get; }
            public string dataType { set; get; }
            public string tableName { set; get; }
            public string columnName { set; get; }
            public string displayName { set; get; }
            public int position { set; get; }

            public OpenReferentialArguments args { set; get; }
            public ListColumnRelation relation { set; get; }
        }
        #endregion ListColumn

    }
    #endregion ListFilterTools


    #region AttrDropDownList
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class AttrDropDownList : DropDownList
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        protected override object SaveViewState()
        {
            // create object array for Item count + 1
            object[] allStates = new object[this.Items.Count + 1];

            // the +1 is to hold the base info
            object baseState = base.SaveViewState();
            allStates[0] = baseState;

            Int32 i = 1;
            // now loop through and save each Style attribute for the List
            foreach (ListItem li in this.Items)
            {
                Int32 j = 0;
                string[][] attributes = new string[li.Attributes.Count][];
                foreach (string attribute in li.Attributes.Keys)
                {
                    attributes[j++] = new string[] { attribute, li.Attributes[attribute] };
                }
                allStates[i++] = attributes;
            }
            return allStates;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] myState = (object[])savedState;

                // restore base first
                if (myState[0] != null)
                    base.LoadViewState(myState[0]);

                Int32 i = 1;
                foreach (ListItem li in this.Items)
                {
                    // loop through and restore each style attribute
                    foreach (string[] attribute in (string[][])myState[i++])
                    {
                        li.Attributes[attribute[0]] = attribute[1];
                    }
                }
            }
        }
    }
    #endregion AttrDropDownList

    #region BS_GroupColumn
    /* Critères de filtre ListViewer.aspx (uc_ListFilter.ascx) */
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_GroupColumn : Panel
    {
        #region Member
        //public CheckBox chkIsActive;
        public bool isColumnByGroup;
        public HtmlInputCheckBox chkIsActive;
        private HtmlButton btnGroup;
        public DropDownList ddlGroup;
        public AttrDropDownList ddlColumn;
        #endregion

        public BS_GroupColumn(int pIndex, bool pIsColumnByGroup)
            : base()
        {
            isColumnByGroup = pIsColumnByGroup;

            chkIsActive = new HtmlInputCheckBox();
            chkIsActive.ID = ListFilterTools.CHK_ACTIVE + pIndex.ToString();

            btnGroup = new HtmlButton();
            btnGroup.ID = ListFilterTools.BTN_GROUP + pIndex.ToString();
            btnGroup.InnerHtml = "Group...";
            btnGroup.Attributes.Add("class", "btn btn-default btn-xs dropdown-toggle");
            btnGroup.Attributes.Add("data-toggle", "dropdown");
            btnGroup.Attributes.Add("aria-expanded", "false");


            ddlGroup = new DropDownList();
            ddlGroup.ID = ListFilterTools.DDL_GROUP + pIndex.ToString();
            ddlGroup.AutoPostBack = true;

            ddlColumn = new AttrDropDownList();
            ddlColumn.ID = ListFilterTools.DDL_COLUMN + pIndex.ToString();
            ddlColumn.Width = Unit.Percentage(100);

            this.CssClass = "col-sm-5";
            this.ID = ListFilterTools.BS_GROUPCOLUMN + pIndex.ToString();

            this.Controls.Add(chkIsActive);
            if (pIsColumnByGroup)
            {
                this.Controls.Add(btnGroup);
                this.Controls.Add(ddlGroup);
            }
            this.Controls.Add(ddlColumn);
        }

        #region LoadCHKIsActive
        public void LoadCHKIsActive(bool pIsMandatory, bool pIsEnabled)
        {
            chkIsActive.Checked = pIsMandatory || pIsEnabled;
            // attribut mandatory
            if (pIsMandatory)
                chkIsActive.Attributes.Add("mandatory", "1");

        }
        #endregion LoadCHKIsActive

        #region LoadGroup
        public void LoadGroup(List<Pair<string, string>> pListGroup)
        {
            ddlGroup.Items.Clear();
            pListGroup.ForEach(group =>
            {
                ddlGroup.Items.Add(new ListItem(group.Second, group.First));
            });
            ddlGroup.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion LoadGroup
        #region LoadColumn
        /// <summary>
        /// Chargement et valorisation d'une DropdownList COLUMN
        /// Valorisation de la DropdownList GROUP associée
        /// </summary>
        /// <param name="pGroup">Liste des groupes disponibles</param>
        /// <param name="pDicColumn">Dictionnaire des colonnes par groupe</param>
        /// <param name="pGroupValue">Groupe sélectionné</param>
        public void LoadColumn(List<Pair<string, string>> pGroup, Dictionary<string, List<ListFilterTools.ListColumn>> pDicColumn, string pGroupValue)
        {
            Pair<string, string> currentGroup = pGroup.Find(group => group.First == pGroupValue);
            List<ListFilterTools.ListColumn> columns = pDicColumn[currentGroup.First];
            // Set du Group
            ddlGroup.SelectedValue = currentGroup.First;

            // Chargement des colonnes
            ddlColumn.Items.Clear();

            columns.ForEach(column =>
            {
                if (false == column.isHideCriteria)
                {
                    // value = ISMANDATORY|TABLE|COLUMN|ALIAS
                    ListItem li = new ListItem(column.displayName, String.Format("{0};{1};{2};{3}",
                         (column.isMandatory ? "1" : "0"), column.tableName, column.columnName, column.alias.TrimEnd()));

                    // attribut Groupe
                    li.Attributes.Add("grp", currentGroup.First);
                    // attribut DataType
                    li.Attributes.Add("data-type", column.dataType);

                    ddlColumn.Items.Add(li);
                }
            });
            ddlColumn.Items.Insert(0, new ListItem(string.Empty, String.Format("0;{0};{1};{2}", string.Empty, string.Empty, string.Empty)));
        }
        #endregion LoadColumn
        #region SetColumn
        public void SetColumn(bool pIsMandatory, string pColumnValue)
        {
            if ((false == ControlsTools.DDLSelectByValue(ddlColumn, (pIsMandatory ? "1" : "0") + pColumnValue)) && (false == pIsMandatory))
            {
                // PL Tip si le Find n'aboutit pas on tente un autre find sur une colonne "Mandatory" 
                // => cas d'une colonne Mandatory utilisée également en tant que critère "Non mandatory"
                ControlsTools.DDLSelectByValue(ddlColumn, "1" + pColumnValue);
            }
        }
        #endregion SetColumn
        #region RenderContents
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (HasControls())
            {
                bool isMandatory = false;
                foreach (Control child in this.Controls)
                {

                    if (child is HtmlInputCheckBox)
                    {
                        isMandatory = StrFunc.IsFilled(((HtmlInputCheckBox)child).Attributes["mandatory"]);
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-sm-1 switch");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);

                        if (isMandatory)
                            ((HtmlInputCheckBox)child).Attributes.Add("disabled", "disabled");
                        child.RenderControl(writer);

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "label-primary");
                        writer.AddAttribute(HtmlTextWriterAttribute.For, child.ClientID.Replace("$", "_"));
                        writer.RenderBeginTag(HtmlTextWriterTag.Label);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                    }
                    else if ((child is HtmlButton))
                    {
                        #region Btn Group
                        // Btn Group
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div); // Start DIV (input-group)

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group-btn");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div); // Start input-group-btn

                        if (isMandatory)
                            ((HtmlButton)child).Attributes.Add("disabled", "disabled");
                        child.RenderControl(writer);
                        #endregion Btn Group
                    }
                    else if ((child is AttrDropDownList))
                    {
                        if (false == isColumnByGroup)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div); // Start DIV (input-group)
                        }
                        #region Column + Link For Remove line of criteria
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "form-control input-xs");
                        if (isMandatory)
                            writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                        child.RenderControl(writer);
                        //if (false == isMandatory)
                        //{
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group-addon input-xs");
                            writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, "-1");
                            writer.RenderBeginTag(HtmlTextWriterTag.A); // Start A 
                            System.Web.HttpUtility.HtmlEncode("x", writer);
                            writer.RenderEndTag(); // Fin A 
                        //}
                        #endregion Column + Link For Remove line of criteria
                    }
                    else if ((child is DropDownList))
                    {
                        #region Group
                        DropDownList ddl = child as DropDownList;
                        if (0 < ddl.Items.Count)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dropdown-menu");
                            writer.AddAttribute(HtmlTextWriterAttribute.Id, ddl.ClientID);
                            writer.RenderBeginTag(HtmlTextWriterTag.Ul); // Start UL
                            ddl.Items.Cast<ListItem>().ToList().ForEach(item =>
                            {
                                if (item.Selected)
                                    writer.AddAttribute("class", "active");
                                writer.RenderBeginTag(HtmlTextWriterTag.Li); // Start LI
                                writer.AddAttribute("value", item.Value);

                                writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                                writer.RenderBeginTag(HtmlTextWriterTag.A); // Start A
                                System.Web.HttpUtility.HtmlEncode(item.Text, writer);
                                writer.RenderEndTag(); // Fin A
                                writer.RenderEndTag(); // Fin LI
                            });
                            writer.RenderEndTag(); // Fin UL
                        }
                        writer.RenderEndTag(); // Fin input-group-btn
                        #endregion Group
                    }
                }
                writer.RenderEndTag(); // Fin DIV (input-group)
            }
        }
        #endregion RenderContents
    }
    #endregion BS_GroupColumn
    #region BS_Operand
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_Operand : Panel
    {
        #region Member
        public DropDownList ddlOperand;
        #endregion

        public BS_Operand(int pIndex)
            : base()
        {

            ddlOperand = new DropDownList();
            ddlOperand.ID = ListFilterTools.DDL_OPERAND + pIndex.ToString();
            ddlOperand.CssClass = "form-control input-xs";

            this.CssClass = "col-sm-2";
            this.ID = ListFilterTools.BS_OPERAND + pIndex.ToString();

            this.Controls.Add(ddlOperand);
        }

        #region LoadOperand
        public void LoadOperand(List<Pair<OperandEnum, string>> pListOperand, string pDataType)
        {
            ddlOperand.Items.Clear();
            pListOperand.ForEach(operand =>
            {
                ListItem li = new ListItem(operand.Second, operand.First.ToString());
                li.Enabled = TypeOperand.IsOperandEnabled(operand.First, pDataType);
                ddlOperand.Items.Add(li);
            });
            ddlOperand.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion LoadOperand
        #region HiddenOperand
        public void HiddenOperand(string pDataType)
        {
            ddlOperand.Items.Cast<ListItem>().ToList().ForEach(item =>
            {
                if (false == TypeOperand.IsOperandEnabled(item.Value, pDataType, false))
                    item.Attributes.Add("class", "hidden");
            });
        }
        #endregion HiddenOperand
        #region SetOperand
        public void SetOperand(bool pIsXMLSource, string pDataType, string pOperand, string pData)
        {
            OperandEnum operand = TypeOperand.GetTypeOperandEnum(pOperand);
            if (pIsXMLSource && TypeData.IsTypeBool(pDataType))
            {
                if ((operand == OperandEnum.equalto) && (("TRUE" == pData.ToUpper()) || ("1" == pData)))
                    operand = OperandEnum.@checked;
                else if ((operand == OperandEnum.notequalto) && (("FALSE" == pData.ToUpper()) || ("0" == pData)))
                    operand = OperandEnum.@checked;
                else if ((operand != OperandEnum.@checked) && ((operand != OperandEnum.@unchecked)))
                    operand = OperandEnum.@unchecked;
            }
            ControlsTools.DDLSelectByValue(ddlOperand, operand.ToString());
        }
        #endregion SetOperand

    }
    #endregion BS_Operand

    #region BS_Data
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_Data : Panel
    {
        #region Member
        public TextBox txtData;
        private HtmlButton btnReferential;
        #endregion

        public BS_Data(int pIndex)
            : base()
        {
            txtData = new TextBox();
            txtData.ID = ListFilterTools.TXT_DATA + pIndex.ToString();
            txtData.CssClass = "form-control input-xs";

            btnReferential = new HtmlButton();
            btnReferential.ID = ListFilterTools.BTN_REF + pIndex.ToString();
            btnReferential.Attributes.Add("class", "btn btn-default btn-xs");
            btnReferential.Attributes.Add("Tabindex", "-1");
            btnReferential.InnerHtml = "<span class='glyphicon glyphicon-option-horizontal'></span>";
            btnReferential.CausesValidation = false;

            this.CssClass = "col-sm-5";
            this.ID = ListFilterTools.BS_DATA + pIndex.ToString();

            this.Controls.Add(txtData);
            this.Controls.Add(btnReferential);
        }
        #region SetTXTData
        /// <summary>
        /// Chargement et Valorisation de la valeur du filtre 
        /// </summary>
        /// <param name="pDDLOperand"></param>
        /// <param name="dataTypeOperand">Type de donnée de la colonne (pour filtrer les opérateurs disponibles)</param>
        /// <param name="selectedOperand">Valeur de l'opérateur</param>
        public void SetTXTData(string pDataType, string pDataFormatted)
        {
            // RD 20110706 [17504]
            // Pour ne pas afficher la zone de saisie 
            if (TypeData.IsTypeBool(pDataType))
            {
                txtData.Text = string.Empty;
                txtData.Visible = false;
            }
            else
            {
                txtData.Text = pDataFormatted;
                txtData.Visible = true;
            }
        }
        #endregion LoadTXTData

        /// <summary>
        /// Formatage des données filtres
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pValue"></param>
        /// <param name="pDataType"></param>
        /// <param name="pIsFormatForDisplay">
        /// <para>si true  retourne la valeur stockée dans LSTWHERE formattée pour l'affichage</para>
        /// <para>si false retourne la valeur saisie au format ISO formattée pour le stockage dans LSTWHERE</para>
        /// </param>
        /// <param name="pIsResource"></param>
        /// <param name="pIsFormatForFormCriteria"></param>
        /// <returns></returns>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (HasControls())
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group");
                writer.RenderBeginTag(HtmlTextWriterTag.Div); // Start DIV (input-group)

                foreach (Control child in this.Controls)
                {
                    System.Diagnostics.Debug.WriteLine(child.UniqueID);
                    if ((child is HtmlButton))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "input-group-btn");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div); // Start DIV
                        child.RenderControl(writer);
                        writer.RenderEndTag(); // Fin DIV (input-group)
                    }
                    else
                    {
                        child.RenderControl(writer);
                    }
                }
                writer.RenderEndTag(); // Fin DIV (input-group)
            }
        }
    }
    #endregion BS_Data
    #region BS_ColumnBase
    /* Colonnes triées + Regroupement (uc_ListDisplay.ascx) */
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public abstract class BS_ColumnBase : BulletedList
    {
        #region Member
        public LstSelectionTypeEnum selectionType;
        public bool isColumnByGroup;
        #endregion

        public BS_ColumnBase(bool pIsColumnByGroup, LstSelectionTypeEnum pLstSelectionType)
            : base()
        {
            isColumnByGroup = pIsColumnByGroup;
            selectionType = pLstSelectionType;
            this.ID = "blColumn" + ((LstSelectionTypeEnum.SORT == selectionType) ? "Sorted" : "Displayed");
            this.Attributes.Add("class", "list-group connected ui-sortable");
        }

        public virtual void AddItem(string pGroup, string pTableName, string pColumnDisplayName, string pColumnName, string pAlias, bool pIsGroupBy, string pSort)
        {
            string @value = pTableName + ListFilterTools.SEP + pColumnName + ListFilterTools.SEP + pAlias;
            ListItem li = new ListItem(pColumnDisplayName, @value);
            li.Attributes.Add("class", "list-group-item");
            if (isColumnByGroup)
                li.Attributes.Add("grpv", pGroup);
            if (pIsGroupBy)
                li.Attributes.Add("isgroupby", "1");

            if (StrFunc.IsFilled(pSort))
                li.Attributes.Add("sort", pSort);

            this.Items.Add(li);
        }

        public virtual void SetGroupingSetValue(Cst.GroupingSet pGroupingSet)
        {
        }

        #region RenderContents
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (0 < this.Items.Count)
            {
                int i = 0;
                this.Items.Cast<ListItem>().ToList().ForEach(item =>
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, item.Attributes["class"]);
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, item.Value);

                    if (isColumnByGroup)
                    {
                        writer.AddAttribute("grpv", item.Attributes["grpv"]);
                    }

                    if (selectionType == LstSelectionTypeEnum.SORT)
                    {
                        writer.AddAttribute("sort", item.Attributes["sort"]);
                        writer.AddAttribute("isgroupby", item.Attributes["isgroupby"]);
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    System.Web.HttpUtility.HtmlEncode(item.Text, writer);

                    if (selectionType == LstSelectionTypeEnum.SORT)
                    {
                        HtmlGenericControl chkGroupBy = new HtmlGenericControl("input");
                        chkGroupBy.Attributes.Add("type", "checkbox");
                        chkGroupBy.Attributes.Add("id", this.ID + "_groupby" + i);
                        if ("1" == item.Attributes["isgroupby"])
                            chkGroupBy.Attributes.Add("checked", "checked");
                        chkGroupBy.RenderControl(writer);
                    }

                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    writer.RenderBeginTag(HtmlTextWriterTag.A); // Start A 
                    System.Web.HttpUtility.HtmlEncode("x", writer);
                    writer.RenderEndTag(); // Fin A 

                    if (selectionType == LstSelectionTypeEnum.SORT)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "switch ascdesc");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div); // Start Div Sorted (ASC|DESC)

                        HtmlGenericControl chkSort = new HtmlGenericControl("input");
                        chkSort.Attributes.Add("id", this.ID + "_sort" + i);
                        chkSort.Attributes.Add("type", "checkbox");
                        if ("DESC" == item.Attributes["sort"])
                            chkSort.Attributes.Add("checked", "checked");
                        chkSort.RenderControl(writer);
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "label-info");
                        writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID + "_sort" + i);
                        writer.RenderBeginTag(HtmlTextWriterTag.Label); // Start Label 
                        writer.RenderEndTag(); // Fin Label

                        writer.RenderEndTag(); // Fin Div Sorted (ASC|DESC)

                    }

                    writer.RenderEndTag();

                    i++;
                });

            }
        }
        #endregion RenderContents

    }
    #endregion BS_ColumnBase
    #region BS_ColumnSelected
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_ColumnSelected : BS_ColumnBase
    {
        public BS_ColumnSelected(bool pIsColumnByGroup, LstSelectionTypeEnum pLstSelectionType)
            : base(pIsColumnByGroup, pLstSelectionType)
        {
        }
    }
    #endregion BS_ColumnSelected
    #region BS_ColumnSorted
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_ColumnSorted : BS_ColumnBase
    {
        #region Member
        public Cst.GroupingSet groupingSet;
        #endregion

        public BS_ColumnSorted(bool pIsColumnByGroup, LstSelectionTypeEnum pLstSelectionType)
            : base(pIsColumnByGroup, pLstSelectionType)
        {
            groupingSet = default(Cst.GroupingSet);

        }

        public override void SetGroupingSetValue(Cst.GroupingSet pCurrentGroupingSet)
        {
            if (selectionType == LstSelectionTypeEnum.SORT)
            {
                groupingSet = pCurrentGroupingSet | groupingSet;
            }
        }
    }
    #endregion BS_ColumnSorted

    #region BS_CheckBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_CheckBox : CheckBox
    {
        #region Members
        private bool _isReadOnly;
        #endregion

        #region Accessor
        #region isReadOnly
        public bool isReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        #endregion
        #endregion

        #region constructor
        public BS_CheckBox()
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
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            Enabled = Enabled && (false == _isReadOnly);
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
    #endregion BS_CheckBox
    #region BS_ClassicCheckBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_ClassicCheckBox : Panel
    {
        #region Member
        public CheckBox chk;
        public Panel div;
        private bool _isReadOnly;
        #endregion

        #region Accessor
        #region isReadOnly
        public bool isReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        #endregion
        #endregion

        #region constructor
        public BS_ClassicCheckBox()
        {
        }
        public BS_ClassicCheckBox(bool pEnabled, string pLabel, bool pIsAutoPostBack) :this(string.Empty, pEnabled, pLabel, pIsAutoPostBack)
        {
        }
        public BS_ClassicCheckBox(string pId, bool pEnabled, string pLabel, bool pIsAutoPostBack)
        {
            chk = new CheckBox();
            chk.ID = pId;
            chk.Text = pLabel;
            chk.EnableViewState = true;
            chk.Enabled = pEnabled;
            chk.AutoPostBack = pIsAutoPostBack;

            this.CssClass = "checkbox";

            div = new Panel();
            div.CssClass = "form-control input-xs";
            div.Controls.Add(chk);

            this.Controls.Add(div);
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
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
    #endregion BS_ClassicCheckBox
    #region BS_RadioButton
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_RadioButton : Panel
    {
        #region Member
        public RadioButton rbPrimary;
        public RadioButton rbSecondary;
        public Panel div;
        private bool _isReadOnly;
        #endregion

        #region Accessor
        #region isReadOnly
        public bool isReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }
        #endregion
        #endregion

        #region constructor
        public BS_RadioButton(string pId, bool pEnabled, string pLabel, bool pIsAutoPostBack, RepeatDirection pDirection)
        {
            this.CssClass = "radio2";

            div = new Panel();
            div.CssClass = "form-control input-xs";

            string[] labels = pLabel.Split("|".ToCharArray());
            if (ArrFunc.IsFilled(labels) && (2 == labels.Length))
            {
                rbPrimary = new RadioButton();
                rbPrimary.ID = pId;
                rbPrimary.GroupName = pId;
                rbPrimary.Text = labels[0];
                rbPrimary.AutoPostBack = pIsAutoPostBack;
                rbPrimary.Enabled = pEnabled;
                rbPrimary.EnableViewState = true;
                div.Controls.Add(rbPrimary);

                rbSecondary = new RadioButton();
                rbSecondary.ID = pId + "Bis";
                rbSecondary.GroupName = pId;
                rbSecondary.Text = labels[1];
                rbSecondary.AutoPostBack = pIsAutoPostBack;
                rbSecondary.Enabled = pEnabled;
                rbSecondary.EnableViewState = true;
                div.Controls.Add(rbSecondary);
            }
            this.Controls.Add(div);
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
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
    #endregion BS_RadioButton

    #region BS_HtmlCheckBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_HtmlCheckBox : HtmlInputCheckBox
    {
        #region Member
        private Label _label;
        private bool _isReadOnly;
        #endregion
        //
        #region Accessor
        public bool isReadOnly
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
        public BS_HtmlCheckBox() : this(false, 0) { }
        public BS_HtmlCheckBox(bool pWithLabel) : this(pWithLabel, 0) { }
        public BS_HtmlCheckBox(bool pWithLabel, int pLblWidth)
        {
            if (pWithLabel)
            {
                _label = new Label();
                if (0 != pLblWidth)
                    _label.Width = Unit.Pixel(pLblWidth);
            }
            EnsureChildControls();
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
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            Enabled = Enabled && (false == _isReadOnly);
            if (this.ExistLabel)
            {
                _label.RenderControl(writer);
                base.Render(writer);
            }
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
    #endregion BS_HtmlCheckBox
    #region BS_Label
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_Label : Label
    {
        #region Constructors
        public BS_Label()
        {
            CssClass = "control-label";
        }
        #endregion Constructors
    }
    #endregion BS_Label


    #region BS_TextArea
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_TextArea : PlaceHolder //Panel
    {
        #region Members
        private string id { get; set; }
        public string text { get; set; }
        public int height { get; set; }
        public bool isContentEditable { get; set; }
        #endregion

        #region Constructors
        public BS_TextArea(string pId, bool pIsContentEditable) : base()
        {
            id = pId;
            isContentEditable = pIsContentEditable;
        }
        #endregion Constructors

        protected override void OnPreRender(EventArgs e)
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("contenteditable", isContentEditable ? "true":"false");
            div.Attributes.Add("class", "form-control input-xs");
            div.Style.Add("height", height + "px");
            div.Style.Add("overflow-y", "auto");
            div.InnerHtml = text.Replace(Cst.CrLf, Cst.HTMLBreakLine);
            div.ID = id;
            this.Controls.Add(div);
        }

    }
    #endregion BS_TextArea

    #region BS_TextBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_TextBox : TextBox
    {
        #region Members
        private List<Validator> lstValidator = new List<Validator>();
        #endregion

        #region Constructors
        public BS_TextBox() 
        {
            CssClass = "form-control input-xs";
        }
        public BS_TextBox(string pId, bool pIsReadOnly, List<Validator> pLstValidators) : base()
        {
            CssClass = "form-control input-xs";
            ID = pId;
            ReadOnly = pIsReadOnly;

            lstValidator = pLstValidators;
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
            if ((null != lstValidator) && (0 < lstValidator.Count))
            {
                lstValidator.ForEach(validator =>
                {
                    this.Controls.AddAt(0, validator.CreateControl(this.ID));
                });
            }
            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            foreach (WebControl ctrl in this.Controls)
                ((BaseValidator)ctrl).ControlToValidate = this.ID;
            base.OnPreRender(e);
        }

        #region protected override Render
        protected override void Render(HtmlTextWriter writer)
        {
            RenderChildren(writer);
            base.Render(writer);
        }
        #endregion


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
        #endregion
    }
    #endregion BS_TextBox
    #region BS_DropDownList
    /// <summary>
    /// DropDownList
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [Serializable]
    [ComVisible(false)]
    public class BS_DropDownList : DropDownList
    {
        #region Members
        [NonSerialized]
        private TextBox  _txtViewer;
        private bool _hasViewer;
        //_isSetTextOnTitle = true 
        //=> Tooltip des Items = Text des items 
        //=> Tooltip de la DDL Text de selectIndex
        private bool _isSetTextOnTitle;
        #endregion Members

        #region Accessors
        public bool IsSetTextOnTitle
        {
            get { return _isSetTextOnTitle; }
            set { _isSetTextOnTitle = value; }
        }
        public bool hasViewer
        {
            get { return _hasViewer; }
            set { _hasViewer = true; }
        }
        public TextBox txtViewer
        {
            get { return _txtViewer; }
        }
        #endregion Accessors

        #region constructor
        public BS_DropDownList() : this(false) { }
        public BS_DropDownList(bool pHasViewer)
        {
            CssClass = "form-control input-xs";
            _hasViewer = pHasViewer;
            _txtViewer = new TextBox();
            _txtViewer.CssClass = this.CssClass;
            _isSetTextOnTitle = false;
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
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

                _txtViewer.Text = string.Empty;
                if (null != SelectedItem)
                {
                    _txtViewer.Text = SelectedItem.Text;
                    _txtViewer.TabIndex = -1;
                    _txtViewer.ReadOnly = true;
                }
                _txtViewer.Attributes.Add("title", this.Attributes["title"]);
                _txtViewer.Attributes.Add("data-content", this.Attributes["data-content"]);
                _txtViewer.Attributes.Add("data-theme", this.Attributes["data-theme"]);
                _txtViewer.Width = Width;
            }
            else
            {
                if (IsSetTextOnTitle)
                    ControlsTools.SetAttributesList(this.Attributes, "onchange:DDLSetTextOnToolTip(this.event)");
            }
            //
            if (null != SelectedItem)
                Attributes["oldvalue"] = SelectedItem.Text;
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

            base.Render(writer);
            if (_hasViewer)
            {
                if (null != this.ID)
                {
                    _txtViewer.ID = Cst.TXT + ID.Substring(Cst.DDL.Length) + "Viewer";
                    _txtViewer.RenderControl(writer);
                    _txtViewer.ToolTip = _txtViewer.Text;
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
    #endregion BS_DropDownList

    #region BS_TextBoxDate
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_TextBoxDate : PlaceHolder
    {
        #region Members
        private List<Validator> lstValidators = new List<Validator>();
        #endregion Members
        #region Accessors
        public Panel Pnl
        {
            get { return this.Controls[this.Controls.Count -1] as Panel; }
        }
        public BS_TextBox Date
        {
            get {return Pnl.Controls[0] as BS_TextBox;}
        }
        #endregion Accessors

        #region Constructors
        public BS_TextBoxDate() { }
        //public BS_TextBoxDate(string pId, TypeData.TypeDataEnum pDataType) : this(pId, false, false, pDataType.ToString()) { }
        //public BS_TextBoxDate(string pId, params Validator[] pValidator) : this(pId, false, false, TypeData.TypeDataEnum.date.ToString(), pValidator) { }

        public BS_TextBoxDate(string pId, bool pIsReadOnly, string pDataType, List<Validator> pListValidators)
        {
            string format = "L";
            switch (TypeData.GetTypeDataEnum(pDataType))
            {
                case TypeData.TypeDataEnum.date:
                    format = "L";
                    break;
                case TypeData.TypeDataEnum.datetime:
                    format = "L LTS";
                    break;
                case TypeData.TypeDataEnum.time:
                    format = "LTS";
                    break;
            }
            Panel div = new Panel();
            div.Attributes.Add("data-type", format);
            div.CssClass = "input-group date";

            BS_TextBox txtDate = new BS_TextBox(pId, pIsReadOnly, null);
            txtDate.Width = Unit.Pixel(80);
            div.Controls.Add(txtDate);

            HtmlGenericControl addon = new HtmlGenericControl("span");
            addon.Attributes.Add("class", "input-group-addon input-xs");
            HtmlGenericControl glyphicon = new HtmlGenericControl("span");
            glyphicon.Attributes.Add("class", String.Format("glyphicon glyphicon-{0}",  TypeData.IsTypeTime(pDataType)? "time" : "calendar" ));
            addon.Controls.Add(glyphicon);
            div.Controls.Add(addon);

            this.Controls.Add(div);

            lstValidators = pListValidators;
            EnsureChildControls();
        }
        #endregion Constructors

        protected override void CreateChildControls()
        {
            Pnl.Attributes.Add("oldvalue", string.Empty);
            if ((null != lstValidators) && (0 < lstValidators.Count))
            {
                lstValidators.ForEach(validator =>
                {
                    this.Controls.AddAt(0, validator.CreateControl(this.ID));
                });
            }
            base.CreateChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
            {
                BaseValidator validator = ctrl as BaseValidator;
                if (null != validator)
                    validator.ControlToValidate = this.Date.ID;
            }
            base.OnPreRender(e);
        }

    }
    #endregion BS_TextBoxDate

    #region BS_ContentCheckBox
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class BS_ContentCheckBox : Label
    {
        #region Constructor
        public BS_ContentCheckBox(Control pControl)
        {
            CssClass = "switch ctrl";
            this.Controls.Add(pControl);
        }
        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            HtmlGenericControl lbl = new HtmlGenericControl("label");
            lbl.Attributes.Add("for", this.Controls[0].ClientID);
            this.Controls.Add(lbl);
        }
    }
    #endregion BS_ContentCheckBox

}

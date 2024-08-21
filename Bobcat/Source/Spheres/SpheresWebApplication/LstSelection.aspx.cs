using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EFS.Referential;
using System;
using System.Collections;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    public partial class LstSelectionPage : PageBaseTemplate
    {
        #region Private Constantes
        private const string lbAvailable = "lstAvailable";
        private const string lbSelected = "lstSelected";
        private const string lbGroupBy = "lstGroupBy";
        // MF 20120430 ruptures with groupingset
        private const string lbGroupingSet = Cst.DDL + "GroupingSet";
        //
        private const string btnSelectAll = "butSelectAll";
        private const string btnSelect = "butSelect";
        private const string btnUnselect = "butUnselect";
        private const string btnUnselectAll = "butUnselectAll";
        //
        private const string btnSelectAllSort = "butSelectAllSort";
        private const string btnSelectSort = "butSelectSort";
        private const string btnUnselectSort = "butUnselectSort";
        private const string btnUnselectAllSort = "butUnselectAllSort";
        //
        private const string btnUp = "butUp";
        private const string btnDown = "butDown";
        private const string btnAsc = "butAsc";
        private const string btnDesc = "butDesc";
        #endregion

        #region Accessor
        /// <summary>
        /// 
        /// </summary>
        protected override string SpecificSubTitle
        {
            get
            {
                string ret = string.Empty;
                if (selectionType == LstSelectionTypeEnum.SORT)
                    ret = Ressource.GetString("btnViewerSort");
                else if (selectionType == LstSelectionTypeEnum.DISP)
                    ret = Ressource.GetString("btnViewerDisplay");

                return ret.Replace("...", string.Empty).Trim();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override PlaceHolder PlhMain
        {
            get
            {
                return plhControl;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200527 [XXXXX] Add
        protected override string PageName
        {
            get { return "LstSelectionPage"; }
        }
        #endregion accessor

        #region Members
        private LstSelectionTypeEnum selectionType;
        #endregion Members

        #region constructor
        public LstSelectionPage()
        {
            selectionType = LstSelectionTypeEnum.NONE;
        }
        #endregion constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            string typeInURL = Request.QueryString["TYPE"];
            if (Enum.IsDefined(typeof(LstSelectionTypeEnum), typeInURL))
                selectionType = (LstSelectionTypeEnum)Enum.Parse(typeof(LstSelectionTypeEnum), typeInURL, true);
            else
                selectionType = LstSelectionTypeEnum.NONE;

            InitializeComponent();
            this.Form = frmLstSelection;
            base.OnInit(e);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void CreateControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ListBox.js"));
            CreateToolBar();
            CreateControlsData();
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void LoadDataControls()
        {
            ThisConsultation.LoadTemplate(SessionTools.CS, idLstTemplate, idA);

            LoadListBoxAvailable();
            LoadListBoxSelected();

            base.VerifyUserRightsLstTemplate();
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void SaveData()
        {
            ThisConsultation.LoadTemplate(SessionTools.CS, idLstTemplate, idA);
            
            // FI 20201209 [XXXXX] Call CreateTemporaryTemplate 
            // Appuie sur le bouton ok => génération d'un template temporaire 
            CreateTemporaryTemplate();

            //on récupère les données selectionnées dans le contrôle caché           
            HtmlInputHidden hidSelection = (HtmlInputHidden)this.FindControl("hidSelection");
            string selectedValues = hidSelection.Value;
            // EG 20130925
            string _temp = selectedValues.Replace("|", string.Empty);
            int nbPipe = selectedValues.Length - _temp.Length + 1;

            string[] selectedValue = selectedValues.Split("|".ToCharArray(), nbPipe);

            
            ArrayList alGroupBy = null;
            //selon si il s'agit de la fenetre 'Tri' ou de 'Affichage'
            Cst.OTCml_TBL Tbl_LSTSELECTorLSTORDERBY = Cst.OTCml_TBL.LSTSELECT;
            if (selectionType == LstSelectionTypeEnum.SORT)
            {
                Tbl_LSTSELECTorLSTORDERBY = Cst.OTCml_TBL.LSTORDERBY;
                //
                HtmlInputHidden hidGroupBy = (HtmlInputHidden)this.FindControl("hidGroupBy");
                string groupByValues = hidGroupBy.Value;
                string[] groupByValue = groupByValues.Split("|".ToCharArray(), nbPipe);
                alGroupBy = new ArrayList(groupByValue);
            }

            //delete from table
            ReferentialWeb.DeleteChild(SessionTools.CS, Tbl_LSTSELECTorLSTORDERBY, idLstConsult, idLstTemplate, idA, false);

            //insert in LSTORDERBY or LSTSELECT
            // FI 20200602 [25370] Appel à InsertLstSelectOrLstOrder
            InsertLstSelectOrLstOrder(selectedValue, Tbl_LSTSELECTorLSTORDERBY, alGroupBy);
            
            ReferentialWeb.WriteTemplateSession(idLstConsult, idLstTemplate, idA, this.ParentGUID);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20210222 [XXXXX] Suppression inscription function SetFocus
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageTools.SetHead(this, this.Title, null, null);
            if (!IsPostBack)
            {
                JavaScript.CallFunction(this, "SetFocus('btnOk_footer')");
            }
        }
        
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.AbortRessource = true;
            this.ID = "frmLstSelection";

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateControlsData()
        {
            Panel pnlBody = new Panel() 
            { 
                ID = "divbody", CssClass = MainClass 
            };
            Panel pnlData = new Panel() 
            { 
                ID = "divlstselect", 
                CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu) + " " + selectionType.ToString().ToLower() 
            };
            PlhMain.Controls.Add(pnlBody);


            Panel pnlData2 = new Panel();
            // Columns available
            WCTogglePanel pnlColumnAvailable = new WCTogglePanel
            {
                CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu)
            };
            pnlColumnAvailable.SetHeaderTitle(Ressource.GetString("lblAvailableColumnList"));
            OptionGroupListBox lstAvailable = ControlsTools.GetOptGroupListBox(lbAvailable);
            lstAvailable.Rows = 18;
            pnlColumnAvailable.AddContent(lstAvailable);

            // Columns selected
            WCTogglePanel pnlColumnSelected = new WCTogglePanel
            {
                CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu)
            };
            pnlColumnSelected.SetHeaderTitle(Ressource.GetString("lblSelectedColumnList"));
            ListBox lstSelected = ControlsTools.GetListBox(lbSelected);
            lstSelected.Rows = 18;
            lstSelected.CssClass = EFSCssClass.DropDownListCaptureLight;
            pnlColumnSelected.AddContent(lstSelected);

            // Toolbar for selected Columns available
            Panel pnlButtonSelect = new Panel();
            string functionToCall;
            // Select All
            if (selectionType == LstSelectionTypeEnum.SORT)
                functionToCall = "MoveAllWithPrefix(document.forms[0].lstAvailable,document.forms[0].lstSelected,'ASC -');";
            else
                functionToCall = "CopyAll(document.forms[0].lstAvailable,document.forms[0].lstSelected);";

            pnlButtonSelect.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnSelectAll, string.Empty, "fas fa-angle-double-right", "SelectAll", functionToCall + "return false;"));
            // Select
            if (selectionType == LstSelectionTypeEnum.SORT)
                functionToCall = "MoveWithPrefix(document.forms[0].lstAvailable,document.forms[0].lstSelected,'ASC -');";
            else
                functionToCall = "Copy(document.forms[0].lstAvailable,document.forms[0].lstSelected);";
            pnlButtonSelect.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnSelect, string.Empty, "fas fa-angle-right", "Select", functionToCall + "return false;"));

            // Remove
            if (selectionType == LstSelectionTypeEnum.SORT)
                functionToCall = "MoveWithoutPrefix(document.forms[0].lstSelected,document.forms[0].lstAvailable,document.forms[0].lstGroupBy,5);";
            else
                functionToCall = "Remove(document.forms[0].lstSelected);";
            pnlButtonSelect.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnUnselect, string.Empty, "fas fa-angle-left", "Unselect", functionToCall + "return false;"));

            //Remove All
            if (selectionType == LstSelectionTypeEnum.SORT)
                functionToCall = "MoveAllWithoutPrefix(document.forms[0].lstSelected,document.forms[0].lstAvailable,document.forms[0].lstGroupBy,5);";
            else
                functionToCall = "RemoveAll(document.forms[0].lstSelected);";
            pnlButtonSelect.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnUnselectAll, string.Empty, "fas fa-angle-double-left", "UnselectAll", functionToCall + "return false;"));

            // Toolbar for order Columns 
            Panel pnlButtonOrder = new Panel();
            if (selectionType == LstSelectionTypeEnum.SORT)
            {
                //Select All
                string functionToCallSort = "CopyAllWithoutPrefix(document.forms[0].lstSelected,document.forms[0].lstGroupBy,5);";
                pnlButtonOrder.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnSelectAllSort, string.Empty, "fas fa-angle-double-right", "SelectAll", functionToCallSort + "return false;"));
                //Select
                functionToCallSort = "CopyWithoutPrefix(document.forms[0].lstSelected,document.forms[0].lstGroupBy,5);";
                pnlButtonOrder.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnSelectSort, string.Empty, "fas fa-angle-right", "Select", functionToCallSort + "return false;"));

                //Remove
                functionToCallSort = "Remove(document.forms[0].lstGroupBy);";
                pnlButtonOrder.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnUnselectSort, string.Empty, "fas fa-angle-left", "Unselect", functionToCallSort + "return false;"));

                //Remove All
                functionToCallSort = "RemoveAll(document.forms[0].lstGroupBy);";
                pnlButtonOrder.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnUnselectAllSort, string.Empty, "fas fa-angle-double-left", "UnselectAll", functionToCallSort + "return false;"));
            }
            else
            {
                // Up/Down
                pnlButtonOrder.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnUp, string.Empty, "fas fa-angle-up", "SelectUp", "Switch(document.forms[0].lstSelected, -1);return false;"));
                pnlButtonOrder.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnDown, string.Empty, "fas fa-angle-down", "SelectDown", "Switch(document.forms[0].lstSelected, 1);return false;"));

            }

            pnlData2.Controls.Add(pnlColumnAvailable);
            pnlData2.Controls.Add(pnlButtonSelect);
            pnlData2.Controls.Add(pnlColumnSelected);
            pnlData2.Controls.Add(pnlButtonOrder);
            pnlData.Controls.Add(pnlData2);

            if (selectionType == LstSelectionTypeEnum.SORT)
            {
                Panel pnlData3 = new Panel();
                pnlData3.Controls.Add(new Panel());

                WCTogglePanel pnlGroupBy = new WCTogglePanel
                {
                    CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu)
                };
                pnlGroupBy.SetHeaderTitle(Ressource.GetString("lblGroupByColumnList"));

                ListBox lstGroupBy = ControlsTools.GetListBox(lbGroupBy);
                lstGroupBy.Rows = 18;
                lstGroupBy.CssClass = EFSCssClass.DropDownListCaptureLight;
                pnlGroupBy.AddContent(lstGroupBy);
                pnlData2.Controls.Add(pnlGroupBy);

                Panel pnlButtonSort = new Panel();
                pnlButtonSort.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnUp, string.Empty, "fas fa-angle-up", "SelectUp", "Switch(document.forms[0].lstSelected, -1);return false;"));
                pnlButtonSort.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnDown, string.Empty, "fas fa-angle-down", "SelectDown", "Switch(document.forms[0].lstSelected, 1);return false;"));
                pnlButtonSort.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnAsc, string.Empty, "fas fa-sort-alpha-down", "SortAsc", "SetAscDesc(document.forms[0].lstSelected, 'ASC');return false;"));
                pnlButtonSort.Controls.Add(ControlsTools.GetAwesomeLinkButton(btnDesc, string.Empty,"fas fa-sort-alpha-up", "SortDesc", "SetAscDesc(document.forms[0].lstSelected, 'DESC');return false;"));
                pnlData3.Controls.Add(pnlButtonSort);

                Panel pnlGroupingSet = new Panel();

                Label lbl = new Label
                {
                    ID = lbGroupingSet + "LABEL",
                    CssClass = EFSCssClass.LabelDisplay,
                    Text = Ressource.GetString("GroupingSet"),
                    EnableViewState = true
                };
                pnlGroupingSet.Controls.Add(lbl);

                DropDownList ddl = new WCDropDownList2
                {
                    ID = lbGroupingSet,
                    EnableViewState = true
                };
                ListItem item = new ListItem("", Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.Unknown));
                ddl.Items.Add(item);
                item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.TotalDetails)),
                    Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.TotalDetails));
                ddl.Items.Add(item);
                item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.TotalSubTotalDetails)),
                    Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.TotalSubTotalDetails));
                ddl.Items.Add(item);
                item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.TotalSubTotal)),
                    Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.TotalSubTotal));
                ddl.Items.Add(item);
                item = new ListItem(Ressource.GetString(String.Format("GroupingSet{0}", Cst.GroupingSet.Total)),
                    Cst.CastGroupingSetToDDLValue(Cst.GroupingSet.Total));
                ddl.Items.Add(item);
                pnlGroupingSet.Controls.Add(ddl);

                WCToolTipLinkButton btn = new WCToolTipLinkButton
                {
                    ID = lbGroupingSet + "TOOLTIP",
                    CssClass = "fa-icon",
                    Text = @"<i class='fa fa-info-circle'></i>"
                };
                btn.Pty.TooltipContent = Ressource.GetString("GroupingSetDescription");
                pnlGroupingSet.Controls.Add(btn);
                pnlData3.Controls.Add(pnlGroupingSet);
                pnlData.Controls.Add(pnlData3);
            }
            pnlBody.Controls.Add(pnlData);
            PlhMain.Controls.Add(pnlBody);
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateToolBar()
        {
            Panel pnlToolBar = AddToolBar();
            string saveScript = "Record(document.forms[0].lstSelected, document.forms[0].hidSelection);";
            if (selectionType == LstSelectionTypeEnum.SORT)
                saveScript += "Record(document.forms[0].lstGroupBy,document.forms[0].hidGroupBy);";
            saveScript += "return true;";
            WCToolTipLinkButton btn = AddButtonOk(saveScript);
            defaultButton = btn.ID;
            pnlToolBar.Controls.Add(btn);
            pnlToolBar.Controls.Add(AddButtonCancel());
            pnlToolBar.Controls.Add(AddButtonOkAndSave(saveScript));
            WCTooltipLabel labelMissingModPermissions = ControlsTools.GetLabelMissingUserRightsLstTemplate(PlhMain);
            pnlToolBar.Controls.Add(labelMissingModPermissions);
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadListBoxAvailable()
        {
            ListBox lbCtrlAvailable = (ListBox)PlhMain.FindControl(lbAvailable);
            bool isOptGroupList = (lbCtrlAvailable.GetType() == typeof(OptionGroupListBox));

            ThisConsultation.LoadLstSelectAvailable(SessionTools.CS);
            // FI 20201201 [XXXXX] Appel de la méthode FilterLstSelectAvailableForSelect
            DataTable dtColumn = ThisConsultation.FilterLstSelectAvailableForSelect();

            if (dtColumn.Rows.Count> 0)
            {
                int extLiIndex;
                ListItem listItem;
                string lastHeader = string.Empty;
                bool isAlternatingTable = false;
                
                OptionGroupListBox optGroupList = null;
                if (isOptGroupList)
                    optGroupList = (OptionGroupListBox)lbCtrlAvailable;
                
                if (lbCtrlAvailable != null)
                {
                    foreach (DataRow row in dtColumn.Select(null, "POSITION, DISPLAYNAME"))
                    {
                        // 20090929 RD / Ne pas charger les colonnes de type Text et Image pour le Tri
                        string dataType = row["DATATYPE"].ToString();
                        
                        if (selectionType != LstSelectionTypeEnum.SORT ||
                            (false == TypeData.IsTypeImage(dataType) && false == TypeData.IsTypeText(dataType)))
                        {
                            string itemText = row["DISPLAYNAME"].ToString();
                            string itemValue = row["TABLENAME"].ToString() + ";" + row["COLUMNNAME"].ToString() + ";" + row["ALIAS"].ToString();

                            //PL 20121022 Add ALIASHEADER
                            //currentHeader = row["ALIASDISPLAYNAME"].ToString();
                            string currentHeader = row["ALIASHEADER"].ToString();
                            ExtendedListItem extLi;
                            if (lastHeader != currentHeader)
                            {
                                lastHeader = currentHeader;

                                if (isOptGroupList)
                                {
                                    string itemTextGroup = string.Empty;
                                    if (ThisConsultation.IsMultiTable(SessionTools.CS))
                                        itemTextGroup = row["ALIASDISPLAYNAME"].ToString();
                                    listItem = new ListItem(itemTextGroup, itemTextGroup);

                                    lbCtrlAvailable.Items.Add(listItem);

                                    extLiIndex = optGroupList.ExtendedItems.Count - 1;
                                    extLi = optGroupList.ExtendedItems[extLiIndex];
                                    extLi.GroupingType = ListItemGroupingTypeEnum.New;
                                    extLi.GroupingText = extLi.Text;
                                }
                                else
                                {
                                    isAlternatingTable = !isAlternatingTable;
                                }

                                if ((false == isOptGroupList) && ThisConsultation.IsMultiTable(SessionTools.CS))
                                    itemText = "[" + row["ALIASDISPLAYNAME"].ToString() + "] " + itemText;
                            }
                            itemText = itemText.Replace(Cst.HTMLBreakLine, " ");//Replace <br/> by space

                            listItem = new ListItem(itemText, itemValue);
                            
                            if ((false == isOptGroupList) && ThisConsultation.IsMultiTable(SessionTools.CS) && isAlternatingTable)
                                listItem.Attributes.Add("class", "roundDef");

                            lbCtrlAvailable.Items.Add(listItem);

                            if (isOptGroupList)
                            {
                                listItem.Attributes.Add("class", EFSCssClass.DropDownListCaptureLight);
                                extLiIndex = optGroupList.ExtendedItems.Count - 1;
                                extLi = optGroupList.ExtendedItems[extLiIndex];
                                extLi.GroupingType = ListItemGroupingTypeEnum.Inherit;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void LoadListBoxSelected()
        {
            DataTable dtSelectedCol;
            //            
            if (selectionType == LstSelectionTypeEnum.SORT)
                dtSelectedCol = ThisConsultation.LoadLstOrderBy(SessionTools.CS);
            else
                dtSelectedCol = ThisConsultation.LoadLstSelectedCol(SessionTools.CS, 0);

            if (dtSelectedCol.Rows.Count > 0)
            {
                ListBox lbCtrlSelected = (ListBox)PlhMain.FindControl(lbSelected);
                ListBox lbCtrlGroupBy = (ListBox)PlhMain.FindControl(lbGroupBy);

                // MF 20120430 ruptures with groupingset
                DropDownList ddlGroupingSet = (DropDownList)PlhMain.FindControl(lbGroupingSet);
                Cst.GroupingSet groupingSet = default;
                
                if (lbCtrlSelected != null)
                {
                    foreach (DataRow row in dtSelectedCol.Select())
                    {
                        string prefixe = string.Empty; 
                        if (selectionType == LstSelectionTypeEnum.SORT)
                        {
                            if (row["ASCDESC"].ToString() != "DESC")
                                prefixe = "ASC -";
                            else
                                prefixe = "DESC-";
                        }
                            
                        string itemText = row["DISPLAYNAME"].ToString();
                        //string itemValue = prefixe + row["IDLSTCOLUMN"].ToString() + ";" + row["ALIAS"].ToString();
                        string itemValue = prefixe + row["TABLENAME"].ToString() + ";" + row["COLUMNNAME"].ToString() + ";" + row["ALIAS"].ToString();
                        
                        if (ThisConsultation.IsMultiTable(SessionTools.CS))
                            itemText = prefixe + "[" + row["ALIASDISPLAYNAME"].ToString() + "] " + itemText;
                        else
                            itemText = prefixe + itemText;
                        
                        //PL 20110623 Remove <br/> by space
                        itemText = itemText.Replace(Cst.HTMLBreakLine, " ");
                        
                        ListItem li = new ListItem(itemText, itemValue);
                        //li.Attributes.Add("class", EFSCssClass.Capture);
                        li.Attributes.Add("class", EFSCssClass.DropDownListCaptureLight);
                        lbCtrlSelected.Items.Add(li);
                        
                        if (selectionType == LstSelectionTypeEnum.SORT)
                        {
                            bool isGroupBy = BoolFunc.IsTrue(row["ISGROUPBY"]);
                            
                            if (isGroupBy)
                            {
                                ListItem liGB = new ListItem(itemText.Substring(5), itemValue.Substring(5));
                                //liGB.Attributes.Add("class", EFSCssClass.Capture);
                                liGB.Attributes.Add("class", EFSCssClass.DropDownListCaptureLight);
                                lbCtrlGroupBy.Items.Add(liGB);
                            }

                            // UNDONE MF 20120430, the GROUPINGSET column as well as the previous one ISGROUPINGSET has been defined on the 
                            //  LSTORDERBY table, this data model take us to keep trace of the value for each group by element defined by the user.
                            //  Think to move this column on upper order table like as LSTTEMPLATE

                            // MF 20120430 ruptures with groupingset
                            groupingSet = (Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET")) | groupingSet;
                        }
                    }

                    if (selectionType == LstSelectionTypeEnum.SORT)
                    {
                        // MF 20120430 ruptures with groupingset
                        if (null != ddlGroupingSet)
                            ddlGroupingSet.SelectedValue = Cst.CastGroupingSetToDDLValue(groupingSet);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedValue"></param>
        /// <param name="Tbl_LSTSELECTorLSTORDERBY"></param>
        /// <param name="alGroupBy"></param>
        /// FI 20200602 [25370] Add method
        private void InsertLstSelectOrLstOrder(string[] selectedValue, Cst.OTCml_TBL Tbl_LSTSELECTorLSTORDERBY, ArrayList alGroupBy)
        {
            string CS = SessionTools.CS;
            //récupération de l'indice max
            int max = selectedValue.GetLength(0);

            string[] sTABLENAME = new string[max];
            string[] sCOLUMNNAME = new string[max];
            string[] sALIAS = new string[max];

            string sqlQuery = SQLCst.INSERT_INTO_DBO + Tbl_LSTSELECTorLSTORDERBY.ToString() + @" (IDLSTCONSULT,IDLSTTEMPLATE,IDA,TABLENAME,COLUMNNAME,ALIAS,POSITION";

            if (selectionType == LstSelectionTypeEnum.SORT)
                // MF 20120430 ruptures with groupingset
                sqlQuery += ",ASCDESC,ISGROUPBY,ISGROUPINGSET,GROUPINGSET";

            sqlQuery += @") values (@IDLSTCONSULT,@IDLSTTEMPLATE,@IDA,@TABLENAME,@COLUMNNAME,@ALIAS,@POSITION";

            if (selectionType == LstSelectionTypeEnum.SORT)
                // MF 20120430 ruptures with groupingset
                sqlQuery += ",@ASCDESC,@ISGROUPBY,@ISGROUPINGSET,@GROUPINGSET";

            sqlQuery += @")";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDLSTCONSULT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            parameters.Add(new DataParameter(CS, "IDLSTTEMPLATE", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            parameters.Add(new DataParameter(CS, "IDA", DbType.Int32));
            //parameters.Add(new DataParameter(CS, "IDLSTCOLUMN", DbType.Int32));
            parameters.Add(new DataParameter(CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN));
            parameters.Add(new DataParameter(CS, "COLUMNNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN));
            parameters.Add(new DataParameter(CS, "ALIAS", DbType.AnsiString, 16));
            parameters.Add(new DataParameter(CS, "POSITION", DbType.Int32));
            //
            if (selectionType == LstSelectionTypeEnum.SORT)
            {
                parameters.Add(new DataParameter(CS, "ASCDESC", DbType.AnsiString, 4));
                parameters.Add(new DataParameter(CS, "ISGROUPBY", DbType.Boolean));
                parameters.Add(new DataParameter(CS, "ISGROUPINGSET", DbType.Boolean));
                // MF 20120430 ruptures with groupingset
                parameters.Add(new DataParameter(CS, "GROUPINGSET", DbType.Int32));
            }

            for (int i = 0; i <= max - 1; i++)
            {
                if ((selectedValue[i] != null) && (selectedValue[i].Length > 1))
                {
                    //string[] splitValues = selectedValue[i].Split(";".ToCharArray(), 2);
                    //sIDLSTCOLUMN[i] = splitValues[0];
                    string[] splitValues = selectedValue[i].Split(";".ToCharArray(), 3);//GLOPLOK mettre un exemple de contenu (surtout pour COLUMNNAME/DESC)
                    sTABLENAME[i] = splitValues[0];
                    sCOLUMNNAME[i] = splitValues[1];
                    sALIAS[i] = splitValues[2];

                    //if (sIDLSTCOLUMN[i] != null && sALIAS[i] != null)
                    if (sCOLUMNNAME[i] != null && sALIAS[i] != null)
                    {
                        parameters["IDLSTCONSULT"].Value = idLstConsult;
                        parameters["IDLSTTEMPLATE"].Value = idLstTemplate;
                        parameters["IDA"].Value = idA;
                        //parameters["IDLSTCOLUMN"].Value = sIDLSTCOLUMN[i];
                        parameters["TABLENAME"].Value = sTABLENAME[i];
                        parameters["COLUMNNAME"].Value = sCOLUMNNAME[i];
                        parameters["ALIAS"].Value = sALIAS[i];
                        parameters["POSITION"].Value = i;

                        if (parameters.Contains("ASCDESC"))
                        {
                            //parameters["IDLSTCOLUMN"].Value = sIDLSTCOLUMN[i].Substring(5);
                            parameters["TABLENAME"].Value = sTABLENAME[i].Substring(5);//Cas particulier où cette donnée est préfixée par "ASC -" ou "DESC-"
                            //if (sIDLSTCOLUMN[i].Substring(0, 4) == "DESC")
                            if (sTABLENAME[i].Substring(0, 4) == "DESC")
                                parameters["ASCDESC"].Value = "DESC";
                            else
                                parameters["ASCDESC"].Value = "ASC";
                        }

                        if (parameters.Contains("ISGROUPBY") && null != alGroupBy)
                        {
                            string itemSortBy = selectedValue[i].Substring(5);

                            if (alGroupBy.Contains(itemSortBy))
                            {
                                parameters["ISGROUPBY"].Value = true;
                                parameters["ISGROUPINGSET"].Value = false;
                            }
                            else
                            {
                                parameters["ISGROUPBY"].Value = false;
                                parameters["ISGROUPINGSET"].Value = false;
                            }
                            
                            DropDownList ddlGroupingSet = (DropDownList)PlhMain.FindControl(lbGroupingSet);
                            // MF 20120509 Bug solved, ticket 17759 item 3 ...
                            //  ... on data bindind when DDL empty value is selected: "DDLGrouping set selected value is out fo range".
                            // MF 20120430 ruptures with groupingset
                            if (!String.IsNullOrEmpty(ddlGroupingSet.SelectedValue))
                            {
                                parameters["GROUPINGSET"].Value = ddlGroupingSet.SelectedValue;
                            }
                            else
                            {
                                parameters["GROUPINGSET"].Value = Cst.CastGroupingSetToDDLValue(default);
                            }

                        }

                        DataHelper.ExecuteNonQuery(CS, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
                    }
                }
            }
        }

        #endregion
    }
}

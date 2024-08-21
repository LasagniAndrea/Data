#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using EFS.Rights;
using EfsML.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#endregion using directives


/// <summary>
/// Description résumée de ListBase
/// </summary>
public class ListBase : ContentPageBase
{
    private const string DDL_SEP = ",";
    public string sessionName_LstColumn;

    #region Members
    protected HiddenField _hiddenFieldGUID;
    public string urlThis;
    public string dataFrom__EVENTTARGET;
    public string dataFrom__EVENTARGUMENT;
    public bool isClose;
    public bool isReloadPage;
    public string initialNameLstTemplate;
    public string newNameLstTemplate;
    public bool isAllowedToOverWrite = true;
    public LstConsultData consult;
    protected string defaultButton;
    /// <summary>
    /// Liste des groupes de colonnes disponibles
    /// </summary>
    public List<Pair<string, string>> lstGroup { set; get; }
    /// <summary>
    /// Dictionnaire des colonnes disponibles par groupe
    /// </summary>
    public Dictionary<string, List<ListFilterTools.ListColumn>> dicColumn { set; get; }
    /// <summary>
    /// Liste des opérateurs disponibles
    /// </summary>
    public List<Pair<OperandEnum, string>> lstOperand { set; get; }

    /// <summary>
    /// Dictionnaire des colonnes disponibles (MODE XML) TO DELETE
    /// </summary>
    public List<ListItem> lstColumns { set; get; }

    #endregion

    public bool IsReloadCriteria 
    {
        get
        {
            //return StrFunc.IsFilled(dataFrom__EVENTTARGET) && dataFrom__EVENTTARGET.StartsWith(ContentPlaceHolder_UniqueID + "uc_lstdisplay");
            return StrFunc.IsFilled(dataFrom__EVENTTARGET) && 
                (dataFrom__EVENTTARGET.EndsWith("btnOk") || dataFrom__EVENTTARGET.EndsWith("btnOkAndSave") || dataFrom__EVENTTARGET.EndsWith("btnCancel"));
        } 
    }
    public bool isColumnByGroup { set; get; }
    public bool isXMLSource { set; get; }
    public bool isSQLSource {get {return (false == isXMLSource);}}
    public string idMenu { set; get; }
    public string parentGUID { set; get; }
    protected bool isConsultation { set; get; }
    /// <summary>
    /// Représente la consultation courante
    /// </summary>
    public string idLstConsult { set; get; }
    /// <summary>
    /// Représente la modèle courant de la consultation
    /// </summary>
    public string idLstTemplate { set; get; }
    /// <summary>
    /// Représente le propriétaire du modèle courant
    /// </summary>
    public int idA { set; get; }


    protected virtual PlaceHolder plhMain
    {
        get { return null; }
    }


    public ListBase()
	{
		//
		// TODO: Add constructor logic here
		//
	}


    #region GetTotalRows
    /// <summary>
    /// get the higher ID of the editable rows we created
    /// </summary>
    /// <returns>higher ID for rows</returns>
    public virtual int GetTotalRows
    {
        get
        {
            return 0;
        }
    }
    #endregion GetTotalRows

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        this.ID = "ListBase";
        AbortRessource = true;

        dataFrom__EVENTTARGET = Request.Params["__EVENTTARGET"];
        dataFrom__EVENTARGUMENT = Request.Params["__EVENTARGUMENT"];
        isReloadPage = (dataFrom__EVENTTARGET == "PAGE" && dataFrom__EVENTARGUMENT == "RELOAD");

        //Identifiant unique sur la page [on ne prend pas pagebase.GUID car ici est non valorisée lors d'un postback]
        if (null != this.MasterPage_ContentPlaceHolder)
        {
            _hiddenFieldGUID = new HiddenField();
            _hiddenFieldGUID.ID = "__GUID";
            this.MasterPage_ContentPlaceHolder.Controls.Add(_hiddenFieldGUID);
            if (StrFunc.IsFilled(Request.Params["GUID"]))
                _hiddenFieldGUID.Value = Request.Params["GUID"];
            else
                _hiddenFieldGUID.Value = (false == IsPostBack) ? GUID : Request.Form[_hiddenFieldGUID.UniqueID];
        }
    }


    public virtual void SetKeyAndLoadTemplate()
    {
        //Getting IDLSTCONSULT, IDA, IDLSTTEMPLATE from queryString, and setting ViewState custom datas				
        isConsultation = (Request.QueryString["T1"] == Cst.ListType.Consultation.ToString());
        idLstConsult = Server.UrlDecode(Request.QueryString["C"]);
        idLstTemplate = Server.UrlDecode(Request.QueryString["T"]);
        idA = IntFunc.IntValue(Request.QueryString["A"]);
        if (0 == idA)
            idA = SessionTools.User.idA;
        idMenu = Request.QueryString["IDMenu"];
        parentGUID = Request.QueryString["ParentGUID"];
        consult = new LstConsultData(this.CS, idLstConsult, string.Empty);
        consult.LoadTemplate(CS, idLstTemplate, idA);
        isColumnByGroup = consult.IsMultiTable(CS);
        isXMLSource = RepositoryWeb.IsReferential(idLstConsult);
        sessionName_LstColumn = Request.QueryString["DDL"];
    }
    public virtual void ReloadGridView()
    {
    }

    protected override void OnPreRender(EventArgs e)
    {
        if ((!IsPostBack) || isReloadPage)
            LoadDataControls();
        DataBind();
        base.OnPreRender(e);
    }




    /// <summary>
    /// 
    /// </summary>
    /// <param name="savedState"></param>
    protected override void LoadViewState(object savedState)
    {
        base.LoadViewState(savedState);
        object[] viewState = (object[])savedState;
        idLstConsult = (string)viewState[2];
        idLstTemplate = (string)viewState[3];
        idA = (int)viewState[4];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override object SaveViewState()
    {
        if (HttpContext.Current == null)
            return null;
        //
        object[] viewState = new object[5];
        viewState[0] = ((Array)base.SaveViewState()).GetValue(0);
        viewState[1] = ((Array)base.SaveViewState()).GetValue(1);
        viewState[2] = idLstConsult;
        viewState[3] = idLstTemplate;
        viewState[4] = idA;
        return viewState;

    }
    /// <summary>
    /// Override by LstCriteria
    /// </summary>
    protected virtual void LoadDataControls() { }

    protected virtual void SaveData() { }

    /// <summary>
    /// Liste des colonnes disponibles
    /// </summary>
    /// <param name="pPlhAvailable"></param>
    public void LoadBulletedListAvailable(PlaceHolder pPlhColumnAvailable, LstSelectionTypeEnum pLstSelectionType)
    {
        DropDownList ddlGroup = null;
        if (isColumnByGroup)
        {
            ddlGroup = new DropDownList();
            ddlGroup.ID = "ddlGroupAvailable";
            ddlGroup.CssClass = "form-control input-xs";
        }

        HtmlGenericControl ul = new HtmlGenericControl("ul");
        ul.ID = "blColumnAvailable";
        ul.Attributes.Add("class", "list-group connected");

        lstGroup.ForEach(group =>
        {
            // Alimentation de la dropdown Group
            if (isColumnByGroup)
                ddlGroup.Items.Add(new ListItem(group.Second, group.First));

            // Alimentation de la bulletedList Column
            dicColumn[group.First].ForEach(column =>
            {
                HtmlGenericControl liItem = new HtmlGenericControl("li");
                liItem.Attributes.Add("grp", group.First);
                liItem.Attributes.Add("grpv", group.Second);
                if (column.isHideCriteria)
                    liItem.Attributes.Add("nocrit", "1");
                if (column.isMandatory)
                    liItem.Attributes.Add("mdty", "1");
                liItem.Attributes.Add("data-type", column.dataType);
                liItem.Attributes.Add("value", column.tableName + ";" + column.columnName + ";" + column.alias);
                liItem.InnerText = column.displayName;
                ul.Controls.Add(liItem);

            });
        });

        if (isColumnByGroup)
        {
            Panel pnlGroup = new Panel();
            pnlGroup.CssClass = "form-group form-group-xs";

            Label lblGroup = new Label();
            lblGroup.Text = Ressource.GetString("lblGroupList");
            lblGroup.CssClass = "col-sm-4 control-label";
            pnlGroup.Controls.Add(lblGroup);

            Panel pnl = new Panel();
            pnl.CssClass = "col-sm-8";
            pnl.Controls.Add(ddlGroup);
            pnlGroup.Controls.Add(pnl);

            pPlhColumnAvailable.Controls.Add(pnlGroup);
        }

        pPlhColumnAvailable.Controls.Add(ul);
    }

    /// <summary>
    /// Liste des colonnes sélectionnées
    /// </summary>
    /// <param name="pPlhSelected"></param>
    public void LoadBulletedListSelected(PlaceHolder pPlhColumnSelected, LstSelectionTypeEnum pLstSelectionType)
    {
        HtmlGenericControl ul = new HtmlGenericControl("ul");
        ul.ID = "blColumn" + ((LstSelectionTypeEnum.SORT == pLstSelectionType) ? "Sorted" : "Displayed") ;
        ul.Attributes.Add("class", "list-group connected");

        DataTable dtSelectedCol;

        BS_ColumnBase bsColumn = null;
        if (pLstSelectionType == LstSelectionTypeEnum.SORT)
        {
            dtSelectedCol = consult.LoadLstOrderBy(CS);
            bsColumn = new BS_ColumnSorted(isColumnByGroup, pLstSelectionType);
        }
        else
        {
            dtSelectedCol = consult.LoadLstSelectedCol(CS, 0);
            bsColumn = new BS_ColumnSelected(isColumnByGroup, pLstSelectionType);
        }

        if (dtSelectedCol.Rows.Count > 0)
        {
            foreach (DataRow row in dtSelectedCol.Select())
            {
                string group = row["ALIASDISPLAYNAME"].ToString();
                string columnDisplayName = row["DISPLAYNAME"].ToString().Replace(Cst.HTMLBreakLine, Cst.Space);
                string tableName = row["TABLENAME"].ToString();
                string columnName = row["COLUMNNAME"].ToString();
                string columnAlias = row["ALIAS"].ToString();

                bool isGroupBy = false;
                string sort = string.Empty;
                if (pLstSelectionType == LstSelectionTypeEnum.SORT)
                {
                    isGroupBy = BoolFunc.IsTrue(row["ISGROUPBY"]);
                    sort = row["ASCDESC"].ToString();
                }

                bsColumn.AddItem(group, tableName, columnDisplayName, columnName, columnAlias, isGroupBy, sort);

                if (pLstSelectionType == LstSelectionTypeEnum.SORT)
                    bsColumn.SetGroupingSetValue(Cst.CastDataColumnToGroupingSet(row, "GROUPINGSET"));
            }
        }
        else
        {
            pPlhColumnSelected.Controls.Add(ul);

        }
        if (pLstSelectionType == LstSelectionTypeEnum.SORT)
        {
            DropDownList ddlGroupingSet = pPlhColumnSelected.FindControl("ddlGroupingSet") as DropDownList;
            if (null != ddlGroupingSet)
                ddlGroupingSet.SelectedValue = Cst.CastGroupingSetToDDLValue(((BS_ColumnSorted)bsColumn).groupingSet);
        }
        pPlhColumnSelected.Controls.Add(bsColumn);
    }

    //
    /// <summary>
    /// Verify the user rights to modify the current LST template. 
    /// When the user has no enough rights to modify the template, the record buttons (if they exist) will be disabled and
    /// a warning message will be displayed on the page.
    /// </summary>
    public void VerifyUserRightsLstTemplate()
    {
        if (consult == null || consult.template == null)
        {
            throw new ArgumentException(@"Consultation object or relate tempate is null. 
                        to verify user rights with VerifyUserRightsToModLstTemplate you need to load a template.");
        }

        bool hasUserRightModify = true;
        bool hasUserRightSave = true;

        if (SessionTools.User.idA != consult.template.IDA)
        {
            hasUserRightModify = consult.template.HasUserRight(CS, SessionTools.User, RightsTypeEnum.MODIFY);
            hasUserRightSave = consult.template.HasUserRight(CS, SessionTools.User, RightsTypeEnum.SAVE);
        }

        WCTooltipLabel lblMissingModPermissions = ControlsTools.GetLabelMissingUserRightsLstTemplate(plhMain);

        if (!hasUserRightSave)
        {
            HtmlButton btnOkAndSave = plhMain.FindControl("btnOkAndSave") as HtmlButton;
            if (null != btnOkAndSave)
                btnOkAndSave.Attributes.Add("disabled" , "disabled");

            if (null != lblMissingModPermissions)
            {
                lblMissingModPermissions.Text = String.Format(Ressource.GetString("lblMissingUserSaveRightsLstTemplate"),
                    consult.template.DISPLAYNAME);
                lblMissingModPermissions.Visible = true;
            }
        }

        if (!hasUserRightModify)
        {
            HtmlButton btnOk = plhMain.FindControl("btnOk") as HtmlButton;
            if (null != btnOk)
                btnOk.Attributes.Add("disabled", "disabled");

            if (null != lblMissingModPermissions)
            {
                // the previous label lblMissingUserSaveRightsLstTemplate will be overwritten by the next one, being more critical
                lblMissingModPermissions.Text = String.Format(Ressource.GetString("lblMissingUserModRightsLstTemplate"),
                    consult.template.DISPLAYNAME);
                lblMissingModPermissions.Visible = true;
            }

        }
    }

    public void InitializeListForControls()
    {
        #region Groups-Columns
        if (isSQLSource)
            LoadSQLColumnAvailable();
        else
            LoadXMLColumnAvailable();

        #endregion Groups-Columns
        #region Operand
        lstOperand = new List<Pair<OperandEnum, string>>();
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.@checked, Ressource.GetString("checked")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.@unchecked, Ressource.GetString("unchecked")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.equalto, Ressource.GetString("equalto")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.notequalto, Ressource.GetString("notequalto")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.greaterthan, Ressource.GetString("greaterthan")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.lessthan, Ressource.GetString("lessthan")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.greaterorequalto, Ressource.GetString("greaterorequalto")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.lessorequalto, Ressource.GetString("lessorequalto")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.contains, Ressource.GetString("contains")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.notcontains, Ressource.GetString("notcontains")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.startswith, Ressource.GetString("startswith")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.endswith, Ressource.GetString("endswith")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.like, Ressource.GetString("like")));
        lstOperand.Add(new Pair<OperandEnum, string>(OperandEnum.notlike, Ressource.GetString("notlike")));
        #endregion Operand
    }

    /// <summary>
    /// Chargement des Colonnes disponibles (Mode SQL)
    /// </summary>
    private void LoadSQLColumnAvailable()
    {
        lstGroup = new List<Pair<string, string>>();
        dicColumn = new Dictionary<string, List<ListFilterTools.ListColumn>>();
        consult.LoadLstSelectAvailable(CS);
        if (consult.dtLstSelectAvailable.Rows.Count > 0)
        {
            string groupText = string.Empty;
            string groupValue = string.Empty;
            string columnText = string.Empty;
            string columnValue = string.Empty;

            List<ListFilterTools.ListColumn> columns = null;
            foreach (DataRow row in consult.dtLstSelectAvailable.Select(null, "POSITION, ALIASDISPLAYNAME, DISPLAYNAME"))
            {
                // Groupes
                groupValue = row["ALIASHEADER"].ToString();
                groupText = row["ALIASDISPLAYNAME"].ToString();

                if (false == lstGroup.Exists(item => item.First == groupValue))
                    lstGroup.Add(new Pair<string, string>(groupValue, groupText));

                // Colonnes
                columnText = row["DISPLAYNAME"].ToString().Replace(Cst.HTMLBreakLine, Cst.Space);
                columnValue = row["TABLENAME"].ToString() + ";" + row["COLUMNNAME"].ToString() + ";" + row["ALIAS"].ToString();
                string alias = row["ALIAS"].ToString();
                string columnName = row["COLUMNNAME"].ToString();
                string tableName = row["TABLENAME"].ToString();

                if (false == dicColumn.ContainsKey(groupValue))
                    dicColumn.Add(groupValue, new List<ListFilterTools.ListColumn>());

                columns = dicColumn[groupValue];
                if (false == columns.Exists(column => (column.tableName == tableName) && (column.columnName == columnName) && (column.alias == alias)))
                {
                    ListFilterTools.ListColumn column = new ListFilterTools.ListColumn();
                    column.tableName = tableName;
                    column.columnName = columnName;
                    column.group = groupValue;
                    column.alias = alias;
                    column.isMandatory = Convert.ToBoolean(row["ISMANDATORY"]);
                    column.isHideCriteria = (DBNull.Value != row["COLUMNXML"]) && (0 < row["COLUMNXML"].ToString().IndexOf(@"<IsHideInCriteria>true"));
                    column.dataType = row["DATATYPE"].ToString();
                    column.displayName = row["DISPLAYNAME"].ToString().Replace(Cst.HTMLBreakLine, Cst.Space); 
                    column.dataType = row["DATATYPE"].ToString();
                    column.aliasDisplayName = row["ALIASDISPLAYNAME"].ToString();
                    columns.Add(column);
                    //columns.Add(new Pair<string, string>(columnValue, columnText));
                }
            }
        }
    }

    /// <summary>
    /// Chargement des Colonnes disponibles (Mode XML)
    /// </summary>
    private void LoadXMLColumnAvailable()
    {
        dicColumn = new Dictionary<string, List<ListFilterTools.ListColumn>>();

        List<ListFilterTools.ListColumn> lstColumnMandatories = new List<ListFilterTools.ListColumn>();
        #region Load d'éventuels critères obligatoires (LSTCONSULTWHERE)
        string sqlSelect = @"select ALIAS, TABLENAME, COLUMNNAME
        from dbo.LSTCONSULTWHERE
        where (IDLSTCONSULT = @IDLSTCONSULT)
        order by POSITION" + Cst.CrLf;

        DataParameters dataParameters = new DataParameters();
        dataParameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDLSTCONSULT), idLstConsult);

        QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect, dataParameters);
        IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.query, dataParameters.GetArrayDbParameter());

        //Récupération des colonnes obligatoires
        while (dr.Read())
        {
            ListFilterTools.ListColumn column = new ListFilterTools.ListColumn();
            column.alias = dr["ALIAS"].ToString();
            column.tableName = dr["TABLENAME"].ToString();
            column.columnName = dr["COLUMNNAME"].ToString();
            lstColumnMandatories.Add(column);
        }
        if (null != dr)
        {
            dr.Dispose();
        }

        if (StrFunc.IsFilled(sessionName_LstColumn))
        {
            List<ListFilterTools.ListColumn> lstColumns = Session[sessionName_LstColumn] as List<ListFilterTools.ListColumn>;
            if (null != lstColumns) 
            {
                lstColumns.ForEach(column =>
                {
                    if (lstColumnMandatories.Exists(match=> (match.alias == column.alias) && 
                                                   (match.tableName == column.tableName) &&
                                                   (match.columnName == column.columnName)))
                        column.isMandatory = true;

                });
                dicColumn.Add("ALL",lstColumns);
            }
        }
        #endregion

    }

    #region SaveDisplayedOrSortedData
    public void SaveDisplayedOrSortedData(PlaceHolder pPlhContainer, LstSelectionTypeEnum pSelectionType)
    {
        List<string> lstValue = null;

        consult.LoadTemplate(CS, idLstTemplate, idA);

        string sqlQuery = string.Empty;
        int groupingSet = 0;

        DataParameters parameters = new DataParameters();
        parameters.Add(new DataParameter(CS, "IDLSTCONSULT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), idLstConsult);
        parameters.Add(new DataParameter(CS, "IDLSTTEMPLATE", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), idLstTemplate);
        parameters.Add(new DataParameter(CS, "IDA", DbType.Int32), idA);
        parameters.Add(new DataParameter(CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN));
        parameters.Add(new DataParameter(CS, "COLUMNNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN));
        parameters.Add(new DataParameter(CS, "ALIAS", DbType.AnsiString, 16));
        parameters.Add(new DataParameter(CS, "POSITION", DbType.Int32));

        switch (pSelectionType)
        {
            case LstSelectionTypeEnum.DISP:

                RepositoryWeb.DeleteChild(CS, Cst.OTCml_TBL.LSTSELECT, idLstConsult, idLstTemplate, idA, false);

                sqlQuery = @"insert into dbo.LSTSELECT (IDLSTCONSULT, IDLSTTEMPLATE, IDA, TABLENAME, COLUMNNAME, ALIAS, POSITION) 
                values (@IDLSTCONSULT, @IDLSTTEMPLATE, @IDA, @TABLENAME, @COLUMNNAME, @ALIAS, @POSITION)";

                //on récupère les données selectionnées dans le contrôle caché           
                HtmlInputHidden hidLstDisplay = (HtmlInputHidden)MasterPage_ContentPlaceHolder.FindControl("hidLstDisplay");
                lstValue = hidLstDisplay.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                break;

            case LstSelectionTypeEnum.SORT:

                RepositoryWeb.DeleteChild(CS, Cst.OTCml_TBL.LSTORDERBY, idLstConsult, idLstTemplate, idA, false);

                sqlQuery = @"insert into dbo.LSTORDERBY (IDLSTCONSULT, IDLSTTEMPLATE, IDA, TABLENAME, COLUMNNAME, ALIAS, POSITION, ASCDESC, ISGROUPBY, ISGROUPINGSET, GROUPINGSET) 
                values (@IDLSTCONSULT, @IDLSTTEMPLATE, @IDA, @TABLENAME, @COLUMNNAME, @ALIAS, @POSITION, @ASCDESC, @ISGROUPBY, @ISGROUPINGSET, @GROUPINGSET)";

                parameters.Add(new DataParameter(CS, "ASCDESC", DbType.AnsiString, 4));
                parameters.Add(new DataParameter(CS, "ISGROUPBY", DbType.Boolean));
                parameters.Add(new DataParameter(CS, "ISGROUPINGSET", DbType.Boolean));
                parameters.Add(new DataParameter(CS, "GROUPINGSET", DbType.Int32));

                DropDownList ddlGroupingSet = (DropDownList)pPlhContainer.FindControl("ddlGroupingSet");
                groupingSet = StrFunc.IsFilled(ddlGroupingSet.SelectedValue) ? Convert.ToInt32(ddlGroupingSet.SelectedValue) : Convert.ToInt32(default(Cst.GroupingSet));

                //on récupère les données selectionnées dans le contrôle caché           
                HtmlInputHidden hidLstSorted = (HtmlInputHidden)MasterPage_ContentPlaceHolder.FindControl("hidLstSorted");
                lstValue = hidLstSorted.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                break;
        }


        string tableName = string.Empty;
        string columnName = string.Empty;
        string alias = string.Empty;

        int i = 0;

        lstValue.ForEach(selected =>
            {
                string[] splitValues = selected.Split(new char[] { ';' }, 5, StringSplitOptions.RemoveEmptyEntries);
                tableName = splitValues[0];
                columnName = splitValues[1];
                alias = splitValues[2];

                if (StrFunc.IsFilled(columnName) && StrFunc.IsFilled(alias))
                {
                    parameters["TABLENAME"].Value = tableName;
                    parameters["COLUMNNAME"].Value = columnName;
                    parameters["ALIAS"].Value = alias;
                    parameters["POSITION"].Value = i;

                    if (pSelectionType == LstSelectionTypeEnum.SORT)
                    {
                        parameters["ASCDESC"].Value = Convert.ToBoolean(splitValues[3])? "DESC":"ASC";
                        parameters["ISGROUPBY"].Value = Convert.ToBoolean(splitValues[4]);
                        parameters["ISGROUPINGSET"].Value = false;
                        parameters["GROUPINGSET"].Value = groupingSet;
                    }

                    QueryParameters qry = new QueryParameters(CS, sqlQuery, parameters);
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                }
                i++;
            });
    }
    #endregion SaveDisplayedOrSortedData

    #region SaveFilteredData
    public void SaveFilteredData(PlaceHolder pPlhContainer, bool pIsEnabledLstWhere)
    {
        consult.LoadTemplate(CS, idLstTemplate, idA);

        if (false == consult.template.isTemporary)
        {
            #region creation d'un modèle temporaire
            if (consult.template.IsUserOwner(SessionTools.Collaborator_IDA))
            {
                // si le user est proprietaire, on créé un modele temporaire de son modele
                idLstTemplate = consult.CreateCopyTemporaryTemplate(CS, idLstTemplate);
                idA = SessionTools.User.idA;
            }
            else
            {
                bool hasUserRightModify = consult.template.HasUserRight(CS, SessionTools.User, RightsTypeEnum.MODIFY);
                if (hasUserRightModify)
                {
                    // si le user n'est pas proprietaire mais qu'il a les droits de modification, on cree un modele temporaire du modele original
                    idLstTemplate = consult.CreateCopyTemporaryTemplate(CS, idLstTemplate, consult.template.IDA, consult.template.IDA);
                    idA = consult.template.IDA;
                }
                else
                {
                    // si le user n'est pas proprietaire et qu'il n'a pas les droits de modification, on cree une copie temporaire de ce modele
                    idLstTemplate = consult.CreateCopyTemporaryTemplate(CS, idLstTemplate, consult.template.IDA);
                    idA = SessionTools.User.idA;
                }
            }
            //Load the temporary copy
            consult.template.Load(CS, idLstConsult, idLstTemplate, idA);
            #endregion
        }
        consult.template.SetIsEnabledLstWhere(CS, idA, idLstTemplate, idLstConsult, pIsEnabledLstWhere);

        // Delete from table LSTWHERE
        RepositoryWeb.DeleteChild(CS, Cst.OTCml_TBL.LSTWHERE, idLstConsult, idLstTemplate, idA, false);

        bool isMandatory;
        bool isEnabled;
        string group = string.Empty;
        string tableName = string.Empty;
        string columnName = string.Empty;
        string alias = string.Empty;
        string operand = string.Empty;
        string dataType = string.Empty;

        string data;

        #region SQL Insert into LSTWHERE

        #region SQLQuery / DataParameter
        string SQLQuery = @"insert into dbo.LSTWHERE (
            IDLSTCONSULT, IDLSTTEMPLATE, IDA,IDLSTCOLUMN, TABLENAME, COLUMNNAME, ALIAS,POSITION, OPERATOR, LSTVALUE, LSTIDVALUE, ISENABLED, ISMANDATORY) 
            select @IDLSTCONSULT, @IDLSTTEMPLATE, @IDA, @IDLSTCOLUMN, @TABLENAME, @COLUMNNAME, @ALIAS, @POSITION,
            case when @LSTVALUE is null then isnull(lcw.OPERATOR, @OPERATOR) else @OPERATOR end, isnull(@LSTVALUE, lcw.LSTVALUE), @LSTIDVALUE, @ISENABLED, @ISMANDATORY
            from dbo.LSTCONSULT lc
            left outer join dbo.LSTCONSULTWHERE lcw on (lcw.IDLSTCONSULT = lc.IDLSTCONSULT) and (lcw.POSITION = @POSITION)
            where (lc.IDLSTCONSULT = @IDLSTCONSULT)" + Cst.CrLf;

        DataParameters parameters = new DataParameters();
        parameters.Add(new DataParameter(CS, "ISMANDATORY", DbType.Boolean));
        parameters.Add(new DataParameter(CS, "ISENABLED", DbType.Boolean));
        parameters.Add(new DataParameter(CS, "IDLSTCONSULT", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
        parameters.Add(new DataParameter(CS, "IDLSTTEMPLATE", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN));
        parameters.Add(new DataParameter(CS, "IDA", DbType.Int32));
        parameters.Add(new DataParameter(CS, "IDLSTCOLUMN", DbType.Int32));
        parameters.Add(new DataParameter(CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_UNC_LEN));
        parameters.Add(new DataParameter(CS, "COLUMNNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN));
        parameters.Add(new DataParameter(CS, "ALIAS", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN));
        parameters.Add(new DataParameter(CS, "POSITION", DbType.Int32));
        parameters.Add(new DataParameter(CS, "OPERATOR", DbType.AnsiString, SQLCst.UT_OPERATOR_LEN));
        parameters.Add(new DataParameter(CS, "LSTVALUE", DbType.AnsiString, SQLCst.UT_LSTVALUE_LEN));
        parameters.Add(new DataParameter(CS, "LSTIDVALUE", DbType.AnsiString, SQLCst.UT_LSTVALUE_LEN));
        #endregion

        string listMandatoryColumn = string.Empty;

        int nbCriteria = GetTotalRows;

        //on récupère les données selectionnées dans le contrôle caché           
        // avec lstValue[0..n] = grp;mandatory;tableName;columnName;alias;data-type|...grp;mandatory;tableName;columnName;alias;data-type
        HtmlInputHidden hidLstFilter = (HtmlInputHidden) MasterPage_ContentPlaceHolder.FindControl("hidLstFilter");
        List<string> columnsSelectedInfo = hidLstFilter.Value.Split(new char[] { '|' }).ToList();

        for (int i = 0; i < nbCriteria; i++)
        {
            if (StrFunc.IsFilled(columnsSelectedInfo[i]))
            {
                string[] columnSelectedInfo = columnsSelectedInfo[i].Split(ListFilterTools.SEP.ToCharArray());
                BS_Operand bsOperand = pPlhContainer.FindControl(ListFilterTools.BS_OPERAND + i.ToString()) as BS_Operand;
                BS_Data bsData = pPlhContainer.FindControl(ListFilterTools.BS_DATA + i.ToString()) as BS_Data;

                if ((null != columnSelectedInfo) && (null != bsOperand) && (null != bsData))
                {
                    ListFilterTools.ListColumn column = null;
                    if (isSQLSource || (6 == columnSelectedInfo.Length))
                    {
                        group = columnSelectedInfo[0];
                        isMandatory = ("1" == columnSelectedInfo[1]);
                        dataType = columnSelectedInfo[5];

                        column = dicColumn[group].Find(item =>
                            (item.tableName == columnSelectedInfo[2]) && (item.columnName == columnSelectedInfo[3]) && (item.alias == columnSelectedInfo[4]));

                    }
                    else
                    {
                        group = columnSelectedInfo[0];
                        isMandatory = ("1" == columnSelectedInfo[1]);
                        dataType = columnSelectedInfo[4];
                        column = dicColumn[group].Find(item =>
                            (item.tableName == columnSelectedInfo[5]) && (item.columnName == columnSelectedInfo[6]) && (item.alias == columnSelectedInfo[3]));
                    }


                    if (null != column)
                    {
                        //20070522 PL Tip pour éviter de considérer une colonne mandatory, comme n fois mandatory
                        if (isMandatory)
                        {
                            if (listMandatoryColumn.IndexOf(column.tableName + ":" + column.columnName + "|") < 0)
                                listMandatoryColumn += column.tableName + ":" + column.columnName + "|";
                            else
                                isMandatory = false;
                        }

                        operand = bsOperand.ddlOperand.SelectedValue;

                        data = bsData.txtData.Text;
                        if (StrFunc.IsFilled(data))
                            data = LstConsultData.FormatLstValue2(CS, data, dataType, false, false, true, SessionTools.User.entity_IdA);

                        Control ctrl = pPlhContainer.FindControl(ListFilterTools.CHK_ACTIVE + i.ToString());
                        isEnabled = (null != ctrl) && ((HtmlInputCheckBox)ctrl).Checked;

                        parameters["ISMANDATORY"].Value = isMandatory;
                        parameters["ISENABLED"].Value = isEnabled;
                        parameters["IDLSTCONSULT"].Value = idLstConsult;
                        parameters["IDLSTTEMPLATE"].Value = idLstTemplate;
                        parameters["IDA"].Value = idA;
                        parameters["IDLSTCOLUMN"].Value = "-1";
                        parameters["TABLENAME"].Value = column.tableName;
                        parameters["COLUMNNAME"].Value = column.columnName;
                        parameters["ALIAS"].Value = column.alias;
                        parameters["POSITION"].Value = i;
                        parameters["OPERATOR"].Value = operand;

                        if (StrFunc.IsFilled(data))
                            parameters["LSTVALUE"].Value = data;
                        else
                            parameters["LSTVALUE"].Value = DBNull.Value;

                        parameters["LSTIDVALUE"].Value = DBNull.Value;

                        QueryParameters qry = new QueryParameters(CS, SQLQuery, parameters);
                        DataHelper.ExecuteNonQuery(CS, CommandType.Text, qry.query.ToString(), qry.parameters.GetArrayDbParameter());
                    }
                }
            }
        }

        #endregion SQL Insert into LSTWHERE

        RepositoryWeb.WriteTemplateSession(this.Page, idLstConsult, idLstTemplate, idA, parentGUID);
    }
    #endregion SaveFilteredData


    #region SaveTemplate
    public void SaveTemplate()
    {
        // on verifie les droits du user sur ce template
        bool hasUserRightSave = consult.template.HasUserRight(CS, SessionTools.User, RightsTypeEnum.SAVE);

        // Template temporaire et identifiant inchangé et (user est propriétaire ou disposant des droits pour enregister les modifs d'un template qui n'est pas le sien)
        if (consult.template.isTemporary && (newNameLstTemplate == initialNameLstTemplate)
            && (hasUserRightSave || consult.template.IsUserOwner(SessionTools.Collaborator_IDA)))
        {
            //il s'agit d'un [SAVE] : on lui enlève son statut 'temporaire'
            consult.template.Update(CS);
            consult.DuplicateTemplate(CS, consult.template.IDLSTTEMPLATE, consult.template.IDA,
                newNameLstTemplate, consult.template.IDA, true);

            consult.template.IDLSTTEMPLATE = newNameLstTemplate;
        }
        else
        {
            //Si l'identifiant a été modifié depuis l'ouverture ou ( si l'utilisateur n'en est pas le propriétaire et qu'il n'a pas les droits pour sauver )
            if ((newNameLstTemplate != initialNameLstTemplate) ||
                (!hasUserRightSave && !consult.template.IsUserOwner(SessionTools.Collaborator_IDA)))
            {
                //il s'agit d'un [SAVE AS]
                bool isTemporary = consult.template.isTemporary;
                string sourceIdLstTemplate;
                if (isTemporary)
                    sourceIdLstTemplate = consult.template.IDLSTTEMPLATE;
                else
                    sourceIdLstTemplate = initialNameLstTemplate;

                consult.template.IDLSTTEMPLATE = newNameLstTemplate;

                bool alreadyExists = false;
                //si on ne passe pas apres une demande confirmation pour overwrite (postback de confirm()), on verifie si le template existe
                if (!isAllowedToOverWrite)
                    alreadyExists = RepositoryWeb.ExistsTemplate(CS, idLstConsult, consult.template.IDLSTTEMPLATE, SessionTools.Collaborator_IDA);


                if (alreadyExists)
                {
                    //si le template existe deja; confirmation d'écrasement
                    string msgConfirm = Ressource.GetStringForJS("Msg_AlreadyExistsTemplateOverWrite").Replace("{0}", consult.template.IDLSTTEMPLATE);
                    JavaScript.ConfirmOnStartUp(this, (isClose ? "CONFIRMSAVEQUIT" : "CONFIRMSAVE"), msgConfirm);
                    return;
                }
                else
                {
                    //sinon on l'insere et on ecrase le cas echeant
                    consult.InsertOverWriteTemplateWithCopyChildsFrom(CS, sourceIdLstTemplate, consult.template.IDA);
                    if (isTemporary)
                    {
                        //Suppression du template temporaire
                        RepositoryWeb.Delete(CS, idLstConsult, sourceIdLstTemplate, consult.template.IDA, true);
                    }
                }
            }
            else
            {
                //Sinon il s'agit d'un [UPDATE]
                consult.template.Update(CS);
            }
        }

        idLstTemplate = consult.template.IDLSTTEMPLATE;
        idA = consult.template.IDA;
        RepositoryWeb.WriteTemplateSession(this, idLstConsult, idLstTemplate, idA, parentGUID);
    }
    #endregion SaveTemplate


    /// <summary>
    /// Identification d'une colonne depuis la variable de session
    /// </summary>
    /// <param name="pAlias"></param>
    /// <param name="pTableName"></param>
    /// <param name="pColumnName"></param>
    /// <returns></returns>
    public ListFilterTools.ListColumn GetColumnInSession(string pAlias, string pTableName, string pColumnName)
    {
        ListFilterTools.ListColumn columnCandidate = null;
        List<ListFilterTools.ListColumn> lstColumns = this.Session[sessionName_LstColumn] as List<ListFilterTools.ListColumn>;
        if (null != lstColumns)
        {
            if (StrFunc.IsFilled(pTableName))
            {
                columnCandidate = lstColumns.Find(column => ((column.alias == pAlias) || String.IsNullOrEmpty(pAlias)) &&
                                           (column.tableName == pTableName) && (column.columnName == pColumnName));
            }

        }
        return columnCandidate;
    }

    /// <summary>
    /// Sauvegarde dans une variable de session des colonnes présentes dans le fichier XML 
    /// </summary>
    /// <param name="pReferential"></param>
    public void SaveInSession(Referential pReferential)
    {
        List<ListFilterTools.ListColumn> lstColumns = new List<ListFilterTools.ListColumn>();
        string previousRessource = string.Empty;
        List<ReferentialColumn> columns = pReferential.Column.ToList();
        columns.ForEach(column =>
        {
            bool isNoRowVersion = (column.ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            bool isNoRowAttribut = (column.ColumnName != Cst.OTCml_COL.ROWATTRIBUT.ToString());
            bool isNoImage = (false == TypeData.IsTypeImage(column.DataType.value));
            bool isNoRole = (false == column.IsRole);
            bool isNoItem = (false == column.IsItem);
            bool isResourceAccepted = ("Empty" != column.Ressource);
            bool isNotHide = (column.IsHideSpecified && (false == column.IsHide)) || 
                             (column.IsHideInDataGridSpecified && (false == column.IsHideInDataGrid)) || 
                             (column.IsHideInCriteriaSpecified && (false == column.IsHideInCriteria));

            bool isColumnCandidate = isNoRowVersion && isNoRowAttribut && isNoImage && isNoRole && isNoItem & isResourceAccepted & isNotHide;

            string displayName = string.Empty;
            if (isColumnCandidate)
            {
                ListFilterTools.ListColumn item = new ListFilterTools.ListColumn();
                item.alias = column.AliasTableNameSpecified ? column.AliasTableName : pReferential.AliasTableName;
                item.aliasDisplayName = column.AliasColumnNameSpecified? column.AliasColumnName:string.Empty;
                item.columnName = column.ColumnName;
                item.dataType = column.DataType.value;

                // Group (Default = ALL)
                item.group = "ALL";

                // Column DisplayName
                if (StrFunc.IsEmpty(column.Ressource))
                {
                    displayName = previousRessource;
                    displayName += " (" + Ressource.GetString(column.ColumnName) + ")";
                }
                else
                {
                    displayName = Ressource.GetMulti(column.Ressource, 2, 0);
                    previousRessource = displayName;

                    int index = columns.IndexOf(column);
                    if (index + 1 < columns.Count())
                    {
                        ReferentialColumn nextColumn = columns.ElementAt(index + 1);
                        if ((null != nextColumn) && StrFunc.IsEmpty(nextColumn.Ressource))
                            displayName += " (" + Ressource.GetString(nextColumn.ColumnName) + ")";
                    }
                }
                item.displayName = displayName.Replace(Cst.HTMLBreakLine, Cst.Space);
                item.isHideCriteria = column.IsHideInCriteriaSpecified && column.IsHideInCriteria;
                item.isMandatory = column.IsMandatorySpecified && column.IsMandatory;
                item.position = 0;
                item.tableName = pReferential.TableName;

                OpenReferentialArguments arg = new OpenReferentialArguments();
                arg.isHideInCriteriaSpecified = true;
                arg.isHideInCriteria = item.isHideCriteria;


                string relationTableName = string.Empty;
                string relationColumnRelation = string.Empty;
                string relationColumnSelect = string.Empty;
                string relationListType = Cst.ListType.Repository.ToString();

                if (ArrFunc.IsFilled(column.Relation))
                {
                    if (ArrFunc.IsFilled(column.Relation[0].ColumnRelation) && ArrFunc.IsFilled(column.Relation[0].ColumnSelect))
                    {
                        item.relation = new ListFilterTools.ListColumnRelation();
                        item.relation.table = column.Relation[0].TableName;
                        item.relation.columnRelation = column.Relation[0].ColumnRelation[0].ColumnName;
                        item.relation.columnSelect = column.Relation[0].ColumnSelect[0].ColumnName;
                        item.relation.type = column.Relation[0].ListTypeSpecified ? column.Relation[0].ListType : Cst.ListType.Repository.ToString(); 

                        if (StrFunc.IsFilled(column.Relation[0].ColumnSelect[0].Ressource))
                        {
                            item.displayName = Ressource.GetMulti(column.Relation[0].ColumnSelect[0].Ressource, 2, 0);
                        }
                    }
                }
                lstColumns.Add(item);
            }
        });
        dicColumn = new Dictionary<string, List<ListFilterTools.ListColumn>>();
        dicColumn.Add("ALL", lstColumns);
        lstGroup = new List<Pair<string, string>>();
        lstGroup.Add(new Pair<string,string>("ALL", "ALL"));
        this.Session[sessionName_LstColumn] = lstColumns;
    }

}


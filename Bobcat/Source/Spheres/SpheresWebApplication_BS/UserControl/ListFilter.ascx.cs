#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using EFS.Rights;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.ListControl
{
    public partial class ListFilter : UserControl
    {
        #region Members
        private bool changeOfColumn = false;
        //private bool changeOfChecked;
        #endregion Members

        #region Accessors
        #region GetTotalRows
        /// <summary>
        /// Retourne le nombre de lignes de filtres (en comptant le nombre de contrôle de type BS_GroupColumn)
        /// </summary>
        /// <returns>higher ID for rows</returns>
        public int GetTotalRows
        {
            get
            {
                List<BS_GroupColumn> ctrls = ControlsTools.GetControls<BS_GroupColumn>(plhFilter.Controls);
                return ctrls.Count;
            }
        }
        #endregion GetTotalRows
        private ListBase listBase
        {
            get { return (ListBase)this.Page; }
        }
        private string CS
        {
            get { return listBase.CS; }
        }
        private string IdLstTemplate
        {
            get { return listBase.idLstTemplate; }
        }
        private string IdLstConsult
        {
            get { return listBase.idLstConsult; }
        }
        private Int32 IdA
        {
            get { return listBase.idA; }
        }
        private string ParentGUID
        {
            get { return listBase.parentGUID; }
        }
        private LstConsultData Consult
        {
            get { return listBase.consult; }
        }
        private LstTemplateData Template
        {
            get { return Consult.template; }
        }
        #endregion Accessors

        #region Events
        protected override void OnInit(EventArgs e)
        {
            this.ID = "uc_lstfilter";
            string scriptRecord = String.Format("RecordFilter('#{1}hidLstFilter');", this.ClientID, ((PageBase)this.Page).ContentPlaceHolder_ClientID);
            btnOk.Attributes.Add("onclick", scriptRecord);
            btnOk.ServerClick += new EventHandler(OnValid);
            btnCancel.ServerClick += new EventHandler(OnCancel);
            btnOkAndSave.Attributes.Add("onclick", scriptRecord);
            btnOkAndSave.ServerClick += new EventHandler(OnValid);
            base.OnInit(e);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CreateControls();
        }
        #region OnCancel
        public void OnCancel(object sender, System.EventArgs e)
        {
        }
        #endregion OnCancel
        #region OnValid
        public void OnValid(object sender, System.EventArgs e)
        {
            // 1- Le même comportement que le bouton "Ok" 
            if (null != listBase)
            {
                listBase.isClose = true;
                listBase.SaveFilteredData(plhFilter, chkIsEnabledLstWhere.Checked);

                Control ctrl = sender as Control;
                switch (ctrl.ID)
                {
                    case "btnOk":
                        RepositoryWeb.WriteTemplateSession(this.Page, IdLstConsult, IdLstTemplate, IdA, ParentGUID);
                        break;
                    case "btnOkAndSave":
                        listBase.initialNameLstTemplate = Template.IDLSTTEMPLATE_WithoutPrefix;
                        listBase.newNameLstTemplate = Template.IDLSTTEMPLATE_WithoutPrefix;
                        listBase.SaveTemplate();
                        break;
                }
            }
        }
        #endregion OnValid

        #endregion Events

        #region Methods

        protected void CreateControls()
        {
            ListBase listBase = this.Page as ListBase;

            Panel pnlForm = new Panel();
            pnlForm.CssClass = "form-group form-group-xs";

            //Getting number of row in Panel to create as control rows as needed
            int nbCriteria = 0;

            string sqlSelect = @"select count(1)
            from dbo.LSTWHERE w
            where (w.IDLSTTEMPLATE = @IDLSTTEMPLATE) and (w.IDLSTCONSULT = @IDLSTCONSULT) and (w.IDA = @IDA)" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDLSTTEMPLATE", DbType.AnsiString, 64), IdLstTemplate);
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDLSTCONSULT), IdLstConsult);
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDA), IdA);

            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            if (null != obj)
                nbCriteria = Convert.ToInt32(obj);

            for (int i = 0; i <= nbCriteria; i++)
            {
                Panel pnlRow = new Panel();
                pnlRow.CssClass = "row";

                BS_GroupColumn bsGrpColumn = new BS_GroupColumn(i, listBase.isColumnByGroup);
                if (listBase.isColumnByGroup)
                    bsGrpColumn.LoadGroup(listBase.lstGroup);
                else if (listBase.isXMLSource)
                    bsGrpColumn.LoadColumn(listBase.lstGroup, listBase.dicColumn, "ALL");
                pnlRow.Controls.Add(bsGrpColumn);

                BS_Operand bsOperand = new BS_Operand(i);
                bsOperand.LoadOperand(listBase.lstOperand, string.Empty);
                pnlRow.Controls.Add(bsOperand);

                BS_Data bsData = new BS_Data(i);
                pnlRow.Controls.Add(bsData);

                plhFilter.Controls.Add(pnlRow);
            }
        }


        public void LoadDataControls()
        {
            if (null != listBase)
            {
                Consult.LoadLstWhere(CS, false, listBase.isSQLSource);

                chkIsEnabledLstWhere.Checked = Template.ISENABLEDLSTWHERE;

                string columnValue = null;
                //Control ctrlFound;
                if (changeOfColumn)
                {
                    #region Modification d'un critère: Reset éventuel OPERAND et/ou DATA
                    #endregion
                }
                else
                {
                    DataRowCollection rows = Consult.dtLstWhere.Rows;
                    #region Chargement initial de tous les critères en vigueur
                    for (int i = 0; i < rows.Count; i++)
                    {
                        BS_GroupColumn bsGrpColumn = plhFilter.FindControl(ListFilterTools.BS_GROUPCOLUMN + i.ToString()) as BS_GroupColumn;
                        BS_Operand bsOperand = plhFilter.FindControl(ListFilterTools.BS_OPERAND + i.ToString()) as BS_Operand;
                        BS_Data bsData = plhFilter.FindControl(ListFilterTools.BS_DATA + i.ToString()) as BS_Data;

                        string dataType = string.Empty;
                        string tableRelation = string.Empty;
                        string columnRelation = string.Empty;
                        string columnSelect = string.Empty;

                        bool isMandatory = Convert.ToBoolean(rows[i]["ISMANDATORY"]);
                        string alias = rows[i]["ALIAS"].ToString().TrimEnd();
                        string tableName = rows[i]["TABLENAME"].ToString();
                        string columnName = rows[i]["COLUMNNAME"].ToString();
                        string typeRelation = Cst.ListType.Repository.ToString();
                        string data = rows[i]["LSTVALUE"].ToString();

                        if (listBase.isSQLSource)
                        {
                            //Consultation LST
                            dataType = rows[i]["DATATYPE"].ToString();
                            // Constitution de la clé de colonne
                            columnValue = String.Format(";{0};{1};{2}", tableName, columnName, alias);
                            bsGrpColumn.LoadColumn(listBase.lstGroup, listBase.dicColumn, rows[i]["ALIASHEADER"].ToString());
                            bsGrpColumn.SetColumn(isMandatory, columnValue);
                        }
                        else
                        {
                            //Consultation XML: Récupération des caractéristiques
                            ListFilterTools.ListColumn columnCandidate = listBase.GetColumnInSession(alias, tableName, columnName);
                            if (null != columnCandidate)
                            {
                                dataType = columnCandidate.dataType;
                                if (null != columnCandidate.relation)
                                {
                                    tableRelation = columnCandidate.relation.table;
                                    columnRelation = columnCandidate.relation.columnRelation;
                                    columnSelect = columnCandidate.relation.columnSelect;
                                    typeRelation = columnCandidate.relation.type;
                                }
                                // Constitution de la clé de colonne
                                columnValue = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                                    ListFilterTools.SEP + "-1" + ListFilterTools.SEP,
                                    alias + ListFilterTools.SEP,
                                    dataType + ListFilterTools.SEP,
                                    tableName + ListFilterTools.SEP,
                                    columnName + ListFilterTools.SEP,
                                    tableRelation + ListFilterTools.SEP,
                                    columnRelation + ListFilterTools.SEP,
                                    columnSelect + ListFilterTools.SEP,
                                    typeRelation);

                                LoadDDLColumnFromSession(i, isMandatory, columnValue);
                            }
                        }

                        // Chargement des checkBox par critère
                        bsGrpColumn.LoadCHKIsActive(isMandatory, Convert.ToBoolean(rows[i]["ISENABLED"]));

                        // Lorsqu'il existe une colonne relation, le critère est nécessairement de type string
                        string dataTypeOperand = StrFunc.IsFilled(columnSelect) ? TypeData.TypeDataEnum.@string.ToString() : dataType;
                        // Assignement de l'opérand en fonction du datatype de la colonne
                        bsOperand.HiddenOperand(dataTypeOperand);
                        bsOperand.SetOperand(listBase.isXMLSource, dataTypeOperand, rows[i]["OPERATOR"].ToString(), data);

                        bsGrpColumn.ddlColumn.Attributes.Add("operand", bsOperand.ddlOperand.SelectedValue);
                        // Chargement des données
                        string formatData = LstConsultData.FormatLstValue2(CS, data, dataTypeOperand, true, false, true, SessionTools.User.entity_IdA);
                        bsData.SetTXTData(dataTypeOperand, formatData);

                        Panel pnl = bsData.Parent as Panel;
                        if ((null != pnl) && isMandatory)
                            pnl.Attributes.Add("mandatory", "1");
                    }
                    #endregion Chargement initial de tous les critères en vigueur
                }
            }
        }

        #region LoadDDLColumnFromSession;
        private void LoadDDLColumnFromSession(int pIndexCriteria, bool pIsMandatory, string pColumnValue)
        {
            DropDownList ddlColumn = plhFilter.FindControl(ListFilterTools.DDL_COLUMN + pIndexCriteria.ToString()) as DropDownList;
            if (null != ddlColumn)
            {
                ddlColumn.Items.Clear();
                List<ListFilterTools.ListColumn> columns = listBase.dicColumn["ALL"];
                if (null != columns)
                {
                    columns.ForEach(column =>
                    {
                        if (false == column.isHideCriteria)
                        {
                            ListItem item = new ListItem(column.displayName,
                                String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                                (column.isMandatory ? "1" : "0") + ListFilterTools.SEP,
                                "-1" + ListFilterTools.SEP,
                                column.alias.TrimEnd() + ListFilterTools.SEP,
                                column.dataType + ListFilterTools.SEP,
                                column.tableName + ListFilterTools.SEP,
                                column.columnName + ListFilterTools.SEP,
                                ListFilterTools.SEP + ListFilterTools.SEP + ListFilterTools.SEP,
                                Cst.ListType.Repository.ToString()));
                            item.Attributes.Add("data-type", column.dataType);

                            ddlColumn.Items.Add(item);
                        }
                    });
                }
                ddlColumn.Items.Insert(0, new ListItem(" ",
                    "0" + ListFilterTools.SEP + "-1" + ListFilterTools.SEP + ListFilterTools.SEP + ListFilterTools.SEP +
                    ListFilterTools.SEP + "-1" + ListFilterTools.SEP + ListFilterTools.SEP + ListFilterTools.SEP + ListFilterTools.SEP));

                if ((false == ControlsTools.DDLSelectByValue(ddlColumn, (pIsMandatory ? "1" : "0") + pColumnValue)) && (false == pIsMandatory))
                {
                    // PL Tip si le Find n'aboutit pas on tente un autre find sur une colonne "Mandatory" 
                    // => cas d'une colonne Mandatory utilisée également en tant que critère "Non mandatory"
                    ControlsTools.DDLSelectByValue(ddlColumn, "1" + pColumnValue);
                }

            }
        }
        #endregion LoadDDLColumnFromSession;

        #region private GetInfosForColReferential
        /// <summary>
        /// Getting column detailled infos that are loaded as array of arrayList in Session before this page open
        /// </summary>
        // <param name="pIdLstColumn">ID for finding the column in the DDL</param> 
        /// <param name="pDDLIndexSession">Session ID and reference for getting data in memory</param> 
        /// <param name="pTableName">TableName for finding the column in the DDL</param>
        /// <param name="pColumnName">ColumnName for finding the column in the DDL</param>
        /// <param name="opDataType"> out : DataType</param>
        /// <param name="opRelationTableName"> out : TableName of the relation</param>
        /// <param name="opRelationColumnRelation"> out : ColumnName for the relation</param>
        /// <param name="opRelationColumnSelect"> out : ColumnName for getting data in the relation</param>
        /// <param name="opRelationListType"> out : ListType of the relation (ie: Referential,Price,Log, ... is used to determine the XMLFile subpath)</param>
        private void GetInfosForColReferential(string pDDLIndexSession, int pIdLstColumn, string pAlias, string pTableName, string pColumnName,
            out string opDataType, out string opRelationTableName, out string opRelationColumnRelation, out string opRelationColumnSelect, out string opRelationListType)
        {
            //Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
            //                  De plus, pTableName et pColumnName reste en "ref" le temps de cette compatibilité...
            string mandatory = "0" + ListFilterTools.SEP;

            ArrayList[] alSelect = (ArrayList[])Session[pDDLIndexSession];

            int indexFound = RepositoryWeb.GetIndexInSession_LstColumn(this.Page, pDDLIndexSession, pIdLstColumn, pAlias, pTableName, pColumnName);

            //if found in array, splitting infos in out parms; else returns empty strings as out parms
            if (indexFound != -1)
            {
                //Exemple de ce que contient la variable alSelect (pour plus de détail voir son initialisation dans SaveInSession_LstColumn()): 
                //alSelect[0] Nom affiché
                //alSelect[1] Clé (ex. 5,tblmain,string,MARKET,SHORTIDENTIFIER,,,,Referential)
                //alSelect[2] Position 
                //alSelect[3] TableName du référentiel
                //alSelect[4] ColumnName
                //alSelect[5] Datatype
                //alSelect[6] OpenReferentialArguments
                if (StrFunc.IsEmpty(pTableName) || StrFunc.IsEmpty(pColumnName))
                {
                    pTableName = alSelect[3][indexFound].ToString();
                    pColumnName = alSelect[4][indexFound].ToString();
                }

                opDataType = alSelect[5][indexFound].ToString();
                string[] infosColumnName = new string[10];
                infosColumnName = (mandatory + (alSelect[1][indexFound].ToString())).Split(ListFilterTools.SEP.ToCharArray());
                opRelationTableName = infosColumnName[6];
                opRelationColumnRelation = infosColumnName[7];
                opRelationColumnSelect = infosColumnName[8];
                opRelationListType = infosColumnName[9];
            }
            else
            {
                opDataType = string.Empty;
                opRelationTableName = string.Empty;
                opRelationColumnRelation = string.Empty;
                opRelationColumnSelect = string.Empty;
                opRelationListType = string.Empty;
            }
        }
        #endregion method GetInfosForColReferential

        #endregion Methods
    }
}
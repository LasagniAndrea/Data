using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    public partial class LstCriteriaPage : PageBaseTemplate
    {
        #region constantes
        private const string DDLDATASEPARATOR = ",";
        private const string BUT_PTSFORLIST = "btn_PtsForList";
        private const int MinNbCriteria = 12;
        #endregion

        #region members
        private bool isPostBack_ChangeOfColumn;
        private bool isPostBack_ChangeOfChecked;
        private string queryStringIdA;
        private string queryStringidLstTemplate;
        private string queryStringSessionName_LstColumn;
        private string queryStringListType;
        private int nbCriteria;
        /// <summary>
        ///  Collection contenant les contôles critères
        ///  <para>Pour eviter l'usage de FindControl (non performant)</para>
        /// </summary>
        /// FI 20191108 add
        private Hashtable lstCriteria;



        /// <summary>
        /// Lsite des colonnes (+ Caractéristiques) 
        /// <para>Alimentation uniquement si referential (fichier XML)</para>
        /// <para>Alimentation à partir d'une variable session</para>
        /// </summary>
        /// FI 20210125 [XXXXX] Add 
        private ArrayList[] LstColumn
        {
            get
            {
                return (ArrayList[])DataCache.GetData<ArrayList[]>(queryStringSessionName_LstColumn);
            }
        }


        #endregion

        #region properties
        protected override string SpecificSubTitle
        {
            get
            {
                string ret = Ressource.GetString("btnViewerCriteria");
                return ret.Replace("...", string.Empty).Trim();
            }
        }
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
            get { return "LstCriteriaPage"; }
        }

        #endregion

        #region constructor
        public LstCriteriaPage()
        {
            isPostBack_ChangeOfColumn = false;
            isPostBack_ChangeOfChecked = false;
            queryStringidLstTemplate = string.Empty;
            queryStringIdA = string.Empty;
            queryStringSessionName_LstColumn = string.Empty;
            queryStringListType = string.Empty;
        }
        #endregion constructor

        #region protected override OnInit
        protected override void OnInit(EventArgs e)
        {
            //Nom du modele courant
            queryStringidLstTemplate = Request.QueryString["T"];
            
            //Propriétaire du modère
            queryStringIdA = Request.QueryString["A"];
            queryStringSessionName_LstColumn = Request.QueryString["DDL"];
            queryStringListType = Request.QueryString["Type"];

            InitializeComponent();
            this.Form = frmLstCriteria;
            base.OnInit(e);
        }
        #endregion

        #region protected override OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (isPostBack_ChangeOfColumn)
                LoadDataControls();

            UpdateLabelClassForDDLOptionGroupList();

            InitializeInputHelpButton();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// FI 20191028 [XXXXX] Add
        protected override bool IsSupportsPartialRendering
        {
            get { return true; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200107 [XXXXX] add
        protected bool IsSupportAutocomplete
        {
            get
            {
                return Convert.ToBoolean(SystemSettings.GetAppSettings("CriteriaAutoComplete", "true"));
            }
        }


        #region protected override CreateControls
        /// <summary>
        /// Main for page controls creation
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void CreateControls()
        {
            CreateToolBar();
            CreateControlsData();
        }
        #endregion

        #region protected override LoadDataControls
        /// <summary>
        /// Load the datas into each controls for each rows
        /// </summary>
        /// PL 20181123 New signature of DDLLoad_LSTCOLUMN() - Add DataTable opDtLstSelect
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        protected override void LoadDataControls()
        {
            #region !IsPostBack
            if (!IsPostBack)
            {
                // FI 20201201 [XXXXX] Refactoring et Appel de la méthode FilterLstSelectAvailableForCriteria

                // FI 20191028 [XXXXX] Chgt de template, LSTWHERE et l'initialiation de la Check  uniquement si !IsPostBack
                //Loading information for this template
                ThisConsultation.LoadTemplate(SessionTools.CS, idLstTemplate, Convert.ToInt32(queryStringIdA));
                ThisConsultation.LoadLstWhere(SessionTools.CS, false, !ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult));

                if (StrFunc.IsEmpty(queryStringSessionName_LstColumn))
                {
                    //Load depuis la table LSTCOLUMN
                    //NB: Dans le cas du référentiel (fichier XML), les données sont issues d'une variable Session initialisée dans la méthode SaveInSession_LstColumn()

                    ThisConsultation.LoadLstSelectAvailable(SessionTools.CS);
                    DataTable dtColumn = ThisConsultation.FilterLstSelectAvailableForCriteria();

                    int max = GetControlIDMax();
                    for (int li = 0; li <= max; li++)
                    {
                        //ddlColumnName = (DropDownList)PlhMain.FindControl(DDL_COLUMNNAME + li.ToString());
                        DropDownList ddlColumnName = lstCriteria[DDL_COLUMNNAME + li.ToString()] as DropDownList;

                        ControlsTools.DDLLoad_LSTCOLUMN(ddlColumnName, DDLDATASEPARATOR, ThisConsultation.IsMultiTable(SessionTools.CS), true, dtColumn);

                        /* Code à utliser en DEBUG si nécessite de consulter le contenu de la DDL
                        if (li == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("----------------------");
                            System.Diagnostics.Debug.WriteLine(ddlColumnName.ID);

                            foreach (ListItem item in ddlColumnName.Items)
                                System.Diagnostics.Debug.WriteLine(item.Value);
                            System.Diagnostics.Debug.WriteLine("----------------------");
                        }
                        */
                    }
                }


                //Set the checkbox data
                SetCheckBoxData(Cst.CHK + "ISENABLEDLSTWHERE", ThisConsultation.template.ISENABLEDLSTWHERE);
            }

            #endregion
            if (isPostBack_ChangeOfColumn)
            {
                #region Modification d'un critère: Reset éventuel de la DDL Operateur associée, de la Donnée critère et depuis la v7.2 de la CHK associée
                //- Reload de la DDLOperator en fonction du Datatype
                //- Réinitialisation de la donnée critère uniquement lorsque le Datatype diffère "fortement" (ex. passage de string à date)
                bool isOperatorReloaded = true;
                string dataType = string.Empty;
                string dataTypeForOperator = string.Empty;

                //ctrlFound = PlhMain.FindControl(dataFrom__EVENTTARGET);
                if (lstCriteria[dataFrom__EVENTTARGET] is Control ctrlFound)
                {
                    DropDownList ddl = ctrlFound as DropDownList;
                    string[] asTemp = ddl.SelectedValue.Split(DDLDATASEPARATOR.ToCharArray());
                    string sIDLSTCOLUMN = asTemp[1];
                    string sTABLENAME = asTemp[1];
                    if (StrFunc.IsFilled(sTABLENAME) || (sIDLSTCOLUMN != "-1"))//Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                        dataType = asTemp[3];

                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //PL 20180529 New feature: on coche/décoche la checkbox associée 
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    bool isFilterChecked = !StrFunc.IsEmpty(dataType);
                    ctrlFound = lstCriteria[dataFrom__EVENTTARGET.Replace(DDL_COLUMNNAME, Cst.CHK)] as Control;
                    if (ctrlFound != null)
                    {
                        ((CheckBox)ctrlFound).Checked = isFilterChecked;
                        ((CheckBox)ctrlFound).Enabled = isFilterChecked;
                    }
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                    //FI 20101103 Lorsqu'il existe une colonne relation, le critère est nécessairement de type string
                    dataTypeForOperator = dataType;
                    if (StrFunc.IsFilled(asTemp[8]))
                        dataTypeForOperator = TypeData.TypeDataEnum.@string.ToString();

                    if (lstCriteria[dataFrom__EVENTTARGET.Replace(DDL_COLUMNNAME, DDL_OPERATOR)] is DropDownList ddlOperator)
                    {
                        //Load DDL Operator from Datatype
                        int countOperator = ddlOperator.Items.Count;
                        if (StrFunc.IsEmpty(dataType))
                        {
                            ddlOperator.Items.Clear();
                            ddlOperator.Enabled = false;
                        }
                        else
                        {
                            ddlOperator.Enabled = true;
                            ControlsTools.DDLLoad_Operator(ddlOperator, dataTypeForOperator);
                            if (TypeData.IsTypeString(dataTypeForOperator) || TypeData.IsTypeText(dataTypeForOperator))
                                ControlsTools.DDLSelectByValue(ddlOperator, "Contains");
                        }
                        //Tip (PL): Astuce pour n'effacer la donnée que s'il y a un changement "fort" de type (donc d'opérateurs disponibles)
                        isOperatorReloaded = (countOperator != ddlOperator.Items.Count);
                    }
                }

                if (isOperatorReloaded || StrFunc.IsEmpty(dataType))
                {
                    //If postback from DDLcolumn (which has changed if doing postback), reset the data textbox
                    if (lstCriteria[dataFrom__EVENTTARGET.Replace(DDL_COLUMNNAME, TXT_VALUE)] is TextBox txtValue)
                    {
                        ((TextBox)txtValue).Text = string.Empty;
                        ((TextBox)txtValue).Visible = !TypeData.IsTypeBool(dataTypeForOperator); //Hiding pour les données de type boolean
                        if (StrFunc.IsEmpty(dataType))
                            ((TextBox)txtValue).Enabled = false;
                    }
                }
                #endregion
            }
            else if (!IsPostBack)
            {
                #region Chargement initial de tous les critères en vigueur
                //For each row of LSTWHERE
                for (int i = 0; i < ThisConsultation.dtLstWhere.Rows.Count; i++)
                {
                    #region Initialize variables
                    int idLstColumn = -1;//Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                    string relationTableName = string.Empty;
                    string relationColumnRelation = string.Empty;
                    string relationColumnSelect = string.Empty;
                    string relationListType = Cst.ListType.Repository.ToString();
                    bool isMandatory = Convert.ToBoolean(ThisConsultation.dtLstWhere.Rows[i]["ISMANDATORY"]);

                    string dataType;
                    string columnName;
                    string tableName;
                    if (ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult))
                    {
                        int tmp_idLstColumn = -1;
                        if (!(ThisConsultation.dtLstWhere.Rows[i]["IDLSTCOLUMN"] is DBNull))
                            tmp_idLstColumn = Convert.ToInt32(ThisConsultation.dtLstWhere.Rows[i]["IDLSTCOLUMN"]);

                        //Consultation XML: Récupération des caractéristiques
                        string alias = ThisConsultation.dtLstWhere.Rows[i]["ALIAS"].ToString().Trim();
                        tableName = ThisConsultation.dtLstWhere.Rows[i]["TABLENAME"].ToString();
                        columnName = ThisConsultation.dtLstWhere.Rows[i]["COLUMNNAME"].ToString();

                        //PL 20121129 Add alias
                        GetInfosForColReferential(tmp_idLstColumn, alias, tableName, columnName,
                            out dataType, out relationTableName, out relationColumnRelation, out relationColumnSelect, out relationListType);
                    }
                    else
                    {
                        //Consultation LST
                        tableName = ThisConsultation.dtLstWhere.Rows[i]["TABLENAME"].ToString();
                        columnName = ThisConsultation.dtLstWhere.Rows[i]["COLUMNNAME"].ToString();
                        dataType = ThisConsultation.dtLstWhere.Rows[i]["DATATYPE"].ToString();
                    }

                    #endregion
                    #region CheckBox: "Enabled"
                    #endregion

                    #region CheckBox: "Enabled"
                    if (lstCriteria[Cst.CHK + i.ToString()] is CheckBox chkEnabled)
                    {
                        chkEnabled.Enabled = !isMandatory;
                        chkEnabled.Checked = isMandatory || Convert.ToBoolean(ThisConsultation.dtLstWhere.Rows[i]["ISENABLED"]);
                    }

                    #endregion

                    #region DDL: "Columns"
                    if (lstCriteria[DDL_COLUMNNAME + i.ToString()] is DropDownList ddlColumnName)
                    {
                        #region Constitution de la clé
                        //Warning: Cette clé sera complétée plus bas, par l'ajout en début d'un indicateur "Mandatory" (1 ou 0)
                        string selectedValue = DDLDATASEPARATOR + idLstColumn.ToString();
                        selectedValue += DDLDATASEPARATOR + ThisConsultation.dtLstWhere.Rows[i]["ALIAS"].ToString().TrimEnd();
                        selectedValue += DDLDATASEPARATOR + dataType;
                        selectedValue += DDLDATASEPARATOR + tableName;
                        selectedValue += DDLDATASEPARATOR + columnName;
                        selectedValue += DDLDATASEPARATOR + relationTableName;
                        selectedValue += DDLDATASEPARATOR + relationColumnRelation;
                        selectedValue += DDLDATASEPARATOR + relationColumnSelect;
                        selectedValue += DDLDATASEPARATOR + relationListType;
                        #endregion

                        bool isOptGroupList = (ddlColumnName.GetType() == typeof(OptionGroupDropDownList));
                        //20090925 PL Add following test on "IsReferential"
                        isOptGroupList &= !ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult);

                        OptionGroupDropDownList optGroupList = null;
                        if ((!ControlsTools.DDLSelectByValue(ddlColumnName, (isMandatory ? "1" : "0") + selectedValue)) && (!isMandatory))
                        {
                            //20070522 PL Tip si le Find n'aboutit pas on tente un autre find sur une colonne "Mandatory" (cas d'une colonne Mandatory utilisé également en tant que critère "Non mandatory")
                            ControlsTools.DDLSelectByValue(ddlColumnName, "1" + selectedValue);
                        }

                        #region Cas particulier des données "Mandatories"
                        if (isMandatory)
                        {
                            //Effacement de tous les items pour ne conserver que celui concerné
                            ListItem li = ddlColumnName.SelectedItem;
                            ddlColumnName.Items.Clear();

                            // 20090909 RD / ListBox avec OptionGroup
                            ExtendedListItem extLi;
                            int extLiIndex;
                            if (isOptGroupList)
                            {
                                li.Attributes.Add("class", EFSCssClass.DropDownListCapture);
                                li.Attributes.Add("style", "background-color:" + System.Drawing.Color.Gold.Name.ToLower());

                                optGroupList = (OptionGroupDropDownList)ddlColumnName;
                                string itemTextGroup;
                                if (StrFunc.IsFilled(relationTableName))
                                    itemTextGroup = Ressource.GetMulti("TBL_" + relationTableName.ToString().Trim());
                                else
                                    itemTextGroup = Ressource.GetMulti("TBL_" + tableName.ToString().Trim());

                                ListItem listItem = new ListItem(itemTextGroup, itemTextGroup);
                                ddlColumnName.Items.Add(listItem);

                                extLiIndex = optGroupList.ExtendedItems.Count - 1;
                                extLi = optGroupList.ExtendedItems[extLiIndex];
                                extLi.GroupingType = ListItemGroupingTypeEnum.New;
                                extLi.GroupingText = extLi.Text;
                            }
                            else
                            {
                                ddlColumnName.BackColor = System.Drawing.Color.Gold;
                            }

                            li.Text = li.Text.Replace(Cst.HTMLBreakLine, " ");//Replace <br/> by space
                            ddlColumnName.Items.Add(li);

                            // 20090914 RD / ListBox avec OptionGroup
                            if (isOptGroupList)
                            {
                                extLiIndex = optGroupList.ExtendedItems.Count - 1;
                                extLi = optGroupList.ExtendedItems[extLiIndex];
                                extLi.GroupingType = ListItemGroupingTypeEnum.Inherit;
                            }

                            ControlsTools.DDLSelectByValue(ddlColumnName, "1" + selectedValue);

                            if (lstCriteria[ControlID.GetID("TOOLTIP" + i.ToString(), null, Cst.IMG)] is WCToolTipPanel pnlTooltip)
                            {
                                pnlTooltip.Pty.TooltipContent = Ressource.GetString("MandatoryFilter");
                                pnlTooltip.CssClass = "fa-icon fas fa-info-circle mandatory";
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region DDL: "Operator"
                    string dataTypeForOperator = dataType;
                    //FI 20101103 Lorsqu'il existe une colonne relation, le critère est nécessairement de type string
                    if (StrFunc.IsFilled(relationColumnSelect))
                        dataTypeForOperator = TypeData.TypeDataEnum.@string.ToString();

                    string lstValue = ThisConsultation.dtLstWhere.Rows[i]["LSTVALUE"].ToString();

                    if (lstCriteria[DDL_OPERATOR + i.ToString()] is DropDownList ddlOperator)
                    {
                        //Load Operator from Datatype
                        ControlsTools.DDLLoad_Operator(ddlOperator, dataTypeForOperator);

                        // EG 20220905 [WI395] Référentiel ACTEUR : Critère Rôle inopérant dans le filtre avec l’opérateur « = ».
                        if ((tableName == "ACTOR") && (columnName == "ROLE"))
                        {
                            ListItem item = ddlOperator.Items.FindByValue("=");
                            if (null != item)
                                ddlOperator.Items.Remove(item);
                        }
                        //
                        // RD 20110706 [17504]
                        // Après la mise en place des nouveaux opérateurs "Checked" et "Unchecked":
                        // - Un script SQL reprend l'existent pour les consultations de type "LST".
                        // - Pour les consultations de type "Referentiel", il est impossible de mettre en place un tel script SQL, 
                        //   car les types de données des colonnes se trouvent dans le descriptif XML.
                        //
                        // Ainsi, une reprise de l'existent pour les consultations de type "Referentiel", est faite dans le code C#,
                        // elle permet de faire une interprétation des anciens opérateurs "=" et "!=", 
                        // et les transformer vers les nouveaux opérateurs "Checked" et "Unchecked", en fonction de la valeur du critère.
                        //
                        // Exemple : Publique est une donnée du référentiel Actor
                        // - « Publique = true », devient : « Publique Checked »
                        // - « Publique = false », devient : « Publique Unchecked »
                        // - « Publique != false », devient : « Publique Checked »
                        //
                        string selectedOperator = ThisConsultation.dtLstWhere.Rows[i]["OPERATOR"].ToString();
                        if (ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult) &&
                            TypeData.IsTypeBool(dataTypeForOperator))
                        {
                            if ((("=" == selectedOperator) && (("TRUE" == lstValue.ToUpper()) || ("1" == lstValue))) ||
                                (("!=" == selectedOperator) && (("FALSE" == lstValue.ToUpper()) || ("0" == lstValue))))
                            {
                                selectedOperator = "Checked";
                            }
                            else if (("Checked" != selectedOperator) && ("Unchecked" != selectedOperator))
                                selectedOperator = "Unchecked";
                        }
                        //Select value
                        ControlsTools.DDLSelectByValue(ddlOperator, selectedOperator);
                    }

                    #endregion
                    #region Textbox: "Data"
                    #endregion

                    #region Textbox: "Data"
                    if (lstCriteria[TXT_VALUE + i.ToString()] is TextBox txtValue)
                    {
                        // RD 20110706 [17504]
                        // Pour ne pas afficher la zone de saisie 
                        if (TypeData.IsTypeBool(dataTypeForOperator))
                        {
                            txtValue.Text = string.Empty;
                            txtValue.Visible = false;
                        }
                        else
                        {
                            ReferentialsReferentialColumnDataType rDataType = new ReferentialsReferentialColumnDataType
                            {
                                value = dataType
                            };
                            txtValue.Text = LstConsult.FormatLstValue2(SessionTools.CS, lstValue, rDataType, true, false, true, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter);
                            txtValue.Visible = true;
                        }
                    }
                    #endregion
                }
                #endregion
            }
        }
        #endregion

        #region protected override SaveData
        /// <summary>
        /// delete and insert new datas from controls in LSTWHERE
        /// </summary>
        protected override void SaveData()
        {
            ThisConsultation.LoadTemplate(SessionTools.CS, idLstTemplate, idA);

            // FI 20201209 [XXXXX] Call CreateTemporaryTemplate
            // Appuie sur le bouton ok => génération d'un template temporaire 
            CreateTemporaryTemplate();

            // FI 20200602 [25370] appel à SetIsEnabledLstWhere2
            Boolean isEnabledLstWhere = GetCheckBoxData(Cst.CHK + "ISENABLEDLSTWHERE");
            ThisConsultation.template.ISENABLEDLSTWHERE = isEnabledLstWhere;
            ThisConsultation.template.SetIsEnabledLstWhere2(SessionTools.CS);

            //delete from table LSTWHERE
            //ThisConsultation.template.DeleteChild(CS, Cst.OTCml_TBL.LSTWHERE);
            ReferentialWeb.DeleteChild(SessionTools.CS, Cst.OTCml_TBL.LSTWHERE, idLstConsult, idLstTemplate, idA, false);

            //insert in table LSTWHERE
            // FI 20200602 [25370] appel à InsertLstWhere
            InsertLstWhere();

            //ThisConsultation.WriteTemplateSession(this, idLstTemplate, idA, this.ParentGUID);
            ReferentialWeb.WriteTemplateSession(idLstConsult, idLstTemplate, idA, this.ParentGUID);

            //if not close this page, reload it
            if (!isClose)
            {
                // RD 20100409 / Bug:
                // EG 20210916 [XXXXX] Ajout IDMenu
                string urlThis = "LstCriteria.aspx" + "?C=" + idLstConsult + "&T=" + idLstTemplate + "&A=" + idA + "&DDL=" + Request.QueryString["DDL"] + "&Type=" + Request.QueryString["Type"] + "&IDMenu=" + IdMenu + " & S=RELOAD&ParentGUID=" + this.ParentGUID;
                Server.Transfer(urlThis);
            }
        }
        #endregion

        #region protected override Reset
        /// <summary>
        /// clear all the controls datas
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void Reset()
        {
            //getting max controlID
            int max = GetControlIDMax();
            //reset all datas in all controls
            for (int i = 0; i <= max; i++)
            {
                //NB: On ne supprime pas les critères Mandatory. Ces derniers sont identifiés en regardant l'image qui y est associée (Image Warning).
                bool isMandatory = (PlhMain.FindControl(ControlID.GetID("TOOLTIP" + i.ToString(), null, Cst.IMG)) is WCToolTipPanel pnl) && (pnl.CssClass.Contains("mandatory"));
                if (!isMandatory)
                {
                    //PL 20180529
                    if (PlhMain.FindControl(Cst.CHK + i.ToString()) is CheckBox chk)
                    {
                        chk.Checked = false;
                        chk.Enabled = false;
                    }

                    if (PlhMain.FindControl(DDL_COLUMNNAME + i.ToString()) is DropDownList ddlColumn)
                        ddlColumn.SelectedIndex = 0;

                    if (PlhMain.FindControl(DDL_OPERATOR + i.ToString()) is DropDownList ddlOperator)
                    {
                        ddlOperator.Items.Clear();
                        ddlOperator.Enabled = false;
                    }

                    if (PlhMain.FindControl(TXT_VALUE + i.ToString()) is TextBox txtValue)
                    {
                        txtValue.Text = string.Empty;
                        txtValue.Enabled = false;
                    }

                    if (PlhMain.FindControl(TXT_IDVALUE + i.ToString()) is TextBox txtIdValue)
                    {
                        txtIdValue.Text = string.Empty;
                    }
                }
            }
        }
        #endregion Method Reset

        #region protected override CreateChildControls
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Resize window
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/LstCriteria.min.js"));

            WindowSetSize();
            
            base.CreateChildControls();
        }
        #endregion

        #region private Page_Load
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20160309 [XXXXX] Modify
        // EG 20210222 [XXXXX] Appel OpenerRefresh (présent dans PageBase.js) en remplacement de OpenerCallRefresh
        // EG 20210222 [XXXXX] Appel SetFocus (présent dans PageBase.js) 
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // FI 20210125 [XXXXX] Appel à CheckSessionState
            CheckSessionState();

            //PageTools.SetHead(this, "Criteria", null, null);
            PageTools.SetHead(this, this.Title, null, null);

            //If control doing postBack is DDL columns, need to reload datatype for new selected column
            Regex r = new Regex(DDL_COLUMNNAME + "\\w+");
            isPostBack_ChangeOfColumn = (StrFunc.IsFilled(dataFrom__EVENTTARGET) && r.IsMatch(dataFrom__EVENTTARGET));

            r = new Regex("CHK" + "\\w+");
            isPostBack_ChangeOfChecked = (StrFunc.IsFilled(dataFrom__EVENTTARGET) && r.IsMatch(dataFrom__EVENTTARGET));


            // FI 20160309 [XXXXX] call SetFocus sur la ddlColumn
            // FI 20191028 [XXXXX] call SetFocus sur la Chk
            if (isPostBack_ChangeOfColumn || isPostBack_ChangeOfChecked)
                SetFocus(dataFrom__EVENTTARGET);

            // FI 20160309 [XXXXX] Mise en commentaire le code est déplacé sous le if (!IsPostBack)
            // Lorsque l'utilisateur change de colonne => il y a un post  
            // Il ne faut pas donc pas rafraîchir dans la foulée. L'utilisateur doit appuyer sur Appliquer pour rafraîchir 
            //if (Request.QueryString["S"] == "RELOAD")
            //    //20071029 FI 15915 Appel à Refresh à la place de Reload 
            //    JavaScript.OpenerCallRefresh((PageBase)this);

            if (!IsPostBack)
            {
                // FI 20160309 [XXXXX]
                //On passe ici à chaque fois que l'utilisateur appuie sur "Appliquer" (!IsPostBack est vérifié)
                if (Request.QueryString["S"] == "RELOAD")
                    JavaScript.CallFunction(this, String.Format("OpenerRefresh('{0}','SELFRELOAD_')", PageName));

                try
                {
                    SetFocus(Cst.CHK + "ISENABLEDLSTWHERE");
                }
                catch
                {
                    JavaScript.CallFunction(this, "SetFocus('" + Cst.CHK + "ISENABLEDLSTWHERE" + "')");
                }
            }
        }
        #endregion private Page_Load

        #region private InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.AbortRessource = true;
            this.ID = "frmLstCriteria";
        }
        #endregion

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateToolBar()
        {
            Panel pnlToolBar = AddToolBar();
            WCToolTipLinkButton btn = AddButtonOk();
            defaultButton = btn.ID;
            pnlToolBar.Controls.Add(btn);
            pnlToolBar.Controls.Add(AddButtonCancel());
            pnlToolBar.Controls.Add(AddButtonApply());
            pnlToolBar.Controls.Add(AddButtonOkAndSave());
            pnlToolBar.Controls.Add(AddButtonReset());
        }

        #region private CreateControlsData
        /// <summary>
        /// Create a line with only controls for each line in table LSTWHERE + a blank line
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateControlsData()
        {
            lstCriteria = new Hashtable();

            Panel pnlBody = new Panel() { ID = "divbody", CssClass = MainClass };
            Panel pnlData = new Panel() { ID = "divlstcriteria" };

            //Getting number of row in table to create as control rows as needed
            GetNbCriteria(SessionTools.CS, out int countLstWhere, out int countLstWhereMandatory);


            WCToolTipPanel pnlMessage = new WCToolTipPanel
            {
                ID = ControlID.GetID("ISENABLEDLSTWHERE", null, Cst.IMG),
                CssClass = "fa-icon fas fa-info-circle"
            };
            pnlMessage.Pty.TooltipContent = Ressource.GetString(Cst.TypeInformationMessage.Information, true);

            Label lblMessage = new Label
            {
                ID = ControlID.GetID("ISENABLEDLSTWHERE", null, Cst.LBL),
                CssClass = EFSCssClass.Msg_Information,
                Text = Ressource.GetString("Msg_ApplyFilters")
            };

            Panel pnl = new Panel
            {
                ID = "divapplyfilter",
                CssClass = MainClass
            };
            pnl.Controls.Add(NewRowCheckbox(false, Cst.CHK + "ISENABLEDLSTWHERE", pnlMessage, lblMessage));
            pnlData.Controls.Add(pnl);

            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //PL 20181120 Newness
            countLstWhere = (countLstWhere < MinNbCriteria) ? MinNbCriteria : countLstWhere + 2;
            nbCriteria = countLstWhere;
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //for (int li = 0; li <= countLstWhere; li++)
            for (int li = 0; li < countLstWhere; li++)
            {
                Panel pnlLine = new Panel
                {
                    ID = "divln" + li.ToString()
                };

                pnlMessage = new WCToolTipPanel
                {
                    ID = ControlID.GetID("TOOLTIP" + li.ToString(), null, Cst.IMG),
                    CssClass = "fa-icon fas fa-info-circle " + (li < countLstWhereMandatory ? "mandatory" : string.Empty)
                };
                pnlMessage.Pty.TooltipContent = Ressource.GetString((li < countLstWhereMandatory) ? "MandatoryFilter" : "OptionalFilter");
                lstCriteria.Add(pnlMessage.ID, pnlMessage);
                pnlLine.Controls.Add(pnlMessage);

                //CheckBox ISENABLED
                CheckBox chkIsEnabled = new CheckBox
                {
                    ID = Cst.CHK + li.ToString(),
                    Text = string.Empty,
                    EnableViewState = true,
                    ToolTip = Ressource.GetString("Msg_ApplyFilter"),
                    AutoPostBack = true
                };
                lstCriteria.Add(chkIsEnabled.ID, chkIsEnabled);

                Panel pnlCtrl = new Panel();
                pnlCtrl.Controls.Add(chkIsEnabled);
                pnlLine.Controls.Add(pnlCtrl);

                //DDL Columns
                DropDownList ddlColumnName = new OptionGroupDropDownList(true, true, null);
                if (StrFunc.IsFilled(queryStringSessionName_LstColumn))
                {
                    //NB: Cas du référentiel (fichier XML), les données sont issues d'une variable Session initialisée dans la méthode SaveInSession_LstColumn()
                    ddlColumnName = DDLLoadFromSession();
                }
                ddlColumnName.CssClass = EFSCssClass.DropDownListCaptureLight;
                ddlColumnName.AutoPostBack = true;
                ddlColumnName.ID = DDL_COLUMNNAME + li.ToString();
                ddlColumnName.Width = Unit.Percentage(100);
                lstCriteria.Add(ddlColumnName.ID, ddlColumnName);
                pnlCtrl = new Panel();
                pnlCtrl.Controls.Add(ddlColumnName);
                pnlLine.Controls.Add(pnlCtrl);


                //DDL Operator
                DropDownList ddlOperator = new DropDownList();
                ddlOperator.Items.Add(new ListItem(string.Empty));
                ddlOperator.CssClass = EFSCssClass.DropDownListCapture;
                ddlOperator.ID = DDL_OPERATOR + li.ToString();
                ddlOperator.Width = Unit.Percentage(100);
                lstCriteria.Add(ddlOperator.ID, ddlOperator);
                pnlCtrl = new Panel();
                pnlCtrl.Controls.Add(ddlOperator);
                pnlLine.Controls.Add(pnlCtrl);

                //Textbox for data
                WCTextBox WCtxtValue = new WCTextBox(TXT_VALUE + li.ToString(), string.Empty, string.Empty, true, Ressource.GetString("RegexDefaultError", true))
                {
                    Width = Unit.Percentage(100),
                    CssClass = EFSCssClass.Capture
                };
                if (IsSupportAutocomplete)
                    WCtxtValue.CssClass = "or-autocomplete " + WCtxtValue.CssClass; //co pour CustomObect
                lstCriteria.Add(WCtxtValue.ID, WCtxtValue);
                pnlCtrl = new Panel();
                pnlCtrl.Controls.Add(WCtxtValue);
                pnlLine.Controls.Add(pnlCtrl);

                WCToolTipLinkButton btnValue = new WCToolTipLinkButton();
                ControlsTools.SetOnClientClick_ButtonReferential(btnValue, "ACTOR", null, Cst.ListType.Repository, true,
                    null, TXT_VALUE + li.ToString(), null, null, null, null, null, null);
                btnValue.CssClass = "fa-icon";
                btnValue.Text = @"<i class='fas fa-ellipsis-h'></i>";
                btnValue.Pty.TooltipContent = Ressource.GetString("SelectAValue");
                btnValue.ID = BUT_PTSFORLIST + li.ToString();
                btnValue.CausesValidation = false;
                btnValue.Visible = true;
                lstCriteria.Add(btnValue.ID, btnValue);
                pnlCtrl = new Panel();
                pnlCtrl.Controls.Add(btnValue);
                pnlLine.Controls.Add(pnlCtrl);

                pnlData.Controls.Add(pnlLine);
            }

            WCTogglePanel togglePanel = new WCTogglePanel
            {
                CssClass = MainClass
            };
            togglePanel.SetHeaderTitle(Ressource.GetString("Filter"));
            togglePanel.AddContent(pnlData);

            // FI 20191028 [XXXXX] Mise en place d'un UpdatePanel
            Control control;
            if (IsSupportsPartialRendering)
            {
                control = new UpdatePanel
                {
                    EnableViewState = true,
                    ID = "UpdatePanel"
                };
                ((UpdatePanel)control).ContentTemplateContainer.Controls.Add(togglePanel);
                control.Visible = true;
            }
            else
            { control = togglePanel; }

            pnlBody.Controls.Add(control);
            PlhMain.Controls.Add(pnlBody);
        }
        #endregion method CreateControlsRow

        #region private InitializeInputHelpButton
        /// <summary>
        /// Initializing controls depending of data type (special controls : Calendar or Button [...])
        /// </summary>
        // 20100901 EG Datepicker 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void InitializeInputHelpButton()
        {

            // FI 20191029 [XXXXX] pas de boucle si isPostBack_ChangeOfColumn 
            int max;
            int min;
            if (isPostBack_ChangeOfColumn)
            {
                min = StrFunc.GetSuffixNumeric2(dataFrom__EVENTTARGET);
                max = min;
            }
            else
            {
                min = 0;
                max = GetControlIDMax();
            }

            // for each control's row
            for (int i = min; i <= max; i++)
            {
                if (lstCriteria[DDL_COLUMNNAME + i.ToString()] is DropDownList ddlColumnName)
                {
                    //if DDLcolumn is not empty, get infos for this column (datatype, need a relation?, ...)
                    string selectedValue = ((DropDownList)ddlColumnName).SelectedValue;

                    bool isString = true;
                    bool isDate = false;
                    bool isDateTime = false;
                    bool isInteger = false;
                    bool isDecimal = false;
                    bool isRelationField = false;
                    string alias = string.Empty;
                    string tableName = string.Empty;
                    string columnName = string.Empty;
                    OpenReferentialArguments arg = null;

                    if (StrFunc.IsFilled(selectedValue))
                    {
                        string[] infosColumnName = selectedValue.Split(DDLDATASEPARATOR.ToCharArray());
                        //PL 20100628 Bug sur l'utilisation du bouton "Reset" (on se trouve parfois sur l'item optgroup...)
                        if (infosColumnName.Length > 1)
                        {
                            int idLstColumn = Convert.ToInt32(infosColumnName[1]);
                            string dataType = infosColumnName[3];
                            isDate = TypeData.IsTypeDate(dataType);
                            isDateTime = TypeData.IsTypeDateTime(dataType);
                            isString = TypeData.IsTypeString(dataType);
                            isInteger = TypeData.IsTypeInt(dataType);
                            isDecimal = TypeData.IsTypeDec(dataType);

                            alias = infosColumnName[2];
                            tableName = infosColumnName[4];
                            columnName = infosColumnName[5];
                            string relationTableName = infosColumnName[6];
                            string relationColumnRelation = infosColumnName[7];
                            string relationColumnSelect = infosColumnName[8];
                            string relationListType = infosColumnName[9];

                            if (StrFunc.IsFilled(queryStringSessionName_LstColumn) && (StrFunc.IsFilled(columnName) || (idLstColumn >= 0)))//Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                            {
                                //PL 20121129 Add pAlias
                                int index = GetIndexInSession_LstColumn(LstColumn, idLstColumn, alias, tableName, columnName);
                                if (index > -1)
                                    arg = (OpenReferentialArguments)LstColumn[6][index]; //Relation TableName
                            }
                            //if this column has a relation, force string datatype
                            isRelationField = (relationTableName.Length > 0 && relationColumnRelation.Length > 0 && relationColumnSelect.Length > 0 && relationTableName != (-1).ToString() && relationListType.Length > 0);
                            isRelationField = isRelationField || ((null != arg) && StrFunc.IsFilled(arg.referential));
                            if (isRelationField)
                            {
                                isString = true;
                                isDate = false;
                                isDateTime = false;
                                isInteger = false;
                                isDecimal = false;
                            }
                        }
                    }
                    //setting regEx for the control depending of the column datatype
                    if (lstCriteria[TXT_VALUE + i.ToString()] is WCTextBox txtValue)
                    {
                        string regularExpression;
                        if (isDate)
                        {
                            regularExpression = EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDate)
                                + "|" + EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDateRelativeOffset)
                                + "|" + "^(null|NULL|{null})$";

                            //
                            txtValue.regularExpression = regularExpression;
                            txtValue.CssClass = "DtPicker " + txtValue.CssClass;
                            txtValue.Width = Unit.Empty;
                        }
                        else if (isDateTime)
                        {
                            regularExpression = EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDateTime)
                                + "|" + EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDate)
                                + "|" + EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDateRelativeOffset)
                                + "|" + "^(null|NULL|{null})$";

                            txtValue.regularExpression = regularExpression;
                            txtValue.CssClass = "DtTimePicker " + txtValue.CssClass;
                            txtValue.Width = Unit.Empty;
                        }
                        else if (isInteger)
                        {
                            regularExpression = EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexInteger);
                            txtValue.regularExpression = regularExpression;
                            txtValue.CssClass = EFSCssClass.Capture;
                            txtValue.Width = Unit.Percentage(100);
                        }
                        else if (isDecimal)
                        {
                            regularExpression = EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDecimal)
                            + "|" + EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDecimalExtend);
                            txtValue.regularExpression = regularExpression;
                            txtValue.CssClass = EFSCssClass.Capture;
                            txtValue.Width = Unit.Percentage(100);
                        }
                        else //string
                        {
                            regularExpression = string.Empty;
                            txtValue.regularExpression = regularExpression;
                            txtValue.CssClass = EFSCssClass.Capture;
                            if (IsSupportAutocomplete)
                                txtValue.CssClass = "or-autocomplete " + txtValue.CssClass; //or pour OpenReferential autocomplete
                            txtValue.Width = Unit.Percentage(100);
                        }
                    }

                    if (lstCriteria["TD" + BUT_PTSFORLIST + i.ToString()] is TableCell cell)
                        cell.Visible = isString || isRelationField;

                    //ctrlFound = PlhMain.FindControl(BUT_PTSFORLIST + i.ToString());
                    if (lstCriteria[BUT_PTSFORLIST + i.ToString()] is LinkButton lnkBtnForList)
                    {
                        lnkBtnForList.Visible = false;
                        if (isString || isRelationField)
                        {
                            lnkBtnForList.Attributes.Remove("onclick");
                            if (isRelationField)
                            {
                                lnkBtnForList.Visible = (null != arg && StrFunc.IsFilled(arg.referential));
                                if (lnkBtnForList.Visible)
                                {
                                    arg.type_KeyField = Cst.DataKeyField;
                                    arg.clientIdForKeyField = TXT_IDVALUE + i.ToString();
                                    arg.clientIdForSqlColumn = TXT_VALUE + i.ToString();
                                    arg.SetAttributeOnCLickButtonReferential(lnkBtnForList);
                                }
                            }
                            else if (isString)
                            {
                                Cst.ListType lstType = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), queryStringListType);
                                // FI 20200326 [XXXXX] Recherche uniquement si Repository
                                // => Permet de ne pas planter (*) sur le menu "Cours clôture(DSP/ EDSP)" sur le critère statut 
                                // (*) Plantage lors de l'exécution de la requête associée au  menu  "Cours clôture(DSP/ EDSP)" dont l'exécution est de toute façon trop lourde pour pouvoir être utilisée ici
                                if (lstType == Cst.ListType.Repository)
                                {
                                    bool isVisible = false;
                                    //CC/PL 20180608
                                    //Warning: Pas d'affichage de bouton "..." pour les (nouvelles) colonnes "multi-données" (ex.: FEEMATRIX.ENVIRONMENT) 
                                    if ((alias != "vw_env") && StrFunc.IsFilled(tableName))
                                    {
                                        // FI 2020010 [XXXXX] Appel à GetReferentialTableName
                                        tableName = GetReferentialTableName(tableName);
                                        if (StrFunc.IsFilled(tableName))
                                        {
                                            string File = ReferentialTools.GetObjectXMLFile(lstType, tableName);
                                            isVisible = System.IO.File.Exists(File);
                                        }
                                    }

                                    lnkBtnForList.Visible = isVisible;
                                    if (lnkBtnForList.Visible)
                                    {
                                        arg = new OpenReferentialArguments
                                        {
                                            type_KeyField = Cst.KeyField,
                                            referential = tableName,
                                            sqlColumn = columnName,
                                            clientIdForKeyField = TXT_IDVALUE + i.ToString(),
                                            clientIdForSqlColumn = TXT_VALUE + i.ToString(),
                                            listType = lstType
                                        };
                                        arg.SetAttributeOnCLickButtonReferential(lnkBtnForList);
                                    }
                                }
                            }
                            else
                            {
                                throw new NotImplementedException("Not implemented");
                            }
                        }

                        if ((lstCriteria[Cst.CHK + i.ToString()] is CheckBox ctrlChk) && !ctrlChk.Checked)
                            lnkBtnForList.Visible = false;

                        // FI 20210407  [XXXXX] Pas de autocomplete s'il n'existe pas de bouton 3pts
                        if (false == lnkBtnForList.Visible)
                        {
                            if (lstCriteria[TXT_VALUE + i.ToString()] is WCTextBox txtValue2 && StrFunc.IsFilled(txtValue2.CssClass))
                                txtValue2.CssClass = txtValue2.CssClass.Replace("or-autocomplete", string.Empty);
                        }
                    }
                }
            }
        }
        #endregion

        #region private GetInfosForColReferential
        /// <summary>
        /// Getting column detailled infos that are loaded as array of arrayList in Session before this page open
        /// </summary>
        /// <param name="pTableName">TableName for finding the column in the DDL</param>
        /// <param name="pColumnName">ColumnName for finding the column in the DDL</param>
        /// <param name="opDataType"> out : DataType</param>
        /// <param name="opRelationTableName"> out : TableName of the relation</param>
        /// <param name="opRelationColumnRelation"> out : ColumnName for the relation</param>
        /// <param name="opRelationColumnSelect"> out : ColumnName for getting data in the relation</param>
        /// <param name="opRelationListType"> out : ListType of the relation (ie: Referential,Price,Log, ... is used to determine the XMLFile subpath)</param>
        //PL 20121129 Add pAlias
        private void GetInfosForColReferential(int pIdLstColumn, string pAlias, string pTableName, string pColumnName,
            out string opDataType, out string opRelationTableName, out string opRelationColumnRelation, out string opRelationColumnSelect, out string opRelationListType)
        {
            //Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
            //                  De plus, pTableName et pColumnName reste en "ref" le temps de cette compatibilité...
            string mandatory = "0" + DDLDATASEPARATOR;

            int indexFound = GetIndexInSession_LstColumn(LstColumn, pIdLstColumn, pAlias, pTableName, pColumnName);

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

                opDataType = LstColumn[5][indexFound].ToString();
                string[] infosColumnName = (mandatory + (LstColumn[1][indexFound].ToString())).Split(DDLDATASEPARATOR.ToCharArray());
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

        #region private GetControlIDMax
        /// <summary>
        /// Get the higher ID of the editable rows we created
        /// </summary>
        /// <returns>Higher ID for rows</returns>
        private int GetControlIDMax()
        {
            // FI 20191108 on part de MinNbCriteria
            int max = MinNbCriteria - 1;
            Control foundControl = PlhMain.FindControl(DDL_COLUMNNAME + max.ToString());
            while (foundControl != null)
            {
                max++;
                foundControl = PlhMain.FindControl(DDL_COLUMNNAME + max.ToString());
            }
            return (max - 1);
        }
        #endregion Method GetControlIDMax

        #region private DDLLoadFromSession
        /// <summary>
        /// Creating DDL with columns name. Loading operated from data that are loaded as array of arrayList in Session
        /// <para>See also: SaveInSession_LstColumn()</para>
        /// </summary>
        /// <param name="pDDLIndexSession">Session ID and reference for getting data in memory</param> 
        /// <returns>DDL for columns choice</returns>
        private DropDownList DDLLoadFromSession()
        {
            const string SEPARATOR = @";";

            #region Load d'éventuels critères obligatoires (LSTCONSULTWHERE)
            string SQLSelect = SQLCst.SELECT + "TABLENAME,COLUMNNAME" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSULTWHERE.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "IDLSTCONSULT=" + DataHelper.SQLString(idLstConsult) + Cst.CrLf;
            SQLSelect += SQLCst.ORDERBY + "POSITION";

            //Récupération des colonnes obligatoires
            string lstIdLstColumMandatory = SEPARATOR;
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, SQLSelect))
            {
                while (dr.Read())
                {
                    lstIdLstColumMandatory += Convert.ToString(dr["TABLENAME"]) + ":" + Convert.ToString(dr["COLUMNNAME"]) + SEPARATOR;
                }
            }
            #endregion

            // 20090909 RD / DropDownList avec OptionGroup
            DropDownList ddlSelect = new OptionGroupDropDownList();

            // FI 20200518 [XXXXX] Utilisation de DataCache
            //ArrayList[] alSelect = (ArrayList[])Session[pDDLIndexSession];
            ArrayList[] alSelect = LstColumn;

            //int j;
            OpenReferentialArguments arg;
            bool isOk, isMandatory;
            if (ArrFunc.IsFilled(alSelect))
            {
                for (int i = 0; i < alSelect[1].Count; i++)
                {
                    //20070427 PL PasGlop A revoir pour géréer les colonnes Hide
                    //j = i;
                    //if (idLstConsult.IndexOf("REF-Price") == 0)
                    //{
                    //    //NB: Ces référentiels diposent d'une 1ère colonne Hide. On décale dont d'un élément.
                    //    j = i + 1;
                    //}

                    isOk = true;
                    try
                    {
                        arg = (OpenReferentialArguments)alSelect[6][i];
                        if (arg.isHideInCriteriaSpecified && (arg.isHideInCriteria))
                            isOk = false;
                    }
                    catch { }

                    if (isOk)
                    {
                        //Identification des critères obligatoires
                        //isMandatory = (lstIdLstColumMandatory.IndexOf(SEPARATOR + j.ToString() + SEPARATOR) >= 0);
                        {
                            string[] aTmp = Convert.ToString(alSelect[1][i]).Split(DDLDATASEPARATOR.ToCharArray());
                            string tmp_TABLENAME = aTmp[3];
                            string tmp_COLUMNNAME = aTmp[4];

                            isMandatory = (lstIdLstColumMandatory.IndexOf(SEPARATOR + tmp_TABLENAME + ":" + tmp_COLUMNNAME + SEPARATOR) >= 0);
                        }

                        //NB: Remove des éventuels "Retour chariot"
                        ddlSelect.Items.Add(
                            new ListItem(alSelect[0][i].ToString().Replace(Cst.HTMLBreakLine, " "),
                                (isMandatory ? "1" : "0") + DDLDATASEPARATOR + alSelect[1][i].ToString()));

                    }
                }
            }
            //Add a "Empty" item
            //ddlSelect.Items.Insert(0, new ListItem(" ", 
            //    "0" + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR 
            //    + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString() + DDLDATASEPARATOR + (-1).ToString()));
            //NB: itemvalue --> ISMANDATORY,IDLSTCOLUMN,ALIAS,DATATYPE,TABLENAME,COLUMNNAME,,,,Referential
            ddlSelect.Items.Insert(0, new ListItem(" ",
                "0" + DDLDATASEPARATOR + "-1" + DDLDATASEPARATOR + string.Empty + DDLDATASEPARATOR + string.Empty + DDLDATASEPARATOR + string.Empty + DDLDATASEPARATOR
                + string.Empty + DDLDATASEPARATOR + string.Empty + DDLDATASEPARATOR + string.Empty + DDLDATASEPARATOR + string.Empty + DDLDATASEPARATOR + string.Empty));

            return ddlSelect;
        }
        #endregion

        #region UpdateLabelClassForDDLOptionGroupList
        /// <summary>
        /// Calcul de la "Class" pour le Label associé en fonction du contexte (Checked/Unchecked filter, New filter)
        /// <para>NB: Seuls les Filtres sur les consultations (multi-tables) affichent un Label. Les Filtres sur les référentiels n'affichent pas ce Label.</para>
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void UpdateLabelClassForDDLOptionGroupList()
        {
            // FI 20191029 [XXXXX] pas de boucle si isPostBack_ChangeOfColumn 
            int max;
            int min;
            if (isPostBack_ChangeOfColumn)
            {
                min = StrFunc.GetSuffixNumeric2(dataFrom__EVENTTARGET);
                max = min;
            }
            else
            {
                min = 0;
                max = GetControlIDMax();
            }


            OptionGroupDropDownList ddlColumnName;
            CheckBox chkColumnName;
            for (int li = min; li <= max; li++)
            {
                //chkColumnName = PlhMain.FindControl("CHK" + li.ToString()) as CheckBox;
                //ddlColumnName = PlhMain.FindControl(DDL_COLUMNNAME + li.ToString()) as OptionGroupDropDownList;
                chkColumnName = lstCriteria["CHK" + li.ToString()] as CheckBox;
                ddlColumnName = lstCriteria[DDL_COLUMNNAME + li.ToString()] as OptionGroupDropDownList;

                if ((null != chkColumnName) && (null != ddlColumnName))
                {
                    bool isFilterChecked = chkColumnName.Checked;

                    ddlColumnName.LabelClass = "criteriaNew";
                    if (StrFunc.IsFilled(ddlColumnName.SelectedItem.Text))
                        ddlColumnName.LabelClass = (isFilterChecked ? "criteriaSelected" : "criteriaUnSelected");

                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //PL 20180529 New feature: on enable/disable la checkbox  
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    if ((lstCriteria[ControlID.GetID("TOOLTIP" + li.ToString(), null, Cst.IMG)] is WCToolTipPanel pnlTooltip) &&
                        !pnlTooltip.CssClass.Contains("mandatory"))
                    {
                        pnlTooltip.CssClass = "fa-icon fas fa-info-circle " + (isFilterChecked ? "selected" : string.Empty);
                        if (ddlColumnName.SelectedIndex == 0)
                            chkColumnName.Enabled = false;
                    }

                    if (lstCriteria[DDL_OPERATOR + li.ToString()] is DropDownList ddlOperator)
                        ddlOperator.Enabled = isFilterChecked;

                    if (lstCriteria[TXT_VALUE + li.ToString()] is TextBox txtValue)
                        txtValue.Enabled = isFilterChecked;

                    if (lstCriteria[BUT_PTSFORLIST + li.ToString()] is WCToolTipLinkButton lnkToolTip)
                        lnkToolTip.Visible = isFilterChecked;
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                }
            }
        }
        #endregion UpdateLabelClassForDDLOptionGroupList

        /// <summary>
        ///  Retourne le nombre de critrère 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCountLstWhere"></param>
        /// <param name="pCountLstWhereMandatory"></param>
        /// FI 20191107 Add Method
        private void GetNbCriteria(string pCS, out int pCountLstWhere, out int pCountLstWhereMandatory)
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            string key = BuildDataCacheKey("nbCriteria");

            if (!IsPostBack)
            {
                GetSQLNbCriteria(pCS, out pCountLstWhere, out pCountLstWhereMandatory);
                DataCache.SetData(key, new Pair<int, int>(pCountLstWhere, pCountLstWhereMandatory));
            }
            else
            {
                //Pair<int, int> returnSession = Session[key] as Pair<int, int>;
                Pair<int, int> returnSession = DataCache.GetData<Pair<int, int>>(key);
                if (null != returnSession)
                {
                    pCountLstWhere = returnSession.First;
                    pCountLstWhereMandatory = returnSession.Second;
                }
                else
                {
                    GetSQLNbCriteria(pCS, out pCountLstWhere, out pCountLstWhereMandatory);
                    //Session[key] = new Pair<int, int>(pCountLstWhere, pCountLstWhereMandatory);
                    DataCache.SetData(key, new Pair<int, int>(pCountLstWhere, pCountLstWhereMandatory));
                }
            }
        }

        /// <summary>
        ///  Execute requête SQL retourner le nombre de critrères 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCountLstWhere"></param>
        /// <param name="pCountLstWhereMandatory"></param>
        /// FI 20191107 Add Method
        private void GetSQLNbCriteria(string pCS, out int pCountLstWhere, out int pCountLstWhereMandatory)
        {

            pCountLstWhere = 0;
            pCountLstWhereMandatory = 0;

            //PL 20180529 Add Group By
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += SQLCst.COUNT_1 + " as NbOfFilter, ISMANDATORY" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDLSTTEMPLATE=@IDLSTTEMPLATE";
            sqlSelect += SQLCst.AND + "IDLSTCONSULT=@IDLSTCONSULT";
            sqlSelect += SQLCst.AND + "IDA=@IDA" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "ISMANDATORY" + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "ISMANDATORY desc";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDLSTTEMPLATE", DbType.AnsiString, 64), queryStringidLstTemplate);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDLSTCONSULT), idLstConsult);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IntFunc.IntValue(queryStringIdA));

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            //20101011 FI Super,Mega,Strange => La query avec utilisation de paramètres donne 0 sur Oracle avec le provider .net Oracle  !!*
            //Spheres® remplace les paramètres par leurs valeurs respectives
            //object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, query);
            //object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            //if (null != obj)
            //    countLstWhere = Convert.ToInt32(obj);
            //PL 20180529 Use ExecuteReader and New query with Group By
            IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            while (dr.Read())
            {
                if (Convert.ToBoolean(dr["ISMANDATORY"]))
                {
                    pCountLstWhereMandatory = Convert.ToInt32(dr["NbOfFilter"]);
                    pCountLstWhere = pCountLstWhereMandatory;
                }
                else
                {
                    pCountLstWhere += Convert.ToInt32(dr["NbOfFilter"]);
                }
            }
            if (null != dr)
            {
                dr.Close();
            }

        }


        /// <summary>
        /// Retourne la table qui permet d'ouvrir le référentiel associé
        /// </summary>
        /// <param name="pTableName"></param>
        /// <returns></returns>
        /// FI 2020010 [XXXXX] Add Method
        // EG 20210412 [XXXXX] Ajout test pour TRADE (sur Trade administratif)
        private string GetReferentialTableName(string pTableName)
        {
            string ret = pTableName;

            switch (pTableName)
            {
                case "CONTRACT":
                    if (IsFilterOnProductOnly(Cst.ProductExchangeTradedDerivative))
                        ret = "DERIVATIVECONTRACT";
                    else if (IsFilterOnProductOnly(Cst.ProductCommoditySpot))
                        ret = "COMMODITYCONTRACT";
                    else
                        ret = "VW_CONTRACT"; //permet d'avoir de l'autocomplete sur les colonnes également présente dans VW_CONTRACT
                    break;
                case "ASSET_ETD_":
                    ret = "ASSET_ETD";
                    break;
                case "VW_ASSET_ETD_EXPANDED":
                    //Nothing
                    break;
                case "ASSET_ETD_ESE_COM":
                    if (IsFilterOnProductOnly(Cst.ProductExchangeTradedDerivative))
                        ret = "VW_ASSET_ETD_EXPANDED";
                    else if (IsFilterOnProductOnly(Cst.ProductEquitySecurityTransaction))
                        ret = "ASSET_EQUITY";
                    else if (IsFilterOnProductOnly(Cst.ProductCommoditySpot))
                        ret = "VW_ASSET_COMMODITY_EXPANDED";
                    break;
                case "COMMONASSET":
                    if (IsFilterOnProductOnly(Cst.ProductExchangeTradedDerivative))
                        ret = "VW_ASSET_ETD_EXPANDED";
                    else if (IsFilterOnProductOnly(Cst.ProductEquitySecurityTransaction))
                        ret = "ASSET_EQUITY";
                    else if (IsFilterOnProductOnly(Cst.ProductDebtSecurityTransaction))
                        ret = "ASSET_DEBTSECURITY";
                    else if (IsFilterOnProductOnly(Cst.ProductCommoditySpot))
                        ret = "VW_ASSET_COMMODITY_EXPANDED";
                    else
                        ret = "VW_ASSET"; //permet d'avoir de l'autocomplete sur les colonnes également présente dans VW_ASSET
                    break;
                case "ASSET":
                    ret = "VW_ASSET";
                    break;
                case "BOOK_ETD":
                case "VW_BOOK_ACTORLEVEL":
                    ret = "BOOK";
                    break;
                case "ACTOR_CLEARER":
                case "ACTOR_EXECUTOR":
                    ret = "ACTOR";
                    break;
                case "TRADE":
                    if ("TRADEADMIN" == idLstConsult)
                        ret = "TRADEADMIN";
                    break;
                default:
                    break;
            }
            return ret;
        }

        /// <summary>
        ///  Retourne true si la consultation en vigeur ne concerne que le produit {pProductIdentifier}
        ///  <para>Return true si la license n'autorise que {pProductIdentifier} ou s'il existe un filtre sur {pProductIdentifier} uniquement</para>
        /// </summary>
        /// <param name="pProductIdentifier"></param>
        /// <returns></returns>
        /// FI 20200101 [XXXXX] Add Method
        /// FI 20201214 [XXXXX] Add test sur operator "="
        private Boolean IsFilterOnProductOnly(string pProductIdentifier)
        {
            LimitationProductEnum limitationProductEnum = (LimitationProductEnum)Enum.Parse(typeof(LimitationProductEnum), pProductIdentifier);
            Boolean ret = SessionTools.License.IsLicProductAuthorised_AddOnly(limitationProductEnum);
            if (false == ret)
            {
                List<string> lstProductIdentifierCriteria = new List<string>();
                int max = GetControlIDMax();
                for (int li = 0; li <= max; li++)
                {
                    DropDownList ddlColumnName = lstCriteria[DDL_COLUMNNAME + li.ToString()] as DropDownList;
                    DropDownList dllOperator = lstCriteria[DDL_OPERATOR + li.ToString()] as DropDownList;
                    // FI 20210412 [XXXXX] Add test Checked pour ne considérer que les critères checked
                    CheckBox chkEnabled = lstCriteria[Cst.CHK + li.ToString()] as CheckBox;
                    if (chkEnabled.Checked && StrFunc.IsFilled(ddlColumnName.SelectedValue) && ddlColumnName.SelectedValue.Contains("PRODUCT,IDENTIFIER") && dllOperator.SelectedValue == "=")
                    {
                        TextBox ctrlFound = lstCriteria[TXT_VALUE + li.ToString()] as TextBox;
                        if (StrFunc.IsFilled(ctrlFound.Text))
                        {
                            lstProductIdentifierCriteria.AddRange(ctrlFound.Text.Split(';').Distinct().ToList());
                            if (lstProductIdentifierCriteria.Count > 1)
                                break;
                        }
                    }
                }
                ret = (lstProductIdentifierCriteria.Count() == 1 && lstProductIdentifierCriteria[0] == pProductIdentifier);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200602 [25370] Add Method
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        private void InsertLstWhere()
        {
            string CS = SessionTools.CS;
            //getting max controlID
            int max = GetControlIDMax();

            bool isMandatory;
            bool bISENABLED;
            string sIDLSTCOLUMN;
            string sTABLENAME;
            string sCOLUMNNAME;
            string sALIAS;
            string sOPERATOR;
            string sLSTVALUE;
            string sDATATYPE;
            string sTemp;
            Control ctrl;

            #region SQLQuery / DataParameter
            string SQLQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LSTWHERE.ToString() + "(";
            //SQLQuery += "IDLSTCONSULT, IDLSTTEMPLATE, IDA, IDLSTCOLUMN, ALIAS, POSITION," + Cst.CrLf;
            SQLQuery += "IDLSTCONSULT,IDLSTTEMPLATE,IDA,IDLSTCOLUMN,TABLENAME,COLUMNNAME,ALIAS,POSITION,OPERATOR,LSTVALUE,LSTIDVALUE,ISENABLED,ISMANDATORY";
            SQLQuery += ")" + Cst.CrLf;
            //SQLQuery += SQLCst.SELECT + "@IDLSTCONSULT, @IDLSTTEMPLATE, @IDA, @IDLSTCOLUMN, @ALIAS, @POSITION,";
            SQLQuery += SQLCst.SELECT + "@IDLSTCONSULT,@IDLSTTEMPLATE,@IDA,@IDLSTCOLUMN,@TABLENAME,@COLUMNNAME,@ALIAS,@POSITION,";
            SQLQuery += "case when @LSTVALUE is null then " + DataHelper.SQLIsNull(CS, "lcw.OPERATOR", "@OPERATOR") + " else @OPERATOR end,";
            SQLQuery += "case when @LSTVALUE is null then lcw.LSTVALUE else @LSTVALUE end,";
            SQLQuery += "@LSTIDVALUE, @ISENABLED, @ISMANDATORY" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTCONSULT.ToString() + " lc" + Cst.CrLf;
            SQLQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.LSTCONSULTWHERE.ToString() + " lcw on (lcw.IDLSTCONSULT=lc.IDLSTCONSULT and lcw.POSITION=@POSITION)" + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "(lc.IDLSTCONSULT=" + DataHelper.SQLString(idLstConsult) + ")" + Cst.CrLf;

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
            for (int i = 0; i <= max; i++)
            {
                ctrl = PlhMain.FindControl(DDL_COLUMNNAME + i.ToString());
                //Exemple de ce que peut contenir la variable sTemp: "0,2,tblmain,string,MARKET,IDENTIFIER,,,,Referential"
                sTemp = ((DropDownList)ctrl).SelectedValue;
                string[] asTemp = sTemp.Split(DDLDATASEPARATOR.ToCharArray());
                sIDLSTCOLUMN = asTemp[1];
                sTABLENAME = asTemp[4];
                sCOLUMNNAME = asTemp[5];
                if (StrFunc.IsFilled(sTABLENAME))
                {
                    isMandatory = (asTemp[0] == "1");
                    //20070522 PL Tip pour éviter de considérer une colonne mandatory, comme n fois mandatory
                    if (isMandatory)
                    {
                        if (listMandatoryColumn.IndexOf(sTABLENAME + ":" + sCOLUMNNAME + "|") < 0)
                            listMandatoryColumn += sTABLENAME + ":" + sCOLUMNNAME + "|";
                        else
                            isMandatory = false;
                    }
                    sALIAS = asTemp[2];
                    sDATATYPE = asTemp[3];

                    ctrl = PlhMain.FindControl(DDL_OPERATOR + i.ToString());
                    sOPERATOR = ((DropDownList)ctrl).SelectedValue;

                    ctrl = PlhMain.FindControl(TXT_VALUE + i.ToString());
                    sLSTVALUE = ((TextBox)ctrl).Text;
                    if (StrFunc.IsFilled(sLSTVALUE))
                    {
                        ReferentialsReferentialColumnDataType rDatatype = new ReferentialsReferentialColumnDataType
                        {
                            value = sDATATYPE
                        };
                        sLSTVALUE = LstConsult.FormatLstValue2(CS, sLSTVALUE, rDatatype, false, false, true, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter);
                    }

                    ctrl = PlhMain.FindControl(Cst.CHK + i.ToString());
                    bISENABLED = ((CheckBox)ctrl).Checked;

                    parameters["ISMANDATORY"].Value = isMandatory;
                    parameters["ISENABLED"].Value = bISENABLED;
                    parameters["IDLSTCONSULT"].Value = idLstConsult;
                    parameters["IDLSTTEMPLATE"].Value = idLstTemplate;
                    parameters["IDA"].Value = idA;
                    parameters["IDLSTCOLUMN"].Value = sIDLSTCOLUMN;
                    parameters["TABLENAME"].Value = sTABLENAME;
                    parameters["COLUMNNAME"].Value = sCOLUMNNAME;
                    parameters["ALIAS"].Value = sALIAS;
                    parameters["POSITION"].Value = i;
                    parameters["OPERATOR"].Value = sOPERATOR;
                    if (StrFunc.IsFilled(sLSTVALUE))
                        parameters["LSTVALUE"].Value = sLSTVALUE;
                    else
                        parameters["LSTVALUE"].Value = DBNull.Value;
                    parameters["LSTIDVALUE"].Value = DBNull.Value;

                    QueryParameters qry = new QueryParameters(CS, SQLQuery, parameters);
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                }
            }
        }

        /// <summary>
        ///  Test la présence la la valeur session queryStringSessionName_LstColumn
        ///  <para>si cette variable est non présence, Spheres® affiche une alerte javascript et ferme la fenêtre</para>
        /// </summary>
        /// FI 20210125 [XXXXX] Add 
        // EG 20210222 [XXXXX] Appel OpenerRefreshAndClose (présent dans PageBase.js) en remplacement de OpenerCallRefreshAndClose2
        private void CheckSessionState()
        {
            // La méthode ne teste pas toutes les variables sessions de maintient d'état
            if (StrFunc.IsFilled(queryStringSessionName_LstColumn) && (null == LstColumn))
            {
                //Spheres® affiche une alerte javascript 
                JavaScript.AlertImmediate(this, Ressource.GetString("Msg_SessionVariableParentLstCriteriaNotAvailable"));

                //Spheres® rafraichie List.aspx de manière à alimenter la variable session queryStringSessionName_LstColumn
                JavaScript.CallFunction(this, String.Format("OpenerRefreshAndClose('{0}','{1}')", PageName, "SELFCLEAR_"));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="alSelect"></param>
        /// <param name="pIdLstColumn"></param>
        /// <param name="pAlias"></param>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        /// FI 20210125 [XXXXX] Déplacée ici 
        private static int GetIndexInSession_LstColumn(ArrayList[] alSelect, int pIdLstColumn, string pAlias, string pTableName, string pColumnName)
        {
            int ret = -1;

            //finding position of column in the array
            if (ArrFunc.IsFilled(alSelect))
            {
                if (StrFunc.IsFilled(pTableName))
                {
                    string alias = null;
                    for (int i = 0; i < alSelect[0].Count; i++)
                    {
                        //PL 20121129 Bidouille en attendant mieux... On récupère l'ALIAS dans Datacol en 2ème position.
                        if (!String.IsNullOrEmpty(pAlias))
                        {
                            string[] dataCols_Item = Convert.ToString(alSelect[1][i]).Split(',');
                            alias = dataCols_Item[1];
                        }

                        if ((String.IsNullOrEmpty(pAlias) || alias == pAlias)
                            && (Convert.ToString(alSelect[3][i]) == pTableName)
                            && (Convert.ToString(alSelect[4][i]) == pColumnName))
                        {
                            ret = i;
                            break;
                        }
                    }
                }
                else //Compatibilié asc: idLstColumn maintenu pour compatibilié ascendante (à supprimer en v2.7)
                {
                    for (int i = 0; i < alSelect[0].Count; i++)
                    {
                        if (Convert.ToInt32(alSelect[2][i]) == pIdLstColumn)
                        {
                            ret = i;
                            break;
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210125 [XXXXX] Déplacée ici 
        private void WindowSetSize()
        {
            // FI 20131031 [XXXXX] resize dynamique de la page
            int minHeight = ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult) ? 540 : 740;
            int height = minHeight;
            if (nbCriteria > MinNbCriteria)
            {
                // 20 : height ddl 
                // 35 : height ddl + height lbl associé  (OptionGroupDropDownList)  
                int coef = ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult) ? 20 : 35;
                height += (nbCriteria - MinNbCriteria) * coef;
            }
            JQuery.WriteInitialisationScripts(this, "windowSize", StrFunc.AppendFormat("window.resizeTo(1000, {0})", height.ToString()));
        }
    }
}
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EFS.Referential;
using EFS.Rights;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    /// <summary>
    /// Fenetre de selection de template
    /// </summary>
    public partial class LstOpenPage : PageBaseTemplate
    {
        #region Privates Constants
        private const string SEPARATOR_IDA_TEMPLATE = "|";
        private const string SESSION_LSTOPEN_DATASET = "SESSION_LSTOPEN_DATASET";
        private const string RB_NEW = "rbNew";
        private const string RB_OPEN = "rbOpen";
        private const string RB_REMOVE = "rbRemove";
        private const string TXT_QUERYNEW = "txtQueryNew";
        private const string DDL_OWNER = "ddlOwner";
        private const string DDL_OWNERDEL = "ddlOwnerDel";
        private const string DDL_QUERY = "ddlQuery";
        private const string DDL_QUERYDEL = "ddlQueryDel";

        private enum EnumTableDDL
        {
            ACTOR,
            LSTTEMPLATE,
            ACTORDEL,
            LSTTEMPLATEDEL
        };
        #endregion Private(s) Constant(s)

        #region Members
        private bool isReloadDDLTemplate;
        private bool isReloadDDLTemplateDel;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200527 [XXXXX] Add
        protected override string PageName
        {
            get { return "LstOpenPage"; }
        }


        protected override string SpecificSubTitle
        {
            get
            {
                string ret = Ressource.GetString("btnViewerOpen");
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
        #endregion

        #region constructor
        public LstOpenPage()
        {
            isReloadDDLTemplate = false;
            isReloadDDLTemplateDel = false;
        }
        #endregion constructor

        #region protected override CreateControls
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void CreateControls()
        {
            CreateToolBar();
            CreateControlsData();
        }
        #endregion
        #region protected override LoadDataControls
        protected override void LoadDataControls()
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            if (DataCache.GetData<DataSet>(BuildDataCacheKey(SESSION_LSTOPEN_DATASET)) == null)
                DataCache.SetData<DataSet>(BuildDataCacheKey(SESSION_LSTOPEN_DATASET), LoaActorsAndTemplates());

            SetTextBoxData(TXT_QUERYNEW, ReferentialWeb.GetNewQueryName());

            SetScriptEnabledDisabled(RB_NEW);
            SetScriptEnabledDisabled(RB_OPEN);
            SetScriptEnabledDisabled(RB_REMOVE);
        }
        #endregion
        #region protected overrided SaveData
        // EG 20210222 [XXXXX] Appel OpenerRefreshAndClose (présentes dans PageBase.js) en remplacement de OpenerCallRefreshAndClose2
        protected override void SaveData()
        {
            isClose = true;

            string queryName;
            string localIdA;
            if (GetRadioButtonData(RB_NEW))
            {
                #region RB_NEW
                bool isLoadOnStart = false; //False par défaut 
                queryName = GetTextBoxData(TXT_QUERYNEW);
                string queryDisplayname = Ressource.GetMenu_Fullname(IdMenu, queryName);
                queryName = ReferentialWeb.CreateNewTemporaryTemplate(SessionTools.CS, idLstConsult, queryName,  isLoadOnStart, queryDisplayname);
                localIdA = SessionTools.Collaborator_IDA.ToString();
                string urlToOpen = "LstSave.aspx?C=" + idLstConsult + "&T=" + queryName + "&A=" + localIdA + "&ParentGUID=" + ParentGUID + "&IDMenu=" + IdMenu;
                Server.Transfer(urlToOpen, true);
                #endregion
            }
            else
            {
                #region RB_NEW, RD_REMOVE
                string ddlData = null;
                if (GetRadioButtonData(RB_OPEN))
                {
                    ddlData = GetDDLData(DDL_QUERY);
                }
                else if (GetRadioButtonData(RB_REMOVE))
                {
                    ddlData = GetDDLData(DDL_QUERYDEL);
                }

                if (StrFunc.IsEmpty(ddlData))
                {
                    isClose = false;
                    string msgAlert = Ressource.GetStringForJS("Msg_NoTemplateSelected");
                    JavaScript.DialogImmediate(this, msgAlert, false);
                }
                else
                {
                    string[] DDLDatas = ddlData.Split(SEPARATOR_IDA_TEMPLATE.ToCharArray(), 3);
                    queryName = DDLDatas[0];
                    string displayName = DDLDatas[1];
                    localIdA = DDLDatas[2];

                    if (GetRadioButtonData(RB_OPEN))
                    {
                        #region RB_OPEN
                        ReferentialWeb.WriteTemplateSession(idLstConsult, queryName, Convert.ToInt32(localIdA), this.ParentGUID);
                        ThisConsultation.LoadTemplate(SessionTools.CS, queryName, Convert.ToInt32(localIdA));
                        ReferentialWeb.CleanDefault(SessionTools.CS, idLstConsult);
                        string arg = "SELFCLEAR_";
                        if (ThisConsultation.template.ISLOADONSTART)
                            arg = "SELFRELOAD_";
                        JavaScript.CallFunction(this, String.Format("OpenerRefreshAndClose('{0}','{1}')", PageName, arg));
                        #endregion
                    }
                    else if (GetRadioButtonData(RB_REMOVE))
                    {
                        #region RD_REMOVE
                        isClose = false;
                        bool isAllowed = ReferentialWeb.HasUserRightForTemplate(SessionTools.CS,
                            Convert.ToInt32(localIdA),
                            idLstConsult,
                            queryName,
                            RightsTypeEnum.REMOVE);
                        if (isAllowed)
                        {
                            // RD 20091104 
                            // Mettre un ";" pour séparer les valeurs à la place de la virgule 
                            // Eviter le bug en cas ou une modèle de consultation contient une virgule.
                            string eventTarget = "CONFIRMDELETE;" + queryName + ";" + localIdA;
                            string msgConfirm = Ressource.GetStringForJS("Msg_DeleteTemplate");
                            msgConfirm = msgConfirm.Replace("{0}", queryName);
                            msgConfirm = msgConfirm.Replace("{1}", displayName);
                            JavaScript.ConfirmOnStartUp(this, eventTarget, msgConfirm);
                        }
                        else
                        {
                            string msgAlert = Ressource.GetString("Msg_DeleteTemplateNotAllowed");
                            JavaScript.DialogImmediate(this, msgAlert, false);
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        #endregion
        #region protected override OnInit
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            this.Form = frmLstOpen;
            base.OnInit(e);
        }
        #endregion
        #region protected override OnPreRender
        /// FI 20140925 [XXXXX] Modify
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // FI 20200518 [XXXXX]  Utilisation de dataCache
            DataSet ds = DataCache.GetData<DataSet>(BuildDataCacheKey(SESSION_LSTOPEN_DATASET));
            if (!IsPostBack || isReloadPage)
            {
                BindDataDDL(DDL_OWNER, ds.Tables[EnumTableDDL.ACTOR.ToString()], string.Empty, true);
                SetDDLData(DDL_OWNER, SessionTools.Collaborator_IDA.ToString());
                BindDataDDL(DDL_QUERY, ds.Tables[EnumTableDDL.LSTTEMPLATE.ToString()], SessionTools.Collaborator_IDA.ToString(), false);

                BindDataDDL(DDL_OWNERDEL, ds.Tables[EnumTableDDL.ACTORDEL.ToString()], string.Empty, true);
                SetDDLData(DDL_OWNERDEL, SessionTools.Collaborator_IDA.ToString());
                BindDataDDL(DDL_QUERYDEL, ds.Tables[EnumTableDDL.LSTTEMPLATEDEL.ToString()], SessionTools.Collaborator_IDA.ToString(), false);
            }
            if (isReloadDDLTemplate)
            {
                string strDDLIdA = GetDDLData(DDL_OWNER);
                if (IsAllValue(strDDLIdA))
                {
                    //FI 20140925 [XXXXX] Chgt d'un ddl avec regroupement par acteur
                    DropDownList DDLFound = (DropDownList)PlhMain.FindControl(DDL_QUERY);
                    if (null != DDLFound)
                        LoadAllUserTemplate(DDLFound, RightsTypeEnum.VIEW);
                }
                else
                {
                    BindDataDDL(DDL_QUERY, ds.Tables[EnumTableDDL.LSTTEMPLATE.ToString()], strDDLIdA, false);
                }
            }
            if (isReloadDDLTemplateDel)
            {
                string strDDLIdA = GetDDLData(DDL_OWNERDEL);
                if (IsAllValue(strDDLIdA))
                {
                    //FI 20140925 [XXXXX] Chgt d'un ddl avec regroupement par acteur
                    DropDownList DDLFound = (DropDownList)PlhMain.FindControl(DDL_QUERYDEL);
                    if (null != DDLFound)
                        LoadAllUserTemplate(DDLFound, RightsTypeEnum.REMOVE);
                }
                else
                {
                    BindDataDDL(DDL_QUERYDEL, ds.Tables[EnumTableDDL.LSTTEMPLATEDEL.ToString()], strDDLIdA, false);
                }
            }

            SetEnabled();
        }
        #endregion

        #region private InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.AbortRessource = true;
            this.ID = "frmLstOpen";
        }
        #endregion
        #region private Page_Load
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20160309 [XXXXX] Modify
        // EG 20210222 [XXXXX] Suppression inscription function EnabledDisabledChecked et SetFocus
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageTools.SetHead(this, this.Title, null, null);

            isReloadDDLTemplate = (dataFrom__EVENTTARGET == (DDL_OWNER));
            isReloadDDLTemplateDel = (dataFrom__EVENTTARGET == (DDL_OWNERDEL));

            // FI 20160309 [XXXXX] Call Setfocus
            if (StrFunc.IsFilled(dataFrom__EVENTTARGET))
            {
                if (this.FindControl(dataFrom__EVENTTARGET) is DropDownList ddl && ddl.AutoPostBack)
                    SetFocus(ddl.ClientID);
            }

            // RD 20091104 
            // Mettre un ";" pour séparer les valeurs à la place de la virgule 
            // Eviter le bug en cas ou une modèle de consultation contient une virgule.
            if (StrFunc.IsFilled(dataFrom__EVENTTARGET) && dataFrom__EVENTTARGET.StartsWith("CONFIRMDELETE;"))
            {
                //Confirmation pour SUPPRESSION
                if (dataFrom__EVENTARGUMENT == "TRUE")
                {
                    // RD 20091104 
                    // Mettre un ";" pour séparer les valeurs à la place de la virgule 
                    // Eviter le bug en cas ou une modèle de consultation contient une virgule.
                    string[] eventTarget = dataFrom__EVENTTARGET.Split(";".ToCharArray(), 3);
                    string idLstTemplate = eventTarget[1];
                    string idA = eventTarget[2];
                    ReferentialWeb.Delete(SessionTools.CS, idLstConsult, idLstTemplate, Convert.ToInt32(idA), true);

                    isReloadPage = true;
                }
            }
            if (!IsPostBack || isReloadPage)
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //Reset du Dataset
                DataCache.SetData<DataSet>(BuildDataCacheKey(SESSION_LSTOPEN_DATASET), null);
            }

            //JavaScript.EnabledDisabledChecked(this);

            if (!IsPostBack)
            {
                JavaScript.CallFunction(this, "SetFocus('" + RB_OPEN + "')");
            }
        }
        #endregion

        #region CreateToolBar
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateToolBar()
        {
            Panel pnlToolBar = AddToolBar();
            WCToolTipLinkButton btn = AddButtonOkQuery();
            defaultButton = btn.ID;
            pnlToolBar.Controls.Add(btn);
            AddSpace(pnlToolBar);
            pnlToolBar.Controls.Add(AddButtonCancel());
        }
        #endregion CreateToolBar
        #region private CreateControlsData
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateControlsData()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = MainClass };
            Panel pnlData = new Panel() { ID = "divlstopen", CssClass = ControlsTools.MainMenuName(IdMenu) };

            pnlData.Controls.Add(AddRadioButton(RB_NEW, "main"));
            pnlData.Controls.Add(NewRowCapture2(TXT_QUERYNEW, true, false, false, 100, 300));

            pnlData.Controls.Add(AddRadioButton(RB_OPEN, "main"));
            Panel pnl = new Panel();
            pnl.Controls.Add(AddDDLData(DDL_OWNER, true));
            pnl.Controls.Add(AddDDLData(DDL_QUERY, true));
            pnlData.Controls.Add(pnl);

            pnlData.Controls.Add(AddRadioButton(RB_REMOVE, "main"));
            pnl = new Panel();
            pnl.Controls.Add(AddDDLData(DDL_OWNERDEL, true));
            pnl.Controls.Add(AddDDLData(DDL_QUERYDEL, false));
            pnlData.Controls.Add(pnl);

            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu) };
            togglePanel.SetHeaderTitle(Ressource.GetString("Model"));
            togglePanel.AddContent(pnlData);
            pnlBody.Controls.Add(togglePanel);
            PlhMain.Controls.Add(pnlBody);
        }
        #endregion

        #region private BindDataDDL
        // EG 20210330 [XXXXX] Gestion message si pas de template
        private DropDownList BindDataDDL(string ctrlID, DataTable dtDDL, string pValueFilter, bool isWithItemAll)
        {
            if (pValueFilter != string.Empty)
                dtDDL.DefaultView.RowFilter = IsAllValue(pValueFilter) ? string.Empty : "FKEY=" + pValueFilter;

            DropDownList DDLFound = (DropDownList)PlhMain.FindControl(ctrlID);
            DDLFound.DataSource = dtDDL;
            DDLFound.DataTextField = "DISPLAYCOL";
            DDLFound.DataValueField = "DATACOL";
            DDLFound.DataBind();
            //PL 20150703 Sur DDL_OWNER, [Tous] permet d'accéder au Template (dont pour rappel le owner est SYSTEM).
            //if (isWithItemAll && DDLFound.Items.Count > 1) 
            if (isWithItemAll && ((DDLFound.Items.Count > 1) || (ctrlID == DDL_OWNER))) 
                ControlsTools.DDLLoad_AddListItemAllAll(DDLFound);

            DDLFound.Visible = (DDLFound.Items.Count > 0);
            DDLFound.Enabled = (DDLFound.Items.Count > 1);

            if (ctrlID.Contains("Query"))
                DisplayMessage(DDLFound);
            return DDLFound;
        }

        // EG 20210330 [XXXXX] Gestion message si pas de template
        private void DisplayMessage(WebControl pControl)
        {
            Label LBLFound = (Label)PlhMain.FindControl(Cst.LBL + pControl.ID.Substring(3) + "_MSG");
            if (null != LBLFound)
                LBLFound.Visible = (false == pControl.Visible);
        }
        private bool IsAllValue(string pData)
        {
            return (pData == Cst.DDLVALUE_ALL) || (pData == Cst.DDLVALUE_ALL_Old);
        }
        #endregion

        #region private LoaActorsAndTemplates
        /// <summary>
        /// Chargement des Acteurs (disposant de Templates) et des Templates.
        /// <para>NB: Pour un user non SYSADMIN, seuls sont conservés les Templates sur lesquels l'utilisateur dispose des droits resquis.</para>
        /// <para>NB: Pour un user GUEST, seuls sont chargés ses Templates et ceux de son parent "direct".</para>
        /// </summary>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        private DataSet LoaActorsAndTemplates()
        {
            bool isOracle = DataHelper.IsDbOracle(SessionTools.CS);
            DataSet ds = new DataSet();

            try
            {
                //Warning: 20050913 PL (Il faudrait tester avec le provider Oracle (ODP.net))
                //Code spécifique pour Oracle qui ne supporte pas le multiple resultsets au sein d'un dataset (Error: invalid character)
                if (isOracle)
                {
                    DataTable dt;

                    DataSet dsOraActors = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, GetSQLSelect_Actors(false));
                    dt = dsOraActors.Tables[0].Copy();
                    dt.TableName = "T1";
                    ds.Tables.Add(dt);

                    DataSet dsOraTemplates = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, GetSQLSelect_Templates());
                    dt = dsOraTemplates.Tables[0].Copy();
                    dt.TableName = "T2";
                    ds.Tables.Add(dt);

                    if (SessionTools.IsSessionGuest)
                    {
                        DataSet dsOraActorsDel = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, GetSQLSelect_Actors(SessionTools.IsSessionGuest));
                        dt = dsOraActorsDel.Tables[0].Copy();
                    }
                    else
                    {
                        dt = dsOraActors.Tables[0].Copy();
                    }
                    dt.TableName = "T3";
                    ds.Tables.Add(dt);

                    dt = dsOraTemplates.Tables[0].Copy();
                    dt.TableName = "T4";
                    ds.Tables.Add(dt);
                }
                else
                {
                    string SQLSelect = string.Empty;
                    //Actors
                    SQLSelect += GetSQLSelect_Actors(false) + ";" + Cst.CrLf;
                    //Templates
                    SQLSelect += GetSQLSelect_Templates() + ";" + Cst.CrLf;
                    //Actors from Template allowed to del
                    SQLSelect += GetSQLSelect_Actors(SessionTools.IsSessionGuest) + ";" + Cst.CrLf;
                    //Templates allowed to Del
                    SQLSelect += GetSQLSelect_Templates() + ";" + Cst.CrLf;

                    ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, SQLSelect);
                }

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    string tableName = Enum.ToObject(typeof(EnumTableDDL), i).ToString();
                    ds.Tables[i].TableName = tableName;
                    ds.Tables[i].DefaultView.Sort = "DISPLAYCOL";

                    if (!SessionTools.IsSessionSysAdmin)
                    {
                        //Session no SYSADMIN: Apply rights filter to datas for template list
                        //                     Pour les 2 datatable, on vérifie si l'utilisateur dispose des droits requis
                        //                     - VIEW pour le datable destiné au load de la DDL des Templates disponibles pour OPEN 
                        //                     - REMOVE pour le datable destiné au load de la DDL des Templates disponibles pour DELETE 
                        if (tableName.StartsWith(EnumTableDDL.LSTTEMPLATE.ToString()))
                        {
                            RightsTypeEnum neededRight = RightsTypeEnum.VIEW;
                            if (tableName == EnumTableDDL.LSTTEMPLATEDEL.ToString())
                                neededRight = RightsTypeEnum.REMOVE;

                            DataTable dt = ds.Tables[i];
                            //PL 20150601 GUEST New feature
                            //GUEST: Pour un GUEST, seuls ses propres Templates peuvent donner lieu à suppression.
                            bool isGuest_Remove = (SessionTools.IsSessionGuest && (neededRight == RightsTypeEnum.REMOVE));
                            for (int numRow = dt.Rows.Count - 1; numRow > -1; numRow--)
                            {
                                int ida = Convert.ToInt32(dt.Rows[numRow]["IDA"]);
                                string idLstTemplate = Convert.ToString(dt.Rows[numRow]["IDLSTTEMPLATE"]);
                                if ((isGuest_Remove && (ida != SessionTools.Collaborator_IDA))
                                    || (!ReferentialWeb.HasUserRightForTemplate(SessionTools.CS, ida, idLstConsult, idLstTemplate, neededRight)))
                                {
                                    dt.Rows[numRow].Delete();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return ds;
        }
        #endregion

        #region private SetScriptEnabledDisabled
        private void SetScriptEnabledDisabled(string pControlName)
        {
            if (PlhMain.FindControl(pControlName) is Control ctrlFound)
            {
                bool isNewQuery = false;
                bool isChooseQuery = false;
                bool isDeleteQuery = false;
                if (pControlName == RB_NEW)
                    isNewQuery = true;
                else if (pControlName == RB_OPEN)
                    isChooseQuery = true;
                else if (pControlName == RB_REMOVE)
                    isDeleteQuery = true;

                string onClick;
                onClick = GetScriptEnabledDisabled(TXT_QUERYNEW, isNewQuery);
                onClick += GetScriptEnabledDisabled(DDL_OWNER, isChooseQuery);
                onClick += GetScriptEnabledDisabled(DDL_QUERY, isChooseQuery);
                onClick += GetScriptEnabledDisabled(DDL_OWNERDEL, isDeleteQuery);
                onClick += GetScriptEnabledDisabled(DDL_QUERYDEL, isDeleteQuery);
                ((RadioButton)ctrlFound).Attributes.Add("onclick", onClick);
            }
        }
        #endregion
        #region private GetScriptEnabledDisabled
        // EG 20210222 [XXXXX] Upd
        // EG 20210309 [XXXXX] Correction Javascript dans LstOpen.aspx Click sur radio button
        private string GetScriptEnabledDisabled(string pControlName, bool pIsEnabled)
        {
            return String.Format("EnabledDisabledChecked(this, '{0}', true, {1});", pControlName, pIsEnabled.ToString().ToLower());
        }
        #endregion
        #region private SetEnabled
        private void SetEnabled()
        {
            bool isSettingRadioButton = false;
            if (Page.IsPostBack)
            {
                //				SetNewEnabled((!isReloadDDLTemplate) && (!isReloadDDLTemplateDel));
                //              SetChooseEnabled(isReloadDDLTemplate);
                //              SetDeleteEnabled(isReloadDDLTemplateDel);            
                //20060420 PL Code ci-dessus mis en commentaire et remplacé par le code ci-dessous
                if (GetRadioButtonData(RB_NEW) || GetRadioButtonData(RB_REMOVE))
                {
                    isSettingRadioButton = true;
                    //
                    SetNewEnabled(GetRadioButtonData(RB_NEW));
                    SetChooseEnabled(false);
                    SetDeleteEnabled(GetRadioButtonData(RB_REMOVE));
                }
            }
            if (!isSettingRadioButton)
            {
                SetNewEnabled(false);
                SetChooseEnabled(true);
                SetDeleteEnabled(false);
            }
        }
        #endregion
        #region private SetNewEnabled
        private void SetNewEnabled(bool pIsEnabled)
        {
            SetRadioButtonData(RB_NEW, pIsEnabled);
            SetTextBoxEnabled(TXT_QUERYNEW, pIsEnabled);
        }
        #endregion
        #region private SetChooseEnabled
        private void SetChooseEnabled(bool pIsEnabled)
        {
            SetRadioButtonData(RB_OPEN, pIsEnabled);
            SetDDLEnabled(DDL_OWNER, pIsEnabled);
            SetDDLEnabled(DDL_QUERY, pIsEnabled);
        }
        #endregion
        #region private SetDeleteEnabled
        private void SetDeleteEnabled(bool pIsEnabled)
        {
            SetRadioButtonData(RB_REMOVE, pIsEnabled);
            SetDDLEnabled(DDL_OWNERDEL, pIsEnabled);
            SetDDLEnabled(DDL_QUERYDEL, pIsEnabled);
        }
        #endregion

        /// <summary>
        /// Retourne l'ordre SELECT pour charger les Acteurs
        /// </summary>
        /// <returns></returns>
        private string GetSQLSelect_Actors(bool pIsForCurrentUserOnly)
        {
            StrBuilder ret = new StrBuilder();

            //Acteur utilisateur connecté
            ret += SQLCst.SELECT + "a.IDA as DATACOL," + GetSQLDisplayColActor(true) + " as DISPLAYCOL" + Cst.CrLf;
            ret += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            ret += SQLCst.WHERE + "(a.IDA=" + SessionTools.Collaborator_IDA.ToString() + ")" + Cst.CrLf;

            if (!pIsForCurrentUserOnly)
            {
                ret += SQLCst.UNION + Cst.CrLf;

                //Acteur(s) disposant d'au moins 1 Template (non temporaire) sur la consultation concernée
                ret += SQLCst.SELECT_DISTINCT + "a.IDA as DATACOL," + GetSQLDisplayColActor(false) + " as DISPLAYCOL" + Cst.CrLf;
                ret += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                ret += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt on (lt.IDA=a.IDA) and (lt.IDLSTCONSULT=" + DataHelper.SQLString(idLstConsult) + ")";
                ret += SQLCst.AND + LstConsult.LstTemplate.GetSQLWhereNotTemporary("lt") + Cst.CrLf;
                
                if (SessionTools.IsSessionGuest) // FI 20210409 [XXXXX] Ajout du if
                {
                    //PL 20150601 GUEST New feature
                    //GUEST: Si celui-ci possède un DEPARTMENT dans ses parents on reteinet celui-ci, sinon on retient son parent direct.
                    if (SessionTools.Collaborator.Department.Ida > 0)
                    {
                        ret += SQLCst.AND + "lt.IDA=" + SessionTools.Collaborator.Department.Ida.ToString() + Cst.CrLf;
                    }
                    else
                    {
                        ret += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar on (ar.IDA_ACTOR=a.IDA) and (ar.IDROLEACTOR='" + Actor.RoleActor.GUEST.ToString() + "')" + Cst.CrLf;
                    }
                }

                ret += SQLCst.WHERE + "(a.IDA not in (1," + SessionTools.Collaborator_IDA.ToString() + "))" + Cst.CrLf;
            }

            return ret.ToString();
        }

        /// <summary>
        /// Retourne l'ordre SELECT pour charger les Templates
        /// </summary>
        /// <returns></returns>
        private string GetSQLSelect_Templates()
        {
            string addWhere = string.Empty;

            StrBuilder ret = new StrBuilder(SQLCst.SELECT_DISTINCT);

            ret += GetSQLDataColTemplate() + " as DATACOL," + Cst.CrLf;
            ret += GetSQLDisplayColTemplate() + " as DISPLAYCOL," + Cst.CrLf;
            ret += "lt.IDA as FKEY, lt.IDLSTTEMPLATE, lt.IDA," + Cst.CrLf;
            ret += "lt.IDA as OPTGROUP," + GetSQLDisplayColActor(false) + " as OPTGROUPTEXT, case when a.IDA=1 then ' ' || a.DISPLAYNAME else a.DISPLAYNAME end as OPTGROUPSORT" + Cst.CrLf;
            ret += SQLCst.FROM_DBO + Cst.OTCml_TBL.LSTTEMPLATE.ToString() + " lt" + Cst.CrLf;
            ret += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a on (a.IDA=lt.IDA)" + Cst.CrLf;
            //PL 20150601 GUEST New feature
            if (SessionTools.IsSessionGuest)
            {
                //GUEST: Si celui-ci possède un DEPARTMENT dans ses parents on retient les templates de celui-ci, sinon on retient ceux de son parent direct.
                if (SessionTools.Collaborator.Department.Ida > 0)
                {
                    ret += SQLCst.AND + "a.IDA in (" + SessionTools.Collaborator_IDA.ToString() + "," + SessionTools.Collaborator.Department.Ida.ToString() + ")" + Cst.CrLf;
                }
                else
                {
                    ret += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar on (ar.IDA_ACTOR=a.IDA) and (ar.IDROLEACTOR='" + Actor.RoleActor.GUEST.ToString() + "')" + Cst.CrLf;
                    addWhere = " and ((a.IDA=" + SessionTools.Collaborator_IDA.ToString() + ") or (ar.IDA=" + SessionTools.Collaborator_IDA.ToString() + "))" + Cst.CrLf;
                }
            }
            ret += SQLCst.WHERE + "(lt.IDLSTCONSULT=" + DataHelper.SQLString(idLstConsult) + ")";
            ret += SQLCst.AND + LstConsult.LstTemplate.GetSQLWhereNotTemporary("lt") + Cst.CrLf;
            ret += addWhere;

            return ret.ToString();
        }

        /// <summary>
        /// Retourne l'expression SQL de la colonne qui alimente les value des DDLs template
        /// </summary>
        /// <returns></returns>
        private string GetSQLDataColTemplate()
        {
            return DataHelper.SQLConcat(SessionTools.CS, "lt.IDLSTTEMPLATE", "'" + SEPARATOR_IDA_TEMPLATE + "'", "a.DISPLAYNAME", "'" + SEPARATOR_IDA_TEMPLATE + "'", DataHelper.SQLNumberToChar(SessionTools.CS, "lt.IDA"));
        }

        /// <summary>
        /// Retourne l'expression SQL de la colonne qui alimente le text des DDLs templates
        /// </summary>
        /// <returns></returns>
        private static string GetSQLDisplayColTemplate()
        {
            return "case when lt.IDLSTTEMPLATE != lt.DISPLAYNAME then lt.IDLSTTEMPLATE || ' - ' ||  lt.DISPLAYNAME else lt.IDLSTTEMPLATE end";
        }

        /// <summary>
        /// Retourne l'expression SQL de la colonne qui alimente le text des DDLs acteurs
        /// </summary>
        /// <param name="pAddFlagCurrentUser"></param>
        /// <returns></returns>
        private string GetSQLDisplayColActor(bool pAddFlagCurrentUser)
        {
            if (pAddFlagCurrentUser)
            {
                string flagCurrentUser = DataHelper.SQLString(" [" + Ressource.GetString("ConnectedUser_Title") + "]");
                if (SessionTools.IsSessionGuest)
                    return DataHelper.SQLConcat(SessionTools.CS, "a.DISPLAYNAME", flagCurrentUser);
                else
                    return DataHelper.SQLConcat(SessionTools.CS, "a.DISPLAYNAME", "' (Id:'", DataHelper.SQLNumberToChar(SessionTools.CS, "a.IDA"), "')'", flagCurrentUser);
            }
            else
            {
                if (SessionTools.IsSessionGuest)
                    return "a.DISPLAYNAME";
                else
                    return DataHelper.SQLConcat(SessionTools.CS, "a.DISPLAYNAME", "' (Id:'", DataHelper.SQLNumberToChar(SessionTools.CS, "a.IDA"), "')'");
            }
        }

        /// <summary>
        /// Chargement d'une DDL avec les templates regroupés par acteur 
        /// </summary>
        /// <param name="DDLFound">DDL</param>
        /// <<param name="pRightsTypeEnum">Type de droit nécessaire poour afficher un template</param>
        private void LoadAllUserTemplate(DropDownList DDLFound, RightsTypeEnum pRightsTypeEnum)
        {
            string SQLSelect = GetSQLSelect_Templates();
            ControlsTools.DDLLoad(this, DDLFound, "DISPLAYCOL", "DATACOL", true, 0, null, SessionTools.CS, SQLSelect, false, true, null);

            DDLFound.Visible = (DDLFound.Items.Count > 0);
            if (DDLFound.Items.Count > 0)
            {
                if (!SessionTools.IsSessionSysAdmin)
                {
                    //PL 20150601 GUEST New feature
                    //GUEST: Pour un GUEST, seuls ses propres Templates peuvent donner lieu à suppression.
                    bool isGuest_Remove = (SessionTools.IsSessionGuest && (pRightsTypeEnum == RightsTypeEnum.REMOVE));
                    for (int i = ArrFunc.Count(DDLFound.Items) - 1; i > -1; i--)
                    {
                        string[] value = DDLFound.Items[i].Value.Split(new char[] { '|' });

                        int ida = Convert.ToInt32(value[2]);
                        string idLstTemplate = Convert.ToString(value[0]);
                        if ((isGuest_Remove && (ida != SessionTools.Collaborator_IDA))
                            || (!ReferentialWeb.HasUserRightForTemplate(SessionTools.CS, ida, idLstConsult, idLstTemplate, pRightsTypeEnum)))
                        {
                            DDLFound.Items.Remove(DDLFound.Items[i]);
                        }
                    }
                }
            }
            DisplayMessage(DDLFound);
        }
    }
}
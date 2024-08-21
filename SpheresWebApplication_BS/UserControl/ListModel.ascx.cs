using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using EFS.Rights;

namespace EFS.ListControl
{
    public partial class ListModel : UserControl
    {
        #region Privates Constants
        private const string SEPARATOR_IDA_TEMPLATE = "|";
        private const string SESSION_LSTOPEN_DATASET = "SESSION_LSTOPEN_DATASET";
        private const string RB_NEW = "rbNew";
        private const string RB_OPEN = "rbOpen";
        private const string RB_REMOVE = "rbRemove";
        private const string DDL_OWNEROPEN = "ddlOwnerOpen";
        private const string DDL_OWNERREMOVE = "ddlOwnerRemove";
        private const string DDL_MODELOPEN = "ddlModelOpen";
        private const string DDL_MODELREMOVE = "ddlModelRemove";

        private enum EnumTableDDL
        {
            ACTOR,
            LSTTEMPLATE,
            ACTORREMOVE,
            LSTTEMPLATEREMOVE
        };
        #endregion Private(s) Constant(s)
        #region Members
        private Uri urlReferrer;
        private bool isReloadDDLTemplateOpen;
        private bool isReloadDDLTemplateRemove;
        #endregion Members
        #region Accessors
        private bool IsSaveMode
        {
            get { return (rbSave.Checked || (rbSave.InputAttributes["checked"] == "true")); }
        }
        private bool IsRemoveMode
        {
            get { return (rbRemove.Checked || (rbRemove.InputAttributes["checked"] == "true")); }
        }
        private bool IsOpenMode
        {
            get { return (rbOpen.Checked || (rbOpen.InputAttributes["checked"] == "true"));}
        }
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
        /// <summary>
        /// Retour à la page appelante 
        /// on passe en paramètre le GUID de la page appelante pour recharger le template sélectionné
        /// </summary>
        private Uri ReturnUrlReferer
        {
            get {

                UriBuilder builder = new UriBuilder(urlReferrer);
                if (StrFunc.IsFilled(builder.Query))
                {
                    string query = builder.Query.Split("?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList().LastOrDefault();

                    if (StrFunc.IsFilled(query))
                    {
                        List<string> itemQuery = query.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                        query = string.Empty;
                        itemQuery.ForEach(item =>
                        {
                            if (false == item.StartsWith("GUID="))
                                query += "&" + item;
                        });
                        if (StrFunc.IsFilled(query))
                            query = query.Substring(1) + "&";

                        builder.Query = query + "GUID=" + ParentGUID;
                    }
                }
                return builder.Uri; 
            }
        }

        public RadioButton RbOpen
        {
            get { return rbOpen; }
        }
        public RadioButton RbSave
        {
            get { return rbSave; }
        }
        public RadioButton RbRemove
        {
            get { return rbRemove; }
        }
        #endregion Accessors


        #region Methods

        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            isReloadDDLTemplateOpen = false;
            isReloadDDLTemplateRemove = false;
            base.OnInit(e);
        }
        #endregion OnInit
        #region Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            isReloadDDLTemplateOpen = false;
            isReloadDDLTemplateRemove = false;
            urlReferrer = Request.UrlReferrer;
            SetKeyAndLoadTemplate();
            isReloadDDLTemplateOpen = StrFunc.IsFilled(listBase.dataFrom__EVENTTARGET) && listBase.dataFrom__EVENTTARGET.EndsWith(DDL_OWNEROPEN);
            isReloadDDLTemplateRemove = StrFunc.IsFilled(listBase.dataFrom__EVENTTARGET) && listBase.dataFrom__EVENTTARGET.EndsWith(DDL_OWNERREMOVE);
            CreateControls();
        }
        #endregion Page_Load
        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ReloadTemplate();
            DisplayControlsByMode();
        }
        #endregion OnPreRender
        #region OnOpen
        public void OnOpen(object sender, System.EventArgs e)
        {
            listBase.isClose = StrFunc.IsFilled(ddlModelOpen.SelectedValue);
            if (listBase.isClose)
            {
                string[] ddlDatas = ddlModelOpen.SelectedValue.Split(SEPARATOR_IDA_TEMPLATE.ToCharArray(), 3);
                int localIdA = Convert.ToInt32(ddlDatas[2]);
                RepositoryWeb.WriteTemplateSession(this.Page, IdLstConsult, ddlDatas[0], localIdA, ParentGUID);
                Consult.LoadTemplate(CS, ddlDatas[0], localIdA);
                RepositoryWeb.CleanDefault(CS, IdLstConsult);

                // Retour à la page appelante
                if (null != urlReferrer)
                    Response.Redirect(ReturnUrlReferer.AbsoluteUri, true);
            }
        }
        #endregion OnOpen
        #region OnRemove
        public void OnRemove(object sender, System.EventArgs e)
        {
            if (StrFunc.IsFilled(ddlModelRemove.SelectedValue))
            {
                string[] ddlDatas = ddlModelRemove.SelectedValue.Split(SEPARATOR_IDA_TEMPLATE.ToCharArray(), 3);
                int localIdA = Convert.ToInt32(ddlDatas[2]);
                listBase.isClose = false;
                // Contrôle de l'autorisation de suppression
                if (RepositoryWeb.HasUserRightForTemplate(CS, localIdA, IdLstConsult, ddlDatas[0], RightsTypeEnum.REMOVE))
                {
                    RepositoryWeb.Delete(CS, IdLstConsult, ddlDatas[0], localIdA, true);

                    Session[SESSION_LSTOPEN_DATASET + listBase.GUID] = LoaActorsAndTemplates();
                    listBase.isReloadPage = true;
                    isReloadDDLTemplateRemove = true;
                    isReloadDDLTemplateOpen = true;
                }

                // Retour à la page appelante
                //if (null != urlReferrer)
                    //Response.Redirect(ReturnUrlReferer.AbsoluteUri, false);

            }
        }
        #endregion OnRemove
        #region OnValid
        public void OnValid(object sender, System.EventArgs e)
        {
            if (IsSaveMode)
                OnRecord(sender, e);
            else if (IsOpenMode)
                OnOpen(sender, e);
            else if (IsRemoveMode)
                OnRemove(sender, e);
        }
        #endregion OnValid
        #region OnRecord
        public void OnRecord(object sender, System.EventArgs e)
        {
            listBase.isClose = false;
            string queryName;
            string localIdA = null;
            if (IsSaveMode)
            {
                #region Save
                queryName = txtIdentifier.Text;
                string queryDisplayname = Ressource.GetMenu_Fullname(listBase.idMenu, queryName);
                queryName = RepositoryWeb.CreateNewTemporaryTemplate(CS, IdLstConsult, queryName, false, queryDisplayname);
                localIdA = SessionTools.Collaborator_IDA.ToString();

                Consult.LoadTemplate(CS, IdLstTemplate, IdA);

                string rowByPage = txtRowByPage.Text;
                int iRowByPage = 0;
                if (StrFunc.IsFilled(rowByPage))
                {
                    try { iRowByPage = Convert.ToInt32(rowByPage); }
                    catch { iRowByPage = 0; }
                }

                string refreshInterval = txtRefreshInterval.Text;
                int iRefreshInterval = 0;
                if (StrFunc.IsFilled(refreshInterval))
                {
                    try
                    {
                        //FI 20110723 Ajout d'un petit message qui va bien Msg_LstConsultRefresh
                        iRefreshInterval = Convert.ToInt32(refreshInterval);
                        if ((iRefreshInterval > 0) && (iRefreshInterval < RepositoryWeb.MinRefreshInterval))
                        {
                            iRefreshInterval = RepositoryWeb.MinRefreshInterval;
                            //this.MsgForAlertImmediate = Ressource.GetString("Msg_LstConsultRefresh");
                        }
                    }
                    catch
                    {
                        iRefreshInterval = 0;
                    }
                }

                bool isLoadDataOnStart = chkIsLoadOnStart.Checked;
                if ((false == isLoadDataOnStart) && (0 < iRefreshInterval))
                {
                    isLoadDataOnStart = true;
                    //this.MsgForAlertImmediate = Ressource.GetString("Msg_LstConsultRefreshOnStart");
                }

                Consult.template.IDLSTCONSULT = txtIdLstConsult.Text;
                Consult.template.DISPLAYNAME = txtDisplayName.Text;
                Consult.template.DESCRIPTION = txtDescription.Text;
                Consult.template.ISDEFAULT = chkTemplateDefault.Checked;
                Consult.template.ISLOADONSTART = isLoadDataOnStart;
                Consult.template.REFRESHINTERVAL = iRefreshInterval;
                Consult.template.ROWBYPAGE = iRowByPage;
                if (SessionTools.IsSessionGuest)
                {
                    //GUEST: on affecte en "dur" les droits 
                    Consult.template.RIGHTPUBLIC = RightsTypeEnum.NONE.ToString();
                    Consult.template.RIGHTENTITY = RightsTypeEnum.REMOVE.ToString();
                    Consult.template.RIGHTDEPARTMENT = RightsTypeEnum.REMOVE.ToString();
                    Consult.template.RIGHTDESK = RightsTypeEnum.NONE.ToString();
                }
                else
                {
                    //On affecte les nouveaux droits qu'après verif des droits actuels
                    if (SessionTools.IsSessionSysAdmin || SessionTools.IsSessionSysOper)
                        Consult.template.RIGHTPUBLIC = ddlRightPublic.SelectedValue;

                    Consult.template.RIGHTENTITY = ddlRightEntity.SelectedValue;
                    Consult.template.RIGHTDEPARTMENT = ddlRightDepartment.SelectedValue;
                    Consult.template.RIGHTDESK = ddlRightDesk.SelectedValue;
                }
                //Invisibles
                Consult.template.IDA = Convert.ToInt32(txtIdA.Text);
                Consult.template.DTINS = new DtFunc().StringyyyyMMddToDateTime(txtDtIns.Text);
                Consult.template.IDAINS = Convert.ToInt32(txtIdAIns.Text);
                Consult.template.EXTLLINK = txtExtlLink.Text;
                Consult.template.ROWATTRIBUT = txtRowAttribut.Text;
                Consult.template.ROWVERSION = txtRowVersion.Text;

                listBase.initialNameLstTemplate = txtInitialIdentifier.Text;
                listBase.newNameLstTemplate = txtIdentifier.Text;

                listBase.SaveTemplate();

                RepositoryWeb.WriteTemplateSession((PageBase)this.Page, IdLstConsult, IdLstTemplate, IdA, ParentGUID);

                #endregion

            }

            // Retour à la page appelante
            if (null != urlReferrer)
                Response.Redirect(ReturnUrlReferer.AbsoluteUri, true);
        }
        #endregion OnRecord
        #region OnCancel
        public void OnCancel(object sender, System.EventArgs e)
        {
            // NOTHING TO DO
            if (null != urlReferrer)
                Response.Redirect(urlReferrer.AbsoluteUri, true);
        }
        #endregion OnCancel

        #region OnRequestModeChanged
        public void OnRequestModeChanged(Object sender, EventArgs e)
        {
            //listBase.isReloadPage = true;
            //LoadDataControls();
        }
        #endregion OnRequestModeChanged
        #region OnActorChange
        public void OnActorChanged(object sender, EventArgs e)
        {
            DataSet ds = (DataSet)Session[SESSION_LSTOPEN_DATASET + listBase.GUID];

            string strDDLIdA = ddlOwnerOpen.SelectedValue;
            DropDownList ddl = ddlModelOpen;
            if (IsRemoveMode)
            {
                strDDLIdA = ddlOwnerRemove.SelectedValue;
                ddl = ddlModelRemove;
            }
            if (IsAllValue(strDDLIdA))
                LoadAllUserTemplate(ddl, RightsTypeEnum.VIEW);
            else
                BindDataDDL(ddl, ds.Tables[EnumTableDDL.LSTTEMPLATE.ToString()], strDDLIdA, false);

            ddl.Attributes.Add("class", "form-control input-xs");
        }
        #endregion OnActorChange
        #region SetKeyAndLoadTemplate
        protected void SetKeyAndLoadTemplate()
        {
            listBase.SetKeyAndLoadTemplate();
        }
        #endregion SetKeyAndLoadTemplate

        #region BindDataDDL
        private DropDownList BindDataDDL(DropDownList pDDL, DataTable pDataTable, string pValueFilter, bool pIsWithItemAll)
        {
            if (StrFunc.IsFilled(pValueFilter))
                pDataTable.DefaultView.RowFilter = IsAllValue(pValueFilter) ? string.Empty : "FKEY=" + pValueFilter;

            pDDL.DataSource = pDataTable;
            pDDL.DataTextField = "DISPLAYCOL";
            pDDL.DataValueField = "DATACOL";
            pDDL.DataBind();
            // Sur DDL_OWNER, [Tous] permet d'accéder au Template (dont pour rappel le owner est SYSTEM).
            if (pIsWithItemAll && ((pDDL.Items.Count > 1) || (pDDL.ID == DDL_OWNEROPEN)))
                ControlsTools.DDLLoad_AddListItemAllAll(pDDL);

            //pDDL.Visible = (pDDL.Items.Count > 0);
            pDDL.Enabled = (pDDL.Items.Count > 1);

            return pDDL;
        }
        #endregion BindDataDDL
        #region IsAllValue
        private bool IsAllValue(string pData)
        {
            return (pData == Cst.DDLVALUE_ALL) || (pData == Cst.DDLVALUE_ALL_Old);
        }
        #endregion IsAllValue

        #region CreateControls
        protected void CreateControls()
        {
            lblModelTitle.InnerText = Ressource.GetString("ViewerModel_Title", true);
            updSubTitle.Attributes.Add("class", "col-sm-8");

            // Initialisation via InputAttributes pour spécifier que l'élément rbOpen est prés-sélectionné quand la page est loadée
            rbOpen.InputAttributes["checked"] = (false == IsPostBack)?"true":"false";
            btnRecord.Text = Ressource.GetString(btnRecord.ID);

            string msgRequiredField = Ressource.GetString("ISMANDATORY", true);
            rqvIdentifier.ErrorMessage = msgRequiredField;
            rqvDisplayName.ErrorMessage = msgRequiredField;

            btnCancel.InnerText = Ressource.GetString(btnCancel.ID);

            ddlOwnerOpen.AutoPostBack = true;
            ddlOwnerRemove.AutoPostBack = true;

            rqvModelOpen.ErrorMessage = msgRequiredField;
            rqvModelRemove.ErrorMessage = msgRequiredField;

            //Permissions
            lblRightModel.InnerText = Ressource.GetString("Permissions", true);

            // Open Model
            rbOpen.Text = Ressource.GetString(rbOpen.ID, true);
            lblOwnerOpen.Text = Ressource.GetString("lblOwner", true);
            lblModelOpen.Text = Ressource.GetString("lblQuery", true);

            // Remove Model
            rbRemove.Text = Ressource.GetString(rbRemove.ID, true);
            lblOwnerRemove.Text = Ressource.GetString("lblOwnerDel", true);
            lblModelRemove.Text = Ressource.GetString("lblQueryDel", true);
            // SaveModel
            rbSave.Text = Ressource.GetString("btnModify", true);
            lblIdentifier.Text = Ressource.GetString("lblQueryNew", true);
            lblDisplayName.Text = Ressource.GetString(lblDisplayName.ID, true);
            lblDescription.Text = Ressource.GetString(lblDescription.ID, true);
            lblRefreshInterval.Text = Ressource.GetString(lblRefreshInterval.ID, true);
            chkTemplateDefault.Text = Ressource.GetString(chkTemplateDefault.ID, true);
            chkIsLoadOnStart.Text = Ressource.GetString(chkIsLoadOnStart.ID, true);

            lblExtlLink.Text = Ressource.GetString("ExtlLink_", true);

            lblRowByPage.Text = Ressource.GetString(lblRowByPage.ID, true);
            lblRightPublic.Text = Ressource.GetString(lblRightPublic.ID, true);
            lblRightEntity.Text = Ressource.GetString(lblRightEntity.ID, true);
            lblRightDepartment.Text = Ressource.GetString(lblRightDepartment.ID, true);
            lblRightDesk.Text = Ressource.GetString(lblRightDesk.ID, true);

            ddlRightPublic.Items.Clear();
            ddlRightEntity.Items.Clear();
            ddlRightDepartment.Items.Clear();
            ddlRightDesk.Items.Clear();
            ControlsTools.DDLLoad_RightType(ddlRightPublic);
            ControlsTools.DDLLoad_RightType(ddlRightEntity);
            ControlsTools.DDLLoad_RightType(ddlRightDepartment);
            ControlsTools.DDLLoad_RightType(ddlRightDesk);
        }
        #endregion CreateControls

        #region DisplayControlsByMode
        protected void DisplayControlsByMode()
        {
            lblInsertDate.Visible = IsSaveMode;
            lblUpdateDate.Visible = IsSaveMode;

            divSave.Visible = IsSaveMode;
            divOpen.Visible = false;
            divRemove.Visible = false;

            if (IsOpenMode)
            {
                lblSubTitle.Text = "Ouvrir un modèle existant";
                divOpen.Visible = true;
                rqvModelOpen.Enabled = true;

                btnRecord.Text = Ressource.GetString("Select");

                ddlOwnerOpen.Enabled = (1 < ddlOwnerOpen.Items.Count);
                ddlModelOpen.Enabled = (1 < ddlModelOpen.Items.Count);

            }
            else if (IsRemoveMode)
            {
                lblSubTitle.Text = "Supprimer un modèle";
                divRemove.Visible = true;
                rqvModelRemove.Enabled = true;
                btnRecord.Text = Ressource.GetString("btnRemove");
                //btnRecord.Attributes.Add("data-title", "title");
                //btnRecord.Attributes.Add("data-content", Ressource.GetString("Msg_DeleteTemplate"));
                //btnRecord.Attributes.Add("data-btn-ok-label", Ressource.GetString("btnYes"));
                //btnRecord.Attributes.Add("data-btn-cancel-label", Ressource.GetString("btnNo"));


                ddlOwnerRemove.Enabled = (1 < ddlOwnerRemove.Items.Count);
                ddlModelRemove.Enabled = (1 < ddlModelRemove.Items.Count);

            }
            else if (IsSaveMode)
            {
                lblSubTitle.Text = "Enregistrer les caractéristiques courantes (Affichage, Filtre, Tri, etc.) dans le modèle";
                btnRecord.Text = Ressource.GetString(btnRecord.ID);
                rqvIdentifier.Enabled = true;
                rqvDisplayName.Enabled = true;

                //btnRecord.Attributes.Add("data-title", "title");
                //btnRecord.Attributes.Add("data-content", Ressource.GetString("Msg_RecordTemplate"));
                //btnRecord.Attributes.Add("data-btn-ok-label", Ressource.GetString("btnYes"));
                //btnRecord.Attributes.Add("data-btn-cancel-label", Ressource.GetString("btnNo"));
            }
        }
        #endregion DisplayControlsByMode
        #region ReloadTemplate
        protected void ReloadTemplate()
        {
            if (false == IsSaveMode)
            {
                DataSet ds = (DataSet)Session[SESSION_LSTOPEN_DATASET + listBase.GUID];
                if ((false == IsPostBack) || listBase.isReloadPage)
                {
                    // Open Model
                    BindDataDDL(ddlOwnerOpen, ds.Tables[EnumTableDDL.ACTOR.ToString()], string.Empty, true);
                    if (StrFunc.IsFilled(SessionTools.Collaborator_IDA.ToString()) && (ddlOwnerOpen.Items.Count > 0))
                        ddlOwnerOpen.SelectedValue = SessionTools.Collaborator_IDA.ToString();
                    BindDataDDL(ddlModelOpen, ds.Tables[EnumTableDDL.LSTTEMPLATE.ToString()], SessionTools.Collaborator_IDA.ToString(), false);

                    // Remove Model
                    BindDataDDL(ddlOwnerRemove, ds.Tables[EnumTableDDL.ACTORREMOVE.ToString()], string.Empty, true);
                    if (StrFunc.IsFilled(SessionTools.Collaborator_IDA.ToString()) && (ddlOwnerRemove.Items.Count > 0))
                        ddlOwnerRemove.SelectedValue = SessionTools.Collaborator_IDA.ToString();
                    BindDataDDL(ddlModelRemove, ds.Tables[EnumTableDDL.LSTTEMPLATEREMOVE.ToString()], SessionTools.Collaborator_IDA.ToString(), false);
                }

                if (isReloadDDLTemplateOpen)
                {
                    string strDDLIdA = ddlOwnerOpen.SelectedValue;
                    if (IsAllValue(strDDLIdA))
                        LoadAllUserTemplate(ddlModelOpen, RightsTypeEnum.VIEW);
                    else
                        BindDataDDL(ddlModelOpen, ds.Tables[EnumTableDDL.LSTTEMPLATE.ToString()], strDDLIdA, false);
                }

                if (isReloadDDLTemplateRemove)
                {
                    string strDDLIdA = ddlOwnerRemove.SelectedValue;
                    if (IsAllValue(strDDLIdA))
                        LoadAllUserTemplate(ddlModelRemove, RightsTypeEnum.REMOVE);
                    else
                        BindDataDDL(ddlModelRemove, ds.Tables[EnumTableDDL.LSTTEMPLATEREMOVE.ToString()], strDDLIdA, false);
                }
            }
        }
        #endregion ReloadTemplate

        #region LoadDataControls
        public void LoadDataControls()
        {
            if (null == Session[SESSION_LSTOPEN_DATASET + listBase.GUID])
                Session[SESSION_LSTOPEN_DATASET + listBase.GUID] = LoaActorsAndTemplates();

            if (IsSaveMode)
            {
                #region Save
                Consult.LoadTemplate(CS, IdLstTemplate, IdA);
                if (null != Consult.template.IDLSTTEMPLATE)
                {
                    txtInitialIdentifier.Text = Consult.template.IDLSTTEMPLATE_WithoutPrefix;
                    txtIdLstConsult.Text = Consult.template.IDLSTCONSULT;
                    txtIdentifier.Text = Consult.template.IDLSTTEMPLATE_WithoutPrefix;
                    txtDisplayName.Text = StrFunc.IsFilled(Consult.template.DISPLAYNAME) ? Consult.template.DISPLAYNAME : Consult.template.IDLSTTEMPLATE_WithoutPrefix;
                    txtDescription.Text = Consult.template.DESCRIPTION;
                    chkTemplateDefault.Checked = Consult.template.ISDEFAULT;
                    chkIsLoadOnStart.Checked = Consult.template.ISLOADONSTART;

                    txtRefreshInterval.Text = Consult.template.IsRefreshIntervalSpecified ? Consult.template.REFRESHINTERVAL.ToString() : string.Empty;
                    txtRowByPage.Text = Consult.template.ROWBYPAGE.ToString();

                    if (false == SessionTools.IsSessionGuest)
                    {
                        if (StrFunc.IsFilled(Consult.template.RIGHTPUBLIC) && (0 < ddlRightPublic.Items.Count))
                            ddlRightPublic.SelectedValue = Consult.template.RIGHTPUBLIC;
                        if (StrFunc.IsFilled(Consult.template.RIGHTENTITY) && (0 < ddlRightEntity.Items.Count))
                            ddlRightEntity.SelectedValue = Consult.template.RIGHTENTITY;
                        if (StrFunc.IsFilled(Consult.template.RIGHTDEPARTMENT) && (0 < ddlRightDepartment.Items.Count))
                            ddlRightDepartment.SelectedValue = Consult.template.RIGHTDEPARTMENT;
                        if (StrFunc.IsFilled(Consult.template.RIGHTDESK) && (0 < ddlRightDesk.Items.Count))
                            ddlRightDesk.SelectedValue = Consult.template.RIGHTPUBLIC;
                    }

                    txtIdA.Text = Consult.template.IDA.ToString();
                    txtDtUpd.Text =  DtFunc.DateTimeToStringyyyyMMdd(Consult.template.DTUPD);
                    txtIdAUpd.Text = Consult.template.IDAUPD.ToString();
                    txtDtIns.Text =  DtFunc.DateTimeToStringyyyyMMdd(Consult.template.DTINS);
                    txtIdAIns.Text = Consult.template.IDAINS.ToString();
                    txtExtlLink.Text = Consult.template.EXTLLINK;
                    txtRowAttribut.Text = Consult.template.ROWATTRIBUT;
                    txtRowVersion.Text = Consult.template.ROWVERSION; 

                    bool isEnabled = true;
                    if (SessionTools.User.idA != Consult.template.IDA)
                    {
                        // Le template n'appartient pas au user connecté --> Gestion des droits
                        isEnabled = Consult.template.HasUserRight(CS, SessionTools.User, RightsTypeEnum.SAVE);
                        txtDisplayName.Text = Consult.template.IDLSTTEMPLATE;
                    }

                    txtDescription.Enabled = isEnabled;
                    txtRefreshInterval.Enabled = isEnabled;
                    txtRowByPage.Enabled = isEnabled;
                    txtDisplayName.Enabled = isEnabled;

                    chkTemplateDefault.Enabled = isEnabled;
                    chkIsLoadOnStart.Enabled = isEnabled;

                    if (false == SessionTools.IsSessionGuest)
                    {
                        ddlRightPublic.Enabled = isEnabled;
                        ddlRightEntity.Enabled = isEnabled;
                        ddlRightDepartment.Enabled = isEnabled;
                        ddlRightDesk.Enabled = isEnabled;
                    }

                    lblInsertDate.Visible = false;
                    lblUpdateDate.Visible = false;

                    if (DateTime.MinValue != Consult.template.DTINS)
                    {
                        lblInsertDate.Visible = true;
                        lblInsertDate.Text = Ressource.GetString_CreatedBy(Consult.template.DTINS, Consult.template.IDAINSDisplayName);
                    }
                    if (DateTime.MinValue != Consult.template.DTUPD)
                    {
                        lblUpdateDate.Visible = true;
                        lblUpdateDate.Text = Ressource.GetString_LastModifyBy(Consult.template.DTUPD, Consult.template.IDAUPDDisplayName);
                    }

                }
                #endregion Save
            }
            else
            {
                txtIdentifier.Text = RepositoryWeb.GetNewQueryName();
                txtIdLstConsult.Text = Consult.template.IDLSTCONSULT;
                txtIdA.Text = SessionTools.Collaborator_IDA.ToString();
                txtIdAIns.Text = SessionTools.Collaborator_IDA.ToString();
            }
        }
        #endregion LoadDataControls

        #region LoaActorsAndTemplates
        /// <summary>
        /// Chargement des Acteurs (disposant de Templates) et des Templates.
        /// <para>NB: Pour un user non SYSADMIN, seuls sont conservés les Templates sur lesquels l'utilisateur dispose des droits resquis.</para>
        /// <para>NB: Pour un user GUEST, seuls sont chargés ses Templates et ceux de son parent "direct".</para>
        /// </summary>
        /// <returns></returns>
        private DataSet LoaActorsAndTemplates()
        {
            DataSet ds = new DataSet();
            try
            {
                string SQLSelect = string.Empty;
                //Actors
                SQLSelect += GetSQLSelect_Actors(false) + ";" + Cst.CrLf;
                //Templates
                SQLSelect += GetSQLSelect_Templates() + ";" + Cst.CrLf;
                //Actors from Template allowed to del
                SQLSelect += GetSQLSelect_Actors(SessionTools.IsSessionGuest) + ";" + Cst.CrLf;
                //Templates allowed to Del
                SQLSelect += GetSQLSelect_Templates() + Cst.CrLf;

                ds = DataHelper.ExecuteDataset(CS, CommandType.Text, SQLSelect);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    string tableName = Enum.ToObject(typeof(EnumTableDDL), i).ToString();
                    ds.Tables[i].TableName = tableName;
                    ds.Tables[i].DefaultView.Sort = "DISPLAYCOL";

                    if (false == SessionTools.IsSessionSysAdmin)
                    {
                        // Session no SYSADMIN: Apply rights filter to datas for template list
                        // Pour les 2 datatables, on vérifie si l'utilisateur dispose des droits requis
                        //   - VIEW pour le datable destiné au load de la DDL des Templates disponibles pour OPEN 
                        //   - REMOVE pour le datable destiné au load de la DDL des Templates disponibles pour DELETE 
                        if (tableName.StartsWith(EnumTableDDL.LSTTEMPLATE.ToString()))
                        {
                            RightsTypeEnum neededRight = RightsTypeEnum.VIEW;
                            if (tableName == EnumTableDDL.LSTTEMPLATEREMOVE.ToString())
                                neededRight = RightsTypeEnum.REMOVE;

                            DataTable dt = ds.Tables[i];
                            // Pour un GUEST, seuls ses propres Templates peuvent donner lieu à suppression.
                            bool isGuest_Remove = (SessionTools.IsSessionGuest && (neededRight == RightsTypeEnum.REMOVE));
                            for (int numRow = dt.Rows.Count - 1; numRow > -1; numRow--)
                            {
                                int ida = Convert.ToInt32(dt.Rows[numRow]["IDA"]);
                                string idLstTemplate = Convert.ToString(dt.Rows[numRow]["IDLSTTEMPLATE"]);
                                if ((isGuest_Remove && (ida != SessionTools.Collaborator_IDA))
                                    || (!RepositoryWeb.HasUserRightForTemplate(CS, ida, IdLstConsult, idLstTemplate, neededRight)))
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
        #endregion LoaActorsAndTemplates

        #region GetSQLSelect_Actors
        /// <summary>
        /// Retourne l'ordre SELECT pour charger les Acteurs
        /// </summary>
        /// <returns></returns>
        private string GetSQLSelect_Actors(bool pIsForCurrentUserOnly)
        {
            // Acteur utilisateur connecté
            string sqlSelect = String.Format(@"select a.IDA as DATACOL, {0} as DISPLAYCOL
            from dbo.ACTOR a
            where (a.IDA = {1})", 
            GetSQLDisplayColActor(true), 
            SessionTools.Collaborator_IDA.ToString());

            if (false == pIsForCurrentUserOnly)
            {
                // Acteur(s) disposant d'au moins 1 Template (non temporaire) sur la consultation concernée
                sqlSelect += String.Format(@"union
                select distinct a.IDA as DATACOL, {0} as DISPLAYCOL
                from dbo.ACTOR a
                inner join dbo.LSTTEMPLATE lt on (lt.IDA = a.IDA) and (lt.IDLSTCONSULT = {1}) and {2}",
                GetSQLDisplayColActor(false), 
                DataHelper.SQLString(IdLstConsult),                                                                                    
                LstTemplateData.GetSQLWhereNotTemporary("lt"));

                // GUEST: Si celui-ci possède un DEPARTMENT dans ses parents on reteinet celui-ci, sinon on retient son parent direct.
                if (SessionTools.Collaborator.Department.Ida > 0)
                    sqlSelect += @" and (lt.IDA = " + SessionTools.Collaborator.Department.Ida.ToString() + ")" + Cst.CrLf;
                else
                    sqlSelect += @"inner join dbo.ACTORROLE ar on (ar.IDA_ACTOR = a.IDA) and (ar.IDROLEACTOR ='" + Actor.RoleActor.GUEST.ToString() + "')" + Cst.CrLf;

                sqlSelect += @"where (a.IDA not in (1," + SessionTools.Collaborator_IDA.ToString() + "))" + Cst.CrLf;
            }
            return sqlSelect;
        }
        #endregion GetSQLSelect_Actors

        #region GetSQLSelect_Templates
        /// <summary>
        /// Retourne l'ordre SELECT pour charger les Templates
        /// </summary>
        /// <returns></returns>
        private string GetSQLSelect_Templates()
        {
            string sqlSelect = String.Format(@"select distinct {0} as DATACOL, {1} as DISPLAYCOL,
            lt.IDA as FKEY, lt.IDLSTTEMPLATE, lt.IDA, lt.IDA as OPTGROUP, {2} as OPTGROUPTEXT,
            case when a.IDA=1 then ' ' || a.DISPLAYNAME else a.DISPLAYNAME end as OPTGROUPSORT
            from dbo.LSTTEMPLATE lt
            inner join dbo.ACTOR a on (a.IDA = lt.IDA)",
            GetSQLDataColTemplate(), GetSQLDisplayColTemplate(), GetSQLDisplayColActor(false)) + Cst.CrLf;
            string addWhere = string.Empty;

            if (SessionTools.IsSessionGuest)
            {
                // GUEST: Si celui-ci possède un DEPARTMENT dans ses parents on retient les templates de celui-ci, 
                //        sinon on retient ceux de son parent direct.
                if (SessionTools.Collaborator.Department.Ida > 0)
                {
                    sqlSelect += @" and (a.IDA in (" + SessionTools.Collaborator_IDA.ToString() + "," + SessionTools.Collaborator.Department.Ida.ToString() + ")" + Cst.CrLf;
                }
                else
                {
                    sqlSelect += @" left outer join dbo.ACTORROLE ar on (ar.IDA_ACTOR = a.IDA) and (ar.IDROLEACTOR='" + Actor.RoleActor.GUEST.ToString() + "')" + Cst.CrLf;
                    addWhere = " and ((a.IDA=" + SessionTools.Collaborator_IDA.ToString() + ") or (ar.IDA=" + SessionTools.Collaborator_IDA.ToString() + "))" + Cst.CrLf;
                }
            }
            sqlSelect += String.Format(@"where (lt.IDLSTCONSULT = {0}) and {1} {2}",
            DataHelper.SQLString(IdLstConsult), LstTemplateData.GetSQLWhereNotTemporary("lt"), addWhere) + Cst.CrLf;

            return sqlSelect;
        }
        #endregion GetSQLSelect_Templates

        #region GetSQLDataColTemplate
        /// <summary>
        /// Retourne l'expression SQL de la colonne qui alimente les value des DDLs template
        /// </summary>
        /// <returns></returns>
        private string GetSQLDataColTemplate()
        {
            return DataHelper.SQLConcat(CS, "lt.IDLSTTEMPLATE", "'" + SEPARATOR_IDA_TEMPLATE + "'", "a.DISPLAYNAME", "'" + SEPARATOR_IDA_TEMPLATE + "'", DataHelper.SQLNumberToChar(CS, "lt.IDA"));
        }
        #endregion GetSQLDataColTemplate

        #region GetSQLDisplayColTemplate
        /// <summary>
        /// Retourne l'expression SQL de la colonne qui alimente le text des DDLs templates
        /// </summary>
        /// <returns></returns>
        private static string GetSQLDisplayColTemplate()
        {
            return "case when lt.IDLSTTEMPLATE != lt.DISPLAYNAME then lt.IDLSTTEMPLATE || ' - ' ||  lt.DISPLAYNAME else lt.IDLSTTEMPLATE end";
        }
        #endregion GetSQLDisplayColTemplate

        #region GetSQLDisplayColActor
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
                    return DataHelper.SQLConcat(CS, "a.DISPLAYNAME", flagCurrentUser);
                else
                    return DataHelper.SQLConcat(CS, "a.DISPLAYNAME", "' (Id:'", DataHelper.SQLNumberToChar(CS, "a.IDA"), "')'", flagCurrentUser);
            }
            else
            {
                if (SessionTools.IsSessionGuest)
                    return "a.DISPLAYNAME";
                else
                    return DataHelper.SQLConcat(CS, "a.DISPLAYNAME", "' (Id:'", DataHelper.SQLNumberToChar(CS, "a.IDA"), "')'");
            }
        }
        #endregion GetSQLDisplayColActor

        #region LoadAllUserTemplate
        /// <summary>
        /// Chargement d'une DDL avec les templates regroupés par acteur 
        /// </summary>
        /// <param name="DDLFound">DDL</param>
        /// <<param name="pRightsTypeEnum">Type de droit nécessaire poour afficher un template</param>
        private void LoadAllUserTemplate(DropDownList pDDL, RightsTypeEnum pRightsTypeEnum)
        {
            string sqlSelect = GetSQLSelect_Templates();
            ControlsTools.DDLLoad(pDDL, "DISPLAYCOL", "DATACOL", true, 0, null, CS, sqlSelect, false, true, null);

            pDDL.Visible = (pDDL.Items.Count > 0);
            if (pDDL.Items.Count > 0)
            {
                if (!SessionTools.IsSessionSysAdmin)
                {
                    //PL 20150601 GUEST New feature
                    //GUEST: Pour un GUEST, seuls ses propres Templates peuvent donner lieu à suppression.
                    bool isGuest_Remove = (SessionTools.IsSessionGuest && (pRightsTypeEnum == RightsTypeEnum.REMOVE));
                    for (int i = ArrFunc.Count(pDDL.Items) - 1; i > -1; i--)
                    {
                        string[] value = pDDL.Items[i].Value.Split(new char[] { '|' });

                        int ida = Convert.ToInt32(value[2]);
                        string idLstTemplate = Convert.ToString(value[0]);
                        if ((isGuest_Remove && (ida != SessionTools.Collaborator_IDA))
                            || (!RepositoryWeb.HasUserRightForTemplate(CS, ida, IdLstConsult, idLstTemplate, pRightsTypeEnum)))
                        {
                            pDDL.Items.Remove(pDDL.Items[i]);
                        }
                    }
                }
            }
        }
        #endregion LoadAllUserTemplate

        #endregion Methods
    }
}
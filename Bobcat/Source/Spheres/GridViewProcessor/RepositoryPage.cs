#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.SpheresIO;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
using SpheresMenu = EFS.Common.Web.Menu;
#endregion using directives

namespace EFS.GridViewProcessor
{
    #region MenuItem
    public class MenuItem
    {
        #region Members
        public string id;
        public string idMenu;
        public string idMenuParent;
        public string displayName;
        public bool isEnabled = true;
        public bool isHidden = false;
        public bool isAction = false;
        public string toolTip;
        public string url;
        public string imageUrl;

        public List<MenuItem> subItems;
        #endregion Members
        #region accessors
        public bool HasSubItemsActions
        {
            get { return HasSubItems && subItems.Exists(item => item.isAction); }
        }
        public bool HasSubItems
        {
            get { return (null != subItems) && (0 < subItems.Count); }
        }
        public string Label
        {
            get { return Ressource.GetMenu_Shortname(idMenu, displayName);}
        }
        public string SubTitle
        {
            get { return Ressource.GetMenu_Shortname2(idMenuParent, idMenuParent); }
        }
        #endregion Accessors
        #region constructor
        public MenuItem(SpheresMenu.Menu pMenu, bool pIsEnabled)
        {
            id = pMenu.Id;
            idMenu = pMenu.IdMenu;
            idMenuParent = pMenu.IdMenu_Parent;
            displayName = pMenu.Displayname;
            isAction = pMenu.IsAction;
            url = pMenu.Url;
            isEnabled = pIsEnabled;
        }
        #endregion constructor
    }
    #endregion

    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class RepositoryPageBase : ContentPageBase
    {
        #region HRTitle
        private enum HRTitle
        {
            ValidationRules,
            AssetEnv
        }
        #endregion HRTitle

        #region MenuConsultEnum
        private enum MenuConsultEnum
        {
            First,
            Previous,
            Next,
            Last
        }
        #endregion

        #region Members
        public string objectName;
        public string dataKeyField;
        public string valueDataKeyField;
        public string dataForeignKeyField;
        public string valueForeignKeyField;

        public bool isMakingChecking_ActionChecking;
        /// <summary>
        /// Indicateur à true si formulaire en mode nouvelle saisie
        /// <para>Request.QueryString["N"]</para>
        /// </summary>
        public bool isNewRecord;
        /// <summary>
        /// Indicateur à true si formulaire en mode duplication
        /// Request.QueryString["DPC"]
        /// </summary>
        public bool isDuplicateRecord;
        /// <summary>
        /// Mode fonctionnement du formulaire de saisie
        /// </summary>
        public Cst.ConsultationMode consultationMode;
        /// <summary>
        /// Définition XML du  référentiel
        /// </summary>
        public Referential referential;
        /// <summary>
        /// Représente un indicateur qui signale si le formulaire est ouvert depuis le datagrid
        /// </summary>
        public bool isOpenFromGrid;

        protected string listType;
        protected string idMenu;
        private string titleMenu;
        protected Nullable<bool> _isExistMenuChild; //true s'il existe des sous menus
        protected bool isMenuChildLoaded;

        protected MenuItem menuDetail;

        private Dictionary<string, JQuery.AutoComplete.InitInfo> _initInfos;

        #endregion Members

        #region Accessor
        /// <summary>
        /// Obtiens true s'il existe des sous menus
        /// </summary>
        protected bool isExistMenuChild
        {
            get
            {
                if (false == _isExistMenuChild.HasValue)
                {
                    SpheresMenu.Menus menus = SessionTools.Menus;
                    _isExistMenuChild = (null != menus);
                    if (referential.consultationMode == Cst.ConsultationMode.ReadOnlyWithoutChildren)
                        _isExistMenuChild = false;
                    else
                        _isExistMenuChild = (bool)_isExistMenuChild && menus.IsParent(idMenu);
                }
                return _isExistMenuChild.Value;
            }
        }

        public bool isDisplayColumn
        {
            get { return ((SessionTools.IsSessionSysAdmin && BoolFunc.IsTrue(Request.Form[Cst.CHK + AdditionalCheckBoxEnum.ShowColumnName]))); }
        }
        public bool IsShowAllData
        {
            //PL 20151013 Checkbox checked by default (so TRUE on not IsPostBack)
            //get { return ((BoolFunc.IsTrue(Request.Form[Cst.CHK + AdditionalCheckBoxEnum.ShowAllData]))); }
            get { return (!this.IsPostBack) || BoolFunc.IsTrue(Request.Form[Cst.CHK + AdditionalCheckBoxEnum.ShowAllData]); }
        }

        /// <summary>
        ///  Obtient la clef d'accès à la variable session qui contient le dataset
        /// </summary>
        public string dataReferentielName
        {
            get
            {
                string prefix = "FORM";
                //
                string ret = prefix + objectName;
                ret += SessionTools.Collaborator_IDENTIFIER.Replace("-", string.Empty);
                //
                if (StrFunc.IsFilled(dataKeyField) && StrFunc.IsFilled(valueDataKeyField))
                    ret += "_" + dataKeyField + "=" + valueDataKeyField;
                //
                if (StrFunc.IsFilled(dataForeignKeyField) && StrFunc.IsFilled(valueForeignKeyField))
                    ret += "_" + dataForeignKeyField + "=" + valueForeignKeyField;
                //
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit le dataSet associé au formulaire
        /// <para>Spheres® stocke le dataset ds une variable session entre 2 post</para>
        /// </summary>
        public DataSet dataReferentiel
        {
            get
            {
                DataSet ret = (HttpSessionStateTools.Get(SessionTools.SessionState, dataReferentielName)) as DataSet;
                return ret;
            }
            set
            {
                HttpSessionStateTools.Set(SessionTools.SessionState, dataReferentielName, value);
            }
        }

        /// <summary>
        /// Obtient le dataSet associé au grid lorsque le formulaire est ouvert depuis le grid
        /// <para>Retourne null si le grid n'est pas chargé ou que la session est indisponible</para>
        /// <remarks>Ce dataset est sauvegardé en session</remarks>
        /// </summary>
        /// <returns></returns>
        public DataSet datasetGrid
        {
            get
            {
                DataSet ret = null;
                if (isOpenFromGrid && SessionTools.IsSessionAvailable)
                {
                    string dsGridCacheName = Request.QueryString["DS"];
                    ret = (HttpSessionStateTools.Get(SessionTools.SessionState, dsGridCacheName)) as DataSet;
                }
                return ret;
            }
        }
        /// <summary>
        /// Obtient true si le grid associé au formulaire a été chargé au moins 1 fois
        /// <para>Depuis la 2.6 les grids ne sont pas systématiquement chargé à l'ouverture</para>
        /// </summary>
        public bool isDatasetGridFilled
        {
            get
            {
                bool ret = false;
                //
                DataSet dsGrid = datasetGrid;
                if (null != dsGrid)
                    ret = (dsGrid.Tables[0].Columns.Count > 0);//Il y a des colonnes des que le grid a été chargé 
                //
                return ret;
            }
        }

        /// <summary>
        /// Get the all the registered autocomplete descriptions used to build the client side controls of this page
        /// </summary>
        public Dictionary<string, JQuery.AutoComplete.InitInfo> initInfos
        {
            get { return _initInfos; }
            private set { _initInfos = value; }
        }

        /// <summary>
        /// Obtient le titre de la page au format Html
        /// </summary>
        protected string htmlName
        {
            get
            {
                string ret = null;
                if (StrFunc.IsFilled(Request.QueryString["TitleRes"]))
                    ret = Ressource.GetString(Request.QueryString["TitleRes"], true);
                else
                    ret = Ressource.GetString(listType, true);
                string resMenu = Ressource.GetMenu_Fullname(idMenu, StrFunc.IsEmpty(titleMenu) ? objectName : titleMenu);
                //return ret + ": " + HtmlTools.HTMLBold(resMenu);
                return resMenu;
            }
        }

        /// <summary>
        /// Obtient une ressource en fonction du mode
        /// </summary>
        protected string mode
        {
            get { return Ressource.GetString_SelectionCreationModification(consultationMode, isNewRecord); }
        }
        protected Pair<string, Cst.Capture.ModeEnum> actionTitle
        {
            get 
            {
                string title = "Consultation";
                Cst.Capture.ModeEnum cssClass = Cst.Capture.ModeEnum.Consult;
                switch (consultationMode)
                {
                    case Cst.ConsultationMode.Normal:
                        if (isNewRecord)
                        {
                            title = "Creation";
                            cssClass = Cst.Capture.ModeEnum.New;
                        }
                        else
                        {
                            title = "Modification";
                            cssClass = Cst.Capture.ModeEnum.Update;
                        }
                        break;
                    case Cst.ConsultationMode.Select:
                        title = "Selection";
                        break;
                }
                return new Pair<string, Cst.Capture.ModeEnum>(Ressource.GetString(title), cssClass);
            }
        }
        #endregion Accessor

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public RepositoryPageBase()
        {
            isOpenFromGrid = false;
            _initInfos = new Dictionary<string, JQuery.AutoComplete.InitInfo>();

            //plhMenuDetail = new PlaceHolder[2];
            //menuDetail = new skmMenu.MenuItemParent(1);
            isMenuChildLoaded = false;
        }
        #endregion constructor

        #region OnInit
        /// <summary>
        /// Initialisation du formulaire
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);

            #region Variables initialization
            //example: "http://localhost/OTC/Referential/Repository.aspx?T=Referential&O=BUSINESSCENTER&M=0&N=0&PK=IDBC&PKV=ARBA&FK=&FKV=&F=frmConsult&IDMenu=16540"
            AbortRessource = true;

            objectName = Request.QueryString["O"];
            //Folder pour recherche du fichier XML 
            listType = Request.QueryString["T"];		                    
            if (String.IsNullOrEmpty(listType))
                listType = Cst.ListType.Repository.ToString();
            dataKeyField = Request.QueryString["PK"];
            valueDataKeyField = Request.QueryString["PKV"];
            dataForeignKeyField = Request.QueryString["FK"];
            valueForeignKeyField = Request.QueryString["FKV"];
            isNewRecord = BoolFunc.IsTrue(Request.QueryString["N"]);        //N:New record
            isDuplicateRecord = BoolFunc.IsTrue(Request.QueryString["DPC"]);//DPC:DuPliCate

            //PL 20110627 (ex. QUOTE_H_EOD)
            if ((objectName == "QUOTE_H_EOD") && (dataKeyField == "KEYVALUE") && valueDataKeyField.StartsWith("0;"))
            {
                //WARNING!
                //Détournement du code source du mode "isDuplicateRecord", afin de considérer que l'on est en "Creation" et pas en "Modification".
                //Pour rappel, on est ici sur une vue qui expose virtuellement les cotations nécessaires à une date de compensation donnée.
                //valueDataKeyField contient: convert(varchar,isnull(q.IDQUOTE_H,0))+';'+'ETD'+';'+convert(varchar,a.IDASSET)
                //donc quand valueDataKeyField commence par "0;" cela indique une nouvelle cotation (IDQUOTE_H = 0).
                isDuplicateRecord = true;
            }

            //IDMenu => permet de recuperer les habilitations sur le referentiel, 
            //			permet aussi d'afficher le titre lorsque ce dernier est inexistant
            idMenu = Request.QueryString["IDMenu"];
            titleMenu = Request.QueryString["TitleMenu"];
            isOpenFromGrid = StrFunc.IsFilled(Request.QueryString["DS"]);  //DS Comme dataset (dataset source du datagrid)

            try
            {
                consultationMode = (Cst.ConsultationMode)Convert.ToInt32(Request.QueryString["M"]);
            }
            catch
            {
                consultationMode = Cst.ConsultationMode.Normal;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["OCR"] == "1")//OCR:OpenerCallReload
                {
                    JavaScript.OpenerCallReload(this);
                }
            }

            string condApp = Request.QueryString["CondApp"];
            string[] param = RepositoryTools.GetQueryStringParam(Page); // %%PARAM%%

            Cst.ListType listeTypeEnum = Cst.ListType.Repository;
            if (Enum.IsDefined(typeof(Cst.ListType), listType))
                listeTypeEnum = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), listType);

            Dictionary<string, StringDynamicData> dynamicArgs = RepositoryTools.CalcDynamicArgumentFromHttpParameter2(Ressource.DecodeDA(Request.QueryString["DA"]));

            List<string> ObjectNameForDeserialize = RepositoryTools.GetObjectNameForDeserialize(idMenu, objectName);
            RepositoryTools.DeserializeXML_ForModeRW(CS, idMenu, listeTypeEnum, ObjectNameForDeserialize, condApp, param, dynamicArgs, out referential);

            referential.SetPermission(idMenu);

            PageTitle = HtmlTools.HTMLBold_Remove(htmlName) + " - " + mode;
            
            RepositoryTools.InitializeReferentialForForm(this);

            if (isDuplicateRecord)
            {
                isDuplicateRecord = false;
                isNewRecord = true;
                referential.isNewRecord = true;
            }
            #endregion

            // Construction du Grid (Application du Grid system BS)
            CreateGridSystem();

            // Creation des controles
            CreateControls();
            
            IsMakingChecking();
            DisplayButtons();

            if (isExistMenuChild)
                CreateControlMenuChilds();

            #region SetFocus
            //Set focus on first control (Seulement en création, afin d'éviter les désagrément sur un DDL liés à la molette de la souris)
            //if (!IsPostBack && referential.isNewRecord)
            //{
            //    if (StrFunc.IsFilled(referential.firstVisibleAndEnabledControlID))
            //    {
            //        JavaScript.SetFocus(this);
            //        JavaScript.ScriptOnStartUp(this, "SetFocus(" + JavaScript.JSString(referential.firstVisibleAndEnabledControlID) + ");", "PageSetFocus");
            //    }
            //}
            #endregion
        }
        #endregion OnInit

        #region OnConsult
        /// <summary>
        /// Evénement de consultation des enregistrements (FIRST|PREVIOUS|NEXT|LAST) sur la base du dataset stocké dans le cache
        /// </summary>
        /// <param name="sender">Bouton appelant (btnFirstRecord|btnPreviousRecord|btnLastRecord|btnEndRecord)</param>
        /// <param name="e"></param>
        protected void OnConsult(object sender, System.EventArgs e)
        {
            DataSet ds = datasetGrid;
            //si dataset stocké dans le cache n'existe plus Spheres® affiche un message
            if (null == ds)
                JavaScript.DialogStartUpImmediate(this, Ressource.GetString("Msg_DataSourceNotFound"), false, ProcessStateTools.StatusWarningEnum);

            if (null != ds)
            {
                #region recherche de la ligne consultée dans le Dataset
                DataColumn columnDataKeyField = ds.Tables[0].Columns[referential.IndexColSQL_DataKeyField];
                string select = dataKeyField + "=";
                if (columnDataKeyField.DataType.Equals(typeof(String)))
                    select += "'" + valueDataKeyField + "'";
                else
                    select += valueDataKeyField;

                int index = -1;
                int newIndex = 0;
                // tblResult contient les lignes triées (utilisation de la propriété DefaultView qui contient les données triées)
                DataTable tblResult = ds.Tables[0].DefaultView.ToTable();
                DataRow[] row = tblResult.Select(select);
                if (ArrFunc.Count(row) == 1)
                    index = tblResult.Rows.IndexOf(row[0]);
                #endregion

                bool isFindNext = false;
                bool isFindPrevious = false;
                HtmlButton btnConsult = sender as HtmlButton;
                if (null != btnConsult)
                {
                    switch (btnConsult.ID)
                    {
                        case "btnFirstRecord":
                            newIndex = 0;
                            isFindNext = true;
                            break;
                        case "btnPreviousRecord":
                            newIndex = Math.Max(0, index - 1);
                            isFindPrevious = true;
                            break;
                        case "btnNextRecord":
                            newIndex = Math.Min(index + 1, tblResult.Rows.Count - 1);
                            isFindNext = true;
                            break;
                        case "btnLastRecord":
                            newIndex = tblResult.Rows.Count - 1;
                            isFindPrevious = true;
                            break;
                    }

                    if (newIndex != index)
                    {
                        string newKeyFieldValue = tblResult.Rows[newIndex][referential.IndexColSQL_DataKeyField].ToString();
                        bool isNewRowNavigable = StrFunc.IsFilled(newKeyFieldValue);
                        if (false == isNewRowNavigable)
                        {
                            //Recherche de la prochaine ligne valide
                            if (isFindNext)
                            {
                                while ((false == isNewRowNavigable) && (newIndex <= tblResult.Rows.Count - 1))
                                {
                                    newIndex = Math.Min(newIndex + 1, tblResult.Rows.Count - 1);
                                    newKeyFieldValue = tblResult.Rows[newIndex][referential.IndexColSQL_DataKeyField].ToString();
                                    isNewRowNavigable = StrFunc.IsFilled(newKeyFieldValue);
                                }
                            }
                            //Recherche de la précédente ligne valide
                            if (isFindPrevious)
                            {
                                while ((false == isNewRowNavigable) && (newIndex >= 0))
                                {
                                    newIndex = Math.Max(0, newIndex - 1);
                                    newKeyFieldValue = tblResult.Rows[newIndex][referential.IndexColSQL_DataKeyField].ToString();
                                    isNewRowNavigable = StrFunc.IsFilled(newKeyFieldValue);
                                }
                            }
                        }

                        if (isNewRowNavigable)
                        {
                            DataRow newRow = tblResult.Rows[newIndex];
                            newKeyFieldValue = newRow[referential.IndexColSQL_DataKeyField].ToString();

                            string url = Request.RawUrl;
                            url = url.Replace("&PKV=" + HttpUtility.UrlEncode(valueDataKeyField), "&PKV=" + HttpUtility.UrlEncode(newKeyFieldValue));
                            Response.Redirect(url, false);
                        }
                    }
                }
            }
        }
        #endregion OnConsult

        protected void OnDuplicate(object sender, EventArgs e)
        {
            // Reload de la page afin de "simuler" une création (&N=1)
            string url = this.Request.RawUrl;
            url = url.Replace("&N=0", "&N=1");          //N:New
            url = url.Replace("&OCR=1", string.Empty);  //OCR:OpenerCallReload
            url += "&DPC=1";                            //DPC:DuPliCate
            Server.Transfer(url);
        }

        #region OnRecord
        /// <summary>
        /// Sauvegarde du formulaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRecord(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                IDbTransaction dbTransaction = null;
                try
                {
                    if (referential.isNewRecord && referential.ExistsColumnIDENTITYWithSource)
                    {
                        dbTransaction = DataHelper.BeginTran(this.CS);
                    }
                    RepositoryTools.UpdateDataRowFromControls(this, dbTransaction);

                    Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
                    HtmlButton btnRecord = sender as HtmlButton;
                    if (null != btnRecord)
                    {
                        switch (btnRecord.ID)
                        {

                            case "btnApply":
                                ret = Save(dbTransaction, false);
                                break;
                            default:
                                ret = Save(dbTransaction, true);
                                break;
                        }

                    }
                    if ((dbTransaction != null))
                    {
                        if (ret == Cst.ErrLevel.SUCCESS)
                            DataHelper.CommitTran(dbTransaction);
                        else
                            DataHelper.RollbackTran(dbTransaction);
                    }

                    if (ret == Cst.ErrLevel.SUCCESS)
                    {
                        RequestTrackProcessEnum actionType = RequestTrackProcessEnum.New;
                        if (StrFunc.IsFilled(valueDataKeyField))
                        {
                            //Enregistrement d'un nouvel item à partir d'un enregistrement existant
                            if (BoolFunc.IsTrue(Request.QueryString["DPC"])) 
                                actionType = RequestTrackProcessEnum.New;
                            else
                                actionType = RequestTrackProcessEnum.Modify;
                        }

                        SetRequestTrackBuilderItemProcess(actionType, referential.dataRow);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        if (null != dbTransaction)
                        {
                            DataHelper.RollbackTran(dbTransaction);
                        }
                    }
                    catch { }
                    throw;
                }
            }
        }
        #endregion OnRecord
        #region OnRemove
        protected void OnRemove(object sender, EventArgs e)
        {
            // FI 20141021 [20350] Sauvegarde des valeurs précédentes
            DataRow savDataRow = referential.CoptyDataRow();

            //Delete principal
            referential.dataRow.Delete();

            //Delete EXTLID si existant
            if (referential.drExternal != null)
            {
                for (int i = 0; i < referential.drExternal.GetLength(0); i++)
                {
                    // Warning: Cas particulier des external de type Role ou Item, ces données sont supprimées par une IR de type cascade, 
                    //          il ne faut donc pas les supprimer via ADO sous peine de s'exposer à une erreur (cf TRIM 16520)
                    ReferentialColumn rrc = referential[i, "ExternalFieldID"];
                    if ((rrc != null) && (!rrc.IsRole) && (!rrc.IsItem))
                        referential.drExternal[i].Delete();
                }
            }

            Cst.ErrLevel errLevel = Save(null, true);

            // FI 20141021 [20350] call SetRequestTrackBuilderItemProcess 
            if (errLevel == Cst.ErrLevel.SUCCESS)
                SetRequestTrackBuilderItemProcess(RequestTrackProcessEnum.Remove, savDataRow);
        }
        #endregion OnRemove


        /// <summary>
        /// TO DO
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            //if (ArrFunc.IsFilled(m_alImageButtonOpenBanner))
            //{
            //    foreach (WCImageButtonOpenBanner img in m_alImageButtonOpenBanner)
            //        img.SetLinkControlDisplay();
            //}
            //bool isApplyOffset = StrFunc.IsFilled(Request.Params["__EVENTTARGET"]) && Request.Params["__EVENTTARGET"].StartsWith("EFS_ApplyOffset");
            //if (isApplyOffset)
            //    ApplyOffSet(Request.Params["__EVENTTARGET"], Request.Params["__EVENTARGUMENT"]);

            //// FI 20141021 [20350] Call SetRequestTrackBuilderItemLoad
            //SetRequestTrackBuilderItemLoad();

            base.OnPreRender(e);
        }

        /// <summary>
        /// TO DO
        /// Alimentation du log des actions utilisateur si consultation d'un élément
        /// </summary>
        private void SetRequestTrackBuilderItemLoad()
        {
            //if (referential.RequestTrackSpecified && SessionTools.IsRequestTrackConsultEnabled)
            //{
            //    if ((false == IsPostBack) && StrFunc.IsFilled(valueDataKeyField) && (BoolFunc.IsFalse(Request.QueryString["DPC"])))
            //    {
            //        RequestTrackItemBuilder builder = new RequestTrackItemBuilder();
            //        builder.action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemLoad, RequestTrackActionMode.manual);
            //        builder.referential = referential;
            //        builder.row = new DataRow[] { referential.dataRow };
            //        this.RequestTrackBuilder = builder;
            //    }
            //}
        }

        /// <summary>
        /// TO DO
        /// Alimentation du log des actions utilisateur si Création, Modification, Annulation d'un élément
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pRow">Enregistrement qui sera inscrit dans le log</param>
        private void SetRequestTrackBuilderItemProcess(RequestTrackProcessEnum pProcessType, DataRow pRow)
        {
            //if (referential.RequestTrackSpecified && SessionTools.IsRequestTrackProcessEnabled)
            //{
            //    RequestTrackItemBuilder builder = new RequestTrackItemBuilder();
            //    builder.action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual);
            //    builder.processType = pProcessType;
            //    builder.referential = referential;
            //    builder.row = new DataRow[] { pRow };
            //    this.RequestTrackBuilder = builder;
            //}
        }




        #region Methods


        /// <summary>
        /// Sauvegarde d'un formulaire "Referential".
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsSelfClose"></param>
        /// <returns></returns>
        private Cst.ErrLevel Save(IDbTransaction pDbTransaction, bool pIsSelfClose)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            int rowsAffected = 0;
            string message, error, keyField, keyFieldNewValue, keyFieldOldValue;
            message = error = keyField = keyFieldNewValue = keyFieldOldValue = string.Empty;

            // RD 20100310 / Automatic Compute identifier
            bool isTriggerLoCommandToExecute = false;

            if (SessionTools.IsSessionSysAdmin && (referential.TableName == Cst.OTCml_TBL.EFSOBJECTDET.ToString()))
            {
                #region Get Modified or Inserted object
                try
                {
                    if (referential.dataRow.RowState == DataRowState.Added ||
                        referential.dataRow.RowState == DataRowState.Modified ||
                        referential.dataRow.RowState == DataRowState.Detached)
                    {
                        Cst.CommandTypeEnum commandeType = (Cst.CommandTypeEnum)Enum.Parse(typeof(Cst.CommandTypeEnum), referential.dataRow["COMMANDTYPE"].ToString());

                        if (commandeType == Cst.CommandTypeEnum.SQLTrigger)
                        {
                            isTriggerLoCommandToExecute = true;
                        }
                    }
                }
                catch (Exception e) { System.Diagnostics.Debug.WriteLine("Error to get Modified or Inserted object: " + e.Message); }
                #endregion
            }

            ret = RepositoryTools.SaveReferential(this, pDbTransaction, idMenu, referential,
                out rowsAffected, out message, out error, out keyField, out keyFieldNewValue, out keyFieldOldValue);

            if (ret == Cst.ErrLevel.SQLDEFINED)
            {
                SetDialogImmediate("SaveRepository", error + Cst.HTMLBreakLine2 + Ressource.GetString(message), ProcessStateTools.StatusWarningEnum);
            }
            else if (rowsAffected <= 0)
            {
                SetDialogImmediate("SaveRepository", Ressource.GetString("Msg_NoModification"), ProcessStateTools.StatusErrorEnum);
            }
            else
            {
                // Spécifique au referentiel   : ACTOR (Create sub-repository from template)
                // Spécifique aux referentiels : SYNDICATIONFEED/SYNDICATIONITEM
                // Spécifique au referentiel   : INSTRUMENT (Add Default Template for Instrument)
                // Spécifique au referentiel   : EFSOBJECTDET (Load and Execute Trigger's SQL scripts)
                SaveSpecific(pDbTransaction, keyFieldOldValue, keyFieldNewValue);

                #region Spécifique au referentiel: EFSOBJECTDET (Load and Execute Trigger's SQL scripts)
                if (isTriggerLoCommandToExecute)
                {
                    string sqlLoCommand = referential.dataRow["LOCOMMAND"].ToString();
                    string commandName = (Convert.IsDBNull(referential.dataRow["COMMANDNAME"]) ? string.Empty : referential.dataRow["COMMANDNAME"].ToString());

                    try
                    {
                        DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlLoCommand);
                    }
                    catch (Exception e)
                    {
                        SetDialogImmediate("SaveRepository",
                            Ressource.GetString("Msg_TriggerScriptNotExecuted") + (StrFunc.IsFilled(commandName) ? " (" + commandName + ")" : string.Empty) + ": " + Cst.CrLf + e.Message, 
                            ProcessStateTools.StatusErrorEnum);
                        ret = Cst.ErrLevel.SQL_ERROR;
                    }
                }
                #endregion

                //S'il s'agit d'une "Création" et qu'on ne ferme pas la fenêtre (Bouton "Apply"), on reload la page afin d'afficher les menus childs (Detail).
                if ((false == pIsSelfClose) && referential.isNewRecord)
                {
                    if (StrFunc.IsFilled(keyField) && StrFunc.IsFilled(keyFieldNewValue))
                    {
                        int pos1, pos2;
                        string url = this.Request.RawUrl;
                        url = url.Replace("&N=1", "&N=0");
                        url = url.Replace("&DPC=1", string.Empty);

                        pos1 = url.IndexOf("&PK=");

                        if (pos1 > 0)
                        {
                            pos2 = url.IndexOf("&", pos1 + 4);
                            url = url.Remove(pos1, pos2 - pos1);
                            pos1 = url.IndexOf("&PKV=");
                            pos2 = url.IndexOf("&", pos1 + 5);
                            if (pos2 >= 0)
                                url = url.Remove(pos1, pos2 - pos1);
                            else
                                url = url.Substring(0, pos1);
                        }
                        url += "&PK=" + keyField;
                        url += "&PKV=" + keyFieldNewValue;
                        url += "&OCR=1"; //OCR:OpenerCallReload
                        Server.Transfer(url);
                    }
                    else
                    {
                        pIsSelfClose = true;
                    }
                }

                // Si selfclose ou si le référentiel a pour datakeyfield ROWVERSION ou si... : on impose la fermeture la fenêtre.
                // PL 20100322 Add DERIVATIVECONTRACT & ASSET_ETD for Trigger (A finaliser...)
                // Appel à Refresh à la place de Reload 
                if (pIsSelfClose
                    || referential.IsROWVERSIONDataKeyField
                    || referential.TableName == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()
                    || referential.TableName == Cst.OTCml_TBL.ASSET_ETD.ToString())
                {
                    this.ClientScript.RegisterStartupScript(this.Page.GetType(), "OpenerCallRefreshAndClose", "OpenerCallRefreshAndClose();",true);
                    //JavaScript.OpenerCallRefreshAndClose(this);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenerCallReload", "OpenerCallReload();", true);
                    //JavaScript.OpenerCallRefresh(this);
                }
            }
            return ret;
        }

        #region SaveSpecific
        private void SaveSpecific(IDbTransaction pDbTransaction, string pKeyFieldOldValue, string pKeyFieldNewValue)
        {
            if (referential.TableName == Cst.OTCml_TBL.ACTOR.ToString())
            {
                #region Spécifique au referentiel: ACTOR (Duplicate sub-reférentials)
                if (referential.isNewRecord)
                {
                    bool isDuplicateRecord = BoolFunc.IsTrue(Request.QueryString["DPC"]);//DPC:DuPliCate
                    if (isDuplicateRecord && IntFunc.IsPositiveInteger(pKeyFieldNewValue) && IntFunc.IsPositiveInteger(pKeyFieldOldValue))
                    {
                        int IdA_Template = IntFunc.IntValue2(pKeyFieldOldValue);
                        int IdA = IntFunc.IntValue2(pKeyFieldNewValue);
                        int nRows = 0;

                        //NB: A des fins d'audit, volontairement on met à jour cette colonne dans un 2ème temps (même si cela engendre de fait un insert dans ACTOR_P)
                        string sqlQuery = @"update dbo.ACTOR set IDA_TEMPLATE = " + IdA_Template.ToString() + @" where IDA = " + IdA.ToString();

                        if (pDbTransaction == null)
                            nRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
                        else
                            nRows = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery);

                        if (0 < nRows)
                        {
                            nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.RISKMARGIN, IdA, IdA_Template);

                            #region CLEARINGORGPARAM|IMREQMARKETPARAM
                            if (0 < nRows)
                            {
                                nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.CLEARINGORGPARAM, IdA, IdA_Template);
                                nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.IMREQMARKETPARAM, IdA, IdA_Template);
                            }
                            #endregion

                            #region CASHBALANCE
                            nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.CASHBALANCE, IdA, IdA_Template);
                            #endregion

                            #region ACTORROLE
                            // Si le formulaire affiche les rôles il n'y a pas de duplication des rôles. 
                            // Aujourd'hui le formulaire affiche les rôles uniquement si l'utilisateur est SYSADMIN
                            if (false == referential.RoleTableNameSpecified)
                                nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.ACTORROLE, IdA, IdA_Template);
                            #endregion

                            #region BOOK
                            string bookIdentifier = Request.QueryString["NBV"];//NBV:New Book Value
                            if (false == String.IsNullOrEmpty(bookIdentifier))
                            {
                                bookIdentifier = DataHelper.SQLString(bookIdentifier);
                            }

                            int bookCount = 0;
                            sqlQuery = @"select count(*) from dbo.BOOK where (IDA = " + IdA_Template.ToString() + ")";
                            object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, sqlQuery);
                            if (null != obj)
                            {
                                bookCount = Convert.ToInt32(obj);
                                if (bookCount > 0)
                                {
                                    nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.BOOK, IdA, IdA_Template);

                                    if (0 < nRows)
                                    {
                                        #region BOOKPOSEFCT
                                        nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.BOOKPOSEFCT, IdA, IdA_Template);
                                        #endregion

                                        #region BOOKG
                                        nRows = Save_ActorFromTemplate(pDbTransaction, Cst.OTCml_TBL.BOOKG, IdA, IdA_Template);
                                        #endregion

                                        #region BOOK update
                                        if (false == String.IsNullOrEmpty(bookIdentifier))
                                        {
                                            if (bookCount == 1)
                                            {
                                                //Utilisation de l'identifiant saisie pour le book (Warning, l'ordre des colonnes est important)
                                                sqlQuery = @"update dbo.BOOK set
                                                DESCRIPTION = replace(DESCRIPTION, IDENTIFIER, {0}),
                                                DISPLAYNAME = replace(DISPLAYNAME, IDENTIFIER, {0}),
                                                IDENTIFIER  = {0}" + Cst.CrLf;
                                                sqlQuery = String.Format(sqlQuery, bookIdentifier);
                                            }
                                            else
                                            {
                                                //Utilisation de l'identifiant saisie pour le book, pour se substituer au keyword représentant l'identifiant de l'acteur Template.... 
                                                sqlQuery = @"select IDENTIFIER from dbo.ACTOR where (IDA = " + IdA_Template.ToString() + ")";
                                                obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, sqlQuery.ToString());
                                                string acttpl_identifier = string.Empty;
                                                if (null != obj)
                                                    acttpl_identifier = DataHelper.SQLString(Convert.ToString(obj));

                                                sqlQuery = @"update dbo.BOOK set
                                                DESCRIPTION = replace(DESCRIPTION, {0}, {1}),
                                                DISPLAYNAME = replace(DISPLAYNAME, {0}, {1}),
                                                IDENTIFIER  = replace(IDENTIFIER, {0}, {1})" + Cst.CrLf;
                                                sqlQuery = String.Format(sqlQuery, acttpl_identifier, bookIdentifier);
                                            }
                                            sqlQuery += SQLCst.WHERE + "IDA=" + IdA.ToString();

                                            if (pDbTransaction == null)
                                                nRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery);
                                            else
                                                nRows = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery);
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }
            else if (referential.TableName.StartsWith("SYNDICATION"))
            {
                #region Spécifique aux referentiels: SYNDICATIONFEED/SYNDICATIONITEM
                string culture, path;
                for (int passCulture = 1; passCulture <= 3; passCulture++)
                {
                    culture = (passCulture == 1 ? "en" : (passCulture == 2 ? "fr" : "it"));
                    path = HttpContext.Current.Server.MapPath(@"Portal/News/" + culture + @"/News_Events_{ALL}.xml");

                    EFSSyndicationFeed.CreateSyndicationFeedFile(
                            EFSSyndicationFeed.GetSyndicationFeed(SessionTools.CS, EFSSyndicationFeed.SyndicationFeedTypeEnum.BUSINESSNEWS, culture)
                            , EFSSyndicationFeed.SyndicationFeedFormatEnum.ALL, path);
                }
                #endregion
            }
            else if (referential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString())
            {
                #region Spécifique au referentiel: INSTRUMENT (Add Default Template for Instrument)
                IDbConnection dbConnection = null;
                IDbTransaction dbTransaction = null;
                // #warning 20050729 PL (temporaire, à revoir... insert template post new instr)
                //	Création d'un template pour les éventuels nouveaux instruments, à partir
                //  du template de l'instrument portant sur le même proudct et ayant le nom du product
                try
                {
                    // Création d'un template, pour tous les éventuels nouveaux instruments qui n'en possèdent pas un, en trois passes et en mode Transactionnel.
                    //
                    // consiste à affecter:
                    //      - pour un instrument (Instrument1), 
                    //      - lié à un product (Product1), 
                    //      - un template (Template1), lié à un Instrument(Instrument2), parmi éventuellement plusieurs instruments du product (Product1) :
                    //
                    // passNumber = 1: 
                    // ---------------
                    // c'est le même fonctionnment que l'existant, en admettant qu'il pourrait y avoir qu'un seul template qui vérifie les deux conditions:
                    //    - Template1.IDENTIFIER = Product1.IDENTIFIER
                    //    - Template1.IDENTIFIER = Instrument2.IDENTIFIER
                    //
                    // passNumber = 2: 
                    // ---------------
                    // prendre le premier Template qui vérifie la condition suivante, de tous les instruments liés au product Product1 :
                    //    - Template1.IDENTIFIER = Product1.IDENTIFIER
                    //
                    // passNumber = 3: 
                    // ---------------
                    // prendre le premier Template de tous les templates de tous les instruments liés au product Product1
                    //    - Template1.IDENTIFIER = Product1.IDENTIFIER
                    //
                    dbConnection = DataHelper.OpenConnection(SessionTools.CS);

                    int nbPass = 3;
                    int nbRow = 0;
                    string sqlQuery;
                    string concat = DataHelper.GetConcatOperator(CS);

                    for (int passNumber = 1; passNumber <= nbPass; passNumber++)
                    {
                        //"Begin Tran" doit être la 1ère instruction, car si Error un rollback est généré de manière systématique
                        dbTransaction = DataHelper.BeginTran(dbConnection);

                        #region Requête d'Insertion du Template
                        sqlQuery = @"insert into dbo.TRADE
                        (IDI, IDENTIFIER, DISPLAYNAME, DESCRIPTION, DTTRADE, DTTIMESTAMP, SOURCE, DTSYS, TRADEXML, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;

                        sqlQuery += SQLCst.SELECT + "i.IDI, ";
                        sqlQuery += DataHelper.SQLSubstring(CS, "i.IDENTIFIER" + concat + @"'(From Template '" + concat + @"t.identifier" + concat + @"')'", 1, SQLCst.UT_IDENTIFIER_LEN) + ",";
                        sqlQuery += DataHelper.SQLSubstring(CS, "i.DISPLAYNAME" + concat + @"'(From Template '" + concat + @"t.identifier" + concat + @"')'", 1, SQLCst.UT_DISPLAYNAME_LEN) + ",";
                        sqlQuery += SQLCst.NULL + SQLCst.AS + "DESCRIPTION, " + DataHelper.SQLGetDate(CS) + SQLCst.AS + "DTTRADE, " + DataHelper.SQLGetDate(CS) + SQLCst.AS + "DTTIMESTAMP, t.SOURCE,";
                        sqlQuery += DataHelper.SQLGetDate(CS) + SQLCst.AS + "DTSYS,t.TRADEXML," + SQLCst.NULL + SQLCst.AS + "EXTLLINK, 'X'" + SQLCst.AS + "ROWATTRIBUT" + Cst.CrLf;

                        sqlQuery += @"from dbo.INSTRUMENT i
                        inner join dbo.PRODUCT p on (p.IDP = i.IDP)
                        inner join dbo.INSTRUMENT ip on (ip.IDP = p.IDP)" + Cst.CrLf;

                        if (passNumber == 1) // Instrument du même nom que le Product
                            sqlQuery += " and (ip.IDENTIFIER = p.IDENTIFIER) " + Cst.CrLf;

                        sqlQuery += "inner join dbo.TRADE t on (t.IDI = ip.IDI)" + Cst.CrLf;
                        if (passNumber == 1 || passNumber == 2) // Template du même nom que le Product
                            sqlQuery += " and  (t.IDENTIFIER = p.IDENTIFIER)" + Cst.CrLf;

                        // Pour des raisons de performances on duplique le test de "TEMPLATE"
                        sqlQuery += "inner join dbo.TRADESTSYS tsys on (tsys.IDT = t.IDT) and (tsys.IDSTENVIRONMENT = 'TEMPLATE')" + Cst.CrLf;

                        // Uniquement les Instruments sans aucun Template
                        sqlQuery += @" where i.IDI not in 
                        (
                            select tr1.IDI 
                            from dbo.TRADE tr1
                            inner oin dbo.TRADESTSYS tsys1 on + (tsys1.IDT = tr1.IDT)
                            where tsys1.IDSTENVIRONMENT='TEMPLATE'
                        )" + Cst.CrLf;

                        if (passNumber == 2 || passNumber == 3)
                        {
                            // Pour pouvoir ne sélectionner que le premier Template parmi tous les Templates de tous les instruments du product 
                            // auquel est lié le nouvel instrument
                            sqlQuery += @" and t.IDT in ({0})" + Cst.CrLf;

                            string sqlQueryAllTemplate = @"select trt.IDT
                            from dbo.TRADE tr2
                            inner join dbo.TRADESTSYS tsys2 on (tsys2.IDT = tr2.IDT) and (tsys2.IDSTENVIRONMENT = 'TEMPLATE')
                            inner join dbo.INSTRUMENT ns on (ns.IDI = tr2.IDI)
                            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)";

                            // Pour des raisons de performances on duplique le test "Template du même nom que le Product"
                            if (passNumber == 2)
                                sqlQueryAllTemplate += @"where (tr2.IDENTIFIER = pr.IDENTIFIER)";

                            sqlQuery = String.Format(sqlQuery, DataHelper.GetSelectTop(SessionTools.CS, sqlQueryAllTemplate, 1));

                        }
                        #endregion

                        nbRow = DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlQuery);

                        if (0 < nbRow)
                        {
                            #region Insertion dans TRADETRAIL

                            sqlQuery = @"insert into dbo.TRADETRAIL 
                            (IDT, IDA, DTSYS, SCREENNAME, ACTION, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDTRADE_P)
                            select t.IDT";
                            sqlQuery += ", " + SessionTools.Collaborator_IDA.ToString();
                            sqlQuery += ", " + DataHelper.SQLGetDate(SessionTools.CS);
                            sqlQuery += ", " + DataHelper.SQLString("Full");
                            sqlQuery += ", " + DataHelper.SQLString("Create");
                            sqlQuery += ", " + DataHelper.SQLString(SessionTools.HostName);
                            sqlQuery += ", " + DataHelper.SQLString(Software.Name);
                            sqlQuery += ", " + DataHelper.SQLString(Software.VersionBuild);
                            sqlQuery += ", " + DataHelper.SQLString(SessionTools.BrowserInfo);
                            //Le dernier enregistrement dans TRADETRAIL correspond au trade present dans TRADE donc pas de IDTRADE_P car pas de ligne correspondante dans TRADE_P
                            sqlQuery += ", " + SQLCst.NULL + Cst.CrLf;
                            sqlQuery += @"from dbo.TRADE t 
                            inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                            left outer join dbo.TRADESTSYS tst on tst.IDT=t.IDT 
                            where (t.ROWATTRIBUT ='X') and (tst.IDT is null)";
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlQuery);
                            #endregion

                            #region Insertion dans TRADESTSYS
                            sqlQuery = @"insert into dbo.TRADESTSYS
                            (IDT, DTEXECUTION, IDSTENVIRONMENT, DTSTENVIRONMENT, IDASTENVIRONMENT, IDSTBUSINESS, DTSTBUSINESS, IDASTBUSINESS, 
                            IDSTACTIVATION, DTSTACTIVATION, IDASTACTIVATION, IDSTPRIORITY, DTSTPRIORITY, IDASTPRIORITY 
                            )
                            select t.IDT, null as DTEXECUTION, 'TEMPLATE' as IDSTENVIRONMENT, t.DTSYS as DTSTENVIRONMENT, 1 as IDASTENVIRONMENT, 
                            'EXECUTED' as IDSTBUSINESS, t.DTSYS as DTSTBUSINESS, 1 as IDASTBUSINESS, 
                            'REGULAR' as IDSTACTIVATION, t.DTSYS as DTSTACTIVATION, 1 as IDASTACTIVATION, 
                            'REGULAR' as IDSTPRIORITY, t.DTSYS as DTSTPRIORITY, 1 as IDASTPRIORITY
                            from dbo.TRADE t 
                            inner join dbo.INSTRUMENT i on i.IDI=t.IDI 
                            left outer join dbo.TRADESTSYS tst on tst.IDT=t.IDT 
                            where (t.ROWATTRIBUT ='X') and (tst.IDT is null)";

                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlQuery);
                            #endregion
                        }

                        //"Commit"
                        DataHelper.CommitTran(dbTransaction, false);
                    }
                }
                catch (Exception e)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                    //
                    System.Diagnostics.Debug.WriteLine("Insert Template: " + e.Message);
                }
                finally
                {
                    if (null != dbConnection)
                        DataHelper.CloseConnection(dbConnection);
                }
                #endregion Specifique au referentiel: INSTRUMENT (Add Default Template for Instrument)
            }
        }
        #endregion SaveSpecific
        #region Save_ActorFromTemplate
        // RD 20200610 [25357] Modify
        private int Save_ActorFromTemplate(IDbTransaction pDbTransaction, Cst.OTCml_TBL pTableName, int pIdA, int pIdA_Template)
        {
            string sqlInsert = string.Empty;

            string sqlInsert_extlid = @"insert into dbo.EXTLID
            (TABLENAME, ID, IDENTIFIER, VALUE, DTINS, IDAINS)
            select eid.TABLENAME, a.IDA, eid.IDENTIFIER, eid.VALUE, a.DTINS, a.IDAINS
            from dbo.ACTOR a
            inner join dbo.EXTLID eid on (eid.TABLENAME = @TABLENAME) and (eid.ID = @IDA_TEMPLATE)
            where (a.IDA = @IDA)" + Cst.CrLf;

            switch (pTableName)
            {
                case Cst.OTCml_TBL.RISKMARGIN:
                    sqlInsert = @"insert into dbo.RISKMARGIN
                    (IDA, DTENABLED, DTDISABLED, DTINS, IDAINS, IDB, ISGROSSMARGINING, IMWEIGHTINGRATIO, IMSCOPE, EXTLLINK, ROWATTRIBUT)
                    select a.IDA, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, b.IDB, rm.ISGROSSMARGINING, rm.IMWEIGHTINGRATIO, rm.IMSCOPE,
                    rm.EXTLLINK, rm.ROWATTRIBUT
                    from dbo.ACTOR a
                    cross join dbo.RISKMARGIN rm
                    left outer join dbo.BOOK b on (b.IDA = a.IDA) and (b.IDB = rm.IDB)
                    where (a.IDA = @IDA) and (rm.IDA = IDA_TEMPLATE)" + Cst.CrLf;
                    break;
                // RD 20200610 [25357] Add column CLEARINGORGPARAM.ISIMFOREXEASSPOS
                case Cst.OTCml_TBL.CLEARINGORGPARAM:
                    sqlInsert = @"insert into dbo.CLEARINGORGPARAM
                    (IDA, DTENABLED, DTDISABLED, DTINS, IDAINS, IDA_CSS, SPANACCOUNTTYPE, ISIMMAINTENANCE,ISIMFOREXEASSPOS,IMWEIGHTINGRATIO, EXTLLINK, ROWATTRIBUT)
                    select a.IDA, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, cop.IDA_CSS, cop.SPANACCOUNTTYPE, cop.ISIMMAINTENANCE, cop.ISIMFOREXEASSPOS, cop.IMWEIGHTINGRATIO,
                    cop.EXTLLINK, cop.ROWATTRIBUT
                    from dbo.ACTOR a
                    cross join dbo.CLEARINGORGPARAM cop
                    where (a.IDA = @IDA) and (cop.IDA = @IDA_TEMPLATE)" + Cst.CrLf;
                    break;

                case Cst.OTCml_TBL.IMREQMARKETPARAM:
                    sqlInsert = @"insert into dbo.IMREQMARKETPARAM
                    (IDA, DTENABLED, DTDISABLED, DTINS, IDAINS, IDM, POSSTOCKCOVER, EXTLLINK, ROWATTRIBUT)
                    select a.IDA, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, imrmp.IDM, imrmp.POSSTOCKCOVER, cop.EXTLLINK, cop.ROWATTRIBUT
                    from dbo.ACTOR a
                    cross join dbo.IMREQMARKETPARAM imrmp
                    where (a.IDA = @IDA) and (imrmp.IDA = @IDA_TEMPLATE)" + Cst.CrLf;
                    break;

                case Cst.OTCml_TBL.CASHBALANCE:
                    sqlInsert = @"insert into dbo.CASHBALANCE
                    (IDA, DTENABLED, DTDISABLED, DTINS, IDAINS, IDB, ISUSEAVAILABLECASH, CASHANDCOLLATERAL, EXCHANGEIDC, CBSCOPE, ISMANAGEMENTBALANCE,
                    CBCALCMETHOD, CBIDC, EXTLLINK, ROWATTRIBUT)
                    select a.IDA, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, b.IDB, cb.ISUSEAVAILABLECASH, cb.CASHANDCOLLATERAL, cb.EXCHANGEIDC, cb.CBSCOPE,
                    cb.ISMANAGEMENTBALANCE, cb.CBCALCMETHOD, cb.CBIDC, cb.EXTLLINK, cb.ROWATTRIBUT
                    from dbo.ACTOR a
                    cross join dbo.CASHBALANCE cb
                    left outer join dbo.BOOK b on (b.IDA = a.IDA) and (b.IDB = cb.IDB)
                    where (a.IDA = @IDA) and (cb.IDA = @IDA_TEMPLATE)" + Cst.CrLf;
                    break;

                case Cst.OTCml_TBL.ACTORROLE:
                    sqlInsert = @"insert into dbo.ACTORROLE
                    (IDA, DTENABLED, DTDISABLED, DTINS, IDAINS, IDROLEACTOR, IDA_ACTOR, IDSTCHECK, IDSTMATCH, EXTLLINK, ROWATTRIBUT)
                    select a.IDA, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, ar.IDROLEACTOR, ar.IDA_ACTOR, ar.IDSTCHECK, ar.IDSTMATCH, ar.EXTLLINK, ar.ROWATTRIBUT
                    from dbo.ACTOR a
                    cross join dbo.ACTORROLE ar
                    where (a.IDA = @IDA) and (ar.IDA = @IDA_TEMPLATE)" + Cst.CrLf;

                    if (SessionTools.User.userType == RoleActor.SYSOPER)
                        sqlInsert += @"and ar.IDROLEACTOR not in ('SYSADMIN')";
                    else if (SessionTools.User.userType == EFS.Actor.RoleActor.USER)
                        sqlInsert += @"and ar.IDROLEACTOR not in ('SYSADMIN','SYSOPER')";

                    break;

                case Cst.OTCml_TBL.BOOK:

                    // {a} pour les colonnes renseignées à partir de l'acteur Source
                    // {b} pour les colonnes renseignées à partir des books Source
                    string column = @"{a}.IDA,{a}.DTENABLED,{a}.DTDISABLED,{a}.DTINS,{a}.IDAINS,
                    {b}.IDENTIFIER,{b}.DISPLAYNAME,{b}.DESCRIPTION,
                    {b}.IDA_ENTITY,{b}.IDA_STLOFFICE,{b}.IDA_CONTACTOFFICE,{b}.IDC,
                    {b}.LOCALCLASSDERV,{b}.ISULOCALCLASSDERV,{b}.IASCLASSDERV,{b}.ISUIASCLASSDERV,{b}.HEDGECLASSDERV,{b}.ISUHEDGECLASSDERV,
                    {b}.FXCLASS,{b}.ISUFXCLASS,
                    {b}.LOCALCLASSNDRV,{b}.ISULOCALCLASSNDRV,{b}.IASCLASSNDRV,{b}.ISUIASCLASSNDRV,{b}.HEDGECLASSNDRV,{b}.ISUHEDGECLASSNDRV,
                    {b}.SECPOSEFFECT,{b}.ISUSECPOSEFFECT_DEPRECATED,{b}.FUTPOSEFFECT,{b}.ISUFUTPOSEFFECT_DEPRECATED,
                    {b}.OTCPOSEFFECT,{b}.OPTPOSEFFECT,
                    {b}.MTMMETHOD,{b}.ACCRUEDINTMETHOD,{b}.ACCRUEDINTPERIOD,
                    {b}.LINEARDEPPERIOD,{b}.IDA_INVOICEDOFFICE,{b}.ISRECEIVENCMSG,{b}.ISUOPTPOSEFFECT_DEPRECATED,
                    {b}.ISPOSKEEPING,{b}.ISFUTCLEARINGEOD,{b}.ISOPTCLEARINGEOD,{b}.VRFEE,
                    {b}.ISFUTPOSEFCTIGNORE,{b}.ISOPTPOSEFCTIGNORE,
                    {b}.EXTLLINK,{b}.EXTLLINK2,{b}.ROWATTRIBUT";

                    sqlInsert = @"insert into dbo.BOOK 
                    ({0})
                    select {1}
                    from dbo.ACTOR a
                    cross join dbo.BOOK b_acttpl
                    inner join dbo.ACTOR a_acttpl on (a_acttpl.IDA = b_acttpl.IDA)
                    where (a.IDA = @IDA) and (b_acttpl.IDA = @IDA_TEMPLATE)" + Cst.CrLf;

                    sqlInsert = String.Format(sqlInsert, 
                        column.Replace("{a}.", string.Empty).Replace("{b}.", string.Empty),
                        column.Replace("{a}.", "a.").Replace("{b}.", "b_acttpl."));
                    break;

                case Cst.OTCml_TBL.BOOKPOSEFCT:
                    sqlInsert = @"insert into dbo.BOOKPOSEFCT 
                    (IDB, DTENABLED, DTDISABLED, DTINS, IDAINS, TYPEINSTR, IDINSTR, TYPECONTRACT, IDCONTRACT, ASSETCATEGORY,
                    FUTPOSEFFECT, OPTPOSEFFECT, ISFUTPOSEFCTIGNORE, ISOPTPOSEFCTIGNORE, ISFUTCLEARINGEOD, ISOPTCLEARINGEOD, EXTLLINK, ROWATTRIBUT)
                    select b.IDB, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, bpe.TYPEINSTR, bpe.IDINSTR, bpe.TYPECONTRACT, bpe.IDCONTRACT, bpe.ASSETCATEGORY, 
                    bpe.FUTPOSEFFECT, bpe.OPTPOSEFFECT, bpe.ISFUTPOSEFCTIGNORE, bpe.ISOPTPOSEFCTIGNORE, bpe.ISFUTCLEARINGEOD, bpe.ISOPTCLEARINGEOD, bpe.EXTLLINK, bpe.ROWATTRIBUT
                    from dbo.ACTOR a
                    inner join dbo.BOOK b on (b.IDA = a.IDA)
                    inner join dbo.BOOK b_acttpl on (b_acttpl.IDENTIFIER = b.IDENTIFIER)
                    inner join dbo.BOOKPOSEFCT bpe on (bpe.IDB = b_acttpl.IDB)
                    where (a.IDA = @IDA) and (b_acttpl.IDA = @IDA_TEMPLATE)" + Cst.CrLf;
                    break;

                case Cst.OTCml_TBL.BOOKG:
                    sqlInsert = @"insert into dbo.BOOKG 
                    (IDB, DTENABLED, DTDISABLED, DTINS, IDAINS, IDGBOOK, EXTLLINK, ROWATTRIBUT)
                    select b.IDB, a.DTENABLED, a.DTDISABLED, a.DTINS, a.IDAINS, bg.IDGBOOK, bg.EXTLLINK, bg.ROWATTRIBUT
                    from dbo.ACTOR a
                    inner join dbo.BOOK b on (b.IDA = a.IDA)
                    inner join dbo.BOOK b_acttpl on (b_acttpl.IDENTIFIER = b.IDENTIFIER)
                    inner join dbo.BOOKG bg on (bg.IDB = b_acttpl.IDB)
                    where (a.IDA = @IDA) and (b_acttpl.IDA = @IDA_TEMPLATE)" + Cst.CrLf;
                    break;

            }


            int nbRows = 0;

            if (StrFunc.IsFilled(sqlInsert))
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(SessionTools.CS, "IDA", DbType.Int32), pIdA);
                parameters.Add(new DataParameter(SessionTools.CS, "IDA_TEMPLATE", DbType.Int32), pIdA_Template);
                QueryParameters qry = new QueryParameters(SessionTools.CS, sqlInsert, parameters);
                if (null == pDbTransaction)
                    nbRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                else
                    nbRows = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());

                if (0 < nbRows)
                {
                    parameters.Add(new DataParameter(SessionTools.CS, "TABLENAME", DbType.String), pTableName);
                    qry = new QueryParameters(SessionTools.CS, sqlInsert_extlid, parameters);
                    if (null == pDbTransaction)
                        nbRows += DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                    else
                        nbRows += DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                }
            }
            return nbRows;
        }
        #endregion Save_ActorFromTemplate



        #region SetRepositoryTitle
        public virtual void SetRepositoryTitle()
        {
        }
        #endregion SetRepositoryTitle
        #region SetActionDates
        public virtual void SetActionDates()
        {
        }
        #endregion SetActionDates

        #region IsMakingChecking
        private void IsMakingChecking()
        {
            isMakingChecking_ActionChecking = false;
            if (referential.ExistsMakingChecking)
            {
                if (false == referential.isNewRecord)
                {
                    DataRow currentRow = referential.dataRow;
                    if (BoolFunc.IsFalse(currentRow["ISCHK"]))
                    {
                        //Record actuellement UNCHECKED --> Making ou Checking
                        try
                        {
                            int idaLast = Convert.ToInt32(currentRow["IDAINS"]);

                            if (currentRow["IDAUPD"] != Convert.DBNull)
                                idaLast = Convert.ToInt32(currentRow["IDAUPD"]);

                            if (idaLast != SessionTools.Collaborator_IDA)
                            {
                                // Utilisateur courant différent du dernier utilisateur ayant modifié les données --> Checking
                                isMakingChecking_ActionChecking = true;
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        #endregion IsMakingChecking

        #region DisplayButtons
        public virtual void DisplayButtons()
        {
        }
        #endregion DisplayButtons

        #region CreateGridSystem
        protected virtual void CreateGridSystem()
        {
        }
        #endregion CreateGridSystem
        #region WriteControls
        protected virtual void CreateControls()
        {
        }
        #endregion WriteControls


        #region CreateControlMenuChilds
        public virtual void CreateControlMenuChilds()
        {
        }
        #endregion CreateControlMenuChilds
        #region GetMenuChilds
        protected void GetMenuChilds(string pIdMenu, SpheresMenu.Menus pMenus, MenuItem pMenuRoot)
        {
            MenuItem menuRoot = pMenuRoot;
            bool isEnabled = (false == referential.isNewRecord);
            pIdMenu = SessionTools.Menus.TranslateMenu(pIdMenu);
            SpheresMenu.Menu menuParent = pMenus.SelectByIDMenu(idMenu);
            if (null != menuParent)
            {
                bool isAdd = false;
                List<SpheresMenu.Menu> lstMenuChild = new List<SpheresMenu.Menu>();
                List<string> lstMenuDetail = new List<string>();
                List<SpheresMenu.Menu> menus = pMenus.Cast<SpheresMenu.Menu>().ToList();
                menus.ForEach(menu =>
                {
                    if ((menu.IdMenu_Parent == pIdMenu) && menu.IsEnabled && (false == lstMenuDetail.Contains(menu.IdMenu)))
                    {
                        lstMenuDetail.Add(menu.IdMenu);

                        #region Gestion "spécifique" des menus enfants "conditionnés"
                        isAdd = true;
                       
                        Nullable<IdMenu.Menu> currentMenu = IdMenu.ConvertToMenu(menu.IdMenu);
                        if (currentMenu.HasValue)
                        {
                           
                            switch (currentMenu.Value)
                            {
                                case IdMenu.Menu.DrvContract:
                                    if (false == referential.dataRow["PRODUCT_IDENTIFIER"].ToString().ToLower().EndsWith("exchangetradedderivative"))
                                        isAdd = false;
                                    break;
                                case IdMenu.Menu.IOTRACK:
                                    if (referential.dataRow["PROCESS"].ToString() != Cst.ProcessTypeEnum.IO.ToString())
                                        isAdd = false;
                                    break;
                                case IdMenu.Menu.MARGINTRACK:
                                    if (referential.dataRow["PROCESS"].ToString() != Cst.ProcessTypeEnum.RISKPERFORMANCE.ToString())
                                        isAdd = false;
                                    break;
                                case IdMenu.Menu.InputDebtSec:
                                    if (false == referential.dataRow["PRODUCT_IDENTIFIER"].ToString().ToLower().EndsWith("debtsecurity"))
                                        isAdd = false;
                                    break;
                                case IdMenu.Menu.AssetEnv:
                                    string gProduct = referential.dataRow["GPRODUCT"].ToString().ToUpper();
                                    string family = referential.dataRow["FAMILY"].ToString().ToUpper();
                                    if ((gProduct != Cst.ProductGProduct_SEC) && (family != Cst.ProductFamily_RTS) && (family != Cst.ProductFamily_BO))
                                    {
                                        isAdd = false;
                                    }
                                    else
                                    {
                                        menu = AddFAMILYAndGPRODUCTToSpecificMenu(menu, referential.dataRow);
                                    }
                                    break;
                                case IdMenu.Menu.IOTRACKCOMPARE:
                                    if (pIdMenu == IdMenu.GetIdMenu(IdMenu.Menu.IOTRACK))
                                    {
                                        string ioTaskElementType = string.Empty;
                                        if ((null == this.referential.dataRow["IOTASKDET_TYPE"]))
                                            throw new Exception("Column IOTASKDET_TYPE doesn't exist");
                                        ioTaskElementType = this.referential.dataRow["IOTASKDET_TYPE"].ToString();
                                        isAdd = (ioTaskElementType.ToLower() == Cst.IOElementType.COMPARE.ToString().ToLower());
                                        if (isAdd)
                                            menu = GetSpecificMenuForIOTRACKCompare(menu, referential.dataRow);
                                    }
                                    break;
                                case IdMenu.Menu.ImCboeMarket:
                                    if (this.referential.dataRow["INITIALMARGINMETH"].ToString().ToUpper() != "CBOE_MARGIN")
                                        // Affichage du menu "Paramètres pour le déposit "CBOE Margin"", accessible depuis le menu Détail 
                                        // du référentiel Marchés, uniquement si la chambre de compensation rattachée au marché dispose de la méthode 
                                        // de calcul de déposit "CBOE Margin" paramétrée.                    
                                        isAdd = false;
                                    break;
                                case IdMenu.Menu.ImTimsIdemContract:
                                    if (this.referential.dataRow["INITIALMARGINMETH"].ToString().ToUpper() != "TIMS_IDEM")
                                        // Affichage du menu "Paramètres pour le déposit "TIMS IDEM"", accessible depuis le menu Détail 
                                        // du référentiel Contrats Dérivés, uniquement si le contrat est rattaché à un marché dont la chambre de 
                                        // compensation dispose de la méthode de calcul de déposit "TIMS IDEM" paramétrée.                    
                                        isAdd = false;
                                    break;
                            }

                        }
                        #endregion Gestion "spécifique" des menus enfants "conditionnés"
                        
                        if (isAdd)
                        {
                            if (this.consultationMode == Cst.ConsultationMode.ReadOnly)
                            {
                                menu = (SpheresMenu.Menu)menu.Clone();
                                menu.Url = ReplaceInputMode(menu.Url, Cst.DataGridMode.FormViewer);
                            }

                            if (null == pMenuRoot)
                            {
                                pMenuRoot = new MenuItem(menuParent, isEnabled);
                                menuDetail = pMenuRoot;
                            }

                            if (null == pMenuRoot.subItems)
                                pMenuRoot.subItems = new List<MenuItem>();

                            if (false == pMenuRoot.subItems.Exists(item => item.id == menu.Id))
                            {
                                MenuItem item = new MenuItem(menu, isEnabled);
                                pMenuRoot.subItems.Add(item);

                                if ((menu.IdMenu_Parent == pIdMenu) && menu.IsEnabled)
                                {
                                    if ((false == menu.IsAction) && pMenus.IsParent(menu.IdMenu))
                                        GetMenuChilds(menu.IdMenu, pMenus, item);
                                }
                            }
                        }
                    }
                });
            }
        }
        #endregion GetMenuChilds
        #region ReplaceInputMode
        /// <summary>
        /// Retourne le résulat du remplacement ds {pURL} de l'élément InputMode par celui spécifié ds {pGridMode}
        /// </summary>
        /// <param name="pURL"></param>
        /// <param name="pGridMode"></param>
        private static string ReplaceInputMode(string pURL, Cst.DataGridMode pGridMode)
        {
            string ret = pURL;
            Regex regEx = new Regex(@"InputMode=\d+");
            if (regEx.IsMatch(pURL))
            {
                string newInputMode = @"InputMode=" + Convert.ToInt32(pGridMode).ToString();
                ret = regEx.Replace(pURL, newInputMode);
            }
            return ret;
        }
        #endregion ReplaceInputMode

        /// <summary>
        /// Retourne le menu IOTRACKCOMPARE à afficher en fonction du type comparaison
        /// </summary>
        /// <param name="pMenu">Représente le menu IOTRACKCOMPARE générique</param>
        /// <param name="pRow">Représente les données affichées ds le formulaire</param>
        /// <returns></returns>
        private SpheresMenu.Menu GetSpecificMenuForIOTRACKCompare(SpheresMenu.Menu pMenu, DataRow pRow)
        {
            SpheresMenu.Menu ret = (SpheresMenu.Menu)pMenu.Clone();
            if ((null == pRow["ELEMENTSUBTYPE"]))
                throw new Exception("Column ELEMENTSUBTYPE doesn't exist");

            string elementSubType = pRow["ELEMENTSUBTYPE"].ToString();
            if (StrFunc.IsFilled(elementSubType))
            {
                if (false == (Enum.IsDefined(typeof(EFS.SpheresIO.CompareOptions), elementSubType)))
                    throw new Exception(StrFunc.AppendFormat("{0} is not defined in enum EFS.SpheresIO.CompareOptions", elementSubType));

                CompareOptions compareOption = (CompareOptions)Enum.Parse(typeof(CompareOptions), elementSubType);

                string shortName = CompareOptionsAttribute.ConvertToString(compareOption);

                if (StrFunc.IsFilled(shortName))
                    ret.Url = ret.Url.Replace("IOTRACKCOMPARE", StrFunc.AppendFormat("IOTRACKCOMPARE_{0}", shortName.ToUpper()));
            }
            else
            {
                //Compatibilté ascendante usage de CompareOptions.Trades ainsi Spheres® utilisera le fichier IOTRACKCOMPARE_TRADE.xml 
                //Jusqu'à présent spheres ne permettait que des comparisons de Trade
                ret.Url = ret.Url.Replace("IOTRACKCOMPARE", StrFunc.AppendFormat("IOTRACKCOMPARE_{0}", CompareOptionsAttribute.ConvertToString(CompareOptions.Trades).ToUpper()));
            }
            return ret;
        }

        /// <summary>
        /// </summary>
        /// <param name="pMenu">Menu générique</param>
        /// <param name="pRow">Données affichées dans le formulaire</param>
        /// <returns></returns>
        private SpheresMenu.Menu AddFAMILYAndGPRODUCTToSpecificMenu(SpheresMenu.Menu pMenu, DataRow pRow)
        {
            SpheresMenu.Menu ret = (SpheresMenu.Menu)pMenu.Clone();

            if ((null == pRow["FAMILY"]))
                throw new Exception("Column FAMILY doesn't exist");
            if ((null == pRow["GPRODUCT"]))
                throw new Exception("Column GPRODUCT doesn't exist");

            string family = pRow["FAMILY"].ToString();
            string gProduct = pRow["GPRODUCT"].ToString();
            if (StrFunc.IsFilled(family) && StrFunc.IsFilled(gProduct))
            {
                string da = "&DA=";
                da += "[[Data name='FAMILY' datatype='string']]" + family + "[[/Data]];";
                da += "[[Data name='GPRODUCT' datatype='string']]" + gProduct + "[[/Data]]";

                if (ret.Url.Contains("&DA="))
                    ret.Url = ret.Url.Replace("&DA=", da);
                else
                    ret.Url = ret.Url + da;
            }
            return ret;
        }

        #region AddButtonSpecific
        protected void AddButtonSpecific(ReferentialButton pButton, int pIndex)
        {
            Button btn = new Button();
            btn.ID = "btnSpec" + pIndex;
            btn.CausesValidation = false;
            btn.Visible = true;
            btn.CssClass = "btn btn-xs btn-specific";
            btn.Text = Ressource.GetString(pButton.title, true);
            if (pButton.tooltipSpecified)
                btn.ToolTip = Ressource.GetString(pButton.tooltip, true);

            string subTitle = Ressource.GetMenu_Shortname2(idMenu, idMenu);
            string url = pButton.url;
            if (url.ToString().IndexOf("&FK=") < 0)
            {
                // NB: Dans certains cas, on override l'usage de la FK (ex. ACTORAMOUNT_H)
                url += String.Format("&FK={0}", valueDataKeyField);
            }
            else if (url.ToString().IndexOf("&FK=%%COLUMN_VALUE%%") > 0)
            {
                // NB: Usage d'une FK autre que la FK standard (ex. ACTORAMOUNT_H, IOTRACK)
                // Utilisation de la chaîne: &FK=%%COLUMN_VALUE%%<fkColumnName>%% où <fkColumnName> représente le nom de la colonne contenant 
                // la donnée faisant office de AK, utile pour la FK du référentiel enfant appelé (ex. &FK=%%COLUMN_VALUE%%IDPROCESS_L%%)
                int posStart = url.IndexOf("&FK=%%COLUMN_VALUE%%") + "&FK=%%COLUMN_VALUE%%".Length;
                int posEnd = url.IndexOf("%%", posStart);
                string fkColumnName = url.Substring(posStart, posEnd - posStart);
                ReferentialColumn rrc = referential[fkColumnName];
                if (rrc == null)
                {
                    btn.Enabled = false;
                }
                else
                {
                    url = pButton.url.Replace("&FK=%%COLUMN_VALUE%%" + fkColumnName + "%%", "&FK=" + ((TextBox)rrc.ControlMain).Text);
                    rrc = null;
                }
            }
            url += String.Format("&SubTitle={0}",
                    HttpUtility.UrlEncode(subTitle + ": " + referential.dataRow[referential.IndexColSQL_KeyField].ToString(), System.Text.Encoding.UTF8));
            btn.OnClientClick = JavaScript.GetWindowOpen(url.ToString()) + ";return false;";

            //TableCell td = new TableCell();
            //td.ID = "td" + button.ID;
            //td.VerticalAlign = VerticalAlign.Middle;
            //td.HorizontalAlign = HorizontalAlign.Left;
            //td.Controls.Add(button);
            //pTr.Cells.Add(td);
        }
        #endregion



        /// <summary>
        /// Construction du GridSystem
        /// </summary>
        /// <param name="pnlGridSystem"></param>
        /// <returns></returns>
        protected PlaceHolder ConstructGridSystem(Panel pnlGridSystem)
        {
            PlaceHolder plhGrid = new PlaceHolder();
            // Création de la forme si inexistante et construction d'un gridSystem par defaut
            if (false == referential.formSpecified)
            {
                referential.CreateForm(idMenu);
                SaveForm();
            }

            if (referential.formSpecified)
            {
                if (StrFunc.IsEmpty(referential.form.mnu))
                    referential.form.mnu = ControlsTools.MainMenuName(idMenu);

                // Initialisation du formulaire
                referential.form.SetClass(pnlGridSystem);

                // Construction du GridSystem (Architecture de la forme avec application du GridSystem BootStrap)
                if (ArrFunc.IsFilled(referential.form.tags))
                {
                    List<ReferentialColumn> columns = referential.Column.ToList();
                    List<BSGridTag> tags = referential.form.tags.Cast<BSGridTag>().ToList();

                    referential.form.DisplayConditionalGridSystem(this);

                    tags.ForEach(tag =>
                    {
                        tag.LstColumns = columns;
                        tag.CreateGridSystem();
                        plhGrid.Controls.Add(tag.Control);
                    });
                }
            }
            return plhGrid;
        }
        private void SaveForm()
        {
            string filename = "Repository_" + referential.TableName;
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(RepositoryForm), referential.form);
            serializeInfo.IsXMLTrade = false;
            AppInstance appInstance = SessionTools.NewAppInstance();
            string path = appInstance.GetTemporaryDirectory(AppInstance.AddFolderSessionId.False);
            if (false == path.EndsWith(@"\"))
                path += @"\";

            if (File.Exists(path + filename))
                File.Delete(path + filename);
            CacheSerializer.Serialize(serializeInfo, path + filename + ".xml");										
        }

        public bool IsColumnReadOnly(ReferentialColumn pRc)
        {
            return referential.isLookReadOnly || (pRc.IsUpdatableSpecified && (false == pRc.IsUpdatable.Value) && (referential.consultationMode == Cst.ConsultationMode.PartialReadOnly));
        }

        #endregion Methods

    }
}

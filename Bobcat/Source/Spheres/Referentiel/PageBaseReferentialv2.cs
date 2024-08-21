#region VSS Auto-Comments
/* 
 * ********************************************************************* 
 $History: PageBaseReferentialv2.cs $
 * 
 * *****************  Version 146  *****************
 * User: Eric         Date: 1/07/16    Time: 10:35
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 145  *****************
 * User: Razik        Date: 3/11/15    Time: 13:39
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 144  *****************
 * User: Eric         Date: 15/10/15   Time: 16:06
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 143  *****************
 * User: Pascal       Date: 13/10/15   Time: 16:57
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 142  *****************
 * User: Eric         Date: 22/04/15   Time: 17:37
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 141  *****************
 * User: Filipe       Date: 23/12/14   Time: 15:09
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 140  *****************
 * User: Filipe       Date: 12/12/14   Time: 15:26
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 139  *****************
 * User: Filipe       Date: 29/10/14   Time: 15:52
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 138  *****************
 * User: Pascal       Date: 28/10/14   Time: 10:05
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 137  *****************
 * User: Razik        Date: 14/10/14   Time: 10:15
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 136  *****************
 * User: Pascal       Date: 30/09/14   Time: 10:47
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 135  *****************
 * User: Eric         Date: 8/07/14    Time: 8:44
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 134  *****************
 * User: Eric         Date: 2/07/14    Time: 11:53
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 133  *****************
 * User: Filipe       Date: 13/06/14   Time: 14:52
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 132  *****************
 * User: Filipe       Date: 11/06/14   Time: 18:28
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 131  *****************
 * User: Filipe       Date: 11/06/14   Time: 18:19
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 130  *****************
 * User: Pascal       Date: 16/04/14   Time: 12:27
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 129  *****************
 * User: Pascal       Date: 9/04/14    Time: 12:55
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * *****************  Version 128  *****************
 * User: Pascal       Date: 3/04/14    Time: 12:31
 * Updated in $/Code_Source/Spheres/Referentiel
 * 
 * **********************************************************************
 * 
*/
#endregion

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.Process;
using EFS.Restriction;
using EFS.SpheresIO;
using EfsML.Business;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
//20071212 FI Ticket 16012 => Migration Asp2.0
using SpheresMenu = EFS.Common.Web.Menu;

namespace EFS.Referential
{
    /// <summary>
    /// Description résumée de PageBaseReferentialv2.
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public partial class PageBaseReferentialv2 : PageBase
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
        private string mainMenuClassName;
        private string objectName;
        //        
        private string dataKeyField, valueDataKeyField;
        private string dataForeignKeyField, valueForeignKeyField;
        //
        /// <summary>
        /// Indicateur à true si formulaire en mode nouvelle saisie
        /// <para>Request.QueryString["N"]</para>
        /// </summary>
        private bool isNewRecord;
        /// <summary>
        /// Indicateur à true si formulaire en mode duplication
        /// Request.QueryString["DPC"]
        /// </summary>
        private bool isDuplicateRecord;
        /// <summary>
        /// 
        /// </summary>
        private Cst.ConsultationMode consultationMode;

        public string pageFooterLeft;
        public string pageFooterRight;
        public string pageInfoLeft;
        public string pageInfoRight;

        /* FI 20210208 [XXXXX] Mise en commentaire ( referential est désormais une variable session puisque utilisée dans une WebMethod )
        /// <summary>
        /// 
        /// </summary>
        private ReferentialsReferential referential;
        */
        /// Représente un indicateur qui signale si le formulaire est ouvert depuis le datagrid
        /// </summary>
        private bool _isOpenFromGrid;

        private string listType;
        private string idMenu;
        private string titleMenu;
        private Nullable<bool> _isExistMenuChild; //true s'il existe des sous menus
        private bool isMenuChildLoaded;
        private skmMenu.MenuItemParent MenuChild;

        private PlaceHolder plhMenuDetail;
        private readonly skmMenu.MenuItemParent menuDetail;
        // FI 20200121 [XXXXX] m_alImageButtonOpenBanner n'est plus nécessaire
        //private ArrayList m_alImageButtonOpenBanner;


        #endregion Members

        #region Accessor
        /// <summary>
        /// Obtient ou définie le nom d'identification du jeu de données dans le cache du serveur web
        /// </summary>
        /// FI 20210208 [XXXXX] add
        public string DataCacheKeyReferential
        {
            get
            {
                return BuildDataCacheKey("Referential");
            }
        }

        /// <summary>
        /// Variable session de de type ReferentialsReferential
        /// </summary>
        /// FI 20210208 [XXXXX] add
        public ReferentialsReferential Referential
        {
            get
            {
                return DataCache.GetData<ReferentialsReferential>(DataCacheKeyReferential);
            }
            set
            {
                DataCache.SetData<ReferentialsReferential>(DataCacheKeyReferential, value);
            }
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected Panel PanelForm
        {
            get
            {
                int i = (null != ScriptManager) ? 1 : 0;
                return ((Panel)Form.Controls[i]);
            }
        }

        /// <summary>
        /// Obtiens true s'il existe des sous menus
        /// </summary>
        private bool IsExistMenuChild
        {
            get
            {
                if (_isExistMenuChild == null)
                {
                    SpheresMenu.Menus menus = SessionTools.Menus;
                    _isExistMenuChild = (null != menus);
                    if (Referential.consultationMode == Cst.ConsultationMode.ReadOnlyWithoutChildren)
                        _isExistMenuChild = false;
                    else
                        _isExistMenuChild = (bool)_isExistMenuChild && menus.IsParent(idMenu);
                }
                return (bool)_isExistMenuChild;
            }
        }

        /// RD 20100503 [16977]
        public bool IsDisplayColumn
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
        public string DataReferentielName
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
        public DataSet DataReferentiel
        {
            get
            {
                return DataCache.GetData<DataSet>(DataReferentielName);
            }
            set
            {
                DataCache.SetData<DataSet>(DataReferentielName, value);
            }
        }

        /// <summary>
        /// Obtient le dataSet associé au grid lorsque le formulaire est ouvert depuis le grid
        /// <para>Retourne null si le grid n'est pas chargé ou que la session est indisponible</para>
        /// <remarks>Ce dataset est sauvegardé en session</remarks>
        /// </summary>
        /// <returns></returns>
        public DataSet DatasetGrid
        {
            get
            {
                DataSet ret = null;
                if (_isOpenFromGrid)
                {
                    string dsGridCacheName = Request.QueryString["DS"];
                    ret = DataCache.GetData<DataSet>(dsGridCacheName);
                }
                return ret;
            }
        }
        /// <summary>
        /// Obtient true si le grid associé au formulaire a été chargé au moins 1 fois
        /// <para>Depuis la 2.6 les grids ne sont pas systématiquement chargé à l'ouverture</para>
        /// </summary>
        public bool IsDatasetGridFilled
        {
            get
            {
                bool ret = false;
                //
                DataSet dsGrid = DatasetGrid;
                if (null != dsGrid)
                    ret = (dsGrid.Tables[0].Columns.Count > 0);//Il y a des colonnes des que le grid a été chargé 
                //
                return ret;
            }
        }



        /// <summary>
        /// Obtient true si le formulaire s'ouvre suite à un click sur le grid
        /// </summary>
        public bool IsOpenFromGrid
        {
            get { return _isOpenFromGrid; }
        }

        /// <summary>
        /// Obtient le titre de la page au format Html
        /// </summary>
        private string HtmlName
        {
            get
            {
                string ret;
                if (StrFunc.IsFilled(Request.QueryString["TitleRes"]))
                    ret = Ressource.GetString(Request.QueryString["TitleRes"], true);
                else
                    ret = Ressource.GetString(listType, true);

                string resMenu = Ressource.GetMenu_Fullname(idMenu, StrFunc.IsEmpty(titleMenu) ? objectName : titleMenu);

                return ret + ": " + HtmlTools.HTMLBold(resMenu);
            }
        }

        /// <summary>
        /// Obtient une ressource en fonction du mode
        /// </summary>
        private string Mode
        {
            get { return Ressource.GetString_SelectionCreationModification(consultationMode, isNewRecord); }
        }
        #endregion Accessor

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public PageBaseReferentialv2()
        {
            _isOpenFromGrid = false;

            plhMenuDetail = new PlaceHolder();
            menuDetail = new skmMenu.MenuItemParent(1);
            isMenuChildLoaded = false;

            pageFooterLeft = string.Empty;
            pageFooterRight = string.Empty;
            pageInfoLeft = string.Empty;
            pageInfoRight = string.Empty;
        }
        #endregion constructor


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20160804 [Migration TFS] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        // EG 20210222 [XXXXX] Appel OpenerReload (présent dans PageBase.js) en remplacement de OpenerCallReload
        // EG 20210222 [XXXXX] Appel SetFocus (présent dans PageBase.js)
        protected override void OnInit(System.EventArgs e)
        {
            // TEST PL -----------------------------------------------------------------------------------------------------------------------
            string redirectDirective = Request.QueryString["Redirect"];
            if (StrFunc.IsFilled(redirectDirective))
            {
                string redirectFK = Request.QueryString["PKV"];
                Response.Redirect(this.Request.ApplicationPath.ToString() + "/List.aspx?Log=IOTRACK&InputMode=2&FK=" + redirectFK, true);
            }
            // TEST PL -----------------------------------------------------------------------------------------------------------------------

            base.OnInit(e);

            #region Variables initialization
            //example: "http://localhost/OTC/Referential/Referential.aspx?T=Repository&O=BUSINESSCENTER&M=0&N=0&PK=IDBC&PKV=ARBA&FK=&FKV=&F=frmConsult&IDMenu=16540"
            AbortRessource = true;

            objectName = Request.QueryString["O"];
            listType = Request.QueryString["T"];		                    //Folder pour recherche du fichier XML 
            if (String.IsNullOrEmpty(listType))
                listType = Cst.ListType.Repository.ToString();  // FI 20160804 [Migration TFS] use Repository Folder

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
            mainMenuClassName = ControlsTools.MainMenuName(idMenu);
            titleMenu = Request.QueryString["TitleMenu"];
            _isOpenFromGrid = StrFunc.IsFilled(Request.QueryString["DS"]);  //DS Comme dataset (dataset source du datagrid)

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
                    JavaScript.CallFunction(this, "OpenerReload()");
                }
            }

            string condApp = Request.QueryString["CondApp"];
            string[] param = ReferentialTools.GetQueryStringParam(Page); // %%PARAM%%
            //
            // FI 20160804 [Migration TFS] Repository instead of Referential
            Cst.ListType listeTypeEnum = Cst.ListType.Repository;
            if (Enum.IsDefined(typeof(Cst.ListType), listType))
                listeTypeEnum = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), listType);
            //
            // FI 20141211 [20563]  
            //PL 20160914 Use DecodeDA()
            Dictionary<string, ReferentialsReferentialStringDynamicData> dynamicArgs = ReferentialTools.CalcDynamicArgumentFromHttpParameter2(Ressource.DecodeDA(Request.QueryString["DA"]));


            List<string> ObjectNameForDeserialize = ReferentialTools.GetObjectNameForDeserialize(idMenu, objectName);
            // FI 20201215 [XXXXX] Alimentation de valueForeignKeyField
            // EG 20231114 [WI736] Passage paramètre pIsFormCalling = True
            ReferentialTools.DeserializeXML_ForModeRW(SessionTools.CS, listeTypeEnum, ObjectNameForDeserialize, condApp, param, dynamicArgs, valueForeignKeyField, out ReferentialsReferential referentialTmp, true);

            Referential = referentialTmp;
            Referential.SetPermission(idMenu);

            // FI 20141211 [20563] Mise en commentaire
            //referential.SetDynamicArgs(null);
            //if (StrFunc.IsFilled(Request.QueryString["DA"]))
            //    referential.SetDynamicDatas(ReferentialTools.CalcDynamicArgumentFromHttpParameter2(Request.QueryString["DA"]));


            PageTitle = HtmlTools.HTMLBold_Remove(HtmlName) + " - " + Mode;

            ReferentialTools.InitializeReferentialForForm_2(this, Referential, isNewRecord, isDuplicateRecord, consultationMode, dataKeyField, valueDataKeyField);

            if (isDuplicateRecord)
            {
                isDuplicateRecord = false;
                isNewRecord = true;
                Referential.isNewRecord = true;
            }
            #endregion

            #region BuildPage
            string subTitle = (StrFunc.IsFilled(pageInfoLeft) ? pageInfoLeft + " - " : string.Empty) + Mode.ToUpper();
            HtmlPageTitle titleLeft = new HtmlPageTitle(HtmlName, subTitle, pageFooterLeft, Color.Red, "FooterLeftId");
            HtmlPageTitle titleRight = new HtmlPageTitle(null, pageInfoRight, pageFooterRight, Color.Transparent, "FooterRightId");
            //			
            GenerateHtmlForm();
            //
            FormTools.AddBanniere(this, Form, titleLeft, titleRight, Referential.HelpUrl, idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, idMenu);
            #endregion
            //
            #region MenuChild
            if (IsExistMenuChild)
            {
                skmMenu.Menu menuMain;
                menuMain = new skmMenu.Menu(0, "menuMain", CstCSS.BannerMenu, CstCSS.BannerMenu, null, null, null)
                {
                    DataSource = menuDetail.InitXmlWriter(),
                    Layout = skmMenu.MenuLayout.Horizontal,
                    LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN
                };
                plhMenuDetail.Controls.Add(menuMain);
            }

            Control pnlToolBar;
            pnlToolBar = FindControl("tblist");
            if (pnlToolBar != null)
                pnlToolBar.DataBind();
            #endregion

            #region SetFocus
            //Set focus on first control (Seulement en création, afin d'éviter les désagrément sur un DDL liés à la molette de la souris)
            if (!IsPostBack && Referential.isNewRecord)
            {
                if (StrFunc.IsFilled(Referential.firstVisibleAndEnabledControlID))
                {
                    JavaScript.CallFunction(this, "SetFocus(" + JavaScript.JSString(Referential.firstVisibleAndEnabledControlID) + ")");
                }
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnLoad(EventArgs e)
        {
            if (IsPostBack)
                MethodsGUI.SetEventHandler(FindControl("divmain").Controls);

            base.OnLoad(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            #region Specifique au référentiel DIVIDENDE_H, enfant de ASSET_EQUITY
            try
            {
                bool isApplyOffset = StrFunc.IsFilled(Request.Params["__EVENTTARGET"]) && Request.Params["__EVENTTARGET"].StartsWith("EFS_ApplyOffset");
                if (isApplyOffset)
                    ApplyOffSet(Request.Params["__EVENTTARGET"], Request.Params["__EVENTARGUMENT"]);
            }
            catch
            {
                //PL 20210122 Avec ValidateRequest="true" sur Referential.aspx pour interdire le STORED CROSS-SITE SCRIPTING 
                //            L'interrogation de "Request.Params" lève une erreur sur les formulaires comportant des données XML (ex. Formulaire marquage Tracker).
            }
            #endregion 

            Control ctrl = this.FindControl("tblFooter");
            if (null != ctrl)
            {
                Control ctrlFooterLeft = ctrl.FindControl("FooterLeftId");
                if (StrFunc.IsFilled(pageFooterLeft))
                    ((TableCell)ctrlFooterLeft).Text = pageFooterLeft;

                Control ctrlFooterRight = ctrl.FindControl("FooterRightId");
                if (StrFunc.IsFilled(pageFooterRight))
                    ((TableCell)ctrlFooterRight).Text = pageFooterRight;
            }

            // FI 20141021 [20350] Call SetRequestTrackBuilderIemLoad
            SetRequestTrackBuilderIemLoad();

            base.OnPreRender(e);
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si consultation d'un élément
        /// </summary>
        /// FI 20141021 [20350] Add Method
        private void SetRequestTrackBuilderIemLoad()
        {
            if (Referential.RequestTrackSpecified && SessionTools.IsRequestTrackConsultEnabled)
            {
                if ((false == IsPostBack) && StrFunc.IsFilled(valueDataKeyField) && (BoolFunc.IsFalse(Request.QueryString["DPC"])))
                {
                    RequestTrackItemBuilder builder = new RequestTrackItemBuilder
                    {
                        action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemLoad, RequestTrackActionMode.manual),
                        referential = Referential,
                        Row = new DataRow[] { Referential.dataRow }
                    };
                    this.RequestTrackBuilder = builder;
                }
            }
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si Création, Modification, Annulation d'un élément
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pRow">Enregistrement qui sera inscrit dans le log</param>
        /// FI 20141021 [20350] Add Method
        private void SetRequestTrackBuilderItemProcess(RequestTrackProcessEnum pProcessType, DataRow pRow)
        {
            if (Referential.RequestTrackSpecified && SessionTools.IsRequestTrackProcessEnabled)
            {
                RequestTrackItemBuilder builder = new RequestTrackItemBuilder
                {
                    action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual),
                    ProcessType = pProcessType,
                    referential = Referential,
                    Row = new DataRow[] { pRow }
                };
                this.RequestTrackBuilder = builder;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void GenerateHtmlForm()
        {
            //base.GenerateHtmlForm();
            Form = new HtmlForm();

            Panel pnlMain = new Panel
            {
                ID = "divmain",
                CssClass = this.CSSMode + " " + mainMenuClassName
            };
            Form.Controls.Add(pnlMain);

            //Si ScriptManager créé par pageBase (Voir onInit) => ScriptManager est le 1er contrôle de la form
            if (null != ScriptManager)
                Form.Controls.AddAt(0, ScriptManager);

            AntiForgeryControl();
            AddInputHiddenAutoPostback();
            // FI 20200217 [XXXXX] Add Call AddInputHidenGUID
            AddInputHiddenGUID();
            //MaintainScrollPositionOnPostBack est positionné ici
            //car Il faut absolument une form pour utiliser la property MaintainScrollPositionOnPostBack
            MaintainScrollPositionOnPostBack = IsMaintainScrollAutomatic;

            CreateToolbar();
            WriteControls();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));

            // FI 20210218 [XXXXX] S'il existe un tag Jquery => il y a obligatoirement un fichier js associé  
            if (Referential.JQuerySpecified)
                ScriptManager.Scripts.Add(new ScriptReference($"~/Javascript/{listType}/{objectName}.js"));

            // FI 20210202 [XXXXX] Add LoadAutoCompleteReferentialColumnRelation and CheckAutoCompleteReferentialColumnRelation
            JQuery.WriteInitialisationScripts(this, "LoadAutoCompleteReferentialColumnRelation", "LoadAutoCompleteReferentialColumnRelation();");
            JQuery.WriteInitialisationScripts(this, "CheckAutoCompleteReferentialColumnRelation", "CheckAutoCompleteReferentialColumnRelation();");

            // FI 202102008 [XXXXX] Add
            JQuery.WriteInitialisationScripts(this, "LoadAutoCompleteReferentialColumn", "LoadAutoCompleteReferentialColumn();");
            // EG 20230921 [WI711] Referential Page: Display columns name enhancement via jQuery (without PostBack)
            JQuery.WriteInitialisationScripts(this, "ToggleReferentialColumnName", "ToggleReferentialColumnName();");

            // FI 20210218 [XXXXX] Add
            if (Referential.JQuerySpecified)
            {
                foreach (ReferentialsReferentialJQuery item in Referential.JQuery)
                    JQuery.WriteInitialisationScripts(this, item.function, $"{item.function}();");
            }
        }

        #region public AddFooterLeft
        public void AddFooterLeft(string pMsgAdd)
        {
            if (StrFunc.IsFilled(pageFooterLeft))
                pageFooterLeft += Cst.CrLf;
            pageFooterLeft += pMsgAdd + Cst.HTMLSpace;
        }
        #endregion
        #region public AddFooterRight
        public void AddFooterRight(string pMsgAdd)
        {
            if (StrFunc.IsFilled(pageFooterRight))
                pageFooterRight += Cst.CrLf;
            pageFooterRight += pMsgAdd + Cst.HTMLSpace;
        }
        #endregion
        #region public AddInfoLeft
        public void AddInfoLeft(string pMsgAdd)
        {
            if (StrFunc.IsFilled(pageInfoLeft))
                pageInfoLeft += Cst.CrLf;
            pageInfoLeft += pMsgAdd + Cst.HTMLSpace;
        }
        #endregion
        #region public AddInfoRight
        public void AddInfoRight(string pMsgAdd)
        {
            if (StrFunc.IsFilled(pageInfoRight))
                pageInfoRight += Cst.CrLf;
            pageInfoRight += pMsgAdd + Cst.HTMLSpace;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSuffix"></param>
        // EG 20180525 [23979] IRQ Processing
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200929 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections diverses
        protected void CreateToolbar()
        {
            bool isMakingChecking_ActionChecking = false;
            Panel pnlAllToolbar = new Panel
            {
                ID = "divalltoolbar"
            };
            PanelForm.Controls.Add(pnlAllToolbar);

            Panel pnlToolBarList = new Panel
            {
                ID = "tblist"
            };
            pnlAllToolbar.Controls.Add(pnlToolBarList);

            Panel pnlToolBarButton = new Panel
            {
                ID = "divtoolbar"
            };

            #region Validation (Enregistrer, Annuler...)
            if ((Referential.consultationMode == Cst.ConsultationMode.ReadOnly) || Referential.isLookReadOnly)
            {
                //Pas de boutons si ReadOnly
            }
            else if (Referential.consultationMode == Cst.ConsultationMode.Select)
            {
                AddLinkButtonValidate(pnlToolBarButton);
                AddLinkButtonCancel(pnlToolBarButton);
            }
            else if (((Referential.consultationMode == Cst.ConsultationMode.Normal) ||
                      (Referential.consultationMode == Cst.ConsultationMode.PartialReadOnly)) &&
                (Referential.Create || Referential.Modify || Referential.Remove))
            {
                #region MakingChecking
                //PL 20161124 - RATP 4Eyes - MakingChecking
                if (Referential.ExistsMakingChecking)
                {
                    if (!Referential.isNewRecord)
                    {
                        DataRow currentRow = Referential.dataRow;
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
                                    // Utilisateur courant différent du dernier utilisateur ayant modifié les données --> Cheking
                                    isMakingChecking_ActionChecking = true;
                                }
                            }
                            catch { }
                        }
                    }
                }
                #endregion MakingChecking

                //PL 20161124 - RATP 4Eyes - MakingChecking
                if (isMakingChecking_ActionChecking)
                {
                    AddLinkButtonRecord(pnlToolBarButton, "btnValidate");
                }
                else
                {
                    AddLinkButtonRecord(pnlToolBarButton, "btnRecord2");
                    AddLinkButtonCancel(pnlToolBarButton);
                    AddLinkButtonRecord(pnlToolBarButton, "btnApply");
                }
                if (!Referential.isNewRecord)
                {
                    //Note: Si non spécifié , on affiche le bouton "Remove"
                    if (!Referential.RemoveSpecified || Referential.Remove)
                        AddLinkButtonRemove(pnlToolBarButton);

                    //PL 20161124 - RATP 4Eyes - MakingChecking
                    if (!isMakingChecking_ActionChecking)
                    {
                        //Note: Si non spécifié , on affiche le bouton "Duplicate"
                        if (!Referential.DuplicateSpecified || Referential.Duplicate)
                            AddLinkButtonDuplicate(pnlToolBarButton);
                    }
                }


                //PL 20120621 New features
                if ((!Referential.isNewRecord) && (Referential.ButtonSpecified))
                {
                    int index = 0;
                    // EG 20210916 [XXXXX] Masquer le bouton spécifique IO si l'on n'est pas sur une ligne IO
                    foreach (ReferentialButton button in Referential.Button)
                    {
                        bool isManaged = true;

                        if ((null != Referential["GROUPTRACKER"]) && button.url.Contains("Log=IOTRACK"))
                            isManaged = (Cst.GroupTrackerEnum.IO == ReflectionTools.ConvertStringToEnum<Cst.GroupTrackerEnum>(Referential.dataRow["GROUPTRACKER"].ToString()));

                        if (isManaged)
                            AddLinkButtonSpecific(pnlToolBarButton, button, ++index);
                    }
                }
            }
            #endregion

            if (pnlToolBarButton.HasControls())
                pnlToolBarList.Controls.Add(pnlToolBarButton);

            AddButtonChildReferential(pnlToolBarList);

            if (!isMakingChecking_ActionChecking)
            {
                if (!Referential.isNewRecord)
                {
                    AddToolBarAction(pnlToolBarList);
                }

                if (_isOpenFromGrid && (!Referential.isNewRecord))
                {
                    //La toolbar n'apparaît que sur le head
                    AddToolBarConsult(pnlToolBarList);
                }
            }

            AddLinkButtonIRQ(pnlToolBarList);

            // FI 20221114 [XXXX] LiteralControl
            // ToolTip = "Filler" => pour appliquer CSS div[id^="tblist"] > div[title="Filler"] (voir EFSThemeCommon.css)
            Panel panelFiller = new Panel() { ToolTip = "Filler" };
            // new LiteralControl(Cst.HTMLSpace) => pour appliquer la règle CSS  div[id^=tblist] > div:not(:first-child):not(:last-child):not(:empty) (voir EFSThemeCommon.css)
            panelFiller.Controls.Add(new LiteralControl(Cst.HTMLSpace));
            pnlToolBarList.Controls.Add(panelFiller);

            Panel pnlCheck = new Panel
            {
                ID = "tbTrace"
            };
            //pnlCheck.CssClass = "checkright";
            pnlToolBarList.Controls.Add(pnlCheck);


            AddCheckBox(pnlCheck);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSuffix"></param>
        /// <param name="pTr"></param>
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
        protected void AddToolBarAction(Panel pPnlParent)
        {
            skmMenu.Menu mnuToolBar = new skmMenu.Menu(2, "mnuAction", null, null, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(3);

            string currentDisplayName = string.Empty;
            if (Referential.IndexColSQL_DISPLAYNAME != -1)
                currentDisplayName = Referential.dataRow[Referential.IndexColSQL_DISPLAYNAME].ToString();

            //Add an image to attach an image to data (only for referential that have LO column (eg.: LOLOGO ))
            string LO_ColumnName = "LOLOGO";
            string imagePath = "fas fa-icon fa-file-image";
            string toolTip = Ressource.GetString("btnLogo");
            bool isEmpty = true;
            bool isEnabled = (Referential.LogoSpecified && Referential.Logo.Value);
            isEnabled &= (Referential.Column[Referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            // to disable: set isEmpty to true
            if (isEnabled)
            {
                if (Referential.Logo.columnnameSpecified)
                    LO_ColumnName = Referential.Logo.columnname;
                isEmpty = (Referential.dataRow[LO_ColumnName] == null || StrFunc.IsEmpty(Referential.dataRow[LO_ColumnName].ToString().Trim()));
            }
            if (!isEmpty)
            {
                // 20100115 MF - Adding custom image in case we have an image attribute ("image") specified on the Logo node
                if (Referential.Logo.imageSpecified && System.IO.File.Exists(Server.MapPath(String.Format("images/{0}", Referential.Logo.image))))
                    imagePath += " green";
                if (Referential.Logo.tooltipSpecified)
                    toolTip = Ressource.GetString(Referential.Logo.tooltip);
            }
            mnu[0] = new skmMenu.MenuItemParent(0)
            {
                Enabled = isEnabled,
                Hidden = (!isEnabled),
                aToolTip = toolTip,
                eImageUrl = imagePath
            };

            string[] columnName = new string[1];
            columnName[0] = Referential.Column[Referential.IndexDataKeyField].ColumnName;

            string[] columnDatatype = new string[1];
            columnDatatype[0] = Referential.Column[Referential.IndexDataKeyField].DataType.value;

            string[] columnValue = new string[1];
            columnValue[0] = valueDataKeyField;

            string columnIDAUPD = string.Empty;
            string columnDTUPD = string.Empty;
            if (true == Referential.ExistsColumnsUPD)
            {
                columnIDAUPD = "IDAUPD";
                columnDTUPD = "DTUPD";
            }

            // PM 20240604 [XXXXX] Ajout GUID pour gestion des restrictions pour les tools
            //mnu[0].eUrl = JavaScript.GetWindowOpenDBImageViewer(Referential.TableName, Referential.TableName, LO_ColumnName, columnName, columnValue, columnDatatype, currentDisplayName, columnIDAUPD, columnDTUPD);
            mnu[0].eUrl = JavaScript.GetWindowOpenDBImageViewer(Referential.TableName, Referential.TableName, LO_ColumnName, columnName, columnValue, columnDatatype, currentDisplayName, columnIDAUPD, columnDTUPD, this.GUID);
            
            #region NOTEPAD
            imagePath = "fas fa-icon fa-file-alt";

            toolTip = Ressource.GetString("btnNotePad");
            isEmpty = true;
            isEnabled = (Referential.NotepadSpecified && Referential.Notepad.Value);
            isEnabled &= (Referential.Column[Referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            // to disable: set isEmpty to true
            if (isEnabled)
                isEmpty = (Referential.dataRow[Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME"] == null || StrFunc.IsEmpty(Referential.dataRow[Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME"].ToString().Trim()));
            //Tips TRIM (PL)
            if (listType == Cst.ListType.TRIM.ToString())
                toolTip = "Thread";
            if (!isEmpty)
            {
                string displayName = Referential.dataRow[Cst.AliasNOTEPADACTOR + "_" + "DISPLAYNAME"].ToString();
                DateTime dtUpd;
                if (Referential.dataRow[Cst.AliasNOTEPAD + "_" + "DTUPD"] == DBNull.Value)
                    dtUpd = DateTime.MinValue;
                else
                    dtUpd = Convert.ToDateTime(Referential.dataRow[Cst.AliasNOTEPAD + "_" + "DTUPD"]);
                imagePath += " green";
                if (dtUpd != DateTime.MinValue)
                {
                    // FI 20200820 [25468] Date systèmes en UTC 
                    DateTimeTz dateTimeTz = new DateTimeTz(dtUpd, "Etc/UTC");
                    toolTip += Cst.CrLf + RessourceExtended.GetString_LastModifyBy(dateTimeTz, displayName, true);
                }
            }
            mnu[1] = new skmMenu.MenuItemParent(0)
            {
                Enabled = isEnabled,
                Hidden = (!isEnabled),
                aToolTip = toolTip,
                eImageUrl = imagePath
            };

            string notepadTableName = objectName;
            string notepadIDValue = valueDataKeyField;
            string notepadKeyType = (TypeData.IsTypeString(Referential.Column[Referential.IndexDataKeyField].DataType.value) ? "1" : "0");
            string notepadDisplayValue = Referential.dataRow[Referential.IndexColSQL_KeyField].ToString();
            if (Referential.NotepadSpecified && Referential.Notepad.tablenameSpecified)
            {
                notepadTableName = Referential.Notepad.tablename;
                if (Referential.Notepad.IDSpecified)
                {
                    notepadIDValue = Referential.dataRow[Referential.Notepad.ID].ToString();
                    notepadKeyType = (TypeData.IsTypeString(Referential[Referential.Notepad.ID].DataType.value) ? "1" : "0");
                    notepadDisplayValue = string.Empty;
                }
            }
            // PM 20240604 [XXXXX] Ajout GUID pour gestion des restrictions pour les tools
            //mnu[1].eUrl = JavaScript.GetWindowOpenNotepad(notepadTableName, notepadIDValue, idMenu, notepadDisplayValue, Referential.consultationMode, notepadKeyType, listType);
            mnu[1].eUrl = JavaScript.GetWindowOpenNotepad(notepadTableName, notepadIDValue, idMenu, notepadDisplayValue, Referential.consultationMode, notepadKeyType, listType, this.GUID);
            #endregion NOTEPAD

            #region ATTACHEDDOC
            imagePath = "fas fa-icon fa-paperclip";
            toolTip = Ressource.GetString("btnAttachedDoc");
            isEmpty = true;
            isEnabled = (Referential.AttachedDocSpecified && Referential.AttachedDoc.Value);
            isEnabled &= (Referential.Column[Referential.IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString());
            // to disable: set isEmpty to true            
            if (isEnabled)
                isEmpty = (Referential.dataRow[Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME"] == null || StrFunc.IsEmpty(Referential.dataRow[Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME"].ToString().Trim()));
            if (!isEmpty)
            {
                string displayName = Referential.dataRow[Cst.AliasATTACHEDDOCACTOR + "_" + "DISPLAYNAME"].ToString();
                DateTime dtUpd;
                if (Referential.dataRow[Cst.AliasATTACHEDDOC + "_" + "DTUPD"] == DBNull.Value)
                    dtUpd = DateTime.MinValue;
                else
                    dtUpd = Convert.ToDateTime(Referential.dataRow[Cst.AliasATTACHEDDOC + "_" + "DTUPD"]);
                imagePath += " green";
                toolTip = Ressource.GetString("btnAttachedDoc");
                if (dtUpd != DateTime.MinValue)
                {
                    // FI 20200820 [25468] Date systèmes en UTC 
                    DateTimeTz dateTimeTz = new DateTimeTz(dtUpd, "Etc/UTC");
                    toolTip += Cst.CrLf + RessourceExtended.GetString_LastModifyBy(dateTimeTz, displayName, true);
                }
            }
            mnu[2] = new skmMenu.MenuItemParent(0)
            {
                Enabled = isEnabled,
                Hidden = !isEnabled,
                aToolTip = toolTip,
                eImageUrl = imagePath
            };

            string tableAttachedDoc = Cst.OTCml_TBL.ATTACHEDDOC.ToString();
            if (TypeData.IsTypeString(Referential.Column[Referential.IndexDataKeyField].DataType.value))
                tableAttachedDoc = Cst.OTCml_TBL.ATTACHEDDOCS.ToString();

            // FI 20191002 prise en compte d'une vue éventuelle (cas du Log\FULLTRACKER_L)
            string viewTableAttachedDoc = string.Empty;
            if (Referential.AttachedDoc.viewNameSpecified)
                viewTableAttachedDoc = Referential.AttachedDoc.viewName;

            string attachedDocTableName = Referential.TableName;
            string attachedDocIDValue = valueDataKeyField;
            string attachedDocDisplayValue = Referential.dataRow[Referential.IndexColSQL_KeyField].ToString();
            string attachedDocDescriptionValue = string.Empty;

            if (Referential.ExistsColumnDESCRIPTION && (Referential.dataRow[Referential.IndexColSQL_DESCRIPTION].ToString().Length > 0))
                attachedDocDescriptionValue = Referential.dataRow[Referential.IndexColSQL_DESCRIPTION].ToString();
            if (Referential.AttachedDocSpecified && Referential.AttachedDoc.tablenameSpecified)
            {
                attachedDocTableName = Referential.AttachedDoc.tablename;
                if (Referential.AttachedDoc.IDSpecified)
                {
                    attachedDocIDValue = Referential.dataRow[Referential.AttachedDoc.ID].ToString();
                    attachedDocDisplayValue = string.Empty;
                    attachedDocDescriptionValue = string.Empty;
                }
            }
            // PM 20240604 [XXXXX] Ajout GUID pour gestion des restrictions pour les tools
            //mnu[2].eUrl = JavaScript.GetWindowOpenAttachedDoc(tableAttachedDoc, attachedDocIDValue, attachedDocDisplayValue, attachedDocDescriptionValue, idMenu, attachedDocTableName, viewTableAttachedDoc);
            mnu[2].eUrl = JavaScript.GetWindowOpenAttachedDoc(tableAttachedDoc, attachedDocIDValue, attachedDocDisplayValue, attachedDocDescriptionValue, idMenu, attachedDocTableName, viewTableAttachedDoc, this.GUID);
            #endregion ATTACHEDDOC


            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            Panel pnl = new Panel()
            {
                ID = "tbAction",
                CssClass = this.CSSMode + " " + mainMenuClassName + " skmnu"
            };
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }

        /// <summary>
        /// Ajout d'une CheckBox qui permet d'afficher ou pas les noms des colonnes sur le formulaire.
        /// </summary>
        /// <param name="pTr"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddCheckBox(Panel pPnlParent)
        {
            if (this.Referential.HasDataLightDisplay || SessionTools.IsSessionSysAdmin)
            {
                Panel pnlCheck = new Panel();

                if (this.Referential.HasDataLightDisplay)
                {
                    WCCheckBox2 chk = new WCCheckBox2
                    {
                        ID = Cst.CHK + AdditionalCheckBoxEnum.ShowAllData,
                        Text = Ressource.GetString("Referential" + AdditionalCheckBoxEnum.ShowAllData),
                        AutoPostBack = true,
                        Checked = true,
                        TextAlign = TextAlign.Left
                    };
                    pnlCheck.Controls.Add(chk);
                }

                if (SessionTools.IsSessionSysAdmin)
                {
                    WCCheckBox2 chk = new WCCheckBox2
                    {
                        ID = Cst.CHK + AdditionalCheckBoxEnum.ShowColumnName,
                        Text = Ressource.GetString("Referential" + AdditionalCheckBoxEnum.ShowColumnName),
                        // EG 20230921 [WI711] Referential Page: Display columns name enhancement via jQuery (without PostBack)
                        //AutoPostBack = true,
                        TextAlign = TextAlign.Left
                    };
                    pnlCheck.Controls.Add(chk);
                }
                pPnlParent.Controls.Add(pnlCheck);
            }
        }

        /// <summary>
        /// Ajout d'une toolbar qui permet de consulter une autre ligne du datagrid 
        /// </summary>
        /// <param name="pSuffix"></param>
        /// <param name="pTr"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddToolBarConsult(Panel pPnlParent)
        {
            skmMenu.Menu mnuToolBar = new skmMenu.Menu(4, "mnuConsult", null, null, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(4);

            mnu[0] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-double-left",
                eCommandName = MenuConsultEnum.First.ToString(),
                aToolTip = Ressource.GetString("imgFirstItem")
            };

            mnu[1] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-left",
                eCommandName = MenuConsultEnum.Previous.ToString(),
                aToolTip = Ressource.GetString("imgPreviousItem")
            };

            mnu[2] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-right",
                eCommandName = MenuConsultEnum.Next.ToString(),
                aToolTip = Ressource.GetString("imgNextItem")
            };

            mnu[3] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-double-right",
                eCommandName = MenuConsultEnum.Last.ToString(),
                aToolTip = Ressource.GetString("imgLastItem")
            };

            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnConsult);

            Panel pnl = new Panel()
            {
                ID = "tbConsult",
                CssClass = this.CSSMode + " " + mainMenuClassName + " skmnu"
            };
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);

        }

        //PL 20161124 - RATP 4Eyes - MakingChecking
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddButtonChildReferential(Panel pPnlParent)
        {
            //Add button for acces child referential (e.g.: Book for Actor)
            string eText = Ressource.GetString("btnDetail");

            if (IsExistMenuChild)
            {
                if (!isMenuChildLoaded)
                {
                    SpheresMenu.Menus menus = SessionTools.Menus;
                    skmMenu.MenuItemParent menuRoot = menuDetail[0];
                    CreateChildsMenu(idMenu, ref menuRoot, menus);

                    isMenuChildLoaded = true;
                    MenuChild = menuRoot;
                }
                menuDetail[0] = MenuChild;
                menuDetail[0].Enabled = true;
                menuDetail[0].eText = eText;
                menuDetail[0].eImageUrl = "fas fa-icon fa-project-diagram";
                Panel pnlSkmMenu = new Panel
                {
                    CssClass = this.CSSMode + " " + mainMenuClassName + " skmnu"
                };
                plhMenuDetail = new PlaceHolder
                {
                    ID = "plhMenuDetail"
                };
                pnlSkmMenu.Controls.Add(plhMenuDetail);
                pPnlParent.Controls.Add(pnlSkmMenu);
            }
        }

        #region protected AddLinkButtonSpecific
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        protected void AddLinkButtonSpecific(Panel pPnlParent, ReferentialButton pButton, int pIndex)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = "btnSpec" + pIndex,
                CssClass = String.Format("fa-icon {0} {1}",
                pButton.bkgcolorSpecified ? "bkg" + pButton.bkgcolor : string.Empty,
                pButton.colorSpecified ? pButton.color : string.Empty),
                CausesValidation = false,
                Visible = true
            };
            if (pButton.faclassSpecified || pButton.colorSpecified)
                btn.Text = String.Format(@"<i class='{0} {1}'></i> ", pButton.faclass, pButton.color);
            btn.Text += Ressource.GetString(pButton.title, true);

            if (pButton.tooltipSpecified)
                btn.Pty.TooltipContent = Ressource.GetString(pButton.tooltip, true);

            string subTitle = Ressource.GetMenu_Shortname2(idMenu, idMenu);
            StringBuilder url = new StringBuilder(pButton.url);
            if (url.ToString().IndexOf("&FK=") < 0)
            {
                //NB: Dans certains cas, on override l'usage de la FK (ex. ACTORAMOUNT_H)
                url.AppendFormat("&FK={0}", valueDataKeyField);
            }
            else if (url.ToString().IndexOf("&FK=%%COLUMN_VALUE%%") > 0)
            {
                //NB: Usage d'une FK autre que la FK standard (ex. ACTORAMOUNT_H, IOTRACK)
                //Utilisation de la chaîne: &FK=%%COLUMN_VALUE%%<fkColumnName>%% où <fkColumnName> représente le nom de la colonne contenant 
                //la donnée faisant office de AK, utile pour la FK du référentiel enfant appelé (ex. &FK=%%COLUMN_VALUE%%IDPROCESS_L%%)
                int posStart = url.ToString().IndexOf("&FK=%%COLUMN_VALUE%%") + "&FK=%%COLUMN_VALUE%%".Length;
                int posEnd = url.ToString().IndexOf("%%", posStart);
                string fkColumnName = url.ToString().Substring(posStart, posEnd - posStart);
                ReferentialsReferentialColumn rrc = Referential[fkColumnName];
                if (rrc == null)
                {
                    btn.Enabled = false;
                }
                else
                {
                    url = new StringBuilder(pButton.url.Replace("&FK=%%COLUMN_VALUE%%" + fkColumnName + "%%", "&FK=" + ((TextBox)rrc.ControlMain).Text));
                }
            }
            url.AppendFormat("&SubTitle={0}",
                    HttpUtility.UrlEncode(subTitle + ": " + Referential.dataRow[Referential.IndexColSQL_KeyField].ToString(), System.Text.Encoding.UTF8));
            btn.OnClientClick = JavaScript.GetWindowOpen(url.ToString()) + "return false;";
            pPnlParent.Controls.Add(btn);
        }

        #endregion
        #region protected AddLinkButtonValidate
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddLinkButtonValidate(Panel pPnlparent)
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnValidate", "fa fa-save", true);
            btn.Click += new EventHandler(this.OnValidateClick);
            pPnlparent.Controls.Add(btn);
        }
        #endregion
        #region protected AddLinkButtonDuplicate
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210128 [XXXXX] Pas de Click si disabled
        protected void AddLinkButtonDuplicate(Panel pPnlparent)
        {
            bool isEnabled = Referential.Modify && Referential.Create &&
                (SessionTools.License.IsLicFunctionalityAuthorised_Add(Referential.TableName, true));

            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnDuplicate", "fa fa-copy", true);
            btn.Enabled = isEnabled;
            if (isEnabled)
            {
                btn.OnClientClick = "return confirm(" + JavaScript.JSString(Ressource.GetString("Msg_Duplicate")) + ");";
                btn.Click += new EventHandler(this.OnDuplicateClick);
            }
            pPnlparent.Controls.Add(btn);
        }
        #endregion
        #region protected AddLinkButtonRecord
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddLinkButtonRecord(Panel pPnlparent, string pBtnName)
        {
            bool isEnabled = ((Referential.isNewRecord && Referential.Create) || Referential.Modify);

            if ((isEnabled) && (Referential.isNewRecord && Referential.Create))
            {
                isEnabled = (SessionTools.License.IsLicFunctionalityAuthorised_Add(Referential.TableName, true));
                if (false == isEnabled)
                {
                    string msg = Ressource.GetString("Msg_LicFunctionality");
                    if (!StrFunc.ContainsIn(pageFooterLeft, msg))
                        pageFooterLeft += msg;
                    this.MsgForAlertImmediate = msg;
                }
            }

            bool RecordAuthorized = true;
            if (Referential.ExistsColumnROWATTRIBUT)
            {
                if (Referential.dataRow[Referential.IndexColSQL_ROWATTRIBUT].ToString() == Cst.RowAttribut_System)
                    RecordAuthorized = false;
            }

            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction(pBtnName, "fa fa-save", true);
            btn.Enabled = isEnabled;
            btn.CommandName = pBtnName;
            btn.Command += new CommandEventHandler(this.OnRecordClick);
            if (!RecordAuthorized)
                btn.Attributes.Add("onclick", "alert('" + Ressource.GetStringForJS("Msg_DataSystem") + "');return false;");
            pPnlparent.Controls.Add(btn);

            //20090923 FI Add le bouton Record est celui actionnée si la touche entrée est activée
            if (isEnabled)
            {
                AddInputHiddenDeFaultControlOnEnter();
                HiddenFieldDeFaultControlOnEnter.Value = btn.ClientID;
            }
        }
        #endregion
        #region protected AddLinkButtonCancel
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddLinkButtonCancel(Panel pPnlparent)
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonCancel(false);
            pPnlparent.Controls.Add(btn);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSuffix"></param>
        /// <param name="pTr"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddLinkButtonRemove(Panel pPnlParent)
        {
            string js, msg = string.Empty;
            bool RemoveAuthorized = true;
            if (Referential.ExistsColumnROWATTRIBUT)
            {
                switch (Referential.dataRow[Referential.IndexColSQL_ROWATTRIBUT].ToString())
                {
                    case Cst.RowAttribut_System:
                        RemoveAuthorized = false;
                        msg = "Msg_DataSystem";
                        break;
                    case Cst.RowAttribut_Protected:
                        RemoveAuthorized = false;
                        msg = "Msg_DataProtected";
                        break;
                }
            }
            if (RemoveAuthorized)
            {
                js = Ressource.GetString("Msg_Remove") + Cst.CrLf + Cst.CrLf;
                js += Ressource.GetStringForJS(idMenu) + ": " + Referential.dataRow[Referential.IndexColSQL_KeyField].ToString();
                if (Referential.ExistsColumnDESCRIPTION && StrFunc.IsFilled(Referential.dataRow[Referential.IndexColSQL_DESCRIPTION].ToString()))
                    js += " - " + Referential.dataRow[Referential.IndexColSQL_DESCRIPTION].ToString();
                js = "return confirm(" + JavaScript.JSString(js) + ");";
            }
            else
                js = "alert('" + Ressource.GetStringForJS(msg) + "');return false;";

            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnRemove", "fa fa-trash-alt", true);
            btn.Enabled = Referential.Remove;
            btn.OnClientClick = js;
            btn.Click += new EventHandler(this.OnRemoveClick);
            pPnlParent.Controls.Add(btn);
        }

        #region protected AddLinkButtonIRQ
        /// <summary>
        /// Bouton d'interruption de traitement
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200724 [XXXXX] Correction Bouton IRQ (CauseValidation = false)
        protected void AddLinkButtonIRQ(Panel pPnlParent)
        {

            if ((null != Referential["READYSTATE"]) &&
                (null != Referential["SYSNUMBER"]) &&
                (null != Referential["GROUPTRACKER"]))
            {

                bool isManaged = IRQTools.IsIRQManaged(Referential.dataRow["GROUPTRACKER"].ToString(), Convert.ToInt32(Referential.dataRow["SYSNUMBER"]));
                isManaged &= (false == ProcessStateTools.IsReadyStateTerminated(Referential.dataRow["READYSTATE"].ToString()));
                if (isManaged)
                {
                    WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnIRQProcess", string.Empty, "far fa-icon fa-stop-circle", true);
                    btn.Enabled = true;
                    btn.CausesValidation = false;
                    btn.Click += new EventHandler(this.OnIRQRequestClick);

                    int idA = SessionTools.User.IdA;
                    ReferentialsReferentialColumn columnUser = Referential["IDAINS"];
                    if (null != columnUser)
                        idA = Convert.ToInt32(Referential.dataRow["IDAINS"]);

                    // Un user Admin peut faire une demande
                    // un autre user doit avoir les droits
                    // Private = il ne peut faire une demande que sur les traitement qu'il a initié
                    // Public  = il peut faire également une demande pour les traitements initiés par d'autres utilisateur
                    bool isIRQAuthorized = SessionTools.User.IsSessionSysAdmin;
                    if (false == isIRQAuthorized)
                    {
                        RestrictionPermission restrictPermission = new RestrictionPermission(idMenu, SessionTools.User);
                        restrictPermission.Initialize(CSTools.SetCacheOn(SessionTools.CS));
                        isIRQAuthorized = (SessionTools.User.IdA == idA) ?
                                (restrictPermission.IsIRQPrivate || restrictPermission.IsIRQPublic) : restrictPermission.IsIRQPublic;
                    }

                    string js = string.Empty;
                    string msg = string.Empty;

                    if (isIRQAuthorized)
                    {
                        js = Ressource.GetString2("Msg_IRQ", Referential.dataRow["SHORTMESSAGE"].ToString());
                        js = "return confirm(" + JavaScript.JSString(js) + ");";
                    }
                    else
                    {
                        js = "alert('" + Ressource.GetStringForJS("Msg_IRQNotAuthorized") + "');return false;";
                    }
                    btn.Attributes.Add("onclick", js);

                    Panel pnl = new Panel
                    {
                        ID = "divtoolbarIRQ"
                    };
                    pnl.Controls.Add(btn);
                    pPnlParent.Controls.Add(pnl);
                }

            }




        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnValidateClick(object sender, EventArgs e)
        {
            ReturnAndClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnDuplicateClick(object sender, EventArgs e)
        {
            //Reload de la page afin de "simuler" une création (&N=1)
            string url = this.Request.RawUrl;
            url = url.Replace("&N=0", "&N=1");          //N:New
            url = url.Replace("&OCR=1", string.Empty);  //OCR:OpenerCallReload
            url += "&DPC=1";                            //DPC:DuPliCate
            Server.Transfer(url);
        }

        /// <summary>
        /// Sauvegarde du formulaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20130425 [] Refactoring Spheres® rentrait tjs le RollbackTran après un commit
        /// => Cela provoquait une exception qui était catchée bien heureusement 
        /// FI 20141021 [20350] Modify 
        protected void OnRecordClick(object sender, CommandEventArgs e)
        {

            if (IsRecordValid(out string warningMsg)) // FI 20201016 [XXXXX] Call IsRecordValid
            {
                IDbTransaction dbTransaction = null;
                try
                {
                    if (Referential.isNewRecord && Referential.ExistsColumnIDENTITYWithSource)
                    {
                        dbTransaction = DataHelper.BeginTran(SessionTools.CS);
                    }

                    ReferentialTools.UpdateDataRowFromControls2(this, Referential, dbTransaction);

                    Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
                    switch (e.CommandName)
                    {
                        case "btnApply":
                            ret = Save(dbTransaction, false, sender);
                            break;
                        // JN 20190510 - plsSelfClose = false pour permettre utilisation de la boite de dialogue Javascript au click sur "Enregistrer"
                        case "btnRecord2":
                            if (Referential.TableName == Cst.OTCml_TBL.MATURITYRULE.ToString())
                                ret = Save(dbTransaction, false, sender);
                            else
                                goto default;
                            break;
                        default:
                            ret = Save(dbTransaction, true);
                            break;
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
                            if (BoolFunc.IsTrue(Request.QueryString["DPC"])) //Enregistrement d'un nouvel item à partir d'un enregistrement existant
                                actionType = RequestTrackProcessEnum.New;
                            else
                                actionType = RequestTrackProcessEnum.Modify;
                        }

                        // FI 20141021 [20350] call SetRequestTrackBuilderIemAction
                        SetRequestTrackBuilderItemProcess(actionType, Referential.dataRow);
                    }
                }
                catch (Exception)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
            //
            if (StrFunc.IsFilled(warningMsg))
            {
                JavaScript.DialogStartUpImmediate(this, warningMsg, false, ProcessStateTools.StatusWarningEnum);
            }
        }
        /// <summary>
        ///  Retourne true, si tous les contrôles  (côte serveur) sont valides  
        /// </summary>
        /// <param name="warningMsg">Eventuel Message de warning en cas de retour négatif</param>
        /// <returns></returns>
        /// FI 20201016 [XXXXX] Add Method
        private Boolean IsRecordValid(out string warningMsg)
        {
            warningMsg = string.Empty;

            bool isOk = this.Page.IsValid;

            if (isOk && Referential.isNewRecord) // Si nouvel enregistrement, Spheres® pré-propose certaines DDL à Empty alors qu'elles sont obligatoires (voir ReferentialTools.LoadDDL). Ici on vérifie que ces DLL ont bien été renseignées par l'utilisateur  
            {
                IEnumerable<ReferentialsReferentialColumn> rrc = from item in Referential.Column.Where(x => (false == x.IsHide) && (x.IsNotVirtualColumn) &&
                                                                 (x.IsMandatory) &&
                                                                 (ReferentialTools.IsDataForDDL(x) && (x.ControlMain as DropDownList) != null) && (StrFunc.IsEmpty(((DropDownList)x.ControlMain).SelectedValue)))
                                                                 select item;
                isOk = (rrc.Count() == 0);
                if (false == isOk)
                {
                    bool isAddColumn = (from item in rrc
                                        select (WCTooltipLabel)item.ControlLabel).GroupBy(y => y.Text).Count() < rrc.Count();

                    foreach (ReferentialsReferentialColumn item in rrc)
                    {
                        string infoCol = isAddColumn ? Cst.Space + $"({item.ColumnName})" : string.Empty;
                        if (StrFunc.IsFilled(warningMsg))
                            warningMsg += Cst.CrLf;
                        warningMsg += $"{((WCTooltipLabel)item.ControlLabel).Text}{infoCol} : {Ressource.GetString("ISMANDATORY")}";
                    }
                }
            }
            return isOk;
        }


        /// <summary>
        /// Construction du message IRQ et postage pour activation à NORMMSGFACTORY
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        protected void OnIRQRequestClick(object sender, EventArgs e)
        {
            IRQRequester irqRequester = new IRQRequester(SessionTools.CS, Convert.ToInt32(Referential.dataRow["IDTRK_L"]));
            if (Cst.ErrLevel.SUCCESS == irqRequester.ConstructNormMsgFactoryMessage())
            {
                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    process = Cst.ProcessTypeEnum.IRQ,
                    connectionString = SessionTools.CS,
                    Session = SessionTools.AppSession,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = Cst.ProcessTypeEnum.IRQ,
                        caller = irqRequester.ProcessToInterrupt,

                        info = TrackerAttributes.BuildInfo(irqRequester.NormMsgFactoryMQueue)
                    },
                    mQueue = new MQueueBase[1] { irqRequester.NormMsgFactoryMQueue }
                };
                taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);

                int idTRK_L = 0;
                var (isOk, errMsg) = MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);

                if (isOk)
                {
                    MsgForAlertImmediate = Ressource.GetString2("Msg_PROCESS_GENERATE_IRQ",
                        LogTools.IdentifierAndId(taskInfo.process.ToString(), idTRK_L),
                        LogTools.IdentifierAndId(Ressource.GetString(irqRequester.ProcessToInterrupt), irqRequester.TrackerIdProcess));
                }
                else
                {
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                    MsgForAlertImmediate = Ressource.GetString2("Msg_ERROR_GENERATE_IRQ",
                        LogTools.IdentifierAndId(taskInfo.process.ToString(), idTRK_L),
                        LogTools.IdentifierAndId(Ressource.GetString(irqRequester.ProcessToInterrupt), irqRequester.TrackerIdProcess),
                        errMsg);
                }

                CloseAfterAlertImmediate = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20141021 [20350] Modify
        /// EG 20210630 [25500] Gestion de la suppression d'un item du flux RSS         
        protected void OnRemoveClick(object sender, EventArgs e)
        {

            // FI 20190718 [XXXXX] Call CheckBeforDelete
            Boolean isOk = ReferentialTools.CheckBeforDelete(this, Referential.TableName, Convert.ToInt32(valueDataKeyField));

            if (isOk)
            {
                // FI 20141021 [20350] Sauvegarde des valeurs précédentes
                DataRow savDataRow = Referential.CoptyDataRow();

                //Delete principal
                Referential.dataRow.Delete();

                if (Referential.TableName.StartsWith("SYNDICATION"))
                    ReferentialTools.UpdateSyndicationFeed();

                //Delete EXTLID si existant
                if (Referential.drExternal != null)
                {
                    for (int i = 0; i < Referential.drExternal.GetLength(0); i++)
                    {
                        //Warning: Cas particulier des external de type Role ou Item, ces données sont supprimées par une IR de type cascade, 
                        //         il ne faut donc pas les supprimer via ADO sous peine de s'exposer à une erreur (cf TRIM 16520)
                        ReferentialsReferentialColumn rrc = Referential[i, "ExternalFieldID"];
                        if ((rrc != null) && (!rrc.IsRole) && (!rrc.IsItem))
                            Referential.drExternal[i].Delete();
                    }
                }

                Cst.ErrLevel errLevel = Save(null, true);

                // FI 20141021 [20350] call SetRequestTrackBuilderItemProcess 
                if (errLevel == Cst.ErrLevel.SUCCESS)
                    SetRequestTrackBuilderItemProcess(RequestTrackProcessEnum.Remove, savDataRow);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnXmlClick(object sender, EventArgs e)
        {

            string clientId = ((Control)sender).ID;  // Que sur table Main
            string col = clientId;
            //
            if (StrFunc.IsFilled(col))
            {
                col = StrFunc.PutOffSuffixNumeric(col);
                if (3 <= col.Length)
                    col = col.Substring(3);
            }
            //
            int i = Referential.GetIndexColSQL(col);
            if (i > -1)
            {
                string strXml = Referential.dataRow[i].ToString();
                if (StrFunc.IsFilled(strXml))
                    DisplayXml("Referential_XML", "Referential_" + Referential.TableName, Referential.dataRow[i].ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnConsult(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            //
            DataSet ds = DatasetGrid;
            //
            //si dataset socké dans le cache n'existe plus Spheres® affiche un message
            if (null == ds)
                JavaScript.DialogStartUpImmediate(this, Ressource.GetString("Msg_DataSourceNotFound"), false, ProcessStateTools.StatusWarningEnum);
            //
            if (null != ds)
            {
                #region recherche de la ligne consultée dans le Dataset
                DataColumn columnDataKeyField = ds.Tables[0].Columns[Referential.IndexColSQL_DataKeyField];
                string select = dataKeyField + "=";
                if (columnDataKeyField.DataType.Equals(typeof(String)))
                    select += "'" + valueDataKeyField + "'";
                else
                    select += valueDataKeyField;
                //
                int index = -1;
                int newIndex = 0;
                //tblResult contient les lignes triées 
                //(utilisation de la propriété DefaultView qui contient les données triées)
                DataTable tblResult = ds.Tables[0].DefaultView.ToTable();
                DataRow[] row = tblResult.Select(select);
                if (ArrFunc.Count(row) == 1)
                    index = tblResult.Rows.IndexOf(row[0]);
                #endregion
                //
                bool isFindNext = false;
                bool isFindPrevious = false;
                MenuConsultEnum menuConsult = (MenuConsultEnum)Enum.Parse(typeof(MenuConsultEnum), e.CommandName);
                switch (menuConsult)
                {
                    case MenuConsultEnum.First:
                        newIndex = 0;
                        isFindNext = true;
                        break;
                    case MenuConsultEnum.Previous:
                        newIndex = Math.Max(0, index - 1);
                        isFindPrevious = true;
                        break;
                    case MenuConsultEnum.Next:
                        newIndex = Math.Min(index + 1, tblResult.Rows.Count - 1);
                        isFindNext = true;
                        break;
                    case MenuConsultEnum.Last:
                        newIndex = tblResult.Rows.Count - 1;
                        isFindPrevious = true;
                        break;
                }
                //
                if (newIndex != index)
                {
                    string newKeyFieldValue = tblResult.Rows[newIndex][Referential.IndexColSQL_DataKeyField].ToString();
                    bool isNewRowNavigable = StrFunc.IsFilled(newKeyFieldValue);
                    //
                    if (false == isNewRowNavigable)
                    {
                        //Recherche de la prochaine ligne valide
                        if (isFindNext)
                        {
                            while ((false == isNewRowNavigable) && (newIndex <= tblResult.Rows.Count - 1))
                            {
                                newIndex = Math.Min(newIndex + 1, tblResult.Rows.Count - 1);
                                newKeyFieldValue = tblResult.Rows[newIndex][Referential.IndexColSQL_DataKeyField].ToString();
                                isNewRowNavigable = StrFunc.IsFilled(newKeyFieldValue);
                            }
                        }
                        //Recherche de la précédente ligne valide
                        if (isFindPrevious)
                        {
                            while ((false == isNewRowNavigable) && (newIndex >= 0))
                            {
                                newIndex = Math.Max(0, newIndex - 1);
                                newKeyFieldValue = tblResult.Rows[newIndex][Referential.IndexColSQL_DataKeyField].ToString();
                                isNewRowNavigable = StrFunc.IsFilled(newKeyFieldValue);
                            }
                        }
                    }
                    //
                    if (isNewRowNavigable)
                    {
                        DataRow newRow = tblResult.Rows[newIndex];
                        newKeyFieldValue = newRow[Referential.IndexColSQL_DataKeyField].ToString();
                        //
                        string url = Request.RawUrl;
                        //FI 20111017 UrlEncode sur  valueDataKeyField et newKeyFieldValue
                        url = url.Replace("&PKV=" + HttpUtility.UrlEncode(valueDataKeyField), "&PKV=" + HttpUtility.UrlEncode(newKeyFieldValue));
                        //
                        Response.Redirect(url, false);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnByRow"></param>
        /// <param name="opTotalColumnByRow"></param>
        /// <param name="opdefaultInputWidth"></param>
        /// <param name="opdefaultLabelWidth"></param>
        private void InitVariables(int pColumnByRow, out int opTotalColumnByRow, out string opdefaultInputWidth, out string opdefaultLabelWidth)
        {
            if (pColumnByRow <= 0)
                pColumnByRow = Referential.ColumnByRow;
            if (pColumnByRow == 0)
                pColumnByRow = 2;

            opTotalColumnByRow = pColumnByRow * 4;//*4 --> label + textbox + empty zone before label and textbox
            if (1 < pColumnByRow)
            {
                opdefaultLabelWidth = @" Width=""" + Convert.ToString((100 * 1 / 8) / (pColumnByRow)) + @"%""";
                opdefaultInputWidth = @" Width=""" + Convert.ToString((100 * 7 / 8) / (pColumnByRow)) + @"%""";
            }
            else
            {
                opdefaultLabelWidth = "";
                opdefaultInputWidth = @" width=""99%""";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <param name="opMenuRoot"></param>
        /// <param name="pSessionToolsMenus"></param>
        /// EG 20150412 [20513] BANCAPERTA
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        private void CreateChildsMenu(string pIdMenu, ref skmMenu.MenuItemParent opMenuRoot, SpheresMenu.Menus pSessionToolsMenus)
        {
            //PL 20120606 Refactoring - Use TranslateMenu()
            pIdMenu = SessionTools.Menus.TranslateMenu(pIdMenu);

            //Debug.WriteLine("================================================");
            //Debug.WriteLine("Menu: "+pIdMenu);
            //Debug.WriteLine("================================================");

            #region Décompte du nombre de menus enfants

            List<SpheresMenu.Menu> alMenuChild = new List<SpheresMenu.Menu>();

            bool isAdd = false;
            //PL 20140409 Add and use listOfMenuDetail (Pour éviter, faute de mieux, l'affichage de menu en double)
            string listOfMenuDetail = ";";
            // FI 20220503 [XXXXX] Usage de Linq 
            //            for (int i = 0; i < pSessionToolsMenus.Count; i++)
            foreach (SpheresMenu.Menu item in pSessionToolsMenus.ToArray().Where(x => x.IdMenu_Parent == pIdMenu && x.IsEnabled))
            {
                // FI 20220503 [XXXXX] Clonage nécessaire puisque les propriétés Url sont modifiées
                SpheresMenu.Menu menu  = (SpheresMenu.Menu)item.Clone();
                if ((listOfMenuDetail.IndexOf(";" + menu.IdMenu + ";") < 0))
                {
                    listOfMenuDetail += menu.IdMenu + ";";

                    //FI 20120123 Mode FormViewer si ReadOnly
                    if (this.consultationMode == Cst.ConsultationMode.ReadOnly)
                        menu.Url = ReplaceInputMode(menu.Url, Cst.DataGridMode.FormViewer);

                    isAdd = true;

                    #region Gestion "spécifique" des menus enfants "conditionnés"
                    // NB: Certains menus enfants ont vocation à être visible uniquement sur certains items du référentiel parent.  
                    //     On traite de ces cas en "dur" ci-dessous. Pour cela, les items du référentiel parent doivent exposer la donnée discriminente.
                    //     Ex.: Depuis le référentiel INSTRUMENT, seul les produits ETD ont vocation à mettre à disposition le menu enfant "Contrats dérivés".
                    //          Pour cela, on regarde s'il s'agit du menu enfant "Menu.DrvContract" et si oui on le rend disonible uniquement si le produit se termine par "exchangetradedderivative".
                    if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.DrvContract))
                    {
                        if (!this.Referential.dataRow["PRODUCT_IDENTIFIER"].ToString().ToLower().EndsWith("exchangetradedderivative"))
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.IOTRACK))
                    {
                        if (this.Referential.dataRow["PROCESS"].ToString() != Cst.ProcessTypeEnum.IO.ToString())
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.MARGINTRACK))
                    {
                        if (this.Referential.dataRow["PROCESS"].ToString() != Cst.ProcessTypeEnum.RISKPERFORMANCE.ToString())
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.InputDebtSec))
                    {
                        if (!this.Referential.dataRow["PRODUCT_IDENTIFIER"].ToString().ToLower().EndsWith("debtsecurity"))
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.AssetEnv))
                    {
                        // EG 20150412 [20513] BANCAPERTA Add Cst.ProductFamily_BO
                        string gProduct = this.Referential.dataRow["GPRODUCT"].ToString().ToUpper();
                        string family = this.Referential.dataRow["FAMILY"].ToString().ToUpper();
                        if ((gProduct != Cst.ProductGProduct_SEC) && (family != Cst.ProductFamily_RTS) && (family != Cst.ProductFamily_BO))
                        //if (this.referential.dataRow["GPRODUCT"].ToString().ToUpper() != "SEC")
                        {
                            //Affichage du menu "AssetEnv" (Environement d'actif) uniquement sur les instruments issus du grp; de produit "SEC"
                            //NB: A enrichir le cas échéant si on crée un jour un instrument ExchangeTradedFund, afin d'y autoriser ce menu. (PL 20130116)
                            isAdd = false;
                        }
                        else
                        {
                            menu = AddFAMILYAndGPRODUCTToSpecificMenu(menu, Referential.dataRow);
                        }
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.AssetEquityBasketConstituent))
                    {
                        isAdd = (this.Referential.dataRow["ASSETCLASS"].ToString() == "FB");
                    }
                    else if ((pIdMenu == IdMenu.GetIdMenu(IdMenu.Menu.IOTRACK) &&
                                (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.IOTRACKCOMPARE))))
                    {
                        string ioTaskElementType = string.Empty;
                        if ((null == this.Referential.dataRow["IOTASKDET_TYPE"]))
                            throw new Exception("Column IOTASKDET_TYPE doesn't exist");
                        ioTaskElementType = this.Referential.dataRow["IOTASKDET_TYPE"].ToString();
                        //
                        isAdd = (ioTaskElementType.ToLower() == Cst.IOElementType.COMPARE.ToString().ToLower());
                        //
                        if (isAdd)
                            menu = GetSpecificMenuForIOTRACKCompare(menu, Referential.dataRow);
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.ImCboeMarket))
                    {
                        // RD 20191218 [25056] Add INITIALMARGINMETHETDOPT
                        if ((this.Referential.dataRow["INITIALMARGINMETH"].ToString().ToUpper() != "CBOE_MARGIN") &&
                            (this.Referential.dataRow["INITIALMARGINMETHETDOPT"].ToString().ToUpper() != "CBOE_MARGIN"))
                            // CC 20130116 Affichage du menu "Paramètres pour le déposit "CBOE Margin"", accessible depuis le menu Détail 
                            // du référentiel Marchés, uniquement si la chambre de compensation rattachée au marché dispose de la méthode 
                            // de calcul de déposit "CBOE Margin" paramétrée.                    
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.ImCboeContract))
                    {
                        if (this.Referential.dataRow["INITIALMARGINMETH"].ToString().ToUpper() != "CBOE_MARGIN")
                            // CC 20130116 Affichage du menu "Paramètres pour le déposit "CBOE Margin"", accessible depuis le menu Détail 
                            // du référentiel Contrats Dérivés, uniquement si le contrat est rattaché à un marché dont la chambre de 
                            // compensation dispose de la méthode de calcul de déposit "CBOE Margin" paramétrée.                    
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.ImTimsIdemContract))
                    {
                        if (this.Referential.dataRow["INITIALMARGINMETH"].ToString().ToUpper() != "TIMS_IDEM")
                            // CC 20130402 Affichage du menu "Paramètres pour le déposit "TIMS IDEM"", accessible depuis le menu Détail 
                            // du référentiel Contrats Dérivés, uniquement si le contrat est rattaché à un marché dont la chambre de 
                            // compensation dispose de la méthode de calcul de déposit "TIMS IDEM" paramétrée.                    
                            isAdd = false;
                    }
                    else if (menu.IdMenu == IdMenu.GetIdMenu(IdMenu.Menu.ImIMSMCESMCommodityContract))
                    {
                        if (this.Referential.dataRow["INITIALMARGINMETH"].ToString().ToUpper() != "IMSM")
                        {
                            // PM 20231129 [XXXXX][WI759] Affichage du menu "Paramètres pour le déposit "IMSM", accessible depuis le menu Détail 
                            // du référentiel Commodity Contracts, uniquement si le contrat est rattaché à un marché dont la chambre de 
                            // compensation dispose de la méthode de calcul de déposit "IMSM" paramétrée.                    
                            isAdd = false;
                        }
                    }
                    #endregion

                    if (isAdd)
                    {
                        alMenuChild.Add(menu);
                    }
                }
            }
            //Debug.WriteLine("Menus enfants: "+nbChildMenu.ToString());
            //Debug.WriteLine("================================================");
            #endregion
            opMenuRoot = new skmMenu.MenuItemParent(alMenuChild.Count);

            #region Affecting childs of current menu
            int current = 0;
            for (int i = 0; i < alMenuChild.Count; i++)
            {
                SpheresMenu.Menu menu = alMenuChild[i];

                #region Creation du menu enfant
                string label = Ressource.GetMenu_Shortname(menu.IdMenu, menu.Displayname);

                //20051019 PL Replace de la ligne suivante
                //if (pSessionToolsMenus.IsParent(menu.IdMenu))
                if ((!menu.IsAction) && (pSessionToolsMenus.IsParent(menu.IdMenu)))
                {
                    skmMenu.MenuItemParent menuRoot = opMenuRoot[current];
                    CreateChildsMenu(menu.IdMenu, ref menuRoot, pSessionToolsMenus);
                    opMenuRoot[current] = menuRoot;
                }

                bool isEnabled = (!Referential.isNewRecord);
                opMenuRoot[current].Enabled = isEnabled;
                opMenuRoot[current].eText = label;
                if (menu.IsAction)
                {
                    // FI 20220503 [XXXXX] Appel à GetURLMenuAction(menu)
                    opMenuRoot[current].eUrl = JavaScript.GetWindowOpen(GetURLMenuAction(menu), Cst.WindowOpenStyle.EfsML_FormReferential);
                }
                else
                {
                    opMenuRoot[current].eUrl = string.Empty;
                }
                opMenuRoot[current].eLayout = skmMenu.MenuLayout.Vertical.ToString();
                #endregion Creation du menu enfant

                current++;

            }
            #endregion
        }
        /// <summary>
        ///
        /// <summary>
        /// EG 20100428 Gestion alignement des colonnes sans ressource
        /// Bidouille pour aligner les colonnes liées (sans ressource) par sauvegarde de compteur des colonnes (columnRemaining)
        /// restant à traiter avant décrémentation (dans la variable : lastColumnRemaining) 
        /// Principe : 
        /// Si la colonne en cours de traitement est une colonne liée (Middle ou LastControlLinked) alors le compteur columnRemaining
        /// est réinitialisé avec lastColumnRemaining (les colonnes liées sont ainsi stockées dans le même contrôle sans rupture de ligne)
        /// </summary>
        /// EG 20170918 [23342] suppression WCImgCalendar
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210222 [XXXXX] Suppression inscription function SetColor (présent dans PageBase.js)
        // EG 20210304 [XXXXX] Relooking Referentiels  
        // EG 20210304 [XXXXX] Relooking Referentiels - Gestion ShowAllData via Css
        // EG 20210304 [XXXXX] Relooking Referentiels - Gestion Ressource sur Panel overflow
        // EG 20210304 [XXXXX] Relooking Referentiels - Pas de Width par défaut sur les colonnes ExternalTableName
        // EG 20210304 [XXXXX] Relooking Referentiels - On insère une table cell que si présence d'un labelMessage
        // EG 20210304 [XXXXX] Relooking Referentiels - Gestion du startdisplay
        private void WriteControls()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + mainMenuClassName };

            #region Declare
            int columnRemaining = 0;
            int lastColumnRemaining = 0;
            int totalBlockByRow = 0;
            int blockRemaining = 0;
            int indexColSQL = -1;

            bool isVisible = false;
            bool isNewTR = false;
            bool isCurrentHR_Visible = true;
            int idTbl = 0;
            ArrayList alPanel = new ArrayList();
            ArrayList alTable = new ArrayList();
            ArrayList alTableRow = new ArrayList();
            ArrayList alTableCell = new ArrayList();
            WCTogglePanel parentPanel = null;
            Panel currentPanel = null;
            Table currentTable = null;
            TableRow currentTableRow = null;
            TableCell currentTableCell = null;

            ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn();

            #endregion Declare

            InitVariables(Referential.ColumnByRow, out int totalColumnByRow, out string defaultInputWidth, out string defaultLabelWidth);

            #region MakingChecking
            //PL 20161124 - RATP 4Eyes - MakingChecking
            bool isMakingChecking_ActionChecking = false;
            if (this.Referential.ExistsMakingChecking)
            {
                if (!Referential.isNewRecord)
                {
                    DataRow currentRow = Referential.dataRow;
                    if (BoolFunc.IsFalse(currentRow["ISCHK"]))
                    {
                        // Record actuellement UNCHECKED --> Making ou Checking
                        try
                        {
                            int idaLast = Convert.ToInt32(currentRow["IDAINS"]);
                            if (currentRow["IDAUPD"] != Convert.DBNull)
                                idaLast = Convert.ToInt32(currentRow["IDAUPD"]);
                            if (idaLast != SessionTools.Collaborator_IDA)
                            {
                                // Utilisateur courant différent du dernier utilisateur ayant modifié les données --> Cheking
                                isMakingChecking_ActionChecking = true;
                            }
                        }
                        catch { }
                    }
                }
            }
            #endregion MakingChecking

            WCTogglePanel pnlOverflow = null;

            for (int indexEltXML = 0; indexEltXML < Referential.Column.Length; indexEltXML++)
            {
                //PL 20161124 - RATP 4Eyes - MakingChecking
                //if (isMakingChecking_ActionChecking)
                //{
                //    rrc.IsUpdatable = new ReferentialsReferentialColumnIsUpdatable();
                //    rrc.IsUpdatableSpecified = true;
                //    rrc.IsUpdatable.Value = false;
                //}

                // EG 20100428 Gestion alignement des colonnes sans ressource
                if (rrc.IsLastControlLinked && (0 == lastColumnRemaining))
                {
                    columnRemaining = lastColumnRemaining;
                }

                #region Step 1: Initialize
                rrc = Referential.Column[indexEltXML];

                #region Gestion alignement des colonnes sans ressource
                if (rrc.IsMiddleControlLinked || rrc.IsLastControlLinked)
                {
                    if (0 == columnRemaining)
                    {
                        columnRemaining = lastColumnRemaining;
                        lastColumnRemaining = 0;
                    }
                }
                #endregion Gestion alignement des colonnes sans ressource

                if (Referential.isNewRecord && rrc.IsExternal
                    && (false == (Referential.IsForm && Referential.IndexKeyField != -1)))
                {
                    rrc.IsHide = true;
                    rrc.IsHideSpecified = true;
                    rrc.html_BLOCK = null;
                }

                if (rrc.Ressource == null)
                {
                    rrc.Ressource = string.Empty;
                    rrc.RessourceSpecified = true;
                }

                indexColSQL++;
                if (rrc.ExistsRelation)
                {
                    indexColSQL++;
                }

                //Permettre la visualisation des Identity (ie: ISSI)
                isVisible = (!rrc.IsHide) && (!(Referential.isNewRecord && rrc.IsIdentity.Value));
                //PL 20130125 Test in progress...
                if (Referential.ItemsSpecified && Referential.Items.mode == "SO" && rrc.IsItem && (((CheckBox)rrc.ControlMain).Checked == false))
                {
                    rrc.IsHideOnLightDisplay = true;
                }
                //PL 20130114 Test in progress... (Hiding some data)
                bool isLightDisplay_And_Hide = (rrc.IsHideOnLightDisplay && (!IsShowAllData));
                // EG 20210304 [XXXXX] Relooking Referentiels - Gestion ShowAllData via Css
                string cssClassDisplayLight = "dsplight";

                isNewTR = (columnRemaining == 0) && (false == rrc.IsLastControlLinked);

                bool setBLOCK = (rrc.html_BLOCK != null) || (indexEltXML == 0);
                bool isContainerOverflow = (null != rrc.ContainerOverflow) && rrc.ContainerOverflow.Value;
                // EG 20210304 [XXXXX] Relooking Referentiels - Gestion Ressource sur Panel overflow
                if (isContainerOverflow)
                {
                    pnlOverflow = new WCTogglePanel() { ID = "divoverflow" };
                    if (rrc.ContainerOverflow.startheightSpecified)
                        pnlOverflow.ControlBody.Attributes.Add("style", String.Format("max-height:calc(100vh - {0}px)", rrc.ContainerOverflow.startheight));
                    pnlOverflow.SetHeaderTitle(Ressource.GetString(rrc.ContainerOverflow.ressourceSpecified ? rrc.ContainerOverflow.ressource : "Detail"));
                }
                bool setHR = false;
                bool isTextBoxModeMultiLine = (rrc.TextMode == TextBoxMode.MultiLine.ToString())
                    || (TypeData.IsTypeString(rrc.DataType.value) && (rrc.Length >= 1000));

                setHR = (rrc.html_HR != null) || (indexEltXML == Referential.Column.Length);
                setHR = setHR || (isTextBoxModeMultiLine && !setBLOCK && !rrc.IsHide);
                #endregion

                #region html_TABLE
                if (setBLOCK)
                {
                    if (blockRemaining == 0)
                    {
                        if ((rrc.html_BLOCK != null) && (rrc.html_BLOCK[0].blockbyrowSpecified) && (rrc.html_BLOCK[0].blockbyrow > 1))
                        {
                            totalBlockByRow = rrc.html_BLOCK[0].blockbyrow;
                            blockRemaining = totalBlockByRow;
                        }
                    }

                    if (rrc.html_BLOCK != null)
                    {
                        InitVariables(rrc.html_BLOCK[0].columnbyrow, out totalColumnByRow, out defaultInputWidth, out defaultLabelWidth);
                    }
                    else if (rrc.html_HR != null)
                    {
                        InitVariables(rrc.html_HR[0].columnbyrow, out totalColumnByRow, out defaultInputWidth, out defaultLabelWidth);
                    }

                    if ((blockRemaining == 0) || (blockRemaining == totalBlockByRow))
                    {
                        #region No group block or First block of group
                        idTbl++;
                        currentTable = new Table()
                        {
                            ID = "tblrefblock" + idTbl.ToString(),
                            CssClass = "referential"
                        };
                        parentPanel = CreateTogglePanel(rrc.html_BLOCK);
                        currentPanel = parentPanel.ControlBody.Controls[0] as Panel;
                        #endregion

                    }
                    if (blockRemaining > 0)
                    {
                        #region Group block
                        string width = Convert.ToString(100 / totalBlockByRow) + "%";
                        if ((rrc.html_BLOCK != null) && (rrc.html_BLOCK[0].width != null))
                            width = rrc.html_BLOCK[0].width;
                        if (blockRemaining == totalBlockByRow)
                            currentTableRow = new TableRow();

                        currentTableCell = new TableCell();
                        TableTools.CellSetWidth(currentTableCell, width);
                        currentTableCell.HorizontalAlign = HorizontalAlign.Left;
                        currentTableCell.VerticalAlign = VerticalAlign.Top;
                        alPanel.Add(parentPanel);
                        alTable.Add(currentTable);
                        alTableRow.Add(currentTableRow);
                        alTableCell.Add(currentTableCell);
                        currentTableRow = null;
                        currentTableCell = null;

                        idTbl++;
                        currentTable = new Table()
                        {
                            ID = "tblrefchild" + idTbl.ToString(),
                            CssClass = "referential"
                        };
                        parentPanel = CreateTogglePanel(rrc.html_BLOCK);
                        currentPanel = parentPanel.ControlBody.Controls[0] as Panel;
                        #endregion Group block
                    }
                }
                #endregion

                #region html_BLOCK
                if (setBLOCK)
                {
                    string title = null, color = null;
                    if ((rrc.html_BLOCK != null) && (rrc.html_BLOCK[0].color != null))
                        color = rrc.html_BLOCK[0].color;
                    if ((rrc.html_BLOCK != null) && (rrc.html_BLOCK[0].title != null))
                        title = rrc.html_BLOCK[0].title;
                    //
                    if (currentPanel != null)
                    {
                        if (StrFunc.IsEmpty(title))
                            title = "Characteristics";
                        if (null != parentPanel.ControlHeaderTitle)
                            parentPanel.SetHeaderTitle(Ressource.GetString(title, true).ToLower());

                        if ((rrc.html_BLOCK != null) && (rrc.html_BLOCK[0].InformationSpecified))
                        {
                            currentTable.Rows.Add(
                                rrc.html_BLOCK[0].Information.GetWebCtrlInformation(totalColumnByRow, "Block_" + rrc.ColumnName, rrc.Ressource));
                        }
                    }
                    else
                    {
                        currentTable.Rows.Add(ControlsTools.GetStaticBlockHtmlPagev2(title, totalColumnByRow));
                    }

                    isNewTR = true;
                    isCurrentHR_Visible = true;
                }
                #endregion

                #region html_HR
                if (setHR)
                {
                    string title = null, color = null, size = null;
                    if (rrc.html_HR != null)
                    {
                        bool isTrace = this.IsTrace;
                        for (int nbHR = 0; nbHR < ArrFunc.Count(rrc.html_HR); nbHR++)
                        {
                            if (rrc.html_HR[nbHR] != null)
                            {
                                title = null;
                                color = null;
                                size = null;
                                if (rrc.html_HR[nbHR].title != null)
                                {
                                    title = string.Empty;
                                    if (rrc.html_HR[nbHR].titlemargin > 0)
                                        title = new StringBuilder().Insert(0, Cst.HTMLSpace, rrc.html_HR[nbHR].titlemargin).ToString();

                                    if (rrc.html_HR[nbHR].titlepuce != null)
                                        title += rrc.html_HR[nbHR].titlepuce;

                                    if (StrFunc.IsFilled(title))
                                        title = "{NoTranslate}" + title + Ressource.GetString(rrc.html_HR[nbHR].title, true);
                                    else
                                        title += rrc.html_HR[nbHR].title;
                                }
                                if (rrc.html_HR[nbHR].color != null)
                                    color = rrc.html_HR[nbHR].color;
                                if (rrc.html_HR[nbHR].size != null)
                                    size = rrc.html_HR[nbHR].size;
                                //
                                TableRow trHR = ControlsTools.GetHRHtmlPage(color, totalColumnByRow, size, title, null);
                                //**************************************************************************************
                                //20090601 PL TestPL 
                                //         En dur pour l'instant en attendant une évolution de l'objet htmlHR et du code
                                //**************************************************************************************

                                if (Referential.TableName == Cst.OTCml_TBL.ASSETENV.ToString())
                                    isCurrentHR_Visible = DisplayHRByProduct(HRTitle.AssetEnv, title, isCurrentHR_Visible);
                                else if (Referential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString())
                                    isCurrentHR_Visible = DisplayHRByProduct(HRTitle.ValidationRules, title, isCurrentHR_Visible);

                                isCurrentHR_Visible |= isTrace;
                                trHR.Visible = isCurrentHR_Visible;
                                trHR.Visible &= (!isLightDisplay_And_Hide);
                                //**************************************************************************************
                                // EG 20210304 [XXXXX] Relooking Referentiels - Gestion ShowAllData via Css
                                if (isLightDisplay_And_Hide)
                                    trHR.CssClass = cssClassDisplayLight;
                                currentTable.Rows.Add(trHR);
                            }
                        }
                    }
                    else
                    {
                        currentTable.Rows.Add(ControlsTools.GetHRHtmlPage(color, totalColumnByRow, size, title, null));
                    }
                    isNewTR = true;
                }
                #endregion

                #region isVisible
                if (isVisible)
                {
                    bool isCheckBoxAlignRight = (TypeData.IsTypeBool(rrc.DataType.value) && ((!rrc.AlignSpecified) || (rrc.Align.ToLower() == "right")));
                    #region Width for Label & Input
                    // EG 20210304 [XXXXX] Relooking Referentiels - Pas de Width par défaut sur les colonnes ExternalTableName
                    string labelWidth = string.Empty;
                    string inputWidth = string.Empty;
                    if (StrFunc.IsEmpty(rrc.ExternalTableName))
                    {
                        labelWidth = defaultLabelWidth;
                        inputWidth = defaultInputWidth;
                    }

                    bool isPercent = false;
                    try
                    {
                        if (StrFunc.IsEmpty(rrc.Ressource))
                            rrc.LabelWidth = @" width=""0""";

                        if (rrc.LabelWidth != null && rrc.LabelWidth.Length > 0)
                            labelWidth = @" width=""" + rrc.LabelWidth + @"""";

                        if ((rrc.InputWidth != null && rrc.InputWidth.Length > 0) && ((!rrc.HasInformationControls) || (!rrc.Information.LabelMessageSpecified)))// 20080619 PL
                            inputWidth = @" width=""" + rrc.InputWidth + @"""";
                        else if ((rrc.Colspan > 1) || (isCheckBoxAlignRight))
                        {
                            string _labelWidth = labelWidth.ToLower();
                            _labelWidth = _labelWidth.Replace("%", string.Empty);
                            _labelWidth = _labelWidth.Replace("width=", string.Empty);
                            _labelWidth = _labelWidth.Replace(@"""", string.Empty);
                            _labelWidth = _labelWidth.Replace(@"px", string.Empty);
                            _labelWidth = _labelWidth.Trim();

                            isPercent = (inputWidth.IndexOf("%") > 1);
                            inputWidth = inputWidth.ToLower();
                            inputWidth = inputWidth.Replace("%", string.Empty);
                            inputWidth = inputWidth.Replace("width=", string.Empty);
                            inputWidth = inputWidth.Replace(@"px", string.Empty);
                            inputWidth = inputWidth.Replace(@"""", string.Empty);
                            inputWidth = inputWidth.Trim();
                            int val_inputwidth = 0;
                            if (!String.IsNullOrEmpty(_labelWidth))
                            {
                                if (isCheckBoxAlignRight)
                                    val_inputwidth += Convert.ToInt32(_labelWidth);
                                else
                                    val_inputwidth = (Convert.ToInt32(inputWidth) * rrc.Colspan) + (Convert.ToInt32(_labelWidth) * (rrc.Colspan - 1));
                            }
                            inputWidth = @" width=""" + Convert.ToString(val_inputwidth);
                            inputWidth += (isPercent ? "%" : string.Empty) + @"""";
                        }
                        else if (rrc.Colspan == 0)
                        {
                            if (setHR)
                                inputWidth = @" width=""99%""";
                            else
                                inputWidth = string.Empty;
                        }
                        if (rrc.IsFirstControlLinked)
                            inputWidth = GetWidthControlLinked(rrc);
                    }
                    catch { }
                    #endregion

                    if (isNewTR)
                    {
                        columnRemaining = totalColumnByRow;
                        lastColumnRemaining = columnRemaining;
                        if (currentTableRow != null)
                            currentTable.Rows.Add(currentTableRow);
                        currentTableRow = new TableRow
                        {
                            Visible = isCurrentHR_Visible
                        };
                    }

                    #region CheckBox (TextAlign Right) / RadioButton
                    if (isCheckBoxAlignRight)
                    {
                        TableTools.AddCellSpace(currentTableRow);
                        currentTableCell = new TableCell();
                        #region Colspan
                        lastColumnRemaining = columnRemaining;
                        if (rrc.Colspan <= 0)
                        {
                            //colspan until end row
                            currentTableCell.ColumnSpan = (columnRemaining - 1);
                            columnRemaining = 0;
                        }
                        else
                        {
                            currentTableCell.ColumnSpan = (3 + ((rrc.Colspan - 1) * 4));
                            columnRemaining -= (4 + ((rrc.Colspan - 1) * 4));
                        }
                        if (rrc.IsRole || rrc.IsItem)
                            currentTableCell.Wrap = false;
                        #endregion

                        #region Information Controls
                        if (rrc.HasInformationControls)
                        {
                            #region Set Current objects to Array
                            alPanel.Add(parentPanel); // GLOP EG
                            currentPanel = null;
                            parentPanel = null;
                            alTable.Add(currentTable);
                            alTableRow.Add(currentTableRow);
                            alTableCell.Add(currentTableCell);
                            #endregion
                            #region New Table,Row,Cell
                            idTbl++;
                            currentTable = new Table
                            {
                                ID = "currentTable" + idTbl.ToString(),
                                Width = Unit.Percentage(100),
                                CellPadding = 1,
                                CellSpacing = 1,
                                BorderStyle = (this.IsDebugDesign ? BorderStyle.Solid : BorderStyle.None),
                                BorderColor = Color.Green
                            };
                            currentTableRow = new TableRow();
                            currentTableCell = new TableCell
                            {
                                Wrap = false
                            };
                            #endregion
                            if ((!rrc.Information.LabelMessageSpecified) && (!ReferentialTools.IsDataForDDL(rrc)))
                                currentTableCell.Width = Unit.Percentage(100);
                        }
                        else
                        {
                            TableTools.CellSetWidth(currentTableCell, inputWidth);
                        }
                        #endregion

                        if (rrc.ControlMainSpecified)
                        {
                            if (rrc.DataType.value.StartsWith("bool2"))
                            {
                                #region Cas de données boolean: radioButton
                                #region RadioButton
                                RadioButton radioButton = (RadioButton)rrc.ControlMain;
                                string[] text = radioButton.Text.Split("|".ToCharArray());
                                radioButton.GroupName = radioButton.ID;
                                radioButton.Text = text[0];
                                radioButton.ToolTip = string.Empty;
                                radioButton.Visible &= (!isLightDisplay_And_Hide);

                                RadioButton radioButton1 = new RadioButton
                                {
                                    AutoPostBack = radioButton.AutoPostBack,
                                    Checked = !radioButton.Checked,
                                    CssClass = radioButton.CssClass,
                                    Enabled = radioButton.Enabled,
                                    EnableViewState = radioButton.EnableViewState,
                                    GroupName = radioButton.GroupName,
                                    ID = radioButton.ID + "Bis"
                                };
                                if (text.Length > 1)
                                {
                                    radioButton1.Text = text[1];
                                }
                                radioButton1.Visible = radioButton.Visible;
                                #endregion

                                currentTableCell.Controls.Add(radioButton);
                                if ((rrc.DataType.value == "bool2h") || (!isLightDisplay_And_Hide))
                                {
                                    //Si isLightDisplay_And_Hide, on considère un affichage horizontal pour gagner de la place.
                                    currentTableCell.Controls.Add(new LiteralControl(Cst.HTMLSpace2));
                                }
                                else //"bool2v"
                                {
                                    currentTableCell.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
                                }
                                currentTableCell.Controls.Add(radioButton1);
                                #endregion
                            }
                            else
                            {
                                rrc.ControlMain.Visible &= (!isLightDisplay_And_Hide);
                                //PL 20161124 - RATP 4Eyes - MakingChecking
                                if (isMakingChecking_ActionChecking)
                                    rrc.ControlMain.Enabled = false;
                                currentTableCell.Controls.Add(rrc.ControlMain);
                            }
                        }

                        #region Information Controls
                        if (rrc.HasInformationControls)
                        {
                            bool isExistLabelMessage = (rrc.InformationControls[1] != null);
                            currentTableRow.Cells.Add(currentTableCell);
                            currentTableCell = null;
                            TableTools.AddCellSpace(currentTableRow);
                            currentTableCell = new TableCell();
                            if (!isExistLabelMessage)
                            {
                                currentTableCell.Width = Unit.Percentage(100);
                            }
                            rrc.InformationControls[0].Visible &= (!isLightDisplay_And_Hide);
                            currentTableCell.Controls.Add(rrc.InformationControls[0]);
                            if (isExistLabelMessage)
                            {
                                currentTableRow.Cells.Add(currentTableCell);
                                currentTableCell = null;
                                TableTools.AddCellSpace(currentTableRow);
                                currentTableCell = new TableCell
                                {
                                    Width = Unit.Percentage(100)
                                };
                                rrc.InformationControls[1].Visible &= (!isLightDisplay_And_Hide);
                                currentTableCell.Controls.Add(rrc.InformationControls[1]);
                            }
                            currentTableRow.Cells.Add(currentTableCell);
                            currentTableCell = null;
                            currentTable.Rows.Add(currentTableRow);
                            currentTableRow = null;
                            currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                            alTableCell.RemoveAt(alTableCell.Count - 1);
                            currentTableCell.Controls.Add(currentTable);
                            currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                            alTableRow.RemoveAt(alTableRow.Count - 1);
                            currentTable = (Table)alTable[alTable.Count - 1];
                            alTable.RemoveAt(alTable.Count - 1);
                            parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                            // EG 20150918 CSS (Radius)
                            int nb = parentPanel.Controls[1].Controls.Count;
                            currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                            alPanel.RemoveAt(alPanel.Count - 1);
                        }
                        #endregion
                        if (currentTableCell != null)
                        {
                            currentTableRow.Cells.Add(currentTableCell);
                            currentTableCell = null;
                        }
                    }
                    #endregion CheckBox (TextAlign Right) / RadioButton
                    #region Control of capture (Textbox, WCTextBox, DDL, Checkbox (TextAlign Left), ...)
                    else
                    {
                        #region Add Label
                        if (!(rrc.IsLastControlLinked || rrc.IsMiddleControlLinked))
                        {
                            TableTools.AddCellSpace(currentTableRow);
                            currentTableCell = new TableCell();
                            if ((!rrc.LabelNoWrapSpecified) || (rrc.LabelNoWrap))
                                currentTableCell.Wrap = false;

                            if ((rrc.Ressource == @":") || (rrc.Ressource == @"/"))
                            {
                                TableTools.CellSetWidth(currentTableCell, @" width=""1%""");
                            }
                            else
                            {
                                //PL 20120423 Add test sur currentTableCell.Wrap pour appliquer ou pas le Width
                                //AVANT
                                //TableTools.CellSetWidth(currentTableCell, labelWidth);
                                //APRES
                                if (currentTableCell.Wrap)
                                    TableTools.CellSetWidth(currentTableCell, labelWidth);
                                else
                                    TableTools.CellSetWidth(currentTableCell, @" width=""1""");
                            }

                            if (isTextBoxModeMultiLine)
                            {
                                currentTableCell.VerticalAlign = VerticalAlign.Top;
                            }

                            lastColumnRemaining = columnRemaining;
                            columnRemaining -= 2;

                            if (rrc.ControlLabelSpecified)
                            {
                                rrc.ControlLabel.Visible &= (!isLightDisplay_And_Hide);
                                currentTableCell.Controls.Add(rrc.ControlLabel);
                            }

                            currentTableRow.Cells.Add(currentTableCell);
                            currentTableCell = null;

                            TableTools.AddCellSpace(currentTableRow);
                            currentTableCell = new TableCell();
                            lastColumnRemaining = columnRemaining;
                            if (rrc.Colspan <= 0)
                            {
                                currentTableCell.ColumnSpan = (columnRemaining - 1);
                                columnRemaining = 0;
                            }
                            else
                            {
                                currentTableCell.ColumnSpan = (1 + ((rrc.Colspan - 1) * 4));
                                columnRemaining -= (2 + ((rrc.Colspan - 1) * 4));
                            }

                            if ((!rrc.IsFirstControlLinked) && (!TypeData.IsTypeDateOrDateTime(rrc.DataType.value)))
                            {
                                TableTools.CellSetWidth(currentTableCell, inputWidth);
                            }
                        }
                        #endregion Add Label

                        if (ArrFunc.IsFilled(rrc.OtherControls) || rrc.HasInformationControls)
                        {
                            #region OtherControls or HasInformationControls
                            //Create table with 2 columns for TextBox and WCImgCalendar or columns with information controls
                            #region Set Current objects to Array
                            alPanel.Add(parentPanel);
                            currentPanel = null;
                            parentPanel = null;
                            alTable.Add(currentTable);
                            alTableRow.Add(currentTableRow);
                            alTableCell.Add(currentTableCell);
                            #endregion
                            #region New Table,Row,Cell
                            idTbl++;
                            currentTable = new Table();
                            currentTable.ID = "tbl" + idTbl.ToString();
                            currentTable.CellPadding = 0;
                            currentTable.CellSpacing = 0;
                            currentTable.BorderStyle = (this.IsDebugDesign ? BorderStyle.Solid : BorderStyle.None);
                            currentTable.BorderColor = Color.Red;
                            #region Table Width
                            currentTable.Width = Unit.Percentage(100);
                            #endregion
                            currentTableRow = new TableRow();
                            currentTableCell = new TableCell();
                            #endregion
                            if ((null != rrc.Information && !rrc.Information.LabelMessageSpecified) && (!ReferentialTools.IsDataForDDL(rrc)))
                            {
                                //Cela permet de cadrer l'image (i ou !) à droite
                                currentTableCell.Width = Unit.Percentage(100);
                            }
                            else
                            {
                                if (StrFunc.IsFilled(rrc.InputWidth))//20080619 PL add du if()
                                {
                                    Unit customWidth;
                                    if (rrc.InputWidth.EndsWith("%") && rrc.InputWidth.Length > 1)
                                        customWidth = Unit.Percentage(Convert.ToInt32(rrc.InputWidth.Substring(0, rrc.InputWidth.Length - 1)));
                                    else if (rrc.InputWidth.EndsWith("px") && rrc.InputWidth.Length > 2)
                                        customWidth = Unit.Pixel(Convert.ToInt32(rrc.InputWidth.Substring(0, rrc.InputWidth.Length - 2)));
                                    else
                                        customWidth = Unit.Pixel(Convert.ToInt32(rrc.InputWidth));
                                    currentTableCell.Width = customWidth;
                                }
                            }
                            #endregion
                        }
                        else if (rrc.IsFirstControlLinked)
                        {
                            #region IsFirstControlLinked
                            //Create table with n columns for columns linked	
                            alPanel.Add(parentPanel);
                            currentPanel = null;
                            parentPanel = null;
                            alTable.Add(currentTable);
                            alTableRow.Add(currentTableRow);
                            alTableCell.Add(currentTableCell);
                            idTbl++;
                            currentTable = new Table
                            {
                                ID = "tbl" + idTbl.ToString(),
                                CellPadding = 0,
                                CellSpacing = 0,
                                BorderStyle = (this.IsDebugDesign ? BorderStyle.Solid : BorderStyle.None),
                                BorderColor = Color.Blue,
                                Width = Unit.Percentage(100)
                            };

                            currentTableRow = new TableRow();
                            currentTableCell = new TableCell();
                            TableTools.CellSetWidth(currentTableCell, inputWidth);
                            #endregion
                        }

                        //GlopPL gérer rrc.Scale                       
                        #region CheckBox (Align Left)
                        if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            if (rrc.ControlMainSpecified)
                            {
                                //20120525 PL Code ci-dessous ajouté...
                                ((CheckBox)rrc.ControlMain).Text = string.Empty;
                                rrc.ControlMain.Visible &= (!isLightDisplay_And_Hide);
                                //PL 20161124 - RATP 4Eyes - MakingChecking
                                if (isMakingChecking_ActionChecking)
                                    rrc.ControlMain.Enabled = false;
                                currentTableCell.Controls.Add(rrc.ControlMain);
                            }

                            #region Information Controls
                            if (rrc.HasInformationControls)
                            {
                                currentTableRow.Cells.Add(currentTableCell);
                                TableTools.AddCellSpace(currentTableRow);
                                currentTableCell = new TableCell();
                                TableTools.CellSetWidth(currentTableCell, "15");

                                rrc.InformationControls[0].Visible &= (!isLightDisplay_And_Hide);
                                currentTableCell.Controls.Add(rrc.InformationControls[0]);

                                currentTableRow.Cells.Add(currentTableCell);
                                TableTools.AddCellSpace(currentTableRow);
                                currentTableCell = new TableCell
                                {
                                    Width = Unit.Percentage(50)
                                };

                                rrc.InformationControls[1].Visible &= (!isLightDisplay_And_Hide);
                                currentTableCell.Controls.Add(rrc.InformationControls[1]);

                                currentTableRow.Cells.Add(currentTableCell);
                                currentTableCell = null;
                                TableTools.AddCellFullSpace(currentTableRow);
                                currentTable.Rows.Add(currentTableRow);
                                currentTableRow = null;
                                //
                                currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                                alTableCell.RemoveAt(alTableCell.Count - 1);
                                currentTableCell.Controls.Add(currentTable);
                                currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                                alTableRow.RemoveAt(alTableRow.Count - 1);
                                currentTable = (Table)alTable[alTable.Count - 1];
                                alTable.RemoveAt(alTable.Count - 1);
                                parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                                if (parentPanel != null)
                                    currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                                alPanel.RemoveAt(alPanel.Count - 1);
                            }
                            #endregion

                            //PL 20120525 Code ci-dessous ajouté...
                            if (currentTableCell != null)
                            {
                                currentTableRow.Cells.Add(currentTableCell);
                                currentTableCell = null;
                            }
                        }
                        #endregion CheckBox (Align Left)
                        #region TextBox, WCTextBox, DropDownList
                        else
                        {
                            #region DropDownList,HtmlSelect
                            if (ReferentialTools.IsDataForDDL(rrc))
                            {
                                if (rrc.ControlMainSpecified)
                                {
                                    rrc.ControlMain.Visible &= (!isLightDisplay_And_Hide);
                                    //PL 20161124 - RATP 4Eyes - MakingChecking
                                    if (isMakingChecking_ActionChecking)
                                        rrc.ControlMain.Enabled = false;
                                    currentTableCell.Controls.Add(rrc.ControlMain);
                                }
                                else if (rrc.HtmlControlMainSpecified)
                                {
                                    rrc.HtmlControlMain.Visible &= (!isLightDisplay_And_Hide);
                                    //PL 20161124 - RATP 4Eyes - MakingChecking
                                    if (isMakingChecking_ActionChecking)
                                        rrc.HtmlControlMain.Disabled = true;
                                    currentTableCell.Controls.Add(rrc.HtmlControlMain);
                                }
                                //
                                #region Code spécifique pour DDLs "Color": Affiche d"un "Preview"
                                if (!ReferentialTools.IsDataForDDLParticular(rrc) && (rrc.Relation[0].DDLType != null)
                                    && Cst.IsDDLTypeStyleComponentColor(rrc.Relation[0].DDLType.Value))
                                {
                                    //JavaScript.SetColor((PageBase)this);
                                    if (!rrc.IsFirstControlLinked && !rrc.IsMiddleControlLinked && rrc.IsLastControlLinked)
                                    {
                                        string ctrlID = "preview" + rrc.FirstControlLinkedColumnName;
                                        currentTableRow.Cells.Add(currentTableCell);
                                        TableTools.AddCellSpace(currentTableRow);
                                        currentTableCell = new TableCell();
                                        //
                                        HtmlInputText htmlInput = new HtmlInputText
                                        {
                                            Value = "PREVIEW"
                                        };
                                        htmlInput.Style.Add(HtmlTextWriterStyle.Width, "70");
                                        htmlInput.Style.Add(HtmlTextWriterStyle.TextAlign, "center");
                                        htmlInput.ID = ctrlID;
                                        currentTableCell.Controls.Add(htmlInput);
                                        currentTableRow.Cells.Add(currentTableCell);
                                        TableTools.AddCellSpace(currentTableRow);
                                        currentTableCell = new TableCell();
                                    }
                                }
                                #endregion
                                #region Code spécifique pour les données "Role"
                                if (rrc.IsRole)
                                {
                                    if (Referential.drExternal[rrc.ExternalFieldID].Table.Rows.Count > 1)
                                    {
                                        //NB: Cas où un même rôle est attribué à l'égard de plusieurs acteurs...
                                        //    Seul le 1er acteur est affiché !
                                        WCTooltipLabel lblInfoMulti = new WCTooltipLabel
                                        {
                                            ForeColor = Color.DarkRed,
                                            Text = @"Warning2"
                                        };
                                        lblInfoMulti.Font.Bold = true;
                                        lblInfoMulti.Pty.TooltipContent = @"Attention: Ce rôle est accordé à l'égard de plusieurs acteurs, pour consulter l'ensemble des acteurs à l'égard desquels ce rôle est donné, veuillez utiliser le menu ""Détail"".";
                                        lblInfoMulti.Attributes.Add("style", "cursor: pointer;border-top: solid 2px red;border-bottom: solid 2px red");
                                        currentTableCell.Controls.Add(new LiteralControl(Cst.HTMLSpace2));
                                        currentTableCell.Controls.Add(lblInfoMulti);
                                    }
                                }
                                #endregion
                                #region Code spécifique pour les "Information Controls": Affiche du "Tooltip"
                                if (rrc.HasInformationControls)
                                {
                                    bool isExistLabelMessage = (rrc.InformationControls[1] != null);
                                    //
                                    currentTableRow.Cells.Add(currentTableCell);
                                    TableTools.AddCellSpace(currentTableRow);
                                    //
                                    currentTableCell = new TableCell();
                                    if (!isExistLabelMessage)
                                        currentTableCell.Width = Unit.Percentage(100);

                                    rrc.InformationControls[0].Visible &= (!isLightDisplay_And_Hide);
                                    currentTableCell.Controls.Add(rrc.InformationControls[0]);
                                    if (isExistLabelMessage)
                                    {
                                        //Create new Cell for "Label" of InformationControls
                                        currentTableRow.Cells.Add(currentTableCell);
                                        TableTools.AddCellSpace(currentTableRow);
                                        currentTableCell = new TableCell
                                        {
                                            Width = Unit.Percentage(100)
                                        };
                                        rrc.InformationControls[1].Visible &= (!isLightDisplay_And_Hide);
                                        currentTableCell.Controls.Add(rrc.InformationControls[1]);
                                    }
                                    currentTableRow.Cells.Add(currentTableCell);
                                    currentTableCell = null;
                                    currentTable.Rows.Add(currentTableRow);
                                    currentTableRow = null;
                                    //
                                    currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                                    alTableCell.RemoveAt(alTableCell.Count - 1);
                                    currentTableCell.Controls.Add(currentTable);
                                    currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                                    alTableRow.RemoveAt(alTableRow.Count - 1);
                                    currentTable = (Table)alTable[alTable.Count - 1];
                                    alTable.RemoveAt(alTable.Count - 1);
                                    parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                                    // EG 20150918 CSS (Radius)
                                    if (parentPanel != null)
                                    {
                                        int nb = parentPanel.Controls[1].Controls.Count;
                                        currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                                    }
                                    alPanel.RemoveAt(alPanel.Count - 1);
                                }
                                #endregion
                            }
                            #endregion DropDownList,HtmlSelect

                            #region TextBox, WCTextBox, AutoComplete
                            else
                            {
                                if (rrc.ControlMainSpecified)
                                {
                                    try
                                    {
                                        if ((rrc.ColumnName == "URL") && (((TextBox)rrc.ControlMain).ReadOnly))
                                        {
                                            HyperLink hl = new HyperLink
                                            {
                                                Target = Cst.HyperLinkTargetEnum._blank.ToString(),
                                                CssClass = "linkDatagrid",
                                                Text = ((TextBox)rrc.ControlMain).Text
                                            };
                                            hl.NavigateUrl = hl.Text;
                                            //
                                            hl.Visible &= (!isLightDisplay_And_Hide);
                                            currentTableCell.Controls.Add(hl);
                                        }
                                        else
                                        {
                                            rrc.ControlMain.Visible &= (!isLightDisplay_And_Hide);
                                            //PL 20161124 - RATP 4Eyes - MakingChecking
                                            if (isMakingChecking_ActionChecking)
                                            {
                                                //((TextBox)rrc.ControlMain).ReadOnly = false;
                                                if (TypeData.IsTypeDateOrDateTime(rrc.DataType.value))
                                                    rrc.ControlMain.CssClass = EFSCssClass.CaptureConsult;
                                                else
                                                    rrc.ControlMain.Enabled = false;
                                            }
                                            currentTableCell.Controls.Add(rrc.ControlMain);
                                            // EG 20100827
                                            currentTableCell.Wrap = false;
                                        }
                                    }
                                    catch
                                    {
                                        rrc.ControlMain.Visible &= (!isLightDisplay_And_Hide);
                                        currentTableCell.Controls.Add(rrc.ControlMain);
                                    }
                                }
                                // GLOP FI 20070206 A faire plus tard....
                                if (rrc.IsDataXml && ArrFunc.IsFilled(rrc.OtherControls) && rrc.OtherControls[0] != null)
                                {
                                    //Exist TR with 4 TD for TextBox and WCImgCalendar
                                    currentTableRow.Cells.Add(currentTableCell);
                                    currentTableCell = null;
                                    TableTools.AddCellSpace(currentTableRow);
                                    currentTableCell = new TableCell();

                                    // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
                                    LinkButton imgbutXml = rrc.OtherControls[0] as LinkButton;
                                    imgbutXml.Click += new EventHandler(OnXmlClick);
                                    imgbutXml.Visible &= (!isLightDisplay_And_Hide);
                                    currentTableCell.Controls.Add(imgbutXml);

                                    currentTableRow.Cells.Add(currentTableCell);
                                    currentTableCell = null;
                                    currentTable.Rows.Add(currentTableRow);
                                    currentTableRow = null;
                                    //
                                    currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                                    alTableCell.RemoveAt(alTableCell.Count - 1);
                                    currentTableCell.Controls.Add(currentTable);
                                    currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                                    alTableRow.RemoveAt(alTableRow.Count - 1);
                                    currentTable = (Table)alTable[alTable.Count - 1];
                                    alTable.RemoveAt(alTable.Count - 1);
                                    parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                                    // EG 20150918 CSS (Radius)
                                    if (parentPanel != null)
                                        currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                                    alPanel.RemoveAt(alPanel.Count - 1);
                                }
                                //
                                //Information Controls
                                if (rrc.HasInformationControls)
                                {
                                    bool isExistLabelMessage = (rrc.InformationControls[1] != null);
                                    // EG 20210304 [XXXXX] Relooking Referentiels - On insère une table cell que si présence d'un labelMessage
                                    // EG 20240606 [XXXXXX] Delete if (isExistLabelMessage)
                                    // EG 20240606 [XXXXX] Integration of a tooltipMessage in the same <table> as the column to which it is attached
                                    // if (isExistLabelMessage)
                                    // {
                                    currentTableRow.Cells.Add(currentTableCell);
                                        currentTableCell = null;
                                        TableTools.AddCellSpace(currentTableRow);
                                        currentTableCell = new TableCell();
                                        if (isExistLabelMessage)
                                            currentTableCell.Width = Unit.Pixel(1);
                                        else
                                            currentTableCell.Width = Unit.Percentage(95);
                                    //}

                                    rrc.InformationControls[0].Visible &= (!isLightDisplay_And_Hide);
                                    currentTableCell.Controls.Add(rrc.InformationControls[0]);
                                    if (isExistLabelMessage)
                                    {
                                        currentTableRow.Cells.Add(currentTableCell);
                                        currentTableCell = null;
                                        TableTools.AddCellSpace(currentTableRow);
                                        currentTableCell = new TableCell();
                                        if (StrFunc.IsFilled(rrc.InputWidth))//20080619 PL add du if()
                                            currentTableCell.Width = currentTableCell.Width;
                                        else
                                            currentTableCell.Width = Unit.Percentage(60);

                                        rrc.InformationControls[1].Visible &= (!isLightDisplay_And_Hide);
                                        currentTableCell.Controls.Add(rrc.InformationControls[1]);
                                    }
                                    currentTableRow.Cells.Add(currentTableCell);
                                    currentTableCell = null;
                                    currentTable.Rows.Add(currentTableRow);
                                    currentTableRow = null;
                                    //
                                    currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                                    alTableCell.RemoveAt(alTableCell.Count - 1);
                                    currentTableCell.Controls.Add(currentTable);
                                    currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                                    alTableRow.RemoveAt(alTableRow.Count - 1);
                                    currentTable = (Table)alTable[alTable.Count - 1];
                                    alTable.RemoveAt(alTable.Count - 1);
                                    parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                                    // EG 20150918 CSS (Radius)
                                    if (parentPanel != null)
                                        currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                                    alPanel.RemoveAt(alPanel.Count - 1);
                                }
                            }
                            #endregion TextBox, WCTextBox

                            if (rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked)
                            {
                                //Exist TR with n TD for columns linked
                                // EG 20100428
                                TableTools.CellSetWidth(currentTableCell, GetWidthControlLinked(rrc));
                                currentTableRow.Cells.Add(currentTableCell);
                                TableTools.AddCellSpace(currentTableRow);
                                currentTableCell = new TableCell();
                                // EG 20100428
                                //TableTools.CellSetWidth(currentTableCell, GetWidthControlLinked(rrc.InputWidth));
                            }
                            else if (rrc.IsLastControlLinked)
                            {
                                //Exist TR with n TD for columns linked
                                //Ajout d'une cellule vide à taille non renseignée (<TD>&nbsp;</TD>) avant de fermer le table 
                                // --> permet de forcer le resize uniquement sur cette cellule
                                // EG 20100428
                                TableTools.CellSetWidth(currentTableCell, GetWidthControlLinked(rrc));
                                currentTableRow.Cells.Add(currentTableCell);
                                currentTable.Rows.Add(currentTableRow);
                                currentTableRow = null;
                                //
                                currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                                alTableCell.RemoveAt(alTableCell.Count - 1);
                                currentTableCell.Controls.Add(currentTable);
                                currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                                alTableRow.RemoveAt(alTableRow.Count - 1);
                                currentTable = (Table)alTable[alTable.Count - 1];
                                alTable.RemoveAt(alTable.Count - 1);
                                parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                                // EG 20150918 CSS (Radius)
                                if (parentPanel != null)
                                    currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                                alPanel.RemoveAt(alPanel.Count - 1);
                            }
                            if (!(rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked))
                            {
                                currentTableRow.Cells.Add(currentTableCell);
                                currentTableCell = null;
                            }
                        }
                        #endregion TextBox, WCTextBox, DropDownList
                    }
                    #endregion Add Control of capture (Textbox, WCTextBox, DDL, Checkbox (TextAlign Left), ...)
                }
                #endregion isVisible

                //Close TR balise if : 
                // - Last column of row							
                // - Last data
                // - Next data is html command
                // - Next data is EXTLLINK data
                bool isCloseTR = false;
                //
                // RD 20100518 / Ajout du test "currentTable != null"
                // Bug dans le cas d'un EXTLID
                // En plus, il n'est pas logique d'essayer de fermer une table qui n'existe pas (currentTable = null)
                //
                bool isCloseTABLE = ((indexEltXML == Referential.Column.Length - 1) && (currentTable != null));//Last ReferentialColumn

                if (false == isCloseTABLE)
                {
                    isCloseTABLE = ((indexEltXML < Referential.Column.Length - 1) && (Referential.Column[indexEltXML + 1].html_BLOCK != null));
                    isCloseTR = (isCloseTABLE || ((columnRemaining <= 0) && !(rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked)));
                    if (!isCloseTR)
                    {
                        isCloseTR = ((indexEltXML < Referential.Column.Length - 1) && (Referential.Column[indexEltXML + 1].html_HR != null));
                        if (!isCloseTR)
                        {
                            isCloseTR = (indexColSQL + 1 == Referential.IndexColSQL_EXTLLINK);
                            //20090107 PL Add pour PERMISSION... et pour les EXTLID
                            if (!isCloseTR)
                            {
                                isCloseTR = (indexColSQL == Referential.IndexColSQL_EXTLLINK)
                                            || (rrc.IsExternal);
                            }
                        }
                    }
                }
                //20090107 PL Pour les EXTLID
                if (isCloseTR || isCloseTABLE)
                {
                    columnRemaining = 0;
                    if (currentTableRow != null)
                    {
                        // EG 20210304 [XXXXX] Relooking Referentiels - Gestion ShowAllData via Css
                        if (isLightDisplay_And_Hide)
                            currentTableRow.CssClass = cssClassDisplayLight;
                        currentTable.Rows.Add(currentTableRow);
                        currentTableRow = null;
                    }
                }
                if (isCloseTABLE)
                {
                    if (alTableCell.Count > 0)
                    {
                        currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                        alTableCell.RemoveAt(alTableCell.Count - 1);
                        if (currentPanel != null)
                        {
                            currentPanel.Controls.Add(currentTable);
                            currentTableCell.Controls.Add(parentPanel);
                            parentPanel = null;
                        }
                        else
                        {
                            currentTableCell.Controls.Add(currentTable);
                        }
                        currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                        alTableRow.RemoveAt(alTableRow.Count - 1);
                        currentTable = (Table)alTable[alTable.Count - 1];
                        alTable.RemoveAt(alTable.Count - 1);
                        parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                        if (parentPanel != null)
                            currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                        alPanel.RemoveAt(alPanel.Count - 1);
                    }
                    else
                    {
                        if (currentPanel != null)
                        {
                            currentPanel.Controls.Add(currentTable);
                            if (null != pnlOverflow)
                                pnlOverflow.AddContent(parentPanel);
                            else
                                pnlBody.Controls.Add(parentPanel);
                            parentPanel = null;
                        }
                        else
                        {
                            if (null != pnlOverflow)
                                pnlOverflow.AddContent(currentTable);
                            else
                                pnlBody.Controls.Add(currentTable);
                        }
                        currentTable = null;
                        currentTableRow = null;
                        currentTableCell = null;
                    }
                    //
                    if (blockRemaining > 0)
                    {
                        blockRemaining--;
                        if (currentTableRow != null)
                        {
                            currentTableRow.Controls.Add(currentTableCell);
                            currentTableCell = null;
                            if (blockRemaining == 0)//Last 
                            {
                                currentTable.Rows.Add(currentTableRow);
                                currentTableRow = null;
                                //
                                if (alTableCell.Count > 0)
                                {

                                    currentTableCell = (TableCell)alTableCell[alTableCell.Count - 1];
                                    alTableCell.RemoveAt(alTableCell.Count - 1);
                                    if (currentPanel != null)
                                    {
                                        currentPanel.Controls.Add(currentTable);
                                        currentTableCell.Controls.Add(parentPanel);
                                        parentPanel = null;
                                    }
                                    else
                                    {
                                        currentTableCell.Controls.Add(currentTable);
                                    }
                                    currentTableRow = (TableRow)alTableRow[alTableRow.Count - 1];
                                    alTableRow.RemoveAt(alTableRow.Count - 1);
                                    currentTable = (Table)alTable[alTable.Count - 1];
                                    alTable.RemoveAt(alTable.Count - 1);
                                    parentPanel = (WCTogglePanel)alPanel[alPanel.Count - 1];
                                    // EG 20150918 CSS (Radius)
                                    if (parentPanel != null)
                                        currentPanel = (Panel)parentPanel.Controls[1].Controls[0];
                                    alPanel.RemoveAt(alPanel.Count - 1);
                                }
                                else
                                {
                                    if (null != pnlOverflow)
                                        pnlOverflow.AddContent(currentTable);
                                    else
                                        pnlBody.Controls.Add(currentTable);
                                    currentTable = null;
                                    currentTableRow = null;
                                    currentTableCell = null;
                                }
                            }
                        }
                    }
                }
            }//end for
            if (currentTable != null)
            {
                if (currentPanel != null)
                {
                    currentPanel.Controls.Add(currentTable);
                    if (null != pnlOverflow)
                        pnlOverflow.AddContent(parentPanel);
                    else
                        pnlBody.Controls.Add(parentPanel);
                    parentPanel = null;
                }
                else
                {
                    if (null != pnlOverflow)
                        pnlOverflow.AddContent(currentTable);
                    else
                        pnlBody.Controls.Add(currentTable);
                }
                currentTable = null;
            }
            //Add ValidationSummary control for all Validator controls
            ValidationSummary validationSummary = new ValidationSummary
            {
                ShowMessageBox = true,
                ShowSummary = false
            };
            if (null != pnlOverflow)
                pnlBody.Controls.Add(pnlOverflow);

            PanelForm.Controls.Add(pnlBody);
            PanelForm.Controls.Add(validationSummary);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBlock"></param>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments
        // EG 20201030 [XXXXX] Control Test Existence de ControlMain (ControlMainSpecified)
        // EG 20210323 [25556] Modification nom de classe (imgSizeX -> SizeX) sur titre Block Header referentiel    
        private WCTogglePanel CreateTogglePanel(ReferentialsReferentialColumnhtml_BLOCK[] pBlock)
        {
            Color backgroundColor = Color.Transparent;
            bool isReverse = false;
            string cssClass = "size3";
            string startdisplay = string.Empty;

            string mainClass = this.CSSMode + " " + mainMenuClassName + " referential";

            if (ArrFunc.IsFilled(pBlock))
            {
                ReferentialsReferentialColumnhtml_BLOCK block = pBlock[0];
                if (block.backcolorheaderSpecified)
                {
                    if (block.backcolorheader.StartsWith("status:"))
                    {
                        try
                        {
                            string columnStatus = block.backcolorheader.Substring(7);//NB: 7 = "status:".Length
                            if (Referential[columnStatus].ControlMainSpecified)
                            {
                                string columValue = ((TextBox)Referential[columnStatus].ControlMain).Text;
                                backgroundColor = ReferentialTools.GetColorForStatusValue(columValue);
                                backgroundColor = Color.FromName(backgroundColor.Name + "!important");
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        backgroundColor = Color.FromName(block.backcolorheader + "!important");
                    }
                }
                if (block.reverseSpecified)
                    isReverse = BoolFunc.IsTrue(block.reverse);
                if (block.cssheadertagSpecified)
                    cssClass = block.cssheadertag;
                if (block.startdisplaySpecified)
                {
                    startdisplay = block.startdisplay;
                }
            }

            WCTogglePanel togglePanel = new WCTogglePanel(backgroundColor, string.Empty, cssClass, isReverse)
            {
                CssClass = mainClass
            };
            // EG 20210304 [XXXXX] Relooking Referentiels - Gestion du startdisplay
            if (startdisplay == "collapse")
                togglePanel.ControlBody.Style.Add(HtmlTextWriterStyle.Display, "none");
            return togglePanel;
        }

        /// <summary>
        /// Sauvegarde d'un formulaire "Referential".
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsSelfClose"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)-->
        // RD 20200610 [25357] Modify
        // EG 20210222 [XXXXX] Suppression inscription OpenerCallReloadAndClose et OpenerCallRefreshAndClose  
        //                     (Remplacé par appel de OpenerRefreshAndClose présent dans PageBase.js)
        // EG 20210222 [XXXXX] Suppression inscription OpenerCallReload et OpenerCallRefresh  
        //                     (Remplacé par appel de OpenerRefresh présent dans PageBase.js)
        // EG 20210628 [25500] Customer Portal / EFS WebSite : Refactoring de la gestion de la mise à jour des flux RSS (Syndication)
        // EG 20210630 [25500] Gestion de la modification d'un item du flux RSS         
        // EG 20220404 [XXXXX][WI612] Correction insertion dans la table TRADEXML d'un template d'instrument
        private Cst.ErrLevel Save(IDbTransaction pDbTransaction, bool pIsSelfClose, object pSender)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            string message, error, keyField, keyFieldNewValue, keyFieldOldValue;
            message = error = keyField = keyFieldNewValue = keyFieldOldValue = string.Empty;

            // RD 20100310 / Automatic Compute identifier
            bool isTriggerLoCommandToExecute = false;

            if (SessionTools.IsSessionSysAdmin && (Referential.TableName == Cst.OTCml_TBL.EFSOBJECTDET.ToString()))
            {
                #region Get Modified or Inserted object
                try
                {
                    if (Referential.dataRow.RowState == DataRowState.Added ||
                        Referential.dataRow.RowState == DataRowState.Modified ||
                        Referential.dataRow.RowState == DataRowState.Detached)
                    {
                        Cst.CommandTypeEnum commandeType = (Cst.CommandTypeEnum)Enum.Parse(typeof(Cst.CommandTypeEnum), Referential.dataRow["COMMANDTYPE"].ToString());

                        if (commandeType == Cst.CommandTypeEnum.SQLTrigger)
                        {
                            isTriggerLoCommandToExecute = true;
                        }
                    }
                }
                catch (Exception e) { System.Diagnostics.Debug.WriteLine("Error to get Modified or Inserted object: " + e.Message); }
                #endregion
            }

            ret = ReferentialTools.SaveReferential(pDbTransaction, idMenu, Referential,
                out int rowsAffected, out message, out error,
                out keyField, out keyFieldNewValue, out keyFieldOldValue);

            if (ret == Cst.ErrLevel.SQLDEFINED)
            {
                JavaScript.DialogImmediate(this, error + Cst.CrLf + Cst.CrLf + Ressource.GetString(message), false, ProcessStateTools.StatusWarningEnum);
            }
            else if (rowsAffected <= 0)
            {
                JavaScript.DialogImmediate(this, Ressource.GetString("Msg_NoModification"), false, ProcessStateTools.StatusErrorEnum);
            }
            else
            {
                //******************************************************************************************************************************
                //Spécifique au referentiel: ACTOR (Create sub-repository from template)
                //******************************************************************************************************************************
                #region Spécifique au referentiel: ACTOR (Duplicate sub-reférentials)
                if (Referential.TableName == Cst.OTCml_TBL.ACTOR.ToString())
                {
                    //ADDCLIENTGLOP Test in progress... TBD Tester que l'on est bien sur une duplication depuis un Template.
                    //PL 20130416  
                    if (Referential.isNewRecord)
                    {
                        bool isDuplicateRecord = BoolFunc.IsTrue(Request.QueryString["DPC"]);//DPC:DuPliCate
                        if (isDuplicateRecord && IntFunc.IsPositiveInteger(keyFieldNewValue) && IntFunc.IsPositiveInteger(keyFieldOldValue))
                        {
                            int IdA_Template = IntFunc.IntValue2(keyFieldOldValue);
                            int IdA = IntFunc.IntValue2(keyFieldNewValue);

                            StrBuilder sqlQuery;
                            int nRows = 0;

                            //NB: A des fins d'audit, volontairement on met à jour cette colonne dans un 2ème temps (même si cela engendre de fait un insert dans ACTOR_P)
                            sqlQuery = new StrBuilder();
                            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.ACTOR.ToString();
                            sqlQuery += SQLCst.SET + "IDA_TEMPLATE=" + IdA_Template.ToString();
                            sqlQuery += SQLCst.WHERE + "IDA=" + IdA.ToString();

                            if (pDbTransaction == null)
                            {
                                nRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery.ToString());
                            }
                            else
                            {
                                nRows = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery.ToString());
                            }

                            if (nRows > 0)
                            {
                                #region RISKMARGIN and CLEARINGORGPARAM/IMREQMARKETPARAM
                                // PL 20191105 Il faudra gérer IDB
                                sqlQuery = new StrBuilder();
                                sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.RISKMARGIN.ToString();
                                sqlQuery += "(IDA,DTENABLED,DTDISABLED,DTINS,IDAINS,IDB,ISGROSSMARGINING,IMWEIGHTINGRATIO,IMSCOPE,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                sqlQuery += SQLCst.SELECT + "a.IDA,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                sqlQuery += "b.IDB,rm.ISGROSSMARGINING,rm.IMWEIGHTINGRATIO,rm.IMSCOPE,rm.EXTLLINK,rm.ROWATTRIBUT" + Cst.CrLf;
                                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.RISKMARGIN.ToString() + " rm" + Cst.CrLf;
                                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on b.IDA=a.IDA and b.IDB=rm.IDB" + Cst.CrLf;
                                sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                sqlQuery += SQLCst.AND + "rm.IDA=" + IdA_Template.ToString();

                                nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);

                                if (nRows > 0)
                                {
                                    #region CLEARINGORGPARAM
                                    // RD 20200610 [25357] Add column CLEARINGORGPARAM.ISIMFOREXEASSPOS
                                    sqlQuery = new StrBuilder();
                                    sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CLEARINGORGPARAM.ToString();
                                    sqlQuery += "(IDA,DTENABLED,DTDISABLED,DTINS,IDAINS,IDA_CSS,SPANACCOUNTTYPE,ISIMMAINTENANCE,ISIMFOREXEASSPOS,IMWEIGHTINGRATIO,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                    sqlQuery += SQLCst.SELECT + "a.IDA,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                    sqlQuery += "cop.IDA_CSS,cop.SPANACCOUNTTYPE,cop.ISIMMAINTENANCE,cop.ISIMFOREXEASSPOS,cop.IMWEIGHTINGRATIO,cop.EXTLLINK,cop.ROWATTRIBUT" + Cst.CrLf;
                                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                    sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.CLEARINGORGPARAM.ToString() + " cop" + Cst.CrLf;
                                    sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                    sqlQuery += SQLCst.AND + "cop.IDA=" + IdA_Template.ToString();

                                    nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                    #endregion

                                    #region IMREQMARKETPARAM
                                    sqlQuery = new StrBuilder();
                                    sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.IMREQMARKETPARAM.ToString();
                                    sqlQuery += "(IDA,DTENABLED,DTDISABLED,DTINS,IDAINS,IDM,POSSTOCKCOVER,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                    sqlQuery += SQLCst.SELECT + "a.IDA,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                    sqlQuery += "imrmp.IDM,imrmp.POSSTOCKCOVER,imrmp.EXTLLINK,imrmp.ROWATTRIBUT" + Cst.CrLf;
                                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                    sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.IMREQMARKETPARAM.ToString() + " imrmp" + Cst.CrLf;
                                    sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                    sqlQuery += SQLCst.AND + "imrmp.IDA=" + IdA_Template.ToString();

                                    nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                    #endregion
                                }
                                #endregion

                                #region CASHBALANCE
                                // RD 20141014 [20394] Add columns CBCALCMETHOD and CBIDC
                                // PL 20191105 Il faudra gérer IDB
                                sqlQuery = new StrBuilder();
                                sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.CASHBALANCE.ToString();
                                sqlQuery += "(IDA,DTENABLED,DTDISABLED,DTINS,IDAINS,IDB,ISUSEAVAILABLECASH,CASHANDCOLLATERAL,EXCHANGEIDC,CBSCOPE,ISMANAGEMENTBALANCE,CBCALCMETHOD,CBIDC,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                sqlQuery += SQLCst.SELECT + "a.IDA,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                sqlQuery += "b.IDB,cb.ISUSEAVAILABLECASH,cb.CASHANDCOLLATERAL,cb.EXCHANGEIDC,cb.CBSCOPE,cb.ISMANAGEMENTBALANCE,cb.CBCALCMETHOD,cb.CBIDC,cb.EXTLLINK,cb.ROWATTRIBUT" + Cst.CrLf;
                                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.CASHBALANCE.ToString() + " cb" + Cst.CrLf;
                                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on b.IDA=a.IDA and b.IDB=cb.IDB" + Cst.CrLf;
                                sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                sqlQuery += SQLCst.AND + "cb.IDA=" + IdA_Template.ToString();

                                nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                #endregion

                                #region ACTORROLE
                                // FI 20141223 [20598] Duplication des rôles  
                                // Si le formulaire affiche les rôles il n'y a pas de duplication des rôles. Aujourd'hui le formulaire affiche les rôles uniquement si l'utilisateur est SYSADMIN
                                if (false == Referential.RoleTableNameSpecified)
                                {
                                    sqlQuery = new StrBuilder();
                                    sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ACTORROLE.ToString();
                                    sqlQuery += "(IDA,IDROLEACTOR,IDA_ACTOR,IDSTCHECK,IDSTMATCH,DTENABLED,DTDISABLED,DTINS,IDAINS,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                    sqlQuery += SQLCst.SELECT + "a.IDA,ar.IDROLEACTOR,ar.IDA_ACTOR,ar.IDSTCHECK,ar.IDSTMATCH,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,ar.EXTLLINK,ar.ROWATTRIBUT" + Cst.CrLf;
                                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                    sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar" + Cst.CrLf;
                                    sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                    sqlQuery += SQLCst.AND + "ar.IDA=" + IdA_Template.ToString();
                                    if (SessionTools.User.UserType == RoleActor.SYSADMIN)
                                        sqlQuery += string.Empty;
                                    else if (SessionTools.User.UserType == RoleActor.SYSOPER)
                                        sqlQuery += SQLCst.AND + "ar.IDROLEACTOR not in ('SYSADMIN')";
                                    else if (SessionTools.User.UserType == EFS.Actor.RoleActor.USER)
                                        sqlQuery += SQLCst.AND + "ar.IDROLEACTOR not in ('SYSADMIN','SYSOPER')";

                                    nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                }
                                #endregion

                                #region BOOK
                                object obj;
                                string aTpl_identifier = DataHelper.SQLString(string.Empty);
                                string bookIdentifier = DataHelper.SQLString(string.Empty);
                                if (!String.IsNullOrEmpty(Request.QueryString["NBV"]))
                                {
                                    bookIdentifier = DataHelper.SQLString(Request.QueryString["NBV"]);//NBV:New Book Value

                                    sqlQuery = new StrBuilder();
                                    sqlQuery += SQLCst.SELECT + "IDENTIFIER" + Cst.CrLf;
                                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + Cst.CrLf;
                                    sqlQuery += SQLCst.WHERE + "IDA=" + IdA_Template.ToString();
                                    obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, sqlQuery.ToString());
                                    if (null != obj)
                                        aTpl_identifier = DataHelper.SQLString(Convert.ToString(obj));
                                }

                                int bookCount = 0;
                                sqlQuery = new StrBuilder();
                                sqlQuery += SQLCst.SELECT + SQLCst.COUNT_ALL + Cst.CrLf;
                                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.BOOK.ToString() + Cst.CrLf;
                                sqlQuery += SQLCst.WHERE + "IDA=" + IdA_Template.ToString();
                                obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, sqlQuery.ToString());
                                if (null != obj)
                                    bookCount = Convert.ToInt32(obj);

                                if (bookCount > 0)
                                {
                                    // FI 20141223 [XXXXX] 
                                    // {a} pour les colonnes renseignées à partir de l'acteur Source
                                    // {b} pour les colonnes renseignées à partir des books Source
                                    // RD 20151103 [XXXXX] use "{a}.DTENABLED" instead "a.DTENABLED"
                                    string column = @"{a}.IDA,{a}.DTENABLED,{a}.DTDISABLED,{a}.DTINS,{a}.IDAINS,
{b}.IDENTIFIER,{b}.DISPLAYNAME,{b}.DESCRIPTION,
{b}.IDA_ENTITY,{b}.IDC,
{b}.IDA_STLOFFICE,
{b}.IDA_CONTACTOFFICE,
{b}.IDA_INVOICEDOFFICE,
{b}.IDA_REGOFFICE,
{b}.IDA_DECISIONOFFICE,
{b}.LOCALCLASSDERV,{b}.ISULOCALCLASSDERV,{b}.IASCLASSDERV,{b}.ISUIASCLASSDERV,{b}.HEDGECLASSDERV,{b}.ISUHEDGECLASSDERV,{b}.FXCLASS,{b}.ISUFXCLASS,
{b}.LOCALCLASSNDRV,{b}.ISULOCALCLASSNDRV,{b}.IASCLASSNDRV,{b}.ISUIASCLASSNDRV,{b}.HEDGECLASSNDRV,{b}.ISUHEDGECLASSNDRV,
{b}.SECPOSEFFECT,{b}.ISUSECPOSEFFECT_DEPRECATED,{b}.FUTPOSEFFECT,{b}.ISUFUTPOSEFFECT_DEPRECATED,
{b}.OTCPOSEFFECT,{b}.OPTPOSEFFECT,
{b}.MTMMETHOD,{b}.ACCRUEDINTMETHOD,{b}.ACCRUEDINTPERIOD,
{b}.LINEARDEPPERIOD,{b}.ISRECEIVENCMSG,{b}.ISUOPTPOSEFFECT_DEPRECATED,
{b}.ISPOSKEEPING,{b}.ISFUTCLEARINGEOD,{b}.ISOPTCLEARINGEOD,{b}.VRFEE,
{b}.ISFUTPOSEFCTIGNORE,{b}.ISOPTPOSEFCTIGNORE,
{b}.EXTLLINK,{b}.EXTLLINK2,{b}.ROWATTRIBUT";

                                    string insColumn = column.Replace("{a}.", string.Empty).Replace("{b}.", string.Empty);
                                    string selColumn = column.Replace("{a}.", "a.").Replace("{b}.", "b_acttpl.");
                                    selColumn = selColumn.Replace("b_acttpl.IDA_STLOFFICE", "case when b_acttpl.IDA_STLOFFICE     =b_acttpl.IDA then a.IDA else b_acttpl.IDA_STLOFFICE end");
                                    selColumn = selColumn.Replace("b_acttpl.IDA_CONTACTOFFICE", "case when b_acttpl.IDA_CONTACTOFFICE =b_acttpl.IDA then a.IDA else b_acttpl.IDA_CONTACTOFFICE end");
                                    selColumn = selColumn.Replace("b_acttpl.IDA_INVOICEDOFFICE", "case when b_acttpl.IDA_INVOICEDOFFICE=b_acttpl.IDA then a.IDA else b_acttpl.IDA_INVOICEDOFFICE end");
                                    selColumn = selColumn.Replace("b_acttpl.IDA_REGOFFICE", "case when b_acttpl.IDA_REGOFFICE     =b_acttpl.IDA then a.IDA else b_acttpl.IDA_REGOFFICE end");
                                    selColumn = selColumn.Replace("b_acttpl.IDA_DECISIONOFFICE", "case when b_acttpl.IDA_DECISIONOFFICE=b_acttpl.IDA then a.IDA else b_acttpl.IDA_DECISIONOFFICE end");

                                    sqlQuery = new StrBuilder();
                                    sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.BOOK.ToString();
                                    sqlQuery += "(" + insColumn + ")" + Cst.CrLf;
                                    sqlQuery += SQLCst.SELECT + selColumn + Cst.CrLf;
                                    sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                    sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b_acttpl" + Cst.CrLf;
                                    sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_acttpl on a_acttpl.IDA=b_acttpl.IDA" + Cst.CrLf;
                                    sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                    sqlQuery += SQLCst.AND + "b_acttpl.IDA=" + IdA_Template.ToString();

                                    //WARNING: Save_RepositoryFromTemplate() doit impérativement êter appeler avant la region "BOOK update" ci-dessous
                                    nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery, Cst.OTCml_TBL.BOOK, IdA, IdA_Template);

                                    if (nRows > 0)
                                    {
                                        #region BOOKPOSEFCT
                                        sqlQuery = new StrBuilder();
                                        sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.BOOKPOSEFCT.ToString();
                                        sqlQuery += "(IDB,DTENABLED,DTDISABLED,DTINS,IDAINS,";
                                        sqlQuery += "TYPEINSTR,IDINSTR,TYPECONTRACT,IDCONTRACT,ASSETCATEGORY,FUTPOSEFFECT,OPTPOSEFFECT,ISFUTPOSEFCTIGNORE,ISOPTPOSEFCTIGNORE,ISFUTCLEARINGEOD,ISOPTCLEARINGEOD,";
                                        sqlQuery += "EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                        sqlQuery += SQLCst.SELECT + "b.IDB,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                        sqlQuery += "bpe.TYPEINSTR,bpe.IDINSTR,bpe.TYPECONTRACT,bpe.IDCONTRACT,bpe.ASSETCATEGORY,bpe.FUTPOSEFFECT,bpe.OPTPOSEFFECT,bpe.ISFUTPOSEFCTIGNORE,bpe.ISOPTPOSEFCTIGNORE,bpe.ISFUTCLEARINGEOD,bpe.ISOPTCLEARINGEOD,";
                                        sqlQuery += "bpe.EXTLLINK,bpe.ROWATTRIBUT" + Cst.CrLf;
                                        sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                        sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on b.IDA=a.IDA" + Cst.CrLf;
                                        sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b_acttpl on b_acttpl.IDENTIFIER=b.IDENTIFIER" + Cst.CrLf;
                                        sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOKPOSEFCT.ToString() + " bpe on bpe.IDB=b_acttpl.IDB" + Cst.CrLf;
                                        sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                        sqlQuery += SQLCst.AND + "b_acttpl.IDA=" + IdA_Template.ToString();

                                        nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                        #endregion

                                        #region BOOKG
                                        sqlQuery = new StrBuilder();
                                        sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.BOOKG.ToString();
                                        sqlQuery += "(IDB,DTENABLED,DTDISABLED,DTINS,IDAINS,";
                                        sqlQuery += "IDGBOOK," + Cst.CrLf;
                                        sqlQuery += "EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                        sqlQuery += SQLCst.SELECT + "b.IDB,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                        sqlQuery += "bg.IDGBOOK,";
                                        sqlQuery += "bg.EXTLLINK,bg.ROWATTRIBUT" + Cst.CrLf;
                                        sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                        sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on b.IDA=a.IDA" + Cst.CrLf;
                                        sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b_acttpl on b_acttpl.IDENTIFIER=b.IDENTIFIER" + Cst.CrLf;
                                        sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOKG.ToString() + " bg on bg.IDB=b_acttpl.IDB" + Cst.CrLf;
                                        sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                        sqlQuery += SQLCst.AND + "b_acttpl.IDA=" + IdA_Template.ToString();

                                        nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                        #endregion

                                        #region BOOK update
                                        // PL 20191105 Newness
                                        //WARNING: cetet region doit impérativement êter appeler après Save_RepositoryFromTemplate() ci-dessus
                                        if (!String.IsNullOrEmpty(bookIdentifier))
                                        {
                                            if (bookCount == 1)
                                            {
                                                //Utilisation de l'identifiant saisie pour le book pour se substituer, le cas échéant, au keyword représentant l'identifiant de l'acteur Template, 
                                                //Sinon utilisation de l'identifiant saisie pour le book 
                                                //Warning, l'ordre des colonnes est important
                                                sqlQuery = new StrBuilder();
                                                sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.BOOK.ToString() + Cst.CrLf;
                                                sqlQuery += "set DESCRIPTION=replace(replace(DESCRIPTION," + aTpl_identifier + "," + bookIdentifier + "),IDENTIFIER," + bookIdentifier + ")," + Cst.CrLf;
                                                sqlQuery += "    DISPLAYNAME=replace(replace(DISPLAYNAME," + aTpl_identifier + "," + bookIdentifier + "),IDENTIFIER," + bookIdentifier + ")," + Cst.CrLf;
                                                sqlQuery += "    IDENTIFIER =replace(replace(IDENTIFIER, " + aTpl_identifier + "," + bookIdentifier + "),IDENTIFIER," + bookIdentifier + ")" + Cst.CrLf;
                                            }
                                            else
                                            {
                                                //Utilisation de l'identifiant saisie pour le book pour se substituer au keyword représentant l'identifiant de l'acteur Template 
                                                //Warning, l'ordre des colonnes est important
                                                sqlQuery = new StrBuilder();
                                                sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.BOOK.ToString() + Cst.CrLf;
                                                sqlQuery += "set DESCRIPTION=replace(DESCRIPTION," + aTpl_identifier + "," + bookIdentifier + ")," + Cst.CrLf;
                                                sqlQuery += "    DISPLAYNAME=replace(DISPLAYNAME," + aTpl_identifier + "," + bookIdentifier + ")," + Cst.CrLf;
                                                sqlQuery += "    IDENTIFIER =replace(IDENTIFIER, " + aTpl_identifier + "," + bookIdentifier + ")" + Cst.CrLf;
                                            }
                                            sqlQuery += SQLCst.WHERE + "IDA=" + IdA.ToString();

                                            if (pDbTransaction == null)
                                            {
                                                nRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery.ToString());
                                            }
                                            else
                                            {
                                                nRows = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery.ToString());
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                #endregion

                                #region INCIRECEIVE
                                // PL 20191105 Newness - Reste à gérer IDA_TRADER
                                sqlQuery = new StrBuilder();
                                sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.INCI.ToString();
                                sqlQuery += "(IDA_CONTACTOFFICE,DTENABLED,DTDISABLED,DTINS,IDAINS," + Cst.CrLf;
                                sqlQuery += "IDB," + Cst.CrLf;
                                sqlQuery += "PRIORITYRANK,IDCNFMESSAGE,STEPLIFE,IDA_NCS,SIDE,GPRODUCT,";
                                sqlQuery += "ISTOSEND,ISTORECEIVE,";
                                sqlQuery += "IDA_TRADER,CNFTYPE,RECIPIENTTYPE,SENDDEFAULT_CLIENT,SENDDEFAULT_EXTCTR,";
                                sqlQuery += "TYPEINSTR,TYPEINSTR_UNL,IDINSTR,IDINSTR_UNL,IDC,IDC2,MSGTYPE,IDSTBUSINESS,TYPECONTRACT,IDCONTRACT,";
                                sqlQuery += "SENDDEFAULT_ENTITY,IDSTCHECK,IDSTMATCH,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                sqlQuery += SQLCst.SELECT + "a.IDA,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                sqlQuery += "isnull(b.IDB,inci.IDB) as IDB,";
                                sqlQuery += "inci.PRIORITYRANK,inci.IDCNFMESSAGE,inci.STEPLIFE,inci.IDA_NCS,inci.SIDE,inci.GPRODUCT,";
                                sqlQuery += "inci.ISTOSEND,inci.ISTORECEIVE,";
                                sqlQuery += "inci.IDA_TRADER,inci.CNFTYPE,inci.RECIPIENTTYPE,inci.SENDDEFAULT_CLIENT,inci.SENDDEFAULT_EXTCTR,";
                                sqlQuery += "inci.TYPEINSTR,inci.TYPEINSTR_UNL,inci.IDINSTR,inci.IDINSTR_UNL,inci.IDC,inci.IDC2,inci.MSGTYPE,inci.IDSTBUSINESS,inci.TYPECONTRACT,inci.IDCONTRACT,";
                                sqlQuery += "inci.SENDDEFAULT_ENTITY,inci.IDSTCHECK,inci.IDSTMATCH,inci.EXTLLINK,inci.ROWATTRIBUT" + Cst.CrLf;
                                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.INCI.ToString() + " inci" + Cst.CrLf;
                                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " bTpl on bTpl.IDB=inci.IDB" + Cst.CrLf;
                                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on b.IDA=a.IDA and b.IDENTIFIER=replace(replace(bTpl.IDENTIFIER, " + aTpl_identifier + "," + bookIdentifier + "),bTpl.IDENTIFIER," + bookIdentifier + ")" + Cst.CrLf;
                                sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                sqlQuery += SQLCst.AND + "inci.IDA_CONTACTOFFICE=" + IdA_Template.ToString();

                                nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                #endregion 

                                #region ADDRESSCOMPL
                                // PL 20191105 Newness - Reste à gérer ADDRESS1
                                sqlQuery = new StrBuilder();
                                sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ADDRESSCOMPL.ToString();
                                sqlQuery += "(IDA,DTENABLED,DTDISABLED,DTINS,IDAINS," + Cst.CrLf;
                                sqlQuery += "IDGINSTR,IDP,IDI,GPRODUCT,ADDRESSIDENT,TELEPHONENUMBER,MOBILEPHONENUMBER,FAXNUMBER,TELEXNUMBER,MAIL,WEB," + Cst.CrLf;
                                sqlQuery += "ADDRESS1,ADDRESS2,ADDRESS3,ADDRESS4,ADDRESS5,ADDRESS6,ADDRESSCITY,ADDRESSCOUNTRY,ADDRESSSTATE,ADDRESSPOSTALCODE," + Cst.CrLf;
                                sqlQuery += "CULTURE,CULTURE_CNF,IDC_CNF,EXTLLINK,ROWATTRIBUT)" + Cst.CrLf;
                                sqlQuery += SQLCst.SELECT + "a.IDA,a.DTENABLED,a.DTDISABLED,a.DTINS,a.IDAINS,";
                                sqlQuery += "acpl.IDGINSTR,acpl.IDP,acpl.IDI,acpl.GPRODUCT,acpl.ADDRESSIDENT,acpl.TELEPHONENUMBER,acpl.MOBILEPHONENUMBER,acpl.FAXNUMBER,acpl.TELEXNUMBER,acpl.MAIL,acpl.WEB,";
                                sqlQuery += "acpl.ADDRESS1,acpl.ADDRESS2,acpl.ADDRESS3,acpl.ADDRESS4,acpl.ADDRESS5,acpl.ADDRESS6,acpl.ADDRESSCITY,acpl.ADDRESSCOUNTRY,acpl.ADDRESSSTATE,acpl.ADDRESSPOSTALCODE,";
                                sqlQuery += "acpl.CULTURE,acpl.CULTURE_CNF,acpl.IDC_CNF,acpl.EXTLLINK,acpl.ROWATTRIBUT" + Cst.CrLf;
                                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                                sqlQuery += SQLCst.CROSSJOIN_DBO + Cst.OTCml_TBL.ADDRESSCOMPL.ToString() + " acpl" + Cst.CrLf;
                                sqlQuery += SQLCst.WHERE + "a.IDA=" + IdA.ToString();
                                sqlQuery += SQLCst.AND + "acpl.IDA=" + IdA_Template.ToString();

                                nRows = Save_RepositoryFromTemplate(pDbTransaction, sqlQuery);
                                #endregion 
                            }
                        }
                    }
                }
                #endregion

                //******************************************************************************************************************************
                //Spécifique aux referentiels: SYNDICATIONFEED/SYNDICATIONITEM
                //******************************************************************************************************************************
                #region Spécifique aux referentiels: SYNDICATIONFEED/SYNDICATIONITEM
                if (Referential.TableName.StartsWith("SYNDICATION"))
                    ReferentialTools.UpdateSyndicationFeed();
                #endregion

                //******************************************************************************************************************************
                //Spécifique au referentiel: INSTRUMENT (Add Default Template for Instrument)
                //******************************************************************************************************************************
                #region Spécifique au referentiel: INSTRUMENT (Add Default Template for Instrument)
                if (Referential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString())
                {
                    IDbConnection dbConnection = null;
                    IDbTransaction dbTransaction = null;
                    // EG 20160404 Migration vs2013
                    // #warning 20050729 PL (temporaire, à revoir... insert template post new instr)
                    //	Création d'un template pour les éventuels nouveaux instruments, à partir
                    //  du template de l'instrument portant sur le même product et ayant le nom du product
                    try
                    {
                        // RD 20091116
                        // Création d'un template, pour tous les éventuels nouveaux instruments qui n'en possèdent pas un, en trois passes et en mode Transactionnel.
                        //
                        // çela consiste à affecter:
                        //      - pour un instrument (Instrument1), 
                        //      - lié à un product (Product1), 
                        //      - un template (Template1), lié à un Instrument(Instrument2), parmi éventuellement plusieurs instruments du product (Product1) :
                        //
                        // passNumber = 1: 
                        // ---------------
                        // c'est le même fonctionnment que l'existant, en admettant qu'il pourrait n'y avoir qu'un seul template vérifiant les deux conditions:
                        //    - Template1.IDENTIFIER = Product1.IDENTIFIER
                        //    - Template1.IDENTIFIER = Instrument2.IDENTIFIER
                        //
                        // passNumber = 2: 
                        // ---------------
                        // prendre le premier Template de tous les instruments liés au product Product1 vérifiant la condition suivante :
                        //    - Template1.IDENTIFIER = Product1.IDENTIFIER
                        //
                        // passNumber = 3: 
                        // ---------------
                        // prendre le premier Template de tous les templates de tous les instruments liés au product Product1
                        //    - Template1.IDENTIFIER = Product1.IDENTIFIER
                        //
                        dbConnection = DataHelper.OpenConnection(SessionTools.CS);
                        //
                        int nbPass = 3;
                        int nbRow = 0;
                        string sqlQuery;

                        string concat = DataHelper.GetConcatOperator(SessionTools.CS);
                        string colIdentifier = DataHelper.SQLSubstring(SessionTools.CS, "ns.IDENTIFIER" + concat + @"'(From Template '" + concat + @"tr.identifier" + concat + @"')'", 1, SQLCst.UT_IDENTIFIER_LEN);
                        string colDislayName = DataHelper.SQLSubstring(SessionTools.CS, "ns.DISPLAYNAME" + concat + @"'(From Template '" + concat + @"tr.identifier" + concat + @"')'", 1, SQLCst.UT_IDENTIFIER_LEN);
                        string colDate = DataHelper.SQLGetDate(SessionTools.CS);

                        for (int passNumber = 1; passNumber <= nbPass; passNumber++)
                        {
                            string var3 = string.Empty;
                            string var4 = string.Empty;
                            string var5 = string.Empty;

                            // Instrument du même nom que le Product
                            if (passNumber == 1)
                                var3 = " and nspr.IDENTIFIER = pr.IDENTIFIER";

                            // Instrument du même nom que le Product
                            if (passNumber == 1 || passNumber == 2)
                                var4 = " and tr.IDENTIFIER = pr.IDENTIFIER";

                            // Pour ne pouvoir sélectionner que le 1er Template parmi tous les Templates de tous les instruments du product 
                            // auquel est lié le nouvel instrument
                            if (passNumber == 2 || passNumber == 3)
                            {
                                string sqlQueryAllTemplate = @"select tr2.IDT
                                from dbo.TRADE tr2
                                inner join dbo.INSTRUMENT ns2 on (ns2.IDI = tr2.IDI)
                                inner join dbo.PRODUCT pr2 on (pr2.IDP = ns2.IDP) and (pr2.IDP = pr.IDP)
                                where (tr2.IDSTENVIRONMENT = 'TEMPLATE')";

                                // Pour des raisons de performances on duplique le test "Template du même nom que le Product"
                                if (passNumber == 2)
                                    sqlQueryAllTemplate += " and (tr2.IDENTIFIER = pr.IDENTIFIER)";

                                var5 = String.Format(@" and tr.IDT in ({0})", DataHelper.GetSelectTop(SessionTools.CS, sqlQueryAllTemplate, 1));
                            }

                            //"Begin Tran" doit être la 1ère instruction, car si Error un rollback est généré de manière systématique
                            dbTransaction = DataHelper.BeginTran(dbConnection);

                            // EG 20220404 [XXXXX][WI612] Split sqlQuery en SqlQuery et SqlFrom
                            #region Requête d'Insertion du Template
                            sqlQuery = String.Format(@"insert into dbo.TRADE (
                            IDI, IDENTIFIER, DISPLAYNAME, DESCRIPTION, DTTRADE, DTTIMESTAMP, SOURCE, DTSYS, 
                            IDSTENVIRONMENT, DTSTENVIRONMENT, IDASTENVIRONMENT, 
                            IDSTBUSINESS, DTSTBUSINESS, IDASTBUSINESS,
                            IDSTACTIVATION, DTSTACTIVATION, IDASTACTIVATION,
                            IDSTPRIORITY, DTSTPRIORITY, IDASTPRIORITY,
                            EXTLLINK, ROWATTRIBUT)
                            select ns.IDI, 
                            {0}, {1}, null as DESCRIPTION, {2} as DTTRADE, {2} as DTTIMESTAMP, tr.SOURCE, {2} as DTSYS, 
                            'TEMPLATE', {2}, 1, 'EXECUTED', {2}, 1, 'REGULAR', {2}, 1, 'REGULAR', {2}, 1,
                            null as EXTLLINK, 'X' as ROWATTRIBUT", colIdentifier, colDislayName, colDate);

                            // EG 20220404 [XXXXX][WI612] Réutilisation de l'instruction From/Join lors de l'insertion dans la table TRADEXML
                            // EG 20220404 [XXXXX][WI612] Introduction d'une jointure sur TRADEXML
                            string sqlFrom = Cst.CrLf + String.Format(@"from dbo.INSTRUMENT ns
                            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                            inner join dbo.INSTRUMENT nspr on (nspr.IDP = pr.IDP) {0}
                            inner join dbo.TRADE tr on (tr.IDI = nspr.IDI) {1}
                            inner join dbo.TRADEXML trxml on (trxml.IDT = tr.IDT)
                            where (tr.IDSTENVIRONMENT = 'TEMPLATE') and 
                            (ns.IDI not in (select IDI 
                                from dbo.TRADE tr 
                                inner join dbo.TRADEXML trxml on (trxml.IDT = tr.IDT)
                                where IDSTENVIRONMENT = 'TEMPLATE')
                            )
                            {2}", var3, var4, var5);

                            #endregion

                            nbRow = DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlQuery + sqlFrom);

                            if (nbRow > 0)
                            {
                                // EG 20220404 [XXXXX][WI612] Récupération de l'IDT nouvellement créé
                                sqlQuery = @"select tr.IDT
                                from dbo.TRADE tr
                                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                                left outer join dbo.TRADEXML trxml on (trxml.IDT = tr.IDT )
                                where (tr.ROWATTRIBUT ='X') and (trxml.IDT is null)";
                                object obj = DataHelper.ExecuteScalar(dbTransaction, CommandType.Text, sqlQuery.ToString());
                                if (null != obj)
                                {
                                    int idT = Convert.ToInt32(obj);
                                    if (idT > 0)
                                    {
                                        #region Insertion dans TRADETRAIL
                                        // EG 20220404 [XXXXX][WI612] Usage de l'IDT
                                        sqlQuery = "insert into TRADETRAIL (IDT, IDA, DTSYS, SCREENNAME, ACTION, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDTRADE_P, IDTRADEXML_P) values" +
                                        $"({idT}, {SessionTools.Collaborator_IDA}, { colDate}, 'Full', 'Create', '{SessionTools.ServerAndUserHost}', '{Software.Name}', '{Software.VersionBuild}', '{SessionTools.BrowserInfo}', null, null)";
                                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlQuery);
                                        #endregion

                                        #region Insertion dans TRADEXML
                                        // EG 20220404 [XXXXX][WI612] Usage de l'IDT et du SqlFrom global 
                                        sqlQuery = $"insert into dbo.TRADEXML ( IDT, TRADEXML, EFSMLVERSION) select {idT}, trxml.TRADEXML, trxml.EFSMLVERSION {sqlFrom}";
                                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, sqlQuery);
                                        #endregion
                                    }
                                }
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
                }
                #endregion Specifique au referentiel: INSTRUMENT (Add Default Template for Instrument)

                //******************************************************************************************************************************
                //Spécifique au referentiel: EFSOBJECTDET (Load and Execute Trigger's SQL scripts)
                //******************************************************************************************************************************
                #region Spécifique au referentiel: EFSOBJECTDET (Load and Execute Trigger's SQL scripts)
                if (isTriggerLoCommandToExecute)
                {
                    string sqlLoCommand = Referential.dataRow["LOCOMMAND"].ToString();
                    string commandName = (Convert.IsDBNull(Referential.dataRow["COMMANDNAME"]) ? string.Empty : Referential.dataRow["COMMANDNAME"].ToString());

                    try
                    {
                        DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlLoCommand);
                    }
                    catch (Exception e)
                    {
                        //JavaScript.AlertImmediate(this, Ressource.GetString("Msg_TriggerScriptNotExecuted") + (StrFunc.IsFilled(commandName) ? " (" + commandName + ")" : string.Empty) + ": " + Cst.CrLf + e.Message, false);
                        JavaScript.DialogImmediate(this, Ressource.GetString("Msg_TriggerScriptNotExecuted") + (StrFunc.IsFilled(commandName) ? " (" + commandName + ")" : string.Empty) + ": " + Cst.CrLf + e.Message, false, ProcessStateTools.StatusErrorEnum);
                        ret = Cst.ErrLevel.SQL_ERROR;
                    }
                }
                #endregion


                //******************************************************************************************************************************
                //Spécifique au referentiel: MATURITYRULE (Update existing trades in position)
                //******************************************************************************************************************************
                #region Spécifique au referentiel: MATURITYRULE (Update existing non-expired trades)
                if (Referential.TableName == Cst.OTCml_TBL.MATURITYRULE.ToString())
                {
                    // FI 20220511 [XXXXX] corrections dysfonctionnement et refactoring
                    WebControl ctrl = pSender as WebControl;
                    string eventTarget = Request.Params["__EVENTTARGET"];
                    string eventArgument = Request.Params["__EVENTARGUMENT"];
                    int idMaturityRule = Convert.ToInt32(Request.Params["PKV"]);
                    
                    JQuery.UI.WriteInitialisationScripts(this, new JQuery.Block(idMenu, "Msg_MaturityRuleUpdateInProgress", true)
                    {
                        Timeout = SystemSettings.GetTimeoutJQueryBlock("UpdateMATURITYRULE")
                    });

                    #region Mise à jour des dates maturités + évènements sur toutes les opérations concernées
                    // vérifier s'il s'agit d'une mise à jour de la méthode de calcul d'une règle d'échéance et non une nouvelle règle
                    if (!Referential.isNewRecord)
                    {
                        // au clic sur le bouton "Enregistrer", on contruit le contenu de la boite de dialogue en fonction des données récupérées sur la règle d'échéance en cours de modification
                        if (StrFunc.IsEmpty(eventArgument))
                        {
                            DateTime dtMinEntityMarket = MarketTools.GetMinDateMarket(CSTools.SetCacheOn(SessionTools.CS));

                            IEnumerable<int> dcMR = GetDerivativeContractsByMaturityRule(SessionTools.CS, idMaturityRule, dtMinEntityMarket);
                            
                            List<Tuple<ISpheresIdentification, List<Tuple<string, int>>>> lsTradesByDC = new List<Tuple<ISpheresIdentification, List<Tuple<string, int>>>>();
                            if (dcMR.Count() > 0)
                            {
                                List<Task<Tuple<ISpheresIdentification, List<Tuple<string, int>>>>> lstTaks = new List<Task<Tuple<ISpheresIdentification, List<Tuple<string, int>>>>>();
                                foreach (int idDC in dcMR)
                                    lstTaks.Add(GetCountOfLivingTradesETDAsync(SessionTools.CS, idDC, idMaturityRule, dtMinEntityMarket));

                                Task.WaitAll(lstTaks.ToArray());

                                foreach (var itemTask in lstTaks)
                                    lsTradesByDC.Add(itemTask.Result);
                            }

                            var dcWithOpenMaturity = from item in lsTradesByDC.Where(x => x.Item2.Where(y => y.Item2 > 0).Count()>0)
                                                     select item;
                            
                            string HTMLListOfTradesNotExpired = Cst.HTMLOrderedList;
                            if (dcWithOpenMaturity.Count() > 0)
                            {
                                foreach(Tuple<ISpheresIdentification, List<Tuple<string, int>>> item in dcWithOpenMaturity)
                                {
                                    HTMLListOfTradesNotExpired += Cst.HTMLListItem + item.Item1.Identifier + " (" + item.Item1.Displayname + ") : ";
                                    HTMLListOfTradesNotExpired += Cst.HTMLUnorderedList;
                                    foreach (var maturity in item.Item2)
                                        HTMLListOfTradesNotExpired += Cst.HTMLListItem + maturity.Item1 + " : " + maturity.Item2 + Cst.Space + Ressource.GetString("Trades") + Cst.HTMLEndListItem;
                                    HTMLListOfTradesNotExpired += Cst.HTMLEndUnorderedList + Cst.HTMLEndListItem;
                                }
                            }
                            HTMLListOfTradesNotExpired += Cst.HTMLEndOrderedList;

                            string msg;
                            string confirmationMessage = string.Empty;
                            if (dcMR.Count() == 0)
                            {
                                msg = Ressource.GetString("Msg_MRUDerivativeContractNoRecord");
                            }
                            else
                            {
                                confirmationMessage = (dcMR.Count() > 0) ? Ressource.GetString("Msg_MRUDerivativeContractNoLivingTrade") : string.Empty;
                                msg = Ressource.GetString2("Msg_MRUDerivativeContractRecap",
                                                            dcMR.Count().ToString(),
                                                            dcWithOpenMaturity.Count().ToString(),
                                                            HTMLListOfTradesNotExpired,
                                                            confirmationMessage) + Cst.CrLf;
                            }

                            if (StrFunc.IsFilled(msg) && (dcMR.Count() == 0 || confirmationMessage == string.Empty))
                                JavaScript.DialogImmediate(this, msg, true, ProcessStateTools.StatusSuccessEnum);
                            else if (StrFunc.IsFilled(msg))
                                JavaScript.ConfirmStartUpImmediate(this, msg, ctrl.ID, "TRUE", "FALSE");
                        }
                        // au clic sur le bouton "Oui" de la boite de dialogue Javascript, traitement
                        else if (eventArgument == "TRUE")
                        {
                            UpdateMaturityRuleProcess updateMaturityRuleProcess = new UpdateMaturityRuleProcess(SessionTools.CS, SessionTools.AppSession);
                            updateMaturityRuleProcess.UpdateMaturitiesAndEvents(idMaturityRule, out _);
                            JavaScript.DialogImmediate(this, Ressource.GetString("Msg_ProcessSuccessfull"), true, ProcessStateTools.StatusSuccessEnum);
                        }
                        // au clic sur le bouton "Non" de la boite de dialogue Javascript, on ferme la fenêtre
                        else if (eventArgument == "FALSE")
                        {
                            this.ClosePage();
                        }
                    }
                    #endregion
                }
                #endregion Spécifique au referentiel: MATURITYRULE (Update existing trades in position)


                //S'il s'agit d'une "Création" et qu'on ne ferme pas la fenêtre (Bouton "Apply"), on reload la page afin d'afficher les menus childs (Detail).
                if (!pIsSelfClose && Referential.isNewRecord)
                {
                    if (StrFunc.IsFilled(keyField) && StrFunc.IsFilled(keyFieldNewValue))
                    {
                        int pos1, pos2;
                        string url = this.Request.RawUrl;
                        url = url.Replace("&N=1", "&N=0");
                        url = url.Replace("&DPC=1", string.Empty);

                        pos1 = url.IndexOf("&PK=");
                        //PL 20110614 Add if() suite à refactoring de l'URL par FDA
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

                //Si selfclose ou si le référentiel a pour datakeyfield ROWVERSION ou si... : on impose la fermeture la fenêtre.
                //PL 20100322 Add DERIVATIVECONTRACT & ASSET_ETD for Trigger (A finaliser...)
                // EG 20220907 [XXXXX] Test pour raffraichissement automatique du tracker après marquage d'un ligne
                if (pIsSelfClose && Referential.TableName == Cst.OTCml_TBL.TRACKER_L.ToString())
                {
                    JavaScript.CallFunction(this, "OpenerRefreshAndClose(null,'LoadTracker')");
                }
                else if (pIsSelfClose
                    || Referential.IsROWVERSIONDataKeyField
                    || Referential.TableName == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()
                    || Referential.TableName == Cst.OTCml_TBL.ASSET_ETD.ToString())
                {
                    JavaScript.CallFunction(this, "OpenerRefreshAndClose(null,null)");
                }
                else
                {
                    JavaScript.CallFunction(this, "OpenerRefresh('ReferentialPage','SELFRELOAD_')");
                }
            }

            return ret;
        }

        private Cst.ErrLevel Save(IDbTransaction pDbTransaction, bool pIsSelfClose)
        {
            return Save(pDbTransaction, pIsSelfClose, null);
        }

        private int Save_RepositoryFromTemplate(IDbTransaction pDbTransaction, StrBuilder pSqlInsert)
        {
            return Save_RepositoryFromTemplate(pDbTransaction, pSqlInsert, Cst.OTCml_TBL.UNKNOW, 0, 0);
        }
        private int Save_RepositoryFromTemplate(IDbTransaction pDbTransaction, StrBuilder pSqlInsert, Cst.OTCml_TBL pTableName, int pIda, int pIda_Template)
        {
            StrBuilder sqlQuery_extlid = new StrBuilder();
            bool isExistExtlId = (pTableName != Cst.OTCml_TBL.UNKNOW);

            if (isExistExtlId)
            {
                string columName = OTCmlHelper.GetColunmID(pTableName.ToString());

                sqlQuery_extlid += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EXTLID.ToString();
                sqlQuery_extlid += "(TABLENAME,ID,IDENTIFIER,VALUE,DTINS,IDAINS)" + Cst.CrLf;
                sqlQuery_extlid += SQLCst.SELECT + "eid.TABLENAME,tgt." + columName + ",eid.IDENTIFIER,eid.VALUE," + Cst.CrLf;
                sqlQuery_extlid += "a.DTINS,a.IDAINS" + Cst.CrLf;
                sqlQuery_extlid += SQLCst.FROM_DBO + Cst.OTCml_TBL.EXTLID.ToString() + " eid" + Cst.CrLf;
                sqlQuery_extlid += SQLCst.INNERJOIN_DBO + pTableName.ToString() + " src on src." + columName + "=eid.ID and src.IDA=" + pIda_Template.ToString() + Cst.CrLf;
                sqlQuery_extlid += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a on a.IDA=" + pIda.ToString() + Cst.CrLf;
                sqlQuery_extlid += SQLCst.INNERJOIN_DBO + pTableName.ToString() + " tgt on tgt.IDA=a.IDA" + Cst.CrLf;
                sqlQuery_extlid += SQLCst.WHERE + "eid.TABLENAME=" + DataHelper.SQLString(pTableName.ToString());
            }

            int nRows;
            if (pDbTransaction == null)
            {
                //Insert main data
                nRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, pSqlInsert.ToString());
                if (isExistExtlId && nRows > 0)
                {
                    //Insert external data
                    nRows += DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, sqlQuery_extlid.ToString());
                }
            }
            else
            {
                //Insert main data
                nRows = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, pSqlInsert.ToString());
                if (isExistExtlId && nRows > 0)
                {
                    //Insert external data
                    nRows += DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery_extlid.ToString());
                }
            }

            return nRows;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReturnAndClose()
        {
            string dataKeyField = null;
            string keyField = null;
            string description = null;

            if (Referential.ExistsColumnDataKeyField)
                dataKeyField = Referential.dataRow[Referential.IndexColSQL_DataKeyField].ToString();
            if (Referential.ExistsColumnKeyField)
                keyField = Referential.dataRow[Referential.IndexColSQL_KeyField].ToString();
            if (Referential.ExistsColumnDISPLAYNAME)
                description = Referential.dataRow[Referential.IndexColSQL_DISPLAYNAME].ToString();
            else if (Referential.ExistsColumnDESCRIPTION)
                description = Referential.dataRow[Referential.IndexColSQL_DESCRIPTION].ToString();

            string display = string.Empty;
            if (dataKeyField != null)
                display += "  dataKeyField: " + dataKeyField;
            if (keyField != null)
                display += "  keyField: " + keyField;
            if (description != null)
                display += "  description: " + description;
            display = display.TrimStart();

            JavaScript.DialogImmediate(this, display, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRcc"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private static string GetWidthControlLinked(ReferentialsReferentialColumn pRcc)
        {
            string inputWidth = string.Empty;
            if (StrFunc.IsFilled(pRcc.InputWidth))
                inputWidth = @" width=""" + pRcc.InputWidth + @"""";
            else if (pRcc.IsLastControlLinked)
                inputWidth = "99%";
            return inputWidth;
        }

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

        /// <summary>
        /// Retourne le menu IOTRACKCOMPARE à afficher en fonction du type comparaison
        /// </summary>
        /// <param name="pMenu">Représente le menu IOTRACKCOMPARE générique</param>
        /// <param name="pRow">Représente les données affichées ds le formulaire</param>
        /// <returns></returns>
        private static SpheresMenu.Menu GetSpecificMenuForIOTRACKCompare(SpheresMenu.Menu pMenu, DataRow pRow)
        {
            SpheresMenu.Menu ret = (SpheresMenu.Menu)pMenu.Clone();
            //            
            if ((null == pRow["ELEMENTSUBTYPE"]))
                throw new Exception("Column ELEMENTSUBTYPE doesn't exist");
            //
            string elementSubType = pRow["ELEMENTSUBTYPE"].ToString();
            if (StrFunc.IsFilled(elementSubType))
            {
                if (false == (Enum.IsDefined(typeof(EFS.SpheresIO.CompareOptions), elementSubType)))
                    throw new Exception(StrFunc.AppendFormat("{0} is not defined in enum EFS.SpheresIO.CompareOptions", elementSubType));
                //
                CompareOptions compareOption = (CompareOptions)Enum.Parse(typeof(CompareOptions), elementSubType);
                //
                string shortName = CompareOptionsAttribute.ConvertToString(compareOption);
                //
                if (StrFunc.IsFilled(shortName))
                    ret.Url = ret.Url.Replace("IOTRACKCOMPARE", StrFunc.AppendFormat("IOTRACKCOMPARE_{0}", shortName.ToUpper()));
            }
            else
            {
                //Compatibilté ascendante usage de CompareOptions.Trades ainsi Spheres® utilisera le fichier IOTRACKCOMPARE_TRADE.xml 
                //Jusqu'à présent spheres ne permettait que des comparisons de Trade
                ret.Url = ret.Url.Replace("IOTRACKCOMPARE", StrFunc.AppendFormat("IOTRACKCOMPARE_{0}", CompareOptionsAttribute.ConvertToString(CompareOptions.Trades).ToUpper()));
            }
            //
            return ret;
        }

        /// <summary>
        /// </summary>
        /// <param name="pMenu">Menu générique</param>
        /// <param name="pRow">Données affichées dans le formulaire</param>
        /// <returns></returns>
        private static SpheresMenu.Menu AddFAMILYAndGPRODUCTToSpecificMenu(SpheresMenu.Menu pMenu, DataRow pRow)
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
                //PL 20160914
                string da = "&DA=";
                da += "[[Data name='FAMILY' datatype='string']]" + family + "[[/Data]];";
                da += "[[Data name='GPRODUCT' datatype='string']]" + gProduct + "[[/Data]]";

                if (ret.Url.Contains("&DA="))
                    ret.Url = ret.Url.Replace("&DA=", da);
                else
                    ret.Url += da;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTarget"></param>
        /// <param name="pArguments"></param>
        private void ApplyOffSet(string pTarget, string pArguments)
        {
            // target[1] = table de lecture
            // args[0] = ID de la table de lecture
            // args[1] = Contrôle Date source du calcul
            // args[2] = Contrôle Date destinataire du résultat
            string[] targets = pTarget.Split(';');
            if ((1 < targets.Length) && Enum.IsDefined(typeof(Cst.OTCml_TBL), targets[1]))
            {
                Cst.OTCml_TBL table = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), targets[1]);
                string[] args = pArguments.Split(';');
                if (0 < args.Length)
                {
                    string ID;
                    if ((this.FindControl(args[0]) is TextBox txtID) && StrFunc.IsFilled(txtID.Text))
                        ID = txtID.Text;
                    else
                        ID = Request.QueryString["FKV"];

                    if (StrFunc.IsFilled(ID))
                    {
                        DataSet ds = null;
                        switch (table)
                        {
                            case Cst.OTCml_TBL.ASSET_EQUITY:
                                string sqlSelect = SQLCst.SELECT + "PERIODMLTPEXDIVOFFSET as MULTIPLIER,";
                                sqlSelect += "PERIODEXDIVOFFSET as PERIOD,";
                                sqlSelect += "DAYTYPEEXDIVOFFSET as DAYTYPE,";
                                sqlSelect += "IDC as IDC" + Cst.CrLf;
                                sqlSelect += SQLCst.FROM_DBO + table + Cst.CrLf;
                                sqlSelect += SQLCst.WHERE + "(IDASSET=" + ID + ")" + Cst.CrLf;
                                //PL 20210122 Add test on null values
                                sqlSelect += SQLCst.AND + "(PERIODMLTPEXDIVOFFSET is not null and PERIODEXDIVOFFSET is not null and DAYTYPEEXDIVOFFSET is not null)";
                                ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect);
                                break;
                        }
                        if ((ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].Rows != null) && (ds.Tables[0].Rows.Count == 1))
                        {
                            int multiplier = Convert.ToInt32(ds.Tables[0].Rows[0]["MULTIPLIER"]);
                            PeriodEnum period = StringToEnum.Period(Convert.ToString(ds.Tables[0].Rows[0]["PERIOD"]));
                            DayTypeEnum dayType = StringToEnum.DayType(Convert.ToString(ds.Tables[0].Rows[0]["DAYTYPE"]));
                            string idC = Convert.ToString(ds.Tables[0].Rows[0]["IDC"]);

                            DateTime dtSource = Convert.ToDateTime(null);
                            if ((this.FindControl(args[1]) is TextBox txtDtSource) && StrFunc.IsFilled(txtDtSource.Text))
                                dtSource = Convert.ToDateTime(txtDtSource.Text);

                            if (DtFunc.IsDateTimeFilled(dtSource))
                            {
                                IProductBase productBase = Tools.GetNewProductBase();
                                IOffset offset = productBase.CreateOffset(period, multiplier, dayType);
                                DateTime dtResult = Tools.ApplyOffsetCurrency(SessionTools.CS, dtSource, offset, idC);
                                if (this.FindControl(args[2]) is TextBox txtDtResult)
                                    txtDtResult.Text = DtFunc.DateTimeToString(dtResult, DtFunc.FmtShortDate);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHRTitleEnum"></param>
        /// <param name="pTitle"></param>
        /// <param name="pIsHR_Visible"></param>
        /// <returns></returns>
        private Boolean DisplayHRByProduct(HRTitle pHRTitleEnum, string pTitle, bool pIsHR_Visible)
        {
            bool isHRVisible = pIsHR_Visible;
            if (StrFunc.IsFilled(pTitle) && (pTitle.Contains(pHRTitleEnum.ToString()) || (!isHRVisible)))
            {
                string family = string.Empty;
                if ((null != Referential["FAMILY"]) && (null != Referential.dataRow["FAMILY"]))
                    family = Referential.dataRow["FAMILY"].ToString();
                else if (Referential.dynamicArgsSpecified && (null != Referential.dynamicArgs["FAMILY"]))
                    family = Referential.dynamicArgs["FAMILY"].value;
                string gProduct = string.Empty;
                if ((null != Referential["GPRODUCT"]) && (null != Referential.dataRow["GPRODUCT"]))
                    gProduct = Referential.dataRow["GPRODUCT"].ToString();
                else if (Referential.dynamicArgsSpecified && (null != Referential.dynamicArgs["GPRODUCT"]))
                    gProduct = Referential.dynamicArgs["GPRODUCT"].value;
                switch (pHRTitleEnum)
                {
                    case HRTitle.AssetEnv:
                        if (!pTitle.Contains(pHRTitleEnum.ToString()))
                            isHRVisible = true;
                        else if (Referential.dynamicArgsSpecified && (null != Referential.dynamicArgs["FAMILY"]))
                            isHRVisible = (pTitle == family + pHRTitleEnum.ToString() + "_Title");
                        else if ((pTitle == "Empty" && (!isHRVisible)))
                            isHRVisible = false;
                        else
                            isHRVisible = true;
                        break;
                    case HRTitle.ValidationRules:
                        if ((null != Referential["IDP"]) && ("PRODUCT_IDENTIFIER" == Referential["IDP"].IDForItemTemplateRelation))
                        {
                            string product = Referential.dataRow["PRODUCT_IDENTIFIER"].ToString();
                            switch (pTitle)
                            {
                                case "IRDValidationRules_Title":
                                    isHRVisible = ("IRD" == family) || (("DSE" == family) && ("SEC" == gProduct));
                                    break;
                                case "FXValidationRules_Title":
                                    isHRVisible = ("FX" == family);
                                    break;
                                case "OptionValidationRules_Title":
                                    if (product.EndsWith("Option") || (product == "swaption"))
                                        isHRVisible = true;
                                    else
                                        isHRVisible = false;
                                    break;
                                case "DSEASSETValidationRules_Title":
                                    isHRVisible = ("DSE" == family) && ("ASSET" == gProduct);
                                    break;
                                case "DSESECValidationRules_Title":
                                    isHRVisible = ("DSE" == family) && ("SEC" == gProduct);
                                    break;
                                case "RepoValidationRules_Title":
                                    switch (product)
                                    {
                                        case "buyAndSellBack":
                                        case "securityLending":
                                        case "repo":
                                            isHRVisible = true;
                                            break;
                                        default:
                                            isHRVisible = false;
                                            break;
                                    }
                                    break;
                                default:
                                    isHRVisible = (pTitle != "Empty") || isHRVisible;
                                    break;
                            }
                        }
                        break;
                }
            }
            return isHRVisible;
        }

        #region GetCountOfLivingTradesETD
        /// <summary>
        /// Retourne le nombre de trades par échéances ouvertes sur le DC <paramref name="idDC"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="idDC">Représente le DC</param>
        /// <param name="idMR">permet de filtrer uniquement sur les échéances ouvertes de la maturityRule</param>
        /// <param name="dtBusiness"></param>
        /// <returns>le DC et une liste de couple échéance/Nbr de trade sur l'échéance</returns>
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        public static Tuple<ISpheresIdentification, List<Tuple<string, int>>> GetCountOfLivingTradesETD(string cs, int idDC, Nullable<int> idMR, DateTime dtBusiness)
        {
            Tuple<ISpheresIdentification, List<Tuple<string, int>>> ret = new Tuple<ISpheresIdentification, List<Tuple<string, int>>>(new SpheresIdentification(), new List<Tuple<string, int>>());

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDDC), idDC);
            if (idMR.HasValue)
                parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDMATURITYRULE), idMR.Value);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + Cst.CrLf;
            sqlQuery += "dc.IDDC, dc.IDENTIFIER, dc.DISPLAYNAME,";
            sqlQuery += "ma.MATURITYMONTHYEAR, ";
            sqlQuery += SQLCst.COUNT_1 + "as TRADECOUNT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ASSET_ETD + " a" + SQLCst.ON + "a.IDASSET = tr.IDASSET" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB + " da" + SQLCst.ON + "da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + SQLCst.ON + "dc.IDDC = da.IDDC" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "da.IDDC = @IDDC" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MATURITY + " ma" + SQLCst.ON + "ma.IDMATURITY = da.IDMATURITY" + Cst.CrLf;
            if (idMR.HasValue)
                sqlQuery += SQLCst.AND + "ma.IDMATURITYRULE = @IDMATURITYRULE" + Cst.CrLf;

            if (DtFunc.IsDateTimeFilled(dtBusiness))
            {
                sqlQuery += SQLCst.AND + "(ma.MATURITYDATE is null or ma.MATURITYDATE >= @DT)" + Cst.CrLf;
                parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DT), dtBusiness); // FI 20201006 [XXXXX] DbType.Date
            }
            sqlQuery += SQLCst.GROUPBY + "dc.IDDC, dc.IDENTIFIER, dc.DISPLAYNAME, ma.MATURITYMONTHYEAR";

            QueryParameters qryParameters = new QueryParameters(cs, sqlQuery.ToString(), parameters);
            DataTable dt = DataHelper.ExecuteDataTable(cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (dt.Rows.Count > 0)
            {
                ret.Item1.OTCmlId = Convert.ToInt32(dt.Rows[0]["IDDC"]);
                ret.Item1.Identifier = dt.Rows[0]["IDENTIFIER"].ToString();
                ret.Item1.Displayname = dt.Rows[0]["DISPLAYNAME"].ToString();

                foreach (DataRow row in dt.Rows)
                    ret.Item2.Add(new Tuple<string, int>(row["MATURITYMONTHYEAR"].ToString(), Convert.ToInt32(row["TRADECOUNT"])));
            }
            return ret;
        }

        /// <summary>
        /// Retourne le nombre de trades par échéances ouvertes sur le DC <paramref name="idDC"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="idDC">Représente le DC</param>
        /// <param name="idMR">permet de filtrer uniquement sur les échéances ouvertes de la maturityRule</param>
        /// <param name="dtBusiness"></param>
        /// <returns></returns>
        public static Task<Tuple<ISpheresIdentification, List<Tuple<string, int>>>> GetCountOfLivingTradesETDAsync(string cs, int idDC, Nullable<int> idMR, DateTime dtBusiness)
        {
            return Task.Run<Tuple<ISpheresIdentification, List<Tuple<string, int>>>>(() =>
            {
                return GetCountOfLivingTradesETD(cs, idDC, idMR, dtBusiness);
            });
        }

        #endregion GetCountOfLivingTradesETD

        #region GetDerivativeContractsByMaturityRule
        /// <summary>
        /// Retourne les DC actif en <paramref name="dtBusiness"/> qui utilise la règle d'échéance <paramref name="idMR"/> 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="idMR"></param>
        /// <param name="dtBusiness"></param>
        /// <returns></returns>
        private static IEnumerable<int> GetDerivativeContractsByMaturityRule(string cs, int idMR, DateTime dtBusiness)
        {
            
            DataDCMREnabled dataDerivativeContractMR = new DataDCMREnabled(cs, null, dtBusiness);
            List<int> ret = dataDerivativeContractMR.LoadDCUsingMR(idMR).ToList();


//            string sqlQuery = $@"select distinct dc.IDDC
//from dbo.VW_DRVCONTRACTMATRULE dcMR 
//inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = dcMR.IDDC and {OTCmlHelper.GetSQLDataDtEnabled(cs, "dc", dtBusiness)}
//where dcMR.IDMATURITYRULE = @IDMATURITYRULE ";

//            DataParameters parameters = new DataParameters();
//            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDMATURITYRULE), idMR);
//            QueryParameters qryParameters = new QueryParameters(cs, sqlQuery, parameters);

//            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
//            {
//                while (dr.Read())
//                {
//                    ret.Add(Convert.ToInt32(dr["IDDC"]));
//                }
//            }
                
            return ret;
        }
        #endregion GetDerivativeContractsByMaturityRule

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        /// FI 20220503 [XXXXX] Add
        private string GetURLMenuAction(SpheresMenu.Menu menu)
        {
            //PL 20160914 Use GetMenu_Shortname2() instead of GetMenu_Shortname() 
            string subTitle = Ressource.GetMenu_Shortname2(menu.IdMenu_Parent, menu.IdMenu_Parent);
            string subTitleData = Referential.dataRow[Referential.IndexColSQL_KeyField].ToString();
            if (Referential.ExistsColumnDESCRIPTION && (Referential.dataRow[Referential.IndexColSQL_DESCRIPTION].ToString().Length > 0))
                subTitleData += $" - {Referential.dataRow[Referential.IndexColSQL_DESCRIPTION]}";

            // FI 20220503 [XXXXX] Nouveauté => s'il existe des accolades Sphere® les remplace par les valeurs des colonnes spécifiées
            // Exemple DerivativeContractUpdateMaturityRule.aspx?IDDC={IDDC}&IDMATURITYRULE={IDMATURITYRULE} devient DerivativeContractUpdateMaturityRule.aspx?IDDC=2452&IDMATURITYRULE=20
            if (StrFunc.IsFilled(menu.Url) && menu.Url.Contains(@"={"))
            {
                using (IDataReader dr = (Referential.dataSet.Tables[0] as DataTable).CreateDataReader())
                {
                    MapDataReaderRow row = DataReaderExtension.DataReaderMapToList(dr, DataReaderExtension.DataReaderMapColumnType(dr)).FirstOrDefault();
                    if (null != row)
                    {
                        foreach (MapDataReaderColumn item in row.Column.Where(x => menu.Url.Contains($"={{{x.Name}}}")))
                            menu.Url = menu.Url.Replace($"={{{item.Name}}}", $"={Convert.ToString(item.Value, System.Globalization.CultureInfo.InvariantCulture)}");
                    }
                }
            }

            string url = $"{menu.Url}&IDMenu={menu.IdMenu}&FK={valueDataKeyField}";
            url += $"&SubTitle={HttpUtility.UrlEncode($"{subTitle}: {subTitleData}", System.Text.Encoding.UTF8)}";
            url += "&";

            return url;
        }
    }
}

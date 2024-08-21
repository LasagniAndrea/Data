using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.Referential;
using EFS.Status;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Controls
{
    /// <summary>
    /// Description résumée de TemplateDataGrid.
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public partial class TemplateDataGrid : DataGrid, IPostBackEventHandler
    {
        #region enum
        /// <summary>
        /// Evènement qui se produit lorsqu'une erreur se produit durant le chargement du jeu de données
        /// </summary>
        public event LoadDataErrorEventHandler LoadDataError;

        /// <summary>
        /// 
        /// </summary>
        public enum RessourceTypeEnum
        {
            Empty,
            Denominator,
            Fill,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum PagingTypeEnum
        {
            /// <summary>
            /// Pagination SQL, seules les lignes de la page courante sont stockées dans la source de donnée [Dataset] 
            /// </summary>
            CustomPaging,
            /// <summary>
            /// Pagination native, toutes les lignes du jeu de résulat sont stockées dans la source de donnée [Dataset]
            /// </summary>
            NativePaging,
        }

        /// <summary>
        /// 
        /// </summary>
        // FI 20200602 [25370] Add 
        private enum ReferentialModeEnum
        {
            XML,
            SQL
        }


        #endregion

        


        #region Members

        #region alException
        /// <summary>
        /// Liste des erreurs rencontrées (lorsque Spheres® est en mode sans Echec [mode par défaut])
        /// </summary>
        private List<SpheresException2> _alException;
        /// <summary>
        /// Obtient la liste des erreurs rencontrées lorsque le datagrid est en mode sans Echec 
        /// </summary>
        public List<SpheresException2> AlException
        {
            get { return _alException; }
        }
        #endregion


        /// <summary>
        /// Obtient si publication pour export excel
        /// </summary>
        /// FI 20160308 [21782] Add 
        /// FI 20200602 [25370] Refactoring
        public Boolean IsModeExportExcel
        {
            /*
            get;
            set;
             */
            get
            {
                return (DataFrom__EVENTTARGET == "imgMSExcel");
            }
        }

        // FI 20200220 [XXXXX] Mis en commentaire
        //private NameValueCollection m_NVC_QueryString;
        private bool isInHeader_OnItemCreated;
        private bool isInBody_OnItemCreated;
        private bool isInFooter_OnItemCreated;

        #region precommand
        /// <summary>
        /// Stocke les dernier scripts exécutés au préalable avant le chargement du grid  
        /// </summary>
        private Pair<string, TimeSpan>[] _lastPreSelectCommand;

        /// <summary>
        /// Obtient le script exécuté au préalable avant le chargement du grid  
        /// </summary>
        public Pair<string, TimeSpan>[] LastPreSelectCommand
        {
            get { return _lastPreSelectCommand; }
        }
        #endregion

        #region lastQuery
        /// <summary>
        /// stocke la dernière query exécutée pour charger le datagrid
        /// </summary>
        private Pair<string, TimeSpan> _lastQuery;

        /// <summary>
        /// Obtient la dernière Query exécutée pour charger la source de donnée du datagrid
        /// </summary>
        public Pair<string, TimeSpan> LastQuery
        {
            get { return _lastQuery; }
        }

        /// <summary>
        /// Obtient la durée total des requêtes SQL (PreSelectCommand+ LastQuery)
        /// </summary>
        /// FI 20210112 [XXXXX] Add
        public Nullable<TimeSpan> LastQueryTotalTimeSpan
        {
            get
            {
                Nullable<TimeSpan> ret = null;
                if (null != LastQuery)
                {
                    ret = new TimeSpan();
                    if (ArrFunc.IsFilled(LastPreSelectCommand))
                    {
                        ret = new TimeSpan();
                        foreach (TimeSpan item in LastPreSelectCommand.Select(x => x.Second))
                            ret += item;
                    }
                    ret += LastQuery.Second;
                }
                return ret;
            }
        }


        #endregion

        #region lastErrorQueryCount
        /// <summary>
        /// stocke le message d'erreur rencontré lorsque La query count est en erreur
        /// <para>uniquement lorsque la pagination customisée est activée</para>
        /// </summary>
        private string _lastErrorQueryCount;
        /// <summary>
        /// Retourne le message d'ereur rencontré lors de la détermination du nombre de ligne
        /// <para>
        /// Alimenté uniquement lorsqe la pagination customisée est activée 
        /// </para>
        /// </summary>
        public string LastErrorQueryCount
        {
            get { return _lastErrorQueryCount; }
        }

        #endregion

        /// <summary>
        /// Obtient true si le mode sans Echec est activé
        /// </summary>
        /// FI 20200602 [25370] Refactoring
        public bool IsNoFailure
        {
            get { return TemplateDataGridPage.IsNoFailure; }
        }

        /// <summary>
        ///  Obtient une référence (de type ITemplateDataGridPage) à la page qui contient le grid 
        /// </summary>
        /// FI 20200602 [25370] add
        public ITemplateDataGridPage TemplateDataGridPage
        {
            get => Page as ITemplateDataGridPage;
        }


        /// <summary>
        /// Obtient ou définit un drapeau qui indique que le chargement du jeu de données est à effectuer (exécution Requête SQL)
        /// </summary>
        public bool IsLoadData
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique que le datagrid doit être lié avec la source de donnée
        /// </summary>
        public bool IsBindData
        {
            get;
            set;
        }



        /// <summary>
        /// Obtient ou définit le type de pagination du datagrid
        /// <para>Lorsque pagingType est null, le type de pagination est déterminée automatiquement</para>
        /// <seealso cref="LoadData"/>
        /// </summary>
        public Nullable<PagingTypeEnum> PagingType
        {
            get;
            set;
        }


        #region isCheckboxColumn
        private bool _isCheckboxColumn;
        /// <summary>
        /// Obtient ou définit la présence d'une colonne de type Checkbox 
        /// <para>Elle permet à l'utilisateur d'effectuer des selections multiples de lignes</para>
        /// </summary>
        public bool IsCheckboxColumn
        {
            get { return _isCheckboxColumn; }
            set { _isCheckboxColumn = value; }
        }
        #endregion

        
        /// <summary>
        /// Obtient ou définit la présence d'une colonne Loupe
        /// <para>Elle permet à l'utilisateur de consulter des documents (Confirmations par exemple)</para>
        /// </summary>
        private bool _isLoupeColumn;
        
        
        #region inputMode

        public Cst.DataGridMode InputMode
        {
            get;
            set;
        }
        #endregion inputMode
        // EG 20160211 New Pager
        private TextBox txtSetPageNumberHeader;
        private TextBox txtSetPageNumberFooter;
        private Label lblRowsCountHeader;
        private Label lblRowsCountFooter;

        private bool isLocalGridSelectMode;
        private int editItemIndex;
        private int deleteItemIndex;
        private int newItemIndex;
        private int attachDocItemIndex;

        private int nbColumnAction;

        private string _dgRessource;
        private Cst.ListType _listType;
        private string _foreignKeyField;
        // FI 20200220 [XXXXX] referential est désormais une property
        //public ReferentialsReferential referential;
        //
        private int indexKeyField;
        private int indexColSQL_KeyField;
        private string queryStringDA;
        
        
        private string openerKeyId;
        private string openerSpecifiedId;
        private string openerSpecifiedSQLField;
        private string valueForFilter;
        private string typeKeyField;
        private string condApp;
        //
        #region  param
        /// <summary>
        /// Liste des paramètre Px spécifié dans l'URL
        /// </summary>
        private string[] param;
        #endregion
        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property localLstConsult
        /// </summary>
        /// FI 20200225 [XXXXX] Add
        public string DataCacheKeyLstConsult
        {
            get { return ((PageBase)this.Page).BuildDataCacheKey("LstConsult"); }
        }

        /// <summary>
        /// Obtient ou définit la consultation courante
        /// </summary>
        /// FI 20200220 [XXXXX] refactoring (usage d'une variable session)
        public LstConsult LocalLstConsult
        {
            get
            {
                return DataCache.GetData<LstConsult>(DataCacheKeyLstConsult);
            }
            set
            {
                DataCache.SetData<LstConsult>(DataCacheKeyLstConsult, value);
            }
        }

        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property IdLstTemplate
        /// </summary>
        /// FI 20200225 [XXXXX] Add
        public string DataCacheKeyIdLstTemplate
        {
            get { return ((PageBase)this.Page).BuildDataCacheKey("IdLstTemplate"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200220 [XXXXX] refactoring (usage d'une variable session)
        public string IdLstTemplate
        {
            get
            {
                return DataCache.GetData<string>(DataCacheKeyIdLstTemplate);
            }
            set
            {
                DataCache.SetData<string>(DataCacheKeyIdLstTemplate, value);
            }
        }
        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property IdA
        /// </summary>
        /// FI 20200225 [XXXXX] Add
        public string DataCacheKeyIdA
        {
            get { return ((PageBase)this.Page).BuildDataCacheKey("IdA"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200220 [XXXXX] refactoring (usage d'une variable session)
        public int IdA
        {
            get
            {
                return DataCache.GetData<int>(DataCacheKeyIdA);
            }
            set
            {
                DataCache.SetData<int>(DataCacheKeyIdA, value);
            }
        }
        //
        public string sessionName_LstColumn;
        //
        private readonly ArrayList alColumnName;
        private readonly ArrayList alColumnNameType;
        private readonly ArrayList alColumnIdForReferencial;
        private readonly ArrayList alColumnIsResource;
        private readonly ArrayList alColumnIsTRIM;
        private readonly ArrayList alColumnIsRTF;
        private readonly ArrayList alColumnRowStyle;
        private readonly ArrayList alColumnAggregate;
        private readonly ArrayList alColumnCellStyle;
        private readonly ArrayList alColumnIsMandatory;
        private readonly ArrayList alColumnRessourceType;//PL 20110913
        /// <summary>
        /// Liste contenant le type de chaque colonne (les valeurs possibles sont issues de l'enumeration TypeDataEnum) 
        /// </summary>
        private readonly ArrayList alColumnDataType;
        /// <summary>
        /// Liste contenant le "type étendu" de chaque colonne (le "type étendude" est de type ReferentialsReferentialColumnDataType) 
        /// </summary>
        // FI 20171025 [23533] Add
        private readonly ArrayList alColumnDataTypeLongForm;
        private readonly ArrayList alColumnDataScale;
        private readonly ArrayList alColumnLengthInDataGrid;
        // RD 20100309 / Automatic Compute identifier
        private readonly ArrayList alColumnDefaultValue;
        // EG 20110104 / Editable column control
        private readonly ArrayList alColumnIsEditable;

        /// <summary>
        /// 
        /// </summary>
        private bool isExistColumnDataTypeBoolean;
        /// <summary>
        /// Indicateur de l'existence colonnes (du referential) avec hyperlink 
        /// </summary>
        private bool isExistHyperLinkColumn;
        /// <summary>
        /// 
        /// </summary>
        private bool isExistEditableColumn;
        /// <summary>
        /// 
        /// </summary>
        private Hashtable _currencyInfos;
        //
        private const string RELATION = "relation";
        private const string COLUMN = "column";
        private const string HEADERWITHMULTICOLUMN = "?hideAndcolspan=1&value=";
        private readonly string resAUTOMATIC_COMPUTE;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la collection des variables de requête HTTP
        /// </summary>
        private NameValueCollection NVC_QueryString
        {
            get
            {
                /* FI 20200220 [XXXX] Lecture directe de Page.Request.QueryString
                if (m_NVC_QueryString == null)
                    m_NVC_QueryString = Page.Request.QueryString;
                return m_NVC_QueryString;
                */
                return Page.Request.QueryString;
            }
        }

        /// <summary>
        /// Obtient true s'il existe une varailble HTTP nommée Consuultation
        /// <para>Le grid est alors chargé depuis les tables LST</para>
        /// </summary>
        public bool IsConsultation
        {
            get
            {
                return StrFunc.IsFilled(NVC_QueryString[Cst.ListType.Consultation.ToString()]);
            }
        }

        private ReferentialModeEnum ReferentialMode
        {
            get
            {
                return IsConsultation ? ReferentialModeEnum.SQL : ReferentialModeEnum.XML;
            }
        }
        /// <summary>
        /// Obtient true si la source de donnée DsData du datagrid est renseignée
        /// <para>Le dataset existe et il contient 1 table</para>
        /// </summary>
        /// <returns></returns>
        public bool IsDataAvailable
        {
            get
            {
                DataTable dt = null;
                if ((DsData != null) && ArrFunc.Count(DsData.Tables) > 0)
                    dt = DsData.Tables[0];
                //
                return (null != dt);
            }
        }

        /// <summary>
        /// Obtient true si la source de donnée DsData du datagrid est renseignée
        /// <para>Le dataset existe et il contient 1 table avec des colonnes</para>
        /// </summary>
        /// <returns></returns>
        public bool IsDataFilled
        {
            get
            {
                bool ret = false;
                if (IsDataAvailable)
                    ret = ArrFunc.IsFilled(DsData.Tables[0].Columns);
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si la source de donnée du datagrid est not null
        /// </summary>
        /// <returns></returns>
        public bool IsDataSourceAvailable
        {
            get
            {
                return (null != DataSource);
            }
        }

        /// <summary>
        /// Obtient true si le dataGrid doit charger les données à la 1er ouverture de la page
        /// </summary>
        public bool IsLoadDataOnStart
        {
            get;
            private set;
        }

        /// <summary>
        /// <para>Obtient true si Spheres® répond à la demande de sélection/désélection de toutes les lignes de toutes les pages</para>
        /// </summary>
        public bool IsCheckUncheckAllRows
        {
            get { return DataFrom__EVENTTARGET.EndsWith("cbSelectAllOnAllPages"); }
        }

        /// <summary>
        /// <para>Obtient true si postback avec demande de chargement des données (LoadData)</para>
        /// </summary>
        /// FI 20140519 [19923] => public property
        /// FI 20160712 [XXXXX] Modify
        public bool IsSelfReload
        {
            get
            {
                Boolean ret = false;
                if (StrFunc.IsFilled(DataFrom__EVENTARGUMENT))
                {
                    if (DataFrom__EVENTARGUMENT.ToUpper() == "SELFRELOAD_")
                    {
                        ret = true;
                    }
                    else if (DataFrom__EVENTARGUMENT.ToUpper() == "SEARCHDATA")
                    {
                        //FI 20160712 [XXXXX] add else if
                        //Lorsque l'utilisateur appuie sur {enter} ou quitte la zone filtre Libre, Spheres® charge la dataset s'il n'a jamais été chargé
                        //Cas d'un grid qui dont le chargement n'est pas effectué par défaut (isLoadDataOnStart = false). 
                        //L'utilisateur rentre des données libres dans le filtre libre, puis il appuie sur {enter} ou quitte la zone 
                        //=> Dans ce dernier cas Spheres charge le dataset avec l'ensemble des données puis applique le filtre (Comportement identique à une demande de rafaîchissement du grid) 
                        ret = (false == IsDataFilled);
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// <para>Obtient true si postback avec demande de purge des données</para>
        /// </summary>
        /// FI 20140519 [19923] => public property
        public bool IsSelfClear
        {
            get
            {
                return StrFunc.IsFilled(DataFrom__EVENTARGUMENT) && (DataFrom__EVENTARGUMENT.ToUpper() == "SELFCLEAR_");
            }
        }

        /// <summary>
        /// <para>Obtient true si postback avec demande de non chargement des données</para>
        /// </summary>
        // FI 20200602 [25370] Add
        public bool IsSelfNoLoad
        {
            get
            {
                Boolean ret = false;
                if (StrFunc.IsFilled(DataFrom__EVENTARGUMENT))
                {
                    if (DataFrom__EVENTARGUMENT.ToUpper() == "SELFNOLOAD_")
                    {
                        ret = true;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si le DblClick est autorisé sur chaque ligne du datagrid
        /// </summary>
        /// FI 20160308 [21782] Modify 
        private bool IsRowDblClickAvailable
        {
            get
            {
                bool ret = (false == IsModeExportExcel); // FI 20160308 [21782]  
                if (ret)
                {
                    if (IsConsultation)
                        ret = (0 <= Referential.IndexColSQL_DataKeyField);
                    else if (Referential.isDblClickSpecified)
                        ret = Referential.isDblClick;
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient true lorsque le grid génère des links vers les référentiels (Links vers Acteur, Marché, etc...)
        /// </summary>
        /// FI 20160308 [21782] Add 
        private Boolean IsHyperLinkAvailable
        {
            get
            {
                Boolean ret = false;
                if (false == IsModeExportExcel) //pas de link avec Excel
                    ret = isExistHyperLinkColumn;
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si les colonnes sont de type TemplateColumn (cad customizables)
        /// </summary>
        private bool IsTemplateColumn
        {
            get
            {
                // FI 20190318 [24568][24588] isTemplateColumn si isModeExportExcel 
                // L'utilisation de TemplateColumn permet d'obtenir le ReferentialsReferentialColumn associé Control (via son ID)
                // voir la méthode TemplateDataGrid.FormatTextExcel
                return (IsGridInputMode || IsModeExportExcel || Referential.IsDataGridWithTemplateColumn);
            }
        }

        #region inputMode
        /// <summary>
        /// Obtient true si le datagrid permet uniquement de consulter le jeu de donnée 
        /// <para>la selection d'une ligne s'effectue depuis le formulaire</para>
        /// </summary>
        public bool IsFormSelectMode
        {
            get { return (InputMode == Cst.DataGridMode.FormSelect); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet uniquement de consulter le jeu de donnée 
        /// <para>la selection d'une ligne s'effectue depuis le datagrid (ie  aide en ligne de la saisie des trades)</para>
        /// </summary>
        public bool IsGridSelectMode
        {
            get { return (InputMode == Cst.DataGridMode.GridSelect); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet de modifier le jeu de donnée via le formulaire de saisie (ie les référentiels)
        /// </summary>
        public bool IsFormInputMode
        {
            get { return (InputMode == Cst.DataGridMode.FormInput); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet de modifier le jeu de donnée
        /// </summary>
        public bool IsGridInputMode
        {
            get { return (InputMode == Cst.DataGridMode.GridInput); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet uniquement de consulter le jeu de donnée via un formulaire
        /// </summary>
        public bool IsFormViewerMode
        {
            get { return (InputMode == Cst.DataGridMode.FormViewer); }
        }

        /// <summary>
        /// Obtient true si le jeu de données est non modifiable (Mode selection) 
        /// </summary>
        public bool IsSelectMode
        {
            get { return IsGridSelectMode || IsFormSelectMode || IsFormViewerMode; }
        }
        /// <summary>
        /// Obtient true si le jeu de données est modifiable (soit via formulaire, soit directement via le datagrid)
        /// </summary>
        public bool IsInputMode
        {
            get { return IsGridInputMode || IsFormInputMode; }
        }
        #endregion


        /// <summary>
        /// Retourne true lorsque le datagrid est en mode pagination customisée et que la détermination du nombre de ligne a généré une erreur
        /// <para>
        /// Dans ce contexte le datagrid n'affiche virtuellement 10 pages
        /// </para>
        /// </summary>
        public bool IsVirtualItemCountError
        {
            get
            {
                return StrFunc.IsFilled(_lastErrorQueryCount);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private string ForeignKeyField
        {
            set { _foreignKeyField = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        [Bindable(true), Category("Localization"), DefaultValue("")]
        public string DGRessource
        {
            get { return _dgRessource; }
            set { _dgRessource = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Cst.ListType ListType
        {
            get { return _listType; }
            set { _listType = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        [Bindable(true), Category("Data"), Description("IDMenu")]
        public string IDMenu
        {
            get { return Convert.ToString(ViewState[this.ClientID + "IDMenu"]); }
            set { ViewState[this.ClientID + "IDMenu"] = value; }
        }
        // EG 20151019 [21465] New
        [Bindable(true), Category("Data"), Description("IDMenuSys")]
        public string IDMenuSys
        {
            get { return Convert.ToString(ViewState[this.ClientID + "IDMenuSys"]); }
            set { ViewState[this.ClientID + "IDMenuSys"] = value; }
        }
        // EG 20151019 [21465] New (utiliser pour compléter le titre d'un menu lorsque celui-ci à plusieurs parent (Empty si un seul parent)
        [Bindable(true), Category("Data"), Description("IDMenuParent")]
        public string IDMenuParent
        {
            get { return Convert.ToString(ViewState[this.ClientID + "IDMenuParent"]); }
            set { ViewState[this.ClientID + "IDMenuParent"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Bindable(true), Category("Data"), Description("TitleMenu")]
        public string TitleMenu
        {
            get { return (string)ViewState[this.ClientID + "TitleMenu"]; }
            set { ViewState[this.ClientID + "TitleMenu"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Bindable(true), Category("Data"), Description("TitleRes")]
        public string TitleRes
        {
            get { return (string)ViewState[this.ClientID + "TitleRes"]; }
            set { ViewState[this.ClientID + "TitleRes"] = value; }
        }

        /// <summary>
        /// Obtient ou définit le nom de la consultation
        /// </summary>
        [Bindable(true), Category("Data"), Description("ObjectName to edit.")]
        public string ObjectName
        {
            get
            {
                string s = (string)ViewState[this.ClientID + "ObjectName"];
                if (s == null)
                    return String.Empty;
                return s;
            }
            set { ViewState[this.ClientID + "ObjectName"] = value; }
        }

        /// <summary>
        /// Obtient ou définit le nom du GUI associé à l'XML (cas d'un GUI partagé)
        /// </summary>
        /// EG 20151019 [21465] New
        [Bindable(true), Category("Data"), Description("GUIName")]
        public string GUIName
        {
            get { return Convert.ToString(ViewState[this.ClientID + "GUIName"]); }
            set { ViewState[this.ClientID + "GUIName"] = value; }
        }


        /// <summary>
        /// Obtient ou définit le type de consultation
        /// <para>Log, EAR, Invoicing, Event, ect...</para>
        /// </summary>
        [Bindable(true), Category("Data"), Description("Title")]
        public string Title
        {
            get
            {
                string s = (string)ViewState[this.ClientID + "Title"];
                if (s == null)
                    return String.Empty;
                return s;
            }
            set
            {
                ViewState[this.ClientID + "Title"] = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le DataSource
        /// <para>Hide some properties on the base DataGrid</para>
        /// </summary>
        [Bindable(false), Browsable(false)]
        public override object DataSource
        {
            get { return base.DataSource; }
            set { base.DataSource = value; }
        }

        /// <summary>
        /// Obtient ou définie le nom d'identification du jeu de données dans le cache du serveur web
        /// </summary>
        /// FI 20200518 [XXXXX] Rename en DataCacheKeyDsData
        public string DataCacheKeyDsData
        {
            get
            {
                // FI 20200518 [XXXXX] use BuildDataCacheKey 
                return ((PageBase)this.Page).BuildDataCacheKey("Data"); 
            }
        }

        /// <summary>
        ///  Obtient ou définit le jeu de données
        ///  <remarks>Le jeu de résultat est stocké en variable Session, Obtient null si le session n'est pas morte</remarks>
        /// </summary>
        /// FI 20200518 [XXXXX] Use DataCache
        public DataSet DsData
        {
            get
            {
                return DataCache.GetData<DataSet>(DataCacheKeyDsData);
            }
            set
            {
                DataCache.SetData<DataSet>(DataCacheKeyDsData, value);
            }
        }


        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property DataSav
        /// </summary>
        /// FI 20200225 [XXXXX] Add
        private string DataCacheKeyDataSav
        {
            get { return ((PageBase)this.Page).BuildDataCacheKey("DataSav"); }
        }

        /// <summary>
        /// Variable session destinée à sauvegarder le jeu de donnée lorsqu'une recherche est effectuée (txtSearch) pour restituér les données lorsque txtSearch est vidé
        /// </summary>
        /// FI 20140926 [XXXXX] Add 
        private DataTable DataSav
        {
            get
            {
                return DataCache.GetData<DataTable>(DataCacheKeyDataSav);
            }
            set
            {
                DataCache.SetData<DataTable>(DataCacheKeyDataSav, value);
            }
        }

        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property referential
        /// </summary>
        /// FI 20200225 [XXXXX] Add
        public string DataCacheKeyReferential
        {
            get { return ((PageBase)this.Page).BuildDataCacheKey("Referential"); }
        }

        /// <summary>
        /// Obtient referential depuis le cache 
        /// </summary>
        /// FI 20200220 [XXXXX] refactoring (usage d'une variable session)
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


        /// <summary>
        /// 
        /// </summary>
        protected string SortExpression
        {
            get { return (string)ViewState[ClientID + "CurrentSortExpression"]; }
            set { ViewState[this.ClientID + "CurrentSortExpression"] = value; }
        }

        /// <summary>
        /// retourne true si le tri est définit par l'utilisateur
        /// <para>un tri utilisateur est activé par un click sur la ligne d'entête</para>
        /// </summary>
        protected bool IsSortedByUser
        {
            get
            {
                return StrFunc.IsFilled(SortExpression);
            }
        }

        /// <summary>
        /// Obtient true, s'il existe ligne du datagrid à modifier
        /// </summary>
        public bool IsLocked
        {
            get { return (this.EditItemIndex >= 0); }
        }

        /// <summary>
        /// Obteint ou définit un indicateur dès lors que le jeu de données a été modifié
        /// </summary>
        /// FI 20200518 [XXXXX] Use DataCache
        public bool IsDataModified
        {
            //20061026 RD Referential Refactor			
            get
            {
                return BoolFunc.IsTrue(DataCache.GetData<Boolean>(DataCacheKeyDsData + "_statusModified"));
            }
            set
            {
                DataCache.SetData<Boolean>(DataCacheKeyDsData + "_statusModified", value);
            }
        }

        /// <summary>
        /// Rtourne le nbr de ligne du jeux de résultat
        /// </summary>
        public int TotalRowsData
        {
            get
            {
                int ret = 0;
                if (IsDataAvailable)
                {
                    if (null != DataSav)
                        ret = DataSav.Rows.Count;
                    else
                        ret = DsData.Tables[0].Rows.Count;
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient le nombre de lignes du datagrid
        /// </summary>
        public int TotalRowscount
        {
            get
            {
                int ret = 0;
                //
                if (AllowCustomPaging)
                {
                    ret = VirtualItemCount;
                }
                else
                {
                    if (IsDataAvailable)
                        ret = DsData.Tables[0].Rows.Count;
                }
                //
                return ret;
            }
        }

        /// <summary>
        /// Obtient si la page courante est la 1er page
        /// </summary>
        protected bool IsFirstPage
        {
            get
            {
                return CurrentPageIndex == 0;
            }
        }

        /// <summary>
        /// Obtient si la page courante est la dernière page
        /// </summary>
        protected bool IsLastPage
        {
            get
            {
                return (CurrentPageIndex + 1 == PageCount);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string DataFrom__EVENTTARGET
        {
            get { return ((null != Page) ? Page.Request.Params["__EVENTTARGET"] : string.Empty); }
        }

        /// <summary>
        /// 
        /// </summary>
        private string DataFrom__EVENTARGUMENT
        {
            get { return ((null != Page) ? Page.Request.Params["__EVENTARGUMENT"] : string.Empty); }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Use DataCache
        private int PageIndexBeforeInsert
        {
            get
            {
                int retValue;
                try
                {
                    retValue = DataCache.GetData<int>(((PageBase)Page).BuildDataCacheKey("PageIndexBeforeInsert"));
                }
                catch
                {
                    retValue = 1;
                }
                return retValue;
            }
            set
            {
                DataCache.SetData<int>(((PageBase)Page).BuildDataCacheKey("PageIndexBeforeInsert"), value);
            }
        }

        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public TemplateDataGrid()
        {
            resAUTOMATIC_COMPUTE = Ressource.GetString(Cst.AUTOMATIC_COMPUTE.ToString());
            //
            indexKeyField = -1;
            indexColSQL_KeyField = -1;
            //
            
            
            openerKeyId = string.Empty;
            openerSpecifiedId = string.Empty;
            openerSpecifiedSQLField = string.Empty;
            valueForFilter = string.Empty;
            typeKeyField = string.Empty;
            condApp = string.Empty;
            //
            alColumnName = new ArrayList();
            alColumnNameType = new ArrayList();
            alColumnIdForReferencial = new ArrayList();
            alColumnIsResource = new ArrayList();
            alColumnIsTRIM = new ArrayList();
            alColumnIsRTF = new ArrayList();
            alColumnRowStyle = new ArrayList();
            alColumnAggregate = new ArrayList();
            alColumnCellStyle = new ArrayList();
            alColumnIsMandatory = new ArrayList();
            alColumnRessourceType = new ArrayList();
            alColumnDataType = new ArrayList();
            alColumnDataTypeLongForm = new ArrayList();
            alColumnDataScale = new ArrayList();
            alColumnLengthInDataGrid = new ArrayList();
            // RD 20100309 / Automatic Compute identifier
            alColumnDefaultValue = new ArrayList();
            alColumnIsEditable = new ArrayList();
            //
            sessionName_LstColumn = string.Empty;
            //
            InitTemplateDataGrid();
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strNewSortExpr"></param>
        private void ProcessSortExpression(String strNewSortExpr)
        {

            // Internal-use constants
            const string ORDER_DESC = " desc";
            const string ORDER_ASC = " asc";
            const string CONCAT_ORDER_DESC = "desc,";
            const string CONCAT_ORDER_ASC = "asc,";

            // Read current settings for fields and orders (DESC/ASC)
            string strSortExpressions = (string)ViewState[this.ClientID + "SortingFields"];
            string strSortOrders = (string)ViewState[this.ClientID + "SortingOrders"];

            // Parse the new sorting string. This string can only be one of 
            // the strings read from the SortExpression attribute for datagrid's
            // columns. It has any of the forms: 
            // "FieldName", "FieldName DESC", "FieldName ASC",
            // "Field1,Field2 DESC", "Field1 ASC,Field2 DESC", "Field1,Field2".
            // The function splits on the comma, trims all strings and 
            // builds up the two string to store in the Attributes repository.

            // Separate field names from the information regarding the order.
            // Use strNewSortExpr to build the order-less string of fields 
            char[] aSplitterChar = new char[] { ',' };
            string[] aSortExpressions = strNewSortExpr.Split(aSplitterChar);

            // Needed in case you want to show a glyph on the caption. In this 
            // case you need to know about the original expression 
            ViewState[this.ClientID + "ColumnSortExpression"] = strNewSortExpr;

            // Separates fields from orders and builds up two distinct, comma-sep strings
            // i.e. INPUT  --> lastname,firstname DESC
            // i.e. OUTPUT --> lastname,firstname AND asc,desc

            string strNewSortExpressions = "";
            string strNewSortOrders = "";
            for (int i = 0; i < aSortExpressions.Length; i++)
            {
                string tmp1 = aSortExpressions[i].ToLower().Trim();

                int nPos;
                if (tmp1.EndsWith(ORDER_DESC))
                {
                    strNewSortOrders += CONCAT_ORDER_DESC;
                    nPos = tmp1.LastIndexOf(ORDER_DESC);
                    strNewSortExpressions += tmp1.Substring(0, nPos).Trim() + ",";
                }
                else		// ASC or nothing specified, as ASC is the default
                {
                    strNewSortOrders += CONCAT_ORDER_ASC;
                    try
                    {
                        nPos = tmp1.LastIndexOf(ORDER_ASC);
                        strNewSortExpressions += tmp1.Substring(0, nPos).Trim() + ",";
                    }
                    catch
                    {
                        strNewSortExpressions += tmp1 + ",";
                    }
                }
            }

            // Cuts the final "," down
            strNewSortExpressions = strNewSortExpressions.Trim(aSplitterChar);
            strNewSortOrders = strNewSortOrders.Trim(aSplitterChar);

            // Compare current sorting fields with the new one. If it is 
            // the same, and we have sorted already on this fields, 
            // then ASC and DESC are inverted in strNewSortOrders.
            if ((strSortExpressions == strNewSortExpressions) && StrFunc.IsFilled(strSortOrders))
            {
                String[] aSortOrders;
                aSortOrders = strSortOrders.Split(aSplitterChar);

                strNewSortOrders = "";
                for (int i = 0; i < aSortOrders.Length; i++)
                {
                    if (aSortOrders[i] == "desc")
                        strNewSortOrders += CONCAT_ORDER_ASC;
                    else
                        strNewSortOrders += CONCAT_ORDER_DESC;
                }

                strNewSortOrders = strNewSortOrders.Trim(aSplitterChar);
            }

            // Stores the sorting settings to the Attributes repository.
            // This information will be retrieved later when the datagrid rebinds
            ViewState[this.ClientID + "SortingFields"] = strNewSortExpressions;
            ViewState[this.ClientID + "SortingOrders"] = strNewSortOrders;
            //
        }

        /// <summary>
        /// 
        /// </summary>
        private void PrepareSortExpression()
        {

            // i.e. INPUT from Attributes  --> lastname,firstname AND asc,desc
            // i.e. OUTPUT to the DataView --> lastname asc,firstname desc
            string strSortInfo = "";

            // Read current settings for fields and orders (DESC/ASC)
            string strSortExpressions = (string)ViewState[this.ClientID + "SortingFields"];
            string strSortOrders = (string)ViewState[this.ClientID + "SortingOrders"];

            // Build the string to sort the datagrid by
            try
            {
                char[] aSplitterChar = new char[] { ',' };
                string[] aSortExpressions = strSortExpressions.Split(aSplitterChar);
                string[] aSortOrders = strSortOrders.Split(aSplitterChar);

                for (int i = 0; i < aSortExpressions.Length; i++)
                    strSortInfo += aSortExpressions[i] + " " + aSortOrders[i] + ",";
                strSortInfo = strSortInfo.Trim(aSplitterChar);
            }
            catch { }
            //
            // Persists the CURRENT sort expression in Attributes
            SortExpression = strSortInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSortExpression"></param>
        /// <returns></returns>
        private bool IsSortedByThisField(string strSortExpression)
        {
            // Retrieves the sort expression associated with the clicked column
            String strRequestedSortExpression = (String)ViewState[this.ClientID + "ColumnSortExpression"];

            // Compares it to the sort expression of the column whose header is being processed
            return (strSortExpression == strRequestedSortExpression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        private string GetOrderSymbolImage()
        {
            // Retrieves the string with order information
            String strSortOrders = (String)ViewState[this.ClientID + "SortingOrders"];

            // Gets the webding corresponding to ASC/DESC for the first (primary) column
            // in the sort expression. The glyph is decided based on the order of the 
            // primary column in the sort expression.
            bool isDescending = strSortOrders.StartsWith("desc");
            return String.Format("fas fa-sort-{0}", isDescending ? "down" : "up");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDGCE"></param>
        private void UpdateDataFromControl(DataGridCommandEventArgs pDGCE)
        {

            int index = pDGCE.Item.DataSetIndex;
            DataRow dr = DsData.Tables[0].Rows[index];
            //
            bool isNewRecord = Referential.ExistsColumnsINS && StrFunc.IsEmpty(dr["IDAINS"].ToString());
            dr.BeginEdit();

            //
            for (int i = 0; i < alColumnName.Count; i++)
            {
                try
                {
                    //string s = alColumnName[i].ToString();
                    string data = string.Empty;
                    bool isRelation = (alColumnNameType[i].ToString() == RELATION);
                    Control ctrlFound;
                    string ctrlID;
                    if (i + 1 < alColumnNameType.Count && alColumnNameType[i + 1].ToString() == RELATION)
                        ctrlID = alColumnName[i + 1].ToString() + ReferentialTools.SuffixEdit;
                    else
                        ctrlID = alColumnName[i].ToString() + ReferentialTools.SuffixEdit;

                    ctrlFound = pDGCE.Item.FindControl(ctrlID);
                    if (ctrlFound == null)
                        throw (new Exception());

                    if (ctrlFound.GetType().Equals(typeof(Label)))
                        data = ((Label)ctrlFound).Text;
                    //WC Controls
                    else if (ctrlFound.GetType().Equals(typeof(WCCheckBox2)))
                    {
                        data = (((WCCheckBox2)ctrlFound).Checked).ToString();
                    }
                    else if (ctrlFound.GetType().Equals(typeof(WCDropDownList2)))
                    {
                        WCDropDownList2 ddl = (WCDropDownList2)ctrlFound;
                        data = ddl.SelectedItem.Value;
                        if (isRelation)
                            data = ddl.SelectedItem.Text;
                    }
                    else if (ctrlFound.GetType().Equals(typeof(WCTextBox)))
                        data = ((WCTextBox)ctrlFound).Text;
                    //WebControls
                    else if (ctrlFound.GetType().Equals(typeof(TextBox)))
                        data = ((TextBox)ctrlFound).Text;
                    else if (ctrlFound.GetType().Equals(typeof(CheckBox)))
                    {
                        data = ((CheckBox)ctrlFound).Checked.ToString();
                    }
                    else if (ctrlFound.GetType().Equals(typeof(DropDownList)))
                    {
                        DropDownList ddl = (DropDownList)ctrlFound;
                        data = ddl.SelectedItem.Value;
                        if (isRelation)
                            data = ddl.SelectedItem.Text;
                    }
                    //HtmlControls
                    else if (ctrlFound.GetType().Equals(typeof(System.Web.UI.HtmlControls.HtmlSelect)))
                    {
                        System.Web.UI.HtmlControls.HtmlSelect ddl = (System.Web.UI.HtmlControls.HtmlSelect)ctrlFound;
                        data = ddl.Items[ddl.SelectedIndex].Value;
                        if (isRelation)
                            data = ddl.Items[ddl.SelectedIndex].Text;
                    }
                    if (false == Convert.ToBoolean((alColumnIsMandatory[i])))
                        if (true == StrFunc.IsEmpty(data.Trim()))
                            data = string.Empty;

                    if (TypeData.IsTypeDateOrDateTime(alColumnDataType[i].ToString()))
                    {
                        DateTime dtData;
                        dtData = Convert.ToDateTime(data);
                        dr[alColumnName[i].ToString()] = dtData;
                    }
                    else if (TypeData.IsTypeBool(alColumnDataType[i].ToString()))
                    {
                        bool boolData = false;
                        boolData = Convert.ToBoolean(data);
                        dr[alColumnName[i].ToString()] = (boolData ? 1 : 0);
                    }
                    else if (TypeData.IsTypeDec(alColumnDataType[i].ToString()))
                    {
                        decimal decData;
                        decData = Convert.ToDecimal(data);
                        dr[alColumnName[i].ToString()] = decData;
                    }
                    else if (TypeData.IsTypeImage(alColumnDataType[i].ToString()))
                    {
                        //Cas particulier: pas d'affectation de valeur
                    }
                    else
                    {
                        if (StrFunc.IsEmpty(data))
                            dr[alColumnName[i].ToString()] = Convert.DBNull;
                        else
                            dr[alColumnName[i].ToString()] = data;
                    }

                    //update pour EXTLID si necessaire                        
                    if (Referential.HasMultiTable && DsData.Tables.Count > 1)
                    {
                        // EG 20110503 Changement Recherche Column dans Referentiel en tenant compte de ColumnPositionInDataGrid
                        //ReferentialsReferentialColumn rrc = Referential.Column[Convert.ToInt32(alColumnIdForReferencial[i])];
                        ReferentialsReferentialColumn rrc = Referential[Convert.ToInt32(alColumnIdForReferencial[i])];
                        if (rrc.IsExternal)
                        {
                            DataTable currentDT = DsData.Tables[1 + rrc.ExternalFieldID];
                            DataRow drExtlId;
                            string filter = string.Empty;
                            filter += " TABLENAME='" + Referential.TableName + "'";
                            filter += " AND";
                            filter += " IDENTIFIER='" + rrc.ExternalIdentifier + "'";
                            filter += " AND";
                            filter += " ID=";
                            if (Referential.IsDataKeyField_String)
                                filter += DataHelper.SQLString(dr[Referential.IndexColSQL_DataKeyField].ToString());
                            else
                                filter += dr[Referential.IndexColSQL_DataKeyField].ToString();

                            //20060524 PL Add: CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
                            currentDT.CaseSensitive = true;
                            DataRow[] rows = currentDT.Select(filter);
                            if (rows != null && rows.Length > 0)
                            {
                                drExtlId = rows[0];
                                if (true == StrFunc.IsEmpty(data))
                                    drExtlId["VALUE"] = Convert.DBNull;
                                else
                                    drExtlId["VALUE"] = data;
                                //FI 20200820 [XXXXXX] Date systeme en UTC
                                drExtlId["DTUPD"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                                drExtlId["IDAUPD"] = SessionTools.Collaborator_IDA;
                            }
                            /* PAS DE CREATE AUTO POUR UNE NEW LINE CAR MANQUE VALEUR POUR FK
                                        else
                                        {
                                            drExtlId = currentDT.NewRow();
                                            drExtlId.BeginEdit();
                                            drExtlId["TABLENAME"]  = Referential.TableName;
                                            drExtlId["IDENTIFIER"] = rrc.ExternalIdentifier;
                                            drExtlId["ID"]         = Convert.ToInt32(dr[Referential.IndexColSQL_DataKeyField].ToString());
                                            drExtlId["VALUE"]      = data;           
                                            drExtlId["DTINS"]      = dtSys;
                                            drExtlId["IDAINS"]     = SessionTools.Collaborator_IDA;
                                            drExtlId.EndEdit();
                                            currentDT.Rows.Add(drExtlId);
                                        }
                                        */
                        }
                    }
                }
                catch
                {
                    dr[alColumnName[i].ToString()] = Convert.DBNull;
                }
            }
            //MAJ champs INS
            if (isNewRecord && Referential.ExistsColumnsINS)
            {
                //FI 20200820 [XXXXXX] Date systeme en UTC
                dr["DTINS"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                dr["IDAINS"] = SessionTools.Collaborator_IDA;
                dr["DTUPD"] = Convert.DBNull;
                dr["IDAUPD"] = Convert.DBNull;
            }
            //MAJ champs UPD
            else if (!isNewRecord && Referential.ExistsColumnsUPD)
            {
                //FI 20200820 [XXXXXX] Date systeme en UTC
                dr["DTUPD"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                dr["IDAUPD"] = SessionTools.Collaborator_IDA;
            }
            dr.EndEdit();
            //
            this.EditItemIndex = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetTableColumnMapping()
        {
            if (IsDataFilled)
            {
                DataTable dt = DsData.Tables[0];
                for (int i = 0; i < alColumnName.Count; i++)
                    dt.Columns[alColumnName[i].ToString()].ColumnMapping = MappingType.Element;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pValue"></param>
        /// <param name="pDefaultValue"></param>
        /// <param name="pIsEnabled"></param>
        private static void SetDataToControl(Control pControl, string pValue, string pDefaultValue, bool pIsEnabled)
        {
            SetDataToControl(pControl, pValue, pDefaultValue, null, pIsEnabled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pValue"></param>
        /// <param name="pDefaultValue"></param>
        /// <param name="pFullValue"></param>
        /// <param name="pIsEnabled"></param>
        /// RD 20100309 / Automatic Compute identifier
        private static void SetDataToControl(Control pControl, string pValue, string pDefaultValue, string pFullValue, bool pIsEnabled)
        {

            if (pControl.GetType().Equals(typeof(Label)))
            {
                // RD 20100309 / Automatic Compute identifier
                if (StrFunc.IsEmpty(pValue) && (StrFunc.IsFilled(pDefaultValue)) && (Cst.AUTOMATIC_COMPUTE.ToString() == pDefaultValue))
                    pValue = Ressource.GetString(pDefaultValue);
                else if (Cst.AUTOMATIC_COMPUTE.ToString() == pValue)
                    pValue = Ressource.GetString(pValue);
                //
                Label lbl = (Label)pControl;
                lbl.Text = pValue;
                lbl.Enabled = pIsEnabled;
                if (pFullValue != null)
                    lbl.ToolTip = pFullValue;
            }
            //WC Controls
            else if (pControl.GetType().Equals(typeof(WCCheckBox2)))
            {
                WCCheckBox2 chk = (WCCheckBox2)pControl;
                chk.Checked = BoolFunc.IsTrue(pValue);
                chk.Text = string.Empty;
                chk.Enabled = pIsEnabled;
            }
            else if (pControl.GetType().Equals(typeof(WCDropDownList2)))
            {
                WCDropDownList2 ddl = (WCDropDownList2)pControl;
                ControlsTools.DDLSelectByValue(ddl, pValue);
                ddl.Enabled = pIsEnabled;
            }
            else if (pControl.GetType().Equals(typeof(WCTextBox)))
            {
                // RD 20100309 / Automatic Compute identifier
                if (StrFunc.IsEmpty(pValue) && (StrFunc.IsFilled(pDefaultValue)) && (Cst.AUTOMATIC_COMPUTE.ToString() == pDefaultValue))
                    pValue = Ressource.GetString(pDefaultValue);
                else if (Cst.AUTOMATIC_COMPUTE.ToString() == pValue)
                    pValue = Ressource.GetString(pValue);
                //
                WCTextBox txt = (WCTextBox)pControl;
                txt.Text = pValue;
                txt.Enabled = pIsEnabled;
            }
            //WebControls
            else if (pControl.GetType().Equals(typeof(TextBox)))
            {
                // RD 20100309 / Automatic Compute identifier
                if (StrFunc.IsEmpty(pValue) && (Cst.AUTOMATIC_COMPUTE.ToString() == pDefaultValue))
                    pValue = Ressource.GetString(pDefaultValue);
                else if (Cst.AUTOMATIC_COMPUTE.ToString() == pValue)
                    pValue = Ressource.GetString(pValue);
                //
                TextBox txt = (TextBox)pControl;
                txt.Text = pValue;
                txt.Enabled = pIsEnabled;
            }
            else if (pControl.GetType().Equals(typeof(CheckBox)))
            {
                CheckBox chk = (CheckBox)pControl;
                chk.Checked = BoolFunc.IsTrue(pValue);
                chk.Text = string.Empty;
                chk.Enabled = pIsEnabled;
            }
            else if (pControl.GetType().Equals(typeof(DropDownList)))
            {
                DropDownList ddl = (DropDownList)pControl;
                ControlsTools.DDLSelectByValue(ddl, pValue);
                ddl.Enabled = pIsEnabled;

            }
            //HtmlControls
            else if (pControl.GetType().Equals(typeof(System.Web.UI.HtmlControls.HtmlSelect)))
            {
                int index = 0;
                System.Web.UI.HtmlControls.HtmlSelect ddl = (System.Web.UI.HtmlControls.HtmlSelect)pControl;
                foreach (ListItem item in ddl.Items)
                {
                    if (item.Value == pValue)
                    {
                        ddl.SelectedIndex = index;
                        break;
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Ajout des colonnes du grid
        /// </summary>
        private void AddColumns()
        {
            AddColumnAction();
            AddColumnData();
        }

        /// <summary>
        /// Ajoute les colonnes actions
        /// <para>1er  colonne => n° de ligne</para>
        /// <para>2ème colonne => check de selection (optionel)</para>
        /// <para>3ème colonne => loupe              (optionel, existe si isFormInputMode ou isFormSelectMode)</para>
        /// <para>                                   et Referential.isLoupe (si Referential.isLoupeSpecified)</para>        
        /// <para>4ème colonne => creation           (optionel,existe si isGridInputMode)</para>
        /// <para>5ème colonne => Modification       (optionel,existe si isGridInputMode)</para>
        /// <para>6ème colonne => Suppression        (optionel,existe si isInputMode)</para>
        /// <para>7ème colonne => ATTACHEDDOC        (optionel,existe si isInputMode et si referentiel is ATTACHEDDOC)</para>
        /// <para>8ème colonne => SQLRowStates       (optionel,existe si la colonne existe)</para>
        /// </summary>
        //PL 20180812 [24252]  
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddColumnAction()
        {
            int currentIndex = 0;

            // Colonne Numéro de ligne ( ne pas déplacer, car cette colonne est supposée la première, voir OnItemDataBound())            
            TemplateColumn tcRowNumber = new TemplateColumn();
            tcRowNumber.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            tcRowNumber.ItemStyle.VerticalAlign = VerticalAlign.Middle;
            tcRowNumber.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
            tcRowNumber.HeaderStyle.VerticalAlign = VerticalAlign.Middle;
            Columns.Add(tcRowNumber);
            nbColumnAction++;
            currentIndex++;

            //Colonne Check de selection
            if (IsCheckboxColumn)
            {
                // Colonne ISSELECTED (uniquement pour les écrans de type "Traitement", ie: avec un Bouton "Générer")
                // ATTENTION: ne pas déplacer, cette colonne est attendue en deuxième postion.   
                TemplateColumn tcIsToProcess = new TemplateColumn();
                tcIsToProcess.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                tcIsToProcess.HeaderStyle.VerticalAlign = VerticalAlign.Middle;
                tcIsToProcess.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                tcIsToProcess.ItemStyle.VerticalAlign = VerticalAlign.Middle;

                string cbSelectAllOnPage = this.ID + "_cbSelectAllOnCurrentPage";
                string cbSelectID = this.ID + "_cbSelect";

                CheckBoxItem_Referencial myHeaderTemplate = new CheckBoxItem_Referencial(cbSelectAllOnPage, true, Ressource.GetString("SelectAllOnCurrentPage"));
                myHeaderTemplate.AddAttribute("onclick", "DataGridSelectAll(this, '" + this.ID + "', '" + cbSelectID + "')");
                myHeaderTemplate.CheckedChanged = new EventHandler(this.OnCheckedChanged_Select); //PL 20180812 [24252] Add EventHandler
                tcIsToProcess.HeaderTemplate = myHeaderTemplate;

                CheckBoxItem_Referencial myItemTemplate = new CheckBoxItem_Referencial(cbSelectID, "ISSELECTED", true, Ressource.GetString("Select"));
                myItemTemplate.AddAttribute("onclick", "DataGridCheckedChanged('" + this.ID + "', '" + cbSelectAllOnPage + "', '" + cbSelectID + "')");
                myItemTemplate.CheckedChanged = new EventHandler(this.OnCheckedChanged_Select);
                tcIsToProcess.ItemTemplate = myItemTemplate;

                this.Columns.Add(tcIsToProcess);
                nbColumnAction++;
                currentIndex++;
            }
            //
            if (IsFormInputMode || IsFormSelectMode || IsFormViewerMode)
            {
                _isLoupeColumn = ((!IsConsultation) && (Cst.ListType.ProcessBase != ListType) && (Cst.ListType.Report != ListType) && (Cst.ListType.Trade != ListType))
                                ||
                                (
                                    Referential.ExistsColumnDataKeyField
                                    &&
                                    (
                                        Referential.Column[Referential.IndexDataKeyField].ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.TRADE.ToString())
                                        ||
                                        Referential.Column[Referential.IndexDataKeyField].ColumnName == "DATAKEYFIELD"
                                    )
                                );

                // EG 20111013
                if (_isLoupeColumn && Referential.isLoupeSpecified)
                    _isLoupeColumn &= Referential.isLoupe;

                if (_isLoupeColumn)
                {
                    ButtonColumn c1 = new ButtonColumn
                    {
                        CommandName = "edit"
                    };
                    this.Columns.Add(c1);
                    editItemIndex = currentIndex;
                    nbColumnAction++;
                    currentIndex++;
                }
            }
            //
            if (IsInputMode)
            {
                if (IsGridInputMode)
                {
                    //colonne Status de ligne (grid only)
                    // EG 20151215 Add Test Referential.consultationMode
                    if (Referential.Create && (Referential.consultationMode != Cst.ConsultationMode.ReadOnly))
                    {
                        TemplateColumn tc = new TemplateColumn
                        {
                            HeaderText = " "
                        };
                        ItemTemplate_Referencial myItemTemplate = new ItemTemplate_Referencial("statusNew");
                        tc.ItemTemplate = myItemTemplate;
                        this.Columns.Add(tc);
                        newItemIndex = currentIndex;
                        nbColumnAction++;
                        currentIndex++;
                    }

                    //colonne Edit -> Update/Cancel (grid only)
                    // EG 20151215 Add Test Referential.consultationMode
                    if ((Referential.Modify || Referential.Create) && (Referential.consultationMode != Cst.ConsultationMode.ReadOnly))
                    {
                        EditCommandColumn c0 = new EditCommandColumn
                        {
                            EditText = @"<div class=""fa-icon"" title=""" + Ressource.GetString(Referential.Modify ? "imgEdit" : "imgView") + @"""><i class=""fas fa-search""></i></div>",
                            UpdateText = @"<div class=""fa-icon"" title=""" + Ressource.GetString("btnValidate") + @"""><i class=""fas fa-check""></i></div>",
                            CancelText = @"<div class=""fa-icon"" title=""" + Ressource.GetString("btnCancel") + @"""><i class=""fas fa-times""></i></div>"
                        };

                        c0.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        c0.ItemStyle.VerticalAlign = VerticalAlign.Middle;
                        c0.HeaderStyle.Width = new Unit("25px");
                        this.Columns.Add(c0);
                        editItemIndex = currentIndex;
                        nbColumnAction++;
                        currentIndex++;
                    }
                }
                //
                //colonne delete (commune a grid et form)
                // EG 20151215 Add Test Referential.consultationMode
                if (Referential.Remove && (Referential.consultationMode != Cst.ConsultationMode.ReadOnly))
                {
                    ButtonColumn c1 = new ButtonColumn
                    {
                        CommandName = "delete",
                        Text = @"<i class=""fas fa-trash-alt"" title=""" + Ressource.GetString("imgRemove") + @"""></i>"
                    };
                    c1.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    c1.ItemStyle.VerticalAlign = VerticalAlign.Middle;
                    c1.HeaderStyle.Width = new Unit("16px");

                    this.Columns.Add(c1);
                    deleteItemIndex = currentIndex;
                    nbColumnAction++;
                    currentIndex++;
                }
                //
                //Colonne consult 
                // EG 20240207 [WI825] Logs: Harmonization data of consultation (VW_ATTACHEDDOC_TRACKER_L)
                if (this.Referential.IsTableAttachedDoc)
                {
                    ButtonColumn c00 = new ButtonColumn
                    {
                        CommandName = "ConsultDoc"
                    };
                    this.Columns.Add(c00);
                    attachDocItemIndex = currentIndex;
                    nbColumnAction++;
                    currentIndex++;
                }
            }

            // SQLRowStates ( ne pas déplacer, car cette colonne est supposée la dérnière, voir OnItemDataBound())
            if (Referential.SQLRowStateSpecified && StrFunc.IsFilled(Referential.SQLRowState.Value))
            {
                TemplateColumn tc = new TemplateColumn();
                if (StrFunc.IsFilled(Referential.SQLRowState.headerText))
                    tc.HeaderText = Ressource.GetString(Referential.SQLRowState.headerText);
                //PL 20110331 Mise en commentaire de la ligne suivante (HorizontalAlign.Center)
                //tc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                tc.ItemStyle.VerticalAlign = VerticalAlign.Middle;
                this.Columns.Add(tc);
                nbColumnAction++;
                currentIndex++;
            }


        }

        /// <summary>
        /// Retourne un hyperlink pou les paires de colonnes suivnates
        /// <para>IDINSTR/TYPEINSTR</para>
        /// <para>IDCONTRACT/TYPECONTRACT</para>
        /// <para>IDPARTY/TYPEPARTY, IDPARTYA/FEETYPEPARTYA, IDPARTYB/FEETYPEPARTYB, IDOTHERPARTY1/FEETYPEOTHERPARTY1, IDOTHERPARTY2/FEETYPEOTHERPARTY2</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pDataGridItem"></param>
        /// <returns></returns>
        /// FI 20180219 [XXXXX] Add Method
        private HyperLink GetHyperLinkByColumnForPair(string pKey, DataGridItem pDataGridItem)
        {
            string column = string.Empty;
            switch (pKey)
            {
                case @"IDCONTRACT/TYPECONTRACT":
                    column = "IDCONTRACT";
                    break;
                case @"IDCONTRACTEXCEPT/TYPECONTRACTEXCEPT":
                    column = "IDCONTRACTEXCEPT";
                    break;
                case @"IDINSTR/TYPEINSTR":
                    column = "IDINSTR";
                    break;
                case @"IDPARTY/TYPEPARTY":
                    column = "IDPARTY";
                    break;
                case @"IDPARTYA/TYPEPARTYA":
                    column = "IDPARTYA";
                    break;
                case @"IDPARTYB/TYPEPARTYB":
                    column = "IDPARTYB";
                    break;
                case "IDOTHERPARTY1/TYPEOTHERPARTY1":
                    column = "IDOTHERPARTY1";
                    break;
                case "IDOTHERPARTY2/TYPEOTHERPARTY2":
                    column = "IDOTHERPARTY2";
                    break;
            }

            DataRow row = null;
            if (null != pDataGridItem)
                row = ((DataRowView)(pDataGridItem.DataItem)).Row;

            ReferentialsReferentialColumn rr = Referential[column];
            int indexCol = Referential.GetIndexColSQL(column);

            string colName = ReferentialTools.GetHyperLinkColumnNameFromRelation(Referential, rr, row);
            string id = FormatToInvariant(row[indexCol].ToString(), rr.DataType.value);
            string identifier = row[indexCol + 1].ToString();

            HyperLink hlkColumn = GetHyperLinkByColumn(pDataGridItem, colName, id, identifier);
            return hlkColumn;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataGridItem"></param>
        /// <param name="pColumnName">Représente le type de colonne gérée (ie IDA, IDDC)</param>
        /// <param name="pId">Représente l'id (id non significatif que doit ouvrir le lien)</param>
        /// <param name="pIdentifier">Permet d'alimenter le .text du link</param>
        /// <returns></returns>
        /// //FI 20120704 [17987] Refactoring, les id ne sont pas nécessairement numériques
        // EG 20170125 [Refactoring URL] Upd
        // EG 20210226 [XXXXX] Réécriture du lien sur Hyperlink (Pb self.Close) Function Javascript HL(this,id)
        private HyperLink GetHyperLinkByColumn(DataGridItem pDataGridItem, string pColumnName, string pId, string pIdentifier)
        {
            HyperLink hl = null;

            if (ReferentialTools.IsHyperLinkColumn(pColumnName))
            {
                if (DecFunc.IsDecimal(pId))
                    pId = IntFunc.IntValue2(pId).ToString();
                // EG 20170125 [Refactoring URL] Upd
                //string url = GetURLForHyperLink(pDataGridItem, pColumnName, pId);
                string url = GetURL("HYPERLINK", pColumnName, pId, pDataGridItem);

                if (StrFunc.IsFilled(url))
                {
                    hl = new HyperLink
                    {
                        Text = pIdentifier,
                        CssClass = "linkDatagrid",
                        Target = Cst.HyperLinkTargetEnum._blank.ToString(),
                    };
                    hl.Attributes.Add("args", url.Replace("hyperlink.aspx?args=", string.Empty));
                    hl.Attributes.Add("onclick", "HL(this)");
                    hl.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                }
            }
            return hl;
        }

        /// <summary>
        /// Génère un link qui ouvre la page ViewDoc.aspx
        /// </summary>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName_Data"></param>
        /// <param name="pColumnName_Type"></param>
        /// <param name="pColumnName_FileName"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDataTypes"></param>
        /// <returns></returns>
        /// 20081020 RD - Ticket 16331
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        private static WCToolTipHyperlink GetHyperLinkForDocument(string pTableName, string pColumnName_Data,
            string pColumnName_Type, string pColumnName_FileName,
            string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes)
        {

            string page = "ViewDoc.aspx?";
            string urlArg = "O=" + HttpUtility.UrlEncode(pTableName);

            if (StrFunc.IsFilled(pColumnName_Data))
                urlArg += @"&d=" + HttpUtility.UrlEncode(pColumnName_Data);
            if (StrFunc.IsFilled(pColumnName_Type))
                urlArg += @"&t=" + HttpUtility.UrlEncode(pColumnName_Type);
            if (StrFunc.IsFilled(pColumnName_FileName))
                urlArg += @"&f=" + HttpUtility.UrlEncode(pColumnName_FileName);
            //
            for (int i = 0; i < pKeyColumns.GetLength(0); i++)
            {
                urlArg += @"&kc" + HttpUtility.UrlEncode(i.ToString()) + @"=" + HttpUtility.UrlEncode(pKeyColumns[i]);
                urlArg += @"&kv" + HttpUtility.UrlEncode(i.ToString()) + @"=" + HttpUtility.UrlEncode(pKeyValues[i]);
                urlArg += @"&kd" + HttpUtility.UrlEncode(i.ToString()) + @"=" + HttpUtility.UrlEncode(pKeyDataTypes[i]);
            }

            WCToolTipHyperlink hlLinkFile = new WCToolTipHyperlink
            {
                CssClass = "fa-icon",
                Text = "<i class='fa fa-search'></i>",
                NavigateUrl = page + urlArg,
                Target = Cst.HyperLinkTargetEnum._blank.ToString(),
            };
            hlLinkFile.Style.Add(HtmlTextWriterStyle.PaddingLeft, "5px");
            hlLinkFile.Pty.TooltipContent = Ressource.GetString("imgConsultDoc");

            return hlLinkFile;
        }


        /// <summary>
        /// 
        /// </summary>
        // EG 20180426 Analyse du code Correction [CA2202]
        private void AddColumnData()
        {

            //Add des columns data via referentials[] 
            bool isSetDataKeyField = false;
            if (Referential != null)
            {
                for (int i = 0; i < Referential.Column.Length; i++)
                {
                    ReferentialsReferentialColumn rrc = Referential[i];
                    if (!(rrc.IsRole || rrc.IsItem))//NB: Les colonnes "Role" et "Item" ne sont pas dans le Select
                    {
                        #region Add column
                        //When isConsultation, Ressource already translated by LSTCONSULT class
                        string ressourceTranslated = (IsConsultation ? rrc.Ressource : Ressource.GetMulti(rrc.Ressource, 1));
                        #region isTemplateColumn
                        if (IsTemplateColumn)
                        {
                            TemplateColumn tc = new TemplateColumn
                            {
                                HeaderText = ressourceTranslated,
                                SortExpression = rrc.IDForItemTemplate,
                            };
                            //S'il s'agit de controls liés (ressource == null) : 1 header pour plusieurs cols (Ex: Tenor = Period + Frequency)
                            bool isHeaderWithMultiColumn = StrFunc.IsEmpty(rrc.Ressource)
                                || (rrc.Ressource == @"/") || (rrc.Ressource == @":");
                            if (isHeaderWithMultiColumn && !rrc.IsHideInDataGrid)
                                tc.HeaderText = HEADERWITHMULTICOLUMN + rrc.Ressource;
                            //
                            if (rrc.ExistsRelation)
                                tc.Visible = false;
                            else
                                tc.Visible = !rrc.IsHideInDataGrid;

                            ITemplate myItemTemplate;
                            // FI 20190318 [24568][24588] en mode export excel, il n'existe pas le label avec la ressource "Totale"  => Utilisation d'un ItemTemplate_Referencial
                            if (Referential.HasAggregateColumns && rrc.GroupBySpecified && rrc.GroupBy.AggregateSpecified && (false == IsModeExportExcel))
                                myItemTemplate = new ItemTemplateLabel_Referencial(rrc.IDForItemTemplate);
                            else
                                myItemTemplate = new ItemTemplate_Referencial(rrc.IDForItemTemplate);
                            tc.ItemTemplate = myItemTemplate;

                            // FI 20190318 [24568][24588] Test sur rrc.ControlMain avant d'instancier tc.EditItemTemplate
                            if (null != rrc.ControlMain)
                            {
                                EditItemTemplate_Referencial myEditItemTemplate = new EditItemTemplate_Referencial(rrc, false);
                                tc.EditItemTemplate = myEditItemTemplate;
                            }

                            if (TypeData.IsTypeDate(rrc.DataType.value))
                            {
                                tc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                            }
                            else if (TypeData.IsTypeDateTime(rrc.DataType.value) || TypeData.IsTypeTime(rrc.DataType.value))
                            {
                                //PL 20171129 New features for timezone suffix
                                tc.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                                if (rrc.AlignSpecified)
                                {
                                    if (rrc.Align.ToLower() == "right")
                                        tc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                                    else if (rrc.Align.ToLower() == "center")
                                        tc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                                }
                            }
                            // Si le champ est invisible on ne le formatte pas
                            else if (TypeData.IsTypeInt(rrc.DataType.value) && !rrc.IsHideInDataGrid)
                            {
                                tc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                            }
                            else if (TypeData.IsTypeDec(rrc.DataType.value))
                            {
                                tc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                            }
                            else if (TypeData.IsTypeString(rrc.DataType.value))
                            {
                                tc.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                                //PL 20121030 New features
                                if (rrc.AlignSpecified)
                                {
                                    if (rrc.Align.ToLower() == "right")
                                        tc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                                    else if (rrc.Align.ToLower() == "center")
                                        tc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                                }
                            }
                            this.Columns.Add(tc);
                            //
                            alColumnName.Add(rrc.IDForItemTemplate);
                            alColumnNameType.Add(COLUMN);
                            alColumnDataType.Add(rrc.DataType.value);
                            // FI 20171025 [23533] Alimentation de alColumnDataTypeLongForm
                            alColumnDataTypeLongForm.Add(rrc.DataType);

                            isExistColumnDataTypeBoolean = isExistColumnDataTypeBoolean || TypeData.IsTypeBool(rrc.DataType.value);
                            alColumnDataScale.Add((rrc.ScaleSpecified ? rrc.Scale.ToString() : string.Empty));
                            alColumnIdForReferencial.Add(i);
                            //PL 20110627
                            //alColumnIsResource.Add((rrc.IsResourceSpecified ? rrc.IsResource : false));
                            if (rrc.IsResourceSpecified && rrc.IsResource.IsResource)
                                alColumnIsResource.Add(rrc.IsResource);
                            else
                                alColumnIsResource.Add(new ReferentialsReferentialColumnResource(false));
                            //
                            alColumnIsTRIM.Add(rrc.IsTRIMSpecified && rrc.IsTRIM);
                            alColumnIsRTF.Add(rrc.IsRTFSpecified && rrc.IsRTF);
                            alColumnRessourceType.Add(StrFunc.IsEmpty(rrc.Ressource)
                                ?
                                RessourceTypeEnum.Empty.ToString()
                                :
                                (rrc.Ressource == @"/"
                                    ?
                                    RessourceTypeEnum.Denominator.ToString()
                                    :
                                    RessourceTypeEnum.Fill.ToString()
                                 ));
                            //
                            alColumnRowStyle.Add(rrc.RowStyleSpecified ? rrc.RowStyle : null);
                            alColumnCellStyle.Add(rrc.CellStyleSpecified ? rrc.CellStyle : null);
                            //
                            alColumnAggregate.Add(rrc.GroupBySpecified && rrc.GroupBy.AggregateSpecified ? rrc.GroupBy.Aggregate : null);
                            //
                            alColumnIsMandatory.Add(rrc.IsMandatorySpecified && rrc.IsMandatory);
                            alColumnLengthInDataGrid.Add(rrc.LengthInDataGridSpecified ? rrc.LengthInDataGrid : 0);
                            // RD 20100309 / Automatic Compute identifier
                            alColumnDefaultValue.Add(rrc.GetStringDefaultValue(Referential.TableName));
                            // EG 20110104 
                            isExistEditableColumn = isExistEditableColumn || rrc.IsEditable;
                            alColumnIsEditable.Add(rrc.IsEditableSpecified && rrc.IsEditable);
                            //
                            if (rrc.IsDataKeyField)
                            {
                                isSetDataKeyField = true;
                                this.DataKeyField = rrc.IDForItemTemplate;
                            }
                            if (rrc.IsKeyField & !isSetDataKeyField)
                                this.DataKeyField = rrc.IDForItemTemplate;
                            if (rrc.IsForeignKeyField)
                                this.ForeignKeyField = rrc.IDForItemTemplate;

                            if (ReferentialTools.IsHyperLinkAvailable(rrc))
                                isExistHyperLinkColumn = true;

                            #region ExistsRelation
                            if (rrc.ExistsRelation)
                            {
                                ItemTemplate_Referencial myItemTemplateRelation = new ItemTemplate_Referencial(rrc.IDForItemTemplateRelation);
                                EditItemTemplate_Referencial myEditItemTemplateRelation = new EditItemTemplate_Referencial(rrc, true);

                                TemplateColumn tcr = new TemplateColumn
                                {
                                    HeaderText = (IsConsultation ? rrc.Relation[0].ColumnSelect[0].Ressource : Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1)),
                                    SortExpression = rrc.IDForItemTemplateRelation,
                                    Visible = !rrc.IsHideInDataGrid,
                                    ItemTemplate = myItemTemplateRelation,
                                    EditItemTemplate = myEditItemTemplateRelation
                                };
                                // EG 20091110
                                tcr.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                                this.Columns.Add(tcr);

                                alColumnName.Add(rrc.IDForItemTemplateRelation);
                                alColumnNameType.Add(RELATION);
                                alColumnDataType.Add(rrc.Relation[0].ColumnSelect[0].DataType);
                                alColumnDataTypeLongForm.Add(new ReferentialsReferentialColumnDataType() { value = rrc.Relation[0].ColumnSelect[0].DataType });

                                isExistColumnDataTypeBoolean = isExistColumnDataTypeBoolean || TypeData.IsTypeBool(rrc.Relation[0].ColumnSelect[0].DataType);
                                alColumnDataScale.Add((rrc.ScaleSpecified ? rrc.Scale.ToString() : string.Empty));
                                alColumnIdForReferencial.Add(i);
                                //PL 20110627
                                //alColumnIsResource.Add((rrc.IsResourceSpecified ? rrc.IsResource : false));
                                if (rrc.IsResourceSpecified && rrc.IsResource.IsResource)
                                    alColumnIsResource.Add(rrc.IsResource);
                                else
                                    alColumnIsResource.Add(new ReferentialsReferentialColumnResource(false));
                                alColumnIsTRIM.Add(rrc.IsTRIMSpecified && rrc.IsTRIM);
                                alColumnRessourceType.Add(StrFunc.IsEmpty(rrc.Ressource)
                                    ?
                                    RessourceTypeEnum.Empty.ToString()
                                    :
                                    (rrc.Ressource == @"/"
                                        ?
                                        RessourceTypeEnum.Denominator.ToString()
                                        :
                                        RessourceTypeEnum.Fill.ToString()
                                     ));
                                //
                                alColumnRowStyle.Add(rrc.RowStyleSpecified ? rrc.RowStyle : null);
                                alColumnCellStyle.Add(rrc.CellStyleSpecified ? rrc.CellStyle : null);
                                //
                                alColumnAggregate.Add(rrc.GroupBySpecified && rrc.GroupBy.AggregateSpecified ? rrc.GroupBy.Aggregate : null);
                                //
                                alColumnIsMandatory.Add(rrc.IsMandatorySpecified && rrc.IsMandatory);
                                alColumnLengthInDataGrid.Add((rrc.LengthInDataGridSpecified ? rrc.LengthInDataGrid : 0));
                                // RD 20100309 / Automatic Compute identifier
                                alColumnDefaultValue.Add(rrc.GetStringDefaultValue(Referential.TableName));
                                // EG 20110104 
                                alColumnIsEditable.Add(rrc.IsEditableSpecified && rrc.IsEditable);
                            }
                            #endregion ExistsRelation
                        }
                        #endregion isTemplateColumn
                        #region !isTemplateColumn
                        if (!IsTemplateColumn)
                        {
                            BoundColumn bc = new BoundColumn
                            {
                                DataField = rrc.IDForItemTemplate,
                                HeaderText = ressourceTranslated
                            };
                            //S'il s'agit de controls liés (ressource == null) : 1 header pour plusieurs cols (Ex: Tenor = Period + Frequency)
                            bool isHeaderWithMultiColumn = StrFunc.IsEmpty(rrc.Ressource)
                                || (rrc.Ressource == @"/") || (rrc.Ressource == @":");
                            if (isHeaderWithMultiColumn && !rrc.IsHideInDataGrid)
                                bc.HeaderText = HEADERWITHMULTICOLUMN + rrc.Ressource;
                            //
                            bc.SortExpression = bc.DataField;
                            if (rrc.ExistsRelation)
                                bc.Visible = false;
                            else
                                bc.Visible = !rrc.IsHideInDataGrid;
                            //
                            if (TypeData.IsTypeDate(rrc.DataType.value))
                            {
                                bc.DataFormatString = "{0:d}";
                                bc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                            }
                            else if (TypeData.IsTypeDateTime(rrc.DataType.value))
                            {
                                bc.DataFormatString = "{0:G}";
                                bc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                            }
                            else if (TypeData.IsTypeTime(rrc.DataType.value))
                            {
                                bc.DataFormatString = "{0:T}";
                                bc.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                            }
                            // Si le champ est invisible on ne le formatte pas pour eviter les problemes avec le ROWVERSION dans le cas ou celui ci est DataKeyField
                            // En effet on passe la valeur affichée en DKF lorsque l'on double clique sur une ligne pour en ouvrir le formulaire
                            // donc lorsqu'il s'agit du ROWVERSION, si la valeur est 99999 on passerai dans l'url "DKF=99 999" (si la valeur est formatée) et le querystring ne tolère pas les espaces
                            // on n'applique donc pas le formattage pour eviter cette erreur sur les champs de type int et invisible
                            //20060304 PL idem avec un DataKeyField (eg. IDPROCESS_L)
                            else if (TypeData.IsTypeInt(rrc.DataType.value) && !rrc.IsHideInDataGrid && !rrc.IsDataKeyField)
                            {
                                bc.DataFormatString = "{0:n0}";
                                bc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                            }
                            else if (TypeData.IsTypeDec(rrc.DataType.value))
                            {
                                //bc.DataFormatString = "{0:n}";
                                // 20090925 RD / Pour afficher les séparateurs de milliers 
                                bc.DataFormatString = "{0:" + (rrc.ScaleSpecified ? "N" + rrc.Scale.ToString() : "n") + "}";
                                bc.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                            }
                            else if (TypeData.IsTypeString(rrc.DataType.value))
                            {
                                //20091005 PL Add
                                bc.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                            }
                            this.Columns.Add(bc);
                            //
                            alColumnName.Add(rrc.IDForItemTemplate);
                            alColumnNameType.Add(COLUMN);
                            alColumnDataType.Add(rrc.DataType.value);
                            alColumnDataTypeLongForm.Add(rrc.DataType);
                            isExistColumnDataTypeBoolean = isExistColumnDataTypeBoolean || TypeData.IsTypeBool(rrc.DataType.value);
                            alColumnDataScale.Add((true == rrc.ScaleSpecified ? rrc.Scale.ToString() : string.Empty));
                            alColumnIdForReferencial.Add(i);
                            alColumnIsResource.Add(new ReferentialsReferentialColumnResource(false));
                            alColumnIsMandatory.Add(rrc.IsMandatorySpecified && rrc.IsMandatory);
                            alColumnLengthInDataGrid.Add(rrc.LengthInDataGridSpecified ? rrc.LengthInDataGrid : 0);
                            // RD 20100309 / Automatic Compute identifier
                            alColumnDefaultValue.Add(rrc.GetStringDefaultValue(Referential.TableName));
                            // EG 20110104 
                            alColumnIsEditable.Add(rrc.IsEditableSpecified && rrc.IsEditable);
                            //
                            if (rrc.IsDataKeyField)
                            {
                                isSetDataKeyField = true;
                                this.DataKeyField = bc.DataField;
                            }
                            if (rrc.IsKeyField & !isSetDataKeyField)
                                this.DataKeyField = bc.DataField;
                            if (rrc.IsForeignKeyField)
                                this.ForeignKeyField = rrc.IDForItemTemplate;

                            //FI 201110262 Mise en commentaire ce code est fait ds CheckForHyperLink
                            //if (rrc.ColumnName == "URL")
                            //    isExistHyperLink = true;

                            //FI 201110262 Fonction apellée ici
                            //CheckForHyperLink(rrc);
                            //FI 20140912 [XXXXX] call ReferentialTools.IsHyperLinkAvailable
                            if (ReferentialTools.IsHyperLinkAvailable(rrc))
                                isExistHyperLinkColumn = true;


                            #region ExistsRelation
                            if (rrc.ExistsRelation)
                            {
                                //FI 201110262 Fonction apellée plus haut
                                //CheckForHyperLink(rrc);

                                BoundColumn bcr = new BoundColumn
                                {
                                    DataField = rrc.IDForItemTemplateRelation,
                                    HeaderText = (IsConsultation ? rrc.Relation[0].ColumnSelect[0].Ressource : Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1)),
                                    Visible = !rrc.IsHideInDataGrid
                                };
                                bcr.SortExpression = bcr.DataField;
                                // EG 20091110
                                bcr.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                                this.Columns.Add(bcr);

                                alColumnName.Add(rrc.IDForItemTemplateRelation);
                                alColumnNameType.Add(RELATION);
                                alColumnDataType.Add(rrc.Relation[0].ColumnSelect[0].DataType);
                                alColumnDataTypeLongForm.Add(new ReferentialsReferentialColumnDataType() { value = rrc.Relation[0].ColumnSelect[0].DataType });
                                isExistColumnDataTypeBoolean = isExistColumnDataTypeBoolean || TypeData.IsTypeBool(rrc.Relation[0].ColumnSelect[0].DataType);
                                // FI 20200903 [XXXXX] alimentation de alColumnIsResource et alColumnDataScale
                                alColumnIsResource.Add(new ReferentialsReferentialColumnResource(false));
                                alColumnDataScale.Add(rrc.ScaleSpecified ? rrc.Scale.ToString() : string.Empty);
                                alColumnIdForReferencial.Add(i);
                                alColumnIsMandatory.Add(rrc.IsMandatorySpecified && rrc.IsMandatory);
                                alColumnLengthInDataGrid.Add(rrc.LengthInDataGridSpecified ? rrc.LengthInDataGrid : 0);
                                // RD 20100309 / Automatic Compute identifier
                                alColumnDefaultValue.Add(rrc.GetStringDefaultValue(Referential.TableName));
                                // EG 20110104 
                                alColumnIsEditable.Add(rrc.IsEditableSpecified && rrc.IsEditable);
                            }
                            #endregion ExistsRelation
                        }
                        #endregion !isTemplateColumn
                        #endregion Add column
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        private void SetFirstPage(Object Src, EventArgs e)
        {
            SetPageIndex(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        private void SetLastPage(Object Src, EventArgs e)
        {
            SetPageIndex(PageCount - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void OnSetPageNumber(Object Src, EventArgs e)
        {
            TextBox txt = new TextBox(); ;
            if (Src is WebControl lblGoToPage)
                txt = lblGoToPage.ID.EndsWith(TableRowSection.TableHeader.ToString()) ? txtSetPageNumberHeader : txtSetPageNumberFooter;

            int pageIndex;
            try { pageIndex = Convert.ToInt32(txt.Text) - 1; }
            catch { pageIndex = 0; txt.Text = "1"; }
            pageIndex = Math.Min(PageCount - 1, pageIndex);
            pageIndex = Math.Max(0, pageIndex);
            SetPageIndex(pageIndex);
        }

        /// <summary>
        /// Affectation CurrentPageIndex
        /// </summary>
        /// <param name="pIndex"></param>
        private void SetPageIndex(int pIndex)
        {
            CurrentPageIndex = pIndex;
            //si pagination personalisée, la source de donnée doit être rechargée avec les lignes correspondant à la page demandée
            //Sinon non, la source de donnée est déjà chargée
            IsLoadData = AllowCustomPaging;

        }

        /// <summary>
        /// Alimente un DataView à partir du jeu de résultat (DsData)
        ///<para>Affecte le DataSource avec le DefaultView issu du jeu de résultat</para>
        /// </summary>
        private void SetDataSource()
        {
            if (IsDataAvailable)
            {
                DataTable dt = DsData.Tables[0];
                //Attention très important, 
                //Lorsque le dataset n'est pas rechargé Spheres® récupère le même tri qu'avant le post
                DataView dv = dt.DefaultView;
                DataSource = dv;
            }
        }

        /// <summary>
        /// Définit l'ordre de tri de la source de donnée (DataSource de type DataView)
        /// </summary>
        private void SetDataSourceOrder()
        {
            if (IsDataSourceAvailable && IsDataFilled)
            {
                DataView dv = (DataView)DataSource;



                string[] sortingFieldsParsed = null;

                // MF 20120604 Ticket 17823
                if (!String.IsNullOrEmpty(SortExpression))
                {
                    sortingFieldsParsed = SortExpression.Split(',');

                    sortingFieldsParsed = sortingFieldsParsed.Where(elem => dv.Table.Columns.Contains(elem.Split(' ')[0])).ToArray();

                    if (sortingFieldsParsed.Length > 0)
                    {
                        ViewState[this.ClientID + "SortingFields"] =
                            sortingFieldsParsed
                            .Select(elem => elem.Split(' ')[0])
                            .Aggregate((curr, next) => String.Format("{0},{1}", curr, next));

                        ViewState[this.ClientID + "SortingOrders"] =
                            sortingFieldsParsed
                            .Select(elem => elem.Split(' ')[1])
                            .Aggregate((curr, next) => String.Format("{0},{1}", curr, next));

                        SortExpression = sortingFieldsParsed.Aggregate((curr, next) => String.Format("{0},{1}", curr, next));
                    }
                    else
                    {
                        ViewState[this.ClientID + "SortingFields"] = null;
                        ViewState[this.ClientID + "SortingOrders"] = null;

                        SortExpression = null;
                    }
                }

                dv.Sort = SortExpression;
            }
        }

        /// <summary>
        /// Définie l'index de page affiché
        /// </summary>
        private void SetCurrentPageIndex()
        {
            if ((TotalRowscount > 0) && PageSize > 0)
            {
                int maxItem = Math.DivRem(TotalRowscount, PageSize, out int result);
                if (result == 0)
                    maxItem--;

                // 20120411 MF Test bug template datagrid
                if (CurrentPageIndex != Math.Min(CurrentPageIndex, maxItem))
                {
                    JavaScript.ReloadImmediate((PageBase)this.Page, Ressource.GetString("BackToTheFirstPage"), "0", "SELFRELOAD_");
                }
                CurrentPageIndex = Math.Min(CurrentPageIndex, maxItem);
            }
            else
            {
                CurrentPageIndex = 0;
            }
        }

        /// <summary>
        /// Constitution de l'URL pour accès au formulaire de saisie par dblclick en mode isFormInputMode
        /// </summary>
        // EG 20170125 [Refactoring URL] Upd
        public string GetURLForInsert()
        {
            return GetURL("INSERT", string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Obtient URL quui ouvre le Formulaire de saisie
        /// </summary>
        /// <param name="pDataGridItem"></param>
        /// <returns></returns>
        // EG 20170125 [Refactoring URL] Upd
        private string GetURLForUpdate(DataGridItem pDataGridItem)
        {
            string ret = string.Empty;
            //FI 20120113 ajout du contrôle Referential.IndexDataKeyField > -1
            if (Referential.IndexDataKeyField > -1)
            {
                string data = GetCellValue(pDataGridItem, Referential.IndexColSQL_DataKeyField + nbColumnAction);
                if (StrFunc.IsFilled(data) && (data != Cst.HTMLSpace))
                {
                    //FI 20120111 Add FormatToInvariant
                    if (false == Referential.IsROWVERSIONDataKeyField)////FI 20110112 Les colonnes ROWVERSION ne sont pas formattées, en plus FormatToInvariant plante si donnée de type ROWVERSION
                        data = FormatToInvariant(data, Referential[Referential.IndexDataKeyField].DataType.value);

                    ret = GetURL("UPDATE", DataKeyField, data, pDataGridItem);
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20110202 Add Boucle Array
        private string AddDynamicArgument()
        {
            string da = string.Empty;
            if (StrFunc.IsFilled(queryStringDA))
            {
                //PL 20160914 Use EncodeDA()
                da += "&DA=" + HttpUtility.UrlEncode(Ressource.EncodeDA(queryStringDA), Encoding.Default);
            }
            else if (ArrFunc.IsFilled(Referential.dynamicArgs))
            {
                // FI 20200205 [XXXXX] Seuls les paramètres GUI et URL sont transmis
                string[] xmlDynamicArgs = ReferentialTools.DynamicDatasToString(Referential.dynamicArgs, DynamicDataSourceEnum.GUI | DynamicDataSourceEnum.URL);
                da += "&DA=";
                for (int i = 0; i < ArrFunc.Count(xmlDynamicArgs); i++)
                {
                    //PL 20160914 Use EncodeDA()
                    da += HttpUtility.UrlEncode(Ressource.EncodeDA(xmlDynamicArgs[i]), Encoding.Default) + StrFunc.StringArrayList.LIST_SEPARATOR;
                }
            }
            return da;
        }

        /// <summary>
        /// initialisation des propriétés du datagrid
        /// </summary>
        private void InitTemplateDataGrid()
        {

            #region Initialisation des propriétés du DataGrid
            // Default settings for the grid Pager
            PagerStyle.Mode = PagerMode.NumericPages;
            PagerStyle.CssClass = "DataGrid_PagerStyle";
            PagerStyle.PageButtonCount = 10;
            PagerStyle.Visible = true;
            PagerStyle.HorizontalAlign = HorizontalAlign.Left;
            //
            if (SessionTools.IsPagerPositionTopAndBottom)
                PagerStyle.Position = PagerPosition.TopAndBottom;
            else
                PagerStyle.Position = PagerPosition.Bottom;
            //	
            // Default settings for pagination
            SetPaging(SessionTools.NumberRowByPage);
            //
            // Other visual default settings
            GridLines = GridLines.Both;
            CellSpacing = 0;
            CellPadding = 3;
            Width = Unit.Percentage(100);
            CssClass = "DataGrid";
            //CssClass = "FixedTables"; //EG FixedTable
            AutoGenerateColumns = false;
            //
            // EditItemStyle
            EditItemStyle.CssClass = "DataGrid_SelectedItemStyle";
            EditItemStyle.CssClass = EFSCssClass.Capture;
            //
            // Settings for normal items (all or odd-only rows)
            ItemStyle.CssClass = "DataGrid_ItemStyle";
            ItemStyle.Wrap = false;
            //
            // Settings for alternating items (none or even-only rows)
            AlternatingItemStyle.CssClass = "DataGrid_AlternatingItemStyle";
            AlternatingItemStyle.Wrap = false;
            //
            // Settings for Selected items (none or even-only rows)
            SelectedItemStyle.CssClass = "DataGrid_SelectedItemStyle";
            SelectedItemStyle.Wrap = false;
            //
            // Settings for Footer
            ShowFooter = false;
            FooterStyle.HorizontalAlign = HorizontalAlign.Left;
            FooterStyle.CssClass = "DataGrid_FooterStyle";
            //
            // Settings for heading  
            HeaderStyle.CssClass = "DataGrid_HeaderStyle";
            HeaderStyle.Wrap = false;
            //
            // Sorting  
            AllowSorting = true;
            ViewState[this.ClientID + "SortedAscending"] = "yes";
            //
            #endregion Initialisation des propriétés du DataGrid

            #region Set event handlers
            CancelCommand += new DataGridCommandEventHandler(OnCancelCommand);
            EditCommand += new DataGridCommandEventHandler(OnEditCommand);
            DeleteCommand += new DataGridCommandEventHandler(OnDeleteCommand);
            UpdateCommand += new DataGridCommandEventHandler(OnUpdateCommand);
            ItemCommand += new DataGridCommandEventHandler(OnItemCommand);
            SortCommand += new DataGridSortCommandEventHandler(OnSortCommand);
            ItemCreated += new DataGridItemEventHandler(OnItemCreated);
            ItemDataBound += new DataGridItemEventHandler(OnItemDataBound);
            PageIndexChanged += new DataGridPageChangedEventHandler(OnPageIndexChanged);
            SelectedIndexChanged += new EventHandler(OnSelectedIndexChanged);
            #endregion Set event handlers

        }

        /// <summary>
        /// Définie la pagination du datagrid
        /// <para>Lorsque la pagination est customisée elle est effectué par le SGBD</para>
        /// <para>Si pNumberRowByPage &lt; 0 alors le datagrid affiche une seule page</para>
        /// </summary>
        /// <param name="pNumberRowByPage">nbr de ligne par page</param>
        public void SetPaging(int pNumberRowByPage)
        {
            AllowPaging = (pNumberRowByPage >= 0);
            //
            if ((AllowPaging) && (pNumberRowByPage > 0))
                PageSize = pNumberRowByPage;
            //

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDynamicDatas">Représentes les donnéees</param>
        /// <param name="pIsInitReferential">Retourne true lorsque L'appel aux méthodes Init a été effectué</param>
        /// <param name="pIsLoadLSTParam">si true, les valeurs des pDynamicArgs de type GUI sont remplacées par les valeurs présentes dans LSTPARAM</param>
        /// FI 20200205 [XXXXX] pDynamicDatas est de type ReferentialsReferentialStringDynamicData
        /// FI 20200602 [25370] add parameter pIsLoadLSTParam
        public void LoadReferential(Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicDatas, Boolean pIsInitReferential, Boolean pIsLoadLSTParam)
        {


            //PL 20120618 Add test on PROCESSDET_L (A voir pour généraliser...)
            //if (isSelectMode)
            if (IsSelectMode
                || StrFunc.ContainsIn(IdMenu.GetIdMenu(IdMenu.Menu.PROCESSDET_L), this.IDMenu)
                || StrFunc.ContainsIn(IdMenu.GetIdMenu(IdMenu.Menu.LOGREQUESTDET_L), this.IDMenu)
                )
                InitForSelectMode();

            if (ArrFunc.IsFilled(NVC_QueryString.AllKeys))
            {
                condApp = NVC_QueryString["CondApp"];
                param = ReferentialTools.GetQueryStringParam(Page);
            }

            // FI 2020021 [XXXXX] Alimentation de referential uniquement si isSelfReload ou isSelfClear (sauf si rafraîchissement via le contrôle imgRefresh)
            // Pas d'appel lors d'un chgt de page du grid ou lors d'une recherche via le filtre libre ou lors du rafraîchissement via le contrôle imgRefresh
            if (pIsInitReferential)
            {
                switch (ReferentialMode)
                {
                    case ReferentialModeEnum.SQL:
                        SQLInit_LoadReferential(pDynamicDatas, pIsLoadLSTParam);
                        break;
                    case ReferentialModeEnum.XML:
                        XMLInit_LoadReferential2(pDynamicDatas, pIsLoadLSTParam);
                        break;
                    default:
                        throw new InvalidProgramException($"Referential Mode {ReferentialMode} not supported");
                }
            }

            Referential.SetPermission(IDMenu);

            // PM 20240604 [XXXXX] Ajout gestion GUID pour restrictions
            if (Referential.IsTableAttachedDoc)
            {
                string parentGUID = NVC_QueryString["GUID"];
                if (StrFunc.IsFilled(parentGUID))
                {
                    ReferentialsReferential referential = DataCache.GetData<ReferentialsReferential>(parentGUID + "_Referential");
                    if (referential != default(ReferentialsReferential))
                    {
                        Referential.Create = referential.Create;
                        Referential.CreateSpecified = referential.CreateSpecified;
                        Referential.Modify = referential.Modify;
                        Referential.ModifySpecified = referential.ModifySpecified;
                        Referential.Remove = referential.Remove;
                        Referential.RemoveSpecified = referential.RemoveSpecified;
                    }
                }
            }

            // FI 20200220 [XXXXX] Appel ici (avant été effectué dans {SQLInit|XMLInit}_LoadReferential et cette dernière méthode n'étant plus appelée systématiquement)
            if ((IsConsultation && IsTemplateColumn) || (!IsConsultation))
                ReferentialTools.CreateControls((PageBase)this.Page, Referential, "grid", false, false);

            if (false == IsConsultation)
            {
                // FI 20200220 [XXXXX] Appel à SetIndexForFilter (était effectué auparavant dans XMLInit_LoadReferential)
                if (IsGridSelectMode)
                    SetIndexForFilter();
                // FI 20200220 [XXXXX] Alimentation de sessionName_LstColumn ici (était effectué auparavant dans XMLInit_LoadReferential)
                sessionName_LstColumn = ReferentialWeb.SaveInSession_LstColumn((PageBase)this.Page, LocalLstConsult.IdLstConsult, IdLstTemplate, IdA, Referential);
            }

            if ((LocalLstConsult.template != null))
                SetPaging(LocalLstConsult.template.ROWBYPAGE);

            

            AddColumns();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        // EG 20210222 [XXXXX] Suppression inscription function DoPostBackImmediate (présent dans PageBase.js)
        public void AddItem(Object Src, EventArgs e)
        {

            if (!IsLocked)
            {
                //
                if (!IsDataModified && false == (DataFrom__EVENTTARGET == "PAGE" && DataFrom__EVENTARGUMENT == "ADDROW"))
                    PageIndexBeforeInsert = this.CurrentPageIndex;
                //
                if (!IsLastPage)
                {
                    //if not is last page, go to last page and re-insert
                    CurrentPageIndex = PageCount - 1;
                    JavaScript.CallFunction((PageBase)this.Page, "DoPostBackImmediate('PAGE','ADDROW')");
                }
                else
                {
                    // Add a blank row to main Table
                    DataRow dr = DsData.Tables[0].NewRow();
                    if (DsData.Tables[0].Rows.Count == 0)
                        Referential.InitializeNewRow(ref dr);
                    else
                    {
                        DataRow drPrevious = null;
                        for (int i = DsData.Tables[0].Rows.Count - 1; i >= 0; i--)
                        {
                            drPrevious = DsData.Tables[0].Rows[i];
                            if (drPrevious.RowState == DataRowState.Deleted)
                                drPrevious = null;
                            else
                                break;
                        }
                        Referential.InitializeNewRow(ref dr, drPrevious);
                    }

                    DsData.Tables[0].Rows.Add(dr);
                    // If the is full, move to next page. In this case, first item
                    int nNewItemIndex = this.Items.Count;
                    DataView dv = DsData.Tables[0].DefaultView;
                    dv.RowStateFilter = DataViewRowState.CurrentRows | DataViewRowState.Deleted;
                    int nCountItemIndex = dv.Count;
                    if ((nCountItemIndex - 1) / this.PageSize == this.PageCount)
                    {
                        this.CurrentPageIndex++;
                        nNewItemIndex = 0;
                    }

                    // Turn edit mode on for the newly added row
                    this.EditItemIndex = nNewItemIndex;

                    // Refresh the grid
                    BindData();
                    //
                    IsDataModified = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDt"></param>
        /// EG 20091106 Total for invoice
        /// EG 20091125 Total for invoice (sign SUM of CN)
        private void AddSubTotalToINVOICEFEESDETAIL(DataTable pDt)
        {

            AllowSorting = false;//Interdiction du tri par colonne
            string newKey = string.Empty;
            string lastKey = string.Empty;
            decimal sum_FEE_Amount = 0;
            int countEntry = 0;
            DataRow dr;
            int totalRows = pDt.Rows.Count;
            int currentRow = 0;
            while (currentRow <= totalRows)
            {
                if (currentRow == totalRows)
                {
                    #region End Of Table
                    lastKey = "EOT";
                    countEntry++;
                    #endregion End Of Table
                }
                else
                {
                    #region Init key
                    newKey = string.Empty;
                    newKey += pDt.Rows[currentRow]["IDT_INVOICE"].ToString() + "|";
                    countEntry++;
                    #endregion Init key
                }
                if ((lastKey == newKey) || (0 == currentRow))
                {
                    #region Construct total
                    lastKey = newKey;
                    sum_FEE_Amount += Convert.ToDecimal(pDt.Rows[currentRow]["FEE_AMOUNT"]);
                    #endregion Construct total
                }
                else
                {
                    #region Add new row for GrossTurnOver
                    dr = pDt.NewRow();
                    Referential.InitializeNewRow(ref dr);
                    dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                    dr["IDT"] = dr["IDT_INVOICE"];
                    dr["GPRODUCT"] = dr["GPRODUCT_INVOICE"];

                    dr["IDENTIFIER_TRADE"] = Ressource.GetString("GrossTurnOverAmount");
                    dr["FEE_AMOUNT"] = sum_FEE_Amount;

                    dr["IDA_PAY"] = 0;
                    dr["IDA_CTR"] = 0;
                    dr["IDI"] = 0;
                    dr["IDFEESCHEDULE"] = 0;
                    dr["IDFEE"] = 0;
                    dr["DTTRADE"] = Convert.DBNull;
                    dr["pay_IDENTIFIER"] = Convert.DBNull;
                    dr["ctr_IDENTIFIER"] = Convert.DBNull;
                    dr["TRADER"] = 0;
                    dr["trader_IDENTIFIER"] = Convert.DBNull;
                    dr["f2_IDENTIFIER"] = Convert.DBNull;
                    dr["f1_IDENTIFIER"] = Convert.DBNull;
                    dr["FORMULADCF"] = Convert.DBNull;
                    //PL 20141023
                    //dr["ASSESSMENTBASISVALUE"] = Convert.DBNull;
                    dr["ASSESSMENTBASISVALUE1"] = Convert.DBNull;
                    dr["ASSESSMENTBASISVALUE2"] = Convert.DBNull;
                    dr["ti_IDENTIFIER"] = Convert.DBNull;
                    //
                    countEntry--;
                    dr["ROWVERSION"] = -1;
                    dr["ROWATTRIBUT"] = "T";
                    pDt.Rows.InsertAt(dr, currentRow);
                    totalRows++;
                    currentRow++;
                    #endregion Add new row for GrossTurnOver
                    #region Add new row for Rebate
                    if (false == Convert.IsDBNull(pDt.Rows[currentRow - 1]["REBATE_AMOUNT"]))
                    {

                        dr = pDt.NewRow();
                        Referential.InitializeNewRow(ref dr);
                        dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                        dr["IDA_INVOICED"] = 0;
                        dr["ac_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDB_INVOICED"] = 0;
                        dr["bk_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDT"] = 0;
                        dr["IDT_INVOICE"] = Convert.DBNull;
                        dr["IDENTIFIER_INVOICE"] = Convert.DBNull;
                        dr["IDSTENVIRONMENT"] = Convert.DBNull;
                        dr["IDENTIFIER_TRADE"] = Ressource.GetString("GlobalRebateAmount");
                        dr["FEE_AMOUNT"] = dr["REBATE_AMOUNT"];
                        dr["FEE_CUR"] = dr["REBATE_CUR"];
                        //
                        countEntry--;
                        dr["ROWVERSION"] = -2;
                        dr["ROWATTRIBUT"] = "T";
                        pDt.Rows.InsertAt(dr, currentRow);
                        totalRows++;
                        currentRow++;
                    }
                    #endregion Add new row for Rebate
                    #region Add new row for NetTurnOverIssue
                    if (false == Convert.IsDBNull(pDt.Rows[currentRow - 1]["AI_NETISSUE_AMOUNT"]) ||
                       (false == Convert.IsDBNull(pDt.Rows[currentRow - 1]["CN_NETISSUE_AMOUNT"])))
                    {
                        dr = pDt.NewRow();
                        Referential.InitializeNewRow(ref dr);
                        dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                        dr["IDA_INVOICED"] = 0;
                        dr["ac_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDB_INVOICED"] = 0;
                        dr["bk_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDT"] = 0;
                        dr["IDENTIFIER_TRADE"] = Ressource.GetString("InitNetTurnOverIssueAmount");
                        dr["FEE_AMOUNT"] = dr["NETISSUE_AMOUNT"];
                        dr["FEE_CUR"] = dr["NETISSUE_CUR"].ToString();
                        dr["IDT_INVOICE"] = Convert.DBNull;
                        dr["IDENTIFIER_INVOICE"] = Convert.DBNull;
                        dr["IDSTENVIRONMENT"] = Convert.DBNull;
                        //
                        countEntry--;
                        dr["ROWVERSION"] = -2;
                        dr["ROWATTRIBUT"] = "T";
                        pDt.Rows.InsertAt(dr, currentRow);
                        totalRows++;
                        currentRow++;
                    }
                    #endregion Add new row for NetTurnOverIssue
                    #region Add new row for Sum of AdditionalInvoice
                    if (false == Convert.IsDBNull(pDt.Rows[currentRow - 1]["AI_NETISSUE_AMOUNT"]))
                    {
                        dr = pDt.NewRow();
                        Referential.InitializeNewRow(ref dr);
                        dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                        dr["IDA_INVOICED"] = 0;
                        dr["ac_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDB_INVOICED"] = 0;
                        dr["bk_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDT"] = 0;
                        dr["IDT_INVOICE"] = Convert.DBNull;
                        dr["IDENTIFIER_INVOICE"] = Convert.DBNull;
                        dr["IDSTENVIRONMENT"] = Convert.DBNull;
                        dr["IDENTIFIER_TRADE"] = Ressource.GetString("AI_NETISSUE_AMOUNT");
                        dr["FEE_AMOUNT"] = dr["AI_NETISSUE_AMOUNT"];
                        dr["FEE_CUR"] = dr["AI_NETISSUE_CUR"].ToString();
                        //
                        countEntry--;
                        dr["ROWVERSION"] = -3;
                        dr["ROWATTRIBUT"] = "T";
                        pDt.Rows.InsertAt(dr, currentRow);
                        totalRows++;
                        currentRow++;
                    }
                    #endregion Add new row for Sum of AdditionalInvoice
                    #region Add new row for Sum of CreditNote
                    if (false == Convert.IsDBNull(pDt.Rows[currentRow - 1]["CN_NETISSUE_AMOUNT"]))
                    {
                        dr = pDt.NewRow();
                        Referential.InitializeNewRow(ref dr);
                        dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                        dr["IDA_INVOICED"] = 0;
                        dr["ac_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDB_INVOICED"] = 0;
                        dr["bk_invoiced_IDENTIFIER"] = Convert.DBNull;
                        dr["IDT"] = 0;
                        dr["IDT_INVOICE"] = Convert.DBNull;
                        dr["IDENTIFIER_INVOICE"] = Convert.DBNull;
                        dr["IDSTENVIRONMENT"] = Convert.DBNull;
                        dr["IDENTIFIER_TRADE"] = Ressource.GetString("CN_NETISSUE_AMOUNT");
                        dr["FEE_AMOUNT"] = -1 * Convert.ToDecimal(dr["CN_NETISSUE_AMOUNT"]);
                        dr["FEE_CUR"] = dr["CN_NETISSUE_CUR"].ToString();
                        //
                        countEntry--;
                        dr["ROWVERSION"] = -3;
                        dr["ROWATTRIBUT"] = "T";
                        pDt.Rows.InsertAt(dr, currentRow);
                        totalRows++;
                        currentRow++;

                    }
                    #endregion Add new row for Sum of CreditNote

                    #region Add new row for ReelNetTurnOverIssue
                    dr = pDt.NewRow();
                    Referential.InitializeNewRow(ref dr);
                    dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                    dr["IDA_INVOICED"] = 0;
                    dr["ac_invoiced_IDENTIFIER"] = Convert.DBNull;
                    dr["IDB_INVOICED"] = 0;
                    dr["bk_invoiced_IDENTIFIER"] = Convert.DBNull;
                    dr["IDT"] = 0;
                    dr["IDT_INVOICE"] = Convert.DBNull;
                    dr["IDENTIFIER_INVOICE"] = Convert.DBNull;
                    dr["IDSTENVIRONMENT"] = Convert.DBNull;
                    dr["IDENTIFIER_TRADE"] = Ressource.GetString("NetTurnOverIssueAmount");
                    decimal netTurnOverIssueAmount = Convert.ToDecimal(dr["NETISSUE_AMOUNT"]);
                    if (false == Convert.IsDBNull(dr["AI_NETISSUE_AMOUNT"]))
                        netTurnOverIssueAmount += Convert.ToDecimal(dr["AI_NETISSUE_AMOUNT"]);
                    // EG 20091125 Total for invoice (sign SUM of CN)
                    if (false == Convert.IsDBNull(dr["CN_NETISSUE_AMOUNT"]))
                        netTurnOverIssueAmount -= Convert.ToDecimal(dr["CN_NETISSUE_AMOUNT"]);
                    dr["FEE_AMOUNT"] = netTurnOverIssueAmount;
                    dr["FEE_CUR"] = dr["NETISSUE_CUR"].ToString();
                    //
                    countEntry--;
                    dr["ROWVERSION"] = -2;
                    dr["ROWATTRIBUT"] = "T";
                    pDt.Rows.InsertAt(dr, currentRow);
                    totalRows++;
                    currentRow++;
                    #endregion Add new row for ReelNetTurnOverIssue
                    #region Add new row for ReelNetTurnOverAccounting
                    dr = pDt.NewRow();
                    Referential.InitializeNewRow(ref dr);
                    dr.ItemArray = (object[])pDt.Rows[currentRow - 1].ItemArray.Clone();
                    dr["IDENTIFIER_TRADE"] = Ressource.GetString("InitNetTurnOverAccountingAmount");
                    decimal netTurnOverAccountingAmount = 0;
                    if (false == Convert.IsDBNull(dr["NETACCOUNTING_AMOUNT"]))
                        netTurnOverAccountingAmount += Convert.ToDecimal(dr["NETACCOUNTING_AMOUNT"]);
                    if (false == Convert.IsDBNull(dr["AI_NETACCOUNTING_AMOUNT"]))
                        netTurnOverAccountingAmount += Convert.ToDecimal(dr["AI_NETACCOUNTING_AMOUNT"]);
                    // EG 20091125 Total for invoice (sign SUM of CN)
                    if (false == Convert.IsDBNull(dr["CN_NETISSUE_AMOUNT"]))
                        netTurnOverAccountingAmount -= Convert.ToDecimal(dr["CN_NETACCOUNTING_AMOUNT"]);
                    dr["FEE_AMOUNT"] = netTurnOverAccountingAmount;
                    dr["FEE_CUR"] = dr["NETACCOUNTING_CUR"].ToString();
                    //
                    countEntry--;
                    dr["ROWVERSION"] = 0;
                    dr["ROWATTRIBUT"] = "T";
                    pDt.Rows.InsertAt(dr, currentRow);
                    totalRows++;
                    currentRow++;
                    #endregion Add new row for ReelNetTurnOverAccounting

                    if (currentRow < totalRows)
                    {
                        #region Construct total
                        lastKey = newKey;
                        countEntry = 1;
                        sum_FEE_Amount = Convert.ToDecimal(pDt.Rows[currentRow]["FEE_AMOUNT"]);
                        #endregion Construct total
                    }
                    if (currentRow < totalRows)
                    {
                        #region Construct total
                        lastKey = newKey;
                        countEntry = 1;
                        sum_FEE_Amount = Convert.ToDecimal(pDt.Rows[currentRow]["FEE_AMOUNT"]);
                        #endregion Construct total
                    }
                }
                currentRow++;
            }
        }

        /// <summary>
        /// Calcul et insertion des lignes de sous-totaux et totaux sur la consultation du Journal comptables (VW_ACCDAYBOOK)
        /// </summary>
        /// <param name="pDt"></param>
        private void AddSubTotalToACCDAYBOOK(DataTable pDt)
        {
            AllowSorting = false;//Interdiction du tri par colonne
            string newKey = string.Empty;
            string lastKey = string.Empty;
            decimal sumAmount = 0;
            int countEntry = 0;
            DataRow dr;
            int totalRows = pDt.Rows.Count;
            int currentRow = 0;
            while (currentRow <= totalRows)
            {
                if (currentRow == totalRows)
                {
                    #region End Of Table
                    lastKey = "EOT";
                    countEntry++;
                    #endregion End Of Table
                }
                else
                {
                    #region Init key
                    newKey = string.Empty;
                    newKey += pDt.Rows[currentRow]["IDT"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDA_PARTY"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDEAR"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["DTEVENT"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["DTENTRY"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDA_ENTITY"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["ACCMODEL"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["INSTRENV"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["INSTRENVDET"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDACCSCHEME"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDC"].ToString() + "|";
                    countEntry++;
                    #endregion Init key
                }
                if ((lastKey == newKey) || (currentRow == 0))
                {
                    #region Construct total
                    lastKey = newKey;
                    if (pDt.Rows[currentRow]["DEBIT_CREDIT"].ToString().ToUpper() == "DEBIT")
                        sumAmount -= Convert.ToDecimal(pDt.Rows[currentRow]["AMOUNT"]);
                    else
                        sumAmount += Convert.ToDecimal(pDt.Rows[currentRow]["AMOUNT"]);
                    #endregion Construct total
                }
                else
                {
                    #region Add new row for total
                    dr = pDt.NewRow();
                    Referential.InitializeNewRow(ref dr);
                    if (sumAmount > 0)
                        dr["DEBIT_CREDIT"] = "Unbalanced (Credit)";
                    else if (sumAmount < 0)
                        dr["DEBIT_CREDIT"] = "Unbalanced (Debit)";
                    else
                        dr["DEBIT_CREDIT"] = "Well-balanced";
                    //
                    countEntry--;
                    dr["LABELVALUE"] = countEntry.ToString() + (countEntry == 1 ? " entry." : " entries.");
                    //Warning: A REVOIR: Utilisation en DUR de ROWATTRIBUT='T' et ROWVERSION=0 pour GetCssClassForSideValue() (voir OnItemDataBound() plus bas dans ce fichier)
                    dr["ROWVERSION"] = 0;
                    dr["ROWATTRIBUT"] = "T";
                    //
                    dr["IDT"] = pDt.Rows[currentRow - 1]["IDT"].ToString();
                    dr["IDA_PARTY"] = pDt.Rows[currentRow - 1]["IDA_PARTY"].ToString();
                    dr["IDEAR"] = pDt.Rows[currentRow - 1]["IDEAR"].ToString();
                    dr["DTEVENT"] = pDt.Rows[currentRow - 1]["DTEVENT"].ToString();
                    dr["DTENTRY"] = pDt.Rows[currentRow - 1]["DTENTRY"].ToString();
                    dr["IDA_ENTITY"] = pDt.Rows[currentRow - 1]["IDA_ENTITY"].ToString();
                    dr["ACCMODEL"] = pDt.Rows[currentRow - 1]["ACCMODEL"].ToString();
                    dr["INSTRENV"] = pDt.Rows[currentRow - 1]["INSTRENV"].ToString();
                    dr["INSTRENVDET"] = pDt.Rows[currentRow - 1]["INSTRENVDET"].ToString();
                    dr["IDACCSCHEME"] = pDt.Rows[currentRow - 1]["IDACCSCHEME"].ToString();
                    if (sumAmount != 0)
                    {
                        dr["IDC"] = pDt.Rows[currentRow - 1]["IDC"].ToString();
                        dr["AMOUNT"] = sumAmount;
                    }
                    else
                        dr["IDC"] = Convert.DBNull;
                    //
                    dr["TRADE_IDENTIFIER"] = pDt.Rows[currentRow - 1]["TRADE_IDENTIFIER"].ToString();
                    dr["a1_IDENTIFIER"] = pDt.Rows[currentRow - 1]["a1_IDENTIFIER"].ToString();
                    dr["a2_IDENTIFIER"] = pDt.Rows[currentRow - 1]["a2_IDENTIFIER"].ToString();
                    dr["a3_IDENTIFIER"] = pDt.Rows[currentRow - 1]["a3_IDENTIFIER"].ToString();
                    dr["b1_IDENTIFIER"] = pDt.Rows[currentRow - 1]["b1_IDENTIFIER"].ToString();
                    dr["b2_IDENTIFIER"] = pDt.Rows[currentRow - 1]["b2_IDENTIFIER"].ToString();
                    dr["ACCSCHEME_IDENTIFIER"] = pDt.Rows[currentRow - 1]["ACCSCHEME_IDENTIFIER"].ToString();
                    //
                    pDt.Rows.InsertAt(dr, currentRow);
                    totalRows++;
                    currentRow++;
                    #endregion Add new row for total
                    //
                    if (currentRow < totalRows)
                    {
                        #region Construct total
                        lastKey = newKey;
                        countEntry = 1;
                        if (pDt.Rows[currentRow]["DEBIT_CREDIT"].ToString().ToUpper() == "DEBIT")
                            sumAmount = 0 - Convert.ToDecimal(pDt.Rows[currentRow]["AMOUNT"]);
                        else
                            sumAmount = 0 + Convert.ToDecimal(pDt.Rows[currentRow]["AMOUNT"]);
                        #endregion Construct total
                    }
                }
                currentRow++;
            }

        }

        /// <summary>
        /// Calcul et mise à jour des lignes de résultat sur la consultation du Contrôle des risques sur flux (FLOWSRISKCTRL_ALLOC)
        /// </summary>
        /// <param name="pDt"></param>
        private void CalcRiskToFLOWSRISKCTRL_ALLOC(DataTable pDt)
        {
            AllowSorting = false;//Interdiction du tri par colonne

            //Warning: Le Select SQL doit impérativement alimenter une colonne "CLASSRECORD" avec ces valeurs.
            const string CLASSRECORD_FLOWS = "1_FLO", CLASSRECORD_CAPITAL = "2_CAP", CLASSRECORD_TOTAL = "3_TOT";
            const string DISPLAYMODE_ALL = "ALL", DISPLAYMODE_ALLPLUS = "ALL+", DISPLAYMODE_ERROR = "ERROR", DISPLAYMODE_WARNING = "WARNING", DISPLAYMODE_NOTSUCCESS = "NOTSUCCESS";

            string cs = SessionTools.CS;
            FpML.Interface.IProductBase productBase = Tools.GetNewProductBase();

            DateTime dtFixing = Convert.ToDateTime(Referential.dynamicArgs["DATE2"].value);
            decimal thresholdReference = Convert.ToDecimal(Referential.dynamicArgs["THRESHOLD"].value) / 100;
            string displayMode = Referential.dynamicArgs["DISPLAYMODE"].value;

            bool isMissingExchangeRateOnFlows = false, isMissingExchangeRateOnCapital = false, isMissingCapital = false, isInvalidCapital = false;
            string classRecord, idc, idcCtrval, information;
            EFS.ACommon.ProcessStateTools.StatusEnum riskStatus;
            decimal vmgAmount = 0, prmAmount = 0, lovAmount = 0, capAmount = 0, totAmount = 0, thresholdMeasured = -1;
            decimal currentVmgAmount = 0, currentPrmAmount = 0, currentLovAmount = 0, currentCapitalAmount = 0;
            bool currentVmgAmountSpecified, currentPrmAmountSpecified, currentLovAmountSpecified, currentCapitalAmountSpecified;

            int lastRecordTotalRow = -1;
            int totalRows = pDt.Rows.Count;
            int currentRow = 0;
            while (currentRow < totalRows)
            {
                classRecord = pDt.Rows[currentRow]["CLASSRECORD"].ToString();
                switch (classRecord)
                {
                    case CLASSRECORD_FLOWS:
                    case CLASSRECORD_CAPITAL:
                        #region
                        currentVmgAmountSpecified = false;
                        currentPrmAmountSpecified = false;
                        currentLovAmountSpecified = false;
                        currentCapitalAmountSpecified = false;

                        idc = pDt.Rows[currentRow]["IDC"].ToString();
                        idcCtrval = pDt.Rows[currentRow]["IDCCTRVAL"].ToString();

                        if (classRecord == CLASSRECORD_FLOWS)
                        {
                            currentVmgAmountSpecified = (pDt.Rows[currentRow]["VMG"] != Convert.DBNull);
                            currentVmgAmount = currentVmgAmountSpecified ? Convert.ToDecimal(pDt.Rows[currentRow]["VMG"]) : 0;

                            currentPrmAmountSpecified = pDt.Rows[currentRow]["PRM"] != Convert.DBNull;
                            currentPrmAmount = currentPrmAmountSpecified ? Convert.ToDecimal(pDt.Rows[currentRow]["PRM"]) : 0;

                            currentLovAmountSpecified = pDt.Rows[currentRow]["LOV_DIF"] != Convert.DBNull;
                            currentLovAmount = currentLovAmountSpecified ? Convert.ToDecimal(pDt.Rows[currentRow]["LOV_DIF"]) : 0;
                        }
                        else
                        {
                            currentCapitalAmountSpecified = pDt.Rows[currentRow]["CAPITAL"] != Convert.DBNull;
                            currentCapitalAmount = currentCapitalAmountSpecified ? Convert.ToDecimal(pDt.Rows[currentRow]["CAPITAL"]) : 0;
                        }

                        if (String.IsNullOrEmpty(idc) || (idc == idcCtrval)
                            || (
                                 (classRecord == CLASSRECORD_FLOWS)
                                 && (currentVmgAmount == 0 && currentPrmAmount == 0 && currentLovAmount == 0))
                            || (
                                 (classRecord == CLASSRECORD_CAPITAL)
                                 && (currentCapitalAmount <= 0))
                            )
                        {
                            if (classRecord == CLASSRECORD_FLOWS)
                            {
                                if (currentVmgAmountSpecified)
                                    vmgAmount += currentVmgAmount;
                                if (currentPrmAmountSpecified)
                                    prmAmount += currentPrmAmount;
                                if (currentLovAmountSpecified)
                                    lovAmount += currentLovAmount;
                            }
                            else
                            {
#if DEBUG
                                if (thresholdReference == 0.1M)
                                {
                                    idc = idcCtrval;
                                    currentCapitalAmountSpecified = true;
                                    currentCapitalAmount = 100000;
                                    pDt.Rows[currentRow]["IDC"] = "EUR";
                                }
#endif
                                if (currentCapitalAmountSpecified)
                                {
                                    capAmount = currentCapitalAmount;
                                }
                                if ((!currentCapitalAmountSpecified) || (capAmount <= 0))
                                {
                                    isInvalidCapital = (capAmount < 0);
                                    isMissingCapital = !isInvalidCapital;

                                    pDt.Rows[currentRow].BeginEdit();
                                    pDt.Rows[currentRow]["RISKSTATUS"] = EFS.ACommon.ProcessStateTools.StatusEnum.ERROR;
                                    pDt.Rows[currentRow]["INFORMATIONSTATUS"] = Ressource.GetString(isInvalidCapital ? "Invalid_Capital" : "Missing_Capital");
                                    pDt.Rows[currentRow].EndEdit();
                                }
                            }
                        }
                        else
                        {
                            #region Quote and Countervalue
                            KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                            {
                                IdC1 = idc,
                                IdC2 = idcCtrval
                            };
                            keyAssetFXRate.SetQuoteBasis(true);
                            KeyQuote keyQuote = new KeyQuote(cs, dtFixing);
                            SQL_Quote quote = new SQL_Quote(cs, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, productBase, keyQuote, keyAssetFXRate);
                            bool isQuoteFounded = quote.IsLoaded && (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);
                            decimal exchangeRate = quote.QuoteValue;
                            FpML.Enum.QuoteBasisEnum quoteBasis = keyAssetFXRate.QuoteBasis;

#if DEBUG
                            if ((thresholdReference == 0.1M) && (!isQuoteFounded) && (idcCtrval == "EUR") && (idc == "USD" || idc == "GBP"))
                            {
                                isQuoteFounded = true;
                                exchangeRate = (idc == "USD") ? 1.2944M : 0.805877226M;
                                quoteBasis = FpML.Enum.QuoteBasisEnum.Currency1PerCurrency2;
                            }
#endif

                            if (isQuoteFounded)
                            {

                                EFS_Cash cash = null;
                                if (classRecord == CLASSRECORD_FLOWS)
                                {
                                    if (currentVmgAmountSpecified)
                                    {
                                        cash = new EFS_Cash(cs, idc, idcCtrval, currentVmgAmount, exchangeRate, quoteBasis);
                                        vmgAmount += cash.ExchangeAmountRounded;
                                    }
                                    if (currentPrmAmountSpecified)
                                    {
                                        cash = new EFS_Cash(cs, idc, idcCtrval, currentPrmAmount, exchangeRate, quoteBasis);
                                        prmAmount += cash.ExchangeAmountRounded;
                                    }
                                    if (currentLovAmountSpecified)
                                    {
                                        cash = new EFS_Cash(cs, idc, idcCtrval, currentLovAmount, exchangeRate, quoteBasis);
                                        lovAmount += cash.ExchangeAmountRounded;
                                    }
                                }
                                else
                                {
                                    if (currentCapitalAmountSpecified)
                                    {
                                        cash = new EFS_Cash(cs, idc, idcCtrval, currentCapitalAmount, exchangeRate, quoteBasis);
                                        capAmount = cash.ExchangeAmountRounded;
                                    }
                                }

                                pDt.Rows[currentRow].BeginEdit();
                                if (quoteBasis == FpML.Enum.QuoteBasisEnum.Currency1PerCurrency2)
                                    pDt.Rows[currentRow]["FXRATE"] = exchangeRate;
                                else
                                    pDt.Rows[currentRow]["FXRATE"] = (decimal)1 / exchangeRate;
                                pDt.Rows[currentRow].EndEdit();
                            }
                            else
                            {
                                if (classRecord == CLASSRECORD_FLOWS)
                                    isMissingExchangeRateOnFlows = true;
                                else if (classRecord == CLASSRECORD_CAPITAL)
                                    isMissingExchangeRateOnCapital = true;

                                pDt.Rows[currentRow].BeginEdit();
                                pDt.Rows[currentRow]["FXRATE"] = Convert.DBNull;
                                pDt.Rows[currentRow]["RISKSTATUS"] = EFS.ACommon.ProcessStateTools.StatusEnum.ERROR;
                                pDt.Rows[currentRow]["INFORMATIONSTATUS"] = Ressource.GetString("Missing_ExchangeRate");
                                pDt.Rows[currentRow].EndEdit();
                            }
                            #endregion
                        }
                        #endregion
                        break;

                    case CLASSRECORD_TOTAL:
                        #region
                        thresholdMeasured = -1;
                        totAmount = vmgAmount + prmAmount + lovAmount;

                        if (isMissingCapital || isInvalidCapital || isMissingExchangeRateOnFlows || isMissingExchangeRateOnCapital)
                        {
                            riskStatus = EFS.ACommon.ProcessStateTools.StatusEnum.ERROR;
                            information = Ressource.GetString("See_above");
                            if ((isMissingCapital || isInvalidCapital) && !(isMissingExchangeRateOnCapital || isMissingExchangeRateOnFlows))
                            {
                                information = Ressource.GetString(isInvalidCapital ? "Invalid_Capital" : "Missing_Capital");
                            }
                            else if (!(isMissingCapital || isInvalidCapital))
                            {
                                information = Ressource.GetString("Missing_ExchangeRate");
                            }
                        }
                        else
                        {
                            if (Math.Sign(totAmount) == -1)
                            {
                                thresholdMeasured = Math.Abs(totAmount) / capAmount;

                                if (Convert.ToDecimal(thresholdMeasured).CompareTo(thresholdReference) > 0)
                                {
                                    riskStatus = riskStatus = EFS.ACommon.ProcessStateTools.StatusEnum.WARNING;
                                    information = Ressource.GetString2("RiskExceeded", Convert.ToDecimal(thresholdReference).ToString("p2"));
                                }
                                else
                                {
                                    riskStatus = riskStatus = EFS.ACommon.ProcessStateTools.StatusEnum.SUCCESS;
                                    information = Ressource.GetString2("RiskCovered", Convert.ToDecimal(thresholdReference).ToString("p2"));
                                }
                            }
                            else
                            {
                                riskStatus = riskStatus = EFS.ACommon.ProcessStateTools.StatusEnum.SUCCESS;
                                information = Ressource.GetString("RiskNone");
                            }
                        }

                        pDt.Rows[currentRow].BeginEdit();
                        if (!isMissingExchangeRateOnFlows)
                        {
                            pDt.Rows[currentRow]["VMG"] = vmgAmount;
                            pDt.Rows[currentRow]["PRM"] = prmAmount;
                            pDt.Rows[currentRow]["LOV_DIF"] = lovAmount;
                            pDt.Rows[currentRow]["TOTAL"] = totAmount;
                        }
                        if (!(isMissingCapital || isInvalidCapital || isMissingExchangeRateOnCapital))
                        {
                            pDt.Rows[currentRow]["CAPITAL"] = capAmount;
                        }
                        if (thresholdMeasured >= 0)
                        {
                            pDt.Rows[currentRow]["THRESHOLD"] = Convert.ToDecimal(thresholdMeasured).ToString("p2");//Pourcentage avec 2 déc.
                        }
                        pDt.Rows[currentRow]["RISKSTATUS"] = riskStatus.ToString();
                        pDt.Rows[currentRow]["INFORMATIONSTATUS"] = information;
                        pDt.Rows[currentRow].EndEdit();

                        #region Suppression des lignes qui ne correspondent pas au Mode d'Affichage.
                        bool isShow = true;
                        switch (displayMode)
                        {
                            case DISPLAYMODE_ERROR:
                                isShow = (riskStatus == ProcessStateTools.StatusEnum.ERROR);
                                break;
                            case DISPLAYMODE_WARNING:
                                isShow = (riskStatus == ProcessStateTools.StatusEnum.WARNING);
                                break;
                            case DISPLAYMODE_NOTSUCCESS:
                                isShow = (riskStatus != ProcessStateTools.StatusEnum.SUCCESS);
                                break;
                            case DISPLAYMODE_ALL:
                            case DISPLAYMODE_ALLPLUS:
                            default:
                                isShow = true;
                                break;
                        }
                        if (!isShow)
                        {
                            for (int iRow = currentRow; iRow > lastRecordTotalRow; iRow--)
                            {
                                pDt.Rows[iRow].Delete();
                            }
                        }

                        lastRecordTotalRow = currentRow;
                        #endregion

                        #region Reset variables
                        information = null;
                        isMissingExchangeRateOnFlows = false;
                        isMissingExchangeRateOnCapital = false;
                        isMissingCapital = false;
                        isInvalidCapital = false;
                        capAmount = 0;
                        vmgAmount = 0;
                        prmAmount = 0;
                        lovAmount = 0;
                        #endregion
                        #endregion
                        break;
                }
                currentRow++;
            }

            if (!(displayMode == DISPLAYMODE_ALL || displayMode == DISPLAYMODE_ALLPLUS))
            {
                //Purge des éventuelles lignes qui ne correspondent pas au Mode d'Affichage.
                pDt.AcceptChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDt"></param>
        ///20090408 EG Gestion NTA non valorisé (null)
        private void AddSubTotalToAGEINGBALANCE(DataTable pDt)
        {
            AllowSorting = false;//Interdiction du tri par colonne
            string newKey = string.Empty;
            string lastKey = string.Empty;
            decimal sum_INIT_NTI_Amount = 0;
            decimal sum_INIT_NTA_Amount = 0;
            decimal sum_REMAIN_NTI_Amount = 0;
            decimal sum_REMAIN_NTA_Amount = 0;
            int countEntry = 0;
            DataRow dr;
            int totalRows = pDt.Rows.Count;
            int currentRow = 0;
            while (currentRow <= totalRows)
            {
                if (currentRow == totalRows)
                {
                    #region End Of Table
                    lastKey = "EOT";
                    countEntry++;
                    #endregion End Of Table
                }
                else
                {
                    #region Init key
                    newKey = string.Empty;
                    newKey += pDt.Rows[currentRow]["IDA_BENEFICIARY"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDB_BENEFICIARY"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDA_INVOICED"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["IDB_INVOICED"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["INVOICEDATE"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["INIT_NTI_CUR"].ToString() + "|";
                    newKey += pDt.Rows[currentRow]["INIT_NTA_CUR"].ToString() + "|";
                    countEntry++;
                    #endregion Init key
                }
                if ((lastKey == newKey) || (0 == currentRow))
                {
                    #region Construct total
                    lastKey = newKey;
                    sum_INIT_NTI_Amount += Convert.ToDecimal(pDt.Rows[currentRow]["INIT_NTI_AMOUNT"]);
                    //20090408 EG Gestion NTA non valorisé (null)
                    if (false == Convert.IsDBNull(pDt.Rows[currentRow]["INIT_NTA_AMOUNT"]))
                        sum_INIT_NTA_Amount += Convert.ToDecimal(pDt.Rows[currentRow]["INIT_NTA_AMOUNT"]);
                    sum_REMAIN_NTI_Amount += Convert.ToDecimal(pDt.Rows[currentRow]["REMAIN_NTI_AMOUNT"]);
                    if (false == Convert.IsDBNull(pDt.Rows[currentRow]["REMAIN_NTA_AMOUNT"]))
                        sum_REMAIN_NTA_Amount += Convert.ToDecimal(pDt.Rows[currentRow]["REMAIN_NTA_AMOUNT"]);
                    #endregion Construct total
                }
                else
                {
                    #region Add new row for total
                    dr = pDt.NewRow();
                    Referential.InitializeNewRow(ref dr);
                    dr["IDENTIFIER"] = "Total";
                    //
                    countEntry--;
                    dr["ROWVERSION"] = 0;
                    dr["ROWATTRIBUT"] = "T";

                    dr["IDA_BENEFICIARY"] = pDt.Rows[currentRow - 1]["IDA_BENEFICIARY"].ToString();
                    dr["ac_beneficiary_IDENTIFIER"] = pDt.Rows[currentRow - 1]["ac_beneficiary_IDENTIFIER"].ToString();
                    dr["IDA_INVOICED"] = pDt.Rows[currentRow - 1]["IDA_INVOICED"].ToString();
                    dr["ac_invoiced_IDENTIFIER"] = pDt.Rows[currentRow - 1]["ac_invoiced_IDENTIFIER"].ToString();
                    if (Convert.IsDBNull(pDt.Rows[currentRow - 1]["IDB_BENEFICIARY"]))
                    {
                        dr["IDB_BENEFICIARY"] = Convert.DBNull;
                        dr["bk_beneficiary_IDENTIFIER"] = Convert.DBNull;
                    }
                    else
                    {
                        dr["IDB_BENEFICIARY"] = Convert.ToInt32(pDt.Rows[currentRow - 1]["IDB_BENEFICIARY"]);
                        dr["bk_beneficiary_IDENTIFIER"] = pDt.Rows[currentRow - 1]["bk_beneficiary_IDENTIFIER"].ToString();
                    }
                    if (Convert.IsDBNull(pDt.Rows[currentRow - 1]["IDB_INVOICED"]))
                    {
                        dr["IDB_INVOICED"] = Convert.DBNull;
                        dr["bk_invoiced_IDENTIFIER"] = Convert.DBNull;
                    }
                    else
                    {
                        dr["IDB_INVOICED"] = Convert.ToInt32(pDt.Rows[currentRow - 1]["IDB_INVOICED"]);
                        dr["bk_invoiced_IDENTIFIER"] = pDt.Rows[currentRow - 1]["bk_invoiced_IDENTIFIER"].ToString();
                    }
                    dr["INVOICEDATE"] = pDt.Rows[currentRow - 1]["INVOICEDATE"].ToString();
                    dr["INIT_NTI_CUR"] = pDt.Rows[currentRow - 1]["INIT_NTI_CUR"].ToString();
                    dr["INIT_NTA_CUR"] = pDt.Rows[currentRow - 1]["INIT_NTA_CUR"].ToString();
                    dr["INIT_NTI_AMOUNT"] = sum_INIT_NTI_Amount;
                    dr["INIT_NTI_CUR"] = pDt.Rows[currentRow - 1]["INIT_NTI_CUR"].ToString();
                    dr["REMAIN_NTI_AMOUNT"] = sum_REMAIN_NTI_Amount;
                    dr["REMAIN_NTI_CUR"] = pDt.Rows[currentRow - 1]["REMAIN_NTI_CUR"].ToString();
                    dr["INIT_NTA_AMOUNT"] = sum_INIT_NTA_Amount;
                    dr["INIT_NTA_CUR"] = pDt.Rows[currentRow - 1]["INIT_NTA_CUR"].ToString();
                    dr["REMAIN_NTA_AMOUNT"] = sum_REMAIN_NTA_Amount;
                    dr["REMAIN_NTA_CUR"] = pDt.Rows[currentRow - 1]["REMAIN_NTA_CUR"].ToString();
                    //
                    pDt.Rows.InsertAt(dr, currentRow);
                    totalRows++;
                    currentRow++;
                    #endregion Add new row for total
                    //
                    if (currentRow < totalRows)
                    {
                        #region Construct total
                        lastKey = newKey;
                        countEntry = 1;
                        sum_INIT_NTI_Amount = Convert.ToDecimal(pDt.Rows[currentRow]["INIT_NTI_AMOUNT"]);
                        //20090408 EG Gestion NTA non valorisé (null)
                        if (false == Convert.IsDBNull(pDt.Rows[currentRow]["INIT_NTA_AMOUNT"]))
                            sum_INIT_NTA_Amount = Convert.ToDecimal(pDt.Rows[currentRow]["INIT_NTA_AMOUNT"]);
                        else
                            sum_INIT_NTA_Amount = 0;
                        sum_REMAIN_NTI_Amount = Convert.ToDecimal(pDt.Rows[currentRow]["REMAIN_NTI_AMOUNT"]);
                        if (false == Convert.IsDBNull(pDt.Rows[currentRow]["REMAIN_NTA_AMOUNT"]))
                            sum_REMAIN_NTA_Amount = Convert.ToDecimal(pDt.Rows[currentRow]["REMAIN_NTA_AMOUNT"]);
                        else
                            sum_REMAIN_NTA_Amount = 0;
                        #endregion Construct total
                    }
                }
                currentRow++;
            }

        }

        /// <summary>
        /// <para>Alimente le DataSource du grid à partir du jeu de données</para>
        /// <para>Trie le dataSource</para>
        /// <para>Lie les contrôles avec le DataSource</para>
        /// </summary>
        public void BindData()
        {

            if (IsDataAvailable)
            {
                SetDataSource();
                SetDataSourceOrder();
                SetCurrentPageIndex();

                isInHeader_OnItemCreated = false;
                isInBody_OnItemCreated = false;
                isInFooter_OnItemCreated = false;
                DataBind();
            }

        }

        /// <summary>
        /// Abort changes and rebind data on DataGrid
        /// </summary>
        public void RejectChanges()
        {

            if (false == IsDataSourceAvailable)
                throw new Exception("No Data is available");
            //
            DataTable dt = ((DataView)DataSource).Table;
            dt.RejectChanges();
            IsDataModified = false;
            //
            BindData();

        }

        /// <summary>
        /// Update Table in DataBase and rebind data on DataGrid
        /// Note: Display success message only in DataGridMode.GridInput (not in DataGridMode.FormInput)
        /// </summary>
        /// FI 20141021 [20350] Modify (add valeur de retour de type  Cst.ErrLevel)  
        /// FI 20150325 [20893] Modify
        /// FI 20150529 [20982] Modify
        public Cst.ErrLevel SubmitChanges(bool pShowSuccessMessage)
        {

            // FI 20150325 [20893] add qRequester and IDMenu 
            Cst.ErrLevel ret = SQLReferentialData.ApplyChangesInSQLTable(SessionTools.CS, null, Referential, DsData,
                            out int rowsaffected, out string message, out string error, IDMenu);

            if (ret == Cst.ErrLevel.SUCCESS && rowsaffected >= 1)
            {
                //purge du cache => suppression dans le cache des requêtes qui s'appuient sur la table mise à jour
                ReferentialTools.RemoveCache(DsData.Tables[0].TableName);

                // FI 20150529 [20982] call AddSessionRestrict
                ReferentialTools.AddItemSessionRestrict(Referential);
            }

            // RD 20150324 [20893] Utilisation de DsData comme paramètre de la méthode SendMQueueAfterChange
            // FI 20150325 [20893] Mise en commetaire de l'appel à Referential.SendMQueueAfterChange(Le postage éventuel de Message Queue est déjà effectué ds SQLReferentialData.ApplyChangesInSQLTable()
            //Send Eventuel de message (dépend du référentiel)
            //if (ret == Cst.ErrLevel.SUCCESS && rowsaffected > 1) //PL 20130416 Pourquoi >1 et non pas >=1 ???
            //Referential.SendMQueueAfterChange(null, DsData, IDMenu);

            if (ret == Cst.ErrLevel.SQLDEFINED)
            {
                // RD 20130523 [18644] Afficher un message plus claire
                //message = message + Cst.CrLf + Cst.CrLf + "(" + error + ")";
                message = error + Cst.CrLf + Cst.CrLf + Ressource.GetString(message);
                JavaScript.DialogImmediate((PageBase)this.Page, message, false, ProcessStateTools.StatusErrorEnum);
            }
            else if (rowsaffected <= 0)
            {
                message = Ressource.GetString("Msg_NoModification");
                //JavaScript.AlertImmediate((PageBase)this.Page, message, false);
                JavaScript.DialogImmediate((PageBase)this.Page, message, false, ProcessStateTools.StatusWarningEnum);
            }
            else if (pShowSuccessMessage)
            {
                CurrentPageIndex = PageIndexBeforeInsert;
                message = Ressource.GetString("Msg_ProcessSuccessfull") + Cst.CrLf;
                message += Ressource.GetString2("Msg_LinesProcessed", rowsaffected.ToString());
                message += Cst.CrLf + Cst.CrLf;
                message += Ressource.GetString("Msg_ContinueProcessingAsk");
                JavaScript.ConfirmImmediate((PageBase)this.Page, message, "0", "SELFRELOAD_");
            }

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMode"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="pIsLoadLSTParam"></param>
        /// <param name="pIsNewTemplate"></param>
        /// <param name="pValueforFilter"></param>
        /// FI 20200602 [25370] AfterGetTemplate
        private void AfterGetTemplate(string pCS, ReferentialModeEnum pMode, Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, Boolean pIsLoadLSTParam, Boolean pIsNewTemplate, string pValueforFilter)
        {
            if (null == pDynamicArgs)
                throw new ArgumentNullException("pDynamicArgs argument is null");

            CustomObject[] customObject = TemplateDataGridPage.CustomObjects?.customObject;
            IEnumerable<ReferentialsReferentialStringDynamicData> dynamicArgsGUI = pDynamicArgs.Values.Where(x => x.source.HasFlag(DynamicDataSourceEnum.GUI));

            // dynamicArgsGUI And CustomObject 
            IEnumerable<Pair<ReferentialsReferentialStringDynamicData, CustomObject>> dynamicArgsGUIAndCustomObject = null;
            // dynamicArgsGUI and Default
            IEnumerable<Pair<ReferentialsReferentialStringDynamicData, String>> dynamicArgsGUIAndDefault = null;
            if (dynamicArgsGUI.Count() > 0)
            {
                // dynamicArgsGUIAndCustomObject contient uniquement les dynamicArgs pouvant être stockés dans LSTPARAM  
                // Pour débrayer le stockage dans LSTPARAM il suffit d'appliquer l'attribut misc="lstParam:false" dans le customObject
                dynamicArgsGUIAndCustomObject = from item in dynamicArgsGUI
                                                join co in customObject.Where(y => y.IsControlData && y.GetMiscValue("lstParam", "true") == "true") on item.name equals co.ClientId
                                                select new Pair<ReferentialsReferentialStringDynamicData, CustomObject>
                                                {
                                                    First = item,
                                                    Second = co
                                                };
                // dynamicArgsGUIAndDefault contient uniquement les dynamicArgs pouvant être stockés dans LSTPARAM  
                // Pour débrayer le stockage dans LSTPARAM il suffit d'appliquer l'attribut misc="lstParam:false" dans le customObject
                dynamicArgsGUIAndDefault = from item in dynamicArgsGUIAndCustomObject
                                           select new Pair<ReferentialsReferentialStringDynamicData, string>
                                           {
                                               First = item.First,
                                               Second = item.Second.ContainsDefaultValue ? item.Second.DefaultValue : null
                                           };

            }

            LocalLstConsult.LoadTemplate(pCS, IdLstTemplate, IdA, (pMode == ReferentialModeEnum.XML) && pIsNewTemplate);
            if ((null != dynamicArgsGUIAndCustomObject) && dynamicArgsGUIAndCustomObject.Count() > 0)
            {
                //Alimentation de LSTPARAM si la table esr non renseignée
                LocalLstConsult.template.UpdateLstParam(pCS, dynamicArgsGUIAndDefault, true);
            }

            if (Page.IsPostBack)
            {
                if (TemplateDataGridPage.IsOptionalFilterDisabled)
                {
                    LocalLstConsult.template.ISENABLEDLSTWHERE = false;
                    LocalLstConsult.template.SetIsEnabledLstWhere2(pCS);
                }
                else if (TemplateDataGridPage.PositionFilterDisabled > -1)
                {
                    LocalLstConsult.template.SetEnabledLstWhereElement(pCS, TemplateDataGridPage.PositionFilterDisabled, false);
                }
                else if (StrFunc.IsFilled(TemplateDataGridPage.ClientIdCustomObjectChanged))
                {
                    if ((null != dynamicArgsGUIAndCustomObject) && dynamicArgsGUIAndCustomObject.Count() > 0 && 
                         null != dynamicArgsGUIAndCustomObject.Where(x=> x.First.name == TemplateDataGridPage.ClientIdCustomObjectChanged).FirstOrDefault())
                    {
                        string clientIdCustomObjectChanged = TemplateDataGridPage.ClientIdCustomObjectChanged;
                        CustomObject co = customObject.Where(x => x.IsControlData && x.ClientId == clientIdCustomObjectChanged).FirstOrDefault();
                        if (null == co)
                            throw new InvalidProgramException($"CustomObject for {TemplateDataGridPage.ClientIdCustomObjectChanged}");

                        if (co.ContainsMisc && StrFunc.IsFilled(co.GetMiscValue("linked", string.Empty)))
                        {
                            // Spheres rentre ici lorsque le contrôle est lié à plusieurs contrôles, dans ce cas tous les contôles liés sont mis à jour  
                            string[] coLinkedClientId = (from item in customObject.Where(x => x.IsControlData && x.ContainsMisc && x.GetMiscValue("linked") == co.GetMiscValue("linked"))
                                                         select item.ClientId).ToArray();

                            LocalLstConsult.template.UpdateLstParam(pCS, dynamicArgsGUIAndDefault.Where(x => ArrFunc.ExistInArray(coLinkedClientId, x.First.name)), false);
                        }
                        else
                        {
                            // FI 20200602 [25370] call InitLstParam
                            LocalLstConsult.template.UpdateLstParam(pCS, dynamicArgsGUIAndDefault.Where(x => x.First.name == clientIdCustomObjectChanged).First());
                        }
                    }
                }
            }

            if (pMode == ReferentialModeEnum.SQL)
            {
                // insertion dans LSTWhere s'il existe un filtre
                // FI 20221026 [XXXXX] Appel à ReferentialWeb.InsertFilter uniquement pIsNewTemplate == true
                if (pIsNewTemplate &&  StrFunc.IsFilled(valueForFilter)) //FI 20210406 [XXXXX] Add est if
                {
                    ReferentialWeb.InsertFilter(pCS, LocalLstConsult.IdLstConsult, LocalLstConsult.template.IDLSTTEMPLATE, LocalLstConsult.template.IDA, new Pair<string, Pair<string, string>>(pValueforFilter, null));
                }

                bool isConsultWithDynamicArgs = StrFunc.IsFilled(NVC_QueryString["DA"]);
                LocalLstConsult.LoadLstDatatable(pCS, isConsultWithDynamicArgs);
            }
            else
            {
                if (dynamicArgsGUI.Count() > 0)
                    LocalLstConsult.LoadLstParam(pCS);
            }

            if (pIsLoadLSTParam && (dynamicArgsGUI.Count() > 0))
            {
                // FI 20200602 [25370] call InitLstParam
                SetReferentialDynamicArgGUI(LocalLstConsult.dtLstParam, dynamicArgsGUI);
            }

        }


        #region OnUpdateView event
        public event EventHandler UpdateView;
        protected virtual void OnUpdateView(EventArgs e)
        {
            UpdateView?.Invoke(this, e);
        }
        #endregion OnUpdateView event

        #region OnSelectView event
        public event EventHandler SelectView;
        protected virtual void OnSelectView(EventArgs e)
        {
            SelectView?.Invoke(this, e);
        }
        #endregion  OnSelectView event

        #region  OnDblClick event
        public delegate void DblClickEventHandler(object sender, DblClickEventArgs dce);
        public event DblClickEventHandler DblClick;
        public class DblClickEventArgs : EventArgs
        {
            public DblClickEventArgs(string rowNum, string colNum, string colValue)
            {
                this.rowNum = rowNum;
                this.colNum = colNum;
                this.colValue = colValue;
            }
            public string rowNum;
            public string colNum;
            public string colValue;
        }
        #endregion  OnDblClick event

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            try
            {
                _alException = new List<SpheresException2>();
                /* FI 20200602 [25370] Mise en commentaire
                 _isApplyOptionalFilter = true;
                 _positionFilterDisabled = -1;
                */

                InitPagingType();

                IDMenu = NVC_QueryString["IDMenu"];
                IDMenuSys = NVC_QueryString["IDMenuSys"];

                //***********************************************************************
                //Rappel: 
                //- Mode 3 boutons (from XML file) : Referential, Log, Price, Process...
                //- Mode 5 boutons (from LST table): Consultation only, Consultation and Process
                //***********************************************************************
                if (IsConsultation)
                    SQLInit_TitleAndObjecName();
                else
                    XMLInit_TitleAndObjecName();

                //Default inputMode value
                #region inputMode value
                if (StrFunc.IsFilled(NVC_QueryString["InputMode"]))
                    InputMode = (Cst.DataGridMode)Enum.Parse(typeof(Cst.DataGridMode), NVC_QueryString["InputMode"]);
                else
                    InputMode = (IsConsultation ? Cst.DataGridMode.FormInput : Cst.DataGridMode.FormSelect);
                #endregion

                if (!isLocalGridSelectMode)
                    isLocalGridSelectMode = (InputMode == Cst.DataGridMode.LocalGridSelect);

                _currencyInfos = new Hashtable();
            }
            catch (Exception ex) { TrapException(ex); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                if (Referential.SQLPreSelectSpecified)
                {
                    bool isPreSelect = true;
                    if (!Page.IsPostBack)
                        isPreSelect = IsLoadDataOnStart;
                    else
                        isPreSelect = IsSelfReload;

                    //Remarque : En mode pagination ExecutePreSelect n'est effectué qu'une seule fois
                    //Si l'utilisateur demande une nouvelle page, les instructions présentes ds PreSelect ne sont pas effectuées
                    if (isPreSelect)
                        ExecutePreSelect();
                }

                InitFlagIsLoadData();

                InitDataset();

                if (IsLoadData)
                {
                    UpdateDataSourceFromRequestValues_OnLoad();
                    IsDataModified = false;
                }

                if (DataFrom__EVENTTARGET == "PAGE" && DataFrom__EVENTARGUMENT == "ADDROW")
                {
                    AddItem(new Object(), new System.EventArgs());
                }
                else if (Page.Request.Params["__EVENTARGUMENT"] == "DOUBLECLICKED_" + this.ClientID & isLocalGridSelectMode)
                {
                    string colValue = Page.Request.Params["__EVENTTARGET"];
                    if (colValue != null)
                    {
                        string[] tmp_ = colValue.Split(",".ToCharArray());
                        DblClickEventArgs DCEa = new DblClickEventArgs(tmp_[0], tmp_[1], tmp_[2]);
                        this.DblClick(this, DCEa);
                    }
                }
            }
            catch (Exception ex) { TrapException(ex); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20140926 [XXXX] Modify
        protected override void OnPreRender(EventArgs e)
        {
            try
            {
                if (IsLoadData)
                {
                    LoadData();
                    // FI 20191210 [XXXXX] Call LoadTxtSearchAutocomplete
                    LoadTxtSearchAutocomplete();
                }

                if (IsBindData)
                {
                    LoadSearch();// FI 20140926 [XXXX] apppel à LoadSearch
                    BindData();
                }

                RefreshInfosTable();

            }
            catch (Exception ex) { TrapException(ex); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                base.Render(writer);
            }
            catch (Exception ex) { TrapException(ex); }
        }

        /// EG 20151222 Refactoring
        // EG 20201204 [XXXXX] Corrections diverses - Erreur Cast sur Label dans tri sur consultation
        protected void RefreshInfosTable()
        {

            const int itemColumnNb = 3;
            string[] aSortExpressions = null;
            string[] aSortOrders = null;

            #region Read current settings for fields and orders (DESC/ASC)
            string strSortExpressions = (string)ViewState[this.ClientID + "SortingFields"];
            string strSortOrders = (string)ViewState[this.ClientID + "SortingOrders"];

            Panel divInfos = (Panel)this.Page.FindControl("divInfos");
            if (StrFunc.IsFilled(strSortExpressions) && StrFunc.IsFilled(strSortOrders))
            {
                char[] aSplitterChar = new char[] { ',' };
                aSortExpressions = strSortExpressions.Split(aSplitterChar);
                aSortOrders = strSortOrders.Split(aSplitterChar);
            }
            #endregion

            if (ArrFunc.IsFilled(aSortExpressions))
            {
                Panel dataPnl = (Panel)this.Page.FindControl("divInfosOrderByDataRow");
                if (null == dataPnl)
                {
                    #region Create SortBy row if does not exist
                    dataPnl = new Panel
                    {
                        ID = divInfos.ID + "OrderByDataRow",
                        CssClass = HtmlTools.cssInfosSort
                    };
                    dataPnl.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("SortedBy") + ":", HtmlTools.cssInfosColumnName));
                    divInfos.Controls.Add(dataPnl);
                    #endregion
                }

                for (int i = 0; i < aSortExpressions.Length; i++)
                {
                    string displayCol = LstConsult.GetLabelReferential(Referential, aSortExpressions[i].ToString());
                    displayCol = displayCol.Replace(Cst.HTMLBreakLine, Cst.Space);

                    int dataLblIndex = -1;
                    #region Check if column already exists in row
                    if (1 < dataPnl.Controls.Count)
                    {
                        dataPnl = dataPnl.Controls[1] as Panel;
                        foreach (Label lbl in dataPnl.Controls)
                        {
                            if (displayCol == lbl.Text)
                            {
                                dataLblIndex = dataPnl.Controls.IndexOf(lbl);
                                break;
                            }
                        }
                    }
                    #endregion

                    if (0 <= dataLblIndex)
                    {
                        #region Remove existing column
                        dataPnl.Controls.RemoveAt(dataLblIndex);
                        dataPnl.Controls.RemoveAt(dataLblIndex);
                        dataPnl.Controls.RemoveAt(dataLblIndex);
                        //
                        if ((dataPnl.Controls.Count - 1) > dataLblIndex)
                        {
                            dataPnl.Controls.RemoveAt(dataLblIndex);
                            dataPnl.Controls.RemoveAt(dataLblIndex);
                        }
                        else if ((dataPnl.Controls.Count - 1) >= itemColumnNb)
                        {
                            dataPnl.Controls.RemoveAt(dataLblIndex - 1);
                            dataPnl.Controls.RemoveAt(dataLblIndex - 2);
                        }
                        #endregion
                    }

                    #region Add new columns in the begining of the row
                    // EG 20210825 [XXXXX] Correction de l'affichage du tri sur un datagrid - via Header de colonne
                    if (i != 0)
                        dataPnl.Controls.Add(HtmlTools.NewLabel(",", HtmlTools.cssInfosAnd));
                    dataPnl.Controls.Add(HtmlTools.NewLabel(displayCol, HtmlTools.cssInfosColumnName));
                    dataPnl.Controls.Add(HtmlTools.NewLabel(aSortOrders[i].ToString().ToUpper(), HtmlTools.cssInfosOperator));
                    #endregion
                }
            }

            // EG 201512 
            if (divInfos.HasControls())
                ControlsTools.RemoveStyleDisplay(divInfos);
            else
                divInfos.Style.Add(HtmlTextWriterStyle.Display, "none");

            if (null != lblRowsCountHeader)
                lblRowsCountFooter.Text = lblRowsCountHeader.Text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnItemCommand(Object sender, DataGridCommandEventArgs e)
        {
            //si l'event provient du click sur consulter le doc
            if (e.CommandName == "ConsultDoc")
            {
                if (false == IsDataSourceAvailable)
                    SetDataSource();

                IsLoadData = false;

                ConsultDoc(e.Item.DataSetIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140630 [20101] Upd
        /// FI 20141021 [20350] Modify
        /// EG 20210630 [25500] Gestion de la suppression d'un item du flux RSS         
        public void OnDeleteCommand(Object sender, DataGridCommandEventArgs e)
        {

            if (false == IsDataSourceAvailable)
                SetDataSource();

            //isDataModified (-> enabled/disabled edit, sort, ... n'est utilisé qu'en mode grid edit)
            if (InputMode == Cst.DataGridMode.GridInput)
            {
                if (false == IsDataModified)
                    PageIndexBeforeInsert = this.CurrentPageIndex;
                IsDataModified = true;
            }
            else
                PageIndexBeforeInsert = this.CurrentPageIndex;

            // FI 20140630 [20101] la méthode GetDataSourceIndex n'est plus utilisée 
            //int sourceIndex = GetDataSourceIndex(e.Item);
            int sourceIndex = e.Item.DataSetIndex;
            DataRowView drv = ((DataView)DataSource)[sourceIndex];
            int rowIndex = ((DataView)DataSource).Table.Rows.IndexOf(drv.Row);

            // FI 20190718 [XXXXX] Call CheckBeforDelete
            Boolean isOk = true;
            int indexCol = Referential.IndexColSQL_DataKeyField;
            if (indexCol > -1)
                isOk = ReferentialTools.CheckBeforDelete((PageBase)this.Page, Referential.TableName, DsData.Tables[0].Rows[rowIndex][indexCol]);

            if (isOk)
            {
                DataRow savDataRow = SaveDataRow(rowIndex);

                //Delete principal              
                DsData.Tables[0].Rows[rowIndex].Delete();

                // En mode grid edit, les lignes supprimées sont visibles (avec un status particulier)
                if (InputMode == Cst.DataGridMode.GridInput)
                {
                    DataView dv = DsData.Tables[0].DefaultView;
                    dv.RowStateFilter = DataViewRowState.CurrentRows | DataViewRowState.Deleted;
                }

                //Delete EXTLID ,si existant
                if (Referential.HasMultiTable && DsData.Tables.Count > 1)
                {
                    for (int i = 1; i < DsData.Tables.Count; i++)
                    {
                        string deletedID;
                        if (Referential.IsDataKeyField_String)
                            deletedID = DataHelper.SQLString(DataKeys[e.Item.ItemIndex].ToString());
                        else
                            deletedID = this.DataKeys[e.Item.ItemIndex].ToString();

                        //20060524 PL Add: CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
                        DsData.Tables[i].CaseSensitive = true;
                        DataRow[] extlRows = DsData.Tables[i].Select(" ID =" + deletedID);
                        if (extlRows.GetLength(0) > 0)
                            extlRows[0].Delete();// [0]: une seule ligne normalement.
                    }
                }

                //en mode form grid; on enregistre immediatement les suppressions
                if (InputMode == Cst.DataGridMode.FormInput)
                {
                    Cst.ErrLevel errLevel = SubmitChanges(false);
                    if (errLevel == Cst.ErrLevel.SUCCESS)
                        SetRequestTrackBuilderItemProcess(RequestTrackProcessEnum.Remove, savDataRow);
                }
                else if (IsGridInputMode)
                    BindData();

                if (IsLocked)
                    OnCancelCommand(sender, e);

                //20091002 FI Purge du cache en cas de suppression
                if (false == IsGridInputMode)
                    ReferentialTools.RemoveCache(DsData.Tables[0].TableName);

                if (Referential.TableName.StartsWith("SYNDICATION"))
                    ReferentialTools.UpdateSyndicationFeed();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUpdateCommand(Object sender, DataGridCommandEventArgs e)
        {
            if (false == IsDataModified)
                PageIndexBeforeInsert = this.CurrentPageIndex;

            UpdateDataFromControl(e);

            IsLoadData = false;
            IsDataModified = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnCancelCommand(Object sender, DataGridCommandEventArgs e)
        {
            // Reset the edit mode for the current item
            this.EditItemIndex = -1;

            // Refresh the grid
            this.BindData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnEditCommand(Object sender, DataGridCommandEventArgs e)
        {
            // Set the current item to edit mode
            this.EditItemIndex = e.Item.ItemIndex;

            // Refresh the grid
            this.BindData();

            OnSelectView(EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectView(EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20090401 Gestion de l'attribut NoWrap spécifié sur une colonne
        /// EG 20090401 Gestion de l'attribut GridWith spécifié sur une colonne
        /// EG 20090401 Gestion accès First & Last Page
        // EG 20160211 New Pager
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        public void OnItemCreated(Object sender, DataGridItemEventArgs e)
        {
            // Get the newly created item
            ListItemType itemType = e.Item.ItemType;
            int firstCol = nbColumnAction;
            #region isGridInputMode
            if (IsGridInputMode)
            {
                for (int i = nbColumnAction; i < alColumnIdForReferencial.Count + nbColumnAction; i++)
                {
                    ReferentialsReferentialColumn rrc = Referential.Column[Convert.ToInt32(alColumnIdForReferencial[i - nbColumnAction])];
                    //On passe les colonnes DDL en mode wrap false
                    if (rrc.ExistsDDLType || rrc.ExistsRelation)
                        Columns[i].ItemStyle.Wrap = false;
                    //On force le resize de la cellule date car contient en mode edit le WCImgCalendar
                    else if (rrc.OtherGridControls != null && rrc.OtherGridControls.GetLength(0) > 0)
                    {
                        Columns[i].ItemStyle.Wrap = false;
                        if (TypeData.IsTypeDateTime(rrc.DataType.value))
                            Columns[i].ItemStyle.Width = new Unit(195);
                        else
                            Columns[i].ItemStyle.Width = new Unit(115);
                    }
                }
            }
            #endregion
            #region Set thead, tbody, tfoot (20080115 PL)
            if ((!isInHeader_OnItemCreated) && (!isInBody_OnItemCreated) && (!isInFooter_OnItemCreated))
            {
                isInHeader_OnItemCreated = true;
                isInBody_OnItemCreated = false;
                isInFooter_OnItemCreated = false;
            }

            if (isInHeader_OnItemCreated)
            {
                e.Item.TableSection = TableRowSection.TableHeader;
                if (itemType == ListItemType.Header)
                {
                    isInHeader_OnItemCreated = false;
                    isInBody_OnItemCreated = true;
                }
            }
            else if (isInFooter_OnItemCreated || (itemType == ListItemType.Footer))
            {
                isInFooter_OnItemCreated = true;
                e.Item.TableSection = TableRowSection.TableFooter;
                if (itemType == ListItemType.Pager)
                    isInFooter_OnItemCreated = false;
            }
            else
            {
                e.Item.TableSection = TableRowSection.TableBody;
            }
            #endregion

            #region PAGER (AllowPaging = true)
            // EG 20160211 New Pager
            AddPagerInfo(sender, e);
            #endregion PAGER (AllowPaging = true)

            #region PAGER on Header/Footer
            // EG 20160211 New Pager
            AddFooterInfo(sender, e);
            #endregion PAGER on Header/Footer

            #region HEADER
            if (itemType == ListItemType.Header)
            {
                string tmp;
                isInHeader_OnItemCreated = false;
                for (int i = 0; i < Columns.Count; i++)
                {
                    // Adds a tooltip with the sort expression
                    TableCell cell = e.Item.Cells[i];
                    if (StrFunc.IsFilled(Columns[i].SortExpression))
                    {
                        cell.ToolTip = Ressource.GetString("SortBy") + @": ";
                        cell.ToolTip += Ressource.GetString(Columns[i].SortExpression, true).Replace(Cst.HTMLBreakLine, string.Empty);
                    }

                    // Draw the glyph to reflect sorting
                    if (IsSortedByThisField(Columns[i].SortExpression))
                    {
                        tmp = SortExpression;
                        bool isAsc = tmp.EndsWith(" asc");
                        tmp = Ressource.GetString(isAsc ? tmp.Replace(" asc", string.Empty) : tmp.Replace(" desc", string.Empty), true);
                        cell.ToolTip = Ressource.GetString("SortedBy") + ": " + tmp + (isAsc ? " asc" : " desc");
                        Panel imgSorted = new Panel
                        {
                            CssClass = GetOrderSymbolImage()
                        };
                        imgSorted.Font.Size = FontUnit.Point(10);
                        imgSorted.Style.Add(HtmlTextWriterStyle.PaddingLeft, "5px");

                        cell.Controls.Add(imgSorted);
                    }
                    if (cell.Controls.Count > 0 && cell.Controls[0] != null)
                        ((WebControl)cell.Controls[0]).Enabled = (false == IsLocked && false == IsDataModified);
                }
            }
            #endregion

            #region HEADER
            if (itemType == ListItemType.Header)
            {
                int removedCol = 0;
                for (int i = Columns.Count - 1; i >= firstCol; i--)
                {
                    //PL 20111109 Pour tenir compte de <ColumnPositionInDataGrid>
                    //ReferentialsReferentialColumn rrc = Referential.Column[Convert.ToInt32(alColumnIdForReferencial[i - nbColumnAction])];
                    ReferentialsReferentialColumn rrc = Referential[Convert.ToInt32(alColumnIdForReferencial[i - nbColumnAction])];

                    TableCell cell = e.Item.Cells[i];
                    string header = cell.Text;
                    if (true == AllowSorting)
                        header = ((LinkButton)cell.Controls[0]).Text;

                    //PL 20120521 TEST
                    if (header.IndexOf("<brnobold/>") > 0)
                    {
                        // FI 20190318 [24568][24588] si ModeExportExcel <brnobold/> remplacé par <br/>
                        // Les en-têtes ne sont jamais en gras lors de l'export excel
                        if (IsModeExportExcel)
                        {
                            header = header.Replace("<brnobold/>", "<br/>");
                            if (AllowSorting)
                                ((LinkButton)cell.Controls[0]).Text = header;
                            else
                                cell.Text = header;
                        }
                        else
                        {
                            int pos_brnobold = header.IndexOf("<brnobold/>");
                            string text_nobold;
                            if (true == AllowSorting)
                            {
                                text_nobold = ((LinkButton)cell.Controls[0]).Text.Substring(pos_brnobold);
                                ((LinkButton)cell.Controls[0]).Text = ((LinkButton)cell.Controls[0]).Text.Substring(0, pos_brnobold);
                            }
                            else
                            {
                                text_nobold = cell.Text.Substring(pos_brnobold);
                                //cell.Text = cell.Text.Substring(0, pos_brnobold);
                                //PL 20121030
                                Label label_bold = new Label
                                {
                                    Text = cell.Text.Substring(0, pos_brnobold)
                                };
                                label_bold.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                                cell.Controls.Add(label_bold);
                            }
                            Label label_nobold = new Label
                            {
                                Text = Cst.HTMLBreakLine + text_nobold
                            };
                            label_nobold.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
                            cell.Controls.Add(label_nobold);
                        }
                    }

                    //On verifie si le header est de type : 1 header pour plusieurs cols
                    if (header.StartsWith(HEADERWITHMULTICOLUMN))
                    {
                        cell.Visible = false;
                        removedCol++;
                        if (rrc.ExistsRelation)
                            e.Item.Cells[i + 1].Visible = false;
                    }
                    else if (removedCol != 0)
                    {
                        cell.ColumnSpan = 1 + removedCol;
                        removedCol = 0;
                    }

                    #region GridWithColumn
                    if (rrc.GridWidthSpecified)
                    {
                        if (rrc.GridWidth < 0)
                        {
                            e.Item.Cells[i].Width = Unit.Percentage(Math.Abs(rrc.GridWidth));
                            this.Width = Unit.Empty;
                        }
                        else
                            e.Item.Cells[i].Width = Unit.Percentage(rrc.GridWidth);
                    }
                    #endregion GridWithColumn
                }
            }

            if ((ListItemType.Pager != itemType) && (ListItemType.Separator != itemType) && (ListItemType.Footer != itemType))
            {
                for (int i = Columns.Count - 1; i >= firstCol; i--)
                {

                    //PL 20111109 Pour tenir compte de <ColumnPositionInDataGrid>
                    //ReferentialsReferentialColumn rrc = Referential.Column[Convert.ToInt32(alColumnIdForReferencial[i - nbColumnAction])];
                    ReferentialsReferentialColumn rrc = Referential[Convert.ToInt32(alColumnIdForReferencial[i - nbColumnAction])];
                    #region NoWrapColumn
                    if (rrc.NowrapSpecified)
                        e.Item.Cells[i].Wrap = (false == rrc.Nowrap);
                    else
                        e.Item.Cells[i].Wrap = false;
                    #endregion NoWrapColumn
                }
            }
            #endregion
        }
        /// <summary>
        /// Affichages des links sur l'ensemble des pages (AllowPagin = true)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddPagerInfo(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Pager)
            {
                TableCell pager = (TableCell)e.Item.Controls[e.Item.Controls.Count - 1];

                // Enumerates all the items in the pager...
                for (int i = 0; i < pager.Controls.Count; i++)
                {
                    Control ctrl = pager.Controls[i] as Control;
                    if (ctrl is LinkButton)
                    {
                        LinkButton lnk = ctrl as LinkButton;
                        lnk.Enabled = (false == IsLocked && false == IsDataModified);
                        if (lnk.Text != lnk.CommandArgument)
                            lnk.Text = String.Format(@"<i class='fas fa-{0}'></i>", (0 == i) ? "backward" : "forward");
                    }
                    else if (ctrl is Label label)
                    {
                        label.Text = Ressource.GetString("Page") + " " + label.Text + " " + Ressource.GetString("of") + " " + this.PageCount.ToString();
                        label.Font.Bold = true;
                    }
                    else
                    {
                        ctrl.Visible = false;
                    }
                }

                Panel pnlPager = new Panel
                {
                    CssClass = "pagerGrid"
                };

                #region Affichage du liens pour accès FirstPage, LastPage et GotoPage
                bool isDisplayButtonFirstAndLast = PagerStyle.Mode != PagerMode.NumericPages || (this.PageCount > PagerStyle.PageButtonCount);
                if (isDisplayButtonFirstAndLast)
                {
                    //hyperlink FirstPage
                    LinkButton lnk = new LinkButton
                    {
                        Text = @"<i class='fas fa-fast-backward'></i>",
                        Enabled = (!IsLocked && !IsDataModified)
                    };
                    lnk.Click += new EventHandler(SetFirstPage);
                    pager.Controls.AddAt(0, lnk);
                    
                    //hyperlink LastPage
                    lnk = new LinkButton
                    {
                        Text = @"<i class='fas fa-fast-forward'></i>",
                        Enabled = (!IsLocked && !IsDataModified)
                    };
                    lnk.Click += new EventHandler(SetLastPage);
                    pnlPager.Controls.Add(lnk);
                }

                #region Ajout Affichage sur une page
                if ((1 < pager.Controls.Count) && (0 < TotalRowscount) && (TotalRowscount < 5000))
                {
                    LinkButton control = new LinkButton
                    {
                        ID = "lnkDisabledPager",
                        Text = Ressource.GetString("disablePaging"),
                        ToolTip = Ressource.GetString("disablePagingInformation"),
                        Enabled = (false == IsLocked)
                    };
                    // FI 20210805 [XXXXX] add OnDisabledPaging
                    control.Click += OnDisabledPaging;
                    // FI 20210806 [XXXXX] add Block Message
                    control.OnClientClick = "Block(" + JavaScript.HTMLBlockUIMessage(this.Page, Ressource.GetString("Msg_WaitingRefresh")) + ");";
                    pnlPager.Controls.Add(control);
                }
                #endregion Ajout Affichage sur une page

                if (isDisplayButtonFirstAndLast)
                {
                    //hyperlink GotoPage
                    LinkButton lnk = new LinkButton
                    {
                        ID = "lblGoToPage" + e.Item.TableSection.ToString(),
                        Text = Ressource.GetString("hlGoToPage"),
                        Enabled = (!IsLocked && !IsDataModified)
                    };
                    lnk.Click += new EventHandler(OnSetPageNumber);
                    pnlPager.Controls.Add(lnk);

                    //textbox for Go to page
                    TextBox txt = new TextBox
                    {
                        Text = string.Empty,
                        CssClass = EFSCssClass.DataGrid_txtSetPageNumber,
                        ID = "txtSetPageNumber" + e.Item.ItemType.ToString(),
                        Width = Unit.Pixel(30),
                        BackColor = Color.Empty,
                        Enabled = (!IsLocked && !IsDataModified)
                    };
                    pnlPager.Controls.Add(txt);

                    switch (e.Item.TableSection)
                    {
                        case TableRowSection.TableHeader:
                            txtSetPageNumberHeader = txt;
                            break;
                        case TableRowSection.TableFooter:
                            txtSetPageNumberFooter = txt;
                            break;
                    }
                }
                #endregion Affichage du liens pour accès FirstPage, LastPage et GotoPage

                pager.Controls.Add(pnlPager);


                #region Affichage du nombre de lignes
                Label lbl = new Label
                {
                    ID = "lblRowsCount" + e.Item.TableSection.ToString()
                };
                lbl.Style.Add("float", "right");
                pager.Controls.Add(lbl);
                switch (e.Item.TableSection)
                {
                    case TableRowSection.TableHeader:
                        lblRowsCountHeader = lbl;
                        break;
                    case TableRowSection.TableFooter:
                        lblRowsCountFooter = lbl;
                        break;
                }
                #endregion Affichage du nombre de lignes

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20210805 [XXXXX] Add 
        private void OnDisabledPaging(Object sender, EventArgs e)
        {
            //Ne pas rafraichir la page lorsque l'utilisateur fait afficher sur une page (sauf si pagination personnalisée)
            IsLoadData = AllowCustomPaging;
        }


        /// <summary>
        /// Affichages des informations de page (Rows number).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void AddFooterInfo(object sender, DataGridItemEventArgs e)
        {
            ShowFooter = false;
            if ((false == AllowPaging) && (e.Item.ItemType == ListItemType.Footer))
            {
                ShowFooter = true;
                Panel pnlPager = new Panel
                {
                    CssClass = "pagerGrid"
                };

                //label Rows count
                Label lbl = new Label
                {
                    ID = "lblRowsCount" + e.Item.TableSection.ToString()
                };
                pnlPager.Controls.Add(lbl);
                switch (e.Item.TableSection)
                {
                    case TableRowSection.TableHeader:
                        lblRowsCountHeader = lbl;
                        break;
                    case TableRowSection.TableFooter:
                        lblRowsCountFooter = lbl;
                        break;
                }
                TableCell c2 = new TableCell();
                c2.Controls.Add(pnlPager);
                c2.ColumnSpan = Math.Max(1, Columns.Cast<DataGridColumn>().Sum(col => Convert.ToInt32(col.Visible)));
                e.Item.Cells.Clear();
                e.Item.Cells.Add(c2);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20150120 [XXXXX] Modify
        private void OnCheckedChanged_Select(object sender, EventArgs e)
        {
            CheckBox chkSelect = (CheckBox)sender;

            if (chkSelect.ID.EndsWith("_cbSelect"))
            {
                #region Changement d'état (Check/Uncheck) d'une CheckBox d'une ligne du DataGrid --> MAJ de la ligne correspondante dans le du DataView
                DataGridItem item = (DataGridItem)chkSelect.NamingContainer;
                int sourceIndex = item.DataSetIndex;

                if (false == IsDataSourceAvailable)
                {
                    SetDataSource();
                    if (false == IsDataSourceAvailable)
                        throw new Exception("DataSource is not Available");
                }

                DataView dv = (DataView)DataSource;
                if (0 < dv.Count)
                    dv[sourceIndex]["ISSELECTED"] = BoolFunc.IsTrue(chkSelect.Checked);

                // FI 20150120 [XXXXX] Set DataSav à null puisqu'il y a ici une modification du datasource
                DataSav = null;
                #endregion
            }
            // EG 20201027 Ajout Test IsCheckUncheckAllRows (L'événement se déclenchait sur un click de page après avoir cliquer sur le contrôle de check de la page courante !!!
            else if (chkSelect.ID.EndsWith("_cbSelectAllOnAllPages") && IsCheckUncheckAllRows)
            {
                #region Changement d'état (Check/Uncheck) de la CheckBox de gauche (...OnAllPages) du header du DataGrid --> MAJ de l'ensemble des lignes dans le du DataView
                if (false == IsDataSourceAvailable)
                {
                    SetDataSource();
                    if (false == IsDataSourceAvailable)
                        throw new Exception("DataSource is not Available");
                }

                DataView dv = (DataView)DataSource;
                for (int i = 0; i < dv.Count; i++)
                    dv[i]["ISSELECTED"] = BoolFunc.IsTrue(chkSelect.Checked);

                // FI 20150120 [XXXXX] Set DataSav à null puisqu'il y a ici une modification du datasource
                DataSav = null;
                #endregion
            }
        }

        /// EG 20090408 TotalErrorStyle Sur ligne de total Balance agée (NTA non valorisé)
        /// EG 20091011 Total for invoice
        /// FI 20140926 [XXXXX] Modify
        /// FI 20170223 [22883] Modify
        /// FI 20170228 [22883] Modify
        /// FI 20171025 [23533] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        // EG 20210505 [25700] FreezeGrid implementation 
        private void OnItemDataBound(Object sender, DataGridItemEventArgs e)
        {

            bool isAlternatingItem = (e.Item.ItemType == ListItemType.AlternatingItem);
            bool isItemOrAlternatingItem = (e.Item.ItemType == ListItemType.Item) || isAlternatingItem;

            #region Binding data
            string rowAttribut = string.Empty;
            int rowVersion = 0;

            DataRowView drv = (DataRowView)e.Item.DataItem;

            // FI 20190318 [24568][24588] initialisation groupByNumber ici
            int groupByNumber = 0;
            if ((e.Item.ItemType == ListItemType.AlternatingItem) || (e.Item.ItemType == ListItemType.Item))
                groupByNumber = GetGroupNummber(drv.Row);

            ItemHeaderDataBound(e.Item);
            RowsCountDataBound(e.Item);
            ItemOrAlternatingItemDataBound(e.Item, groupByNumber);

            if (IsTemplateColumn)
            {
                #region Binding data to template column item (Note: a template column item can't be binded)
                if (isItemOrAlternatingItem)
                {
                    // FI 20200224 [XXXXX] Alimentation de context
                    ContextFormat context = GetCurrentContextFormat();

                    EFSCssClass.CssClassEnum sideCSSClass = EFSCssClass.CssClassEnum.UNKNOWN;
                    EFSCssClass.CssClassEnum last_CSSClass = EFSCssClass.CssClassEnum.UNKNOWN;
                    int last_I = -1;
                    //PL 20110913 A finaliser...

                    #region Getting ROWATTRIBUT
                    try
                    {
                        //Warning: A REVOIR: Utilisation en DUR de ROWATTRIBUT='T' et ROWVERSION=0 (voir BindData() plus haut dans ce fichier)
                        //PL DISPLAY_RUPTURE_TOTAL Code ajouté pour géré en dur une rupture avec sous-total pour les écritures comptables
                        // 20091022 EG
                        // EG 20210505 [25700] FreezeGrid implementation 
                        // EG 20210505 [25700] FreezeGrid Gestion image dans datagrid avec FontAwesome
                        if ("VW_ACCDAYBOOK" == Referential.TableName)
                        {
                            rowAttribut = drv["ROWATTRIBUT"].ToString();
                            rowVersion = Convert.ToInt32(drv["ROWVERSION"]);
                        }
                        else if ("FLOWSRISKCTRL_ALLOC" == Referential.TableName)
                        {
                            #region FLOWSRISKCTRL_ALLOC
                            if (drv.Row["CLASSRECORD"].ToString() == "3_TOT")
                            {
                                string imgTooltip = string.Empty;
                                if (drv.Row["RISKSTATUS"].ToString() == ProcessStateTools.StatusEnum.SUCCESS.ToString())
                                {
                                    e.Item.CssClass = "DataGrid_ResultSuccess";
                                    imgTooltip = "success";
                                }
                                else if (drv.Row["RISKSTATUS"].ToString() == ProcessStateTools.StatusEnum.WARNING.ToString())
                                {
                                    e.Item.CssClass = "DataGrid_ResultWarning";
                                    imgTooltip = "warning";
                                }
                                else
                                {
                                    e.Item.CssClass = "DataGrid_ResultError";
                                    imgTooltip = "error";
                                }
                                if (StrFunc.IsFilled(imgTooltip))
                                {
                                    Label lbl = new Label()
                                    {
                                        CssClass = String.Format("fa-icon fas led{0}", imgTooltip)
                                    };
                                    TableCell cell = e.Item.Cells[1];
                                    cell.Controls.RemoveAt(0);
                                    cell.Controls.Add(lbl);
                                }
                            }
                            #endregion
                        }
                    }
                    catch
                    {
                        rowAttribut = string.Empty;
                        rowVersion = 0;
                    }
                    #endregion

                    for (int i = 0; i < alColumnName.Count; i++)
                    {
                        Control ctrlFound = e.Item.FindControl(alColumnName[i].ToString());
                        if (ctrlFound != null)
                        {
                            WebControl webCtrlFound = ctrlFound as WebControl;
                            WebControl webCtrlFoundParent = ctrlFound.Parent as WebControl;

                            // FI 20171025 [23533] Call GetFormatedData 
                            object colData = drv[alColumnName[i].ToString()];
                            // EG 20171120 [23533] Upd
                            string data = SetClassForTzdbid(i, GetFormatedData(context, i, drv.Row));

                            #region Aggregate Label
                            // FI 20190318 [24568][24588] en mode export excel, il n'existe pas le label  
                            if (false == IsModeExportExcel)
                            {
                                if (groupByNumber > 0)
                                {
                                    string aggregateFunction = string.Empty;
                                    if (null != alColumnAggregate[i])
                                        aggregateFunction = alColumnAggregate[i].ToString();

                                    // FI 20170223 [22883]  Appel à FindControl s'il existe aggregateFunction
                                    if (StrFunc.IsFilled(aggregateFunction))
                                    {
                                        if (e.Item.FindControl(alColumnName[i].ToString() + ReferentialTools.SuffixAggregate) is Label ctrlAggregateLabel)
                                            SetDataToControl(ctrlAggregateLabel, Ressource.GetString(aggregateFunction, true) + ": ", string.Empty, string.Empty, true);// RD 20100309 / Automatic Compute identifier                                 
                                    }
                                }
                            }
                            #endregion

                            #region Column CellStyle
                            if (null == alColumnCellStyle[i])
                            {
                                if ((alColumnRessourceType[i].ToString() == RessourceTypeEnum.Empty.ToString())
                                    && (last_I == i - 1)
                                    && !EFSCssClass.IsUnknown(last_CSSClass))
                                {
                                    //Cas d'une colonne link/slave (cad sans ressource) --> Application du style de la colonne précédente
                                    webCtrlFound.CssClass = last_CSSClass.ToString();
                                    webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                }
                                else
                                {
                                    //PL 20150721 Newness
                                    int next_i = i + 1;
                                    if ((next_i < alColumnName.Count)
                                        && (alColumnRessourceType[next_i].ToString() == RessourceTypeEnum.Empty.ToString())
                                        && (alColumnCellStyle[next_i] != null)
                                        )
                                    {
                                        //Cas d'une colonne link/master (cad dont la suivante est sans ressource) --> Application du style de la colonne suivante
                                        ReferentialsReferentialColumnStyleBase next_columnCellStyle = (ReferentialsReferentialColumnStyleBase)alColumnCellStyle[next_i];
                                        if (next_columnCellStyle.modelSpecified)
                                        {
                                            if (next_columnCellStyle.model.Contains("side")
                                                ||
                                                ((next_columnCellStyle.model.StartsWith("quantity") || ((rowAttribut == "T") && (rowVersion == 0)))
                                                && (Referential.ExistsColumnISSIDE)))
                                            {
                                                //RAS
                                            }
                                            else
                                            {
                                                // FI 20170223 [22883] (valeur null pour pDecScaleSubstitute) 
                                                // FI 20171025 [23533] Call GetFormatedData 
                                                // EG 20171120 [23533] Upd
                                                string next_data = SetClassForTzdbid(next_i, GetFormatedData(context, next_i, drv.Row));

                                                EFSCssClass.CssClassEnum next_cssClass = ReferentialTools.GetCssClassForModel(next_columnCellStyle.model, next_data);
                                                if (false == EFSCssClass.IsUnknown(next_cssClass))
                                                {
                                                    webCtrlFound.CssClass = next_cssClass.ToString();
                                                    webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ReferentialsReferentialColumnStyleBase columnCellStyle = (ReferentialsReferentialColumnStyleBase)alColumnCellStyle[i];
                                //
                                try
                                {
                                    // RD 20110704 [17501]
                                    // Utilisation d'une nouvelle méthode ReferentialTools.GetCssClassForModel()
                                    if (columnCellStyle.modelSpecified)
                                    {
                                        // Gérer le cas particulier de la présence d'une colonne Side et Quantity sur la même ligne: 
                                        // pour avoir le même model pour les deux colonnes, c'est à dire ce lui de la colonne Side
                                        if (columnCellStyle.model.Contains("side"))
                                        {
                                            #region
                                            sideCSSClass = ReferentialTools.GetCssClassForModel(columnCellStyle.model, colData.ToString());
                                            webCtrlFound.CssClass = sideCSSClass.ToString();
                                            webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                            #endregion
                                        }
                                        else if ((columnCellStyle.model.StartsWith("quantity") || ((rowAttribut == "T") && (rowVersion == 0)))
                                            && (Referential.ExistsColumnISSIDE))
                                        {
                                            #region
                                            if (EFSCssClass.IsUnknown(sideCSSClass))
                                            {
                                                // Retrouver la colonne ISSIDE et son model
                                                ReferentialsReferentialColumnStyleBase colSQL_ISSIDECellStyle = (ReferentialsReferentialColumnStyleBase)alColumnCellStyle[Referential.IndexColSQL_ISSIDE];
                                                sideCSSClass = ReferentialTools.GetCssClassForModel(colSQL_ISSIDECellStyle.model, drv[alColumnName[Referential.IndexColSQL_ISSIDE].ToString()].ToString());
                                            }
                                            if (false == EFSCssClass.IsUnknown(sideCSSClass))
                                            {
                                                if (columnCellStyle.model.StartsWith("quantity_light"))
                                                {
                                                    if (sideCSSClass == EFSCssClass.CssClassEnum.DataGrid_Buyer)
                                                        webCtrlFound.CssClass = EFSCssClass.CssClassEnum.DataGrid_Buyer_Light.ToString();
                                                    else if (sideCSSClass == EFSCssClass.CssClassEnum.DataGrid_Seller)
                                                        webCtrlFound.CssClass = EFSCssClass.CssClassEnum.DataGrid_Seller_Light.ToString();
                                                    else
                                                        webCtrlFound.CssClass = sideCSSClass.ToString();

                                                    webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                                }
                                                else
                                                {
                                                    webCtrlFound.CssClass = sideCSSClass.ToString();
                                                    webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            EFSCssClass.CssClassEnum cssClass = ReferentialTools.GetCssClassForModel(columnCellStyle.model, colData.ToString());
                                            if (false == EFSCssClass.IsUnknown(cssClass))
                                            {
                                                webCtrlFound.CssClass = cssClass.ToString();
                                                webCtrlFoundParent.CssClass = webCtrlFound.CssClass;

                                                last_CSSClass = cssClass;
                                                last_I = i;

                                            }
                                        }
                                    }
                                    else if (columnCellStyle.typeSpecified)
                                    {
                                        string columnStyle = ReferentialTools.GetCssClassFromDataValue(columnCellStyle, colData.ToString());
                                        #region class
                                        if ("class" == columnCellStyle.type)
                                        {
                                            webCtrlFound.CssClass = columnStyle;
                                            ((WebControl)ctrlFound.Parent).CssClass = webCtrlFound.CssClass;
                                        }
                                        #endregion
                                        #region style
                                        else if ("style" == columnCellStyle.type)
                                        {
                                            webCtrlFound.Style.Value = columnStyle;
                                            ((TableCell)ctrlFound.Parent).Style.Value = webCtrlFound.Style.Value;
                                        }
                                        #endregion
                                    }
                                }
                                catch { }
                            }
                            #endregion
                            #region Column RowStyle
                            if (null != alColumnRowStyle[i])
                            {
                                string columnRowCSSStyle = string.Empty;
                                ReferentialsReferentialColumnStyleBase columnRowStyle = (ReferentialsReferentialColumnStyleBase)alColumnRowStyle[i];
                                //
                                try
                                {
                                    if (columnRowStyle.modelSpecified)
                                    {
                                        // RD 20110704 [17501]
                                        EFSCssClass.CssClassEnum cssClass = ReferentialTools.GetCssClassForModel(columnRowStyle.model, colData.ToString());
                                        if (false == EFSCssClass.IsUnknown(cssClass))
                                            columnRowCSSStyle = cssClass.ToString();
                                    }
                                    else
                                    {
                                        columnRowCSSStyle = ReferentialTools.GetCssClassFromDataValue(columnRowStyle, colData.ToString());
                                    }
                                    //
                                    if (columnRowStyle.typeSpecified && StrFunc.IsFilled(columnRowCSSStyle))
                                    {
                                        if ("class" == columnRowStyle.type)
                                            e.Item.CssClass = columnRowCSSStyle;
                                        else if ("style" == columnRowStyle.type)
                                            e.Item.Style.Value = columnRowCSSStyle;
                                    }
                                }
                                catch { }
                            }
                            #endregion
                            // RD 20110704 [17501] / Suppression des deux colonnes LSTCOLUMN.ISSIDE et LSTCOLUMN.ISQUANTITY
                            #region Column Status [Warning: ne pas déplacer en dessous de "IsResource"]
                            if (alColumnName[i].ToString().StartsWith("IDST")
                                || (alColumnName[i].ToString().IndexOf("_IDST") > 0))
                            {
                                try
                                {
                                    //FI 20120124 mise en commentaire les enums sont chargés à la connexion ou en cas de modifs
                                    //ExtendEnumsTools.LoadFpMLEnumsAndSchemes(SessionTools.CS);
                                    //
                                    /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                                    ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                                    //
                                    ExtendEnum extendEnum = null;
                                    //
                                    
                                    if (alColumnName[i].ToString().IndexOf("IDSTENVIRONMENT") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusEnvironment.ToString()];
                                    //PL 20100930 (Non testé)
                                    else if (alColumnName[i].ToString().IndexOf("IDSTBUSINESS") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusBusiness.ToString()];
                                    else if (alColumnName[i].ToString().IndexOf("IDSTACTIVATION") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusActivation.ToString()];
                                    else if (alColumnName[i].ToString().IndexOf("IDSTPRIORITY") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusPriority.ToString()];
                                    else if (alColumnName[i].ToString().IndexOf("IDSTCHECK") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusCheck.ToString()];
                                    else if (alColumnName[i].ToString().IndexOf("IDSTMATCH") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusMatch.ToString()];
                                    */

                                    ExtendEnum extendEnum = null;
                                    if (alColumnName[i].ToString().IndexOf("IDSTENVIRONMENT") >= 0)
                                        extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, StatusEnum.StatusEnvironment.ToString());
                                    else if (alColumnName[i].ToString().IndexOf("IDSTBUSINESS") >= 0)
                                        extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, StatusEnum.StatusBusiness.ToString());
                                    else if (alColumnName[i].ToString().IndexOf("IDSTACTIVATION") >= 0)
                                        extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, StatusEnum.StatusActivation.ToString());
                                    else if (alColumnName[i].ToString().IndexOf("IDSTPRIORITY") >= 0)
                                        extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, StatusEnum.StatusPriority.ToString());
                                    else if (alColumnName[i].ToString().IndexOf("IDSTCHECK") >= 0)
                                        extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, StatusEnum.StatusCheck.ToString());
                                    else if (alColumnName[i].ToString().IndexOf("IDSTMATCH") >= 0)
                                        extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, StatusEnum.StatusMatch.ToString());
                                    //
                                    if (extendEnum != null)
                                    {
                                        ExtendEnumValue extendEnumValue = extendEnum.GetExtendEnumValueByValue(colData.ToString());
                                        //
                                        if (extendEnumValue != null)
                                        {
                                            string forecolor = string.Empty;
                                            if (StrFunc.IsFilled(extendEnumValue.ForeColor))
                                                forecolor = "color:" + extendEnumValue.ForeColor + ";";
                                            string backcolor = string.Empty;
                                            if (StrFunc.IsFilled(extendEnumValue.BackColor))
                                                backcolor = (extendEnumValue.BackColor.ToLower() == "white" ? string.Empty : "background-color:" + extendEnumValue.BackColor);
                                            if (StrFunc.IsFilled(forecolor) || StrFunc.IsFilled(backcolor))
                                                webCtrlFound.Attributes.Add("style", forecolor + backcolor);
                                        }
                                    }
                                }
                                catch { }
                            }
                            #endregion



                            // RD 20110712 [17514]
                            // Code déplacé dans la méthode LoadData(), pour faire la sustitution non pas dans le DataGrid, 
                            // mais bien la faire dans le DataSet
                            #region Column IsTRIM
                            if (Convert.ToBoolean(alColumnIsTRIM[i]))
                            {
                                data = ReferentialTools.GetDataTRIM(data, out _);
                            }
                            #endregion

                            #region MultiLine
                            //20071102 PL A terminer...
                            //								if (TypeData.IsTypeText(alColumnDataType[i].ToString()))
                            //									data = data.Replace(Cst.CrLf, Cst.HTMLBreakLine);	
                            #endregion
                            #region Length in DataGrid
                            // FI 20190408 [XXXXX] En mode Export Spheres ne réduit pas la longueur
                            string fullData = null;
                            if (false == IsModeExportExcel)
                            {
                                int lengthInDataGrid = Convert.ToInt32(alColumnLengthInDataGrid[i]);
                                if (lengthInDataGrid > 0)
                                {
                                    if (data.Length > lengthInDataGrid)
                                    {
                                        data = data.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                                        data = data.Replace(Cst.Lf, Cst.HTMLBreakLine);
                                        data = data.Replace(Cst.Tab, Cst.HTMLSpace4);
                                        fullData = data;
                                    }

                                    data = ReferentialTools.FormatLongData(data, lengthInDataGrid);
                                }
                            }
                            #endregion
                            //Binding data to column control
                            // RD 20100309 / Automatic Compute identifier
                            string defaultValue = (null == alColumnDefaultValue[i] ? string.Empty : alColumnDefaultValue[i].ToString());
                            // EG 20160224 New Gestion CSS complex sur Cellule (REQUESTTYPE)
                            if ((null != alColumnCellStyle[i]) && ((ReferentialsReferentialColumnStyleBase)alColumnCellStyle[i]).complexModelSpecified)
                            {
                                ReferentialsReferentialColumnStyleBase columnCellStyle = (ReferentialsReferentialColumnStyleBase)alColumnCellStyle[i];
                                ReferentialTools.SetCssClassForComplexModel(columnCellStyle.complexModel, ctrlFound,
                                    columnCellStyle.versionSpecified ? columnCellStyle.version : string.Empty, data);
                            }
                            else
                            {
                                SetDataToControl(ctrlFound, data, defaultValue, fullData, true);
                            }
                        }
                    }
                }
                #endregion

                #region Binding data to edit item (Note: an edit item can't be binded)
                if (e.Item.ItemType == ListItemType.EditItem)
                {
                    //DataRowView drv = (DataRowView)e.Item.DataItem;
                    for (int i = 0; i < alColumnName.Count; i++)
                    {
                        Control ctrlFound = e.Item.FindControl(alColumnName[i].ToString() + ReferentialTools.SuffixEdit);
                        if (ctrlFound != null)
                        {
                            //Getting data
                            string data;
                            if (alColumnNameType[i].ToString() == RELATION)
                                data = drv[alColumnName[i - 1].ToString()].ToString();
                            else
                                data = drv[alColumnName[i].ToString()].ToString();

                            //particular formats
                            //bool
                            if (TypeData.IsTypeBool(alColumnDataType[i].ToString()))
                                data = (BoolFunc.IsTrue(data) ? "TRUE" : "FALSE");
                            //
                            //date -> without time
                            if (TypeData.IsTypeDate(alColumnDataType[i].ToString()) && !StrFunc.IsEmpty(data))
                                data = Convert.ToDateTime(data).ToString("d");
                            //
                            if (TypeData.IsTypeDec(alColumnDataType[i].ToString()) && StrFunc.IsFilled(data))
                            {




                                string decFormat = "d";
                                if (StrFunc.IsFilled(alColumnDataScale[i].ToString()))
                                    decFormat = "f" + alColumnDataScale[i].ToString();
                                data = Convert.ToDecimal(data).ToString(decFormat);
                            }

                            //set hidden if new record and is EXTLID
                            // EG 20110503 Changement Recherche Column dans Referentiel en tenant compte de ColumnPositionInDataGrid
                            //ReferentialsReferentialColumn rrc = Referential.Column[Convert.ToInt32(alColumnIdForReferencial[i])];
                            ReferentialsReferentialColumn rrc = Referential[Convert.ToInt32(alColumnIdForReferencial[i])];
                            if (rrc.IsExternal && StrFunc.IsEmpty(this.DataKeys[e.Item.ItemIndex].ToString()))
                                ctrlFound.Visible = false;

                            bool isEnabled = ((!rrc.IsForeignKeyField) || StrFunc.IsEmpty(Referential.ValueForeignKeyField));
                            if (!(StrFunc.IsEmpty(this.DataKeys[e.Item.ItemIndex].ToString())))
                            {
                                //Enabled if IsUpdatable
                                //isEnabled &= rrc.IsUpdatable.Value;
                                //20090603 PL Add IsUpdatableSpecified
                                isEnabled &= (rrc.IsUpdatableSpecified && rrc.IsUpdatable.Value);
                            }
                            //Binding data to column control
                            // RD 20100309 / Automatic Compute identifier
                            string defaultValue = (null == alColumnDefaultValue[i] ? string.Empty : alColumnDefaultValue[i].ToString());
                            SetDataToControl(ctrlFound, data, defaultValue, isEnabled);
                        }
                    }
                    //
                    // Enabled the delete item for edit line
                    if ((IsGridInputMode || IsFormInputMode) && (Referential.Remove))
                    {
                        WebControl wc = ((WebControl)e.Item.Cells[deleteItemIndex]);
                        if (wc.Controls[0] == null)
                            throw new Exception();
                        LinkButton linkButton = (LinkButton)wc.Controls[0];
                        linkButton.ToolTip = Ressource.GetString("imgRemove");
                        linkButton.CssClass = "fa-icon";
                        wc.Attributes["onclick"] = "return true;";
                    }
                }
                #endregion Binding data to edit item (Note: an edit item can't be binded)
            }
            else
            {
                // EG 20091103
                if (isItemOrAlternatingItem)
                {
                    if (("VW_AGEINGBALANCEDET" == Referential.TableName) || ("VW_INVOICEFEESDETAIL" == Referential.TableName))
                    {
                        #region Spécifique à VW_AGEINGBALANCEDET et VW_INVOICEFEESDETAIL: Totaux
                        try
                        {
                            rowAttribut = drv["ROWATTRIBUT"].ToString();
                            rowVersion = Convert.ToInt32(drv["ROWVERSION"]);
                        }
                        catch
                        {
                            rowAttribut = string.Empty;
                            rowVersion = 0;
                        }
                        e.Item.CssClass = EFSCssClass.DataGrid_ItemStyle;
                        if ((rowAttribut == "T") && (rowVersion <= 0))//T --> Total
                        {
                            foreach (TableCell cell in e.Item.Cells)
                            {
                                cell.CssClass = EFSCssClass.TotalConsult;
                                if (-3 == rowVersion)
                                    cell.CssClass = EFSCssClass.TotalConsult2;
                            }
                            if (("VW_AGEINGBALANCEDET" == Referential.TableName) &&
                                (Convert.IsDBNull(drv.Row["INIT_NTA_AMOUNT"]) || (0 == Convert.ToDecimal(drv.Row["INIT_NTA_AMOUNT"]))))
                                e.Item.CssClass = EFSCssClass.DataGrid_AlternatingTotalErrorStyle;
                            else if (("VW_INVOICEFEESDETAIL" == Referential.TableName) &&
                                (Convert.IsDBNull(drv.Row["NETACCOUNTING_AMOUNT"])) && (0 == rowVersion))
                                e.Item.CssClass = EFSCssClass.DataGrid_AlternatingTotalErrorStyle;
                            else
                                e.Item.CssClass = EFSCssClass.DataGrid_AlternatingItemStyle;
                        }
                        #endregion
                    }
                    else if (("VW_AGEINGBALANCE" == Referential.TableName) &&
                            (Convert.IsDBNull(drv.Row["INIT_NTA_AMOUNT"]) || (0 == Convert.ToDecimal(drv.Row["INIT_NTA_AMOUNT"]))))
                        e.Item.CssClass = EFSCssClass.DataGrid_TotalErrorStyle;
                    else
                    {
                        int indexERRORLINE = Referential.GetIndexColSQL("CSS_ERRORLINE");
                        if ((-1 < indexERRORLINE) && Convert.ToBoolean(drv.Row["CSS_ERRORLINE"]))
                            e.Item.CssClass = EFSCssClass.DataGrid_TotalErrorStyle;
                    }
                }
            }
            #endregion Binding data

            if (isItemOrAlternatingItem)
            {
                if (isExistColumnDataTypeBoolean || IsHyperLinkAvailable)
                    FinalizeDisplayForBoolean_Compute_Hyperlink(e.Item);
                if (isExistEditableColumn)
                    FinalizeDisplayPermanentEditableColumn(e.Item);
            }

            #region Data item
            if (isItemOrAlternatingItem)
            {
                bool isDeletedItem = false;
                #region isGridInputMode
                if (IsGridInputMode && Referential.Create)
                {
                    WebControl wc = ((WebControl)e.Item.Cells[newItemIndex]);
                    if (wc.Controls[0] == null)
                        throw new Exception("Firts Control doesn't exist");
                    string toolTip = string.Empty;
                    string status;
                    switch (((DataView)DsData.Tables[0].DefaultView)[e.Item.DataSetIndex].Row.RowState)
                    {
                        case DataRowState.Added:
                            status = "*";
                            toolTip = Ressource.GetString("StatusLineAdded");
                            break;
                        case DataRowState.Modified:
                            status = "!";
                            toolTip = Ressource.GetString("StatusLineModified");
                            break;
                        case DataRowState.Deleted:
                            status = "x";
                            toolTip = Ressource.GetString("StatusLineDeleted");
                            isDeletedItem = true;
                            break;
                        default:
                            status = " ";
                            break;
                    }
                    ((Label)wc.Controls[0]).Text = status;
                    ((Label)wc.Controls[0]).ToolTip = toolTip;
                }
                #endregion

                ModifyActionDataBound(e.Item, rowVersion, rowAttribut, isDeletedItem);

                RemoveActionDataBound(e.Item, isDeletedItem);

                DblClickDataBound(e.Item);

                ApplyItemCssClass(e.Item, isDeletedItem, groupByNumber);
            }
            #endregion Data item

            #region CheckBox selected
            // EG 20140120 [20683] Masque le check de sélection sur ligne obsolete
            if ((("MCO" == Referential.TableName) || ("VW_MCO" == Referential.TableName)) &&
                isItemOrAlternatingItem && IsCheckboxColumn)
            {
                TableCell cellSelect = e.Item.Cells[1];
                WCCheckBox2 chkSelect = (WCCheckBox2)cellSelect.Controls[0];
                string columnName = (Referential.AliasTableNameSpecified ? Referential.AliasTableName + "_" : string.Empty) + "DTOBSOLETE";
                if ((drv.DataView.Table.Columns.Contains(columnName)))
                {
                    chkSelect.Visible = Convert.IsDBNull(drv[columnName]);
                    if (false == Convert.IsDBNull(drv[columnName]))
                    {
                        chkSelect.Checked = false;
                        ((DataView)DataSource)[e.Item.DataSetIndex]["ISSELECTED"] = false;
                    }
                }
            }
            #endregion CheckBox selected
        }

        // EG 20171120 [23533] New
        /// <summary>
        /// Ajout span pour application CSS de la classe tz sur timezone
        /// </summary>
        /// <param name="pIndex"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        private string SetClassForTzdbid(int pIndex, string pData)
        {
            string ret = pData;
            // FI 20190318 [24568][24588] add false == isModeExportExcel
            if (false == IsModeExportExcel)
            {
                ReferentialsReferentialColumnDataType rrcDataType = (ReferentialsReferentialColumnDataType)alColumnDataTypeLongForm[pIndex];
                if (rrcDataType.datakindSpecified && (rrcDataType.datakind == Cst.DataKind.Timestamp))
                {
                    //Regex regExTimeZone = new Regex(@"[A-Za-z]+$", RegexOptions.IgnoreCase);
                    Regex regExTimeZone = new Regex(@"[A-Za-z_\- ]+$", RegexOptions.IgnoreCase);
                    Match match = regExTimeZone.Match(ret);
                    if (match.Success)
                        ret = ret.Replace(match.Groups[0].Value, StrFunc.AppendFormat("<span class='tz'>{0}</span>", match.Groups[0].Value));
                }
            }
            return ret;

        }
        /// <summary>
        /// Applique une classe de style à la ligne
        /// </summary>
        /// <param name="pDataGridItem"></param>
        /// <param name="isDeletedItem"></param>
        /// FI 20160308 [21782] Add Method
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        // EG 20240513 [WI916] CSS customisation : Views (Disabled data)
        private void ApplyItemCssClass(DataGridItem pDataGridItem, Boolean isDeletedItem, int pGroupByNumber)
        {
            if (false == ((pDataGridItem.ItemType == ListItemType.AlternatingItem) || (pDataGridItem.ItemType == ListItemType.Item)))
                throw new ArgumentException("DataGrid item is not an Item or an AlternatingItem");

            // FI 20190318 [24568][24588] Initilisation ici de CssClass pour les lignes de regroupements 
            // => toutes les initilisations des css sur DataGridItem sont effectuées dans la même méthode)
            // Remarque en isModeExportExcel seuls les lignes de regroupements conservent un CssClass
            if (0 < pGroupByNumber)
            {
                if (pGroupByNumber == 1)
                    pDataGridItem.CssClass = EFSCssClass.DataGrid_GroupStyle1;
                else if (pGroupByNumber == 2)
                    pDataGridItem.CssClass = EFSCssClass.DataGrid_GroupStyle2;
                else
                    pDataGridItem.CssClass = EFSCssClass.DataGrid_GroupStyle;
            }

            if (false == IsModeExportExcel) //pas de CSS si export Excel. 
            {
                if (String.IsNullOrEmpty(pDataGridItem.CssClass))
                {
                    if (isDeletedItem)
                    {
                        pDataGridItem.CssClass = "DataGrid_DeletedItemStyle";
                    }
                    else
                    {
                        Boolean isAlternatingItem = (pDataGridItem.ItemType == ListItemType.AlternatingItem);
                        bool isDisabledItem = IsItemDateDisabled(pDataGridItem);
                        if (isDisabledItem)
                        {
                            pDataGridItem.CssClass = StrFunc.AppendFormat("DataGrid_Disabled{0}ItemStyle", isAlternatingItem ? "Alternating" : string.Empty);
                        }
                        else if (StrFunc.IsEmpty(pDataGridItem.CssClass))
                        {
                            if (StrFunc.IsEmpty(pDataGridItem.Style.Value))
                                pDataGridItem.CssClass = isAlternatingItem ? AlternatingItemStyle.CssClass : ItemStyle.CssClass;
                            else
                                pDataGridItem.CssClass = "DataGrid_ItemStyle";
                        }

                        // Implementing styles on mouse mouve over the rows
                        if (ItemStyle.CssClass == pDataGridItem.CssClass)
                            pDataGridItem.Attributes.Add("onmouseover", "this.className='" + SelectedItemStyle.CssClass + "';");
                        else
                            pDataGridItem.Attributes.Add("onmouseover", "this.className='" + pDataGridItem.CssClass + " " + SelectedItemStyle.CssClass + "';");

                        pDataGridItem.Attributes.Add("onmouseout", "this.className='" + pDataGridItem.CssClass + "';");
                    }
                }
                else
                {
                    pDataGridItem.CssClass = pDataGridItem.CssClass.Replace("DataGrid_DeletedItemStyle", string.Empty).Replace("DataGrid_DisabledItemStyle", string.Empty);
                    if (isDeletedItem)
                        pDataGridItem.CssClass += " DataGrid_DeletedItemStyle";
                    else
                    {
                        if (IsItemDateDisabled(pDataGridItem))
                            pDataGridItem.CssClass += " DataGrid_DisabledItemStyle";

                        if (ItemStyle.CssClass == pDataGridItem.CssClass)
                            pDataGridItem.Attributes.Add("onmouseover", $"this.className='{SelectedItemStyle.CssClass}';");
                        else
                            pDataGridItem.Attributes.Add("onmouseover", $"this.className='{pDataGridItem.CssClass} {SelectedItemStyle.CssClass}';");
                        pDataGridItem.Attributes.Add("onmouseout", $"this.className='{pDataGridItem.CssClass.Replace($"{SelectedItemStyle.CssClass}",string.Empty)}';");
                    }
                }
            }
        }


        /// <summary>
        ///  Retourne true si la ligne est  disabled (Prise en considération des colonnes DTENABLED, DTDISABLED) 
        /// </summary>
        /// <returns></returns>
        /// FI 20160310 [XXXXX] Add method
        private Boolean IsItemDateDisabled(DataGridItem pDataGridItem)
        {
            Boolean isDisabledItem = false;

            if (Referential.ExistsColumnsDateValidity)
            {
                string dtEnabled = null, dtDisabled = null;
                //PL 20121031 TRIM 18211
                //dtEnabled = GetCellValue(e.Item, Referential.IndexColSQL_DTENABLED + nbColumnAction);
                //dtDisabled = GetCellValue(e.Item, Referential.IndexColSQL_DTDISABLED + nbColumnAction);
                //test dtEnabled = GetCellValueByColumnName(e.Item, "DTENABLED"); 
                //test dtDisabled = GetCellValueByColumnName(e.Item, "DTDISABLED");
                //------------------------------------------------------------------------------
                for (int i_tmp = Columns.Count - 1; i_tmp >= nbColumnAction; i_tmp--)//Tip: Les colonnes DTENABLED et DTDISABLED sont proches de la fin
                {
                    ReferentialsReferentialColumn rrc_tmp = Referential[Convert.ToInt32(alColumnIdForReferencial[i_tmp - nbColumnAction])];
                    if (rrc_tmp.ColumnName == "DTENABLED")
                    {
                        dtEnabled = GetCellValue(pDataGridItem, i_tmp);
                        break;//Tip: La colonne DTENABLED est tjs avant la colonne DTDISABLED
                    }
                    else if (rrc_tmp.ColumnName == "DTDISABLED")
                    {
                        dtDisabled = GetCellValue(pDataGridItem, i_tmp);
                    }
                }
                //------------------------------------------------------------------------------


                //PL 20121031 Use DateTime.TryParse
                if (DateTime.TryParse(dtEnabled, out DateTime dtTmp))
                {
                    if (dtTmp > DateTime.Today)
                    {
                        isDisabledItem = true;
                    }
                    else if (DateTime.TryParse(dtDisabled, out dtTmp))
                    {
                        if (dtTmp < DateTime.Today)
                            isDisabledItem = true;
                    }
                }
            }

            return isDisabledItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void ItemHeaderDataBound(DataGridItem Item)
        {
            if (Item.ItemType == ListItemType.Header && IsCheckboxColumn)
            {
                // La colonne "Select" est en deuxième position des colonnes dites "ColumnAction" ( voir AddColumnAction() )
                TableCell cellSelectAll = Item.Cells[1];

                #region SelectAllOnAllPages - Firstcheckbox
                CheckBox cbSelectAllOnAllPages = (CheckBox)cellSelectAll.Controls[0];

                cbSelectAllOnAllPages.Visible = (TotalRowscount > 0);
                if (TotalRowscount > 0)
                {
                    int firstRowIndex = 0;
                    int lastRowIndex = TotalRowscount - 1;

                    bool isNotSelectedRowExist = false;

                    for (int i = firstRowIndex; i <= lastRowIndex; i++)
                    {
                        isNotSelectedRowExist = BoolFunc.IsFalse(DsData.Tables[0].Rows[i]["ISSELECTED"]);
                        if (isNotSelectedRowExist)
                            break;
                    }

                    cbSelectAllOnAllPages.Checked = !isNotSelectedRowExist;
                }
                #endregion SelectAll

                #region SelectAllOnCurrentPage - Second checkbox
                CheckBox cbSelectAllOnCurrentPage = (CheckBox)cellSelectAll.Controls[cellSelectAll.Controls.Count - 1];

                cbSelectAllOnCurrentPage.Visible = (TotalRowscount > 0);
                if (TotalRowscount > 0)
                {
                    int firstRowIndex = this.PageSize * this.CurrentPageIndex;
                    int lastRowIndex = (TotalRowscount > this.PageSize * (CurrentPageIndex + 1) ? this.PageSize * (CurrentPageIndex + 1) - 1 : TotalRowscount - 1);

                    bool isNotSelectedRowExist = false;

                    for (int i = firstRowIndex; i <= lastRowIndex; i++)
                    {
                        isNotSelectedRowExist = BoolFunc.IsFalse(DsData.Tables[0].Rows[i]["ISSELECTED"]);
                        if (isNotSelectedRowExist)
                            break;
                    }

                    cbSelectAllOnCurrentPage.Checked = !isNotSelectedRowExist;
                }
                #endregion SelectAllOnPage
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void RowsCountDataBound(DataGridItem Item)
        {

            Label lblRowsCount = (Label)Item.FindControl("lblRowsCount" + Item.TableSection.ToString());

            if (Item.ItemType == ListItemType.Header)
                lblRowsCount = lblRowsCountHeader;
            else if (Item.ItemType == ListItemType.Footer)
                lblRowsCount = lblRowsCountFooter;

            if (null != lblRowsCount)
            {
                int lastRows = PageSize * CurrentPageIndex;
                int currentDisplayedRows = (IsLastPage ? TotalRowscount - lastRows : this.PageSize);
                if (TotalRowscount == 0)
                    lblRowsCount.Text = string.Empty;
                else if (TotalRowscount == 1 || PageSize == 1)
                    lblRowsCount.Text = "<b>" + Ressource.GetString("lblRowsCount_Row") + " " +
                        Convert.ToString(lastRows + 1) + "</b> " + Ressource.GetString("of") + " " + TotalRowscount.ToString();
                else
                    lblRowsCount.Text = "<b>" + Ressource.GetString("lblRowsCount_Rows") + " " +
                        Convert.ToString(lastRows + 1) + " " + Ressource.GetString("lblRowsCount_To") + " " +
                        Convert.ToString(IsLastPage ? TotalRowscount : (lastRows + this.PageSize)) + "</b> (" +
                        Convert.ToString(currentDisplayedRows) + " " + Ressource.GetString("of") + " " + TotalRowscount.ToString() + ")";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// FI 20171025 [23533] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210505 [25700] FreezeGrid implementation 
        // EG 20210505 [25700] FreezeGrid Gestion image dans datagrid avec FontAwesome
        private void ItemOrAlternatingItemDataBound(DataGridItem Item, int pGroupByNumber)
        {
            if ((Item.ItemType == ListItemType.AlternatingItem) || (Item.ItemType == ListItemType.Item))
            {
                DataRowView drv = (DataRowView)Item.DataItem;
                //
                #region Column RowNumber
                try
                {
                    // En supposant que la colonne "RowNumber" est en première position des colonnes dites "ColumnAction" ( voir AddColumnAction())
                    TableCell cellRowNumber = Item.Cells[0];
                    int rowNumber = this.PageSize * this.CurrentPageIndex + Item.ItemIndex + 1;
                    cellRowNumber.Text = Convert.ToString(rowNumber);
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unexpected error for column RowNumber", ex);
                }
                #endregion Column RowNumber
                //
                #region Column Loupe
                try
                {
                    if (_isLoupeColumn) //&& (0 < groupByNumber))
                    {
                        TableCell cellLoupe = null;
                        if (IsCheckboxColumn)
                            cellLoupe = Item.Cells[2];
                        else
                            cellLoupe = Item.Cells[1];

                        LinkButton c1 = (LinkButton)cellLoupe.Controls[0];
                        if (0 < pGroupByNumber)
                        {
                            c1.CssClass = "fa-icon";
                            c1.Text = @"<i class='fas fa-project-diagram'></i>";
                            c1.ToolTip = string.Empty;
                        }
                        else
                        {
                            c1.CssClass = "fa-icon";
                            c1.Text = @"<i class='fas fa-search'></i>";
                            c1.ToolTip = Ressource.GetString(Referential.Modify ? "imgEdit" : "imgView");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unexpected error for column magnifying glass", ex);
                }
                #endregion Column Loupe

                #region Column Column AttachedDOC
                try
                {
                    // EG 20240207 [WI825] Logs: Harmonization data of consultation (VW_ATTACHEDDOC_TRACKER_L)
                    if (IsInputMode && this.Referential.IsTableAttachedDoc)
                    {
                        TableCell cellAttachedDoc  = Item.Cells[attachDocItemIndex];
                        LinkButton c1 = (LinkButton)cellAttachedDoc.Controls[0];
                        c1.CssClass = "fa-icon";
                        c1.Text = @"<i class='fas fa-paperclip'></i>";
                        c1.ToolTip = Ressource.GetString("imgConsultDoc");
                    }
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unexpected error for column magnifying glass", ex);
                }
                #endregion Column AttachedDOC
                // EG 20121122 Add Background on RowState (Image via CSS)
                // EG 20210505 [25700] FreezeGrid implementation 
                // EG 20210505 [25700] FreezeGrid Gestion image dans datagrid avec FontAwesome
                #region Column RowState
                try
                {
                    string rowState = string.Empty;
                    if (drv.DataView.Table.Columns.Contains("ROWSTATE"))
                        rowState = (null != drv["ROWSTATE"] ? drv["ROWSTATE"].ToString() : string.Empty);
                    //
                    if (StrFunc.IsFilled(rowState) && (0 < nbColumnAction))
                    {
                        // En supposant que la colonne "ROWSTATE" est en dérnière position des colonnes dites "ColumnAction" ( voir AddColumnAction())
                        TableCell cellRowState = Item.Cells[nbColumnAction - 1];
                        string[] arrRowState = rowState.Split("+".ToCharArray());
                        for (int i = 0; i < arrRowState.Length; i++)
                        {
                            if (StrFunc.IsFilled(arrRowState[i]))
                            {
                                string rowStateCtrlAttributes = string.Empty;
                                Control rowStateCtrl = null;
                                //
                                bool isWebdings = arrRowState[i].StartsWith("webdings=");
                                bool isWingdings = arrRowState[i].StartsWith("wingdings=");
                                bool isImage = arrRowState[i].StartsWith("img=");
                                bool isLabelBackground = arrRowState[i].StartsWith("lblbkg=");
                                bool isButtonBackground = arrRowState[i].StartsWith("btnbkg=");
                                // EG 20160224 New Gestion class= dans SQLRowState
                                bool isClass = arrRowState[i].StartsWith("class=");
                                bool isListAw = arrRowState[i].StartsWith("lstaw=");

                                if (isWebdings)
                                {
                                    Label lblRowState = new Label();
                                    lblRowState.Font.Name = "webdings";
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("webdings=".ToCharArray());
                                }
                                else if (isWingdings)
                                {
                                    Label lblRowState = new Label();
                                    lblRowState.Font.Name = "wingdings";
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("wingdings=".ToCharArray());
                                }
                                else if (isImage)
                                {
                                    WCToolTipPanel imgRowState = new WCToolTipPanel();
                                    rowStateCtrl = imgRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("img=".ToCharArray());
                                }
                                else if (isClass)
                                {
                                    WCToolTipPanel pnlRowState = new WCToolTipPanel();
                                    rowStateCtrl = pnlRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("class=".ToCharArray());
                                }
                                else if (isListAw)
                                {
                                    WCTooltipLabel lblRowState = new WCTooltipLabel
                                    {
                                        CssClass = "fa-icon fas"
                                    };
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("lstaw=".ToCharArray());
                                    cellRowState.CssClass = "lstaw";
                                }
                                else if (isLabelBackground)
                                {
                                    WCTooltipLabel lblRowState = new WCTooltipLabel();
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("lblbkg=".ToCharArray());
                                }
                                else if (isButtonBackground)
                                {
                                    WCToolTipButton lblRowState = new WCToolTipButton();
                                    rowStateCtrl = lblRowState;
                                    rowStateCtrlAttributes = arrRowState[i].TrimStart("btnbkg=".ToCharArray());
                                }

                                if (null != rowStateCtrl)
                                {
                                    rowStateCtrlAttributes = rowStateCtrlAttributes.Trim(@"\''".ToCharArray());
                                    string[] arrRowStateCtrlAttributes = rowStateCtrlAttributes.Split(";".ToCharArray());
                                    for (int j = 0; j < arrRowStateCtrlAttributes.Length; j++)
                                    {
                                        if (StrFunc.IsFilled(arrRowStateCtrlAttributes[j]))
                                        {
                                            string[] arrAttributeKeyValue = arrRowStateCtrlAttributes[j].Split(":".ToCharArray());
                                            string attributeKey = arrAttributeKeyValue[0].Trim(Cst.Cr.ToCharArray()).Trim(@"\".ToCharArray());

                                            if (StrFunc.IsFilled(attributeKey))
                                            {
                                                string attributeValue = string.Empty;
                                                //PL 20150520 
                                                if (arrAttributeKeyValue.Length > 1)
                                                {
                                                    attributeValue = arrAttributeKeyValue[1].Trim(Cst.Cr.ToCharArray()).Trim(@"\".ToCharArray());
                                                }

                                                if (isWebdings || isWingdings)
                                                {
                                                    Label lblRowState = (Label)rowStateCtrl;
                                                    if ("code" == attributeKey)
                                                        lblRowState.Text = attributeValue;
                                                    else if ("title" == attributeKey)
                                                        lblRowState.ToolTip = attributeValue;
                                                    else
                                                        lblRowState.Style.Value = lblRowState.Style.Value + ";" + arrRowStateCtrlAttributes[j];
                                                }
                                                else if (isImage)
                                                {
                                                    WCToolTipPanel imgRowState = rowStateCtrl as WCToolTipPanel;
                                                    if ("src" == attributeKey)
                                                    {
                                                        if (StrFunc.IsFilled(imgRowState.CssClass))
                                                            imgRowState.CssClass += " ";
                                                        imgRowState.CssClass += CSS.SetCssClass(attributeValue);
                                                        imgRowState.Style.Add(HtmlTextWriterStyle.MarginLeft, "2px");
                                                        imgRowState.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                                                    }
                                                    else if ("class" == attributeKey)
                                                    {
                                                        if (StrFunc.IsFilled(imgRowState.CssClass))
                                                            imgRowState.CssClass += " ";
                                                        imgRowState.CssClass += attributeValue;
                                                        imgRowState.Style.Add(HtmlTextWriterStyle.MarginLeft, "2px");
                                                        imgRowState.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                                                    }
                                                    else
                                                    {
                                                        if ("title" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        imgRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                                // EG 20160224 New Gestion class= dans SQLRowState
                                                else if (isClass)
                                                {
                                                    WCToolTipPanel pnlRowState = rowStateCtrl as WCToolTipPanel;
                                                    if ("name" == attributeKey)
                                                    {
                                                        if (StrFunc.IsFilled(pnlRowState.CssClass))
                                                            pnlRowState.CssClass += " ";
                                                        pnlRowState.CssClass += attributeValue;
                                                    }
                                                    else
                                                    {
                                                        if ("title" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        pnlRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                                else if (isListAw)
                                                {
                                                    if (rowStateCtrl is WCTooltipLabel lblRowState)
                                                    {
                                                        if ("name" == attributeKey)
                                                        {
                                                            if (StrFunc.IsFilled(lblRowState.CssClass))
                                                                lblRowState.CssClass += " ";
                                                            lblRowState.CssClass += attributeValue;

                                                        }
                                                        else
                                                        {
                                                            if ("title" == attributeKey)
                                                                attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                            else if ("alt" == attributeKey)
                                                                attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                            lblRowState.Attributes.Add(attributeKey, attributeValue);
                                                        }
                                                    }
                                                }
                                                else if (isLabelBackground)
                                                {
                                                    if (rowStateCtrl is WCTooltipLabel lblRowState)
                                                    {
                                                        if ("title" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            attributeValue = ACommon.Ressource.GetString(attributeValue, true);
                                                        lblRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                                else if (isButtonBackground)
                                                {
                                                    if (rowStateCtrl is WCToolTipButton btnRowState)
                                                    {
                                                        if ("title" == attributeKey)
                                                            btnRowState.Pty.TooltipTitle = Ressource.GetString(attributeValue, true);
                                                        else if ("alt" == attributeKey)
                                                            btnRowState.Pty.TooltipContent = Ressource.GetString(attributeValue, true);
                                                        btnRowState.Attributes.Add(attributeKey, attributeValue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    cellRowState.Controls.Add(rowStateCtrl);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unexpected error for column Raw State", ex);
                }

                #endregion Column RowState
                //
                #region Apply RowStyle
                try
                {
                    string rowStyle = string.Empty;
                    if (drv.DataView.Table.Columns.Contains("ROWSTYLE"))
                        rowStyle = (null != drv["ROWSTYLE"] ? drv["ROWSTYLE"].ToString() : string.Empty);

                    if (StrFunc.IsFilled(rowStyle))
                    {
                        if ("class" == Referential.SQLRowStyle.type)
                            Item.CssClass = rowStyle;
                        else if ("style" == Referential.SQLRowStyle.type)
                            Item.Style.Value = rowStyle;
                    }
                }
                catch (Exception ex)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Unexpected error when applying style", ex);
                }
                #endregion  Apply RowStyle
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pItem"></param>
        /// FI 20160406 [XXXXX] Modify
        /// FI 20180216 [XXXXX] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210412 [XXXXX] Construction du lien hyperlink si value = true
        private void FinalizeDisplayForBoolean_Compute_Hyperlink(DataGridItem pItem)
        {
            for (int i = 0; i < alColumnName.Count; i++)
            {
                int index = i + nbColumnAction;
                TableCell cell = pItem.Cells[index];

                #region Boolean data
                if (TypeData.IsTypeBool(alColumnDataType[i].ToString()))
                {
                    Panel pnl = new Panel
                    {
                        CssClass = "fa-icon"
                    };
                    LiteralControl lit = new LiteralControl();
                    pnl.Controls.Add(lit);
                    if (cell.HasControls() && (null != cell.Controls[0]) && cell.Controls[0].GetType().Equals(typeof(Label)))
                    {
                        // FI 20160406 [XXXXX] Usage de BoolFunc.IsTrue pour au final avoir les seules valeurs possibles true,false
                        string value = ((Label)cell.Controls[0]).Text.ToLower();
                        lit.Text = String.Format("<i class='fas fa-{0}'></i>", BoolFunc.IsTrue(value)?"check-circle":"times");
                        cell.Controls.RemoveAt(0);
                    }
                    else
                    {
                        // FI 20160406 [XXXXX] Usage de BoolFunc.IsTrue pour au final avoir les seules valeurs possibles true,false
                        string value = cell.Text.ToLower();
                        lit.Text = String.Format("<i class='fas fa-{0}'></i>", BoolFunc.IsTrue(value) ? "check-circle" : "times");
                    }
                    cell.Controls.Add(pnl);
                    pItem.HorizontalAlign = HorizontalAlign.Center;
                }
                #endregion Boolean data

                //20100310 RD Automatic Compute identifier
                #region Automatic Compute identifier
                //string defaultValue = (null == alColumnDefaultValue[i] ? string.Empty : alColumnDefaultValue[i].ToString());
                //if (StrFunc.IsEmpty(cell.Text) && (StrFunc.IsFilled(defaultValue)) && (Cst.AUTOMATIC_COMPUTE.ToString() == defaultValue))
                //    cell.Text = Ressource.GetString(defaultValue);
                //else if (Cst.AUTOMATIC_COMPUTE.ToString() == cell.Text)
                //    cell.Text = Ressource.GetString(cell.Text);
                //PL 20100914 Refactoring for tuning
                ////((PageBase)Page).AddAuditTimeStep("TIME-PL: AUTOMATIC_COMPUTE");

                //20110204 PL Use GetCellValue()
                //if ((alColumnDefaultValue[i] != null) && (alColumnDefaultValue[i].ToString() == Cst.AUTOMATIC_COMPUTE.ToString()) && (StrFunc.IsEmpty(cell.Text)))
                if ((alColumnDefaultValue[i] != null) && (alColumnDefaultValue[i].ToString() == Cst.AUTOMATIC_COMPUTE.ToString()) && (StrFunc.IsEmpty(GetCellValue(pItem, index))))
                {
                    cell.Text = resAUTOMATIC_COMPUTE;
                }
                //else if (cell.Text == Cst.AUTOMATIC_COMPUTE.ToString())
                else if (GetCellValue(pItem, index) == Cst.AUTOMATIC_COMPUTE.ToString())
                {
                    cell.Text = resAUTOMATIC_COMPUTE;
                }
                #endregion

                #region HyperLink
                else if (IsHyperLinkAvailable)
                {
                    try
                    {
                        if (RELATION == alColumnNameType[i].ToString())
                        {
                            ReferentialsReferentialColumn rrc = Referential[Convert.ToInt32(alColumnIdForReferencial[i])];
                            if (ArrFunc.IsFilled(rrc.Relation) &&
                                ArrFunc.IsFilled(rrc.Relation[0].ColumnRelation))
                            {
                                //FI 20120111 
                                string id = GetCellValue(pItem, index - 1);
                                if (StrFunc.IsFilled(id) && (id != Cst.HTMLSpace) && (id != "0"))
                                {
                                    id = FormatToInvariant(id, rrc.Relation[0].ColumnRelation[0].DataType);
                                    string identifier = GetCellValue(pItem, index);

                                    // FI 20180216 [XXXXX] call GetHyperLinkColumnNameFromRelation
                                    string columnName = ReferentialTools.GetHyperLinkColumnNameFromRelation(Referential, rrc, ((DataRowView)(pItem.DataItem)).Row);

                                    if (StrFunc.IsFilled(columnName))
                                    {
                                        HyperLink hl = GetHyperLinkByColumn(pItem, columnName, id, identifier);
                                        if ((null != hl) && (hl.Text != Cst.HTMLSpace))
                                        {
                                            //20090914 PL Add RemoveAt()
                                            if (IsTemplateColumn)
                                                pItem.Cells[index].Controls.RemoveAt(0);
                                            pItem.Cells[index].Controls.Add(hl);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ReferentialsReferentialColumn rrc = Referential[Convert.ToInt32(alColumnIdForReferencial[i])];
                            if (rrc.ExistsHyperLinkDocument)
                            {
                                #region rrc.ExistsHyperLinkDocument
                                WCToolTipHyperlink hlkFile;
                                string text = string.Empty;
                                //20090910 PL Use GetCellValue()
                                text = GetCellValue(pItem, index);
                                //
                                if (StrFunc.IsFilled(text) && (text != Cst.HTMLSpace))
                                {
                                    ReferentialsReferentialColumn rrcDataKeyField = Referential.Column[Referential.IndexDataKeyField];
                                    //
                                    string[] _keyColumns = new string[1];
                                    _keyColumns[0] = rrcDataKeyField.ColumnName;
                                    string[] _keyValues = new string[1];
                                    //20081212 PL/CC Bug lorsqu'un "Tri" est utilisé sur le Datagrid via un click sur une entête de colonne
                                    //_keyValues[0] = Data.Tables[0].Rows[e.Item.DataSetIndex][rrcDataKeyField.ColumnName].ToString();
                                    if (IsTemplateColumn)
                                    {
                                        //20090306 PL BUG Index_DataKeyField remplacé par IndexColSQL_DataKeyField
                                        //_keyValues[0] = ((Label)e.Item.Cells[Referential.IndexDataKeyField + nbColumnAction].Controls[0]).Text;
                                        _keyValues[0] = ((Label)pItem.Cells[Referential.IndexColSQL_DataKeyField + nbColumnAction].Controls[0]).Text;
                                    }
                                    else
                                    {
                                        //20090306 PL BUG Index_DataKeyField remplacé par IndexColSQL_DataKeyField
                                        //_keyValues[0] = e.Item.Cells[Referential.IndexDataKeyField + nbColumnAction].Text;
                                        _keyValues[0] = pItem.Cells[Referential.IndexColSQL_DataKeyField + nbColumnAction].Text;
                                    }
                                    //FI 20120608 [17867] désormais _keyValues[0] n'est plus formatée selon la culture de la station
                                    //Les type ROWVERSION ne sont jamais formaté, il ne faut pas le transformer en int
                                    if (false == Referential.IsROWVERSIONDataKeyField)
                                        _keyValues[0] = FormatToInvariant(_keyValues[0], rrcDataKeyField.DataType.value);

                                    string[] _keyDatatypes = new string[1];
                                    _keyDatatypes[0] = rrcDataKeyField.DataType.value;

                                    // 20081020 RD - Ticket 16331
                                    hlkFile = GetHyperLinkForDocument(
                                        StrFunc.IsFilled(rrc.IsHyperLink.linktable) ? rrc.IsHyperLink.linktable : this.Referential.TableName,
                                        StrFunc.IsFilled(rrc.IsHyperLink.data) ? rrc.IsHyperLink.data : rrc.ColumnName,
                                        StrFunc.IsFilled(rrc.IsHyperLink.type) ? rrc.IsHyperLink.type : rrc.ColumnName,
                                        StrFunc.IsFilled(rrc.IsHyperLink.name) ? rrc.IsHyperLink.name : rrc.ColumnName,
                                        _keyColumns, _keyValues, _keyDatatypes);
                                    //
                                    if (hlkFile != null)
                                    {
                                        //FI 20110111 mise encommentaire
                                        //car cela duplique le contenu de la cellule (a la place de txt\xml on se retrouve avec  txt\xmltxt\xml)
                                        // e.Item.Cells[index].Controls.Add(new LiteralControl(text + Cst.HTMLSpace));
                                        pItem.Cells[index].Controls.Add(hlkFile);
                                    }
                                }
                                #endregion rrc.ExistsHyperLinkDocument
                            }
                            else if (rrc.ExistsHyperLinkColumn && rrc.IsHyperLink.Value)
                            {
                                #region ExistsHyperLinkColumn
                                //Type de Link (voir méthode IsExistsHyperLinkColumn(string pColumnName))
                                string colName = rrc.IsHyperLink.type;
                                //Colonne qui détient l'id ds le jeu de résultat
                                //FI 20120926 [] usage of data attribute for HyperLink column
                                //string colNameId = rrc.IsHyperLink.name;
                                string colNameId = rrc.IsHyperLink.data;
                                if (ReferentialTools.IsHyperLinkColumn(colName))
                                {
                                    int indexColId = Referential.GetIndexColSQL(colNameId);
                                    if (-1 == indexColId)
                                        throw new Exception(StrFunc.AppendFormat("Column [name:{0}] doesn't exist", colNameId));
                                    //
                                    string identifier = GetCellValue(pItem, index);
                                    if (StrFunc.IsFilled(identifier) && (identifier != Cst.HTMLSpace))
                                    {
                                        DataRow row = ((DataRowView)pItem.DataItem).Row;
                                        string idValue = row[indexColId].ToString();
                                        if (StrFunc.IsFilled(idValue) && (idValue != Cst.HTMLSpace) && (idValue != "0"))
                                        {
                                            //
                                            HyperLink hlkColumn = GetHyperLinkByColumn(pItem, colName, idValue, identifier);
                                            if (hlkColumn != null)
                                            {
                                                if (IsTemplateColumn)
                                                    pItem.Cells[index].Controls.RemoveAt(0);
                                                pItem.Cells[index].Controls.Add(hlkColumn);
                                            }
                                        }
                                    }
                                }
                                #endregion ExistsHyperLinkColumn
                            }
                            else if (rrc.ExistsHyperLinkExternal)
                            {
                                #region ExistsHyperLinkExternal
                                string identifier = GetCellValue(pItem, index);
                                string colNameId = rrc.IsHyperLink.data;
                                int indexColId = Referential.GetIndexColSQL(colNameId);
                                if (-1 == indexColId)
                                    throw new Exception(StrFunc.AppendFormat("Column [name:{0}] doesn't exist", colNameId));

                                if (StrFunc.IsFilled(identifier) && (identifier != Cst.HTMLSpace))
                                {
                                    DataRow row = ((DataRowView)pItem.DataItem).Row;
                                    string idValue = row[indexColId].ToString();
                                    if (StrFunc.IsFilled(idValue) && (idValue != Cst.HTMLSpace) && (idValue != "0"))
                                    {
                                        HyperLink hl = new HyperLink
                                        {
                                            Target = Cst.HyperLinkTargetEnum._blank.ToString(),
                                            CssClass = "linkDatagrid",
                                            Text = identifier
                                        };
                                        if (IsTemplateColumn)
                                            pItem.Cells[index].Controls.RemoveAt(0);

                                        hl.NavigateUrl = idValue;
                                        pItem.Cells[index].Controls.Add(hl);
                                    }
                                }
                                #endregion ExistsHyperLinkExternal
                            }
                            else if (rrc.ColumnName == "URL")
                            {
                                // FI 20180219 [XXXXX] call FinalyzeURLColumn
                                FinalizeURLColumn(pItem,  index);
                            }
                            else if (rrc.ColumnName.StartsWith("ENVIRONMENT") && rrc.IsVirtualColumn)
                            {
                                // FI 20180219 [XXXXX] call FinalyzeEnvironmentColumn
                                FinalyzeEnvironmentColumn(pItem, cell);
                            }
                        }
                    }
                    catch { }
                }
                #endregion  HyperLink
            }
        }

        /// <summary>
        ///  Display Editable control for editable column
        /// </summary>
        /// <param name="e"></param>
        // EG 20210505 [25700] FreezeGrid implementation 
        private void FinalizeDisplayPermanentEditableColumn(DataGridItem item)
        {

            // Construction du suffixe à l'aide de la valeur de la cellule de la colonne DATAKEYFIELD
            if (Referential.ExistsColumnDataKeyField)
            {
                ReferentialsReferentialColumn rrcKeyField = Referential.Column[Referential.IndexDataKeyField];
                DataRowView drv = (DataRowView)item.DataItem;
                string suffix = Convert.ToString(drv.Row[rrcKeyField.IndexColSQL]);
                for (int i = 0; i < alColumnName.Count; i++)
                {
                    if (Convert.ToBoolean((alColumnIsEditable[i])))
                    {
                        // EG 20110503 Changement Recherche Column dans Referentiel en tenant compte de ColumnPositionInDataGrid
                        //ReferentialsReferentialColumn rrc = Referential.Column[Convert.ToInt32(alColumnIdForReferencial[i])];
                        int index = i + nbColumnAction;
                        TableCell cell = item.Cells[index];
                        cell.CssClass = "editable";
                        TemplateColumn tc = (TemplateColumn)this.Columns[index];
                        EditItemTemplate_Referencial editTemplate = (EditItemTemplate_Referencial)tc.EditItemTemplate;
                        Control ctrl = editTemplate.CloneControlMain;
                        //ctrl.ID += "_" + e.Item.ItemIndex;
                        //ctrl.ID += "_" + e.Item.DataSetIndex;
                        ctrl.ID += "_" + suffix;
                        ((WCTextBox)ctrl).Enabled = true;
                        item.Cells[index].Controls.Add(ctrl);
                        item.Cells[index].Controls.RemoveAt(0);

                        SetValidatorAndDefaultValueOnEditableColumn(item, (WCTextBox)ctrl);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrencyInfos"></param>
        /// <param name="pUnitType"></param>
        /// <param name="pValue"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        /// FI 20200224 [XXXXX] Method Static
        private static string FormattingValorisation(Hashtable pCurrencyInfos, string pUnitType, string pValue, string pCurrency)
        {
            string returnValue = pValue;
            if (StrFunc.IsFilled(pUnitType))
            {
                UnitTypeEnum unitType = (UnitTypeEnum)System.Enum.Parse(typeof(UnitTypeEnum), pUnitType);
                decimal decval = DecFunc.DecValue(pValue, Thread.CurrentThread.CurrentCulture);
                switch (unitType)
                {
                    case UnitTypeEnum.Qty:
                        // EG 20170127 Qty Long To Decimal
                        returnValue = StrFunc.FmtDecimalToCurrentCulture(decval, 4);
                        break;
                    case UnitTypeEnum.Currency:
                        CurrencyCashInfo currencyInfo;
                        if (false == pCurrencyInfos.ContainsKey(pCurrency))
                        {
                            currencyInfo = new CurrencyCashInfo(SessionTools.CS, pCurrency);
                            if (null != currencyInfo)
                                pCurrencyInfos.Add(pCurrency, currencyInfo);
                        }
                        if (pCurrencyInfos.ContainsKey(pCurrency))
                        {
                            currencyInfo = (CurrencyCashInfo)pCurrencyInfos[pCurrency];
                            returnValue = StrFunc.FmtDecimalToCurrentCulture(decval, currencyInfo.RoundPrec);
                        }
                        break;
                    case UnitTypeEnum.Percentage:
                    case UnitTypeEnum.Rate:
                        returnValue = StrFunc.FmtDecimalToCurrentCulture(decval);
                        break;
                    case UnitTypeEnum.None:
                        break;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Update all the datasource fields connected with the editable columns, using the form request parameters
        /// </summary>
        /// <remarks>
        /// NORMALLY this form of reverse bind can be executed directly by the ASP control values, in this case the NOT .NET COMPLIANT
        /// manipulation made in FinalizeDisplayPermanentEditableColumn corrupts the ASP control state of the datagrid and just the form
        /// request values are congruent... 
        /// </remarks>
        public void UpdateDataSourceFromRequestValues_OnLoad()
        {
            if (Referential.ExistsColumnDataKeyField)
            {
                ReferentialsReferentialColumn rrcKeyField = Referential.Column[Referential.IndexDataKeyField];
                for (int i = 0; i < alColumnName.Count; i++)
                {
                    bool bColumnEditable = Convert.ToBoolean(alColumnIsEditable[i]);
                    if (bColumnEditable)
                    {
                        foreach (DataGridItem dgItem in Items)
                        {
                            int dsIndex = dgItem.DataSetIndex;
                            // EG 20151102 [21465] Add test on DsData.Tables[0].Rows.Count
                            if ((dsIndex > DsData.Tables[0].Rows.Count) || (0 == DsData.Tables[0].Rows.Count))
                                break;
                            if (null != DsData)
                            {

                                DataRow dr = DsData.Tables[0].Rows[dsIndex];
                                string suffix = Convert.ToString(dr[rrcKeyField.IndexColSQL]);
                                Control referentialControl = dgItem.FindControl(alColumnName[i].ToString());

                                string currentRequestValue = Page.Request.Form[String.Format("{0}{1}_{2}",
                                                             referentialControl.UniqueID,
                                                             ReferentialTools.SuffixEdit, suffix)];
                                if (StrFunc.IsFilled(currentRequestValue))
                                {
                                    dr.BeginEdit();
                                    dr[alColumnName[i].ToString()] = currentRequestValue;
                                    dr.EndEdit();
                                    dr.AcceptChanges();
                                }
                            }
                        }
                    }
                }
            }
        }

        // UNDONE MF 20110912 UNCLEARINGQTY en dur, il faut le rendre générique après avoir corrigé les méthodes  
        //  SetValidatorAndDefaultValueOnEditableColumn et FinalizeDisplayPermanentEditableColumn
        /// <summary>
        /// Update all the values bound to the current  UNCLEARINGQTY datagrid element
        /// </summary>
        /// <param name="pDgItem">datagrid item identifying the built row</param>
        /// <param name="pControl">ASP UNCLEARINGQTY custom control that replaces the  template datagrid  standard control</param>
        /// <param name="pColumnName"></param>
        /// <param name="pValue">default value</param>
        /// FI 20140409 [XXXXX] Utilisation de DataSource
        private void UpdateEditableColumnValues_OnItemDataBound(DataGridItem pDgItem, WCTextBox pControl, string pColumnName, string pValue)
        {
            int dsIndex = pDgItem.DataSetIndex;

            //FI 20140409 [XXXXX] Utilisation de DataSource parce qu'un grid peut être trié au niveau de l'entête 
            //DataRow dr = DsData.Tables[0].Rows[dsIndex];
            DataRow dr = ((DataView)(this.DataSource))[dsIndex].Row;

            string currentDataSetValue = Convert.ToString(dr[pColumnName]);
            string currentRequestValue = Page.Request.Form[pControl.UniqueID];

            //if (currentRequestValue != null)
            if (!String.IsNullOrEmpty(currentRequestValue))
            {
                pControl.Text = currentRequestValue;
                dr.BeginEdit();
                dr[pColumnName] = pControl.Text;
                dr.EndEdit();
                dr.AcceptChanges();
            }
            else if (!String.IsNullOrEmpty(currentDataSetValue) && currentDataSetValue != "0")
            {
                pControl.Text = currentDataSetValue;
            }
            else
            {
                pControl.Text = pValue;
                dr.BeginEdit();
                dr[pColumnName] = pControl.Text;
                dr.EndEdit();
                dr.AcceptChanges();
            }
        }

        /// <summary>
        /// Set Value (.TEXT) on Editable Control for editable column
        /// </summary>
        /// <param name="pDgItem"></param>
        /// <param name="pIndex"></param>
        /// <param name="pControl"></param>
        // EG 20141230 [20587]
        // EG 20150920 [21314] Int (int32) to Long (Int64) 
        private void SetValidatorAndDefaultValueOnEditableColumn(DataGridItem pDgItem, WCTextBox pControl)
        {
            string msgValidator = string.Empty;
            long availableQty = 0;
            string regExValidator = string.Empty;
            Validator validator;
            WebControl wcValidator;
            string subRegex;
            long maxValue;
            switch (Referential.TableName)
            {
                case "CLEARINGBULK":
                    long qtyBuy = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "QTY_BUY"), TypeData.TypeDataEnum.@int.ToString()));
                    long qtySell = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "QTY_SELL"), TypeData.TypeDataEnum.@int.ToString()));
                    maxValue = Math.Min(qtyBuy, qtySell);
                    //
                    UpdateEditableColumnValues_OnItemDataBound(pDgItem, pControl, "CLEARINGQTY", maxValue.ToString());
                    //
                    msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", maxValue.ToString());
                    //Quantité comprise entre 1 et " + maxValue.ToString();
                    //FI 20130205 [] valeur minimum à 1 comme l'indique par ailleurs Msg_PosKeepingInputQty
                    //CC/FI 20130305 Passage de la valeur minimum de 1 à 0  cf Ticket 18476 
                    subRegex = GetRegExForQuantity(maxValue);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, maxValue.ToString());
                    validator = new Validator(msgValidator, true, regExValidator, false);
                    //validator = new Validator(msgValidator, false, false, ValidationDataType.Integer, "0", maxValue.ToString());
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                case "TRANSFERBULK":
                    // EG 20141230 [20587]
                    long initialQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "INITIALQTY"), TypeData.TypeDataEnum.@int.ToString()));
                    availableQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "AVAILABLEQTY"), TypeData.TypeDataEnum.@int.ToString()));
                    maxValue = Math.Min(initialQty, availableQty);
                    UpdateEditableColumnValues_OnItemDataBound(pDgItem, pControl, "CLEARINGQTY", maxValue.ToString());

                    if (initialQty == availableQty)
                        msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", maxValue.ToString());
                    else if (1 == availableQty)
                        msgValidator = Ressource.GetString2("Msg_PosKeepingInputTransferQty", maxValue.ToString());
                    else
                        msgValidator = Ressource.GetString2("Msg_PosKeepingInputTransferQty2", maxValue.ToString());

                    //string regExValidator = String.Format("^([1-{0}]|{1})$", availableQty.ToString(), initialQty.ToString());

                    // EG 20150907 [21314] New
                    subRegex = GetRegExForQuantity(availableQty);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, initialQty.ToString());
                    validator = new Validator(msgValidator, true, regExValidator, false);
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                case "CLEARINGSPEC":
                    // EG 20150920 [21314] Int (int32) to Long (Int64) 
                    availableQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "AVAILABLEQTY"), TypeData.TypeDataEnum.@int.ToString()));
                    long sourceAvailableQty = 0;
                    if (Referential.dynamicArgsSpecified)
                        sourceAvailableQty = Convert.ToInt64(Referential.dynamicArgs["AVAILABLEQTY"].value);
                    maxValue = Math.Min(sourceAvailableQty, availableQty);
                    //
                    UpdateEditableColumnValues_OnItemDataBound(pDgItem, pControl, "CLOSABLEQTY", "0");
                    //
                    msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", maxValue.ToString());
                    //FI 20130205 [] valeur minimum à 1 comme l'indique par ailleurs Msg_PosKeepingInputQty
                    //CC/FI 20130305 Passage de la valeur minimum de 1 à 0  cf Ticket 18476 
                    // EG 20150907 [21314] New
                    subRegex = GetRegExForQuantity(availableQty, true);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, maxValue.ToString());
                    validator = new Validator(msgValidator, true, regExValidator, false);
                    //validator = new Validator(msgValidator, false, false, ValidationDataType.Double, "0", maxValue.ToString());
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                case "UNCLEARING":
                    // EG 20131121 [19222] Conversion au format invariant
                    long clearingQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "QTY"), TypeData.TypeDataEnum.@int.ToString()));
                    UpdateEditableColumnValues_OnItemDataBound(pDgItem, pControl, "UNCLEARINGQTY", clearingQty.ToString());
                    msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", clearingQty.ToString());
                    // EG 20150907 [21314] New
                    subRegex = GetRegExForQuantity(clearingQty);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, clearingQty.ToString());
                    validator = new Validator(msgValidator, true, regExValidator, false);
                    //validator = new Validator(msgValidator, false, false, ValidationDataType.Double, "0", clearingQty);
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                default:
                    // EG 20151019 [21465] New
                    if (Referential.TableName.Contains("ASSEXEBULK") || Referential.TableName.Contains("ABNBULK"))
                    {
                        if (Referential.dynamicArgsSpecified && (null != Referential.dynamicArgs["DENOPTIONACTIONTYPE"]))
                        {
                            long posQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "POSQTY"), TypeData.TypeDataEnum.@int.ToString()));
                            long denqty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pDgItem, "DENQTY"), TypeData.TypeDataEnum.@int.ToString()));
                            int nbDen = Convert.ToInt32(FormatToInvariant(GetCellValueByColumnName(pDgItem, "NBIDPR"), TypeData.TypeDataEnum.@int.ToString()));

                            Cst.DenOptionActionType denOptionActionType = (Cst.DenOptionActionType)ReflectionTools.EnumParse(new Cst.DenOptionActionType(), Referential.dynamicArgs["DENOPTIONACTIONTYPE"].value);
                            switch (denOptionActionType)
                            {
                                case Cst.DenOptionActionType.newRemaining: // Application sur la quantité restante en position
                                    pControl.Enabled = false;
                                    availableQty = posQty;
                                    if (1 == nbDen)
                                        availableQty += denqty;
                                    regExValidator = String.Format("^(?:{0})$", availableQty.ToString());
                                    pControl.CssClass = "txtCaptureNumericConsult";
                                    break;
                                case Cst.DenOptionActionType.@new: // Application sur la quantité saisie
                                    pControl.Enabled = true;
                                    pControl.CssClass = "txtCaptureNumeric";
                                    if (0 == posQty)
                                    {
                                        availableQty = Convert.ToInt32(FormatToInvariant(GetCellValueByColumnName(pDgItem, "AVAILABLEQTY"), TypeData.TypeDataEnum.@int.ToString()));
                                    }
                                    else
                                    {
                                        availableQty = posQty;
                                        if (1 == nbDen)
                                            availableQty += denqty;
                                    }
                                    subRegex = GetRegExForQuantity(availableQty, false);
                                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, availableQty.ToString());
                                    msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", availableQty.ToString());
                                    break;
                                case Cst.DenOptionActionType.remove: // Annulation des dénouements du jour
                                    pControl.Enabled = false;
                                    availableQty = 0;
                                    regExValidator = String.Format("^(?:{0})$", availableQty.ToString());
                                    pControl.CssClass = "txtCaptureNumericConsult";
                                    break;

                            }
                            validator = new Validator(msgValidator, true, regExValidator, false);
                            wcValidator = validator.CreateControl(pControl.ID);
                            pControl.Controls.Add(new LiteralControl("<br/>"));
                            pControl.Controls.Add(wcValidator);
                            UpdateEditableColumnValues_OnItemDataBound(pDgItem, pControl, "CLEARINGQTY", availableQty.ToString());
                        }
                    }
                    break;
            }

        }
        #region DecompQuantity
        // EG 20141230 [20587]
        // EG 20150920 [21314] Int (int32) to Long (Int64) 
        private long[] DecompQuantity(long pQty)
        {
            long qty = pQty;
            int length = qty.ToString().Length;
            long[] result = new long[length];
            length--;
            for (int i = length; i >= 0; i--)
            {
                long power = Convert.ToInt64(Math.Pow(Convert.ToDouble(10), Convert.ToDouble(i)));
                result[length - i] = qty / power;
                if (0 == result[length - i])
                    result[length - i] = qty;
                else
                    qty -= result[length - i] * power;
            }
            return result;
        }
        #endregion DecompQuantity
        #region GetRegExForQuantity
        // EG 20150907 [21314] New
        // EG 20151019 [21495] Add pIsZeroAccepted
        private string GetRegExForQuantity(long pQuantity)
        {
            return GetRegExForQuantity(pQuantity, false);
        }
        // EG 20151019 [21495] Add pIsZeroAccepted
        private string GetRegExForQuantity(long pQuantity, bool pIsZeroAccepted)
        {
            long[] result = DecompQuantity(pQuantity);
            int length = result.Length;

            string subRegEx;
            switch (length)
            {
                case 1:
                    subRegEx = String.Format("[{1}-{0}]", result[0], pIsZeroAccepted ? "0" : "1");
                    break;
                default:
                    subRegEx = String.Format("[{0}-9]", pIsZeroAccepted ? "0" : "1");
                    //subRegEx = "[1-9]";

                    for (int i = 1; i < length - 1; i++)
                    {
                        subRegEx += String.Format("|[1-9]{0}", @"\d(" + i.ToString() + ")");
                    }

                    if (result[0] > 1)
                        subRegEx += String.Format(@"|[1-{0}]{1}", result[0] - 1, @"\d(" + (length - 1).ToString() + ")");

                    for (int i = 1; i < length - 1; i++)
                    {
                        if (result[i] > 0) // 1
                        {
                            subRegEx += String.Format(@"|{0}", result[0]);
                            for (int j = 1; j < i; j++)
                            {
                                subRegEx += String.Format(@"[0-{0}]", result[j]);
                            }
                            subRegEx += String.Format(@"[0-{0}]{1}", result[i] - 1, @"\d(" + (length - i - 1).ToString() + ")");
                        }
                    }

                    string endValue = string.Empty;
                    for (int i = 0; i < length - 1; i++)
                        endValue += result[i].ToString();
                    subRegEx += String.Format("|{0}[0-{1}]", endValue, result[length - 1]);

                    subRegEx = subRegEx.Replace("(", "{").Replace(")", "}");
                    break;
            }
            return subRegEx;
        }
        #endregion GetRegExForQuantity

        /// <summary>
        /// Modify action: Enabled, disabled the edit item
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pRowVersion"></param>
        /// <param name="pRowAttribut"></param>
        /// <param name="pIsDeleteItem"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void ModifyActionDataBound(DataGridItem pItem, int pRowVersion, string pRowAttribut, bool pIsDeleteItem)
        {

            //20090129 PL Afin d'afficher la loupe sur les LOGs, mise en comment du test
            //if ((isGridInputMode || isFormInputMode) && Referential.Modify)
            if ((IsGridInputMode && Referential.Modify) || ((IsFormInputMode || IsFormViewerMode) && _isLoupeColumn))
            {
                WebControl wc = ((WebControl)pItem.Cells[editItemIndex]);
                LinkButton linkButton;
                if (null != wc.Controls[0])
                {
                    linkButton = (LinkButton)wc.Controls[0];
                    //if datagrid is locked (a row is already editing) or this row has been deleted, disable the edit item
                    if (IsGridInputMode && (IsLocked || pIsDeleteItem))
                    {
                        wc.Attributes["onclick"] = "return false;";
                        linkButton.CssClass = "fa-icon";
                        linkButton.Enabled = false;
                        linkButton.ToolTip = Ressource.GetString("imgEditUnavailable");
                    }
                    else if (IsFormInputMode || IsFormViewerMode)
                    {
                        //20090910 PL Use GetCellValue()
                        wc.Attributes["onclick"] = GetURLForUpdate(pItem) + ";return false;";
                    }
                    else
                    {
                        wc.Attributes["onclick"] = "return true;";
                        linkButton.CssClass = "fa-icon";
                        linkButton.ToolTip = Ressource.GetString(Referential.Modify ? "imgEdit" : "imgView");
                    }
                    // EG 20091110
                    if ((pRowAttribut == "T") && (pRowVersion <= 0))//T --> Total
                    {
                        if (("VW_INVOICEFEESDETAIL" == Referential.TableName) && (-1 == pRowVersion))
                        {
                            // on garde le lien vers TRADE (mais en TRADEADMIN)
                            wc.Attributes["onclick"].Replace("TradeCapture", "TradeAdminCapture");
                        }
                        else
                            wc.Controls.RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// Remove action: Enabled, disabled the delete item
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pIsDeleteItem"></param>
        private void RemoveActionDataBound(DataGridItem pItem, bool pIsDeleteItem)
        {

            if (nbColumnAction > 0)
            {
                // EG 20151215 Add Test Referential.consultationMode
                if ((IsGridInputMode || IsFormInputMode) && (Referential.Remove && (Referential.consultationMode != Cst.ConsultationMode.ReadOnly)))
                {
                    WebControl wc = ((WebControl)pItem.Cells[deleteItemIndex]);
                    LinkButton linkButton;
                    if (wc.Controls[0] != null)
                        linkButton = (LinkButton)wc.Controls[0];
                    else
                        throw new Exception();

                    string js;
                    //if datagrid is unlocked (a row is not being editing) and data is not deleted, enable the delete item
                    if (!IsLocked && !pIsDeleteItem)
                    {
                        //Creating delete message for each row with row-informations
                        bool RemoveAuthorized = true;
                        string msgAlert = string.Empty;
                        js = Ressource.GetString("Msg_Remove") + Cst.CrLf;
                        ReferentialsReferentialColumn rrc;
                        //
                        if (Referential.ExistsColumnROWATTRIBUT)
                        {
                            //si la cellule ne contient pas directement la donnée, on va la chercher dans son control container (cas pour les TemplateColumn)
                            //20090910 PL Use GetCellValue()
                            string rowAttributValue = GetCellValue(pItem, Referential.IndexColSQL_ROWATTRIBUT + nbColumnAction);
                            switch (rowAttributValue)
                            {
                                case Cst.RowAttribut_System:
                                    RemoveAuthorized = false;
                                    msgAlert = "Msg_DataSystem";
                                    break;
                                case Cst.RowAttribut_Protected:
                                    RemoveAuthorized = false;
                                    msgAlert = "Msg_DataProtected";
                                    break;
                            }
                        }
                        if (RemoveAuthorized)
                        {
                            for (int i = 0; i < Referential.Column.GetLength(0); i++)
                            {
                                rrc = Referential.Column[i];
                                int indexAddForRelation = 0;
                                if (false == TypeData.IsTypeBool(rrc.DataType.value))
                                {
                                    if (
                                        ((rrc.IsKeyFieldSpecified && rrc.IsKeyField) || (rrc.IsMandatorySpecified && rrc.IsMandatory))
                                        &&
                                        (rrc.IsHideInDataGridSpecified && !rrc.IsHideInDataGrid)
                                        &&
                                        (!rrc.IsExternal)
                                        )
                                    {
                                        string desc;
                                        string ressource;
                                        if (rrc.ExistsRelation)
                                        {
                                            ressource = rrc.Relation[0].ColumnSelect[0].Ressource;
                                            indexAddForRelation++;
                                        }
                                        else
                                            ressource = rrc.Ressource;

                                        if (StrFunc.IsEmpty(ressource))
                                            desc = @" ";
                                        else
                                        {
                                            desc = Cst.CrLf;
                                            desc += Ressource.GetMulti(ressource, 1);
                                            desc += ": ";
                                        }
                                        js += desc;
                                        //Si la cellule ne contient pas directement la donnée, on va la chercher dans son control container (cas pour les TemplateColumn)
                                        //20090910 PL Use GetCellValue()
                                        js += GetCellValue(pItem, rrc.IndexColSQL + nbColumnAction + indexAddForRelation);
                                    }
                                }
                            }
                        }
                        if (RemoveAuthorized)
                            js = "return confirm(" + JavaScript.JSString(js) + ");";
                        else
                            js = "alert('" + Ressource.GetStringForJS(msgAlert) + "');return false;";

                        linkButton.CssClass = "fa-icon";
                        linkButton.ToolTip = Ressource.GetString("imgRemove");
                    }
                    else
                    {
                        //if datagrid is locked (a row is editing) or data have been modified, disable the delete item    
                        js = "return false;";
                        linkButton.CssClass = "fa-icon";
                        linkButton.Enabled = false;
                        linkButton.ToolTip = Ressource.GetString("imgRemoveUnavailable");
                    }
                    wc.Attributes["onclick"] = js;
                }
            }
        }

        /// <summary>
        /// Double-click event, if is enable, add it on item
        /// </summary>
        /// <param name="pItem"></param>
        private void DblClickDataBound(DataGridItem pItem)
        {
            if (IsRowDblClickAvailable)
            {
                //if enabled for opening detailled form, creating open-script as url
                if (IsFormInputMode || IsFormViewerMode || IsFormSelectMode)
                {
                    // EG 20121029 Add Referential.consultationMode parameter
                    pItem.Attributes.Add("ondblclick", GetURLForUpdate(pItem));
                }
                //if enabled for selecting value, creating returnvalue-script as url
                else if (IsGridSelectMode)
                {
                    #region isGridSelectMode
                    string keyField = string.Empty;
                    if (indexColSQL_KeyField != -1)
                    {
                        //20090910 PL Use GetCellValue()
                        //keyField = e.Item.Cells[indexColSQL_KeyField].Text;
                        // 20091030 RD / 
                        // Prendre en compte d'eventuelles colonnes dites de type "Action" ( Exemple: Numéro de ligne, Loupe, etc...)
                        // Les colonnes de type "Action" ne sont pas incluses dans Referential.Column
                        // Par contre elles le sont pour l'affichage dans le DataGrid
                        //
                        int indexColSQL_KeyFieldWithColumnAction = indexColSQL_KeyField;
                        if (indexColSQL_KeyFieldWithColumnAction != -1)
                            indexColSQL_KeyFieldWithColumnAction += nbColumnAction;

                        keyField = GetCellValue(pItem, indexColSQL_KeyFieldWithColumnAction);
                    }

                    string JSopenerKeyId = JavaScript.JSString(openerKeyId);
                    string JSkeyField = JavaScript.JSString(keyField);

                    int indexColSQLFound = -1;
                    if (openerSpecifiedSQLField.Trim().Length > 0)
                    {
                        indexColSQLFound = Referential.GetIndexColSQL(openerSpecifiedSQLField);
                        // 20091030 RD 
                        if (indexColSQLFound != -1)
                            indexColSQLFound += nbColumnAction;
                    }

                    string JSspecifiedField;
                    string JSopenerSpecifiedId;
                    if (openerSpecifiedId.Trim().Length > 0 && indexColSQLFound != -1)
                    {
                        JSopenerSpecifiedId = JavaScript.JSString(openerSpecifiedId);
                        //20090910 PL Use GetCellValue()
                        //JSspecifiedField = JavaScript.JSString(e.Item.Cells[indexColSQLFound].Text);
                        JSspecifiedField = JavaScript.JSString(GetCellValue(pItem, indexColSQLFound));
                    }
                    else
                    {
                        JSopenerSpecifiedId = JavaScript.JS_NULL;
                        JSspecifiedField = JavaScript.JS_NULL;
                    }

                    string url = "GetReferential(" + JSopenerKeyId + "," + JSkeyField + "," + JSopenerSpecifiedId + "," + JSspecifiedField + ");return false;";
                    pItem.Attributes.Add("ondblclick", url);
                    #endregion
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSortCommand(Object sender, DataGridSortCommandEventArgs e)
        {

            // Processes the sort expression. Determines whether auto-reverse is
            // needed and stores in Attributes two comma-separated strings. One
            // contains the names of the involved columns and one contains – in the
            // same order – the respective direction verbs.
            if (false == IsDataModified)
            {
                ProcessSortExpression(e.SortExpression);
                PrepareSortExpression();
                //
                //Rechargement du datagrid si pagination personalisée car le Datatable ne contient qu'une partie du jeu de résultat
                //Remarque Lorsqu'un tri user est spécifié alors le datagrid charge la totalité du jeu de résultat (voir LoadData)
                IsLoadData = AllowCustomPaging;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPageIndexChanged(Object sender, DataGridPageChangedEventArgs e)
        {
            SetPageIndex(e.NewPageIndex);
        }

        /// <summary>
        /// Génère la publication d'un évènement par le datagrid
        /// </summary>
        /// <param name="eventArgument"></param>
        public void RaisePostBackEvent(string eventArgument)
        {

            LoadDataErrorEventArgs loadDsErrEventArg = null;
            //
            string[] stringSeparators = new string[] { "{-}" };
            String[] aEventArg = eventArgument.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            //
            if (ArrFunc.IsFilled(aEventArg))
            {
                if (aEventArg[0] == "LOAD_DATA_ERROR")
                    loadDsErrEventArg = new LoadDataErrorEventArgs(aEventArg[1], aEventArg[2]);
                //
                if (null != loadDsErrEventArg)
                    OnLoadDataError(loadDsErrEventArg);
            }
        }


        /// <summary>
        /// Chargement et exécution de la requête avant exportation CSV si ce n'est déjà fait
        /// </summary>
        // EG 20190411 [ExportFromCSV]  New 
        public void CsvLoadData()
        {
            if (Referential.SQLPreSelectSpecified)
                ExecutePreSelect();
            LoadData();
        }

        /// <summary>
        /// Chargement du jeu de données
        /// <para>Alimentation d'un dataset (DsData qui sera à l'origine du DataSource du grid)</para>
        /// <para>Si AllowCustomPaging = true alors le chargement est partiel, il ne contient qu'une partie du jeu de résultat</para>
        /// </summary>
        /// FI 20140626 [20142] Appel à ReplaceConsultCriteriaKeyword
        /// FI 20170531 [23206] Modify
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void LoadData()
        {
            ((PageBase)Page).AddAuditTimeStep("LoadData", true);
            IDbTransaction transactIsolationLevel = null;

            try
            {
                string[] referentialWithSubTotal = new string[] { "VW_ACCDAYBOOK", "VW_AGEINGBALANCEDET", "VW_INVOICEFEESDETAIL", "FLOWSRISKCTRL_ALLOC" };

                //initialisation par défaut
                AllowCustomPaging = AllowPaging;
                
                /* FI 20201214 [XXXXX] est déjà valorisé dans le OnInit
                // RD 20110421 [17418] Bug dans le cas de la suppression
                //valueFK = NVC_QueryString["FK"] + string.Empty;
                */

                // RD 20110705 [17502] Appliquer un niveau d'isolation à la transation, au chargement des données.
                IsolationLevel isolationLevel = IsolationLevel.Unspecified;
                string param_IsolationLevel = NVC_QueryString["Iso"];
                if (StrFunc.IsEmpty(param_IsolationLevel))
                    param_IsolationLevel = (string)SystemSettings.GetAppSettings("Spheres_DataViewIso");
                //
                if (StrFunc.IsFilled(param_IsolationLevel))
                {
                    try
                    {
                        isolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel), param_IsolationLevel, true);
                        //
                        if ((isolationLevel == IsolationLevel.Chaos) || (isolationLevel == IsolationLevel.Snapshot))
                            isolationLevel = IsolationLevel.Unspecified;
                    }
                    catch { isolationLevel = IsolationLevel.Unspecified; }
                    //
                    if (IsolationLevel.Unspecified != isolationLevel)
                        transactIsolationLevel = DataHelper.BeginTran(SessionTools.CS, isolationLevel);
                }

                // FI 20170531 [23206] Appel à ReferentialTools.AlterConnectionString
                string cs = ReferentialTools.AlterConnectionString(SessionTools.CS, Referential);

                //QueryParameters query = SQLReferentialData.GetQueryLoadReferential(cs, referential, columnFK, valueFK, false, true, out isQueryWithSubTotal);

                SQLReferentialData.SelectedColumnsEnum selectedColumns = SQLReferentialData.SelectedColumnsEnum.NoHideOnly;
                //PL 20120723 Utilisation de "All" si "ProcessBase", car certains traitements exploitent des données "Hide" pour produire le message MQueue.
                //PL 20120903 Utilisation de "All" si "Consultation".
                if ((this.ListType == Cst.ListType.ProcessBase) || (this.ListType == Cst.ListType.Consultation))
                    selectedColumns = SQLReferentialData.SelectedColumnsEnum.All;

                //PL 20130102 Test in progress... (Use Oracle® hints)
                string sqlHints = string.Empty + NVC_QueryString["hints"];

                SQLReferentialData.SQLSelectParameters sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(cs, selectedColumns, Referential)
                {
                    sqlHints = sqlHints
                };
                bool isTableMainOnly_True = true;
                bool isColumnDataKeyField_False = false;
                // FI 20201215 [XXXXX] Usage de Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField 
                QueryParameters query = SQLReferentialData.GetQuery_LoadReferential(sqlSelectParameters,
                                             Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField, isColumnDataKeyField_False,
                                             isTableMainOnly_True,
                                             out ArrayList alChildSQLSelect, out ArrayList alChildTablename, out bool isQueryWithSubTotal);
                if (isQueryWithSubTotal)
                    AllowSorting = false;   //Interdiction du tri par colonne
                // FI 20201125 [XXXXX] Mise en commentaire (L'appel est déjà effectué en amont)
                //query.Query = ConsultationCriteria.ReplaceConsultCriteriaKeyword(referential,query);

                bool isQueryWithFKValue = (query.Query.IndexOf(Cst.FOREIGN_KEY) >= 0);
                if (isQueryWithFKValue)
                {
                    // FI 20201215 [XXXXX] Usage de Referential.ValueForeignKeyField 
                    query.Query = query.Query.Replace(Cst.FOREIGN_KEY, SQLReferentialData.BuildValueFormated(Referential, Referential.ValueForeignKeyField, isColumnDataKeyField_False));
                }

                //---------------------------------------------------------
                //orderBy sans prendre en consideration les alias des colonnes
                // --------------------------------------------------------------------------------------------------------
                // RD 20120629 [17968] Désormais toutes  les clauses OrderBy sont avec "Alias" (Utilisation de la syntaxe Over( order by ...)), 
                // donc on garde une seule variable: orderByWithColumnAlias
                //string orderBy = SQLReferentialData.GetSQLOrderBy(cs, referential);
                // --------------------------------------------------------------------------------------------------------

                //orderBy où chaque colonne est remplacée par son alias 
                //ie Si colonne TRADRE.EXLLINK as TOTO avec order specifique isnull(trade.EXLLINK,1) 
                //=> order by isnull(TOTO,1)
                string orderByWithColumnAlias = SQLReferentialData.GetSQLOrderBy(cs, Referential, true, true);
                //
                //FI 20101206
                //orderBy où chaque colonne est remplacée par son alias sauf s'il existe un order specifique sur la colonne
                //ie Si colonne TRADRE.EXLLINK as TOTO avec order specifique isnull(trade.EXLLINK,1) 
                //=> order by isnull(trade.EXLLINK,1)
                // --------------------------------------------------------------------------------------------------------
                // RD 20120629 [17968] Désormais toutes  les clauses OrderBy sont avec "Alias" (Utilisation de la syntaxe Over( order by ...)), 
                // donc on garde une seule variable: orderByWithColumnAlias
                //string orderByWithColumnAlias2 = SQLReferentialData.GetSQLOrderBy(cs, referential, true, false);
                // --------------------------------------------------------------------------------------------------------

                if (AllowCustomPaging)
                {
                    AllowCustomPaging = (null == PagingType) || (PagingTypeEnum.CustomPaging == PagingType);

                    //Pas de pagination personalisée sur les référentiel ou il existe des sous-totaux
                    if (AllowCustomPaging)
                        AllowCustomPaging = !ArrFunc.ExistInArray(referentialWithSubTotal, Referential.TableName);
                    //
                    //Pas de pagination personalisée S'il existe un tri 
                    //car il est global à l'ensemble des données => Chargement complet du jeu de résulat
                    if (AllowCustomPaging)
                        AllowCustomPaging = (false == IsSortedByUser);
                    //
                    //Pas de pagination personalisée s'il n'existe par un order 
                    //La pagination personnalisée SQL nécessite la présence d'un order by    
                    // --------------------------------------------------------------------------------------------------------
                    // RD 20120629 [17968] Désormais toutes  les clauses OrderBy sont avec "Alias" (Utilisation de la syntaxe Over( order by ...)), 
                    // donc on garde une seule variable: orderByWithColumnAlias
                    // --------------------------------------------------------------------------------------------------------
                    if (AllowCustomPaging)
                        AllowCustomPaging = StrFunc.IsFilled(orderByWithColumnAlias);
                }
                //
                if (AllowCustomPaging)
                {
                    // Pagination personalisée: calcul du nombre de lignes pour alimentation du VirtualItemCount
                    sqlSelectParameters.selectedColumns = IsConsultation ? SQLReferentialData.SelectedColumnsEnum.All : SQLReferentialData.SelectedColumnsEnum.None;
                    // FI 20201215 [XXXXX] Usage de Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField 
                    QueryParameters queryCount = SQLReferentialData.GetQueryCountReferential(sqlSelectParameters, Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField);
                    // FI 20201125 [XXXXX] Mise en commentaire (L'appel est déjà effectué en amont)
                    //queryCount.Query = ConsultationCriteria.ReplaceConsultCriteriaKeyword(referential, query);
                    if (isQueryWithFKValue)
                        queryCount.Query = queryCount.Query.Replace(Cst.FOREIGN_KEY, SQLReferentialData.BuildValueFormated(Referential, Referential.ValueForeignKeyField, false));

                    Object obj = null;
                    try
                    {
                        // PL 20110303 TEST for SQLServer WITH (TBD)
                        // FI 20201215 [XXXXX] Usage de Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField 
                        queryCount.Query = SQLReferentialData.TransformRecursiveQuery(cs, Referential, Referential.ColumnForeignKeyField , Referential.ValueForeignKeyField, false, queryCount.Query);
                        //
                        ((PageBase)Page).AddAuditTimeStep("LoadData.QueryCount", true, queryCount.QueryReplaceParameters);
                        if (null != transactIsolationLevel)
                            obj = DataHelper.ExecuteScalar(transactIsolationLevel, CommandType.Text, queryCount.Query, queryCount.Parameters.GetArrayDbParameter());
                        else
                            obj = DataHelper.ExecuteScalar(cs, CommandType.Text, queryCount.Query, queryCount.Parameters.GetArrayDbParameter());
                        ((PageBase)Page).AddAuditTimeStep("LoadData.QueryCount", false);
                    }
                    catch (Exception ex)
                    {
                        bool isSqlError = DataHelper.AnalyseSQLException(cs, ex, out string errorCode); ;
                        if (false == isSqlError)
                            throw;
                        //
                        _lastErrorQueryCount = errorCode;
                        obj = 10 * PageSize;
                    }
                    VirtualItemCount = Convert.ToInt32(obj);
                }

                // --------------------------------------------------------------------------------------------------------
                // PL 20191015 WARNING: Test in progress... 
                // --------------------------------------------------------------------------------------------------------
                if ((this.ListType == Cst.ListType.Consultation) && (this.Referential.IdLstConsult == "TRADEOTC_ALLOC") &&
                   (!SessionTools.IsDataArchive) && (SessionTools.License.licensee == "VALBURY") &&
                   (this.Referential.SQLWhere[0].ColumnName == "DTBUSINESS"))
                {
                    try
                    {
                        string businessDateCriteria = this.Referential.SQLWhere[0].LstValue;
                        DateTime dtBusiness = new DtFunc().StringToDateTime(businessDateCriteria, DtFunc.FmtISODate);
                        DateTime dtArchive = new DateTime(2018, 12, 31); //TBD
                        if (DateTime.Compare(dtBusiness, dtArchive) <= 0)
                        {
                            //Le critère date est une date inférieure à la date d'archive
                            string dbNameArchive = "VALBURY_H2018_RO"; //TBD
                            string qryArchive = query.Query.Replace(" dbo.", " " + dbNameArchive + ".dbo.");

                            query.Query += Cst.CrLf;
                            query.Query += "-- ".PadRight(80, '-') + Cst.CrLf;
                            query.Query += SQLCst.UNION + Cst.CrLf;
                            query.Query += "-- ".PadRight(80, '-') + Cst.CrLf;
                            query.Query += qryArchive + Cst.CrLf;
                        }
                    }
                    catch (Exception)
                    {
                        //TBD
                    }
                }
                // --------------------------------------------------------------------------------------------------------
                //
                // Generation de la query définitive
                // --------------------------------------------------------------------------------------------------------
                if (AllowCustomPaging)
                {
                    // Enrichir la requête avec le OrderBy
                    // FI 20210726 [XXXXX] Appel à GetSelectOrderRowNumber
                    query.Query = DataHelper.GetSelectOrderRowNumber(query.Query, orderByWithColumnAlias, true, CurrentPageIndex * PageSize + 1, CurrentPageIndex * PageSize + PageSize);
                }
                else
                {
                    /* FI 20210726 [XXXXX] Appels à DataHelper.GetSelectOrderRowNumber ou DataHelper.GetSelectTop */
                    int top = (int)SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(Int32), -1);
                    if (IsSortedByUser)
                    {
                        if (top > -1)
                        {
                            // Spheres® execute une requête sans tri avec une limitation sur le nb de lignes 
                            // Il faudrait tenir compte du tri utilisateur
                            // Le tri est effectué en c# ensuite
                            query.Query = DataHelper.GetSelectTop(cs, query.Query, top);
                        }
                    }
                    else
                    {
                        // Spheres® execute une requête avec tri et limitation éventuelle sur le nb de lignes appliquée après le tri
                        // Le tri est effectué en c# ensuite
                        query.Query = DataHelper.GetSelectOrderRowNumber(query.Query, orderByWithColumnAlias, true, 1, top);
                    }
                }
                // --------------------------------------------------------------------------------------------------------

                // RD 20120629 mise en commentaire ------------------------------------------------------------------------
                ////
                ////Generation de la query définitive
                //if (AllowCustomPaging)
                //{
                //    // RD 20120524 
                //    // Enrichir la requête avec le OrderBy
                //    //query.Query = query.Query.Replace(orderBy, string.Empty);
                //    query.Query = DataHelper.GetSelectForLimitedRows(query.Query, orderByWithColumnAlias, CurrentPageIndex * PageSize + 1, CurrentPageIndex * PageSize + PageSize);
                //}
                //else if (isSortedByUser)
                //{
                //    //Si chargement complet du jeu de résultat et qu'il existe un tri user, Spheres supprime le tri par défaut 
                //    //=> optimisation de la requête afin de réduire le temps d'exécution
                //    //query.Query = query.Query.Replace(orderBy, string.Empty);
                //    // RD 20120524 
                //    // Ne pas Enrichir la requête avec le OrderBy                    
                //    orderByWithColumnAlias2 = string.Empty;
                //}
                ////Si chargement complet du jeu de résultat, limitation du nbr de lignes en fonction du paramétrage du web.config 
                //if (false == AllowCustomPaging)
                //{
                //    //---------------------------------------------------------------
                //    //PL 20101102 Utilisation systématique de orderByWithColumnAlias pour parer à un bug sous Oracle 
                //    //            dans le cas d'utilisation d'OrderBy et de GroupBy
                //    //            Warning: Changement de code sensible sur l'automate, à bien tester !!
                //    //                     Ultérieurement un refactoring s'imposera pour supprimer totalement l'utilisation de la variable "orderBy"
                //    //---------------------------------------------------------------
                //    //FI 20101206 usage de orderByWithColumnAlias2
                //    // RD 20120524 
                //    // Enrichir la requête avec le OrderBy                    
                //    if (StrFunc.IsFilled(orderByWithColumnAlias2))
                //        query.Query = DataHelper.GetSelectForLimitedRows(query.Query, orderByWithColumnAlias2, 0, 0);
                //        //query.Query = query.Query.Replace(orderBy, orderByWithColumnAlias2);
                //    //---------------------------------------------------------------
                //    int top = (int)SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(Int32), -1);
                //    if (top > -1)
                //        query.Query = DataHelper.GetSelectTop(cs, query.Query, top);
                //}
                // --------------------------------------------------------------------------------------------------------

                //Chargement de la source de donnée                  
                bool isLoadOk = true;
                DatetimeProfiler dtExec = null;
                try
                {
                    // PL 20110303 TEST for SQLServer WITH (TBD)
                    // FI 20201215 [XXXXX] Usage de Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField 
                    query.Query = SQLReferentialData.TransformRecursiveQuery(cs, Referential, Referential.ColumnForeignKeyField, Referential.ValueForeignKeyField, false, query.Query);
                    //Sauvegarde de la Query qui va être exécutée
                    //La sauvegarde est effectuée avant de manière a permettre la consultation de la query même en cas d'erreur SQL

                    //PL 20180620 PERF - New feature
                    if (((PageBase)Page).IsFlushRDBMSCache)
                    {
                        ((PageBase)Page).AddAuditTimeStep("FlushRDBMSCache", true);
                        DataHelper.FlushRDBMSCache(cs);
                        ((PageBase)Page).AddAuditTimeStep("FlushRDBMSCache", false);
                    }

                    //PL 20200227 Use new queryReplaceAndCommentParameters instead of queryReplaceParameters
                    string queryWoParameters = query.QueryReplaceAndCommentParameters;
                    _lastQuery = new Pair<string, TimeSpan>
                    {
                        First = DataHelper.SqlCommandToDisplay(cs, queryWoParameters)
                    };

                    ((PageBase)Page).AddAuditTimeStep("LoadData.QueryData", true, queryWoParameters);
                    dtExec = new DatetimeProfiler(DateTime.Now);

                    DsData = DataHelper.ExecuteDataset(cs, transactIsolationLevel, CommandType.Text, query.QueryHint, query.GetArrayDbParameterHint());

                    _lastQuery.Second = dtExec.GetTimeSpan();
                    ((PageBase)Page).AddAuditTimeStep("LoadData.QueryData", false);

                    DsData.Tables[0].TableName = Referential.TableName;
                    DataSav = null;
                }
                catch (Exception ex)
                {
                    if (null != dtExec)
                        _lastQuery.Second = dtExec.GetTimeSpan();

                    DsData = null;
                    IsBindData = false;
                    //
                    bool isSqlError = DataHelper.AnalyseSQLException(cs, ex, out string errorCode, out string message);
                    if (false == isSqlError)
                        throw;
                    //
                    LoadDataErrorEventArgs args = new LoadDataErrorEventArgs(errorCode, message);
                    //
                    string postBack_Argument = "LOAD_DATA_ERROR" + "{-}" + args.GetEventArgument();
                    string script = Page.ClientScript.GetPostBackEventReference(this, postBack_Argument);
                    //post de la page et invocation de l'évènement LoadDataErrorEvent
                    JavaScript.ScriptOnStartUp((PageBase)this.Page, script, "LOAD_DATA_ERROR");
                    //
                    isLoadOk = false;
                }
                //
                //20070611 PL
                //PL DISPLAY_RUPTURE_TOTAL Code ajouté pour géré en dur une rupture avec sous-total pour les écritures comptables
                // RD 20110712 [17514]
                // Réfactoring pour substituer la valeur de la colonne DTHOLIDAYVALUE, par la prochaine date du jour férié correspondant
                if (isLoadOk)
                {
                    bool isWithSubTotal = ArrFunc.ExistInArray(referentialWithSubTotal, Referential.TableName);
                    //
                    if (isWithSubTotal || (Referential.ExistsColumnDTHOLIDAYVALUE))
                    {
                        DataTable dt = null;
                        //
                        if ((DsData != null) && (DsData.Tables.Count > 0))
                            dt = DsData.Tables[0];
                        //
                        if ((null != dt && 0 < ArrFunc.Count(dt.Rows)))
                        {
                            if (isWithSubTotal)
                            {
                                if ("VW_ACCDAYBOOK" == Referential.TableName)
                                    AddSubTotalToACCDAYBOOK(dt);
                                else if ("VW_AGEINGBALANCEDET" == Referential.TableName)
                                    AddSubTotalToAGEINGBALANCE(dt);
                                else if ("VW_INVOICEFEESDETAIL" == Referential.TableName)
                                    AddSubTotalToINVOICEFEESDETAIL(dt);
                                else if ("FLOWSRISKCTRL_ALLOC" == Referential.TableName)
                                    CalcRiskToFLOWSRISKCTRL_ALLOC(dt);
                            }
                            //
                            if (Referential.ExistsColumnDTHOLIDAYVALUE)
                            {
                                // FI 20190509 [24661] Gestion d'une date de reference
                                Nullable<DateTime> dtRef = null;
                                if (Referential.dynamicArgsSpecified && (Referential.dynamicArgs.Keys.Contains("DTREF")))
                                    dtRef = new DtFunc().StringDateISOToDateTime(Referential.dynamicArgs["DTREF"].GetDataValue(cs, null));
                                ReferentialTools.ValuateDTHOLIDAYVALUE(dt, dtRef);
                            }
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != transactIsolationLevel)
                    DataHelper.RollbackTran(transactIsolationLevel);
            }
            ((PageBase)Page).AddAuditTimeStep("LoadData", false);
        }

        protected virtual void OnLoadDataError(LoadDataErrorEventArgs e)
        {
            LoadDataError?.Invoke(this, e);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void SetIndexForFilter()
        {
            //20090429 PL Modif
            if (StrFunc.IsFilled(openerSpecifiedSQLField))
            {
                indexKeyField = Referential.GetIndexDataGrid(openerSpecifiedSQLField);
                indexColSQL_KeyField = Referential.GetIndexColSQL(openerSpecifiedSQLField);
            }
            else if (typeKeyField == Cst.KeyField && Referential.ExistsColumnKeyField)
            {
                indexKeyField = Referential.IndexKeyField;
                indexColSQL_KeyField = Referential.IndexColSQL_KeyField;
            }
            else if (typeKeyField == Cst.DataKeyField && Referential.ExistsColumnDataKeyField)
            {
                indexKeyField = Referential.IndexDataKeyField;
                indexColSQL_KeyField = Referential.IndexColSQL_DataKeyField;
            }
            
        }
        

        /// <summary>
        /// Retourne la propriété .Text du control qui affiche la colonne {pColName}
        /// <para>LA valeur de retour est formattée selon les caractéristiques de la culture</para>
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        private string GetCellValueByColumnName(DataGridItem pItem, string pColName)
        {
            int index = ((DataRowView)pItem.DataItem).Row.Table.Columns[pColName].Ordinal + nbColumnAction;
            return GetCellValue(pItem, index);
        }

        /// <summary>
        /// Retourne la propriété .Text du control qui affiche la donnée 
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pIndex"></param>
        /// <param name="pConvertToInvariantCulture">false :résultat formatté selon les caractéristiques de la culture, true: résultat  au format invariant</param>
        /// <returns></returns>
        private string GetCellValue(DataGridItem pItem, int pIndex)
        {
            string ret = null;
            //
            if (IsTemplateColumn)
            {
                if ((pItem.Cells[pIndex].Controls.Count > 0)
                    && (pItem.Cells[pIndex].Controls[0] != null))
                {
                    if (pItem.Cells[pIndex].Controls[0].GetType().Equals(typeof(Label)))
                        ret = ((Label)pItem.Cells[pIndex].Controls[0]).Text;
                    else if (pItem.Cells[pIndex].Controls[0].GetType().Equals(typeof(HyperLink)))
                        ret = ((HyperLink)pItem.Cells[pIndex].Controls[0]).Text;
                }
            }
            else
            {
                ret = pItem.Cells[pIndex].Text;
            }
            //
            return ret;
        }

        /// <summary>
        /// Si isNoFailure, alimente la liste des erreurs (alException) avec l'exception rencontrée, sinon génére l'exception
        /// </summary>
        /// <param name="ex">Représente l'exception rencontrée</param>
        private void TrapException(Exception pEx)
        {
            if (IsNoFailure)
            {
                SpheresException2 ex = SpheresExceptionParser.GetSpheresException(null, pEx);
                _alException.Add(ex);
                TemplateDataGridPage.WriteLogException(ex);
            }
            else
            {
                throw pEx;
            }
        }

        /// <summary>
        /// Execute la commande SQLPreSelect définie dans referential
        /// <remarks>Remplace %%DA:PK%% et %%DA:FK%% pouvant exister ds la commande</remarks>
        /// </summary>
        /// FI 20120719 Add try catch pour que lstLastPreSelectCommand soit aussi alimentée en cas d'erreur
        /// FI 20170531 [20206] Modify 
        private void ExecutePreSelect()
        {
            _lastPreSelectCommand = null;
            //
            List<Pair<string, TimeSpan>> lstLastPreSelectCommand = new List<Pair<string, TimeSpan>>();
            try
            {
                if (Referential.SQLPreSelectSpecified)
                {
                    BeforeExecutePreselect();

                    //FI 20191227 [XXXXX] Appel à ReferentialTools.PreparePreSelect
                    QueryParameters[] preSelectCommand = ReferentialTools.PreparePreSelect(Referential, string.Empty);
                    for (int i = 0; i < ArrFunc.Count(preSelectCommand); i++)
                    {
                        // FI 20201125 [XXXXX] Mise en commentaire (L'appel est déjà effectué en amont)
                        //preSelectCommand[i].Query = ReplaceConsultCriteriaKeyword(preSelectCommand[i]);
                        Pair<string, TimeSpan> queryItem = new Pair<string, TimeSpan>
                        {
                            First = DataHelper.SqlCommandToDisplay(SessionTools.CS, preSelectCommand[i].QueryReplaceParameters)
                        };
                        DatetimeProfiler dtExec = null;
                        try
                        {
                            dtExec = new DatetimeProfiler(DateTime.Now);

                            ((PageBase)Page).AddAuditTimeStep("QueryPreSelect." + i.ToString(), true, preSelectCommand[i].QueryReplaceParameters);

                            //PL 20130723 TEST HINT /* Spheres:Hint ... */
                            //DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, preSelectCommand[i].Query, preSelectCommand[i].Parameters.GetArrayDbParameter());

                            // FI 20170531 [20206] Call ReferentialTools.AlterConnectionString
                            string cs = ReferentialTools.AlterConnectionString(SessionTools.CS, Referential);
                            DataHelper.ExecuteNonQuery(cs, CommandType.Text, preSelectCommand[i].QueryHint, preSelectCommand[i].GetArrayDbParameterHint());

                            ((PageBase)Page).AddAuditTimeStep("QueryPreSelect." + i.ToString(), false);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            queryItem.Second = dtExec.GetTimeSpan();
                            lstLastPreSelectCommand.Add(queryItem);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (ArrFunc.IsFilled(lstLastPreSelectCommand))
                    _lastPreSelectCommand = lstLastPreSelectCommand.ToArray();
            }
        }

        /// <summary>
        /// Initialisation spécifique lorsque le grid est en selectMode
        /// </summary>
        private void InitForSelectMode()
        {
            openerKeyId = NVC_QueryString["OpenerKeyId"] + string.Empty;
            openerSpecifiedId = NVC_QueryString["OpenerSpecifiedId"] + string.Empty;
            openerSpecifiedSQLField = NVC_QueryString["OpenerSpecifiedSQLField"] + string.Empty;
            valueForFilter = NVC_QueryString["ValueForFilter"] + string.Empty;
            typeKeyField = NVC_QueryString["TypeKeyField"] + string.Empty;
            //
            //On verifie la value passée en filter (cas où l'on clique sur [...] après avoir renseigné le champ
            // par ex: champ: A     : on va recupérer les lignes commencant par A
            //         champ: AAA;  : on recupère toutes les lignes car ici on est dans le cas d'une multi selection
            //         champ: AAA;B : on va recupérer les lignes commencant par B car ici on est dans le cas d'une multi selection
            if (StrFunc.IsFilled(valueForFilter))
            {
                //PL 20120618 Add test on ";;" (Utile au nouveau bouton d'accès direct au détail du log)
                if (valueForFilter.EndsWith(";;"))
                {
                    valueForFilter = valueForFilter.Remove(valueForFilter.Length - 2);
                }
                else
                {
                    if (valueForFilter.EndsWith(";"))
                        valueForFilter = string.Empty;
                    else if ((valueForFilter.Length > 1) && (valueForFilter.LastIndexOf(";", valueForFilter.Length - 2) >= 0))
                        valueForFilter = valueForFilter.Substring(valueForFilter.LastIndexOf(";", valueForFilter.Length - 2) + 1);
                }
            }
            if (StrFunc.IsFilled(typeKeyField))
            {
                //20090514 PL Add if() and "issuer" 
                if (valueForFilter.IndexOf("{") == 0)
                {
                    valueForFilter = valueForFilter.Replace("{issuer}", string.Empty);
                    valueForFilter = valueForFilter.Replace("{Issuer}", string.Empty);
                    valueForFilter = valueForFilter.Replace("{unknown}", string.Empty);
                    valueForFilter = valueForFilter.Replace("{Unknown}", string.Empty);
                    valueForFilter = valueForFilter.Replace("{null}", string.Empty);
                    valueForFilter = valueForFilter.Replace("{Null}", string.Empty);
                }
                //FI 20111202 Mise en place de l'opérateur Contains à la place de like
                //valueForFilter = valueForFilter.Replace(" ", "%") + "%";
                if (StrFunc.IsEmpty(valueForFilter))
                    valueForFilter = "%";
            }
        }


        /// <summary>
        /// Excécution des certaines tâches nécessaires a l'appel à ExecutePreselect
        /// <para>Ces tâches sont spécifiques à la consultation en cours</para>
        /// </summary>
        // FI 20140115 [19482] upd QUOTE_H_EOD
        // FI 20170531 [23206] Modify
        // EG 20180912 PERF (POSSYNT|POSDET : New temporary table POSSYNTTRD_xxxx_W)
        // EG 20181024 PERF Change model table (TRADE_MOO_MODEL => "TRADE_EOD_MODEL)
        private void BeforeExecutePreselect()
        {
            if (this.Referential.TableName == "POSSYNT")
            {
                //Création des tables spécifiques à la "consultation synthétique des positions" (ces tables sont utilisées par la commande PreSelect)
                string table1 = GetWorkTableName("POSSYNT");
                string table2 = GetWorkTableName("POSSYNTQUOTE");
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSSYNT_MODEL", table1);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSSYNTQUOTE_MODEL", table2);

                if (DbSvrType.dbSQL == DataHelper.GetDbSvrType(SessionTools.CS))
                {
                    string table3 = GetWorkTableName("POSSYNTTRD");
                    if (false == DataHelper.IsExistTable(SessionTools.CS, table3))
                    {
                        DataHelper.CreateTableAsSelect(SessionTools.CS, "TRADE_EOD_MODEL", table3);
                        DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, String.Format("create clustered index UX_{0} on dbo.{0} (IDT)", table3));
                    }
                    else
                    {
                        DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, String.Format("truncate table dbo.{0}", table3));
                    }
                }
            }
            else if (this.Referential.TableName == "POSDET")
            {
                //Création des tables spécifiques à la consultation détaillée des positions 
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName(this.Referential.TableName);
                string table2 = GetWorkTableName("POSDETQUOTE");
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSDET_MODEL", table1);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSDETQUOTE_MODEL", table2);

                if (DbSvrType.dbSQL == DataHelper.GetDbSvrType(SessionTools.CS))
                {
                    string table3 = GetWorkTableName("POSDETTRD");
                    if (false == DataHelper.IsExistTable(SessionTools.CS, table3))
                    {
                        DataHelper.CreateTableAsSelect(SessionTools.CS, "TRADE_EOD_MODEL", table3);
                        DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, String.Format("create clustered index UX_{0} on dbo.{0} (IDT)", table3));
                    }
                    else
                    {
                        DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, String.Format("truncate table dbo.{0}", table3));
                    }
                }
            }
            else if (this.Referential.TableName == "POSDETOTC")
            {
                //Création des tables spécifiques à la consultation détaillée des positions 
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName(this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSDET_MODEL", table1);
            }
            else if (this.Referential.TableName == "QUOTE_H_EOD")
            {
                //Création des tables spécifiques à la consultation des prix EOD
                //Ces tables sont utilisées par la commande PreSelect

                //FI 20140115 [19482] add ASSETQUOTE_EOD
                string table1 = GetWorkTableName("ASSET_EOD");
                string table2 = GetWorkTableName("ASSETQUOTE_EOD");
                //SQLUP.CreateTableFromTableModel(SessionTools.CS, "ASSET_EOD_MODEL", table1);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "ASSET_EOD_MODEL", table1);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "ASSETQUOTE_EOD_MODEL", table2);
            }
            else if ((this.Referential.TableName == "POSACTIONDET") || (this.Referential.TableName == "POSACTIONDET_OTC"))
            {
                string table1 = GetWorkTableName(this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSACTIONDET_MODEL", table1);
            }
            // EG 20151019 [21465]
            else if (this.Referential.TableName.StartsWith("ASSEXEBULK") || this.Referential.TableName.StartsWith("ABNBULK"))
            {
                // Création des tables spécifiques au traitement de masse des dénouements
                // FI 20231215 [WI783] add Table DENOPTBULKPA_MODEL => Table qui stocke, pour chaque trade, les qtés impliquées dans des actions
                string table1 = GetWorkTableName(this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "DENOPTBULK_MODEL", table1);

                string table2 = GetWorkTableName("PA" + this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "DENOPTBULKPA_MODEL", table2);

                string table3 = GetWorkTableName("Q" + this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "DENOPTBULKQUOTE_MODEL", table3);
            }
            // EG 20151130 [20979]
            else if (this.Referential.TableName.StartsWith("CLEARINGBULK"))
            {
                //Création des tables spécifiques au traitement de masse des compensations de masse
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName(this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "CLEARINGBULK_MODEL", table1);
            }
            // FI 20170531 [23206] Add EARGEN
            else if (this.Referential.TableName == "EARGEN")
            {
                //Création des tables spécifiques (Lancement du traitement des EARs)
                //Table utilisée par la commande PreSelect
                string table1 = GetWorkTableName(this.Referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "EARGEN_MODEL", table1);
            }
        }
        
        /// <summary>
        ///  Convertie une donnée {pData} au format invariant
        /// </summary>
        /// <param name="pData">Représente la donnée</param>
        ///<param name="pDataType">Type de la donnée</param>
        /// <returns></returns>
        /// FI 20110111 Add FormatToInvariant
        private static string FormatToInvariant(string pData, string pDataType)
        {
            string ret = pData;

            // RD 20130327 [18534] 
            // Dans le cas ou la donnée est vide ne pas la renseigner, afin notamment dans le cas d'un @integer, ne pas mettre 0 à la place.
            if (!String.IsNullOrEmpty(ret))
            {
                if (TypeData.IsTypeInt(pDataType))
                {
                    //Suppression du séparateur de millier
                    // EG 20150920 [21314] Int (int32) to Long (Int64) 
                    ret = IntFunc.IntValue64(ret).ToString();
                }
                else if (TypeData.IsTypeDate(pDataType))
                {
                    ret = new DtFunc().GetTimeStringFromIso(ret);
                }
                else if (TypeData.IsTypeDec(pDataType))
                {
                    ret = StrFunc.FmtAmountToInvariantCulture(ret);
                }
                else if (TypeData.IsTypeString(pDataType))
                {
                    //Rien à faire
                }
                else
                {
#if DEBUG
                    throw new NotImplementedException(StrFunc.AppendFormat("DataType {0} is not implemented", pDataType));
#endif
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne le nom de la table de travail 
        /// <para>Les tables de travail se compose ainsi {pPrefix}_{ShortSessionID}_W </para>
        /// </summary>
        /// <param name="pTableModel"></param>
        /// <returns></returns>
        private static string GetWorkTableName(string pPrefix)
        {
            return pPrefix + "_" + SessionTools.ShortSessionID.ToUpper() + "_W";
        }

        /// <summary>
        /// Retourne la valeur associée à la colonne FK
        /// <para>La valeur est au format iso</para>
        /// <para>Retourne string.Empty s'il n'existe pas de colonne FK</para>
        /// </summary>
        /// <param name="pDataGridItem"></param>
        /// <returns></returns>
        /// FI 20130208 [] new method
        private string GetItemValueFK(DataGridItem pDataGridItem)
        {
            string ret = string.Empty;
            // FI 20201215 [XXXXX] Usagde de Referential.ExistsColumnForeignKeyField
            //if (columnFK.Length > 0)
            if (Referential.ExistsColumnForeignKeyField)
            {
                if (Referential.ColumnPosition_ForeignKeyField >= 0)
                {
                    //PL 20201215 Warning: lorsque le DataGrid est présenté dans un ordre différent du formulaire (usage du tag <ColumnPositionInDataGrid>), 
                    //                     si la colonne FK se trouve dans le descriptif XML après une colonne affichée après elle dans le DataGrid, il importe 
                    //                     d'utiliser les accesseurs et indexeur suivant (ex. QUOTE_FXRATE_H).
                    //                     
                    ret = GetCellValue(pDataGridItem, Referential.ColumnPosition_ForeignKeyField + nbColumnAction);
                    ret = FormatToInvariant(ret, Referential.Column[Referential.IndexForeignKeyField].DataType.value);
                }
                else
                {
                    ret = GetCellValue(pDataGridItem, Referential.IndexColSQL_ForeignKeyField + nbColumnAction);
                    ret = FormatToInvariant(ret, Referential[Referential.IndexForeignKeyField].DataType.value);
                }
            }
            return ret;
        }

        /// <summary>
        /// Initialisation du type de pagination en fonction de la requête Http
        /// <para>Par défaut et sans présence de l'argument custompaging dans la requête HTTP, le grid utilise la pagination native </para>
        /// </summary>
        private void InitPagingType()
        {
            //Pagination personalisée (Warning: A partir de la v3.1, plus de pagination personalisée par défaut)
            string QS_AllowCustomPaging = string.Empty + NVC_QueryString["allowcustompaging"];
            QS_AllowCustomPaging += string.Empty + NVC_QueryString["custompaging"];
            if (StrFunc.IsEmpty(QS_AllowCustomPaging))
                QS_AllowCustomPaging = SystemSettings.GetAppSettings("AllowCustomPaging", "false");
            PagingType = BoolFunc.IsTrue(QS_AllowCustomPaging) ? PagingTypeEnum.CustomPaging : PagingTypeEnum.NativePaging;
        }

        /// <summary>
        ///  Rend les colonnes actions visibles ou pas 
        /// </summary>
        /// FI 20130417 [18596] add Method
        public void SetColumnActionVisible(bool pIsVisible)
        {
            if (nbColumnAction > 0)
            {
                for (int i = 0; i < nbColumnAction; i++)
                {
                    DataGridColumn column = this.Columns[i];
                    column.Visible = pIsVisible;
                }
            }
        }
        /// <summary>
        /// Recherche dans le datarow d'une colonne qui contient la category d'un asset
        /// <para>Recherche des colonnes suivantes par ordre de préférence ASSETCATEGORY,ASSET_CATEGORY,ASSETTYPE,ASSET_TYPE</para>
        /// </summary>
        /// <param name="referential"></param>
        /// <param name="row"></param>
        /// <param name="isUnderluyingAsset"></param>
        /// <returns></returns>
        /// FI 20131206 [] add method
        /// FI 20140120 [19482] add parameter pIsUnderlying
        private static Nullable<Cst.UnderlyingAsset> GetAssetCategory(ReferentialsReferential pReferential, DataRow pRow, bool pIsUnderlying)
        {
            Nullable<Cst.UnderlyingAsset> ret = null;

            string suffix = (pIsUnderlying) ? "_UNL" : string.Empty;

            for (int i = 0; i < 4; i++)
            {
                string col = string.Empty;
                if (i == 0)
                    col = "ASSETCATEGORY" + suffix;
                else if (i == 1)
                    col = "ASSET_CATEGORY" + suffix;
                else if (i == 2)
                    col = "ASSETTYPE" + suffix;
                else if (i == 3)
                    col = "ASSET_TYPE" + suffix;

                int index = pReferential.GetIndexColSQL(col);
                if (index > 0)
                {
                    string value = pRow.ItemArray[index].ToString();
                    if (Enum.IsDefined(typeof(Cst.UnderlyingAsset), value))
                    {
                        ret = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), value);
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Recherche dans le datarow d'une colonne qui contient la category d'un contract
        /// <para>Recherche des colonnes suivantes par ordre de préférence CONTRACTCATEGORY,CONTRACT_CATEGORY,CONTRACTTYPE,CONTRACT_TYPE</para>
        /// </summary>
        /// <param name="referential"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        /// FI 20170908 [23409] Add Method
        private static Nullable<Cst.ContractCategory> GetContractCategory(ReferentialsReferential referential, DataRow row)
        {
            Nullable<Cst.ContractCategory> ret = null;

            for (int i = 0; i < 4; i++)
            {
                string col = string.Empty;
                if (i == 0)
                    col = "CONTRACTCATEGORY";
                else if (i == 1)
                    col = "CONTRACT_CATEGORY";
                else if (i == 2)
                    col = "CONTRACTTYPE";
                else if (i == 3)
                    col = "CONTRACT_TYPE";

                int index = referential.GetIndexColSQL(col);
                if (index > 0)
                {
                    string value = row.ItemArray[index].ToString();
                    if (Enum.IsDefined(typeof(Cst.ContractCategory), value))
                    {
                        ret = (Cst.ContractCategory)Enum.Parse(typeof(Cst.ContractCategory), value);
                        break;
                    }
                }
            }
            return ret;
        }



        /// <summary>
        /// Affiche le document associé à l'enregistrement  pRowIndex
        /// <param name="pRowIndex"></param>
        /// </summary>
        /// FI 20140409 [XXXXX] add Method
        private void ConsultDoc(int pRowIndex)
        {
            //On recupere le dataKeyField de la ligne courante
            ReferentialsReferentialColumn rrc = Referential.Column[Referential.IndexDataKeyField];
            //
            string[] keyColumns = new string[3];
            keyColumns[0] = "TABLENAME";
            keyColumns[1] = "ID";
            keyColumns[2] = rrc.ColumnName;
            string[] keyValues = new string[3];
            //PL 20160914 Use DecodeDA()
            keyValues[0] = Ressource.DecodeDA(this.NVC_QueryString["DA"]);
            keyValues[1] = this.NVC_QueryString["FK"];

            //FI 20140409 [XXXXX] Utilisation de DataSource parce qu'un grid peut être trié au niveau de l'entête 
            //keyValues[2] = DsData.Tables[0].Rows[e.Item.DataSetIndex][rrc.ColumnName].ToString();
            DataRowView rowview = ((DataView)(this.DataSource))[pRowIndex];
            keyValues[2] = rowview.Row[rrc.ColumnName].ToString();
            string[] keyDatatypes = new string[3];
            keyDatatypes[0] = TypeData.TypeDataEnum.@string.ToString();
            keyDatatypes[1] = (ObjectName == Cst.OTCml_TBL.ATTACHEDDOC.ToString() ? TypeData.TypeDataEnum.integer.ToString() : TypeData.TypeDataEnum.@string.ToString());
            keyDatatypes[2] = rrc.DataType.value;

            //On renvoie le document dans le flux "Response"
            LOFileColumn dbfc = new LOFileColumn(SessionTools.CS, ObjectName, keyColumns, keyValues, keyDatatypes);
            if (dbfc != null)
            {
                //Lecture des données 
                LOFile loFile = dbfc.LoadFile();
                
                if (null != loFile.FileContent)
                {
                    //Génération du fichier physique
                    string path = SessionTools.TemporaryDirectory.PathMapped;
                    // FI 20200409 [XXXXX] Call ReplaceFilenameInvalidChar
                    string fileName = FileTools.ReplaceFilenameInvalidChar(loFile.FileName);

                    if (Cst.ErrLevel.SUCCESS == FileTools.WriteBytesToFile(loFile.FileContent, path + fileName,FileTools.WriteFileOverrideMode.Override))
                    {
                        //Postage du fichier physique dans le flux HTML
                        TemplateDataGridPage.ResponseWriteFile(loFile.FileType, fileName, path);
                    }
                }
            }
        }

        /// <summary>
        /// Retourne les enregistrements affichés sur la page courante  
        /// <para>Cette méthode prend en considération l'éventuel tri existant sur le grid</para>
        /// <para>Retourne null si le jeu de résultat ne retourne aucune ligne</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20140519 [19923] Add methode
        /// FI 20141021 [20350] Modify method signature (add pOnlyIsSelectedRow)
        public DataRow[] GetRowPage(Boolean pOnlyDetail)
        {
            DataRow[] ret = null;

            if (IsDataAvailable)
            {
                if (!(DataSource is DataView dataView))
                    throw new Exception("Source is not an DataView");

                DataRowCollection rows = dataView.ToTable().Rows;
                if (rows.Count > 0)
                {
                    DataRow[] rowSource = new DataRow[rows.Count];
                    rows.CopyTo(rowSource, 0);

                    if (AllowPaging && false == AllowCustomPaging)
                    {
                        int firstIndex = CurrentPageIndex * PageSize;
                        int lastIndex = (CurrentPageIndex + 1) * PageSize - 1;
                        lastIndex = Math.Max(0, Math.Min(lastIndex, rows.Count - 1));
                        int count = lastIndex - firstIndex + 1;
                        ret = new DataRow[count];
                        Array.Copy(rowSource, firstIndex, ret, 0, count);
                    }
                    else
                    {
                        ret = rowSource;
                    }

                    if (pOnlyDetail)
                    {
                        if (dataView.Table.Columns.Contains("GROUPBYNUMBER"))
                            ret = ret.Where(r => Convert.ToInt32(r["GROUPBYNUMBER"]) == 0).ToArray();
                    }
                }
            }

            return ret;
        }




        /// <summary>
        /// Ecrase le jeu de résultat (DsData) s'il existe un filtre (control txtSearch alimenté)
        /// <para>Spheres® affichent uniquement les enregistrements qui matchent avec la donnée saisie</para>
        /// </summary>
        /// FI 20140926 [XXXXX] Add Method
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        /// FI 20171102 [XXXXX] Modify
        /// FI 20190607 [XXXXX] Gestion de Afficher/Masquer
        /// FI 20191218 [XXXXX] Refactoring => Mise en place de tâches asynchrone
        private void LoadSearch()
        {
            if ((IsDataAvailable) && (false == IsSelfClear))
            {
                Cst.DisplayMask mode = default;
                if (Page.FindControl("ddlSearch") is WCDropDownList2 dllSearch)
                    mode = (Cst.DisplayMask)Enum.Parse(typeof(Cst.DisplayMask), dllSearch.SelectedValue);

                string filterValue = string.Empty;
                if (Page.FindControl("txtSearch") is TextBox txtSearch)
                    filterValue = txtSearch.Text;

                if (StrFunc.IsFilled(filterValue))
                {
                    //Sauvegarde du jeu de résultat de la requeête SQL sans l'élément de recherche présent dans DsData si isLoadData
                    if (DataSav == null)
                    {
                        DataTable dt = DsData.Tables[0];
                        DataSav = dt.Clone();
                        foreach (DataRow row in dt.Rows)
                            DataSav.ImportRow(row);
                    }

                    // FI 20170116 [21916]
                    //Recherche sur une colonne en particulier   (exemple in:IDENTIFIER Filip => Recherche de la string Filip uniquement dans la colonne IDENTIFIER)  
                    string particularColumnName = string.Empty;
                    if (filterValue.StartsWith("in:")) //exemple in:IDENTIFIER Filip
                    {
                        int indexFirstSpace = filterValue.IndexOf(" ");
                        if (indexFirstSpace > -1)
                        {
                            particularColumnName = filterValue.Substring(3, indexFirstSpace - 3); //  columnName => IDENTIFIER
                            // FI 20211118 [XXXX] Amélioration pour ne pas prendre les éventuels caractères espace saisis
                            //filterValue = filterValue.Split(' ')[1];  // value => Filip
                            filterValue = filterValue.Substring(indexFirstSpace + 1); // value => Filip 
                        }
                        else
                        {
                            //Ex in:IDENTIFIER
                            particularColumnName = filterValue.Substring(3);
                            if (StrFunc.IsEmpty(particularColumnName))
                                particularColumnName = Cst.NotAvailable;
                            filterValue = string.Empty;
                        }
                    }

                    DataTable dtWork = DataSav.Copy();
                    int nbRows = DataSav.Rows.Count;
                    // FI 20200224 [XXXXX] searchColumns est de type List<GridColumn> puisque utilisée dans une tâche asynchrone
                    List<GridColumn> searchColumns = GetColumnsSearch(dtWork, particularColumnName, false).ToList();

                    ContextFormat context = GetCurrentContextFormat();

                    // Plusieurs Tâches peunent explorer le jeu de donnée
                    // Chaque Tâche Traite 1000 enregistrements max;
                    const int nbRowByTask = 1000;

                    List<DataRow> rowFilter = new List<DataRow>();
                    Boolean isModeAsync = (nbRows > nbRowByTask);
                    if (false == isModeAsync)
                    {
                        rowFilter = LoadRowsFilter(context, dtWork, searchColumns, mode, filterValue, null, null);
                    }
                    else
                    {
                        int nbTask = Convert.ToInt32(Math.Truncate(nbRows / Convert.ToDecimal(nbRowByTask))) + 1;

                        List<Task> tasks = new List<Task>();
                        for (int i = 0; i < nbTask; i++)
                        {
                            int indexStart = i * nbRowByTask;
                            tasks.Add(
                                 Task.Run(() => LoadRowsFilter(context, dtWork, searchColumns, mode, filterValue, indexStart, nbRowByTask))
                                );
                        }

                        try
                        {
                            Task.WaitAll(tasks.ToArray());
                        }
                        catch (AggregateException ae)
                        {
                            throw ae.Flatten();
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        foreach (Task<List<DataRow>> taskItem in tasks)
                            rowFilter.AddRange(taskItem.Result);
                    }

                    //DsData.Tables[0] est écrasé avec les lignes filtrées
                    DsData.Tables[0].Rows.Clear();
                    foreach (DataRow row in rowFilter)
                        DsData.Tables[0].ImportRow(row);

                }
                else if (false == IsLoadData)
                {
                    //Si le filtre està blanc => Spheres® restitue le jeu de résultat de la requête SQL
                    if (null != DataSav)
                    {
                        DsData.Tables[0].Rows.Clear();
                        foreach (DataRow row in DataSav.Rows)
                            DsData.Tables[0].ImportRow(row);
                    }
                }
            }
        }


        /// <summary>
        ///  Retourne true s'il une donnée qui matche avec le filtre  pour l'enregistremnt {pRow} et sur la colonne {pDataColumn}
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pIndexCol">index de la colonne dans le jeu de résultat</param>
        /// <param name="pDataColumn">Nom de la colonne dans le jeu de résultat</param>
        /// <param name="pRow">Représente l'enregistrement</param>
        /// <param name="pFilter">représente le filtre </param>
        /// <returns></returns>
        /// FI 20140926 [XXXXX] Add Method
        /// FI 20170116 [21916] Modify
        /// FI 20171025 [23533] Modify
        /// FI 20200224 [XXXXX] Method Static, Add pContext
        private static Boolean IsDataMatch(ContextFormat pContext, int pIndexCol, DataColumn pDataColumn, DataRow pRow, string pFilter)
        {
            bool ret = false;

            if (pRow[pDataColumn.ColumnName] != Convert.DBNull)
            {
                Boolean columnPositionInDataGridSpecified = pContext.referential.Column.Where(x => x.ColumnPositionInDataGridSpecified).Count() > 0;

                // FI 20171025 [23533] usage de GetFormatedData 
                // Rq si columnPositionInDataGridSpecified alors l'ordre des colonnes dans le grid n'est pas le même que l'ordre des colonnes  dans le jeu de résultat
                // => Spheres ne fait pas appel à GetFormatedData
                string sValue = string.Empty;
                if (pIndexCol > -1 && (false == columnPositionInDataGridSpecified))
                    sValue = GetFormatedData(pContext, pIndexCol, pRow);
                else
                    sValue = pRow[pDataColumn.ColumnName].ToString();

                // FI 20201230 [XXXXX] bold tags removed
                sValue = HtmlTools.HTMLBold_Remove(sValue);

                // FI 20111124 [XXXXX] Gestion des mots clé {##E}, {##M}, {##C}
                if (pFilter.Length < 5)
                    throw new NotSupportedException($"{pFilter} not supported.{{##E}},{{##M}},{{##C}} expected");

                String filterKey = pFilter.Substring(0, 5);
                String filter = pFilter.Remove(0, 5);
                switch (filterKey)
                {
                    case "{##E}": //Equal
                        ret = (sValue == filter);
                        break;
                    case "{##M}": //Match
                        Regex regEx = new Regex(filter, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        ret = regEx.IsMatch(sValue);
                        break;
                    case "{##C}": //Contains
                        ret = sValue.ToUpper().Contains(filter.ToUpper());
                        break;
                    default:
                        throw new NotSupportedException($"{filter} is not supported.{{##E}},{{##M}},{{##C}} expected");
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne le true si la colonne {pDataColumn} est utilisée pour la recherche à travers le filtre libre
        /// </summary>
        /// <param name="pDataColum"></param>
        /// <param name="pParticularColumn"></param>
        /// <param name="pIsForLoadAutoComplete">
        /// <para>
        /// true, s'il s'agit de prendre en considération les colonnes lors du chgt des données autocomplete associé au filtre libre
        /// </para>
        /// <para>
        /// false, s'il s'agit de prendre en considération les colonnes lors de la recherche de données dans le grid qui matchent avec le filtre libre
        /// </para>
        /// </param>
        /// FI 20171025 [23533] Add
        private static Boolean IsColumnMatch(DataColumn pDataColumn, String pParticularColumn, Boolean pIsForLoadAutoComplete)
        {

            Boolean ret =
                pDataColumn.DataType != typeof(Boolean) &&
                            (false == pDataColumn.ColumnName.StartsWith("TRK_")) &&
                            (pDataColumn.ColumnName != "RANK_COL") &&
                            (pDataColumn.ColumnName != "ROWSTATE") &&
                            (pDataColumn.ColumnName != "ROWSTYLE") &&
                            (pDataColumn.ColumnName != "ISSELECTED") &&
                            (pDataColumn.ColumnName != "ROWVERSION");

            if (pIsForLoadAutoComplete)
            {
                //Les colonnes TEXT (avec souvent des tags HTML, des retours chariots, etc ) ne sont pas considérées lors de l'alimenation de l'autocomplete associé au filtre libre
                //=> inutile de charger des données trop gourmande (et sans intérêt fonctionnel dans le cadre de propositions autocomplete)
                //Spheres les reconnait les colonne TEXT via leur nom qui contient MESSAGE (peut-être faudra-t-il mieux faire à lavenir ?)
                ret = ret && (!pDataColumn.ColumnName.EndsWith("MESSAGE")); //ie SHORTMESSAGE, MESSAGE
            }

            if (ret && StrFunc.IsFilled(pParticularColumn))
                ret = (pDataColumn.ColumnName == pParticularColumn);

            return ret;
        }

        /// <summary>
        /// Retourne l'échéance formatée en fonction du nom de colonne SQL et du format d'affichage appliqué à l'utilisateur 
        /// </summary>
        /// <param name="columnName">Nom de colonne </param>
        /// <param name="columnValue">valeur présente dans la colonne</param>
        /// <param name="pRRcDatatype"></param>
        /// <param name="pFmtDisplayCollection">Liste des formats d'affichage</param>
        /// <returns></returns>
        /// FI 20140926 [XXXXX] Add Method
        /// FI 20171025 [23533] Add Gestion de pRRcDatatype (Permet d'éviter le codage en dur ssur les colonnes)
        /// FI 20200106 [XXXXX] Add des formats d'affichage de la session
        private static string GetFormattedMaturity(string columnName, string columnValue, ReferentialsReferentialColumnDataType pRRcDatatype, NameValueCollection pFmtDisplayCollection)
        {

            Cst.ETDMaturityInputFormatEnum format = Cst.ETDMaturityInputFormatEnum.FIX;

            if (columnName.EndsWith("MATFMT_MMMYY") ||
                ((pRRcDatatype.datakind == Cst.DataKind.ETDMaturity) && pRRcDatatype.displaySpecified && pRRcDatatype.display == Cst.DataTypeDisplayMode.MatFmt_MMMspaceYY))
            {
                format = Cst.ETDMaturityInputFormatEnum.MMMspaceYY;
            }
            else if (columnName.EndsWith("MATFMT_MY") ||
                ((pRRcDatatype.datakind == Cst.DataKind.ETDMaturity) && pRRcDatatype.displaySpecified && pRRcDatatype.display == Cst.DataTypeDisplayMode.MatFmt_MMMspaceYY))
            {
                format = Cst.ETDMaturityInputFormatEnum.MY;
            }
            else if (columnName.EndsWith("MATFMT_PROFIL")
                ||
                ((pRRcDatatype.datakind == Cst.DataKind.ETDMaturity) && pRRcDatatype.displaySpecified && pRRcDatatype.display == Cst.DataTypeDisplayMode.MatFmt_Profil))
            {
                format = (Cst.ETDMaturityInputFormatEnum)Enum.Parse(typeof(Cst.ETDMaturityInputFormatEnum), pFmtDisplayCollection["ETDMaturityFormat"]);
            }

            string ret = MaturityHelper.FormatMaturityFIX(columnValue.Replace("~", string.Empty), format);

            return ret;
        }


        /// <summary>
        /// Sauvegarde de l'enregistrement principal
        /// </summary>
        /// <param name="pRowIndex"></param>
        /// FI 20141021 [20350] Add
        private DataRow SaveDataRow(int pRowIndex)
        {
            DataTable tablSav = DsData.Tables[0].Clone();
            tablSav.ImportRow(DsData.Tables[0].Rows[pRowIndex]);
            return tablSav.Rows[0];
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si Création, Modification, Annulation d'un élément
        /// </summary>
        /// <param name="pActionType"></param>
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
                ((PageBase)this.Page).RequestTrackBuilder = builder;
            }
        }

        /// <summary>
        /// Extrait le contenu du contrôle serveur dans un objet System.Web.UI.HtmlTextWriter fourni
        /// </summary>
        /// <param name="writer"></param>
        /// FI 20160308 [21782] Add Method
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (IsModeExportExcel)
                PrepareControlForExcel();

            base.RenderControl(writer);
        }

        /// <summary>
        /// Extrait le contenu du contrôle serveur dans un objet System.Web.UI.HtmlTextWriter fourni
        /// <para>Le rendu HTML êut être interpété par Microsoft Excel</para>
        /// <para>Les colonnes actions et le footer se sont pas extraits</para>
        /// <para>Les contrôles sont remplacés par des cellules</para>
        /// <param name="pWriter"></param>
        /// </summary>
        /// FI 20160308 [21782] Add Method
        private void PrepareControlForExcel()
        {
            ShowHeader = true;
            ShowFooter = false;

            SetColumnActionVisible(false);

            List<string> log = new List<string>();
            ReplaceControls(this, log);
            if (log.Count > 0)
            {
                Debug.Write("********* PrepareControlForExcel: ReplaceControls LOG **********" + Cst.CrLf);
                foreach (string item in log)
                    Debug.Write(item + Cst.CrLf);
                Debug.Write("****************************************************************");
            }
        }

        /// <summary>
        /// Remplace certains contrôles de manières à exporter le grid en excel
        /// </summary>
        /// <param name="pCtrl"></param>
        /// PL 20130416 [18596] add method
        /// FI 20160308 [21782] Modify
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        // EG 20230706 [XXXXX][WI624] Inversion valeurs pour literalControl.Text (pCtrl is Panel)
        private void ReplaceControls(Control pCtrl, List<string> pLog)
        {
            // On récupère le nombre de controles enfants composant le DataGrid
            int nbControls = pCtrl.Controls.Count - 1;


            while (nbControls >= 0)
            {
                if (pCtrl.Visible) // FI 20190318 [24568][24588] Ajout de pCtrl.Visible == true
                    ReplaceControls(pCtrl.Controls[nbControls], pLog);
                nbControls--;
            }

            if (pCtrl is DataGrid) // FI 20190318 [24568][24588] Ajout du cas is DataGrid
            {
                DataGrid datagrid = pCtrl as DataGrid;
                datagrid.CssClass = string.Empty;
                datagrid.ControlStyle.Width = Unit.Empty;
                datagrid.ControlStyle.BorderStyle = System.Web.UI.WebControls.BorderStyle.NotSet;
            }
            else if (pCtrl is DataGridItem)
            {
                // FI 20160308 [21782] suppression des attributs, style , class  pour allerger le flux HTML
                // row.Style est conservé car interprété par Spheres® 
                DataGridItem row = pCtrl as DataGridItem;
                // FI 20190318 [24568]  HorizontalAlign.NotSet et VerticalAlign.NotSet pour alléger le flux
                row.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.NotSet;
                row.VerticalAlign = System.Web.UI.WebControls.VerticalAlign.NotSet;

                row.Attributes.Remove("ondblclick");
                row.Attributes.Remove("onmouseover");
                row.Attributes.Remove("onmouseout");

                // FI 20190318 [24568][24588] on conserve le style associé au lignes de regroupemment uniquement
                if ((row.CssClass != EFSCssClass.DataGrid_GroupStyle) && (row.CssClass != EFSCssClass.DataGrid_GroupStyle1) && (row.CssClass != EFSCssClass.DataGrid_GroupStyle2))
                    row.CssClass = string.Empty;

                row.ToolTip = null;
            }
            else if (pCtrl is TableCell)
            {
                // FI 20160308 [21782] suppression des attributs, class  pour allerger le flux HTML
                // style est conservé car interprété par Spheres® 
                TableCell cell = (pCtrl as TableCell);
                cell.Attributes.Remove("onclick");
                cell.CssClass = string.Empty;
                cell.VerticalAlign = VerticalAlign.NotSet;
                cell.HorizontalAlign = HorizontalAlign.NotSet;
                cell.Wrap = true; // Supprime l'attribut style="white-space:nowrap;"
                cell.ToolTip = null;
            }
            else if (pCtrl is Panel) // FI 20160308 [21782] gestion des panel qui représente les images true/false
            {
                WebControl control = pCtrl as WebControl;
                if (control.CssClass.Contains("fa-icon"))
                {
                    if ((control is Panel) && control.HasControls() && (control.Controls[0] is LiteralControl))
                    {
                        LiteralControl ctrlChild = control.Controls[0] as LiteralControl;
                        if (ctrlChild.Text.Contains("check"))  // Font Awesome => TRUE
                        {
                            LiteralControl literalControl = new LiteralControl
                            {
                                Text = "1"
                            };
                            pCtrl.Parent.Controls.Add(literalControl);
                            pCtrl.Parent.Controls.Remove(pCtrl);
                        }
                        else if (ctrlChild.Text.Contains("times")) // Font Awesome => FALSE
                        {
                            LiteralControl literalControl = new LiteralControl
                            {
                                Text = "0"
                            };
                            pCtrl.Parent.Controls.Add(literalControl);
                            pCtrl.Parent.Controls.Remove(pCtrl);
                        }
                    }
                }
                else if (pCtrl is WCToolTipPanel) // FI 20190318 [24568][24588] gestion des WCToolTipPanel où prt-exdata est renseigné
                {
                    if (control.Attributes.Count > 0)
                    {
                        if (StrFunc.IsFilled(control.Attributes["prt-exdata"]))
                        {
                            LiteralControl literalControl = new LiteralControl
                            {
                                Text = FormatStringDataExcel(control.Attributes["prt-exdata"])
                            };
                            pCtrl.Parent.Controls.Add(literalControl);
                            pCtrl.Parent.Controls.Remove(pCtrl);
                        }
                    }
                }
                else
                {
                    string log = StrFunc.AppendFormat("No replace of Panel: {0} ;", pCtrl.ClientID);
                    if (false == pLog.Contains(log))
                        pLog.Add(log);
                }
            }
            else if (pCtrl is System.Web.UI.WebControls.Image image)
            {
                if (StrFunc.IsFilled(image.ImageUrl))
                {
                    if ((image.ImageUrl.ToLower().EndsWith("0.png") || (image.ImageUrl.ToLower().EndsWith("false.png"))))
                    {
                        LiteralControl literalControl = new LiteralControl
                        {
                            Text = "0"
                        };
                        pCtrl.Parent.Controls.Add(literalControl);
                        pCtrl.Parent.Controls.Remove(pCtrl);
                    }
                    else if ((image.ImageUrl.ToLower().EndsWith("1.png") || (image.ImageUrl.ToLower().EndsWith("true.png"))))
                    {
                        LiteralControl literalControl = new LiteralControl
                        {
                            Text = "1"
                        };
                        pCtrl.Parent.Controls.Add(literalControl);
                        pCtrl.Parent.Controls.Remove(pCtrl);
                    }
                    else
                    {
                        string log = StrFunc.AppendFormat("No replace of Image: {0} ;", image.ImageUrl);
                        if (false == pLog.Contains(log))
                            pLog.Add(log);
                    }
                }
            }
            else if ((pCtrl.GetType().GetProperty("SelectedItem") != null))
            {
                // La cellule prend alors pour valeur le texte correspondant à la propriété "SelectedItem"
                string txt = (string)(pCtrl.GetType().GetProperty("SelectedItem").GetValue(pCtrl, null));
                // FI 20190318 [24568][24588] call FormatTextExcel
                if (StrFunc.IsFilled(txt))
                    txt = FormatTextExcel(pCtrl.ID, txt);

                LiteralControl literalControl = new LiteralControl
                {
                    Text = txt
                };
                pCtrl.Parent.Controls.Add(literalControl);

                // Le controle concerné est retiré
                pCtrl.Parent.Controls.Remove(pCtrl);
            }
            else if ((pCtrl.GetType().GetProperty("Text") != null))
            {
                // On attribue le texte de la propriété "Text" à la cellule concernée
                string txt = (string)(pCtrl.GetType().GetProperty("Text").GetValue(pCtrl, null));
                // FI 20190318 [24568][24588] call FormatTextExcel
                if (StrFunc.IsFilled(txt) && StrFunc.IsFilled(pCtrl.ID))
                    txt = FormatTextExcel(pCtrl.ID, txt);

                LiteralControl literalControl = new LiteralControl
                {
                    Text = txt
                };
                pCtrl.Parent.Controls.Add(literalControl);

                pCtrl.Parent.Controls.Remove(pCtrl);
            }
            else
            {
                string log = StrFunc.AppendFormat("No replace of Ctrl: {0} ;", pCtrl.GetType().ToString());
                if (false == pLog.Contains(log))
                    pLog.Add(log);
            }
        }


        /// <summary>
        /// Retourne la donnée {pTxt} (présente dans le controle {pControlId}) pour un affichage correct sous Excel
        /// </summary>
        /// <param name="pControlId"></param>
        /// <param name="pTxt"></param>
        /// <returns></returns>
        /// FI 20190318 [24568][24588] Add Method
        private string FormatTextExcel(string pControlId, string pTxt)
        {
            Boolean isRelation = false;
            string ret = pTxt;

            //Recherche du rrc
            ReferentialsReferentialColumn rrc = (from item in Referential.Column.Where(x => x.IDForItemTemplate == pControlId)
                                                 select item).FirstOrDefault();
            if (null == rrc)
            {
                rrc = (from item in Referential.Column.Where(x => x.IDForItemTemplateRelation == pControlId)
                       select item).FirstOrDefault();

                if (null != rrc)
                    isRelation = true;
            }
            // si pControlId est piloté par un ReferentialsReferentialColumn
            if (null != rrc)
            {
                Boolean isStringData = false;

                if (isRelation)
                    isStringData = TypeData.IsTypeString(rrc.Relation[0].ColumnSelect[0].DataType);
                else
                    isStringData = TypeData.IsTypeString(rrc.DataType.value);

                if (isStringData)
                {
                    if (rrc.ColumnName.IndexOf("MATFMT_") >= 0)
                    {
                        // Astuce pour que Excel n'interprète pas une donnée maturité en date
                        //Ex Mars 19 => Excel doit afficher Mars 19 (alignement gauche) et non pas Mars 19 (alignement droite) qui est la représentation de la date 01/03/2019
                        ret = StrFunc.AppendFormat(@"=""{0}""", ret);
                    }
                    else
                    {
                        ret = FormatStringDataExcel(ret);
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// Retourne la donnée string {pStringData}  pour un affichage correct sous Excel
        /// </summary>
        /// <param name="pStringData"></param>
        /// <returns></returns>
        /// FI 20190318 [24568][24588] Add Method
        private static string FormatStringDataExcel(string pStringData)
        {
            string ret = pStringData;
            /* FI 20211215 [25904] mise en commentaire
            Boolean isUseEqual = false;
            if (false == new Regex(@"[a-zA-Z_]").IsMatch(ret))
            {
                decimal parseResult;
                // Astuce pour que Excel n'interprète pas une donnée string en donnée numérique 
                // Ex Book identifier ="0900075", Excel doit afficher 0900075 (alignement gauche) et non pas 900075 (alignement à droite)
                // Optimisation => Usage de la regex en priorité dont l'exécution est plus rapide qu'un Decimal.TryParse
                isUseEqual = new Regex(@"^\d+$").IsMatch(ret) || Decimal.TryParse(ret, out parseResult);
            }
            */

            // FI 20211215 [25904] Modification de la regex pour tenir compte des données qui commencent par =, @, +, -
            // Si la cellule commence par =, @, +, - => mise en place de ="{donnée}"
            // Si la cellule contient une donnée numérique  => mise en place de ="{donnée}"
            Boolean isUseEqual = new Regex(@"^[\+-@=]|^\d+$").IsMatch(ret);
            if (isUseEqual)
                ret = StrFunc.AppendFormat(@"=""{0}""", ret);

            return ret;
        }



        // EG 20170125 [Refactoring URL] New
        // FI 20170908 [23409] Modify
        // EG 20200923 [XXXXX] Correction Mauvais lien Hyperlink sur Trade Risk (ajout INDENTIFIER du PRODUCT)
        // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        private string GetURL(string pMode, string pColumn, string pData, DataGridItem pDataGridItem)
        {
            // Pour interdire le dblclick sur les lignes de rupture)
            if ((pMode != "INSERT") && StrFunc.IsEmpty(pData))
                return "return false";

            string url = string.Empty;
            bool isOpenReferential = false;
            bool isOpenTrade = false;

            DataRow row = null;
            if (null != pDataGridItem)
                row = ((DataRowView)(pDataGridItem.DataItem)).Row;

            Nullable<IdMenu.Menu> idMenu = null;
            if (pMode == "HYPERLINK")
            {
                #region Mode Hyperlink
                if (null != pDataGridItem)
                {
                    switch (pColumn)
                    {
                        case "IDT":
                            isOpenTrade = true;
                            idMenu = SpheresURL.GetMenu_Trade(pColumn, Referential.TableName, row, Referential.GetIndexColSQL("GPRODUCT"), Referential.GetIndexColSQL("PRODUCT_IDENTIFIER"));
                            break;
                        case "IDASSET":
                        case "IDASSET_UNL":
                        case "IDCONTRACT":
                            Nullable<Cst.UnderlyingAsset> assetCategoryEnum = GetAssetCategory(Referential, row, pColumn == "IDASSET_UNL");
                            if (assetCategoryEnum.HasValue)
                                idMenu = SpheresURL.GetMenu_Asset(pColumn, Referential.TableName, assetCategoryEnum);
                            break;
                        case "IDXC": // FI 20170908 [23409] Add IDXC
                            Nullable<Cst.ContractCategory> contractCategoryEnum = GetContractCategory(Referential, row);
                            if (contractCategoryEnum.HasValue)
                                idMenu = SpheresURL.GetMenu_Contract(pColumn, contractCategoryEnum);
                            break;
                        case "IDIOELEMENT":
                            idMenu = SpheresURL.GetMenu_IO(row, Referential.GetIndexColSQL("ELEMENTTYPE"));
                            break;
                        default:
                            idMenu = SpheresURL.GetMenu_Repository(pColumn, pData);
                            break;
                    }
                }

                url = SpheresURL.GetURL(idMenu, pData);
                #endregion Mode Hyperlink
            }
            else
            {
                #region LOUPE
                isOpenReferential = true;
                string pageName;
                if (Referential.ExistsColumnDataKeyField)
                {
                    string DKF_ColumnName = Referential.Column[Referential.IndexDataKeyField].ColumnName;
                    isOpenTrade = (DKF_ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.TRADE.ToString()));
                    if (isOpenTrade)
                    {
                        isOpenReferential = false;
                        #region Consultations sur la base de la table TRADE
                        idMenu = SpheresURL.GetMenu_Zoom(row, ObjectName, Referential.TableName, Referential.GetIndexColSQL("GPRODUCT"), Referential.GetIndexColSQL("PRODUCT_IDENTIFIER"));
                        url = SpheresURL.GetURL(idMenu, pData, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal);
                        #endregion Consultations sur la base de la table TRADE
                    }
                    else if (DKF_ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.EVENT.ToString()))
                    {
                        isOpenReferential = false;
                        #region Consultation d'un événement
                        string sIdEvent = pData.ToString();
                        string sIdT = string.Empty;
                        int indexIDT = Referential.GetIndexColSQL("IDT");
                        if (-1 < indexIDT)
                            sIdT = row.ItemArray[indexIDT].ToString();

                        _ = SpheresURL.GetUrlFormEvent(HttpUtility.UrlEncode(sIdEvent, Encoding.Default), HttpUtility.UrlEncode(sIdT, Encoding.Default), SessionTools.Menus);

                        url = SpheresURL.GetURL(IdMenu.Menu.InputEvent, pData, sIdT, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, string.Empty, DataCacheKeyDsData);

                        #endregion
                    }
                    else if (DKF_ColumnName == "DATAKEYFIELD")
                    {
                        isOpenReferential = false;
                        #region Consultation de la POSITION
                        idMenu = SpheresURL.GetMenu_Zoom(row, ObjectName, Referential.TableName, Referential.GetIndexColSQL("GPRODUCT"), Referential.GetIndexColSQL("PRODUCT_IDENTIFIER"));
                        url = SpheresURL.GetURL(idMenu, null, HttpUtility.UrlEncode(pData, Encoding.Default),
                            SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, AddDynamicArgument(), DataCacheKeyDsData);
                        #endregion SpheresURL de la POSITION
                    }
                    else if ((DKF_ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.VW_ENTITYCSS.ToString()))
                        && ((ObjectName == "ENTITYCSS_EOD") || (ObjectName == "ENTITYCSS_CLOSINGDAY")))
                    {
                        isOpenReferential = false;
                        #region POSREQUEST des traitements EOD et CLOSINGDAY
                        idMenu = (ObjectName == "ENTITYCSS_EOD") ? IdMenu.Menu.LogEndOfDay : IdMenu.Menu.LogClosingDay;
                        url = SpheresURL.GetURL(idMenu, pData, string.Empty, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, AddDynamicArgument(), DataCacheKeyDsData);
                        #endregion POSREQUEST des traitements EOD et CLOSINGDAY
                    }
                    else if (DKF_ColumnName.StartsWith("IDCA"))
                    {
                        isOpenReferential = false;
                        #region CORPORATE ACTION embedded|issue
                        idMenu = (DKF_ColumnName == "IDCA" ? IdMenu.Menu.InputCorporateActionEmbedded : IdMenu.Menu.InputCorporateActionIssue);
                        url = SpheresURL.GetURL(idMenu, pData, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal);
                        #endregion CORPORATE ACTION embedded|issue
                    }
                    else if (DKF_ColumnName == "IDARQ")
                    {
                        isOpenReferential = false;
                        #region CLOSING/REOPENING POSITION
                        idMenu = IdMenu.Menu.InputClosingReopeningPosition;
                        url = SpheresURL.GetURL(idMenu, pData, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal);
                        #endregion CLOSING/REOPENING POSITION
                    }
                }
                #endregion

                #region Repository
                if (isOpenReferential)
                {
                    string @objectItem = ObjectName;
                    //FI 20120904 Bidouille juste avant la sortie de la release
                    //Lorsque l'utilisateur effectue un dbClick sur un enregistrement de la consultation  des extraits de compte
                    //Spheres® ouvre un un formulaire qui s'appuie MCO_RPT.xml
                    if (@objectItem == "MCO_RPT_FINPER")
                        @objectItem = "MCO_RPT";

                    string idMenu2 = IDMenu;
                    string type = Title;

                    if (IsConsultation)
                    {
                        //FI 20120215 Ds le cas des consultation Le formulaire est ouvert à partir du fichier xml présent sous le répertoire  consultation
                        type = Cst.ListType.Consultation.ToString();
                    }

                    if (Referential.TargetNameSpecified)
                        //Ouverture sur un autre XML (ex. FEE_ACTOR --> FEEMATRIX)
                        @objectItem = Referential.TargetName;

                    #region OpenReferentialFormArguments
                    OpenReferentialFormArguments arg = new OpenReferentialFormArguments
                    {
                        Type = type,
                        @Object = @objectItem,
                        IdMenu = idMenu2,
                        PK = pColumn,
                        PKValue = pData,

                        Mode = Referential.consultationMode
                    };

                    if (IsGridSelectMode || IsFormSelectMode)
                        arg.Mode = Cst.ConsultationMode.Select;
                    else if (IsFormViewerMode)
                        arg.Mode = Cst.ConsultationMode.ReadOnly;

                    if (StrFunc.IsFilled(condApp))
                        arg.CondApp = condApp;

                    arg.DS = DataCacheKeyDsData;
                    arg.Param = param;
                    // FI 20200205 [XXXXX] Seuls les paramètres GUI et URL sont transmis
                    //arg.dynamicArg = Referential.xmlDynamicArgs;
                    arg.DynamicArg = ReferentialTools.DynamicDatasToString(Referential.dynamicArgs, DynamicDataSourceEnum.GUI | DynamicDataSourceEnum.URL);

                    arg.IsNewRecord = (pMode == "INSERT");
                    string itemValueFK = Referential.ValueForeignKeyField; // FI 20201215 [XXXXX] Usagde de Referential.ValueForeignKeyField
                    if (false == arg.IsNewRecord)
                        itemValueFK = GetItemValueFK(pDataGridItem);
                    if (StrFunc.IsFilled(itemValueFK))
                    {
                        arg.FK = Referential.ColumnForeignKeyField; // FI 20201215 [XXXXX] Usagde de Referential.ColumnForeignKeyField
                        arg.FKValue = itemValueFK;
                    }

                    arg.TitleMenu = TitleMenu;
                    arg.TitleRes = TitleRes;
                    arg.FormId = Parent.ID;

                    pageName = arg.GetURLOpenFormReferential();
                    if (StrFunc.IsFilled(pageName))
                        url = JavaScript.GetWindowOpen(pageName, Cst.WindowOpenStyle.EfsML_FormReferential);
                    #endregion
                }
                #endregion Repository

            }

            if (SessionTools.User.IsSessionGuest && (false == isOpenTrade) && (false == isOpenReferential))
            {
                //UserWithLimitedRights
                //NB: On conserve uniquement les liens sur des trades
                url = string.Empty;
            }
            return url;
        }
        #endregion

        /// <summary>
        /// Retourne le nbr de decimale à appliquer aux colonnes qui représente une quantité
        /// <para>le nbr de decimale est fonction du contrat/asset</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20170223 [22883] Add
        /// FI 20171025 [23533] Private Method
        private static Nullable<int> GetQtyScale(string pCS, ReferentialsReferential rr, DataRow dr)
        {
            Nullable<int> ret = null;
            if (ret == null)
            {
                Pair<int, Cst.UnderlyingAsset> contrat = GetContract(rr, dr);
                if (null != contrat)
                {
                    switch (contrat.Second)
                    {
                        case Cst.UnderlyingAsset.Commodity:
                            SQL_CommodityContract sqlCommodityContract = new SQL_CommodityContract(CSTools.SetCacheOn(pCS), contrat.First);
                            bool isOk = sqlCommodityContract.LoadTable(new string[] { "IDCC", "SCALEQTY" });
                            if (false == isOk)
                                throw new NullReferenceException(StrFunc.AppendFormat("CommodityContract (Id:{0}) not found", contrat.First));
                            ret = sqlCommodityContract.QtyScale;
                            break;
                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                            ret = 0;
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("UnderlyingAsset (:{0}))", contrat.Second));
                    }
                }
            }

            if (ret == null)
            {
                Pair<int, Cst.UnderlyingAsset> asset = GetAsset(rr, dr);
                if (null != asset)
                {
                    _ = EfsML.AssetTools.NewSQLAsset(CSTools.SetCacheOn(pCS), asset.Second, asset.First);
                    switch (asset.Second)
                    {
                        case Cst.UnderlyingAsset.Commodity:
                            SQL_AssetCommodityContract sql_AssetCommodity = new SQL_AssetCommodityContract(CSTools.SetCacheOn(pCS), asset.First);
                            Boolean isOk = sql_AssetCommodity.LoadTable(new string[] { "IDCC", "SCALEQTY" });
                            if (isOk) // false est possible si asset commodity sans CommodityContract
                                ret = sql_AssetCommodity.CommodityContract_QtyScale;
                            break;
                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                            ret = 0;
                            break;
                            // FI 20191023 [XXXXX] La function retourne null (pas de précision particuliere cas des GILTs par exemple)
                            //default:
                            //    ret = 0;
                            //    break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        ///  Retourne le 1er Contrat (DERIVATIVECONTRACT ou COMMODITYCONTRACT) qui pourrait être associé à l'enregistrement dr
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// FI 20170223 [22883] Add
        private static Pair<int, Cst.UnderlyingAsset> GetContract(ReferentialsReferential rr, DataRow dr)
        {

            Pair<int, Cst.UnderlyingAsset> ret = null;

            /* Recherche du contrat à partir d'un link */
            ReferentialsReferentialColumn colLinkContrat =
                (from item in
                     rr.Column.Where(x => x.ExistsHyperLinkColumn && (x.IsHyperLink.type == "IDDC" ||
                                                                      x.IsHyperLink.type == "IDCC" ||
                                                                      x.IsHyperLink.type == "IDCONTRACT"))
                 select item).DefaultIfEmpty(new ReferentialsReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();

            if (colLinkContrat.ColumnName != "N/A")
            {
                Nullable<Cst.UnderlyingAsset> assetCategory = null;
                switch (colLinkContrat.IsHyperLink.type)
                {
                    case "IDDC":
                        assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;
                        break;
                    case "IDCC":
                        assetCategory = Cst.UnderlyingAsset.Commodity;
                        break;
                    case "IDCONTRACT":
                        assetCategory = GetAssetCategory(rr, dr, false);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("HyperLinkType (Id:{0}) is not implemented", colLinkContrat.IsHyperLink.type));
                }

                int index = rr.GetIndexColSQL(colLinkContrat.IsHyperLink.data);
                if (index == -1)
                    throw new Exception(StrFunc.AppendFormat("column (Id:{0}) not found", colLinkContrat.IsHyperLink.data));

                if (dr[index] != Convert.DBNull) //DBNull possible su colonne total
                    ret = new Pair<int, Cst.UnderlyingAsset>(Convert.ToInt32(dr[index]), assetCategory.Value);

            }

            /* Recherche du contrat à partir d'un Relation */
            if (null == ret)
            {

                ReferentialsReferentialColumn colRelationContrat =
                (from item in
                     rr.Column.Where(x => ArrFunc.IsFilled(x.Relation) && (x.Relation[0].TableName == "DERIVATIVECONTRACT" ||
                                                                            x.Relation[0].TableName == "COMMODITYCONTRACT" ||
                                                                            x.Relation[0].TableName == "VW_COMMODITYCONTRAT"
                                                                            ))
                 select item).DefaultIfEmpty(new ReferentialsReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();
                if (colRelationContrat.ColumnName != "N/A")
                {
                    Nullable<Cst.UnderlyingAsset> assetCategory = null;

                    switch (colRelationContrat.Relation[0].TableName)
                    {
                        case "DERIVATIVECONTRACT":
                            assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;
                            break;
                        case "COMMODITYCONTRACT":
                        case "VW_COMMODITYCONTRACT":
                            assetCategory = Cst.UnderlyingAsset.Commodity;
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("Relation on TableName (Id:{0}) is not implemented", colRelationContrat.Relation[0].TableName));
                    }

                    if (false == (dr.Table.Columns.Contains(colRelationContrat.DataField)))
                        throw new Exception(StrFunc.AppendFormat("column (Id:{0}) not found", colRelationContrat.DataField));

                    if (dr[colRelationContrat.DataField] != Convert.DBNull) //DBNull possible sur colonne total
                        ret = new Pair<int, Cst.UnderlyingAsset>(Convert.ToInt32(dr[colRelationContrat.DataField]), assetCategory.Value);
                }
            }

            return ret;
        }

        /// <summary>
        ///  Retourne le 1er asset qui pourrait être associé à l'enregistrement dr
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// FI 20170223 [22883] Add
        /// FI 20170303 [XXXXX] Modify
        private static Pair<int, Cst.UnderlyingAsset> GetAsset(ReferentialsReferential rr, DataRow dr)
        {

            Pair<int, Cst.UnderlyingAsset> ret = null;

            ReferentialsReferentialColumn colLinkAsset =
                (from item in
                     rr.Column.Where(x => x.ExistsHyperLinkColumn && x.IsHyperLink.type.StartsWith("IDASSET"))
                 select item).DefaultIfEmpty(new ReferentialsReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();


            if (colLinkAsset.ColumnName != "N/A")
            {
                Nullable<Cst.UnderlyingAsset> assetCategory = null;
                switch (colLinkAsset.IsHyperLink.type)
                {
                    case "IDASSET":
                        assetCategory = GetAssetCategory(rr, dr, false);
                        break;
                    case "IDASSET_ETD":
                        assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;
                        break;
                    case "IDASSET_INDEX":
                        assetCategory = Cst.UnderlyingAsset.Index;
                        break;
                    case "IDASSET_RATEINDEX":
                        assetCategory = Cst.UnderlyingAsset.RateIndex;
                        break;
                    case "IDASSET_COMMODITY":
                    case "IDASSET_COMMODITYCONTRACT":
                        assetCategory = Cst.UnderlyingAsset.Commodity;
                        break;
                    case "IDASSET_EQUITY":
                        assetCategory = Cst.UnderlyingAsset.EquityAsset;
                        break;
                    case "IDASSET_EXTRDFUND":
                        assetCategory = Cst.UnderlyingAsset.ExchangeTradedFund;
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("HyperLinkType (Id:{0}) is not implemented", colLinkAsset.IsHyperLink.type));
                }

                int index = rr.GetIndexColSQL(colLinkAsset.IsHyperLink.data);
                if (index == -1)
                    throw new Exception(StrFunc.AppendFormat("column (Id:{0}) not found", colLinkAsset.IsHyperLink.data));

                if (dr[index] != Convert.DBNull) //DBNull possible sur colonne total
                    ret = new Pair<int, Cst.UnderlyingAsset>(Convert.ToInt32(dr[index]), assetCategory.Value);

            }

            /* Recherche du contrat à partir d'un Relation */
            // FI 20170303 [XXXXX] test TableName != null
            if (null == ret)
            {
                ReferentialsReferentialColumn colRelationAsset =
                (from item in
                     rr.Column.Where(x => ArrFunc.IsFilled(x.Relation) && (StrFunc.IsFilled(x.Relation[0].TableName)) &&
                                                                           (x.Relation[0].TableName.StartsWith("ASSET") ||
                                                                           x.Relation[0].TableName.StartsWith("VW_ASSET")))
                 select item).DefaultIfEmpty(new ReferentialsReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();
                if (colRelationAsset.ColumnName != "N/A")
                {
                    Nullable<Cst.UnderlyingAsset> assetCategory = null;
                    switch (colRelationAsset.Relation[0].TableName)
                    {
                        case "ASSET":
                        case "VW_ASSET":
                            assetCategory = GetAssetCategory(rr, dr, false);
                            break;
                        case "ASSET_ETD":
                            assetCategory = Cst.UnderlyingAsset.ExchangeTradedContract;
                            break;
                        case "ASSET_INDEX":
                            assetCategory = Cst.UnderlyingAsset.Index;
                            break;
                        case "ASSET_RATEINDEX":
                            assetCategory = Cst.UnderlyingAsset.RateIndex;
                            break;
                        case "ASSET_COMMODITY":
                        case "ASSET_COMMODITYCONTRACT":
                            assetCategory = Cst.UnderlyingAsset.Commodity;
                            break;
                        case "ASSET_EQUITY":
                            assetCategory = Cst.UnderlyingAsset.EquityAsset;
                            break;
                        case "ASSET_EXTRDFUND":
                            assetCategory = Cst.UnderlyingAsset.ExchangeTradedFund;
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("Relation on TableName (Id:{0}) is not implemented", colRelationAsset.Relation[0].TableName));
                    }

                    if (false == (dr.Table.Columns.Contains(colRelationAsset.DataField)))
                        throw new Exception(StrFunc.AppendFormat("column (Id:{0}) not found", colRelationAsset.DataField));

                    if (dr[colRelationAsset.DataField] != Convert.DBNull) //DBNull possible sur colonne total
                        ret = new Pair<int, Cst.UnderlyingAsset>(Convert.ToInt32(dr[colRelationAsset.DataField]), assetCategory.Value);
                }
            }
            return ret;

        }


        /// <summary>
        ///  Retourne les colonnes utilisées par le groupby représenté via {pGroupByNumber}
        /// <para> Rq si l'utilisateur choisit de trier,regrouper sur 3 colonnes COL1, COL2 et COL3 et que le regroupement est de type sous-totaux et totaux et que grid est affché COL1, puis COL2, puis COL3</para> 
        /// <para> - pGroupByNumber = 1 signifie regroupement sur COL1, COL2, COL3</para>
        /// <para> - pGroupByNumber = 2 signifie regroupement sur COL1, COL2</para>
        /// <para> - pGroupByNumber = 3 signifie regroupement sur COL1</para>
        /// <para>L'ordre d'affichage est déterminant</para>
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pGroupByNumber"></param>
        /// FI 20170228 [22883] Add
        /// FI 20200224 [XXXXX] Method Static
        private static List<ReferentialsReferentialColumn> GetColumnsOrder(ReferentialsReferential pReferential, int pGroupByNumber)
        {
            List<ReferentialsReferentialColumn> ret = new List<ReferentialsReferentialColumn>();

            List<ReferentialsReferentialColumn> columnsGroupBy
                            = (from item in pReferential.Column.Where(x => x.GroupBySpecified && x.GroupBy.IsGroupBy)
                               select item).ToList();

            int nbColumnsGroupByNumber = columnsGroupBy.Count() - pGroupByNumber + 1;

            int k = 0;
            foreach (ReferentialsReferentialColumn item in columnsGroupBy)
            {
                k++;
                ret.Add(item);
                if (k == nbColumnsGroupByNumber)
                    break;
            }

            return ret;
        }

        /// <summary>
        ///  Retourne les lignes de détail associées à une ligne de regroupement 
        /// </summary>
        /// <returns></returns>
        /// <param name="pReferential"></param>
        /// <param name="pGroupRow">Représente l'enregistrement de regroupement </param>
        /// FI 20170228 [22883] Add
        /// FI 20200224 [XXXXX] Method Static
        private static DataRow[] LoadRowsDetail(ReferentialsReferential pReferential, DataRow pGroupRow)
        {
            int groupByNumber = Convert.ToInt32(pGroupRow["GROUPBYNUMBER"]);
            List<ReferentialsReferentialColumn> columnsOrder = GetColumnsOrder(pReferential, groupByNumber);
            var dataColumnsOrder = from item in
                                       columnsOrder
                                   select new
                                   {
                                       column = item,
                                       columnValue = pGroupRow[item.DataField]
                                   };

            string filter = "groupByNumber = 0"; // considération des lignes de détail 
            foreach (var item in dataColumnsOrder)
            {
                if (item.columnValue == Convert.DBNull)
                    filter += StrFunc.AppendFormat(" and {0} is null", item.column.DataField);
                else
                {
                    if (TypeData.IsTypeInt(item.column.DataType.value) || TypeData.IsTypeBool(item.column.DataType.value))
                    {
                        filter += StrFunc.AppendFormat(" and {0} = {1}", item.column.DataField, item.columnValue);
                    }
                    else if (TypeData.IsTypeDec(item.column.DataType.value))
                    {
                        string value = StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(item.columnValue));
                        filter += StrFunc.AppendFormat(" and {0} = {1}", item.column.DataField, value);
                    }
                    else if (TypeData.IsTypeDate(item.column.DataType.value))
                    {
                        string value = DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(item.columnValue));
                        filter += StrFunc.AppendFormat(" and {0} = '{1}'", item.column.DataField, value);
                    }
                    else
                    {
                        filter += StrFunc.AppendFormat(" and {0} = '{1}'", item.column.DataField, item.columnValue);
                    }
                }
            }

            DataRow[] ret = pGroupRow.Table.Select(filter);

            return ret;
        }

        

        /// <summary>
        ///  Retourne précision à appliquer sur une colonne decimale
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pIndex">Index de la colonne</param>
        /// <param name="pRow">Représente l'enregistrement</param>
        /// FI 20171025 [23533] Add
        /// FI 20200224 [XXXXX] Method Static
        private static Nullable<int> GetScale(ContextFormat pContext, int i, DataRow pRow)
        {
            // FI 20170223 [22883] Recherche du nbr de décimale en fonction du contract/asset

            // FI 20191023 [XXXXX] La méthode peut retourner une valeur null sur les quantités
            // Nullable<int> ret = 0; //0 Decimale par défaut => Semblant de compatibilité ascendante
            Nullable<int> ret = null;
            if (IsColumnQty(pContext, i))
            {
                try
                {
                    if (GetGroupNummber(pRow) > 0)
                    {
                        DataRow[] rowsDetail = LoadRowsDetail(pContext.referential, pRow);
                        if (ArrFunc.IsFilled(rowsDetail))
                        {
                            Decimal[] decValues =
                                (from item in rowsDetail
                                 select Convert.ToDecimal(item[pContext.alColumnName[i].ToString()] == Convert.DBNull ? 0 : item[pContext.alColumnName[i].ToString()])).ToArray();

                            ret = DecFunc.PrecisionOf(decValues);
                        }
                    }
                    else
                    {
                        ret = GetQtyScale(pContext.CS, pContext.referential, pRow);
                        // FI 20191023 [XXXXX]
                        // Mise en commentaire: Valeur de retour null autorisée (Cas des GILTS notamment)
                        //if (false == ret.HasValue)
                        //    ret = 0;
                    }
                }
                catch // A conserver 3 mois => A supprimer que toutes les corrections seront appliquées  
                {
#if DEBUG
                    throw;
#else
                                    ret = 0;
#endif
                }
            }
            else
            {
                // FI 20191023 [XXXXX] compatibilié ascendante
                ret = 0;
                if (StrFunc.IsFilled(pContext.alColumnDataScale[i].ToString()))
                    ret = IntFunc.IntValue(pContext.alColumnDataScale[i].ToString());
            }

            return ret;
        }
        /// <summary>
        ///  Retourne true si la colonne représente une quantité
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pIndex">Index de la colonne</param>
        /// <returns></returns>
        /// FI 20191023 [XXXXX] Add Method
        /// FI 20200224 [XXXXX] Method Static
        private static Boolean IsColumnQty(ContextFormat pContext, int pIndex)
        {
            Boolean ret =
             (TypeData.IsTypeDec(pContext.alColumnDataType[pIndex].ToString()) &&
                (pContext.alColumnName[pIndex].ToString().Contains("QTY") || pContext.alColumnName[pIndex].ToString().Contains("QUANTITY") ||
                ((ReferentialsReferentialColumnDataType)pContext.alColumnDataTypeLongForm[pIndex]).datakind == Cst.DataKind.Qty));
            return ret;
        }

        /// <summary>
        /// Obtient une valeur supérieure à zéro si l'enregistrement {drv} est une ligne de regroupment 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        private static int GetGroupNummber(DataRow pRow)
        {
            int ret = 0;
            if ((pRow.Table.Columns.Contains("GROUPBYNUMBER")))
                ret = (null != pRow["GROUPBYNUMBER"] ? Convert.ToInt32(pRow["GROUPBYNUMBER"]) : 0);
            return ret;
        }

        /// <summary>
        ///  Retourne la donnée formattée
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pIndex">Index de la colonne</param>
        /// <param name="pRow">Représente l'enregistrement</param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        /// FI 20200106 [XXXXX] Add des formats d'affichage de la session
        /// FI 20200108 ATTENTION, la méthode GetFormatedData peut est appelée dans un tâche asynchrone (thread séparé), il ne faut pas utiliser la classe SessionTools (HttpContext.Current)
        /// FI 20200224 [XXXXX] Method static , Add ContextFormat
        private static string GetFormatedData(ContextFormat pContext, int pIndex, DataRow pRow)
        {
            string columnName = pContext.alColumnName[pIndex].ToString();
            object colValue = pRow[columnName];
            string ret = colValue.ToString(); // Rq si colValue == Convert.DBNull  alors ret = String.Empty

            //FI 20111112 <Multi-Books> si book non présent
            //FI 20120306 <Multi-Books> si book non présent
            if (pContext.lstConsult.IsConsultation(LstConsult.ConsultEnum.MCO_RPT))
            {
                if ((pContext.alColumnName[pIndex].ToString() == "b_stp_IDENTIFIER") && StrFunc.IsEmpty(ret))
                    ret = "&lt;Multi-Books&gt;";
            }
            else if ((pContext.lstConsult.IdLstConsult == "REF-ProcessBaseTRADE_RIMGEN"))
            {
                if ((pContext.alColumnName[pIndex].ToString() == "bsendto_IDENTIFIER") && StrFunc.IsEmpty(ret))
                    ret = "&lt;Multi-Books&gt;";
            }

            try
            {
                string dataType = pContext.alColumnDataType[pIndex].ToString();
                ReferentialsReferentialColumnDataType rrcDataType = (ReferentialsReferentialColumnDataType)pContext.alColumnDataTypeLongForm[pIndex];

                if (TypeData.IsTypeBool(dataType))
                    ret = (BoolFunc.IsTrue(ret) ? "TRUE" : "FALSE");

                if (StrFunc.IsFilled(ret))
                {
                    if (TypeData.IsTypeDateTime(dataType) || TypeData.IsTypeDate(dataType) || TypeData.IsTypeTime(dataType))
                    {
                        ret = ReferentialTools.GetFormatedDateTime(pContext.fmtDisplayCol, pContext.Collaborator, rrcDataType, colValue, pRow);
                    }
                    else if (TypeData.IsTypeInt(dataType))
                    {
                        if (pContext.alColumnName[pIndex].ToString() != "ROWVERSION")//rowversion ne peut être cast in int32
                        {
                            //FI 20120207 Lorque la colonne est de type string
                            //Spheres® considère que les données sont en invariant culture
                            //C'est le cas des données issues d'extraction XML
                            //Exemple d'expression de colonne: extractvalue(t_lsd.TRADEXML,'(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@LastQty)[0]')
                            if (colValue.GetType().Equals(typeof(String)))
                                ret = StrFunc.FmtDecimalToCurrentCulture(ret);

                            // AL 20240703 [WI605] Datakind Seconds for integer type
                            if (rrcDataType.datakindSpecified && rrcDataType.datakind == Cst.DataKind.Seconds)
                            {
                                ret = ReferentialTools.GetFormattedDuration(IntFunc.IntValue(ret));
                            }
                            else
                            {
                                // EG 20150920 [21314] Int (int32) to Long (Int64) 
                                long intvalue = IntFunc.IntValue64(ret);
                                ret = intvalue.ToString("n0");
                            }
                        }
                    }
                    else if (TypeData.IsTypeDec(dataType))
                    {
                        if (columnName.EndsWith("VALORISATION_FMT"))
                        {
                            // RoundPrec application by Currency for Amount/Qty/Rate
                            ReferentialsReferentialColumn rrc1 = pContext.referential["UNITTYPE"];
                            ReferentialsReferentialColumn rrc2 = pContext.referential["UNIT"];
                            if ((null != rrc1) && (null != rrc2))
                                ret = FormattingValorisation(pContext.currencyInfo, pRow[rrc1.ColumnName].ToString(), ret, pRow[rrc2.ColumnName].ToString());
                        }
                        else
                        {
                            // RD 20150911 [21327] Récupérer d'abord la valeur du Décimal avant de la transformer en string
                            // Pour les petit décimaux (en dessous de 0.0001):
                            // le fait de convertir directement le contenu de la colonne en String, produit un décimal en notation scientifique
                            // (Exemple: 0.00001 donne 1E-05)
                            // ce qui provoque une erreur lors du formatage
                            decimal decValue = Convert.ToDecimal(colValue);
                            ret = decValue.ToString();
                            ret = StrFunc.FmtDecimalToInvariantCulture(ret);

                            // 20090925 RD / Pour afficher les séparateurs de milliers 
                            int decimalDigits = -1;
                            Nullable<int> scale = GetScale(pContext, pIndex, pRow);
                            if (scale.HasValue)// FI 20170223 [22883] Modify
                            {
                                decimalDigits = scale.Value;
                            }
                            else if (IsColumnQty(pContext, pIndex)) // FI 20191023 [XXXXX] cas particulier sur les colonnes QTY
                            {
                                // FI 20191023 [XXXXX] sur les colonnes QTY s'il n'y a pas de précision de spécifiée, Spheres affiche toutes les decimales
                                decimalDigits = DecFunc.PrecisionOf(decValue);
                            }

                            if (decimalDigits > -1)
                                ret = StrFunc.FmtAmountToGUI(ret, decimalDigits);
                            else
                                ret = StrFunc.FmtAmountToGUI(ret);

                        }
                    }
                    else if (TypeData.IsTypeString(dataType) && ((ReferentialsReferentialColumnResource)pContext.alColumnIsResource[pIndex]).IsResource)
                    {

                        ReferentialsReferentialColumnResource rrcR = (ReferentialsReferentialColumnResource)pContext.alColumnIsResource[pIndex];
                        string resource = ret;
                        if (StrFunc.IsFilled(resource) && rrcR.prefixSpecified && StrFunc.IsFilled(rrcR.prefix))
                        {
                            // FI 20190527 [XXXXX] le prefix est séparé de la ressource pas un "_"
                            resource = rrcR.prefix + "_" + resource;
                        }

                        if (!rrcR.sqltableSpecified)
                        {
                            //PL 20120412 Replace GetStringByRef by GetMultiByRef
                            //bool existResource = Ressource.GetStringByRef(resource, ref resource);
                            if (Ressource.GetMultiByRef(resource, 1, ref resource))
                            {
                                ret = resource;
                            }
                            else if (columnName.IndexOf("MATFMT_") >= 0 || (rrcDataType.datakindSpecified && rrcDataType.datakind == Cst.DataKind.ETDMaturity))
                            {
                                // FI 20140926 [XXXXX] call GetFormattedMaturity method
                                ret = GetFormattedMaturity(columnName, ret, rrcDataType, pContext.fmtDisplayCol);
                            }
                        }
                        else if (StrFunc.IsFilled(resource))
                        {
                            //PL 20120518 New feature
                            //FI 20200108 ATTENTION, la méthode GetFormatedData peut est appelée dans un tâche asynchrone (thread séparé)
                            //Si tel était le cas la variable SessionTools.CS retourne empty produisant une exception
                            //Ce code est malgré tout conservé car les seuls référentiels contenant rrcR.sqltable renseigné sont ajourd'hui IOPARAM et IOPARAMDET. 
                            //Ce sont des référentiel avec très peu d'enregistements et qui par conséquent ne nécessite pas d'appel à des tâches asynchrone
                            if (OTCmlHelper.GetRessource(pContext.CS, rrcR.sqltable, resource, ref resource))
                                ret += " [Res: " + resource + "]";
                        }
                    }
                    else if (TypeData.IsTypeImage(dataType))
                    {
                        ret = "Image";
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20190403 Les colonnes EXTLLINK, EXTLLINK2, EXTLATTRB peuvent être cutomisées et changer de type
                // Exemple EXTLLINK peut devenir integer 
                // Cela evite de planter lorsqu'il existe des données non numériques ( l'acteur EFS a pour  EXTLLINK http://www.euro-finance-systems.fr)
                if (pContext.referential.ExistsColumnEXTL)
                {
                    if (pIndex == pContext.referential.IndexColSQL_EXTLLINK || pIndex == pContext.referential.IndexColSQL_EXTLLINK2 || pIndex == pContext.referential.IndexColSQL_EXTLATTRB)
                        ret = colValue.ToString(); // Rq si colValue == Convert.DBNull  alors ret = String.Empty
                }
                else
                {
                    throw SpheresExceptionParser.GetSpheresException(
                        StrFunc.AppendFormat("error when formatting Data:{0} [column {1}]", ret, pContext.alColumnName[pIndex]), ex);
                }
            }
            return ret;
        }

        /// <summary>
        ///  Remplace le contenu d'une colonne ENVIRONMENT (colonne Virtuelle)
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pCell"></param>
        /// FI 20180219 [XXXXX]
        private void FinalyzeEnvironmentColumn(DataGridItem pItem, TableCell pCell)
        {
            DataRow row = ((DataRowView)(pItem.DataItem)).Row;
            string text;
            if (IsTemplateColumn)
            {
                if (pCell.HasControls() && pCell.Controls[0].GetType().Equals(typeof(Label)))
                    text = ((Label)pCell.Controls[0]).Text;
                else
                    throw new NotSupportedException("pCell.Controls[0] is not a label");
            }
            else
                text = pCell.Text;

            Regex regex = new Regex(@"(\w+\.: !?|G\.\w+\.: !?|,\w+)");
            if (regex.IsMatch(text))
            {
                if (IsTemplateColumn)
                    pCell.Controls.RemoveAt(0);

                MatchCollection matchcol = regex.Matches(text);
                for (int k = 0; k < matchcol.Count; k++)
                {
                    bool ishyperlink = true;
                    WebControl control;
                    int indexcol;
                    switch (matchcol[k].Value.Replace(": !", string.Empty).Replace(": ", string.Empty))
                    {
                        case "Derv.":
                        case "Commo.":
                        case "G.Ct.":
                        case "Ct.":
                        case "G.Mkt.":
                        case "Mkt.":
                            if (matchcol[k].Value.EndsWith(": !"))
                                control = GetHyperLinkByColumnForPair("IDCONTRACTEXCEPT/TYPECONTRACTEXCEPT", pItem);
                            else
                                control = GetHyperLinkByColumnForPair("IDCONTRACT/TYPECONTRACT", pItem);
                            break;
                        case "Prd.":
                        case "G.Instr.":
                        case "Instr.":
                            control = GetHyperLinkByColumnForPair("IDINSTR/TYPEINSTR", pItem);
                            break;
                        case "G.Prd.":
                            indexcol = Referential.GetIndexColSQL("GPRODUCT");
                            control = new Label() { Text = row[indexcol].ToString() };
                            break;
                        case "G.Pty.":
                        case "Pty.":
                        case "G.Bk.":
                        case "Bk.":
                            control = GetHyperLinkByColumnForPair("IDPARTY/TYPEPARTY", pItem);
                            break;
                        case "G.PtyA.":
                        case "PtyA.":
                        case "G.BkA.":
                        case "BkA.":
                            indexcol = Referential.GetIndexColSQL("TYPEPARTYA");
                            if (indexcol == -1)
                                indexcol = Referential.GetIndexColSQL("FEETYPEPARTYA");

                            if (row[indexcol].ToString() == "All")
                                control = new Label() { Text = row[indexcol].ToString() };
                            else
                                control = GetHyperLinkByColumnForPair("IDPARTYA/TYPEPARTYA", pItem);
                            break;
                        case "G.PtyB.":
                        case "PtyB.":
                        case "G.BkB.":
                        case "BkB.":
                            indexcol = Referential.GetIndexColSQL("TYPEPARTYB");
                            if (indexcol == -1)
                                indexcol = Referential.GetIndexColSQL("FEETYPEPARTYB");

                            if (row[indexcol].ToString() == "All")
                                control = new Label() { Text = row[indexcol].ToString() };
                            else
                                control = GetHyperLinkByColumnForPair("IDPARTYB/TYPEPARTYB", pItem);
                            break;
                        case "G.OPty1.":
                        case "OPty1.":
                            control = GetHyperLinkByColumnForPair("IDOTHERPARTY1/TYPEOTHERPARTY1", pItem);
                            break;
                        case "G.OPty2.":
                        case "OPty2.":
                            control = GetHyperLinkByColumnForPair("IDOTHERPARTY2/TYPEOTHERPARTY2", pItem);
                            break;
                        default:
                            ishyperlink = false;
                            control = new Label() { Text = matchcol[k].Value };
                            break;
                    }
                    if (null != control)
                    {
                        if (ishyperlink)
                        {
                            if (pCell.Controls.Count != 0)
                                pCell.Controls.Add(new LiteralControl(Cst.HTMLSpace));
                            pCell.Controls.Add(new Label() { Text = matchcol[k].Value });
                        }
                        pCell.Controls.Add(control);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pCell"></param>
        /// FI 20180219 [XXXXX]
        private void FinalizeURLColumn(DataGridItem pItem, int index)
        {
            HyperLink hl = new HyperLink
            {
                Target = Cst.HyperLinkTargetEnum._blank.ToString(),
                CssClass = "linkDatagrid",
                Text = GetCellValue(pItem, index)
            };

            if (IsTemplateColumn)
                pItem.Cells[index].Controls.RemoveAt(0);

            hl.NavigateUrl = hl.Text;

            pItem.Cells[index].Controls.Add(hl);

        }

        /// <summary>
        /// Chgt des données autocomplete du contrôle txtSearch
        /// </summary>
        /// FI 20191210 [XXXXX] Add
        /// FI 20191216 [XXXXX] Mode Asynchrone

        private void LoadTxtSearchAutocomplete()
        {
            if (BoolFunc.IsTrue(SystemSettings.GetAppSettings("txtSearchAutoComplete", "true")))
            {
                if ((IsDataAvailable) && (false == IsSelfClear))
                {
                    int lmaxRows = Convert.ToInt32(SystemSettings.GetAppSettings("txtSearchAutoCompleteMaxRows", "20000"));
                    int nbRows = DsData.Tables[0].Rows.Count;
                    if (nbRows <= lmaxRows)
                    {
                        // Plusieurs Tâches peunent explorer le jeu de donnée
                        // Chaque Tâche Traite 1000 enregistrements max;
                        const int nbRowByTask = 1000;

                        AutoCompleteKey autoCompleteKey = new AutoCompleteKey()
                        {
                            pageGuId = ((PageBase)Page).GUID,
                            controlId = "txtSearch"
                        };

                        DataTable dtWork = DsData.Tables[0].Copy();
                        // FI 20200224 [XXXXX] searchColumns est de type List<GridColumn> puisque utilisée dans une tâche asynchrone
                        List<GridColumn> searchColumns = GetColumnsSearch(dtWork, string.Empty, true).ToList();

                        /*
                        // FI 20200224 [XXXXX] Mis en commentaire puisque remplacé par context
                        // FI 20200106 [XXXXX] chgt des formats d'affichage de la session
                        NameValueCollection fmtDisplayCol = SessionTools.FmtDisplayCollection();
                        */

                        // FI 20200224 [XXXXX] Add context
                        ContextFormat context = GetCurrentContextFormat();

                        Boolean isModeAsync = (nbRows > nbRowByTask);
                        if (false == isModeAsync)
                        {
                            AutoCompleteDataCache.SetData(autoCompleteKey, LoadAutocomplete(context, dtWork, searchColumns, null, null));
                        }
                        else
                        {
                            DataTable lastDataTable = DsData.Tables[0].Copy();
                            int nbTask = Convert.ToInt32(Math.Truncate(nbRows / Convert.ToDecimal(nbRowByTask))) + 1;

                            List<Task> tasks = new List<Task>();
                            for (int i = 0; i < nbTask; i++)
                            {
                                int indexStart = i * nbRowByTask;
                                tasks.Add(
                                     Task.Run(() => LoadAutocomplete(context, lastDataTable, searchColumns, indexStart, nbRowByTask))
                                    );
                            }

                            bool isOk = true;
                            try
                            {
                                int lTimeOut = -1;
                                if (SystemSettings.GetAppSettings("txtSearchAutoCompleteTimeOut", "-1") != "-1")
                                    lTimeOut = Convert.ToInt32(SystemSettings.GetAppSettings("txtSearchAutoCompleteTimeOut", "-1")) * 1000;
                                isOk = Task.WaitAll(tasks.ToArray(), lTimeOut);
                            }
                            catch (AggregateException ae)
                            {
                                throw ae.Flatten();
                            }
                            catch (Exception)
                            {
                                throw;
                            }

                            if (isOk)
                            {
                                List<String> result = new List<string>();

                                foreach (Task<List<string>> taskItem in tasks)
                                    result.AddRange(taskItem.Result);

                                AutoCompleteDataCache.SetData(autoCompleteKey, result);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retourne les données autocomplete du contrôle TxtSearch (Filtre libre)
        /// <para>Possibilité de sélectionner une partie du jeu de résulat via pIndex et NbRows</para>
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pDataTable">Jeux de résultat du datagrid</param>
        /// <param name="pSelectedColumns">Liste des colonnes considérées par l'application du filtre libre</param>
        /// <param name="pIndex"></param>
        /// <param name="pNbRow"></param>
        /// <returns></returns>
        /// FI 20200224 [XXXXX] Method Static, Add pContext
        private static List<string> LoadAutocomplete(ContextFormat pContext,
            DataTable pDataTable, List<GridColumn> pSelectedColumns, Nullable<int> pIndex, Nullable<int> pNbRow)
        {
            List<string> ret = new List<string>();
            DataTable dt = pDataTable;

            Boolean columnPositionInDataGridSpecified = pContext.referential.Column.Where(x => x.ColumnPositionInDataGridSpecified).Count() > 0;

            int indexStart = 0;
            int indexEnd = dt.Rows.Count - 1;
            if ((pIndex.HasValue) && (pNbRow.HasValue))
            {
                indexStart = pIndex.Value;
                indexEnd = Math.Min(indexStart + pNbRow.Value, dt.Rows.Count - 1);
            }

            for (int i = indexStart; i <= indexEnd; i++)
            {
                DataRow row = dt.Rows[i];
                foreach (GridColumn item in pSelectedColumns)
                {
                    // Rq si columnPositionInDataGridSpecified alors l'ordre des colonnes dans le grid n'est pas le même que l'ordre des colonnes  dans le jeu de résultat
                    // => Spheres ne fait pas appel à GetFormatedData
                    string sValue = string.Empty;
                    if (item.indexCol > -1 && (false == columnPositionInDataGridSpecified))
                        sValue = GetFormatedData(pContext, item.indexCol, row);
                    else
                        sValue = row[item.column.ColumnName].ToString();
                    
                    // FI 20201230 [XXXXX] bold tags removed
                    ret.Add(HtmlTools.HTMLBold_Remove(sValue));
                }
            }
            return ret;
        }


        /// <summary>
        /// Retourne la liste des rows qui matchent avec le filtre {pFilterValue}
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pDataTable"></param>
        /// <param name="pColumns">liste des colonnes considérées</param>
        /// <param name="pMode">Affiche ou Masque les données qui matchent avec le filtre</param>
        /// <param name="pFilterValue">Filtre</param>
        /// <param name="pIndex"></param>
        /// <param name="pNbRow"></param>
        /// <returns></returns>
        /// FI 20200224 [XXXXX] Add pContext
        private static List<DataRow> LoadRowsFilter(ContextFormat pContext, DataTable pDataTable, List<GridColumn> pColumns,
            Cst.DisplayMask pMode, string pFilterValue,
            Nullable<int> pIndex, Nullable<int> pNbRow)
        {
            // FI 20111124 [XXXXX] Gestion des mots clé {##E}, {##M}, {##C}   
            // FI 20170116 [21916] Possibilité de faire une recherche exacte si présence de " (Ex:"toto") 
            if (pFilterValue.StartsWith("\"") && pFilterValue.EndsWith("\""))
            {
                //Recherche Exacte 
                pFilterValue = "{##E}" + pFilterValue.Replace("\"", string.Empty);
            }
            else
            {
                // FI 20211118 [XXXX] Gestion des caractères génériques ? et *
                if (pFilterValue.Contains("?") || pFilterValue.Contains("*"))
                {
                    //^ $ [ ] . ( ) * + ? | { } \ caractère speciaux
                    string filter = pFilterValue.Replace("^", @"\^").Replace("$", @"\$").Replace("[",@"\[").Replace("]", @"\]")
                        .Replace(".", @"\.").Replace("+", @"\+").Replace("|", @"\|").Replace("|", @"\|")
                        .Replace("{", @"\{").Replace("}", @"\}");
                    
                    if (filter.StartsWith("*"))
                    {
                        filter = filter.Remove(0, 1) + "$";
                    }
                    if (filter.EndsWith("*") && !(filter.EndsWith(@"\*")))
                    {
                        filter = "^" + filter.Remove(filter.Length - 1, 1);
                    }
                    if (filter.Contains("?"))
                    {
                        filter = filter.Replace(@"\?", "{##ESCAPE##}");
                        filter = filter.Replace("?", ".");
                        filter = filter.Replace(@"{##ESCAPE##}", @"\?");
                    }
                    if (filter.Contains("*"))
                    {
                        filter = filter.Replace(@"\*", "{##ESCAPE##}");
                        filter = filter.Replace("*", ".+");
                        filter = filter.Replace(@"{##ESCAPE##}", @"\*");
                    }
                    //Recherche de type Match
                    pFilterValue = "{##M}" + filter;
                }
                else
                {
                    //Recherche de type contains sans distinction de case
                    pFilterValue = "{##C}" + pFilterValue;
                }
            }

            List<DataRow> ret = new List<DataRow>();

            DataTable dt = pDataTable;

            int indexStart = 0;
            int indexEnd = dt.Rows.Count - 1;
            if ((pIndex.HasValue) && (pNbRow.HasValue))
            {
                indexStart = pIndex.Value;
                indexEnd = Math.Min(indexStart + pNbRow.Value -1 , dt.Rows.Count - 1);
            }

            for (int i = indexStart; i <= indexEnd; i++)
            {
                DataRow row = dt.Rows[i];
                Boolean isAdd = pMode != Cst.DisplayMask.Display;
                foreach (var item in pColumns)
                {
                    if (IsDataMatch(pContext, item.indexCol, item.column, row, pFilterValue))
                    {
                        switch (pMode)
                        {
                            case Cst.DisplayMask.Display:
                                isAdd = true;
                                break;
                            case Cst.DisplayMask.Mask:
                                isAdd = false;
                                break;
                        }
                        break;
                    }
                }
                if (isAdd)
                    ret.Add(row);
            }
            return ret;
        }

        /// <summary>
        /// Retourne les colonnes considérées par le filtre libre
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pParticularColumn">doit être renseigné lors de la recherche de données du grid qui matchent avec le filtre libre (lorsque la recherche s'applique uniquement sur 1 colonne)</param>
        /// <param name="pIsForLoadAutoComplete">
        /// <para>
        /// true, s'il s'agit de prendre en considération les colonnes lors du chgt des données autocomplete associé au filtre libre
        /// </para>
        /// <para>
        /// false, s'il s'agit de prendre en considération les colonnes lors de la recherche de données dans le grid qui matchent avec le filtre libre
        /// </para>
        /// </param>
        /// <returns></returns>
        /// FI 220191218 [XXXXX] Add
        private IEnumerable<GridColumn> GetColumnsSearch(DataTable dt, String pParticularColumn, Boolean pIsForLoadAutoComplete)
        {
            DataRow[] rows = dt.Rows.Cast<DataRow>().ToArray();

            IEnumerable<GridColumn> columns = from item in dt.Columns.Cast<DataColumn>()
                                              select new GridColumn
                                              {
                                                  indexCol = Referential.GetIndexColSQL(item.ColumnName),
                                                  column = item
                                              };

            string[] SQLColumnName = (from item in Referential.Column.Where(x => x.IsHideInDataGrid == false)
                                      select ArrFunc.IsFilled(item.Relation) && (null != item.Relation[0]) && StrFunc.IsFilled(item.Relation[0].RelationColumnSQLName) ?
                                      item.Relation[0].RelationColumnSQLName.ToUpper() : item.DataField.ToUpper()).ToArray();


#if DEBUG
            //FI 20190205 Restriction aux colonnes affichées
            string[] colNotFound = (from item in columns.Where(x => ArrFunc.ExistInArrayString(SQLColumnName, x.column.ColumnName)).Where(x => x.indexCol == -1)
                                    select item.column.ColumnName).ToArray();
            if (ArrFunc.IsFilled(colNotFound))
            {
                if (SessionTools.ClientMachineName == "DWS-138")
                    throw new Exception(StrFunc.AppendFormat("  Warning! Referential.TableName {0}: => pColumnName (id:{1}) not found", Referential.TableName, ArrFunc.GetStringList(colNotFound)));
                else
                    Debug.Print(StrFunc.AppendFormat("  Warning! Referential.TableName {0}: => pColumnName (id:{1}) not found", Referential.TableName, ArrFunc.GetStringList(colNotFound)));
            }
#endif 


            IEnumerable<GridColumn> ret = from item in columns.Where(x => ArrFunc.ExistInArrayString(SQLColumnName, x.column.ColumnName))
                                                .Where(x => IsColumnMatch(x.column, pParticularColumn, pIsForLoadAutoComplete))
                                          select item;

            return ret;
        }

        /// <summary>
        ///  Initialisation de isLoadData
        ///  <para>Si (false == isLoadData) alors Alimentation d'un jeu de donnée vierge</para>
        /// </summary>
        private void InitFlagIsLoadData()
        {
            if (!Page.IsPostBack)
            {
                IsLoadData = IsLoadDataOnStart;
            }
            else // si Postback
            {
                if (IsCheckUncheckAllRows)
                {
                    IsLoadData = false;
                }
                else
                {
                    IsLoadData = StrFunc.IsEmpty(DataFrom__EVENTARGUMENT) || IsSelfReload;
                }
            }
        }

        /// <summary>
        ///  Alimentation d'un jeu de donnée vierge si nécessaire
        /// </summary>
        private void InitDataset()
        {
            Boolean isResetDataSet;

            if (!Page.IsPostBack)
                isResetDataSet = (false == IsLoadData);
            else
                isResetDataSet = IsSelfClear;

            if (isResetDataSet)
            {
                //Alimentation d'un jeu de donnée vierge afin d'afficher un DataGrid vide
                DsData = new DataSet();
                DsData.Tables.Add();
            }
        }

        /// <summary>
        /// Context associé au formatage des données dans le grid
        /// </summary>
        /// <returns></returns>
        /// FI 20200224 [XXXXX] Add Class
        /// FI 20200720 [XXXXX] Add Collaborator
        private ContextFormat GetCurrentContextFormat()
        {
            return new ContextFormat
            {
                /* Variable SESSION */
                CS = SessionTools.CS,
                fmtDisplayCol = SessionTools.FmtDisplayCollection(),
                referential = Referential,
                lstConsult = LocalLstConsult,
                Collaborator = SessionTools.Collaborator,

                alColumnName = alColumnName,
                alColumnDataType = alColumnDataType,
                alColumnDataTypeLongForm = alColumnDataTypeLongForm,
                alColumnDataScale = alColumnDataScale,
                alColumnIsResource = alColumnIsResource,
                currencyInfo = _currencyInfos
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Cst.ListType GetListType()
        {
            Cst.ListType listeTypeEnum = Cst.ListType.Repository;
            if (Enum.IsDefined(typeof(Cst.ListType), Title))
                listeTypeEnum = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), Title);

            return listeTypeEnum;
        }

        /// <summary>
        ///  Alimentation de la property IsLoadDataOnStart
        /// </summary>
        /// <param name="rr"></param>
        /// <param name="isNewTemporaryTemplate"></param>
        private void SetIsLoadDataOnStart(ReferentialsReferential rr, Boolean isNewTemporaryTemplate)
        {
            if (false == Page.IsPostBack)
            {
                bool isLoadDataOnStart;
                if (StrFunc.IsFilled(NVC_QueryString["isLoadData"]))
                {
                    isLoadDataOnStart = BoolFunc.IsTrue(NVC_QueryString["isLoadData"]);
                }
                else if (isNewTemporaryTemplate)
                {
                    isLoadDataOnStart = true;
                }
                else if (LocalLstConsult.template.IDLSTTEMPLATE == LstConsult.LstTemplate.TEMPORARYPREFIX + ReferentialWeb.GetNewQueryName())
                {
                    // FI 20201211 [XXXXX] Par défaut les template temporaires sont enregistrés avec ISLOADONSTART = false
                    // si 1er ouverture d'un menu pour lequel il n'existe pas template Spheres® génère un nouveau template dont le nom est *Nouveau 
                    // Dansd ce cas, il y a lecture de la directive LoadOnStart (présente dans le fichier XML ou dans LSTCONSULT.CONSULTXML) si elle est renseignée
                    // Si aucune directive => Application d'un comportement par défaut
                    if (false == rr.LoadOnStartSpecified)
                    {
                        Boolean isLoadOnStart = true; //Defaut

                        Cst.ListType listeTypeEnum = GetListType();
                        if ((listeTypeEnum == Cst.ListType.ProcessBase) || (listeTypeEnum == Cst.ListType.Price) || (listeTypeEnum == Cst.ListType.Log)
                            || (listeTypeEnum == Cst.ListType.Accounting) || (listeTypeEnum == Cst.ListType.Invoicing) || (listeTypeEnum == Cst.ListType.Report)
                            || (listeTypeEnum == Cst.ListType.ConfirmationMsg) || (listeTypeEnum == Cst.ListType.SettlementMsg) || (listeTypeEnum == Cst.ListType.Consultation))
                            isLoadOnStart = false;

                        isLoadDataOnStart = isLoadOnStart;
                    }
                    else
                    {
                        isLoadDataOnStart = rr.LoadOnStart;
                    }
                }
                else
                {
                    isLoadDataOnStart = LocalLstConsult.template.ISLOADONSTART;
                }

                IsLoadDataOnStart = isLoadDataOnStart;

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20191218 [XXXXX] Add
        public class GridColumn
        {
            /// <summary>
            /// 
            /// </summary>
            public int indexCol;
            /// <summary>
            /// 
            /// </summary>
            public DataColumn column;
        }

        /// <summary>
        /// Context Session. Représente les données stockées dans la session Web 
        /// </summary>
        /// FI 20200224 [XXXXX] Add Class
        internal class ContextSession
        {
            internal string CS;
            internal ReferentialsReferential referential;
            internal LstConsult lstConsult;
            internal NameValueCollection fmtDisplayCol;
            // FI 20200720 [XXXXX] Add Collaborator
            internal Collaborator Collaborator;
        }

        /// <summary>
        /// Context associé au formatage des données dans le grid
        /// </summary>
        /// FI 20200224 [XXXXX] Add Class
        internal class ContextFormat : ContextSession
        {
            internal ArrayList alColumnName;
            internal ArrayList alColumnDataType;
            internal ArrayList alColumnDataTypeLongForm;
            internal ArrayList alColumnDataScale;
            internal ArrayList alColumnIsResource;
            internal Hashtable currencyInfo;
        }
    }
}
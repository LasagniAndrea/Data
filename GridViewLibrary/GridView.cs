#region using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Referentiel;
using EFS.Status; 
using EFS.GridViewProcessor;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
#endregion using directives

namespace EFS.Controls
{
    public enum GridViewColumnType
    {
        column,
        relation,
    }

    public enum RessourceTypeEnum
    {
        Empty,
        Denominator,
        Fill,
    }
    /// <summary>
    /// Remplace les variables alxxx de type ArrayList
    /// </summary>
    public class GridViewColumn
    {
        #region Members
        public GridViewColumnType columnType;
        public string columnName;
        public int columnId;
        public TypeData.TypeDataEnum dataType;
        public bool scaleSpecified;
        public int scale;

        public bool resourceSpecified;
        public ReferentialColumnResource resource;
        public RessourceTypeEnum ressourceType;

        public bool isTRIM;
        public bool isRTF;
        public bool rowStyleSpecified;
        public ReferentialColumnStyleBase rowStyle;
        public bool cellStyleSpecified;
        public ReferentialColumnStyleBase cellStyle;
        public bool aggregateSpecified;
        public string aggregate;
        public bool isMandatory;
        public bool isEditable;
        public bool isHideInDatagrid;
        public bool isDataKeyField;
        public int lengthInGridView;
        public string defaultValue;

        public bool isExistHyperlink;

        public string align;

        #endregion Members
        #region Accessors
        public bool IsScaleQuantity
        {
            get
            {
                return (TypeData.IsTypeDec(dataType) && 
                    (columnName.Contains("QTY") || columnName.Contains("QUANTITY")));
            }
        }
        public HorizontalAlign HorizontalAlign
        {
            get
            {
                HorizontalAlign hAlign = HorizontalAlign.NotSet;
                if (TypeData.IsTypeDate(dataType))
                {
                    hAlign = HorizontalAlign.Center;
                }
                else if (TypeData.IsTypeDateTime(dataType))
                {
                    hAlign = HorizontalAlign.Center;
                }
                else if (TypeData.IsTypeTime(dataType))
                {
                    hAlign = HorizontalAlign.Center;
                }
                // Si le champ est invisible on ne le formatte pas
                else if (TypeData.IsTypeInt(dataType) && (false == isHideInDatagrid))
                {
                    hAlign = HorizontalAlign.Right;
                }
                else if (TypeData.IsTypeDec(dataType))
                {
                    hAlign = HorizontalAlign.Right;
                }
                else if (TypeData.IsTypeString(dataType))
                {
                    hAlign = HorizontalAlign.Left;
                    if (StrFunc.IsFilled(align))
                    {
                        if (align.ToLower() == "right")
                            hAlign = HorizontalAlign.Right;
                        else if (align.ToLower() == "center")
                            hAlign = HorizontalAlign.Center;
                    }
                }

                return hAlign;
            }
        }
        public string DataFormatString
        {
            get
            {
                string dataFormat = string.Empty;
                if (TypeData.IsTypeDate(dataType))
                {
                    dataFormat = "{0:d}";
                }
                else if (TypeData.IsTypeDateTime(dataType))
                {
                    dataFormat = "{0:G}";
                }
                else if (TypeData.IsTypeTime(dataType))
                {
                    dataFormat = "{0:T}";
                }
                // Si le champ est invisible on ne le formatte pas
                else if (TypeData.IsTypeInt(dataType) && (false == isHideInDatagrid) && (false == isDataKeyField))
                {
                    dataFormat = "{0:n0}";
                }
                else if (TypeData.IsTypeDec(dataType))
                {
                    dataFormat = "{0:" + (scaleSpecified ? "N" + scale.ToString() : "n") + "}";
                }
                return dataFormat;
            }
        }
        #endregion Accessors
        #region Constructors
        public GridViewColumn(string pTableName, GridViewColumnType pColumnType, int pColumnId, ReferentialColumn pReferentialColumn)
        {
            columnType = pColumnType;
            columnId = pColumnId;

            switch (columnType)
            {
                case GridViewColumnType.column:
                    columnName = pReferentialColumn.IDForItemTemplate;
                    dataType = TypeData.GetTypeDataEnum(pReferentialColumn.DataType.value);
                    break;
                case GridViewColumnType.relation:
                    columnName = pReferentialColumn.IDForItemTemplateRelation;
                    dataType = TypeData.GetTypeDataEnum(pReferentialColumn.Relation[0].ColumnSelect[0].DataType);
                    break;
            }
            scaleSpecified = pReferentialColumn.ScaleSpecified;
            if (scaleSpecified)
                scale = pReferentialColumn.Scale;
            TypeData.IsTypeBool(pReferentialColumn.DataType.value);

            resourceSpecified = pReferentialColumn.IsResourceSpecified && pReferentialColumn.IsResource.IsResource;
            if (resourceSpecified)
                resource = pReferentialColumn.IsResource;
            else
                resource = new ReferentialColumnResource(false);

            isTRIM = pReferentialColumn.IsTRIM;
            isRTF = pReferentialColumn.IsRTF;

            if (StrFunc.IsEmpty(pReferentialColumn.Ressource))
                ressourceType = RessourceTypeEnum.Empty;
            else if (pReferentialColumn.Ressource == @"/")
                ressourceType = RessourceTypeEnum.Denominator;
            else
                ressourceType = RessourceTypeEnum.Fill;

            rowStyleSpecified = pReferentialColumn.RowStyleSpecified;
            if (rowStyleSpecified)
                rowStyle = pReferentialColumn.RowStyle;

            cellStyleSpecified = pReferentialColumn.CellStyleSpecified;
            if (cellStyleSpecified)
                cellStyle = pReferentialColumn.CellStyle;

            aggregateSpecified = pReferentialColumn.GroupBySpecified && pReferentialColumn.GroupBy.AggregateSpecified;
            if (aggregateSpecified)
                aggregate = pReferentialColumn.GroupBy.Aggregate;

            isMandatory = pReferentialColumn.IsMandatory;
            isHideInDatagrid = pReferentialColumn.IsHideInDataGrid;
            isDataKeyField = pReferentialColumn.IsDataKeyField;

            lengthInGridView = pReferentialColumn.LengthInDataGrid;
            defaultValue = pReferentialColumn.GetStringDefaultValue(pTableName);
            isEditable = pReferentialColumn.IsEditable;
            if (pReferentialColumn.AlignSpecified)
                align = pReferentialColumn.Align;

            isExistHyperlink = RepositoryTools.IsHyperLinkAvailable(pReferentialColumn);
        }
        #endregion Constructors
    }
    /// <summary>
    /// Description résumée de GridViewTemplate.
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public partial class GridViewTemplate : GridView, IPageableItemContainer, IPostBackEventHandler 
    {
       
        /// <summary>
        /// Evènement qui se déclenche lorsqu'une erreur se produit durant le chargement du jeu de données
        /// </summary>
        public event GridViewDataErrorEventHandler LoadDataError;

        #region Members
        #region alException
        /// <summary>
        /// Liste des erreurs rencontrées (lorsque Spheres® est en mode sans Echec [mode par défaut])
        /// </summary>
        private List<SpheresException> _alException;
        /// <summary>
        /// Obtient la liste des erreurs rencontrées lorsque le datagrid est en mode sans Echec 
        /// </summary>
        public List<SpheresException> alException
        {
            get { return _alException; }
        }
        #endregion

        private NameValueCollection m_NVC_QueryString;
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
        public Pair<string, TimeSpan>[] lastPreSelectCommand
        {
            get { return _lastPreSelectCommand; }
        }
        #endregion

        #region lastQuery
        /// <summary>
        /// stocke la dernière query exécutée pour charger le gridView
        /// </summary>
        private Pair<string, TimeSpan> _lastQuery;

        /// <summary>
        /// Obtient la dernière Query exécutée pour charger la source de donnée du gridView
        /// </summary>
        public Pair<string, TimeSpan> lastQuery
        {
            get { return _lastQuery; }
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
        public string lastErrorQueryCount
        {
            get { return _lastErrorQueryCount; }
        }

        #endregion

        #region isNoFailure
        /// <summary>
        /// true si le mode sans Echec est activé
        /// </summary>
        private bool _isNoFailure;
        /// <summary>
        /// Obtient ou définit le mode sans Echec
        /// </summary>
        public bool isNoFailure
        {
            get { return _isNoFailure; }
            set { _isNoFailure = value; }
        }
        #endregion isNoFailure

        public HtmlGenericControl FooterContainer { set; get; }


        /// <summary>
        /// Obtient ou définit un indicateur pour activer un rendu interprétable par excel
        /// </summary>
        public bool isModeExportExcel
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un drapeau qui indique que le chargement du jeu de données est à effectuer (exécution Requête SQL)
        /// </summary>
        public bool isLoadData
        {
            get;
            set;
        }




        /// <summary>
        /// Obtient ou définit un drapeau qui indique que le datagrid doit être lié avec la source de donnée
        /// </summary>
        public bool isBindData
        {
            get;
            set;
        }



        /// <summary>
        /// Obtient ou définit le type de pagination du datagrid
        /// <para>Lorsque pagingType est null, le type de pagination est déterminée automatiquement</para>
        /// <seealso cref="LoadData"/>
        /// </summary>
        public Nullable<PagingTypeEnum> pagingType
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

        #region isLoupeColumn
        /// <summary>
        /// Obtient ou définit la présence d'une colonne Loupe
        /// <para>Elle permet à l'utilisateur de consulter des documents (Confirmations par exemple)</para>
        /// </summary>
        private bool _isLoupeColumn;
        #endregion

        #region isApplyOptionalFilter
        private bool _isApplyOptionalFilter;
        /// <summary>
        /// Obtient ou définit un indicateur pour appliquer ou non les critères optionnels de la consultation en cours
        /// <para>Lorsque _isApplyOptionalFilter =false, Spheres update la template de consultation pour mettre ISENABLED à false </para>
        /// </summary>
        public bool isApplyOptionalFilter
        {
            get { return _isApplyOptionalFilter; }
            set { _isApplyOptionalFilter = value; }
        }
        #endregion

        #region positionFilterDisabled
        private int _positionFilterDisabled;
        /// <summary>
        /// Obtient ou définit la position d'un des critères optionnels à désactiver lors du chargemement du datagrid. 
        /// <para>- Si supérieur ou égal à 0, le critère correspondant sera désactivé (LSTWHERE.ISENABLED=0 where LSTWHERE.PSOITION=positionFilterDisabled)</para>
        /// <para>- Si inférieur à 0, aucun critère à désactiver (valeur par défaut: -1)</para>
        /// <para>NB: Ce membre est valorisé lors d'un clic opéré par l'utilisateur pour désactiver un critère, directement depuis la page ListViewer.aspx, sans passer par la fenêtre LstCriteria.aspx.</para>
        /// </summary>
        public int positionFilterDisabled
        {
            get { return _positionFilterDisabled; }
            set { _positionFilterDisabled = value; }
        }
        #endregion
        //
        #region inputMode

        public Cst.DataGridMode inputMode
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

        private int nbColumnAction;

        private int nbColumnGrid;
        private string _dgRessource;
        private Cst.ListType _listType;
        private string _foreignKeyField;

        public Referential referential;
        //
        private int indexKeyField;
        private int indexColSQL_KeyField;
        private string queryStringDA;
        private string columnFK;
        private string valueFK;
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
        //
        public LstConsultData consult
        {
            get;
            set;
        }
        public string IdLstTemplate
        {
            get;
            set;
        }

        public int IdA
        {
            get;
            set;
        }
        //
        public string sessionName_LstColumn;

        private List<GridViewColumn> lstGdvColumn;

        /// <summary>
        /// Indicateur de l'existence colonnes (du refrential) avec hyperlink 
        /// </summary>
        private bool isExistHyperLinkColumn;

        private Hashtable _currencyInfos;

        private const string HEADERWITHMULTICOLUMN = "?hideAndcolspan=1&value=";
        private string resAUTOMATIC_COMPUTE;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la collection des variables de requête HTTP
        /// </summary>
        private NameValueCollection NVC_QueryString
        {
            get
            {
                if (m_NVC_QueryString == null)
                    m_NVC_QueryString = Page.Request.QueryString;
                return m_NVC_QueryString;
            }
        }

        /// <summary>
        /// Obtient true s'il existe une varailble HTTP nommée Consuultation
        /// <para>Le grid est alors chargé depuis les tables LST</para>
        /// </summary>
        public bool isConsultation
        {
            get
            {
                return StrFunc.IsFilled(NVC_QueryString[Cst.ListType.Consultation.ToString()]);
            }
        }

        /// <summary>
        /// Obtient true si la source de donnée DsData du datagrid est renseignée
        /// <para>Le dataset existe et il contient 1 table</para>
        /// </summary>
        /// <returns></returns>
        public bool isDataAvailable
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
        /// <para>Le dataset existe et il contient 1 table, et les colonnes</para>
        /// </summary>
        /// <returns></returns>
        public bool isDataFilled
        {
            get
            {
                bool ret = false;
                if (isDataAvailable)
                    ret = ArrFunc.IsFilled(DsData.Tables[0].Columns);
                return ret;
            }
        }




        /// <summary>
        /// Obtient true si la source de donnée du datagrid est not null
        /// </summary>
        /// <returns></returns>
        public bool isDataSourceAvailable
        {
            get
            {
                return (null != DataSource);
            }
        }

        /// <summary>
        /// Obtient true si le dataGrid doit charger les données à la 1er ouverture de la page
        /// </summary>
        ///  RD 20110520 [17464]
        ///  Ne pas Tenir compte du paramétre isLoadData (LoadOnStart): 
        ///  - Dans le cas d'existence d'une FK
        ///     Exemple:
        ///      - Ouverture d'un sous menu depuis un formulaire (menu Détail\xxx)
        ///  - Dans le cas d'existence d'une ValueForFilter
        ///     Exemple:
        ///      - Ouverture d’un référentiel d’aide à la saisie (bouton « … »)
        ///  - Dans le cas où REFRESHINTERVAL est valorisé 
        /// FI 20140519 [19923] => public property
        public bool isLoadDataOnStart
        {
            get
            {
                bool ret = true;
                //
                if (StrFunc.IsFilled(NVC_QueryString["isLoadData"]))
                    ret = BoolFunc.IsTrue(NVC_QueryString["isLoadData"]);
                else
                    ret = BoolFunc.IsTrue(consult.template.ISLOADONSTART);
                //
                //Cas particulier
                //Si rafraîchissement périodique alors spheres® charge les données du datagrid à l'ouverture
                if (consult.template.IsRefreshIntervalSpecified)
                    ret = true;
                //
                //Cas particulier
                //Si referentiel enfant alors spheres® charge les données du datagrid à l'ouverture
                if (StrFunc.IsFilled(NVC_QueryString["FK"]))
                    ret = true;
                else
                {
                    // RD 20110719
                    //Cas particulier
                    //Si referentiel d’aide à la saisie (bouton « … ») et
                    //Si une données unique, ne contenat pas de «;» et pas de «{}» est saisie dans la zone correspondante
                    //alors spheres® charge les données du datagrid à l'ouverture,
                    //sinon les données du datagrid ne seronta pas chargées
                    if (ArrFunc.ExistInArrayString(NVC_QueryString, "ValueForFilter"))
                    {
                        string valueForFilter = NVC_QueryString["ValueForFilter"];
                        if (StrFunc.IsFilled(valueForFilter) &&
                            (false == valueForFilter.Contains(";")) &&
                            (false == valueForFilter.Contains("{")) &&
                            (false == valueForFilter.Contains("}")))
                        {
                            ret = true;
                        }
                        else
                            ret = false;
                    }
                }
                //
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si Spheres® répond à la demande de rafaîchissement du grid
        /// </summary>
        /// FI 20140519 [19923] => public property
        public bool isSelfReload
        {
            get
            {
                return StrFunc.IsFilled(dataFrom__EVENTARGUMENT) && (dataFrom__EVENTARGUMENT.ToUpper() == "SELFRELOAD_");
            }
        }

        public bool isReloadCriteria
        {
            get
            {
                //return StrFunc.IsFilled(dataFrom__EVENTTARGET) && dataFrom__EVENTTARGET.StartsWith(ContentPlaceHolder_UniqueID + "uc_lstdisplay");
                return StrFunc.IsFilled(dataFrom__EVENTTARGET) &&
                    (dataFrom__EVENTTARGET.EndsWith("btnOk") || dataFrom__EVENTTARGET.EndsWith("btnOkAndSave") || dataFrom__EVENTTARGET.EndsWith("btnCancel"));
            }
        }


        /// <summary>
        /// Obtient true si Spheres® répond à la demande de purge du grid
        /// </summary>
        /// FI 20140519 [19923] => public property
        public bool isSelfClear
        {
            get
            {
                return StrFunc.IsFilled(dataFrom__EVENTARGUMENT) && (dataFrom__EVENTARGUMENT.ToUpper() == "SELFCLEAR_");
            }
        }


        /// <summary>
        /// Obtient true lorsque le grid génère des links vers les référentiels (Links vers Acteur, Marché, etc...)
        /// </summary>
        private Boolean isHyperLinkAvailable
        {
            get
            {
                Boolean ret = false;
                if (false == isModeExportExcel) //pas de link avec Excel
                    ret = isExistHyperLinkColumn;
                return ret;
            }
        }


        /// <summary>
        /// Obtient true si le DblClick est autorisé sur chaque ligne du datagrid
        /// </summary>
        private bool isRowDblClickAvailable
        {
            get
            {
                bool ret = (false == isModeExportExcel);
                if (isConsultation)
                    ret = (0 <= referential.IndexColSQL_DataKeyField);
                else if (referential.isDblClickSpecified)
                    ret = referential.isDblClick;
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si les colonnes sont de type TemplateColumn (cad customizables)
        /// </summary>
        private bool isTemplateColumn
        {
            get
            {
                return (isGridInputMode || referential.IsDataGridWithTemplateColumn);
            }
        }

        #region inputMode
        /// <summary>
        /// Obtient true si le datagrid permet uniquement de consulter le jeu de donnée 
        /// <para>la selection d'une ligne s'effectue depuis le formulaire</para>
        /// </summary>
        public bool isFormSelectMode
        {
            get { return (inputMode == Cst.DataGridMode.FormSelect); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet uniquement de consulter le jeu de donnée 
        /// <para>la selection d'une ligne s'effectue depuis le datagrid (ie  aide en ligne de la saisie des trades)</para>
        /// </summary>
        public bool isGridSelectMode
        {
            get { return (inputMode == Cst.DataGridMode.GridSelect); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet de modifier le jeu de donnée via le formulaire de saisie (ie les référentiels)
        /// </summary>
        public bool isFormInputMode
        {
            get { return (inputMode == Cst.DataGridMode.FormInput); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet de modifier le jeu de donnée
        /// </summary>
        public bool isGridInputMode
        {
            get { return (inputMode == Cst.DataGridMode.GridInput); }
        }

        /// <summary>
        /// Obtient true si le datagrid permet uniquement de consulter le jeu de donnée via un formulaire
        /// </summary>
        public bool isFormViewerMode
        {
            get { return (inputMode == Cst.DataGridMode.FormViewer); }
        }

        /// <summary>
        /// Obtient true si le jeu de données est non modifiable (Mode selection) 
        /// </summary>
        public bool isSelectMode
        {
            get { return isGridSelectMode || isFormSelectMode || isFormViewerMode; }
        }
        /// <summary>
        /// Obtient true si le jeu de données est modifiable (soit via formulaire, soit directement via le datagrid)
        /// </summary>
        public bool isInputMode
        {
            get { return isGridInputMode || isFormInputMode; }
        }
        #endregion


        /// <summary>
        /// Retourne true lorsque le datagrid est en mode pagination customisée et que la détermination du nombre de ligne a généré une erreur
        /// <para>
        /// Dans ce contexte le datagrid n'affiche virtuellement 10 pages
        /// </para>
        /// </summary>
        public bool isVirtualItemCountError
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
            get { return _foreignKeyField; }
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
        public string Data_CacheName
        {
            get;
            set;
        }



        /// <summary>
        ///  Obtient ou définit le jeu de données
        ///  <remarks>Le jeu de résultat est stocké en variable Session, Obtient null si le session n'est pas morte</remarks>
        /// </summary>
        public DataSet DsData
        {
            get
            {
                DataSet ds = null;
                if (SessionTools.IsSessionAvailable)
                    ds = HttpSessionStateTools.Get(SessionTools.SessionState, Data_CacheName) as DataSet;
                return ds;
            }
            set
            {
                if (SessionTools.IsSessionAvailable)
                    HttpSessionStateTools.Set(SessionTools.SessionState, Data_CacheName, value);
            }
        }

        /// <summary>
        /// Variable session destinée à sauvegarder le jeu de donnée lorsqu'une recherche est effectuée (txtSearch) pour restituér les données lorsque txtSearch est vidé
        /// </summary>
        /// FI 20140926 [XXXXX] Add 
        private DataTable DataSav
        {
            get
            {
                DataTable dt = null;
                if (SessionTools.IsSessionAvailable)
                    dt = HttpSessionStateTools.Get(SessionTools.SessionState, Data_CacheName + "_dtSav") as DataTable;
                return dt;
            }
            set
            {
                if (SessionTools.IsSessionAvailable)
                    HttpSessionStateTools.Set(SessionTools.SessionState, Data_CacheName + "_dtSav", value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string sortExpression
        {
            get { return (string)ViewState[ClientID + "CurrentSortExpression"]; }
            set { ViewState[this.ClientID + "CurrentSortExpression"] = value; }
        }

        /// <summary>
        /// retourne true si le tri est définit par l'utilisateur
        /// <para>un tri utilisateur est activé par un click sur la ligne d'entête</para>
        /// </summary>
        protected bool isSortedByUser
        {
            get
            {
                return StrFunc.IsFilled(sortExpression);
            }
        }

        /// <summary>
        /// Obtient true, s'il existe ligne du datagrid à modifier
        /// </summary>
        public bool isLocked
        {
            get { return (this.EditIndex >= 0); }
        }

        /// <summary>
        /// Obteint ou définit un indicateur dès lors que le jeu de données a été modifié
        /// </summary>
        public bool isDataModified
        {
            //20061026 RD Referential Refactor			
            get
            {
                bool isModified = BoolFunc.IsTrue(HttpContext.Current.Session[Data_CacheName + "statusModified"]);
                return isModified;
            }
            set
            {
                HttpContext.Current.Session[Data_CacheName + "statusModified"] = value;
            }
        }

        /// <summary>
        /// Obtient le nombre de lignes du datagrid
        /// </summary>
        public int totalRowscount
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
                    if (isDataAvailable)
                        ret = DsData.Tables[0].Rows.Count;
                }
                //
                return ret;
            }
        }

        /// <summary>
        /// Obtient si la page courante est la 1er page
        /// </summary>
        protected bool isFirstPage
        {
            get
            {
                return PageIndex == 0;
            }
        }

        /// <summary>
        /// Obtient si la page courante est la dernière page
        /// </summary>
        protected bool isLastPage
        {
            get
            {
                return (PageIndex + 1 == PageCount);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string dataFrom__EVENTTARGET
        {
            get { return Page.Request.Params["__EVENTTARGET"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        private string dataFrom__EVENTARGUMENT
        {
            get { return Page.Request.Params["__EVENTARGUMENT"]; }
        }


        /// <summary>
        /// 
        /// </summary>
        private int PageIndexBeforeInsert
        {
            get
            {
                int retValue;
                try
                {
                    retValue = Convert.ToInt32(Page.Session["PageIndexBeforeInsert"].ToString());
                }
                catch
                {
                    retValue = 1;
                }
                return retValue;
            }
            set
            {
                Page.Session["PageIndexBeforeInsert"] = value.ToString();
            }
        }

        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public GridViewTemplate()
            : base()
        {
            resAUTOMATIC_COMPUTE = Ressource.GetString(Cst.AUTOMATIC_COMPUTE.ToString());

            indexKeyField = -1;
            indexColSQL_KeyField = -1;

            columnFK = string.Empty;
            valueFK = string.Empty;
            openerKeyId = string.Empty;
            openerSpecifiedId = string.Empty;
            openerSpecifiedSQLField = string.Empty;
            valueForFilter = string.Empty;
            typeKeyField = string.Empty;
            condApp= string.Empty;

            sessionName_LstColumn = string.Empty;
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
            sortExpression = strSortInfo;
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
        private string GetOrderSymbolImage()
        {
            // Retrieves the string with order information
            String strSortOrders = (String)ViewState[this.ClientID + "SortingOrders"];

            // Gets the webding corresponding to ASC/DESC for the first (primary) column
            // in the sort expression. The glyph is decided based on the order of the 
            // primary column in the sort expression.
            bool isDescending = strSortOrders.StartsWith("desc");
            return CSS.SetCssClass(isDescending ? CSS.Main.mnudown:CSS.Main.mnuup);
        }


        /// <summary>
        /// Retourne le nbr de decimale à appliquer aux colonnes qui représente une quantité
        /// <para>le nbr de decimale est fonction du contrat/asset</para>
        /// </summary>
        /// <returns></returns>
        /// EG 20170223 [22883] Add
        public static Nullable<int> GetQtyScale(Referential rr, DataRow dr)
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
                            SQL_CommodityContract sqlCommodityContract = new SQL_CommodityContract(CSTools.SetCacheOn(SessionTools.CS), contrat.First);
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
                    SQL_AssetBase sqlAsset = EfsML.AssetTools.NewSQLAsset(CSTools.SetCacheOn(SessionTools.CS), asset.Second, asset.First);
                    switch (asset.Second)
                    {
                        case Cst.UnderlyingAsset.Commodity:
                            SQL_AssetCommodityContract sql_AssetCommodity = new SQL_AssetCommodityContract(CSTools.SetCacheOn(SessionTools.CS), asset.First);
                            Boolean isOk = sql_AssetCommodity.LoadTable(new string[] { "IDCC", "SCALEQTY" });
                            if (isOk) // false est possible si asset commodity sans CommodityContract
                                ret = sql_AssetCommodity.CommodityContract_QtyScale;
                            break;
                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                            ret = 0;
                            break;
                        default:
                            ret = 0;
                            break;
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
        /// EG 20170223 [22883] Add
        private static Pair<int, Cst.UnderlyingAsset> GetContract(Referential rr, DataRow dr)
        {

            Pair<int, Cst.UnderlyingAsset> ret = null;

            /* Recherche du contrat à partir d'un link */
            ReferentialColumn colLinkContrat =
                (from item in
                     rr.Column.Where(x => x.ExistsHyperLinkColumn && (x.IsHyperLink.type == "IDDC" ||
                                                                      x.IsHyperLink.type == "IDCC" ||
                                                                      x.IsHyperLink.type == "IDCONTRACT"))
                 select item).DefaultIfEmpty(new ReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();

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

                ReferentialColumn colRelationContrat =
                (from item in
                     rr.Column.Where(x => ArrFunc.IsFilled(x.Relation) && (x.Relation[0].TableName == "DERIVATIVECONTRACT" ||
                                                                            x.Relation[0].TableName == "COMMODITYCONTRACT" ||
                                                                            x.Relation[0].TableName == "VW_COMMODITYCONTRAT"
                                                                            ))
                 select item).DefaultIfEmpty(new ReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();
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

                    if (dr[colRelationContrat.DataField] != Convert.DBNull) //DBNull possible su colonne total
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
        private static Pair<int, Cst.UnderlyingAsset> GetAsset(Referential rr, DataRow dr)
        {

            Pair<int, Cst.UnderlyingAsset> ret = null;

            ReferentialColumn colLinkAsset =
                (from item in
                     rr.Column.Where(x => x.ExistsHyperLinkColumn && x.IsHyperLink.type.StartsWith("IDASSET"))
                 select item).DefaultIfEmpty(new ReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();


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
            if (null == ret)
            {

                ReferentialColumn colRelationAsset =
                (from item in
                     rr.Column.Where(x => ArrFunc.IsFilled(x.Relation) &&  (StrFunc.IsFilled(x.Relation[0].TableName)) &&  
                                                                           (x.Relation[0].TableName.StartsWith("ASSET") ||  
                                                                           x.Relation[0].TableName.StartsWith("VW_ASSET")))
                 select item).DefaultIfEmpty(new ReferentialColumn() { ColumnName = "N/A" }).FirstOrDefault();
                if (colRelationAsset.ColumnName != "N/A")
                {
                    Nullable<Cst.UnderlyingAsset> assetCategory = null;
                    switch (colRelationAsset.Relation[0].TableName)
                    {
                        case "ASSET":
                        case "VW_ASSET":
                            assetCategory = GetAssetCategory(rr, dr, false);
                            if ((null == assetCategory) && colRelationAsset.ColumnName.EndsWith("UNL"))
                                assetCategory = GetAssetCategory(rr, dr, true);
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
        /// <param name="pGroupByNumber"></param>
        /// EG 20170228 [22883] Add
        private List<ReferentialColumn> GetColumnsOrder(int pGroupByNumber)
        {
            List<ReferentialColumn> ret = new List<ReferentialColumn>();

            List<ReferentialColumn> columnsGroupBy
                            = (from item in referential.Column.Where(x => x.GroupBySpecified && x.GroupBy.IsGroupBy)
                               select item).ToList();

            int nbColumnsGroupByNumber = columnsGroupBy.Count() - pGroupByNumber + 1;

            int k = 0;
            foreach (ReferentialColumn item in columnsGroupBy)
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
        /// <param name="pGroupRow">Représente l'enregistrement de regroupement </param>
        /// FI 20170228 [22883] Add
        private DataRow[] LoadRowsDetail(DataRow pGroupRow)
        {
            int groupByNumber = Convert.ToInt32(pGroupRow["GROUPBYNUMBER"]);
            List<ReferentialColumn> columnsOrder = GetColumnsOrder(groupByNumber);
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
        /// 
        /// </summary>
        /// <param name="pDGCE"></param>
        private void UpdateDataFromControl(object sender, GridViewUpdateEventArgs pDGCE)
        {

            int index = pDGCE.RowIndex;
            DataRow dr = DsData.Tables[0].Rows[index];

            bool isNewRecord = (referential.ExistsColumnsINS && StrFunc.IsEmpty(dr["IDAINS"].ToString()) ? true : false);
            dr.BeginEdit();

            lstGdvColumn.ForEach(column =>
            {

                try
                {
                    int i = lstGdvColumn.IndexOf(column);

                    string data = string.Empty;
                    bool isRelation = (column .columnType == GridViewColumnType.relation);
                    Control ctrlFound;
                    string ctrlID = column.columnName + RepositoryTools.SuffixEdit;
                    if ((i + 1 < lstGdvColumn.Count) && (lstGdvColumn[i + 1].columnType == GridViewColumnType.relation))
                        ctrlID = lstGdvColumn[i + 1].columnName + RepositoryTools.SuffixEdit;

                    GridView gridView = sender as GridView;
                    ctrlFound = gridView.Rows[index].FindControl(ctrlID);
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
                    if (false == column.isMandatory)
                        if (true == StrFunc.IsEmpty(data.Trim()))
                            data = string.Empty;

                    if (TypeData.IsTypeDateOrDateTime(column.dataType))
                    {
                        dr[column.columnName] = Convert.ToDateTime(data);
                    }
                    else if (TypeData.IsTypeBool(column.dataType))
                    {
                        bool boolData = Convert.ToBoolean(data);
                        dr[column.columnName] = (boolData ? 1 : 0);
                    }
                    else if (TypeData.IsTypeDec(column.dataType))
                    {
                        dr[column.columnName] = Convert.ToDecimal(data);
                    }
                    else if (TypeData.IsTypeImage(column.dataType))
                    {
                        //Cas particulier: pas d'affectation de valeur
                    }
                    else
                    {
                        dr[column.columnName] = (StrFunc.IsFilled(data)? data : Convert.DBNull);
                    }

                    //update pour EXTLID si necessaire                        
                    if (referential.HasMultiTable && DsData.Tables.Count > 1)
                    {
                        ReferentialColumn rrc = referential.Column[column.columnId];
                        if (rrc.IsExternal)
                        {
                            DataTable currentDT = DsData.Tables[1 + rrc.ExternalFieldID];
                            DataRow drExtlId;
                            string filter = string.Empty;
                            filter += " TABLENAME='" + referential.TableName + "'";
                            filter += " AND";
                            filter += " IDENTIFIER='" + rrc.ExternalIdentifier + "'";
                            filter += " AND";
                            filter += " ID=";
                            if (referential.IsDataKeyField_String)
                                filter += DataHelper.SQLString(dr[referential.IndexColSQL_DataKeyField].ToString());
                            else
                                filter += dr[referential.IndexColSQL_DataKeyField].ToString();

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
                                drExtlId["DTUPD"] = OTCmlHelper.GetDateSys(SessionTools.CS);
                                drExtlId["IDAUPD"] = SessionTools.Collaborator_IDA;
                            }
                            /* PAS DE CREATE AUTO POUR UNE NEW LINE CAR MANQUE VALEUR POUR FK
                            else
                            {
                                drExtlId = currentDT.NewRow();
                                drExtlId.BeginEdit();
                                drExtlId["TABLENAME"]  = referential.TableName;
                                drExtlId["IDENTIFIER"] = rrc.ExternalIdentifier;
                                drExtlId["ID"]         = Convert.ToInt32(dr[referential.IndexColSQL_DataKeyField].ToString());
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
                    dr[column.columnName] = Convert.DBNull;
                }
            });


            //MAJ champs INS
            if (isNewRecord && referential.ExistsColumnsINS)
            {
                dr["DTINS"] = OTCmlHelper.GetDateSys(SessionTools.CS);
                dr["IDAINS"] = SessionTools.Collaborator_IDA;
                dr["DTUPD"] = Convert.DBNull;
                dr["IDAUPD"] = Convert.DBNull;
            }
            //MAJ champs UPD
            else if (!isNewRecord && referential.ExistsColumnsUPD)
            {
                dr["DTUPD"] = OTCmlHelper.GetDateSys(SessionTools.CS);
                dr["IDAUPD"] = SessionTools.Collaborator_IDA;
            }
            dr.EndEdit();

            this.EditIndex = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetTableColumnMapping()
        {
            if (isDataFilled)
            {
                DataTable dt = DsData.Tables[0];
                lstGdvColumn.ForEach(column => dt.Columns[column.columnName].ColumnMapping = MappingType.Element);
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

        public void ResetColumns()
        {
            lstGdvColumn = new List<GridViewColumn>();
            nbColumnAction = 0;
        }


        /// <summary>
        /// Ajoute les colonnes actions
        /// <para>1er  colonne => n° de ligne</para>
        /// <para>2ème colonne => check de selection (optionel)</para>
        /// <para>3ème colonne => loupe              (optionel, existe si isFormInputMode ou isFormSelectMode)</para>
        /// <para>                                   et referential.isLoupe (si referential.isLoupeSpecified)</para>        
        /// <para>4ème colonne => creation           (optionel,existe si isGridInputMode)</para>
        /// <para>5ème colonne => Modification       (optionel,existe si isGridInputMode)</para>
        /// <para>6ème colonne => Suppression        (optionel,existe si isInputMode)</para>
        /// <para>7ème colonne => ATTACHEDDOC        (optionel,existe si isInputMode et si referentiel is ATTACHEDDOC)</para>
        /// <para>8ème colonne => SQLRowStates       (optionel,existe si la colonne existe)</para>
        /// </summary>
        private void AddColumnAction()
        {

            int currentIndex = 0;
            //
            // Colonne Numéro de ligne ( ne pas déplacer, car cette colonne est supposée la première, voir OnItemDataBound())            
            TemplateField tcRowNumber = new TemplateField();
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
                //Colonne ISSELECTED ( uniquement pour les écrans de type "Traitement", ie: Avec un Bouton "Générer")
                // Ne pas déplacer, car cette colonne est supposée la deuxième    
                TemplateField tcIsToProcess = new TemplateField();
                tcIsToProcess.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                tcIsToProcess.ItemStyle.VerticalAlign = VerticalAlign.Middle;

                string cbSelectAllID = this.ClientID + "_cbSelectAll";
                string cbSelectID = this.ClientID + "_cbSelect_";

                GridViewItemTemplateCheckBox myHeaderTemplate = new GridViewItemTemplateCheckBox(cbSelectAllID, true, true, Ressource.GetString("SelectAll"));
                myHeaderTemplate.AddAttribute("onclick", "GridViewSelectAll(this, '" + this.ClientID + "', '" + cbSelectID + "')");
                tcIsToProcess.HeaderTemplate = myHeaderTemplate;
                tcIsToProcess.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                tcIsToProcess.HeaderStyle.VerticalAlign = VerticalAlign.Middle;

                GridViewItemTemplateCheckBox myItemTemplate = new GridViewItemTemplateCheckBox(cbSelectID, "ISSELECTED", true, Ressource.GetString("Select"));
                myItemTemplate.AddAttribute("onclick", "GridViewCheckedChanged('" + this.ClientID + "', '" + cbSelectAllID + "', '" + cbSelectID + "')");
                myItemTemplate.CheckedChanged = new EventHandler(this.OnCheckedChanged_Select);
                tcIsToProcess.ItemTemplate = myItemTemplate;

                this.Columns.Add(tcIsToProcess);
                nbColumnAction++;
                currentIndex++;
            }

            if (isFormInputMode || isFormSelectMode || isFormViewerMode)
            {
                _isLoupeColumn = ((!isConsultation) && (Cst.ListType.ProcessBase != ListType) && (Cst.ListType.Report != ListType) && (Cst.ListType.Trade != ListType))
                                ||
                                (
                                    referential.ExistsColumnDataKeyField
                                    &&
                                    (
                                        referential.Column[referential.IndexDataKeyField].ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.TRADE.ToString())
                                        ||
                                        referential.Column[referential.IndexDataKeyField].ColumnName == "DATAKEYFIELD"
                                    )
                                );

                // EG 20111013
                if (_isLoupeColumn && referential.isLoupeSpecified)
                    _isLoupeColumn &= referential.isLoupe;

                if (_isLoupeColumn)
                {
                    ButtonField c1 = new ButtonField();
                    c1.CommandName = "edit";
                    c1.Text = @"<i aria-hidden='true' class='glyphicon glyphicon-search' title=""" + Ressource.GetString(referential.Modify ? "imgEdit" : "imgView") + @"""></i>";
                    this.Columns.Add(c1);
                    editItemIndex = currentIndex;
                    nbColumnAction++;
                    currentIndex++;
                }
            }
            //
            if (isInputMode)
            {
                if (isGridInputMode)
                {
                    //colonne Status de ligne (grid only)
                    // EG 20151215 Add Test referential.consultationMode
                    if (referential.Create && (referential.consultationMode != Cst.ConsultationMode.ReadOnly))
                    {
                        TemplateField tc = new TemplateField();
                        tc.HeaderText = " ";
                        GridViewItemTemplateLabel myItemTemplate = new GridViewItemTemplateLabel("statusNew");
                        tc.ItemTemplate = myItemTemplate;
                        this.Columns.Add(tc);
                        newItemIndex = currentIndex;
                        nbColumnAction++;
                        currentIndex++;
                    }

                    //colonne Edit -> Update/Cancel (grid only)
                    // EG 20151215 Add Test referential.consultationMode
                    if ((referential.Modify || referential.Create) && (referential.consultationMode != Cst.ConsultationMode.ReadOnly))
                    {
                        CommandField c0 = new CommandField();
                        c0.EditText = @"<i aria-hidden='true' class='glyphicon glyphicon-search' title=""" + Ressource.GetString(referential.Modify ? "imgEdit" : "imgView") + @"""></i>";
                        c0.UpdateText = @"<i aria-hidden='true' class='glyphicon glyphicon-floppy-save' title=""" + Ressource.GetString("btnValidate") + @"""></i>";
                        c0.CancelText = @"<i aria-hidden='true' class='glyphicon glyphicon-floppy-remove' title=""" + Ressource.GetString("btnCancel") + @"""></i>";
                        this.Columns.Add(c0);
                        editItemIndex = currentIndex;
                        nbColumnAction++;
                        currentIndex++;
                    }
                }

                //colonne delete (commune a grid et form)
                // EG 20151215 Add Test referential.consultationMode
                if (referential.Remove && (referential.consultationMode != Cst.ConsultationMode.ReadOnly))
                {
                    ButtonField c1 = new ButtonField();
                    c1.CommandName = "delete";
                    c1.Text = @"<i aria-hidden='true' class='glyphicon glyphicon-floppy-remove' title=""" + Ressource.GetString("imgRemove") + @"""></i>";
                    this.Columns.Add(c1);
                    deleteItemIndex = currentIndex;
                    nbColumnAction++;
                    currentIndex++;
                }

                //Colonne consult 
                if (this.referential.TableName.StartsWith(Cst.OTCml_TBL.ATTACHEDDOC.ToString()))
                {
                    ButtonField c00 = new ButtonField();
                    c00.CommandName = "ConsultDoc";
                    c00.Text = @"<i aria-hidden='true' class='glyphicon glyphicon-paperclip' title=""" + Ressource.GetString("btnAttachedDoc") + @"""></i>";
                    this.Columns.Add(c00);
                    nbColumnAction++;
                    currentIndex++;
                }
            }

            // SQLRowStates ( ne pas déplacer, car cette colonne est supposée la dérnière, voir OnItemDataBound())
            if (referential.SQLRowStateSpecified && StrFunc.IsFilled(referential.SQLRowState.Value))
            {
                TemplateField tc = new TemplateField();
                if (StrFunc.IsFilled(referential.SQLRowState.headerText))
                    tc.HeaderText = Ressource.GetString(referential.SQLRowState.headerText);
                tc.ItemStyle.VerticalAlign = VerticalAlign.Middle;
                this.Columns.Add(tc);
                nbColumnAction++;
                currentIndex++;
            }


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pColumnName">Représente le type de colonne gérée (ie IDA, IDDC)</param>
        /// <param name="pId">Représente l'id (id non significatif que doit ouvrir le lien)</param>
        /// <param name="pIdentifier">Permet d'alimenter le .text du link</param>
        /// <returns></returns>
        /// //FI 20120704 [17987] Refactoring, les id ne sont pas nécessairement numériques
        private HyperLink GetHyperLinkByColumn(GridViewRow pRow, string pColumnName, string pId, string pIdentifier)
        {
            HyperLink hl = null;

            if (RepositoryTools.IsHyperLinkColumn(pColumnName))
            {
                if (DecFunc.IsDecimal(pId))
                    pId = IntFunc.IntValue2(pId).ToString();

                string url = GetURL("HYPERLINK", pColumnName, pId, pRow);

                if (StrFunc.IsFilled(url))
                {
                    hl = new HyperLink();
                    hl.Text = pIdentifier;
                    //hl.ToolTip = "See the identification sheet...";
                    hl.CssClass = "linkDatagrid";
                    hl.Target = "_blank";
                    hl.NavigateUrl = url;
                }
            }
            //
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

            WCToolTipHyperlink hlLinkFile = new WCToolTipHyperlink();
            hlLinkFile.CssClass = CSS.SetCssClass(CSS.Main.loupe);
            hlLinkFile.NavigateUrl = page + urlArg;
            hlLinkFile.Target = "_blank";
            hlLinkFile.Pty.TooltipContent = Ressource.GetString("imgConsultDoc");

            return hlLinkFile;
        }


        /// <summary>
        /// 
        /// </summary>
        private void AddColumnData()
        {

            //Add des columns data via referentials[] 
            bool isSetDataKeyField = false;
            string ressourceTranslated = null;

            lstGdvColumn = new List<GridViewColumn>();

            if (null != referential)
            {
                for (int i = 0; i < referential.Column.Length; i++)
                {
                    ReferentialColumn rrc = referential[i];
                    if (!(rrc.IsRole || rrc.IsItem))//NB: Les colonnes "Role" et "Item" ne sont pas dans le Select
                    {
                        if (!rrc.IsHideInDataGrid)
                            nbColumnGrid++;

                        #region Columns EXTLLINK & EXTLLINK2 & EXTLATTRB
                        if (rrc.ColumnName == "EXTLLINK" || rrc.ColumnName == "EXTLLINK2" || rrc.ColumnName == "EXTLATTRB")
                        {
                            //Recherche de l'éventuelle customisation de la colonne 
                            string SQLSelect = @"select DISPLAYNAME, ISHIDEINDATAGRID, DEFAULTVALUE
                            from dbo.DEFINEEXTLLINK
                            where TABLENAME = {0} and EXTLTYPE = {1}";
                            SQLSelect = String.Format(SQLSelect, DataHelper.SQLString(referential.TableName), DataHelper.SQLString(rrc.ColumnName));
                            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, SQLSelect))
                            {
                                if (dr.Read())
                                {
                                    if (rrc.Default == null)
                                        rrc.Default = new ReferentialColumnDefault[1];
                                    if (rrc.Default[0] == null)
                                        rrc.Default[0] = new ReferentialColumnDefault();
                                    rrc.Default[0].Value = dr["DEFAULTVALUE"].ToString();
                                    rrc.Ressource = dr["DISPLAYNAME"].ToString();
                                    rrc.IsHideInDataGridSpecified = true;
                                    rrc.IsHideInDataGrid = Convert.ToBoolean(dr["ISHIDEINDATAGRID"]);
                                }
                            }
                        }
                        #endregion Columns EXTLLINK & EXTLLINK2 & EXTLATTRB

                        #region Add column
                        //When isConsultation, Ressource already translated by LSTCONSULT class
                        ressourceTranslated = (isConsultation ? rrc.Ressource : Ressource.GetMulti(rrc.Ressource, 1));

                        GridViewColumn gdvColumn = new GridViewColumn(referential.TableName, GridViewColumnType.column, i, rrc);
                        lstGdvColumn.Add(gdvColumn);
                        if (rrc.ExistsRelation)
                            lstGdvColumn.Add(new GridViewColumn(referential.TableName, GridViewColumnType.relation, i, rrc));

                        if (isTemplateColumn)
                        {
                            #region isTemplateColumn
                            TemplateField tc = new TemplateField();

                            tc.HeaderText = ressourceTranslated;
                            //S'il s'agit de controls liés (ressource == null) : 1 header pour plusieurs cols (Ex: Tenor = Period + Frequency)
                            bool isHeaderWithMultiColumn = StrFunc.IsEmpty(rrc.Ressource)
                                || (rrc.Ressource == @"/") || (rrc.Ressource == @":");
                            if (isHeaderWithMultiColumn && !rrc.IsHideInDataGrid)
                                tc.HeaderText = HEADERWITHMULTICOLUMN + rrc.Ressource;
                            //
                            tc.SortExpression = rrc.IDForItemTemplate;
                            if (rrc.ExistsRelation)
                                tc.Visible = false;
                            else
                                tc.Visible = !rrc.IsHideInDataGrid;

                            //
                            ITemplate itemTemplate = null;
                            if (referential.HasAggregateColumns && rrc.GroupBySpecified && rrc.GroupBy.AggregateSpecified)
                                itemTemplate = new GridViewItemTemplateAggregate(rrc.IDForItemTemplate);
                            else
                                itemTemplate = new GridViewItemTemplateLabel(rrc.IDForItemTemplate);
                            tc.ItemTemplate = itemTemplate;

                            GridViewEditItemTemplate editItemTemplate = new GridViewEditItemTemplate(rrc, false);
                            tc.EditItemTemplate = editItemTemplate;

                            tc.ItemStyle.HorizontalAlign = gdvColumn.HorizontalAlign;

                            this.Columns.Add(tc);

                            if (rrc.IsDataKeyField)
                            {
                                isSetDataKeyField = true;
                                this.DataKeyNames = new string[] { rrc.IDForItemTemplate };
                            }

                            if (rrc.IsKeyField & !isSetDataKeyField)
                                this.DataKeyNames = new string[] { rrc.IDForItemTemplate };

                            if (rrc.IsForeignKeyField)
                                this.ForeignKeyField = rrc.IDForItemTemplate;

                            if (RepositoryTools.IsHyperLinkAvailable(rrc))
                                isExistHyperLinkColumn = true;


                            #region ExistsRelation
                            if (rrc.ExistsRelation)
                            {
                                GridViewItemTemplateLabel itemTemplateRelation = new GridViewItemTemplateLabel(rrc.IDForItemTemplateRelation);
                                GridViewEditItemTemplate editItemTemplateRelation = new GridViewEditItemTemplate(rrc, true);

                                TemplateField tcr = new TemplateField();
                                tcr.HeaderText = (isConsultation ? rrc.Relation[0].ColumnSelect[0].Ressource : Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1));
                                tcr.SortExpression = rrc.IDForItemTemplateRelation;
                                tcr.Visible = !rrc.IsHideInDataGrid;
                                tcr.ItemTemplate = itemTemplateRelation;
                                tcr.EditItemTemplate = editItemTemplateRelation;
                                tcr.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                                this.Columns.Add(tcr);
                            }
                            #endregion ExistsRelation
                            #endregion isTemplateColumn
                                }
                                else
                                {
                        #region !isTemplateColumn
                            BoundField bc = new BoundField();
                            bc.HtmlEncode = false;
                            bc.DataField = rrc.IDForItemTemplate;
                            bc.HeaderText = ressourceTranslated;
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

                            bc.ItemStyle.HorizontalAlign = gdvColumn.HorizontalAlign;
                            bc.DataFormatString = gdvColumn.DataFormatString;

                            this.Columns.Add(bc);

                            if (rrc.IsDataKeyField)
                            {
                                isSetDataKeyField = true;
                                DataKeyNames = new string[1] { bc.DataField };
                            }

                            if (rrc.IsKeyField & !isSetDataKeyField)
                                DataKeyNames = new string[1] { bc.DataField };

                            if (rrc.IsForeignKeyField)
                                this.ForeignKeyField = rrc.IDForItemTemplate;

                            if (RepositoryTools.IsHyperLinkAvailable(rrc))
                                isExistHyperLinkColumn = true;

                            #region ExistsRelation
                            if (rrc.ExistsRelation)
                            {
                                //FI 201110262 Fonction apellée plus haut
                                //CheckForHyperLink(rrc);

                                BoundField bcr = new BoundField();
                                bcr.HtmlEncode = false;
                                bcr.DataField = rrc.IDForItemTemplateRelation;
                                bcr.HeaderText = (isConsultation ? rrc.Relation[0].ColumnSelect[0].Ressource : Ressource.GetMulti(rrc.Relation[0].ColumnSelect[0].Ressource, 1));
                                bcr.SortExpression = bcr.DataField;
                                bcr.Visible = !rrc.IsHideInDataGrid;
                                // EG 20091110
                                bcr.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                                this.Columns.Add(bcr);
                            }
                            #endregion ExistsRelation

                            #endregion !isTemplateColumn
                        }
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
            SetIndexPage(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        private void SetLastPage(Object Src, EventArgs e)
        {
            SetIndexPage(PageCount - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void OnSetPageNumber(Object Src, EventArgs e)
        {
            int pageIndex = 0;
            WebControl lblGoToPage = Src as WebControl;
            TextBox txt = new TextBox();;
            if (null != lblGoToPage)
                txt = lblGoToPage.ID.EndsWith(TableRowSection.TableHeader.ToString())?txtSetPageNumberHeader:txtSetPageNumberFooter;
            
            try {pageIndex = Convert.ToInt32(txt.Text) - 1;}
            catch {pageIndex = 0;txt.Text = "1";}
            pageIndex = Math.Min(PageCount - 1, pageIndex);
            pageIndex = Math.Max(0, pageIndex);
            SetIndexPage(pageIndex);
        }

        /// <summary>
        /// Affectation CurrentPageIndex
        /// </summary>
        /// <param name="pIndex"></param>
        private void SetIndexPage(int pIndex)
        {
            PageIndex = pIndex;
            //si pagination personalisée, la source de donnée doit être rechargée avec les lignes correspondant à la page demandée
            //Sinon non, la source de donnée est déjà chargée
            isLoadData = AllowCustomPaging;

        }

        /// <summary>
        /// Alimente un DataView à partir du jeu de résultat (DsData)
        ///<para>Affecte le DataSource avec le DefaultView issu du jeu de résultat</para>
        /// </summary>
        private void SetDataSource()
        {
            if (isDataAvailable)
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
            if (isDataSourceAvailable && isDataFilled)
            {
                DataView dv = (DataView)DataSource;
                


                string[] sortingFieldsParsed = null;

                // MF 20120604 Ticket 17823
                if (!String.IsNullOrEmpty(sortExpression))
                {
                    sortingFieldsParsed = sortExpression.Split(',');

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

                        sortExpression = sortingFieldsParsed.Aggregate((curr, next) => String.Format("{0},{1}", curr, next));
                    }
                    else
                    {
                        ViewState[this.ClientID + "SortingFields"] = null;
                        ViewState[this.ClientID + "SortingOrders"] = null;

                        sortExpression = null;
                    }
                }

                dv.Sort = sortExpression;
            }
        }

        /// <summary>
        /// Définie l'index de page affiché
        /// </summary>
        private void SetCurrentPageIndex()
        {
            if ((totalRowscount > 0) && PageSize > 0)
            {
                int result;
                int maxItem = Math.DivRem(totalRowscount, PageSize, out result);
                if (result == 0)
                    maxItem--;
                //

                // 20120411 MF Test bug template datagrid
                if (PageIndex != Math.Min(PageIndex, maxItem))
                {
                    JavaScript.ReloadImmediate((PageBase)this.Page, Ressource.GetString("BackToTheFirstPage"), "0", "SELFRELOAD_");
                }
                PageIndex = Math.Min(PageIndex, maxItem);
            }
            else
            {
                PageIndex = 0;
            }
        }

        /// <summary>
        /// Constitution de l'URL pour accès au formulaire de saisie par dblclick en mode isFormInputMode
        /// </summary>
        public string GetURLForInsert(string pForeign)
        {
            return GetURL("INSERT", string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Obtient URL quui ouvre le Formulaire de saisie
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private string GetURLForUpdate(GridViewRow pRow)
        {
            string ret = string.Empty;
            if (referential.IndexDataKeyField > -1)
            {
                string columnName = referential[referential.IndexDataKeyField].DataField;
                DataRow row = ((DataRowView)pRow.DataItem).Row;
                string data = row[row.Table.Columns[columnName].Ordinal].ToString();

                if (StrFunc.IsFilled(data) && (data != Cst.HTMLSpace))
                {
                    //FI 20120111 Add FormatToInvariant
                    if (false == referential.IsROWVERSIONDataKeyField)////FI 20110112 Les colonnes ROWVERSION ne sont pas formattées, en plus FormatToInvariant plante si donnée de type ROWVERSION
                        data = FormatToInvariant(data, referential[referential.IndexDataKeyField].DataType.value);

                    ret = GetURL("UPDATE", DataKeyNames[0], data, pRow);
                }
            }
            return ret;
        }

        // EG 20170125 [Refactoring URL] New
        private string GetURL(string pMode, string pColumn, string pData, GridViewRow pRow)
        {
            // Pour interdire le dblclick sur les lignes de rupture)
            if ((pMode != "INSERT") && StrFunc.IsEmpty(pData))
                return "return false";

            string url = string.Empty;
            bool isOpenReferential = false;
            bool isOpenTrade = false;

            DataRow row = null;
            if ((null != pRow) && (null != pRow.DataItem))
                row = ((DataRowView)(pRow.DataItem)).Row; 
            Nullable<Cst.UnderlyingAsset> assetCategoryEnum = null;
            Nullable<IdMenu.Menu> idMenu = null;
            string pageName = string.Empty;
            
            if (pMode == "HYPERLINK")
            {
                #region Mode Hyperlink
                if (null != pRow)
                {
                    switch (pColumn)
                    {
                        case "IDT":
                            isOpenTrade = true;
                            idMenu = SpheresURL.GetMenu_Trade(pColumn, referential.TableName, row, referential.GetIndexColSQL("GPRODUCT"));
                            break;
                        case "IDASSET":
                        case "IDASSET_UNL":
                        case "IDCONTRACT":
                            assetCategoryEnum = GetAssetCategory(referential, row, (pColumn == "IDASSET_UNL") ? true : false);
                            if (assetCategoryEnum.HasValue)
                                idMenu = SpheresURL.GetMenu_Asset(pColumn, referential.TableName, assetCategoryEnum);
                            break;
                        case "IDXC": // FI 20170908 [23409] Add IDXC
                            Nullable<Cst.ContractCategory> contractCategoryEnum = GetContractCategory(referential, row);
                            if (contractCategoryEnum.HasValue)
                                idMenu = SpheresURL.GetMenu_Contract(pColumn, contractCategoryEnum);
                            break;
                        case "IDIOELEMENT":
                            idMenu = SpheresURL.GetMenu_IO(row, referential.GetIndexColSQL("ELEMENTTYPE"));
                            break;
                        default:
                            idMenu = SpheresURL.GetMenu_Repository(pColumn, pData);
                            break;
                    }
                }

                Cst.ConsultationMode mode = referential.consultationMode;
                if (isGridSelectMode || isFormSelectMode)
                    mode = Cst.ConsultationMode.Select;
                else if (isFormViewerMode)
                    mode = Cst.ConsultationMode.ReadOnly;

                url = SpheresURL.GetURL(idMenu, pData, mode, Parent.ID.ToString());
                #endregion Mode Hyperlink
            }
            else
            {
                #region LOUPE
                isOpenReferential = true;
                if (referential.ExistsColumnDataKeyField)
                {
                    string DKF_ColumnName = referential.Column[referential.IndexDataKeyField].ColumnName;
                    isOpenTrade = (DKF_ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.TRADE.ToString()));
                    if (isOpenTrade)
                    {
                        isOpenReferential = false;
                        #region Consultations sur la base de la table TRADE
                        idMenu = SpheresURL.GetMenu_Zoom(row, ObjectName, DKF_ColumnName, referential.TableName, referential.GetIndexColSQL("GPRODUCT"));
                        url = SpheresURL.GetURL(idMenu, pData, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, Parent.ID);
                        #endregion Consultations sur la base de la table TRADE
                    }
                    else if (DKF_ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.EVENT.ToString()))
                    {
                        isOpenReferential = false;
                        #region Consultation d'un événement
                        string sIdEvent = pData.ToString();
                        string sIdT = string.Empty;
                        int indexIDT = referential.GetIndexColSQL("IDT");
                        if (-1 < indexIDT)
                        {
                            row = ((DataRowView)(pRow.DataItem)).Row;
                            sIdT = row.ItemArray[indexIDT].ToString();
                        }
                        pageName = SpheresURL.GetUrlFormEvent(HttpUtility.UrlEncode(sIdEvent, Encoding.Default), HttpUtility.UrlEncode(sIdT, Encoding.Default), SessionTools.Menus);


                        url = SpheresURL.GetURL(IdMenu.Menu.InputEvent, pData, sIdT, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, string.Empty, Data_CacheName, Parent.ID);

                        #endregion
                    }
                    else if (DKF_ColumnName == "DATAKEYFIELD")
                    {
                        isOpenReferential = false;
                        #region Consultation de la POSITION
                        idMenu = SpheresURL.GetMenu_Zoom(row, ObjectName, DKF_ColumnName, referential.TableName, referential.GetIndexColSQL("GPRODUCT"));
                        url = SpheresURL.GetURL(idMenu, null, HttpUtility.UrlEncode(pData, Encoding.Default),
                            SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, AddDynamicArgument(), Data_CacheName, Parent.ID);
                        #endregion SpheresURL de la POSITION
                    }
                    else if ((DKF_ColumnName == OTCmlHelper.GetColunmID(Cst.OTCml_TBL.VW_ENTITYCSS.ToString()))
                        && ((ObjectName == "ENTITYCSS_EOD") || (ObjectName == "ENTITYCSS_CLOSINGDAY")))
                    {
                        isOpenReferential = false;
                        #region POSREQUEST des traitements EOD et CLOSINGDAY
                        idMenu = (ObjectName == "ENTITYCSS_EOD") ? IdMenu.Menu.LogEndOfDay : IdMenu.Menu.LogClosingDay;
                        url = SpheresURL.GetURL(idMenu, pData, string.Empty, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal,
                             AddDynamicArgument(), Data_CacheName, Parent.ID.ToString());
                        #endregion POSREQUEST des traitements EOD et CLOSINGDAY
                    }
                    else if (DKF_ColumnName.StartsWith("IDCA"))
                    {
                        isOpenReferential = false;
                        #region CORPORATE ACTION embedded|issue
                        idMenu = (DKF_ColumnName == "IDCA" ? IdMenu.Menu.InputCorporateActionEmbedded : IdMenu.Menu.InputCorporateActionIssue);
                        url = SpheresURL.GetURL(idMenu, pData, SpheresURL.LinkEvent.ondblclick, Cst.ConsultationMode.Normal, Parent.ID);
                        #endregion CORPORATE ACTION embedded|issue
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

                    if (isConsultation)
                    {
                        //FI 20120215 Ds le cas des consultation Le formulaire est ouvert à partir du fichier xml présent sous le répertoire  consultation
                        type = Cst.ListType.Consultation.ToString();
                    }

                    if (referential.TargetNameSpecified)
                        //Ouverture sur un autre XML (ex. FEE_ACTOR --> FEEMATRIX)
                        @objectItem = referential.TargetName;

                    #region OpenReferentialFormArguments
                    OpenReferentialFormArguments arg = new OpenReferentialFormArguments();
                    arg.type = type;
                    arg.@object = @objectItem;
                    arg.idMenu = idMenu2;
                    arg.PK = pColumn;
                    arg.PKValue = pData;

                    arg.mode = referential.consultationMode;

                    if (isGridSelectMode || isFormSelectMode)
                        arg.mode = Cst.ConsultationMode.Select;
                    else if (isFormViewerMode)
                        arg.mode = Cst.ConsultationMode.ReadOnly;

                    if (StrFunc.IsFilled(condApp))
                        arg.condApp = condApp;

                    arg.dS = Data_CacheName;
                    arg.param = param;
                    arg.dynamicArg = referential.xmlDynamicArgs;

                    arg.IsnewRecord = (pMode == "INSERT");
                    string itemValueFK = valueFK;
                    if (false == arg.IsnewRecord)
                        itemValueFK = GetItemValueFK(pRow);
                    if (StrFunc.IsFilled(itemValueFK))
                    {
                        arg.FK = columnFK;
                        arg.FKValue = itemValueFK;
                    }

                    arg.titleMenu = TitleMenu;
                    arg.titleRes = TitleRes;
                    arg.formId = Parent.ID;

                    pageName = arg.GetURLOpenFormRepository();
                    if (StrFunc.IsFilled(pageName))
                        url = JavaScript.GetWindowOpen(pageName, Cst.WindowOpenStyle.OTCml_FormReferential);
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// EG 20110622 New
        private Nullable<Cst.OTCml_TBL> GetTradeObjectNameByProduct(GridViewRow pRow)
        {
            Nullable<Cst.OTCml_TBL> ret = null;
            int indexGPRODUCT = referential.GetIndexColSQL("GPRODUCT");

            if (-1 < indexGPRODUCT)
            {
                // EG 20091110
                DataRow row = ((DataRowView)(pRow.DataItem)).Row;
                //
                string product = row.ItemArray[indexGPRODUCT].ToString();
                switch (product)
                {
                    case Cst.ProductGProduct_RISK:
                        ret = Cst.OTCml_TBL.TRADERISK;
                        break;
                    case Cst.ProductGProduct_ASSET:
                        ret = Cst.OTCml_TBL.DEBTSECURITY;
                        break;
                    case Cst.ProductGProduct_ADM:
                        ret = Cst.OTCml_TBL.TRADEADMIN;
                        break;
                    default:
                        ret = Cst.OTCml_TBL.TRADE;
                        break;
                }
            }
            else
            {
                //PL 20111025 Tip
                if (referential.TableName == Cst.OTCml_TBL.QUOTE_DEBTSEC_H.ToString())
                    ret = Cst.OTCml_TBL.DEBTSECURITY;
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
                da += "&DA=" + HttpUtility.UrlEncode(queryStringDA, Encoding.Default);
            }
            else if (ArrFunc.IsFilled(referential.xmlDynamicArgs))
            {
                da += "&DA=";
                for (int i = 0; i < referential.xmlDynamicArgs.Length; i++)
                    da += HttpUtility.UrlEncode(referential.xmlDynamicArgs[i], Encoding.Default) + StrFunc.StringArrayList.LIST_SEPARATOR;
            }
            return da;
        }

        /// <summary>
        /// initialisation des propriétés du datagrid
        /// </summary>
        private void InitTemplateGridView()
        {

            #region Initialisation des propriétés du GridView
            // Default settings for the grid Pager
            //PagerStyle.CssClass = "DataGrid_PagerStyle";
            //PagerStyle.HorizontalAlign = HorizontalAlign.Left;
            //PagerSettings.Mode = PagerButtons.Numeric;
            //PagerSettings.PageButtonCount = 10;
            PagerSettings.Visible = false;

            //if (SessionTools.IsPagerPositionTopAndBottom)
            //    PagerSettings.Position = PagerPosition.TopAndBottom;
            //else
            //    PagerSettings.Position = PagerPosition.Bottom;
            //	
            // Default settings for pagination
            SetPaging(SessionTools.NumberRowByPage);

            // Other visual default settings
            GridLines = GridLines.Both;
            CellSpacing = 0;
            CellPadding = 3;
            Width = Unit.Percentage(100);
            CssClass = "table table-striped table-bordered table-hover table-condensed";
            AutoGenerateColumns = false;

            // EditItemStyle
            EditRowStyle.CssClass = "DataGrid_SelectedItemStyle";
            EditRowStyle.CssClass = EFSCssClass.Capture;

            // Settings for normal items (all or odd-only rows)
            //RowStyle.CssClass = "DataGrid_ItemStyle";
            RowStyle.Wrap = false;

            // Settings for alternating items (none or even-only rows)
            //AlternatingRowStyle.CssClass = "DataGrid_AlternatingItemStyle";
            AlternatingRowStyle.Wrap = false;

            // Settings for Selected items (none or even-only rows)
            //SelectedRowStyle.CssClass = "DataGrid_SelectedItemStyle";
            SelectedRowStyle.Wrap = false;

            // Settings for Footer
            ShowFooter = false;
            FooterStyle.HorizontalAlign = HorizontalAlign.Left;
            //FooterStyle.CssClass = "DataGrid_FooterStyle";

            // Settings for heading  
            //HeaderStyle.CssClass = "header";
            HeaderStyle.Wrap = false;

            // Sorting  
            AllowSorting = true;
            ViewState[this.ClientID + "SortedAscending"] = "yes";

            #endregion Initialisation des propriétés du DataGrid

            #region Set event handlers
            RowCancelingEdit += new GridViewCancelEditEventHandler(OnCancelingEdit);
            RowEditing += new GridViewEditEventHandler(OnEditing);
            RowDeleting += new GridViewDeleteEventHandler(OnDeleting);
            RowUpdating += new GridViewUpdateEventHandler(OnUpdating);
            RowCommand += new GridViewCommandEventHandler(OnRowCommand);
            Sorting += new GridViewSortEventHandler(OnSorting);
            RowCreated += new GridViewRowEventHandler(OnRowCreated);
            RowDataBound += new GridViewRowEventHandler(OnRowDataBound);
            PageIndexChanging += new GridViewPageEventHandler(OnPageIndexChanging);
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
        /// <param name="pDynamicDatas"></param>
        public void LoadReferential(Dictionary<string, StringDynamicData> pDynamicDatas)
        {
            //PL 20120618 Add test on PROCESSDET_L (A voir pour généraliser...)
            //if (isSelectMode)
            if (isSelectMode
                || StrFunc.ContainsIn(IdMenu.GetIdMenu(IdMenu.Menu.PROCESSDET_L), this.IDMenu)//"OTC_VIEW_LOGPROCESS_DET"
                || StrFunc.ContainsIn(IdMenu.GetIdMenu(IdMenu.Menu.TRACKERDET_L), this.IDMenu)//"OTC_VIEW_LOGREQUESTPROCESS"
                )
                InitForSelectMode();

            if (ArrFunc.IsFilled(NVC_QueryString.AllKeys))
            {
                condApp = NVC_QueryString["CondApp"];
                param = RepositoryTools.GetQueryStringParam(Page);

            }

            if (isConsultation)
                SQLInit_LoadReferential(pDynamicDatas);
            else
                XMLInit_LoadReferential(pDynamicDatas);

            referential.SetPermission(IDMenu);

            if ((consult.template != null))
                SetPaging(consult.template.ROWBYPAGE);

            InitColumnFK();

            AddColumns();
        }

        /// <summary>
        /// Retourne un array où chaque item contient un StringDynamicData serializé
        /// </summary>
        /// <param name="pDynamicDatas"></param>
        /// <returns></returns>
        private static string[] DynamicDatasToString(Dictionary<string, StringDynamicData> pDynamicDatas)
        {

            string[] ret = null;
            if (null != pDynamicDatas && ArrFunc.Count(pDynamicDatas) > 0)
            {
                string dA = string.Empty;
                IEnumerator listDA = pDynamicDatas.Values.GetEnumerator();
                while (listDA.MoveNext())
                {
                    if (StrFunc.IsFilled(dA))
                        dA += StrFunc.StringArrayList.LIST_SEPARATOR;
                    dA += ((StringDynamicData)listDA.Current).Serialize();
                }
                ret = StrFunc.StringArrayList.StringListToStringArray(dA);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="e"></param>
        public void AddRow(Object Src, EventArgs e)
        {

            if (!isLocked)
            {
                //
                if (!isDataModified && false == (dataFrom__EVENTTARGET == "PAGE" && dataFrom__EVENTARGUMENT == "ADDROW"))
                    PageIndexBeforeInsert = this.PageIndex;
                //
                if (!isLastPage)
                {
                    //if not is last page, go to last page and re-insert
                    PageIndex = PageCount - 1;
                    JavaScript.DoPostBackImmediate((PageBase)this.Page, "PAGE", "ADDROW");
                }
                else
                {
                    // Add a blank row to main Table
                    DataRow dr = DsData.Tables[0].NewRow();
                    if (DsData.Tables[0].Rows.Count == 0)
                        referential.InitializeNewRow(ref dr);
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
                        referential.InitializeNewRow(ref dr, drPrevious);
                    }

                    DsData.Tables[0].Rows.Add(dr);
                    // If the is full, move to next page. In this case, first item
                    int newRowIndex = this.Rows.Count;
                    DataView dv = DsData.Tables[0].DefaultView;
                    dv.RowStateFilter = DataViewRowState.CurrentRows | DataViewRowState.Deleted;
                    int countRowIndex = dv.Count;
                    if ((countRowIndex - 1) / this.PageSize == this.PageCount)
                    {
                        this.PageIndex++;
                        newRowIndex = 0;
                    }

                    // Turn edit mode on for the newly added row
                    this.EditIndex = newRowIndex;

                    // Refresh the grid
                    BindData();
                    //
                    isDataModified = true;
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
                    referential.InitializeNewRow(ref dr);
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
                        referential.InitializeNewRow(ref dr);
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
                        referential.InitializeNewRow(ref dr);
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
                        referential.InitializeNewRow(ref dr);
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
                        referential.InitializeNewRow(ref dr);
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
                    referential.InitializeNewRow(ref dr);
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
                    referential.InitializeNewRow(ref dr);
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
                        sumAmount = sumAmount - Convert.ToDecimal(pDt.Rows[currentRow]["AMOUNT"]);
                    else
                        sumAmount = sumAmount + Convert.ToDecimal(pDt.Rows[currentRow]["AMOUNT"]);
                    #endregion Construct total
                }
                else
                {
                    #region Add new row for total
                    dr = pDt.NewRow();
                    referential.InitializeNewRow(ref dr);
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

            DateTime dtFixing = Convert.ToDateTime(referential.dynamicArgs["DATE2"].value);
            decimal thresholdReference = Convert.ToDecimal(referential.dynamicArgs["THRESHOLD"].value) / 100;
            string displayMode = referential.dynamicArgs["DISPLAYMODE"].value;

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
                            KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate();
                            keyAssetFXRate.IdC1 = idc;
                            keyAssetFXRate.IdC2 = idcCtrval;
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
                    referential.InitializeNewRow(ref dr);
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

            if (isDataAvailable)
            {
                //
                SetDataSource();
                //
                SetDataSourceOrder();
                //
                //SetCurrentPageIndex();
                //
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

            if (false == isDataSourceAvailable)
                throw new Exception("No Data is available");
            //
            DataTable dt = ((DataView)DataSource).Table;
            dt.RejectChanges();
            isDataModified = false;
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

            int rowsaffected = -1;
            string message = string.Empty;
            string error = string.Empty;

            MQueueRequester qRequester = new MQueueRequester(new TrackerRequest(OTCmlHelper.GetDateSys(SessionTools.CS), SessionTools.NewAppInstance()));
            // FI 20150325 [20893] add qRequester and IDMenu 
            Cst.ErrLevel ret = EFS.GridViewProcessor.SQLReferentialData.ApplyChangesInSQLTable(SessionTools.CS, null, referential, DsData,
                        out rowsaffected, out message, out error, qRequester, IDMenu);

            if (ret == Cst.ErrLevel.SUCCESS && rowsaffected >= 1)
            {
                //purge du cache => suppression dans le cache des requêtes qui s'appuient sur la table mise à jour
                RepositoryTools.RemoveCache(DsData.Tables[0].TableName);

                // FI 20150529 [20982] call AddSessionRestrict
                RepositoryTools.AddItemSessionRestrict(referential);
            }

            // RD 20150324 [20893] Utilisation de DsData comme paramètre de la méthode SendMQueueAfterChange
            // FI 20150325 [20893] Mise en commetaire de l'appel à referential.SendMQueueAfterChange(Le postage éventuel de Message Queue est déjà effectué ds SQLReferentialData.ApplyChangesInSQLTable()
            //Send Eventuel de message (dépend du référentiel)
            //if (ret == Cst.ErrLevel.SUCCESS && rowsaffected > 1) //PL 20130416 Pourquoi >1 et non pas >=1 ???
                //referential.SendMQueueAfterChange(null, DsData, IDMenu);

            if (ret == Cst.ErrLevel.SQLDEFINED)
            {
                (this.Page as ContentPageBase).SetDialogImmediate("SubmitChange", error + Cst.HTMLBreakLine2 + Ressource.GetString(message), ProcessStateTools.StatusErrorEnum);

            }
            else if (rowsaffected <= 0)
            {
                (this.Page as ContentPageBase).SetDialogImmediate("SubmitChange", Ressource.GetString("Msg_NoModification"), ProcessStateTools.StatusWarningEnum);
            }
            else if (pShowSuccessMessage)
            {
                PageIndex = PageIndexBeforeInsert;
                message = Ressource.GetString("Msg_ProcessSuccessfull") + Cst.CrLf;
                message += Ressource.GetString2("Msg_LinesProcessed", rowsaffected.ToString());
                message += Cst.CrLf + Cst.CrLf;
                message += Ressource.GetString("Msg_ContinueProcessingAsk");
                JavaScript.ConfirmImmediate((PageBase)this.Page, message, "0", "SELFRELOAD_");
            }

            return ret;
        }
        


        #region OnUpdateView event
        public event EventHandler UpdateView;
        protected virtual void OnUpdateView(EventArgs e)
        {
            if (UpdateView != null)
                UpdateView(this, e);
        }
        #endregion OnUpdateView event

        #region OnSelectView event
        public event EventHandler SelectView;
        protected virtual void OnSelectView(EventArgs e)
        {
            if (SelectView != null)
                SelectView(this, e);
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
                _alException = new List<SpheresException>();

                _isApplyOptionalFilter = true;
                _positionFilterDisabled = -1;

                InitPagingType();

                IDMenu = NVC_QueryString["IDMenu"];
                IDMenuSys = NVC_QueryString["IDMenuSys"];

                //***********************************************************************
                //Rappel: 
                //- Mode 3 boutons (from XML file) : Referential, Log, Price, Process...
                //- Mode 5 boutons (from LST table): Consultation only, Consultation and Process
                //***********************************************************************
                if (isConsultation)
                    SQLInit_TitleAndObjecName();
                else
                    XMLInit_TitleAndObjecName();

                //Default inputMode value
                #region inputMode value
                if (StrFunc.IsFilled(NVC_QueryString["InputMode"]))
                    inputMode = (Cst.DataGridMode)Enum.Parse(typeof(Cst.DataGridMode), NVC_QueryString["InputMode"]);
                else
                    inputMode = (isConsultation ? Cst.DataGridMode.FormInput : Cst.DataGridMode.FormSelect);
                #endregion

                if (!isLocalGridSelectMode)
                    isLocalGridSelectMode = (inputMode == Cst.DataGridMode.LocalGridSelect);

                if (isGridSelectMode)
                    JavaScript.GetReferential((PageBase)this.Page, true);

                _currencyInfos = new Hashtable();

                InitTemplateGridView();

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
                if (referential.SQLPreSelectSpecified)
                {
                    bool isPreSelect = true;
                    //
                    if (!Page.IsPostBack)
                        isPreSelect = isLoadDataOnStart;
                    else
                        isPreSelect = isSelfReload || isReloadCriteria;
                    //Remarque : En mode pagination ExecutePreSelect n'est effectué qu'une seule fois
                    //Si l'utilisateur demande une nouvelle page, les instructions présentes ds PreSelect ne sont pas effectué
                    if (isPreSelect)
                        ExecutePreSelect();
                }
                //
                if ((!Page.IsPostBack) ||
                    (StrFunc.IsEmpty(dataFrom__EVENTARGUMENT) || isSelfReload))
                {
                    //
                    isLoadData = true;
                    if ((!Page.IsPostBack) && (!isLoadDataOnStart))
                    {
                        isLoadData = false;
                        //Alimentation d'un jeu de sonnée vierge afin d'afficher un DataGrid vide
                        DsData = new DataSet();
                        DsData.Tables.Add();
                    }
                }
                else
                {
                    if (Page.Request.Params["__EVENTARGUMENT"] == "DOUBLECLICKED_" + this.ClientID & isLocalGridSelectMode)
                    {
                        string colValue = Page.Request.Params["__EVENTTARGET"];
                        if (colValue != null)
                        {
                            string[] tmp_ = colValue.Split(",".ToCharArray());
                            DblClickEventArgs DCEa = new DblClickEventArgs(tmp_[0], tmp_[1], tmp_[2]);
                            this.DblClick(this, DCEa);
                        }
                    }
                    else if (Page.Request.Params["__EVENTARGUMENT"].ToUpper() == "SELFCLEAR_")
                    {
                        //Alimentation d'un jeu de sonnée vierge afin d'afficher un DataGrid vide
                        isLoadData = false;
                        DsData = new DataSet();
                        DsData.Tables.Add();
                    }
                }
                //
                if (isLoadData)
                {
                    UpdateDataSourceFromRequestValues_OnLoad();
                    isDataModified = false;
                }
                //
                if (dataFrom__EVENTTARGET == "PAGE" && dataFrom__EVENTARGUMENT == "ADDROW")
                    AddRow(new Object(), new System.EventArgs());
                //

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
                if (isLoadData)
                    LoadData();

                if (isBindData)
                {
                    LoadSearch();// FI 20140926 [XXXX] apppel à LoadSearch
                    BindData();
                }

                RefreshInfosTable();

            }
            catch (Exception ex) { TrapException(ex); }
        }

        /// EG 20151222 Refactoring
        protected void RefreshInfosTable()
        {

            const int itemColumnNb = 3;

            string displayCol = string.Empty;
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
                    dataPnl = new Panel();
                    dataPnl.ID = divInfos.ID + "OrderByDataRow";
                    dataPnl.CssClass = HtmlTools.cssInfosSort;
                    dataPnl.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("SortedBy") + ":", HtmlTools.cssInfosColumnName));
                    divInfos.Controls.Add(dataPnl);
                    #endregion
                }

                for (int i = 0; i < aSortExpressions.Length; i++)
                {
                    displayCol = LstConsultData.GetLabelReferential(referential, aSortExpressions[i].ToString());

                    int dataLblIndex = -1;
                    #region Check if column already exists in row
                    foreach (Label lbl in dataPnl.Controls)
                    {
                        if (displayCol == lbl.Text)
                        {
                            dataLblIndex = dataPnl.Controls.IndexOf(lbl);
                            break;
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
                    dataPnl.Controls.AddAt((i * itemColumnNb) + 1, HtmlTools.NewLabel(displayCol, HtmlTools.cssInfosColumnName));
                    dataPnl.Controls.AddAt((i * itemColumnNb) + 2, HtmlTools.NewLabel(aSortOrders[i].ToString().ToUpper(), HtmlTools.cssInfosOperator));
                    //
                    if ((i != (aSortExpressions.Length - 1)) || 
                        ((dataPnl.Controls.Count - 1) > ((aSortExpressions.Length * itemColumnNb) - 2)))
                    {
                        dataPnl.Controls.AddAt((i * itemColumnNb) + 3, HtmlTools.NewLabel(",", HtmlTools.cssInfosAnd));
                    }
                    #endregion
                }
            }

            // EG 201512 
            if (null != divInfos)
            {
                if (divInfos.HasControls())
                    ControlsTools.RemoveStyleDisplay(divInfos);
                else
                    divInfos.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
            if (null != lblRowsCountHeader)
                lblRowsCountFooter.Text = lblRowsCountHeader.Text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnRowCommand(Object sender, GridViewCommandEventArgs e)
        {
            //si l'event provient du click sur consulter le doc
            if (e.CommandName == "ConsultDoc")
            {
                if (false == isDataSourceAvailable)
                    SetDataSource();

                isLoadData = false;
                GridView gridView = (GridView)e.CommandSource;
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gridView.Rows[index];
                ConsultDoc(row.DataItemIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140630 [20101] Upd
        /// FI 20141021 [20350] Modify
        public void OnDeleting(Object sender, GridViewDeleteEventArgs e)
        {

            if (false == isDataSourceAvailable)
                SetDataSource();
            
            //isDataModified (-> enabled/disabled edit, sort, ... n'est utilisé qu'en mode grid edit)
            if (inputMode == Cst.DataGridMode.GridInput)
            {
                if (false == isDataModified)
                    PageIndexBeforeInsert = this.PageIndex;
                isDataModified = true;
            }
            else
                PageIndexBeforeInsert = this.PageIndex;
            
            // FI 20140630 [20101] la méthode GetDataSourceIndex n'est plus utilisée 
            //int sourceIndex = GetDataSourceIndex(e.Item);
            int sourceIndex = e.RowIndex;
            DataRowView drv = ((DataView)DataSource)[sourceIndex];
            int rowIndex = ((DataView)DataSource).Table.Rows.IndexOf(drv.Row);

            DataRow savDataRow = SaveDataRow(rowIndex);
            
            //Delete principal              
            DsData.Tables[0].Rows[rowIndex].Delete();

            // En mode grid edit, les lignes supprimées sont visibles (avec un status particulier)
            if (inputMode == Cst.DataGridMode.GridInput)
            {
                DataView dv = DsData.Tables[0].DefaultView;
                dv.RowStateFilter = DataViewRowState.CurrentRows | DataViewRowState.Deleted;
            }
                        
            //Delete EXTLID ,si existant
            if (referential.HasMultiTable && DsData.Tables.Count > 1)
            {
                for (int i = 1; i < DsData.Tables.Count; i++)
                {
                    string deletedID;
                    if (referential.IsDataKeyField_String)
                        deletedID = DataHelper.SQLString(DataKeys[e.RowIndex].ToString());
                    else
                        deletedID = this.DataKeys[e.RowIndex].ToString();
                    
                    //20060524 PL Add: CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
                    DsData.Tables[i].CaseSensitive = true;
                    DataRow[] extlRows = DsData.Tables[i].Select(" ID =" + deletedID);
                    if (extlRows.GetLength(0) > 0)
                        extlRows[0].Delete();// [0]: une seule ligne normalement.
                }
            }

            //en mode form grid; on enregistre immediatement les suppressions
            if (inputMode == Cst.DataGridMode.FormInput)
            {
                Cst.ErrLevel errLevel = SubmitChanges(false);
                if (errLevel == Cst.ErrLevel.SUCCESS)
                    SetRequestTrackBuilderItemProcess(RequestTrackProcessEnum.Remove, savDataRow);
            }
            else if (isGridInputMode)
                BindData();
            
            if (isLocked)
                OnCancelingEdit(sender, (CancelEventArgs) e);
            
            //20091002 FI Purge du cache en cas de suppression
            if (false == isGridInputMode)
                RepositoryTools.RemoveCache(DsData.Tables[0].TableName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUpdating(Object sender, GridViewUpdateEventArgs e)
        {
            if (false == isDataModified)
                PageIndexBeforeInsert = this.PageIndex;

            UpdateDataFromControl(sender, e);

            isLoadData = false;
            isDataModified = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnCancelingEdit(Object sender, CancelEventArgs e)
        {
            // Reset the edit mode for the current item
            this.editItemIndex = -1;

            // Refresh the grid
            this.BindData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnEditing(Object sender, GridViewEditEventArgs e)
        {
            // Set the current item to edit mode
            this.EditIndex = e.NewEditIndex;

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
        public void OnRowCreated(Object sender, GridViewRowEventArgs e)
        {
            // Get the newly created item
            DataControlRowType rowType = e.Row.RowType;
            int firstCol = nbColumnAction;
            #region isGridInputMode
            if (isGridInputMode)
            {
                lstGdvColumn.ForEach(column =>
                {
                    int i = lstGdvColumn.IndexOf(column);
                    ReferentialColumn rrc = referential.Column[column.columnId];
                    //On passe les colonnes DDL en mode wrap false
                    DataControlField dataControlField = Columns[i + nbColumnAction];
                    if (rrc.ExistsDDLType || rrc.ExistsRelation)
                        dataControlField.ItemStyle.Wrap = false;
                    //On force le resize de la cellule date car contient en mode edit le WCImgCalendar
                    else if (rrc.OtherGridControls != null && rrc.OtherGridControls.GetLength(0) > 0)
                    {
                        dataControlField.ItemStyle.Wrap = false;
                        if (TypeData.IsTypeDateTime(rrc.DataType.value))
                            dataControlField.ItemStyle.Width = new Unit(195);
                        else
                            dataControlField.ItemStyle.Width = new Unit(115);
                }

                });
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
                e.Row.TableSection = TableRowSection.TableHeader;
                if (rowType == DataControlRowType.Header)
                {
                    isInHeader_OnItemCreated = false;
                    isInBody_OnItemCreated = true;
                }
            }
            else if (isInFooter_OnItemCreated || (rowType == DataControlRowType.Footer))
            {
                isInFooter_OnItemCreated = true;
                e.Row.TableSection = TableRowSection.TableFooter;
                if (rowType == DataControlRowType.Pager)
                    isInFooter_OnItemCreated = false;
            }
            else
            {
                e.Row.TableSection = TableRowSection.TableBody;
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
            if (rowType == DataControlRowType.Header)
            {
                string tmp;
                isInHeader_OnItemCreated = false;
                for (int i = 0; i < Columns.Count; i++)
                {
                    // Adds a tooltip with the sort expression
                    TableCell cell = e.Row.Cells[i];
                    if (StrFunc.IsFilled(Columns[i].SortExpression))
                    {
                        cell.ToolTip = Ressource.GetString("SortBy") + @": ";
                        cell.ToolTip += Ressource.GetString(Columns[i].SortExpression, true).Replace(Cst.HTMLBreakLine, string.Empty);
                    }

                    // Draw the glyph to reflect sorting
                    if (IsSortedByThisField(Columns[i].SortExpression))
                    {
                        tmp = sortExpression;
                        bool isAsc = tmp.EndsWith(" asc");
                        tmp = Ressource.GetString(isAsc ? tmp.Replace(" asc", string.Empty) : tmp.Replace(" desc", string.Empty), true);
                        cell.ToolTip = Ressource.GetString("SortedBy") + ": " + tmp + (isAsc ? " asc" : " desc");
                        Panel imgSorted = new Panel();
                        imgSorted.CssClass = GetOrderSymbolImage();
                        imgSorted.Width = Unit.Pixel(10);
                        imgSorted.Height = Unit.Pixel(10);
                        cell.Controls.Add(imgSorted);
                    }
                    if (cell.Controls.Count > 0 && cell.Controls[0] != null)
                        ((WebControl)cell.Controls[0]).Enabled = (false == isLocked && false == isDataModified);
                }
            }
            #endregion

            #region HEADER
            if (rowType == DataControlRowType.Header)
            {
                int removedCol = 0;
                string header = string.Empty;
                List<GridViewColumn> sortedList = lstGdvColumn.OrderByDescending(column=>column.columnId).ToList();
                sortedList.ForEach(column =>
                {
                    int i = lstGdvColumn.IndexOf(column) + nbColumnAction;
                    ReferentialColumn rrc = referential.Column[column.columnId];
                    //On passe les colonnes DDL en mode wrap false
                    DataControlField dataControlField = Columns[i];
                    TableCell cell = e.Row.Cells[i];
                    header = cell.Text;
                    if (true == AllowSorting)
                        header = ((LinkButton)cell.Controls[0]).Text;

                    //PL 20120521 TEST
                    if (header.IndexOf("<brnobold/>") > 0)
                    {
                        int pos_brnobold = header.IndexOf("<brnobold/>");
                        string text_nobold = null;
                        if (true == AllowSorting)
                        {
                            text_nobold = ((LinkButton)cell.Controls[0]).Text.Substring(pos_brnobold);
                            ((LinkButton)cell.Controls[0]).Text = ((LinkButton)cell.Controls[0]).Text.Substring(0, pos_brnobold);
                        }
                        else
                        {
                            text_nobold = cell.Text.Substring(pos_brnobold);
                            Label label_bold = new Label();
                            label_bold.Text = cell.Text.Substring(0, pos_brnobold);
                            label_bold.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                            cell.Controls.Add(label_bold);
                        }
                        Label label_nobold = new Label();
                        label_nobold.Text = Cst.HTMLBreakLine + text_nobold;
                        label_nobold.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
                        cell.Controls.Add(label_nobold);
                    }

                    //On verifie si le header est de type : 1 header pour plusieurs cols
                    if (header.StartsWith(HEADERWITHMULTICOLUMN))
                    {
                        cell.Visible = false;
                        removedCol++;
                        if (rrc.ExistsRelation)
                            e.Row.Cells[i + 1].Visible = false;
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
                            e.Row.Cells[i].Width = Unit.Percentage(Math.Abs(rrc.GridWidth));
                            this.Width = Unit.Empty;
                        }
                        else
                            e.Row.Cells[i].Width = Unit.Percentage(rrc.GridWidth);
                    }
                    #endregion GridWithColumn

                });
            }

            if ((DataControlRowType.Pager != rowType) && (DataControlRowType.Separator != rowType) && (DataControlRowType.Footer != rowType))
            {
                lstGdvColumn.ForEach(column =>
                {
                    int i = lstGdvColumn.IndexOf(column) + nbColumnAction;
                    ReferentialColumn rrc = referential.Column[column.columnId];
                    TableCell cell = e.Row.Cells[i];
                    #region NoWrapColumn
                    if (rrc.NowrapSpecified)
                        e.Row.Cells[i].Wrap = (false == rrc.Nowrap);
                    else
                        e.Row.Cells[i].Wrap = false;
                    #endregion NoWrapColumn

                });
            }
            #endregion
        }
        /// <summary>
        /// Affichages des links sur l'ensemble des pages (AllowPagin = true)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void AddPagerInfo(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Pager)
            {
                TableCell pager = (TableCell)e.Row.Controls[e.Row.Controls.Count - 1];

                // Enumerates all the items in the pager...
                for (int i = 0; i < pager.Controls.Count; i++)
                {
                    Control ctrl = pager.Controls[i] as Control;
                    if (ctrl is LinkButton)
                    {
                        LinkButton lnk = ctrl as LinkButton;
                        lnk.Enabled = (false == isLocked && false == isDataModified);
                        if (lnk.Text != lnk.CommandArgument)
                            lnk.Text = (0 == i) ? "<<" : ">>";
                    }
                    else if (ctrl is Label)
                    {
                        ((Label)ctrl).Text = Ressource.GetString("Page") + " " + ((Label)ctrl).Text + " " + Ressource.GetString("of") + " " + this.PageCount.ToString();
                        ((Label)ctrl).Font.Bold = true;
                    }
                    else
                    {
                        //ctrl.Visible = false;
                    }
                }

                Panel pnlPager = new Panel();
                pnlPager.CssClass = "pagerGrid";

                #region Affichage du liens pour accès FirstPage, LastPage et GotoPage
                bool isDisplayButtonFirstAndLast = (PagerSettings.Mode == PagerButtons.Numeric) ? (this.PageCount > PagerSettings.PageButtonCount) : true;
                if (isDisplayButtonFirstAndLast)
                {
                    //hyperlink FirstPage
                    LinkButton lnk = new LinkButton();
                    lnk.Click += new EventHandler(SetFirstPage);
                    lnk.Text = Ressource.GetString("hlFirstPage");
                    lnk.Enabled = (!isLocked && !isDataModified);
                    pnlPager.Controls.Add(lnk);
                    //hyperlink LastPage
                    lnk = new LinkButton();
                    lnk.Click += new EventHandler(SetLastPage);
                    lnk.Text = Ressource.GetString("hlLastPage");
                    lnk.Enabled = (!isLocked && !isDataModified);
                    pnlPager.Controls.Add(lnk);
                }

                #region Ajout Affichage sur une page
                if ((1 < pager.Controls.Count) && (0 < totalRowscount) && (totalRowscount < 5000))
                {
                    LinkButton control = new LinkButton();
                    control.ID = "lnkDisabledPager";
                    control.Text = Ressource.GetString("disablePaging");
                    control.ToolTip = Ressource.GetString("disablePagingInformation");
                    control.Enabled = (false == isLocked);
                    pnlPager.Controls.Add(control);
                }
                #endregion Ajout Affichage sur une page

                if (isDisplayButtonFirstAndLast)
                {
                    //hyperlink GotoPage
                    LinkButton lnk = new LinkButton();
                    lnk.ID = "lblGoToPage" + e.Row.TableSection.ToString();
                    lnk.Click += new EventHandler(OnSetPageNumber);
                    lnk.Text = Ressource.GetString("hlGoToPage");
                    lnk.Enabled = (!isLocked && !isDataModified);
                    pnlPager.Controls.Add(lnk);
                    //textbox for Go to page
                    TextBox txt = new TextBox();
                    txt.Text = string.Empty;
                    txt.CssClass = EFSCssClass.DataGrid_txtSetPageNumber;
                    txt.ID = "txtSetPageNumber" + e.Row.RowType.ToString();
                    txt.Width = Unit.Pixel(30);
                    txt.BackColor = Color.Empty;
                    txt.Enabled = (!isLocked && !isDataModified);
                    pnlPager.Controls.Add(txt);

                    switch (e.Row.TableSection)
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
                Label lbl = new Label();
                lbl.ID = "lblRowsCount" + e.Row.TableSection.ToString();
                lbl.Style.Add("float", "right");
                pager.Controls.Add(lbl);
                switch (e.Row.TableSection)
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
        /// Affichages des informations de page (Rows number).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void AddFooterInfo(object sender, GridViewRowEventArgs e)
        {
            ShowFooter = false;
            if ((false == AllowPaging) && (e.Row.RowType== DataControlRowType.Footer))
            {
                ShowFooter = true;
                Panel pnlPager = new Panel();
                pnlPager.CssClass = "pagerGrid";

                //label Rows count
                Label lbl = new Label();
                lbl.ID = "lblRowsCount" + e.Row.TableSection.ToString();
                pnlPager.Controls.Add(lbl);
                switch (e.Row.TableSection)
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
                c2.ColumnSpan = Math.Max(1, Columns.Cast<DataControlField>().Sum(col => Convert.ToInt32(col.Visible)));
                e.Row.Cells.Clear();
                e.Row.Cells.Add(c2);
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

            CheckBox check = (CheckBox)sender;
            GridViewRow row = (GridViewRow)check.NamingContainer;

            int sourceIndex = row.DataItemIndex;
            if (false == isDataSourceAvailable)
                SetDataSource();

            if (false == isDataSourceAvailable)
                throw new Exception("DataSource is not Available");

            DataView dv = (DataView)DataSource;

            if (0 < dv.Count)
                dv[sourceIndex]["ISSELECTED"] = BoolFunc.IsTrue(check.Checked);

            // FI 20150120 [XXXXX] set DataSav à null puisqu'il y modification du datasource
            DataSav = null;
        }

        //20090408 EG TotalErrorStyle Sur ligne de total Balance agée (NTA non valorisé)
        // EG 20091011 Total for invoice
        // FI 20140926 [XXXXX] Modify
        // EG 20170223 [22883] Modify
        // EG 20170228 [22883] Modify
        private void OnRowDataBound(Object sender, GridViewRowEventArgs e)
        {
            bool isDataRow = (e.Row.RowType == DataControlRowType.DataRow);
            bool isAlternateRow = (e.Row.RowState == DataControlRowState.Alternate) && isDataRow;
            bool isNormalRow = (e.Row.RowState == DataControlRowState.Normal) && isDataRow;
            bool isEdit = (e.Row.RowState == DataControlRowState.Edit) && isDataRow;

            int indexColSQL_DataKeyField = referential.IndexColSQL_DataKeyField + nbColumnAction;

            #region Binding data
            string rowAttribut = string.Empty;
            int rowVersion = 0;

            RowHeaderDataBound(e);
            RowsCountDataBound(e);
            int groupByNumber = (isNormalRow || isAlternateRow) ? NormalOrAlternateRowDataBound(e) : 0;

            DataRowView drv = (DataRowView)e.Row.DataItem;

            if (isTemplateColumn)
            {
                #region Binding data to template column item (Note: a template column item can't be binded)
                if (isDataRow || isAlternateRow)
                {
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
                        if ("VW_ACCDAYBOOK" == referential.TableName)
                        {
                            rowAttribut = drv["ROWATTRIBUT"].ToString();
                            rowVersion = Convert.ToInt32(drv["ROWVERSION"]);
                        }
                        else if ("FLOWSRISKCTRL_ALLOC" == referential.TableName)
                        {
                            #region FLOWSRISKCTRL_ALLOC
                            if (drv.Row["CLASSRECORD"].ToString() == "3_TOT")
                            {
                                Nullable<CSS.Main> imgUrl = null;
                                if (drv.Row["RISKSTATUS"].ToString() == EFS.ACommon.ProcessStateTools.StatusEnum.SUCCESS.ToString())
                                {
                                    e.Row.CssClass = "DataGrid_ResultSuccess";
                                    imgUrl = CSS.Main.tooltipsuccess;
                                }
                                else if (drv.Row["RISKSTATUS"].ToString() == EFS.ACommon.ProcessStateTools.StatusEnum.WARNING.ToString())
                                {
                                    e.Row.CssClass = "DataGrid_ResultWarning";
                                    imgUrl = CSS.Main.tooltipwarning;
                                }
                                else
                                {
                                    e.Row.CssClass = "DataGrid_ResultError";
                                }
                                if (imgUrl.HasValue)
                                {
                                    Panel img = new Panel();
                                    img.CssClass = CSS.SetCssClass(imgUrl.Value);
                                    TableCell cell = e.Row.Cells[1];
                                    cell.Controls.RemoveAt(0);
                                    cell.Controls.Add(img);
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

                    int i = 0;
                    lstGdvColumn.ForEach(column =>
                    {
                        i = lstGdvColumn.IndexOf(column);

                        Control ctrlFound = e.Row.FindControl(column.columnName);
                        if (ctrlFound != null)
                        {
                            WebControl webCtrlFound = ctrlFound as WebControl;
                            WebControl webCtrlFoundParent = ctrlFound.Parent as WebControl;

                            // EG 20170223 [22883] Recherche du nbr de décimale en fonction du contract/asset
                            Nullable<int> scale = null; //0 Decimale par défaut => Semblant de compatibilité ascendante
                            
                            if (column.IsScaleQuantity)
                            {
                                scale = 0; //0 Decimale par défaut => Semblant de compatibilité ascendante
                                try
                                {
                                    if (groupByNumber > 0) // EG 20170228 [22883] gestion des lignes de regroupement 
                                    {
                                        DataRow[] rowsDetail = LoadRowsDetail(drv.Row);
                                        if (ArrFunc.IsFilled(rowsDetail))
                                        {
                                            scale = 0;
                                            Decimal[] decValues =
                                                (from item in rowsDetail
                                                 select Convert.ToDecimal(item[column.columnName] == Convert.DBNull ? 0 : item[column.columnName])).ToArray();
                                            foreach (Decimal item in decValues)
                                            {
                                                int currentScale = DecFunc.PrecisionOf(item);
                                                if (currentScale > scale)
                                                    scale = currentScale;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        scale = GetQtyScale(referential, drv.Row);
                                        if (false == scale.HasValue)
                                            scale = 0; //0 Decimale par défaut => Semblant de compatibilité ascendante

                                    }
                                }
                                catch // A conserver 3 mois => A supprimer que toutes les corrections seront appliquées  
                                {
#if DEBUG
                                    throw;
#else
                    scale = 0;
#endif
                                }
                            }

                            #region Getting data
                            string data = GetAndFormatData(column, drv, scale);
                            #endregion Getting data

                            #region Aggregate Label
                            if (groupByNumber > 0)
                            {
                                string aggregateFunction = string.Empty;
                                if (column.aggregateSpecified)
                                {
                                    Control ctrlAggregateLabel = e.Row.FindControl(column.columnName + RepositoryTools.SuffixAggregate);
                                    if (ctrlAggregateLabel != null && column.aggregateSpecified)
                                        SetDataToControl(ctrlAggregateLabel, Ressource.GetString(column.aggregate, true) + ": ", string.Empty, string.Empty, true);// RD 20100309 / Automatic Compute identifier                                 
                                }
                            }
                            #endregion Aggregate Label
                            
                            #region Column Image
                            if (TypeData.IsTypeImage(column.dataType))
                                data = "Image";
                            #endregion Column Image

                            #region Column CellStyle
                            if (false == column.cellStyleSpecified)
                            {
                                if ((RessourceTypeEnum.Empty == column.ressourceType)
                                    && (last_I == i - 1)
                                    && !EFSCssClass.IsUnknown(last_CSSClass))
                                {
                                    //Cas d'une colonne link/slave (cad sans ressource) --> Application du style de la colonne précédente
                                    webCtrlFound.CssClass = last_CSSClass.ToString();
                                    webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                }
                                else
                                {
                                    int next_i = i + 1;
                                    if (next_i < lstGdvColumn.Count)
                                    {
                                        GridViewColumn gdvColumnNext = lstGdvColumn[next_i];
                                        if ((gdvColumnNext.ressourceType == RessourceTypeEnum.Empty) && gdvColumnNext.cellStyleSpecified)
                                    {
                                        //Cas d'une colonne link/master (cad dont la suivante est sans ressource) --> Application du style de la colonne suivante
                                            if (gdvColumnNext.cellStyle.modelSpecified)
                                        {
                                                if (gdvColumnNext.cellStyle.model.Contains("side") ||
                                                    ((gdvColumnNext.cellStyle.model.StartsWith("quantity") || ((rowAttribut == "T") && (rowVersion == 0)))
                                                && (referential.ExistsColumnISSIDE)))
                                            {
                                                //RAS
                                            }
                                            else
                                            {
                                                    string next_data = GetAndFormatData(gdvColumnNext, drv, null);
                                                    EFSCssClass.CssClassEnum next_cssClass =
                                                        RepositoryTools.GetCssClassForModel(gdvColumnNext.cellStyle.model, next_data);
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
                            }
                            else
                            {
                                try
                                {
                                    // RD 20110704 [17501]
                                    // Utilisation d'une nouvelle méthode ReferentialTools.GetCssClassForModel()
                                    if (column.cellStyle.modelSpecified)
                                    {
                                        // Gérer le cas particulier de la présence d'une colonne Side et Quantity sur la même ligne: 
                                        // pour avoir le même model pour les deux colonnes, c'est à dire ce lui de la colonne Side
                                        if (column.cellStyle.model.Contains("side"))
                                        {
                                            #region
                                            sideCSSClass = RepositoryTools.GetCssClassForModel(column.cellStyle.model, data);
                                            webCtrlFound.CssClass = sideCSSClass.ToString();
                                            webCtrlFoundParent.CssClass = webCtrlFound.CssClass;
                                            #endregion
                                        }
                                        else if ((column.cellStyle.model.StartsWith("quantity") || ((rowAttribut == "T") && (rowVersion == 0)))
                                            && (referential.ExistsColumnISSIDE))
                                        {
                                            #region
                                            if (EFSCssClass.IsUnknown(sideCSSClass))
                                            {
                                                // Retrouver la colonne ISSIDE et son model
                                                GridViewColumn columnIsSide = lstGdvColumn[referential.IndexColSQL_ISSIDE];
                                                sideCSSClass = RepositoryTools.GetCssClassForModel(columnIsSide.cellStyle.model, drv[columnIsSide.columnName].ToString());
                                            }
                                            if (false == EFSCssClass.IsUnknown(sideCSSClass))
                                            {
                                                if (column.cellStyle.model.StartsWith("quantity_light"))
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
                                            EFSCssClass.CssClassEnum cssClass = RepositoryTools.GetCssClassForModel(column.cellStyle.model, data);
                                            if (false == EFSCssClass.IsUnknown(cssClass))
                                            {
                                                webCtrlFound.CssClass = cssClass.ToString();
                                                webCtrlFoundParent.CssClass = webCtrlFound.CssClass;

                                                last_CSSClass = cssClass;
                                                last_I = i;

                                            }
                                        }
                                    }
                                    else if (column.cellStyle.typeSpecified)
                                    {
                                        string columnStyle = RepositoryTools.GetCssClassFromDataValue(column.cellStyle, data);
                                        #region class
                                        if ("class" == column.cellStyle.type)
                                        {
                                            webCtrlFound.CssClass = columnStyle;
                                            ((WebControl)ctrlFound.Parent).CssClass = webCtrlFound.CssClass;
                                        }
                                        #endregion
                                        #region style
                                        else if ("style" == column.cellStyle.type)
                                        {
                                            webCtrlFound.Style.Value = columnStyle;
                                            ((TableCell)ctrlFound.Parent).Style.Value = webCtrlFound.Style.Value;
                                        }
                                        #endregion
                                    }
                                }
                                catch { }
                            }
                            #endregion Column CellStyle

                            #region Column RowStyle
                            if (column.rowStyleSpecified)
                            {
                                string columnRowCSSStyle = string.Empty;
                                try
                                {
                                    if (column.rowStyle.modelSpecified)
                                    {
                                        // RD 20110704 [17501]
                                        EFSCssClass.CssClassEnum cssClass = RepositoryTools.GetCssClassForModel(column.rowStyle.model, data);
                                        if (false == EFSCssClass.IsUnknown(cssClass))
                                            columnRowCSSStyle = cssClass.ToString();
                                    }
                                    else
                                    {
                                        columnRowCSSStyle = RepositoryTools.GetCssClassFromDataValue(column.rowStyle, data);
                                    }
                                    //
                                    if (column.rowStyle.typeSpecified && StrFunc.IsFilled(columnRowCSSStyle))
                                    {
                                        if ("class" == column.rowStyle.type)
                                            e.Row.CssClass = columnRowCSSStyle;
                                        else if ("style" == column.rowStyle.type)
                                            e.Row.Style.Value = columnRowCSSStyle;
                                    }
                                }
                                catch { }
                            }
                            #endregion Column RowStyle

                            #region Column Status [Warning: ne pas déplacer en dessous de "IsResource"]
                            if (column.columnName.StartsWith("IDST") || (column.columnName.IndexOf("_IDST") > 0))
                            {
                                try
                                {
                                    ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                                    ExtendEnum extendEnum = null;

                                    if (column.columnName.IndexOf("IDSTENVIRONMENT") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusEnvironment.ToString()];
                                    //PL 20100930 (Non testé)
                                    else if (column.columnName.IndexOf("IDSTBUSINESS") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusBusiness.ToString()];
                                    else if (column.columnName.IndexOf("IDSTACTIVATION") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusActivation.ToString()];
                                    else if (column.columnName.IndexOf("IDSTPRIORITY") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusPriority.ToString()];
                                    else if (column.columnName.IndexOf("IDSTCHECK") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusCheck.ToString()];
                                    else if (column.columnName.IndexOf("IDSTMATCH") >= 0)
                                        extendEnum = ListEnumsSchemes[StatusEnum.StatusMatch.ToString()];

                                    if (extendEnum != null)
                                    {
                                        ExtendEnumValue extendEnumValue = extendEnum.GetExtendEnumValueByValue(data);
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
                            #endregion Column Status [Warning: ne pas déplacer en dessous de "IsResource"]

                            #region Column IsResource
                            if (column.resourceSpecified)
                            {
                                string resource = data;
                                if (StrFunc.IsFilled(resource) && column.resource.prefixSpecified)
                                    resource = column.resource.prefix + resource;

                                if (false == column.resource.sqltableSpecified)
                                {
                                    //PL 20120412 Replace GetStringByRef by GetMultiByRef
                                    if (Ressource.GetMultiByRef(resource, 1, ref resource))
                                    {
                                        data = resource;
                                    }
                                    else if (0 <= column.columnName.IndexOf("MATFMT_"))
                                    {
                                        data = GetFormattedMaturity(column.columnName, data);
                                    }
                                }
                                else if (StrFunc.IsFilled(resource))
                                {
                                    //PL 20120518 New feature
                                    if (OTCmlHelper.GetRessource(SessionTools.CS, column.resource.sqltable, resource, ref resource))
                                        data += " [Res: " + resource + "]";
                                }
                            }
                            #endregion Column IsResource

                            #region Column VALORISATION_FMT
                            if (column.columnName.EndsWith("VALORISATION_FMT"))
                            {
                                #region RoundPrec application by Currency for Amount/Qty/Rate
                                ReferentialColumn rrc1 = referential["UNITTYPE"];
                                ReferentialColumn rrc2 = referential["UNIT"];
                                if ((null != rrc1) && (null != rrc2))
                                    data = FormattingValorisation(drv[rrc1.ColumnName].ToString(), data, drv[rrc2.ColumnName].ToString());
                                #endregion RoundPrec application by Currency for Amount/Qty
                            }
                            #endregion Column VALORISATION_FMT

                            #region Column IsTRIM
                            if (column.isTRIM)
                            {
                                string opCulture;
                                data = RepositoryTools.GetDataTRIM(data, out opCulture);
                            }
                            #endregion Column IsTRIM

                            #region Column IsDenominator ou IsIdent
                            if (column.ressourceType == RessourceTypeEnum.Denominator)
                                {
                                    if (StrFunc.IsFilled(data))
                                    {
                                        try
                                        {
                                            if (Convert.ToDecimal(data) == 1)
                                                data = string.Empty;
                                            else
                                                data = @"/" + Cst.HTMLSpace + data;
                                        }
                                        catch { }
                                    }
                            }
                            #endregion Column IsDenominator ou IsIdent

                            #region MultiLine
                            //20071102 PL A terminer...
                            //								if (TypeData.IsTypeText(alColumnDataType[i].ToString()))
                            //									data = data.Replace(Cst.CrLf, Cst.HTMLBreakLine);	
                            #endregion

                            #region Length in Gridview
                            string fullData = null;
                            if (0 < column.lengthInGridView)
                            {
                                if (column.lengthInGridView < data.Length)
                                {
                                    data = data.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                                    data = data.Replace(Cst.Lf, Cst.HTMLBreakLine);
                                    data = data.Replace(Cst.Tab, Cst.HTMLSpace4);
                                    fullData = data;
                                }

                                data = RepositoryTools.FormatLongData(data, column.lengthInGridView);
                            }
                            #endregion Length in Gridview

                            //Binding data to column control
                            // EG 20160224 New Gestion CSS complex sur Cellule (REQUESTTYPE)
                            if (column.cellStyleSpecified && column.cellStyle.complexModelSpecified)
                            {
                                RepositoryTools.SetCssClassForComplexModel(column.cellStyle.complexModel, ctrlFound,
                                    column.cellStyle.versionSpecified ? column.cellStyle.version : string.Empty, data);
                            }
                            else
                            {
                                SetDataToControl(ctrlFound, data, column.defaultValue, fullData, true);

                            }

                        }
                    });
                }
                #endregion

                #region Binding data to edit item (Note: an edit item can't be binded)
                if (e.Row.RowState == DataControlRowState.Edit)
                {
                    lstGdvColumn.ForEach(column =>
                    {
                        Control ctrlFound = e.Row.FindControl(column.columnName + RepositoryTools.SuffixEdit);
                        if (ctrlFound != null)
                        {
                            //Getting data
                            string data;
                            if (column.columnType == GridViewColumnType.relation)
                                data = drv[lstGdvColumn[lstGdvColumn.IndexOf(column) - 1].columnName].ToString();
                            else
                                data = drv[column.columnName].ToString();

                            //particular formats
                            if (TypeData.IsTypeBool(column.dataType))
                                data = (BoolFunc.IsTrue(data) ? "TRUE" : "FALSE");
                            else if (TypeData.IsTypeDate(column.dataType) && StrFunc.IsFilled(data))
                                data = Convert.ToDateTime(data).ToString("d");
                            else if (TypeData.IsTypeDec(column.dataType) && StrFunc.IsFilled(data))
                            {
                                string decFormat = "d";
                                if (column.scaleSpecified)
                                    decFormat = "f" + column.scale.ToString();
                                data = Convert.ToDecimal(data).ToString(decFormat);
                            }

                            //set hidden if new record and is EXTLID
                            ReferentialColumn rrc = referential[column.columnId];
                            if (rrc.IsExternal && StrFunc.IsEmpty(this.DataKeys[e.Row.RowIndex].ToString()))
                                ctrlFound.Visible = false;

                            bool isEnabled = ((!rrc.IsForeignKeyField) || StrFunc.IsEmpty(referential.valueForeignKeyField));
                            if (!(StrFunc.IsEmpty(this.DataKeys[e.Row.RowIndex].ToString())))
                            {
                                //Enabled if IsUpdatable
                                isEnabled &= (rrc.IsUpdatableSpecified && rrc.IsUpdatable.Value);
                            }
                            //Binding data to column control
                            // RD 20100309 / Automatic Compute identifier
                            SetDataToControl(ctrlFound, data, column.defaultValue, isEnabled);
                    }

                    });

                    //
                    // Enabled the delete item for edit line
                    if ((isGridInputMode || isFormInputMode) && (referential.Remove))
                    {
                        WebControl wc = ((WebControl)e.Row.Cells[deleteItemIndex]);
                        LinkButton linkButton = null;
                        if (wc.Controls[0] == null)
                            throw new Exception();
                        linkButton = (LinkButton)wc.Controls[0];
                        linkButton.ToolTip = Ressource.GetString("imgRemove");
                        linkButton.CssClass = CSS.SetCssClass(CSS.Main.remove);
                        wc.Attributes["onclick"] = "return true;";
                    }
                }
                #endregion Binding data to edit item (Note: an edit item can't be binded)
            }
            else
            {
                if (isDataRow || isAlternateRow)
                {
                    if (("VW_AGEINGBALANCEDET" == referential.TableName) || ("VW_INVOICEFEESDETAIL" == referential.TableName))
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
                        e.Row.CssClass = EFSCssClass.DataGrid_ItemStyle;
                        if ((rowAttribut == "T") && (rowVersion <= 0))//T --> Total
                        {
                            foreach (TableCell cell in e.Row.Cells)
                            {
                                cell.CssClass = EFSCssClass.TotalConsult;
                                if (-3 == rowVersion)
                                    cell.CssClass = EFSCssClass.TotalConsult2;
                            }
                            if (("VW_AGEINGBALANCEDET" == referential.TableName) &&
                                (Convert.IsDBNull(drv.Row["INIT_NTA_AMOUNT"]) || (0 == Convert.ToDecimal(drv.Row["INIT_NTA_AMOUNT"]))))
                                e.Row.CssClass = EFSCssClass.DataGrid_AlternatingTotalErrorStyle;
                            else if (("VW_INVOICEFEESDETAIL" == referential.TableName) &&
                                (Convert.IsDBNull(drv.Row["NETACCOUNTING_AMOUNT"])) && (0 == rowVersion))
                                e.Row.CssClass = EFSCssClass.DataGrid_AlternatingTotalErrorStyle;
                            else
                                e.Row.CssClass = EFSCssClass.DataGrid_AlternatingItemStyle;
                        }
                        #endregion
                    }
                    else if (("VW_AGEINGBALANCE" == referential.TableName) &&
                            (Convert.IsDBNull(drv.Row["INIT_NTA_AMOUNT"]) || (0 == Convert.ToDecimal(drv.Row["INIT_NTA_AMOUNT"]))))
                        e.Row.CssClass = EFSCssClass.DataGrid_TotalErrorStyle;
                    else
                    {
                        int indexERRORLINE = referential.GetIndexColSQL("CSS_ERRORLINE");
                        if ((-1 < indexERRORLINE) && Convert.ToBoolean(drv.Row["CSS_ERRORLINE"]))
                            e.Row.CssClass = EFSCssClass.DataGrid_TotalErrorStyle;
                    }
                }
            }
            #endregion Binding data

            if (isDataRow || isAlternateRow)
            {
                if (lstGdvColumn.Exists(column => TypeData.IsTypeBool(column.dataType) || isHyperLinkAvailable))
                    FinalizeDisplayForBoolean_Compute_Hyperlink(e);

                if (lstGdvColumn.Exists(column => column.isEditable))
                    FinalizeDisplayPermanentEditableColumn(e);
            }
            
            #region Data item
            if (isDataRow || isAlternateRow)
            {
                bool isDeletedItem = false;
                #region isGridInputMode
                if (isGridInputMode && referential.Create)
                {
                    WebControl wc = ((WebControl)e.Row.Cells[newItemIndex]);
                    if (wc.Controls[0] == null)
                        throw new Exception();
                    //
                    string status = string.Empty;
                    string toolTip = string.Empty;
                    switch (((DataView)DsData.Tables[0].DefaultView)[e.Row.DataItemIndex].Row.RowState)
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

                //getting FK-Value for double click use
                //FI 20130208 mise en commentaire, il ne faut pas écraser la donnée valueFK 
                //Pour lire cette donner il faut utiliser la méthode GetItemValueFK 
                //ValueFK doit être la donnée que arrive de l'URL
                //if (columnFK.Length > 0)
                //{
                //    //FI 20110101 Add FormatToInvariant
                //    valueFK = GetCellValue(e.Item, referential.IndexColSQL_ForeignKeyField + nbColumnAction);
                //    valueFK = FormatToInvariant(valueFK, referential[referential.IndexForeignKeyField].DataType);
                //}

                ModifyActionDataBound(e.Row, rowVersion, rowAttribut, isDeletedItem);
                RemoveActionDataBound(e.Row, isDeletedItem);
                DblClickDataBound(e);
                //}
                //
                #region CssClass
                if (isDeletedItem)
                {
                    e.Row.CssClass = "DataGrid_DeletedItemStyle";
                }
                else
                {
                    bool isDisabledItem = false;
                    #region dtEnabled/dtDisabled
                    if (referential.ExistsColumnsDateValidity)
                    {
                        string dtEnabled = null, dtDisabled = null;
                        for (int i_tmp = Columns.Count - 1; i_tmp >= nbColumnAction; i_tmp--)//Tip: Les colonnes DTENABLED et DTDISABLED sont proches de la fin
                        {
                            if (lstGdvColumn[i_tmp - nbColumnAction].columnName == "DTENABLED")
                            {
                                dtEnabled = GetCellValue(e.Row, i_tmp);
                                break;//Tip: La colonne DTENABLED est tjs avant la colonne DTDISABLED
                            }
                            else if (lstGdvColumn[i_tmp - nbColumnAction].columnName == "DISABLED")
                            {
                                dtDisabled = GetCellValue(e.Row, i_tmp);
                            }
                        }

                        DateTime dtTmp;
                        if (DateTime.TryParse(dtEnabled, out dtTmp))
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
                    #endregion

                    if (isAlternateRow)
                    {
                        if (isDisabledItem)
                            e.Row.CssClass = "DataGrid_DisabledAlternatingItemStyle";
                        else if (StrFunc.IsEmpty(e.Row.CssClass))
                        {
                            // 20090818 RD / Pour forcer l'application du style 
                            if (StrFunc.IsEmpty(e.Row.Style.Value))
                                e.Row.CssClass = AlternatingRowStyle.CssClass;
                            else
                                e.Row.CssClass = "Unknown";
                        }
                    }
                    else
                    {
                        if (isDisabledItem)
                            e.Row.CssClass = "DataGrid_DisabledItemStyle";
                        else if (StrFunc.IsEmpty(e.Row.CssClass))
                        {
                            // 20090818 RD / Pour forcer l'application du style 
                            if (StrFunc.IsEmpty(e.Row.Style.Value))
                                e.Row.CssClass = RowStyle.CssClass;
                            else
                                e.Row.CssClass = "Unknown";
                        }
                    }
                }
                #endregion
            }
            #endregion Data item

            #region CheckBox selected
            // EG 20140120 [20683] Masque le check de sélection sur ligne obsolete
            if ((("MCO" == referential.TableName) || ("VW_MCO" == referential.TableName)) && (isDataRow || isAlternateRow) && IsCheckboxColumn)
            {
                TableCell cellSelect = e.Row.Cells[1];
                WCCheckBox2 chkSelect = (WCCheckBox2)cellSelect.Controls[0];
                string columnName = (referential.AliasTableNameSpecified ? referential.AliasTableName + "_" : string.Empty) + "DTOBSOLETE";
                if ((drv.DataView.Table.Columns.Contains(columnName)))
                {
                    chkSelect.Visible = Convert.IsDBNull(drv[columnName]);
                    if (false == Convert.IsDBNull(drv[columnName]))
                    {
                        chkSelect.Checked = false;
                        ((DataView)DataSource)[e.Row.DataItemIndex]["ISSELECTED"] = false;
                    }
                }
            }
            #endregion CheckBox selected
        }
        /// <summary>
        ///  Retourne la donnée formattée
        /// </summary>
        /// <param name="pColumn">Colonne à traiter pDataRowView[pColumn.columnName] = objet à formater</param>
        /// <param name="pDataRowView">Représente le DataRowView à formater</param>
        /// <param name="pDecScaleSubstitute">Nbr de digit pour les données de type décimale
        /// <para>Lorsque renseigné Remplace le Nbr de digit par défaut stipulé sur la colonne</para></param>
        /// <returns></returns>
        // PL 20150722 New method - Externalisation du code issu de la section "Getting" datade la méthode "OnItemDataBound"
        // EG 20170223 [22883] Modify (Modification de signature. Nouveau paramètre  pDecScaleSubstitute)
        private string GetAndFormatData(GridViewColumn pColumn, DataRowView pDataRowView, Nullable<int> pDecScaleSubstitute)
        {
            object obj = pDataRowView[pColumn.columnName];
            string pData = obj.ToString();

            // RD 20150911 [21327] Récupérer d'abord la valeur du Décimal avant de la transformer en string
            // Pour les petit décimaux (en dessous de 0.0001):
            // le fait de convertir directement le contenu de la colonne en String, produit un décimal en notation scientifique
            // (Exemple: 0.00001 donne 1E-05)
            // ce qui provoque une erreur lors du formatage
            if (StrFunc.IsFilled(pData) && TypeData.IsTypeDec(pColumn.dataType))
            {
                decimal decValue = Convert.ToDecimal(obj);
                pData = decValue.ToString();
            }

            //FI 20120306 <Multi-Books> si book non présent
            if (consult.IsConsultation(LstConsultData.ConsultEnum.MCO_RPT))
            {
                if ((pColumn.columnName == "b_stp_IDENTIFIER") && StrFunc.IsEmpty(pData))
                    pData = "&lt;Multi-Books&gt;";
            }
            else if ((consult.IdLstConsult == "REF-ProcessBaseTRADE_RIMGEN"))
            {
                if ((pColumn.columnName == "bsendto_IDENTIFIER") && StrFunc.IsEmpty(pData))
                    pData = "&lt;Multi-Books&gt;";
            }
            try
            {
                if (TypeData.IsTypeBool(pColumn.dataType))
                    pData = (BoolFunc.IsTrue(pData) ? "TRUE" : "FALSE");
                //
                // 20090925 RD / Gérer le format d'affichage des types INT, TIME et DATETIME
                if (StrFunc.IsFilled(pData))
                {
                    if (TypeData.IsTypeDateTime(pColumn.dataType))
                        pData = Convert.ToDateTime(pData).ToString("G");
                    else if (TypeData.IsTypeDate(pColumn.dataType))
                        pData = Convert.ToDateTime(pData).ToString("d");
                    else if (TypeData.IsTypeTime(pColumn.dataType))
                        pData = Convert.ToDateTime(pData).ToString("T");
                    // 20090925 RD / Attention de ne pas formater les colonnes DataKeyFild
                    //FI 20120111 Il faut formater les colonnes Int (Suppression du commentaire)
                    else if (TypeData.IsTypeInt(pColumn.dataType))
                    {
                        if (pColumn.columnName != "ROWVERSION")//rowversion ne peut être cast in int32
                        {
                            //FI 20120207 Lorque la colonne est de type string
                            //Spheres® considère que les données sont en invariant culture
                            //C'est le cas des données issues d'extraction XML
                            //Exemple d'expression de colonne: extractvalue(t_lsd.TRADEXML,'(efs:EfsML/trade/efs:exchangeTradedDerivative/fixml:FIXML/fixml:TrdCaptRpt/@LastQty)[0]')
                            if (obj.GetType().Equals(typeof(String)))
                                pData = StrFunc.FmtDecimalToCurrentCulture(pData);

                            // EG 20150920 [21314] Int (int32) to Long (Int64) 
                            long intvalue = IntFunc.IntValue64(pData);
                            pData = intvalue.ToString("n0");
                        }
                    }
                    else if (TypeData.IsTypeDec(pColumn.dataType))
                    {
                        // 20090925 RD / Pour afficher les séparateurs de milliers 
                        int decimalDigits = -1;

                        if (pDecScaleSubstitute.HasValue) // EG 20170223 [22883] Modify
                            decimalDigits = pDecScaleSubstitute.Value;
                        else if (pColumn.scaleSpecified)
                            decimalDigits = pColumn.scale;

                        if (decimalDigits > -1)
                            pData = StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pData), decimalDigits);
                        else
                            pData = StrFunc.FmtAmountToGUI(StrFunc.FmtDecimalToInvariantCulture(pData));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    StrFunc.AppendFormat("ERROR on formating pData:{0} [column {1}]", pData, pColumn.columnName, ex));
            }
            return pData;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void RowHeaderDataBound(GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.Header && IsCheckboxColumn)
            {
                // En supposant que la colonne "Select" est en deuxième position des colonnes dites "ColumnAction" ( voir AddColumnAction())
                TableCell cellSelectAll = e.Row.Cells[1];
                CheckBox cbSelectAll = (CheckBox)cellSelectAll.Controls[0];

                cbSelectAll.Visible = (totalRowscount > 0);
                if (totalRowscount > 0)
                {
                    int firstRowIndex = this.PageSize * this.PageIndex;
                    int lastRowIndex = (totalRowscount > this.PageSize * (PageIndex + 1) ? this.PageSize * (PageIndex + 1) - 1 : totalRowscount - 1);

                    bool isNotSelectedRowExist = false;

                    for (int i = firstRowIndex; i <= lastRowIndex; i++)
                    {
                        isNotSelectedRowExist = BoolFunc.IsFalse(DsData.Tables[0].Rows[i]["ISSELECTED"]);
                        if (isNotSelectedRowExist)
                            break;
                    }

                    cbSelectAll.Checked = (false == isNotSelectedRowExist);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20160211 New Pager
        private void RowsCountDataBound(GridViewRowEventArgs e)
        {

            Label lblRowsCount = (Label)e.Row.FindControl("lblRowsCount" + e.Row.TableSection.ToString());

            if (e.Row.RowType == DataControlRowType.Header)
                lblRowsCount = lblRowsCountHeader;
            else if (e.Row.RowType == DataControlRowType.Footer)
                lblRowsCount = lblRowsCountFooter;

            if (null != lblRowsCount)
            {
                int lastRows = PageSize * PageIndex;
                int currentDisplayedRows = (isLastPage ? totalRowscount - lastRows : this.PageSize);
                if (totalRowscount == 0)
                    lblRowsCount.Text = string.Empty;
                else if (totalRowscount == 1 || PageSize == 1)
                    lblRowsCount.Text = "<b>" + Ressource.GetString("lblRowsCount_Row") + " " +
                        Convert.ToString(lastRows + 1) + "</b> " + Ressource.GetString("of") + " " + totalRowscount.ToString();
                else
                    lblRowsCount.Text = "<b>" + Ressource.GetString("lblRowsCount_Rows") + " " +
                        Convert.ToString(lastRows + 1) + " " + Ressource.GetString("lblRowsCount_To") + " " +
                        Convert.ToString(isLastPage ? totalRowscount : (lastRows + this.PageSize)) + "</b> (" +
                        Convert.ToString(currentDisplayedRows) + " " + Ressource.GetString("of") + " " + totalRowscount.ToString() + ")";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private int NormalOrAlternateRowDataBound(GridViewRowEventArgs e)
        {
            int groupByNumber = 0;
            DataRowView drv = (DataRowView)e.Row.DataItem;
            if ((drv.DataView.Table.Columns.Contains("GROUPBYNUMBER")))
                groupByNumber = (null != drv["GROUPBYNUMBER"] ? Convert.ToInt32(drv["GROUPBYNUMBER"]) : 0);

            #region Column RowNumber
            try
            {
                // En supposant que la colonne "RowNumber" est en première position des colonnes dites "ColumnAction" ( voir AddColumnAction())
                TableCell cellRowNumber = e.Row.Cells[0];
                int rowNumber = this.PageSize * this.PageIndex + e.Row.RowIndex + 1;
                cellRowNumber.Text = Convert.ToString(rowNumber);
            }
            catch (Exception ex)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    "Unexpected error for column RowNumber", ex);
            }
            #endregion Column RowNumber

            #region Column Loupe
            try
            {
                if (_isLoupeColumn ) //&& (0 < groupByNumber))
                {
                    TableCell cellLoupe = null;
                    if (IsCheckboxColumn)
                        cellLoupe = e.Row.Cells[2];
                    else
                        cellLoupe = e.Row.Cells[1];

                    //cellLoupe.CssClass = "img loupe";

                    LinkButton c1 = (LinkButton)cellLoupe.Controls[0];
                    if (0 < groupByNumber)
                    {
                        c1.CssClass = CSS.SetCssClass(CSS.Main.groupby);
                        c1.ToolTip = string.Empty;
                    }
                    else
                    {
                        //c1.CssClass = CSS.SetCssClass(CSS.Main.loupe);
                        //c1.ToolTip = Ressource.GetString(referential.Modify ? "imgEdit" : "imgView");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    "Unexpected error for column magnifying glass", ex);
            }
            #endregion Column Loupe

            #region Column Column AttachedDOC
            try
            {
                if (isInputMode && this.referential.TableName.StartsWith(Cst.OTCml_TBL.ATTACHEDDOC.ToString()))
                {
                    TableCell cellAttachedDoc = null;
                    if (IsCheckboxColumn)
                        cellAttachedDoc = e.Row.Cells[4];
                    else
                        cellAttachedDoc = e.Row.Cells[3];

                    LinkButton c1 = (LinkButton)cellAttachedDoc.Controls[0];
                    c1.CssClass = CSS.SetCssClass(CSS.Main.consult);
                    c1.ToolTip = Ressource.GetString("imgConsultDoc");
                }
            }
            catch (Exception ex)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    "Unexpected error for column magnifying glass", ex);
            }
            #endregion Column AttachedDOC

            // EG 20121122 Add Background on RowState (Image via CSS)
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
                    TableCell cellRowState = e.Row.Cells[nbColumnAction - 1];
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
                            //
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
                                            else if (isLabelBackground)
                                            {
                                                WCTooltipLabel lblRowState = rowStateCtrl as WCTooltipLabel;
                                                if (null != lblRowState)
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
                                                WCToolTipButton btnRowState = rowStateCtrl as WCToolTipButton;
                                                if (null != btnRowState)
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
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    "Unexpected error for column Raw State", ex);
            }

            #endregion Column RowState

            #region Apply RowStyle
            try
            {
                string rowStyle = string.Empty;
                if (drv.DataView.Table.Columns.Contains("ROWSTYLE"))
                    rowStyle = (null != drv["ROWSTYLE"] ? drv["ROWSTYLE"].ToString() : string.Empty);
                //
                if (StrFunc.IsFilled(rowStyle))
                {
                    if ("class" == referential.SQLRowStyle.type)
                        e.Row.CssClass = rowStyle;
                    else if ("style" == referential.SQLRowStyle.type)
                        e.Row.Style.Value = rowStyle;
                }
                else if (0 < groupByNumber)
                {
                    if (groupByNumber == 1)
                        e.Row.CssClass = EFSCssClass.DataGrid_GroupStyle1;
                    else if (groupByNumber == 2)
                        e.Row.CssClass = EFSCssClass.DataGrid_GroupStyle2;
                    else
                        e.Row.CssClass = EFSCssClass.DataGrid_GroupStyle;
                }
            }
            catch (Exception ex)
            {
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    "Unexpected error when applying style", ex);
            }
            #endregion  Apply RowStyle
            return groupByNumber;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void FinalizeDisplayForBoolean_Compute_Hyperlink(GridViewRowEventArgs e)
        {
            #region Modifing all the boolean columns (true/false) value for display appearence
            ReferentialColumn rrc = null;
            lstGdvColumn.ForEach(column =>
            {
                int index = lstGdvColumn.IndexOf(column) + nbColumnAction;

                TableCell cell = e.Row.Cells[index];

                #region Boolean data
                if (TypeData.IsTypeBool(column.dataType))
                {
                    if (cell.HasControls() && (null != cell.Controls[0]) && cell.Controls[0].GetType().Equals(typeof(Label)))
                    {
                        cell.CssClass = "img " + ((Label)cell.Controls[0]).Text.ToLower();
                        cell.Controls.RemoveAt(0);
                    }
                    else
                    {
                        cell.CssClass = "img " + cell.Text.ToLower().Replace("0","false").Replace("1","true");
                    }
                    cell.Text = string.Empty;
                    e.Row.HorizontalAlign = HorizontalAlign.Center;
                }
                #endregion Boolean data

                //20100310 RD Automatic Compute identifier
                #region Automatic Compute identifier
                if ((column.defaultValue == Cst.AUTOMATIC_COMPUTE.ToString()) && (StrFunc.IsEmpty(GetCellValue(e.Row, index))))
                {
                    cell.Text = resAUTOMATIC_COMPUTE;
                }
                else if (GetCellValue(e.Row, index) == Cst.AUTOMATIC_COMPUTE.ToString())
                {
                    cell.Text = resAUTOMATIC_COMPUTE;
                }
                #endregion

                #region HyperLink
                else if (isHyperLinkAvailable)
                {
                    try
                    {
                        rrc = referential[column.columnId];

                        if (column.columnType == GridViewColumnType.relation)
                        {
                            if (ArrFunc.IsFilled(rrc.Relation) && ArrFunc.IsFilled(rrc.Relation[0].ColumnRelation))
                            {
                                //FI 20120111 
                                string id = GetCellValue(e.Row, index - 1);
                                if (StrFunc.IsFilled(id) && (id != Cst.HTMLSpace) && (id != "0"))
                                {
                                    id = FormatToInvariant(id, rrc.Relation[0].ColumnRelation[0].DataType);
                                    string identifier = GetCellValue(e.Row, index);

                                    string columnName = RepositoryTools.GetHyperLinkColumnName(rrc.Relation[0]);

                                    if (StrFunc.IsFilled(columnName))
                                    {
                                        HyperLink hl = GetHyperLinkByColumn(e.Row, columnName, id, identifier);
                                        if ((null != hl) && (hl.Text != Cst.HTMLSpace))
                                        {
                                            if (isTemplateColumn)
                                                e.Row.Cells[index].Controls.RemoveAt(0);
                                            e.Row.Cells[index].Controls.Add(hl);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (rrc.ExistsHyperLinkDocument)
                            {
                                #region rrc.ExistsHyperLinkDocument
                                WCToolTipHyperlink hlkFile;
                                string text = string.Empty;
                                text = GetCellValue(e.Row, index);
                                //
                                if (StrFunc.IsFilled(text) && (text != Cst.HTMLSpace))
                                {
                                    ReferentialColumn rrcDataKeyField = referential.Column[referential.IndexDataKeyField];
                                    //
                                    string[] _keyColumns = new string[1];
                                    _keyColumns[0] = rrcDataKeyField.ColumnName;
                                    string[] _keyValues = new string[1];

                                    if (isTemplateColumn)
                                    {
                                        _keyValues[0] = ((Label)e.Row.Cells[referential.IndexColSQL_DataKeyField + nbColumnAction].Controls[0]).Text;
                                    }
                                    else
                                    {
                                        _keyValues[0] = e.Row.Cells[referential.IndexColSQL_DataKeyField + nbColumnAction].Text;
                                    }
                                    //Les type ROWVERSION ne sont jamais formaté, il ne faut pas le transformer en int
                                    if (false == referential.IsROWVERSIONDataKeyField)
                                        _keyValues[0] = FormatToInvariant(_keyValues[0], rrcDataKeyField.DataType.value);

                                    string[] _keyDatatypes = new string[1];
                                    _keyDatatypes[0] = rrcDataKeyField.DataType.value;

                                    hlkFile = GetHyperLinkForDocument(
                                        StrFunc.IsFilled(rrc.IsHyperLink.linktable) ? rrc.IsHyperLink.linktable : this.referential.TableName,
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
                                        e.Row.Cells[index].Controls.Add(hlkFile);
                                    }
                                }
                                #endregion rrc.ExistsHyperLinkDocument
                            }
                            else if (rrc.ExistsHyperLinkColumn)
                            {
                                //Type de Link (voir méthode IsExistsHyperLinkColumn(string pColumnName))
                                string colName = rrc.IsHyperLink.type;
                                //Colonne qui détient l'id ds le jeu de résultat
                                string colNameId = rrc.IsHyperLink.data;
                                if (RepositoryTools.IsHyperLinkColumn(colName))
                                {
                                    int indexColId = referential.GetIndexColSQL(colNameId);
                                    if (-1 == indexColId)
                                        throw new Exception(StrFunc.AppendFormat("Column [name:{0}] doesn't exist", colNameId));
                                    //
                                    string identifier = GetCellValue(e.Row, index);
                                    if (StrFunc.IsFilled(identifier) && (identifier != Cst.HTMLSpace))
                                    {
                                        DataRow row = ((DataRowView)e.Row.DataItem).Row;
                                        string idValue = row[indexColId].ToString();
                                        if (StrFunc.IsFilled(idValue) && (idValue != Cst.HTMLSpace) && (idValue != "0"))
                                        {
                                            //
                                            HyperLink hlkColumn = GetHyperLinkByColumn(e.Row, colName, idValue, identifier);
                                            if (hlkColumn != null)
                                            {
                                                if (isTemplateColumn)
                                                    e.Row.Cells[index].Controls.RemoveAt(0);
                                                e.Row.Cells[index].Controls.Add(hlkColumn);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (rrc.ExistsHyperLinkExternal)
                            {
                                string identifier = GetCellValue(e.Row, index);
                                string colNameId = rrc.IsHyperLink.data;
                                int indexColId = referential.GetIndexColSQL(colNameId);
                                if (-1 == indexColId)
                                    throw new Exception(StrFunc.AppendFormat("Column [name:{0}] doesn't exist", colNameId));

                                if (StrFunc.IsFilled(identifier) && (identifier != Cst.HTMLSpace))
                                {
                                    DataRow row = ((DataRowView)e.Row.DataItem).Row;
                                    string idValue = row[indexColId].ToString();
                                    if (StrFunc.IsFilled(idValue) && (idValue != Cst.HTMLSpace) && (idValue != "0"))
                                    {
                                        HyperLink hl = new HyperLink();
                                        hl.Target = "_blank";
                                        hl.CssClass = "linkDatagrid";
                                        hl.Text = identifier;
                                        if (isTemplateColumn)
                                            e.Row.Cells[index].Controls.RemoveAt(0);

                                        hl.NavigateUrl = idValue;
                                        e.Row.Cells[index].Controls.Add(hl);
                                    }
                                }
                            }
                            else if (rrc.ColumnName == "URL")
                            {
                                HyperLink hl = new HyperLink();
                                hl.Target = "_blank";
                                hl.CssClass = "linkDatagrid";
                                hl.Text = GetCellValue(e.Row, index);
                                if (isTemplateColumn)
                                    e.Row.Cells[index].Controls.RemoveAt(0);
                                //
                                hl.NavigateUrl = hl.Text;
                                //
                                e.Row.Cells[index].Controls.Add(hl);
                            }
                        }
                    }
                    catch { }
                }
                #endregion HyperLink

            });
            #endregion Modifing all the cols true/false value for display appearence
        }


        /// Display Editable control for editable column
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void FinalizeDisplayPermanentEditableColumn(GridViewRowEventArgs e)
        {
            // Construction du suffixe à l'aide de la valeur de la cellule de la colonne DATAKEYFIELD
            if (referential.ExistsColumnDataKeyField)
            {
                ReferentialColumn rrcKeyField = referential.Column[referential.IndexDataKeyField];
                DataRowView drv = (DataRowView)e.Row.DataItem;
                string suffix = Convert.ToString(drv.Row[rrcKeyField.IndexColSQL]);
                lstGdvColumn.ForEach(column =>
                {
                    if (column.isEditable)
                    {
                        int index = lstGdvColumn.IndexOf(column) + nbColumnAction;
                        TableCell cell = e.Row.Cells[index];
                        TemplateField tc = (TemplateField)this.Columns[index];
                        GridViewEditItemTemplate editTemplate = (GridViewEditItemTemplate)tc.EditItemTemplate;
                        Control ctrl = editTemplate.CloneControlMain;
                        if (null != ctrl)
                        {
                        ctrl.ID += "_" + suffix;
                        ((WCTextBox)ctrl).Enabled = true;
                        e.Row.Cells[index].Controls.Add(ctrl);
                        e.Row.Cells[index].Controls.RemoveAt(0);
                        SetValidatorAndDefaultValueOnEditableColumn(e.Row, index, (WCTextBox)ctrl);
                    }
                    }

                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUnitType"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private string FormattingValorisation(string pUnitType, string pValue, string pCurrency)
        {
            string returnValue = pValue;
            if (StrFunc.IsFilled(pUnitType))
            {
                UnitTypeEnum unitType = (UnitTypeEnum)System.Enum.Parse(typeof(UnitTypeEnum), pUnitType);
                decimal decval = DecFunc.DecValue(pValue, Thread.CurrentThread.CurrentCulture);
                switch (unitType)
                {
                    case UnitTypeEnum.Qty:
                        returnValue = StrFunc.FmtDecimalToCurrentCulture(decval, 3);
                        break;
                    case UnitTypeEnum.Currency:
                        CurrencyCashInfo currencyInfo = null;
                        if (false == _currencyInfos.ContainsKey(pCurrency))
                        {
                            currencyInfo = new CurrencyCashInfo(SessionTools.CS, pCurrency);
                            if (null != currencyInfo)
                                _currencyInfos.Add(pCurrency, currencyInfo);
                        }
                        if (_currencyInfos.ContainsKey(pCurrency))
                        {
                            currencyInfo = (CurrencyCashInfo)_currencyInfos[pCurrency];
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
            if (referential.ExistsColumnDataKeyField)
            {
                ReferentialColumn rrcKeyField = referential.Column[referential.IndexDataKeyField];
                lstGdvColumn.ForEach(column =>
                {
                    if (column.isEditable)
                    {
                        foreach (GridViewRow row in Rows)
                        {
                            int dsIndex = row.DataItemIndex;
                            // EG 20151102 [21465] Add test on DsData.Tables[0].Rows.Count
                            if ((dsIndex > DsData.Tables[0].Rows.Count) || (0 == DsData.Tables[0].Rows.Count))
                                break;
                            if (null != DsData)
                            {

                                DataRow dr = DsData.Tables[0].Rows[dsIndex];
                                string suffix = Convert.ToString(dr[rrcKeyField.IndexColSQL]);
                                Control referentialControl = row.FindControl(column.columnName);

                                string currentRequestValue = Page.Request.Form[String.Format("{0}{1}_{2}",
                                                             referentialControl.UniqueID,
                                                             RepositoryTools.SuffixEdit, suffix)];
                                if (StrFunc.IsFilled(currentRequestValue))
                                {
                                    dr.BeginEdit();
                                    dr[column.columnName] = currentRequestValue;
                                    dr.EndEdit();
                                    dr.AcceptChanges();
                                }
                            }
                        }

                }

                });
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
        private void UpdateEditableColumnValues_OnRowDataBound(GridViewRow pRow, WCTextBox pControl, string pColumnName, string pValue)
        {
            int dsIndex = pRow.DataItemIndex;

            //FI 20140409 [XXXXX] Utilisation de DataSource parce qu'un grid peut être trié au niveau de l'entête 
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
        /// <param name="pRow"></param>
        /// <param name="pIndex"></param>
        /// <param name="pControl"></param>
        // EG 20141230 [20587]
        // EG 20150920 [21314] Int (int32) to Long (Int64) 
        private void SetValidatorAndDefaultValueOnEditableColumn(GridViewRow pRow, int pIndex, WCTextBox pControl)
        {

            Validator validator = null;
            WebControl wcValidator = null;
            string msgValidator = string.Empty;
            long maxValue = 0;
            long availableQty = 0;
            string regExValidator = string.Empty;
            string subRegex = string.Empty;
            string subRegex2 = string.Empty;

            switch (referential.TableName)
            {
                case "CLEARINGBULK":
                    long qtyBuy = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "QTY_BUY"), TypeData.TypeDataEnum.@int.ToString()));
                    long qtySell = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "QTY_SELL"), TypeData.TypeDataEnum.@int.ToString()));
                    maxValue = Math.Min(qtyBuy, qtySell);

                    UpdateEditableColumnValues_OnRowDataBound(pRow, pControl, "CLEARINGQTY", maxValue.ToString());

                    msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", maxValue.ToString());
                    subRegex = GetRegExForQuantity(maxValue);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, maxValue.ToString());
                    validator = new Validator(msgValidator, true, regExValidator, false);
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                case "TRANSFERBULK":
                    long initialQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "INITIALQTY"), TypeData.TypeDataEnum.@int.ToString()));
                    availableQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "AVAILABLEQTY"), TypeData.TypeDataEnum.@int.ToString()));
                    maxValue = Math.Min(initialQty, availableQty);
                    UpdateEditableColumnValues_OnRowDataBound(pRow, pControl, "CLEARINGQTY", maxValue.ToString());

                    if (initialQty == availableQty)
                        msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", maxValue.ToString());
                    else if (1 == availableQty)
                        msgValidator = Ressource.GetString2("Msg_PosKeepingInputTransferQty", maxValue.ToString());
                    else
                        msgValidator = Ressource.GetString2("Msg_PosKeepingInputTransferQty2", maxValue.ToString());

                    subRegex = GetRegExForQuantity(availableQty);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, initialQty.ToString());
                    validator = new Validator(msgValidator, true, regExValidator,false);
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                case "CLEARINGSPEC":
                    availableQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "AVAILABLEQTY"), TypeData.TypeDataEnum.@int.ToString()));
                    long sourceAvailableQty = 0;
                    if (referential.dynamicArgsSpecified)
                        sourceAvailableQty = Convert.ToInt64(referential.dynamicArgs["AVAILABLEQTY"].value);
                    maxValue = Math.Min(sourceAvailableQty, availableQty);

                    UpdateEditableColumnValues_OnRowDataBound(pRow, pControl, "CLOSABLEQTY", "0");

                    msgValidator = Ressource.GetString2("Msg_PosKeepingInputQty", maxValue.ToString());
                    subRegex = GetRegExForQuantity(availableQty, true);
                    regExValidator = String.Format("^(?:{0}|{1})$", subRegex, maxValue.ToString());
                    validator = new Validator(msgValidator, true, regExValidator, false);
                    wcValidator = validator.CreateControl(pControl.ID);
                    pControl.Controls.Add(new LiteralControl("<br/>"));
                    pControl.Controls.Add(wcValidator);
                    break;
                case "UNCLEARING":
                    // EG 20131121 [19222] Conversion au format invariant
                    long clearingQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "QTY"), TypeData.TypeDataEnum.@int.ToString()));
                    UpdateEditableColumnValues_OnRowDataBound(pRow, pControl, "UNCLEARINGQTY", clearingQty.ToString());
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
                    if (referential.TableName.Contains("ASSEXEBULK") || referential.TableName.Contains("ABNBULK"))
                    {
                        if (referential.dynamicArgsSpecified && (null != referential.dynamicArgs["DENOPTIONACTIONTYPE"]))
                        {
                            long posQty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "POSQTY"), TypeData.TypeDataEnum.@int.ToString()));
                            long denqty = Convert.ToInt64(FormatToInvariant(GetCellValueByColumnName(pRow, "DENQTY"), TypeData.TypeDataEnum.@int.ToString()));
                            int nbDen = Convert.ToInt32(FormatToInvariant(GetCellValueByColumnName(pRow, "NBIDPR"), TypeData.TypeDataEnum.@int.ToString()));

                            Cst.DenOptionActionType denOptionActionType = (Cst.DenOptionActionType)ReflectionTools.EnumParse(new Cst.DenOptionActionType(), referential.dynamicArgs["DENOPTIONACTIONTYPE"].value);
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
                                        availableQty = Convert.ToInt32(FormatToInvariant(GetCellValueByColumnName(pRow, "AVAILABLEQTY"), TypeData.TypeDataEnum.@int.ToString()));
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
                            UpdateEditableColumnValues_OnRowDataBound(pRow, pControl, "CLEARINGQTY", availableQty.ToString());
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
            string subRegEx = string.Empty;
            int length = result.Length;

            switch (length)
            {
                case 1:
                    subRegEx = String.Format("[{1}-{0}]", result[0], pIsZeroAccepted?"0":"1");
                    break;
                default:
                    subRegEx = String.Format("[{0}-9]", pIsZeroAccepted?"0":"1");
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
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="pRowVersion"></param>
        /// <param name="pRowAttribut"></param>
        /// <param name="pIsDeleteItem"></param>
        private void ModifyActionDataBound(GridViewRow pRow, int pRowVersion, string pRowAttribut, bool pIsDeleteItem)
        {

            #region Modify action: Enabled, disabled the edit item
            //20090129 PL Afin d'afficher la loupe sur les LOGs, mise en comment du test
            //if ((isGridInputMode || isFormInputMode) && referential.Modify)
            if ((isGridInputMode && referential.Modify) || ((isFormInputMode || isFormViewerMode) && _isLoupeColumn))
            {
                WebControl wc = ((WebControl)pRow.Cells[editItemIndex]);
                LinkButton linkButton;
                if (null != wc.Controls[0])
                {
                    linkButton = (LinkButton)wc.Controls[0];
                    //if datagrid is locked (a row is already editing) or this row has been deleted, disable the edit item
                    if (isGridInputMode && (isLocked || pIsDeleteItem))
                    {
                        wc.Attributes["onclick"] = "return false;";
                        linkButton.CssClass = "btn btn-xs btn-edit";
                        linkButton.Enabled = false;
                        linkButton.ToolTip = Ressource.GetString("imgEditUnavailable");

                        //linkButton.Text = @"<div class=""ui-main ui-main-updatedisable"" title=""" + Ressource.GetString("imgEditUnavailable") + @"""/>";
                    }
                    else if (isFormInputMode || isFormViewerMode)
                    {
                        //20090910 PL Use GetCellValue()
                        linkButton.CssClass = "btn btn-xs btn-apply";
                        wc.Attributes["onclick"] = GetURLForUpdate(pRow) + ";return false;";
                    }
                    else
                    {
                        wc.Attributes["onclick"] = "return true;";
                        linkButton.CssClass = "btn btn-xs btn-apply";
                        linkButton.ToolTip = Ressource.GetString(referential.Modify ? "imgEdit" : "imgView");
                    }
                    // EG 20091110
                    if ((pRowAttribut == "T") && (pRowVersion <= 0))//T --> Total
                    {
                        if (("VW_INVOICEFEESDETAIL" == referential.TableName) && (-1 == pRowVersion))
                        {
                            // on garde le lien vers TRADE (mais en TRADEADMIN)
                            wc.Attributes["onclick"].Replace("TradeCapture", "TradeAdminCapture");
                        }
                        else
                            wc.Controls.RemoveAt(0);
                    }
                }
            }
            #endregion Modify action: Enabled, disabled the edit item


        }

        /// <summary>
        /// Remove action: Enabled, disabled the delete item
        /// </summary>
        /// <param name="e"></param>
        /// <param name="pIsDeleteItem"></param>
        private void RemoveActionDataBound(GridViewRow pRow, bool pIsDeleteItem)
        {

            if (nbColumnAction > 0)
            {
                // EG 20151215 Add Test referential.consultationMode
                if ((isGridInputMode || isFormInputMode) && (referential.Remove && (referential.consultationMode != Cst.ConsultationMode.ReadOnly)))
                {
                    WebControl wc = ((WebControl)pRow.Cells[deleteItemIndex]);
                    LinkButton linkButton;
                    if (wc.Controls[0] != null)
                        linkButton = (LinkButton)wc.Controls[0];
                    else
                        throw new Exception();

                    string js = string.Empty;
                    //if datagrid is unlocked (a row is not being editing) and data is not deleted, enable the delete item
                    if (!isLocked && !pIsDeleteItem)
                    {
                        //Creating delete message for each row with row-informations
                        bool RemoveAuthorized = true;
                        string msgAlert = string.Empty;
                        js = Ressource.GetString("Msg_Remove") + Cst.CrLf;
                        ReferentialColumn rrc;
                        //
                        if (referential.ExistsColumnROWATTRIBUT)
                        {
                            //si la cellule ne contient pas directement la donnée, on va la chercher dans son control container (cas pour les TemplateColumn)
                            //20090910 PL Use GetCellValue()
                            string rowAttributValue = GetCellValue(pRow, referential.IndexColSQL_ROWATTRIBUT + nbColumnAction);
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
                            for (int i = 0; i < referential.Column.GetLength(0); i++)
                            {
                                rrc = referential.Column[i];
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
                                        js += GetCellValue(pRow, rrc.IndexColSQL + nbColumnAction + indexAddForRelation);
                                    }
                                }
                            }
                        }
                        if (RemoveAuthorized)
                            js = "return confirm(" + JavaScript.JSString(js) + ");";
                        else
                            js = "alert('" + Ressource.GetStringForJS(msgAlert) + "');return false;";

                        linkButton.CssClass = "btn btn-xs btn-remove";
                        linkButton.ToolTip = Ressource.GetString("imgRemove");
                    }
                    else
                    {
                        //if datagrid is locked (a row is editing) or data have been modified, disable the delete item    
                        js = "return false;";
                        linkButton.CssClass = "btn btn-xs btn-remove";
                        linkButton.Enabled = false;
                        linkButton.ToolTip = Ressource.GetString("imgRemoveUnavailable");
                    }
                    wc.Attributes["onclick"] = js;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void DblClickDataBound(GridViewRowEventArgs e)
        {
            #region Double-click event, if is enable, add it on item
            if (isRowDblClickAvailable)
            {
                //if enabled for opening detailled form, creating open-script as url
                if (isFormInputMode || isFormViewerMode || isFormSelectMode)
                {
                    // EG 20121029 Add referential.consultationMode parameter
                    e.Row.Attributes.Add("ondblclick", GetURLForUpdate(e.Row));
                }
                //if enabled for selecting value, creating returnvalue-script as url
                else if (isGridSelectMode)
                {
                    #region isGridSelectMode
                    string keyField = string.Empty;
                    string JSkeyField = string.Empty;
                    string JSopenerKeyId = string.Empty;
                    string JSspecifiedField = string.Empty;
                    string JSopenerSpecifiedId = string.Empty;

                    if (indexColSQL_KeyField != -1)
                    {
                        //20090910 PL Use GetCellValue()
                        //keyField = e.Item.Cells[indexColSQL_KeyField].Text;
                        // 20091030 RD / 
                        // Prendre en compte d'eventuelles colonnes dites de type "Action" ( Exemple: Numéro de ligne, Loupe, etc...)
                        // Les colonnes de type "Action" ne sont pas incluses dans referential.Column
                        // Par contre elles le sont pour l'affichage dans le DataGrid
                        //
                        int indexColSQL_KeyFieldWithColumnAction = indexColSQL_KeyField;
                        if (indexColSQL_KeyFieldWithColumnAction != -1)
                            indexColSQL_KeyFieldWithColumnAction += nbColumnAction;

                        keyField = GetCellValue(e.Row, indexColSQL_KeyFieldWithColumnAction);
                    }

                    JSopenerKeyId = JavaScript.JSString(openerKeyId);
                    JSkeyField = JavaScript.JSString(keyField);

                    int indexColSQLFound = -1;
                    if (openerSpecifiedSQLField.Trim().Length > 0)
                    {
                        indexColSQLFound = referential.GetIndexColSQL(openerSpecifiedSQLField);
                        // 20091030 RD 
                        if (indexColSQLFound != -1)
                            indexColSQLFound += nbColumnAction;
                    }

                    if (openerSpecifiedId.Trim().Length > 0 && indexColSQLFound != -1)
                    {
                        JSopenerSpecifiedId = JavaScript.JSString(openerSpecifiedId);
                        //20090910 PL Use GetCellValue()
                        //JSspecifiedField = JavaScript.JSString(e.Item.Cells[indexColSQLFound].Text);
                        JSspecifiedField = JavaScript.JSString(GetCellValue(e.Row, indexColSQLFound));
                    }
                    else
                    {
                        JSopenerSpecifiedId = JavaScript.JS_NULL;
                        JSspecifiedField = JavaScript.JS_NULL;
                    }

                    string url = "GetReferential(" + JSopenerKeyId + "," + JSkeyField + "," + JSopenerSpecifiedId + "," + JSspecifiedField + ");return false;";
                    e.Row.Attributes.Add("ondblclick", url);
                    #endregion
                }
            }
            #endregion Double-click event, if is enable, add it on item

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSorting(Object sender, GridViewSortEventArgs e)
        {

            // Processes the sort expression. Determines whether auto-reverse is
            // needed and stores in Attributes two comma-separated strings. One
            // contains the names of the involved columns and one contains – in the
            // same order – the respective direction verbs.
            if (false == isDataModified)
            {
                ProcessSortExpression(e.SortExpression);
                PrepareSortExpression();
                //
                //Rechargement du datagrid si pagination personalisée car le Datatable ne contient qu'une partie du jeu de résultat
                //Remarque Lorsqu'un tri user est spécifié alors le datagrid charge la totalité du jeu de résultat (voir LoadData)
                isLoadData = (AllowCustomPaging);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPageIndexChanging(Object sender, GridViewPageEventArgs e)
        {
            SetIndexPage(e.NewPageIndex);
        }

        /// <summary>
        /// Génère la publication d'un évènement par le gridView
        /// </summary>
        /// <param name="eventArgument"></param>
        protected override void RaisePostBackEvent(string eventArgument)
        {

            GridViewDataErrorEventArgs eventArgs = null;
            //
            string[] stringSeparators = new string[] { "{-}" };
            String[] aEventArg = eventArgument.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            //
            if (ArrFunc.IsFilled(aEventArg))
            {
                if (aEventArg[0] == "LOAD_DATA_ERROR")
                    eventArgs = new GridViewDataErrorEventArgs(aEventArg[1], aEventArg[2]);
                //
                if (null != eventArgs)
                    OnLoadDataError(eventArgs);
                else
                    base.RaisePostBackEvent(eventArgument);
            }
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
                //
                // RD 20110421 [17418] Bug dans le cas de la suppression
                valueFK = NVC_QueryString["FK"] + string.Empty;
                //
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

                // FI 20170531 [23206] all method AlterConnectionString
                string cs = RepositoryTools.AlterConnectionString(SessionTools.CS, referential); 
                //QueryParameters query = SQLReferentialData.GetQueryLoadReferential(cs, referential, columnFK, valueFK, false, true, out isQueryWithSubTotal);
                ArrayList alChildSQLSelect;
                ArrayList alChildTablename;
                bool isQueryWithSubTotal;

                EFS.GridViewProcessor.SQLReferentialData.SelectedColumnsEnum selectedColumns = EFS.GridViewProcessor.SQLReferentialData.SelectedColumnsEnum.NoHideOnly;
                //PL 20120723 Utilisation de "All" si "ProcessBase", car certains traitements exploitent des données "Hide" pour produire le message MQueue.
                //PL 20120903 Utilisation de "All" si "Consultation".
                if ((this.ListType == Cst.ListType.ProcessBase) || (this.ListType == Cst.ListType.Consultation))
                    selectedColumns = EFS.GridViewProcessor.SQLReferentialData.SelectedColumnsEnum.All;

                //PL 20130102 Test in progress... (Use Oracle® hints)
                string sqlHints = string.Empty + NVC_QueryString["hints"];

                EFS.GridViewProcessor.SQLReferentialData.SQLSelectParameters sqlSelectParameters = new EFS.GridViewProcessor.SQLReferentialData.SQLSelectParameters(cs, selectedColumns, referential);
                sqlSelectParameters.sqlHints = sqlHints;
                QueryParameters query = EFS.GridViewProcessor.SQLReferentialData.GetQuery_LoadReferential(sqlSelectParameters,
                                             columnFK, valueFK, false, 
                                             true,
                                             out alChildSQLSelect, out alChildTablename, out isQueryWithSubTotal);

                //FI 20140626 [20142] Appel à ReplaceCriteriaKeyword
                query.query = ReplaceConsultCriteriaKeyword(query);

                if (isQueryWithSubTotal)
                    AllowSorting = false;//Interdiction du tri par colonne

                bool isQueryWithFKValue = (query.query.IndexOf(Cst.FOREIGN_KEY) >= 0);
                //---------------------------------------------------------
                //PL 20110315
                //---------------------------------------------------------
                if (isQueryWithFKValue)
                    query.query = query.query.Replace(Cst.FOREIGN_KEY, EFS.GridViewProcessor.SQLReferentialData.BuildValueFormated(referential, valueFK, false));
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
                string orderByWithColumnAlias = EFS.GridViewProcessor.SQLReferentialData.GetSQLOrderBy(cs, referential, true, true);
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
                    AllowCustomPaging = (null == pagingType) || (PagingTypeEnum.CustomPaging == pagingType);
                    //
                    //Pas de pagination personalisée sur les référentiel ou il existe des sous-totaux
                    if (AllowCustomPaging)
                        AllowCustomPaging = !ArrFunc.ExistInArray(referentialWithSubTotal, referential.TableName);
                    //
                    //Pas de pagination personalisée S'il existe un tri 
                    //car il est global à l'ensemble des données => Chargement complet du jeu de résulat
                    if (AllowCustomPaging)
                        AllowCustomPaging = (false == isSortedByUser);
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
                    sqlSelectParameters.selectedColumns = isConsultation ? EFS.GridViewProcessor.SQLReferentialData.SelectedColumnsEnum.All : EFS.GridViewProcessor.SQLReferentialData.SelectedColumnsEnum.None;
                    QueryParameters queryCount = EFS.GridViewProcessor.SQLReferentialData.GetQueryCountReferential(sqlSelectParameters, columnFK, valueFK);
                    queryCount.query = ReplaceConsultCriteriaKeyword(query);

                    if (isQueryWithFKValue)
                        queryCount.query = queryCount.query.Replace(Cst.FOREIGN_KEY, EFS.GridViewProcessor.SQLReferentialData.BuildValueFormated(referential, valueFK, false));

                    Object obj = null;
                    try
                    {
                        //PL 20110303 TEST for SQLServer WITH (TBD)
                        queryCount.query = EFS.GridViewProcessor.SQLReferentialData.TransformRecursiveQuery(cs, referential, columnFK, valueFK, false, queryCount.query);
                        //
                        ((PageBase)Page).AddAuditTimeStep("LoadData.QueryCount", true, queryCount.queryReplaceParameters);
                        if (null != transactIsolationLevel)
                            obj = DataHelper.ExecuteScalar(transactIsolationLevel, CommandType.Text, queryCount.query, queryCount.parameters.GetArrayDbParameter());
                        else
                            obj = DataHelper.ExecuteScalar(cs, CommandType.Text, queryCount.query, queryCount.parameters.GetArrayDbParameter());
                        ((PageBase)Page).AddAuditTimeStep("LoadData.QueryCount", false);
                    }
                    catch (Exception ex)
                    {
                        string errorCode = null;
                        bool isSqlError = DataHelper.AnalyseSQLException(cs, ex, out errorCode); ;
                        if (false == isSqlError)
                            throw;
                        //
                        _lastErrorQueryCount = errorCode;
                        obj = 10 * PageSize;
                    }
                    VirtualItemCount = Convert.ToInt32(obj);
                }
                //
                //Generation de la query définitive
                // --------------------------------------------------------------------------------------------------------
                // RD 20120629 [17968] Désormais toutes  les clauses OrderBy sont avec "Alias" (Utilisation de la syntaxe Over( order by ...)), 
                // Réfactoring et mise en commentaire du block ci-dessous, pour simplifier le code
                if (AllowCustomPaging)
                {
                    // Enrichir la requête avec le OrderBy
                    query.query = DataHelper.GetSelectForLimitedRows(query.query, orderByWithColumnAlias, PageIndex * PageSize + 1, PageIndex * PageSize + PageSize);
                }
                else
                {
                    // Enrichir la requête avec le OrderBy uniquement s'il n'existe pas un tri user  
                    if (false == isSortedByUser)
                        query.query = DataHelper.GetSelectForLimitedRows(query.query, orderByWithColumnAlias, 0, 0);

                    int top = (int)SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(Int32), -1);
                    if (top > -1)
                        query.query = DataHelper.GetSelectTop(cs, query.query, top);
                }
                // --------------------------------------------------------------------------------------------------------

                // RD 20120629 mise en commentaire ------------------------------------------------------------------------
                ////
                ////Generation de la query définitive
                //if (AllowCustomPaging)
                //{
                //    // RD 20120524 
                //    // Enrichir la requête avec le OrderBy
                //    //query.query = query.query.Replace(orderBy, string.Empty);
                //    query.query = DataHelper.GetSelectForLimitedRows(query.query, orderByWithColumnAlias, CurrentPageIndex * PageSize + 1, CurrentPageIndex * PageSize + PageSize);
                //}
                //else if (isSortedByUser)
                //{
                //    //Si chargement complet du jeu de résultat et qu'il existe un tri user, Spheres supprime le tri par défaut 
                //    //=> optimisation de la requête afin de réduire le temps d'exécution
                //    //query.query = query.query.Replace(orderBy, string.Empty);
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
                //        query.query = DataHelper.GetSelectForLimitedRows(query.query, orderByWithColumnAlias2, 0, 0);
                //        //query.query = query.query.Replace(orderBy, orderByWithColumnAlias2);
                //    //---------------------------------------------------------------
                //    int top = (int)SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(Int32), -1);
                //    if (top > -1)
                //        query.query = DataHelper.GetSelectTop(cs, query.query, top);
                //}
                // --------------------------------------------------------------------------------------------------------

                //Chargement de la source de donnée                  
                bool isLoadOk = true;
                DatetimeProfiler dtExec = null; ;
                try
                {
                    //PL 20110303 TEST for SQLServer WITH (TBD)
                    query.query = EFS.GridViewProcessor.SQLReferentialData.TransformRecursiveQuery(cs, referential, columnFK, valueFK, false, query.query);
                    //Sauvegarde de la Query qui va être exécutée
                    //La sauvegarde est effectuée avant de manière a permettre la consultation de la query même en cas d'erreur SQL
                    _lastQuery = new Pair<string, TimeSpan>();
                    _lastQuery.First = SqlCommandToDisplay(cs, query.queryReplaceParameters);
                    //     
                    ((PageBase)Page).AddAuditTimeStep("LoadData.QueryData", true, query.queryReplaceParameters);
                    dtExec = new DatetimeProfiler(DateTime.Now);

                    DsData = DataHelper.ExecuteDataset(cs, transactIsolationLevel, CommandType.Text, query.queryHint, query.GetArrayDbParameterHint());

                    _lastQuery.Second = dtExec.GetTimeSpan();
                    ((PageBase)Page).AddAuditTimeStep("LoadData.QueryData", false);

                    DsData.Tables[0].TableName = referential.TableName;
                    DataSav = null;
                }
                catch (Exception ex)
                {
                    if (null != dtExec)
                        _lastQuery.Second = dtExec.GetTimeSpan();

                    DsData = null;
                    isBindData = false;
                    //
                    string errorCode = null;
                    string message = null;
                    bool isSqlError = DataHelper.AnalyseSQLException(cs, ex, out errorCode, out message);
                    if (false == isSqlError)
                        throw;
                    //
                    GridViewDataErrorEventArgs eventArgs = new GridViewDataErrorEventArgs(errorCode, message);
                    //
                    string postBack_Argument = "LOAD_DATA_ERROR" + "{-}" + eventArgs.GetEventArgument();
                    string script = Page.ClientScript.GetPostBackEventReference(this, postBack_Argument);
                    //post de la page et invocation de l'évènement LoadDataErrorEvent
                    JavaScript.ScriptOnStartUp((PageBase)this.Page, script, "LOAD_DATA_ERROR");
                    isLoadOk = false;
                }
                //
                //20070611 PL
                //PL DISPLAY_RUPTURE_TOTAL Code ajouté pour géré en dur une rupture avec sous-total pour les écritures comptables
                // RD 20110712 [17514]
                // Réfactoring pour substituer la valeur de la colonne DTHOLIDAYVALUE, par la prochaine date du jour férié correspondant
                if (isLoadOk)
                {
                    bool isWithSubTotal = ArrFunc.ExistInArray(referentialWithSubTotal, referential.TableName);
                    //
                    if (isWithSubTotal || (referential.ExistsColumnDTHOLIDAYVALUE))
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
                                if ("VW_ACCDAYBOOK" == referential.TableName)
                                    AddSubTotalToACCDAYBOOK(dt);
                                else if ("VW_AGEINGBALANCEDET" == referential.TableName)
                                    AddSubTotalToAGEINGBALANCE(dt);
                                else if ("VW_INVOICEFEESDETAIL" == referential.TableName)
                                    AddSubTotalToINVOICEFEESDETAIL(dt);
                                else if ("FLOWSRISKCTRL_ALLOC" == referential.TableName)
                                    CalcRiskToFLOWSRISKCTRL_ALLOC(dt);
                            }
                            //
                            if (referential.ExistsColumnDTHOLIDAYVALUE)
                            {
                                // FI 20190509 [24661] Gestion d'une date de reference
                                Nullable<DateTime> dtRef = null;
                                if (referential.dynamicArgsSpecified && (referential.dynamicArgs.Keys.Contains("DTREF")))
                                    dtRef = new DtFunc().StringDateISOToDateTime(referential.dynamicArgs["DTREF"].GetDataValue(cs, null));
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

        protected virtual void OnLoadDataError(GridViewDataErrorEventArgs e)
        {
            if (LoadDataError != null)
                LoadDataError(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int GetIndexForFilter()
        {
            //20090429 PL Modif
            if (StrFunc.IsFilled(openerSpecifiedSQLField))
            {
                indexKeyField = referential.GetIndexDataGrid(openerSpecifiedSQLField);
                indexColSQL_KeyField = referential.GetIndexColSQL(openerSpecifiedSQLField);
            }
            else if (typeKeyField == Cst.KeyField && referential.ExistsColumnKeyField)
            {
                indexKeyField = referential.IndexKeyField;
                indexColSQL_KeyField = referential.IndexColSQL_KeyField;
            }
            else if (typeKeyField == Cst.DataKeyField && referential.ExistsColumnDataKeyField)
            {
                indexKeyField = referential.IndexDataKeyField;
                indexColSQL_KeyField = referential.IndexColSQL_DataKeyField;
            }
            return indexKeyField;
        }

        /// <summary>
        /// Retourne la propriété .Text du control qui affiche la colonne {pColName}
        /// <para>LA valeur de retour est formattée selon les caractéristiques de la culture</para>
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        private string GetCellValueByColumnName(GridViewRow pRow, string pColName)
        {
            int index = ((DataRowView)pRow.DataItem).Row.Table.Columns[pColName].Ordinal + nbColumnAction;
            return GetCellValue(pRow, index);
        }

        /// <summary>
        /// Retourne la propriété .Text du control qui affiche la donnée 
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pIndex"></param>
        /// <param name="pConvertToInvariantCulture">false :résultat formatté selon les caractéristiques de la culture, true: résultat  au format invariant</param>
        /// <returns></returns>
        private string GetCellValue(GridViewRow pRow, int pIndex)
        {
            string ret = null;
            if ((null != pRow) && (null != pRow.DataItem))
            {
                DataRow row = ((DataRowView)pRow.DataItem).Row;
            if (isTemplateColumn)
            {
                if ((pRow.Cells[pIndex].Controls.Count > 0) && (pRow.Cells[pIndex].Controls[0] != null))
                {
                    if (pRow.Cells[pIndex].Controls[0].GetType().Equals(typeof(Label)))
                        ret = ((Label)pRow.Cells[pIndex].Controls[0]).Text;
                    else if (pRow.Cells[pIndex].Controls[0].GetType().Equals(typeof(HyperLink)))
                        ret = ((HyperLink)pRow.Cells[pIndex].Controls[0]).Text;
                }
            }
            else
            {
                    ret = row[pIndex - nbColumnAction].ToString();
                }
            }

            return ret;
        }

        /// <summary>
        /// Si isNoFailure, alimente la liste des erreurs (alException) avec l'exception rencontrée, sinon génére l'exception
        /// </summary>
        /// <param name="ex">Représente l'exception rencontrée</param>
        private void TrapException(Exception pEx)
        {
            if (isNoFailure)
            {
                SpheresException ex = SpheresExceptionParser.GetSpheresException(null, pEx);
                _alException.Add(ex);
                if (Page is PageBase)
                    ((PageBase)Page).WriteLogException(ex);
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
        /// FI 20170531 [23206] Modify
        private void ExecutePreSelect()
        {
            _lastPreSelectCommand = null;
            //
            List<Pair<string, TimeSpan>> lstLastPreSelectCommand = new List<Pair<string, TimeSpan>>();
            try
            {
                BeforeExecutePreselect();
                //
                QueryParameters[] preSelectCommand = referential.GetSqlPreSelectCommand(SessionTools.CS);
                //
                for (int i = 0; i < ArrFunc.Count(preSelectCommand); i++)
                {
                    if (referential.IndexDataKeyField > -1)
                    {
                        //Replace du %%DA:PK()%% pouvant exister ds la query
                        //Le grid n'étant jamais ouvert avec une PK, les valeurs seront remplacé par -1 ou N/A
                        StringDynamicData sdPk = new StringDynamicData();
                        sdPk.name = "PK";
                        sdPk.datatype = referential[referential.IndexDataKeyField].DataType.value;
                        if (TypeData.IsTypeString(sdPk.datatype))
                            sdPk.value = "N/A";
                        else if (TypeData.IsTypeInt(sdPk.datatype))
                            sdPk.value = "-1";
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("dataType [{0}] is not implemented", sdPk.datatype));
                        //
                        preSelectCommand[i].query = sdPk.ReplaceInString(SessionTools.CS, preSelectCommand[i].query);
                        if ((referential.IsUseSQLParameters) && preSelectCommand[i].query.Contains("@PK"))
                        {
                            DataParameter pkParameter = sdPk.GetDataParameter(SessionTools.CS, null, CommandType.Text, 0, ParameterDirection.Input);
                            preSelectCommand[i].parameters.Add(pkParameter);
                        }
                    }
                    //
                    if (referential.IndexForeignKeyField > -1)
                    {
                        //Replace du %%DA:FK%% pouvant exister ds la query
                        //Si le grid est ouvert sans FK, %%DA:FK%% sera remplacé par -1 
                        StringDynamicData sdFk = new StringDynamicData();
                        sdFk.name = "FK";
                        sdFk.datatype = referential[referential.IndexForeignKeyField].DataType.value;
                        if (StrFunc.IsFilled(valueFK))
                        {
                            sdFk.value = valueFK;
                        }
                        else
                        {
                            if (TypeData.IsTypeString(sdFk.datatype))
                                sdFk.value = "N/A";
                            else if (TypeData.IsTypeInt(sdFk.datatype))
                                sdFk.value = "-1";
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("dataType [{0}] is not implemented", sdFk.datatype));
                        }
                        //
                        preSelectCommand[i].query = sdFk.ReplaceInString(SessionTools.CS, preSelectCommand[i].query);
                        if ((referential.IsUseSQLParameters) && preSelectCommand[i].query.Contains("@FK"))
                        {
                            DataParameter fkParameter = sdFk.GetDataParameter(SessionTools.CS, null, CommandType.Text, 0, ParameterDirection.Input);
                            preSelectCommand[i].parameters.Add(fkParameter);
                        }
                    }

                    //FI 20140626 [20142] appel à ReplaceCriteriaKeyword
                    preSelectCommand[i].query = ReplaceConsultCriteriaKeyword(preSelectCommand[i]);

                    //
                    Pair<string, TimeSpan> queryItem = new Pair<string, TimeSpan>();
                    queryItem.First = SqlCommandToDisplay(SessionTools.CS, preSelectCommand[i].queryReplaceParameters);
                    DatetimeProfiler dtExec = null;
                    try
                    {
                        dtExec = new DatetimeProfiler(DateTime.Now);

                        ((PageBase)Page).AddAuditTimeStep("QueryPreSelect." + i.ToString(), true, preSelectCommand[i].queryReplaceParameters);

                        //PL 20130723 TEST HINT /* Spheres:Hint ... */           
                        //DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, preSelectCommand[i].query, preSelectCommand[i].parameters.GetArrayDbParameter());
                        // FI 20170531 [23206] appel ReferentialTools.AlterConectionString
                        string cs = RepositoryTools.AlterConnectionString(SessionTools.CS, referential);
                        DataHelper.ExecuteNonQuery(cs, CommandType.Text, preSelectCommand[i].queryHint, preSelectCommand[i].GetArrayDbParameterHint());
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
        ///  Affecte le membre columnFK
        /// </summary>
        private void InitColumnFK()
        {
            if (StrFunc.IsFilled(valueFK))
            {
                //On est ici sur un appel d'un sous-référentiel et cela depuis un formulaire, donc pour un parent précis
                //Ex: Appel du référentiel Book depuis l'acteur CHASE --> on ne doit affichier que les Books de CHASE
                // 20101020 Test referential.ExistsColumnForeignKeyField
                if (referential.ExistsColumnForeignKeyField)
                {
                    columnFK = referential.Column[referential.IndexForeignKeyField].ColumnName;
                    //20090611 PL Add AliasColumnName (utile pour DEBTSECURITY)
                    if (referential.Column[referential.IndexForeignKeyField].AliasTableNameSpecified)
                        columnFK = referential.Column[referential.IndexForeignKeyField].AliasTableName + "." + columnFK;
                }
            }
            else if (referential.ExistsColumnForeignKeyField)
            {
                //On est ici sur un appel d'un sous-référentiel et cela depuis le menu principal
                //Ex: Appel du référentiel Book depuis le menu principal --> on doit affichier tous les Books de tous les Acteurs
                columnFK = referential.Column[referential.IndexForeignKeyField].ColumnName;
            }
        }

        /// <summary>
        ///  Excécution des certaines tâches nécessaires a l'appel à ExecutePreselect
        ///  <para>Ces tâches sont spécifiques à la consultation en cours</para>
        /// </summary>
        /// FI 20140115 [19482] upd QUOTE_H_EOD
        private void BeforeExecutePreselect()
        {

            if (this.referential.TableName == "POSSYNT")
            {
                //Création des tables spécifiques à la consultation synthétique des positions
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName("POSSYNT");
                string table2 = GetWorkTableName("POSSYNTQUOTE");
                //SQLUP.CreateTableFromTableModel(SessionTools.CS, "POSSYNT_MODEL", table1);
                //SQLUP.CreateTableFromTableModel(SessionTools.CS, "POSSYNTQUOTE_MODEL", table2);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSSYNT_MODEL", table1);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSSYNTQUOTE_MODEL", table2);
            }
            else if ((this.referential.TableName == "POSDET") || (this.referential.TableName == "POSDETOTC"))
            {
                //Création des tables spécifiques à la consultation détaillée des positions 
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName(this.referential.TableName);
                string table2 = string.Empty;
                if (this.referential.TableName == "POSDET")
                    table2 = GetWorkTableName("POSDETQUOTE");
                //
                //SQLUP.CreateTableFromTableModel(SessionTools.CS, "POSDET_MODEL", table1);
                //SQLUP.CreateTableFromTableModel(SessionTools.CS, "POSDETQUOTE_MODEL", table2);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSDET_MODEL", table1);
                if (StrFunc.IsFilled(table2))
                    DataHelper.CreateTableAsSelect(SessionTools.CS, "POSDETQUOTE_MODEL", table2);
            }
            else if (this.referential.TableName == "QUOTE_H_EOD")
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
            else if ((this.referential.TableName == "POSACTIONDET") || (this.referential.TableName == "POSACTIONDET_OTC"))
            {
                string table1 = GetWorkTableName(this.referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "POSACTIONDET_MODEL", table1);
            }
            // EG 20151019 [21465]
            else if (this.referential.TableName.StartsWith("ASSEXEBULK") || this.referential.TableName.StartsWith("ABNBULK"))
            {
                //Création des tables spécifiques au traitement de masse des dénouements
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName(this.referential.TableName);
                // EG 20160203 Q for QUOTE (Table Size limit in Oracle)
                string table2 = GetWorkTableName("Q" + this.referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "DENOPTBULK_MODEL", table1);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "QUOTEDENOPTBULK_MODEL", table2);
            }
            // EG 20151130 [20979]
            else if (this.referential.TableName.StartsWith("CLEARINGBULK"))
            {
                //Création des tables spécifiques au traitement de masse des compensations de masse
                //Ces tables sont utilisées par la commande PreSelect
                string table1 = GetWorkTableName(this.referential.TableName);
                DataHelper.CreateTableAsSelect(SessionTools.CS, "CLEARINGBULK_MODEL", table1);
            }


        }

        /// <summary>
        /// Retourne la commande {pSqlCommand} au format du SGBD encapsulé avec les informations du DAL
        /// <para>Utilisé pour l'affichage des requêtes en mode trace</para>
        /// </summary>
        /// <param name="pCommand"></param>
        private static string SqlCommandToDisplay(string pCS, string pSqlCommand)
        {
            string ret = string.Empty;
            CSManager csManager = new CSManager(pCS);
            DbSvrType serverType = csManager.GetDbSvrType();
            ret = DataHelper.TransformQuery(serverType, csManager.csSpheres, CommandType.Text, pSqlCommand, null);
            ret = DataHelper.AddDALAssemblyInfos(serverType, ret);
            return ret;
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
        /// Retourne la valeur de la colonne ELEMENTTYPE
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private Nullable<Cst.IOElementType> GetIOElementType(GridViewRow pRow)
        {
            Nullable<Cst.IOElementType> ret = null;
            int indexElementType = referential.GetIndexColSQL("ELEMENTTYPE");
            if (-1 < indexElementType)
            {
                DataRow row = ((DataRowView)(pRow.DataItem)).Row;
                //
                string elementType = row.ItemArray[indexElementType].ToString();
                if (Enum.IsDefined(typeof(Cst.IOElementType), elementType))
                    ret = (Cst.IOElementType)Enum.Parse(typeof(Cst.IOElementType), elementType);
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
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// FI 20130208 [] new method
        private string GetItemValueFK(GridViewRow pRow)
        {
            string ret = string.Empty;
            if (columnFK.Length > 0)
            {
                ret = GetCellValue(pRow, referential.IndexColSQL_ForeignKeyField + nbColumnAction);
                ret = FormatToInvariant(ret, referential[referential.IndexForeignKeyField].DataType.value);
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
            pagingType = BoolFunc.IsTrue(QS_AllowCustomPaging) ? PagingTypeEnum.CustomPaging : PagingTypeEnum.NativePaging;
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
                    DataControlField column = this.Columns[i];
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
        private static Nullable<Cst.UnderlyingAsset> GetAssetCategory(EFS.GridViewProcessor.Referential referential, DataRow row, bool pIsUnderlying)
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

                int index = referential.GetIndexColSQL(col);
                if (index > 0)
                {
                    string value = row.ItemArray[index].ToString();
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
        private static Nullable<Cst.ContractCategory> GetContractCategory(EFS.GridViewProcessor.Referential referential, DataRow row)
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
            ReferentialColumn rrc = referential.Column[referential.IndexDataKeyField];
            //
            string[] keyColumns = new string[3];
            keyColumns[0] = "TABLENAME";
            keyColumns[1] = "ID";
            keyColumns[2] = rrc.ColumnName;
            string[] keyValues = new string[3];
            keyValues[0] = this.NVC_QueryString["DA"];
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
            LOColumn dbfc = new LOColumn(SessionTools.CS, ObjectName, keyColumns, keyValues, keyDatatypes);
            if (dbfc != null)
            {
                //Lecture des données 
                dbfc.Load();
                //
                if (null != dbfc.fileContent)
                {
                    //Génération du fichier physique
                    string path = SessionTools.TemporaryDirectory.PathMapped;
                    if (Cst.ErrLevel.SUCCESS == FileTools.WriteBytesToFile(dbfc.fileContent, path + dbfc.fileName, true))
                    {
                        //Postage du fichier physique dans le flux HTML
                        ((PageBase)this.Page).ResponseWriteFile(dbfc.fileType, dbfc.fileName, path);
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
        public DataRow[] GetRowPage(Boolean pOnlyDetail, Boolean pOnlyIsSelectedRow)
        {
            DataRow[] ret = null;

            if (isDataAvailable)
            {
                DataView dataView = DataSource as DataView;
                if (null == dataView)
                    throw new Exception("Source is not an DataView");

                DataRowCollection rows = dataView.ToTable().Rows;
                if (rows.Count > 0)
                {
                    DataRow[] rowSource = new DataRow[rows.Count];
                    rows.CopyTo(rowSource, 0);

                    if (AllowPaging && false == AllowCustomPaging)
                    {
                        int firstIndex = PageIndex * PageSize;
                        int lastIndex = (PageIndex + 1) * PageSize - 1;
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

                    //FI 20141021 [20350] add
                    if (pOnlyIsSelectedRow)
                    {

                        if (dataView.Table.Columns.Contains("ISSELECTED"))
                            ret = ret.Where(r => Convert.ToInt32(r["ISSELECTED"]) == 1).ToArray();
                    }

                }
            }

            return ret;
        }



        /// <summary>
        /// Applique les critères spécifiés par l'utilisateur à la requête {pQuery}
        /// <para>Cette fonctionalité est disponible uniquement sur les requêtes qui utilisent les mots clés %%CC:</para>
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        ///FI 20140626 [20142] Add Method
        private string ReplaceConsultCriteriaKeyword(QueryParameters pQuery)
        {
            string ret = pQuery.query;

            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.CC_START) >= 0))
            {
                //lstCriteria contient les expressions SQL associées aux critères mis en place
                List<String> lstCriteria = new List<string>();
                if (referential.SQLWhereSpecified)
                {
                    for (int j = 0; j < ArrFunc.Count(referential.SQLWhere); j++)
                    {
                        ReferentialSQLWhere rrw = referential.SQLWhere[j];
                        if (rrw.ColumnNameSpecified)
                        {
                            SQL_ColumnCriteria sqlColumnCriteria = EFS.GridViewProcessor.SQLReferentialData.GetSQL_ColumnCriteria(SessionTools.CS, rrw, referential);
                            bool applySimpleCollation = SystemSettings.IsCollationCI();
                            string expression = sqlColumnCriteria.GetExpression2(SessionTools.CS, SQLCst.TBLMAIN + ".", applySimpleCollation);
                            lstCriteria.Add(expression);
                        }
                    }
                }
                
                #region %%CC:ITRADEINSTRUMENT_JOIN%% / %%CC:ITRADEINSTRUMENT_WHERE_PREDICATE%%
                //Remplacement des mots clés %%CC:ITRADEINSTRUMENT_JOIN%% et %%CC:ITRADEINSTRUMENT_WHERE_PREDICATE%%
                //par du code SQL qui prend en considération les critères spécifiés par l'utilisateur 
                int guard = 0; 
                while (ret.Contains(Cst.CC_ITRADEINSTRUMENT_JOIN) & (guard < 100))
                {
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.CC_ITRADEINSTRUMENT_JOIN);
                    string alias = arg[0];

                    string join = string.Empty;
                    string where = "1=1";
                    //FI 20160201 [XXXXX] 
                    //Par défaut, lorsque le tag IsUseCC est non présent il y a interprétation des mots clés  %%CC:ITRADEINSTRUMENT_XXX
                    if ((!referential.IsUseCCSpecified) || referential.IsUseCC)
                        GetSQLITradeInstrument(alias, lstCriteria, out join, out where);

                    ret = ret.Replace(Cst.CC_ITRADEINSTRUMENT_JOIN + "(" + alias + ")", join);
                    ret = ret.Replace(Cst.CC_ITRADEINSTRUMENT_WHERE_PREDICATE, where);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop on " + Cst.CC_ITRADEINSTRUMENT_JOIN);
                #endregion
                
                //PL 20150716 Newness
                #region %%CC:ITRADE_PREDICATE%% 
                //Remplacement du mot clé %%CC:ITRADE_ON_PREDICATE%%
                //par du code SQL qui prend en considération les critères spécifiés par l'utilisateur 
                guard = 0;
                while (ret.Contains(Cst.CC_ITRADE_ON_PREDICATE) & (guard < 100))
                {
                    //string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.CC_ITRADE_ON_PREDICATE);
                    //string alias = arg[0];
                    string alias = "t";

                    //string join = string.Empty;
                    string where = string.Empty;
                    if ((!referential.IsUseCCSpecified) || referential.IsUseCC)
                    {
                        SQLWhere sqlWhere = new SQLWhere();
                        AddSqlWhereITradeInstrument(alias, null, lstCriteria, sqlWhere);
                        if (sqlWhere.ToString().Length > 0)
                            where = sqlWhere.ToString().Replace(SQLCst.WHERE, SQLCst.AND);
                    }
                    if (string.IsNullOrEmpty(where))
                    {
                        where = StrFunc.Frame("Auto add filter - Warning! Missing filter on TRADE", "/*");
                    }
                    else
	                {
                        where = Cst.CrLf + StrFunc.Frame("Auto add filter", "/*") + Cst.CrLf + where;
	                }
                    ret = ret.Replace(StrFunc.Frame(Cst.CC_ITRADE_ON_PREDICATE, "/*"), where);
                    ret = ret.Replace(Cst.CC_ITRADE_ON_PREDICATE, where);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop on " + Cst.CC_ITRADE_ON_PREDICATE);
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// Retourne les jointures à appliquer à la table ITRADEINSTRUMENT et l'expression WHERE, compte tenu des critères en vigueur
        /// </summary>
        /// <param name="pAlias">Alias de la table ITRADEINSTRUMENT (une table ITRADEINSTRUMENT possède les colonnes IDA_DEALER,IDA_CLEARER, IDM, IDASSET)</param>
        /// <param name="pLstCriteria">Liste des expressions SQL issues des critètes spécifiés par l'utilsateur</param>
        /// <param name="opJoin">Retourne les jointures à appliquer pour prendre en considération les critères spécifiés</param>
        /// <param name="opWhere">Retourne l'expression where à appliquer pour prendre en considération les critères spécifiés</param>
        ///FI 20140626 [20142] Add Method
        ///FI 20140702 [20142] add QUOTE_H_EOD 
        ///FI 20140923 [XXXXX] Modify
        ///FI 20141118 [XXXXX] Modify 
        ///FI 20160201 [XXXXX] Modify (gestion de FLOWSBYASSETOTC_ALLOC)
        private void GetSQLITradeInstrument(string pAliasITradeInstrument, List<string> pLstCriteria, out string opJoin, out string opWhere)
        {
            opJoin = string.Empty;
            opWhere = "1=1"; ;

            /* 
             Explication sur isModeInnerJoin 
             Si true: Spheres® génère des jointures "inner join" avec les critères (Mode utilisé dans 90% des cas)
             Exemple  
               inner join dbo.BOOK b_dealclg on (b_dealclg.IDB = ti.IDB_DEALER) and (b_dealclg.IDENTIFIER  like  '%cfd%' escape '#')            
             
             Si false: Spheres® génère des jointures "left outer join" et génère un where avec les critères 
             Exemple 
               left outer join dbo.ASSET_EQUITY a_eqty on (a_eqty.IDASSET = ti.IDASSET)
               left outer join dbo.ASSET_INDEX a_idx on (a_idx.IDASSET = ti.IDASSET)
               where (case ti.ASSETCATEGORY when 'EquityAsset' then a_eqty.IDENTIFIER when 'Index' then a_idx.IDENTIFIER else null end  like  '%AA%' escape '#') 
            */

            Boolean isModeInnerJoin = true;
            SQLWhere sqlWhere = new SQLWhere();

            if ((pLstCriteria != null) && pLstCriteria.Count > 0)
            {
                switch (referential.TableName)
                {
                    case "POSSYNT":
                    case "POSDET":
                    case "POSDETOTC": //FI 20140923 [XXXXX] add
                    case "POSACTIONDET":
                    case "POSACTIONDET_OTC": 
                    case "FLOWSBYASSET":
                    case "FLOWSBYASSETOTC": //FI 20160201 [XXXXX]
                    case "QUOTE_H_EOD":

                       //criteriaActor
                        string aliasActor = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSSYNT_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDETOTC_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET_OTC)
                            )
                        {
                            aliasActor = "a_dealclg";
                        }
                        else if (consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSET_ALLOC) ||
                                 consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSETOTC_ALLOC))
                        {
                            aliasActor = "a_alloc_p";
                        }

                        if (StrFunc.IsFilled(aliasActor))
                            opJoin = AddSqlJoinITradeInstrument(aliasActor, pAliasITradeInstrument, pLstCriteria, opJoin, true);

                        //criteriaBook
                        string aliasBook = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSSYNT_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDETOTC_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET_OTC))
                        {
                            aliasBook = "b_dealclg";
                        }
                        else if (consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSET_ALLOC) ||
                                 consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSETOTC_ALLOC))
                        {
                            aliasBook = "b_alloc_p";
                        }

                        if (StrFunc.IsFilled(aliasBook))
                            opJoin = AddSqlJoinITradeInstrument(aliasBook, pAliasITradeInstrument, pLstCriteria, opJoin, true);

                        //criteriaAssetETD
                        string aliasAssetETD = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSSYNT_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSET_ALLOC))
                        {
                            aliasAssetETD = "a_etd_";
                        }
                        else if (consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC) ||
                                 consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET))
                        {
                            aliasAssetETD = "a_etd";
                        }

                        if (StrFunc.IsFilled(aliasAssetETD))
                            opJoin = AddSqlJoinITradeInstrument(aliasAssetETD, pAliasITradeInstrument, pLstCriteria, opJoin, true);

                        //criteria DC
                        string aliasDC = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSSYNT_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSET_ALLOC) ||
                            referential.TableName == "QUOTE_H_EOD")
                        {
                            aliasDC = "dc_etd";
                        }
                        else if (consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC) ||
                                 consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET))
                        {
                            aliasDC = string.Empty; //pour ces consultations => utilisation de l'alias a_etd qui contient toutes les colonnes DC disponibles
                        }

                        if (StrFunc.IsFilled(aliasDC))
                            opJoin = AddSqlJoinITradeInstrument(aliasDC, pAliasITradeInstrument, pLstCriteria, opJoin, true);


                        // FI 20141120 [XXXXX] add alias asset
                        //Criteria asset
                        string aliasAsset = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSDETOTC_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET_OTC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSETOTC_ALLOC))
                        {
                            isModeInnerJoin = true;
                            aliasAsset = "asset";
                        }
                        if (StrFunc.IsFilled(aliasAsset))
                        {
                            opJoin = AddSqlJoinITradeInstrument(aliasAsset, pAliasITradeInstrument, pLstCriteria, opJoin, isModeInnerJoin);
                            if (false == isModeInnerJoin)
                                AddSqlWhereITradeInstrument(aliasAsset, pAliasITradeInstrument, pLstCriteria, sqlWhere);
                        }

                        //criteriaMarket
                        string aliasMarket = String.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSSYNT_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDETOTC_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET_OTC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSET_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSETOTC_ALLOC) ||
                            (referential.TableName == "QUOTE_H_EOD"))
                        {
                            aliasMarket = "mktident";
                        }

                        if (StrFunc.IsFilled(aliasMarket))
                            opJoin = AddSqlJoinITradeInstrument(aliasMarket, pAliasITradeInstrument, pLstCriteria, opJoin, true);

                        //criteriaCss
                        string aliasCss = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSSYNT_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.POSDETOTC_ALLOC) ||
                            consult.IsConsultation(LstConsultData.ConsultEnum.FLOWSBYASSET_ALLOC))
                            aliasCss = "a_css";
                        else if (consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET) ||
                                 consult.IsConsultation(LstConsultData.ConsultEnum.POSACTIONDET_OTC))
                            aliasCss = "a_csspos";

                        if (StrFunc.IsFilled(aliasCss))
                            opJoin = AddSqlJoinITradeInstrument(aliasCss, pAliasITradeInstrument, pLstCriteria, opJoin, true);

                        
                        // CriteriaTrade 
                        /* Fonctionnement très particulier sur la table TRADE pour les consultation position détaillée 
                         * S'il existe des critères sur TRADE Spheres® ajoute uniquement un where 
                         * Il est ici prévu que la table TRADEINSTRUMENT est nécessairement en jointure sur la table TRADE et que cette dernière a pour alias "t" 
                         * Il existe donc nécessairement 
                         * From Trade t 
                         * inner join TRADEINSTRUMENT {pAliasITradeInstrument} on {pAliasITradeInstrument}.IDT = t.IDT etc... 
                         */
                        string aliasTrade = string.Empty;
                        if (consult.IsConsultation(LstConsultData.ConsultEnum.POSDET_ALLOC))
                        {
                            aliasTrade = "t_lsd";
                            AddSqlWhereITradeInstrument(aliasTrade, pAliasITradeInstrument, pLstCriteria, sqlWhere);
                            if (sqlWhere.Length() > 0)
                                sqlWhere = new SQLWhere(sqlWhere.ToString().Replace("t_lsd.", "t."));

                        }
                        else if (consult.IsConsultation(LstConsultData.ConsultEnum.POSDETOTC_ALLOC))
                        {
                            aliasTrade = "t";
                            AddSqlWhereITradeInstrument(aliasTrade, pAliasITradeInstrument, pLstCriteria, sqlWhere);
                        }
                        break;

                    default:
                        opJoin = string.Empty;
                        sqlWhere = new SQLWhere();
                        break;
                }
            }

            if (sqlWhere.ToString().Length > 0)
                opWhere = sqlWhere.ToString().Replace(SQLCst.WHERE, string.Empty);
        }

        /// <summary>
        /// Ajoute les jointures et les restictions au code SQL {pSQL} et retourne le résultat
        /// </summary>
        /// <param name="pLstInner">Liste des jointures</param>
        /// <param name="criteria">Liste des critères qui utilisent les jointures</param>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// FI 20140626 [20142] Add Method
        private static string AddSqlJoin(List<string> pLstJoin, IEnumerable<string> pCriteria, string pSQL)
        {
            string ret = pSQL;

            if ((null != pLstJoin) && pLstJoin.Count > 0)
            {
                foreach (string item in pLstJoin)
                {
                    if (false == pSQL.Contains(item))
                        ret += Cst.CrLf + item;
                }

                if (null != pCriteria)
                {
                    foreach (string item in pCriteria)
                        ret += SQLCst.AND + item;
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne les jointure nécessaires pour application des critères qui s'appuient sur l'alias {pAlias} 
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="pAliasITradeInsrument">alias de la table ITradeInstrument</param>
        /// <returns></returns>
        /// FI 20140626 [20142] Add Method
        /// FI 20140923 [XXXXX] Modify  
        /// FI 20141118 [XXXXX] Modify  
        /// FI 20160201 [XXXXX] Modify 
        private List<string> GetjoinFromITradeInstrument(string pAlias, string pAliasITradeInsrument, Boolean pIsInnerJoin)
        {
            List<string> ret = new List<string>();

            string table = string.Empty;
            string col = string.Empty;
            string sqlJoin = pIsInnerJoin ? "inner join" : "left outer join";

            switch (pAlias) 
            {
                case "a_dealclg":
                case "b_dealclg":
                    table = pAlias.StartsWith("a") ? "ACTOR" : "BOOK";
                    col = pAlias.StartsWith("a") ? "IDA" : "IDB";

                    string defaultjoin = StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on (({1}.{2} = {3}.{2}_DEALER) or ({1}.{2} = {3}.{2}_CLEARER))", table, pAlias, col, pAliasITradeInsrument);

                    if (referential.dynamicArgsSpecified)
                    {
                        if (referential.dynamicArgs.ContainsKey("POSTYPE"))
                        {
                            StringDynamicData sdd = referential.dynamicArgs["POSTYPE"];
                            if (sdd.value == "1")
                                ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.{2} = {3}.{2}_DEALER)", table, pAlias, col, pAliasITradeInsrument));
                            else if (sdd.value == "2")
                                ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.{2} = {3}.{2}_CLEARER)", table, pAlias, col, pAliasITradeInsrument));
                        }
                    }
                    if (ret.Count == 0)
                        ret.Add(defaultjoin);
                    break;

                case "a_alloc_p":
                case "b_alloc_p":
                    table = pAlias.StartsWith("a") ? "ACTOR" : "BOOK";
                    col = pAlias.StartsWith("a") ? "IDA" : "IDB";
                    if (referential.dynamicArgsSpecified)
                    {
                        if (referential.dynamicArgs.ContainsKey("ACTORSIDE"))
                        {
                            StringDynamicData sdd = referential.dynamicArgs["ACTORSIDE"];
                            if (sdd.value == "1")
                                ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.{2} = {3}.{2}_DEALER)", table, pAlias, col, pAliasITradeInsrument));
                            else if (sdd.value == "2")
                                ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.{2} = {3}.{2}_CLEARER)", table, pAlias, col, pAliasITradeInsrument));
                        }
                        else if (referential.dynamicArgs.ContainsKey("POSTYPE")) // FI 20160201 [XXXXX] add POSTYPE 
                        {
                            StringDynamicData sdd = referential.dynamicArgs["POSTYPE"];
                            if (sdd.value == "1")
                                ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.{2} = {3}.{2}_DEALER)", table, pAlias, col, pAliasITradeInsrument));
                            else if (sdd.value == "2")
                                ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.{2} = {3}.{2}_CLEARER)", table, pAlias, col, pAliasITradeInsrument));
                        }
                    }
                    break;

                case "a_etd_":
                case "a_etd":
                    string tableAsset = (pAlias == "a_etd_") ? "ASSET_ETD" : "VW_ASSET_ETD_EXPANDED";
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.IDASSET = {2}.IDASSET)", tableAsset, pAlias, pAliasITradeInsrument));
                    break;

                case "dc_etd":
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.ASSET_ETD aetdcc on (aetdcc.IDASSET = {0}.IDASSET)", pAliasITradeInsrument));
                    ret.Add(sqlJoin + " dbo.DERIVATIVEATTRIB dacc on (dacc.IDDERIVATIVEATTRIB=aetdcc.IDDERIVATIVEATTRIB)");
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.DERIVATIVECONTRACT {0} on ({0}.IDDC=dacc.IDDC)", pAlias));
                    break;

                case "mktident":
                    ret.Add(StrFunc.AppendFormat(sqlJoin  + " dbo.VW_MARKET_IDENTIFIER mktident on (mktident.IDM = {0}.IDM)", pAliasITradeInsrument));
                    break;

                case "a_css":
                case "a_csspos":
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.MARKET mktcc on (mktcc.IDM = {0}.IDM)", pAliasITradeInsrument));
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.ACTOR {0} on ({0}.IDA = mktcc.IDA)", pAlias));
                    break;

                case "a_eqty": //FI 20140923 [XXXXX] add
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.IDASSET = {2}.IDASSET)", "ASSET_EQUITY", pAlias, pAliasITradeInsrument));
                    break;

                case "a_idx": //FI 20140923 [XXXXX] add 
                    ret.Add(StrFunc.AppendFormat(sqlJoin +  " dbo.{0} {1} on ({1}.IDASSET = {2}.IDASSET)", "ASSET_INDEX", pAlias, pAliasITradeInsrument));
                    break;

                case "asset": //FI 20141120 [XXXXX] add 
                    ret.Add(StrFunc.AppendFormat(sqlJoin + " dbo.{0} {1} on ({1}.IDASSET = {2}.IDASSET and {1}.ASSETCATEGORY = {2}.ASSETCATEGORY)", "VW_ASSET", pAlias, pAliasITradeInsrument));
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("alias (value:{0}) is not implemented", pAlias));
            }

            return ret;
        }
        
        /// <summary>
        /// Ajoute les jointures nécessaires sur l'alias {pAlias} s'il existe des critères qui portent sur l'alias {pAlias}
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="pAliasITradeInstrument">alias de la table ITRADEINSTRUMENT</param>
        /// <param name="pLstCriteria">Liste des critères</param>
        /// <param name="pJoin">Jointures en cours de construction</param>
        /// <param name="pIsInnerJoin">
        /// <para>Si true, ajoute des jointures de type inner join </para>
        /// <para>Si false, ajoute des jointures de type Left outer join</para>
        /// </param>
        /// <returns></returns>
        /// FI 20140923 [XXXXX] Modify
        private string AddSqlJoinITradeInstrument(string pAlias, string pAliasITradeInstrument, List<string> pLstCriteria, string pJoin, Boolean pIsInnerJoin)
        {
            string ret = pJoin;

            if (StrFunc.IsFilled(pAlias))
            {
                var criteria = from item in pLstCriteria
                               where (item.Contains(StrFunc.AppendFormat(" {0}.", pAlias)) ||
                                     item.Contains(StrFunc.AppendFormat("({0}.", pAlias)))
                               select item;

                if (criteria.Count() > 0)
                {
                    List<string> lstInner = GetjoinFromITradeInstrument(pAlias, pAliasITradeInstrument, pIsInnerJoin);

                    if (pIsInnerJoin)
                        ret = AddSqlJoin(lstInner, criteria, pJoin);
                    else
                        ret = AddSqlJoin(lstInner, null, pJoin);
                    // FI 20140923 [XXXXX] add
                    // petite codage en dur car Cristina utilise l'alias "ti_lsd" dans les consultations 
                    // =>Certaines expressions dans LSTCOLUMN s'appuient donc sur l'alais "ti_lsd" (voir table ASSET)
                    if (ret.Contains("ti_lsd."))
                        ret = ret.Replace("ti_lsd.", pAliasITradeInstrument + ".");
                }
            }
            return ret;
        }

        /// <summary>
        /// Ajoute les where nécessaires sur l'alias {pAlias} s'il existe des critères qui portent sur l'alias {pAlias}
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="pAliasITradeInstrument">alias de la table TRADEINSTRUMENT</param>
        /// <param name="pLstCriteria">Liste des critères</param>
        /// <param name="sqlWhere">Expression where en cours de construction</param>
        /// <returns></returns>
        /// FI 20140923 [XXXXX] Modify
        private void AddSqlWhereITradeInstrument(string pAlias, string pAliasITradeInstrument, List<string> pLstCriteria, SQLWhere sqlWhere)
        {
            if (StrFunc.IsFilled(pAlias))
            {
                List<string> criteria = (from item in pLstCriteria
                                         where (item.Contains(StrFunc.AppendFormat(" {0}.", pAlias)) || 
                                                item.Contains(StrFunc.AppendFormat("({0}.", pAlias)))
                                         select item).ToList();

                if (criteria.Count() > 0)
                {
                    foreach (string item in criteria)
                    {
                        string item2 = item;
                        // FI 20140923 [XXXXX] add
                        // petite codage en dur car Cristina utilise l'alias "ti_lsd" dans les consultations 
                        // =>Certaines expressions dans LSTCOLUMN s'appuient donc sur l'alais "ti_lsd" (voir table ASSET)
                        if (item2.Contains("ti_lsd."))
                            item2 = item2.Replace("ti_lsd.", pAliasITradeInstrument + ".");

                        if (false == (sqlWhere.ToString().Contains(item2)))
                            sqlWhere.Append(item2);
                    }
                }
            }
        }
        
        /// <summary>
        /// Ecrase le jeu de résultat (DsData) s'il existe un filtre via le control txtSearch
        /// <para>S'il existe ce type de filtre Spheres® retient tous les enregistrements qui contiennent la donnée présente dans txtSearch</para>
        /// </summary>
        /// FI 20140926 [XXXXX] Add Method
        private void LoadSearch()
        {
            if ((isDataAvailable) && (false == isSelfClear) )
            {
                string value = string.Empty;
                TextBox txtSearch = Page.FindControl("txtSearch") as TextBox;

                if (null != txtSearch)
                    value = txtSearch.Text;

                if (StrFunc.IsFilled(value))
                {
                    //Sauvegarde du jeu de résultat de la requeête SQL sans l'élément de recherche présent dans DsData si isLoadData
                    if (DataSav == null)
                    {
                        DataTable dt = DsData.Tables[0];
                        DataSav = dt.Clone();
                        foreach (DataRow row in dt.Rows)
                            DataSav.ImportRow(row);
                    }

                    DataRow[] rows = DataSav.Rows.Cast<DataRow>().ToArray();
                    DataColumn[] columns = DataSav.Columns.Cast<DataColumn>().ToArray();

                    //Distinct de manière à retourner 1 seul row lorsque plusiuers colonnes de ce row matchent avec le filtre saisi  
                    IEnumerable rowFilter = (from DataRow row in rows
                                             from DataColumn column in columns
                                             where IsDataMatch(column, row[column], value)
                                             select row).Distinct();

                    DataTable dtResultFilter = DsData.Tables[0].Clone();
                    foreach (DataRow row in rowFilter)
                        dtResultFilter.ImportRow(row);

                    //DsData.Tables[0] est écrasé avec les lignes filtrées
                    DsData.Tables[0].Rows.Clear();
                    foreach (DataRow row in dtResultFilter.Rows)
                        DsData.Tables[0].ImportRow(row);
                }
                else if (false == isLoadData)
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
        /// 
        /// </summary>
        /// <param name="pDataColumn"></param>
        /// <param name="pValue"></param>
        /// <param name="pSearch"></param>
        /// <returns></returns>
        /// FI 20140926 [XXXXX] Add Method
        private static Boolean IsDataMatch(DataColumn pDataColumn, object pValue, string pSearch)
        {
            Boolean ret = false;

            Boolean isOk = ((pValue != Convert.DBNull) &&
                            (false == pDataColumn.ColumnName.StartsWith("TRK_")) &&            
                            (pDataColumn.ColumnName != "RANK_COL") &&
                            (pDataColumn.ColumnName != "ROWSTATE") &&
                            (pDataColumn.ColumnName != "ROWSTYLE") &&
                            (pDataColumn.ColumnName != "ISSELECTED") &&
                            (pDataColumn.ColumnName != "ISSELECTED") &&
                            (pDataColumn.ColumnName != "ROWVERSION") );
            if (isOk)
            {
                string sValue = string.Empty;
                string columnName = pDataColumn.ColumnName;
                if ((columnName.IndexOf("MATFMT_") >= 0))
                {
                    sValue = GetFormattedMaturity(columnName, pValue.ToString());
                }
                else
                {
                    sValue = pValue.ToString();
                }

                ret = sValue.ToUpper().Contains(pSearch.ToUpper());
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'échéance formatée en fonction du nom de colonne SQL et du format d'affichage appliqué à l'utilisateur 
        /// </summary>
        /// <param name="columnName">Nom de colonne </param>
        /// <param name="columnValue">valeur présente dans la colonne</param>
        /// <returns></returns>
        /// FI 20140926 [XXXXX] Add Method
        private static string GetFormattedMaturity(string columnName, string columnValue)
        {
            
            Cst.ETDMaturityInputFormatEnum format = Cst.ETDMaturityInputFormatEnum.FIX;

            if ((columnName.EndsWith("MATFMT_MMMYY")) ||
                (columnName.EndsWith("MATFMT_PROFIL") && SessionTools.ETDMaturityFormat == Cst.ETDMaturityInputFormatEnum.MMMspaceYY))
            {
                format = Cst.ETDMaturityInputFormatEnum.MMMspaceYY;
            }
            else if ((columnName.EndsWith("MATFMT_MY")) ||
                     (columnName.EndsWith("MATFMT_PROFIL") && SessionTools.ETDMaturityFormat == Cst.ETDMaturityInputFormatEnum.MY))
            {
                format = Cst.ETDMaturityInputFormatEnum.MY;
            }
            else if (columnName.EndsWith("MATFMT_PROFIL"))
            {
                format = SessionTools.ETDMaturityFormat;
            }
            
            string ret  = Tools.FormattingETDMaturity(format, columnValue.Replace("~", string.Empty));
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
            if (referential.RequestTrackSpecified && SessionTools.IsRequestTrackProcessEnabled)
            {
                RequestTrackRepositoryBuilder builder = new RequestTrackRepositoryBuilder();
                builder.action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual);
                builder.processType = pProcessType;
                builder.referential = referential;
                builder.row = new DataRow[] { pRow };
                ((PageBase)this.Page).RequestTrackBuilder = builder;
            }
        }

        /// <summary>
        /// Extrait le contenu du contrôle serveur dans un objet System.Web.UI.HtmlTextWriter fourni
        /// </summary>
        /// <param name="writer"></param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (isModeExportExcel)
                PrepareControlForExcel(writer);

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
        private void PrepareControlForExcel(HtmlTextWriter pWriter)
        {
            ShowHeader = true;
            ShowFooter = false;

            SetColumnActionVisible(false);

            List<string> log = new List<string>();
            ReplaceControls(this, log);

#if DEBUG
            if (log.Count > 0)
            {
                System.Diagnostics.Debug.Write("********* PrepareControlForExcel: ReplaceControls LOG **********" + Cst.CrLf);
                foreach (string item in log)
                    System.Diagnostics.Debug.Write(item + Cst.CrLf);
                System.Diagnostics.Debug.Write("****************************************************************");
            }
#endif
        }

        /// <summary>
        /// Remplace certains contrôles de manières à exporter le grid en excel
        /// </summary>
        /// <param name="pCtrl"></param>
        /// PL 20130416 [18596] add method
        /// FI 20160308 [21782] Modify
        private static void ReplaceControls(Control pCtrl, List<string> pLog)
        {
            // On récupère le nombre de controles enfants composant le GridView
            int nbControls = pCtrl.Controls.Count - 1;

            while (nbControls >= 0)
            {
                ReplaceControls(pCtrl.Controls[nbControls], pLog);
                nbControls = nbControls - 1;
            }

            if (pCtrl is GridViewRow)
            {
                // Suppression des attributs, style , class  pour alleger le flux HTML
                // row.Style est conservé car interprété par Spheres® 
                TableRow row = ((TableRow)pCtrl);
                row.Attributes.Remove("ondblclick");
                row.Attributes.Remove("onmouseover");
                row.Attributes.Remove("onmouseout");
                row.CssClass = string.Empty;
                row.ToolTip = null;
            }
            else if (pCtrl is TableCell)
            {
                // suppression des attributs, class  pour alleger le flux HTML
                // style est conservé car interprété par Spheres® 
                TableCell cell = ((TableCell)pCtrl);
                cell.Attributes.Remove("onclick");

                // gestion des panel qui représente les images true/false
                if (cell.CssClass == "img false")
                {
                    LiteralControl literalControl = new LiteralControl();
                    literalControl.Text = "0";
                    pCtrl.Controls.Add(literalControl);
                }
                else if (cell.CssClass == "img true")
                {
                    LiteralControl literalControl = new LiteralControl();
                    literalControl.Text = "1";
                    pCtrl.Controls.Add(literalControl);
                }
                cell.CssClass = string.Empty;
                cell.Wrap = true; // Supprime l'attribut style="white-space:nowrap;"
                cell.ToolTip = null;
            }
            else if ((pCtrl.GetType().GetProperty("SelectedItem") != null))
            {
                LiteralControl literalControl = new LiteralControl();
                pCtrl.Parent.Controls.Add(literalControl);
                // La cellule prend alors pour valeur le texte correspondant à la propriété "SelectedItem"
                literalControl.Text = (string)(pCtrl.GetType().GetProperty("SelectedItem").GetValue(pCtrl, null));
                // Le controle concerné est retiré
                pCtrl.Parent.Controls.Remove(pCtrl);
            }
            else if ((pCtrl.GetType().GetProperty("Text") != null))
            {
                LiteralControl literalControl = new LiteralControl();
                pCtrl.Parent.Controls.Add(literalControl);
                // On attribue le texte de la propriété "Text" à la cellule concernée
                literalControl.Text = (string)(pCtrl.GetType().GetProperty("Text").GetValue(pCtrl, null));
                pCtrl.Parent.Controls.Remove(pCtrl);
            }
            else
            {
                string log = StrFunc.AppendFormat("No replace of Ctrl: {0} ;", pCtrl.GetType().ToString());
                if (false == pLog.Contains(log))
                    pLog.Add(log);
            }
        }


        #endregion
    }
}

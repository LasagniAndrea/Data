#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Restriction;
using EfsML.DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.GridViewProcessor
{
    /// <summary>
    /// Referential Root
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Referentials
    {
        [System.Xml.Serialization.XmlElementAttribute("Referential", Form = XmlSchemaForm.Unqualified)]
        public Referential[] Items;
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tableName"></param>
        /// <returns></returns>
        public Referential this[string _tableName]
        {
            get
            {
                if (Items.Length == 1)
                    return Items[0];
                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i].TableName == _tableName)
                        return Items[i];
                }
                return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// EG 20110906 Add SQLCheckSelectedDefaultValueSpecified/SQLCheckSelectedDefaultValue
    /// EG 20111013 Add isLoupe & isLoupeSpecified
    public class Referential
    {
        public Referential()
        {
            this.Notepad = new ReferentialBooleanEltAndTablenameAttrib();
            this.AttachedDoc = new ReferentialBooleanEltAndTablenameAttrib();
        }

        #region Members

        [System.Xml.Serialization.XmlElementAttribute("form", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public RepositoryForm form;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool formSpecified;


        //PL 20120611 Add CmptLevel/CmptLevelSpecified
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CmptLevelSpecified;
        /// <summary>
        /// Indicateur de compatibilité 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("CmptLevel", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CmptLevel;

        //PL 20120621 Add Button/ButtonSpecified
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ButtonSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Button", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialButton[] Button;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsUseSQLParametersSpecified;

        /// <summary>
        /// Indicateur pour usage de paramètres SQL ds les requêtes 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IsUseSQLParameters", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsUseSQLParameters;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("customObject", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CustomObject[] customObject;

        /// <summary>
        /// Table principale
        /// <para>Table surlaquelle sont appliqués les mis à jour</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TableName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AliasTableNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AliasTableName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RoleTableNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RoleTableName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialItemTableName RoleTableName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ItemsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Items", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialItems Items;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HideExtlIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HideExtlId;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Ressource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TargetNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TargetName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnByRowSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int ColumnByRow;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CreateSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Create;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ModifySpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Modify;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RemoveSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Remove;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DuplicateSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Duplicate;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ImportSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Import;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExportSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Export;
        //
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Image;
        //
        [System.Xml.Serialization.XmlElementAttribute("Column", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumn[] Column;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ToolBarSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ToolBar;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TableName_PSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool TableName_P;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UseStatisticSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool UseStatistic;
        //
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string HelpUrl;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HelpUrlSpecified;
        //
        [System.Xml.Serialization.XmlElementAttribute("Logo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialLogo Logo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LogoSpecified;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NotepadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Notepad", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //public bool Notepad;
        public ReferentialBooleanEltAndTablenameAttrib Notepad;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AttachedDocSpecified;
        [System.Xml.Serialization.XmlElementAttribute("AttachedDoc", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //public bool AttachedDoc;
        public ReferentialBooleanEltAndTablenameAttrib AttachedDoc;
        //
        // EG 20111013 Utilisation de Flag pour forcer la gestion ou non de la loupe
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isLoupeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IsLoupe", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isLoupe;
        // EG 20111013 Utilisation de Flag pour forcer la gestion ou non du doubleclick
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isDblClickSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IsDblClick", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isDblClick;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLSelectSpecified;
        /// <summary>
        /// Représente la commande Select exécutée pour charger les données ds le grid
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SQLSelect", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelect[] SQLSelect;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLSelectResourceSpecified;
        /// <summary>
        /// Définie la resource qui permet d'obtenir SQLSelect 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SQLSelectResource", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelectResource SQLSelectResource;

        /// <summary>
        /// Représente le script Select qui charge les données (lorsque SQLSelect est renseigné)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SQLSelectCommand;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLPreSelectSpecified;
        /// <summary>
        /// Représente les scripts SQL exécutables avant le chargement des données
        /// <para>Plusieurs scripts sont présents (exemple: un script pour Oracle et un script pour SqlServer)</para>
        /// <para>exemple: création d'une table temporaire</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SQLPreSelect", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelect[] SQLPreSelect;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLPreSelectResourceSpecified;
        /// <summary>
        /// Définie la resource qui permet d'obtenir SQLPreSelectResource 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SQLPreSelectResource", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelectResource SQLPreSelectResource;

        /// <summary>
        /// Représente les scripts retenus qui seront exécutés avant le chargement des données
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private string[] SQLPreSelectCommand;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLWhereSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLWhere", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLWhere[] SQLWhere;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLOrderBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLOrderBy", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLOrderBy[] SQLOrderBy;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLGroupBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLGroupBy", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string[] SQLGroupBy;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLJoinSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLJoin", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string[] SQLJoin;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLRowStyleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLRowStyle", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLRowStyle SQLRowStyle;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLRowStateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLRowState", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLRowState SQLRowState;
        //
        // EG 20110906
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLCheckSelectedDefaultValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLCheckSelectedDefaultValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool SQLCheckSelectedDefaultValue;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool XSLFileNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("XSLFileName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialXSLFileName[] XSLFileName;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LoadOnStartSpecified;
        [System.Xml.Serialization.XmlElementAttribute("LoadOnStart", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool LoadOnStart;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RefreshIntervalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RefreshInterval", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int RefreshInterval;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RequestTrackSpecified;
        /// <summary>
        /// Pilote la journalisation des actions utilisateurs
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("RequestTrack", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialRequestTrack RequestTrack;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasMultiTable;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasLstWhereClause; // = false;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDynamicConsult; // = false;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDataKeyField_String; // = false;        
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasColumnsWithStyle; // = false;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasAggregateColumns; // = false;        
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasEditableColumns;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasDynamicResource;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasLengthInDatagrid;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasDataTRIM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasDataLightDisplay;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsConsultation;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsForm;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsGrid;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Cst.ConsultationMode consultationMode;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string valueForeignKeyField
        {
            get;
            set;
        }

        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string firstVisibleAndEnabledControlID;
        //

        /// <summary>
        /// Liste des arguments
        /// <para>
        /// Chaque item est au format XML correspondant à la sérialisation d'un StringDynamicData
        /// </para>
        /// </summary>
        /// FI 20141211 [20563] Rename en xmlDynamicArgs
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string[] xmlDynamicArgs
        {
            set;
            get;
        }

        /// <summary>
        /// Liste des arguments
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Dictionary<string, StringDynamicData> dynamicArgs
        {
            set;
            get;
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dynamicArgsSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataSet dataSet;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataRow dataRow;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DataRow[] drExternal;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool[] isNewDrExternal;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isNewRecord;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isLookReadOnly;
        //	
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isValidDataOnly;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isDailyUpdDataOnly;
        //	
        //Variables pour la gestion des linked controls
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int NbControlLinked; // = 0;
        //
        private int m_IndexColSQL_DataKeyField, m_IndexDataKeyField, m_IndexKeyField, m_IndexColSQL_KeyField, m_IndexColSQL_ForeignKeyField, m_IndexForeignKeyField;
        private int m_IndexColSQL_DISPLAYNAME, m_IndexColSQL_DESCRIPTION, m_IndexColSQL_EXTLLINK, m_IndexColSQL_EXTLLINK2, m_IndexColSQL_EXTLATTRB, m_IndexEXTLLINK, m_IndexEXTLLINK2, m_IndexEXTLATTRB;
        private int m_IndexColSQL_IDENTITY, m_IndexColSQL_IDENTITYWithSource, m_IndexIDENTITY;
        private int m_IndexColSQL_DTENABLED, m_IndexColSQL_DTDISABLED, m_IndexDTENABLED, m_IndexDTDISABLED;
        private int m_IndexColSQL_DTUPD, m_IndexColSQL_IDAUPD, m_IndexColSQL_DTINS, m_IndexColSQL_IDAINS;
        private int m_IndexColSQL_DTCHK, m_IndexColSQL_IDACHK, m_IndexColSQL_IDCHK, m_IndexColSQL_ISCHK;
        private int m_IndexColSQL_ROWATTRIBUT, m_IndexColSQL_DTHOLIDAYVALUE;
        // RD 20110704 [17501]
        // Utilisation d'un Index à la place du nom, pour avoir accéder aux caractèristique de coloration (alColumnCellStyle)        
        private int m_IndexColSQL_ISSIDE;
        // EG 20120608 Add Timeout (en seconde)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimeoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Timeout", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Timeout;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsUseCCSpecified;
        /// <summary>
        /// Indicateur pour activer ou non l'interprétation des mots clé %%CC:
        /// <para>Flag mis en place juste avant livraison de la 4.1 de manière à pouvoir débrayer l'interprétation en cas de dysfonctionnement</para>
        /// </summary>
        /// FI 20140626 [20142]  add property
        [System.Xml.Serialization.XmlElementAttribute("IsUseCC", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsUseCC;
        #endregion Members

        #region indexor
        public ReferentialColumn this[string _columnName]
        {
            get
            {
                for (int i = 0; i < Column.Length; i++)
                {
                    if (Column[i].ColumnName == _columnName)
                        return Column[i];
                }
                return null;
            }
        }
        //20090305 PL New
        public ReferentialColumn this[string _columnName, string _aliasTableName]
        {
            get
            {
                if (_aliasTableName == null)
                {
                    return this[_columnName];
                }
                else
                {
                    //PL 20120924 Add TrimEnd
                    _aliasTableName = _aliasTableName.TrimEnd(' ');
                    for (int i = 0; i < Column.Length; i++)
                    {
                        if ((null != Column[i])
                            && (Column[i].ColumnName == _columnName)
                            && (Column[i].AliasTableNameSpecified && (Column[i].AliasTableName == _aliasTableName)))
                            return Column[i];
                    }
                }
                return null;
            }
        }
        //20070607 PL New
        public ReferentialColumn this[int _columnPositionInDataGrid]
        {
            get
            {
                for (int i = 0; i < Column.Length; i++)
                {
                    if (Column[i].ColumnPositionInDataGridSpecified && (Column[i].ColumnPositionInDataGrid == _columnPositionInDataGrid))
                        return Column[i];
                }
                return Column[_columnPositionInDataGrid];
            }
        }
        //20090224 PL New
        public ReferentialColumn this[int _index, string property]
        {
            get
            {
                switch (property)
                {
                    case "ExternalFieldID":
                        for (int i = 0; i < Column.Length; i++)
                        {
                            if (Column[i].IsAdditionalData && (Column[i].ExternalFieldID == _index))
                                return Column[i];
                        }
                        break;
                }
                return null;
            }
        }
        #endregion indexor

        #region properties
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLightDisplayAvailable
        {
            get
            {
                return (true);
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20110105
        public bool IsDataGridWithTemplateColumn
        {
            /* 
            Utilisation d'une TemplateColumn ou d'une BoundColumn
                TemplateColumn: Colonne dont on veux personnaliser le contenu
                        ex: isGridInputMode ou existance d'une colonne dont on veux traduire le contenu
                BoundColumn: Colonne dont on n'a pas besoin de personnaliser le contenu
            */
            get
            {
                return (this.HasDynamicResource
                        || this.HasDataTRIM
                        || this.HasLengthInDatagrid
                        || this.ExistsColumnISSIDE
                        || this.HasColumnsWithStyle
                        || this.HasAggregateColumns
                        || this.HasEditableColumns
                        );
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnDataKeyField
        {
            get { return (m_IndexColSQL_DataKeyField >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnKeyField
        {
            get { return (m_IndexColSQL_KeyField >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnForeignKeyField
        {
            get { return (m_IndexColSQL_ForeignKeyField >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnDISPLAYNAME
        {
            get { return (m_IndexColSQL_DISPLAYNAME >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnDESCRIPTION
        {
            get { return (m_IndexColSQL_DESCRIPTION >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnEXTL
        {
            get { return (ExistsColumnEXTLLINK || ExistsColumnEXTLLINK2 || ExistsColumnEXTLATTRB); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnEXTLLINK
        {
            get { return (m_IndexColSQL_EXTLLINK >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnEXTLLINK2
        {
            get { return (m_IndexColSQL_EXTLLINK2 >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnEXTLATTRB
        {
            get { return (m_IndexColSQL_EXTLATTRB >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnIDENTITY
        {
            get { return (m_IndexColSQL_IDENTITY >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnIDENTITYWithSource
        {
            get { return (m_IndexColSQL_IDENTITYWithSource >= 0); }
        }
        /// <summary>
        /// Obtient true s'il existe les colonnes DTENABLED et DTDISABLED
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnsDateValidity
        {
            get { return (m_IndexColSQL_DTENABLED >= 0) && (m_IndexColSQL_DTDISABLED >= 0); }
        }

        /// <summary>
        /// Obtient true s'il existe une colonne DTCHK
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsMakingChecking
        {
            get { return (m_IndexColSQL_ISCHK >= 0); }
        }
        /// <summary>
        /// Obtient true s'il existe les colonnes ROWATTRIBUT
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnROWATTRIBUT
        {
            get { return (m_IndexColSQL_ROWATTRIBUT >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnISSIDE
        {
            get { return (m_IndexColSQL_ISSIDE >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnsUPD
        {
            get { return (m_IndexColSQL_DTUPD >= 0) && (m_IndexColSQL_IDAUPD >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnsINS
        {
            get { return (m_IndexColSQL_DTINS >= 0) && (m_IndexColSQL_IDAINS >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsColumnDTHOLIDAYVALUE
        {
            get { return (m_IndexColSQL_DTHOLIDAYVALUE >= 0); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DataKeyField
        {
            get { return m_IndexColSQL_DataKeyField; }
        }
        /// <summary>
        /// Obtient l'index de position de la colonnes dite "DataKeyField"
        /// <para>Obtient -1 si aucune colonne DataKeyField </para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexDataKeyField
        {
            get { return m_IndexDataKeyField; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexKeyField
        {
            get { return m_IndexKeyField; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_KeyField
        {
            get { return m_IndexColSQL_KeyField; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_ForeignKeyField
        {
            get { return m_IndexColSQL_ForeignKeyField; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexForeignKeyField
        {
            get { return m_IndexForeignKeyField; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DISPLAYNAME
        {
            get { return m_IndexColSQL_DISPLAYNAME; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DESCRIPTION
        {
            get { return m_IndexColSQL_DESCRIPTION; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_EXTLLINK
        {
            get { return m_IndexColSQL_EXTLLINK; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_EXTLLINK2
        {
            get { return m_IndexColSQL_EXTLLINK2; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_EXTLATTRB
        {
            get { return m_IndexColSQL_EXTLATTRB; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_IDENTITY
        {
            get { return m_IndexColSQL_IDENTITY; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DTENABLED
        {
            get { return m_IndexColSQL_DTENABLED; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DTDISABLED
        {
            get { return m_IndexColSQL_DTDISABLED; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexEXTLLINK
        {
            get { return m_IndexEXTLLINK; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexEXTLLINK2
        {
            get { return m_IndexEXTLLINK2; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexEXTLATTRB
        {
            get { return m_IndexEXTLATTRB; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexIDENTITY
        {
            get { return m_IndexIDENTITY; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexDTENABLED
        {
            get { return m_IndexDTENABLED; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexDTDISABLED
        {
            get { return m_IndexDTDISABLED; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_ROWATTRIBUT
        {
            get { return m_IndexColSQL_ROWATTRIBUT; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_ISSIDE
        {
            get { return m_IndexColSQL_ISSIDE; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DTUPD
        {
            get { return m_IndexColSQL_DTUPD; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_IDAUPD
        {
            get { return m_IndexColSQL_IDAUPD; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DTINS
        {
            get { return m_IndexColSQL_DTINS; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_IDAINS
        {
            get { return m_IndexColSQL_IDAINS; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_DTCHK
        {
            get { return m_IndexColSQL_DTCHK; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_IDACHK
        {
            get { return m_IndexColSQL_IDACHK; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_IDCHK
        {
            get { return m_IndexColSQL_IDCHK; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL_ISCHK
        {
            get { return m_IndexColSQL_ISCHK; }
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsROWVERSIONDataKeyField
        {
            get { return ((this.IndexDataKeyField != -1) && (this.Column[this.IndexDataKeyField].ColumnName == Cst.OTCml_COL.ROWVERSION.ToString())); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Nullable<ProductTools.GroupProductEnum> GroupProduct
        {
            get
            {
                Nullable<ProductTools.GroupProductEnum> groupProduct = null;
                string gProduct = string.Empty;
                if ((null != this["GPRODUCT"]) && (null != dataRow["GPRODUCT"]))
                    gProduct = dataRow["GPRODUCT"].ToString();
                else if (dynamicArgsSpecified && (null != dynamicArgs["GPRODUCT"]))
                    gProduct = this.dynamicArgs["GPRODUCT"].value;

                if (StrFunc.IsFilled(gProduct))
                    groupProduct = (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), gProduct);
                return groupProduct;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Nullable<ProductTools.FamilyEnum> FamilyProduct
        {
            get
            {
                Nullable<ProductTools.FamilyEnum> familyProduct = null;
                string family = string.Empty;
                if ((null != this["FAMILY"]) && (null != dataRow["FAMILY"]))
                    family = dataRow["FAMILY"].ToString();
                else if (dynamicArgsSpecified && (null != dynamicArgs["FAMILY"]))
                    family = dynamicArgs["FAMILY"].value;

                if (StrFunc.IsFilled(family))
                    familyProduct = (ProductTools.FamilyEnum)ReflectionTools.EnumParse(new ProductTools.FamilyEnum(), family);

                return familyProduct;
            }
        }
        #endregion

        #region Methods
        #region CreateForm
        /// <summary>
        /// Création d'un formulaire par défault avec :
        /// 1 section (COMMON) dédiée à la FOREIGNKEY, DATAKEYFIELD et/ou IDENTIFIER, DISPLAYNAME, DESCRIPTION, DTENABLED, DTDISABLED, EXTLLINK
        /// 1 section (MAIN)   dédiée aux autres colonnes déclarées visible dans le fichier XML dans leur ordre d'apparition.
        /// </summary>
        public void CreateForm(string pIdMenu)
        {
            List<BSGridTag> lstBSGridTag = new List<BSGridTag>();

            formSpecified = true;
            form = new RepositoryForm(pIdMenu);
            form.autogen = true;

            BSGridCommon common = new BSGridCommon();
            common.CreateCommon(pIdMenu);
            lstBSGridTag.Add(common);

            // Lecture de toutes les colonnes visibles en mode Formulaire du référentiel
            // - hors colonnes de type Common
            // - hors colonnes de type AdditionalData
            List<ReferentialColumn> lstRrc = Column.Where(column =>
                (false == column.IsAdditionalData) &&
                (false == "IDENTIFIER|DISPLAYNAME|DESCRIPTION|DTENABLED|DTDISABLED|EXTLLINK|EXTLLINK2".Contains(column.ColumnName))).ToList();
            BSGridTag bsGridTag = BSGridTag.CreateContent(lstRrc);
            if (null != bsGridTag)
                lstBSGridTag.Add(bsGridTag);

            // Lecture de toutes les colonnes IsAdditionalData :

            // IsItem
            BSGridItem item = new BSGridItem();
            item.LstColumns = Column.ToList();
            item.CreateTemplate(lstBSGridTag, bsGridTag);

            // IsRole
            BSGridRole role = new BSGridRole();
            role.LstColumns = Column.ToList();
            role.CreateTemplate(lstBSGridTag, bsGridTag);

            BSGridExternal external = new BSGridExternal();
            external.LstColumns = Column.ToList();
            external.CreateTemplate(lstBSGridTag, bsGridTag);

            form.tags = lstBSGridTag.ToArray();
        }
        #endregion CreateForm

        private BSGridTag CreateAdditionalInfos(List<BSGridTag> pLstBSGridTag, BSGridTag pBSGridTag, List<ReferentialColumn> pLstRrc)
        {
            BSGridTag bsGridTag_Additional = BSGridTag.CreateContent(pLstRrc);
            if (null != bsGridTag_Additional)
            {
                if (pBSGridTag is BSGridTabs)
                {
                    BSGridTabs tabs = pBSGridTag as BSGridTabs;
                    List<BSGridSection> lstSections = tabs.sections.ToList();
                    if (bsGridTag_Additional is BSGridTabs)
                    {
                        List<BSGridSection> lstSections_Additional = (bsGridTag_Additional as BSGridTabs).sections.ToList();
                        lstSections_Additional.ForEach(item => item.active = false);
                        lstSections.AddRange(lstSections_Additional);
                    }
                    else
                    {
                        string title = EFS.ACommon.Ressource.GetMulti("ADDITIONALINFO");
                        //BSGridSection section = new BSGridSection(lstSections.First().color, title);
                        BSGridSection section = new BSGridSection(title);
                        section.tags = new BSGridTag[1] { bsGridTag_Additional };
                        lstSections.Add(section);
                    }
                    // Les sections pour les données additionnelles ont désormais pour container le tabs (pBSGridTabs) 
                    tabs.sections = lstSections.ToArray();
                }
                else
                {
                    pLstBSGridTag.Add(bsGridTag_Additional);
                }
            }
            return bsGridTag_Additional;
        }

        /// <summary>
        /// Retourne l'index de la colonne {pColumnName}
        /// <para>Retourne -1 si la colonne n'existe pas</para>
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        /// // 20091110 EG Test nullable
        public int GetIndexColSQL(string pColumnName)
        {
            for (int i = 0; i < Column.Length; i++)
            {
                if (Column[i].ColumnName == pColumnName)
                    return Column[i].IndexColSQL;
            }
            //Ex a1_Identifier
            int pos = pColumnName.IndexOf("_");
            if (pos > 0)
            {
                string[] aliasNameColumnName = pColumnName.Split("_".ToCharArray());
                string aliasName = aliasNameColumnName[0];
                string columnName = aliasNameColumnName[1];
                for (int i = 0; i < Column.Length; i++)
                {
                    if ((null != Column[i].Relation) && (null != Column[i].Relation[0]))
                    {
                        ReferentialColumnRelation relation = Column[i].Relation[0];
                        if (ArrFunc.IsFilled(relation.ColumnSelect) && (null != relation.ColumnSelect[0]))
                        {
                            if ((relation.ColumnSelect[0].ColumnName == columnName) && (relation.AliasTableName == aliasName))
                                return Column[i].IndexColSQL + 1;
                        }
                    }
                }
            }

            for (int i = 0; i < Column.Length; i++)
            {
                if (Column[i].DataField.ToUpper() == pColumnName.ToUpper())
                    return Column[i].IndexColSQL;
            }

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        /// 20091110 EG Test nullable
        public int GetIndexDataGrid(string pColumnName)
        {
            for (int i = 0; i < Column.Length; i++)
            {
                if (Column[i].ColumnName == pColumnName)
                    return i;
            }
            //Ex a1_Identifier
            int pos = pColumnName.IndexOf("_");
            if (pos > 0)
            {
                string[] aliasNameColumnName = pColumnName.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                string previousAliasName = string.Empty;
                while (2 == aliasNameColumnName.Length)
                {
                    string aliasName = previousAliasName + aliasNameColumnName[0];
                    string columnName = aliasNameColumnName[1];
                    for (int i = 0; i < Column.Length; i++)
                    {
                        if ((null != Column[i].Relation) && (null != Column[i].Relation[0]))
                        {
                            ReferentialColumnRelation relation = Column[i].Relation[0];
                            if (ArrFunc.IsFilled(relation.ColumnSelect) && (null != relation.ColumnSelect[0]))
                            {
                                if ((relation.ColumnSelect[0].ColumnName == columnName) && (relation.AliasTableName == aliasName))
                                    return i;
                            }
                        }
                        else if (Column[i].ColumnNameOrColumnSQLSelectSpecified)
                        {
                            if (Column[i].ColumnNameOrColumnSQLSelect.ToUpper() == (aliasName + "." + columnName).ToUpper())
                                return i;
                        }
                    }
                    previousAliasName += aliasNameColumnName[0] + "_";
                    aliasNameColumnName = aliasNameColumnName[1].Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        public int GetIndexCol(string pColumnName)
        {
            for (int i = 0; i < Column.Length; i++)
            {
                if (Column[i].ColumnName == pColumnName)
                    return i;
            }
            return -1;
        }

        #region public InitializeNewRow
        public void InitializeNewRow(ref DataRow opNewRow)
        {
            InitializeNewRow(ref opNewRow, null);
        }
        public void InitializeNewRow(ref DataRow opNewRow, DataRow pPreviousDataRow)
        {
            bool previousDataRowSpecified = (pPreviousDataRow != null);

            for (int index = 0; index < this.Column.Length; index++)
            {
                ReferentialColumn rrc = this.Column[index];
                if (rrc.IsAdditionalData)
                {
                    //if external data : no creation until ID is affected                    
                }
                else
                {
                    Object oDefaultValue = null;
                    if (rrc.IsForeignKeyField)
                        oDefaultValue = this.valueForeignKeyField;
                    else if (rrc.IsMandatory && rrc.ExistsDDLType)
                        oDefaultValue = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_" + rrc.Relation[0].DDLType.Value);

                    //Automatic setting Logo on/off if exists column LOLOGO
                    if (rrc.ColumnName == "LOLOGO")
                    {
                        this.Logo.Value = true;
                        this.LogoSpecified = true;
                        this.Logo.columnname = rrc.ColumnName;
                        this.Logo.columnnameSpecified = true;
                    }

                    if (oDefaultValue == null)
                    {
                        if (rrc.ExistsDefaultValue)
                        {
                            if (rrc.Default[0].Value.ToString().ToLower() == "{Previous}".ToLower())
                            {
                                if (previousDataRowSpecified)
                                {
                                    oDefaultValue = pPreviousDataRow[rrc.DataField];
                                }
                            }
                            else
                            {
                                string defaultValue = rrc.GetStringDefaultValue(this.TableName);
                                if (StrFunc.IsEmpty(defaultValue))
                                {
                                    oDefaultValue = Convert.DBNull;
                                }
                                else
                                {
                                    oDefaultValue = defaultValue;
                                }
                            }
                        }
                        else if (rrc.ExistsDefaultColumnName)
                        {
                            try
                            {
                                oDefaultValue = opNewRow[this[rrc.Default[0].ColumnName].DataField];
                            }
                            catch { }
                        }
                        else if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            string defaultValue = rrc.GetStringDefaultValue(this.TableName);
                            if (StrFunc.IsEmpty(defaultValue))
                                oDefaultValue = Convert.DBNull;
                            else
                                oDefaultValue = defaultValue;
                        }
                    }
                    if (oDefaultValue != null && StrFunc.IsFilled(oDefaultValue.ToString()))
                        try
                        {
                            opNewRow[rrc.DataField] = oDefaultValue;
                        }
                        catch
                        {
                            opNewRow[rrc.DataField] = Convert.DBNull;
                        }
                }
            }
        }


        public void InitializeUpdateRow(DataRow pUpdRow)
        {
            for (int index = 0; index < this.Column.Length; index++)
            {
                ReferentialColumn rrc = this.Column[index];
                if (rrc.IsAdditionalData)
                {
                    //if external data : no creation until ID is affected                    
                }
                else
                {
                    Object oDefaultValue = null;
                    if (rrc.IsForeignKeyField)
                        oDefaultValue = this.valueForeignKeyField;
                    else if (rrc.IsMandatory && rrc.ExistsDDLType)
                        oDefaultValue = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_" + rrc.Relation[0].DDLType.Value);

                    if (oDefaultValue == null)
                    {
                        if (rrc.ExistsDefaultValue)
                        {
                            string defaultValue = rrc.GetStringDefaultValue2(this.TableName);
                            if (StrFunc.IsEmpty(defaultValue))
                                oDefaultValue = Convert.DBNull;
                            else
                                oDefaultValue = defaultValue;
                        }
                        else if (rrc.ExistsDefaultColumnName)
                        {
                            try
                            {
                                oDefaultValue = pUpdRow[this[rrc.Default[0].ColumnName].DataField];
                            }
                            catch { }
                        }
                        else if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            string defaultValue = rrc.GetStringDefaultValue2(this.TableName);
                            if (StrFunc.IsEmpty(defaultValue))
                                oDefaultValue = Convert.DBNull;
                            else
                                oDefaultValue = defaultValue;
                        }
                    }
                    if (oDefaultValue != null && StrFunc.IsFilled(oDefaultValue.ToString()))
                        try
                        {
                            pUpdRow[rrc.DataField] = oDefaultValue;
                        }
                        catch
                        {
                            pUpdRow[rrc.DataField] = Convert.DBNull;
                        }
                }
            }
        }
        #endregion

        /// <summary>
        /// Duplication d'un DataRow, colonne par colonne.
        /// <para>NB: Cette méthode est utilisée suite à un clic sur le bouton "Duliquer".</para>
        /// </summary>
        /// <param name="opNewRow"></param>
        /// <param name="pRow"></param>
        public void DuplicateRow(ref DataRow opNewRow, DataRow pRow, string pActorIdentifier)
        {
            // EG 20160308 Migration vs2013
            //bool isResetIdentifier = false;
            for (int index = 0; index < Column.Length; index++)
            {
                ReferentialColumn rrc = this.Column[index];
                if (!rrc.IsAdditionalData)
                {
                    try
                    {
                        //PL 20130416 Newness (Cela permet de ne plus mettre DISPLAYNAME IsMandatory dans les fichiers XML) 
                        if ((rrc.ColumnName == "IDENTIFIER") && (rrc.IsDataKeyField || rrc.IsKeyField))
                        {
                            //Init ou Reset de la valeur issue du record dupliqué.
                            // EG 20160308 Migration vs2013
                            //isResetIdentifier = true;
                            if (String.IsNullOrEmpty(pActorIdentifier))
                            {
                                opNewRow[rrc.DataField] = Convert.DBNull;
                            }
                            else
                            {
                                opNewRow[rrc.DataField] = pActorIdentifier;
                            }
                        }
                        else if ((rrc.ColumnName == "DISPLAYNAME") || (rrc.ColumnName == "ISTEMPLATE"))
                        {
                            //Reset de la valeur issue du record dupliqué.
                            opNewRow[rrc.DataField] = Convert.DBNull;
                        }
                        else
                        {
                            opNewRow[rrc.DataField] = pRow[rrc.DataField];
                        }
                    }
                    catch
                    {
                        opNewRow[rrc.DataField] = Convert.DBNull;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="pCondApp">Contient le nom de la condition d'application</param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// FI 20141211 [20563] Modification signature add paramètre pDynamicArgs
        public void Initialize(bool pIsIgnoreNotepadAndAttacheddoc, Nullable<Cst.ListType> pType, string pCondApp, string[] pParam, Dictionary<string, StringDynamicData> pDynamicArgs)
        {
            //PL 20120629 Add pIsIgnoreNotepadAndAttacheddoc afin, sur les consultations LST, de ne plus engendrer de jointure sur NOTEPAD et ATTACHEDOC.

            #region Initialisation
            HasMultiTable = false;

            m_IndexColSQL_DTCHK = -1;
            m_IndexColSQL_IDACHK = -1;
            m_IndexColSQL_IDCHK = -1;
            m_IndexColSQL_ISCHK = -1;

            m_IndexColSQL_DTUPD = -1;
            m_IndexColSQL_IDAUPD = -1;
            m_IndexColSQL_DTINS = -1;
            m_IndexColSQL_IDAINS = -1;
            m_IndexColSQL_DataKeyField = -1;
            m_IndexDataKeyField = -1;
            m_IndexKeyField = -1;
            m_IndexColSQL_KeyField = -1;
            m_IndexColSQL_ForeignKeyField = -1;
            m_IndexForeignKeyField = -1;
            m_IndexColSQL_DISPLAYNAME = -1;
            m_IndexColSQL_DESCRIPTION = -1;
            m_IndexColSQL_IDENTITY = -1;
            m_IndexColSQL_IDENTITYWithSource = -1;
            m_IndexColSQL_DTENABLED = -1;
            m_IndexColSQL_DTDISABLED = -1;
            m_IndexDTENABLED = -1;
            m_IndexDTDISABLED = -1;
            m_IndexEXTLLINK = -1;
            m_IndexEXTLLINK2 = -1;
            m_IndexEXTLATTRB = -1;
            m_IndexColSQL_EXTLLINK = -1;
            m_IndexColSQL_EXTLLINK2 = -1;
            m_IndexColSQL_EXTLATTRB = -1;
            m_IndexColSQL_ROWATTRIBUT = -1;
            m_IndexColSQL_ISSIDE = -1;
            m_IndexColSQL_DTHOLIDAYVALUE = -1;
            #endregion

            if (!ImportSpecified)
            {
                Import = false;
                ImportSpecified = true;
            }
            if (!ExportSpecified)
            {
                Export = false;
                ExportSpecified = true;
            }

            // FI 20141211 [20563] Appel à SetDynamicArgs
            if (null != pDynamicArgs)
                SetDynamicArgs(pDynamicArgs);

            this.TableName = ReplaceDynamicData(this.TableName, pParam);
            this.Ressource = ReplaceDynamicData(this.Ressource, pParam);

            #region ItemTableName - ReplaceDynamicData() - Oracle®: limitation à 30 car.
            if (this.ItemsSpecified)
            {
                this.Items.srctablename = ReplaceDynamicData(this.Items.srctablename, pParam);
                this.Items.tablename = ReplaceDynamicData(this.Items.tablename, pParam);
                this.Items.columnname = ReplaceDynamicData(this.Items.columnname, pParam);
                #region Items.columnname - Specific
                if (Items.columnname.IndexOf("(") > 0)
                {
                    int posOpenParenthesis = Items.columnname.IndexOf("(");
                    int posCloseParenthesis = Items.columnname.IndexOf(")");
                    switch (Items.columnname.Substring(0, posOpenParenthesis))
                    {
                        case "substring":
                            int posComma = Items.columnname.IndexOf(",");
                            int len = Convert.ToInt32(Items.columnname.Substring(posComma + 1, posCloseParenthesis - posComma - 1));
                            this.Items.columnname = this.Items.columnname.Substring(posOpenParenthesis + 1, len);
                            break;
                        case "PKofTable":
                            string table = this.Items.columnname.Substring(posOpenParenthesis + 1, posCloseParenthesis - posOpenParenthesis - 1);
                            this.Items.columnname = OTCmlHelper.GetColunmID(table);
                            break;
                    }
                }
                #endregion

                //PL 20111223 Add TBD... (See also: ReferentialTools.DeserializeXML_ForModeRW())
                if (DataHelper.isDbOracle(SessionTools.CS))
                {
                    //WARNING: En Oracle la longueur d'un objet est limitée à 30 car. 
                    //         Si une donnée d'une table dépasse cette taille on ne peut donc utiliser l'automate pour convertir 
                    //         ces données en tables "virtuelles" destinées à afficher chaque élément sous forme d'item.
                    string sqlSelect28 = SQLCst.SELECT + "1" + Cst.CrLf;
                    sqlSelect28 += SQLCst.FROM_DBO + this.Items.srctablename + Cst.CrLf;
                    sqlSelect28 += SQLCst.WHERE + DataHelper.SQLLength(SessionTools.CS, this.Items.columnname) + ">28";

                    object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, sqlSelect28);
                    if (obj != null)
                    {
                        //On considère dans ce cas cet élement comme "inexistant", car on ne peut offrir cette fonctionnalité.
                        this.ItemsSpecified = false;
                    }
                }
            }
            #endregion

            if (!AliasTableNameSpecified || StrFunc.IsEmpty(AliasTableName))
                AliasTableName = SQLCst.TBLMAIN;

            #region Replace in SQLSelect or SQLWhere/SQLJoin, constant by value and set null SQLWhere with pCondApp
            //
            InitializeSQLPreSelectCommand(pParam, pCondApp);
            //
            InitializeSQLSelectCommand(pParam, pCondApp);
            //
            InitializeSQLWhere(pParam, pCondApp);
            //
            InitializeSQLJoin(pParam);
            #endregion

            // EG 20110530 New
            #region ReplaceDynamicData for SQLRowStyle and SQLRowState
            if (this.SQLRowStyleSpecified)
                this.SQLRowStyle.Value = ReplaceDynamicData(this.SQLRowStyle.Value, pParam);
            if (this.SQLRowStateSpecified)
                this.SQLRowState.Value = ReplaceDynamicData(this.SQLRowState.Value, pParam);
            #endregion ReplaceDynamicData for SQLRowStyle and SQLRowState

            #region DTHOLIDAYVALUE: Create a second column
            //PL 20120116 Newness: Création de DTHOLIDAYNEXTDATE, colonne fictive associée à la colonne DTHOLIDAYVALUE
            ReferentialColumn rrc_DTHOLIDAYVALUE = this[Cst.OTCml_COL.DTHOLIDAYVALUE.ToString()];
            if (rrc_DTHOLIDAYVALUE != null)
            {
                //Modification de la colonne(Name et Type), pour bénéficier de la position.
                rrc_DTHOLIDAYVALUE.ColumnName = Cst.OTCml_COL.DTHOLIDAYNEXTDATE.ToString();
                rrc_DTHOLIDAYVALUE.DataType = new ReferentialColumnDataType();
                rrc_DTHOLIDAYVALUE.DataType.value = TypeData.TypeDataEnum.date.ToString();

                ArrayList aObjects = new ArrayList();
                for (int index = 0; index < Column.Length; index++)
                    aObjects.Add(((System.Array)Column).GetValue(index));

                //Ajout d'une nouvelle colonne identique à celle d'origine 
                ReferentialColumn newColumn = new ReferentialColumn();
                newColumn.ColumnName = Cst.OTCml_COL.DTHOLIDAYVALUE.ToString();
                newColumn.RessourceSpecified = rrc_DTHOLIDAYVALUE.RessourceSpecified;
                newColumn.DataType = new ReferentialColumnDataType();
                newColumn.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                if (newColumn.RessourceSpecified)
                    newColumn.Ressource = rrc_DTHOLIDAYVALUE.Ressource;
                newColumn.AliasTableNameSpecified = rrc_DTHOLIDAYVALUE.AliasTableNameSpecified;
                if (newColumn.AliasTableNameSpecified)
                    newColumn.AliasTableName = rrc_DTHOLIDAYVALUE.AliasTableName;
                newColumn.AliasColumnNameSpecified = rrc_DTHOLIDAYVALUE.AliasColumnNameSpecified;
                if (newColumn.AliasColumnNameSpecified)
                    newColumn.AliasColumnName = rrc_DTHOLIDAYVALUE.AliasColumnName;

                newColumn.IsVirtualColumnSpecified = rrc_DTHOLIDAYVALUE.IsVirtualColumnSpecified;
                if (newColumn.IsVirtualColumnSpecified)
                    newColumn.IsVirtualColumn = rrc_DTHOLIDAYVALUE.IsVirtualColumn;

                newColumn.IsHideInDataGridSpecified = true;
                newColumn.IsHideInDataGrid = true;
                newColumn.IsHideSpecified = true;
                newColumn.IsHide = true;

                aObjects.Add(newColumn);

                System.Type type = ((System.Array)Column).GetType().GetElementType();
                Column = (ReferentialColumn[])aObjects.ToArray(type);
            }
            #endregion

            int indexColSQL = 0;
            bool isTableStatistic = TableName.EndsWith("_S");
            bool isTablePreviousImage = TableName.EndsWith("_P");

            #region isTablePreviousImage --> Ajout automatique des colonnes complémentaires (ACTION_, ...)
            if (isTablePreviousImage)
            {
                //------------------------------------------------
                //PL 20120118 
                //------------------------------------------------
                this.Create = false;
                this.Modify = false;
                this.Remove = false;
                this.ItemsSpecified = false;
                this.RoleTableNameSpecified = false;
                //------------------------------------------------
                ArrayList aObjects = new ArrayList();
                ArrayList aObjectsRelation = new ArrayList();

                UseStatistic = false;
                System.Type typerelation = ((Array)this.Column[0].Relation).GetType().GetElementType();
                ReferentialColumnRelation newColumnRelation = new ReferentialColumnRelation();
                //newColumnRelation
                aObjectsRelation.Add(newColumnRelation);

                ReferentialColumn newColumn1 = new ReferentialColumn();
                newColumn1.ColumnName = "ID" + TableName;
                newColumn1.Ressource = "ID";
                newColumn1.RessourceSpecified = true;
                newColumn1.DataType = new ReferentialColumnDataType(); 
                newColumn1.DataType.value = "int";
                newColumn1.AliasTableNameSpecified = false;
                newColumn1.IsHideInDataGrid = false;
                newColumn1.IsHideInDataGridSpecified = true;
                newColumn1.IsHide = true;
                newColumn1.IsHideSpecified = true;
                newColumn1.IsMandatory = true;
                newColumn1.IsMandatorySpecified = true;
                newColumn1.IsAutoPostBack = false;
                newColumn1.IsAutoPostBackSpecified = true;
                newColumn1.IsVirtualColumn = false;
                newColumn1.IsVirtualColumnSpecified = true;
                newColumn1.IsDataKeyField = true;//Warning: Set IsDataKeyField to true
                newColumn1.IsDataKeyFieldSpecified = true;//PL 20120118
                newColumn1.IsOrderBy.Value = SQLCst.DESC.Trim(); //PL 20120118
                newColumn1.IsOrderBySpecified = true;
                newColumn1.Relation = (ReferentialColumnRelation[])aObjectsRelation.ToArray(typerelation);
                System.Type typeblock = ((System.Array)this.Column[0].html_BLOCK).GetType().GetElementType();
                ReferentialColumnhtml_BLOCK newColumnBlock = new ReferentialColumnhtml_BLOCK();
                newColumnBlock.title = "Tracks";
                newColumnBlock.columnbyrow = 2;
                newColumnBlock.columnbyrowSpecified = true;
                newColumnBlock.backcolorheader = System.Drawing.Color.DarkRed.Name;//PL 20120118
                newColumnBlock.backcolorheaderSpecified = true;
                System.Collections.ArrayList aObjectsBlock = new System.Collections.ArrayList();
                aObjectsBlock.Add(newColumnBlock);
                newColumn1.html_BLOCK = (ReferentialColumnhtml_BLOCK[])aObjectsBlock.ToArray(typeblock);
                aObjects.Add(newColumn1);
                ReferentialColumn newColumn5 = new ReferentialColumn();
                newColumn5.ColumnName = "ACTION_";
                newColumn5.Ressource = "ACTION_";
                newColumn5.RessourceSpecified = true;
                newColumn5.DataType = new ReferentialColumnDataType(); 
                newColumn5.DataType.value = "string";
                newColumn5.AliasTableNameSpecified = false;
                newColumn5.IsHideInDataGrid = false;
                newColumn5.IsHideInDataGridSpecified = true;
                newColumn5.IsHide = false;
                newColumn5.IsHideSpecified = true;
                newColumn5.IsMandatory = true;
                newColumn5.IsMandatorySpecified = true;
                //newColumn5.IsUpdatable.Value = false;
                //newColumn5.IsUpdatableSpecified = true;
                newColumn5.IsAutoPostBack = false;
                newColumn5.IsAutoPostBackSpecified = true;
                newColumn5.IsVirtualColumn = false;
                newColumn5.IsVirtualColumnSpecified = true;
                newColumn5.Relation = (ReferentialColumnRelation[])aObjectsRelation.ToArray(typerelation);
                aObjects.Add(newColumn5);
                ReferentialColumn newColumn2 = new ReferentialColumn();
                newColumn2.ColumnName = "DTSYS_";
                newColumn2.Ressource = "DTSYS_";
                newColumn2.RessourceSpecified = true;
                newColumn2.DataType = new ReferentialColumnDataType();
                newColumn2.DataType.value = "datetime";
                
                newColumn2.AliasTableNameSpecified = false;
                newColumn2.IsHideInDataGrid = false;
                newColumn2.IsHideInDataGridSpecified = true;
                newColumn2.IsHide = false;
                newColumn2.IsHideSpecified = true;
                newColumn2.IsMandatory = true;
                newColumn2.IsMandatorySpecified = true;
                //newColumn2.IsUpdatable.Value = false;
                //newColumn2.IsUpdatableSpecified = true;
                newColumn2.IsAutoPostBack = false;
                newColumn2.IsAutoPostBackSpecified = true;
                newColumn2.IsVirtualColumn = false;
                newColumn2.IsVirtualColumnSpecified = true;
                newColumn2.Relation = (ReferentialColumnRelation[])aObjectsRelation.ToArray(typerelation);
                aObjects.Add(newColumn2);
                ReferentialColumn newColumn3 = new ReferentialColumn();
                newColumn3.ColumnName = "USER_";
                newColumn3.Ressource = "USER_";
                newColumn3.RessourceSpecified = true;
                newColumn3.DataType = new ReferentialColumnDataType();
                newColumn3.DataType.value = "string";

                newColumn3.AliasTableNameSpecified = false;
                newColumn3.IsHideInDataGrid = false;
                newColumn3.IsHideInDataGridSpecified = true;
                newColumn3.IsHide = false;
                newColumn3.IsHideSpecified = true;
                newColumn3.IsMandatory = true;
                newColumn3.IsMandatorySpecified = true;
                //newColumn3.IsUpdatable.Value = false;
                //newColumn3.IsUpdatableSpecified = true;
                newColumn3.IsAutoPostBack = false;
                newColumn3.IsAutoPostBackSpecified = true;
                newColumn3.IsVirtualColumn = false;
                newColumn3.IsVirtualColumnSpecified = true;
                newColumn3.Relation = (ReferentialColumnRelation[])aObjectsRelation.ToArray(typerelation);
                aObjects.Add(newColumn3);
                ReferentialColumn newColumn4 = new ReferentialColumn();
                newColumn4.ColumnName = "HOSTNAME_";
                newColumn4.Ressource = "HOSTNAME_";
                newColumn4.RessourceSpecified = true;
                newColumn4.DataType = new ReferentialColumnDataType(); 
                newColumn4.DataType.value = "string";
                newColumn4.AliasTableNameSpecified = false;
                newColumn4.IsHideInDataGrid = false;
                newColumn4.IsHideInDataGridSpecified = true;
                newColumn4.IsHide = false;
                newColumn4.IsHideSpecified = true;
                newColumn4.IsMandatory = true;
                newColumn4.IsMandatorySpecified = true;
                //newColumn4.IsUpdatable.Value = false;
                //newColumn4.IsUpdatableSpecified = true;
                newColumn4.IsAutoPostBack = false;
                newColumn4.IsAutoPostBackSpecified = true;
                newColumn4.IsVirtualColumn = false;
                newColumn4.IsVirtualColumnSpecified = true;
                newColumn4.Relation = (ReferentialColumnRelation[])aObjectsRelation.ToArray(typerelation);
                aObjects.Add(newColumn4);

                for (int index = 0; index < Column.Length; index++)
                {
                    //Warning: Reinit de IsDataKeyField
                    Column[index].IsOrderBySpecified = false;   //PL 20120118
                    if (Column[index].IsDataKeyField)           //PL 20120118
                    {
                        Column[index].IsHideInDataGrid = false; //PL 20120118
                        Column[index].IsDataKeyField = false;
                        if (TypeData.IsTypeInt(Column[index].DataType.value)) //PL 20120118
                            Column[index].Ressource = "ID";
                    }
                    aObjects.Add(((System.Array)Column).GetValue(index));
                }

                System.Type type = ((System.Array)Column).GetType().GetElementType();
                Column = (ReferentialColumn[])aObjects.ToArray(type);
            }
            #endregion isTablePreviousImage

            #region isTableStatistic --> Todo
            if (isTableStatistic)
            {
                ArrayList aObjects = new ArrayList();
                ArrayList aObjectsRelation = new ArrayList();

                UseStatistic = false;
                // EG 20160308 Migration vs2013
                //#warning 20050528 PL (Not Urgent) Finaliser la gestion des Statistics
            }
            #endregion isTableStatistic

            #region UseStatisticSpecified --> Ajout automatique de la colonne LIBUSEFREQUENCY
            if (this.UseStatisticSpecified && (this.UseStatistic))
            {
                ArrayList aObjects = new System.Collections.ArrayList();
                ArrayList aObjectsRelation = new System.Collections.ArrayList();

                for (int index = 0; index < Column.Length; index++)
                {
                    aObjects.Add(((System.Array)Column).GetValue(index));
                }

                ReferentialColumn newColumn = new ReferentialColumn();
                newColumn.ColumnName = "LIBUSEFREQUENCY";
                newColumn.Ressource = "LIBUSEFREQUENCY";
                newColumn.RessourceSpecified = true;
                newColumn.DataType = new ReferentialColumnDataType(); 
                newColumn.DataType.value = "string";
                newColumn.RegularExpression = string.Empty;
                newColumn.AliasTableName = this.TableName + "_S";
                newColumn.AliasTableNameSpecified = true;
                newColumn.IsHideInDataGrid = false;
                newColumn.IsHideInDataGridSpecified = true;
                newColumn.IsHide = true;
                newColumn.IsHideSpecified = true;
                newColumn.IsMandatory = false;
                newColumn.IsMandatorySpecified = false;
                //newColumn.IsUpdatable.Value = false;
                //newColumn.IsUpdatableSpecified      = true;
                newColumn.IsAutoPostBack = false;
                newColumn.IsAutoPostBackSpecified = true;
                newColumn.IsVirtualColumn = false;
                newColumn.IsVirtualColumnSpecified = true;
                aObjects.Add(newColumn);

                ReferentialColumn newColumn2 = new ReferentialColumn();
                //"1" --> Never
                newColumn2.ColumnName = DataHelper.SQLIsNullChar(SessionTools.CS, this.TableName + "_S" + "." + "USEFREQUENCY", "1");
                newColumn2.Ressource = "USEFREQUENCY";
                newColumn2.RessourceSpecified = true;
                newColumn2.DataType = new ReferentialColumnDataType(); 
                newColumn2.DataType.value = "int";
                newColumn2.RegularExpression = string.Empty;
                newColumn2.AliasTableName = this.TableName + "_S";
                newColumn2.AliasTableNameSpecified = true;
                newColumn2.AliasColumnName = newColumn2.AliasTableName + "_" + "USEFREQUENCY";
                newColumn2.AliasColumnNameSpecified = true;
                newColumn2.IsHideInDataGrid = true;
                newColumn2.IsHideInDataGridSpecified = true;
                newColumn2.IsHide = true;
                newColumn2.IsHideSpecified = true;
                newColumn2.IsMandatory = false;
                newColumn2.IsMandatorySpecified = false;
                //newColumn2.IsUpdatable.Value = false;
                //newColumn2.IsUpdatableSpecified      = true;
                newColumn2.IsAutoPostBack = false;
                newColumn2.IsAutoPostBackSpecified = true;
                newColumn2.IsVirtualColumn = false;
                newColumn2.IsVirtualColumnSpecified = true;
                aObjects.Add(newColumn2);
                System.Type type = ((System.Array)Column).GetType().GetElementType();
                Column = (ReferentialColumn[])aObjects.ToArray(type);
            }
            #endregion UseStatisticSpecified
            //WARNING: Temporaire CC/FL/FDA/PL 20120126 Pb checkbox disabled / ex.: repository ACTOR
            //PL 20120221 Add test sur isUserReadOnly()
            //if (!SessionTools.IsSessionSysAdmin)
            if ((!SessionTools.IsSessionSysAdmin) || CSTools.isUserReadOnly(SessionTools.CS))
            {
                this.ItemsSpecified = false;
                this.RoleTableNameSpecified = false;
            }

            HasLengthInDatagrid = false;
            HasDynamicResource = false;
            HasDataTRIM = false;
            HasDataLightDisplay = false;

            ArrayList listJoinTable = new ArrayList();
            for (int index = 0; index < Column.Length; index++)
            {
                ReferentialColumn c = Column[index];
                c.IndexColSQL = indexColSQL;
                if (!(c.Scale > 0))
                    c.Scale = 0;

                c.ColumnName = ReplaceDynamicData(c.ColumnName, pParam);

                if (c.RessourceSpecified || (c.Ressource != null))
                {
                    c.RessourceSpecified = true;
                    c.Ressource = ReplaceDynamicData(c.Ressource, pParam);
                }
                else
                {
                    c.RessourceSpecified = true;
                    c.Ressource = string.Empty;
                }

                if (c.ExistsRelation)
                {
                    c.Relation[0].TableName = ReplaceDynamicData(c.Relation[0].TableName, pParam);
                    if (c.Relation[0].TableNameForDDLSpecified)
                        c.Relation[0].TableNameForDDL = ReplaceDynamicData(c.Relation[0].TableNameForDDL, pParam);

                    if (ArrFunc.IsFilled(c.Relation[0].ColumnSelect))
                    {
                        c.Relation[0].ColumnSelect[0].ColumnName = ReplaceDynamicData(c.Relation[0].ColumnSelect[0].ColumnName, pParam);
                        c.Relation[0].ColumnSelect[0].Ressource = ReplaceDynamicData(c.Relation[0].ColumnSelect[0].Ressource, pParam);
                    }
                    if (ArrFunc.IsFilled(c.Relation[0].ColumnLabel))
                    {
                        c.Relation[0].ColumnLabel[0].ColumnName = ReplaceDynamicData(c.Relation[0].ColumnLabel[0].ColumnName, pParam);
                        c.Relation[0].ColumnLabel[0].Ressource = ReplaceDynamicData(c.Relation[0].ColumnLabel[0].Ressource, pParam);
                    }
                    if (ArrFunc.IsFilled(c.Relation[0].ColumnRelation))
                        c.Relation[0].ColumnRelation[0].ColumnName = ReplaceDynamicData(c.Relation[0].ColumnRelation[0].ColumnName, pParam);

                    //PL 20111021 Refactoring Relation[0].AliasTableName 
                    if (c.Relation[0].AliasTableName == null)
                    {
                        if (listJoinTable.IndexOf(c.Relation[0].TableName) < 0)
                        {
                            c.Relation[0].AliasTableName = c.Relation[0].TableName;
                        }
                        else
                        {
                            int numAlias = 1;
                            while (listJoinTable.IndexOf(c.Relation[0].TableName + numAlias.ToString()) >= 0)
                            {
                                numAlias++;
                            }
                            c.Relation[0].AliasTableName = c.Relation[0].TableName + numAlias.ToString();
                        }
                    }
                    listJoinTable.Add(c.Relation[0].AliasTableName);
                }
                //PL 20110324 New
                // EG 20120417 Add Gestion Enum directement sur Tooltip en utilisant le tag URL (exemple: <URL>enum.SecurityClassEnum</URL>)
                else
                {
                    bool isDDLEnum = c.ExistsRelationDDLType && (c.Relation[0].DDLType.Value.IndexOf("enum") >= 0) && c.InformationSpecified && (!c.Information.URLSpecified);
                    bool isOtherEnum = (false == isDDLEnum) && c.InformationSpecified && c.Information.URLSpecified && (c.Information.URL.Value.IndexOf("enum") >= 0);
                    if (isDDLEnum || isOtherEnum)
                    {
                        string codeEnum = string.Empty;
                        if (isDDLEnum)
                            codeEnum = c.Relation[0].DDLType.Value;
                        if (isOtherEnum)
                            codeEnum = c.Information.URL.Value;
                        //codeEnum = c.Information.Type;
                        //Création automatique d'un hyperlink d'accès aux référentiels ENUMS
                        LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments(codeEnum, false);
                        if (null != loadEnum)
                        {
                            ReferentialInformationURL riURL = new ReferentialInformationURL();
                            riURL.@enum = loadEnum.code;
                            riURL.enumSpecified = true;
                            c.Information.URL = riURL;
                            c.Information.URLSpecified = true;
                        }
                    }
                }

                if (c.TypeAheadEnabled)
                {
                    c.TypeAhead.Table = ReplaceDynamicData(c.TypeAhead.Table, pParam);
                    if (StrFunc.IsFilled(c.TypeAhead.Values))
                        c.TypeAhead.Values = ReplaceDynamicData(c.TypeAhead.Values, pParam);
                }



                HasLengthInDatagrid = HasLengthInDatagrid || (c.LengthInDataGridSpecified);
                HasDynamicResource = HasDynamicResource || (c.IsResourceSpecified && c.IsResource.IsResource)
                    || (c.Ressource == @"/") || (c.Ressource == @":");
                HasDataTRIM = HasDataTRIM || (c.IsTRIMSpecified && c.IsTRIM);
                HasDataLightDisplay = HasDataLightDisplay || (c.IsHideOnLightDisplaySpecified && c.IsHideOnLightDisplay);

                #region ROWVERSION
                //20060524 PL Pour Oracle
                if (c.ColumnName == Cst.OTCml_COL.ROWVERSION.ToString())
                {
                    //20061114 PL Gestion du ROWVERSION a revoir
                    //if (isSqlServer)
                    //c.DataType = "int";
                    c.DataType = new ReferentialColumnDataType();
                    c.DataType.value = TypeData.TypeDataEnum.integer.ToString();//PL 20100914 Tuning
                    //else
                    //	c.DataType = "string";
                }
                #endregion

                if (!c.ColspanSpecified)
                    c.Colspan = 1;
                if (c.LabelWidth == null)
                    c.LabelWidth = string.Empty;
                if (c.InputWidth == null)
                    c.InputWidth = string.Empty;
                if (c.InputHeight == null)
                    c.InputHeight = 50.ToString();
                if (c.TextMode == null)
                {
                    if (TypeData.IsTypeText(c.DataType.value))
                        c.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine.ToString();
                    else
                        c.TextMode = System.Web.UI.WebControls.TextBoxMode.SingleLine.ToString();
                }
                if (c.ExistsDefault)
                {
                    if (c.Default[0].Value != null)
                        c.Default[0].Value = ReplaceDynamicData(c.Default[0].Value, pParam);
                }
                //PL 20120118 Set comment
                //if (isTablePreviousImage)
                //{
                //    if (!(c.IsDataKeyField || c.IsKeyField || (c.ColumnName == "IDENTIFIER") || (c.ColumnName == "DISPLAYNAME")
                //        || (c.ColumnName == "DESCRIPTION") || (c.ColumnName == "EXTLLINK") || (c.ColumnName == "ACTION_")
                //        || (c.ColumnName == "DTSYS_") || (c.ColumnName == "USER_") || (c.ColumnName == "HOSTNAME_")))
                //        c.IsHideInDataGrid = true;
                //}

                #region ListType --> utilisé par les boutons "..."
                if (true == c.ExistsRelation)
                {
                    if (false == c.Relation[0].ListTypeSpecified || c.Relation[0].ListType == null)
                    {
                        c.Relation[0].ListTypeSpecified = true;
                        c.Relation[0].ListType = Cst.ListType.Repository.ToString();
                    }
                    else
                    {
                        try
                        {
                            //Cst.ListType myEnum = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), c.Relation[0].ListType, true);
                        }
                        catch (ArgumentException)
                        {
                            Exception ex = new Exception("Invalid ListType specified : " + c.Relation[0].ListType + " is not a part of Cst.ListType");
                            throw ex;
                        }
                    }
                }
                #endregion ListType
                if (!c.AliasTableNameSpecified || c.AliasTableName == null)
                {
                    c.AliasTableName = this.AliasTableName;
                    // 20081023 EG 
                    c.AliasTableNameSpecified = StrFunc.IsFilled(c.AliasTableName);
                }
                if (StrFunc.IsEmpty(c.DataField))
                {
                    if (c.AliasColumnNameSpecified && StrFunc.IsFilled(c.AliasColumnName))
                        c.DataField = c.AliasColumnName;
                    else
                        c.DataField = c.ColumnName;
                }

                if (c.CellStyleSpecified && c.CellStyle.modelSpecified && (c.CellStyle.model.Contains("side")))
                    m_IndexColSQL_ISSIDE = indexColSQL;
                // 20090805 RD Pour gérére la nouvelle colonne CellStyle 
                HasColumnsWithStyle = HasColumnsWithStyle || c.CellStyleSpecified;
                // EG 20110105 Gestion de la nouvelle propriété IsEditable 
                HasEditableColumns = HasEditableColumns || c.IsEditableSpecified;
                // RD 20110704 [17501] / Suppression des deux colonnes LSTCOLUMN.ISSIDE et LSTCOLUMN.ISQUANTITY

                #region switch (c.ColumnName) --> Valorise les variables m_Index...
                switch (c.ColumnName)
                {
                    case "DISPLAYNAME":
                        m_IndexColSQL_DISPLAYNAME = indexColSQL;
                        break;
                    case "DESCRIPTION":
                        m_IndexColSQL_DESCRIPTION = indexColSQL;
                        break;
                    case "EXTLLINK":
                        m_IndexColSQL_EXTLLINK = indexColSQL;
                        m_IndexEXTLLINK = index;
                        break;
                    case "EXTLLINK2":
                        m_IndexColSQL_EXTLLINK2 = indexColSQL;
                        m_IndexEXTLLINK2 = index;
                        break;
                    case "EXTLATTRB":
                        m_IndexColSQL_EXTLATTRB = indexColSQL;
                        m_IndexEXTLATTRB = index;
                        break;
                    case "DTENABLED":
                        m_IndexColSQL_DTENABLED = indexColSQL;
                        m_IndexDTENABLED = index;
                        break;
                    case "DTDISABLED":
                        m_IndexColSQL_DTDISABLED = indexColSQL;
                        m_IndexDTDISABLED = index;
                        break;
                    case "DTUPD":
                        m_IndexColSQL_DTUPD = indexColSQL;
                        break;
                    case "IDAUPD":
                        m_IndexColSQL_IDAUPD = indexColSQL;
                        break;
                    case "DTINS":
                        m_IndexColSQL_DTINS = indexColSQL;
                        break;
                    case "IDAINS":
                        m_IndexColSQL_IDAINS = indexColSQL;
                        break;
                    case "ROWATTRIBUT"://Cst.OTCml_COL.ROWATTRIBUT
                        m_IndexColSQL_ROWATTRIBUT = indexColSQL;
                        break;
                    case "DTHOLIDAYVALUE"://Cst.OTCml_COL.DTHOLIDAYVALUE
                        m_IndexColSQL_DTHOLIDAYVALUE = indexColSQL;
                        break;

                    case "DTCHK":
                        m_IndexColSQL_DTCHK = indexColSQL;
                        break;
                    case "IDACHK":
                        m_IndexColSQL_IDACHK = indexColSQL;
                        break;
                    case "IDCHK":
                        m_IndexColSQL_IDCHK = indexColSQL;
                        break;
                    case "ISCHK":
                        m_IndexColSQL_ISCHK = indexColSQL;
                        break;

                    default:
                        if (c.IsDataKeyField)
                        {
                            m_IndexDataKeyField = index;
                            m_IndexColSQL_DataKeyField = indexColSQL;
                        }
                        if (c.IsKeyField)
                        {
                            m_IndexKeyField = index;
                            m_IndexColSQL_KeyField = indexColSQL;
                        }
                        if (c.IsForeignKeyField)
                        {
                            m_IndexForeignKeyField = index;
                            m_IndexColSQL_ForeignKeyField = indexColSQL;
                        }
                        if (c.IsIdentity.Value)
                        {
                            m_IndexIDENTITY = index;
                            m_IndexColSQL_IDENTITY = indexColSQL;
                            if (c.IsIdentity.sourceSpecified)
                            {
                                m_IndexColSQL_IDENTITYWithSource = indexColSQL; ;
                            }
                        }
                        break;
                }
                #endregion switch (c.ColumnName)
                if (c.ExistsRelation)
                {
                    indexColSQL++;
                    #region Replace in SQLWhere constant by value
                    if ((c.Relation[0].Condition != null) && (c.Relation[0].Condition[0].SQLWhere != null))
                        c.Relation[0].Condition[0].SQLWhere = ReplaceDynamicData(c.Relation[0].Condition[0].SQLWhere, pParam);
                    #endregion
                }
                indexColSQL++;
            }
            //PL 20110526 Compatibilité ascendante
            if (!LoadOnStartSpecified)
            {
                LoadOnStartSpecified = true;
                LoadOnStart = true; //Default
                if (pType.HasValue)
                {
                    if ((pType == Cst.ListType.ProcessBase) || (pType == Cst.ListType.Price) || (pType == Cst.ListType.Log)
                        || (pType == Cst.ListType.Accounting) || (pType == Cst.ListType.Invoicing) || (pType == Cst.ListType.Report)
                        || (pType == Cst.ListType.ConfirmationMsg) || (pType == Cst.ListType.SettlementMsg) || (pType == Cst.ListType.Consultation))
                        LoadOnStart = false;
                }
            }

            if (m_IndexColSQL_KeyField == -1)
                //La colonne PK est a priori une colonne de données et non pas un identity 
                m_IndexColSQL_KeyField = m_IndexColSQL_DataKeyField;

            if (!this.ColumnByRowSpecified)
                ColumnByRow = 2;
            if (StrFunc.IsEmpty(Image))
            {
                if (!Create && !Modify && !Remove)
                    Image = CSS.Main.loupe.ToString();
                else
                    Image = CSS.Main.settings.ToString();
            }
            if (!pIsIgnoreNotepadAndAttacheddoc)
            {
                if (!NotepadSpecified)
                {
                    NotepadSpecified = true;
                    if (IndexDataKeyField == -1)
                        Notepad.Value = false;
                    else
                        Notepad.Value = TypeData.IsTypeInt(this.Column[this.IndexDataKeyField].DataType.value) || TypeData.IsTypeString(this.Column[this.IndexDataKeyField].DataType.value);
                }
                if (!AttachedDocSpecified)
                {
                    AttachedDocSpecified = true;
                    AttachedDoc.Value = this.Notepad.Value;
                    if (this.Notepad.tablenameSpecified)
                    {
                        AttachedDoc.tablenameSpecified = true;
                        AttachedDoc.tablename = this.Notepad.tablename;
                        if (this.Notepad.IDSpecified)
                        {
                            AttachedDoc.IDSpecified = true;
                            AttachedDoc.ID = this.Notepad.ID;
                        }
                    }
                }
            }
            //Type de PK (alpha ou num) pour les tables EXTLID(S), ATTACHEDDOC(S), ...
            if (this.IndexDataKeyField != -1)
                this.IsDataKeyField_String = !TypeData.IsTypeNumeric(this.Column[this.IndexDataKeyField].DataType.value);

            #region tables NOTEPAD(S) and ATTACHEDDOC(S)
            if (m_IndexDataKeyField != -1 && Column[m_IndexDataKeyField].ColumnName != Cst.OTCml_COL.ROWVERSION.ToString())
            {
                bool isNotepad = (NotepadSpecified && Notepad.Value);
                bool isAttachedDoc = (AttachedDocSpecified && AttachedDoc.Value);
                bool isBoth = (isNotepad && isAttachedDoc);
                bool isHideInDatagrid = true;
                if (isNotepad || isAttachedDoc)
                {
                    ReferentialColumn rrcolumnDataKeyField = Column[m_IndexDataKeyField];
                    string tblNOTEPAD = Cst.OTCml_TBL.NOTEPAD.ToString();
                    string tblATTACHEDDOC = Cst.OTCml_TBL.ATTACHEDDOC.ToString();
                    string tblACTOR = Cst.OTCml_TBL.ACTOR.ToString();
                    if (IsDataKeyField_String)
                    {
                        tblNOTEPAD += "S";
                        tblATTACHEDDOC += "S";
                    }
                    #region Add Join for tables NOTEPAD(S) and ATTACHEDDOC(S)
                    string[] SQLJoinsAdd = new string[(isBoth ? 4 : 2)];
                    int currentIndex = -1;
                    #region NOTEPAD(S)
                    if (isNotepad)
                    {
                        currentIndex++;
                        SQLJoinsAdd[currentIndex] = SQLCst.LEFTJOIN_DBO + tblNOTEPAD + " " + Cst.AliasNOTEPAD
                            + SQLCst.ON + "(" + Cst.AliasNOTEPAD + ".TABLENAME=" + DataHelper.SQLString(this.TableName)
                            + SQLCst.AND + Cst.AliasNOTEPAD + ".ID=" + this.AliasTableName + "." + rrcolumnDataKeyField.ColumnName + ")";
                        currentIndex++;
                        SQLJoinsAdd[currentIndex] = SQLCst.LEFTJOIN_DBO + tblACTOR + " " + Cst.AliasNOTEPADACTOR
                            + SQLCst.ON + "(" + Cst.AliasNOTEPADACTOR + ".IDA=" + Cst.AliasNOTEPAD + ".IDAUPD)";
                    }
                    #endregion
                    #region ATTACHEDDOC(S)
                    if (isAttachedDoc)
                    {
                        string qry;
                        string tblName = DataHelper.SQLString(this.TableName);
                        string aliasMaxAD = "max" + Cst.AliasATTACHEDDOC;
                        string aliasAD = Cst.AliasATTACHEDDOC;
                        string aliasADA = Cst.AliasATTACHEDDOCACTOR;

                        //Left outer join on ATTACHEDDOC table
                        //PL 20121112 Add max(DOCNAME) pour évtier doublon avec Spheres I/O qui attache plusieurs pièces jointes à une même datetime
                        qry = SQLCst.X_LEFT + "(" + SQLCst.SELECT + "ID,max(DTUPD) as DTUPD,max(DOCNAME) as DOCNAME" + SQLCst.FROM_DBO + tblATTACHEDDOC + SQLCst.WHERE + "TABLENAME=" + tblName + SQLCst.GROUPBY + "ID) " + aliasMaxAD + " on " + aliasMaxAD + ".ID=" + this.AliasTableName + "." + rrcolumnDataKeyField.ColumnName + Cst.CrLf;
                        qry += SQLCst.LEFTJOIN_DBO + tblATTACHEDDOC + " " + aliasAD + " on (" + aliasAD + ".TABLENAME=" + tblName;
                        qry += SQLCst.AND + aliasAD + ".ID=" + this.AliasTableName + "." + rrcolumnDataKeyField.ColumnName;
                        qry += SQLCst.AND + aliasAD + ".DTUPD=" + aliasMaxAD + ".DTUPD";
                        qry += SQLCst.AND + aliasAD + ".DOCNAME=" + aliasMaxAD + ".DOCNAME)";

                        //20060524 PL Code mis en commentaire car bug en Oracle9i (left outer avec sous-select)
                        // EG 20160308 Migration vs2013
                        //#warning 20050331 PL left outer avec sous-select à gérer sous Oracle 9i (plantage)
                        //*************************************************************************************************
                        ////qry += SQLCst.AND + aliasAD + ".DTUPD ";
                        ////if (DataHelper.isDbSqlServer(SessionTools.CS))
                        ////{
                        ////    qry += "= (";
                        ////    qry += SQLCst.SELECT + SQLCst.MAX + "(DTUPD)" + SQLCst.FROM_DBO + tblATTACHEDDOC + " max" + aliasAD;
                        ////    qry += SQLCst.WHERE + " max" + aliasAD + ".TABLENAME = " + tblName;
                        ////    qry += SQLCst.AND + " max" + aliasAD + ".ID = " + this.AliasTableName + "." + rrcolumnDataKeyField.ColumnName;
                        ////    qry += ")";
                        ////}
                        ////else
                        ////    qry += " is null";
                        //////*************************************************************************************************
                        ////qry += ")";
                        currentIndex++;
                        SQLJoinsAdd[currentIndex] = qry;

                        //Left outer join on ACTOR table for ATTACHEDDOC
                        qry = SQLCst.LEFTJOIN_DBO + tblACTOR + " " + aliasADA + SQLCst.ON + "(" + aliasADA + ".IDA = " + aliasAD + ".IDAUPD)";

                        currentIndex++;
                        SQLJoinsAdd[currentIndex] = qry;

                        //*******************
                        //20060914 RD

                        // 1 - Ajouter un Where
                        qry = "( ( " + aliasAD + ".DTUPD = ";
                        qry += "(";
                        qry += SQLCst.SELECT + SQLCst.MAX + "(max" + aliasAD + ".DTUPD)" + SQLCst.AS + "DTUPD";
                        qry += SQLCst.FROM_DBO + tblATTACHEDDOC + " max" + aliasAD + ", " + this.TableName + " " + this.AliasTableName;
                        qry += SQLCst.WHERE + " max" + aliasAD + ".TABLENAME = " + tblName;
                        qry += SQLCst.AND + " max" + aliasAD + ".ID = " + this.AliasTableName + "." + rrcolumnDataKeyField.ColumnName;
                        qry += ") ";
                        qry += ") " + SQLCst.OR + "( " + aliasAD + ".DTUPD" + SQLCst.IS_NULL + " ))";
                        //******************							
                    }
                    #endregion
                    #region Add in SQLJoin
                    if (SQLJoin == null || SQLJoin.GetLength(0) == 0)
                    {
                        SQLJoin = SQLJoinsAdd;
                        SQLJoinSpecified = true;
                    }
                    else
                    {
                        string[] newSQLJoin = new string[SQLJoin.GetLength(0) + SQLJoinsAdd.GetLength(0)];
                        for (int i = 0; i < SQLJoin.GetLength(0); i++)
                            newSQLJoin[i] = SQLJoin[i];
                        for (int i = SQLJoin.GetLength(0); i < SQLJoin.GetLength(0) + SQLJoinsAdd.GetLength(0); i++)
                            newSQLJoin[i] = SQLJoinsAdd[i - SQLJoin.GetLength(0)];
                        SQLJoin = newSQLJoin;
                    }
                    #endregion
                    #endregion Add Join for tables NOTEPAD(S) and ATTACHEDDOC(S)

                    #region add Column for tables NOTEPAD(S) and ATTACHEDDOC(S)
                    System.Collections.ArrayList alColumns = new System.Collections.ArrayList();
                    for (int index = 0; index < Column.Length; index++)
                    {
                        alColumns.Add(((System.Array)Column).GetValue(index));
                    }
                    //NOTEPAD(S) ACTOR.DISPLAYNAME & ACTOR.DTUPD
                    if (isNotepad)
                    {
                        ReferentialColumn newColumn0 = new ReferentialColumn();
                        newColumn0.ColumnName = "DISPLAYNAME";
                        newColumn0.Ressource = "NotePad";
                        newColumn0.RessourceSpecified = true;
                        newColumn0.DataType = new ReferentialColumnDataType(); 
                        newColumn0.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                        newColumn0.RegularExpression = string.Empty;
                        newColumn0.AliasTableName = Cst.AliasNOTEPADACTOR;
                        newColumn0.AliasTableNameSpecified = true;
                        newColumn0.IsHideInDataGrid = isHideInDatagrid;
                        newColumn0.IsHideInDataGridSpecified = true;
                        newColumn0.IsHide = true;
                        newColumn0.IsHideSpecified = true;
                        newColumn0.IsMandatory = false;
                        newColumn0.IsMandatorySpecified = false;
                        //newColumn0.IsUpdatable.Value = false;
                        //newColumn0.IsUpdatableSpecified      = true;
                        newColumn0.IsAutoPostBack = false;
                        newColumn0.IsAutoPostBackSpecified = true;
                        newColumn0.IsVirtualColumn = false;
                        newColumn0.IsVirtualColumnSpecified = true;
                        newColumn0.DataField = newColumn0.AliasTableName + "_" + newColumn0.ColumnName;
                        alColumns.Add(newColumn0);
                        ReferentialColumn newColumn1 = new ReferentialColumn();
                        newColumn1.ColumnName = "DTUPD";
                        newColumn1.Ressource = "NotePadDtUpd";
                        newColumn1.RessourceSpecified = true;
                        newColumn1.DataType = new ReferentialColumnDataType();
                        newColumn1.DataType.value = TypeData.TypeDataEnum.@datetime.ToString();
                        newColumn1.RegularExpression = string.Empty;
                        newColumn1.AliasTableName = Cst.AliasNOTEPAD;
                        newColumn1.AliasTableNameSpecified = true;
                        newColumn1.IsHideInDataGrid = isHideInDatagrid;
                        newColumn1.IsHideInDataGridSpecified = true;
                        newColumn1.IsHide = true;
                        newColumn1.IsHideSpecified = true;
                        newColumn1.IsMandatory = false;
                        newColumn1.IsMandatorySpecified = false;
                        //newColumn1.IsUpdatable.Value = false;
                        //newColumn1.IsUpdatableSpecified      = true;
                        newColumn1.IsAutoPostBack = false;
                        newColumn1.IsAutoPostBackSpecified = true;
                        newColumn1.IsVirtualColumn = false;
                        newColumn1.IsVirtualColumnSpecified = true;
                        newColumn1.DataField = newColumn1.AliasTableName + "_" + newColumn1.ColumnName;
                        alColumns.Add(newColumn1);
                    }
                    //ATTACHEDDOC(S) ACTOR.DISPLAYNAME & ACTOR.DTUPD                    
                    if (isAttachedDoc)
                    {
                        ReferentialColumn newColumn2 = new ReferentialColumn();
                        newColumn2.ColumnName = "DISPLAYNAME";
                        newColumn2.Ressource = "AttachedDoc";
                        newColumn2.RessourceSpecified = true;
                        newColumn2.DataType = new ReferentialColumnDataType(); 
                        newColumn2.DataType.value = TypeData.TypeDataEnum.@string.ToString();
                        newColumn2.RegularExpression = string.Empty;
                        newColumn2.AliasTableName = Cst.AliasATTACHEDDOCACTOR;
                        newColumn2.AliasTableNameSpecified = true;
                        newColumn2.IsHideInDataGrid = isHideInDatagrid;
                        newColumn2.IsHideInDataGridSpecified = true;
                        newColumn2.IsHide = true;
                        newColumn2.IsHideSpecified = true;
                        newColumn2.IsMandatory = false;
                        newColumn2.IsMandatorySpecified = false;
                        //newColumn2.IsUpdatable.Value = false;
                        //newColumn2.IsUpdatableSpecified      = true;
                        newColumn2.IsAutoPostBack = false;
                        newColumn2.IsAutoPostBackSpecified = true;
                        newColumn2.IsVirtualColumn = false;
                        newColumn2.IsVirtualColumnSpecified = true;
                        newColumn2.DataField = newColumn2.AliasTableName + "_" + newColumn2.ColumnName;
                        alColumns.Add(newColumn2);
                        ReferentialColumn newColumn3 = new ReferentialColumn();
                        newColumn3.ColumnName = "DTUPD";
                        newColumn3.Ressource = "AttachedDocDtUpd";
                        newColumn3.RessourceSpecified = true;
                        newColumn3.DataType = new ReferentialColumnDataType(); 
                        newColumn3.DataType.value = TypeData.TypeDataEnum.datetime.ToString(); ;
                        newColumn3.RegularExpression = string.Empty;
                        newColumn3.AliasTableName = Cst.AliasATTACHEDDOC;
                        newColumn3.AliasTableNameSpecified = true;
                        newColumn3.IsHideInDataGrid = isHideInDatagrid;
                        newColumn3.IsHideInDataGridSpecified = true;
                        newColumn3.IsHide = true;
                        newColumn3.IsHideSpecified = true;
                        newColumn3.IsMandatory = false;
                        newColumn3.IsMandatorySpecified = false;
                        //newColumn3.IsUpdatable.Value = false;
                        //newColumn3.IsUpdatableSpecified      = true;
                        newColumn3.IsAutoPostBack = false;
                        newColumn3.IsAutoPostBackSpecified = true;
                        newColumn3.IsVirtualColumn = false;
                        newColumn3.IsVirtualColumnSpecified = true;
                        newColumn3.DataField = newColumn3.AliasTableName + "_" + newColumn3.ColumnName;
                        alColumns.Add(newColumn3);
                    }
                    System.Type type = ((System.Array)Column).GetType().GetElementType();
                    Column = (ReferentialColumn[])alColumns.ToArray(type);
                    #endregion add Column for tables NOTEPAD(S) and ATTACHEDDOC(S)
                }
            }
            #endregion tables NOTEPAD(S) and ATTACHEDDOC(S)
        }

        /// <summary>
        /// Initiale SQLSelectCommand
        /// </summary>
        /// FI 20141211 [20563] add parameter pCondApp
        public void InitializeSQLSelectCommand(string[] pParam, string pCondApp)
        {
            //
            //Select
            //Les commandes présentes ds SQLSelect sont prioritaires vis à vis des commandes resources
            //Cela permet de modifier le xml ou les lst de manière et d'écraser la resource incorporée
            if (SQLSelectResourceSpecified && (false == SQLSelectSpecified))
            {
                ReferentialSQLSelects rrSqlSelects = RepositoryTools.LoadReferentialsReferentialSQLSelect(SQLSelectResource.name);
                SQLSelect = rrSqlSelects.item;
                SQLSelectSpecified = true;
            }
            if (SQLSelectSpecified)
            {
                string[] sqlCmd = GetSqlCommand(SQLSelect, pParam, pCondApp);
                if (ArrFunc.IsFilled(sqlCmd))
                {
                    //Il ne peut exister qu'une commande
                    SQLSelectCommand = sqlCmd[0];
                }
                SQLSelectSpecified = StrFunc.IsFilled(SQLSelectCommand);
            }
        }

        /// <summary>
        /// Initiale SQLPreSelectCommand
        /// </summary>
        public void InitializeSQLPreSelectCommand(string[] pParam, string pCondApp)
        {
            //Preselect
            //Les commandes présentes ds SQLPreSelect sont prioritaires vis à vis des commandes resources
            //Cela permet de modifier le xml ou les lst de manière et d'écraser la resource incorporée
            if (SQLPreSelectResourceSpecified && (false == SQLPreSelectSpecified))
            {
                ReferentialSQLSelects rrSqlSelects = RepositoryTools.LoadReferentialsReferentialSQLSelect(SQLPreSelectResource.name);
                SQLPreSelect = rrSqlSelects.item;
                SQLPreSelectSpecified = true;
            }
            if (SQLPreSelectSpecified)
            {
                SQLPreSelectCommand = GetSqlCommand(SQLPreSelect, pParam, pCondApp);
                SQLPreSelectSpecified = ArrFunc.IsFilled(SQLPreSelectCommand);
            }
        }

        /// <summary>
        /// Initiale SQLWhere
        /// </summary>
        /// <param name="pParam"></param>
        /// <param name="pCondApp"></param>
        /// FI 20141107 [20441] Modify
        /// FI 20141201 [20533] Modify
		/// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void InitializeSQLWhere(string[] pParam, string pCondApp)
        {
            if (ArrFunc.IsFilled(SQLWhere))
            {
                for (int index = 0; index < SQLWhere.Length; index++)
                {
                    if (StrFunc.IsFilled(SQLWhere[index].ConditionApplication))
                    {
                        //lorsque ConditionApplication du where ne correspond pas au contexte retrait des infos
                        //ie  il n'existe pas de pCondApp ou ConditionApplication n'existe pas ds pCondApp
                        if (StrFunc.IsEmpty(pCondApp) || (pCondApp != SQLWhere[index].ConditionApplication))
                            SQLWhere[index] = null;
                    }
                    //
                    if (SQLWhere[index] != null)
                    {
                        if (StrFunc.IsFilled(SQLWhere[index].ConditionSystem))
                        {
                            switch (SQLWhere[index].ConditionSystem)
                            {
                                case Cst.SESSIONRESTRICT:
                                    //FI 20141107 call method User.IsApplySessionRestrict()
                                    if (false == SessionTools.User.IsApplySessionRestrict())
                                        SQLWhere[index] = null;
                                    break;
                            }
                        }
                    }
                    //
                    if (SQLWhere[index] != null)
                    {
                        if (SQLWhere[index].ConditionSystemSpecified && SQLWhere[index].ConditionSystem == Cst.SESSIONRESTRICT)
                        {
                            if (SQLWhere[index].SQLWhereSpecified)
                            {
                                if (SQLWhere[index].SQLWhere.StartsWith(Cst.SR_START))
                                {
                                    SessionRestrictHelper srH = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, this.IsUseSQLParameters);
                                    string join = null;
                                    string where = null;
                                    string[] arg = null;
                                    string alias1 = string.Empty;
                                    string alias2 = string.Empty;

                                    switch (SQLWhere[index].SQLWhere)
                                    {
                                        case Cst.SR_TRADE_WHERE_PREDICATE:
                                            // FI 20141107 [20441] 
                                            arg = StrFunc.GetArgumentKeyWord(SQLWhere[index].SQLJoin[0], Cst.SR_TRADE_JOIN);
                                            if (ArrFunc.Count(arg) >= 2)
                                                alias1 = arg[1];
                                            // FI 20160810 [22086] add addTradeMissing 
                                            AddMissingTrade addTradeMissing = AddMissingTrade.no;
                                            if (ArrFunc.Count(arg) >= 3)
                                                addTradeMissing = ReflectionTools.ConvertStringToEnumOrDefault<AddMissingTrade>(arg[2], AddMissingTrade.no);

                                            srH.GetSQLTrade(arg[0], alias1, addTradeMissing, out join, out where);

                                            SQLWhere[index].SQLWhere = where;
                                            break;
                                        case Cst.SR_TRADEALLOC_WHERE_PREDICATE:
                                            // FI 20141107 [20441] 
                                            arg = StrFunc.GetArgumentKeyWord(SQLWhere[index].SQLJoin[0], Cst.SR_TRADEALLOC_JOIN);
                                            alias1 = string.Empty;
                                            if (ArrFunc.Count(arg) >= 2)
                                                alias1 = arg[1];
                                            alias2 = string.Empty;
                                            if (ArrFunc.Count(arg) >= 3)
                                                alias2 = arg[2];
                                            // FI 20160810 [22086] add addMissingTrade
                                            AddMissingTrade addMissingTrade = AddMissingTrade.no;
                                            if (ArrFunc.Count(arg) >= 4)
                                                addTradeMissing = ReflectionTools.ConvertStringToEnumOrDefault<AddMissingTrade>(arg[3], AddMissingTrade.no);
                                            srH.GetSQLTradeAlloc(arg[0], alias1, alias2, addMissingTrade, out join, out where);
                                            SQLWhere[index].SQLWhere = where;
                                            break;
                                        case Cst.SR_TRADERISK_WHERE_PREDICATE:
                                            arg = StrFunc.GetArgumentKeyWord(SQLWhere[index].SQLJoin[0], Cst.SR_TRADERISK_JOIN);
                                            alias1 = string.Empty;
                                            if (ArrFunc.Count(arg) >= 2)
                                                alias1 = arg[1];
                                            srH.GetSQLTradeRisk(arg[0], alias1, out join, out where);
                                            SQLWhere[index].SQLWhere = where;
                                            break;
                                        case Cst.SR_TRADEADMIN_WHERE_PREDICATE:
                                            arg = StrFunc.GetArgumentKeyWord(SQLWhere[index].SQLJoin[0], Cst.SR_TRADEADMIN_JOIN);
                                            alias1 = string.Empty;
                                            if (ArrFunc.Count(arg) >= 2)
                                                alias1 = arg[1];
                                            srH.GetSQLTradeAdmin(AliasTableName + ".IDT", alias1, out join, out where);
                                            SQLWhere[index].SQLWhere = where;
                                            break;
                                        case Cst.SR_TRADEDEBTSEC_WHERE_PREDICATE:
                                            arg = StrFunc.GetArgumentKeyWord(SQLWhere[index].SQLJoin[0], Cst.SR_TRADEDEBTSEC_JOIN);
                                            alias1 = string.Empty;
                                            if (ArrFunc.Count(arg) >= 2)
                                                alias1 = arg[1];
                                            srH.GetSQLTradeDebtSec(AliasTableName + ".IDT", alias1, out join, out where);
                                            SQLWhere[index].SQLWhere = where;
                                            break;
                                        case Cst.SR_POSCOLLATERAL_WHERE_PREDICATE:
                                            srH.GetSQLPosCollateral(AliasTableName, out join, out where);
                                            SQLWhere[index].SQLWhere = where;
                                            break;
                                    }
                                }
                            }
                            //
                            if ((SQLWhere[index].SQLJoinSpecified))
                            {
                                for (int i = 0; i < ArrFunc.Count(SQLWhere[index].SQLJoin); i++)
                                {
                                    if (StrFunc.IsFilled(SQLWhere[index].SQLJoin[i]) &&
                                        SQLWhere[index].SQLJoin[i].StartsWith(Cst.SR_START))
                                    {
                                        SessionRestrictHelper srH = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, IsUseSQLParameters);
                                        SQLWhere[index].SQLJoin[i] = srH.ReplaceKeyword(SQLWhere[index].SQLJoin[i]);
                                    }
                                }
                            }
                        }
                        //    
                        if (SQLWhere[index].SQLWhereSpecified)
                            SQLWhere[index].SQLWhere = ReplaceDynamicData(SQLWhere[index].SQLWhere, pParam);
                        if (SQLWhere[index].LstValueSpecified)
                            SQLWhere[index].LstValue = ReplaceDynamicData(SQLWhere[index].LstValue, pParam);
                        if (SQLWhere[index].SQLJoinSpecified)
                        {
                            for (int i = 0; i < SQLWhere[index].SQLJoin.Length; i++)
                                SQLWhere[index].SQLJoin[i] = ReplaceDynamicData(SQLWhere[index].SQLJoin[i], pParam);
                        }
                    }
                }

                //FI 20141201 [20533] Call ReflectionTools.RemoveItemInArray de manière à supprimer les items qui sont à null
                for (int i = 0; i < ArrFunc.Count(SQLWhere); i++)
                {
                    if (null == SQLWhere[i])
                        ReflectionTools.RemoveItemInArray(this, "SQLWhere", i);
                }
            }
        }

        /// <summary>
        /// Initiale SQLJoin
        /// </summary>
        private void InitializeSQLJoin(string[] pParam)
        {
            if (ArrFunc.IsFilled(SQLJoin))
            {
                for (int index = 0; index < SQLJoin.Length; index++)
                {
                    SQLJoin[index] = ReplaceDynamicData(SQLJoin[index], pParam);
                }
            }
        }

        /// <summary>
        /// Retourne true s'il existe au minimum un SQLWhere tel que ConditionSystem = "SESSIONRESTRICT"
        /// </summary>
        /// <returns></returns>
        public bool IsSQLWhereWithSessionRestrict()
        {
            bool ret = false;
            if (ArrFunc.IsFilled(SQLWhere))
            {
                for (int i = 0; i < SQLWhere.Length; i++)
                {
                    if ((null != SQLWhere[i]) &&
                        SQLWhere[i].ConditionSystemSpecified &&
                        SQLWhere[i].ConditionSystem == Cst.SESSIONRESTRICT)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Remplace les paramètres PARAM1,PARAM2, etc... par leur valeur
        /// <para>Remplace les mots clefs session par leur valeur (voir SessionTools.ReplaceDynamicConstantsWithValues) </para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pParam"></param>
        /// <returns></returns>
        private static string ReplaceDynamicData(string pData, string[] pParam)
        {
            string ret = pData;
            //
            if (ArrFunc.IsFilled(pParam) && StrFunc.IsFilled(ret))
            {
                for (int i = 0; i < pParam.Length; i++)
                {
                    string param = Cst.PARAM_START + "$" + Cst.PARAM_END;
                    param = param.Replace("$", (i + 1).ToString());
                    ret = ret.Replace(param, pParam[i]);
                }
            }
            //
            ret = SessionTools.ReplaceDynamicConstantsWithValues(ret);
            //
            return ret;
        }

        /// <summary>
        /// Affecte les permission en fonction du menu
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// FI 20090416 création
        /// FI 20120116 Un admin n'a pas nécessairement les droits Create,Modify,Delete etc.. (c'est le cas lorsque la Database est de type READONLY
        public void SetPermission(string pIdMenu)
        {
            if (StrFunc.IsFilled(pIdMenu))
            {
                if ((pIdMenu == "ATTACHEDDOC") || (pIdMenu == "ATTACHEDDOCS"))
                {
                    //Note: 
                    //Il n'existe pas pour l'instant de gestion des permissions sur l'accès aux pièces jointes
                    //Cela peut être gênant, car un user qui n'aurait pas le droit de modifié/supprimé un item 
                    //pourra malgré tout ajouter/supprimer une pièce jointe (cf. TRIM n°3230)
                }
                else
                {
                    //Note: 
                    //Les permissions "FALSE" définies dans le fichier XML sont prioritaires à celles définis via les habilitations.
                    //ici this.Create,etc... est déjà valoriser avec les permissions spécfiées sur le fichier XML oi les lst
                    //
                    RestrictionPermission restrictPermission =
                        new RestrictionPermission(pIdMenu, SessionTools.User);
                    restrictPermission.Initialize(CSTools.SetCacheOn(SessionTools.CS));
                    //
                    if (!restrictPermission.isCreate)
                        this.Create = false;
                    if (!restrictPermission.isModify)
                        this.Modify = false;
                    if (!restrictPermission.isRemove)
                        this.Remove = false;
                    if (!restrictPermission.isImport)
                        this.Import = false;
                    if ((!restrictPermission.isEnabledDisabled) && ExistsColumnsDateValidity)
                    {
                        Column[IndexDTENABLED].IsUpdatable.Value = false;
                        Column[IndexDTDISABLED].IsUpdatable.Value = false;
                    }
                    restrictPermission = null;
                }
            }
        }

        /// <summary>
        /// Envoye des message Queue vers les services lorsque la modification du référentiel doit produire un traitement 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdMenu"></param>
        /// RD 20150324 [20893] Create
        /// FI 20150325 [20893] Methode est maintenant obsolete puisqu'elle n'est pus appelée 
        [Obsolete("Obsolete puisque plus appelée", true)]
        public void SendMQueueAfterChange(IDbTransaction pDbTransaction, string pIdMenu)
        {
            SendMQueueAfterChange(pDbTransaction, dataSet, pIdMenu);
        }

        /// <summary>
        /// Envoye des message Queue vers les services lorsque la modification du référentiel doit produire un traitement 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataSet"></param>
        /// <param name="pIdMenu"></param>
        /// RD 20150324 [20893] Add pDataSet
        /// FI 20150325 [20893] Methode est maintenant obsolete puisqu'elle n'est pus appelée
        [Obsolete("Obsolete puisque plus appelée", true)]
        public void SendMQueueAfterChange(IDbTransaction pDbTransaction, DataSet pDataSet, string pIdMenu)
        {
            //Send Eventuel de message (dépend du référentiel)
            MQueueDataset mQueueDatasetWriter = new MQueueDataset(SessionTools.CS, TableName);
            // RD 20150324 [20893] Use pDataSet
            mQueueDatasetWriter.Prepare(pDataSet.Tables[0]);

            TrackerRequest trackerRequest = new TrackerRequest(OTCmlHelper.GetDateSys(SessionTools.CS), SessionTools.NewAppInstance());
            MQueueRequester qRequester = new MQueueRequester(trackerRequest);

            MQueueSendInfo mqSendInfo = new MQueueSendInfo();
            mqSendInfo.LoadCurrentAppSettings();
            mQueueDatasetWriter.Send(pDbTransaction, qRequester, SessionTools.NewAppInstance(), mqSendInfo, pIdMenu);
        }

        /// <summary>
        ///  Retourne un compteur avec le nombre d'item dans {pDynamicArgCondition} qui matchent avec les dynamicArgs
        /// </summary>
        /// <param name="pDynamicArgCondition"></param>
        /// <returns></returns>
        private Int32 DynamicArgMatchNumber(ConditionDynamicArg pDynamicArgCondition)
        {
            if (null == pDynamicArgCondition)
                throw new ArgumentNullException("pDynamicArgCondition is null");

            Int32 ret = 0;
            if (pDynamicArgCondition.dynamicArgValueSpecified)
            {
                DynamicArgValue[] dynamicArgValue = pDynamicArgCondition.dynamicArgValue;
                if (ArrFunc.IsFilled(dynamicArgValue))
                {
                    foreach (string key in dynamicArgs.Keys)
                    {
                        int mathResult = dynamicArgValue.Where(item => item.name == key & item.value == dynamicArgs[key].value).Count();
                        ret += mathResult;
                    }
                }
            }
            return ret;
        }



        /// <summary>
        /// Retourne les scripts SQL à exécuter 
        /// <para>pSQL contient une liste de script, seuls certains sont à exécuter.Cela en fonction du contexte (Oracle,SQLServer, sessionRestrict, conditionsystem, etc)</para>
        /// </summary>
        /// <param name="pSQL">Liste des scripts SQL</param>
        /// <param name="pParam"></param>
        /// <param name="pCondApp"></param>
        /// <returns></returns>
        /// FI 20141211 [20563] Refactoring complet, Add parameter pCondApp
        /// FI 20150107 [XXXXX] Modify 
        private string[] GetSqlCommand(ReferentialSQLSelect[] pSQL, string[] pParam, string pCondApp)
        {
            //FI 20150107 
            if (StrFunc.IsFilled(pCondApp) && pCondApp == "*")
                throw new NotSupportedException(StrFunc.AppendFormat("CondApp:{0} is not supported. Value:{0} is a keyword", pCondApp));

            string[] ret = null;
            if (ArrFunc.IsEmpty(pSQL))
                return ret;

            List<ReferentialSQLSelect> lstSQLSelect = pSQL.Cast<ReferentialSQLSelect>().ToList();
            //FI 20150107 Spheres® conserve les select où ConditionApplication="*"
            //Suppression des requêtes qui ne s'appliquent pas vis-à-vis de {pCondApp} 
            if (StrFunc.IsFilled(pCondApp))
                lstSQLSelect.RemoveAll(item => (item.ConditionApplicationSpecified && item.ConditionApplication != "*"
                                                                                   && item.ConditionApplication != pCondApp) ||
                                                    (false == item.ConditionApplicationSpecified));
            else
                lstSQLSelect.RemoveAll(item => (item.ConditionApplicationSpecified && item.ConditionApplication != "*"));

            //Attention La condition SESSIONRESTRICT est à utilisé uniquement avec l'élément SQLSelect 
            //Avec l'élément SQLSelect, par convention, il est prévu que pSQL contient uniquement 2 éléments 
            //- le 1er avec ConditionSystem == SESSIONRESTRICT 
            //- le 2nd sans ConditionSystem
            //Avec l'élément SQLSelect Spheres® ne retient que ler élement de la valeur retour de cette fonction
            if (SessionTools.IsSessionSysAdmin)
                lstSQLSelect.RemoveAll(item => item.ConditionSystemSpecified && item.ConditionSystem == Cst.SESSIONRESTRICT);

            if (TableName.ToUpper() == Cst.OTCml_TBL.PROCESS_L.ToString())
            {
                if (LogTools.GetLogConsultMode() == LogConsultMode.Admin)
                    lstSQLSelect.RemoveAll(item => item.ConditionSystemSpecified && item.ConditionSystem == Cst.SESSIONRESTRICT);
            }

            if (ArrFunc.IsFilled(dynamicArgs))
            {
                //Contrôle => Les conditions sur dynamicArg possèdent obligatoirement les valeurs acceptées et/ou une étiquette
                //Si ce n'est pas le cas Spheres® génère une erreur
                //Les éléments étiquettées uniquement permettent de définir une requête par défaut lorsque dynamicArg ne matchent pas avec les autres éléments de même étiquettage
                var control = lstSQLSelect.Where(item => (item.ConditionDynamicArgSpecified &&
                                                         (item.ConditionDynamicArg.dynamicArgValueSpecified == false) &&
                                                          StrFunc.IsEmpty(item.ConditionDynamicArg.grp))).ToList();
                if (control.Count() > 0)
                    throw new Exception("ConditionDynamicArg is not valid. ConditionDynamicArg must contains dynamicArgValue element or grp attribut");


                //lstSQLSelect contient :
                // - les éléments sans condition sur les dynamicsArgs (élément ConditionDynamicArg absent) ou 
                // - les éléments avec condition sur les dynamicsArgs pour lesquels il n'existe aucun paramétrage des valeurs acceptées mais où il existe nécessairement une étiquette
                // - les éléments avec condition sur les dynamicsArgs où les valeurs acceptées matchent avec les valeurs des dynamicArgs
                lstSQLSelect = lstSQLSelect.Where(item => (item.ConditionDynamicArgSpecified == false) ||
                                                           ((item.ConditionDynamicArg.dynamicArgValueSpecified == false) && StrFunc.IsFilled(item.ConditionDynamicArg.grp)) ||
                                                            DynamicArgMatchNumber(item.ConditionDynamicArg) > 0).ToList();


                List<ReferentialSQLSelect> lstSQLSelectTmp = new List<ReferentialSQLSelect>();
                List<string> lstGrp = new List<string>();
                foreach (ReferentialSQLSelect item in lstSQLSelect)
                {
                    if (item.ConditionDynamicArgSpecified && StrFunc.IsFilled(item.ConditionDynamicArg.grp))
                    {
                        //Parmi les éléments avec condition, Sphere® récupère celle qui matche le plus avec les valeurs des dynamicArgs

                        string grp = item.ConditionDynamicArg.grp;
                        if (false == lstGrp.Contains(grp))
                        {
                            ReferentialSQLSelect elementMaxMatch =
                                lstSQLSelect.Where(Element => Element.ConditionDynamicArgSpecified
                                    && StrFunc.IsFilled(Element.ConditionDynamicArg.grp) && Element.ConditionDynamicArg.grp == grp)
                                    .OrderByDescending(arg => DynamicArgMatchNumber(arg.ConditionDynamicArg)).FirstOrDefault();
                            if (null == elementMaxMatch)
                                throw new InvalidProgramException("elementMaxMatch is null");

                            lstSQLSelectTmp.Add(elementMaxMatch);

                            lstGrp.Add(grp);
                        }
                    }
                    else
                    {
                        lstSQLSelectTmp.Add(item);
                    }
                }
                lstSQLSelect = lstSQLSelectTmp;
            }


            string rdbms = string.Empty;
            DbSvrType dbSvrType = DataHelper.GetDbSvrType(SessionTools.CS);
            switch (dbSvrType)
            {
                case DbSvrType.dbORA:
                    rdbms = "oracle";
                    break;
                case DbSvrType.dbSQL:
                    rdbms = "sqlserver";
                    break;
            }

            List<string> lstSqlCommand =
               (from ReferentialSQLSelect item in lstSQLSelect
                from ReferentialSQLSelectCommand command in item.Command
                where (command.rdbms == rdbms || command.rdbms == "all")
                select command.Value).ToList();

            if (ArrFunc.IsFilled(lstSqlCommand))
            {
                for (int i = 0; i < ArrFunc.Count(lstSqlCommand); i++)
                {
                    if (StrFunc.ContainsIn(lstSqlCommand[i], "%%SELECT_LOGSESSION_PROCESS_L%%"))
                    {
                        QueryParameters queryLog = LogTools.GetQueryForLoadLogBySession(SessionTools.CS, SessionTools.User, SessionTools.SessionID, Cst.OTCml_TBL.PROCESS_L.ToString());
                        if (false == IsUseSQLParameters)
                            lstSqlCommand[i] = lstSqlCommand[i].Replace("%%SELECT_LOGSESSION_PROCESS_L%%", queryLog.GetQueryReplaceParameters(false));
                        else
                            lstSqlCommand[i] = lstSqlCommand[i].Replace("%%SELECT_LOGSESSION_PROCESS_L%%", queryLog.query);
                    }

                    lstSqlCommand[i] = ReplaceDynamicData(lstSqlCommand[i], pParam);

                    SessionRestrictHelper srH = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, IsUseSQLParameters);
                    lstSqlCommand[i] = srH.ReplaceKeyword(lstSqlCommand[i]);

                    // FI 20120803 call of OTCmlHelper.ReplaceKeyword
                    lstSqlCommand[i] = OTCmlHelper.ReplaceKeyword(SessionTools.CS, lstSqlCommand[i]);

                    // FI 20150907 [21312] Appel de StrFuncExtended.ReplaceChooseExpression2 (Fonctionnalité requête dynamique en fonction des Dynamic Arg)
                    if (this.dynamicArgsSpecified && lstSqlCommand[i].Contains(@"<choose>"))
                    {
                        TypeBuilder dynamicArgsType = new TypeBuilder(SessionTools.CS, (from item in dynamicArgs
                                                                                        select item.Value).ToList(), "DynamicData", "Referential");
                        // EG 20161122 GetNewObject
                        lstSqlCommand[i] = StrFuncExtended.ReplaceChooseExpression2(lstSqlCommand[i], dynamicArgsType.GetNewObject(), true);
                    }
                }
                ret = lstSqlCommand.ToArray();
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans {pData} les DynamicArguments par leur valeurs ou expressions
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// EG 201306026 Appel à la méthode de Désérialisation d'un StringDynamicData en chaine
        public string ReplaceDynamicArgument(string pCS, string pData)
        {
            string ret = pData;
            if (ArrFunc.IsFilled(xmlDynamicArgs))
            {
                for (int index = 0; index < xmlDynamicArgs.Length; index++)
                {
                    string da = xmlDynamicArgs[index];
                    if (StrFunc.IsFilled(da))
                    {
                        // EG 20130626
                        //EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), da);
                        //StringDynamicData sDa = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
                        StringDynamicData sDa = RepositoryTools.DeserializeDA(da);
                        ret = sDa.ReplaceInString(pCS, ret);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les scripts qu'il faut exécuter avant le chargement du datagrid
        /// <para>Cette méthode interprète les Dynamics Argument</para>
        /// <para>Retourne null s'il n'existe pas de PreSelect commandes</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        public QueryParameters[] GetSqlPreSelectCommand(string pCs)
        {
            QueryParameters[] ret = null;
            List<QueryParameters> queryParameters = new List<QueryParameters>();
            //
            string[] query = this.SQLPreSelectCommand;
            if (ArrFunc.IsFilled(query))
            {
                for (int i = 0; i < ArrFunc.Count(query); i++)
                {
                    query[i] = ReplaceDynamicArgument(pCs, query[i]);
                    queryParameters.Add(ConvertQueryToQueryParameters(pCs, query[i]));
                }
            }
            //
            if (ArrFunc.IsFilled(queryParameters))
                ret = queryParameters.ToArray();
            //            
            return ret;
        }

        /// <summary>
        /// Retourne la commande sql qui charge le datagrid lorsque SQLSelect est renseigné
        /// <para>Cette méthode interprète les Dynamics Argument</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        public QueryParameters GetSqlSelectCommand(string pCs)
        {
            string query = this.SQLSelectCommand;
            query = ReplaceDynamicArgument(pCs, query);
            QueryParameters ret = ConvertQueryToQueryParameters(pCs, query);
            return ret;
        }

        /// <summary>
        /// Ajoute une colonne 
        /// </summary>
        /// <param name="pRC"></param>
        public void AddColumn(ReferentialColumn pRC)
        {
            List<ReferentialColumn> al = new List<ReferentialColumn>();
            if (ArrFunc.IsFilled(this.Column))
                al.AddRange(this.Column);
            al.Add(pRC);
            this.Column = al.ToArray();
        }

        /// <summary>
        /// Retourne une requête paramétrée à partir de la requête pQuery
        /// <para>Seuls les dynamicDatas donnent lieu à des Parameters</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        public QueryParameters ConvertQueryToQueryParameters(string pCS, string pQuery)
        {
            string query = pQuery;
            DataParameters parameters = new DataParameters();
            //
            if (IsUseSQLParameters & StrFunc.IsFilled(query))
            {
                if (ArrFunc.IsFilled(this.dynamicArgs))
                {
                    //Spheres® trie la liste du plus long au plus petit
                    List<string> lst = new List<string>(dynamicArgs.Keys);
                    lst.Sort(StrFunc.CompareLength);
                    lst.Reverse();
                    //
                    foreach (string key in lst.ToArray())
                    {
                        if (query.Contains("@" + key))
                        {
                            if (false == parameters.Contains(key))
                                parameters.Add(dynamicArgs[key].GetDataParameter(pCS, null, CommandType.Text, 0, ParameterDirection.Input));
                            query = query.Replace("@" + key, Cst.DA_START + "@" + key + Cst.DA_END);
                        }
                    }
                    //
                    foreach (string key in lst.ToArray())
                    {
                        query = query.Replace(Cst.DA_START + "@" + key + Cst.DA_END, "@" + key);
                    }
                }
                //
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                srh.SetParameter(pCS, query, parameters);
            }
            //
            QueryParameters ret = new QueryParameters(pCS, query, parameters);
            //
            return ret;
        }

        /// <summary>
        /// Retourne l'enregistrement présent ds le jeu de résultat dont la PK vaut {pValueDataKeyField}
        /// <para>Retourne null si l'enregistrement n'existe pas</para>
        /// </summary>
        /// <param name="pValueDataKeyField"></param>
        /// <returns></returns>
        public DataRow GetRow(int pTableIndex, string pDataKeyField, string pValueDataKeyField)
        {
            DataRow ret = null;

            if (IndexDataKeyField > -1)
            {
                if (String.IsNullOrEmpty(pDataKeyField))
                {
                    //La colonne clé est celle du référentiel courant
                    pDataKeyField = Column[IndexDataKeyField].DataField;
                }

                string select = pDataKeyField + "=";

                if (TypeData.IsTypeString(Column[IndexDataKeyField].DataType.value))
                {
                    select += "'" + pValueDataKeyField + "'";
                }
                else
                {
                    select += pValueDataKeyField;
                }

                DataRow[] rows = dataSet.Tables[pTableIndex].Select(select);
                if (ArrFunc.IsFilled(rows))
                {
                    ret = rows[0];
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne un array où chaque item contient un StringDynamicData serializé
        /// </summary>
        /// <param name="pDynamicDatas"></param>
        /// <returns></returns>
        /// EG 201306026 Appel à la méthode de Sérialisation d'une chaine en StringDynamicData 
        private static string[] DynamicDatasToString(Dictionary<string, StringDynamicData> pDynamicDatas)
        {

            string[] ret = null;
            if ((null != pDynamicDatas) && (ArrFunc.Count(pDynamicDatas) > 0))
            {
                string dA = string.Empty;
                IEnumerator listDA = pDynamicDatas.Values.GetEnumerator();
                while (listDA.MoveNext())
                {
                    if (StrFunc.IsFilled(dA))
                        dA += StrFunc.StringArrayList.LIST_SEPARATOR;

                    //dA += ((StringDynamicData)listDA.Current).Serialize();
                    StringDynamicData _dd = listDA.Current as StringDynamicData;
                    dA += RepositoryTools.SerializeDA(_dd);
                }
                ret = StrFunc.StringArrayList.StringListToStringArray(dA);
            }
            return ret;
        }

        /// <summary>
        ///  Valorise les membres dynamicArgs et xmlDynamicArgs
        /// </summary>
        /// <param name="pDynamicDatas"></param>
        /// FI 20141211 [20563] Rename de la méthode
        public void SetDynamicArgs(Dictionary<string, StringDynamicData> pDynamicDatas)
        {
            dynamicArgs = pDynamicDatas;
            dynamicArgsSpecified = ArrFunc.IsFilled(dynamicArgs);
            xmlDynamicArgs = DynamicDatasToString(pDynamicDatas);
        }

        /// <summary>
        /// Copie this dataRow
        /// </summary>
        /// <returns></returns>
        /// FI 20141021 [20350] Add Method
        public DataRow CoptyDataRow()
        {
            if (null == dataRow)
                throw new NullReferenceException("dataRow is null");

            DataRow ret = null;
            DataTable tblSav = dataSet.Tables[0].Clone();
            tblSav.ImportRow(dataRow);
            ret = tblSav.Rows[0];

            return ret;
        }

        #endregion
    }

    #region ReferentialBooleanEltAndTablenameAttrib
    public class ReferentialBooleanEltAndTablenameAttrib
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool Value;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tablename;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tablenameSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDSpecified;
    }
    #endregion class ReferentialsReferentialBooleanEltAndTablenameAttrib

    /// <summary>
    /// Représente une ressource qui définie un SQLSelect
    /// </summary>
    public class ReferentialSQLSelectResource
    {
        /// <summary>
        /// Nom de la ressource
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string name;
    }

    /// <summary>
    /// Représente un array de commande SQL de type SQLSelect
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", ElementName = "sqlCommands", IsNullable = false)]
    public class ReferentialSQLSelects
    {
        /// <summary>
        /// Représente une liste de requête 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sqlCommand", IsNullable = false, Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelect[] item;
    }

    /// <summary>
    /// Représente un array de commande SQL de type ReferentialsReferentialSQLSelect ou une resource incorposée qui permet d'accéder à un array de commande SQL de type ReferentialsReferentialSQLSelect
    /// </summary>
    /// FI 20121219 Cadeau de noel pour Spheres®
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", ElementName = "SQLSelectOrResource", IsNullable = false)]
    public class ReferentialSQLSelectOrResource
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLSelectSpecified;
        /// <summary>
        /// Représente la commande Select exécutée pour charger les données ds le grid
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SQLSelect", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelect[] SQLSelect;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLSelectResourceSpecified;
        /// <summary>
        /// Définie la resource qui permet d'obtenir SQLSelect 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SQLSelectResource", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelectResource SQLSelectResource;

    }

    /// <summary>
    /// Représente une condition sur les dynamics Arguments
    /// </summary>
    /// FI 20141211 [20563] Add class
    public class ConditionDynamicArg
    {
        /// <summary>
        ///  Etiquetage de la condition
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string grp;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dynamicArgValueSpecified;
        /// <summary>
        /// Représente la liste des valeurs acceptées
        /// <para>Si non renseigné, toutes les valeurs sont acceptées</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DynamicArgValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DynamicArgValue[] dynamicArgValue;

    }

    /// <summary>
    /// Représente la valeur d'un Dynamic Argument
    /// <para>Rappel Les dynamic Argument sont générés à partir des contôles GUI ou à partir de l'URL (&DA=)</para>
    /// </summary>
    /// FI 20141211 [20563] Add class
    public class DynamicArgValue
    {
        /// <summary>
        /// Représente l'identifiant du Dynamic Argument
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;

        /// <summary>
        /// Représente la valeur du Dynamic Argument
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string @value;

    }



    /// <summary>
    /// Représente un array de commande SQL
    /// <para>chaque Commande peut être spécifique à une condition d'application</para>
    /// <para>chaque Commande peut être spécifique à une condition système</para>
    /// <para>chaque Commande est spécifique à une type de SGBD système</para>
    /// </summary>
    public class ReferentialSQLSelect
    {



        /// <summary>
        ///  Lorsque renseigné, signifie que les requêtes SQL s'appliquent uniquement pour la condition spécifiée
        ///  <para>S'il n'existe pas de condition ou si la condition est différente de {ConditionApplication} Spheres® ignorent les requêtes ici déclérées</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ConditionApplication;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConditionApplicationSpecified;

        /// <summary>
        ///  FI (Usage très particulier .....)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ConditionSystem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConditionSystemSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ConditionDynamicArg ConditionDynamicArg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConditionDynamicArgSpecified;

        /// <summary>
        /// Représente n requêtes  
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialSQLSelectCommand[] Command;
    }

    /// <summary>
    /// Représente une commande SQL 
    /// </summary>
    public class ReferentialSQLSelectCommand
    {
        /// <summary>
        /// type de SGBD associée à la commande SQL
        /// <para>valeurs possibles: all, oracle, sqlserver</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rdbms;
        /// <summary>
        /// Commande SQL
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }

    public class ReferentialButton
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tooltip;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tooltipSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool colorSpecified;
    }

    public class ReferentialItemTableName
    {
        /// <summary>
        /// TableName
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;

        /// <summary>
        /// ColumnName
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string itemcolumnname;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool itemcolumnnameSpecified;

        /// <summary>
        /// Number of columns displayed by row 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int columnbyrow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool columnbyrowSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRoleWithIDA
        {
            get { return (IsGInstrRole || IsActorRole || IsGContractRole); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsGInstrRole
        {
            get { return (Value == "GINSTRROLE"); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsActorRole
        {
            get { return (Value == "ACTORROLE"); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsGContractRole
        {
            get { return (Value == "GCONTRACTROLE"); }
        }

    }
    public class ReferentialItems
    {
        /// <summary>
        /// Source table name
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string srctablename;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool srctablenameSpecified;

        /// <summary>
        /// Target table name
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tablename;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tablenameSpecified;

        /// <summary>
        /// Target column name
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string columnname;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool columnnameSpecified;

        /// <summary>
        /// Source table name
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string datatype;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datatypeSpecified;

        /// <summary>
        /// Additional columns name not null or necessary to unique key
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string addcolumns;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool addcolumnsSpecified;

        /// <summary>
        /// Mode
        /// <para>RW: Read Write - Affichage des informations en lecture/écriture</para>
        /// <para>RO: Read Only - Affichage des informations en lecture seule</para>
        /// <para>SO: Selected Only - Affichage uniquement des informations sélectionnées et en lecture seule</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool modeSpecified;

        [System.Xml.Serialization.XmlElementAttribute("html_BLOCK", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnhtml_BLOCK html_BLOCK;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool html_BLOCKSpecified;
    }

    public class ReferentialSQLOrderBy
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ColumnName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Alias;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string DataType;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnNotInReferential;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GroupBySpecified;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ReferentialColumnGroupBy GroupBy;
        //
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnNameOrColumnSQLSelect;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnNameOrColumnSQLSelectSpecified;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string GroupBySort;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string GroupBySortWithAlias;
        //
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ValueWithAlias;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ReferentialSQLRowStyle
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string type;

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ReferentialSQLRowState
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string headerText;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ReferentialSQLWhere
    {
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ConditionApplication;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConditionApplicationSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ConditionSystem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConditionSystemSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AliasTableName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AliasTableNameSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnNameSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LstValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LstValueSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnDataType DataType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataTypeSpecified;

        

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Operator;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OperatorSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SQLWhere;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLWhereSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SQLJoin", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string[] SQLJoin;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLJoinSpecified;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnSQLWhereSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnSQLWhere;
        //
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnNameOrColumnSQLSelect;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnNameOrColumnSQLSelectSpecified;
    }

    /// <summary>
    /// 
    /// </summary>
    /// // EG 20110104 Add controlEdit/controlEditSpecified
    [System.Xml.Serialization.XmlRootAttribute("Column", Namespace = "", IsNullable = false)]
    public class ReferentialColumn
    {
        public ReferentialColumn()
        {
            this.IsOrderBy = new ReferentialColumnIsOrderBy();
            this.IsIdentity = new ReferentialColumnIsIdentity();
        }

        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool colorSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int ColumnPositionInDataGrid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnPositionInDataGridSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnNameOrColumnSQLSelect;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnNameOrColumnSQLSelectSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string DataField;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int IndexColSQL;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AliasTableName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AliasTableNameSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AliasColumnName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AliasColumnNameSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Ressource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RessourceSpecified;//PL 20120109 New

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnDataType DataType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataTypeSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsDataXml;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TextMode;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Coding;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Align;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AlignSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool LabelNoWrap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LabelNoWrapSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LabelWidth;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string InputWidth;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string InputHeight;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int GridWidth;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GridWidthSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GroupBy", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnGroupBy GroupBy;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GroupBySpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Colspan;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColspanSpecified;

        /// <summary>
        /// Obtient ou définit la taille de la donnée dans le Datagrid
        /// <para>Ne s'applique que au champs text et string</para>
        /// <para>si -1 la colonne est présente dans le select du datagrid mais elle est non chargée => la query fait un select 'text' as COLUMNNAME, cela permet d'améliorer sensiblement les perfs</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Length;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LengthSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int LengthInDataGrid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LengthInDataGridSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Nowrap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NowrapSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Scale;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ScaleSpecified;

        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnSqlWhereSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SQLWhere", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnSqlWhere;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsMandatory;
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsMandatorySpecified;

        /// <remarks/>
        // EG 20110104
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsEditableSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsEditable;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RegularExpression;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Default", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnDefault[] Default;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnResource IsResource;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsResourceSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsRTF;
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRTFSpecified;
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsTRIM;
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsTRIMSpecified;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RowStyleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RowStyle", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnStyleBase RowStyle;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CellStyleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CellStyle", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnStyleBase CellStyle;

        // RD 20110704 [17501] / Suppression des deux colonnes LSTCOLUMN.ISSIDE et LSTCOLUMN.ISQUANTITY

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsHide;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHideSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsHideInDataGrid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHideInDataGridSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsHideInCriteria;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHideInCriteriaSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsHideOnLightDisplay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHideOnLightDisplaySpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsKeyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsKeyFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsDataKeyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsDataKeyFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsForeignKeyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsForeignKeyFieldSpecified;

        [System.Xml.Serialization.XmlElementAttribute("IsIdentity", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnIsIdentity IsIdentity;
        //public bool IsIdentity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsIdentitySpecified;

        [System.Xml.Serialization.XmlElementAttribute("IsUpdatable", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnIsUpdatable IsUpdatable;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsUpdatableSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsAutoPostBack;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAutoPostBackSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsVirtualColumn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsVirtualColumnSpecified;

        [System.Xml.Serialization.XmlElementAttribute("IsOrderBy", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnIsOrderBy IsOrderBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsOrderBySpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnIsHyperLink IsHyperLink;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHyperLinkSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ToolTip;
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ToolTipSpecified;

        [System.Xml.Serialization.XmlElementAttribute("Information", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialInformation Information;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InformationSpecified;

        [System.Xml.Serialization.XmlElementAttribute("Misc", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Misc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MiscSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Help;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ListRetrievalType;
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ListRetrievalData;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsExternal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsRole;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsItem;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExternalTableName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExternalIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int ExternalFieldID;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ColumnRef;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Relation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelation[] Relation;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("html_BLOCK", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnhtml_BLOCK[] html_BLOCK;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("html_HR", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnhtml_HR[] html_HR;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("html_TITLE", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnhtml_TITLE[] html_TITLE;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("JavaScript", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnJavaScript JavaScript;

        //Gestion des controles        

        //Label
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private System.Web.UI.WebControls.WebControl controlLabel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Web.UI.WebControls.WebControl ControlLabel
        {
            get
            {
                return controlLabel;
            }
            set
            {
                controlLabel = value;
                ControlLabelSpecified = true;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ControlLabelSpecified;// = false;

        //Controle principal (html ou web)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private System.Web.UI.WebControls.WebControl controlMain;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Web.UI.WebControls.WebControl ControlMain
        {
            get
            {
                return controlMain;
            }
            set
            {
                controlMain = value;
                ControlMainSpecified = true;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ControlMainSpecified; // = false;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private System.Web.UI.HtmlControls.HtmlControl htmlControlMain;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Web.UI.HtmlControls.HtmlControl HtmlControlMain
        {
            get
            {
                return htmlControlMain;
            }
            set
            {
                htmlControlMain = value;
                HtmlControlMainSpecified = true;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HtmlControlMainSpecified; // = false;

        //Autres controles associés (ex: calendar pour un champ date (le seul a l'h actuelle: 06/07/2004))
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Web.UI.WebControls.WebControl[] OtherControls;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Web.UI.WebControls.WebControl[] OtherGridControls;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public System.Web.UI.WebControls.WebControl[] InformationControls;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HasInformationControls;

        //Données diverses
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Label
        {
            get
            {
                string ret = string.Empty;
                if (this.ExistsRelation)
                    ret = this.Relation[0].ColumnSelect[0].Ressource;
                else
                    ret = this.Ressource;

                ret = EFS.ACommon.Ressource.GetMultiEmpty(ret);
                return ret;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string MsgErrRequiredField;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string MsgErr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsFirstControlLinked;// = false;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FirstControlLinkedColumnName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsMiddleControlLinked;// = false;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsLastControlLinked;// = false;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string IDForItemTemplate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string IDForItemTemplateRelation;




        #region DataTypeEnum
        public TypeData.TypeDataEnum DataTypeEnum
        {
            get
            {
                return TypeData.GetTypeDataEnum(DataType.value, true);
            }
        }
        #endregion DataTypeEnum

        [System.Xml.Serialization.XmlElementAttribute("TypeAhead", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialTypeAhead TypeAhead;

        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsDDLType
        {
            get { return ((this.Relation != null) && (this.Relation[0].DDLType != null)) || (this.ListRetrievalData != null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsRelation
        {
            get { return (this.Relation != null) && (this.Relation[0].TableName != null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsRelationDDLType
        {
            get { return (this.Relation != null) && (this.Relation[0].DDLType != null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsDefault
        {
            get { return (this.Default != null) && ((this.Default[0].Value != null) || (this.Default[0].ColumnName != null)); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsDefaultValue
        {
            get { return (this.Default != null) && (this.Default[0].Value != null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsDefaultColumnName
        {
            get { return (this.Default != null) && (this.Default[0].ColumnName != null); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsJavaScript
        {
            get { return (this.JavaScript != null) && (this.JavaScript.Script.Length > 0); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsHelp
        {
            get { return (this.Help != null); }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsHyperLinkDocument
        {
            get
            {
                return (this.IsHyperLinkSpecified) &&
                    (this.IsHyperLink.linktypeSpecified) &&
                    (this.IsHyperLink.linktype == "document");
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsHyperLinkExternal
        {
            get
            {
                return (this.IsHyperLinkSpecified) &&
                    (this.IsHyperLink.linktypeSpecified) &&
                    (this.IsHyperLink.linktype == "external");
            }
        }

        /// <summary>
        /// Obtient true si le link est spécifié via un nom de colonne
        /// <para>IsHyperLink.type contient la nom de colonne qui permet d'ouvrir un référentiel (Ex IDA, IDDC, IDM)</para>
        /// <para>IsHyperLink.name contient la colonne qui détient la valeur id associé</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExistsHyperLinkColumn
        {
            get
            {
                return (this.IsHyperLinkSpecified) &&
                    (this.IsHyperLink.linktypeSpecified) &&
                    (this.IsHyperLink.linktype == "column");
            }
        }






        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsExternal2
        {
            get
            {
                return (("EXTLLINK" == this.ColumnName) || ("EXTLLINK2" == this.ColumnName) || this.IsExternal);
            }
        }
        /// <summary>
        /// Obtient true si IsExternal ou IsRole ou IsItem
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsAdditionalData
        {
            get
            {
                return (this.IsExternal || this.IsRole || this.IsItem);
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsNotVirtualColumn
        {
            get
            {
                return (false == IsVirtualColumnSpecified) || (false == IsVirtualColumn);
            }
        }

        #region AutoComplete

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AutoCompleteEnabled
        {
            get
            {
                return
                    // relation enabled
                    (this.Relation != null)
                    && (this.Relation[0].TableName != null)
                    // autocomplete enabled
                    && (this.Relation[0].AutoComplete != null)
                    && (this.Relation[0].AutoComplete.Enabled);
            }
        }

        #endregion AutoComplete

        #region TypeAheadEnabled
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeAheadEnabled
        {
            get
            {
                return (null != this.TypeAhead) && StrFunc.IsFilled(this.TypeAhead.Table) && this.TypeAhead.Enabled;
            }
        }

        #endregion TypeAheadEnabled

        #region BSColumnName
        public string BSColumnName
        {
            get
            {
                return (this.IsAdditionalData ? this.ColumnName + this.ExternalFieldID: this.ColumnName);
            }
        }
        #endregion BSColumnName

        #region RegexValue
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFSRegex.TypeRegex RegexValue
        {
            get
            {
                EFSRegex.TypeRegex typeRegex = EFSRegex.TypeRegex.None;
                if (System.Enum.IsDefined(typeof(EFSRegex.TypeRegex), this.RegularExpression))
                    typeRegex = (EFSRegex.TypeRegex)System.Enum.Parse(typeof(EFSRegex.TypeRegex), this.RegularExpression);
                return typeRegex;
            }
        }
        #endregion RegexValue

        #endregion Accessors
        #region Methods
        #region IsAliasEqualToMasterAliasTableName
        public bool IsAliasEqualToMasterAliasTableName(string pAliasMasterTableName)
        {
            return (AliasTableNameSpecified && (AliasTableName == pAliasMasterTableName));
        }
        #endregion IsAliasEqualToMasterAliasTableName
        #region GetStringDefaultValue
        public string GetStringDefaultValue(string pTableName)
        {
            string retValue = string.Empty;
            if (ExistsDefaultValue)
            {
                if (this.Default[0].Value.ToLower() == "{Previous}".ToLower())
                {
                    retValue = String.Empty;
                }
                else if (TypeData.IsTypeDateOrDateTime(DataType.value))
                {
                    retValue = new DtFunc().GetDateString(Default[0].Value);
                }

                else
                {
                    retValue = this.Default[0].Value;
                }
            }
            //CC 20110210 Ticket 17315
            else if (this.ColumnName == "ISENABLED" || this.ColumnName == "ISSECURITYSTATUS")
            {
                //20081013 PL Gestion dynamique via le web.config des defaults sur les colonnes ISENABLED
                retValue = ConfigurationManager.AppSettings["Spheres_ReferentialDefaultEnabled_" + pTableName];
            }
            //20081204 PL Add if()
            if (StrFunc.IsFilled(retValue) && (TypeData.IsTypeBool(this.DataType.value)))
                retValue = OTCmlHelper.GetADONetBoolValue(SessionTools.CS, retValue);
            //
            return retValue;
        }
        #endregion GetStringDefaultValue
        #region GetStringDefaultValue2
        /// <summary>
        /// EG 20121029 Fonction (de merde) appelée à l'initialisation d'un formulaire en modification 
        /// dans le mode consultationMode: PartialReadOnly
        /// </summary>
        /// <param name="pTableName"></param>
        /// <returns></returns>

        public string GetStringDefaultValue2(string pTableName)
        {
            string retValue = string.Empty;
            if (ExistsDefaultValue)
            {
                if (this.Default[0].Value.ToLower() == "{Previous}".ToLower())
                    retValue = String.Empty;
                else if (TypeData.IsTypeDate(DataType.value))
                    retValue = new DtFunc().GetDateString(Default[0].Value);
                else if (TypeData.IsTypeDateTime(DataType.value))
                    retValue = new DtFunc().GetDateTimeString(Default[0].Value, DtFunc.FmtDateLongTime);
                else
                    retValue = this.Default[0].Value;
            }
            if (StrFunc.IsFilled(retValue) && (TypeData.IsTypeBool(this.DataType.value)))
                retValue = OTCmlHelper.GetADONetBoolValue(SessionTools.CS, retValue);
            return retValue;
        }
        #endregion GetStringDefaultValue2
        #region GetStringExternalValue
        /// <summary>
        /// Format string data (ISO) To Thread Format  
        /// </summary>
        private string GetStringExternalValue(string pValue)
        {
            string data = pValue;
            //
            if (TypeData.IsTypeDateOrDateTime(DataType.value))
            {
                try
                {
                    DateTime dt;
                    if (TypeData.IsTypeDateTime(DataType.value))
                    {
                        dt = new DtFunc().StringDateTimeISOToDateTime(pValue);

                        //MF 20120601 Ticket 17856 Mail Information complémentaire : une date à null
                        if (dt <= DateTime.MinValue && this.IsAdditionalData)
                        {
                            data = String.Empty;
                        }
                        else
                        {
                            data = DtFunc.DateTimeToString(dt, DtFunc.FmtDateTime);
                        }
                    }
                    else if (TypeData.IsTypeDate(DataType.value))
                    {
                        //PL 20111227 Nouveau format de stockage: FmtISODate 
                        dt = new DtFunc().StringDateISOToDateTime(pValue);
                        //Pour compatibiliter ascendante 
                        if (dt == DateTime.MinValue)
                            dt = new DtFunc().StringyyyyMMddToDateTime(pValue);

                        //MF 20120601 Ticket 17856 Mail Information complémentaire : une date à null
                        if (dt <= DateTime.MinValue && this.IsAdditionalData)
                        {
                            data = String.Empty;
                        }
                        else
                        {
                            data = DtFunc.DateTimeToString(dt, DtFunc.FmtShortDate);
                        }
                    }


                }
                catch { data = string.Empty; }
            }
            return data;
        }
        #endregion GetStringExternalValue
        #region GetStringValue
        /// <revision>
        ///     <version>1.0.7</version><date>20050620</date><author>PL</author>
        ///     <EurosysSupport></EurosysSupport>
        ///     <comment>
        ///     TypeData.IsTypeDateTime: Utilisation de DtFunc.FmtShortDate à la place de DtFunc.FmtDateLongTime
        ///     Permet de gérer l'affichage de DTSYS_ sur la consultation des table xxx_P
        ///     </comment>
        /// </revision>
        public string GetStringValue(object pValue)
        {
            string data = string.Empty;

            if (!(pValue is DBNull))
            {
                if (this.IsExternal2)
                {
                    data = this.GetStringExternalValue((String)pValue);
                }
                else
                {
                    if (TypeData.IsTypeDate(this.DataType.value) && pValue is DateTime)
                        data = DtFunc.DateTimeToString((DateTime)pValue, DtFunc.FmtShortDate);
                    else if (TypeData.IsTypeDateTime(this.DataType.value) && pValue is DateTime)
                        data = DtFunc.DateTimeToString((DateTime)pValue, DtFunc.FmtDateLongTime);
                    //20090909 PL Add IsTypeTime()
                    else if (TypeData.IsTypeTime(this.DataType.value) && pValue is DateTime)
                        data = DtFunc.DateTimeToString((DateTime)pValue, DtFunc.FmtISOShortTime);
                    else if (TypeData.IsTypeDec(this.DataType.value))
                    {
                        string decFormat = "d";
                        if (this.ScaleSpecified)
                            decFormat = "f" + this.Scale.ToString();
                        data = Convert.ToDecimal(pValue).ToString(decFormat);
                    }
                    else
                        data = pValue.ToString();
                }
            }
            return data;
        }
        #endregion GetStringValue

        public OpenReferentialArguments SetDDLRelation()
        {
            OpenReferentialArguments arg = new OpenReferentialArguments();
            arg.isHideInCriteriaSpecified = true;
            arg.isHideInCriteria = IsHideInCriteriaSpecified && IsHideInCriteria;


            string relationTableName = string.Empty;
            string relationColumnRelation = string.Empty;
            string relationColumnSelect = string.Empty;
            string relationListType = Cst.ListType.Repository.ToString();

            if (ArrFunc.IsFilled(Relation))
            {
                ReferentialColumnRelation relation = Relation[0];
                if (ArrFunc.IsFilled(relation.ColumnRelation) && ArrFunc.IsFilled(relation.ColumnSelect))
                {
                    arg.referential = Relation[0].TableName;
                    arg.sqlColumn = Relation[0].ColumnSelect[0].ColumnName;
                    arg.listType = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), relation.ListType, true);
                }
                else if ((relation.DDLType != null) && StrFunc.IsFilled((relation.DDLType.Value)))
                {
                    string ddlType = (relation.DDLType.Value);
                    if (Cst.IsDDLTypeBusinessCenter(ddlType) || Cst.IsDDLTypeBusinessCenterId(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.BUSINESSCENTER.ToString();
                        arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                    }
                    else if (Cst.IsDDLAmountType(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.VW_AMOUNTTYPE.ToString();
                        arg.sqlColumn = "VALUE";
                    }
                    else if (Cst.IsDDLTypeCountry(ddlType) || Cst.IsDDLTypeCountry_Country(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.COUNTRY.ToString();
                        arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                    }
                    else if (Cst.IsDDLTypeCSS(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.ACTOR.ToString();
                        arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                        arg.condition = ddlType;
                    }
                    else if (Cst.IsDDLTypeCurrency(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.CURRENCY.ToString();
                        arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                    }

                    else if (Cst.IsDDLTypeEnum(ddlType))
                    {
                        LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments(ddlType, false);
                        arg.referential = Cst.OTCml_TBL.ENUM.ToString();
                        arg.fk = loadEnum.code;
                        arg.sqlColumn = "VALUE";
                    }
                    else if (Cst.IsDDLTypeMarketId(ddlType) || Cst.IsDDLTypeMarketETD(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.MARKET.ToString();
                        arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                    }
                    else if (Cst.IsDDLTypeClearingHouse(ddlType))
                    {
                        arg.referential = Cst.OTCml_TBL.ACTOR.ToString();
                        arg.sqlColumn = OTCmlHelper.GetColunmID(arg.referential);
                    }
                    else
                    {
                        //Nickel on continue
                    }
                }
            }
            return arg;
        }

        public void SetColumnIDARole(bool pIsActorRole, int pFieldID, string pTable, string pIdentifier, string pColumnName)
        {
            IsRole = true;
            ExternalFieldID = pFieldID;
            ExternalTableName = pTable;
            ExternalIdentifier = "IDA" + "_" + pIdentifier;
            ColumnName = pIsActorRole?"IDA_ACTOR":"IDA";
            AliasTableNameSpecified = true;
            AliasTableName = "r" + pTable.Substring(0, 1).ToLower() + pIdentifier;
            DataField = ColumnName;
            Ressource = "Actor";
            RessourceSpecified = true;
            IsMandatorySpecified = true;
            IsMandatory = false;
            DataType = new ReferentialColumnDataType(); 
            DataType.value = TypeData.TypeDataEnum.integer.ToString(); ;
            ColspanSpecified = true;
            Colspan = 0;
            LabelNoWrap = true;
            Length = 10;
            Scale = 0;
            Default = new ReferentialColumnDefault[1]{new ReferentialColumnDefault()};
            Default[0].Value = string.Empty;
            IsHideSpecified = true;
            IsHide = false;
            IsHideInDataGridSpecified = true;
            IsHideInDataGrid = true;
            IsKeyFieldSpecified = true;
            IsKeyField = false;
            IsDataKeyFieldSpecified = true;
            IsDataKeyField = false;
            IsIdentitySpecified = true;
            IsIdentity.Value = false;

            IsUpdatableSpecified = true;
            IsUpdatable = new ReferentialColumnIsUpdatable();

            Relation = new ReferentialColumnRelation[1] { new ReferentialColumnRelation() };
            Relation[0].TableName = Cst.OTCml_TBL.ACTOR.ToString();
            Relation[0].ColumnRelation = new ReferentialColumnRelationColumnRelation[1] { new ReferentialColumnRelationColumnRelation() };
            Relation[0].ColumnRelation[0].ColumnName = "IDA";
            Relation[0].ColumnRelation[0].DataType = TypeData.TypeDataEnum.integer.ToString();
            Relation[0].ColumnSelect = new ReferentialColumnRelationColumnSelect[1] { new ReferentialColumnRelationColumnSelect() };
            Relation[0].ColumnSelect[0].ColumnName = "IDENTIFIER";
            Relation[0].ColumnSelect[0].DataType = TypeData.TypeDataEnum.@string.ToString();
            Relation[0].ColumnSelect[0].Ressource = "ForActor";
            Relation[0].ColumnLabel = new ReferentialColumnRelationColumnLabel[1] { new ReferentialColumnRelationColumnLabel() };
            Relation[0].ColumnLabel[0].ColumnName = "DISPLAYNAME";
            Relation[0].ColumnLabel[0].DataType = TypeData.TypeDataEnum.@string.ToString();
            Relation[0].ColumnLabel[0].Ressource = "DISPLAYNAME";
            Relation[0].Condition = new ReferentialColumnRelationCondition[1] { new ReferentialColumnRelationCondition() };
            Relation[0].Condition[0].SQLWhereSpecified = true;
            if (pIsActorRole)
            {
                Relation[0].Condition[0].SQLWhere = "(1 = " + Cst.COLUMN_VALUE + AliasTableName + "." + pColumnName + "%%)";
            }
            else
            {
                Relation[0].Condition[0].SQLWhere = ((pIdentifier == "INVOICING") ? "1=1" : "1=2");
            }
            Relation[0].ListTypeSpecified = true;
            Relation[0].ListType = Cst.ListType.Repository.ToString();

            if (pIsActorRole)
            {
                Relation[0].AutoComplete = new ReferentialAutoComplete();
                Relation[0].AutoComplete.Value = "IDACTORROLE" +pFieldID;

            }
        }
        public void SetColumnIDRole(int pFieldID, string pTable, string pIdentifier, string pColumnName, 
            string pRoleType, string pRessourceName, string pDefaultValue, bool pIsUpdable)
        {
            IsRole = true;
            ExternalFieldID = pFieldID;
            ExternalTableName = pTable;
            ExternalIdentifier = pIdentifier;
            ColumnName = pColumnName;
            AliasTableNameSpecified = true;
            AliasTableName = "r" + pTable.Substring(0, 1).ToLower() + pIdentifier;
            DataField = pColumnName;
            Ressource = pRessourceName.Trim();
            RessourceSpecified = true;
            IsMandatorySpecified = true;
            IsMandatory = true;
            DataType = new ReferentialColumnDataType(); 
            DataType.value = TypeData.TypeDataEnum.@bool.ToString();
            ColspanSpecified = true;
            Colspan = 1;
            Length = 1;
            Scale = 0;
            Default = new ReferentialColumnDefault[1] { new ReferentialColumnDefault() };
            Default[0].Value = pDefaultValue;
            IsHideSpecified = true;
            IsHide = false;
            IsHideInDataGridSpecified = true;
            IsHideInDataGrid = true;
            IsKeyFieldSpecified = true;
            IsKeyField = false;
            IsDataKeyFieldSpecified = true;
            IsDataKeyField = false;
            IsIdentitySpecified = true;
            IsIdentity.Value = false;

            IsUpdatable = new ReferentialColumnIsUpdatable();
            IsUpdatableSpecified = true;
            IsUpdatable.Value = pIsUpdable;
        }
        #endregion Methods

    }

    #region class Columnhtml_BLOCK
    public class ReferentialColumnhtml_BLOCK
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int blockbyrow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool blockbyrowSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int columnbyrow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool columnbyrowSpecified;
        //2009109 PL
        [System.Xml.Serialization.XmlElementAttribute("Information", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialInformation Information;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InformationSpecified;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool backcolorheaderSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string backcolorheader;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool backcolorSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string backcolor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bordercolorSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string bordercolor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cssheadertagSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string cssheadertag;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool startdisplaySpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string startdisplay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reverseSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool reverse;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool squareSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool square;


        #region SetBlock
        public void SetBlock(int pColumnByRow, string pTitle, string pLabelMessage)
        {
            columnbyrowSpecified = true;
            columnbyrow = pColumnByRow;
            title = Ressource.GetString(pTitle, true);
            InformationSpecified = true;
            Information = new ReferentialInformation();
            Information.TypeSpecified = true;
            Information.Type = Cst.TypeInformationMessage.Information;
            Information.LabelMessageSpecified = true;
            Information.LabelMessage = pLabelMessage;
        }
        #endregion SetBlock

    }
    #endregion Columnhtml_BLOCK
    #region class Columnhtml_TITLE
    public class ReferentialColumnhtml_TITLE
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int blockbyrow;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool blockbyrowSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int columnbyrow;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool columnbyrowSpecified;
    }

    #endregion Columnhtml_TITLE
    #region ColumnJavaScript
    public class ReferentialColumnJavaScript
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Script", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public JavaScript.JavaScriptScript[] Script;
    }

    #endregion ColumnJavaScript
    #region Columnhtml_HR
    public class ReferentialColumnhtml_HR
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int titlemargin;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string titlepuce;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string color;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string size;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int columnbyrow;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool columnbyrowSpecified;
    }
    #endregion Columnhtml_HR
    #region ColumnIsOrderBy
    public class ReferentialColumnIsOrderBy
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string order;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool orderSpecified;
    }
    #endregion
    #region ColumnIsIdentity
    public class ReferentialColumnIsIdentity
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool Value;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string source;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceSpecified;
    }
    #endregion
    #region ReferentialColumnIsUpdatable
    public class ReferentialColumnIsUpdatable
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool Value;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool isupdatableincreation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isupdatableincreationSpecified;
    }
    #endregion ReferentialColumnIsUpdatable

    /// <summary>
    /// Représente un link 
    /// <para>Le link ouvre un référentiel ou un document (exemple un pdf)</para>
    /// </summary>
    public class ReferentialColumnIsHyperLink
    {
        /// <summary>
        /// True pour activer l'hyperlink sur la colonne
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool Value;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool linktypeSpecified;
        /// <summary>
        /// Type de Link 
        /// <para>Les valeurs possibles sont "document" ou "column" ou "external"</para>
        /// <para>document: pour l'ouverture d'un document</para>
        /// <para>column:   pour l'ouverture d'un référentiel</para>
        /// <para>external: pour l'ouverture d'un document externe à Spheres</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string linktype;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dataSpecified;
        /// <summary>
        /// <para>
        /// Lorsque link de de type "document", représente la colonne qui contient le document
        /// </para>
        /// <para>
        /// Lorsque link est de type "column", représente la colonne qui contient la valeur PK utilisée pour ouvrir le référentiel
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string data;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeSpecified;
        /// <summary>
        /// <para>
        /// Lorsque link est de type "document", représente le type de document (exemple application/pdf)
        /// </para>
        /// <para>
        /// Lorsque link est de type "column", représente un nom de colonne caractéristique d'un référentiel (exemple IDA pour le référentiel ACTEUR)
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool nameSpecified;
        /// <summary>
        /// <para>
        /// Lorsque link est de type "document", représente le nom final du fichier
        /// </para>
        /// <para>
        /// Lorsque link est de type "column", non utilisé
        /// </para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool linktableSpecified;
        /// <summary>
        /// <para>
        /// Lorsque link est de type "document", représente la table qui contient le document
        /// </para>
        /// <para>
        /// Lorsque link de de type "column", non utilisé
        /// </para>
        /// </summary>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string linktable;
    }

    public class ReferentialLogo
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValueSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string columnname;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool columnnameSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string image;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool imageSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tooltip;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tooltipSpecified;
    }

    #region ReferentialColumnDefault
    public class ReferentialColumnDefault
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Value;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;
    }

    #endregion ReferentialColumnDefault

    #region ReferentialColumnGroupBy
    public class ReferentialColumnGroupBy
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Aggregate;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AggregateSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SqlGroupBy;

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SqlGroupBySpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Cst.GroupingSet GroupingSet;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsGroupBy;
    }

    #endregion class ReferentialColumnGroupBy

    #region ReferentialInformationToolTipMessage
    public class ReferentialInformationToolTipMessage
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool titleSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string cssToolTip;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cssToolTipSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool widthSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string height;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool heightSpecified;
    }
    #endregion ReferentialInformationToolTipMessage
    #region ReferentialInformationURL
    public class ReferentialInformationURL
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string @enum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool @enumSpecified;
    }
    #endregion ReferentialInformationURL
    #region ReferentialColumnRelation
    public class ReferentialColumnRelation
    {
        //20090625 PL Add iscolor
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool iscolor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool iscolorSpecified;

        [System.Xml.Serialization.XmlElementAttribute("DDLType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelationDDLType DDLType;

        // FI 20171025 [23533]  ce membre devient une property
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string RelationColumnSQLName
        {
            get
            {
                string ret = string.Empty;
                if (ArrFunc.IsFilled(ColumnSelect))
                {
                    if (-1 == ColumnSelect[0].ColumnName.IndexOf("."))
                        ret = AliasTableName + "_" + ColumnSelect[0].ColumnName;
                    else
                        ret = ColumnSelect[0].ColumnName;
                }
                return ret.ToUpper();
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ListType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ListTypeSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TableName;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TableNameForDDL;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TableNameForDDLSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AliasTableName;

        [System.Xml.Serialization.XmlElementAttribute("ColumnRelation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelationColumnRelation[] ColumnRelation;

        [System.Xml.Serialization.XmlElementAttribute("ColumnSelect", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelationColumnSelect[] ColumnSelect;

        [System.Xml.Serialization.XmlElementAttribute("ColumnLabel", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelationColumnLabel[] ColumnLabel;

        [System.Xml.Serialization.XmlElementAttribute("Condition", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelationCondition[] Condition;

        [System.Xml.Serialization.XmlElementAttribute("AutoComplete", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialAutoComplete AutoComplete;
    }
    #endregion ReferentialColumnRelation

    #region AutoComplete
    public class ReferentialAutoComplete
    {
        #region Members
        private bool _enabled = false;
        private JQuery.Engines _engine = JQuery.Engines.JQuery;
        private bool _matchContains = false;
        private int _scrollHeight = 220;
        private int _minChars = 0;
        private Nullable<int> _width = null;
        private bool _sourceLocal = true;
        private int _max = 300;
        private bool _multiple = true;
        private string _multipleSeparator = ";";
        private string _source = "defaultset";
        private string _formatItem = "";
        private string _formatResult = "";
        private string _key = "";

        private bool _labelVisible = false;
        private List<string> _emphElements = new List<string>();
        #endregion Members
        #region Accessors
        #region Value
        /// <summary>
        /// Value for InitInfoCollection
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value
        {
            get { return _key; }
            set
            {
                _key = value;
                _enabled = true;
                _engine = JQuery.Engines.JQuery;
                _matchContains = true;
                _scrollHeight = 300;
                _minChars = 2;
                if ((false == _width.HasValue) || (_width.Value < 0))
                    _width = 500;
                _max = 50;
                _sourceLocal = false;
                _multiple = false;
                _multipleSeparator = ";";
                _source = "./DataWebServices/GetAutoCompleteData.aspx";
                _formatItem = "row[0]";
                _formatResult = "row[0]";
                _emphElements = new List<string>();
                _emphElements.Add("FRA");
                _emphElements.Add("USA");
            }
        }
        #endregion Value

        #region Enabled
        /// <summary>
        /// Get and set the activation state of the autocomplete engine
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        #endregion Enabled
        #region Engine
        /// <summary>
        /// Get and set the autocomplete engine name
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("engine")]
        public string Engine
        {
            get { return _engine.ToString(); }
            set
            {
                try
                {
                    _engine = (JQuery.Engines)Enum.Parse(typeof(JQuery.Engines), value, true);
                }
                catch
                {
                    _engine = JQuery.Engines.JQuery;
                    // when an unsupported engine is requested then the autocomplete functionality will be disabled
                    _enabled = false;
                }
            }
        }
        #endregion Engine
        #region MatchContains
        /// <summary>
        /// Get and set the rule used to find the matched elements. 
        /// If true then all the elements will be returned which contain the input string filter,
        /// if false then only the elements will be returned which begin with the input string filter.
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("matchContains")]
        public bool MatchContains
        {
            get { return _matchContains; }
            set { _matchContains = value; }
        }
        #endregion MatchContains
        #region ScrollHeight
        /// <summary>
        /// Get and set the maximum heigth of the autocomplete graphical control
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("scrollHeight")]
        public int ScrollHeight
        {
            get { return _scrollHeight; }
            set { _scrollHeight = value; }
        }
        #endregion ScrollHeight
        #region MinChars
        /// <summary>
        /// Get and set the minimum length of the input string filter before the autocomplete get some results
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("minChars")]
        public int MinChars
        {
            get { return _minChars; }
            set { _minChars = value; }
        }
        #endregion MinChars
        #region Width
        /// <summary>
        /// Get and set the width of the autocomplete graphical control
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("width")]
        public int Width
        {
            get
            {
                if (_width.HasValue)
                    return _width.Value;
                else
                    return -1;
            }
            set { _width = value; }
        }
        #endregion Width
        #region SourceLocal
        /// <summary>
        /// Get and set the source type.
        /// If true then the data source will be a Javascript object (ex: var vec=new Array(["1","2"])), 
        /// if false then the data source will be a Web Service (ex: source.aspx)
        /// </summary>
        /// <remarks>
        /// 1. The type of a local source can be 
        /// - a vector (TOTEST) (ex ["first", "second"]), 
        /// - a matrix (ex: [["first", "second"],["third", "fourth"]]) 
        /// - or (NOT SUPPORTED yet) a vector of records .
        /// 2. A remote source (NOT SUPPORTED yet!)  should return 
        /// - a strings collection separated by the carriage return char (\n) to emulate a vector object,
        ///   Ex:
        ///   "first value"
        ///   "second value"
        /// - or a strings of strings collection whose the line elements are separated by the pipe elements 
        ///   and the column elements are separated by a carriage return char to emulate a multidimensional matrix
        ///   Ex:
        ///   "first"|"second"|"third"
        ///   "fourth"|"fifth"|"..."
        /// </remarks>
        [System.Xml.Serialization.XmlAttribute("sourceLocal")]
        public bool SourceLocal
        {
            get { return _sourceLocal; }
            set { _sourceLocal = value; }
        }
        #endregion SourceLocal
        #region Max
        /// <summary>
        /// Limit the number of items in the select box. Is also sent as a "limit" parameter with a remote request
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("max")]
        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }
        #endregion Max
        #region Multiple
        /// <summary>
        /// Whether to allow more than one autocompleted-value to enter
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("multiple")]
        public bool Multiple
        {
            get { return _multiple; }
            set { _multiple = value; }
        }
        #endregion Multiple
        #region MultipleSeparator
        /// <summary>
        /// Seperator to put between values when using multiple option
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("multipleSeparator")]
        public string MultipleSeparator
        {
            get { return _multipleSeparator; }
            set { _multipleSeparator = value; }
        }
        #endregion MultipleSeparator
        #region Source
        /// <summary>
        /// The identifier of the source which gives us the data set of the auto complete control.
        /// If the source is LOCAL then the source name is the name of the vector/matrix variable containing the dataset (Ex: 'vec' <seealso cref="SourceLocal"/>) 
        /// if the source is REMOTE then the source name is the name of the web service returning the dataset (Ex: 'ListViewer.aspx')
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("source")]
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }
        #endregion Source
        #region FormatItem
        /// <summary>
        /// The format with which the matched elements of the data source will be shown to the user. 
        /// Every single element of any line o the data source is identified by his own index (just 0, when vector is used, [0..n] when a matrix is used).
        /// Ex: format: "'Contract:' + row[0] + '(id:' + row[1] + ')'", matched element ["FP2", "62298", "Fut"] --> resulting HMI string: "Contract: FPD (id: 62298)"
        /// </summary>
        /// <remarks>see <seealso cref="EFS.Common.Web.JQuery.AutoComplete.ConvertReferentialFormatToHTML"/>  to obtain the supported HTML format tags </remarks>
        [System.Xml.Serialization.XmlAttribute("formatItem")]
        public string FormatItem
        {
            get { return _formatItem; }
            set { _formatItem = value; }
        }
        #endregion FormatItem
        #region FormatResult
        /// <summary>
        /// The format with which the source elements will be shown to the user.
        /// Every single element of any line o the data source is identified by his own index (just 0, when vector is used, [0..n] when a matrix is used).
        /// Ex: format: "'Contract:' + row[0] + '(id:' + row[1] + ')'", matched element ["FP2", "62298", "Fut"] --> resulting HMI string: "Contract: FPD (id: 62298)"
        /// </summary>
        /// <remarks>see <seealso cref="EFS.Common.Web.JQuery.AutoComplete.ConvertReferentialFormatToHTML"/> to obtain the supported HTML format tags </remarks>
        [System.Xml.Serialization.XmlAttribute("formatResult")]
        public string FormatResult
        {
            get { return _formatResult; }
            set { _formatResult = value; }
        }
        #endregion FormatResult
        #region Key
        /// <summary>
        /// Key for InitInfoCollection
        /// </summary>
        //PL 20130516 Test
        [System.Xml.Serialization.XmlAttribute("key")]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        #endregion Key
        #region LabelVisible
        /// <summary>
        /// Show or hide the autocomplete label (default value: false)
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("labelVisible")]
        public bool LabelVisible
        {
            get { return _labelVisible; }
            set { _labelVisible = value; }
        }
        #endregion LabelVisible
        #region EmphElements
        /// <summary>
        /// Additional style collections for the matched elements returned to the user. 
        /// If a matched element contains one or more of the collected strings
        /// then that string will be "emphatised" (IOW contourned by the "emph" html tag) 
        /// </summary>
        /// <remarks>not yet supported</remarks>
        [System.Xml.Serialization.XmlIgnore()]
        public List<string> EmphElements
        {
            get { return _emphElements; }
            set { _emphElements = value; }
        }
        #endregion EmphElements
        #endregion Accessors
        #region Constructors
        public ReferentialAutoComplete()
        {
        }
        public ReferentialAutoComplete(bool pEnabled, JQuery.Engines pEngine, bool pMatchContains, int pScrollHeight, int pMinChars,
            int pWidth, bool pSourceLocal, int pMax, bool pMultiple, string pMultipleSeparator, string pSource,
            string pFormatItem, string pFormatResult, bool pLabelVisible, string pKey)
        {
            _enabled = pEnabled;
            _engine = pEngine;
            _matchContains = pMatchContains;
            _scrollHeight = pScrollHeight;
            _minChars = pMinChars;
            _width = pWidth;
            _sourceLocal = pSourceLocal;
            _max = pMax;
            _multiple = pMultiple;
            _multipleSeparator = pMultipleSeparator;
            _source = pSource;
            _formatItem = pFormatItem;
            _formatResult = pFormatResult;
            _labelVisible = pLabelVisible;
            _key = pKey;
        }
        #endregion Constructors
        #region Methods
        #region ToInitInfo
        /// <summary>
        /// Translate an xml autocomplete referential class in a Java.InitInfo struct
        /// </summary>
        /// <remarks>No source is initialised, 
        /// call <seealso cref="JQuery.AutoComplete.InitInfo.InitLocalDataSource"/> from the returned structure</remarks>
        /// <param name="idHdn">mandatory, the control id of the hidden field part of the autocomplete html structure</param>
        /// <param name="idTxt">mandatory, the control id of the text field part of the autocomplete html structure</param>
        /// <param name="idLbl">mandatory, the control id of the label part of the autocomplete html structure</param>
        /// <returns>one brand new InitInfo structure sharing the same instance parameter </returns>
        public JQuery.AutoComplete.InitInfo ToInitInfo(string idHdn, string idTxt, string idLbl)
        {
            JQuery.AutoComplete.InitInfo infoAutoComplete =
                new JQuery.AutoComplete.InitInfo(this.Key, idHdn, idTxt, idLbl, this.Source);

            infoAutoComplete.MatchContains = this.MatchContains;
            infoAutoComplete.FormatItem = this.FormatItem;
            infoAutoComplete.FormatResult = this.FormatResult;
            infoAutoComplete.MinChars = this.MinChars;
            infoAutoComplete.ScrollHeight = this.ScrollHeight;
            infoAutoComplete.Width = this.Width;
            infoAutoComplete.SourceLocal = this.SourceLocal;
            infoAutoComplete.Engine = this._engine;
            infoAutoComplete.Max = this.Max;
            infoAutoComplete.Multiple = this.Multiple;
            infoAutoComplete.MultipleSeparator = this.MultipleSeparator;

            return infoAutoComplete;
        }
        #endregion ToInitInfo
        #endregion Methods
    }
    #endregion AutoComplete

    #region ReferentialColumnRelationDDLType
    public class ReferentialColumnRelationDDLType
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string misc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool miscSpecified;
    }
    #endregion ReferentialColumnRelationDDLType
    #region ReferentialColumnRelationColumnRelation
    public class ReferentialColumnRelationColumnRelation
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DataType;
    }

    #endregion ReferentialColumnRelationColumnRelation
    #region ReferentialColumnRelationColumnLabel
    public class ReferentialColumnRelationColumnLabel
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Ressource;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DataType;
    }

    #endregion ReferentialColumnRelationColumnLabel
    #region ReferentialColumnRelationColumnSelect
    public class ReferentialColumnRelationColumnSelect
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Ressource;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DataType;
    }

    #endregion ReferentialColumnRelationColumnSelect
    #region ReferentialColumnRelationCondition
    public class ReferentialColumnRelationCondition
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apply;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool applySpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TableName;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AliasTableName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AliasTableNameSpecified;

        [System.Xml.Serialization.XmlElementAttribute("ColumnRelation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnRelationColumnRelation[] ColumnRelation;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SQLWhere;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SQLWhereSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ColumnName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnNameSpecified;
    }
    #endregion ReferentialColumnRelationCondition
    #region ReferentialColumnStyleBase
    public class ReferentialColumnStyleBase
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string type;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool modelSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string model;
        // EG 20160224 New Gestion du rendu d'une cellule sur la base d'une class CSS complexe (Gestion du REQUESTTYPE)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool complexModelSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string complexModel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool versionSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string version;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool WhenSpecified;
        [System.Xml.Serialization.XmlElementAttribute("When", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnStyleWhen[] When;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OtherwiseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("Otherwise", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialColumnStyleOtherwise Otherwise;
    }
    #endregion ReferentialColumnStyleBase
    #region ReferentialColumnStyleWhen
    public class ReferentialColumnStyleWhen
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string test;
        //
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion ReferentialColumnStyleWhen
    #region ReferentialColumnStyleOtherwise
    public class ReferentialColumnStyleOtherwise
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion ReferentialColumnStyleOtherwise
    #region ReferentialColumnRowStyle
    public class ReferentialColumnRowStyle : ReferentialColumnStyleBase
    {
    }
    #endregion ReferentialColumnRowStyle
    #region ReferentialColumnCellStyle
    public class ReferentialColumnCellStyle : ReferentialColumnStyleBase
    {
    }
    #endregion ReferentialColumnCellStyle

    /// <summary>
    /// Représente les propriétés d'une colonne vis à vis des resources
    /// </summary>
    public class ReferentialColumnResource
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool prefixSpecified;
        /// <summary>
        /// Obtient ou définit un prefix qui sert à obtenir le nom de la ressource
        /// <para>Lorsque prefix est renseigné Spheres® recherche la resource qui commence par {prefix}</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("prefix", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string prefix;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sqltableSpecified;
        /// <summary>
        /// Obtient ou définit le nom d'une table SQL qui sert à obtenir le nom de la ressource
        /// <para>Lorsque sqltable est renseigné Spheres® recherche la resource dans cette table (ex. SYSTEMMSG)</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("sqltable", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sqltable;

        /// <summary>
        /// Obtient ou définit un drapeau qui indique si l'affichage de la donnée doit s'appuyer sur les resources
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool IsResource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isCriteriaDisplaySpecified;
        /// <summary>
        /// Obtient ou définit un drapeau qui indique si la valeur d'un critère sur cette donnée doit être remplacée par une resource
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("criteriadisplay", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isCriteriaDisplay;

        /// <summary>
        /// 
        /// </summary>
        public ReferentialColumnResource()
        {
            IsResource = false;
            isCriteriaDisplay = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        public ReferentialColumnResource(bool pValue)
        {
            IsResource = pValue;
            isCriteriaDisplay = true;
        }
    }

    #region ReferentialXSLFileName
    public class ReferentialXSLFileName
    {
        /// <summary>
        /// Full pathname of XSL file 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;

        /// <summary>
        /// Title for item menu
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool titleSpecified;
        /// <summary>
        /// Tooltip for item menu
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tooltip;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool tooltipSpecified;
    }
    #endregion XSLFileName


    /// <summary>
    /// Repésente le type de la donnée
    /// </summary>
    /// FI 20171025 [23533] Add 
    public class ReferentialColumnDataType
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tzdbidSpecified;
        /// <summary>
        /// Donne le timeZone associé à un horodatage
        /// <para>S'aplique uniquement aux données date, datetime, time lorsque la colonne Spheres® représente un horodatage (DTINS, DTUPD,DTEXECUTION etc...)</para>
        /// <para>Iana timezone expected</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("tzdbid")]
        public string tzdbid;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displaySpecified;
        /// <summary>
        /// Indique les instructions d'affichage d'un horodatage
        /// <para>S'aplique uniquement aux données date, datetime, time lorsque la colonne Spheres® représente un horodatage (DTINS, DTUPD,DTEXECUTION etc...)</para>
        /// <para>L'affichage est fonction du paramétrage dans profil</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("display")]
        public Cst.DataTypeDisplayMode display;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datakindSpecified;
        /// <summary>
        /// Donne des informations supplémentaires sur le contenu de la colonne
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("datakind")]
        public Cst.DataKind datakind;

        /// <summary>
        /// Représente le type de la donnée (voir TypeData.TypeDataEnum )
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string value;

        /// <summary>
        /// 
        /// </summary>
        public ReferentialColumnDataType()
        {
        }

    }


    /// <summary>
    /// Permet de piloter la journalisation des actions utilisateurs
    /// </summary>
    /// FI 20140519 [19923] add Class
    public class ReferentialRequestTrack
    {
        /// <summary>
        /// description des data insérées dans le log 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("RequestTrackData")]
        public ReferentialRequestTrackData[] RequestTrackData;
    }


    /// <summary>
    /// Pilote l'alimentation des data dans le journal des actions utilisateurs 
    /// </summary>
    public class ReferentialRequestTrackData
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean columnIdASpecified;

        /// <summary>
        /// Colonne des acteurs 
        /// </summary>
        [System.Xml.Serialization.XmlElement("ColumnIdA")]
        public ReferentialRequestTrackDataColumn columnIdA;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean columnIdBSpecified;
        /// <summary>
        /// Colonne des books
        /// </summary>
        [System.Xml.Serialization.XmlElement("ColumnIdB")]
        public ReferentialRequestTrackDataColumn columnIdB;

        /// <summary>
        /// Colonne des regroupements
        /// </summary>
        [System.Xml.Serialization.XmlElement("ColumnGrp")]
        public ReferentialRequestTrackDataColumn columnGrp;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ReferentialRequestTrackDataColumn
    {
        /// <summary>
        /// alias de la colonne 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("alias")]
        public string alias;
        /// <summary>
        /// Expression SQL de la colonne 
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string sqlCol;

    }


    public class ReferentialInformation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Type;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeSpecified;

        [System.Xml.Serialization.XmlElementAttribute("ToolTipMessage", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialInformationToolTipMessage ToolTipMessage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ToolTipMessageSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LabelMessage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LabelMessageSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PopupMessage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PopupMessageSpecified;

        //PL 20110324
        [System.Xml.Serialization.XmlElementAttribute("URL", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ReferentialInformationURL URL;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool URLSpecified;
        #endregion Members
    }

    #region TypeAhead (New autocomplete)
    public class ReferentialTypeAhead
    {
        #region Members
        private bool _enabled = true;
        private string _table;
        private string _values;
        #endregion Members
        #region Accessors
        #region Enabled
        [System.Xml.Serialization.XmlAttribute("enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        #endregion Enabled
        #region Role
        [System.Xml.Serialization.XmlAttribute("values")]
        public string Values
        {
            get { return _values; }
            set { _values = value; }
        }
        #endregion Role
        #region Table
        [System.Xml.Serialization.XmlAttribute("table")]
        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }
        #endregion Table
        #endregion Accessors
        #region Constructors
        public ReferentialTypeAhead()
        {
        }
        public ReferentialTypeAhead(bool pEnabled, string pTable, string pValues)
        {
            _enabled = pEnabled;
            _table = pTable;
            _values = pValues;
        }
        #endregion Constructors
    }
    #endregion TypeAhead (New autocomplete)
}
#region Using Directives
using System;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;


using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Permission;
using EFS.Status;

using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Enum;
#endregion Using Directives

namespace EFS.Common
{

    #region SQL_Table
    public class SQL_Table
    {
        #region Enums

        /// <summary>
        /// Yes: Avec prise en compte de DTENABLED, No: Sans prise en compte de DTENABLED
        /// </summary>
        public enum ScanDataDtEnabledEnum
        {
            /// <summary>
            /// Avec prise en compte de DTENABLED
            /// </summary>
            Yes,
            /// <summary>
            /// Sans prise en compte de DTENABLED
            /// </summary>
            No
        }

        /// <summary>
        /// Recherche des données template oui/non 
        /// </summary>
        /// FI 20130423 [] add enum ScanDataTemplate
        public enum ScanDataTemplate
        {
            /// <summary>
            /// Considération des données Template uniquement
            /// </summary>
            Yes,
            /// <summary>
            /// Considération des données non Template uniquement
            /// </summary>
            No
        }

        /// <summary>
        /// Yes: avec restrictions, No:Sans restrictions
        /// </summary>
        public enum RestrictEnum
        {
            /// <summary>
            /// Avec restrictions
            /// </summary>
            Yes,
            /// <summary>
            /// Sans restrictions
            /// </summary>
            No
        }

        #endregion

        #region Memders
        protected CSManager _csManager;
        /// <summary>
        ///  transaction, doit être valorisé pour que select s'effectue ds une transaction
        /// </summary>
        protected IDbTransaction _dbTransaction;
        /// <summary>
        /// 
        /// </summary>
        private string _sqlObject;
        /// <summary>
        /// Colonnes de la query de chgt
        /// </summary>
        private string _sqlCol;
        /// <summary>
        /// From de la query de chgt
        /// </summary>
        private string _sqlFrom;
        /// <summary>
        /// Where de la query de chgt
        /// </summary>
        private string _sqlWhere;
        /// <summary>
        /// Order by de la query de chgt
        /// </summary>
        private string _sqlOrderBy;
        /// <summary>
        /// 
        /// </summary>
        private SQL_EfsObject _efsObject;
        /// <summary>
        /// 
        /// </summary>
        private bool _isLoaded;
        /// <summary>
        /// 
        /// </summary>
        private bool _isExecutedScalar;
        /// <summary>
        /// 
        /// </summary>
        /// RD 20090609  
        /// _isExecutedColumn changé de private en protected, pour pouvoir l'utiliser dans SQL_Quote.GetQuoteByPivotCurrency()
        protected bool _isExecutedColumn;
        private bool _isFound;

        private int _maxRows;

        /// <summary>
        /// Datatable qui contient le jeu de résultat
        /// </summary>
        private DataTable _dt;
        /// <summary>
        ///  Pilote la prise en compte de DTENABLED
        /// </summary>
        private ScanDataDtEnabledEnum _scanDataDtEnabled;
        /// <summary>
        ///  Pilote la prise en compte de DTENABLED
        /// </summary>
        private DateTime _dtRefForDtEnabled;

        /// <summary>
        ///  Pilote la prise en compte des données TEMPLATE
        /// </summary>
        private Nullable<ScanDataTemplate> _scanDataTemplate;

        /// <summary>
        /// Indicateur qui permet l'ajout de ROWVERSION
        /// </summary>
        private bool _isAddRowVersion;
        /// <summary>
        /// Parameters de la query de chargement
        /// </summary>
        private readonly DataParameters _dataparameters;
        #endregion

        #region Accessor
        /// <summary>
        /// Obtient le datatable qui contient le jeu de résultat
        /// </summary>
        public DataTable Dt
        {
            get
            {
                return _dt;
            }
        }

        /// <summary>
        /// Obtient les lignes du jeu de résultat
        /// <para>Obtient null si chgt non effectué</para>
        /// </summary>
        public DataRowCollection Rows
        {
            get
            {
                DataRowCollection ret = null;
                if (IsLoaded)
                    ret = Dt.Rows;
                return ret;
            }
        }

        /// <summary>
        /// Obtient la 1er ligne du jeu de résultat
        /// <para>Obtient null si chgt non effectué</para>
        /// </summary>
        public DataRow FirstRow
        {
            get
            {
                DataRow dr = null;
                if (IsLoaded)
                    dr = Dt.Rows[0];
                return dr;
            }
        }

        /// <summary>
        /// Obtient ou définit le nb de ligne Max que doit contenir le jeu de résultat
        /// </summary>
        public int MaxRows
        {
            get { return _maxRows; }
            set
            {
                if (_maxRows != value)
                {
                    InitProperty();
                    _maxRows = value;
                }
            }
        }

        /// <summary>
        /// Return true when find in table xxx, else false.
        /// if  IsScanned == false => Execute select(count(*)) only and the dataSet is never Filled
        /// Use LoadMethod to Fill the dataset with select complete
        /// </summary>
        public bool IsFound
        {
            get
            {
                if ((!_isExecutedScalar) && (!_isExecutedColumn))
                    LoadTable(new string[1] { "1" });
                return _isFound;
            }
        }


        /// <summary>
        /// Obtient True si le jeu de résultat est chargé
        /// if IsScanned == false => Execute select complete for fill dataSet 
        /// </summary>
        public virtual bool IsLoaded
        {
            get
            {
                if (!_isExecutedColumn)
                    LoadTable(new string[1] { "*" });
                return _isLoaded;
            }
        }

        /// <summary>
        /// Obtient le nbr de ligne du jeu de résultat
        /// </summary>
        public int RowsCount
        {
            get
            {
                if (null != Dt)
                    return Dt.Rows.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Key
        {
            get { return string.Empty; }
        }


        /// <summary>
        /// Obtient la ConnectionString (sans attribut Spheres [Cache, trace etc..] 
        /// </summary>
        public string CS
        {
            get { return _csManager.Cs; }
        }


        /// <summary>
        /// Défine une transaction pour l'exécution des requête
        /// </summary>
        public IDbTransaction DbTransaction
        {
            get { return _dbTransaction; }
            set { _dbTransaction = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected string SQLCol
        {
            get { return _sqlCol; }
            set { _sqlCol = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected string SQLFrom
        {
            get { return _sqlFrom; }
            set { _sqlFrom = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected string SQLWhere
        {
            get
            {
                string sqlWhere = StrFunc.TrimStart_CrLfTabSpace(_sqlWhere);
                if (StrFunc.IsFilled(sqlWhere) && !sqlWhere.StartsWith(SQLCst.WHERE.Trim()))
                    sqlWhere = Cst.CrLf + SQLCst.WHERE + sqlWhere;
                return sqlWhere;
            }
            set { _sqlWhere = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected string SQLOrderBy
        {
            get
            {
                string sqlOrderBy = StrFunc.TrimStart_CrLfTabSpace(_sqlOrderBy);
                if (StrFunc.IsFilled(sqlOrderBy) && !sqlOrderBy.StartsWith(SQLCst.ORDERBY.Trim()))
                    sqlOrderBy = Cst.CrLf + SQLCst.ORDERBY + sqlOrderBy;
                return sqlOrderBy;
            }
            set { _sqlOrderBy = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string SQLObject
        {
            get { return _sqlObject; }
            set { _sqlObject = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected SQL_EfsObject EfsObject
        {
            set { _efsObject = value; }
            get { return _efsObject; }
        }

        /// <summary>
        /// Obtient ou définit la prise en compte de DTENABLED
        /// </summary>
        public ScanDataDtEnabledEnum ScanDataDtEnabled
        {
            get { return _scanDataDtEnabled; }
            set
            {
                if (_scanDataDtEnabled != value)
                {
                    InitProperty();
                    _scanDataDtEnabled = value;
                }
            }
        }
        /// <summary>
        /// Obtient ou définit la date de référence pour la prise en compte de DTENABLED
        /// </summary>
        public DateTime DtRefForDtEnabled
        {
            get { return _dtRefForDtEnabled; }
            set
            {
                if (_dtRefForDtEnabled != value)
                {
                    InitProperty();
                    _dtRefForDtEnabled = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit la prise en compte des données TEMPLATE 
        /// <para>Null pour considérer toutes les données (template ou non) (Comportement par défaut)</para>
        /// <para>Yes pour considérer uniquement les données template</para>
        /// <para>No pour considérer uniquement les données non template </para>
        /// </summary>
        /// FI 20130423 [] add ScanTemplate
        public Nullable<ScanDataTemplate> ScanTemplate
        {
            get { return _scanDataTemplate; }
            set
            {
                if (_scanDataTemplate != value)
                {
                    InitProperty();
                    _scanDataTemplate = value;
                }
            }
        }

        /// <summary>
        /// Obtient ou définit l'ajout systématique de la colonne ROWVERSION 
        /// </summary>
        public bool IsAddRowVersion
        {
            get { return _isAddRowVersion; }
            set { _isAddRowVersion = value; }
        }

        /// <summary>
        /// Obtient les parameters utilisés pour exécuter la query
        /// </summary>
        public DataParameters Dataparameters
        {
            get { return _dataparameters; }
        }

        #endregion
        //
        #region constructor
        /// <summary>
        /// Constructeur qui permet de passer directement un jeu de résultat. 
        /// <para>Attention les noms de colonnes du jeu de résultat doivent coincider avec ceux attendus dans les properties</para>
        /// </summary>
        /// FI 20131223 [19337] add Constructor 
        public SQL_Table(DataTable pDt)
        {
            SetDataTable(pDt);
        }
        public SQL_Table(string pCs, string pSqlObject) 
            : this(pCs, null, pSqlObject, ScanDataDtEnabledEnum.No, DateTime.MinValue) { }
        public SQL_Table(string pCs, string pSqlObject, ScanDataDtEnabledEnum pScanDataDtEnabled) 
            : this(pCs, null, pSqlObject, pScanDataDtEnabled, DateTime.MinValue) { }
        public SQL_Table(string pCs, IDbTransaction pDbTransaction, string pSqlObject, ScanDataDtEnabledEnum pScanDataDtEnabled)
            : this(pCs, pDbTransaction, pSqlObject, pScanDataDtEnabled, DateTime.MinValue) { }
        public SQL_Table(string pCs, IDbTransaction pDbTransaction, string pSqlObject, ScanDataDtEnabledEnum pScanDataDtEnabled, DateTime pDtRefForDtEnabled)
        {
            //PL 20130604 New Constructor with pDtRefForDtEnabled (Vu)
            _dbTransaction = pDbTransaction;
            _csManager = new CSManager(pCs);
            _sqlObject = pSqlObject;
            _scanDataDtEnabled = pScanDataDtEnabled;
            _dtRefForDtEnabled = pDtRefForDtEnabled;
            _isAddRowVersion = false;
            _dataparameters = new DataParameters();
            _scanDataTemplate = null;
        }
        #endregion
        //
        #region protected virtual SetSQLCol
        protected virtual void SetSQLCol(string[] pCol)
        {
            if (ArrFunc.IsFilled(pCol))
                _sqlCol = SQLCst.SELECT + ArrFunc.GetStringList(new ArrayList(pCol));
        }
        #endregion
        #region protected virtual SetSQLFrom
        protected virtual void SetSQLFrom()
        {
            _sqlFrom = SQLCst.FROM_DBO + SQLObject;
        }
        #endregion virtual SetSQLFrom
        #region protected virtual SetSQLWhere
        protected virtual void SetSQLWhere()
        {
        }
        protected virtual void SetSQLWhere(string pSQLWhere)
        {
        }
        #endregion SetSQLWhere
        #region protected virtual SetSQLOrderBy
        protected virtual void SetSQLOrderBy()
        {
        }
        #endregion SetSQLOrderBy
        #region protected virtual SetPrimaryKey
        protected virtual void SetPrimaryKey()
        {

        }
        #endregion SetPrimaryKey
        //
        #region protected AddSQLCol
        protected void AddSQLCol(string pCol)
        {
            if (StrFunc.IsFilled(pCol))
            {
                if (StrFunc.IsFilled(_sqlCol))
                    _sqlCol += ",";
                _sqlCol += pCol;
            }
        }
        #endregion virtual AddSQLCol
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAddFrom"></param>
        protected void ConstructFrom(string pAddFrom)
        {
            if (StrFunc.IsFilled(pAddFrom))
            {
                if (StrFunc.IsEmpty(SQLFrom))
                    SQLFrom = SQLCst.FROM_DBO + SQLObject;
                //
                if (!SQLFrom.EndsWith(Cst.CrLf))
                    SQLFrom += Cst.CrLf;
                SQLFrom += pAddFrom;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClause"></param>
        protected void ConstructWhere(string pClause)
        {
            if (StrFunc.IsFilled(pClause))
            {
                if (StrFunc.IsEmpty(SQLWhere))
                    SQLWhere = SQLCst.WHERE + "(" + pClause.Replace(SQLCst.WHERE, string.Empty) + ")" + Cst.CrLf;
                else
                    SQLWhere += SQLCst.AND + "(" + pClause + ")" + Cst.CrLf;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLOrderBy"></param>
        protected void ConstructOrderBy(string pSQLOrderBy)
        {
            if (StrFunc.IsFilled(pSQLOrderBy))
            {
                if (StrFunc.IsEmpty(SQLOrderBy))
                    SQLOrderBy = SQLCst.ORDERBY + pSQLOrderBy.Replace(SQLCst.ORDERBY, string.Empty);
                else
                    SQLOrderBy += "," + pSQLOrderBy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLWhereOrSqlColumnExpression"></param>
        /// <param name="pIsColum"></param>
        protected void InitSQLWhere(string pSQLWhereOrSqlColumnExpression, bool pIsColum)
        {
            if (pIsColum)
                SQLWhere = "(" + this.SQLObject + "." + pSQLWhereOrSqlColumnExpression + ")";
            else
                SQLWhere = pSQLWhereOrSqlColumnExpression;
        }


        /// <summary>
        /// Obtient true si la liste de colonnes {pCol} contient uniquement un item dont la valeur est "1"
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        protected bool IsQueryFindOnly(string[] pCol)
        {
            return (ArrFunc.Count(pCol) == 1 && pCol[0] == "1");
        }

        /// <summary>
        /// Obtient true si la liste de colonnes {pCol} contient uniquement un item dont la valeur est "*"
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        protected bool IsQuerySelectAllColumns(string[] pCol)
        {
            return (ArrFunc.Count(pCol) == 1 && pCol[0] == "*");
        }

        /// <summary>
        /// 
        /// </summary>
        protected void InitProperty()
        {
            _dt = null;
            _isExecutedScalar = false;
            _isExecutedColumn = false;
            _isFound = false;
            _isLoaded = false;
        }

        /// <summary>
        /// Applique une valeur à un paramètre de la requête
        /// <para>Ajoute le paramètre s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParameter"></param>
        /// <param name="pValue"></param>
        protected void SetDataParameter(DataParameter pParameter, object pValue)
        {
            if (false == _dataparameters.Contains(pParameter.ParameterKey))
                _dataparameters.Add(pParameter, pValue);
            else
                _dataparameters[pParameter.ParameterKey].Value = pValue;
        }

        /// <summary>
        /// Chgt des donnée via un select * 
        /// </summary>
        public bool LoadTable()
        {
            return LoadTable(new string[1] { "*" });
        }

        /// <summary>
        /// Chgt des donnée avec un choix de colonne
        /// </summary>
        /// FI 20131223 [19337] Appel à SetDataTable
        /// FI 20180316 [23769] Modify
        public bool LoadTable(string[] pCol)
        {
            bool isFindOnly = IsQueryFindOnly(pCol);
            QueryParameters queryParam = GetQueryParameters(pCol);
            if (isFindOnly)
            {
                object obj;
                try
                {
                    // FI 20180316 [23769] Les requêtes exécutées sous une transaction peuvent rentrer dans le cache SQL 
                    obj = DataHelper.ExecuteScalar(_csManager.CsSpheres, _dbTransaction, CommandType.Text, queryParam.Query, queryParam.Parameters.GetArrayDbParameter());

                    _isExecutedScalar = true;
                    _isExecutedColumn = false;
                    _isLoaded = false;
                    _isFound = (null != obj);
                }
                catch (Exception e)
                {
                    string query = queryParam.TransformQuery().ToString();
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, query, e);
                }
            }
            else
            {

                try
                {
                    //20090618 PL TEST GetSelectTop()
                    //NB: Dans le cas où l'on souhaite obtenir un seul record (ex. saisie d'un titre sur un trade), il est inutile de remonter plusieurs record...
                    //PL 20091221 Déplace du MaxRows dans queryParam.Query
                    //if (MaxRows > 1)
                    //    query = DataHelper.GetSelectTop(_csManager.Cs, query, MaxRows); 
                    // FI 20180316 [23769] Les requêtes exécutées sous une transaction peuvent rentrer dans le cache SQL 
                    DataTable dt = DataHelper.ExecuteDataTable(_csManager.CsSpheres, _dbTransaction, queryParam.Query, queryParam.Parameters.GetArrayDbParameter());
                    // FI 20131223 [19337] Appel à la méthode SetDataTable
                    SetDataTable(dt);
                }
                catch (Exception e)
                {
                    string query = queryParam.TransformQuery().ToString();
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, query, e);
                }
            }
            return _isFound;

        }

        /// <summary>
        /// Retourne une nouvelle instance de QueryParameters, les colonnes de la requête de chgt sont définies via {pCol}
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        public virtual QueryParameters GetQueryParameters(string[] pCol)
        {
            return new QueryParameters(CS, GetQuery(pCol), Dataparameters);
        }

        /// <summary>
        /// Retourne la valeur de la colonne {pColumnName} de la ligne {pRow} issu du jeu de résultat
        /// <para>Obtient null, si le jeu de résultat est non chargé</para>
        /// <para>Obtient null, si la ligne n'existe pas</para>
        /// <para>Obtient null, si la cononne a pour valeur DBNull</para>
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        public object GetColumnValue(int pRow, string pColumnName)
        {
            object ret = null;
            DataRow dr = null;
            //	
            if (IsLoaded)
            {
                if (null != Dt)
                    dr = Dt.Rows[pRow];
            }
            //
            if (null != dr)
            {
                ret = Dt.Rows[pRow][pColumnName];
                if (Convert.IsDBNull(ret))
                    ret = null;
            }
            //
            return ret;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        /// EG 20171115 [23509] New
        /// FI 20190404 [XXXXX] Refactoring (Contexte Recette BANCAPERTA 8.1)
        public Nullable<DateTime> GetNullableDateTimeValue(string pColumnName)
        {
            // FI 20190404 [XXXXX]
            /*
            object ret = GetFirstRowColumnValue(pColumnName);
            return ret == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(ret);
             */

            Nullable<DateTime> ret = null;
            object colValue = GetFirstRowColumnValue(pColumnName);
            if (null != colValue)
                ret = (DateTime?)Convert.ToDateTime(colValue);
            return ret;
        }

        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public Nullable<int> GetNullableIntValue(string pColumnName)
        {
            Nullable<int> ret = null;
            object colValue = GetFirstRowColumnValue(pColumnName);
            if (null != colValue)
                ret = Convert.ToInt32(colValue);
            return ret;
        }
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public Nullable<decimal> GetNullableDecimalValue(string pColumnName)
        {
            Nullable<decimal> ret = null;
            object colValue = GetFirstRowColumnValue(pColumnName);
            if (null != colValue)
                ret = Convert.ToDecimal(colValue);
            return ret;
        }

        /// <summary>
        /// Obtient la valeur de la colonne {pColumnName} de la 1ère ligne issue du jeu de résultat
        /// <para>Obtient null, si le jeu de résultat est non chargé</para>
        /// <para>Obtient null, si la colonne a pour valeur DBNull</para>
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        public object GetFirstRowColumnValue(string pColumnName)
        {
            return GetColumnValue(0, pColumnName);
        }

        //PL 20111228 Newness
        public Cst.CheckModeEnum GetFirstRowColumnCheckMode(string pColumnName)
        {
            Cst.CheckModeEnum ret = Cst.CheckModeEnum.None;
            object obj = GetFirstRowColumnValue(pColumnName);
            if ((obj != null) && (Enum.IsDefined(typeof(Cst.CheckModeEnum), obj)))
                ret = (Cst.CheckModeEnum)Enum.Parse(typeof(Cst.CheckModeEnum), Convert.ToString(obj), true);

            return ret;
        }
        
        /// <summary>
        /// Obtient la valeur de la colonne ROWVERSION 
        /// </summary>
        public string RowVersion
        {
            get { return Convert.ToString(GetFirstRowColumnValue(SQLObject + "_" + "ROWVERSION")); }
        }

        /// <summary>
        /// Obtient la valeur de la colonne ROWATTRIBUT 
        /// </summary>
        public string RowAttribut
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ROWATTRIBUT")); }
        }

        /// <summary>
        /// Obtient true si la 1er ligne du jeu de résultat est Enabled en prenant en considération DTSYS
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        public bool IsEnabled
        {
            get
            {
                bool ret = true;
                // FI 20191209 [XXXXX] Economies en requêtes SQL. L'appel à OTCmlHelper.GetDateSys n'est effectué que si nécessaire 
                //DateTime dtCurrent = OTCmlHelper.GetDateSys(_csManager.Cs, dbTransaction);
                DateTime dtEnabled = Convert.ToDateTime(GetFirstRowColumnValue("DTENABLED"));
                DateTime dtDisabled = Convert.ToDateTime(GetFirstRowColumnValue("DTDISABLED"));
                if (DtFunc.IsDateTimeFilled(dtEnabled))
                {
                    DateTime dtCurrent = OTCmlHelper.GetDateSys(_csManager.Cs);
                    ret = (DateTime.Compare(dtEnabled, dtCurrent) <= 0);
                    if (DtFunc.IsDateTimeFilled(dtDisabled))
                        ret &= (DateTime.Compare(dtCurrent, dtDisabled) <= 0);
                }
                return ret;
            }
        }

        /// <summary>
        /// Ajoute la colonne ROWVERSION ds la liste des colonnes de chgt
        /// </summary>
        private void AddSQLRowVersionCol()
        {
            if (StrFunc.IsFilled(_sqlCol))
                _sqlCol += ",";
            _sqlCol += DataHelper.GetROWVERSION(CS, SQLObject, SQLObject + "_" + "ROWVERSION");
        }


        /// <summary>
        /// Retourne la query qui sera exécutée
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        private string GetQuery(string[] pCol)
        {
            SetSQLCol(pCol);
            
            // Ne pas generer RowVersion si select * 
            // car plantage Oracle => select *,alias.column non autorisé   
            if (false == IsQuerySelectAllColumns(pCol) && (_isAddRowVersion))
                AddSQLRowVersionCol();
            
            SetSQLFrom();
            SetSQLWhere();
            SetSQLOrderBy();

            string sqlSelect = $"{SQLCol}{Cst.CrLf}{SQLFrom}{Cst.CrLf}";
            
            string where = SQLWhere + Cst.CrLf;
            if (ScanDataDtEnabled == ScanDataDtEnabledEnum.Yes)
                where += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(CS, SQLObject, DtRefForDtEnabled) + Cst.CrLf;
            sqlSelect += AddRestrictScanTemplate(where);
            
            sqlSelect += SQLOrderBy;

            if (MaxRows >= 1)
                sqlSelect = DataHelper.GetSelectTop(CS, sqlSelect, MaxRows);
            
            return sqlSelect;
        }

        /// <summary>
        ///  Ajoute une restriction where {SQLObject}.ISTEMPLATE=1 ou {SQLObject}.ISTEMPLATE=0
        ///  <para>Restriction ajouté uniquement si l'objet contient une colonne ISTEMPLATE et si ScanTemplate est != null </para>
        /// </summary>
        /// <param name="pWhere"></param>
        /// <returns></returns>
        /// FI 20130423 [18620]
        private string AddRestrictScanTemplate(string pWhere)
        {
            string ret = pWhere;
            if (ScanTemplate != null)
            {
                switch (ScanTemplate.Value)
                {
                    case ScanDataTemplate.Yes:
                        ret += SQLCst.AND + StrFunc.AppendFormat("{0}.ISTEMPLATE=1", SQLObject);
                        break;
                    case ScanDataTemplate.No:
                        ret += SQLCst.AND + StrFunc.AppendFormat("{0}.ISTEMPLATE=0", SQLObject);
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Affecte le membre _dt
        /// </summary>
        /// <param name="pDt"></param>
        /// FI 20131223 [19337] add Method
        private void SetDataTable(DataTable pDt)
        {
            _dt = pDt;
            _isExecutedScalar = false;
            _isExecutedColumn = true;
            _isFound = (0 < ArrFunc.Count(_dt.Rows));
            _isLoaded = _isFound;
            //
            SetPrimaryKey();
        }

    }
    #endregion SQL_Table

    #region SQL_Country
    public class SQL_Country : SQL_Table
    {
        #region public IDType
        public enum IDType
        {
            IdCountry,
            Iso3166Alpha2,
        }
        #endregion public IDType

        #region private Members
        private string _iso3166Alpha2_In;
        private string _idCountry_In;
        private readonly IDType _idType;
        #endregion private Members

        #region Constructors
        public SQL_Country(string pSource, string pId)
            : this(pSource, IDType.IdCountry, pId, ScanDataDtEnabledEnum.No) { }
        public SQL_Country(string pSource, IDType pIdType, string pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, null, Cst.OTCml_TBL.COUNTRY.ToString(), pScanDataEnabled)
        {

            //
            _idType = pIdType;
            switch (_idType)
            {
                case IDType.IdCountry:
                    IdCountry_In = pId;
                    break;
                case IDType.Iso3166Alpha2:
                    Iso3166Alpha2_In = pId;
                    break;
            }
        }
        #endregion Constructors

        #region public_property_get_set
        public string Iso3166Alpha2_In
        {
            get
            {
                return _iso3166Alpha2_In;
            }
            set
            {
                if (_iso3166Alpha2_In != value)
                {
                    InitProperty(true);
                    _iso3166Alpha2_In = value;
                }
            }
        }
        public string IdCountry_In
        {
            get
            {
                return _idCountry_In;
            }
            set
            {
                if (_idCountry_In != value)
                {
                    InitProperty(true);
                    _idCountry_In = value;
                }
            }
        }
        #endregion
        #region public_property_get
        /// <summary>
        /// 
        /// </summary>
        public string IDCOUNTRY
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCOUNTRY")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IDC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME")); }
        }

        #endregion public_property_get

        #region protected override SetSQLWhere
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            //
            switch (_idType)
            {
                case IDType.IdCountry:
                    sqlWhere.Append(SQLObject + ".IDCOUNTRY" + SQLCst.LIKE + "@IDCOUNTRY");
                    SetDataParameter(new DataParameter(CS, "IDCOUNTRY", DbType.AnsiString, 64), _idCountry_In);
                    break;
                case IDType.Iso3166Alpha2:
                    sqlWhere.Append(SQLObject + ".ISO3166_ALPHA2" + SQLCst.LIKE + "@ISO3166_ALPHA2");
                    SetDataParameter(new DataParameter(CS, "ISO3166_ALPHA2", DbType.AnsiString, 2), _iso3166Alpha2_In);
                    break;
            }
            //
            InitSQLWhere(sqlWhere.ToString(), false);
            //
        }
        #endregion

        #region private method
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _iso3166Alpha2_In = string.Empty;
            }
            base.InitProperty();
        }
        #endregion
    }
    #endregion SQL_Country

    #region SQL_Currency
    /// <summary>
    /// 
    /// </summary>
    //PM 20140507 [19970][19259] Ajout de l'accesseur IdCQuoted
    public class SQL_Currency : SQL_Table
    {
        #region public IDType
        public enum IDType
        {
            IdC,
            Iso4217,
        }
        #endregion public IDType

        #region private Members
        private string _iso4217_In = string.Empty;
        private string _idC_In = string.Empty;
        private readonly IDType _idType;
        #endregion private Members

        #region constructors
        public SQL_Currency(string pSource, string pIso4217)
            : this(pSource, IDType.Iso4217, pIso4217, ScanDataDtEnabledEnum.No) { }
        public SQL_Currency(string pSource, IDType pIdType, string pId)
            : this(pSource, pIdType, pId, ScanDataDtEnabledEnum.No) { }
        public SQL_Currency(string pSource, IDType pIdType, string pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, null, Cst.OTCml_TBL.CURRENCY.ToString(), pScanDataEnabled)
        {
            _idType = pIdType;
            switch (_idType)
            {
                case IDType.IdC:
                    IdC_In = pId;
                    break;
                case IDType.Iso4217:
                    Iso4217_In = pId;
                    break;
            }

            if (IsUseOrderBy)
            {
                //20090724 FI SetCacheOn
                base.EfsObject = new SQL_EfsObject(CSTools.SetCacheOn(_csManager.CsSpheres), this.SQLObject);
                base.EfsObject.LoadTable(new string[] { "ISWITHSTATISTIC" });
            }
        }
        #endregion constructors

        #region private property Get
        private bool IsUseOrderBy
        {
            get
            {
                //Critère sur la PK ou
                //Critère sur Identifier ou ExtLink sans "%" 
                //--> La query retournera 1 ou 0 record  
                bool ret = false;
                switch (_idType)
                {
                    case IDType.IdC:
                        ret = StrFunc.ContainsIn(IdC_In, "%");
                        break;
                    case IDType.Iso4217:
                        ret = StrFunc.ContainsIn(Iso4217_In, "%");
                        break;
                }
                return ret;
            }
        }
        #endregion private property Get

        #region public_property_get

        public string IdC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDC")); }
        }
        public string IdBC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBC")); }
        }
        public string IdBC2
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBC2")); }
        }
        public string IdBC3
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBC3")); }
        }
        public string Iso4217
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISO4217_ALPHA3")); }
        }
        public string Iso4217_Num3
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ISO4217_NUM3")); }
        }
        public string Symbol
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SYMBOL")); }
        }
        public string SymbolAlign
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SYMBOLALIGN")); }
        }
        public string DisplayName
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME")); }
        }
        public string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DESCRIPTION")); }
        }
        public string DayCountFraction
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DCF")); }
        }
        public DayCountFractionEnum FpML_Enum_DayCountFraction
        {
            get { return StringToEnum.DayCountFraction(DayCountFraction); }
        }

        public bool IsEmergingMarket
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISEMERGINGMARKET")); }
        }

        public int Factor
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("FACTOR")); }
        }
        public int RoundPrec
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("ROUNDPREC")); }
        }
        public string RoundDir
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ROUNDDIR")); }
        }
        public Cst.RoundingDirectionSQL RoundingDirectionSQL
        {
            get
            {
                if (IsLoaded)
                    return (Cst.RoundingDirectionSQL)Enum.Parse(typeof(Cst.RoundingDirectionSQL), Convert.ToString(Dt.Rows[0]["ROUNDDIR"]));
                else
                    return Cst.RoundingDirectionSQL.N;
            }
        }
        public RoundingDirectionEnum RoundingDirectionFpML
        {
            get
            {
                RoundingDirectionEnum roundingDirection = RoundingDirectionEnum.Nearest;
                if (IsLoaded)
                {
                    switch (this.RoundingDirectionSQL)
                    {
                        case Cst.RoundingDirectionSQL.D:
                            roundingDirection = RoundingDirectionEnum.Down;
                            break;
                        case Cst.RoundingDirectionSQL.N:
                            roundingDirection = RoundingDirectionEnum.Nearest;
                            break;
                        case Cst.RoundingDirectionSQL.U:
                            roundingDirection = RoundingDirectionEnum.Up;
                            break;
                    }
                }
                return roundingDirection;
            }
        }
        //PM 20140507 [19970][19259] Ajout de l'accesseur IdCQuoted
        public string IdCQuoted
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDCQUOTED")); }
        }
        #endregion public_property_get
        #region public_property_get_set
        public string Iso4217_In
        {
            get
            {
                return _iso4217_In;
            }
            set
            {
                if (_iso4217_In != value)
                {
                    InitProperty(true);
                    _iso4217_In = value;
                }
            }
        }
        public string IdC_In
        {
            get
            {
                return _idC_In;
            }
            set
            {
                if (_idC_In != value)
                {
                    InitProperty(true);
                    _idC_In = value;
                }
            }
        }
        #endregion

        #region private method
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _iso4217_In = string.Empty;
            }
            base.InitProperty();
        }
        #endregion

        #region protected override Method
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            //
            string sqlFrom = string.Empty;
            //Statistics: table CURRENCY_S
            if (IsUseOrderBy && EfsObject.IsWithStatistic)
                sqlFrom += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(SQLObject, SQLObject);
            //
            ConstructFrom(sqlFrom);
        }

        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            //
            switch (_idType)
            {
                case IDType.IdC:
                    sqlWhere.Append(SQLObject + ".IDC" + SQLCst.LIKE + "@IDC");
                    SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDC), _idC_In);
                    break;
                case IDType.Iso4217:
                    sqlWhere.Append(SQLObject + ".ISO4217_ALPHA3" + SQLCst.LIKE + "@ISO4217_ALPHA3");
                    SetDataParameter(new DataParameter(CS, "ISO4217_ALPHA3", DbType.AnsiString, SQLCst.UT_CURR_LEN), _iso4217_In);
                    break;
            }
            //
            InitSQLWhere(sqlWhere.ToString(), false);
            //
        }
        protected override void SetSQLOrderBy()
        {
            string sColumOrderBy = string.Empty;

            if (IsUseOrderBy)
            {
                //Statistics: table CURRENCY_S
                if (EfsObject.IsWithStatistic)
                    sColumOrderBy = OTCmlHelper.GetSQLOrderBy_Statistic(CS, this.SQLObject);
            }
            //
            ConstructOrderBy(sColumOrderBy);
        }
        #endregion
    }
    #endregion SQL_Currency

    #region SQL_EfsObject
    public class SQL_EfsObject : SQL_Table
    {
        #region private variable
        private string _objectName_In = string.Empty;
        #endregion private variable

        #region constructors
        /// <summary>
        /// Scan by IDA
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIsScanDataEnabled"></param>

        public SQL_EfsObject(string pSource, string pObjectName)
            : base(pSource, Cst.OTCml_TBL.EFSOBJECT.ToString())
        {
            //			base.Cs = pSource;
            //			base.SQLObject = Cst.OTCml_TBL.EFSOBJECT.ToString();  
            //			base.ScanDataDtEnabled  = ScanDataDtEnabledEnum.No; 
            //
            this.ObjectNameIn = pObjectName;
        }
        #endregion constructors

        #region public_property_get

        public string ObjectName
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["OBJECTNAME"]);
                else
                    return null;
            }
        }

        public string ObjectType
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["OBJECTTYPE"]);
                else
                    return null;
            }
        }
        public int ObjectOrder
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["OBJECTORDER"]);
                else
                    return 0;
            }
        }
        public bool IsWithHistoric
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToBoolean(Dt.Rows[0]["ISWITHHISTORIC"]);
                else
                    return false;
            }
        }

        public bool IsWithStatistic
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToBoolean(Dt.Rows[0]["ISWITHSTATISTIC"]);
                else
                    return false;
            }
        }


        #endregion public_property_get

        #region public_property_get_set
        public string ObjectNameIn
        {
            get
            { return _objectName_In; }
            set
            {
                if (_objectName_In != value)
                {
                    InitProperty(true);
                    _objectName_In = value;
                }
            }
        }
        #endregion

        #region private methods
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _objectName_In = string.Empty;
            }
            base.InitProperty();
        }
        #endregion private_methods

        #region override method
        protected override void SetSQLWhere()
        {
            if (StrFunc.IsFilled(ObjectNameIn))
                InitSQLWhere(DataHelper.SQLUpper(CS, this.SQLObject + "." + "OBJECTNAME") + " = " + DataHelper.SQLString(ObjectNameIn.ToString(), CompareEnum.Upper), false);
            else
                InitSQLWhere(string.Empty, false);
        }
        #endregion
    }
    #endregion SQL_EfsObject

    #region SQL_LastTrade_L
    /// <summary>
    /// Lecture de TRADETRAIL
    /// </summary>
    public class SQL_LastTrade_L : SQL_Table
    {
        #region private variable
        private int _idT_In;
        private readonly Permission.PermissionEnum[] _permission_In;
        #endregion private variable

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPermission">Permet de réduire la liste des actions prise en compte (valeur null possible)</param>
        public SQL_LastTrade_L(string pCs, int pIdT, PermissionEnum[] pPermission)
            : base(pCs, Cst.OTCml_TBL.TRADETRAIL.ToString())
        {
            IdT_In = pIdT;
            _permission_In = pPermission;
        }
        #endregion constructors

        #region public_property_get
        public int Id
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["IDT_L"]);
                else
                    return 0;
            }
        }
        public string ScreenName
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToString(Dt.Rows[0]["SCREENNAME"]);
                else
                    return null;
            }
        }
        public int IdA
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["IDA"]);
                else
                    return 0;
            }
        }
        public DateTime DtSys
        {
            get
            {
                DateTime dt = DateTime.MinValue;
                //	
                if (IsLoaded)
                {
                    // FI 20200820 [25468] Dates systemes en UTC
                    dt = DateTime.SpecifyKind(Convert.ToDateTime(Dt.Rows[0]["DTSYS"]), DateTimeKind.Utc);
                }
                return dt;
            }
        }
        /// <summary>
        /// Obtient l'action
        /// </summary>
        public string Action
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["ACTION"]);
                return ret;
            }
        }
        /// <summary>
        /// Obtient l'Id du process générateur du trade
        /// </summary>
        public int IdProcessL
        {
            get
            {
                return Convert.ToInt32(this.GetFirstRowColumnValue("IDPROCESS_L"));
            }
        }
        #endregion public_property_get

        #region public_property_get_set
        public int IdT_In
        {
            get { return _idT_In; }
            set
            {
                if (_idT_In != value)
                {
                    InitProperty(true);
                    _idT_In = value;
                }
            }
        }
        #endregion

        #region private method
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idT_In = 0;
            }
            base.InitProperty();
        }
        #endregion

        #region protected override Method
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();

            //FI 20121113 [18224] Use parameter
            sqlWhere.Append(SQLObject + ".IDT=@IDT");
            SetDataParameter(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDT), IdT_In);

            if (ArrFunc.IsFilled(_permission_In))
                sqlWhere.Append(DataHelper.SQLColumnIn(CS, SQLObject + ".ACTION", _permission_In, TypeData.TypeDataEnum.@string));

            InitSQLWhere(sqlWhere.ToString(), false);

        }
        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLOrderBy()
        {
            ConstructOrderBy("IDT_L" + SQLCst.DESC);
        }

        #endregion
    }
    #endregion SQL_Currency

    #region SQL_IssiItem
    public class SQL_IssiItem : SQL_Table
    {
        #region members
        private int _idIssi_In;
        #endregion

        #region constructors
        public SQL_IssiItem(string pSource, int pIdIssi)
            : this(pSource, pIdIssi, ScanDataDtEnabledEnum.No) { }
        public SQL_IssiItem(string pSource, int pIdIssi, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, null, Cst.OTCml_TBL.ISSIITEM.ToString(), pScanDataEnabled)
        {
            _idIssi_In = pIdIssi;
        }
        #endregion

        #region public_property_get_set
        public int IdIssi_In
        {
            get { return _idIssi_In; }
            set
            {
                if (_idIssi_In != value)
                {
                    InitProperty(true);
                    _idIssi_In = value;
                }
            }
        }
        #endregion

        #region public override Method
        protected override void SetSQLCol(string[] pCol)
        {
            StrBuilder colOrder = new StrBuilder();
            StrBuilder col = new StrBuilder();
            //
            base.SetSQLCol(pCol);
            //
            if (IsUseOrderBy)
            {
                #region COL_ORDER
                foreach (string s in Enum.GetNames(typeof(RoleActorSSI)))
                {

                    RoleActorSSI roleActor = (RoleActorSSI)Enum.Parse(typeof(RoleActorSSI), s);
                    col += " when IDROLEACTOR=" + DataHelper.SQLString(roleActor.ToString()) + " Then ";
                    //
                    switch (roleActor)
                    {
                        case RoleActorSSI.INVESTOR:
                            col += "10";
                            break;
                        case RoleActorSSI.ORIGINATOR:
                            col += "20";
                            break;
                        case RoleActorSSI.ACCOUNTOWNER:
                            col += "30";
                            break;
                        case RoleActorSSI.ACCOUNTSERVICER:
                            col += "40";
                            break;
                        case RoleActorSSI.INTERMEDIARY:
                            col += "51";
                            break;
                        case RoleActorSSI.CORRESPONDENT:
                            col += "52";
                            break;
                        case RoleActorSSI.CSS:
                            col += "60";
                            break;
                        default:
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Role " + roleActor.ToString() + " not managed");
                    }
                    //
                    col += Cst.CrLf;
                }
                //
                colOrder += new StrBuilder("case " + Cst.CrLf + col.ToString() + Cst.CrLf + "else 99 end as COL_ORDER");
                #endregion
            }
            //
            if (StrFunc.IsFilled(colOrder.ToString()))
                AddSQLCol(colOrder.ToString());
        }


        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            if (_idIssi_In != 0)
                sqlWhere.Append("IDISSI=" + _idIssi_In.ToString());
            InitSQLWhere(sqlWhere.ToString(), false);

        }

        protected override void SetSQLOrderBy()
        {
            string orderBy = string.Empty;
            if (IsUseOrderBy)
                orderBy = "COL_ORDER asc,SEQUENCENO asc";
            ConstructOrderBy(orderBy);

        }
        #endregion

        #region public property Get
        protected bool IsUseOrderBy
        {
            get { return (_idIssi_In != 0); }
        }

        //IDISSI/IDA/IDROLEACTOR/SEQUENCENO/CHAINPARTY/PARTYROLE/CACCOUNTNUMBER/CACCOUNTNUMBERIDENT                                              CACCOUNTNAME                                                     SACCOUNTNUMBER                                                   SACCOUNTNUMBERIDENT                                              SACCOUNTNAME    
        public int IdIssi
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDISSI")); }
        }
        public int IdA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        public int IdRoleActor
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDROLEACTOR")); }
        }
        #endregion public property Get

        #region private method
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idIssi_In = 0;
            }
            base.InitProperty();
        }
        #endregion
    }
    #endregion SQL_IssiItem

    #region SQL_EntityMarket
    /// <summary>
    /// 
    /// </summary>
    public class SQL_EntityMarket : SQL_Table
    {
        #region members
        private int _idEM_In;
        #endregion
        //
        #region public_property_get_set
        public int IdEM_In
        {
            get { return _idEM_In; }
            set
            {
                if (_idEM_In != value)
                {
                    InitProperty(true);
                    _idEM_In = value;
                }
            }
        }
        #endregion

        #region public property get
        public Boolean IsAutoABN
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISAUTOABN")); }
        }
        public Boolean IsShortCallCover
        {
            get { return BoolFunc.IsTrue(GetFirstRowColumnValue("ISSHORTCALLCOVER")); }
        }
        public DateTime DtMarket
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTMARKET")); }
        }
        public DateTime DtMarketPrev
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTMARKETPREV")); }
        }
        public DateTime DtMarketNext
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTMARKETNEXT")); }
        }
        /// <summary>
        /// Date de traitement courante pour l'entity
        /// </summary>
        //PM 20150422 [20575] Add dtEntity
        public DateTime DtEntity
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTENTITY")); }
        }
        /// <summary>
        /// Précédente date de traitement pour l'entity
        /// </summary>
        //PM 20150422 [20575] Add dtEntityPrev
        public DateTime DtEntityPrev
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTENTITYPREV")); }
        }
        /// <summary>
        /// Prochaine date de traitement pour l'entity
        /// </summary>
        //PM 20150422 [20575] Add dtEntityNext
        public DateTime DtEntityNext
        {
            get { return Convert.ToDateTime(GetFirstRowColumnValue("DTENTITYNEXT")); }
        }
        public int IdM
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDM")); }
        }
        public string Market_IDENTIFIER
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Market_IDENTIFIER")); }
        }
        public string IdBC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBC")); }
        }
        // EG 20240520 [WI930] New 
        public string Entity_IdBC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ENTITY_IDBC")); }
        }
        public int IdA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        public int IdA_CSS
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_CSS")); }
        }
        public Nullable<int> IdA_CUSTODIAN
        {
            get 
            {
                Nullable<int> _idA_Custodian = null;
                if (null != GetFirstRowColumnValue("IDA_CUSTODIAN"))
                    _idA_Custodian = Convert.ToInt32(GetFirstRowColumnValue("IDA_CUSTODIAN"));
                return _idA_Custodian; 
            }
        }
        public string Entity_IDENTIFIER
        {
            get { return Convert.ToString(GetFirstRowColumnValue("Entity_IDENTIFIER")); }
        }
        public int IdEM
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDEM")); }
        }
        #endregion
        //
        #region constructor
        public SQL_EntityMarket(string pCS)
            : base(pCS, null, Cst.OTCml_TBL.ENTITYMARKET.ToString(), ScanDataDtEnabledEnum.No)
        {
        }
        //


        public SQL_EntityMarket(string pCS, int pIdEM)
            : this(pCS, pIdEM, ScanDataDtEnabledEnum.No)
        {
        }
        public SQL_EntityMarket(string pCS, int pIdEM, ScanDataDtEnabledEnum pScanDataDtEnabled)
            : base(pCS, null, Cst.OTCml_TBL.ENTITYMARKET.ToString(), pScanDataDtEnabled)
        {
            _idEM_In = pIdEM;
        }
        //
        // EG 20180307 [23769] Gestion dbTransaction
        public SQL_EntityMarket(string pCS, IDbTransaction pDbTransaction, int pIdEntity, int pIdMarket, Nullable<int> pIdCustodian)
            : this(pCS, pDbTransaction, pIdEntity, pIdMarket, pIdCustodian, ScanDataDtEnabledEnum.No)
        {
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public SQL_EntityMarket(string pCS, IDbTransaction pDbTransaction, int pIdEntity, int pIdMarket, Nullable<int> pIdCustodian, ScanDataDtEnabledEnum pScanDataDtEnabled)
            : base(pCS, pDbTransaction, Cst.OTCml_TBL.ENTITYMARKET.ToString(), pScanDataDtEnabled)
        {
            SetIdEM(pIdEntity, pIdMarket, pIdCustodian);
        }
        #endregion
        //
        #region private method
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _idEM_In = 0;
            }
            base.InitProperty();
        }
        #endregion
        //
        #region protected override SetSQLCol
        protected override void SetSQLCol(string[] pCol)
        {
            if (IsQueryFindOnly(pCol))
            {
                base.SetSQLCol(pCol);
            }
            else
            {
                if (IsQuerySelectAllColumns(pCol))
                    pCol[0] = SQLObject + "." + "*";  // Pour Oracle Necessaire permet d'obtenir select ACTOR.*, a.IDENTIFIER  as  ACTOR_IDENTIFIER etc 
                //
                base.SetSQLCol(pCol);
                //
                AddSQLCol("ety.IDENTIFIER as Entity_IDENTIFIER");
                AddSQLCol("mk.IDENTIFIER as Market_IDENTIFIER");
                AddSQLCol("mk.IDA as IDA_CSS");
                AddSQLCol("mk.IDBC as IDBC");
                // EG 20240520 [WI930] New
                AddSQLCol("ety.IDBC as Entity_IDBC");
            }
        }
        #endregion
        #region protected override SetSQLFrom
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();
            string sqlFrom = SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET + " mk " + SQLCst.ON + "(mk.IDM = " + SQLObject + ".IDM)" + Cst.CrLf;
            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR + " ety " + SQLCst.ON + "(ety.IDA = " + SQLObject + ".IDA)" + Cst.CrLf;
            ConstructFrom(sqlFrom);
        }
        #endregion

        #region protected override SetSQLWhere
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            //
            if (_idEM_In != 0)
                sqlWhere.Append("IDEM=" + _idEM_In.ToString());
            //
            InitSQLWhere(sqlWhere.ToString(), false);

        }
        #endregion
        //
        #region public SetIdEM
        /// <summary>
        /// Alimente IdEm en fonction de l'entity et du marché  
        /// </summary>
        /// <param name="pIdEntity"></param>
        /// <param name="pIdMarket"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public void SetIdEM(int pIdEntity, int pIdMarket, Nullable<int> pIdCustodian)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDA", DbType.Int32), pIdEntity);
            dp.Add(new DataParameter(CS, "IDM", DbType.Int32), pIdMarket);
            if (pIdCustodian.HasValue)
                dp.Add(new DataParameter(CS, "IDA_CUSTODIAN", DbType.Int32), pIdCustodian);

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "em.IDEM" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENTITYMARKET.ToString() + " em" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(em.IDM=@IDM) and (em.IDA=@IDA) and";
            sqlSelect += pIdCustodian.HasValue?"(em.IDA_CUSTODIAN=@IDA_CUSTODIAN)":"(em.IDA_CUSTODIAN is null)";

            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect.ToString(), dp);
            object result = DataHelper.ExecuteScalar(CS, DbTransaction, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter());
            if (null != result)
                IdEM_In = Convert.ToInt32(result);

        }
        #endregion
    }
    #endregion

    #region SQL_TradeLink
    // EG 20091202 Add Accessor SQLFrom
    // EG 20120621 Add GProduct for IDT_A and IDT_B
    public class SQL_TradeLink : SQL_Table
    {
        #region Members
        private readonly int m_IdT;
        private string _gProduct_In;
        #endregion Members
        #region Accessors
        //
        public int IdT
        {
            get
            {
                int idT = 0;
                if (IsLoaded)
                    idT = Convert.ToInt32(GetFirstRowColumnValue("IDT"));
                return idT;
            }
        }
        public int IdA
        {
            get
            {
                if (IsLoaded)
                    return Convert.ToInt32(Dt.Rows[0]["IDA"]);
                else
                    return 0;
            }
        }
        public DateTime DtSys
        {
            get
            {
                DateTime dt = DateTime.MinValue;
                //	
                if (IsLoaded)
                {
                    // FI 20200820 [25468] Dates systemes en UTC
                    dt = DateTime.SpecifyKind(Convert.ToDateTime(Dt.Rows[0]["DTSYS"]), DateTimeKind.Utc);
                }
                return dt;
            }
        }
        public string ActionForIdT_A
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["ACTION_A"]);
                return ret;
            }
        }
        public string Identifier_A
        {
            get
            {
                string identifier = string.Empty;
                if (IsLoaded)
                    identifier = Convert.ToString(Dt.Rows[0]["IDENTIFIER_A"]);
                return identifier;
            }
        }
        public string Identifier_B
        {
            get
            {
                string identifier = string.Empty;
                if (IsLoaded)
                    identifier = Convert.ToString(Dt.Rows[0]["IDENTIFIER_B"]);
                return identifier;
            }
        }
        // EG 20120621 Add GProduct for IDT_A and IDT_B
        public string GProduct_A
        {
            get
            {
                string gProduct = string.Empty;
                if (IsLoaded)
                    gProduct = Convert.ToString(Dt.Rows[0]["GPRODUCT_A"]);
                return gProduct;
            }
        }
        public string GProduct_B
        {
            get
            {
                string gProduct = string.Empty;
                if (IsLoaded)
                    gProduct = Convert.ToString(Dt.Rows[0]["GPRODUCT_B"]);
                return gProduct;
            }
        }
        public int IdT_A
        {
            get
            {
                int idT_A = 0;
                if (IsLoaded)
                    idT_A = Convert.ToInt32(GetFirstRowColumnValue("IDT_A"));
                return idT_A;
            }
        }
        public int IdT_B
        {
            get
            {
                int idT_B = 0;
                if (IsLoaded)
                    idT_B = Convert.ToInt32(GetFirstRowColumnValue("IDT_B"));
                return idT_B;
            }
        }
        public string Link
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["LINK"]);
                return ret;
            }
        }
        public string Message
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["MESSAGE"]);
                return ret;
            }
        }
        public string Data1
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["DATA1"]);
                return ret;
            }
        }
        public string Data2
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["DATA2"]);
                return ret;
            }
        }
        // EG 20091208 New
        public string Data3
        {
            get
            {
                string ret = string.Empty;
                if (IsLoaded)
                    ret = Convert.ToString(Dt.Rows[0]["DATA3"]);
                return ret;
            }
        }
        // EG 20120621 Add GProduct for IDT_A and IDT_B
        // EG 20160404 Migration vs2013
        public new string SQLFrom
        {
            get
            {
                string sqlFrom = Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADETRAIL + " tl ";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(tl.IDT_L = " + SQLObject + ".IDT_L)";
                sqlFrom += Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " t_A";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(t_A.IDT = " + SQLObject + ".IDT_A)";
                sqlFrom += Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i_A";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(i_A.IDI = t_A.IDI)";
                sqlFrom += Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p_A";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(p_A.IDP = i_A.IDP)";
                sqlFrom += Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " t_B";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(t_B.IDT = " + SQLObject + ".IDT_B)";
                sqlFrom += Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i_B";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(i_B.IDI = t_B.IDI)";
                sqlFrom += Cst.CrLf + SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p_B";
                sqlFrom += Cst.CrLf + SQLCst.ON + "(p_B.IDP = i_B.IDP)";

                return sqlFrom;
            }
        }
        // RD 20091231 [16814] Modification of Trade included in Invoice
        // EG 20150716 Gestion IDSTACTIVATION = REGULAR|SIMUL pour IDT_A (facture)
        public bool IsInvoiced
        {
            get
            {
                if (IsLoaded)
                {
                    foreach (DataRow dr in Dt.Rows)
                    {
                        if ((dr["LINK"].ToString() == TradeLink.TradeLinkType.Invoice.ToString()) &&
                            (dr["IDSTACTIVATION_A"].ToString() != Cst.StatusActivation.DEACTIV.ToString()))
                            return true;
                    }
                }
                //
                return false;
            }
        }
        // EG 20120621 Add GProduct for IDT_A and IDT_B
        public string GProduct_In
        {
            get
            {
                return _gProduct_In;
            }
            set
            {
                if (_gProduct_In != value)
                {
                    InitProperty(true);
                    _gProduct_In = value;
                }
            }
        }
        #endregion Accessors

        #region Constructor
        // EG 20120621 Add GProduct for IDT_A and IDT_B
        // PM 20150311 [POC] Replace TRADELINK with VW_TRADELINK
        public SQL_TradeLink(string pCS, int pIdT)
            //: base(pCS, Cst.OTCml_TBL.TRADELINK.ToString())
            : base(pCS, Cst.OTCml_TBL.VW_TRADELINK.ToString())
        {
            m_IdT = pIdT;
        }
        // PM 20150311 [POC] Replace TRADELINK with VW_TRADELINK
        public SQL_TradeLink(string pCS, int pIdT, string pGProduct)
            //: base(pCS, Cst.OTCml_TBL.TRADELINK.ToString())
            : base(pCS, Cst.OTCml_TBL.VW_TRADELINK.ToString())
        {
            m_IdT = pIdT;
            _gProduct_In = pGProduct;
        }
        #endregion Constructor

        #region Methods
        private void InitProperty(bool pAll)
        {
            if (pAll)
            {
                _gProduct_In = string.Empty;
            }
            base.InitProperty();
        }
        // EG 20120621 Add GProduct for IDT_A and IDT_B
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append("(" + SQLObject + ".IDT_A=@IDTLINK" + SQLCst.OR + SQLObject + ".IDT_B=@IDTLINK)");
            SetDataParameter(new DataParameter(CS, "IDTLINK", DbType.Int32), m_IdT);
            if (StrFunc.IsFilled(_gProduct_In))
            {
                sqlWhere.Append("(p_A.GPRODUCT = @GPRODUCT) and (p_B.GPRODUCT = @GPRODUCT)");
                SetDataParameter(new DataParameter(CS, "GPRODUCT", DbType.AnsiString, 64), _gProduct_In);
            }
            InitSQLWhere(sqlWhere.ToString(), false);

        }
        // EG 20150716 Add IDSTACTIVATION for ID_B and IDT_A
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override void SetSQLFrom()
        {
            base.SetSQLFrom();

            string sqlFrom = Cst.CrLf + String.Format(@"inner join dbo.TRADETRAIL tl on (tl.IDT_L = {0}.IDT_L)
            inner join dbo.TRADE t_A on  (t_A.IDT = {0}.IDT_A)
            inner join dbo.INSTRUMENT i_A on (i_A.IDI = t_A.IDI)
            inner join dbo.PRODUCT p_A on (p_A.IDP = i_A.IDP)
            inner join dbo.TRADE t_B on  (t_B.IDT = {0}.IDT_B)
            inner join dbo.INSTRUMENT i_B on (i_B.IDI = t_B.IDI)
            inner join dbo.PRODUCT p_B on (p_B.IDP = i_B.IDP)", SQLObject);

            ConstructFrom(sqlFrom);
        }
        // EG 20120621 Add GProduct for IDT_A and IDT_B
        protected override void SetSQLOrderBy()
        {
            // RD 20120601 [17855] - Mettre en dernier les liens "Garantie et Soldes" sur un ETD
            //string orderBy = SQLCst.CASE + SQLCst.CASE_WHEN + SQLObject + ".LINK=" + DataHelper.SQLString(TradeLink.TradeLinkType.ExchangeTradedDerivativeInCashBalance.ToString());
            //orderBy += SQLCst.CASE_THEN + "1" + SQLCst.CASE_ELSE + "0" + SQLCst.CASE_END + ", " + SQLObject + ".IDT_L" + SQLCst.DESC;
            ConstructOrderBy(SQLObject + ".IDT_L" + SQLCst.DESC);
        }
        #endregion Methods

    }
    #endregion SQL_TradeLink

    
    /// <summary>
    /// 
    /// </summary>
    // FI 20150304 [XXPOC] Add
    public class SQL_BusinessCenter : SQL_Table
    {

        #region private Members
        private readonly string _id_In;
        #endregion private Members

        #region Constructors
        public SQL_BusinessCenter(string pSource, string pId)
            : this(pSource, pId,  ScanDataDtEnabledEnum.No) { }
        public SQL_BusinessCenter(string pSource, string pId, ScanDataDtEnabledEnum pScanDataEnabled)
            : base(pSource, Cst.OTCml_TBL.BUSINESSCENTER.ToString(), pScanDataEnabled)
        {
            _id_In = pId; 
        }
        #endregion Constructors

        #region method

        /// <summary>
        /// 
        /// </summary>
        public string IdBC
        {
            get { return Convert.ToString(GetFirstRowColumnValue("IDBC")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DisPlayname
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DISPLAYNAME")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return Convert.ToString(GetFirstRowColumnValue("DESCRIPTION")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExtlLink
        {
            get { return Convert.ToString(GetFirstRowColumnValue("EXTLLINK")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Source
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SOURCE")); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetSQLWhere()
        {
            SQLWhere sqlWhere = new SQLWhere();
            sqlWhere.Append(SQLObject + ".IDBC" + SQLCst.LIKE + "@IDBC");

            SetDataParameter(new DataParameter(CS, "IDBC", DbType.AnsiString, 64), _id_In);

            InitSQLWhere(sqlWhere.ToString(), false);
        }
        #endregion
    }



}

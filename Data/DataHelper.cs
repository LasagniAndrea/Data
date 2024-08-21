//===============================================================================
// DataHelper by Tyler Jensen, Principal, NetBrick Inc. - tyler@netbrick.net 
// Verion 1.0 - March 2003
// Release Notes: 
// This class is a generic bridge to either SqlHelper or OraHelper. It uses interfaces
// for generic access to the database without knowing specifically which database server
// is being accessed.
//===============================================================================
// Copyright (C) 2003 NetBrick Inc.. Limited rights reserved as stated here:
// This work is a derivative work and thus likely falls under the copyrights of Microsoft.
// The author claims no copyright other than for the portion of the work which
// he produced. For such copyrights, the author reserves no rights other than
// to be notified by email at the above address of improvements to the work.
//===============================================================================
// Portions taken from Microsoft's original work: Microsoft.ApplictionBlocks.Data
// Copyright (C) 2000-2001 Microsoft Corporation
// Full download for the Microsoft.ApplictionBlocks.Data.SqlHelper class can be found at:
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/emab-rm.asp
//===============================================================================
// Any user of this work should consider the copyrights of Microsoft Corporation
// prior to the distribution of this code.
//===============================================================================
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Configuration;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Concurrent;
//
using EFS.ACommon;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq.Expressions;

namespace EFS.ApplicationBlocks.Data
{
    #region Enums
    #region public enum RdbmsEnum
    public enum RdbmsEnum
    {
        IBMDB2,
        //ORACL9I2, ORACL10G, ORACL11G, 
        ORACL12C, ORACL18C, ORACL19C,
        //SQLSRV2K, SQLSRV2K5, 
        //SQLSRV2K8,
        SQLSRV2K12, SQLSRV2K14, SQLSRV2K16, SQLSRV2K17, SQLSRV2K19, SQLSRV2K22, 
        //SYBASE12
        SAPASE
    }
    #endregion RdbmsEnum
    #endregion Enums

    /// <summary>
    ///  Delegue destiné à la journalisation d'une erreur lorsqu'une requête produit une exception
    /// </summary>
    /// <param name="pSource"></param>
    /// <param name="pMessage">Message d'erreur</param>
    /// FI 20190705 [XXXXX] Add
    public delegate void TraceQueryError(object pSource, string pMessage);

    /// <summary>
    ///  Delegue destiné à la journalisation d'une warning
    /// </summary>
    /// <param name="pSource"></param>
    /// <param name="pMessage">Message de warning</param>
    /// FI 20190719 [XXXXX] Add
    public delegate void TraceQueryWarning(object pSource, string pMessage);


    #region public Class DataHelper
    #region DataHelper/Master
    /// <summary>
    /// The DataHelper class is intended to pass through to either SqlHelper or OraHelper 
    /// </summary>
    public sealed partial class DataHelper
    {
        public static RowUpdated RowUpdated;
#if DEBUG
        // Activer la tarec SQL en mode Debug
        private static bool isTraceQueryForDebug = false;
        private static DateTime dtStartTraceQuery;
#endif
        /// <summary>
        ///  cache contenant les résulats obtenus via l'appel de dataHelper.TransformQuery
        /// </summary>
        /// FI 20201009 [XXXXX] Add 
        private static readonly ConcurrentDictionary<string, String> _tranformQueryCacheConcurrent = new ConcurrentDictionary<string, string>();

        /// <summary>
        ///  cache contenant les résulats obtenus via l'appel de dataHelper.TransformQuery
        /// </summary>
        private static readonly Dictionary<string, String> _tranformQueryCacheBasic = new Dictionary<string, string>();
        /// <summary>
        /// 
        /// </summary>
        private enum TranformQueryCacheMode
        {
            /// <summary>
            /// No cache (Permet de désactiver le cache)
            /// </summary>
            None,
            /// <summary>
            /// cache using Dictionary (Valeur par défaut)
            /// </summary>
            Basic,
            /// <summary>
            /// cache using ConcurrentDictionary
            /// </summary>
            Concurrent
        }


        #region public const SQL Bit Values
        public const int
            SQL_Bit_False = 0,
            SQL_Bit_True = 1;
        #endregion

        #region Constante
        private const string
            ORA_ALL_ = " " + "ALL_",
            ORA_USER_ = " " + "USER_",
            ORA_DUALTABLE = " " + "DUAL",
            TBLTMP_USER = " " + "UTT_",
            TBL_USER = " " + "UT_",
            V_USER = " " + "UV_";
            //ORA_INNERJOIN = "=",
            //ORA_LEFTJOIN = "(+)" + "=",
            //ORA_RIGHTJOIN = "=" + "(+)",
            //ORA_JOINEXTERNE = " " + "(+)" + " ",
            //ORA_ISNULL = "nvl";
        #endregion Constante

        #region Members
        public static QueryCache queryCache = InitializeQueryCache();

        /// <summary>
        /// Find all the || operator occurrences into a string
        /// </summary>
        //private static Regex findPipeOperators = new Regex(@"(\s+)(\|\|)(\s+)");

        #endregion

        #region constructor
        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelper()".
        private DataHelper()
        {

        }
        #endregion constructor

        #region internal Method
        #region internal GetDAL
        /// <summary>
        ///  Obtient le DAL
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <exception cref="DataHelperException si une exception se produit"></exception>
        internal static IDataAccess GetDAL(string pCS)
        {
            return GetDAL(DataHelper.GetDbSvrType(new CSManager(pCS).Cs));
        }
        /// <summary>
        ///  Obtient le DAL
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <exception cref="DataHelperException si une exception se produit"></exception>
        internal static IDataAccess GetDAL(DbSvrType serverType)
        {
            // RD 20100111 [16818] Add new DALOleDb
            IDataAccess dal;
            string errMsg = "Data Access Layer not found" + Cst.CrLf;
            //
            try
            {
                dal = DALCache.GetDAL(serverType);
                //
                if (null == dal)
                {
                    string className = string.Empty;
                    string errMsg2 = "{1}DAL DLL doesn't exist in the BIN directory of application";
                    //
                    switch (serverType)
                    {
                        case DbSvrType.dbORA:
                            
                            string OracleDataAccessMode = ConfigurationManager.AppSettings["OracleDataAccessMode"];
                            switch (OracleDataAccessMode)
                            {
                                case "Managed":
                                    className = "EFS.DALOracle.Managed.DataAccess, EFS.DALOracleManaged";
                                    break;
                                case "Unmanaged":
                                    className = "EFS.DALOracle.DataAccess, EFS.DALOracle";
                                    break;
                                default:
                                    className = "EFS.DALOracleManaged.DataAccess, EFS.DALOracleManaged";
                                    break;
                            }
                            break;
                        case DbSvrType.dbSQL:
                            className = "EFS.DALSQLServer.DataAccess, EFS.DALSQLServer";
                            break;
                        case DbSvrType.dbOLEDB:
                            className = "EFS.DALOleDb.DataAccess, EFS.DALOleDb";
                            break;
                        default:
                            throw (new ArgumentException("No match for connection found", "connectionString"));
                    }
                    
                    Type dalType = Type.GetType(className);
                    if (null == dalType)
                        throw new Exception(errMsg2.Replace("{1}", "Data access component: ''" + className + "'' is not correct, or "));

                    dal = (IDataAccess)Activator.CreateInstance(dalType);

                    if (null == dal)
                        throw new Exception(errMsg2.Replace("{1}", string.Empty));

                    DALCache.CacheDAL(serverType, dal);
                }
                return dal;
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.loadDal, errMsg, ex);
            }
        }
        #endregion
        #endregion

        #region public Method
        /// <summary>
        ///  Returne the referenced assembly used by the DAL (Data Access Layer) associated to <paramref name="serverType"/> and the <paramref name="query"/>
        /// </summary>
        /// <param name="serverType"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string AddDALRefencedAssemblyInfos(DbSvrType serverType, string query)
        {
            return "/* " + GetDAL(serverType).GetReferencedAssemblyInfos() + " */" + Cst.CrLf + query;
        }
        
        // RD 20100114 [16792]
        #region public GetSQLErrorMessage
        ///
        /// Retourne un message destiné à être traduit et affiché
        public static string GetSQLErrorMessage(SQLErrorEnum opMessage)
        {
            return "SQLError_" + opMessage.ToString();
        }
        #endregion
        // RD 20100114 [16792] Ajout de surcharges
        #region public AnalyseSQLException
        /// <summary>
        /// Retourne true si l'exception est de type SQL 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pException"></param>
        /// <returns></returns>
        public static SQLErrorEnum AnalyseSQLException(string pSource, Exception pException)
        {
            return GetDAL(pSource).AnalyseSQLException(pException);
        }
        /// <summary>
        /// Retourne true si l'exception est de type SQL
        /// <para>Retourne un message d'erreur clair si l'erreur est de type SQL</para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pException"></param>
        /// <param name="opMessageError"></param>
        /// <returns></returns>
        public static bool AnalyseSQLException(string pSource, Exception pException, out string opMessageError)
        {
            return GetDAL(pSource).AnalyseSQLException(pException, out opMessageError, out _);
        }
        /// <summary>
        /// Retourne true si l'exception est de type SQL
        /// <para>Retourne un message d'erreur clair et un code erreur clair si l'erreur est de type SQL</para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pException"></param>
        /// <param name="opMessageError"></param>
        /// <param name="opErrorCode"></param>
        /// <returns></returns>
        public static bool AnalyseSQLException(string pSource, Exception pException, out string opMessageError, out string opErrorCode)
        {
            opErrorCode = string.Empty;
            bool ret = GetDAL(pSource).AnalyseSQLException(pException, out opMessageError, out SQLErrorEnum sqlErrorMessage);
            if (ret)
                opErrorCode = Ressource.GetString("SQLError_" + sqlErrorMessage.ToString());
            return ret;
        }
        /// <summary>
        /// Retourne true si l'exception est de type SQL
        /// <para>Retourne un message d'erreur clair et un code erreur si l'erreur est de type SQL</para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pException"></param>
        /// <param name="opMessageError">Message d'erreur</param>
        /// <param name="opErrorCode">Code erreur</param>
        /// <returns></returns>
        public static bool AnalyseSQLException(string pSource, Exception pException, out string opMessageError, out SQLErrorEnum opErrorCode)
        {
            return GetDAL(pSource).AnalyseSQLException(pException, out opMessageError, out opErrorCode);
        }
        #endregion
        //
        #region public CheckConnection
        /// <summary>
        /// Contrôle la connection à la base de donnée
        /// <para>Charge le DAL puis vérifie que la connexion s'opère à la base de donnée </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pThrowException"></param>
        /// <returns></returns>
        /// <exception cref="DataHelperException si le DAL est non chargé ou si la connexion n'aboutie pas"></exception>
        /// FI 20140123 [XXXXX] l'appel à GetDAL doit se faire dans le try catch pour respecter le commentaire suivant
        ///     exception cref="DataHelperException si le DAL est non chargé ou si la connexion n'aboutie pas
        public static void CheckConnection(string pCS)
        {
            WriteTraceQuery(true, pCS, "Check Connection", string.Empty, null);
            string cs = new CSManager(pCS).Cs;

            IDataAccess dataAcces;
            try
            {
                dataAcces = GetDAL(cs);
                dataAcces.CheckConnection(cs);
            }
            catch (Exception ex)
            {
                // LP 20240705 [WI992] Service : Add trace entry when the connection to server failed
                TraceQueryError(new CSManager(cs), ex.Message, CommandType.Text.ToString(), null);
                DataHelperException dataHelperEx = new DataHelperException(DataHelperErrorEnum.connection, "Database connection could not be established", ex);
                throw dataHelperEx;
            }
            //
            //Ajout de la CS dans le Cache
            // RD 20100111 [16818] Add new DALOleDb
            if (null != dataAcces)
                ConnectionStringCache.CacheConnectionString(cs, dataAcces.GetDbSvrType);
            WriteTraceQuery();
        }
        #endregion
        #region public GetServerVersion
        /// <summary>
        /// Retourne la version du serveur de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static string GetServerVersion(string pCS)
        {
            WriteTraceQuery(true, pCS, "Get Server Version", string.Empty, null);
            string cs = new CSManager(pCS).Cs;

            IDataAccess dataAcces = GetDAL(cs);

            string ret;
            try
            {
                ret = dataAcces.GetServerVersion(cs);
            }
            catch (Exception ex)
            {
                DataHelperException dataHelperEx = new DataHelperException(DataHelperErrorEnum.connection, "Database connection could not be established", ex);
                throw dataHelperEx;
            }

            WriteTraceQuery();

            return ret;
        }
        #endregion

        #region public DbTypeToSqlDbType
        // EG 20180423 Analyse du code Correction [CA2200]
        public static SqlDbType DbTypeToSqlDbType(DbType pdbType)
        {
            // 20090206 RD 16480 (DAL) 
            //return GetDAL().DbTypeToSqlDbType(pdbType);
            //----------------
            SqlParameter parm = new SqlParameter();
            try
            {
                parm.DbType = pdbType;
            }
            catch (Exception)
            {
                throw;
                // can't map
            }
            return parm.SqlDbType;
            //----------------
        }
        #endregion
        //
        #region public GetDbSvrType
        public static DbSvrType GetDbSvrType(string pCS)
        {
            // RD 20100111 [16818] Add new DALOleDb
            DbSvrType ret = DbSvrType.dbUNKNOWN;
            CSManager csManager = new CSManager(pCS);
            //
            if (ConnectionStringCache.GetConnectionStringState(csManager.Cs) == ConnectionStringCacheState.isNotCached)
            {
                ret = csManager.GetDbSvrType();
            }
            else
            {
                if (ConnectionStringCache.GetConnectionStringState(csManager.Cs) == ConnectionStringCacheState.isOracle)
                    ret = DbSvrType.dbORA;
                else if (ConnectionStringCache.GetConnectionStringState(csManager.Cs) == ConnectionStringCacheState.isSqlServer)
                    ret = DbSvrType.dbSQL;
                else if (ConnectionStringCache.GetConnectionStringState(csManager.Cs) == ConnectionStringCacheState.isOleDb)
                    ret = DbSvrType.dbOLEDB;
            }
            //
            return ret;
        }
        public static DbSvrType GetDbSvrType(IDbConnection dbConn)
        {
            DbSvrType ret = DbSvrType.dbUNKNOWN;
            // 20080402 RD ODP.NET
            // RD 20100111 [16818] Add new DALOleDb
            if (dbConn.GetType().FullName.ToLower().IndexOf("oracle") >= 0)
            {
                ret = DbSvrType.dbORA;
            }
            else if (dbConn.GetType().FullName.ToLower().IndexOf("sql") > 0)
            {
                ret = DbSvrType.dbSQL;
            }
            else if (dbConn.GetType().FullName.ToLower().IndexOf("oledb") > 0)
            {
                ret = DbSvrType.dbOLEDB;
            }
            return ret;
        }
        public static DbSvrType GetDbSvrType(IDbTransaction trans)
        {
            DbSvrType ret = DbSvrType.dbUNKNOWN;
            // 20080402 RD ODP.NET
            // RD 20100111 [16818] Add new DALOleDb
            if (trans.GetType().FullName.ToLower().IndexOf("oracle") >= 0)
            {
                ret = DbSvrType.dbORA;
            }
            else if (trans.GetType().FullName.ToLower().IndexOf("sql") > 0)
            {
                ret = DbSvrType.dbSQL;
            }
            else if (trans.GetType().FullName.ToLower().IndexOf("oledb") > 0)
            {
                ret = DbSvrType.dbOLEDB;
            }
            return ret;
        }
        #endregion
        #region public GetVarPrefix
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string GetVarPrefix(DbSvrType ServerType)
        {
            // 20090206 RD 16480 (DAL) 
            return GetDAL(ServerType).GetVarPrefix;
        }
        public static string GetVarPrefix(string pCS)
        {
            return GetVarPrefix(DataHelper.GetDbSvrType(pCS));
        }
        #endregion GetVarPrefix
        #region public GetRowNumber
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <param name="pDataRow"></param> 
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static int GetRowNumber(DataTable pDataTable, DataRow pDataRow)
        {
            int row;
            try
            {
                Type t = pDataTable.Rows.GetType();
                row = ((IList)t.GetMethod("get_List", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(pDataTable.Rows, null)).IndexOf(pDataRow);
                return row;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion GetRowNumber

        #region public GetSelectTop
        /// <summary>
        /// Limite le jeu de résultat d'une query
        /// <para>Sur SqlServer utilise un select Top</para>
        /// <para>Sur Oracle utilise la clause fetch first (syntaxe disponible depuis la 12 c)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSelect"></param>
        /// <param name="pTop"></param>
        /// <returns></returns>
        public static string GetSelectTop(string pCS, string pSelect, int pTop)
        {
            return GetSelectTop(GetDbSvrType(pCS), pSelect, pTop);
        }
        /// <summary>
        /// Limite le jeu de résultat d'une query
        /// <para>Sur SqlServer utilise un select Top</para>
        /// <para>Sur Oracle utilise la clause fetch first (syntaxe disponible depuis la 12 c)</para>
        /// </summary>
        /// <param name="ServerType"></param>
        /// <param name="pSelect"></param>
        /// <param name="pTop"></param>
        /// <returns></returns>
        /// FI 20230620 [XXXXX] évolution Pour Oracle
        public static string GetSelectTop(DbSvrType ServerType, string pSelect, int pTop)
        {
            string select = pSelect.TrimStart();
            string ret = pSelect;
            
            if (pTop > -1)
            {
                switch (ServerType)
                {
                    case DbSvrType.dbORA:
                        // FI 20230620 [XXXXX] utilisation de la caluse fetch
                        //ret = SQLCst.SELECT_ALL + SQLCst.X_FROM + "(" + Cst.CrLf + select + Cst.CrLf + ")" + Cst.CrLf;
                        //ret += SQLCst.WHERE + "rownum <=" + pTop.ToString();
                        //since 12c release, ORACLE provided a FETCH Clause Syntax 
                        ret += $"{Cst.CrLf} fetch first {pTop} rows only";

                        break;
                    case DbSvrType.dbSQL:
                        int posSelect = select.IndexOf(SQLCst.SELECT_DISTINCT);
                        bool isSelectDistinct = (posSelect == 0); // Commemce avec un select distinct ?
                        if (false == isSelectDistinct)
                            posSelect = select.IndexOf(SQLCst.SELECT);
                        //
                        if (posSelect != 0)
                            throw new ArgumentException(StrFunc.AppendFormat("Query must start With '{0}'", SQLCst.SELECT));
                        //
                        int length;
                        if (isSelectDistinct)
                        {
                            length = SQLCst.SELECT_DISTINCT.Length;
                            ret = SQLCst.SELECT_DISTINCT_TOP;
                        }
                        else
                        {
                            length = SQLCst.SELECT.Length;
                            ret = SQLCst.SELECT_TOP;
                        }
                        //
                        ret += pTop.ToString() + Cst.Space + select.Substring(posSelect + length);
                        break;
                    default:
                        throw new ArgumentException(StrFunc.AppendFormat("SerTyper[{0}] not implemented", ServerType.ToString()));
                }
            }
            return ret;
        }
        #endregion

        #region GetSelectOrderRownumber
        /// <summary>
        ///  Retourne une commande select qui s'appuie sur la fonction ROWNUMBER() pour trier les données (La fonction ROW_NUMBER permet d'attribuer un numéro de ligne selon un tri. La numérotation commence à 1)
        ///  <para>Possibilité de filtrer les données dans une plage de numéro de ligne (exemple : de la 5ème ligne à la 15ème ligne, ou de la 1ère ligne à 100ème ligne)</para>
        /// </summary>
        /// <param name="pSelect">requête select à trier</param>
        /// <param name="pOrderByClause">clause order by utilisée pour le tri (doit commener par order by)</param>
        /// <param name="pSelectInSubquery">si true {pSelect} est dans un sous-select de la commande retournée</param>
        /// <param name="pStartRow">Ligne début (par défaut 1) pour le filtre</param>
        /// <param name="pEndRow">Ligne fin pour le filtre (-1 pour récupérer toutes les lignes à partir de pStartRow)</param>
        /// FI 20210726 [XXXXX] Add Method
        public static string GetSelectOrderRowNumber(string pSelect, string pOrderByClause, Boolean pSelectInSubquery, int pStartRow = 1, int pEndRow = -1)
        {
            if (StrFunc.IsEmpty(pSelect))
                throw new ArgumentException("Argument is empty", "pSelect");

            if (StrFunc.IsEmpty(pOrderByClause))
                throw new ArgumentException("Argument is empty", "pOrderByClause");

            if (false == pOrderByClause.TrimStart().StartsWith(SQLCst.ORDERBY.TrimStart()))
                throw new ArgumentException($"Argument must start with {SQLCst.ORDERBY.TrimStart()}", "pOrderByClause");

            if ((pStartRow < 1))
                throw new ArgumentException($"StartRow must be greater than 1", "pEndRow");

            if ((pEndRow != -1) && pEndRow < pStartRow)
                throw new ArgumentException($"EndRow must be greater than startRow", "pEndRow");

            string sQLWhereClause = string.Empty;
            if (pEndRow > -1)
            {
                if (pStartRow != 1)
                    sQLWhereClause = $"{SQLCst.WHERE} ROW_NUM {SQLCst.BETWEEN} {pStartRow} {SQLCst.AND} {pEndRow}";
                else
                    sQLWhereClause = $"{SQLCst.WHERE} ROW_NUM <= {pEndRow}";
            }

            string ret;
            if (pSelectInSubquery)
            {
                // EG 20210813 Upd with
                ret = $@"
with CTE_ROW_NUM as (
{SQLCst.SELECT} rankQry.*, ROW_NUMBER() over({pOrderByClause}) ROW_NUM
    {SQLCst.X_FROM} 
    (
        {pSelect}  
    ) rankQry
)";

            }
            else
            {
                if (pSelect.Contains(SQLCst.ORDERBY))
                    throw new ArgumentException(StrFunc.AppendFormat("Keyword [{0}] not allowed in parameter select [{1}]", SQLCst.ORDERBY, pSelect));

                string strBefore = StrFunc.Before(pSelect, SQLCst.X_FROM.Trim());
                string strAfter = StrFunc.After(pSelect, SQLCst.X_FROM.Trim(), OccurenceEnum.First);

                ret = $@"
with CTE_ROW_NUM as (
{strBefore}, ROW_NUMBER() over({pOrderByClause}) ROW_NUM {SQLCst.X_FROM.Trim()}{strAfter}
)";
            }

            ret += Cst.CrLf;
            ret += $"{SQLCst.SELECT}*{SQLCst.X_FROM} CTE_ROW_NUM {sQLWhereClause} {SQLCst.ORDERBY} ROW_NUM {SQLCst.ASC}";

            return ret;
        }
        #endregion

        #region public GetDBData
        public static object GetDBData(DateTime pData)
        {
            return (DtFunc.IsDateTimeEmpty(pData) ? Convert.DBNull : pData);
        }
        public static object GetDBData(string pData)
        {
            return pData ?? Convert.DBNull;
        }
        public static object GetDBData(object pData)
        {
            return pData ?? Convert.DBNull;
        }
        public static object GetDBData(int pData)
        {
            return (0 == pData) ? Convert.DBNull : pData;
        }
        #endregion GetDBData


        /// <summary>
        /// Retourne l'opérateur SQL de tri (ASC/DESC) pour avoir les valeurs "Null" en première position.
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string GetOrderBySideForNullValueInFirstPosition(string pCS)
        {
            switch (GetDbSvrType(pCS))
            {
                case DbSvrType.dbORA:
                    return SQLCst.DESC;
                case DbSvrType.dbSQL:
                    return SQLCst.ASC;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        /// <summary>
        /// Retourne l'opérateur SQL de tri (ASC/DESC) pour avoir les valeurs "Null" en dernière position.
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string GetOrderBySideForNullValueInLastPosition(string pCS)
        {
            switch (GetDbSvrType(pCS))
            {
                case DbSvrType.dbORA:
                    return SQLCst.ASC;
                case DbSvrType.dbSQL:
                    return SQLCst.DESC;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string GetConcatOperator(string pCS)
        {
            switch (GetDbSvrType(pCS))
            {
                case DbSvrType.dbORA:
                    return " || ";
                case DbSvrType.dbSQL:
                    return " + ";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pAliasTable"></param>
        /// <returns></returns>
        public static string GetROWVERSION(string pConnectionString, string pAliasTable)
        {
            return GetROWVERSION(pConnectionString, pAliasTable, Cst.OTCml_COL.ROWVERSION.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pAliasCol"></param>
        /// <returns></returns>
        public static string GetROWVERSION(string pConnectionString, string pAliasTable, string pAliasCol)
        {
            string rowversion;

            if (pAliasTable == null)
                pAliasTable = string.Empty;
            else
                pAliasTable += ".";

            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    //20061114 PL Gestion du ROWVERSION a revoir
                    rowversion = "TO_NUMBER(" + pAliasTable + Cst.OTCml_COL.ROWVERSION.ToString() + ")";
                    break;
                case DbSvrType.dbSQL:
                    rowversion = "convert(int, " + pAliasTable + Cst.OTCml_COL.ROWVERSION.ToString() + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            // 20090902 RD  / Chantier des totaux
            if (StrFunc.IsFilled(pAliasCol))
                rowversion += SQLCst.AS + pAliasCol;
            return rowversion;
        }

        #region public static GetSQLXQuery_ExistsNode
        public static string GetSQLXQuery_ExistsNode(string pCs, string pColumn, string pAliasTable, string pXQuery, string pNameSpace)
        {
            string ret = string.Empty;
            //
            string column = pColumn;
            if (StrFunc.IsFilled(pAliasTable))
                column = pAliasTable + "." + column;
            //
            if (IsDbSqlServer(pCs))
            {
                ret += column + @".exist('";
                ret += pNameSpace;
                ret += "(" + pXQuery + ")')=1" + Cst.CrLf;
            }
            else if (IsDbOracle(pCs))
            {
                ret += @"existsnode(";
                ret += column + ", " + Cst.CrLf;
                ret += "'" + pXQuery + "', " + Cst.CrLf;
                ret += "'" + pNameSpace + "')=1" + Cst.CrLf;
            }
            else
                throw new NotImplementedException("RDBMS not implemented");
            //
            return ret;
        }
        #endregion

        #region public static GetSQLXQuery_ExtractValue
        public static string GetSQLXQuery_ExtractValue(string pCs, string pColumn, string pColumnType, string pAliasTable, string pXQuery, string pNameSpace)
        {
            return GetSQLXQuery_ExtractValue(GetDbSvrType(pCs), pColumn, pColumnType, pAliasTable, pXQuery, pNameSpace);
        }
        public static string GetSQLXQuery_ExtractValue(DbSvrType pServerType, string pColumn, string pColumnType, string pAliasTable, string pXQuery, string pNameSpace)
        {
            string ret = string.Empty;
            //
            string column = pColumn;
            if (StrFunc.IsFilled(pAliasTable))
                column = pAliasTable + "." + column;
            //
            if (pServerType == DbSvrType.dbSQL)
            {
                ret += column + @".value('";
                ret += pNameSpace;
                ret += "(" + pXQuery + ")[1]', '" + pColumnType + "')" + Cst.CrLf;
            }
            else if (pServerType == DbSvrType.dbORA)
            {
                ret += @"extractvalue(";
                ret += column + ", " + Cst.CrLf;
                ret += "'" + pXQuery + "', " + Cst.CrLf;
                ret += "'" + pNameSpace + "')" + Cst.CrLf;
            }
            else
                throw new NotImplementedException("RDBMS not implemented");
            //
            return ret;
        }
        #endregion

        public static bool IsRdmbsOracle(string pRdmbs)
        {
            //bool ret = (IsRdmbsOracle9i(pRdmbs) || IsRdmbsOracle10g(pRdmbs));
            bool ret = (pRdmbs == RdbmsEnum.ORACL12C.ToString())
                || (pRdmbs == RdbmsEnum.ORACL18C.ToString())
                || (pRdmbs == RdbmsEnum.ORACL19C.ToString());
            return ret;
        }
        //private static bool IsRdmbsOracle9i(string pRdmbs)
        //{
        //    bool ret = (pRdmbs == RdbmsEnum.ORACL9I2.ToString());
        //    return ret;
        //}
        //private static bool IsRdmbsOracle10g(string pRdmbs)
        //{
        //    bool ret = (pRdmbs == RdbmsEnum.ORACL10G.ToString())
        //        || (pRdmbs == RdbmsEnum.ORACL11G.ToString())
        //        || (pRdmbs == RdbmsEnum.ORACL12C.ToString());
        //    return ret;
        //}
        public static bool IsRdmbsSQLServer(string pRdmbs)
        {
            return
                //(pRdmbs == RdbmsEnum.SQLSRV2K.ToString()) ||
                //(pRdmbs == RdbmsEnum.SQLSRV2K5.ToString()) || 
                //(pRdmbs == RdbmsEnum.SQLSRV2K8.ToString()) ||
                (pRdmbs == RdbmsEnum.SQLSRV2K12.ToString()) ||
                (pRdmbs == RdbmsEnum.SQLSRV2K14.ToString()) ||
                (pRdmbs == RdbmsEnum.SQLSRV2K16.ToString()) ||
                (pRdmbs == RdbmsEnum.SQLSRV2K17.ToString()) ||
                (pRdmbs == RdbmsEnum.SQLSRV2K19.ToString()) ||
                (pRdmbs == RdbmsEnum.SQLSRV2K22.ToString());
        }
        // EG 20200527 [XXXXX] SQLServer 2016 ou plus  
        // EG 20200706 [XXXXX] Upd by Contains
        public static bool IsRdmbsSQLServer16OrHigher(string pRdmbs)
        {
            return (pRdmbs.Contains(RdbmsEnum.SQLSRV2K16.ToString())) ||
                   (pRdmbs.Contains(RdbmsEnum.SQLSRV2K17.ToString())) ||
                   (pRdmbs.Contains(RdbmsEnum.SQLSRV2K19.ToString())) ||
                   (pRdmbs.Contains(RdbmsEnum.SQLSRV2K22.ToString())); 
        }

        public static bool IsRdmbsIBMDB2(string pRdmbs)
        {
            return (pRdmbs == RdbmsEnum.IBMDB2.ToString());
        }
        //public static bool IsRdmbsSybase(string pRdmbs)
        public static bool IsRdmbsSAPASE(string pRdmbs)
        {
            //return (pRdmbs == RdbmsEnum.SYBASE12.ToString());
            return (pRdmbs == RdbmsEnum.SAPASE.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static bool IsDbOracle(string pCS)
        {
            return (GetDbSvrType(pCS) == DbSvrType.dbORA);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static bool IsDbSqlServer(string pCS)
        {
            return (GetDbSvrType(pCS) == DbSvrType.dbSQL);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static bool IsDbOleDb(string pCS)
        {
            return (GetDbSvrType(pCS) == DbSvrType.dbOLEDB);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pEx"></param>
        /// <returns></returns>
        public static bool IsDuplicateKeyError(string pCs, Exception pEx)
        {
            return GetDAL(pCs).IsDuplicateKeyError(pEx);
        }

        /// <summary>
        /// Retourne true si l'exception est de type exception SQL 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pEx"></param>
        /// <returns></returns>
        public static bool IsSQLException(string pCs, Exception pEx)
        {
            return GetDAL(pCs).IsSQLException(pEx);
        }

        /// <summary>
        /// Retourne une collection sous forme de liste où les données sont séparées par des virgules afin de permettre leur exploitation dans un IN par exemple
        /// <para>Lorsque le type de donnée est de type String, chaque donné est entourée par 2 simple cote (')</para>
        /// <para>Retourne null si la collection est vide</para>
        /// </summary>
        public static string SQLCollectionToSqlList(string pCS, ICollection pcol, TypeData.TypeDataEnum pDataType)
        {
            string ret = null;
            if (null != pcol)
                ret = SQLArrayListToSqlList(pCS, pcol, pDataType, false);
            return ret;

        }
        /// <summary>
        /// Obtient une collection sous forme de liste où les données sont séparées par des virgules afin de permettre leur exploitation dans un IN par exemple
        /// <para>Lorsque le type de donnée est de type String, chaque donné est entourée par 2 simple cote (') avec possibilité de généré des upper ()</para>
        /// </summary>
        public static string SQLCollectionToSqlList(string pCS, ICollection pcol, TypeData.TypeDataEnum pDataType, bool pIsUseUpper)
        {

            string ret = null;
            if (null != pcol)
                ret = SQLArrayListToSqlList(pCS, pcol, pDataType, pIsUseUpper);


            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCol"></param>
        /// <param name="pDataType"></param>
        /// <param name="pIsUseUpper"></param>
        /// <returns></returns>
        /// FI 20170330 [23031] Add Modify
        private static string SQLArrayListToSqlList(string pCs, ICollection pcol, TypeData.TypeDataEnum pDataType, bool pIsUseUpper)
        {

            if ((pIsUseUpper) && (pDataType != TypeData.TypeDataEnum.@string))
                throw new ArgumentException(StrFunc.AppendFormat("Invalid DataType {0}", pDataType.ToString()));

            ArrayList arrayList = new ArrayList(pcol);

            string ret = string.Empty;
            for (int i = 0; i < arrayList.Count; i++)
            {
                string currentValue = arrayList[i].ToString();

                switch (pDataType) // A enrichir
                {
                    case TypeData.TypeDataEnum.text:
                    case TypeData.TypeDataEnum.@string:
                        if (pIsUseUpper)
                            currentValue = DataHelper.SQLUpper(pCs, DataHelper.SQLString(currentValue));
                        else
                            currentValue = DataHelper.SQLString(currentValue);
                        break;
                    case TypeData.TypeDataEnum.date: // FI 20170330 [23031] gestion de date
                        currentValue = DataHelper.SQLToDate(pCs, (DateTime)arrayList[i]);
                        break;
                    case TypeData.TypeDataEnum.datetime: // FI 20170330 [23031] gestion de date
                        currentValue = DataHelper.SQLToDateTime(pCs, (DateTime)arrayList[i]);
                        break;
                    default:
                        break;
                }

                ret += currentValue;

                if (i != arrayList.Count - 1)
                    ret += ",";
            }
            return ret;
        }

        /// <summary>
        ///  Retourne le propriétaire des objets (Tables, Views, SP...)
        ///  <para>SQLServer: dbo</para>
        ///  <para>Oracle: schema (User ID de la ConnectionString)</para>
        /// </summary>
        /// <param name="pCS"></param>
        public static string GetObjectsOwner(string pCS)
        {
            return GetObjectsOwner(new CSManager(pCS));
        }
        // EG 20181119 PERF Correction post RC (Step 2)
        public static void UpdateStatTable(string pCS, string pTable)
        {
            UpdateStatTable(new CSManager(pCS), pTable);
        }
        // EG 20181119 PERF Correction post RC (Step 2)
        public static void UpdateStatTable(CSManager pCSManager, string pTable)
        {
            DbSvrType dbSvrType = pCSManager.GetDbSvrType();


            switch (dbSvrType)
            {
                case DbSvrType.dbORA:
                    DataParameters dataParameters = new DataParameters();
                    dataParameters.Add(new DataParameter(pCSManager.Cs, CommandType.StoredProcedure, "ownname", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DataHelper.GetObjectsOwner(pCSManager));
                    dataParameters.Add(new DataParameter(pCSManager.Cs, CommandType.StoredProcedure, "tabname", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pTable);
                    QueryParameters qryParameters = new QueryParameters(pCSManager.Cs, "DBMS_STATS.GATHER_TABLE_STATS", dataParameters);
                    DataHelper.ExecuteNonQuery(pCSManager.Cs, CommandType.StoredProcedure, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        ///  Retourne le propriétaire des objets (Tables, Views, SP...)
        ///  <para>SQLServer: dbo</para>
        ///  <para>Oracle: schema (User ID de la ConnectionString)</para>
        /// </summary>
        /// <param name="pCSManager"></param>
        /// <returns></returns>
        public static string GetObjectsOwner(CSManager pCSManager)
        {
            string ret;
            DbSvrType dbSvrType = pCSManager.GetDbSvrType();

            switch (dbSvrType)
            {
                case DbSvrType.dbSQL:
                    ret = SQLCst.DBO.Trim().Replace(".", string.Empty);
                    break;
                case DbSvrType.dbORA:
                    ret = pCSManager.GetAttribute("User ID");
                    if (pCSManager.IsUserReadOnly())
                    {
                        //PL 20120105 New features
                        if (ret.EndsWith("_READONLY"))
                            ret = ret.Substring(0, ret.Length - 9);
                        else if (ret.EndsWith("_RO"))
                            ret = ret.Substring(0, ret.Length - 3);
                    }
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} not implemented", dbSvrType.ToString()));
            }
            return ret;
        }

        /// <summary>
        /// Pour découper la liste {pCol} en plusieurs sous listes de taille fixe
        /// <para>La taille est de 1000 actuellement qui est la limite sur Oracle</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pColumn"></param>
        /// <param name="pCol"></param>
        /// <param name="pDataType"></param>
        /// <param name="pReverse"></param>
        /// <param name="pWithDefaultSubListSize"></param>
        /// <returns></returns>
        public static string SQLColumnIn(string pCS, string pColumn, ICollection pCol, TypeData.TypeDataEnum pDataType,
            bool pReverse, bool pWithDefaultSubListSize)
        {
            // Il est peut être utile de mettre une constante
            return SQLColumnIn(pCS, pColumn, pCol, pDataType, pReverse, 1000);
        }

        /// <summary>
        /// Pour découper la liste {pCol} en plusieurs sous listes de taille fixe {pSubCollectionSize}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pColumn"></param>
        /// <param name="pCol"></param>
        /// <param name="pSubCollectionSize"></param>
        /// <param name="pDataType"></param>
        /// <param name="pReverse"></param>
        /// <returns></returns>
        public static string SQLColumnIn(string pCS, string pColumn, ICollection pCol, TypeData.TypeDataEnum pDataType,
            bool pReverse, int pSubCollectionSize)
        {
            string ret;
            int collectionSize = ArrFunc.Count(pCol);

            if (collectionSize <= pSubCollectionSize)
                ret = SQLColumnIn(pCS, pColumn, pCol, pDataType, pReverse);
            else
            {
                ArrayList list = ArrFunc.TransformCollectionToArrayList(pCol);
                int[] sourceList = (int[])list.ToArray(typeof(int));
                Array subList = Array.CreateInstance(typeof(System.Int32), pSubCollectionSize);

                // Copier le premier morceau
                Array.Copy(sourceList, 0, subList, 0, pSubCollectionSize);
                // PM 20140108 [19462] Ajout encadrement des In par des ()
                //ret = SQLColumnIn(pCS, pColumn, subList, pDataType, pReverse);
                ret = "(" + SQLColumnIn(pCS, pColumn, subList, pDataType, pReverse);

                int subCollectionCount = (collectionSize / pSubCollectionSize);
                int lengthToCopy = pSubCollectionSize;

                for (int i = 0; i < subCollectionCount; i++)
                {
                    if ((i + 1) == subCollectionCount)
                    {
                        lengthToCopy = collectionSize - (subCollectionCount * pSubCollectionSize);
                        if (lengthToCopy == 0)
                            continue;
                        //
                        subList = Array.CreateInstance(typeof(System.Int32), lengthToCopy);
                    }
                    else
                        Array.Clear(subList, 0, subList.Length);

                    Array.Copy(sourceList, (i + 1) * pSubCollectionSize, subList, 0, lengthToCopy);

                    ret += SQLCst.OR + SQLColumnIn(pCS, pColumn, subList, pDataType, pReverse);
                }
                // PM 20140108 [19462] Ajout encadrement des In par des ()
                ret += ")";
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumn"></param>
        /// <param name="pCol"></param>
        /// <param name="pDataType"></param>
        /// <returns></returns>
        public static string SQLColumnIn(string pCS, string pColumn, ICollection pCol, TypeData.TypeDataEnum pDataType)
        {
            return SQLColumnIn(pCS, pColumn, pCol, pDataType, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pColumn"></param>
        /// <param name="pCol"></param>
        /// <param name="pDataType"></param>
        /// <param name="pReverse"></param>
        /// <returns></returns>
        public static string SQLColumnIn(string pCS, string pColumn, ICollection pCol, TypeData.TypeDataEnum pDataType, bool pReverse)
        {

            string ret;
            if (ArrFunc.IsFilled(pCol))
            {
                bool IsOneValue = (1 == pCol.Count);
                string sqlOperator;

                if (IsOneValue)
                    sqlOperator = pReverse ? " != " : " = ";
                else
                    sqlOperator = pReverse ? " not in (" : " in (";

                ret = pColumn + sqlOperator + SQLCollectionToSqlList(pCS, pCol, pDataType);

                if (false == IsOneValue)
                    ret += ")";
            }
            else
                ret = pColumn + (pReverse ? SQLCst.IS_NOT_NULL.ToString() : SQLCst.IS_NULL.ToString());
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static string SQLComment(string pString)
        {
            return "/* " + pString + " */";
        }

        #region SQLBitand
        /// <summary>
        /// Comparaison 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pSubstitute"></param>
        /// <returns></returns>
        public static string SQLBitand(string pConnectionString, string pCol, string pValue)
        {
            return SQLBitand(pConnectionString, pCol, pValue, null);
        }
        public static string SQLBitand(string pConnectionString, string pCol, string pValue, string pAlias)
        {

            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = "bitand(" + pCol + "," + pValue + ")";
                    break;
                case DbSvrType.dbSQL:
                    ret = pCol + " & " + pValue;
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            if (StrFunc.IsFilled(pAlias))
                ret = ret + SQLCst.AS + pAlias;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pSubstitute"></param>
        /// <returns></returns>
        public static string SQLBitandCompare(string pConnectionString, string pCol, string pValue)
        {
            return SQLBitand(pConnectionString, pCol, pValue, null);
        }
        public static string SQLBitandCompare(string pConnectionString, string pCol, string pValue, string pAlias)
        {
            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = "bitand(" + pCol + "," + pValue + ") = " + pValue;
                    break;
                case DbSvrType.dbSQL:
                    ret = pCol + " & " + pValue + " = " + pValue;
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            if (StrFunc.IsFilled(pAlias))
                ret = ret + SQLCst.AS + pAlias;
            return ret;
        }
        #endregion SQLBitand
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBool"></param>
        /// <returns></returns>
        public static string SQLBoolean(bool pBool)
        {
            return (pBool ? SQL_Bit_True.ToString() : SQL_Bit_False.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static string SQLConcat(string pConnectionString, params string[] pData)
        {
            string ret = pData[0];
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    //return pData1 + " || " + pData2;
                    for (int i = 1; i < pData.Length; i++)
                        ret += " || " + pData[i];
                    return ret;

                case DbSvrType.dbSQL:
                    //return pData1 + " + " + pData2;
                    for (int i = 1; i < pData.Length; i++)
                        ret += " + " + pData[i];
                    return ret;

                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pStart"></param>
        /// <param name="pLenght"></param>
        /// <returns></returns>
        public static string SQLSubstring(string pConnectionString, string pData, int pStart, int pLenght)
        {
            string ret = string.Empty;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret += "substr";
                    break;

                case DbSvrType.dbSQL:
                    ret += "substring";
                    break;

                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret += "(" + pData + "," + pStart.ToString() + "," + pLenght.ToString() + ")";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pStart"></param>
        /// <param name="pLenght"></param>
        /// <returns></returns>
        public static string SQLSubstring(string pConnectionString, string pData, string pStart, string pLenght)
        {
            string ret = string.Empty;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret += "substr";
                    break;

                case DbSvrType.dbSQL:
                    ret += "substring";
                    break;

                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret += "(" + pData + "," + pStart + "," + pLenght + ")";
        }

        /// <summary>
        /// Retourne une commande SQL de type "case when then else end"
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pMaxLen"></param>
        /// <returns></returns>
        public static string SQLTruncstring(string pConnectionString, string pData, int pMaxLen)
        {
            string ret = string.Empty;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret += "case when length(nvl(" + pData + ",' ')) > " + pMaxLen.ToString() + " then substr(nvl(" + pData + ",' '),1," + (pMaxLen - 3).ToString() + ") || '...' else nvl(" + pData + ",' ') end";

                    break;

                case DbSvrType.dbSQL:
                    ret += "case when len(isnull(" + pData + ",' ')) > " + pMaxLen.ToString() + " then substring(isnull(" + pData + ",' '),1," + (pMaxLen - 3).ToString() + ") || '...' else isnull(" + pData + ",' ') end";
                    break;

                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }

        
        #region SQLGetExcept
        /// <summary>
        /// Retourne MINUS ou except en fonction du moteur SQL
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string SQLGetExcept(string pConnectionString)
        {
            return SQLGetExcept(GetDbSvrType(pConnectionString));
        }
        public static string SQLGetExcept(IDbTransaction pDbTransaction)
        {
            return SQLGetExcept(GetDbSvrType(pDbTransaction));
        }
        public static string SQLGetExcept(DbSvrType pDbSvrType)
        {
            switch (pDbSvrType)
            {
                case DbSvrType.dbORA:
                    return "MINUS";
                case DbSvrType.dbSQL:
                    return "except";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion SQLGetExcept
        #region SQLGetDate
        /// <summary>
        /// Retourne l'expression SQL qui retoune la date système 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20141208 [20549] Modify 
        public static string SQLGetDate(string pCS)
        {
            return SQLGetDate(GetDbSvrType(pCS), false);
        }
        /// <summary>
        /// Retourne l'expression SQL qui retoune la date système 
        /// </summary>
        /// <param name="pDbSvrType">Type de moteur SQL</param>
        /// <returns></returns>
        /// FI 20141208 [20549] Modify 
        public static string SQLGetDate(DbSvrType pDbSvrType)
        {
            return SQLGetDate(pDbSvrType, false);
        }

        /// <summary>
        /// Retourne l'expression SQL qui retoune la date système 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsUTC">si true, l'expression SQL retourne l'heure au format UTC</param>
        /// <returns></returns>
        /// FI 20141208 [20549] Modify 
        public static string SQLGetDate(string pCS, Boolean pIsUTC)
        {
            return SQLGetDate(GetDbSvrType(pCS), pIsUTC);
        }
      
        /// <summary>
        /// Retourne l'expression SQL qui retoune la date système 
        /// </summary>
        /// <param name="pDbSvrType">Type de moteur SQL</param>
        /// <param name="pIsUTC">si true, l'expression SQL retourne l'heure au format UTC</param>
        /// <returns></returns>
        /// FI 20141208 [20549] Modify 
        public static string SQLGetDate(DbSvrType pDbSvrType, Boolean pIsUTC)
        {
            string ret;
            switch (pDbSvrType)
            {
                case DbSvrType.dbORA:
                    if (pIsUTC)
                        ret = "CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) as TIMESTAMP)"; // FI 20200811 [XXXXX] use type TIMESTAMP (meilleure précision)
                    else
                        ret = "SYSDATE";
                    break;
                case DbSvrType.dbSQL:
                    if (pIsUTC)
                        ret = "getutcdate()";
                    else
                        ret = "getdate()";
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("dbType :{0} is not implemented", pDbSvrType.ToString()));
            }
            return ret;
        }

        // EG 20201125 [XXXXX] CAST Datetime2 to DateTime Column (SQLSERVER ONLY : Substract not possible between DateTime2 and Datetime)
        public static string SQLDatetime2ToDateTime(string pCS, string pColumn)
        {
            return SQLDatetime2ToDateTime(pCS, pColumn, true);
        }
        // EG 20201125 [XXXXX] CAST Datetime2 to DateTime Column (SQLSERVER ONLY : Substract not possible between DateTime2 and Datetime)
        public static string SQLDatetime2ToDateTime(string pCS, string pColumn, Boolean pIsDatetime2)
        {
            return SQLDatetime2ToDateTime(GetDbSvrType(pCS), pColumn, pIsDatetime2);
        }
        // EG 20201125 [XXXXX] CAST Datetime2 to DateTime Column (SQLSERVER ONLY : Substract not possible between DateTime2 and Datetime)
        public static string SQLDatetime2ToDateTime(DbSvrType pDbSvrType, string pColumn, Boolean pIsDatetime2)
        {
            string ret = pColumn;
            if (pDbSvrType == DbSvrType.dbSQL)
                ret = String.Format("Cast({0} as datetime)", pColumn);
            return ret;
        }
        /// <summary>
        /// Retourne L'expression SQL qui donne la date système du SGBD sans l'heure 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string SQLGetDateNoTime(string pConnectionString)
        {
            return SQLGetDateNoTime(GetDbSvrType(pConnectionString), false);
        }


        /// <summary>
        /// Retourne l'expression SQL qui donne la date système du SGBD sans l'heure 
        /// </summary>
        /// <param name="pDbSvrType"></param>
        /// <param name="pIsUTC"></param>
        /// <returns></returns>
        /// FI 20200820 [25468] add parameter pIsUTC
        public static string SQLGetDateNoTime(DbSvrType pDbSvrType, Boolean pIsUTC)
        {
            string ret;
            switch (pDbSvrType)
            {
                case DbSvrType.dbORA:
                    if (pIsUTC)
                        ret = "TRUNC(SYS_EXTRACT_UTC(SYSTIMESTAMP))";
                    else
                        ret = "TRUNC(SYSDATE)";
                    break;
                case DbSvrType.dbSQL:
                    if (pIsUTC)
                        ret = "convert(date,getutcdate())";
                    else
                        ret = "convert(date,getdate())";
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("dbType :{0} is not implemented", pDbSvrType.ToString()));
            }
            return ret;
        }
        #endregion SQLGetDate
        #region SQLGetElapsedTime
        /// <summary>
        /// Retourne une commande SQL, en fonction du moteur SGBD, qui permet de calculer le temps passé (en millisecondes) entre:
        /// <para>- Une date début: exprimée par {pDtStartColumn}</para>
        /// <para>- Une date fin: exprimée par {pDtEndColumn}</para>
        /// <para>Remarques pour le SGBD Oracle:</para>
        /// <para>- Le type DATE ne contient pas les millisecondes, le résultat est donné en millisecondes dans le but d'être homogène avec SQL Server</para>
        /// <para>- 86 400 000 = 24 (heures/jour) * 60 (minutes/heure) * 60 (secondes/minutes) * 1000 (ms/seconde</para>
        /// </summary>
        /// <param name="pDbSvrType"></param>
        /// <param name="pDtStartColumn"></param>
        /// <param name="pDtEndColumn"></param>
        /// <returns></returns>
        public static string SQLGetElapsedTime(DbSvrType pDbSvrType, string pDtStartColumn, string pDtEndColumn)
        {
            switch (pDbSvrType)
            {
                case DbSvrType.dbORA:
                    return "((" + pDtEndColumn + " - " + pDtStartColumn + ") * 86400000)";
                case DbSvrType.dbSQL:
                    return "(datediff(ms, " + pDtStartColumn + ", " + pDtEndColumn + "))";
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("dbType :{0} is not implemented", pDbSvrType.ToString()));
            }
        }
        #endregion SQLGetElapsedTime
        #region SQLIsNull
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pSubstitute"></param>
        /// <returns></returns>
        public static string SQLIsNull(string pConnectionString, string pCol, string pSubstitute, string pAlias)
        {

            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = "nvl(" + pCol + "," + pSubstitute + ")";
                    break;
                case DbSvrType.dbSQL:
                    ret = "isnull(" + pCol + "," + pSubstitute + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }

            if (StrFunc.IsFilled(pAlias))
                ret = ret + SQLCst.AS + pAlias;
            return ret;

        }
        public static string SQLIsNull(string pConnectionString, string pCol, string pSubstitute)
        {
            return SQLIsNull(pConnectionString, pCol, pSubstitute, string.Empty);
        }

        #endregion SQLIsNull
        #region SQLIsNullChar
        public static string SQLIsNullChar(string pConnectionString, string pCol, string pStringSubstitute, string pAlias)
        {
            return SQLIsNull(pConnectionString, pCol, DataHelper.SQLString(pStringSubstitute), pAlias);
        }
        public static string SQLIsNullChar(string pConnectionString, string pCol, string pStringSubstitute)
        {
            return SQLIsNull(pConnectionString, pCol, DataHelper.SQLString(pStringSubstitute), string.Empty);
        }
        public static string SQLIsNullChar(string pConnectionString, string pCol)
        {
            return SQLIsNull(pConnectionString, pCol, DataHelper.SQLString(string.Empty), string.Empty);
        }
        #endregion SQLIsNullChar
        #region SQLGetContains
        /// <summary>
        /// Retourne une commande SQL, en fonction du moteur SGBD, 
        /// pour indiquer si la donnée de type caractère {pSearchSqlString} apparaît dans une colonne {pColumnName} 
        /// contenant des données de type caractère et séparées par un délimiteur {pDelimiter}.
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pColumnName">Nom de la colonne dans laquelle s'effectuera la recherche</param>
        /// <param name="pDelimiter">Délimiteur des données de la colonne</param>
        /// <param name="pSearchSqlString">La donnée de type caractère à chercher, soit un paramètre, soit une chaine de caractères encadrée par des "'"</param>
        /// <returns></returns>
        public static string SQLGetContains(string pConnectionString, string pColumnName, string pDelimiter, string pSearchSqlString)
        {
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    return "(('" + pDelimiter + "' || " + pColumnName + " || '" + pDelimiter + "') like ('%" + pDelimiter + "' || " + pSearchSqlString + " || '" + pDelimiter + "%'))";
                case DbSvrType.dbSQL:
                    return "(('" + pDelimiter + "' + " + pColumnName + " + '" + pDelimiter + "') like ('%" + pDelimiter + "' + " + pSearchSqlString + " + '" + pDelimiter + "%'))";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion SQLGetContains
        #region SQLLength
        /// <summary>
        /// Renvoi la fonction SQL de longueur selon le SGBD
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <returns>La fonction SQL de longueur selon le SGBD, plus de la donnée entre parenthèses</returns>
        public static string SQLLength(string pConnectionString, string pData)
        {
            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = " length(" + pData + ")";
                    break;
                case DbSvrType.dbSQL:
                    ret = " len(" + pData + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }
        #endregion SQLLength
        #region SQLLike
        public static string SQLLike(string pString, bool pAddCharPercent, CompareEnum pCompare)
        {
            string expression = string.Empty;
            //
            if (StrFunc.IsFilled(pString))
                expression = pString;
            //
            switch (pCompare)
            {
                case CompareEnum.Lower:
                    expression = expression.ToLower();
                    break;
                case CompareEnum.Upper:
                    expression = expression.ToUpper();
                    break;
            }
            //
            if (pAddCharPercent)
                expression += "%";
            //	
            return SQLCst.LIKE + SQLString(expression);
        }
        public static string SQLLike(string pString, bool pAddCharPercent)
        {
            return SQLLike(pString, pAddCharPercent, CompareEnum.Normal);
        }
        public static string SQLLike(string pString, CompareEnum pCompare)
        {
            return SQLLike(pString, false, pCompare);
        }
        public static string SQLLike(string pString)
        {
            return SQLLike(pString, false, CompareEnum.Normal);
        }
        #endregion SQLLike
        #region SQLNumber
        /// <summary>
        /// Permet de retourner la [string] 'null' lorsque le nombre représenté par pNumber vaut null.
        /// Aucune vérification n'est faite pour savoir si pNumber représente bien un nombre.
        /// </summary>
        /// <param name="pNumber"></param>
        /// <returns>La [string] pNumber qui représente un nombre ou 'null' si pNumber vaut null</returns>
        public static string SQLNumber(string pNumber)
        {
            string ret = pNumber;

            if (null == ret)
                ret = SQLCst.NULL;
            //
            return ret;
        }
        #endregion SQLNumber
        #region SQLNumberToChar
        public static string SQLNumberToChar(string pConnectionString, int pNumber)
        {
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    return "TO_CHAR(" + pNumber.ToString() + ")";
                case DbSvrType.dbSQL:
                    return "convert(varchar," + pNumber.ToString() + ")";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        public static string SQLNumberToChar(string pConnectionString, string pNumber)
        {
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    return "TO_CHAR(" + pNumber + ")";
                case DbSvrType.dbSQL:
                    return "convert(varchar," + pNumber + ")";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }

        }
        #endregion SQLNumberToChar
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pStart"></param>
        /// <param name="pLenght"></param>
        /// <returns></returns>
        public static string SQLRight(string pConnectionString, string pData, int pLenght)
        {
            string ret = string.Empty;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret += "substr";
                    pLenght *= -1;
                    break;

                case DbSvrType.dbSQL:
                    ret += "right";
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("dbType :{0} is not implemented", GetDbSvrType(pConnectionString).ToString()));
            }
            return ret += "(" + pData + "," + pLenght.ToString() + ")";
        }
        #region SQLReplace
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pData"></param>
        /// <param name="pSubstitute"></param>
        /// <returns></returns>
        public static string SQLReplace(string pConnectionString, string pEltSource, string pSearch, string pReplace)
        {
            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = "replace(" + pEltSource + "," + pSearch + "," + pReplace + ")";
                    break;
                case DbSvrType.dbSQL:
                    ret = "replace(" + pEltSource + "," + pSearch + "," + pReplace + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }
        #endregion SQLReplace


        #region SQLRTrim
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pColSql"></param>
        /// <returns></returns>
        public static string SQLRTrim(string pConnectionString, string pColSql)
        {
            return SQLRTrim(pConnectionString, pColSql, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pColSql"></param>
        /// <param name="pAlias"></param>
        /// <returns></returns>
        public static string SQLRTrim(string pConnectionString, string pColSql, string pAlias)
        {
            string ret;

            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                case DbSvrType.dbSQL:
                    ret = "RTRIM(" + pColSql + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }

            if (StrFunc.IsFilled(pAlias))
                ret += SQLCst.AS + pAlias;

            return ret;
        }
        #endregion SQLTrim
        #region SQLString
        public static string SQLString(string pString)
        {
            return SQLString(pString, string.Empty, CompareEnum.Normal);
        }
        public static string SQLString(string pString, CompareEnum pCompare)
        {
            return SQLString(pString, string.Empty, pCompare);
        }
        public static string SQLString(string pString, string pAlias)
        {
            return SQLString(pString, pAlias, CompareEnum.Normal);
        }
        public static string SQLString(string pString, string pAlias, CompareEnum pCompare)
        {
            string ret = pString;

            if (null == ret)
                ret = SQLCst.NULL;
            else
                ret = "'" + ret.Replace("'", "''") + "'";
            //
            switch (pCompare)
            {
                case CompareEnum.Lower:
                    ret = ret.ToLower();
                    break;
                case CompareEnum.Upper:
                    ret = ret.ToUpper();
                    break;
            }
            //
            if (StrFunc.IsFilled(pAlias))
                ret = ret + SQLCst.AS + pAlias;
            //
            return ret;
        }
        #endregion SQLString
        #region SQLAddTime

        /// <summary>
        /// Get the SQL script to add the time part of the pDateColumnWithTime parameter to the pDateColumnWithoutTime parameter
        /// </summary>
        /// <param name="pConnectionString">the actual DBMS string</param>
        /// <param name="pDateTimeDestination">an SQL column/parameter containing a datetime value, 
        /// the time part of pDateTimeSource will be added to that</param>
        /// <param name="pDateTimeSource">an SQL column/parameter containing a datetime value, 
        /// the time part will extracted from this parameter</param>
        /// <returns>a SQL string containing the script in order to pass the time part of pDateTimeSource to  pDateTimeDestination</returns>
        public static string SQLAddTime(string pConnectionString, string pDateTimeDestination, string pDateTimeSource)
        {
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    {
                        return
                            String.Format(@"{0} + ({1} - TRUNC({1}))",
                            pDateTimeDestination, pDateTimeSource);
                    }
                case DbSvrType.dbSQL:
                    {
                        return
                            String.Format(@"DATEADD(minute, DATEPART(minute, {1}), DATEADD(hour, DATEPART(hour, {1}), {0}))",
                            pDateTimeDestination, pDateTimeSource);
                    }
                default:
                    throw (new ArgumentException("DBMS type unknown inside of the current connection string", "connectionString"));
            }
        }

        #endregion
        #region SQLToDate

        /// <summary>
        /// Convertion en date (sans heure) d'un DateTime c#
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDate"></param>
        public static string SQLToDate(string pConnectionString, DateTime pDate)
        {
            return SQLToDate(GetDbSvrType(pConnectionString), pDate);
        }
        /// <summary>
        /// Convertion en date (sans heure) d'un DateTime c#
        /// </summary>
        /// <param name="pDbSvrType"></param>
        /// <param name="pDate"></param>
        public static string SQLToDate(DbSvrType pDbSvrType, DateTime pDate)
        {
            if (pDate.Date == DateTime.MinValue)
                return SQLCst.NULL;
            else
                return SQLToDate(pDbSvrType, pDate.ToString(DtFunc.FmtDateyyyyMMdd));
        }
        /// <summary>
        /// Convertion en date (sans heure) d'une string représentant une date au format yyyyMMdd
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pISODate"></param>
        public static string SQLToDate(string pConnectionString, string pISODate)
        {
            return SQLToDate(GetDbSvrType(pConnectionString), pISODate);
        }

        /// <summary>
        /// Convertion en date (sans heure) d'une string représentant une date au format yyyyMMdd
        /// </summary>
        /// <param name="pSvrType"></param>
        /// <param name="pISODate">date au format yyyyMMdd</param>
        /// <returns></returns>
        public static string SQLToDate(DbSvrType pSvrType, string pISODate)
        {
            //PL 20120116
            if (pISODate != SQLCst.NULL)
                pISODate = DataHelper.SQLString(pISODate);
            switch (pSvrType)
            {
                case DbSvrType.dbORA:
                    return "TO_DATE(" + pISODate + ",'YYYYMMDD')";
                case DbSvrType.dbSQL:
                    return "convert(datetime," + pISODate + ",112)";
                default:
                    throw new NotImplementedException($"Type:{pSvrType} is not implemented");
            }
        }
        #endregion SQLToDate
        #region SQLToDateTime
        /// <summary>
        /// Convertion en date (précision à la seconde) d'un DateTime c#
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDateTime"></param>
        public static string SQLToDateTime(string pConnectionString, DateTime pDateTime)
        {
            return SQLToDateTime(GetDbSvrType(pConnectionString), pDateTime);
        }
        /// <summary>
        /// Convertion en date (précision à la seconde) d'un DateTime c#
        /// </summary>
        /// <param name="pSvrType"></param>
        /// <param name="pDateTime"></param>
        public static string SQLToDateTime(DbSvrType pSvrType, DateTime pDateTime)
        {
            if (pDateTime.Date == DateTime.MinValue)
                return SQLCst.NULL;
            else
                return SQLToDateTime(pSvrType, DtFunc.DateTimeToStringISO(pDateTime));
        }
        /// <summary>
        /// Convertion en date (précision à la seconde) d'une string représentant une date
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pISODateTime">date au format yyyy-MM-ddTHH:mm:ss</param>
        /// <returns></returns>
        public static string SQLToDateTime(string pConnectionString, string pISODateTime)
        {
            return SQLToDateTime(GetDbSvrType(pConnectionString), pISODateTime);
        }
        /// <summary>
        /// Convertion en date (précision à la seconde) d'une string représentant une date
        /// </summary>
        /// <param name="pSvrType"></param>
        /// <param name="pISODateTime">date au format yyyy-MM-ddTHH:mm:ss</param>
        /// <returns></returns>
        /// EG 20220915 [XXXXX][WI417] Colonnes d'audit d'Oracle de DATE en TIMESTAMP(7)
        public static string SQLToDateTime(DbSvrType pSvrType, string pISODateTime)
        {
            //ISO8601 : yyyy-mm-ddThh:mm:ss.mmm //http://www.pvv.ntnu.no/~nsaa/ISO8601.html
            switch (pSvrType)
            {
                case DbSvrType.dbORA:
                    //20060606 PL Warning, Oracle n'accepte pas ce format avec le "T"
                    pISODateTime = pISODateTime.Replace("T", " ");
                    return "TO_TIMESTAMP('" + pISODateTime + @"','YYYY-MM-DD HH24:MI:SS')";
                case DbSvrType.dbSQL:
                    return "convert(datetime,'" + pISODateTime + "',126)";
                default:
                    throw new NotImplementedException($"Type:{pSvrType} is not implemented");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pODBCCanoniqueDateTime"></param>
        /// <returns></returns>
        /// EG 20220915 [XXXXX][WI417] Colonnes d'audit d'Oracle de DATE en TIMESTAMP(7)
        public static string SQLToDateTime_ODBC(string pConnectionString, string pODBCCanoniqueDateTime)
        {
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    return "TO_TIMESTAMP('" + pODBCCanoniqueDateTime + "','YYYY-MM-DD HH24:MI:SS')";
                case DbSvrType.dbSQL:
                    //Code 120 --> yyyy-mm-dd hh:mi:ss(24h)
                    return "convert(datetime,'" + pODBCCanoniqueDateTime + "',120)";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion SQLToDateTime
        #region SQLToTime2
        /// <summary>
        /// Convertion en datetime2 (SQLServer)/timestamp(Oracle) (précision à la nanoseconde) d'un DateTime c#
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDateTime">Représente l'horodatage</param>
        /// FI 20220805 [XXXXX] Add
        public static string SQLToDateTime2(string pConnectionString, DateTime pDateTime)
        {
            return SQLToDateTime2(GetDbSvrType(pConnectionString), pDateTime);
        }
        /// <summary>
        /// Convertion en datetime2 (SQLServer)/timestamp(Oracle) (précision à la nanoseconde) d'un DateTime c#
        /// </summary>
        /// <param name="pSvrType"></param>
        /// <param name="pDateTime">Représente l'horodatage</param>
        /// FI 20220805 [XXXXX] Add
        public static string SQLToDateTime2(DbSvrType pSvrType, DateTime pDateTime)
        {
            if (pDateTime.Date == DateTime.MinValue)
                return SQLCst.NULL;
            else
                return SQLToDateTime2(pSvrType, DtFunc.DateTimeToString(pDateTime, "yyyy-MM-ddTHH:mm:ss.fffffff"));
        }
        /// <summary>
        /// Convertion en datetime2 (SQLServer)/timestamp(Oracle) (précision à la nanoseconde) d'une string représentant un horodatage
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pISODateTime">date au format yyyy-MM-ddTHH:mm:ss.fffffff</param>
        /// <returns></returns>
        /// FI 20220805 [XXXXX] Add
        public static string SQLToDateTime2(string pConnectionString, string pISODateTime)
        {
            return SQLToDateTime2(GetDbSvrType(pConnectionString), pISODateTime);
        }
        /// <summary>
        /// Convertion en datetime2 (SQLServer)/timestamp(Oracle) (précision à la nanoseconde) d'une string représentant un horodatage
        /// </summary>
        /// <param name="pSvrType"></param>
        /// <param name="pISODateTime">date au format yyyy-MM-ddTHH:mm:ss.fffffff</param>
        /// <returns></returns>
        /// FI 20220805 [XXXXX] Add
        public static string SQLToDateTime2(DbSvrType pSvrType, string pISODateTime)
        {
            switch (pSvrType)
            {
                case DbSvrType.dbORA:
                    //Oracle n'accepte pas ce format avec le "T"
                    pISODateTime = pISODateTime.Replace("T", " ");
                    return $"TO_TIMESTAMP('{pISODateTime}','YYYY-MM-DD HH24:MI:SS.FF')";
                case DbSvrType.dbSQL:
                    return $"Cast('{pISODateTime}'as datetime2(7))";
                default:
                    throw new NotImplementedException($"Type:{pSvrType} is not implemented");
            }
        }
        #endregion SQLToDateTime2

        #region SQLFormatColumnDateInIso
        public static string SQLFormatColumnDateInIso(string pConnectionString, string pColumnDate)
        {
            //FmtISODate
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    return "TO_CHAR(" + pColumnDate + ",'YYYY-MM-DD')";
                case DbSvrType.dbSQL:
                    return "convert(varchar(10)," + pColumnDate + ",120)";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion
        #region SQLFormatColumnTimeInIso
        public static string SQLFormatColumnTimeInIso(string pConnectionString, string pColumnTime)
        {
            //FmtISODate
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    return "TO_CHAR(" + pColumnTime + ",'HH24:MI:SS')";
                case DbSvrType.dbSQL:
                    return "convert(varchar(10)," + pColumnTime + ",108)";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion
        #region SQLFormatColumnDateTimeInIso
        public static string SQLFormatColumnDateTimeInIso(string pConnectionString, string pColumnDateTime)
        {
            //FmtISODate
            return SQLFormatColumnDateTimeInIso(pConnectionString, pColumnDateTime, "T");
        }
        public static string SQLFormatColumnDateTimeInIso(string pConnectionString, string pColumnDateTime, string pSeparator)
        {
            //FmtISODate
            return DataHelper.SQLConcat(pConnectionString,
                SQLFormatColumnDateInIso(pConnectionString, pColumnDateTime),
                DataHelper.SQLString(pSeparator),
                SQLFormatColumnTimeInIso(pConnectionString, pColumnDateTime));
        }
        #endregion
        #region SQLFormatColumnDateTimeWithMilli
        public static string SQLFormatColumnDateTimeWithMilli(string pConnectionString, string pColumnDateTime, bool pMilli)
        {
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    // TODO MF 20101214 - To test
                    if (pMilli)
                        return "TO_CHAR(" + pColumnDateTime + ",'YYYY-MM-DD HH24:MI:SS.FF')";
                    else
                        return "TO_CHAR(" + pColumnDateTime + ",'YYYY-MM-DD HH24:MI:SS')";
                case DbSvrType.dbSQL:
                    if (pMilli)
                        return "substring(convert(varchar(23)," + pColumnDateTime + ",126) || '.000',1,23)";
                    else
                        return "convert(varchar(19)," + pColumnDateTime + ",126)";
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion
        #region SQLUpper
        public static string SQLUpper(string pConnectionString, string pData)
        {
            return SQLUpper(pConnectionString, pData, null);
        }
        public static string SQLUpper(string pConnectionString, string pData, string pAlias)
        {
            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = " UPPER(" + pData + ")";
                    break;
                case DbSvrType.dbSQL:
                    ret = " upper(" + pData + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }

            if (StrFunc.IsFilled(pAlias))
                ret = ret + SQLCst.AS + pAlias;
            return ret;
        }
        #endregion SQLUpper
        #region SQLColumnXML
        public static string SQLColumnXML(string pConnectionString, string pColumnName)
        {
            string columnAlias = string.Empty;
            string ret = SQLColumnXML(pConnectionString, pColumnName, ref columnAlias);
            //
            return ret + columnAlias;
        }
        public static string SQLColumnXML(string pConnectionString, string pColumnName, ref string pColumnAlias)
        {
            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = " sys.xmltype.getCLOBVal(" + pColumnName + ") ";
                    if (StrFunc.IsEmpty(pColumnAlias))
                    {
                        if (pColumnName.IndexOf(".") >= 0)
                            pColumnAlias = SQLCst.AS + pColumnName.Substring(pColumnName.LastIndexOf(".") + 1);
                        else
                            pColumnAlias = SQLCst.AS + pColumnName;
                    }
                    break;
                case DbSvrType.dbSQL:
                    ret = pColumnName;
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }
        #endregion SQLColumnXML
        #region SQLLower
        public static string SQLLower(string pConnectionString, string pData)
        {
            return SQLLower(pConnectionString, pData, null);
        }
        public static string SQLLower(string pConnectionString, string pData, string pAlias)
        {
            string ret;
            switch (GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbORA:
                    ret = " LOWER(" + pData + ")";
                    break;
                case DbSvrType.dbSQL:
                    ret = " lower(" + pData + ")";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }

            if (StrFunc.IsFilled(pAlias))
                ret = ret + SQLCst.AS + pAlias;
            return ret;
        }
        #endregion SQLLower
        //
        #region public SQLFromDual
        public static string SQLFromDual(string pCS)
        {
            return SQLFromDual(GetDbSvrType(pCS));
        }
        public static string SQLFromDual(DbSvrType pServerType)
        {
            switch (pServerType)
            {
                case DbSvrType.dbORA:
                    return SQLCst.FROM_DUAL;
                case DbSvrType.dbSQL:
                    return string.Empty;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion public SQLFromDual
        #region public SQLNOLOCK
        //EG 20111219 New
        public static string SQLNOLOCK(string pCS)
        {
            return SQLNOLOCK(GetDbSvrType(pCS));
        }
        public static string SQLNOLOCK(DbSvrType pServerType)
        {
            switch (pServerType)
            {
                case DbSvrType.dbORA:
                    return string.Empty;
                case DbSvrType.dbSQL:
                    return SQLCst.NOLOCK;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
        }
        #endregion public SQLNOLOCK

        /// <summary>
        /// Retourne l'expression SQL qui donne le 1er jour du mois
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCol"></param>
        /// <returns></returns>
        /// FI 20120803 
        public static string SQLFirstDayOfMonth(string pCs, string pCol)
        {
            string ret;
            switch (GetDbSvrType(pCs))
            {
                case DbSvrType.dbORA:
                    ret = "#DATE - TO_NUMBER(TO_CHAR(#DATE, 'DD')) + 1";
                    ret = ret.Replace("#DATE", pCol);
                    break;
                case DbSvrType.dbSQL:
                    ret = "DateAdd(DAY, -DAY(#DATE) +1,@DATE1)";
                    ret = ret.Replace("#DATE", pCol);
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'expression SQL qui donne le dernier jour du mois
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCol"></param>
        /// <returns></returns>
        /// FI 20120803
        public static string SQLLastDayOfMonth(string pCs, string pCol)
        {
            string ret;
            switch (GetDbSvrType(pCs))
            {
                case DbSvrType.dbORA:
                    ret = "Add_months(#DATE, 1)-TO_NUMBER(TO_CHAR(Add_months(#DATE, 1), 'DD'))";
                    ret = ret.Replace("#DATE", pCol);
                    break;
                case DbSvrType.dbSQL:
                    ret = "DateAdd(DAY, -DAY(DateAdd(MONTH, 1 , #DATE)), DateAdd(MONTH, 1 , #DATE))";
                    ret = ret.Replace("#DATE", pCol);
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }

        /// <summary>
        /// Replace les éventuels parametres par leurs valeurs
        /// </summary>
        /// <param name="pQuery">Query SQL</param>
        /// <param name="pServerType">Type de Server RDBMS</param>
        /// <param name="pDataParameter">Liste des paramètres</param>
        /// <returns></returns>
        public static string ReplaceParametersInQuery(string pQuery, DbSvrType pServerType, IDataParameter[] pDataParameter)
        {
            return ReplaceParametersInQuery(pQuery, pServerType, false, pDataParameter);
        }

        /// <summary>
        /// Replace les éventuels parametres par leurs valeurs
        /// </summary>
        /// <param name="pQuery">Query SQL</param>
        /// <param name="pServerType">Type de Server RDBMS</param>
        /// <param name="pCommentParamName">Si true, conserve le nom du paramètre en commentaire </param>
        /// <param name="pDataParameter">Liste des paramètres</param>
        /// <returns></returns>
        /// FI 20220805 [XXXXX] Gestion de datetime2
        public static string ReplaceParametersInQuery(string pQuery, DbSvrType pServerType, bool pCommentParamName, IDataParameter[] pDataParameter)
        {
            string ret = pQuery;

            if (ArrFunc.IsFilled(pDataParameter))
            {
                IDataParameter[] dataParameter = null;
                
                //FI 20110422 [17415] La fonction fait un Replace (voir en fin de fonction)
                //                    IMPORTANT: Il convient donc de traiter les paramètres de nom le plus long au paramètre de nom le plus petit
                //                               Les paramètres sont donc ordonnés !
                
                //FI 20120225 [18441] Parfois pDataParameter est renseigné des item qui sont null
                //                    Recherche du paramètre 1er paramètre non null
                //                    Ceci du fait d'une intervention MF qui positionne à null les parameters Oracle lorsqu'ils sont non utilisés par une requête
                if (ArrFunc.Count(pDataParameter) > 1)
                {
                    Type t = null;
                    for (int i = 0; i < ArrFunc.Count(pDataParameter); i++)
                    {
                        if (null != pDataParameter[i])
                        {
                            t = pDataParameter[i].GetType();
                            break;
                        }
                    }

                    Array targetArray = null;
                    if (pServerType == DbSvrType.dbSQL)
                        targetArray = Array.CreateInstance(t, pDataParameter.Length);
                    else if (pServerType == DbSvrType.dbORA)
                        targetArray = Array.CreateInstance(t, pDataParameter.Length);
                    else
                        throw new NotImplementedException("RDBMS not implemented");

                    Array.Copy(pDataParameter, targetArray, pDataParameter.Length);
                    dataParameter = (IDataParameter[])targetArray;

                    Comparison<IDataParameter> comparison = new Comparison<IDataParameter>(DataHelper.CompareDataParameterName);
                    Array.Sort(dataParameter, comparison);
                }
                else
                {
                    dataParameter = pDataParameter;
                }

                const string commentParam = Cst.Space + SQLCst.SQL_ANSI_COMMENT_BEGIN + " P{0}: {1} " + SQLCst.SQL_ANSI_COMMENT_END;
                SortedList slParam = new SortedList();
                
                for (int j = ArrFunc.Count(dataParameter) - 1; j > -1; j--)
                {
                    IDataParameter param = dataParameter[j];

                    // 20100803 MF - lookat 'sql:variable' translation in TransformQuery_
                    if (param == null)
                        continue;

                    string value = string.Empty;
                    #region switch (param.DbType)
                    switch (param.DbType)
                    {
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.String:
                        case DbType.StringFixedLength:
                            if (!(param.Value is System.DBNull)) // FI 20130225
                            {
                                if ((null != param.Value) && (StrFunc.IsFilled(param.Value.ToString())))
                                    value = DataHelper.SQLString(param.Value.ToString());
                            }
                            break;

                        case DbType.Date:     //MPD Spheres => ce type existe uniquement sur Oracle
                        case DbType.DateTime:
                        case DbType.DateTime2: //ce type existe uniquement sur SQLServer
                        case DbType.Time:
                            if (!(param.Value is System.DBNull))// FI 20130225
                            {
                                DateTime dtValue = Convert.ToDateTime(param.Value);
                                if (DtFunc.IsDateTimeFilled(dtValue))
                                {
                                    if (param.DbType == DbType.Date)
                                        value = DataHelper.SQLToDate(pServerType, dtValue);
                                    else if (param.DbType == DbType.DateTime)
                                        value = DataHelper.SQLToDateTime(pServerType, dtValue);
                                    else if (param.DbType == DbType.DateTime2)
                                        value = DataHelper.SQLToDateTime2(pServerType, dtValue);
                                }
                            }
                            break;

                        case DbType.Boolean:
                            if (!(param.Value is System.DBNull))// FI 20130225
                                value = BoolFunc.IsTrue(param.Value) ? "1" : "0";
                            break;

                        case DbType.Decimal:
                            if ((null != param.Value) && !(param.Value is System.DBNull))// FI 20130225
                                value = StrFunc.FmtDecimalToInvariantCulture((decimal)param.Value);
                            break;

                        default:
                            //FI 20140321 [XXXX] sur Oracle DbType.Boolean n'existe pas et est remplacé par DbType.Int32
                            if ((param.DbType == DbType.Int32) && (param.Value is System.Boolean))
                            {
                                value = BoolFunc.IsTrue(param.Value) ? "1" : "0";
                            }
                            else
                            {
                                string otherValue = (null == param.Value ? string.Empty : param.Value.ToString());
                                if (StrFunc.IsFilled(otherValue))
                                    value = otherValue;
                            }
                            break;
                    }
                    #endregion 

                    if (StrFunc.IsEmpty(value))
                        value = SQLCst.NULL;

                    #region pCommentParamName
                    if (pCommentParamName)
                    {
                        for (int i = 0; i < ArrFunc.Count(pDataParameter); i++)
                        {
                            if (pDataParameter[i].ParameterName == param.ParameterName)
                            {
                                slParam.Add(string.Format("P{0}", i), string.Format(" P{0}: {1}={2}", i, param.ParameterName, value));
                                value += string.Format(commentParam, i, param.ParameterName);
                                break;
                            }
                        }
                    }
                    #endregion 

                    ret = ret.Replace(param.ParameterName, value);
                }

                if (pCommentParamName)
                {
                    string resumeParam = " NONE";
                    if (slParam.Count > 0)
                    {
                        resumeParam = string.Empty;
                        for (int i = 0; i < slParam.Count; i++)
                            resumeParam += slParam.GetByIndex(i);
                    }

                    if (IsQueryRDBMS(ret))
                        ret = ret.Replace(SQLCst.SQL_RDBMS + Cst.CrLf, SQLCst.SQL_RDBMS + Cst.CrLf + string.Format(SQLCst.SQL_PARAM, resumeParam)) + Cst.CrLf;
                    else if (IsQueryANSI(ret))
                        ret = ret.Replace(SQLCst.SQL_ANSI + Cst.CrLf, SQLCst.SQL_ANSI + Cst.CrLf + string.Format(SQLCst.SQL_PARAM, resumeParam)) + Cst.CrLf;
                    else 
                        ret = string.Format(SQLCst.SQL_PARAM, resumeParam) + Cst.CrLf + ret;
                }
            }
            return ret;
        }

        #region public TransformQuery
        /* FI 20201009 [XXXXX] Mise en commentaire
        /// <summary>
        /// Tranform a Transact-SQL query to PL/SQL query 
        /// </summary>
        /// <param name="pServerType">Type of SGBD/R (enum DbSvrType)</param>
        /// <param name="pCommandText">Query to transform</param>
        /// <returns>New query</returns>
        public static string TransformQuery(DbSvrType pServerType, IDbTransaction pDbTransaction, CommandType pCommandType, string pCommandText, params IDbDataParameter[] commandParameters)
        {
            return TransformQuery_(pServerType, pCommandType, pCommandText, pDbTransaction, commandParameters);
        }
        public static string TransformQuery(DbSvrType pServerType, IDbConnection pDbConnection, CommandType pCommandType, string pCommandText, params IDbDataParameter[] commandParameters)
        {
            return TransformQuery_(pServerType, pCommandType, pCommandText, pDbConnection.ConnectionString, commandParameters);
        }
        public static string TransformQuery(DbSvrType pServerType, string pConnectionString, CommandType pCommandType, string pCommandText, params IDbDataParameter[] commandParameters)
        {
            return TransformQuery_(pServerType, pCommandType, pCommandText, pConnectionString, commandParameters);
        }
        //
        private static string TransformQuery_(DbSvrType pServerType, CommandType pCommandType, string pCommandText, IDbTransaction pDbTransaction, params IDbDataParameter[] commandParameters)
        {
            return TransformQuery_(pServerType, pCommandType, pCommandText, pDbTransaction.Connection.ConnectionString, commandParameters);
        }
        /// <summary>
        /// Traduit une instruction SQL Spheres® en language SQL d'une DB  
        /// </summary>
        /// <param name="pServerType">Repésente la DB</param>
        /// <param name="pCommandType">Type de commande</param>
        /// <param name="pCommandText">Instruction SQL</param>
        /// <param name="pConnectionString">CS d'accès à la DB</param>
        /// <param name="commandParameters">paramètres rattachés à l'instruction SQL</param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        private static string TransformQuery_(DbSvrType pServerType, CommandType pCommandType, string pCommandText, string pConnectionString, params IDbDataParameter[] commandParameters)
        {
            string ret = string.Empty;
            CSManager conManager = new CSManager(pConnectionString);
            //
            bool isQueryRDBMS = IsQueryRDBMS(pCommandText);
            bool isQueryANSI = IsQueryANSI(pCommandText);
            //
            string command = pCommandText;
            //
            if (isQueryRDBMS)
            {
                ret = command;
            }
            else
            {
                //Spheres préfixe avec SQL_RDBMS pour indiquer que la query a été transformé
                if ((pCommandType != CommandType.StoredProcedure) && (pServerType != DbSvrType.dbOLEDB))
                {
                    //20050907 PL Warning: Pb Oracle, on ne préfixe pas avec un commentaire si stored procedure
                    //RD 20100521 Warning: Pb SQLServer AUSSI, on ne préfixe pas avec un commentaire si stored procedure
                    ret += SQLCst.SQL_RDBMS + Cst.CrLf;
                }
                //
                if ((pServerType != DbSvrType.dbOLEDB) && (pServerType != DbSvrType.dbORA))
                {
                    //Comme opérateur de concaténation de string Spheres® accepte l'opérateur d'Oracle® "||"
                    //On remplace donc cet opérateur par l'opérateur du moteur SQL 
                    //Ceci n'est fait que sur des serverType de type base de données

                    // 20101019 the Replace command search the exact input string, 
                    //      then || without spaces but including line breaks can not be found.
                    //      Solution: 
                    //      using a regexp to add more searching pattern and to maintain the original whitespace pattern of the string.
                    //command = command.Replace(" || ", GetDAL(pServerType).GetConcatOperator);

                    //command = findPipeOperators.Replace(command, String.Format("$1{0}$3", GetDAL(pServerType).GetConcatOperator));
                    // 20101020 MF - whitespaces existence is optional, we may use a straigh replace of the operator symbol
                    command = command.Replace("||", GetDAL(pServerType).GetConcatOperator);

                    //RD 20111110 Newness
                    //Comme table à une colonne, Spheres® accepte celle d'Oracle® "from DUAL"
                    //On remplace donc cette table par celle du moteur SQL en cours (SqlServer n'en posséde pas une)
                    //Ceci n'est fait que sur des serverType de type base de données
                    command = command.Replace(SQLCst.FROM_DUAL.Trim(), SQLFromDual(pServerType).Trim());

                }
                //
                if (pServerType == DbSvrType.dbORA)
                {
                    //PL 20110628 Newness
                    //Comme function isnull() Spheres® accepte la function isnull() de SQLServer®
                    //On remplace donc cette function par la function nvl() du moteur Oracle® 
                    command = command.Replace("isnull(", "nvl(");
                    //
                    //FI 20111221 Replace des convert(dateTime et des convert(varchar
                    //command = command.Replace("convert(varchar,", "TO_CHAR(");
                    command = ReplaceConvertToOracle(command);
                    //
                    //PL 20110826 Newness
                    //Comme function SysDate Spheres® accepte la function getdate() de SQLServer®
                    //On remplace donc cette function par la function SYSDATE du moteur Oracle® 
                    command = command.Replace("getdate()", "SYSDATE");
                    // FI 20200820 [25468] date systmes en UTC
                    command = command.Replace("getutcdate()", "SYS_EXTRACT_UTC(SYSTIMESTAMP)");
                    //
                    //EG 20111214 Newness
                    command = command.Replace("substring(", "substr(");
                    //EG 20150923 Newness
                    command = command.Replace("cast(0 as bigint)", "cast(0 as number(10))");
                    //PM 20130704 Newness
                    command = command.Replace("len(", "length(");
                    // FI 20170531 [23206] Remplacement des DATEADD 
                    command = ReplaceDateAddToOra(command);


                    //EG 20130712 Newness
                    if (command.IndexOf(SQLCst.EXCEPT.Trim(), StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        command = command.Replace("exception", "e$x$c$e$p$t$i$o$n").Replace(SQLCst.EXCEPT.Trim(), "minus").Replace("e$x$c$e$p$t$i$o$n", "exception");
                    }
                    //RD 20170803 [23248] Newness
                    if (command.IndexOf(SQLCst.NOLOCK.Trim(), StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        command = command.Replace(SQLCst.NOLOCK.Trim(), SQLNOLOCK(pServerType).Trim());
                    }
                }
                //
                if (pServerType == DbSvrType.dbSQL)
                {
                    //FI 20111221 Replace des Mod(x,y) par une syntaxe SQLServer
                    command = ReplaceModToSqlServer(command);
                }
                //
                // RD 20100520: Replace de "@" par ":"
                // Code anciennement dans DataAccess.ReplaceVarPrefix
                if ((pServerType != DbSvrType.dbOLEDB) && (pServerType != DbSvrType.dbSQL))
                {
                    //Comme préfixe de paramètre Spheres® accepte l'opérateur SQLServer "@"
                    //On remplace donc ce préfixe par le préfixe du moteur SQL 
                    //
                    if (ArrFunc.IsFilled(commandParameters))
                    {
                        foreach (IDbDataParameter param in commandParameters)
                        {
                            // FI 20130225 [18441] add du if (param == null)
                            // L'instruction continue est utilise parce que déjà utilisé plus loin
                            // Il est possible que param soit à null car cette même méthode positionne à null les parameters non utilisés
                            // voir plus loin dans la fonction commandParameters.SetValue(null, idx);
                            if (param == null)
                                continue;
                            //PL 20100716 Replace with IgnoreCase (Car le nom des paramètres est depuis tjs converti en UPPER dans StringDynamicData)
                            //command = command.Replace("@" + param.ParameterName.Replace(GetVarPrefix(pServerType), ""), param.ParameterName);
                            string nameInSQLServerFormat = param.ParameterName.Replace(GetVarPrefix(pServerType), "@");
                            command = Regex.Replace(command, nameInSQLServerFormat, param.ParameterName, RegexOptions.IgnoreCase);
                        }
                    }
                }
                //
                switch (pServerType)
                {
                    case DbSvrType.dbORA:
                        #region Oracle
                        //RD 20100521 code déplacer plus haut pour faire la même chose pour SQLServer
                        //
                        //Spheres Remplace DBO. par le SCHEMA Oracle
                        //string schema = conManager.GetAttribute("User ID");
                        //schema = " " + schema + ".";
                        string schema = " " + GetObjectsOwner(conManager) + ".";

                        command = command.Replace(SQLCst.SQL_ANSI + Cst.CrLf, string.Empty);//Warning: 20050330 PL Bug Oracle s'il existe plusieurs commentaire en début de query
                        command = command.Replace(SQLCst.SQL_ANSI, string.Empty);
                        command = command.Replace(SQLCst.DBO + "UF_DATAENABLED", schema + "UP_GENERAL.UF_DATAENABLED");

                        //20091026 PL Add If() avec Trim()
                        if (pCommandType == CommandType.StoredProcedure)
                            command = command.Replace(SQLCst.DBO.Trim(), schema.Trim());
                        else
                            command = command.Replace(SQLCst.DBO, schema);


                        if (commandParameters != null)
                        {
                            // 20100803 MF - 'sql:variable' oracle translation 
                            // 1. Find all the sql:variable references in the sql query
                            Regex regSQLVariable = new Regex("(sql:variable\\(\")([@:])(\\w*)(\"\\))");

                            foreach (Match matchVar in regSQLVariable.Matches(command))
                            {
                                // 2. Get the third matching group (\\w*) containing the var name
                                string strNom = matchVar.Groups[3].Captures[0].Value;
                                // 3. Searching the parameter strNom from the commandParameters collection
                                for (int idx = 0; idx < commandParameters.Length; idx++)
                                {
                                    IDbDataParameter param = commandParameters[idx];

                                    if (param == null)
                                        continue;

                                    if (Regex.Match(
                                            param.ParameterName,
                                            String.Format("^[@:]?({0})$", strNom)
                                        ).Success)
                                    {
                                        // 4. If the parameter exists then we replace 'sql:variable(@<var name>)' with '"<var value>"'
                                        switch (param.DbType)
                                        {
                                            // PM 20120920 [18058] Ajout de "" dans le cas de remplacement d'une variable de type String
                                            case DbType.AnsiString:
                                                command = Regex.Replace(
                                                    command,
                                                    String.Format("(sql:variable\\(\")([@:])?({0})(\"\\))", strNom),
                                                    "\"" + Convert.ToString(param.Value) + "\"");
                                                break;
                                            default:
                                                command = Regex.Replace(
                                                    command,
                                                    String.Format("(sql:variable\\(\")([@:])?({0})(\"\\))", strNom),
                                                    Convert.ToString(param.Value));
                                                break;
                                        }
                                    }
                                }
                            }

                            // 20101115 MF - Set null value for the parameters that are not used inside an SQL query CommandType.Text, 
                            //  to avoid the ORA-01036 error. 
                            if (pCommandType == CommandType.Text)
                                for (int idx = 0; idx < commandParameters.Length; idx++)
                                {
                                    IDbDataParameter param = commandParameters[idx];

                                    // FI 20130225 [18441] add du if (param == null)
                                    // Il est possible que param soit à null car cette même méthode positionne à null les parameters non utilisés
                                    // voir plus loin dans la fonction commandParameters.SetValue(null, idx);
                                    if (param == null)
                                        continue;

                                    // The regex search all the occurences of the current parameter name imposing the next constraint :
                                    //  the target string must be terminated at least with one character that is not a word character (/W+),
                                    //  otherwise we might match some strings containing the target string.    
                                    if (!Regex.Match(command, String.Format(@"({0})(\W+|$)", param.ParameterName)).Success)
                                        commandParameters.SetValue(null, idx);
                                }
                        }
                        // 'sql:variable' translation END - a simpler implementantion is done cycling on parameters and forcing blind replaces
                        ret += command;
                        #endregion
                        break;

                    case DbSvrType.dbSQL:
                        #region SQLServer
                        //command = command.Replace(SQLCst.FROM_DUAL, string.Empty);
                        ret += command;
                        #endregion
                        break;
                    case DbSvrType.dbOLEDB:
                        ret = command;
                        break;
                    default:
                        throw (new ArgumentException("No match for server type", "ServerType"));
                }
            }
            //
            return ret;
        }
        */
        /// <summary>
        /// Remplace dans une string les occurences "DATEADD(datepart,number,date)" en "Add_Months(date,number)" ou "date + number"
        /// </summary>
        /// <param name="pInput">string en entrée</param>
        /// <returns></returns>
        /// FI 20170531 [23206] add Method
        private static string ReplaceDateAddToOra(string pInput)
        {
            string ret = string.Empty;
            Regex regexDateAdd = new Regex(@"DATEADD\(\s*(DAY|MONTH|YEAR)\s*,\s*(\+|\-)?\s*(\d+)\s*,\s*(.+)\s*\)", RegexOptions.IgnoreCase);
            ret = regexDateAdd.Replace(pInput, new MatchEvaluator(DateAddToORA));
            return ret;
        }


        /// <summary>
        /// Méthode déléguée pour remplacer "DATEADD(datepart,number,date)" en "Add_Months(date,number)" ou "date + number"
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        /// FI 20170531 [23206] add Method
        private static string DateAddToORA(Match m)
        {
            string ret;
            if (false == (m.Groups.Count == 5))
                throw new NotSupportedException(StrFunc.AppendFormat("Value :{0} is not an valid for DateAdd expression", m.ToString()));

            string function = m.Groups[1].Value;
            string @operator = m.Groups[2].Value;
            string number = m.Groups[3].Value;
            string sqlCol = m.Groups[4].Value;

            //Si l'expression de la colonne SQL contient un "DATEADD(datepart,number,date)" alors on le remplace
            sqlCol = ReplaceDateAddToOra(sqlCol);

            switch (function.ToUpper())
            {
                case "YEAR":
                    ret = StrFunc.AppendFormat("Add_Months({0},{1}{2} * 12)", sqlCol, @operator, number);
                    break;
                case "MONTH":
                    ret = StrFunc.AppendFormat("Add_Months({0},{1}{2})", sqlCol, @operator, number);
                    break;
                case "DAY":
                    ret = StrFunc.AppendFormat("{0}{1}{2}", sqlCol, StrFunc.IsEmpty(@operator) ? "+" : "-", number);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemeted", function));
            }

            return ret;
        }

        /// <summary>
        /// Gérer la présence ou pas de la clause OrderBy dans la requête
        /// </summary>
        /// <param name="pQuery">la requête concernée</param>
        /// <param name="pOrderBy">la clause OrderBy</param>
        /// <param name="pIsSetOrderBy">true: Enrichir la requête avec la clause OrderBy, false: supprimer la clause OrderBy de la requête</param>
        /// <param name="pIsQueryWithUnion">si la requête contient un UNION</param>
        /// <returns></returns>
        public static string TransformQueryForOrderBy(string pQuery, string pOrderBy, bool pIsSetOrderBy, bool pIsQueryWithUnion)
        {
            // La requête suivante ne passe pas:
            //
            //      select col1 as value1 from tbl1
            //      union all  
            //      select col1 as value1 from tbl2
            //      order by case when value1 = 'xxx' then 'xxxX' else value1 end
            //    
            // Il faudrait l'écrire comme suit: 
            //
            //      select * from 
            //      (
            //          select col1 as value1 from tbl1
            //          union all  
            //          select col1 as value1 from tbl2
            //      ) tblForOrderBy
            //      order by case when value1 = 'xxx' then 'xxxX' else value1 end
            //
            string queryForOrderBy1 = SQLCst.SELECT + "*" + SQLCst.X_FROM + "(" + Cst.CrLf;
            string queryForOrderBy2 = ") tblForOrderBy";
            //
            string retQuery = pQuery;

            if (pIsSetOrderBy)
            {
                if (pIsQueryWithUnion)
                    retQuery = queryForOrderBy1 + retQuery + queryForOrderBy2;
                //
                retQuery += Cst.CrLf + pOrderBy;
            }
            else
            {
                retQuery = retQuery.Replace(Cst.CrLf + pOrderBy, string.Empty);
                //
                if (pIsQueryWithUnion)
                {
                    retQuery = retQuery.Replace(queryForOrderBy1, string.Empty);
                    retQuery = retQuery.Replace(queryForOrderBy2, string.Empty);
                }
            }
            //
            return retQuery;
        }

        /// <summary>
        /// Remplace ds la commande SQL les "convert(varchar,xxxxxxx" et les "convert(datetime, xxxxxxxxxxx" par l'equivalence sous Oracle
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static string ReplaceConvertToOracle(string command)
        {
            int guard, posConvert, posClosingParenthesis, posOpeningParenthesis;
            string cmd, oracle_cmd;

            //PL 20120307 Retour arrière suite à plantage sur déposit ex.: ae.IDASSET in (13,32,4,23,12,15,126)
            //20120307 command = command.Replace("convert(datetime,", "TO_DATE(");
            //20120307 command = command.Replace("convert(varchar,", "TO_CHAR(");
            //20120307 command = command.Replace(",112)", ",'YYYYMMDD')");
            //20120307 command = command.Replace(",126)", ",'YYYY-MM-DD HH24:MI:SS')");
            //20120307 command = command.Replace(",120)", ",'YYYY-MM-DD HH24:MI:SS')");

            //FI 20121221 Mis en commentaire de l'ancienne approche
            //Elle ne fonctionne pas si l'expression à l'intétieur du convert contient ")"
            //Exemple : convert(varchar,%%DA:DATE1.GetDataSQLValue()%%,112)
            #region convert(datetime,...
            guard = 0;
            posConvert = command.IndexOf("convert(datetime,");
            while ((++guard < 999) && (posConvert > -1))
            {
                posClosingParenthesis = command.IndexOf(')', posConvert);

                //Recherche d'une éventuelle parenthèse ouvrante intermédiaire 
                posOpeningParenthesis = command.IndexOf('(', posConvert + "convert(".Length);
                while ((posOpeningParenthesis > -1) && (posOpeningParenthesis < posClosingParenthesis))
                {
                    //Recherche de la parenthèse fermante liée à la parenthèse ouvrante intermédiaire
                    posClosingParenthesis = command.IndexOf(')', posOpeningParenthesis);
                    //Recherche de la prochaine parenthèse fermante 
                    posClosingParenthesis = command.IndexOf(')', posClosingParenthesis + 1);

                    //Recherche d'une autre éventuelle parenthèse ouvrante intermédiaire 
                    posOpeningParenthesis = command.IndexOf('(', posOpeningParenthesis + "(".Length);
                }

                cmd = command.Substring(posConvert, posClosingParenthesis - posConvert + 1);

                oracle_cmd = cmd.Replace(", ", ",").Replace(" )", ")");
                oracle_cmd = oracle_cmd.Replace("convert(datetime,", "TO_DATE(");
                oracle_cmd = oracle_cmd.Replace(",112)", ",'YYYYMMDD')");
                oracle_cmd = oracle_cmd.Replace(",126)", ",'YYYY-MM-DD HH24:MI:SS')");
                oracle_cmd = oracle_cmd.Replace(",120)", ",'YYYY-MM-DD HH24:MI:SS')");

                command = command.Replace(cmd, oracle_cmd);

                posConvert = command.IndexOf("convert(datetime,");
            }
            #endregion

            #region convert(varchar,...
            guard = 0;
            posConvert = command.IndexOf("convert(varchar,");
            while ((++guard < 999) && (posConvert > -1))
            {
                posClosingParenthesis = command.IndexOf(')', posConvert);

                //Recherche d'une éventuelle parenthèse ouvrante intermédiaire 
                posOpeningParenthesis = command.IndexOf('(', posConvert + "convert(".Length);
                while ((posOpeningParenthesis > -1) && (posOpeningParenthesis < posClosingParenthesis))
                {
                    //Recherche de la parenthèse fermante liée à la parenthèse ouvrante intermédiaire
                    posClosingParenthesis = command.IndexOf(')', posOpeningParenthesis);
                    //Recherche de la prochaine parenthèse fermante 
                    posClosingParenthesis = command.IndexOf(')', posClosingParenthesis + 1);

                    //Recherche d'une autre éventuelle parenthèse ouvrante intermédiaire 
                    posOpeningParenthesis = command.IndexOf('(', posOpeningParenthesis + "(".Length);
                }

                cmd = command.Substring(posConvert, posClosingParenthesis - posConvert + 1);

                oracle_cmd = cmd.Replace(", ", ",").Replace(" )", ")");
                oracle_cmd = oracle_cmd.Replace("convert(varchar,", "TO_CHAR(");
                oracle_cmd = oracle_cmd.Replace(",112)", ",'YYYYMMDD')");
                oracle_cmd = oracle_cmd.Replace(",126)", ",'YYYY-MM-DD HH24:MI:SS')");
                oracle_cmd = oracle_cmd.Replace(",120)", ",'YYYY-MM-DD HH24:MI:SS')");

                command = command.Replace(cmd, oracle_cmd);

                posConvert = command.IndexOf("convert(varchar,");
            }
            #endregion

            // RD 20200824 [25239] Add
            #region convert(int,...
            command = command.Replace("convert(int,", "TO_NUMBER(");
            #endregion

            return command;
        }
        /// <summary>
        /// Remplace la syntaxe Mod(x,y) par x%y spécifique à sqlServer
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static string ReplaceModToSqlServer(string command)
        {
            int guard = 0;
            while (command.Contains("mod(") & (guard < 100))
            {
                int posMod = command.IndexOf("mod(");
                int posParenthesis = command.IndexOf(')', posMod);
                if (posParenthesis == -1)
                    throw new Exception(StrFunc.AppendFormat("Method [{0}] {1} is expected", "mod", ")"));
                //
                int lenMod = "mod(".Length;
                string arg = command.Substring(posMod + lenMod, posParenthesis - posMod - lenMod);
                if (false == arg.Contains(","))
                    throw new Exception(StrFunc.AppendFormat("Method [{0}] 2 arguments are expected", "mod"));
                //
                string[] arg2 = arg.Split(',');
                if (ArrFunc.Count(arg2) != 2)
                    throw new Exception(StrFunc.AppendFormat("Method [{0}] 2 arguments are expected", "mod"));
                //
                command = command.Replace("mod(" + arg2[0] + "," + arg2[1] + ")", arg2[0] + "%" + arg2[1]);

                //
                guard++;
            }

            if (guard == 100)
                throw new Exception("Infinite Loop");

            return command;
        }

        /// <summary>
        /// Retourne une instruction SQL à partir d'une instruction SQL en language "Spheres®" 
        /// <para>Remarque :Les paramètres en entrées non utilisés sont remplacés par Null sous ORACLE (car sous Oracle® l'existence de paramètrès non utilisés provoque un plantage)</para>
        /// <para>Mis en cache par défaut des requêtes traduites pour éviter une convertion systématique. Le cache peut être débrayé via config avec la clé TransformQueryCacheEnabled</para>
        /// </summary>
        /// <param name="pConnectionString">CS d'accès à la DB</param>
        /// <param name="pCommandType">Type de commande</param>
        /// <param name="pCommandText">Instruction SQL en entrée</param>
        /// <param name="commandParameters">paramètres rattachés à l'instruction SQL en entrée</param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        /// FI 20201009 [XXXXX] Refactoring et usage d'un cache
        /// EG 20231005 [WI715][26497] Appel à la méthode SafeReplaceParameters (Pour ne faire qu'un remplacement exact des paramètres @PARAM => :PARAM pour Oracle)
        public static string TransformQuery2(string pConnectionString, CommandType pCommandType, string pCommandText, params IDbDataParameter[] commandParameters)
        {

            DbSvrType svrType = GetDbSvrType(pConnectionString);

            string ret;
            bool isQueryRDBMS = IsQueryRDBMS(pCommandText);
            //bool isQueryANSI = IsQueryANSI(pCommandText);

            TranformQueryCacheMode mode = TranformQueryCacheMode.Basic;
            string modeSetting = ConfigurationManager.AppSettings["TransformQueryCacheMode"];
            if (StrFunc.IsFilled(modeSetting))
                mode = (TranformQueryCacheMode)Enum.Parse(typeof(TranformQueryCacheMode), modeSetting, true);

            bool isCacheEnabled = (mode != TranformQueryCacheMode.None);

            if (isQueryRDBMS)
            {
                ret = pCommandText;
            }
            else
            {
                ret = null;

                Boolean isInCache = false;
                string key = string.Empty;

                if (isCacheEnabled)
                {
                    key = $"{svrType}-{pCommandText}";
                    switch (mode)
                    {
                        case TranformQueryCacheMode.Concurrent:
                            isInCache = _tranformQueryCacheConcurrent.TryGetValue(key, out ret);
                            break;
                        case TranformQueryCacheMode.Basic:
                            lock (((ICollection)_tranformQueryCacheBasic).SyncRoot)
                            {
                                isInCache = _tranformQueryCacheBasic.TryGetValue(key, out ret);
                            }
                            break;
                        default:
                            throw new NotSupportedException($"Mode : {mode} is not supported");
                    }

                    if (isInCache)
                    {
                        switch (svrType)
                        {
                            case DbSvrType.dbORA:
                                if ((pCommandType == CommandType.Text) && ArrFunc.IsFilled(commandParameters))
                                    ParametersNotUsedSetNull(ret, commandParameters);
                                break;
                        }
                    }
                }

                if (false == isInCache)
                {
                    string command;
                    //Spheres préfixe avec SQL_RDBMS pour indiquer que la query a été transformé
                    if ((pCommandType != CommandType.StoredProcedure) && (svrType != DbSvrType.dbOLEDB))
                    {
                        //20050907 PL Warning: Pb Oracle, on ne préfixe pas avec un commentaire si stored procedure
                        //RD 20100521 Warning: Pb SQLServer AUSSI, on ne préfixe pas avec un commentaire si stored procedure
                        command = SQLCst.SQL_RDBMS + Cst.CrLf + pCommandText;
                    }
                    else
                    {
                        command = pCommandText;
                    }

                    if ((svrType != DbSvrType.dbOLEDB) && (svrType != DbSvrType.dbORA))
                    {
                        //Comme opérateur de concaténation de string Spheres® accepte l'opérateur d'Oracle® "||"
                        //On remplace donc cet opérateur par l'opérateur du moteur SQL 
                        //Ceci n'est fait que sur des serverType de type base de données

                        // 20101019 the Replace command search the exact input string, 
                        //      then || without spaces but including line breaks can not be found.
                        //      Solution: 
                        //      using a regexp to add more searching pattern and to maintain the original whitespace pattern of the string.
                        //command = command.Replace(" || ", GetDAL(pServerType).GetConcatOperator);

                        //command = findPipeOperators.Replace(command, String.Format("$1{0}$3", GetDAL(pServerType).GetConcatOperator));
                        // 20101020 MF - whitespaces existence is optional, we may use a straigh replace of the operator symbol
                        command = command.Replace("||", GetDAL(svrType).GetConcatOperator);

                        //RD 20111110 Newness
                        //Comme table à une colonne, Spheres® accepte celle d'Oracle® "from DUAL"
                        //On remplace donc cette table par celle du moteur SQL en cours (SqlServer n'en posséde pas une)
                        //Ceci n'est fait que sur des serverType de type base de données
                        command = command.Replace(SQLCst.FROM_DUAL.Trim(), SQLFromDual(svrType).Trim());

                    }
                    //
                    if (svrType == DbSvrType.dbORA)
                    {
                        //PL 20110628 Newness
                        //Comme function isnull() Spheres® accepte la function isnull() de SQLServer®
                        //On remplace donc cette function par la function nvl() du moteur Oracle® 
                        command = command.Replace("isnull(", "nvl(");
                        //
                        //FI 20111221 Replace des convert(dateTime et des convert(varchar
                        //command = command.Replace("convert(varchar,", "TO_CHAR(");
                        command = ReplaceConvertToOracle(command);
                        //
                        //PL 20110826 Newness
                        //Comme function SysDate Spheres® accepte la function getdate() de SQLServer®
                        //On remplace donc cette function par la function SYSDATE du moteur Oracle® 
                        command = command.Replace("getdate()", "SYSDATE");
                        // FI 20200820 [25468] date systmes en UTC
                        command = command.Replace("getutcdate()", "SYS_EXTRACT_UTC(SYSTIMESTAMP)");
                        //
                        //EG 20111214 Newness
                        command = command.Replace("substring(", "substr(");
                        //EG 20150923 Newness
                        command = command.Replace("cast(0 as bigint)", "cast(0 as number(10))");
                        //PM 20130704 Newness
                        command = command.Replace("len(", "length(");
                        // FI 20170531 [23206] Remplacement des DATEADD 
                        command = ReplaceDateAddToOra(command);


                        //EG 20130712 Newness
                        if (command.IndexOf(SQLCst.EXCEPT.Trim(), StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            command = command.Replace("exception", "e$x$c$e$p$t$i$o$n").Replace(SQLCst.EXCEPT.Trim(), "minus").Replace("e$x$c$e$p$t$i$o$n", "exception");
                        }
                        //RD 20170803 [23248] Newness
                        if (command.IndexOf(SQLCst.NOLOCK.Trim(), StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            command = command.Replace(SQLCst.NOLOCK.Trim(), SQLNOLOCK(svrType).Trim());
                        }
                    }
                    //
                    if (svrType == DbSvrType.dbSQL)
                    {
                        //FI 20111221 Replace des Mod(x,y) par une syntaxe SQLServer
                        command = ReplaceModToSqlServer(command);
                    }
                    //
                    // RD 20100520: Replace de "@" par ":"
                    // Code anciennement dans DataAccess.ReplaceVarPrefix
                    if ((svrType != DbSvrType.dbOLEDB) && (svrType != DbSvrType.dbSQL))
                    {
                        //Comme préfixe de paramètre Spheres® accepte l'opérateur SQLServer "@"
                        //On remplace donc ce préfixe par le préfixe du moteur SQL 
                        //
                        if (ArrFunc.IsFilled(commandParameters))
                        {
                            foreach (IDbDataParameter param in commandParameters)
                            {
                                // FI 20130225 [18441] add du if (param == null)
                                // L'instruction continue est utilise parce que déjà utilisé plus loin
                                // Il est possible que param soit à null car cette même méthode positionne à null les parameters non utilisés
                                // voir plus loin dans la fonction commandParameters.SetValue(null, idx);
                                if (param == null)
                                    continue;
                                //PL 20100716 Replace with IgnoreCase (Car le nom des paramètres est depuis tjs converti en UPPER dans StringDynamicData)
                                //command = command.Replace("@" + param.ParameterName.Replace(GetVarPrefix(pServerType), ""), param.ParameterName);
                                string nameInSQLServerFormat = param.ParameterName.Replace(GetVarPrefix(svrType), "@");
                                //command = Regex.Replace(command, nameInSQLServerFormat, param.ParameterName, RegexOptions.IgnoreCase);
                                command = SafeReplaceParameters(command, nameInSQLServerFormat, param.ParameterName, RegexOptions.IgnoreCase, true);
                            }
                        }
                    }

                    switch (svrType)
                    {
                        case DbSvrType.dbORA:
                            #region Oracle
                            // FI 20201127 [XXXXX] Mise en commentaire du replace dbo par le schema
                            // Dans le cache les requêtes sont stockées avec dbo.
                            //RD 20100521 code déplacer plus haut pour faire la même chose pour SQLServer
                            //
                            //Spheres Remplace DBO. par le SCHEMA Oracle
                            //string schema = conManager.GetAttribute("User ID");
                            //schema = " " + schema + ".";
                            //CSManager conManager = new CSManager(pConnectionString);
                            //string schema = " " + GetObjectsOwner(conManager) + ".";

                            command = command.Replace(SQLCst.SQL_ANSI + Cst.CrLf, string.Empty);//Warning: 20050330 PL Bug Oracle s'il existe plusieurs commentaire en début de query
                            command = command.Replace(SQLCst.SQL_ANSI, string.Empty);
                            //command = command.Replace(SQLCst.DBO + "UF_DATAENABLED", schema + "UP_GENERAL.UF_DATAENABLED");

                            ////20091026 PL Add If() avec Trim()
                            //if (pCommandType == CommandType.StoredProcedure)
                            //    command = command.Replace(SQLCst.DBO.Trim(), schema.Trim());
                            //else
                            //    command = command.Replace(SQLCst.DBO, schema);


                            if (commandParameters != null)
                            {
                                // 20100803 MF - 'sql:variable' oracle translation 
                                // 1. Find all the sql:variable references in the sql query
                                Regex regSQLVariable = new Regex("(sql:variable\\(\")([@:])(\\w*)(\"\\))");

                                foreach (Match matchVar in regSQLVariable.Matches(command))
                                {
                                    // 2. Get the third matching group (\\w*) containing the var name
                                    string strNom = matchVar.Groups[3].Captures[0].Value;
                                    // 3. Searching the parameter strNom from the commandParameters collection
                                    for (int idx = 0; idx < commandParameters.Length; idx++)
                                    {
                                        IDbDataParameter param = commandParameters[idx];

                                        if (param == null)
                                            continue;

                                        if (Regex.Match(
                                                param.ParameterName,
                                                String.Format("^[@:]?({0})$", strNom)
                                            ).Success)
                                        {
                                            // 4. If the parameter exists then we replace 'sql:variable(@<var name>)' with '"<var value>"'
                                            switch (param.DbType)
                                            {
                                                // PM 20120920 [18058] Ajout de "" dans le cas de remplacement d'une variable de type String
                                                case DbType.AnsiString:
                                                    command = Regex.Replace(
                                                        command,
                                                        String.Format("(sql:variable\\(\")([@:])?({0})(\"\\))", strNom),
                                                        "\"" + Convert.ToString(param.Value) + "\"");
                                                    break;
                                                default:
                                                    command = Regex.Replace(
                                                        command,
                                                        String.Format("(sql:variable\\(\")([@:])?({0})(\"\\))", strNom),
                                                        Convert.ToString(param.Value));
                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (pCommandType == CommandType.Text)
                                    ParametersNotUsedSetNull(command, commandParameters);
                            }
                            // 'sql:variable' translation END - a simpler implementantion is done cycling on parameters and forcing blind replaces
                            #endregion
                            break;
                        case DbSvrType.dbSQL:
                            break;
                        case DbSvrType.dbOLEDB:
                            break;
                        default:
                            throw (new ArgumentException("No match for server type", "ServerType"));
                    }

                    ret = command;

                    if (isCacheEnabled) // Mise en cache de l'instruction SQL obtenue si le cache est actif  
                    {
                        switch (mode)
                        {
                            case TranformQueryCacheMode.Concurrent:
                                _tranformQueryCacheConcurrent.TryAdd(key, ret);
                                break;
                            case TranformQueryCacheMode.Basic:
                                lock (((ICollection)_tranformQueryCacheBasic).SyncRoot)
                                {
                                    if (false == _tranformQueryCacheBasic.ContainsKey(key))
                                        _tranformQueryCacheBasic.Add(key, ret);
                                }
                                break;
                            default:
                                throw new NotSupportedException($"Mode : {mode} is not supported");
                        }
                    }
                }

                // FI 20201127 [XXXXX] Replace de dbo. par le schema
                switch (svrType)
                {
                    case DbSvrType.dbORA:
                        string schema = $" {GetObjectsOwner(new CSManager(pConnectionString))}.";
                        ret = ret.Replace(SQLCst.DBO + "UF_DATAENABLED", schema + "UP_GENERAL.UF_DATAENABLED");

                        //20091026 PL Add If() avec Trim()
                        if (pCommandType == CommandType.StoredProcedure)
                            ret = ret.Replace(SQLCst.DBO.Trim(), schema.Trim());
                        else
                            ret = ret.Replace(SQLCst.DBO, schema);
                        break;
                }

            }
            return ret;
        }

        /// <summary>
        /// Remplacement exact des paramètres d'une query 
        /// Exemple : query = Select 1 from TABLE where DATA = @PARAM1 and DATA2=@PARAM11
        /// 
        /// 1. SafeReplaceParameters(query, @PARAM1, :PARAM1, IgnoreCase, true)
        ///    After => query = Select 1 from TABLE where DATA = :PARAM1 and DATA2=@PARAM11
        /// 2. SafeReplaceParameters(query, @PARAM11, :PARAM11, IgnoreCase, true)
        ///    After => query = Select 1 from TABLE where DATA = :PARAM1 and DATA2=:PARAM11
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pFind"></param>
        /// <param name="pReplace"></param>
        /// <param name="pRegexOptions"></param>
        /// <param name="pMatchWholeWord"></param>
        /// <returns></returns>
        /// EG 20231005 [WI715][26497] New 
        public static string SafeReplaceParameters(string pInput, string pFind, string pReplace, RegexOptions pRegexOptions, bool pMatchWholeWord)
        {
            string searchString = pFind.StartsWith("@") ? $@"@\b{pFind.Substring(1)}\b" : $@"\b{pFind}\b";
            string textToFind = pMatchWholeWord ? searchString : pFind;
            return Regex.Replace(pInput, textToFind, pReplace, pRegexOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCommandText"></param>
        /// <param name="commandParameters"></param>
        /// FI 20201009 [XXXXX] Add
        private static void ParametersNotUsedSetNull(string pCommandText, params IDbDataParameter[] commandParameters)
        {
            // 20101115 MF - Set null value for the parameters that are not used inside an SQL query CommandType.Text, 
            //  to avoid the ORA-01036 error. 
            if (ArrFunc.IsFilled(commandParameters))
            {
                for (int idx = 0; idx < commandParameters.Length; idx++)
                {
                    IDbDataParameter param = commandParameters[idx];

                    // FI 20130225 [18441] add du if (param == null)
                    // Il est possible que param soit à null car cette même méthode positionne à null les parameters non utilisés
                    // voir plus loin dans la fonction commandParameters.SetValue(null, idx);
                    if (param == null)
                        continue;

                    // The regex search all the occurences of the current parameter name imposing the next constraint :
                    //  the target string must be terminated at least with one character that is not a word character (/W+),
                    //  otherwise we might match some strings containing the target string.    
                    if (!Regex.Match(pCommandText, String.Format(@"({0})(\W+|$)", param.ParameterName)).Success)
                        commandParameters.SetValue(null, idx);
                }
            }
        }

        #endregion Method TransformQuery

        /// <summary>
        ///  Insertion d'un enregistrement dans une table (Avec 1 colonne Identity).
        ///  <para>Retourne la valeur attribuée par le SGBDR pour la colonne IDENTITY</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuery">Requête d'insert (i.e. insert into dbo.TABLE1(EMPLOYEE) values ('Filip')</param>
        /// <param name="pDp">Parameters de la requeête d'insert</param>
        /// <returns></returns>
        /// FI 20180308 [XXXXX] Add
        public static Int32 InsertTableIdentity(string pCS, String pQuery, DataParameters pDp)
        {

            int retId = 0;

            Regex regexinsert = new Regex(SQLCst.INSERT_INTO_DBO + @"(\w+)");
            if (false == regexinsert.IsMatch(pQuery))
                throw new NotSupportedException("Query is not an insert into dbo.");

            MatchCollection matchCol = regexinsert.Matches(pQuery);
            if (matchCol.Count == 1)
            {
                string tableName = matchCol[0].Groups[1].Value;
                string colIdentity = OTCmlHelper.GetColunmID(tableName);

                if (DataHelper.IsDbSqlServer(pCS))
                {
                    string[] querySplit = pQuery.Split(new string[] { SQLCst.VALUES }, StringSplitOptions.RemoveEmptyEntries);

                    string query = StrFunc.AppendFormat(@"declare @MyTableVar TABLE(ID int); 
                    {0}
                    output inserted.{1} into @MyTableVar
                    {2} {3}
                    select ID from @MyTableVar", querySplit[0], colIdentity, SQLCst.VALUES, querySplit[1]);

                    object ret;
                    if (null != pDp)
                        ret = DataHelper.ExecuteScalar(pCS, CommandType.Text, query, pDp.GetArrayDbParameter());
                    else
                        ret = DataHelper.ExecuteScalar(pCS, CommandType.Text, query);

                    retId = Convert.ToInt32(ret);

                }
                else if (DataHelper.IsDbOracle(pCS))
                {
                    if (null == pDp)
                        pDp = new DataParameters();

                    pDp.Add(new DataParameter(pCS, "COLIDENTITY", DbType.Int32));
                    pDp["COLIDENTITY"].Direction = ParameterDirection.Output;

                    string query = StrFunc.AppendFormat(@"{0} returning ID into :COLIDENTITY;", pQuery);

                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, query, pDp.GetArrayDbParameter());
                    object ret = pDp["COLIDENTITY"].DbDataParameter.Value;
                    retId = Convert.ToInt32(ret);
                }
            }
            return retId;
        }


        #endregion

        #region private Method
        #region private InitializeQueryCache
        private static QueryCache InitializeQueryCache()
        {
            return new QueryCache();
        }

        #endregion
        #region private IsSystemObject
        /// <summary>
        /// Return true if object <c>psObject</c> is system object 
        /// </summary>
        /// <param name="psObject">object</param>
        /// <returns>Boolean</returns>
        private static bool IsSystemObject(string psObject)
        {
            psObject = " " + psObject.Trim();
            return ((psObject.IndexOf(ORA_ALL_) >= 0) && (psObject.IndexOf(ORA_USER_) >= 0) && (psObject.IndexOf(ORA_DUALTABLE) >= 0));
        }
        #endregion
        #region private IsUserObject
        /// <summary>
        /// Return true if object <c>psObject</c> is user object 
        /// </summary>
        /// <param name="psObject">object</param>
        /// <returns>Boolean</returns>
        private static bool IsUserObject(string psObject)
        {
            psObject = " " + psObject.Trim();
            return ((psObject.IndexOf(TBLTMP_USER) >= 0) && (psObject.IndexOf(TBL_USER) >= 0) && (psObject.IndexOf(V_USER) >= 0));
        }
        #endregion

        /// <summary>
        /// Compare la taille des ParameterName
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static int CompareDataParameterName(IDataParameter p1, IDataParameter p2)
        {
            if ((p1 == null) && p2 == null) // (p2=p1)
                return 0;
            else if ((p1 == null) && p2 != null) // (p2>p1)
                return -1;
            else if ((p2 == null) && p1 != null) // (p1>p2)
                return 1;
            else
                return StrFunc.CompareLength(p1.ParameterName, p2.ParameterName);

        }



        #endregion

    }
    #endregion
    //
    #region DataHelper/DataSet
    public sealed partial class DataHelper
    {
        #region public ExecuteDataSet
        #region ExecuteDataset(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // EG 20180205 [23769] New
        // FI 20180316 [23769] Modify
        public static DataSet ExecuteDataset(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            DataSet ret = null;

            if (null != transaction)
            {
                // FI 20180316 [23769] Lecture et Mise à jour du cache SQL lorsqu'une requête s'exécute sous une transaction
                CSManager csManager = new CSManager(connectionString);

                bool isUseCache = BoolFunc.IsTrue(csManager.IsUseCache) && (CommandType.Text == commandType);
                commandText = TransformQuery2(csManager.Cs, commandType, commandText, commandParameters);

                if (isUseCache)
                    ret = (DataSet)queryCache.GetResult(csManager.Cs, commandText, commandParameters, QueryCacheResultTypeEnum.dataset);

                if (null == ret)
                {
                    ret = DataHelper.ExecuteDataset(transaction, commandType, commandText, commandParameters);

                    //Mise à jour du cache
                    if (isUseCache)
                        queryCache.Update(csManager, commandText, commandParameters, ret, QueryCacheResultTypeEnum.dataset);
                }

                return ret;
            }
            else
            {
                ret = DataHelper.ExecuteDataset(connectionString, commandType, commandText, commandParameters);
            }

            return ret;
        }
        #endregion
        #region ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        // RD 20100111 [16818] Add new DALOleDb
        public static DataSet ExecuteDataset(string connectionString, string connectionString2, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connectionString), connectionString, connectionString2, commandType, commandText);
        }
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connectionString), connectionString, commandType, commandText);
        }
        #endregion
        #region ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connectionString), connectionString, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connectionString), connectionString, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteDataset(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        // RD 20100111 [16818] Add new DALOleDb
        public static DataSet ExecuteDataset(DbSvrType ServerType, string connectionString, string connectionString2, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteDataset(ServerType, connectionString, connectionString2, commandType, commandText, null);
        }
        public static DataSet ExecuteDataset(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteDataset(ServerType, connectionString, commandType, commandText, null);
        }
        #endregion
        #region ExecuteDataset(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // RD 20100111 [16818] Add new DALOleDb
        public static DataSet ExecuteDataset(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteDataset(ServerType, connectionString, string.Empty, commandType, commandText, commandParameters);
        }
        // RD 20100111 [16818] Add new DALOleDb
        // EG 20240304 Restauration de cette Signature (Bug Code analysis)
        public static DataSet ExecuteDataset(DbSvrType ServerType, string connectionString, string connectionString2, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            DataSet ret = null;
            CSManager csManager = new CSManager(connectionString);
            

            bool isUseCache = BoolFunc.IsTrue(csManager.IsUseCache) && (CommandType.Text == commandType);
            commandText = TransformQuery2(csManager.Cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, connectionString, commandType, commandText, commandParameters);

            if (isUseCache)
                ret = (DataSet)queryCache.GetResult(csManager.Cs, commandText, commandParameters, QueryCacheResultTypeEnum.dataset);

            if (null == ret)
            {
                IDataAccess dal = GetDAL(ServerType);
                try
                {
                    DatetimeProfiler dtProfiler = new DatetimeProfiler();
                    ret = dal.ExecuteDataset(csManager.Cs, commandType, commandText, commandParameters);
                    TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
                }
                catch (Exception ex)
                {
                    TraceQueryError(csManager, ex.Message, commandText, commandParameters);
                    throw;
                }

                WriteRDBMSLog(connectionString);

                //Mise à jour du cache
                if (isUseCache)
                    queryCache.Update(csManager, commandText, commandParameters, ret, QueryCacheResultTypeEnum.dataset);
            }
            else
                WriteTraceQuery("(Cache used)");

            return ret;
        }
        #endregion
        #region ExecuteDataset(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        public static DataSet ExecuteDataset(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        {
            CSManager csManager = new CSManager(connectionString);

            WriteRDBMSLog(true, connectionString, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            DataSet ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteDataset(csManager.Cs, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        #endregion

        #region ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connection), connection, commandType, commandText);
        }
        #endregion
        #region  ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connection), connection, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(connection), connection, spName, parameterValues);
        }
        #endregion

        #region ExecuteDataset(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        public static DataSet ExecuteDataset(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(ServerType, connection, commandType, commandText, null);
        }
        #endregion
        #region ExecuteDataset(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static DataSet ExecuteDataset(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            commandText = TransformQuery2(connection.ConnectionString, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, connection.ConnectionString, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            DataSet ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = dal.ExecuteDataset(connection, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(connection != null ? new CSManager(connection.ConnectionString) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        #region ExecuteDataset(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        public static DataSet ExecuteDataset(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            WriteRDBMSLog(true, connection.ConnectionString, CommandType.StoredProcedure, spName, parameterValues);
            
            IDataAccess dal = GetDAL(ServerType);
            DataSet ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = dal.ExecuteDataset(connection, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(connection != null ? new CSManager(connection.ConnectionString) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion

        #region ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        public static DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(transaction), transaction, commandType, commandText);
        }
        #endregion
        #region ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(transaction), transaction, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteDataset(GetDbSvrType(transaction), transaction, spName, parameterValues);
        }
        #endregion
        //
        #region  ExecuteDataset(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(ServerType, transaction, commandType, commandText, null);
        }
        #endregion
        #region ExecuteDataset(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static DataSet ExecuteDataset(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {            
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;
            commandText = TransformQuery2(cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, cs, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            DataSet ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteDataset(transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion

        #region ExecuteDataset(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        public static DataSet ExecuteDataset(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;
            WriteRDBMSLog(true, cs, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            DataSet ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteDataset(transaction, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(false, cs, CommandType.StoredProcedure, spName);

            return ret;
        }
        #endregion

        #region ExecuteDataset(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static DataSet ExecuteDataset(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == pDataDbTransaction)
                throw new ArgumentNullException("paramer is null", "pDataDbTransaction");
            if (null == pDataDbTransaction.Transaction)
                throw new NullReferenceException("pDataDbTransaction.Transaction is null");
            if (null == pDataDbTransaction.Transaction.Connection)
                throw new NullReferenceException("pDataDbTransaction.Transaction.Connection is null");

            string cs = pDataDbTransaction.Transaction.Connection.ConnectionString;

            commandText = TransformQuery2(cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, pDataDbTransaction.CsSpheres, commandType, commandText, commandParameters);
            
            IDataAccess dal = GetDAL(pDataDbTransaction.ServerType);
            DataSet ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteDataset(pDataDbTransaction.Transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(false, pDataDbTransaction.CsSpheres, commandType, commandText);

            return ret;
        }
        #endregion
        #endregion ExecuteDataSet

        #region ExecuteDataTable
        #region ExecuteDataTable(string connectionString, IDbTransaction transaction,CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // EG 20180205 [23769] New
        // FI 20180316 [23769] Modify
        public static DataTable ExecuteDataTable(string connectionString, IDbTransaction transaction, string commandText, params IDbDataParameter[] commandParameters)
        {
            DataTable ret = null;

            if (null != transaction)
            {
                // FI 20180316 [23769] Lecture et Mise à jour du cache SQL lorsqu'une requête s'exécute sous une transaction
                CSManager csManager = new CSManager(connectionString);
                bool isUseCache = BoolFunc.IsTrue(csManager.IsUseCache);

                commandText = TransformQuery2(csManager.Cs, CommandType.Text, commandText, commandParameters);
                if (isUseCache)
                    ret = (DataTable)queryCache.GetResult(csManager.Cs, commandText, commandParameters, QueryCacheResultTypeEnum.datatable);

                if (null == ret)
                {
                    ret = DataHelper.ExecuteDataTable(transaction, commandText, commandParameters);
                    if (isUseCache)
                        queryCache.Update(csManager, commandText, commandParameters , ret, QueryCacheResultTypeEnum.datatable);
                }
            }
            else
            {
                ret = DataHelper.ExecuteDataTable(connectionString, commandText, commandParameters);
            }

            return ret;
        }
        #endregion
        #region ExecuteDataTable(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static DataTable ExecuteDataTable(string connectionString, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteDataTable(GetDbSvrType(connectionString), connectionString, commandText, commandParameters);
        }
        #endregion
        #region ExecuteDataTable(DbSvrType ServerType, string connectionString, string commandText, params IDbDataParameter[] commandParameters)
        public static DataTable ExecuteDataTable(DbSvrType ServerType, string connectionString, string commandText, params IDbDataParameter[] commandParameters)
        {
            DataTable ret = null;
            CSManager csManager = new CSManager(connectionString);
            bool isUseCache = BoolFunc.IsTrue(csManager.IsUseCache);

            commandText = TransformQuery2(csManager.Cs, CommandType.Text, commandText, commandParameters);
#if DEBUG
            if (isTraceQueryForDebug)
                dtStartTraceQuery = DateTime.Now;
#endif
            //Lecture du cache
            if (isUseCache)
                ret = (DataTable)queryCache.GetResult(csManager.Cs, commandText, commandParameters, QueryCacheResultTypeEnum.datatable);
            
            if (null == ret)
            {
                //PL 20180903 TEST "isUseCache = false", afin que ExecuteDataset n'exploite pas ici inutilement le cache
                csManager.IsUseCache = false;
                DataSet ds = DataHelper.ExecuteDataset(csManager.CsSpheres, CommandType.Text, commandText, commandParameters);
                ret = ds.Tables[0];

                if (isUseCache)
                    queryCache.Update(csManager, commandText, commandParameters, ret, QueryCacheResultTypeEnum.datatable);
            }
            else
            {
#if DEBUG
                WriteTraceQuery(true, dtStartTraceQuery, connectionString, CommandType.Text.ToString(), commandText, commandParameters);
                WriteTraceQuery("(Cache used)");
#endif
            }
            //
            return ret;
        }
        #endregion

        #region ExecuteDataTable(IDbTransaction pDbTransaction, string commandText, params IDbDataParameter[] commandParameters)
        public static DataTable ExecuteDataTable(IDbTransaction pDbTransaction, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteDataTable(DataHelper.GetDbSvrType(pDbTransaction), pDbTransaction, commandText, commandParameters);
        }
        #endregion
        #region ExecuteDataTable(DbSvrType ServerType,IDbTransaction pDbTransaction, string commandText, params IDbDataParameter[] commandParameters)
        public static DataTable ExecuteDataTable(DbSvrType ServerType, IDbTransaction pDbTransaction, string commandText, params IDbDataParameter[] commandParameters)
        {
            DataSet ds = DataHelper.ExecuteDataset(ServerType, pDbTransaction, CommandType.Text, commandText, commandParameters);
            DataTable ret = ds.Tables[0];
            return ret;
        }
        #endregion
        #endregion ExecuteDataTable
    }
    #endregion
    //
    #region DataHelper/ExecuteReader
    public sealed partial class DataHelper
    {
        #region FetchSize
        public static void SetFetchSize(string pConnectionString, IDataReader pDataReader, long pNumRows)
        {
            SetFetchSize(GetDbSvrType(pConnectionString), pDataReader, pNumRows);
        }
        public static void SetFetchSize(DbSvrType pServerType, IDataReader pDataReader, long pNumRows)
        {
            GetDAL(pServerType).SetFetchSize(pDataReader, pNumRows);
        }
        #endregion FetchSize
        #region public ExecuteReader
        #region ExecuteReader(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // EG 20180205 [23769] New
        public static IDataReader ExecuteReader(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null != transaction)
                return DataHelper.ExecuteReader(transaction, commandType, commandText, commandParameters);
            else
                return DataHelper.ExecuteReader(connectionString, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteReader(string connectionString, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        // RD 20100111 [16818] Add new DALOleDb
        public static IDataReader ExecuteReader(string connectionString, string connectionString2, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connectionString), connectionString, connectionString2, commandType, commandText);
        }
        public static IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connectionString), connectionString, commandType, commandText);
        }
        #endregion
        #region ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connectionString), connectionString, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connectionString), connectionString, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteReader(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        // RD 20100111 [16818] Add new DALOleDb
        public static IDataReader ExecuteReader(DbSvrType ServerType, string connectionString, string connectionString2, CommandType commandType, string commandText)
        {
            return ExecuteReader(ServerType, connectionString, connectionString2, commandType, commandText, null);
        }
        public static IDataReader ExecuteReader(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteReader(ServerType, connectionString, commandType, commandText, null);
        }
        #endregion
        #region  ExecuteReader(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // RD 20100111 [16818] Add new DALOleDb
        public static IDataReader ExecuteReader(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return ExecuteReader(ServerType, connectionString, string.Empty, commandType, commandText, commandParameters);
        }
        // RD 20100111 [16818] Add new DALOleDb
        public static IDataReader ExecuteReader(DbSvrType ServerType, string connectionString, string connectionString2, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            CSManager csManager = new CSManager(connectionString);

            commandText = TransformQuery2(csManager.Cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, connectionString, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteReader(csManager.Cs, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        #endregion
        #region ExecuteReader(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        public static IDataReader ExecuteReader(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        {
            CSManager csManager = new CSManager(connectionString);

            WriteRDBMSLog(true, connectionString, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteReader(csManager.Cs, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connection), connection, commandType, commandText);
        }
        #endregion
        #region ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connection), connection, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(connection), connection, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        public static IDataReader ExecuteReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(ServerType, connection, commandType, commandText, null);
        }
        #endregion
        #region  ExecuteReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static IDataReader ExecuteReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            commandText = TransformQuery2(connection.ConnectionString, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, connection.ConnectionString, commandType, commandText, commandParameters);
            // PM 20180108 [CHEETAH] commandParameters était absent
            //IDataReader ret = GetDAL(ServerType).ExecuteReader(connection, commandType, commandText);

            IDataAccess dal = GetDAL(ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = dal.ExecuteReader(connection, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(connection != null ? new CSManager(connection.ConnectionString) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        #region ExecuteReader(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        public static IDataReader ExecuteReader(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            WriteRDBMSLog(true, connection.ConnectionString, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = dal.ExecuteReader(connection, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName,parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(connection != null ? new CSManager(connection.ConnectionString) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(transaction), transaction, commandType, commandText);
        }
        #endregion
        #region ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   IDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(transaction), transaction, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  IDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a IDataReader containing the resultset generated by the command</returns>
        public static IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteReader(GetDbSvrType(transaction), transaction, spName, parameterValues);
        }
        public static IDataReader ExecuteReader(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;

            WriteRDBMSLog(true, cs, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteReader(transaction, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion ExecuteReader
        //
        #region ExecuteReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        public static IDataReader ExecuteReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(ServerType, transaction, commandType, commandText, null);
        }
        #endregion
        #region ExecuteReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static IDataReader ExecuteReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;

            commandText = TransformQuery2(cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, cs, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteReader(transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteReader(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText)
        public static IDataReader ExecuteReader(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText)
        {
            if (null == pDataDbTransaction)
                throw new ArgumentNullException("paramer is null", "pDataDbTransaction");
            if (null == pDataDbTransaction.Transaction)
                throw new NullReferenceException("pDataDbTransaction.Transaction is null");
            if (null == pDataDbTransaction.Transaction.Connection)
                throw new NullReferenceException("pDataDbTransaction.Transaction.Connection is null");

            string cs = pDataDbTransaction.Transaction.Connection.ConnectionString;

            commandText = TransformQuery2(cs, commandType, commandText, null);

            WriteRDBMSLog(true, pDataDbTransaction.CsSpheres, commandType, commandText);

            IDataAccess dal = GetDAL(pDataDbTransaction.ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteReader(pDataDbTransaction.Transaction, commandType, commandText);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText);
                throw;
            }

            WriteRDBMSLog(pDataDbTransaction.CsSpheres);

            return ret;
        }
        #endregion
        #region ExecuteReader(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static IDataReader ExecuteReader(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == pDataDbTransaction)
                throw new ArgumentNullException("paramer is null", "pDataDbTransaction");
            if (null == pDataDbTransaction.Transaction)
                throw new NullReferenceException("pDataDbTransaction.Transaction is null");

            commandText = TransformQuery2(pDataDbTransaction.Cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, pDataDbTransaction.CsSpheres, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(pDataDbTransaction.ServerType);
            IDataReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(pDataDbTransaction.Cs);
                ret = dal.ExecuteReader(pDataDbTransaction.Transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(pDataDbTransaction != null ? new CSManager(pDataDbTransaction.Cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(pDataDbTransaction.CsSpheres);

            return ret;
        }
        #endregion
        #endregion
    }
    #endregion
    //
    #region DataHelper/ExecuteScalar
    public sealed partial class DataHelper
    {
        #region public ExecuteScalar
        #region ExecuteScalar(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // EG 20180205 [23769] New
        // FI 20180316 [23769] Modify
        // EG 20180322 [23769] Add Test DBNull.Value
        public static object ExecuteScalar(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            object ret = null;

            if (null != transaction)
            {
                // FI 20180316 [23769] Lecture et Mise à jour du cache SQL lorsqu'une requête s'exécute sous une transaction
                CSManager csManager = new CSManager(connectionString);

                bool isUseCache = BoolFunc.IsTrue(csManager.IsUseCache) && (CommandType.Text == commandType);
                commandText = TransformQuery2(csManager.Cs, commandType, commandText, commandParameters);

                if (isUseCache)
                    ret = queryCache.GetResult(csManager.Cs, commandText, commandParameters, QueryCacheResultTypeEnum.@object);

                if (null == ret)
                {
                    ret = DataHelper.ExecuteScalar(transaction, commandType, commandText, commandParameters);

                    if (isUseCache)
                        queryCache.Update(csManager, commandText, commandParameters, ret, QueryCacheResultTypeEnum.@object);
                }

                if (DBNull.Value == ret)
                    ret = null;
            }
            else
            {
                ret = DataHelper.ExecuteScalar(connectionString, commandType, commandText, commandParameters);
            }
            return ret;
        }
        #endregion
        #region ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(connectionString), connectionString, commandType, commandText);
        }
        #endregion
        #region ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(connectionString), connectionString, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(connectionString), connectionString, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteScalar(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        public static object ExecuteScalar(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteScalar(ServerType, connectionString, commandType, commandText, null);
        }
        #endregion
        #region ExecuteScalar(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText,  params IDbDataParameter[] commandParameters)
        public static object ExecuteScalar(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            object ret = null;
            CSManager csManager = new CSManager(connectionString);
            bool isUseCache = (BoolFunc.IsTrue(csManager.IsUseCache) && (CommandType.Text == commandType));

            commandText = TransformQuery2(csManager.Cs, commandType, commandText, commandParameters);
            WriteRDBMSLog(true, connectionString, commandType,commandText, commandParameters);

            //Lecture du cache
            if (isUseCache)
                ret = queryCache.GetResult(csManager.Cs, commandText, commandParameters, QueryCacheResultTypeEnum.@object);

            if (null == ret)
            {
                IDataAccess dal = GetDAL(ServerType);
                try
                {
                    DatetimeProfiler dtProfiler = new DatetimeProfiler();
                    ret = dal.ExecuteScalar(csManager.Cs, commandType,commandText, commandParameters);
                    TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
                }
                catch (Exception ex)
                {
                    TraceQueryError(csManager, ex.Message,commandText, commandParameters);
                    throw;
                }

                WriteRDBMSLog(connectionString);

                if (isUseCache)
                    queryCache.Update(csManager, commandText, commandParameters, ret, QueryCacheResultTypeEnum.@object);
            }
            else
                WriteTraceQuery("(Cache used)");

            if (DBNull.Value == ret)
                ret = null;

            return ret;
        }
        #endregion
        #region ExecuteScalar(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        public static object ExecuteScalar(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        {
            CSManager csManager = new CSManager(connectionString);

            spName = TransformQuery2(csManager.Cs, CommandType.StoredProcedure, spName, null);

            WriteRDBMSLog(true, connectionString, CommandType.StoredProcedure, spName, parameterValues);
            
            IDataAccess dal = GetDAL(ServerType);
            object ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteScalar(csManager.Cs, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(connection), connection, commandType, commandText);
        }
        #endregion
        #region ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(connection), connection, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(connection), connection, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteScalar(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        public static object ExecuteScalar(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteScalar(ServerType, connection, commandType, commandText, null);
        }
        #endregion
        #region ExecuteScalar(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static object ExecuteScalar(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");
            
            string cs = connection.ConnectionString;

            commandText = TransformQuery2(cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, cs, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            object ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteScalar(connection, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion
        #region ExecuteScalar(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        public static object ExecuteScalar(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            string cs = connection.ConnectionString;

            spName = TransformQuery2(cs, CommandType.StoredProcedure, spName, null);

            WriteRDBMSLog(true, cs, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            object ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteScalar(connection, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, spName, parameterValues);
                throw;
            }
            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(transaction), transaction, commandType, commandText);
        }
        #endregion
        #region ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(transaction), transaction, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteScalar(GetDbSvrType(transaction), transaction, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteScalar(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        public static object ExecuteScalar(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(ServerType, transaction, commandType, commandText, null);
        }
        #endregion
        #region ExecuteScalar(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static object ExecuteScalar(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;

            commandText = TransformQuery2(cs, commandType, commandText, commandParameters);

            WriteRDBMSLog(true, cs, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            object ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteScalar(transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion
        #region ExecuteScalar(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        public static object ExecuteScalar(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;

            spName = TransformQuery2(cs, CommandType.StoredProcedure, spName, null);

            WriteRDBMSLog(true, cs, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            object ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteScalar(transaction, spName, parameterValues);
                CSManager csManager = new CSManager(cs);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion

        public static object ExecuteScalar(DataDbTransaction pDataDbTransaction, CommandType commandType, string commandText)
        {
            if (null == pDataDbTransaction)
                throw new ArgumentNullException("paramer is null", "pDataDbTransaction");
            if (null == pDataDbTransaction.Transaction)
                throw new NullReferenceException("pDataDbTransaction.Transaction is null");
            if (null == pDataDbTransaction.Transaction.Connection)
                throw new NullReferenceException("pDataDbTransaction.Transaction.Connection is null");

            string cs = pDataDbTransaction.Transaction.Connection.ConnectionString;

            commandText = TransformQuery2(cs, commandType, commandText, null);

            WriteRDBMSLog(true, pDataDbTransaction.CsSpheres, commandType, commandText);

            IDataAccess dal = GetDAL(pDataDbTransaction.ServerType);
            object ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteScalar(pDataDbTransaction.Transaction, commandType, commandText, null);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, commandText);
                throw;
            }

            WriteRDBMSLog(pDataDbTransaction.CsSpheres);

            return ret;
        }

        #endregion ExecuteScalar
    }
    #endregion
    //
    #region DataHelper/ExecuteNonQuery
    public sealed partial class DataHelper
    {
        #region public ExecuteNonQuery
        #region ExecuteNonQuery(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        // EG 20180205 [23769] New
        public static int ExecuteNonQuery(string connectionString, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == transaction)
                return DataHelper.ExecuteNonQuery(connectionString, commandType, commandText, commandParameters);
            else
                return DataHelper.ExecuteNonQuery(transaction, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(connectionString), connectionString, commandType, commandText);
        }
        #endregion
        #region ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(connectionString), connectionString, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a IDbConnection</param>
        /// <param name="spName">the name of the stored prcedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(connectionString), connectionString, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteNonQuery(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        public static int ExecuteNonQuery(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(ServerType, connectionString, commandType, commandText, null);
        }
        #endregion
        #region ExecuteNonQuery(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static int ExecuteNonQuery(DbSvrType ServerType, string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            CSManager csManager = new CSManager(connectionString);
            //commandText = TransformQuery(ServerType, connectionString, commandType, commandText);		
            //20060526 PL/FDA Warning: Pb Oracle on indique StoredProcedure pour ne pas insérer de Commentaire qui pose pb lors d ela saisie des trades
            commandText = TransformQuery2(csManager.Cs, CommandType.StoredProcedure, commandText, commandParameters);
            WriteRDBMSLog(true, connectionString, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteNonQuery(csManager.Cs, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        #endregion

        /// <summary>
        /// Execute a single element INSERT including a big (more than 32KB ) xml field. 
        /// </summary>
        /// <remarks>Oracle specific</remarks>
        /// <param name="ServerType">server type, just ORA value is accepted</param>
        /// <param name="connectionString">the string to the Oracle DBMS </param>
        /// <param name="commandText">the insert command, type text</param>
        /// <param name="pXml">the xml document including the xml content we have to insert</param>
        /// <param name="pXmlParameterName">the name of the parameter we have to fill with the xml contents inside of pXml.
        /// Do not prefix with special characters like ":" or "@"</param>
        /// <param name="commandParameters">additional command parameters</param>
        /// <returns>1 when the insert succeed, 0 otherwise</returns>
        /// <exception cref="NotSupportedException">when server type is not Oracle</exception>
        public static int ExecuteNonQueryXmlForOracle
            (DbSvrType ServerType, string connectionString, string commandText,
            System.Xml.XmlDocument pXml, string pXmlParameterName, params IDbDataParameter[] commandParameters)
        {
            if (ServerType != DbSvrType.dbORA)
            {
                throw new NotSupportedException("ExecuteNonQueryXmlForOracle is compliant with Oracle DBMS only");
            }

            CSManager csManager = new CSManager(connectionString);

            //20060526 PL/FDA Warning: Pb Oracle on indique StoredProcedure pour ne pas insérer de 
            // Commentaire qui pose pb lors d ela saisie des trades
            commandText = TransformQuery2(csManager.Cs, CommandType.StoredProcedure, commandText, commandParameters);

            WriteRDBMSLog(true, connectionString, CommandType.Text, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteNonQueryXmlForOracle(csManager.Cs, commandText, pXml, pXmlParameterName, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }


        #region ExecuteNonQuery(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        public static int ExecuteNonQuery(DbSvrType ServerType, string connectionString, string spName, params object[] parameterValues)
        {
            CSManager csManager = new CSManager(connectionString);

            WriteRDBMSLog(true, connectionString, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                ret = dal.ExecuteNonQuery(csManager.Cs, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(connection), connection, commandType, commandText);
        }
        #endregion
        #region ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(connection), connection, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(connection), connection, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteNonQuery(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        public static int ExecuteNonQuery(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(ServerType, connection, commandType, commandText, null);
        }
        #endregion
        #region ExecuteNonQuery(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static int ExecuteNonQuery(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            //20060526 PL/FDA Warning: Pb Oracle on indique StoredProcedure pour ne pas insérer de Commentaire qui pose pb lors de la saisie des trades
            commandText = TransformQuery2(connection.ConnectionString, CommandType.StoredProcedure, commandText, commandParameters);

            WriteRDBMSLog(true, connection.ConnectionString, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = dal.ExecuteNonQuery(connection, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(connection != null ? new CSManager(connection.ConnectionString) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        #region ExecuteNonQuery(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        public static int ExecuteNonQuery(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (null == connection)
                throw new ArgumentNullException("paramer is null", "connection");

            WriteRDBMSLog(true, connection.ConnectionString, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = dal.ExecuteNonQuery(connection, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(connection != null ? new CSManager(connection.ConnectionString) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(transaction), transaction, commandType, commandText);
        }
        #endregion
        #region ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(transaction), transaction, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteNonQuery(GetDbSvrType(transaction), transaction, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteNonQuery(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        public static int ExecuteNonQuery(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(ServerType, transaction, commandType, commandText, null);
        }
        #endregion
        #region ExecuteNonQuery(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static int ExecuteNonQuery(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;

            //commandText = TransformQuery(ServerType, transaction, commandType, commandText);
            //20060526 PL/FDA Warning: Pb Oracle on indique StoredProcedure pour ne pas insérer de Commentaire qui pose pb lors d ela saisie des trades
            commandText = TransformQuery2(cs, CommandType.StoredProcedure, commandText, commandParameters);

            WriteRDBMSLog(true, cs, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteNonQuery(transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message,commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion
        #region ExecuteNonQuery(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        public static int ExecuteNonQuery(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Connection)
                throw new NullReferenceException("transaction.Connection is null");

            string cs = transaction.Connection.ConnectionString;

            WriteRDBMSLog(true, cs, CommandType.StoredProcedure, spName, parameterValues);

            IDataAccess dal = GetDAL(ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(cs);
                ret = dal.ExecuteNonQuery(transaction, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(cs != null ? new CSManager(cs) : null, ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(cs);

            return ret;
        }
        #endregion
        //
        #region ExecuteNonQuery(DataDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static int ExecuteNonQuery(DataDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (null == transaction)
                throw new ArgumentNullException("paramer is null", "transaction");
            if (null == transaction.Transaction)
                throw new NullReferenceException("transaction.Transaction is null");

            commandText = TransformQuery2(transaction.Cs, CommandType.StoredProcedure, commandText, commandParameters);

            WriteRDBMSLog(true, transaction.CsSpheres, commandType, commandText, commandParameters);

            IDataAccess dal = GetDAL(transaction.ServerType);
            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(transaction.Cs);
                ret = dal.ExecuteNonQuery(transaction.Transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(transaction != null ? new CSManager(transaction.Cs) : null, ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(transaction.CsSpheres);

            return ret;
        }
        #endregion
        #endregion ExecuteNonQuery
    }
    #endregion
    //
    #region DataHelper/WriteRDBMSLog
    public sealed partial class DataHelper
    {
        #region WriteRDBMSLog
        private static void WriteTraceQuery()
        {
            WriteTraceQuery(string.Empty);
        }
        private static void WriteTraceQuery(string pComment)
        {
            WriteTraceQuery(false, pComment, DateTime.MinValue, string.Empty, string.Empty, string.Empty, null);
        }
        private static void WriteTraceQuery(bool pIsStart, string pCs, string pCommandType, string pText, params object[] commandParameters)
        {
            WriteTraceQuery(pIsStart, DateTime.Now, pCs, pCommandType, pText, commandParameters);
        }
        private static void WriteTraceQuery(bool pIsStart, DateTime pDtStart, string pCs, string pCommandType, string pText, params object[] commandParameters)
        {
            WriteTraceQuery(pIsStart, string.Empty, pDtStart, pCs, pCommandType, pText, commandParameters);
        }
        private static void WriteTraceQuery(bool pIsStart, string pComment, DateTime pDtStart, string pCs, string pCommandType, string pText, params object[] commandParameters)
        {
#if DEBUG
            if (isTraceQueryForDebug)
            {
                if (pIsStart)
                {
                    CSManager csManager = new CSManager(pCs);
                    dtStartTraceQuery = pDtStart;

                    System.Diagnostics.Debug.WriteLine("**************************************************************");
                    System.Diagnostics.Debug.WriteLine("Start       : " + DtFunc.DateTimeToStringISO(pDtStart));
                    System.Diagnostics.Debug.WriteLine("Connection  : " + csManager.Cs);
                    System.Diagnostics.Debug.WriteLine("CS info     : " + csManager.GetSpheresInfoCs());
                    System.Diagnostics.Debug.WriteLine("Command Type: " + pCommandType);

                    if (StrFunc.IsFilled(pText))
                    {
                        System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------");
                        System.Diagnostics.Debug.WriteLine(pText);
                        #region // FI 20130227 ajout des parameters
                        try
                        {
                            if (ArrFunc.IsFilled(commandParameters))
                            {
                                System.Diagnostics.Debug.WriteLine("Parameters----------------------------------------------------");

                                for (int i = 0; i < ArrFunc.Count(commandParameters); i++)
                                {
                                    if (null != commandParameters[i])
                                    {
                                        IDataParameter sqlParameter = (IDataParameter)commandParameters[i];
                                        string parameterName = sqlParameter.ParameterName;
                                        string value = "null";
                                        if (!(sqlParameter.Value is System.DBNull))
                                            value = sqlParameter.Value.ToString();

                                        string parameter = StrFunc.AppendFormat("Param =>{0} Value =>{1}", parameterName, value);
                                        System.Diagnostics.Debug.WriteLine(parameter);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // FI 20200910 [XXXXX] Appel à MessageExtended
                            System.Diagnostics.Debug.WriteLine(SpheresExceptionParser.GetSpheresException("Error on writting parameters", ex).MessageExtended);
                        }
                        #endregion
                        System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------");
                    }
                }
                else
                {
                    TimeSpan duration = (DateTime.Now - dtStartTraceQuery);

                    System.Diagnostics.Debug.Write("Elapsed time: " + string.Format("({0:N0},{1} sec.)",
                        duration.Days * 24 * 60 * 60 + duration.Hours * 60 * 60 + duration.Minutes * 60 + duration.Seconds,
                        StrFunc.IntegerPadding(duration.Milliseconds, 3)));

                    if (StrFunc.IsFilled(pComment))
                        System.Diagnostics.Debug.Write(" " + pComment);

                    System.Diagnostics.Debug.WriteLine(" (Cost " +
                        (duration.TotalMilliseconds >= 1000 ? ">= 1000" :
                        (duration.TotalMilliseconds >= 500 ? ">= 500" :
                        (duration.TotalMilliseconds >= 100 ? ">= 100" :
                        (duration.TotalMilliseconds >= 50 ? ">= 50" :
                        (duration.TotalMilliseconds >= 25 ? ">= 25" :
                        (duration.TotalMilliseconds >= 20 ? ">= 20" :
                        (duration.TotalMilliseconds >= 15 ? ">= 15" :
                        (duration.TotalMilliseconds >= 10 ? ">= 10" :
                        (duration.TotalMilliseconds >= 5 ? ">= 5" :
                        (duration.TotalMilliseconds > 0 ? "> 0" : "= 0")))))))))) +
                        " ms)");
                }
            }
#endif
        }
        public static void WriteRDBMSLog(string pConnectionString)
        {
            WriteRDBMSLog(false, pConnectionString, CommandType.StoredProcedure, string.Empty, null);
        }
        public static void WriteRDBMSLog(bool pIsStart, string pConnectionString, CommandType pCommandType, string pCommandText,
            params object[] commandParameters)
        {
            CSManager csManager = new CSManager(pConnectionString);
            bool isTraceRDBMS = IntFunc.IsFilledAndNoZero(csManager.IdA) || StrFunc.IsFilled(csManager.HostName);

#if DEBUG
            isTraceQueryForDebug |= isTraceRDBMS;
#endif
            WriteTraceQuery(pIsStart, pConnectionString, pCommandType.ToString(), pCommandText, commandParameters);

            if (isTraceRDBMS)
            {
                DbSvrType serverType = GetDbSvrType(pConnectionString);

                try
                {
                    if (pIsStart)
                    {
                        #region Insert into RDBMS_L
                        // RD 20100521 Pour afficher les détails du DAL
                        pCommandText = AddDALRefencedAssemblyInfos(serverType, pCommandText);

                        string commandParamVal = pCommandText;
                        if (false == commandParamVal.EndsWith(Cst.CrLf))
                            commandParamVal += Cst.CrLf;

                        if (ArrFunc.Count(commandParameters) > 0)
                        {
                            #region Replace Parameters with values
                            IDbDataParameter[] commandDbDataParameters = null;
                            try { commandDbDataParameters = (IDbDataParameter[])commandParameters; }
                            catch { };

                            if (null != commandDbDataParameters)
                                commandParamVal = DataHelper.ReplaceParametersInQuery(commandParamVal, serverType, commandDbDataParameters);
                            else
                                commandParamVal = commandParamVal + "(" + ArrFunc.GetStringList(commandParameters) + ")";
                            #endregion
                        }

                        string insertTrace = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.RDBMS_L.ToString();
                        insertTrace += "(DTSTART,COMMANDTYPE,COMMAND,COMMANDPARAMVAL,IDA,HOSTNAME,DTINS)";
                        insertTrace += SQLCst.VALUES;
                        insertTrace += "(@DTSTRART,@COMMANDTYPE,@COMMAND,@COMMANDPARAMVAL,@IDA,@HOSTNAME,@DTSTRART)";

                        DataParameters dataParameters = new DataParameters();
                        // FI 20200820 [25468] // FI 20200820 [25468] Dates systemes en UTC (add parameter DTSTART)
                        dataParameters.Add(DataParameter.GetParameter(csManager.Cs, DataParameter.ParameterEnum.DTSTART), OTCmlHelper.GetDateSysUTC(csManager.Cs));
                        dataParameters.Add(new DataParameter(csManager.Cs, pCommandType, "COMMANDTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pCommandType.ToString());
                        dataParameters.Add(new DataParameter(csManager.Cs, pCommandType, "COMMAND", DbType.AnsiString), pCommandText.Replace(@"\r\n", "\r\n"));
                        dataParameters.Add(new DataParameter(csManager.Cs, pCommandType, "COMMANDPARAMVAL", DbType.AnsiString), commandParamVal.Replace(@"\r\n", "\r\n"));
                        dataParameters.Add(new DataParameter(csManager.Cs, pCommandType, "IDA", DbType.Int32), csManager.IdA);
                        dataParameters.Add(new DataParameter(csManager.Cs, pCommandType, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN), csManager.HostName);

                        IDbDataParameter[] TraceParameters = dataParameters.GetArrayDbParameter();
                        insertTrace = TransformQuery2(csManager.Cs, CommandType.StoredProcedure, insertTrace, TraceParameters);
                        GetDAL(serverType).ExecuteNonQuery(csManager.Cs, CommandType.Text, insertTrace, TraceParameters);
                        #endregion
                    }
                    else
                    {
                        #region Update RDBMS_L with DTEND, and ELAPSEDTIME with the difference between DTEND and DTSTRAT
                        //FI 20111215 Cette requête ne marche pas sous Oracle
                        //RD 20111225 Correction de la requête pour Oracle
                        // FI 20200820 [25468] Alimentation de DTUPD 
                        // FI 20200820 [25468] DTSTART, DTEND en UTC
                        string updateTrace = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.RDBMS_L.ToString() + Cst.CrLf;
                        updateTrace += SQLCst.SET + "DTEND=" + DataHelper.SQLGetDate(serverType, true) + ",DTUPD =" + DataHelper.SQLGetDate(serverType, true) + ",";
                        updateTrace += "ELAPSEDTIME=" + DataHelper.SQLGetElapsedTime(serverType, "DTSTART", DataHelper.SQLGetDate(serverType, true)) + Cst.CrLf;
                        updateTrace += SQLCst.WHERE + "IDRDBMS_L=(" + Cst.CrLf;
                        updateTrace += SQLCst.SELECT + SQLCst.MAX + "(IDRDBMS_L)" + Cst.CrLf;
                        updateTrace += SQLCst.FROM_DBO + Cst.OTCml_TBL.RDBMS_L.ToString() + Cst.CrLf;
                        updateTrace += ")";

                        updateTrace = TransformQuery2(csManager.Cs, CommandType.StoredProcedure, updateTrace, null);
                        GetDAL(serverType).ExecuteNonQuery(csManager.Cs, CommandType.Text, updateTrace, null);
                        #endregion
                    }
                }
                catch (Exception e) { string s = e.Message; }
            }
        }
        #endregion

        #region IsQueryRDBMS
        /// <summary>
        /// Retourne true si {pQuery] commence par /* SQL RDBMS */ 
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static bool IsQueryRDBMS(string pQuery)
        {
            bool ret = false;
            if (StrFunc.IsFilled(pQuery))
                ret = (SQLCst.SQL_RDBMS.Length < pQuery.Length) && (SQLCst.SQL_RDBMS == pQuery.Substring(0, SQLCst.SQL_RDBMS.Length));
            return ret;
        }
        #endregion
        #region IsQueryANSI
        /// <summary>
        /// Retourne true si {pQuery] commence par /* SQL ANSI */ 
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static bool IsQueryANSI(string pQuery)
        {
            bool ret = false;
            if (StrFunc.IsFilled(pQuery))
                ret = (SQLCst.SQL_ANSI.Length < pQuery.Length) && (SQLCst.SQL_ANSI == pQuery.Substring(0, SQLCst.SQL_ANSI.Length));
            return ret;
        }
        #endregion
    }
    #endregion
    //
    #region DataHelper/ExecuteDataAdapter
    public sealed partial class DataHelper
    {
        #region public ExecuteDataAdapter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="commandText"></param>
        /// <param name="datatable"></param>
        /// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
        /// <returns></returns>
        /// EG 20180205 [23769] New
        /// FI 20220908 [XXXXX] Add updateBatchSize
        public static int ExecuteDataAdapter(string connectionString, IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            if (null == pDbTransaction)
                return ExecuteDataAdapter(connectionString, commandText, datatable, updateBatchSize);
            else
                return ExecuteDataAdapter(pDbTransaction, commandText, datatable, updateBatchSize);
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandText"></param>
        /// <param name="datatable"></param>
        /// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
        /// <returns></returns>
        /// FI 20220908 [XXXXX] Add updateBatchSize
        public static int ExecuteDataAdapter(string connectionString, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            DbSvrType ServerType = GetDbSvrType(connectionString);
            CSManager csManager = new CSManager(connectionString);
            commandText = TransformQuery2(csManager.Cs, CommandType.Text, commandText, null);

            //20060526 PL/FDA Warning: Pb Oracle on suprime les Commentaires => pose pb lors de la saisie des trades
            if (IsQueryRDBMS(commandText) || IsQueryANSI(commandText))
            {
                commandText = commandText.Replace(SQLCst.SQL_RDBMS, string.Empty);
                commandText = commandText.Replace(SQLCst.SQL_ANSI, string.Empty);
            }

            WriteRDBMSLog(true, connectionString, CommandType.Text, commandText);

            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();                
                ret = GetDAL(ServerType).ExecuteDataAdapter(csManager.Cs, commandText, datatable, updateBatchSize);
                TraceQueryExceededDuration(csManager, dtProfiler, $"ExecuteDataAdapter using command: {commandText}");
            }
            catch (Exception ex)
            {
                TraceQueryError(csManager, ex.Message, commandText);
                throw;
            }

            WriteRDBMSLog(connectionString);

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="commandText"></param>
        /// <param name="datatable"></param>
        /// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
        /// <returns></returns>
        /// FI 20220908 [XXXXX] Add updateBatchSize
        public static int ExecuteDataAdapter(IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            DbSvrType ServerType = GetDbSvrType(pDbTransaction);

            commandText = TransformQuery2(pDbTransaction.Connection.ConnectionString, CommandType.Text, commandText, null);

            //20060526 PL/FDA Warning: Pb Oracle on suprime les Commentaires => pose pb lors de la saisie des trades
            if (IsQueryRDBMS(commandText) || IsQueryANSI(commandText))
            {
                commandText = commandText.Replace(SQLCst.SQL_RDBMS, string.Empty);
                commandText = commandText.Replace(SQLCst.SQL_ANSI, string.Empty);
            }

            WriteRDBMSLog(true, pDbTransaction.Connection.ConnectionString, CommandType.Text, commandText);

            int ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(pDbTransaction.Connection.ConnectionString);
                ret = GetDAL(ServerType).ExecuteDataAdapter(pDbTransaction,  commandText, datatable, updateBatchSize);
                TraceQueryExceededDuration(csManager, dtProfiler, $"ExecuteDataAdapter using command: {commandText}");
            }
            catch (Exception ex)
            {
                CSManager csManager = new CSManager(pDbTransaction.Connection.ConnectionString);
                TraceQueryError(csManager, ex.Message, commandText);
                throw;
            }

            WriteRDBMSLog(pDbTransaction.Connection.ConnectionString);

            return ret;
        }
        #endregion ExecuteDataAdapter
        #region SetUpdatedEventHandler
        public static void SetUpdatedEventHandler(IDbTransaction pDbTransaction, DataAdapter pDa, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            DbSvrType ServerType = GetDbSvrType(pDbTransaction);
            GetDAL(ServerType).SetUpdatedEventHandler(pDa, pRowUpdating, pRowUpdated);
        }
        #endregion SetUpdatedEventHandler

        #region GetDataAdapter
        public static DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            out DataSet opFilledData, out IDbCommand opCommand)
        {

            DbSvrType ServerType = GetDbSvrType(pDbTransaction);
            pCommandText = TransformQuery2(pDbTransaction.Connection.ConnectionString, CommandType.Text, pCommandText, pCommandParameters);
            //20060526 PL/FDA Warning: Pb Oracle on suprime les Commentaires => pose pb lors de la saisie des trades
            if (IsQueryRDBMS(pCommandText) || IsQueryANSI(pCommandText))
            {
                pCommandText = pCommandText.Replace(SQLCst.SQL_RDBMS, string.Empty);
                pCommandText = pCommandText.Replace(SQLCst.SQL_ANSI, string.Empty);
            }

            WriteRDBMSLog(true, pDbTransaction.Connection.ConnectionString, CommandType.Text, pCommandText, pCommandParameters);

            DataAdapter ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(pDbTransaction.Connection.ConnectionString);
                ret = GetDAL(ServerType).GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, out opFilledData, out opCommand);
                TraceQueryExceededDuration(csManager, dtProfiler, pCommandText, pCommandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(new CSManager(pDbTransaction.Connection.ConnectionString), ex.Message, pCommandText, pCommandParameters);
                throw;
            }

            WriteRDBMSLog(pDbTransaction.Connection.ConnectionString);

            return ret;
        }
        public static DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters, string pTableName,
            ref DataSet pDataSet, out IDbCommand opCommand)
        {

            DbSvrType ServerType = GetDbSvrType(pDbTransaction);
            pCommandText = TransformQuery2(pDbTransaction.Connection.ConnectionString, CommandType.Text, pCommandText, pCommandParameters);
            //20060526 PL/FDA Warning: Pb Oracle on suprime les Commentaires => pose pb lors de la saisie des trades
            if (IsQueryRDBMS(pCommandText) || IsQueryANSI(pCommandText))
            {
                pCommandText = pCommandText.Replace(SQLCst.SQL_RDBMS, string.Empty);
                pCommandText = pCommandText.Replace(SQLCst.SQL_ANSI, string.Empty);
            }

            WriteRDBMSLog(true, pDbTransaction.Connection.ConnectionString, CommandType.Text, pCommandText, pCommandParameters);

            DataAdapter ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(pDbTransaction.Connection.ConnectionString);
                ret = GetDAL(ServerType).GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, pTableName, ref pDataSet, out opCommand);
                TraceQueryExceededDuration(csManager, dtProfiler, pCommandText, pCommandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(new CSManager(pDbTransaction.Connection.ConnectionString), ex.Message, pCommandText, pCommandParameters);
                throw;
            }

            WriteRDBMSLog(pDbTransaction.Connection.ConnectionString);

            return ret;
        }
        #endregion GetDataAdapter
        #region Update
        public static int Update(IDbTransaction pDbTransaction, DataAdapter pDa, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            DbSvrType ServerType = GetDbSvrType(pDbTransaction);
            return GetDAL(ServerType).Update(pDa, pDataTable, pRowUpdating, pRowUpdated);
        }
        #endregion Update
        #region Fill
        public static int Fill(IDbTransaction pDbTransaction, DataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            DbSvrType ServerType = GetDbSvrType(pDbTransaction);
            return GetDAL(ServerType).Fill(pDa, pDataSet, pTableName);
        }
        public static int Fill(string pConnectionString, DataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            DbSvrType ServerType = GetDbSvrType(pConnectionString);
            return GetDAL(ServerType).Fill(pDa, pDataSet, pTableName);
        }
        #endregion Fill
        #region SetSelectCommandText
        public static void SetSelectCommandText(IDbTransaction pDbTransaction, DataAdapter pDa, string pQuery)
        {
            SetSelectCommandText(pDbTransaction.Connection.ConnectionString, pDa, pQuery);
        }
        // EG 20160105 [34091]
        public static void SetSelectCommandText(string pConnectionString, DataAdapter pDa, string pQuery)
        {
            DbSvrType dbSvrType = GetDbSvrType(pConnectionString);
            string qry = TransformQuery2( pConnectionString, CommandType.Text, pQuery);
            // EG 20160126 [34091] Add Test
            if (IsQueryRDBMS(qry) || IsQueryANSI(qry))
            {
                qry = qry.Replace(SQLCst.SQL_RDBMS, string.Empty);
                qry = qry.Replace(SQLCst.SQL_ANSI, string.Empty);
            }
            GetDAL(dbSvrType).SetSelectCommandText(pDa, qry);
        }
        #endregion SetSelectCommandText

    }
    #endregion/ExecuteDataAdapter
    //
    #region DataHelper/ExecuteXmlReader
    public sealed partial class DataHelper
    {
        #region public ExecuteXmlReader
        #region ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteXmlReader(GetDbSvrType(connection), connection, commandType, commandText);
        }
        #endregion
        #region  ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteXmlReader(GetDbSvrType(connection), connection, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">a valid IDbConnection</param>
        /// <param name="spName">the name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteXmlReader(GetDbSvrType(connection), connection, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteXmlReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        public static XmlReader ExecuteXmlReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText)
        {
            // 20090206 RD 16480 (DAL) 
            return ExecuteXmlReader(ServerType, connection, commandType, commandText, null);
        }
        #endregion
        #region ExecuteXmlReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static XmlReader ExecuteXmlReader(DbSvrType ServerType, IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            WriteRDBMSLog(true, connection.ConnectionString, commandType, commandText, commandParameters);

            XmlReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = GetDAL(ServerType).ExecuteXmlReader(connection, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(new CSManager(connection.ConnectionString), ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        #region ExecuteXmlReader(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        public static XmlReader ExecuteXmlReader(DbSvrType ServerType, IDbConnection connection, string spName, params object[] parameterValues)
        {
            WriteRDBMSLog(true, connection.ConnectionString, CommandType.StoredProcedure, spName, parameterValues);

            XmlReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(connection.ConnectionString);
                ret = GetDAL(ServerType).ExecuteXmlReader(connection, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(new CSManager(connection.ConnectionString), ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(connection.ConnectionString);

            return ret;
        }
        #endregion
        //
        //
        #region ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return DataHelper.ExecuteXmlReader(GetDbSvrType(transaction), transaction, commandType, commandText);
        }
        #endregion
        #region ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return DataHelper.ExecuteXmlReader(GetDbSvrType(transaction), transaction, commandType, commandText, commandParameters);
        }
        #endregion
        #region ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">a valid IDbTransaction</param>
        /// <param name="spName">the name of the stored procedure</param>
        /// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>a dataset containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return DataHelper.ExecuteXmlReader(GetDbSvrType(transaction), transaction, spName, parameterValues);
        }
        #endregion
        //
        #region ExecuteXmlReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        public static XmlReader ExecuteXmlReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(ServerType, transaction, commandType, commandText, null);
        }
        #endregion
        #region ExecuteXmlReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        public static XmlReader ExecuteXmlReader(DbSvrType ServerType, IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            WriteRDBMSLog(true, transaction.Connection.ConnectionString, commandType, commandText, commandParameters);
            XmlReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(transaction.Connection.ConnectionString);
                ret = GetDAL(ServerType).ExecuteXmlReader(transaction, commandType, commandText, commandParameters);
                TraceQueryExceededDuration(csManager, dtProfiler, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                TraceQueryError(new CSManager(transaction.Connection.ConnectionString), ex.Message, commandText, commandParameters);
                throw;
            }

            WriteRDBMSLog(transaction.Connection.ConnectionString);

            return ret;
        }
        #endregion
        #region ExecuteXmlReader(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        public static XmlReader ExecuteXmlReader(DbSvrType ServerType, IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            WriteRDBMSLog(true, transaction.Connection.ConnectionString, CommandType.StoredProcedure, spName, parameterValues);

            XmlReader ret;
            try
            {
                DatetimeProfiler dtProfiler = new DatetimeProfiler();
                CSManager csManager = new CSManager(transaction.Connection.ConnectionString);
                ret = GetDAL(ServerType).ExecuteXmlReader(transaction, spName, parameterValues);
                TraceQueryExceededDuration(csManager, dtProfiler, spName, parameterValues);
            }
            catch (Exception ex)
            {
                TraceQueryError(new CSManager(transaction.Connection.ConnectionString), ex.Message, spName, parameterValues);
                throw;
            }

            WriteRDBMSLog(transaction.Connection.ConnectionString);

            return ret;
        }
        #endregion
        #endregion ExecuteXmlReader
    }
    #endregion
    //
    #region DataHelper/Transaction
    public partial class DataHelper
    {
        #region  public GetSvrInfoConnection
        // EG 20181010 PERF New
        public static SvrInfoConnection GetSvrInfoConnection(string pConnectionString)
        {
            string cs = new CSManager(pConnectionString).Cs;
            DbSvrType ServerType = GetDbSvrType(cs);
            return GetDAL(ServerType).GetSvrInfoConnection(cs);
        }
        #endregion GetSvrInfoConnection

        #region  public OpenConnection
        public static IDbConnection OpenConnection(string pConnectionString)
        {
            // 20090206 RD 16480 (DAL) 
            string cs = new CSManager(pConnectionString).Cs;
            DbSvrType ServerType = GetDbSvrType(cs);
            return GetDAL(ServerType).OpenConnection(cs);
        }
        #endregion
        #region public CloseConnection
        public static void CloseConnection(IDbConnection pDbConnection)
        {
            if (pDbConnection.State != ConnectionState.Closed)
                pDbConnection.Close();
        }
        #endregion
        #region public BeginTran
        public static IDbTransaction BeginTran(IDbConnection pConnection)
        {
            // 20090206 RD 16480 (DAL)
            DbSvrType ServerType = GetDbSvrType(pConnection);
            return GetDAL(ServerType).BeginTran(pConnection);
        }

        public static IDbTransaction BeginTran(string pConnectionString)
        {
            // 20130226 FI utilisation de la propterty CSManager(pConnectionString).cs
            string cs = new CSManager(pConnectionString).Cs;
            return DataHelper.BeginTran(cs, out _);
        }
        public static IDbTransaction BeginTran(string pConnectionString, IsolationLevel pIsolevel)
        {
            string cs = new CSManager(pConnectionString).Cs;
            DbSvrType ServerType = GetDbSvrType(cs);
            return GetDAL(ServerType).BeginTran(cs, pIsolevel);
        }
        public static IDbTransaction BeginTran(string pConnectionString, out IDbConnection opConnection)
        {
            // 20090206 RD 16480 (DAL) 
            string cs = new CSManager(pConnectionString).Cs;
            DbSvrType ServerType = GetDbSvrType(cs);
            return GetDAL(ServerType).BeginTran(cs, out opConnection);
        }
        #endregion
        #region public Commit
        /// <summary>
        /// Commit et fermeture de la connexion assocociée à la transaction
        /// </summary>
        /// <param name="pDbTransaction"></param>
        public static void CommitTran(IDbTransaction pDbTransaction)
        {
            bool isCloseConnection = true;
            DataHelper.CommitTran(pDbTransaction, isCloseConnection);
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void CommitTran(IDbTransaction pDbTransaction, bool pIsCloseConnection)
        {
            IDbConnection dbConnection = null;
            try
            {
             
                if (pIsCloseConnection)
                    dbConnection = pDbTransaction.Connection;
             
                pDbTransaction.Commit();

                if (pIsCloseConnection)
                {
                    dbConnection.Close();
                    // FI 20200723 [XXXXX]
                    dbConnection.Dispose();
                }
             
            }
            catch (Exception)
            {
                try
                {
                    if ((null != dbConnection) && pIsCloseConnection && (dbConnection.State != ConnectionState.Closed))
                        dbConnection.Close();
                }
                catch { }
                throw;
            }
        }
        #endregion
        #region public Rollback
        /// <summary>
        ///  Annule la transaction et ferme la connexion associée à la transaction  
        /// </summary>
        /// <param name="pDbTransaction">représente la transaction</param>
        public static void RollbackTran(IDbTransaction pDbTransaction)
        {
            bool isCloseConnection = true;
            DataHelper.RollbackTran(pDbTransaction, isCloseConnection);
        }
        /// <summary>
        ///  Annule la transaction
        /// </summary>
        /// <param name="pDbTransaction">représente la transaction</param>
        /// <param name="pIsCloseConnection">si true ferme la connexion associée à la transaction</param>
        /// EG 20180423 Analyse du code Correction [CA2200]
        public static void RollbackTran(IDbTransaction pDbTransaction, bool pIsCloseConnection)
        {

            IDbConnection dbConnection = null;
            try
            {
                if (pDbTransaction != null)
                {
                    // FI 20211130 [XXXXX] Ajout test sur IsTransactionValid car un Rollback sur une transaction non valide lève une InvalidOperationException
                    if (IsTransactionValid(pDbTransaction))
                    {

                        if (pIsCloseConnection)
                            dbConnection = pDbTransaction.Connection;

                        pDbTransaction.Rollback();

                        if ((pIsCloseConnection) && (null != dbConnection))
                        {
                            dbConnection.Close();
                            // FI 20200723 [XXXXX]
                            dbConnection.Dispose();
                        }
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    if (pIsCloseConnection && (null != dbConnection) && (dbConnection.State != ConnectionState.Closed))
                        dbConnection.Close();
                }
                catch
                { }
                throw;
            }
        }
        #endregion

        /// <summary>
        ///  Return false if the transaction is no longer valid
        ///  <para>Sur SQLSERVER une transaction peut devenir non valide lorsque l'exécution d'une requête provoque une erreur sévère.</para> 
        ///  <para>Si tel est le cas, SQLSERVER fait un roolback auto et tue la connexion. L'objet dbTransaction n'est alors plus valide (Zombie)</para> 
        /// </summary>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        /// FI 20211130 [XXXXX] Add Method
        public static Boolean IsTransactionValid(IDbTransaction dbTransaction)
        {
            if (null == dbTransaction)
                throw new ArgumentNullException("dbtransaction is null");

            // Connection property can be null if the transaction is no longer valid
            // https://docs.microsoft.com/en-gb/dotnet/api/system.data.sqlclient.sqltransaction.connection?view=dotnet-plat-ext-6.0#System_Data_SqlClient_SqlTransaction_Connection

            return dbTransaction.Connection != null;
        }
    }
    #endregion
    //
    #region DataHelper
    public partial class DataHelper
    {
        #region IsParamDirection
        public static bool IsParamDirection(string pParamDirection, ParameterDirection pDirection)
        {
            bool isParamDirection = false;
            if (StrFunc.IsFilled(pParamDirection))
            {
                ParameterDirection paramDirectionEnum = (ParameterDirection)Enum.Parse(typeof(ParameterDirection), pParamDirection);
                isParamDirection = (paramDirectionEnum == pDirection);
            }
            return isParamDirection;
        }
        #endregion IsParamDirection
        #region IsParamDirectionInput
        public static bool IsParamDirectionInput(string pParamDirection)
        {
            return IsParamDirection(pParamDirection, ParameterDirection.Input);
        }
        #endregion IsParamDirectionInput
        #region IsParamDirectionOutput
        public static bool IsParamDirectionOutput(string pParamDirection)
        {
            return IsParamDirection(pParamDirection, ParameterDirection.Output);
        }
        #endregion IsParamDirectionOutput
        #region IsParamDirectionInputOutput
        public static bool IsParamDirectionInputOutput(string pParamDirection)
        {
            return IsParamDirection(pParamDirection, ParameterDirection.InputOutput);
        }

        #endregion IsParamDirectionInputOutput

        /// <summary>
        /// Génère une nouvelle table vierge de données à partir de la table {pTableName}
        /// <para>La table {pTableName} est présente ds le schéma dbo</para>
        /// <para>Cette table est générée ds le schéma associé à la connexion</para>
        /// <para>Utilise la méthode select into sous SqlServer®</para>
        /// <para>Utilise la méthode create table as select sous Oracle®</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableNameSource">Nom de la table modèle</param>
        /// <param name="pTableNameTarget">Nom de la table à générer</param>
        /// <param name="pCommandText">Récupère la commande exécutée</param>
        public static void CreateTableAsSelect(string pCS, string pTableNameSource, string pTableNameTarget, out string pCommandText)
        {
            pCommandText = null;
            if (false == IsExistTable(pCS, pTableNameTarget))
            {
                StrBuilder cmd = new StrBuilder();
                if (IsDbSqlServer(pCS))
                {
                    cmd += SQLCst.SELECT_ALL + Cst.CrLf;
                    cmd += StrFunc.AppendFormat("INTO {0}", pTableNameTarget) + Cst.CrLf;
                    cmd += StrFunc.AppendFormat(SQLCst.FROM_DBO + "{0}", pTableNameSource) + Cst.CrLf;
                    cmd += SQLCst.WHERE + "1=0";
                }
                else if (IsDbOracle(pCS))
                {
                    cmd += StrFunc.AppendFormat("CREATE TABLE {0} AS ", pTableNameTarget);
                    cmd += Cst.CrLf;
                    cmd += SQLCst.SELECT_ALL + Cst.CrLf;
                    cmd += StrFunc.AppendFormat(SQLCst.FROM_DBO + "{0}", pTableNameSource) + Cst.CrLf;
                    cmd += SQLCst.WHERE + "1=0";
                }
                else
                    throw new NotImplementedException("RDBMS not implemented");

                pCommandText = cmd.ToString();
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, cmd.ToString());
            }
        }
        /// <summary>
        /// Génère une nouvelle table vierge de données à partir de la table {pTableName}
        /// <para>La table {pTableName} est présente ds le schéma dbo</para>
        /// <para>Cette table est générée ds le schéma associé à la connexion</para>
        /// <para>Utilise la méthode select into sous SqlServer®</para>
        /// <para>Utilise la méthode create table as select sous Oracle®</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableNameSource">Nom de la table modèle</param>
        /// <param name="pTableNameTarget">Nom de la table à générer</param>
        public static void CreateTableAsSelect(string pCS, string pTableNameSource, string pTableNameTarget)
        {
            CreateTableAsSelect(pCS, pTableNameSource, pTableNameTarget, out _);
        }

        /// <summary>
        /// Retourne true si la table {pTable} existe ds le schéma associé à la connexion
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable"></param>
        /// <returns></returns>
        public static bool IsExistTable(string pCS, string pTable)
        {
            string cmd;
            if (DataHelper.IsDbSqlServer(pCS))
            {
                cmd = StrFunc.AppendFormat("select name from sysobjects where name = '{0}' and xtype='U'", pTable);
            }
            else if (DataHelper.IsDbOracle(pCS))
            {
                cmd = StrFunc.AppendFormat("select TABLE_NAME from USER_TABLES where TABLE_NAME = '{0}'", pTable);
            }
            else
                throw new NotImplementedException("RDBMS not implemented");

            object tableName = DataHelper.ExecuteScalar(pCS, CommandType.Text, cmd);
            return (null != tableName);
        }

        /// <summary>
        /// Retourne true si la colonne {pColumn} de la vue/table {pTable} existe dans le schéma associé à la connexion
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable"></param>
        /// <param name="pColumn"></param>
        /// <returns></returns>
        public static bool IsExistColumn(string pCS, string pTable, string pColumn)
        {
            string cmd;
            if (DataHelper.IsDbSqlServer(pCS))
            {
                cmd = StrFunc.AppendFormat("select name from syscolumns where id = OBJECT_ID('{0}') and name = '{1}'", pTable, pColumn);
            }
            else if (DataHelper.IsDbOracle(pCS))
            {
                cmd = StrFunc.AppendFormat(@"select atc.COLUMN_NAME
                from ALL_TAB_COLUMNS atc
                left outer join USER_TAB_PRIVS utp on (utp.TABLE_NAME = atc.TABLE_NAME) and (utp.OWNER = atc.OWNER) and (utp.GRANTEE = user)
                where (atc.TABLE_NAME = '{0}') and (atc.COLUMN_NAME = '{1}') and ((atc.OWNER = user) or (utp.TABLE_NAME is not null))", pTable, pColumn);
            }
            else
                throw new NotImplementedException("RDBMS not implemented");

            object tableName = DataHelper.ExecuteScalar(pCS, CommandType.Text, cmd);
            return (null != tableName);
        }

        /// <summary>
        /// Purge du cache (système) du serveur de données: Plans d'exécution, Données montées en mémoire, etc. 
        /// </summary>
        /// <param name="pCS"></param>
        public static void FlushRDBMSCache(string pCS)
        {
            if (DataHelper.IsDbSqlServer(pCS))
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, "CHECKPOINT;" + Cst.CrLf + "DBCC DROPCLEANBUFFERS;" + Cst.CrLf + "DBCC FREEPROCCACHE;");
            //PL 20181119 Sous Oracle la purge nécessite une connexion SYSDBA
            //else if (DataHelper.isDbOracle(pCS))
            //    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, "alter system flush shared_pool;" + Cst.CrLf + "alter system flush buffer_cache;");
        }

        /// <summary>
        /// Retourne la commande {pSqlCommand} au format du SGBD encapsulé avec les informations du DAL
        /// <para>Utilisé pour l'affichage des requêtes en mode trace</para>
        /// </summary>
        /// <param name="pCommand"></param>
        /// FI 20180910 [XXXXX] Add
        public static string SqlCommandToDisplay(string pCS, QueryParameters pQuery)
        {
            return SqlCommandToDisplay(pCS, pQuery.QueryReplaceParameters);
        }

        /// <summary>
        /// Retourne la commande {pSqlCommand} au format du SGBD encapsulé avec les informations du DAL
        /// <para>Utilisé pour l'affichage des requêtes en mode trace</para>
        /// </summary>
        /// <param name="pCommand"></param>
        /// FI 20180910 [XXXXX] Add
        public static string SqlCommandToDisplay(string pCS, string pSqlCommand)
        {
            CSManager csManager = new CSManager(pCS);
            DbSvrType serverType = csManager.GetDbSvrType();

            string ret = DataHelper.TransformQuery2(csManager.Cs, CommandType.Text, pSqlCommand, null);
            ret = DataHelper.AddDALRefencedAssemblyInfos(serverType, ret);
            return ret;
        }
    }
    #endregion DataHelper

    #region TraceQuery
    /// <summary>
    /// 
    /// </summary>
    public partial class DataHelper
    {
        /// <summary>
        /// Durée limite pour exécution d'une requête (valeur en seconde)
        /// <para>Au dela de cette durée, un warning est inscrit dans le journal</para>
        /// <para>si la valeur -1 => aucun warning, la valeur est définie à partir de la classe <see cref="SpheresTraceSource"/></para>
        /// </summary>
        public static int sqlDurationLimit = -1;

        /// <summary>
        /// Méthode deleguée pour écriture d'une erreur dans le journal
        /// </summary>
        /// FI 210170705 [XXXXX] add
        public static TraceQueryError traceQueryError;

        /// <summary>
        /// Méthode deleguée pour écriture d'un warning dans le journal
        /// </summary>
        /// FI 210170705 [XXXXX] add
        public static TraceQueryWarning traceQueryWarning;

        /// <summary>
        /// Ecriture dans le journal d'une Error lorsqu'une requête produit une exception
        /// </summary>
        /// <param name="pExceptionMessage">Message d'erreur issu de moteur SQL</param>
        /// <param name="pCommand">commande SQL</param>
        ///<param name="pParameters"></param>
        /// FI 210170705 [XXXXX] add
        /// AL 20240415 [WI891] Added CSManager parameter to dump connection string informations
        private static void TraceQueryError(CSManager pCsManager, string pExceptionMessage, string pCommand, params object[] pParameters)
        {
            if (null != traceQueryError)
            {
                StrBuilder message = new StrBuilder();
                message.AppendFormat("SQL:{0}", pCommand);
                message.Append(Cst.CrLf);
                if (ArrFunc.Count(pParameters) > 0)
                {
                    message.AppendFormat("Params:{0}", TraceGetParametersInfo(pParameters));
                    message.Append(Cst.CrLf);
                }
                message.AppendFormat("Exception:{0}", pExceptionMessage);
                //AL 20240415 [WI891] Dump connection string informations
                if (pCsManager != null) {                    
                    message.Append(Cst.CrLf);
                    message.AppendFormat("Connection Info:{0}", pCsManager.GetCSAnonymizePwd());
                }
                traceQueryError.Invoke(new DataHelper(), message.ToString());
            }
        }

        /// <summary>
        /// Ecriture dans le journal d'un warning lorsqu'une la durée qu'une instruction SQL est trop longgue
        /// </summary>
        /// <param name="pDateProfiler"></param>
        /// <param name="pCommand">commande SQL</param>
        /// <param name="pParameters"></param>
        /// EG 20230223 [XXXXX] Correction Bug sur contrôle DurationLimit : usage de TotalSeconds à la place de Seconds
        /// AL 20240415 [WI891] Added CSManager parameter to dump connection string informations
        private static void TraceQueryExceededDuration(CSManager pCsManager, DatetimeProfiler pDateProfiler, string pCommand, params object[] pParameters)
        {
            if ((null != traceQueryWarning) && sqlDurationLimit != -1)
            {
                TimeSpan timespan = pDateProfiler.GetTimeSpan();
                // EG 20230223 USage de TotalSeconds 
                if (timespan.TotalSeconds >= sqlDurationLimit)
                {
                    StrBuilder message = new StrBuilder();
                    message.AppendFormat("SQL:{0}", pCommand);
                    message.Append(Cst.CrLf);
                    if (ArrFunc.Count(pParameters) > 0)
                    {
                        message.AppendFormat("Params:{0}", TraceGetParametersInfo(pParameters));
                        message.Append(Cst.CrLf);
                    }
                    message.AppendFormat("Exceeded duration:{0}", timespan.ToString(@"hh\:mm\:ss\.ffffff"));
                    
                    //AL 20240415 [WI891] Dump connection string informations
                    if (pCsManager != null)
                    {
                        message.Append(Cst.CrLf);
                        message.AppendFormat("Connection Info:{0}", pCsManager.GetCSAnonymizePwd());
                    }
                    traceQueryWarning.Invoke(new DataHelper(), message.ToString());
                }
            }
        }

        /// <summary>
        ///  Ecriture des paramètres dans la trace 
        /// </summary>
        /// <param name="pParameters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// FI 20190725 [XXXXX] Add Method
        /// EG 20230227 [XXXXX] Add test null for pParameters[0] and item
        private static string TraceGetParametersInfo(object[] pParameters)
        {
            StrBuilder ret = new StrBuilder();
            if (null == pParameters)
                throw new ArgumentNullException("pParameters is null.");

            Boolean isDbDataParameter = (null != pParameters[0]) && (null != pParameters[0].GetType().GetInterface(typeof(IDbDataParameter).Name));
            if (isDbDataParameter)
            {
                IDbDataParameter[] dbDataParameters = (IDbDataParameter[])pParameters;
                foreach (IDbDataParameter item in dbDataParameters)
                {
                    if (null != item)
                    {
                        TypeData.TypeDataEnum typeData = TypeData.GetTypeFromSystemType(item.Value.GetType());

                        string value = Convert.IsDBNull(item.Value) ? "NULL" : ObjFunc.FmtToISo(item.Value, typeData);
                        ret.Append(StrFunc.AppendFormat("[{0}:{1}]", item.ParameterName, value));
                    }
                }
            }
            else
            {
                foreach (object item in pParameters)
                {
                    if (null != item)
                    {
                        TypeData.TypeDataEnum typeData = TypeData.GetTypeFromSystemType(item.GetType());
                        string value = Convert.IsDBNull(item) ? "NULL" : ObjFunc.FmtToISo(item, typeData);
                        ret.Append(StrFunc.AppendFormat("[{0}]", value));
                    }
                }
            }
            return ret.ToString();
        }
    }
    #endregion

    #region DataHelper / Bulk
    /// <summary>
    /// 
    /// </summary>
    public partial class DataHelper
    {
        public static void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt)
        {
            GetDAL(GetDbSvrType(pConnectionString)).BulkWriteToServer(pConnectionString, pDestinationTable, pDt);
        }
    }
    #endregion DataHelper / Bulk

    #endregion Class DataHelper

    #region Class DataHelper<T>

    /// <summary>
    /// DataHelper Template class : DataHelper extention class 
    /// to fetch DataSet directly into a generic T type
    /// </summary>
    /// <typeparam name="T">
    /// <list type="">
    /// <listheader>T types</listheader>
    /// <item>T must be a class implementing or defining a data contract.</item>
    /// <item>T is built by the LINQ to SQL model</item>
    /// </list>
    /// <example>
    /// Example to fetch the former two fields of the IOCOMPARE table into the IOCompare class
    /// <code>
    ///[DataContract(Name = DataHelper<T>.DATASETROWNAME, Namespace = DataHelper<T>.DATASETNAMESPACE)]
    ///public class IOCompare
    ///{
    ///    private string m_IdIoCompare;
    ///
    ///   [DataMember(Name = "IDIOCOMPARE", Order = 1)]
    ///   public string IdIoCompare
    ///   {
    ///       get { return m_IdIoCompare; }
    ///       set { m_IdIoCompare = value; }
    ///   }

    ///   private string m_DisplayName;

    ///   [DataMember(Name = "DISPLAYNAME", Order = 2)]
    ///   public string DisplayName
    ///   {
    ///       get { return m_DisplayName; }
    ///       set { m_DisplayName = value; }
    ///   }
    ///}
    /// </code>
    /// </example>
    /// </typeparam>
    public static class DataHelper<T>
    {
        /// <summary>
        /// Root name of a DataSet element as defined in the 3.0+ .Net Framework
        /// </summary>
        public const string DATASETNODENAME = "NewDataSet";

        /// <summary>
        /// Row name of any DataSet element as defined in the 3.0+ .Net Framework
        /// </summary>
        public const string DATASETROWNAME = "Table";
        public const string CA_DC = "DERIVATIVECONTRACT";
        public const string CA_DA = "DERIVATIVEATTRIB";
        public const string CA_ASSET = "ASSET_ETD";

        /// <summary>
        /// Default namespace of a Xml DataSet element as defined in the 3.0+ .Net Framework
        /// </summary>
        public const string DATASETNAMESPACE = "";

        // UNDONE MF 20120327 Enrichir la condition "negative lookbehind" (?<!UT_NUMBER10|UT_VALUE) à fur et à mesure des besoins 
        /// <summary>
        /// Parsing regex for the Microsoft SQL server XQuery 'value' function, to use with NOT numeric DB data types only
        /// </summary>
        private static readonly Regex regXQueryParseNotNumericValueFunction =
            new Regex(@"([\w\.]+)\.value\('\(([\w/:\(\)]+)(/@[\w]+)*\)(\[\d+\])','([\w\(\)]+)(?<!UT_NUMBER10|UT_VALUE)'\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        // UNDONE MF 20120327 Enrichir la condition "positive lookbehind" (?<=UT_NUMBER10|UT_VALUE) à fur et à mesure des besoins 
        /// <summary>
        /// Parsing regex for the Microsoft SQL server XQuery 'value' function, to use for numeric DB data types only
        /// </summary>
        private static readonly Regex regXQueryParseNumericValueFunction =
            new Regex(@"([\w\.]+)\.value\('\(([\w/:\(\)]+)(/@[\w]+)*\)(\[\d+\])','([\w\(\)]+)(?<=UT_NUMBER10|UT_VALUE)'\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        // 20120613 MF add the @ char to retrieve attributes too
        /// <summary>
        /// Parsing regex for the Microsoft SQL server XQuery 'exist' function, provided with a condition on a attribute value
        /// </summary>
        private static readonly Regex reXQueryParseExistFunction = new Regex(@"([\w\.]+).exist\('\(([\w/:\(\)]+\[@[\w]+=""[\w\-\:]+""\])\)'\)\s*=\s*1",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parsing regex for the Microsoft SQL server XQuery 'exist' function, straight on the node/attribute name
        /// </summary>
        private static readonly Regex reXQueryParseExistFunctionWithoutAttrCond = new Regex(@"([\w\.]+).exist\('\(([\w/:@\(\)]+)\)'\)\s*=\s*1",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parsing regex for the Microsoft SQL server 'cast' function
        /// </summary>
        private static readonly Regex regParseCast = new Regex(@"cast\s*\(\s*([a-z\(\)]*)\s*as\s*\w*\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parsing regex for the Microsoft SQl server 'datalength' and 'len' function
        /// </summary>
        private static readonly Regex regParseDatalength = new Regex(@"(datalength|len)\s*\(\s*([\w-\.]+)\s*\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// parsing regex for the Microsoft SQL Server 'replicate' function.
        /// the sequence to replicate must be 1 character long [\w\.-]{1}.
        /// </summary>
        private static readonly Regex regParseReplicate = new Regex(@"replicate\s*\(\s*'([\w\.-]{1})'\s*,\s*([\s\w\(\)\.-]+)\s*\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);


        private static readonly Regex regParseIsNull = new Regex(@"isnull\s*\(\s*([\w\.]+)\s*,\s*([\w\d]+)\)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Execute a DataSet and cast that to a list of T elements
        /// </summary>
        /// <param name="connection">connection object</param>
        /// <param name="commandType">SQL request type</param>
        /// <param name="commandText">the SQL request</param>
        /// <exception cref="Exception">One error is occurred during the fetch of the dataset, 
        /// or during the cast to the T class
        /// </exception>
        /// <returns>A list of type T objects,  T must be a type implementing or defining a data contract</returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static List<T> ExecuteDataSet(IDbConnection connection, CommandType commandType, string commandText)
        {
            List<T> ret = null;

            try
            {
                DataSet dataset = DataHelper.ExecuteDataset(connection, commandType, commandText);

                // forcing the row name for oracle dataset
                if (dataset.Tables[0].TableName != DATASETROWNAME)
                    dataset.Tables[0].TableName = DATASETROWNAME;

                using (Stream xmlinput = new MemoryStream(Encoding.UTF8.GetBytes(dataset.GetXml())))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(List<T>), DATASETNODENAME, DATASETNAMESPACE);

                    ret = (List<T>)ser.ReadObject(xmlinput);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return ret;
        }

        /// <summary>
        /// Execute a DataSet and cast that to a list of T elements
        /// </summary>
        /// <param name="connection">DB connection</param>
        /// <param name="commandType">SQL request type</param>
        /// <param name="commandText">SQL request </param>
        /// <param name="commandParameters">parameters for the SQL request</param>
        /// <exception cref="SpheresException2">exception raised during the fetch of the dataset or the cast to the T class</exception>
        /// <returns></returns>
        public static List<T> ExecuteDataSet(IDbConnection connection, CommandType commandType, string commandText,
            IDbDataParameter[] commandParameters)
        {
            List<T> ret;

            try
            {
                DataSet dataset = DataHelper.ExecuteDataset(connection, commandType, commandText, commandParameters);

                // forcing the row name for oracle dataset
                if (dataset.Tables[0].TableName != DATASETROWNAME)
                    dataset.Tables[0].TableName = DATASETROWNAME;

                ret = TransformDataSet(dataset);

                dataset.Dispose();

                //using (Stream xmlinput = new MemoryStream(Encoding.UTF8.GetBytes(dataset.GetXml())))
                //{
                //    DataContractSerializer ser = new DataContractSerializer(typeof(List<T>), DATASETNODENAME, DATASETNAMESPACE);

                //    ret = (List<T>)ser.ReadObject(xmlinput);
                //}

            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
            return ret;
        }

        public static List<T> TransformDataSet(DataSet pDataset)
        {
            List<T> ret = null;

            try
            {
                using (Stream xmlinput = new MemoryStream(Encoding.UTF8.GetBytes(pDataset.GetXml())))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(List<T>), DATASETNODENAME, DATASETNAMESPACE);

                    ret = (List<T>)ser.ReadObject(xmlinput);
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
            return ret;
        }

        /// <summary>
        /// Transform the input dataset in a objects collection of the given type T.
        /// </summary>
        /// <param name="pDataset">Input dataset, can be empty but must be not null</param>
        /// <param name="pSplitIndex">split index of the transformation, 
        /// the transformation will be performed extracting form the dataset n datarows per step, with n equals to pSplitIndex</param>
        /// <returns>A collection of objects type T, an object  will be returned for each datarow of the input dataset pDataset</returns>
        public static List<T> TransformDataSet(DataSet pDataset, int pSplitIndex)
        {
            if (pSplitIndex <= 0 || pDataset == null)
            {
                throw new ArgumentException("The TransformDataSet call is malformed, pass a positive index and one not null dataset");
            }

            IEnumerable<T> ret = new List<T>();
            DataTable sourceDataTable = pDataset.Tables[0];

            DataSet tempDataSet = pDataset.Clone();
            DataTable tempDataTable = tempDataSet.Tables[0];

            int currentIndex = 0;
            int nextIndex = pSplitIndex < sourceDataTable.Rows.Count ? pSplitIndex : sourceDataTable.Rows.Count;

            while (currentIndex < sourceDataTable.Rows.Count)
            {
                tempDataTable.Rows.Clear();

                for (int copyIndex = currentIndex; copyIndex < nextIndex; copyIndex++)
                {
                    tempDataTable.ImportRow(sourceDataTable.Rows[copyIndex]);
                }

                ret = ret.Union(TransformDataSet(tempDataSet));

                currentIndex = nextIndex;
                nextIndex += pSplitIndex;

                if (nextIndex > sourceDataTable.Rows.Count)
                {
                    nextIndex = sourceDataTable.Rows.Count;
                }
            }

            return ret.ToList();
        }

        /// <summary>
        /// Remplace '[node].value' and '[node].exists' properties 
        /// with the relative functions 'extractvalue([node], ...)' and 'existsnode([node], ...)'.
        /// The input command text is completed with the default XML namespaces. 
        /// The translation is one way only, from the SQL server syntax to the Oracle syntax.
        /// </summary>
        /// <param name="connection">the current connection string in order to extract the DBMS value</param>
        /// <param name="commandType">the command type</param>
        /// <param name="commandText">the sql request to be translated from the Microsoft syntax to the current connection DBMS syntax</param>
        /// <returns>the translated query including default XML namespaces</returns>
        static public string XQueryTransform(IDbConnection connection, CommandType commandType, string commandText)
        {
            return XQueryTransform(connection.ConnectionString, commandType, commandText);
        }

        static public string XQueryTransform(string pCS, CommandType commandType, string commandText)
        {
            if (commandType != CommandType.Text)
                throw new NotSupportedException("Text command is supported");
            DbSvrType typeDB = DataHelper.GetDbSvrType(pCS);


            string res;
            switch (typeDB)
            {

                case DbSvrType.dbORA:

                    // MF 20120327 
                    //  1. cast numeric values, extracted by xquery expression, 
                    //  2. taking care of index

                    res = regXQueryParseNotNumericValueFunction.Replace(commandText,
                                @"extractvalue($1,'$2$4$3','{0}')");
                    // the given Oracle format value is compatible with this maximal precision: numeric(26,9) (or UT_VALUE as Spheres type)
                    res = regXQueryParseNumericValueFunction.Replace(res,
                                @"TO_NUMBER(extractvalue($1,'$2$4$3','{0}'), '999999999999999.999999999')");

                    res = String.Format(res, OTCmlHelper.GetXMLNamespace_3_0(pCS));

                    res = reXQueryParseExistFunction.Replace(res, @"existsnode($1, '$2', '{0}') = 1");
                    res = reXQueryParseExistFunctionWithoutAttrCond.Replace(res, @"existsnode($1, '$2', '{0}') = 1");

                    res = String.Format(res, OTCmlHelper.GetXMLNamespace_3_0(pCS));

                    break;

                case DbSvrType.dbSQL:

                    res = String.Format(@"
                    WITH XMLNAMESPACES (
                        {0} as fixml,
                        {1} as fpml,
                        {2} as efs,
                        DEFAULT {3})
                    {4}",
                         DataHelper.SQLString(Cst.FixML_Namespace_5_0_SP1),
                         DataHelper.SQLString(Cst.FpML_Namespace_4_4),
                         DataHelper.SQLString(Cst.EFSmL_Namespace_3_0),
                         DataHelper.SQLString(Cst.FpML_Namespace_4_4),
                         commandText);

                    break;

                default:
                    throw new NotSupportedException();
            }

            return res;
        }

        /// <summary>
        /// Remove all the 'cast [columnname] as [type]' structs, replacing them with the simple [columnname] value.
        /// The translation is one way only, from the SQL server syntax to the Oracle syntax.
        /// </summary>
        /// <param name="connection">the current connection string in order to extract the DBMS value</param>
        /// <param name="commandType">the command type</param>
        /// <param name="commandText">the sql request to be translated from the Microsoft syntax to the current connection DBMS syntax</param>
        /// <returns></returns>
        static public string CastTransform(IDbConnection connection, CommandType commandType, string commandText)
        {
            if (commandType != CommandType.Text)
                throw new NotSupportedException("Text command type only");
            DbSvrType typeDB = DataHelper.GetDbSvrType(connection);


            string res;
            switch (typeDB)
            {

                case DbSvrType.dbORA:

                    res = regParseCast.Replace(commandText, @"$1");

                    break;

                case DbSvrType.dbSQL:

                    res = commandText;

                    break;

                default:
                    throw new NotSupportedException();
            }

            return res;
        }

        /// <summary>
        /// Remove all the SQL server 'replicate(...)' functions, replacing them with the corresponding DBMS function.
        /// The translation is one way only, from the SQL server syntax to the Oracle syntax.
        /// </summary>
        /// <param name="connection">the current connection string in order to extract the DBMS value</param>
        /// <param name="commandType">the command type</param>
        /// <param name="commandText">the sql request to be translated from the Microsoft syntax to the current connection DBMS syntax</param>
        /// <returns></returns>
        static public string ReplicateTransform(IDbConnection connection, CommandType commandType, string commandText)
        {
            // TODO MF 20101214 - to test...

            if (commandType != CommandType.Text)
                throw new NotSupportedException("Text command type only");

            DbSvrType typeDB = DataHelper.GetDbSvrType(connection);

            string res;
            switch (typeDB)
            {

                case DbSvrType.dbORA:

                    // the sql server 'replicate' function does not have a quite same function in the Oracle API, but we have to use the oracle 'rpad' function.
                    //  To force on Oracle the behaviour of a 'replicate' function then we pass to the first parameter the same string $1 we pass as third parameter.
                    //  Then we pass as second parameter the number of times $2 we want to replicate $1 less 1.
                    //  1 must be the lenght of the string we want to replicate. see also the regParseReplicate definition.
                    res = regParseReplicate.Replace(commandText, @"rpad('$1', $2 - 1, '$1')");

                    break;

                case DbSvrType.dbSQL:

                    res = commandText;

                    break;

                default:
                    throw new NotSupportedException();
            }

            return res;
        }

        public static string IsNullTransform(IDbConnection connection, CommandType commandType, string commandText)
        {
            if (commandType != CommandType.Text)
                throw new NotSupportedException("Text command type only");

            DbSvrType typeDB = DataHelper.GetDbSvrType(connection);

            string res;
            switch (typeDB)
            {

                case DbSvrType.dbORA:

                    res = regParseIsNull.Replace(commandText, @"nvl($1,$2)");

                    break;

                case DbSvrType.dbSQL:

                    res = commandText;

                    break;

                default:
                    throw new NotSupportedException();
            }

            return res;
        }

        /// <summary>
        /// Build the structure of a SQL table typeof T
        /// </summary>
        /// <returns>A structured datatable</returns>
        public static DataTable BuildTableStructure()
        {
            DataTable retTable = new DataTable();

            foreach (var column in typeof(T).GetProperties())
            {
                Type columnType = column.PropertyType;

                if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    columnType = Nullable.GetUnderlyingType(columnType);

                if (column.Name ==
                        Enum.GetName(typeof(EFS.ACommon.Cst.OTCml_COL), EFS.ACommon.Cst.OTCml_COL.ROWVERSION))
                    continue;

                retTable.Columns.Add(column.Name, columnType);
            }

            return retTable;
        }

    }

    #endregion Class DataHelper<T>

    #region public class DataDbTransaction
    public class DataDbTransaction
    {
        #region Members
        private readonly CSManager _csManger;
        private IDbTransaction _transaction;
        #endregion

        #region Accessor
        /// <summary>
        /// Obtient la ConnectionString  utilisé pour l'exécution des requêtes sur le SGBD  (sans attribut Spheres)
        /// </summary>
        public string Cs
        {
            get { return _csManger.Cs; }
        }
        /// <summary>
        /// Obtient la ConnectionString enrichie des attributs specifiques a Spheres
        /// Exemple {SpheresCache = true;hostName = xxxx;}
        /// </summary>
        public string CsSpheres
        {
            get { return _csManger.CsSpheres; }
        }
        /// <summary>
        /// Obtient ou définie la transaction
        /// </summary>
        public IDbTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }
        public DbSvrType ServerType
        {
            get { return DataHelper.GetDbSvrType(Transaction); }
        }
        #endregion

        #region  constructor
        public DataDbTransaction(string pCS, IDbTransaction pTransaction)
        {
            _csManger = new CSManager(pCS);
            _transaction = pTransaction;
        }
        #endregion
    }
    #endregion

    #region public class DatasetSerializer
    /// <summary>
    /// Class utilisé pour la serialization des DataSet
    /// <para>Possibilité de spécifier le format des dates </para>
    /// </summary>
    public class DatasetSerializer
    {
        #region Members
        readonly DataSet _dataSet;
        readonly string _dateFormat;
        #endregion Members
        //
        #region constructor
        public DatasetSerializer(DataSet pDataSet)
            : this(pDataSet, string.Empty)
        {
        }
        public DatasetSerializer(DataSet pDataSet, string pDateFormat)
        {
            _dataSet = pDataSet;
            _dataSet.RemotingFormat = SerializationFormat.Xml;
            _dateFormat = pDateFormat;
        }
        #endregion constructor
        //
        #region public Serialize
        public String Serialize()
        {
            StringBuilder sb;
            TextWriter writer = null;
            try
            {
                sb = new StringBuilder();
                DataSetXmlTextWritter dsXmlWritter = null;
                if (StrFunc.IsFilled(_dateFormat))
                    dsXmlWritter = new DataSetXmlTextWritter(new StringWriter(sb), _dateFormat);
                else
                    dsXmlWritter = new DataSetXmlTextWritter(new StringWriter(sb));
                dsXmlWritter.Formatting = Formatting.Indented;
                _dataSet.WriteXml(dsXmlWritter);
                return sb.ToString();
            }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
        }
        #endregion
    }
    #endregion class DatasetTools

    #region internal class DataSetXmlTextWritter
    /// <summary>
    /// 
    /// </summary>
    internal class DataSetXmlTextWritter : XmlTextWriter
    {
        #region Members
        private bool onWatchDateElement;
        private readonly string dateFormat;
        #endregion
        //
        #region constructor
        public DataSetXmlTextWritter(TextWriter writer)
            : base(writer)
        {
            dateFormat = DtFunc.FmtISODateTime;
        }
        public DataSetXmlTextWritter(TextWriter writer, string pDateFormat)
            : base(writer)
        {
            dateFormat = pDateFormat;
        }
        #endregion constructor
        //
        #region Methodes
        #region public override WriteStartElement
        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            base.WriteStartElement(prefix, localName, ns);
            if (localName.StartsWith("DT"))
                onWatchDateElement = true;
        }
        #endregion
        #region public override WriteString
        public override void WriteString(string text)
        {
            if (onWatchDateElement)
            {
                try
                {
                    DateTime dt = DateTime.Parse(text);
                    text = DtFunc.DateTimeToString(dt, dateFormat);
                }
                catch (FormatException)
                {
                    ;
                }
                onWatchDateElement = false;
            }
            //
            base.WriteString(text);
        }
        #endregion
        #endregion
    }
    #endregion class DataSetXmlTextWritter
}

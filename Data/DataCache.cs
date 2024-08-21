using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Linq;

using EFS.ACommon;


namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// this enum is used in the connection string cache class to return the cached state for a given connection string
    /// </summary>    
    internal enum ConnectionStringCacheState
    {
        // RD 20100111 [16818] Add new DALOleDb
        /// <summary>Connection string is for Oracle</summary>
        isOracle,
        /// <summary>Connection string is for SqlServer</summary>
        isSqlServer,
        /// <summary>Connection string is for OleDb</summary>
        isOleDb,
        /// <summary>Connection string is not cached</summary>
        isNotCached
    }

    

    /// <summary>
    /// DALCache cache provides functions to leverage a static cache of DALs
    /// </summary>
    internal sealed class DALCache
    {
        #region Members
        private static readonly Hashtable dalCache = Hashtable.Synchronized(new Hashtable());
        #endregion

        #region constructor
        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new OraHelperParameterCache()".
        private DALCache() { }
        #endregion

        #region Methodes
        #region CacheDAL
        /// <summary>
        /// add connectionString to the cache - stores connectionString as key with value of isOracle
        /// </summary>
        public static void CacheDAL(DbSvrType serverType, IDataAccess dal)
        {
            string hashKey = serverType.ToString().ToLower();
            dalCache[hashKey] = dal;
        }
        #endregion
        #region GetDAL
        /// <summary>
        /// retrieve a parameter array from the cache
        /// </summary>
        public static IDataAccess GetDAL(DbSvrType serverType)
        {
            string hashKey = serverType.ToString().ToLower();
            //
            if (dalCache.ContainsKey(hashKey))
                return (IDataAccess)dalCache[hashKey];
            else
                return null;
        }
        #endregion
        #endregion Methodes
    }

    /// <summary>
    /// ConnectionString cache provides functions to leverage a static cache of connection strings
    /// </summary>
    internal sealed class ConnectionStringCache
    {
        #region private methods, variables, and constructors
        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new OraHelperParameterCache()".
        private ConnectionStringCache() { }

        private static readonly Hashtable connStringCache = Hashtable.Synchronized(new Hashtable());

        #endregion private methods, variables, and constructors

        #region caching functions
        /// <summary>
        /// add connectionString to the cache - stores connectionString as key with value of isOracle
        /// </summary>
        public static void CacheConnectionString(string connectionString, DbSvrType pDbSvrType)
        {
            ConnectionStringCacheState state;
            switch (pDbSvrType)
            {
                case DbSvrType.dbORA:
                    state = ConnectionStringCacheState.isOracle;
                    break;
                case DbSvrType.dbSQL:
                    state = ConnectionStringCacheState.isSqlServer;
                    break;
                case DbSvrType.dbOLEDB:
                    state = ConnectionStringCacheState.isOleDb;
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            //
            string hashKey = connectionString;
            connStringCache[hashKey] = state;
        }

        /// <summary>
        /// retrieve a parameter array from the cache
        /// </summary>
        public static ConnectionStringCacheState GetConnectionStringState(string connectionString)
        {
            // 20070822 EG Add CleanCS sur ConnectionString
            // RD 20100111 [16818] Add new DALOleDb
            connectionString = new CSManager(connectionString).Cs;
            string hashKey = connectionString;
            
            //PL 20120425 Refactoring suite à pb lors d'un rajout d'un "Connection Timeout"
            //if ((null != connStringCache) && connStringCache.ContainsKey(hashKey))
            //    return (ConnectionStringCacheState)connStringCache[hashKey];
            //else
            //    return ConnectionStringCacheState.isNotCached;
            if (null == connStringCache)
            {
                return ConnectionStringCacheState.isNotCached;
            }
            else
            {
                if (!connStringCache.ContainsKey(hashKey))
                {
                    //DataHelper.CheckConnection(hashKey);
                    CacheConnectionString(hashKey, new CSManager(connectionString).GetDbSvrType());
                }

                return (ConnectionStringCacheState)connStringCache[hashKey];
            }
        }

        #endregion caching functions
    }

}

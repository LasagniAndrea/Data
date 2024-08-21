using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;

namespace EFS.Common
{

    /// <summary>
    /// Contient, en mémoire, des datas enabled 
    /// </summary>
    public abstract class DataEnabledBase
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly object _cacheLock = new object();
        /// <summary>
        /// 
        /// </summary>
        private static readonly Dictionary<DataEnabledCacheKey, IEnumerable> _dataCache
            = new Dictionary<DataEnabledCacheKey, IEnumerable>(new DataEnabledCacheKeyComparer());

        /// <summary>
        /// 
        /// </summary>
        private readonly DateTime _dtReference;

        /// <summary>
        /// connection string sans attribut Spheres®
        /// </summary>
        private readonly string _cs;

        /// <summary>
        /// 
        /// </summary>
        private readonly IDbTransaction _dbTransaction;

        /// <summary>
        /// 
        /// </summary>
        private DataEnabledCacheKey _cacheKey;

        /// <summary>
        /// 
        /// </summary>
        protected virtual string Key
        {
            get
            {
                throw new NotImplementedException("Key property is not implemented");
            }
        }

        /// <summary>
        /// Date de référence
        /// </summary>
        public DateTime DtReference
        {
            get { return _dtReference; }
        }

        /// <summary>
        /// Représente la base de données
        /// </summary>
        protected string CS
        {
            get { return _cs; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected IDbTransaction DbTransaction
        {
            get { return _dbTransaction; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected T GetData<T>() where T : IEnumerable
        {
            return (T)_dataCache[_cacheKey];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        protected void SetData<T>(T data) where T : IEnumerable
        {

            lock (_cacheLock)
            {
                _dataCache.Add(_cacheKey, data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected DataEnabledBase()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        protected DataEnabledBase(string cs, IDbTransaction dbTransaction, DateTime dtReference)
        {
            _cs = new CSManager(cs).Cs;
            _dbTransaction = dbTransaction;
            _dtReference = dtReference;

            SetCacheKey();

            if (false == _dataCache.ContainsKey(_cacheKey))
                LoadData();
        }

        /// <summary>
        /// Suppresssion du cache
        /// </summary>
        public static int ClearCache()
        {
            int ret = 0;
            lock (_cacheLock)
            {
                foreach (DataEnabledCacheKey item in _dataCache.Keys)
                    ret += ArrFunc.Count(_dataCache[item]);

                _dataCache.Clear();
            }
            return ret;
        }

        /// <summary>
        /// Suppresssion du cache en relation avec une base de donnée
        /// </summary>
        /// <param name="cs"></param>
        public static int ClearCache(String cs)
        {
            int ret = 0;
            lock (_cacheLock)
            {
                if (_dataCache.Count > 0)
                {
                    string keyDb = GetDatabaseFromCS(cs);

                    IEnumerable<DataEnabledCacheKey> lst = from item in _dataCache.Keys.Where(x => x.DbName == keyDb)
                                                           select item;

                    foreach (DataEnabledCacheKey item in lst)
                        ret += ArrFunc.Count(_dataCache[item]);

                    if (lst.Count() == _dataCache.Count)
                    {
                        _dataCache.Clear();
                    }
                    else
                    {
                        foreach (DataEnabledCacheKey item in lst.ToArray())
                            _dataCache.Remove(item);
                    }
                }
            }

            return ret;

        }

        /// <summary>
        /// Suppresssion du cache en relation avec la base de donnée <paramref name="cs"/> et la clé <paramref name="key"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="key"></param>
        protected static int ClearCache(String cs, string key)
        {
            int ret = 0;

            lock (_cacheLock)
            {
                if (_dataCache.Count > 0)
                {
                    string keyDb = GetDatabaseFromCS(cs);

                    IEnumerable<DataEnabledCacheKey> lst = from item in _dataCache.Keys.Where(x => x.DbName == keyDb && x.Key == key)
                                                           select item;

                    foreach (DataEnabledCacheKey item in lst)
                        ret += ArrFunc.Count(_dataCache[item]);

                    if (lst.Count() == _dataCache.Count)
                    {
                        _dataCache.Clear();
                    }
                    else
                    {
                        foreach (DataEnabledCacheKey item in lst.ToArray())
                            _dataCache.Remove(item);
                    }
                }
            }

            return ret;

        }

        /// <summary>
        /// 
        /// </summary>

        private void SetCacheKey()
        {
            _cacheKey = new DataEnabledCacheKey()
            {
                CS = _cs,
                DbName = GetDatabaseFromCS(_cs),
                Key = Key,
                DtRefence = _dtReference
            };
        }


        /// <summary>
        /// 
        /// </summary>
        protected virtual void LoadData()
        {
            throw new NotImplementedException("Load method is not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        private static string GetDatabaseFromCS(string pCs)
        {
            CSManager csManager = new CSManager(pCs);
            return GetDatabaseFromCS(csManager);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCsManager"></param>
        /// <returns></returns>
        private static string GetDatabaseFromCS(CSManager pCsManager)
        {
            return @"\\" + pCsManager.GetSvrName() + @"\" + pCsManager.GetDbName();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        protected string GetWhereDtReference(string alias)
        {
            string ret = string.Empty;
            if (DtFunc.IsDateTimeFilled(DtReference))
                ret = $"where {OTCmlHelper.GetSQLDataDtEnabled(CS, alias, DtReference)}";
            return ret;

        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class DataEnabledCacheKey
    {
        /// <summary>
        /// Represente la connection string [sans les attributs spheres] 
        /// </summary>
        public string CS;
        /// <summary>
        /// \\Serveur\database
        /// </summary>
        public string DbName;
        /// <summary>
        /// 
        /// </summary>
        public string Key;
        /// <summary>
        /// 
        /// </summary>
        public DateTime DtRefence;
    }


    /// <summary>
    /// 
    /// </summary>
    internal class DataEnabledCacheKeyComparer : IEqualityComparer<DataEnabledCacheKey>
    {
        /// <summary>
        /// Check the equality of two keys
        /// </summary>
        /// <param name="x">first key to be compared</param>
        /// <param name="y">second key  to be compared</param>
        /// <returns>true when the provided log keys are equal</returns>
        public bool Equals(DataEnabledCacheKey x, DataEnabledCacheKey y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return
                x.DbName.Equals(y.DbName) &&
                x.DtRefence.Equals(y.DtRefence) &&
                x.Key.Equals(y.Key);
        }

        /// <summary>
        /// Get the hashing code of the input  key
        /// </summary>
        /// <param name="obj">input log key we want ot compute the hashing code</param>
        /// <returns></returns>
        public int GetHashCode(DataEnabledCacheKey obj)
        {
            if (obj is null) 
                return 0;

            int hashA = obj.DbName.GetHashCode();
            int hashB = obj.DtRefence.GetHashCode();
            int hashC = obj.Key.GetHashCode();

            return hashA ^ hashB ^ hashC;
        }
    }

}

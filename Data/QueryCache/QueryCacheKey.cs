using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Timers;
using System.Xml;
using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// 
    /// </summary>
    internal class QueryCacheKeyComparer : IEqualityComparer<QueryCacheKey>
    {
        /// <summary>
        /// Check the equality of two keys
        /// </summary>
        /// <param name="x">first key to be compared</param>
        /// <param name="y">second key  to be compared</param>
        /// <returns>true when the provided log keys are equal</returns>
        public bool Equals(QueryCacheKey x, QueryCacheKey y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return
                x.dbname.Equals(y.dbname) &&
                x.query.Equals(y.query) &&
                x.queryResultType.Equals(y.queryResultType);
        }

        /// <summary>
        /// Get the hashing code of the input  key
        /// </summary>
        /// <param name="obj">input log key we want ot compute the hashing code</param>
        /// <returns></returns>
        public int GetHashCode(QueryCacheKey obj)
        {
            if (obj is null) return 0;

            int hashA = obj.dbname.GetHashCode();
            int hashB = obj.query.GetHashCode();
            int hashC = obj.queryResultType.GetHashCode();

            return hashA ^ hashB ^ hashC;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class QueryCacheKey
    {
        /// <summary>
        /// Represente la connection string [sans les attributs spheres] 
        /// </summary>
        public string cs;
        /// <summary>
        /// \\Serveur\database
        /// </summary>
        public string dbname;
        /// <summary>
        /// Requête avec les paramètres suistituées
        /// </summary>
        public string query;
        /// <summary>
        /// 
        /// </summary>
        public QueryCacheResultTypeEnum queryResultType;

    }
}

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
    ///  Représente une entrée du cache
    /// </summary>
    internal class QueryCacheItem
    {
        #region Members
        /// <summary>
        /// Represente la date d'expiration du jeu de résultat associé
        /// </summary>
        public DateTime dtExpiration;
        /// <summary>
        /// Represente le résultat de la query (Object peut être de type DataSet, etc...)
        /// </summary>
        public Object result;
        /// <summary>
        /// Represente le type du résultat de la query (DataSet, etc...)
        /// </summary>
        public QueryCacheResultTypeEnum resultType;
        #endregion Members

        #region constructor
        public QueryCacheItem(DateTime pDtExpiration, object pResult, QueryCacheResultTypeEnum pResultType)
        {
            dtExpiration = pDtExpiration;
            result = pResult;
            resultType = pResultType;
        }
        #endregion
    }


}

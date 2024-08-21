using EFS.ACommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;




namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// type d'objet mis en cache
    /// </summary>
    public enum QueryCacheResultTypeEnum
    {
        datatable,
        dataset,
        @object
    }

    /// <summary>
    /// Classe qui gère la liste des requêtes mises en cache
    /// </summary>
    public class QueryCache
    {
        #region Members
        /// <summary>
        /// Timer afin de déclencher périodiquement la purge des éléments expirés
        /// </summary>
        private readonly Timer _timer;
        /// <summary>
        /// Liste qui contient les requêtes du cache 
        /// </summary>
        private readonly Dictionary<QueryCacheKey, QueryCacheItem> _dic;

        private TimeSpan _timeSpanDataEnabled;
        private TimeSpan _timeSpanDataClean;
        private readonly DatetimeProfiler _dateTimeProfiler;

        private Nullable<int> _maxRowsInResult;
        private Nullable<int> _minRowsInResult;
        private readonly Diagnostic _diag;

        private QueryCacheDelCascading _delcascading;
        
        #endregion

        #region accessor
        /// <summary>
        ///obtient ou définit un intervalle de temps où les données sont valides dans le cache 
        /// </summary>
        public int TimeSpanDataEnabled
        {
            get { return (int)_timeSpanDataEnabled.TotalMinutes; }
            set { _timeSpanDataEnabled = new TimeSpan(0, value, 0); }
        }
        
        /// <summary>
        ///obtient ou définit un intervalle de temps entre chaque nettoyage du cache en minute
        /// </summary>
        public int TimeSpanDataClean
        {
            get { return (int)_timeSpanDataClean.TotalMinutes; }
            set { _timeSpanDataClean = new TimeSpan(0, value, 0); }
        }

        /// <summary>
        /// Obtient ou définit le nb de ligne Maximum que doit contenir un jeu de résultat pour rentrer dans le cache
        /// </summary>
        public Nullable<int> MaxRowsInResult
        {
            get { return _maxRowsInResult; }
            set { _maxRowsInResult = value; }
        }

        /// <summary>
        /// Obtient ou définit le nb de ligne Minimum que doit contenir un jeu de résultat pour rentrer dans le cache
        /// </summary>
        public Nullable<int> MinRowsInResult
        {
            get { return _minRowsInResult; }
            set { _minRowsInResult = value; }
        }

        /// <summary>
        /// Ontient la classe de diagnostic associée
        /// </summary>
        /// 
        public Diagnostic Diag
        {
            get { return _diag; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// <para>Pas de purge des requêtes non valides</para>
        /// <para>Mise en cache des requêtes dont le jeux de résultat n'excède pas 50 lignes</para>
        /// </summary>
        /// <param name="pIntervalDataEnabled"></param>
        public QueryCache(int pIntervalDataEnabled)
            : this(pIntervalDataEnabled, 0, 50)
        {
        }
        /// <summary>
        /// <para>Requête valide 20 minutes</para> 
        /// <para>Purge des requêtes non valides toutes les 30 minutes</para> 
        /// <para>Mise en cache des requêtes dont le jeux de résultat n'excède pas 50 lignes</para> 
        /// </summary>
        /// FI 20120614 passage à 20 minutes par défaut et purge toutes les 30 minutes
        /// C'est le paramétrage mis en place chez HPC
        /// Le fichier AppSettingsSpheres.config contient ces valeurs également
        public QueryCache()
            : this(20, 30, 50)
        {
        }
        /// <summary>
        /// <para>Requête valide pIntervalDataEnabled minutes</para>  
        /// <para>Purge des requêtes non valides toutes les pIntervalDataClean minutes</para> 
        /// <para>Mise en cache des requêtes dont le jeux de résultat retourne pas plus de pMaxRowsInResult lignes</para> 
        /// </summary>
        /// <param name="pIntervalDataEnabled"></param>
        /// <param name="pIntervalDataClean"></param>
        /// <param name="pMaxRowsInResult"></param>
        public QueryCache(int pIntervalDataEnabled, int pIntervalDataClean, int pMaxRowsInResult)
        {
            _dateTimeProfiler = new DatetimeProfiler(DateTime.Now);

            _timeSpanDataEnabled = new TimeSpan(0, pIntervalDataEnabled, 0);
            _timeSpanDataClean = new TimeSpan(0, pIntervalDataClean, 0);
            _maxRowsInResult = pMaxRowsInResult;

            _dic = new Dictionary<QueryCacheKey, QueryCacheItem>(new QueryCacheKeyComparer());

            _timer = new Timer();
            if (pIntervalDataClean > 0)
            {
                _timer.Interval = _timeSpanDataClean.TotalMilliseconds;
                _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                _timer.Start();
            }
            _diag = new Diagnostic(this);

            LoadCascading();
        }
        #endregion

        #region indexor
        /// <summary>
        /// Retourne l'element en cache
        /// <para>si le cache est verouillé cette fonction retourne null</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pQuery"></param>
        /// <param name="pParameter"></param>
        /// <param name="pResultType"></param>
        /// <returns></returns>
        internal QueryCacheItem this[string pCs, string pQuery, IDbDataParameter[] pParameter, QueryCacheResultTypeEnum pResultType]
        {
            get
            {
                QueryCacheItem ret = null;

                QueryCacheKey key = NewQueryCacheKey(pCs, pQuery, pParameter, pResultType);
                if (_dic.ContainsKey(key))
                    ret = _dic[key];

                return ret;
            }
        }
        #endregion indexor

        #region Method
        /// <summary>
        /// Retourne le résulat d'une query (de type DataTable ou DataSet ou oject..)
        /// <para>
        /// Retourne null si la query est inexistante dans le cache
        /// </para>
        /// <para>
        /// Retourne DBNull.Value si la Query existente dans le cache et que son exécution retourne null (cas possible avec ExecuteScalar uniquement)
        /// <remarks>cas possible avec ExecuteScalar</remarks>
        /// 
        /// </para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pQuery"></param>
        /// <param name="pParameter"></param>
        /// <param name="pResultType"></param>
        /// <returns></returns>
        public object GetResult(string pCs, string pQuery, IDbDataParameter[] pParameter, QueryCacheResultTypeEnum pResultType)
        {
            lock (((ICollection)this._dic).SyncRoot)
            {
                object ret = null;
                QueryCacheItem item = this[pCs, pQuery, pParameter, pResultType];
                if (null != item && IsQueryEnabled(item))
                {
                    if (null == item.result)
                        ret = DBNull.Value;
                    else
                        ret = GetCopy(item.result, item.resultType);
                }
                return ret;
            }
        }

        /// <summary>
        /// Alimentation du cache
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pQueryParameters"></param>
        /// <param name="pResult"></param>
        public void Update(CSManager pCsManager, string pQuery, IDbDataParameter[] pParameter, object pResult, QueryCacheResultTypeEnum pResultType)
        {
            lock (((ICollection)this._dic).SyncRoot) 
            {
                QueryCacheItem item = this[pCsManager.Cs, pQuery, pParameter, pResultType];
                if (null == item)
                {
                    Insert(pCsManager, pQuery, pParameter, pResult, pResultType);
                }
                else if (false == IsQueryEnabled(item))
                {
                    // A chaque fois qu'une requête est exécutée, Spheres® repousse la date d'expiration dans le cache
                    item.dtExpiration = GetDateExpiration();
                    item.result = GetCopy(pResult, item.resultType);
                }
            }
        }

        /// <summary>
        /// Retourne true si la query présente dans le cache {item} est active (date expiration supérieure à la date en cours)
        /// </summary>
        private bool IsQueryEnabled(QueryCacheItem item)
        {
            return (item.dtExpiration.CompareTo(_dateTimeProfiler.GetDate()) >= 0);
        }


        /// <summary>
        /// Purge Total du Cache
        /// <para>Retourne le ndr de ligne(s) supprimée(s)</para>
        /// </summary>
        public int Clear()
        {
            int ret = 0;
            lock (((ICollection)this._dic).SyncRoot)  
            {
                ret = _dic.Count;
                _dic.Clear();
            }
            return ret;
        }

        /// <summary>
        /// Purge le cache des requêtes jouées sur une base de données précise
        /// <para>Retourne le ndr de ligne(s) supprimée(s)</para>
        /// </summary>
        /// <param name="pCS">Purge du cache pour une connectionstring spécifique</param>
        /// <returns></returns>
        public int Clear(string pCS)
        {
            int ret = 0;

            lock (((ICollection)this._dic).SyncRoot)  
            {
                if (_dic.Count > 0)
                {
                    string keyDb = GetDatabaseFromCS(pCS);

                    List<QueryCacheKey> lst = (from item in _dic.Keys.Where(x => x.dbname == keyDb)
                                               select item).ToList();

                    foreach (QueryCacheKey item in lst)
                        _dic.Remove(item);
                }
            }
            return ret;
        }



        /// <summary>
        /// Suppression dans le cache des requêtes qui font reference à l'objet pObject (Table ou Vue) 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pCs"></param>
        public void Remove(string pObject, string pCs)
        {
            Remove(new string[] { pObject }, pCs, true);
        }
        /// <summary>
        /// Suppression dans le cache des requêtes qui font reference à l'objet pObject (Table ou Vue) 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pCs"></param>
        /// <param name="pIsWithDependencies">avec suppression des requêtes qui reference pObject</param>
        public void Remove(string pObject, string pCs, bool pIsWithDependencies)
        {
            Remove(new string[] { pObject }, pCs, pIsWithDependencies);
        }
        /// <summary>
        /// Suppression dans le cache des requêtes qui font reference à l'objet pObject (Table ou Vue) 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pCs"></param>
        /// <param name="pIsWithDependencies">avec suppression des requêtes qui reference pObject</param>
        /// <param name="pIsThrowException">génération d'Exception</param>
        public void Remove(string pObject, string pCs, bool pIsWithDependencies, bool pIsThrowException)
        {
            Remove(new string[] { pObject }, pCs, pIsWithDependencies, pIsThrowException);
        }
        /// <summary>
        /// Suppression dans le cache des requêtes qui font reference à pTable
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pCs"></param>
        /// <param name="pIsWithDependencies">avec suppression des requêtes qui reference pObject</param>
        public void Remove(string[] pObject, string pCs, bool pIsWithDependencies)
        {
            Remove(pObject, pCs, pIsWithDependencies, true);
        }
        /// <summary>
        /// Suppression dans le cache des requêtes qui font reference à pTable
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pCs"></param>
        /// <param name="pIsWithDependencies">avec suppression des requêtes qui reference pObject</param>
        /// <param name="pIsThrowException">si true,  throw de l'exception lorsqu'elle se produit</param>
        public void Remove(string[] pObject, string pCs, bool pIsWithDependencies, bool pIsThrowException)
        {
            if (ArrFunc.IsFilled(pObject))
            {
                lock (((ICollection)this._dic).SyncRoot)  
                {
                    try
                    {
                        List<QueryCacheKey> lst = new List<QueryCacheKey>();
                        
                        for (int i = 0; i < ArrFunc.Count(pObject); i++)
                            lst.AddRange(GetKeyItemIsObjectReferenced(pObject[i], pCs, pIsWithDependencies));

                        if (lst.Count > 0)
                        {
                            foreach (QueryCacheKey item in lst)
                                _dic.Remove(item);
                        }
                    }
                    catch (Exception)
                    {
                        if (pIsThrowException)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retourne toutes les requêtes présentes dans le cache qui référencent obj
        /// <para>obj est une table ou une vue </para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pCs"></param>
        /// <param name="pIsWithDependencies">
         /// <returns></returns>
        private List<QueryCacheKey> GetKeyItemIsObjectReferenced(string obj, string pCs, bool pIsWithDependencies)
        {
            List<QueryCacheKey> ret = new List<QueryCacheKey>();

            string keyDb = GetDatabaseFromCS(pCs);

            #region objMaster
            string objMaster = string.Empty;
            if (false == pIsWithDependencies)
            {
                objMaster = obj;
            }
            else
            {
                //si obj.StartsWith(obj, "VW_" // obj est une vue
                if (obj.StartsWith("VW_"))
                {
                    //Recherche du nom de table principal
                    //ex VW_ACTOR_CHILDDESK => ACTOR
                    objMaster = obj.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else
                {
                    objMaster = obj;
                }
            }
            #endregion


            foreach (QueryCacheKey key in _dic.Keys.Where(x => x.dbname == keyDb))
            {
                //Exemple objMaster=ACTOR
                //retourne toutes les requêtes qui référence ACTOR
                if (IsObjectReferenced(key, objMaster))
                {
                    ret.Add(key);
                }
                else if (pIsWithDependencies)
                {
                    //Exemple objMaster=ACTOR
                    //retourne toutes les requêtes référence sur VW_ACTORXXX (VW_ACTOR_TRADER par exemple)
                    if (IsObjectReferenced(key, "VW_" + objMaster))
                        ret.Add(key);
                }
            }

            if (pIsWithDependencies)
            {
                QueryCacheTable table = _delcascading.table.Where(x => x.id == objMaster).FirstOrDefault();
                if (null != table)
                {
                    List<string> objCascading = GetSqlObjCascading(table);
                    List<QueryCacheKey> keyDependencies = GetKeyItemIsObjectReferenced(objCascading.ToArray(), pCs, false);
                    ret.AddRange(keyDependencies);
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne toutes les requêtes présentes dans le cache qui référencent obj
        /// <para>obj est une table ou une vue </para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pCs"></param>
        /// <returns></returns>
        private List<QueryCacheKey> GetKeyItemIsObjectReferenced(string[] obj, string pCs, bool pIsWithDependencies)
        {
            List<QueryCacheKey> ret = new List<QueryCacheKey>();
            if (ArrFunc.IsFilled(obj))
            {
                for (int i = 0; i < ArrFunc.Count(obj); i++)
                {
                    List<QueryCacheKey> key = GetKeyItemIsObjectReferenced(obj[i], pCs, pIsWithDependencies);
                    if (key.Count > 0)
                        ret.AddRange(key);
                }
            }
            return ret;

        }

        /// <summary>
        /// Insert dans le cache de la query et du jeu de résultat associé
        /// </summary>
        /// <param name="pCsManager"></param>
        /// <param name="pQueryParametrs"></param>
        /// <param name="pObject"></param>
        /// EG 20180205 [23769] Use function with lock
        private void Insert(CSManager pCsManager, string pQuery, IDbDataParameter[] pParameter, object pResult, QueryCacheResultTypeEnum pResultType)
        {
            Nullable<int> minRows = pCsManager.CacheMinRows;
            Nullable<int> maxRows = pCsManager.CacheMaxRows;

            if (IntFunc.IsEmpty(minRows))
                minRows = MinRowsInResult;
            else if (-1 == minRows) //-1 = no minRows
                minRows = null;

            if (IntFunc.IsEmpty(maxRows))
                maxRows = MaxRowsInResult;
            else if (-1 == maxRows)//-1 = no maxRows
                maxRows = null;

            //calcul du nbr de ligne lorsque le type de jeu de résultat est un @object [Résultat d'un ExecuteScalar] 
            int resultNbRows;
            if (QueryCacheResultTypeEnum.datatable == pResultType)
                resultNbRows = ArrFunc.Count(((DataTable)pResult).Rows);
            else if (QueryCacheResultTypeEnum.dataset == pResultType)
                resultNbRows = ArrFunc.Count(((DataSet)pResult).Tables[0].Rows);
            else if (QueryCacheResultTypeEnum.@object == pResultType)
                resultNbRows = (pResult == null) ? 0 : 1;
            else
                throw new NotImplementedException(pResultType.ToString() + "is not implemented");
            
            bool isInsertOk = true;
            if (IntFunc.IsFilled(minRows))
                isInsertOk &= (minRows <= resultNbRows);
            if (IntFunc.IsFilled(maxRows))
                isInsertOk &= (maxRows >= resultNbRows);
            
            if (isInsertOk)
            {
                QueryCacheKey key = NewQueryCacheKey(pCsManager.Cs, pQuery, pParameter, pResultType);
                QueryCacheItem item = new QueryCacheItem(GetDateExpiration(pCsManager.CacheInterval), GetCopy(pResult, pResultType), pResultType);
                _dic.Add(key, item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DateTime GetDateExpiration()
        {
            return _dateTimeProfiler.GetDate().Add(_timeSpanDataEnabled);
        }
        private DateTime GetDateExpiration(Nullable<int> pIntervalDataEnabled)
        {
            if (pIntervalDataEnabled.HasValue && (pIntervalDataEnabled > 0))
                return _dateTimeProfiler.GetDate().Add(new TimeSpan(0, 0, (int)pIntervalDataEnabled));
            else
                return GetDateExpiration();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            RemoveExpiredQuery();
        }


        /// <summary>
        /// Retourne une nouvelle instance de QueryCacheKey
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pQuery"></param>
        /// <param name="pParameter"></param>
        /// <param name="pResultType"></param>
        /// <returns></returns>
        private static QueryCacheKey NewQueryCacheKey(string pCs, string pQuery, IDbDataParameter[] pParameter, QueryCacheResultTypeEnum pResultType)
        {
            CSManager csManager = new CSManager(pCs);
            return new QueryCacheKey
            {
                cs = csManager.Cs,
                dbname = GetDatabaseFromCS(csManager),
                query = DataHelper.ReplaceParametersInQuery(pQuery, csManager.GetDbSvrType(), pParameter),
                queryResultType = pResultType
            };
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
        /// Supprime les requêtes expirées du cache 
        /// </summary>
        private void RemoveExpiredQuery()
        {
            lock (((ICollection)this._dic).SyncRoot) 
            {
                List<QueryCacheKey> lst =
                                            (from item in
                                                 _dic.Keys.Where(x => (false == IsQueryEnabled(_dic[x])))
                                             select item).ToList();

                foreach (QueryCacheKey key in lst)
                    _dic.Remove(key);

            }
        }

        /// <summary>
        ///  Retourne une copie de {pResul} 
        /// </summary>
        /// <param name="pResult"></param>
        /// <param name="pResultType"></param>
        /// <returns></returns>
        private static object GetCopy(object pResult, QueryCacheResultTypeEnum pResultType)
        {
            object ret = null;
            if (null != pResult)
            {
                switch (pResultType)
                {
                    case QueryCacheResultTypeEnum.dataset:
                        ret = ((DataSet)pResult).Copy();
                        break;
                    case QueryCacheResultTypeEnum.datatable:
                        ret = ((DataTable)pResult).Copy();
                        break;
                    case QueryCacheResultTypeEnum.@object:
                        if (null != pResult.GetType().GetInterface("ICloneable"))
                            ret = ((ICloneable)pResult).Clone();
                        else
                            ret = pResult;
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pResultType.ToString()));
                }
            }
            return ret;
        }

        /// <summary>
        ///  return true si la requête contenue dans {pley} contient un from ou une jointure sur {pObjectName}    
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pObjectName"></param>
        /// <returns></returns>
        private static bool IsObjectReferenced(QueryCacheKey pKey, string pObjectName)
        {

            bool ret = false;
            string owner = DataHelper.GetObjectsOwner(pKey.cs);

            for (int i = 0; i < 4; i++)
            {
                string sqlKey = string.Empty;
                switch (i)
                {
                    case 0:
                        sqlKey = SQLCst.FROM_DBO;
                        break;
                    case 1:
                        sqlKey = SQLCst.INNERJOIN_DBO;
                        break;
                    case 2:
                        sqlKey = SQLCst.LEFTJOIN_DBO;
                        break;
                    case 3:
                        sqlKey = SQLCst.RIGHTJOIN_DBO;
                        break;
                }
                sqlKey = sqlKey.Replace(SQLCst.DBO, " ");

                // Suppression du blanc devant la constante [pour les requêtes écrites à la main sans les constantes (ex. dans fichiers XML)]
                sqlKey = sqlKey.Substring(1, sqlKey.Length - 1);
                string keyOwner = sqlKey + owner + ".";

                ret = pKey.query.Contains(sqlKey + pObjectName) ||
                      pKey.query.Contains(keyOwner + pObjectName);

                if (ret)
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadCascading()
        {

            string streamName = Assembly.GetExecutingAssembly().GetName().Name + ".QueryCache.QueryCacheDelCascading.xml";
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamName);
            if (null == stream)
                throw new NullReferenceException("QueryCache.QueryCacheCascading.xml Ressource not found");


            using (StreamReader streamReader = new StreamReader(stream))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(QueryCacheDelCascading));
                _delcascading = (QueryCacheDelCascading)xmlSerializer.Deserialize(streamReader);
            }
        }

        private List<string> GetSqlObjCascading(QueryCacheTable pQueryCacheTable)
        {
            List<string> ret = new List<string>();

            if (pQueryCacheTable.objSpecified)
            {
                ret.AddRange((from item in pQueryCacheTable.obj
                              select item.name));

            }

            if (pQueryCacheTable.tableReferenceSpecified)
            {
                foreach (QueryCacheTableReference item in pQueryCacheTable.tableReference)
                {
                    QueryCacheTable tableRef = _delcascading.table.Where(x => x.id == item.href).FirstOrDefault();
                    ret.AddRange(GetSqlObjCascading(tableRef));
                }

            }

            return ret;
        }



        #endregion

        public class Diagnostic
        {
            #region Members
            /// <summary>
            /// 
            /// </summary>
            private readonly QueryCache queryCache;
            #endregion

            #region constructor
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pQueryCache"></param>
            public Diagnostic(QueryCache pQueryCache)
            {
                queryCache = pQueryCache;
            }
            #endregion constructor
            #region public WriteElements
            /// <summary>
            /// Ecriture d'un fichier log  dans le chemin d'accès temporaire de l'utilisateur wwindows® courant
            /// <para>Ce log contient le contenu du cache</para>
            /// <para></para>
            /// </summary>
            public string WriteElements()
            {
                string fileName = Path.GetTempPath() + "CacheElements" + DtFunc.DateTimeToStringISO(queryCache._dateTimeProfiler.GetDate()).Replace(@":", string.Empty) + ".xml";
                WriteElements(fileName);

                return fileName;
            }
            /// <summary>
            /// Ecriture des elements du cache dans le fichier {pFileName}
            /// </summary>
            public void WriteElements(string pFileName)
            {

                XmlDocument xmlDoc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                //Declaration
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                //Comment
                string comment = StrFunc.AppendFormat("Cache Elements, date system: {0}", DtFunc.DateTimeToStringISO(queryCache._dateTimeProfiler.GetDate()));
                xmlDoc.AppendChild(xmlDoc.CreateComment(comment));
                //Root
                XmlElement xmlRoot = xmlDoc.CreateElement("dataCache");
                xmlDoc.AppendChild(xmlRoot);
                //each item
                foreach (QueryCacheKey key in queryCache._dic.Keys)
                {
                    QueryCacheItem queryCacheItem = queryCache._dic[key];
                    //
                    XmlElement elementItem = (XmlElement)xmlRoot.AppendChild(xmlDoc.CreateElement("item"));
                    elementItem.Attributes.Append(xmlDoc.CreateAttribute("dtexpiration"));
                    elementItem.Attributes["dtexpiration"].Value = DtFunc.DateTimeToStringISO(queryCacheItem.dtExpiration);
                    //<dbname>
                    XmlElement elementDbName = (XmlElement)elementItem.AppendChild(xmlDoc.CreateElement("dbname"));
                    elementDbName.AppendChild(xmlDoc.CreateTextNode(key.dbname));
                    //<query>
                    XmlElement elementQuery = (XmlElement)elementItem.AppendChild(xmlDoc.CreateElement("query"));
                    elementQuery.AppendChild(xmlDoc.CreateCDataSection(key.query));
                    //<Result>
                    XmlElement elementResultType = (XmlElement)elementItem.AppendChild(xmlDoc.CreateElement("resultType"));
                    elementResultType.AppendChild(xmlDoc.CreateTextNode(key.queryResultType.ToString()));

                }

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true
                };
                XmlWriter xmlWritter = XmlTextWriter.Create(pFileName, xmlWriterSettings);

                xmlDoc.Save(xmlWritter);
            }
            #endregion
        }

    }
}

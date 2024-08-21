using System;
using System.Collections.Generic;
using System.Text;

using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{
    #region public class CSExtendAttributes
    /// <summary>
    /// Contient les attributs spécifiques définis dans la ConnectionString
    ///<para>
    /// Exemple SpheresCache ou SpheresHostname ou....
    ///</para>
    /// </summary>
    public class CSExtendAttributes
    {
        #region Members
        #region idA
        /// <summary>
        /// indicateur Actor [utilisé pour la trace SQL]
        /// </summary>
        public Nullable<int> idA;
        #endregion
        #region hostName
        /// <summary>
        /// indicateur Hostname [utilisé pour la trace SQL]
        /// </summary>
        public String hostName;
        #endregion
        #region isUseCache
        /// <summary>
        /// indicateur afin d'activer/desactiver le cache sur les requêtes exécutées 
        /// </summary>
        public Nullable<bool> isUseCache;
        #endregion
        #region cacheMinRows
        /// <summary>
        /// indicateur afin d'intégrer la requête dans le cache si le nbr de ligne de la requête dépasse MinRows
        /// </summary>
        public Nullable<int> cacheMinRows;
        #endregion
        #region cacheMaxRows
        /// <summary>
        /// indicateur afin d'intégrer la requête dans le cache si la requête ne dépasse pas MaxRows 
        /// </summary>
        public Nullable<int> cacheMaxRows;
        #endregion
        #region cacheInterval
        /// <summary>
        /// indicateur de durée de vie de la requête dans le cache 
        /// </summary>
        public Nullable<int> cacheInterval;
        #endregion

        /// <summary>
        /// Get/Set the RDBMS case sensitivity
        /// </summary>
        /// <value>CS: all sensitive/CI: case insensitive /AI (Oracle only) : case and  accents insensitive</value>
        public string Collation { get; set; }

        #endregion

        #region constructor
        public CSExtendAttributes()
        {
            idA = null;
            isUseCache = null;
        }
        #endregion
    }
    #endregion

    #region public class CSManager
    /// <summary>
    /// Gestionnaire de CS
    /// <Remark> 
    /// <para>CS Sqlserver "Data Source=myServerAddress;Initial Catalog=myDataBase;User Id=myUsername;Password=myPassword;" </para> 
    /// <para>CS Oracle    "Data Source=TORCL;User Id=myUsername;Password=myPassword" </para> 
    /// </Remark> 
    /// </summary>
    public class CSManager
    {
        #region Members
        /// <summary>
        /// Représente la connexionString sans attributs Spheres®
        /// </summary>
        private string _cs;
        /// <summary>
        /// Représente la connexionString avec les éventuelles attributs Spheres®
        /// </summary>
        private string _csSpheres;
        private Nullable<int> _timeOut;
        private readonly CSExtendAttributes _csExtendAttributes;
        #endregion
        //
        #region accessor

        /// <summary>
        /// Obtient la ConnectionString  utilisé pour l'exécution des requêtes sur le SGBD  (sans attribut Spheres)
        /// </summary>
        public string Cs
        {
            get { return _cs; }
        }

        /// <summary>
        /// Obtient la ConnectionString enrichie des attributs specifiques a Spheres
        /// Exemple {SpheresCache = true;hostName = xxxx;}
        /// </summary>
        public string CsSpheres
        {
            get { return _csSpheres; }
        }

        /// <summary>
        /// Obtient la durée d'attente d'une query avant
        /// que la tentative ne soit abandonnée et qu'une erreur ne soit générée.(en secondes)
        /// </summary>
        public Nullable<int> TimeOut
        {
            get { return _timeOut; }
            set
            {
                _timeOut = value;
                SetSpheresCs();
            }
        }


        #region CSExtendAttributes
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> IdA
        {
            get { return _csExtendAttributes.idA; }
            set
            {
                _csExtendAttributes.idA = value;
                SetSpheresCs();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string HostName
        {
            get { return _csExtendAttributes.hostName; }
            set
            {
                _csExtendAttributes.hostName = value;
                SetSpheresCs();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<bool> IsUseCache
        {
            get { return _csExtendAttributes.isUseCache; }
            set
            {
                _csExtendAttributes.isUseCache = value;
                SetSpheresCs();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> CacheMinRows
        {
            get { return _csExtendAttributes.cacheMinRows; }
            set
            {
                _csExtendAttributes.cacheMinRows = value;
                SetSpheresCs();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> CacheMaxRows
        {
            get { return _csExtendAttributes.cacheMaxRows; }
            set
            {
                _csExtendAttributes.cacheMaxRows = value;
                SetSpheresCs();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> CacheInterval
        {
            get { return _csExtendAttributes.cacheInterval; }
            set
            {
                _csExtendAttributes.cacheInterval = value;
                SetSpheresCs();
            }
        }

        /// <summary>
        /// Get the current collation value from the extended attributes collection.
        /// Set the current collation attribute and rebuild the connection string. 
        /// </summary>
        public string Collation
        {
            get { return _csExtendAttributes.Collation; }

            set
            {
                _csExtendAttributes.Collation = value;

                SetSpheresCs();
            }
        }

        #endregion

        /// <summary>
        /// Obtient true si des attributs specifiques a Spheres sont spécifiés
        /// </summary>
        public bool ContainsSpheresAttributes
        {
            get
            {
                bool ret = false;
                if (StrFunc.IsFilled(_csSpheres))
                    ret = (_csSpheres.StartsWith("{") && _csSpheres.IndexOf("}") > 0);
                return ret;
            }
        }


        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs">connectionString SGBD classique ou une connectionString avec des attributs Spheres </param>
        public CSManager(string pCs)
        {
            _csSpheres = pCs;
            _cs = GetCleanSpheresInfo(pCs);

            _csExtendAttributes = new CSExtendAttributes();

            if (ContainsSpheresAttributes)
            {
                if (StrFunc.IsFilled(GetSpheresAttributes("SpheresUser IDA")))
                    _csExtendAttributes.idA = IntFunc.IntValue2(GetSpheresAttributes("SpheresUser IDA"));
                if (StrFunc.IsFilled(GetSpheresAttributes("SpheresHostname")))
                    _csExtendAttributes.hostName = GetSpheresAttributes("SpheresHostname");
                if (StrFunc.IsFilled(GetSpheresAttributes("SpheresCache")))
                    _csExtendAttributes.isUseCache = BoolFunc.IsTrue(GetSpheresAttributes("SpheresCache"));
                if (StrFunc.IsFilled(GetSpheresAttributes("SpheresCacheMinRows")))
                    _csExtendAttributes.cacheMinRows = IntFunc.IntValue((GetSpheresAttributes("SpheresCacheMinRows")));
                if (StrFunc.IsFilled(GetSpheresAttributes("SpheresCacheMaxRows")))
                    _csExtendAttributes.cacheMaxRows = IntFunc.IntValue((GetSpheresAttributes("SpheresCacheMaxRows")));
                if (StrFunc.IsFilled(GetSpheresAttributes("SpheresCacheInterval")))
                    _csExtendAttributes.cacheInterval = IntFunc.IntValue((GetSpheresAttributes("SpheresCacheInterval")));
            }
        }
        #endregion

        #region Method
        public string GetSpheresInfoCs()
        {
            return GetSpheresInfoCs(false);
        }
        /// <summary>
        /// Retourne la valeur literale des attributs Spheres positionnés dans la connectionString  
        /// Exemple SpheresCache = true;hostName = xxxx;
        /// </summary>
        public string GetSpheresInfoCs(bool pIsWithDelimiter)
        {
            string ret = string.Empty;
            if (ContainsSpheresAttributes)
            {
                ret = _csSpheres.Substring(1, _csSpheres.IndexOf("}") - 1);
                if (pIsWithDelimiter)
                    ret = "{" + ret + "}";
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public string GetAttribute(string pKey)
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(_csSpheres))
            {
                if (_csSpheres.IndexOf(pKey + "=") >= 0)
                {
                    ret = _csSpheres.Substring(_csSpheres.IndexOf(pKey + "=") + (pKey + "=").Length);
                    ret = ret.Substring(0, ret.IndexOf(";"));
                }
            }
            return ret;
        }


        /// <summary>
        /// Returns the Server Name from the connection string
        /// </summary>
        /// <param name="isShortName">Indicates if the returned value is in short format or extended format (default is true: short format)</param>
        /// <returns>
        /// When Oracle EZConnect connection string is used and isShortName is false, returns a string composed by {SERVICE_NAME}/{HOST}/{PORT}.
        /// When Oracle EZConnect connection string is used and isShortName is true (default), returns {SERVICE_NAME}.
        /// In all other cases, returns the Data Source value.
        /// </returns>
        // AL 20240705 [WI996] Manage the Oracle EZConnect CS and returns full server info if needed
        public string GetSvrName(bool isShortName = true)
        {
            string ret = null;
            int len, pos, pos2;

            if (StrFunc.IsFilled(Cs))
            {
                if (IsOracle() && Cs.IndexOf("SERVICE_NAME=") >= 0) //Oracle EZConnect
                {
                    if (!isShortName)
                    {
                        string[] props = new string[] { "SERVICE_NAME=", "HOST=", "PORT=" };
                        List<string> values = new List<string>();
                        foreach (string p in props)
                        {
                            len = p.Length;
                            pos = Cs.IndexOf(p);
                            pos2 = Cs.IndexOf(")", pos);
                            values.Add(Cs.Substring(pos + len, pos2 - pos - len));
                        }
                        ret = String.Join("/", values);
                    }
                    else // Retrieves only SERVICE_NAME (default)
                    {
                        // AL 20240706 [WI993] Oracle EZConnect
                        len = "SERVICE_NAME=".Length;
                        pos = Cs.IndexOf("SERVICE_NAME=");
                        pos2 = Cs.IndexOf(")", pos);
                        ret = Cs.Substring(pos + len, pos2 - pos - len);
                        if (ret.IndexOf(".") >= 0)
                        {
                            ret = ret.Substring(ret.IndexOf(".") + 1);
                        }
                    }
                }
                else if (Cs.IndexOf("Data Source=") >= 0) //SQL Server or Oracle tnsname or OleDb
                {
                    len = "Data Source=".Length;
                    pos = Cs.IndexOf("Data Source=");
                    pos2 = Cs.IndexOf(";", pos);
                    ret = Cs.Substring(pos + len, pos2 - pos - len);
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne la base de donnée associée
        /// <para>Sur Oracle: retourne le schema User ID</para>
        /// <para>Sur SqlServer: Initial Catalog</para>
        /// </summary>
        /// <returns></returns>
        public string GetDbName()
        {
            string ret = null;
            //
            if (StrFunc.IsFilled(Cs))
            {
                int pos, pos2, len;
                if (IsSQLServer())  // AL 20240705 [WI996] Use of IsSQLServer
                {
                    len = "Initial Catalog=".Length;
                    pos = Cs.IndexOf("Initial Catalog=");
                    pos2 = Cs.IndexOf(";", pos);
                    ret = Cs.Substring(pos + len, pos2 - pos - len);
                }
                else if (IsOracle()) // AL 20240705 [WI996] Use of IsOracle
                {
                    len = "User ID=".Length;
                    pos = Cs.IndexOf("User ID=");
                    pos2 = Cs.IndexOf(";", pos);
                    ret = Cs.Substring(pos + len, pos2 - pos - len);
                }
                else // RD 20100111 [16818] Add new DALOleDb
                    ret = GetSvrName();
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException si connection string est non renseigné"></exception>
        /// <exception cref="NotImplementedException si connection string est non reconnue"></exception>
        /// <returns></returns>
        /// FI 20140123 [XXXXX] add message connection string is empty
        public DbSvrType GetDbSvrType()
        {
            // RD 20100111 [16818] Add new DALOleDb
            DbSvrType ret = DbSvrType.dbUNKNOWN;
            //
            if (StrFunc.IsEmpty(Cs))
                throw new InvalidOperationException("DbSvrType is unknow, connection string is empty");

            if (Cs.IndexOf("Initial Catalog=") >= 0)
                ret = DbSvrType.dbSQL;
            else if (Cs.IndexOf("User ID=") >= 0)
                ret = DbSvrType.dbORA;
            else if (Cs.IndexOf("Provider=") >= 0)
                ret = DbSvrType.dbOLEDB;

            if (DbSvrType.dbUNKNOWN == ret)
                throw new NotImplementedException("DbSvrType is unknow, connection string is not valid");

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsSQLServer()
        {
            return this.GetDbSvrType() == DbSvrType.dbSQL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsOracle()
        {
            return this.GetDbSvrType() == DbSvrType.dbORA;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsOleDb()
        {
            return this.GetDbSvrType() == DbSvrType.dbOLEDB;
        }

        /// <summary>
        ///  ConnectionString dans sa plus simple forme et sans mot de passe de connexion 
        ///  <para>MSSQL => Serveur, base de donnée, user</para>
        ///  <para>ORA => Serveur et schema</para>
        /// </summary>
        /// <returns></returns>
        public string GetCSWithoutPwd()
        {
            // RD 20100111 [16818] Add new DALOleDb
            StrBuilder ret = new StrBuilder();
            string[] infos = _cs.Split(';');

            if (IsOleDb())
            {
                ret += _cs.Replace(";", " ");
            }
            else
            {
                foreach (string info in infos)
                {
                    if (info.StartsWith("Data"))
                        ret += info + " ";
                    if (info.StartsWith("Initial")) //Special SqlServer
                        ret += info + " ";
                    if (info.StartsWith("User"))
                        ret += info;
                }
            }

            return ret.ToString();
        }

        /// <summary>
        ///  ConnectionString avec mot de passe anonymisé
        /// </summary>
        /// <returns></returns>
        public string GetCSAnonymizePwd()
        {
            StrBuilder ret = new StrBuilder();
            string[] infos = _cs.Split(';');

            if (IsOleDb())
            {
                ret += GetCSWithoutPwd();
            }
            else
            {
                foreach (string info in infos)
                {
                    if (info.Trim().ToUpper().StartsWith("PASSWORD"))
                        ret += "Password=######## ";
                    else if (info.ToUpper().IndexOf("PASSWORD") == -1)
                        ret += info + " ";
                }
            }

            return ret.ToString();
        }
        /// <summary>
        /// Supprime les attributs spheres existant
        /// </summary>
        /// <returns></returns>
        private string GetCleanSpheresInfo(string pCs)
        {
            string ret = pCs;
            if (ContainsSpheresAttributes)
                ret = ret.Remove(0, ret.IndexOf("}") + 1);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        private string GetSpheresAttributes(string pKey)
        {
            string ret = string.Empty;
            if (ContainsSpheresAttributes)
                ret = GetAttribute(pKey);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetSpheresCs()
        {
            _csSpheres = string.Empty;
            //
            if (IntFunc.IsFilled(TimeOut))
            {
                if (this.IsSQLServer())
                {
                    const string SQLSERVER_CS_TIMEOUT = "Connection Timeout=";
                    int start = _cs.IndexOf(SQLSERVER_CS_TIMEOUT);
                    if (start >= 0)
                    {
                        //Modification du Timeout existant
                        string cs_temp = _cs.Substring(0, start) + SQLSERVER_CS_TIMEOUT + TimeOut.ToString();
                        int semicolon = _cs.IndexOf(";", start);
                        if (semicolon > 0)
                            cs_temp += _cs.Substring(semicolon);

                        _cs = cs_temp;
                    }
                    else
                    {
                        //Ajout du Timeout
                        if (!_cs.EndsWith(";"))
                            _cs += ";";
                        _cs += SQLSERVER_CS_TIMEOUT + TimeOut.ToString();
                    }
                }
            }
            //
            if ((IntFunc.IsFilled(IdA) || StrFunc.IsFilled(HostName) || BoolFunc.IsFilled(IsUseCache) || !String.IsNullOrEmpty(Collation)))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                if (IntFunc.IsFilledAndNoZero(IdA))
                    sb.Append("SpheresUser IDA=" + IdA.ToString() + ";");
                if (StrFunc.IsFilled(HostName))
                    sb.Append("SpheresHostname=" + HostName + ";");
                if (BoolFunc.IsFilled(IsUseCache))
                    sb.Append("SpheresCache=" + IsUseCache.ToString().ToLower() + ";");
                if (IntFunc.IsFilled(CacheMinRows))
                    sb.Append("SpheresCacheMinRows=" + CacheMinRows.ToString() + ";");
                if (IntFunc.IsFilled(CacheMaxRows))
                    sb.Append("SpheresCacheMaxRows=" + CacheMaxRows.ToString() + ";");
                if (IntFunc.IsFilled(CacheInterval))
                    sb.Append("SpheresCacheInterval=" + CacheInterval.ToString() + ";");
                if (!String.IsNullOrEmpty(Collation))
                    sb.Append("Spheres_Collation=" + Collation + ";");
                sb.Append("}");
                //
                _csSpheres = sb.ToString();
            }
            //PL 20110704
            _csSpheres += _cs;
        }

        /// <summary>
        /// Retourne true si le user dispose uniquement d'accès ReadOnly (select) sur les tables du MPD de Spheres®
        /// </summary>
        /// <returns></returns>
        public bool IsUserReadOnly()
        {
            bool ret = false;
            string dbName = GetDbName();
            if (StrFunc.IsEmpty(dbName))
                throw new NotSupportedException("DbName is null");

            //Aujourd'hui si le nom de la DB se termine "_RO" ou "_READONLY", on considère alors que le User de la base de données est de type ReadOnly 
            if (dbName.EndsWith("_READONLY"))
                ret = true;
            else if (dbName.EndsWith("_RO"))
                ret = true;

            return ret;
        }

        #endregion
    }
    #endregion

    #region public class CSTools
    public class CSTools
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static string SetCacheOn(string pCS)
        {
            return SetCacheOn(pCS, null, null);
        }
        public static string SetCacheOn(string pCS, Nullable<int> pInterval)
        {
            return SetCacheOn(pCS, pInterval, null, null);
        }

        /// <summary>
        /// Ajoute dans la connectionString l'attribut Spheres associés à la mise en cache
        ///<remarks>l'attributs SpheresCache n'est valorisé que s'il est absent</remarks>
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pMinRows">nbr de ligne min pour rentrer dans le cache
        /// <para>-1 => aucune limitation</para>
        /// </param>
        /// <param name="pMaxRows">nbr de ligne max pour rentrer dans le cache
        /// <para>-1 => aucune limitation</para>
        /// </param>
        public static string SetCacheOn(string pCS, Nullable<int> pMinRows, Nullable<int> pMaxRows)
        {
            return SetCacheOn(pCS, null, pMinRows, pMaxRows);
        }
        public static string SetCacheOn(string pCS, Nullable<int> pInterval, Nullable<int> pMinRows, Nullable<int> pMaxRows)
        {
            CSManager csManager = new CSManager(pCS);
            if (null == csManager.IsUseCache)
                csManager.IsUseCache = true;

            if (IntFunc.IsFilled(pInterval))
                csManager.CacheInterval = pInterval;

            if (IntFunc.IsFilled(pMinRows))
                csManager.CacheMinRows = pMinRows;

            if (IntFunc.IsFilled(pMaxRows))
                csManager.CacheMaxRows = pMaxRows;

            return csManager.CsSpheres;
        }

        /// <summary>
        /// Ajoute dans la connectionString l'attribut SpheresCache=no
        /// La Query exécutée avec la connectionString ainsi constituée ne sera pas placée en Cache
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        public static string SetCacheOff(string pCS)
        {
            CSManager csManager = new CSManager(pCS)
            {
                IsUseCache = false
            };
            return csManager.CsSpheres;
        }

        /// <summary>
        /// Ajoute dans la connectionString un TimeOut
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTimeOut"></param>
        /// <returns></returns>
        public static string SetMaxTimeOut(string pCS, int pTimeOut)
        {
            int _currentTimeOut = GetTimout(pCS);
            return CSTools.SetTimeOut(pCS, Math.Max(_currentTimeOut, pTimeOut));
        }

        /// <summary>
        /// Ajoute dans la connectionString un TimeOut
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTimeOut"></param>
        /// <returns></returns>
        public static string SetTimeOut(string pCS, int pTimeOut)
        {
            CSManager csManager = new CSManager(pCS)
            {
                TimeOut = pTimeOut
            };
            return csManager.CsSpheres;
        }

        // EG 20120608 New
        public static int GetTimout(string pCS)
        {
            int timeout = -1;
            const string SQLSERVER_CS_TIMEOUT = "Connection Timeout=";
            int start = pCS.IndexOf(SQLSERVER_CS_TIMEOUT);
            if (start >= 0)
            {
                string temp = pCS.Substring(start).Replace(SQLSERVER_CS_TIMEOUT, string.Empty);
                int semicolon = temp.IndexOf(";");
                if (semicolon > 0)
                    temp = temp.Substring(0, semicolon);

                timeout = IntFunc.IntValue(temp);
            }
            return timeout;
        }
        /// <summary>
        /// Add the collation parameter to the connection string
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCollation"></param>
        /// <returns></returns>
        public static string SetCollation(string pCS, string pCollation)
        {
            CSManager csManager = new CSManager(pCS)
            {
                Collation = pCollation
            };
            return csManager.CsSpheres;
        }

        /// <summary>
        /// Retourne true si le user dispose uniquement d'accès ReadOnly (select) sur les tables du MPD de Spheres® 
        /// <para>NB: on évoque par user un utilisateur de la base de données.</para>
        /// </summary>
        /// <param name="pCS"></param>
        public static bool IsUserReadOnly(string pCS)
        {
            CSManager csManager = new CSManager(pCS);
            return csManager.IsUserReadOnly();
        }
    }
    #endregion

    /// <summary>
    /// Informations sur la connexion
    /// </summary>
    // PM 20200102 [XXXXX] New
    public class RDBMSConnectionInfo
    {
        #region Members
        private readonly string m_ConnectionString;
        private readonly CSManager m_CSManager;
        private readonly DbSvrType m_SGBDType;
        private readonly string m_ServerName;
        private readonly string m_DataBaseName;
        private readonly string m_ShortCSWithoutPwd;
        #endregion Members

        #region Accessors
        /// <summary>
        /// ConnectionString d'origine
        /// </summary>
        public string ConnectionString
        {
            get { return m_CSManager.Cs; }
        }

        /// <summary>
        /// Type de base de données
        /// </summary>
        public DbSvrType SGBDType
        {
            get { return m_SGBDType; }
        }

        /// <summary>
        /// Nom du serveur de base de données
        /// </summary>
        public string ServerName
        {
            get { return m_ServerName; }
        }

        /// <summary>
        /// Nom de la base de données
        /// </summary>
        public string DataBaseName
        {
            get { return m_DataBaseName; }
        }

        /// <summary>
        /// ConnectionString courte et sans PassWord
        /// </summary>
        public string ShortCSWithoutPwd
        {
            get { return m_ShortCSWithoutPwd; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        public RDBMSConnectionInfo(string pConnectionString)
        {
            m_ConnectionString = pConnectionString;
            m_CSManager = new CSManager(m_ConnectionString);
            m_SGBDType = m_CSManager.GetDbSvrType();
            m_ServerName = m_CSManager.GetSvrName();
            m_DataBaseName = m_CSManager.GetDbName();
            m_ShortCSWithoutPwd = m_CSManager.GetCSWithoutPwd();
        }
        #endregion Constructors
    }
}

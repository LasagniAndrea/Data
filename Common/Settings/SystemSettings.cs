using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Configuration;
using System.Threading;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;


namespace EFS.Common
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class SystemSettings
    {
        private const string SUFFIX_LOG = "Log";
        //
        public const string Spheres_Collation_CS = "CS"; //Case sensitive
        public const string Spheres_Collation_CI = "CI"; //Case insensitive
        /// <summary>
        /// Hachage de la donnée 
        /// Lecture de l'algorithme de hachage par défaut (si manquant SHA256)
        /// Attention dans le fichier config l'on peut trouver SHA256;MD5;YYYY-MM-DD (voir ticket 25660)
        /// Hachage de la donnée
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        // EG 20210209 [25660] New
        public static string HashData(string pData)
        {
            Tuple<string, string, DateTime> ret = SystemSettings.GetAppSettings_HashAlgorithm;
            Cst.HashAlgorithm defaultAlgorithm = Cst.HashAlgorithm.SHA256;
            if (StrFunc.IsFilled(ret.Item1) && Enum.IsDefined(typeof(Cst.HashAlgorithm), ret.Item1))
                defaultAlgorithm = (Cst.HashAlgorithm)Enum.Parse(typeof(Cst.HashAlgorithm), ret.Item1, true);
            return StrFunc.HashData(pData, defaultAlgorithm);
        }
        /// <summary>
        /// Lecture de l'algorithme de hachage par défaut (si manquant SHA256)
        /// Attention dans le fichier config l'on peut trouver SHA256;MD5;YYYY-MM-DD (voir ticket 25660)
        /// retourne un Tuple(item1, item2, item3) où :
        /// item1 : la nouvelle valeur de hachage
        /// item2 : l'ancienne valeur de hachage
        /// item3 : la date de fin de validié de l'ancienne valeur de hachage
        /// </summary>
        // EG 20210209 [25660] New
        public static Tuple<string, string, DateTime> GetAppSettings_HashAlgorithm
        {
            get {
                string hash = GetAppSettings_Software("_Hash");
                string[] hashResult = hash.Split(";".ToCharArray());

                Tuple<string, string, DateTime> ret;
                if (StrFunc.IsFilled(hash))
                {
                    if (hashResult.Length == 3)
                        ret = new Tuple<string, string, DateTime>(hashResult[0], hashResult[1], new DtFunc().StringDateISOToDateTime(hashResult[2]));
                    else
                        ret = new Tuple<string, string, DateTime>(hashResult[0], null, DateTime.MinValue);
                }
                else
                {
                    ret = new Tuple<string, string, DateTime>(Cst.HashAlgorithm.SHA256.ToString(), null, DateTime.MinValue);
                }
                return ret;
            }
        }
        public static string GetAppSettings_Software(string pKey)
        {
            string prefix = Software.Name;

            string ret = (string)GetAppSettings(prefix + pKey, typeof(string), null);
            if (StrFunc.IsEmpty(ret))
            {
                if (pKey.StartsWith("_"))
                    pKey = pKey.Remove(0, 1);
                ret = (string)GetAppSettings(pKey, typeof(System.String), null);
            }
            return ret;
        }
        public static string GetAppSettings_Software(string pKey, string pSuffix)
        {
            string ret = GetAppSettings_Software(pKey + pSuffix);
            //PL 20140512 Debug
            //if (StrFunc.IsEmpty(ret) && StrFunc.IsEmpty(pSuffix))
            if (StrFunc.IsEmpty(ret) && StrFunc.IsFilled(pSuffix))
            {
                //Search without suffix
                ret = GetAppSettings_Software(pKey);
            }

            return ret;
        }
        /// <summary>
        /// Retourne la valeur associée à la clé {pKey}
        /// <para>Retourne null lorsque la clé n'existe pas</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public static string GetAppSettings(string pKey)
        {
            return (string)GetAppSettings(pKey, typeof(System.String), null);
        }
        /// <summary>
        /// Retourne la valeur string associée à la clé {pKey}
        /// <para>Retourne la valeur par défaut lorsque la clé n'existe pas</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pDefaultValue">Valeur par défaut</param>
        /// <returns></returns>
        public static string GetAppSettings(string pKey, string pDefaultValue)
        {
            return (string)GetAppSettings(pKey, typeof(System.String), pDefaultValue);
        }
        /// <summary>
        /// Retourne la valeur associée à la clé {pKey}
        /// <para>Retourne la valeur par défaut lorsque la clé n'existe pas</para>
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pReturnType"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static object GetAppSettings(string pKey, Type pReturnType, object pDefaultValue)
        {
            string appSetting = ConfigurationManager.AppSettings[pKey];
            if (StrFunc.IsEmpty(appSetting) && (null != pDefaultValue))
            {
                return pDefaultValue;
            }
            else
            {
                if (pReturnType.Equals(typeof(System.Int32)))
                    return Convert.ToInt32(appSetting);
                else if (pReturnType.Equals(typeof(System.Boolean)))
                    return Convert.ToBoolean(appSetting);
                else
                    return appSetting;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pIsAppHelp"></param>
        /// <returns></returns>
        public static string GetUrlForHelp(string pUrl, bool pIsAppHelp)
        {
            string ret;
            if (pIsAppHelp)
            {
                ret = GetAppSettings_Software("HelpUrl") + pUrl;
            }
            else
            {
                //EFSmLHelpSchemasUrl ou FpMLHelpUrl
                bool isEFSmLHelp = (bool)GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
                ret = GetAppSettings(isEFSmLHelp ? "EFSmLHelpSchemasUrl" : "FpMLHelpUrl") + pUrl;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUrl"></param>
        /// <returns></returns>
        public static string GetWindowOpenForHelpApp(string pUrl)
        {
            if (StrFunc.IsEmpty(pUrl))
                pUrl = "/ApplicationHelp.htm";
            //
            string url = GetUrlForHelp(pUrl, true);
            if (StrFunc.IsFilled(url))
                return @"window.open('" + url + @"','Help');";
            else
                return "alert('On line help not installed...')";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetWindowOpenForHelpSchemas()
        {
            string url = GetUrlForHelp("/default.html", false);
            if (StrFunc.IsFilled(url))
            {
                return @"window.open('" + url + @"','HelpSchemas');";
            }
            else
                return "alert('On line help schemas not installed...')";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsHidePassword">Utilisé pour afficher la CS</param>
        /// <returns></returns>
        public static string GetLogConnectionString(bool pIsHidePassword, string pApplicationPath)
        {
            string rdbmsName, serverName, databaseName, userName, pwd;
            rdbmsName = serverName = databaseName = userName = pwd = null;
            SystemSettings.GetDefaultConnectionCharacteristics(true, pApplicationPath,
                ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

            //string rdbms = GetAppSettings_Software("_RdbmsName" + SUFFIX_LOG);
            //if (StrFunc.IsEmpty(rdbms))
            //    rdbms = GetAppSettings_Software("_RdbmsName");
            //
            //string server = GetAppSettings_Software("_ServerName" + SUFFIX_LOG);
            //if (StrFunc.IsEmpty(server))
            //    server = GetAppSettings_Software("_ServerName");
            //
            //string database = GetAppSettings_Software("_DatabaseName" + SUFFIX_LOG);
            //if (StrFunc.IsEmpty(database))
            //    database = GetAppSettings_Software("_DatabaseName");
            //
            string defUser = GetDefUser(rdbmsName, true);
            if (StrFunc.IsEmpty(defUser))
                defUser = GetDefUser(rdbmsName, false);
            //20070907 PL Tip pour simplifier et permettre de swapper aisément de SQL à ORACLE
            if (StrFunc.IsFilled(rdbmsName) && rdbmsName.StartsWith("ORACL"))
                defUser = databaseName;



            bool isPwdCrypted = (GetAppSettings_Software("_IsPwdCrypted" + SUFFIX_LOG) == "1");
            string defPwd = "*****";
            if (!pIsHidePassword)
            {
                defPwd = GetDefPwd(rdbmsName, true);
                if (StrFunc.IsEmpty(defPwd))
                {
                    defPwd = GetDefPwd(rdbmsName, false);
                    isPwdCrypted = (GetAppSettings_Software("_IsPwdCrypted") == "1");
                }
                if (isPwdCrypted)
                    defPwd = Cryptography.Decrypt(defPwd);
            }

            return GetMainConnectionString(rdbmsName, serverName, databaseName, defUser, defPwd);
        }

        /// <summary>
        /// Retourne la connectionString 
        /// <para>La connectionString est bâtie selon les templates fournis dans le fichier de config</para>
        /// </summary>
        /// <param name="pRdbms">type de base de donnée</param>
        /// <param name="pServer">Serveur</param>
        /// <param name="pDatabase">Schéma</param>
        /// <param name="pDefUser"></param>
        /// <param name="pDefPwd"></param>
        /// <returns></returns>
        public static string GetMainConnectionString(string pRdbms, string pServer, string pDatabase, string pDefUser, string pDefPwd)
        {
            string ret = GetAppSettings_Software("ConnectionString");
            if (StrFunc.IsEmpty(ret))
            {
                string sourceTemplate = string.Empty;
                if (DataHelper.IsRdmbsOracle(pRdbms))
                    sourceTemplate = GetAppSettings("OracleTemplateConnectionString");
                else if (DataHelper.IsRdmbsSQLServer(pRdbms))
                    sourceTemplate = GetAppSettings("SqlServerTemplateConnectionString");

                ret = ConstructConnectionString(sourceTemplate, pRdbms, pServer, pDatabase, pDefUser, pDefPwd);
            }
            // PL 20180720 Newness
            ret = CompleteConnectionString(ret);

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRdbms"></param>
        /// <param name="pIsLog"></param>
        /// <returns></returns>
        private static string GetDefUser(string pRdbms, bool pIsLog)
        {
            string suffix = (pIsLog ? SUFFIX_LOG : string.Empty);
            string ret = (pIsLog ? string.Empty : "EFS_DBO");
            //********************************************************
            //20081027 PL Tip for Web.Config multi RDBMS
            //********************************************************
            string tmpValue = string.Empty + GetAppSettings_Software("_" + pRdbms + "_UserName" + suffix);
            //********************************************************
            if (StrFunc.IsEmpty(tmpValue))
                tmpValue = string.Empty + GetAppSettings_Software("_UserName" + suffix);
            if (StrFunc.IsFilled(tmpValue))
                ret = tmpValue;
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRdbms"></param>
        /// <param name="pIsLog"></param>
        /// <returns></returns>
        private static string GetDefPwd(string pRdbms, bool pIsLog)
        {
            string suffix = (pIsLog ? SUFFIX_LOG : string.Empty);
            string ret = (pIsLog ? string.Empty : "efs");
            //********************************************************
            //20081027 PL Tip for Web.Config multi RDBMS
            //********************************************************
            string tmpValue = string.Empty + GetAppSettings_Software("_" + pRdbms + "_Pwd" + suffix);
            //********************************************************
            if (StrFunc.IsEmpty(tmpValue))
                tmpValue = string.Empty + GetAppSettings_Software("_Pwd" + suffix);
            if ("{null}" == tmpValue)
                ret = string.Empty;
            else if (StrFunc.IsFilled(tmpValue))
            {
                ret = tmpValue;
                tmpValue = GetAppSettings_Software("_IsPwdCrypted");
                if (tmpValue == "1")
                    ret = Cryptography.Decrypt(ret);
            }
            return ret;
        }

        /// <summary>
        /// Attention cette méthode doit etre exécutée uniquement dans un contexte application web
        /// </summary>
        /// <param name="pApplicationPath"></param>
        /// <param name="opRdbmsName"></param>
        /// <param name="opServerName"></param>
        /// <param name="opDatabaseName"></param>
        /// <param name="opUserName"></param>
        /// <param name="opPwd"></param>
        /// EG 20220715 Suppression du ReturnURL (en Mode RdbmsName = URL)
        public static string GetDefaultConnectionCharacteristics(bool pIsLog, string pApplicationPath,
                                ref string opRdbmsName, ref string opServerName, ref string opDatabaseName,
                                ref string opUserName, ref string opPwd)
        {
            string ret = "Default";

            string suffix = (pIsLog ? SUFFIX_LOG : string.Empty);
            string tmpValue;

            string[] urlReferrer_Queries = null;
            bool isExisturlReferrer_Query = false;
            string urlReferrer_Query = string.Empty;

            if (SystemSettings.GetAppSettings_Software("_RdbmsName") == "URL")
            {
                //RdbmsName="URL": L'URL contient les informations de connexion à la BdD (Usage réservé à EFS)
                if (System.Web.HttpContext.Current.Items.Count > 0) //NB: Utilisation de "HttpContext.Current.Items.Count" pour évite rune erreur losr de l'appel depuis Application_Start() 
                {
                    if ((System.Web.HttpContext.Current.Request.UrlReferrer != null) && 
                        StrFunc.IsFilled(System.Web.HttpContext.Current.Request.UrlReferrer.Query))
                    {
                        //NB: Sur Login.aspx, Banner.aspx, ... l'URL initiale (Default.aspx) est disponible sous "UrlReferrer"
                        urlReferrer_Query = System.Web.HttpContext.Current.Request.UrlReferrer.Query;
                    }
                    else
                    {
                        //NB: Sur Default.aspx.aspx l'URL est disponible sous "Url"
                        urlReferrer_Query = System.Web.HttpContext.Current.Request.Url.Query;
                    }
                }
                isExisturlReferrer_Query = StrFunc.IsFilled(urlReferrer_Query);
                if (isExisturlReferrer_Query)
                {
                    urlReferrer_Query = urlReferrer_Query.Remove(0, 1);//Remove "?"
                    urlReferrer_Query = System.Web.HttpContext.Current.Server.UrlDecode(urlReferrer_Query);
                    // EG 20220715 Suppression ReturnURL jusqu'au caractère "?"
                    if (urlReferrer_Query.Contains("?"))
                        urlReferrer_Query = urlReferrer_Query.Remove(0, urlReferrer_Query.IndexOf("?") + 1);//Remove "?"
                    urlReferrer_Queries = urlReferrer_Query.Split('&');

                    ret = "URL";
                }
            }

            tmpValue = null;
            if (isExisturlReferrer_Query && (urlReferrer_Queries.Length > 0) && (urlReferrer_Queries[0].StartsWith("RdbmsName")))
            {
                tmpValue = (urlReferrer_Queries[0].Split('='))[1];
            }
            if (StrFunc.IsEmpty(tmpValue))
            {
                tmpValue = GetAppSettings_Software("_RdbmsName", suffix);
            }
            if (StrFunc.IsFilled(tmpValue))
            {
                opRdbmsName = tmpValue;
            }

            string cs = GetAppSettings_Software("ConnectionString");
            if (StrFunc.IsFilled(cs))
            {
                #region ConnectionString
                ret = "ConnectionString";

                CSManager csManager = new CSManager(cs);
                if (StrFunc.IsFilled(tmpValue))
                {
                    if ((csManager.GetDbSvrType() == DbSvrType.dbORA) && ((!tmpValue.StartsWith("ORACL")) || (tmpValue.IndexOf("SQLSRV") >= 0)))
                        tmpValue = null; 
                    else if ((csManager.GetDbSvrType() == DbSvrType.dbSQL) && ((!tmpValue.StartsWith("SQLSRV")) || (tmpValue.IndexOf("ORACL") >= 0)))
                        tmpValue = null;
                }
                if (StrFunc.IsEmpty(tmpValue))
                {
                    opRdbmsName = (csManager.GetDbSvrType() == DbSvrType.dbORA ? RdbmsEnum.ORACL12C.ToString() : RdbmsEnum.SQLSRV2K12.ToString());
                }
                opServerName = csManager.GetSvrName();
                opDatabaseName = csManager.GetDbName();
                #endregion
            }
            else
            {
                #region Not ConnectionString
                tmpValue = null;
                if (isExisturlReferrer_Query && (urlReferrer_Queries.Length > 1) && (urlReferrer_Queries[1].StartsWith("ServerName")))
                {
                    tmpValue = (urlReferrer_Queries[1].Split('='))[1];
                }
                if (StrFunc.IsEmpty(tmpValue))
                {
                    tmpValue = GetAppSettings_Software("_ServerName", suffix);
                }
                if (StrFunc.IsFilled(tmpValue))
                {
                    opServerName = tmpValue;
                }

                tmpValue = null;
                if (isExisturlReferrer_Query && (urlReferrer_Queries.Length > 2) && (urlReferrer_Queries[2].StartsWith("DatabaseName")))
                {
                    tmpValue = (urlReferrer_Queries[2].Split('='))[1];
                }
                if (StrFunc.IsEmpty(tmpValue))
                {
                    tmpValue = GetAppSettings_Software("_DatabaseName", suffix);
                }
                if (StrFunc.IsFilled(tmpValue))
                {
                    opDatabaseName = tmpValue;
                    if ("WEBSITE()" == opDatabaseName.ToUpper())
                    {
                        opDatabaseName = pApplicationPath;
                        opDatabaseName = opDatabaseName.Substring(1, opDatabaseName.Length - 1);
                    }
                }

                if (isExisturlReferrer_Query && (urlReferrer_Queries.Length > 3) && (urlReferrer_Queries[3].StartsWith("UserName")))
                {
                    tmpValue = (urlReferrer_Queries[3].Split('='))[1];
                    if (StrFunc.IsFilled(tmpValue))
                    {
                        opUserName = tmpValue;
                    }
                }
                if (isExisturlReferrer_Query && (urlReferrer_Queries.Length > 4) && (urlReferrer_Queries[4].StartsWith("Pwd")))
                {
                    tmpValue = (urlReferrer_Queries[4].Split('='))[1];
                    if (StrFunc.IsFilled(tmpValue))
                    {
                        opPwd = tmpValue;
                    }
                }
                #endregion
            }

            return ret;
        }

        /// <summary>
        /// Lecture de la valeur d'un timeout sur JQuery.Block dans fichier Config
        /// </summary>
        /// <param name="pSuffix"></param>
        /// <returns></returns>
        public static int GetTimeoutJQueryBlock(string pSuffix)
        {
            return GetTimeoutJQueryBlock(pSuffix, null);
        }
        public static int GetTimeoutJQueryBlock(string pSuffix, string pSuffixeSubstitute)
        {
            int _timeout = (int)SystemSettings.GetAppSettings("JQueryBlockTimeOut", typeof(Int32), 0);

            string _suffix;
            if (StrFunc.IsFilled(pSuffixeSubstitute))
            {
                _suffix = "_" + pSuffixeSubstitute;
                _timeout = (int)SystemSettings.GetAppSettings("JQueryBlockTimeOut" + _suffix, typeof(Int32), _timeout);
            }
            if (StrFunc.IsFilled(pSuffix))
            {
                _suffix = "_" + pSuffix;
                _timeout = (int)SystemSettings.GetAppSettings("JQueryBlockTimeOut" + _suffix, typeof(Int32), _timeout);
            }
            return _timeout * 1000;
        }

        private static string ConstructConnectionString(string pSourceTemplate, string pRdbms, string pServer, string pDatabase, string pDefUser, string pDefPwd)
        {
            string source = pSourceTemplate;

            //PL 20091105 L'utilisation de "Request" est incompatible avec le mode "pipeline intégré" de IIS7.0
            if (source.IndexOf("WEBSITE()") >= 0)
            {
                string tmpValue = System.Web.HttpContext.Current.Request.ApplicationPath;
                tmpValue = tmpValue.Substring(1, tmpValue.Length - 1);
                source = source.Replace("WEBSITE()", tmpValue);
            }
            if (StrFunc.IsFilled(pServer))
            {
                source = source.Replace("SID()", pServer);          //ORA
                source = source.Replace("SERVERNAME()", pServer);   //MSS
            }
            if (StrFunc.IsFilled(pDatabase))
            {
                source = source.Replace("SCHEMA()", pDatabase);     //ORA
                source = source.Replace("DBNAME()", pDatabase);     //MSS
            }
            if (StrFunc.IsEmpty(pDefUser))
            {
                pDefUser = GetDefUser(pRdbms, false);
            }
            source = source.Replace("USERNAME()", pDefUser);
            if (StrFunc.IsEmpty(pDefPwd))
            {
                pDefPwd = GetDefPwd(pRdbms, false);
            }
            source = source.Replace("PWD()", pDefPwd);

            return source;
        }
        private static string CompleteConnectionString(string pSource)
        {
            string source = pSource;
            if (source.IndexOf("Initial Catalog=") >= 0)
            {
                //Source is a MS SQLServer connection string (NB: Oracle don't support the key "Application Name". See also CheckOracleConnection(...) in DataAccess.cs)
                if (source.IndexOf("Application Name") < 0)
                {
                    //PL 20190107 Use csManager 
                    if (source.StartsWith("{"))
                    {
                        CSManager csManager = new CSManager(pSource);
                        source = csManager.GetSpheresInfoCs(true) + "Application Name=" + Software.NameMajorMinorType + " - Web;" + csManager.Cs;
                    }
                    else
                    {
                        source = "Application Name=" + Software.NameMajorMinorType + " - Web;" + source;
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// Get the flag indicating when the current Spheres_Collation parameter is configuring a case insensitive SQL compare
        /// </summary>
        /// <returns>true when Spheres_Collation is CI or AI, false otherwise</returns>
        /// 
        /// FI 20180906 [24159] Add Method (anciennement présente dans dataHelper)
        public static bool IsCollationCI()
        {
            //CI => Case insensitive, but accent sensitive
            //AI => Both case and accent insensitive.
            return ConfigurationManager.AppSettings["Spheres_Collation"] == "CI" || ConfigurationManager.AppSettings["Spheres_Collation"] == "AI";
        }


        /// <summary>
        /// Retourne un offset pour déterminer la date Début des flux à considérer par les EARs
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20180907 [24160] Add Method
        public static string EARHist(string pCS)
        {

            string sqlQuery = @"select 
case when EARPERIODMLTPCONSIDERED is null or EARPERIODCONSIDERED is null then
    null
else        
    convert(varchar,EARPERIODMLTPCONSIDERED) || ',' ||  EARPERIODCONSIDERED 
end as OFFSET
from dbo.EFSSOFTWARE
where IDEFSSOFTWARE=@IDEFSSOFTWARE";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDEFSSOFTWARE", System.Data.DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Software.Name);

            object obj = DataHelper.ExecuteScalar(pCS, System.Data.CommandType.Text, sqlQuery, dp.GetArrayDbParameter());
            if (null != obj)
            {
                string offset = obj.ToString();

                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(\d+),(D|M|Y)$");
                if (false == regex.IsMatch(offset))
                    throw new NotSupportedException(StrFunc.AppendFormat(@"Invalid Offset {0}. Expected Value ^(\d+),(D|M|Y)$", offset));
            }
            else
            {
                obj = "2,M"; //historique de 2 mois par défaut
            }
            return obj.ToString();
        }

        /// <summary>
        /// Retourne TRUE si la base de données courante est une base archive.
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static DateTime ReadDtArchive(string pCS)
        {
            DateTime ret = DateTime.MinValue;

            string sqlQuery = @"select DTARCHIVE from dbo.EFSSOFTWARE where IDEFSSOFTWARE=@IDEFSSOFTWARE";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDEFSSOFTWARE", System.Data.DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Software.Name);

            try
            {
                object obj = DataHelper.ExecuteScalar(pCS, System.Data.CommandType.Text, sqlQuery, dp.GetArrayDbParameter());
                if (null != obj)
                    ret = Convert.ToDateTime(obj);
            }
            catch
            {
                ret = DateTime.MinValue;
                #if DEBUG
                if (pCS.IndexOf("Connection Timeout=33") > 0)
                {
                    ret = new DtFunc().StringDateISOToDateTime("2019-01-01"); //PL 20190304 Test in progress...
                }
                #endif
            }
            return ret;
        }
    }
}
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common.Web;
using EFS.Referential;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Services;
using System.Web.Services;

namespace EFS.Spheres.WebServices
{
    /// <summary>
    /// WebService utilisé par la saisie Light
    /// </summary>
    /// FI 20191217 [XXXXX] Add
    [WebService(Namespace = "http://EFS.Spheres.Services/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public partial class CommonWebService : WebService
    {


        /// <summary>
        ///  Retourne une resource
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        /// FI 20210208 [XXXXX] Add 
        /// FI 20210302 [XXXXX] Add Try catch
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetResource(string res)
        {
            try
            {
                return Ressource.GetString(res);
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException(this, ex);
                throw;
            }
        }

        /// <summary>
        ///  Retourne une resource
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        /// FI 20221024 [XXXXX] Add 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string[] GetResourceArray(string[] res)
        {
            try
            {
                string[] ret = new string[] { };
                if (ArrFunc.IsFilled(res))
                {
                    ret = (from item in res
                           select Ressource.GetString(item)).ToArray();
                }
                return ret;
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException(this, ex);
                throw;
            }
        }

        /// <summary>
        ///  Exécute la commande select <paramref name="qry"/>
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">lorsqu'une injection SQL non autorisée est détectée (Exemple delete)</exception>
        private List<Dictionary<string, object>> ExecuteSelect(QueryParameters qry)
        {

            // search for SlqInjection
            // FI 20230202 [26251] add TRUNC(ATE)
            Regex regex = new Regex(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE){0,1}|INSERT( +INTO){0,1}|MERGE|TRUNC(ATE)|UPDATE)\b)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (regex.IsMatch(qry.Query))
                throw new InvalidOperationException($"command {regex.Match(qry.Query).Value} unauthorized");

            DataTable dt = DataHelper.ExecuteDataTable(SessionTools.CS, qry.Query, qry.Parameters.GetArrayDbParameter());

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return rows;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="idM"></param>
        /// <param name="currentDCIdentifier"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        /// FI 20230202 [26251]  Add
        public List<Dictionary<string, object>> LoadDCSHIFT(string[] col, int idM, string currentDCIdentifier)
        {
            Dictionary<string, object> restrict1 = new Dictionary<string, object>
            {
                { "col", "IDENTIFIER" },
                { "operator", "!=" },
                { "value", currentDCIdentifier }
            };

            Dictionary<string, object> restrict2 = new Dictionary<string, object>
            {
                {"col","IDM" },
                { "operator", "=" },
                { "value", idM },
            };

            return InternalLoadDataTable(col, Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(), new Dictionary<string, object>[] { restrict1, restrict2 });

        }

        /// <summary>
        /// Chargement des règle de maturité active (vue VW_MATURITYRULE_ENABLED) sauf Default Rule
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Dictionary<string, object>> LoadMaturityRule(string[] col)
        {
            Dictionary<string, object> restrict1 = new Dictionary<string, object>
            {
                { "col", "IDENTIFIER" },
                { "operator", "!=" },
                { "value", "Default Rule" }
            };

            return InternalLoadDataTable(col, "VW_MATURITYRULE_ENABLED", new Dictionary<string, object>[] { restrict1 });
        }

        /// <summary>
        ///  Génère et exécute une requête SQL. Afin d'éviter les vulnaribilités aux SQL injections certaines contraintes doivent être respectées.
        ///  <para>Seules certaines tables, vues du schema de spheres sont accessibles</para>
        ///  <para>Au minimum une restiction est obligatoire et seul l'opérateur "=" est accepté pour chaque restiction</para>
        /// </summary>
        /// <param name="col">Liste des colonnes</param>
        /// <param name="table">table</param>
        /// <param name="restrict">Liste des restrictions</param>
        /// <returns></returns>
        /// FI 20230202 [26251] 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        /// FI 20230202 [26251] Refactoring 
        public List<Dictionary<string, object>> LoadDataTable(string[] col, string table, Dictionary<string, Object>[] restrict)
        {

            if (false == IsSessionAvailable())
                throw new Exception("Authorization Required");

            try
            {

                // Afin d'éviter les vulnaribilités aux SQL injections, des restrictions sont obligatoires (cela évite que Spheres® retourne toutes les données d'une table
                if (ArrFunc.IsEmpty(restrict))
                    throw new InvalidOperationException($"restrict: empty unvalid");

                // Afin d'éviter les vulnaribilités aux SQL injections, seul l'orérateur "=" est accepté (par exemple "IDM != null" n'est pas possible => Spheres® retourne toutes les données d'une table)
                if (restrict.Where(x => x.ContainsKey("operator") && x["operator"].ToString() != "=").Count() > 0)
                    throw new InvalidOperationException($"restrict: operator unvalid");

                LoadDataTableCheckTable(table);

                return InternalLoadDataTable(col, table, restrict);
            }

            catch (Exception ex)
            {
                AspTools.WriteLogException(this, ex);
#if DEBUG
                throw;
#else
                // Message volontairement particulier afin de na pas afficher la méthode LoadDataTable
                throw new Exception($"Failed to load data (0x00230303).{Cst.CrLf}If this error occurs regularly, please contact your administrator.");
#endif
            }
        }







        /// <summary>
        ///  Afin d'éviter les vulnaribilités aux SQL injections, seules les tables existants dans EFSOBJECT (et parmi une liste restreinte) sont acceptées.
        ///  Il s'agit d'interdire tout accès à une table non autorisée
        /// </summary>
        /// <param name="table"></param>
        /// <exception cref="InvalidOperationException">si <paramref name="table"/> n'est pas accepté</exception>
        private static void LoadDataTableCheckTable(string table)
        {

            IEnumerable<(String objectName, String objectType)> efsObject = LoadEFSOBJECT();
            (String objectName, String objectType) efsObjectFound = efsObject.Where(x => x.objectName == table).FirstOrDefault();

            if (efsObjectFound == default)
            {
                throw new InvalidOperationException($"table: {table} unauthorized");
            }
            else if (false == ArrFunc.ExistInArray(new string[] { "TABLE", "VIEW" }, efsObjectFound.objectType))
            {
                throw new InvalidOperationException($"table: {table} unauthorized");
            }
            else
            {
                // Liste restreinte des tables, vues acceptées par LoadDataTable
                // en v12.1 => il faudra ajouter un attribut dans la table EFS_OBJECT 
                // Cette liste sera étendue 
                // EG 20231114 [WI737] Add VW_FUNDING....
                string[] objAuthorized = new string[] { "DERIVATIVEATTRIB",
                                                        "DERIVATIVECONTRACT", "MATURITY","MATURITYRULE",
                                                        "VW_BOOKPOSEFCTCONTRACT", "VW_BOOKPOSEFCTINSTR","VW_BOOK_VIEWER",
                                                        "VW_MARKET_IDENTIFIER",
                                                        "VW_CLEARINGTPINSTR","VW_EXTENDINSTR","VW_DRVCONTRACTMATRULE",
                                                        "VW_FEEMATRIXINSTR","VW_FEEMATRIXINSTR_UNL","VW_FEESCHEDLIBRARY","VW_FEESCHEDINSTR",
                                                        "VW_FUNDINGSCHEDLIBRARY", "VW_FUNDINGSCHEDINSTR", "VW_FUNDINGMATRIXINSTR", 
                                                        "VW_IOELEMENT","VW_STGIDENTRULESINSTR","VW_TRADEMERGERULEINSTR"};

                if (false == ArrFunc.ExistInArray(objAuthorized, efsObjectFound.objectName))
                    throw new InvalidOperationException($"table: {table} unauthorized");
            }
        }

        /// <summary>
        ///  Génère et exécute une requête SQL 
        ///  <para>Méthode méthode doit rester privée car elle accepte toute table et toute sorte de restrictions</para>
        /// </summary>
        /// <param name="col">Liste des colonnes</param>
        /// <param name="table">table</param>
        /// <param name="restrict">Liste des restrictions</param>
        /// <returns></returns>
        /// FI 20210218 [XXXXX] Add
        /// FI 20210224 [XXXXX] Gestion de column: et columnDisplay:
        /// FI 20210302 [XXXXX] Add Try catch
        /// FI 20220802 [XXXXX] Suppression de la serialisation JSON puisque effectué automatiquement par le webService
        /// FI 20230202 [26251] Refactoring pour supprimer toute vulnaribilité aux SQL injections (exemple : usage de paramètres, contôle que table est présent dans EFSOBJECT etc..)
        /// FI 20230202 [26251] Rename en InternalLoadDataTable et method private
        private List<Dictionary<string, object>> InternalLoadDataTable(string[] col, string table, Dictionary<string, Object>[] restrict)
        {
            string cs = SessionTools.CS;

            if (ArrFunc.IsEmpty(col))
                throw new ArgumentNullException("col");
            if (StrFunc.IsEmpty(table))
                throw new ArgumentNullException("table");

            string lstColumn = string.Empty;
            foreach (string item in col)
            {
                if (item.Contains(","))
                {
                    string[] resSplit = item.Split(',');

                    string column = resSplit.Where(x => x.StartsWith("column:")).FirstOrDefault();
                    if (StrFunc.IsEmpty(column))
                        throw new InvalidOperationException("column not found");
                    column = column.Replace("column:", string.Empty);

                    if (false == IsColumnNameValid(column))
                        throw new InvalidOperationException($"col: {column} unvalid");

                    string columnDisplay = resSplit.Where(x => x.StartsWith("columnDisplay:")).FirstOrDefault();
                    if (StrFunc.IsEmpty(column))
                        throw new InvalidOperationException("columnDisplay not found");
                    columnDisplay = columnDisplay.Replace("columnDisplay:", string.Empty);

                    string colExpression = ReferentialTools.SqlColExpression(null, column, columnDisplay, out string aliasColum);
                    lstColumn += $"{colExpression} as {aliasColum},";
                }
                else
                {
                    if (false == IsColumnNameValid(item))
                        throw new InvalidOperationException($"col: {item} unvalid");

                    lstColumn += $"{item} as {item},";
                }
            }
            lstColumn = StrFunc.Before(lstColumn, ",", OccurenceEnum.Last);

            string query = $"select {lstColumn}{Cst.CrLf}";
            query += $"from dbo.{table}";

            DataParameters dp = new DataParameters();

            SQLWhere sQLWhere = new SQLWhere();
            if (ArrFunc.IsFilled(restrict))
            {
                int i = 0;
                foreach (Dictionary<string, Object> item in restrict)
                {
                    i++;

                    if (false == item.ContainsKey("col"))
                        throw new InvalidOperationException($"col: expected");
                    if (false == item.ContainsKey("value"))
                        throw new InvalidOperationException($"value: expected");

                    string colR = item["col"].ToString();
                    object colValue = item["value"];
                    string @operator = item.ContainsKey("operator") ? item["operator"].ToString() : "=";

                    if (false == IsColumnNameValid(colR))
                        throw new InvalidOperationException($"col: {colR} unvalid");
                    if (false == ArrFunc.ExistInArray(new string[] { "=", "<", "<=", ">", ">=", "!=" }, @operator))
                        throw new InvalidOperationException($"operator: {table} unvalid");

                    // FI 20210224 [XXXXX] test du type de colValue  
                    if (colValue == null)
                    {
                        if (@operator == "=")
                            sQLWhere.Append($"{colR} is null");
                        else if (@operator == "!=")
                            sQLWhere.Append($"{colR} is not null");
                    }
                    else
                    {
                        string parameter;
                        if (Enum.TryParse<DataParameter.ParameterEnum>(colR, out DataParameter.ParameterEnum parameterEnum))
                        {
                            DataParameter dataParameter = DataParameter.GetParameter(cs, parameterEnum);
                            dp.Add(dataParameter, colValue);
                            parameter = $"@{dataParameter.ParameterKey}";
                        }
                        else if (colValue.GetType().Equals(typeof(System.String)))
                        {
                            dp.Add(new DataParameter(cs, $"PARAM{i}", DbType.AnsiString, 256), ObjFunc.FmtToISo(colValue, TypeData.TypeDataEnum.@string));
                            parameter = $"@PARAM{i}";
                        }
                        else if (colValue.GetType().Equals(typeof(System.Int32)))
                        {
                            dp.Add(new DataParameter(cs, $"PARAM{i}", DbType.Int32), ObjFunc.FmtToISo(colValue, TypeData.TypeDataEnum.integer));
                            parameter = $"@PARAM{i}";
                        }
                        else
                            throw new NotSupportedException($"type: {colValue.GetType()} is not supported");

                        sQLWhere.Append($"{colR} {@operator} {parameter}");
                    }
                }
            }

            if (sQLWhere.Length() > 0)
                query += $"{Cst.CrLf}{sQLWhere}";


            QueryParameters queryParameters = new QueryParameters(cs, query, dp);

            return ExecuteSelect(queryParameters);

        }

        /// <summary>
        ///  Retourne true si le nom de colonne <paramref name="columnName"/> est valide. seuls les caractères 0-9A-Z_ sont acceptés (sauf null/NULL qui n'est pas accepté).  
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// FI 20230202 [26251] Add 
        private static Boolean IsColumnNameValid(string columnName)
        {
            Boolean ret = columnName.ToUpper() != "NULL";
            if (ret)
            {
                Regex regex = new Regex($"[^0-9A-Z_]");
                ret = (false == regex.IsMatch(columnName));
            }
            return ret;
        }


        /// <summary>
        /// Sauvegarde dans la table COOKIE
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        // EG 20220613 [XXXXX] New
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void SaveSQLCookie(string element, string value)
        {
            AspTools.WriteSQLCookie(element, value);
        }

        /// <summary>
        /// Retourne true la session est toujours active
        /// </summary>
        /// <returns></returns>
        /// EG 20220613 [XXXXX] New 
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public bool IsSessionAvailable()
        {
            // FI 20221118 [XXXXX] Mise en commentaire
            // SessionTools.IsSessionAvailable ne fait que vérifier que (null !=HttpContext.Session)
            //return SessionTools.IsSessionAvailable;

            // FI 20221118 usage de SessionTools.IsConnected => La session est active si SessionTools.IsConnected
            // Lorsque la session expire SessionTools.Collaborator devient null et SessionTools.IsConnected retourne false (voir Session_End dans global.asax.cs)  
            return SessionTools.IsConnected;
        }
        /// <summary>
        /// Affichage d'un éventuelm message retourné lors de la connexion après le chargement du menu Spheres.
        /// Message construit dans la méthode OnConnectOk  dans SummaryCommon.cs
        /// </summary>
        /// <returns></returns>
        /// EG 20221121 New : Affichage message géré via Default.js (Méthode _DisplayMessageAfterOnConnectOk)
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Tuple<string, string, string> GetMessageAfterOnConnectOk()
        {
            Tuple<string, string, string> retMessage = null;
            if (null != SessionTools.MessageAfterOnConnectOk)
            {
                string title = SessionTools.MessageAfterOnConnectOk.Item1;
                string status = SessionTools.MessageAfterOnConnectOk.Item2.ToString().ToLower();
                string message = SessionTools.MessageAfterOnConnectOk.Item3;
                retMessage = new Tuple<string, string, string>(title, status, message);
            }
            return retMessage;
        }
        /// <summary>
        /// Retourne l'action à opérer après connexion
        /// </summary>
        /// <returns></returns>
        /// EG 20220123 [26235][WI543] New
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetActionOnConnect()
        {
            return SessionTools.ActionOnConnect.ToString();
        }


        /// <summary>
        /// Chargement des entrées présentes dans EFSOBJECT
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<(String objectName, String objectType)> LoadEFSOBJECT()
        {
            List<MapDataReaderRow> row = new List<MapDataReaderRow>();
            QueryParameters qry = new QueryParameters(SessionTools.CS, "select obj.OBJECTNAME, obj.OBJECTTYPE from dbo.EFSOBJECT obj", new DataParameters());

            using (IDataReader dr = DataHelper.ExecuteReader(CSTools.SetCacheOn(SessionTools.CS), CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                row = DataReaderExtension.DataReaderMapToList(dr);
            }

            IEnumerable<(String objectName, String objectType)> ret = from item in row
                                                                      select (Convert.ToString(item["OBJECTNAME"].Value), Convert.ToString(item["OBJECTTYPE"].Value));


            return ret;
        }
    }

#if DEBUG
    public partial class CommonWebService
    {

        /// <summary>
        ///  Exécute la commande select
        /// </summary>
        /// <remarks>
        /// Cette méthode n'existe qu'en DEBUG. Il est strictement interdit de publier cette web méthode en Release.
        /// ExecuteSelect est vulnérable aux injections SQL et n'est pas livrée en Release.
        /// ExecuteSelect est présente en debug uniquement à titre educatif pour illustrer l'outil sqlMap (https://sqlmap.org/)
        /// </remarks>
        /// <param name="col">Liste des colonnes</param>
        /// <param name="table"></param>
        /// <param name="restrict"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Dictionary<string, object>> LoadDataTableVulnerable(string[] col, string table, Dictionary<string, Object>[] restrict)
        {
            try
            {
                if (ArrFunc.IsEmpty(col))
                    throw new ArgumentNullException("col");
                if (StrFunc.IsEmpty(table))
                    throw new ArgumentNullException("table");

                string lstColumn = string.Empty;
                foreach (string item in col)
                {
                    if (item.Contains(","))
                    {
                        string[] resSplit = item.Split(',');

                        string column = resSplit.Where(x => x.StartsWith("column:")).FirstOrDefault();
                        if (StrFunc.IsEmpty(column))
                            throw new InvalidOperationException("column not found");
                        column = column.Replace("column:", string.Empty);

                        string columnDisplay = resSplit.Where(x => x.StartsWith("columnDisplay:")).FirstOrDefault();
                        if (StrFunc.IsEmpty(column))
                            throw new InvalidOperationException("columnDisplay not found");
                        columnDisplay = columnDisplay.Replace("columnDisplay:", string.Empty);

                        string colExpression = ReferentialTools.SqlColExpression(null, column, columnDisplay, out string aliasColum);
                        lstColumn += $"{colExpression} as {aliasColum},";
                    }
                    else
                    {
                        lstColumn += $"{item} as {item},";
                    }
                }
                lstColumn = StrFunc.Before(lstColumn, ",", OccurenceEnum.Last);

                string query = $"select {lstColumn}{Cst.CrLf}";
                query += $"from dbo.{table}";

                SQLWhere sQLWhere = new SQLWhere();
                if (ArrFunc.IsFilled(restrict))
                {
                    foreach (Dictionary<string, Object> item in restrict)
                    {
                        string colfK = item["col"].ToString();
                        object colValue = item["value"];
                        string @operator = item.ContainsKey("operator") ? item["operator"].ToString() : "=";

                        // FI 20210224 [XXXXX] test du type de colValue  
                        if (colValue == null)
                        {
                            if (@operator == "=")
                                sQLWhere.Append($"{colfK} is null");
                            else
                                sQLWhere.Append($"{colfK} is not null");
                        }
                        else
                        {
                            if (colValue.GetType().Equals(typeof(System.String)))
                            {
                                sQLWhere.Append($"{colfK} {@operator} { DataHelper.SQLString(ObjFunc.FmtToISo(colValue, TypeData.TypeDataEnum.@string))}");
                            }
                            else if (colValue.GetType().Equals(typeof(System.Int32)))
                            {
                                sQLWhere.Append($"{colfK} {@operator} { ObjFunc.FmtToISo(colValue, TypeData.TypeDataEnum.integer)}");
                            }
                            else
                                throw new NotSupportedException($"type: {colValue.GetType()} is not supported");
                        }
                    }
                }

                if (sQLWhere.Length() > 0)
                    query += $"{Cst.CrLf}{sQLWhere}";

                QueryParameters queryParameters = new QueryParameters(SessionTools.CS, query, new DataParameters());
                return ExecuteSelect(queryParameters);
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException(this, ex);
                throw;
            }
        }

        /// <summary>
        ///  Exécute la commande select <paramref name="selectcmd"/>
        /// </summary>
        /// <param name="selectcmd"></param>
        /// <returns></returns>
        /// <remarks>
        /// Cette méthode n'existe qu'en DEBUG. Il est strictement interdit de publier cette web méthode en Release.
        /// ExecuteSelect est vulnérable aux injections SQL et n'est pas livrée en Release.
        /// ExecuteSelect est présente en debug uniquement à titre educatif pour illustrer l'outil sqlMap (https://sqlmap.org/)
        /// </remarks>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Dictionary<string, object>> ExecuteSelectVulnerable(string selectcmd)
        {
            QueryParameters queryParameters = new QueryParameters(SessionTools.CS, selectcmd, new DataParameters());
            return ExecuteSelect(queryParameters);
        }

    }
#endif

}


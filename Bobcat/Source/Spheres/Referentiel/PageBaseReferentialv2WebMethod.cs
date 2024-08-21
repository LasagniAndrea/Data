using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.Web.Services;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Process;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.Common.MQueue;
using EFS.Referential;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.Restriction;
using EFS.SpheresIO;
using EfsML.DynamicData;
using EfsML.Business;

//20071212 FI Ticket 16012 => Migration Asp2.0
using SpheresMenu = EFS.Common.Web.Menu;
using System.Collections.Generic;

using FpML.Enum;
using EfsML.Enum.Tools;
using FpML.Interface;
using EfsML;

namespace EFS.Referential
{
    /// <summary>
    /// Description résumée de PageBaseReferentialv2.
    /// </summary>
    public partial class PageBaseReferentialv2
    {

        /// <summary>
        ///  Retourne le jeux de donnée autocomplete sur une referential colonne avec Relation
        ///  <para>Cette Méthode est utilisée par JQuery Widget autocomplete</para>
        /// </summary> 
        /// <param name="guid">Identifiant unique de la page</param>
        /// <param name="request">Donnée saisi par l'utilisateur</param>
        /// <param name="controlId">contrôle associé à la donnée pour laquelle il existe une relation</param>
        /// <returns></returns>
        /// FI 20210202 [XXXXX] Add
        [WebMethod]
        public static List<AutoCompleteDataItem> LoadAutoCompleteReferentialColumnRelation(string request, string guid, string controlId)
        {
            try
            {
                AutoCompleteKey key = new AutoCompleteKey
                {
                    pageGuId = guid,
                    controlId = controlId,
                };

                // Recherche des données dans le cache lorsque non présentes alimentation du cache
                if (!AutoCompleteDataCache.ContainsKey(key))
                    AutoCompleteDataCache.SetData<List<AutoCompleteDataItem>>(key, LoadAutoCompleteReferentialColumnRelation(guid, controlId));

                // Lecture des données dans le cache
                List<AutoCompleteDataItem> lst = AutoCompleteDataCache.GetData<List<AutoCompleteDataItem>>(key);

                List<AutoCompleteDataItem> ret = null;
                if (ArrFunc.IsFilled(lst))
                    ret = AutocompleteTools.OrderAutocompletata(lst, request).ToList();

                return ret;
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException("LoadAutoCompleteReferentialColumnRelation", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Retourne toutes les valeurs possibles pour l'autocomplete relation
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        /// FI 20210202 [XXXXX] Add
        private static List<AutoCompleteDataItem> LoadAutoCompleteReferentialColumnRelation(string guid, string controlId)
        {
            List<AutoCompleteDataItem> ret = new List<AutoCompleteDataItem>();

            // Recherque dans le cache de la query. Elle est bâtie à partir de la propriété relation 
            string query = DataCache.GetData<String>($"{guid}_{controlId}_Query");
            if (StrFunc.IsFilled(query))
            {
                using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, query))
                {
                    while (dr.Read())
                    {
                        ret.Add(new AutoCompleteDataItem() { id = dr[0].ToString(), identifier = dr[1].ToString() });
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Permet de mettre à jour la requête de chargement d'une donnée autocompleRelation lorsque cette dernière sont fonction de valeurs saisies (Présence de %%COLUMN_VALUE%%IDDC%% dans l'élément de la relation Condition) 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="controls">liste des contrôles (avec pour chacun leur valeur) qui ont des impacts sur données autocompleRelation</param>
        /// FI 20210116 [XXXXX] Add Method
        [WebMethod]
        public static void SyncAutoCompleteReferentialColumnRelation(string guid, Dictionary<string, object>[] controls)
        {
            try
            {
                if (ArrFunc.IsFilled(controls))
                {
                    ReferentialsReferential referential = DataCache.GetData<ReferentialsReferential>(StrFunc.AppendFormat("{0}_{1}", guid, "Referential"));
                    if (null == referential)
                        throw new NullReferenceException("referential is null");

                    foreach (ReferentialsReferentialColumn rrc in referential.Column.Where(x => x.AutoCompleteRelationEnabled && ArrFunc.IsFilled(x.Relation[0].Condition)))
                    {
                        Boolean isToSync = false;
                        foreach (ReferentialsReferentialColumnRelationCondition itemCondition in rrc.Relation[0].Condition.Where(x => x.SQLWhereSpecified))
                        {
                            foreach (Dictionary<string, object> item in controls)
                            {
                                string controlId = item["controlId"].ToString();
                                string column = GetColumn(controlId);
                                isToSync = itemCondition.SQLWhere.Contains($"%%COLUMN_VALUE%%{column}%%");
                                if (isToSync)
                                    break;
                            }
                            if (isToSync)
                                break;
                        }

                        if (isToSync)
                        {
                            string keyQuery = $"{guid}_TXT{rrc.ColumnName}_Query";
                            string keyWhereWithOutReplace = $"{guid}_TXT{rrc.ColumnName}_WhereWithoutReplace";

                            string query = DataCache.GetData<String>(keyQuery);
                            query = query.Remove(query.IndexOf("where", StringComparison.OrdinalIgnoreCase));
                            query = query + Cst.CrLf + DataCache.GetData<String>(keyWhereWithOutReplace);
                            if (StrFunc.IsFilled(query))
                            {
                                foreach (Dictionary<string, object> item in controls)
                                {
                                    string controlId = item["controlId"].ToString();
                                    string controlValue = item["value"].ToString();
                                    string column = GetColumn(controlId);
                                    if (StrFunc.IsFilled(controlValue))
                                        query = query.Replace($"%%COLUMN_VALUE%%{column}%%", $"{controlValue}");
                                    else
                                        query = query.Replace($"%%COLUMN_VALUE%%{column}%%", "null");
                                }
                            }

                            // Mise en place de la nouvelle query dans le cache
                            DataCache.SetData(keyQuery, query);

                            // Suppression de l'éventuel jeu de donnée déjà stocké
                            AutoCompleteDataCache.RemoveData(new AutoCompleteKey { pageGuId = guid, controlId = $"TXT{rrc.ColumnName}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException("SyncAutoCompleteReferentialColumnRelation", ex);
                throw ex;
            }
        }


        /// <summary>
        /// Vérification que le couple id, identifier existe
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="controlId"></param>
        /// <param name="id"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        /// FI 20210202 [XXXXX] Add
        [WebMethod]
        public static Boolean CheckAutoCompleteReferentialColumnRelation(string guid, string controlId, string id, string identifier)
        {
            try
            {
                Boolean ret = false;

                AutoCompleteKey key = new AutoCompleteKey
                {
                    pageGuId = guid,
                    controlId = controlId,
                };

                if (StrFunc.IsFilled(id) && StrFunc.IsFilled(identifier))
                {
                    // Lecture des données dans le cache
                    List<AutoCompleteDataItem> lst = AutoCompleteDataCache.GetData<List<AutoCompleteDataItem>>(key);
                    if (ArrFunc.IsFilled(lst))
                        ret = (null != lst.FirstOrDefault(x => (x.id == id) && (x.identifier == identifier)));
                }

                return ret;
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException("CheckAutoCompleteReferentialColumnRelation", ex);
                throw ex;
            }
        }


        /// <summary>
        ///  Retourne le jeux de donnée autocomplete sur une referential colonne. Spheres® retourne les valuers déjà existante
        ///  <para>Cette Méthode est utilisée par JQuery Widget autocomplete</para>
        /// </summary> 
        /// <param name="guid">Identifiant unique de la page</param>
        /// <param name="request">Donnée saisi par l'utilisateur</param>
        /// <param name="controlId">Contrôle associé à la donnée</param>
        /// <param name="controlValues">Liste des contôles (id + Value pour chacun item)</param>
        /// <returns></returns>
        [WebMethod]
        public static List<String> LoadAutoCompleteReferentialColumn(string request, string guid, string controlId, List<Dictionary<string, object>> controlValues)
        {
            try
            {
                ReferentialsReferential referential = DataCache.GetData<ReferentialsReferential>(StrFunc.AppendFormat("{0}_{1}", guid, "Referential"));
                if (null == referential)
                    throw new NullReferenceException("referential is null");


                string tableName = referential.TableName;
                string column = GetColumn(controlId);
                string filter = string.Empty;
                switch (referential.TableName)
                {
                    case "FEESCHEDULE":
                        if (column == "SCHEDULELIBRARY")
                        {
                            Dictionary<string, object> controlIdFee = (from item in controlValues.Where(x => GetColumn(Convert.ToString(x["id"])) == "IDFEE")
                                                                       select item).FirstOrDefault();

                            if (null == controlIdFee)
                                throw new NullReferenceException("IDFEE control is null");

                            filter = $"IDFEE={controlIdFee["value"]}";
                        }
                        break;
                }


                AutoCompleteKey key = new AutoCompleteKey
                {
                    pageGuId = guid,
                    controlId = controlId,
                    additionnalKey = filter
                };

                // Recherche des données dans le cache lorsque non présentes alimentation du cache
                if (!AutoCompleteDataCache.ContainsKey(key))
                    AutoCompleteDataCache.SetData<List<String>>(key, LoadAutoCompleteReferentialColumn(tableName, column, filter));

                // Lecture des données dans le cache
                List<String> lst = AutoCompleteDataCache.GetData<List<String>>(key);

                List<String> ret = null;
                if (ArrFunc.IsFilled(lst))
                    ret = AutocompleteTools.OrderAutocompletata(lst, request).ToList();

                return ret;
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException("LoadAutoCompleteReferentialColumn", ex);
                throw ex;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        // FI 20210208 [XXXXX] Add
        private static List<String> LoadAutoCompleteReferentialColumn(string table, string column, string filter)
        {
            List<String> ret = new List<String>();

            string query = $"select {column} from dbo.{table}";
            if (StrFunc.IsFilled(filter))
                query += Cst.CrLf + $"where {filter}";

            if (StrFunc.IsFilled(query))
            {
                using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, query))
                {
                    while (dr.Read())
                    {
                        ret.Add(Convert.ToString(dr[0]));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne le nom de colonne à partir du controlId
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        // FI 20210208 [XXXXX] Add
        private static string GetColumn(string controlId)
        {
            return controlId.Substring(3, controlId.Length - 3);
        }

    }
}

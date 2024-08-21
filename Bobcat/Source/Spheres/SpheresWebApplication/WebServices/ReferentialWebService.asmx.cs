using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Text.RegularExpressions;


using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;

using EfsML.DynamicData;
using EFS.Common.Web;
using EFS.Referential;
using System.IO;

namespace EFS.Spheres.WebServices
{
    /// <summary>
    /// WebService utilisé par la saisie Light
    /// </summary>
    /// FI 20191217 [XXXXX] Add
    [WebService(Namespace = "http://EFS.Spheres.WebServices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Pour autoriser l'appel de ce service Web depuis un script à l'aide d'ASP.NET AJAX, supprimez les marques de commentaire de la ligne suivante. 
    [System.Web.Script.Services.ScriptService]
    public class ReferentialWebService : WebService
    {

        /// <summary>
        /// 
        /// </summary>
        private const string OpenReferentialPattern = @"^OpenReferential\((.+,?)\)$";

        /// <summary>
        ///  Charge dans une liste les valeurs autocomplete associées à un contrôle
        /// </summary>
        /// <param name="request">donnée saisi par l'utilisateur</param>
        /// <param name="controlId">control sur le quel s'applique l'autocomplete</param>
        /// <param name="openReferential">Contenu de l'appel à la function js OpenReferential (Contient toutes les caractéristiques nécessaires à l'ouverture du grid)</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        /// FI 20210302 [XXXXX] Add Try catch
        public List<string> LoadAutoCompleteData(string request, string controlId, string openReferential)
        {
            try
            {
                Regex regex = new Regex(OpenReferentialPattern);

                if (false == regex.IsMatch(openReferential))
                    throw new ArgumentException("openReferential Function is expected");

                List<string> ret = new List<string>();

                AutoCompleteKey key = new AutoCompleteKey
                {
                    pageGuId = string.Empty, //Cache partagé entre plusieurs pages
                    controlId = controlId,
                    additionnalKey = openReferential
                };

                if (!AutoCompleteDataCache.ContainsKey(key))
                    AutoCompleteDataCache.SetData<List<string>>(key, LoadAutocompleteData(openReferential));

                List<string> lstData = AutoCompleteDataCache.GetData<List<string>>(key);
                if (ArrFunc.IsFilled(lstData))
                    ret = AutocompleteTools.OrderAutocompletata(lstData, request).ToList();

                return ret;
            }
            catch (Exception ex)
            {
                AspTools.WriteLogException(this, ex);
                throw;
            }
        }

        /// <summary>
        ///  Chargement des données possibles pour alimenattion de l'autocomplete
        /// </summary>
        /// <param name="openReferential">Contenu de l'appel à la méthode javascript:OpenReferential</param>
        /// <returns></returns>
        /// FI 20191227 [XXXXX] Add
        /// FI 20221107 [XXXXX] Refactoring (use of DataCache)
        private static List<string> LoadAutocompleteData(string openReferential)
        {
            List<String> ret = new List<string>();

            if (false == SessionTools.IsConnected)
                return ret;

            ReferentialsReferential referential;
            int indexCol;

            Regex regex = new Regex(OpenReferentialPattern);
            //Valeurs des paramètres de la fonction OpenReferential
            string[] OpenReferentialParameters = regex.Matches(openReferential)[0].Groups[1].Value.Split(',').ToList().Select(p => p.Trim()).ToArray();
            var parameter = new
            {
                XMLName = (OpenReferentialParameters[0] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[0],
                title = OpenReferentialParameters[1],
                subTitle = (OpenReferentialParameters[2] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[2],
                listType = (OpenReferentialParameters[3] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[3],
                isFiltered = OpenReferentialParameters[4],
                type_KeyField = (OpenReferentialParameters[5] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[5],
                clientIdForDumpKeyField = (OpenReferentialParameters[6] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[6],
                sqlColumn = (OpenReferentialParameters[7] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[7],
                clientIdForDumpSqlColumn = (OpenReferentialParameters[8] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[8],
                condApp = (OpenReferentialParameters[9] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[9],
                valueFK = (OpenReferentialParameters[10] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[10],
                listParam = (OpenReferentialParameters[11] == JavaScript.JS_NULL) ? new string[] { string.Empty } : OpenReferentialParameters[11].Split(';'),
                dynamicArg = (OpenReferentialParameters[12] == JavaScript.JS_NULL) ? string.Empty : OpenReferentialParameters[12]
            };

            //key => key est constituée des élements qui rentrent dans l'initialisation de ReferentialsReferential et de sa query associée
            string cacheKey = $"LoadAutocompleteDataOpenReferential_{parameter.XMLName}||{parameter.listType}||{parameter.condApp}||{parameter.valueFK}||{ArrFunc.GetStringList(parameter.listParam)}||{parameter.dynamicArg}";

            Tuple<ReferentialsReferential, QueryParameters> cacheResult = DataCache.GetData<Tuple<ReferentialsReferential, QueryParameters>>($"{cacheKey}");
            if (null != cacheResult)
            {
                referential = cacheResult.Item1;
                QueryParameters queryParameters = cacheResult.Item2;
                string cs = ReferentialTools.AlterConnectionString(SessionTools.CS, referential);

                indexCol = -1;
                if (StrFunc.IsFilled(parameter.sqlColumn))
                {
                    indexCol = referential.GetIndexColSQL(parameter.sqlColumn);
                }
                else if (parameter.type_KeyField == Cst.KeyField && referential.ExistsColumnKeyField)
                {
                    indexCol = referential.IndexColSQL_KeyField;
                }
                else if (parameter.type_KeyField == Cst.DataKeyField && referential.ExistsColumnDataKeyField)
                {
                    indexCol = referential.IndexColSQL_DataKeyField;
                }

                if (indexCol > -1)
                {
                    ReferentialTools.ExecutePreSelect(referential, string.Empty);
                    ret = LoadDataAutocompleteQuery(ReferentialTools.AlterConnectionString(SessionTools.CS, referential), queryParameters, indexCol);
                }
                return ret;
            }
            else
            {
                string condApp = parameter.condApp;
                string FKValueForFilter = string.Empty; //ici, on ne filtre pas avec  la donnée en cours de saisie dans le contrôle {controlId} . C'est l'autocomplte qui gère cela 

                string valueFK = parameter.valueFK;
                string columnFK = string.Empty;

                string listeType = parameter.listType;
                string ObjectName = parameter.XMLName;

                string IDMenu = string.Empty;
                string[] param = parameter.listParam;
                Dictionary<string, ReferentialsReferentialStringDynamicData> dynamicArgs = ReferentialTools.CalcDynamicArgumentFromHttpParameter2(Ressource.DecodeDA(parameter.dynamicArg));

                Cst.ListType listeTypeEnum = Cst.ListType.Repository;
                if (Enum.IsDefined(typeof(Cst.ListType), listeType))
                    listeTypeEnum = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), listeType);

                Boolean isConsultation = (listeTypeEnum == Cst.ListType.Consultation);

                if (isConsultation)
                {
                    LstConsult localLstConsult = new LstConsult(SessionTools.CS, ObjectName, IDMenu);

                    bool isLoadOnStart = false;
                    string idLstTemplate = ReferentialWeb.CreateNewTemporaryTemplate(SessionTools.CS, localLstConsult.IdLstConsult, "Select", isLoadOnStart, localLstConsult.Title);
                    localLstConsult.LoadTemplate(SessionTools.CS, idLstTemplate, SessionTools.Collaborator_IDA);

                    // FI 20200602 [25370] Type  Pair<string, Pair<string, string>>
                    Pair<string, Pair<string, string>> filter = new Pair<string, Pair<string, string>>() { First = "%" };
                    ReferentialWeb.InsertFilter(SessionTools.CS, localLstConsult.IdLstConsult, localLstConsult.template.IDLSTTEMPLATE, localLstConsult.template.IDA, filter);

                    bool isConsultWithDynamicArgs = ArrFunc.IsFilled(dynamicArgs);
                    localLstConsult.LoadLstDatatable(SessionTools.CS, isConsultWithDynamicArgs);

                    referential = localLstConsult.GetReferentials().Items[0];
                    //FI 20141211 [20563] Mise en commentaire
                    //referential.SetDynamicArgs(pDynamicDatas);
                    // FI 20201215 [XXXXX] Alimentation du paramètre pValueFk
                    referential.Initialize(true, condApp, param, dynamicArgs, valueFK);
                    ReferentialTools.InitializeID(referential);
                }
                else
                {
                    referential = null;
                    try
                    {
                        List<string> ObjectNameForDeserialize = ReferentialTools.GetObjectNameForDeserialize(IDMenu, ObjectName);
                        // FI 20201215 [XXXXX] Alimentation du paramètre pValueFk
                        ReferentialTools.DeserializeXML_ForModeRW(SessionTools.CS, listeTypeEnum, ObjectNameForDeserialize, condApp, param, dynamicArgs, valueFK, out referential);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }

                if (null != referential)
                {
                    indexCol = -1;
                    if (StrFunc.IsFilled(parameter.sqlColumn))
                    {
                        indexCol = referential.GetIndexColSQL(parameter.sqlColumn);
                    }
                    else if (parameter.type_KeyField == Cst.KeyField && referential.ExistsColumnKeyField)
                    {
                        indexCol = referential.IndexColSQL_KeyField;
                    }
                    else if (parameter.type_KeyField == Cst.DataKeyField && referential.ExistsColumnDataKeyField)
                    {
                        indexCol = referential.IndexColSQL_DataKeyField;
                    }

                    if (indexCol > -1)
                    {
                        ReferentialTools.ExecutePreSelect(referential, string.Empty);

                        SQLReferentialData.SelectedColumnsEnum selectedColumns = SQLReferentialData.SelectedColumnsEnum.NoHideOnly;
                        if (listeTypeEnum == Cst.ListType.Consultation)
                            selectedColumns = SQLReferentialData.SelectedColumnsEnum.All;

                        SQLReferentialData.SQLSelectParameters sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, selectedColumns, referential);

                        bool isTableMainOnly_True = true;
                        bool isColumnDataKeyField_False = false;

                        // FI 20201215 [XXXXX] usade de referential.ColumnForeignKeyField et referential.ValueForeignKeyField
                        QueryParameters query = SQLReferentialData.GetQuery_LoadReferential(sqlSelectParameters,
                                                     referential.ColumnForeignKeyField, referential.ValueForeignKeyField, isColumnDataKeyField_False,
                                                     isTableMainOnly_True,
                                                     out ArrayList alChildSQLSelect, out ArrayList alChildTablename, out bool isQueryWithSubTotal);

                        bool isQueryWithFKValue = (query.Query.IndexOf(Cst.FOREIGN_KEY) >= 0);
                        if (isQueryWithFKValue)
                        {
                            // FI 20201215 [XXXXX] usade de referential.ValueForeignKeyField
                            query.Query = query.Query.Replace(Cst.FOREIGN_KEY, SQLReferentialData.BuildValueFormated(referential, referential.ValueForeignKeyField, isColumnDataKeyField_False));
                        }

                        DataCache.SetData<Tuple<ReferentialsReferential, QueryParameters>>(cacheKey, new Tuple<ReferentialsReferential, QueryParameters>(referential, query));

                        ret = LoadDataAutocompleteQuery(ReferentialTools.AlterConnectionString(SessionTools.CS, referential), query, indexCol);
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="query"></param>
        /// <param name="indexCol"></param>
        /// <returns></returns>
        /// FI 20221107 [XXXXX] 
        private static List<string> LoadDataAutocompleteQuery(string cs, QueryParameters query, int indexCol)
        {
            List<String> ret = new List<string>();

            using (IDataReader dr = DataHelper.ExecuteReader(cs, null, CommandType.Text, query.QueryHint, query.GetArrayDbParameterHint()))
            {
                while (dr.Read())
                {
                    ret.Add(dr[indexCol].ToString());
                }
            }

            return ret;
        }
    }

}

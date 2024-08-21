using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Data;
using System.Drawing;
using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.Spheres.WebServices
{
    /// <summary>
    /// Description résumée de WSDataService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Pour autoriser l'appel de ce service Web depuis un script à l'aide d'ASP.NET AJAX, supprimez les marques de commentaire de la ligne suivante. 
    [ScriptService]
    public class WSDataService : WebService
    {
        private Dictionary<string, List<DataTypeAhead>> cachedLstData = new Dictionary<string, List<DataTypeAhead>>();
        private Dictionary<string, QueryParameters> cachedLstQuery = new Dictionary<string, QueryParameters>();

        public WSDataService()
        {
            cachedLstQuery = SessionTools.CachedWSLstQuery;
            if (null == cachedLstQuery)
                cachedLstQuery = new Dictionary<string, QueryParameters>();

            cachedLstData = SessionTools.CachedWSLstData;
            if (null == cachedLstData)
                cachedLstData = new Dictionary<string, List<DataTypeAhead>>();
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<DataTypeAhead> GetData(string pKeyValues, string pFilter)
        {
            if (false == cachedLstData.ContainsKey(pKeyValues))
                GetDataList(pKeyValues);

            return cachedLstData[pKeyValues].Where(data => data.DisplayValue.ToLower().Contains(pFilter.ToLower())).ToList();
        }

        private void GetDataList(string pKeyValues)
        {
            List<DataTypeAhead> lstData = new List<DataTypeAhead>();
            try
            {
                if (pKeyValues.ToLower().StartsWith("static"))
                {
                    lstData = GetStaticList(pKeyValues);
                }
                else
                {
                    if (false == cachedLstQuery.ContainsKey(pKeyValues))
                    {
                        QueryParameters queryParameters = GetQuery(pKeyValues);
                        if (null != queryParameters)
                            cachedLstQuery.Add(pKeyValues, queryParameters);
                    }


                    if (StrFunc.IsFilled(cachedLstQuery[pKeyValues].query))
                    {
                        IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text,
                            cachedLstQuery[pKeyValues].query, cachedLstQuery[pKeyValues].parameters.GetArrayDbParameter());

                        List<string> lstColumns = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToList();
                        bool isDisplayValue = lstColumns.Contains("DISPLAYVALUE");
                        bool isDescription = lstColumns.Contains("DESCRIPTION");

                        while (dr.Read())
                        {
                            DataTypeAhead data = new DataTypeAhead();
                            data.Id = dr["ID"].ToString();
                            data.Identifier = dr["IDENTIFIER"].ToString();
                            data.DisplayName = dr["DISPLAYNAME"].ToString();
                            if (isDisplayValue)
                                data.DisplayValue = dr["DISPLAYVALUE"].ToString();
                            else if (isDescription)
                                data.DisplayValue = dr["IDENTIFIER"].ToString() + " -" + dr["DESCRIPTION"].ToString();
                            else
                                data.DisplayValue = dr["IDENTIFIER"].ToString();
                            lstData.Add(data);
                        }

                        SessionTools.CachedWSLstQuery = cachedLstQuery;
                    }
                }
                if (null != lstData)
                {
                    cachedLstData.Add(pKeyValues, lstData);
                    SessionTools.CachedWSLstData = cachedLstData;
                }

            }
            catch (Exception ex)
            {
                string exp = ex.ToString(); //Setup a breakpoint here to verify any exceptions raised.
            }
        }

        private List<DataTypeAhead> GetStaticList(string pKeyValues)
        {
            List<DataTypeAhead> _lstData = new List<DataTypeAhead>();
            List<CultureInfo> lstCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(culture => (false == culture.IsNeutralCulture)).ToList();
            lstCultures.ForEach(culture =>
            {
                        DataTypeAhead data = new DataTypeAhead();
                        data.Id = culture.Name;
                        data.Identifier = culture.DisplayName;
                        data.DisplayName = culture.NativeName;
                        data.DisplayValue = culture.NativeName;
                        _lstData.Add(data);
            });
            return _lstData;
        }

        private QueryParameters GetQuery(string pKeyValues)
        {
            QueryParameters queryParameters = null;
            DataParameters parameters = new DataParameters();
            string sqlSelect = string.Empty;

            string[] _keyValues = pKeyValues.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (ArrFunc.IsFilled(_keyValues))
            {
                string _key = _keyValues[0];
                string[] _values = null;
                if (2 == _keyValues.Length)
                    _values = _keyValues[1].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                switch (_key)
                {
                    case "ACTOR":
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME
                        from dbo.ACTOR ac";
                        break;

                    case "ACTORROLE":
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                        from dbo.ACTOR ac
                        inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR" + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + ")";
                        break;

                    case "ACTOR_POSCOLLATERAL": // Critère manquant : <SQLWhere>tblmain.ISTEMPLATE=0</SQLWhere>
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                        from dbo.ACTOR ac
                        inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR" + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + @")
                        where (ac.DTDISABLED is null)";
                        break;

                    case "ASSET_COMMODITY":
                    case "ASSET_EQUITY":
                    case "ASSET_RATEINDEX":
                        sqlSelect = @"select IDASSET as ID, IDENTIFIER as IDENTIFIER, DISPLAYNAME as DISPLAYNAME 
                        from dbo." + _key;
                        break;

                    case "BUSINESSCENTER":
                        sqlSelect = @"select bc.IDBC as ID, bc.IDBC as IDENTIFIER, bc.DISPLAYNAME as DISPLAYNAME, bc.IDBC || ' - ' || bc.DESCRIPTION as DISPLAYVALUE
                        from dbo.BUSINESSCENTER bc
                        where " + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, "bc").ToString();
                        break;

                    case "DERIVATIVECONTRACT":
                        sqlSelect = @"select dc.IDDC as ID, dc.IDENTIFIER, dc.DISPLAYNAME 
                        from dbo.DERIVATIVECONTRACT dc";
                        break;

                    case "COUNTRY":
                        sqlSelect = @"select ct.IDCOUNTRY as ID, ct.DESCRIPTION as IDENTIFIER, isnull(ct.ISO3166_ALPHA2, ct.IDCOUNTRY) as DISPLAYNAME 
                                from dbo.COUNTRY ct
                                where " + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, "ct").ToString();
                        if (1 < _keyValues.Length)
                            sqlSelect += @" and (ct.COUNTRYTYPE" + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + @")";
                        break;

                    case "CURRENCY":
                        sqlSelect = @"select cu.IDC as ID, cu.ISO4217_ALPHA3 as IDENTIFIER, cu.DISPLAYNAME
                                from dbo.CURRENCY cu
                                where " + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, "cu").ToString();
                        break;

                    case "CSS":
                    case "ENTITY":
                    case "NCS":
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                        from dbo.ACTOR ac
                        inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = " + DataHelper.SQLString(_key) + @")
                        inner join dbo." + _key + @" tbl on (tbl.IDA = ar.IDA)";
                        break;

                    case "GBOOKROLE":
                    case "GCONTRACTROLE":
                    case "GINSTRROLE":
                    case "GMARKETROLE":
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                        from dbo.ACTOR ac
                        left outer join " + _key + @" gr on (isnull(gr.IDA, ac.IDA) = ac.IDA)  and (gr.IDROLE" +
                        _key.Replace("ROLE", string.Empty) + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + ")";
                        break;

                    case "MARKET":
                        sqlSelect = @"select IDM as ID, SHORT_ACRONYM as IDENTIFIER, DISPLAYNAME 
                        from dbo.VW_MARKET_IDENTIFIER";
                        break;

                    case "MARKET_EQUITY":
                    case "MARKET_ETD":
                    case "MARKET_COMMODITYSPOT":
                        sqlSelect = @"select IDM as ID, SHORT_ACRONYM as IDENTIFIER, DISPLAYNAME 
                        from dbo.VW_MARKET_IDENTIFIER";

                        if (_key == "MARKET_EQUITY")
                            sqlSelect += @" where (EQUITYMARKET = 1)";
                        else if (_key == "MARKET_ETD")
                            sqlSelect += @" where (ISTRADEDDERIVATIVE = 1)";
                        else if (_key == "MARKET_COMMODITYSPOT")
                            sqlSelect += @" where (ISCOMMODITYSPOT = 1)";
            
                        break;

                    case "PARTYRELATION":
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                        from dbo.ACTOR ac
                        inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA)
                        inner join dbo.ROLEACTOR ra on (ra.IDROLEACTOR = ar.IDROLEACTOR) 
                        where (ar.IDROLEACTOR" + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + ") or (ra.ISPARTYRELATION = 1)";
                        break;

                    case "ROLEACTOR":
                        sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                        from dbo.ACTOR ac
                        inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA)
                        inner join dbo.ROLEACTOR ra on (ra.IDROLEACTOR = ar.IDROLEACTOR) and 
                        (ra.ROLETYPE" + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + ")";
                        break;

                    case "VW_BOOK_ENTITY":
                        sqlSelect = @"select IDB as ID, IDENTIFIER as IDENTIFIER, FULLNAME as DISPLAYNAME 
                        from dbo.VW_BOOK_ENTITY";
                        break;

                    case "VW_PERMIS_MENU":
                        sqlSelect = @"select IDPERMISSION as ID, MNU_PERM_DESC as IDENTIFIER, null as DISPLAYNAME 
                        from dbo.VW_PERMIS_MENU";
                        break;

                    case "VW_MENU":
                        sqlSelect = @"select IDMENU as ID, MENUTYPE as IDENTIFIER, MENUNAME as DISPLAYNAME, IDMENU || ' - ' || MENUTYPEANDNAME as DISPLAYVALUE 
                        from dbo.VW_MENU";
                        break;

                    default:

                        // *******************************************************
                        // TO COMPLETE OR TO TEST = IFOK MOVE
                        // *******************************************************
                        switch (_key)
                        {
                            case "ACTOR_TRADER":// GLOP
                                break;

                            case "ASSETCATEGORY": // GLOP 
                                sqlSelect = @"select IDASSET as ID, IDENTIFIER_DISPLAYNAME as IDENTIFIER, null as DISPLAYNAME 
                                from dbo.VW_ASSET
                                where (ASSETCATEGORY " + ConstructListOfValues(_values, TypeData.TypeDataEnum.@string) + ")";
                                break;

                            case "ASSET_ETDPREVIOUS": // GLOP
                                sqlSelect = @"select ast.IDASSET as ID, ast.IDENTIFIER, ast.DISPLAYNAME 
                                from dbo.ASSET_ETD ast
                                where (ast.IDDERIVATIVEATTRIB in 
                                (   select pda.IDDERIVATIVEATTRIB from dbo.DERIVATIVEATTRIB pda
                                    inner join dbo.DERIVATIVECONTRACT pdc on pdc.IDDC = pda.IDDC
                                    inner join dbo.MARKET ma on ma.IDM = pdc.IDM
                                    inner join dbo.DERIVATIVECONTRACT dc on dc.IDM = ma.IDM
                                    inner join dbo.DERIVATIVEATTRIB da on da.IDDC = dc.IDDC
                                    where da.IDDERIVATIVEATTRIB" + ConstructListOfValues(_values, TypeData.TypeDataEnum.integer) + 
                                ")";
                                break;


                            case "ASSET_UNL": // GLOP
                                sqlSelect = @"select ast.IDASSET as ID, ast.IDENTIFIER, ast.DISPLAYNAME 
                                from dbo.VW_ASSET ast
                                where (ASSETCATEGORY = '" + _keyValues[1] + "' and ('Future' <> '" + _keyValues[1] + "')";
                                break;

                            case "ASSET_ETD_UNL": // GLOP
                                sqlSelect = @"select ast.IDASSET as ID, ast.IDENTIFIER, ast.DISPLAYNAME 
                                from dbo.ASSET_ETD ast
                                where (ISOPENCLOSESETTLT = 0) and (ISSECURITYSTATUS = 1) and (IDDERIVATIVEATTRIB in
                                (
                                    select da.IDDERIVATIVEATTRIB
                                    from dbo.DERIVATIVEATTRIB da
                                    inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC_UNL=da.IDDC and dc.IDDC = " + _keyValues[1] + @")
                                )";
                                break;

                            case "CASCADING":
                                sqlSelect = @"select dc.IDDC as ID, dc.IDENTIFIER, dc.DISPLAYNAME 
                                from dbo.DERIVATIVECONTRACT dc
                                where (dc.IDDC <> " + _key[1] + @") and (IDM in (select IDM from dbo.DERIVATIVECONTRACT where IDDC = " + _keyValues[1] + "))";
                                break;

                            case "COMMODITYSPOT":
                                sqlSelect = @"select IDM as ID, SHORT_ACRONYM as IDENTIFIER, DISPLAYNAME 
                                from dbo.VW_MARKET_IDENTIFIER
                                where (ISCOMMODITYSPOT = 1)";
                                break;

                            case "COMPARTMENT": // GLOP
                                sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                                from dbo.ACTOR ac
                                inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and 
                                (ar.IDROLEACTOR in ('CCLEARINGCOMPART','HCLEARINGCOMPART','MCLEARINGCOMPART'))";
                                if (1 < _keyValues.Length)
                                    sqlSelect += @"and (ar.IDA_ACTOR = " + _keyValues[1] + ")";
                                break;


                            case "CSSMARKET":
                                sqlSelect = @"select IDM as ID, SHORT_ACRONYM as IDENTIFIER, DISPLAYNAME 
                                from dbo.VW_MARKET_IDENTIFIER
                                where (IDM in (select IDM from dbo.MARKET where IDA = " + _keyValues[1] + ")";
                                break;

                            case "CSS_SECURITIES":
                                sqlSelect = @"select ac.IDA as ID, ac.IDENTIFIER, ac.DISPLAYNAME 
                                from dbo.ACTOR ac
                                inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = 'CSS')
                                inner join dbo.CSS css on (css.IDA = ar.IDA) and (css.CSSTYPE in ('SECURITIES','CASH-SECURITIES'))";
                                break;

                            case "DERIVATIVECONTRACT_DELIVERY": // GLOP
                                break;

                            case "DERIVATIVECONTRACT_POSEQUSECURITY": // GLOP
                                break;

                            case "DERIVATIVECONTRACT_UNL": // GLOP
                                break;


                            case "VW_BOOK_VIEWER": // GLOP
                                break;

                            case "VW_CLEARINGTPPARTY": // GLOP
                                sqlSelect = @"select ID_ as ID, IDENTIFIER as IDENTIFIER, FULLNAME as DISPLAYNAME 
                                from dbo.VW_CLEARINGTPPARTY";
                                if (1 < _key.Length)
                                    sqlSelect += @"where (TYPEPARTY = '" + _keyValues[1] + "')";
                                else
                                    sqlSelect += @"where (TYPEPARTY = tblmain.TYPEPARTY)";
                                break;

                            case "VW_COLLATERALENVPARTY": // GLOP
                                sqlSelect = @"select ID_ as ID, IDENTIFIER as IDENTIFIER, FULLNAME as DISPLAYNAME 
                                from dbo.VW_COLLATERALENVPARTY";
                                if (1 < _key.Length)
                                    sqlSelect += @"where (TYPEPARTY = '" + _keyValues[1] + "')";
                                break;

                            case "VW_FEEMATRIXPARTYA": // GLOP
                                break;
                            case "VW_FEEMATRIXPARTYB": // GLOP
                                break;
                            case "VW_FEEMATRIXPARTYB_ACTOR":
                                sqlSelect = @"select ID_ as ID, IDENTIFIER as IDENTIFIER, FULLNAME as DISPLAYNAME 
                                from dbo.VW_FEEMATRIXPARTYB
                                where (FEETYPEPARTYB = 'Actor')";
                                break;
                            case "VW_FEEMATRIXOP1": // GLOP
                                break;
                            case "VW_FEEMATRIXOP2": // GLOP
                                break;
                            case "VW_INVRULESBOOK": // GLOP
                                break;

                            case "VW_PARTYTEMPLATESALES": // GLOP
                                break;

                            case "VW_TRADEMERGERULEPARTY": // GLOP
                                break;
                        }

                        break;
                }
            }
            if (StrFunc.IsFilled(sqlSelect))
                queryParameters = new QueryParameters(SessionTools.CS, sqlSelect, parameters);
            return queryParameters;
        }


        private string ConstructListOfValues(string[] pValues, TypeData.TypeDataEnum pDataType)
        {
            string ret = string.Empty;
            if (ArrFunc.IsFilled(pValues))
            {
                if (1 == pValues.Length)
                {
                    ret = " = " + (TypeData.IsTypeString(pDataType)? DataHelper.SQLString(pValues[0]):pValues[0]);
                }
                else
                {
                    string lstValues = string.Empty;
                    pValues.ToList().ForEach(value => 
                    {
                        lstValues += (TypeData.IsTypeString(pDataType) ? DataHelper.SQLString(value) : value) + ",";
                    });
                    ret += " in (" + lstValues.Remove(lstValues.Length - 1) + ")";
                }
            }
            return ret;

        }

        private string SetGroupRole(string pTable, string[] pValues)
        {
            return "(gr.IDROLE" + pTable.Replace("ROLE",string.Empty) + ConstructListOfValues(pValues,TypeData.TypeDataEnum.@string) + ")";
        }

    }


}

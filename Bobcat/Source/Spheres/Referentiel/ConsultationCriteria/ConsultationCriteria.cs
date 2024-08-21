using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.Referential
{
    /// <summary>
    ///  Classe en charge du remplacement des mots clés %%CC:  (Consultation Criteria)
    /// </summary>
    /// FI 20201125 [XXXXX] Add
    public class ConsultationCriteria
    {
        /// <summary>
        /// Liste des Table (consultation où les mots clés %%CC: sont gérés 
        /// </summary>
        public readonly string[] AvailableTableName = {  "POSSYNT", "POSDET", "POSACTIONDET", "FLOWSBYTRADE", "FLOWSBYASSET",
                                                            "POSDETOTC", "POSACTIONDET_OTC","FLOWSBYTRADEOTC", "FLOWSBYASSETOTC",
                                                            "QUOTE_H_EOD" };
        /// <summary>                           
        ///                                     
        /// </summary>                          
        public ReferentialsReferential referential;
                                                
        /// <summary>
        /// Application éventuelle des filtres du modèle  à la requête {pQuery}
        /// <para>Cette fonctionalité est disponible uniquement sur les requêtes qui utilisent les mots clés %%CC:</para>
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        public static string ReplaceConsultCriteriaKeyword(ReferentialsReferential pReferential, string pQuery)
        {
            return new ConsultationCriteria()
            {
                referential = pReferential
            }.ReplaceConsultCriteriaKeyword(pQuery);
        }

        /// <summary>
        /// Application éventuelle des filtres du modèle à la requête {pQuery}
        /// <para>Cette fonctionalité est disponible uniquement sur les requêtes qui utilisent les mots clés %%CC:</para>
        /// </summary>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// FI 20140626 [20142] Add Method
        public string ReplaceConsultCriteriaKeyword(string pQuery)
        {
            if (referential == null)
                throw new NullReferenceException("referential is null.");

            string ret = pQuery;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.CC_START) >= 0))
            {
                //lstCriteria contient les expressions SQL associées aux filtres utilisateur
                List<String> lstCriteria = new List<string>();
                if (referential.SQLWhereSpecified)
                {
                    // Liste des filtres utilisateurs
                    lstCriteria = (referential.SQLWhere.Where(x => x.ColumnNameSpecified).Select(y =>
                    {
                        SQL_ColumnCriteria sqlColumnCriteria = SQLReferentialData.GetSQL_ColumnCriteria(SessionTools.CS, y, referential);
                        //FI 20180906 [24159] Appel à la méthode SystemSettings.IsCollationCI()
                        return sqlColumnCriteria.GetExpression(SessionTools.CS, SQLCst.TBLMAIN + ".", SystemSettings.IsCollationCI());
                    })).ToList();
                }

                #region %%CC:ITRADE_JOIN%% / %%CC:ITRADE_WHERE_PREDICATE%%
                //Remplacement des mots clés %%CC:ITRADE_JOIN%% et %%CC:ITRADE_WHERE_PREDICATE%% 
                //par du code SQL qui prend en considération les filtres spécifiés par l'utilisateur
                //Il est attendu que les mots clés s'appliquent à la table TRADE (ou Equivalent VW_TRADE_XXX)
                int guard = 0;
                while (ret.Contains(Cst.CC_ITRADE_JOIN) & (guard < 100))
                {
                    guard++;
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.CC_ITRADE_JOIN);
                    string alias = arg[0];

                    string join = string.Empty;
                    string where = "1=1";

                    //FI 20160201 [XXXXX] 
                    //Par défaut, lorsque le tag IsUseCC est non présent il y a interprétation des mots clés  %%CC:ITRADE_XXX
                    if ((!referential.IsUseCCSpecified) || referential.IsUseCC)
                        GetSQLITrade(alias, lstCriteria, out join, out where);

                    ret = ret.Replace(Cst.CC_ITRADE_JOIN + "(" + alias + ")", join);
                    ret = ret.Replace(Cst.CC_ITRADE_WHERE_PREDICATE, where);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop on " + Cst.CC_ITRADE_JOIN);

#if DEBUG
                if ((guard > 1) && (false == ArrFunc.ExistInArray(AvailableTableName, referential.TableName)))
                {
                    // en debug => plantage si'il existe des mots %%CC: alors que la consultation (référentiel) n'est pas géré par la class
                    throw new Exception($"{referential.TableName} is not supported in ConsultationCriteria.");
                }
#endif
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// Retourne les jointures à appliquer à la table ITRADE et l'expression WHERE, compte tenu des critères en vigueur
        /// </summary>
        /// <param name="pAliasITrade">Alias de la table ITRADE (une table ITRADE possède les colonnes IDA_DEALER, IDA_CLEARER, IDB_DEALER, IDB_CLEARER IDM, IDASSET, IDACSS_CUSTODIAN)</param>
        /// <param name="pLstCriteria">Liste des expressions SQL issues des filtres spécifiés par l'utilsateur</param>
        /// <param name="opJoin">Retourne les jointures à appliquer pour prendre en considération les critères spécifiés</param>
        /// <param name="opWhere">Retourne l'expression where à appliquer pour prendre en considération les critères spécifiés</param>
        ///FI 20140626 [20142] Add Method
        ///FI 20140702 [20142] add QUOTE_H_EOD 
        ///FI 20140923 [XXXXX] Modify
        ///FI 20141118 [XXXXX] Modify 
        ///FI 20160201 [XXXXX] Modify (gestion de FLOWSBYASSETOTC_ALLOC)
        ///EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void GetSQLITrade(string pAliasITrade, List<string> pLstCriteria, out string opJoin, out string opWhere)
        {
            opJoin = string.Empty;
            opWhere = "1=1";

            /* 
             Explication sur isModeInnerJoin 
             Si true: Spheres® génère des jointures "inner join" avec les critères (Mode utilisé dans 90% des cas)
             Exemple  
               inner join dbo.BOOK b_dealclg on (b_dealclg.IDB = t.IDB_DEALER) and (b_dealclg.IDENTIFIER  like  '%cfd%' escape '#')            
             
             Si false: Spheres® génère des jointures "left outer join" et génère un where avec les critères 
             Exemple 
               left outer join dbo.ASSET_EQUITY a_eqty on (a_eqty.IDASSET = t.IDASSET)
               left outer join dbo.ASSET_INDEX a_idx on (a_idx.IDASSET = t.IDASSET)
               where (case ti.ASSETCATEGORY when 'EquityAsset' then a_eqty.IDENTIFIER when 'Index' then a_idx.IDENTIFIER else null end  like  '%AA%' escape '#') 
            */

            Boolean isModeInnerJoin = true;
            SQLWhere sqlWhere = new SQLWhere();

            if ((pLstCriteria != null) && pLstCriteria.Count > 0)
            {
                if (ArrFunc.ExistInArray(AvailableTableName, referential.TableName))
                {
                    // Pour chaque consultation, liste des alias qui donne accès au dealer, clearer, asset, contrat, marché, trade

                    //criteriaActor : liste des alias qui donnent les dealers et les clearers
                    string[] aliasActor = null;
                    switch (referential.TableName)
                    {
                        case "POSSYNT":
                        case "POSDET":
                        case "POSDETOTC":
                        case "POSACTIONDET":
                        case "POSACTIONDET_OTC":
                            aliasActor = new string[] { "a_dlrclr" };
                            break;
                        case "FLOWSBYTRADE":
                            aliasActor = new string[] { "a_dlr" }; /* "a_clr" est exclu car expression complexe fonction du bookDealer et de l'entité) */
                            break;
                        case "FLOWSBYTRADEOTC":
                            aliasActor = new string[] { "a_dlr", "a_clrcus" };
                            break;
                        case "FLOWSBYASSET":
                        case "FLOWSBYASSETOTC":
                            aliasActor = new string[] { "a_alloc_p" };
                            break;
                        case "QUOTE_H_EOD":
                            // N/A
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (ArrFunc.IsFilled(aliasActor))
                        opJoin = AddSqlJoinITrade(aliasActor, pAliasITrade, pLstCriteria, opJoin, isModeInnerJoin);

                    //criteriaActor : liste des alias qui donnent les book dealers et les book clearers (codage en dur en fonction de la consultation)
                    string[] aliasBook = null;
                    switch (referential.TableName)
                    {
                        case "POSSYNT":
                        case "POSDET":
                        case "POSDETOTC":
                        case "POSACTIONDET":
                        case "POSACTIONDET_OTC":
                            aliasBook = new string[] { "b_dlrclr" };
                            break;
                        case "FLOWSBYTRADE":
                            aliasBook = new string[] { "b_dlr", "b_clr" };
                            break;
                        case "FLOWSBYTRADEOTC":
                            aliasBook = new string[] { "b_dlr", "b_clrcus" };
                            break;
                        case "FLOWSBYASSET":
                        case "FLOWSBYASSETOTC":
                            aliasBook = new string[] { "b_alloc_p" };
                            break;
                        case "QUOTE_H_EOD":
                            // N/A
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (ArrFunc.IsFilled(aliasBook))
                        opJoin = AddSqlJoinITrade(aliasBook, pAliasITrade, pLstCriteria, opJoin, isModeInnerJoin);

                    //criteriaAsset : alias qui donnent l'asset (codage en dur en fonction de la consultation)
                    string[] aliasAsset = null;
                    switch (referential.TableName)
                    {
                        case "POSSYNT":
                        case "FLOWSBYASSET":
                            aliasAsset = new string[] { "a_etd" };
                            break;
                        case "POSDET":
                        case "POSACTIONDET":
                            aliasAsset = new string[] { "vw_a_etd" };
                            break;
                        case "FLOWSBYTRADE": // FI 20210401 [XXXXX] 3 alias possibles 
                            aliasAsset = new string[] { "a_etd", "a_eqty", "a_com" };
                            break;
                        case "POSDETOTC":
                        case "FLOWSBYTRADEOTC":
                            aliasAsset = new string[] { "cmnasset" , "asset"  }; //Il y a 2 alias possible pour remonter des informations sur l'asset Exemple cmnasset.IDENTIFIER ou asset.BBGCODE
                            break;
                        case "POSACTIONDET_OTC":
                        case "FLOWSBYASSETOTC":
                            aliasAsset = new string[] { "asset" };
                            break;
                        case "QUOTE_H_EOD":
                            // N/A
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (ArrFunc.IsFilled(aliasAsset))
                        opJoin = AddSqlJoinITrade(aliasAsset, pAliasITrade, pLstCriteria, opJoin, isModeInnerJoin);

                    //criteriaContrat : alias qui donnent le contrat (ETD ou COM) (codage en dur en fonction de la consultation)
                    string aliasContract = string.Empty;
                    switch (referential.TableName)
                    {
                        case "QUOTE_H_EOD":
                            aliasContract = "dc_etd";
                            break;
                        case "POSSYNT":
                        case "FLOWSBYASSET":
                            aliasContract = "dc";
                            break;
                        case "POSDET":
                        case "POSACTIONDET":
                            aliasContract = "vw_a_etd";
                            break;
                        case "FLOWSBYTRADE":
                            // N/A Complexe expression Exemple case product.GPRODUCT when 'FUT' then dc.CONTRACTSYMBOL when 'COM' then a_com.CONTRACTSYMBOL else null end as CONTRACT_CONTRACTSYMBOL
                            break;
                        case "POSDETOTC":
                        case "FLOWSBYTRADEOTC":
                            aliasContract = "cmnasset";
                            break;
                        case "POSACTIONDET_OTC":
                        case "FLOWSBYASSETOTC":
                            // N/A
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (StrFunc.IsFilled(aliasContract))
                        opJoin = AddSqlJoinITrade(new string[] { aliasContract }, pAliasITrade, pLstCriteria, opJoin, isModeInnerJoin);

                    //criteriaMarket : alias qui donnent le marché (codage en dur en fonction de la consultation)
                    string aliasMarket;
                    switch (referential.TableName)
                    {
                        case "POSSYNT":
                        case "POSDET":
                        case "POSDETOTC":
                        case "POSACTIONDET":
                        case "POSACTIONDET_OTC":
                        case "FLOWSBYTRADE":
                        case "FLOWSBYTRADEOTC":
                        case "FLOWSBYASSET":
                        case "FLOWSBYASSETOTC":
                        case "QUOTE_H_EOD":
                            aliasMarket = "mktident";
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (StrFunc.IsFilled(aliasMarket))
                        opJoin = AddSqlJoinITrade(new string[] { aliasMarket }, pAliasITrade, pLstCriteria, opJoin, isModeInnerJoin);

                    //criteriaCssCustodian : alias qui donnent le css ou le custodian (codage en dur en fonction de la consultation)
                    string aliasCss = string.Empty;
                    switch (referential.TableName)
                    {
                        case "POSSYNT":
                        case "POSDET":
                        case "POSDETOTC":
                        case "FLOWSBYASSET":
                        case "FLOWSBYASSETOTC":
                            aliasCss = "a_css";
                            break;
                        case "POSACTIONDET":
                        case "POSACTIONDET_OTC":
                        case "FLOWSBYTRADE":
                        case "FLOWSBYTRADEOTC":
                            aliasCss = "a_csscus";
                            break;
                        case "QUOTE_H_EOD":
                            // N/A
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (StrFunc.IsFilled(aliasCss))
                        opJoin = AddSqlJoinITrade(new string[] { aliasCss }, pAliasITrade, pLstCriteria, opJoin, isModeInnerJoin);

                    //criteriaTrade : alias qui donnent le trade
                    string aliasTrade = string.Empty;
                    switch (referential.TableName)
                    {
                        case "POSDET":
                        case "POSDETOTC":
                        case "POSACTIONDET":
                        case "POSACTIONDET_OTC":
                            aliasTrade = "t";
                            break;
                        case "POSSYNT":
                        case "FLOWSBYASSET":
                        case "FLOWSBYASSETOTC":
                        case "FLOWSBYTRADE":
                        case "FLOWSBYTRADEOTC":
                        case "QUOTE_H_EOD":
                            // N/A
                            break;
                        default:
                            throw new NotImplementedException($"tableNmame: {referential.TableName} is not implmented.");
                    }
                    if (StrFunc.IsFilled(aliasTrade))
                    {
                        AddSqlWhereITrade(aliasTrade, pLstCriteria, sqlWhere);
                        if (sqlWhere.Length() > 0)
                            sqlWhere = new SQLWhere(sqlWhere.ToString().Replace($"{aliasTrade}.", $"{pAliasITrade}."));
                    }
                }
            }

            if (sqlWhere.ToString().Length > 0)
                opWhere = sqlWhere.ToString().Replace(SQLCst.WHERE, string.Empty);
        }

        /// <summary>
        /// Ajoute les jointures et les restictions au code SQL {pSQL} et retourne le résultat
        /// </summary>
        /// <param name="pLstInner">Liste des jointures</param>
        /// <param name="criteria">Liste des critères qui utilisent les jointures</param>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// FI 20140626 [20142] Add Method
        private static string AddSqlJoin(List<string> pLstJoin, IEnumerable<string> pCriteria, string pSQL)
        {
            string ret = pSQL;

            if ((null != pLstJoin) && pLstJoin.Count > 0)
            {
                foreach (string item in pLstJoin)
                {
                    if (false == pSQL.Contains(item))
                        ret += Cst.CrLf + item;
                }

                if (null != pCriteria)
                {
                    foreach (string item in pCriteria)
                    {
                        if (false == pSQL.Contains(SQLCst.AND + item))
                            ret += SQLCst.AND + item;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne les jointure nécessaires pour application des critères qui s'appuient sur l'alias {pAlias} 
        /// </summary>
        /// <param name="pAlias">alias issu du critère et exploiter pour appliquer la jointure vers la table ITRADE</param>
        /// <param name="pAliasITrade">alias de la table ITrade</param>
        /// <returns></returns>
        /// FI 20140626 [20142] Add Method
        /// FI 20140923 [XXXXX] Modify  
        /// FI 20141118 [XXXXX] Modify  
        /// FI 20160201 [XXXXX] Modify 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private List<string> GetjoinFromITrade(string pAlias, string pAliasITrade, Boolean pIsInnerJoin)
        {
            List<string> ret = new List<string>();

            string sqlJoin = pIsInnerJoin ? "inner join" : "left outer join";

            switch (pAlias)
            {
                case "a_dlrclr":
                case "b_dlrclr":
                case "a_alloc_p":
                case "b_alloc_p":
                    string table = pAlias.StartsWith("a") ? "ACTOR" : "BOOK";
                    string col = pAlias.StartsWith("a") ? "IDA" : "IDB";

                    if (referential.dynamicArgsSpecified)
                    {
                        string key = referential.dynamicArgs.Keys.FirstOrDefault(x => ((x == "POSTYPE") || (x == "ACTORSIDE")));
                        if (StrFunc.IsFilled(key))
                        {
                            if (referential.dynamicArgs[key].value == "1")
                                ret.Add(sqlJoin + $" dbo.{table} {pAlias} on ({pAlias}.{col} = {pAliasITrade}.{col}_DEALER)");
                            else if (referential.dynamicArgs[key].value == "2")
                                ret.Add(sqlJoin + $" dbo.{table} {pAlias} on ({pAlias}.{col} = {pAliasITrade}.{col}_CLEARER)");
                            else if (referential.dynamicArgs[key].value == "0")
                            {
                                //default will be apply
                            }
                        }
                    }

                    if (ret.Count == 0)
                    {
                        string defaultjoin = sqlJoin + $" dbo.{table} {pAlias} on (({pAlias}.{col} = {pAliasITrade}.{col}_DEALER) or ({pAlias}.{col} = {pAliasITrade}.{col}_CLEARER))";
                        ret.Add(defaultjoin);
                    }
                    break;
                case "a_dlr":
                    ret.Add(sqlJoin + $" dbo.ACTOR {pAlias} on ({pAlias}.IDA = {pAliasITrade}.IDA_DEALER)");
                    break;
                case "a_clr":
                case "a_clrcus":
                    ret.Add(sqlJoin + $" dbo.ACTOR {pAlias} on ({pAlias}.IDA = {pAliasITrade}.IDA_CLEARER)");
                    break;
                case "b_dlr":
                    ret.Add(sqlJoin + $" dbo.BOOK {pAlias} on ({pAlias}.IDB = {pAliasITrade}.IDB_DEALER)");
                    break;
                case "b_clr":
                case "b_clrcus":
                    ret.Add(sqlJoin + $" dbo.BOOK {pAlias} on ({pAlias}.IDB = {pAliasITrade}.IDB_CLEARER)");
                    break;

                case "a_etd":
                case "vw_a_etd":
                    string tableAsset = (pAlias == "a_etd") ? "ASSET_ETD" : "VW_ASSET_ETD_EXPANDED";
                    ret.Add(sqlJoin + $" dbo.{tableAsset} {pAlias} on ({pAlias}.IDASSET = {pAliasITrade}.IDASSET)");
                    break;

                case "asset":
                    ret.Add(sqlJoin + $" dbo.VW_ASSET {pAlias} on ({pAlias}.IDASSET = {pAliasITrade}.IDASSET and {pAlias}.ASSETCATEGORY = {pAliasITrade}.ASSETCATEGORY)");
                    break;
                case "cmnasset":
                    string cmd = "select SQLJOIN from dbo.LSTJOIN where IDLSTJOIN = 'COMMONASSET'";
                    using (IDataReader dr = DataHelper.ExecuteReader(CSTools.SetCacheOn(SessionTools.CS), CommandType.Text, cmd))
                    {
                        if (dr.Read())
                        {
                            string join = ReferentialTools.ReplaceDynamicArgsInChooseExpression(referential, Convert.ToString(dr["SQLJOIN"]).Replace("<aliasTable>", pAliasITrade));

                            string[] joinSplit = join.Split('(');
                            if (ArrFunc.Count(joinSplit) == 1)
                                throw new Exception("Left parenthesis not found");
                            else if (!((joinSplit[0].Trim() == "left outer join") || (joinSplit[0].Trim() == "inner join")))
                                throw new Exception($"join : {joinSplit[0]} not expected");

                            ret.Add(sqlJoin + $" {StrFunc.After(join, joinSplit[0], OccurenceEnum.First)}");
                        }
                    }
                    break;
                case "dc":
                case "dc_etd":
                    ret.Add(sqlJoin + $" dbo.ASSET_ETD aetdcc on (aetdcc.IDASSET = {pAliasITrade}.IDASSET)");
                    ret.Add(sqlJoin + $" dbo.DERIVATIVEATTRIB dacc on (dacc.IDDERIVATIVEATTRIB=aetdcc.IDDERIVATIVEATTRIB)");
                    ret.Add(sqlJoin + $" dbo.DERIVATIVECONTRACT {pAlias} on ({pAlias}.IDDC=dacc.IDDC)");
                    break;

                case "mktident":
                    ret.Add(sqlJoin + $" dbo.VW_MARKET_IDENTIFIER mktident on (mktident.IDM = {pAliasITrade}.IDM)");
                    break;

                case "a_css":
                case "a_csscus":
                    ret.Add(sqlJoin + $" dbo.ACTOR {pAlias} on ({pAlias}.IDA = {pAliasITrade}.IDA_CSSCUSTODIAN)");
                    break;

                case "a_eqty": //FI 20140923 [XXXXX] add
                    ret.Add(sqlJoin + $" dbo.ASSET_EQUITY {pAlias} on ({pAlias}.IDASSET = {pAliasITrade}.IDASSET)");
                    break;

                case "a_idx": //FI 20140923 [XXXXX] add 
                    ret.Add(sqlJoin + $" dbo.ASSET_INDEX {pAlias} on ({pAlias}.IDASSET = {pAliasITrade}.IDASSET)");
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("alias (value:{0}) is not implemented", pAlias));
            }

            return ret;
        }

        /// <summary>
        ///  Ajoute les jointures nécessaires pour application des critères qui s'appuient sur l'alias {pAlias} 
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="pAliasITrade">alias de la table ITRADE</param>
        /// <param name="pLstCriteria">Liste des critères</param>
        /// <param name="pJoin">Jointures en cours de construction</param>
        /// <param name="pIsInnerJoin">
        /// <para>Si true, ajoute des jointures de type inner join </para>
        /// <para>Si false, ajoute des jointures de type Left outer join</para>
        /// </param>
        /// <returns></returns>
        /// FI 20140923 [XXXXX] Modify
        private string AddSqlJoinITrade(string[] pAlias, string pAliasITrade, List<string> pLstCriteria, string pJoin, Boolean pIsInnerJoin)
        {
            string ret = pJoin;

            if (ArrFunc.IsFilled(pAlias))
            {
                foreach (string alias in pAlias)
                {
                    IEnumerable<String> criteria = from item in pLstCriteria.Where(x =>
                           x.StartsWith(StrFunc.AppendFormat(" {0}.", alias)) ||
                           x.StartsWith(StrFunc.AppendFormat("({0}.", alias)))
                                                   select item;

                    if (criteria.Count() > 0)
                    {
                        List<string> lstInner = GetjoinFromITrade(alias, pAliasITrade, pIsInnerJoin);

                        if (pIsInnerJoin)
                            ret = AddSqlJoin(lstInner, criteria, ret);
                        else
                            ret = AddSqlJoin(lstInner, null, ret);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Ajoute les where nécessaires sur l'alias {pAlias} s'il existe des critères qui portent sur l'alias {pAlias}
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="pLstCriteria">Liste des critères</param>
        /// <param name="sqlWhere">Expression where en cours de construction</param>
        /// <returns></returns>
        /// FI 20140923 [XXXXX] Modify
        private void AddSqlWhereITrade(string pAlias, List<string> pLstCriteria, SQLWhere sqlWhere)
        {
            if (StrFunc.IsFilled(pAlias))
            {
                List<string> criteria = (from item in pLstCriteria
                                         where (item.Contains(StrFunc.AppendFormat(" {0}.", pAlias)) ||
                                                item.Contains(StrFunc.AppendFormat("({0}.", pAlias)))
                                         select item).ToList();

                if (criteria.Count() > 0)
                {
                    foreach (string item in criteria)
                    {
                        if (false == (sqlWhere.ToString().Contains(item)))
                            sqlWhere.Append(item);
                    }
                }
            }
        }
    }
}

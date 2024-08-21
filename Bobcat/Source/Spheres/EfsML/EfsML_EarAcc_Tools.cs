#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
#endregion Using Directives

namespace EfsML.EarAcc
{
    public class AccVariable
    {
        public string IDAccVariable;
        public string DisplayName;
        public string Table;
        public string Column;
        public Regex Regex;
        public bool IsTableWithIDE;

        public string Data
        {
            get { return LogTools.IdentifierAndId(DisplayName + " [" + Table + "." + Column + "]", IDAccVariable); }
        }

        public AccVariable(string pIdAccVariable, string pDisplayName, string pTableData, string pColumnData)
        {
            IDAccVariable = pIdAccVariable;
            DisplayName = pDisplayName;
            Table = pTableData;
            Column = pColumnData;
            Regex = new Regex(AccVariableTools.ELEMENTSTART + pIdAccVariable + AccVariableTools.ELEMENTEND, RegexOptions.IgnoreCase);
            IsTableWithIDE = false;
        }
    }

    public static class AccVariableTools
    {
        public static string ELEMENTSTART = "{";
        public static string ELEMENTEND = "}";
        public static string ELEMENTNOSTRO = "NOSTRO";
        public static string ELEMENTNOSTROJOURNALCODE = "NOSTROJOURNALCODE";
        public static string ELEMENTACCOUNT = "INTERNALACCOUNT";

        // RD 20140916 [20328] Script to valuate acc variable
        // Le montant peut être issu de l'EarDet en cours ou bien de l'EarDet0 
        // Voir la méthode AccEarDetFlowsList.GetEarDetFlowsForStreamZero()
        // Prendre le montant sur l'EarDet en cours s'il existe, sinon prendre celui sur l'EarDet0
        public static string SQL_Select_TableData = SQLCst.SQL_ANSI + @"
select {columnList}
from dbo.{table} tblData
where (tblData.IDEAR=@IDEAR)
and (tblData.INSTRUMENTNO in (@INSTRUMENTNO,@NOZERO))
and (tblData.STREAMNO in (@STREAMNO,@NOZERO))
and (tblData.EARCODE = @EARCODE)
and (tblData.EVENTTYPE = @AMOUNTTYPE)
and (tblData.EVENTCLASS = @EVENTCLASS)
order by tblData.INSTRUMENTNO desc, tblData.STREAMNO desc";

        // RD 20140916 [20328] Script to valuate acc variable 
        public static string SQL_Select_TableDataWithIDE = SQLCst.SQL_ANSI + @"
select {columnList}
from dbo.{table} tblData
inner join dbo.VW_EARACCAMOUNT amount on (amount.EARCODE = tblData.EARCODE) 
and (amount.AMOUNTTYPE = tblData.EVENTTYPE) 
and (amount.EVENTCLASS = tblData.EVENTCLASS)
inner join dbo.VW_EAREVENT evt on (evt.IDEARTYPE = amount.IDEARTYPE)
and (evt.EARTYPE = amount.EARTYPE)
and (evt.IDE = tblData.IDE)
where (tblData.IDEAR = @IDEAR) 
and (amount.IDEARTYPE = @IDEARTYPE) 
and (amount.EARTYPE = @EARTYPE)";

        /// <summary>
        /// Extrait toutes les variables contenues dans le String {pStringsWithVariable}
        /// </summary>
        /// <param name="pStringsWithVariable"></param>
        /// <returns></returns>
        public static List<string> InitVariables(string pStringWithVariable)
        {
            ArrayList stringsWithVariable = new ArrayList
            {
                pStringWithVariable
            };

            return InitVariables(stringsWithVariable); ;
        }

        /// <summary>
        /// Extrait toutes les variables contenues dans les différents Strings de la liste {pStringsWithVariable}
        /// </summary>
        /// <param name="pStringsWithVariable"></param>
        /// <returns></returns>
        public static List<string> InitVariables(ArrayList pStringsWithVariable)
        {
            try
            {
                List<string> variables = new List<string>();

                foreach (string strWithVariable in pStringsWithVariable)
                {
                    int elementStartPos = -1;
                    int elementEndPos = -1;
                    //
                    if (StrFunc.IsFilled(strWithVariable))
                    {
                        elementStartPos = strWithVariable.IndexOf(AccVariableTools.ELEMENTSTART, 0);
                        //
                        while (elementStartPos > -1)
                        {
                            elementEndPos = strWithVariable.IndexOf(AccVariableTools.ELEMENTEND, elementStartPos);
                            //
                            if (elementEndPos > -1)
                            {
                                string strVar = strWithVariable.Substring(elementStartPos, elementEndPos + AccVariableTools.ELEMENTEND.Length - elementStartPos);
                                //
                                if (false == variables.Contains(strVar))
                                    variables.Add(strVar);
                                //
                                elementStartPos = strWithVariable.IndexOf(AccVariableTools.ELEMENTSTART, elementEndPos + 1);
                            }
                            else
                                elementStartPos = -1;
                        }
                    }
                }

                return variables;
            }
            catch (Exception ex) { throw new Exception("Step 1 : Error to get variables from entry" + Cst.CrLf + ex.Message); }
        }

        /// <summary>
        /// Charger la liste des variables comptables à partir de la table ACCVARIABLE
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pVariables"></param>
        /// <param name="pAccVariables"></param>
        public static void LoadAccVariable(string pCs, List<string> pVariables, ref List<AccVariable> pAccVariables)
        {
            string sqlQuery = string.Empty;
            string strSqlVar = string.Empty;
            string sqlWhere = string.Empty;

            foreach (string stringVar in pVariables)
            {
                strSqlVar = stringVar.Substring(AccVariableTools.ELEMENTSTART.Length, stringVar.Length - AccVariableTools.ELEMENTEND.Length - 1).ToUpper();
                if (false == pAccVariables.Exists(var => var.IDAccVariable == strSqlVar))
                    sqlWhere += DataHelper.SQLString(strSqlVar) + ",";
            }

            if (StrFunc.IsFilled(sqlWhere))
            {
                sqlWhere = DataHelper.SQLUpper(pCs, "IDACCVARIABLE") + " in ( " + sqlWhere.TrimEnd(',') + ")";

                sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT + Cst.CrLf;
                sqlQuery += "IDACCVARIABLE, DISPLAYNAME, TABLEDATA, COLUMNDATA, SQLJOINDATA " + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCVARIABLE.ToString() + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + sqlWhere + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, Cst.OTCml_TBL.ACCVARIABLE) + Cst.CrLf;

                string displayName = string.Empty;
                string tableData = string.Empty;
                string columnData = string.Empty;

                DataSet dsVariable = DataHelper.ExecuteDataset(CSTools.SetCacheOn(pCs), CommandType.Text, sqlQuery);
                DataTable dtVariable = dsVariable.Tables[0];
                DataRow[] drVariable = dtVariable.Select();

                foreach (DataRow rowVariable in drVariable)
                {
                    strSqlVar = rowVariable["IDACCVARIABLE"].ToString().ToUpper();
                    displayName = rowVariable["DISPLAYNAME"].ToString();
                    tableData = rowVariable["TABLEDATA"].ToString();
                    columnData = rowVariable["COLUMNDATA"].ToString();

                    if (false == pAccVariables.Exists(var => var.IDAccVariable == strSqlVar))
                    {
                        AccVariable accVariable = new AccVariable(strSqlVar, displayName, tableData, columnData);
                        pAccVariables.Add(accVariable);

                        // Déterminer si  les table/vue exposent ou pas la colonne IDE
                        accVariable.IsTableWithIDE = DataHelper.IsExistColumn(CSTools.SetCacheOn(pCs), tableData, "IDE");
                    }
                }
            }
        }

        /// <summary>
        /// Obtien la liste des tables avec les colonnes associées
        /// </summary>
        /// <param name="pVariables"></param>
        /// <param name="pAccVariables"></param>
        /// <param name="pByEventStep"></param>
        /// <returns></returns>
        public static IEnumerable<Pair<string, string>> GetTables(List<string> pVariables, List<AccVariable> pAccVariables, bool pByEventStep)
        {
            // La liste des tables et des colonnes
            IEnumerable<IGrouping<string, AccVariable>> lstGrpTable =
                (from table in
                     (from accVariableItem in pAccVariables
                      from currentVariableItem in pVariables
                      where accVariableItem.Regex.Match(currentVariableItem).Success
                      select accVariableItem).GroupBy(item => item.Table)
                 select table);

            IEnumerable<Pair<string, string>> lstTable =
                (from table in lstGrpTable
                 where pAccVariables.Exists(variable => variable.Table == table.Key && variable.IsTableWithIDE == pByEventStep)
                 select new Pair<string, string>
                     (table.Key, (from row in table select "tblData." + row.Column + " as " + row.IDAccVariable).Aggregate((i, j) => i + "," + j)));

            return lstTable;
        }


        /// <summary>
        /// Valorisation de la variable comptable {pAccVariable}
        /// </summary>
        /// <param name="pVariable"></param>
        /// <param name="pAccVariables"></param>
        /// <param name="pParameters"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        public static string ValuateAccVariable(DataDbTransaction pEfsTransaction, AccVariable pAccVariable, List<Pair<string, IDbDataParameter>> pParameters)
        {
            string ret = string.Empty;
            
            string sqlQuery = string.Empty;
            IDbDataParameter[] parameters = null;

            if (pAccVariable.IsTableWithIDE)
            {
                sqlQuery = AccVariableTools.SQL_Select_TableDataWithIDE
                    .Replace("{columnList}", "tblData." + pAccVariable.Column + " as " + pAccVariable.IDAccVariable)
                    .Replace("{table}", pAccVariable.Table);

                parameters = new IDbDataParameter[3] { pParameters.Find(match => match.First == "IdEAR").Second,
                    pParameters.Find(match => match.First == "EARTYPE").Second,
                    pParameters.Find(match => match.First == "IDEARTYPE").Second};
            }
            else
            {
                sqlQuery = AccVariableTools.SQL_Select_TableData
                    .Replace("{columnList}", "tblData." + pAccVariable.Column + " as " + pAccVariable.IDAccVariable)
                    .Replace("{table}", pAccVariable.Table);

                parameters = new IDbDataParameter[7] { pParameters.Find(match => match.First == "IdEAR").Second,
                    pParameters.Find(match => match.First == "InstrumentNo").Second,
                    pParameters.Find(match => match.First == "StreamNo").Second,
                    pParameters.Find(match => match.First == "NoZero").Second,
                    pParameters.Find(match => match.First == "EarCode").Second,
                    pParameters.Find(match => match.First == "AmountType").Second,
                    pParameters.Find(match => match.First == "EventClass").Second};
            }

            using (IDataReader drVariableVal = DataHelper.ExecuteReader(pEfsTransaction, CommandType.Text, sqlQuery, parameters))
            {
                if (drVariableVal.Read())
                    ret = (Convert.IsDBNull(drVariableVal[0]) ? string.Empty : drVariableVal[0].ToString());
            }
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AccLog
    {

        /// <summary>
        /// <para>- Ajoute l'exception <paramref name="pEx"/> dans le log (null accepté)</para>
        /// <para>- Ajoute l'exception <paramref name="pSpheresEx"/> dans le log</para>
        /// <para>- Génére un throw d'une nouvelle exception <see cref="SpheresException2"/> pour stopper le traitement</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// <param name="pSpheresEx"></param>        
        public static void FireException(Exception pEx, SpheresException2 pSpheresEx)
        {
            // S'il s'agit d'une exception NON SpheresException, alors l'écrire dans le Log
            // Sinon, elle est déjà écrite au moment de sa production
            // FI 20200623 [XXXXX] add test (null != pEx)
            
            if ((null != pEx) && false == (pEx is SpheresException2))
            {
                SpheresException2 exLog = SpheresExceptionParser.GetSpheresException(null, pEx);
                
                Logger.Log(new LoggerData(exLog));
            }
            
            Logger.Log(new LoggerData(pSpheresEx));

            // Pour arrêter le traitement
            throw new SpheresException2(new ProcessState(ProcessStateTools.StatusEnum.ERROR, Cst.ErrLevel.FAILURE));
        }
    }
}



namespace EfsML.Ear
{

    public static class EARTools
    {
        /// <summary>
        /// Retourne la date Début des flux à considérer par les EARs
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDt">date de traitement</param>
        /// <returns></returns>
        /// FI 20180907 [24160] Add Method
        public static DateTime LaunchProcessGetStartDate(string pCS, DateTime pDt)
        {
            string hist = SystemSettings.EARHist(CSTools.SetCacheOn(pCS));

            int mltp = Convert.ToInt32(hist.Split(',')[0]);
            string period = hist.Split(',')[1];


            DateTime ret;
            switch (period)
            {
                case "D":
                    ret = pDt.AddDays(-1 * mltp);
                    break;
                case "M":
                    ret = pDt.AddMonths(-1 * mltp);
                    break;
                case "Y":
                    ret = pDt.AddYears(-1 * mltp);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Period {0} is not implemented", period));
            }

            return ret;
        }

    }
}

using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;

namespace EFS.Spheres.Trial.Oracle
{
    public partial class OracleQueryPlan : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string cs = SessionTools.CS;

            /* Les dataset contiennent 2 table. Table[0] le résultat du select et  Table[1] le plan de la requête */

            // Sans DataParameters
            string query = @"EXPLAIN PLAN FOR
SELECT SUM(t.QTY) FROM TRADE t 
WHERE t.DTBUSINESS = TO_TIMESTAMP('20170616','yyyyMMdd')  AND t.EXTLLINK LIKE 'MX%';
SELECT PLAN_TABLE_OUTPUT 
    FROM TABLE(DBMS_XPLAN.DISPLAY)";
            _ = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, query);



            // Avec DataParameters
            string query2 = @"SELECT /*+ GATHER_PLAN_STATISTICS */
            SUM(t1.QTY) 
FROM TRADE t1 
WHERE t1.DTBUSINESS = @DT  AND t1.EXTLLINK LIKE 'MX%' 
GROUP BY t1.DTBUSINESS;
SELECT PLAN_TABLE_OUTPUT 
  FROM TABLE(DBMS_XPLAN.DISPLAY_CURSOR(NULL,NULL,'ADVANCED LAST +IOSTATS +MEMSTATS'))";

            // Date
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(cs, "DT", DbType.Date), new DateTime(2017, 06, 16));
            _ = DataHelper.ExecuteDataset(cs, CommandType.Text, query2, dp.GetArrayDbParameter());
        }
    }
}

using System;
//
using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{
    #region public class SQLCst
    // [23509] timezone
    public sealed class SQLCst
    {
        public SQLCst()
        { }
        public const int
            UT_CFICODE_LEN = 6,
            UT_COLOR_LEN = 32,
            UT_CULTURE_LEN = 10,
            UT_CURR_LEN = 3,
            UT_EVENT_LEN = 3,
            UT_EARCLASS_LEN = 16,
            UT_EARCODE_LEN = 16,
            UT_EARTYPE_LEN = 16,
            UT_AMOUNTTYPE_LEN = 16,
            UT_PRICE_DEC_LEN = 9,
            UT_ROLEGCONTRACT_LEN = 16,
            UT_ROLEGINSTR_LEN = 16,
            UT_ROLEGACTOR_LEN = 16,
            UT_ROLEGMARKET_LEN = 16,
            UT_ROLEGBOOK_LEN = 16,
            UT_ROLEACTOR_LEN = 16,
            UT_ROLEBOOK_LEN = 16,
            UT_EXTLINK_LEN = 128,
            UT_DESCRIPTION_LEN = 128,
            UT_APPNAME_LEN = 128,
            UT_APPVERSION_LEN = 128,
            UT_SESSIONID_LEN = 128,
            UT_ACCOUNTNUMBER_LEN = 64,
            UT_DISPLAYNAME_LEN = 64,
            UT_ENUMCHAR_MANDATORY_LEN = 1,
            UT_ENUMCHAR_OPTIONAL_LEN = 1,
            UT_ENUM_MANDATORY_LEN = 64,
            UT_ENUM_OPTIONAL_LEN = 64,
            UT_HOST_LEN = 64,
            UT_IDENTIFIER_LEN = 64,
            UT_LINKCODE_LEN = 64,
            UT_MARKET_LEN = 4,
            UT_MATURITY_LEN = 8,
            UT_LSTVALUE_LEN = 2000,
            UT_NOTE_LEN = 2000,
            UT_MESSAGE_LEN = 4000,
            UT_ROUNDDIR_LEN = 1,
            UT_ROWATTRIBUT_LEN = 1,
            UT_ROWVERSION_LEN = 15,
            UT_STATUS_LEN = 8,
            UT_OPERATOR_LEN = 16,
            UT_TABLENAME_LEN = 32,
            UT_TIME_MANDATORY_LEN = 5,
            UT_TIME_OPTIONAL_LEN = 5,
            UT_TIMEZONE_LEN = 64,
            UT_UNC_LEN = 256,
            UT_LABEL_LEN = 256,
            UT_ISINCODE_LEN = 12,
            UT_COUNTRY_LEN = 64;


        #region SQL ANSI String Constant
        
        public static string GetSQLSyntaxForJoin(string pTypeJoin)
        {
            switch (pTypeJoin)
            {
                case SQLCst.TypeJoinInnerJoin:
                    return SQLCst.INNERJOIN_DBO;
                case SQLCst.TypeJoinLeftOuterJoin:
                    return SQLCst.LEFTJOIN_DBO;
                case SQLCst.TypeJoinRightOuterJoin:
                    return SQLCst.RIGHTJOIN_DBO;
                case SQLCst.TypeJoinFullOuterJoin:
                    return SQLCst.FULLJOIN_DBO;
                case SQLCst.TypeJoinInnerOrLeftJoin:
                    //PL 20180620 PERF
                    string ret = @"<choose>" + Cst.CrLf;
                    ret += @"  <when test=""{ISWITHINCOMPLETETRADE}=1"">" + SQLCst.LEFTJOIN_DBO + @"</when>" + Cst.CrLf;
                    ret += @"  <otherwise>" + SQLCst.INNERJOIN_DBO + @"</otherwise>" + Cst.CrLf;
                    ret += @"</choose>";
                    return ret;
                case SQLCst.TypeJoinAlreadyExist:
                    return string.Empty;
                default:
                    return SQLCst.INNERJOIN_DBO;
            }
        }

        /// <summary>
        /// Constant for SQL query 
        /// </summary>
        public const string
            TypeJoinInnerJoin = "I",
            TypeJoinLeftOuterJoin = "L",
            TypeJoinRightOuterJoin = "R",
            TypeJoinFullOuterJoin = "F",
            //PL 20180620 PERF
            TypeJoinInnerOrLeftJoin = "j",  //Inner or Left outer according Dynamic Argument (DA) "ISWITHINCOMPLETETRADE" 
            TypeJoinAlreadyExist = "x";     //Inner or Left outer already exist in join

        
        public const string
            SQL_SUBQUERY = "~SQL SUBQUERY~",
            SEPARATOR_MULTISELECT = ";" + Cst.CrLf,
            SEPARATOR_MULTIDML = ";" + Cst.CrLf,

            SQL_NOANSI_COMMENT = "--",
            SQL_ANSI_COMMENT_BEGIN = "/*",
            SQL_ANSI_COMMENT_END = "*/",     
            SQL_ANSI = "/* SQL ANSI */ ",           //Used in comment, for bypass the TransformQuery() method.
            SQL_RDBMS = "/* SQL RDBMS */ ",         //Used in comment, for bypass the TransformQuery() method.
            SQL_PARAM = "/* SQL PARAM - {0} */ ",   //Used in comment, for display parameters
            NOLOCK = " (NOLOCK) ",
            
            DECLARE = "Declare ",
            BEGIN = "begin ",
            END = " end;",
            DBO = " dbo.",

            X_FROM = " from ",            
            X_INNER = " inner join ",
            X_LEFT = " left outer join ",
            X_RIGHT = " right outer join ",
            X_FULL = " full outer join ",
            X_INSERT = "insert into ",
            X_UPDATE = "update ",
            X_DELETE = "delete ", 

            FROM_DBO = " from dbo.",
            FROM_DUAL = " from DUAL",
            INNERJOIN_DBO = " inner join dbo.",
            LEFTJOIN_DBO = " left outer join dbo.",
            RIGHTJOIN_DBO = " right outer join dbo.",
            FULLJOIN_DBO = " full outer join dbo.",
            CROSSJOIN_DBO = " cross join dbo.",
            UNION = " union ",
            UNIONALL = " union all ",
            UNIONALL_DASH100 = " union all -- ----------------------------------------------------------------------------------------------------",
            UNIONALL_STAR100 = " union all -- ****************************************************************************************************",
            SELECT = "select ",
            SELECT_ALL = "select * ",
            SELECT_DISTINCT = "select distinct ",
            SELECT_TOP = "select top ",
            SELECT_TOP1 = "select top 1 ",
            SELECT_DISTINCT_TOP = "select distinct top ",
            SELECT_DISTINCT_TOP1 = "select distinct top 1 ",
            DISTINCT = " distinct ",
            
            UPDATE_DBO = "update dbo.",
            //Warning: 2 espaces sont nécessaires à TransformQuery() entre "delete" et "from"
            DELETE_DBO = "delete from dbo.",
            DELETE_TOP1_DBO = "delete top(1) from dbo.",
            INSERT_INTO_DBO = "insert into dbo.",
            VALUES = " values ",
            SET = " set ",
            ON = " on ",
            WHERE = " where ",
            GROUPBY = " group by ",
            HAVING = " having ",
            ORDERBY = " order by ",
            AS = " as ",
            ASC = " asc ",
            DESC = " desc ",
            DIFFERENT = " <> ",
            NOTEQUAL = " != ",
            NOT = " not ",
            AND = " and ",
            BETWEEN = " between ",
            LIKE = " like ",
            NOT_LIKE = " not like ",
            OR = " or ",
            IN = " in ",
            NOT_IN = " not in ",
            IS_NOT_NULL = " is not null ",
            IS_NULL = " is null ",
            EXISTS = " exists ",
            EXISTS_SELECT = " exists (select 1 from dbo.",
            NOT_EXISTS = " not exists ",
            NOT_EXISTS_SELECT = " not exists (select 1 from dbo.",
            MIN = " min",
            MAX = " max",
            NULL = "null",
            COUNT = " count ",
            COUNT_ALL = " count(*) ",
            COUNT_1 = " count(1) ",
            INF = " < ",
            INFEQUAL = " <= ",
            SUP = " > ",
            SUPEQUAL = " >= ",
            TBLMAIN = "tblmain", // Used as default alias of table for the main table.
            CASE = " case ",  // RD 20060421
            CASE_WHEN = " when ",
            CASE_THEN = " then ",
            CASE_ELSE = " else ",
            CASE_END = " end ",
            CASE_WHEN_THEN_ELSE_END = " case when {0} then {1} else {2} end ",
            WITH = " with ",  // 20081021 RD - Used for CTE, only for MSSQL Server
            START_WITH = " start with ", //20081022 RD - Used for recursive query, only for Oracle
            CONNECT_BY_NOCYCLE = " connect by nocycle ",
            PRIOR = " prior ",
            EXCEPT = " except ",
            INTERSECT = " intersect ";
        #endregion SQL ANSI String Constant
    }
    #endregion
}

#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;

using EfsML.Business;
#endregion Using Directives

namespace EfsML.DynamicData
{
    /// <summary>
    /// Représente une expression SQL 
    /// </summary>
    [XmlRoot(ElementName = "SQL", IsNullable = true)]
    public class DataSQL
    {
        #region Members

        /// <summary>
        /// Valeur POSSIBLE SELECT,TEXT,.... ou PROCEDURE
        /// <para>Devrait être CommandType </para>
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public string command;
        
        /// <summary>
        /// PROCEDURE name
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public string name;

        /// <summary>
        /// Column for Result in command SELECT or PROCEDURE
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public string result;
        /// <summary>
        /// Query when command is SELECT or PROCEDURE
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string Value;

        /// <summary>
        /// SQL Parameters
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("Param", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ParamData[] param;
        
        [System.Xml.Serialization.XmlIgnore()]
        public bool isCacheSpecified;

        /// <summary>
        /// Obtient ou définit l'usage du cache SQL
        /// </summary>
        /// FI 20120126 usage de cache
        [System.Xml.Serialization.XmlAttribute("cache", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isCache;

        /// <summary>
        /// Obtient ou définit le nbre d'usage possible d'une donnée en cache
        /// </summary>
        /// PL 20180903 New feature (for Trade Importation)
        [System.Xml.Serialization.XmlAttribute("cacheinterval", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int cacheIntervalDataEnabled;
        #endregion Members

        #region accessor
        /// <summary>
        /// Obtient true si la commande SQL est une stored procedure
        /// </summary>
        public bool IsStoredProcedure
        {
            get
            {
                bool ret = false;
                if (StrFunc.IsFilled(command))
                    ret = (command.ToUpper() == "PROCEDURE");
                return ret;
            }
        }
        /// <summary>
        ///  Obtient true si la commande SQL est une commandText  
        /// </summary>
        public bool IsSelect
        {
            get
            {
                return (command.ToUpper() == "SELECT") || StrFunc.IsEmpty(command);
            }
        }
        #endregion accessor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="opParam"></param>
        /// <returns></returns>
        public bool GetParam(string pName, ref ParamData opParam)
        {
            opParam = GetParam(pName);
            //
            return (opParam != null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        public ParamData GetParam(string pName)
        {
            ParamData retParam = null;
            //
            if (ArrFunc.IsFilled(param))
            {
                string name = pName.ToLower();
                for (int i = 0; i < ArrFunc.Count(param); i++)
                {
                    if (param[i].name.ToLower() == name)
                        retParam = param[i];
                }
                if (retParam == null)
                {
                    name = "p" + pName.ToLower();
                    for (int i = 0; i < ArrFunc.Count(param); i++)
                    {
                        if (param[i].name.ToLower() == name)
                            retParam = param[i];
                    }
                }
            }
            //
            return retParam;
        }

        /// <summary>
        /// Exécute la requête et retourne le résultat au format ISO
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pdbTransaction"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        // EG 20180423 Analyse du code Correction [CA2200]
        public string Exec(string pCs, IDbTransaction pdbTransaction)
        {
            string retValue = null;
            IDataReader drValue = null;
            DataTable dtValue = null;
            bool isOracle = DataHelper.IsDbOracle(pCs);

            #region PL - Debug - Variables et code associé sont destinés à tracer, pour debug, query et résultat lorsque:
            //------------------------------------------------------------------------------------------------------------------------------------------
            //- la query contient le mot clé SPHERES_DEBUG (ex. "/* SPHERES_DEBUG */ select COL from TABLE where ...")
            //- la trace es active et la query contient la colonne BOOK_IDENTIFIER (ex. query d'identification du BOOK lors de l'importation des Trades)
            //------------------------------------------------------------------------------------------------------------------------------------------
            //Code mis en commentaire. Ne pas supprimer !
            //------------------------------------------------------------------------------------------------------------------------------------------
            //bool isTrace_Keyword_SPHERES_DEBUG = Value.Contains("SPHERES_DEBUG");
            //bool isTrace_Column_BOOK_IDENTIFIER = (_IsTraceDebug && Value.Contains("BOOK_IDENTIFIER"));
            //bool isTrace_Detail = isTrace_Keyword_SPHERES_DEBUG || isTrace_Column_BOOK_IDENTIFIER;
            #endregion PL - Debug

            try
            {
                string selectQuery = Value;

                #region Parameters
                DataParameters dataParameters = new DataParameters();
                if (ArrFunc.IsFilled(param))
                {
                    for (int i = 0; i < param.Length; i++)
                    {
                        dataParameters.Add(param[i].GetDataParameter(pCs, pdbTransaction, IsStoredProcedure ? CommandType.StoredProcedure : CommandType.Text));
                    }
                }
                #endregion 

                if (IsSelect)
                {
                    #region Cache
                    bool isUseCache = false;
                    CSManager csManager = new CSManager(pCs);
                    
                    if (null != csManager.IsUseCache)
                    {
                        //Si pCs contient une instruction sur le cache
                        isUseCache = BoolFunc.IsTrue(csManager.IsUseCache);
                        if (isCacheSpecified && isUseCache)
                            isUseCache &= isCache;
                    }
                    else
                    {
                        isUseCache = isCache;
                    }
                    isUseCache = (isUseCache && (null == pdbTransaction));
                    #endregion

                    QueryParameters qryParameters = new QueryParameters(pCs, selectQuery, dataParameters);

                    #region PL - Debug
                    //if (isTrace_Detail)//LOGGLOP
                    //{
                    //    if (null != _Log)
                    //    {
                    //        string[] msgArg = new string[] { "LOG-00003" + "Cache: " + isUseCache.ToString() + Cst.CrLf + "Query (parameters): " + qryParameters.Query };
                    //        ProcessLogInfo info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                    //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                    //        msgArg = new string[] { "LOG-00003" + "Cache: " + isUseCache.ToString() + Cst.CrLf + "Query  (values): " + qryParameters.queryReplaceParameters };
                    //        info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                    //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);
                    //    }

                    //    // PM 20200102 [XXXXX] New Log
                    //    Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                    //        new LogParam("Cache: " + isUseCache.ToString() + Cst.CrLf + "Query (parameters): " + qryParameters.Query)));

                    //    // PM 20200102 [XXXXX] New Log
                    //    Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                    //        new LogParam("Cache: " + isUseCache.ToString() + Cst.CrLf + "Query  (values): " + qryParameters.queryReplaceParameters)));
                    //}
                    #endregion PL - Debug

                    if (isUseCache)
                    {
                        #region Cache ON
                        dtValue = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCs, cacheIntervalDataEnabled), qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                        #region PL - Debug
                        ////LOGGLOP
                        //if (isTrace_Detail
                        //    && ((null == dtValue) || (ArrFunc.IsEmpty(dtValue.Rows) || (dtValue.Rows.Count == 1))))
                        //{
                        //    if (null != _Log)
                        //    {
                        //        string[] msgArg = new string[] { "LOG-00003" + "Test: With NO CACHE" };
                        //        ProcessLogInfo info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                        //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                        //        // PM 20200102 [XXXXX] New Log
                        //        Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                        //            new LogParam("Test: With NO CACHE")));

                        //        string tmp_query = qryParameters.Query.Replace(SQLCst.FROM_DUAL, SQLCst.FROM_DUAL + " where (1=2)");
                        //        drValue = DataHelper.ExecuteReader(pCs, CommandType.Text, tmp_query, qryParameters.Parameters.GetArrayDbParameter());

                        //        bool isSuccessWithNoCache = ((null != drValue) && ((System.Data.Common.DbDataReader)drValue).HasRows);
                        //        msgArg = new string[] { "LOG-00003" + "Result: " + (isSuccessWithNoCache ? "SUCCESS" : "NOT FOUND") + " with NO CACHE" };
                        //        info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                        //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                        //        // PM 20200102 [XXXXX] New Log
                        //        Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                        //            new LogParam("Result: " + (isSuccessWithNoCache ? "SUCCESS" : "NOT FOUND") + " with NO CACHE")));
                        //    }
                        //}
                        #endregion PL - Debug
                        #endregion
                    }
                    else 
                    {
                        #region Cache OFF
                        if (null != pdbTransaction)
                        {
                            drValue = DataHelper.ExecuteReader(pdbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                            #region PL - Debug
                            ////LOGGLOP
                            //if (isTrace_Detail
                            //    && ((null == drValue) || (!((System.Data.Common.DbDataReader)drValue).HasRows)))
                            //{
                            //    if (null != _Log)
                            //    {
                            //        string[] msgArg = new string[] { "LOG-00003" + "Test: With CACHE" };
                            //        ProcessLogInfo info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                            //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                            //        // PM 20200102 [XXXXX] New Log
                            //        Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                            //            new LogParam("Test: With CACHE")));

                            //        dtValue = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(pCs), qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                            //        bool isSuccessWithNoCache = ((null != dtValue) && ArrFunc.IsFilled(dtValue.Rows) && (dtValue.Rows.Count == 2));
                            //        msgArg = new string[] { "LOG-00003" + "Result: " + (isSuccessWithNoCache ? "SUCCESS" : "NOT FOUND") + " with CACHE" };
                            //        info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                            //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                            //        // PM 20200102 [XXXXX] New Log
                            //        Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                            //            new LogParam("Result: " + (isSuccessWithNoCache ? "SUCCESS" : "NOT FOUND") + " with CACHE")));
                            //    }
                            //}
                            #endregion PL - Debug
                        }
                        else
                        {
                            drValue = DataHelper.ExecuteReader(pCs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }
                        #endregion
                    }
                }
                //GLOP FI IO Voir pour lancer une procedure avec des paramètres (notamment Oracle)
                else if (IsStoredProcedure)
                {
                    #region StoredProcedure
                    if (isOracle)
                    {
                        DataParameter paramCursor = new DataParameter();
                        paramCursor.InitializeOracleCursor("curs", ParameterDirection.Output);
                        dataParameters.Add(paramCursor);
                    }

                    drValue = DataHelper.ExecuteReader(pCs, pdbTransaction, CommandType.StoredProcedure, name, dataParameters.GetArrayDbParameter());
                    #endregion
                }
                else // Normalement on ne passe pas ici
                {
                    #region Other
                    DataHelper.ExecuteNonQuery(pCs, pdbTransaction, CommandType.Text, selectQuery, dataParameters.GetArrayDbParameter());
                    #endregion
                }

                if ((null != drValue) && drValue.Read())
                {
                    #region Cache OFF --> DataReader
                    if (StrFunc.IsFilled(result))
                    {
                        Type columnType = typeof(System.String);
                        if (IsExistColumn(drValue.GetSchemaTable(), result, ref columnType))
                        {
                            retValue = (Convert.IsDBNull(drValue[result]) ? null : ObjFunc.FmtToISo(drValue[result], TypeData.GetTypeFromSystemType(columnType)));
                        }
                    }
                    else
                    {
                        retValue = (Convert.IsDBNull(drValue.GetValue(0)) ? null : ObjFunc.FmtToISo(drValue.GetValue(0), TypeData.GetTypeFromSystemType(drValue.GetValue(0).GetType())));
                    }

                    #region PL - Debug
                    //if (isTrace_Detail)//LOGGLOP
                    //{
                    //    if (null != _Log)
                    //    {
                    //        string data = string.Empty;
                    //        int line = 1;
                    //        data += "Line " + line.ToString() + ": ";
                    //        for (int i = 0; i < drValue.FieldCount; i++)
                    //        {
                    //            data += (Convert.IsDBNull(drValue.GetValue(i)) ? "null" : drValue.GetValue(i).ToString()) + ";";
                    //        }
                    //        while (drValue.Read())
                    //        {
                    //            line++;
                    //            data += "Line " + line.ToString() + ": ";
                    //            for (int i = 0; i < drValue.FieldCount; i++)
                    //            {
                    //                data += (Convert.IsDBNull(drValue.GetValue(i)) ? "null" : drValue.GetValue(i).ToString()) + ";";
                    //            }
                    //        }
                    //        string[] msgArg = new string[] { "LOG-00003" + "Data: " + Cst.CrLf + data };
                    //        ProcessLogInfo info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                    //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                    //        // PM 20200102 [XXXXX] New Log
                    //        Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                    //            new LogParam("Data: " + Cst.CrLf + data)));
                    //    }
                    //}
                    #endregion PL - Debug
                    #endregion 
                }
                else if ((null != dtValue) && (ArrFunc.IsFilled(dtValue.Rows)))
                {
                    #region Cache ON --> DataTable
                    #region PL - Debug
                    //if (isTrace_Detail)//LOGGLOP
                    //{
                    //    if (null != _Log)
                    //    {
                    //        string data = string.Empty;
                    //        int line = 0;
                    //        foreach (DataRow dr in dtValue.Rows)
                    //        {
                    //            line++;
                    //            data += "Line " + line.ToString() + ": ";
                    //            for (int i = 0; i < dr.ItemArray.GetLength(0); i++)
                    //            {
                    //                data += (Convert.IsDBNull(dr[i]) ? "null" : dr[i].ToString()) + ";";
                    //            }
                    //            data += Cst.CrLf;
                    //        }
                    //        string[] msgArg = new string[] { "LOG-00003" + "Data: " + Cst.CrLf + data };
                    //        ProcessLogInfo info = new ProcessLogInfo(ProcessStateTools.StatusNoneEnum, 0, string.Empty, msgArg);
                    //        _Log.Invoke(EFS.Common.Log.LogLevel.LEVEL3, info);

                    //        // PM 20200102 [XXXXX] New Log
                    //        Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 3), 0,
                    //            new LogParam("Data: " + Cst.CrLf + data)));
                    //    }
                    //}
                    #endregion PL - Debug

                    if (StrFunc.IsFilled(result))
                    {
                        if (dtValue.Columns.Contains(result))
                        {
                            retValue = (Convert.IsDBNull(dtValue.Rows[0][result]) ? null : ObjFunc.FmtToISo(dtValue.Rows[0][result], TypeData.GetTypeFromSystemType(dtValue.Columns[result].DataType)));
                        }
                    }
                    else
                    {
                        retValue = (Convert.IsDBNull(dtValue.Rows[0][0]) ? null : ObjFunc.FmtToISo(dtValue.Rows[0][0], TypeData.GetTypeFromSystemType(dtValue.Columns[0].DataType)));
                    }
                    #endregion 
                }


                retValue = (StrFunc.IsFilled(retValue) ? retValue.Trim() : retValue);
            }
            catch 
            {
               throw;
            }
            finally
            {
                if (null != drValue)
                {
                    drValue.Close();
                    drValue.Dispose();
                }
            }
            
            return retValue;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSchemaTable"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pColumnType"></param>
        /// <returns></returns>
        private static bool IsExistColumn(DataTable pSchemaTable, string pColumnName, ref Type pColumnType)
        {
            bool ret = false;
            if (StrFunc.IsFilled(pColumnName))
            {
                foreach (DataRow myRow in pSchemaTable.Rows)
                {
                    // BUG Oracle : the Oracle DataTable have all the DataColumn name returned using upper case;
                    //         when the parameter is passed using another character case strategy, then the comparison failed.
                    //         Ex: "XY" == "xY" // false 
                    //if ( myRow["ColumnName"].ToString() == pColumnName)
                    if (String.Compare((string)myRow["ColumnName"], pColumnName, true) == 0)
                    {
                        ret = true;
                        pColumnType = (Type)myRow["DataType"];
                        break;
                    }
                }
            }
            return ret;
        }
    }
}

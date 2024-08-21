using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

namespace EFS.DALSQLServer
{
    // EG 20181010 PERF Add GetSvrInfoConnection
    public sealed class DataAccess : DataAccessBase
    {
        #region properties
        #region public override GetVarPrefix
        public override string GetVarPrefix
        {
            get
            {
                return "@";
            }
        }
        #endregion GetVarPrefix
        #region public override GetDbSvrType
        public override DbSvrType GetDbSvrType
        {
            get
            {
                return DbSvrType.dbSQL;
            }
        }

        #endregion
        #region public override GetConcatOperator
        public override string GetConcatOperator
        {
            get
            {
                return "+";
            }
        }
        #endregion GetConcatOperator
        #endregion

        /// <summary>
        /// Récupère les information Serveurs SQLServer
        /// </summary>
        /// <returns></returns>
        public override SvrInfoConnection GetSvrInfoConnection(string pCS)
        {

            SvrInfoConnection svrInfo;
            using (SqlConnection cn = OpenSqlConnection(pCS))
            {
                svrInfo = new SvrInfoConnection
                {
                    ServerType = DbSvrType.dbSQL,
                    Database = cn.Database,
                    DataSource = cn.DataSource
                };
                string[] version = cn.ServerVersion.Split(".".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries);
                svrInfo.ServerVersion = String.Format("{0}.{1}", version);
            };

            return svrInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GetReferencedAssemblyInfos()
        {
            AssemblyName assembly = typeof(SqlConnection).Assembly.GetName();
            return $"Assembly identity={assembly.Name}, Version={assembly.Version}";
            
        }
        


        #region public override  CheckConnection
        public override void CheckConnection(string pConnectionString)
        {
            CheckSqlConnection(pConnectionString);
        }
        #endregion
        #region public GetServerVersion
        /// <summary>
        /// Retourne la version du serveur de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public override string GetServerVersion(string pCS)
        {
            string ret = null;

            using (SqlConnection testConn = new SqlConnection(pCS))
            {
                testConn.Open();
                ret = "Microsoft SQL Server " + testConn.ServerVersion;
                testConn.Close();
            }
            return ret;
        }
        #endregion

        #region public override OpenConnection
        public override IDbConnection OpenConnection(string pConnectionString)
        {
            return OpenSqlConnection(pConnectionString);
        }
        #endregion

        #region public override BeginTransaction
        public override IDbTransaction BeginTran(string pConnectionString, out IDbConnection opConnection)
        {
            SqlConnection cn = OpenSqlConnection(pConnectionString);
            SqlTransaction sqlTransaction = cn.BeginTransaction();
            opConnection = cn;
            //
            return sqlTransaction;
        }
        public override IDbTransaction BeginTran(string pConnectionString, IsolationLevel pIsolevel)
        {
            SqlConnection cn = OpenSqlConnection(pConnectionString);
            SqlTransaction sqlTransaction = cn.BeginTransaction(pIsolevel);
            //
            return sqlTransaction;
        }
        public override IDbTransaction BeginTran(IDbConnection pConnection)
        {
            SqlTransaction sqlTransaction;
            //
            if (pConnection.State != ConnectionState.Open)
                pConnection.Open();
            sqlTransaction = (SqlTransaction)pConnection.BeginTransaction();
            //
            return sqlTransaction;
        }
        #endregion
        
        #region public override AnalyseSQLException
        /// <summary>
        /// Retourne true si l'erreur est de type SQL (SqlException)
        /// </summary>
        /// <param name="pException"></param>
        /// <param name="opError"></param>
        /// <param name="opMessage"></param>
        /// <returns></returns>
        // RD 20200114 [25131] Add errorCode 2627: Violation in unique constraint
        public override bool AnalyseSQLException(Exception pException, out string opErrorMessage, out SQLErrorEnum opErrorEnum)
        {
            opErrorMessage = string.Empty;
            bool ret;
            if (IsSQLException(pException))
            {
                #region SqlException
                ret = true;
                SqlException sqlException;
                sqlException = (SqlException)pException;

                int errorCode = sqlException.Number;
                opErrorMessage += "SQL:";
                opErrorMessage += errorCode.ToString();
                switch (errorCode)
                {
                    case 515:
                        {
                            opErrorEnum = SQLErrorEnum.NullValue; //Cannot insert the value NULL into column '%.*ls'; column does not allow nulls. INSERT fails.                            
                            //20081110 PL Ajout de la ligne suivante afin d'afficher l'erreur complète et disposer ainsi du nom de la colonne concernée.
                            opErrorMessage += " [" + sqlException.Message + "]";
                            break;
                        }
                    case 547:
                        {
                            opErrorEnum = SQLErrorEnum.Integrity; //Update statement conflicted with TABLE FOREIGN KEY constraint '%.*ls'. The conflict occurred in database '%.*ls', table '%.*ls'.
                            opErrorMessage += " [" + sqlException.Message + "]";
                            break;
                        }
                    // RD 20200114 [25131] Add 2627
                    // 2601 - Violation in unique index      : Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'.
                    // 2627 - Violation in unique constraint : Cannot insert duplicate key in object '%.*ls'. The duplicate key value is %.*ls.
                    case 2601:
                    case 2627:
                        {
                            opErrorEnum = SQLErrorEnum.DuplicateKey;
                            opErrorMessage += " [" + sqlException.Message + "]";
                            break;
                        }
                    case 1205:
                        {
                            opErrorEnum = SQLErrorEnum.DeadLock; //Deadlock detected                            
                            opErrorMessage += " [" + sqlException.Message + "]";
                            break;
                        }
                    /*case 2731:
                    {
                        retvalue = "SQLError_InvalidWidth";      //Column '%.*ls' has invalid width: %d.
                        break;
                    }
                    case 2750:
                    {
                        retvalue = "SQLError_DECPrecision";      //Column or parameter #%d: Specified column precision %d is greater than the maximum precision of %d.
                        break;
                    }
                    case 2751:
                    {
                        retvalue = "SQLError_DECScale";          //Column or parameter #%d: Specified column scale %d is greater than the specified precision of %d.
                        break;
                    }*/

                    default:
                        {
                            //20090917 PL Add Management of TimeOut error
                            if (errorCode == -2)
                            {
                                //Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.
                                const string SQLSERVER_TIMEOUT_EN = "Timeout expired.";
                                //Expiration du délai d'attente. Le délai d'attente s'est écoulé avant la fin de l'opération ou le serveur ne répond pas.
                                const string SQLSERVER_TIMEOUT_FR = "Expiration du délai d'attente.";
                                // FI 20211206 [XXXXX] Add SQLSERVER_TIMEOUT_FR2 (mesage constaté sur SVR-DB01\SQL2017)
                                const string SQLSERVER_TIMEOUT_FR2 = "délai d'exécution a expiré.";
                                //
                                const string SQLSERVER_TIMEOUT_IT = "Timeout";
                                if (sqlException.Message.Contains(SQLSERVER_TIMEOUT_EN)
                                    || sqlException.Message.Contains(SQLSERVER_TIMEOUT_FR) || sqlException.Message.Contains(SQLSERVER_TIMEOUT_FR2)
                                    || sqlException.Message.Contains(SQLSERVER_TIMEOUT_IT))
                                {
                                    opErrorEnum = SQLErrorEnum.Timeout;
                                    opErrorMessage += " [" + sqlException.Message + "]";
                                    break;
                                }
                            }

                            opErrorEnum = SQLErrorEnum.Unknown;
                            opErrorMessage += " [" + sqlException.Message + "]";
                            break;
                        }
                }
                #endregion
            }
            else
            {
                ret = base.AnalyseSQLException(pException, out opErrorMessage, out opErrorEnum);
            }
            return ret;
        }
        #endregion
        #region public override IsSQLException
        public override bool IsSQLException(Exception pException)
        {
            return (pException.GetType() == typeof(SqlException));
        }
        public override bool IsDuplicateKeyError(Exception pException)
        {
            bool ret = false;
            //
            if (IsSQLException(pException))
            {
                SqlException sqlEx = (SqlException)pException;
                //
                if (sqlEx.Number == 2601)
                    ret = true;
            }
            //
            return ret;
        }
        #endregion
        
        #region ExecuteNonQuery
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteNonQuery(connectionString, commandType, commandText);
        }
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteNonQuery(connectionString, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteNonQuery(connectionString, spName, parameterValues);
        }
        //
        public override int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteNonQuery((SqlConnection)connection, commandType, commandText);
        }
        public override int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteNonQuery((SqlConnection)connection, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteNonQuery((SqlConnection)connection, spName, parameterValues);
        }
        //
        public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, commandType, commandText);
        }
        public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, string spheresInfo, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, spName, parameterValues);
        }
        #endregion
        #region ExecuteDataAdapter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandText"></param>
        /// <param name="datatable"></param>
        /// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
        /// <returns></returns>
        /// FI 20220908 [XXXXX] Add updateBatchSize
        public override int ExecuteDataAdapter(string connectionString, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            return SqlHelper.ExecuteDataAdapter(connectionString, commandText, datatable, updateBatchSize);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="commandText"></param>
        /// <param name="datatable"></param>
        /// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
        /// <returns></returns>
        /// FI 20220908 [XXXXX] Add updateBatchSize
        public override int ExecuteDataAdapter(IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            return SqlHelper.ExecuteDataAdapter(pDbTransaction, commandText, datatable, updateBatchSize);
        }
        #endregion ExecuteDataAdapter
        #region ExecuteDataSet
        public override DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteDataset(connectionString, commandType, commandText);
        }
        public override DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteDataset(connectionString, commandType, commandText, commandParameters);
        }
        public override DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteDataset(connectionString, spName, parameterValues);
        }
        //
        public override DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteDataset((SqlConnection)connection, commandType, commandText);
        }
        public override DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteDataset((SqlConnection)connection, commandType, commandText, commandParameters);
        }
        public override DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteDataset((SqlConnection)connection, spName, parameterValues);
        }
        //
        public override DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteDataset((SqlTransaction)transaction, commandType, commandText);
        }
        public override DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteDataset((SqlTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteDataset((SqlTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteDataSet
        #region ExecuteReader
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteReader(connectionString, commandType, commandText);
        }
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteReader(connectionString, commandType, commandText, commandParameters);
        }
        
        public override IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteReader(connectionString, spName, parameterValues);
        }
        //
        public override IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteReader((SqlConnection)connection, commandType, commandText);
        }
        public override IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteReader((SqlConnection)connection, commandType, commandText, commandParameters);
        }
        public override IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteReader((SqlConnection)connection, spName, parameterValues);
        }
        //
        public override IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteReader((SqlTransaction)transaction, commandType, commandText);
        }
        public override IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteReader((SqlTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteReader((SqlTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteReader
        #region ExecuteScalar
        public override object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteScalar(connectionString, commandType, commandText);
        }
        public override object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteScalar(connectionString, commandType, commandText, commandParameters);
        }
        public override object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteScalar(connectionString, spName, parameterValues);
        }
        //
        public override object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteScalar((SqlConnection)connection, commandType, commandText);
        }
        public override object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteScalar((SqlConnection)connection, commandType, commandText, commandParameters);
        }
        public override object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteScalar((SqlConnection)connection, spName, parameterValues);
        }
        //
        public override object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteScalar((SqlTransaction)transaction, commandType, commandText);
        }
        public override object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteScalar((SqlTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteScalar((SqlTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteScalar
        #region ExecuteXmlReader
        public override XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteXmlReader((SqlConnection)connection, commandType, commandText);
        }
        public override XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteXmlReader((SqlConnection)connection, commandType, commandText, commandParameters);
        }
        public override XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteXmlReader((SqlConnection)connection, spName, parameterValues);
        }
        //
        public override XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return SqlHelper.ExecuteXmlReader((SqlTransaction)transaction, commandType, commandText);
        }
        public override XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return SqlHelper.ExecuteXmlReader((SqlTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return SqlHelper.ExecuteXmlReader((SqlTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteXmlReader

        #region GetDataAdapter
        public override DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            string pTableName, ref DataSet pDs, out IDbCommand opCommand)
        {
            return SqlHelper.GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, pTableName, ref pDs, out opCommand);
        }
        public override DataAdapter GetDataAdapter (IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
                    out DataSet opFilledData, out IDbCommand opCommand)
        {
            return SqlHelper.GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, out opFilledData, out opCommand);
        }
        #endregion GetDataAdapter
        #region SetUpdatedEventHandler
        public override void SetUpdatedEventHandler(DataAdapter pDa, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            SqlHelper.SetUpdatedEventHandler(pDa as SqlDataAdapter, pRowUpdating, pRowUpdated);
        }
        #endregion SetUpdatedEventHandler
        #region Update
        public override int Update(DataAdapter pDa, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            return SqlHelper.Update(pDa as SqlDataAdapter, pDataTable, pRowUpdating, pRowUpdated);
        }
        #endregion Update
        #region Fill
        public override int Fill(DataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            return SqlHelper.Fill(pDa as SqlDataAdapter, pDataSet, pTableName);
        }
        #endregion Fill
        #region SetSelectCommandText
        public override void SetSelectCommandText(DataAdapter pDa, string pQuery)
        {
            SqlHelper.SetSelectCommandText(pDa as SqlDataAdapter, pQuery);
        }
        #endregion SetSelectCommandText
        
        #region public override NewParameter
        //public override IDbDataParameter NewParameter()
        public override IDbDataParameter NewParameter(DbType dbType, ref bool isSpecialDbType)
        {
            SqlParameter sqlParameter = new SqlParameter();
            return sqlParameter;
        }
        public override IDbDataParameter NewParameter(CommandType pCommandType, ref string parameterName, ref DbType dbType, ref bool isSpecialDbType)
        {
            string vp = GetVarPrefix;
            //
            if (!parameterName.StartsWith(vp))
                parameterName = vp + parameterName;
            //
            SqlParameter dataParameter = new SqlParameter();
            //
            return dataParameter;
        }
        public override IDbDataParameter NewCursorParameter(string parameterName)
        {
            throw (new ArgumentException("SQLServer does not support Cursor parameter", "parameterType"));
        }
        #endregion
        
        #region private CheckSqlConnection
        private void CheckSqlConnection(string pConnectionString)
        {
            using (SqlConnection testConn = new SqlConnection(pConnectionString))
            {
                testConn.Open();
                testConn.Close();
            }
        }
        #endregion
        #region private OpenSqlConnection
        private SqlConnection OpenSqlConnection(string pConnectionString)
        {
            SqlConnection cn = new SqlConnection(pConnectionString);
            cn.Open();
            return cn;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDestinationTable"></param>
        /// <param name="pDt"></param>
        // PM 20200102 [XXXXX] New
        public override void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt)
        {
            SqlHelper.BulkWriteToServer(pConnectionString, pDestinationTable, pDt);
        }
    }
}

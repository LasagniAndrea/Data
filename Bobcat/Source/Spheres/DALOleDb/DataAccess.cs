using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;

namespace EFS.DALOleDb
{

    public sealed class DataAccess : DataAccessBase
    {
        #region property
        #region public GetVarPrefix
        public override string GetVarPrefix
        {
            get
            {
                return "@";
            }
        }
        #endregion GetVarPrefix
        #region public GetDbSvrType
        public override DbSvrType GetDbSvrType
        {
            get
            {
                return DbSvrType.dbOLEDB;
            }
        }

        #endregion
        #endregion
        /// <summary>
        /// Récupère les information Serveurs OLE
        /// </summary>
        /// <returns></returns>
        public override SvrInfoConnection GetSvrInfoConnection(string pCS)
        {
            SvrInfoConnection svrInfo;
            using (OleDbConnection cn = OpenOleDbConnection(pCS))
            {
                svrInfo = new SvrInfoConnection
                {
                    ServerType = DbSvrType.dbOLEDB,
                    Database = cn.Database,
                    DataSource = cn.DataSource
                };

                string[] version = cn.ServerVersion.Split(".".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries);
                svrInfo.ServerVersion = String.Format("{0}.{1}", version);
            }
            return svrInfo;
        }


        /// <summary>
        /// 
        /// </summary>
        public override string GetReferencedAssemblyInfos()
        {
            AssemblyName assembly = typeof(OleDbConnection).Assembly.GetName();
            return $"Assembly identity={assembly.Name}, Version={assembly.Version}";
        }

        

        /// <summary>
        /// Vérifie que la chaîne de connexion est valide
        /// </summary>
        /// <param name="pCS"></param>
        public override void CheckConnection(string pCS)
        {
            CheckSqlConnection(pCS);
        }
        
        
        #region OpenConnection
        public override IDbConnection OpenConnection(string pConnectionString)
        {
            return OpenOleDbConnection(pConnectionString);
        }
        private OleDbConnection OpenOleDbConnection(string pConnectionString)
        {
            OleDbConnection cn = new OleDbConnection(pConnectionString);
            cn.Open();
            return cn;
        }
        #endregion
        #region BeginTransaction
        public override IDbTransaction BeginTran(string pConnectionString, out IDbConnection opConnection)
        {
            OleDbConnection cn;
            OleDbTransaction oleDbTransaction;
            //
            cn = OpenOleDbConnection(pConnectionString);
            oleDbTransaction = cn.BeginTransaction();
            opConnection = cn;
            //
            return oleDbTransaction;
        }
        public override IDbTransaction BeginTran(string pConnectionString, IsolationLevel pIsolevel)
        {
            OleDbConnection cn;
            OleDbTransaction oleDbTransaction;
            //
            cn = OpenOleDbConnection(pConnectionString);
            oleDbTransaction = cn.BeginTransaction(pIsolevel);
            //
            return oleDbTransaction;
        }
        public override IDbTransaction BeginTran(IDbConnection pConnection)
        {
            OleDbTransaction oleDbTransaction;
            //
            if (pConnection.State != ConnectionState.Open)
                pConnection.Open();
            oleDbTransaction = (OleDbTransaction)pConnection.BeginTransaction();
            //
            return oleDbTransaction;
        }
        #endregion

        #region AnalyseSQLException
        /// <summary>
        /// Retourne true si l'exeption est de type SQL (OleDbException)
        /// </summary>
        /// <param name="pException"></param>
        /// <param name="opErrorMessage"></param>
        /// <param name="opErrorEnum"></param>
        /// <returns></returns>
        public override bool AnalyseSQLException(Exception pException,out string opErrorMessage, out SQLErrorEnum opErrorEnum)
        {
            opErrorMessage = string.Empty;
            bool ret;
            if (IsSQLException(pException))
            {
                #region OleDbException
                ret = true;
                OleDbException oleDbException;
                oleDbException = (OleDbException)pException;

                int errorCode = oleDbException.ErrorCode;
                opErrorEnum = SQLErrorEnum.OleDb;
                opErrorMessage += "ErrorCode: " + errorCode.ToString() + Cst.CrLf2;
                //
                for (int i = 0; i < oleDbException.Errors.Count; i++)
                {
                    opErrorMessage += (oleDbException.Errors.Count > 1 ? "Index #" + i + Cst.CrLf : string.Empty) +
                               "Message: " + oleDbException.Errors[i].Message + Cst.CrLf +
                               "[NativeError: " + oleDbException.Errors[i].NativeError + "]" + Cst.CrLf +
                               "[Source: " + oleDbException.Errors[i].Source + "]" + Cst.CrLf +
                               "[SQLState: " + oleDbException.Errors[i].SQLState + "]" + Cst.CrLf2;
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
        #region IsSQLException
        public override bool IsSQLException(Exception pException)
        {
            return (pException.GetType() == typeof(OleDbException));
        }
        public override bool IsDuplicateKeyError(Exception pException)
        {
            bool ret = false;
            //
            //if (IsSQLException(pException))
            //{
            //    SqlException sqlEx = (SqlException)pException;
            //    //
            //    if (sqlEx.Number == 2601)
            //        ret = true;
            //}
            //
            return ret;
        }
        #endregion
        #region DbTypeToSqlDbType
        //public override SqlDbType DbTypeToSqlDbType(DbType pdbType)
        //{
        //    SqlParameter parm = new SqlParameter();
        //    try
        //    {
        //        parm.DbType = pdbType;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        // can't map
        //    }
        //    return parm.SqlDbType;
        //}
        #endregion

        #region Execute...()
        #region ExecuteNonQuery
        //public override int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteNonQuery(connectionString, commandType, commandText);
        //}
        //public override int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteNonQuery(connectionString, commandType, commandText, commandParameters);
        //}
        //public override int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteNonQuery(connectionString, spName, parameterValues);
        //}
        ////
        //public override int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlConnection)connection, commandType, commandText);
        //}
        //public override int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlConnection)connection, commandType, commandText, commandParameters);
        //}
        //public override int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlConnection)connection, spName, parameterValues);
        //}
        ////
        //public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, commandType, commandText);
        //}
        //public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, string spheresInfo, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, commandType, commandText, commandParameters);
        //}
        //public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, commandType, commandText, commandParameters);
        //}
        //public override int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteNonQuery((SqlTransaction)transaction, spName, parameterValues);
        //}
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
            return OleDbHelper.ExecuteDataAdapter(connectionString, commandText, datatable, updateBatchSize);
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
            return OleDbHelper.ExecuteDataAdapter(pDbTransaction, commandText, datatable, updateBatchSize);
        }
        #endregion ExecuteDataAdapter
        #region ExecuteDataSet
        public override DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return OleDbHelper.ExecuteDataset(connectionString, commandType, commandText);
        }
        public override DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return OleDbHelper.ExecuteDataset(connectionString, commandType, commandText, commandParameters);
        }
        //public override DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        //{
        //    return OleDbHelper.ExecuteDataset(connectionString, spName, parameterValues);
        //}
        //
        public override DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            return OleDbHelper.ExecuteDataset((OleDbConnection)connection, commandType, commandText);
        }
        public override DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return OleDbHelper.ExecuteDataset((OleDbConnection)connection, commandType, commandText, commandParameters);
        }
        //public override DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        //{
        //    return OleDbHelper.ExecuteDataset((OleDbConnection)connection, spName, parameterValues);
        //}
        //
        public override DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return OleDbHelper.ExecuteDataset((OleDbTransaction)transaction, commandType, commandText);
        }
        public override DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return OleDbHelper.ExecuteDataset((OleDbTransaction)transaction, commandType, commandText, commandParameters);
        }
        //public override DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    return OleDbHelper.ExecuteDataset((OleDbTransaction)transaction, spName, parameterValues);
        //}
        #endregion ExecuteDataSet
        #region ExecuteReader
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return OleDbHelper.ExecuteReader(connectionString, commandType, commandText);
        }
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return OleDbHelper.ExecuteReader(connectionString, commandType, commandText, commandParameters);
        }
        
        //public override IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        //{
        //    return OleDbHelper.ExecuteReader(connectionString, spName, parameterValues);
        //}
        //
        public override IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return OleDbHelper.ExecuteReader((OleDbConnection)connection, commandType, commandText);
        }
        public override IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return OleDbHelper.ExecuteReader((OleDbConnection)connection, commandType, commandText, commandParameters);
        }
        //public override IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        //{
        //    return OleDbHelper.ExecuteReader((SqlConnection)connection, spName, parameterValues);
        //}
        //
        public override IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return OleDbHelper.ExecuteReader((OleDbTransaction)transaction, commandType, commandText);
        }
        public override IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            return OleDbHelper.ExecuteReader((OleDbTransaction)transaction, commandType, commandText, commandParameters);
        }
        //public override IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    return OleDbHelper.ExecuteReader((OleDbTransaction)transaction, spName, parameterValues);
        //}
        #endregion ExecuteReader
        #region ExecuteScalar
        //public override object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteScalar(connectionString, commandType, commandText);
        //}
        //public override object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteScalar(connectionString, commandType, commandText, commandParameters);
        //}
        //public override object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteScalar(connectionString, spName, parameterValues);
        //}
        ////
        //public override object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteScalar((SqlConnection)connection, commandType, commandText);
        //}
        //public override object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteScalar((SqlConnection)connection, commandType, commandText, commandParameters);
        //}
        //public override object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteScalar((SqlConnection)connection, spName, parameterValues);
        //}
        ////
        //public override object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteScalar((SqlTransaction)transaction, commandType, commandText);
        //}
        //public override object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteScalar((SqlTransaction)transaction, commandType, commandText, commandParameters);
        //}
        //public override object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteScalar((SqlTransaction)transaction, spName, parameterValues);
        //}
        #endregion ExecuteScalar
        #region ExecuteXmlReader
        //public override XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteXmlReader((SqlConnection)connection, commandType, commandText);
        //}
        //public override XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteXmlReader((SqlConnection)connection, commandType, commandText, commandParameters);
        //}
        //public override XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteXmlReader((SqlConnection)connection, spName, parameterValues);
        //}
        ////
        //public override XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        //{
        //    return SqlHelper.ExecuteXmlReader((SqlTransaction)transaction, commandType, commandText);
        //}
        //public override XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        //{
        //    return SqlHelper.ExecuteXmlReader((SqlTransaction)transaction, commandType, commandText, commandParameters);
        //}
        //public override XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        //{
        //    return SqlHelper.ExecuteXmlReader((SqlTransaction)transaction, spName, parameterValues);
        //}
        #endregion ExecuteXmlReader

        #region GetDataAdapter
        public override DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            string pTableName, ref DataSet pDs, out IDbCommand opCommand)
        {
            return OleDbHelper.GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, pTableName, ref pDs, out opCommand);
        }
        public override DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, 
            IDbDataParameter[] pCommandParameters, out DataSet opFilledData, out IDbCommand opCommand)
        {
            return OleDbHelper.GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, out opFilledData, out opCommand);
        }
        #endregion GetDataAdapter

        #region SetUpdatedEventHandler
        public override void SetUpdatedEventHandler(DataAdapter pDa, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            OleDbHelper.SetUpdatedEventHandler(pDa as OleDbDataAdapter, pRowUpdating, pRowUpdated);
        }
        #endregion SetUpdatedEventHandler
        #region Update
        public override int Update(DataAdapter pDa, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            return OleDbHelper.Update(pDa as OleDbDataAdapter, pDataTable, pRowUpdating, pRowUpdated);
        }
        #endregion Update
        #region Fill
        public override int Fill(DataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            return OleDbHelper.Fill(pDa as OleDbDataAdapter, pDataSet, pTableName);
        }
        #endregion Fill
        #region SetSelectCommandText
        public override void SetSelectCommandText(DataAdapter pDa, string pQuery)
        {
            OleDbHelper.SetSelectCommandText(pDa as OleDbDataAdapter,pQuery);
        }
        #endregion SetSelectCommandText



        #endregion
        
        #region NewParameter
        //public override IDbDataParameter NewParameter()
        public override IDbDataParameter NewParameter(DbType dbType, ref bool isSpecialDbType)
        {
            return new OleDbParameter();
        }
        public override IDbDataParameter NewParameter(CommandType pCommandType, ref string parameterName, ref DbType dbType, ref bool isSpecialDbType)
        {
            string vp = GetVarPrefix;
            //
            if (!parameterName.StartsWith(vp))
                parameterName = vp + parameterName;
            //
            OleDbParameter dataParameter = new OleDbParameter();
            //
            return dataParameter;
        }
        public override IDbDataParameter NewCursorParameter(string parameterName)
        {
            throw (new ArgumentException("OleDb does not support Cursor parameter", "parameterType"));
        }
        #endregion
        
        #region Private methods
        /// <summary>
        /// tente l'ouverture d'une connexion 
        /// <para>La connexion est fermée après ouverture</para>
        /// </summary>
        /// <param name="pCS"></param>
        private void CheckSqlConnection(string pCS)
        {
            using (IDbConnection testConn = new OleDbConnection(pCS))
            {
                testConn.Open();
                testConn.Close();
            }
        }
        #endregion
    }
}

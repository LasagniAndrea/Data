using System;
using System.Data;
using System.Data.Common;
using System.Xml;

namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// Delegate method implementing RowUpdated on DataAdapter
    /// </summary>
    public delegate void RowUpdating(object sender, RowUpdatingEventArgs args);
    public delegate void RowUpdated(object sender, RowUpdatedEventArgs args);


    /// <summary>
    /// 
    /// </summary>
    public interface IDataAccess
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        SvrInfoConnection GetSvrInfoConnection(string pCS);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        IDbConnection OpenConnection(string pCS);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnection"></param>
        /// <returns></returns>
        IDbTransaction BeginTran(IDbConnection pConnection);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="opConnection"></param>
        /// <returns></returns>
        IDbTransaction BeginTran(string pCS, out IDbConnection opConnection);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsolevel"></param>
        /// <returns></returns>
        IDbTransaction BeginTran(string pCS, IsolationLevel pIsolevel);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pException"></param>
        /// <returns></returns>
        SQLErrorEnum AnalyseSQLException(Exception pException);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pException"></param>
        /// <param name="opErrorMessage"></param>
        /// <param name="opErrorEnum"></param>
        /// <returns></returns>
        bool AnalyseSQLException(Exception pException, out string opErrorMessage, out SQLErrorEnum opErrorEnum);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pException"></param>
        /// <returns></returns>
        bool IsSQLException(Exception pException);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pException"></param>
        /// <returns></returns>
        bool IsDuplicateKeyError(Exception pException);


        /// <summary>
        /// Return the referenced assembly used by the DAL (Data Access Layer)
        /// </summary>
        string GetReferencedAssemblyInfos();

        
        /// <summary>
        /// 
        /// </summary>
        DbSvrType GetDbSvrType { get; }

        /// <summary>
        /// 
        /// </summary>
        string GetVarPrefix { get; }

        /// <summary>
        /// 
        /// </summary>
        string GetConcatOperator { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        void CheckConnection(string pCS);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        string GetServerVersion(string pCS);

        
        #region ExecuteNonQuery
        int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText);
        int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues);
        //
        int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText);
        int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues);
        //
        int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText);
        int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, string spheresInfo, params IDbDataParameter[] commandParameters);
        int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues);

        /// <summary>
        /// Execute a single element INSERT including a big (more than 32KB ) xml field. 
        /// </summary>
        /// <remarks>Oracle specific</remarks>
        /// <param name="connectionString">the string to the Oracle DBMS </param>
        /// <param name="commandText">the insert command, type text</param>
        /// <param name="pXml">the xml document including the xml content we have to insert</param>
        /// <param name="commandParameters">additional command parameters</param>
        /// <param name="pXmlParameterName">the name of the parameter we have to fill with the xml contents inside of pXml.
        /// Do not prefix with special characters like ":" or "@"</param>
        /// <returns>1 when the insert succeed, 0 otherwise</returns>
        int ExecuteNonQueryXmlForOracle(string connectionString, string commandText,
            System.Xml.XmlDocument pXml, string pXmlParameterName, params IDbDataParameter[] commandParameters);

        #endregion
        #region ExecuteDataAdapter
        
        // FI 20220908 [XXXXX] Add updateBatchSize
        int ExecuteDataAdapter(string connectionString, string commandText, DataTable datatable, int updateBatchSize = 1);
        // FI 20220908 [XXXXX] Add updateBatchSize
        int ExecuteDataAdapter(IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize = 1);
        #endregion ExecuteDataAdapter
        #region ExecuteDataSet
        DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText);
        DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues);
        //
        DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText);
        DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues);
        //
        DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText);
        DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues);
        #endregion ExecuteDataSet
        #region ExecuteReader
        IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText);
        IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);

        IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues);
        //
        IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText);
        IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues);
        //
        IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText);
        IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues);
        #endregion ExecuteReader
        #region ExecuteScalar
        object ExecuteScalar(string connectionString, CommandType commandType, string commandText);
        object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        object ExecuteScalar(string connectionString, string spName, params object[] parameterValues);
        //
        object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText);
        object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues);
        //
        object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText);
        object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues);
        #endregion ExecuteScalar
        #region ExecuteXmlReader
        XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText);
        XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues);
        //
        XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText);
        XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters);
        XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues);
        #endregion ExecuteXmlReader

        DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            string pTableName, ref DataSet pDataset, out IDbCommand opCommand);
        DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            out DataSet opFilledData, out IDbCommand opCommand);
        void SetUpdatedEventHandler(DataAdapter pDataAdapter, RowUpdating pRowUpdating, RowUpdated pRowUpdated);
        int Update(DataAdapter pDataAdapter, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated);
        int Fill(DataAdapter pDataAdapter, DataSet pDataSet, string pTableName);
        void SetSelectCommandText(DataAdapter pDa, string pQuery);
        

        #region NewParameter
        //IDbDataParameter NewParameter();
        IDbDataParameter NewParameter(DbType dbType, ref bool isSpecialDbType);
        IDbDataParameter NewParameter(CommandType pCommandType, ref string parameterName, ref DbType dbType, ref bool isSpecialDbType);
        IDbDataParameter NewCursorParameter(string parameterName);
        #endregion

        void SetFetchSize(IDataReader pDataReader, long pNumRows);

        // PM 20200102 [XXXXX] New
        void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt);
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class DataAccessBase : IDataAccess
    {
        #region Private
        readonly string msgNoMatch = "No match for connection found";
        #endregion

        #region property
        #region public GetDbSvrType
        public virtual DbSvrType GetDbSvrType
        {
            get
            {
                return DbSvrType.dbUNKNOWN;
            }
        }
        #endregion
        #region public GetVarPrefix
        // EG 20180423 Analyse du code Correction [CA1065]
        public virtual string GetVarPrefix
        {
            get
            {
                throw (new InvalidOperationException(msgNoMatch));
            }
        }
        #endregion GetVarPrefix
        #region public GetConcatOperator
        // EG 20180423 Analyse du code Correction [CA1065]
        public virtual string GetConcatOperator
        {
            get
            {
                throw (new InvalidOperationException(msgNoMatch));
            }
        }
        #endregion
        #endregion

        #region Method
        /// <summary>
        /// Return the referenced component assembly
        /// </summary>
        public virtual string GetReferencedAssemblyInfos()
        {

            return "Not Provided";
        }

        

        #region Transaction
        public virtual SvrInfoConnection GetSvrInfoConnection(string pConnectionString)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        #region OpenConnection
        public virtual IDbConnection OpenConnection(string pConnectionString)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        #endregion
        #region BeginTransaction
        public virtual IDbTransaction BeginTran(IDbConnection pConnection)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual IDbTransaction BeginTran(string pConnectionString, IsolationLevel pIsolevel)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual IDbTransaction BeginTran(string pConnectionString, out IDbConnection opConnection)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        #endregion
        #endregion

        #region AnalyseSQLException
        /// <summary>
        /// Retourne le type d'erreur SQL si l'exception est de type SQL (Erreur issu du moteur SQL)
        /// </summary>
        public virtual SQLErrorEnum AnalyseSQLException(Exception pException)
        {
            _ = AnalyseSQLException(pException, out _, out SQLErrorEnum opErrorEnum);
            return opErrorEnum;
        }
        /// <summary>
        /// Retourne true si l'exception est de type SQL (Erreur issu du moteur SQL)
        /// </summary>
        /// <param name="pException"></param>
        /// <param name="opError"></param>
        /// <param name="opMessage"></param>
        /// <returns></returns>
        public virtual bool AnalyseSQLException(Exception pException, out string opErrorMessage, out SQLErrorEnum opErrorEnum)
        {
            bool ret = false;
            opErrorMessage = string.Empty;
            opErrorEnum = SQLErrorEnum.Unknown;
            //
            if (pException.GetType() == typeof(DBConcurrencyException))
            {
                #region DBConcurrencyException
                ret = true;
                DBConcurrencyException dbConcurrencyException;
                dbConcurrencyException = (DBConcurrencyException)pException;

                int errorCode = 0;//dbConcurrencyException.;
                opErrorMessage += "SQL:";
                //opError += errorCode.ToString();
                switch (errorCode)
                {
                    case 0:
                    default:
                        opErrorEnum = SQLErrorEnum.Concurrency;
                        opErrorMessage += " [" + dbConcurrencyException.Message + "]";
                        break;
                }
                #endregion
            }
            // EG 20130724
            else if (pException.GetType() == typeof(TimeoutException))
            {
                #region TimeoutException
                ret = true;
                TimeoutException timeoutException = pException as TimeoutException;
                opErrorEnum = SQLErrorEnum.Timeout;
                opErrorMessage += " [" + timeoutException.Message + "]";
                #endregion TimeoutException
            }
            return ret;
        }
        public virtual bool IsSQLException(Exception pException)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual bool IsDuplicateKeyError(Exception pException)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        #endregion
        #region DbTypeToSqlDbType
        //public virtual SqlDbType DbTypeToSqlDbType(DbType pdbType)
        //{
        //    return SqlDbType.Variant;
        //}
        #endregion

        #region  CheckConnection
        public virtual void CheckConnection(string pConnectionString)
        {
        }
        #endregion
        #region  GetServerVersion
        public virtual string GetServerVersion(string pConnectionString)
        {
            return "Unknow";
        }
        #endregion

        
        #region ExecuteNonQuery
        public virtual int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        //
        public virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        //
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, string spheresInfo, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }

        /// <summary>
        /// Execute a single element INSERT including a big (more than 32KB ) xml field. 
        /// </summary>
        /// <remarks>Oracle specific</remarks>
        /// <param name="connectionString">the string to the Oracle DBMS </param>
        /// <param name="commandText">the insert command, type text</param>
        /// <param name="pXml">the xml document including the xml content we have to insert</param>
        /// <param name="commandParameters">additional command parameters</param>
        /// <param name="pXmlParameterName">the name of the parameter we have to fill with the xml contents inside of pXml.
        /// Do not prefix with special characters like ":" or "@"</param>
        /// <returns>1 when the insert succeed, 0 otherwise</returns>
        public virtual int ExecuteNonQueryXmlForOracle(string connectionString, string commandText,
            System.Xml.XmlDocument pXml, string pXmlParameterName, params IDbDataParameter[] commandParameters)
        {
            throw new NotSupportedException("ExecuteNonQueryBigXml is compliant with Oracle DBMS only");
        }


        #endregion
        #region ExecuteDataAdapter
        public virtual int ExecuteDataAdapter(string connectionString, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual int ExecuteDataAdapter(IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize = 1)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }

        public abstract DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            out DataSet opFilledData, out IDbCommand opCommand);
        public abstract DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            string pTableName, ref DataSet pDataSet, out IDbCommand opCommand);
        public abstract void SetUpdatedEventHandler(DataAdapter pDataAdapter, RowUpdating pRowUpdating, RowUpdated pRowUpdated);
        public abstract int Update(DataAdapter pDataAdapter, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated);
        public abstract int Fill(DataAdapter pDataAdapter, DataSet pDataSet, string pTableName);
        public abstract void SetSelectCommandText(DataAdapter pDa, string pQuery);

        #endregion ExecuteDataAdapter
        #region ExecuteDataSet
        public virtual DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        //
        public virtual DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        //
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        #endregion ExecuteDataSet
        #region ExecuteReader
        public virtual IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }

        public virtual IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        //
        public virtual IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        //
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }

        #endregion ExecuteReader
        #region ExecuteScalar
        public virtual object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        public virtual object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }
        //
        public virtual object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        //
        public virtual object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }

        #endregion ExecuteScalar
        #region ExecuteXmlReader
        public virtual XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        public virtual XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "connection"));
        }
        //
        public virtual XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        public virtual XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException(msgNoMatch, "transaction"));
        }
        #endregion ExecuteXmlReader
        
        #region NewParameter
        //public virtual IDbDataParameter NewParameter()
        public virtual IDbDataParameter NewParameter(DbType dbType, ref bool isSpecialDbType)
        {
            throw (new NotImplementedException("NewParameter must be overrided"));
        }
        public virtual IDbDataParameter NewParameter(CommandType pCommandType, ref string parameterName, ref DbType dbType, ref bool isSpecialDbType)
        {
            throw (new NotImplementedException("NewParameter must be overrided"));
        }
        public virtual IDbDataParameter NewCursorParameter(string parameterName)
        {
            throw (new NotImplementedException("NewCursorParameter NewParameter must be overrided"));
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDestinationTable"></param>
        /// <param name="pDt"></param>
        /// PM 20200102 [XXXXX] New
        public virtual void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt)
        {
            throw (new ArgumentException(msgNoMatch, "connectionString"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataReader"></param>
        /// <param name="pNumRows"></param>
        public virtual void SetFetchSize(IDataReader pDataReader, long pNumRows) { }
        #endregion

    }

}


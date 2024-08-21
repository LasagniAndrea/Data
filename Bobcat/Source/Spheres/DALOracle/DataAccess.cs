using System;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Reflection;
using Oracle.DataAccess.Client;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

namespace EFS.DALOracle
{
    /// <summary>
    /// Used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
    /// we can set the appropriate CommandBehavior when calling ExecuteReader()
    /// </summary>
    public enum DbConnectionOwnership
    {
        /// <summary>Connection is owned and managed by SqlHelper</summary>
        Internal,
        /// <summary>Connection is owned and managed by the caller</summary>
        External
    }

    // EG 20181010 PERF Add GetSvrInfoConnection
    public sealed class DataAccess : DataAccessBase, ICultureParameter
    {

        #region property
        
        #region public override GetDbSvrType
        public override DbSvrType GetDbSvrType
        {
            get
            {
                return DbSvrType.dbORA;
            }
        }

        #endregion
        #region public override GetVarPrefix
        public override string GetVarPrefix
        {
            get
            {
                return ":";
            }
        }
        #endregion GetVarPrefix
        #region public override GetConcatOperator
        public override string GetConcatOperator
        {
            get
            {
                return "||";
            }
        }
        #endregion GetConcatOperator
        #endregion


        /// <summary>
        /// 
        /// </summary>
        public override string GetReferencedAssemblyInfos()
        {
            AssemblyName assembly = typeof(OracleConnection).Assembly.GetName();
            return $"Assembly identity={assembly.Name}, Version={assembly.Version}";
        }

        /// <summary>
        /// Récupère les information Serveurs Oracle
        /// </summary>
        /// <returns></returns>
        public override SvrInfoConnection GetSvrInfoConnection(string pCS)
        {
            SvrInfoConnection svrInfo;
            using (OracleConnection cn = OpenOracleConnection(pCS))
            {
                svrInfo = new SvrInfoConnection
                {
                    ServerType = DbSvrType.dbORA,
                    ServiceName = cn.ServiceName,
                    DatabaseDomainName = cn.DatabaseDomainName,
                    Database = cn.Database,
                    DatabaseName = cn.DatabaseName,
                    DataSource = cn.DataSource,
                    HostName = cn.HostName,
                    InstanceName = cn.InstanceName
                };
                string[] version = cn.ServerVersion.Split(".".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries);
                svrInfo.ServerVersion = String.Format("{0}.{1}", version);
            };

            try
            {

                PropertyInfo ptyInfo = typeof(OracleConnection).GetProperty("ProviderVersion");
                if (null != ptyInfo)
                    svrInfo.ProviderVersion = ptyInfo.GetValue("ProviderVersion").ToString();
            }
            catch { svrInfo.ProviderVersion = Cst.NotAvailable; }

            return svrInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        public override void CheckConnection(string pCS)
        {
            CheckOracleConnection(pCS);
        }
        
        /// <summary>
        /// Retourne la version du serveur de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public override string GetServerVersion(string pCS)
        {
            OracleConnection testConn = new OracleConnection(pCS);
            testConn.Open();
            string ret = "Oracle " + testConn.ServerVersion;
            testConn.Close();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public override IDbConnection OpenConnection(string pCS)
        {
            return OpenOracleConnection(pCS);
        }
        private OracleConnection OpenOracleConnection(string pS)
        {
            OracleConnection cn = new OracleConnection(pS);
            cn.Open();
            return cn;
        }


        #region public override BeginTransaction
        public override IDbTransaction BeginTran(string pConnectionString, out IDbConnection opConnection)
        {
           
            //
            OracleConnection cn = OpenOracleConnection(pConnectionString);
            OracleTransaction oracleTransaction = cn.BeginTransaction();
            opConnection = cn;
            
            return oracleTransaction;
        }
        public override IDbTransaction BeginTran(string pConnectionString, IsolationLevel pIsolevel)
        {
            OracleConnection cn;
            OracleTransaction oracleTransaction;
            // PM 20121023 / RD 20121024 : Oracle ne supporte que les deux niveaux d'isolation:
            // - READ COMMITTED.
            // - SERIALIZABLE
            // Dans ce cas "Isolation Level" est remis à la valeur par défaut qui est "Read Committed"
            if (IsolationLevel.ReadCommitted != pIsolevel && IsolationLevel.Serializable != pIsolevel)
                pIsolevel = IsolationLevel.ReadCommitted;
            //
            cn = OpenOracleConnection(pConnectionString);
            oracleTransaction = cn.BeginTransaction(pIsolevel);
            //
            return oracleTransaction;
        }
        public override IDbTransaction BeginTran(IDbConnection pConnection)
        {
            OracleTransaction oracleTransaction;
            //
            if (pConnection.State != ConnectionState.Open)
                pConnection.Open();
            oracleTransaction = (OracleTransaction)pConnection.BeginTransaction();
            //
            return oracleTransaction;
        }
        #endregion
        //
        // RD 20100114 [16792] Réfactoring pour utiliser SQLErrorEnum et Gestion de DeadLock
        #region public override AnalyseSQLException
        public override bool AnalyseSQLException(Exception pException, out string opErrorMessage, out SQLErrorEnum opErrorCode)
        {
            opErrorMessage = string.Empty;
            bool ret;
            if (IsSQLException(pException))
            {
                #region OracleException
                ret = true;
                OracleException sqlException;
                sqlException = (OracleException)pException;
                int errorCode = sqlException.Number;

                opErrorMessage += "SQL:";
                opErrorMessage += errorCode.ToString();

                switch (errorCode)
                {
                    case 001400:
                        {
                            opErrorCode = SQLErrorEnum.NullValue; // Cannot insert the value NULL into column '%.*ls'; column does not allow nulls. INSERT fails.                            
                            //20080908 PL Ajout de la ligne suivante afin d'afficher l'erreur complète et disposer ainsi du nom de la colonne concernée.
                            opErrorMessage += " [" + sqlException.Message.Replace("\n", " ") + "]";
                            break;
                        }
                    case 000001:
                        {
                            opErrorCode = SQLErrorEnum.DuplicateKey; //Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'.                            
                            opErrorMessage += " [" + sqlException.Message.Replace("\n", " ") + "]";
                            break;
                        }
                    case 000060:
                        {
                            opErrorCode = SQLErrorEnum.DeadLock; //Deadlock detected                            
                            opErrorMessage += " [" + sqlException.Message.Replace("\n", " ") + "]";
                            break;
                        }
                    case 02292:
                        {
                            opErrorCode = SQLErrorEnum.Integrity; //Update statement conflicted with TABLE FOREIGN KEY constraint '%.*ls'. The conflict occurred in database '%.*ls', table '%.*ls'.
                            opErrorMessage += " [" + sqlException.Message.Replace("\n", " ") + "]";
                            break;
                        }
                    default:
                        {
                            opErrorCode = SQLErrorEnum.Unknown;
                            //Oracle renvoie des \n seuls et non pas des \r\n traités eux par nos fonctions JS
                            opErrorMessage += " [" + sqlException.Message.Replace("\n", " ") + "]";
                            break;
                        }
                }
                #endregion
            }
            else
                ret = base.AnalyseSQLException(pException, out opErrorMessage, out opErrorCode);
            return ret;
        }
        #endregion
        #region public override IsSQLException
        public override bool IsSQLException(Exception pException)
        {
            return (pException.GetType() == typeof(OracleException));
        }
        public override bool IsDuplicateKeyError(Exception pException)
        {
            bool ret = false;
            //
            if (IsSQLException(pException))
            {
                OracleException sqlEx = (OracleException)pException;
                //
                if (sqlEx.Number == 1)
                    ret = true;
            }
            //
            return ret;
        }
        #endregion
        

        #region Execute...()
        #region ExecuteNonQuery
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery(connectionString, commandType, commandText);
        }
        public override int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //commandText = ReplaceVarPrefix(commandText);
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery(connectionString, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteNonQuery(connectionString, spName, parameterValues);
        }
        //
        public override int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery((OracleConnection)connection, commandType, commandText);
        }
        public override int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //commandText = ReplaceVarPrefix(commandText);
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery((OracleConnection)connection, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteNonQuery((OracleConnection)connection, spName, parameterValues);
        }
        //
        public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery((OracleTransaction)transaction, commandType, commandText);
        }
        public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, string spheresInfo, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //commandText = ReplaceVarPrefix(commandText);
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery((OracleTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //commandText = ReplaceVarPrefix(commandText);
            //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
            if (commandType == CommandType.Text)
                commandText = AddBlockBeginEnd(commandText);
            //
            return OraHelper.ExecuteNonQuery((OracleTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteNonQuery((OracleTransaction)transaction, spName, parameterValues);
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
        public override int ExecuteNonQueryXmlForOracle(string connectionString, string commandText, 
            System.Xml.XmlDocument pXml, string pXmlParameterName, params IDbDataParameter[] commandParameters)
        {
            return OraHelper.ExecuteNonQueryXmlForOracle(connectionString, commandText, pXml, pXmlParameterName, commandParameters);
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
            return OraHelper.ExecuteDataAdapter(connectionString, commandText, datatable, updateBatchSize);
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
            return OraHelper.ExecuteDataAdapter(pDbTransaction, commandText, datatable, updateBatchSize);
        }
        #endregion ExecuteDataAdapter
        #region ExecuteDataSet
        public override DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            if (commandType == CommandType.Text)
                return OraHelper.ExecuteDatasetMultiSelect(connectionString, commandText, null);
            else
                return OraHelper.ExecuteDataset(connectionString, commandType, commandText);
        }
        public override DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (commandType == CommandType.Text)
            {
                // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
                //commandText = ReplaceVarPrefix(commandText);
                return OraHelper.ExecuteDatasetMultiSelect(connectionString, commandText, commandParameters);
            }
            else
            {
                return OraHelper.ExecuteDataset(connectionString, commandType, commandText, commandParameters);
            }
        }
        public override DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteDataset(connectionString, spName, parameterValues);
        }
        //
        public override DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            if (commandType == CommandType.Text)
                return OraHelper.ExecuteDatasetMultiSelect((OracleConnection)connection, commandText, null);
            else
                return OraHelper.ExecuteDataset((OracleConnection)connection, commandType, commandText);
        }
        public override DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (commandType == CommandType.Text)
            {
                // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
                //commandText = ReplaceVarPrefix(commandText);
                return OraHelper.ExecuteDatasetMultiSelect((OracleConnection)connection, commandText, commandParameters);
            }
            else
                return OraHelper.ExecuteDataset((OracleConnection)connection, commandType, commandText, commandParameters);
        }
        public override DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteDataset((OracleConnection)connection, spName, parameterValues);
        }
        //
        public override DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            if (commandType == CommandType.Text)
                return OraHelper.ExecuteDatasetMultiSelect((OracleTransaction)transaction, commandText, null);
            else
                return OraHelper.ExecuteDataset((OracleTransaction)transaction, commandType, commandText);
        }
        public override DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (commandType == CommandType.Text)
            {
                // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
                //commandText = ReplaceVarPrefix(commandText);
                return OraHelper.ExecuteDatasetMultiSelect((OracleTransaction)transaction, commandText, commandParameters);
            }
            else
                return OraHelper.ExecuteDataset((OracleTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteDataset((OracleTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteDataSet
        #region ExecuteReader
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return OraHelper.ExecuteReader(connectionString, commandType, commandText);
        }
        public override IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteReader(connectionString, commandType, commandText, commandParameters);
        }
        public override IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteReader(connectionString, spName, parameterValues);
        }
        //
        public override IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteReader((OracleConnection)connection, commandType, commandText);
        }
        public override IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteReader((OracleConnection)connection, commandType, commandText, commandParameters);
        }
        public override IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteReader((OracleConnection)connection, spName, parameterValues);
        }
        //
        public override IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteReader((OracleTransaction)transaction, commandType, commandText);
        }
        public override IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteReader((OracleTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteReader((OracleTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteReader
        #region ExecuteScalar
        public override object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteScalar(connectionString, commandType, commandText);
        }
        public override object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteScalar(connectionString, commandType, commandText, commandParameters);
        }
        public override object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteScalar(connectionString, spName, parameterValues);
        }
        //
        public override object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            return OraHelper.ExecuteScalar((OracleConnection)connection, commandType, commandText);
        }
        public override object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteScalar((OracleConnection)connection, commandType, commandText, commandParameters);
        }
        public override object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteScalar((OracleConnection)connection, spName, parameterValues);
        }
        //
        public override object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteScalar((OracleTransaction)transaction, commandType, commandText);
        }
        public override object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            // RD 20100520 & FI 20100520 : Code déplacé dans DataHelper.TransformQuery
            //if (commandType == CommandType.Text)
            //    commandText = ReplaceVarPrefix(commandText);
            //
            return OraHelper.ExecuteScalar((OracleTransaction)transaction, commandType, commandText, commandParameters);
        }
        public override object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            return OraHelper.ExecuteScalar((OracleTransaction)transaction, spName, parameterValues);
        }
        #endregion ExecuteScalar
        #region ExecuteXmlReader
        public override XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            throw (new ArgumentException("Oracle does not support the ExecuteXmlReader method", "connection"));
        }
        public override XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException("Oracle does not support the ExecuteXmlReader method", "connection"));
        }
        public override XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException("Oracle does not support the ExecuteXmlReader method", "connection"));
        }
        //
        public override XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            throw (new ArgumentException("Oracle does not support the ExecuteXmlReader method", "transaction"));
        }
        public override XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
        {
            throw (new ArgumentException("Oracle does not support the ExecuteXmlReader method", "transaction"));
        }
        public override XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            throw (new ArgumentException("Oracle does not support the ExecuteXmlReader method", "transaction"));
        }
        #endregion ExecuteXmlReader

        #region GetDataAdapter
        public override DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            string pTableName, ref DataSet pDs, out IDbCommand opCommand)
        {
            return OraHelper.GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, pTableName, ref pDs, out opCommand);
        }
        public override DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
                out DataSet opFilledData, out IDbCommand opCommand)
        {
            return OraHelper.GetDataAdapter(pDbTransaction, pCommandText, pCommandParameters, out opFilledData, out opCommand);
        }
        #endregion GetDataAdapter
        #region SetUpdatedEventHandler
        public override void SetUpdatedEventHandler(DataAdapter pDa, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            OraHelper.SetUpdatedEventHandler(pDa as OracleDataAdapter, pRowUpdating, pRowUpdated);
        }
        #endregion SetUpdatedEventHandler
        #region Update
        public override int Update(DataAdapter pDa, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            return OraHelper.Update(pDa as OracleDataAdapter, pDataTable, pRowUpdating, pRowUpdated);
        }
        #endregion Update
        #region Fill
        public override int Fill(DataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            return OraHelper.Fill(pDa as OracleDataAdapter, pDataSet, pTableName);
        }
        #endregion Fill
        #region SetSelectCommandText
        public override void SetSelectCommandText(DataAdapter pDa, string pQuery)
        {
            OraHelper.SetSelectCommandText(pDa as OracleDataAdapter, pQuery);
        }
        #endregion SetSelectCommandText
        #endregion

        #region NewParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="pIsSpecialDbType"></param>
        /// <returns></returns>
        /// FI 20170116 [21916] Modify
        public override IDbDataParameter NewParameter(DbType dbType, ref bool pIsSpecialDbType)
        {
            OracleParameter parameter = new OracleParameter();
            pIsSpecialDbType = false;
            switch (dbType)
            {
                case DbType.Xml:
                    pIsSpecialDbType = true;
                    parameter.OracleDbType = OracleDbType.XmlType;
                    break;
                case DbType.Boolean:
                    dbType = DbType.Int32;
                    break;
                case DbType.DateTimeOffset: // FI 20170116 [21916] add case (DateTimeOffset => TimeStampTZ sous Oracle)
                    pIsSpecialDbType = true;
                    parameter.OracleDbType = OracleDbType.TimeStampTZ;
                    break;
                default:
                    break;
            }
            return parameter;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCommandType"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="pIsSpecialDbType"></param>
        /// <returns></returns>
        // FI 20170116 [21916] Modify
        // PL 20171128 Modify
        public override IDbDataParameter NewParameter(CommandType pCommandType, ref string parameterName, ref DbType dbType, ref bool pIsSpecialDbType)
        {
            string vp = GetVarPrefix;
            
            if ((pCommandType != CommandType.StoredProcedure) && (!parameterName.StartsWith(vp)))
                parameterName = vp + parameterName;
            
            OracleParameter parameter = new OracleParameter();
            pIsSpecialDbType = false;
            switch (dbType)
            {
                case DbType.Xml:
                    pIsSpecialDbType = true;
                    parameter.OracleDbType = OracleDbType.XmlType;
                    break;
                case DbType.Boolean:
                    dbType = DbType.Int32;
                    break;
                case DbType.DateTime2: // PL 20171128 add case (DateTime2 => TimeStamp sous Oracle)
                    pIsSpecialDbType = true;
                    parameter.OracleDbType = OracleDbType.TimeStamp;
                    break;
                case DbType.DateTimeOffset: // FI 20170116 [21916] add case (DateTimeOffset => TimeStampTZ sous Oracle)
                    pIsSpecialDbType = true;
                    parameter.OracleDbType = OracleDbType.TimeStampTZ;
                    break;
                default :
                    break;
            }
            return parameter;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public override IDbDataParameter NewCursorParameter(string parameterName)
        {
            return new OracleParameter(parameterName, OracleDbType.RefCursor);
        }
        #endregion

        public override void SetFetchSize(IDataReader pDataReader, long pNumRows)
        {
            OraHelper.SetFetchSize(pDataReader as OracleDataReader, pNumRows);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDestinationTable"></param>
        /// <param name="pDt"></param>
        // PM 20200102 [XXXXX] New
        public override void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt)
        {
            OraHelper.BulkWriteToServer(pConnectionString, pDestinationTable, pDt);
        }

        

        #region Private methods

        /// <summary>
        /// Encapsulation d'une commande SQL dans un Bloc "begin ... end" 
        /// <para>
        /// ATTENTION, la valeur de retour de l'exécution d'un Bloc n'indique plus le nombre de lignes impactées, 
        /// mais renvoie 1 en cas de succès, et ce même si aucune ligne n'a été impactée (ex. cas d'un update ou d'un delete)
        /// </para>
        /// </summary>
        /// <param name="pCommandText"></param>
        /// <returns></returns>
        //20131230 PL New version
        private static string AddBlockBeginEnd(string pCommandText)
        {
            pCommandText = pCommandText.Trim();

            //Si la command se termine par un ";", cela indique qu'il existe plusieurs query dans la commande
            if (
                (pCommandText.EndsWith(";") || pCommandText.EndsWith(SQLCst.SEPARATOR_MULTIDML))
                &&
                ((!pCommandText.StartsWith(SQLCst.BEGIN)) && (!pCommandText.StartsWith(SQLCst.DECLARE)))
               )
            {
                //20060526 PL/FDA Warning: Pb Oracle avec \r\n dans les blocs
                //20131230 PL On croyait qu'il y avait également un pb avec les commentaires en début, mais a priori le pb est là aussi unqiuement lié aux \r\n (Cst.CrLf).
                //            On substitue donc les \r\n (Cst.CrLf) par des simple \n (Cst.Lf) 
                //            NB: A priori, ce pb ne semble plus persister en Oracle 11g.
                pCommandText = SQLCst.BEGIN
                             + Cst.Lf
                             //+ pCommandText.Replace(SQLCst.SEPARATOR_MULTIDML, @"; ").Replace(Cst.CrLf, Cst.Lf)
                             + pCommandText.Replace(Cst.CrLf, Cst.Lf)
                             + Cst.Lf
                             + SQLCst.END;
            }

            return pCommandText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        private void CheckOracleConnection(string pConnectionString)
        {
            using (OracleConnection testConn = new OracleConnection(pConnectionString))
            {
                testConn.Open();

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20181110 Mise en commentaire afin de vérifier si c'est la cause des ORA-12571 : TNS packet Writer Failure
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20180926 Test in progress... (See also CompleteConnectionString(...) in SystemSettings.cs)
                //string cmd;
                ////cmd = "DBMS_APPLICATION_INFO.SET_CLIENT_INFO('" + Software.CopyrightSmall + " - Web');";
                //cmd = "DBMS_APPLICATION_INFO.SET_CLIENT_INFO('" + Software.CopyrightSmall + "');";
                //ExecuteNonQuery(testConn, CommandType.Text, cmd, null);

                //cmd = "DBMS_APPLICATION_INFO.SET_MODULE('MS IIS','Spheres Web');";
                //ExecuteNonQuery(testConn, CommandType.Text, cmd, null);
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                testConn.Close();
            }
        }
        #endregion

        #region ICultureParameter Membres

        public string Sort
        {
            get
            {
                return OraHelper.Sort;
            }
            set
            {
                OraHelper.Sort = value;
            }
        }

        public string Compare
        {
            get
            {
                return OraHelper.Where;
            }
            set
            {
                OraHelper.Where = value;
            }
        }

        #endregion
    }
}

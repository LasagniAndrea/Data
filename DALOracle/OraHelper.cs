//===============================================================================
// OraHelper by Tyler Jensen, Principal, NetBrick Inc. - tyler@netbrick.net 
// Verion 1.0 - March 2003
// Release Notes: 
// No ExecuteXmlReader methods are implemented because the System.Data.OracleClient
// class does not currently implement the ExecuteXmlReader command method.
//===============================================================================
// Copyright (C) 2003 NetBrick Inc.. Limited rights reserved as stated here:
// This work is a derivative work and thus likely falls under the copyrights of Microsoft.
// The author claims no copyright other than for the portion of the work which
// he produced. For such copyrights, the author reserves no rights other than
// to be notified by email at the above address of improvements to the work.
//===============================================================================
// Portions taken from Microsoft's original work: Microsoft.ApplictionBlocks.Data
// Copyright (C) 2000-2001 Microsoft Corporation
// Full download for the Microsoft.ApplictionBlocks.Data.SqlHelper class can be found at:
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/emab-rm.asp
//===============================================================================
// Any user of this work should consider the copyrights of Microsoft Corporation
// prior to the distribution of this code.
//===============================================================================
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;

using System.Data;
using System.Data.Common;
using System.Collections;
using System.Text.RegularExpressions;

using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System.Xml;

namespace EFS.DALOracle
{
  /// <summary>
  /// The OraHelper class is intended to encapsulate high performance, scalable best practices for 
  /// common uses of SqlClient.
  /// </summary>
	public sealed class OraHelper
	{
        internal static void SetFetchSize(OracleDataReader pDataReader, long pNumRows)
        {
            long newFetchSize = pDataReader.RowSize * pNumRows;
            pDataReader.FetchSize = Math.Max(pDataReader.FetchSize, newFetchSize);
        }

		#region private utility methods & constructors

		//Since this class provides only static methods, make the default constructor private to prevent 
		//instances from being created with "new OraHelper()".
		private OraHelper() {}

		/// <summary>
		/// This method is used to attach array of OracleParameters to a OracleCommand.
		/// 
		/// This method will assign a value of DbNull to any parameter with a direction of
		/// InputOutput and a value of null.  
		/// 
		/// This behavior will prevent default values from being used, but
		/// this will be the less common case than an intended pure output parameter (derived as InputOutput)
		/// where the user provided no input value.
		/// </summary>
		/// <param name="command">The command to which the parameters will be added</param>
		/// <param name="commandParameters">an array of OracleParameters tho be added to command</param>
        private static void AttachParameters(OracleCommand command, IDbDataParameter[] commandParameters)
		{
			foreach (IDbDataParameter p in commandParameters)
			{
				if(p!=null)
				{
					//check for derived output value with no value assigned
					if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
					{
						p.Value = DBNull.Value;
					}
				
					command.Parameters.Add((OracleParameter) p);
				}
			}
		}

		/// <summary>
		/// This method assigns an array of values to an array of OracleParameters.
		/// </summary>
		/// <param name="commandParameters">array of OracleParameters to be assigned values</param>
		/// <param name="parameterValues">array of objects holding the values to be assigned</param>
		private static void AssignParameterValues(OracleParameter[] commandParameters, object[] parameterValues)
		{
			if ((commandParameters == null) || (parameterValues == null)) 
			{
				//do nothing if we get no data
				return;
			}

			// we must have the same number of values as we pave parameters to put them in
			if (commandParameters.Length != parameterValues.Length + 1)
			{
				throw new ArgumentException("Parameter count does not match Parameter Value count.");
			}

			//iterate through the OracleParameters, assigning the values from the corresponding position in the 
			//value array     TODO TODO - may have to start with 0 or 1 depending on whether return param (cursor)
			for (int i = 0, j = commandParameters.Length - 1; i < j; i++)
			{
				commandParameters[i+1].Value = parameterValues[i];
			}
		}

		/// <summary>
		/// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
		/// to the provided command.
		/// </summary>
		/// <param name="command">the OracleCommand to be prepared</param>
		/// <param name="connection">a valid OracleConnection, on which to execute this command</param>
		/// <param name="transaction">a valid OracleTransaction, or 'null'</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters to be associated with the command or 'null' if no parameters are required</param>
		private static void PrepareCommand(OracleCommand command, OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters)
		{
			//if the provided connection is not open, we will open it
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

            SetOracleGlobalization(connection);

			//associate the connection with the command
			command.Connection = connection;

			//set the command text (stored procedure name or SQL statement)
			command.CommandText = commandText;

			//if we were provided a transaction, assign it.
            // 20080402 RD ODP.NET
            //if (transaction != null)
            //{
            //    command.Transaction = transaction;
            //}

            // 20080402 RD ODP.NET
            // Set if the parameter bind is by name or by order number
            command.BindByName = true;//OracleClient_ORACLE GLOP

			//set the command type
			command.CommandType = commandType;

			//attach the command parameters if they are provided
			if (commandParameters != null)
			{
				AttachParameters(command, commandParameters);
			}

			return;
		}

        /// <summary>
        /// Set Sort and Comparison property according with the web.Config value
        /// </summary>
        /// <param name="pConnection">current connection to a DB Oracle</param>
        /// <remarks>
        /// 20120504 MF ticket 17740
        /// 20120506 MF ticket 17743
        /// </remarks>
        private static void SetOracleGlobalization(OracleConnection pConnection)
        {
            OracleGlobalization sessionInfo = pConnection.GetSessionInfo();
            sessionInfo.Sort = OraHelper.Sort;
            sessionInfo.Comparison = OraHelper.Where;

            //WARNING: ---------------------------------------------------------------------------------
            sessionInfo.Sort = "BINARY";                    //EG/PM/PL 20180926 Test in progress... use "BINARY" instead of "GENERIC_M" by MF
            sessionInfo.NCharConversionException = false;   //EG/PM/PL 20180926 Test in progress... use "false" instead of "true" by default.
            //WARNING: ---------------------------------------------------------------------------------
            
            pConnection.SetSessionInfo(sessionInfo);
            // RD 20121231 [18328] Fuite mémoire
            // La méthode GetSessionInfo() instancie un nouvel objet 
            sessionInfo.Dispose();
        }
		#endregion private utility methods & constructors

		#region ExecuteDataAdapter
		/// <summary>
		/// 
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="commandText"></param>
		/// <param name="datatable"></param>
		/// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
		/// <returns></returns>
		/// /// 20091001 EG Dispose() sur OracleCommand
		/// EG 20180423 Analyse du code Correction [CA2200]
		/// EG 20180425 Analyse du code Correction [CA2202]
		/// FI 20220908 [XXXXX] Add updateBatchSize
		internal static int ExecuteDataAdapter(string connectionString, string commandText, DataTable datatable, int updateBatchSize =1)
		{
			int nRows=-1;
			//create & open a SqlConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				try
				{
					commandText = commandText.Replace(SQLCst.SQL_RDBMS + Cst.CrLf, string.Empty);//Warning: 20050616 PL Bug Oracle s'il existe un commentaire en début de query
					commandText = commandText.Replace(SQLCst.SQL_ANSI + Cst.CrLf, string.Empty);//Warning: 20050616 PL Bug Oracle s'il existe un commentaire en début de query
																								//create a command and prepare it for execution
					using (OracleCommand cmd = new OracleCommand())
					{

						PrepareCommand(cmd, cn, (OracleTransaction)null, CommandType.Text, commandText, null);

						//create the DataAdapter & CommandBuilder
						using (OracleDataAdapter da = new OracleDataAdapter(cmd))
						{
							if (updateBatchSize != 1)
								da.UpdateBatchSize = updateBatchSize;

							using (OracleCommandBuilder cb = new OracleCommandBuilder(da))
							{

								nRows = da.Update(datatable);
								/*
								// FI 20231205 La verrue n'est plus appliquée après tests concluants lors du marquage d'un process
								// EG 20201106 Verrue lorsque la méthode update plante 
								try { nRows = da.Update(datatable); }
								catch 
								{
                                    da.InsertCommand = cb.GetInsertCommand();
                                    da.UpdateCommand = cb.GetUpdateCommand();
                                    da.DeleteCommand = cb.GetDeleteCommand();
									nRows = da.Update(datatable);
								}
								*/
							}
						}
					}
				}
				catch { throw; }
			}
            return nRows;						
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pDbTransaction"></param>
		/// <param name="commandText"></param>
		/// <param name="datatable"></param>
		/// <param name="updateBatchSize">Activation/Désactivation de la mise à jour par lots (valeur par défaut 1 : Désactivation de la mise à jour par lot)</param>
		/// <returns></returns>
		/// 20091001 EG Dispose() sur OracleCommand
		/// EG 20180423 Analyse du code Correction [CA2200]
		/// EG 20180425 Analyse du code Correction [CA2202]
		/// FI 20220908 [XXXXX] Add updateBatchSize
		internal static int ExecuteDataAdapter(IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize = 1)
		{
			int nRows = -1;
			try
			{
				OracleTransaction oracleTransaction = (OracleTransaction)pDbTransaction;
				//create a command and prepare it for execution
				using (OracleCommand cmd = new OracleCommand())
				{
					PrepareCommand(cmd, oracleTransaction.Connection, oracleTransaction, CommandType.Text, commandText, null);

					//create the DataAdapter & CommandBuilder
					using (OracleDataAdapter da = new OracleDataAdapter(cmd))
					{
						if (updateBatchSize != 1)
							da.UpdateBatchSize = updateBatchSize;

						using (OracleCommandBuilder cb = new OracleCommandBuilder(da))
						{
							nRows = da.Update(datatable);

							/*
							// FI 20231205 La verrue n'est plus appliquée après tests concluants lors du marquage d'un process
							// EG 20201106 Verrue lorsque la méthode update plante 
							try { nRows = da.Update(datatable); }
							catch 
							{
								da.InsertCommand = cb.GetInsertCommand();
								da.UpdateCommand = cb.GetUpdateCommand();
								da.DeleteCommand = cb.GetDeleteCommand();
								nRows = da.Update(datatable);
							}
							*/

						}
					}

				}
			}
			catch { throw; }
			return nRows;
		}
		#endregion ExecuteDataAdapter
	  
		#region ExecuteNonQuery

		/// <summary>
		/// Execute a OracleCommand (that returns no resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteNonQuery(connectionString, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns no resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
        // EG 20151224 [POC - MUREX] Add cn.Close()
        // EG 20180425 Analyse du code Correction [CA2202]
        internal static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a OracleConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
                //return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
                int ret = ExecuteNonQuery(cn, commandType, commandText, commandParameters);
                return ret;
			}
		}

		/// <summary>
		/// Execute a single element INSERT including a big (more than 32KB ) xml field. 
		/// </summary>
		/// <remarks>Oracle specific</remarks>
		/// <param name="connectionString">the string to the Oracle DBMS </param>
		/// <param name="commandText">the insert command, type text</param>
		/// <param name="pXml">the xml document including the xml content we have to insert</param>
		/// <param name="pXmlParameterName">the name of the parameter we have to fill with the xml contents inside of pXml.
		/// Do not prefix with special characters like ":" or "@"</param>
		/// <param name="commandParameters">additional command parameters</param>
		/// <returns>1 when the insert succeed</returns>
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static int ExecuteNonQueryXmlForOracle(string connectionString, string commandText, 
						XmlDocument pXml, string pXmlParameterName, IDbDataParameter[] commandParameters)
        {
            int retval = 0;

            using (OracleConnection cn = new OracleConnection(connectionString))
            {
                cn.Open();

				using (OracleCommand cmd = new OracleCommand())
				{
					// 1. Start a transaction to get rid of big xml (more than 32KB)
					using (OracleTransaction tran = cn.BeginTransaction())
					{
						OracleXmlType xmlinfo = new OracleXmlType(cn, pXml);

						PrepareCommand(cmd, cn, (OracleTransaction)null, CommandType.Text, commandText, commandParameters);
						try
						{
							OracleParameter xmlParam = cmd.Parameters[String.Format(":{0}", pXmlParameterName)];

							// 2. Assign value here, after starting the Transaction. to pass with a doc with way more than 4000 chars
							xmlParam.Value = xmlinfo;

							// 3. Execute the command.
							retval = cmd.ExecuteNonQuery();
						}
						finally
						{
							// 4 detach the OracleParameters from the command object, so they can be used again.
							cmd.Parameters.Clear();
						}

						// 4.1 Finally commit the transaction
						tran.Commit();

					}
				}
            }

            return retval;
        }

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns no resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored prcedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns no resultset and takes no parameters) against the provided OracleConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteNonQuery(connection, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns no resultset) against the specified OracleConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		/// 20091001 EG Dispose() sur OracleCommand
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
				try
				{
					//finally, execute the command.
					return cmd.ExecuteNonQuery();
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns no resultset) against the specified OracleConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns no resultset and takes no parameters) against the provided OracleTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteNonQuery(transaction, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns no resultset) against the specified OracleTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		/// 20091001 EG Dispose() sur OracleCommand
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//finally, execute the command.
					return cmd.ExecuteNonQuery();
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns no resultset) against the specified 
		/// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
			}
		}


		#endregion ExecuteNonQuery

		#region ExecuteDataSet

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteDataset(connectionString, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a OracleConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteDataset(cn, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset and takes no parameters) against the provided OracleConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteDataset(connection, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset) against the specified OracleConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur OracleCommand
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
				try
				{
					//create the DataAdapter & DataSet
					using (OracleDataAdapter da = new OracleDataAdapter(cmd))
					{
						DataSet ds = new DataSet();
						//fill the DataSet using default values for DataTable names, etc.
						da.Fill(ds);
						//return the dataset
						return ds;
					}
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.			
					cmd.Parameters.Clear();
				}
			}
		}
		
		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a resultset) against the specified OracleConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset and takes no parameters) against the provided OracleTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteDataset(transaction, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset) against the specified OracleTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur OracleCommand
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//create the DataAdapter & DataSet
					using (OracleDataAdapter da = new OracleDataAdapter(cmd))
					{
						DataSet ds = new DataSet();
						//fill the DataSet using default values for DataTable names, etc.
						da.Fill(ds);
						//return the dataset
						return ds;
					}
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}
		
		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a resultset) against the specified 
		/// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteDataSet

		#region ExecuteReader

		/// <summary>
		/// Create and prepare a OracleCommand, and call ExecuteReader with the appropriate CommandBehavior.
		/// </summary>
		/// <remarks>
		/// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
		/// 
		/// If the caller provided the connection, we want to leave it to them to manage.
		/// </remarks>
		/// <param name="connection">a valid OracleConnection, on which to execute this command</param>
		/// <param name="transaction">a valid OracleTransaction, or 'null'</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters to be associated with the command or 'null' if no parameters are required</param>
		/// <param name="connectionOwnership">indicates whether the connection parameter was provided by the caller, or created by OraHelper</param>
		/// <returns>OracleDataReader containing the results of the command</returns>
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		private static OracleDataReader ExecuteReader(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters, DbConnectionOwnership connectionOwnership)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

				try
				{
					//create a reader
					OracleDataReader dr;
					// call ExecuteReader with the appropriate CommandBehavior
					if (connectionOwnership == DbConnectionOwnership.External)
					{
						dr = cmd.ExecuteReader();
					}
					else
					{
						dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					}

					return dr;
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteReader(connectionString, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a OracleConnection
			OracleConnection cn = new OracleConnection(connectionString);
			cn.Open();

			try
			{
				//call the private overload that takes an internally owned connection in place of the connection string
				return ExecuteReader(cn, null, commandType, commandText, commandParameters,DbConnectionOwnership.Internal);
			}
			catch
			{
				//if we fail to return the SqlDatReader, we need to close the connection ourselves
				cn.Close();
				throw;
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset and takes no parameters) against the provided OracleConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteReader(connection, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset) against the specified OracleConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//pass through the call to the private overload using a null transaction value and an externally owned connection
			return ExecuteReader(connection, (OracleTransaction)null, commandType, commandText, commandParameters, DbConnectionOwnership.External);
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a resultset) against the specified OracleConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteReader(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset and takes no parameters) against the provided OracleTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteReader(transaction, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a resultset) against the specified OracleTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///   OracleDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//pass through to private overload, indicating that the connection is owned by the caller
			return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, DbConnectionOwnership.External);
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a resultset) against the specified
		/// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  OracleDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a OracleDataReader containing the resultset generated by the command</returns>
		internal static OracleDataReader ExecuteReader(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteReader

		#region ExecuteScalar
		
		/// <summary>
		/// Execute a OracleCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteScalar(connectionString, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a 1x1 resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a OracleConnection, and dispose of it after we are done.
			using (OracleConnection cn = new OracleConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteScalar(cn, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a 1x1 resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a 1x1 resultset and takes no parameters) against the provided OracleConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteScalar(connection, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a 1x1 resultset) against the specified OracleConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur OracleCommand
		/// FI 20210125 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
				try
				{
					//execute the command & return the results
					return cmd.ExecuteScalar();
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a 1x1 resultset) against the specified OracleConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid OracleConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(OracleConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a 1x1 resultset and takes no parameters) against the provided OracleTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of OracleParameters
			return ExecuteScalar(transaction, commandType, commandText, null);
		}

		/// <summary>
		/// Execute a OracleCommand (that returns a 1x1 resultset) against the specified OracleTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or PL-SQL command</param>
		/// <param name="commandParameters">an array of OracleParameters used to execute the command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur OracleCommand
		internal static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (OracleCommand cmd = new OracleCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//execute the command & return the results
					return cmd.ExecuteScalar();
				}
				finally
				{
					// detach the OracleParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a OracleCommand (that returns a 1x1 resultset) against the specified
		/// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid OracleTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(OracleTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				OracleParameter[] commandParameters = OraHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of OracleParameters
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteScalar	


        #region ExecuteDataAdapter
        // EG 20180423 Analyse du code Correction [CA2200]
        internal static System.Data.Common.DataAdapter GetDataAdapter
            (IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            out DataSet opFilledData, out IDbCommand opCommand)
        {

            opCommand = null;
            OracleDataAdapter da = null;

            opFilledData = new DataSet();

            try
            {
                OracleTransaction sqlTransaction = (OracleTransaction)pDbTransaction;
                //create a command and prepare it for execution
                opCommand = new OracleCommand();
                PrepareCommand
                    ((OracleCommand)opCommand, sqlTransaction.Connection, sqlTransaction, CommandType.Text, pCommandText, pCommandParameters);

                //create the DataAdapter & CommandBuilder
                da = new OracleDataAdapter((OracleCommand)opCommand);

                OracleCommandBuilder cb = new OracleCommandBuilder(da);

                da.Fill(opFilledData);
            }
            catch (Exception)
            {
                if (null != opCommand)
                {
                    opCommand.Parameters.Clear();
                    opCommand.Dispose();
                    opCommand = null;
                }

                if (opFilledData != null)
                {
                    opFilledData.Dispose();
                    opFilledData = null;
                }

                if (da != null)
                {
                    da.Dispose();
                }

                throw;
            }

            return da;
        }

        // EG 20180423 Analyse du code Correction [CA2200]
        internal static DataAdapter GetDataAdapter(IDbTransaction pDbTransaction, string pCommandText,
            IDbDataParameter[] pCommandParameters, string pTableName, ref DataSet pDataSet, out IDbCommand opCommand)
        {

            opCommand = null;
            OracleDataAdapter da = null;
            try
            {
                OracleTransaction sqlTransaction = (OracleTransaction)pDbTransaction;
                //create a command and prepare it for execution
                opCommand = new OracleCommand();
                PrepareCommand((OracleCommand)opCommand, sqlTransaction.Connection, sqlTransaction, CommandType.Text, pCommandText, pCommandParameters);

                //create the DataAdapter & CommandBuilder
                da = new OracleDataAdapter((OracleCommand)opCommand);
                OracleCommandBuilder cb = new OracleCommandBuilder(da);
                da.Fill(pDataSet, pTableName);
            }
            catch (Exception)
            {
                if (null != opCommand)
                {
                    opCommand.Parameters.Clear();
                    opCommand.Dispose();
                    opCommand = null;
                }
                if (da != null)
                {
                    da.Dispose();
                }
                throw;
            }
            return da;
        }
		#endregion ExecuteDataAdapter

        #region SetUpdatedEventHandler
        internal static void SetUpdatedEventHandler(OracleDataAdapter pDa, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            pDa.RowUpdating += new OracleRowUpdatingEventHandler(pRowUpdating);
            pDa.RowUpdated += new OracleRowUpdatedEventHandler(pRowUpdated);
        }
		#endregion RowUpdatedEventHandler
		#region Update
		// EG 20210326 [XXXXX] Corporate actions : Gestion bug sur Oracle et le DataAdapter
		public static int Update(OracleDataAdapter pDa, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
			int ret = 0;
            pDa.RowUpdating += new OracleRowUpdatingEventHandler(pRowUpdating);
            pDa.RowUpdated += new OracleRowUpdatedEventHandler(pRowUpdated);
			using (OracleCommandBuilder cb = new OracleCommandBuilder(pDa))
			{
				try { ret = pDa.Update(pDataTable); }
				catch
				{
					pDa.InsertCommand = cb.GetInsertCommand();
					pDa.UpdateCommand = cb.GetUpdateCommand();
					pDa.DeleteCommand = cb.GetDeleteCommand();
					ret = pDa.Update(pDataTable);
				}
			}
			return ret;
        }
        #endregion Update
        #region Fill
        public static int Fill(OracleDataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            return pDa.Fill(pDataSet, pTableName);
        }
        #endregion Fill
        #region SetSelectCommandText
        public static void SetSelectCommandText(OracleDataAdapter pDa, string pQuery)
        {
            if (null != pDa.SelectCommand)
                pDa.SelectCommand.CommandText = pQuery;
        }
        #endregion SetSelectCommandText

		#region internal ExecuteDatasetMultiSelect
		internal static DataSet ExecuteDatasetMultiSelect (string connectionString, string pCommandText, params IDbDataParameter[] commandParameters)
		{
			DataSet ret = new DataSet();
			string[] aTmp = Regex.Split(pCommandText, SQLCst.SEPARATOR_MULTISELECT);
			if (ArrFunc.IsFilled(aTmp))
			{
				for (int i=0 ; i< aTmp.Length ; i++)
				{
					if (StrFunc.IsFilled(aTmp[i].Trim()))
					{
                        DataSet ds;
                        if (aTmp[i].IndexOf(":") < 0) 
							//Aucun paramètre sur la query
							ds = ExecuteDataset (connectionString,CommandType.Text,aTmp[i]);
					    else
							ds = ExecuteDataset (connectionString,CommandType.Text,aTmp[i],commandParameters);
						//
						if (null!=ds && (ds.Tables.Count >0))
						{
							DataTable dt  = ds.Tables[0];  
							dt.TableName = "T" + i.ToString();
							ret.Tables.Add(dt.Copy());
						}
					}
				}
			}
			return ret;
		}


		// EG 20180423 Analyse du code Correction [CA2200]
		internal static DataSet ExecuteDatasetMultiSelect(OracleConnection connection, string commandText, params IDbDataParameter[] commandParameters)
		{
			DataSet ret = new DataSet();
			string[] aTmp = Regex.Split(commandText, SQLCst.SEPARATOR_MULTISELECT);
			if (ArrFunc.IsFilled(aTmp))
			{
				for (int i = 0; i < aTmp.Length; i++)
				{
					if (StrFunc.IsFilled(aTmp[i].Trim()))
					{
						DataSet ds;
						if (aTmp[i].IndexOf(":") < 0)
							//Aucun paramètre sur la query
							ds = ExecuteDataset(connection, CommandType.Text, aTmp[i]);
						else
							ds = ExecuteDataset(connection, CommandType.Text, aTmp[i], commandParameters);

						if (ds.Tables.Count > 0)
						{
							DataTable dt = ds.Tables[0];
							dt.TableName = "T" + i.ToString();
							ret.Tables.Add(dt.Copy());
						}
					}
				}
			}
			return ret;
		}
		//
		internal static DataSet ExecuteDatasetMultiSelect (OracleTransaction transaction, string commandText, params IDbDataParameter[] commandParameters)
		{
			
			DataSet ret = new DataSet(); 
			string[] aTmp = Regex.Split(commandText, SQLCst.SEPARATOR_MULTISELECT);
			if (ArrFunc.IsFilled(aTmp))
			{
				for (int i=0 ; i< aTmp.Length ; i++)
				{
					if (StrFunc.IsFilled(aTmp[i].Trim()))
					{
                        DataSet ds;
                        if (aTmp[i].IndexOf(":") < 0) 
							//Aucun paramètre sur la query
							ds = ExecuteDataset (transaction,CommandType.Text,aTmp[i]);
					    else
							ds = ExecuteDataset (transaction,CommandType.Text,aTmp[i],commandParameters);
						if (null!= ds && (ds.Tables.Count >0))
						{
							DataTable dt  = ds.Tables[0];  
							dt.TableName = "Table_" + i.ToString();
							ret.Tables.Add(dt.Copy());
						}
					}
				}
			}
			return ret;
		}
		#endregion

        /// <summary>
        /// Set default property values
        /// </summary>
        static OraHelper()
        {
            // default NLS_SORT setting: 
            Sort = "GENERIC_M";
            
            // default NLS_COMP setting 
            Where = "BINARY";
        }

        /// <summary>
        /// Set/Get the NLS_SORT session parameter of the ODP client.
        /// Specifies the type of sort for character data
        /// </summary>
        /// <value>
        /// contains either of the following values:
        ///NLS_SORT = BINARY | sort_name (FRENCH, FRENCH_M, GENERIC, GENERIC_M,.....)
        /// </value>
        public static string Sort { get; set; }

        /// <summary>
        /// Set/Get the NLS_COMP session parameter of the ODP client.
        /// affects the comparison behavior of SQL operations.
        /// </summary>
        /// <value>
        /// Range of values: BINARY , LINGUISTIC, or ANSI
        /// </value>
        public static string Where { get; set; }

        #region BulkCopy
        /// <summary>
        /// Création d'un objet BulkCopy
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New
        internal static OracleBulkCopy BulkCopy(string pConnectionString)
        {
            return new OracleBulkCopy(pConnectionString);
        }
        
        /// <summary>
        /// Création d'un objet BulkCopy
        /// </summary>
        /// <param name="pConnection"></param>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New
        internal static OracleBulkCopy BulkCopy(OracleConnection pConnection)
        {
            return new OracleBulkCopy(pConnection);
        }

        /// <summary>
        /// Ecriture des données d'un DataTable via BulkCopy
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDestinationTable"></param>
        /// <param name="pDt"></param>
        // PM 20200102 [XXXXX] New
        internal static void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt)
        {
            using (OracleBulkCopy bulkCopy = BulkCopy(pConnectionString))
            {
                bulkCopy.DestinationTableName = pDestinationTable;

                foreach (DataColumn dc in pDt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }

                bulkCopy.WriteToServer(pDt);
            }
        }
        #endregion BulkCopy
    }

  /// <summary>
  /// OraHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
  /// ability to discover parameters for stored procedures at run-time.
  /// </summary>
  public sealed class OraHelperParameterCache
  {
		#region private methods, variables, and constructors

    //Since this class provides only static methods, make the default constructor private to prevent 
    //instances from being created with "new OraHelperParameterCache()".
    private OraHelperParameterCache() {}

    private readonly static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

    /// <summary>
    /// resolve at run time the appropriate set of OracleParameters for a stored procedure
    /// </summary>
    /// <param name="connectionString">a valid connection string for a OracleConnection</param>
    /// <param name="spName">the name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">whether or not to include their return value parameter</param>
    /// <returns></returns>
    private static OracleParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
    {
      using (OracleConnection cn = new OracleConnection(connectionString)) 
      using (OracleCommand cmd = new OracleCommand(spName,cn))
      {
        cn.Open();
        cmd.CommandType = CommandType.StoredProcedure;

        // 20060626 RD ODP.NET
        // Method doesn't exist in ODP.NET 9.2 but it exists in ODP.NET 10g
        // OracleCommandBuilder.DeriveParameters(cmd);

        //Oracle requires the return value parameter (the cursor) 
        //but it's direction must be changed to Output (not InputOutput as the .NET provider returns here)
        //if (!includeReturnValueParameter) 
        //{
        //  cmd.Parameters.RemoveAt(0);
        //}
        // 20060626 RD ODP.NET
        //if (cmd.Parameters[0].OracleType == OracleType.Cursor)//OracleClient_FRAMEWORK
        if (cmd.Parameters[0].OracleDbType == OracleDbType.RefCursor)//OracleClient_ORACLE
            cmd.Parameters[0].Direction = ParameterDirection.Output;

        OracleParameter[] discoveredParameters = new OracleParameter[cmd.Parameters.Count];

        cmd.Parameters.CopyTo(discoveredParameters, 0);

        return discoveredParameters;
      }
    }

    //deep copy of cached OracleParameter array
    private static OracleParameter[] CloneParameters(OracleParameter[] originalParameters)
    {
      OracleParameter[] clonedParameters = new OracleParameter[originalParameters.Length];

      for (int i = 0, j = originalParameters.Length; i < j; i++)
      {
        clonedParameters[i] = (OracleParameter)((ICloneable)originalParameters[i]).Clone();
      }

      return clonedParameters;
    }

		#endregion private methods, variables, and constructors

		#region caching functions

    /// <summary>
    /// add parameter array to the cache
    /// </summary>
    /// <param name="connectionString">a valid connection string for a OracleConnection</param>
    /// <param name="commandText">the stored procedure name or PL-SQL command</param>
    /// <param name="commandParameters">an array of OracleParameters to be cached</param>
    internal static void CacheParameterSet(string connectionString, string commandText, params OracleParameter[] commandParameters)
    {
      string hashKey = connectionString + ":" + commandText;

      paramCache[hashKey] = commandParameters;
    }

    /// <summary>
    /// retrieve a parameter array from the cache
    /// </summary>
    /// <param name="connectionString">a valid connection string for a OracleConnection</param>
    /// <param name="commandText">the stored procedure name or PL-SQL command</param>
    /// <returns>an array of OracleParameters</returns>
    internal static OracleParameter[] GetCachedParameterSet(string connectionString, string commandText)
    {
      string hashKey = connectionString + ":" + commandText;

      OracleParameter[] cachedParameters = (OracleParameter[])paramCache[hashKey];
			
      if (cachedParameters == null)
      {			
        return null;
      }
      else
      {
        return CloneParameters(cachedParameters);
      }
    }

		#endregion caching functions

		#region Parameter Discovery Functions

    /// <summary>
    /// Retrieves the set of OracleParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connectionString">a valid connection string for a OracleConnection</param>
    /// <param name="spName">the name of the stored procedure</param>
    /// <returns>an array of OracleParameters</returns>
    internal static OracleParameter[] GetSpParameterSet(string connectionString, string spName)
    {
      return GetSpParameterSet(connectionString, spName, false);
    }

    /// <summary>
    /// Retrieves the set of OracleParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connectionString">a valid connection string for a OracleConnection</param>
    /// <param name="spName">the name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">a bool value indicating whether the return value parameter should be included in the results</param>
    /// <returns>an array of OracleParameters</returns>
    internal static OracleParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
    {
      string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter":"");

      OracleParameter[] cachedParameters;
			
      cachedParameters = (OracleParameter[])paramCache[hashKey];

      if (cachedParameters == null)
      {			
        cachedParameters = (OracleParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
      }
			
      return CloneParameters(cachedParameters);
    }

		#endregion Parameter Discovery Functions

  }
}

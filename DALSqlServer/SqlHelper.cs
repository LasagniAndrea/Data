using System;
using System.Data.Common;
using System.Reflection;
using System.Data;
using System.Xml;
using System.Data.SqlClient;
using System.Collections;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

namespace EFS.DALSQLServer
{
	/// <summary>
	/// The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
	/// common uses of SqlClient.
	/// </summary>
	public sealed class SqlHelper
	{
		#region private utility methods & constructors

		//Since this class provides only static methods, make the default constructor private to prevent 
		//instances from being created with "new OTCAccess()".
		private SqlHelper() { }

		/// <summary>
		/// This method is used to attach array of SqlParameters to a SqlCommand.
		/// 
		/// This method will assign a value of DbNull to any parameter with a direction of
		/// InputOutput and a value of null.  
		/// 
		/// This behavior will prevent default values from being used, but
		/// this will be the less common case than an intended pure output parameter (derived as InputOutput)
		/// where the user provided no input value.
		/// </summary>
		/// <param name="command">The command to which the parameters will be added</param>
		/// <param name="commandParameters">an array of SqlParameters tho be added to command</param>
		private static void AttachParameters(SqlCommand command, IDbDataParameter[] commandParameters)
		{
			foreach (IDbDataParameter p in commandParameters)
			{
				if (p != null)
				{
					//check for derived output value with no value assigned
					if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
					{
						p.Value = DBNull.Value;
					}

					command.Parameters.Add((SqlParameter)p);
				}
			}
		}

		/// <summary>
		/// This method assigns an array of values to an array of SqlParameters.
		/// </summary>
		/// <param name="commandParameters">array of SqlParameters to be assigned values</param>
		/// <param name="parameterValues">array of objects holding the values to be assigned</param>
		private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
		{
			if ((commandParameters == null) || (parameterValues == null))
			{
				//do nothing if we get no data
				return;
			}

			// we must have the same number of values as we pave parameters to put them in
			if (commandParameters.Length != parameterValues.Length)
			{
				throw new ArgumentException("Parameter count does not match Parameter Value count.");
			}

			//iterate through the SqlParameters, assigning the values from the corresponding position in the 
			//value array
			for (int i = 0, j = commandParameters.Length; i < j; i++)
			{
				commandParameters[i].Value = parameterValues[i];
			}
		}

		/// <summary>
		/// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
		/// to the provided command.
		/// </summary>
		/// <param name="command">the SqlCommand to be prepared</param>
		/// <param name="connection">a valid SqlConnection, on which to execute this command</param>
		/// <param name="transaction">a valid SqlTransaction, or 'null'</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
		private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters)
		{
			//if the provided connection is not open, we will open it
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}
			//associate the connection with the command
			command.Connection = connection;
			//PL test
			//            if (commandText.IndexOf("ALLOCATION Mono book") > 0)
			//            {
			//                command.CommandType = CommandType.Text;
			////                command.CommandText = @"
			////                    SET ANSI_NULLS ON
			////                    SET ANSI_PADDING ON
			////                    SET ANSI_WARNINGS ON
			////                    SET ARITHABORT ON
			////                    SET CONCAT_NULL_YIELDS_NULL ON
			////                    SET QUOTED_IDENTIFIER ON
			////                    SET NUMERIC_ROUNDABORT OFF";
			//                command.CommandText = @"SET ARITHABORT ON";
			//                command.ExecuteNonQuery();
			//            }
			//command timeout
			#region 20080108 PL Test TimeOut
			if (connection.ConnectionTimeout > 30) //Note: 30 sec. is the defaut commant timeout value
			{
				command.CommandTimeout = connection.ConnectionTimeout;
			}
			#endregion
			//set the command text (stored procedure name or SQL statement)
			command.CommandText = commandText;
			//if we were provided a transaction, assign it.
			if (transaction != null)
			{
				command.Transaction = transaction;
			}
			//set the command type
			command.CommandType = commandType;
			//attach the command parameters if they are provided
			if (commandParameters != null)
			{
				AttachParameters(command, commandParameters);
			}
			//
			return;
		}


		#endregion private utility methods & constructors      

		#region ExecuteDataAdapter (PL)
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="commandText"></param>
		/// <param name="datatable"></param>
		/// <param name="updateBatchSize">Activation/D�sactivation de la mise � jour par lots (valeur par d�faut 1 : D�sactivation de la mise � jour par lot)</param>
		/// <returns></returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// EG 20180423 Analyse du code Correction [CA2200]
		/// EG 20180425 Analyse du code Correction [CA2202]
		/// FI 20220908 [XXXXX] Add updateBatchSize
		internal static int ExecuteDataAdapter(string connectionString, string commandText, DataTable datatable, int updateBatchSize = 1)
		{
			int nRows = -1;
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();
				try
				{
					//create a command and prepare it for execution
					using (SqlCommand cmd = new SqlCommand())
					{
						PrepareCommand(cmd, cn, (SqlTransaction)null, CommandType.Text, commandText, (SqlParameter[])null);
						//create the DataAdapter & CommandBuilder

						using (SqlDataAdapter da = new SqlDataAdapter(cmd))
						{
							if (updateBatchSize != 1)
								da.UpdateBatchSize = updateBatchSize;

							using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
							{
								nRows = da.Update(datatable);
								nRows = nRows + 1 - 1;
							}
						}
					}
				}
				catch (Exception ex)
				{
					string s = ex.Message;
					throw;
				}
			}
			return nRows;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pDbTransaction"></param>
		/// <param name="commandText"></param>
		/// <param name="datatable"></param>
		/// <param name="updateBatchSize">Activation/D�sactivation de la mise � jour par lots (valeur par d�faut 1 : D�sactivation de la mise � jour par lot)</param>
		/// <returns></returns>
		//// 20091001 EG Dispose() sur SQLCommand
		/// EG 20180423 Analyse du code Correction [CA2200]
		/// FI 20220908 [XXXXX] Add updateBatchSize
		internal static int ExecuteDataAdapter(IDbTransaction pDbTransaction, string commandText, DataTable datatable, int updateBatchSize=1)
		{
			int nRows = -1;
			try
			{
				SqlTransaction sqlTransaction = (SqlTransaction)pDbTransaction;
				//create a command and prepare it for execution
				using (SqlCommand cmd = new SqlCommand())
				{
					PrepareCommand(cmd, sqlTransaction.Connection, sqlTransaction, CommandType.Text, commandText, (SqlParameter[])null);
					//create the DataAdapter & CommandBuilder

					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
					{
						if (updateBatchSize != 1)
							da.UpdateBatchSize = updateBatchSize;

						using (SqlCommandBuilder cb = new SqlCommandBuilder(da))
						{
							nRows = da.Update(datatable);
							nRows = nRows + 1 - 1;
						}
					}
				}
			}
			catch (Exception)
			{
                throw;
			}
			return nRows;
		}
		#endregion ExecuteDataAdapter

		#region ExecuteNonQuery

		/// <summary>
		/// Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				//gloppl SqlParameter[] sqlParameters;
				//sqlParameters = (SqlParameter[]) commandParameters;
				//return ExecuteNonQuery(cn, commandType, commandText, SqlParameter[] commandParameters);
				return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored prcedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
			//otherwise we can just call the SP without params
			else
			{
				return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteNonQuery(connection, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
				try
				{
					//finally, execute the command.
					return cmd.ExecuteNonQuery();
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			};
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			//otherwise we can just call the SP without params
			else
			{
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteNonQuery(transaction, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//finally, execute the command.
					return cmd.ExecuteNonQuery();
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified 
		/// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an int representing the number of rows affected by the command</returns>
		internal static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
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
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(connectionString, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteDataset(cn, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
			//otherwise we can just call the SP without params
			else
			{
				return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// EG 20180423 Analyse du code Correction [CA2200]
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters);
				try
				{
					//create the DataAdapter & DataSet
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
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
					// detach the SqlParameters from the command object, so they can be used again.			
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			//otherwise we can just call the SP without params
			else
			{
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//create the DataAdapter & DataSet
					using (SqlDataAdapter da = new SqlDataAdapter(cmd))
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
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
		/// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
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
		/// this enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
		/// we can set the appropriate CommandBehavior when calling ExecuteReader()
		/// </summary>
		private enum SqlConnectionOwnership
		{
			/// <summary>Connection is owned and managed by SqlHelper</summary>
			Internal,
			/// <summary>Connection is owned and managed by the caller</summary>
			External
		}

		/// <summary>
		/// Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
		/// </summary>
		/// <remarks>
		/// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
		/// 
		/// If the caller provided the connection, we want to leave it to them to manage.
		/// </remarks>
		/// <param name="connection">a valid SqlConnection, on which to execute this command</param>
		/// <param name="transaction">a valid SqlTransaction, or 'null'</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
		/// <param name="connectionOwnership">indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
		/// <returns>SqlDataReader containing the results of the command</returns>
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, IDbDataParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

				//create a reader
				SqlDataReader dr;
				try
				{
					// call ExecuteReader with the appropriate CommandBehavior
					if (connectionOwnership == SqlConnectionOwnership.External)
					{
						dr = cmd.ExecuteReader();
					}
					else
					{
						dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					}
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
				return dr;
			}
		}
	
        

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteReader(connectionString, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a SqlConnection
			SqlConnection cn = new SqlConnection(connectionString);
			cn.Open();

			try
			{
				//call the private overload that takes an internally owned connection in place of the connection string
				return ExecuteReader(cn, null, commandType, commandText, commandParameters,SqlConnectionOwnership.Internal);
			}
			catch
			{
				//if we fail to return the SqlDatReader, we need to close the connection ourselves
				cn.Close();
				throw;
			}
		}
        

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteReader(connection, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//pass through the call to the private overload using a null transaction value and an externally owned connection
			return ExecuteReader(connection, (SqlTransaction)null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no SqlHelper to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

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
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteReader(transaction, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//pass through to private overload, indicating that the connection is owned by the caller
			return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
		/// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a SqlDataReader containing the resultset generated by the command</returns>
		internal static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

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
		/// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
		/// the connection string. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteScalar(connectionString, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (SqlConnection cn = new SqlConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteScalar(cn, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
		/// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteScalar(connection, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters);
				try
				{
					//execute the command & return the results
					return cmd.ExecuteScalar();
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteScalar(transaction, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		/// 20091001 EG Dispose() sur SQLCommand
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//execute the command & return the results
					return cmd.ExecuteScalar();
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}
		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified
		/// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an object containing the value in the 1x1 resultset generated by the command</returns>
		internal static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
			}
		}

		#endregion ExecuteScalar	

		#region ExecuteXmlReader

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
		/// <returns>an XmlReader containing the resultset generated by the command</returns>
		internal static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteXmlReader(connection, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an XmlReader containing the resultset generated by the command</returns>
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters);
				try
				{
					//create the DataAdapter & DataSet
					XmlReader retval = cmd.ExecuteXmlReader();
					return retval;
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
		/// using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">a valid SqlConnection</param>
		/// <param name="spName">the name of the stored procedure using "FOR XML AUTO"</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>an XmlReader containing the resultset generated by the command</returns>
		internal static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
		/// <returns>an XmlReader containing the resultset generated by the command</returns>
		internal static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteXmlReader(transaction, commandType, commandText, (SqlParameter[])null);
		}

		/// <summary>
		/// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
		/// using the provided parameters.
		/// </summary>
		/// <remarks>
		/// e.g.:  
		///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
		/// <param name="commandText">the stored procedure name or T-SQL command using "FOR XML AUTO"</param>
		/// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
		/// <returns>an XmlReader containing the resultset generated by the command</returns>
		/// FI 20210122 [XXXXX] Refactoring (using + cmd.Parameters.Clear in finally)
		internal static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params IDbDataParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			using (SqlCommand cmd = new SqlCommand())
			{
				PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
				try
				{
					//create the DataAdapter & DataSet
					XmlReader retval = cmd.ExecuteXmlReader();
					return retval;
				}
				finally
				{
					// detach the SqlParameters from the command object, so they can be used again.
					cmd.Parameters.Clear();
				}
			}
		}

		/// <summary>
		/// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
		/// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
		/// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
		/// </summary>
		/// <remarks>
		/// This method provides no access to output parameters or the stored procedure's return value parameter.
		/// 
		/// e.g.:  
		///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">a valid SqlTransaction</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="parameterValues">an array of objects to be assigned as the input values of the stored procedure</param>
		/// <returns>a dataset containing the resultset generated by the command</returns>
		internal static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
		{
			//if we receive parameter values, we need to figure out where they go
			if ((parameterValues != null) && (parameterValues.Length > 0)) 
			{
				//pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
				SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

				//assign the provided values to these parameters based on parameter order
				AssignParameterValues(commandParameters, parameterValues);

				//call the overload that takes an array of SqlParameters
				return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
				//otherwise we can just call the SP without params
			else 
			{
				return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
			}
		}


		#endregion ExecuteXmlReader

        #region GetDataAdapter
        // EG 20180423 Analyse du code Correction [CA2200]
        internal static System.Data.Common.DataAdapter GetDataAdapter
            (IDbTransaction pDbTransaction, string pCommandText, IDbDataParameter[] pCommandParameters,
            out DataSet opFilledData, out IDbCommand opCommand)
        {

            opCommand = null;
            SqlDataAdapter da = null;

            opFilledData = new DataSet();

            try
            {
                SqlTransaction sqlTransaction = (SqlTransaction)pDbTransaction;
                //create a command and prepare it for execution
                opCommand = new SqlCommand();
                PrepareCommand((SqlCommand)opCommand, sqlTransaction.Connection, sqlTransaction, CommandType.Text, pCommandText, pCommandParameters);

                //create the DataAdapter & CommandBuilder
                da = new SqlDataAdapter((SqlCommand)opCommand);

                SqlCommandBuilder cb = new SqlCommandBuilder(da);

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
            SqlDataAdapter da = null;
            try
            {
                SqlTransaction sqlTransaction = (SqlTransaction)pDbTransaction;
                //create a command and prepare it for execution
                opCommand = new SqlCommand();
                PrepareCommand((SqlCommand)opCommand, sqlTransaction.Connection, sqlTransaction, CommandType.Text, pCommandText, pCommandParameters);

                //create the DataAdapter & CommandBuilder
                da = new SqlDataAdapter((SqlCommand)opCommand);
                SqlCommandBuilder cb = new SqlCommandBuilder(da);
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
        #endregion GetDataAdapter

        #region SetUpdatedEventHandler
        internal static void SetUpdatedEventHandler(SqlDataAdapter pDa, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            pDa.RowUpdating += new SqlRowUpdatingEventHandler(pRowUpdating); 
            pDa.RowUpdated += new SqlRowUpdatedEventHandler(pRowUpdated);
        }
        #endregion SetUpdatedEventHandler
        #region Update
        public static int Update(SqlDataAdapter pDa, DataTable pDataTable, RowUpdating pRowUpdating, RowUpdated pRowUpdated)
        {
            pDa.RowUpdating += new SqlRowUpdatingEventHandler(pRowUpdating);
            pDa.RowUpdated += new SqlRowUpdatedEventHandler(pRowUpdated);
            return pDa.Update(pDataTable);
        }
        #endregion Update
        #region Fill
        public static int Fill(SqlDataAdapter pDa, DataSet pDataSet, string pTableName)
        {
            return pDa.Fill(pDataSet, pTableName);
        }
        #endregion Fill
        #region SetSelectCommandText
        public static void SetSelectCommandText(SqlDataAdapter pDa, string pQuery)
        {
            if (null != pDa.SelectCommand)
                pDa.SelectCommand.CommandText = pQuery;
        }
        #endregion SetSelectCommandText

        #region BulkCopy
        /// <summary>
        /// Cr�ation d'un objet BulkCopy
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New
        internal static SqlBulkCopy BulkCopy(string pConnectionString)
        {
            return new SqlBulkCopy(pConnectionString);
        }
        
        /// <summary>
        /// Cr�ation d'un objet BulkCopy
        /// </summary>
        /// <param name="pConnection"></param>
        /// <returns></returns>
        // PM 20200102 [XXXXX] New
        internal static SqlBulkCopy BulkCopy(SqlConnection pConnection)
        {
            return new SqlBulkCopy(pConnection);
        }

        /// <summary>
        /// Ecriture des donn�es d'un DataTable via BulkCopy
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pDestinationTable"></param>
        /// <param name="pDt"></param>
        // PM 20200102 [XXXXX] New
        internal static void BulkWriteToServer(string pConnectionString, string pDestinationTable, DataTable pDt)
        {
            using (SqlBulkCopy bulkCopy = BulkCopy(pConnectionString))
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
	/// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
	/// ability to discover parameters for stored procedures at run-time.
	/// </summary>
	public sealed class SqlHelperParameterCache
	{
		#region private methods, variables, and constructors

		//Since this class provides only static methods, make the default constructor private to prevent 
		//instances from being created with "new SqlHelperParameterCache()".
		private SqlHelperParameterCache() {}

		private readonly static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

		/// <summary>
		/// resolve at run time the appropriate set of SqlParameters for a stored procedure
		/// </summary>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="includeReturnValueParameter">whether or not to include their return value parameter</param>
		/// <returns></returns>
		private static SqlParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
		{
			using (SqlConnection cn = new SqlConnection(connectionString)) 
			using (SqlCommand cmd = new SqlCommand(spName, cn))
			{
				cn.Open();
				cmd.CommandType = CommandType.StoredProcedure;

				SqlCommandBuilder.DeriveParameters(cmd);

				if (!includeReturnValueParameter) 
				{
					cmd.Parameters.RemoveAt(0);
				}

				SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];;

				cmd.Parameters.CopyTo(discoveredParameters, 0);

				return discoveredParameters;
			}
		}

		//deep copy of cached SqlParameter array
		private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
		{
			SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

			for (int i = 0, j = originalParameters.Length; i < j; i++)
			{
				clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
			}

			return clonedParameters;
		}

		#endregion private methods, variables, and constructors

		#region caching functions

		/// <summary>
		/// add parameter array to the cache
		/// </summary>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <param name="commandParameters">an array of SqlParamters to be cached</param>
		internal static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
		{
			string hashKey = connectionString + ":" + commandText;

			paramCache[hashKey] = commandParameters;
		}

		/// <summary>
		/// retrieve a parameter array from the cache
		/// </summary>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="commandText">the stored procedure name or T-SQL command</param>
		/// <returns>an array of SqlParamters</returns>
		internal static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
		{
			string hashKey = connectionString + ":" + commandText;

			SqlParameter[] cachedParameters = (SqlParameter[])paramCache[hashKey];
			
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
		/// Retrieves the set of SqlParameters appropriate for the stored procedure
		/// </summary>
		/// <remarks>
		/// This method will query the database for this information, and then store it in a cache for future requests.
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <returns>an array of SqlParameters</returns>
		internal static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
		{
			return GetSpParameterSet(connectionString, spName, false);
		}

		/// <summary>
		/// Retrieves the set of SqlParameters appropriate for the stored procedure
		/// </summary>
		/// <remarks>
		/// This method will query the database for this information, and then store it in a cache for future requests.
		/// </remarks>
		/// <param name="connectionString">a valid connection string for a SqlConnection</param>
		/// <param name="spName">the name of the stored procedure</param>
		/// <param name="includeReturnValueParameter">a bool value indicating whether the return value parameter should be included in the results</param>
		/// <returns>an array of SqlParameters</returns>
		internal static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
		{
			string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter":"");

			SqlParameter[] cachedParameters;
			
			cachedParameters = (SqlParameter[])paramCache[hashKey];

			if (cachedParameters == null)
			{			
				cachedParameters = (SqlParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
			}
			
			return CloneParameters(cachedParameters);
		}

		#endregion Parameter Discovery Functions

	}
}

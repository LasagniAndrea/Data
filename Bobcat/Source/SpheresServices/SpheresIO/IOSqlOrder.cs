using System;
using System.Collections;
using System.Reflection;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.MQueue;
using EFS.Common.Log; 

using EfsML.DynamicData;

namespace EFS.SpheresIO
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlOrder
    {
        #region Members
        private string m_Cs;
        private IDbTransaction m_DbTransaction;
        private IDataReader m_DataReader;
        //
        private string m_Request_S;
        private string m_Request_I;
        private string m_Request_U;
        private string m_Request_D;
        //
        private DataParameters m_ParamCommande_S;
        private DataParameters m_ParamCommande_I;
        private DataParameters m_ParamCommande_U;
        private DataParameters m_ParamCommande_D;
        //
        private bool m_IsInsertUpdateControl;
        //
        private Cst.ErrLevel m_codeReturn;
        private ProcessLogInfo m_LogInfo;
        //
        private readonly string m_TableName;

        private readonly Task m_Task;
        private readonly IOTaskDetInOutFileRowTable m_RowTable;
        private readonly IOTaskDetInOutFileRow m_Row;
        #endregion Members

        #region Accessors
        public IDataReader DataReader
        {
            get { return m_DataReader; }
        }
        public Cst.ErrLevel CodeReturn
        {
            get { return m_codeReturn; }
            set { m_codeReturn = value; }
        }
        
        
        public IDbTransaction DbTransaction
        {
            get { return m_DbTransaction; }
            set { m_DbTransaction = value; }
        }
        public bool IsInsertUpdateControl
        {
            get { return m_IsInsertUpdateControl; }
        }
        public bool IsQuoteTable
        {
            get { return m_TableName.StartsWith("QUOTE_") && m_TableName.EndsWith("_H"); }
        }
        #endregion

        #region Constructors
        public SqlOrder(IDbTransaction pDbTransaction, Task pTask, IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTable pRowTable)
        {
            m_Cs = pTask.Cs;
            m_DbTransaction = pDbTransaction;
            m_Task = pTask;
            //
            m_Row = pRow;
            m_RowTable = pRowTable;
            //
            m_TableName = pRowTable.name.ToUpper();
            //
            m_IsInsertUpdateControl = false;
        }

        /// <summary>
        ///  Constructeur dans le cadre de l'exportation d'une table/vue ou d'un requête SQL
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pType"></param>
        /// <param name="pDataName"></param>
        /// <param name="pIOTask"></param>
        /// FI 20130504[] Refactoring 
        public SqlOrder(string pCs, CommandType pType, string pDataName, IOTask pIOTask)
        {
            switch (pType)
            {
                case CommandType.Text:
                    SetReaderFromCommand(pCs, pDataName, pIOTask);
                    break;
                case CommandType.TableDirect:
                    SetReaderFromTable(pCs, pDataName, pIOTask);
                    break;
            }
        }
        
        /// <summary>
        ///  Alimente m_DataReader à partir d'un select dur une table
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTableOrView"></param>
        /// <param name="pIOTask"></param>
        /// FI 20130503[] Add method (code issu de l'ancien constructor)
        private void SetReaderFromTable(string pCs, string pTableOrView, IOTask pIOTask)
        {
            string tableOrView = pTableOrView;

            m_Cs = pCs;

            //Add object owner (dbo by default)
            int posPoint = tableOrView.IndexOf(".");
            if (posPoint < 0)
                pTableOrView = SQLCst.DBO + pTableOrView;
            else
                tableOrView = pTableOrView.Substring(posPoint + 1);

            //Get restrict parameters
            GetWhereForSelect(m_Cs, pIOTask, out string sqlWhere, out DataParameters paramSQL);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SQL_ANSI + Cst.CrLf;
            sqlQuery += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + pTableOrView + Cst.CrLf;
            sqlQuery += sqlWhere;

            QueryParameters qryParameters = new QueryParameters(m_Cs, sqlQuery.ToString(), paramSQL);

            m_DataReader = DataHelper.ExecuteReader(m_Cs, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters?.GetArrayDbParameter());
            m_DataReader.GetSchemaTable().TableName = tableOrView;
        }

        /// <summary>
        ///  Alimente m_DataReader à partir d'un select dur une table
        /// <para>La requête et les paramètres doivent être au format EFS (Utilisation de @)</para> 
        /// <para> - @ doit être utilisé pour les paramètres</para>
        /// <para> - isnull et autres fonctions peuvent être utilisés</para>
        /// </summary>
        /// <param name="pIOTask"></param>
        /// <param name="pCommand"></param>
        /// <param name="pCs"></param>
        /// FI 20130503[] Add method
        /// FI 20131119[] Codage en dur spécifique pour la tâche d'export des CA
        private void SetReaderFromCommand(string pCs, string pCommand, IOTask pIOTask)
        {
            m_Cs = pCs;

            SQL_IOTaskParams taskParams = new SQL_IOTaskParams(pCs, pIOTask.id);
            DataRow[] sqlParams = taskParams.Select("ISSQLPARAMETER = 1");

            DataParameters parameters = null;
            if (sqlParams.Length > 0)
            {
                parameters = new DataParameters();
                for (int i = 0; i < sqlParams.Length; i++)
                {
                    ParamData paramData = new ParamData
                    {
                        name = sqlParams[i]["IDIOPARAMDET"].ToString(),
                        datatype = sqlParams[i]["DATATYPE"].ToString()
                    };
                    paramData.value = pIOTask.GetTaskParamValue(paramData.name);

                    DataParameter dataParameter = paramData.GetDataParameter(pCs, null, CommandType.Text);
                    parameters.Add(dataParameter);
                }
            }

            QueryParameters qryParameters = new QueryParameters(m_Cs, pCommand, parameters);

            // FI 20131119 []  Codage en dur sur l'exportation des CA
            // Si aucune ligne n'est retourné par le jeu de résultat alors I/O execute une query qui retourne nécessairement 1 ligne
            // Usage de la query initiale avec un UNIONALL de manière à ce que la colonne soit détectée comme de type XML
            if (pIOTask.name == "Corporate Actions Output")
            {
                string queryFor1Row = @"select '<normMsgFactory/>' as BUILDINFO from DUAL";
                if (false == qryParameters.Query.Contains(queryFor1Row))
                {
                    DataTable dt = DataHelper.ExecuteDataTable(m_Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    if (ArrFunc.IsEmpty(dt.Rows))
                    {
                        string newQuery = qryParameters.Query + Cst.CrLf + SQLCst.UNIONALL + queryFor1Row;
                        qryParameters = new QueryParameters(pCs, newQuery, parameters);
                    }
                }
            }

            m_DataReader = DataHelper.ExecuteReader(m_Cs, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters?.GetArrayDbParameter());
            m_DataReader.GetSchemaTable().TableName = "COMMAND";
        }


        /// <summary>
        ///  Constructeur dans le cadre de l'exportation qui s'appuie sur une stored procedure
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pCs"></param>
        /// <param name="pSP"></param>
        /// <param name="paramSQL"></param>
        public SqlOrder(IOTask pIOTask, string pCs, string pSP, out DataParameters paramSQL)
        {
            m_Cs = pCs;

            paramSQL = GetTaskSqlParameterForSP(pIOTask);
            LoadDataFromSP(pSP, paramSQL);
        }


        #endregion Constructors

        #region Methods

        /// <summary>
        /// Valorisation des colonnes et construction des différentes requêtes SQL
        /// </summary>
        /// <param name="pActionId">Type d'action pour laquelle les requêtes SQL seront générées</param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <returns>Retourne False si la ligne est rejetée par un contrôle</returns>
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public bool GetQuery(IOCommonTools.SqlAction pActionId, SetErrorWarning pSetErrorWarning)
        {
            return GetQuery(pActionId, false, pSetErrorWarning);
        }

        /// <summary>
        /// Valorisation des colonnes et construction des différentes requêtes SQL
        /// </summary>
        /// <param name="pActionId">Type d'action pour laquelle les requêtes SQL seront générées</param>
        /// <param name="pIsOnlyControl">Pour indiquer si les colonnes ont été déjà valorisées, et il faudrait uniquement exécuter les contrôles</param>
        /// <param name="pSetErrorWarning">Delegue pour inscrire un warning ou une erreur</param>
        /// <returns>Retourne False si la ligne est rejetée par un contrôle</returns>
        /// FI 20201221 [XXXXX] Add pSetErrorWarning
        public bool GetQuery(IOCommonTools.SqlAction pActionId, bool pIsOnlyControl, SetErrorWarning pSetErrorWarning)
        {
            // RD 20100305 / Optimisation de l'algorithme de génération des requêtes SQL
            m_ParamCommande_S = null;
            m_ParamCommande_I = null;
            m_ParamCommande_U = null;
            m_ParamCommande_D = null;

            m_Request_S = string.Empty;
            m_Request_I = string.Empty;
            m_Request_U = string.Empty;
            m_Request_D = string.Empty;

            #region Variables
            string sqlUpdate = string.Empty;
            string sqlInsertColumn = string.Empty;
            string sqlInsertValues = string.Empty;
            //
            string sqlWhereNotExistsDataKey = SQLCst.WHERE + SQLCst.NOT_EXISTS_SELECT +
                                              m_TableName + SQLCst.WHERE; // BD 20130610: Gestion des datakey lors de l'insert
            string sqlWhereDataKey = SQLCst.WHERE;
            string sqlWhereUpdKey = string.Empty;
            string columnValue = string.Empty;
            //
            bool isRowRejected = false;

            //
            bool isInsertUpdate = (pActionId == IOCommonTools.SqlAction.IU);
            bool isInsert = (isInsertUpdate || pActionId == IOCommonTools.SqlAction.I);
            bool isUpdate = (isInsertUpdate || pActionId == IOCommonTools.SqlAction.U);
            bool isDelete = (pActionId == IOCommonTools.SqlAction.D);
            #endregion

            m_ParamCommande_S = new DataParameters();
            m_ParamCommande_I = new DataParameters();
            m_ParamCommande_U = new DataParameters();
            m_ParamCommande_D = new DataParameters();

            ArrayList rowTableMsgLog = IOTools.RowTableDesc(m_Row, m_RowTable);

            bool isRowWithParameters = false;
            // RD 20100728 [17101] Input: Use parameters in Post Mapping XML flow                
            if (false == pIsOnlyControl)
            {
                //valorisation des parameters de la row
                isRowWithParameters = ValuateParameters(m_Row.parameters, pActionId, pIsOnlyControl, ref isRowRejected, rowTableMsgLog, pSetErrorWarning);
                // Le "row" peut être rejeté par un "parameter"
                if (isRowRejected)
                    return false;
            }


            for (int mappColumnId = 0; (mappColumnId <= m_RowTable.column.Length - 1) && !isRowRejected; mappColumnId++)
            {
                #region Initialisation des infos de la colonne
                IOTaskDetInOutFileRowTableColumn column = m_RowTable.column[mappColumnId];

                string columnName = column.name.ToUpper();
                string columnDataType = column.datatype;
                //columnDataType doit maintenant être renseigné => alimentation de columnDataType si non renseigné, pour compatibilité ascendante.
                if (String.IsNullOrEmpty(columnDataType))
                    columnDataType = "string";

                string columnDataFormat = column.dataformat;
                string columnDataKey = column.datakey;
                columnDataKey = (StrFunc.IsFilled(columnDataKey) ? columnDataKey : "FALSE");

                string columnDataKeyUpd = column.datakeyupd;
                columnDataKeyUpd = (StrFunc.IsFilled(columnDataKeyUpd) ? columnDataKeyUpd : "FALSE");

                string resultAction = string.Empty;
                ArrayList columnMesgLog = new ArrayList(rowTableMsgLog)
                {
                    $" Column (<b>{columnName}</b>)"
                };
                #endregion

                // RD 20100728 [17101] Input: Use parameters in Post Mapping XML flow
                bool isColumnWithParameters = false;
                if (false == pIsOnlyControl)
                {
                    //valorisation des parameters de la colonne
                    isColumnWithParameters = ValuateParameters(column.parameters, pActionId, pIsOnlyControl, ref isRowRejected, columnMesgLog, pSetErrorWarning);
                }

                // Le "row" peut être rejeté par un "parameter"
                if (false == isRowRejected)
                {
                    #region Valorisation des colonnes et complément des requêtes
                    // RD 20100728 [17101] Input: Use parameters in Post Mapping XML flow
                    // RD 20100728 [17097] Input: XML Column
                    // RD 20110706 Problème de perf
                    if (false == pIsOnlyControl)
                    {
                        #region Colonne XML ou Row/Colonne avec paramètres: Valorisation des colonnes avec les parameters et valueXML avec DynamicData
                        bool isColumnValueXML = IsColumnValueXML(column);
                        XmlDocument xmlDocumentColumn = LoadXMLDocument(column, isColumnWithParameters, isRowWithParameters);

                        // RD 20110629
                        // Gestion des Paramétres dans mQueue
                        // Utilisation d'une méthode commune: IOTools.ParametersTools.ValuateDataWithParameters()
                        //
                        #region Valorisation des éléments qui font référence à des parameters
                        if (isColumnWithParameters || isRowWithParameters)
                        {
                            // PM 20180219 [23824] Déplacé de IOTools vers IOCommonTools
                            //IOTools.ParametersTools.ValuateDataWithParameters(m_Row, column, xmlDocumentColumn);
                            IOCommonTools.ParametersTools.ValuateDataWithParameters(m_Row, column, xmlDocumentColumn);
                        }
                        #endregion

                        if (IsColumnValueXML(m_RowTable.column[mappColumnId]))
                        {
                            #region Colonne de Type XML
                            // Valoriser les éléments SQL et SpheresLib au seins du flux XML
                            IOTools.ValuateDynamicDataInXml(m_Task, xmlDocumentColumn, pSetErrorWarning);

                            if (column.isColumnMQueue && xmlDocumentColumn.DocumentElement.Name == "normMsgFactory")
                                QueueNormMsgFactory(m_Task, column, xmlDocumentColumn, m_Row.parameters);

                            // Mettre à jour la valeur de la colonne avec le nouveau flux ValueXML 
                            // Notter que la valeur du départ est "valueXML", pour enfin valoriser "value":
                            // c'est fait pour pouvoir importer des flux XML dans une colonne XML (Ticket [17097])
                            m_RowTable.column[mappColumnId].value = xmlDocumentColumn.OuterXml;
                            #endregion
                        }
                        else if (isColumnWithParameters || isRowWithParameters)
                        {
                            // Si des Parameters existent, donc la valeur de la colonne a eventuellement changée, 
                            // alors mettre à jour la colonne avec le nouveau flux
                            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(IOTaskDetInOutFileRowTableColumn), xmlDocumentColumn.OuterXml);
                            m_RowTable.column[mappColumnId] = (IOTaskDetInOutFileRowTableColumn)CacheSerializer.Deserialize(serializeInfo);
                        }
                        #endregion
                    }

                    // Valorisation de la colonne
                    ProcessMapping.GetMappedDataValue(m_Task, pSetErrorWarning, m_RowTable.column[mappColumnId], m_DbTransaction, pActionId,
                        ref columnValue, ref resultAction, ref m_IsInsertUpdateControl, pIsOnlyControl, columnMesgLog);

                    if (resultAction == "REJECTROW")
                    {
                        isRowRejected = true;
                    }
                    else
                    {
                        #region Compléter les requêtes et les parameters SQL correspondants
                        DataParameter columnDataParameter = GetDataParameter(m_Cs, columnName, columnDataType, columnDataFormat, columnValue);
                        //                                            
                        if (resultAction != "REJECTCOLUMN")
                        {
                            #region Inclure la colonne dans la requête
                            if (m_IsInsertUpdateControl && isInsertUpdate)
                            {
                                if (StrFunc.IsFilled(sqlUpdate))
                                {
                                    sqlUpdate = string.Empty;
                                    m_ParamCommande_U.Clear();
                                    m_ParamCommande_U = null;
                                }
                                //
                                if (StrFunc.IsFilled(sqlInsertColumn))
                                {
                                    sqlInsertColumn = string.Empty;
                                    sqlInsertValues = string.Empty;
                                    m_ParamCommande_I.Clear();
                                    m_ParamCommande_I = null;
                                }
                            }
                            else
                            {
                                if (isUpdate)
                                {
                                    sqlUpdate += columnName + "=@" + columnName + ", ";
                                    m_ParamCommande_U.Add(columnDataParameter);
                                }
                                //
                                if (isInsert)
                                {
                                    sqlInsertColumn += columnName + ", ";
                                    sqlInsertValues += "@" + columnName + ", ";
                                    //
                                    m_ParamCommande_I.Add(columnDataParameter);
                                }
                            }
                            #endregion
                        }
                        //
                        if ((false == isInsert) || (true == isInsertUpdate))
                        {
                            #region clause Where et Parameter
                            if (columnDataKey.ToUpper() == "TRUE")
                            {
                                if (null == columnValue)
                                {
                                    sqlWhereDataKey += "(" + m_TableName + "." + columnName + SQLCst.IS_NULL + ")" + SQLCst.AND;
                                }
                                else
                                {
                                    sqlWhereDataKey += "(" + m_TableName + "." + columnName + "=@" + columnName + ")" + SQLCst.AND;

                                    if (isDelete)
                                    {
                                        m_ParamCommande_D.Add(columnDataParameter);
                                    }

                                    if (isInsertUpdate)
                                    {
                                        m_ParamCommande_S.Add(columnDataParameter);
                                    }

                                    if (isUpdate && (resultAction == "REJECTCOLUMN") && (false == (m_IsInsertUpdateControl && isInsertUpdate)))
                                    {
                                        m_ParamCommande_U.Add(columnDataParameter);
                                    }
                                }
                            }
                            else
                            {
                                if ((isInsertUpdate || isUpdate) && (columnDataKeyUpd.ToUpper() == "TRUE"))
                                {
                                    if (!Convert.IsDBNull(columnDataParameter.Value))
                                    {
                                        //EG 20070817 Ticket 15649
                                        string nullSubstitute = GetNullSubstituteByDataType(columnDataType);
                                        //PL 20130225 Add parenthesis
                                        sqlWhereUpdKey += "(" + DataHelper.SQLIsNull(m_Cs, m_TableName + "." + columnName, nullSubstitute) + "!=" + DataHelper.SQLIsNull(m_Cs, "@" + columnName, nullSubstitute) + ")";

                                        if (isInsertUpdate)
                                        {
                                            m_ParamCommande_S.Add(columnDataParameter);
                                        }
                                        else if (isUpdate && (resultAction == "REJECTCOLUMN") && (false == (m_IsInsertUpdateControl && isInsertUpdate)))
                                        {
                                            m_ParamCommande_U.Add(columnDataParameter);
                                        }
                                    }
                                    else
                                    {
                                        sqlWhereUpdKey += "(" + m_TableName + "." + columnName + SQLCst.IS_NOT_NULL + ")";
                                    }
                                    sqlWhereUpdKey += Cst.CrLf + SQLCst.OR;
                                }
                            }
                            #endregion
                        }
                        else if ((true == isInsert) && (false == isInsertUpdate))
                        {
                            /* BD 20130610: Gestion des datakey lors de l'insert */
                            // EG 20130626 Test sur columnValue = null
                            if (columnDataKey.ToUpper() == "TRUE")
                            {
                                if (null == columnValue)
                                {
                                    sqlWhereNotExistsDataKey += m_TableName + "." + columnName + SQLCst.IS_NULL + SQLCst.AND;
                                }
                                else
                                {
                                    sqlWhereNotExistsDataKey += m_TableName + "." + columnName + "=@" + columnName + SQLCst.AND;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }

            if (false == isRowRejected)
            {
                #region Finaliser les requêtes
                if (isInsert && (false == (m_IsInsertUpdateControl && isInsertUpdate)))
                {
                    sqlInsertColumn = sqlInsertColumn.Trim().TrimEnd(',');
                    sqlInsertValues = sqlInsertValues.Trim().TrimEnd(',');

                    m_Request_I = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.INSERT_INTO_DBO + m_TableName;
                    m_Request_I += " ( " + sqlInsertColumn + ") ";

                    /* BD 20130610: Gestion des datakey lors de l'insert */
                    if (sqlWhereNotExistsDataKey.Contains(SQLCst.AND))
                    {
                        sqlWhereNotExistsDataKey = sqlWhereNotExistsDataKey.TrimEnd(SQLCst.AND.ToString().ToCharArray()) + ")";
                        m_Request_I += "select " + sqlInsertValues + DataHelper.SQLFromDual(m_Cs) + sqlWhereNotExistsDataKey;
                    }
                    else
                    {

                        m_Request_I += "values (" + sqlInsertValues + ")";
                    }
                }

                if ((false == isInsert) || (true == isInsertUpdate))
                {
                    if (sqlWhereDataKey == SQLCst.WHERE)
                    {
                        sqlWhereDataKey = string.Empty;
                    }
                    else
                    {
                        sqlWhereDataKey = sqlWhereDataKey.TrimEnd(SQLCst.AND.ToString().ToCharArray());
                    }
                    if (StrFunc.IsFilled(sqlWhereUpdKey) && (isInsertUpdate || isUpdate))
                    {
                        sqlWhereUpdKey = "(" + sqlWhereUpdKey.TrimEnd(SQLCst.OR.ToString().ToCharArray()) + ")";
                    }
                }

                if (isInsertUpdate)
                {
                    m_Request_S = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT + Cst.CrLf;

                    if (StrFunc.IsFilled(sqlWhereUpdKey))
                    {
                        //PL 20130225 New features (à finaliser au retour de RD)
                        //if (m_TableName == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString())
                        //{
                        //    //Table DERIVATIVECONTRACT: Update autorisé uniquement si ISAUTOSETTING=1
                        //    sqlWhereUpdKey += SQLCst.AND + "(ISAUTOSETTING=1)";
                        //}

                        m_Request_S += SQLCst.CASE + Cst.CrLf;
                        m_Request_S += SQLCst.CASE_WHEN + Cst.CrLf + sqlWhereUpdKey + SQLCst.CASE_THEN + "1" + Cst.CrLf;
                        m_Request_S += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                        m_Request_S += SQLCst.CASE_END + "as Result";
                    }
                    else
                    {
                        m_Request_S += "1 as Result";
                    }

                    //PL 20140918 Newness
                    string join = string.Empty;
                    if (IsQuoteTable)
                    {
                        //Cas particulier des historiques de cotations (ex. QUOTE_ETD_H)
                        m_Request_S += "," + m_TableName + ".SOURCE,e.CUSTOMVALUE";
                        join = SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ENUM.ToString() + " e on (e.CODE='InformationProvider') and (e.VALUE=" + m_TableName + ".SOURCE)" + Cst.CrLf;
                    }

                    m_Request_S += Cst.CrLf + SQLCst.FROM_DBO + m_TableName + Cst.CrLf + join + sqlWhereDataKey;
                }

                if (isUpdate && StrFunc.IsFilled(sqlUpdate) && (false == (m_IsInsertUpdateControl && isInsertUpdate)))
                {
                    m_Request_U = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.UPDATE_DBO + m_TableName + Cst.CrLf + SQLCst.SET + sqlUpdate.Trim().TrimEnd(',') + Cst.CrLf + sqlWhereDataKey;

                    if (false == isInsertUpdate && StrFunc.IsFilled(sqlWhereUpdKey))
                    {
                        m_Request_U += (StrFunc.IsFilled(sqlWhereDataKey) ? Cst.CrLf + SQLCst.AND : SQLCst.WHERE) + sqlWhereUpdKey;
                    }
                }

                if (isDelete)
                {
                    m_Request_D = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.DELETE_DBO + m_TableName + Cst.CrLf + sqlWhereDataKey;
                }
                #endregion

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Retourne la valeur du paramètre SQL {pParamName} dans la requête associée à la commande {pAction}
        /// </summary>
        /// <param name="pAction"></param>
        /// <param name="pParamName"></param>
        /// <returns></returns>
        public object GetDataParameterValue(IOCommonTools.SqlAction pAction, string pParamName)
        {
            Object ret = null;
            DataParameters paramCommande = null;
            //
            GetActionRequest(pAction, ref paramCommande);
            //
            if (paramCommande != null && paramCommande.Contains(pParamName))
                ret = paramCommande[pParamName].Value;
            //
            return ret;
        }

        /// <summary>
        /// Retourne une nouvelle isntance d'un paramètre d'une requête SQL
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pColumnDataType"></param>
        /// <param name="pColumnDataFormat"></param>
        /// <param name="pColumnValue"></param>
        /// <returns></returns>
        private static DataParameter GetDataParameter(string pCS, string pColumnName, string pColumnDataType, string pColumnDataFormat, string pColumnValue)
        {
            ParamData paramData = new ParamData
            {
                name = pColumnName,
                datatype = pColumnDataType,
                dataformat = pColumnDataFormat,
                value = pColumnValue
            };
            return paramData.GetDataParameter(pCS, null, CommandType.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAction"></param>
        /// <param name="pParamCommande"></param>
        /// <returns></returns>
        public string GetActionRequest(IOCommonTools.SqlAction pAction, ref DataParameters pParamCommande)
        {
            string request = string.Empty;
            //
            switch (pAction)
            {
                case IOCommonTools.SqlAction.S:
                    request = m_Request_S;
                    pParamCommande = m_ParamCommande_S;
                    break;
                case IOCommonTools.SqlAction.I:
                    request = m_Request_I;
                    pParamCommande = m_ParamCommande_I;
                    break;
                case IOCommonTools.SqlAction.U:
                    request = m_Request_U;
                    pParamCommande = m_ParamCommande_U;
                    break;
                case IOCommonTools.SqlAction.D:
                    request = m_Request_D;
                    pParamCommande = m_ParamCommande_D;
                    break;
            }
            //
            return request;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAction"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(IOCommonTools.SqlAction pAction)
        {
            DataParameters dataParameters = null;
            string request = GetActionRequest(pAction, ref dataParameters);

            QueryParameters qryParameters = new QueryParameters(m_Cs, request, dataParameters);

            if (null != m_DbTransaction)
                return DataHelper.ExecuteNonQuery(m_DbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters?.GetArrayDbParameter());
            else
                return DataHelper.ExecuteNonQuery(m_Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters?.GetArrayDbParameter());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAction"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(IOCommonTools.SqlAction pAction)
        {
            DataParameters dataParameters = null;
            string request = GetActionRequest(pAction, ref dataParameters);
            if (null != m_DbTransaction)
                return DataHelper.ExecuteReader(m_DbTransaction, CommandType.Text, request, dataParameters?.GetArrayDbParameter());
            else
                return DataHelper.ExecuteReader(m_Cs, CommandType.Text, request, dataParameters?.GetArrayDbParameter());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnDataType"></param>
        /// <returns></returns>
        private string GetNullSubstituteByDataType(string pColumnDataType)
        {
            string ret = string.Empty;
            //
            if (TypeData.IsTypeString(pColumnDataType))
                ret = DataHelper.SQLString("X");
            else if (TypeData.IsTypeNumeric(pColumnDataType) || TypeData.IsTypeBool(pColumnDataType))
                ret = "0";
            else if (TypeData.IsTypeDateOrDateTime(pColumnDataType))
                ret = DataHelper.SQLGetDate(m_Cs);
            //
            return ret;
        }

        /// <summary>
        /// Retourne une restrition where et les parameters utilisés par cette restriction en fonction des paramètres de la tâche
        /// <para>Seuls les paramètres (ISSQLPARAMETER = 1) de la tâche sont considérés</para>
        /// </summary>
        /// <param name="pIOTask"></param>
        /// <param name="opSqlWhere"></param>
        /// <returns></returns>
        /// FI 20130424 [] Refactoring de la méthode 
        private static DataParameters GetWhereForSelect(string pCS, IOTask pIOTask, out string pSqlWhere, out DataParameters pDataParameters)
        {
            pDataParameters = null;
            pSqlWhere = string.Empty;

            SQL_IOTaskParams taskParams = new SQL_IOTaskParams(pCS, pIOTask.id);
            DataRow[] sqlParams = taskParams.Select("ISSQLPARAMETER = 1");
            if (sqlParams.Length > 0)
            {
                const string STRING_SEPARATOR = ";";

                SQLWhere sqlWhere = new SQLWhere();
                DataParameters paramSQL = new DataParameters();

                for (int i = 0; i < sqlParams.Length; i++)
                {
                    ParamData paramData = new ParamData
                    {
                        name = sqlParams[i]["IDIOPARAMDET"].ToString(),
                        datatype = sqlParams[i]["DATATYPE"].ToString(),
                        value = pIOTask.GetTaskParamValue(sqlParams[i]["IDIOPARAMDET"].ToString())
                    };
                    bool paramIsMandatory = BoolFunc.IsTrue(sqlParams[i]["ISMANDATORY"].ToString());
                    paramSQL.Add(paramData.GetDataParameter(pCS, null, CommandType.StoredProcedure));

                    if ((false == paramIsMandatory) && StrFunc.IsEmpty(paramData.value))
                    {
                        // un paramètre vide et non renseigné est supprimé de al collection des paramètres et ne donne lia à aucune restriction
                        paramSQL.Remove(paramData.name);
                    }
                    else
                    {
                        if (null == paramData.GetDataValue(pCS, null)) // Lorsque l'utilisateur saisi null 
                        {
                            sqlWhere.Append(paramData.name + " is null", true);
                            paramSQL.Remove(paramData.name);
                        }
                        else if (TypeData.IsTypeString(paramData.datatype) && (paramData.value.IndexOf(STRING_SEPARATOR) > 0))
                        {
                            //String multi-value with semi-colon separator, on exclue le paramètre par une "astuce" (cf ci-dessus)
                            string[] tmpValues = paramData.value.Split(STRING_SEPARATOR.ToCharArray());
                            string listValues = DataHelper.SQLCollectionToSqlList(pCS, tmpValues, TypeData.TypeDataEnum.@string);
                            sqlWhere.Append(paramData.name + " in (" + listValues + ")", true);
                            //
                            paramSQL.Remove(paramData.name);
                        }
                    }
                    //
                    if (paramSQL.Contains(paramData.name))
                        sqlWhere.Append(paramData.name + SQLCst.LIKE + DataHelper.GetVarPrefix(pCS) + paramData.name, true);
                }

                pSqlWhere = sqlWhere.ToString();
                pDataParameters = paramSQL;
            }
            return pDataParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <returns></returns>
        /// 20070711 PL public to private
        /// GP 20070125. Ajout du paramètre int pUserId. Cela sert à GetTaskParameterForSP pour bien appeler 
        /// CheckDynamicString (...)	
        /// GP 20070202 et PL. Ajout des 4 arguments ref int opPosition<ReturnType>. 
        private DataParameters GetTaskSqlParameterForSP(IOTask pXMLTask)
        {
            //Warning Oracle: (20070124 PL)
            // - Sous Oracle, le dataset récupère le jeu de donnés via un paramètre out de type cursor. 
            //   On ajoute donc systématiquement ici (en dur) un 1er paramètre out de type cursor (il ne faut donc pas le définir dans le paramétrage des paramètres associés à une tâche).
            // - Sous SqlServer, bien qu'une SP puisse retourner aussi un paramètre out de type cursor, .Net 1.1 n'offre pas de type cursor pour SqlServer.
            //   On fonctionne donc différemment, il suffit que le dernier ordre SQL de la SP soit un select pour récupérer dans le dataset le jeu de résultat.


            DataParameters ret = new DataParameters();
            //
            SQL_IOTaskParams taskParams = new SQL_IOTaskParams(m_Cs, pXMLTask.id);
            string sqlFiltre = "ISSQLPARAMETER = 1";
            DataRow[] sqlParams = taskParams.Select(sqlFiltre);
            //
            bool isOracle = DataHelper.IsDbOracle(m_Cs);
            if (isOracle)
            {
                DataParameter paramCursor = new DataParameter();
                paramCursor.InitializeOracleCursor("curs", ParameterDirection.Output);
                ret.Add(paramCursor);
            }
            //
            if (ArrFunc.IsFilled(sqlParams))
            {
                for (int i = 0; i < sqlParams.Length; i++)
                {
                    ParameterDirection paramDirectionEnum = ParameterDirection.Input;
                    if (StrFunc.IsFilled(sqlParams[i]["DIRECTION"].ToString()))
                        paramDirectionEnum = (ParameterDirection)Enum.Parse(typeof(ParameterDirection), sqlParams[i]["DIRECTION"].ToString(), true);

                    ParamData paramData = new ParamData
                    {
                        name = sqlParams[i]["IDIOPARAMDET"].ToString(),
                        datatype = sqlParams[i]["DATATYPE"].ToString(),
                        direction = paramDirectionEnum,
                        value = pXMLTask.GetTaskParamValue(sqlParams[i]["IDIOPARAMDET"].ToString())
                    };

                    ret.Add(paramData.GetDataParameter(m_Cs, null, System.Data.CommandType.StoredProcedure));
                }
            }
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSP"></param>
        /// <param name="pParamSP"></param>
        private void LoadDataFromSP(string pSP, DataParameters pParamSP)
        {
            //20081119 PL/EG ATTENTION 
            //m_DataReader = DataHelper.ExecuteReader(m_Cs, CommandType.StoredProcedure, pSP, pParamSP.GetArrayDbParameter());
            m_DataReader = DataHelper.ExecuteReader(m_Cs, CommandType.StoredProcedure, pSP, pParamSP.GetArrayDbParameter());
        }

        
        /// <summary>
        /// Evaluation des paramètres d'un row ou d'une colonne
        /// <seealso cref="(voir ticket TRIM 17101 pour plus de détail)"/>
        /// </summary>
        /// <param name="pParameters">Représente les paramètres</param>
        /// <param name="pActionId"></param>
        /// <param name="pIsOnlyControl"></param>
        /// <param name="opIsRowRejected"></param>
        /// <param name="pMessage"></param>
        /// <param name="pSetErrorWarning"></param>
        /// <returns></returns>
        /// RD 20110629 
        /// Gestion des paramètres dans mQueue
        /// Utilisation d'une méthode commune: IOTools.ParametersTools.ValuateDataWithParameters()
        private bool ValuateParameters(IOMappedDataParameters pParameters, IOCommonTools.SqlAction pActionId, bool pIsOnlyControl, ref bool opIsRowRejected, ArrayList pMessage , SetErrorWarning pSetErrorWarning)
        {
            bool ret = (null!= pParameters) &&  ArrFunc.IsFilled(pParameters.parameter);
            if (ret)
            {
                foreach (IOMappedData param in pParameters.parameter)
                {
                    ArrayList paramMsgLog = new ArrayList(pMessage)
                    {
                        $"Parameter ({param.name})"
                    };

                    string dynamicDataResult = string.Empty;
                    string resultAction = string.Empty;
                    ProcessMapping.GetMappedDataValue(m_Task, pSetErrorWarning, param, m_DbTransaction, pActionId,
                        ref dynamicDataResult, ref resultAction, ref m_IsInsertUpdateControl, pIsOnlyControl, paramMsgLog);

                    if (resultAction == "REJECTROW")
                    {
                        opIsRowRejected = true;
                        break;
                    }
                    else
                    {
                        param.spheresLib = null;
                        param.sql = null;
                        param.value = dynamicDataResult;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Chargement d'un document XML avec
        /// <para>
        ///  - le contenu d'une colonne type XML  ou
        /// </para>
        /// <para>
        /// - le résultat de la serialization de la colonne (lorsque isRowWithParameters ou isColumnWithParameters)
        /// </para>
        /// </summary>
        /// <param name="pColumn"></param>
        /// <param name="isRowWithParameters"></param>
        /// <param name="isColumnWithParameters"></param>
        /// FI 20130426 [18344] 
        private static XmlDocument LoadXMLDocument(IOTaskDetInOutFileRowTableColumn pColumn, bool isRowWithParameters, bool isColumnWithParameters)
        {
            try
            {
                XmlDocument ret = null;

                string xmlColumn = string.Empty;
                if (IsColumnValueXML(pColumn))
                {
                    xmlColumn = pColumn.valueXML.Data;
                }
                else if (isColumnWithParameters || isRowWithParameters)
                {
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(IOTaskDetInOutFileRowTableColumn), pColumn);
                    xmlColumn = CacheSerializer.Serialize(serializeInfo).ToString();
                }
                if (StrFunc.IsFilled(xmlColumn))
                {
                    ret = new XmlDocument();
                    ret.LoadXml(xmlColumn);
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(@"<b>Error on loading the column in XML Document</b>", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumn"></param>
        /// <returns></returns>
        private static bool IsColumnValueXML(IOTaskDetInOutFileRowTableColumn pColumn)
        {
            return (TypeData.IsTypeXml(pColumn.datatype) && (null != pColumn.valueXML));
        }

        /// <summary>
        /// Remplace la ConnectionString et l'id d'un flux XML de type NormMsgFactoryMQueue
        /// <para></para>
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pColumn"></param>
        /// <param name="xmlDocumentColumn"></param>
        /// <param name="parameters"></param>
        /// FI 20130426 [18344] add Method
        private static void QueueNormMsgFactory(Task pTask, IOTaskDetInOutFileRowTableColumn pColumn, XmlDocument xmlDocumentColumn, IOMappedDataParameters parameters)
        {
            if (false == (pColumn.isColumnMQueue && xmlDocumentColumn.DocumentElement.Name == "normMsgFactory"))
                throw new NotSupportedException(StrFunc.AppendFormat("QueueNormMsgFactory Method is not supported for column {0}", pColumn.name));

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(NormMsgFactoryMQueue), xmlDocumentColumn.OuterXml);
            if (!(CacheSerializer.Deserialize(serializeInfo) is NormMsgFactoryMQueue queue))
                throw new Exception(StrFunc.AppendFormat("{0} is not a  NormMsgFactoryMQueue", pColumn.name));

            // Spheres® remplace l'id présent dans le flux normMsgFactory
            // Spheres® remplace l'id par 0
            queue.id = 0;
            queue.buildingInfo.id = 0;
            
            // Spheres® remplace l'id par le paramètre nommé ID s'il existe
            if ((null != parameters) && ArrFunc.IsFilled(parameters.parameter))
            {
                IOMappedData parameter = parameters.GetParameter("ID");
                if (null != parameter)
                {
                    int id = Convert.ToInt32(parameter.value);
                    queue.id = id;
                    queue.buildingInfo.id = id;
                }
            }

            // Spheres® remplace la CS présente dans le flux normMsgFactory
            queue.header.ConnectionString = pTask.Cs;
            queue.header.connectionString.cryptSpecified = true;
            queue.header.connectionString.crypt = true;
            queue.header.BuildConnectionStringValue();

            serializeInfo = new EFS_SerializeInfoBase(typeof(NormMsgFactoryMQueue), queue);
            string xmlFlow = CacheSerializer.Serialize(serializeInfo).ToString();

            xmlDocumentColumn.LoadXml(xmlFlow);
            
            // FI 20130513 [] Add supprsession de le declaration afin de ne pas planter lors de l'insert dans la table
            XMLTools.RemoveXmlDeclaration(xmlDocumentColumn);
        }

        #endregion
    }
}
#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Common.Web
{
    /// <summary>
    /// Description résumée de SQLSessionClipBoard.
    /// </summary>
    /// EG 20234429 [WI756] Spheres Core : Refactoring Code Analysis
    public class SessionClipBoard
    {
        #region Members
        private readonly string m_Cs;
        private readonly AppSession m_Session;
        #endregion Members

        #region Constructors
        public SessionClipBoard(string pSource, AppSession pSession)
        {
            m_Cs = pSource;
            m_Session = pSession;
        }
        #endregion Constructors
        #region Methods


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel Delete() { return Delete(0); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdClipBoard"></param>
        /// <returns></returns>
        public Cst.ErrLevel Delete(int pIdClipBoard)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;
            if (StrFunc.IsFilled(m_Cs))
            {
                bool isOneIdClipBoard = (0 < pIdClipBoard);
                SQLSessionClipBoard sqlClipBoard = new SQLSessionClipBoard(m_Cs);
                QueryParameters queryParameters = sqlClipBoard.SQLDelete(isOneIdClipBoard);
                if (isOneIdClipBoard)
                    queryParameters.Parameters["IDCLIPBOARD"].Value = pIdClipBoard;
                else
                    queryParameters.Parameters["SESSIONID"].Value = m_Session.SessionId;
                int nbDelete = DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                if (0 < nbDelete)
                    errLevel = Cst.ErrLevel.SUCCESS;
            }
            return errLevel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pObjectID"></param>
        /// <param name="pDisplayName"></param>
        /// <param name="pObjectXML"></param>
        /// <returns></returns>
        public Cst.ErrLevel Insert(string pObjectName, string pFieldName, string pObjectID, string pDisplayName, string pObjectXML)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (StrFunc.IsFilled(m_Cs))
            {
                SQLSessionClipBoard sqlClipBoard = new SQLSessionClipBoard(m_Cs);
                QueryParameters queryParameters = sqlClipBoard.SQLInsert();
                queryParameters.Parameters["SESSIONID"].Value = m_Session.SessionId;
                queryParameters.Parameters["OBJECTNAME"].Value = pObjectName;
                queryParameters.Parameters["FIELDNAME"].Value = pFieldName;
                queryParameters.Parameters["OBJECTID"].Value = pObjectID;
                queryParameters.Parameters["DISPLAYNAME"].Value = pDisplayName;
                queryParameters.Parameters["HOSTNAME"].Value = m_Session.AppInstance.HostName;
                queryParameters.Parameters["OBJECTXML"].Value = pObjectXML;
                // FI 20200820 [25468] Dates systemes en UTC
                queryParameters.Parameters["DTSYS"].Value = OTCmlHelper.GetDateSysUTC(m_Cs);
                queryParameters.Parameters["IDAINS"].Value = m_Session.IdA;
                queryParameters.Parameters["EXTLLINK"].Value = Convert.DBNull;
                queryParameters.Parameters["ROWATTRIBUT"].Value = Cst.RowAttribut_System;
                DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public DataSet Select(string pObjectName)
        {
            return Select(pObjectName, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pIDClipBoard"></param>
        /// <returns></returns>
        public DataSet Select(string pObjectName, int pIDClipBoard)
        {
            return Select(pObjectName, pIDClipBoard, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pIDClipBoard"></param>
        /// <param name="pDisplayName"></param>
        /// <returns></returns>
        public DataSet Select(string pObjectName, int pIDClipBoard, string pDisplayName)
        {
            DataSet dsSessionClipBoard = null;
            if (StrFunc.IsFilled(m_Cs))
            {
                bool isDisplayNameSpecified = StrFunc.IsFilled(pDisplayName);
                bool isIdSpecified = (0 < pIDClipBoard);
                SQLSessionClipBoard sqlClipBoard = new SQLSessionClipBoard(m_Cs);
                QueryParameters queryParameters = sqlClipBoard.SQLSelect(isIdSpecified, isDisplayNameSpecified);
                //
                queryParameters.Parameters["SESSIONID"].Value = m_Session.SessionId;
                queryParameters.Parameters["OBJECTNAME"].Value = pObjectName;
                //
                if (isIdSpecified)
                    queryParameters.Parameters["IDCLIPBOARD"].Value = pIDClipBoard;
                else if (isDisplayNameSpecified)
                    queryParameters.Parameters["DISPLAYNAME"].Value = pDisplayName;
                //
                dsSessionClipBoard = DataHelper.ExecuteDataset(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            }

            return dsSessionClipBoard;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public string GetItemClipBoard(string pObjectName)
        {
            return GetItemClipBoard(pObjectName, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <param name="pIdClipBoard"></param>
        /// <returns></returns>
        public string GetItemClipBoard(string pObjectName, int pIdClipBoard)
        {
            string xmlClipBoard = string.Empty;
            DataSet dsClipBoard = Select(pObjectName, pIdClipBoard);
            if ((null != dsClipBoard) && (0 < dsClipBoard.Tables[0].Rows.Count))
            {
                DataRow row = dsClipBoard.Tables[0].Rows[0];
                xmlClipBoard = row["OBJECTXML"].ToString();
            }

            return xmlClipBoard;
        }

        /// <summary>
        /// Supprime toutes les lignes présentes dans SESSIONCLIPBOARD pour une session donnée
        /// </summary>
        /// FI 20120907 [18113] add methode Dispose
        /// FI 20131108 [] Valorisation du paramètre SESSIONID 
        public static void CleanUp(string pCS, string pSessionId)
        {
            SQLSessionClipBoard sqlSessionClipBoard = new SQLSessionClipBoard(pCS);
            QueryParameters queryParameters = sqlSessionClipBoard.SQLDelete(false);
            queryParameters.Parameters["SESSIONID"].Value = pSessionId; 

            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
        }

        #endregion Methods
    }
	

	/// <summary>
	/// 
	/// </summary>
	public class SQLSessionClipBoard
	{
		#region Members
		private readonly string        m_Cs;
		private DataParameter m_ParamIdClipboard;
		private DataParameter m_ParamSessionId;
		private DataParameter m_ParamObjectName;
		private DataParameter m_ParamFieldName;
		private DataParameter m_ParamObjectID;
		private DataParameter m_ParamDisplayName;
		private DataParameter m_ParamHostName;
		private DataParameter m_ParamObjectXML;
		private DataParameter m_ParamDtSys;
		private DataParameter m_ParamIdAIns;
		private DataParameter m_ParamExtlLink;
		private DataParameter m_ParamRowAttribut;
		#endregion Members
		#region Accessors
		#endregion Accessors
		#region Constructors
		public SQLSessionClipBoard(string pCs)
		{
			m_Cs = pCs;
			InitParameter();
		}
        #endregion Constructors
        #region Methods
        #region InitParameter
        private void InitParameter()
        {
            m_ParamIdClipboard = new DataParameter(m_Cs, "IDCLIPBOARD", DbType.Int32);
            m_ParamSessionId = new DataParameter(m_Cs, "SESSIONID", DbType.AnsiString);
            m_ParamObjectName = new DataParameter(m_Cs, "OBJECTNAME", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            m_ParamFieldName = new DataParameter(m_Cs, "FIELDNAME", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            m_ParamObjectID = new DataParameter(m_Cs, "OBJECTID", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            m_ParamDisplayName = new DataParameter(m_Cs, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN);
            m_ParamHostName = new DataParameter(m_Cs, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN);
            m_ParamObjectXML = new DataParameter(m_Cs, "OBJECTXML", DbType.Xml);
            m_ParamDtSys = DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.DTSYS);
            m_ParamIdAIns = DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.IDAINS);
            m_ParamExtlLink = new DataParameter(m_Cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            m_ParamRowAttribut = new DataParameter(m_Cs, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
        }
		#endregion InitParameter
		#region SQLDelete
        public QueryParameters SQLDelete(bool pIsOneIdCLipBoard)
        {

            #region parameters
            DataParameters parameters = new DataParameters();
            if (pIsOneIdCLipBoard)
                parameters.Add(m_ParamIdClipboard);
            else
                parameters.Add(m_ParamSessionId);
            #endregion parameters
            //
            #region Query
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.SESSIONCLIPBOARD.ToString() + Cst.CrLf;
            if (pIsOneIdCLipBoard)
                sqlQuery += SQLCst.WHERE + "(IDCLIPBOARD=@IDCLIPBOARD)" + Cst.CrLf;
            else
                sqlQuery += SQLCst.WHERE + "(SESSIONID=@SESSIONID)" + Cst.CrLf;
            #endregion Query
            //
            QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
            //
            return ret;

        }
		#endregion SQLDelete
		#region SQLInsert
        public QueryParameters SQLInsert()
        {
            #region parameters
            DataParameters parameters = new DataParameters();
            parameters.Add(m_ParamSessionId);
            parameters.Add(m_ParamObjectName);
            parameters.Add(m_ParamFieldName);
            parameters.Add(m_ParamObjectID);
            parameters.Add(m_ParamDisplayName);
            parameters.Add(m_ParamHostName);
            parameters.Add(m_ParamObjectXML);
            parameters.Add(m_ParamDtSys);
            parameters.Add(m_ParamIdAIns);
            parameters.Add(m_ParamExtlLink);
            parameters.Add(m_ParamRowAttribut);
            #endregion parameters
            #region Query
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.SESSIONCLIPBOARD.ToString() + Cst.CrLf;
            sqlQuery += @"(SESSIONID, OBJECTNAME, FIELDNAME, OBJECTID, DISPLAYNAME, HOSTNAME," + Cst.CrLf;
            sqlQuery += @"OBJECTXML, DTSYS, IDAINS, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@SESSIONID, @OBJECTNAME, @FIELDNAME, @OBJECTID, @DISPLAYNAME, @HOSTNAME," + Cst.CrLf;
            sqlQuery += @"@OBJECTXML, @DTSYS, @IDAINS, @EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;
            #endregion Query
            QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
            return ret;
        }
		#endregion SQLInsert
		#region SQLSelect
		public QueryParameters SQLSelect()
		{
			return SQLSelect(false,false);
		}
        public QueryParameters SQLSelect(bool pIsIdSpecified, bool pIsDisplayNameSpecified)
        {
            #region parameters
            DataParameters parameters = new DataParameters();
            parameters.Add(m_ParamSessionId);
            parameters.Add(m_ParamObjectName);
            if (pIsIdSpecified)
                parameters.Add(m_ParamIdClipboard);
            else if (pIsDisplayNameSpecified)
                parameters.Add(m_ParamDisplayName);
            #endregion parameters
            //
            #region Query
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDCLIPBOARD, OBJECTNAME, FIELDNAME, OBJECTID, DISPLAYNAME, HOSTNAME," + Cst.CrLf;
            sqlQuery += @"OBJECTXML, DTSYS, IDAINS, EXTLLINK, ROWATTRIBUT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.SESSIONCLIPBOARD.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(SESSIONID=@SESSIONID)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(OBJECTNAME=@OBJECTNAME)" + Cst.CrLf;
            if (pIsIdSpecified)
                sqlQuery += SQLCst.AND + "(IDCLIPBOARD=@IDCLIPBOARD)" + Cst.CrLf;
            else if (pIsDisplayNameSpecified)
                sqlQuery += SQLCst.AND + "(DISPLAYNAME=@DISPLAYNAME)" + Cst.CrLf;
            else
                sqlQuery += SQLCst.ORDERBY + "DTSYS desc" + Cst.CrLf;
            #endregion Query
            QueryParameters ret = new QueryParameters(m_Cs, sqlQuery.ToString(), parameters);
            return ret;
        }
		#endregion SQLSelect
		#endregion Methods
	}
	

}

#region Using Directives
using System;
using System.Data;
using System.Text;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Status;
using EFS.Common.Log;
#endregion Using Directives

namespace EFS.Common
{
    #region SQL_IOTask

    public class SQL_IOTask : SQL_TableWithID
    {
        public SQL_IOTask(string pSource,int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No){}
        public SQL_IOTask(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataDtEnabledEnum){}
        public SQL_IOTask(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier,  ScanDataDtEnabledEnum.No){}
        public SQL_IOTask(string pSource,IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource,Cst.OTCml_TBL.IOTASK , pIdType, pIdentifier,pScanDataDtEnabledEnum)
        {}
	
        #region public property
        public string CommitMode
        {
            get 
            {
                string data = Convert.ToString (GetFirstRowColumnValue("COMMITMODE"));

                if (false == Enum.IsDefined(typeof(Cst.CommitMode), data))
                {
                    //Pour compatibilité ascendante, si besoin... 
                    data = Cst.CommitMode.INHERITED.ToString();
                }
                return data;
            }
        }
        public string LogLevel
        {
            get 
            {
                string data = Convert.ToString(GetFirstRowColumnValue("LOGLEVEL"));

                // PM 20200910 [XXXXX] Ajout test sinon on se retrouve toujours dans le default du switch
                if (false == Enum.IsDefined(typeof(LogLevelDetail), data))
                {
                    //Pour compatibilité ascendante, si besoin... 
                    switch (data)
                    {
                        case "FULL":
                            data = LogLevelDetail.LEVEL4.ToString();
                            break;
                        case "NONE":
                            data = LogLevelDetail.LEVEL2.ToString();
                            break;
                        default:
                            data = LogLevelDetail.LEVEL3.ToString();
                            break;
                    }
                }
                return data;
            }
        }
        public string InOut
        {
            get { return Convert.ToString (GetFirstRowColumnValue("IN_OUT"));}
        }
        public int IDA
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA")); }
        }
        public bool IsNotif_SMTP
        {
            get { return Convert.ToBoolean(GetFirstRowColumnValue("ISNOTIF_SMTP")); }
        }
        public int IDSMTPServer
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDSMTPSERVER")); }
        }
        public string OriginAdress
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ORIGINADRESS")); }
        }
        public int IDA_SMTP
        {
            get { return Convert.ToInt32(GetFirstRowColumnValue("IDA_SMTP")); }
        }
        public string AddressIdentDENT_SMTP
        {
            get { return Convert.ToString(GetFirstRowColumnValue("ADRESSIDENT_SMTP")); }
        }
        public string MailAdress
        {
            get { return Convert.ToString(GetFirstRowColumnValue("MAILADRESS")); }
        }
        public Cst.StatusTask SendON_SMTP
        {
            get
            {
                if (IsLoaded && (false == Convert.IsDBNull(Dt.Rows[0]["SENDON_SMTP"])))
                    return (Cst.StatusTask)Enum.Parse(typeof(Cst.StatusTask), Convert.ToString(Dt.Rows[0]["SENDON_SMTP"]));
                else
                    return Cst.StatusTask.ONTERMINATED;
            }
        }
        public string Object_SMTP
        {
            get { return Convert.ToString(GetFirstRowColumnValue("OBJECT_SMTP")); }
        }
        public string Signature_SMTP
        {
            get { return Convert.ToString(GetFirstRowColumnValue("SIGNATURE_SMTP")); }
        }
        public Cst.StatusPriority PriorityOnSUCCESS
        {
            get
            {
                if (IsLoaded && (false == Convert.IsDBNull(Dt.Rows[0]["PRIORITYONSUCCESS"])))
                    return (Cst.StatusPriority)Enum.Parse(typeof(Cst.StatusPriority), Convert.ToString(Dt.Rows[0]["PRIORITYONSUCCESS"]));
                else
                    return Cst.StatusPriority.HIGH;
            }
        }
        public Cst.StatusPriority PriorityOnERROR
        {
            get
            {
                if (IsLoaded && (false == Convert.IsDBNull(Dt.Rows[0]["PRIORITYONERROR"])))
                    return (Cst.StatusPriority)Enum.Parse(typeof(Cst.StatusPriority), Convert.ToString(Dt.Rows[0]["PRIORITYONERROR"]));
                else
                    return Cst.StatusPriority.HIGH;
            }
        }
        #endregion 
    }
    #endregion SQL_IOTask
    #region SQL_IOInput

    public class SQL_IOInput : SQL_TableWithID
    {
        public SQL_IOInput(string pSource,int pId)
            : this(pSource, IDType.Id, pId.ToString(), ScanDataDtEnabledEnum.No){}
        public SQL_IOInput(string pSource, int pId, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : this(pSource, IDType.Id, pId.ToString(), pScanDataDtEnabledEnum){}
        public SQL_IOInput(string pSource, IDType pIdType, string pIdentifier)
            : this(pSource, pIdType, pIdentifier,  ScanDataDtEnabledEnum.No){}
        public SQL_IOInput(string pSource,IDType pIdType, string pIdentifier, ScanDataDtEnabledEnum pScanDataDtEnabledEnum)
            : base(pSource,Cst.OTCml_TBL.IOINPUT , pIdType, pIdentifier,pScanDataDtEnabledEnum)
        {}
	
        #region public property
        public string CommitMode
        {
            get
            {
                string data = Convert.ToString(GetFirstRowColumnValue("COMMITMODE"));

                if (false == Enum.IsDefined(typeof(Cst.CommitMode), data))
                {
                    //Pour compatibilité ascendante, si besoin... 
                    data = Cst.CommitMode.INHERITED.ToString();
                }
                return data;
            }
        }
        public string LogLevel
        {
            get 
            {
                string data = Convert.ToString(GetFirstRowColumnValue("LOGLEVEL"));

                // PM 20200910 [XXXXX] Ajout test sinon on se retrouve toujours dans le default du switch
                if (false == Enum.IsDefined(typeof(EFS.Common.Log.LogLevelDetail), data))
                {
                    //Pour compatibilité ascendante, si besoin... 
                    switch (data)
                    {
                        case "FULL":
                            data = EFS.Common.Log.LogLevelDetail.LEVEL4.ToString();
                            break;
                        case "NONE":
                            data = EFS.Common.Log.LogLevelDetail.LEVEL2.ToString();
                            break;
                        default:
                            data = EFS.Common.Log.LogLevelDetail.LEVEL3.ToString();
                            break;
                    }
                }
                return data;
            }
        }
        #endregion 
    }
    #endregion SQL_IOInput
	#region SQL_IOTaskParams
	public class SQL_IOTaskParams 
	{
		#region Members 

		private readonly DataTable m_DtParams;

		#endregion Members
		#region Constructors 
        public SQL_IOTaskParams(string pSource, int pTaskId)
            : this(pSource, pTaskId.ToString()) { }
        public SQL_IOTaskParams(string pSource, string pTaskId)
        {
            // RD 20100923 / Il faudrait mettre des INNER
            string sqlQuery;
            DataSet dsParams;
            //
            sqlQuery = SQLCst.SELECT;
            sqlQuery += "t.DISPLAYNAME as TDISPLAYNAME, t.DESCRIPTION as TDESCRIPTION, " + Cst.CrLf;
            sqlQuery += "p.IDIOPARAM, p.DISPLAYNAME as PDISPLAYNAME, p.DESCRIPTION as PDESCRIPTION, " + Cst.CrLf;
            sqlQuery += "pd.IDIOPARAMDET, pd.DISPLAYNAME, pd.DESCRIPTION, pd.NOTE, " + Cst.CrLf;
            sqlQuery += "pd.DIRECTION, pd.DATATYPE, pd.ISMANDATORY, pd.REGULAREXPRESSION, pd.DEFAULTVALUE, pd.LISTRETRIEVAL, pd.ISSQLPARAMETER, pd.ISHIDEONGUI, pd.RETURNTYPE" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK.ToString() + " t" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOTASK_PARAM.ToString() + " tp" + SQLCst.ON + "tp.IDIOTASK=t.IDIOTASK" + Cst.CrLf;
            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pSource, "tp", DateTime.Today) + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOPARAM + " p" + SQLCst.ON + "p.IDIOPARAM=tp.IDIOPARAM" + Cst.CrLf;
            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pSource, "p", DateTime.Today) + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOPARAMDET.ToString() + " pd" + SQLCst.ON + "pd.IDIOPARAM=p.IDIOPARAM" + Cst.CrLf;
            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pSource, "pd", DateTime.Today) + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "t.IDIOTASK = " + DataHelper.SQLString(pTaskId) + Cst.CrLf;
            sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pSource, "t", DateTime.Today) + Cst.CrLf;
            sqlQuery += SQLCst.ORDERBY + "tp.SEQUENCENO, pd.SEQUENCENO";
            //
            dsParams = DataHelper.ExecuteDataset(pSource, CommandType.Text, sqlQuery);
            //
            m_DtParams = dsParams.Tables[0];
        }
		#endregion Constructors 
		#region Methods  

		public DataRow[] Select ()
		{
			return Select(string.Empty);	
		}
		public DataRow[] Select (string pFilter)
		{
			if(StrFunc.IsFilled(pFilter))
				return m_DtParams.Select(pFilter);	
			else
				return m_DtParams.Select();	
		}
		#endregion Methods 		
	
		static public string GetParamNameOfReturnSPParam(string pCs, string pIdTask, Cst.ReturnSPParamTypeEnum pParam)
		{
			string ret = string.Empty; 	
			//
			SQL_IOTaskParams taskParams = new SQL_IOTaskParams(pCs, pIdTask);
			string sqlFiltre = "ISSQLPARAMETER = 1 And RETURNTYPE = " + DataHelper.SQLString(pParam.ToString())       ;
			DataRow[] sqlParams = taskParams.Select(sqlFiltre);
			if (ArrFunc.IsFilled(sqlParams))
				ret = sqlParams[0]["IDIOPARAMDET"].ToString();
			return ret;
		}	
	}
	#endregion SQL_IOTaskParams
	
}

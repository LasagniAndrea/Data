using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;

namespace EFS.LoggerService
{
    /// <summary>
    /// Classe de gestion de l'écriture du log
    /// </summary>
    internal class LoggerSQLWriter
    {
        #region Const

        //private const int MaxLenghtMessage = SQLCst.UT_MESSAGE_LEN; 

        private const string m_SqlInsertPROCESS_L = @"
        insert into dbo.PROCESS_L
             ( IDPROCESS_L, IDTRK_L, PROCESS, IDSTPROCESS, DTSTPROCESS, DTPROCESSSTART,
               IDDATA, IDDATAIDENT, GUID,
               SYSCODE, SYSNUMBER, MESSAGE, DATA1, DATA2, DATA3, DATA4, DATA5, LODATATXT, QUEUEMSG,
               IDA, HOSTNAME, APPNAME, APPVERSION, ROWATTRIBUT )
        values
             ( @IDPROCESS_L, @IDTRK_L, @PROCESS, @IDSTPROCESS, @DTSTPROCESS, @DTSTPROCESS,
               @IDDATA, @IDDATAIDENT, @GUID,
               @SYSCODE, @SYSNUMBER, @MESSAGE, @DATA1, @DATA2, @DATA3, @DATA4, @DATA5, @LODATATXT, @QUEUEMSG,
               @IDA, @HOSTNAME, @APPNAME, @APPVERSION, @ROWATTRIBUT )";
        private const string m_SqlUpdatePROCESS_L = @"
        update dbo.PROCESS_L
           set IDSTPROCESS = @IDSTPROCESS,
               DTSTPROCESS = @DTSTPROCESS,
               DTPROCESSEND = @DTPROCESSEND
         where (IDPROCESS_L = @IDPROCESS_L)";
        private const string m_SqlUpdatePROCESS_L_DTPROCESSEND = @"
        update dbo.PROCESS_L
           set DTPROCESSEND = @DTPROCESSEND
         where (IDPROCESS_L = @IDPROCESS_L)";
        //
        private const string m_SqlSelectPROCESSDET_LColumns = @"
        select IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS,
               IDDATA, IDDATAIDENT, HOSTNAME, APPNAME, APPVERSION, OSPID, THREADID, SEQNO, GRPNO,
               MESSAGE, SYSCODE, SYSNUMBER, 
               DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10,
               DATA1IDENT, DATA2IDENT, DATA3IDENT, DATA4IDENT, DATA5IDENT, DATA6IDENT, DATA7IDENT, DATA8IDENT, DATA9IDENT, DATA10IDENT,
               LEVELORDER, QUEUEMSG
          from dbo.PROCESSDET_L
         where 0=1";
        private const string m_SqlInsertPROCESSDET_L = @"
        insert into dbo.PROCESSDET_L
             ( IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS,
               IDDATA, IDDATAIDENT, HOSTNAME, APPNAME, APPVERSION, OSPID, THREADID, SEQNO, GRPNO,
               MESSAGE, SYSCODE, SYSNUMBER, 
               DATA1, DATA2, DATA3, DATA4, DATA5, DATA6, DATA7, DATA8, DATA9, DATA10,
               DATA1IDENT, DATA2IDENT, DATA3IDENT, DATA4IDENT, DATA5IDENT, DATA6IDENT, DATA7IDENT, DATA8IDENT, DATA9IDENT, DATA10IDENT,
               LEVELORDER, QUEUEMSG )
        values
             ( @IDPROCESS_L, @PROCESS, @IDSTPROCESS, @DTSTPROCESS,
               @IDDATA, @IDDATAIDENT, @HOSTNAME, @APPNAME, @APPVERSION, @OSPID, @THREADID, @SEQNO, @GRPNO,
               @MESSAGE, @SYSCODE, @SYSNUMBER, 
               @DATA1, @DATA2, @DATA3, @DATA4, @DATA5, @DATA6, @DATA7, @DATA8, @DATA9, @DATA10,
               @DATA1IDENT, @DATA2IDENT, @DATA3IDENT, @DATA4IDENT, @DATA5IDENT, @DATA6IDENT, @DATA7IDENT, @DATA8IDENT, @DATA9IDENT, @DATA10IDENT,
               @LEVELORDER, @QUEUEMSG )";
        private const string m_SqlUpdatePROCESSDET_L = @"
        update dbo.PROCESSDET_L
           set IDSTPROCESS = @IDSTPROCESS
         where (IDPROCESS_L = @IDPROCESS_L)
           and (DTSTPROCESS = @DTSTPROCESS)
           and ((@SYSCODE is null) or ((SYSCODE = @SYSCODE) and (SYSNUMBER = @SYSNUMBER)))
           and ((@MESSAGE is null) or (MESSAGE = @MESSAGE))";
        private const string m_SqlDistinctWorstStatus = @"
        select distinct IDSTPROCESS
          from dbo.PROCESSDET_L
         where (IDPROCESS_L = @IDPROCESS_L)
           and (DTSTPROCESS >= @DTSTPROCESS)
           and (IDSTPROCESS in ('WARNING','ERROR','CRITICAL'))";
        #endregion Const

        #region Members
        private readonly RDBMSConnectionInfo m_RDBMSConnectionInfo;
        private readonly string m_ConnectionString;
        private readonly SemaphoreSlim m_SGBDLock = new SemaphoreSlim(1, 1);
        private readonly DbSvrType m_SGBDType;
        private DataTable m_ProcessDetLDatTbl = default;
        #endregion Members

        #region Property
        /// <summary>
        /// 
        /// </summary>
        /// FI 20210715 [XXXXX] Même si la colonne est dimmensionnée avec 4000 caractères, Spheres® n'alimente que les 3500 caractères   
        private int MaxLenghtMessage
        {
            get
            {
                int ret = SQLCst.UT_MESSAGE_LEN;
                if (DataHelper.IsDbOracle(m_ConnectionString))
                    ret -= 500;
                return ret;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionInfo"></param>
        public LoggerSQLWriter(RDBMSConnectionInfo pConnectionInfo)
        {
            m_RDBMSConnectionInfo = pConnectionInfo;
            if (m_RDBMSConnectionInfo != default(RDBMSConnectionInfo))
            {
                m_ConnectionString = m_RDBMSConnectionInfo.ConnectionString;
                m_SGBDType = m_RDBMSConnectionInfo.SGBDType;
                InitDataTableProcessDet();
            }
        }
        #endregion Constructors

        #region Methods
        #region private Methods
        #region private InitDataTableProcessDet
        /// <summary>
        /// Initialisation de la DataTable pour l'écriture Bulk dans PROCESSDET_L
        /// </summary>
        private void InitDataTableProcessDet()
        {
            m_ProcessDetLDatTbl = new DataTable();
            //
            m_ProcessDetLDatTbl.Columns.Add("IDPROCESS_L");
            m_ProcessDetLDatTbl.Columns.Add("PROCESS");
            m_ProcessDetLDatTbl.Columns.Add("IDSTPROCESS");
            m_ProcessDetLDatTbl.Columns.Add("DTSTPROCESS");
            m_ProcessDetLDatTbl.Columns.Add("IDDATA");
            m_ProcessDetLDatTbl.Columns.Add("IDDATAIDENT");
            m_ProcessDetLDatTbl.Columns.Add("HOSTNAME");
            m_ProcessDetLDatTbl.Columns.Add("APPNAME");
            m_ProcessDetLDatTbl.Columns.Add("APPVERSION");
            m_ProcessDetLDatTbl.Columns.Add("OSPID"); 
            m_ProcessDetLDatTbl.Columns.Add("THREADID");
            m_ProcessDetLDatTbl.Columns.Add("SEQNO");
            m_ProcessDetLDatTbl.Columns.Add("GRPNO");
            m_ProcessDetLDatTbl.Columns.Add("MESSAGE");
            m_ProcessDetLDatTbl.Columns.Add("DATA1");
            m_ProcessDetLDatTbl.Columns.Add("DATA2");
            m_ProcessDetLDatTbl.Columns.Add("DATA3");
            m_ProcessDetLDatTbl.Columns.Add("DATA4");
            m_ProcessDetLDatTbl.Columns.Add("DATA5");
            m_ProcessDetLDatTbl.Columns.Add("DATA6");
            m_ProcessDetLDatTbl.Columns.Add("DATA7");
            m_ProcessDetLDatTbl.Columns.Add("DATA8");
            m_ProcessDetLDatTbl.Columns.Add("DATA9");
            m_ProcessDetLDatTbl.Columns.Add("DATA10");
            m_ProcessDetLDatTbl.Columns.Add("DATA1IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA2IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA3IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA4IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA5IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA6IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA7IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA8IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA9IDENT");
            m_ProcessDetLDatTbl.Columns.Add("DATA10IDENT");
            m_ProcessDetLDatTbl.Columns.Add("SYSCODE");
            m_ProcessDetLDatTbl.Columns.Add("SYSNUMBER");
            m_ProcessDetLDatTbl.Columns.Add("LEVELORDER");
            m_ProcessDetLDatTbl.Columns.Add("QUEUEMSG");
            //
            m_ProcessDetLDatTbl = DataHelper.ExecuteDataTable(m_ConnectionString, m_SqlSelectPROCESSDET_LColumns);
        }
        #endregion private InitDataTableProcessDet

        #region private QueryParamInsertPROCESS_L
        /// <summary>
        /// Construction du QueryParameters d'insertion dans PROCESS_L
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryParamInsertPROCESS_L()
        {
            DataParameters parameters = new DataParameters();
            //
            DataParameter paramIdProcess = new DataParameter(m_ConnectionString, "IDPROCESS_L", DbType.Int32);
            DataParameter paramIdTRK_L = new DataParameter(m_ConnectionString, "IDTRK_L", DbType.Int32);
            DataParameter paramProcess = new DataParameter(m_ConnectionString, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            DataParameter paramIdStProcess = new DataParameter(m_ConnectionString, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN);
            DataParameter paramDtStProcess = DataParameter.GetParameter(m_ConnectionString, DataParameter.ParameterEnum.DTSTPROCESS);
            DataParameter paramIdData = new DataParameter(m_ConnectionString, "IDDATA", DbType.Int32);
            DataParameter paramIdDataIdent = new DataParameter(m_ConnectionString, "IDDATAIDENT", DbType.AnsiString, 64);
            DataParameter paramGuid = new DataParameter(m_ConnectionString, "GUID", DbType.AnsiString, 64);
            DataParameter paramSysCode = new DataParameter(m_ConnectionString, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN);
            DataParameter paramSysNumber = new DataParameter(m_ConnectionString, "SYSNUMBER", DbType.Int32);
            DataParameter paramMessage = new DataParameter(m_ConnectionString, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN);
            DataParameter paramData1 = new DataParameter(m_ConnectionString, "DATA1", DbType.AnsiString, 128);
            DataParameter paramData2 = new DataParameter(m_ConnectionString, "DATA2", DbType.AnsiString, 128);
            DataParameter paramData3 = new DataParameter(m_ConnectionString, "DATA3", DbType.AnsiString, 128);
            DataParameter paramData4 = new DataParameter(m_ConnectionString, "DATA4", DbType.AnsiString, 128);
            DataParameter paramData5 = new DataParameter(m_ConnectionString, "DATA5", DbType.AnsiString, 128);
            DataParameter paramLoDataTxt = new DataParameter(m_ConnectionString, "LODATATXT", DbType.String);
            DataParameter paramQueueMsg = new DataParameter(m_ConnectionString, "QUEUEMSG", DbType.String);
            DataParameter paramIdA = new DataParameter(m_ConnectionString, "IDA", DbType.Int32);
            DataParameter paramHostName = new DataParameter(m_ConnectionString, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN);
            DataParameter paramAppName = new DataParameter(m_ConnectionString, "APPNAME", DbType.AnsiString, 64);
            DataParameter paramAppVersion = new DataParameter(m_ConnectionString, "APPVERSION", DbType.AnsiString, 64);
            DataParameter paramRowAttribut = new DataParameter(m_ConnectionString, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
            //
            parameters.Add(paramIdProcess);
            parameters.Add(paramIdTRK_L);
            parameters.Add(paramProcess);
            parameters.Add(paramIdStProcess);
            parameters.Add(paramDtStProcess);
            parameters.Add(paramIdData);
            parameters.Add(paramIdDataIdent);
            parameters.Add(paramGuid);
            parameters.Add(paramSysCode);
            parameters.Add(paramSysNumber);
            parameters.Add(paramMessage);
            parameters.Add(paramData1);
            parameters.Add(paramData2);
            parameters.Add(paramData3);
            parameters.Add(paramData4);
            parameters.Add(paramData5);
            parameters.Add(paramLoDataTxt);
            parameters.Add(paramQueueMsg);
            parameters.Add(paramIdA);
            parameters.Add(paramHostName);
            parameters.Add(paramAppName);
            parameters.Add(paramAppVersion);
            parameters.Add(paramRowAttribut);
            //
            QueryParameters ret = new QueryParameters(m_ConnectionString, m_SqlInsertPROCESS_L, parameters);
            return ret;
        }
        #endregion private QueryParamInsertPROCESS_L

        #region private QueryParamUpdatePROCESS_L
        /// <summary>
        /// Construction du QueryParameters de mise à jour de PROCESS_L
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryParamUpdatePROCESS_L()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_ConnectionString, "IDPROCESS_L", DbType.Int32));
            parameters.Add(new DataParameter(m_ConnectionString, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
            parameters.Add(DataParameter.GetParameter(m_ConnectionString, DataParameter.ParameterEnum.DTSTPROCESS));
            parameters.Add(new DataParameter(m_ConnectionString, "DTPROCESSEND", DbType.DateTime2));
            
            QueryParameters ret = new QueryParameters(m_ConnectionString, m_SqlUpdatePROCESS_L, parameters);
            return ret;
        }
        #endregion private QueryParamUpdatePROCESS_L

        #region private QueryParamUpdatePROCESS_L_DTPROCESSEND
        /// <summary>
        /// Construction du QueryParameters de mise à jour de DTPROCESSEND de PROCESS_L
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryParamUpdatePROCESS_L_DTPROCESSEND()
        {
            DataParameters parameters = new DataParameters();
            //
            DataParameter paramIdProcess = new DataParameter(m_ConnectionString, "IDPROCESS_L", DbType.Int32);
            DataParameter paramDtProcessEnd = new DataParameter(m_ConnectionString, "DTPROCESSEND", DbType.DateTime2);
            //
            parameters.Add(paramIdProcess);
            parameters.Add(paramDtProcessEnd);
            //
            QueryParameters ret = new QueryParameters(m_ConnectionString, m_SqlUpdatePROCESS_L_DTPROCESSEND, parameters);
            return ret;
        }
        #endregion private QueryParamUpdatePROCESS_L_DTPROCESSEND

        #region private QueryParamInsertPROCESSDET_L
        /// <summary>
        /// Construction du QueryParameters d'insertion dans PROCESSDET_L
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryParamInsertPROCESSDET_L()
        {
            DataParameters parameters = new DataParameters();
            //
            DataParameter paramIdProcess = new DataParameter(m_ConnectionString, "IDPROCESS_L", DbType.Int32);
            DataParameter paramProcess = new DataParameter(m_ConnectionString, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            DataParameter paramIdStProcess = new DataParameter(m_ConnectionString, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN);
            DataParameter paramDtStProcess = DataParameter.GetParameter(m_ConnectionString, DataParameter.ParameterEnum.DTSTPROCESS);
            DataParameter paramIdData = new DataParameter(m_ConnectionString, "IDDATA", DbType.Int32);
            DataParameter paramIdDataIdent = new DataParameter(m_ConnectionString, "IDDATAIDENT", DbType.AnsiString, 64);
            DataParameter paramHostName = new DataParameter(m_ConnectionString, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN);
            DataParameter paramAppName = new DataParameter(m_ConnectionString, "APPNAME", DbType.AnsiString, 64);
            DataParameter paramAppVersion = new DataParameter(m_ConnectionString, "APPVERSION", DbType.AnsiString, 64);
            DataParameter paramOSPID = new DataParameter(m_ConnectionString, "OSPID", DbType.Int32);
            DataParameter paramThreadId = new DataParameter(m_ConnectionString, "THREADID", DbType.Int32);
            DataParameter paramSequenceNumber = new DataParameter(m_ConnectionString, "SEQNO", DbType.Int32);
            DataParameter paramGoupSequenceNumber = new DataParameter(m_ConnectionString, "GRPNO", DbType.Int32);
            DataParameter paramMessage = new DataParameter(m_ConnectionString, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN);
            DataParameter paramSysCode = new DataParameter(m_ConnectionString, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN);
            DataParameter paramSysNumber = new DataParameter(m_ConnectionString, "SYSNUMBER", DbType.Int32);
            DataParameter paramData1 = new DataParameter(m_ConnectionString, "DATA1", DbType.AnsiString, 128);
            DataParameter paramData2 = new DataParameter(m_ConnectionString, "DATA2", DbType.AnsiString, 128);
            DataParameter paramData3 = new DataParameter(m_ConnectionString, "DATA3", DbType.AnsiString, 128);
            DataParameter paramData4 = new DataParameter(m_ConnectionString, "DATA4", DbType.AnsiString, 128);
            DataParameter paramData5 = new DataParameter(m_ConnectionString, "DATA5", DbType.AnsiString, 128);
            DataParameter paramData6 = new DataParameter(m_ConnectionString, "DATA6", DbType.AnsiString, 128);
            DataParameter paramData7 = new DataParameter(m_ConnectionString, "DATA7", DbType.AnsiString, 128);
            DataParameter paramData8 = new DataParameter(m_ConnectionString, "DATA8", DbType.AnsiString, 128);
            DataParameter paramData9 = new DataParameter(m_ConnectionString, "DATA9", DbType.AnsiString, 128);
            DataParameter paramData10 = new DataParameter(m_ConnectionString, "DATA10", DbType.AnsiString, 128);
            DataParameter paramData1Ident = new DataParameter(m_ConnectionString, "DATA1IDENT", DbType.AnsiString, 64);
            DataParameter paramData2Ident = new DataParameter(m_ConnectionString, "DATA2IDENT", DbType.AnsiString, 64);
            DataParameter paramData3Ident = new DataParameter(m_ConnectionString, "DATA3IDENT", DbType.AnsiString, 64);
            DataParameter paramData4Ident = new DataParameter(m_ConnectionString, "DATA4IDENT", DbType.AnsiString, 64);
            DataParameter paramData5Ident = new DataParameter(m_ConnectionString, "DATA5IDENT", DbType.AnsiString, 64);
            DataParameter paramData6Ident = new DataParameter(m_ConnectionString, "DATA6IDENT", DbType.AnsiString, 64);
            DataParameter paramData7Ident = new DataParameter(m_ConnectionString, "DATA7IDENT", DbType.AnsiString, 64);
            DataParameter paramData8Ident = new DataParameter(m_ConnectionString, "DATA8IDENT", DbType.AnsiString, 64);
            DataParameter paramData9Ident = new DataParameter(m_ConnectionString, "DATA9IDENT", DbType.AnsiString, 64);
            DataParameter paramData10Ident = new DataParameter(m_ConnectionString, "DATA10IDENT", DbType.AnsiString, 64);
            DataParameter paramLevelOrder = new DataParameter(m_ConnectionString, "LEVELORDER", DbType.Int32);
            DataParameter paramQueueMsg = new DataParameter(m_ConnectionString, "QUEUEMSG", DbType.String);
            //
            parameters.Add(paramIdProcess);
            parameters.Add(paramProcess);
            parameters.Add(paramIdStProcess);
            parameters.Add(paramDtStProcess);
            parameters.Add(paramIdData);
            parameters.Add(paramIdDataIdent);
            parameters.Add(paramHostName);
            parameters.Add(paramAppName);
            parameters.Add(paramAppVersion);
            parameters.Add(paramOSPID);
            parameters.Add(paramThreadId);
            parameters.Add(paramSequenceNumber);
            parameters.Add(paramGoupSequenceNumber);
            parameters.Add(paramMessage);
            parameters.Add(paramSysCode);
            parameters.Add(paramSysNumber);
            parameters.Add(paramData1);
            parameters.Add(paramData2);
            parameters.Add(paramData3);
            parameters.Add(paramData4);
            parameters.Add(paramData5);
            parameters.Add(paramData6);
            parameters.Add(paramData7);
            parameters.Add(paramData8);
            parameters.Add(paramData9);
            parameters.Add(paramData10);
            parameters.Add(paramData1Ident);
            parameters.Add(paramData2Ident);
            parameters.Add(paramData3Ident);
            parameters.Add(paramData4Ident);
            parameters.Add(paramData5Ident);
            parameters.Add(paramData6Ident);
            parameters.Add(paramData7Ident);
            parameters.Add(paramData8Ident);
            parameters.Add(paramData9Ident);
            parameters.Add(paramData10Ident);
            parameters.Add(paramLevelOrder);
            parameters.Add(paramQueueMsg);
            //
            QueryParameters ret = new QueryParameters(m_ConnectionString, m_SqlInsertPROCESSDET_L, parameters);
            return ret;
        }
        #endregion private QueryParamInsertPROCESSDET_L

        #region private QueryParamUpdatePROCESSDET_L
        /// <summary>
        /// Construction du QueryParameters de mise à jour de PROCESSDET_L
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryParamUpdatePROCESSDET_L()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_ConnectionString, "IDPROCESS_L", DbType.Int32));
            parameters.Add(new DataParameter(m_ConnectionString, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
            parameters.Add(DataParameter.GetParameter(m_ConnectionString, DataParameter.ParameterEnum.DTSTPROCESS));
            parameters.Add(new DataParameter(m_ConnectionString, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
            parameters.Add(new DataParameter(m_ConnectionString, "SYSNUMBER", DbType.Int32));
            parameters.Add(new DataParameter(m_ConnectionString, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN));
            
            QueryParameters ret = new QueryParameters(m_ConnectionString, m_SqlUpdatePROCESSDET_L, parameters);
            return ret;
        }
        #endregion private QueryParamUpdatePROCESSDET_L

        #region private QueryParamDistinctWorstStatus
        /// <summary>
        /// Construction du QueryParameters de recherche des pires status dans PROCESSDET_L
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryParamDistinctWorstStatus()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_ConnectionString, "IDPROCESS_L", DbType.Int32));
            parameters.Add(DataParameter.GetParameter(m_ConnectionString, DataParameter.ParameterEnum.DTSTPROCESS));
            QueryParameters ret = new QueryParameters(m_ConnectionString, m_SqlDistinctWorstStatus, parameters);
            return ret;
        }
        #endregion private QueryParamDistinctWorstStatus

        #region private SQLBulkCopy
        /// <summary>
        /// Ecriture Bulk d'une DataTable dans une Table
        /// </summary>
        /// <param name="pDestTableName"></param>
        /// <param name="pDataTable"></param>
        private void SQLBulkCopy(string pDestTableName, DataTable pDataTable)
        {
            DataHelper.BulkWriteToServer(m_ConnectionString, pDestTableName, pDataTable);
        }
        #endregion private SQLBulkCopy

        #region private SetProcessLParameters
        /// <summary>
        /// Affectation des valeurs à chaque paramètre de donnée des colonnes de la table PROCESS_L
        /// </summary>
        /// <param name="pParameters"></param>
        /// <param name="pData"></param>
        private void SetProcessLParameters(DataParameters pParameters, LogUnit pData)
        {
            if ((pParameters != default(DataParameters)) && (pData != default(LogUnit)))
            {
                LoggerData logData = pData.LogData;
                if (logData != default(LoggerData))
                {
                    pParameters["IDPROCESS_L"].Value = logData.LogScope.IdPROCESS_L;
                    pParameters["HOSTNAME"].Value = logData.HostName;
                    pParameters["APPNAME"].Value = logData.InstanceName;
                    pParameters["APPVERSION"].Value = logData.InstanceVersion;
                    pParameters["PROCESS"].Value = logData.ProcessType;
                    //
                    if (logData.SysMsg != default)
                    {
                        pParameters["SYSCODE"].Value = logData.SysMsg.SysCode;
                        pParameters["SYSNUMBER"].Value = logData.SysMsg.SysNumber;
                        pParameters["MESSAGE"].Value = Cst.Space;
                    }
                    else
                    {
                        pParameters["SYSCODE"].Value = Convert.DBNull;
                        pParameters["SYSNUMBER"].Value = Convert.DBNull;
                        pParameters["MESSAGE"].Value = (logData.Message != default) ? logData.Message.Substring(0, System.Math.Min(MaxLenghtMessage, logData.Message.Length)) : Cst.Space;
                    }
                    //
                    pParameters["IDTRK_L"].Value = (logData.LogScope.IdTRK_L > 0) ? logData.LogScope.IdTRK_L : Convert.DBNull;
                    pParameters["DTSTPROCESS"].Value = logData.DtUtcLog;
                    pParameters["IDSTPROCESS"].Value = "NONE";
                    //
                    LogParam idData = default;
                    LogParam queuemsg = default;
                    LogParam lodatatxt = default;
                    LogParam[] infoData = new LogParam[0];
                    if ((logData.Parameters != default(LogParam[])) && logData.Parameters.Count() > 0)
                    {
                        idData = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.IDDATA.ToString()));
                        queuemsg = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.QUEUEMSG.ToString()));
                        lodatatxt = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.LODATATXT.ToString()));
                        infoData = logData.Parameters.Where(l => l.DataSpecified).ToArray();
                    }
                    pParameters["QUEUEMSG"].Value = (queuemsg != default(LogParam)) ? queuemsg.Identifier : Convert.DBNull;
                    pParameters["IDDATA"].Value = (idData != default(LogParam)) ? idData.InternalId : 0;
                    pParameters["IDDATAIDENT"].Value = (idData != default(LogParam)) ? DataHelper.GetDBData(idData.Referential) : Convert.DBNull;
                    //
                    pParameters["GUID"].Value = logData.LogScope.LogScopeId != default ? logData.LogScope.LogScopeId.ToString() : Convert.DBNull;
                    //
                    pParameters["DATA1"].Value = (infoData.Count() > 0) ? SubStringOrDBNull(infoData[0].Data, 128) : Convert.DBNull;
                    pParameters["DATA2"].Value = (infoData.Count() > 1) ? SubStringOrDBNull(infoData[1].Data, 128) : Convert.DBNull;
                    pParameters["DATA3"].Value = (infoData.Count() > 2) ? SubStringOrDBNull(infoData[2].Data, 128) : Convert.DBNull;
                    pParameters["DATA4"].Value = (infoData.Count() > 3) ? SubStringOrDBNull(infoData[3].Data, 128) : Convert.DBNull;
                    pParameters["DATA5"].Value = (infoData.Count() > 4) ? SubStringOrDBNull(infoData[4].Data, 128) : Convert.DBNull;
                    //
                    pParameters["LODATATXT"].Value = (lodatatxt != default(LogParam)) ? SubStringOrDBNull(lodatatxt.Identifier, 4000) : Convert.DBNull;
                    //
                    pParameters["IDA"].Value = (logData.InstanceIda == 0 ? 1 : logData.InstanceIda);
                    pParameters["ROWATTRIBUT"].Value = "S";
                }
            }
        }
        #endregion private SetProcessLParameters

        #region private SetProcessDetLRow
        /// <summary>
        /// Affectation des valeurs du journal a chaque colonne du DataRow de la table PROCESSDET_L
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        private bool SetProcessDetLRow(DataRow pRow, LogUnit pData)
        {
            bool isOk = false;
            if ((pRow != default(DataRow)) && (pData != default(LogUnit)))
            {
                LoggerData logData = pData.LogData;
                if (logData != default(LoggerData))
                {
                    int idLink = logData.LogScope.IdPROCESS_L;
                    if (idLink > 0)
                    {
                        pRow["IDPROCESS_L"] = idLink;
                        pRow["PROCESS"] = logData.ProcessType;
                        pRow["DTSTPROCESS"] = logData.DtUtcLog;
                        pRow["IDSTPROCESS"] = logData.LogLevel.ToString().ToUpper();
                        pRow["HOSTNAME"] = logData.HostName;
                        pRow["APPNAME"] = logData.InstanceName;
                        pRow["APPVERSION"] = logData.InstanceVersion;
                        pRow["OSPID"] = logData.OS_PID;
                        pRow["THREADID"] = logData.ThreadID;
                        pRow["SEQNO"] = logData.SequenceNumber;
                        // PL 20210422 TIP A REVOIR AU RETOUR DE PM
                        if (logData.GroupSequenceNumber == -999)
                            pRow["GRPNO"] = 0;
                        else
                            pRow["GRPNO"] = ((logData.GroupSequenceNumber != 0) ? logData.GroupSequenceNumber : Convert.DBNull);
                        //
                        if (logData.SysMsg != default)
                        {
                            pRow["SYSCODE"] = logData.SysMsg.SysCode.ToString();
                            pRow["SYSNUMBER"] = logData.SysMsg.SysNumber;
                            pRow["MESSAGE"] = Convert.DBNull;
                        }
                        else
                        {
                            pRow["SYSCODE"] = Convert.DBNull;
                            pRow["SYSNUMBER"] = Convert.DBNull;
                            pRow["MESSAGE"] = (logData.Message != default) ? logData.Message.Substring(0, System.Math.Min(MaxLenghtMessage, logData.Message.Length)) : Convert.DBNull;
                        }
                        //
                        LogParam idData = default;
                        LogParam queuemsg = default;
                        List<LogParam> data = new List<LogParam>();
                        if ((logData.Parameters != default(LogParam[])) && logData.Parameters.Count() > 0)
                        {
                            idData = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.IDDATA.ToString()));
                            queuemsg = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.QUEUEMSG.ToString()));
                            data = logData.Parameters.Where(l => l.DataSpecified || (l.IsReferentialIdentifier && (l.Link != Cst.LoggerParameterLink.IDDATA.ToString()) && (l.Link != Cst.LoggerParameterLink.QUEUEMSG.ToString()))).ToList();
                        }
                        pRow["QUEUEMSG"] = (queuemsg != default) ? queuemsg.Identifier : Convert.DBNull;
                        pRow["IDDATA"] = (idData != default) ? idData.InternalId : 0;
                        pRow["IDDATAIDENT"] = (idData != default) ? DataHelper.GetDBData(idData.Referential) : Convert.DBNull;
                        //
                        if ((logData.LogLevel == LogLevelEnum.Error) || (logData.LogLevel == LogLevelEnum.Critical) || (logData.LogLevel == LogLevelEnum.Trace))
                        {
                            if (StrFunc.IsFilled(logData.CallingMethod))
                            {
                                string method = logData.CallingMethod;
                                if (StrFunc.IsFilled(logData.CallingClassType))
                                {
                                    method = logData.CallingClassType + "." + method;
                                }
                                LogParam methodInfo = new LogParam
                                {
                                    Data = "Method [" + method + "]",
                                    DataSpecified = true
                                };
                                data.Add(methodInfo);
                            }
                        }
                        //
                        LogParam[] infoData = data.ToArray();
                        SetProcessDetLRowData(pRow, (infoData.Count() > 0) ? infoData[0] : default, "DATA1", "DATA1IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 1) ? infoData[1] : default, "DATA2", "DATA2IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 2) ? infoData[2] : default, "DATA3", "DATA3IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 3) ? infoData[3] : default, "DATA4", "DATA4IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 4) ? infoData[4] : default, "DATA5", "DATA5IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 5) ? infoData[5] : default, "DATA6", "DATA6IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 6) ? infoData[6] : default, "DATA7", "DATA7IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 7) ? infoData[7] : default, "DATA8", "DATA8IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 8) ? infoData[8] : default, "DATA9", "DATA9IDENT");
                        SetProcessDetLRowData(pRow, (infoData.Count() > 9) ? infoData[9] : default, "DATA10", "DATA10IDENT");
                        //
                        pRow["LEVELORDER"] = logData.RankOrder;
                        //
                        isOk = true;
                    }
                }
            }
            return isOk;
        }
        #endregion private SetProcessDetLRow

        #region private SetProcessDetLParameters
        /// <summary>
        /// Affectation des valeurs a chaque paramètre de donnée des colonnes de la table PROCESSDET_L
        /// </summary>
        /// <param name="pParameters"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        private bool SetProcessDetLParameters(DataParameters pParameters, LogUnit pData)
        {
            bool isOk = false;
            if ((pParameters != default(DataParameters)) && (pData != default(LogUnit)))
            {
                LoggerData logData = pData.LogData;
                if (logData != default(LoggerData))
                {
                    int idLink = logData.LogScope.IdPROCESS_L;
                    if (idLink > 0)
                    {
                        pParameters["IDPROCESS_L"].Value = idLink;
                        pParameters["PROCESS"].Value = logData.ProcessType;
                        pParameters["DTSTPROCESS"].Value = logData.DtUtcLog;
                        pParameters["IDSTPROCESS"].Value = logData.LogLevel.ToString().ToUpper();
                        pParameters["HOSTNAME"].Value = logData.HostName;
                        pParameters["APPNAME"].Value = logData.InstanceName;
                        pParameters["APPVERSION"].Value = logData.InstanceVersion;
                        pParameters["OSPID"].Value = logData.OS_PID;
                        pParameters["THREADID"].Value = logData.ThreadID;
                        pParameters["SEQNO"].Value = logData.SequenceNumber;
                        // PL 20210422 TIP A REVOIR AU RETOUR DE PM
                        if (logData.GroupSequenceNumber == -999)
                            pParameters["GRPNO"].Value = 0;
                        else
                            pParameters["GRPNO"].Value = ((logData.GroupSequenceNumber != 0) ? logData.GroupSequenceNumber : Convert.DBNull);
                        //
                        if (logData.SysMsg != default)
                        {
                            pParameters["SYSCODE"].Value = logData.SysMsg.SysCode.ToString();
                            pParameters["SYSNUMBER"].Value = logData.SysMsg.SysNumber;
                            pParameters["MESSAGE"].Value = Convert.DBNull;
                        }
                        else
                        {
                            pParameters["SYSCODE"].Value = Convert.DBNull;
                            pParameters["SYSNUMBER"].Value = Convert.DBNull;
                            pParameters["MESSAGE"].Value = (logData.Message != default) ? logData.Message.Substring(0, System.Math.Min(MaxLenghtMessage, logData.Message.Length)) : Convert.DBNull;
                        }
                        //
                        LogParam idData = default;
                        LogParam queuemsg = default;
                        List<LogParam> data = new List<LogParam>();
                        if ((logData.Parameters != default(LogParam[])) && logData.Parameters.Count() > 0)
                        {
                            idData = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.IDDATA.ToString()));
                            queuemsg = logData.Parameters.FirstOrDefault(l => l.IsReferentialIdentifier && (l.Link == Cst.LoggerParameterLink.QUEUEMSG.ToString()));
                            data = logData.Parameters.Where(l => l.DataSpecified || (l.IsReferentialIdentifier && (l.Link != Cst.LoggerParameterLink.IDDATA.ToString()) && (l.Link != Cst.LoggerParameterLink.QUEUEMSG.ToString()))).ToList();
                        }
                        pParameters["QUEUEMSG"].Value = (queuemsg != default) ? queuemsg.Identifier : Convert.DBNull;
                        pParameters["IDDATA"].Value = (idData != default) ? idData.InternalId : 0;
                        pParameters["IDDATAIDENT"].Value = (idData != default) ? DataHelper.GetDBData(idData.Referential) : Convert.DBNull;
                        //
                        if ((logData.LogLevel == LogLevelEnum.Error) || (logData.LogLevel == LogLevelEnum.Critical) || (logData.LogLevel == LogLevelEnum.Trace))
                        {
                            if (StrFunc.IsFilled(logData.CallingMethod))
                            {
                                string method = logData.CallingMethod;
                                if (StrFunc.IsFilled(logData.CallingClassType))
                                {
                                    method = logData.CallingClassType + "." + method;
                                }
                                LogParam methodInfo = new LogParam
                                {
                                    Data = "Method [" + method + "]",
                                    DataSpecified = true
                                };
                                data.Add(methodInfo);
                            }
                        }
                        //
                        LogParam[] infoData = data.ToArray();
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 0) ? infoData[0] : default, "DATA1", "DATA1IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 1) ? infoData[1] : default, "DATA2", "DATA2IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 2) ? infoData[2] : default, "DATA3", "DATA3IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 3) ? infoData[3] : default, "DATA4", "DATA4IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 4) ? infoData[4] : default, "DATA5", "DATA5IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 5) ? infoData[5] : default, "DATA6", "DATA6IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 6) ? infoData[6] : default, "DATA7", "DATA7IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 7) ? infoData[7] : default, "DATA8", "DATA8IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 8) ? infoData[8] : default, "DATA9", "DATA9IDENT");
                        SetProcessDetLpParameterData(pParameters, (infoData.Count() > 9) ? infoData[9] : default, "DATA10", "DATA10IDENT");
                        //
                        pParameters["LEVELORDER"].Value = logData.RankOrder;
                        //
                        isOk = true;
                    }
                }
            }
            return isOk;
        }
        #endregion private SetProcessDetLParameters

        #region private SetProcessDetLRowData
        /// <summary>
        /// Affectation de la valeur d'une donnée "Data" à la colonne correspondante du DataRow de la table PROCESSDET_L
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pDataParam"></param>
        /// <param name="pColData"></param>
        /// <param name="pColIdent"></param>
        private void SetProcessDetLRowData(DataRow pRow, LogParam pDataParam, string pColData, string pColIdent)
        {
            if ((pRow != default(DataRow)) && StrFunc.IsFilled(pColData) && StrFunc.IsFilled(pColIdent))
            {
                if ((pDataParam != default(LogParam)))
                {
                    if (pDataParam.IsReferentialIdentifier)
                    {
                        if (pDataParam.InternalId > 0)
                        {
                            pRow[pColData] = SubStringOrDBNull(string.Format("{0} (id: {1})", pDataParam.Identifier, pDataParam.InternalId), 128);
                        }
                        else
                        {
                            pRow[pColData] = SubStringOrDBNull(pDataParam.Identifier, 128);
                        }
                        pRow[pColIdent] = SubStringOrDBNull(pDataParam.Referential, 64);
                    }
                    else
                    {
                        pRow[pColData] = SubStringOrDBNull(pDataParam.Data, 128);
                        pRow[pColIdent] = Convert.DBNull;
                    }
                }
                else
                {
                    pRow[pColData] = Convert.DBNull;
                    pRow[pColIdent] = Convert.DBNull;
                }
            }
        }
        #endregion private SetProcessDetLRowData

        #region private SetProcessDetLpParameterData
        /// <summary>
        /// Affectation de la valeur d'une donnée "Data" aux parametres correspondants des colonnes de la table PROCESSDET_L
        /// </summary>
        /// <param name="pParameters"></param>
        /// <param name="pDataParam"></param>
        /// <param name="pColData"></param>
        /// <param name="pColIdent"></param>
        private void SetProcessDetLpParameterData(DataParameters pParameters, LogParam pDataParam, string pColData, string pColIdent)
        {
            if ((pParameters != default(DataParameters)) && StrFunc.IsFilled(pColData) && StrFunc.IsFilled(pColIdent))
            {
                if ((pDataParam != default(LogParam)))
                {
                    if (pDataParam.IsReferentialIdentifier)
                    {
                        if (pDataParam.InternalId > 0)
                        {
                            pParameters[pColData].Value = SubStringOrDBNull(string.Format("{0} (id: {1})", pDataParam.Identifier, pDataParam.InternalId), 128);
                        }
                        else
                        {
                            pParameters[pColData].Value = SubStringOrDBNull(pDataParam.Identifier, 128);
                        }
                        pParameters[pColIdent].Value = SubStringOrDBNull(pDataParam.Referential, 64);
                    }
                    else
                    {
                        pParameters[pColData].Value = SubStringOrDBNull(pDataParam.Data, 128);
                        pParameters[pColIdent].Value = Convert.DBNull;
                    }
                }
                else
                {
                    pParameters[pColData].Value = Convert.DBNull;
                    pParameters[pColIdent].Value = Convert.DBNull;
                }
            }
        }
        #endregion private SetProcessDetLpParameterData

        #region SQLWriteProcessDetLBulk
        /// <summary>
        /// Ecriture d'un ensemble de journaux dans PROCESSDET_L en mode Bulk
        /// </summary>
        /// <param name="pLogScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        private int SQLWriteProcessDetLBulk(LogScope pLogScope, IEnumerable<LogUnit> pData)
        {
            int nbWrite = 0;
            if ((pData != default(IEnumerable<LogUnit>)) && (pData.Count() > 0))
            {
                m_SGBDLock.Wait();
                try
                {
                    if (m_ProcessDetLDatTbl == default(DataTable))
                    {
                        InitDataTableProcessDet();
                    }
                    else
                    {
                        m_ProcessDetLDatTbl.Clear();
                    }
                    foreach (LogUnit log in pData)
                    {
                        DataRow logRow = m_ProcessDetLDatTbl.NewRow();
                        if ((log != default(LogUnit)) && (log.LogData != default(LoggerData)))
                        {
                            if (log.LogData.LogScope.IdPROCESS_L == 0)
                            {
                                // Dans le cas où l'IdProcess ne serait pas présent, prendre celui du LogScope en paramètre (mais ne devrait pas se produire)
                                log.LogData.LogScope.IdPROCESS_L = pLogScope.IdPROCESS_L;
                            }
                            // Alimentation et ajout du DataRow
                            if (SetProcessDetLRow(logRow, log))
                            {
                                m_ProcessDetLDatTbl.Rows.Add(logRow);
                            }
                        }
                    }
                    SQLBulkCopy("PROCESSDET_L", m_ProcessDetLDatTbl);
                    nbWrite = m_ProcessDetLDatTbl.Rows.Count;
                }
                finally
                {
                    m_ProcessDetLDatTbl.Clear();
                    m_SGBDLock.Release();
                }
            }
            return nbWrite;
        }
        #endregion SQLWriteProcessDetLBulk

        #region SQLWriteProcessDetLInsert
        /// <summary>
        /// Ecriture d'un ensemble de journaux dans PROCESSDET_L en mode insert ligne par ligne
        /// </summary>
        /// <param name="pLogScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        private int SQLWriteProcessDetLInsert(LogScope pLogScope, IEnumerable<LogUnit> pData)
        {
            int nbWrite = 0;
            if ((pData != default(IEnumerable<LogUnit>)) && (pData.Count() > 0))
            {
                m_SGBDLock.Wait();
                try
                {
                    QueryParameters queryParameters = QueryParamInsertPROCESSDET_L();
                    foreach (LogUnit log in pData)
                    {
                        if ((log != default(LogUnit)) && (log.LogData != default(LoggerData)))
                        {
                            if (log.LogData.LogScope.IdPROCESS_L == 0)
                            {
                                // Dans le cas où l'IdProcess ne serait pas présent, prendre celui du LogScope en paramètre (mais ne devrait pas se produire)
                                log.LogData.LogScope.IdPROCESS_L = pLogScope.IdPROCESS_L;
                            }
                            // Alimentation des parameters
                            SetProcessDetLParameters(queryParameters.Parameters, log);
                            // Insert
                            nbWrite += DataHelper.ExecuteNonQuery(m_ConnectionString, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                        }
                    }
                }
                finally
                {
                    m_SGBDLock.Release();
                }
            }
            return nbWrite;
        }
        #endregion SQLWriteProcessDetLInsert

        #region private SubStringOrDBNull
        /// <summary>
        /// Substring de pData ou DBNull si pData est vide
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pMaxSize"></param>
        /// <returns></returns>
        private object SubStringOrDBNull(string pData, int pMaxSize)
        {
            object ret = Convert.DBNull;
            // PM 20201126 [XXXXXX] Autoriser les paramétres chaine vide
            //if (StrFunc.IsFilled(pData))
            if (pData != default)
            {
                ret = pData.Substring(0, System.Math.Min(pData.Length, pMaxSize));
            }
            return ret;
        }
        #endregion private SubStringOrDBNull
        #endregion private Methods

        #region public Methods
        #region public SQLWriteProcessDetL
        /// <summary>
        /// Ecriture d'un ensemble de journaux dans PROCESSDET_L
        /// </summary>
        /// <param name="pLogScope"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public int SQLWriteProcessDetL(LogScope pLogScope, IEnumerable<LogUnit> pData)
        {
            int nbWrite = 0;
            if ((pData != default(IEnumerable<LogUnit>)) && (pData.Count() > 0))
            {
                if (m_SGBDType == DbSvrType.dbSQL)
                {
                    nbWrite = SQLWriteProcessDetLBulk(pLogScope, pData);
                    //nbWrite = SQLWriteProcessDetLInsert(pLogScope, pData);
                }
                else if (m_SGBDType == DbSvrType.dbORA)
                {
                    nbWrite = SQLWriteProcessDetLInsert(pLogScope, pData);
                }
                // FI 20200909 [XXXXX] Alimentation de ATTACHEDDOC lorsque le message dépasse les 4000 caractères (peut se produire sur des exceptions, notamment sur les SpheresExceptions puisque le propriété message contient la stack)   
                foreach (LogUnit item in pData.Where(x => StrFunc.IsFilled(x.LogData.Message) && x.LogData.Message.Length > MaxLenghtMessage))
                {
                    string fileName = $"LogMessageComplete{FileTools.ReplaceFilenameInvalidChar(DtFunc.DateTimeToString(item.LogData.DtUtcLog, "yyyy-MM-ddTHH:mm:ss.fffZ"))}.log";
                    string fileNameFull = $@"{AppDomain.CurrentDomain.BaseDirectory}\{fileName}";
                    FileTools.WriteStringToFile(item.LogData.Message, fileNameFull);
                    byte[] data = FileTools.ReadFileToBytes(fileNameFull);
                    LogTools.AddAttachedDoc(m_ConnectionString, item.LogData.LogScope.IdPROCESS_L, item.LogData.InstanceIda, data, fileName, Cst.TypeMIME.Text.ALL);
                }
            }
            return nbWrite;
        }
        #endregion public SQLWriteProcessDetL

        #region public SQLUpdateProcessDetL
        /// <summary>
        /// Mise à jour de la table PROCESSDET_L
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public int SQLUpdateProcessDetL(IEnumerable<LogUnit> pData)
        {
            int nbUpdate = 0;
            if ((pData != default(IEnumerable<LogUnit>)) && (pData.Count() > 0))
            {
                foreach (LogUnit log in pData)
                {
                    LoggerData logData = log.LogData;
                    if (logData != default(LoggerData))
                    {
                        int idLink = logData.LogScope.IdPROCESS_L;
                        if (idLink != 0)
                        {
                            m_SGBDLock.Wait();
                            try
                            {
                                QueryParameters queryParameters = QueryParamUpdatePROCESSDET_L();
                                //
                                queryParameters.Parameters["IDPROCESS_L"].Value = idLink;
                                queryParameters.Parameters["IDSTPROCESS"].Value = logData.LogLevel.ToString().ToUpper();
                                queryParameters.Parameters["DTSTPROCESS"].Value = logData.DtUtcLog;
                                //
                                if (logData.SysMsg != default)
                                {
                                    queryParameters.Parameters["SYSCODE"].Value = logData.SysMsg.SysCode.ToString();
                                    queryParameters.Parameters["SYSNUMBER"].Value = logData.SysMsg.SysNumber;
                                    queryParameters.Parameters["MESSAGE"].Value = Convert.DBNull;
                                }
                                else
                                {
                                    queryParameters.Parameters["SYSCODE"].Value = Convert.DBNull;
                                    queryParameters.Parameters["SYSNUMBER"].Value = Convert.DBNull;
                                    queryParameters.Parameters["MESSAGE"].Value = (logData.Message != default) ? logData.Message.Substring(0, System.Math.Min(MaxLenghtMessage, logData.Message.Length)) : Convert.DBNull;
                                }
                                //
                                if (m_SGBDType == DbSvrType.dbSQL)
                                {
                                    nbUpdate += DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                                }
                                else if (m_SGBDType == DbSvrType.dbORA)
                                {
                                    // PB sur Oracle avec DTSTPROCESS en parameters, ok avec GetQueryReplaceParameters
                                    nbUpdate += DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.GetQueryReplaceParameters());
                                }
                            }
                            catch (Exception e)
                            {
                                LoggerServiceAppTool.TraceManager.TraceError(this, string.Format( "Error on update PROCESSDET_L (id:{0})", idLink));
                                throw e;
                            }
                            finally
                            {
                                m_SGBDLock.Release();
                            }
                        }
                    }
                    log.IsToUpdate = false;
                }
            }
            return nbUpdate;
        }
        #endregion public SQLUpdateProcessDetL

        #region public WorstLogLevel
        /// <summary>
        /// Obtient le pire LogLevel du scope parmi Info, Warning, Error et Critical présent aprés la réception du journal pFirstLog (pour compatibilité avec IO)
        /// </summary>
        /// <param name="pFirstLog"></param>
        /// <returns></returns>
        public LogLevelEnum WorstLogLevel(LogUnit pFirstLog)
        {
            LogLevelEnum worstLogLevel = LogLevelEnum.Info;
            if ((pFirstLog != default(LogUnit)) && (pFirstLog.LogData != default(LoggerData)))
            {
                m_SGBDLock.Wait();
                try
                {
                    QueryParameters queryParameters = QueryParamDistinctWorstStatus();
                    int idLink = pFirstLog.LogData.LogScope.IdPROCESS_L;
                    if (idLink != 0)
                    {
                        List<LogLevelEnum> distinctLogLevel = new List<LogLevelEnum>();
                        queryParameters.Parameters["IDPROCESS_L"].Value = idLink;
                        queryParameters.Parameters["DTSTPROCESS"].Value = pFirstLog.LogData.DtUtcLog;
                        //
                        using (IDataReader dr = DataHelper.ExecuteReader(m_ConnectionString, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                        {
                            while (dr.Read())
                            {
                                string IdStProcess = Convert.ToString(dr["IDSTPROCESS"]);
                                switch (IdStProcess)
                                {
                                    case "CRITICAL":
                                        distinctLogLevel.Add(LogLevelEnum.Critical);
                                        break;
                                    case "ERROR":
                                        distinctLogLevel.Add(LogLevelEnum.Error);
                                        break;
                                    case "WARNING":
                                        distinctLogLevel.Add(LogLevelEnum.Warning);
                                        break;
                                }
                            }
                        }
                        //
                        if (distinctLogLevel.Contains(LogLevelEnum.Critical))
                        {
                            worstLogLevel = LogLevelEnum.Critical;
                        }
                        else if (distinctLogLevel.Contains(LogLevelEnum.Error))
                        {
                            worstLogLevel = LogLevelEnum.Error;
                        }
                        else if (distinctLogLevel.Contains(LogLevelEnum.Warning))
                        {
                            worstLogLevel = LogLevelEnum.Warning;
                        }
                    }
                }
                finally
                {
                    m_SGBDLock.Release();
                }
            }
            return worstLogLevel;
        }
        #endregion public WorstLogLevel

        #region public SQLWriteProcessL
        /// <summary>
        /// Ecriture dans PROCESS_L (Mode compatibilité)
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public bool SQLWriteProcessL(LogUnit pData)
        {
            bool isOk = false;
            if ((pData != default(LogUnit)) && (pData.LogData != default(LoggerData)) && (pData.LogData.LogScope != default(LogScope)))
            {
                IDbTransaction dbTransaction = null;
                m_SGBDLock.Wait();
                try
                {
                    int id = 0;
                    int nbTry = 0;
                    // Gestion de 2 tentatives de création de la ligne dans PROCESS_L
                    // Cela afin de gérer le cas où la base de données a été remontée ou redémarrée sans que le service n'ai été lui-même redémarré.
                    while (id == 0 && nbTry < 2)
                    {
                        try
                        {
                            dbTransaction = DataHelper.BeginTran(m_ConnectionString);
                            //
                            SQLUP.GetId(out id, dbTransaction, SQLUP.IdGetId.PROCESS_L);
                            // FI 20201013 [XXXXX] Il faut valioriser pData.LogData.LogScope.IdPROCESS_L car utiliser dans QueryParamInsertPROCESS_L
                            pData.LogData.LogScope.IdPROCESS_L = id;
                            QueryParameters queryParameters = QueryParamInsertPROCESS_L();
                            SetProcessLParameters(queryParameters.Parameters, pData);
                            //
                            DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                            DataHelper.CommitTran(dbTransaction);
                        }
                        catch (Exception e)
                        {
                            id = 0;
                            if (nbTry > 0)
                            {
                                // En cas d'erreur sur la 2ème tentative on propage l'erreur
                                LoggerServiceAppTool.TraceManager.TraceError(this, "Error on insert into PROCESS_L after 2 try.");
                                throw e;
                            }
                            try
                            {
                                // On tente un rollback en cas d'erreur sur la 1ère tentative
                                DataHelper.RollbackTran(dbTransaction);
                            }
                            catch { }
                        }
                        nbTry += 1;
                    }
                    // PM 20201022 Mise à jour du IdPROCESS_L du LogScope tout à la fin afin qu'il reste à 0 en cas de plantage
                    pData.LogData.LogScope.IdPROCESS_L = id;
                    pData.IsSaved = true;
                    isOk = true;
                }
                catch (Exception e)
                {
                    isOk = false;
                    LoggerServiceAppTool.TraceManager.TraceError(this, "Error on insert into PROCESS_L");
                    throw e;
                }
                finally
                {
                    m_SGBDLock.Release();
                    //
                    if (false == isOk)
                    {
                        if (null != dbTransaction)
                        {
                            DataHelper.RollbackTran(dbTransaction);
                        }
                    }
                }
            }
            return isOk;
        }
        #endregion public SQLWriteProcessL

        #region public SQLUpdateProcessL
        /// <summary>
        /// Mise à jour de la table PROCESS_L
        /// </summary>
        /// <param name="pIdProcess"></param>
        /// <param name="pIdStProcess"></param>
        /// <param name="pDtStProcess"></param>
        /// <param name="pDtProcessEnd"></param>
        /// <returns></returns>
        public int SQLUpdateProcessL(int pIdProcess, string pIdStProcess, DateTime pDtStProcess, DateTime pDtProcessEnd)
        {
            int nbUpdate = 0;
            if ((pIdProcess >0) && (pDtStProcess != default) && StrFunc.IsFilled(pIdStProcess))
            {
                m_SGBDLock.Wait();
                try
                {
                    QueryParameters queryParameters = QueryParamUpdatePROCESS_L();
                    queryParameters.Parameters["IDPROCESS_L"].Value = pIdProcess;
                    queryParameters.Parameters["IDSTPROCESS"].Value = pIdStProcess;
                    queryParameters.Parameters["DTSTPROCESS"].Value = pDtStProcess;
                    queryParameters.Parameters["DTPROCESSEND"].Value = pDtProcessEnd;
                    //
                    nbUpdate = DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                }
                finally
                {
                    m_SGBDLock.Release();
                }
            }
            return nbUpdate;
        }

        /// <summary>
        /// Mise à jour de la colonne DTPROCESSEND de la table PROCESS_L
        /// </summary>
        /// <param name="pIdProcess"></param>
        /// <param name="pDtProcessEnd"></param>
        /// <returns></returns>
        public int SQLUpdateProcessL(int pIdProcess, DateTime pDtProcessEnd)
        {
            int nbUpdate = 0;
            if ((pIdProcess > 0) && (pDtProcessEnd != default))
            {
                m_SGBDLock.Wait();
                try
                {
                    QueryParameters queryParameters = QueryParamUpdatePROCESS_L_DTPROCESSEND();
                    queryParameters.Parameters["IDPROCESS_L"].Value = pIdProcess;
                    queryParameters.Parameters["DTPROCESSEND"].Value = pDtProcessEnd;
                    //
                    nbUpdate = DataHelper.ExecuteNonQuery(queryParameters.Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                }
                finally
                {
                    m_SGBDLock.Release();
                }
            }
            return nbUpdate;
        }    
        #endregion public SQLUpdateProcessL
        #endregion public Methods
        #endregion Methods
    }
}

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLog
{
    #region ProcessLogQuery
    public class ProcessLogQuery
    {
        #region Members
        private DataParameter paramIdProcess;
        private DataParameter paramIdTRK_L;
        private DataParameter paramProcess;
        private DataParameter paramIdStProcess;
        private DataParameter paramDtStProcess;
        private DataParameter paramIdData;
        private DataParameter paramIdDataIdent;
        private DataParameter paramCode;
        private DataParameter paramNumber;
        private DataParameter paramMessage;
        private DataParameter paramData1;
        private DataParameter paramData2;
        private DataParameter paramData3;
        private DataParameter paramData4;
        private DataParameter paramData5;
        private DataParameter paramData6;
        private DataParameter paramData7;
        private DataParameter paramData8;
        private DataParameter paramData9;
        private DataParameter paramData10;
        private DataParameter paramLoDataTxt;
        private DataParameter paramQueueMsg;
        private DataParameter paramIdA;
        private DataParameter paramHostName;
        private DataParameter paramAppName;
        private DataParameter paramAppVersion;
        private DataParameter paramExtlLink;
        private DataParameter paramRowAttribut;
        private DataParameter paramLevelOrder;
        #endregion Members
        #region Constructor
        public ProcessLogQuery(string pCS)
        {
            InitParameter(pCS);
        }
        #endregion
        #region Methods
        #region GetQuerySelectPROCESS_L
        public QueryParameters GetQuerySelectPROCESS_L(string pCS)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdProcess);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDPROCESS_L, IDTRK_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER, " + Cst.CrLf;
            sqlQuery += "IDA, HOSTNAME, APPNAME, APPVERSION, EXTLLINK, ROWATTRIBUT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDPROCESS_L = @IDPROCESS_L)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion
        #region GetQuerySelectPROCESS_L
        /// <summary>
        /// Lecture de PROCESS_L via IDTRK_L
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        public QueryParameters GetQuerySelectPROCESS_L_ByTracker(string pCS)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdTRK_L);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDPROCESS_L, IDTRK_L, PROCESS, IDSTPROCESS, DTSTPROCESS, SYSCODE, SYSNUMBER, " + Cst.CrLf;
            sqlQuery += "IDA, HOSTNAME, APPNAME, APPVERSION, EXTLLINK, ROWATTRIBUT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDTRK_L = @IDTRK_L)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion
        #region GetQueryInsertPROCESS_L
        public QueryParameters GetQueryInsertPROCESS_L(string pCS)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdProcess);
            parameters.Add(paramIdTRK_L);
            parameters.Add(paramProcess);
            parameters.Add(paramIdStProcess);
            parameters.Add(paramDtStProcess);
            parameters.Add(paramIdData);
            parameters.Add(paramIdDataIdent);
            parameters.Add(paramCode);
            parameters.Add(paramNumber);
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
            parameters.Add(paramExtlLink);
            parameters.Add(paramRowAttribut);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + Cst.CrLf;
            sqlQuery += @"(IDPROCESS_L, IDTRK_L, PROCESS, IDSTPROCESS, DTSTPROCESS, " + Cst.CrLf;
            sqlQuery += @"IDDATA, IDDATAIDENT, " + Cst.CrLf;
            sqlQuery += @"SYSCODE, SYSNUMBER, MESSAGE, DATA1, DATA2, DATA3, DATA4, DATA5, LODATATXT, QUEUEMSG, " + Cst.CrLf;
            sqlQuery += @"IDA, HOSTNAME, APPNAME, APPVERSION, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@IDPROCESS_L, @IDTRK_L, @PROCESS, @IDSTPROCESS, @DTSTPROCESS," + Cst.CrLf;
            sqlQuery += @"@IDDATA,@IDDATAIDENT," + Cst.CrLf;
            sqlQuery += @"@SYSCODE, @SYSNUMBER, @MESSAGE, @DATA1, @DATA2, @DATA3, @DATA4, @DATA5, @LODATATXT, @QUEUEMSG, " + Cst.CrLf;
            sqlQuery += @"@IDA, @HOSTNAME, @APPNAME, @APPVERSION, @EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;
            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion
        #region GetQueryUpdatePROCESS_L
        // RD 20161212 [22679] Modify
        public QueryParameters GetQueryUpdatePROCESS_L(string pCS, string pStatus)
        {
            DataParameter paramTmp = null;
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdProcess);
            parameters.Add(paramIdTRK_L);
            parameters.Add(paramIdStProcess);
            parameters.Add(paramDtStProcess);
            parameters.Add(paramCode);
            parameters.Add(paramNumber);
            parameters.Add(paramMessage);
            parameters.Add(paramData1);
            parameters.Add(paramData2);
            parameters.Add(paramData3);
            parameters.Add(paramData4);
            parameters.Add(paramData5);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + Cst.CrLf;
            // RD 20161212 [22679] Mise à jour de IDSTPROCESS avec un status d'importance inférieur à celui déjà en place.
            // 1- Toujours mettre à jour avec le statut ERROR
            // 2- Mettre à jour avec le statut WARNING uniquement si le statut en place n'est pas ERROR
            // 3- Mettre à jour avec les autres statuts, uniquement si le statut en place n'est pas ERROR ou WARNING
            //sqlQuery += SQLCst.SET + "IDSTPROCESS=@IDSTPROCESS, DTSTPROCESS=@DTSTPROCESS," + Cst.CrLf;
            sqlQuery += SQLCst.SET + "DTSTPROCESS=@DTSTPROCESS," + Cst.CrLf;

            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.NONE;
            if ((StrFunc.IsFilled(pStatus)) && Enum.IsDefined(typeof(ProcessStateTools.StatusEnum), pStatus))
                status = (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), pStatus, true);

            switch (status)
            {
                case ProcessStateTools.StatusEnum.ERROR:
                    sqlQuery += "IDSTPROCESS=@IDSTPROCESS," + Cst.CrLf;
                    break;
                case ProcessStateTools.StatusEnum.WARNING:
                    sqlQuery += "IDSTPROCESS = case when (IDSTPROCESS = @STATUSERROR) then IDSTPROCESS else @IDSTPROCESS end," + Cst.CrLf;
                    paramTmp = new DataParameter(pCS, "STATUSERROR", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                    paramTmp.Value = ProcessStateTools.StatusError;
                    parameters.Add(paramTmp);
                    break;
                default:
                    sqlQuery += "IDSTPROCESS = case when (IDSTPROCESS in (@STATUSERROR,@STATUSWARNING)) then IDSTPROCESS else @IDSTPROCESS end," + Cst.CrLf;
                    paramTmp = new DataParameter(pCS, "STATUSERROR", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                    paramTmp.Value = ProcessStateTools.StatusError;
                    parameters.Add(paramTmp);
                    paramTmp = new DataParameter(pCS, "STATUSWARNING", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                    paramTmp.Value = ProcessStateTools.StatusWarning;
                    parameters.Add(paramTmp);
                    break;
            }
            sqlQuery += @"IDTRK_L=@IDTRK_L," + Cst.CrLf;
            sqlQuery += @"SYSCODE=@SYSCODE, SYSNUMBER=@SYSNUMBER, MESSAGE=@MESSAGE," + Cst.CrLf;
            sqlQuery += @"DATA1=@DATA1, DATA2=@DATA2, DATA3=@DATA3, DATA4=@DATA4, DATA5=@DATA5" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "IDPROCESS_L=@IDPROCESS_L";
            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion
        #region GetQueryInsertPROCESSDET_L
        public QueryParameters GetQueryInsertPROCESSDET_L(string pCS)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdProcess);
            parameters.Add(paramProcess);
            parameters.Add(paramIdStProcess);
            parameters.Add(paramDtStProcess);
            parameters.Add(paramIdData);
            parameters.Add(paramIdDataIdent);
            parameters.Add(paramCode);
            parameters.Add(paramNumber);
            parameters.Add(paramMessage);
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
            parameters.Add(paramExtlLink);
            parameters.Add(paramRowAttribut);
            parameters.Add(paramLevelOrder);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.PROCESSDET_L.ToString() + Cst.CrLf;
            sqlQuery += @"(IDPROCESS_L, PROCESS, IDSTPROCESS, DTSTPROCESS," + Cst.CrLf;
            sqlQuery += @"IDDATA,IDDATAIDENT, SYSCODE, SYSNUMBER, MESSAGE, " + Cst.CrLf;
            sqlQuery += @"DATA1, DATA2, DATA3, DATA4, DATA5, " + Cst.CrLf;
            sqlQuery += @"DATA6, DATA7, DATA8, DATA9, DATA10, " + Cst.CrLf;
            sqlQuery += @"LEVELORDER, EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;
            sqlQuery += @"values" + Cst.CrLf;
            sqlQuery += @"(@IDPROCESS_L, @PROCESS, @IDSTPROCESS, @DTSTPROCESS," + Cst.CrLf;
            sqlQuery += @"@IDDATA, @IDDATAIDENT, @SYSCODE, @SYSNUMBER, @MESSAGE, " + Cst.CrLf;
            sqlQuery += @"@DATA1, @DATA2, @DATA3, @DATA4, @DATA5, " + Cst.CrLf;
            sqlQuery += @"@DATA6, @DATA7, @DATA8, @DATA9, @DATA10, @LEVELORDER, @EXTLLINK, @ROWATTRIBUT)";
            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion
        /// <summary>
        /// Retourne la requête SQL qui supprime les enregisrements détails d'un logs
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProcessType"></param>
        /// <returns></returns>
        /// FI 20130206 [] ajout du paramètre pProcess 
        /// Cette nouveauté est nécessaire car un même log peut désormais contenir n process
        public QueryParameters GetQueryDeletePROCESSDET_L(string pCS, Nullable<Cst.ProcessTypeEnum> pProcess)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdProcess);
            if (null != pProcess)
                parameters.Add(paramProcess);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.PROCESSDET_L.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "IDPROCESS_L=@IDPROCESS_L";
            if (null != pProcess)
                sqlQuery += SQLCst.AND + "PROCESS=@PROCESS";

            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            return ret;
        }

        #region GetQueryUpdatePROCESSDET_L
        /// <summary>
        /// Retourne la requête SQL qui met à jour le Statut d'un enregisrement détail d'un logs
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pStartLogDet"></param>
        /// <param name="pEndLogDet"></param>
        /// <returns></returns>
        public QueryParameters GetQueryUpdatePROCESSDET_L(string pCS, int pStartLogDet, int pEndLogDet)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(paramIdProcess);
            parameters.Add(paramIdStProcess);
            //parameters.Add(paramDtStProcess);
            parameters.Add(paramCode);
            parameters.Add(paramNumber);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.PROCESSDET_L.ToString() + Cst.CrLf;
            //sqlQuery += SQLCst.SET + "IDSTPROCESS=@IDSTPROCESS, DTSTPROCESS=@DTSTPROCESS" + Cst.CrLf;
            sqlQuery += SQLCst.SET + "IDSTPROCESS=@IDSTPROCESS" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDPROCESS_L=@IDPROCESS_L)" + Cst.CrLf;
            if (pStartLogDet > 0)
            {
                sqlQuery += SQLCst.AND + "(IDPROCESSDET_L>=@IDDETSTART)" + Cst.CrLf;
                parameters.Add(new DataParameter(pCS, "IDDETSTART", DbType.Int32), pStartLogDet);
            }
            if (pEndLogDet > 0)
            {
                sqlQuery += SQLCst.AND + "(IDPROCESSDET_L<=@IDDETEND)" + Cst.CrLf;
                parameters.Add(new DataParameter(pCS, "IDDETEND", DbType.Int32), pEndLogDet);
            }
            sqlQuery += SQLCst.AND + "SYSCODE=@SYSCODE" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "SYSNUMBER=@SYSNUMBER" + Cst.CrLf;

            QueryParameters ret = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            return ret;
        }
        #endregion

        #region InitParameter
        private void InitParameter(string pCS)
        {
            paramIdProcess = new DataParameter(pCS, "IDPROCESS_L", DbType.Int32);
            paramIdTRK_L = new DataParameter(pCS, "IDTRK_L", DbType.Int32);
            paramProcess = new DataParameter(pCS, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramIdStProcess = new DataParameter(pCS, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN);
            paramDtStProcess = new DataParameter(pCS, "DTSTPROCESS", DbType.DateTime);
            paramIdData = new DataParameter(pCS, "IDDATA", DbType.Int32);
            paramIdDataIdent = new DataParameter(pCS, "IDDATAIDENT", DbType.AnsiString, 64);
            paramCode = new DataParameter(pCS, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN);
            paramNumber = new DataParameter(pCS, "SYSNUMBER", DbType.Int32);
            paramMessage = new DataParameter(pCS, "MESSAGE", DbType.AnsiString, 4000);
            // RD 20120928 [18155] 
            // Elargissement (de 64 à 128) des colonnes DATA1..DATA10
            paramData1 = new DataParameter(pCS, "DATA1", DbType.AnsiString, 128);
            paramData2 = new DataParameter(pCS, "DATA2", DbType.AnsiString, 128);
            paramData3 = new DataParameter(pCS, "DATA3", DbType.AnsiString, 128);
            paramData4 = new DataParameter(pCS, "DATA4", DbType.AnsiString, 128);
            paramData5 = new DataParameter(pCS, "DATA5", DbType.AnsiString, 128);
            paramData6 = new DataParameter(pCS, "DATA6", DbType.AnsiString, 128);
            paramData7 = new DataParameter(pCS, "DATA7", DbType.AnsiString, 128);
            paramData8 = new DataParameter(pCS, "DATA8", DbType.AnsiString, 128);
            paramData9 = new DataParameter(pCS, "DATA9", DbType.AnsiString, 128);
            paramData10 = new DataParameter(pCS, "DATA10", DbType.AnsiString, 128);
            paramLoDataTxt = new DataParameter(pCS, "LODATATXT", DbType.String);
            paramQueueMsg = new DataParameter(pCS, "QUEUEMSG", DbType.String);
            paramIdA = new DataParameter(pCS, "IDA", DbType.Int32);
            paramHostName = new DataParameter(pCS, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN);
            paramAppName = new DataParameter(pCS, "APPNAME", DbType.AnsiString, 64);
            paramAppVersion = new DataParameter(pCS, "APPVERSION", DbType.AnsiString, 64);
            paramExtlLink = new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            paramRowAttribut = new DataParameter(pCS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN);
            paramLevelOrder = new DataParameter(pCS, "LEVELORDER", DbType.Int32);
        }
        #endregion
        #endregion Methods
    }
    #endregion
}

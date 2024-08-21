#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Restriction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

#endregion using directives

namespace EFS.Common.Log
{

    #region Enums
    /// <summary>
    /// 
    /// </summary>
    /// FI 20240111 [WI793] Add (Remplace ErrorManager.ErrorDetail)
    public enum LogLevelDetail
    {
        /// <summary>
        /// Réservé au paramétrage (Héritage de paramétrage) 
        /// </summary>
        INHERITED = 0,
        /// <summary>
        /// Uniquement les erreurs (None, Critical, Error)
        /// <para>Sans équivalence v8</para>
        /// </summary>
        LEVEL1 = 1,
        /// <summary>
        /// Uniquement les erreurs et avertissements (None, Critical, Error, Warning)
        /// <para>Equivalence v8: NONE</para>
        /// </summary>
        LEVEL2 = 2,
        /// <summary>
        /// Tout sauf Trace et Debug (None, Critical, Error, Warning, Info)
        /// <para>Equivalence v8: EXPANDED/DEFAULT</para>
        /// </summary>
        LEVEL3 = 3,
        /// <summary>
        /// Tout sauf Trace (None, Critical, Error, Warning, Info, Debug)
        /// <para>Equivalence v8: FULL</para>
        /// </summary>
        LEVEL4 = 4,
        /// <summary>
        /// Détail maximal (None, Critical, Error, Warning, Info, Debug, Trace)
        /// <para>Sans équivalence v8</para>
        /// </summary>
        LEVEL5 = 5,
    };
    #endregion


    #region LogConsultMode
    /// <summary>
    /// Représente un type de consultation des logs
    /// <para>1er Type Admin => L'utilisateur consulte tous les logs sans restrictions sur les droits</para>
    /// <para>2nd Type Normal => L'utilisateur consulte les logs qui concerne uniquement les éléments sur lesquels il a des droits
    /// <para>Ex Spheres® affiche le log d'une tâche IO si l'utilisateur possède les droits sur cette tâche</para>
    /// </para>
    /// </summary>
    public enum LogConsultMode
    {
        /// <summary>
        /// Consultation des lignes du TRACKER_L et du  PROCESS_L "visibles"
        /// <para>Utilisation de SESSIONRESTRICT si user non admin</para>
        /// <para>Remarque : un user Admin consulte nécessairement toutes les lignes du tracker</para>
        /// </summary>
        Normal,
        /// <summary>
        /// Consultation de toutes les lignes du TRACKER_L et du  PROCESS_L
        /// <para>Remarque : un user non admin consulte toutes les lignes du tracker</para>
        /// <para>Ce mode est à utiliser pour débrayer les jointures sur SESSIONRESTRICT en cas de mauvaises performances</para>
        /// </summary>
        Admin
    }
    #endregion

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

            DataParameter paramTmp;
            switch (status)
            {
                case ProcessStateTools.StatusEnum.ERROR:
                    sqlQuery += "IDSTPROCESS=@IDSTPROCESS," + Cst.CrLf;
                    break;
                case ProcessStateTools.StatusEnum.WARNING:
                    sqlQuery += "IDSTPROCESS = case when (IDSTPROCESS = @STATUSERROR) then IDSTPROCESS else @IDSTPROCESS end," + Cst.CrLf;
                    paramTmp = new DataParameter(pCS, "STATUSERROR", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN)
                    {
                        Value = ProcessStateTools.StatusError
                    };
                    parameters.Add(paramTmp);
                    break;
                default:
                    sqlQuery += "IDSTPROCESS = case when (IDSTPROCESS in (@STATUSERROR,@STATUSWARNING)) then IDSTPROCESS else @IDSTPROCESS end," + Cst.CrLf;
                    paramTmp = new DataParameter(pCS, "STATUSERROR", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN)
                    {
                        Value = ProcessStateTools.StatusError
                    };
                    parameters.Add(paramTmp);
                    paramTmp = new DataParameter(pCS, "STATUSWARNING", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN)
                    {
                        Value = ProcessStateTools.StatusWarning
                    };
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
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
            paramDtStProcess = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSTPROCESS); // FI 20201006 [XXXXX] Appel à GetParameter
            paramIdData = new DataParameter(pCS, "IDDATA", DbType.Int32);
            paramIdDataIdent = new DataParameter(pCS, "IDDATAIDENT", DbType.AnsiString, 64);
            paramCode = new DataParameter(pCS, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN);
            paramNumber = new DataParameter(pCS, "SYSNUMBER", DbType.Int32);
            paramMessage = new DataParameter(pCS, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN);
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
    #region ProcessLogBase
    /// <summary>
    /// class de base d'un log de process (log entête(PROCESS_L) ou log détail(PROCESSDET_L))
    /// </summary>
    public class ProcessLogBase
    {
        #region Members
        /// <summary>
        /// Identitiant unique du process maître(IDPROCESS_L)
        /// </summary>
        private int m_IdProcess;
        /// <summary>
        /// Représente le type de process 
        /// </summary>
        private Cst.ProcessTypeEnum m_ProcessType;
        /// <summary>
        /// Représente l'information du log 
        /// </summary>
        private ProcessLogInfo m_Info;
        /// <summary>
        /// 
        /// </summary>
        private string m_ExtlLink;
        private string m_RowAttribut;
        #endregion Members
        #region Accessors
        public int IdProcess
        {
            set { m_IdProcess = value; }
            get { return m_IdProcess; }
        }
        public Cst.ProcessTypeEnum ProcessType
        {
            set { m_ProcessType = value; }
            get { return (m_ProcessType); }
        }
        public ProcessLogInfo Info
        {
            set { m_Info = value; }
            get { return (m_Info); }
        }
        public string ExtlLink
        {
            set { m_ExtlLink = value; }
            get { return (m_ExtlLink); }
        }
        public string RowAttribut
        {
            set { m_RowAttribut = value; }
            get { return (m_RowAttribut); }
        }
        #endregion Accessors
        #region Constructor
        public ProcessLogBase(Cst.ProcessTypeEnum pProcessType)
        {
            m_ProcessType = pProcessType;
            m_ExtlLink = string.Empty;
            m_RowAttribut = Cst.RowAttribut_System;
        }
        #endregion Constructor
    }
    #endregion ProcessLogBase

    #region ProcessLog
    /// <summary>
    /// Gère le log d'un process
    /// </summary>
    // EG 20180205 [23769] Add m_LogLock  
    // EG 20190114 ProcessLog Refactoring
    public class ProcessLog : DatetimeProfiler
    {
        #region Members
        /// <summary>
        /// ConnectionString où le log va être écrit
        /// </summary>

        public string cs;
        public ProcessLogHeader header;
        // EG 20190114 ProcessLog Refactoring
        public List<ProcessLogDetail> detail;
        

        #endregion Members
        #region Constructors
        /// <summary>
        /// Avec ce constructor Lecture d'un LOG existant
        /// <para>Initialisation de la date de début avec la date du SGBD</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pSession"></param>
        public ProcessLog(string pCs, Cst.ProcessTypeEnum pProcessType, AppSession pSession, ProcessLogInfo pLogInfoHeader, int pIdPROCESS)
            : this(pCs, pProcessType, pSession, pLogInfoHeader)
        {
            header.IdProcess = pIdPROCESS;
        }
        /// <summary>
        /// Avec ce constructor PROCESS_L.MESSAGE est alimenté avec la ressource associée à {pProcessType}
        /// <para>Initialisation de la date de début avec la date du SGBD</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pSession"></param>
        // PL 20200826 Use Utc Date  
        public ProcessLog(string pCs, Cst.ProcessTypeEnum pProcessType, AppSession pSession)
            : base(OTCmlHelper.GetDateSysUTC(pCs))
        {
            cs = pCs;
            Initialize(pProcessType, pSession);

            header.Info = new ProcessLogInfo
            {
                status = ProcessStateTools.StatusUnknownEnum.ToString()
            };
            header.Info.SetMessageAndData(new string[] { header.Title });
        }
        /// <summary>
        /// Avec ce constructor, possibilité d'alimenté les informations Message,DATA1,etc...
        /// <para>Initialisation de la date de début avec la date du SGBD</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pSession"></param>
        /// <param name="pLogInfoHeader"></param>
        // PL 20200826 Use Utc Date  
        public ProcessLog(string pCs, Cst.ProcessTypeEnum pProcessType, AppSession pSession, ProcessLogInfo pLogInfoHeader)
            //: base(OTCmlHelper.GetDateSys(pCs))
            : base(OTCmlHelper.GetDateSysUTC(pCs))
        {
            cs = pCs;
            Initialize(pProcessType, pSession);
            header.Info = pLogInfoHeader;
        }
        #endregion
        #region Methods

        #region AddDetail
        /// <summary>
        /// Ajoute un enregistrement dans le log détail avec les informations liées à une exception 
        /// </summary>
        /// <param name="ex"></param>
        public void AddDetail(SpheresException2 ex)
        {
            AddDetail(ex.GetLogInfo(), LoggerTools.StatusToLogLevelEnum(ex.ProcessState.Status));
        }

        /// <summary>
        ///  Ajoute un enregistrement dans le log détail avec un message et un statut
        /// </summary>
        /// <param name="logInfo">représente le message et les data associées au message</param>
        /// <param name="logLevel"></param>
        public void AddDetail(string[] logInfo, LogLevelEnum logLevel)
        {
            ProcessLogInfo ProcessLogInfo = new ProcessLogInfo();
            ProcessLogInfo.status = logLevel.ToString();
            ProcessLogInfo.SetMessageAndData(logInfo);
            AddDetail(ProcessLogInfo);
        }

        /// <summary>
        ///  Ajoute un enregistrement dans le log détail 
        /// </summary>
        /// <param name="logInfo"></param>
        public void AddDetail(ProcessLogInfo logInfo)
        {

            ProcessLogDetail logDetail = new ProcessLogDetail(header, logInfo);

            if (DtFunc.IsDateTimeEmpty(logDetail.Info.dtstatus))
                logDetail.Info.dtstatus = GetDate();

            if (AppInstance.TraceManager != null)
            {
                // FI 20190716 [XXXXX] Akimentation de la trace avec la ressource SYSTEMMSG
                string message;
                if (!String.IsNullOrEmpty(logInfo.Code))
                    message = Ressource.GetSystemMsg(logInfo.Code, logInfo.Number.ToString());
                else
                    message = logInfo.message;

                for (int i = 1; i <= 10; i++)
                {
                    if (ReflectionTools.GetFieldByName(logInfo, "data" + i.ToString()) is string itemData)
                        message = message.Replace(StrFunc.AppendFormat("{{{0}}}", i.ToString()), itemData);
                }

                if (!String.IsNullOrEmpty(logInfo.Code))
                    AppInstance.TraceManager.TraceInformation(this, string.Format("Code:{0} Msg:{1}",
                                String.Format("{0}-{1:00000}", logInfo.Code, logInfo.Number), message));
                else
                    AppInstance.TraceManager.TraceInformation(this, string.Format("Msg:{0}", message));
            }

            if (null == detail)
                detail = new List<ProcessLogDetail>();

            detail.Add(logDetail);
        }
        #endregion





        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        // EG 20180525 [23979] IRQ Processing
        public void SetHeaderStatus(ProcessStateTools.StatusEnum pStatus)
        {
            if (false == ProcessStateTools.IsStatusErrorWarningInterrupt(header.Info.status))
                header.Info.status = pStatus.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        public void SQLUpdateHeader()
        {
            header.Info.dtstatus = GetDate();
            header.SQLUpdate(cs);
        }


        /// <summary>
        /// Ecrit dans les tables PROCESSS_L et PROCESSDET_L du log
        /// </summary>
        public void SQLWrite()
        {
            SQLWriteHeader();
            SQLWriteDetail();
        }

        /// <summary>
        /// Ecrit dans les tables PROCESSS_L
        /// </summary>
        public void SQLWriteHeader()
        {
            header.Info.dtstatus = GetDate();
            header.SQLWrite(cs);

        }

        /// <summary>
        /// Ecrit dans les tables PROCESSSDET_L
        /// </summary>
        // EG 20190114 ProcessLog Refactoring
        public void SQLWriteDetail()
        {
            if (null != detail)
            {
                detail.ForEach(item =>
                {
                    item.IdProcess = header.IdProcess;
                    item.SQLWrite(cs);
                });
                detail.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pSession"></param>
        private void Initialize(Cst.ProcessTypeEnum pProcessType, AppSession pSession)
        {
            header = new ProcessLogHeader(pProcessType, pSession);
        }
        #endregion Methods
    }
    #endregion ProcessLog

    #region ProcessLogHeader
    /// <summary>
    /// Représente l'entête d'un log de process (équivalent à un item de PROCESS_L)
    /// </summary>
    public class ProcessLogHeader : ProcessLogBase
    {
        #region Members
        /// <summary>
        /// Représente l'identifiant de la demande de process via Tracker
        /// </summary>
        private int m_IdTRK_L;
        /// <summary>
        /// Représente la session qui lance le process
        /// </summary>
        private readonly AppSession _session;

        #endregion Members
        #region Accessors
        /// <summary>
        /// Obtient l'application qui écrit dans le log
        /// </summary>
        public AppSession Session
        {
            get { return _session; }
        }
        /// <summary>
        /// Obtient la ressource associée au Process
        /// </summary>
        public string Title
        {
            get { return Ressource.GetString(ProcessType.ToString(), CultureInfo.InvariantCulture); }
        }
        /// <summary>
        /// Obtient ou définit la demande du tracker
        /// </summary>
        public int IdTRK_L
        {
            set { m_IdTRK_L = value; }
            get { return (m_IdTRK_L); }
        }
        #endregion Accessors
        #region Constructors
        public ProcessLogHeader(Cst.ProcessTypeEnum pProcessType, AppSession pSession)
            : base(pProcessType)
        {
            _session = pSession;
        }
        #endregion
        #region Methods
        #region SQLWrite
        /// <summary>
        /// Injecte la ligne principale du log du process dans PROCESS_L
        /// </summary>
        /// <param name="pCs"></param>
        public void SQLWrite(string pCs)
        {
            IDbTransaction dbTransaction = null;
            bool isOk = true;
            try
            {
                dbTransaction = DataHelper.BeginTran(pCs);

                SQLUP.GetId(out int id, dbTransaction, SQLUP.IdGetId.PROCESS_L);
                IdProcess = id;
                ProcessLogQuery processLogQry = new ProcessLogQuery(pCs);
                QueryParameters qry = processLogQry.GetQueryInsertPROCESS_L(pCs);
                SetParameters(qry.Parameters);

                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception) { isOk = false; throw; }
            finally
            {
                if (false == isOk)
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);
                }
            }
        }
        #endregion SQLWrite
        #region SQLUpdate
        /// <summary>
        /// Mise à jour de la table PROCESS_L avec les informations du log
        /// </summary>
        /// <param name="pCs"></param>
        public void SQLUpdate(string pCS)
        {
            ProcessLogQuery processLogQry = new ProcessLogQuery(pCS);
            // RD 20161212 [22679] Add parameter Info.status
            QueryParameters qry = processLogQry.GetQueryUpdatePROCESS_L(pCS, Info.status);
            qry.Parameters["IDPROCESS_L"].Value = IdProcess;
            qry.Parameters["IDTRK_L"].Value = (m_IdTRK_L > 0) ? m_IdTRK_L : Convert.DBNull;
            qry.Parameters["DTSTPROCESS"].Value = Info.dtstatus;
            qry.Parameters["IDSTPROCESS"].Value = Info.status.ToString();
            if (StrFunc.IsFilled(Info.Code) && IntFunc.IsFilled(Info.Number))
            {
                qry.Parameters["SYSCODE"].Value = Info.Code;
                qry.Parameters["SYSNUMBER"].Value = Info.Number;
                qry.Parameters["MESSAGE"].Value = "-";
            }
            else
            {
                qry.Parameters["SYSCODE"].Value = Convert.DBNull;
                qry.Parameters["SYSNUMBER"].Value = Convert.DBNull;
                qry.Parameters["MESSAGE"].Value = Info.message;
            }
            qry.Parameters["DATA1"].Value = Info.data1;
            qry.Parameters["DATA2"].Value = Info.data2;
            qry.Parameters["DATA3"].Value = Info.data3;
            qry.Parameters["DATA4"].Value = Info.data4;
            qry.Parameters["DATA5"].Value = Info.data5;
            DataHelper.ExecuteNonQuery(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
        }
        #endregion SQLUpdate
        #region SetParameters
        /// <summary>
        /// 
        /// </summary>
        private void SetParameters(DataParameters pParameters)
        {
            #region queryParameters
            pParameters["IDPROCESS_L"].Value = IdProcess;
            pParameters["PROCESS"].Value = ProcessType.ToString();
            pParameters["IDTRK_L"].Value = (m_IdTRK_L > 0) ? m_IdTRK_L : Convert.DBNull;
            pParameters["DTSTPROCESS"].Value = Info.dtstatus;
            pParameters["IDSTPROCESS"].Value = Info.status.ToString();
            pParameters["IDDATA"].Value = Info.idData;
            pParameters["IDDATAIDENT"].Value = DataHelper.GetDBData(Info.idDataIdent);
            if (StrFunc.IsFilled(Info.Code) && IntFunc.IsFilled(Info.Number))
            {
                pParameters["SYSCODE"].Value = Info.Code;
                pParameters["SYSNUMBER"].Value = Info.Number;
                pParameters["MESSAGE"].Value = Cst.Space; //string.Empty génère DBNull sous Oracle
            }
            else
            {
                pParameters["SYSCODE"].Value = Convert.DBNull;
                pParameters["SYSNUMBER"].Value = Convert.DBNull;
                pParameters["MESSAGE"].Value = Info.message;
            }
            pParameters["DATA1"].Value = Info.data1;
            pParameters["DATA2"].Value = Info.data2;
            pParameters["DATA3"].Value = Info.data3;
            pParameters["DATA4"].Value = Info.data4;
            pParameters["DATA5"].Value = Info.data5;
            // RD 20101019 / Gestion de la nouvelle colonne PROCESS_L.LODATATEXT
            if (StrFunc.IsFilled(Info.loDataTxt))
                pParameters["LODATATXT"].Value = Info.loDataTxt;
            else
                pParameters["LODATATXT"].Value = DBNull.Value;
            //
            pParameters["QUEUEMSG"].Value = StrFunc.IsFilled(Info.queueMessage) ? Info.queueMessage : Convert.DBNull;
            pParameters["IDA"].Value = (Session.IdA == 0 ? 1 : Session.IdA);
            pParameters["HOSTNAME"].Value = Session.AppInstance.HostName;
            pParameters["APPNAME"].Value = Session.AppInstance.AppNameInstance;
            pParameters["APPVERSION"].Value = Session.AppInstance.AppVersion;
            pParameters["EXTLLINK"].Value = ExtlLink;
            pParameters["ROWATTRIBUT"].Value = RowAttribut;
            #endregion
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion
    #region ProcessLogDetail
    /// <summary>
    /// Représente un log détail d'un process (équivalent à un item de PROCESSDET_L)
    /// </summary>
    public class ProcessLogDetail : ProcessLogBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLogHeader"></param>
        /// <param name="pLogInfo"></param>
        public ProcessLogDetail(ProcessLogHeader pLogHeader, ProcessLogInfo pLogInfo)
            : base(pLogHeader.ProcessType)
        {
            IdProcess = pLogHeader.IdProcess;
            Info = pLogInfo;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        internal void SQLWrite(string pCS)
        {
            ProcessLogQuery processLogQry = new ProcessLogQuery(pCS);
            QueryParameters qry = processLogQry.GetQueryInsertPROCESSDET_L(pCS);

            qry.Parameters["IDPROCESS_L"].Value = IdProcess;
            qry.Parameters["PROCESS"].Value = ProcessType.ToString();
            qry.Parameters["DTSTPROCESS"].Value = Info.dtstatus;
            qry.Parameters["IDSTPROCESS"].Value = Info.status.ToString();
            qry.Parameters["IDDATA"].Value = Info.idData;
            qry.Parameters["IDDATAIDENT"].Value = DataHelper.GetDBData(Info.idDataIdent);

            if (StrFunc.IsFilled(Info.Code) && IntFunc.IsFilled(Info.Number))
            {
                qry.Parameters["SYSCODE"].Value = Info.Code;
                qry.Parameters["SYSNUMBER"].Value = Info.Number;
            }
            else
            {
                qry.Parameters["SYSCODE"].Value = Convert.DBNull;
                qry.Parameters["SYSNUMBER"].Value = Convert.DBNull;
            }
            // MF 20120530 Ticket 17811
            qry.Parameters["MESSAGE"].Value = Info.message;

            qry.Parameters["DATA1"].Value = Info.data1;
            qry.Parameters["DATA2"].Value = Info.data2;
            qry.Parameters["DATA3"].Value = Info.data3;
            qry.Parameters["DATA4"].Value = Info.data4;
            qry.Parameters["DATA5"].Value = Info.data5;
            qry.Parameters["DATA6"].Value = Info.data6;
            qry.Parameters["DATA7"].Value = Info.data7;
            qry.Parameters["DATA8"].Value = Info.data8;
            qry.Parameters["DATA9"].Value = Info.data9;
            qry.Parameters["DATA10"].Value = Info.data10;

            qry.Parameters["LEVELORDER"].Value = Info.levelOrder;
            qry.Parameters["EXTLLINK"].Value = ExtlLink;
            qry.Parameters["ROWATTRIBUT"].Value = RowAttribut;
            DataHelper.ExecuteNonQuery(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
        }

    }
    #endregion ProcessLogDetail

    #region ProcessLogInfo
    /// <summary>
    /// Représente une information qui sera inséré dans un log
    /// </summary>
    public class ProcessLogInfo
    {
        #region Members
        //ProcessStateTools.StatusEnum  => string (Ouverture des status a des valeurs libres dans PROCESSDET_L uniquement - fonctionnalite IO)
        [System.Xml.Serialization.XmlAttribute()]
        public string status;
        [System.Xml.Serialization.XmlAttribute()]
        public DateTime dtstatus;
        /// <summary>
        /// Représente une identification pour Id
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public string idDataIdent;
        /// <summary>
        /// Représente l'id
        /// </summary>
        [System.Xml.Serialization.XmlAttribute()]
        public int idData;
        [System.Xml.Serialization.XmlElement()]
        public string message;
        [System.Xml.Serialization.XmlElement()]
        public string data1;
        [System.Xml.Serialization.XmlElement()]
        public string data2;
        [System.Xml.Serialization.XmlElement()]
        public string data3;
        [System.Xml.Serialization.XmlElement()]
        public string data4;
        [System.Xml.Serialization.XmlElement()]
        public string data5;
        [System.Xml.Serialization.XmlElement()]
        public string data6;
        [System.Xml.Serialization.XmlElement()]
        public string data7;
        [System.Xml.Serialization.XmlElement()]
        public string data8;
        [System.Xml.Serialization.XmlElement()]
        public string data9;
        [System.Xml.Serialization.XmlElement()]
        public string data10;
        [System.Xml.Serialization.XmlElement()]
        public string loDataTxt;
        [System.Xml.Serialization.XmlElement()]
        public string queueMessage;

        // PM 20200102 [XXXXX] New Log : Suppression
        ////PL 20110621 Newness
        //[System.Xml.Serialization.XmlElement()]
        //public string code;
        //[System.Xml.Serialization.XmlElement()]
        //public int number;
        [System.Xml.Serialization.XmlElement()]
        public int levelOrder;

        /// <summary>
        /// Code pour SYSTEMMSG
        /// </summary>
        /// PM 20200102 [XXXXX] New Log
        private SysMsgCode m_SysMsgCode;
        #endregion
        #region Accessors
        /// <summary>
        /// Code de message pour SYSTEMMSG sous la forme d'un string
        /// </summary>
        /// PM 20200102 [XXXXX] New Log
        [System.Xml.Serialization.XmlElement()]
        public string Code
        {
            get { return m_SysMsgCode != default(SysMsgCode) ? m_SysMsgCode.SysCode.ToString() : default; }
        }
        // EG 20200305 [25077] RDBMS : Correction Erreurs (step4) - Utilisé par IO
        [System.Xml.Serialization.XmlElement()]
        public string code
        {
            set
            {
                Nullable<SysCodeEnum> enumValue = ReflectionTools.ConvertStringToEnumOrNullable<SysCodeEnum>(value);
                if (enumValue.HasValue)
                    m_SysMsgCode = new SysMsgCode(enumValue.Value, 0);
            }
            get { return Code; }
        }

        /// <summary>
        /// Numéro de message pour SYSTEMMSG sous la forme d'un string
        /// </summary>
        /// PM 20200102 [XXXXX] New Log
        [System.Xml.Serialization.XmlElement()]
        public int Number
        {
            get { return (m_SysMsgCode != default(SysMsgCode) ? m_SysMsgCode.SysNumber : 0); }
        }

        // EG 20200305 [25077] RDBMS : Correction Erreurs (step4) - Utilisé par IO
        [System.Xml.Serialization.XmlElement()]
        public int number
        {
            set
            {
                if (m_SysMsgCode != default(SysMsgCode))
                    m_SysMsgCode.SysNumber = value;
            }
            get { return Number; }
        }

        /// <summary>
        /// Code pour SYSTEMMSG
        /// </summary>
        /// PM 20200102 [XXXXX] New Log
        [System.Xml.Serialization.XmlIgnore()]
        public SysMsgCode SysMsgCode
        {
            get { return m_SysMsgCode; }
        }
        #endregion Accessors
        #region Constructors
        public ProcessLogInfo()
        {

        }
        #endregion
        #region Methods
        /// <summary>
        ///  Alimente <see cref="message"/> et data (exemple <see cref="data1"/>,<see cref="data2"/> ). 
        ///  <para>Le 1er élément du tableau alimente <see cref="message"/></para>
        /// </summary>
        /// <param name="data"></param>
        public void SetMessageAndData(string[] data)
        {
            int i = ArrFunc.Count(data);
            if (i == 0)
                throw new ArgumentNullException($"{nameof(data)} is null");

            message = data[0];
            if (null == message)
                throw new InvalidProgramException("Message is null.");

            Match matchCode = Cst.Regex_CodeNumber.Match(message);
            if (matchCode.Success)
            {
                string code = matchCode.Groups[1].Value;
                if (code != "ORA")
                {
                    int number = Convert.ToInt32(matchCode.Groups[2].Value);
                    // PM 20200102 [XXXXX] New Log
                    SetSysMsgCode(code, number);

                    // MF 20120530 Ticket 17811
                    message = matchCode.Groups[3].Value;
                }
            }

            data1 = 1 < i ? data[1] : string.Empty;
            data2 = 2 < i ? data[2] : string.Empty;
            data3 = 3 < i ? data[3] : string.Empty;
            data4 = 4 < i ? data[4] : string.Empty;
            data5 = 5 < i ? data[5] : string.Empty;
            data6 = 6 < i ? data[6] : string.Empty;
            data7 = 7 < i ? data[7] : string.Empty;
            data8 = 8 < i ? data[8] : string.Empty;
            data9 = 9 < i ? data[9] : string.Empty;
            data10 = 10 < i ? data[10] : string.Empty;
        }

        /// <summary>
        /// Affectation de m_SysMsgCode 
        /// </summary>
        /// <param name="pCode"></param>
        /// <param name="pNumber"></param>
        /// PM 20200102 [XXXXX] New Log : pour compatibilité
        public void SetSysMsgCode(string pCode, int pNumber)
        {
            SysCodeEnum sysCode = ReflectionTools.ConvertStringToEnumOrDefault<SysCodeEnum>(pCode, SysCodeEnum.LOG);
            m_SysMsgCode = new SysMsgCode(sysCode, pNumber);
        }
        /// <summary>
        /// Affectation de m_SysMsgCode 
        /// </summary>
        /// <param name="pSysMsgCode"></param>
        /// PM 20200102 [XXXXX] New Log
        public void SetSysMsgCode(SysMsgCode pSysMsgCode)
        {
            m_SysMsgCode = pSysMsgCode;
        }
        #endregion Methods
    }
    #endregion ProcessLogInfo

    #region SystemMSGInfo
    /// <summary>
    ///
    /// </summary>
    // EG 20190318 New Basic Constructor for Serialization
    public class SystemMSGInfo
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnoreAttribute]
        public ProcessState processState;

        /// <summary>
        /// Liste des args utilisé 
        /// </summary>
        public string[] datas;

        /// <summary>
        /// Elements de l'identifiant du message
        /// </summary>
        // PM 20200102 [XXXXX] New Log
        private readonly SysMsgCode m_SysMsgCode;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Identifiant interne du message (ex SYS-1004)
        /// </summary>
        // PM 20200102 [XXXXX] New Log : pour remplacer identifier
        public string Identifier
        { get { return m_SysMsgCode != default(SysMsgCode) ? m_SysMsgCode.MessageCode : default; } }

        /// <summary>
        /// Classe SysMsgCode de codes pour SYSTEMMSG
        /// </summary>
        /// PM 20200102 [XXXXX] New Log
        public SysMsgCode SysMsgCode
        { get { return m_SysMsgCode; } }

        /// <summary>
        /// Obtient les paramètres du message sous forme de tableau de LogParam
        /// </summary>
        public LogParam[] LogParamDatas
        { get { return datas != default ? datas.Select(d => new LogParam(d)).ToArray() : default; } }
        #endregion Accessors

        #region Constructors
        public SystemMSGInfo()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSysCode"></param>
        /// <param name="pSysNumber"></param>
        /// <param name="pProcessState"></param>
        /// <param name="pDatas"></param>
        public SystemMSGInfo(SysCodeEnum pSysCode, int pSysNumber, ProcessState pProcessState, params string[] pDatas)
        {
            m_SysMsgCode = new SysMsgCode(pSysCode, pSysNumber);
            processState = pProcessState;
            datas = pDatas;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSysMsgCode"></param>
        /// <param name="pProcessState"></param>
        /// <param name="pDatas"></param>
        public SystemMSGInfo(SysMsgCode pSysMsgCode, ProcessState pProcessState, params string[] pDatas)
        {
            m_SysMsgCode = pSysMsgCode;
            processState = pProcessState;
            datas = pDatas;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Fournit un LoggerData correspondant au SystemMSGInfo
        /// </summary>
        /// <param name="pRankOrder"></param>
        /// <returns></returns>
        /// PM 20200102 [XXXXX] New Log : ajout
        public LoggerData ToLoggerData(int pRankOrder)
        {
            LoggerData logData = new LoggerData(LoggerTools.StatusToLogLevelEnum(processState.Status), SysMsgCode, pRankOrder, LogParamDatas);
            return logData;
        }
        #endregion Methods
    }
    #endregion SystemMSGInfo

    #region LogTools
    /// <summary>
    /// 
    /// </summary>
    public class LogTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        public static string AmountAndCurrency(decimal pAmount, string pCurrency)
        {
            return StrFunc.FmtDecimalToInvariantCulture(pAmount) + " " + pCurrency;
        }

        /// <summary>
        /// Retourne la query 'select' de selection les logs rattachés aux données visibles 
        /// de l'utilisateur connecté (table TRACKER_L ou PROCESS_L)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUser">Représente l'utilisateur connecté</param>
        /// <param name="pSessionId">Représente l'identifiant de la session côté SESSIONRESTRICT</param>
        /// <param name="pTypeLog">Représente le type de log ( A reseigner avec PROCESS_L ou TRACKER_L uniquement)</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException si la ressource incorporée qui contient la commande est non chargée"></exception>
        /// FI 20111125 Ajout du paramètre entité de rattachement de l'acteur 
        public static QueryParameters GetQueryForLoadLogBySession(string pCS, User pUser, string pSessionId, string pTypeLog)
        {
            bool isTblLogValid = ((pTypeLog == Cst.OTCml_TBL.PROCESS_L.ToString()) ||
                                  (pTypeLog == Cst.OTCml_TBL.TRACKER_L.ToString()));

            if (false == isTblLogValid)
                throw new ArgumentException("Argument pTypeLog is not valid");

            string streamName = Assembly.GetExecutingAssembly().GetName().Name + ".Log." + "SelectLog.xml";
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamName);
            if (null == stream)
                throw new NullReferenceException("SelectLog.xml Ressource not found");

            StreamReader reader = new StreamReader(stream);
            string xml = reader.ReadToEnd();
            reader.Close();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            bool isSoftwareOTCml = Software.IsSoftwareOTCmlOrFnOml();
            bool isVision = Software.IsSoftwareVision();

            string xPath;
            if (isSoftwareOTCml)
                xPath = StrFunc.AppendFormat(@"SelectLog/Software[@name='{0}']/Command", Software.SOFTWARE_Spheres);
            else if (isVision)
                xPath = StrFunc.AppendFormat(@"SelectLog/Software[@name='{0}']/Command", Software.SOFTWARE_Vision);
            else
                throw new NotImplementedException("Current SoftWare is not implemented");

            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            if (false == (list.Count > 0))
                throw new Exception(@"Tag 'Command' not Found");

            string select = list.Item(0).InnerText;

            //Replace de {0} par le nom de table 
            select = String.Format(select, pTypeLog);

            SessionRestrictHelper srh = new SessionRestrictHelper(pUser, pSessionId, true);
            select = srh.ReplaceKeyword(select);

            DataParameters parameters = new DataParameters();
            srh.SetParameter(pCS, select, parameters);

            QueryParameters ret = new QueryParameters(pCS, select, parameters);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static LogConsultMode GetLogConsultMode()
        {
            string trackerConsultMode;
            try
            {
                trackerConsultMode = SystemSettings.GetAppSettings("Spheres_LogConsultMode", LogConsultMode.Normal.ToString());
            }
            catch { trackerConsultMode = LogConsultMode.Normal.ToString(); };
            LogConsultMode ret = (LogConsultMode)Enum.Parse(typeof(LogConsultMode), trackerConsultMode);

            return ret;

        }

        /// <summary>
        /// Retourne la ressource dans la culture utilisée en cours, correspondant au message {pMsgSource}.
        /// <para>Le message peut être:</para>
        /// <para>- Un couple "CODE-NUMBER" (Exemple: "RES-00502"): dans ce cas la ressource est à rechercher dans SYSTEMMSG</para>
        /// <para>- Une liste de ressources, séparées par ";" (Exemple:"fr:Montant;en:Amount;it:Ammontare"): ça revient à parser le message pour extraire la ressource correspondant à la culture</para>
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentCultureString(string pCs, string pMsgIn, List<string> pData)
        {
            string msgOut = pMsgIn;

            try
            {
                Match matchCode = Cst.Regex_CodeNumber.Match(pMsgIn);

                bool isMatchCodeSuccess = matchCode.Success;
                //PL 20121113 Add test on "ORA", (à compléter si nécessaire)
                if (isMatchCodeSuccess && (matchCode.Groups[1].Value == "ORA"))
                {
                    isMatchCodeSuccess = false;
                }
                if (isMatchCodeSuccess)
                {
                    #region Msg dans SYSTEMMSG
                    OTCmlHelper.GetRessource(pCs, Cst.OTCml_TBL.SYSTEMMSG.ToString(), msgOut, ref msgOut);
                    #endregion
                }
                else
                {
                    #region Msg Contenant les différentes cultures
                    // Msg Contenant les différentes cultures, donc à parser pour extraire la culture et la ressource qui va avec
                    // Exemple: fr:Montant;en:AMount
                    string[] aStringWithCulture = pMsgIn.Split(';');
                    //
                    if (aStringWithCulture.Length > 1)
                    {
                        string defaultString = string.Empty;
                        string englishString = string.Empty;
                        string currentCultureString = string.Empty;
                        //
                        for (int i = 0; i < aStringWithCulture.Length; i++)
                        {
                            string[] aCultureString = aStringWithCulture[i].Split(':');
                            //
                            if (aCultureString.Length > 1)
                            {
                                #region Recherche de la culture
                                if (aCultureString[0] == CultureInfo.CurrentCulture.Name)
                                {
                                    //Current Culture (ex.: "fr-FR:Montant;")
                                    currentCultureString = aCultureString[1];
                                    break;
                                }
                                else if (aCultureString[0] == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                                {
                                    //Current Language (ex.: "fr:Montant;")
                                    currentCultureString = aCultureString[1];
                                }
                                else if (aCultureString[0] == "en")
                                {
                                    //English Language (ex.: "en:Amount;")
                                    englishString = aCultureString[1];
                                }
                                else if (aCultureString[0].StartsWith("en-"))
                                {
                                    //English Language (ex.: "en-GB:Amount;")
                                    if (StrFunc.IsEmpty(englishString))
                                        englishString = aCultureString[1];
                                }
                                #endregion
                            }
                            else if (aCultureString.Length == 1)
                            {
                                //Default Culture (ex.: "Montant;")
                                if (StrFunc.IsEmpty(defaultString))
                                    defaultString = aCultureString[0];
                            }
                        }
                        //
                        if (StrFunc.IsFilled(currentCultureString))
                            msgOut = currentCultureString;
                        else if (StrFunc.IsFilled(defaultString))
                            msgOut = defaultString;
                        else if (StrFunc.IsFilled(englishString))
                            msgOut = englishString;
                    }
                    #endregion
                }

                if (StrFunc.IsFilled(msgOut) && ArrFunc.IsFilled(pData))
                {
                    for (int i = 1; i <= ArrFunc.Count(pData); i++)
                        msgOut = msgOut.Replace("{" + i.ToString() + "}", pData[i - 1]);
                }
            }
            catch
            {
                //Pas besoin de gérer les erreurs d'erreur
                msgOut = pMsgIn + Cst.CrLf + "[Warning: Message unparsed]";
            }

            return msgOut;
        }

        public static string IdentifierAndId(string pIdentifier)
        {
            return IdentifierAndId(pIdentifier, 0);
        }
        public static string IdentifierDnAndId(string pIdentifier, string pDisplayName, int pId)
        {
            return IdentifierAndId(pIdentifier + " / " + pDisplayName, pId);
        }
        public static string IdentifierAndId(KeyValuePair<int, string> pIdentifiers) //PL 20200109 New
        {
            return IdentifierAndId(pIdentifiers.Value, pIdentifiers.Key);
        }
        public static string IdentifierAndId(string pIdentifier, int pId)
        {
            return (StrFunc.IsEmpty(pIdentifier) ? "N/A" : pIdentifier) + " (id: " + (pId == 0 ? "N/A" : pId.ToString()) + ")";
        }
        public static string IdentifierAndId(string pIdentifier, string pId)
        {
            return (StrFunc.IsEmpty(pIdentifier) ? "N/A" : pIdentifier) + " (id: " + (StrFunc.IsEmpty(pId) ? "N/A" : pId) + ")";
        }

        /// <summary>
        /// Retourne true s'il existe une ligne dans la table PROCESS_L pour le triplet {pIdTRK},{pProcess},{pAppName}  
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTRK">id non significatif du tracker</param></param>
        /// <param name="pProcess">Nom du process</param>
        /// <param name="pAppName">Instance</param>
        /// <returns></returns>
        /// FI 20190528 [23912] Add pProcess, pAppName
        public static bool ExistLogForTracker(string pCS, int pIdTRK, Cst.ProcessTypeEnum pProcess, string pAppName)
        {
            string query = @"select 1 from dbo.PROCESS_L 
where IDTRK_L=@ID and PROCESS=@PROCESS and APPNAME=@APPNAME";
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ID), pIdTRK);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PROCESS), pProcess.ToString());
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPNAME), pAppName);

            QueryParameters qryParameters = new QueryParameters(pCS, query, parameters);
            object existRequest = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return (existRequest != null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdProcess"></param>
        /// <param name="pIdA"></param>
        /// <param name="pData"></param>
        /// <param name="pName"></param>
        /// <param name="pDocType"></param>
        public static void AddAttachedDoc(string pCS, int pIdProcess, int pIdA, byte[] pData, string pName, string pDocType)
        {
            LOFile loFile = new LOFile(pName, pDocType, pData);

            string[] keyColumns = new string[] { "TABLENAME", "ID" };
            string[] keyData = new string[] { Cst.OTCml_TBL.PROCESS_L.ToString(), pIdProcess.ToString() };
            string[] keyDataType = new string[] { TypeData.TypeDataEnum.@string.ToString(), TypeData.TypeDataEnum.integer.ToString() };
            LOFileColumn loCol = new LOFileColumn(pCS, Cst.OTCml_TBL.ATTACHEDDOC.ToString(), keyColumns, keyData, keyDataType);
            loCol.SaveFile(loFile, pIdA, out _);
        }


    }
    #endregion LogTools
}




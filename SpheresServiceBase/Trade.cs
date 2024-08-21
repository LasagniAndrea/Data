#region Using Directives
using System;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.OTCmlStatus;
using EFS.Tuning;
using EFS.GUI.Interface;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Enum.Tools;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process 
{
    #region DataSetEventTrade
    // **************************************************************
    // DataSetEventTrade is moved to EfsML_Business_Shared_Events.cs
    // **************************************************************
    #endregion DataSetEventTrade

    #region EventProcess
    /// <summary>
    /// Classe chargée d'alimenter la table EVENTPROCESS
    /// </summary>
    /// IDTRK_L remplace IDREQUEST_L
    public class EventProcess
    {
        #region Members
        private string m_Cs;
        private string m_SQLQueryEventProcess;

        #region Parameters variables
        private DataParameter paramIdE;
        private DataParameter paramIdTRK;
        private DataParameter paramProcess;
        private DataParameter paramIdStProcess;
        private DataParameter paramDtStProcess;
        private DataParameter paramEventClass;
        private DataParameter paramExtLink;
        private DataParameter paramIdData;
        #endregion Parameters variables

        #endregion Members
        #region Constructors
        public EventProcess(string pCs)
        {
            m_Cs = pCs;
            m_SQLQueryEventProcess = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EVENTPROCESS.ToString();
            m_SQLQueryEventProcess += "(IDE, PROCESS, IDTRK_L, IDSTPROCESS, DTSTPROCESS, EVENTCLASS, IDDATA, EXTLLINK)" + Cst.CrLf;
            m_SQLQueryEventProcess += "values (@IDE, @PROCESS, @IDTRK_L, @IDSTPROCESS, @DTSTPROCESS, @EVENTCLASS, @IDDATA, @EXTLLINK)";
            paramIdTRK = new DataParameter(m_Cs, "IDTRK_L", DbType.Int32);
            paramIdE = new DataParameter(m_Cs, "IDE", DbType.Int32);
            paramProcess = new DataParameter(m_Cs, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            paramIdStProcess = new DataParameter(m_Cs, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN);
            paramDtStProcess = new DataParameter(m_Cs, "DTSTPROCESS", DbType.DateTime);
            paramEventClass = new DataParameter(m_Cs, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN);
            paramExtLink = new DataParameter(m_Cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            paramIdData = new DataParameter(m_Cs, "IDDATA", DbType.Int32);
        }
        #endregion Constructors
        #region Methods
        #region Write
        // Without pDbTransaction
        public int Write(int pIdE, Cst.ProcessTypeEnum pProcessType, ProcessStateTools.StatusEnum pStatusProcess, int pIdTRK)
        {
            return Write(null, pIdE, pProcessType, pStatusProcess, OTCmlHelper.GetDateSys(m_Cs), string.Empty, string.Empty, pIdTRK, 0);
        }
        public int Write(int pIdE, Cst.ProcessTypeEnum pProcessType, ProcessStateTools.StatusEnum pStatusProcess, int pIdTRK, int pIdData)
        {
            return Write(null, pIdE, pProcessType, pStatusProcess, OTCmlHelper.GetDateSys(m_Cs), string.Empty, string.Empty, pIdTRK, pIdData);
        }
        // With pDbTransaction
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType, ProcessStateTools.StatusEnum pStatusProcess, int pIdTRK)
        {
            return Write(pDbTransaction, pIdE, pProcessType, pStatusProcess, OTCmlHelper.GetDateSys(m_Cs), string.Empty, string.Empty, pIdTRK, 0);
        }
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType, ProcessStateTools.StatusEnum pStatusProcess, int pIdTRK, int pIdData)
        {
            return Write(pDbTransaction, pIdE, pProcessType, pStatusProcess, OTCmlHelper.GetDateSys(m_Cs), string.Empty, string.Empty, pIdTRK, pIdData);
        }
        //
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType,
                        ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSysBusiness, int pIdTRK)
        {
            return Write(pDbTransaction, pIdE, pProcessType, pStatusProcess, pDtSysBusiness, string.Empty, string.Empty, pIdTRK, 0);
        }
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType,
                        ProcessStateTools.StatusEnum pStatusProcess, string pStrDtSysBusiness, int pIdTRK)
        {
            return Write(pDbTransaction, pIdE, pProcessType, pStatusProcess, DateTime.MinValue, pStrDtSysBusiness, string.Empty, pIdTRK, 0);
        }
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType,
                                ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSysBusiness, string pStrDtSysBusiness, string pEventClass, int pIdTRK, int pIdData)
        {

            int rowAffected = 0;

            #region Parameters
            paramIdE.Value = pIdE;
            paramProcess.Value = pProcessType.ToString();
            paramIdStProcess.Value = pStatusProcess.ToString();
            paramEventClass.Value = StrFunc.IsFilled(pEventClass) ? pEventClass : Convert.DBNull;
            paramIdTRK.Value = (pIdTRK > 0) ? pIdTRK : Convert.DBNull;
            paramIdData.Value = (pIdData > 0) ? pIdData : Convert.DBNull;
            paramExtLink.Value = Convert.DBNull;
            #endregion Parameters

            DataParameters parameters = null;
            if (StrFunc.IsEmpty(pStrDtSysBusiness))
            {
                paramDtStProcess.Value = pDtSysBusiness;
                parameters = new DataParameters(new DataParameter[] { paramIdE, paramProcess, paramIdStProcess, paramDtStProcess, paramEventClass, paramExtLink, paramIdTRK, paramIdData });
                if (null == pDbTransaction)
                    rowAffected = DataHelper.ExecuteNonQuery(m_Cs, CommandType.Text, m_SQLQueryEventProcess, parameters.GetArrayDbParameter());
                else
                    rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, m_SQLQueryEventProcess, parameters.GetArrayDbParameter());
            }
            else
            {
                parameters = new DataParameters(new DataParameter[] { paramIdE, paramProcess, paramIdStProcess, paramEventClass, paramExtLink, paramIdTRK, paramIdData });
                string sqlQuery = m_SQLQueryEventProcess.Replace("@" + paramDtStProcess.ParameterName.Substring(1), pStrDtSysBusiness);
                if (null == pDbTransaction)
                    rowAffected = DataHelper.ExecuteNonQuery(m_Cs, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
                else
                    rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            }
            return rowAffected;
        }

        public static void Write(string pCs, IDbTransaction pDbTransaction, List<int> pEvents,
            Cst.ProcessTypeEnum pProcessType, ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSysBusiness,
            string pEventClass, int pIdTRK, int pIdData)
        {
            if (pEvents.Count > 0)
            {
                string sql_Select_EventProcess = SQLCst.SELECT + "IDEP, IDE, PROCESS, IDTRK_L, IDSTPROCESS, DTSTPROCESS, EVENTCLASS, IDDATA" + Cst.CrLf;
                sql_Select_EventProcess += SQLCst.FROM_DBO + "EVENTPROCESS";
                string sql_Where_EventProcess = SQLCst.WHERE + "(" + DataHelper.SQLColumnIn(pCs, "IDE", pEvents, TypeData.TypeDataEnum.integer, false, true) + ")";
                sql_Where_EventProcess += SQLCst.AND + "(PROCESS = @PROCESS)" + Cst.CrLf;
                sql_Where_EventProcess += SQLCst.ORDERBY + "DTSTPROCESS";

                DataParameter dbParamProcess = new DataParameter(pCs, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
                dbParamProcess.Value = pProcessType.ToString();
                DataSet dsEventProcess = null;
                if (null == pDbTransaction)
                    dsEventProcess = DataHelper.ExecuteDataset(pCs, CommandType.Text,
                        sql_Select_EventProcess + sql_Where_EventProcess,
                        dbParamProcess.DbDataParameter);
                else
                    dsEventProcess = DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text,
                        sql_Select_EventProcess + sql_Where_EventProcess,
                        dbParamProcess.DbDataParameter);

                DataTable dtEventProcess = dsEventProcess.Tables[0];

                foreach (int ide in pEvents)
                {
                    DataRow[] drEventProcess = dtEventProcess.Select("IDE = " + ide);
                    int count = drEventProcess.Length;

                    if (count > 0)
                    {
                        // Mettre à jour la dérnière ligne
                        drEventProcess[count - 1].BeginEdit();
                        drEventProcess[count - 1]["IDTRK_L"] = (pIdTRK > 0) ? pIdTRK : Convert.DBNull;
                        drEventProcess[count - 1]["IDSTPROCESS"] = pStatusProcess.ToString();
                        drEventProcess[count - 1]["DTSTPROCESS"] = pDtSysBusiness;
                        drEventProcess[count - 1]["EVENTCLASS"] = StrFunc.IsFilled(pEventClass) ? pEventClass : Convert.DBNull;
                        drEventProcess[count - 1]["IDDATA"] = (pIdData > 0) ? pIdData : Convert.DBNull;
                        drEventProcess[count - 1].EndEdit();
                    }
                    else
                    {
                        DataRow newRow = dtEventProcess.NewRow();
                        newRow.BeginEdit();
                        newRow["IDE"] = ide;
                        newRow["PROCESS"] = pProcessType.ToString();
                        newRow["IDTRK_L"] = (pIdTRK > 0) ? pIdTRK : Convert.DBNull;
                        newRow["IDSTPROCESS"] = pStatusProcess.ToString();
                        newRow["DTSTPROCESS"] = pDtSysBusiness;
                        newRow["EVENTCLASS"] = StrFunc.IsFilled(pEventClass) ? pEventClass : Convert.DBNull;
                        newRow["IDDATA"] = (pIdData > 0) ? pIdData : Convert.DBNull;
                        newRow.EndEdit();
                        dtEventProcess.Rows.Add(newRow);
                    }
                }

                int rowEventProcess;
                if (null == pDbTransaction)
                    rowEventProcess = DataHelper.ExecuteDataAdapter(pCs, sql_Select_EventProcess, dtEventProcess);
                else
                    rowEventProcess = DataHelper.ExecuteDataAdapter(pDbTransaction, sql_Select_EventProcess, dtEventProcess);
            }
        }
        #endregion Write
        #endregion Methods
    }
    #endregion EventProcess

    #region DataRowEvent
    public class DataRowEvent
    {
        #region Members
        private DataRow _rowEvent;
        #endregion Members
        #region Accessors
        #region rowEvent
        public DataRow rowEvent
        {
            get { return _rowEvent; }
        }
        #endregion
        #region id
        public int id
        {
            get
            {
                return Convert.ToInt32(_rowEvent["IDE"]);
            }
        }
        #endregion
        #region idParentEvent
        public int idParentEvent
        {
            get
            {
                return Convert.ToInt32(_rowEvent["IDE_EVENT"]);
            }
        }
        #endregion
        #region idParent
        public int idParent
        {
            get
            {
                return Convert.ToInt32(_rowEvent["IDT"]);
            }
        }
        #endregion

        #region instrumentNo
        public int instrumentNo
        {
            get
            {
                return Convert.ToInt32(_rowEvent["INSTRUMENTNO"]);
            }
        }
        #endregion
        #region streamNo
        public int streamNo
        {
            get
            {
                return Convert.ToInt32(_rowEvent["STREAMNO"]);
            }
        }
        #endregion

        #region idA_Pay
        public Nullable<Int32> idA_Pay
        {
            get
            {
                Nullable<Int32> ret = 0;
                if (rowEvent["IDA_PAY"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDA_PAY"]);
                return ret;
            }
        }
        #endregion
        #region idB_Pay
        public Nullable<Int32> idB_Pay
        {
            get
            {
                Nullable<Int32> ret = 0;
                if (rowEvent["IDB_PAY"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDB_PAY"]);
                return ret;
            }
        }
        #endregion
        #region idA_Rec
        public Nullable<Int32> idA_Rec
        {
            get
            {
                Nullable<Int32> ret = 0;
                if (rowEvent["IDA_REC"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDA_REC"]);
                return ret;
            }
        }
        #endregion
        #region idB_Rec
        public Nullable<Int32> idB_Rec
        {
            get
            {
                Nullable<Int32> ret = null;
                if (rowEvent["IDB_REC"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDB_REC"]);
                return ret;
            }
        }
        #endregion

        #region eventCode
        public string eventCode
        {
            get { return _rowEvent["EVENTCODE"].ToString(); }
        }
        #endregion
        #region eventType
        public string eventType
        {
            get { return _rowEvent["EVENTTYPE"].ToString(); }
        }
        #endregion


        #region dtStartUnadj
        public DateTime dtStartUnadj
        {
            get { return Convert.ToDateTime(_rowEvent["DTSTARTUNADJ"]); }
        }
        #endregion
        #region dtStartAdj
        public DateTime dtStartAdj
        {
            get { return Convert.ToDateTime(_rowEvent["DTSTARTADJ"]); }
        }
        #endregion

        #region dtEndUnadj
        public DateTime dtEndUnadj
        {
            get { return Convert.ToDateTime(_rowEvent["DTENDUNADJ"]); }
        }
        #endregion
        #region dtEndAdj
        public DateTime dtEndAdj
        {
            get { return Convert.ToDateTime(_rowEvent["DTENDADJ"]); }
        }
        #endregion

        #region valorisation
        public Nullable<Decimal> valorisation
        {
            get
            {
                Nullable<Decimal> ret = null;
                if (Convert.DBNull != _rowEvent["VALORISATION"])
                    ret = Convert.ToDecimal(_rowEvent["VALORISATION"]);
                return ret;
            }
        }
        #endregion
        #region valorisationSys
        public Nullable<Decimal> valorisationSys
        {
            get
            {
                Nullable<Decimal> ret = null;
                if (Convert.DBNull != _rowEvent["VALORISATIONSYS"])
                    ret = Convert.ToDecimal(_rowEvent["VALORISATIONSYS"]);
                return ret;
            }
        }
        #endregion
        #region unit
        public string unit
        {
            get
            {
                string ret = null;
                if (Convert.DBNull != _rowEvent["UNIT"])
                    ret = Convert.ToString(_rowEvent["UNIT"]);
                return ret;
            }
        }
        #endregion
        #region unitSys
        public string unitSys
        {
            get
            {
                string ret = null;
                if (Convert.DBNull != _rowEvent["UNITSYS"])
                    ret = Convert.ToString(_rowEvent["UNITSYS"]);
                return ret;
            }
        }
        #endregion
        #endregion Accessors
        #region Constructor
        public DataRowEvent(DataRow pRowEvent)
        {
            _rowEvent = pRowEvent;
        }
        #endregion Constructor
    }
    #endregion DataRowEvent

    #region EventQuery
    // EG 20120328 Add SetFeeLogInformation, InsertPaymentEvents et InitPaymentForEvent (Ticket 17706 Recalcul des frais )
    // EG 20141006 Fusion EVENDET_ETD/EVENTDET -> EVENTDET
    public class EventQuery
    {
        #region Members
        private ProcessBase m_ProcessBase;
        private EventProcess m_EventProcess;
        private DateTime m_DtSys;

        private string m_SqlInsertEvent;
        private string m_SqlInsertEventClass;
        private string m_SqlInsertEventAsset;
        private string m_SqlInsertEventDet;
        private string m_SqlInsertEventFee;
        private string m_SqlInsertEventPosActionDet;
        // EG 20160404 Migration vs2013
        //private string m_SqlInsertEventPricing;
        //private string m_SqlInsertEventPricing2;

        private DataParameters m_ParamEvent;
        private DataParameters m_ParamEventAsset;
        private DataParameters m_ParamEventClass;
        private DataParameters m_ParamEventDet;
        private DataParameters m_ParamEventFee;
        private DataParameters m_ParamEventPricing;
        private DataParameters m_ParamEventPricing2;
        private DataParameters m_ParamEventPosActionDet;

        #endregion Members
        #region Accessors
        private string CS
        {
            get { return m_ProcessBase.cs; }
        }
        public DateTime DtSys
        {
            get { return m_DtSys; }
        }
        #endregion Accessors
        #region Constructors
        public EventQuery(ProcessBase pProcessBase, EventProcess pEventProcess)
        {
            m_ProcessBase = pProcessBase;
            m_EventProcess = pEventProcess;
            m_DtSys = OTCmlHelper.GetDateSys(CS).Date;
            InitQueries();
        }
        #endregion Constructors
        #region Methods

        #region InitParameters
        /// <summary>
        /// Initialisation de la collections des paramètres pour insertion/mise à jour des tables
        /// </summary>
        /// <param name="pTable">Enumérateur déterminant la table mise à jour (EVENT|EVENTCLASS|EVENTASSET|EVENTDET|EVENTFEE|EVENTPOSACTIONDET)</param>
        private void InitParameters(Cst.OTCml_TBL pTable)
        {
            switch (pTable)
            {
                case Cst.OTCml_TBL.EVENT:
                    #region EVENT
                    if (null == m_ParamEvent)
                    {
                        m_ParamEvent = new DataParameters(new DataParameter[] { });
                        m_ParamEvent.Add(new DataParameter(CS, "IDT", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "INSTRUMENTNO", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "STREAMNO", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDE_EVENT", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDE_SOURCE", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDA_PAY", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDB_PAY", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDA_REC", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "IDB_REC", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "DTSTARTADJ", DbType.DateTime));
                        m_ParamEvent.Add(new DataParameter(CS, "DTSTARTUNADJ", DbType.DateTime));
                        m_ParamEvent.Add(new DataParameter(CS, "DTENDADJ", DbType.DateTime));
                        m_ParamEvent.Add(new DataParameter(CS, "DTENDUNADJ", DbType.DateTime));
                        m_ParamEvent.Add(new DataParameter(CS, "VALORISATION", DbType.Decimal));
                        m_ParamEvent.Add(new DataParameter(CS, "UNIT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "UNITTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "VALORISATIONSYS", DbType.Decimal));
                        m_ParamEvent.Add(new DataParameter(CS, "UNITSYS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "UNITTYPESYS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "TAXLEVYOPT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "IDASTACTIVATION", DbType.Int32));
                        m_ParamEvent.Add(new DataParameter(CS, "DTSTACTIVATION", DbType.DateTime));
                        m_ParamEvent.Add(new DataParameter(CS, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "IDSTCALCUL", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                        m_ParamEvent.Add(new DataParameter(CS, "IDSTTRIGGER", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                    }
                    m_ParamEvent.SetAllDBNull();
                    #endregion EVENT
                    break;
                case Cst.OTCml_TBL.EVENTASSET:
                    #region EVENTASSET
                    if (null == m_ParamEventAsset)
                    {
                        m_ParamEventAsset = new DataParameters(new DataParameter[] { });
                        m_ParamEventAsset.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEventAsset.Add(new DataParameter(CS, "IDASSET", DbType.Int32));
                        m_ParamEventAsset.Add(new DataParameter(CS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "ASSETSYMBOL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "ASSETTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "CATEGORY", DbType.AnsiString, SQLCst.UT_CFICODE_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "CLEARANCESYSTEM", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "CONTRACTMULTIPLIER", DbType.Decimal));
                        m_ParamEventAsset.Add(new DataParameter(CS, "CONTRACTSYMBOL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "DELIVERYDATE", DbType.DateTime));
                        m_ParamEventAsset.Add(new DataParameter(CS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "IDM", DbType.Int32));
                        m_ParamEventAsset.Add(new DataParameter(CS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "ISINCODE", DbType.AnsiString, SQLCst.UT_ISINCODE_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "MATURITYDATE", DbType.DateTime));
                        m_ParamEventAsset.Add(new DataParameter(CS, "MATURITYDATESYS", DbType.DateTime));
                        m_ParamEventAsset.Add(new DataParameter(CS, "NOMINALVALUE", DbType.Decimal));
                        m_ParamEventAsset.Add(new DataParameter(CS, "PRIMARYRATESRC", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "PRIMARYRATESRCHEAD", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "PRIMARYRATESRCPAGE", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "PUTORCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                        // EG/CC/PL 20141128 Decimal
                        m_ParamEventAsset.Add(new DataParameter(CS, "STRIKEPRICE", DbType.Decimal));
                        m_ParamEventAsset.Add(new DataParameter(CS, "TIME", DbType.DateTime));
                        m_ParamEventAsset.Add(new DataParameter(CS, "WEIGHT", DbType.Decimal));
                        m_ParamEventAsset.Add(new DataParameter(CS, "UNITWEIGHT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventAsset.Add(new DataParameter(CS, "UNITTYPEWEIGHT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    }
                    m_ParamEventAsset.SetAllDBNull();
                    #endregion EVENTASSET
                    break;
                case Cst.OTCml_TBL.EVENTCLASS:
                    #region EVENTCLASS
                    if (null == m_ParamEventClass)
                    {
                        m_ParamEventClass = new DataParameters(new DataParameter[] { });
                        m_ParamEventClass.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEventClass.Add(new DataParameter(CS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
                        m_ParamEventClass.Add(new DataParameter(CS, "DTEVENT", DbType.DateTime));
                        m_ParamEventClass.Add(new DataParameter(CS, "DTEVENTFORCED", DbType.DateTime));
                        m_ParamEventClass.Add(new DataParameter(CS, "IDNETCONVENTION", DbType.Int32));
                        m_ParamEventClass.Add(new DataParameter(CS, "IDNETDESIGNATION", DbType.Int32));
                        m_ParamEventClass.Add(new DataParameter(CS, "ISPAYMENT", DbType.Boolean));
                        m_ParamEventClass.Add(new DataParameter(CS, "NETMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventClass.Add(new DataParameter(CS, "EXTLLINK", DbType.DateTime));
                    }
                    m_ParamEventClass.SetAllDBNull();
                    #endregion EVENTCLASS
                    break;
                case Cst.OTCml_TBL.EVENTDET:
                    #region EVENTDET
                    if (null == m_ParamEventDet)
                    {
                        m_ParamEventDet = new DataParameters(new DataParameter[] { });
                        m_ParamEventDet.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEventDet.Add(new DataParameter(CS, "BASIS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "CLOSINGPRICE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "CLOSINGPRICE100", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "CONTRACTMULTIPLIER", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "CONVERSIONRATE", DbType.Decimal));
                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                        m_ParamEventDet.Add(new DataParameter(CS, "DAILYQUANTITY", DbType.Int64));
                        m_ParamEventDet.Add(new DataParameter(CS, "DCF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "DCFDEN", DbType.Int32));
                        m_ParamEventDet.Add(new DataParameter(CS, "DCFNUM", DbType.Int32));
                        m_ParamEventDet.Add(new DataParameter(CS, "DTACTION", DbType.DateTime));
                        m_ParamEventDet.Add(new DataParameter(CS, "DTFIXING", DbType.DateTime));
                        m_ParamEventDet.Add(new DataParameter(CS, "DTSETTLTPRICE", DbType.DateTime));
                        m_ParamEventDet.Add(new DataParameter(CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "FACTOR", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "FWDPOINTS", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "FXTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "GAPRATE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "IDC_BASE", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "IDC_REF", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "INTEREST", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "MULTIPLIER", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "NOTE", DbType.AnsiString, SQLCst.UT_NOTE_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "NOTIONALAMOUNT", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "NOTIONALREFERENCE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "PCTPAYOUT", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "PCTRATE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "PERIODPAYOUT", DbType.Int32));
                        m_ParamEventDet.Add(new DataParameter(CS, "PRICE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "PRICE100", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "QUOTEDELTA", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "QUOTEPRICE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "QUOTEPRICE100", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "QUOTEPRICEYEST", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "QUOTEPRICEYEST100", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "RATE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "SETTLTPRICE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "SETTLTPRICE100", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "SETTLTQUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "SETTLTQUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventDet.Add(new DataParameter(CS, "SETTLEMENTRATE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "SPOTRATE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "SPREAD", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "STRIKEPRICE", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "TOTALOFDAY", DbType.Int32));
                        m_ParamEventDet.Add(new DataParameter(CS, "TOTALOFYEAR", DbType.Int32));
                        m_ParamEventDet.Add(new DataParameter(CS, "TOTALPAYOUTAMOUNT", DbType.Decimal));
                        m_ParamEventDet.Add(new DataParameter(CS, "PIP", DbType.Decimal));
                    }
                    m_ParamEventDet.SetAllDBNull();
                    #endregion EVENTDET
                    break;
                case Cst.OTCml_TBL.EVENTFEE:
                    #region EVENTFEE

                    if (null == m_ParamEventFee)
                    {
                        m_ParamEventFee = new DataParameters(new DataParameter[] { });
                        m_ParamEventFee.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEventFee.Add(new DataParameter(CS, "STATUS", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "IDFEEMATRIX", DbType.Int32));
                        m_ParamEventFee.Add(new DataParameter(CS, "IDFEE", DbType.Int32));
                        m_ParamEventFee.Add(new DataParameter(CS, "IDFEESCHEDULE", DbType.Int32));
                        m_ParamEventFee.Add(new DataParameter(CS, "BRACKET1", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "BRACKET2", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULA", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULAVALUE1", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULAVALUE2", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULAVALUEBRACKET", DbType.AnsiString, SQLCst.UT_NOTE_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULADCF", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULAMIN", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FORMULAMAX", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "FEEPAYMENTFREQUENCY", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                        //PL 20141023
                        //m_ParamEventFee.Add(new DataParameter(CS, "ASSESSMENTBASISVALUE", DbType.Decimal));
                        m_ParamEventFee.Add(new DataParameter(CS, "ASSESSMENTBASISVALUE1", DbType.Decimal));
                        m_ParamEventFee.Add(new DataParameter(CS, "ASSESSMENTBASISVALUE2", DbType.Decimal));
                        m_ParamEventFee.Add(new DataParameter(CS, "ISFEEINVOICING", DbType.Boolean));
                        m_ParamEventFee.Add(new DataParameter(CS, "PAYMENTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "ASSESSMENTBASISDET", DbType.AnsiString, 1000));
                        m_ParamEventFee.Add(new DataParameter(CS, "IDTAX", DbType.Int32));
                        m_ParamEventFee.Add(new DataParameter(CS, "IDTAXDET", DbType.Int32));
                        m_ParamEventFee.Add(new DataParameter(CS, "TAXTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventFee.Add(new DataParameter(CS, "TAXRATE", DbType.Decimal));
                        m_ParamEventFee.Add(new DataParameter(CS, "TAXCOUNTRY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    }
                    m_ParamEventFee.SetAllDBNull();
                    #endregion EVENTFEE
                    break;
                case Cst.OTCml_TBL.EVENTPOSACTIONDET:
                    #region EVENTPOSACTIONDET
                    if (null == m_ParamEventPosActionDet)
                    {
                        m_ParamEventPosActionDet = new DataParameters(new DataParameter[] { });
                        m_ParamEventPosActionDet.Add(new DataParameter(CS, "IDPADET", DbType.Int32));
                        m_ParamEventPosActionDet.Add(new DataParameter(CS, "IDE", DbType.Int32));
                    }
                    m_ParamEventPosActionDet.SetAllDBNull();
                    #endregion EVENTPOSACTIONDET
                    break;

                case Cst.OTCml_TBL.EVENTPRICING:
                    #region EVENTPRICING
                    if (null == m_ParamEventPricing)
                    {
                        m_ParamEventPricing = new DataParameters(new DataParameter[] { });
                        m_ParamEventPricing.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventPricing.Add(new DataParameter(CS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DCF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DCFNUM", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DCFDEN", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "TOTALOFYEAR", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "TOTALOFDAY", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "TIMETOEXPIRATION", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DCF2", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DCFNUM2", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DCFDEN2", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "TOTALOFYEAR2", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "TOTALOFDAY2", DbType.Int32));
                        m_ParamEventPricing.Add(new DataParameter(CS, "TIMETOEXPIRATION2", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "STRIKE", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "INTERESTRATE1", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "INTERESTRATE2", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "SPOTRATE", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "EXCHANGERATE", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "VOLATILITY", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "UNDERLYINGPRICE", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DIVIDENDYIELD", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "RISKFREEINTEREST", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CALLPRICE", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CALLDELTA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CALLRHO1", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CALLRHO2", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CALLTHETA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CALLCHARM", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "PUTPRICE", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "PUTDELTA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "PUTRHO1", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "PUTRHO2", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "PUTTHETA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "PUTCHARM", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "DELTA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "GAMMA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "VEGA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "THETA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "RHO", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "BPV", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "COLOR", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "SPEED", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "VANNA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "VOLGA", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "CONVEXITY", DbType.Decimal));
                        m_ParamEventPricing.Add(new DataParameter(CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                    }
                    m_ParamEventPricing.SetAllDBNull();
                    #endregion EVENTPRICING
                    break;

                case Cst.OTCml_TBL.EVENTPRICING2:
                    #region EVENTPRICING2
                    if (null == m_ParamEventPricing2)
                    {
                        m_ParamEventPricing2 = new DataParameters(new DataParameter[] { });
                        m_ParamEventPricing2.Add(new DataParameter(CS, "IDE", DbType.Int32));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "IDE_SOURCE", DbType.Int32));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "FLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DTFIXING", DbType.DateTime));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "CASHFLOW", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DTSTART", DbType.DateTime));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DTCLOSING", DbType.DateTime));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DTEND", DbType.DateTime));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DTPAYMENT", DbType.DateTime));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "TOTALOFDAY", DbType.Int32));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "VOLATILITY", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DISCOUNTFACTOR", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "RATE", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "STRIKE", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "BARRIER", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "FWDDELTA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "FWDGAMMA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "VEGA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "FXVEGA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "THETA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "BPV", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "CONVEXITY", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "DELTA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "GAMMA", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "NPV", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "METHOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "IDYIELDCURVEVAL_H", DbType.Int32));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "ZEROCOUPON1", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "ZEROCOUPON2", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "FORWARDRATE", DbType.Decimal));
                        m_ParamEventPricing2.Add(new DataParameter(CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));

                    }
                    m_ParamEventPricing2.SetAllDBNull();
                    #endregion EVENTPRICING
                    break;

            }
        }
        #endregion InitParameters

        #region InitPaymentForEvent
        /// <summary>
        ///  Enrichie l'array de pPaymentFees pour la génération des évènements
        /// <para>Ex alimentation des EFS_Payment</para>
        /// <para>Retourne le nb d'évènements qui doivent être injectés</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPaymentFees"></param>
        /// <param name="pNbEvent">Nbr d'évènements à injecter</param>
        public void InitPaymentForEvent(string pCS, IPayment[] pPaymentFees, DataDocumentContainer pDataDocument, out int pNbEvent)
        {
            int nbEvent = 0;
            if (ArrFunc.IsFilled(pPaymentFees))
            {
                EFS_TradeLibrary savTradeLibrary = EFS_Current.tradeLibrary;
                if (null == EFS_Current.tradeLibrary)
                    EFS_Current.tradeLibrary = new EFS_TradeLibrary(pDataDocument.dataDocument);

                foreach (IPayment payment in pPaymentFees)
                {
                    payment.efs_Payment = new EFS_Payment(pCS, payment, pDataDocument.currentTrade.product);
                    nbEvent++;
                    if (payment.efs_Payment.originalPaymentSpecified)
                        nbEvent += 1;
                    if (payment.efs_Payment.taxSpecified)
                        nbEvent += payment.efs_Payment.tax.Length;
                }
                EFS_Current.tradeLibrary = savTradeLibrary;
            }
            pNbEvent = nbEvent;
        }
        #endregion InitPaymentForEvent

        #region InitQueries
        private void InitQueries()
        {
            // EG 20141008 NEXTVERSION
            m_SqlInsertEvent = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENT);
            m_SqlInsertEventAsset = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENTASSET);
            m_SqlInsertEventClass = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENTCLASS);
            m_SqlInsertEventDet = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENTDET);
            m_SqlInsertEventFee = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENTFEE);
            m_SqlInsertEventPosActionDet = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENTPOSACTIONDET);
        }

        #endregion InitQueries

        #region InsertEvent
        // EG 20150128 [20726] Math.Abs on VALORISATION|VALORISATIONSYS
        public Cst.ErrLevel InsertEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, Nullable<int> pIdE_Source, int pInstrumentNo, int pStreamNo,
            Nullable<int> pIdA_Payer, Nullable<int> pIdB_Payer, Nullable<int> pIdA_Receiver, Nullable<int> pIdB_Receiver,
            string pEventCode, string pEventType, DateTime pDtStartUnadj, DateTime pDtStartAdj,
            DateTime pDtEndUnadj, DateTime pDtEndAdj, Nullable<decimal> pValorisation,
            string pUnit, string pUnitType, Nullable<StatusCalculEnum> pStatusCalcul, Nullable<Cst.StatusTrigger.StatusTriggerEnum> pIdStTrigger)
        {

            InitParameters(Cst.OTCml_TBL.EVENT);

            m_ParamEvent["IDT"].Value = pIdT;
            m_ParamEvent["IDE"].Value = pIdE;
            m_ParamEvent["IDE_EVENT"].Value = pIdE_Event;
            m_ParamEvent["INSTRUMENTNO"].Value = pInstrumentNo;
            m_ParamEvent["STREAMNO"].Value = pStreamNo;

            if (pIdE_Source.HasValue)
                m_ParamEvent["IDE_SOURCE"].Value = pIdE_Source.Value;

            m_ParamEvent["EVENTCODE"].Value = pEventCode;
            m_ParamEvent["EVENTTYPE"].Value = pEventType;

            m_ParamEvent["DTSTARTADJ"].Value = DataHelper.GetDBData(pDtStartAdj);
            m_ParamEvent["DTSTARTUNADJ"].Value = DataHelper.GetDBData(pDtStartUnadj);
            // EG 20150723 [21215]
            m_ParamEvent["DTENDADJ"].Value = DataHelper.GetDBData(pDtEndAdj);
            m_ParamEvent["DTENDUNADJ"].Value = DataHelper.GetDBData(pDtEndUnadj);

            if (pIdA_Payer.HasValue)
                m_ParamEvent["IDA_PAY"].Value = pIdA_Payer.Value;
            if (pIdB_Payer.HasValue)
                m_ParamEvent["IDB_PAY"].Value = pIdB_Payer.Value;
            if (pIdA_Receiver.HasValue)
                m_ParamEvent["IDA_REC"].Value = pIdA_Receiver.Value;
            if (pIdB_Receiver.HasValue)
                m_ParamEvent["IDB_REC"].Value = pIdB_Receiver.Value;

            // EG 20150129 Add Math.Abs
            if (pValorisation.HasValue)
            {
                m_ParamEvent["VALORISATION"].Value = Math.Abs(pValorisation.Value);
                m_ParamEvent["VALORISATIONSYS"].Value = Math.Abs(pValorisation.Value);
            }
            if (StrFunc.IsFilled(pUnit))
            {
                m_ParamEvent["UNIT"].Value = pUnit;
                m_ParamEvent["UNITSYS"].Value = pUnit;
            }
            if (StrFunc.IsFilled(pUnitType))
            {
                m_ParamEvent["UNITTYPE"].Value = pUnitType;
                m_ParamEvent["UNITTYPESYS"].Value = pUnitType;
            }

            m_ParamEvent["IDSTACTIVATION"].Value = Cst.STATUSREGULAR.ToString();
            m_ParamEvent["IDASTACTIVATION"].Value = m_ProcessBase.appInstance.IdA; 
            m_ParamEvent["DTSTACTIVATION"].Value = m_DtSys;

            m_ParamEvent["IDSTCALCUL"].Value = pStatusCalcul.HasValue ? pStatusCalcul.Value.ToString() : StatusCalculEnum.CALC.ToString();
            m_ParamEvent["IDSTTRIGGER"].Value = pIdStTrigger.HasValue ? pIdStTrigger.Value.ToString() : Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            m_ParamEvent["SOURCE"].Value = m_ProcessBase.appInstance.serviceName;

            QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEvent, m_ParamEvent);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            m_EventProcess.Write(pDbTransaction, pIdE, Cst.ProcessTypeEnum.CLOSINGGEN, ProcessStateTools.StatusSuccessEnum, m_DtSys, m_ProcessBase.tracker.idTRK_L);
            return Cst.ErrLevel.SUCCESS;
        }
        /// <summary>
        /// Appel via EVENTSGEN
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pEvent">Données de l'événement à générer</param>
        /// <param name="pEventParent">Données de l'événement parent</param>
        /// <param name="pDataDocument">DataDocumentContainer</param>
        /// <param name="pIsEventOnly">Génération des événement seuls</param>
        /// <returns></returns>
        /// EG 20150706 [21021] Nullable integer (idA_Pay|idB_Pay|idA_Rec|idB_Rec)
        public int InsertEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, EFS_Event pEvent, EFS_EventParent pEventParent, 
            DataDocumentContainer pDataDocument, bool pIsEventOnly)
        {
            InitParameters(Cst.OTCml_TBL.EVENT);

            Nullable<int> idE_Source = null;
            if (pEvent.eventKey.idE_SourceSpecified)
            idE_Source = pEvent.eventKey.idE_Source;

            int idE_Event = 0;
            
            // Case of OPP and ADP events
            if (null != pEventParent)
            {
                idE_Event = pEventParent.idE;
                // PM 20150324 [POC] Ne pas mettre à 0 le N° de stream des OPP pour les CashBalances
                if (false == pDataDocument.currentProduct.IsCashBalance)
                {
                    pEventParent.ChangeSequence(pEvent);
                }
            }
            m_ParamEvent["INSTRUMENTNO"].Value = pEvent.instrumentNo;
            m_ParamEvent["STREAMNO"].Value = pEvent.streamNo;


            Nullable<int> idA_Pay = null;
            Nullable<int> idB_Pay = null;
            Nullable<int> idA_Rec = null;
            Nullable<int> idB_Rec = null;

            // Payer/Receiver (If... Else pour gérer les payeurs des frais élémentaires sur facture)
            if ((pEvent.eventKey.idE_SourceSpecified || ((null != pEventParent) && pEventParent.idE_SourceSpecified)) &&
                ((null != pEvent.payer) && (-1 < pEvent.payer.IndexOf(';'))))
            {
                string[] invoice_Payer = pEvent.payer.Split(';');
                idA_Pay = Convert.ToInt32(invoice_Payer[0]);
                if (1 < invoice_Payer.Length)
                    idB_Pay = Convert.ToInt32(invoice_Payer[1]);
            }
            else
            {
                idA_Pay = pDataDocument.GetOTCmlId_Party(pEvent.payer);
                idB_Pay = pDataDocument.GetOTCmlId_Book(pEvent.payer);
            }
            idA_Rec = pDataDocument.GetOTCmlId_Party(pEvent.receiver);
            idB_Rec = pDataDocument.GetOTCmlId_Book(pEvent.receiver);

            string eventCode = EventCodeFunc.IsStrategy(pEvent.eventKey.eventCode) ? EventCodeFunc.Product : pEvent.eventKey.eventCode;

            Nullable<decimal> valorisation = null;
            if (pEvent.valorisationSpecified) 
                valorisation = pEvent.valorisation.DecValue;
            
            string unit = string.Empty;
            if (pEvent.unitSpecified) 
                unit = pEvent.unit;

            string unitType = string.Empty;
            if (pEvent.unitTypeSpecified) 
                unitType = pEvent.unitType.ToString();

            Nullable<Cst.StatusTrigger.StatusTriggerEnum> idStTrigger = null;
            if (EventCodeFunc.IsTrigger(pEvent.eventKey.eventCode))
                idStTrigger = Cst.StatusTrigger.StatusTriggerEnum.NONE;

            Cst.ErrLevel ret = InsertEvent(pDbTransaction, pIdT, pIdE, idE_Event, idE_Source, pEvent.instrumentNo, pEvent.streamNo,
                idA_Pay, idB_Pay, idA_Rec, idB_Rec, eventCode, pEvent.eventKey.eventType,
                pEvent.startDate.unadjustedDate.DateValue, pEvent.startDate.adjustedDate.DateValue,
                pEvent.endDate.unadjustedDate.DateValue, pEvent.endDate.adjustedDate.DateValue,
                valorisation, unit, unitType, pEvent.idStCalcul, idStTrigger);

            return (ret == Cst.ErrLevel.SUCCESS?1:-1);

        }
        #endregion InsertEvent

        #region InsertEventAsset
        /// <summary>
        /// Insertion dans la table EVENTASSET (NEW VERSION)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pAsset"></param>
        /// <returns></returns>
        // EG 20150203 IDM Setting to all Asset and IDEC to FXRateAsset
        public Cst.ErrLevel InsertEventAsset(IDbTransaction pDbTransaction, int pIdE, EFS_Asset pAsset)
        {
            if ((null != pAsset) && (pAsset.assetCategory.HasValue))
            {
                if (pAsset.assetCategory.HasValue)
                {
                    InitParameters(Cst.OTCml_TBL.EVENTASSET);

                    m_ParamEventAsset["IDE"].Value = pIdE;
                    m_ParamEventAsset["IDASSET"].Value = pAsset.idAsset;
                    m_ParamEventAsset["ASSETCATEGORY"].Value = DataHelper.GetDBData(pAsset.assetCategory.Value);
                    m_ParamEventAsset["ASSETTYPE"].Value = DataHelper.GetDBData(pAsset.AssetType.Value);
                    m_ParamEventAsset["IDM"].Value = DataHelper.GetDBData(pAsset.IdMarket);

                    switch (pAsset.assetCategory.Value)
                    {
                        case Cst.UnderlyingAsset.RateIndex:
                        case Cst.UnderlyingAsset.FxRateAsset:
                            m_ParamEventAsset["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            m_ParamEventAsset["TIME"].Value = DataHelper.GetDBData(pAsset.time);
                            m_ParamEventAsset["IDBC"].Value = DataHelper.GetDBData(pAsset.idBC);
                            m_ParamEventAsset["PRIMARYRATESRC"].Value = DataHelper.GetDBData(pAsset.primaryRateSrc);
                            m_ParamEventAsset["PRIMARYRATESRCPAGE"].Value = DataHelper.GetDBData(pAsset.primaryRateSrcPage);
                            m_ParamEventAsset["PRIMARYRATESRCHEAD"].Value = DataHelper.GetDBData(pAsset.primaryRateSrcHead);
                            break;
                        case Cst.UnderlyingAsset.EquityAsset:
                            m_ParamEventAsset["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            m_ParamEventAsset["ISINCODE"].Value = DataHelper.GetDBData(pAsset.isinCode);
                            m_ParamEventAsset["ASSETSYMBOL"].Value = DataHelper.GetDBData(pAsset.assetSymbol);
                            break;
                        case Cst.UnderlyingAsset.Index:
                        case Cst.UnderlyingAsset.Bond:
                            m_ParamEventAsset["CLEARANCESYSTEM"].Value = DataHelper.GetDBData(pAsset.clearanceSystem);
                            m_ParamEventAsset["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            m_ParamEventAsset["ISINCODE"].Value = DataHelper.GetDBData(pAsset.isinCode);
                            break;
                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                        case Cst.UnderlyingAsset.Future:
                            m_ParamEventAsset["CLEARANCESYSTEM"].Value = DataHelper.GetDBData(pAsset.clearanceSystem);
                            m_ParamEventAsset["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            m_ParamEventAsset["ISINCODE"].Value = DataHelper.GetDBData(pAsset.isinCode);
                            m_ParamEventAsset["ASSETSYMBOL"].Value = DataHelper.GetDBData(pAsset.assetSymbol);
                            m_ParamEventAsset["IDENTIFIER"].Value = DataHelper.GetDBData(pAsset.contractIdentifier);
                            m_ParamEventAsset["DISPLAYNAME"].Value = DataHelper.GetDBData(pAsset.contractDisplayName);
                            m_ParamEventAsset["CONTRACTSYMBOL"].Value = DataHelper.GetDBData(pAsset.contractSymbol);
                            m_ParamEventAsset["CATEGORY"].Value = DataHelper.GetDBData(pAsset.category);
                            if (CfiCodeCategoryEnum.Option == pAsset.category)
                            {
                                m_ParamEventAsset["PUTORCALL"].Value = DataHelper.GetDBData(pAsset.putOrCall);
                                if (null != pAsset.strikePrice)
                                    m_ParamEventAsset["STRIKEPRICE"].Value = DataHelper.GetDBData(pAsset.strikePrice.DecValue);
                            }
                            if (null != pAsset.contractMultiplier)
                                m_ParamEventAsset["CONTRACTMULTIPLIER"].Value = DataHelper.GetDBData(pAsset.contractMultiplier.DecValue);
                            m_ParamEventAsset["MATURITYDATE"].Value = DataHelper.GetDBData(pAsset.maturityDate);
                            m_ParamEventAsset["MATURITYDATESYS"].Value = DataHelper.GetDBData(pAsset.maturityDateSys);
                            m_ParamEventAsset["DELIVERYDATE"].Value = DataHelper.GetDBData(pAsset.deliveryDate);
                            m_ParamEventAsset["NOMINALVALUE"].Value = DataHelper.GetDBData(pAsset.nominalValue);
                            break;
                    }
                    QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventAsset, m_ParamEventAsset);
                    int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventAsset
        #region InsertEventClass
        /// <summary>
        /// Insertion dans la table EVENTCLASS (NEW VERSION)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pEventClass"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pIsPayment"></param>
        /// <returns></returns>
        public Cst.ErrLevel InsertEventClass(IDbTransaction pDbTransaction, int pIdE, string pEventClass, DateTime pDtEvent, bool pIsPayment)
        {
            InitParameters(Cst.OTCml_TBL.EVENTCLASS);

            m_ParamEventClass["IDE"].Value = pIdE;
            m_ParamEventClass["EVENTCLASS"].Value = pEventClass;
            m_ParamEventClass["DTEVENT"].Value = pDtEvent;
            m_ParamEventClass["DTEVENTFORCED"].Value = m_DtSys;
            m_ParamEventClass["ISPAYMENT"].Value = pIsPayment;

            if (0 < DateTime.Compare(pDtEvent, m_DtSys))
                m_ParamEventClass["DTEVENTFORCED"].Value = pDtEvent;

            QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventClass, m_ParamEventClass);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventClass
        #region InsertEventFee
        /// <summary>
        /// Insertion dans la table EVENTFEE (NEW VERSION)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pSource"></param>
        /// <returns></returns>
        public Cst.ErrLevel InsertEventFee<T>(IDbTransaction pDbTransaction, int pIdE, T pSource)
        {
            bool isEventFee = false;

            InitParameters(Cst.OTCml_TBL.EVENTFEE);

            m_ParamEventFee["IDE"].Value = pIdE;

            if (pSource is EFS_PaymentSource)
            {
                #region PAYMENT
                EFS_PaymentSource _source = pSource as EFS_PaymentSource;
                if (null != _source)
                {
                    isEventFee = true;
                    if (_source.statusSpecified)
                        m_ParamEventFee["STATUS"].Value = _source.status.ToString();
                    if (_source.idFeeMatrixSpecified)
                        m_ParamEventFee["IDFEEMATRIX"].Value = _source.idFeeMatrix;
                    if (_source.idFeeSpecified)
                        m_ParamEventFee["IDFEE"].Value = _source.idFee;
                    if (_source.idFeeScheduleSpecified)
                        m_ParamEventFee["IDFEESCHEDULE"].Value = _source.idFeeSchedule;
                    if (_source.bracket1Specified)
                        m_ParamEventFee["BRACKET1"].Value = _source.bracket1;
                    if (_source.bracket2Specified)
                        m_ParamEventFee["BRACKET2"].Value = _source.bracket2;
                    if (_source.formulaSpecified)
                        m_ParamEventFee["FORMULA"].Value = _source.formula;
                    if (_source.formulaValue1Specified)
                        m_ParamEventFee["FORMULAVALUE1"].Value = _source.formulaValue1;
                    if (_source.formulaValue2Specified)
                        m_ParamEventFee["FORMULAVALUE2"].Value = _source.formulaValue2;
                    if (_source.formulaValueBracketSpecified)
                        m_ParamEventFee["FORMULAVALUEBRACKET"].Value = _source.formulaValueBracket;
                    if (_source.formulaDCFSpecified)
                        m_ParamEventFee["FORMULADCF"].Value = _source.formulaDCF;
                    if (_source.formulaMinSpecified)
                        m_ParamEventFee["FORMULAMIN"].Value = _source.formulaMin;
                    if (_source.formulaMaxSpecified)
                        m_ParamEventFee["FORMULAMAX"].Value = _source.formulaMax;
                    if (_source.feePaymentFrequencySpecified)
                        m_ParamEventFee["FEEPAYMENTFREQUENCY"].Value = _source.feePaymentFrequency;
                    //PL 20141023
                    //if (_source.assessmentBasisValueSpecified)
                    //    m_ParamEventFee["ASSESSMENTBASISVALUE"].Value = _source.assessmentBasisValue;
                    if (_source.assessmentBasisValue1Specified)
                        m_ParamEventFee["ASSESSMENTBASISVALUE1"].Value = _source.assessmentBasisValue1;
                    if (_source.assessmentBasisValue2Specified)
                        m_ParamEventFee["ASSESSMENTBASISVALUE2"].Value = _source.assessmentBasisValue2;
                    if (_source.paymentTypeSpecified)
                        m_ParamEventFee["PAYMENTTYPE"].Value = _source.paymentType;
                    if (_source.assessmentBasisDetSpecified)
                        m_ParamEventFee["ASSESSMENTBASISDET"].Value = _source.assessmentBasisDet;

                    m_ParamEventFee["ISFEEINVOICING"].Value = _source.isFeeInvoicing;
                }
                #endregion PAYMENT
            }
            else if (pSource is EFS_TaxSource)
            {
                #region TAX
                EFS_TaxSource _source = pSource as EFS_TaxSource;
                if (null != _source)
                {
                    isEventFee = true;
                    if (_source.idTaxSpecified)
                        m_ParamEventFee["IDTAX"].Value = _source.idTax;
                    if (_source.idTaxDetSpecified)
                        m_ParamEventFee["IDTAXDET"].Value = _source.idTaxDet;
                    if (_source.taxTypeSpecified)
                        m_ParamEventFee["TAXTYPE"].Value = _source.taxType;
                    if (_source.taxRateSpecified)
                        m_ParamEventFee["TAXRATE"].Value = _source.taxRate;
                    if (_source.taxCountrySpecified)
                        m_ParamEventFee["TAXCOUNTRY"].Value = _source.taxCountry;
                }
                #endregion TAX
            }

            int rowAffected = 0;
            if (isEventFee)
            {
                QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventFee, m_ParamEventFee);
                rowAffected += DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventFee
        #region InsertEventPosActionDet
        /// <summary>
        /// Insertion de la ligne matérialisant les liens entre POSACTIONDET et EVENT
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPADET">Identifiant de la ligne matérialisant la clôture</param>
        /// <param name="pIdE">Identifiant de l'événément de départ (boucle de 2x4 EVENT (Clôturée et clôturante avec OFS,NOM,QTY et RMG)</param>
        /// <returns></returns>
        public Cst.ErrLevel InsertEventPosActionDet(IDbTransaction pDbTransaction, int pIdPADET, int pIdE)
        {
            InitParameters(Cst.OTCml_TBL.EVENTPOSACTIONDET);
            m_ParamEventPosActionDet["IDPADET"].Value = pIdPADET;
            m_ParamEventPosActionDet["IDE"].Value = pIdE;
            QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventPosActionDet, m_ParamEventPosActionDet);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventPosActionDet
        #region InsertEventDet
        public Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, int pIdE, EFS_EventDetail pEventDetail, string pUnitBase)
        {
            if (pEventDetail.IsEventDetSpecified)
            {
                InitParameters(Cst.OTCml_TBL.EVENTDET);
                m_ParamEventDet["IDE"].Value = pIdE;

                #region FixedRate
                if (pEventDetail.fixedRateSpecified)
                    m_ParamEventDet["RATE"].Value = pEventDetail.fixedRate.DecValue;
                #endregion FixedRate
                #region DayCountFraction
                if (pEventDetail.dayCountFractionSpecified)
                {
                    m_ParamEventDet["DCF"].Value = pEventDetail.dayCountFraction.DayCountFractionFpML;
                    m_ParamEventDet["DCFNUM"].Value = pEventDetail.dayCountFraction.Numerator;
                    m_ParamEventDet["DCFDEN"].Value = pEventDetail.dayCountFraction.Denominator;
                    m_ParamEventDet["TOTALOFYEAR"].Value = pEventDetail.dayCountFraction.NumberOfCalendarYears;
                    m_ParamEventDet["TOTALOFDAY"].Value = pEventDetail.dayCountFraction.TotalNumberOfCalculatedDays;
                }
                #endregion DayCountFraction
                #region Multiplier
                if (pEventDetail.multiplierSpecified)
                    m_ParamEventDet["MULTIPLIER"].Value = pEventDetail.multiplier.DecValue;
                #endregion Multiplier
                #region Spread
                if (pEventDetail.spreadSpecified)
                    m_ParamEventDet["SPREAD"].Value = pEventDetail.spread.DecValue;
                #endregion Spread

                if (pEventDetail.paymentQuoteSpecified)
                {
                    #region PaymentQuote
                    if (null != pEventDetail.paymentQuote.notionalAmount)
                        m_ParamEventDet["NOTIONALREFERENCE"].Value = pEventDetail.paymentQuote.notionalAmount.DecValue;
                    if (null != pEventDetail.paymentQuote.percentageRate)
                        m_ParamEventDet["PCTRATE"].Value = pEventDetail.paymentQuote.percentageRate.DecValue;
                    #endregion PaymentQuote
                }
                else if (pEventDetail.premiumQuoteSpecified)
                {
                    #region PremiumQuote
                    EFS_FxPremiumQuote fxPremiumQuote = pEventDetail.premiumQuote;
                    m_ParamEventDet["IDC1"].Value = fxPremiumQuote.callCurrency;
                    m_ParamEventDet["IDC2"].Value = fxPremiumQuote.putCurrency;
                    m_ParamEventDet["BASIS"].Value = fxPremiumQuote.premiumQuote.premiumQuoteBasis.ToString();
                    if (fxPremiumQuote.amountReferenceSpecified)
                    {
                        m_ParamEventDet["NOTIONALREFERENCE"].Value = fxPremiumQuote.amountReference.amount.DecValue;
                        m_ParamEventDet["IDC_REF"].Value = fxPremiumQuote.amountReference.currency;
                        m_ParamEventDet["PCTRATE"].Value = fxPremiumQuote.premiumQuote.premiumValue.DecValue;
                    }
                    #endregion PremiumQuote
                }
                else if (pEventDetail.etdPremiumCalculationSpecified)
                {
                    #region ETDPremium
                    EFS_ETDPremiumCalculation premiumCalculation = pEventDetail.etdPremiumCalculation;
                    m_ParamEventDet["DAILYQUANTITY"].Value = premiumCalculation.qty;
                    m_ParamEventDet["CONTRACTMULTIPLIER"].Value = premiumCalculation.multiplier;
                    m_ParamEventDet["PRICE"].Value = premiumCalculation.price;
                    m_ParamEventDet["PRICE100"].Value = premiumCalculation.price100;
                    #endregion ETDPremium
                }

                if (pEventDetail.exchangeRateSpecified)
                {
                    #region ExchangeRate
                    IExchangeRate iExchangeRate = pEventDetail.exchangeRate.exchangeRate;
                    m_ParamEventDet["IDC1"].Value = iExchangeRate.quotedCurrencyPair.currency1;
                    m_ParamEventDet["IDC2"].Value = iExchangeRate.quotedCurrencyPair.currency2;
                    m_ParamEventDet["BASIS"].Value = iExchangeRate.quotedCurrencyPair.quoteBasis.ToString();
                    m_ParamEventDet["RATE"].Value = iExchangeRate.rate.DecValue;
                    if (iExchangeRate.spotRateSpecified)
                        m_ParamEventDet["SPOTRATE"].Value = iExchangeRate.spotRate.DecValue;
                    if (iExchangeRate.forwardPointsSpecified)
                        m_ParamEventDet["FWDPOINTS"].Value = iExchangeRate.forwardPoints.DecValue;
                    #endregion ExchangeRate
                    if (pEventDetail.exchangeRate.referenceCurrencySpecified)
                        m_ParamEventDet["IDC_REF"].Value = pEventDetail.exchangeRate.referenceCurrency;
                    if (pEventDetail.exchangeRate.notionalAmountSpecified)
                        m_ParamEventDet["NOTIONALAMOUNT"].Value = pEventDetail.exchangeRate.notionalAmount.DecValue;
                }
                else if (pEventDetail.sideRateSpecified)
                {
                    #region SideRate
                    ISideRate iSideRate = pEventDetail.sideRate;
                    m_ParamEventDet["IDC_BASE"].Value = StrFunc.IsFilled(pUnitBase)? pUnitBase:Convert.DBNull;
                    if ((SideRateBasisEnum.BaseCurrencyPerCurrency1 == iSideRate.sideRateBasis) ||
                        (SideRateBasisEnum.Currency1PerBaseCurrency == iSideRate.sideRateBasis))
                        m_ParamEventDet["IDC1"].Value = iSideRate.currency;
                    else if ((SideRateBasisEnum.BaseCurrencyPerCurrency2 == iSideRate.sideRateBasis) ||
                        (SideRateBasisEnum.Currency2PerBaseCurrency == iSideRate.sideRateBasis))
                        m_ParamEventDet["IDC2"].Value = iSideRate.currency;
                    m_ParamEventDet["BASIS"].Value = iSideRate.sideRateBasis.ToString();
                    m_ParamEventDet["RATE"].Value = iSideRate.rate.DecValue;
                    if (iSideRate.spotRateSpecified)
                        m_ParamEventDet["SPOTRATE"].Value = iSideRate.spotRate.DecValue;
                    if (iSideRate.forwardPointsSpecified)
                        m_ParamEventDet["FWDPOINTS"].Value = iSideRate.forwardPoints.DecValue;
                    #endregion SideRate
                }
                else if (pEventDetail.fixingRateSpecified)
                {
                    #region FixingRate
                    EFS_FxFixing fixingRate = pEventDetail.fixingRate;
                    IFxFixing iFxFixing = fixingRate.fixing;
                    DateTime dtFixingTime = iFxFixing.fixingTime.hourMinuteTime.TimeValue;
                    DateTime dtFixing = DtFunc.AddTimeToDate(iFxFixing.fixingDate.DateValue, dtFixingTime);
                    m_ParamEventDet["DTFIXING"].Value = DataHelper.GetDBData(dtFixing);
                    m_ParamEventDet["IDBC"].Value = iFxFixing.fixingTime.businessCenter.Value;
                    m_ParamEventDet["IDC1"].Value = iFxFixing.quotedCurrencyPair.currency1;
                    m_ParamEventDet["IDC2"].Value = iFxFixing.quotedCurrencyPair.currency2;
                    m_ParamEventDet["BASIS"].Value = iFxFixing.quotedCurrencyPair.quoteBasis.ToString();
                    if (fixingRate.referenceCurrencySpecified)
                        m_ParamEventDet["IDC_REF"].Value = fixingRate.referenceCurrency;
                    if (fixingRate.notionalAmountSpecified)
                        m_ParamEventDet["NOTIONALAMOUNT"].Value = fixingRate.notionalAmount.DecValue;
                    #endregion FixingRate
                }
                else if (pEventDetail.invoicingAmountBaseSpecified)
                {
                    #region InvoicingAmountBase
                    EFS_InvoicingAmountBase invoicingAmountBase = pEventDetail.invoicingAmountBase;
                    m_ParamEventDet["NOTIONALAMOUNT"].Value = invoicingAmountBase.notionalAmount.DecValue;
                    m_ParamEventDet["IDC_REF"].Value = invoicingAmountBase.referenceCurrency;
                    #endregion InvoicingAmountBase
                }
                else if (pEventDetail.settlementRateSpecified)
                {
                    #region SettlementRate
                    EFS_SettlementRate settlementRate = pEventDetail.settlementRate;
                    m_ParamEventDet["FXTYPE"].Value = settlementRate.fxType.ToString();
                    m_ParamEventDet["NOTIONALAMOUNT"].Value = settlementRate.notionalAmount.DecValue;
                    m_ParamEventDet["RATE"].Value = settlementRate.forwardRate.DecValue;
                    m_ParamEventDet["IDC_REF"].Value = settlementRate.referenceCurrency;
                    if (settlementRate.spotRateSpecified)
                        m_ParamEventDet["SPOTRATE"].Value = settlementRate.spotRate.DecValue;
                    if (settlementRate.settlementRateSpecified)
                        m_ParamEventDet["SETTLEMENTRATE"].Value = settlementRate.settlementRate.DecValue;
                    if (settlementRate.conversionRateSpecified)
                        m_ParamEventDet["CONVERSIONRATE"].Value = settlementRate.conversionRate.DecValue;
                    #endregion SettlementRate
                }
                else if (pEventDetail.currencyPairSpecified)
                {
                    #region CurrencyPair
                    IQuotedCurrencyPair iCurrencyPair = pEventDetail.currencyPair;
                    m_ParamEventDet["IDC1"].Value = iCurrencyPair.currency1;
                    m_ParamEventDet["IDC2"].Value = iCurrencyPair.currency2;
                    m_ParamEventDet["BASIS"].Value = iCurrencyPair.quoteBasis.ToString();
                    #endregion CurrencyPair
                }
                else if (pEventDetail.triggerRateSpecified)
                {
                    #region TriggerRate
                    EFS_TriggerRate triggerRate = pEventDetail.triggerRate;
                    m_ParamEventDet["SPOTRATE"].Value = triggerRate.spotRate.DecValue;
                    m_ParamEventDet["RATE"].Value = triggerRate.triggerRate.DecValue;
                    IQuotedCurrencyPair iCurrencyPair = triggerRate.currencyPair;
                    m_ParamEventDet["IDC1"].Value = iCurrencyPair.currency1;
                    m_ParamEventDet["IDC2"].Value = iCurrencyPair.currency2;
                    m_ParamEventDet["BASIS"].Value = iCurrencyPair.quoteBasis.ToString();
                    #endregion TriggerRate
                }
                else if (pEventDetail.strikePriceSpecified)
                {
                    #region StrikePrice
                    IFxStrikePrice iStrikePrice = pEventDetail.strikePrice;
                    m_ParamEventDet["STRIKEPRICE"].Value = iStrikePrice.rate.DecValue;
                    m_ParamEventDet["BASIS"].Value = iStrikePrice.strikeQuoteBasis.ToString();
                    #endregion StrikePrice
                }
                else if (pEventDetail.strikeSpecified)
                {
                    #region Strike
                    m_ParamEventDet["STRIKEPRICE"].Value = pEventDetail.strike.DecValue;
                    #endregion Strike
                }
                else if (pEventDetail.triggerEventSpecified)
                {
                    #region TriggerEvent
                    EFS_TriggerEvent triggerEvent = pEventDetail.triggerEvent;
                    m_ParamEventDet["RATE"].Value = triggerEvent.triggerPrice.DecValue;
                    if (triggerEvent.strike.spotPriceSpecified)
                        m_ParamEventDet["SPOTRATE"].Value = triggerEvent.strike.spotPrice.DecValue;
                    #endregion TriggerRate
                }

                QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventDet, m_ParamEventDet);
                int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            }
            return Cst.ErrLevel.SUCCESS;


        }
        #endregion InsertEventDet
        #region InsertEventDet_Notes
        // EG 20141114 Correction alimentation paramètres IDE, DTACTION, NOTE
        public Cst.ErrLevel InsertEventDet_Notes(IDbTransaction pDbTransaction, int pIdE, string pNotes)
        {
            InitParameters(Cst.OTCml_TBL.EVENTDET);
            m_ParamEventDet["IDE"].Value = pIdE;
            m_ParamEventDet["DTACTION"].Value = m_DtSys;
            m_ParamEventDet["NOTE"].Value = (StrFunc.IsFilled(pNotes) ? pNotes : Convert.DBNull);
            QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventDet, m_ParamEventDet);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet_Notes
        #region InsertEventDet_Denouement
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public Cst.ErrLevel InsertEventDet_Denouement(IDbTransaction pDbTransaction, int pIdE, long pQuantity,
            Nullable<decimal> pContractMultiplier, Nullable<decimal> pStrikePrice, 
            Nullable<QuotationSideEnum> pSettltQuoteSide, Nullable<QuoteTimingEnum> pSettltQuoteTiming,
            Nullable<DateTime> pDtSettltPrice, Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100, string pNotes)
        {
            InitParameters(Cst.OTCml_TBL.EVENTDET);

            m_ParamEventDet["IDE"].Value = pIdE;
            m_ParamEventDet["DTACTION"].Value = m_DtSys;
            m_ParamEventDet["NOTE"].Value = (StrFunc.IsFilled(pNotes) ? pNotes : Convert.DBNull);

            if (pContractMultiplier.HasValue)
                m_ParamEventDet["CONTRACTMULTIPLIER"].Value = pContractMultiplier.Value;

            m_ParamEventDet["DAILYQUANTITY"].Value = pQuantity;
            
            if (pDtSettltPrice.HasValue)
                m_ParamEventDet["DTSETTLTPRICE"].Value = pDtSettltPrice.Value;

            if (pSettltQuoteSide.HasValue)
                m_ParamEventDet["SETTLTQUOTESIDE"].Value = pSettltQuoteSide.Value;
            if (pSettltQuoteTiming.HasValue)
                m_ParamEventDet["SETTLTQUOTETIMING"].Value = pSettltQuoteTiming.Value;
            if (pSettltPrice.HasValue)
                m_ParamEventDet["SETTLTPRICE"].Value = pSettltPrice.Value;
            if (pSettltPrice100.HasValue)
                m_ParamEventDet["SETTLTPRICE100"].Value = pSettltPrice100.Value;
            if (pStrikePrice.HasValue)
                m_ParamEventDet["STRIKEPRICE"].Value = pStrikePrice.Value;

            QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventDet, m_ParamEventDet);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS; 
        }
        #endregion InsertEventDet_Denouement
        #region InsertEventDet_Closing
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public Cst.ErrLevel InsertEventDet_Closing(IDbTransaction pDbTransaction, int pIdE, long pQuantity,
            decimal pContractMultiplier, Nullable<decimal> pPrice, Nullable<decimal> pPrice100, Nullable<decimal> pClosingPrice, Nullable<decimal> pClosingPrice100)
        {
            return InsertEventDet_Closing(pDbTransaction, pIdE, pQuantity, pContractMultiplier, null, null, pPrice, pPrice100,
                null, null, null, null, null, null, pClosingPrice, pClosingPrice100);
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        public Cst.ErrLevel InsertEventDet_Closing(IDbTransaction pDbTransaction, int pIdE, long pQuantity,
            Nullable<decimal> pContractMultiplier, Nullable<decimal> pFactor, Nullable<decimal> pStrikePrice,
            Nullable<decimal> pPrice, Nullable<decimal> pPrice100,
            string pQuoteTiming, Nullable<decimal> pQuotePrice, Nullable<decimal> pQuotePrice100, Nullable<decimal> pQuoteDelta,
            Nullable<decimal> pQuotePriceYest, Nullable<decimal> pQuotePriceYest100,
            Nullable<decimal> pClosingPrice, Nullable<decimal> pClosingPrice100)
        {
            InitParameters(Cst.OTCml_TBL.EVENTDET);

            m_ParamEventDet["IDE"].Value = pIdE;

            if (pClosingPrice.HasValue)
                m_ParamEventDet["CLOSINGPRICE"].Value = pClosingPrice.Value;
            if (pClosingPrice100.HasValue)
                m_ParamEventDet["CLOSINGPRICE100"].Value = pClosingPrice100.Value;

            if (pContractMultiplier.HasValue)
                m_ParamEventDet["CONTRACTMULTIPLIER"].Value = pContractMultiplier.Value;

            m_ParamEventDet["DAILYQUANTITY"].Value = pQuantity;
            m_ParamEventDet["DTACTION"].Value = m_DtSys;

            if (pFactor.HasValue)
                m_ParamEventDet["FACTOR"].Value = pFactor.Value;

            if (pPrice.HasValue)
                m_ParamEventDet["PRICE"].Value = pPrice.Value;
            if (pPrice100.HasValue)
                m_ParamEventDet["PRICE100"].Value = pPrice100.Value;

            if (pQuoteDelta.HasValue)
                m_ParamEventDet["QUOTEDELTA"].Value = pQuoteDelta;
            if (StrFunc.IsFilled(pQuoteTiming))
                m_ParamEventDet["QUOTETIMING"].Value = pQuoteTiming;
            if (pQuotePrice.HasValue)
                m_ParamEventDet["QUOTEPRICE"].Value = pQuotePrice.Value;
            if (pQuotePrice100.HasValue)
                m_ParamEventDet["QUOTEPRICE100"].Value = pQuotePrice100.Value;
            if (pQuotePriceYest.HasValue)
                m_ParamEventDet["QUOTEPRICEYEST"].Value = pQuotePriceYest.Value;
            if (pQuotePriceYest100.HasValue)
                m_ParamEventDet["QUOTEPRICEYEST100"].Value = pQuotePriceYest100.Value;

            if (pStrikePrice.HasValue)
                m_ParamEventDet["STRIKEPRICE"].Value = pStrikePrice.Value;

            QueryParameters qryParameters = new QueryParameters(CS, m_SqlInsertEventDet, m_ParamEventDet);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet_Closing

        #region SetFeeLogInformation
        public void SetFeeLogInformation(ProcessBase pProcess, DataDocumentContainer pDataDocument, IPayment[] pPayments)
        {
            foreach (IPayment payment in pPayments)
                SetFeeLogInformation(pProcess, pDataDocument, payment);
        }
        // EG 20150706 [21021] Nullable integer (idA_Payer|idA_Receiver)
        public void SetFeeLogInformation(ProcessBase pProcess, DataDocumentContainer pDataDocument, IPayment pPayment)
        {
            string typePayment = Cst.NotAvailable;
            if (pPayment.paymentTypeSpecified)
                typePayment = pPayment.paymentType.Value;
            if (pPayment.paymentSourceSpecified)
            {
                ISpheresIdSchemeId feeMatrix = pPayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme);
                if (null != feeMatrix)
                {
                    typePayment += " : " + feeMatrix.Value;
                    ISpheresIdSchemeId feeSchedule = pPayment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme);
                    if (null != feeSchedule)
                        typePayment += " / " + feeSchedule.Value;
                }
            }
            Nullable<int> idA_Payer = pDataDocument.GetOTCmlId_Party(pPayment.payerPartyReference.hRef, PartyInfoEnum.id);
            Nullable<int> idA_Receiver = pDataDocument.GetOTCmlId_Party(pPayment.receiverPartyReference.hRef, PartyInfoEnum.id);
            IBookId bookPayer = pDataDocument.GetBookId(pPayment.payerPartyReference.hRef);
            IBookId bookReceiver = pDataDocument.GetBookId(pPayment.receiverPartyReference.hRef);

            pProcess.ProcessLogAddDetail(ProcessStateTools.StatusNoneEnum, ErrorManager.DetailEnum.FULL, "LOG-05021",
                typePayment,
                pPayment.paymentDateSpecified ? DtFunc.DateTimeToStringDateISO(pPayment.paymentDate.unadjustedDate.DateValue) : Cst.NotAvailable,
                LogTools.IdentifierAndId(pPayment.payerPartyReference.hRef, idA_Payer.Value) + " / " +
                ((null != bookPayer) ? LogTools.IdentifierAndId(bookPayer.bookName, bookPayer.OTCmlId) : ""),
                LogTools.IdentifierAndId(pPayment.receiverPartyReference.hRef, idA_Receiver.Value) + " / " +
                ((null != bookReceiver) ? LogTools.IdentifierAndId(bookReceiver.bookName, bookReceiver.OTCmlId) : ""),
                StrFunc.FmtDecimalToInvariantCulture(pPayment.paymentAmount.amount.DecValue) + " " + pPayment.PaymentCurrency);
        }

        #endregion SetFeeLogInformation

        /* ---------------------------------------------------------------------- */
        /* Utilisées pour le recalcul des SCU|VMG sur Dénouements d'options ETD   */
        /* ---------------------------------------------------------------------- */
        #region UpdateEvent_Amount
        // EG 20140116 [19456]
        // EG 20150129 [20726]
        public Cst.ErrLevel UpdateEvent_Amount(string pCS, IDbTransaction pDbTransaction, int pIdE,
            Nullable<decimal> pAmount, Nullable<decimal> pIdA_Payer, Nullable<decimal> pIdB_Payer,
            Nullable<decimal> pIdA_Receiver, Nullable<decimal> pIdB_Receiver, Nullable<StatusCalculEnum> pStatusCalculEnum)
        {
            string sqlUpdate = @"update dbo.EVENT set
            VALORISATION = @VALORISATION, IDA_PAY= @IDA_PAY, IDB_PAY = @IDB_PAY, IDA_REC= @IDA_REC, IDB_REC = @IDB_REC, IDSTCALCUL = @IDSTCALCUL
            where IDE = @IDE";

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDE", DbType.Int32), pIdE);
            parameters.Add(new DataParameter(CS, "IDA_PAY", DbType.Int32));
            parameters.Add(new DataParameter(CS, "IDB_PAY", DbType.Int32));
            parameters.Add(new DataParameter(CS, "IDA_REC", DbType.Int32));
            parameters.Add(new DataParameter(CS, "IDB_REC", DbType.Int32));
            parameters.Add(new DataParameter(CS, "VALORISATION", DbType.Decimal));
            parameters.Add(new DataParameter(CS, "VALORISATIONSYS", DbType.Decimal));
            parameters.Add(new DataParameter(CS, "IDSTCALCUL", DbType.AnsiString, SQLCst.UT_STATUS_LEN));

            parameters.SetAllDBNull();

            parameters["IDE"].Value = pIdE;
            if (pIdA_Payer.HasValue) parameters["IDA_PAY"].Value = pIdA_Payer;
            if (pIdB_Payer.HasValue) parameters["IDB_PAY"].Value = pIdB_Payer;
            if (pIdA_Receiver.HasValue) parameters["IDA_REC"].Value = pIdA_Receiver;
            if (pIdB_Receiver.HasValue) parameters["IDB_REC"].Value = pIdB_Receiver;
            if (pAmount.HasValue)
            {
                parameters["VALORISATION"].Value = Math.Abs(pAmount.Value);
                parameters["VALORISATIONSYS"].Value = Math.Abs(pAmount.Value);
            }
            if (pStatusCalculEnum.HasValue) parameters["IDSTCALCUL"].Value = pStatusCalculEnum;


            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateEvent_Amount
        #region UpdateEventDet_Closing
        // EG 20150129 [20726] New
        public Cst.ErrLevel UpdateEventDet_Closing(string pCS, IDbTransaction pDbTransaction, int pIdE, Nullable<decimal> pQuotePrice, Nullable<decimal> pQuotePrice100, Nullable<decimal> pQuoteDelta)
        {
            string sqlUpdate = @"update dbo.EVENTDET set 
            QUOTEPRICE = @QUOTEPRICE, QUOTEPRICE100 = @QUOTEPRICE100, QUOTEDELTA = @QUOTEDELTA
            where IDE = @IDE";

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDE", DbType.Int32));
            parameters.Add(new DataParameter(CS, "QUOTEPRICE", DbType.Decimal));
            parameters.Add(new DataParameter(CS, "QUOTEPRICE100", DbType.Decimal));
            parameters.Add(new DataParameter(CS, "QUOTEDELTA", DbType.Decimal));
            parameters.SetAllDBNull();

            parameters["IDE"].Value = pIdE;
            if (pQuotePrice.HasValue) parameters["QUOTEPRICE"].Value = pQuotePrice;
            if (pQuotePrice100.HasValue) parameters["QUOTEPRICE100"].Value = pQuotePrice100;
            if (pQuoteDelta.HasValue) parameters["QUOTEDELTA"].Value = pQuoteDelta;

            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateEventDet_Closing
        #region UpdateEventDet_Denouement
        // EG 20140116 [19456]
        public Cst.ErrLevel UpdateEventDet_Denouement(string pCS, IDbTransaction pDbTransaction, int pIdE,
            Nullable<decimal> pStrikePrice, Nullable<QuotationSideEnum> pSettltQuoteSide, Nullable<QuoteTimingEnum> pSettltQuoteTiming,
            Nullable<DateTime> pDtSettltPrice, Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100)
        {

            string sqlUpdate = @"update dbo.EVENTDET set 
            STRIKEPRICE = @STRIKEPRICE, SETTLTQUOTESIDE = @SETTLTQUOTESIDE, SETTLTQUOTETIMING = @SETTLTQUOTETIMING, 
            DTSETTLTPRICE = @DTSETTLTPRICE, SETTLTPRICE = @SETTLTPRICE, SETTLTPRICE100 = @SETTLTPRICE100
            where IDE = @IDE";

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDE", DbType.Int32));
            parameters.Add(new DataParameter(CS, "STRIKEPRICE", DbType.Decimal));
            parameters.Add(new DataParameter(CS, "SETTLTQUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            parameters.Add(new DataParameter(CS, "SETTLTQUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            parameters.Add(new DataParameter(CS, "DTSETTLTPRICE", DbType.DateTime));
            parameters.Add(new DataParameter(CS, "SETTLTPRICE", DbType.Decimal));
            parameters.Add(new DataParameter(CS, "SETTLTPRICE100", DbType.Decimal));
            parameters.SetAllDBNull();

            parameters["IDE"].Value = pIdE;
            if (pStrikePrice.HasValue) parameters["STRIKEPRICE"].Value = pStrikePrice;
            if (pSettltQuoteSide.HasValue) parameters["SETTLTQUOTESIDE"].Value = pSettltQuoteSide;
            if (pSettltQuoteTiming.HasValue) parameters["SETTLTQUOTETIMING"].Value = pSettltQuoteTiming;
            if (pDtSettltPrice.HasValue) parameters["DTSETTLTPRICE"].Value = pDtSettltPrice;
            if (pSettltPrice.HasValue) parameters["SETTLTPRICE"].Value = pSettltPrice;
            if (pSettltPrice100.HasValue) parameters["SETTLTPRICE100"].Value = pSettltPrice100;

            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateEventDet_Denouement

        /* ---------------------------------------------------------------------- */
        /* Utilisées pour le recalcul des frais et calcul des frais manquants     */
        /* ---------------------------------------------------------------------- */

        #region DeleteFeeEvents
        /// <summary>
        /// Suppression des lignes de frais
        /// Utilisé dans :
        /// le recalcul des frais (ActionGen.FeeCalculation)
        /// le calcul des frais manquants (PosKeepingGenProcessBase.FeesCalculationGen)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// EG 20130703 New Mise en commun
        public void DeleteFeeEvents(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            string sqlDelete = @"delete from dbo.EVENT
            where (IDT = @IDT) and (EVENTCODE = @EVENTCODE);
            delete from dbo.EVENT
            where (IDT = @IDT) and (IDE_EVENT <> 0) and (IDE_EVENT not in (select IDE from dbo.EVENT where (IDT = @IDT)));" + Cst.CrLf;

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.OtherPartyPayment);
            
            QueryParameters qryParameters = new QueryParameters(CS, sqlDelete, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
        }
        #endregion DeleteFeeEvents
        #region DeleteSafekeepingEvent
        /// <summary>
        /// Suppression de la ligne de frais de garde de la journée de bourse en cours (pDtEvent = DTBUSINESS)
        /// Utilisé dans : Safekeeping process
        /// EG 20150708 [21103] New
        public void DeleteSafekeepingEvent(string pCS, IDbTransaction pDbTransaction, int pIdT, DateTime pDtEvent)
        {
            string sqlDelete = @"delete from dbo.EVENT
            where (IDT = @IDT) and (EVENTCODE = @EVENTCODE) and (IDE in (
            select ev.IDE 
            from dbo.EVENT ev
            inner join dbo.EVENT evp on (evp.IDE = ev.IDE_EVENT)
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTEVENT)
            where (evp.EVENTCODE = 'LPC') and (evp.EVENTTYPE = 'AMT')))" + Cst.CrLf;

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.SafeKeepingPayment);
            parameters.Add(new DataParameter(pCS, "DTEVENT", DbType.DateTime), pDtEvent);

            QueryParameters qryParameters = new QueryParameters(CS, sqlDelete, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
        }
        #endregion DeleteSafekeepingEvent
        
        #region InsertFeeEvents
        /// <summary>
        /// Insertion des lignes de frais
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pProduct"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pIdT"></param>
        /// <param name="pDtMarket"></param>
        /// <param name="pIdE_Event"></param>
        /// <param name="paPayments"></param>
        /// <returns></returns>
        // EG 20150306 [POC-BERKELEY] : Refactoring
        // EG 2015124 [POC-MUREX] 
        public Cst.ErrLevel DEPRECATED_InsertFeeEvents(string pCS, IDbTransaction pDbTransaction, IProduct pProduct, DataDocumentContainer pDataDocument, 
            int pIdT, DateTime pDtMarket, int pIdE_Event, ArrayList paPayments)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            RptSideProductContainer container = null;
            if (pProduct is IExchangeTradedDerivative)
            {
                IExchangeTradedDerivative etd = pProduct as IExchangeTradedDerivative;
                // EG 2015124 [POC-MUREX] Add pDbTransaction
                container = new ExchangeTradedDerivativeContainer(pCS, pDbTransaction, etd, pDataDocument);
                ((ExchangeTradedDerivativeContainer)container).efs_ExchangeTradedDerivative =
                    new EFS_ExchangeTradedDerivative(pCS, pDbTransaction, etd, pDataDocument, Cst.StatusBusiness.ALLOC, pIdT);
            }
            else if (pProduct is IEquitySecurityTransaction)
            {
                IEquitySecurityTransaction eqs = pProduct as IEquitySecurityTransaction;
                // EG 2015124 [POC-MUREX] Add pDbTransaction
                container = new EquitySecurityTransactionContainer(pCS, pDbTransaction, eqs, pDataDocument);
                ((EquitySecurityTransactionContainer)container).efs_EquitySecurityTransaction = new EFS_EquitySecurityTransaction(pCS, pDbTransaction, eqs,
                    pDataDocument, Cst.StatusBusiness.ALLOC);
            }
            else if (pProduct is IReturnSwap)
            {
            }
            /*
            IExchangeTradedDerivative etd = pProduct as IExchangeTradedDerivative;
            ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer = new ExchangeTradedDerivativeContainer(pCS, etd, pDataDocument);
            exchangeTradedDerivativeContainer.efs_ExchangeTradedDerivative =
                new EFS_ExchangeTradedDerivative(pCS, exchangeTradedDerivativeContainer.ExchangeTradedDerivative, pDataDocument, Cst.StatusBusiness.ALLOC, pIdT);
            */
            IPayment[] payments = (IPayment[])paPayments.ToArray(typeof(IPayment));

            int nbEvent = 0;
            // Détermination du nombre d'IDE en fonction des  événements potentiel de taxes...
            InitPaymentForEvent(pCS, payments, pDataDocument, out nbEvent);
            int newIdE = 0;
            SQLUP.GetId(out newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);
            // Ecriture des événements de frais
            foreach (IPayment payment in payments)
            {
                // EG 20141027 Initialisation des efs_Payment si null)
                // EG 2015124 [POC-MUREX] Replace pDbTransaction.Connection.ConnectionString by pCS
                if (null == payment.efs_Payment)
                    payment.efs_Payment = new EFS_Payment(pCS, payment, pDataDocument.currentProduct.product);

                codeReturn = InsertPaymentEvents(pDbTransaction, pDataDocument, pIdT, payment, pDtMarket, 0, 0, ref newIdE, pIdE_Event);
                if (Cst.ErrLevel.SUCCESS != codeReturn)
                    break;

                newIdE++;
            }
            return codeReturn;
        }
        /// <summary>
        /// Insertion des lignes de frais
        /// </summary>
        // EG 20160116 [POC-MUREX] New
        public Cst.ErrLevel InsertFeeEvents(string pCS, IDbTransaction pDbTransaction, IProduct pProduct, DataDocumentContainer pDataDocument,
            int pIdT, DateTime pDtMarket, int pIdE_Event, IPayment[] pPayments, int newIdE)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            // Ecriture des événements de frais
            foreach (IPayment payment in pPayments)
            {
                if (null == payment.efs_Payment)
                    payment.efs_Payment = new EFS_Payment(pCS, payment, pDataDocument.currentProduct.product);
                codeReturn = InsertPaymentEvents(pDbTransaction, pDataDocument, pIdT, payment, pDtMarket, 0, 0, ref newIdE, pIdE_Event);
                if (Cst.ErrLevel.SUCCESS != codeReturn)
                    break;
                newIdE++;
            }
            return codeReturn;
        }
        #endregion InsertFeeEvents
        #region PrepareFeeEvents
        /// <summary>
        /// Préparation des lignes de paiements avant insertion des événements
        /// </summary>
        /// <returns></returns>
        // EG 20160115 [POC-MUREX] New
        public IPayment[] PrepareFeeEvents(string pCS, IProduct pProduct, DataDocumentContainer pDataDocument, int pIdT, ArrayList paPayments, ref int pNbEvent)
        {
            RptSideProductContainer container = null;
            if (pProduct is IExchangeTradedDerivative)
            {
                IExchangeTradedDerivative etd = pProduct as IExchangeTradedDerivative;
                container = new ExchangeTradedDerivativeContainer(pCS, etd, pDataDocument);
                ((ExchangeTradedDerivativeContainer)container).efs_ExchangeTradedDerivative =
                    new EFS_ExchangeTradedDerivative(pCS, etd, pDataDocument, Cst.StatusBusiness.ALLOC, pIdT);
            }
            else if (pProduct is IEquitySecurityTransaction)
            {
                IEquitySecurityTransaction eqs = pProduct as IEquitySecurityTransaction;
                container = new EquitySecurityTransactionContainer(pCS, eqs, pDataDocument);
                ((EquitySecurityTransactionContainer)container).efs_EquitySecurityTransaction = new EFS_EquitySecurityTransaction(pCS, eqs, pDataDocument, Cst.StatusBusiness.ALLOC);
            }
            // EG 20160121 Add DebtSecurityTransaction
            else if (pProduct is IDebtSecurityTransaction)
            {
                IDebtSecurityTransaction dst = pProduct as IDebtSecurityTransaction;
                container = new DebtSecurityTransactionContainer(pCS, dst, pDataDocument);
                ((DebtSecurityTransactionContainer)container).debtSecurityTransaction.SetStreams(pCS, pDataDocument, Cst.StatusBusiness.ALLOC);
            }
            // Détermination du nombre d'IDE en fonction des  événements potentiels de taxes...
            IPayment[] payments = (IPayment[])paPayments.ToArray(typeof(IPayment));
            InitPaymentForEvent(pCS, payments, pDataDocument, out pNbEvent);
            return payments;
        }
        #endregion PrepareFeeEvents
        #region InsertPaymentEvents
        /// <summary>
        /// Ajoute ds la table EVENT les enregistrements associé à {pPayment}
        /// <para>pDataDocument doit être correctement alimenté</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPayment"></param>
        /// <param name="pIdE">Nouvelle valeur du jeton EVENT</param>
        /// <param name="pIdEParent">Evènement parent</param>
        /// <returns></returns>
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150706 [21021] Nullable Payer|Receiver (Actor|Book)
        // EG 20150708 [21103] New
        public Cst.ErrLevel InsertPaymentEvents(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument,
            int pIdT, IPayment pPayment, DateTime pDtBusiness, int pInstrumentNo, int pStreamNo, ref int pIdE, int pIdEParent)
        {
            return InsertPaymentEvents(pDbTransaction, pDataDocument, pIdT, pPayment, EventCodeFunc.OtherPartyPayment, pDtBusiness, pInstrumentNo, pStreamNo, ref pIdE, pIdEParent);
        }
        /// <summary>
        /// Ajout d'une ligne de paiement matérialisant les frais de garde (SKP) ou les frais classique (OPP|ADP)
        /// </summary>
        /// EG 20150708 [21103] New Add pEventCode
        public Cst.ErrLevel InsertPaymentEvents(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument,
            int pIdT, IPayment pPayment, string pEventCode, DateTime pDtBusiness, int pInstrumentNo, int pStreamNo, ref int pIdE, int pIdEParent)
        {

            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            #region Variables
            EFS_Payment efs_Payment = pPayment.efs_Payment;
            int idE = pIdE;
            int idE_Event = pIdEParent;

            // EG 20150706 [21021]
            Nullable<int> idA_Payer = pDataDocument.GetOTCmlId_Party(pPayment.payerPartyReference.hRef);
            Nullable<int> idB_Payer = pDataDocument.GetOTCmlId_Book(pPayment.payerPartyReference.hRef);
            Nullable<int> idA_Receiver = pDataDocument.GetOTCmlId_Party(pPayment.receiverPartyReference.hRef);
            Nullable<int> idB_Receiver = pDataDocument.GetOTCmlId_Book(pPayment.receiverPartyReference.hRef);

            // EG 20150708 [21103] pEventCode = [OPP|ADP|SKP]
            string eventType = pPayment.GetPaymentType(pEventCode);
            EFS_EventDate dtExpirationDate = efs_Payment.ExpirationDate;
            EFS_EventDate dtPaymentDate = efs_Payment.PaymentDate;
            EFS_Date dtAdjustedPaymentDate = efs_Payment.AdjustedPaymentDate;
            EFS_Date dtPreSettlementDate = null;
            if (efs_Payment.preSettlementSpecified)
                dtPreSettlementDate = efs_Payment.preSettlement.AdjustedPreSettlementDate;
            EFS_Date dtInvoicingDate = efs_Payment.Invoicing_AdjustedPaymentDate;
            decimal paymentAmount = efs_Payment.paymentAmount.amount.DecValue;
            string currency = efs_Payment.paymentAmount.currency;

            #endregion Variables

            #region EVENT PAYMENT
            // EG 20120217 DTPAYMENDATE = DTSTART = DTEND
            // EG 20150708 [21103] Upd efs_Payment.dtEventStartPeriod|efs_Payment.dtEventEndPeriod
            codeReturn = InsertEvent(pDbTransaction, pIdT, idE, idE_Event, null, pInstrumentNo, pStreamNo, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                pEventCode, eventType,
                efs_Payment.dtEventStartPeriod.First, efs_Payment.dtEventStartPeriod.Second,
                efs_Payment.dtEventEndPeriod.First, efs_Payment.dtEventEndPeriod.Second,
                paymentAmount, currency, UnitTypeEnum.Currency.ToString(), null, null);

            #region EVENTCLASS
            // EG 20120220 RECOGNITION = DTBUSINESS
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtPaymentDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Recognition, pDtBusiness, false);

            // EG 20150616 [21124]
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtPaymentDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.ValueDate, 
                    pDataDocument.currentProduct.isExchangeTradedDerivative ? pDtBusiness : dtAdjustedPaymentDate.DateValue, false);

            if ((Cst.ErrLevel.SUCCESS == codeReturn) && efs_Payment.preSettlementSpecified)
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.PreSettlement, dtPreSettlementDate.DateValue, false);
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtAdjustedPaymentDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Settlement, dtAdjustedPaymentDate.DateValue, true);
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtInvoicingDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Invoiced, dtInvoicingDate.DateValue, true);
            #endregion EVENTCLASS

            #region EVENTFEE
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && efs_Payment.paymentSourceSpecified)
                codeReturn = InsertEventFee(pDbTransaction, idE, efs_Payment.paymentSource);
            #endregion EVENTFEE

            #endregion EVENT PAYMENT

            idE_Event = idE;
            if (efs_Payment.originalPaymentSpecified)
            {
                #region ORIGINAL PAYMENT
                EFS_OriginalPayment originalPayment = efs_Payment.originalPayment;
                dtExpirationDate = originalPayment.ExpirationDate;
                dtPaymentDate = originalPayment.PaymentDate;
                paymentAmount = originalPayment.PaymentAmount.DecValue;
                currency = originalPayment.PaymentCurrency;
                idE++;
                // EG 20150708 [SKP]
                // EG 20150708 [SKP] Upd originalPayment.dtEventStartPeriod|originalPayment.dtEventEndPeriod
                codeReturn = InsertEvent(pDbTransaction, pIdT, idE, idE_Event, null, pInstrumentNo, pStreamNo, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                    pEventCode, eventType,
                    originalPayment.dtEventStartPeriod.First, originalPayment.dtEventStartPeriod.Second,
                    originalPayment.dtEventEndPeriod.First, originalPayment.dtEventEndPeriod.Second,
                    paymentAmount, currency, UnitTypeEnum.Currency.ToString(), null, null);
                #region EVENTCLASS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.GroupLevel, dtPaymentDate.adjustedDate.DateValue, false);
                #endregion EVENTCLASS
                #endregion ORIGINAL PAYMENT
            }
            if (efs_Payment.taxSpecified)
            {
                #region TAX
                foreach (EFS_Tax tax in efs_Payment.tax)
                {
                    idE++;
                    dtExpirationDate = tax.ExpirationDate;
                    dtPaymentDate = tax.PaymentDate;

                    paymentAmount = tax.TaxAmount.DecValue;
                    currency = tax.TaxCurrency;
                    // EG 20150708 [SKP]
                    // EG 20150708 [SKP] Upd tax.dtEventStartPeriod|tax.dtEventEndPeriod
                    codeReturn = InsertEvent(pDbTransaction, pIdT, idE, idE_Event, null, pInstrumentNo, pStreamNo, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                        pEventCode, tax.EventType,
                        tax.dtEventStartPeriod.First, tax.dtEventStartPeriod.Second,
                        tax.dtEventEndPeriod.First, tax.dtEventEndPeriod.Second,
                        paymentAmount, currency, UnitTypeEnum.Currency.ToString(), null, null);
                    #region EVENTCLASS
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Recognition, pDtBusiness, false);

                    // EG 20150616 [21124]
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.ValueDate, 
                            pDataDocument.currentProduct.isExchangeTradedDerivative ? pDtBusiness : dtAdjustedPaymentDate.DateValue, false);

                    if ((Cst.ErrLevel.SUCCESS == codeReturn) && efs_Payment.preSettlementSpecified)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.PreSettlement, dtPreSettlementDate.DateValue, false);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Settlement, dtPaymentDate.adjustedDate.DateValue, true);
                    #endregion EVENTCLASS

                    #region EVENTFEE
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventFee(pDbTransaction, idE, tax.taxSource);
                    #endregion EVENTFEE

                }
                #endregion TAX
            }
            pIdE = idE;
            return codeReturn;
        }
        #endregion InsertPaymentEvents

        #region UpdateTradeXMLForFees
        /// <summary>
        /// Mise à jour du trade XML avec les frais
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pDataDocument">DatadocumentContainer</param>
        /// <param name="paManualPayments">Paiements manuels</param>
        /// <param name="paAutoPayments">Paiements automatiques</param>
        /// <returns></returns>
        public Cst.ErrLevel UpdateTradeXMLForFees(IDbTransaction pDbTransaction, int pIdT, DataDocumentContainer pDataDocument, ArrayList paManualPayments, ArrayList paAutoPayments)
        {
            // LISTE DES FRAIS APRES TRAITEMENT DE RECALCUL
            ArrayList aPayments = new ArrayList();
            aPayments.AddRange(paManualPayments);
            aPayments.AddRange(paAutoPayments);
            pDataDocument.currentTrade.SetOtherPartyPayment(aPayments);
            pDataDocument.currentTrade.otherPartyPaymentSpecified = (0 < aPayments.Count);

            EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(pDataDocument.dataDocument);
            StringBuilder sb = CacheSerializer.Serialize(serializerInfo);

            string sqlUpdate = @"update dbo.TRADE set TRADEXML = @TRADEXML where (IDT = @IDT)";

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(CS, "TRADEXML", DbType.Xml), sb.ToString());

            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS; 
        }
        /// EG 20160208 [POC-MUREX] New (La sérialisation du TRADEXML a lieu à priori (résultat dans paramètre :  pTradeXML)
        public Cst.ErrLevel UpdateTradeXMLForFees(IDbTransaction pDbTransaction, int pIdT, StringBuilder pTradeXML)
        {
            // LISTE DES FRAIS APRES TRAITEMENT DE RECALCUL
            string sqlUpdate = @"update dbo.TRADE set TRADEXML = @TRADEXML where (IDT = @IDT)";
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(CS, "TRADEXML", DbType.Xml), pTradeXML.ToString());
            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateTradeXMLForFees


        /* ---------------------------------------------------------------------- */
        /* Utilisées pour le MTM                                                  */
        /* ---------------------------------------------------------------------- */

        #region UpdateEventPricing
        /// <summary>
        /// INSERT|UPDATE EVENTPRICING
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pPricingValues"></param>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        public Cst.ErrLevel UpdateEventPricing(IDbTransaction pDbTransaction, int pIdE, PricingValues pPricingValues, IProduct pProduct)
        {
            InitParameters(Cst.OTCml_TBL.EVENTPRICING);

            m_ParamEventPricing["IDE"].Value = pIdE;

            if (StrFunc.IsFilled(pPricingValues.Currency1))
                m_ParamEventPricing["IDC1"].Value = pPricingValues.Currency1;
            if (StrFunc.IsFilled(pPricingValues.Currency2))
                m_ParamEventPricing["IDC2"].Value = pPricingValues.Currency2;

            if (null != pPricingValues.DayCountFraction)
            {
                m_ParamEventPricing["DCF"].Value = pPricingValues.DayCountFraction.DayCountFraction_FpML;
                m_ParamEventPricing["DCFNUM"].Value = pPricingValues.DayCountFraction.Numerator;
                m_ParamEventPricing["DCFDEN"].Value = pPricingValues.DayCountFraction.Denominator;
                m_ParamEventPricing["TOTALOFYEAR"].Value = pPricingValues.DayCountFraction.NumberOfCalendarYears;
                m_ParamEventPricing["TOTALOFDAY"].Value = pPricingValues.DayCountFraction.TotalNumberOfCalculatedDays;
                m_ParamEventPricing["TIMETOEXPIRATION"].Value = pPricingValues.DayCountFraction.Factor;
            }

            if (pProduct.productBase.IsFxOption)
            {
                #region FxOption
                m_ParamEventPricing["STRIKE"].Value = pPricingValues.StrikeCertain;
                m_ParamEventPricing["VOLATILITY"].Value = pPricingValues.Volatility;
                if (pProduct.productBase.IsFxOptionLeg)
                {
                    m_ParamEventPricing["EXCHANGERATE"].Value = pPricingValues.ForwardPrice.HasValue ? pPricingValues.ForwardPrice.Value : 
                        pPricingValues.UnderlyingPriceCertain.HasValue?pPricingValues.UnderlyingPriceCertain:Convert.DBNull;
                    m_ParamEventPricing["INTERESTRATE1"].Value = pPricingValues.DomesticInterest;
                    m_ParamEventPricing["INTERESTRATE2"].Value = pPricingValues.ForeignInterest;
                }
                else
                {
                    m_ParamEventPricing["UNDERLYINGPRICE"].Value = pPricingValues.UnderlyingPriceCertain;
                    m_ParamEventPricing["DIVIDENDYIELD"].Value = pPricingValues.DividendYield;
                    m_ParamEventPricing["RISKFREEINTEREST"].Value = pPricingValues.RiskFreeInterest;
                }

                #region Call values
                if (0 != pPricingValues.CallPrice)
                    m_ParamEventPricing["CALLPRICE"].Value = pPricingValues.CallPrice;
                if (0 != pPricingValues.CallDelta)
                    m_ParamEventPricing["CALLDELTA"].Value = pPricingValues.CallDelta;
                if (0 != pPricingValues.CallRho1)
                    m_ParamEventPricing["CALLRHO1"].Value = pPricingValues.CallRho1;
                if (0 != pPricingValues.CallRho2)
                    m_ParamEventPricing["CALLRHO2"].Value = pPricingValues.CallRho2;
                if (0 != pPricingValues.CallTheta)
                    m_ParamEventPricing["CALLTHETA"].Value = pPricingValues.CallTheta;
                if (0 != pPricingValues.CallCharm)
                    m_ParamEventPricing["CALLCHARM"].Value = pPricingValues.CallCharm;
                #endregion Call values
                #region Put values
                if (0 != pPricingValues.PutPrice)
                    m_ParamEventPricing["PUTPRICE"].Value = pPricingValues.PutPrice;
                if (0 != pPricingValues.PutDelta)
                    m_ParamEventPricing["PUTDELTA"].Value = pPricingValues.PutDelta;
                if (0 != pPricingValues.PutRho1)
                    m_ParamEventPricing["PUTRHO1"].Value = pPricingValues.PutRho1;
                if (0 != pPricingValues.PutRho2)
                    m_ParamEventPricing["PUTRHO2"].Value = pPricingValues.PutRho2;
                if (0 != pPricingValues.PutTheta)
                    m_ParamEventPricing["PUTTHETA"].Value = pPricingValues.PutTheta;
                if (0 != pPricingValues.PutCharm)
                    m_ParamEventPricing["PUTCHARM"].Value = pPricingValues.PutCharm;
                #endregion Put values
                #region Others values
                if (0 != pPricingValues.Gamma)
                    m_ParamEventPricing["GAMMA"].Value = pPricingValues.Gamma;
                if (0 != pPricingValues.Vega)
                    m_ParamEventPricing["VEGA"].Value = pPricingValues.Vega;
                if (0 != pPricingValues.Color)
                    m_ParamEventPricing["COLOR"].Value = pPricingValues.Color;
                if (0 != pPricingValues.Speed)
                    m_ParamEventPricing["SPEED"].Value = pPricingValues.Speed;
                if (0 != pPricingValues.Vanna)
                    m_ParamEventPricing["VANNA"].Value = pPricingValues.Vanna;
                if (0 != pPricingValues.Volga)
                    m_ParamEventPricing["VOLGA"].Value = pPricingValues.Volga;
                #endregion Others values

                #endregion FxOption
            }
            else if (pProduct.productBase.IsFxLeg)
            {
                #region FxLeg
                if (null != pPricingValues.DayCountFraction2)
                {
                    m_ParamEventPricing["DCF2"].Value = pPricingValues.DayCountFraction2.DayCountFraction_FpML;
                    m_ParamEventPricing["DCFNUM2"].Value = pPricingValues.DayCountFraction2.Numerator;
                    m_ParamEventPricing["DCFDEN2"].Value = pPricingValues.DayCountFraction2.Denominator;
                    m_ParamEventPricing["TOTALOFYEAR2"].Value = pPricingValues.DayCountFraction2.NumberOfCalendarYears;
                    m_ParamEventPricing["TOTALOFDAY2"].Value = pPricingValues.DayCountFraction2.TotalNumberOfCalculatedDays;
                    m_ParamEventPricing["TIMETOEXPIRATION2"].Value = pPricingValues.DayCountFraction2.Factor;
                }

                m_ParamEventPricing["EXCHANGERATE"].Value = pPricingValues.ForwardPrice.HasValue ? pPricingValues.ForwardPrice .Value: Convert.DBNull;
                m_ParamEventPricing["INTERESTRATE1"].Value = pPricingValues.InterestRate1.HasValue ? pPricingValues.InterestRate1.Value : Convert.DBNull;
                m_ParamEventPricing["INTERESTRATE2"].Value = pPricingValues.InterestRate2.HasValue ? pPricingValues.InterestRate2.Value : Convert.DBNull;
                m_ParamEventPricing["SPOTRATE"].Value = pPricingValues.SpotRate.HasValue ? pPricingValues.SpotRate.Value : Convert.DBNull;

                #endregion FxLeg
            }

            int count = 0;
            string sqlSelect = @"select  count(*)  from dbo.EVENTPRICING where (IDE = @IDE)";
            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDE", DbType.Int32), pIdE);
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            if (null != obj)
                count = Convert.ToInt32(obj);

            string sqlInsertUpdate = string.Empty;
            if (0 == count)
                sqlInsertUpdate = QueryLibraryTools.GetQueryInsert(Cst.OTCml_TBL.EVENTPRICING);
            else
                sqlInsertUpdate = QueryLibraryTools.GetQueryUpdate(Cst.OTCml_TBL.EVENTPRICING);

            qryParameters = new QueryParameters(CS, sqlInsertUpdate, m_ParamEventPricing);
            int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion WriteEventPricing
        #endregion Methods
    }
    #endregion EventQuery
}

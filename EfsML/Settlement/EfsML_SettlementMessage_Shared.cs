#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Xml;
#endregion Using Directives

namespace EfsML.Settlement.Message
{

    #region SettlementMessageTools
    public class SettlementMessageTools
    {
        #region Constructor
        public SettlementMessageTools() { }
        #endregion
        #region Methods
        #region IsEventUseByMSO
        /// <summary>
        ///  IDE fait-il parti d'un message envoyé?  
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIde"></param>
        /// <returns></returns>
        public static bool IsEventUseByMSO(string pSource, int pIde)
        {
            DataParameter param = DataParameter.GetParameter(pSource, DataParameter.ParameterEnum.IDE);
            param.Value = pIde;
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT_DISTINCT + "1 As EXISTMSO" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MSO + " mso" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MSODET + " msodet on msodet.IDMSO = mso.IDMSO" + Cst.CrLf;
            sqlQuery += OTCmlHelper.GetSQLJoin(pSource, Cst.OTCml_TBL.ESR, true, "msodet.IDESR", "esr");
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ESRDET + " esrdet on esrdet.IDESR = esr.IDESR" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "esrdet.IDE=@IDE";

            object obj = DataHelper.ExecuteScalar(pSource, CommandType.Text, sqlQuery.ToString(), param.DbDataParameter);
            return (null != obj);
        }
        #endregion IsEventUseByMSO
        #region GetEventInEsr
        // EG 20180426 Analyse du code Correction [CA2202]
        public static int[] GetEventInEsr(string pSource, int pIdEsr)
        {
            int[] ret = null;

            ArrayList alist = new ArrayList();

            DataParameter param = DataParameter.GetParameter(pSource, DataParameter.ParameterEnum.ID);
            param.Value = pIdEsr;

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "esrdet.IDE" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ESRDET + " esrdet" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "esrdet.IDESR=@ID";
            
            using (IDataReader dr = DataHelper.ExecuteReader(pSource, CommandType.Text, sqlQuery.ToString(), param.DbDataParameter))
            {
                while (dr.Read())
                {
                    alist.Add(Convert.ToInt32(dr["IDE"]));
                }
            }
            if (ArrFunc.IsFilled(alist))
                ret = (int[])alist.ToArray(typeof(int));

            return ret;
        }
        #endregion GetEventInEsr
        #region GetEsrVersion
        public static EfsMLDocumentVersionEnum GetEsrVersion(string pCS, int pIdEsr)
        {

            EfsML.Enum.EfsMLDocumentVersionEnum ret = EfsMLDocumentVersionEnum.Version20;
            int[] ide = SettlementMessageTools.GetEventInEsr(pCS, pIdEsr);
            if (ArrFunc.IsFilled(ide))
            {
                SQL_Event sqlEvent = new SQL_Event(pCS, ide[0]);
                sqlEvent.LoadTable(new string[] { "IDE,IDT" });
                if (sqlEvent.IsLoaded)
                    ret = TradeRDBMSTools.GetTradeVersion(pCS, sqlEvent.IdT);
            }
            return ret;

        }
        #endregion
        #endregion Methods
    }
    #endregion

    #region DatasetSettlementMessage
    public class DatasetSettlementMessage
    {
        #region Members
        private readonly string _cs;
        private readonly DateTime _date;
        private DataSet _ds;
        private readonly LoadDataSetSettlementMessage _loadTypeEnum;
        private readonly bool _isModeSimulation;
        #endregion Members
        #region Accessors
        public DataTable DtESR
        {
            get { return _ds.Tables["ESR"]; }
        }
        public DataTable DtESRDET
        {
            get { return _ds.Tables["ESRDET"]; }
        }
        public DataTable DtMSO
        {
            get { return _ds.Tables["MSO"]; }
        }
        public DataTable DtMSODET
        {
            get { return _ds.Tables["MSODET"]; }
        }
        public string Cs
        {
            get { return _cs; }
        }
        public DateTime Date
        {
            get { return _date; }
        }

        #region ChildESR_MSODET
        public DataRelation ChildESR_MSODET
        {
            get { return _ds.Relations["ESR_MSODET"]; }
        }
        #endregion
        #region ChildESR_ESRDET
        public DataRelation ChildESR_ESRDET
        {
            get { return _ds.Relations["ESR_ESRDET"]; }
        }
        #endregion
        #endregion Accessors
        #region Constructors
        public DatasetSettlementMessage(string pCs, DateTime pDate, LoadDataSetSettlementMessage pTypeLoad, bool pIsModeSimulation)
        {
            _date = pDate;
            _cs = pCs;
            _loadTypeEnum = pTypeLoad;
            _isModeSimulation = pIsModeSimulation;
        }
        #endregion Constructors
        #region Methods
        #region AddRowESR
        public void AddRowESR(EventSettlementReportKey pKey, PayerReceiverEnum pSide, Decimal pAmount, int pIdAIns)
        {

            DataRow row = DtESR.NewRow();
            //
            row["DTSTM"] = pKey.dtSTM;
            row["DTSTMFORCED"] = pKey.dtSTMForced;
            row["IDA_SENDERPARTY"] = pKey.idASenderParty;
            row["IDA_SENDER"] = pKey.idASender;
            row["IDA_RECEIVER"] = pKey.idAReceiver;
            row["IDA_RECEIVERPARTY"] = pKey.idAReceiverParty;
            row["IDA_CSS"] = pKey.idACss;
            row["SCREF"] = pKey.settlementChainRef;
            row["IDC"] = pKey.idC;
            row["NETMETHOD"] = pKey.netMethod;
            row["IDNETCONVENTION"] = (pKey.idNetConvention > 0) ? pKey.idNetConvention : Convert.DBNull;
            row["IDNETDESIGNATION"] = (pKey.idNetDesignation > 0) ? pKey.idNetDesignation : Convert.DBNull;
            //
            row["PAYER_RECEIVER"] = pSide.ToString();
            row["AMOUNT"] = pAmount;
            //
            row["IDT"] = (pKey.idT > 0) ? pKey.idT : Convert.DBNull;
            // FI 20200820 [25468] Dates systemes en UTC
            row["DTINS"] = OTCmlHelper.GetDateSysUTC(_cs);
            row["IDAINS"] = pIdAIns;
            row["DTUPD"] = Convert.DBNull;
            row["IDAUPD"] = Convert.DBNull;
            //
            row["EXTLLINK"] = "NEW";
            DtESR.Rows.Add(row);

        }
        #endregion AddRowESR
        #region AddRowESRDET
        public void AddRowESRDET(int pIdEsr, int pIdE, int pIdAIns)
        {
            DataRow row = DtESRDET.NewRow();
            //
            row["ID"] = pIdEsr;
            row["IDE"] = pIdE;
            // FI 20200820 [25468] Dates systemes en UTC
            row["DTINS"] = OTCmlHelper.GetDateSysUTC(Cs);
            row["IDAINS"] = pIdAIns;
            //
            row["DTUPD"] = Convert.DBNull;
            row["IDAUPD"] = Convert.DBNull;
            //
            DtESRDET.Rows.Add(row);

        }
        #endregion AddRowESRDET
        #region AddRowMSO
        public void AddRowMSO(int pIdMSO, string pStlMsgXML, int pIdStlMessage, string pPathFileXslt, string pStlMsgMSO, int pIdAIns)
        {

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(pPathFileXslt);
            //
            XMLTools.RemoveXmlDeclaration(xmldoc);
            //
            DataRow row = DtMSO.NewRow();
            //
            row["ID"] = pIdMSO;
            row["DTMSO"] = _date;
            row["STLMSGXML"] = pStlMsgXML;
            row["IDSTLMESSAGE"] = pIdStlMessage;
            row["STLMSGXSLT"] = xmldoc.InnerXml;
            row["STLMSO"] = pStlMsgMSO;
            // FI 20200820 [25468] Dates systemes en UTC
            row["DTINS"] = OTCmlHelper.GetDateSysUTC(_cs);
            row["IDAINS"] = pIdAIns;
            row["DTUPD"] = Convert.DBNull;
            row["IDAUPD"] = Convert.DBNull;
            //
            row["EXTLLINK"] = Convert.DBNull;
            DtMSO.Rows.Add(row);

        }
        #endregion AddRowMSO
        #region AddRowMSODET
        public void AddRowMSODET(int pIdMSO, int pIdESR, int pIdAIns)
        {
            DataRow row = DtMSODET.NewRow();
            //
            row["ID"] = pIdMSO;
            row["IDESR"] = pIdESR;
            // FI 20200820 [25468] Dates systemes en UTC
            row["DTINS"] = OTCmlHelper.GetDateSysUTC(_cs);
            row["IDAINS"] = pIdAIns;
            //
            row["DTUPD"] = Convert.DBNull;
            row["IDAUPD"] = Convert.DBNull;
            //
            DtMSODET.Rows.Add(row);

        }
        #endregion AddRowMSODET

        #region GetSelectEsrColumn
        private string GetSelectEsrColumn()
        {

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + @"esr.IDESR as ID,esr.DTSTM,esr.DTESR as DTSTMFORCED," + Cst.CrLf;
            sqlSelect += "esr.IDA_SENDERPARTY,esr.IDA_SENDER,esr.IDA_RECEIVER,esr.IDA_RECEIVERPARTY,esr.IDA_CSS," + Cst.CrLf;
            sqlSelect += "esr.NETMETHOD,esr.IDNETCONVENTION,esr.IDNETDESIGNATION,esr.SCREF," + Cst.CrLf;
            sqlSelect += "esr.IDC,esr.AMOUNT,esr.PAYER_RECEIVER," + Cst.CrLf;
            sqlSelect += "esr.IDT," + Cst.CrLf;
            sqlSelect += "esr.DTUPD,esr.IDAUPD,esr.DTINS,esr.IDAINS," + Cst.CrLf;
            sqlSelect += "esr.EXTLLINK,esr.ROWATTRIBUT" + Cst.CrLf;
            //
            string tbl = _isModeSimulation ? Cst.OTCml_TBL.ESR_T.ToString() : Cst.OTCml_TBL.ESR.ToString();
            sqlSelect += SQLCst.FROM_DBO + tbl + " esr " + Cst.CrLf;
            //				
            return sqlSelect.ToString();

        }
        #endregion GetSelectEsrColumn
        #region GetSelectEsrDetColumn
        private string GetSelectEsrDetColumn()
        {

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + @"esrd.IDESR As ID,esrd.IDE," + Cst.CrLf;
            sqlSelect += "esrd.DTUPD,esrd.IDAUPD,esrd.DTINS,esrd.IDAINS," + Cst.CrLf;
            sqlSelect += "esrd.EXTLLINK,esrd.ROWATTRIBUT" + Cst.CrLf;
            //
            string tbl = _isModeSimulation ? Cst.OTCml_TBL.ESRDET_T.ToString() : Cst.OTCml_TBL.ESRDET.ToString();
            sqlSelect += SQLCst.FROM_DBO + tbl + " esrd " + Cst.CrLf;
            //				
            return sqlSelect.ToString();

        }
        #endregion GetSelectEsrDetColumn
        #region GetSelectMsoColumn
        private string GetSelectMsoColumn()
        {

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + @"mso.IDMSO as ID,mso.DTMSO," + Cst.CrLf;
            sqlSelect += "mso.STLMSGXML," + Cst.CrLf;
            sqlSelect += "mso.IDSTLMESSAGE,mso.STLMSGXSLT," + Cst.CrLf;
            sqlSelect += "mso.STLMSO," + Cst.CrLf;
            sqlSelect += "mso.DTUPD,mso.IDAUPD,mso.DTINS,mso.IDAINS," + Cst.CrLf;
            sqlSelect += "mso.EXTLLINK,mso.ROWATTRIBUT" + Cst.CrLf;
            //
            string tbl = _isModeSimulation ? Cst.OTCml_TBL.MSO_T.ToString() : Cst.OTCml_TBL.MSO.ToString();
            sqlSelect += SQLCst.FROM_DBO + tbl + " mso " + Cst.CrLf;
            //				
            return sqlSelect.ToString();

        }
        #endregion GetSelectMsoColumn
        #region GetSelectMsoDetColumn
        private string GetSelectMsoDetColumn()
        {

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + @"msod.IDMSO As ID,msod.IDESR," + Cst.CrLf;
            sqlSelect += "msod.DTUPD,msod.IDAUPD,msod.DTINS,msod.IDAINS," + Cst.CrLf;
            sqlSelect += "msod.EXTLLINK,msod.ROWATTRIBUT" + Cst.CrLf;
            //
            string tbl = _isModeSimulation ? Cst.OTCml_TBL.MSODET_T.ToString() : Cst.OTCml_TBL.MSODET.ToString();
            sqlSelect += SQLCst.FROM_DBO + tbl + " msod" + Cst.CrLf;
            //				
            return sqlSelect.ToString();

        }
        #endregion GetSelectMsoDetColumn

        #region InitializeDs
        private void InitializeDs()
        {
            DataTable dt;
            if ((_loadTypeEnum == LoadDataSetSettlementMessage.LoadESR) || (_loadTypeEnum == LoadDataSetSettlementMessage.LoadESRAndMSO))
            {
                dt = _ds.Tables[0];
                dt.TableName = "ESR";

                dt = _ds.Tables[1];
                dt.TableName = "ESRDET";

                //Relations
                DataRelation rel_Esr_EsrDet = new DataRelation("ESR_ESRDET", DtESR.Columns["ID"],
                    DtESRDET.Columns["ID"], false);
                _ds.Relations.Add(rel_Esr_EsrDet);
            }

            if ((_loadTypeEnum == LoadDataSetSettlementMessage.LoadMSO) || (_loadTypeEnum == LoadDataSetSettlementMessage.LoadESRAndMSO))
            {
                int indexTblMso = (_loadTypeEnum == LoadDataSetSettlementMessage.LoadMSO) ? 0 : 2;
                int indexTblMsoDet = indexTblMso + 1;

                dt = _ds.Tables[indexTblMso];
                dt.TableName = "MSO";

                //
                dt = _ds.Tables[indexTblMsoDet];
                dt.TableName = "MSODET";

                //Relations
                DataRelation rel_Mso_MsoDet = new DataRelation("MSO_MSODET", DtMSO.Columns["ID"], DtMSODET.Columns["ID"], false);
                DataRelation rel_Esr_MsoDet = new DataRelation("ESR_MSODET", DtESR.Columns["ID"], DtMSODET.Columns["IDESR"], false);
                //					
                _ds.Relations.Add(rel_Esr_MsoDet);
                _ds.Relations.Add(rel_Mso_MsoDet);
            }


        }
        #endregion InitializeDs
        #region LoadDs
        public void LoadDs()
        {
            LoadDs(null);
        }
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        public void LoadDs(IDbTransaction pDbTransaction)
        {

            DataParameter paramDate = new DataParameter(_cs, "DT", DbType.Date)
            {
                Value = _date
            }; // FI 20201006 [XXXXX] DbType.Date
            //
            string sqlSelect = string.Empty;
            if ((_loadTypeEnum == LoadDataSetSettlementMessage.LoadESR) || (_loadTypeEnum == LoadDataSetSettlementMessage.LoadESRAndMSO))
            {
                StrBuilder sqlSelectEsr = new StrBuilder(GetSelectEsrColumn());
                sqlSelectEsr += SQLCst.WHERE + @"(esr.DTESR = @DT)" + Cst.CrLf;
                sqlSelectEsr += SQLCst.ORDERBY + "esr.IDESR ";
                //
                StrBuilder sqlSelectEsrDet = new StrBuilder(GetSelectEsrDetColumn());
                sqlSelectEsrDet += SQLCst.INNERJOIN_DBO + "ESR esr on esr.IDESR=esrd.IDESR";
                sqlSelectEsrDet += SQLCst.WHERE + @"(esr.DTESR = @DT)";
                //
                sqlSelect = sqlSelectEsr.ToString() + SQLCst.SEPARATOR_MULTISELECT + sqlSelectEsrDet.ToString();
            }
            //
            if ((_loadTypeEnum == LoadDataSetSettlementMessage.LoadMSO) || (_loadTypeEnum == LoadDataSetSettlementMessage.LoadESRAndMSO))
            {
                StrBuilder sqlSelectMso = new StrBuilder(GetSelectMsoColumn());
                sqlSelectMso += SQLCst.WHERE + @"(mso.DTMSO = @DT)" + Cst.CrLf;
                sqlSelectMso += SQLCst.ORDERBY + "mso.IDMSO ";
                //
                StrBuilder sqlSelectMsoDet = new StrBuilder(GetSelectMsoDetColumn());
                sqlSelectMsoDet += SQLCst.INNERJOIN_DBO + "MSO mso on mso.IDMSO=msod.IDMSO";
                sqlSelectMsoDet += SQLCst.WHERE + @"(mso.DTMSO = @DT)";
                //
                if (StrFunc.IsFilled(sqlSelect))
                    sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
                sqlSelect += sqlSelectMso.ToString() + SQLCst.SEPARATOR_MULTISELECT + sqlSelectMsoDet.ToString();
            }

            _ds = DataHelper.ExecuteDataset(_cs, pDbTransaction, CommandType.Text, sqlSelect, paramDate.DbDataParameter);

            InitializeDs();
        }
        #endregion LoadDs

        #region SetIdESR
        public void SetIdESR(IDbTransaction pDbTransaction)
        {

            DataRow[] dr = DtESR.Select("EXTLLINK = 'NEW'");
            //
            if (null != dr)
            {
                int newId;
                //
                if (null != pDbTransaction)
                    SQLUP.GetId(out newId, pDbTransaction, SQLUP.IdGetId.ESR, SQLUP.PosRetGetId.First, ArrFunc.Count(dr));
                else
                    SQLUP.GetId(out newId, _cs, SQLUP.IdGetId.ESR, SQLUP.PosRetGetId.First, ArrFunc.Count(dr));
                //
                for (int i = 0; i < dr.Length; i++)
                {
                    dr[i]["ID"] = newId;
                    dr[i]["EXTLLINK"] = Convert.DBNull;
                    newId++;
                }
            }

        }
        #endregion SetIdESR
        #region SetIdMSO
        public void SetIdMSO(IDbTransaction pDbTransaction)
        {

            DataRow[] dr = DtMSO.Select("EXTLLINK = 'NEW'");
            //
            if (null != dr)
            {
                int newId;
                //
                if (null != pDbTransaction)
                    SQLUP.GetId(out newId, pDbTransaction, SQLUP.IdGetId.MSO, SQLUP.PosRetGetId.First, ArrFunc.Count(dr));
                else
                    SQLUP.GetId(out newId, _cs, SQLUP.IdGetId.MSO, SQLUP.PosRetGetId.First, ArrFunc.Count(dr));
                //
                for (int i = 0; i < dr.Length; i++)
                {
                    dr[i]["ID"] = newId;
                    newId++;
                }
            }

        }
        #endregion SetIdMSO

        #region UpdateESR
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void UpdateESR(IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectEsrColumn();
            DataHelper.ExecuteDataAdapter(_cs, pDbTransaction, sqlSelect, DtESR);
        }
        #endregion UpdateESR
        #region UpdateESRDET
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void UpdateESRDET(IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectEsrDetColumn();
            DataHelper.ExecuteDataAdapter(_cs, pDbTransaction, sqlSelect, DtESRDET);
        }
        #endregion UpdateESRDET
        #region UpdateMSO
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void UpdateMSO(IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectMsoColumn();
            DataHelper.ExecuteDataAdapter(_cs, pDbTransaction, sqlSelect, DtMSO);
        }
        #endregion UpdateMSO
        #region UpdateMSODET
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void UpdateMSODET(IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectMsoDetColumn();
            DataHelper.ExecuteDataAdapter(_cs, pDbTransaction, sqlSelect, DtMSODET);
        }
        #endregion UpdateMSODET
        #endregion Methods
    }
    #endregion DatasetStlMsg

    #region EventSettlementReportKey
    public struct EventSettlementReportKey
    {
        #region Members
        public DateTime dtSTM;
        public DateTime dtSTMForced;
        public int idASenderParty;
        public int idASender;
        public int idAReceiver;
        public int idAReceiverParty;
        public int idACss;
        public string idC;
        public string settlementChainRef;
        public string netMethod;   //
        public int idT;
        public int idNetConvention;
        public int idNetDesignation;
        #endregion Members
        //
        #region Constructors
        public EventSettlementReportKey(DateTime pDtSTM, DateTime pDtSTMForced,
            int pIdASenderParty, int pIdASender, int pIdAReceiver, int pIdAReceiverParty, int pIdACss,
            string pIdC, string pSettlementChainRef,
            string pNetMethod, int pIdt, int pIdNetConvention, int pIdNetDesignation)
        {
            dtSTM = pDtSTM;
            dtSTMForced = pDtSTMForced;
            //
            idASenderParty = pIdASenderParty;
            idASender = pIdASender;
            idAReceiver = pIdAReceiver;
            idAReceiverParty = pIdAReceiverParty;
            idACss = pIdACss;
            //
            idC = pIdC;
            settlementChainRef = pSettlementChainRef;
            //
            netMethod = pNetMethod;
            idT = pIdt;
            idNetConvention = pIdNetConvention;
            idNetDesignation = pIdNetDesignation;

        }
        public EventSettlementReportKey(DataRow pRow)
            : this
            (
            Convert.ToDateTime(pRow["DTSTM"]),
            Convert.ToDateTime(pRow["DTSTMFORCED"]),
            Convert.ToInt32(pRow["IDA_SENDERPARTY"]),
            Convert.ToInt32(pRow["IDA_SENDER"]),
            Convert.ToInt32(pRow["IDA_RECEIVER"]),
            Convert.ToInt32(pRow["IDA_RECEIVERPARTY"]),
            Convert.ToInt32(pRow["IDA_CSS"]),
            Convert.ToString(pRow["IDC"]),
            Convert.ToString(pRow["SCREF"]),
            Convert.ToString(pRow["NETMETHOD"]),
            Convert.ToInt32(pRow["IDT"] == Convert.DBNull ? 0 : pRow["IDT"]),
            Convert.ToInt32(pRow["IDNETCONVENTION"] == Convert.DBNull ? 0 : pRow["IDNETCONVENTION"]),
            Convert.ToInt32(pRow["IDNETDESIGNATION"] == Convert.DBNull ? 0 : pRow["IDNETDESIGNATION"]))
        { }
        public EventSettlementReportKey(IDataReader pRow)
            : this
            (
            Convert.ToDateTime(pRow["DTSTM"]),
            Convert.ToDateTime(pRow["DTSTMFORCED"]),
            Convert.ToInt32(pRow["IDA_SENDERPARTY"]),
            Convert.ToInt32(pRow["IDA_SENDER"]),
            Convert.ToInt32(pRow["IDA_RECEIVER"]),
            Convert.ToInt32(pRow["IDA_RECEIVERPARTY"]),
            Convert.ToInt32(pRow["IDA_CSS"]),
            Convert.ToString(pRow["IDC"]),
            Convert.ToString(pRow["SCREF"]),
            Convert.ToString(pRow["NETMETHOD"]),
            Convert.ToInt32(pRow["IDT"] == Convert.DBNull ? 0 : pRow["IDT"]),
            Convert.ToInt32(pRow["IDNETCONVENTION"] == Convert.DBNull ? 0 : pRow["IDNETCONVENTION"]),
            Convert.ToInt32(pRow["IDNETDESIGNATION"] == Convert.DBNull ? 0 : pRow["IDNETDESIGNATION"]))
        { }
        #endregion Constructors
        //
        #region Methods
        #region ToString
        public override string ToString()
        {
            StrBuilder ret = new StrBuilder();
            ret.AppendFormat("STM [{0}], ", DtFunc.DateTimeToStringyyyyMMdd(dtSTM));
            ret.AppendFormat("STMFORCED [{0}], ", DtFunc.DateTimeToStringyyyyMMdd(dtSTMForced));
            ret.AppendFormat("IDA_SENDERPARTY[{0}], ", idASenderParty.ToString());
            ret.AppendFormat("IDA_SENDER[{0}], ", idASender.ToString());
            ret.AppendFormat("IDA_RECEIVER[{0}], ", idAReceiver.ToString());
            ret.AppendFormat("IDA_RECEIVERPARTY[{0}], ", idAReceiverParty.ToString());
            ret.AppendFormat("IDA_CSS[{0}], ", idACss.ToString());
            ret.AppendFormat("IDC[{0}], ", idC);
            ret.AppendFormat("SCREF[{0}], ", settlementChainRef);
            ret.AppendFormat("NETMETHOD[{0}], ", netMethod);
            ret.AppendFormat("IDNETCONVENTION[{0}], ", (idNetConvention > 0 ? idNetConvention.ToString() : "Null"));
            ret.AppendFormat("IDNETDESIGNATION[{0}], ", (idNetDesignation > 0 ? idNetDesignation.ToString() : "Null"));
            ret.AppendFormat("IDT[{0}], ", (idT > 0 ? idT.ToString() : "Null"));
            //
            return ret.ToString();
        }
        #endregion
        #endregion Methods
    }
    #endregion EventSettlementReportKey

    #region NetConventions
    /// <summary>
    /// Load Info of SsiDbs
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("NETCONVENTIONS", Namespace = "", IsNullable = true)]
    public partial class NetConventions
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idI;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string idC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idA1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idA2;
        [System.Xml.Serialization.XmlElementAttribute("NETCONVENTION", Order = 1)]
        public NetConvention[] netConvention;
        #endregion Members
    }
    #endregion NetConventions
    #region NetConvention
    public partial class NetConvention
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDMASTERAGREEMENT", Order = 1)]
        public int idMasterAgreement;
        //
        [System.Xml.Serialization.XmlElementAttribute("IDNETCONVENTION", Order = 2)]
        public int idNetConvention;
        //		
        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER", Order = 3)]
        public string identifier;
        //
        [System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME", Order = 4)]
        public string displayName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DESCRIPTION", Order = 5)]
        public string description;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gProductSpecified;
        [System.Xml.Serialization.XmlElementAttribute("GPRODUCT", Order = 6)]
        public string gProduct;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDP", Order = 7)]
        public int idP;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idGInstrSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDGINSTR", Order = 8)]
        public int idGInstr;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idISpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDI", Order = 9)]
        public string idI;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC", Order = 10)]
        public string idC;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK", Order = 11)]
        public string extLink;
        #endregion
    }
    #endregion NetConvention

    #region NetConventionIds
    public partial class NetConventionIds
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("netConventionId", Order = 1)]
        public NetConventionId[] netConventionId;
        #endregion Members
    }
    #endregion NetConventionIds
    #region NetConventionId
    /// <summary>
    /// Permet d'identifier la Convention de netting appliquée pour un règlement 
    /// </summary>
    public partial class NetConventionId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string netConventionIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion NetConventionId

    #region NetDesignationIds
    public partial class NetDesignationIds
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("netDesignationId", Order = 1)]
        public NetDesignationId[] netDesignationId;
        #endregion Members
    }
    #endregion NetDesignationIds
    #region NetDesignationId
    /// <summary>
    /// Permet d'identifier le netting par designation appliquée pour un règlement 
    /// </summary>
    public partial class NetDesignationId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string netDesignationIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion NetDesignationId

    #region NettingInformation
    /// <summary>
    /// Represente le type de netting à l'ogine du règlement 
    /// </summary>
    public partial class NettingInformation
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("nettingMethod", Order = 1)]
        public NettingMethodEnum nettingMethod;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool netConventionIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netConventionId", Order = 2)]
        public NetConventionId[] netConventionId;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool netDesignationIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netDesignationId", Order = 3)]
        public NetDesignationId[] netDesignationId;
        #endregion Members
    }
    #endregion NettingInformation

    #region PaymentId
    /// <summary>
    /// Permet d'identifier la source d'un payment (ESR ou EVENT)
    /// </summary>
    public partial class PaymentId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string paymentIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion PaymentId

    #region SettlementMessageId
    /// <summary>
    /// Permet d'identifier Le type de  message de règelement (SWFIT 202,210 Par Exemple)
    /// </summary>
    public partial class SettlementMessageId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settlementMessageIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion SettlementMessageId
    #region SettlementMessageIds
    /// <summary>
    /// represente n MessageId
    /// </summary>
    public partial class SettlementMessageIds
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("messageId", Order = 1)]
        public SettlementMessageId[] messageId;
        #endregion Members
    }
    #endregion SettlementMessageIds

    #region SettlementMessagePaymentStructure
    /// <summary>
    /// Paiement
    /// </summary>
    public partial class SettlementMessagePaymentStructure
    {
        #region Members
        public int id;
        public string idScheme;
        public DateTime dtSTM;                // Date initiale du message 
        public DateTime dtSTMForced;          // Date du message
        public int idASenderParty;
        public int idASender;
        public int idAReceiver;
        public int idAReceiverParty;
        public int idACss;
        public string idC;
        public string settlementChainReference;
        /// <summary>
        /// 
        /// </summary>
        public NettingMethodEnum netMethod;
        public int idNetConvention;
        public int idNetDesignation;
        public PayerReceiverEnum payerReceiver;
        public decimal amount;
        public int idT;
        public EfsMLDocumentVersionEnum efsMLversion;
        #endregion Members
    }
    #endregion struct SettlementMessagePaymentStructure

    #region SettlementMessagePaymentContainer
    public class SettlementMessagePaymentContainer
    {
        public ISettlementMessagePayment settlementMessagePayment;

        #region Methods
        #region LoadSettlementInstruction
        public void LoadSettlementInstruction(string pCs)
        {

            if (null != settlementMessagePayment.PaymentId)
            {
                switch (settlementMessagePayment.PaymentId.paymentIdScheme)
                {
                    case Cst.OTCml_EsrIdScheme:
                        LoadESRSettlementInstruction(pCs);
                        break;
                    case Cst.OTCml_EventIdScheme:
                        LoadEventSettlementInstruction(pCs);
                        break;
                }
            }

        }
        #endregion public LoadSettlementInstruction
        #region LoadTrade
        // EG 20180426 Analyse du code Correction [CA2202]
        public void LoadTrade(string pCs)
        {
            if (null != settlementMessagePayment.PaymentId)
            {
                QueryParameters qryParameters = null;

                switch (settlementMessagePayment.PaymentId.paymentIdScheme)
                {
                    case Cst.OTCml_EsrIdScheme:
                        qryParameters = GetQueryTradeFromESR(pCs);
                        break;
                    case Cst.OTCml_EventIdScheme:
                        qryParameters = GetQueryTradeFromEvent(pCs);
                        break;
                }

                if (null != qryParameters)
                {
                    using (IDataReader dr = DataHelper.ExecuteReader(pCs, System.Data.CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            settlementMessagePayment.DataDocumentSpecified = true;
                            EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(dr["TRADEXML"].ToString());
                            settlementMessagePayment.DataDocument = (IDataDocument)CacheSerializer.Deserialize(serializeInfo);
                        }
                    }
                }
            }
        }
        #endregion LoadTrade
        #region LoadEvent
        public void LoadEvent(string pCs)
        {
            int[] idEvent = null;

            if (null != settlementMessagePayment.PaymentId)
            {
                switch (settlementMessagePayment.PaymentId.paymentIdScheme)
                {
                    case Cst.OTCml_EsrIdScheme:
                        idEvent = SettlementMessageTools.GetEventInEsr(pCs, Convert.ToInt32(settlementMessagePayment.PaymentId.Value));
                        break;
                    case Cst.OTCml_EventIdScheme:
                        idEvent = new int[] { Convert.ToInt32(settlementMessagePayment.PaymentId.Value) };
                        break;
                }
            }
            //
            if (ArrFunc.IsFilled(idEvent))
            {
                DataSetEvent ds = new DataSetEvent(pCs);
                ds.Load(null, idEvent);
                settlementMessagePayment.Events = ds.GetEventItems();
                settlementMessagePayment.EventsSpecified = (null != this.settlementMessagePayment.Events);
            }
        }
        #endregion LoadEvent
        #region LoadNettingInformation
        public void LoadNettingInformation(string pCs, NettingMethodEnum pNettingMethod, int pIdNetConvention, int pIdNetDesignation)
        {

            settlementMessagePayment.NettingInformation = new NettingInformation
            {
                nettingMethod = pNettingMethod
            };
            switch (settlementMessagePayment.NettingInformation.nettingMethod)
            {
                case NettingMethodEnum.Convention:
                    SQL_NetConvention sqlNetConvention = new SQL_NetConvention(pCs, pIdNetConvention);
                    settlementMessagePayment.NettingInformation.netConventionIdSpecified = sqlNetConvention.IsLoaded;
                    //
                    if (sqlNetConvention.IsLoaded)
                    {
                        settlementMessagePayment.NettingInformation.netConventionId = new NetConventionId[2]					
							{
								new NetConventionId(Cst.OTCml_NetConventionIdScheme  , sqlNetConvention.Id.ToString()),
								new NetConventionId(Cst.OTCml_NetConventionIdentifierScheme, sqlNetConvention.Identifier)
							};
                        if (StrFunc.IsFilled(sqlNetConvention.ExtlLink))
                        {
                            ReflectionTools.AddItemInArray(this, "netConventionId", 0);
                            settlementMessagePayment.NettingInformation.netConventionId[2].netConventionIdScheme = Cst.OTCml_NetDesignationExtlLinkScheme;
                            settlementMessagePayment.NettingInformation.netConventionId[2].Value = sqlNetConvention.ExtlLink;
                        }
                    }
                    break;
                case NettingMethodEnum.Designation:
                    SQL_NetDesignation sqlDesignation = new SQL_NetDesignation(pCs, pIdNetDesignation);
                    settlementMessagePayment.NettingInformation.netDesignationIdSpecified = sqlDesignation.IsLoaded;
                    //
                    if (sqlDesignation.IsLoaded)
                    {
                        settlementMessagePayment.NettingInformation.netDesignationId = new NetDesignationId[2]					
								{
									new NetDesignationId(Cst.OTCml_NetDesignationIdScheme  , sqlDesignation.Id.ToString()),
									new NetDesignationId(Cst.OTCml_NetDesignationIdentifierScheme, sqlDesignation.Identifier)
								};
                        if (StrFunc.IsFilled(sqlDesignation.ExtlLink))
                        {
                            ReflectionTools.AddItemInArray(this, "netDesignationId", 0);
                            settlementMessagePayment.NettingInformation.netDesignationId[2].netDesignationIdScheme = Cst.OTCml_NetDesignationExtlLinkScheme;
                            settlementMessagePayment.NettingInformation.netDesignationId[2].Value = sqlDesignation.ExtlLink;
                        }
                    }
                    break;
                case NettingMethodEnum.Standard:
                case NettingMethodEnum.None:
                    settlementMessagePayment.NettingInformation.netDesignationIdSpecified = false;
                    settlementMessagePayment.NettingInformation.netConventionIdSpecified = false;
                    break;
            }

        }
        #endregion LoadNettingInformation
        #region LoadESRSettlementInstruction
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180426 Analyse du code Correction [CA2202]
        private void LoadESRSettlementInstruction(string pCs)
        {

            if (StrFunc.IsFilled(settlementMessagePayment.PaymentId.Value))
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(new DataParameter(pCs, "IDPAYER", DbType.Int32), new RoutingContainer((IRouting)this.settlementMessagePayment.Payer).GetRoutingIdA(pCs));
                dataParameters.Add(new DataParameter(pCs, "IDRECEIVER", DbType.Int32), new RoutingContainer((IRouting)this.settlementMessagePayment.Receiver).GetRoutingIdA(pCs));
                dataParameters.Add(new DataParameter(pCs, "PAYER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Payer.ToString());
                dataParameters.Add(new DataParameter(pCs, "RECEIVER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Receiver.ToString());
                dataParameters.Add(new DataParameter(pCs, "IDESR", DbType.Int32), Convert.ToInt32(settlementMessagePayment.PaymentId.Value));
                //	
                StrBuilder sql = new StrBuilder();
                sql += SQLCst.SELECT + "evsipay.SIXML, evsiRec.SIXML" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ESR + " esr " + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ESRDET + " esrDet on esrDet.IDESR=esr.IDESR" + Cst.CrLf;
                // Joiunture sur 1 des EVENT ou le Payer est le Payer de L'Event 
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " evpay on evpay.IDE= esrdet.IDE  And evpay.IDA_PAY = @IDPAYER" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTSI + " evsiPay on evsiPay.IDE= evpay.IDE and evsiPay.PAYER_RECEIVER = @PAYER" + Cst.CrLf;
                // Joiunture sur 1'EVENT ou le Receiver est le receiver de L'Event
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " evrec on evrec.IDE= esrdet.IDE  And evrec.IDA_REC = @IDRECEIVER" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTSI + " evsirec on evsirec.IDE= evrec.IDE and evsirec.PAYER_RECEIVER = @RECEIVER" + Cst.CrLf;
                //
                sql += SQLCst.WHERE + " esr.IDESR= @IDESR" + Cst.CrLf;
                //
                using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, sql.ToString(), dataParameters.GetArrayDbParameter()))
                {
                    if (dr.Read())
                    {
                        //Type tDocument					= Tools.GetTypeSettlementInstruction(m_Document);
                        //EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(tDocument, dr.GetString(0));
                        EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(dr.GetString(0));
                        settlementMessagePayment.Payer.SettlementInstruction = (IEfsSettlementInstruction)CacheSerializer.Deserialize(serializeInfo);
                        //serializeInfo = new EFS_SerializeInfoBase(tDocument, dr.GetString(1));
                        serializeInfo = new EFS_SerializeInfo(dr.GetString(1));
                        settlementMessagePayment.Receiver.SettlementInstruction = (IEfsSettlementInstruction)CacheSerializer.Deserialize(serializeInfo);
                    }
                }
            }

        }
        #endregion LoadESRSettlementInstruction
        #region LoadEventSettlementInstruction
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180426 Analyse du code Correction [CA2202]
        private void LoadEventSettlementInstruction(string pCs)
        {
            if (StrFunc.IsFilled(settlementMessagePayment.PaymentId.Value))
            {
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(new DataParameter(pCs, "PAYER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Payer.ToString());
                dataParameters.Add(new DataParameter(pCs, "RECEIVER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Receiver.ToString());
                dataParameters.Add(new DataParameter(pCs, "IDE", DbType.Int32), Convert.ToInt32(settlementMessagePayment.PaymentId.Value));

                StrBuilder sql = new StrBuilder();
                sql += SQLCst.SELECT + "evsipay.SIXML, evsiRec.SIXML" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " evt " + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTSI + " evsiPay on evsiPay.IDE= evt.IDE and evsiPay.PAYER_RECEIVER = @PAYER" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTSI + " evsirec on evsiRec.IDE= evt.IDE and evsiRec.PAYER_RECEIVER = @RECEIVER" + Cst.CrLf;
                sql += SQLCst.WHERE + " evt.IDE= @IDE" + Cst.CrLf;

                using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, sql.ToString(), dataParameters.GetArrayDbParameter()))
                {
                    if (dr.Read())
                    {
                        EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(dr.GetString(0));
                        settlementMessagePayment.Payer.SettlementInstruction = (IEfsSettlementInstruction)CacheSerializer.Deserialize(serializeInfo);
                        serializeInfo = new EFS_SerializeInfo(dr.GetString(1));
                        settlementMessagePayment.Receiver.SettlementInstruction = (IEfsSettlementInstruction)CacheSerializer.Deserialize(serializeInfo);
                    }
                }
            }
        }
        #endregion LoadEventSettlementInstruction
        #region GetQueryTradeFromEvent
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private QueryParameters GetQueryTradeFromEvent(string pCs)
        {


            //
            DataParameter paramIde = DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDE);
            paramIde.Value = Convert.ToInt32(settlementMessagePayment.PaymentId.Value);
            string sqlSelect = @"select trx.TRADEXML, pr.SOURCE
            from dbo.EVENT ev
            inner join dbo.TRADE tr on (tr.IDT = ev.IDT)
            inner join dbo.TRADEXML trx on (trx.IDT = ev.IDT)
            inner join dbo.INSTRUMENT ns on (ns.IDI = t.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
            where (ev.IDE = @IDE)";

            QueryParameters ret = new QueryParameters(pCs, sqlSelect, new DataParameters(new DataParameter[] { paramIde }));
            return ret;

        }
        #endregion GetQueryTradeFromEvent
        #region GetQueryTradeFromESR
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private QueryParameters GetQueryTradeFromESR(string pCs)
        {
            QueryParameters ret = null;

            DataParameter paramIdESR = DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID);
            paramIdESR.Value = Convert.ToInt32(settlementMessagePayment.PaymentId.Value);

            string sqlSelect = @"select isnull(e.IDT,0) as IDT
            from dbo.ESR e
            where (e.IDESR = @ID)" + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(pCs, System.Data.CommandType.Text, sqlSelect, paramIdESR.DbDataParameter))
            {
                if (dr.Read())
                {
                    DataParameter paramIdT = DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT);
                    paramIdT.Value = Convert.ToInt32(dr["IDT"]);

                    string sqlSelectTrade = @"select tr.IDT, trx.TRADEXML, pr.SOURCE
                    from dbo.TRADE tr
                    inner join dbo.TRADEXML trx on (trx.IDT = tr.IDT)
                    inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                    inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                    where (tr.IDT = @IDT)" + Cst.CrLf;

                    ret = new QueryParameters(pCs, sqlSelectTrade, new DataParameters(new DataParameter[] { paramIdT }));
                }
                else
                {
                    string sqlSelectTrade = @"select tr.IDT, trx.TRADEXML, pr.SOURCE
                    from dbo.ESR esr
                    inner join dbo.ESRDET esrdet on (esrdet.IDESR = esr.IDESR)
                    inner join dbo.EVENT ev on (ev.IDE = esrdet.IDE)
                    inner join dbo.TRADE tr on (tr.IDT = (ev.IDT)
                    inner join dbo.TRADEXML trx on (trx.IDT = tr.IDT)
                    inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                    inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
                    where (esr.IDESR = @ID) 
                    order by tr.IDT" + Cst.CrLf;

                    ret = new QueryParameters(pCs, sqlSelectTrade.ToString(), new DataParameters(new DataParameter[] { paramIdESR }));
                }
            }
            return ret;
        }
        #endregion GetQueryTradeFromESR
        #endregion Methods

        #region constructor
        public SettlementMessagePaymentContainer(ISettlementMessagePayment pSettlementMesssagePayment)
        {
            settlementMessagePayment = pSettlementMesssagePayment;
        }
        #endregion
    }
    #endregion

    #region SettlementMessageDocumentContainer
    public class SettlementMessageDocumentContainer
    {
        public ISettlementMessageDocument settlementMessageDoc;

        #region constructor
        public SettlementMessageDocumentContainer(ISettlementMessageDocument pSettlementMessageDoc)
        {
            settlementMessageDoc = pSettlementMessageDoc;
        }
        #endregion

        #region Methods
        #region Initialize
        public void Initialize(string pCs, int pOTCmlId, SettlementMessagePaymentStructure[] pPaymentStruct, string pSettlementMessageIdentifier)
        {

            if (StrFunc.IsFilled(pSettlementMessageIdentifier))
            {
                SQL_SettlementMessage sqlStlMessage = new SQL_SettlementMessage(pCs, pSettlementMessageIdentifier);
                bool isMessageExist = sqlStlMessage.LoadTable(new string[] { "IDSTLMESSAGE", "IDENTIFIER", "EXTLLINK", "ISUSEEVENT", "ISUSETRADE" });
                //
                if (isMessageExist)
                    Initialize(pCs, pOTCmlId, pPaymentStruct, sqlStlMessage);
                else
                    Initialize(pCs, pOTCmlId, pPaymentStruct, new SettlementMessageId[] { new SettlementMessageId(Cst.OTCml_STLMessageIdentifierScheme, pSettlementMessageIdentifier) }, false, false);
            }

        }
        public void Initialize(string pCs, int pOTCmlId, SettlementMessagePaymentStructure[] pPaymentStruct, SQL_SettlementMessage pSqlStlMsg)
        {
            //

            if (null != pSqlStlMsg)
            {
                ArrayList al = new ArrayList
                {
                    new SettlementMessageId(Cst.OTCml_STLMessageIdScheme, pSqlStlMsg.Id.ToString()),
                    new SettlementMessageId(Cst.OTCml_STLMessageIdentifierScheme, pSqlStlMsg.Identifier)
                };
                if (StrFunc.IsFilled(pSqlStlMsg.ExtlLink))
                    al.Add(new SettlementMessageId(Cst.OTCml_STLMessageExtlLinkScheme, pSqlStlMsg.ExtlLink));
                //
                SettlementMessageId[] messageId = (SettlementMessageId[])al.ToArray(typeof(SettlementMessageId));
                bool isUseTrade = pSqlStlMsg.IsUseTrade;
                bool isUseEvent = pSqlStlMsg.IsUseEvent;
                //
                Initialize(pCs, pOTCmlId, pPaymentStruct, messageId, isUseTrade, isUseEvent);
            }
            else throw new ArgumentNullException("SQL_SettlementMessage is null");

        }
        private void Initialize(string pCs, int pOTCmlId, SettlementMessagePaymentStructure[] pPaymentStruct, SettlementMessageId[] pSettlementMessageId, bool pLoadTrade, bool pLoadEvent)
        {

            if (ArrFunc.IsFilled(pPaymentStruct))
            {
                #region Header
                settlementMessageDoc.Header = settlementMessageDoc.CreateSettlementMessageHeader();
                //Id unique pour la transaction 
                settlementMessageDoc.Header.OTCmlId = pOTCmlId;
                //stlMessageId Type de Message (202,210 etc...)
                settlementMessageDoc.Header.SettlementMessageId = pSettlementMessageId;
                //Date Creation
                settlementMessageDoc.Header.CreationTimestamp = new EFS_DateTime
                {
                    DateTimeValue = OTCmlHelper.GetDateSys(pCs)
                };
                //Date Value
                settlementMessageDoc.Header.ValueDate = new EFS_Date
                {
                    DateValue = pPaymentStruct[0].dtSTMForced
                };
                //
                //Sender and Receiver
                int idASender = pPaymentStruct[0].idASender;
                int idAReceiver = pPaymentStruct[0].idAReceiver;
                int idACss = pPaymentStruct[0].idACss;
                //
                SettlementRoutingActorsBuilder actorInfo = new SettlementRoutingActorsBuilder(pCs, idACss, settlementMessageDoc.CreateRoutingCreateElement());
                actorInfo.Load(pCs, new int[] { idASender, idAReceiver, idACss });
                //
                settlementMessageDoc.Header.Sender = actorInfo.GetRouting(idASender);
                settlementMessageDoc.Header.Receiver = actorInfo.GetRouting(idAReceiver);
                //	
                #region Calc SumOfAMount
                bool AddSumOfAmount = (pPaymentStruct.Length > 1);
                decimal sumOfAmount = 0;
                if (AddSumOfAmount)
                {
                    string idc = pPaymentStruct[0].idC;
                    for (int i = 0; i < pPaymentStruct.Length; i++)
                    {
                        sumOfAmount += pPaymentStruct[i].amount;
                        if (pPaymentStruct[i].idC != idc)
                        {
                            AddSumOfAmount = false;
                            break;
                        }
                    }
                }
                settlementMessageDoc.Header.SumOfPaymentAmountsSpecified = AddSumOfAmount;
                settlementMessageDoc.Header.SumOfPaymentAmounts = new EFS_Decimal(sumOfAmount);
                #endregion SumOfAMount
                #endregion Header
                #region pPaymentStruct[]
                settlementMessageDoc.Payment = settlementMessageDoc.CreateSettlementMessagePayment(pPaymentStruct.Length);
                for (int i = 0; i < pPaymentStruct.Length; i++)
                {
                    SettlementMessagePaymentContainer settlementMessagePaymentContainer = new SettlementMessagePaymentContainer(settlementMessageDoc.Payment[i]);
                    ISettlementMessagePayment payment = settlementMessagePaymentContainer.settlementMessagePayment;

                    // PayerReceiverEnum.Payer == pTransaction[i].payerReceiver => Transaction de Payment de fond
                    int idPayer = (PayerReceiverEnum.Payer == pPaymentStruct[i].payerReceiver) ? pPaymentStruct[i].idASenderParty : pPaymentStruct[i].idAReceiverParty;
                    // PayerReceiverEnum.Payer == pTransaction[i].Receiver => Transaction de reception de fond
                    int idReceiver = (PayerReceiverEnum.Receiver == pPaymentStruct[i].payerReceiver) ? pPaymentStruct[i].idASenderParty : pPaymentStruct[i].idAReceiverParty;
                    //
                    SettlementRoutingActorsBuilder actorInfo2 = new SettlementRoutingActorsBuilder(pCs, idACss, settlementMessageDoc.CreateRoutingCreateElement());
                    actorInfo2.Load(pCs, new int[] { idPayer, idReceiver, idACss });
                    //
                    payment.PaymentId = new PaymentId(pPaymentStruct[0].idScheme, pPaymentStruct[0].id.ToString());
                    //				
                    payment.Payer = (ISettlementMessagePartyPayment)settlementMessageDoc.CreateSettlementMessagePartyPayment();
                    IRouting routingPayer = (IRouting)payment.Payer;
                    routingPayer.RoutingIdsAndExplicitDetails = actorInfo2.GetRoutingIdsAndExplicitDetails(idPayer);
                    routingPayer.RoutingIdsAndExplicitDetailsSpecified = true;
                    //
                    payment.Receiver = (ISettlementMessagePartyPayment)settlementMessageDoc.CreateSettlementMessagePartyPayment();
                    IRouting routingReceiver = (IRouting)payment.Receiver;
                    routingReceiver.RoutingIdsAndExplicitDetails = actorInfo2.GetRoutingIdsAndExplicitDetails(idReceiver);
                    routingReceiver.RoutingIdsAndExplicitDetailsSpecified = true;
                    //
                    payment.ValueDate = new EFS_Date
                    {
                        DateValue = pPaymentStruct[0].dtSTMForced
                    };
                    payment.PaymentAmount = (IMoney)settlementMessageDoc.CreateMoney(pPaymentStruct[i].amount, pPaymentStruct[i].idC);
                    //
                    settlementMessagePaymentContainer.LoadNettingInformation(pCs, pPaymentStruct[i].netMethod, pPaymentStruct[i].idNetConvention, pPaymentStruct[i].idNetDesignation);
                    if ((NettingMethodEnum.Standard == pPaymentStruct[i].netMethod) ||
                        (NettingMethodEnum.None == pPaymentStruct[i].netMethod))
                    {
                        payment.Payer.LoadTradeId(pCs, pPaymentStruct[i].idT);
                        payment.Receiver.LoadTradeId(pCs, pPaymentStruct[i].idT);
                    }
                    //
                    settlementMessagePaymentContainer.LoadSettlementInstruction(pCs);
                    //	
                    if (pLoadTrade)
                        settlementMessagePaymentContainer.LoadTrade(pCs);
                    if (pLoadEvent)
                        settlementMessagePaymentContainer.LoadEvent(pCs);
                    //
                }
                #endregion pPaymentStruct[]
            }

        }
        #endregion Initialize
        #endregion Methods
    }
    #endregion
}
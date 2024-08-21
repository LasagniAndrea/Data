#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Classe qui permet la mise à jour de la table TRADESTREAM
    /// </summary>
    /// EG 20150706 521021] Nullable integer (m_IdA_Pay|m_IdA_Rec)
    public class TradeStreamInfoShortForm
    {
        #region Members
        protected int m_IdT;
        protected int m_InstrumentNo;
        protected int m_StreamNo;

        protected string m_IdC1;
        protected string m_IdC2;
        protected Nullable<int> m_IdA_Pay;
        protected Nullable<int> m_IdA_Rec;
        #endregion Members

        #region Accessors

        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> IdA_Pay
        {
            get { return m_IdA_Pay; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> IdA_Rec
        {
            get { return m_IdA_Rec; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IdC1
        {
            set { m_IdC1 = value; }
            get { return m_IdC1; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IdC2
        {
            set { m_IdC2 = value; }
            get { return m_IdC2; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int InstrumentNo
        {
            set { m_InstrumentNo = value; }
            get { return m_InstrumentNo; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int StreamNo
        {
            set { m_StreamNo = value; }
            get { return m_StreamNo; }
        }
        #endregion Accessors

        #region Constructors
        public TradeStreamInfoShortForm(int pIdT, int pInstrumentNo, int pStreamNo)
        {
            m_IdT = pIdT;
            m_InstrumentNo = pInstrumentNo;
            m_StreamNo = pStreamNo;
        }
        #endregion Constructors

        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        public void Insert(string pCs, IDbTransaction pDbTransaction)
        {
            QueryParameters queryParameters = GetQueryInsert(pCs);
            //
            DataHelper.ExecuteNonQuery(pCs, pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        public void Update(string pCs, IDbTransaction pDbTransaction)
        {
            QueryParameters queryParameters = GetQueryUpdate(pCs);
            int rowUpdated = DataHelper.ExecuteNonQuery(pCs, pDbTransaction, CommandType.Text,
                queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            if (1 != rowUpdated)
                throw new Exception(StrFunc.AppendFormat("Error on update TRADESTREAM,  rowUpdated= {0}", rowUpdated.ToString()));

        }
        /// <summary>
        /// Affecte IdA_Pay et IdA_Rec 
        /// </summary>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="pPayer"></param>
        /// <param name="pReceiver"></param>
        public void SetIdParty(TradeCommonInput pTradeCommonInput, string pPayer, string pReceiver)
        {
            m_IdA_Pay = pTradeCommonInput.DataDocument.GetOTCmlId_Party(pPayer);
            m_IdA_Rec = pTradeCommonInput.DataDocument.GetOTCmlId_Party(pReceiver);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected virtual QueryParameters GetQueryUpdate(string pCS)
        {
            StrBuilder update = new StrBuilder();
            update += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRADESTREAM + SQLCst.SET + Cst.CrLf;
            update += "IDA_PAY = @IDA_PAY, IDA_REC = @IDA_REC, IDC = @IDC, IDC2 = @IDC2" + Cst.CrLf;
            update += SQLCst.WHERE + "IDT=@IDT and INSTRUMENTNO=@INSTRUMENTNO and STREAMNO=@STREAMNO";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), m_IdT);
            dataParameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), m_InstrumentNo);
            dataParameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), m_StreamNo);
            dataParameters.Add(new DataParameter(pCS, "IDA_PAY", DbType.Int32), DB_IdAPay());
            dataParameters.Add(new DataParameter(pCS, "IDA_REC", DbType.Int32), DB_IdARec());
            dataParameters.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC1());
            dataParameters.Add(new DataParameter(pCS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC2());

            return new QueryParameters(pCS, update.ToString(), dataParameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected virtual QueryParameters GetQueryInsert(string pCS)
        {
            StrBuilder insert = new StrBuilder();
            insert += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADESTREAM + Cst.CrLf;
            insert += "(IDT, INSTRUMENTNO, STREAMNO,";
            insert += "IDA_PAY, IDA_REC, IDC, IDC2)" + Cst.CrLf;
            insert += "values " + Cst.CrLf;
            insert += "(@IDT, @INSTRUMENTNO, @STREAMNO,";
            insert += "@IDA_PAY, @IDA_REC, @IDC, @IDC2)";
            //
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), m_IdT);
            dataParameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), m_InstrumentNo);
            dataParameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), m_StreamNo);
            dataParameters.Add(new DataParameter(pCS, "IDA_PAY", DbType.Int32), DB_IdAPay());
            dataParameters.Add(new DataParameter(pCS, "IDA_REC", DbType.Int32), DB_IdARec());
            dataParameters.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC1());
            dataParameters.Add(new DataParameter(pCS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC2());
            //                
            QueryParameters qry = new QueryParameters(pCS, insert.ToString(), dataParameters);

            return qry;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected object DB_IdC1()
        {
            return StrFunc.IsFilled(m_IdC1) ? m_IdC1 : Convert.DBNull;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected object DB_IdC2()
        {
            return StrFunc.IsFilled(m_IdC2) ? m_IdC2 : Convert.DBNull;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected object DB_IdAPay()
        {
            return (m_IdA_Pay > 0) ? m_IdA_Pay : Convert.DBNull;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected object DB_IdARec()
        {
            return (m_IdA_Rec > 0) ? m_IdA_Rec : Convert.DBNull;
        }
        #endregion Methods
    }

}
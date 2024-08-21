#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using System;
using System.Collections;
using System.Data;


#endregion Using Directives


namespace EfsML
{
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    public enum TradeTableEnum
    {
        NONE=1,
        TRADEACTOR = 2,
        TRADESTREAM = 4,
        TRADEXML = 8,
        TRADENOTEPAD = 16,
    }
    /// <summary>
    /// 
    /// </summary>
    public class DataSetTrade
    {
   
        #region members
        private readonly string m_Cs;
        private DataSet m_DsTrade;
        #endregion members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public string Cs
        {
            get { return m_Cs; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DataSet DsTrade
        {
            get { return m_DsTrade; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdI
        {
            get
            {
                int ret = 0;
                if ((null != m_DsTrade) && (null != m_DsTrade.Tables["Trade"]))
                    ret = Convert.ToInt32(m_DsTrade.Tables["Trade"].Rows[0]["IDI"]);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdT
        {
            get
            {
                int ret = 0;
                if ((null != m_DsTrade) && (null != m_DsTrade.Tables["Trade"]))
                    ret = Convert.ToInt32(m_DsTrade.Tables["Trade"].Rows[0]["IDT"]);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Identifier
        {
            get
            {
                string ret = string.Empty;
                if ((null != m_DsTrade) && (null != m_DsTrade.Tables["Trade"]))
                    ret = m_DsTrade.Tables["Trade"].Rows[0]["IDENTIFIER"].ToString();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtTrade
        {
            get { return m_DsTrade.Tables["Trade"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public DataTable DtTradeXML
        {
            get { return m_DsTrade.Tables["TradeXML"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtTradeStream
        {
            get { return m_DsTrade.Tables["TradeStream"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtTradeActor
        {
            get { return m_DsTrade.Tables["TradeActor"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtTradeNotepad
        {
            get { return m_DsTrade.Tables["TradeNotePad"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable[] DataTable
        {
            get
            {
                DataTable[] ret = null;
                if ((DsTrade.Tables.Count) > 0)
                {
                    ret = new DataTable[DsTrade.Tables.Count];
                    DsTrade.Tables.CopyTo((DataTable[])ret, 0);
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public bool IsDeactiv
        {
            get
            {
                DataRow rowTrade = DtTrade.Rows[0];
                return (rowTrade["IDSTACTIVATION"].ToString() == Cst.StatusActivation.DEACTIV.ToString());
            }
        }

        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name=" pCs"></param>
        public DataSetTrade(string pCs)
        {
            m_Cs = pCs;
        }
        /// <summary>
        ///  Surcharge qui load le trade 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdt"></param>
        // EG 20190613 [24683] Use DbTransaction
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public DataSetTrade(string pCs, int pIdT)
            : this(pCs, null, pIdT)
        {
        }
        // EG 20190613 [24683] Use DbTransaction
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public DataSetTrade(string pCs, IDbTransaction pDbTransaction, int pIdT)
            : this(pCs)
        {
            Load(pDbTransaction, pIdT, TradeTableEnum.TRADEACTOR | TradeTableEnum.TRADENOTEPAD | TradeTableEnum.TRADESTREAM | TradeTableEnum.TRADEXML);
        }
        // EG 20190613 [24683] Use DbTransaction
        public DataSetTrade(string pCs, IDbTransaction pDbTransaction, int pIdT, TradeTableEnum pTradeTableEnum)
            : this(pCs)
        {
            Load(pDbTransaction, pIdT, pTradeTableEnum);
        }
        #endregion Constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdT"></param>
        // EG 20190613 [24683] Use DbTransaction
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public void Load(IDbTransaction pDbTransaction, int pIdT, TradeTableEnum pTradeTableEnum)
        {
            bool isAlltable = (pTradeTableEnum == 0);
            DataParameter paramIdT = new DataParameter(m_Cs, "IDT", DbType.Int32)
            {
                Value = pIdT
            };

            StrBuilder sqlSelect = new StrBuilder(string.Empty);
            sqlSelect += GetSelectTradeColumn() + @" where (tr.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;

            if (isAlltable || ((TradeTableEnum.TRADEACTOR & pTradeTableEnum) > 0))
                sqlSelect += GetSelectTradeActorColumn() + @" where (ta.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            if (isAlltable || ((TradeTableEnum.TRADENOTEPAD & pTradeTableEnum) > 0))
                sqlSelect += GetSelectTradeNotepadColumn() + @" where (np.ID = @IDT) and (np.TABLENAME = 'TRADE')" + SQLCst.SEPARATOR_MULTISELECT;
            if (isAlltable || ((TradeTableEnum.TRADESTREAM & pTradeTableEnum) > 0))
                sqlSelect += GetSelectTradeStreamColumn() + @" where (ts.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            if (isAlltable || ((TradeTableEnum.TRADEXML & pTradeTableEnum) > 0))
                sqlSelect += GetSelectTradeXMLColumn() + @" where (trx.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;


            m_DsTrade = DataHelper.ExecuteDataset(m_Cs, pDbTransaction, CommandType.Text, sqlSelect.ToString(), paramIdT.DbDataParameter);

            InitializeDataSet("Trades", pTradeTableEnum);

            if (0 == m_DsTrade.Tables["Trade"].Rows.Count)
                throw new Exception(StrFunc.AppendFormat("Trade n°{0} not found.", pIdT.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        public void UpdateTradeNotepad(IDbTransaction pDbTransaction)
        {

            string sqlSelect = GetSelectTradeNotepadColumn();
            DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, DtTradeNotepad);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public void UpdateTradeStSys(IDbTransaction pDbTransaction)
        {

            string sqlSelect = GetSelectTradeStSysColumn(true);
            DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, DtTrade);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200914 [25077] Correction (DtTradeXML)
        public void UpdateTradeXML(IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectTradeXMLColumn(true);
            DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, DtTradeXML);
        }
        // EG 20200914 [XXXXX] New
        public void UpdateTrade(IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectTradeColumn(true);
            DataHelper.ExecuteDataAdapter(pDbTransaction, sqlSelect, DtTrade);
        }

        // EG 20230222 [WI853][26600] New 
        public void UpdateUnallocatedTradeInvoiceRowAttribute(IDbTransaction pDbTransaction, int pIdT)
        {
            string sqlUpdate = $"Update TRADE set ROWATTRIBUT = null where IDT = {pIdT} and ROWATTRIBUT = '{Cst.RowAttribut_InvoiceClosed}'";
            DataHelper.ExecuteScalar(pDbTransaction,CommandType.Text, sqlUpdate);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSelectTradeColumn()
        {
            return GetSelectTradeColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        /// EG 20141230 [20587]
        /// EG 20171025 [23509] add column TZFACILITY
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200914 [XXXXX] Bug Add ROWATTRIBUT
        private string GetSelectTradeColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select tr.IDT, tr.IDI, tr.IDENTIFIER, tr.DISPLAYNAME, tr.DESCRIPTION,
            tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.DTORDERENTERED,  tr.DTBUSINESS, tr.TZFACILITY, 
            tr.IDT_SOURCE, tr.SOURCE, tr.DTSYS, tr.EXTLLINK, tr.ROWATTRIBUT {0}, 
            tr.IDSTENVIRONMENT, tr.DTSTENVIRONMENT, tr.IDASTENVIRONMENT,
            tr.IDSTPRIORITY, tr.DTSTPRIORITY, tr.IDASTPRIORITY,
            tr.IDSTACTIVATION, tr.DTSTACTIVATION, tr.IDASTACTIVATION,
            tr.IDSTBUSINESS, tr.DTSTBUSINESS, tr.IDASTBUSINESS,
            tr.IDSTUSEDBY, tr.LIBSTUSEDBY, tr.DTSTUSEDBY, tr.IDASTUSEDBY
            from dbo.TRADE tr
            {1}" + Cst.CrLf;

            string colSource = ", pr.SOURCE as SOURCE_PRODUCT" + Cst.CrLf;
            string sqlJoin = @"inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)";

            return String.Format(sqlSelect, (pWithOnlyTblMain ? string.Empty : colSource), (pWithOnlyTblMain ? string.Empty : sqlJoin));
        }


        private static string GetSelectTradeXMLColumn()
        {
            return GetSelectTradeXMLColumn(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectTradeXMLColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select trx.IDT, trx.EFSMLVERSION, trx.TRADEXML" + Cst.CrLf;

            if (false == pWithOnlyTblMain)
                sqlSelect += @",tr.IDENTIFIER" + Cst.CrLf;

            sqlSelect += @"from dbo.TRADEXML trx " + Cst.CrLf;

            if (false == pWithOnlyTblMain)
            {
                sqlSelect += @"inner join dbo.TRADE tr on (tr.IDT = trx.IDT)" + Cst.CrLf;
            }
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectTradeNotepadColumn()
        {
            string sqlSelect = @"select np.ID as IDT,np.LONOTE,np.TABLENAME,np.DTUPD,np.IDAUPD as IDA
            from dbo.NOTEPAD np" + Cst.CrLf;
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetSelectTradeStreamColumn()
        {
            string sqlSelect = @"select ts.IDT, ts.INSTRUMENTNO, ts.STREAMNO, ts.IDC, ts.IDC2, ts.OPTIONTYPE,
            ts.IDA_PAY, tapay.IDB as IDB_PAY, tapay.LOCALCLASSDERV as LOCALCLASSDERV_PAY,
            bkpay.IDA_ENTITY as IDA_PAY_ENTITY, etypay.DISPLAYNAME as PAY_ENTITY_DISPLAYNAME, 
            bkpay.ACCRUEDINTMETHOD as PAY_ACCRUEDINTMETHOD,
            ts.IDA_REC, tarec.IDB as IDB_REC, tarec.LOCALCLASSDERV as LOCALCLASSDERV_REC, 
            bkrec.IDA_ENTITY as IDA_REC_ENTITY, etyrec.DISPLAYNAME as REC_ENTITY_DISPLAYNAME,
            bkrec.ACCRUEDINTMETHOD as REC_ACCRUEDINTMETHOD
            from dbo.TRADESTREAM ts
            inner join dbo.TRADEACTOR tapay on (tapay.IDA = ts.IDA_PAY) and (tapay.IDT = ts.IDT) and (tapay.IDROLEACTOR='COUNTERPARTY')
            inner join dbo.TRADEACTOR tarec on (tarec.IDA = ts.IDA_REC) and (tarec.IDT = ts.IDT) and (tarec.IDROLEACTOR='COUNTERPARTY')
            left outer join dbo.BOOK  bkpay on (bkpay.IDB = tapay.IDB)
            left outer join dbo.ACTOR etypay on (etypay.IDA = bkpay.IDA_ENTITY)
            left outer join dbo.BOOK  bkrec on (bkrec.IDB = tarec.IDB)
            left outer join dbo.ACTOR etyrec on (etyrec.IDA = bkrec.IDA_ENTITY)" + Cst.CrLf;
            return sqlSelect.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetSelectTradeStSysColumn(bool pWithOnlyTblMain)
        {
            // FI 20210127 [XXXXX] Ajout de la colonne tr.IDENTIFIER (nécessaire depuis que la PK IDT n'est plus un clustered index sur SQLSERVER)
            // En effet IDT n'est plus détecté comme PrimaryKey par dataadapter. Par contre ce dernier détecte l'index unique sur la colonne IDENTIFIER et l'utilise comme PrimaryKey.
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER,
            tr.IDSTACTIVATION, tr.DTSTACTIVATION, tr.IDASTACTIVATION, 
            tr.IDSTUSEDBY, tr.DTSTUSEDBY, tr.IDASTUSEDBY, tr.LIBSTUSEDBY,
            tr.IDSTBUSINESS, tr.DTSTBUSINESS, tr.IDASTBUSINESS" + Cst.CrLf;

            if (false == pWithOnlyTblMain)
            {
                sqlSelect += @",a1.DISPLAYNAME as IDA_STACTIVATION_DISPLAYNAME,
                a2.DISPLAYNAME as IDA_STUSEDBY_DISPLAYNAME,
                a3.DISPLAYNAME as IDA_STBUSINESS_DISPLAYNAME" + Cst.CrLf;
            }

            sqlSelect += @"from dbo.TRADE tr " + Cst.CrLf;

            if (false == pWithOnlyTblMain)
            {
                sqlSelect += @"inner dbo.ACTOR a1 on (a1.IDA = tr.IDASTACTIVATION)
                left outer join dbo.ACTOR a2 on (a2.IDA = tr.IDASTUSEDBY)
                left outer join dbo.ACTOR a3 on (a3.IDA = tr.IDASTBUSINESS)" + Cst.CrLf;
            }
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetSelectTradeActorColumn()
        {
            string sqlSelect = @"select ta.IDT, ta.IDA, ac.DISPLAYNAME as ACTOR_DISPLAYNAME,
            ta.IDB, bk.DISPLAYNAME as BOOK_DISPLAYNAME, ta.BUYER_SELLER,
            ta.LOCALCLASSDERV, ta.IASCLASSDERV, ta.HEDGECLASSDERV, ta.FXCLASS,
            ta.LOCALCLASSNDRV, ta.IASCLASSNDRV, ta.HEDGECLASSNDRV, ta.FIXPARTYROLE
            from dbo.TRADEACTOR ta
            inner join dbo.ACTOR ac on (ac.IDA = ta.IDA)
            inner join dbo.BOOK bk on (bk.IDB = ta.IDB)" + Cst.CrLf;
            return sqlSelect.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataSetName"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void InitializeDataSet(string pDataSetName)
        {
            InitializeDataSet(pDataSetName, TradeTableEnum.TRADEACTOR | TradeTableEnum.TRADENOTEPAD | TradeTableEnum.TRADESTREAM | TradeTableEnum.TRADEXML);
        }

        private void InitializeDataSet(string pDataSetName, TradeTableEnum pTradeTableEnum)
        {
            bool isAlltable = (pTradeTableEnum == 0);

            m_DsTrade.DataSetName = pDataSetName;
            m_DsTrade.Tables[0].TableName = "Trade";

            DsTrade.Relations.Clear();

            int _index = 1;
            if (isAlltable || ((TradeTableEnum.TRADEACTOR & pTradeTableEnum) > 0))
            {
                m_DsTrade.Tables[_index].TableName = "TradeActor";
                DataRelation relTradeActor = new DataRelation(DtTradeActor.TableName, DtTrade.Columns["IDT"], DtTradeActor.Columns["IDT"], false)
                {
                    Nested = true
                };
                DsTrade.Relations.Add(relTradeActor);
                _index++;
            }
            if (isAlltable || ((TradeTableEnum.TRADENOTEPAD & pTradeTableEnum) > 0))
            {
                m_DsTrade.Tables[_index].TableName = "TradeNotepad";
                DataRelation relTradeNotepad = new DataRelation(DtTradeNotepad.TableName, DtTrade.Columns["IDT"], DtTradeNotepad.Columns["IDT"], false)
                {
                    Nested = true
                };
                DsTrade.Relations.Add(relTradeNotepad);
                _index++;
            }
            if (isAlltable || ((TradeTableEnum.TRADESTREAM & pTradeTableEnum) > 0))
            {
                m_DsTrade.Tables[_index].TableName = "TradeStream";
                DataRelation relTradeStream = new DataRelation(DtTradeStream.TableName, DtTrade.Columns["IDT"], DtTradeStream.Columns["IDT"], false)
                {
                    Nested = true
                };
                DsTrade.Relations.Add(relTradeStream);
                _index++;
            }
            if (isAlltable || ((TradeTableEnum.TRADEXML & pTradeTableEnum) > 0))
            {
                m_DsTrade.Tables[_index].TableName = "TradeXML";
                DataRelation relTradeXML = new DataRelation(DtTradeXML.TableName, DtTrade.Columns["IDT"], DtTradeXML.Columns["IDT"], false)
                {
                    Nested = true
                };
                DsTrade.Relations.Add(relTradeXML);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TradeItems GetTradeItems()
        {
            TradeItems ret = null;
            if ((null != DsTrade) && (0 < DtTrade.Rows.Count))
            {
                InitializeDataSet("Trades");
                //
                string serializeResult = new DatasetSerializer(DsTrade).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(TradeItems), serializeResult);
                ret = (TradeItems)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TradeInfo GetTradeInfo()
        {
            TradeInfo ret = null;
            if ((null != DsTrade) && (0 < DtTrade.Rows.Count))
            {
                //
                InitializeDataSet("TradeInfo" );
                string serializeResult = new DatasetSerializer(DsTrade).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(TradeInfo), serializeResult);
                ret = (TradeInfo)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("Trades", IsNullable = false)]
    public class TradeItems
    {
        [System.Xml.Serialization.XmlElementAttribute("Trade", Order = 1)]
        public TradeItem[] trade;
    }


    /// <summary>
    /// 
    /// </summary>
    /// EG 20171025 [23509] add column TZFACILITY (change DTTIMESTAMP type)
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    public class TradeItem
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDT", Order = 1)]
        public int idT;
        [System.Xml.Serialization.XmlElementAttribute("IDI", Order = 2)]
        public int idI;
        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER", Order = 3)]
        public string identifier;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME", Order = 4)]
        public string displayName;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DESCRIPTION", Order = 5)]
        public string description;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtTradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTTRADE", Order = 6)]
        public EFS_Date dtTrade;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtTimeStampSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTTIMESTAMP", Order = 7)]
        public EFS_DateTime dtTimeStamp;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtExecutionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTEXECUTION", Order = 8)]
        public EFS_DateTime dtExecution;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtOrderEnteredSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTORDERENTERED", Order = 9)]
        public EFS_DateTime dtOrderEntered;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtBusinessSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTBUSINESS", Order = 10)]
        public EFS_DateTime dtBusiness;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tzFacilitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("TZFACILITY", Order = 11)]
        public string tzFacility;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDT_SOURCE", Order = 12)]
        public int idTSource;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SOURCE", Order = 13)]
        public string source;
        //		
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtSysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSYS", Order = 14)]
        public EFS_DateTime dtSys;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK", Order = 15)]
        public string extlLink;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SourceProductSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SOURCE_PRODUCT", Order = 16)]
        public string SourceProduct;
        #endregion

        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDSTENVIRONMENT", Order = 17)]
        public string idStEnvironment;
        [System.Xml.Serialization.XmlElementAttribute("DTSTENVIRONMENT", Order = 18)]
        public EFS_DateTime dtStEnvironment;
        [System.Xml.Serialization.XmlElementAttribute("IDASTENVIRONMENT", Order = 19)]
        public int idAStEnvironment;

        [System.Xml.Serialization.XmlElementAttribute("IDSTPRIORITY", Order = 20)]
        public string idStPriority;
        [System.Xml.Serialization.XmlElementAttribute("DTSTPRIORITY", Order = 21)]
        public EFS_DateTime dtStPriority;
        [System.Xml.Serialization.XmlElementAttribute("IDASTPRIORITY", Order = 22)]
        public int idAStPriority;

        [System.Xml.Serialization.XmlElementAttribute("IDSTACTIVATION", Order = 23)]
        public string idStActivation;
        [System.Xml.Serialization.XmlElementAttribute("DTSTACTIVATION", Order = 24)]
        public EFS_Date dtStActivation;
        [System.Xml.Serialization.XmlElementAttribute("IDASTACTIVATION", Order = 25)]
        public int idAStActivation;

        [System.Xml.Serialization.XmlElementAttribute("IDSTBUSINESS", Order = 26)]
        public string idStBusiness;
        [System.Xml.Serialization.XmlElementAttribute("DTSTBUSINESS", Order = 27)]
        public EFS_DateTime dtStBusiness;
        [System.Xml.Serialization.XmlElementAttribute("IDASTBUSINESS", Order = 28)]
        public int idAStBusiness;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTUSEDBY", Order = 29)]
        public string idStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSTUSEDBY", Order = 30)]
        public EFS_DateTime dtStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool libStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("LIBSTUSEDBY", Order = 31)]
        public string libStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDASTUSEDBY", Order = 32)]
        public int idAStUsedBy;
        #endregion Members

    }

    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    public class TradeItemXML
    {
        [System.Xml.Serialization.XmlElementAttribute("IDT", Order = 1)]
        public int idT;

        [System.Xml.Serialization.XmlElementAttribute("EFSMLVERSION", Order = 2)]
        public string version;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeXMLSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TRADEXML", Order = 3)]
        public CDATA tradeXML;


    }

    /// <summary>
    ///  Glop A completer avec TRADESTREAM etc
    /// </summary>
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    [System.Xml.Serialization.XmlRootAttribute("TradeInfo", IsNullable = false)]
    public class TradeInfo
    {
        [System.Xml.Serialization.XmlElementAttribute("Trade", Order = 1)]
        public TradeItem trade;
        [System.Xml.Serialization.XmlElementAttribute("TradeXML")]
        public TradeItemXML xml;
    }

    /// <summary>
    /// 
    /// </summary>
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    [System.Xml.Serialization.XmlRootAttribute("TradeExport", IsNullable = false)]
    public class TradeExport
    {
        [System.Xml.Serialization.XmlElementAttribute("Trade")]
        public TradeItem trade;
        [System.Xml.Serialization.XmlElementAttribute("TradeXML")]
        public TradeItemXML xml;
        [System.Xml.Serialization.XmlElementAttribute("TradeActor")]
        public TradeActor tradeActor;
        [System.Xml.Serialization.XmlElementAttribute("TradeStream")]
        public TradeStream tradeStream;
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public HierarchicalEvent hEvent;
    }


    /// <summary>
    /// 
    /// </summary>
    public class TradeActor
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDT", Order = 1)]
        public int idT;
        [System.Xml.Serialization.XmlElementAttribute("IDA", Order = 2)]
        public int idA;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ACTOR_DISPLAYNAME", Order = 3)]
        public string actorDisplayName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB", Order = 4)]
        public int idB;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BOOK_DISPLAYNAME", Order = 5)]
        public string bookDisplayName;
        [System.Xml.Serialization.XmlElementAttribute("BUYER_SELLER", Order = 6)]
        public string buyer_Seller;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool localClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("LOCALCLASSDERV", Order = 7)]
        public string localClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool iasClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IASCLASSDERV", Order = 8)]
        public string iasClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hedgeClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("HEDGECLASSDERV", Order = 9)]
        public string hedgeClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fxClassSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FXCLASS", Order = 10)]
        public string fxClass;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool localClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("LOCALCLASSNDRV", Order = 11)]
        public string localClassNDrv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool iasClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IASCLASSNDRV", Order = 12)]
        public string iasClassNDrv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hedgeClassNDrvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("HEDGECLASSNDRV", Order = 13)]
        public string hedgeClassNDrv;
        #endregion Members
    }


    /// <summary>
    /// 
    /// </summary>
    public class TradeStream
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDT", Order = 1)]
        public int idT;
        [System.Xml.Serialization.XmlElementAttribute("INSTRUMENTNO", Order = 2)]
        public int instrumentNo;
        [System.Xml.Serialization.XmlElementAttribute("STREAMNO", Order = 3)]
        public int streamNo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC", Order = 4)]
        public string idC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC2Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDC2", Order = 5)]
        public string idC2Description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool optionTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("OPTIONTYPE", Order = 6)]
        public string optionType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorPayerAccruedIntMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PAY_ACCRUEDINTMETHOD", Order = 7)]
        public string actorPayerAccruedIntMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorPayerAccruedIntPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PAY_ACCRUEDINTPERIOD", Order = 8)]
        public string actorPayerAccruedIntPeriod;
        [System.Xml.Serialization.XmlElementAttribute("IDA_PAY", Order = 9)]
        public int idA_Pay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idB_PaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB_PAY", Order = 10)]
        public int idB_Pay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorPayerLocalClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_PAY_LOCALCLASSDERV", Order = 11)]
        public string actorPayerLocalClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_Pay_EntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_PAY_ENTITY", Order = 12)]
        public int idA_Pay_Entity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorPayerEntityDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PAY_ENTITY_DISPLAYNAME", Order = 13)]
        public string actorPayerEntityDisplayName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorReceiverAccruedIntMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("REC_ACCRUEDINTMETHOD", Order = 14)]
        public string actorReceiverAccruedIntMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorReceiverAccruedIntPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("REC_ACCRUEDINTPERIOD", Order = 15)]
        public string actorReceiverAccruedIntPeriod;
        [System.Xml.Serialization.XmlElementAttribute("IDA_REC", Order = 16)]
        public int idA_Rec;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idB_RecSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB_REC", Order = 17)]
        public int idB_Rec;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorReceiverLocalClassDervSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_REC_LOCALCLASSDERV", Order = 18)]
        public string actorReceiverLocalClassDerv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_Rec_EntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_REC_ENTITY", Order = 19)]
        public int idA_Rec_Entity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorReceiverEntityDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("REC_ENTITY_DISPLAYNAME", Order = 20)]
        public string actorReceiverEntityDisplayName;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
    public class TradeStSys
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDSTENVIRONMENT", Order = 1)]
        public string idStEnvironment;
        [System.Xml.Serialization.XmlElementAttribute("DTSTENVIRONMENT", Order = 2)]
        public DateTime dtStEnvironment;
        [System.Xml.Serialization.XmlElementAttribute("IDASTENVIRONMENT", Order = 3)]
        public int idAStEnvironment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStEnvironmentDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_STENVIRONMENT_DISPLAYNAME", Order = 4)]
        public string idAStEnvironmentDisplayName;

        [System.Xml.Serialization.XmlElementAttribute("IDSTPRIORITY", Order = 5)]
        public string idStPriority;
        [System.Xml.Serialization.XmlElementAttribute("DTSTPRIORITY", Order = 6)]
        public DateTime dtStPriority;
        [System.Xml.Serialization.XmlElementAttribute("IDASTPRIORITY", Order = 7)]
        public int idAStPriority;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStPriorityDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_STPRIORITY_DISPLAYNAME", Order = 8)]
        public string idAStPriorityDisplayName;

        [System.Xml.Serialization.XmlElementAttribute("IDSTACTIVATION", Order = 9)]
        public string idStActivation;
        [System.Xml.Serialization.XmlElementAttribute("DTSTACTIVATION", Order = 10)]
        public DateTime dtStActivation;
        [System.Xml.Serialization.XmlElementAttribute("IDASTACTIVATION", Order = 11)]
        public int idAStActivation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStActivationDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_STACTIVATION_DISPLAYNAME", Order = 12)]
        public string idAStActivationDisplayName;

        [System.Xml.Serialization.XmlElementAttribute("IDSTBUSINESS", Order = 13)]
        public string idStBusiness;
        [System.Xml.Serialization.XmlElementAttribute("DTSTBUSINESS", Order = 14)]
        public DateTime dtStBusiness;
        [System.Xml.Serialization.XmlElementAttribute("IDASTBUSINESS", Order = 15)]
        public int idAStBusiness;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStBusinessDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_STBUSINESS_DISPLAYNAME", Order = 16)]
        public string idABusinessDisplayName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTUSEDBY", Order = 17)]
        public string idStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSTUSEDBY", Order = 18)]
        public DateTime dtStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool libStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("LIBSTUSEDBY", Order = 19)]
        public string libStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStUsedBySpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDASTUSEDBY", Order = 20)]
        public int idAStUsedBy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStUsedByDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_STUSEDBY_DISPLAYNAME", Order = 21)]
        public string idAStUsedByDisplayName;
        #endregion Members
    }

    /// <summary>
    /// Classe destinée à la gestion des flags de messagerie de notification/Confirmation pour un trade
    /// </summary>
    public class TradeNotification
    {
        #region Members
        protected ActorNotification[] _partyNotification;
        #endregion

        #region Accessor
        /// <summary>
        /// Obtient ou définit les ActorNotification d'un trade (il en existe 2)
        /// </summary>
        public ActorNotification[] PartyNotification
        {
            get { return _partyNotification; }
            set { _partyNotification = value; }
        }
        #endregion Accessor

        #region Constructor
        public TradeNotification()
        {
            _partyNotification = new ActorNotification[2] { new ActorNotification(), new ActorNotification() };
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Initialisation à partir d'un trade 
        /// <para>Attention, l'ordre des items n'est pas nécessairement celui des parties du DataDocument</para>
        /// <para>L'ordre est aléatoire car fonction du jeu de résultat SQL obtenu</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public void InitializeFromTrade(string pCS, int pIdT)
        {
            InitializeFromTrade(pCS, null, pIdT);
        }
        /// <summary>
        /// Initialisation à partir d'un trade 
        /// <para>Attention, l'ordre des items n'est pas nécessairement celui des parties du DataDocument</para>
        /// <para>L'ordre est aléatoire car fonction du jeu de résultat SQL obtenu</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        public void InitializeFromTrade(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += @"t.IDT, t.IDENTIFIER, ta.IDA, ta.ISNCMINI, ta.ISNCMINT, ta.ISNCMFIN" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEACTOR + " ta" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " t on t.IDT = ta.IDT" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + @"(t.IDT = @IDT)";
            sqlSelect += SQLCst.AND + @"(ta.IDROLEACTOR = @IDROLEACTOR)";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.COUNTERPARTY.ToString());

            int i = 0;
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dp);
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    if (i > 1)
                    {
                        string identifier = dr["IDENTIFIER"].ToString();
                        throw new NotSupportedException(StrFunc.AppendFormat("Too many COUNTERPARTY for trade(identifier:{0})", identifier));
                    }

                    _partyNotification[i].Initialize(
                        Convert.ToInt32(dr["IDA"]),
                        BoolFunc.IsTrue(dr["ISNCMINI"]),
                        BoolFunc.IsTrue(dr["ISNCMINT"]),
                        BoolFunc.IsTrue(dr["ISNCMFIN"]));
                    i++;
                }
            }
        }

        /// <summary>
        /// Obtient true si un des identificateurs de confirmation est à true pour un acteur donné
        /// </summary>
        /// <param name="pIda">acteur</param>
        /// <returns></returns>
        public bool IsConfirmSpecified(int pIda)
        {

            bool ret = false;
            //
            ActorNotification notificationActor = GetActorNotification(pIda);
            if (null != notificationActor)
                ret = notificationActor.IsConfirmSpecified;
            //
            return ret;

        }

        /// <summary>
        /// Retourne dans la liste des types de messages activé pour l'acteur {pIdA}
        /// </summary>
        /// <param name="pIda"></param>
        /// <returns></returns>
        public NotificationStepLifeEnum[] GetStepLifeWithActiveConfirmation(int pIdA)
        {
            NotificationStepLifeEnum[] ret = null;
            ArrayList al = new ArrayList();
            //
            ActorNotification notificationActor = GetActorNotification(pIdA);
            if (null != notificationActor)
            {
                if (notificationActor.GetConfirm(NotificationStepLifeEnum.INITIAL))
                    al.Add(NotificationStepLifeEnum.INITIAL);
                if (notificationActor.GetConfirm(NotificationStepLifeEnum.INTERMEDIARY))
                    al.Add(NotificationStepLifeEnum.INTERMEDIARY);
                if (notificationActor.GetConfirm(NotificationStepLifeEnum.FINAL))
                    al.Add(NotificationStepLifeEnum.FINAL);
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (NotificationStepLifeEnum[])al.ToArray(typeof(NotificationStepLifeEnum));
            //
            return ret;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="pValue"></param>
        public void SetSetConfirmation(bool pValue)
        {
            for (int i = 0; i < ArrFunc.Count(PartyNotification); i++)
                PartyNotification[i].SetConfirmation(pValue);

        }

        /// <summary>
        /// Retourne l'ActorNotification associé à un acteur {pIda}
        /// <para>Retourne null si l'acteur n'est pas référencé</para>
        /// </summary>
        /// <param name="pIda">Représente l'acteur</param>
        /// <returns></returns>
        public ActorNotification GetActorNotification(int pIda)
        {
            ActorNotification ret = null;
            for (int i = 0; i < ArrFunc.Count(PartyNotification); i++)
            {
                if (PartyNotification[i].IdActor == pIda)
                {
                    ret = _partyNotification[i];
                    break;
                }
            }
            //
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// Classe destinée à la gestion des flags de messagerie de notification/Confirmation pour un acteur donnée
    /// </summary>
    public class ActorNotification
    {
        #region Member
        private int _idActor;
        private bool _isConfirmInitial;
        private bool _isConfirmInterim;
        private bool _isConfirmFinal;
        #endregion

        #region accessors
        /// <summary>
        /// Représente l'acteur auquel s'applique les indicateurs de "Confirmations"
        /// </summary>
        public int IdActor
        {
            get { return _idActor; }
            set { _idActor = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IdActorSpecified
        {
            get { return (IdActor > 0); }
        }
        /// <summary>
        /// Obtient true si un des indicateurs de confirmation est à true
        /// </summary>
        public bool IsConfirmSpecified
        {
            get { return (_isConfirmInitial || _isConfirmInterim || _isConfirmFinal); }
        }

        #endregion accessors

        #region Constructor
        public ActorNotification() :
            this(true) { }// Par défaut c'est true
        public ActorNotification(bool pIsConfirm) :
            this(-99, pIsConfirm, pIsConfirm, pIsConfirm) { }
        public ActorNotification(int pIda, bool pIsConfirmInitial, bool pIsConfirmInterim, bool pIsConfirmFinal)
        {
            Initialize(pIda, pIsConfirmInitial, pIsConfirmInterim, pIsConfirmFinal);
        }
        #endregion Constructor

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIsConfirmInitial"></param>
        /// <param name="pIsConfirmInterim"></param>
        /// <param name="pIsConfirmFinal"></param>
        public void Initialize(int pIda, bool pIsConfirmInitial, bool pIsConfirmInterim, bool pIsConfirmFinal)
        {
            _idActor = pIda;
            _isConfirmInitial = pIsConfirmInitial;
            _isConfirmInterim = pIsConfirmInterim;
            _isConfirmFinal = pIsConfirmFinal;
        }
        /// <summary>
        /// Définit l'indicateur "Confirmations" pour le Step spécifié
        /// </summary>
        /// <param name="pStepLife"></param>
        /// <param name="pNewValue"></param>
        public void SetConfirmation(NotificationStepLifeEnum pStepLife, bool pValue)
        {
            //            
            switch (pStepLife)
            {
                case NotificationStepLifeEnum.INITIAL:
                    _isConfirmInitial = pValue;
                    break;
                case NotificationStepLifeEnum.INTERMEDIARY:
                    _isConfirmInterim = pValue;
                    break;
                case NotificationStepLifeEnum.FINAL:
                    _isConfirmFinal = pValue;
                    break;
            }
        }
        /// <summary>
        ///  Définit tous les indicateurs "Confirmations" 
        /// </summary>
        /// <param name="pValue"></param>
        public void SetConfirmation(bool pValue)
        {
            _isConfirmInitial = pValue;
            _isConfirmInterim = pValue;
            _isConfirmFinal = pValue;
        }
        /// <summary>
        /// Retourne la valeur de l'indicateur "Confirmations" pour le Step spécifié
        /// </summary>
        /// <param name="pStepLife"></param>
        /// <returns></returns>
        public bool GetConfirm(NotificationStepLifeEnum pStepLife)
        {
            bool data = true; // par défaut tout est coché
            //
            switch (pStepLife)
            {
                case NotificationStepLifeEnum.INITIAL:
                    #region INITIAL
                    data = _isConfirmInitial;
                    #endregion INITIAL
                    break;
                case NotificationStepLifeEnum.INTERMEDIARY:
                    #region INTERMEDIARY
                    data = _isConfirmInterim;
                    #endregion INTERMEDIARY
                    break;
                case NotificationStepLifeEnum.FINAL:
                    #region FINAL
                    data = _isConfirmFinal;
                    #endregion FINAL
                    break;
            }
            //
            return data;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class DatasetTradeEvent
    {
        #region Members
        private readonly string _cs;
        private DataSetEvent _dsEvent;
        private DataSetTrade _dsTrade;
        private DataSet _ds;
        #endregion Members

        #region Public DtTrade
        public DataTable DtTrade
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("Trade"))
                    ret = _ds.Tables["Trade"];
                return ret;
            }
        }
        #endregion DtTrade
        #region Public DtEvent
        public DataTable DtEvent
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("Event"))
                    ret = _ds.Tables["Event"];
                return ret;
            }
        }
        #endregion DtEvent
        #region Public DtEventClass
        public DataTable DtEventClass
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("EventClass"))
                    ret = _ds.Tables["EventClass"];
                return ret;
            }
        }
        #endregion DtEventClass
        #region public DtEventAsset
        public DataTable DtEventAsset
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("EventAsset"))
                    ret = _ds.Tables["EventAsset"];
                return ret;
            }
        }
        #endregion DtEventAsset
        #region public DtEventDetails
        public DataTable DtEventDetails
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("EventDetails"))
                    ret = _ds.Tables["EventDetails"];
                return ret;
            }
        }
        #endregion DtEventDetails
        #region public DtEventPricing
        public DataTable DtEventPricing
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("EventPricing"))
                    ret = _ds.Tables["EventPricing"];
                return ret;
            }
        }
        #endregion DtEventPricing
        #region public DtEventProcess
        public DataTable DtEventProcess
        {
            get
            {
                DataTable ret = null;
                if (_ds.Tables.Contains("EventProcess"))
                    ret = _ds.Tables["EventProcess"];
                return ret;
            }
        }
        #endregion DtEventProcess

        #region constructor
        public DatasetTradeEvent(string pCs)
        {
            _cs = pCs;
        }
        #endregion constructor

        #region Load
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public void Load(int pIdT)
        {
            _dsTrade = new DataSetTrade(_cs);
            _dsTrade.Load(null, pIdT, 0);

            _dsEvent = new DataSetEvent(_cs);
            _dsEvent.Load(null, pIdT);

            _ds = new DataSet
            {
                DataSetName = "TradeExport"
            };

            DataTable[] dataTable;
            if (ArrFunc.IsFilled(_dsTrade.DataTable))
            {
                dataTable = _dsTrade.DataTable;
                for (int i = 0; i < ArrFunc.Count(dataTable); i++)
                    _ds.Tables.Add(dataTable[i].Copy());
            }
            if (ArrFunc.IsFilled(_dsEvent.DataTable))
            {
                dataTable = _dsEvent.DataTable;
                for (int i = 0; i < ArrFunc.Count(dataTable); i++)
                    _ds.Tables.Add(dataTable[i].Copy());
            }
            InitializeRelations();

        }

        #endregion

        #region public GetTradeExport
        public TradeExport GetTradeExport()
        {
            TradeExport ret = null;
            if ((null != _ds) && (0 < DtTrade.Rows.Count))
            {
                InitializeRelations();
                _ds.DataSetName = "TradeExport";
                //
                string serializeResult = new DatasetSerializer(_ds).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(TradeExport), serializeResult);
                ret = (TradeExport)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }
        #endregion public GetEventItems

        #region private InitializeRelations
        // EG 20141120 Suppression EventDetails_ETD
        private void InitializeRelations()
        {
            _ds.Relations.Clear();
            //
            if ((null != DtTrade) && (null != DtEvent))
            {
                DataRelation relTrade = new DataRelation(DtTrade.TableName, DtTrade.Columns["IDT"], DtEvent.Columns["IDT"], false)
                {
                    Nested = false
                };
                _ds.Relations.Add(relTrade);
            }
            //
            if (null != DtEvent)
            {
                DataRelation relEvent = new DataRelation(DtEvent.TableName, DtEvent.Columns["IDE"], DtEvent.Columns["IDE_EVENT"], false)
                {
                    Nested = true
                };
                _ds.Relations.Add(relEvent);
            }
            //
            if ((null != DtEventClass) && (null != DtEvent))
            {
                DataRelation relEventClass = new DataRelation(DtEventClass.TableName, DtEvent.Columns["IDE"], DtEventClass.Columns["IDE"], false)
                {
                    Nested = true
                };
                _ds.Relations.Add(relEventClass);
            }
            //
            if ((null != DtEventDetails) && (null != DtEvent))
            {
                DataRelation relEventDet = new DataRelation(DtEventDetails.TableName, DtEvent.Columns["IDE"], DtEventDetails.Columns["IDE"], false)
                {
                    Nested = true
                };
                _ds.Relations.Add(relEventDet);
            }
            //
            if ((null != DtEventAsset) && (null != DtEvent))
            {
                DataRelation relEventAsset = new DataRelation(DtEventAsset.TableName, DtEvent.Columns["IDE"], DtEventAsset.Columns["IDE"], false)
                {
                    Nested = true
                };
                _ds.Relations.Add(relEventAsset);
            }
            //
            if ((null != DtEventPricing) && (null != DtEvent))
            {
                DataRelation relEventPricing = new DataRelation(DtEventPricing.TableName, DtEvent.Columns["IDE"], DtEventPricing.Columns["IDE"], false)
                {
                    Nested = true
                };
                _ds.Relations.Add(relEventPricing);
            }
            if ((null != DtEventProcess) && (null != DtEvent))
            {
                DataRelation relEventProcess = new DataRelation(DtEventProcess.TableName, DtEvent.Columns["IDE"], DtEventProcess.Columns["IDE"], false)
                {
                    Nested = true
                };
                _ds.Relations.Add(relEventProcess);
            }
        }
        #endregion
    }

}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EFS.ApplicationBlocks.Data;
using EFS.SpheresIO;
using IOCompareCommon.Interfaces;

// Any data contract class reflects a specific dataset issue from an SQL request. 
// Any data member MUST be defined in the CompareParameters.xml file: to any data member Name corresponds a parameter Id.
// The Order of any data member must reflects the order with which you put the relative parameter in the CompareParameters.xml file.

namespace IOCompareCommon.DataContracts
{

    /// <summary>
    /// Compare element class for 'TradeFix' on Spheres
    /// </summary>
    /// <remarks>
    /// External data : EXTLDATA + EXTLDATADET
    /// Spheres data : TRADE
    /// </remarks>
    [DataContract(
        Name = DataHelper<TradeFixCompareData>.DATASETROWNAME,
        Namespace = DataHelper<TradeFixCompareData>.DATASETNAMESPACE)]
    public sealed class TradeFixCompareData : ISpheresComparer
    {

        private int m_idData = 0;

        [DataMember(Name = "IDDATA", Order = 1)]
        public int IdData
        {
            get { return m_idData; }
            set { m_idData = value; }
        }

        private string m_CollectedValues = null;

        [DataMember(Name = "COLLECTEDVALUES", Order = 2)]
        public string CollectedValues
        {
            get { return m_CollectedValues; }
            set { m_CollectedValues = value; }
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private decimal m_lastQty = 0;

        [DataMember(Name = "LASTQTY", Order = 3)]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal LastQty
        {
            get { return m_lastQty; }
            set { m_lastQty = value; }
        }

        private string m_Side = null;

        [DataMember(Name = "SIDE", Order = 4)]
        public string Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }

        private float m_LastPx = 0;

        [DataMember(Name = "LASTPX", Order = 5)]
        public float LastPx
        {
            get { return m_LastPx; }
            set { m_LastPx = value; }
        }

        private string m_MMY = null;

        [DataMember(Name = "MMY", Order = 6)]
        public string MMY
        {
            get { return m_MMY; }
            set { m_MMY = value; }
        }

        private string m_Sym = null;

        [DataMember(Name = "SYM", Order = 7)]
        public string Sym
        {
            get { return m_Sym; }
            set { m_Sym = value; }
        }

        private string m_Exch = null;

        [DataMember(Name = "EXCH", Order = 8)]
        public string Exch
        {
            get { return m_Exch; }
            set { m_Exch = value; }
        }

        private float m_StrkPx = 0;

        [DataMember(Name = "STRKPX", Order = 9)]
        public float StrkPx
        {
            get { return m_StrkPx; }
            set { m_StrkPx = value; }
        }

        private string m_PutCall = null;

        [DataMember(Name = "PUTCALL", Order = 10)]
        public string PutCall
        {
            get { return m_PutCall; }
            set { m_PutCall = value; }
        }

        private string m_TxnTm = null;

        [DataMember(Name = "TXNTM", Order = 11)]
        public string TxnTm
        {
            get { return m_TxnTm; }
            set { m_TxnTm = value; }
        }

        private string m_Acct = null;

        [DataMember(Name = "ACCT", Order = 12)]
        public string Acct
        {
            get { return m_Acct; }
            set { m_Acct = value; }
        }

        private string m_AcctTyp = null;

        [DataMember(Name = "ACCTTYP", Order = 13)]
        public string AcctTyp
        {
            get { return m_AcctTyp; }
            set { m_AcctTyp = value; }
        }

        private string m_PosEfct = null;

        [DataMember(Name = "POSEFCT", Order = 14)]
        public string PosEfct
        {
            get { return m_PosEfct; }
            set { m_PosEfct = value; }
        }

        private int m_dataRowNumber = 0;

        [DataMember(Name = "DATAROWNUMBER", Order = 15)]
        public int DataRowNumber
        {
            get { return m_dataRowNumber; }
            set { m_dataRowNumber = value; }
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>();

            if (Side != null)
                m_QuantityByKeyName.Add(Side, new ValueErrorStatus(LastQty,
                    Side == "1" ? MatchStatus.UNMATCH_LONGQTY : MatchStatus.UNMATCH_SHORTQTY));
        }

        public ValueErrorStatus[] Values
        {
            get
            {
                ValueErrorStatus[] temp = new ValueErrorStatus[m_QuantityByKeyName.Count];
                m_QuantityByKeyName.Values.CopyTo(temp, 0);

                return temp;
            }
        }

        public ValueErrorStatus QtyErrorStatusByKey(string checkSide)
        {
            ValueErrorStatus res;
            m_QuantityByKeyName.TryGetValue(checkSide, out res);

            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { Side, LastPx, MMY, Sym, Exch, StrkPx, PutCall, TxnTm, Acct, AcctTyp, PosEfct };

                return m_comparisonKey;
            }
        }

        public string Message
        {
            get { return CollectedValues; }
        }

        public object Id
        {
            get { return IdData; }
        }

        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        public string Typ_Compte_Eurosys
        {
            get { return null; }
        }

        #endregion
    }


    /// <summary>
    /// Compare element class for 'PositionFix' on Spheres
    /// </summary>
    /// <remarks>
    /// External data : EXTLDATA + EXTLDATADET
    /// Spheres data : ???
    /// </remarks>
    [DataContract(
        Name = DataHelper<PositionFixCompareData>.DATASETROWNAME,
        Namespace = DataHelper<PositionFixCompareData>.DATASETNAMESPACE)]
    public sealed class PositionFixCompareData : ISpheresComparer
    {

        private int m_idData = 0;

        [DataMember(Name = "IDDATA", Order = 1)]
        public int IdData
        {
            get { return m_idData; }
            set { m_idData = value; }
        }

        private string m_CollectedValues = null;

        [DataMember(Name = "COLLECTEDVALUES", Order = 2)]
        public string CollectedValues
        {
            get { return m_CollectedValues; }
            set { m_CollectedValues = value; }
        }

        private decimal m_Long = 0;

        [DataMember(Name = "LONG", Order = 3)]
        // RD 20170228 Qty int32 To Decimal
        public decimal Long
        {
            get { return m_Long; }
            set { m_Long = value; }
        }

        private decimal m_Short = 0;

        [DataMember(Name = "SHORT", Order = 4)]
        // RD 20170228 Qty int32 To Decimal
        public decimal Short
        {
            get { return m_Short; }
            set { m_Short = value; }
        }

        private string m_MMY = null;

        [DataMember(Name = "MMY", Order = 5)]
        public string MMY
        {
            get { return m_MMY; }
            set { m_MMY = value; }
        }

        private string m_Sym = null;

        [DataMember(Name = "SYM", Order = 6)]
        public string Sym
        {
            get { return m_Sym; }
            set { m_Sym = value; }
        }

        private string m_Exch = null;

        [DataMember(Name = "EXCH", Order = 7)]
        public string Exch
        {
            get { return m_Exch; }
            set { m_Exch = value; }
        }

        private float m_StrkPx = 0;

        [DataMember(Name = "STRKPX", Order = 8)]
        public float StrkPx
        {
            get { return m_StrkPx; }
            set { m_StrkPx = value; }
        }

        private string m_PutCall = null;

        [DataMember(Name = "PUTCALL", Order = 9)]
        public string PutCall
        {
            get { return m_PutCall; }
            set { m_PutCall = value; }
        }

        private string m_Acct = null;

        [DataMember(Name = "ACCT", Order = 10)]
        public string Acct
        {
            get { return m_Acct; }
            set { m_Acct = value; }
        }

        private string m_AcctTyp = null;

        [DataMember(Name = "ACCTTYP", Order = 11)]
        public string AcctTyp
        {
            get { return m_AcctTyp; }
            set { m_AcctTyp = value; }
        }

        private int m_dataRowNumber = 0;

        [DataMember(Name = "DATAROWNUMBER", Order = 12)]
        public int DataRowNumber
        {
            get { return m_dataRowNumber; }
            set { m_dataRowNumber = value; }
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>();
            m_QuantityByKeyName.Add("1", new ValueErrorStatus(Long, MatchStatus.UNMATCH_LONGQTY));
            m_QuantityByKeyName.Add("2", new ValueErrorStatus(Short, MatchStatus.UNMATCH_SHORTQTY));
        }

        public ValueErrorStatus[] Values
        {
            get
            {
                ValueErrorStatus[] temp = new ValueErrorStatus[m_QuantityByKeyName.Count];
                m_QuantityByKeyName.Values.CopyTo(temp, 0);

                return temp;
            }
        }

        public ValueErrorStatus QtyErrorStatusByKey(string checkSide)
        {
            ValueErrorStatus res;
            m_QuantityByKeyName.TryGetValue(checkSide, out res);

            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { MMY, Sym, Exch, StrkPx, PutCall, Acct, AcctTyp };

                return m_comparisonKey;
            }
        }

        public string Message
        {
            get { return CollectedValues; }
        }

        public object Id
        {
            get { return IdData; }
        }

        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        public string Typ_Compte_Eurosys
        {
            get { return null; }
        }

        #endregion
    }


    // 20110905 MF - Ticket 17443 
    /// <summary>
    /// Compare element class for 'TradesInPositionFix' on Spheres
    /// </summary>
    /// <remarks>
    /// External data : EXTLDATA + EXTLDATADET
    /// Spheres data : VW_TRADE_POSETD
    /// </remarks>
    [DataContract(
        Name = DataHelper<TradesInPositionFixCompareData>.DATASETROWNAME,
        Namespace = DataHelper<TradesInPositionFixCompareData>.DATASETNAMESPACE)]
    public sealed class TradesInPositionFixCompareData : ISpheresComparer
    {

        private int m_idData = 0;

        [DataMember(Name = "IDDATA", Order = 1)]
        public int IdData
        {
            get { return m_idData; }
            set { m_idData = value; }
        }

        private string m_CollectedValues = null;

        [DataMember(Name = "COLLECTEDVALUES", Order = 2)]
        public string CollectedValues
        {
            get { return m_CollectedValues; }
            set { m_CollectedValues = value; }
        }

        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private decimal m_lastQty = 0;

        [DataMember(Name = "LASTQTY", Order = 3)]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal LastQty
        {
            get { return m_lastQty; }
            set { m_lastQty = value; }
        }

        private string m_Side = null;

        [DataMember(Name = "SIDE", Order = 4)]
        public string Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }

        private float m_LastPx = 0;

        [DataMember(Name = "LASTPX", Order = 5)]
        public float LastPx
        {
            get { return m_LastPx; }
            set { m_LastPx = value; }
        }

        private string m_MMY = null;

        [DataMember(Name = "MMY", Order = 6)]
        public string MMY
        {
            get { return m_MMY; }
            set { m_MMY = value; }
        }

        private string m_Sym = null;

        [DataMember(Name = "SYM", Order = 7)]
        public string Sym
        {
            get { return m_Sym; }
            set { m_Sym = value; }
        }

        private string m_Exch = null;

        [DataMember(Name = "EXCH", Order = 8)]
        public string Exch
        {
            get { return m_Exch; }
            set { m_Exch = value; }
        }

        private float m_StrkPx = 0;

        [DataMember(Name = "STRKPX", Order = 9)]
        public float StrkPx
        {
            get { return m_StrkPx; }
            set { m_StrkPx = value; }
        }

        private string m_PutCall = null;

        [DataMember(Name = "PUTCALL", Order = 10)]
        public string PutCall
        {
            get { return m_PutCall; }
            set { m_PutCall = value; }
        }

        private string m_TxnTm = null;

        [DataMember(Name = "TXNTM", Order = 11)]
        public string TxnTm
        {
            get { return m_TxnTm; }
            set { m_TxnTm = value; }
        }

        private string m_Acct = null;

        [DataMember(Name = "ACCT", Order = 12)]
        public string Acct
        {
            get { return m_Acct; }
            set { m_Acct = value; }
        }

        private string m_AcctTyp = null;

        [DataMember(Name = "ACCTTYP", Order = 13)]
        public string AcctTyp
        {
            get { return m_AcctTyp; }
            set { m_AcctTyp = value; }
        }

        private string m_PosEfct = null;

        [DataMember(Name = "POSEFCT", Order = 14)]
        public string PosEfct
        {
            get { return m_PosEfct; }
            set { m_PosEfct = value; }
        }

        private int m_dataRowNumber = 0;

        [DataMember(Name = "DATAROWNUMBER", Order = 15)]
        public int DataRowNumber
        {
            get { return m_dataRowNumber; }
            set { m_dataRowNumber = value; }
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>();

            if (Side != null)
                m_QuantityByKeyName.Add(Side, new ValueErrorStatus(LastQty,
                    Side == "1" ? MatchStatus.UNMATCH_LONGQTY : MatchStatus.UNMATCH_SHORTQTY));
        }

        public ValueErrorStatus[] Values
        {
            get
            {
                ValueErrorStatus[] temp = new ValueErrorStatus[m_QuantityByKeyName.Count];
                m_QuantityByKeyName.Values.CopyTo(temp, 0);

                return temp;
            }
        }

        public ValueErrorStatus QtyErrorStatusByKey(string checkSide)
        {
            ValueErrorStatus res;
            m_QuantityByKeyName.TryGetValue(checkSide, out res);

            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { Side, LastPx, MMY, Sym, Exch, StrkPx, PutCall, TxnTm, Acct, AcctTyp, PosEfct };

                return m_comparisonKey;
            }
        }

        public string Message
        {
            get { return CollectedValues; }
        }

        public object Id
        {
            get { return IdData; }
        }

        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        public string Typ_Compte_Eurosys
        {
            get { return null; }
        }

        #endregion
    }

    /// <summary>
    /// Comparison class for cash flows grouped by book/currency
    /// </summary>
    /// External data : EXTLDATA (business type: CashFlow) join EXTLDATADET
    /// Spheres data : TRADE (Product cashBalance)
    [DataContract(
        Name = DataHelper<CashFlowsCompareData>.DATASETROWNAME,
        Namespace = DataHelper<CashFlowsCompareData>.DATASETNAMESPACE)]
    public sealed class CashFlowsCompareData : ISpheresComparer, IIoTrackElements, ISupportKey
    {

        [DataMember(Name = "IDDATA", Order = 1)]
        public int IdData
        {
            get;
            set;
        }

        [DataMember(Name = "COLLECTEDVALUES", Order = 2)]
        public string CollectedValues
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMAMT", Order = 3)]
        public decimal TaxComAmt
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMCCY", Order = 4)]
        public string TaxComCcy
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMACT", Order = 5)]
        public int TaxComAct
        {
            get;
            set;
        }

        [DataMember(Name = "COLLAMT", Order = 6)]
        public decimal CollAmt
        {
            get;
            set;
        }

        [DataMember(Name = "COLLCCY", Order = 7)]
        public string CollCcy
        {
            get;
            set;
        }

        [DataMember(Name = "COLLACT", Order = 8)]
        public int CollAct
        {
            get;
            set;
        }

        [DataMember(Name = "PMTAMT", Order = 9)]
        public decimal PmtAmt
        {
            get;
            set;
        }

        [DataMember(Name = "PMTCCY", Order = 10)]
        public string PmtCcy
        {
            get;
            set;
        }

        [DataMember(Name = "PMTACT", Order = 11)]
        public int PmtAct
        {
            get;
            set;
        }

        [DataMember(Name = "PRMAMT", Order = 12)]
        public decimal PrmAmt
        {
            get;
            set;
        }

        [DataMember(Name = "PRMCCY", Order = 13)]
        public string PrmCcy
        {
            get;
            set;
        }

        [DataMember(Name = "PRMACT", Order = 14)]
        public int PrmAct
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNAMT", Order = 15)]
        public decimal VrMrgnAmt
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNCCY", Order = 16)]
        public string VrMrgnCcy
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNACT", Order = 17)]
        public int VrMrgnAct
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKAMT", Order = 18)]
        public decimal TaxComBrkAmt
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKCCY", Order = 19)]
        public string TaxComBrkCcy
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKACT", Order = 20)]
        public int TaxComBrkAct
        {
            get;
            set;
        }

        [DataMember(Name = "CALLAMT", Order = 21)]
        public decimal CallAmt
        {
            get;
            set;
        }

        [DataMember(Name = "CALLCCY", Order = 22)]
        public string CallCcy
        {
            get;
            set;
        }

        [DataMember(Name = "CALLACT", Order = 23)]
        public int CallAct
        {
            get;
            set;
        }

        [DataMember(Name = "RPTAMT", Order = 24)]
        public decimal RptAmt
        {
            get;
            set;
        }

        [DataMember(Name = "RPTCCY", Order = 25)]
        public string RptCcy
        {
            get;
            set;
        }

        [DataMember(Name = "RPTACT", Order = 26)]
        public int RptAct
        {
            get;
            set;
        }

        // RD 20121022 [18112] 
        // - Add new amount CSBAMT            
        [DataMember(Name = "CSBAMT", Order = 27)]
        public decimal CSBAmt
        {
            get;
            set;
        }
                
        [DataMember(Name = "CSBACT", Order = 28)]
        public int CSBAct
        {
            get;
            set;
        }

        [DataMember(Name = "ACCT", Order = 29)]
        public string Acct
        {
            get;
            set;
        }

        [DataMember(Name = "ACCTTYP", Order = 30)]
        public string AcctTyp
        {
            get;
            set;
        }

        // MF 20120926 MF - Ticket 18149 - Adding epsilon/tolerance elements - START
        # region Epsilon

        [DataMember(Name = "TAXCOMEPSILON", Order = 31)]
        public decimal TaxComEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "COLLEPSILON", Order = 32)]
        public decimal CollEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "PMTEPSILON", Order = 33)]
        public decimal PmtEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "PRMEPSILON", Order = 34)]
        public decimal PrmEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNEPSILON", Order = 35)]
        public decimal VrMrgEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKEPSILON", Order = 36)]
        public decimal TaxComBrkEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "CALLEPSILON", Order = 37)]
        public decimal CallEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "RPTEPSILON", Order = 38)]
        public decimal RptEpsilon
        {
            get;
            set;
        }

        # endregion Epsilon

        // MF 20120926 MF - Ticket 18149 - END

        [DataMember(Name = "DATAROWNUMBER", Order = 39)]
        public int DataRowNumber
        {
            get;
            set;
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>();

            m_QuantityByKeyName.Add("TAXCOMAMT", 
                new ValueErrorStatus(TaxComAmt, MatchStatus.UNMATCH_TAXCOMAMT, TaxComAct > 0 ? true : false, TaxComEpsilon));
            m_QuantityByKeyName.Add("COLLAMT", 
                new ValueErrorStatus(CollAmt, MatchStatus.UNMATCH_COLLAMT, CollAct > 0 ? true: false, CollEpsilon));
            m_QuantityByKeyName.Add("PMTAMT", 
                new ValueErrorStatus(PmtAmt, MatchStatus.UNMATCH_PMTAMT, PmtAct > 0 ? true : false, PmtEpsilon));

            m_QuantityByKeyName.Add("PRMAMT", 
                new ValueErrorStatus(PrmAmt, MatchStatus.UNMATCH_PRMAMT, PrmAct > 0 ? true : false, PrmEpsilon));
            m_QuantityByKeyName.Add("VRMRGNAMT", 
                new ValueErrorStatus(VrMrgnAmt, MatchStatus.UNMATCH_VRMRGNAMT, VrMrgnAct > 0 ? true : false, this.VrMrgEpsilon));
            m_QuantityByKeyName.Add("TAXCOMBRKAMT", 
                new ValueErrorStatus(TaxComBrkAmt, MatchStatus.UNMATCH_TAXCOMBRKAMT, TaxComBrkAct > 0 ? true : false, this.TaxComBrkEpsilon));
            m_QuantityByKeyName.Add("CALLAMT",
                new ValueErrorStatus(CallAmt, MatchStatus.UNMATCH_CALLAMT, CallAct > 0 ? true : false, this.CallEpsilon));
            m_QuantityByKeyName.Add("RPTAMT",
                new ValueErrorStatus(RptAmt, MatchStatus.UNMATCH_RPTAMT, RptAct > 0 ? true : false, this.RptEpsilon));
            m_QuantityByKeyName.Add("CSBAMT",
                new ValueErrorStatus(CSBAmt, MatchStatus.UNMATCH_RPTAMT, CSBAct > 0 ? true : false, this.RptEpsilon));
        }

        public ValueErrorStatus[] Values
        {
            get
            {
                ValueErrorStatus[] temp = new ValueErrorStatus[m_QuantityByKeyName.Count];
                m_QuantityByKeyName.Values.CopyTo(temp, 0);

                return temp;
            }
        }

        public ValueErrorStatus QtyErrorStatusByKey(string quantiType)
        {
            ValueErrorStatus res;
            m_QuantityByKeyName.TryGetValue(quantiType, out res);

            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { TaxComCcy, CollCcy, PmtCcy, PrmCcy, VrMrgnCcy, TaxComBrkCcy, CallCcy, RptCcy, Acct, AcctTyp };

                return m_comparisonKey;
            }
        }

        public string Message
        {
            get { return CollectedValues; }
        }

        public object Id
        {
            get { return IdData; }
        }

        [Obsolete("EUROSYS specific", true)]
        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        [Obsolete("EUROSYS specific", true)]
        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        [Obsolete("EUROSYS specific", true)]
        public string Typ_Compte_Eurosys
        {
            get { return null; }
        }

        #endregion

        #region IIoTrackElements Membres

        public string[] GetKeyElements()
        {
            return new string[] { "TAXCOMAMT", "COLLAMT", "PMTAMT", "PRMAMT", "VRMRGNAMT", "TAXCOMBRKAMT", "CALLAMT", "RPTAMT", "CSBAMT" };
        }

        #endregion

        #region ISupportKey Membres

        public object SupportValueId
        {
            get { return null; }
        }

        public object GetSupportValueIdByElem(string pElemName)
        {
            string SupportId = null;

            switch (pElemName)
            {
                case "TAXCOMAMT":

                    SupportId = TaxComCcy;

                    break;
                case "COLLAMT":

                    SupportId = CollCcy;

                    break;
                case "PMTAMT":

                    SupportId = PmtCcy;

                    break;

                case "PRMAMT":

                    SupportId = PrmCcy;

                    break;

                case "VRMRGNAMT":

                    SupportId = VrMrgnCcy;

                    break;

                case "TAXCOMBRKAMT":

                    SupportId = TaxComBrkCcy;

                    break;

                case "CALLAMT":

                    SupportId = CallCcy;

                    break;

                case "RPTAMT":
                case "CSBAMT":

                    SupportId = RptCcy;

                    break;

                default:
                    break;
            }

            return SupportId;
        }

        #endregion
    }

    /// <summary>
    /// Comparison class for cash flows grouped by product
    /// </summary>
    /// External data : EXTLDATA (business type: CashFlowsInstr) join EXTLDATADET
    /// Spheres data : ..
    [DataContract(
        Name = DataHelper<CashFlowsInstrCompareData>.DATASETROWNAME,
        Namespace = DataHelper<CashFlowsInstrCompareData>.DATASETNAMESPACE)]
    public sealed class CashFlowsInstrCompareData : ISpheresComparer, IIoTrackElements, ISupportKey
    {

        [DataMember(Name = "IDDATA", Order = 1)]
        public int IdData
        {
            get;
            set;
        }

        [DataMember(Name = "COLLECTEDVALUES", Order = 2)]
        public string CollectedValues
        {
            get;
            set;
        }

        [DataMember(Name = "SYM", Order = 3)]
        public string Sym
        {
            get;
            set;
        }


        [DataMember(Name = "MRK", Order = 4)]
        public string Mrk
        {
            get;
            set;
        }

        [DataMember(Name = "PUTCALL", Order = 5)]
        public string PutCall
        {
            get;
            set;
        }


        [DataMember(Name = "UMGAMT", Order = 6)]
        public decimal UMgAmt
        {
            get;
            set;
        }

        [DataMember(Name = "UMGCCY", Order = 7)]
        public string UMgCcy
        {
            get;
            set;
        }

        [DataMember(Name = "UMGACT", Order = 8)]
        public int UMgAct
        {
            get;
            set;
        }

        [DataMember(Name = "PRMAMT", Order = 9)]
        public decimal PrmAmt
        {
            get;
            set;
        }

        [DataMember(Name = "PRMCCY", Order = 10)]
        public string PrmCcy
        {
            get;
            set;
        }

        [DataMember(Name = "PRMACT", Order = 11)]
        public int PrmAct
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNAMT", Order = 12)]
        public decimal VrMrgnAmt
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNCCY", Order = 13)]
        public string VrMrgnCcy
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNACT", Order = 14)]
        public int VrMrgnAct
        {
            get;
            set;
        }

        [DataMember(Name = "LOVAMT", Order = 15)]
        public decimal LovAmt
        {
            get;
            set;
        }

        [DataMember(Name = "LOVCCY", Order = 16)]
        public string LovCcy
        {
            get;
            set;
        }

        [DataMember(Name = "LOVACT", Order = 17)]
        public int LovAct
        {
            get;
            set;
        }

        [DataMember(Name = "RMGAMT", Order = 18)]
        public decimal RMgAmt
        {
            get;
            set;
        }

        [DataMember(Name = "RMGCCY", Order = 19)]
        public string RMgCcy
        {
            get;
            set;
        }

        [DataMember(Name = "RMGACT", Order = 20)]
        public int RMgAct
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKAMT", Order = 21)]
        public decimal TaxComBrkAmt
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKCCY", Order = 22)]
        public string TaxComBrkCcy
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKACT", Order = 23)]
        public int TaxComBrkAct
        {
            get;
            set;
        }

        [DataMember(Name = "ACCT", Order = 24)]
        public string Acct
        {
            get;
            set;
        }

        [DataMember(Name = "ACCTTYP", Order = 25)]
        public string AcctTyp
        {
            get;
            set;
        }

        // MF 20120926 MF - Ticket 18149 - Adding epsilon/tolerance elements - START
        # region Epsilon

        [DataMember(Name = "UMGEPSILON", Order = 26)]
        public decimal UMgEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "PRMEPSILON", Order = 27)]
        public decimal PrmEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "VRMRGNEPSILON", Order = 28)]
        public decimal VrMrgEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "LOVEPSILON", Order = 29)]
        public decimal LovEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "RMGEPSILON", Order = 30)]
        public decimal RMgEpsilon
        {
            get;
            set;
        }

        [DataMember(Name = "TAXCOMBRKEPSILON", Order = 31)]
        public decimal TaxComBrkEpsilon
        {
            get;
            set;
        }

        # endregion Epsilon

        // MF 20120926 MF - Ticket 18149 - END

        [DataMember(Name = "DATAROWNUMBER", Order = 32)]
        public int DataRowNumber
        {
            get;
            set;
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>();

            m_QuantityByKeyName.Add("UMGAMT", 
                new ValueErrorStatus(UMgAmt, MatchStatus.UNMATCH_UMGAMT, UMgAct > 0 ? true : false, this.UMgEpsilon));
            m_QuantityByKeyName.Add("PRMAMT", 
                new ValueErrorStatus(PrmAmt, MatchStatus.UNMATCH_PRMAMT, PrmAct > 0 ? true : false, this.PrmEpsilon));
            m_QuantityByKeyName.Add("VRMRGNAMT", 
                new ValueErrorStatus(VrMrgnAmt, MatchStatus.UNMATCH_VRMRGNAMT, VrMrgnAct > 0 ? true : false, this.VrMrgEpsilon));
            m_QuantityByKeyName.Add("LOVAMT", 
                new ValueErrorStatus(LovAmt, MatchStatus.UNMATCH_LOVAMT, LovAct > 0 ? true : false, this.LovEpsilon));
            m_QuantityByKeyName.Add("RMGAMT",
                new ValueErrorStatus(RMgAmt, MatchStatus.UNMATCH_RMGAMT, RMgAct > 0 ? true : false, this.RMgEpsilon));
            m_QuantityByKeyName.Add("TAXCOMBRKAMT", 
                new ValueErrorStatus(TaxComBrkAmt, MatchStatus.UNMATCH_TAXCOMBRKAMT, TaxComBrkAct > 0 ? true : false, this.TaxComBrkEpsilon));
        }

        public ValueErrorStatus[] Values
        {
            get
            {
                ValueErrorStatus[] temp = new ValueErrorStatus[m_QuantityByKeyName.Count];
                m_QuantityByKeyName.Values.CopyTo(temp, 0);

                return temp;
            }
        }

        public ValueErrorStatus QtyErrorStatusByKey(string quantiType)
        {
            ValueErrorStatus res;
            m_QuantityByKeyName.TryGetValue(quantiType, out res);

            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { UMgCcy, PrmCcy, VrMrgnCcy, LovCcy, RMgCcy, TaxComBrkCcy, Acct, AcctTyp, Sym, Mrk, PutCall };

                return m_comparisonKey;
            }
        }

        public string Message
        {
            get { return CollectedValues; }
        }

        public object Id
        {
            get { return IdData; }
        }

        [Obsolete("EUROSYS specific", true)]
        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        [Obsolete("EUROSYS specific", true)]
        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        [Obsolete("EUROSYS specific", true)]
        public string Typ_Compte_Eurosys
        {
            get { return null; }
        }

        #endregion

        #region IIoTrackElements Membres

        public string[] GetKeyElements()
        {
            return new string[] { "UMGAMT", "PRMAMT", "VRMRGNAMT", "LOVAMT", "RMGAMT", "TAXCOMBRKAMT" };
        }

        #endregion

        #region ISupportKey Membres

        /// <summary>
        /// Get the account name, it is the additional key, used together with the IdData field.
        /// </summary>
        public object SupportValueId
        {
            get { return this.Acct; }
        }

        /// <summary>
        /// Get the account name, it is the additional key, used together with the IdData field.
        /// </summary>
        public object GetSupportValueIdByElem(string pElemName)
        {
            return SupportValueId;
        }

        #endregion
    }

}

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
    /// Compare element class for 'TradeFix' on Eurosys
    /// </summary>
    /// <remarks>
    /// External data : EXTLDATA + EXTLDATADET
    /// EUROSYS/Vision  data : VUE_ARCH_NEGO
    /// </remarks>
    [DataContract(
        Name = DataHelper<TradeFixCompareDataEurosys>.DATASETROWNAME,
        Namespace = DataHelper<TradeFixCompareDataEurosys>.DATASETNAMESPACE)]
    public sealed class TradeFixCompareDataEurosys : ISpheresComparer
    {

        private string m_idData = String.Empty;

        [DataMember(Name = "IDDATA", Order = 1)]
        public string IdData
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

        private DateTime m_Age_Enreg;

        [DataMember(Name = "AGE_ENREG", Order = 11)]
        public DateTime Age_Enreg
        {
            get { return m_Age_Enreg; }
            set { m_Age_Enreg = value; }
        }

        private string m_Time_Enreg;

        [DataMember(Name = "TIME_ENREG", Order = 12)]
        public string Time_Enreg
        {
            get { return m_Time_Enreg; }
            set { m_Time_Enreg = value; }
        }

        private string m_Typ_Compte;

        [DataMember(Name = "TYP_COMPTE", Order = 13)]
        public string Typ_Compte
        {
            get { return m_Typ_Compte; }
            set { m_Typ_Compte = value; }
        }

        private string m_TxnTm = null;

        [DataMember(Name = "TXNTM", Order = 14)]
        public string TxnTm
        {
            get { return m_TxnTm; }
            set { m_TxnTm = value; }
        }

        private string m_Acct = null;

        [DataMember(Name = "ACCT", Order = 15)]
        public string Acct
        {
            get { return m_Acct; }
            set { m_Acct = value; }
        }

        private string m_AcctTyp = null;

        [DataMember(Name = "ACCTTYP", Order = 16)]
        public string AcctTyp
        {
            get { return m_AcctTyp; }
            set { m_AcctTyp = value; }
        }

        private string m_PosEfct = null;

        [DataMember(Name = "POSEFCT", Order = 17)]
        public string PosEfct
        {
            get { return m_PosEfct; }
            set { m_PosEfct = value; }
        }

        private int m_dataRowNumber = 0;

        [DataMember(Name = "DATAROWNUMBER", Order = 18)]
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
            m_QuantityByKeyName.TryGetValue(checkSide, out ValueErrorStatus res);
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
            get { return Age_Enreg; }
        }

        public string Time_Enreg_Eurosys
        {
            get { return Time_Enreg; }
        }

        public string Typ_Compte_Eurosys
        {
            get { return Typ_Compte; }
        }

        #endregion
    }

    /// <summary>
    /// Compare element class for 'PositionFix' on Eurosys
    /// </summary>
    /// <remarks>
    /// External data : EXTLDATA + EXTLDATADET
    /// EUROSYS/Vision data : VUE_ARCH_POS_OUV
    /// </remarks>
    [DataContract(
        Name = DataHelper<PositionFixCompareDataEurosys>.DATASETROWNAME,
        Namespace = DataHelper<PositionFixCompareDataEurosys>.DATASETNAMESPACE)]
    public sealed class PositionFixCompareDataEurosys : ISpheresComparer
    {

        private string m_idData = String.Empty;

        [DataMember(Name = "IDDATA", Order = 1)]
        public string IdData
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

        private DateTime m_Age_Enreg;

        [DataMember(Name = "AGE_ENREG", Order = 10)]
        public DateTime Age_Enreg
        {
            get { return m_Age_Enreg; }
            set { m_Age_Enreg = value; }
        }

        private string m_Time_Enreg;

        [DataMember(Name = "TIME_ENREG", Order = 11)]
        public string Time_Enreg
        {
            get { return m_Time_Enreg; }
            set { m_Time_Enreg = value; }
        }

        private string m_Typ_Compte;

        [DataMember(Name = "TYP_COMPTE", Order = 12)]
        public string Typ_Compte
        {
            get { return m_Typ_Compte; }
            set { m_Typ_Compte = value; }
        }

        private string m_Acct = null;

        [DataMember(Name = "ACCT", Order = 13)]
        public string Acct
        {
            get { return m_Acct; }
            set { m_Acct = value; }
        }

        private string m_AcctTyp = null;

        [DataMember(Name = "ACCTTYP", Order = 14)]
        public string AcctTyp
        {
            get { return m_AcctTyp; }
            set { m_AcctTyp = value; }
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
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>
            {
                { "1", new ValueErrorStatus(Long, MatchStatus.UNMATCH_LONGQTY) },
                { "2", new ValueErrorStatus(Short, MatchStatus.UNMATCH_SHORTQTY) }
            };
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
            m_QuantityByKeyName.TryGetValue(checkSide, out ValueErrorStatus res);
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
            get { return Age_Enreg; }
        }

        public string Time_Enreg_Eurosys
        {
            get { return Time_Enreg; }
        }

        public string Typ_Compte_Eurosys
        {
            get { return Typ_Compte; }
        }

        #endregion
    }

    // 20110905 MF - Ticket 17443 
    /// <summary>
    /// Compare element class for 'TradesInPositionFix' on Eurosys
    /// </summary>
    /// <remarks>
    /// External data : EXTLDATA + EXTLDATADET
    /// EUROSYS/Vision data : VUE_ARCH_POS_OUV
    /// </remarks>
    [DataContract(
        Name = DataHelper<TradesInPositionFixCompareDataEurosys>.DATASETROWNAME,
        Namespace = DataHelper<TradesInPositionFixCompareDataEurosys>.DATASETNAMESPACE)]
    public sealed class TradesInPositionFixCompareDataEurosys : ISpheresComparer
    {
        private string m_idData = String.Empty;

        [DataMember(Name = "IDDATA", Order = 1)]
        public string IdData
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

        private DateTime m_Age_Enreg;

        [DataMember(Name = "AGE_ENREG", Order = 11)]
        public DateTime Age_Enreg
        {
            get { return m_Age_Enreg; }
            set { m_Age_Enreg = value; }
        }

        private string m_Time_Enreg;

        [DataMember(Name = "TIME_ENREG", Order = 12)]
        public string Time_Enreg
        {
            get { return m_Time_Enreg; }
            set { m_Time_Enreg = value; }
        }

        private string m_Typ_Compte;

        [DataMember(Name = "TYP_COMPTE", Order = 13)]
        public string Typ_Compte
        {
            get { return m_Typ_Compte; }
            set { m_Typ_Compte = value; }
        }

        private string m_TxnTm = null;

        [DataMember(Name = "TXNTM", Order = 14)]
        public string TxnTm
        {
            get { return m_TxnTm; }
            set { m_TxnTm = value; }
        }

        private string m_Acct = null;

        [DataMember(Name = "ACCT", Order = 15)]
        public string Acct
        {
            get { return m_Acct; }
            set { m_Acct = value; }
        }

        private string m_AcctTyp = null;

        [DataMember(Name = "ACCTTYP", Order = 16)]
        public string AcctTyp
        {
            get { return m_AcctTyp; }
            set { m_AcctTyp = value; }
        }

        private string m_PosEfct = null;

        [DataMember(Name = "POSEFCT", Order = 17)]
        public string PosEfct
        {
            get { return m_PosEfct; }
            set { m_PosEfct = value; }
        }

        private int m_dataRowNumber = 0;

        [DataMember(Name = "DATAROWNUMBER", Order = 18)]
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
            m_QuantityByKeyName.TryGetValue(checkSide, out ValueErrorStatus res);
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
            get { return Age_Enreg; }
        }

        public string Time_Enreg_Eurosys
        {
            get { return Time_Enreg; }
        }

        public string Typ_Compte_Eurosys
        {
            get { return Typ_Compte; }
        }

        #endregion
    }

    /// <summary>
    /// Comparison class for cash flows grouped by book/currency
    /// </summary>
    /// External data : EXTLDATA (business type: CashFlow) join EXTLDATADET
    /// EUROSYS/Vision data : ..
    [DataContract(
        Name = DataHelper<CashFlowsCompareDataEurosys>.DATASETROWNAME,
        Namespace = DataHelper<CashFlowsCompareDataEurosys>.DATASETNAMESPACE)]
    public sealed class CashFlowsCompareDataEurosys : ISpheresComparer, IIoTrackElements
    {

        [DataMember(Name = "IDDATA", Order = 1)]
        public string IdData
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

        [DataMember(Name = "COLLAMT", Order = 5)]
        public decimal CollAmt
        {
            get;
            set;
        }

        [DataMember(Name = "COLLCCY", Order = 6)]
        public string CollCcy
        {
            get;
            set;
        }

        [DataMember(Name = "PMTAMT", Order = 7)]
        public decimal PmtAmt
        {
            get;
            set;
        }

        [DataMember(Name = "PMTCCY", Order = 8)]
        public string PmtCcy
        {
            get;
            set;
        }

        [DataMember(Name = "ACCT", Order = 13)]
        public string Acct
        {
            get;
            set;
        }

        [DataMember(Name = "ACCTTYP", Order = 14)]
        public string AcctTyp
        {
            get;
            set;
        }

        [DataMember(Name = "DATAROWNUMBER", Order = 15)]
        public int DataRowNumber
        {
            get;
            set;
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>
            {
                { "TAXCOMAMT", new ValueErrorStatus(TaxComAmt, MatchStatus.UNMATCH_TAXCOMAMT) },
                { "COLLAMT", new ValueErrorStatus(CollAmt, MatchStatus.UNMATCH_COLLAMT) },
                { "PMTAMT", new ValueErrorStatus(PmtAmt, MatchStatus.UNMATCH_PMTAMT) },
                {"PRMAMT",new ValueErrorStatus(0, MatchStatus.UNMATCH_PRMAMT, false)},
                {"VRMRGNAMT",new ValueErrorStatus(0, MatchStatus.UNMATCH_VRMRGNAMT, false)},
                {"TAXCOMBRKAMT",new ValueErrorStatus(0, MatchStatus.UNMATCH_TAXCOMBRKAMT, false)},
                {"CALLAMT",new ValueErrorStatus(0, MatchStatus.UNMATCH_CALLAMT, false)},
                {"RPTAMT",new ValueErrorStatus(0, MatchStatus.UNMATCH_RPTAMT, false)}
            };
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
            m_QuantityByKeyName.TryGetValue(quantiType, out ValueErrorStatus res);
            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { TaxComCcy, CollCcy, PmtCcy, null, null, null, null, null, Acct, AcctTyp };

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

        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        public string Typ_Compte_Eurosys
        {
            get { return null; }
        }

        #endregion

        #region IIoTrackElements Membres

        public string[] GetKeyElements()
        {
            return new string[] { "TAXCOMAMT", "COLLAMT", "PMTAMT", "PRMAMT", "VRMRGNAMT", "TAXCOMBRKAMT", "CALLAMT", "RPTAMT" };
        }

        #endregion
    }

    /// <summary>
    /// Comparison class for cash flows grouped by product
    /// </summary>
    /// External data : EXTLDATA (business type: CashFlowsInstr) join EXTLDATADET
    /// EUROSYS/Vision data : ..
    [DataContract(
        Name = DataHelper<CashFlowsInstrCompareDataEurosys>.DATASETROWNAME,
        Namespace = DataHelper<CashFlowsInstrCompareDataEurosys>.DATASETNAMESPACE)]
    public sealed class CashFlowsInstrCompareDataEurosys : ISpheresComparer, IIoTrackElements
    {

        [DataMember(Name = "IDDATA", Order = 1)]
        public string IdData
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

        [DataMember(Name = "ACCT", Order = 14)]
        public string Acct
        {
            get;
            set;
        }

        [DataMember(Name = "ACCTTYP", Order = 15)]
        public string AcctTyp
        {
            get;
            set;
        }


        [DataMember(Name = "DATAROWNUMBER", Order = 16)]
        public int DataRowNumber
        {
            get;
            set;
        }

        #region ISpheresComparer Membres

        Dictionary<string, ValueErrorStatus> m_QuantityByKeyName = null;

        public void Initialise()
        {
            m_QuantityByKeyName = new Dictionary<string, ValueErrorStatus>
            {
                { "UMGAMT", new ValueErrorStatus(UMgAmt, MatchStatus.UNMATCH_UMGAMT, true) },
                { "PRMAMT", new ValueErrorStatus(0, MatchStatus.UNMATCH_PRMAMT, false) },
                { "VRMRGNAMT", new ValueErrorStatus(0, MatchStatus.UNMATCH_VRMRGNAMT, false) },
                { "LOVAMT", new ValueErrorStatus(0, MatchStatus.UNMATCH_LOVAMT, false) },
                { "RMGAMT", new ValueErrorStatus(0, MatchStatus.UNMATCH_RMGAMT, false) },
                { "TAXCOMBRKAMT", new ValueErrorStatus(0, MatchStatus.UNMATCH_TAXCOMBRKAMT, false) }
            };
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
            m_QuantityByKeyName.TryGetValue(quantiType, out ValueErrorStatus res);
            return res;
        }

        object[] m_comparisonKey;

        public object[] ComparisonKey
        {
            get
            {
                if (m_comparisonKey == null)
                    m_comparisonKey = new object[] { UMgCcy, null, null, null, null, null, Acct, AcctTyp, Sym, Mrk, PutCall };

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

        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        public DateTime Age_Enreg_Eurosys
        {
            get { return DateTime.MinValue; }
        }

        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        public string Time_Enreg_Eurosys
        {
            get { return null; }
        }

        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
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
    }

}

#region Debug Directives
// Pour debugguer pas à pas ce module mettre un 1er point d'arrêt sur la méthode suivante:
//      class AccountGenProcess
//      method ProcessEarDetails()
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Status;
using EFS.Tuning;
//
using EfsML.Business;
using EfsML.EarAcc;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.v30.Fix;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

// RD 20120809 [18070] Optimisation de la compta
namespace EFS.SpheresAccounting
{
    #region public class AccQuery
    // RD 20140916 [20328] Add parameters paramIDE, paramEARTYPE and paramIDEARTYPE
    // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
    public class AccQuery
    {
        public string CS;
        public DataParameter paramIdT;
        public DataParameter paramIdEAR;
        public DataParameter paramIdB;

        public DataParameter paramDtProcess;
        public DataParameter paramIdAccModel;
        public DataParameter paramIdAccInstrEnvDet;
        public DataParameter paramIdAccCondition;

        public DataParameter paramInstrumentNo;
        public DataParameter paramStreamNo;
        public DataParameter paramNoZero;

        public DataParameter paramEarType;
        public DataParameter paramIdEarType;
        public DataParameter paramEarCode;
        public DataParameter paramAmountType;
        public DataParameter paramEventClass;

        #region Entry Parameters
        public DataParameter paramIDACCDAYBOOK;
        public DataParameter paramIDACCDAYBOOK_SRC;
        public DataParameter paramIDACCSCHEME;
        public DataParameter paramIDACCENTRY;
        public DataParameter paramIDSESSION;

        public DataParameter paramIDE;
        public DataParameter paramIDA_ENTITY;
        public DataParameter paramIDA_PARTY;
        public DataParameter paramIDB_PARTY;
        public DataParameter paramIDA_COUNTERPARTY;
        public DataParameter paramIDB_COUNTERPARTY;
        public DataParameter paramDTEVENT;
        public DataParameter paramDTEAR;
        public DataParameter paramDTENTRY;
        public DataParameter paramDTACCDAYBOOK;
        public DataParameter paramIDSTACTIVATION;
        //
        public DataParameter paramCASHSECURITIES;
        public DataParameter paramEXCHANGETYPE;
        public DataParameter paramQUOTEEXCHANGETYPE;
        //
        public DataParameter paramAMOUNT;
        public DataParameter paramIDC;
        public DataParameter paramISDEBIT;
        public DataParameter paramACCOUNT;
        public DataParameter paramACCOUNTVALUE;
        //
        public DataParameter paramLABEL;
        public DataParameter paramLABELVALUE;
        public DataParameter paramDEACTIVLABEL;
        public DataParameter paramDEACTIVLABELVALUE;
        public DataParameter paramDEACTIVPOS;
        public DataParameter paramREVERSALLABEL;
        public DataParameter paramREVERSALLABELVALUE;
        public DataParameter paramREVERSALPOS;
        //
        public DataParameter paramLABEL2;
        public DataParameter paramLABELVALUE2;
        public DataParameter paramDEACTIVLABEL2;
        public DataParameter paramDEACTIVLABELVALUE2;
        public DataParameter paramDEACTIVPOS2;
        public DataParameter paramREVERSALLABEL2;
        public DataParameter paramREVERSALLABELVALUE2;
        public DataParameter paramREVERSALPOS2;
        //
        public DataParameter paramJOURNALCODE;
        public DataParameter paramJOURNALCODEVALUE;
        public DataParameter paramCONSOCODE;
        public DataParameter paramCONSOCODEVALUE;
        //
        public DataParameter paramACCEXTLLINK;
        public DataParameter paramACCEXTLLINKVALUE;
        public DataParameter paramIDAINS;

        public DataParameter paramREVERSEDTENTRY;
        public DataParameter paramREVERSEISDEBIT;
        public DataParameter paramREVERSEIDENTRY_SRC;
        public DataParameter paramNEWLABELVALUE;
        public DataParameter paramNEWLABELVALUE2;
        #endregion

        public string SQL_Select_EarAccModel =
@"select IDEAR, IDACCMODEL, ISIGNORED, DTINS, IDAINS
from dbo.EAR_ACCMODEL";

        // RD 20140916 [20328] Add EARTYPE and IDEARTYPE
        // FI 20200820 [XXXXXX] Date systeme en UTC (use of getutcdate())
        public string SQL_Insert_Entry = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ACCDAYBOOK.ToString() + @"
(IDACCDAYBOOK, IDACCDAYBOOK_SRC, IDACCSCHEME, IDACCENTRY, IDSESSION, IDEAR, INSTRUMENTNO, STREAMNO, EARTYPE, IDEARTYPE, 
IDT, IDA_ENTITY, IDA_PARTY, IDB_PARTY,
IDA_COUNTERPARTY, IDB_COUNTERPARTY, DTEVENT, DTEAR, DTENTRY, DTACCDAYBOOK, DTSYS, IDSTACTIVATION, CASHSECURITIES, 
EARCODE, EVENTCLASS, EXCHANGETYPE, QUOTEEXCHANGETYPE, AMOUNTTYPE, AMOUNT, IDC, ISDEBIT, ACCOUNT, ACCOUNTVALUE, 
LABEL, LABELVALUE, DEACTIVLABEL, DEACTIVPOS, DEACTIVLABELVALUE, REVERSALLABEL, REVERSALPOS,
LABEL2, LABELVALUE2, DEACTIVLABEL2, DEACTIVPOS2, DEACTIVLABELVALUE2, REVERSALLABEL2, REVERSALPOS2, 
JOURNALCODE, JOURNALCODEVALUE, CONSOCODE, CONSOCODEVALUE,  
ACCEXTLLINK, ACCEXTLLINKVALUE, DTINS, IDAINS) 
values 
(@IDACCDAYBOOK, @IDACCDAYBOOK_SRC, @IDACCSCHEME, @IDACCENTRY, @IDSESSION, @IDEAR, @INSTRUMENTNO, @STREAMNO, @EARTYPE, @IDEARTYPE, 
@IDT, @IDA_ENTITY, @IDA_PARTY, @IDB_PARTY,
@IDA_COUNTERPARTY, @IDB_COUNTERPARTY, @DTEVENT, @DTEAR, @DTENTRY, @DTACCDAYBOOK, getutcdate(), @IDSTACTIVATION, @CASHSECURITIES, 
@EARCODE, @EVENTCLASS, @EXCHANGETYPE, @QUOTEEXCHANGETYPE, @AMOUNTTYPE, @AMOUNT, @IDC, @ISDEBIT, @ACCOUNT, @ACCOUNTVALUE, 
@LABEL, @LABELVALUE, @DEACTIVLABEL, @DEACTIVPOS, @DEACTIVLABELVALUE, @REVERSALLABEL, @REVERSALPOS,
@LABEL2, @LABELVALUE2, @DEACTIVLABEL2, @DEACTIVPOS2, @DEACTIVLABELVALUE2, @REVERSALLABEL2, @REVERSALPOS2,  
@JOURNALCODE, @JOURNALCODEVALUE, @CONSOCODE, @CONSOCODEVALUE,  
@ACCEXTLLINK, @ACCEXTLLINKVALUE, getutcdate(), @IDAINS)";

        // RD 20140916 [20328] Script to feed ACCDAYBOOK_EVENT
        // FI 20200820 [XXXXXX] Date systeme en UTC (use of getutcdate())
        public string SQL_Insert_AccDaybookEvent = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ACCDAYBOOK_EVENT.ToString() + @"
(IDACCDAYBOOK, IDE, DTINS, IDAINS) 
values 
(@IDACCDAYBOOK, @IDE, getutcdate(), @IDAINS)";

    }
    #endregion AccQuery
    #region public class AccTradeInfo
    public class AccTradeInfo
    {
        readonly string m_Cs;
        readonly DataSet m_DsTrade;
        readonly ExchangeTradedDerivative m_UnknownProduct;

        public ExchangeTradedDerivative UnknownProduct
        {
            get { return m_UnknownProduct; }
        }

        public DataTable DtTrade
        {
            get { return m_DsTrade.Tables["Trade"]; }
        }
        // EG 20141113 Gestion GPRODUCT
        public string GProduct
        {
            get { return DtTrade.Rows[0]["GPRODUCT"].ToString(); }
        }

        public bool IsDeactiv
        {
            get { return (DtTrade.Rows[0]["IDSTACTIVATION"].ToString() == Cst.StatusActivation.DEACTIV.ToString()); }
        }

        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public AccTradeInfo(string pCs, int pIdT)
        {
            m_Cs = pCs;

            // Load trade infos
            string sqlSelect = GetSelectTradeColumn() + Cst.CrLf + "where (t.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_Cs, "IDT", DbType.Int32), pIdT);
            QueryParameters qryParam = new QueryParameters(m_Cs, sqlSelect, parameters);
            m_DsTrade = DataHelper.ExecuteDataset(m_Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
            m_DsTrade.DataSetName = "TradeInfo";
            m_DsTrade.Tables[0].TableName = "Trade";

            // Unknown product
            m_UnknownProduct = new ExchangeTradedDerivative();
        }
        // EG 20141113 Gestion GPRODUCT
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetSelectTradeColumn()
        {
            return @"select t.IDSTACTIVATION, ns.GPRODUCT
            from dbo.TRADE t 
            inner join dbo.VW_INSTR_PRODUCT ns on (ns.IDI = t.IDI)" + Cst.CrLf;
        }
    }
    #endregion AccTradeInfo
    #region public class AccEventInfo
    // EG 20141113 Gestion GPRODUCT
    public class AccEventInfo : EfsML.Business.DataSetEventTrade
    {
        readonly ProcessTuningOutput m_Tuning;
        // EG 20141113 Gestion GPRODUCT
        readonly string m_GProduct;

        public ProcessTuningOutput Tuning
        {
            get { return m_Tuning; }
        }
        // EG 20141113 Gestion GPRODUCT
        public AccEventInfo(string pConnectionString, int pIdT, string pGProduct, ProcessTuningOutput pTuning) : base()
        {
            m_cs = pConnectionString;
            m_DbTransaction = null;
            m_Tuning = pTuning;
            m_GProduct = pGProduct;
            m_IdT = pIdT;
            Load();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170609 [22189] Modify
        /// EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public override void Load()
        {
            DataParameters parameters = new DataParameters();
            SQLWhere sqlWhere = new SQLWhere();

            bool isWithStCheck = ((m_Tuning != null) && m_Tuning.IdStSpecified(StatusEnum.StatusCheck));
            bool isWithStMatch = ((m_Tuning != null) && m_Tuning.IdStSpecified(StatusEnum.StatusMatch));

            if (0 < m_IdT)
            {
                parameters.Add(new DataParameter(m_cs, "IDT", DbType.Int32), m_IdT);
                sqlWhere.Append(@"(ev.IDT = @IDT)");
            }

            // EG 20141113 Gestion GPRODUCT
            parameters.Add(new DataParameter(m_cs, "GPRODUCT", DbType.String), m_GProduct);
            if (DataHelper.IsDbOracle(m_cs))
                sqlWhere.Append(@"(@GPRODUCT = @GPRODUCT)");

            StrBuilder SQLSelect = new StrBuilder();
            SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENT, Cst.ProcessTypeEnum.ACCOUNTGEN) + sqlWhere.ToString() + Cst.CrLf + SQLCst.ORDERBY + "ev.IDE";
            SQLSelect += SQLCst.SEPARATOR_MULTISELECT;
            // FI 20170609 [22189] Utilisation de ACCOUNTGEN
            //SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTCLASS, Cst.ProcessTypeEnum.EARGEN) + sqlWhere.ToString();
            SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTCLASS, Cst.ProcessTypeEnum.ACCOUNTGEN) + sqlWhere.ToString();
            SQLSelect += SQLCst.SEPARATOR_MULTISELECT;

            if (isWithStCheck)
                SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSTCHECK) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;
            if (isWithStMatch)
                SQLSelect += QueryLibraryTools.GetQuerySelect(CS, Cst.OTCml_TBL.EVENTSTMATCH) + sqlWhere.ToString() + SQLCst.SEPARATOR_MULTISELECT;

            QueryParameters qryParam = new QueryParameters(m_cs, SQLSelect.ToString(), parameters);
            m_dsEvents = DataHelper.ExecuteDataset(m_cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

            m_dsEvents.DataSetName = "Events";
            m_dsEvents.Tables[0].TableName = "Event";
            m_dsEvents.Tables[1].TableName = "EventClass";
            if (isWithStCheck)
                m_dsEvents.Tables[2].TableName = "EventStCheck";
            if (isWithStMatch)
                m_dsEvents.Tables[3].TableName = "EventStMatch";

            if ((null != DtEvent) && (null != DtEventClass))
            {
                DataRelation relEventClass = new DataRelation("Event_EventClass", DtEvent.Columns["IDE"], DtEventClass.Columns["IDE"], false);
                m_dsEvents.Relations.Add(relEventClass);
            }
            if ((null != DtEvent) && (null != DtEventStCheck))
            {
                DataRelation relEventStCheck = new DataRelation("Event_EventStCheck", DtEvent.Columns["IDE"], DtEventStCheck.Columns["IDE"], false);
                m_dsEvents.Relations.Add(relEventStCheck);
            }
            if ((null != DtEvent) && (null != DtEventStMatch))
            {
                DataRelation relEventStMatch = new DataRelation("Event_EventStMatch", DtEvent.Columns["IDE"], DtEventStMatch.Columns["IDE"], false);
                m_dsEvents.Relations.Add(relEventStMatch);
            }
        }
    }
    #endregion AccEventInfo

    #region public class AccBook
    public class AccBook
    {
        public int IDB;
        public List<int> AccModels;

        public AccBook(int pIdB)
        {
            IDB = pIdB;
            AccModels = new List<int>();
        }
    }
    #endregion AccBook
    #region public class AccModel
    /// <summary>
    /// 
    /// </summary>
    public class AccModel
    {
        public List<AccModelInstrument> AccInstruments;

        public int IDAccModel;
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();
        public bool IsEarIgnoredByModel;

        public AccModel(int pIDACCModel, string pAccModelIdentifier)
        {
            IDAccModel = pIDACCModel;
            lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("ACCMODEL", LogTools.IdentifierAndId(pAccModelIdentifier, IDAccModel))
            };
            AccInstruments = new List<AccModelInstrument>();
        }
    }
    #endregion AccModel
    #region public class AccModelInstrument
    public class AccModelInstrument
    {
        public int IDI;
        public List<AccInstrEnv> InstrEnvs;

        public AccModelInstrument(int pIDI)
        {
            IDI = pIDI;
            InstrEnvs = new List<AccInstrEnv>();
        }
    }
    #endregion AccModelInstrument

    #region public class AccInstrEnv
    public class AccInstrEnv
    {
        public int IDAccInstrEnv;
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();
        public List<AccInstrEnvDet> InstrEnvDets;

        public AccInstrEnv(int pInstrEnvID, string pInstrEnvIdentifier)
        {
            IDAccInstrEnv = pInstrEnvID;
            lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("ACCINSTRENV", LogTools.IdentifierAndId(pInstrEnvIdentifier, pInstrEnvID))
            };
            InstrEnvDets = new List<AccInstrEnvDet>();
        }
    }
    #endregion AccInstrEnv
    #region public class AccInstrEnvDet
    public class AccInstrEnvDet
    {
        public List<AccDefinedScheme> Schemes;
        public List<AccKey> Keys;
        public int IDInstrEnvDet;
        public bool IsToIgnore;
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();

        public AccInstrEnvDet(int pInstrEnvDetID, string pInstrEnvDetIdentifier, bool pIsToIgnore)
        {
            IDInstrEnvDet = pInstrEnvDetID;
            IsToIgnore = pIsToIgnore;
            lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("ACCINSTRENVDET", LogTools.IdentifierAndId(pInstrEnvDetIdentifier, IDInstrEnvDet))
            };
            Schemes = new List<AccDefinedScheme>();
            Keys = new List<AccKey>();
        }
    }
    #endregion AccInstrEnvDet

    #region public class AccKey
    public class AccKey
    {
        #region Members
        public string Table;
        public string Column;
        public string DataType;
        public string DataValue;
        #endregion Members
        #region Constructors
        public AccKey(string pKeyTable, string pKeyColumn, string pKeyDataType, string pKeyDataValue)
        {
            Table = pKeyTable;
            Column = pKeyColumn;
            DataType = pKeyDataType;
            DataValue = pKeyDataValue;
        }
        #endregion Constructors
        #region Methods
        #region CompareTo
        public int CompareTo(object obj)
        {
            if (obj is AccKey key)
            {
                if ((Table == key.Table) && (Column == key.Column) &&
                    (DataType == key.DataType) && (DataValue == key.DataValue))
                    return 0;
                else
                    return 1;
            }

            throw new ArgumentException("object is not a AccKey");
        }
        #endregion CompareTo
        #endregion Methods
    }
    #endregion AccKey

    #region public class AccDefinedScheme
    /// <summary>
    /// Stock les schémas parametrés
    /// </summary>
    public class AccDefinedScheme
    {
        private decimal m_EntriesAmountNet;
        private decimal m_ReversalEntriesAmountNet;
        private int m_IDAccScheme;
        //
        public string CashSecurities;
        public string EarCode;
        public string EventClass;
        public string VRUnbalanced;
        //
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();
        public List<AccDefinedEntry> DefinedEntries;

        public int IDAccScheme
        {
            get { return m_IDAccScheme; }
            set { m_IDAccScheme = value; }
        }
        public decimal EntriesAmountNet
        {
            get { return m_EntriesAmountNet; }
            set { m_EntriesAmountNet = value; }
        }
        public decimal ReversalEntriesAmountNet
        {
            get { return m_ReversalEntriesAmountNet; }
            set { m_ReversalEntriesAmountNet = value; }
        }
        public AccDefinedScheme(string pEarCode, string pEventClass)
        {
            EarCode = pEarCode;
            EventClass = pEventClass;

            DefinedEntries = new List<AccDefinedEntry>();
        }
        public AccDefinedScheme(int pIDAccScheme, string pAccSchemeIdentifier, string pCashSecurities,
            string pEarCode, string pEventClass, string pVRUnbalanced)
            : this(pEarCode, pEventClass)
        {
            m_IDAccScheme = pIDAccScheme;
            CashSecurities = pCashSecurities;
            VRUnbalanced = pVRUnbalanced;
            m_EntriesAmountNet = 0;
            m_ReversalEntriesAmountNet = 0;

            lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("ACCDEFSCHEME", LogTools.IdentifierAndId(pAccSchemeIdentifier, m_IDAccScheme))
            };
        }
    }
    #endregion AccDefinedScheme
    #region public class AccDefinedEntry
    /// <summary>
    /// Stock les écritures parametrées
    /// </summary>
    public class AccDefinedEntry
    {
        #region Members
        private readonly bool m_IsReversal;
        private readonly string m_DayTypeReversal;
        private int m_IDAccEntry;
        private List<string> m_Variables;
        //
        public int IDAccCondition;
        //
        public string EarCode;
        public string EventClass;
        public string ExchangeType;
        public string QuoteExchangeType;
        public string AmountType;
        //
        public string DebitAccountPay;
        public string DebitAccountPayValue;
        public string DebitAccountPayByEvent;
        public string CreditAccountPay;
        public string CreditAccountPayValue;
        public string CreditAccountPayByEvent;
        public string DebitAccountRec;
        public string DebitAccountRecValue;
        public string DebitAccountRecByEvent;
        public string CreditAccountRec;
        public string CreditAccountRecValue;
        public string CreditAccountRecByEvent;
        //
        public int PeriodMultipOffset;
        public string PeriodOffset;
        public string DayTypeOffset;
        //
        public AccLabel LabelMain;
        public AccLabel LabelSecondary;
        //
        public string JournalCode;
        public string JournalCodeValue;
        public string JournalCodeByEvent;
        public string ConsoCode;
        public string ConsoCodeValue;
        public string ConsoCodeByEvent;
        //
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();
        #endregion Members
        #region Accessors
        public int IDAccEntry
        {
            get { return m_IDAccEntry; }
            set { m_IDAccEntry = value; }
        }
        //	
        public bool IsReversal
        {
            get { return m_IsReversal; }
        }
        public string DayTypeReversal
        {
            get { return m_DayTypeReversal; }
        }
        /// <summary>
        /// La liste des variables comptables contenues dans les différents membres de la classe.
        /// </summary>
        public List<string> Variables
        {
            get { return m_Variables; }
        }
        #endregion
        #region Constructor
        public AccDefinedEntry(int pAccEntryID, int pAccConditionID, string pEarCode, string pEventClass, string pExchangeType,
            string pQuoteExchangeType, string pAmountType, string pDebitAccountPay, string pCreditAccountPay, string pDebitAccountRec,
            string pCreditAccountRec, int pPeriodMultipOffset, string pPeriodOffset, string pDayTypeOffset, bool pIsReversal,
            string pDayTypeReversal, string pJournalCode, string pConsoCode)
        {
            m_IDAccEntry = pAccEntryID;
            IDAccCondition = pAccConditionID;
            EarCode = pEarCode;
            EventClass = pEventClass;
            ExchangeType = pExchangeType;
            QuoteExchangeType = pQuoteExchangeType;
            AmountType = pAmountType;
            //
            DebitAccountPay = pDebitAccountPay;
            CreditAccountPay = pCreditAccountPay;
            DebitAccountRec = pDebitAccountRec;
            CreditAccountRec = pCreditAccountRec;
            //
            PeriodMultipOffset = pPeriodMultipOffset;
            PeriodOffset = pPeriodOffset;
            DayTypeOffset = pDayTypeOffset;
            m_IsReversal = pIsReversal;
            m_DayTypeReversal = pDayTypeReversal;
            //
            JournalCode = pJournalCode;
            ConsoCode = pConsoCode;

            lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("ACCDEFENTRY",
                LogTools.IdentifierAndId(EarCode + "/" + AmountType + "/" + EventClass + "[" + ExchangeType + "]", IDAccEntry))
            };
        }
        #endregion Constructor
        #region Methods

        /// <summary>
        /// Extrait toutes les Variables comptables contenues dans les membres à valoriser
        /// <para>Les membres à valoriser sont ceux qui permettent la saisie des variables comptables:</para>
        /// <para>- LabelMain.LabelValue</para>
        /// <para>- LabelMain.ReversalLabelValue</para>
        /// <para>- LabelMain.DeactivLabelValue</para>
        /// <para>- LabelSecondary.LabelValue</para>
        /// <para>- LabelSecondary.ReversalLabelValue</para>
        /// <para>- LabelSecondary.DeactivLabelValue</para>
        /// <para>- CreditAccountPayValue</para>
        /// <para>- CreditAccountRecValue</para>
        /// <para>- DebitAccountPayValue</para>
        /// <para>- DebitAccountRecValue</para>
        /// <para>- JournalCodeValue</para>
        /// <para>- ConsoCodeValue</para>
        /// </summary>
        public void InitVariables()
        {
            // Faire l'initialisation une seule fois par écriture comptable parametrée
            if (m_Variables == null)
            {
                ArrayList stringsWithVariable = new ArrayList();

                LabelMain.GetStringWithVariable(stringsWithVariable);
                LabelSecondary.GetStringWithVariable(stringsWithVariable);

                stringsWithVariable.Add(DebitAccountPay);
                stringsWithVariable.Add(CreditAccountPay);
                stringsWithVariable.Add(DebitAccountRec);
                stringsWithVariable.Add(CreditAccountRec);
                stringsWithVariable.Add(ConsoCode);
                stringsWithVariable.Add(JournalCode);

                m_Variables = AccVariableTools.InitVariables(stringsWithVariable);
            }
        }

        /// <summary>
        /// Initialise les membres avec le suffix 'Value' à partir des membres correspondants susceptibles de contenir des variables 
        /// </summary>
        public void InitStringValue()
        {
            LabelMain.InitStringValue();
            LabelSecondary.InitStringValue();

            DebitAccountPayValue = DebitAccountPay;
            CreditAccountPayValue = CreditAccountPay;
            DebitAccountRecValue = DebitAccountRec;
            CreditAccountRecValue = CreditAccountRec;

            JournalCodeValue = JournalCode;
            ConsoCodeValue = ConsoCode;
        }

        /// <summary>
        /// Initialise les membres avec le suffix 'ByEvent' à partir des membres avec le suffix 'Value'
        /// </summary>
        public void InitStringByEvent()
        {
            LabelMain.InitStringByEvent();
            LabelSecondary.InitStringByEvent();

            DebitAccountPayByEvent = DebitAccountPayValue;
            CreditAccountPayByEvent = CreditAccountPayValue;
            DebitAccountRecByEvent = DebitAccountRecValue;
            CreditAccountRecByEvent = CreditAccountRecValue;

            JournalCodeByEvent = JournalCodeValue;
            ConsoCodeByEvent = ConsoCodeValue;
        }

        /// <summary>
        /// Initialise les membres avec le suffix 'Value' à partir des membres avec le suffix 'ByEvent'
        /// </summary>
        public void InitStringValueForEvent()
        {
            LabelMain.InitStringValueForEvent();
            LabelSecondary.InitStringValueForEvent();

            DebitAccountPayValue = DebitAccountPayByEvent;
            CreditAccountPayValue = CreditAccountPayByEvent;
            DebitAccountRecValue = DebitAccountRecByEvent;
            CreditAccountRecValue = CreditAccountRecByEvent;

            JournalCodeValue = JournalCodeByEvent;
            ConsoCodeValue = ConsoCodeByEvent;
        }

        /// <summary>
        /// Remplacer la variable {pVariable} par sa valaur {pVariableValue} dans tous les membres à valoriser avec le suffix "Value"
        /// </summary>
        /// <param name="pVariableRegex"></param>
        /// <param name="pVariableValue"></param>
        public void ValuateStringValue(Regex pVariableRegex, string pVariableValue)
        {
            LabelMain.ValuateStringValue(pVariableRegex, pVariableValue);
            LabelSecondary.ValuateStringValue(pVariableRegex, pVariableValue);

            if (StrFunc.IsFilled(CreditAccountPayValue))
                CreditAccountPayValue = pVariableRegex.Replace(CreditAccountPayValue, pVariableValue);
            if (StrFunc.IsFilled(CreditAccountRecValue))
                CreditAccountRecValue = pVariableRegex.Replace(CreditAccountRecValue, pVariableValue);
            if (StrFunc.IsFilled(DebitAccountPayValue))
                DebitAccountPayValue = pVariableRegex.Replace(DebitAccountPayValue, pVariableValue);
            if (StrFunc.IsFilled(DebitAccountRecValue))
                DebitAccountRecValue = pVariableRegex.Replace(DebitAccountRecValue, pVariableValue);

            if (StrFunc.IsFilled(JournalCodeValue))
                JournalCodeValue = pVariableRegex.Replace(JournalCodeValue, pVariableValue);
            if (StrFunc.IsFilled(ConsoCodeValue))
                ConsoCodeValue = pVariableRegex.Replace(ConsoCodeValue, pVariableValue);
        }
        #endregion Methods
    }
    #endregion AccDefinedEntry

    #region public class AccEarDetFlows
    /// <summary>
    /// Stock tous les flux (EarDay, EarCommon, ...) dépondants d'un même EarDet (InstrumentNO, StreamNO) 
    /// </summary>
    public class AccEarDetFlows
    {
        public int InstrumentNO;
        public int StreamNO;
        public List<AccEarFlow> Flows;

        public AccEarDetFlows(int pInstrumentNO, int pStreamNO)
        {
            InstrumentNO = pInstrumentNO;
            StreamNO = pStreamNO;

            Flows = new List<AccEarFlow>();
        }

        public void SetEarSchemeProcessed(string pEarCode, string pEventClass)
        {
            foreach (AccEarFlow flow in Flows.FindAll(flow =>
                flow.IsEarDay && flow.EarCode == pEarCode && flow.EventClass == pEventClass))
            {
                flow.IsSchemeProcessed = true;
            }
        }

        public bool IsEarSchemeProcessed(string pEarCode, string pEventClass)
        {
            return (null != Flows.Find(flow =>
                (flow.IsEarDay && flow.EarCode == pEarCode && flow.EventClass == pEventClass && flow.IsSchemeProcessed)));
        }
    }
    #endregion AccEarDetFlows
    #region public class AccEarFlow
    /// <summary>
    /// Stock un flux issue des Ear (EarDay, EarCommon, ...):
    /// <para>Type de flux: EarCode, AmountType, EventClass</para>
    /// <para>Liste des montants de toutes les contrevaleurs du flux</para>
    /// </summary>
    public class AccEarFlow
    {
        public List<int> AccountedEntries;
        public List<AccAmount> Amounts;

        public string EarType;
        public DateTime DTAccount;
        public string EarCode;
        public string AmountType;
        public string EventClass;
        //
        public bool IsEarDay { get { return (EarType == "EARDAY"); } }
        public bool IsSchemeProcessed;

        public AccEarFlow(string pEarType, DateTime pDTAccount, string pEarCode, string pAmountType, string pEventClass)
        {
            EarType = pEarType;
            DTAccount = pDTAccount;
            EarCode = pEarCode;
            AmountType = pAmountType;
            EventClass = pEventClass;

            Amounts = new List<AccAmount>();
            AccountedEntries = new List<int>();
        }
    }
    #endregion AccEarFlow
    #region public class AccAmount
    /// <summary>
    /// Stock le montant avec:
    /// <para>Type de contrevleur (ExchangeType)</para>
    /// <para>Montants payé et/ou reçu</para>
    /// <para>Devise</para>
    /// </summary>
    public class AccAmount
    {
        public int IdEarType;
        public string ExchangeType;
        public string IDC;
        public decimal Received;
        public decimal Paid;
        public string IDStProcess;
        public List<int> Events;

        public AccAmount(int pIdEarType, string pExchangeType) :
            this(pIdEarType, pExchangeType, string.Empty, 0, 0, ProcessStateTools.StatusError) { }
        public AccAmount(int pIdEarType, string pExchangeType, string pIDC, decimal pReceived, decimal pPaid, string pIDStProcess)
        {
            IdEarType = pIdEarType;
            ExchangeType = pExchangeType;
            IDC = pIDC;
            Received = pReceived;
            Paid = pPaid;
            IDStProcess = pIDStProcess;

            Events = new List<int>();
        }
    }
    #endregion AccAmount

    #region public class AccEntryFlow
    /// <summary>
    /// Stock un flux Comptable (EarDay, EarCommon, ...):
    /// <para>Type de flux: EarCode, AmountType, EventClass</para>
    /// <para>Liste des montants, avec leur contrevaleur</para>
    /// </summary>
    // RD 20140916 [20328] Add member EARTYPE
    public class AccEntryFlow
    {
        public DateTime DTAccount;
        public string EarType;
        public string EarCode;
        public string AmountType;
        public string EventClass;

        public List<AccAmount> Amounts;

        public AccEntryFlow(AccEarFlow pAccEarFlow)
        {
            DTAccount = pAccEarFlow.DTAccount;
            EarType = pAccEarFlow.EarType;
            EarCode = pAccEarFlow.EarCode;
            AmountType = pAccEarFlow.AmountType;
            EventClass = pAccEarFlow.EventClass;

            Amounts = new List<AccAmount>();
        }

        public void Add(List<AccAmount> pAmount)
        {
            Amounts.AddRange(pAmount);
        }
    }
    #endregion AccEntryFlow
    #region public class AccCondition
    public class AccCondition
    {
        #region Members
        public int IdAccCondition;
        public string SqlCondition;
        public string TableData;
        //
        public bool ByInstrumentNo;
        public bool ByStreamNo;
        public bool ByEarCode;
        public bool ByEventType;
        public bool ByEventClass;
        public bool ByEvent;
        //
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();
        #endregion Members
        #region Constructors
        public AccCondition(int pIdAccCondition, string pIdentifier, string pSqlCondition, string pTableData,
            bool pByInstrumentNo, bool pByStreamNo, bool pByEarCode, bool pByEventType, bool pByEventClass, bool pByEvent)
        {
            IdAccCondition = pIdAccCondition;
            SqlCondition = pSqlCondition;
            TableData = pTableData;
            //
            ByInstrumentNo = pByInstrumentNo;
            ByStreamNo = pByStreamNo;
            ByEarCode = pByEarCode;
            ByEventType = pByEventType;
            ByEventClass = pByEventClass;
            ByEvent = pByEvent;

            lstData = new List<Pair<string, string>>
            {
                new Pair<string, string>("ACCCOND", LogTools.IdentifierAndId(pIdentifier, IdAccCondition)),
                new Pair<string, string>("SOURCE", TableData),
                new Pair<string, string>("SQLCOND", SqlCondition)
            };
        }
        #endregion Constructors
    }
    #endregion AccCondition

    #region public class AccLabel
    public class AccLabel
    {
        #region Members
        public string Label;
        public string LabelValue;
        public string LabelByEvent;
        public string ReversalLabel;
        public string ReversalLabelValue;
        public string ReversalLabelByEvent;
        public string ReversalPos;
        public string DeactivLabel;
        public string DeactivLabelValue;
        public string DeactivLabelByEvent;
        public string DeactivPos;
        #endregion Members
        #region Constructors
        public AccLabel(object pLabel, object pReversalLabel, object pReversalPos, object pDeactivLabel, object pDeactivPos)
        {
            Label = (Convert.IsDBNull(pLabel) ? string.Empty : pLabel.ToString());
            ReversalLabel = (Convert.IsDBNull(pReversalLabel) ? string.Empty : pReversalLabel.ToString());
            ReversalPos = (Convert.IsDBNull(pReversalPos) ? string.Empty : pReversalPos.ToString());
            DeactivLabel = (Convert.IsDBNull(pDeactivLabel) ? string.Empty : pDeactivLabel.ToString());
            DeactivPos = (Convert.IsDBNull(pDeactivPos) ? string.Empty : pDeactivPos.ToString());
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Charger dans la liste {pStringsWithVariable} la liste des membres susceptibles de contenir des variables
        /// </summary>
        /// <param name="pStringsWithVariable"></param>
        public void GetStringWithVariable(ArrayList pStringsWithVariable)
        {
            pStringsWithVariable.Add(Label);
            pStringsWithVariable.Add(ReversalLabel);
            pStringsWithVariable.Add(DeactivLabel);
        }

        /// <summary>
        /// Initialise les membres avec le suffix 'Value' à partir des membres correspondants susceptibles de contenir des variables 
        /// </summary>
        public void InitStringValue()
        {
            LabelValue = Label;
            ReversalLabelValue = ReversalLabel;
            DeactivLabelValue = DeactivLabel;
        }

        /// <summary>
        /// Initialise les membres avec le suffix 'ByEvent' à partir des membres avec le suffix 'Value'
        /// </summary>
        public void InitStringByEvent()
        {
            LabelByEvent = LabelValue;
            ReversalLabelByEvent = ReversalLabelValue;
            DeactivLabelByEvent = DeactivLabelValue;
        }

        /// <summary>
        /// Initialise les membres avec le suffix 'Value' à partir des membres avec le suffix 'ByEvent'
        /// </summary>
        public void InitStringValueForEvent()
        {
            LabelValue = LabelByEvent;
            ReversalLabelValue = ReversalLabelByEvent;
            DeactivLabelValue = DeactivLabelByEvent;
        }

        /// <summary>
        /// Remplacer la variable {pVariable} par sa valaur {pVariableValue} dans tous les membres à valoriser avec le suffix "Value"
        /// </summary>
        /// <param name="pVariableRegex"></param>
        /// <param name="pVariableValue"></param>
        public void ValuateStringValue(Regex pVariableRegex, string pVariableValue)
        {
            if (StrFunc.IsFilled(LabelValue))
                LabelValue = pVariableRegex.Replace(LabelValue, pVariableValue);
            if (StrFunc.IsFilled(ReversalLabelValue))
                ReversalLabelValue = pVariableRegex.Replace(ReversalLabelValue, pVariableValue);
            if (StrFunc.IsFilled(DeactivLabelValue))
                DeactivLabelValue = pVariableRegex.Replace(DeactivLabelValue, pVariableValue);
        }
        #endregion
    }
    #endregion AccLabel

    #region  public class AccEar
    /// <summary>
    /// Stock un EAR avec tous ses flux
    /// </summary>
    // EG 20190114 Add detail to ProcessLog Refactoring
    public class AccEar : AccLog
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        private readonly SetErrorWarning m_SetErrorWarning = null;

        private readonly string m_Cs;
        private readonly string m_Source;
        public IDbTransaction DBTransaction;
        public DateTime DateAccounting;
        public DateTime DateEar;
        public DateTime DateEvent;
        public DateTime DateEarRemove;

        public int IDT;
        public int IDEAR;
        public int IDB;
        public string Identifier_Book;
        public int IdA_Entity;
        public string Identifier_Entity;
        public string IdBCAccount_Entity;
        public string IDStActivation;
        public DateTime DateEarCancel;
        public DateTime DateEventCancel;

        public List<AccEarDetFlows> EarDetFlows;

        public AccEarDet CurrentEarDet;
        public List<AccEntryFlow> CurrentEntryFlows;
        public AccDefinedScheme CurrentDefinedScheme;
        public AccDefinedEntry CurrentDefinedEntry;

        public List<Pre_DayBook> AccDayBooks;

        public List<int> ModelsAlreadyIgnore;
        public List<int> ModelsNewlyIgnore;

        /// <summary>
        /// 0: Trade identifier
        /// <para>1: IDT</para>
        /// <para>2: Process date</para>
        /// <para>3: Book</para>
        /// <para>4: Entity</para>
        /// <para>5: EAR</para>
        /// </summary>
        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();

        /// <summary>
        /// retourne true s'il s'agit d'un EAR jour (DateEvent de l'EAR = date de traitement)
        /// </summary>
        public bool IsToDayEAR
        {
            get
            {
                if (DateTime.Compare(DateEvent, DateAccounting) == 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// retourne true s'il existe un stream avec StreamNO=0 sur l'EAR
        /// </summary>
        public bool IsSream0Exist
        {
            get { return (null != EarDetFlows.Find(det => det.StreamNO == 0)); }
        }

        /// <summary>
        /// retourne true s'il existe un stream avec StreamNO!=0 sur l'EAR
        /// </summary>
        public bool IsOtherSreamExist
        {
            get { return (null != EarDetFlows.Find(det => det.StreamNO != 0)); }
        }

        /// <summary>
        /// retourne true s'il s'agit d'un EAR annulé suite à l'annulation d'un événement
        /// <para>Attention: à ne pas confondre avec un EAR annulé suite à l'annulation du Trade entier</para>
        /// </summary>
        public bool IsEARRemoved
        {
            get
            {
                return (IDStActivation == Cst.StatusActivation.REMOVED.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        /// <param name="pAccQuery"></param>
        /// <param name="pDrEar"></param>
        /// <param name="pIsTradeRemoved"></param>
        /// <param name="pLstData"></param>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20200623 [XXXXX] Add SetErrorWarning
        public AccEar(SetErrorWarning pSetErrorWarning,
            AccQuery pAccQuery, DataRow pDrEar, bool pIsTradeRemoved, List<Pair<string, string>> pLstData)
            : base()
        {
            
            m_SetErrorWarning = pSetErrorWarning;
            m_Cs = pAccQuery.CS;

            DateAccounting = (DateTime)pAccQuery.paramDtProcess.Value;
            IDT = (int)pAccQuery.paramIdT.Value;

            IDB = Convert.ToInt32(pDrEar["IDB"]);
            IDEAR = Convert.ToInt32(pDrEar["IDEAR"]);
            DateEar = Convert.ToDateTime(pDrEar["DTEAR"]);
            DateEvent = Convert.ToDateTime(pDrEar["DTEVENT"]);
            IDStActivation = pDrEar["IDSTACTIVATION"].ToString();
            DateEarRemove = (Convert.IsDBNull(pDrEar["DTREMOVED"]) ? DateTime.MinValue : Convert.ToDateTime(pDrEar["DTREMOVED"]));
            m_Source = pDrEar["SOURCE"].ToString();

            if (pIsTradeRemoved)
            {
                DateEarCancel = Convert.ToDateTime(pDrEar["DTEARCANCEL"]);
                DateEventCancel = Convert.ToDateTime(pDrEar["DTEVENTCANCEL"]);
            }

            SQL_Book book = new SQL_Book(CSTools.SetCacheOn(m_Cs), IDB);
            book.LoadTable(new string[] { "IDENTIFIER", "IDA_ENTITY" });

            Identifier_Book = book.Identifier;
            IdA_Entity = book.IdA_Entity;

            SQL_Actor sql_ActorEntity = new SQL_Actor(CSTools.SetCacheOn(m_Cs), IdA_Entity)
            {
                WithInfoEntity = true
            };
            sql_ActorEntity.LoadTable(new string[] { "ACTOR.IDENTIFIER, ent.IDBCACCOUNT" });

            Identifier_Entity = sql_ActorEntity.Identifier;
            IdBCAccount_Entity = sql_ActorEntity.IdBCAccount;

            lstData = new List<Pair<string, string>>();
            lstData.AddRange(pLstData);
            lstData.Add(new Pair<string, string>("BOOK", LogTools.IdentifierAndId(Identifier_Book, IDB)));
            lstData.Add(new Pair<string, string>("ENTITY", LogTools.IdentifierAndId(Identifier_Entity, IdA_Entity)));
            lstData.Add(new Pair<string, string>("EAR", LogTools.IdentifierAndId(DtFunc.DateTimeToStringDateISO(DateEar), IDEAR)));
            lstData.Add(new Pair<string, string>("DTEVENT", DtFunc.DateTimeToStringDateISO(DateEvent)));
            lstData.Add(new Pair<string, string>("IDSTACTIVATION", IDStActivation));
            lstData.Add(new Pair<string, string>("SOURCE", m_Source));

            EarDetFlows = new List<AccEarDetFlows>();
            ModelsAlreadyIgnore = new List<int>();
            ModelsNewlyIgnore = new List<int>();
        }

        /// <summary>
        /// Charge tous les détails de l'EAR
        /// <para>- InstrumentNO, StreamNO et IDInstrument</para>
        /// <para>- Tous les flux</para>
        /// </summary>
        // RD 20161118 POC RATP
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public Cst.ErrLevel LoadEARDetails(DataParameter pParamIDEAR, ref DataTable pDtEarDet)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            try
            {
                //PL 20111103 WARNING: Refactoring pour ne plus utiliser VW_ACCKEYDATA (TRIM 17409,17617)
                string sqlQueryEarDet = SQLCst.SQL_ANSI + Cst.CrLf;
                sqlQueryEarDet += "select eardet.IDEAR, eardet.IDI, eardet.INSTRUMENTNO, eardet.STREAMNO, eardet.INSTR_IDENTIFIER, " + Cst.CrLf;
                sqlQueryEarDet += "b.IDA_ENTITY as IDA_ENTITYPARTY, taEar.IDA as IDA_PARTY, b.IDB as IDB_PARTY," + Cst.CrLf;

                // RD 20120307 / Gérer le cas du Book d'un Broker
                // 1/ Dans le cas ou le Broker n’est rattaché à aucune contrepartie , alors:
                // - considérer le Broker lui même comme étant la contrepartie en face 
                // 2/ Dans le cas ou le même Broker existe sur les deux contreparties, alors:
                // - pour les ETD: considérer celui rattaché au Dealer (FIXPARTYROLE=27)
                // - pour les OTC: considérer les deux Broker, avec comme conséquence:
                //  * Duplication des lignes de log pour l’EARDET concerné, car l’EARDET est traité deux fois
                //  * Pas de doublons au niveau des écritures comptables
                // NB: IDA_COUNTERPARTY et IDB_COUNTERPARTY sont restituées dans le journal comptable (ACCDAYBOOK)
                sqlQueryEarDet += "case when ta_EarCtrPty.IDA is not null then ta_EarCtrPty.IDA else taEar.IDA end as IDA_COUNTERPARTY," + Cst.CrLf;
                sqlQueryEarDet += "case when ta_EarCtrPty.IDA is not null then b_EarCtrPty.IDB else b.IDB end as IDB_COUNTERPARTY" + Cst.CrLf;
                //
                //
                // RD 20120307 / Gérer le cas du Book d'un Broker
                // Dans le cas ou le même Broker existe sur les deux contreparties, alors:
                // pour les ETD: considérer celui rattaché au Dealer (FIXPARTYROLE=27)
                // pour les OTC: considérer les deux 

                // RD 20161124 [22621] Modifications pour gérer le cas où acteur Entity est en même temps Partie et Broker
                // RD 20180226 [23797] Utiliser le role 27 uniquement pour les ETD
                // RD 20180612 [23797] Ajouter la jointure sur PRODUCT
                sqlQueryEarDet += @"from dbo.VW_EARDET eardet
inner join dbo.EAR ear on (ear.IDEAR=eardet.IDEAR)
-- Acteur DO pour les ETD
inner join dbo.INSTRUMENT i on (i.IDI=eardet.IDI)
inner join dbo.PRODUCT p on (p.IDP=i.IDP)
left outer join dbo.TRADEACTOR ta27 on (ta27.IDT=ear.IDT) and (ta27.FIXPARTYROLE=27) and (p.IDENTIFIER='exchangeTradedDerivative')
-- L'Ear concerne le Book sur le Broker coté DO pour les ETD
left outer join dbo.TRADEACTOR taEarBroker27 on (taEarBroker27.IDT=ear.IDT) and (taEarBroker27.IDB=ear.IDB) and (taEarBroker27.IDA_ACTOR=ta27.IDA)
-- L'Ear concerne le Book sur le Buyer ou bien le Seller
left outer join dbo.TRADEACTOR taEarBuySell on (taEarBuySell.IDT=ear.IDT) and (taEarBuySell.IDB=ear.IDB) and (taEarBuySell.BUYER_SELLER is not null)
-- La ligne de la table TRADEACTOR concernée par l'Ear:
-- 1/ Soit c'est la ligne du Buyer ou bien le Seller
-- 2/ Soit c'est la ligne du Broker coté DO pour les ETD
-- 3/ Soit c'est la ligne d'un Broker
inner join dbo.TRADEACTOR taEar on (taEar.IDT=ear.IDT) and (taEar.IDB=ear.IDB) 
	and 
	(
		((taEarBuySell.IDA is not null) and (taEar.BUYER_SELLER is not null))
		or ((taEarBuySell.IDA is null) and ((taEarBroker27.IDA is null) or (taEar.IDA_ACTOR=taEarBroker27.IDA_ACTOR)))
	)
-- La ligne de la table TRADEACTOR du Buyer ou bien le Seller concrnée par L'Ear:
-- 1/ Si l'Ear concerne la ligne du Buyer ou bien le Seller alors c'est la ligne de l'Ear elle même
-- 2/ Si l'Ear concerne la ligne du Broker alors c'est la ligne du Buyer ou bien le Seller parent du Broker
left outer join dbo.TRADEACTOR taDealer on (taDealer.IDT=ear.IDT) and (taDealer.BUYER_SELLER is not null)
	and 
	(
		((taEar.BUYER_SELLER is not null) and (taDealer.IDB=ear.IDB))
		or ((taEar.BUYER_SELLER is null) and (taDealer.IDA=taEar.IDA_ACTOR))
	)
--Le Book de l'EAR
inner join dbo.BOOK b on (b.IDB = ear.IDB)
-- La CounterParty en face
left outer join dbo.TRADEACTOR ta_EarCtrPty on (ta_EarCtrPty.IDT = ear.IDT)
    and (ta_EarCtrPty.BUYER_SELLER = case taDealer.BUYER_SELLER when 'Buyer' then 'Seller' when 'Seller' then 'Buyer' else '*' end)
-- Le Book de la CounterParty en face
left outer join dbo.BOOK b_EarCtrPty on (b_EarCtrPty.IDB = ta_EarCtrPty.IDB)
where(eardet.IDEAR = @IDEAR)
order by eardet.IDI, eardet.INSTRUMENTNO, eardet.STREAMNO" + Cst.CrLf;

                // RD 20150108 [20637]
                //string sqlQueryEarAmount = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                //sqlQueryEarAmount += "eaamount.INSTRUMENTNO, eaamount.STREAMNO, eaamount.DTACCOUNT," + Cst.CrLf;
                //sqlQueryEarAmount += "eaamount.EARCODE, eaamount.AMOUNTTYPE, eaamount.EVENTCLASS, eaamount.EXCHANGETYPE, eaamount.IDC," + Cst.CrLf;
                //sqlQueryEarAmount += "eaamount.RECEIVED, eaamount.PAID, eaamount.IDSTPROCESS, eaamount.IDEARTYPE, eaamount.EARTYPE, eevt.IDE" + Cst.CrLf;
                //sqlQueryEarAmount += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EARACCAMOUNT + " eaamount" + Cst.CrLf;
                //sqlQueryEarAmount += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.VW_EAREVENT + " eevt on (eevt.IDEARTYPE = eaamount.IDEARTYPE)" + Cst.CrLf;
                //sqlQueryEarAmount += " and (eevt.EARTYPE = eaamount.EARTYPE)" + Cst.CrLf;
                //sqlQueryEarAmount += SQLCst.WHERE + "(eaamount.IDEAR = @IDEAR)" + Cst.CrLf;
                string sqlQueryEarAmount = @"select amt.INSTRUMENTNO, amt.STREAMNO, amt.DTACCOUNT, 
amt.EARCODE, amt.AMOUNTTYPE, amt.EVENTCLASS, amt.EXCHANGETYPE, amt.IDC,
amt.RECEIVED, amt.PAID, amt.IDSTPROCESS, amt.IDEARTYPE, amt.EARTYPE, amt.IDE
from dbo.VW_EARACCAMOUNTEVT amt
where (amt.IDEAR = @IDEAR)
";

                DataParameters parameters = new DataParameters();
                parameters.Add(pParamIDEAR);
                QueryParameters qryParam = new QueryParameters(m_Cs, sqlQueryEarDet + SQLCst.SEPARATOR_MULTISELECT + sqlQueryEarAmount, parameters);
                DataSet dsEarDet = DataHelper.ExecuteDataset(m_Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

                #region EarDet
                pDtEarDet = dsEarDet.Tables[0];
                pDtEarDet.TableName = "EARDET";
                DataRow[] drEarDet = pDtEarDet.Select();
                //
                if (drEarDet.Length == 0)
                {
                    // No Ear details at this day
                    codeReturn = Cst.ErrLevel.DATANOTFOUND;
                    
                    // FI 20200623 [XXXXX] Add
                    m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.LOG, 5339), 0,
                        new LogParam(lstData.Find(match => match.First == "EAR").Second),
                        new LogParam(lstData.Find(match => match.First == "BOOK").Second)));

                    return codeReturn;
                }
                #endregion EarDet
                #region EarAmount
                DataTable dtEarAmount = dsEarDet.Tables[1];
                dtEarAmount.TableName = "EARAMOUNT";
                DataRow[] drEarAmount = dtEarAmount.Select();

                AccEarDetFlows accEarDet;
                int instrumentNO;
                int streamNO;

                int amountIdEarType;
                string amountIDC;
                decimal amountReceived;
                decimal amountPaid;
                string amountIdStProcess;
                string amountExchangeType;

                DateTime amountDTAccount;
                string amountEarType;
                string amountEarCode;
                string amountAmountType;
                string amountEventClass;

                int amountIDE;

                foreach (DataRow rowAmount in drEarAmount)
                {
                    instrumentNO = Convert.ToInt32(rowAmount["INSTRUMENTNO"]);
                    streamNO = Convert.ToInt32(rowAmount["STREAMNO"]);

                    accEarDet = EarDetFlows.Find(det => det.InstrumentNO == instrumentNO && det.StreamNO == streamNO);
                    if (null == accEarDet)
                    {
                        accEarDet = new AccEarDetFlows(instrumentNO, streamNO);
                        EarDetFlows.Add(accEarDet);
                    }

                    amountEarType = rowAmount["EARTYPE"].ToString();
                    amountDTAccount = Convert.ToDateTime(rowAmount["DTACCOUNT"]);
                    amountEarCode = rowAmount["EARCODE"].ToString();
                    amountAmountType = rowAmount["AMOUNTTYPE"].ToString();
                    amountEventClass = rowAmount["EVENTCLASS"].ToString();

                    AccEarFlow accEarFlow = accEarDet.Flows.Find(flow =>
                        (flow.EarType == amountEarType) && (flow.EarCode == amountEarCode) &&
                        (flow.AmountType == amountAmountType) && (flow.EventClass == amountEventClass) &&
                        (flow.DTAccount == amountDTAccount));

                    if (null == accEarFlow)
                    {
                        accEarFlow = new AccEarFlow(amountEarType, amountDTAccount, amountEarCode, amountAmountType, amountEventClass);
                        accEarDet.Flows.Add(accEarFlow);
                    }

                    amountIdEarType = Convert.ToInt32(rowAmount["IDEARTYPE"]);
                    amountExchangeType = (Convert.IsDBNull(rowAmount["EXCHANGETYPE"]) ? string.Empty : rowAmount["EXCHANGETYPE"].ToString());
                    amountIDC = rowAmount["IDC"].ToString();
                    amountReceived = (Convert.IsDBNull(rowAmount["RECEIVED"]) ? 0 : Convert.ToDecimal(rowAmount["RECEIVED"]));
                    amountPaid = (Convert.IsDBNull(rowAmount["PAID"]) ? 0 : Convert.ToDecimal(rowAmount["PAID"]));
                    amountIdStProcess = rowAmount["IDSTPROCESS"].ToString();

                    // RD 20120823 / Un montant est caractérisé par:
                    // - son ID du flux ( IDAEARDAY, IDEARCOMMON ou bien IDEARCALC) 
                    // - et la contrevaleur
                    // NB: les tables EARDAYAMOUNT, EARCOMMONAMOUNT et EARCALCAMOUNT contient des index uniques sur repectivement:
                    // (IDEARDAY, EXCHANGETYPE), (IDEARCOMMON, EXCHANGETYPE) et (IDEARCALC, EXCHANGETYPE),                
                    AccAmount accAmount = accEarFlow.Amounts.Find(amount => amount.IdEarType == amountIdEarType && amount.ExchangeType == amountExchangeType);
                    if (null == accAmount)
                    {
                        accAmount = new AccAmount(amountIdEarType, amountExchangeType, amountIDC, amountReceived, amountPaid, amountIdStProcess);
                        accEarFlow.Amounts.Add(accAmount);
                    }

                    amountIDE = (Convert.IsDBNull(rowAmount["IDE"]) ? 0 : Convert.ToInt32(rowAmount["IDE"]));

                    if (false == accAmount.Events.Exists(ide => ide == amountIDE))
                        accAmount.Events.Add(amountIDE);
                }
                #endregion EarAmount
            }
            catch (Exception ex)
            {
                FireException(ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05358",
                    new ProcessState(ProcessStateTools.StatusEnum.ERROR, ProcessStateTools.CodeReturnFailureEnum),
                        lstData.Find(match => match.First == "EAR").Second,
                        lstData.Find(match => match.First == "DTEVENT").Second,
                        lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        lstData.Find(match => match.First == "SOURCE").Second,
                        lstData.Find(match => match.First == "BOOK").Second,
                        lstData.Find(match => match.First == "TRADE").Second,
                        lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return codeReturn;
        }

        #region StreamNOAmounts
        private bool GetStreamAmounts()
        {
            bool pIsAlreadyAccounted = false;

            AccEarDetFlows accEarDet = EarDetFlows.Find(det => det.InstrumentNO == CurrentEarDet.InstrumentNO && det.StreamNO == CurrentEarDet.StreamNO);

            return GetStreamAmounts(accEarDet, ref pIsAlreadyAccounted);
        }
        private bool GetStreamAmounts(AccEarDetFlows pAccEarDetFlows, ref bool pIsAlreadyAccounted)
        {
            AccEntryFlow accEntryFlow;
            List<AccAmount> accAmount;
            //
            pIsAlreadyAccounted = false;
            //
            foreach (AccEarFlow flow in pAccEarDetFlows.Flows)
            {
                if (flow.EarCode == CurrentDefinedEntry.EarCode &&
                    flow.AmountType == CurrentDefinedEntry.AmountType &&
                    flow.EventClass == CurrentDefinedEntry.EventClass)
                {
                    // 
                    // If this amount exits in a collection of SreamNO 0 amounts Then
                    //		check if this amount is already accounted by a previous StreamNO ( so by the same Entry)
                    //
                    if ((pAccEarDetFlows.StreamNO != 0) ||
                        (false == flow.AccountedEntries.Exists(entry => entry == CurrentDefinedEntry.IDAccEntry)))
                    {
                        accEntryFlow = new AccEntryFlow(flow);

                        // RD 20111230 / Bug: non prise en compte de plusieurs flux de même type
                        // Obtenir la liste des montants avec la même contrevaleur, correspondant à l'écriture définie
                        accAmount = flow.Amounts.FindAll(amount => amount.ExchangeType == CurrentDefinedEntry.ExchangeType);

                        // 20090303 RD / Pour le cas de montants non calculés pour une contrevaleur donnée
                        if (ArrFunc.IsEmpty(accAmount))
                        {
                            accAmount = new List<AccAmount>
                            {
                                new AccAmount(0, CurrentDefinedEntry.ExchangeType)
                            };
                        }
                        //
                        accEntryFlow.Add(accAmount);
                        CurrentEntryFlows.Add(accEntryFlow);

                        // Pour noter que l'écriture définie, est déjà exploitée pour le Flux en cours, 
                        // ça évite de comptabiliser un flux plusieurs fois
                        flow.AccountedEntries.Add(CurrentDefinedEntry.IDAccEntry);
                    }
                    else
                        pIsAlreadyAccounted = true;
                    //
                    // RD 20120823 / C'est le premier flux (EARDAY, EARCOMMON ou bien EARCALC) qui est considéré
                    // le flux contient tous les montants
                    // Exemple:
                    // Si j'ai un EAR avec les montants suivants:
                    // - EARDAY     LPP/NTA/STL 100
                    // - EARDAY     LPP/NTA/STL 50
                    // - EARDAY     LPP/NTA/STL 50
                    // - EARCOMMON  LPP/NTA/STL 400
                    // - EARCOMMON  LPP/NTA/STL 200
                    // la méthode va retourner:
                    // - le premier flux qui est EARDAY LPP/NTA/STL (le flux EARCOMMON LPP/NTA/STL est ignoré)
                    // - avec les trois montants: 100, 50 et 50
                    return true;
                }
            }
            //
            return false;
        }
        #endregion StreamNOAmounts
        #region LoadCurrentEntryFlows
        /// <summary>
        /// Charger tous les flux qui match avec l'écritures comptable parametrée
        /// </summary>
        /// <param name="pEarDetFlowsStreamZero"></param>
        /// <param name="pIsAlreadyAccounted"></param>
        /// <param name="pIsStream0Amount"></param>
        /// <param name="pAccEar"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        public void GetCurrentEntryFlows(List<AccEarDetFlows> pEarDetFlowsStreamZero,
            ref bool pIsAlreadyAccounted, ref bool pIsStream0Amount, AccEar pAccEar, AccModel pAccModel, AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet)
        {
            try
            {
                pIsAlreadyAccounted = false;
                pIsStream0Amount = false;

                // Looking in a collection of current SreamNO amounts
                CurrentEntryFlows = new List<AccEntryFlow>();
                GetStreamAmounts();

                //
                // If amount does not exist in current StreamNO and 
                // StreamNO 0 exists in this EAR Then
                //		looking for this amount in a collection of SreamNO 0 amounts
                //				
                #region Looking in a collection of SreamNO 0 amounts
                if (CurrentEntryFlows.Count == 0 &&
                    IsSream0Exist &&
                    CurrentEarDet.StreamNO != 0 &&
                    CurrentDefinedEntry.EarCode != (EventCodeFunc.DailyClosing + "-1"))
                {
                    foreach (AccEarDetFlows earDetFlowsStreamZero in pEarDetFlowsStreamZero)
                    {
                        if (GetStreamAmounts(earDetFlowsStreamZero, ref pIsAlreadyAccounted))
                        {
                            pIsStream0Amount = true;
                            break;
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                FireException(ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05367",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                    pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                    pAccEar.CurrentDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                    pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                    pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                    pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }
        }
        #endregion LoadCurrentEntryFlows

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsDebit"></param>
        /// <param name="pAmount"></param>
        /// <param name="pAmountIDC"></param>
        /// <param name="pAccount"></param>
        /// <param name="pAccountValue"></param>
        /// <param name="pEarType"></param>
        /// <param name="pIdEarType"></param>
        /// <param name="pEvents"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        // RD 20140916 [20328] Add parameters pEarType and pIdEarType
        public void AddEntry(bool pIsDebit, decimal pAmount, string pAmountIDC, string pAccount, string pAccountValue,
            string pEarType, int pIdEarType, List<int> pEvents,
            AccModel pAccModel, AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet)
        {
            try
            {
                this.AccDayBooks.Add(new Pre_DayBook(this.IsEARRemoved,
                    this.CurrentEarDet, this.CurrentDefinedScheme, this.CurrentDefinedEntry,
                    pIsDebit, pAmount, pAmountIDC, pAccount, pAccountValue, pEarType, pIdEarType, pEvents));

                this.CurrentDefinedScheme.EntriesAmountNet += pAmount * (Convert.ToBoolean(pIsDebit) ? -1 : 1);
                if (this.CurrentDefinedEntry.IsReversal)
                    this.CurrentDefinedScheme.ReversalEntriesAmountNet += pAmount * (BoolFunc.IsFalse(pIsDebit) ? -1 : 1);
            }
            catch (Exception ex)
            {
                FireException(ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05366",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                    this.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                    this.CurrentDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                    pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                    pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                    pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                    this.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                    this.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                    this.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                    this.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                    this.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                    this.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                    this.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                    this.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }
        }
    }
    #endregion AccEar

    #region public class AccEarDet
    public class AccEarDet : AccLog
    {
        #region Members
        public int IDI;
        public int InstrumentNO;
        public int StreamNO;
        //
        public int IDAccModel;
        public int IDA_ENTITY;
        public int IDA_PARTY;
        public int IDB_PARTY;
        public int IDA_COUNTERPARTY;
        public int IDB_COUNTERPARTY;

        public List<Pair<string, string>> lstData = new List<Pair<string, string>>();
        public DataRow[] EarSchemes;
        public bool IsInstrEnvDetMatch;
        public bool IsKeyMatch;
        #endregion Members
        #region Constructor
        public AccEarDet( DataRow pRowEarDet, int pAccModelID,
            List<Pair<string, string>> pEarLstData, List<Pair<string, string>> pAccModelLstData)
            : base()
        {
            try
            {
                IDAccModel = pAccModelID;
                //
                InstrumentNO = Convert.ToInt32(pRowEarDet["INSTRUMENTNO"]);
                StreamNO = Convert.ToInt32(pRowEarDet["STREAMNO"]);
                IDI = (Convert.IsDBNull(pRowEarDet["IDI"]) ? 0 : Convert.ToInt32(pRowEarDet["IDI"]));
                //
                IDA_ENTITY = (Convert.IsDBNull(pRowEarDet["IDA_ENTITYPARTY"]) ? 0 : Convert.ToInt32(pRowEarDet["IDA_ENTITYPARTY"]));
                IDA_PARTY = (Convert.IsDBNull(pRowEarDet["IDA_PARTY"]) ? 0 : Convert.ToInt32(pRowEarDet["IDA_PARTY"]));
                IDB_PARTY = (Convert.IsDBNull(pRowEarDet["IDB_PARTY"]) ? 0 : Convert.ToInt32(pRowEarDet["IDB_PARTY"]));
                IDA_COUNTERPARTY = (Convert.IsDBNull(pRowEarDet["IDA_COUNTERPARTY"]) ? 0 : Convert.ToInt32(pRowEarDet["IDA_COUNTERPARTY"]));
                IDB_COUNTERPARTY = (Convert.IsDBNull(pRowEarDet["IDB_COUNTERPARTY"]) ? 0 : Convert.ToInt32(pRowEarDet["IDB_COUNTERPARTY"]));
                //
                //IsInstrEnvDetMatch    = false;
                //IsKeyMatch       = false;
                //		
                lstData = new List<Pair<string, string>>
                {
                    new Pair<string, string>("INSTR_IDENTIFIER",
                    LogTools.IdentifierAndId(Convert.IsDBNull(pRowEarDet["INSTR_IDENTIFIER"]) ? "N/A" : pRowEarDet["INSTR_IDENTIFIER"].ToString(), IDI)),
                    new Pair<string, string>("INSTRNO", InstrumentNO.ToString()),
                    new Pair<string, string>("STREAMNO", StreamNO.ToString())
                };
                lstData.AddRange(pEarLstData);
                lstData.AddRange(pAccModelLstData);
            }
            catch (Exception ex)
            {
                FireException(ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05376",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pAccModelLstData.Find(match => match.First == "ACCMODEL").Second,
                        pEarLstData.Find(match => match.First == "EAR").Second,
                        pEarLstData.Find(match => match.First == "DTEVENT").Second,
                        pEarLstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        pEarLstData.Find(match => match.First == "SOURCE").Second,
                        pEarLstData.Find(match => match.First == "BOOK").Second,
                        pEarLstData.Find(match => match.First == "TRADE").Second,
                        pEarLstData.Find(match => match.First == "PROCESSDATE").Second));
            }
        }
        #endregion Constructor
    }
    #endregion class AccEarDet
        
    #region public class Pre_DayBook
    // RD 20140916 [20328] Add parameters paramEARTYPE and paramIDEARTYPE
    public class Pre_DayBook
    {
        #region Members
        public bool IsReversal;
        public bool IsEarRemoved;
        public string DayTypeReversal;
        public List<int> Events;

        private readonly int paramPeriodMultipOffset;
        private readonly string paramPeriodOffset;
        private readonly string paramDayTypeOffset;

        private readonly int paramIDACCSCHEME;
        private readonly int paramIDACCENTRY;
        //
        private readonly int paramINSTRUMENTNO;
        private readonly int paramSTREAMNO;
        private readonly string paramEARTYPE;
        private readonly int paramIDEARTYPE;
        private readonly int paramIDA_ENTITY;
        private readonly int paramIDA_PARTY;
        private readonly int paramIDB_PARTY;
        private readonly int paramIDA_COUNTERPARTY;
        private readonly int paramIDB_COUNTERPARTY;
        //
        private readonly string paramCASHSECURITIES;
        private readonly string paramEARCODE;
        private readonly string paramEVENTCLASS;
        private readonly string paramEXCHANGETYPE;
        private readonly string paramQUOTEEXCHANGETYPE;
        private readonly string paramAMOUNTTYPE;
        //
        private readonly decimal paramAMOUNT;
        private readonly string paramIDC;
        private readonly bool paramISDEBIT;
        private readonly string paramACCOUNT;
        private readonly string paramACCOUNTVALUE;
        //
        private readonly string paramLABEL;
        private readonly string paramLABELVALUE;
        private readonly string paramDEACTIVLABEL;
        private readonly string paramDEACTIVLABELVALUE;
        private readonly string paramDEACTIVPOS;
        private readonly string paramREVERSALLABEL;
        private readonly string paramREVERSALLABELVALUE;
        private readonly string paramREVERSALPOS;
        //
        private readonly string paramLABEL2;
        private readonly string paramLABELVALUE2;
        private readonly string paramDEACTIVLABEL2;
        private readonly string paramDEACTIVLABELVALUE2;
        private readonly string paramDEACTIVPOS2;
        private readonly string paramREVERSALLABEL2;
        private readonly string paramREVERSALLABELVALUE2;
        private readonly string paramREVERSALPOS2;
        //
        private readonly string paramJOURNALCODE;
        private readonly string paramJOURNALCODEVALUE;
        private readonly string paramCONSOCODE;
        private readonly string paramCONSOCODEVALUE;
        #endregion
        #region Accessors

        #endregion
        #region Constructor
        // RD 20140916 [20328] Add parameters pEarType and pIdEarType
        public Pre_DayBook(bool pIsEarRemoved, AccEarDet pAccEarDet, AccDefinedScheme pAccDefinedScheme, AccDefinedEntry pAccDefinedEntry,
            bool pIsDebit, decimal pAmount, string pAMountIDC, string pAccount, string pAccountValue, string pEarType, int pIdEarType, List<int> pEvents)
        {
            IsEarRemoved = pIsEarRemoved;
            IsReversal = pAccDefinedEntry.IsReversal;
            DayTypeReversal = pAccDefinedEntry.DayTypeReversal;
            Events = pEvents;

            paramISDEBIT = pIsDebit;

            paramINSTRUMENTNO = pAccEarDet.InstrumentNO;
            paramSTREAMNO = pAccEarDet.StreamNO;

            paramEARTYPE = pEarType;
            paramIDEARTYPE = pIdEarType;

            paramIDA_ENTITY = pAccEarDet.IDA_ENTITY;
            paramIDA_PARTY = pAccEarDet.IDA_PARTY;
            paramIDB_PARTY = pAccEarDet.IDB_PARTY;
            paramIDA_COUNTERPARTY = pAccEarDet.IDA_COUNTERPARTY;
            paramIDB_COUNTERPARTY = pAccEarDet.IDB_COUNTERPARTY;

            paramCASHSECURITIES = pAccDefinedScheme.CashSecurities;
            paramIDACCSCHEME = pAccDefinedScheme.IDAccScheme;

            paramIDACCENTRY = pAccDefinedEntry.IDAccEntry;
            paramEARCODE = pAccDefinedEntry.EarCode;
            paramEVENTCLASS = pAccDefinedEntry.EventClass;
            paramEXCHANGETYPE = pAccDefinedEntry.ExchangeType;
            paramQUOTEEXCHANGETYPE = pAccDefinedEntry.QuoteExchangeType;
            paramAMOUNTTYPE = pAccDefinedEntry.AmountType;

            paramPeriodMultipOffset = pAccDefinedEntry.PeriodMultipOffset;
            paramPeriodOffset = pAccDefinedEntry.PeriodOffset;
            paramDayTypeOffset = pAccDefinedEntry.DayTypeOffset;

            paramLABEL = pAccDefinedEntry.LabelMain.Label;
            paramLABELVALUE = pAccDefinedEntry.LabelMain.LabelValue;
            paramDEACTIVLABEL = pAccDefinedEntry.LabelMain.DeactivLabel;
            paramDEACTIVLABELVALUE = pAccDefinedEntry.LabelMain.DeactivLabelValue;
            paramDEACTIVPOS = pAccDefinedEntry.LabelMain.DeactivPos;
            paramREVERSALLABEL = pAccDefinedEntry.LabelMain.ReversalLabel;
            paramREVERSALLABELVALUE = pAccDefinedEntry.LabelMain.ReversalLabelValue;
            paramREVERSALPOS = pAccDefinedEntry.LabelMain.ReversalPos;

            paramLABEL2 = pAccDefinedEntry.LabelSecondary.Label;
            paramLABELVALUE2 = pAccDefinedEntry.LabelSecondary.LabelValue;
            paramDEACTIVLABEL2 = pAccDefinedEntry.LabelSecondary.DeactivLabel;
            paramDEACTIVLABELVALUE2 = pAccDefinedEntry.LabelSecondary.DeactivLabelValue;
            paramDEACTIVPOS2 = pAccDefinedEntry.LabelSecondary.DeactivPos;
            paramREVERSALLABEL2 = pAccDefinedEntry.LabelSecondary.ReversalLabel;
            paramREVERSALLABELVALUE2 = pAccDefinedEntry.LabelSecondary.ReversalLabelValue;
            paramREVERSALPOS2 = pAccDefinedEntry.LabelSecondary.ReversalPos;

            paramJOURNALCODE = pAccDefinedEntry.JournalCode;
            paramJOURNALCODEVALUE = pAccDefinedEntry.JournalCodeValue;
            paramCONSOCODE = pAccDefinedEntry.ConsoCode;
            paramCONSOCODEVALUE = pAccDefinedEntry.ConsoCodeValue;

            paramAMOUNT = pAmount;
            paramIDC = pAMountIDC;
            paramACCOUNT = pAccount;
            paramACCOUNTVALUE = pAccountValue;
        }
        #endregion
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAccQuery"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pIDBC"></param>
        // RD 20140916 [20328] Add parameters paramEARTYPE and paramIDEARTYPE
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void Initialize(AccQuery pAccQuery, IProductBase pProductBase, string pIDBC)
        {
            // RD 20121025 [18201] Inverser le sens ("Debit" devient "Credit" et vice versa) pour un EAR Removed
            pAccQuery.paramISDEBIT.Value = (IsEarRemoved ? !paramISDEBIT : paramISDEBIT);
            pAccQuery.paramAMOUNT.Value = paramAMOUNT;
            pAccQuery.paramIDC.Value = paramIDC;
            pAccQuery.paramACCOUNT.Value = paramACCOUNT;
            pAccQuery.paramACCOUNTVALUE.Value = paramACCOUNTVALUE;

            pAccQuery.paramIDA_ENTITY.Value = paramIDA_ENTITY;
            pAccQuery.paramIDA_PARTY.Value = paramIDA_PARTY;
            pAccQuery.paramIDB_PARTY.Value = paramIDB_PARTY;
            pAccQuery.paramIDA_COUNTERPARTY.Value = paramIDA_COUNTERPARTY;
            pAccQuery.paramIDB_COUNTERPARTY.Value = (paramIDB_COUNTERPARTY == 0 ? Convert.DBNull : paramIDB_COUNTERPARTY);

            pAccQuery.paramInstrumentNo.Value = paramINSTRUMENTNO;
            pAccQuery.paramStreamNo.Value = paramSTREAMNO;

            pAccQuery.paramEarType.Value = paramEARTYPE;
            pAccQuery.paramIdEarType.Value = paramIDEARTYPE;

            pAccQuery.paramCASHSECURITIES.Value = paramCASHSECURITIES;

            pAccQuery.paramIDACCSCHEME.Value = paramIDACCSCHEME;
            pAccQuery.paramIDACCENTRY.Value = paramIDACCENTRY;
            pAccQuery.paramEarCode.Value = paramEARCODE;
            pAccQuery.paramEventClass.Value = paramEVENTCLASS;
            pAccQuery.paramEXCHANGETYPE.Value = paramEXCHANGETYPE;
            pAccQuery.paramQUOTEEXCHANGETYPE.Value = paramQUOTEEXCHANGETYPE;
            pAccQuery.paramAmountType.Value = paramAMOUNTTYPE;

            pAccQuery.paramDTENTRY.Value = pAccQuery.paramDTEVENT.Value;

            #region Calculate ACCDAYBOOK Entry date according to Entry OFFSET
            if (paramPeriodMultipOffset != 0 &&
                StrFunc.IsFilled(paramPeriodOffset) &&
                StrFunc.IsFilled(paramDayTypeOffset))
            {
                IBusinessDayAdjustments bda = pProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, pIDBC);
                PeriodEnum period = StringToEnum.Period(paramPeriodOffset);
                DayTypeEnum dayType = StringToEnum.DayType(paramDayTypeOffset);
                //
                if ((DayTypeEnum.Business == dayType) && StrFunc.IsEmpty(pIDBC))
                    dayType = DayTypeEnum.Calendar;
                //
                IOffset offset = pProductBase.CreateOffset(period, paramPeriodMultipOffset, dayType);
                EFS_Offset efsOffset = new EFS_Offset(pAccQuery.CS, offset, Convert.ToDateTime(pAccQuery.paramDTEVENT.Value), bda, null);
                if (DtFunc.IsDateTimeFilled(efsOffset.offsetDate[0]))
                    pAccQuery.paramDTENTRY.Value = efsOffset.offsetDate[0];
            }
            #endregion

            pAccQuery.paramLABEL.Value = (StrFunc.IsFilled(paramLABEL) ? (object)paramLABEL : DBNull.Value);
            pAccQuery.paramLABELVALUE.Value = (StrFunc.IsFilled(paramLABELVALUE) ? (object)paramLABELVALUE : DBNull.Value);
            pAccQuery.paramDEACTIVLABEL.Value = (StrFunc.IsFilled(paramDEACTIVLABEL) ? (object)paramDEACTIVLABEL : DBNull.Value);
            pAccQuery.paramDEACTIVLABELVALUE.Value = (StrFunc.IsFilled(paramDEACTIVLABELVALUE) ? (object)paramDEACTIVLABELVALUE : DBNull.Value);
            pAccQuery.paramDEACTIVPOS.Value = (StrFunc.IsFilled(paramDEACTIVPOS) ? (object)paramDEACTIVPOS : DBNull.Value);
            pAccQuery.paramREVERSALLABEL.Value = (StrFunc.IsFilled(paramREVERSALLABEL) ? (object)paramREVERSALLABEL : DBNull.Value);
            pAccQuery.paramREVERSALLABELVALUE.Value = (StrFunc.IsFilled(paramREVERSALLABELVALUE) ? (object)paramREVERSALLABELVALUE : DBNull.Value);
            pAccQuery.paramREVERSALPOS.Value = (StrFunc.IsFilled(paramREVERSALPOS) ? (object)paramREVERSALPOS : DBNull.Value);

            pAccQuery.paramLABEL2.Value = (StrFunc.IsFilled(paramLABEL2) ? (object)paramLABEL2 : DBNull.Value);
            pAccQuery.paramLABELVALUE2.Value = (StrFunc.IsFilled(paramLABELVALUE2) ? (object)paramLABELVALUE2 : DBNull.Value);
            pAccQuery.paramDEACTIVLABEL2.Value = (StrFunc.IsFilled(paramDEACTIVLABEL2) ? (object)paramDEACTIVLABEL2 : DBNull.Value);
            pAccQuery.paramDEACTIVLABELVALUE2.Value = (StrFunc.IsFilled(paramDEACTIVLABELVALUE2) ? (object)paramDEACTIVLABELVALUE2 : DBNull.Value);
            pAccQuery.paramDEACTIVPOS2.Value = (StrFunc.IsFilled(paramDEACTIVPOS2) ? (object)paramDEACTIVPOS2 : DBNull.Value);
            pAccQuery.paramREVERSALLABEL2.Value = (StrFunc.IsFilled(paramREVERSALLABEL2) ? (object)paramREVERSALLABEL2 : DBNull.Value);
            pAccQuery.paramREVERSALLABELVALUE2.Value = (StrFunc.IsFilled(paramREVERSALLABELVALUE2) ? (object)paramREVERSALLABELVALUE2 : DBNull.Value);
            pAccQuery.paramREVERSALPOS2.Value = (StrFunc.IsFilled(paramREVERSALPOS2) ? (object)paramREVERSALPOS2 : DBNull.Value);

            // RD 20121025 [18201] Calculer le libellé d'Annulation                        
            if (IsEarRemoved)
            {
                pAccQuery.paramLABELVALUE.Value = GetParamLabelValue(pAccQuery.paramLABELVALUE, pAccQuery.paramDEACTIVPOS, pAccQuery.paramDEACTIVLABELVALUE);
                pAccQuery.paramLABELVALUE2.Value = GetParamLabelValue(pAccQuery.paramLABELVALUE2, pAccQuery.paramDEACTIVPOS2, pAccQuery.paramDEACTIVLABELVALUE2);
            }

            pAccQuery.paramJOURNALCODE.Value = (StrFunc.IsFilled(paramJOURNALCODE) ? (object)paramJOURNALCODE : DBNull.Value);
            pAccQuery.paramJOURNALCODEVALUE.Value = (StrFunc.IsFilled(paramJOURNALCODEVALUE) ? (object)paramJOURNALCODEVALUE : DBNull.Value);
            pAccQuery.paramCONSOCODE.Value = (StrFunc.IsFilled(paramCONSOCODE) ? (object)paramCONSOCODE : DBNull.Value);
            pAccQuery.paramCONSOCODEVALUE.Value = (StrFunc.IsFilled(paramCONSOCODEVALUE) ? (object)paramCONSOCODEVALUE : DBNull.Value);
        }
        /// <summary>
        /// Enrichir le Libellé avec un Libellé additonnel selon la position spécifiée
        /// </summary>
        /// <param name="pLABELVALUE"></param>
        /// <param name="pPOS"></param>
        /// <param name="pAdditionnalLABELVALUE"></param>
        /// <returns></returns>
        public static string GetParamLabelValue(IDbDataParameter pLABELVALUE, IDbDataParameter pPOS, IDbDataParameter pAdditionnalLABELVALUE)
        {
            string ret = pLABELVALUE.Value.ToString();
            if (pPOS.Value != DBNull.Value && pAdditionnalLABELVALUE.Value != DBNull.Value)
            {
                Cst.LabelPositionEnum labelPos = (Cst.LabelPositionEnum)Enum.Parse(typeof(Cst.LabelPositionEnum), pPOS.Value.ToString(), true);
                string additionnalLabel = pAdditionnalLABELVALUE.Value.ToString();
                if (labelPos == Cst.LabelPositionEnum.PREFIX)
                    ret = additionnalLabel + ret;
                else if (labelPos == Cst.LabelPositionEnum.SUFFIX)
                    ret += additionnalLabel;
            }
            return ret;
        }
        #endregion
    }
    #endregion class Pre_DayBook

    #region class AccountGenProcess
    public class AccountGenProcess : ProcessTradeBase
    {
        #region Members
        private readonly AccountGenMQueue accGenMQueue;
        /// <summary>
        /// 0: Trade identifier
        /// <para>1: IDT</para>
        /// <para>2: Process date</para>
        /// </summary>
        readonly List<Pair<string, string>> m_LstMasterData = new List<Pair<string, string>>();
        private EFS_TradeLibrary m_tradeLibrary;
        private AccTradeInfo m_TradeInfo;
        private AccEventInfo m_EventsInfo;
        private readonly DateTime m_AccDate;

        private List<AccBook> m_Books;
        private List<AccModel> m_Models;

        private List<AccCondition> m_Conditions;
        private List<AccVariable> m_AccVariables;

        private bool m_IsAccErrorNoAmount;

        private AccQuery m_AccQuery;
        #endregion Members
        #region Constructor
        public AccountGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
#if DEBUG
            diagnosticDebug.Write("Accdaybook generation for trade: " + pMQueue.identifier + " (" + DtFunc.DateTimeToStringISO(DateTime.Now) + ") **********");
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            try
            {
                accGenMQueue = (AccountGenMQueue)pMQueue;
                if (false == this.IsProcessObserver)
                {
                    // EG 20121022
                    m_LstMasterData = new List<Pair<string, string>>();
                    Pair<string, string> trade = new Pair<string, string>("TRADE", "N/A");
                    if (accGenMQueue.idSpecified && accGenMQueue.identifierSpecified)
                        trade.Second = LogTools.IdentifierAndId(accGenMQueue.identifier, accGenMQueue.id);
                    else if (accGenMQueue.identifierSpecified)
                        trade.Second = LogTools.IdentifierAndId(accGenMQueue.identifierSpecified ? accGenMQueue.identifier : "N/A");
                    m_LstMasterData.Add(trade);

                    if (accGenMQueue.IsMasterDateSpecified)
                    {
                        m_AccDate = accGenMQueue.AccountDate;
                        m_LstMasterData.Add(new Pair<string, string>("PROCESSDATE", DtFunc.DateTimeToStringDateISO(m_AccDate)));
                    }
                    else
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05344",
                            new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND),
                            m_LstMasterData.Find(match => match.First == "TRADE").Second);
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-00010", new ProcessState(ProcessStateTools.StatusErrorEnum), ex); }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }
        #endregion Constructor
        #region Methods
        

        /// <summary>
        /// Charger la liste des variables comptables à partir de la table ACCVARIABLE
        /// </summary>
        /// <param name="pAccEar"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pDefinedScheme"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        // RD 20150814 [21280] Modify
        private void LoadAccVariable(AccEar pAccEar, AccModel pAccModel, AccDefinedScheme pDefinedScheme, AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            try
            {                
                pAccEar.CurrentDefinedEntry.InitVariables();

                try
                {
                    // RD 20150814 [21280] Move code to AccVariableTools.LoadAccVariable
                    AccVariableTools.LoadAccVariable(Cs, pAccEar.CurrentDefinedEntry.Variables, ref m_AccVariables);
                }
                catch (Exception ex)
                {
                    AccLog.FireException( ex,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05368",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                        pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
                }

                foreach (string stringVar in pAccEar.CurrentDefinedEntry.Variables)
                {
                    if (false == m_AccVariables.Exists(var => var.Regex.Match(stringVar).Success))
                    {
                        AccLog.FireException( null,
                            new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05347",
                                new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                stringVar,
                                pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                                pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                                pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second));
                    }
                }
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05371",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                        pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));

            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }

        /// <summary>
        /// Valorisation des variables comptables dans les membres de {pAccEar.CurrentDefinedEntry} susceptibles de contenir des variables
        /// </summary>
        /// <param name="pAccEar"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pDefinedScheme"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        private void ValuateAccVariable(bool pByEventStep, AccEar pAccEar, AccModel pAccModel, AccDefinedScheme pDefinedScheme, AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            string logMessage = string.Empty;

            try
            {
                string strSqlVar = string.Empty;
                string sqlQuery = string.Empty;
                string strVar = string.Empty;
                string strVarValue = string.Empty;

                AccVariable accVariable = null;

                if (pByEventStep)
                {
                    // Initialise les membres avec le suffix 'ByEvent' à partir des membres avec le suffix 'Value'
                    pAccEar.CurrentDefinedEntry.InitStringValueForEvent();
                }
                else
                {
                    // Initialise les membres avec le suffix 'Value' à partir des membres correspondants susceptibles de contenir des variables 
                    pAccEar.CurrentDefinedEntry.InitStringValue();
                }

                // La liste des tables et des colonnes
                IEnumerable<IGrouping<string, AccVariable>> lstGrpTable =
                    (from table in
                         (from accVariableItem in m_AccVariables
                          from currentVariableItem in pAccEar.CurrentDefinedEntry.Variables
                          where accVariableItem.Regex.Match(currentVariableItem).Success
                          select accVariableItem).GroupBy(item => item.Table)
                     select table);

                IEnumerable<Pair<string, string>> lstTable =
                    (from table in lstGrpTable
                     where m_AccVariables.Exists(variable => variable.Table == table.Key && variable.IsTableWithIDE == pByEventStep)
                     select new Pair<string, string>
                         (table.Key, (from row in table select "tblData." + row.Column + " as " + row.IDAccVariable).Aggregate((i, j) => i + "," + j)));

                DataParameters parameters = new DataParameters();

                foreach (Pair<string, string> table in lstTable)
                {
                    if (pByEventStep)
                    {
                        sqlQuery = AccVariableTools.SQL_Select_TableDataWithIDE
                            .Replace("{columnList}", table.Second.TrimEnd(','))
                            .Replace("{table}", table.First);

                        parameters.Add(m_AccQuery.paramIdEAR);
                        parameters.Add(m_AccQuery.paramEarType);
                        parameters.Add(m_AccQuery.paramIdEarType);
                    }
                    else
                    {
                        sqlQuery = AccVariableTools.SQL_Select_TableData
                            .Replace("{columnList}", table.Second.TrimEnd(','))
                            .Replace("{table}", table.First);

                        parameters.Add(m_AccQuery.paramIdEAR);
                        parameters.Add(m_AccQuery.paramInstrumentNo);
                        parameters.Add(m_AccQuery.paramStreamNo);
                        parameters.Add(m_AccQuery.paramNoZero);
                        parameters.Add(m_AccQuery.paramEarCode);
                        parameters.Add(m_AccQuery.paramAmountType);
                        parameters.Add(m_AccQuery.paramEventClass);
                    }

                    QueryParameters qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                    using (IDataReader drVariableVal = DataHelper.ExecuteReader(Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter()))
                    {
                        if (drVariableVal.Read())
                        {
                            for (int j = 0; j < drVariableVal.FieldCount; j++)
                            {
                                #region Valuate variables
                                strSqlVar = drVariableVal.GetName(j);
                                strVar = AccVariableTools.ELEMENTSTART + strSqlVar + AccVariableTools.ELEMENTEND;
                                strVarValue = (Convert.IsDBNull(drVariableVal[j]) ? string.Empty : drVariableVal[j].ToString());
                                //
                                logMessage = "SYS-05348";//The table or the view parametrized for this variable restores no value corresponding to the accouting entry

                                // RD 20120809 [18070] Optimisation
                                // Instanciation du DataDocument (EFS_TradeLibrary) uniquement s'il existe des variables NOSTRO...
                                if (strSqlVar == AccVariableTools.ELEMENTNOSTRO ||
                                    strSqlVar == AccVariableTools.ELEMENTNOSTROJOURNALCODE ||
                                    strSqlVar == AccVariableTools.ELEMENTACCOUNT)
                                {
                                    #region Load dataDocument
                                    if (StrFunc.IsEmpty(strVarValue))
                                    {
                                        logMessage = "SYS-05349";//The settlement instructions are unavailable
                                    }
                                    else
                                    {
                                        if (null == this.m_tradeLibrary)
                                            m_tradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);

                                        if (strSqlVar == AccVariableTools.ELEMENTNOSTRO)
                                        {
                                            strVarValue = SettlementTools.GetNostroAccountNumber(Cs, this.m_tradeLibrary.DataDocument.DataDocument, strVarValue);
                                            if (StrFunc.IsEmpty(strVarValue))
                                                logMessage = "SYS-05365";//The settlement instructions are available, but no account was found.
                                        }
                                        //
                                        if (strSqlVar == AccVariableTools.ELEMENTNOSTROJOURNALCODE)
                                        {
                                            strVarValue = SettlementTools.GetNostroJournalCode(
                                                    Cs, this.m_tradeLibrary.DataDocument.DataDocument, strVarValue);
                                            if (StrFunc.IsEmpty(strVarValue))
                                                //The settlement instructions are available, but no account was found.
                                                logMessage = "SYS-05365";
                                        }
                                        //
                                        if (strSqlVar == AccVariableTools.ELEMENTACCOUNT)
                                        {
                                            strVarValue = SettlementTools.GetInternalAccountNumber(this.m_tradeLibrary.DataDocument.DataDocument, strVarValue);
                                            if (StrFunc.IsEmpty(strVarValue))
                                                logMessage = "SYS-05365";//The settlement instructions are available, but no account was found.
                                        }
                                    }
                                    #endregion
                                }

                                accVariable = m_AccVariables.Find(var => var.IDAccVariable == strSqlVar);

                                if (StrFunc.IsEmpty(strVarValue))
                                {
                                    // Unable to evaluate variable
                                    AccLog.FireException( null,
                                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, logMessage,
                                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                        accVariable.Data,
                                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second,
                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second));
                                }

                                // Remplacer la variable par sa valaur dans tous les membres avec le suffix "Value"
                                pAccEar.CurrentDefinedEntry.ValuateStringValue(accVariable.Regex, strVarValue);
                                #endregion
                            }
                        }
                    }
                }

                if (pByEventStep == false)
                {
                    // Initialise les membres avec le suffix 'Value' à partir des membres correspondants susceptibles de contenir des variables 
                    pAccEar.CurrentDefinedEntry.InitStringByEvent();
                }
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05371",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                        pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));

            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }

        #region IsVerifyEntryCondition
        /// <summary>
        /// True si l'EAR det match avec la condition du schéma
        /// </summary>
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public bool IsCurrentEarDetVerifyEntryCondition_OBSOLETTE(AccEar pAccEar, AccModel pAccModel, AccDefinedScheme pDefinedScheme,
            AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet)
        {
#if DEBUG
            diagnosticDebug.Start(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            AccCondition accCondition = null;
            bool isVerify = false;
            DataParameters parameters = new DataParameters();
            QueryParameters qryParam = null;

            try
            {
                if ((int)m_AccQuery.paramIdAccCondition.Value != 0)
                {
                    #region Entry Condition
                    string sqlQuery = string.Empty;

                    if (false == m_Conditions.Exists(condition => condition.IdAccCondition == (int)m_AccQuery.paramIdAccCondition.Value))
                    {
                        #region Load Condition from referential
                        sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                        sqlQuery += "IDENTIFIER, SQLCONDITION, TABLEDATA, BYINSTRUMENTNO, BYSTREAMNO, BYEARCODE, BYEVENTTYPE, BYEVENTCLASS " + Cst.CrLf;
                        sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCCONDITION.ToString() + Cst.CrLf;
                        sqlQuery += SQLCst.WHERE + "IDACCCONDITION = @IDACCCONDITION" + Cst.CrLf;
                        sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, Cst.OTCml_TBL.ACCCONDITION) + Cst.CrLf;

                        parameters.Add(m_AccQuery.paramIdAccCondition);
                        qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                        using (IDataReader conditionData = DataHelper.ExecuteReader(CSTools.SetCacheOn(Cs), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter()))
                        {
                            if (conditionData.Read())
                            {
                                string identifier = conditionData["IDENTIFIER"].ToString();
                                string sqlCondition = conditionData["SQLCONDITION"].ToString();
                                string tableData = conditionData["TABLEDATA"].ToString();
                                //
                                bool byInstrumentNo = BoolFunc.IsTrue(conditionData["BYINSTRUMENTNO"].ToString());
                                bool byStreamNo = BoolFunc.IsTrue(conditionData["BYSTREAMNO"].ToString());
                                bool byEarCode = BoolFunc.IsTrue(conditionData["BYEARCODE"].ToString());
                                bool byEventType = BoolFunc.IsTrue(conditionData["BYEVENTTYPE"].ToString());
                                bool byEventClass = BoolFunc.IsTrue(conditionData["BYEVENTCLASS"].ToString());
                                //
                                m_Conditions.Add(new AccCondition((int)m_AccQuery.paramIdAccCondition.Value, identifier, sqlCondition, tableData,
                                    byInstrumentNo, byStreamNo, byEarCode, byEventType, byEventClass, false));
                            }
                        }
                        #endregion
                    }

                    accCondition = m_Conditions.Find(condition => condition.IdAccCondition == (int)m_AccQuery.paramIdAccCondition.Value);

                    #region VerifyCondition
                    sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT + "1" + Cst.CrLf;
                    sqlQuery += SQLCst.X_FROM + accCondition.TableData + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + accCondition.TableData + ".IDEAR = @IDEAR" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + accCondition.SqlCondition.TrimEnd() + Cst.CrLf;

                    parameters = new DataParameters();
                    parameters.Add(m_AccQuery.paramIdEAR);
                    if (accCondition.ByInstrumentNo)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".INSTRUMENTNO = @INSTRUMENTNO" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramInstrumentNo);
                    }
                    if (accCondition.ByStreamNo)
                    {
                        sqlQuery += SQLCst.AND + " (" + accCondition.TableData + ".STREAMNO = @STREAMNO" + Cst.CrLf;
                        sqlQuery += SQLCst.OR + accCondition.TableData + ".STREAMNO = 0 )" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramStreamNo);
                    }
                    if (accCondition.ByEarCode)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".EARCODE = @EARCODE" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramEarCode);
                    }
                    if (accCondition.ByEventType)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".EVENTTYPE = @AMOUNTTYPE" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramAmountType);
                    }
                    if (accCondition.ByEventClass)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".EVENTCLASS = @EVENTCLASS" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramEventClass);
                    }

                    qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                    using (IDataReader conditionResult = DataHelper.ExecuteReader(CSTools.SetCacheOn(Cs), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter()))
                    {
                        if (conditionResult.Read())
                        {
                            int result = (Convert.IsDBNull(conditionResult.GetValue(0)) ? 0 : Convert.ToInt32(conditionResult.GetValue(0)));
                            if (result == 1)
                                isVerify = true;
                        }
                    }
                    #endregion
                    //
                    if (!isVerify)
                    {
                        // Log: Condition does not match                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5343), 0,
                            new LogParam(accCondition.lstData.Find(match => match.First == "ACCCOND").Second),
                            new LogParam(pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second),
                            new LogParam(pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second),
                            new LogParam(pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second)));
                    }
                    #endregion  Entry Condition
                }
                else
                    isVerify = true;
                //
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05359",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        (null != accCondition) ? accCondition.lstData.Find(match => match.First == "ACCCOND").Second :
                            m_AccQuery.paramIdAccCondition.Value.ToString(),
                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                        pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }
            return isVerify;
        }

        /// <summary>
        /// Chargement de la condition du schéma
        /// <para>Vérification si l'EAR Det (Stream, Instrument) match avec la condition</para>
        /// <para>Renvoi la condition chargée et la requête SQL de vérification</para>
        /// </summary>
        /// <param name="pAccEar"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pDefinedScheme"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        /// <param name="pAccCondition">La condition chargée</param>
        /// <param name="pVerifyQryParameters">la requête SQL de vérification</param>
        /// <returns>True si l'EAR Det (Stream, Instrument) match avec la condition du schéma</returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public bool IsCurrentEarDetVerifyEntryCondition(AccEar pAccEar, AccModel pAccModel, AccDefinedScheme pDefinedScheme,
            AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet,
            out AccCondition pAccCondition, out QueryParameters pVerifyQryParameters)
        {
#if DEBUG
            diagnosticDebug.Start(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            pAccCondition = null;
            pVerifyQryParameters = null;

            AccCondition accCondition = null;
            bool isVerify = false;

            DataParameters parameters = new DataParameters();
            QueryParameters qryParam = null;

            try
            {
                if ((int)m_AccQuery.paramIdAccCondition.Value != 0)
                {
                    string sqlQuery = string.Empty;

                    if (false == m_Conditions.Exists(condition => condition.IdAccCondition == (int)m_AccQuery.paramIdAccCondition.Value))
                    {
                        #region Load Condition from referential
                        sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                        sqlQuery += "IDENTIFIER, SQLCONDITION, TABLEDATA, BYINSTRUMENTNO, BYSTREAMNO, BYEARCODE, BYEVENTTYPE, BYEVENTCLASS, BYEVENT " + Cst.CrLf;
                        sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCCONDITION.ToString() + Cst.CrLf;
                        sqlQuery += SQLCst.WHERE + "IDACCCONDITION = @IDACCCONDITION" + Cst.CrLf;
                        sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, Cst.OTCml_TBL.ACCCONDITION) + Cst.CrLf;

                        parameters.Add(m_AccQuery.paramIdAccCondition);
                        qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                        using (IDataReader conditionData = DataHelper.ExecuteReader(CSTools.SetCacheOn(Cs), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter()))
                        {
                            if (conditionData.Read())
                            {
                                string identifier = conditionData["IDENTIFIER"].ToString();
                                string sqlCondition = conditionData["SQLCONDITION"].ToString();
                                string tableData = conditionData["TABLEDATA"].ToString();
                                //
                                bool byInstrumentNo = BoolFunc.IsTrue(conditionData["BYINSTRUMENTNO"].ToString());
                                bool byStreamNo = BoolFunc.IsTrue(conditionData["BYSTREAMNO"].ToString());
                                bool byEarCode = BoolFunc.IsTrue(conditionData["BYEARCODE"].ToString());
                                bool byEventType = BoolFunc.IsTrue(conditionData["BYEVENTTYPE"].ToString());
                                bool byEventClass = BoolFunc.IsTrue(conditionData["BYEVENTCLASS"].ToString());
                                bool byEvent = BoolFunc.IsTrue(conditionData["BYEVENT"].ToString());
                                //
                                m_Conditions.Add(new AccCondition((int)m_AccQuery.paramIdAccCondition.Value, identifier, sqlCondition, tableData,
                                    byInstrumentNo, byStreamNo, byEarCode, byEventType, byEventClass, byEvent));
                            }
                        }
                        #endregion
                    }

                    accCondition = m_Conditions.Find(condition => condition.IdAccCondition == (int)m_AccQuery.paramIdAccCondition.Value);

                    sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT + "1" + Cst.CrLf;
                    sqlQuery += SQLCst.X_FROM + accCondition.TableData + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + accCondition.TableData + ".IDEAR = @IDEAR" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + accCondition.SqlCondition.TrimEnd() + Cst.CrLf;

                    parameters = new DataParameters();
                    parameters.Add(m_AccQuery.paramIdEAR);

                    if (accCondition.ByInstrumentNo)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".INSTRUMENTNO = @INSTRUMENTNO" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramInstrumentNo);
                    }
                    if (accCondition.ByStreamNo)
                    {
                        sqlQuery += SQLCst.AND + " (" + accCondition.TableData + ".STREAMNO = @STREAMNO" + Cst.CrLf;
                        sqlQuery += SQLCst.OR + accCondition.TableData + ".STREAMNO = 0 )" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramStreamNo);
                    }
                    if (accCondition.ByEarCode)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".EARCODE = @EARCODE" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramEarCode);
                    }
                    if (accCondition.ByEventType)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".EVENTTYPE = @AMOUNTTYPE" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramAmountType);
                    }
                    if (accCondition.ByEventClass)
                    {
                        sqlQuery += SQLCst.AND + accCondition.TableData + ".EVENTCLASS = @EVENTCLASS" + Cst.CrLf;
                        parameters.Add(m_AccQuery.paramEventClass);
                    }

                    pVerifyQryParameters = new QueryParameters(Cs, sqlQuery, parameters);
                    pAccCondition = accCondition;

                    if (accCondition.ByEvent == false)
                    {
                        isVerify = IsVerifyEntryCondition(pAccEar, pAccModel, pDefinedScheme, pAccInstrEnv, pAccInstrEnvDet, pAccCondition, pVerifyQryParameters);
                    }
                    else
                        isVerify = true;
                }
                else
                    isVerify = true;
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05359",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        (null != accCondition) ? accCondition.lstData.Find(match => match.First == "ACCCOND").Second :
                            m_AccQuery.paramIdAccCondition.Value.ToString(),
                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                        pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return isVerify;
        }

        /// <summary>
        /// Vérification si l'Evénement (Montant en cours) match avec la condition
        /// </summary>
        /// <param name="pAccEar"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pDefinedScheme"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        /// <param name="pIde"></param>
        /// <param name="pAccCondition">La condition à vérifier</param>
        /// <param name="pVerifyQryParameters">la requête SQL de vérification</param>
        /// <returns>True si l'Evénement (Montant en cours) match avec la condition du schéma</returns>
        public bool IsCurrentEventVerifyEntryCondition(AccEar pAccEar, AccModel pAccModel, AccDefinedScheme pDefinedScheme,
            AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet,
            int pIde, AccCondition pAccCondition, QueryParameters pVerifyQryParameters)
        {
#if DEBUG
            diagnosticDebug.Start(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            bool isVerify = false;
            if ((int)m_AccQuery.paramIdAccCondition.Value != 0 && pAccCondition.ByEvent)
            {
                pVerifyQryParameters.Query += SQLCst.AND + pAccCondition.TableData + ".IDE = @IDE" + Cst.CrLf;
                DataParameter dpIDE = new DataParameter(Cs, "IDE", DbType.Int32);
                if (pVerifyQryParameters.Parameters.Contains(dpIDE.ParameterKey))
                    dpIDE = pVerifyQryParameters.Parameters[dpIDE.ParameterKey];
                else
                    pVerifyQryParameters.Parameters.Add(dpIDE);

                dpIDE.Value = pIde;

                isVerify = IsVerifyEntryCondition(pAccEar, pAccModel, pDefinedScheme, pAccInstrEnv, pAccInstrEnvDet, pAccCondition, pVerifyQryParameters);
            }
            else
                isVerify = true;
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return isVerify;
        }

        /// <summary>
        /// Vérification de la condition en lançant la requête SQL de vérification 
        /// </summary>
        /// <param name="pAccEar"></param>
        /// <param name="pAccModel"></param>
        /// <param name="pDefinedScheme"></param>
        /// <param name="pAccInstrEnv"></param>
        /// <param name="pAccInstrEnvDet"></param>
        /// <param name="pAccCondition">La condition à vérifier</param>
        /// <param name="pVerifyQryParameters">la requête SQL de vérification</param>
        /// <returns>True si la condition est vérifiée</returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public bool IsVerifyEntryCondition(AccEar pAccEar, AccModel pAccModel, AccDefinedScheme pDefinedScheme,
            AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet, AccCondition pAccCondition, QueryParameters pVerifyQryParameters)
        {
#if DEBUG
            diagnosticDebug.Start(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            bool isVerify = false;

            try
            {
                using (IDataReader conditionResult = DataHelper.ExecuteReader(CSTools.SetCacheOn(Cs), CommandType.Text, pVerifyQryParameters.Query, pVerifyQryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (conditionResult.Read())
                    {
                        int result = (Convert.IsDBNull(conditionResult.GetValue(0)) ? 0 : Convert.ToInt32(conditionResult.GetValue(0)));
                        if (result == 1)
                            isVerify = true;
                    }
                }

                if (!isVerify)
                {
                    // Log: Condition does not match                        
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5343), 0,
                        new LogParam(pAccCondition.lstData.Find(match => match.First == "ACCCOND").Second),
                        new LogParam(pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second),
                        new LogParam(pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second),
                        new LogParam(pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                        new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second),
                        new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second)));
                }
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05359",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                        (null != pAccCondition) ? pAccCondition.lstData.Find(match => match.First == "ACCCOND").Second :
                            m_AccQuery.paramIdAccCondition.Value.ToString(),
                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                        pDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                        pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return isVerify;
        }
        #endregion IsVerifyEntryCondition

        #region GenerateEntriesForScheme
        /// <summary>
        /// Génération des écritures comptables pour chaque schéma parametré 
        /// </summary>
        // RD 20140729 [20201] Gestion du critère "ByEvent" sur les conditions
        public Cst.ErrLevel GenerateEntriesForScheme(AccEar pAccEar, AccModel pAccModel,
            List<AccEarDetFlows> pEarDetFlowsStreamZero, List<AccDefinedScheme> pAccDefinedSchemes,
            AccInstrEnv pAccInstrEnv, AccInstrEnvDet pAccInstrEnvDet)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            string logMessage = string.Empty;
            // EG 20160404 Migration vs2013
            bool isMsgWhenNoFlowsFounded = false;

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            ProcessState processState;

            try
            {
                decimal amountPaid = 0;
                decimal amountReceived = 0;
                string amountIDC;
                string amountIdStProcess;
                int countEntryForAmount = 0;

                foreach (AccDefinedScheme accDefinedScheme in pAccDefinedSchemes)
                {
                    if (accDefinedScheme.DefinedEntries.Count == 0)
                    {
                        // No activated entry defined
                        codeReturn = Cst.ErrLevel.MISSINGPARAMETER;
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.LOG, 5342), 0,
                            new LogParam(accDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second),
                            new LogParam(pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                            new LogParam(pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                                pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                                pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                                pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")"),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second),
                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second)));
                    }
                    else
                    {
                        #region Traitement
                        pAccEar.CurrentDefinedScheme = accDefinedScheme;

                        foreach (AccDefinedEntry definedEntry in accDefinedScheme.DefinedEntries)
                        {
                            bool isDebitAccountPayFilled = StrFunc.IsFilled(definedEntry.DebitAccountPay);
                            bool isDebitAccountRecFilled = StrFunc.IsFilled(definedEntry.DebitAccountRec);
                            bool isCreditAccountPayFilled = StrFunc.IsFilled(definedEntry.CreditAccountPay);
                            bool isCreditAccountRecFilled = StrFunc.IsFilled(definedEntry.CreditAccountRec);

                            // Si aucun Compte n'est spécifié, alors ne pas générer d'erreur
                            if (isDebitAccountPayFilled || isDebitAccountRecFilled || isCreditAccountPayFilled || isCreditAccountRecFilled)
                            {
                                #region Generate DayBook entries according to defined scheme entries
                                pAccEar.CurrentDefinedEntry = definedEntry;

                                m_AccQuery.paramIdAccCondition.Value = pAccEar.CurrentDefinedEntry.IDAccCondition;
                                m_AccQuery.paramEarCode.Value = pAccEar.CurrentDefinedEntry.EarCode;
                                m_AccQuery.paramAmountType.Value = pAccEar.CurrentDefinedEntry.AmountType;
                                m_AccQuery.paramEventClass.Value = pAccEar.CurrentDefinedEntry.EventClass;

                                bool isAlreadyAccounted = false;
                                bool pIsStream0Amount = false;

                                // EG 20160404 Migration vs2013
                                AccCondition accCondition = null;
                                QueryParameters verifyQryParameters = null;

                                // Load all flows for this Entry
                                pAccEar.GetCurrentEntryFlows(pEarDetFlowsStreamZero, ref isAlreadyAccounted, ref pIsStream0Amount, pAccEar, pAccModel, pAccInstrEnv, pAccInstrEnvDet);

                                // EG 20160404 Migration vs2013
                                //if (pAccEar.CurrentEntryFlows.Count > 0 &&
                                //    IsCurrentEarDetVerifyEntryCondition(pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet,
                                //    out accCondition, out verifyQryParameters))
                                bool isFlowsFounded = (0 < pAccEar.CurrentEntryFlows.Count) &&
                                    IsCurrentEarDetVerifyEntryCondition(pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet, out accCondition, out verifyQryParameters);
                                if (isFlowsFounded)
                                {
                                    #region Flows founded

                                    // Load variables
                                    LoadAccVariable(pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet);

                                    // Valuate variable in Label and Accounts
                                    //ValuateVariable(pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet);
                                    ValuateAccVariable(false, pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet);

                                    foreach (AccEntryFlow accEntryFlow in pAccEar.CurrentEntryFlows)
                                    {
                                        #region Generate DayBook entries for this flow
                                        // RD 20111230 / Bug: non prise en compte de plusieurs flux de même type
                                        // Ajout d'une boucle pour traiter tous les montants d'un flux
                                        foreach (AccAmount accAmount in accEntryFlow.Amounts)
                                        {
                                            int amountIde = 0;
                                            if (ArrFunc.IsFilled(accAmount.Events))
                                                amountIde = accAmount.Events[0];

                                            if (IsCurrentEventVerifyEntryCondition(pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet,
                                                amountIde, accCondition, verifyQryParameters))
                                            {
                                                m_AccQuery.paramEarType.Value = accEntryFlow.EarType;
                                                m_AccQuery.paramIdEarType.Value = accAmount.IdEarType;

                                                // Valuate variable in Label and Accounts
                                                ValuateAccVariable(true, pAccEar, pAccModel, accDefinedScheme, pAccInstrEnv, pAccInstrEnvDet);

                                                amountIDC = accAmount.IDC;
                                                amountPaid = accAmount.Paid;
                                                amountReceived = accAmount.Received;
                                                amountIdStProcess = accAmount.IDStProcess;

                                                countEntryForAmount = 0;

                                                if (false == ProcessStateTools.IsStatusSuccess(amountIdStProcess))
                                                {
                                                    // Missing countervalue
                                                    AccLog.FireException( null,
                                                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05361",
                                                        new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                                                        pAccEar.CurrentDefinedEntry.lstData.Find(match => match.First == "ACCDEFENTRY").Second,
                                                        accDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                                                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second,
                                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second,
                                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second));
                                                }

                                                #region DEBIT
                                                // Paid Amount
                                                // RD 20120824 [18070] modif temporaire (pour bench) - Test SIGMA
                                                // Ne pas comptabiliser les montants à zéro
                                                //if ((amountPaid > 0) && 
                                                // PL 20120905 Rollback de la modif temporaire, sinon les EAR concernés restent éternellement candidats à comptabilisation. 
                                                // RD/FL 20140925 [] Ne pas comptabiliser les montants Payés à zéro
                                                //if (((amountReceived == 0) || (amountPaid > 0)) &&
                                                if ((amountPaid > 0) &&
                                                    isDebitAccountPayFilled &&
                                                    StrFunc.IsFilled(pAccEar.CurrentDefinedEntry.DebitAccountPayValue))
                                                {
                                                    // RD 20140916 [20328] Add parameters EarType and IdEarType
                                                    pAccEar.AddEntry(true, amountPaid, amountIDC,
                                                        pAccEar.CurrentDefinedEntry.DebitAccountPay,
                                                        pAccEar.CurrentDefinedEntry.DebitAccountPayValue,
                                                        accEntryFlow.EarType, accAmount.IdEarType, accAmount.Events,
                                                        pAccModel, pAccInstrEnv, pAccInstrEnvDet);
                                                    countEntryForAmount++;
                                                }

                                                // Received Amount
                                                // RD 20120824 [18070] modif temporaire (pour bench) - Test SIGMA
                                                // Ne pas comptabiliser les montants à zéro
                                                //if ((amountReceived > 0) &&
                                                // PL 20120905 Rollback de la modif temporaire, sinon les EAR concernés restent éternellement candidats à comptabilisation. 
                                                if (((amountPaid == 0) || (amountReceived > 0)) &&
                                                    isDebitAccountRecFilled &&
                                                    StrFunc.IsFilled(pAccEar.CurrentDefinedEntry.DebitAccountRecValue))
                                                {
                                                    // RD 20140916 [20328] Add parameters EarType and IdEarType
                                                    pAccEar.AddEntry(true, amountReceived, amountIDC,
                                                        pAccEar.CurrentDefinedEntry.DebitAccountRec,
                                                        pAccEar.CurrentDefinedEntry.DebitAccountRecValue,
                                                        accEntryFlow.EarType, accAmount.IdEarType, accAmount.Events,
                                                        pAccModel, pAccInstrEnv, pAccInstrEnvDet);
                                                    countEntryForAmount++;
                                                }
                                                #endregion DEBIT
                                                #region CREDIT
                                                // Paid Amount
                                                // RD 20120824 [18070] modif temporaire (pour bench) - Test SIGMA
                                                // Ne pas comptabiliser les montants à zéro
                                                //if ((amountPaid > 0) &&
                                                // PL 20120905 Rollback de la modif temporaire, sinon les EAR concernés restent éternellement candidats à comptabilisation. 
                                                // RD/FL 20140925 [] Ne pas comptabiliser les montants Payés à zéro
                                                //if (((amountReceived == 0) || (amountPaid > 0)) &&
                                                if ((amountPaid > 0) &&
                                                    isCreditAccountPayFilled &&
                                                    StrFunc.IsFilled(pAccEar.CurrentDefinedEntry.CreditAccountPayValue))
                                                {
                                                    // RD 20140916 [20328] Add parameters EarType and IdEarType
                                                    pAccEar.AddEntry(false, amountPaid, amountIDC,
                                                        pAccEar.CurrentDefinedEntry.CreditAccountPay,
                                                        pAccEar.CurrentDefinedEntry.CreditAccountPayValue,
                                                        accEntryFlow.EarType, accAmount.IdEarType, accAmount.Events,
                                                        pAccModel, pAccInstrEnv, pAccInstrEnvDet);
                                                    countEntryForAmount++;
                                                }
                                                // Received Amount
                                                // RD 20120824 [18070] modif temporaire (pour bench) - Test SIGMA
                                                // Ne pas comptabiliser les montants à zéro
                                                //if (amountReceived > 0 &&
                                                // PL 20120905 Rollback de la modif temporaire, sinon les EAR concernés restent éternellement candidats à comptabilisation. 
                                                if (((amountPaid == 0) || (amountReceived > 0)) &&
                                                    isCreditAccountRecFilled &&
                                                    StrFunc.IsFilled(pAccEar.CurrentDefinedEntry.CreditAccountRecValue))
                                                {
                                                    // RD 20140916 [20328] Add parameters EarType and IdEarType
                                                    pAccEar.AddEntry(false, amountReceived, amountIDC,
                                                        pAccEar.CurrentDefinedEntry.CreditAccountRec,
                                                        pAccEar.CurrentDefinedEntry.CreditAccountRecValue,
                                                        accEntryFlow.EarType, accAmount.IdEarType, accAmount.Events,
                                                        pAccModel, pAccInstrEnv, pAccInstrEnvDet);
                                                    countEntryForAmount++;
                                                }
                                                #endregion CREDIT

                                                #region Set ProcessTunning
                                                if (countEntryForAmount > 0 && ProcessTuningSpecified)
                                                {
                                                    foreach (int ide in accAmount.Events)
                                                    {
                                                        // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                                                        //m_EventsInfo.SetEventStatus(ide,
                                                        //    processTuning.GetProcessTuningOutput(Tuning.TuningOutputTypeEnum.OES),
                                                        //    appInstance.IdA, processLog.GetDate());
                                                        m_EventsInfo.SetEventStatus(ide,
                                                            ProcessTuning.GetProcessTuningOutput(Tuning.TuningOutputTypeEnum.OES),
                                                            Session.IdA, OTCmlHelper.GetDateSys(Cs));
                                                    }
                                                }
                                                #endregion Set ProcessTunning
                                            }
                                        }
                                        //
                                        #endregion
                                    }
                                    #endregion Flows founded
                                }
                                // EG 20160404 Migration vs2013
                                else if (isMsgWhenNoFlowsFounded)
                                {
                                    //-------------------------------------------------------------------
                                    // WARNING! 20080228 PL 
                                    //-------------------------------------------------------------------
                                    // EG 20160404 Migration vs2013
                                    // Ajout du isMsgWhenNoFlowsFounded = false en tête de méthode
                                    //
                                    // - On ne peut à ce jour déterminer si on ne trouve aucun montant du fait:
                                    //     - que le paramétrage est incohérent (eg. INT PRM CLA combinaison inexistante)
                                    //     - que le paramétrage est cohérent mais le flux pas tjs existant (eg. INT NOM STL pas de varition de capital)
                                    //     - que le paramétrage est cohérent mais le flux inexistant (eg. LPP PRM STL à la mise en place)
                                    // - On décide donc de ne plus générer ni warning ni erreur dans ce cas de figure !
                                    //-------------------------------------------------------------------
                                    #region No Flow found
                                    // Check if this amount is already accounted by a previous scheme in consequence by a previous stream
                                    // if yes, then in this case don't generate log message
                                    if (isAlreadyAccounted == false)
                                    {
                                        #region Generate Log Message
                                        // RD 20071126

                                        // Attention, dans le cas d’une variation de notionnel, celle-ci peut être (techniquement) engagée (REC) 
                                        // à une date différente de la date d’engagement de la tombée d’intérêt
                                        // En effet, en cas de variation de notionnel un jour férié, celle-ci peut être engagée (REC) ce jour férié, 
                                        // alors que la date d’engagement de la tombée d’intérêt sera sans doute ajustée avec la BDC de la période de calcul
                                        //
                                        //Ex. : 
                                        // - Effective date: 25/mm/yyyy
                                        // - Termination date: 25/mm/yyyy
                                        // - Calculation Period Dates BDC : Following
                                        // - Payment Period Dates BDC : Following
                                        // - Notional step: Dimanche 25
                                        // 
                                        // - INT/NOM/STL:  Lundi 26
                                        // - INT/INT/REC:  Lundi 26
                                        // - INT/INT/STL:  Lundi 26
                                        if (pAccEar.CurrentDefinedEntry.EarCode == (EventCodeFunc.DailyClosing + "-1"))
                                        {
                                            //S'il s'agit d'un montant issu d'un EARCODE="CLO-1", celui-ci n'est pas forcément présent
                                            //donc --> Ni Warning, Ni Error
                                        }
                                        else
                                        {

                                            logMessage = "No amount for this entry or it's an incoherent entry";
                                            // Si on recherche un montant:  
                                            //	earcode		= INT
                                            //	amounttype	= « tous » (ex. INT, NETINT, NOM, …)
                                            //	eventclass	= REC

                                            // et Si on est sur un schéma:
                                            //	earcode		= INT 
                                            //	eventclass	= REC
                                            if (
                                                (
                                                pAccEar.CurrentDefinedEntry.EarCode == EventCodeFunc.Intermediary &&
                                                pAccEar.CurrentDefinedEntry.EventClass == EventClassFunc.Recognition &&
                                                pAccEar.CurrentDefinedScheme.EarCode == EventCodeFunc.Intermediary &&
                                                pAccEar.CurrentDefinedScheme.EventClass == EventClassFunc.Recognition
                                                )
                                                ||
                                                (!m_IsAccErrorNoAmount)
                                                )
                                            {
                                                // Alors on génère un warning et le traitement continue	
                                                codeReturn = Cst.ErrLevel.DATANOTFOUND;

                                                
                                                ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                                                
                                                Logger.Log(new LoggerData(LogLevelEnum.Warning, logMessage));
                                            }
                                            else
                                            {
                                                // Sinon on génère une erreur et le traitement s'arrête
                                                codeReturn = Cst.ErrLevel.INCORRECTPARAMETER;
                                                AccLog.FireException(null,
                                                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, logMessage,
                                                    new ProcessState(ProcessStateTools.StatusErrorEnum, codeReturn)));
                                            }
                                        }
                                        #endregion Generate Log Message
                                    }
                                    #endregion No Flow found
                                }
                                #endregion Generate DayBook entries according to defined scheme entries
                            }
                        }
                        #endregion Traitement
                        //
                        #region Check if scheme daybook entries are balanced
                        if (pAccEar.CurrentDefinedScheme.EntriesAmountNet != 0 ||
                            pAccEar.CurrentDefinedScheme.ReversalEntriesAmountNet != 0)
                        {
                            codeReturn = Cst.ErrLevel.ABORTED;
                            //		
                            switch (pAccEar.CurrentDefinedScheme.VRUnbalanced.ToUpper())
                            {
                                case "WARNING":
                                    processState = new ProcessState(ProcessStateTools.StatusWarningEnum, codeReturn);

                                    if (pAccEar.CurrentDefinedScheme.EntriesAmountNet != 0)
                                    {
                                        
                                        // Entries Unbalanced
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5353), 0,
                                            new LogParam(pAccEar.CurrentDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second),
                                            new LogParam(pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second),
                                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second),
                                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second)));
                                    }
                                    //
                                    if (pAccEar.CurrentDefinedScheme.ReversalEntriesAmountNet != 0)
                                    {
                                        // Reversal entries Unbalanced
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5354), 2,
                                            new LogParam(pAccEar.CurrentDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second),
                                            new LogParam(pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second),
                                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second),
                                            new LogParam(pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second)));
                                    }
                                    break;
                                case "ERROR":
                                    // Entries are unbalanced
                                    AccLog.FireException(null,
                                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05352",
                                        new ProcessState(ProcessStateTools.StatusErrorEnum, codeReturn),
                                        accDefinedScheme.lstData.Find(match => match.First == "ACCDEFSCHEME").Second,
                                        pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second,
                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second,
                                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second));
                                    break;
                                default:
                                    break;
                            }
                        }
                        #endregion Check if scheme daybook entries are balanced
                    }
                }
                //
#if DEBUG
                diagnosticDebug.End(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05369",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, codeReturn),
                    m_AccQuery.paramEarCode.Value.ToString() + " / " + m_AccQuery.paramEventClass.Value.ToString(),
                    pAccInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                    pAccInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "SOURCE").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                    pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return codeReturn;
        }
        #endregion GenerateEntriesForScheme

        #region LoadInstrEnvDetScheme
        /// <summary>
        /// Retourne la liste des Schémas comptables d'un Instrument Environment Detail
        /// </summary>
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public Cst.ErrLevel LoadInstrEnvDetScheme(AccEar pAccEar, AccInstrEnv pInstrEnv, AccInstrEnvDet pInstrEnvDet)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            try
            {
                #region Declaration
                int accSchemeID;
                string accSchemeIdentifier;
                string accCashSecurities;
                string accVRUnbalanced;
                //
                int accEntryID;
                int accConditionID;
                string accEarCode;
                string accEventClass;
                string accExchangeType;
                string accQuoteExchangeType;
                string accAmountType;
                string accDebitAccountPay;
                string accCreditAccountPay;
                string accDebitAccountRec;
                string accCreditAccountRec;
                int accPeriodMultipOffset;
                string accPeriodOffset;
                string accDayTypeOffset;
                bool accIsReversal;
                string accDayTypeReversal;
                string accJournalCode;
                string accConsoCode;
                //	
                AccDefinedScheme accScheme;
                AccDefinedEntry accEntry;
                #endregion
                //
                string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT_DISTINCT;
                sqlQuery += "sch.IDACCSCHEME, sch.CASHSECURITIES, sch.VRUNBALANCED, sch.IDENTIFIER," + Cst.CrLf;
                sqlQuery += "ent.IDACCENTRY, ent.IDACCCONDITION, ent.EARCODE," + Cst.CrLf;
                sqlQuery += "ent.EVENTCLASS, ent.EXCHANGETYPE, ent.QUOTEEXCHANGETYPE," + Cst.CrLf;
                sqlQuery += "ent.AMOUNTTYPE, ent.D_ACCOUNT_PAY, ent.C_ACCOUNT_PAY," + Cst.CrLf;
                sqlQuery += "ent.D_ACCOUNT_REC, ent.C_ACCOUNT_REC, ent.PERIODMLTPOFFSET," + Cst.CrLf;
                sqlQuery += "ent.PERIODOFFSET, ent.DAYTYPEOFFSET, ent.ISREVERSAL," + Cst.CrLf;
                sqlQuery += "ent.DAYTYPEREVERSAL,ent.JOURNALCODE, ent.CONSOCODE," + Cst.CrLf;
                sqlQuery += "l.LABEL, l.REVERSALLABEL, l.REVERSALPOS, l.DEACTIVLABEL, l.DEACTIVPOS," + Cst.CrLf;
                sqlQuery += "l.LABEL2, l.REVERSALLABEL2, l.REVERSALPOS2, l.DEACTIVLABEL2, l.DEACTIVPOS2," + Cst.CrLf;
                sqlQuery += "l.BUYER_SELLER," + Cst.CrLf;
                // Le but est d'avoir en première position la ligne ACCLABEL avec un BUYER_SELLER non NULL
                sqlQuery += SQLCst.CASE + SQLCst.CASE_WHEN + "l.BUYER_SELLER" + SQLCst.IS_NULL + SQLCst.CASE_THEN + "1" + SQLCst.CASE_ELSE + "0" + SQLCst.CASE_END + SQLCst.AS + "NUMORDER" + Cst.CrLf;
                //
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCSCHEME.ToString() + " sch" + Cst.CrLf;
                //
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCENTRY.ToString() + " ent ";
                sqlQuery += SQLCst.ON + "(ent.IDACCSCHEME = sch.IDACCSCHEME)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "ent") + Cst.CrLf;
                //
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCINSTRENVDET + " iedet ";
                sqlQuery += SQLCst.ON + "(iedet.IDACCINSTRENVDET = sch.IDACCINSTRENVDET)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "iedet") + Cst.CrLf;
                //
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCINSTRENV + " ie ";
                sqlQuery += SQLCst.ON + "(ie.IDACCINSTRENV = iedet.IDACCINSTRENV)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "ie") + Cst.CrLf;
                //
                //PL 20111103 WARNING: Aucun refactoring pour ne plus utiliser VW_ACCKEYDATA (TRIM 17409,17617) car:
                //                     - le select est ici un select "distinct"
                //                     - on exploite ici réellement une colonne de cette vue: KeyTradeSide
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.VW_ACCKEYDATA.ToString() + " kd ";
                sqlQuery += SQLCst.ON + "(kd.IDEAR = @IDEAR) and (kd.INSTRUMENTNO = @INSTRUMENTNO)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "((kd.STREAMNO = @STREAMNO) or (kd.STREAMNO = 0))" + Cst.CrLf;
                //
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCLABEL.ToString() + " l ";
                sqlQuery += SQLCst.ON + "(l.IDACCINSTRENV = ie.IDACCINSTRENV) ";
                sqlQuery += SQLCst.AND + "(l.CASHSECURITIES = sch.CASHSECURITIES) ";
                sqlQuery += SQLCst.AND + "(l.EARCODE = ent.EARCODE) ";
                sqlQuery += SQLCst.AND + "(l.EVENTCLASS = ent.EVENTCLASS)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "(l.AMOUNTTYPE = ent.AMOUNTTYPE) ";
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "l") + Cst.CrLf;
                sqlQuery += SQLCst.AND + "((l.BUYER_SELLER = kd.KeyTradeSide) or (l.BUYER_SELLER" + SQLCst.IS_NULL + "))" + Cst.CrLf;
                //
                sqlQuery += SQLCst.WHERE + "(sch.IDACCINSTRENVDET = @IDACCINSTRENVDET)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "sch") + Cst.CrLf;
                sqlQuery += SQLCst.AND + "(sch.EARCODE = @EARCODE)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "(sch.EVENTCLASS = @EVENTCLASS)" + Cst.CrLf;
                //
                sqlQuery += SQLCst.ORDERBY + "NUMORDER";

                DataParameters parameters = new DataParameters();
                parameters.Add(m_AccQuery.paramIdEAR);
                parameters.Add(m_AccQuery.paramInstrumentNo);
                parameters.Add(m_AccQuery.paramStreamNo);
                parameters.Add(m_AccQuery.paramIdAccInstrEnvDet);
                parameters.Add(m_AccQuery.paramEarCode);
                parameters.Add(m_AccQuery.paramEventClass);
                QueryParameters qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                DataSet dsScheme = DataHelper.ExecuteDataset(CSTools.SetCacheOn(Cs), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
                dsScheme.Tables[0].TableName = Cst.OTCml_TBL.ACCSCHEME.ToString();
                DataTable dtScheme = dsScheme.Tables[0];
                DataRow[] drScheme = dtScheme.Select();
                //
                if (drScheme.Length == 0)
                {
                    if (false == pInstrEnvDet.Schemes.Exists(
                        scheme => scheme.EarCode == m_AccQuery.paramEarCode.Value.ToString()
                            && scheme.EventClass == m_AccQuery.paramEventClass.Value.ToString()))
                    {
                        pInstrEnvDet.Schemes.Add(new AccDefinedScheme(m_AccQuery.paramEarCode.Value.ToString(), m_AccQuery.paramEventClass.Value.ToString()));
                    }
                }
                else
                {
                    foreach (DataRow rowScheme in drScheme)
                    {
                        accSchemeID = Convert.ToInt32(rowScheme["IDACCSCHEME"]);
                        accSchemeIdentifier = rowScheme["IDENTIFIER"].ToString();
                        accCashSecurities = rowScheme["CASHSECURITIES"].ToString();
                        accVRUnbalanced = rowScheme["VRUNBALANCED"].ToString();

                        accScheme = pInstrEnvDet.Schemes.Find(scheme => scheme.IDAccScheme == accSchemeID);
                        if (null == accScheme)
                        {
                            accScheme = new AccDefinedScheme(accSchemeID, accSchemeIdentifier, accCashSecurities,
                                m_AccQuery.paramEarCode.Value.ToString(), m_AccQuery.paramEventClass.Value.ToString(),
                                accVRUnbalanced);

                            pInstrEnvDet.Schemes.Add(accScheme);
                        }

                        accEntryID = (Convert.IsDBNull(rowScheme["IDACCENTRY"]) ? 0 : Convert.ToInt32(rowScheme["IDACCENTRY"]));
                        accConditionID = (Convert.IsDBNull(rowScheme["IDACCCONDITION"]) ? 0 : Convert.ToInt32(rowScheme["IDACCCONDITION"]));
                        accEarCode = (Convert.IsDBNull(rowScheme["EARCODE"]) ? null : rowScheme["EARCODE"].ToString());
                        accEventClass = (Convert.IsDBNull(rowScheme["EVENTCLASS"]) ? null : rowScheme["EVENTCLASS"].ToString());
                        accExchangeType = (Convert.IsDBNull(rowScheme["EXCHANGETYPE"]) ? null : rowScheme["EXCHANGETYPE"].ToString());
                        accQuoteExchangeType = rowScheme["QUOTEEXCHANGETYPE"].ToString();
                        accAmountType = (Convert.IsDBNull(rowScheme["AMOUNTTYPE"]) ? null : rowScheme["AMOUNTTYPE"].ToString());
                        accDebitAccountPay = (Convert.IsDBNull(rowScheme["D_ACCOUNT_PAY"]) ? null : rowScheme["D_ACCOUNT_PAY"].ToString());
                        accCreditAccountPay = (Convert.IsDBNull(rowScheme["C_ACCOUNT_PAY"]) ? null : rowScheme["C_ACCOUNT_PAY"].ToString());
                        accDebitAccountRec = (Convert.IsDBNull(rowScheme["D_ACCOUNT_REC"]) ? null : rowScheme["D_ACCOUNT_REC"].ToString());
                        accCreditAccountRec = (Convert.IsDBNull(rowScheme["C_ACCOUNT_REC"]) ? null : rowScheme["C_ACCOUNT_REC"].ToString());
                        accPeriodMultipOffset = (Convert.IsDBNull(rowScheme["PERIODMLTPOFFSET"]) ? 0 : Convert.ToInt32(rowScheme["PERIODMLTPOFFSET"]));
                        accPeriodOffset = (Convert.IsDBNull(rowScheme["PERIODOFFSET"]) ? null : rowScheme["PERIODOFFSET"].ToString());
                        accDayTypeOffset = (Convert.IsDBNull(rowScheme["DAYTYPEOFFSET"]) ? null : rowScheme["DAYTYPEOFFSET"].ToString());
                        accIsReversal = (!Convert.IsDBNull(rowScheme["ISREVERSAL"]) && BoolFunc.IsTrue(rowScheme["ISREVERSAL"].ToString()));
                        accDayTypeReversal = (Convert.IsDBNull(rowScheme["DAYTYPEREVERSAL"]) ? null : rowScheme["DAYTYPEREVERSAL"].ToString());
                        accJournalCode = (Convert.IsDBNull(rowScheme["JOURNALCODE"]) ? null : rowScheme["JOURNALCODE"].ToString());
                        accConsoCode = (Convert.IsDBNull(rowScheme["CONSOCODE"]) ? null : rowScheme["CONSOCODE"].ToString());

                        if (accEntryID != 0)
                        {
                            accEntry = accScheme.DefinedEntries.Find(entry => entry.IDAccEntry == accEntryID);
                            if (null == accEntry)
                            {
                                accEntry = new AccDefinedEntry(accEntryID, accConditionID, accEarCode, accEventClass, accExchangeType, accQuoteExchangeType,
                                    accAmountType, accDebitAccountPay, accCreditAccountPay, accDebitAccountRec, accCreditAccountRec, accPeriodMultipOffset,
                                    accPeriodOffset, accDayTypeOffset, accIsReversal, accDayTypeReversal, accJournalCode, accConsoCode);

                                accScheme.DefinedEntries.Add(accEntry);
                            }

                            if (accEntry.LabelMain == null)
                            {
                                accEntry.LabelMain = new AccLabel(rowScheme["LABEL"], rowScheme["REVERSALLABEL"], rowScheme["REVERSALPOS"],
                                    rowScheme["DEACTIVLABEL"], rowScheme["DEACTIVPOS"]);
                                accEntry.LabelSecondary = new AccLabel(rowScheme["LABEL2"], rowScheme["REVERSALLABEL2"], rowScheme["REVERSALPOS2"],
                                    rowScheme["DEACTIVLABEL2"], rowScheme["DEACTIVPOS2"]);
                            }
                        }
                    }
                }
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05375",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                        m_AccQuery.paramEarCode.Value.ToString() + " / " + m_AccQuery.paramEventClass.Value.ToString(),
                        pInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "SOURCE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return codeReturn;
        }
        #endregion LoadInstrEnvDetScheme
        #region IsEarDetMatchKeyValue
        /// <summary>
        /// True si l'EAR Det match avec les valeurs des critères de décomposition
        /// </summary>
        // RD 20150309 [20856]
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public bool IsEarDetMatchKeyValue(AccEar pAccEar, AccInstrEnv pInstrEnv, AccInstrEnvDet pInstrEnvDet)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            bool isMatch = true;
            DataParameters parameters = new DataParameters();
            DataParameter parameter;
            QueryParameters qryParam = null;

            try
            {

                if (pInstrEnvDet.Keys.Count > 0)
                {
                    string keyTable = string.Empty;
                    string sqlWhere = string.Empty;
                    string sqlQuery = string.Empty;
                    string keyTableOld = string.Empty;
                    string keyParamName = string.Empty;
                    // RD 20150309 [20856] Clés de décomposition avec des vues différentes, 
                    int keyNumber = 3;

                    sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT + Cst.CrLf;
                    sqlQuery += SQLCst.CASE + Cst.CrLf;
                    sqlQuery += SQLCst.CASE_WHEN;
                    sqlQuery += "{sqlWhere}";
                    sqlQuery += SQLCst.CASE_THEN + "1" + Cst.CrLf;
                    sqlQuery += SQLCst.CASE_ELSE + "0" + Cst.CrLf;
                    sqlQuery += SQLCst.CASE_END + Cst.CrLf;
                    sqlQuery += SQLCst.FROM_DBO + "{keyTableData}" + Cst.CrLf;
                    sqlQuery += SQLCst.WHERE + "{keyTableData}.IDEAR = @IDEAR" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + "{keyTableData}.INSTRUMENTNO = @INSTRUMENTNO" + Cst.CrLf;
                    sqlQuery += SQLCst.AND + "{keyTableData}.STREAMNO = @STREAMNO" + Cst.CrLf;

                    // RD 20150309 [20856] Clés de décomposition avec des vues différentes, 
                    parameters.Add(m_AccQuery.paramIdEAR);
                    parameters.Add(m_AccQuery.paramInstrumentNo);
                    parameters.Add(m_AccQuery.paramStreamNo);

                    #region Build sqlWhere
                    foreach (AccKey key in pInstrEnvDet.Keys)
                    {
                        keyTable = key.Table;

                        if (keyTableOld != keyTable)
                        {
                            if (StrFunc.IsFilled(keyTableOld))
                            {
                                sqlWhere = sqlWhere.TrimEnd(SQLCst.AND.ToString().ToCharArray());
                                // RD 20150309 [20856] Clés de décomposition avec des vues différentes, 
                                qryParam = new QueryParameters(Cs, sqlQuery.Replace("{sqlWhere}", sqlWhere.TrimEnd()).Replace("{keyTableData}", keyTableOld), parameters);
                                using (IDataReader keyData = DataHelper.ExecuteReader(Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter()))
                                {
                                    if (keyData.Read())
                                    {
                                        int result = (Convert.IsDBNull(keyData.GetValue(0)) ? 0 : Convert.ToInt32(keyData.GetValue(0)));
                                        if (result != 1)
                                            isMatch = false;
                                    }
                                    else
                                        isMatch = false;
                                }

                                if (isMatch == false)
                                    break;

                                // RD 20150309 [20856] Clés de décomposition avec des vues différentes, 
                                sqlWhere = string.Empty;
                                parameters.Clear();
                                parameters.Add(m_AccQuery.paramIdEAR);
                                parameters.Add(m_AccQuery.paramInstrumentNo);
                                parameters.Add(m_AccQuery.paramStreamNo);
                            }

                            keyTableOld = keyTable;
                        }

                        // RD 20150309 [20856] Clés de décomposition avec des vues différentes, 
                        keyParamName = key.Column.ToUpper() + keyNumber.ToString();

                        parameter = new DataParameter(Cs, keyParamName, DbType.AnsiString)
                        {
                            Value = (Convert.IsDBNull(key.DataValue) || key.DataValue == null ? Convert.DBNull : DataHelper.GetDBData(key.DataValue))
                        };
                        parameters.Add(parameter);

                        sqlWhere += keyTable + "." + key.Column + " = @" + keyParamName + " " + SQLCst.AND;

                        keyNumber++;
                    }
                    #endregion
                    if (isMatch && StrFunc.IsFilled(sqlWhere))
                    {
                        sqlWhere = sqlWhere.TrimEnd(SQLCst.AND.ToString().ToCharArray());
                        // RD 20150309 [20856] Clés de décomposition avec des vues différentes, 
                        qryParam = new QueryParameters(Cs, sqlQuery.Replace("{sqlWhere}", sqlWhere.TrimEnd()).Replace("{keyTableData}", keyTable), parameters);
                        #region Check if Ear detail match
                        using (IDataReader keyData = DataHelper.ExecuteReader(Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter()))
                        {
                            if (keyData.Read())
                            {
                                int result = (Convert.IsDBNull(keyData.GetValue(0)) ? 0 : Convert.ToInt32(keyData.GetValue(0)));
                                if (result != 1)
                                    isMatch = false;
                            }
                            else
                                isMatch = false;
                        }
                        #endregion
                    }
                }
#if DEBUG
                diagnosticDebug.End(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05374",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pInstrEnv.lstData.Find(match => match.First == "ACCINSTRENV").Second + " / " +
                        pInstrEnvDet.lstData.Find(match => match.First == "ACCINSTRENVDET").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + " (" +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " / " +
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second + ")",
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "SOURCE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return isMatch;
        }
        #endregion IsEarDetMatchKeyValue
        #region LoadInstrEnv
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public Cst.ErrLevel LoadInstrEnv(AccEar pAccEar, AccModel pAccModel, ref AccModelInstrument pAccModelInstrument)
        {
#if DEBUG
            diagnosticDebug.Start(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            string sqlQuery;

            try
            {
                sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                sqlQuery += "aie.IDACCINSTRENV, aie.IDENTIFIER, aie.IDI," + Cst.CrLf;
                sqlQuery += "aied.IDACCINSTRENVDET, aied.IDENTIFIER " + SQLCst.AS + "IDENTIFIERDET, aied.ISTOIGNORE," + Cst.CrLf;
                sqlQuery += "ake.TABLEDATA, ake.COLUMNDATA, ake.DATATYPE, akv.VALUE" + Cst.CrLf;

                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCINSTRENV + " aie" + Cst.CrLf;
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCINSTRENVDET.ToString() + " aied ";
                sqlQuery += SQLCst.ON + "(aied.IDACCINSTRENV = aie.IDACCINSTRENV)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "aied") + Cst.CrLf;
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCKEY.ToString() + " ak ";
                sqlQuery += SQLCst.ON + "(ak.IDACCINSTRENV = aie.IDACCINSTRENV)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "ak") + Cst.CrLf;
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCKEYVALUE.ToString() + " akv ";
                sqlQuery += SQLCst.ON + "(akv.IDACCINSTRENVDET = aied.IDACCINSTRENVDET)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "(akv.IDACCKEY = ak.IDACCKEY)";
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "akv") + Cst.CrLf;
                sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.ACCKEYENUM.ToString() + " ake ";
                sqlQuery += SQLCst.ON + "(ake.IDACCKEYENUM = ak.IDACCKEYENUM)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "ake") + Cst.CrLf;

                sqlQuery += SQLCst.WHERE + "(aie.IDACCMODEL = @IDACCMODEL)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "aie") + Cst.CrLf;
                //FI 20100325 appel à SQLInstrCriteria à la place de InstrTools.GetSQLCriteriaInstr2
                //sqlQuery += InstrTools.GetSQLCriteriaInstr(Cs, pAccEar.CurrentEarDet.IDI, "aie", RoleGInstr.ACCOUNTING);
                SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(CSTools.SetCacheOn(Cs), null, pAccEar.CurrentEarDet.IDI, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
                sqlQuery += SQLCst.AND + sqlInstrCriteria.GetSQLRestriction("aie", RoleGInstr.ACCOUNTING);

                sqlQuery += SQLCst.ORDERBY + "aied.ISTOIGNORE " + SQLCst.DESC + ", aie.IDACCINSTRENV, aied.IDACCINSTRENVDET, ake.TABLEDATA";

                DataParameters parameters = new DataParameters();
                parameters.Add(m_AccQuery.paramIdAccModel);
                QueryParameters qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                DataSet dsInstrEnv = DataHelper.ExecuteDataset(CSTools.SetCacheOn(Cs), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
                DataTable dtInstrEnv = dsInstrEnv.Tables[0];
                DataRow[] drInstrEnv = dtInstrEnv.Select();
                //
                if (drInstrEnv.Length == 0)
                {
                    // No Model InstrEnv matchs with this EAR detail
                    codeReturn = Cst.ErrLevel.INCORRECTPARAMETER;
                    AccLog.FireException(null,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05356",
                            new ProcessState(ProcessStateTools.StatusErrorEnum, codeReturn),
                            pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second,
                            pAccEar.lstData.Find(match => match.First == "EAR").Second,
                            pAccEar.lstData.Find(match => match.First == "BOOK").Second,
                            m_LstMasterData.Find(match => match.First == "TRADE").Second,
                            m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second));
                }

                int idInstrEnv;
                string identifierInstrEnv;
                int idInstrEnvDet;
                string identifierInstrEnvDet;
                bool isInstrEnvDetToIgnore;
                string accKeyTableData;
                string accKeyColumnData;
                string accKeyDataType;
                string accKeyValue;

                AccInstrEnv accInstrEnv;
                AccInstrEnvDet accInstrEnvDet;
                AccKey accKey;

                foreach (DataRow rowInstrEnv in drInstrEnv)
                {
                    idInstrEnv = (Convert.IsDBNull(rowInstrEnv["IDACCINSTRENV"]) ? 0 : Convert.ToInt32(rowInstrEnv["IDACCINSTRENV"]));
                    identifierInstrEnv = (Convert.IsDBNull(rowInstrEnv["IDENTIFIER"]) ? string.Empty : rowInstrEnv["IDENTIFIER"].ToString());

                    idInstrEnvDet = (Convert.IsDBNull(rowInstrEnv["IDACCINSTRENVDET"]) ? 0 : Convert.ToInt32(rowInstrEnv["IDACCINSTRENVDET"]));
                    identifierInstrEnvDet = (Convert.IsDBNull(rowInstrEnv["IDENTIFIERDET"]) ? string.Empty : rowInstrEnv["IDENTIFIERDET"].ToString());
                    isInstrEnvDetToIgnore = !Convert.IsDBNull(rowInstrEnv["ISTOIGNORE"]) && Convert.ToBoolean(rowInstrEnv["ISTOIGNORE"]);

                    accKeyTableData = (Convert.IsDBNull(rowInstrEnv["TABLEDATA"]) ? string.Empty : rowInstrEnv["TABLEDATA"].ToString());
                    accKeyColumnData = (Convert.IsDBNull(rowInstrEnv["COLUMNDATA"]) ? string.Empty : rowInstrEnv["COLUMNDATA"].ToString());
                    accKeyDataType = (Convert.IsDBNull(rowInstrEnv["DATATYPE"]) ? string.Empty : rowInstrEnv["DATATYPE"].ToString());
                    accKeyValue = (Convert.IsDBNull(rowInstrEnv["VALUE"]) ? string.Empty : rowInstrEnv["VALUE"].ToString());

                    accInstrEnv = pAccModelInstrument.InstrEnvs.Find(instrEnv => instrEnv.IDAccInstrEnv == idInstrEnv);
                    if (null == accInstrEnv)
                    {
                        accInstrEnv = new AccInstrEnv(idInstrEnv, identifierInstrEnv);
                        pAccModelInstrument.InstrEnvs.Add(accInstrEnv);
                    }

                    if (idInstrEnvDet > 0)
                    {
                        accInstrEnvDet = accInstrEnv.InstrEnvDets.Find(instrEnvDet => instrEnvDet.IDInstrEnvDet == idInstrEnvDet);
                        if (null == accInstrEnvDet)
                        {
                            accInstrEnvDet = new AccInstrEnvDet(idInstrEnvDet, identifierInstrEnvDet, isInstrEnvDetToIgnore);
                            accInstrEnv.InstrEnvDets.Add(accInstrEnvDet);
                        }

                        if (StrFunc.IsFilled(accKeyValue))
                        {
                            accKey = new AccKey(accKeyTableData, accKeyColumnData, accKeyDataType, accKeyValue);
                            if (false == accInstrEnvDet.Keys.Exists(key => key.CompareTo(accKey) == 0))
                                accInstrEnvDet.Keys.Add(accKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                     new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05373",
                     new ProcessState(ProcessStateTools.StatusErrorEnum),
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " (" +
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + ")",
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "STREAMNO").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "EAR").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "SOURCE").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "BOOK").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "TRADE").Second,
                         pAccEar.CurrentEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }
            //			
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return codeReturn;
        }
        #endregion
        #region LoadEarDetSchemes
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public bool LoadEarDetSchemes(AccEarDet pAccEarDet)
        {
            bool ret = false;

            try
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
                string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                sqlQuery += "EARCODE, EVENTCLASS, 0 as ISSCHEMEDEFINED" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EARACCSCHEME + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "(IDEAR = @IDEAR)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "(INSTRUMENTNO = @INSTRUMENTNO)" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "(STREAMNO = @STREAMNO)" + Cst.CrLf;
                sqlQuery += SQLCst.ORDERBY + "EARCODE, EVENTCLASS" + Cst.CrLf;

                DataParameters parameters = new DataParameters();
                parameters.Add(m_AccQuery.paramIdEAR);
                parameters.Add(m_AccQuery.paramInstrumentNo);
                parameters.Add(m_AccQuery.paramStreamNo);

                QueryParameters qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                DataSet dsEarDay = DataHelper.ExecuteDataset(Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
                DataTable dtEarDay = dsEarDay.Tables[0];
                pAccEarDet.EarSchemes = dtEarDay.Select();

#if DEBUG
                diagnosticDebug.End(System.Reflection.MethodInfo.GetCurrentMethod().Name);
#endif
                ret = (pAccEarDet.EarSchemes.Length > 0);
            }
            catch (Exception ex)
            {
                AccLog.FireException(ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05384",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pAccEarDet.lstData.Find(match => match.First == "INSTRNO").Second + " (" +
                        pAccEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second + ")",
                        pAccEarDet.lstData.Find(match => match.First == "STREAMNO").Second,
                        pAccEarDet.lstData.Find(match => match.First == "EAR").Second,
                        pAccEarDet.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEarDet.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        pAccEarDet.lstData.Find(match => match.First == "SOURCE").Second,
                        pAccEarDet.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEarDet.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEarDet.lstData.Find(match => match.First == "PROCESSDATE").Second));
            }

            return ret;
        }
        #endregion LoadEarDetSchemes
        #region ProcessEarDetailByModel
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void ProcessEarByModel(AccEar pAccEar, DataRow pRowEarDet, AccModel pAccModel,
            ref List<AccEarDet> pProcessedAccEarDetForModel)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " ........................");
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            AccModelInstrument accInstrument = null;

            pAccEar.CurrentEarDet = new AccEarDet(pRowEarDet, pAccModel.IDAccModel, pAccEar.lstData, pAccModel.lstData);

            m_AccQuery.paramInstrumentNo.Value = pAccEar.CurrentEarDet.InstrumentNO;
            m_AccQuery.paramStreamNo.Value = pAccEar.CurrentEarDet.StreamNO;
            m_AccQuery.paramIdAccModel.Value = pAccEar.CurrentEarDet.IDAccModel;

            #region Process all EAR details for each AccModel
            if (LoadEarDetSchemes(pAccEar.CurrentEarDet))
            {
                accInstrument = pAccModel.AccInstruments.Find(instrument => instrument.IDI == pAccEar.CurrentEarDet.IDI);
                if (null == accInstrument)
                {
                    accInstrument = new AccModelInstrument(pAccEar.CurrentEarDet.IDI);
                    pAccModel.AccInstruments.Add(accInstrument);
                    codeReturn = LoadInstrEnv(pAccEar, pAccModel, ref accInstrument);
                }

                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                {
                    foreach (AccInstrEnv instrEnv in accInstrument.InstrEnvs)
                    {
                        #region Process All Instrumental environments
                        if (pAccModel.IsEarIgnoredByModel == false)
                        {
                            if (instrEnv.InstrEnvDets.Count > 0)
                            {
                                pAccEar.CurrentEarDet.IsInstrEnvDetMatch = true;

                                foreach (AccInstrEnvDet instrEnvDet in instrEnv.InstrEnvDets)
                                {
                                    #region All Environment details
                                    if (pAccModel.IsEarIgnoredByModel == false)
                                    {
                                        m_AccQuery.paramIdAccInstrEnvDet.Value = instrEnvDet.IDInstrEnvDet;

                                        if (IsEarDetMatchKeyValue(pAccEar, instrEnv, instrEnvDet))
                                        {
                                            pAccEar.CurrentEarDet.IsKeyMatch = true;

                                            if (instrEnvDet.IsToIgnore)
                                            {
                                                #region Entries are to ignore for this EAR
                                                if (DateTime.Compare(pAccEar.DateEvent, pAccEar.DateAccounting) == 0)
                                                {
                                                    codeReturn = Cst.ErrLevel.DATAIGNORE;

                                                    
                                                    
                                                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5341), 0,
                                                        new LogParam(pAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                                                        new LogParam(pAccEar.lstData.Find(match => match.First == "EAR").Second),
                                                        new LogParam(pAccEar.lstData.Find(match => match.First == "BOOK").Second)));
                                                }

                                                pAccModel.IsEarIgnoredByModel = true;
                                                #endregion Entries are to ignore for this EAR
                                            }
                                            else
                                            {
                                                #region Ear is to account
                                                List<AccEarDetFlows> earDetFlowsStreamZero = pAccEar.EarDetFlows.FindAll(flow => flow.InstrumentNO == pAccEar.CurrentEarDet.InstrumentNO && flow.StreamNO == 0);
                                                List<AccDefinedScheme> definedSchemes;

                                                foreach (DataRow rowEarScheme in pAccEar.CurrentEarDet.EarSchemes)
                                                {
                                                    codeReturn = Cst.ErrLevel.SUCCESS;

                                                    m_AccQuery.paramEarCode.Value = rowEarScheme["EARCODE"].ToString();
                                                    m_AccQuery.paramEventClass.Value = rowEarScheme["EVENTCLASS"].ToString();

                                                    if (false == instrEnvDet.Schemes.Exists(
                                                        scheme => scheme.EarCode == m_AccQuery.paramEarCode.Value.ToString()
                                                            && scheme.EventClass == m_AccQuery.paramEventClass.Value.ToString()))
                                                    {
                                                        codeReturn = LoadInstrEnvDetScheme(pAccEar, instrEnv, instrEnvDet);
                                                    }

                                                    definedSchemes = instrEnvDet.Schemes.FindAll(
                                                        scheme => (scheme.EarCode == m_AccQuery.paramEarCode.Value.ToString())
                                                            && (scheme.EventClass == m_AccQuery.paramEventClass.Value.ToString())
                                                            && (scheme.IDAccScheme > 0));

                                                    if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                                    {
                                                        if (ArrFunc.IsFilled(definedSchemes))
                                                        {
                                                            rowEarScheme["ISSCHEMEDEFINED"] = 1;

                                                            bool isToProcess = true;

                                                            if (pAccEar.CurrentEarDet.StreamNO == 0)
                                                            {
                                                                foreach (AccEarDetFlows earDetStreamZero in earDetFlowsStreamZero)
                                                                {
                                                                    if ((true == earDetStreamZero.IsEarSchemeProcessed(m_AccQuery.paramEarCode.Value.ToString(), m_AccQuery.paramEventClass.Value.ToString())))
                                                                    {
                                                                        isToProcess = false;
                                                                        break;
                                                                    }
                                                                }

                                                                isToProcess = true;
                                                            }

                                                            if (isToProcess)
                                                            {
                                                                codeReturn = GenerateEntriesForScheme(pAccEar, pAccModel, earDetFlowsStreamZero, definedSchemes, instrEnv, instrEnvDet);

                                                                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                                                {
                                                                    if (pAccEar.IsSream0Exist)
                                                                    {
                                                                        foreach (AccEarDetFlows earDetStreamZero in earDetFlowsStreamZero)
                                                                            earDetStreamZero.SetEarSchemeProcessed(m_AccQuery.paramEarCode.Value.ToString(), m_AccQuery.paramEventClass.Value.ToString());
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                    else
                                        break;
                                    #endregion
                                }
                            }
                        }
                        else
                            break;
                        #endregion
                    }
                }

                pProcessedAccEarDetForModel.Add(pAccEar.CurrentEarDet);

            }
            #endregion
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "..........................................");
#endif
        }
        #endregion ProcessEarDetailByModel
        #region LoadEarAccountingModel
        /// <summary>
        /// Retourne la liste des Models ( ACCMODEL) qui match avec le Book de l'EAR
        /// </summary>
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        private Cst.ErrLevel LoadBookAccountingModel(AccEar pAccEar, ref AccBook pBook)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            try
            {
                string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
                sqlQuery += "am.IDACCMODEL, am.IDENTIFIER" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCMODEL + " am" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACCENTITYMODEL.ToString() + " aem";
                sqlQuery += SQLCst.ON + "aem.IDACCMODEL = am.IDACCMODEL" + Cst.CrLf;
                sqlQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(Cs, "aem") + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b";
                sqlQuery += SQLCst.ON + "b.IDB = @IDB" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "b.IDA_ENTITY = aem.IDA" + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(Cs, "am") + Cst.CrLf;
                sqlQuery += SQLCst.ORDERBY + "am.IDACCMODEL";

                DataParameters parameters = new DataParameters();
                parameters.Add(m_AccQuery.paramIdB);
                QueryParameters qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                DataSet dsEarAccModel = DataHelper.ExecuteDataset(CSTools.SetCacheOn(Cs), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
                DataTable dtEarAccModel = dsEarAccModel.Tables[0];
                DataRow[] drEarAccModel = dtEarAccModel.Select();

                if (drEarAccModel.Length == 0)
                {
                    // No accounting model defined for this book
                    codeReturn = Cst.ErrLevel.MISSINGPARAMETER;
                    AccLog.FireException(null,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05345",
                            new ProcessState(ProcessStateTools.StatusErrorEnum, codeReturn),
                                pAccEar.lstData.Find(match => match.First == "ENTITY").Second,
                                pAccEar.lstData.Find(match => match.First == "BOOK").Second,
                                pAccEar.lstData.Find(match => match.First == "TRADE").Second,
                                pAccEar.lstData.Find(match => match.First == "PROCESSDATE").Second));
                }

                int accModelID;
                string accModelIdentifier;

                foreach (DataRow rowAccModel in drEarAccModel)
                {
                    accModelID = Convert.ToInt32(rowAccModel["IDACCMODEL"]);
                    accModelIdentifier = rowAccModel["IDENTIFIER"].ToString();

                    if (false == pBook.AccModels.Exists(model => model == accModelID))
                        pBook.AccModels.Add(accModelID);

                    if (false == m_Models.Exists(model => model.IDAccModel == accModelID))
                        m_Models.Add(new AccModel(accModelID, accModelIdentifier));
                }

#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
            catch (Exception ex)
            {
                AccLog.FireException( ex,
                    new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05383",
                    new ProcessState(ProcessStateTools.StatusErrorEnum),
                        pAccEar.lstData.Find(match => match.First == "EAR").Second,
                        pAccEar.lstData.Find(match => match.First == "DTEVENT").Second,
                        pAccEar.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                        pAccEar.lstData.Find(match => match.First == "SOURCE").Second,
                        pAccEar.lstData.Find(match => match.First == "BOOK").Second,
                        pAccEar.lstData.Find(match => match.First == "TRADE").Second,
                        pAccEar.lstData.Find(match => match.First == "PROCESSDATE").Second));

            }

            return codeReturn;
        }
        #endregion LoadEarAccountingModel

        /// <summary>
        /// Mettre à jour la table EAR_ACCMODEL pour tous les EARs ignorés par le parametrage
        /// </summary>
        /// <param name="pIgnoredEAR"></param>
        private void UpdateEARIgnored(List<Pair<int, int>> pIgnoredEAR)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            if (pIgnoredEAR.Count > 0)
            {
                List<int> lstEARs = new List<int>();
                List<int> lstModels = new List<int>();

                foreach (Pair<int, int> ignoredEAR in pIgnoredEAR)
                {
                    if (false == lstEARs.Exists(id => id == ignoredEAR.First))
                        lstEARs.Add(ignoredEAR.First);

                    if (false == lstModels.Exists(id => id == ignoredEAR.Second))
                        lstModels.Add(ignoredEAR.Second);
                }

                string sqlLstEARs = DataHelper.SQLCollectionToSqlList(Cs, lstEARs, TypeData.TypeDataEnum.@int);
                string sqlLstModels = DataHelper.SQLCollectionToSqlList(Cs, lstModels, TypeData.TypeDataEnum.@int);

                string SQLSelectEarAccModelWhere = SQLCst.WHERE + "(IDEAR in (" + sqlLstEARs + "))";
                SQLSelectEarAccModelWhere += SQLCst.AND + "(IDACCMODEL in (" + sqlLstModels + "))";

                DataSet dsEarAccModel = DataHelper.ExecuteDataset(CSTools.SetCacheOn(Cs), CommandType.Text,
                    m_AccQuery.SQL_Select_EarAccModel + SQLSelectEarAccModelWhere);

                DataTable dtEARAccModel = dsEarAccModel.Tables[0];

                foreach (Pair<int, int> ignoredEAR in pIgnoredEAR)
                {
                    DataRow[] drEarModel = dtEARAccModel.Select("IDEAR = " + ignoredEAR.First + " and  IDACCMODEL = " + ignoredEAR.Second);

                    if (drEarModel.Length > 0)
                    {
                        drEarModel[0].BeginEdit();
                        drEarModel[0]["ISIGNORED"] = 1;
                        drEarModel[0].EndEdit();
                    }
                    else
                    {
                        DataRow drEarModelIgnored = dtEARAccModel.NewRow();
                        drEarModelIgnored.BeginEdit();
                        drEarModelIgnored["IDEAR"] = ignoredEAR.First;
                        drEarModelIgnored["IDACCMODEL"] = ignoredEAR.Second;
                        drEarModelIgnored["ISIGNORED"] = 1;
                        // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                        //drEarModelIgnored["DTINS"] = processLog.GetDate();
                        drEarModelIgnored["DTINS"] = OTCmlHelper.GetDateSysUTC(Cs);
                        drEarModelIgnored["IDAINS"] = Session.IdA;
                        drEarModelIgnored.EndEdit();

                        dtEARAccModel.Rows.Add(drEarModelIgnored);
                    }
                }

                int rowEarAccModel = DataHelper.ExecuteDataAdapter(Cs, m_AccQuery.SQL_Select_EarAccModel, dtEARAccModel);
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }

        #region DeleteEarEntries
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        private int DeleteEarEntries(AccEar pAccEar)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            string sqlQuery = SQLCst.DELETE_DBO + Cst.OTCml_TBL.ACCDAYBOOK.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "IDEAR = @IDEAR" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(m_AccQuery.paramIdEAR);
            QueryParameters qryParam = new QueryParameters(accGenMQueue.ConnectionString, sqlQuery, parameters);
            int ret = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return ret;
        }
        #endregion
        //
        #region ProcessEarDetails
        /// <summary>
        /// Génération des écritures comptables pour chaque EARDET de l'EAR
        /// </summary>
        /// <param name="pIsTradeRemoved"></param>
        /// <param name="pAccEar"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public Cst.ErrLevel ProcessEAR(bool pIsTradeRemoved, AccEar pAccEar)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, "  ...............................");
#endif
            
            //string logMessage = string.Empty;
            SysMsgCode logMessage = new SysMsgCode(SysCodeEnum.SYS, 0);
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            ProcessState processState;

            if (pIsTradeRemoved)
            {
                #region Remove Ear Entries
                try
                {
                    #region Load Cancelled entries
                    string SQLSelectEarEntries = SQLCst.SELECT + "IDACCDAYBOOK,IDSTACTIVATION,";
                    SQLSelectEarEntries += "DTEVENTCANCEL,DTEARCANCEL,DTACCDAYBOOKCANCEL,DTSYSCANCEL" + Cst.CrLf;
                    SQLSelectEarEntries += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCDAYBOOK + Cst.CrLf;

                    string SQLSelectEarEntriesWhere = SQLCst.WHERE + "(IDEAR = @IDEAR)" + Cst.CrLf;

                    DataParameters parameters = new DataParameters();
                    parameters.Add(m_AccQuery.paramIdEAR);
                    QueryParameters qryParam = new QueryParameters(Cs, SQLSelectEarEntries + SQLSelectEarEntriesWhere, parameters);
                    DataSet dsEntry = DataHelper.ExecuteDataset(Cs, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
                    DataTable dtEntry = dsEntry.Tables[0];
                    #endregion
                    //
                    if (null != dtEntry.Rows)
                    {
                        if (dtEntry.Rows.Count > 0)
                        {
                            string dtMinString = DtFunc.DateTimeToStringDateISO(DateTime.MinValue);
                            string dtAccdaybookIsNull = "ISNULL(DTACCDAYBOOKCANCEL,#" + dtMinString + "#)=#" + dtMinString + "#";
                            //
                            DataRow[] rowsEntryCancelled = dtEntry.Select("NOT(" + dtAccdaybookIsNull + ")");
                            DataRow[] rowsEntryNotCancelled = dtEntry.Select(dtAccdaybookIsNull);
                            int nbEntryCancelled = (rowsEntryCancelled == null ? 0 : rowsEntryCancelled.Length);
                            int nbEntryNotCancelled = (rowsEntryNotCancelled == null ? 0 : rowsEntryNotCancelled.Length);

                            if (nbEntryCancelled == dtEntry.Rows.Count)
                            {
                                #region All entries are already cancelled
                                codeReturn = Cst.ErrLevel.DATANOTFOUND;
                                processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, codeReturn);
                                
                                
                                Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(processState.CurrentStatus), new SysMsgCode(SysCodeEnum.LOG, 5346), 0,
                                    new LogParam(pAccEar.lstData.Find(match => match.First == "EAR").Second),
                                    new LogParam(pAccEar.lstData.Find(match => match.First == "BOOK").Second)));
                                #endregion
                            }
                            else
                            {
                                #region Cancel entries
                                foreach (DataRow rowEntryToCancel in rowsEntryNotCancelled)
                                {
                                    rowEntryToCancel["IDSTACTIVATION"] = Cst.StatusActivation.DEACTIV;
                                    rowEntryToCancel["DTEVENTCANCEL"] = pAccEar.DateEventCancel;
                                    rowEntryToCancel["DTEARCANCEL"] = pAccEar.DateEarCancel;
                                    rowEntryToCancel["DTACCDAYBOOKCANCEL"] = pAccEar.DateAccounting;
                                    // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                                    //rowEntryToCancel["DTSYSCANCEL"] = processLog.GetDate();
                                    // FI 20200820 [25468] date systèmes en UTC
                                    rowEntryToCancel["DTSYSCANCEL"] = OTCmlHelper.GetDateSysUTC(Cs);
                                }

                                int nRows = DataHelper.ExecuteDataAdapter(Cs, SQLSelectEarEntries, dtEntry);
                                #endregion
                            }
                        }
                        else
                        {
                            #region No entries
                            codeReturn = Cst.ErrLevel.DATANOTFOUND;

                            processState = new ProcessState(ProcessStateTools.StatusWarningEnum, codeReturn);
                            
                            
                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(processState.CurrentStatus), new SysMsgCode(SysCodeEnum.LOG, 5347), 0,
                                new LogParam(pAccEar.lstData.Find(match => match.First == "EAR").Second),
                                new LogParam(pAccEar.lstData.Find(match => match.First == "BOOK").Second)));
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    AccLog.FireException( ex,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05372",
                            new ProcessState(ProcessStateTools.StatusErrorEnum),
                                pAccEar.lstData.Find(match => match.First == "EAR").Second,
                                pAccEar.lstData.Find(match => match.First == "DTEVENT").Second,
                                pAccEar.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                                pAccEar.lstData.Find(match => match.First == "SOURCE").Second,
                                pAccEar.lstData.Find(match => match.First == "BOOK").Second,
                                pAccEar.lstData.Find(match => match.First == "TRADE").Second,
                                pAccEar.lstData.Find(match => match.First == "PROCESSDATE").Second));
                }
                #endregion
            }
            else
            {
                #region Generate EAR Entries
                try
                {
                    #region Declarations
                    IDbConnection dbConnection = null;
                    //
                    DataTable dtEARDet = null;
                    DataRow[] drEARDet = null;
                    DataRow[] drEARDetStream0 = null;
                    //
                    AccBook accBook = null;
                    AccModel currentAccModel = null;
                    #endregion

                    accBook = m_Books.Find(book => book.IDB == pAccEar.IDB);
                    if (null == accBook)
                    {
                        accBook = new AccBook(pAccEar.IDB);
                        m_Books.Add(accBook);
                        codeReturn = LoadBookAccountingModel(pAccEar, ref accBook);
                    }

                    if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                    {
                        // Supprimer les écritures des EARs du jour,
                        // NB: les EARs passés qui ont déjà généré des écritures ne sont pas chargés
                        if (pAccEar.IsToDayEAR)
                            DeleteEarEntries(pAccEar);
#if DEBUG
                        diagnosticDebug.Start("LoadEARDetails");
#endif
                        codeReturn = pAccEar.LoadEARDetails(m_AccQuery.paramIdEAR, ref dtEARDet);
                        drEARDet = dtEARDet.Select();
#if DEBUG
                        diagnosticDebug.End("LoadEARDetails");
#endif
                        #region Process all EarDets with all Models
                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                        {
                            try
                            {
                                List<AccEarDet> processedEarDetsForModel = null;
                                int streamNO;

                                #region Begin Transaction
                                dbConnection = DataHelper.OpenConnection(Cs);
                                pAccEar.DBTransaction = DataHelper.BeginTran(dbConnection);
                                #endregion

                                pAccEar.AccDayBooks = new List<Pre_DayBook>();

                                foreach (int currentIDAccModel in accBook.AccModels)
                                {
                                    #region Model by Model
                                    currentAccModel = m_Models.Find(model => model.IDAccModel == currentIDAccModel);
                                    currentAccModel.IsEarIgnoredByModel = false;

                                    if (false == pAccEar.IsToDayEAR)
                                        currentAccModel.IsEarIgnoredByModel = pAccEar.ModelsAlreadyIgnore.Exists(id => id == currentAccModel.IDAccModel);

                                    if (currentAccModel.IsEarIgnoredByModel == false)
                                    {
                                        processedEarDetsForModel = new List<AccEarDet>();

                                        foreach (DataRow rowEarDet in drEARDet)
                                        {
                                            // All Streams
                                            if (currentAccModel.IsEarIgnoredByModel == false)
                                            {
                                                streamNO = Convert.ToInt32(rowEarDet["STREAMNO"]);

                                                if ((pAccEar.IsSream0Exist == false ||
                                                    pAccEar.IsOtherSreamExist == false ||
                                                    streamNO != 0))
                                                {
                                                    ProcessEarByModel(pAccEar, rowEarDet, currentAccModel,
                                                        ref processedEarDetsForModel);
                                                }
                                            }
                                            else
                                                break;
                                        }

                                        if (currentAccModel.IsEarIgnoredByModel == false &&
                                            pAccEar.IsSream0Exist &&
                                            pAccEar.IsOtherSreamExist)
                                        {
                                            // Stream 0
                                            drEARDetStream0 = dtEARDet.Select("STREAMNO=0");
                                            foreach (DataRow rowEarDetStream0 in drEARDetStream0)
                                                ProcessEarByModel(pAccEar, rowEarDetStream0, currentAccModel,
                                                    ref processedEarDetsForModel);
                                        }

                                        if (currentAccModel.IsEarIgnoredByModel == false)
                                        {
                                            #region Check if Entries are generated for this EAR
                                            processState = new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.CodeReturnMissingParameterEnum);
                                            bool isTolog = false;

                                            foreach (AccEarDet accEarDet in processedEarDetsForModel)
                                            {
                                                if (accEarDet.IsInstrEnvDetMatch == false)
                                                {
                                                    isTolog = true;
                                                    
                                                    //logMessage = "SYS-05355"; //"No entry generated [No set defined]";
                                                    logMessage = new SysMsgCode(SysCodeEnum.SYS, 5355); //"No entry generated [No set defined]";
                                                }
                                                else if (accEarDet.IsKeyMatch == false)
                                                {
                                                    isTolog = true;
                                                    
                                                    //logMessage = "SYS-05356"; //"No entry generated [No breaking down key matchs]";
                                                    logMessage = new SysMsgCode(SysCodeEnum.SYS, 5356); //"No entry generated [No breaking down key matchs]";
                                                }
                                                else if (currentAccModel.IsEarIgnoredByModel == true)
                                                    isTolog = false;
                                                else
                                                {
                                                    #region Check all Eardays
                                                    string earCode = string.Empty;
                                                    string eventClass = string.Empty;
                                                    string earCodeLoged = string.Empty;
                                                    string eventClassLoged = string.Empty;

                                                    isTolog = false;

                                                    foreach (DataRow rowScheme in accEarDet.EarSchemes)
                                                    {
                                                        if (Convert.ToBoolean(rowScheme["ISSCHEMEDEFINED"]) == false)
                                                        {
                                                            earCode = rowScheme["EARCODE"].ToString();
                                                            eventClass = rowScheme["EVENTCLASS"].ToString();
                                                            //													
                                                            if (earCodeLoged != earCode || eventClassLoged != eventClass)
                                                            {
                                                                // "Scheme not defined [" + earCode + "/" + eventClass + "]";
                                                                isTolog = true;

                                                                earCodeLoged = earCode;
                                                                eventClassLoged = eventClass;
                                                            }
                                                        }
                                                        //
                                                        if (isTolog)
                                                        {
                                                            
                                                            
                                                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5357), 0,
                                                                    new LogParam(earCode + "/" + eventClass),
                                                                    new LogParam(currentAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                                                                    new LogParam(accEarDet.lstData.Find(match => match.First == "INSTR_IDENTIFIER").Second),
                                                                    new LogParam(accEarDet.lstData.Find(match => match.First == "STREAMNO").Second),
                                                                    new LogParam(pAccEar.lstData.Find(match => match.First == "EAR").Second),
                                                                    new LogParam(pAccEar.lstData.Find(match => match.First == "BOOK").Second),
                                                                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                                                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second)));

                                                            isTolog = false;
                                                        }
                                                    }
                                                    #endregion
                                                }

                                                if (isTolog)
                                                {
                                                    
                                                    
                                                    Logger.Log(new LoggerData(LogLevelEnum.Warning, logMessage, 0,
                                                        new LogParam(currentAccModel.lstData.Find(match => match.First == "ACCMODEL").Second),
                                                        new LogParam(pAccEar.lstData.Find(match => match.First == "EAR").Second),
                                                        new LogParam(pAccEar.lstData.Find(match => match.First == "BOOK").Second),
                                                        new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                                                        new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second)));
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                            // EAR is ignored by Model
                                            pAccEar.ModelsNewlyIgnore.Add(Convert.ToInt32(m_AccQuery.paramIdAccModel.Value));
                                    }
                                    #endregion
                                }

                                WriteEntry(pAccEar);

                                if (ProcessTuningSpecified)
                                {
#if DEBUG
                                    diagnosticDebug.Start("ProcessTuning: UpdateEventStatus");
#endif
                                    //m_EventsInfo.Update(pAccEar.DBTransaction,
                                    m_EventsInfo.Update(pAccEar.DBTransaction, Cst.OTCml_TBL.EVENTSTCHECK);
                                    m_EventsInfo.Update(pAccEar.DBTransaction, Cst.OTCml_TBL.EVENTSTMATCH);
                                    m_EventsInfo.Update(pAccEar.DBTransaction, Cst.OTCml_TBL.EVENT);
#if DEBUG
                                    diagnosticDebug.End("ProcessTuning: UpdateEventStatus");
#endif
                                }

                                // RD 20120809 [18070] Optimisation
#if DEBUG
                                diagnosticDebug.Start("WriteEventProcess");
#endif
                                List<int> events =
                                    (from pre_DayBook in pAccEar.AccDayBooks
                                     from ide in pre_DayBook.Events
                                     select ide).Distinct().ToList();

                                // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                                //EventProcess.Write(m_AccQuery.CS, pAccEar.DBTransaction, events,
                                //        Cst.ProcessTypeEnum.ACCOUNTGEN, ProcessStateTools.StatusSuccessEnum, processLog.GetDate(),
                                //        string.Empty, tracker.idTRK_L, 0);
                                EventProcess.Write(m_AccQuery.CS, pAccEar.DBTransaction, events,
                                        Cst.ProcessTypeEnum.ACCOUNTGEN, ProcessStateTools.StatusSuccessEnum, OTCmlHelper.GetDateSysUTC(Cs),
                                        string.Empty, Tracker.IdTRK_L, 0);
#if DEBUG
                                diagnosticDebug.End("WriteEventProcess");
#endif
                                // Commit
                                if (pAccEar.DBTransaction != null)
                                    DataHelper.CommitTran(pAccEar.DBTransaction, false);
                            }
                            catch (Exception)
                            {
                                if (pAccEar.DBTransaction != null)
                                    DataHelper.RollbackTran(pAccEar.DBTransaction, false);

                                throw;
                            }
                            finally
                            {
                                pAccEar.DBTransaction = null;
                                DataHelper.CloseConnection(dbConnection);
                            }
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    AccLog.FireException( ex,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05382",
                        new ProcessState(ProcessStateTools.StatusErrorEnum),
                            pAccEar.lstData.Find(match => match.First == "EAR").Second,
                            pAccEar.lstData.Find(match => match.First == "DTEVENT").Second,
                            pAccEar.lstData.Find(match => match.First == "IDSTACTIVATION").Second,
                            pAccEar.lstData.Find(match => match.First == "SOURCE").Second,
                            pAccEar.lstData.Find(match => match.First == "BOOK").Second,
                            pAccEar.lstData.Find(match => match.First == "TRADE").Second,
                            pAccEar.lstData.Find(match => match.First == "PROCESSDATE").Second));


                }
                #endregion
            }
            //
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "..........................................");
#endif
            return codeReturn;
        }
        #endregion ProcessEarDetails
        //
        #region private SetIsAccErrorNoAmount
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        private void SetIsAccErrorNoAmount()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            m_IsAccErrorNoAmount = true;

            try
            {
                string sqlQuery = SQLCst.SELECT + "ISACCERRORNOAMOUNT" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "IDEFSSOFTWARE=@IDEFSSOFTWARE" + Cst.CrLf;
                
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(accGenMQueue.ConnectionString, "IDEFSSOFTWARE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),Software.Name);
                QueryParameters qryParam = new QueryParameters(Cs, sqlQuery, parameters);
                object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(accGenMQueue.ConnectionString), CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

                if (null != obj)
                    m_IsAccErrorNoAmount = Convert.ToBoolean(obj);
            }
            catch { m_IsAccErrorNoAmount = true; }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }
        #endregion SetIsAccErrorNoAmount
        #region private LoadRemovedTradeEar
        /// <summary>
        /// Charge tous les EARs de ce Trade, pour lesquels les écritures comptables seront annulées.
        /// </summary>
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        private void LoadTradeEarForRemove(ref DataRow[] pdrEar)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            try
            {
                StrBuilder sqlQuery = new StrBuilder();
                sqlQuery += SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT_DISTINCT + Cst.CrLf;
                sqlQuery += "ear.IDB, ear.IDEAR, ear.DTEAR, ear.DTEVENT, ear.IDSTACTIVATION, ear.DTREMOVED, ear.SOURCE," + Cst.CrLf;
                sqlQuery += "ear.DTEARCANCEL, ear.DTEVENTCANCEL" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EAR.ToString() + " ear " + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "ear.IDT = @IDT" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "ear.DTEVENT <= @DTPROCESS" + Cst.CrLf;

                DataParameters parameters = new DataParameters();
                parameters.Add(m_AccQuery.paramIdT);
                parameters.Add(m_AccQuery.paramDtProcess);
                QueryParameters qryParam = new QueryParameters(Cs, sqlQuery.ToString(), parameters);
                DataSet dsEar = DataHelper.ExecuteDataset(accGenMQueue.ConnectionString, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
                DataTable dtEar = dsEar.Tables[0];
                pdrEar = dtEar.Select();
                //
                if (pdrEar.Length == 0)
                {
                    // No Ear for this Trade
                    AccLog.FireException(null,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, "LOG-05345",
                        new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND),
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second));
                }
            }
            catch (Exception ex)
            {
                AccLog.FireException(ex,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05381",
                        new ProcessState(ProcessStateTools.StatusErrorEnum), ex,
                            m_LstMasterData.Find(match => match.First == "TRADE").Second,
                            m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second));

            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }
        #endregion LoadTradeEAR
        #region private LoadTradeEAR
        /// <summary>
        /// Charge tous les EARs de ce Trade
        /// </summary>
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        private void LoadTradeEAR(out DataRow[] pDrEAR, out DataTable pDtEARIgnored)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            // RD 20091207 16783 Modification de la query pour tenir compte de EAR_ACCMODEL.ISIGNORED
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT_DISTINCT + Cst.CrLf;
            sqlQuery += "tblmain.IDEAR, tblmain.IDB, tblmain.DTEAR, tblmain.DTEVENT, tblmain.IDSTACTIVATION, tblmain.DTREMOVED, tblmain.SOURCE, tblmain.IDACCMODEL" + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf + Cst.CrLf;
            /* Tous les EARs du jour non annulés*/
            sqlQuery += SQLCst.SELECT;
            sqlQuery += "ear.IDEAR, ear.IDB, ear.DTEAR, ear.DTEVENT, ear.IDSTACTIVATION, ear.DTREMOVED, ear.SOURCE, null as IDACCMODEL" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EAR + " ear" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "ear.IDT = @IDT" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "ear.DTEVENT = @DTPROCESS" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "ear.DTEVENTCANCEL" + SQLCst.IS_NULL + Cst.CrLf;
            sqlQuery += SQLCst.UNION + Cst.CrLf;
            /* Tous les EARs passés non annulés, non déjà ignorés par le paramétrage et qui n'ont pas généré d'écritures*/
            sqlQuery += SQLCst.SELECT;
            sqlQuery += "ear.IDEAR, ear.IDB, ear.DTEAR, ear.DTEVENT, ear.IDSTACTIVATION, ear.DTREMOVED, ear.SOURCE, eam.IDACCMODEL" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EAR + " ear" + Cst.CrLf;
            sqlQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.EAR_ACCMODEL.ToString() + " eam" + SQLCst.ON + "eam.IDEAR = ear.IDEAR" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(ear.IDT = @IDT)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(ear.DTEVENT < @DTPROCESS)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(ear.DTEVENTCANCEL" + SQLCst.IS_NULL + ")" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(eam.ISIGNORED" + SQLCst.IS_NULL + SQLCst.OR + "eam.ISIGNORED = 0)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACCDAYBOOK + " entry" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "entry.IDEAR = ear.IDEAR)" + Cst.CrLf;
            sqlQuery += ") tblmain" + Cst.CrLf;
            sqlQuery += SQLCst.ORDERBY + "tblmain.IDB, tblmain.DTEAR, tblmain.DTEVENT" + Cst.CrLf;
            sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            /* Tous les EARs passés, non annulés et ignorés par le parametrage*/
            sqlQuery += SQLCst.SQL_ANSI + Cst.CrLf + SQLCst.SELECT;
            sqlQuery += "eam.IDEAR, eam.IDACCMODEL" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EAR + " ear" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EAR_ACCMODEL.ToString() + " eam" + SQLCst.ON + "eam.IDEAR = ear.IDEAR" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "ear.IDT = @IDT" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "ear.DTEVENT < @DTPROCESS" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "ear.DTEVENTCANCEL" + SQLCst.IS_NULL + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(eam.ISIGNORED = 1)" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(m_AccQuery.paramIdT);
            parameters.Add(m_AccQuery.paramDtProcess);
            QueryParameters qryParam = new QueryParameters(accGenMQueue.ConnectionString, sqlQuery.ToString(), parameters);
            DataSet dsEAR = DataHelper.ExecuteDataset(accGenMQueue.ConnectionString, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

            DataTable dtEAR = dsEAR.Tables[0];
            pDtEARIgnored = dsEAR.Tables[1];
            pDrEAR = dtEAR.Select();

            if (pDrEAR.Length == 0)
            {
                AccLog.FireException(null,
                        new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "LOG-05338",
                        new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND)));
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }
        #endregion LoadTradeEAR
        //
        #region InitParameters
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        protected void InitAccQuery()
        {
            m_AccQuery = new AccQuery();
            m_AccQuery.CS = Cs;

            m_AccQuery.paramDtProcess = new DataParameter(Cs, "DTPROCESS", DbType.DateTime);
            m_AccQuery.paramIdAccModel = new DataParameter(Cs, "IDACCMODEL", DbType.Int32);
            m_AccQuery.paramIdAccInstrEnvDet = new DataParameter(Cs, "IDACCINSTRENVDET", DbType.Int32);
            m_AccQuery.paramIdAccCondition = new DataParameter(Cs, "IDACCCONDITION", DbType.Int32);

            #region Ear
            m_AccQuery.paramIdEAR = new DataParameter(Cs, "IDEAR", DbType.Int32);
            m_AccQuery.paramIdT = new DataParameter(Cs, "IDT", DbType.Int32);
            m_AccQuery.paramIdB = new DataParameter(Cs, "IDB", DbType.Int32);
            #endregion Ear
            #region EarDet
            m_AccQuery.paramInstrumentNo = new DataParameter(Cs, "INSTRUMENTNO", DbType.Int32);
            m_AccQuery.paramStreamNo = new DataParameter(Cs, "STREAMNO", DbType.Int32);
            m_AccQuery.paramNoZero = new DataParameter(Cs, "NOZERO", DbType.Int32)
            {
                Value = 0
            };
            #endregion EarDet
            #region EarDay
            m_AccQuery.paramEarType = new DataParameter(Cs, "EARTYPE", DbType.AnsiString, SQLCst.UT_EARTYPE_LEN);
            m_AccQuery.paramIdEarType = new DataParameter(Cs, "IDEARTYPE", DbType.Int32);

            m_AccQuery.paramEarCode = new DataParameter(Cs, "EARCODE", DbType.AnsiString, SQLCst.UT_EARCODE_LEN);
            m_AccQuery.paramEventClass = new DataParameter(Cs, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN);
            m_AccQuery.paramAmountType = new DataParameter(Cs, "AMOUNTTYPE", DbType.AnsiString, SQLCst.UT_AMOUNTTYPE_LEN);
            #endregion EarDay

            #region Valuation
            m_AccQuery.paramIdT.Value = accGenMQueue.id;
            m_AccQuery.paramDtProcess.Value = m_AccDate;
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAccEar"></param>
        // RD 20140916 [20328] Add parameters paramIDE, paramEARTYPE and paramIDEARTYPE
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        protected void InitAccQueryForWrite(AccEar pAccEar)
        {
            #region AccDayBook
            m_AccQuery.paramIDACCDAYBOOK = new DataParameter(Cs, "IDACCDAYBOOK", DbType.Int32) ;
            m_AccQuery.paramIDACCDAYBOOK_SRC = new DataParameter(Cs, "IDACCDAYBOOK_SRC", DbType.Int32);
            m_AccQuery.paramIDACCSCHEME = new DataParameter(Cs, "IDACCSCHEME", DbType.Int32);
            m_AccQuery.paramIDACCENTRY = new DataParameter(Cs, "IDACCENTRY", DbType.Int32);
            m_AccQuery.paramIDSESSION = new DataParameter(Cs, "IDSESSION", DbType.Int32);

            m_AccQuery.paramIDE = new DataParameter(Cs, "IDE", DbType.Int32);
            m_AccQuery.paramIDA_ENTITY = new DataParameter(Cs, "IDA_ENTITY", DbType.Int32);
            m_AccQuery.paramIDA_PARTY = new DataParameter(Cs, "IDA_PARTY", DbType.Int32);
            m_AccQuery.paramIDB_PARTY = new DataParameter(Cs, "IDB_PARTY", DbType.Int32);
            m_AccQuery.paramIDA_COUNTERPARTY = new DataParameter(Cs, "IDA_COUNTERPARTY", DbType.Int32);
            m_AccQuery.paramIDB_COUNTERPARTY = new DataParameter(Cs, "IDB_COUNTERPARTY", DbType.Int32);
            m_AccQuery.paramDTEVENT = new DataParameter(Cs, "DTEVENT", DbType.Date); // FI 20200610 [XXXXX] DbType.Date
            m_AccQuery.paramDTEAR = new DataParameter(Cs, "DTEAR", DbType.Date); // FI 20200610 [XXXXX] DbType.Date
            m_AccQuery.paramDTENTRY = new DataParameter(Cs, "DTENTRY", DbType.Date); // FI 20200610 [XXXXX] DbType.Date
            m_AccQuery.paramDTACCDAYBOOK = new DataParameter(Cs, "DTACCDAYBOOK", DbType.Date); // FI 20200610 [XXXXX] DbType.Date
            m_AccQuery.paramIDSTACTIVATION = new DataParameter(Cs, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN);

            m_AccQuery.paramCASHSECURITIES = new DataParameter(Cs, "CASHSECURITIES", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            m_AccQuery.paramEXCHANGETYPE = new DataParameter(Cs, "EXCHANGETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);
            m_AccQuery.paramQUOTEEXCHANGETYPE = new DataParameter(Cs, "QUOTEEXCHANGETYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN);

            m_AccQuery.paramAMOUNT = new DataParameter(Cs, "AMOUNT", DbType.Decimal); 
            m_AccQuery.paramIDC = new DataParameter(Cs, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN); 
            m_AccQuery.paramISDEBIT = new DataParameter(Cs, "ISDEBIT", DbType.Boolean);
            m_AccQuery.paramACCOUNT = new DataParameter(Cs, "ACCOUNT", DbType.AnsiString, SQLCst.UT_ACCOUNTNUMBER_LEN);
            m_AccQuery.paramACCOUNTVALUE = new DataParameter(Cs, "ACCOUNTVALUE", DbType.AnsiString, SQLCst.UT_ACCOUNTNUMBER_LEN);

            m_AccQuery.paramLABEL = new DataParameter(Cs, "LABEL", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramLABELVALUE = new DataParameter(Cs, "LABELVALUE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramDEACTIVLABEL = new DataParameter(Cs, "DEACTIVLABEL", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramDEACTIVLABELVALUE = new DataParameter(Cs, "DEACTIVLABELVALUE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramDEACTIVPOS = new DataParameter(Cs, "DEACTIVPOS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            m_AccQuery.paramREVERSALLABEL = new DataParameter(Cs, "REVERSALLABEL", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramREVERSALLABELVALUE = new DataParameter(Cs, "REVERSALLABELVALUE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramREVERSALPOS = new DataParameter(Cs, "REVERSALPOS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);

            m_AccQuery.paramLABEL2 = new DataParameter(Cs, "LABEL2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramLABELVALUE2 = new DataParameter(Cs, "LABELVALUE2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramDEACTIVLABEL2 = new DataParameter(Cs, "DEACTIVLABEL2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramDEACTIVLABELVALUE2 = new DataParameter(Cs, "DEACTIVLABELVALUE2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramDEACTIVPOS2 = new DataParameter(Cs, "DEACTIVPOS2", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);
            m_AccQuery.paramREVERSALLABEL2 = new DataParameter(Cs, "REVERSALLABEL2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramREVERSALLABELVALUE2 = new DataParameter(Cs, "REVERSALLABELVALUE2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramREVERSALPOS2 = new DataParameter(Cs, "REVERSALPOS2", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN);

            m_AccQuery.paramJOURNALCODE = new DataParameter(Cs, "JOURNALCODE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramJOURNALCODEVALUE = new DataParameter(Cs, "JOURNALCODEVALUE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramCONSOCODE = new DataParameter(Cs, "CONSOCODE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            m_AccQuery.paramCONSOCODEVALUE = new DataParameter(Cs, "CONSOCODEVALUE", DbType.AnsiString, SQLCst.UT_LABEL_LEN);

            m_AccQuery.paramACCEXTLLINK = new DataParameter(Cs, "ACCEXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            m_AccQuery.paramACCEXTLLINKVALUE = new DataParameter(Cs, "ACCEXTLLINKVALUE", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN);
            m_AccQuery.paramIDAINS = new DataParameter(Cs, "IDAINS", DbType.Int32);

            m_AccQuery.paramREVERSEDTENTRY = new DataParameter(Cs, "DTENTRY", DbType.Date);  // FI 20200610 [XXXXX] DbType.Date
            m_AccQuery.paramREVERSEISDEBIT = new DataParameter(Cs, "ISDEBIT", DbType.Boolean); 
            m_AccQuery.paramREVERSEIDENTRY_SRC = new DataParameter(Cs, "IDACCDAYBOOK_SRC", DbType.Int32); 
            m_AccQuery.paramNEWLABELVALUE = new DataParameter(Cs, "LABELVALUE", DbType.AnsiString, SQLCst.UT_LABEL_LEN); 
            m_AccQuery.paramNEWLABELVALUE2 = new DataParameter(Cs, "LABELVALUE2", DbType.AnsiString, SQLCst.UT_LABEL_LEN);
            #endregion

            #region Valuation
            m_AccQuery.paramIDAINS.Value = Session.IdA;

            m_AccQuery.paramDTEAR.Value = pAccEar.DateEar;
            m_AccQuery.paramDTEVENT.Value = pAccEar.DateEvent;
            m_AccQuery.paramDTACCDAYBOOK.Value = pAccEar.DateAccounting;
            m_AccQuery.paramIDSTACTIVATION.Value = pAccEar.IDStActivation;

            m_AccQuery.paramACCEXTLLINK.Value = string.Empty;
            m_AccQuery.paramACCEXTLLINKVALUE.Value = string.Empty;
            m_AccQuery.paramIDACCDAYBOOK_SRC.Value = Convert.DBNull;
            m_AccQuery.paramIDSESSION.Value = 0;
            #endregion
        }
        #endregion InitParameters

        /// <summary>
        /// Ecriture dans la table ACCDAYBOOK
        /// </summary>
        /// <param name="pAccEar"></param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public void WriteEntry(AccEar pAccEar)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);

#endif
            int numberOfTokenAccDayBook = pAccEar.AccDayBooks.Count;
            numberOfTokenAccDayBook +=
                (from pre_DayBook in pAccEar.AccDayBooks
                 where pre_DayBook.IsReversal
                 select pre_DayBook).Count();

            Cst.ErrLevel codeReturn = SQLUP.GetId(out int newIDACCDAYBOOK, pAccEar.DBTransaction, SQLUP.IdGetId.ACCDAYBOOK,
                    SQLUP.PosRetGetId.First, numberOfTokenAccDayBook);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                InitAccQueryForWrite(pAccEar);

                foreach (Pre_DayBook pre_DayBook in pAccEar.AccDayBooks)
                {
                    pre_DayBook.Initialize(m_AccQuery, m_TradeInfo.UnknownProduct, pAccEar.IdBCAccount_Entity);

                    m_AccQuery.paramIDACCDAYBOOK.Value = newIDACCDAYBOOK;

                    DataParameters parameters = new DataParameters();
                    parameters.Add(m_AccQuery.paramIdT);
                    parameters.Add(m_AccQuery.paramIDACCDAYBOOK);
                    parameters.Add(m_AccQuery.paramIDACCDAYBOOK_SRC);
                    parameters.Add(m_AccQuery.paramIDACCSCHEME);
                    parameters.Add(m_AccQuery.paramIDACCENTRY);
                    parameters.Add(m_AccQuery.paramIDSESSION);
                    parameters.Add(m_AccQuery.paramIdEAR);
                    parameters.Add(m_AccQuery.paramInstrumentNo);
                    parameters.Add(m_AccQuery.paramStreamNo);
                    parameters.Add(m_AccQuery.paramEarType);
                    parameters.Add(m_AccQuery.paramIdEarType);
                    parameters.Add(m_AccQuery.paramIDA_ENTITY);
                    parameters.Add(m_AccQuery.paramIDA_PARTY);
                    parameters.Add(m_AccQuery.paramIDB_PARTY);
                    parameters.Add(m_AccQuery.paramIDA_COUNTERPARTY);
                    parameters.Add(m_AccQuery.paramIDB_COUNTERPARTY);
                    parameters.Add(m_AccQuery.paramDTEVENT);
                    parameters.Add(m_AccQuery.paramDTEAR);
                    parameters.Add(m_AccQuery.paramDTENTRY);
                    parameters.Add(m_AccQuery.paramDTACCDAYBOOK);
                    parameters.Add(m_AccQuery.paramIDSTACTIVATION);
                    parameters.Add(m_AccQuery.paramCASHSECURITIES);
                    parameters.Add(m_AccQuery.paramEarCode);
                    parameters.Add(m_AccQuery.paramEventClass);
                    parameters.Add(m_AccQuery.paramEXCHANGETYPE);
                    parameters.Add(m_AccQuery.paramQUOTEEXCHANGETYPE);
                    parameters.Add(m_AccQuery.paramAmountType);
                    parameters.Add(m_AccQuery.paramAMOUNT);
                    parameters.Add(m_AccQuery.paramIDC);
                    parameters.Add(m_AccQuery.paramISDEBIT);
                    parameters.Add(m_AccQuery.paramACCOUNT);
                    parameters.Add(m_AccQuery.paramACCOUNTVALUE);
                    parameters.Add(m_AccQuery.paramLABEL);
                    parameters.Add(m_AccQuery.paramLABELVALUE);
                    parameters.Add(m_AccQuery.paramDEACTIVLABEL);
                    parameters.Add(m_AccQuery.paramDEACTIVLABELVALUE);
                    parameters.Add(m_AccQuery.paramDEACTIVPOS);
                    parameters.Add(m_AccQuery.paramREVERSALLABEL);
                    parameters.Add(m_AccQuery.paramREVERSALPOS);
                    parameters.Add(m_AccQuery.paramLABEL2);
                    parameters.Add(m_AccQuery.paramLABELVALUE2);
                    parameters.Add(m_AccQuery.paramDEACTIVLABEL2);
                    parameters.Add(m_AccQuery.paramDEACTIVLABELVALUE2);
                    parameters.Add(m_AccQuery.paramDEACTIVPOS2);
                    parameters.Add(m_AccQuery.paramREVERSALLABEL2);
                    parameters.Add(m_AccQuery.paramREVERSALPOS2);
                    parameters.Add(m_AccQuery.paramJOURNALCODE);
                    parameters.Add(m_AccQuery.paramJOURNALCODEVALUE);
                    parameters.Add(m_AccQuery.paramCONSOCODE);
                    parameters.Add(m_AccQuery.paramCONSOCODEVALUE);
                    parameters.Add(m_AccQuery.paramACCEXTLLINK);
                    parameters.Add(m_AccQuery.paramACCEXTLLINKVALUE);
                    parameters.Add(m_AccQuery.paramIDAINS);

                    QueryParameters qryParam = new QueryParameters(Cs, m_AccQuery.SQL_Insert_Entry, parameters);
                    DataHelper.ExecuteNonQuery(pAccEar.DBTransaction, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

                    // RD 20140916 [20328] Insert Events into ACCDAYBOOK_EVENT
                    WriteAccDaybookEvent(pAccEar.DBTransaction, pre_DayBook.Events);

                    newIDACCDAYBOOK++;

                    if (pre_DayBook.IsReversal)
                    {
                        // Inverser le sens 
                        m_AccQuery.paramREVERSEISDEBIT.Value = !Convert.ToBoolean(m_AccQuery.paramISDEBIT.Value);
                        // Calculer la date de contrepassation
                        m_AccQuery.paramREVERSEDTENTRY.Value = Convert.ToDateTime(m_AccQuery.paramDTENTRY.Value).AddDays(1);
                        // Calculer le libellé de contrepassation
                        m_AccQuery.paramNEWLABELVALUE.Value = Pre_DayBook.GetParamLabelValue(m_AccQuery.paramLABELVALUE, m_AccQuery.paramREVERSALPOS, m_AccQuery.paramREVERSALLABELVALUE);
                        m_AccQuery.paramNEWLABELVALUE2.Value = Pre_DayBook.GetParamLabelValue(m_AccQuery.paramLABELVALUE2, m_AccQuery.paramREVERSALPOS2, m_AccQuery.paramREVERSALLABELVALUE2);

                        if (StrFunc.IsFilled(pre_DayBook.DayTypeReversal) &&
                            (pre_DayBook.DayTypeReversal.ToUpper() == DayTypeEnum.Business.ToString().ToUpper()))
                        {
                            if (StrFunc.IsFilled(pAccEar.IdBCAccount_Entity))
                            {
                                // CC/PL 20180612 Init m_tradeLibrary
                                if (null == this.m_tradeLibrary)
                                    m_tradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId); 
                                
                                DateTime dtAdjustedTime = Convert.ToDateTime(m_AccQuery.paramREVERSEDTENTRY.Value);
                                IBusinessDayAdjustments bda = ((IProductBase)m_TradeInfo.UnknownProduct).CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, pAccEar.IdBCAccount_Entity);
                                EFS_AdjustableDate efs_AdjustableDate = new EFS_AdjustableDate(m_AccQuery.CS, dtAdjustedTime, bda, m_tradeLibrary.DataDocument);
                                dtAdjustedTime = efs_AdjustableDate.adjustedDate.DateValue;
                                m_AccQuery.paramREVERSEDTENTRY.Value = dtAdjustedTime;
                            }
                        }

                        // Faire le lien avec l'écriture en cours
                        m_AccQuery.paramREVERSEIDENTRY_SRC.Value = m_AccQuery.paramIDACCDAYBOOK.Value;
                        m_AccQuery.paramIDACCDAYBOOK.Value = newIDACCDAYBOOK;

                        qryParam = new QueryParameters(Cs, m_AccQuery.SQL_Insert_Entry, parameters);
                        DataHelper.ExecuteNonQuery(pAccEar.DBTransaction, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());

                        // RD 20140916 [20328] Insert Events into ACCDAYBOOK_EVENT
                        WriteAccDaybookEvent(pAccEar.DBTransaction, pre_DayBook.Events);

                        newIDACCDAYBOOK++;
                    }
                }
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }

        /// <summary>
        /// Insert into table ACCDAYBOOK_EVENT events of current entry {m_AccQuery.paramIDACCDAYBOOK}
        /// </summary>
        /// <param name="pTransaction"></param>
        /// <param name="pEvents"></param>
        // EG 20231005 [WI721] Comptabilité : Mauvais casting de paramètres sur requêtes
        public void WriteAccDaybookEvent(IDbTransaction pTransaction, List<int> pEvents)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            DataParameters parameters = new DataParameters();
            parameters.Add(m_AccQuery.paramIDE);
            parameters.Add(m_AccQuery.paramIDACCDAYBOOK);
            parameters.Add(m_AccQuery.paramIDAINS);
            QueryParameters qryParam = new QueryParameters(Cs, m_AccQuery.SQL_Insert_AccDaybookEvent, parameters);
            foreach (int ide in pEvents)
            {
                m_AccQuery.paramIDE.Value = ide;
                DataHelper.ExecuteNonQuery(pTransaction, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
        }

        #region protected override ProcessExecuteSpecific
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " -------------------");
#endif
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            try
            {
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5333), 0,
                    new LogParam(m_LstMasterData.Find(match => match.First == "TRADE").Second),
                    new LogParam(m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second)));

                DataRow[] drEar = null;
                DataTable dtEarIgnored = null;

                AccEar accEar = null;
                bool IsTradeRemoved = false;
                List<Pair<int, int>> IgnoredEARByModels = new List<Pair<int, int>>();

                InitAccQuery();

#if DEBUG
                diagnosticDebug.Start("LoadTradeEvents");
#endif
                // EG 20141113 Gestion GPRODUCT
                if (ProcessTuningSpecified)
                    m_EventsInfo = new AccEventInfo(Cs, CurrentId, m_TradeInfo.GProduct, ProcessTuning.GetProcessTuningOutput(Tuning.TuningOutputTypeEnum.OES));
                else
                    m_EventsInfo = new AccEventInfo(Cs, CurrentId, m_TradeInfo.GProduct, null);
#if DEBUG
                diagnosticDebug.End("LoadTradeEvents");
#endif
                if (m_TradeInfo.IsDeactiv)
                {
                    #region Trade is Deactiv
                    if (m_EventsInfo.IsTradeRemoved(m_AccDate))
                    {
                        LoadTradeEarForRemove(ref drEar);
                        IsTradeRemoved = true;
                    }
                    else
                    {
                        #region Trade already deactivated
                        // Log Warning
                        codeReturn = Cst.ErrLevel.DATANOTFOUND;
                        AccLog.FireException( null,
                            new SpheresException2(System.Reflection.MethodInfo.GetCurrentMethod().Name, 2, "LOG-05344",
                            new ProcessState(ProcessStateTools.StatusWarningEnum, codeReturn)));
                        #endregion
                    }
                    #endregion
                }
                else
                    LoadTradeEAR(out drEar, out dtEarIgnored);

                m_Books = new List<AccBook>();
                m_Models = new List<AccModel>();
                m_AccVariables = new List<AccVariable>();
                m_Conditions = new List<AccCondition>();

                SetIsAccErrorNoAmount();

                // Process EAR by EAR
                foreach (DataRow rowEAR in drEar)
                {
                    accEar = new AccEar( ProcessState.SetErrorWarning, m_AccQuery, rowEAR, IsTradeRemoved, m_LstMasterData);

                    if (false == IsTradeRemoved)
                    {
                        // Valoriser la liste des Models qui ont déjà ignoré l'EAR
                        foreach (DataRow rowEARIgnore in dtEarIgnored.Select("IDEAR=" + accEar.IDEAR))
                            accEar.ModelsAlreadyIgnore.Add(Convert.ToInt32(rowEARIgnore["IDACCMODEL"]));
                    }

                    m_AccQuery.paramIdEAR.Value = accEar.IDEAR;
                    m_AccQuery.paramIdB.Value = accEar.IDB;

                    // Génération des écritures pour l'EAR
                    codeReturn = ProcessEAR(IsTradeRemoved, accEar);

                    if (false == IsTradeRemoved)
                    {
                        // Ajouter cet EAR à la liste des EARs nouvellement ignorés par le paramètrage le cas échéant
                        foreach (int idACCMODEL in accEar.ModelsNewlyIgnore)
                            IgnoredEARByModels.Add(new Pair<int, int>(accEar.IDEAR, idACCMODEL));
                    }

                    if (codeReturn != Cst.ErrLevel.SUCCESS)
                        ProcessState.CodeReturn = codeReturn;
                }

                // Mettre à jour les EARs nouvellement ignorés par le paramétrage
                UpdateEARIgnored(IgnoredEARByModels);

            }
            catch (Exception ex)
            {
                // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                AppInstance.AppTraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));

                SpheresException2 exLog = SpheresExceptionParser.GetSpheresException(null, ex);

                // S'il s'agit d'une exception NON SpheresException, alors l'écrire dans le Log
                // Sinon, elle est déjà écrite au moment de sa production
                if (false == (ex is SpheresException2))
                {
                    // FI 20200623 [XXXXX] AddCriticalException
                    ProcessState.AddCriticalException(ex);

                    
                    Logger.Log(new LoggerData(exLog));
                }
                ProcessState.CodeReturn = exLog.ProcessState.CodeReturn;

                // FI 20200916 [XXXXX] pas de throw pour eviter double ajout dans le log de l'exception
                //throw exLog;
            }
            finally
            {

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5334), 0));
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "------------------------------------------");
#endif
            return ProcessState.CodeReturn;
        }
        #endregion ProcessExecuteSpecific
        #region protected override SelectTrades
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override void SelectTrades()
        {
            try
            {
                //20071008 FI ticket 15831 gestion du parametre ENTITY
                if (false == MQueue.IsMasterDateSpecified)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05344",
                        new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND));
                }
                //
                DataParameters parametres = new DataParameters();
                parametres.Add(new DataParameter(Cs, "DATE1", DbType.Date), accGenMQueue.GetMasterDate());

                if (accGenMQueue.idSpecified)
                    parametres.Add(new DataParameter(Cs, "IDT", DbType.Int32), accGenMQueue.id);
                else if (accGenMQueue.identifierSpecified)
                    parametres.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), accGenMQueue.identifier);
                //				
                if (null != accGenMQueue.GetObjectValueParameterById(MQueueBase.PARAM_ENTITY))
                    parametres.Add(new DataParameter(Cs, "ENTITY", DbType.Int32), accGenMQueue.GetIntValueParameterById(EarGenMQueue.PARAM_ENTITY));

                // ***********************************************************************************************
                // ATTENTION: 
                //      Cette query doit être issue du fichier XML du lancement (~\OTCml\XML_Files\ProcessBase\ACCOUNTGEN.xml) de l'appli Web.
                //
                //      Par ailleurs, il faudrait opérer les modifications suivantes sur la query:
                //          1 - Il faut veiller à ce que la 1ère colonne soit IDT et aliasée avec IDDATA (select tblresult.IDT as IDDATA)
                //          2 - Supprimer toutes les lignes de la query qui concernent Session Restrict (voir "%%SR:")
                //          3 - Dans la partie SQL optionnel (voir %%DH:SQLIF%%):
                //              * ne garder que les lignes concernant les trades "COMMON" (Voir définition plus bas) 
                //              * nettoyer la ligne des trades COMMON pour ne garder que le SQL
                //   
                // ***********************************************************************************************
                //  Vu avec PL: 
                //      Le lancement d'un traitement des Ecritures massif est valable uniquement pour les trades "COMMON" (Voir définition plus bas) 
                // ***********************************************************************************************
                // 
                // COMMON : 
                //      - Exclure les trades Administratifs (Factures, ...)
                //      - Exclure les trades Asset (Titres, ...)
                //      - Exclure les trades Risk (Deposit, ...) SAUF: CashBalance et CashPayment
                //
                //      (p.GPRODUCT not in ('ADM','ASSET','RISK') or p.IDENTIFIER in ('cashBalance','cashPayment'))
                //
                // ADMIN : 
                //      - Tous les trades Administratifs (Factures, ...)
                //      
                //      (p.GPRODUCT='ADM')
                //
                // CBI : 
                //      - Les trades CashBalanceInterest (Echelle d'intérêts)
                //
                //      (p.IDENTIFIER='cashBalanceInterest')
                // ***********************************************************************************************

                // 20090615 CC 16057 Modification de la query pour tenir compte de INSTRUMENT.ISACCOUNTING
                // RD 20091207 16783 Modification de la query pour tenir compte de EAR_ACCMODEL.ISIGNORED
                StrBuilder sqlSelect = new StrBuilder();
                sqlSelect += @"select tblresult.IDT as IDDATA, tblresult.IDENTIFIER, tblresult.DISPLAYNAME, tblresult.DESCRIPTION, tblresult.IDI, tblresult.DTTRADE, tblresult.DTSYS,
        tblresult.IDSTENVIRONMENT, tblresult.IDSTBUSINESS, tblresult.IDSTACTIVATION, tblresult.IDSTPRIORITY,
        tblresult.SOURCE, tblresult.GPRODUCT, tblresult.EXTLLINK
        from
        (        
          /* ----------------------------------------------------------------------------------------------------------- */
          /* Tous les EARs du jour non annules                                                                           */
          /* Tous les EARs passes non annules, non deja ignores par le parametrage et qui n''ont pas genere d''ecritures */
          /* ----------------------------------------------------------------------------------------------------------- */
          select tblmain.IDT, tblmain.IDENTIFIER, tblmain.DISPLAYNAME, tblmain.DESCRIPTION, tblmain.IDI, tblmain.DTTRADE, tblmain.DTSYS, 
          tblmain.IDSTENVIRONMENT, tblmain.IDSTBUSINESS, tblmain.IDSTACTIVATION, tblmain.IDSTPRIORITY, 
          tblmain.SOURCE, p.GPRODUCT, tblmain.EXTLLINK
          from dbo.TRADE tblmain
          inner join dbo.INSTRUMENT i on (i.IDI=tblmain.IDI) and (i.ISACCOUNTING=1)
          inner join dbo.PRODUCT p on (p.IDP=i.IDP) and
          (p.GPRODUCT not in ('ADM','ASSET','RISK') or p.IDENTIFIER in ('cashBalance','cashPayment'))
          /* Ne pas considérer les événements des trades désactivés */
          inner join dbo.EAR ear on (ear.IDT=tblmain.IDT)
          left outer join dbo.EAR_ACCMODEL eam on (eam.IDEAR=ear.IDEAR)
          where (tblmain.IDSTENVIRONMENT='REGULAR') and (tblmain.IDSTACTIVATION='REGULAR') and
          (
            /*EARs du jour non annulés*/
            ((ear.DTEVENT=@DATE1) and (ear.DTEVENTCANCEL is null))
            or
            (
              /*EARs passes non annulés*/
              (ear.DTEVENT<@DATE1)
              and (ear.DTEVENTCANCEL is null)
              /*non deja ignores par le parametrage*/
              and (eam.ISIGNORED is null or eam.ISIGNORED=0)
              /*pas d'écritures générées*/
              and not exists 
              (
                select 1
                from dbo.ACCDAYBOOK
                where (ACCDAYBOOK.IDEAR=ear.IDEAR)
              )
            )
          )
          
          union

          /* ----------------------------------------------------------------- */
          /* Tous les EARs annules du jour et passes qui n'ont pas ete traites */
          /* ----------------------------------------------------------------- */
          select tblmain.IDT, tblmain.IDENTIFIER, tblmain.DISPLAYNAME, tblmain.DESCRIPTION, tblmain.IDI, tblmain.DTTRADE, tblmain.DTSYS, 
          tblmain.IDSTENVIRONMENT, tblmain.IDSTBUSINESS, tblmain.IDSTACTIVATION, tblmain.IDSTPRIORITY, 
          tblmain.SOURCE, p.GPRODUCT, tblmain.EXTLLINK
          from dbo.TRADE tblmain
          inner join dbo.INSTRUMENT i on (i.IDI=tblmain.IDI) and (i.ISACCOUNTING=1)
          inner join dbo.PRODUCT p on (p.IDP=i.IDP) and
          (p.GPRODUCT not in ('ADM','ASSET','RISK') or p.IDENTIFIER in ('cashBalance','cashPayment'))
          inner join dbo.EAR ear on (ear.IDT=tblmain.IDT)
          where (tblmain.IDSTENVIRONMENT='REGULAR') and
          /*EARs annules du jour et passes*/
          (ear.DTEVENTCANCEL<=@DATE1)
          /*EARs annules NON traités*/
          and exists 
          (
            select 1
            from dbo.ACCDAYBOOK
            where (ACCDAYBOOK.IDEAR=ear.IDEAR)
            and (ACCDAYBOOK.DTACCDAYBOOKCANCEL is null)
          )
          
        ) tblresult        
        inner join dbo.TRADEACTOR tabuyer on (tabuyer.IDT=tblresult.IDT) and (tabuyer.IDROLEACTOR='COUNTERPARTY')  and (tabuyer.BUYER_SELLER='Buyer')
        inner join dbo.TRADEACTOR taseller on (taseller.IDT=tblresult.IDT) and (taseller.IDROLEACTOR='COUNTERPARTY') and (taseller.BUYER_SELLER='Seller')
        left outer join dbo.BOOK bbuyer on (bbuyer.IDB=tabuyer.IDB)
        left outer join dbo.BOOK bseller on (bseller.IDB=taseller.IDB)
        where (-1=@ENTITY or bbuyer.IDA_ENTITY=@ENTITY or bseller.IDA_ENTITY=@ENTITY)";

                string sqlSelectTrade = sqlSelect.ToString();

                SQLWhere sqlWhere = new SQLWhere();
                if (MQueue.idSpecified)
                    sqlWhere.Append(@"(tblresult.IDT=@IDT)");
                else if (MQueue.identifierSpecified)
                    sqlWhere.Append(@"(tblresult.IDENTIFIER=@IDENTIFIER)");

                sqlSelectTrade += sqlWhere.ToString();

                DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelectTrade, parametres.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-05380",
                    new ProcessState(ProcessStateTools.StatusErrorEnum), ex,
                        m_LstMasterData.Find(match => match.First == "TRADE").Second,
                        m_LstMasterData.Find(match => match.First == "PROCESSDATE").Second);
            }
        }
        #endregion protected SelectTrades()

        protected override void ProcessPreExecute()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " ------------------------");
#endif
            ProcessState.CodeReturn = Cst.ErrLevel.SUCCESS;

            // RD 20120809 [18070] Optimisation
            //CheckLicense();

            if (false == IsProcessObserver)
            {
#if DEBUG
                diagnosticDebug.Start("TradeInfo");
#endif
                m_TradeInfo = new AccTradeInfo(Cs, CurrentId);
#if DEBUG
                diagnosticDebug.End("TradeInfo");
#endif
                if (m_TradeInfo.DtTrade == null)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat("Trade n°{0} not found", CurrentId.ToString()),
                        new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum));
                }
                //
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn) && (false == NoLockCurrentId))
                    ProcessState.CodeReturn = LockCurrentObjectId();
                //
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                    ProcessState.CodeReturn = ScanCompatibility_Trade(CurrentId);
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "------------------------------------------");
#endif
        }
        #endregion Methods
    }
    #endregion class AccountGenProcess
}

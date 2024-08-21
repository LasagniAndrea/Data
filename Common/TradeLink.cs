using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

namespace EFS.TradeLink
{
    #region Enum
    // PM 20130218 [18414] Add PositionAfterCascading & PositionAfterShifting
    // PM 20150311 [POC] Add Folder
    // EG 20190613 [24683] New ClosingPosition, ReopeningPosition
    // EG 20231030 [WI725] New Closing/Reopening : Use module to manage closing suspens after GiveUp cleared / Input
    public enum TradeLinkType
    {
        NA,
        Replace,
        NewIdentifier,
        CreditNote,
        AddInvoice,
        StlInvoice,
        Invoice,
        RemoveInvoice,
        RemoveStlInvoice,
        UnderlyerDeliveryAfterOptionAssignment,
        UnderlyerDeliveryAfterAutomaticOptionAssignment,
        UnderlyerDeliveryAfterAutomaticOptionExercise,
        UnderlyerDeliveryAfterOptionExercise,
        PositionTransfert,
        PrevCashBalance,
        ExchangeTradedDerivativeInCashBalance,
        MarginRequirementInCashBalance,
        CashPaymentInCashBalance,
        PositionAfterCascading,
        PositionAfterShifting,
        PositionAfterCorporateAction,
        MergedTrade,
        SplitTrade,
        Folder,
        Reflect,
        ClosingPosition,
        ReopeningPosition,
        // EG 20231030 [WI725] New
        GiveUpSuspens,
    }
    public enum TradeLinkIdDataIdentification
    {
        NA,
    }
    public enum TradeLinkDataIdentification
    {
        NA,
        OldIdentifier,
        NewIdentifier,
        CreditNoteIdentifier,
        InvoiceIdentifier,
        AddInvoiceIdentifier,
        TradeIdentifier,
        InvoiceSettlementIdentifier,
        RemoveInvoiceSettlementIdentifier,
        AllocatedInvoiceIdentifier,
        RemoveInvoiceIdentifier,
        LinkedInvoiceIdentifier,
        CashBalanceIdentifier,
        PrevCashBalanceIdentifier,
        ExchangeTradedDerivativeIdentifier,
        MarginRequirementIdentifier,
        CashPaymentIdentifier,
        TradeSourceIdentifier,
        TradeReflectIdentifier,
    }
    #endregion Enum
    #region TradeLink
    public class TradeLink
    {
        #region Members
        private int m_IdT_L;
        private int m_IdT_A;//After
        private readonly int m_IdT_B;//Before
        private readonly TradeLinkType m_Link;
        private readonly Nullable<int> m_IdData;
        private readonly string m_IdDataIdentification;
        private string m_Message;
        private readonly string[] m_Data = new string[5] { null, null, null, null, null };
        private readonly string[] m_DataIdentification = new string[5] { null, null, null, null, null };
        #endregion

        #region Constructors
        public TradeLink(int pId_A, int pId_B, TradeLinkType pLink)
            : this(pId_A, pId_B, pLink, null, null)
        { }
        public TradeLink(int pId_A, int pId_B, TradeLinkType pLink,
            Nullable<int> pIdData, string pIdDataIdentification)
            : this(pId_A, pId_B, pLink,
            pIdData, pIdDataIdentification,
            new string[5] { null, null, null, null, null }, new string[5] { null, null, null, null, null })
        { }
        public TradeLink(int pId_A, int pId_B, TradeLinkType pLink,
            Nullable<int> pIdData, string pIdDataIdentification,
            string[] pData, string[] pDataIdentification)
        {
            
            m_IdT_A = pId_A;
            m_IdT_B = pId_B;
            m_Link = pLink;
            //
            m_IdData = pIdData;
            m_IdDataIdentification = pIdDataIdentification;
            //
            for (int i = 0; i < ArrFunc.Count(pData); i++)
            {
                m_Data[i] = pData[i];
            }
            //
            for (int i = 0; i < ArrFunc.Count(pDataIdentification); i++)
            {
                m_DataIdentification[i] = pDataIdentification[i];
            }
        }
        #endregion

        #region Accessors
        public int IdT_A { set { m_IdT_A = value; } get { return m_IdT_A; } }
        public int IdT_B { get { return m_IdT_B; } }
        public TradeLinkType Link { get { return m_Link; } }
        public Nullable<int> IdData { get { return m_IdData; } }
        public string IdDataIdentification { get { return m_IdDataIdentification; } }
        public string Message { get { return m_Message; } }
        public string[] Data { get { return m_Data; } }
        public string[] DataIdentification
        {
            get { return m_DataIdentification; }
        }

        /// <summary>
        /// Schema utilisé dans le flux XML du trade A (After) , pour exprimer le lien sur le trade B (Before)
        /// </summary>
        public string LinkScheme_A
        {
            get
            {
                switch (m_Link)
                {
                    case TradeLinkType.Replace:
                        return Cst.Spheres_Canceled_TradeIdentifierScheme;
                    case TradeLinkType.Reflect:
                        return Cst.Spheres_ReflectionOf_TradeIdentifierScheme;
                    case TradeLinkType.NewIdentifier:
                        return string.Empty; // Pas de Link dans le Trade XML
                    case TradeLinkType.PrevCashBalance:
                        return Cst.Spheres_PrevCashBalance_TradeIdentifierScheme;
                    case TradeLinkType.ExchangeTradedDerivativeInCashBalance:
                        return Cst.Spheres_ETD_TradeIdentifierScheme;
                    case TradeLinkType.MarginRequirementInCashBalance:
                        return Cst.Spheres_MarginRequirement_TradeIdentifierScheme;
                    case TradeLinkType.CashPaymentInCashBalance:
                        return Cst.Spheres_CashPayment_TradeIdentifierScheme;
                    default:
                        return string.Empty;
                }
            }
        }
        /// <summary>
        /// Schema utilisé dans le flux XML du trade B (Before), pour exprimer le lien sur le trade A (After)
        /// </summary>
        public string LinkScheme_B
        {
            get
            {
                switch (m_Link)
                {
                    case TradeLinkType.Replace:
                        return Cst.Spheres_Replacement_TradeIdentifierScheme;
                    case TradeLinkType.Reflect:
                        return Cst.Spheres_ReflectedBy_TradeIdentifierScheme;
                    case TradeLinkType.NewIdentifier:
                        return string.Empty; // Pas de Link dans le Trade XML
                    case TradeLinkType.PrevCashBalance:
                        return Cst.Spheres_NextCashBalance_TradeIdentifierScheme;
                    case TradeLinkType.ExchangeTradedDerivativeInCashBalance:
                    case TradeLinkType.MarginRequirementInCashBalance:
                    case TradeLinkType.CashPaymentInCashBalance:
                        return Cst.Spheres_CashBalance_TradeIdentifierScheme;
                    default:
                        return string.Empty;
                }
            }
        }
        /// <summary>
        /// Identification du trade A (After) dans la table TradeLink (via DATAx)
        /// </summary>
        public string LinkData_A
        {
            get
            {
                switch (m_Link)
                {
                    case TradeLinkType.Replace:
                        return string.Empty; // Pas de détail dans TRADELINK
                    case TradeLinkType.Reflect:
                        return TradeLinkDataIdentification.TradeReflectIdentifier.ToString();
                    case TradeLinkType.NewIdentifier:
                        return TradeLinkDataIdentification.NewIdentifier.ToString();
                    case TradeLinkType.PrevCashBalance:
                    case TradeLinkType.ExchangeTradedDerivativeInCashBalance:
                    case TradeLinkType.MarginRequirementInCashBalance:
                    case TradeLinkType.CashPaymentInCashBalance:
                        return TradeLinkDataIdentification.CashBalanceIdentifier.ToString();
                    default:
                        return string.Empty;
                }
            }
        }
        /// <summary>
        /// Identification du trade B (Before) dans la table TradeLink (via DATAx)
        /// </summary>
        public string LinkData_B
        {
            get
            {
                switch (m_Link)
                {
                    case TradeLinkType.Replace:
                        return string.Empty; // Pas de détail dans TRADELINK
                    case TradeLinkType.Reflect:
                        return TradeLinkDataIdentification.TradeSourceIdentifier.ToString();
                    case TradeLinkType.NewIdentifier:
                        return TradeLinkDataIdentification.OldIdentifier.ToString();
                    case TradeLinkType.PrevCashBalance:
                        return TradeLinkDataIdentification.PrevCashBalanceIdentifier.ToString();
                    case TradeLinkType.ExchangeTradedDerivativeInCashBalance:
                        return TradeLinkDataIdentification.ExchangeTradedDerivativeIdentifier.ToString();
                    case TradeLinkType.MarginRequirementInCashBalance:
                        return TradeLinkDataIdentification.MarginRequirementIdentifier.ToString();
                    case TradeLinkType.CashPaymentInCashBalance:
                        return TradeLinkDataIdentification.CashPaymentIdentifier.ToString();
                    default:
                        return string.Empty;
                }
            }
        }
        /// <summary>
        /// True, si le Lien est à purger s'il existe.
        /// </summary>
        public bool IsDelete
        {
            get
            {
                switch (m_Link)
                {
                    case TradeLinkType.Replace:
                        return false;
                    case TradeLinkType.Reflect:
                        return false;
                    case TradeLinkType.NewIdentifier:
                        return false;
                    case TradeLinkType.PrevCashBalance:
                    case TradeLinkType.ExchangeTradedDerivativeInCashBalance:
                    case TradeLinkType.MarginRequirementInCashBalance:
                    case TradeLinkType.CashPaymentInCashBalance:
                        // Il est possible, de régénérer un Cash-Balance, 
                        // donc il est nécessaire de supprimer l'ancien Lien, pour éviter d'avoir des doublons
                        return true;
                    default:
                        return false;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///  Mise à jour de la table TRADELINK
        /// </summary>
        /// <returns></returns>
        /// FI 20131209 [19320] modification dans le delete
        /// EG 20140224 [19667] 
        public bool Insert(string cs, IDbTransaction dbTransaction)
        {
            bool ret = false;

            /// EG 20140224 [19667] Mise en commentaire du Delete
            //if (IsDelete)
            //{
            //    // FI 20131209 [19320] Appel de la méthode Delete
            //    Delete();
            //}

            #region Select IDTRADE_L
            if (m_IdT_L <= 0)
            {
                string sqlQuery = SQLCst.SELECT + SQLCst.MAX + "(IDT_L) as IDT_L";
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADETRAIL + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "IDT=@IDT" + Cst.CrLf;
                //
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(cs, "IDT", DbType.Int32), m_IdT_A);
                //
                object obj = DataHelper.ExecuteScalar(cs, dbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
                if ((null != obj) && (false == Convert.IsDBNull(obj)))
                    m_IdT_L = Convert.ToInt32(obj);
            }
            #endregion
            //
            if (m_IdT_L > 0)
            {
                #region Query Insert
                string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADELINK + Cst.CrLf;
                SQLInsert += "(IDT_L, IDT_A, IDT_B, LINK, MESSAGE, IDDATA, IDDATAIDENT, DATA1, DATA2, DATA3, DATA4, DATA5, DATA1IDENT, DATA2IDENT, DATA3IDENT, DATA4IDENT, DATA5IDENT)" + Cst.CrLf;
                SQLInsert += "values" + Cst.CrLf;
                SQLInsert += "(@IDT_L, @IDT_A, @IDT_B, @LINK, @MESSAGE, @IDDATA, @IDDATAIDENT, @DATA1, @DATA2, @DATA3, @DATA4, @DATA5, @DATA1IDENT, @DATA2IDENT, @DATA3IDENT, @DATA4IDENT, @DATA5IDENT);";
                #endregion Query Insert
                //
                #region Parameters setting
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(cs, "IDT_L", DbType.Int32), m_IdT_L);
                parameters.Add(new DataParameter(cs, "IDT_A", DbType.Int32), m_IdT_A);
                parameters.Add(new DataParameter(cs, "IDT_B", DbType.Int32), m_IdT_B);
                parameters.Add(new DataParameter(cs, "LINK", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), m_Link.ToString());
                #region Message
                if (StrFunc.IsEmpty(m_Message))
                {
                    switch (m_Link)
                    {
                        case TradeLinkType.Replace:
                            m_Message = "Cancel and replaces";
                            break;
                        case TradeLinkType.Reflect:
                            m_Message = "The trade has been reflected";
                            break;
                        case TradeLinkType.NewIdentifier:
                            m_Message = "New identifier is generated";
                            break;
                        case TradeLinkType.AddInvoice:
                            m_Message = "Additional invoice is generated";
                            break;
                        case TradeLinkType.CreditNote:
                            m_Message = "Credit note is generated";
                            break;
                        case TradeLinkType.Invoice:
                            m_Message = "Invoice is generated";
                            break;
                        case TradeLinkType.StlInvoice:
                            m_Message = "Invoice settlement is generated";
                            break;
                        case TradeLinkType.RemoveInvoice:
                            m_Message = "Invoice is cancelled";
                            break;
                        case TradeLinkType.RemoveStlInvoice:
                            m_Message = "Invoice settlement is cancelled";
                            break;
                        case TradeLinkType.PrevCashBalance:
                            m_Message = "Cash-Balance include Previous one";
                            break;
                        case TradeLinkType.ExchangeTradedDerivativeInCashBalance:
                            m_Message = "Cash-Balance include Exchange Traded Derivative";
                            break;
                        case TradeLinkType.MarginRequirementInCashBalance:
                            m_Message = "Cash-Balance include Margin Requirement";
                            break;
                        case TradeLinkType.CashPaymentInCashBalance:
                            m_Message = "Cash-Balance include Cash Payment";
                            break;
                        case TradeLinkType.PositionAfterCorporateAction:
                            m_Message = "Adjusted trade post CA";
                            break;
                    }
                }
                #endregion
                parameters.Add(new DataParameter(cs, "MESSAGE", DbType.AnsiString, SQLCst.UT_MESSAGE_LEN), m_Message);
                //                    
                parameters.Add(new DataParameter(cs, "IDDATA", DbType.Int32), m_IdData);
                parameters.Add(new DataParameter(cs, "IDDATAIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), m_IdDataIdentification);
                //
                for (int i = 0; i < ArrFunc.Count(m_Data); i++)
                {
                    parameters.Add(new DataParameter(cs, "DATA" + (i + 1).ToString(), DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), m_Data[i]);
                    parameters.Add(new DataParameter(cs, "DATA" + (i + 1).ToString() + "IDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), m_DataIdentification[i]);
                }
                #endregion Parameters setting
                //
                int nRow = DataHelper.ExecuteNonQuery(cs, dbTransaction, CommandType.Text, SQLInsert, parameters.GetArrayDbParameter());
                ret = (nRow == 1);
            }
            return ret;

        }
        #endregion
    }
    #endregion TradeLink
}

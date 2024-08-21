#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.TradeLink;
using EfsML.Business;
using EfsML.Interface;
using System;
using System.Collections.Specialized;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region TradeAdminInput
    public class TradeAdminInput : TradeCommonInput
    {
        #region Members
        private bool m_IsAllocatedInvoiceDates;
        private decimal m_AllocatedAmountInvoiceDates;
        // EG 20110308 HPC Nb ligne de frais sur facture
        private int m_NbDisplayInvoiceTrade;
        private LinkedTradeAdminRemove _linkedTradeAdminRemove;
        #endregion Members
        //
        #region Accessors
        #region AllocatedAmountInvoiceDates
        public decimal AllocatedAmountInvoiceDates
        {
            set { m_AllocatedAmountInvoiceDates = value; }
            get { return m_AllocatedAmountInvoiceDates; }
        }
        #endregion AllocatedAmountInvoiceDates
        #region NbDisplayInvoiceTrade
        // EG 20110308 HPC Nb ligne de frais sur facture
        public int NbDisplayInvoiceTrade
        {
            get
            {
                return m_NbDisplayInvoiceTrade;
            }
        }
        #endregion NbInvoiceTrade


        #region CustomCaptureInfos
        public new TradeAdminCustomCaptureInfos CustomCaptureInfos
        {
            set { base.CustomCaptureInfos = value; }
            get { return (TradeAdminCustomCaptureInfos)base.CustomCaptureInfos; }
        }
        #endregion CustomCaptureInfos
        #region IsAllocatedInvoiceDates
        public bool IsAllocatedInvoiceDates
        {
            set { m_IsAllocatedInvoiceDates = value; }
            get { return m_IsAllocatedInvoiceDates; }
        }
        #endregion IsAllocatedInvoiceDates
        #region SQLInvoiceTradeInstrument
        #endregion SQLInvoiceTradeInstrument
        #region IsInvoiceFullyAllocated
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsInvoiceClosed
        {
            get { return m_SQLTrade.RowAttribut == Cst.RowAttribut_InvoiceClosed; }
        }
        #endregion IsInvoiceClosed
        public LinkedTradeAdminRemove LinkedTradeAdminRemove
        {
            get
            {
                return _linkedTradeAdminRemove;
            }
        }

        #endregion Accessors
        //
        #region Constructor
        public TradeAdminInput()
            : base()
        {
        }
        #endregion Constructors
        //
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pItemOccurs"></param>
        /// <returns></returns>
        // EG 20200903 [XXXXX] Test invoiceTrade.identifier non null
        public string DisplayInvoiceTradeBanner(string pCS, int pItemOccurs)
        {

            string ret = string.Empty;
            if (IsTradeFound)
            {
                if (Product.IsInvoice || Product.IsAdditionalInvoice || Product.IsCreditNote)
                {
                    IInvoice invoice = (IInvoice)Product.Product;
                    if ((null != invoice.InvoiceDetails.InvoiceTrade) && (0 < invoice.InvoiceDetails.InvoiceTrade.Length))
                    {
                        IInvoiceTrade invoiceTrade = invoice.InvoiceDetails.InvoiceTrade[pItemOccurs - 1];
                        #region Trade
                        if (null != invoiceTrade.Identifier)
                            ret = invoiceTrade.Identifier.Value;
                        #endregion Trade
                        #region Instrument
                        if (null != invoiceTrade.Instrument)
                        {
                            SQL_Instrument sql_Instrument = SQLInvoiceTradeInstrument(pCS, invoiceTrade.Instrument.OTCmlId);
                            if ((null != sql_Instrument) && sql_Instrument.IsLoaded)
                                ret += Cst.Space + "[" + sql_Instrument.Identifier + "]";
                        }
                        #endregion Instrument
                    }
                }
                else if (Product.IsInvoiceSettlement)
                {

                }
            }
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNbTrade"></param>
        /// <returns></returns>
        /// EG 20110308 Gestion du titre sur le détail des frais (ajout nb de ligne dans le cas d'un affichage partiel)
        public string DisplayInvoiceFee(int pNbTrade)
        {
            string ret = string.Empty;
            if (IsTradeFound)
            {
                if (Product.IsInvoice || Product.IsAdditionalInvoice || Product.IsCreditNote)
                {
                    ret = Ressource.GetString("InvoiceFees");
                    IInvoice invoice = (IInvoice)Product.Product;
                    if ((null != invoice.InvoiceDetails.InvoiceTrade) && (0 < invoice.InvoiceDetails.InvoiceTrade.Length))
                    {
                        m_NbDisplayInvoiceTrade = invoice.InvoiceDetails.InvoiceTrade.Length;
                        if (pNbTrade < m_NbDisplayInvoiceTrade)
                        {
                            m_NbDisplayInvoiceTrade = pNbTrade;
                            ret += "<span class='size5'>" + Ressource.GetString2("NbDisplayLine", pNbTrade.ToString(), invoice.InvoiceDetails.InvoiceTrade.Length.ToString()) + "</span>";
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne une nouvelle instance de SQL_Instrument
        /// </summary>
        /// <param name="pOTCmlId"></param>
        /// <returns></returns>
        public SQL_Instrument SQLInvoiceTradeInstrument(string pCS, int pOTCmlId)
        {
            SQL_Instrument sql_Instrument = new SQL_Instrument(CSTools.SetCacheOn(pCS), pOTCmlId);
            return sql_Instrument;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeIdentifier"></param>
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pTradeIdentifier)
        {
            InitializeSqlTrade(pCS, pDbTransaction, pTradeIdentifier, SQL_TableWithID.IDType.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdType"></param>
        /// <param name="pId"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public override void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType)
        {
            m_SQLTrade = new SQL_TradeAdmin(pCS, pIdType, pId)
            {
                DbTransaction = pDbTransaction
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        protected override bool CheckIsTradeFound(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            Cst.StatusEnvironment stEnv = Cst.StatusEnvironment.UNDEFINED;
            bool isTemplale = TradeRDBMSTools.IsTradeTemplate(pCS, pId, pIdType);
            if (isTemplale)
                stEnv = Cst.StatusEnvironment.TEMPLATE;

            string sqlTradeId = pId.Replace("%", SQL_TableWithID.StringForPERCENT);

            SQL_TradeAdmin sqlTrade = new SQL_TradeAdmin(pCS, pIdType, sqlTradeId, stEnv, SQL_Table.RestrictEnum.Yes, pUser, pSessionId)
            {
                DbTransaction = pDbTransaction
            };

            return sqlTrade.IsFound;
        }

        /// <summary>
        /// Définit une nouvelle instance des ccis
        /// <para>
        /// Synchronize des pointeurs existants dans les CciContainers avec les éléments du dataDocument
        /// </para>
        /// </summary>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Modification de siganture
        public override void InitializeCustomCaptureInfos(string pCS, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
        {
            CustomCaptureInfos = new TradeAdminCustomCaptureInfos(pCS, this, pUser, pSessionId,  pIsGetDefaultOnInitializeCci);
            // FI 20180709 [XXXXX]
            CustomCaptureInfos.InitializeCciContainer();
        }
        
        /// <summary>
        /// Initialise les membres dédiés à une action (Remove,Exercice,CorrectionOfQunatity,etc...)
        /// </summary>
        /// <param name="pMode"></param>
        /// EG 20150513 [20513] Type RemoveTradeMsg remplace RemoveTrade
        public override void InitializeForAction(string pCS, Cst.Capture.ModeEnum pMode)
        {
            if (Cst.Capture.IsModeRemove(pMode))
            {
                _linkedTradeAdminRemove = new LinkedTradeAdminRemove(pCS, this);
                m_RemoveTrade = new RemoveTradeMsg(IdT, Identifier, GetDefaultDateAction(pCS, pMode), LinkedTradeAdminRemove.LinkedTradeId, null);
            }
        }
        

        #endregion Methods
    }
    #endregion TradeAdminInput

    #region TradeAdminInputGUI
    public class TradeAdminInputGUI : TradeCommonInputGUI
    {
        #region Accessors
        
        /// <summary>
        ///  Retourne l'identifiant du regroupement des produits 
        /// </summary>
        public override Cst.SQLCookieGrpElement GrpElement
        {
            get
            {
                return Cst.SQLCookieGrpElement.SelADMProduct;
            }
        }
        /// <summary>
        ///  Retoune la restriction sur la table PRODUCT
        /// </summary>
        /// <param name="pSqlAlias">alias de la table PRODUCT</param>
        public override string GetSQLRestrictProduct(string pSqlAlias)
        {
            return pSqlAlias + ".GPRODUCT=" + DataHelper.SQLString(Cst.ProductGProduct_ADM);
        }
        
        #endregion Accessors
        #region Constructors
        public TradeAdminInputGUI(string pIdMenu, User pUser, string pXMLFilePath)
            : base(pIdMenu, pUser, pXMLFilePath)
        {
        }
        #endregion Constructors
    }
    #endregion TradeInputGUI

    #region LinkedTradeAdminRemove
    public class LinkedTradeAdminRemove
    {
        #region Membres
        private readonly StringDictionary _linkedTradeIdentifier;
        private readonly StringDictionary _linkedTradeId;
        private readonly TradeAdminInput _tradeAdminInput;
        #endregion Membres

        #region properties
        #region LinkedTradeId
        public StringDictionary LinkedTradeId
        {
            get { return _linkedTradeId; }
        }
        #endregion LinkedTradeId
        #region LinkedTradeIdentifier
        public StringDictionary LinkedTradeIdentifier
        {
            get { return _linkedTradeIdentifier; }
        }
        #endregion LinkedTradeId
        #endregion properties

        #region constructor
        public LinkedTradeAdminRemove(string pCS, TradeAdminInput pInputAdmin)
        {
            _tradeAdminInput = pInputAdmin;
            _linkedTradeId = new StringDictionary();
            _linkedTradeIdentifier = new StringDictionary();
            GetLinkedTrade(pCS);
        }
        #endregion constructor

        #region Method
        #region GetLinkedTrade
        private void GetLinkedTrade(string pCS)
        {
            TradeLinkDataIdentification tradelinkDataIdentification = TradeLinkDataIdentification.NA;
            if (_tradeAdminInput.Product.IsCreditNote)
                tradelinkDataIdentification = TradeLinkDataIdentification.CreditNoteIdentifier;
            else if ((_tradeAdminInput.Product.IsAdditionalInvoice) || (_tradeAdminInput.Product.IsInvoice))
                tradelinkDataIdentification = TradeLinkDataIdentification.InvoiceIdentifier;
            else if (_tradeAdminInput.Product.IsInvoiceSettlement)
                tradelinkDataIdentification = TradeLinkDataIdentification.InvoiceSettlementIdentifier;


            _linkedTradeIdentifier[tradelinkDataIdentification.ToString()] = _tradeAdminInput.Identifier;
            _linkedTradeId[tradelinkDataIdentification.ToString()] = _tradeAdminInput.IdT.ToString();

            GetLinkedTrade(pCS, _tradeAdminInput.IdT, _tradeAdminInput.SQLTradeLink.Rows);
        }
        #endregion
        #region GetLinkedTrade
        // EG/PL/FI 20110204 GROS BUG Select * sur TRADELINK
        private void GetLinkedTrade(string pCS, int pIdT, DataRowCollection pRows)
        {
            #region foreach
            if (null != pRows)
            {
                foreach (DataRow row in pRows)
                {
                    TradeLinkType tradelinkType = TradeLinkType.NA;
                    if (Enum.IsDefined(typeof(TradeLinkType), row["LINK"].ToString()))
                        tradelinkType = (TradeLinkType)Enum.Parse(typeof(TradeLinkType), row["LINK"].ToString(), true);

                    TradeLinkDataIdentification tradelinkDataIdentification = TradeLinkDataIdentification.NA;
                    int idT_A = Convert.ToInt32(row["IDT_A"]);
                    int idT_B = Convert.ToInt32(row["IDT_B"]);

                    string identifier = string.Empty;
                    int id = 0;
                    string dataIdentification = string.Empty;
                    switch (tradelinkType)
                    {
                        case TradeLinkType.AddInvoice:
                        case TradeLinkType.CreditNote:
                        case TradeLinkType.StlInvoice:
                            if (idT_A == pIdT)
                            {
                                id = idT_B;
                                identifier = row["DATA2"].ToString();
                                dataIdentification = row["DATA2IDENT"].ToString();
                            }
                            else if (idT_B == pIdT)
                            {
                                id = idT_A;
                                identifier = row["DATA1"].ToString();
                                dataIdentification = row["DATA1IDENT"].ToString();
                            }
                            if (Enum.IsDefined(typeof(TradeLinkDataIdentification), dataIdentification))
                                tradelinkDataIdentification = (TradeLinkDataIdentification)Enum.Parse(typeof(TradeLinkDataIdentification), dataIdentification, true);
                            {
                                if (tradelinkDataIdentification == TradeLinkDataIdentification.AddInvoiceIdentifier)
                                    tradelinkDataIdentification = TradeLinkDataIdentification.InvoiceIdentifier;

                                if ((tradelinkDataIdentification == TradeLinkDataIdentification.InvoiceSettlementIdentifier) &&
                                    (id != _tradeAdminInput.IdT))
                                {
                                    // Autre règlement que celui en cours d'annulation donc : à ne pas prendre 
                                    continue;
                                }
                                if (_linkedTradeIdentifier.ContainsKey(tradelinkDataIdentification.ToString()))
                                {
                                    if (false == _linkedTradeIdentifier[tradelinkDataIdentification.ToString()].Contains(identifier))
                                    {
                                        _linkedTradeIdentifier[tradelinkDataIdentification.ToString()] += ";" + identifier;
                                        _linkedTradeId[tradelinkDataIdentification.ToString()] += ";" + id.ToString();
                                    }
                                }
                                else
                                {
                                    _linkedTradeIdentifier.Add(tradelinkDataIdentification.ToString(), identifier);
                                    _linkedTradeId.Add(tradelinkDataIdentification.ToString(), id.ToString());
                                }
                            }
                            if (idT_A == pIdT)
                            {
                                SQL_TradeLink sqlTradeLink = new SQL_TradeLink(pCS, idT_B);
                                // EG/PL/FI 20110204 GROS BUG Select * sur TRADELINK
                                // EG 20120621 Add GProduct for IDT_A and IDT_B
                                sqlTradeLink.LoadTable(new string[]{"tl.ACTION as ACTION_A","tl.IDT","tl.IDA","tl.DTSYS",
                                                      "IDT_A","t_A.IDENTIFIER as IDENTIFIER_A", "p_A.GPRODUCT as GPRODUCT_A",
                                                      "IDT_B","t_B.IDENTIFIER as IDENTIFIER_B", "p_B.GPRODUCT as GPRODUCT_B",
                                                      "LINK","MESSAGE","DATA1","DATA1IDENT","DATA2","DATA2IDENT","DATA3","DATA3IDENT"});

                                GetLinkedTrade(pCS, idT_B, sqlTradeLink.Rows);
                            }
                            break;
                    }
                }
            }
            #endregion foreach
        }
        #endregion
        #endregion
    }
    #endregion LinkedTradeAdminRemove
}

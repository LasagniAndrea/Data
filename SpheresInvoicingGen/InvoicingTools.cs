#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.TradeInformation;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
//
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.Administrative.Invoicing
{
    #region InvoicingApplicationPeriodTypeEnum
    public enum InvoicingApplicationPeriodTypeEnum
    {
        InvoicingDate,
        RebateBracket,
        RebateCap,
    }
    #endregion InvoicingApplicationPeriodTypeEnum
    #region InvoicingInstrumentTypeEnum
    public enum InvoicingInstrumentTypeEnum
    {
        AdditionalInvoicing,
        CreditNote,
        Invoicing,
        InvoicingSettlement,
    }
    #endregion InvoicingInstrumentTypeEnum

    #region AdditionalInvoiceInstrument
    /// <summary>
    /// Classe de travail matérialisant un instrument de type : Facture additionnelle
    /// </summary>
    public class AdditionalInvoiceInstrument : AdministrativeInstrumentBase
    {
        #region Constructors
        public AdditionalInvoiceInstrument(ProcessBase pProcess, InvoicingInstrumentTypeEnum pInvoicingInstrumentType, int pIdI, string pLogEntity)
            : base(pProcess, pInvoicingInstrumentType, pIdI, pLogEntity) { }
        #endregion Constructors
    }
    #endregion AdditionalInvoiceInstrument
    #region AdministrativeInstrumentBase
    /// <summary>
    /// Classe commune de travail matérialisant un instrument pour la facturation
    /// </summary>
    public class AdministrativeInstrumentBase
    {
        #region Members
        private readonly string m_Cs;
        private SQL_Instrument m_SQLInstrument;
        private SQL_Product m_SQLProduct;
        private SQL_TradeCommon m_SQLTrade;
        private string m_TemplateIdentifier;
        private string m_ScreenName;
        private DataDocumentContainer m_DataDocument;
        private ProductContainer m_ProductContainer;
        private readonly ProcessBase m_Process;
        private readonly InvoicingInstrumentTypeEnum m_InvoicingInstrumentType;
        #endregion Members
        #region Accessors
        #region DataDocument
        public DataDocumentContainer DataDocument
        {
            get { return m_DataDocument; }
        }
        #endregion DataDocument
        #region ProductContainer
        public ProductContainer ProductContainer
        {
            get { return m_ProductContainer; }
        }
        #endregion ProductContainer
        #region ScreenName
        public string ScreenName
        {
            get { return m_ScreenName; }
        }
        #endregion ScreenName
        #region SQLInstrument
        public SQL_Instrument SQLInstrument
        {
            get { return m_SQLInstrument; }
        }
        #endregion SQLInstrument
        #region SQLProduct
        public SQL_Product SQLProduct
        {
            get { return m_SQLProduct; }
        }
        #endregion SQLProduct
        #region SQLTrade
        public SQL_TradeCommon SQLTrade
        {
            get { return m_SQLTrade; }
        }
        #endregion SQLTrade
        #endregion Accessors
        #region Constructors
        public AdministrativeInstrumentBase(ProcessBase pProcess, InvoicingInstrumentTypeEnum pInvoicingInstrumentType, int pIdI, string pLogEntity)
        {
            m_Process = pProcess;
            m_Cs = pProcess.Cs;
            m_InvoicingInstrumentType = pInvoicingInstrumentType;
            LoadInstrumentAndProduct(pIdI, pLogEntity);
        }
        #endregion Constructors
        #region Methods
        #region CreateDataDocument
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        public Cst.ErrLevel CreateDataDocument()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            m_SQLTrade = new SQL_TradeCommon(m_Cs, m_TemplateIdentifier)
            {
                IsWithTradeXML = true,
                IsAddRowVersion = true
            };
            // RD 20210304 Add "trx."            
            if (m_SQLTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML" }))
            {
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_SQLTrade.TradeXml);
                IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                m_DataDocument = new DataDocumentContainer(dataDoc);
                m_ProductContainer = m_DataDocument.CurrentProduct;
                codeReturn = Cst.ErrLevel.SUCCESS;
            }
            return codeReturn;
        }
        #endregion CreateDataDocument
        #region CreateFrequency
        public ICalculationPeriodFrequency CreateFrequency(PeriodEnum pPeriod, int pPeriodMultiplier, RollConventionEnum pRollConvention)
        {
            return m_DataDocument.CurrentProduct.Product.ProductBase.CreateFrequency(pPeriod, pPeriodMultiplier, pRollConvention);
        }
        #endregion CreateFrequency
        #region LoadInstrumentAndProduct
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel LoadInstrumentAndProduct(int pIdI, string pLogEntity)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            m_SQLInstrument = new SQL_Instrument(m_Cs, pIdI);
            if (m_SQLInstrument.IsLoaded)
            {
                StrBuilder sqlSelect = new StrBuilder();
                sqlSelect += SQLCst.SELECT + "tr.IDENTIFIER as TEMPLATENAME,ig.SCREENNAME" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENTGUI.ToString() + " ig ";
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE.ToString() + " tr ";
                sqlSelect += SQLCst.ON + "(tr.IDT = ig.IDT_TEMPLATE)" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(ig.IDI = " + pIdI.ToString() + ")" + SQLCst.AND + "(ig.ISDEFAULT = 1)" + Cst.CrLf;

                using (IDataReader dr = DataHelper.ExecuteReader(m_Cs, CommandType.Text, sqlSelect.ToString()))
                {
                    if (dr.Read())
                    {
                        m_TemplateIdentifier = dr.GetValue(0).ToString();
                        m_ScreenName = dr.GetValue(1).ToString();
                        m_SQLProduct = new SQL_Product(m_Cs, m_SQLInstrument.IdP);
                        if (false == m_SQLProduct.IsLoaded)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5207), 0,
                                new LogParam(m_SQLInstrument.IdP),
                                new LogParam(LogTools.IdentifierAndId(m_SQLInstrument.Identifier, m_SQLInstrument.IdI)),
                                new LogParam(m_InvoicingInstrumentType),
                                new LogParam(pLogEntity)));
                        }
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        m_Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5208), 0,
                            new LogParam(m_SQLInstrument.Identifier),
                            new LogParam(m_InvoicingInstrumentType),
                            new LogParam(pLogEntity)));
                    }
                }
            }
            else
            {
                m_Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5209), 0,
                    new LogParam(pIdI),
                    new LogParam(m_InvoicingInstrumentType),
                    new LogParam(pLogEntity)));
            }
            return codeReturn;
        }
        #endregion LoadInstrumentAndProduct
        #endregion Methods
    }
    #endregion AdministrativeInstrumentBase
    #region CreditNoteInstrument
    /// <summary>
    /// Classe de travail matérialisant un instrument de type : Avoir
    /// </summary>
    public class CreditNoteInstrument : AdministrativeInstrumentBase
    {
        #region Constructors
        public CreditNoteInstrument(ProcessBase pProcess, InvoicingInstrumentTypeEnum pInvoicingInstrumentType, int pIdI, string pLogEntity)
            : base(pProcess, pInvoicingInstrumentType, pIdI, pLogEntity) { }
        #endregion Constructors
    }
    #endregion CreditNoteInstrument
    #region Invoicing
    /// <summary>
    /// Classe de travail des éléments principaux du traitement de facturation
    /// - Entité
    /// - Contextes de facturations
    /// - Paramètres de la demande de traitement (Entité, Simulation, Critères de filtre)
    /// - Ajout des événements sur le périmètre candidat
    /// </summary>
    public class Invoicing : IComparer
    {
        #region Members
        private readonly InvoicingGenProcessBase m_InvoicingGenProcess;
        public InvoicingParameters parameter;
        public InvoicingEntity entity;
        public InvoicingScope[] scopes;
        #endregion Members
        #region Accessors
        #region EntityIdentifier
        public string EntityIdentifier
        {
            get { return entity.actorEntity.SQLActor.Identifier; }
        }
        #endregion EntityIdentifier
        #region ProcessBase
        public ProcessBase ProcessBase
        {
            get { return m_InvoicingGenProcess.ProcessBase; }
        }
        #endregion ProcessBase
        #endregion Accessors
        #region Constructors
        // EG 20150706 [21021] Nullable<int> for idA_entity
        public Invoicing(InvoicingGenProcessBase pInvoicingGenProcess)
        {
            m_InvoicingGenProcess = pInvoicingGenProcess;
            DateTime invoiceDate = m_InvoicingGenProcess.MasterDate;
            Nullable<int> idA_entity = m_InvoicingGenProcess.Entity;
            bool isSimul = m_InvoicingGenProcess.IsSimulation;
            SQL_Criteria criteria = m_InvoicingGenProcess.Criteria;
            if (idA_entity.HasValue)
            {
                parameter = new InvoicingParameters(invoiceDate, idA_entity.Value, isSimul, criteria);
                entity = new InvoicingEntity(m_InvoicingGenProcess, idA_entity.Value);
            }
        }
        #endregion Constructors
        #region Indexors
        public InvoicingScope this[int pIndex]
        {
            get
            {
                if (null != scopes)
                    return scopes[pIndex];
                return null;
            }
        }
        public InvoicingScope this[string pIdentifier]
        {
            get
            {
                if (null != scopes)
                {
                    foreach (InvoicingScope item in scopes)
                    {
                        if (item.identifier == pIdentifier)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #region AddEvent
        /// <summary>
        /// Ajout d'un événement sur le périmètre de facturation candidat
        /// - les périmètres de facturation sont triés de tels sorte que le dernier périmètre est celui de poids le plus fort.
        /// - l'événement sera donc ajouté sur ce périmètre s'il est candidat (0 < scopeCandidate.ResultMatching)
        /// - sinon un message d'alerte sera généré : Aucune instruction de facturation applicable à la ligne de frais
        /// </summary>
        /// <param name="pRowEvent">Evénement de frais</param>
        /// <param name="pInvoicingContextEvent">Contexte d'événement de l'événement de frais</param>
        /// <param name="pTraderActorByTrade">Traders du trade</param>
        /// <param name="pSalesActorByTrade">Sales du trade</param>
        /// <returns></returns>
        //20091021 FI [add sales in invoice] Add pSalesActorByTrade
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel AddEvent(DataRow pRowEvent, InvoicingContextEvent pInvoicingContextEvent, DataRelation pTraderActorByTrade, DataRelation pSalesActorByTrade)
        {
            InvoicingScope scopeCandidate = this[scopes.Length - 1];
            Cst.ErrLevel codeReturn;
            if (0 < scopeCandidate.ResultMatching)
            {
                codeReturn = scopeCandidate.AddEvent(pRowEvent, pInvoicingContextEvent, pTraderActorByTrade, pSalesActorByTrade);
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5204), 0,
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["TRADE_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDT"]))),
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["INSTR_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDI"]))),
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["PAYER_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDA_PAY"]))),
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["EVENTCODE"].ToString() + "-" + pRowEvent["EVENTTYPE"].ToString(), Convert.ToInt32(pRowEvent["IDE"]))),
                    new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(pRowEvent["VALORISATION"])) + " " + pRowEvent["IDC_FEE"].ToString())));

                codeReturn = Cst.ErrLevel.DATAUNMATCH;
            }
            return codeReturn;
        }
        #endregion AddEvent
        #region WeightingScope
        /// <summary>
        /// Détermination du périmètre de facturation le plus fort par rapport au contexte d'un événement de frais 
        /// Tri des périmètres en fonction du poid de matching
        /// </summary>
        /// <param name="pInvoicingContextEvent">Contexte d'événement de l'événement de frais</param>
        public void WeightingScope(InvoicingContextEvent pInvoicingContextEvent)
        {
            foreach (InvoicingScope item in scopes)
            {
                int contextWeight = 0;
                item.ResultMatching = 1;
                InvoicingContextEnum contextName = InvoicingContextEnum.None;
                FieldInfo[] flds = pInvoicingContextEvent.GetType().GetFields();
                foreach (FieldInfo fld in flds)
                {
                    object[] attributes = fld.GetCustomAttributes(typeof(InvoicingContextWeight), false);
                    if (0 != attributes.GetLength(0))
                    {
                        contextWeight = ((InvoicingContextWeight)attributes[0]).Weight;
                        contextName = ((InvoicingContextWeight)attributes[0]).Name;

                        if (MatchingEnum.UnMatch == item.MatchInvoicingScope(contextName, contextWeight, pInvoicingContextEvent))
                            break;
                    }
                }
            }
            Sort();
        }
        #endregion WeightingScope
        #region Sort
        /// <summary>
        /// Tri des périmètres de facturation 
        /// sur la base du Matching avec les contextes (ResultMatching)
        /// </summary>
        /// <returns></returns>
        public bool Sort()
        {
            bool isOk = (0 < scopes.Length);
            if (isOk)
                Array.Sort(scopes, this);
            return isOk;
        }
        #endregion Sort
        #endregion Methods
        #region Interfaces
        #region IComparer Members
        public int Compare(object pObj1, object pObj2)
        {
            InvoicingScope scope1 = (InvoicingScope)pObj1;
            InvoicingScope scope2 = (InvoicingScope)pObj2;
            return scope1.ResultMatching.CompareTo(scope2.ResultMatching);
        }
        #endregion IComparer Members
        #endregion Interfaces
    }
    #endregion Invoicing
    #region InvoicingActor
    /// <summary>
    /// Classe de travail pour chaque acteur présent sur la facturation
    /// - Acteur Bénéficiaire
    /// - Acteur payeur des frais
    /// - Acteur facturé
    /// - Contrepartie
    /// - Entité
    /// </summary>
    public class InvoicingActor
    {
        #region Members
        private readonly string m_Cs;
        private string xmlId;
        private int id;
        private string identifier;
        private string displayName;
        private SQL_Actor m_SQLActor;
        private string bic;
        #endregion Members
        #region Accessors
        #region SQLActor
        public SQL_Actor SQLActor
        {
            set { m_SQLActor = value; }
            get { return m_SQLActor; }
        }
        #endregion SQLActor
        public string Identifier {get { return identifier; }}
        public string DisplayName { get { return displayName; } }
        public string BIC { get { return bic; } }
        public int Id {get { return id; }}
        public string XmlId {get { return xmlId; }}
        #endregion Accessors
        #region Constructors
        public InvoicingActor(string pConnectionString, int pIdA)
        {
            m_Cs = pConnectionString;
            LoadActor(pIdA);
        }
        #endregion Constructors
        #region Methods
        #region LoadActor
        private void LoadActor(int pIdA)
        {
            m_SQLActor = new SQL_Actor(m_Cs, pIdA);
            if (m_SQLActor.LoadTable())
            {
                id = m_SQLActor.Id;
                identifier = m_SQLActor.Identifier;
                xmlId = m_SQLActor.XmlId;
                displayName = m_SQLActor.DisplayName;
                bic = m_SQLActor.BICorNull;
            }
        }
        #endregion LoadActor
        #endregion Methods
    }
    #endregion InvoicingActor
    #region InvoicingBeneficiary
    /// <summary>
    /// Classe de travail pour l'acteur Bénéficiaire de la facturationchaque acteur présent sur la facturation
    /// - Acteur et Book
    /// </summary>
    // EG 20141020 [20442] New 
    public class InvoicingBeneficiary
    {
        public InvoicingActor actor;
        public InvoicingBook book;

        public InvoicingBeneficiary(string pCS, int pIdA, int pIdB)
        {
            actor = new InvoicingActor(pCS, pIdA);
            book = new InvoicingBook(pCS, pIdB);
        }
    }
    #endregion InvoicingBeneficiary
    #region InvoicingBook
    /// <summary>
    /// Classe de travail pour le book de l'acteur Bénéficiaire et Entité présent sur la facturation
    /// </summary>
    public class InvoicingBook
    {
        #region Members
        private readonly string m_Cs;
        private SQL_Book m_SQLBook;
        #endregion Members
        #region Accessors
        #region SQLBook
        public SQL_Book SQLBook
        {
            get { return m_SQLBook; }
        }
        #endregion SQLBook
        #endregion Accessors
        #region Constructors
        public InvoicingBook(string pConnectionString, int pIdB)
        {
            m_Cs = pConnectionString;
            LoadBook(pIdB);
        }
        #endregion Constructors
        #region Methods
        #region LoadBook
        private void LoadBook(int pIdB)
        {
            m_SQLBook = new SQL_Book(m_Cs, pIdB);
            m_SQLBook.LoadTable();
        }
        #endregion LoadBook
        #endregion Methods
    }
    #endregion InvoicingBook
    #region InvoicingBracket
    /// <summary>
    /// Classe de travail des tranches (Conditions commerciales) d'un périmètre de  facturation
    /// - Caractéristiques
    /// - Bornes et remise
    /// </summary>
    public class InvoicingBracket
    {
        #region Members
        public int idInvoicingRules;
        public int idInvRulesBracket;
        public string identifier;
        public string displayName;
        public decimal lowValue;
        public decimal highValue;
        public decimal discountRate;
        #endregion Members
        #region Constructors
        public InvoicingBracket(int pIdInvoicingRules, DataRow pRowInvRulesBracket)
        {
            idInvoicingRules = pIdInvoicingRules;
            idInvRulesBracket = Convert.ToInt32(pRowInvRulesBracket["IDINVRULESBRACKET"]);
            identifier = pRowInvRulesBracket["IDENTIFIER"].ToString();
            displayName = pRowInvRulesBracket["DISPLAYNAME"].ToString();
            lowValue = Convert.ToDecimal(pRowInvRulesBracket["LOWVALUE"]);
            highValue = Convert.ToDecimal(pRowInvRulesBracket["HIGHVALUE"]);
            discountRate = Convert.ToDecimal(pRowInvRulesBracket["DISCOUNTRATE"]);
        }
        #endregion Constructors
    }
    #endregion InvoicingBracket
    #region InvoicingConditions
    /// <summary>
    /// Classe de travail des conditions commerciales de  facturation
    /// - Plafond
    /// - Tranche et Remise
    /// </summary>
    public class InvoicingConditions : IComparer
    {
        #region Members
        public bool maxValueSpecified;
        public decimal maxValue;
        public bool maxPeriodMltpSpecified;
        public int maxPeriodMltp;
        public bool maxPeriodSpecified;
        public PeriodEnum maxPeriod;
        public bool discountPeriodMltpSpecified;
        public int discountPeriodMltp;
        public bool discountPeriodSpecified;
        public PeriodEnum discountPeriod;
        public bool bracketApplicationSpecified;
        public BracketApplicationEnum bracketApplication;
        public InvoicingBracket[] brackets;
        public InvoicingDates capPeriodDates;
        public InvoicingDates bracketPeriodDates;
        #endregion Members
        #region Constructors
        public InvoicingConditions(DataRow pRowInvoicingRule)
        {
            maxValueSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["MAXVALUE"]));
            if (maxValueSpecified)
                maxValue = Convert.ToDecimal(pRowInvoicingRule["MAXVALUE"]);

            maxPeriodMltpSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["MAXPERIODMLTP"]));
            if (maxPeriodMltpSpecified)
                maxPeriodMltp = Convert.ToInt32(pRowInvoicingRule["MAXPERIODMLTP"]);

            maxPeriodSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["MAXPERIOD"]));
            if (maxPeriodSpecified)
                maxPeriod = StringToEnum.Period(pRowInvoicingRule["MAXPERIOD"].ToString());

            discountPeriodMltpSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["DISCOUNTPERIODMLTP"]));
            if (discountPeriodMltpSpecified)
                discountPeriodMltp = Convert.ToInt32(pRowInvoicingRule["DISCOUNTPERIODMLTP"]);

            discountPeriodSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["DISCOUNTPERIOD"]));
            if (discountPeriodSpecified)
                discountPeriod = StringToEnum.Period(pRowInvoicingRule["DISCOUNTPERIOD"].ToString());

            bracketApplicationSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["BRACKETAPPLICATION"]));
            if (bracketApplicationSpecified)
                bracketApplication = (BracketApplicationEnum)StringToEnum.Parse(pRowInvoicingRule["BRACKETAPPLICATION"].ToString(), BracketApplicationEnum.Unit);
        }
        #endregion Constructors
        #region Indexors
        public InvoicingBracket this[int pIndex]
        {
            get
            {
                if (null != brackets)
                    return brackets[pIndex];
                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #region Sort
        public bool Sort()
        {
            bool isOk = (0 < brackets.Length);
            if (isOk)
                Array.Sort(brackets, this);
            return isOk;
        }
        #endregion Sort
        #endregion Methods
        #region Interfaces
        #region IComparer Members
        public int Compare(object pObj1, object pObj2)
        {
            InvoicingBracket bracket1 = (InvoicingBracket)pObj1;
            InvoicingBracket bracket2 = (InvoicingBracket)pObj2;
            return bracket1.lowValue.CompareTo(bracket2.lowValue);
        }
        #endregion IComparer Members
        #endregion Interfaces
    }
    #endregion InvoicingConditions
    #region InvoicingContext
    /// <summary>
    /// Classe de travail du contexte environnemental d'un événement de frais
    /// - Frais (Type d'événement, Devise, Type de frais)
    /// - Environnement instrumental
    /// - Marché / Contrat
    /// - Sous-jacent
    /// - Devise du trade
    /// - Book du payeur
    /// - Trader
    /// </summary>
    // EG 20141020 [20442] TypeContractEnum replace TypeMarketEnum
    public class InvoicingContext
    {
        #region Members
        public int idA_Payer;
        public string eventType;
        public int idA_Invoiced;
        public string idC_Fee;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_TraderSpecified;
        public int idA_Trader;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeBookSpecified;
        public TypePartyEnum typeBook;
        public bool bookSpecified;
        public int book;
        // EG 20141020 [20442] TypeContractEnum replace TypeMarketEnum
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeMarketContractSpecified;
        public TypeContractEnum typeMarketContract;
        // EG 20141020 [20442] TypeContractEnum replace TypeMarketEnum
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketContractSpecified;
        public int marketContract;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool grpProductSpecified;
        public string grpProduct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeInstrSpecified;
        public TypeInstrEnum typeInstr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool instrSpecified;
        public int instr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool typeInstrUnderlyerSpecified;
        public TypeInstrEnum typeInstrUnderlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool instrUnderlyerSpecified;
        public int instrUnderlyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC_TradeSpecified;
        public string idC_Trade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentTypeSpecified;
        public string paymentType;
        #endregion Members
        #region Constructors
        // EG 20141020 [20442]
        public InvoicingContext(DataRow pRowInvoicingRule)
        {
            // Acteur payer
            idA_Payer = Convert.ToInt32(pRowInvoicingRule["IDA"]);

            // Acteur facturé
            idA_Invoiced = Convert.ToInt32(pRowInvoicingRule["IDA_INVOICED"]);

            // EventType
            eventType = pRowInvoicingRule["EVENTTYPE"].ToString();

            // Devise de frais
            idC_Fee = pRowInvoicingRule["IDC_FEE"].ToString();

            // Trader
            idA_TraderSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["IDA_TRADER"]));
            if (idA_TraderSpecified)
                idA_Trader = Convert.ToInt32(pRowInvoicingRule["IDA_TRADER"]);

            // Book du payer de frais
            typeBookSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["TYPEBOOK"]) &&
                                 Enum.IsDefined(typeof(TypePartyEnum), pRowInvoicingRule["TYPEBOOK"].ToString()));
            if (typeBookSpecified)
                typeBook = (TypePartyEnum)Enum.Parse(typeof(TypePartyEnum), pRowInvoicingRule["TYPEBOOK"].ToString(), true);
            bookSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["BOOK"]));
            if (bookSpecified)
                book = Convert.ToInt32(pRowInvoicingRule["BOOK"]);

            // Marché / Contrat
            // EG 20141020 [20442]
            typeMarketContractSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["TYPEMARKETCONTRACT"]) &&
                                   Enum.IsDefined(typeof(TypeContractEnum), pRowInvoicingRule["TYPEMARKETCONTRACT"].ToString()));
            if (typeMarketContractSpecified)
                typeMarketContract = (TypeContractEnum)Enum.Parse(typeof(TypeContractEnum), pRowInvoicingRule["TYPEMARKETCONTRACT"].ToString(), true);
            marketContractSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["MARKETCONTRACT"]));
            if (marketContractSpecified)
                marketContract = Convert.ToInt32(pRowInvoicingRule["MARKETCONTRACT"]);

            // Groupe de produits - Produit - Groupe d'instruments - Instrument
            grpProductSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["GPRODUCT"]));
            if (grpProductSpecified)
                grpProduct = pRowInvoicingRule["GPRODUCT"].ToString();
            typeInstrSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["TYPEINSTR"]) &&
                                   Enum.IsDefined(typeof(TypeInstrEnum), pRowInvoicingRule["TYPEINSTR"].ToString()));
            if (typeInstrSpecified)
                typeInstr = (TypeInstrEnum)Enum.Parse(typeof(TypeInstrEnum), pRowInvoicingRule["TYPEINSTR"].ToString(), true);
            instrSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["INSTR"]));
            if (instrSpecified)
                instr = Convert.ToInt32(pRowInvoicingRule["INSTR"]);

            // Groupe d'instruments sous-jacent - Instrument sous-jacent
            typeInstrUnderlyerSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["TYPEINSTR_UNL"]) &&
                                   Enum.IsDefined(typeof(TypeInstrEnum), pRowInvoicingRule["TYPEINSTR_UNL"].ToString()));
            if (typeInstrUnderlyerSpecified)
                typeInstrUnderlyer = (TypeInstrEnum)Enum.Parse(typeof(TypeInstrEnum), pRowInvoicingRule["TYPEINSTR_UNL"].ToString(), true);
            instrUnderlyerSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["INSTR_UNL"]));
            if (instrUnderlyerSpecified)
                instrUnderlyer = Convert.ToInt32(pRowInvoicingRule["INSTR_UNL"]);

            // Devise du trade
            idC_TradeSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["IDC_TRADE"]));
            if (idC_TradeSpecified)
                idC_Trade = pRowInvoicingRule["IDC_TRADE"].ToString();

            // PaymentType
            paymentTypeSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["PAYMENTTYPE"]));
            if (paymentTypeSpecified)
                paymentType = pRowInvoicingRule["PAYMENTTYPE"].ToString();
        }
        #endregion Constructors
        #region Accessors
        #region BookSpecified
        public bool BookSpecified
        {
            get { return typeBookSpecified && bookSpecified; }
        }
        #endregion BookSpecified
        #region InstrSpecified
        public bool InstrSpecified
        {
            get { return typeInstrSpecified && instrSpecified; }
        }
        #endregion InstrSpecified
        #region InstrUnderlyerSpecified
        public bool InstrUnderlyerSpecified
        {
            get { return typeInstrUnderlyerSpecified && instrUnderlyerSpecified; }
        }
        #endregion InstrUnderlyerSpecified
        #region MarketContractSpecified
        // EG 20141020 [20442] New
        public bool MarketContractSpecified
        {
            get { return typeMarketContractSpecified && marketContractSpecified; }
        }
        #endregion MarketContractSpecified
        #endregion Accessors
        #region Methods
        #region IsMatch_Contract
        // EG 20141020 [20442] New
        // FI 20170908 [23409] Modify
        public MatchingEnum IsMatch_Contract(Pair<string,int> pIdContract)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (MarketContractSpecified)
            {
                // FI 20170908 [23409] use DerivativeContract
                //if (TypeContractEnum.Contract == typeMarketContract) 
                if (TypeContractEnum.DerivativeContract == typeMarketContract)
                    match = (("DerivativeContract" == pIdContract.First) && (marketContract == pIdContract.Second)) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
                else if (TypeContractEnum.CommodityContract == typeMarketContract)
                    match = (("CommodityContract" == pIdContract.First) && (marketContract == pIdContract.Second)) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_Contract
        #region IsMatch_EventType
        public MatchingEnum IsMatch_EventType(string pEventType)
        {
            if (pEventType == eventType)
                return MatchingEnum.HiMatch;
            return MatchingEnum.UnMatch;
        }
        #endregion IsMatch_EventType
        #region IsMatch_IdA_Invoiced
        public MatchingEnum IsMatch_IdA_Invoiced(int pIdA_Invoiced)
        {
            if (pIdA_Invoiced == idA_Invoiced)
                return MatchingEnum.HiMatch;
            return MatchingEnum.UnMatch;
        }
        #endregion IsMatch_IdA_Invoiced
        #region IsMatch_IdA_Payer
        public MatchingEnum IsMatch_IdA_Payer(int pIdA_Payer)
        {
            if (pIdA_Payer == idA_Payer)
                return MatchingEnum.HiMatch;
            return MatchingEnum.UnMatch;
        }
        #endregion IsMatch_IdA_Payer
        #region IsMatch_IdA_Trader
        // EG 20121002 Test de contrôle du Trader
        public MatchingEnum IsMatch_IdA_Trader(int[] pIdA_Trader)
        {
            if (idA_TraderSpecified && (null != pIdA_Trader) && (1 == pIdA_Trader.Length))
            {
                if (idA_Trader == pIdA_Trader[0])
                    return MatchingEnum.HiMatch;
                else
                    return MatchingEnum.UnMatch;
            }
            else if (false == idA_TraderSpecified)
                return MatchingEnum.LoMatch;
            return MatchingEnum.UnMatch;
        }
        #endregion IsMatch_IdA_Trader
        #region IsMatch_IdB
        public MatchingEnum IsMatch_IdB(int pIdB)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (BookSpecified)
            {
                if (TypePartyEnum.Book == typeBook)
                    match = (book == pIdB) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_IdA_Trader
        #region IsMatch_IdC_Fee
        public MatchingEnum IsMatch_IdC_Fee(string pIdC_Fee)
        {
            if (pIdC_Fee == idC_Fee)
                return MatchingEnum.HiMatch;
            return MatchingEnum.UnMatch;
        }
        #endregion IsMatch_IdC_Fee
        #region IsMatch_IdC_Trade
        public MatchingEnum IsMatch_IdC_Trade(string pIdC_Trade)
        {
            if (idC_TradeSpecified && (pIdC_Trade == idC_Trade))
                return MatchingEnum.HiMatch;
            else if (false == idC_TradeSpecified)
                return MatchingEnum.LoMatch;
            return MatchingEnum.UnMatch;
        }
        #endregion IsMatch_IdC_Trade
        #region IsMatch_Instr
        public MatchingEnum IsMatch_Instr(int pIdI)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (InstrSpecified)
            {
                if (TypeInstrEnum.Instr == typeInstr)
                    match = (instr == pIdI) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_Instr
        #region IsMatch_InstrUnderlyer
        public MatchingEnum IsMatch_InstrUnderlyer(int pIdI)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (InstrUnderlyerSpecified)
            {
                if (TypeInstrEnum.Instr == typeInstr)
                    match = (instrUnderlyer == pIdI) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_InstrUnderlyer
        #region IsMatch_GrpBook
        public MatchingEnum IsMatch_GrpBook(int pGrpBook)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (BookSpecified)
            {
                if (TypePartyEnum.GrpBook == typeBook)
                    match = (book == pGrpBook) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_GrpProduct
        #region IsMatch_GrpContract
        // EG 20141020 [20442] New
        public MatchingEnum IsMatch_GrpContract(int pGrpContract)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (MarketContractSpecified)
            {
                if (TypeContractEnum.GrpContract == typeMarketContract)
                    match = (marketContract == pGrpContract) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_GrpContract
        #region IsMatch_GrpInstr
        public MatchingEnum IsMatch_GrpInstr(int pIdGrpInstr)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (InstrSpecified)
            {
                if (TypeInstrEnum.GrpInstr == typeInstr)
                    match = (instr == pIdGrpInstr) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_GrpInstr
        #region IsMatch_GrpInstrUnderlyer
        public MatchingEnum IsMatch_GrpInstrUnderlyer(int pIdGrpInstr)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (InstrUnderlyerSpecified)
            {
                if (TypeInstrEnum.GrpInstr == typeInstrUnderlyer)
                    match = (instrUnderlyer == pIdGrpInstr) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_GrpInstrUnderlyer
        #region IsMatch_GrpMarket
        // EG 20141020 [20442] Upd
        public MatchingEnum IsMatch_GrpMarket(int pGrpMarket)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (MarketContractSpecified)
            {
                if (TypeContractEnum.GrpMarket == typeMarketContract)
                    match = (marketContract == pGrpMarket) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_GrpMarket
        #region IsMatch_GrpProduct
        public MatchingEnum IsMatch_GrpProduct(string pGrpProduct)
        {
            MatchingEnum match;
            if (grpProductSpecified)
                match = (grpProduct == pGrpProduct) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_GrpProduct
        #region IsMatch_Market
        // EG 20141020 [20442] Upd
        public MatchingEnum IsMatch_Market(int pIdM)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (MarketContractSpecified)
            {
                if (TypeContractEnum.Market == typeMarketContract)
                    match = (marketContract == pIdM) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_Market
        #region IsMatch_PaymentType
        public MatchingEnum IsMatch_PaymentType(string pPaymentType)
        {
            MatchingEnum match;
            if (paymentTypeSpecified)
                match = (pPaymentType == paymentType) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_PaymentType
        #region IsMatch_Product
        public MatchingEnum IsMatch_Product(int pIdP)
        {
            MatchingEnum match = MatchingEnum.Ignore;
            if (InstrSpecified)
            {
                if (TypeInstrEnum.Product == typeInstr)
                    match = (instr == pIdP) ? MatchingEnum.HiMatch : MatchingEnum.UnMatch;
            }
            else
                match = MatchingEnum.LoMatch;

            return match;
        }
        #endregion IsMatch_Product
        #endregion Methods
    }
    #endregion InvoicingContext
    #region InvoicingContextEnum
    /// <summary>
    /// Enumérateur des éléments de contexte
    /// </summary>
    // EG 20141020 [20442] Add IdGrpContract|IdContract
    public enum InvoicingContextEnum
    {
        #region Members
        IdA_Payer,
        IdA_Invoiced,
        EventType,
        IdC_Fee,
        IdA_Trader,
        IdB,
        IdGrpBook,
        IdContract,
        IdGrpContract,
        IdM,
        IdGrpMarket,
        IdI_Underlyer,
        IdI,
        IdGrpInstr_Underlyer,
        IdGrpInstr,
        IdP,
        GrpProduct,
        IdC_Trade,
        PaymentType,
        None,
        #endregion Members
    }
    #endregion InvoicingContextEnum
    #region InvoicingContextEvent
    /// <summary>
    /// Liste des élements d'un contexte facturation avec leur poids (le plus grand = le plus fort)
    /// </summary>
    // EG 20141020 [20442] Add IdGrpContract|IdDC
    public class InvoicingContextEvent
    {
        #region Members
        [InvoicingContextWeight(Weight = 19, Name = InvoicingContextEnum.IdA_Payer)]
        public int idA_Payer;

        [InvoicingContextWeight(Weight = 18, Name = InvoicingContextEnum.IdA_Invoiced)]
        public int idA_Invoiced;

        [InvoicingContextWeight(Weight = 17, Name = InvoicingContextEnum.EventType)]
        public string eventType;

        [InvoicingContextWeight(Weight = 16, Name = InvoicingContextEnum.IdC_Fee)]
        public string idC_Fee;

        [InvoicingContextWeight(Weight = 15, Name = InvoicingContextEnum.IdA_Trader)]
        public int[] idA_Trader;
        public bool idA_TraderSpecified;

        [InvoicingContextWeight(Weight = 14, Name = InvoicingContextEnum.IdB)]
        public int idB;
        public bool idBSpecified;
        [InvoicingContextWeight(Weight = 13, Name = InvoicingContextEnum.IdGrpBook)]
        public int idGrpBook;
        public bool idGrpBookSpecified;

        // EG 20141020 [20442] New
        [InvoicingContextWeight(Weight = 12, Name = InvoicingContextEnum.IdContract)]
        //public int idDC;
        //public bool idDCSpecified;
        public Pair<string,int> idContract;
        public bool idContractSpecified;
        // EG 20141020 [20442] New
        [InvoicingContextWeight(Weight = 11, Name = InvoicingContextEnum.IdGrpContract)]
        public int idGrpContract;
        public bool idGrpContractSpecified;

        [InvoicingContextWeight(Weight = 10, Name = InvoicingContextEnum.IdM)]
        public int idM;
        public bool idMSpecified;
        [InvoicingContextWeight(Weight = 9, Name = InvoicingContextEnum.IdGrpMarket)]
        public int idGrpMarket;
        public bool idGrpMarketSpecified;

        [InvoicingContextWeight(Weight = 8, Name = InvoicingContextEnum.IdI_Underlyer)]
        public int idI_Underlyer;
        public bool idI_UnderlyerSpecified;
        [InvoicingContextWeight(Weight = 7, Name = InvoicingContextEnum.IdI)]
        public int idI;
        [InvoicingContextWeight(Weight = 6, Name = InvoicingContextEnum.IdGrpInstr_Underlyer)]
        public int idGrpInstr_Underlyer;
        public bool idGrpInstr_UnderlyerSpecified;
        [InvoicingContextWeight(Weight = 5, Name = InvoicingContextEnum.IdGrpInstr)]
        public int idGrpInstr;
        public bool idGrpInstrSpecified;

        [InvoicingContextWeight(Weight = 4, Name = InvoicingContextEnum.IdP)]
        public int idP;
        [InvoicingContextWeight(Weight = 3, Name = InvoicingContextEnum.GrpProduct)]
        public string grpProduct;

        [InvoicingContextWeight(Weight = 2, Name = InvoicingContextEnum.IdC_Trade)]
        public string idC_Trade;

        [InvoicingContextWeight(Weight = 1, Name = InvoicingContextEnum.PaymentType)]
        public string paymentType;
        // EG 20091110
        public string family;
        #endregion Members
        #region Accessors
        #region IsRepoOrOption
        private bool IsRepoOrOption
        {
            get
            {
                // EG 20091110
                return (Cst.ProductFamily_DSE == family) || (Cst.ProductFamily_IRD == family);
            }
        }
        #endregion IsRepoOrOption
        #endregion Accessors
        #region Constructors
        // EG 20141020 [20442] New Contract|IdGrpContract
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        public InvoicingContextEvent(string pCS, DataRow pRowEvent, DataRelation pTraderActorByTrade)
        {
            int idT = IntFunc.IntValue(pRowEvent["IDT"].ToString());
            #region idA_Payer
            idA_Payer = Convert.ToInt32(pRowEvent["IDA_PAY"]);
            #endregion idA_Payer
            #region idA_Invoiced
            idA_Invoiced = Convert.ToInt32(pRowEvent["IDA_INVOICED"]);
            #endregion idA_Invoiced
            #region EventType
            eventType = pRowEvent["EVENTTYPE"].ToString();
            #endregion EventType
            #region idC_Fee
            idC_Fee = pRowEvent["IDC_FEE"].ToString();
            #endregion idC_Fee
            #region Trader
            //20091021 FI [add sales in invoice] use pTraderActorByTrade
            DataRow[] rowTraders = pRowEvent.GetChildRows(pTraderActorByTrade);
            if (null != rowTraders)
            {
                ArrayList aTraders = new ArrayList();
                foreach (DataRow rowTrader in rowTraders)
                {
                    if (Convert.ToInt32(rowTrader["IDA_ACTOR"]) == idA_Payer)
                        aTraders.Add(Convert.ToInt32(rowTrader["IDA"]));
                }
                idA_TraderSpecified = 0 < aTraders.Count;
                if (idA_TraderSpecified)
                    idA_Trader = (int[])aTraders.ToArray(typeof(int));

            }
            #endregion Trader
            #region Contract
            // EG 20141020 [20442] New Contract|IdGrpContract
            //idDCSpecified = (false == Convert.IsDBNull(pRowEvent["IDDC"]));
            //if (idDCSpecified)
            //    idDC = Convert.ToInt32(pRowEvent["IDDC"].ToString());
            idContractSpecified = (false == Convert.IsDBNull(pRowEvent["IDXC"]));
            if (idContractSpecified)
            {
                idContract = new Pair<string, int>(pRowEvent["CONTRACTCATEGORY"].ToString(), Convert.ToInt32(pRowEvent["IDXC"].ToString()));
            }
            idGrpContractSpecified = (false == Convert.IsDBNull(pRowEvent["IDGCONTRACT"]));
            if (idGrpContractSpecified)
                idGrpContract = Convert.ToInt32(pRowEvent["IDGCONTRACT"].ToString());
            #endregion Contract
            #region Market
            idMSpecified = (false == Convert.IsDBNull(pRowEvent["IDM"]));
            if (idMSpecified)
                idM = Convert.ToInt32(pRowEvent["IDM"].ToString());
            idGrpMarketSpecified = (false == Convert.IsDBNull(pRowEvent["IDGMARKET"]));
            if (idGrpMarketSpecified)
                idGrpMarket = Convert.ToInt32(pRowEvent["IDGMARKET"].ToString());
            #endregion Market
            #region Product
            grpProduct = pRowEvent["GPRODUCT"].ToString();
            family = pRowEvent["FAMILY"].ToString();
            idP = Convert.ToInt32(pRowEvent["IDP"].ToString());
            #endregion Product
            #region Instr
            idI = Convert.ToInt32(pRowEvent["IDI"].ToString());
            idGrpInstrSpecified = (false == Convert.IsDBNull(pRowEvent["IDGINSTR"]));
            if (idGrpInstrSpecified)
                idGrpInstr = Convert.ToInt32(pRowEvent["IDGINSTR"].ToString());
            DataParameters parameters;
            #endregion Instr
            #region Underlyer
            if (IsRepoOrOption)
            {
                #region Parameters
                parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, InvoicingGenMQueue.PARAM_IDT, DbType.Int32), idT);
                parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), idA_Invoiced);
                parameters.Add(new DataParameter(pCS, "IDROLEGINSTR", DbType.AnsiString), RoleGInstr.INVOICING.ToString());
                #endregion Parameters
                #region Query Search Instr/Ginstr
                string sqlInstrument_UNL = @"select ti.IDI, ig.IDGINSTR
                from dbo.VW_ALLTRADEINSTRUMENT ti
                inner join dbo.INSTRUMENT i on (i.IDI = ti.IDI)
                inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.GPRODUCT <> 'ADM')
                left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = ti.IDI) and (ig.IDROLEGINSTR = @IDROLEGINSTR) and (ig.IDA = @IDA)
                where (ti.IDT = @IDT)

                union all

                select tr.IDI, ig.IDGINSTR
                from dbo.VW_TRADEREPO_UNL tr
                left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = tr.IDI) and (ig.IDROLEGINSTR = @IDROLEGINSTR) and (ig.IDA = @IDA)
                where (tr.IDT = @IDT)" + Cst.CrLf;

                DataSet dsInstrument = DataHelper.ExecuteDataset(pCS, CommandType.Text, sqlInstrument_UNL.ToString(), parameters.GetArrayDbParameter());
                if ((null != dsInstrument) && (0 < dsInstrument.Tables[0].Rows.Count))
                {
                    DataRow row = dsInstrument.Tables[0].Rows[0];
                    idI_Underlyer = Convert.ToInt32(row["IDI"]);
                    idGrpInstr_UnderlyerSpecified = (false == Convert.IsDBNull(row["IDGINSTR"]));
                    if (idGrpInstr_UnderlyerSpecified)
                        idGrpInstr_Underlyer = Convert.ToInt32(row["IDGINSTR"].ToString());

                }
                #endregion Query Search Instr/Ginstr
            }
            #endregion Underlyer
            #region Book
            idBSpecified = (false == Convert.IsDBNull(pRowEvent["IDB_PAY"]));
            if (idBSpecified)
            {
                idB = Convert.ToInt32(pRowEvent["IDB_PAY"].ToString());
                idGrpBookSpecified = (false == Convert.IsDBNull(pRowEvent["IDGBOOK"]));
                if (idGrpBookSpecified)
                    idGrpBook = Convert.ToInt32(pRowEvent["IDGBOOK"].ToString());
            }
            #endregion Book
            #region PaymentType
            paymentType = pRowEvent["PAYMENTTYPE"].ToString();
            #endregion PaymentType
            #region idC_Trade
            //FI 20091211 [16785] Si Swaption recherche de la devise sur le stream du swap dont la effectiveDate est la plus récente
            //EG 20100705 Si strategie recherche de la devise sur le stream dont la effectiveDate la plus petite
            SQL_Product sqlProduct = new SQL_Product(pCS, idP);
            sqlProduct.LoadTable();
            bool isSwaption = (sqlProduct.Identifier == Cst.ProductSwaption);
            bool isSTGexchangeTradedDerivative = (sqlProduct.Identifier == Cst.ProductSTGexchangeTradedDerivative);
            int instrumentNo = (isSwaption || isSTGexchangeTradedDerivative) ? 2 : 1;
            parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, CommandType.Text, "IDT", DbType.Int32), idT);
            parameters.Add(new DataParameter(pCS, CommandType.Text, "INSTRUMENTNO", DbType.Int32), instrumentNo);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + "ts.IDC, ts.IDC2" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADESTREAM.ToString() + " ts" + Cst.CrLf;
            if (isSTGexchangeTradedDerivative)
                query += SQLCst.WHERE + "ts.INSTRUMENTNO>=@INSTRUMENTNO" + Cst.CrLf;
            else
                query += SQLCst.WHERE + "ts.INSTRUMENTNO=@INSTRUMENTNO" + Cst.CrLf;
            query += SQLCst.AND + "ts.IDT=@IDT" + Cst.CrLf;
            query += SQLCst.ORDERBY + "ts.DTEFFECTIVEUNADJ" + Cst.CrLf;
            QueryParameters qry = new QueryParameters(pCS, query.ToString(), parameters);
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                if (dr.Read())
                {
                    object idc = dr["IDC2"];
                    if (idc == Convert.DBNull)
                        idc = dr["IDC"];
                    idC_Trade = Convert.ToString(idc);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
            #endregion idC_Trade
        }
        #endregion Constructors
    }
    #endregion InvoicingContextEvent
    #region InvoicingContextWeight
    /// <summary>
    /// Attribute de détermination du poids d'un élément de contexte
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public sealed class InvoicingContextWeight : Attribute
    {
        #region Members
        private int m_Weight;
        private InvoicingContextEnum m_Name;
        #endregion Members
        #region Accessors
        #region Name
        public InvoicingContextEnum Name
        {
            get { return (m_Name); }
            set { m_Name = value; }
        }
        #endregion Name
        #region Weight
        public int Weight
        {
            get { return (m_Weight); }
            set { m_Weight = value; }
        }
        #endregion Weight
        #endregion Accessors
    }
    #endregion InvoicingContextWeight
    #region InvoicingDates
    /// <summary>
    /// Dates de la facturation
    /// - Périodicité
    /// - Début et fin de période
    /// - Date de facture
    /// </summary>
    public class InvoicingDates
    {
        #region Members
        public ICalculationPeriodFrequency applicationPeriod;
        public DateTime invoiceDate;
        public DateTime startPeriod;
        public DateTime startPeriodPlusOneDay;
        public DateTime endPeriod;
        #endregion Members
        #region Accessors
        #region IsInvoiceDateEqualToEndPeriod
        public bool IsInvoiceDateEqualToEndPeriod
        {
            get { return (invoiceDate.Date == endPeriod.Date); }
        }
        #endregion IsInvoiceDateEqualToEndPeriod
        #region IsInvoicePeriodApplication
        public bool IsInvoicePeriodApplication
        {
            get
            {
                return (PeriodEnum.T == applicationPeriod.Period) && (1 == applicationPeriod.PeriodMultiplier.DecValue);
            }
        }
        #endregion IsInvoicePeriodApplication
        #region IsEqualToInvoicingApplicationPeriod
        public bool IsEqualToInvoicingApplicationPeriod(ICalculationPeriodFrequency pInvoicingApplicationPeriod)
        {
            return (pInvoicingApplicationPeriod.Period == applicationPeriod.Period) &&
                   (pInvoicingApplicationPeriod.PeriodMultiplier.IntValue == applicationPeriod.PeriodMultiplier.IntValue) &&
                   (pInvoicingApplicationPeriod.RollConvention == applicationPeriod.RollConvention);
        }
        #endregion IsEqualToInvoicingApplicationPeriod
        #endregion Accessors
        #region Constructors
        // 20090408 EG AddDays -1 à StartDate
        // 20090408 EG AddDays 1  à StartPeriod
        // EG 20190114 Add detail to ProcessLog Refactoring
        public InvoicingDates(InvoicingGenProcessBase pInvoicingGenProcessBase, InvoicingApplicationPeriodTypeEnum pInvoicingApplicationPeriodType, DateTime pInvoiceDate, ICalculationPeriodFrequency pApplicationPeriod, string pIdC_Fee)
        {

            invoiceDate = pInvoiceDate;
            applicationPeriod = pApplicationPeriod;
            DateTime startDate = new DateTime(invoiceDate.Year, 1, 1);
            // 20090408 EG AddDays -1 à StartDate
            startDate = startDate.AddDays(-1);
            DateTime endDate = new DateTime(invoiceDate.Year, 12, 31);
            EFS_Period[] periods = Tools.ApplyInterval(startDate, endDate, pApplicationPeriod.Interval, pApplicationPeriod.RollConvention);
            if (ArrFunc.IsFilled(periods))
            {
                foreach (EFS_Period period in periods)
                {
                    if ((0 < DateTime.Compare(invoiceDate, period.date1)) && (0 <= DateTime.Compare(period.date2, invoiceDate)))
                    {
                        startPeriod = period.date1.AddDays(1);
                        startPeriodPlusOneDay = startPeriod;
                        endPeriod = period.date2;
                        if (InvoicingApplicationPeriodTypeEnum.InvoicingDate == pInvoicingApplicationPeriodType)
                        {
                            IProduct product = pInvoicingGenProcessBase.CurrentInstrument.DataDocument.CurrentProduct.Product;
                            IOffset offset = product.ProductBase.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.Business);
                            IBusinessCenters businessCenters = offset.GetBusinessCentersCurrency(pInvoicingGenProcessBase.ProcessBase.Cs, null, pIdC_Fee);
                            startPeriodPlusOneDay = Tools.ApplyOffset(pInvoicingGenProcessBase.ProcessBase.Cs, startPeriod, offset, businessCenters);
                        }
                        break;
                    }
                }
            }
            if (DtFunc.IsDateTimeEmpty(startPeriod) || DtFunc.IsDateTimeEmpty(endPeriod))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pInvoicingGenProcessBase.ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5206), 0,
                    new LogParam(DtFunc.DateTimeToStringDateISO(invoiceDate)),
                    new LogParam(pInvoicingApplicationPeriodType),
                    new LogParam(pApplicationPeriod.PeriodMultiplier),
                    new LogParam(pApplicationPeriod.Period),
                    new LogParam(pApplicationPeriod.RollConvention)));
            }
        }
        #endregion Constructors
    }
    #endregion InvoicingDates
    #region InvoicingEntity
    /// <summary>
    /// Classe de travail liée à l'acteur Entité pour la facturation
    /// - Acteur et Book de l'Entité
    /// - Détermination des instruments liés à la facturation (Facture, Facture additionnelle, Avoir et Règlement
    /// - Période de facturation
    /// </summary>
    public class InvoicingEntity
    {
        #region Members
        private readonly InvoicingGenProcessBase m_InvoicingGenProcess;
        public InvoicingInstrumentTypeEnum defaultInstrumentType;

        public InvoicingActor actorEntity;
        public InvoicingBook bookEntity;
        public bool additionalInvoiceInstrSpecified;
        public AdditionalInvoiceInstrument additionalInvoiceInstr;
        public bool invoiceInstrSpecified;
        public InvoiceInstrument invoiceInstr;
        public bool creditNoteInstrSpecified;
        public CreditNoteInstrument creditNoteInstr;
        public bool invoiceStlInstrSpecified;
        public InvoiceStlInstrument invoiceStlInstr;

        public bool defaultApplicationPeriodSpecified;
        public PeriodEnum defaultPeriod_Invoicing;
        public int defaultPeriodMltp_Invoicing;
        public RollConventionEnum defaultRollConvention_Invoicing;
        #endregion Members
        #region Accessors
        #region CurrentInstrument
        public AdministrativeInstrumentBase CurrentInstrument
        {
            get
            {
                bool isInstrSpecified = false;
                AdministrativeInstrumentBase instrument = null;
                switch (defaultInstrumentType)
                {
                    case InvoicingInstrumentTypeEnum.AdditionalInvoicing:
                        isInstrSpecified = additionalInvoiceInstrSpecified;
                        instrument = (AdministrativeInstrumentBase)additionalInvoiceInstr;
                        break;
                    case InvoicingInstrumentTypeEnum.CreditNote:
                        isInstrSpecified = creditNoteInstrSpecified;
                        instrument = (AdministrativeInstrumentBase)creditNoteInstr;
                        break;
                    case InvoicingInstrumentTypeEnum.Invoicing:
                        isInstrSpecified = invoiceInstrSpecified;
                        instrument = (AdministrativeInstrumentBase)invoiceInstr;
                        break;
                    case InvoicingInstrumentTypeEnum.InvoicingSettlement:
                        isInstrSpecified = invoiceStlInstrSpecified;
                        instrument = (AdministrativeInstrumentBase)invoiceStlInstr;
                        break;
                }
                if (false == isInstrSpecified)
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5225), 0,
                        new LogParam(defaultInstrumentType.ToString()), new LogParam(EntityIdentifier)));
                }
                return instrument;
            }
        }
        #endregion CurrentInstrument
        #region CS
        public string CS
        {
            get { return m_InvoicingGenProcess.ProcessBase.Cs; }
        }
        #endregion CS
        #region EntityIdentifier
        public string EntityIdentifier
        {
            get { return actorEntity.SQLActor.Identifier; }
        }
        #endregion EntityIdentifier

        #region Process
        public InvoicingGenProcessBase Process
        {
            get { return m_InvoicingGenProcess; }
        }
        #endregion Process
        #region ProcessBase
        public ProcessBase ProcessBase
        {
            get { return m_InvoicingGenProcess.ProcessBase; }
        }
        #endregion ProcessBase
        #endregion Accessors
        #region Constructors
        public InvoicingEntity(InvoicingGenProcessBase pInvoicingGenProcess, int pIdA)
        {
            m_InvoicingGenProcess = pInvoicingGenProcess;
            defaultInstrumentType = InvoicingInstrumentTypeEnum.Invoicing;
            LoadEntity(pIdA);
        }
        #endregion Constructors
        #region Methods
        #region LoadEntity
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void LoadEntity(int pIdA)
        {
            int idB;
            int idI;
            actorEntity = new InvoicingActor(CS, pIdA);
            actorEntity.SQLActor.WithInfoEntity = true;
            if (actorEntity.SQLActor.IsLoaded)
            {
                string logEntity = LogTools.IdentifierAndId(actorEntity.SQLActor.Identifier, pIdA);

                #region Administrative instruments
                idI = GetInvoicingIdIAttached(InvoicingInstrumentTypeEnum.AdditionalInvoicing);
                additionalInvoiceInstrSpecified = (0 < idI);
                if (additionalInvoiceInstrSpecified)
                {
                    additionalInvoiceInstr = new AdditionalInvoiceInstrument(ProcessBase, InvoicingInstrumentTypeEnum.AdditionalInvoicing, idI, logEntity);
                }
                else
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5210), 0,
                        new LogParam(logEntity)));
                }

                idI = GetInvoicingIdIAttached(InvoicingInstrumentTypeEnum.Invoicing);
                invoiceInstrSpecified = (0 < idI);
                if (invoiceInstrSpecified)
                {
                    invoiceInstr = new InvoiceInstrument(ProcessBase, InvoicingInstrumentTypeEnum.Invoicing, idI, logEntity);
                }
                else
                {
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5211), 0,
                        new LogParam(logEntity)));
                }

                idI = GetInvoicingIdIAttached(InvoicingInstrumentTypeEnum.CreditNote);
                creditNoteInstrSpecified = (0 < idI);
                if (creditNoteInstrSpecified)
                {
                    creditNoteInstr = new CreditNoteInstrument(ProcessBase, InvoicingInstrumentTypeEnum.CreditNote, idI, logEntity);
                }
                else
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5212), 0,
                        new LogParam(logEntity)));
                }

                idI = GetInvoicingIdIAttached(InvoicingInstrumentTypeEnum.InvoicingSettlement);
                invoiceStlInstrSpecified = (0 < idI);
                if (invoiceStlInstrSpecified)
                {
                    invoiceStlInstr = new InvoiceStlInstrument(ProcessBase, InvoicingInstrumentTypeEnum.InvoicingSettlement, idI, logEntity);
                }
                else
                {
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5213), 0, new LogParam(logEntity)));
                }

                #endregion Administrative instruments

                #region Invoicing Book
                if (null != actorEntity.SQLActor.GetFirstRowColumnValue("IDB_INVOICING"))
                {
                    idB = Convert.ToInt32(actorEntity.SQLActor.GetFirstRowColumnValue("IDB_INVOICING"));
                    bookEntity = new InvoicingBook(CS, idB);
                }
                else
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5214), 0,
                        new LogParam(logEntity)));
                }
                
                #endregion Invoicing Book
                #region Default Invoicing Application Period
                defaultApplicationPeriodSpecified = (null != actorEntity.SQLActor.GetFirstRowColumnValue("PERIOD")) ||
                                                    (null != actorEntity.SQLActor.GetFirstRowColumnValue("PERIODMLTP"));

                if (defaultApplicationPeriodSpecified)
                {
                    defaultPeriodMltp_Invoicing = Convert.ToInt32(actorEntity.SQLActor.GetFirstRowColumnValue("PERIODMLTP"));
                    defaultPeriod_Invoicing = StringToEnum.Period(actorEntity.SQLActor.GetFirstRowColumnValue("PERIOD").ToString());

                    if (null != actorEntity.SQLActor.GetFirstRowColumnValue("ROLLCONVENTION"))
                        defaultRollConvention_Invoicing = StringToEnum.RollConvention(actorEntity.SQLActor.GetFirstRowColumnValue("ROLLCONVENTION").ToString(), RollConventionEnum.EOM);
                    else
                        defaultRollConvention_Invoicing = RollConventionEnum.EOM;
                }
                #endregion Default Invoicing Application Period
            }
        }
        #endregion LoadEntity
        #region GetInvoicingIdIAttached
        private int GetInvoicingIdIAttached(InvoicingInstrumentTypeEnum pInvoicingInstrumentType)
        {
            int idI = 0;
            string columnName = string.Empty;
            switch (pInvoicingInstrumentType)
            {
                case InvoicingInstrumentTypeEnum.AdditionalInvoicing:
                    columnName = "IDI_ADDINVOICE";
                    break;
                case InvoicingInstrumentTypeEnum.CreditNote:
                    columnName = "IDI_CREDITNOTE";
                    break;
                case InvoicingInstrumentTypeEnum.Invoicing:
                    columnName = "IDI_INVOICE";
                    break;
                case InvoicingInstrumentTypeEnum.InvoicingSettlement:
                    columnName = "IDI_INVOICESTL";
                    break;
            }
            if (StrFunc.IsFilled(columnName))
            {
                bool isSpecified = (false == Convert.IsDBNull(actorEntity.SQLActor.GetFirstRowColumnValue(columnName)));
                if (isSpecified)
                    idI = Convert.ToInt32(actorEntity.SQLActor.GetFirstRowColumnValue(columnName));
            }
            return idI;
        }
        #endregion GetInvoicingIdIAttached
        #endregion Methods
    }
    #endregion InvoicingEntity
    #region InvoicingEvent
    /// <summary>
    /// Classe de travail liée à un événement de frais facturable
    /// </summary>
    public class InvoicingEvent
    {
        #region Members
        public int idT;
        public int idE;
        public int instrumentNo;
        public string eventCode;
        public string eventType;
        public DateTime eventDate;
        public int idA_Payer;
        public bool idB_PayerSpecified;
        public int idB_Payer;
        public int idA_Receiver;
        public bool idB_ReceiverSpecified;
        public int idB_Receiver;
        public decimal amount;
        public string currency;
        public decimal accountingAmount;
        public string accountingCurrency;
        // EG 20091110
        public decimal baseAmount;
        public decimal baseAccountingAmount;
        public decimal initialAmount;
        public decimal initialAccountingAmount;
        // EG 20101020 Ticket 17185
        public bool noteSpecified;
        public string note;

        public bool isAmountIsCorrected = true;

        public bool feeScheduleSpecified;
        public InvoicingFeeSchedule feeSchedule;

        public bool paymentTypeSpecified;
        public string paymentType;

        #endregion Members
        #region Constructors
        public InvoicingEvent(DataRow pRowEvent, string pIdCAccount)
        {
            idT = Convert.ToInt32(pRowEvent["IDT"]);
            idE = Convert.ToInt32(pRowEvent["IDE"]);
            instrumentNo = Convert.ToInt32(pRowEvent["INSTRUMENTNO"]);
            eventCode = pRowEvent["EVENTCODE"].ToString();
            eventType = pRowEvent["EVENTTYPE"].ToString();
            eventDate = Convert.ToDateTime(pRowEvent["DTEVENT"]);
            amount = Convert.ToDecimal(pRowEvent["VALORISATION"]);
            // EG 20091110
            baseAmount = amount;
            initialAmount = amount;
            currency = pRowEvent["IDC_FEE"].ToString();
            if (pIdCAccount == currency)
            {
                accountingCurrency = currency;
                accountingAmount = amount;
                // EG 20091110
                baseAccountingAmount = baseAmount;
                initialAccountingAmount = initialAmount;
            }
            idA_Payer = Convert.ToInt32(pRowEvent["IDA_PAY"]);
            idB_PayerSpecified = (false == Convert.IsDBNull(pRowEvent["IDB_PAY"]));
            if (idB_PayerSpecified)
                idB_Payer = Convert.ToInt32(pRowEvent["IDB_PAY"]);
            idB_Receiver = Convert.ToInt32(pRowEvent["IDA_REC"]);
            idB_ReceiverSpecified = (false == Convert.IsDBNull(pRowEvent["IDB_REC"]));
            if (idB_ReceiverSpecified)
                idB_Receiver = Convert.ToInt32(pRowEvent["IDB_REC"]);

            feeScheduleSpecified = (false == Convert.IsDBNull(pRowEvent["IDFEESCHEDULE"]));
            if (feeScheduleSpecified)
                feeSchedule = new InvoicingFeeSchedule(pRowEvent);

            paymentTypeSpecified = (false == Convert.IsDBNull(pRowEvent["PAYMENTTYPE"]));
            if (paymentTypeSpecified)
                paymentType = pRowEvent["PAYMENTTYPE"].ToString();
        }
        public InvoicingEvent(int pIdT, IInvoiceFee pFee)
        {
            idT = pIdT;
            idE = pFee.OTCmlId;
            eventType = pFee.FeeType.Value;
            amount = pFee.FeeAmount.Amount.DecValue;
            // EG 20091110
            baseAmount = pFee.FeeBaseAmount.Amount.DecValue;
            initialAmount = pFee.FeeInitialAmount.Amount.DecValue;
            currency = pFee.FeeAmount.Currency;
            if (pFee.FeeAccountingAmountSpecified)
            {
                accountingCurrency = pFee.FeeAccountingAmount.Currency;
                accountingAmount = pFee.FeeAccountingAmount.Amount.DecValue;
            }
            // EG 20091110
            if (pFee.FeeBaseAccountingAmountSpecified)
                baseAccountingAmount = pFee.FeeBaseAccountingAmount.Amount.DecValue;
            if (pFee.FeeInitialAccountingAmountSpecified)
                initialAccountingAmount = pFee.FeeInitialAccountingAmount.Amount.DecValue;
            eventDate = pFee.FeeDate.DateValue;
            idA_Payer = pFee.IdA_Pay.IntValue;
            idB_PayerSpecified = pFee.IdB_PaySpecified;
            if (idB_PayerSpecified)
                idB_Payer = pFee.IdB_Pay.IntValue;

            feeScheduleSpecified = pFee.FeeScheduleSpecified;
            if (feeScheduleSpecified)
                feeSchedule = new InvoicingFeeSchedule(pFee);
        }
        #endregion Constructors
        #region Methods
        #region SetInvoiceFee
        public Cst.ErrLevel SetInvoiceFee(IInvoiceFees pInvoiceFees, int pIndex, string pIdCAccount)
        {
            IInvoiceFee invoiceFee = pInvoiceFees.CreateInvoiceFee(idE, eventType, currency, amount, baseAmount, initialAmount, eventDate, idA_Payer, idB_Payer, pIdCAccount);
            if (pIdCAccount != currency)
            {
                invoiceFee.FeeInitialAccountingAmountSpecified = (0 != initialAccountingAmount);
                invoiceFee.FeeInitialAccountingAmount = pInvoiceFees.CreateMoney(initialAccountingAmount, pIdCAccount);
            }
            invoiceFee.FeeScheduleSpecified = feeScheduleSpecified;
            if (invoiceFee.FeeScheduleSpecified)
            {
                invoiceFee.FeeSchedule = pInvoiceFees.CreateInvoiceFeeSchedule();
                if (feeSchedule.idFeeScheduleSpecified)
                    invoiceFee.FeeSchedule.OTCmlId = feeSchedule.idFeeSchedule;
                invoiceFee.FeeSchedule.IdentifierSpecified = feeSchedule.identifierSpecified;
                if (feeSchedule.identifierSpecified)
                    invoiceFee.FeeSchedule.Identifier = new EFS_String(feeSchedule.identifier);
                invoiceFee.FeeSchedule.FormulaDCFSpecified = feeSchedule.formulaDCFSpecified;
                if (feeSchedule.formulaDCFSpecified)
                    invoiceFee.FeeSchedule.FormulaDCF = new EFS_String(feeSchedule.formulaDCF);
                invoiceFee.FeeSchedule.DurationSpecified = feeSchedule.durationSpecified;
                if (feeSchedule.durationSpecified)
                    invoiceFee.FeeSchedule.Duration = new EFS_String(feeSchedule.duration);
                //PL 20141023
                //invoiceFee.feeSchedule.assessmentBasisValueSpecified = feeSchedule.assessmentBasisValueSpecified;
                //if (feeSchedule.assessmentBasisValueSpecified)
                //    invoiceFee.feeSchedule.assessmentBasisValue = new EFS_Decimal(feeSchedule.assessmentBasisValue);
                invoiceFee.FeeSchedule.AssessmentBasisValue1Specified = feeSchedule.assessmentBasisValue1Specified;
                if (feeSchedule.assessmentBasisValue1Specified)
                    invoiceFee.FeeSchedule.AssessmentBasisValue1 = new EFS_Decimal(feeSchedule.assessmentBasisValue1);
                invoiceFee.FeeSchedule.AssessmentBasisValue2Specified = feeSchedule.assessmentBasisValue2Specified;
                if (feeSchedule.assessmentBasisValue2Specified)
                    invoiceFee.FeeSchedule.AssessmentBasisValue2 = new EFS_Decimal(feeSchedule.assessmentBasisValue2);
            }
            pInvoiceFees.SetInvoiceFee(pIndex, invoiceFee);
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetInvoiceFee
        #endregion Methods
    }
    #endregion InvoicingEvent
    #region InvoicingFeeSchedule
    /// <summary>
    /// Classe pour le schedule de frais d'un événement facturable
    /// </summary>
    public class InvoicingFeeSchedule
    {
        #region Members
        public bool idFeeScheduleSpecified;
        public int idFeeSchedule;
        public bool identifierSpecified;
        public string identifier;
        public bool formulaDCFSpecified;
        public string formulaDCF;
        public bool durationSpecified;
        public string duration;
        //PL 20141023
        //public bool assessmentBasisValueSpecified;
        //public decimal assessmentBasisValue;
        public bool assessmentBasisValue1Specified;
        public decimal assessmentBasisValue1;
        public bool assessmentBasisValue2Specified;
        public decimal assessmentBasisValue2;
        #endregion Members
        #region Constructors
        public InvoicingFeeSchedule(DataRow pRowEvent)
        {
            idFeeScheduleSpecified = (false == Convert.IsDBNull(pRowEvent["IDFEESCHEDULE"]));
            if (idFeeScheduleSpecified)
                idFeeSchedule = Convert.ToInt32(pRowEvent["IDFEESCHEDULE"]);
            identifierSpecified = (false == Convert.IsDBNull(pRowEvent["FEESCHEDULE_IDENTIFIER"]));
            if (identifierSpecified)
                identifier = pRowEvent["FEESCHEDULE_IDENTIFIER"].ToString();
            formulaDCFSpecified = (false == Convert.IsDBNull(pRowEvent["FORMULADCF"]));
            if (formulaDCFSpecified)
                formulaDCF = pRowEvent["FORMULADCF"].ToString();
            durationSpecified = formulaDCFSpecified;
            if (durationSpecified)
                duration = formulaDCF;
            //PL 20141023
            //assessmentBasisValueSpecified = (false == Convert.IsDBNull(pRowEvent["ASSESSMENTBASISVALUE"]));
            //if (assessmentBasisValueSpecified)
            //    assessmentBasisValue = Convert.ToDecimal(pRowEvent["ASSESSMENTBASISVALUE"]);
            assessmentBasisValue1Specified = (false == Convert.IsDBNull(pRowEvent["ASSESSMENTBASISVALUE1"]));
            if (assessmentBasisValue1Specified)
                assessmentBasisValue1 = Convert.ToDecimal(pRowEvent["ASSESSMENTBASISVALUE1"]);
            assessmentBasisValue2Specified = (false == Convert.IsDBNull(pRowEvent["ASSESSMENTBASISVALUE2"]));
            if (assessmentBasisValue2Specified)
                assessmentBasisValue2 = Convert.ToDecimal(pRowEvent["ASSESSMENTBASISVALUE2"]);
        }
        public InvoicingFeeSchedule(IInvoiceFee pFee)
        {
            IInvoiceFeeSchedule feeSchedule = pFee.FeeSchedule;
            idFeeScheduleSpecified = (0 < feeSchedule.OTCmlId);
            if (idFeeScheduleSpecified)
                idFeeSchedule = feeSchedule.OTCmlId;
            identifierSpecified = feeSchedule.IdentifierSpecified;
            if (identifierSpecified)
                identifier = feeSchedule.Identifier.Value;
            formulaDCFSpecified = feeSchedule.FormulaDCFSpecified;
            if (formulaDCFSpecified)
                formulaDCF = feeSchedule.FormulaDCF.Value;
            durationSpecified = feeSchedule.DurationSpecified;
            if (durationSpecified)
                duration = feeSchedule.Duration.Value;
            //PL 20141023
            //assessmentBasisValueSpecified = feeSchedule.assessmentBasisValueSpecified;
            //if (assessmentBasisValueSpecified)
            //    assessmentBasisValue = feeSchedule.assessmentBasisValue.DecValue;
            assessmentBasisValue1Specified = feeSchedule.AssessmentBasisValue1Specified;
            if (assessmentBasisValue1Specified)
                assessmentBasisValue1 = feeSchedule.AssessmentBasisValue1.DecValue;
            assessmentBasisValue2Specified = feeSchedule.AssessmentBasisValue2Specified;
            if (assessmentBasisValue2Specified)
                assessmentBasisValue2 = feeSchedule.AssessmentBasisValue2.DecValue;
        }
        #endregion Constructors
    }
    #endregion InvoicingFeeSchedule
    #region InvoiceInstrument
    /// <summary>
    /// Classe de travail matérialisant un instrument de type : Facture
    /// </summary>
    public class InvoiceInstrument : AdministrativeInstrumentBase
    {
        #region Constructors
        public InvoiceInstrument(ProcessBase pProcess, InvoicingInstrumentTypeEnum pInvoicingInstrumentType, int pIdI, string pLogEntity)
            : base(pProcess, pInvoicingInstrumentType, pIdI, pLogEntity) { }
        #endregion Constructors
    }
    #endregion InvoiceInstrument
    #region InvoicingParameters
    /// <summary>
    /// Liste des paramètres pour déterminer les événements candidats et le mode de facturation
    /// - Date
    /// - Entité
    /// - Mode simulation ou non
    /// - Critères de filtre
    /// </summary>
    public class InvoicingParameters
    {
        #region Members
        public DateTime invoiceDate;
        public int idA_Entity;
        public bool isSimul;
        public bool criteriaSpecified;
        public SQL_Criteria criteria;
        #endregion Members
        #region Accessors
        #region IdA_PayerSpecified
        public bool IdA_PayerSpecified
        {
            get
            {
                bool isSpecified = false;
                if (criteriaSpecified)
                    isSpecified = (null != criteria["IDA_PAY"]);
                return isSpecified;
            }
        }
        #endregion IdA_PayerSpecified
        #region IdC_FeeSpecified
        public bool IdC_FeeSpecified
        {
            get
            {
                bool isSpecified = false;
                if (criteriaSpecified)
                    isSpecified = (null != criteria["IDC_FEE"]);
                return isSpecified;
            }
        }
        #endregion IdC_FeeSpecified
        #endregion Accessors
        #region Constructors
        public InvoicingParameters(DateTime pInvoiceDate, int pIdA_Entity, bool pIsSimul, SQL_Criteria pCriteria)
        {
            invoiceDate = pInvoiceDate;
            idA_Entity = pIdA_Entity;
            isSimul = pIsSimul;
            criteriaSpecified = (null != pCriteria);
            criteria = pCriteria;
        }
        #endregion Constructors
        #region Methods
        #region GetSQLWhere
        public string GetSQLWhere(string pConnectionString, string pAliasTable)
        {
            string SQLWhere = string.Empty;
            if (criteriaSpecified)
                SQLWhere = criteria.GetSQLWhere(pConnectionString, pAliasTable);
            return SQLWhere;
        }
        #endregion GetSQLWhere
        #endregion Methods
    }
    #endregion InvoicingParameters
    #region InvoicingRules
    /// <summary>
    /// Règles de facturation :
    /// - les acteurs liés à la facturation
    /// - Dates
    /// - Contexte
    /// - Règles de facturation
    /// - Taxes
    /// - Conditions de facturation
    /// - Trades (Frais) rattachés en fonction de la détermination du poids de contexte
    /// - Trades en mode simulation rattachés en fonction de la détermination du poids de contexte
    /// </summary>
    public class InvoicingRules
    {
        #region Members
        public bool addressIdentSpecified;
        public string addressIdent;
        public string idC_Invoicing;
        public bool idAsset_InvoicingSpecified;
        public int idAsset_Invoicing;
        public bool periodMltpOffset_InvoicingSpecified;
        public int periodMltpOffset_Invoicing;
        public bool periodOffset_InvoicingSpecified;
        public PeriodEnum periodOffset_Invoicing;
        public bool dayTypeOffset_InvoicingSpecified;
        public DayTypeEnum dayTypeOffset_Invoicing;
        public bool periodMltp_InvoicingSpecified;
        public int periodMltp_Invoicing;
        public bool period_InvoicingSpecified;
        public PeriodEnum period_Invoicing;
        public bool rollConvention_InvoicingSpecified;
        public RollConventionEnum rollConvention_Invoicing;
        public InvoiceSettlementDelayRelativeToEnum relativeTo_StlDelay;
        public int periodMltp_StlDelay;
        public PeriodEnum period_StlDelay;
        public BusinessDayConventionEnum bdc_StlDelay;
        public DayTypeEnum dayType_StlDelay;
        #endregion Members
        #region Constructors
        public InvoicingRules(InvoicingEntity pEntity, DataRow pRowInvoicingRule)
        {
            #region Devise de facturation
            if (Convert.IsDBNull(pRowInvoicingRule["IDC_INVOICING"]))
                idC_Invoicing = pRowInvoicingRule["IDC_FEE"].ToString();
            else
            {
                idC_Invoicing = pRowInvoicingRule["IDC_INVOICING"].ToString();
                // Asset et Offset si Devise de facturation spécifiée
                idAsset_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["IDASSET_FXRATE_INV"]));
                if (idAsset_InvoicingSpecified)
                    idAsset_Invoicing = Convert.ToInt32(pRowInvoicingRule["IDASSET_FXRATE_INV"]);
                periodMltpOffset_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["PERIODMLTPOFFSET"]));
                if (periodMltpOffset_InvoicingSpecified)
                    periodMltpOffset_Invoicing = Convert.ToInt32(pRowInvoicingRule["PERIODMLTPOFFSET"]);
                periodOffset_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["PERIODOFFSET"]));
                if (periodOffset_InvoicingSpecified)
                    periodOffset_Invoicing = StringToEnum.Period(pRowInvoicingRule["PERIODOFFSET"].ToString());

                dayTypeOffset_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["DAYTYPEOFFSET"]));
                if (dayTypeOffset_InvoicingSpecified)
                    dayTypeOffset_Invoicing = StringToEnum.DayType(pRowInvoicingRule["DAYTYPEOFFSET"].ToString());
            }
            #endregion Devise de facturation
            #region Adresse de l'acteur facturé
            addressIdentSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["ADDRESSIDENT"]));
            if (addressIdentSpecified)
                addressIdent = pRowInvoicingRule["ADDRESSIDENT"].ToString();
            #endregion Adresse de l'acteur facturé
            #region Période de facturation
            periodMltp_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["PERIODMLTP"]));
            period_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["PERIOD"]));
            rollConvention_InvoicingSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["ROLLCONVENTION"]));

            if (periodMltp_InvoicingSpecified && period_InvoicingSpecified && rollConvention_InvoicingSpecified)
            {
                periodMltp_Invoicing = Convert.ToInt32(pRowInvoicingRule["PERIODMLTP"]);
                period_Invoicing = StringToEnum.Period(pRowInvoicingRule["PERIOD"].ToString());
                rollConvention_Invoicing = StringToEnum.RollConvention(pRowInvoicingRule["ROLLCONVENTION"].ToString());
            }
            else if (pEntity.defaultApplicationPeriodSpecified)
            {
                periodMltp_Invoicing = pEntity.defaultPeriodMltp_Invoicing;
                period_Invoicing = pEntity.defaultPeriod_Invoicing;
                rollConvention_Invoicing = pEntity.defaultRollConvention_Invoicing;
            }
            #endregion Période de facturation
            #region Délai de paiement
            periodMltp_StlDelay = Convert.ToInt32(pRowInvoicingRule["PERIODMLTPSTLDELAY"]);
            period_StlDelay = StringToEnum.Period(pRowInvoicingRule["PERIODSTLDELAY"].ToString());
            dayType_StlDelay = StringToEnum.DayType(pRowInvoicingRule["DAYTYPESTLDELAY"].ToString());
            bdc_StlDelay = StringToEnum.BusinessDayConvention(pRowInvoicingRule["BDC_STLDELAY"].ToString());
            relativeTo_StlDelay = (InvoiceSettlementDelayRelativeToEnum)StringToEnum.Parse(pRowInvoicingRule["RELATIVESTLDELAY"].ToString(),
                                   InvoiceSettlementDelayRelativeToEnum.invoiceDate);
            #endregion Délai de paiement
        }
        #endregion Constructors
    }
    #endregion InvoicingRules
    #region InvoicingScope
    /// <summary>
    /// Classe principale qui contient pour un périmètre de facturation :
    /// - les acteurs liés à la facturation
    /// - Dates
    /// - Contexte
    /// - Règles de facturation
    /// - Taxes
    /// - Conditions de facturation
    /// - Trades (Frais) rattachés en fonction de la détermination du poids de contexte
    /// - Trades en mode simulation rattachés en fonction de la détermination du poids de contexte
    /// </summary>
    // EG 20141020 [20442] Add ActorBeneficiary
    public class InvoicingScope
    {
        #region Members
        private readonly InvoicingGenProcessBase m_InvoicingGenProcess;

        public InvoicingActor actorPayer;
        public InvoicingDates invoicingPeriodDates;
        public int idInvoicingRules;
        public string identifier;
        public string displayName;
        public InvoicingActor actorInvoiced;
        public InvoicingBeneficiary actorBeneficiary;
        public InvoicingContext context;
        public InvoicingRules rules;
        public InvoicingTax tax;
        public InvoicingConditions conditions;
        public InvoicingTrade[] trades;
        public InvoicingTradeSimul[] tradeSimulToDelete;

        private LockObject m_LockObject;
        private double m_ResultMatching;
        private DataDocumentContainer m_DataDocument;
        private ProductContainer m_ProductContainer;
        //private EFS_TradeLibrary m_TradeLibrary;
        private readonly int m_IdA_Entity;
        private readonly int m_IdB_Entity;
        private readonly string m_IdCAccount;
        #region Trade admin results
        public string tradeIdentifier;
        public int idT;
        
        // EG 20091105
        public bool isInvoicingEventIsCorrected;
        #endregion Trade admin results

        #endregion Members
        #region Accessors
        #region CS
        public string CS
        {
            get { return m_InvoicingGenProcess.ProcessBase.Cs; }
        }
        #endregion CS
        #region InvoicedIdentifier
        public string InvoicedIdentifier
        {
            get { return actorInvoiced.SQLActor.Identifier; }
        }
        #endregion InvoicedIdentifier
        #region DataDocument
        public DataDocumentContainer DataDocument
        {
            get { return m_DataDocument; }
            set { m_DataDocument = value; }
        }
        #endregion DataDocument
        #region ProcessBase
        private ProcessBase ProcessBase
        {
            get { return m_InvoicingGenProcess.ProcessBase; }
        }
        #endregion ProcessBase
        #region ProductBase
        private IProductBase ProductBase
        {
            get { return m_DataDocument.CurrentProduct.Product.ProductBase; }
        }
        #endregion ProductBase
        #region ResultMatching
        public double ResultMatching
        {
            set { m_ResultMatching = value; }
            get { return m_ResultMatching; }
        }
        #endregion ResultMatching
        #endregion Accessors
        #region Constructors
        public InvoicingScope(InvoicingEntity pEntity, DataRow pRowInvoicingRule, DataRelation pBracketByInvoicingRules)
        {
            m_IdA_Entity = pEntity.actorEntity.SQLActor.Id;
            m_IdB_Entity = pEntity.bookEntity.SQLBook.Id;
            m_IdCAccount = pEntity.actorEntity.SQLActor.IdCAccount;
            m_InvoicingGenProcess = pEntity.Process;
            


            idInvoicingRules = Convert.ToInt32(pRowInvoicingRule["IDINVOICINGRULES"]);
            identifier = pRowInvoicingRule["IDENTIFIER"].ToString();
            displayName = pRowInvoicingRule["DISPLAYNAME"].ToString();
            actorPayer = new InvoicingActor(CS, Convert.ToInt32(pRowInvoicingRule["IDA"]));
            // EG 20141020 [20442] 
            //actorPayer.SQLActor = null;
            actorInvoiced = new InvoicingActor(CS, Convert.ToInt32(pRowInvoicingRule["IDA_INVOICED"]));
            // EG 20141020 [20442]
            //actorInvoiced.SQLActor = null;
            context = new InvoicingContext(pRowInvoicingRule);
            conditions = new InvoicingConditions(pRowInvoicingRule);
            tax = new InvoicingTax(CS, pRowInvoicingRule);
            rules = new InvoicingRules(pEntity, pRowInvoicingRule);
            DataRow[] rowInvRulesBrackets = pRowInvoicingRule.GetChildRows(pBracketByInvoicingRules);
            conditions.brackets = new InvoicingBracket[rowInvRulesBrackets.Length];
            for (int i = 0; i < rowInvRulesBrackets.Length; i++)
            {
                conditions.brackets[i] = new InvoicingBracket(idInvoicingRules, rowInvRulesBrackets[i]);
            }
        }
        #endregion Constructors
        #region Indexors
        public InvoicingTrade this[int pIdT]
        {
            get
            {
                if (null != trades)
                {
                    foreach (InvoicingTrade item in trades)
                    {
                        if (item.idT == pIdT)
                            return item;
                    }
                }
                return null;
            }
        }
        public InvoicingTrade this[string pIdentifier]
        {
            get
            {
                if (null != trades)
                {
                    foreach (InvoicingTrade item in trades)
                    {
                        if (item.tradeIdentifier == pIdentifier)
                            return item;
                    }
                }
                return null;
            }
        }
        public InvoicingTradeSimul this[int pIdT, bool pUnused]
        {
            get
            {
                if (null != tradeSimulToDelete)
                {
                    foreach (InvoicingTradeSimul item in tradeSimulToDelete)
                    {
                        if (item.idT == pIdT)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #region AddEvent
        // FI 20091021 [add sales in invoice] Add pSalesActorByTrade
        // EG 20090108 Add lastError setting where Cst.ErrLevel.LOCKUNSUCCESSFUL == codeReturn
        // EG 20141020 [20442] ActorBeneficiary
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20230526 [WI640] Add Mise en cache
        public Cst.ErrLevel AddEvent(DataRow pRowEvent, InvoicingContextEvent pInvoicingContextEvent, DataRelation pTraderActorByTrade, DataRelation pSalesActorByTrade)
        {
            string cs = CSTools.SetCacheOn(CS);
            int idT = Convert.ToInt32(pRowEvent["IDT"]);
            string identifier = pRowEvent["TRADE_IDENTIFIER"].ToString();

            Cst.ErrLevel codeReturn = AddTradeSimulToDelete(pRowEvent);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                InvoicingTrade trade = this[idT];
                if (null == trade)
                {
                    #region Ajout du trade
                    ArrayList aTrades = new ArrayList();
                    if (ArrFunc.IsFilled(trades))
                        aTrades.AddRange(trades);
                    InvoicingTrade invoicingTrade = new InvoicingTrade(cs, ProcessBase, pRowEvent, pInvoicingContextEvent, pTraderActorByTrade, pSalesActorByTrade);
                    codeReturn = invoicingTrade.ErrLevel;
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                    {
                        aTrades.Add(invoicingTrade);
                        if (0 < aTrades.Count)
                            trades = (InvoicingTrade[])aTrades.ToArray(typeof(InvoicingTrade));
                        trade = this[idT];
                        #region Lock du trade
                        codeReturn = LockTrade(idT, identifier);
                        if (Cst.ErrLevel.LOCKUNSUCCESSFUL == codeReturn)
                        {
                            trade.lastError = codeReturn;

                            // FI 20200623 [XXXXX] SetErrorWarning
                            ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5203), 0,
                                new LogParam(LogTools.IdentifierAndId(pRowEvent["TRADE_IDENTIFIER"].ToString(), idT)),
                                new LogParam(LogTools.IdentifierAndId(pRowEvent["INSTR_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDI"]))),
                                new LogParam(LogTools.IdentifierAndId(pRowEvent["PAYER_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDA_PAY"]))),
                                new LogParam(LogTools.IdentifierAndId(pRowEvent["EVENTCODE"].ToString() + "-" + pRowEvent["EVENTTYPE"].ToString(), Convert.ToInt32(pRowEvent["IDE"]))),
                                new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(pRowEvent["VALORISATION"])) + " " + pRowEvent["IDC_FEE"].ToString())));
                        }
                        #endregion Lock du trade
                    }
                    #endregion Ajout du trade
                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    #region Ajout du bénéficiaire de la facture si <> de l'entité
                    // EG 20141020 [20442] New
                    if ((null == actorBeneficiary) && (m_IdA_Entity == actorInvoiced.Id))
                    {
                        int idA_REC = Convert.ToInt32(pRowEvent["IDA_REC"]);
                        int idB_REC = Convert.ToInt32(pRowEvent["IDB_REC"]);
                        actorBeneficiary = new InvoicingBeneficiary(cs, idA_REC, idB_REC);
                    }
                    #endregion Ajout du bénéficiaire de la facture
                    #region Ajout de l'événement pour le trade concerné
                    // EG 20121205 Test Existence de l'événement sur le trade
                    int idE = Convert.ToInt32(pRowEvent["IDE"]);
                    InvoicingEvent @event = trade[idE];
                    if (null == @event)
                    {
                        ArrayList aEvents = new ArrayList();
                        if (ArrFunc.IsFilled(trade.events))
                            aEvents.AddRange(trade.events);
                        aEvents.Add(new InvoicingEvent(pRowEvent, m_IdCAccount));
                        if (0 < aEvents.Count)
                            trade.events = (InvoicingEvent[])aEvents.ToArray(typeof(InvoicingEvent));
                    }
                    #endregion Ajout de l'événement pour le trade concerné
                }
            }
            return codeReturn;
        }
        #endregion AddEvent
        #region AddTradeSimulToDelete
        // EG 20110504 Changement appel constructeur de InvoicingTradeSimul
        public Cst.ErrLevel AddTradeSimulToDelete(DataRow pRowEvent)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isMustBeDeleted = (false == Convert.IsDBNull(pRowEvent["IDSTENVIRONMENT_INVOICE"])) &&
                                   (Cst.StatusEnvironment.SIMUL.ToString() == pRowEvent["IDSTENVIRONMENT_INVOICE"].ToString());
            if (isMustBeDeleted)
            {
                int idT = Convert.ToInt32(pRowEvent["IDT_INVOICE"]);
                InvoicingTradeSimul trade = this[idT, true];
                if (null == trade)
                {
                    #region Ajout du trade
                    ArrayList aTradeSimulToDelete = new ArrayList();
                    if (ArrFunc.IsFilled(tradeSimulToDelete))
                        aTradeSimulToDelete.AddRange(tradeSimulToDelete);
                    InvoicingTradeSimul simul = new InvoicingTradeSimul(CS, idT);
                    aTradeSimulToDelete.Add(simul);
                    if (0 < aTradeSimulToDelete.Count)
                        tradeSimulToDelete = (InvoicingTradeSimul[])aTradeSimulToDelete.ToArray(typeof(InvoicingTradeSimul));
                    #endregion Ajout du trade
                    #region Lock du trade
                    codeReturn = LockTrade(idT, simul.tradeIdentifier);
                    #endregion Lock du trade
                }
            }
            return codeReturn;
        }
        #endregion AddTradeSimulToDelete
        #region CalculGrossTurnOverAmount
        // EG 20091207 Round
        public IMoney CalculGrossTurnOverAmount()
        {
            decimal amount = 0;
            foreach (InvoicingTrade trade in trades)
            {
                foreach (InvoicingEvent eventFee in trade.events)
                {
                    amount += eventFee.amount;
                }
            }
            // EG 20091207 Round
            EFS_Cash cash = new EFS_Cash(CS, amount, this.context.idC_Fee);
            return ProductBase.CreateMoney(cash.AmountRounded, this.context.idC_Fee);
        }
        #endregion CalculGrossTurnOverAmount
        #region CalculNetTurnOverIssueRate
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CalculNetTurnOverIssueRate()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            SystemMSGInfo systemMsgInfo = null;
            try
            {
                IInvoice invoice = (IInvoice)m_DataDocument.CurrentProduct.Product;
                IInvoiceRateSource source = invoice.InvoiceRateSource;
                #region KeyQuote
                DateTime offsetDate = DtFunc.AddTimeToDate(source.AdjustedFixingDate.DateValue, source.FixingTime.HourMinuteTime.TimeValue);
                // EG 20150706 [21021]
                KeyQuote keyQuote = new KeyQuote(CS, offsetDate, actorInvoiced.Id, null, m_IdA_Entity, m_IdB_Entity, QuoteTimingEnum.Close);
                #endregion KeyQuote
                SQL_Quote quote = CommonValFunc.ReadQuote_AssetByType(CS, QuoteEnum.FXRATE, m_DataDocument.CurrentProduct.ProductBase, keyQuote, source.RateSource.OTCmlId, ref systemMsgInfo);
                decimal issueRate = quote.QuoteValue;
                invoice.NetTurnOverIssueRateSpecified = (0 < issueRate);
                invoice.NetTurnOverIssueRate = new EFS_Decimal(issueRate);
            }
            catch (SpheresException2 ex)
            {
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    if (ProcessStateTools.IsStatusError(systemMsgInfo.processState.Status))
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        Logger.Log(systemMsgInfo.ToLoggerData(0));
                        codeReturn = ex.ProcessState.CodeReturn;
                    }
                }
                else
                    throw;
            }
            catch (Exception) { throw; }
            return codeReturn;
        }
        #endregion GetNetTurnOverIssueRate
        #region DeleteTradeSimul
        // EG 20100823 Delete (Error FK : inversion de l'ordre des 2 instructions delete)
        /// EG 20150115 [20683]
        // EG 20171121 Use ProcessBase.cs
        // EG 20190114 Add detail to ProcessLog Refactoring
        public TradeCommonCaptureGen.ErrorLevel DeleteTradeSimul(IDbTransaction pDbTransaction)
        {
            TradeCommonCaptureGen.ErrorLevel codeReturn = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            string methodName = "InvoicingScope.DeleteTradeSimul";
            try
            {
                if (ArrFunc.IsFilled(tradeSimulToDelete))
                {
                    ProcessState processState = null;
                    string sqlWhere = SQLCst.WHERE + "IDT=@IDT" + Cst.CrLf;
                    string SQLDelete = SQLCst.SQL_ANSI + Cst.CrLf;
                    SQLDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADE + Cst.CrLf;
                    SQLDelete += sqlWhere + ";" + Cst.CrLf;
                    foreach (InvoicingTradeSimul trade in tradeSimulToDelete)
                    {
                        // EG 20150115 [20683]
                        // FI 20160816 [22146] passage des paramètres idA, pDateSys
                        //string cs = pDbTransaction.Connection.ConnectionString;
                        // FI 20200820 [25468] dates systemes en UTC
                        TradeRDBMSTools.DeleteEvent(ProcessBase.Cs, pDbTransaction, trade.idT, Cst.ProductGProduct_ADM, ProcessBase.Session.IdA, OTCmlHelper.GetDateSysUTC(ProcessBase.Cs));
                        DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, SQLDelete.Replace("@IDT", trade.idT.ToString()));
                        processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, ProcessStateTools.CodeReturnSuccessEnum);

                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5231), 1,
                            new LogParam(LogTools.IdentifierAndId(trade.tradeIdentifier, trade.idT))));
                    }
                }
            }
            catch (TradeCommonCaptureGenException) { throw; }
            catch (Exception ex) { throw new TradeCommonCaptureGenException(methodName, ex, TradeCommonCaptureGen.ErrorLevel.REMOVEINVOICESIMUL_ERROR); }
            return codeReturn;
        }
        #endregion DeleteTradeSimul
        #region GetPreviousInvoicingAmount
        // EG 20110830 Xquery via XQueryTransform
        private Cst.ErrLevel GetPreviousInvoicingAmount(InvoicingEntity pInvoicingEntity, InvoicingDates pInvoicingDates, out Nullable<decimal> pPreviousGrossTurnOverAmount, out Nullable<decimal> pPreviousNetTurnOverAmount)
        {
            StrBuilder sqlSelect = new StrBuilder();
            decimal grossTurnOverAmount = 0;
            decimal netTurnOverAmount = 0;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDINVOICINGRULES", DbType.Int32), idInvoicingRules);
            parameters.Add(new DataParameter(CS, "IDI_INVOICE", DbType.Int32), pInvoicingEntity.invoiceInstr.SQLInstrument.Id);
            parameters.Add(new DataParameter(CS, "IDI_ADDINVOICE", DbType.Int32), pInvoicingEntity.additionalInvoiceInstr.SQLInstrument.Id);
            parameters.Add(new DataParameter(CS, "IDI_CREDITNOTE", DbType.Int32), pInvoicingEntity.creditNoteInstr.SQLInstrument.Id);
            parameters.Add(new DataParameter(CS, "DTSTARTPERIOD", DbType.Date), pInvoicingDates.startPeriod); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "DTENDPERIOD", DbType.Date), pInvoicingDates.invoiceDate.AddDays(-1)); // FI 20201006 [XXXXX] DbType.Date
            sqlSelect += SQLCst.SELECT + Cst.CrLf;
            sqlSelect += "SUM(ev.VALORISATION) as AMOUNT,ev.EVENTTYPE,tr.IDI" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " ev " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_TRADE.ToString() + " tr " + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(tr.IDT = e.IDT)" + SQLCst.AND + "(tr.IDI In (@IDI_INVOICE,@IDI_ADDINVOICE,@IDI_CREDITNOTE))" + Cst.CrLf;
            // EG 20120516 Gestion column IDINVOICINGRULES dans TRADE
            sqlSelect += SQLCst.AND + "(tr.IDINVOICINGRULES = @IDINVOICINGRULES)" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "(ev.EVENTTYPE In (";
            sqlSelect += DataHelper.SQLString(EventTypeFunc.GrossTurnOverAmount) + "," + DataHelper.SQLString(EventTypeFunc.NetTurnOverAmount) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "ev.DTENDADJ" + SQLCst.BETWEEN + "@DTSTARTPERIOD" + SQLCst.AND + "@DTINVOICE" + Cst.CrLf;
            // EG 20120516 Gestion column IDINVOICINGRULES dans TRADE
            //sqlSelect += SQLCst.AND + "TRADEXML.exist('efs:EfsML/fpml:trade/efs:invoice/efs:scope[@OTCmlId=@IDINVOICINGRULES]')=1" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "ev.EVENTTYPE,tr.IDI";

            string sqlQuery = DataHelper<string>.XQueryTransform(CS, CommandType.Text, sqlSelect.ToString());
            DataSet dsPreviousAmount = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            if ((null != dsPreviousAmount) || (0 < dsPreviousAmount.Tables[0].Rows.Count))
            {
                foreach (DataRow row in dsPreviousAmount.Tables[0].Rows)
                {
                    string eventType = row["EVENTTYPE"].ToString();
                    bool isCreditNoteAmount = (pInvoicingEntity.creditNoteInstr.SQLInstrument.Id == Convert.ToInt32(row["IDI"]));
                    decimal amount = Convert.ToDecimal(row["AMOUNT"]) * (isCreditNoteAmount ? -1 : 1);

                    if (EventTypeFunc.IsGrossTurnOverAmount(eventType))
                        grossTurnOverAmount += amount;
                    else if (EventTypeFunc.IsNetTurnOverAmount(eventType))
                        netTurnOverAmount += amount;
                }
            }
            pPreviousGrossTurnOverAmount = grossTurnOverAmount;
            pPreviousNetTurnOverAmount = netTurnOverAmount;
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion GetPreviousInvoicingAmount
        #region InvoiceAmountsCalculation
        /// <summary>
        /// Calcul des montants de facturation
        /// - Montant brut (GTO)
        /// - Montant de remise total (Plafond + Tranche)
        /// - Taxes
        /// - Montants Nets de facture (NTO, NTI et NTA)
        /// </summary>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel InvoiceAmountsCalculation(InvoicingEntity pInvoicingEntity)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            decimal netTurnOverIssueAmount = 0;
            decimal? previousGrossTurnOverAmount = null;
            decimal? previousNetTurnOverAmount = null;
            decimal? netTurnOverInExcessAmount = null;
            decimal? totalRebateBracketAmount = null;
            decimal? rebateAmount = null;
            decimal taxAmount = 0;
            decimal taxIssueAmount = 0;


            IInvoice invoice = (IInvoice)m_DataDocument.CurrentProduct.Product;
            #region Fee CurrencyCashInfo
            CurrencyCashInfo currencyCashInfo = new CurrencyCashInfo(CS, context.idC_Fee);
            #endregion Fee CurrencyCashInfo
            #region GrossTurnOverAmount
            invoice.GrossTurnOverAmount = CalculGrossTurnOverAmount();
            invoice.GrossTurnOverAmountSpecified = true;
            decimal grossTurnOverAmount = invoice.GrossTurnOverAmount.Amount.DecValue;
            #endregion GrossTurnOverAmount
            #region Rebates
            if (invoice.RebateConditionsSpecified)
            {
                #region BracketConditions
                if (invoice.RebateConditions.BracketConditionsSpecified)
                {
                    IRebateBracketConditions bracketConditions = invoice.RebateConditions.BracketConditions;
                    IRebateBracketParameters parameters = bracketConditions.Parameters;
                    IRebateBracketResult result = bracketConditions.Result;
                    if (null == conditions.bracketPeriodDates)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.SYS, 5221), 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, idInvoicingRules)),
                            new LogParam(LogTools.IdentifierAndId(actorInvoiced.Identifier, actorInvoiced.Id))));

                        codeReturn = Cst.ErrLevel.DATANOTFOUND;
                    }
                    else
                    {
                        if (conditions.bracketPeriodDates.IsInvoicePeriodApplication ||
                            conditions.bracketPeriodDates.IsEqualToInvoicingApplicationPeriod(invoicingPeriodDates.applicationPeriod))
                        {
                            // Si la période d'application de la REMISE sur TRANCHE
                            // = Facture ou = périodicité de FACTURATION du périmètre en cours
                            totalRebateBracketAmount = result.Calculations.CalculRebateBracketAmount(CS, parameters.ManagementType, grossTurnOverAmount, currencyCashInfo);
                        }
                        else if (false == conditions.bracketPeriodDates.IsInvoiceDateEqualToEndPeriod)
                        {
                            // La date de facturation ne coîncide pas avec la date de fin de période d'application de la REMISE sur TRANCHE
                            // Ne rien faire
                        }
                        else
                        {
                            // Recherche de toutes les factures précédentes dans la période d'application de la REMISE sur TRANCHE
                            // pour récupérer la somme des montants bruts déjà facturés
                            codeReturn = GetPreviousInvoicingAmount(pInvoicingEntity, conditions.bracketPeriodDates, out previousGrossTurnOverAmount, out previousNetTurnOverAmount);
                            totalRebateBracketAmount = result.Calculations.CalculRebateBracketAmount(CS, parameters.ManagementType, grossTurnOverAmount + previousGrossTurnOverAmount.Value, currencyCashInfo);
                        }
                        // EG 20100217 (0 <= totalRebateBracketAmount.Value) replace (0 < totalRebateBracketAmount.Value)
                        result.TotalRebateBracketAmountSpecified = totalRebateBracketAmount.HasValue && (0 <= totalRebateBracketAmount.Value);
                        if (result.TotalRebateBracketAmountSpecified)
                            result.TotalRebateBracketAmount = ProductBase.CreateMoney(totalRebateBracketAmount.Value, context.idC_Fee);
                        result.SumOfGrossTurnOverPreviousPeriodAmountSpecified = previousGrossTurnOverAmount.HasValue;
                        if (result.SumOfGrossTurnOverPreviousPeriodAmountSpecified)
                            result.SumOfGrossTurnOverPreviousPeriodAmount = ProductBase.CreateMoney(previousGrossTurnOverAmount.Value, context.idC_Fee);
                    }
                }
                #endregion BracketConditions
                #region CapConditions
                if (invoice.RebateConditions.CapConditionsSpecified)
                {
                    IRebateCapConditions capConditions = invoice.RebateConditions.CapConditions;
                    IRebateCapParameters parameters = capConditions.Parameters;
                    IRebateCapResult result = capConditions.Result;
                    if (conditions.capPeriodDates.IsInvoicePeriodApplication ||
                        conditions.capPeriodDates.IsEqualToInvoicingApplicationPeriod(invoicingPeriodDates.applicationPeriod))
                    {
                        // Si la période d'application de la REMISE du PLAFOND
                        // = Facture ou = périodicité de FACTURATION du périmètre en cours
                        if (totalRebateBracketAmount.HasValue && (0 < totalRebateBracketAmount.Value))
                            netTurnOverInExcessAmount = Math.Max(0, grossTurnOverAmount - totalRebateBracketAmount.Value - parameters.MaximumNetTurnOverAmount.Amount.DecValue);
                        else
                            netTurnOverInExcessAmount = Math.Max(0, grossTurnOverAmount - parameters.MaximumNetTurnOverAmount.Amount.DecValue);
                    }
                    else
                    {
                        // Recherche de toutes les factures précédentes dans la période d'application du PLAFOND
                        // pour récupérer la somme des montants nets déjà facturés
                        codeReturn = GetPreviousInvoicingAmount(pInvoicingEntity, conditions.bracketPeriodDates, out _, out previousNetTurnOverAmount);
                        netTurnOverInExcessAmount = Math.Max(0, previousNetTurnOverAmount.Value + grossTurnOverAmount - totalRebateBracketAmount.Value - parameters.MaximumNetTurnOverAmount.Amount.DecValue);
                    }
                    result.NetTurnOverInExcessAmountSpecified = netTurnOverInExcessAmount.HasValue && (0 < netTurnOverInExcessAmount.Value);
                    if (result.NetTurnOverInExcessAmountSpecified)
                        result.NetTurnOverInExcessAmount = ProductBase.CreateMoney(netTurnOverInExcessAmount.Value, context.idC_Fee);
                    result.SumOfNetTurnOverPreviousPeriodAmountSpecified = previousNetTurnOverAmount.HasValue && (0 < previousNetTurnOverAmount.Value);
                    if (result.SumOfNetTurnOverPreviousPeriodAmountSpecified)
                        result.SumOfNetTurnOverPreviousPeriodAmount = ProductBase.CreateMoney(previousNetTurnOverAmount.Value, context.idC_Fee);
                }
                #endregion CapConditions
                #region TOTAL REBATE
                // Montant total de la remise
                // EG 20100217 (0 <= totalRebateBracketAmount.Value) replace (0 < totalRebateBracketAmount.Value)
                invoice.RebateConditions.TotalRebateAmountSpecified = (totalRebateBracketAmount.HasValue && (0 <= totalRebateBracketAmount.Value)) ||
                                                                      (netTurnOverInExcessAmount.HasValue && (0 < netTurnOverInExcessAmount.Value));
                invoice.RebateAmountSpecified = invoice.RebateConditions.TotalRebateAmountSpecified;
                if (invoice.RebateConditions.TotalRebateAmountSpecified)
                {
                    rebateAmount = (totalRebateBracketAmount ?? 0) + (netTurnOverInExcessAmount ?? 0);
                    invoice.RebateConditions.TotalRebateAmount = ProductBase.CreateMoney(rebateAmount.Value, context.idC_Fee);
                    invoice.RebateAmount = ProductBase.CreateMoney(rebateAmount.Value, context.idC_Fee);
                }
                #endregion TOTAL REBATE
            }
            #endregion Rebates

            decimal grossTurnOverAmountForTax = grossTurnOverAmount - (rebateAmount ?? 0);

            // EG 20240205 [WI640] Gestion de l'application de plafonds négatifs
            // Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
            if (grossTurnOverAmountForTax < 0)
            {
                grossTurnOverAmountForTax = Math.Abs(grossTurnOverAmountForTax);
                IParty partyBuyer = m_DataDocument.GetPartyBuyer();
                IParty partySeller = m_DataDocument.GetPartySeller();
                string tmp = invoice.PayerPartyReference.HRef;
                invoice.PayerPartyReference.HRef = invoice.ReceiverPartyReference.HRef;
                invoice.ReceiverPartyReference.HRef = tmp;
            }
            #region TaxAmount
            int nbTax = 0;
            // Montant tax en devise de courtage
            if (tax.TaxDetailsSpecified)
            {
                invoice.TaxSpecified = tax.TaxDetailsSpecified;
                nbTax = tax.TaxDetails.Length;
                invoice.Tax = invoice.CreateInvoiceTax(nbTax);
                for (int i = 0; i < nbTax; i++)
                {
                    decimal amount = tax.TaxDetails[i].TaxRate * grossTurnOverAmountForTax;
                    EFS_Cash cash = new EFS_Cash(CS, amount, context.idC_Fee);
                    invoice.Tax.Details[i].TaxAmountSpecified = true;
                    invoice.Tax.Details[i].TaxAmount = invoice.CreateTripleInvoiceAmounts();
                    invoice.Tax.Details[i].TaxAmount.Amount = ProductBase.CreateMoney(cash.AmountRounded, context.idC_Fee);
                    #region Source Tax
                    ISpheresSource source = invoice.Tax.Details[i].TaxSource;
                    source.SpheresId = ProductBase.CreateSpheresId(3);
                    source.SpheresId[0].Scheme = Cst.OTCml_RepositoryTaxDetailScheme;
                    source.SpheresId[0].OTCmlId = tax.TaxDetails[i].IdTaxDet;
                    source.SpheresId[0].Value = tax.TaxDetails[i].Identifier;
                    source.SpheresId[1].Scheme = Cst.OTCml_RepositoryTaxDetailRateScheme;
                    source.SpheresId[1].Value = StrFunc.FmtDecimalToInvariantCulture(tax.TaxDetails[i].TaxRate);
                    source.SpheresId[2].Scheme = Cst.OTCml_RepositoryTaxDetailEventTypeScheme;
                    source.SpheresId[2].Value = tax.TaxDetails[i].EventType;

                    #endregion Source Tax
                    taxAmount += cash.AmountRounded;
                }
                invoice.Tax.Amount = ProductBase.CreateMoney(taxAmount, context.idC_Fee);
            }
            #endregion TaxAmount


            #region NetTurnOverAmount  / Tax
            // Montant net en devise de courtage
            //netTurnOverAmount = grossTurnOverAmount - (rebateAmount.HasValue ? rebateAmount.Value : 0) + taxAmount;
            decimal netTurnOverAmount = grossTurnOverAmountForTax + taxAmount;
            invoice.NetTurnOverAmount = ProductBase.CreateMoney(netTurnOverAmount, context.idC_Fee);
            #endregion NetTurnOverAmount  / Tax
            #region NetTurnOverIssueAmount  / Tax
            // Montant net en devise de facturation
            if (rules.idC_Invoicing == context.idC_Fee)
            {
                netTurnOverIssueAmount = netTurnOverAmount;
                taxIssueAmount = taxAmount;
                #region Tax details
                if (tax.TaxDetailsSpecified)
                {
                    for (int i = 0; i < nbTax; i++)
                    {
                        invoice.Tax.Details[i].TaxAmount.IssueAmountSpecified = true;
                        invoice.Tax.Details[i].TaxAmount.IssueAmount =
                            ProductBase.CreateMoney(invoice.Tax.Details[i].TaxAmount.Amount.Amount.DecValue, rules.idC_Invoicing);
                    }
                }
                #endregion Tax details
            }
            else if (rules.periodOffset_InvoicingSpecified && rules.idAsset_InvoicingSpecified)
            {
                #region Issue Rate
                SQL_AssetFxRate sql_Asset = new SQL_AssetFxRate(CS, rules.idAsset_Invoicing);
                if (sql_Asset.IsLoaded)
                {
                    IInformationSource rateSource = invoice.CreateInformationSource(rules.idAsset_Invoicing, sql_Asset.PrimaryRateSrc);
                    rateSource.RateSourcePage.Value = sql_Asset.PrimaryRateSrcPage;
                    rateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_Asset.PrimaryRateSrcHead);
                    if (rateSource.RateSourcePageHeadingSpecified)
                        rateSource.RateSourcePageHeading = sql_Asset.PrimaryRateSrcHead;

                    IBusinessCenterTime businessCenterTime = ProductBase.CreateBusinessCenterTime(sql_Asset.TimeRateSrc, sql_Asset.IdBC_RateSrc);

                    IRelativeDateOffset relativeDateOffset = ProductBase.CreateRelativeDateOffset();
                    relativeDateOffset.DateRelativeToValue = InvoiceSettlementDelayRelativeToEnum.invoiceDate.ToString();
                    relativeDateOffset.Period = rules.periodOffset_Invoicing;
                    relativeDateOffset.PeriodMultiplier = new EFS_Integer(rules.periodMltpOffset_Invoicing);
                    relativeDateOffset.BusinessDayConvention = BusinessDayConventionEnum.NONE;
                    relativeDateOffset.DayTypeSpecified = rules.dayTypeOffset_InvoicingSpecified;
                    if (relativeDateOffset.DayTypeSpecified)
                        relativeDateOffset.DayType = rules.dayTypeOffset_Invoicing;

                    invoice.InvoiceRateSource = invoice.CreateInvoiceRateSource(rateSource, businessCenterTime, relativeDateOffset);
                    invoice.InvoiceRateSourceSpecified = true;

                    #region Fixing Date & IssueRate for NetTurnOverIssueAmount
                    codeReturn = Tools.OffSetDateRelativeTo(CS, invoice.InvoiceRateSource.FixingDate, out DateTime offsetDate, m_DataDocument);
                    if (codeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        invoice.InvoiceRateSource.AdjustedFixingDateSpecified = DtFunc.IsDateTimeFilled(offsetDate);
                        invoice.InvoiceRateSource.AdjustedFixingDate = ProductBase.CreateAdjustedDate(offsetDate);
                        codeReturn = CalculNetTurnOverIssueRate();
                        if ((Cst.ErrLevel.SUCCESS == codeReturn) && invoice.NetTurnOverIssueRateSpecified)
                        {
                            IMoney amount = ProductBase.CreateMoney(netTurnOverAmount, rules.idC_Invoicing);
                            IQuotedCurrencyPair qcp = ProductBase.CreateQuotedCurrencyPair(sql_Asset.QCP_Cur1, sql_Asset.QCP_Cur2, sql_Asset.QCP_QuoteBasisEnum);
                            EFS_Cash cash = new EFS_Cash(CS, amount, invoice.NetTurnOverIssueRate.DecValue, qcp);
                            netTurnOverIssueAmount = cash.ExchangeAmountRounded;
                            if (cash.IsExchangeRateIsReverse)
                            {
                                invoice.IssueRateIsReverseSpecified = true;
                                invoice.IssueRateIsReverse = new EFS_Boolean(cash.IsExchangeRateIsReverse);
                                invoice.IssueRateReadSpecified = true;
                                invoice.IssueRateRead = new EFS_Decimal(invoice.NetTurnOverIssueRate.DecValue);
                            }
                            if (invoice.TaxSpecified)
                            {
                                taxAmount = 0;
                                #region Tax details
                                if (tax.TaxDetailsSpecified)
                                {
                                    for (int i = 0; i < nbTax; i++)
                                    {
                                        amount = ProductBase.CreateMoney(invoice.Tax.Details[i].TaxAmount.Amount.Amount.DecValue, rules.idC_Invoicing);
                                        cash = new EFS_Cash(CS, amount, invoice.NetTurnOverIssueRate.DecValue, qcp);
                                        invoice.Tax.Details[i].TaxAmount.IssueAmountSpecified = true;
                                        invoice.Tax.Details[i].TaxAmount.IssueAmount = ProductBase.CreateMoney(cash.ExchangeAmountRounded, rules.idC_Invoicing);
                                        taxAmount += cash.ExchangeAmountRounded;
                                    }
                                }
                                taxIssueAmount = taxAmount;
                                #endregion Tax details
                            }
                        }
                    }
                    else
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5216), 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, idInvoicingRules)),
                            new LogParam(rules.idC_Invoicing),
                            new LogParam(context.idC_Fee)));

                        codeReturn = Cst.ErrLevel.FAILURE;
                    }
                    #endregion Fixing Date & IssueRate for NetTurnOverIssueAmount
                }
                #endregion Issue Rate
            }
            else
            {
                // EG 20110205 Add Message
                if ((false == rules.idAsset_InvoicingSpecified) || (false == rules.periodMltpOffset_InvoicingSpecified) ||
                    (false == rules.periodOffset_InvoicingSpecified) || (false == rules.dayTypeOffset_InvoicingSpecified))
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5216), 0,
                        new LogParam(LogTools.IdentifierAndId(identifier, idInvoicingRules)),
                        new LogParam(rules.idC_Invoicing),
                        new LogParam(context.idC_Fee)));

                    codeReturn = Cst.ErrLevel.FAILURE;
                }
            }
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                invoice.NetTurnOverIssueAmount = ProductBase.CreateMoney(netTurnOverIssueAmount, rules.idC_Invoicing);
                if (invoice.TaxSpecified)
                {
                    invoice.Tax.IssueAmountSpecified = true;
                    invoice.Tax.IssueAmount = ProductBase.CreateMoney(taxIssueAmount, rules.idC_Invoicing);
                }
            }
            #endregion NetTurnOverIssueAmount / Tax
            #region NetTurnOverAccountingAmount / Tax
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // Montant net en devise de comptabilisation initialisé si devise identique sinon prise en charge par
                // la génération des événements.
                if (pInvoicingEntity.actorEntity.SQLActor.IdCAccount == context.idC_Fee)
                {
                    invoice.NetTurnOverAccountingAmountSpecified = true;
                    invoice.NetTurnOverAccountingAmount = ProductBase.CreateMoney(netTurnOverAmount, context.idC_Fee);
                    if (invoice.TaxSpecified)
                    {
                        #region Tax details
                        if (tax.TaxDetailsSpecified)
                        {
                            for (int i = 0; i < nbTax; i++)
                            {
                                ITripleInvoiceAmounts taxAmountDetails = invoice.Tax.Details[i].TaxAmount;
                                taxAmountDetails.AccountingAmountSpecified = true;
                                taxAmountDetails.AccountingAmount = ProductBase.CreateMoney(taxAmountDetails.Amount.Amount.DecValue, context.idC_Fee);
                            }
                            invoice.Tax.AccountingAmountSpecified = true;
                            invoice.Tax.AccountingAmount = ProductBase.CreateMoney(invoice.Tax.Amount.Amount.DecValue, context.idC_Fee);

                        }
                        #endregion Tax details
                    }
                }
            }
            #endregion NetTurnOverAccountingAmount  / Tax
            return codeReturn;
        }
        #endregion InvoiceAmountsCalculation
        #region LockTrade
        public Cst.ErrLevel LockTrade(int pIdT, string pIdentifier)
        {
            m_LockObject = ProcessBase.LockElement(TypeLockEnum.TRADE, pIdT, pIdentifier, true, LockTools.Exclusive);
            if (null == m_LockObject)
                return Cst.ErrLevel.LOCKUNSUCCESSFUL;
            return Cst.ErrLevel.SUCCESS;


        }
        #endregion LockTrade
        #region MatchInvoicingScope
        /// <summary>
        /// Matchage d'une élément avec le contexte en cours (Instruction de facturation) 
        /// </summary>
        /// <param name="pContext"></param>
        /// <param name="pWeight"></param>
        /// <param name="pInvoicingContextEvent"></param>
        /// <returns></returns>
        // EG 20141020 [20442] Add IdGrpContract|GrpContract
        public MatchingEnum MatchInvoicingScope(InvoicingContextEnum pContext, int pWeight, InvoicingContextEvent pInvoicingContextEvent)
        {
            MatchingEnum match;
            switch (pContext)
            {
                case InvoicingContextEnum.IdA_Payer:
                    match = context.IsMatch_IdA_Payer(pInvoicingContextEvent.idA_Payer);
                    break;
                case InvoicingContextEnum.EventType:
                    match = context.IsMatch_EventType(pInvoicingContextEvent.eventType);
                    break;
                case InvoicingContextEnum.GrpProduct:
                    match = context.IsMatch_GrpProduct(pInvoicingContextEvent.grpProduct);
                    break;
                case InvoicingContextEnum.IdA_Invoiced:
                    match = context.IsMatch_IdA_Invoiced(pInvoicingContextEvent.idA_Invoiced);
                    break;
                case InvoicingContextEnum.IdA_Trader:
                    match = context.IsMatch_IdA_Trader(pInvoicingContextEvent.idA_Trader);
                    break;
                case InvoicingContextEnum.IdB:
                    match = context.IsMatch_IdB(pInvoicingContextEvent.idB);
                    break;
                case InvoicingContextEnum.IdC_Fee:
                    match = context.IsMatch_IdC_Fee(pInvoicingContextEvent.idC_Fee);
                    break;
                case InvoicingContextEnum.IdC_Trade:
                    match = context.IsMatch_IdC_Trade(pInvoicingContextEvent.idC_Trade);
                    break;
                case InvoicingContextEnum.IdGrpBook:
                    match = context.IsMatch_GrpBook(pInvoicingContextEvent.idGrpBook);
                    break;
                // EG 20141020 [20442]
                case InvoicingContextEnum.IdGrpContract:
                    match = context.IsMatch_GrpContract(pInvoicingContextEvent.idGrpContract);
                    break;
                case InvoicingContextEnum.IdGrpInstr:
                    match = context.IsMatch_GrpInstr(pInvoicingContextEvent.idGrpInstr);
                    break;
                case InvoicingContextEnum.IdGrpInstr_Underlyer:
                    match = context.IsMatch_GrpInstrUnderlyer(pInvoicingContextEvent.idGrpInstr_Underlyer);
                    break;
                case InvoicingContextEnum.IdGrpMarket:
                    match = context.IsMatch_GrpMarket(pInvoicingContextEvent.idGrpMarket);
                    break;
                // EG 20141020 [20442] Add 
                case InvoicingContextEnum.IdContract:
                    match = context.IsMatch_Contract(pInvoicingContextEvent.idContract);
                    break;
                case InvoicingContextEnum.IdI:
                    match = context.IsMatch_Instr(pInvoicingContextEvent.idI);
                    break;
                case InvoicingContextEnum.IdI_Underlyer:
                    match = context.IsMatch_InstrUnderlyer(pInvoicingContextEvent.idI_Underlyer);
                    break;
                case InvoicingContextEnum.IdM:
                    match = context.IsMatch_Market(pInvoicingContextEvent.idM);
                    break;
                case InvoicingContextEnum.IdP:
                    match = context.IsMatch_Product(pInvoicingContextEvent.idP);
                    break;
                case InvoicingContextEnum.PaymentType:
                    match = context.IsMatch_PaymentType(pInvoicingContextEvent.paymentType);
                    break;
                default:
                    match = MatchingEnum.Ignore;
                    break;
            }
            if (MatchingEnum.HiMatch == match)
                m_ResultMatching += Math.Pow(Convert.ToDouble(2), Convert.ToDouble(pWeight));
            else if (MatchingEnum.UnMatch == match)
                m_ResultMatching = 0;
            return match;
        }
        #endregion MatchInvoicingScope
        #region SetDataDocument
        /// <summary>
        /// Alimentation du DataDocument de facturation (Facture, Facture additionnelle, Avoir)
        /// - Détermination du type de document (Facture, Facture additionnelle ou Avoir)
        /// - Alimentation des parties
        /// - Alimentation du tradeHeader
        /// - Alimentation des données de facturation
        /// - Alimentation des données "Détails" de facturation
        /// - Alimentation des paramètres de remise
        /// 
        /// - Calcul des montants de facturation
        /// 
        /// </summary>
        /// <param name="pInvoiceDate"></param>
        /// <param name="pInvoicingEntity"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Del m_TradeLibrary (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel SetDataDocument(DateTime pInvoiceDate, InvoicingEntity pInvoicingEntity)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            AdministrativeInstrumentBase currentInstrument = pInvoicingEntity.CurrentInstrument;
            if (null == currentInstrument)
                codeReturn = Cst.ErrLevel.DATANOTFOUND;

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                m_DataDocument = (DataDocumentContainer)currentInstrument.DataDocument.Clone();
                m_ProductContainer = m_DataDocument.CurrentProduct;
                codeReturn = SetParties(pInvoicingEntity);
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = SetTradeHeader(pInvoicingEntity);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                if (m_ProductContainer.IsInvoice || m_ProductContainer.IsAdditionalInvoice || m_ProductContainer.IsCreditNote)
                {
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = SetInvoice(pInvoiceDate, pInvoicingEntity);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = SetInvoiceDetails();
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = SetInvoiceRebateParameters();
                }
                if ((Cst.ErrLevel.SUCCESS == codeReturn) && m_ProductContainer.IsInvoice)
                    codeReturn = InvoiceAmountsCalculation(pInvoicingEntity);
            }

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                m_DataDocument.SetTradeSide(CS);

            return codeReturn;
        }
        #endregion SetDataDocument
        #region SetInvoice
        /// <summary>
        /// Mise à jour des données générales de facturation
        /// - Instrument associé (Facture, Facture additionnelles ou Avoir)
        /// - Payeur et Receveur
        /// - Scope (Instructions de facturation associé)
        /// - Date de facturation (EOM)
        /// - Date de paiement
        /// </summary>
        /// <param name="pInvoiceDate"></param>
        /// <param name="pInvoicingEntity"></param>
        /// <returns></returns>
        // EG 20141020 [20442] Add actorBeneficiary
        private Cst.ErrLevel SetInvoice(DateTime pInvoiceDate, InvoicingEntity pInvoicingEntity)
        {
            IInvoice invoice = (IInvoice)m_DataDocument.CurrentProduct.Product;
            #region Scope
            invoice.Scope = invoice.CreateInvoicingScope(idInvoicingRules, identifier);
            #endregion Scope
            #region PayerPartyReference / ReceiverPartyReference
            switch (pInvoicingEntity.defaultInstrumentType)
            {
                case InvoicingInstrumentTypeEnum.AdditionalInvoicing:
                case InvoicingInstrumentTypeEnum.Invoicing:
                case InvoicingInstrumentTypeEnum.InvoicingSettlement:
                    invoice.PayerPartyReference = ProductBase.CreatePartyOrAccountReference(actorInvoiced.XmlId);

                    if (null != actorBeneficiary)
                        invoice.ReceiverPartyReference = ProductBase.CreatePartyOrAccountReference(actorBeneficiary.actor.XmlId);
                    else
                        invoice.ReceiverPartyReference = ProductBase.CreatePartyOrAccountReference(pInvoicingEntity.actorEntity.XmlId);
                    break;
                case InvoicingInstrumentTypeEnum.CreditNote:
                    if (null != actorBeneficiary)
                        invoice.PayerPartyReference = ProductBase.CreatePartyOrAccountReference(actorBeneficiary.actor.XmlId);
                    else
                        invoice.PayerPartyReference = ProductBase.CreatePartyOrAccountReference(pInvoicingEntity.actorEntity.XmlId);

                    invoice.ReceiverPartyReference = ProductBase.CreatePartyOrAccountReference(actorInvoiced.XmlId);
                    break;
            }
            #endregion PayerPartyReference / ReceiverPartyReference
            #region InvoiceDate
            if (null == invoice.InvoiceDate)
                invoice.InvoiceDate = ProductBase.CreateAdjustedDate(pInvoiceDate.Date);
            else
                invoice.InvoiceDate.DateValue = pInvoiceDate.Date;
            invoice.InvoiceDate.Id = InvoiceSettlementDelayRelativeToEnum.invoiceDate.ToString();
            #endregion InvoiceDate
            #region PaymentDate
            Cst.ErrLevel codeReturn = SetPaymentDate();
            #endregion PaymentDate
            return codeReturn;
        }
        #endregion SetInvoice
        #region SetInvoiceDetails
        /// <summary>
        /// Mise à jour des données élémentaires des trades de marché associés à la facturation (Facture, Facture additionnelles ou Avoir)
        /// - Trade (Identifiant et Id, Instrument, BusinessDate, In/Out date, Side, Contrepartie, notionalAmount)
        /// - Marché
        /// - Contrat et sa catégorie
        /// - Trader et Sales
        /// - Frais (Id = IDE, Type, Montants et Devises)
        /// </summary>
        /// <returns></returns>
        // EG 20090108 Add Else Break & New Parameters for SetInvoiceTrade calling
        public Cst.ErrLevel SetInvoiceDetails()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IInvoice invoice = (IInvoice)m_DataDocument.CurrentProduct.Product;
            invoice.InvoiceDetails = invoice.CreateInvoiceDetails(this.trades.Length);

            string logScope = LogTools.IdentifierAndId(identifier, idInvoicingRules);
            string logActorInvoiced = LogTools.IdentifierAndId(actorInvoiced.Identifier, actorInvoiced.Id);
            
            for (int i = 0; i < trades.Length; i++)
            {
                codeReturn = trades[i].SetInvoiceTrade(ProcessBase, invoice.InvoiceDetails, i, m_IdCAccount, logScope, logActorInvoiced);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    IParty party = m_DataDocument.GetParty(trades[i].actorCounterparty.XmlId, PartyInfoEnum.id);
                    if (null == party)
                    {
                        party = m_DataDocument.AddParty(trades[i].actorCounterparty.XmlId);
                        party.Id = trades[i].actorCounterparty.XmlId;
                        party.OTCmlId = trades[i].actorCounterparty.Id;
                        party.PartyId = trades[i].actorCounterparty.Identifier;
                        party.PartyName = trades[i].actorCounterparty.DisplayName;
                        Tools.AddPartyId(party, Cst.OTCml_ActorBicScheme, trades[i].actorCounterparty.BIC);
                    }
                }
                else
                    break;
            }
            return codeReturn;
        }
        #endregion SetInvoiceDetails
        #region SetInvoiceRebateParameters
        /// <summary>
        /// Alimentation des paramètres de conditions éventuelles de remise
        /// - Par plafond et par Tranche
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel SetInvoiceRebateParameters()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            IInvoice invoice = (IInvoice)m_DataDocument.CurrentProduct.Product;
            #region Rebates
            invoice.RebateConditionsSpecified = conditions.maxValueSpecified || (0 < conditions.brackets.Length);
            invoice.RebateConditions = invoice.CreateRebateConditions();
            #region CapConditions
            invoice.RebateConditions.CapConditionsSpecified = conditions.maxValueSpecified;
            if (invoice.RebateConditions.CapConditionsSpecified)
            {
                IRebateCapConditions capConditions = invoice.RebateConditions.CapConditions;
                capConditions.Parameters = capConditions.CreateParameters(conditions.maxPeriod, conditions.maxPeriodMltp, rules.rollConvention_Invoicing, conditions.maxValue, this.context.idC_Fee);
                capConditions.Result = capConditions.CreateResult();
            }
            #endregion CapConditions
            #region BraketConditions
            int bracketLength = conditions.brackets.Length;
            invoice.RebateConditions.BracketConditionsSpecified = (0 < bracketLength);
            if (invoice.RebateConditions.BracketConditionsSpecified)
            {
                conditions.Sort();
                IRebateBracketConditions bracketConditions = invoice.RebateConditions.BracketConditions;
                bracketConditions.Parameters = bracketConditions.CreateParameters(conditions.bracketApplication,
                    conditions.discountPeriod, conditions.discountPeriodMltp, rules.rollConvention_Invoicing, bracketLength);
                for (int i = 0; i < bracketLength; i++)
                {
                    InvoicingBracket invoicingBracket = conditions[i];
                    IRebateBracketParameter parameter = bracketConditions.Parameters.CreateParameter(invoicingBracket.lowValue, invoicingBracket.highValue, invoicingBracket.discountRate);
                    bracketConditions.Parameters.SetParameter(i, parameter);
                }
                bracketConditions.Result = bracketConditions.CreateResult(bracketConditions.Parameters.Parameter);
            }
            #endregion BraketConditions
            #endregion Rebates
            return codeReturn;
        }
        #endregion SetInvoiceRebateParameters
        #region SetParties
        /// <summary>
        /// Mise à jour des parties pour une facture, facture additionnelle ou avoir
        /// - pour l'acteur bénéficiaire
        /// - pour l'entité
        /// - pour l'acteur facturé
        /// </summary>
        /// <param name="pInvoicingEntity"></param>
        /// <returns></returns>
        // EG 20141020 [20442] Refactoring
        // EG 20230526 [WI640] Nouvelle gestion Mise à jour des parties (Cas des REBATE)
        private Cst.ErrLevel SetParties(InvoicingEntity pInvoicingEntity)
        {
            if (null != actorBeneficiary)
                SetParties(actorBeneficiary.actor, (pInvoicingEntity.actorEntity.Id == actorBeneficiary.actor.Id));
            else
                SetParties(pInvoicingEntity.actorEntity, true);
            SetParties(actorInvoiced, pInvoicingEntity.actorEntity.Id == actorInvoiced.Id);
            return Cst.ErrLevel.SUCCESS;
        }
        /// <summary>
        /// Mise à jour d'une party pour une facture, facture additionnelle ou avoir
        /// </summary>
        /// <param name="pActor">Acteur concerné</param>
        /// <param name="pIsEntity">Indicateur si l'acteur est l'entité</param>
        /// <returns></returns>
        // EG 20141020 [20442] New
        private Cst.ErrLevel SetParties(InvoicingActor pActor, bool pIsEntity)
        {
            SQL_Actor actor = pActor.SQLActor;
            IParty party = m_DataDocument.GetParty(pIsEntity ? Cst.FpML_EntityOfUserIdentifier : actor.XmlId, PartyInfoEnum.id);
            if (null == party)
                party = m_DataDocument.GetParty(actor.XmlId, PartyInfoEnum.id);
            if (null == party)
                m_DataDocument.AddParty(actor);
            else
                Tools.SetParty(party, actor);
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetParties
        #region SetPaymentDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel SetPaymentDate()
        {
            IInvoice invoice = (IInvoice)m_DataDocument.CurrentProduct.Product;
            invoice.PaymentDate.RelativeDateSpecified = true;
            invoice.PaymentDate.RelativeDate = ProductBase.CreateRelativeDateOffset();
            invoice.PaymentDate.RelativeDate.BusinessDayConvention = rules.bdc_StlDelay;
            invoice.PaymentDate.RelativeDate.DateRelativeToValue = rules.relativeTo_StlDelay.ToString();
            invoice.PaymentDate.RelativeDate.DayType = rules.dayType_StlDelay;
            invoice.PaymentDate.RelativeDate.DayTypeSpecified = true;
            invoice.PaymentDate.RelativeDate.PeriodMultiplier = new EFS_Integer(rules.periodMltp_StlDelay);
            invoice.PaymentDate.RelativeDate.Period = rules.period_StlDelay;
            Cst.ErrLevel codeReturn = Tools.OffSetDateRelativeTo(CS, invoice.PaymentDate.RelativeDate, out DateTime offsetDate, m_DataDocument);
            if (codeReturn == Cst.ErrLevel.SUCCESS)
            {
                invoice.PaymentDate.AdjustedDateSpecified = DtFunc.IsDateTimeFilled(offsetDate);
                invoice.PaymentDate.AdjustedDate = ProductBase.CreateAdjustedDate(offsetDate);
            }
            return codeReturn;
        }
        #endregion SetPaymentDate
        #region SetTradeHeader
        /// <summary>
        /// Mise à jour du TradeHeader pour une facture, facture additionnelle ou avoir
        /// </summary>
        /// <param name="pInvoicingEntity">Paramètres de facturation liés à l'entité</param>
        /// <returns></returns>
        // EG 20141020 [20442] Refactoring
        private Cst.ErrLevel SetTradeHeader(InvoicingEntity pInvoicingEntity)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DateTime dtSys = OTCmlHelper.GetDateBusiness(CS);

            SQL_Actor actor = pInvoicingEntity.actorEntity.SQLActor;
            SQL_Book book = pInvoicingEntity.bookEntity.SQLBook;
            IParty party = m_DataDocument.GetParty(actor.XmlId, PartyInfoEnum.id);

            #region PartyTradeIdentifier
            IPartyTradeIdentifier partyTradeIdentifier = m_DataDocument.GetPartyTradeIdentifier(Cst.FpML_EntityOfUserIdentifier);
            if (null == partyTradeIdentifier)
                partyTradeIdentifier = m_DataDocument.GetPartyTradeIdentifier(party.Id);
            if (null == partyTradeIdentifier)
                m_DataDocument.AddPartyTradeIndentifier(party.Id);
            else
                Tools.SetPartyTradeIdentifier(partyTradeIdentifier, party.Id, book);
            #endregion PartyTradeIdentifier

            #region PartyTradeInformation
            IPartyTradeInformation partyTradeInformation = m_DataDocument.GetPartyTradeInformation(Cst.FpML_EntityOfUserIdentifier);
            if (null == partyTradeInformation)
                partyTradeInformation = m_DataDocument.AddPartyTradeInformation(party.Id);

            if (null != partyTradeInformation)
            {
                ITradeProcessingTimestamps timestamps = DataDocument.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();
                timestamps.OrderEntered = Tz.Tools.ToString(dtSys);
                timestamps.OrderEnteredSpecified = true;
                partyTradeInformation.Timestamps = timestamps;
                partyTradeInformation.TimestampsSpecified = true;
            }
            #endregion PartyTradeInformation

            // EG 20141020 [20442] New
            if (null != actorBeneficiary)
            {
                #region PartyTradeIdentifier
                party = m_DataDocument.GetParty(actorBeneficiary.actor.XmlId, PartyInfoEnum.id);
                partyTradeIdentifier = m_DataDocument.GetPartyTradeIdentifier(actorBeneficiary.actor.XmlId);
                if (null == partyTradeIdentifier)
                    partyTradeIdentifier = m_DataDocument.GetPartyTradeIdentifier(party.Id);
                if (null == partyTradeIdentifier)
                    partyTradeIdentifier = m_DataDocument.AddPartyTradeIndentifier(party.Id);
                Tools.SetPartyTradeIdentifier(partyTradeIdentifier, party.Id, actorBeneficiary.book.SQLBook);
                #endregion PartyTradeIdentifier

                #region PartyTradeInformation
                partyTradeInformation = m_DataDocument.AddPartyTradeInformation(party.Id);

                if (null != partyTradeInformation)
                {
                    ITradeProcessingTimestamps timestamps = DataDocument.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();
                    timestamps.OrderEntered = Tz.Tools.ToString(dtSys);
                    timestamps.OrderEnteredSpecified = true;
                    partyTradeInformation.Timestamps = timestamps;
                    partyTradeInformation.TimestampsSpecified = true;
                }
                #endregion PartyTradeInformation
            }
            #region TradeDate
            m_DataDocument.TradeDate = this.invoicingPeriodDates.startPeriod;
            m_DataDocument.TradeHeader.TradeDate.Efs_id = InvoiceSettlementDelayRelativeToEnum.tradeDate.ToString();
            #endregion TradeDate
            return codeReturn;

        }
        #endregion SetTradeHeader
        #region UnLockTrade
        public void UnLockTrade(int pIdT)
        {
            if ((null != m_LockObject) && (m_LockObject.ObjectId == pIdT.ToString() ))
            {
                if (LockTools.UnLock(CS, m_LockObject, ProcessBase.Session.SessionId))
                    m_LockObject = null;
            }
        }
        #endregion UnLockTrade
        #endregion Methods
    }
    #endregion InvoicingScope
    #region InvoiceStlInstrument
    /// <summary>
    /// Classe de travail matérialisant un instrument de type : Règlement de facture
    /// </summary>
    public class InvoiceStlInstrument : AdministrativeInstrumentBase
    {
        #region Constructors
        public InvoiceStlInstrument(ProcessBase pProcess, InvoicingInstrumentTypeEnum pInvoicingInstrumentType, int pIdI, string pLogEntity)
            : base(pProcess, pInvoicingInstrumentType, pIdI, pLogEntity) { }
        #endregion Constructors
    }
    #endregion InvoiceStlInstrument
    #region InvoicingTax
    /// <summary>
    /// Classe qui contient les paramètres de taxes pour un périmètre de facturation :
    /// - Application
    /// - Conditions
    /// </summary>

    public class InvoicingTax
    {
        #region Members
        private readonly string m_Cs;
        private readonly string m_EventType;
        private bool m_IsTaxApplied;
        private readonly TaxApplicationEnum m_TaxApplication;
        private readonly bool m_TaxConditionSpecified;
        private readonly string m_TaxCondition;
        private bool m_TaxDetailsSpecified;
        private InvoicingTaxDetail[] m_TaxDetails;
        #endregion Members
        #region Accessors
        #region TaxDetails
        public InvoicingTaxDetail[] TaxDetails
        {
            get { return m_TaxDetails; }
        }
        #endregion TaxDetails
        #region EventType
        public string EventType
        {
            get { return m_EventType; }
        }
        #endregion EventType
        #region IsTaxApplied
        public bool IsTaxApplied
        {
            get { return m_IsTaxApplied; }
        }
        #endregion IsTaxApplied
        #region TaxApplication
        public TaxApplicationEnum TaxApplication
        {
            get { return m_TaxApplication; }
        }
        #endregion TaxApplication
        #region TaxConditionSpecified
        public bool TaxConditionSpecified
        {
            get { return m_TaxConditionSpecified; }
        }
        #endregion TaxConditionSpecified
        #region TaxCondition
        public string TaxCondition
        {
            get { return m_TaxCondition; }
        }
        #endregion TaxCondition
        #region TaxDetailsSpecified
        public bool TaxDetailsSpecified
        {
            get { return m_TaxDetailsSpecified; }
        }
        #endregion TaxDetailsSpecified
        #endregion Accessors
        #region Constructors
        public InvoicingTax(string pConnectionString, DataRow pRowInvoicingRule)
        {
            m_Cs = pConnectionString;
            m_EventType = pRowInvoicingRule["EVENTTYPE"].ToString();

            m_TaxApplication = TaxApplicationEnum.Always;
            if (false == Convert.IsDBNull(pRowInvoicingRule["TAXAPPLICATION"]))
                m_TaxApplication = (TaxApplicationEnum)System.Enum.Parse(typeof(TaxApplicationEnum), pRowInvoicingRule["TAXAPPLICATION"].ToString(), true);

            m_TaxConditionSpecified = (false == Convert.IsDBNull(pRowInvoicingRule["TAXCONDITION"]));
            if (m_TaxConditionSpecified)
                m_TaxCondition = pRowInvoicingRule["TAXCONDITION"].ToString();
        }
        #endregion Constructors
        #region Methods
        #region GetTax
        public Cst.ErrLevel GetTax(DateTime pInvoiceDate, InvoicingEntity pEntity)
        {
            int idA_Receiver = pEntity.actorEntity.SQLActor.Id;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            m_IsTaxApplied = (m_TaxApplication == TaxApplicationEnum.Always);
            if ((false == m_IsTaxApplied) && (m_TaxApplication == TaxApplicationEnum.Condition))
            {
                // EG Tester la condition (lorsqu'elle sera gérée)
                m_IsTaxApplied = false;
            }
            if (m_IsTaxApplied)
            {
                #region Query Tax
                string sqlSelect = SQLCst.SELECT + "tx.IDTAX, txd.IDTAXDET, txd.IDENTIFIER," + Cst.CrLf;
                sqlSelect += "txd.IDCOUNTRY, txd.TAXTYPE, txd.TAXRATE, txd.EVENTTYPE" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TAX + " tx " + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORTAX + " atx ";
                sqlSelect += SQLCst.ON + "(atx.IDTAX = tx.IDTAX)" + SQLCst.AND + "(atx.IDA=" + idA_Receiver.ToString() + ")";
                sqlSelect += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(m_Cs, "atx", pInvoiceDate) + ")" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR + " ac ";
                sqlSelect += SQLCst.ON + "(ac.IDA = atx.IDA)" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TAXEVENT + " txe ";
                sqlSelect += SQLCst.ON + "(txe.IDTAX = tx.IDTAX)" + SQLCst.AND + "(txe.EVENTTYPE=" + DataHelper.SQLString(m_EventType) + ")";
                sqlSelect += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(m_Cs, "txe", pInvoiceDate) + ")" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TAXDET + " txd ";
                sqlSelect += SQLCst.ON + "(txd.IDTAX = tx.IDTAX)" + SQLCst.AND + "(txd.IDCOUNTRY=ac.IDCOUNTRYTAX)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(m_Cs, "txd", pInvoiceDate) + ")" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "(" + OTCmlHelper.GetSQLDataDtEnabled(m_Cs, "tx", pInvoiceDate) + ")";
                #endregion Query Tax
                #region Filling Tax
                DataSet ds = DataHelper.ExecuteDataset(m_Cs, CommandType.Text, sqlSelect);
                if ((null != ds) && (0 < ds.Tables[0].Rows.Count))
                {
                    ArrayList aTaxDetail = new ArrayList();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        aTaxDetail.Add(new InvoicingTaxDetail(row));
                    }
                    m_TaxDetailsSpecified = (0 < aTaxDetail.Count);
                    if (m_TaxDetailsSpecified)
                        m_TaxDetails = (InvoicingTaxDetail[])aTaxDetail.ToArray(typeof(InvoicingTaxDetail));
                }
                #endregion Filling Tax
            }
            return codeReturn;
        }
        #endregion GetTax
        #endregion Methods
    }
    #endregion InvoicingTax
    #region InvoicingTaxDetail
    /// <summary>
    /// Classe qui contient les paramètres détaillés de taxes si applicables
    /// </summary>

    public class InvoicingTaxDetail
    {
        #region Members
        private int m_IdTax;
        private int m_IdTaxDet;
        private string m_Identifier;
        private string m_IdCountry;
        private string m_TaxType;
        private decimal m_TaxRate;
        private readonly string m_EventType;

        #endregion Members
        #region Accessors
        #region EventType
        public string EventType
        {
            get { return m_EventType; }
        }
        #endregion EventType
        #region IdCountry
        public string IdCountry
        {
            set { m_IdCountry = value; }
            get { return m_IdCountry; }
        }
        #endregion IdCountry
        #region IdTax
        public int IdTax
        {
            set { m_IdTax = value; }
            get { return m_IdTax; }
        }
        #endregion IdTax
        #region IdTaxDet
        public int IdTaxDet
        {
            set { m_IdTaxDet = value; }
            get { return m_IdTaxDet; }
        }
        #endregion IdTaxDet
        #region Identifier
        public string Identifier
        {
            set { m_Identifier = value; }
            get { return m_Identifier; }
        }
        #endregion Identifier
        #region TaxRate
        public decimal TaxRate
        {
            set { m_TaxRate = value; }
            get { return m_TaxRate; }
        }
        #endregion TaxRate
        #region TaxType
        public string TaxType
        {
            set { m_TaxType = value; }
            get { return m_TaxType; }
        }
        #endregion TaxType
        #endregion Accessors
        #region Constructors
        public InvoicingTaxDetail(DataRow pRowTax)
        {
            m_IdTax = Convert.ToInt32(pRowTax["IDTAX"]);
            m_IdTaxDet = Convert.ToInt32(pRowTax["IDTAXDET"]);
            m_Identifier = pRowTax["IDENTIFIER"].ToString();
            m_IdCountry = pRowTax["IDCOUNTRY"].ToString();
            m_TaxType = pRowTax["TAXTYPE"].ToString();
            m_TaxRate = Convert.ToDecimal(pRowTax["TAXRATE"]);
            m_EventType = pRowTax["EVENTTYPE"].ToString();
        }
        #endregion Constructors
    }
    #endregion InvoicingTaxDetail
    #region InvoicingTrade
    /// <summary>
    /// Classe qui contient les caractéristiques des trades de marché liés à une facturation
    /// - Caractéristiques générales (Id, Identifiant, Instrument, Dates, Acteur)
    /// - Evénements
    /// - Traders et Sales
    /// </summary>
    // 20090427 EG Add side (TradeSideEnum)
    // 20090427 EG Add Counterparty
    // 20090909 FI [Add Asset on InvoiceTrade]
    // 20091013 EG Add Exception for DtIn/Out
    // EG 20100108 [Add new parameters to SetInvoiceTrade calling + Test orphan trade (event)]
    // EG 20100108 [Add lastError member]
    public class InvoicingTrade
    {
        #region Members
        private readonly Cst.ErrLevel m_ErrLevel;
        public int idT;
        public string tradeIdentifier;
        public int idI;
        public string instrIdentifier;
        public TradeSideEnum side;
        public DateTime dtTrade;
        public DateTime dtIn;
        public DateTime dtOut;
        public string idC;
        public InvoicingActor actorCounterparty;
        public InvoicingEvent[] events;
        public bool isInvoicingEventIsCorrected;
        public bool tradersSpecified;
        public InvoicingTrader[] traders;
        public bool salesSpecified;
        public InvoicingTrader[] sales;
        public DataDocumentContainer dataDocument;
        public Cst.ErrLevel lastError;
        #endregion Members
        #region Accessors
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #endregion Accessors
        #region Constructors
        //20091021 FI [add sales in invoice] Add pSalesActorByTrade
        // EG 20190114 Add detail to ProcessLog Refactoring
        public InvoicingTrade(string pCS, ProcessBase pProcessBase, DataRow pRowEvent, InvoicingContextEvent pInvoicingContextEvent, DataRelation pTraderActorByTrade, DataRelation pSalesActorByTrade)
        {
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
            lastError = Cst.ErrLevel.SUCCESS;
            idT = Convert.ToInt32(pRowEvent["IDT"]);
            tradeIdentifier = pRowEvent["TRADE_IDENTIFIER"].ToString();
            instrIdentifier = pRowEvent["INSTR_IDENTIFIER"].ToString();
            idI = Convert.ToInt32(pRowEvent["IDI"]);
            dtTrade = DtFunc.AddTimeToDate(Convert.ToDateTime(pRowEvent["DTTRADE"]), Convert.ToDateTime(pRowEvent["DTTIMESTAMP"]));
            if (Convert.IsDBNull(pRowEvent["DTINADJ"]) || Convert.IsDBNull(pRowEvent["DTOUTADJ"]))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5205), 0,
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["TRADE_IDENTIFIER"].ToString(), idT)),
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["INSTR_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDI"]))),
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["PAYER_IDENTIFIER"].ToString(), Convert.ToInt32(pRowEvent["IDA_PAY"]))),
                    new LogParam(LogTools.IdentifierAndId(pRowEvent["EVENTCODE"].ToString() + "-" + pRowEvent["EVENTTYPE"].ToString(), Convert.ToInt32(pRowEvent["IDE"]))),
                    new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(pRowEvent["VALORISATION"])) + " " + pRowEvent["IDC_FEE"].ToString())));

                m_ErrLevel = Cst.ErrLevel.DATANOTFOUND;
            }
            else
            {
                dtIn = Convert.ToDateTime(pRowEvent["DTINADJ"]);
                dtOut = Convert.ToDateTime(pRowEvent["DTOUTADJ"]);
                idC = pInvoicingContextEvent.idC_Trade;
                side = (TradeSideEnum)Enum.Parse(typeof(TradeSideEnum), pRowEvent["SIDE"].ToString());
                actorCounterparty = new InvoicingActor(pCS, Convert.ToInt32(pRowEvent["COUNTERPARTY"]))
                {
                    SQLActor = null
                };

                #region Trader
                DataRow[] rowTraders = pRowEvent.GetChildRows(pTraderActorByTrade);
                if (null != rowTraders)
                {
                    ArrayList aTraders = new ArrayList();
                    int idA_Payer = Convert.ToInt32(pRowEvent["IDA_PAY"]);
                    foreach (DataRow rowTrader in rowTraders)
                    {
                        if (Convert.ToInt32(rowTrader["IDA_ACTOR"]) == idA_Payer)
                            aTraders.Add(new InvoicingTrader(rowTrader));
                    }
                    tradersSpecified = 0 < aTraders.Count;
                    if (tradersSpecified)
                        traders = (InvoicingTrader[])aTraders.ToArray(typeof(InvoicingTrader));
                }
                #endregion Trader

                //20091021 FI [add sales in invoice] Add Sales
                #region Sales
                DataRow[] rowSales = pRowEvent.GetChildRows(pSalesActorByTrade);
                if (null != rowSales)
                {
                    ArrayList aSales = new ArrayList();
                    int idA_Payer = Convert.ToInt32(pRowEvent["IDA_PAY"]);
                    foreach (DataRow row in rowSales)
                    {
                        if (Convert.ToInt32(row["IDA_ACTOR"]) == idA_Payer)
                            aSales.Add(new InvoicingTrader(row));
                    }
                    salesSpecified = 0 < aSales.Count;
                    if (salesSpecified)
                        sales = (InvoicingTrader[])aSales.ToArray(typeof(InvoicingTrader));
                }
                #endregion Sales

                #region SetDataDocument
                // 20090909 FI [Add Asset on InvoiceTrade]
                SetDataDocument(pCS);
                #endregion SetDataDocument
            }
        }
        //EG 20100331 [m_ErrLevel = Cst.ErrLevel.SUCCESS;] en tête de procédure
        public InvoicingTrade(string pCS, IInvoiceTrade pTrade, int pIdA_Counterparty)
        {
            idT = pTrade.OTCmlId;
            tradeIdentifier = pTrade.Identifier.Value;
            instrIdentifier = pTrade.Instrument.Value;
            idI = pTrade.Instrument.OTCmlId;
            dtTrade = pTrade.TradeDate.DateTimeValue;
            dtIn = pTrade.InDate.DateValue;
            dtOut = pTrade.OutDate.DateValue;
            idC = pTrade.Currency.Value;
            side = pTrade.Side;
            actorCounterparty = new InvoicingActor(pCS, pIdA_Counterparty);
            //
            SetInvoicingTrader(pTrade);
            //
            //20091021 FI [add sales in invoice] add Sales
            SetInvoicingSales(pTrade);
            //
            // 20090909 FI [Add Asset on InvoiceTrade]
            SetDataDocument(pCS);
            //
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }
        #endregion Constructors
        #region Indexors
        public InvoicingEvent this[int pIdE]
        {
            get
            {
                if (null != events)
                {
                    foreach (InvoicingEvent item in events)
                    {
                        if (item.idE == pIdE)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #region SetInvoiceTrade
        /// <summary>
        /// Mise à jour des données élémentaires des trades de marché associés à la facturation (Facture, Facture additionnelles ou Avoir)
        /// - Trade (Identifiant et Id, Instrument, BusinessDate, In/Out date, Side, Contrepartie, notionalAmount)
        /// - Marché
        /// - Contrat et sa catégorie
        /// - Trader et Sales
        /// - Frais (Id = IDE, Type, Montants et Devises)
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel SetInvoiceTrade(ProcessBase pProcessBase, IInvoiceDetails pInvoiceDetails, int pIndex, string pIdCAccount, string pLogScope, string pLogActorInvoiced)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            //20091021 FI [add sales in invoice] add salesLength
            int traderLength = ArrFunc.IsFilled(traders) ? traders.Length : 0;
            int salesLength = ArrFunc.IsFilled(sales) ? sales.Length : 0;
            int eventsLength = ArrFunc.IsFilled(events) ? events.Length : 0;
            if (0 == eventsLength)
            {
                // Trade orphelin (no events)
                codeReturn = Cst.ErrLevel.DATAREJECTED;
                pProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusNoneEnum);
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 5220), 0,
                    new LogParam(pLogScope),
                    new LogParam(pLogActorInvoiced),
                    new LogParam(LogTools.IdentifierAndId(tradeIdentifier, idT)),
                    new LogParam(lastError)));
            }
            else
            {
                IInvoiceTrade invoiceTrade = pInvoiceDetails.CreateInvoiceTrade(pProcessBase.Cs, tradeIdentifier, idT, dtTrade, dtIn, dtOut, idC, side, actorCounterparty.XmlId,
                                                                                instrIdentifier, idI, eventsLength, traderLength, salesLength, dataDocument);
                
                pInvoiceDetails.SetInvoiceTrade(pIndex, invoiceTrade);
                
                for (int i = 0; i < events.Length; i++)
                    codeReturn = events[i].SetInvoiceFee(invoiceTrade.InvoiceFees, i, pIdCAccount);
                
                for (int i = 0; i < traderLength; i++)
                    traders[i].SetInvoiceTrader(invoiceTrade, i, false);
                
                for (int i = 0; i < salesLength; i++)
                    sales[i].SetInvoiceTrader(invoiceTrade, i, true);
                
            }
            return codeReturn;
        }
        #endregion SetInvoiceTrade
        #region SetInvoicingTrader
        public Cst.ErrLevel SetInvoicingTrader(IInvoiceTrade pInvoiceTrade)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            ArrayList aTraders = new ArrayList();
            if (pInvoiceTrade.TradersSpecified)
            {
                foreach (ITrader item in pInvoiceTrade.Traders)
                {
                    InvoicingTrader trader = new InvoicingTrader
                    {
                        OTCmlId = item.OTCmlId,
                        factor = item.Factor,
                        identifier = item.Identifier,
                        name = item.Name
                    };
                    aTraders.Add(trader);
                }
            }
            tradersSpecified = (0 < aTraders.Count);
            if (tradersSpecified)
                traders = (InvoicingTrader[])aTraders.ToArray(typeof(InvoicingTrader));

            return codeReturn;
        }
        #endregion SetInvoicingTrader
        #region SetInvoicingSales
        //20091021 FI [add sales in invoice] add SetInvoicingSales
        // EG 20100218 replace pInvoiceTrade.traders par pInvoiceTrade.sales
        public Cst.ErrLevel SetInvoicingSales(IInvoiceTrade pInvoiceTrade)
        {
            ArrayList aSales = new ArrayList();
            if (pInvoiceTrade.SalesSpecified)
            {
                foreach (ITrader item in pInvoiceTrade.Sales)
                {
                    InvoicingTrader sale = new InvoicingTrader
                    {
                        OTCmlId = item.OTCmlId,
                        factor = item.Factor,
                        identifier = item.Identifier,
                        name = item.Name
                    };
                    aSales.Add(sale);
                }
            }
            salesSpecified = (0 < aSales.Count);
            if (salesSpecified)
                sales = (InvoicingTrader[])aSales.ToArray(typeof(InvoicingTrader));
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetInvoiceTrade
        #region SetDataDocument
        /// <summary>
        /// Chargement Du DataDocument à partir de l'IdT
        /// <para>Permet d'enrichir le flux avec des données incrites dans le DataDocument [Exemple : Asset]</para>
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20180205 [23769] Upd EFS_TradeLibray constructor call  (substitution to the static class EFS_CURRENT)  
        private void SetDataDocument(string pCS)
        {
            if (idT == 0)
                throw new Exception("Load DataDocument Error, idT equal Zero");

            EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(pCS, null, idT);
            dataDocument = tradeLibrary.DataDocument;
        }
        #endregion
        #endregion Methods
    }
    #endregion InvoicingTrade
    #region InvoicingTrader
    /// <summary>
    /// Classe qui contient les caractéristiques des traders et sales des trades de marché liés à une facturation
    /// </summary>
    public class InvoicingTrader
    {
        #region Members
        public int OTCmlId;
        public string identifier;
        //20091021 FI [add sales in invoice] Add name
        public string name;
        public decimal factor;
        #endregion Members
        #region Constructors
        public InvoicingTrader()
        {
        }
        public InvoicingTrader(DataRow pRowTrader)
        {
            OTCmlId = Convert.ToInt32(pRowTrader["IDA"]);
            identifier = pRowTrader["IDENTIFIER"].ToString();
            if (false == Convert.IsDBNull(pRowTrader["FACTOR"]))
                factor = Convert.ToDecimal(pRowTrader["FACTOR"]);
            //EG 20100629  Set name [missing by FI]
            if (false == Convert.IsDBNull(pRowTrader["DISPLAYNAME"]))
                name = pRowTrader["DISPLAYNAME"].ToString();
        }
        #endregion Constructors
        #region Methods
        #region SetInvoiceTrader
        //20091021 FI [add sales in invoice] Add Parameter pIsSales
        public void SetInvoiceTrader(IInvoiceTrade pInvoiceTrade, int pIndex, bool pIsSales)
        {
            ITrader trader = pInvoiceTrade.CreateTrader();
            trader.OTCmlId = OTCmlId;
            trader.Identifier = identifier;
            trader.Scheme = Cst.OTCml_ActorIdentifierScheme;
            //20091021 FI [add sales in invoice] alimentation de trader.name
            trader.Name = name;
            //
            trader.Factor = factor;
            //
            if (pIsSales)
                pInvoiceTrade.Sales[pIndex] = trader;
            else
                pInvoiceTrade.Traders[pIndex] = trader;
        }
        #endregion SetInvoiceTrader
        #endregion Methods
    }
    #endregion InvoicingTrader
    #region InvoicingTradeSimul
    /// <summary>
    /// Classe qui stocke les identifiants des factures en mode simulation (pour suppression avant regénération)
    /// </summary>
    // EG 20110504 Ajout Query de lecture des infos (IDENTIFIER, INSTRUMENT, DEVISE...) de la facture simulée
    public class InvoicingTradeSimul
    {
        #region Members
        public int idT;
        public string tradeIdentifier;
        public int idI;
        public string instrIdentifier;
        public string idC;
        #endregion Members
        #region Constructors
        // EG 20180425 Analyse du code Correction [CA2202]
        public InvoicingTradeSimul(string pCS, int pIdT)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, InvoicingGenMQueue.PARAM_IDT, DbType.Int32), pIdT);

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += @"select tr.IDENTIFIER as TRADE_IDENTIFIER_INVOICE,ns.IDI as IDI_INVOICE,
                        ns.IDENTIFIER as INSTR_IDENTIFIER_INVOICE, ts.IDC as IDC_INVOICE
                        from dbo.TRADE tr
                        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                        inner join dbo.PRODUCT p on (p.IDP = ns.IDP) and (p.GPRODUCT='ADM')
                        inner join dbo.TRADESTREAM ts on (ts.IDT = tr.IDT) and (ts.INSTRUMENTNO = 1) and (ts.STREAMNO = 1) 
                        where tr.IDT = @IDT";

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    idT = pIdT;
                    tradeIdentifier = dr["TRADE_IDENTIFIER_INVOICE"].ToString();
                    instrIdentifier = dr["INSTR_IDENTIFIER_INVOICE"].ToString();
                    idI = Convert.ToInt32(dr["IDI_INVOICE"]);
                    idC = dr["IDC_INVOICE"].ToString();
                }
            }
        }
        #endregion Constructors
    }
    #endregion InvoicingTradeSimul
}

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;

namespace EFS.SpheresRiskPerformance.DataContracts
{
    /// <summary>
    /// Additional risk margining parameters per clearing house 
    /// </summary>
    /// <remarks>Only for actors with role MARGINREQOFFICE and at least a MarginReqOfficeBooksAndParameters instance associated</remarks>
    [DataContract(
        Name = DataHelper<ClearingOrgParameter>.DATASETROWNAME,
        Namespace = DataHelper<ClearingOrgParameter>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "ClearingOrgParameter")]
    public sealed class ClearingOrgParameter : IDataContractEnabled
    {
        /// <summary>
        /// Clearing house internal id, where the parameter is active
        /// </summary>
        [DataMember(Name = "IDA_CSS", Order = 1)]
        public int CssId
        {
            get;
            set;
        }

        /// <summary>
        /// Multiplier ratio affecting the final deposit amount. 
        /// It has the priority in comparison with the same value inside the MarginReqOfficeBooksAndParameters class.
        /// </summary>
        /// <value>when -1 the value does not exist and must not be considered, 
        /// when >= 0 the value is valid and mus replace the other ratios</value>
        /// <remarks>primary data source: CLEARINGORGPARAM table</remarks>
        [DataMember(Name = "IMWEIGHTINGRATIO", Order = 2)]
        public double WeightingRatioDouble
        { get; set; }
        public decimal WeightingRatio
        { get { return (decimal)WeightingRatioDouble; } }

        /// <summary>
        /// SPAN Account type
        /// </summary>
        [DataMember(Name = "SPANACCOUNTTYPE", Order = 3)]
        public SpanAccountType SpanAccountType
        {
            get;
            set;
        }

        /// <summary>
        /// Flag indicator to include the maintenance amount for a deposit calculate with the SPAN method.
        /// When true the amount will be considered.
        /// </summary>
        [DataMember(Name = "ISIMMAINTENANCE", Order = 4)]
        public bool SpanMaintenanceAmountIndicator
        {
            get;
            set;
        }

        /// <summary>
        /// SPAN Scan Risk Offset Cap Percentage's
        /// </summary>
        /// PM 20150930 [21134] Add SCANOFFSETCAPPCT
        [DataMember(Name = "SCANOFFSETCAPPCT", Order = 7)]
        public double ScanRiskOffsetCapPrctDouble
        { get; set; }
        public decimal ScanRiskOffsetCapPrct
        { get { return (decimal)ScanRiskOffsetCapPrctDouble; } }

        /// <summary>
        /// Indique s'il faut calculer un déposit pour les positions en attente de livraison (exercée/assignée) lorsque la méthode de calcul le permet
        /// </summary>
        /// PM 20170106 [22633] Add IsInitialMarginForExeAssPosition
        [DataMember(Name = "ISIMFOREXEASSPOS", Order = 8)]
        public bool IsInitialMarginForExeAssPosition
        { get; set; }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Activation date of the risk parameter
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 9)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Deactivation date of the risk parameter
        /// </summary>
        [DataMember(Name = "DTDISABLED", Order = 10)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// Additional risk margining parameters per market 
    /// </summary>
    /// <remarks>Only for actors with role MARGINREQOFFICE and at least a MarginReqOfficeBooksAndParameters instance associated</remarks>
    [DataContract(
        Name = DataHelper<EquityMarketParameter>.DATASETROWNAME,
        Namespace = DataHelper<EquityMarketParameter>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "EquityMarketParameter")]
    public sealed class EquityMarketParameter : IDataContractEnabled
    {
        /// <summary>
        /// Market internal id, where the parameter is active
        /// </summary>
        [DataMember(Name = "IDM", Order = 1)]
        public int MarketId
        {
            get;
            set;
        }

        /// <summary>
        /// Coverage type/priority for the loaded stocks
        /// </summary>
        [DataMember(Name = "POSSTOCKCOVER", Order = 2)]
        public PosStockCoverEnum StockCoverType
        {
            get;
            set;
        }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Activation date of the risk parameter
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 3)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Deactivation date of the risk parameter
        /// </summary>
        [DataMember(Name = "DTDISABLED", Order = 4)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// Class representing a book enriched with additional risk margining parameters, 
    /// </summary>
    /// <remarks>Only for actors with role MARGINREQOFFICE</remarks>
    [DataContract(
        Name = DataHelper<MarginReqOfficeBooksAndParameters>.DATASETROWNAME,
        Namespace = DataHelper<MarginReqOfficeBooksAndParameters>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "BookWithRiskMarginParameter")]
    public sealed class MarginReqOfficeBooksAndParameters : IDataContractEnabled
    {
        int m_ActorId;

        /// <summary>
        /// Actor internal ID
        /// </summary>
        [DataMember(Name = "ACTORID", Order = 1)]
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        string m_Book;

        /// <summary>
        /// Book identifier
        /// </summary>
        [DataMember(Name = "BOOK", Order = 2)]
        public string Book
        {
            get { return m_Book; }
            set { m_Book = value; }
        }

        int m_BookId;

        /// <summary>
        /// Book internal id
        /// </summary>
        [DataMember(Name = "BOOKID", Order = 3)]
        public int BookId
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        string m_BookName;

        /// <summary>
        /// Book display name
        /// </summary>
        [DataMember(Name = "BOOKNAME", Order = 4)]
        public string BookName
        {
            get { return m_BookName; }
            set { m_BookName = value; }
        }

        int m_IMRBookId;

        /// <summary>
        /// IMR Book Id, id of the book IMR affected by the risk margin total amount, 
        /// which has been computed on all the actor books and actor childs
        /// </summary>
        [DataMember(Name = "IMRBOOKID", Order = 5)]
        public int IMRBookId
        {
            get { return m_IMRBookId; }
            set { m_IMRBookId = value; }
        }

        EfsML.Enum.GlobalElementaryEnum m_AffectAllBooks;

        /// <summary>
        /// When true the risk margin evaluation is computed for each books separately 
        /// and the resulting amount will affect again each book separately.
        /// </summary>
        [DataMember(Name = "AFFECTALLBOOKS", Order = 6)]
        public EfsML.Enum.GlobalElementaryEnum AffectAllBooks
        {
            get { return m_AffectAllBooks; }
            set { m_AffectAllBooks = value; }
        }

        bool m_IsGrossMargining;

        /// <summary>
        /// The risk margin evaluation flag, when true the gross modality will be activated for this actor/book
        /// </summary>
        [DataMember(Name = "ISGROSSMARGINING", Order = 7)]
        public bool IsGrossMargining
        {
            get { return m_IsGrossMargining; }
            set { m_IsGrossMargining = value; }
        }

        decimal m_WeightingRatio;

        /// <summary>
        /// Multiplier ratio affecting the final deposit amount
        /// </summary>
        /// <remarks>primary data source: RISKMARGIN table</remarks>
        [DataMember(Name = "IMWEIGHTINGRATIO", Order = 8)]
        public decimal WeightingRatio
        {
            get { return m_WeightingRatio; }
            set { m_WeightingRatio = value; }
        }

        #region IDataContractEnabled Membres
        /// <summary>
        /// Activation date of the risk parameter
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 9)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Deactivation date of the risk parameter
        /// </summary>
        [DataMember(Name = "DTDISABLED", Order = 10)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// Class de chargement de la valeur des trades
    /// </summary>
    /// PM 20170313 [22833] Ajout
    /// PM 20170808 [23371] Ajout accessors IdAsset, IdCC, Currency et DtSettlement
    [DataContract(Name = DataHelper<TradeValue>.DATASETROWNAME, Namespace = DataHelper<TradeValue>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "Allocation")]
    public class TradeValue
    {
        #region Accessors
        /// <summary>
        /// Id interne de l'acteur dealer
        /// </summary>
        [DataMember(Name = "IDA_DEALER", Order = 1)]
        public int IdADealer
        { get; set; }

        /// <summary>
        /// Id interne du book du dealer, présent sur le trade
        /// </summary>
        [DataMember(Name = "IDB_DEALER", Order = 2)]
        public int IdBDealer
        { get; set; }

        /// <summary>
        /// Id interne de l'acteur clearer
        /// </summary>
        [DataMember(Name = "IDA_CLEARER", Order = 3)]
        public int IdAClearer
        { get; set; }

        /// <summary>
        /// Id interne du book du clearer, présent sur le trade
        /// </summary>
        [DataMember(Name = "IDB_CLEARER", Order = 4)]
        public int IdBClearer
        { get; set; }

        /// <summary>
        /// Marché du trade
        /// </summary>
        [DataMember(Name = "IDM", Order = 5)]
        public int IdM
        { get; set; }

        /// <summary>
        /// Identifiant de l'instrument
        /// </summary>
        [DataMember(Name = "IDI", Order = 6)]
        public int IdI
        { get; set; }

        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 7)]
        public int IdAsset
        { get; set; }

        /// <summary>
        /// Id interne du commodity contract
        /// </summary>
        [DataMember(Name = "IDCC", Order = 8)]
        public int IdCC
        { get; set; }

        /// <summary>
        /// Id interne du trade
        /// </summary>
        [DataMember(Name = "IDT", Order = 9)]
        public int IdT
        { get; set; }

        /// <summary>
        /// Date et heure du trade
        /// </summary>
        [DataMember(Name = "DTTIMESTAMP", Order = 10)]
        public DateTime DtTimestamp
        { get; set; }

        /// <summary>
        /// Date et heure d'execution du trade
        /// </summary>
        /// EG 20171016 [23509] New
        [DataMember(Name = "DTEXECUTION", Order = 11)]
        public DateTime DtExecution
        { get; set; }

        /// <summary>
        /// Gross amount du trade
        /// </summary>
        [DataMember(Name = "VALORISATION", Order = 12)]
        public double ValueDouble
        { get; set; }
        public decimal Value
        { get { return (decimal)ValueDouble; } }

        /// <summary>
        /// Devise du gross amount du trade
        /// </summary>
        [DataMember(Name = "IDC", Order = 13)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Date de réglement
        /// </summary>
        [DataMember(Name = "DTSTL", Order = 14)]
        public DateTime DtSettlement
        { get; set; }

        /// <summary>
        /// Id de la méthode de calcul IMSM uniquement dans le cas des MasterAgreement
        /// </summary>
        // PM 20230104 [26181] Ajout
        [DataMember(Name = "IDIMMETHOD", Order = 15)]
        public int IdIMMethod
        { get; set; }
        #endregion Accessors
    }

    /// <summary>
    /// Class representing a trade allocation
    /// </summary>
    [DataContract(
        Name = DataHelper<TradeAllocation>.DATASETROWNAME,
        Namespace = DataHelper<TradeAllocation>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "Allocation")]
    public class TradeAllocation
    {
        int m_ActorId;

        /// <summary>
        /// Internal id of the actor-dealer of the allocation
        /// </summary>
        [DataMember(Name = "ACTORID", Order = 1)]
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        int m_BookId;

        /// <summary>
        /// Internal id of the book owned by the dealer, where the trade has been registered
        /// </summary>
        [DataMember(Name = "BOOKID", Order = 2)]
        public int BookId
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        int m_TradeId;

        /// <summary>
        /// Allocation internal id
        /// </summary>
        [DataMember(Name = "IDT", Order = 3)]
        public int TradeId
        {
            get { return m_TradeId; }
            set { m_TradeId = value; }
        }

        int m_AssetId;

        /// <summary>
        /// Internal id of the trade asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 4)]
        public int AssetId
        {
            get { return m_AssetId; }
            set { m_AssetId = value; }
        }

        string m_Side;

        /// <summary>
        /// Trade dealer direction (buyer = "1" seller = "2")
        /// </summary>
        [DataMember(Name = "SIDE", Order = 5)]
        public string Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }

        int m_ClearerId;

        /// <summary>
        /// clearer internal id
        /// </summary>
        [DataMember(Name = "CLEARERID", Order = 6)]
        public int ClearerId
        {
            get { return m_ClearerId; }
            set { m_ClearerId = value; }
        }

        int m_BookClearerId;

        /// <summary>
        /// clearer book internal id
        /// </summary>
        [DataMember(Name = "BOOKCLEARERID", Order = 7)]
        public int BookClearerId
        {
            get { return m_BookClearerId; }
            set { m_BookClearerId = value; }
        }

        float m_Quantity;

        /// <summary>
        /// Number of the assets for the trade 
        /// </summary>
        [DataMember(Name = "QTY", Order = 8)]
        public float Quantity
        {
            get { return m_Quantity; }
            set { m_Quantity = value; }
        }

        int m_DerivativeContractId;

        /// <summary>
        /// Asset derivative contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 9)]
        public int DerivativeContractId
        {
            get { return m_DerivativeContractId; }
            set { m_DerivativeContractId = value; }
        }

        string m_DerivativeContractSymbol;

        /// <summary>
        /// Derivative contract symbol
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 10)]
        public string DerivativeContractSymbol
        {
            get { return m_DerivativeContractSymbol; }
            set { m_DerivativeContractSymbol = value; }
        }

        int m_MarketId;

        /// <summary>
        /// Market hosting the derivative contract
        /// </summary>
        [DataMember(Name = "MARKETID", Order = 11)]
        public int MarketId
        {
            get { return m_MarketId; }
            set { m_MarketId = value; }
        }

        /// <summary>
        /// Number of the pending quantity to be settled for the trade (!= 0 for virtual trades representing physical settlement only)
        /// </summary>
        [DataMember(Name = "EXEASSQTY", Order = 12)]
        public float ExeAssQuantity
        {
            get;
            set;
        }

        /// <summary>
        /// Delivery date (not null for virtual trades representing physical settlement only)
        /// </summary>
        [DataMember(Name = "DELIVERYDATE", Order = 13)]
        public DateTime? DeliveryDate
        {
            get;
            set;
        }

        /// <summary>
        /// Settlement date (not null for virtual trades representing physical settlement only)
        /// </summary>
        [DataMember(Name = "SETLLEMENTDATE", Order = 14)]
        public DateTime? SettlementDate
        {
            get;
            set;
        }

        /// <summary>
        /// Date de l'étape courante de livraison
        /// </summary>
        // PM 20130905 [17949] Livraison
        [DataMember(Name = "DELIVERYSTEPDATE", Order = 15)]
        public DateTime? DeliveryStepDate
        {
            get;
            set;
        }

        /// <summary>
        /// Etape courante de livraison
        /// </summary>
        // PM 20130905 [17949] Livraison
        [DataMember(Name = "DELIVERYSTEP", Order = 16)]
        public InitialMarginDeliveryStepEnum DeliveryStep
        {
            get;
            set;
        }

        /// <summary>
        /// Quantité en livraison
        /// </summary>
        // PM 20130905 [17949] Livraison
        [DataMember(Name = "DELIVERYQTY", Order = 17)]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal DeliveryQuantity
        {
            get;
            set;
        }

        /// <summary>
        /// Identifiant de l'instrument
        /// </summary>
        // PM 20170206 [22833] Ajout IDI
        [DataMember(Name = "IDI", Order = 18)]
        public int IdI
        {
            get;
            set;
        }

        /// <summary>
        /// Type d'expression pour déposit de livraison
        /// </summary>
        // PM 20190401 [24625][24387] Ajout DeliveryExpressionType
        [DataMember(Name = "IMEXPRESSIONTYPE", Order = 19)]
        public ExpressionType DeliveryExpressionType
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Class representant un trade allocation du jour courante dont le méthode de déposit est NONE
    /// </summary>
    // PM 20221212 [XXXXX] Ajout
    [DataContract(
        Name = DataHelper<TradeAllocation>.DATASETROWNAME,
        Namespace = DataHelper<TradeAllocation>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "Allocation")]
    public class TradeNoMargin
    {
        /*
                   tr.IDT, tr.IDENTIFIER, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER,
                   tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, tr.DTBUSINESS,
                   tr.IDI, tr.IDM, tr.MARKET_IDENTIFIER, tr.IDASSET, tr.ASSETCATEGORY,
	               tr.SIDE, tr.QTY, tr.PRICE,
				   a.IDC, a.IDDC as IDCONTRACT, a.CONTRACTIDENTIFIER, a.IDENTIFIER as ASSETIDENTIFIER
        */
        #region Members
        int m_IdT;
        string m_TradeIdentifier;
        int m_IdA_Dealer;
        int m_IdB_Dealer;
        int m_IdA_Clearer;
        int m_IdB_Clearer;
        DateTime m_DtTimestamp;
        DateTime m_DtExecution;
        string m_TzFacility;
        DateTime m_DtBusiness;
        int m_IdI;
        int m_IdM;
        string m_MarketIdentifier;
        int m_IdAsset;
        Cst.UnderlyingAsset m_AssetCategory;
        string m_Side;
        decimal m_Quantity;
        decimal m_Price;
        string m_Currency;
        int m_IdContract;
        string m_ContractIdentifier;
        string m_AssetIdentifier;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id interne du trade
        /// </summary>
        [DataMember(Name = "IDT", Order = 1)]
        public int IdT
        {
            get { return m_IdT; }
            set { m_IdT = value; }
        }

        /// <summary>
        /// Identifiant du trade
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string TradeIdentifier
        {
            get { return m_TradeIdentifier; }
            set { m_TradeIdentifier = value; }
        }

        /// <summary>
        /// Id interne de l'acteur dealer, présent sur le trade
        /// </summary>
        [DataMember(Name = "IDA_DEALER", Order = 3)]
        public int IdA_Dealer
        {
            get { return m_IdA_Dealer; }
            set { m_IdA_Dealer = value; }
        }

        /// <summary>
        /// Id interne du book du dealer, présent sur le trade
        /// </summary>
        [DataMember(Name = "IDB_DEALER", Order = 4)]
        public int IdB_Dealer
        {
            get { return m_IdB_Dealer; }
            set { m_IdB_Dealer = value; }
        }

        /// <summary>
        /// Id interne de l'acteur clearer, présent sur le trade
        /// </summary>
        [DataMember(Name = "IDA_CLEARER", Order = 5)]
        public int IdA_Clearer
        {
            get { return m_IdA_Clearer; }
            set { m_IdA_Clearer = value; }
        }

        /// <summary>
        /// Id interne du book du clearer, présent sur le trade
        /// </summary>
        [DataMember(Name = "IDB_CLEARER", Order = 6)]
        public int IdB_Clearer
        {
            get { return m_IdB_Clearer; }
            set { m_IdB_Clearer = value; }
        }

        /// <summary>
        /// Date et heure du trade
        /// </summary>
        [DataMember(Name = "DTTIMESTAMP", Order = 7)]
        public DateTime DtTimestamp
        {
            get { return m_DtTimestamp; }
            set { m_DtTimestamp = value; }
        }

        /// <summary>
        /// Date et heure d'execution du trade
        /// </summary>
        [DataMember(Name = "DTEXECUTION", Order = 8)]
        public DateTime DtExecution
        {
            get { return m_DtExecution; }
            set { m_DtExecution = value; }
        }

        /// <summary>
        /// Timezone de l'heure du trade
        /// </summary>
        [DataMember(Name = "TZFACILITY", Order = 9)]
        public string TzFacility
        {
            get { return m_TzFacility; }
            set { m_TzFacility = value; }
        }

        /// <summary>
        /// Date business du trade
        /// </summary>
        [DataMember(Name = "DTBUSINESS", Order = 10)]
        public DateTime DtBusiness
        {
            get { return m_DtBusiness; }
            set { m_DtBusiness = value; }
        }

        /// <summary>
        /// Identifiant de l'instrument
        /// </summary>
        [DataMember(Name = "IDI", Order = 11)]
        public int IdI
        {
            get { return m_IdI; }
            set { m_IdI = value; }
        }

        /// <summary>
        /// Id interne du Marché du trade
        /// </summary>
        [DataMember(Name = "IDM", Order = 12)]
        public int IdM
        {
            get { return m_IdM; }
            set { m_IdM = value; }
        }

        /// <summary>
        /// Marché du trade
        /// </summary>
        [DataMember(Name = "MARKET_IDENTIFIER", Order = 13)]
        public string MarketIdentifier
        {
            get { return m_MarketIdentifier; }
            set { m_MarketIdentifier = value; }
        }

        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 14)]
        public int IdAsset
        {
            get { return m_IdAsset; }
            set { m_IdAsset = value; }
        }

        /// <summary>
        /// Type d'asset (Asset catégorie) 
        /// </summary>
        [DataMember(Name = "ASSETCATEGORY", Order = 15)]
        public string AssetCategoryString
        { get; set; }
        public Cst.UnderlyingAsset AssetCategory
        {
            get { m_AssetCategory = ReflectionTools.ConvertStringToEnumOrDefault<Cst.UnderlyingAsset>(AssetCategoryString, Cst.UnderlyingAsset.Commodity); return m_AssetCategory; }
            set { m_AssetCategory = value; AssetCategoryString = m_AssetCategory.ToString(); }
        }

        /// <summary>
        /// Sens du trade pour le dealer (buyer = "1" seller = "2")
        /// </summary>
        [DataMember(Name = "SIDE", Order = 16)]
        public string Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }

        /// <summary>
        /// Quantity d'asset du trade 
        /// </summary>
        [DataMember(Name = "QTY", Order = 17)]
        public double QuantityDouble
        { get; set; }
        public decimal Quantity
        {
            get { m_Quantity = (decimal)QuantityDouble; return m_Quantity; }
            set { m_Quantity = value; QuantityDouble = (double)m_Quantity; }
        }

        /// <summary>
        /// Price de négociation du trade 
        /// </summary>
        [DataMember(Name = "PRICE", Order = 18)]
        public double PriceDouble
        { get; set; }
        public decimal Price
        {
            get { m_Price = (decimal)PriceDouble; return m_Price; }
            set { m_Price = value; PriceDouble = (double)m_Price; }
        }

        /// <summary>
        /// Devise
        /// </summary>
        [DataMember(Name = "IDC", Order = 19)]
        public string Currency
        {
            get { return m_Currency; }
            set { m_Currency = value; }
        }

        /// <summary>
        /// Id Interne du contrat
        /// </summary>
        [DataMember(Name = "IDCONTRACT", Order = 20)]
        public int IdContract
        {
            get { return m_IdContract; }
            set { m_IdContract = value; }
        }

        /// <summary>
        /// Identifiant du contrat
        /// </summary>
        [DataMember(Name = "CONTRACTIDENTIFIER", Order = 21)]
        public string ContractIdentifier
        {
            get { return m_ContractIdentifier; }
            set { m_ContractIdentifier = value; }
        }

        /// <summary>
        /// Identifiant de l'asset
        /// </summary>
        [DataMember(Name = "ASSETIDENTIFIER", Order = 22)]
        public string AssetIdentifier
        {
            get { return m_AssetIdentifier; }
            set { m_AssetIdentifier = value; }
        }
        #endregion Accessors
    }
}

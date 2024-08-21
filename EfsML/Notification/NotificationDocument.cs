using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFS.ACommon;

using FpML.Interface;
using EfsML.Enum;
using FixML.Enum;
using EFS.GUI.Interface;

namespace EfsML.Notification
{

    /// <summary>
    /// Permet d'identifier la notification
    /// </summary>
    public partial class NotificationId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI", AttributeName = "confirmationMessageIdScheme")]
        public string notificationIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationConfirmationSystemIds
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("notificationConfirmationSystemId", typeof(NotificationConfirmationSystemId), Order = 1)]
        public NotificationConfirmationSystemId[] notificationConfirmationSystemId;
        #endregion Members
    }

    /// <summary>
    /// Permet d'identifier le NCS (canal de communication)
    /// </summary>
    public partial class NotificationConfirmationSystemId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string notificationConfirmationSystemIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
	/// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class ReportTradeSortKey : ISchemeId
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string scheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members

        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        #endregion Accessors

        #region Constructors
        public ReportTradeSortKey()
        {
            this.scheme = "http://www.efs.org/2007/ReportSortEnum";
        }
        #endregion Constructors

        #region ISchemeId Members
        string ISchemeId.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
        #endregion ISchemeId Members

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.scheme = value; }
            get { return this.scheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ReportTradeSortKeys
    {

        [System.Xml.Serialization.XmlElementAttribute("key", Order = 1)]
        public ReportTradeSortKey[] key;

        #region constructor
        public ReportTradeSortKeys()
        {
        }
        #endregion
    }

    /// <summary>
    /// Représente le tri qui doit être appliqué pour afficher le trade
    /// </summary>
    public partial class ReportTradeSort
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("keys", Order = 1)]
        public ReportTradeSortKeys keys;
        //[System.Xml.Serialization.XmlElementAttribute("groups", Order = 2)]
        //public InvoiceTradeSortGroups groups;

        #region constructor
        public ReportTradeSort()
        {
            //this.groups = new InvoiceTradeSortGroups();
            this.keys = new ReportTradeSortKeys();
        }
        #endregion
    }

    /// <summary>
    ///  Liste des positions synthétiques à une date
    /// </summary>
    /// FI 20150427 [20987] Add class
    public partial class PosSynthetics
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150427 [20987] Add
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        ///  Liste des positions synthétiques
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("posSynthetic")]
        public List<PosSynthetic> posSynthetic;
        #endregion Members
    }

    /// <summary>
    /// Représente une position synthétiques (version de document >= 3.1)
    /// </summary>
    /// FI 20150408 [XXPOC] Modify
    /// FI 20150709 [XXXXX] Modify
    /// EG 20150920 [21374] Modify
    /// FI 20170201 [21916] Modify
    public partial class PosSynthetic
    {
        /// <summary>
        ///  Représente la clé de position 
        /// </summary>
        /// FI 20150113 [20672]
        [System.Xml.Serialization.XmlIgnore]
        public PositionKey posKey;

        /// <summary>
        /// Identifiant non significatif du book dealer 
        /// </summary>
        /// FI 20150113 [20672] Modify ce membre devient une property
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idb")]
        public int Idb
        {
            get { return posKey.idB_Dealer; }
            set { posKey.idB_Dealer = value; }
        }

        /// <summary>
        /// Identifiant non significatif de l'instruement
        /// </summary>
        /// FI 20150408 [XXPOC] Add idI
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idI")]
        public int IdI
        {
            get { return posKey.idI; }
            set { posKey.idI = value; }
        }


        /// <summary>
        /// Identifiant non significatif de l'asset
        /// </summary>
        /// FI 20150113 [20672] Modify ce membre devient une property
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int IdAsset
        {
            get { return posKey.idAsset; }
            set { posKey.idAsset = value; }
        }

        /// <summary>
        /// type d'asset
        /// </summary>
        /// FI 20140820 [20275] Add Member
        /// le nom assetCategory est choisi pour la serailization car il existe déjà ce même nom d'attribut DerivativeContractRepository
        /// FI 20150113 [20672] Modify ce membre devient une property
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "assetCategory")]
        public Cst.UnderlyingAsset AssetCategory
        {
            get { return posKey.assetCategory; }
            set { posKey.assetCategory = value; }
        }

        /// <summary>
        /// sens de la position 
        /// <para>1 pour Long</para> 
        /// <para>2 pour Short</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "side")]
        public int side;

        /// <summary>
        /// Quantité en position
        /// </summary>
        /// EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "dailyQty")]
        public decimal qty;


        /// <summary>
        /// Quantité en position (formatée)
        /// </summary>
        /// FI 20170201 [21916] Add 
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtDailyQty")]
        public string fmtQty;


        /// <summary>
        ///  Prix moyen
        /// <para>Prix exprimé en base 100</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "avgPrice")]
        public decimal avgPrice;

        /// <summary>
        ///  Prix moyen formaté
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtAvgPrice")]
        public string fmtAvgPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean closingPriceSpecified;
        /// <summary>
        /// Prix de cloture de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "clrPx")]
        public decimal closingPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fmtClosingPriceSpecified;
        /// <summary>
        /// Prix de cloture formaté de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtClrPx")]
        public string fmtClosingPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean deltaSpecified;

        /// <summary>
        /// delta associé au prix de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "delta")]
        public decimal delta;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean lovCurrencySpecified;
        /// <summary>
        ///  Devise de la valeur liquidative de la position (si option )
        /// <para>Renseigné uniquement sur les documents de version=3.0</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lovCurrency")]
        public string lovCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean lovAmountSpecified;

        /// <summary>
        ///  Valeur liquidative de la position (si option )
        /// <para>Renseigné uniquement sur les documents de version=3.0</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lovAmount")]
        public decimal lovAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean lovSpecified;
        /// <summary>
        /// Liquidation Option Value (si option)
        /// <para>Renseigné uniquement sur les documents de version>=3.1</para>
        /// </summary>
        /// FI 20150218 [20275] => utilisation de nov ( nov pour Net Option Value est la somme des Long Option Value et Short Option Value)
        [System.Xml.Serialization.XmlElementAttribute("nov")]
        public ReportAmountSide lov;

        /// RD 20131018 [18600] Chargement du montant UMG            
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean umgSpecified;
        /// <summary>
        /// Unrealized Margin (Résulat potentiel)
        /// <para>Renseigné uniquement sur les documents de version>=3.1</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("umg")]
        public ReportAmountSide umg;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fdaSpecified;
        /// <summary>
        /// Funding Amount (Montant de rémunération )
        /// <para>Renseigné uniquement sur les documents de version>=3.1</para>
        /// </summary>
        ///FI 20150127 [20275] Add
        [System.Xml.Serialization.XmlElementAttribute("fda")]
        public List<ReportAmountSideFundingAmount> fda;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean bwaSpecified;
        /// <summary>
        /// Borrowing Amount
        /// <para>Renseigné uniquement sur les documents de version>=3.1</para>
        /// </summary>
        ///FI 20150324 [XXPOC] Add
        [System.Xml.Serialization.XmlElementAttribute("bwa")]
        public List<ReportAmountSideBorrowingAmount> bwa;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skpSpecified;
        /// <summary>
        /// représente les frais de garde lié à la position
        /// </summary>
        /// FI 20150709 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("skp")]
        public List<ReportFeeLabel> skp;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool positionUtiRefSpecified;
        /// <summary>
        /// Représente les Position Unique Trade Identifier (côté Dealer et côté Clearer)
        /// </summary>
        /// FI 20150113 [20672]
        [System.Xml.Serialization.XmlElement("puti")]
        public PositionUTIReference positionUtiRef;

    }

    /// <summary>
    /// Représente les trades en position
    /// </summary>
    /// FI 20150427 [20987] Modify
    public partial class PosTrades
    {
        #region Members

        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150427 [20987] Add
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des trades en position
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public List<PosTrade> trade;

        /// <summary>
        /// Prix moyens des achats/ventes
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("subTotal")]
        public List<PositionSubTotal> subTotal;
        #endregion Members
    }

    /// <summary>
    /// Représente un trade en position 
    /// </summary>
    /// FI 20150218 [20275] 
    /// FI 20150331 [XXPOC] PosTrade herite de PosTradeCommon
    // EG 20190730 New element AssetMeasure (associated with closingPrice to determine measure; CleanPrice|DirtyPrice)
    public partial class PosTrade : PosTradeCommon
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetMeasureClosingPriceSpecified;
        /// <summary>
        /// assetMeasure du prix de clôture de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("meaClrPx")]
        public AssetMeasureEnum assetMeasureClosingPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool closingPriceSpecified;
        /// <summary>
        /// prix de clôture de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("clrPx")]
        public Decimal closingPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fmtClosingPriceSpecified;
        /// <summary>
        /// prix de clôture formaté de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("fmtClrPx")]
        public string fmtClosingPrice;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean umgSpecified;
        /// <summary>
        ///  Unrealized Margin
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("umg")]
        public ReportAmountSide umg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean novSpecified;
        /// <summary>
        /// Liquidation Option Value
        /// </summary>
        /// FI 20150218 [20275] => utilisation de nov ( nov pour Net Option Value est la somme des Long Option Value et Short Option Value)
        [System.Xml.Serialization.XmlElementAttribute("nov")]
        public ReportAmountSide nov;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean prmSpecified;
        /// <summary>
        /// Prime
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("prm")]
        public ReportAmountSide prm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean mkvSpecified;
        /// <summary>
        /// Market value 
        /// </summary>
        /// FI 20140821 [20275] Add property
        [System.Xml.Serialization.XmlElementAttribute("mkv")]
        public ReportAmountSide mkv;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean ainSpecified;
        /// <summary>
        /// Intérêts courus
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("ain")]
        public ReportAmountAccruedInterest ain;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean pamSpecified;
        /// <summary>
        /// Principal amount (ie qty x SecurityNominal x closing price) 
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("pam")]
        public ReportAmountSide pam;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean mgrSpecified;
        /// <summary>
        /// Margin Requirement
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("mgr")]
        public ReportAmountSideRatio mgr;

    }

    /// <summary>
    /// Représente des sous-totaux par  position
    /// <para>
    /// Prix moyen des achats et prix moyens des ventes pour une position
    /// </para>
    /// </summary>
    /// FI 20150218 [20275] Add
    public partial class PositionSubTotal
    {
        /// <summary>
        /// Identifiant non significatif du book (clé du regroupement)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idB")]
        public int idb;

        /// <summary>
        /// Identifiant non significatif de l'instrument (clé du regroupement)
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idI")]
        public int idI;

        /// <summary>
        /// Identifiant non significatif de l'asset (clé du regroupement)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        /// <summary>
        /// type d'asset (clé du regroupement)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "assetCategory")]
        public Cst.UnderlyingAsset assetCategory;

        /// <summary>
        /// Prix moyen des achats
        /// </summary>
        [System.Xml.Serialization.XmlElement("long")]
        public AveragePrice @long;

        /// <summary>
        /// Prix moyen des ventes
        /// </summary>
        [System.Xml.Serialization.XmlElement("short")]
        public AveragePrice @short;

        /// <summary>
        /// Classe de calcul des moyennes
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ConvertedPrices builder;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean predicateSpecified;

        /// <summary>
        /// Filtre en place lors du calcul des sous-totaux
        /// </summary>
        // FI 20150218 [20275] Add 
        [System.Xml.Serialization.XmlArray("predicates")]
        [System.Xml.Serialization.XmlArrayItem("predicate", typeof(Predicate))]
        public Predicate[] predicate;
    }

    /// <summary>
    /// Représente un filtre
    /// </summary>
    /// FI 20150218 [20275] Add 
    public partial class Predicate
    {
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "trdTyp")]
        public TrdTypeEnum trdType;
    }

    /// <summary>
    /// Représente un prix moyen
    /// </summary>
    /// FI 20150218 [20275] Add 
    /// EG 20150920 [21374] Int (int32) to Long (Int64) 
    /// FI 20170201 [21916] Modify 
    public partial class AveragePrice
    {
        /// <summary>
        /// Somme des qté prises en considération
        /// </summary>
        /// EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qty")]
        public decimal qty;

        /// <summary>
        /// Somme des qté prises en considération (formatée)
        /// </summary>
        /// FI 20170201 [21916] Add 
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtQty")]
        public string fmtQty;

        /// <summary>
        /// Prix moyen
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "avgPx")]
        public Decimal avgPx;

        /// <summary>
        /// Prix moyen formaté
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtAvgPx")]
        public String fmtAvgPx;

        public AveragePrice()
        {
            avgPx = Decimal.Zero;
            fmtAvgPx = "0";
        }
    }

    /// <summary>
    /// Représente de données complémentaires des trades et assets
    /// </summary>
    public partial class CommonData
    {
        #region Members
        /// <summary>
        /// Données communes des trades (Statut Alloc)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public List<CommonTradeAlloc> trade;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool positionUTISpecified;

        /// <summary>
        /// Liste des UTIs de position
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("puti")]
        public List<PositionUTIId> positionUTI;

        #endregion Members
    }

    /// <summary>
    /// Données communes sur les Trades de type alloc
    /// </summary>
    /// FI 20141218 [20574] CommonTradeAlloc herite de CommonTrade
    public partial class CommonTradeAlloc : CommonTrade
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool utiSpecified;
        /// <summary>
        /// Représente les Unique Trade Identifier (côté Dealer et côté Clearer)
        /// </summary>
        [System.Xml.Serialization.XmlElement("uti")]
        public UTIAlloc uti;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool positionUtiRefSpecified;
        /// <summary>
        /// Représente les Position Unique Trade Identifier (côté Dealer et côté Clearer)
        /// </summary>
        [System.Xml.Serialization.XmlElement("puti")]
        public PositionUTIReference positionUtiRef;
    }

    /// <summary>
    /// Données communes sur les Trades
    /// </summary>
    // FI 20150116 [XXXXX] Modify
    // FI 20150218 [20275] Modify
    // FI 20150522 [20275] Modify
    // FI 20161214 [21916] Modify
    // FI 20170116 [21916] Modify
    // EG 20171025 [23509] Add dlvTimezone
    public partial class CommonTrade : TradeIdentification
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool frontIdSpecified;
        /// <summary>
        /// Représente la référence Front du trade (côté dealer)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("frontId", DataType = "normalizedString")]
        public string frontId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool folderIdSpecified;
        /// <summary>
        /// Représente la référence dossier du trade (côté dealer)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("folderId", DataType = "normalizedString")]
        public string folderId;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool orderIdSpecified;
        /// <summary>
        /// Représente la référence de l'ordre trade (côté dealer)
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlAttributeAttribute("orderId", DataType = "normalizedString")]
        public string orderId;

        /// <summary>
        /// Transaction/Execution date au format YYYY-MM-DD (Timezone de la plateforme)
        /// </summary>
        /// FI 20150116 [XXXXX] Add
        [System.Xml.Serialization.XmlAttributeAttribute("trdDt", DataType = "normalizedString")]
        public string trdDt;

        /// <summary>
        /// Transaction/Execution Timestamp au format HH:MM:SS (Timezone de la plateforme)
        /// </summary>
        /// FI 20150513 [XXXXX] Add 
        [System.Xml.Serialization.XmlAttributeAttribute("trdTm", DataType = "normalizedString")]
        public string trdTm;

        /// <summary>
        /// Order entered datetime au format YYYY-MM-DD (Timezone de la plateforme)
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlAttributeAttribute("ordDt", DataType = "normalizedString")]
        public string ordDt;

        /// <summary>
        /// Order entered datetime au format YYYY-MM-DD (Timezone de la plateforme)
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlAttributeAttribute("ordTm", DataType = "normalizedString")]
        public string ordTm;
        
        /// <summary>
        /// Timezone de la plateforme 
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlAttributeAttribute("tzdbid", DataType = "normalizedString")]
        public string tzFacility;

        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150116 [XXXXX] Add
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valDtSpecified;
        /// <summary>
        /// Value Date au format YYYY-MM-DD
        /// <para>- Date de règlement sur les options de change (((IFxOptionBase)product.product).valueDate.Value) </para>
        /// <para>- Date de valeur sur le change à terme  (valDt = ((IFxLeg)product.product).fxDateValueDate.Value;)  </para>
        /// </summary>
        /// FI 20150116 [XXXXX] Add
        [System.Xml.Serialization.XmlAttributeAttribute("valDt", DataType = "normalizedString")]
        public string valDt;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stlDtSpecified;
        /// <summary>
        /// Settlement Date au format YYYY-MM-DD
        /// <para>- Date de règlement du GrosAmount sur les Equity et DebtSecurityTransaction</para>
        /// <para>- Date de règlement de la prime sur les options de change </para>
        /// </summary>
        /// FI 20150702 [XXXXX] Add
        [System.Xml.Serialization.XmlAttributeAttribute("stlDt", DataType = "normalizedString")]
        public string stlDt;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expDtSpecified;
        /// <summary>
        /// Expiration Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150522 [20275] Add
        [System.Xml.Serialization.XmlAttributeAttribute("expDt", DataType = "normalizedString")]
        public string expDt;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expIndSpecified;

        /// <summary>
        /// Indicateur de proximité d'échéance.  
        /// <para>Cet indicateur contient le nombre de jour avant l'échéance lorsqu'un trade est proche de l'échéance</para>
        /// </summary>
        /// FI 20150522 [20275] Add
        [System.Xml.Serialization.XmlAttributeAttribute("expInd")]
        public int expInd;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastPriceSpecified;
        /// <summary>
        /// Prix de négo
        /// </summary>
        /// FI 20150218 [20275]
        [System.Xml.Serialization.XmlAttributeAttribute("lastPx")]
        public decimal lastPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fmtLastPriceSpecified;
        /// <summary>
        /// Prix de négo converti/formaté
        /// </summary>
        /// FI 20150218 [20275]
        [System.Xml.Serialization.XmlAttributeAttribute("fmtLastPx")]
        public string fmtLastPrice;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dlvStartSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        [System.Xml.Serialization.XmlAttributeAttribute("dlvStart")]
        public string dlvStart;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dlvEndSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        [System.Xml.Serialization.XmlAttributeAttribute("dlvEnd")]
        public string dlvEnd;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dlvTimezoneSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute("dlvTimezone")]
        public string dlvTimezone;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAssetSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Add
        [System.Xml.Serialization.XmlAttributeAttribute("idAsset")]
        public int idAsset;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCategoryIdSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Add
        [System.Xml.Serialization.XmlAttributeAttribute("assetCategory")]
        public string assetCategory;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool traderSpecified;
        /// <summary>
        /// Trader présents sur le trade
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlElementAttribute("trader")]
        public List<TraderReport> trader;
    }

    /// <summary>
    /// Repérente les UTIs d'un trade alloc ou d'une position
    /// </summary>
    /// FI 20141218 [20574] Add Class
    public class UTIAlloc
    {
        /// <summary>
        /// Représente l'uti Dealer
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("dealer", DataType = "normalizedString")]
        public string utiDealer;
        /// <summary>
        /// Représente l'uti Clearer
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("clearer", DataType = "normalizedString")]
        public string utiClearer;
    }

    /// <summary>
    /// Représente les actions sur positions à une date
    /// </summary>
    /// FI 20150427 [20987] Modify
    public partial class PosActions
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150427 [20987] Add
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des actions sur positions en Business Date
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("posAction")]
        public List<PosAction> posAction;
        #endregion Members
    }

    /// <summary>
    /// Représente une action sur Position
    /// </summary>
    /// FI 20150218 [20275]
    /// EG 20150920 [21374] Int (int32) to Long (Int64) 
    /// FI 20151019 [21317] Modify 
    public partial class PosAction
    {
        #region members
        /// <summary>
        /// Identifiant non significatif de l'action 
        /// <para>Permet de remonter sur les évènements générés par cette action</para>
        /// <para>En cas de décompensation, conteint IdPosActionDet de la compensation</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("idPADet")]
        public int IdPosActionDet;

        /// <summary>
        /// Type d'action 
        /// <para>OptionAbandon,AutomaticOptionExercise,Closure,ClearingEndOfDay,MaturityOffsettingFuture,PositionCancelation etc...</para>
        /// <para>En cas de décompensation contient requestType de la compensation</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("requestType")]
        public Cst.PosRequestTypeEnum requestType;

        /// <summary>
        /// Intraday, EndOfDay
        /// <para>En cas de décompensation, contient le requestMode de la compensation</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("requestMode")]
        public SettlSessIDEnum requestMode;

        /// <summary>
        /// Date de l'action 
        /// <para>En cas de décompensation, contient la date business initiale</para>
        /// <para>Date au format ISO</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("dtBusiness")]
        public string dtBusiness;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtUnclearingSpecified;

        /// <summary>
        /// Date de décompensation (renseignée uniquement si décompensation)
        /// <para>Date au format ISO</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("dtUnclearing")]
        public string dtUnclearing;

        /// <summary>
        /// Quantité associé à l'action
        /// <para>En cas de décompensation, contient la quantité de la compensation initiale</para>
        /// </summary>
        ///  EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlAttributeAttribute("qty")]
        public decimal qty;
        
        /// <summary>
        /// Quantité associé à l'action (formatée)
        /// </summary>
        /// FI 20170201 [21916] Add 
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtQty")]
        public string fmtQty;


        /// <summary>
        /// trades impliqués par l'action
        /// <para>En cas de décompensation, contient les trades de la compensation</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trades")]
        public PosActionTrades trades;

        /// <summary>
        /// Date système de l'action
        /// <para>Permet d'afficher les actions dans l'ordre chronologique</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("dtSys")]
        public DateTime dtSys;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool underlyerPriceSpecified;

        /// <summary>
        /// <para>prix du sous jacent si EXE ou ASS</para>
        /// <para>Prix de clôture du future si Liquidation de future</para>
        /// <para>prix exprimé en base 100</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("unlPx")]
        public Decimal underlyerPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fmtUnderlyerPriceSpecified;
        /// <summary>
        ///<para>prix du sous jacent formaté</para> 
        /// <para>Prix de clôture du future si Liquidation de future</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("fmtUnlPx")]
        public string fmtUnderlyerPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rmgSpecified;
        /// <summary>
        /// Realized Margin
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("rmg")]
        public ReportAmountSide rmg;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scuSpecified;
        /// <summary>
        /// Settlement currency
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("scu")]
        public ReportAmountSide scu;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool prmSpecified;
        /// <summary>
        /// Premium
        /// <para>POC => Restitution de prime</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("prm")]
        public ReportAmountSide prm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gamSpecified;
        /// <summary>
        /// GrossAmount
        /// <para>POC => Restitution du gross Aamount</para>
        /// </summary>
        /// FI 20150827 [21287] add gam
        [System.Xml.Serialization.XmlElementAttribute("gam")]
        public ReportAmountSide gam;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pamSpecified;
        /// <summary>
        /// Principal Amount
        /// <para>POC => Restitution du principal amount (le GAM intègre déjà ce montant)</para>
        /// </summary>
        /// FI 20151019 [21317] add pam
        [System.Xml.Serialization.XmlElementAttribute("pam")]
        public ReportAmountSide pam;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean ainSpecified;
        /// <summary>
        /// Accrued Interest
        /// <para>POC => Restitution du intérêts courus (le GAM intègre déjà ce montant)</para>
        /// </summary>
        /// FI 20151019 [21317] add ain
        [System.Xml.Serialization.XmlElementAttribute("ain")]
        public ReportAmountSide ain;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeSpecified;
        /// <summary>
        /// représente les frais rattachés à l'action
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fee")]
        public ReportFee[] fee;

        #endregion


    }

    /// <summary>
    /// Représente les trades impliqués dans une action
    /// </summary>
    public partial class PosActionTrades
    {
        #region members
        /// <summary>
        /// Représente le trade qui subit l'action (Exercice, Assignation, Liquidation, Correction,...)
        /// <para>Représente le trade clôturant lorsqu'il y a clôture/compensation/décompensation</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public PosActionTrade trade;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool trade2Specified;
        /// <summary>
        /// <para>Représente le trade clôturé lorsqu'il y a clôture/compensation/décompensation</para> 
        /// <para>Représente le trade résultat d'un exercice/assignation, d'une liquidation de future, d'un transfert</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trade2")]
        public PosActionTrade trade2;

        #endregion
    }

    /// <summary>
    /// Représente un trade impliqué dans une action sur position
    /// </summary>
    /// FI 20150522 [20275] Modify
    /// FI 20151019 [21317] Modify 
    public class PosActionTrade : TradeIdentification
    {

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string gProduct;

        /// <summary>
        /// Famille du produit
        /// </summary>
        /// FI 20151019 [21317] Modify
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string family;


        /// <summary>
        ///  Identifiant non significatif de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int idAsset;

        /// <summary>
        ///  type d'asset
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Cst.UnderlyingAsset assetCategory;

        /// <summary>
        /// Prix de négo 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public decimal price;


        /// <summary>
        /// Date Expity du trade (Renseigné uniquement si TRADEINSTUEMENT.DTOUTADJ != Datetime.MaxValue)
        /// </summary>
        /// FI 20150522 [20275] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Nullable<DateTime> dtOut;
    }

    /// <summary>
    /// Représente l'identification d'un trade
    /// </summary>
    /// FI 20151016 [21458] Modify
    public partial class TradeIdentification
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }

        #region Members
        /// <summary>
        /// Représente l'idT du trade
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        /// <summary>
        /// Représente l'identifiant du trade
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("tradeIdentifier", DataType = "normalizedString")]
        public string tradeIdentifier;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameSpecified;

        /// <summary>
        /// Représente le displayName du trade
        /// </summary>
        /// FI 20151016 [21458] Add
        [System.Xml.Serialization.XmlAttributeAttribute("displayName", DataType = "normalizedString")]
        public string displayName;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;

        /// <summary>
        /// Représente la descriptio du trade du trade
        /// </summary>
        /// FI 20151016 [21458] Add
        [System.Xml.Serialization.XmlAttributeAttribute("description", DataType = "normalizedString")]
        public string description;


        #endregion

    }
    
    /// <summary>
    /// Représente un trader
    /// </summary>
    /// FI 20190515 [23912] Add
    public partial class TraderReport
    {
        /// <summary>
        /// IdA du trader
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }

        #region Members
        /// <summary>
        /// Représente l'idA de l'acteur trader
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// Représente l'identifiant du trader
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("traderId", DataType = "normalizedString")]
        public string traderIdentifier;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool partyReferenceSpecified;
        /// <summary>
        /// Acteur auquel l'acteur trader est associé
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("partyRelativeTo", Order = 1)]
        public FpML.v44.Shared.PartyReference partyReference;
        #endregion Members
    }

    /// <summary>
    /// Représente les trades 
    /// </summary>
    /// FI 20150427 [20987] Modify
    /// FI 20150623 [21149] Rename class
    public partial class TradesReport
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150427 [20987] Add
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des trades négociés
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public List<TradeReport> trade;

        /// <summary>
        /// Prix moyens des achats/ventes
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("subTotal")]
        public List<PositionSubTotal> subTotal;
        #endregion Members
    }

    /// <summary>
    /// Représente un trade
    /// </summary>
    /// FI 20150331 [XXPOC] hérite de PosTradeCommon
    /// FI 20150623 [21149] Rename class
    /// FI 20151019 [21317] Modify 
    public partial class TradeReport : PosTradeCommon
    {
        /// <summary>
        /// Date de transaction
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime dtTransac;

        /// <summary>
        /// Date Business
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime dtBusiness;


        /// <summary>
        /// Date de règlement (devrait, sauf exception, être égale à la date de « Transaction » + 2 JO)
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DateTime dtSettlt;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "trdTyp")]
        public TrdTypeEnum trdType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean trdSubTypeSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public TrdSubTypeEnum trdSubType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean posResultSpecified;
        /// <summary>
        ///  Représente un indicateur qui signale si le trade clôture ou non une position déjà existante
        ///  <para>○ "Open" : si le trade ouvre une nouvelle position </para>
        ///  <para>○ "Close ou Partial Close" : si le trade clôture une position déjà existante</para>
        ///  <para>○ "Unavailable" : si aucune mise à jour de position en date Business (Ce cas n'est généralement pas possible puisque les reports sont normalement exécutés après un traitement EOD)</para>
        /// </summary>
        /// FI 20140915 [20275] add posResult
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "posResult")]
        public string posResult;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean prmSpecified;
        /// <summary>
        /// Prime pour un trade option
        /// </summary>
        // FI 20150331 [XXPOC] prm est de type ReportAmountSideSettlementDate
        [System.Xml.Serialization.XmlElementAttribute("prm")]
        public ReportAmountSideSettlementDate prm;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean gamSpecified;
        /// <summary>
        /// GrossAmount sur Equity Security Transaction
        /// </summary>
        /// FI 20150316 [20275] add gam
        [System.Xml.Serialization.XmlElementAttribute("gam")]
        public ReportAmountSideSettlementDate gam;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean netSpecified;
        /// <summary>
        /// NetAmount sur Equity Security Transaction (NET = GAM + FEE)
        /// </summary>
        /// FI 20150702 [XXXXX] add net
        [System.Xml.Serialization.XmlElementAttribute("net")]
        public ReportAmountSideSettlementDate net;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean feeSpecified;
        /// <summary>
        /// représente les frais
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fee")]
        public ReportFee[] fee;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean tradeSrcSpecified;
        /// <summary>
        /// Trade source lorsque le trade est issu d'un transfert interne
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlElementAttribute("tradeSrc")]
        public TradeIdentification tradeSrc;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean ainSpecified;
        /// <summary>
        /// Accrued Interest
        /// <para>Montant des intérêts courus (renseigné uniquement sur les trades DSE)</para>
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("ain")]
        public ReportAmountAccruedInterest ain;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean pamSpecified;
        /// <summary>
        /// Principal Amount
        /// <para>Montant = qté*prix (Renseigné uniquement sur les trades DSE et si CleanPrice)</para>
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("pam")]
        public ReportAmountSide pam;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean umgSpecified;
        /// <summary>
        /// Unrealized Margin (Résulat potentiel)
        /// <para>Renseigné uniquement sur les documents de version>=3.1</para>
        /// </summary>
        ///FI 20150716 [20892] Add (utilisé sur les unsettledTrades) 
        [System.Xml.Serialization.XmlElementAttribute("umg")]
        public ReportAmountSide umg;
    }

    /// <summary>
    ///  Représente un frais (un payment plus des taxes)
    /// </summary>
    /// FI 20150128 [20275] Modify Class (Herite désormais de ReportPayment)
    public partial class ReportFee : ReportPayment
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean taxSpecified;
        /// <summary>
        /// Représente les taxes
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tax", Order = 1)]
        public ReportTaxAmount[] tax;
    }

    /// <summary>
    ///  Représente un frais (un payment plus des taxes) avec possibilité d'ajouter une description
    /// </summary>
    /// FI 201507090 [XXXXX] Add 
    public partial class ReportFeeLabel : ReportFee
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean labelSpecified;
        /// <summary>
        /// Description du frais 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lbl")]
        public string label;
    }
    
    /// <summary>
    ///  Représente un payment
    ///  <para>Un montant, une devise, en crédit ou débit et un éventuel paymentType </para>
    /// </summary>
    /// FI 20150128 [20275] Add Class
    /// FI 20150709 [XXXXX] Modify
    public partial class ReportPayment : ReportAmountSide
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean paymentTypeSpecified;

        /// <summary>
        /// paymentType 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "paymentType")]
        public string paymentType;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean eventTypeSpecified;

        /// <summary>
        /// eventType 
        /// </summary>
        /// FI 20150709 [XXXXX] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "eventType")]
        public string eventType;

    }

    /// <summary>
    /// un montant, une devise
    /// </summary>
    /// FI 20140808 [XXXXX] Modify
    public partial class ReportAmount
    {
        /// <summary>
        /// Valeur du montant
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "amt")]
        public Decimal amount;

        /// <summary>
        /// devise
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "ccy")]
        public String currency;
    }

    /// <summary>
    /// un montant, une devise, en crédit ou débit
    /// </summary>
    /// FI 20140808 [XXXXX] Modify
    public partial class ReportAmountSide : ReportAmount
    {

        /// FI 20141208 [XXXXX] add property
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean sideSpecified;

        /// <summary>
        /// Sens 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "side")]
        public CrDrEnum side;

    }

    /// <summary>
    /// un montant, une devise, en crédit ou débit, et une date de rglt (facultative)
    /// </summary>
    /// FI 20150316 [20275] Add
    public partial class ReportAmountSideSettlementDate : ReportAmountSide
    {

        [System.Xml.Serialization.XmlIgnore]
        public Boolean stlDtSpecified;

        /// <summary>
        /// Date au format YYYY-MM-DD
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("stlDt", DataType = "normalizedString")]
        public string stlDt;
    }

    /// <summary>
    /// un montant, une devise, en crédit ou débit et le facteur utilisé pour obtenir ce montant
    /// </summary>
    /// FI 20140821 [20275] Add classe
    public partial class ReportAmountSideRatio : ReportAmountSide
    {
        /// <summary>
        /// Valeur du facteur
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "factor")]
        public Decimal factor;
    }

    /// <summary>
    /// Un montant d'intérêts courus avec le nbr je jour 
    /// </summary>
    /// FI 20151019 [21317] Add
    // EG 20190730 New element rate (rate read on EVENTDET in MKP|MKA|MKV events)
    public partial class ReportAmountAccruedInterest : ReportAmountSide
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean daysSpecified;

        /// <summary>
        /// Nbj de jour des intérêts courus
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "days")]
        public int days;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean rateSpecified;

        /// <summary>
        /// Tx
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "rate")]
        public decimal rate;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fmtRateSpecified;

        /// <summary>
        /// Tx
        /// </summary>
        /// FI 20191203 [XXXXX] Add fmtRate
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtRate")]
        public string fmtRate;

    }

    /// <summary>
    ///  Représente un Funding Amount (montant de financement)
    /// </summary>
    /// FI 20150127 [20275] Add Classe
    /// FI 20150326 [XXPOC] Modify (Heritage de  ReportAmountSideFundingAmountBase)
    public partial class ReportAmountSideFundingAmount : ReportAmountSideFundingAmountBase
    {

        [System.Xml.Serialization.XmlIgnore]
        public Boolean fdrSpecified;

        /// <summary>
        /// Représente les caractéristiques de la rémunération
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fdr", Order = 1)]
        public InsterestRate Fdr
        {
            get { return base.rate; }
            set { base.rate = value; }
        }
    }

    /// <summary>
    /// Représente un Borrowing Amount (montant de financement sur les positions shorts)
    /// </summary>
    /// FI 20150326 [XXPOC] Modify (Heritage de  ReportAmountSideFundingAmountBase)
    public partial class ReportAmountSideBorrowingAmount : ReportAmountSideFundingAmountBase
    {

        [System.Xml.Serialization.XmlIgnore]
        public Boolean bwrSpecified;


        /// <summary>
        /// Représente les caractéristiques de la rémunération
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("bwr", Order = 1)]
        public InsterestRate Bwr
        {
            get { return base.rate; }
            set { base.rate = value; }
        }
    }

    /// <summary>
    /// Classe de base pour les Funding et Borrowing
    /// </summary>
    /// FI 20150326 [XXPOC] Add Class
    public abstract class ReportAmountSideFundingAmountBase : ReportAmountSide
    {

        /// FI 20150327 [20275] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean labelSpecified;

        /// <summary>
        /// Description de la rémunération 
        /// </summary>
        /// FI 20150327 [20275] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lbl")]
        public string label;


        /// <summary>
        /// Représente les caractéristiques de la rémunération
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public InsterestRate rate;
    }

    /// <summary>
    ///  Caractéristiques principales d'une rémunération
    /// </summary>
    /// FI 20150127 [20275] Add Classe
    public partial class InsterestRate
    {
        /// <summary>
        /// Valeur du Taux
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "rate")]
        public decimal rate;
        /// <summary>
        /// Spread
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "spread")]
        public decimal spread;
        /// <summary>
        /// DayCountFraction
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "dcf")]
        public FpML.Enum.DayCountFractionEnum dcf;
        /// <summary>
        /// Nbr de jours
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "days")]
        public int days;
    }

    /// <summary>
    /// Repésente un montant de taxe
    /// </summary>
    /// FI 20150119 [XXXXX] Modify
    public partial class ReportTaxAmount
    {
        /// <summary>
        ///  Identifiant de la taxe
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "taxId")]
        public string taxId;

        /// <summary>
        ///  Type de la taxe
        /// </summary>
        /// FI 20150119 [XXXXX] Add member
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "taxType")]
        public string taxType;

        /// <summary>
        /// Taux de la tax
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "rate")]
        public decimal rate;

        /// <summary>
        /// Valeur de la taxe
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "amt")]
        public Decimal amount;

    }
    
    /// <summary>
    /// Représente les données sujettes à contrôle matching d'un trade 
    /// </summary>
    /// FI 20140731 [20179] add class
    public partial class TradeCciMatch : TradeIdentification
    {
        /// <summary>
        /// Représentes les données sujettes à contrôle matching
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cciMatch")]
        public List<CciMatch> cciMatch;
    }

    /// <summary>
    /// Représente une donnée sujette à contrôle matching  (Fonctionalité liée au Menu => Matching:Appariement manuel des données) 
    /// </summary>
    /// FI 20140731 [20179] add class
    public partial class CciMatch
    {
        /// <summary>
        /// Représente le clientId du CCI 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("clientId")]
        public string clientId;

        /// <summary>
        /// Représente le type de la donnée
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("dataType")]
        public string dataType;

        /// <summary>
        /// Représente la valeur de la donnée
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("value")]
        public string value;

        /// <summary>
        /// Représente les labels associé 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("lbl")]
        public List<CciLabel> label;

        /// <summary>
        /// Représente la valeur de la donnée
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("status")]
        public string status;

    }

    /// <summary>
    /// Représente une étiquette
    /// </summary>
    /// FI 20140731 [20179] add class
    public partial class CciLabel
    {
        /// <summary>
        ///  Représente la culture 
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("culture")]
        public string culture;
        /// <summary>
        ///  Représente la valeur de l'étiquette
        /// </summary>
        [System.Xml.Serialization.XmlAttribute("value")]
        public string value;
    }

    /// <summary>
    /// UTI de type position
    /// </summary>
    /// FI 20141218 [20574] Add Class
    public partial class PositionUTIId
    {
        /// <summary>
        /// Id Unique  de type xsd:ID
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "ID")]
        public string id;

        /// <summary>
        /// Représente les UTIs de la position côté dealer et clearer 
        /// </summary>
        public UTIAlloc uti;
    }

    /// <summary>
    /// Référence à un PositionUTIId
    /// </summary>
    /// FI 20141218 [20574] Add Class
    public partial class PositionUTIReference : IReference
    {
        /// <summary>
        /// Reférence à un xsd:ID qui représente un UTI de position
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150113 [20672] Add
    public partial class PositionKey
    {
        /// <summary>
        /// Identifiant non significatif de l'acteur dealer 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "ida_dealer")]
        public int idA_Dealer;

        /// <summary>
        /// Identifiant non significatif du book dealer 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idb_dealer")]
        public int idB_Dealer;

        /// <summary>
        /// Identifiant non significatif de l'acteur clearer
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "ida_clearer")]
        public int idA_Clearer;

        /// <summary>
        /// Identifiant non significatif du book clearer
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idb_clearer")]
        public int idB_Clearer;

        /// <summary>
        /// Identifiant non significatif de l'instrument
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idI")]
        public int idI;

        /// <summary>
        /// Identifiant non significatif de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        /// <summary>
        /// type d'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "assetCategory")]
        public Cst.UnderlyingAsset assetCategory;

    }

    /// <summary>
    /// Liste des versements/dépôts à une date
    /// </summary>
    /// FI 20150128 [20275] Add
    /// FI 20150427 [20987] Modify
    public partial class CashPayments
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        /// FI 20150427 [20987] Add
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des cashPayment en business Date
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cashPayment")]
        public List<CashPayment> cashPayment;
        #endregion
    }

    /// <summary>
    /// Représente un versement/retrais (versement de cash ou versement de dividend...etc)  à une date
    /// </summary>
    /// FI 20150128 [20275] Add
    /// FI 20150128 [20275] Add
    /// FI 20151019 [21317] Modify
    public partial class CashPayment : TradeIdentification
    {
        /// <summary>
        /// Identifiant non significatif de l'acteur
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idA")]
        public int ida;

        /// <summary>
        /// Identifiant non significatif du Book
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idB")]
        public int idb;

        /// <summary>
        /// Date de transaction
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("trdDt", DataType = "normalizedString")]
        public string trdDt;

        /// <summary>
        /// Date valeur
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("valDt", DataType = "normalizedString")]
        public string valDt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean sideSpecified;

        /// <summary>
        /// Sens 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "side")]
        public CrDrEnum side;

        /// <summary>
        /// Valeur du montant
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "amt")]
        public Decimal amount;

        /// <summary>
        /// devise
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "ccy")]
        public String currency;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean paymentTypeSpecified;

        /// <summary>
        /// paymentType 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "paymentType")]
        public string paymentType;

        /// <summary>
        /// eventType 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "eventType")]
        public string eventType;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean labelSpecified;

        /// <summary>
        ///
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lbl")]
        public string label;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean detailSpecified;

        /// <summary>
        /// Informations supplémentaires concernat le versement 
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("detail")]
        public CashPaymentDet detail;

    }

    /// <summary>
    ///  Détail d'un cashPayment (=> position (asset + qty) et/ou rémunération)  
    ///  <para>lorsque le cash Payment représente un coupon => permet le stockage de la position et de la rémunération</para>
    ///  <para>lorsque le cash Payment représente un dividend => permet le stockage de la position </para>
    /// </summary>
    /// FI 20151019 [21317] Add
    /// FI 20170201 [21916] Add 
    public class CashPaymentDet
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean idAssetSpecified;
        /// <summary>
        /// Identifiant non significatif de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean assetCategorySpecified;
        /// <summary>
        /// type d'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "assetCategory")]
        public Cst.UnderlyingAsset assetCategory;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean qtySpecified;
        /// <summary>
        /// Quantité en position
        /// </summary>
        /// EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qty")]
        public decimal qty;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fmtQtySpecified;
        /// <summary> 
        /// Quantité en position (formatée)
        /// </summary>
        /// FI 20170201 [21916] Add 
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtQty")]
        public string fmtQty;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean cprSpecified;
        /// <summary>
        ///  Caractéristiques principales de la rémunération lorsque le cashPayement est issu d'une rémunération
        ///  <para>cpr (cash payment rate)</para>
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cpr")]
        public InsterestRate cpr;

    }

    /// <summary>
    ///  Contient les informations communes à postrade et tradeOfDay   
    /// </summary>
    /// FI 20150331 [XXPOC] Add classe  
    /// FI 20150407 [XXPOC] Modify
    /// FI 20150522 [20275] Modify
    /// EG 20150920 [21374] Int (int32) to Long (Int64) 
    /// FI 20151019 [21317] Modify 
    /// FI 20161214 [21916] Modify
    /// FI 20170116 [21916] Modify
    /// FI 20170201 [21916] Modify 
    public abstract partial class PosTradeCommon : TradeIdentification
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean gProductSpecified;
        /// <summary>
        /// Groupe de produit
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "gProduct")]
        public string gProduct;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean familySpecified;

        /// <summary>
        /// Famille du produit
        /// </summary>
        /// FI 20151019 [21317] add 
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "family")]
        public string family;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fungibilityModeSpecified;
        /// <summary>
        ///  Représente le type de fungibilité associé à l'instrument
        /// </summary>
        /// FI 20150407 [XXPOC] add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fungibilityMode")]
        public FungibilityModeEnum fungibilityMode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean idISpecified;
        /// <summary>
        /// Identifiant non significatif de l'instrument
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idI")]
        public int idI;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean idAssetSpecified;
        /// <summary>
        /// Identifiant non significatif de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean assetCategorySpecified;
        /// <summary>
        /// type d'asset
        /// </summary>
        /// FI 20140813 [20275] Add Member
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "assetCategory")]
        public Cst.UnderlyingAsset assetCategory;

        /// <summary>
        /// Identifiant non significatif du book Dealer
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean idbSpecified;
        /// <summary>
        /// Identifiant non significatif du Book Dealer
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idB")]
        public int idb;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean sideSpecified;
        /// <summary>
        /// Sens de l'opération
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "side")]
        public SideEnum side;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean qtySpecified;
        /// <summary>
        /// Quantité en position ou négociée
        /// </summary>
        /// EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qty")]
        public decimal qty;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fmtQtySpecified;

        /// <summary>
        /// Quantité en position ou négociée (formatée)
        /// </summary>
        /// FI 20170201 [21916] Add 
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtQty")]
        public string fmtQty;

        /* FI 20170116 [21916] mise en commentaire car déplacé sous asset 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean qtyUnitSpecified;
        /// <summary>
        /// Quantité unité (Mwh par exemple) 
        /// </summary>
        /// FI 20161214 [21916] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qtyUnit")]
        public string qtyUnit;
        */

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean nomSpecified;
        /// <summary>
        ///  Nominal ( => Qté (en position) * Nominal unitaire du titre
        ///  Renseigné uniquement sur les DebtSecurityTransaction 
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("nom")]
        public ReportAmountSide nom;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean priceSpecified;
        /// <summary>
        /// Prix de négo
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lastPx")]
        public Decimal price;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean dtOutSpecified;

        /// <summary>
        /// Date d'expiration du trade
        /// </summary>
        // FI 20150522 [20275]
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "dtOut")]
        public DateTime dtOut;                

    }

    /// <summary>
    /// Représente l'identification d'un collateral
    /// </summary>
    /// FI 20160530 [21885] Add  ( FI 20160530 [21885] GLOP, il faudrait partager ce code avec tradeIdentification) 
    public partial class CollateralIdentification
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }

        #region Members
        /// <summary>
        /// id non significatif
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// identifiant 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("colId", DataType = "normalizedString")]
        public string identifier;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameSpecified;

        /// <summary>
        /// displayName
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("displayName", DataType = "normalizedString")]
        public string displayName;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;

        /// <summary>
        /// description
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("description", DataType = "normalizedString")]
        public string description;
        #endregion

    }

    /// <summary>
    ///  Représente un dépôts de garantie
    /// </summary>
    /// FI 20160530 [21885] Add 
    /// FI 20170201 [21916] Add 
    public class CollateralReport : CollateralIdentification, IAssetLabel
    {
        /// <summary>
        /// Identifiant non significatif du book 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idB")]
        public int idB;

        /// <summary>
        /// Identifiant non significatif de l'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        /// <summary>
        /// type d'asset
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "assetCategory")]
        public Cst.UnderlyingAsset assetCategory;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean qtySpecified;
        /// <summary>
        /// Quantité 
        /// </summary>
        /// EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qty")]
        public decimal qty;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fmtQtySpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtQty")]
        public string fmtQty;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean sideSpecified;

        /// <summary>
        /// Sens 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "side")]
        public CrDrEnum side;

        /// <summary>
        /// Valorisation du dépôt de garanties
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "amt")]
        public Decimal valuation;

        /// <summary>
        /// devise
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "ccy")]
        public String currency;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean labelSpecified;

        /// <summary>
        /// Description du collateral
        /// <para>Utilisé pour l'alimentation de la colonne designation</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lbl")]
        public string label;


        /// <summary>
        /// haircut applicable par par chambre 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("haircut")]
        public CssValueReport[] haircut;


        #region IAssetLabel Membres

        Cst.UnderlyingAsset IAssetLabel.AssetCategory
        {
            get { return this.assetCategory; }
        }

        int IAssetLabel.IdAsset
        {
            get { return this.idAsset; }
        }

        string IAssetLabel.Label
        {
            get { return this.label; }
            set { label = value; }
        }

        Boolean IAssetLabel.LabelSpecified
        {
            get { return this.labelSpecified; }
            set { labelSpecified = value; }
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20160530 [21885] Add 
    public class CssValueReport
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idASpecified;

        /// <summary>
        /// IdA non significatif de la chambre de compensation
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("idA")]
        public Int32 idA;


        /// <summary>
        /// Valeur du montant
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("value")]
        public Decimal @value;
    }

    /// <summary>
    /// Liste des Dépôts de garantie à une date
    /// </summary>
    public class CollateralsReport
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des dépôts de garantie en business Date
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateral")]
        public List<CollateralReport> collateral;
        #endregion
    }

    /// <summary>
    /// Représente l'identification d'un position Stock utilisée pour réduire les positions short CALL et FUTURE  
    /// </summary>
    /// FI 20160613 [22256] Add   ( // FI 20160613 [22256] GLOP, il faudrait partager ce code avec tradeIdentification) 
    public class UnderlyingStockIdentification
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }

        #region Members
        /// <summary>
        /// id non significatif
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// identifiant 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("unsId", DataType = "normalizedString")]
        public string identifier;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameSpecified;

        /// <summary>
        /// displayName
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("displayName", DataType = "normalizedString")]
        public string displayName;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;

        /// <summary>
        /// description
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("description", DataType = "normalizedString")]
        public string description;


        /// <summary>
        ///  Priorité de couverture pour la selection des assets ETD
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("stockCover", DataType = "normalizedString")]
        public string posStockCoverEum;

        #endregion

    }

    /// <summary>
    ///  Représente une position action déposées et utilisées pour la réduction des postions ETD Short futures et Short call à une date donné
    /// </summary>
    /// FI 20160613 [22256] Add
    public class UnderlyingStockReport : UnderlyingStockIdentification, IAssetLabel
    {

        /// <summary>
        /// Identifiant non significatif du book 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idB")]
        public int idB;

        /// <summary>
        /// Identifiant non significatif de l'asset Equity
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "idAsset")]
        public int idAsset;

        /// <summary>
        /// Quantité disponible (qté déposée)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qtyAvailable")]
        // EG 20170127 Qty Long To Decimal
        public decimal qtyAvailable;


        /// <summary>
        /// Quantité utilisée pour couvrir des positions Short Future
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qtyUsedFut")]
        // EG 20170127 Qty Long To Decimal
        public decimal qtyUsedFut;


        /// <summary>
        /// Quantité utilisée pour couvrir des positions Short Call Option
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qtyUsedOpt")]
        // EG 20170127 Qty Long To Decimal
        public decimal qtyUsedOpt;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean labelSpecified;

        /// <summary>
        /// Utilisée pour l'alimenation de la colonne designation
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "lbl")]
        public string label;


        #region IAssetLabel Membres

        Cst.UnderlyingAsset IAssetLabel.AssetCategory
        {
            get { return Cst.UnderlyingAsset.EquityAsset; }
        }

        int IAssetLabel.IdAsset
        {
            get { return this.idAsset; }
        }

        string IAssetLabel.Label
        {
            get { return this.label; }
            set { label = value; }
        }

        Boolean IAssetLabel.LabelSpecified
        {
            get { return this.labelSpecified; }
            set { labelSpecified = value; }
        }

        #endregion

    }

    /// <summary>
    ///  Liste des positions actions déposées et utilisées pour la réduction des postions ETD Short futures et Short call à une date donné
    /// </summary>
    /// FI 20160613 [22256] Add
    public class UnderlyingStocksReport
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des positions actions déposées et utilisées pour la réduction des postions ETD Short futures et Short call
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("underlyingStock")]
        public List<UnderlyingStockReport> underlyingStock;
        #endregion
    }

    /// <summary>
    ///  Date d'une précédente génération avec son équivalent UTC
    /// </summary>
    /// FI 20160624 [22286] Add 
    public class PreviousDateTimeUTC : EFS_DateTimeUTC
    {
        /// <summary>
        ///  Indicateur qui stipule si la précédente version est obsolete 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "obsolete")]
        public Boolean obsolete;
    }

    /// <summary>
    /// Physical Delivery Trades
    /// <para>Trades pour lesquels il y a paiement d'une livraison (cas de livraisons périodique de ss-jacent commodity)</para>
    /// </summary>
    /// FI 20170217 [22862] Add
    public partial class DlvTrades
    {
        #region Members
        /// <summary>
        /// Business Date au format YYYY-MM-DD
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("bizDt", DataType = "normalizedString")]
        public string bizDt;

        /// <summary>
        /// Liste des trades en positions
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public List<DlvTrade> trade;
        #endregion
    }

    /// <summary>
    /// Physical Delivery Trade
    /// <para>Trade pour lesquel il y a paiement d'une livraison (Livraison périodfique de ss-jacent commodity)</para>
    /// FI 20170217 [22862] Add
    public partial class DlvTrade : PosTradeCommon
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stlPriceSpecified;

        /// <summary>
        /// <para>final settlement price du future (prix utilisé pour déterminer tous les paiments)</para>
        /// <para>prix exprimé en base 100</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("stlPx")]
        public Decimal stlPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fmtStlPriceSpecified;
        /// <summary>
        /// <para>final settlement price du future (prix utilisé pour déterminer tous les paiments)</para>
        /// <para>settlement price  formaté</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("fmtStlPx")]
        public string fmtStlPrice;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dvaSpecified;

        /// <summary>
        /// Représente le payment
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("dva")]
        public ReportAmountSide dva;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool phyQtySpecified;

        /// <summary>
        /// Qté physique de ss-jacent livré 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("phy")]
        public PhysicalDelivery phyQty;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeSpecified;
        /// <summary>
        /// représente les frais éventuels
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fee")]
        public ReportFee[] fee;
    }
    
    /// <summary>
    ///  Représente une quantité de ss-jacent 
    /// </summary>
    /// FI 20170217 [22862] Add
    public class PhysicalDelivery
    {
        /// <summary>
        /// Quantité livré (généralement en MWh)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qty")]
        public decimal qty;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean fmtQtySpecified;

        /// <summary>
        /// Quantité livré (généralement en MWh)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "fmtQty")]
        public string fmtQty;

        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean qtyUnitSpecified;
        /// <summary>
        /// Quantité unité (Mwh par exemple) 
        /// </summary>
        /// FI 20161214 [21916] Add
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "qtyUnit")]
        public string qtyUnit;
        
    }

}

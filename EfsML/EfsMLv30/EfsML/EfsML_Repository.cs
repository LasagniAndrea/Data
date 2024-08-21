#region using directives
using System;
using System.Linq;

using EFS.ACommon;
using EFS.GUI.Interface;

using EfsML.Enum;
using EfsML.Interface;
using EfsML.Repository;

using EfsML.v30.Shared;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Enum;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.Repository
{
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20220324 [XXXXX] New
    public partial class ContractRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractCategorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractCategory", Order = 1)]
        public string contractCategory;
        #endregion Members
    }
    /// <summary>
    /// 
    /// </summary>
    // PM 20140516 [19970][19259] Ajout de idC et idCSpecified
    // FI 20140903 [20275] ajout de IdM
    // FI 20150218 [20275] Modify
    // FI 20150513 [XXXXX] Modify Heritage de AssetRepositoryBase
    // FI 20150522 [20275] Mofify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetETDRepository : AssetRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFICodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CFICode", Order = 1)]
        public string CFICode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSymbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("assetSymbol", Order = 2)]
        public string assetSymbol;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractMultiplier", Order = 3)]
        public decimal contractMultiplier;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        /// <summary>
        ///  strike si asset de type option
        /// </summary>
        /// FI 20150218 [20275] strikePrice est de type RepositoryPrice
        [System.Xml.Serialization.XmlElementAttribute("strikePrice", Order = 4)]
        public RepositoryPrice strikePrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putCallSpecified;
        [System.Xml.Serialization.XmlElementAttribute("putCall", Order = 5)]
        public string putCall;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISINCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("isinCode", Order = 6)]
        public string ISINCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool aiiSpecified;
        [System.Xml.Serialization.XmlElementAttribute("aii", Order = 7)]
        public string aii;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool factorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("factor", Order = 8)]
        public decimal factor;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturityMonthYearSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturityMonthYear", Order = 9)]
        public string maturityMonthYear;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturityDateSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("maturityDate", Order = 10)]
        public DateTime maturityDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expIndSpecified;
        /// <summary>
        /// Indicateur de proximité d'échéance 
        /// </summary>
        /// FI 20150522 [20275] Add
        [System.Xml.Serialization.XmlElementAttribute("expInd", Order = 11)]
        public int expInd;

        [System.Xml.Serialization.XmlElementAttribute("idDC", Order = 12)]
        public int idDC;

        // 20120820 MF [18073]
        ConvertedPrices _ConvertedPrices = new ConvertedPrices();
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ConvertedPricesSpecified;
        /// <summary>
        /// Converted values collection. The values conversion is performed according with a specific base and format style specified
        /// on the derivative contract referential related to the current asset.
        /// </summary>
        [System.Xml.Serialization.XmlElement("convertedPrices", Order = 13)]
        public ConvertedPrices ConvertedPrices
        {
            get { return _ConvertedPrices; }
            set { _ConvertedPrices = value; }
        }

        // FI 20150513 [XXXXX] Mise en commetaire
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool idMSpecified;
        //[System.Xml.Serialization.XmlElementAttribute("idM", Order = 14)]
        //public int idM;

        //PM 20140516 [19970][19259] Ajout de idC et idCSpecified
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool idCSpecified;
        //[System.Xml.Serialization.XmlElementAttribute("idC", Order = 15)]
        //public string idC;

        // PL 20181001 [24211] RICCODE/BBGCODE
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RICCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ricCode", Order = 14)]
        public string RICCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BBGCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bbgCode", Order = 15)]
        public string BBGCode;
        #endregion Members
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150513 [XXXXX] Add 
    public partial class AssetRepository : CommonRepository
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool altIdentifierSpecified;
        /// <summary>
        /// Alternative Identifier (en générale => nom affiché de l'asset, du quel on supprime la devise, le code ISIN, le CodeISO du marché)
        /// </summary>
        /// FI 20150513 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("altIdentifier", Order = 1)]
        public string altIdentifier;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMSpecified;
        /// <summary>
        /// Marché (Id non significatif d'un marché)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("idM", Order = 3)]
        public int idM;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        /// <summary>
        /// Devise de cotation 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("idC", Order = 4)]
        public string idC;

    }


    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExtendRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extendDetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extendDet", Order = 1)]
        public ExtendDetRepository[] extendDet;
        #endregion Members
    }
    
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ExtendDetRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("dataType", Order = 1)]
        public string dataType;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20150304 [XXPOC] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommonRepository
    {
        #region Members
        /// FI 20150304 [XXPOC] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool otcmlIdSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "ID")]
        public string Id;
        [System.Xml.Serialization.XmlElementAttribute("identifier", Order = 1)]
        public string identifier;
        [System.Xml.Serialization.XmlElementAttribute("displayname", Order = 2)]
        public string displayname;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Order = 3)]
        public string description;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extllinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extllink", Order = 4)]
        public string extllink;

        #endregion Members
    }
    
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EnumsRepository
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "ID")]
        public string Id;
        [System.Xml.Serialization.XmlElementAttribute("code", Order = 1)]
        public string code;
        [System.Xml.Serialization.XmlElementAttribute("extcode", Order = 2)]
        public string extCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool enumsDetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("enum", Order = 3)]
        public EnumRepository[] enumsDet;
        #endregion Members
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170216 [21916] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class EnumRepository
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "ID")]
        public string Id;
        [System.Xml.Serialization.XmlElementAttribute("value", Order = 1)]
        public string value;
        [System.Xml.Serialization.XmlElementAttribute("extvalue", Order = 2)]
        public string extValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean extAttrbSpecified; // FI 20170216 [21916] add
        [System.Xml.Serialization.XmlElementAttribute("extattrb", Order = 3)]
        public string extAttrb;
        #endregion Members
    }

    /// <summary>
    /// Réprésente le référentiel Marché
    /// </summary>
    // PL 20181001 [24211] RICCODE/BBGCODE
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class MarketRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISO10383_ALPHA4Specified;
        [System.Xml.Serialization.XmlElementAttribute("ISO10383_ALPHA4", Order = 1)]
        public string ISO10383_ALPHA4;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool acronymSpecified;
        [System.Xml.Serialization.XmlElementAttribute("acronym", Order = 2)]
        public string acronym;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool citySpecified;
        [System.Xml.Serialization.XmlElementAttribute("city", Order = 3)]
        public string city;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeSymbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeSymbol", Order = 4)]
        public string exchangeSymbol;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool shortIdentifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("shortIdentifier", Order = 5)]
        public string shortIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixml_SecurityExchangeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixml_SecurityExchange", Order = 6)]
        public string fixml_SecurityExchange;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RICCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ricCode", Order = 7)]
        public string RICCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BBGCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bbgCode", Order = 8)]
        public string BBGCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ETDIdentifierFormatSpecified;
        // FI 20220912 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("etdIdentifierFormat", Order = 9)]
        public string ETDIdentifierFormat;

        #endregion Members
    }

    /// <summary>
    /// Réprésente le référentiel Marché
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InvoicingScopeRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC_FeeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idC_Fee", Order = 1)]
        public string idC_Fee;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventType", Order = 2)]
        public string eventType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentType", Order = 3)]
        public string paymentType;
        #endregion Members
    }


    /// <summary>
    /// Réprésente l'instrument
    /// </summary>
    /// FI 20150218 [20275] Add
    /// FI 20150625 [21149] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class InstrumentRepository : CommonRepository
    {
        [System.Xml.Serialization.XmlElementAttribute("product", Order = 1)]
        public string product;
        [System.Xml.Serialization.XmlElementAttribute("gProduct", Order = 2)]
        public string gProduct;
        [System.Xml.Serialization.XmlElementAttribute("isMargining", Order = 3)]
        public Boolean isMargining;
        [System.Xml.Serialization.XmlElementAttribute("isFunding", Order = 4)]
        public Boolean isFunding;
    }





    /// <summary>
    /// Réprésente le référentiel Book
    /// </summary>
    /// FI 20150603 [XXXXX] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class BookRepository : CommonRepository
    {
        #region Members

        /// <summary>
        /// Représente le propriétaire du book
        /// </summary>
        /// FI 20150603 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("owner", Order = 1)]
        public BookOwnerRepository owner;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idcSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("idc", Order = 2)]
        public string idc;

        #endregion Members
    }

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class TradeLinkRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("link", Order = 1)]
        public string link;
        [System.Xml.Serialization.XmlElementAttribute("idt", Order = 2)]
        public int idt;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool executionIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("executionId", Order = 3)]
        public string executionId;
        [System.Xml.Serialization.XmlElementAttribute("idt_a", Order = 4)]
        public int idt_a;
        [System.Xml.Serialization.XmlElementAttribute("identifier_a", Order = 5)]
        public string identifier_a;
        #endregion Members
    }

    /// <summary>
    /// Réprésente le référentiel Indice de taux
    /// </summary>
    // FI 20150513 [XXXXX] Modify Herite de AssetRepositoryBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetRateIndexRepository : AssetRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool informationSourceSpecified;
        
        [System.Xml.Serialization.XmlElementAttribute("informationSource", typeof(InformationSource), Order = 1)]
        public InformationSource informationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        
        public bool rateTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateType", Order = 2)]
        public string rateType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        
        public bool calculationRuleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationRule", Order = 3)]
        public string calculationRule;

        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20240216 [WI850][26600] Ajout Request Date pour édition des confirmation sur facture Migration MAREX
    public partial class FxRateRepository : FxRate
    {
        #region Members
        [System.Xml.Serialization.XmlIgnore()]
        public DateTime requestDate;
        [System.Xml.Serialization.XmlElementAttribute("fixingDate", Order = 1)]
        public EFS_Date fixingDate;
        #endregion Members
    }

    /// <summary>
    /// Réprésente le référentiel Devise
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CurrencyRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool symbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("symbol", Order = 3)]
        public string symbol;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool symbolalignSpecified;
        [System.Xml.Serialization.XmlElementAttribute("symbolalign", Order = 4)]
        public string symbolalign;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISO4217_num3Specified;
        [System.Xml.Serialization.XmlElementAttribute("ISO4217_num3", Order = 5)]
        public string ISO4217_num3;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool factorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("factor", Order = 6)]
        public int factor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rounddirSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rounddir", Order = 7)]
        public RoundingDirectionEnum rounddir;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool roundprecSpecified;
        [System.Xml.Serialization.XmlElementAttribute("roundprec", Order = 8)]
        public int roundprec;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fxrateSpecified;
        // RD 20131015 [19067] Extrait de compte / un taux de change par jour        
        [System.Xml.Serialization.XmlElementAttribute("fxrate", Order = 9)]
        public FxRateRepository[] fxrate;
        #endregion Members
    }

    /// <summary>
    /// Réprésente les éléments du référentiel
    /// </summary>
    /// FI 20180818 [20275] Modify
    /// FI 20150304 [XXPOC] Modify
    /// FI 20151019 [21317] Modify
    /// FI 20161214 [21916] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Repository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool enumsSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("enums", Order = 1)]
        public EnumsRepository[] enums;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool marketSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("market", Order = 2)]
        public MarketRepository[] market;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool businessCenterSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150304 [XXPOC] Add
        [System.Xml.Serialization.XmlElementAttribute("businessCenter", Order = 3)]
        public BusinessCenterRepository[] businessCenter;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool currencySpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 4)]
        public CurrencyRepository[] currency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool derivativeContractSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("derivativeContract", Order = 5)]
        public DerivativeContractRepository[] derivativeContract;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetETDSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("etd", Order = 6)]
        public AssetETDRepository[] assetETD;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetEquitySpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180818 [20275] Add
        /// FI 20150120 [XXXXX] Modify
        [System.Xml.Serialization.XmlElementAttribute("equity", Order = 7)]
        public AssetEquityRepository[] assetEquity;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetIndexSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180818 [20275] Add
        /// FI 20150120 [XXXXX] Modify
        [System.Xml.Serialization.XmlElementAttribute("index", Order = 8)]
        public AssetIndexRepository[] assetIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetRateIndexSpecified;
        /// <summary>
        /// Représente les indice de taux flottant
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("rateIndex", Order = 9)]
        public AssetRateIndexRepository[] assetRateIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetFxRateSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fxRate", Order = 10)]
        public AssetFxRateRepository[] assetFxRate;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetDebtSecuritySpecified;
        /// <summary>
        /// Représente les titres de rémunération (Bond par exemple)
        /// </summary>
        /// FI 20151019 [21317] Add
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", Order = 11)]
        public AssetDebtSecurityRepository[] assetDebtSecurity;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCashSpecified;
        /// <summary>
        /// Représente les assets cash
        /// </summary>
        /// FI 20160530 [21885] Add
        [System.Xml.Serialization.XmlElementAttribute("cash", Order = 12)]
        public AssetCashRepository[]  assetCash;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCommoditySpecified;
        /// <summary>
        /// Représente les assets commodity
        /// </summary>
        /// FI 20161214 [21916] Modify 
        [System.Xml.Serialization.XmlElementAttribute("commodity", Order = 13)]
        public AssetCommodityRepository[] assetCommodity;
        

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool instrumentSpecified;
        /// <summary>
        /// Représente les instrument
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlElementAttribute("instr", Order = 14)]
        public InstrumentRepository[] instrument;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool actorSpecified;
        /// <summary>
        /// Représente les instrument
        /// </summary>
        /// FI 20150218 [20275] Add
        [System.Xml.Serialization.XmlElementAttribute("actor", Order = 15)]
        public ActorRepository[] actor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bookSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("book", Order = 16)]
        public BookRepository[] book;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extendSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("extend", Order = 17)]
        public ExtendRepository[] extend;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeLinkSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tradeLink", Order = 18)]
        public TradeLinkRepository[] tradeLink;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool invoicingScopeSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("invoicingScope", Order = 19)]
        public InvoicingScopeRepository[] invoicingScope;

        #endregion Members
    }

    /// <summary>
    /// Réprésente le référentile d'un contrat Dérivé
    /// </summary>
    /// FI 20150218 [20275] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DerivativeContractRepository : CommonRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMSpecified;
        /// <summary>
        ///  Représente le marché
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("idM", Order = 1)]
        public int idM;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractSymbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractSymbol", Order = 2)]
        public string contractSymbol;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool categorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("category", Order = 3)]
        public string category;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settltMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settltMethod", Order = 4)]
        public string settltMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exerciseStyleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Order = 5)]
        public string exerciseStyle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool attribSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220908 [XXXXX] Add  
        [System.Xml.Serialization.XmlElementAttribute("attrib", Order = 6)]
        public string attrib;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlDescSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220906 [XXXXX] Add 
        [System.Xml.Serialization.XmlElementAttribute("extlDesc", Order = 7)]
        public string extlDesc;

        // PL 20181001 [24211] RICCODE/BBGCODE
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RICCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ricCode", Order = 8)]
        public string RICCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BBGCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bbgCode", Order = 9)]
        public string BBGCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool futValuationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("futValuationMethod", Order = 10)]
        public string futValuationMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC_PriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idC_Price", Order = 11)]
        public string idC_Price;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC_NominalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idC_Nominal", Order = 12)]
        public string idC_Nominal;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractMultiplier", Order = 13)]
        public decimal contractMultiplier;
        
        [System.Xml.Serialization.XmlElementAttribute("instrumentDen", Order = 14)]
        public int instrumentDen;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool factorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("factor", Order = 15)]
        public decimal factor;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCategorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("assetCategory", Order = 16)]
        public string assetCategory;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idDC_UnlSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idDC_Unl", Order = 17)]
        public string idDC_Unl;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAsset_UnlSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idAsset_Unl", Order = 18)]
        public string idAsset_Unl;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool priceFmtStyleSpecified;
        /// <summary>
        /// Représente le format d'affichage des prix pour les éditions
        /// </summary>
        /// FI 20150218 [20275] add
        [System.Xml.Serialization.XmlElementAttribute("priceFmtStyle", Order = 19)]
        public string priceFmtStyle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikeFmtStyleSpecified;
        /// <summary>
        /// Représente le format d'affichage du strike pour les éditions
        /// </summary>
        /// FI 20150218 [20275] add
        [System.Xml.Serialization.XmlElementAttribute("strikeFmtStyle", Order = 20)]
        public string strikeFmtStyle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ETDIdentifierFormatSpecified;
        // FI 20220912 [XXXXX] Add
        [System.Xml.Serialization.XmlElementAttribute("etdIdentifierFormat", Order = 21)]
        public string ETDIdentifierFormat;
        #endregion Members
    }

    /// <summary>
    /// Réprésente le référentiel d'un asset Equity (action)
    /// </summary>
    /// FI 20140818 [20275] add
    /// FI 20150513 [XXXXX] Modify => Heritage de AssetRepositoryBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetEquityRepository : AssetRepository
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSymbolSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("assetSymbol", Order = 1)]
        public string assetSymbol;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RICCodeSpecified;
        /// <summary>
        /// Reuters® code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ricCode", Order = 2)]
        public string RICCode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BBGCodeSpecified;
        /// <summary>
        /// Bloomberg code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("bbgCode", Order = 3)]
        public string BBGCode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISINCodeSpecified;
        /// <summary>
        /// ISIN code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("isinCode", Order = 4)]
        public string ISINCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFICodeSpecified;
        /// <summary>
        /// CFI code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cfiCode", Order = 5)]
        public string CFICode;


        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool idMSpecified;
        ///// <summary>
        ///// Marché (Id non significatif d'un marché)
        ///// </summary>
        //[System.Xml.Serialization.XmlElementAttribute("idM", Order = 6)]
        //public int idM;


        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool idCSpecified;
        ///// <summary>
        ///// Devise de cotation 
        ///// </summary>
        //[System.Xml.Serialization.XmlElementAttribute("idC", Order = 7)]
        //public string idC;
        #endregion Members
    }


    /// <summary>
    /// Réprésente le référentiel d'un titre de rémunération
    /// </summary>
    /// FI 20151019 [21317] Add class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetDebtSecurityRepository : AssetRepository
    {

        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSymbolSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("assetSymbol", Order = 1)]
        public string assetSymbol;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RICCodeSpecified;
        /// <summary>
        /// Reuters® code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ricCode", Order = 2)]
        public string RICCode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BBGCodeSpecified;
        /// <summary>
        /// Bloomberg code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("bbgCode", Order = 3)]
        public string BBGCode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISINCodeSpecified;
        /// <summary>
        /// ISIN code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("isinCode", Order = 4)]
        public string ISINCode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CFICodeSpecified;
        /// <summary>
        /// CFI code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("cfiCode", Order = 5)]
        public string CFICode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SEDOLCodeSpecified;
        /// <summary>
        ///  SEDOL code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sedolCode", Order = 6)]
        public string SEDOLCode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool parValueSpecified;

        /// <summary>
        ///  Nominal du titre
        /// </summary>
        /// FI 20151019 [21317] GLOP (cela me gêne de faire appel à EfsML.Notification)
        [System.Xml.Serialization.XmlElementAttribute("parValue", Order = 7)]
        public EfsML.Notification.ReportAmount parValue;

        #endregion Members
    }


    /// <summary>
    /// Réprésente le référentiel d'un asset Index (indice)
    /// </summary>
    /// FI 20140818 [20275] add
    /// FI 20150513 [XXXXX] Modify => Heritage de AssetRepositoryBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetIndexRepository : AssetRepository
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSymbolSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("assetSymbol", Order = 1)]
        public string assetSymbol;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RICCodeSpecified;
        /// <summary>
        /// Reuters® code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ricCode", Order = 2)]
        public string RICCode;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BBGCodeSpecified;
        /// <summary>
        /// Bloomberg code
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("bbgCode", Order = 3)]
        public string BBGCode;
    }

    /// <summary>
    /// Réprésente le référentiel d'un asset FxRate
    /// </summary>
    /// EG 20150222 Add
    /// FI 20150331 [XXPOC] Modify
    /// FI 20150513 [XXXXX] Herite de AssetRepositoryBase
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetFxRateRepository : AssetRepository
    {
        /// <summary>
        /// The primary source for where the rate observation will occur. Will typically be either a page or a reference bank published rate.
        /// </summary>
        /// FI 20150331 [XXPOC] add
        [System.Xml.Serialization.XmlElementAttribute("primaryRateSrc", typeof(InformationSource), Order = 1)]
        public InformationSource primaryRateSrc;

        /// <summary>
        /// Defines the two currencies for an FX trade and the quotation relationship between the two currencies.
        /// </summary>
        /// FI 20150331 [XXPOC] add
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", typeof(QuotedCurrencyPair), Order = 2)]
        public QuotedCurrencyPair quotedCurrencyPair;

        /// <summary>
        /// specific date when a non-deliverable forward or non-deliverable option will \"fix\" against a particular rate, which will be used to compute the ultimate cash settlement.
        /// </summary>
        /// FI 20150331 [XXPOC] add
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", typeof(BusinessCenterTime), Order = 3)]
        public BusinessCenterTime fixingTime;

    }



    /// <summary>
    /// Réprésente le référentiel d'un asset Cash
    /// </summary>
    /// FI 20160530 [21885] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetCashRepository : AssetRepository
    {


    }

    /// <summary>
    /// Réprésente le référentiel d'un asset commodity
    /// </summary>
    /// FI 20161214 [21916] Add
    /// FI 20170116 [21916] Modify
    /// FI 20170201 [21916] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AssetCommodityRepository : AssetRepository
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSymbolSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("assetSymbol", Order = 1)]
        public string assetSymbol;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractSymbolSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("contractSymbol", Order = 2)]
        public string contractSymbol;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchContractSymbolSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("exchContractSymbol", Order = 3)]
        public string exchContractSymbol;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool qtyUnitSpecified;
        /// <summary>
        /// Unité dans lesquelles sont exprimées les quantités sur les trades 
        /// </summary>
        /// FI 20170116 [21916] Add qtyUnit
        [System.Xml.Serialization.XmlElementAttribute("qtyUnit", Order = 4)]
        public string qtyUnit;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtyScaleSpecified { set; get; }
        /// <summary>
        /// Nombre de digit utilisés pour la partie décimale
        /// </summary>
        ///  FI 20170201 [21916] Add 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int QtyScale { set; get; }


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryPointSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("deliveryPoint", Order = 5)]
        public string deliveryPoint;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryTimezoneSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("deliveryTimezone", Order = 6)]
        public string deliveryTimezone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool durationSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("duration", Order = 7)]
        public string duration;

    }



    /// <summary>
    /// Représente une référence vers le repository Actor
    /// </summary>
    /// FI 20150603 [XXXXX] Add Class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ActorRepositoryReference
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
        #endregion Members
    }


    /// <summary>
    ///  Rerprésente le propriétaire d'un book
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    /// FI 20150310 [XXXXX] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class BookOwnerRepository : ActorRepositoryReference
    {
        /// <summary>
        /// Représente l'IDA de l'acteur 
        /// </summary>
        /// FI 20150310 [XXXXX] => Mise en commentaire (attribut non nécessaire)
        //[System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        //public string otcmlId;
    }



    /// <summary>
    /// Réprésente un acteur
    /// </summary>
    /// FI 20150603 [XXXXX] Add
    /// FI 20160530 [21885] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class ActorRepository : CommonRepository
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISO10383_ALPHA4Specified;
        /// <summary>
        /// Code MIC de l'acteur 
        /// </summary>
        /// FI 20160530 [21885] Modify
        [System.Xml.Serialization.XmlElementAttribute("ISO10383_ALPHA4", Order = 1)]
        public string ISO10383_ALPHA4;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISO17442Specified;
        /// <summary>
        /// Code LEI de l'acteur 
        /// </summary>
        /// FI 20190515 [21885] Add
        [System.Xml.Serialization.XmlElementAttribute("ISO17442", Order = 2)]
        public string ISO17442;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool telephoneNumberSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlElementAttribute("telNumber", Order = 3)]
        public string telephoneNumber;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool mobileNumberSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlElementAttribute("mobilNumber", Order = 4)]
        public string mobileNumber;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool emailSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlElementAttribute("email", Order = 5)]
        public string email;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool webSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlElementAttribute("web", Order = 6)]
        string web;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool addressSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20190515 [23912] Add
        [System.Xml.Serialization.XmlElementAttribute("address", Order = 7)]
        public Address address;
    }




    /// <summary>
    /// Réprésente un businessCenterRepository
    /// </summary>
    /// FI 20150304 [XXPOC] Add
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class BusinessCenterRepository : CommonRepository
    {


    }


    


}

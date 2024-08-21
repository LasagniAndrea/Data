#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EFS.Status;
using FpML.Enum;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EfsML
{
    /// <summary>
    /// Cet Enumerateur est utilisé pour spécifié les tables Event chargées via : DataSetEventTrade
    /// BUT = Optimisation des temps de traitement par restriction des lignes à charger
    /// </summary>
    // EG 20150612 (20665] Refactoring : Chargement DataSetEventTrade
    public enum EventTableEnum
    {
        None = 1,
        Detail = 2,
        Asset = 4,
        Class = 8,
        Process = 16,
        Pricing2 = 32,
        Fee = 64,
        StCheck = 128,
        StMatch = 256,
        SettlSi = 512,
    }


    /// <summary>
    /// 
    /// </summary>
    public class EventInstrument
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idISpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDI")]
        public int idI;
        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER_INSTR")]
        public string identifierInstr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameInstrSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME_INSTR")]
        public string displayNameInstr;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool familySpecified;
        [System.Xml.Serialization.XmlElementAttribute("FAMILY_INSTR")]
        public string family;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fungibilityModeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FUNGIBILITYMODE_INSTR")]
        public string fungibilityMode;
        #endregion Members
    }


    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("Events", IsNullable = false)]
    public class EventItems
    {
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public EventItem[] eventItem;
    }


    /// <summary>
    /// 
    /// </summary>
    /// FI 20120426 [17703] add IDPADET
    public class EventItem : EventInstrument
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlElementAttribute("IDT")]
        public int idT;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool identifierTradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER_TRADE")]
        public string identifierTrade;
        [System.Xml.Serialization.XmlElementAttribute("INSTRUMENTNO")]
        public string instrumentNo;
        [System.Xml.Serialization.XmlElementAttribute("STREAMNO")]
        public string streamNo;
        [System.Xml.Serialization.XmlElementAttribute("IDE_EVENT")]
        public int idEParent;
        [System.Xml.Serialization.XmlElementAttribute("IDE_SOURCE")]
        public int idE_Source;
        [System.Xml.Serialization.XmlElementAttribute("EVENTCODE")]
        public string eventCode;
        [System.Xml.Serialization.XmlElementAttribute("EVENTTYPE")]
        public string eventType;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPayerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_PAY")]
        public int idPayer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPayerBookSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB_PAY")]
        public int idPayerBook;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idReceiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDA_REC")]
        public int idReceiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idReceiverBookSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDB_REC")]
        public int idReceiverBook;
        //		
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtStartPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSTARTADJ")]
        public EFS_Date dtStartPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtEndPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTENDADJ")]
        public EFS_Date dtEndPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valorisationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("VALORISATION")]
        public EFS_Decimal valorisation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitSpecified;
        [System.Xml.Serialization.XmlElementAttribute("UNIT")]
        public string unit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valorisationSysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("VALORISATIONSYS")]
        public EFS_Decimal valorisationSys;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitSysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("UNITSYS")]
        public string unitSys;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStTriggerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTTRIGGER")]
        public string idStTrigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStCalculSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTCALCUL")]
        public string idStCalcul;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStActivationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTACTIVATION")]
        public string idStActivation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAStActivationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDASTACTIVATION")]
        public int idAStActivation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtStActivationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSTACTIVATION")]
        public EFS_Date dtStActivation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SOURCE")]
        public string source;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]
        public string extlLink;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idPosActionDetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDPADET")]
        public int idPosActionDet;
        #endregion
    }

    /// <summary>
    /// Class where are stored by serialisation the lines of the table <b>EventAsset</b> for a trade.
    /// </summary>
    public class EventAsset
    {
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDASSET")]
        public int idAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ASSETTYPE")]
        public string assetType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetCategorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("ASSETCATEGORY")]
        public string assetCategory;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TIME")]
        public EFS_Time time;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idBCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDBC")]
        public string idBC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTESIDE")]
        public string quoteSide;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteTimingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTETIMING")]
        public string quoteTiming;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool primaryRateSrcSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRIMARYRATESRC")]
        public string primaryRateSrc;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool primaryRateSrcPageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRIMARYRATESRCPAGE")]
        public string primaryRateSrcPage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool primaryRateSrcHeadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRIMARYRATESRCHEAD")]
        public string primaryRateSrcHead;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clearanceSystemSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CLEARANCESYSTEM")]
        public string clearanceSystem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMarketEnvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDMARKETENV")]
        public string idMarketEnv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idValScenarioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDVALSCENARIO")]
        public string idValScenario;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC")]
        public string idC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDM")]
        public EFS_Integer idM;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isinCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ISINCODE")]
        public string isinCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSymbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ASSETSYMBOL")]
        public string assetSymbol;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool identifierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER")]
        public string identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool displayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME")]
        public string displayName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractSymbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CONTRACTSYMBOL")]
        public string contractSymbol;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool categorySpecified;
        [System.Xml.Serialization.XmlElementAttribute("CATEGORY")]
        public string category;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putOrCallSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PUTORCALL")]
        public string putOrCall;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("STRIKEPRICE")]
        public EFS_Decimal strikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CONTRACTMULTIPLIER")]
        public EFS_Decimal contractMultiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturityDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("MATURITYDATE")]
        public EFS_Date maturityDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maturityDateSysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("MATURITYDATESYS")]
        public EFS_Date maturityDateSys;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deliveryDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DELIVERYDATE")]
        public EFS_Date deliveryDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool nominalValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("NOMINALVALUE")]
        public EFS_Decimal nominalValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool weightSpecified;
        [System.Xml.Serialization.XmlElementAttribute("WEIGHT")]
        public EFS_Decimal weight;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitWeightSpecified;
        [System.Xml.Serialization.XmlElementAttribute("UNITWEIGHT")]
        public string unitWeight;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitTypeWeightSpecified;
        [System.Xml.Serialization.XmlElementAttribute("UNITTYPEWEIGHT")]
        public string unitTypeWeight;


    }

    /// <summary>
    /// Class where are stored by serialisation the lines of the table <b>Event</b> for a trade.
    /// </summary>
    public class EventClass
    {
        [System.Xml.Serialization.XmlElementAttribute("IDEC")]
        public int idEC;
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool codeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EVENTCLASS")]
        public string code;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtEventSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTEVENT")]
        public EFS_Date dtEvent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtEventForcedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTEVENTFORCED")]
        public EFS_Date dtEventForced;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ISPAYMENT")]
        public EFS_Boolean isPayment;
        [System.Xml.Serialization.XmlElementAttribute("NETMETHOD")]
        public string netMethod;
        [System.Xml.Serialization.XmlElementAttribute("IDNETCONVENTION")]
        public int idNetConvention;
        [System.Xml.Serialization.XmlElementAttribute("IDNETDESIGNATION")]
        public int idNetDesignation;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventDetails
    {
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DCF")]
        public string dcf;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfNumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DCFNUM")]
        public EFS_Integer dcfNum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfDenSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DCFDEN")]
        public EFS_Integer dcfDen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfYearSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFYEAR")]
        public EFS_Integer totalOfYear;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFDAY")]
        public EFS_Integer totalOfDay;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fxTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FXTYPE")]
        public string fxType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC1Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDC1")]
        public string idC1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC2Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDC2")]
        public string idC2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCRefSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC_REF")]
        public string idCRef;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCBaseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC_BASE")]
        public string idCBase;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RATE")]
        public EFS_Decimal rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SPOTRATE")]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fwdPointsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FWDPOINTS")]
        public EFS_Decimal fwdPoints;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("NOTIONALAMOUNT")]
        public EFS_Decimal notionalAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool interestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("INTEREST")]
        public EFS_Decimal interest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gapRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("GAPRATE")]
        public EFS_Decimal gapRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settlementRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SETTLEMENTRATE")]
        public EFS_Decimal settlementRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtFixingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTFIXING")]
        public EFS_DateTime dtFixing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idBCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDBC")]
        public string idBC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalPayoutAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALPAYOUTAMOUNT")]
        public EFS_Decimal totalPayoutAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool nbPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PERIODPAYOUT")]
        public EFS_NonNegativeInteger nbPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool percentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PCTPAYOUT")]
        public EFS_Decimal percentage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notionalReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("NOTIONALREFERENCE")]
        public EFS_Decimal notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pctRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PCTRATE")]
        public EFS_Decimal pctRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool noteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("NOTE")]
        public string note;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtActionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTACTION")]
        public EFS_Date dtAction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]
        public string extlLink;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool contractMultiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CONTRACTMULTIPLIER")]
        public EFS_Decimal contractMultiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dailyQuantitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("DAILYQUANTITY")]
        // EG 20170127 Qty Long To Decimal
        public EFS_Decimal dailyQuantity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool unitDailyQuantitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("UNITDAILYQUANTITY")]
        public string unitDailyQuantity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool priceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PRICE")]
        public EFS_Decimal price;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool price100Specified;
        [System.Xml.Serialization.XmlElementAttribute("PRICE100")]
        public EFS_Decimal price100;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteTimingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTETIMING")]
        public string quoteTiming;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quotePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTEPRICE")]
        public EFS_Decimal quotePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quotePrice100Specified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTEPRICE100")]
        public EFS_Decimal quotePrice100;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settltPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SETTLTPRICE")]
        public EFS_Decimal settltPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settltPrice100Specified;
        [System.Xml.Serialization.XmlElementAttribute("SETTLTPRICE100")]
        public EFS_Decimal settltPrice100;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settltTimingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SETTLTQUOTETIMING")]
        public string settltTiming;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteDeltaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTEDELTA")]
        public EFS_Decimal quoteDelta;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quotePriceYestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTEPRICEYEST")]
        public EFS_Decimal quotePriceYest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quotePriceYest100Specified;
        [System.Xml.Serialization.XmlElementAttribute("QUOTEPRICEYEST100")]
        public EFS_Decimal quotePriceYest100;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool closingPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CLOSINGPRICE")]
        public EFS_Decimal closingPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool closingPrice100Specified;
        [System.Xml.Serialization.XmlElementAttribute("CLOSINGPRICE100")]
        public EFS_Decimal closingPrice100;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("STRIKEPRICE")]
        public EFS_Decimal strikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool factorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FACTOR")]
        public EFS_Decimal factor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtDlvyStartSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTDLVYSTART")]
        public EFS_Date dtDlvyStart;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtDlvyEndSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTDLVYEND")]
        public EFS_Date dtDlvyEnd;

        // 20120820 MF - Ticket 18073
        #region Base conversion

        ConvertedPrices _ConvertedPrices = new ConvertedPrices();
        /// <summary>
        /// Converted event price values collection. The values conversion is performed according with a specific base and format style specified
        /// on the derivative contract referential related to the current trade event.
        /// </summary>
        [System.Xml.Serialization.XmlElement("convertedPrices")]
        public ConvertedPrices ConvertedPrices
        {
            get { return _ConvertedPrices; }
            set { _ConvertedPrices = value; }
        }

        #endregion Base conversion


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteBasisSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public QuoteBasisEnum quoteBasis;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sideRateBasisSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SideRateBasisEnum sideRateBasis;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikeQuoteBasisSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StrikeQuoteBasisEnum strikeQuoteBasis;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool premiumQuoteBasisSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public PremiumQuoteBasisEnum premiumQuoteBasis;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool basisSpecified;

        [System.Xml.Serialization.XmlElementAttribute("BASIS")]
        public string Basis
        {
            set
            {
                if (System.Enum.IsDefined(typeof(QuoteBasisEnum), value))
                {
                    quoteBasis = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), value, true);
                    quoteBasisSpecified = true;
                    basisSpecified = true;
                }
                else if (System.Enum.IsDefined(typeof(SideRateBasisEnum), value))
                {
                    sideRateBasis = (SideRateBasisEnum)System.Enum.Parse(typeof(SideRateBasisEnum), value, true);
                    sideRateBasisSpecified = true;
                    basisSpecified = true;
                }
                else if (System.Enum.IsDefined(typeof(StrikeQuoteBasisEnum), value))
                {
                    strikeQuoteBasis = (StrikeQuoteBasisEnum)System.Enum.Parse(typeof(StrikeQuoteBasisEnum), value, true);
                    strikeQuoteBasisSpecified = true;
                    basisSpecified = true;
                }
                else if (System.Enum.IsDefined(typeof(PremiumQuoteBasisEnum), value))
                {
                    premiumQuoteBasis = (PremiumQuoteBasisEnum)System.Enum.Parse(typeof(PremiumQuoteBasisEnum), value, true);
                    premiumQuoteBasisSpecified = true;
                    basisSpecified = true;
                }
                else
                    basisSpecified = false;
            }
            get
            {
                if (quoteBasisSpecified)
                    return quoteBasis.ToString();
                else if (sideRateBasisSpecified)
                    return sideRateBasis.ToString();
                else if (strikeQuoteBasisSpecified)
                    return strikeQuoteBasis.ToString();
                else if (premiumQuoteBasisSpecified)
                    return premiumQuoteBasis.ToString();
                else
                    return string.Empty;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class EventPricing
    {
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DCF")]
        public string dcf;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfNumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DCFNUM")]
        public EFS_Integer dcfNum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfDenSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DCFDEN")]
        public EFS_Integer dcfDen;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfYearSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFYEAR")]
        public EFS_Integer totalOfYear;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFDAY")]
        public EFS_Integer totalOfDay;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcf2Specified;
        [System.Xml.Serialization.XmlElementAttribute("DCF2")]
        public string dcf2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfNum2Specified;
        [System.Xml.Serialization.XmlElementAttribute("DCFNUM2")]
        public EFS_Integer dcfNum2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dcfDen2Specified;
        [System.Xml.Serialization.XmlElementAttribute("DCFDEN2")]
        public EFS_Integer dcfDen2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfYear2Specified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFYEAR2")]
        public EFS_Integer totalOfYear2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfDay2Specified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFDAY2")]
        public EFS_Integer totalOfDay2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC1Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDC1")]
        public string idC1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idC2Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDC2")]
        public string idC2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timeToExpirationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TIMETOEXPIRATION")]
        public EFS_Decimal timeToExpiration;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timeToExpiration2Specified;
        [System.Xml.Serialization.XmlElementAttribute("TIMETOEXPIRATION2")]
        public EFS_Decimal timeToExpiration2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("STRIKE")]
        public EFS_Decimal strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXCHANGERATE")]
        public EFS_Decimal exchangeRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool interestRate1Specified;
        [System.Xml.Serialization.XmlElementAttribute("INTERESTRATE1")]
        public EFS_Decimal interestRate1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool interestRate2Specified;
        [System.Xml.Serialization.XmlElementAttribute("INTERESTRATE2")]
        public EFS_Decimal interestRate2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SPOTRATE")]
        public EFS_Decimal spotRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool volatilitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("VOLATILITY")]
        public EFS_Decimal volatility;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CALLPRICE")]
        public EFS_Decimal callPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callCharmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CALLCHARM")]
        public EFS_Decimal callCharm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callDeltaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CALLDELTA")]
        public EFS_Decimal callDelta;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callRho1Specified;
        [System.Xml.Serialization.XmlElementAttribute("CALLRHO1")]
        public EFS_Decimal callRho1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callRho2Specified;
        [System.Xml.Serialization.XmlElementAttribute("CALLRHO2")]
        public EFS_Decimal callRho2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callThetaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CALLTHETA")]
        public EFS_Decimal callTheta;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PUTPRICE")]
        public EFS_Decimal putPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putCharmSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PUTCHARM")]
        public EFS_Decimal putCharm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putDeltaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PUTDELTA")]
        public EFS_Decimal putDelta;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putRho1Specified;
        [System.Xml.Serialization.XmlElementAttribute("PUTRHO1")]
        public EFS_Decimal putRho1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putRho2Specified;
        [System.Xml.Serialization.XmlElementAttribute("PUTRHO2")]
        public EFS_Decimal putRho2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putThetaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PUTTHETA")]
        public EFS_Decimal putTheta;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gammaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("GAMMA")]
        public EFS_Decimal gamma;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vegaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("VEGA")]
        public EFS_Decimal vega;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool colorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("COLOR")]
        public EFS_Decimal color;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool speedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SPEED")]
        public EFS_Decimal speed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vannaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("VANNA")]
        public EFS_Decimal vanna;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool volgaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("VOLGA")]
        public EFS_Decimal volga;

    }

    /// <summary>
    /// 
    /// </summary>
    public class EventPricing2
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("IDPRICING2")]
        public int idPricing2;
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlElementAttribute("IDE_SOURCE")]
        public int idE_Source;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool flowTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FLOWTYPE")]
        public string flowType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtFixingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTFIXING")]
        public EFS_Date dtFixing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashFlowSpecified;
        [System.Xml.Serialization.XmlElementAttribute("CASHFLOW")]
        public EFS_Decimal cashFlow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtStartSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSTART")]
        public EFS_Date dtStart;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtEndSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTEND")]
        public EFS_Date dtEnd;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTPAYMENT")]
        public EFS_Date dtPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool totalOfDaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("TOTALOFDAY")]
        public EFS_Integer totalOfDay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDC")]
        public string idC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool volatilitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("VOLATILITY")]
        public EFS_Decimal volatility;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool discountFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DISCOUNTFACTOR")]
        public EFS_Decimal discountFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RATE")]
        public EFS_Decimal rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool strikeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("STRIKE")]
        public EFS_Decimal strike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool barrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BARRIER")]
        public EFS_Decimal barrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fwdDeltaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FWDDELTA")]
        public EFS_Decimal fwdDelta;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fwdGammaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FWDGAMMA")]
        public EFS_Decimal fwdGamma;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool vegaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("VEGA")]
        public EFS_Decimal vega;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fxVegaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FXVEGA")]
        public EFS_Decimal fxVega;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool thetaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("THETA")]
        public EFS_Decimal theta;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bpvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("BPV")]
        public EFS_Decimal bpv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool convexitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("CONVEXITY")]
        public EFS_Decimal convexity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deltaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DELTA")]
        public EFS_Decimal delta;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gammaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("GAMMA")]
        public EFS_Decimal gamma;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool npvSpecified;
        [System.Xml.Serialization.XmlElementAttribute("NPV")]
        public EFS_Decimal npv;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtClosingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTCLOSING")]
        public EFS_Date dtClosing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool methodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("METHOD")]
        public string method;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool methodDisplayNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("METHODDISPLAYNAME")]
        public string methodDisplayName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idYieldCurveVal_HSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDYIELDCURVEVAL_H")]
        public EFS_Integer idYieldCurveVal_H;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idYieldCurveDefSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDYIELDCURVEDEF")]
        public string idYieldCurveDef;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool zeroCoupon1Specified;
        [System.Xml.Serialization.XmlElementAttribute("ZEROCOUPON1")]
        public EFS_Decimal zeroCoupon1;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool zeroCoupon2Specified;
        [System.Xml.Serialization.XmlElementAttribute("ZEROCOUPON2")]
        public EFS_Decimal zeroCoupon2;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forwardRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FORWARDRATE")]
        public EFS_Decimal forwardRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]
        public string extlLink;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventProcess
    {
        [System.Xml.Serialization.XmlElementAttribute("IDEP")]
        public int idEP;
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool processSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PROCESS")]
        public string process;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idStProcessSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDSTPROCESS")]
        public string idStProcess;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dtStProcessSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DTSTPROCESS")]
        public EFS_Date dtStProcess;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]
        public string extlLink;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventFee
    {

        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool statusSpecified;
        [System.Xml.Serialization.XmlElementAttribute("STATUS")]
        public string status;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool formulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FORMULA")]
        public string formula;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool formulaValue1Specified;
        [System.Xml.Serialization.XmlElementAttribute("FORMULAVALUE1")]
        public string formulaValue1;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool formulaValue2Specified;
        [System.Xml.Serialization.XmlElementAttribute("FORMULAVALUE2")]
        public string formulaValue2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feePaymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEEPAYMENTFREQUENCY")]
        public string feePaymentFrequency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feePaymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PAYMENTTYPE")]
        public string feePaymentType;

        //EG 20130911 [18076] Add
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assessmentBasisDetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ASSESSMENTBASISDET")]
        public string assessmentBasisDet;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxDetailSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TAXDETAIL")]
        public string taxDetail;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TAXTYPE")]
        public string taxType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TAXRATE")]
        public EFS_Decimal taxRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool taxCountrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("TAXCOUNTRY")]
        public string taxCountry;

    }

    /// <summary>
    /// 
    /// </summary>
    public class SIXML
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] EfsSettlementInstruction;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EventSi
    {
        [System.Xml.Serialization.XmlElementAttribute("IDE")]
        public int idE;
        [System.Xml.Serialization.XmlElementAttribute("SIXML")]
        //public CDATA siXml;
        public SIXML siXml;
        [System.Xml.Serialization.XmlElementAttribute("PAYER_RECEIVER")]
        public string payer_receiver;
        [System.Xml.Serialization.XmlElementAttribute("SIMODE")]
        public string siMode;
        [System.Xml.Serialization.XmlElementAttribute("IDA_CSS")]
        public string idACss;
        [System.Xml.Serialization.XmlElementAttribute("IDA_STLOFFICE")]
        public string idAStlOffice;
        [System.Xml.Serialization.XmlElementAttribute("IDA_MSGRECEIVER")]
        public string idAMsgReceiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extlLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]
        public string extlLink;
    }

    /// <summary>
    /// Représente plusieurs Event 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("Events", IsNullable = false)]
    public class Events
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public Event[] @event;
        #endregion Members

        #region Indexors
        public Event this[int pIdE]
        {
            get
            {
                if ((null != @event) && (0 < @event.Length))
                {
                    foreach (Event item in @event)
                    {
                        if (item.idE == pIdE)
                            return item;
                    }
                }
                return null;
            }
        }
        #endregion Indexors
    }

    /// <summary>
    /// Représente un Event 
    /// <para>Chaque event contient des EventClass, EventSi, EventAsset, EventDetails, etc... </para>
    /// </summary>
    public class Event : EventItem
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventClassSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventClass")]
        public EventClass[] eventClass;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool siSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventSi")]
        public EventSi[] si;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool assetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventAsset")]
        public EventAsset[] asset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventDetails")]
        public EventDetails details;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pricingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventPricing")]
        public EventPricing pricing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pricing2Specified;
        [System.Xml.Serialization.XmlElementAttribute("EventPricing2")]
        public EventPricing2[] pricing2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool processSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventProcess")]
        public EventProcess[] process;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("EventFee")]
        public EventFee[] fee;

        #region Indexors
        public object this[EventTableEnum pEventTableEnum, int pId]
        {
            get
            {
                if (EventTableEnum.Class == pEventTableEnum)
                {
                    if (eventClassSpecified)
                    {
                        foreach (EventClass item in eventClass)
                        {
                            if (item.idEC == pId)
                                return item;
                        }
                    }
                }
                else if (EventTableEnum.Process == pEventTableEnum)
                {
                    if (processSpecified)
                    {
                        foreach (EventProcess item in process)
                        {
                            if (item.idEP == pId)
                                return item;
                        }
                    }
                }
                else if (EventTableEnum.Pricing2 == pEventTableEnum)
                {
                    if (pricing2Specified)
                    {
                        foreach (EventPricing2 item in pricing2)
                        {
                            if (item.idPricing2 == pId)
                                return item;
                        }
                    }
                }
                return null;
            }
        }
        #endregion Indexors

        /// <summary>
        /// Retourne  [{eventCode}-{eventType}]
        /// </summary>
        /// <returns></returns>
        /// FI 20161124 [22634] Add method
        public string GetDisplayName()
        {
            string ret = StrFunc.AppendFormat("[{0}-{1}]", eventCode, eventType);
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("Events", IsNullable = false)]
    public class HierarchicalEventContainer
    {
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public HierarchicalEvent hEvent;  // Pour un Trade un unique Event TRD,DAT (lui même constitué d'autres Evènements Imbriqués)
    }

    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("Events", IsNullable = false)]
    public class HierarchicalEvents
    {
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public HierarchicalEvent[] hEvent;  // Pour un Trade un unique Event TRD,DAT (lui même constitué d'autres Evènements Imbriqués)
    }

    /// <summary>
    /// 
    /// </summary>
    public class HierarchicalEvent : Event
    {
        [System.Xml.Serialization.XmlElementAttribute("Event")]
        public HierarchicalEvent[] hEvent;
    }

    /// <summary>
    /// Cache mémoire d'une liste dévènements
    /// </summary>
    public class DataSetEvent
    {
        #region members
        readonly string _cs;
        private DataSet m_dsEvents;
        private DataSetEventLoadSettings _setting;
        #endregion members

        #region Accessor

        /// <summary>
        /// 
        /// </summary>
        public DataSet DsEvent
        {
            get { return m_dsEvents; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEvent
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("Event"))
                    ret = m_dsEvents.Tables["Event"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventClass
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventClass"))
                    ret = m_dsEvents.Tables["EventClass"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventSi
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventSi"))
                    ret = m_dsEvents.Tables["EventSi"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventAsset
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventAsset"))
                    ret = m_dsEvents.Tables["EventAsset"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventDetails
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventDetails"))
                    ret = m_dsEvents.Tables["EventDetails"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventPricing
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventPricing"))
                    ret = m_dsEvents.Tables["EventPricing"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventPricing2
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventPricing2"))
                    ret = m_dsEvents.Tables["EventPricing2"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventProcess
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventProcess"))
                    ret = m_dsEvents.Tables["EventProcess"];
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventFee
        {
            get
            {
                DataTable ret = null;
                if (m_dsEvents.Tables.Contains("EventFee"))
                    ret = m_dsEvents.Tables["EventFee"];
                return ret;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public DataTable[] DataTable
        {
            get
            {
                DataTable[] ret = null;
                if ((DsEvent.Tables.Count) > 0)
                {
                    ret = new DataTable[DsEvent.Tables.Count];
                    DsEvent.Tables.CopyTo((DataTable[])ret, 0);
                }
                return ret;
            }
        }

        #endregion Accessor

        #region constructor
        public DataSetEvent(string pCS)
        {
            _cs = pCS;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Charge les évènements du trade 
        /// <para>Les évènements sont triés par IDE</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        public void Load(IDbTransaction pDbTransaction, int pIdT)
        {
            Load(pDbTransaction, pIdT, null, new DataSetEventLoadSettings(DataSetEventLoadEnum.EventComplete));
        }

        /// <summary>
        /// Charge les évènements
        /// <para>Les évènements sont triés par IDE</para>
        /// </summary>
        /// <param name="pIdE"></param>
        public void Load(IDbTransaction pDbTransaction, int[] pIdE)
        {
            Load(pDbTransaction, 0, pIdE, new DataSetEventLoadSettings(DataSetEventLoadEnum.EventComplete));
        }

        /// <summary>
        /// Charge les évènements qui appartiennent à un même trade  
        /// <para>Les évènements sont triés par IDE</para>
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSettings"></param>
        public void Load(IDbTransaction pDbTransaction, int pIdT, int[] pIdE, DataSetEventLoadSettings pSettings)
        {
            Load(pDbTransaction, new int[] { pIdT }, pIdE, pSettings);
        }

        /// <summary>
        /// Charge les évènements triés par IDE qui appartiennent à une liste de trade  
        /// <para>Les évènements sont triés par IDE</para>
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pSettings"></param>
        public void Load(IDbTransaction pDbTransaction, int[] pIdT, int[] pIdE, DataSetEventLoadSettings pSettings)
        {
            Load(pDbTransaction, string.Empty, pIdT, pIdE, pSettings);
        }

        /// <summary>
        /// Charge les évènements triés par IDE qui appartiennent à une liste de trade  
        /// <para>Les évènements sont triés par IDE</para>
        /// </summary>
        /// <param name="pdbTransaction"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pSettings"></param>
        /// FI 20120531 Refactoring =>  usage d'un inner pour améliorer les perfs de chargement
        /// Dans la messagerie le in peut contenir plus de 500 trades 
        /// Avec un in classique les perfs d'écroulent
        /// RD 20130430 [] Utilisation des tables TRADELIST et EVENTLIST
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        public void Load(IDbTransaction pdbTransaction, string pSessionId, int[] pIdT, int[] pIdE, DataSetEventLoadSettings pSettings)
        {
            _setting = pSettings;

            SQLWhere sqlWhere = new SQLWhere();
            DataParameters dataParams = new DataParameters();
            string innerJoin = string.Empty;

            bool isUseTRADELIST = (StrFunc.IsFilled(pSessionId) && (ArrFunc.Count(pIdT) > TradeRDBMSTools.SqlINListMax));
            bool isUseEVENTLIST = (StrFunc.IsFilled(pSessionId) && (ArrFunc.Count(pIdE) > EventRDBMSTools.SqlINListMax));

            if (isUseTRADELIST || isUseEVENTLIST)
                dataParams.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.SESSIONID), pSessionId);

            if (ArrFunc.IsFilled(pIdT))
            {
                if (ArrFunc.Count(pIdT) == 1)
                {
                    dataParams.Add(new DataParameter(_cs, "IDT", System.Data.DbType.Int32), pIdT[0]);
                    sqlWhere.Append(@"(ev.IDT = @IDT)");
                }
                else if (false == isUseTRADELIST)
                {
                    //FI 20120531 add innerJoin usage d'un inner 
                    SQLWhere SQLWhereTrade = new SQLWhere();
                    SQLWhereTrade.Append(@"(" + DataHelper.SQLColumnIn(_cs, "tr.IDT", pIdT, TypeData.TypeDataEnum.integer, false, true) + ")");
                    innerJoin = SQLCst.X_INNER + "(select tr.IDT from dbo.TRADE tr " + Cst.CrLf + SQLWhereTrade.ToString() + ") lst on (lst.IDT = ev.IDT)" + Cst.CrLf;
                }
                else
                {
                    // 1- Vider la table TRADELIST
                    TradeRDBMSTools.DeleteTradeList(_cs, pSessionId);

                    // 2- Insérer la liste des IDT dans la table TRADELIST
                    TradeRDBMSTools.InsertTradeList(_cs, pIdT, pSessionId);

                    // 3- Utiliser la table TRADELIST en jointure
                    innerJoin = TradeRDBMSTools.SqlInnerTRADELIST + Cst.CrLf;
                }
            }

            if (ArrFunc.IsFilled(pIdE))
            {
                if (ArrFunc.Count(pIdE) == 1)
                {
                    dataParams.Add(new DataParameter(_cs, "IDE", System.Data.DbType.Int32), pIdE[0]);
                    sqlWhere.Append(@"(ev.IDE = @IDE)");
                }
                else if (false == isUseEVENTLIST)
                    sqlWhere.Append(@"(" + DataHelper.SQLColumnIn(_cs, "ev.IDE", pIdE, TypeData.TypeDataEnum.integer, false, true) + ")");
                else
                {
                    // 1- Vider la table EVENTLIST
                    EventRDBMSTools.DeleteEventList(_cs, pSessionId);

                    // 2- Insérer la liste des IDE dans la table EVENTLIST
                    EventRDBMSTools.InsertEventList(_cs, pIdE, pSessionId);

                    // 3- Utiliser la table EVENTLIST en jointure
                    innerJoin += EventRDBMSTools.SqlInnerEVENTLIST + Cst.CrLf;
                }
            }

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += GetSelectEventColumn() + Cst.CrLf;
            if (StrFunc.IsFilled(innerJoin))
                sqlSelect += innerJoin;
            sqlSelect += sqlWhere.ToString() + Cst.CrLf;
            //sqlSelect += SQLCst.ORDERBY + "ev.IDE";
            sqlSelect += SQLCst.ORDERBY + "ev.SORTINSTRUMENT, ev.SORTSTREAM, ev.IDE_EVENT, ev.SORTORDER, ev.DTSTARTADJ, ev.DTENDADJ" + Cst.CrLf;
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;
            //
            if (pSettings.isLoadEventClass)
            {
                sqlSelect += GetSelectEventClassColumn();
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString();
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }
            if (pSettings.isLoadEventSI)
            {
                sqlSelect += GetSelectEventSiColumn() + Cst.CrLf;
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString();
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }
            if (pSettings.isLoadEventAsset)
            {
                //sqlSelect += GetSelectEventAssetColumn() + Cst.CrLf;
                sqlSelect += QueryLibraryTools.GetQuerySelect(_cs, Cst.OTCml_TBL.EVENTASSET) + Cst.CrLf;

                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString();
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }
            if (pSettings.isLoadEventDetail)
            {
                sqlSelect += GetSelectEventDetailsColumn() + Cst.CrLf;
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString();
                sqlSelect += GetSqlWhereEventDetails(pSettings, sqlWhere) + Cst.CrLf;
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }
            if (pSettings.isLoadEventPricing)
            {
                sqlSelect += GetSelectEventPricingColumn() + Cst.CrLf;
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString() + Cst.CrLf;
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;

                sqlSelect += GetSelectEventPricing2Column() + Cst.CrLf;
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString() + Cst.CrLf;

                sqlSelect += SQLCst.ORDERBY + "ep.IDE_SOURCE" + Cst.CrLf;
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }
            if (pSettings.isLoadEventProcess)
            {
                sqlSelect += GetSelectEventProcessColumn() + Cst.CrLf;
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString();
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }

            if (pSettings.isLoadEventFee)
            {
                sqlSelect += GetSelectEventFeeColumn() + Cst.CrLf;
                if (StrFunc.IsFilled(innerJoin))
                    sqlSelect += innerJoin;
                sqlSelect += sqlWhere.ToString();
                sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            }

            //
            QueryParameters qryParamters = new QueryParameters(_cs, sqlSelect.ToString(), dataParams);
            //
            m_dsEvents = DataHelper.ExecuteDataset(qryParamters.Cs, pdbTransaction, CommandType.Text, qryParamters.Query, qryParamters.Parameters.GetArrayDbParameter());

            #region DataSet Initialize
            m_dsEvents.DataSetName = "Events";
            m_dsEvents.Tables[0].TableName = "Event";
            //
            int k = 0;
            if (pSettings.isLoadEventClass)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventClass";
            }
            if (pSettings.isLoadEventSI)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventSi";
            }
            if (pSettings.isLoadEventAsset)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventAsset";
            }
            if (pSettings.isLoadEventDetail)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventDetails";
            }
            if (pSettings.isLoadEventPricing)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventPricing";
                k++;
                m_dsEvents.Tables[k].TableName = "EventPricing2";
            }
            if (pSettings.isLoadEventProcess)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventProcess";
            }
            if (pSettings.isLoadEventFee)
            {
                k++;
                m_dsEvents.Tables[k].TableName = "EventFee";
            }
            //
            InitializeRelations();
            #endregion DataSet Initialize

            // Vider la table TRADELIST
            if (isUseTRADELIST)
                TradeRDBMSTools.DeleteTradeList(_cs, pSessionId);

            // Vider la table EVENTLIST
            if (isUseEVENTLIST)
                EventRDBMSTools.DeleteEventList(_cs, pSessionId);
        }

        /// <summary>
        /// Pour charger EventDetails que pour un Event Specifique
        /// </summary>
        /// <param name="pSettings"></param>
        /// <param name="pSqlWhere"></param>
        /// <returns></returns>
        private static string GetSqlWhereEventDetails(DataSetEventLoadSettings pSettings, SQLWhere pSqlWhere)
        {
            string ret = string.Empty;
            
            if ((StrFunc.IsFilled(pSettings.eventCodeWithDet)) || (StrFunc.IsFilled(pSettings.eventTypeWithDet)))
            {
                if (StrFunc.IsFilled(pSqlWhere.ToString()))
                    ret = SQLCst.AND;
                else
                    ret = SQLCst.WHERE;
             
                if (StrFunc.IsFilled(pSettings.eventCodeWithDet))
                    ret += @"(ev.EVENTCODE = " + DataHelper.SQLString(pSettings.eventCodeWithDet) + ")" + SQLCst.AND;
             
                if (StrFunc.IsFilled(pSettings.eventTypeWithDet))
                    ret += @"(ev.EVENTTYPE = " + DataHelper.SQLString(pSettings.eventTypeWithDet) + ")";
             
                ret = ret.TrimEnd(SQLCst.AND.ToCharArray());
            }
            
            return ret;
        }

        /// <summary>
        /// Retourne la liste des évènements présents ds DtEvent
        /// </summary>
        /// <returns></returns>
        public EventItems GetEventItems()
        {
            EventItems ret = null;
            if ((null != DsEvent) && (0 < DtEvent.Rows.Count))
            {
                //
                InitializeRelations();
                //
                if (DsEvent.Relations.Contains(DtEvent.TableName))
                    DsEvent.Relations.Remove(DtEvent.TableName);

                string serializeResult = new DatasetSerializer(DsEvent).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(EventItems), serializeResult);
                ret = (EventItems)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la liste des évènements présents ds DtEvent, les évènements sont hierarchisés
        /// </summary>
        /// <returns></returns>
        public HierarchicalEventContainer GetTradeEvents()
        {
            HierarchicalEventContainer ret = null;
            if ((null != DsEvent) && (0 < DtEvent.Rows.Count))
            {
                InitializeRelations();
                string serializeResult = new DatasetSerializer(DsEvent).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(HierarchicalEventContainer), serializeResult);
                ret = (HierarchicalEventContainer)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la liste des évènements enfant d'un IDE présents ds DtEvent, les évènements sont hierarchisés
        /// </summary>
        /// <param name="pIDE"></param>
        /// <returns></returns>
        public HierarchicalEventContainer GetChildEvents(int pIDE)
        {
            HierarchicalEventContainer ret = null;
            InitializeRelations();
            //
            if ((null != DsEvent) && (0 < DtEvent.Rows.Count))
            {
                DataSet dsClone = DsEvent.Clone();
                //
                DataRow rowIdE = null;
                DataRow[] row = DtEvent.Select("IDE=" + pIDE.ToString());
                if (ArrFunc.IsFilled(row))
                    rowIdE = row[0];
                //
                if (null != rowIdE)
                {
                    //Stocker l'Identifier du Trade pour le mettre uniquement sur l'Event Racine
                    string identifierTrade = rowIdE["IDENTIFIER_TRADE"].ToString();
                    //Event
                    ImportChildRowEventInTable(dsClone.Tables["Event"], rowIdE);
                    //pas de parent pour le 1er item, c'est la racine
                    dsClone.Tables["Event"].BeginInit();
                    dsClone.Tables["Event"].Rows[0]["IDE_EVENT"] = Convert.DBNull;
                    //Mettre l'Identifier du Trade uniquement sur l'Event Racine
                    dsClone.Tables["Event"].Rows[0]["IDENTIFIER_TRADE"] = identifierTrade;
                    dsClone.Tables["Event"].EndInit();
                    //
                    string[] ide = new string[dsClone.Tables["Event"].Rows.Count];
                    for (int i = 0; i < ide.Length; i++)
                        ide[i] = Convert.ToString(dsClone.Tables["Event"].Rows[i]["IDE"]);
                    //
                    string inExpression = "IDE IN " + "(" + StrFunc.StringArrayList.StringArrayToStringList(ide, false).Replace(";", ",") + ")";
                    //
                    //EventClass
                    if (null != DtEventClass)
                    {
                        row = DtEventClass.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventClass.TableName].ImportRow(row[i]);
                    }
                    //
                    //EventSI
                    if (null != DtEventSi)
                    {
                        row = DtEventSi.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventSi.TableName].ImportRow(row[i]);
                    }
                    //
                    //EventAsset
                    if (null != DtEventAsset)
                    {
                        row = DtEventAsset.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventAsset.TableName].ImportRow(row[i]);
                    }
                    //
                    //EventDetails
                    if (null != DtEventDetails)
                    {
                        row = DtEventDetails.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventDetails.TableName].ImportRow(row[i]);
                    }
                    //
                    //EventPricing
                    if (null != DtEventPricing)
                    {
                        row = DtEventPricing.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventPricing.TableName].ImportRow(row[i]);
                    }
                    //
                    //EventProcess
                    if (null != DtEventProcess)
                    {
                        row = DtEventProcess.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventProcess.TableName].ImportRow(row[i]);
                    }
                    //EventProcess
                    if (null != DtEventFee)
                    {
                        row = DtEventFee.Select(inExpression);
                        for (int i = 0; i < ArrFunc.Count(row); i++)
                            dsClone.Tables[DtEventFee.TableName].ImportRow(row[i]);
                    }

                    //
                    string serializeResult = new DatasetSerializer(dsClone).Serialize();
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(HierarchicalEventContainer), serializeResult);
                    ret = (HierarchicalEventContainer)CacheSerializer.Deserialize(serializeInfo);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne la liste des évènements présents ds DtEvent
        /// <para>Retourne null si le dataset n'est pas chargé</para>
        /// </summary>
        /// <returns></returns>
        public Events GetEvents()
        {
            Events ret = null;
            if ((null != DsEvent) && (0 < DtEvent.Rows.Count))
            {
                InitializeRelations();
                //
                if (DsEvent.Relations.Contains(DtEvent.TableName))
                    DsEvent.Relations.Remove(DtEvent.TableName);
                //
                string serializeResult = new DatasetSerializer(DsEvent).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(Events), serializeResult);
                ret = (Events)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;

        }

        /// <summary>
        /// Retourne les IDE tels que 
        /// <para>
        /// EVENTCODE = {pEventCode}
        /// </para>
        /// <para>
        /// EVENTTYPE = {pEventType}, {pEventType} est optionnel
        /// </para>
        /// <para>
        /// EVENTCLASS = {pEventClass} 
        /// </para>
        /// <para>
        /// DTEVENTCLASS ou DTEVENTCLASSFORCED = {pDate}
        /// </para>
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType">null autorisé</param>
        /// <param name="pEventClass"></param>
        /// <param name="pDate"></param>
        /// <param name="pUsedEventForced"></param>
        /// <returns></returns>
        public int[] GetIDE(string pEventCode, string pEventType, string pEventClass, DateTime pDate, bool pUsedEventForced)
        {
            int[] ret = null;
            //
            if (null != DtEvent)
            {
                ArrayList al = new ArrayList();
                Events eventTrade = GetEvents();
                if (null != eventTrade)
                {
                    Event[] @event = eventTrade.@event;
                    for (int i = 0; i < ArrFunc.Count(@event); i++)
                    {
                        bool isToAdd = (pEventCode == @event[i].eventCode);

                        if (isToAdd)
                        {
                            if (StrFunc.IsFilled(pEventType))
                                isToAdd = (pEventType == @event[i].eventType);
                        }

                        if (isToAdd)
                        {
                            isToAdd = false;
                            if (@event[i].eventClassSpecified)
                            {
                                for (int j = 0; j < ArrFunc.Count(@event[i].eventClass); j++)
                                {
                                    EventClass ec = @event[i].eventClass[j];
                                    if (ec.codeSpecified)
                                    {
                                        if (DtFunc.IsDateTimeFilled(pDate))
                                        {
                                            if ((ec.dtEventForcedSpecified) && pUsedEventForced)
                                                isToAdd = (ec.dtEventForced.DateValue == pDate) && (ec.code == pEventClass);
                                            else if (ec.dtEventSpecified)
                                                isToAdd = (ec.dtEvent.DateValue == pDate) && (ec.code == pEventClass);
                                        }
                                        else
                                            isToAdd = (ec.code == pEventClass);
                                    }
                                    if (isToAdd)
                                        break;
                                }
                            }
                        }
                        //
                        if (isToAdd)
                            al.Add(@event[i].idE);
                    }
                    //
                    if (ArrFunc.IsFilled(al))
                        ret = (int[])al.ToArray(typeof(int));
                }
            }
            return ret;
        }

        /// <summary>
        /// Insertion/modification d'un évènement
        /// <para>Mise à jour des tables EVENT, EVENTSTMATCH, EVENTSTCHECK</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pEventIndex">Index dans dtEvent (utilisé en si modification d'un évènement existant uniquement)</param>
        /// <param name="pEvent">Contient les caractéristiques de l'évènement 
        /// <para>Si pEvent.idE == -1 => Création d'un nouvel évènement</para>
        /// <para>En sortie pEvent.idE contient l'identifiant de l'évènement généré</para>
        /// </param>
        /// <param name="pStatus">Status de l'évènement (valeur null autorisé lors d'une modification dévènement)</param>
        /// <param name="pSession"></param>
        /// 20161123 [22629] Modify
        public void UpdateEvent(IDbTransaction pDbTransaction, int pEventIndex, Event pEvent, EventStatus pStatus, AppSession pSession)
        {
            // FI 20200820 [25468] Dates systemes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(_cs);

            string SQLQuery = GetSelectEventColumn(true);
            Boolean isModeNew = (pEvent.idE == -1);

            if ((isModeNew) && (pStatus == null))
                throw new ArgumentException("{pStatus} is null");
            int idE = int.MinValue;


            // 20161123 [22629] Insertion d'un nouvel évènement
            DataRow rowEvent;
            if (isModeNew)
            {
                // add EVENT
                rowEvent = DtEvent.NewRow();
                Cst.ErrLevel errLevel = SQLUP.GetId(out idE, this._cs, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                if (Cst.ErrLevel.SUCCESS != errLevel)
                    throw new Exception(StrFunc.AppendFormat("Error:{0} on get Id for New Event", errLevel.ToString()));

                pEvent.idE = idE;

                if (pEvent.eventClassSpecified)
                {
                    foreach (EventClass item in pEvent.eventClass)
                        item.idE = pEvent.idE;
                }
                DtEvent.Rows.Add(rowEvent);
            }
            else
            {
                rowEvent = DtEvent.Rows[pEventIndex];
            }

            #region Update columns
            rowEvent.BeginEdit();
            if (isModeNew)
            {
                rowEvent["IDE"] = idE;
                rowEvent["IDT"] = pEvent.idT;
                rowEvent["IDE_EVENT"] = pEvent.idEParent;
                rowEvent["EVENTCODE"] = pEvent.eventCode;
                rowEvent["EVENTTYPE"] = pEvent.eventType;

                rowEvent["DTSTARTUNADJ"] = pEvent.dtStartPeriodSpecified ? pEvent.dtStartPeriod.DateValue : Convert.DBNull;
                rowEvent["DTENDUNADJ"] = pEvent.dtEndPeriodSpecified ? pEvent.dtEndPeriod.DateValue : Convert.DBNull;

                rowEvent["IDSTCALCUL"] = pEvent.idStCalcul;
                rowEvent["IDSTTRIGGER"] = StrFunc.IsFilled(pEvent.idStTrigger) ? pEvent.idStTrigger : "NA";

                rowEvent["SOURCE"] = pEvent.source;
                rowEvent["INSTRUMENTNO"] = pEvent.instrumentNo;
                rowEvent["STREAMNO"] = pEvent.streamNo;

                rowEvent["UNITTYPE"] = "Currency";
            }

            rowEvent["DTSTARTADJ"] = pEvent.dtStartPeriodSpecified ? pEvent.dtStartPeriod.DateValue : Convert.DBNull;
            rowEvent["DTENDADJ"] = pEvent.dtEndPeriodSpecified ? pEvent.dtEndPeriod.DateValue : Convert.DBNull;
            rowEvent["VALORISATION"] = pEvent.valorisationSpecified ? pEvent.valorisation.DecValue : Convert.DBNull;
            rowEvent["UNIT"] = pEvent.unitSpecified ? pEvent.unit : Convert.DBNull;

            rowEvent["IDA_PAY"] = pEvent.idPayerSpecified ? pEvent.idPayer : Convert.DBNull;
            rowEvent["IDB_PAY"] = pEvent.idPayerBookSpecified ? pEvent.idPayerBook : Convert.DBNull;
            rowEvent["IDA_REC"] = pEvent.idReceiverSpecified ? pEvent.idReceiver : Convert.DBNull;
            rowEvent["IDB_REC"] = pEvent.idReceiverBookSpecified ? pEvent.idReceiverBook : Convert.DBNull;

            if (null != pStatus)
            {
                rowEvent["IDSTACTIVATION"] = pStatus.stActivation.NewSt;
                rowEvent["IDASTACTIVATION"] = pSession.IdA;
                rowEvent["DTSTACTIVATION"] = dtSys;
            }
            rowEvent.EndEdit();
            #endregion Update columns

            DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, DtEvent);

            if (null != pStatus)
                pStatus.UpdateStUser(this._cs, pDbTransaction, Mode.Event, pEvent.idE, pSession.IdA, dtSys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCurrentEventItem"></param>
        /// <param name="pEvent"></param>
        public void UpdateEventAsset(IDbTransaction pDbTransaction, Event pEvent)
        {

            string SQLQuery = GetSelectEventAssetColumn(true);
            EventAsset[] eventAssets = pEvent.asset;
            DataRow[] rowEventAssets = DtEventAsset.Select("IDE=" + pEvent.idE);
            DataRow rowEventAsset = null;

            if (pEvent.assetSpecified)
            {
                for (int i = 0; i < eventAssets.Length; i++)
                {
                    try { rowEventAsset = rowEventAssets[i]; }
                    catch (System.IndexOutOfRangeException)
                    {
                        //string s = ex.Message;
                        rowEventAsset = DtEventAsset.NewRow();
                        DtEventAsset.Rows.Add(rowEventAsset);
                    }
                    finally
                    {
                        if (eventAssets[i].idAssetSpecified)
                        {
                            SetRowEventAsset(rowEventAsset, pEvent.idE, eventAssets[i]);
                        }
                        else
                        {
                            rowEventAsset.Delete();
                        }
                    }
                }
                DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, DtEventAsset);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCurrentEventItem"></param>
        /// <param name="pEvent"></param>
        public void UpdateEventClass(IDbTransaction pDbTransaction, Event pEvent)
        {
            string SQLQuery = GetSelectEventClassColumn(true);
            EventClass[] eventClasses = pEvent.eventClass;
            if (pEvent.eventClassSpecified)
            {

                DataRow[] rowEventClasses;
                foreach (EventClass eventClass in eventClasses)
                {
                    DataRow rowEventClass = null;

                    rowEventClasses = DtEventClass.Select("IDEC=" + eventClass.idEC + " and IDE=" + pEvent.idE);
                    if ((null != rowEventClasses) && (0 < rowEventClasses.Length))
                    {
                        rowEventClass = rowEventClasses[0];
                    }
                    else
                    {
                        rowEventClass = DtEventClass.NewRow();
                        DtEventClass.Rows.Add(rowEventClass);
                    }
                    #region Update columns
                    rowEventClass.BeginEdit();
                    rowEventClass["IDE"] = pEvent.idE;
                    rowEventClass["EVENTCLASS"] = eventClass.code;
                    rowEventClass["DTEVENT"] = eventClass.dtEvent.DateValue;
                    rowEventClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(_cs, eventClass.dtEvent.DateValue);
                    if (eventClass.isPaymentSpecified)
                        rowEventClass["ISPAYMENT"] = eventClass.isPayment.BoolValue;
                    else // RD 20100517 / Bug Sous Oracle: Cette colonne doit être renseignée
                        rowEventClass["ISPAYMENT"] = false;
                    //
                    rowEventClass.EndEdit();
                    #endregion Update columns
                }

                rowEventClasses = DtEventClass.Select("IDE=" + pEvent.idE);
                if ((null != rowEventClasses) && (eventClasses.Length < rowEventClasses.Length))
                {
                    foreach (DataRow row in rowEventClasses)
                    {
                        if (false == Convert.IsDBNull(row["IDEC"]))
                        {
                            if (null == pEvent[EventTableEnum.Class, Convert.ToInt32(row["IDEC"])])
                                row.Delete();
                        }
                    }
                }
                DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, DtEventClass);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCurrentEventItem"></param>
        /// <param name="pEvent"></param>
        public void UpdateEventDetail(IDbTransaction pDbTransaction, Event pEvent)
        {

            string SQLQuery = GetSelectEventDetailsColumn(true);
            
            EventDetails eventDetail = pEvent.details;
            DataRow[] rowEventDetails = DtEventDetails.Select("IDE=" + pEvent.idE);

            DataRow rowEventDetail;
            if ((null != rowEventDetails) && (0 < rowEventDetails.Length))
            {
                rowEventDetail = rowEventDetails[0];
            }
            else
            {
                rowEventDetail = DtEventDetails.NewRow();
                DtEventDetails.Rows.Add(rowEventDetail);
            }
            if (pEvent.detailsSpecified)
            {
                #region Update columns
                rowEventDetail.BeginEdit();
                rowEventDetail["IDE"] = pEvent.idE;
                if (eventDetail.noteSpecified)
                    rowEventDetail["NOTE"] = eventDetail.note;

                if (eventDetail.extlLinkSpecified)
                    rowEventDetail["EXTLLINK"] = eventDetail.extlLink;
                rowEventDetail.EndEdit();
                #endregion Update columns
            }
            else
                rowEventDetail.Delete();

            DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, DtEventDetails);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCurrentEventItem"></param>
        /// <param name="pEvent"></param>
        public void UpdateEventPricing(IDbTransaction pDbTransaction, Event pEvent)
        {

            string SQLQuery = GetSelectEventPricingColumn();
            //EventPricing eventPricing = pEvent.pricing;
            DataRow[] rowEventPricings = DtEventPricing.Select("IDE=" + pEvent.idE);
            DataRow rowEventPricing;
            if ((null != rowEventPricings) && (0 < rowEventPricings.Length))
                rowEventPricing = rowEventPricings[0];
            else
            {
                rowEventPricing = DtEventPricing.NewRow();
                DtEventPricing.Rows.Add(rowEventPricing);
            }
            if (pEvent.pricingSpecified)
            {
                #region Update columns
                rowEventPricing.BeginEdit();
                rowEventPricing["IDE"] = pEvent.idE;
                rowEventPricing.EndEdit();
                #endregion Update columns
            }
            else
                rowEventPricing.Delete();

            DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, DtEventPricing);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCurrentEventItem"></param>
        /// <param name="pEvent"></param>
        public void UpdateEventProcess(IDbTransaction pDbTransaction, Event pEvent)
        {
            string SQLQuery = GetSelectEventProcessColumn(true);
            
            EventProcess[] processes = pEvent.process;
            if (pEvent.processSpecified)
            {
                DataRow[] rowEventProcesses;
                foreach (EventProcess process in processes)
                {
                    rowEventProcesses = DtEventProcess.Select("IDEP=" + process.idEP);
                    DataRow rowEventProcess;
                    if ((null != rowEventProcesses) && (0 < rowEventProcesses.Length))
                    {
                        rowEventProcess = rowEventProcesses[0];
                    }
                    else
                    {
                        rowEventProcess = DtEventProcess.NewRow();
                        DtEventProcess.Rows.Add(rowEventProcess);
                    }
                    #region Update columns
                    rowEventProcess.BeginEdit();
                    rowEventProcess["IDE"] = pEvent.idE;
                    rowEventProcess["PROCESS"] = process.process;
                    rowEventProcess["IDSTPROCESS"] = process.idStProcess;
                    if ((null != process.dtStProcess) && DtFunc.IsDateTimeFilled(process.dtStProcess.DateValue))
                    {
                        rowEventProcess["DTSTPROCESS"] = process.dtStProcess.DateValue;
                    }
                    else
                    {
                        // FI 20200820 [25468] Dates systemes en UTC
                        rowEventProcess["DTSTPROCESS"] = OTCmlHelper.GetDateSysUTC(_cs);
                    }
                    rowEventProcess.EndEdit();
                    #endregion Update columns
                }

                rowEventProcesses = DtEventProcess.Select("IDE=" + pEvent.idE);
                if ((null != rowEventProcesses) && (processes.Length < rowEventProcesses.Length))
                {
                    foreach (DataRow row in rowEventProcesses)
                    {
                        if (false == Convert.IsDBNull(row["IDEP"]))
                        {
                            if (null == pEvent[EventTableEnum.Process, Convert.ToInt32(row["IDEP"])])
                                row.Delete();
                        }
                    }
                }
            }
            DataHelper.ExecuteDataAdapter(pDbTransaction, SQLQuery, DtEventProcess);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeRelations()
        {

            DsEvent.Relations.Clear();
            //
            if (null != DtEvent)
            {
                DataRelation relEvent = new DataRelation(DtEvent.TableName, DtEvent.Columns["IDE"], DtEvent.Columns["IDE_EVENT"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEvent);
            }
            //
            if (null != DtEventClass)
            {
                DataRelation relEventClass = new DataRelation(DtEventClass.TableName, DtEvent.Columns["IDE"], DtEventClass.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventClass);
            }
            if (null != DtEventSi)
            {
                DataRelation relEventSi = new DataRelation(DtEventSi.TableName, DtEvent.Columns["IDE"], DtEventSi.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventSi);
            }
            //
            if (null != DtEventDetails)
            {
                DataRelation relEventDet = new DataRelation(DtEventDetails.TableName, DtEvent.Columns["IDE"], DtEventDetails.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventDet);
            }
            //
            if (null != DtEventAsset)
            {
                DataRelation relEventAsset = new DataRelation(DtEventAsset.TableName, DtEvent.Columns["IDE"], DtEventAsset.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventAsset);
            }
            //
            if (null != DtEventPricing)
            {
                DataRelation relEventPricing = new DataRelation(DtEventPricing.TableName, DtEvent.Columns["IDE"], DtEventPricing.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventPricing);
            }
            //
            if (null != DtEventPricing2)
            {
                DataRelation relEventPricing2 = new DataRelation(DtEventPricing2.TableName, DtEvent.Columns["IDE"], DtEventPricing2.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventPricing2);
            }
            if (null != DtEventProcess)
            {
                DataRelation relEventProcess = new DataRelation(DtEventProcess.TableName, DtEvent.Columns["IDE"], DtEventProcess.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventProcess);
            }

            if (null != DtEventFee)
            {
                DataRelation relEventFee = new DataRelation(DtEventFee.TableName, DtEvent.Columns["IDE"], DtEventFee.Columns["IDE"], false)
                {
                    Nested = true
                };
                DsEvent.Relations.Add(relEventFee);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRowEventAsset"></param>
        /// <param name="pIdE"></param>
        /// <param name="pEventAsset"></param>
        /// FI 20140930 [XXXXX] Modify
        private static void SetRowEventAsset(DataRow pRowEventAsset, int pIdE, EventAsset pEventAsset)
        {
            pRowEventAsset.BeginEdit();
            pRowEventAsset["IDE"] = pIdE;
            pRowEventAsset["IDASSET"] = pEventAsset.idAsset;
            pRowEventAsset["ASSETTYPE"] = pEventAsset.assetType;
            pRowEventAsset["ASSETCATEGORY"] = pEventAsset.assetCategory;
            pRowEventAsset["IDBC"] = pEventAsset.idBCSpecified ? pEventAsset.idBC : Convert.DBNull;
            pRowEventAsset["PRIMARYRATESRC"] = pEventAsset.primaryRateSrcSpecified ? pEventAsset.primaryRateSrc : Convert.DBNull;
            pRowEventAsset["PRIMARYRATESRCPAGE"] = pEventAsset.primaryRateSrcPageSpecified ? pEventAsset.primaryRateSrcPage : Convert.DBNull;
            pRowEventAsset["PRIMARYRATESRCHEAD"] = pEventAsset.primaryRateSrcHeadSpecified ? pEventAsset.primaryRateSrcHead : Convert.DBNull;
            pRowEventAsset["QUOTESIDE"] = pEventAsset.quoteSideSpecified ? pEventAsset.quoteSide : Convert.DBNull;
            pRowEventAsset["QUOTETIMING"] = pEventAsset.quoteTimingSpecified ? pEventAsset.quoteTiming : Convert.DBNull;
            pRowEventAsset["TIME"] = pEventAsset.timeSpecified ? pEventAsset.time.TimeValue : Convert.DBNull;

            pRowEventAsset["IDM"] = pEventAsset.idMSpecified ? pEventAsset.idM.IntValue : Convert.DBNull;
            pRowEventAsset["ISINCODE"] = pEventAsset.isinCodeSpecified ? pEventAsset.isinCode : Convert.DBNull;
            pRowEventAsset["ASSETSYMBOL"] = pEventAsset.assetSymbolSpecified ? pEventAsset.assetSymbol : Convert.DBNull;
            pRowEventAsset["IDENTIFIER"] = pEventAsset.identifierSpecified ? pEventAsset.identifier : Convert.DBNull;
            pRowEventAsset["DISPLAYNAME"] = pEventAsset.displayNameSpecified ? pEventAsset.displayName : Convert.DBNull;
            pRowEventAsset["CONTRACTSYMBOL"] = pEventAsset.contractSymbolSpecified ? pEventAsset.contractSymbol : Convert.DBNull;
            pRowEventAsset["CATEGORY"] = pEventAsset.categorySpecified ? pEventAsset.category : Convert.DBNull;
            pRowEventAsset["PUTORCALL"] = pEventAsset.putOrCallSpecified ? pEventAsset.putOrCall : Convert.DBNull;
            pRowEventAsset["STRIKEPRICE"] = pEventAsset.strikePriceSpecified ? pEventAsset.strikePrice.DecValue : Convert.DBNull;
            pRowEventAsset["CONTRACTMULTIPLIER"] = pEventAsset.contractMultiplierSpecified ? pEventAsset.contractMultiplier.DecValue : Convert.DBNull;
            pRowEventAsset["MATURITYDATE"] = pEventAsset.maturityDateSpecified ? pEventAsset.maturityDate.DateTimeValue : Convert.DBNull;
            pRowEventAsset["DELIVERYDATE"] = pEventAsset.deliveryDateSpecified ? pEventAsset.deliveryDate.DateTimeValue : Convert.DBNull;
            pRowEventAsset["NOMINALVALUE"] = pEventAsset.nominalValueSpecified ? pEventAsset.nominalValue.DecValue : Convert.DBNull;
            pRowEventAsset.EndEdit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventAssetColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ea.IDE, ea.IDASSET, ea.ASSETTYPE, ea.ASSETCATEGORY, ea.TIME, ea.IDBC, ea.QUOTESIDE, ea.QUOTETIMING,
            ea.PRIMARYRATESRC, ea.PRIMARYRATESRCPAGE, ea.PRIMARYRATESRCHEAD,
            ea.IDM, ea.ISINCODE, ea.ASSETSYMBOL, ea.IDENTIFIER, ea.DISPLAYNAME,
            ea.CONTRACTSYMBOL, ea.CATEGORY, ea.PUTORCALL, ea.STRIKEPRICE,
            ea.CONTRACTMULTIPLIER, ea.MATURITYDATE, ea.DELIVERYDATE, ea.NOMINALVALUE
            from dbo.EVENTASSET ea " + Cst.CrLf;
            if (false == pWithOnlyTblMain)
                sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ea.IDE)" + Cst.CrLf;
            return sqlSelect;
        }

        /// <summary>
        /// Retourne la commande Select sur la table EVENT et les tables satellites
        /// </summary>
        /// <returns></returns>
        private string GetSelectEventColumn()
        {
            return GetSelectEventColumn(false);
        }
        /// <summary>
        /// Retourne la commande Select sur la table EVENT et potentiellement sur les tables satellites
        /// </summary>
        /// <returns></returns>
        /// <param name="pWithOnlyTblMain">si true Select sur la table EVENT uniquement</param>
        /// <returns></returns>
        /// //EG 20100127 Add columns IDT/IDENTIFIER du trade source éventuel
        /// //FI 20120426 [17703] add join on EVENTPOSACTIONDET
        private string GetSelectEventColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ev.IDE, ev.IDT, ev.INSTRUMENTNO, ev.STREAMNO, ev.IDE_EVENT, ev.IDE_SOURCE, 
            ev.IDA_PAY, ev.IDB_PAY, ev.IDA_REC, ev.IDB_REC, ev.EVENTCODE, ev.EVENTTYPE, 
            ev.DTSTARTADJ, ev.DTSTARTUNADJ, ev.DTENDADJ, ev.DTENDUNADJ, 
            ev.VALORISATION, ev.UNIT, ev.UNITTYPE, ev.VALORISATIONSYS, ev.UNITSYS, ev.UNITTYPESYS, 
            ev.TAXLEVYOPT, ev.IDSTCALCUL, ev.SOURCE, ev.EXTLLINK, ev.IDSTTRIGGER, ev.IDSTACTIVATION, ev.IDASTACTIVATION, ev.DTSTACTIVATION " + Cst.CrLf;
            if (pWithOnlyTblMain)
            {
                sqlSelect += @"from dbo.EVENT ev
                order by ev.IDE_EVENT, ev.IDE" + Cst.CrLf;
            }
            else
            {
                sqlSelect += @", ev.IDENTIFIER_TRADE, trs.IDT_SOURCE, trs.IDENTIFIER as IDENTIFIER_TRADESOURCE, 
                ev.IDI, ev.IDENTIFIER_INSTR, ev.DISPLAYNAME_INSTR, ev.FAMILY as FAMILY_INSTR, ev.FUNGIBILITYMODE as FUNGIBILITYMODE_INSTR,
                epad.IDPADET
                from dbo.VW_EVENT ev
                left join dbo.EVENT evs on (evs.IDE = ev.IDE_SOURCE)
                left join dbo.TRADE trs on (trs.IDT = evs.IDT)
                left join dbo.EVENT evp on (evp.IDE = ev.IDE_EVENT)
                left join dbo.EVENTGROUP eg on (eg.EVENTCODE = ev.EVENTCODE) and 
                                               ((eg.EVENTTYPE is null) or (eg.EVENTTYPE = ev.EVENTTYPE)) and ((eg.FAMILY is null) or (eg.FAMILY = ev.FAMILY))
                left join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ev.IDE)" + Cst.CrLf;

                if (_setting.restricColEventEnum.HasValue)
                {
                    sqlSelect += @"inner join dbo.EVENTENUM co on (co.CODE = 'EventCode') and (co.VALUE = ev.EVENTCODE) and ";
                    sqlSelect += "(co." + _setting.restricColEventEnum.Value.ToString() + "= 1)" + Cst.CrLf;
                    sqlSelect += @"inner join dbo.EVENTENUM ty on (ty.CODE = 'EventType') and (ty.VALUE = ev.EVENTTYPE) and ";
                    sqlSelect += "(ty." + _setting.restricColEventEnum.Value.ToString() + "= 1)" + Cst.CrLf;
                }
            }
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSelectEventClassColumn()
        {
            return GetSelectEventClassColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        public string GetSelectEventClassColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ec.IDEC, ec.IDE, ec.EVENTCLASS, ec.DTEVENT, ec.DTEVENTFORCED, ec.ISPAYMENT,
            ec.NETMETHOD,ec.IDNETCONVENTION,ec.IDNETDESIGNATION, ec.EXTLLINK
            from dbo.EVENTCLASS ec" + Cst.CrLf;

            if (false == pWithOnlyTblMain)
            {
                sqlSelect += "inner join dbo.EVENT ev on (ev.IDE = ec.IDE)" + Cst.CrLf;
                if (_setting.restricColEventEnum.HasValue)
                {
                    sqlSelect += @"inner join dbo.EVENTENUM cl on (cl.CODE = 'EventClass') and (cl.VALUE = ec.EVENTCLASS) and ";
                    sqlSelect += "(cl." + _setting.restricColEventEnum.Value.ToString() + "= 1)" + Cst.CrLf;
                }
            }
            return sqlSelect.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectEventDetailsColumn()
        {
            return GetSelectEventDetailsColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventDetailsColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ed.IDE, ed.BASIS, ed.CLOSINGPRICE, ed.CLOSINGPRICE100, ed.CONTRACTMULTIPLIER, ed.CONVERSIONRATE, ed.DAILYQUANTITY, ed.UNITDAILYQUANTITY,
            ed.DCF, ed.DCFDEN, ed.DCFNUM, ed.DTACTION, ed.DTFIXING, ed.DTSETTLTPRICE, ed.EXTLLINK, ed.FACTOR, ed.FWDPOINTS, ed.FXTYPE, ed.GAPRATE, ed.IDBC, 
            ed.IDC1, ed.IDC2, ed.IDC_BASE, ed.IDC_REF, ed.INTEREST, ed.MULTIPLIER, ed.NOTE, ed.NOTIONALAMOUNT, ed.NOTIONALREFERENCE, ed.PCTRATE, ed.PCTPAYOUT, 
            ed.PERIODPAYOUT, ed.PRICE, ed.PRICE100, ed.QUOTEDELTA, ed.QUOTETIMING, ed.QUOTEPRICE, ed.QUOTEPRICE100, ed.QUOTEPRICEYEST, ed.QUOTEPRICEYEST100, 
            ed.RATE, ed.ROWATTRIBUT, ed.SETTLTPRICE, ed.SETTLTPRICE100, ed.SETTLTQUOTESIDE, ed.SETTLTQUOTETIMING, ed.SETTLEMENTRATE, ed.SPOTRATE, ed.SPREAD, 
            ed.STRIKEPRICE, ed.TOTALOFDAY, ed.TOTALOFYEAR, ed.TOTALPAYOUTAMOUNT, ed.PIP , ed.DTDLVYSTART, ed.DTDLVYEND, ed.ASSETMEASURE
            from dbo.EVENTDET ed" + Cst.CrLf;
            if (false == pWithOnlyTblMain)
                sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ed.IDE)" + Cst.CrLf;
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectEventPricingColumn()
        {
            return GetSelectEventPricingColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventPricingColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ep.IDE, ep.DCF, ep.DCFNUM, ep.DCFDEN, ep.TOTALOFYEAR, ep.TOTALOFDAY, ep.TIMETOEXPIRATION, 
            ep.DCF2, ep.DCFNUM2, ep.DCFDEN2, ep.TOTALOFYEAR2, ep.TOTALOFDAY2, ep.TIMETOEXPIRATION2, ep.IDC1, ep.IDC2, 
            ep.STRIKE, ep.EXCHANGERATE, ep.SPOTRATE, ep.INTERESTRATE1, ep.INTERESTRATE2, ep.VOLATILITY, 
            ep.CALLPRICE, ep.CALLCHARM, ep.CALLDELTA, ep.CALLRHO1, ep.CALLRHO2, ep.CALLTHETA, 
            ep.PUTPRICE, ep.PUTCHARM, ep.PUTDELTA, ep.PUTRHO1, ep.PUTRHO2, ep.PUTTHETA, 
            ep.GAMMA, ep.VEGA, ep.COLOR, ep.SPEED, ep.VANNA, ep.VOLGA
            from dbo.EVENTPRICING ep " + Cst.CrLf;
            if (false == pWithOnlyTblMain)
                sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ep.IDE)" + Cst.CrLf;
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectEventPricing2Column()
        {
            return GetSelectEventPricing2Column(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventPricing2Column(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ep.IDE, ep.IDE_SOURCE, ep.IDEVENTPRICING2, ep.FLOWTYPE, 
            ep.DTFIXING, ep.CASHFLOW, ep.DTSTART, ep.DTEND, ep.DTPAYMENT, ep.TOTALOFDAY, 
            ep.IDC, ep.VOLATILITY, ep.DISCOUNTFACTOR, ep.RATE, ep.STRIKE, ep.BARRIER, 
            ep.FWDDELTA, ep.FWDGAMMA, ep.VEGA, ep.FXVEGA, ep.THETA, ep.BPV, ep.CONVEXITY, 
            ep.DELTA, ep.GAMMA, ep.NPV, ep.DTCLOSING, ep.METHOD, ep.IDYIELDCURVEVAL_H, 
            ep.ZEROCOUPON1, ep.ZEROCOUPON2, ep.FORWARDRATE, ep.EXTLLINK " + Cst.CrLf;
            if (false == pWithOnlyTblMain)
            {
                sqlSelect += @", yc.IDYIELDCURVEDEF, en.EXTVALUE as METHODDISPLAYNAME
                from dbo.EVENTPRICING2 ep 
                inner join dbo.EVENT ev on (ev.IDE = ep.IDE)
                inner join dbo.YIELDCURVEVAL_H yc on (yc.IDYIELDCURVEVAL_H = ep.IDYIELDCURVEVAL_H)
                left join dbo.EVENTENUM en on (en.CODE = 'EventClass') and (en.VALUE = ep.METHOD)" + Cst.CrLf;
            }
            else
            {
                sqlSelect += @"from dbo.EVENTPRICING2 ep " + Cst.CrLf;
            }
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectEventProcessColumn()
        {
            return GetSelectEventProcessColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventProcessColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select ep.IDE, ep.IDEP, ep.PROCESS, ep.IDSTPROCESS, ep.DTSTPROCESS, ep.EXTLLINK
            from dbo.EVENTPROCESS ep" + Cst.CrLf;
            if (false == pWithOnlyTblMain)
                sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ep.IDE)" + Cst.CrLf;
            return sqlSelect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectEventSiColumn()
        {
            return GetSelectEventSiColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventSiColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select esi.IDE, esi.PAYER_RECEIVER, esi.SIXML, esi.SIMODE, esi.IDSSIDB, 
            esi.IDISSI, esi.IDA_CSS, esi.IDCSSLINK, esi.IDA_STLOFFICE, esi.IDA_MSGRECEIVER, esi.SIREF,
            esi.IDAINS, esi.DTINS, esi.IDAUPD, esi.DTUPD
            from dbo.EVENTSI esi" + Cst.CrLf;
            if (false == pWithOnlyTblMain)
                sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = esi.IDE)" + Cst.CrLf;
            return sqlSelect.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pRowEventParent"></param>
        private void ImportChildRowEventInTable(DataTable pTable, DataRow pRowEventParent)
        {
            pTable.ImportRow(pRowEventParent);
            // Pas de Trade Identifier sur tous les Events, Mettre l'Identifier du Trade uniquement sur l'Event Racine
            pTable.BeginInit();
            pTable.Rows[pTable.Rows.Count - 1]["IDENTIFIER_TRADE"] = Convert.DBNull;
            pTable.EndInit();
            //            
            DataRow[] datarow = pRowEventParent.GetChildRows(DtEvent.TableName);
            if (ArrFunc.IsFilled(datarow))
            {
                for (int i = 0; i < ArrFunc.Count(datarow); i++)
                    ImportChildRowEventInTable(pTable, datarow[i]);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTradeIdentifier"></param>
        public static void Test(string pCs, string pTradeIdentifier)
        {

            int idt = TradeRDBMSTools.GetTradeIdT(pCs, pTradeIdentifier);
            if (idt > 0)
            {
                DataSetEvent ds = new DataSetEvent(pCs);
                ds.Load(null, idt, null, new DataSetEventLoadSettings(DataSetEventLoadEnum.Event));
                //
                HierarchicalEventContainer tradeEvents = ds.GetTradeEvents();
                HierarchicalEventContainer childEvents = ds.GetChildEvents(39428);
                System.Diagnostics.Debug.WriteLine((null != childEvents));
                //
                Events events = ds.GetEvents();
                EventItems eventItems = ds.GetEventItems();
                //
                int i = tradeEvents.hEvent.idReceiver;
                int j = events.@event[0].idReceiver;
                int k = eventItems.eventItem[0].idReceiver;
                System.Diagnostics.Debug.WriteLine(i);
                System.Diagnostics.Debug.WriteLine(j);
                System.Diagnostics.Debug.WriteLine(k);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetSelectEventFeeColumn()
        {
            return GetSelectEventFeeColumn(false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pWithOnlyTblMain"></param>
        /// <returns></returns>
        private static string GetSelectEventFeeColumn(bool pWithOnlyTblMain)
        {
            string sqlSelect = @"select efee.IDE, efee.STATUS, efee.FORMULA, efee.FORMULAVALUE1, efee.FORMULAVALUE2, efee.FEEPAYMENTFREQUENCY,
            efee.PAYMENTTYPE, efee.TAXTYPE, efee.TAXRATE, efee.TAXCOUNTRY" + Cst.CrLf;
            if (false == pWithOnlyTblMain)
            {
                sqlSelect += @",td.IDENTIFIER as TAXDETAIL
                from dbo.EVENTFEE efee
                inner join dbo.EVENT ev on (ev.IDE = efee.IDE)
                left outer join dbo.TAXDET td on (td.IDTAXDET = efee.IDTAXDET)" + Cst.CrLf;
            }
            else
            {
                sqlSelect += @"from dbo.EVENTFEE efee " + Cst.CrLf;
            }
            return sqlSelect;
        }

        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DataSetEventLoadEnum
    {
        /// <summary>
        /// Charge EVENT et EVENTCLASS
        /// </summary>
        Event,
        /// <summary>
        /// Charge EVENT et EVENTCLASS et   EVENTSI
        /// </summary>
        EventAndEventSi,
        /// <summary>
        /// Charge EVENT et  EVENTDETAIL et  EVENTASSET
        /// </summary>
        EventAndDetail,
        /// <summary>
        /// Tous (EVENT,EVENTCLASS,EVENTDETAIL,EVANTASSET,EVENTPRICING)
        /// </summary>
        EventComplete
    }

    /// <summary>
    /// Classe de pilotage pour le chgt des évènements
    /// </summary>
    public class DataSetEventLoadSettings
    {
        /// <summary>
        ///  Représente les évènements filtrés
        ///  <para>Les valeurs de cet enum sont nécessairement présentes dans EVENTENUM</para>
        /// </summary>
        public enum EventRestrictEnum
        {
            /// <summary>
            /// 
            /// </summary>
            ISCNFMESSAGING,
            /// <summary>
            /// 
            /// </summary>
            ISEARDAY,
            /// <summary>
            /// 
            /// </summary>
            ISACCOUNTING,
            /// <summary>
            /// 
            /// </summary>
            ISTRADEACTIONTRACKING,
        }

        #region Members
        /// <summary>
        /// Pilote le chargement de eventClass
        /// </summary>
        public bool isLoadEventClass;
        /// <summary>
        /// Pilote le chargement de EventDetail;
        /// </summary>
        public bool isLoadEventDetail;
        /// <summary>
        /// Pilote le chargement de EventAsset
        /// </summary>
        public bool isLoadEventAsset;
        /// <summary>
        /// Pilote le chargement de EventPricing
        /// </summary>
        public bool isLoadEventPricing;
        /// <summary>
        /// Pilote le chargement de EventProcess
        /// </summary>
        public bool isLoadEventProcess;
        /// <summary>
        /// Pilote le chargement de EventFee
        /// </summary>
        public bool isLoadEventFee;
        /// <summary>
        /// Pilote le chargement de EventSI
        /// </summary>
        public bool isLoadEventSI;
        /// <summary>
        /// Permet de filtrer les évènements  
        /// </summary>
        public Nullable<EventRestrictEnum> restricColEventEnum;
        /// <summary>
        /// Pilote l'alimentation de EventDet
        /// <para>Si renseigné, EventDet est alimenté pour ce code Uniquement</para>
        /// </summary>
        public string eventCodeWithDet;
        /// <summary>
        /// Pilote l'alimentation de EventDet
        /// <para>Si renseigné, EventDet est alimenté pour ce code Uniquement</para>
        /// </summary>
        public string eventTypeWithDet;

        #endregion Members

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLoadEnum"></param>
        public DataSetEventLoadSettings(DataSetEventLoadEnum pLoadEnum)
        {
            if (DataSetEventLoadEnum.Event == pLoadEnum)
                SetSettingsValue(true, false, false, false, false, false, false);
            if (DataSetEventLoadEnum.EventAndEventSi == pLoadEnum)
                SetSettingsValue(true, true, false, false, false, false, false);
            if (DataSetEventLoadEnum.EventAndDetail == pLoadEnum)
                SetSettingsValue(true, false, true, true, false, false, false);
            if (DataSetEventLoadEnum.EventComplete == pLoadEnum)
                SetSettingsValue(true, true, true, true, true, true, true);
            //
            restricColEventEnum = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsLoadEventClass"></param>
        /// <param name="pIsLoadEventSI"></param>
        /// <param name="pIsLoadEventDetail"></param>
        /// <param name="pIsLoadEventAsset"></param>
        /// <param name="pIsLoadEventPricing"></param>
        /// <param name="pIsLoadEventProcess"></param>
        /// <param name="pisLoadEventFee"></param>
        public DataSetEventLoadSettings(bool pIsLoadEventClass, bool pIsLoadEventSI,
                                        bool pIsLoadEventDetail, bool pIsLoadEventAsset,
                                        bool pIsLoadEventPricing, bool pIsLoadEventProcess, bool pisLoadEventFee)
        {
            SetSettingsValue(pIsLoadEventClass, pIsLoadEventSI, pIsLoadEventDetail, pIsLoadEventAsset, pIsLoadEventPricing, pIsLoadEventProcess, pisLoadEventFee);
            //
            restricColEventEnum = null;
        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsLoadEventClass"></param>
        /// <param name="pIsLoadEventSI"></param>
        /// <param name="pIsLoadEventDetail"></param>
        /// <param name="pIsLoadEventAsset"></param>
        /// <param name="pIsLoadEventPricing"></param>
        /// <param name="pIsLoadEventProcess"></param>
        /// <param name="pIsLoadEventFee"></param>
        private void SetSettingsValue(bool pIsLoadEventClass, bool pIsLoadEventSI,
                                        bool pIsLoadEventDetail, bool pIsLoadEventAsset,
                                        bool pIsLoadEventPricing, bool pIsLoadEventProcess, bool pIsLoadEventFee)
        {
            isLoadEventClass = pIsLoadEventClass;
            isLoadEventSI = pIsLoadEventSI;
            isLoadEventDetail = pIsLoadEventDetail;
            isLoadEventAsset = pIsLoadEventAsset;
            isLoadEventPricing = pIsLoadEventPricing;
            isLoadEventProcess = pIsLoadEventProcess;
            isLoadEventFee = pIsLoadEventFee;
        }
        #endregion
    }

}

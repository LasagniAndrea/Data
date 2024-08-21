using System;
using System.Linq;
//
using EFS.ACommon;
//
using EfsML.Enum;
//
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.Common.IO
{
    #region Classes for Nasdaq OMX Market Data
    /// <summary>
    /// Classes d'accès au données spécifique au marché Nasdaq OMX [nom de code SERIES (50000)]
    /// <para>La définition de chaque champ est disponible sur la doc http://nordic.nasdaqomxtrader.com/digitalAssets/80/80290_omnet_messref_nordic_va472.pdf</para>
    /// <para>Attention cette URL doit être ouverte depuis un web browser</para>
    /// </summary>
    public class NOMXSeries
    {
        #region Members
        #region static Members
        /// <summary>
        /// Indique si les méthodes de la classe peuvent lever des exceptions ou non
        /// </summary>
        public static bool IsWithException = false;
        #endregion static Members

        #region Nasdaq OMX Series data
        /// <summary>
        /// Country and exchange identity. Country Number is a part of the series definition
        /// <para>voir fichier omxmainfeaturesandparticularitiesefs.zip ds le ticket TRIM 31657 (book1.xlsx) pour consulter toutes les valeurs</para>
        /// <para>1(SE): Nasdaq OMX Derivatives Markets</para>
        /// <para>2(ST): Nasdaq OMX Stockholm</para>
        /// <para>3(CO): Nasdaq OMX Copenhagen</para>
        /// <para>4(HE): Nasdaq OMX Helsinki</para>
        /// <para>5(RI): Nasdaq OMX Riga</para>
        /// <para>8(FS): First North Stockholm</para>
        /// <para>9(FC): First North Denmark</para>
        /// <para>10(VI): Nasdaq OMX Vilnius </para>
        /// <para>17(TA): Nasdaq OMX Tallinn</para>
        /// <para>18(NC): NASDAQ OMX Commodities</para>
        /// <para>20(TT): TOM Trading</para>
        /// </summary>
        public string country_c;
        /// <summary>
        /// Binary representation of the market. Unique together with country_c
        /// <para>voir fichier omxmainfeaturesandparticularitiesefs.zip ds le ticket TRIM 31657 (book1.xlsx) pour consulter toutes les valeurs</para>
        /// <para>1 (SEI): SWEDISH INDEX</para>
        /// <para>2 (SES): SWEDISH STOCK</para>
        /// <para>3 (SEB): SWEDISH BOND</para>
        /// <para>4 (DB): DANISH BOND</para>
        /// <para>5 (NB): NORWEGIAN BOND</para>
        /// <para>6 (SEOI): SWEDISH TMC INDEX</para>
        /// <para>7 (SEOS): SWEDISH TMC STOCK</para>
        /// <para>9 (SEOB): SWEDISH TMC BOND</para>
        /// <para>10 (EB): EURO BOND</para>
        /// <para>11 (DBTMC): DANISH TMC BOND</para>
        /// <para>12 (EBTMC): EURO TMC BOND</para>
        /// <para>13 (NBTMC): NORWEGIAN TMC BOND</para>
        /// <para>13 (NBTMC): NORWEGIAN TMC BOND</para>
        /// <para>20 (HXTS): FINNISH STOCK</para>
        /// <para>21 (HXSOR): FINNISH STOCK ON REQUEST</para>
        /// <para>25 (COL): COLLATERAL</para>
        /// <para>44 (EUI): EURO INDEX</para>
        /// <para>55 (CBND): CASH BOND</para>
        /// <para>61 (HXI): EURO TMC INDEX</para>
        /// <para>62 (HXTS): FINISH TM STOCK</para>
        /// <para>71 (USI): USD INDEX</para>
        /// <para>72 (USS): USD STOCK</para>
        /// <para>73 (USTI): USD TM INDEX</para>
        /// <para>74 (USTS): USD TM STOCK</para>
        /// <para>81 (DAI): DANISH INDEX</para>
        /// <para>82 (DAS): DANISH STOCK</para>
        /// <para>83 (DATI): DANISH TM INDEX</para>
        /// <para>84 (DATS): DANISH TM STOCK</para>
        /// <para>101 (NOTI): NASDAQ OMX NORWEGIAN TM INDEX</para>
        /// <para>102 (NOTS): NASDAQ OMX NORWEGIAN TM STOCK</para>
        /// <para>103 (NNOI): NASDAQ OMX NORWEGIAN INDEX</para>
        /// <para>104 (NNOS): NASDAQ OMX NORWEGIAN STOCK</para>
        /// <para>....</para>
        /// </summary>
        public string market_c;
        /// <summary>
        ///  A unique binary representation of the instrument group
        /// </summary>
        public string instrument_group_c;
        /// <summary>
        ///  Expiration date modifier.this vale is set to zero when the instrument is new.
        ///  <para>The value is incremented  by one each time the instrument is involved in an issue,split, tec</para>
        ///  <para>Note that the mofifier value can be different for bid and ask options in the same series</para>
        /// </summary>
        public string modifier_c;
        /// <summary>
        /// underlying definitions are defined by each exchange. Commodity Code is part of the series definition.
        /// </summary>
        public string commodity_n;
        /// <summary>
        /// Expiration date of financial instrument.
        /// <para>A bit pattern is used. The seven most significant bits are used for year, the next four for month
        /// and the five least significant bits for day. All these bits make up an unsigned word.</para>
        /// <para>The year-field starts counting from 1990. Thus, 1990=1, 1991=2 ... 2001=12.</para>
        /// <para>
        /// Example: January 1, 1990: Binary: 0000001 0001 00001 year month day 7 bits 4 bits 5 bits
        /// Decimal: 545
        /// </para>
        /// </summary>
        public string expiration_date_n;
        /// <summary>
        /// The Strike Price is a part of the binary Series for options.
        /// <para>If theStrikePrice is equal to zero, it implies that theStrikePrice is not applicable.This is always an integer</para>
        ///<para>The implicit number of decimals is given in the decimals, strike price field.</para>
        /// </summary>
        public string strike_price_i;
        #endregion Nasdaq OMX Series data
        #endregion Members

        #region Constructors
        public NOMXSeries() { }
        #endregion Constructors

        #region Methods
        #region Tools for Nasdaq OMX Market Data
        /// <summary>
        /// Récupère la serie à partir d'une ligne   
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        public static NOMXSeries ParseNOMXSeries(string pLine)
        {
            NOMXSeries serie = default;
            try
            {
                string[] separator = new string[] { "\t" };
                string[] splittedLine = pLine.Split(separator, StringSplitOptions.None);
                //
                if (splittedLine.Count() >= 7)
                {
                    serie = new NOMXSeries
                    {
                        country_c = splittedLine[0],
                        market_c = splittedLine[1],
                        instrument_group_c = splittedLine[2],
                        modifier_c = splittedLine[3],
                        commodity_n = splittedLine[4],
                        expiration_date_n = splittedLine[5],
                        strike_price_i = splittedLine[6]
                    };
                }
            }
            catch (Exception ex)
            {
                if (IsWithException)
                {
                    throw new Exception("Retrieve series information error", ex);
                }
            }
            return serie;
        }

        /// <summary>
        /// Retourne True si la serie est gérée par Spheres® 
        /// <para>Spheres gère les futures et options(*) sur SWEDISH INDEX et SWEDISH BOND uniquement</para>
        /// <para>(*)Les options sont gérées partiellement puisque Spheres® ne récupère pas le prix des indices sous jacent</para>
        /// <para>(*)les options weekly ne sont pas gérées</para>
        /// </summary>
        /// <returns></returns>
        public Boolean IsToImport()
        {
            return IsToImport(this);
        }
        /// <summary>
        /// Retourne True si la serie est gérée par Spheres® 
        /// <para>Spheres gère les futures et options(*) sur SWEDISH INDEX et SWEDISH BOND uniquement</para>
        /// <para>(*)Les options sont gérées partiellement puisque Spheres® ne récupère pas le prix des indices sous jacent</para>
        /// <para>(*)les options weekly ne sont pas gérées</para>
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        public static Boolean IsToImport(NOMXSeries pSerie)
        {
            bool ret = false;
            // Spheres® gère uniquement les marchés SWEDISH INDEX et SWEDISH BOND
            // Sur ces marchés Spheres 
            // - intègre les prix des Futures et des Options 
            // - intègre les Vector file des Futures et des Options 
            //
            // Attention Spheres® n'intègre pas le prix l'indice sous-jacent pour l'instant
            // Cela sera un problème le jour où un client va négociées des options puisque Spheres® ne pourra pas vérifier si l'option est in/out la money
            //
            // PM 20190222 [24326] Ajout Market "3" (Bond)
            // PM 20190318 [24601] Ajout Market "2" & "82" (Stock)
            //if (pSerie.country_c == "1" && pSerie.market_c == "1")
            if ((pSerie.country_c == "1")
                && ((pSerie.market_c == "1") || (pSerie.market_c == "2") || (pSerie.market_c == "3") || (pSerie.market_c == "82")))
            {
                ret = true;
            }
            //
            if (ret)
            {
                //FI les codifications 1,2,4,5,81,82,12,13 sont spécifiques aux marchés SWEDISH
                // PM 20190318 [24601] Ajout "6","7","10","11","72","73","74","75","76","77","78","79"
                switch (pSerie.instrument_group_c)
                {
                    case "1":   // Call Option
                    case "2":   // Put Option
                    case "6":   // Call Option
                    case "7":   // Put Option
                        ret = true;
                        break;
                    case "4":   // Future
                    case "10":  // Future (Deliverable)
                    case "11":  // Future (Cash Settlment)
                        ret = true;
                        break;
                    case "5":
                        //5 => INDEX
                        ret = false;
                        break;
                    case "12":
                    case "13":
                        //12=> BINARY OVER (BINARY CALL OPTION)
                        //13=> BINARY UNDER (BINARY PUT OPTION)
                        // Spheres® ne gère pas cette typologie d'instrument
                        ret = false;
                        break;
                    case "18":
                        //18 => BOND
                        ret = false;
                        break;
                    case "72":  // Call Option Week 1
                    case "73":  // Put Option Week 1
                    case "74":  // Call Option Week 2
                    case "75":  // Put Option Week 2
                    case "76":  // Call Option Week 4
                    case "77":  // Put Option Week 4
                    case "78":  // Call Option Week 5
                    case "79":  // Put Option Week 5
                        ret = true;
                        break;
                    case "81":
                    case "82":
                        //81 => WEEKLY CALL OPTION
                        //82 => WEEKLY PUT OPTION
                        // Spheres® ne gère pas cette typologie d'instrument
                        ret = false;
                        break;
                    default:
                        ret = false;
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne "F" pour Future ou "O" pour Option ou null
        /// </summary>
        /// <returns></returns>
        public string GetCategory()
        {
            return GetCategory(this);
        }
        /// <summary>
        /// Retourne "F" pour Future ou "O" pour Option ou null
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static string GetCategory(NOMXSeries pSerie)
        {
            string ret = null;
            // PM 20190222 [24326] Ajout Market "3" (Bond)
            // PM 20190318 [24601] Ajout Market "2" & "82" (Stock)
            //if (pSerie.market_c != "1")
            if ((pSerie.country_c == "1")
                && ((pSerie.market_c == "1") || (pSerie.market_c == "2") || (pSerie.market_c == "3") || (pSerie.market_c == "82")))
            {
                //FI Les codification suivantes sont valables uniquement sur le marché SWEDISH
                //les codifications 1,2,4,5,81,82,12,13 sont spécifiques au marché SWEDISH INDEX
                //par exemple sur le marché DANISH INDEX les futures ont pour instrument_group_c le code "11"
                // PM 20190318 [24601] Ajout "6","7","10","11","72","73","74","75","76","77","78","79"
                switch (pSerie.instrument_group_c)
                {
                    case "1":   // Call Option
                    case "2":   // Put Option
                    case "6":   // Call Option
                    case "7":   // Put Option
                    case "12":  // BINARY OVER (Binary Call Option)
                    case "13":  // BINARY UNDER (Binary Put Option)
                    case "72":  // Call Option Week 1
                    case "73":  // Put Option Week 1
                    case "74":  // Call Option Week 2
                    case "75":  // Put Option Week 2
                    case "76":  // Call Option Week 4
                    case "77":  // Put Option Week 4
                    case "78":  // Call Option Week 5
                    case "79":  // Put Option Week 5
                    case "81":  // Weekly Call Option
                    case "82":  // Weekly Put Option
                        ret = "O";
                        break;
                    case "4":   // Future
                    case "10":  // Future (Deliverable)
                    case "11":  // Future (Cash Settlment)
                        ret = "F";
                        break;
                    case "5":
                        //5 => INDEX
                        ret = null;
                        break;
                    case "18":
                        //18 => BOND
                        ret = null;
                        break;
                    default:
                        ret = null;
                        break;
                }
            }
            else
            {
                if (IsWithException)
                {
                    throw new NotImplementedException(StrFunc.AppendFormat("Market {0} is not implemented", pSerie.market_c));
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le symbol du contract de l'asset
        /// </summary>
        /// <returns></returns>
        public string GetContractSymbol()
        {
            return GetContractSymbol(this);
        }
        /// <summary>
        /// Retourne le symbol du contract de l'asset
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static string GetContractSymbol(NOMXSeries pSerie)
        {
            return pSerie.commodity_n;
        }

        /// <summary>
        /// Retourne le contract attribute de l'asset
        /// </summary>
        /// <returns></returns>
        public string GetContractAttribute()
        {
            return GetContractAttribute(this);
        }
        /// <summary>
        /// Retourne le contract attribute de l'asset
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static string GetContractAttribute(NOMXSeries pSerie)
        {
            string ret = string.Empty;
            if (pSerie.modifier_c != "0")
            {
                ret = pSerie.modifier_c;
            }
            return ret;
        }

        /// <summary>
        /// Retourne la date de maturité de l'asset
        /// </summary>
        /// <returns></returns>
        public DateTime GetMaturityDate()
        {
            return GetMaturityDate(this);
        }
        /// <summary>
        /// Retourne la date de maturité de l'asset
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static DateTime GetMaturityDate(NOMXSeries pSerie)
        {
            int dateBase10 = IntFunc.IntValue(pSerie.expiration_date_n);
            DateTime date = DtFunc.StringDec16BitsToDate(dateBase10);
            return date;
        }

        /// <summary>
        /// Retourne "C" ou "P" ou null
        /// </summary>
        /// <returns></returns>
        public string GetCallPut()
        {
            return GetCallPut(this);
        }
        /// <summary>
        /// Retourne "C" ou "P" ou null
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static string GetCallPut(NOMXSeries pSerie)
        {
            string ret = string.Empty;
            // PM 20190222 [24326] Ajout Market "3" (Bond)
            // PM 20190318 [24601] Ajout Market "2" & "82" (Stock)
            //if (pSerie.market_c != "1")
            if ((pSerie.country_c == "1")
                && ((pSerie.market_c == "1") || (pSerie.market_c == "2") || (pSerie.market_c == "3") || (pSerie.market_c == "82")))
            {
                //FI Les codification suivantes sont valables uniquement sur le marché SWEDISH
                //les codifications 1,2,4,5,81,82,12,13 sont spécifiques au marché SWEDISH
                // PM 20190318 [24601] Ajout "6","7","10","11","72","73","74","75","76","77","78","79"
                switch (pSerie.instrument_group_c)
                {
                    case "1":   // Call Option
                    case "6":   // Call Option
                    case "12":  // BINARY OVER (Binary Call Option)
                    case "72":  // Call Option Week 1
                    case "74":  // Call Option Week 2
                    case "76":  // Call Option Week 4
                    case "78":  // Call Option Week 5
                    case "81":  // Weekly Call Option
                        ret = "C";
                        break;
                    case "2":   // Put Option
                    case "7":   // Put Option
                    case "13":  // BINARY UNDER (Binary Put Option)
                    case "73":  // Put Option Week 1
                    case "75":  // Put Option Week 2
                    case "77":  // Put Option Week 4
                    case "79":  // Put Option Week 5
                    case "82":  // Weekly Put Option
                        ret = "P";
                        break;
                    case "4":   // Future
                    case "10":  // Future (Deliverable)
                    case "11":  // Future (Cash Settlment)
                        ret = "";
                        break;
                    case "5":
                        //5 => INDEX
                        ret = "";
                        break;
                    case "18":
                        //18 => BOND
                        ret = "";
                        break;
                    default:
                        ret = null;
                        break;
                }
            }
            else
            {
                if (IsWithException)
                {
                    throw new NotImplementedException(StrFunc.AppendFormat("Market {0} is not implemented", pSerie.market_c));
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne "C" ou "P" ou null
        /// </summary>
        /// <returns></returns>
        public Nullable<PutOrCallEnum> GetPutOrCall()
        {
            return GetPutOrCall(this);
        }
        /// <summary>
        /// Return PutOrCallEnum.Call, PutOrCallEnum.Put ou null
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static Nullable<PutOrCallEnum> GetPutOrCall(NOMXSeries pSerie)
        {
            Nullable<PutOrCallEnum> putOrCall = default;
            string callPut = GetCallPut(pSerie);
            if (StrFunc.IsFilled(callPut))
            {
                if ("C" == callPut)
                {
                    putOrCall = PutOrCallEnum.Call;
                }
                else if ("P" == callPut)
                {
                    putOrCall = PutOrCallEnum.Put;
                }
            }
            return putOrCall;
        }

        /// <summary>
        /// Retourne le strike de l'asset
        /// </summary>
        /// <returns></returns>
        public decimal GetStrike()
        {
            return GetStrike(this);
        }
        /// <summary>
        /// Retourne le strike de l'asset
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        public static decimal GetStrike(NOMXSeries pSerie)
        {
            return DecFunc.DecValueFromInvariantCulture(pSerie.strike_price_i);
        }
        #endregion Tools for Nasdaq OMX Market Data
        #endregion Methods
    }

    public class NOMXTools
    {
        /// <summary>
        /// Settings pour la recherche d'un asset ETD depuis une série Nasdaq OMX
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        /// FI 20220321 [XXXXX] Add
        /// PM 20230622 [26091][WI390] Ajout de IsWithIsinCode
        public static MarketAssetETDRequestSettings GetAssetETDRequestSettings(NOMXSeries pSerie)
        {
            MarketAssetETDRequestSettings settings = new MarketAssetETDRequestSettings()
            {
                ContractSymbolMode = AssetETDRequestSymbolMode.ElectronicContractSymbol,
                ContractMaturityMode = AssetETDRequestMaturityMode.MaturityDate,
                IsWithContractAttrib = true,
                IsWithExerciseStyle = false,
                IsWithStrikeDecNo = true,
                IsWithContractMultiplier = false,
                IsWithIsinCode = false,
            };
            switch (pSerie.instrument_group_c)
            {
                case "10":
                case "11":
                    settings.IsWithSettlementMethod = true;
                    break;
                default:
                    settings.IsWithSettlementMethod = false;
                    break;
            }

            return settings;
        }

        /// <summary>
        /// Critères pour la recherche d'un asset ETD depuis une série Nasdaq OMX
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        /// FI 20220321 [XXXXX] Add
        public static MarketAssetETDRequest GetAssetETDRequest(NOMXSeries pSerie)
        {
            // FI 20220311 [XXXXX] use DerivativeContractTypeEnum
            MarketAssetETDRequest request = new MarketAssetETDRequest()
            {
                ElectronicContractSymbol = pSerie.GetContractSymbol(),
                ContractType = DerivativeContractTypeEnum.STD,
                ContractAttribute = pSerie.GetContractAttribute(),
                ContractCategory = pSerie.GetCategory(),
                PutCall = pSerie.GetPutOrCall(),
                StrikePrice = pSerie.GetStrike(),
                MaturityDate = pSerie.GetMaturityDate()
            };

            switch (pSerie.instrument_group_c)
            {
                case "10":
                    request.SettlementMethod = FixML.Enum.SettlMethodEnum.PhysicalSettlement;
                    break;
                case "11":
                    request.SettlementMethod = FixML.Enum.SettlMethodEnum.CashSettlement;
                    break;
            }
            return request;
        }
    }
    #endregion Classes for Nasdaq OMX Market Data

    #region Classes for Prisma
    /// <summary>
    /// 
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public class PrismaExpiryDateComponent
    {
        public string ContractYear { get; set; }
        public string ContractMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationDay { get; set; }
        public string ContractDate { get; set; }

        /// <summary>
        ///  Retourne le nom de l'échéance 
        /// </summary>
        /// <param name="isFlex"></param>
        /// <returns></returns>
        public string GetMaturityMonthYear(Boolean isFlex)
        {
            // L'échéance n'est pas forcement la date d'échéance : utilisation de Contract Year/Month au lieu de Expiration Year/Month
            string year = ContractYear;
            string month = ContractMonth;

            // Contract Year/Month n'est pas toujours renseigné (notamment pour les flex), dans ce cas utiliser Expiration Year/Month
            if (StrFunc.IsEmpty(year))
            {
                year = ExpirationYear;
            }
            if (StrFunc.IsEmpty(month))
            {
                month = ExpirationMonth;
            }

            // MaturityMonthYear
            year = DateTime.Today.Year.ToString().Substring(0, 2) + (StrFunc.IsEmpty(year) ? "00" : year.PadLeft(2, '0'));
            month = StrFunc.IsEmpty(month) ? "00" : month.PadLeft(2, '0');

            string ret = year + month;
            if (isFlex)
            {
                string day = StrFunc.IsEmpty(ExpirationDay) ? "00" : ExpirationDay.PadLeft(2, '0');
                ret += day;
            }
            return ret;

        }

        /// <summary>
        /// Retourne le nom de l'échéance YYYYMMDD
        /// </summary>
        /// <returns>YYYYMMDD</returns>
        public string GetMaturityDayMonthYear()
        {
            string ret = String.Empty;
            if (StrFunc.IsFilled(ContractDate) && (ContractDate.Length == 10))
            {
                ret = ContractDate.Replace("-", string.Empty);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la date d'échéance
        /// </summary>
        /// FI 20220321 [XXXXX] Add
        public DateTime GetExpirationDate()
        {
            string year = DateTime.Today.Year.ToString().Substring(0, 2) + (StrFunc.IsEmpty(ExpirationYear) ? (StrFunc.IsEmpty(ContractYear) ? "00" : ContractYear.PadLeft(2, '0')) : ExpirationYear.PadLeft(2, '0'));
            string month = StrFunc.IsEmpty(ExpirationMonth) ? (StrFunc.IsEmpty(ContractMonth) ? "00" : ContractMonth.PadLeft(2, '0')) : ExpirationMonth.PadLeft(2, '0');
            string day = StrFunc.IsEmpty(ExpirationDay) ? "00" : ExpirationDay.PadLeft(2, '0');

            DateTime ret = DtFunc.ParseDate(year + month + day, DtFunc.FmtDateyyyyMMdd, null);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // FI 20220311 [25699] Add
        public Nullable<DateTime> GetContractDate()
        {
            Nullable<DateTime> ret = null;

            if (StrFunc.IsFilled(ContractDate))
                ret = DtFunc.ParseDate(ContractDate, DtFunc.FmtISODate, null);

            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public class PrismaSerieMainComponent
    {

        private Nullable<ContractFrequencyEnum> _contractFrequencyEnum;
        private string _contractFrequency;

        private SettlMethodEnum _settlMethodEnum;
        private string _settlementType;

        Nullable<DerivativeExerciseStyleEnum> _exerciseStyleEnum = null;
        private string _exerciseStyle;

        private string _callPut;
        private Nullable<PutOrCallEnum> _callPutEnum;

        private decimal _strikePriceDecValue;
        private string _strikePrice;

        /// <summary>
        /// 
        /// </summary>
        public Nullable<PutOrCallEnum> CallPutEnum { get { return _callPutEnum; } }
        /// <summary>
        /// 
        /// </summary>
        public string CallPut {get { return _callPut; } set { _callPut = value; SetCallPutEnum(); } }

        /// <summary>
        /// 
        /// </summary>
        public Decimal StrikePriceDecValue { get { return _strikePriceDecValue; } }
        /// <summary>
        /// 
        /// </summary>
        public string StrikePrice { get { return _strikePrice; } set { _strikePrice = value; SetStrikeDecValue(); } }

        /// <summary>
        /// 
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SettlMethodEnum SettlementMethodEnum { get { return _settlMethodEnum; } }
        /// <summary>
        /// 
        /// </summary>
        public string SettlementType {  get { return _settlementType; } set { _settlementType = value; SetSettlementMethodEnum(); } }

        /// <summary>
        /// 
        /// </summary>
        public Nullable<DerivativeExerciseStyleEnum> ExerciseStyleEnum { get { return _exerciseStyleEnum; } }
        /// <summary>
        /// 
        /// </summary>
        public string ExerciseStyle { get { return _exerciseStyle; } set { _exerciseStyle = value; SetExerciseStyleEnum(); } }

        public Boolean IsFlex { get { return BoolFunc.IsTrue(FlexFlag); } }
        /// <summary>
        /// 
        /// </summary>
        public string FlexFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FlexProductID { get; set; }

        /// <summary>
        /// Unique numerical ID for contract identification valid across the Eurex system landscape
        /// <para>
        /// (available from release 11.1 A) (range 0 - 2147483647)
        /// </para>
        /// </summary>
        /// FI 20220311 [25699] Add (Release 11.1 A)
        public string UniqueContractID { get; set; }

        /// <summary>
        /// Defines a string identifying the contract
        /// <para>
        /// (available from release 11.1 A) (i.e. ODAX SI 20210319 CS EU C 14000.00 0)
        /// </para>
        /// </summary>
        /// FI 20220311 [25699] Add (Release 11.1 A)
        public string ContractMnemonic { get; set; }

        /// <summary>
        /// Provides information how granular the expiration of the contract can take place
        /// <para>
        ///  (available from release 11.1 A)
        /// </para>
        /// </summary>
        /// FI 20220311 [25699] Add (Release 11.1 A)
        public Nullable<ContractFrequencyEnum> ContractFrequencyEnum { get { return _contractFrequencyEnum; } }
        /// <summary>
        /// 
        /// </summary>
        public string ContractFrequency { get { return _contractFrequency; } set { _contractFrequency = value; SetContractFrequencyEnum(); } }

        /// <summary>
        /// Contract Category
        /// </summary>
        public string ContractCategory { get { return CallPutEnum.HasValue ? "O" : "F"; } }
        
        /// <summary>
        /// 
        /// </summary>
        public DerivativeContractTypeEnum DerivativeContractTypeEnum { get { return IsFlex ? DerivativeContractTypeEnum.FLEX : DerivativeContractTypeEnum.STD; } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void SetContractFrequencyEnum()
        {
            //D'après la doc Eurex Les valeurs possibles sont D(Day), W (Week), M (Month), E (EOM), F (Flex)
            _contractFrequencyEnum = null;
            if (StrFunc.IsFilled(ContractFrequency))
            {
                string[] contractFrequency = System.Enum.GetNames(typeof(ContractFrequencyEnum));
                string[] contractFrequencyFirstLetter = ArrFunc.Map<string, string>(contractFrequency, (x) => { return x.Substring(0, 1); });
                int i = ArrFunc.GetFirstItemIndex(contractFrequencyFirstLetter, ContractFrequency);
                if (i >= 0)
                    _contractFrequencyEnum = (ContractFrequencyEnum)System.Enum.Parse(typeof(ContractFrequencyEnum), contractFrequency[i]);
            }
        }

        /// <summary>
        /// Convertion de la méthode de livraison Eurex
        /// </summary>
        /// <returns></returns>
        private void SetSettlementMethodEnum()
        {
            _settlMethodEnum = SettlMethodEnum.CashSettlement;
            switch (SettlementType)
            {
                case "C": //Cash Settlement
                case "P": //Payment-Vs-Payment
                case "T": //Cascade
                case "A": //Alternate
                    _settlMethodEnum = SettlMethodEnum.CashSettlement;
                    break;
                case "D": //Derivative
                case "E": //Physical Settlement
                case "N": //Notional Settlement
                case "S": //Stock
                    _settlMethodEnum = SettlMethodEnum.PhysicalSettlement;
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SetExerciseStyleEnum()
        {
            _exerciseStyleEnum = null;

            switch (ExerciseStyle)
            {
                case "E":
                    _exerciseStyleEnum = DerivativeExerciseStyleEnum.European;
                    break;
                case "A":
                    _exerciseStyleEnum = DerivativeExerciseStyleEnum.American;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetCallPutEnum()
        {
            _callPutEnum = null;
            switch (CallPut)
            {
                case "C":
                    _callPutEnum = PutOrCallEnum.Call;
                    break;
                case "P":
                    _callPutEnum = PutOrCallEnum.Put;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetStrikeDecValue()
        {
            _strikePriceDecValue = decimal.Zero;
            if (StrFunc.IsFilled(StrikePrice))
                _strikePriceDecValue = DecFunc.DecValue(StrikePrice);
        }

    }
    /// <summary>
    /// 
    /// </summary>
    /// FI 20220321 [XXXXX] Add
    public class PrismaTools
    {
        /// <summary>
        /// Settings pour la recherche d'un asset ETD sous PRISMA
        /// </summary>
        /// <param name="pMaturityMode">Mode de recherche de l'échéance</param>
        /// <returns></returns>
        /// FI 20220321 [XXXXX] Add
        /// PM 20220701 [XXXXX] Modification de signature: ajout AssetETDRequestMaturityMode
        /// PM 20230622 [26091][WI390] Ajout de IsWithIsinCode
        public static MarketAssetETDRequestSettings GetAssetETDRequestSettings(AssetETDRequestMaturityMode pMaturityMode)
        {
            MarketAssetETDRequestSettings settings = new MarketAssetETDRequestSettings()
            {
                ContractSymbolMode = AssetETDRequestSymbolMode.ContractSymbol,
                ContractMaturityMode = pMaturityMode,
                IsWithContractAttrib = true,
                IsWithSettlementMethod = true,
                IsWithExerciseStyle = true,
                IsWithStrikeDecNo = false,
                IsWithContractMultiplier = false,
                IsWithIsinCode = false,
            };
            return settings;
        }

        /// <summary>
        /// Critères pour la recherche d'un asset ETD  une série sous PRISMA
        /// </summary>
        /// <param name="productID">Main productID</param>
        /// <param name="expiry"></param>
        /// <param name="serie"></param>
        /// <returns></returns>
        // FI 20220321 [XXXXX] Add
        // PM 20221014 [XXXXX] Rename
        public static MarketAssetETDRequest GetAssetRequestContractMonthYear(string productID, PrismaExpiryDateComponent expiry, PrismaSerieMainComponent serie)
        {
            MarketAssetETDRequest request = new MarketAssetETDRequest()
            {
                ContractSymbol = serie.IsFlex ? serie.FlexProductID : productID,
                ContractType = serie.DerivativeContractTypeEnum,
                ContractAttribute = serie.Version,
                ContractCategory = serie.ContractCategory,
                SettlementMethod = serie.SettlementMethodEnum,
                ExerciseStyle = serie.ExerciseStyleEnum,
                PutCall = serie.CallPutEnum,
                StrikePrice = serie.StrikePriceDecValue,
                MaturityMonthYear = expiry.GetMaturityMonthYear(serie.IsFlex),
                // PM 20220701 [XXXXX] Ajout MaturityDate & FrequencyMaturity
                MaturityDate = expiry.GetContractDate(),
                FrequencyMaturity = serie.ContractFrequency,
            };
            return request;
        }

        /// <summary>
        /// Critères pour la recherche d'un asset ETD  une série sous PRISMA (Nom échéance à partir de ContractDate)
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="expiry"></param>
        /// <param name="serie"></param>
        /// <returns></returns>
        // PM 20221014 [XXXXX] Add
        public static MarketAssetETDRequest GetAssetRequestContractDate(string productID, PrismaExpiryDateComponent expiry, PrismaSerieMainComponent serie)
        {
            MarketAssetETDRequest request = new MarketAssetETDRequest()
            {
                ContractSymbol = serie.IsFlex ? serie.FlexProductID : productID,
                ContractType = serie.DerivativeContractTypeEnum,
                ContractAttribute = serie.Version,
                ContractCategory = serie.ContractCategory,
                SettlementMethod = serie.SettlementMethodEnum,
                ExerciseStyle = serie.ExerciseStyleEnum,
                PutCall = serie.CallPutEnum,
                StrikePrice = serie.StrikePriceDecValue,
                MaturityMonthYear = expiry.GetMaturityDayMonthYear(),
                // PM 20220701 [XXXXX] Ajout MaturityDate & FrequencyMaturity
                MaturityDate = expiry.GetContractDate(),
                FrequencyMaturity = serie.ContractFrequency,
                // RD 20230403 [26332] Ajout FrequencyMaturityEnum
                FrequencyMaturityEnum = serie.ContractFrequencyEnum,
            };
            return request;
        }
    }
    #endregion

    #region Classes for Euronext Var
    /// <summary>
    /// Classe de base représentant les informations sur un asset pour les fichiers Euronext Var 
    /// </summary>
    // PM 20230622 [26091][WI390] Ajout
    // PM 20240122 [WI822] Add PriceCurrency, SettlementType, SettlementMethodEnum, UnderlyingCurrency, DecorrelationGroup, ProductGroup, SubPortfolio
    public abstract class EuronextVarSerieBase
    {
        #region Members
        #region Données du fichier
        protected string m_IsinCode;
        protected string m_AssetType;
        protected string m_OptionType;
        protected string m_Symbol;
        protected string m_UnderlyingISIN;
        protected DateTime m_MaturityDate;
        protected decimal m_Multiplier;
        protected decimal m_StrikePrice;
        protected string m_PriceCurrency;
        protected string m_SettlementType;
        protected string m_UnderlyingCurrency;
        #endregion Données du fichier
        private Nullable<PutOrCallEnum> m_CallPutEnum;
        private Nullable<SettlMethodEnum> m_SettlementMethodEnum;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Call or Put
        /// </summary>
        public Nullable<PutOrCallEnum> CallPutEnum { get { return m_CallPutEnum; } }

        /// <summary>
        /// Settlement Method (CashSettlement / PhysicalSettlement)
        /// </summary>
        public Nullable<SettlMethodEnum> SettlementMethodEnum { get { return m_SettlementMethodEnum; } }

        /// <summary>
        /// ContractCategory : Future "F", Option "O" (autre default)
        /// </summary>
        public String ContractCategory
        {
            get
            {
                if ((m_AssetType == "F") || (m_AssetType == "O"))
                {
                    return m_AssetType;
                }
                else
                {
                    return default;
                }
            }
        }

        /// <summary>
        /// Code Isin
        /// </summary>
        public string IsinCode
        {
            get { return m_IsinCode; }
            set { m_IsinCode = value; }
        }

        /// <summary>
        /// Asset Type : cash (‘C’), futures (‘F’) or option (‘O’)
        /// </summary>
        public string AssetType
        {
            get { return m_AssetType; }
            set { m_AssetType = value; }
        }

        /// <summary>
        /// Option type: call (‘C’) or put (‘P’) (‘N’ for cash and futures products)
        /// </summary>
        public string OptionType
        {
            get { return m_OptionType; }
            set { m_OptionType = value; SetCallOrPutEnum(); }
        }

        /// <summary>
        /// Product class code
        /// </summary>
        public string ContractSymbol
        {
            get { return m_Symbol; }
            set { m_Symbol = value; }
        }

        /// <summary>
        /// Code Isin du sous-jacent
        /// </summary>
        public string UnderlyingISINCode
        {
            get { return m_UnderlyingISIN; }
            set { m_UnderlyingISIN = value; }
        }

        /// <summary>
        /// Maturity Date
        /// </summary>
        public DateTime MaturityDate
        {
            get { return m_MaturityDate; }
            set { m_MaturityDate = value; }
        }

        /// <summary>
        /// Multiplier
        /// </summary>
        public decimal Multiplier
        {
            get { return m_Multiplier; }
            set { m_Multiplier = value; }
        }

        /// <summary>
        /// Strike Price
        /// </summary>
        public decimal StrikePrice
        {
            get { return m_StrikePrice; }
            set { m_StrikePrice = value; }
        }

        /// <summary>
        /// Price Currency
        /// </summary>
        public string PriceCurrency
        {
            get { return m_PriceCurrency; }
            set { m_PriceCurrency = value; }
        }

        /// <summary>
        /// Settlement Type
        /// </summary>
        public string SettlementType
        {
            get { return m_SettlementType; }
            set { m_SettlementType = value; m_SettlementMethodEnum = ReflectionTools.ConvertStringToEnumOrNullable<SettlMethodEnum>(m_SettlementType); }
        }

        /// <summary>
        /// Underlying Currency
        /// </summary>
        public string UnderlyingCurrency
        {
            get { return m_UnderlyingCurrency; }
            set { m_UnderlyingCurrency = value; }
        }
        #endregion Accessors

        #region Constructors
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Affectation de m_CallPutEnum
        /// </summary>
        private void SetCallOrPutEnum()
        {
            switch (m_OptionType)
            {
                case "C":
                    m_CallPutEnum = PutOrCallEnum.Call;
                    break;
                case "P":
                    m_CallPutEnum = PutOrCallEnum.Put;
                    break;
                default:
                    m_CallPutEnum = default;
                    break;
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de méthodes pour la recherche d'un asset ETD à partir des données des fichiers Euronext Var
    /// </summary>
    /// PM 20230622 [26091][WI390] Ajout
    public class EuronextVarTools
    {
        #region Members
        #endregion Members

        #region Constructors
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Settings pour la recherche d'un asset ETD pour Euronext Var
        /// </summary>
        /// <param name="pIsWithIsinCode">Indicateur d'utilisation de l'Isin Code</param>
        /// <param name="pIsWithSettlementMethod">Indicateur d'utilisation de la Settlement Method</param>
        /// <returns></returns>
        // PM 20240122 [WI822] Ajout paramètre pIsWithPriceCurrency et pIsWithSettlementMethod
        //public static MarketAssetETDRequestSettings GetAssetETDRequestSettings(bool pIsWithIsinCode)
        public static MarketAssetETDRequestSettings GetAssetETDRequestSettings(bool pIsWithIsinCode, bool pIsWithPriceCurrency = false, bool pIsWithSettlementMethod = false)
        {
            MarketAssetETDRequestSettings settings = new MarketAssetETDRequestSettings()
            {
                ContractSymbolMode = AssetETDRequestSymbolMode.ContractSymbol,
                ContractMaturityMode = AssetETDRequestMaturityMode.MaturityDate,
                IsWithContractAttrib = false,
                IsWithSettlementMethod = pIsWithSettlementMethod,
                IsWithExerciseStyle = false,
                IsWithStrikeDecNo = false,
                IsWithContractMultiplier = true,
                IsWithIsinCode = pIsWithIsinCode,
                IsWithPriceCurrency = pIsWithPriceCurrency,
            };
            return settings;
        }

        /// <summary>
        /// Critères pour la recherche d'un asset ETD depuis une série Euronext Var
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] Ajout PriceCurrency et SettlementMethod
        public static MarketAssetETDRequest GetAssetETDRequest(EuronextVarSerieBase pSerie)
        {
            MarketAssetETDRequest request = new MarketAssetETDRequest()
            {
                ContractSymbol = pSerie.ContractSymbol,
                ContractMultiplier = pSerie.Multiplier,
                ContractType = DerivativeContractTypeEnum.STD,
                ContractCategory = pSerie.ContractCategory,
                PutCall = pSerie.CallPutEnum,
                StrikePrice = pSerie.StrikePrice,
                MaturityDate = pSerie.MaturityDate,
                ISINCode = pSerie.IsinCode,
                PriceCurrency = pSerie.PriceCurrency,
                SettlementMethod = (pSerie.SettlementMethodEnum.HasValue ? pSerie.SettlementMethodEnum .Value : SettlMethodEnum.CashSettlement),
            };
            return request;
        }
        #endregion Methods
    }

    #endregion Classes for Euronext Var
}

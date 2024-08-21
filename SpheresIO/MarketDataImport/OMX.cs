using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.IO;
using System.Diagnostics;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    ///  Classe de gestion des fichiers issues du marché OMX
    /// </summary>
    /// FI 20130412 [18382]  add class
    internal class MarketDataImportOMX : MarketDataImportBase
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        public MarketDataImportOMX(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle)
        {
            string fileName = Path.GetFileNameWithoutExtension(pDataName);

            if (StrFunc.IsEmpty(fileName))
                throw new Exception("File Name is Empty");
            if (16 != fileName.Length)
                throw new Exception(StrFunc.AppendFormat("File ({0}) is not in correct format, correct format is country_market_yyyyMMdd_A.XXX", pDataName));

            m_dtFile = new DtFunc().StringyyyyMMddToDateTime(fileName.Substring(6, 8)); //Nom du fichier : "01_01_yyyyMMdd_A.FMS" ou "01_01_yyyyMMdd_A.VCT"
        }
        #endregion constructor

        #region methods
        /// <summary>
        /// Création d'un fichier pour l'importation des données series OMX 
        /// <para>Le fichier résultat contient les données strictement nécessaires, Chaque enregistrement est enrichi avec une nouvelle colonne IDASSET</para>
        /// </summary>
        /// <param name="pOutputFileName"></param>
        public void CreateSpheresSeriesFile(string pOutputFileName)
        {
            try
            {
                NOMXSeries.IsWithException = true;

                OpenOutputFileName(pOutputFileName);

                OpenInputFileName();

                int guard = 9999999;
                int currentLineNumber = 0;
                while (++currentLineNumber < guard)
                {
                    // PM 20180219 [23824] IOTools => IOCommonTools
                    //string currentLine = IOTools.StreamReader.ReadLine();
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == null)
                    {
                        Debug.WriteLine("Line number: " + currentLineNumber.ToString());
                        Debug.WriteLine("Guard: " + guard.ToString());
                        Debug.WriteLine("ENDED");
                        break;
                    }

                    NOMXSeries series = NOMXSeries.ParseNOMXSeries(currentLine);

                    bool isToImport = NOMXSeries.IsToImport(series);
                    if (isToImport)
                    {
                        string category = NOMXSeries.GetCategory(series);

                        bool isOkToCopy = false;
                        int idAsset = 0;
                        switch (category)
                        {
                            case "O":
                            case "F":
                                isOkToCopy = ExistDerivativeAsset(series, out idAsset);
                                break;
                            default:
                                isOkToCopy = false;
                                break;
                        }

                        if (isOkToCopy)
                        {
                            currentLine = idAsset.ToString() + "\t" + currentLine;
                            //Ecriture dans le fichier "light"
                            // PM 20180219 [23824] IOTools => IOCommonTools
                            //IOTools.StreamWriter.WriteLine(currentLine);
                            IOCommonTools.StreamWriter.WriteLine(currentLine);
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                CloseAllFiles();
            }
        }

        /// <summary>
        /// Récupère la serie à partir d'une ligne   
        /// </summary>
        /// <param name="pLine"></param>
        /// <returns></returns>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static OMXSeries GetOMXSeries(string pLine)
        //{
        //    try
        //    {
        //        OMXSeries serie;
        //        string[] separator = new string[] { "\t" };
        //        string[] splittedLine = pLine.Split(separator, StringSplitOptions.None);

        //        serie.country_c = splittedLine[0];
        //        serie.market_c = splittedLine[1];
        //        serie.instrument_group_c = splittedLine[2];
        //        serie.modifier_c = splittedLine[3];
        //        serie.commodity_n = splittedLine[4];
        //        serie.expiration_date_n = splittedLine[5];
        //        serie.strike_price_i = splittedLine[6];

        //        return serie;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Retrieve series information error", ex);
        //    }
        //}

        /// <summary>
        /// Retourne "F" pour Future ou "O" pour Option ou null
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static string GetCategory(OMXSeries pSerie)
        //{
        //    // PM 20190222 [24326] Ajout Market "3" (Bond)
        //    //if (pSerie.market_c != "1")
        //    if ((pSerie.market_c != "1") && (pSerie.market_c != "3"))
        //    {
        //        throw new NotImplementedException(StrFunc.AppendFormat("Market {0} is not implemented", pSerie.market_c));
        //    }

        //    //FI Les codification suivantes sont valables uniquement sur le marché SWEDISH INDEX
        //    //les codifications 1,2,4,5,81,82,12,13 sont spécifiques au marché SWEDISH INDEX
        //    //par exemple sur le marché DANISH INDEX les future ont pour instrument_group_c le code "11"
        //    string ret = null;
        //    switch (pSerie.instrument_group_c)
        //    {
        //        case "4":
        //            //4 => FUTURE
        //            ret = "F";
        //            break;
        //        case "1":
        //        case "2":
        //        case "81":
        //        case "82":
        //        case "12":
        //        case "13":
        //            //1  => CALL OPTION
        //            //2  => PUT OPTION
        //            //81 => WEEKLY CALL OPTION
        //            //82 => WEEKLY PUT OPTION
        //            //12 => BINARY OVER (BINARY CALL OPTION)
        //            //13 => BINARY UNDER (BINARY PUT OPTION)
        //            ret = "O";
        //            break;
        //        case "5":
        //            //5 => INDEX
        //            ret = null;
        //            break;
        //        default:
        //            ret = null;
        //            break;
        //        //throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSerie.instrument_group_c));
        //    }
        //    return ret;
        //}

        /// <summary>
        /// Retourne la date de maturité de l'asset
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static DateTime GetMaturityDate(NOMXSeries pSerie)
        //{
        //    int dateBase10 = IntFunc.IntValue(pSerie.expiration_date_n);
        //    DateTime date = DtFunc.StringDec16BitsToDate(dateBase10);
        //    return date;
        //}

        /// <summary>
        /// Retourne "C" ou "P" ou null
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static string GetCallPut(NOMXSeries pSerie)
        //{
        //    if (pSerie.market_c != "1")
        //        throw new NotImplementedException(StrFunc.AppendFormat("Market {0} is not implemented", pSerie.market_c));

        //    //FI Les codification suivantes sont valables uniquement sur le marché SWEDISH INDEX
        //    //les codifications 1,2,4,5,81,82,12,13 sont spécifiques au marché SWEDISH INDEX

        //    string ret = string.Empty;
        //    switch (pSerie.instrument_group_c)
        //    {
        //        case "1":
        //        case "81":
        //        case "12":
        //            //1  =>  CALL OPTION
        //            //81 => WEEKLY CALL OPTION
        //            //12 => BINARY OVER (BINARY CALL OPTION)
        //            ret = "C";
        //            break;
        //        case "2":
        //        case "82":
        //        case "13":
        //            //2 =>  PUT OPTION
        //            //82 => WEEKLY PUT OPTION
        //            //13 => BINARY UNDER (BINARY PUT OPTION)
        //            ret = "P";
        //            break;
        //        case "4":
        //            //4 => FUTURE
        //            ret = "";
        //            break;
        //        case "5":
        //            //5 => INDEX
        //            ret = "";
        //            break;
        //        default:
        //            ret = null;
        //            break;
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static decimal GetStrike(NOMXSeries pSerie)
        //{
        //    decimal ret = decimal.Zero;
        //    ret = DecFunc.DecValueFromInvariantCulture(pSerie.strike_price_i);
        //    return ret;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static string GetContractAttribute(NOMXSeries pSerie)
        //{
        //    string ret = string.Empty;
        //    if (pSerie.modifier_c != "0")
        //        ret = pSerie.modifier_c;
        //    return ret;
        //}

        /// <summary>
        ///  Retourne le code ISO d'un marché en fonction de son ExchangeAcronym
        /// </summary>
        /// <param name="pExchangeAcronym"></param>
        /// <returns></returns>
        protected override string GetISO183803FromExchangeAcronym(string pExchangeAcronym)
        {
            string ret;
            // PM 20190222 [24326] Ajout Market "3" (Bond)
            // PM 20190318 [24601] Ajout Market "2" & "82" (Stock)
            switch (pExchangeAcronym)
            {
                case "1":
                case "2":
                case "3":
                case "82":
                    ret = "XSTO";
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pExchangeAcronym));
            }

            return ret;
        }

        /// <summary>
        /// Retourne True si la serie est gérée par Spheres® 
        /// <para>Spheres gère les futures et options(*) sur SWEDISH INDEX uniquement</para>
        /// <para>(*)Les options sont gérées partiellement puisque Spheres® ne récupère pas le prix des indices sous jacent</para>
        /// <para>(*)les options weekly ne sont pas gérées</para>
        /// </summary>
        /// <param name="series"></param>
        // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
        //private static Boolean IsToImport(OMXSeries pSerie)
        //{
        //    bool ret = false;

        //    // FI Spheres® gère uniquement le marché SWEDISH INDEX
        //    // Sur ce marché Spheres 
        //    // - intègre les prix des Futures et des options 
        //    // - intègre les Vector file des Futures et des options 
        //    //
        //    // Attention Spheres® n'intègre pas le prix l'indice sous-jacent pour l'instant
        //    // Cela sera un problème le jour où un client va négociées des options puisque Spheres® ne pourra pas vérifier si l'option est in/out de money
        //    // Spheres® gère donc correctement (de A à Z) uniquement les futures sur indice OMXS30
        //    //
        //    // PM 20190222 [24326] Ajout Market "3" (Bond)
        //    //if (pSerie.country_c == "1" && pSerie.market_c == "1")
        //    if (pSerie.country_c == "1" && ((pSerie.market_c == "1") || (pSerie.market_c == "3")))
        //    {
        //        ret = true;
        //    }
        //    //
        //    if (ret)
        //    {
        //        //les codifications 1,2,4,5,81,82,12,13 sont spécifiques au marché SWEDISH INDEX
        //        switch (pSerie.instrument_group_c)
        //        {
        //            case "4":
        //                //4 => FUTURE
        //                ret = true;
        //                break;
        //            case "1":
        //            case "2":
        //                //1 => CALL OPTION
        //                //2 => PUT OPTION
        //                ret = true;
        //                break;
        //            case "5":
        //                //5 => INDEX
        //                ret = false;
        //                break;
        //            case "81":
        //            case "82":
        //                //81 => WEEKLY CALL OPTION
        //                //82 => WEEKLY PUT OPTION
        //                // Spheres® ne gère pas cette typologie d'instrument
        //                ret = false;
        //                break;
        //            case "12":
        //            case "13":
        //                //12=> BINARY OVER (BINARY CALL OPTION)
        //                //13=> BINARY UNDER (BINARY PUT OPTION)
        //                // Spheres® ne gère pas cette typologie d'instrument
        //                ret = false;
        //                break;
        //            default:
        //                ret = false;
        //                break;
        //        }
        //    }
        //    return ret;
        //}

        /// <summary>
        ///  Retourne le symbol d'un contrat Option/Future à partir de OMXSeries
        ///  <para>Sous Spheres® {pSerie}.commodity_n doit être renseigné dans DERIVATIVECONTRACT.ELECCONTRACTSYMBOL</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSerie"></param>
        /// <param name="pMarketCodeISO"></param>
        //  PM 20190222 [24326] Gestion Future sur Bond
        private static string GetContractSymbol(string pCS, NOMXSeries pSerie, string pMarketCodeISO)
        {
            string ret = string.Empty;

            string category = NOMXSeries.GetCategory(pSerie);

            StrBuilder sql = new StrBuilder();
            //  PM 20190222 [24326] Prise en compte de DERIVATIVEATTRIB.ELECCONTRACTSYMBOL
            //sql +=
            //            @"select dc.CONTRACTSYMBOL 
            //  from dbo.DERIVATIVECONTRACT dc
            // inner join dbo.MARKET m on m.IDM = dc.IDM
            // where (dc.ELECCONTRACTSYMBOL = @ELECCONTRACTSYMBOL)
            //   and (dc.CATEGORY = @CATEGORY)
            //   and (m.ISO10383_ALPHA4 = @ISO10383_ALPHA4)";
            sql += @"select dc.CONTRACTSYMBOL
  from dbo.DERIVATIVECONTRACT dc
 inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
 inner join dbo.MARKET m on (m.IDM = dc.IDM)
 where (dc.CATEGORY = @CATEGORY)
   and (m.ISO10383_ALPHA4 = @ISO10383_ALPHA4)
   and ((da.ELECCONTRACTSYMBOL = @ELECCONTRACTSYMBOL)
     or ((da.ELECCONTRACTSYMBOL is null) and (dc.ELECCONTRACTSYMBOL = @ELECCONTRACTSYMBOL)))";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISO10383_ALPHA4), pMarketCodeISO);
            dataParameters.Add(new DataParameter(pCS, "ELECCONTRACTSYMBOL", DbType.AnsiString, 64), pSerie.commodity_n);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CATEGORY), category);

            QueryParameters queryParameters = new QueryParameters(pCS, sql.ToString(), dataParameters);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
            {
                ret = Convert.ToString(obj);
            }

            return ret;
        }

        /// <summary>
        /// Retourne true si l'asset est négocié dans Spheres®
        /// 
        /// </summary>
        /// <param name="series"></param>
        /// <param name="idAsset">Retourne l'idAsset</param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private Boolean ExistDerivativeAsset(NOMXSeries series, out int idAsset)
        {
            bool isOk = false;
            idAsset = 0;

            string category = NOMXSeries.GetCategory(series);
            string iso10383_alpha4 = GetISO183803FromExchangeAcronym(series.market_c);
            string contractSymbol = GetContractSymbol(CSTools.SetCacheOn(Cs), series, iso10383_alpha4);

            if (StrFunc.IsFilled(contractSymbol))
            {
                DateTime maturityDate = NOMXSeries.GetMaturityDate(series);
                decimal strike = NOMXSeries.GetStrike(series);
                string contractAttribut = NOMXSeries.GetContractAttribute(series);

                m_IsUseContractAttrib = StrFunc.IsFilled(contractAttribut);

                //Spheres® recupère les vector files des assets Option et Future
                //Spheres® recupère les prix des options et des futures
                //Rappel 
                //Spheres® ne devrait récupérer que les données sur des assets FUTURE.
                //Pour l'instant nos clients négocient uniquement des contrat Future
                //
                //Spheres® ne récupère pas les prix des indices
                // FI 20131121 [19224] paramètre AssetFindMaturityEnum.MATURITYDATE
                SetQueryExistAssetMaturityDateInTrade(category, AssetFindMaturityEnum.MATURITYDATE);
                /* Valorisation des paramètres de la requête */
                m_QueryExistAssetMaturityDateInTrade.Parameters["DTFILE"].Value = m_dtFile;
                m_QueryExistAssetMaturityDateInTrade.Parameters["ISO10383_ALPHA4"].Value = iso10383_alpha4;
                m_QueryExistAssetMaturityDateInTrade.Parameters["CONTRACTSYMBOL"].Value = contractSymbol;
                m_QueryExistAssetMaturityDateInTrade.Parameters["CATEGORY"].Value = category;
                m_QueryExistAssetMaturityDateInTrade.Parameters["MATURITYDATE"].Value = maturityDate;
                if (category == "O")
                {
                    string callPut = GetPutCall(NOMXSeries.GetCallPut(series));
                    m_QueryExistAssetMaturityDateInTrade.Parameters["PUTCALL"].Value = callPut;
                    m_QueryExistAssetMaturityDateInTrade.Parameters["STRIKEPRICE"].Value = strike;
                }
                if (m_IsUseContractAttrib)
                {
                    m_QueryExistAssetMaturityDateInTrade.Parameters["CONTRACTATTRIBUTE"].Value = contractAttribut;
                }
                using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, m_QueryExistAssetMaturityDateInTrade.Query, m_QueryExistAssetMaturityDateInTrade.Parameters.GetArrayDbParameter()))
                {
                    isOk = dr.Read();
                    if (isOk)
                    {
                        idAsset = Convert.ToInt32(dr["IDASSET"]);
                    }
                }
            }
            return isOk;
        }
        #endregion methods
    }

    /// <summary>
    /// Structure d'accès au données spécifique au marché OMX [nom de code SERIES (50000)]
    /// <para>La définition de chaque champ est disponible sur la doc http://nordic.nasdaqomxtrader.com/digitalAssets/80/80290_omnet_messref_nordic_va472.pdf</para>
    /// <para>Attention cette URL doit être ouverte depuis un web browser</para>
    /// </summary>
    // PM 20190222 [24326] Déplacé dans EFS.Common.IO.NOMXSeries
    //internal struct OMXSeries
    //{
    //    /// <summary>
    //    /// Country and exchange identity. Country Number is a part of the series definition
    //    /// <para>voir fichier omxmainfeaturesandparticularitiesefs.zip ds le ticket TRIM 31657 (book1.xlsx) pour consulter toutes les valeurs</para>
    //    /// <para>1(SE): OMX Derivatives Markets</para>
    //    /// <para>2(ST): Nasdaq OMX Stockholm</para>
    //    /// <para>3(CO): Nasdaq OMX Copenhagen</para>
    //    /// <para>4(HE): Nasdaq OMX Helsinki</para>
    //    /// <para>5(RI): Nasdaq OMX Riga</para>
    //    /// <para>8(FS): First North Stockholm</para>
    //    /// <para>9(FC): First North Denmark</para>
    //    /// <para>10(VI): Nasdaq OMX Vilnius </para>
    //    /// <para>17(TA): Nasdaq OMX Tallinn</para>
    //    /// <para>18(NC): NASDAQ OMX Commodities</para>
    //    /// <para>20(TT): TOM Trading</para>
    //    /// </summary>
    //    public string country_c;
    //    /// <summary>
    //    /// Binary representation of the market. Unique together with country_c
    //    /// <para>voir fichier omxmainfeaturesandparticularitiesefs.zip ds le ticket TRIM 31657 (book1.xlsx) pour consulter toutes les valeurs</para>
    //    /// <para>1 (SEI): SWEDISH INDEX</para>
    //    /// <para>2 (SES): SWEDISH STOCK</para>
    //    /// <para>3 (SEB): SWEDISH BOND</para>
    //    /// <para>4 (DB): DANISH BOND</para>
    //    /// <para>5 (NB): NORWEGIAN BOND</para>
    //    /// <para>6 (SEOI): SWEDISH TMC INDEX</para>
    //    /// <para>7 (SEOS): SWEDISH TMC STOCK</para>
    //    /// <para>9 (SEOB): SWEDISH TMC BOND</para>
    //    /// <para>10 (EB): EURO BOND</para>
    //    /// <para>11 (DBTMC): DANISH TMC BOND</para>
    //    /// <para>12 (EBTMC): EURO TMC BOND</para>
    //    /// <para>13 (NBTMC): NORWEGIAN TMC BOND</para>
    //    /// <para>13 (NBTMC): NORWEGIAN TMC BOND</para>
    //    /// <para>20 (HXTS): FINNISH STOCK</para>
    //    /// <para>21 (HXSOR): FINNISH STOCK ON REQUEST</para>
    //    /// <para>25 (COL): COLLATERAL</para>
    //    /// <para>44 (EUI): EURO INDEX</para>
    //    /// <para>55 (CBND): CASH BOND</para>
    //    /// <para>61 (HXI): EURO TMC INDEX</para>
    //    /// <para>62 (HXTS): FINISH TM STOCK</para>
    //    /// <para>71 (USI): USD INDEX</para>
    //    /// <para>72 (USS): USD STOCK</para>
    //    /// <para>73 (USTI): USD TM INDEX</para>
    //    /// <para>74 (USTS): USD TM STOCK</para>
    //    /// <para>81 (DAI): DANISH INDEX</para>
    //    /// <para>82 (DAS): DANISH STOCK</para>
    //    /// <para>83 (DATI): DANISH TM INDEX</para>
    //    /// <para>84 (DATS): DANISH TM STOCK</para>
    //    /// <para>101 (NOTI): NASDAQ OMX NORWEGIAN TM INDEX</para>
    //    /// <para>102 (NOTS): NASDAQ OMX NORWEGIAN TM STOCK</para>
    //    /// <para>103 (NNOI): NASDAQ OMX NORWEGIAN INDEX</para>
    //    /// <para>104 (NNOS): NASDAQ OMX NORWEGIAN STOCK</para>
    //    /// <para>....</para>
    //    /// </summary>
    //    public string market_c;
    //    /// <summary>
    //    ///  A unique binary representation of the instrument group
    //    /// </summary>
    //    public string instrument_group_c;
    //    /// <summary>
    //    ///  Expiration date modifier.this vale is set to zero when the instrument is new.
    //    ///  <para>The value is incremented  by one each time the instrument is involved in an issue,split, tec</para>
    //    ///  <para>Note that the mofifier value can be different for bid and ask options in the same series</para>
    //    /// </summary>
    //    public string modifier_c;
    //    /// <summary>
    //    /// underlying definitions are defined by each exchange. Commodity Code is part of the series definition.
    //    /// </summary>
    //    public string commodity_n;
    //    /// <summary>
    //    /// Expiration date of financial instrument.
    //    /// <para>A bit pattern is used. The seven most significant bits are used for year, the next four for month
    //    /// and the five least significant bits for day. All these bits make up an unsigned word.</para>
    //    /// <para>The year-field starts counting from 1990. Thus, 1990=1, 1991=2 ... 2001=12.</para>
    //    /// <para>
    //    /// Example: January 1, 1990: Binary: 0000001 0001 00001 year month day 7 bits 4 bits 5 bits
    //    /// Decimal: 545
    //    /// </para>
    //    /// </summary>
    //    public string expiration_date_n;
    //    /// <summary>
    //    /// The Strike Price is a part of the binary Series for options.
    //    /// <para>If theStrikePrice is equal to zero, it implies that theStrikePrice is not applicable.This is always an integer</para>
    //    ///<para>The implicit number of decimals is given in the decimals, strike price field.</para>
    //    /// </summary>
    //    public string strike_price_i;
    //}
}

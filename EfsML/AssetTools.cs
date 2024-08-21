using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FixML.Enum;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;


namespace EfsML
{
    /// <summary>
    /// 
    /// </summary>
    /// EG 20150302 Add AddTimeRateSourceToDate
    public sealed partial class AssetTools
    {
        /// <summary>
        /// Retourne une nouvelle instance d'un classe héritée de SQL_AssetBase
        /// <para>Le type de la classe est fonction de la categorie de l'asset</para>
        /// <para>Aucun chargement n'est effectué</para>
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetCategory">catégorie de l'asset</param>
        /// <param name="pIdAsset">pIdAsset doit être supérieure à zéro</param>
        /// <returns/>
        /// <exception cref="NotImplementedException lorsque pAssetCategory n'est pas géré"></exception>
        /// <exception cref="ArgumentException lorsque pIdAsset n'est pas &gt; 0"></exception>
        /// FI 20110630 use ETD for ExchangeTradedContract, Future
        /// FI 20130312 [] add ExchangeTradedFund
        /// FI 20170130 [22767] Modify
        public static SQL_AssetBase NewSQLAsset(string pCS, Cst.UnderlyingAsset pAssetCategory, int pIdAsset)
        {
            if (pIdAsset <= 0)
                throw new ArgumentException("the given asset id is not valid", "pIdAsset");

            SQL_AssetBase ret;
            switch (pAssetCategory)
            {
                //case Cst.UnderlyingAsset.Bond:
                //    ret = new SQL_AssetBond(pCS, pIdAsset);
                //    break;
                case Cst.UnderlyingAsset.Bond:
                case Cst.UnderlyingAsset.ConvertibleBond:
                    ret = new SQL_AssetDebtSecurity(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.Deposit:
                    ret = new SQL_AssetDeposit(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.EquityAsset:
                    ret = new SQL_AssetEquity(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.ExchangeTradedContract:
                case Cst.UnderlyingAsset.Future:
                    ret = new SQL_AssetETD(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.FxRateAsset:
                    ret = new SQL_AssetFxRate(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.Index:
                    ret = new SQL_AssetIndex(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.MutualFund:
                    ret = new SQL_AssetMutualFund(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.Cash:
                    ret = new SQL_AssetCash(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.RateIndex:
                    ret = new SQL_AssetRateIndex(pCS, SQL_AssetRateIndex.IDType.IDASSET, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.ExchangeTradedFund:
                    ret = new SQL_AssetExchangeTradedFund(pCS, pIdAsset);
                    break;

                case Cst.UnderlyingAsset.Commodity:
                    // FI 20170130 [22767] Il faut utiliser SQL_AssetCommodity plutôt que SQL_AssetCommodityContract
                    // Des assets commodity ne s'appuient pas nécessaireement sur un COMMODITYCONTRACT 
                    //ret = new SQL_AssetCommodityContract(pCS, pIdAsset);
                    ret = new SQL_AssetCommodity(pCS, pIdAsset);
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pAssetCategory));

            }
            return ret;
        }
        /// <summary>
        /// Ajoute l'heure de cotation à une date
        /// </summary>
        /// EG 20150302 New
        public static DateTime AddTimeRateSourceToDate(string pCS, Cst.UnderlyingAsset pAssetCategory, int pIdAsset, DateTime pDate)
        {
            DateTime newDate = pDate;
            SQL_AssetBase sql_AssetBase = NewSQLAsset(pCS, pAssetCategory, pIdAsset);
            if (null != sql_AssetBase)
                newDate = DtFunc.AddTimeToDate(newDate.Date, sql_AssetBase.TimeRateSrc);
            return newDate;
        }

        /// <summary>
        /// Get the quote relative value to the input asset category
        /// </summary>
        /// <param name="pAssetCategory"></param>
        /// <returns>the converted category, the default category (Bond) when the input category is not recognized</returns>
        public static QuoteEnum ConvertUnderlyingAssetToQuoteEnum(Cst.UnderlyingAsset pAssetCategory)
        {
            // EG 20160404 Migration vs2013
            // #warning UNDONE....

            QuoteEnum ret = default;

            switch (pAssetCategory)
            {
                case Cst.UnderlyingAsset.Bond:
                    //PL 20111024 ret = QuoteEnum.BOND;
                    ret = QuoteEnum.DEBTSECURITY;
                    break;

                case Cst.UnderlyingAsset.RateIndex:
                    ret = QuoteEnum.RATEINDEX;
                    break;

                case Cst.UnderlyingAsset.ConvertibleBond:
                    //PL 20111024 ret = QuoteEnum.CONVERTIBLEBOND;
                    ret = QuoteEnum.DEBTSECURITY;
                    break;

                case Cst.UnderlyingAsset.Deposit:
                    ret = QuoteEnum.DEPOSIT;
                    break;

                case Cst.UnderlyingAsset.EquityAsset:
                    ret = QuoteEnum.EQUITY;
                    break;
                case Cst.UnderlyingAsset.ExchangeTradedContract:
                case Cst.UnderlyingAsset.Future:
                    ret = QuoteEnum.ETD;
                    break;

                case Cst.UnderlyingAsset.ExchangeTradedFund:
                    ret = QuoteEnum.EXCHANGETRADEDFUND;
                    break;

                case Cst.UnderlyingAsset.FxRateAsset:
                    ret = QuoteEnum.FXRATE;
                    break;

                case Cst.UnderlyingAsset.Index:
                    ret = QuoteEnum.INDEX;
                    break;

                case Cst.UnderlyingAsset.MutualFund:
                    ret = QuoteEnum.MUTUALFUND;
                    break;

                case Cst.UnderlyingAsset.SimpleCreditDefaultSwap:
                    ret = QuoteEnum.SIMPLECREDITDEFAULTSWAP;
                    break;

                case Cst.UnderlyingAsset.SimpleFra:
                    ret = QuoteEnum.SIMPLEFRA;
                    break;

                case Cst.UnderlyingAsset.SimpleIRSwap:
                    ret = QuoteEnum.SIMPLEIRS;
                    break;

                case Cst.UnderlyingAsset.Cash:
                default:
                    // UNDONE for cash which category?
                    break;

            }

            return ret;
        }

        /// <summary>
        /// Fournit la table de cotation correspondant à une catégorie d'asset coté.
        /// </summary>
        /// <param name="pQuoteAssetCategory">Catégorie d'asset coté</param>
        /// <returns>La table de cotation correspondant à la catégorie d'asset coté, ou QUOTE_OTHER_H lorsque la catégorie n'est pas gérée</returns>
        /// PM 20151124 [20124] New
        public static Cst.OTCml_TBL ConvertQuoteEnumToQuoteTbl(QuoteEnum pQuoteAssetCategory)
        {
            Cst.OTCml_TBL quoteTable = Cst.OTCml_TBL.QUOTE_OTHER_H;
            switch (pQuoteAssetCategory)
            {
                case QuoteEnum.COMMODITY:
                    quoteTable = Cst.OTCml_TBL.QUOTE_COMMODITY_H;
                    break;
                case QuoteEnum.DEBTSECURITY:
                    quoteTable = Cst.OTCml_TBL.QUOTE_DEBTSEC_H;
                    break;
                case QuoteEnum.DEPOSIT:
                    quoteTable = Cst.OTCml_TBL.QUOTE_DEPOSIT_H;
                    break;
                case QuoteEnum.EQUITY:
                    quoteTable = Cst.OTCml_TBL.QUOTE_EQUITY_H;
                    break;
                case QuoteEnum.ETD:
                    quoteTable = Cst.OTCml_TBL.QUOTE_ETD_H;
                    break;
                case QuoteEnum.EXCHANGETRADEDFUND:
                    quoteTable = Cst.OTCml_TBL.QUOTE_EXTRDFUND_H;
                    break;
                case QuoteEnum.FXRATE:
                    quoteTable = Cst.OTCml_TBL.QUOTE_FXRATE_H;
                    break;
                case QuoteEnum.INDEX:
                    quoteTable = Cst.OTCml_TBL.QUOTE_INDEX_H;
                    break;
                case QuoteEnum.MUTUALFUND:
                    quoteTable = Cst.OTCml_TBL.QUOTE_MUTUALFUND_H;
                    break;
                case QuoteEnum.RATEINDEX:
                    quoteTable = Cst.OTCml_TBL.QUOTE_RATEINDEX_H;
                    break;
                case QuoteEnum.SIMPLECREDITDEFAULTSWAP:
                    quoteTable = Cst.OTCml_TBL.QUOTE_SCDEFSWAP_H;
                    break;
                case QuoteEnum.SIMPLEFRA:
                    quoteTable = Cst.OTCml_TBL.QUOTE_SIMPLEFRA_H;
                    break;
                case QuoteEnum.SIMPLEIRS:
                    quoteTable = Cst.OTCml_TBL.QUOTE_SIMPLEIRS_H;
                    break;
            }
            return quoteTable;
        }

        /// <summary>
        /// Retourne la table SQL qui stocke le type d'asset {pAssetCategory}
        /// </summary>
        /// <param name="pAssetCategory"></param>
        /// <returns></returns>
        /// FI 20130228 New method
        /// FI 20130312 add ETF
        /// FI 20150312 [XXXXX] Modify 
        public static Cst.OTCml_TBL ConvertUnderlyingAssetToTBL(Cst.UnderlyingAsset pAssetCategory)
        {
            Cst.OTCml_TBL ret;
            switch (pAssetCategory)
            {
                case Cst.UnderlyingAsset.Bond:
                case Cst.UnderlyingAsset.ConvertibleBond:
                    ret = Cst.OTCml_TBL.VW_TRADEDEBTSEC;
                    break;

                case Cst.UnderlyingAsset.Cash:
                    ret = Cst.OTCml_TBL.ASSET_CASH;
                    break;

                case Cst.UnderlyingAsset.Commodity:
                    ret = Cst.OTCml_TBL.VW_ASSET_COMMODITY_EXPANDED;
                    break;

                case Cst.UnderlyingAsset.Deposit:
                    ret = Cst.OTCml_TBL.ASSET_DEPOSIT;
                    break;

                case Cst.UnderlyingAsset.ExchangeTradedContract:
                case Cst.UnderlyingAsset.Future:
                    ret = Cst.OTCml_TBL.ASSET_ETD;
                    break;

                case Cst.UnderlyingAsset.EquityAsset:
                    ret = Cst.OTCml_TBL.ASSET_EQUITY;
                    break;

                case Cst.UnderlyingAsset.ExchangeTradedFund:
                    ret = Cst.OTCml_TBL.ASSET_EXTRDFUND;
                    break;

                case Cst.UnderlyingAsset.Index:
                    ret = Cst.OTCml_TBL.ASSET_INDEX;
                    break;

                case Cst.UnderlyingAsset.FxRateAsset:
                    ret = Cst.OTCml_TBL.ASSET_FXRATE;
                    break;

                case Cst.UnderlyingAsset.MutualFund:
                    ret = Cst.OTCml_TBL.ASSET_MUTUALFUND;
                    break;

                case Cst.UnderlyingAsset.SimpleFra:
                    ret = Cst.OTCml_TBL.ASSET_SIMPLEFRA;
                    break;

                // FI 20150312 [XXXXX] Add
                case Cst.UnderlyingAsset.RateIndex:
                    ret = Cst.OTCml_TBL.ASSET_RATEINDEX;
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pAssetCategory));
            }

            return ret;
        }

        /// <summary>
        /// Retourne la cotation d'un asset,  
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pBaseProduct"></param>
        /// <param name="pKeyQuote">Représente la clef d'accès à la cotation</param>
        /// <param name="pQuoteEnum">Représente le type d'asset</param>
        /// <param name="idAsset">Représente l'id de l'asset</param>
        /// <param name="pOfficialSettlementBehavior">Comportement lors de la recherche d'un prix OfficialSettlement</param>
        /// <param name="pQuoteValue">Retourne la valeur de la cotation, retourne 0 si aucune cotation trouvée</param>
        /// <param name="pQuoteDatetime">Retourne la date/heure de la quotation</param>
        /// <param name="pQuoteSource">Retourne le fournissuer de la cotation, retrourne N/A si aucune cotation trouvée</param>
        /// <param name="pQuoteTiming">retrourne N/A si aucune cotation trouvée</param>
        public static SQL_Quote GetQuote(
            string pCS, IProductBase pBaseProduct, KeyQuote pKeyQuote, QuoteEnum pQuoteEnum, int idAsset,
            Nullable<SQL_Quote.OfficialSettlementBehaviorEnum> pOfficialSettlementBehavior,
            out decimal pQuoteValue, out DateTime pQuoteDatetime, out string pQuoteSource, out string pQuoteTiming)
        {

            pQuoteValue = 0;
            pQuoteDatetime = pKeyQuote.Time;
            pQuoteSource = Cst.NotAvailable;
            pQuoteTiming = Cst.NotAvailable;
            //
            SQL_Quote quote =
                new SQL_Quote(pCS, pQuoteEnum, AvailabilityEnum.Enabled,
                                pBaseProduct, pKeyQuote, idAsset);
            if (null != pOfficialSettlementBehavior)
                quote.OfficialSettlementBehavior = pOfficialSettlementBehavior;
            //
            if (quote.IsLoaded)
            {
                pQuoteValue = quote.QuoteValue;
                //FI 20130307 [] D'après les commentaires  pQuoteDatetime doit retourner la date de la cotation 
                //La date de cotation se lit sur quote.Time
                //pQuoteDatetime = quote.AdjustedTime;
                pQuoteDatetime = quote.Time;
                pQuoteSource = quote.QuoteSource;
                if (!String.IsNullOrEmpty(quote.QuoteTiming))
                    pQuoteTiming = quote.QuoteTiming;
            }
            //
            return quote;

        }

        /// <summary>
        /// Retourne un asset ETD à partir des critères Marché, Contrat, Strike... spécifiés dans un {IFixInstrument}
        /// <para>Tous les critères sont indépendants et facultatifs, cependant une restriction sur DTENABLED, DTDISABLED est appliquée</para>
        /// <para>Retourne un asset ETD uniquement si la requête exécutée renvoie un et un seul asset</para>
        /// <para>Retourne null si l'asset n'existe pas</para>
        /// <para>Retourne null si plusieurs assets existent</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdI"></param>
        /// <param name="pCategory"></param>
        /// <param name="pFixInstrument"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public static SQL_AssetETD LoadAssetETD(string pCS, IDbTransaction pDbTransaction,
            int pIdI, CfiCodeCategoryEnum pCategory, IFixInstrument pFixInstrument, DateTime pDtBusiness)
        {
            int idDerivativeAttrib = 0;
            return LoadAssetETD(pCS, pDbTransaction, pIdI, idDerivativeAttrib, pCategory, pFixInstrument, pDtBusiness, out _);
        }

        /// <summary>
        /// Retourne un asset ETD à partir des critères Marché, Contrat, Strike... spécifiés dans un {IFixInstrument}
        /// <para>Tous les critères sont indépendants et facultatifs, cependant une restriction sur DTENABLED, DTDISABLED est appliquée</para>
        /// <para>Retourne un asset ETD uniquement si la requête exécutée renvoie un et un seul asset</para>
        /// <para>Retourne null si l'asset n'existe pas</para>
        /// <para>Retourne null si plusieurs assets existent</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCategory"></param>
        /// <param name="pFixInstrument"></param>
        /// <returns>SQL_AssetETD</returns>
        /// FI 20131223 [19337] Modifications pour charger SQL_AssetETD à partir du jeux de résultat
        /// EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        /// EG 20180307 [23769] Gestion dbTransaction
        public static SQL_AssetETD LoadAssetETD(string pCS, IDbTransaction pDbTransaction,
            int pIdI, int pIDDerivativeAttrib, CfiCodeCategoryEnum pCategory, IFixInstrument pFixInstrument, DateTime pDtBusiness,
            out string opErrMsg)
        {
            //PL 20131031 Add pIDDerivativeAttrib [TRIM 19121]
            SQL_AssetETD sqlAsset = null;
            EFS_Decimal strikePrice = null;
            Nullable<PutOrCallEnum> putCall = null;

            if (pCategory == CfiCodeCategoryEnum.Option)
            {
                if (pFixInstrument.StrikePriceSpecified)
                    strikePrice = pFixInstrument.StrikePrice;

                if (pFixInstrument.PutOrCallSpecified)
                    putCall = pFixInstrument.PutOrCall;
            }
            else if (pCategory == CfiCodeCategoryEnum.Future)
            {
                strikePrice = null;
                putCall = null;
            }
            else
            {
                throw new Exception(StrFunc.AppendFormat("category {0} is not supported", pCategory));
            }

            SQL_Market sqlMarket = ExchangeTradedDerivativeTools.LoadSqlMarketFromFixInstrument(pCS, pDbTransaction, pFixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
            //PL 20140304 19672
            //SQL_DerivativeContract sqlDerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument(pCS, pDbTransaction, pFixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
            SQL_DerivativeContract sqlDerivativeContract;
            if (StrFunc.IsEmpty(pFixInstrument.SecurityId) || IntFunc.IntValue(pFixInstrument.SecurityId) <= 0)
            {
                //Recherche du DC sur la base d'un "Symbol" dans l'objet "pFixInstrument" (WARNING: en tenant compte des DTENABLED/DTDISABLED)
                sqlDerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(pCS, pDbTransaction, pFixInstrument.SecurityExchange, pFixInstrument.Symbol, SQL_Table.ScanDataDtEnabledEnum.Yes, pDtBusiness);
            }
            else
            {
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                //PL 20211022 On dispose ici d'un IDASSET que l'on utilise pour identifier un IDDC.
                //            A partir de cet IDDC et d'autres caractéristiques (MATURITY, PUTCALL, STRIKE, ...) on va charger un ASSET.
                //            Cet ASSET chargé sera potentiellement différent de l'IDASSET d'origine. C'est potentiellement étonnant !
                //            Suite à Visio/R&D, on ne sait pas où cela peut être utile dans Spheres. Peut-être C.A. ou Stratégie ?
                //
                //            Autre aspect particulier non élucidé, la recherche ici sans tenir compte des DTENABLED/DTDISABLED, à la différence du if() ci-dessus
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

                //Recherche du DC sur la base d'un Asset (IDASSET) renseigné dans l'objet "pFixInstrument" (WARNING: sans tenir compte des DTENABLED/DTDISABLED)
                sqlDerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument(pCS, pDbTransaction, pFixInstrument);
            }

            bool isExistDerivativeContract = false;
            string maturityMonthYear = pFixInstrument.MaturityMonthYear;

            DataParameters dp = new DataParameters();
            if (null != sqlMarket && sqlMarket.IsLoaded)
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), sqlMarket.Id);
            if (null != sqlDerivativeContract && sqlDerivativeContract.IsLoaded)
            {
                isExistDerivativeContract = true;
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDDC), sqlDerivativeContract.Id);
            }
            if (null != strikePrice && (strikePrice.DecValue > decimal.Zero))
                dp.Add(new DataParameter(pCS, "STRIKE", DbType.Decimal), strikePrice.DecValue);
            if (null != putCall)
                dp.Add(new DataParameter(pCS, "PUTCALL", DbType.AnsiString, 1), ReflectionTools.ConvertEnumToString<PutOrCallEnum>(putCall.Value));
            if (StrFunc.IsFilled(maturityMonthYear))
                dp.Add(new DataParameter(pCS, "MATURITYMONTHYEAR", DbType.AnsiString, 32), maturityMonthYear);
            if (pIDDerivativeAttrib > 0)
                dp.Add(new DataParameter(pCS, "IDDERIVATIVEATTRIB", DbType.Int32, 32), pIDDerivativeAttrib);
            if (pIdI > 0)
                dp.Add(new DataParameter(pCS, "IDI", DbType.Int32, 32), pIdI);

            //FI 20131223 [19337] Mise en commentaire des lignes suivantes
            //StrBuilder sqlquery = new StrBuilder(SQLCst.SELECT + "a.IDASSET") + Cst.CrLf;
            //sqlquery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_ETD + " a" + Cst.CrLf;
            //Utilisation de la vue VW_ASSET_ETD_EXPANDED pour que les colonnes du jeu de résultat soient celles attendues par la vue l'objet SQL_AssetETD
            //Alimentation des colonnes "Market_" pour être compatible avec SQL_AssetETD
            StrBuilder sqlquery = new StrBuilder(SQLCst.SELECT + "a.*") + "," + Cst.CrLf;
            sqlquery += "m.IDENTIFIER as Market_IDENTIFIER,m.ISO10383_ALPHA4 as Market_ISO10383_ALPHA4,m.IDBC as Market_IDBC,m.IDBC as Market_IDBC,";
            sqlquery += MarketTools.BuildSQLColMarket_FIXML_SecurityExchange("m") + Cst.CrLf;
            sqlquery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " a" + Cst.CrLf;
            //FI 20131223 [19337] add jointure sur Market - PL 20211025 bien qu'existant dans la vue, on maintient jointure sur MARKET pour la colonne FIXML_SecurityExchange qui est complexe.
            sqlquery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET + " m on (m.IDM=a.IDM)";

            SQLWhere sqlWhere = new SQLWhere();
            //PL 20211025 Mis en commentaire, car critère déjà présent dans la vue.
            //sqlWhere.Append("a.ISSECURITYSTATUS=1"); 
            //PL 20211025 new view with DTENABLED/DTDISABLED on dc, da et ma / Suppression des jointures sur ces 3 tables dans la query ci-dessus. 
            sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "a", pDtBusiness), true);
            sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "a", "DVATTRIB", pDtBusiness, true), true);
            sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "a", "CONTRACT", pDtBusiness, true), true);
            sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pCS, "a", "MATURITY", pDtBusiness, true), true);

            if (dp.Contains("IDM"))
                sqlWhere.Append("a.IDM=@IDM");
            if (dp.Contains("IDDC"))
                sqlWhere.Append("a.IDDC=@IDDC");
            if (dp.Contains("STRIKE"))
                sqlWhere.Append("a.STRIKEPRICE=@STRIKE");
            if (dp.Contains("PUTCALL"))
                sqlWhere.Append("a.PUTCALL=@PUTCALL");
            if (dp.Contains("MATURITYMONTHYEAR"))
                sqlWhere.Append("a.MATURITYMONTHYEAR=@MATURITYMONTHYEAR");
            if (dp.Contains("IDDERIVATIVEATTRIB"))
                sqlWhere.Append("a.IDDERIVATIVEATTRIB=@IDDERIVATIVEATTRIB");
            if (dp.Contains("IDI"))
                sqlWhere.Append("a.IDI=@IDI");

            sqlquery += sqlWhere.ToString();

            QueryParameters qryParameters = new QueryParameters(pCS, sqlquery.ToString(), dp);

            if (isExistDerivativeContract)
            {
                DataSet ds = DataHelper.ExecuteDataset(qryParameters.Cs, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                int rowCount = ArrFunc.Count(ds.Tables[0].Rows);
                switch (rowCount)
                {
                    case 0:
                        opErrMsg = String.Format(@"No asset found! [Parameters values:{0}][Parameters names:{1}]",
                                                qryParameters.Parameters.GetDataParameter(),
                                                qryParameters.Parameters.GetSqlServerParamList());
                        break;

                    case 1:
                        opErrMsg = string.Empty;
                        // FI 20131223 [19337] sqlAsset n'est plus chargée via la colonne IDASSET, mais via un DataTable initialisé précédemment.
                        sqlAsset = new SQL_AssetETD(ds.Tables[0])
                        {
                            DbTransaction = pDbTransaction
                        };
                        break;

                    default:
                        opErrMsg = String.Format(@"Multi assets found! [Count:{2}][Parameters values:{0}][Parameters names:{1}]",
                                                qryParameters.Parameters.GetDataParameter(),
                                                qryParameters.Parameters.GetSqlServerParamList(),
                                                rowCount);
                        break;
                }
            }
            else
            {
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20211022 Ajout d'un garde-fou afin de ne pas retourner un Asset sur un DC aléatoirement
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                opErrMsg = String.Format(@"No contract found! [Parameters values:{0}][Parameters names:{1}]",
                                         qryParameters.Parameters.GetDataParameter(),
                                         qryParameters.Parameters.GetSqlServerParamList());
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            }

            return sqlAsset;
        }

        /// <summary>
        /// Création d'un asset ETD.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pMarket_ISO10383_ALPHA4">Représente le code MIC (ISO10383_ALPHA4) auquel est rattaché le contrat dérivé</param>
        /// <param name="pDC_IDI">Représente l'instrument auquel est rattaché le contrat dérivé</param>
        /// <param name="pDC_IDENTIFIER">Représente l'identifier Spheres du contrat dérivé</param>
        /// <param name="pDC_CATEGORY">Représente la catégorie (Future/Option) du contrat dérivé</param>
        /// <param name="pMaturity_MATURITYMONTHYEAR">Représente l'échéance de l'asset à créer</param>
        /// <param name="pAsset_PUTCALL">Représente le type de l'asset option à créer</param>
        /// <param name="pAsset_STRIKEPRICE">Représente le strike de l'asset option à créer</param>
        /// <param name="pIdA"></param>
        /// <param name="pDtsys"></param>
        /// <param name="pDtBusiness">Date business en vigueur. Utilisée pour déterminer la date d'activation de l'asset.</param>
        /// <param name="pInfoMsg"></param>
        /// <param name="pVRMsg"></param>
        /// <param name="opIsAssetAlreadyExist">Retourne true si l'asset existe déjà</param>
        /// <returns></returns>
        /// FI 20180927 [24202] Add parameter opIsAssetAlreadyExist
        public static SQL_AssetETD CreateAssetETD(string pCS, IProductBase pProductBase,
            string pMarket_ISO10383_ALPHA4,
            string pDC_IDENTIFIER, string pDC_CATEGORY,
            string pMaturity_MATURITYMONTHYEAR, string pAsset_PUTCALL, decimal pAsset_STRIKEPRICE,
            int pIdA, DateTime pDtsys, DateTime pDtBusiness,
            out string pVRMsg, out string pInfoMsg, out Boolean opIsAssetAlreadyExist)
        {
            CfiCodeCategoryEnum categoryEnum = (pDC_CATEGORY == "O" ? CfiCodeCategoryEnum.Option : CfiCodeCategoryEnum.Future);
            SQL_Market sql_Market = new SQL_Market(CSTools.SetCacheOn(pCS), SQL_TableWithID.IDType.ISO10383_ALPHA4, pMarket_ISO10383_ALPHA4, SQL_Table.ScanDataDtEnabledEnum.Yes);

            IFixInstrument fixInstrument = pProductBase.CreateFixInstrument();

            fixInstrument.SecurityExchange = sql_Market.FIXML_SecurityExchange;
            fixInstrument.Symbol = pDC_IDENTIFIER;
            fixInstrument.MaturityMonthYear = pMaturity_MATURITYMONTHYEAR;

            fixInstrument.PutOrCallSpecified = (categoryEnum == CfiCodeCategoryEnum.Option);
            if (fixInstrument.PutOrCallSpecified)
                fixInstrument.PutOrCall = (FixML.Enum.PutOrCallEnum)ReflectionTools.EnumParse(fixInstrument.PutOrCall, pAsset_PUTCALL);

            fixInstrument.StrikePriceSpecified = (categoryEnum == CfiCodeCategoryEnum.Option);
            if (fixInstrument.StrikePriceSpecified)
                fixInstrument.StrikePrice = new EFS_Decimal(pAsset_STRIKEPRICE);

            return CreateAssetETD(pCS, null, pProductBase, fixInstrument, categoryEnum, pIdA, pDtsys, pDtBusiness,
                    out pVRMsg, out pInfoMsg, out opIsAssetAlreadyExist);
        }

        /// <summary>
        /// Création d'un asset ETD.
        /// <para>NB: Les caractéristiques de l'asset sont spécifiées dans pFixInstrument</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pFixInstrument">Représente les caractéristiques de l'asset</param>
        /// <param name="pDC_Category">Option ou Future</param>
        /// <param name="pIdA"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pDtBusiness">Date business en vigueur. Utilisée pour la date d'activation de l'asset.</param>
        /// <param name="opVRMsg">OUT: Message de warning associé aux validation Rule non respectées sur lesquelles un warning est demandé</param>
        /// <param name="opInfoMsg">OUT: Message d'information</param>
        /// <param name="opIsAssetAlreadyExist"></param>
        /// <returns>L'asset ETD créé. Null si l'asset n'a pu être créé.</returns>
        /// <exception cref="Exception"></exception>
        /// FI 20180927 [24202] Add parameter opIsAssetAlreadyExist
        public static SQL_AssetETD CreateAssetETD(string pCS, IDbTransaction pDbTransaction, IProductBase pProductBase,
                IFixInstrument pFixInstrument, CfiCodeCategoryEnum pDC_Category, int pIdA, DateTime pDtSys, DateTime pDtBusiness,
                out string opVRMsg, out string opInfoMsg, out Boolean opIsAssetAlreadyExist)
        {
            SQL_AssetETD ret;

            opIsAssetAlreadyExist = false;
            opVRMsg = string.Empty;     //Message de warning associé à la création de l'asset
            opInfoMsg = string.Empty;   //Message d'info des actions menées


            AssetETDBuilder assetETDBuilder = new AssetETDBuilder(pCS, pDbTransaction, pFixInstrument, pProductBase, pDtBusiness);
            if (assetETDBuilder.CheckCharacteristicsBeforeCreateAssetETD2(pDC_Category, out AssetETDBuilder.CheckMessageEnum VRMsgEnum))
            {
                // opMsgValidation => Message de warning associé à VRMsgEnum
                if (VRMsgEnum != AssetETDBuilder.CheckMessageEnum.None)
                {
                    //Rappel des VR non vérifiées (nécessirement de type Warning) 
                    opVRMsg = Ressource.GetString("Msg_ETD_" + VRMsgEnum.ToString());
                }

                ret = assetETDBuilder.InsertAndLoadAssetETD(pIdA, pDtSys, out string VRDateMsg, out opInfoMsg, out opIsAssetAlreadyExist);

                if (!string.IsNullOrEmpty(VRDateMsg))
                {
                    opVRMsg += Cst.CrLf + VRDateMsg;
                }
            }
            else
            {
                //Caractéristiques NON VALIDES, on force une erreur avec le message.
                throw new Exception(Ressource.GetString("Msg_ETD_" + VRMsgEnum.ToString()));
            }

            return ret;
        }

        /// <summary>
        /// Maj d'une échéance ouverte d'un asset Option ETD (Table DERIVATIVEATTRIB).
        /// <para>NB: On y référence l'asset Future ETD sous-jacent (Column IDASSET).</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdAssetOptDerivativeAttrib"></param>
        /// <param name="pIdAssetFuture"></param>
        /// <param name="pIdAUpd"></param>
        /// <param name="pDtSys"></param>
        //PL 20180924 New signature (Add pIdAUpd) - Update IDAUPD & DTUPD (L'absence de maj de ces 2 infos nous a fait défaut avec CC pour établir un diag)
        public static void SetUnderlyingAssetETD(string pCS, IDbTransaction pDbTransaction,
                                    int pIdAssetOptDerivativeAttrib, int pIdAssetFuture, int pIdAUpd, DateTime pDtSys)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), pIdAssetFuture);
            dp.Add(new DataParameter(pCS, "IDDERIVATIVEATTRIB", DbType.Int32), pIdAssetOptDerivativeAttrib);
            dp.Add(new DataParameter(pCS, "IDAUPD", DbType.Int32), pIdAUpd);
            // FI 20200820 [25468] dates systèmes en UTC
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), pDtSys);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB + Cst.CrLf;
            sql += SQLCst.SET + "IDASSET=@IDASSET, IDAUPD=@IDAUPD, DTUPD=@DTUPD" + Cst.CrLf;
            sql += SQLCst.WHERE + "(IDDERIVATIVEATTRIB=@IDDERIVATIVEATTRIB)";

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dp);

            int nbRow = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            DataHelper.queryCache.Remove(Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString(), pCS, false, false);
            DataEnabledHelper.ClearCache(pCS, Cst.OTCml_TBL.DERIVATIVEATTRIB);
            if (nbRow == 0)
                throw new Exception("Update table DERIVATIVEATTRIB error, no rows affected");
        }

        /// <summary>
        /// Retourne un asset Equity à partir des critères Marché, ...spécifiés dans un {IFixInstrument}
        /// <para>Tous les critères sont indépendants et facultatifs, aucune restriction sur DTENABLED, DTDISABLED n'est appliquée</para>
        /// <para>Retourne un asset Equity uniquement si la requête exécutée renvoie 1 et 1 seul asset</para>
        /// <para>Retourne null si l'asset n'existe pas</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdI"></param>
        /// <param name="pFixInstrument"></param>
        /// <returns>SQL_AssetETD</returns>
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        // EG 20180307 [23769] Gestion dbTransaction
        public static SQL_AssetEquity LoadAssetEquity(IDbTransaction pDbTransaction, string pCS, int pIdI, IFixInstrument pFixInstrument)
        {
            SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(pCS, pDbTransaction, pFixInstrument, SQL_Table.ScanDataDtEnabledEnum.No);
            SQL_AssetEquity ret = null;
            //
            DataParameters dp = new DataParameters();
            if (null != sqlMarket && sqlMarket.IsLoaded)
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), sqlMarket.Id);
            if (pIdI > 0)
                dp.Add(new DataParameter(pCS, "IDI", DbType.Int32, 32), pIdI);
            //
            StrBuilder sqlquery = new StrBuilder(SQLCst.SELECT + "a.IDASSET") + Cst.CrLf;
            sqlquery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_EQUITY + " a" + Cst.CrLf;
            //
            SQLWhere sqlWhere = new SQLWhere();
            if (dp.Contains("IDM"))
                sqlWhere.Append("a.IDM=@IDM");
            if (dp.Contains("IDI"))
                sqlWhere.Append("isnull(a.IDI,@IDI)=@IDI");
            //
            sqlquery += sqlWhere.ToString();
            //
            QueryParameters qryParameters = new QueryParameters(pCS, sqlquery.ToString(), dp);

            DataSet ds = DataHelper.ExecuteDataset(qryParameters.Cs, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            SQL_AssetEquity sqlAsset = null;
            if (ArrFunc.Count(ds.Tables[0].Rows) == 1)
            {
                int idAsset = Convert.ToInt32(ds.Tables[0].Rows[0]["IDASSET"]);
                sqlAsset = new SQL_AssetEquity(pCS, idAsset)
                {
                    DbTransaction = pDbTransaction
                };
                sqlAsset.LoadTable();
            }
            //
            if (null != sqlAsset)
                ret = sqlAsset;
            //
            return ret;

        }

        /// <summary>
        /// Retourne les identifiants du DerivativeContrat et FIXML_SECURITYEXCHANGE du marché vis à vis d'un {pIdDC}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pSymbol">Identifiant du DC</param>
        /// <param name="pExchange">FIXML_SECURITYEXCHANGE du marché</param>
        /// RD 20140627 [20147] Tous les flux XML véhiculent FIXML_SECURITYEXCHANGE du marché
        // EG 20180205 [23769] Add dbTransaction  
        public static void LoadDerivativeContratIdentifier(string pCS, int pIdDC, out string pSymbol, out string pExchange)
        {
            LoadDerivativeContratIdentifier(pCS, null, pIdDC, out pSymbol, out pExchange);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20190613 [24683] Use Datatable instead of DataReader
        public static void LoadDerivativeContratIdentifier(string pCS, IDbTransaction pDbTransaction, int pIdDC, out string pSymbol, out string pExchange)
        {
            pSymbol = string.Empty;
            pExchange = string.Empty;

            string query = @"select dc.IDENTIFIER as DC_IDENTIFIER, m.FIXML_SECURITYEXCHANGE
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM = dc.IDM
            where  dc.IDDC = @IDDC";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDDC), pIdDC);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, pDbTransaction, qry.Query, qry.Parameters.GetArrayDbParameter());
            if ((null != dt) && (0 < dt.Rows.Count))
            {
                pSymbol = dt.Rows[0]["DC_IDENTIFIER"].ToString();
                pExchange = dt.Rows[0]["FIXML_SECURITYEXCHANGE"].ToString();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAsset">Représente l'asset</param>
        /// <param name="instrumentDen">retourne la base d'expression des prix de négo, clôture</param>
        /// <param name="strikePrice">retourne le strike pour les assets options (zéro si l'asset n'est pas une option)</param>
        /// <param name="currency">Retourne la devise d'expression du prix</param>
        /// <param name="priceFormatStyle">retourne le format d'affichage pour les éditions (ETD)</param>
        /// <param name="priceNumericBase">retourne la base d'expression du prix dédiée au format d'affichage pour les éditions(ETD)</param>
        /// <param name="priceMultiplier">retourne le facteur de multiplication dédiée au format d'affichage pour les éditions(ETD)</param>
        /// FI 20140904 [20275] add Method
        /// FI 20150218 [20275] Add method (méthode existante déplacée ici)
        public static void GetAssetETDPriceInfo(string pCS, int pIdAsset,
            out int instrumentDen, out decimal strikePrice, out string currency,
            out Cst.PriceFormatStyle priceFormatStyle, out int priceNumericBase, out decimal priceMultiplier)
        {
            instrumentDen = 100;

            priceFormatStyle = default;
            priceNumericBase = 0;
            priceMultiplier = 0;

            strikePrice = decimal.Zero;
            currency = string.Empty;


            SQL_AssetETD assetETD = new SQL_AssetETD(pCS, pIdAsset);
            if (false == assetETD.LoadTable(new string[] { "IDDC,PRICECURRENCY,STRIKEPRICE" }))
                throw new Exception(StrFunc.AppendFormat("Asset ETD (id:{0}) doesn't exist", pIdAsset));

            int idDC = assetETD.IdDerivativeContract;

            SQL_DerivativeContract sql_DerivativeContract = new SQL_DerivativeContract(pCS, idDC);
            if (false == sql_DerivativeContract.LoadTable(new string[] { "INSTRUMENTDEN,PRICEFORMATSTYLE,PRICENUMERICBASE,PRICEMULTIPLIER" }))
                throw new Exception(StrFunc.AppendFormat("Derivative Contract (id:{0}) doesn't exist", idDC));

            instrumentDen = sql_DerivativeContract.InstrumentDen;

            priceFormatStyle = sql_DerivativeContract.PriceFormatStyle;
            priceNumericBase = sql_DerivativeContract.PriceNumericBase;
            priceMultiplier = sql_DerivativeContract.PriceMultiplier;


            currency = assetETD.DrvContract_PriceCurrency;
            strikePrice = assetETD.StrikePrice;
        }



        /// <summary>
        /// Retourne l'actif Future sous-jacent d'un contrat Option sur Future, après, le cas échéant, l'avoir créé. 
        /// <para>Retourne null s'il n'a été déterminé (possible par exemple si sur le DC Future la création automatique d'asset n'est pas activée) </para>
        /// <para>Le calcul est opéré uniquement pour les contrats Future dont le format d'échéance est YYYYMM ou YYYYMMDD. Aucun calcul  le format YYYYMMwN</para>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="productBase"></param>
        /// <param name="assetOpt">Asset Option</param>
        /// <param name="dtBusiness"></param>
        /// <param name="idA"></param>
        /// <param name="dtSys"></param>
        /// <param name="opVRMsg">Message de warning associé à la création de l'asset Future</param>
        /// <param name="opInfoMsg">Message d'info des actions menées lors de la création de l'asset Future</param>
        /// <param name="opIsFutRelativeCreated">Retourne True si l'asset Futurea été créé </param>
        /// <returns></returns>
        /// PL 20160615 New feature [22084]
        /// EG 20180307 Gestion dbTransaction [23769] 
        /// PL 20180517 New [23968]
        /// FI 20180927 [24202] Add parameter opIsAssetAlreadyExist
        public static SQL_AssetETD GetAssetFutureRelativeToMaturityOption(string cs, IDbTransaction dbTransaction,
                                                                          IProductBase productBase, SQL_AssetETD assetOpt,
                                                                          DateTime dtBusiness, int idA, DateTime dtSys,
                                                                          out string opVRMsg, out string opInfoMsg, out Boolean opIsFutRelativeCreated)
        {

            opVRMsg = string.Empty;
            opInfoMsg = string.Empty;
            opIsFutRelativeCreated = false;

            SQL_AssetETD ret = null;
            string csCacheOn = CSTools.SetCacheOn(cs);

            if (assetOpt.DrvContract_IdDerivativeContractUnl == 0)
                throw new InvalidProgramException("Future Derivative Contrat equals 0");

            SQL_DerivativeContract sqlFut_DC = new SQL_DerivativeContract(csCacheOn, assetOpt.DrvContract_IdDerivativeContractUnl)
            {
                DbTransaction = dbTransaction
            };

            SQL_Market sqlFut_Market = new SQL_Market(csCacheOn, sqlFut_DC.IdMarket)
            {
                DbTransaction = dbTransaction
            };
            
            //firstDayOfMonthOptionMaturity: => 1er jour du mois de l'échéance de l'Option
            string dtTmp_yyyyMMdd = assetOpt.Maturity_MaturityMonthYear.Substring(0, 6) + "01";
            if (false == StrFunc.IsDate(dtTmp_yyyyMMdd, DtFunc.FmtDateyyyyMMdd, null, out DateTime firstDayOfMonthOptionMaturity))
                throw new InvalidProgramException($"yyyyMMdd:{dtTmp_yyyyMMdd} is not a date");

            if (false == MaturityHelper.IsInputInFixFormat(assetOpt.Maturity_MaturityMonthYear, out Cst.MaturityMonthYearFmtEnum assetOptMaturityMonthYearFmt))
                throw new InvalidOperationException($"MaturityMonthYear format : {assetOpt.Maturity_MaturityMonthYear} is not valid");

            if (Cst.MaturityMonthYearFmtEnum.YearMonthDay == assetOptMaturityMonthYearFmt)
            {
                // FI 20230615 [26398] chargement des propriétés  ROLLCONVMMY, MONTHREF si format YearMonthDay uniquement 
                SQL_MaturityRuleRead sqlAssetOptMaturityRule = new SQL_MaturityRuleRead(csCacheOn, assetOpt.IdMaturityRule)
                {
                    DbTransaction = dbTransaction
                };
                if (false == sqlAssetOptMaturityRule.LoadTable(new string[] { "MONTHREF", "ROLLCONVMMY" }))
                    throw new InvalidOperationException($"MaturityRule (Id:{assetOpt.IdMaturityRule}) doesn't exist");

                // FI 20230615 [26398]  Appel à ApplyMonthOffsetReversesi si sa MR implique des chgt de mois lors de détermination de la date d'échéance de l'asset
                if (MaturityRuleHelper.IsMonthOffset((sqlAssetOptMaturityRule.MaturityRollConv, sqlAssetOptMaturityRule.MaturityMonthReference)))
                    firstDayOfMonthOptionMaturity = MaturityRuleHelper.ApplyMonthOffsetReverse(firstDayOfMonthOptionMaturity, (sqlAssetOptMaturityRule.MaturityRollConv, sqlAssetOptMaturityRule.MaturityMonthReference));
            }

            //Warning:
            //Dans une V1 nous avons arrêté de ne traiter que la MR principal du DC Future. En effet, il semble que les MR multiples existent uniquement sur les options (Pascal a regardé sur le CBOE) .
            //PL 20131112 [19164]
            //NB: Utilisation, faute de mieux, de YYYYMM de l'échéance de l'Option pour constituer une date 01/MM/YYYY utilisée comme référence 
            //    pour exploiter les référentiels actifs (DTENABLED/DTDISABLED)
            SQL_MaturityRuleActive sqlFut_MaturityRule = new SQL_MaturityRuleActive(csCacheOn, sqlFut_DC.IdMaturityRule, firstDayOfMonthOptionMaturity)
            {
                DbTransaction = dbTransaction
            };

            if (false == sqlFut_MaturityRule.LoadTable())
                throw new InvalidOperationException($"MaturityRule (Id:{ sqlFut_DC.IdMaturityRule}) is not enabled on {DtFunc.DateTimeToStringDateISO(firstDayOfMonthOptionMaturity)}");


            //Spheres® calcule l'échéance d'un Future uniquement si le format d'échéance du Future est
            //- YYYYMM (v10) ou
            //- YYYYMMDD dont le cycle est monthly ou supérieur (à partir de la v12)
            //NB: Il pourrait exister sur le marché un contrat Option Echéance 201102w3 qui porterait sur le contrat Future Echéance 201103w1. Ceci est aujourd'hui indéterminable...
            Boolean isAssetFutToLoad = false;
            switch (sqlFut_MaturityRule.MaturityFormatEnum)
            {
                case Cst.MaturityMonthYearFmtEnum.YearMonthOnly:
                    isAssetFutToLoad = true;
                    break;
                case Cst.MaturityMonthYearFmtEnum.YearMonthDay:
                    isAssetFutToLoad = sqlFut_MaturityRule.IsMaturityMMYRule_Monthly || sqlFut_MaturityRule.IsMaturityMMYRule_Multiple_MonthlyInclude || sqlFut_MaturityRule.IsMaturityMMYRule_Multiple_QuaterlyInclude;
                    break;
            }

            if (isAssetFutToLoad)
            {
                string monthLetterList = "FGHJKMNQUVXZ";

                //DC FUTURE
                //Par défaut: we consider a monthly frequency
                string fut_FreqMMYRule;
                if (String.IsNullOrEmpty(sqlFut_MaturityRule.MaturityMMYRule))
                    fut_FreqMMYRule = monthLetterList;
                else if (sqlFut_MaturityRule.IsMaturityMMYRule_Multiple_MonthlyInclude || sqlFut_MaturityRule.IsMaturityMMYRule_Multiple_QuaterlyInclude)
                    fut_FreqMMYRule = sqlFut_MaturityRule.MaturityMMYRule_Multiple_Monthly + sqlFut_MaturityRule.MaturityMMYRule_Multiple_Quaterly;
                else
                    fut_FreqMMYRule = sqlFut_MaturityRule.MaturityMMYRule;

                //DC Option
                //Par défaut: we consider that option contract exercises into the futures contract that is nearest
                string opt_ExerciseRule = String.IsNullOrEmpty(assetOpt.DrvContract_ExerciseRule) ? monthLetterList : assetOpt.DrvContract_ExerciseRule;

                int addMonths = 0;
                int maxGuard = 24;
                //On démarre le processus d'identification à partir du mois d'échéance de l'Option
                int num_MM = Convert.ToInt32(firstDayOfMonthOptionMaturity.Month);
                string letter_MM = StrFunc.GetMaturityLetter(num_MM.ToString());

                Boolean isFutMaturityMonthYearFound = false;
                while ((false == isFutMaturityMonthYearFound) && (addMonths < maxGuard))
                {
                    #region STEP 1 - Identification de l'échéance du contrat Future sous-jacent
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    // Contrôle: on avance de mois en mois jusqu'à trouver un mois d'échéance autorisé. Un mois d'échéance est autorisé:
                    //           - si ce mois est un mois d'échéance autorisé par l'Option pour livraison du Future sous-jacent (opt_ExerciseRule)
                    //           et 
                    //           - si ce mois est également un mois d'échéance autorisé sur le Future sous-jacent (fut_FreqMMYRule) 
                    //           et (new feature, see STEP 3 below)
                    //           - la "date d'échéance" de l'option doit être inférieure ou égale à la "date d'échéance" du Future sous-jacent à livrer
                    // Exemple 1: 
                    //           - une Option, mensuelle (FGHJKMNQUVXZ), livre quel que soit son échéance l'échéance Novembre (X) du Future sous-jacent   
                    //           - le Future sous-jacent autorise les échéances HMUXZ
                    //           - une Option Mars (H) livrera l'échéance Novembre (X) du Future sous-jacent
                    // Exemple 2: 
                    //           - une Option, mensuelle (FGHJKMNQUVXZ), livre, quel que soit son échéance, l'échéance la plus proche du Future sous-jacent   
                    //           - le Future sous-jacent autorise les échéances HMUZ
                    //           - une Option Mars (H) dont la date est le 31/03/2018 livrera l'échéance Juin (M) du Future sous-jacent, 
                    //             car la date est le 20/03/2018 pour l'échéance Mars (H) de ce Future sous-jacent 
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    while (
                            ((opt_ExerciseRule.IndexOf(letter_MM) < 0) || (fut_FreqMMYRule.IndexOf(letter_MM) < 0))
                            &&
                            (++addMonths < maxGuard)
                          )
                    {
                        if (num_MM < 12)
                            num_MM++;
                        else
                            num_MM = 1;
                        letter_MM = StrFunc.GetMaturityLetter(num_MM.ToString());
                    }

                    #region Error
                    if (addMonths >= maxGuard)
                    {
                        throw new Exception(StrFunc.AppendFormat($"Infinite Loop: Option Maturity[{assetOpt.Maturity_MaturityMonthYear}], Option ExerciseRule[{assetOpt.DrvContract_ExerciseRule}], Future DC[{sqlFut_DC.Identifier}, Future MMYRule[{fut_FreqMMYRule}]]"));
                    }
                    #endregion

                    #endregion STEP 1

                    #region STEP 2 - Identification de l'actif relatif à l'échéance du contrat Future sous-jacent (s'il n'existe pas on le crée)

                    IFixInstrument futFixInstrument = productBase.CreateFixInstrument();
                    futFixInstrument.SecurityExchange = sqlFut_Market.FIXML_SecurityExchange;
                    futFixInstrument.Symbol = sqlFut_DC.Identifier;
                    futFixInstrument.PutOrCallSpecified = false;
                    futFixInstrument.StrikePriceSpecified = false;

                    DateTime dtMonthRef = firstDayOfMonthOptionMaturity.AddMonths(addMonths);
                    var firstDayOfMonthRef = new DateTime(dtMonthRef.Year, dtMonthRef.Month, dtMonthRef.Day);
                    var lastDayOfMonthRef = firstDayOfMonthRef.AddMonths(1).AddDays(-1);

                    AssetETDBuilder assetETDBuilder;

                    switch (sqlFut_MaturityRule.MaturityFormatEnum)
                    {
                        case Cst.MaturityMonthYearFmtEnum.YearMonthOnly:
                            futFixInstrument.MaturityMonthYear = DtFunc.DateTimeToStringyyyyMMdd(firstDayOfMonthRef).Substring(0, 6);
                            isFutMaturityMonthYearFound = true;
                            if (isFutMaturityMonthYearFound)
                            {
                                ret = AssetTools.LoadAssetETD(csCacheOn, dbTransaction, sqlFut_DC.IdI, CfiCodeCategoryEnum.Future, futFixInstrument, dtBusiness);
                                if ((ret == null) && (sqlFut_DC.IsAutoCreateAsset))
                                {
                                    //Création de l'asset Future: s'il n'existe pas et si la création automatique est autorisée
                                    ret = AssetTools.CreateAssetETD(csCacheOn, dbTransaction, productBase, futFixInstrument, CfiCodeCategoryEnum.Future, idA, dtSys, dtBusiness,
                                                                             out opVRMsg, out opInfoMsg, out Boolean isFutAssetAlreadyExist);
                                    opIsFutRelativeCreated = (false == isFutAssetAlreadyExist);
                                }
                            }
                            break;
                        case Cst.MaturityMonthYearFmtEnum.YearMonthDay:
                            //NB : dans un soucis d’optimisation, les échéances étant plus fréquemment sur la seconde partie du mois on opère de la fin du mois au 1er jour du mois
                            //NB2: dans le cas d’un mois identique à celui de la date d’échéance du DC Option, on balayera uniquement les dates supérieures ou égales à la date d’échéance de l’option puisque la date d’échéance du Future SSJ doit obligatoirement être supérieure ou égale à la date d’échéance du DC Option.
                            int dayStart = 1;
                            if (DtFunc.IsDateTimeFilled(assetOpt.Maturity_MaturityDateSys) && assetOpt.Maturity_MaturityDateSys.Month == dtMonthRef.Month)
                                dayStart = assetOpt.Maturity_MaturityDateSys.Day;
                            int dayEnd = lastDayOfMonthRef.Day;

                            for (int d = dayEnd; d >= dayStart; d--)
                            {
                                futFixInstrument.MaturityMonthYear = $"{dtMonthRef.Year}{dtMonthRef.Month.ToString().PadLeft(2, '0')}{d.ToString().PadLeft(2, '0')}";
                                // FI 20220822 [XXXXX] Optimisation => les jours fériés sont exclus
                                // FI 20221118 [26178] Prise en compte uniquement des jours fériés de type ScheduledTradingDay  
                                if (false == Tools.IsHoliday(CSTools.SetCacheOn(cs), new DtFunc().StringyyyyMMddToDateTime(futFixInstrument.MaturityMonthYear), new string[] { sqlFut_Market.IdBC }, DayTypeEnum.ScheduledTradingDay))
                                {
                                    assetETDBuilder = new AssetETDBuilder(cs, dbTransaction, futFixInstrument, productBase, dtBusiness);
                                    //Warning:
                                    //Dans une V1 nous avons arrêté de ne traiter que la MR principal du DC Future. En effet, il semble que les MR multiples existent uniquement sur les options (Pascal a regardé sur le CBOE) .
                                    isFutMaturityMonthYearFound = assetETDBuilder.IsMaturityMonthYearCompliantWithMR(sqlFut_DC.IdMaturityRule, dtMonthRef, Cst.CheckModeEnum.Error, out _);
                                    if (isFutMaturityMonthYearFound)
                                    {
                                        ret = AssetTools.LoadAssetETD(csCacheOn, dbTransaction, sqlFut_DC.IdI, CfiCodeCategoryEnum.Future, futFixInstrument, dtBusiness);
                                        if ((ret == null) && (sqlFut_DC.IsAutoCreateAsset))
                                        {
                                            //Création de l'asset Future: s'il n'existe pas et si la création automatique est autorisée
                                            ret = AssetTools.CreateAssetETD(csCacheOn, dbTransaction, productBase, futFixInstrument, CfiCodeCategoryEnum.Future, idA, dtSys, dtBusiness,
                                                                            out opVRMsg, out opInfoMsg, out Boolean isFutAssetAlreadyExist);
                                            opIsFutRelativeCreated = (false == isFutAssetAlreadyExist);

                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        default:
                            throw new NotSupportedException($"Format: {sqlFut_MaturityRule.MaturityFormatEnum} is not supported");
                    }
                    #endregion STEP 2

                    #region STEP 3 - Contrôle de cohérence entre les "dates" d'échéance de l'Option et du Future sous-jacent (New feature for [23968])
                    Boolean nextMonth = false;
                    if (isFutMaturityMonthYearFound && (ret != null))
                    {
                        //NB: si les MaturityRules ne me permettent pas de calculer un de ces 2 dates, on considèrera le mois d'échéance identifié comme autorisé (compatibilité ascendante).
                        if ((DtFunc.IsDateTimeFilled(assetOpt.Maturity_MaturityDate)
                            && DtFunc.IsDateTimeFilled(ret.Maturity_MaturityDate)))
                        {
                            if (opt_ExerciseRule.EndsWith("+"))
                            {
                                //La "date d'échéance" de l'option est supérieure ou égale à la "date d'échéance" du Future sous-jacent !
                                //On poursuit dans la boucle principale avec le prochain mois...
                                nextMonth = (DateTime.Compare(assetOpt.Maturity_MaturityDate, ret.Maturity_MaturityDate) >= 0);
                            }
                            else
                            {
                                //La "date d'échéance" de l'option est supérieure à la "date d'échéance" du Future sous-jacent !
                                //On poursuit dans la boucle principale avec le prochain mois...
                                nextMonth = (DateTime.Compare(assetOpt.Maturity_MaturityDate, ret.Maturity_MaturityDate) > 0);
                            }
                        }
                    }
                    else if (false == isFutMaturityMonthYearFound)
                    {
                        // Aucune échéance Compliant avec MR => Passage au mois suivant
                        nextMonth = true;
                    }

                    if (nextMonth)
                    {
                        ret = null;
                        isFutMaturityMonthYearFound = false;
                        opIsFutRelativeCreated = false;

                        if (num_MM < 12)
                            num_MM++;
                        else
                            num_MM = 1;
                        letter_MM = StrFunc.GetMaturityLetter(num_MM.ToString());
                        addMonths++;
                    }
                    #endregion STEP 3
                }
            }


            return ret;
        }


    }
    

    /// <summary>
    /// Classe qui permet de générer un nouvel asset ETD
    /// </summary>
    public class AssetETDBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        // PM 20160307 Ajout MaturityRuleNotFound
        public enum CheckMessageEnum
        {
            CharacteristicMissing,
            //

            ContractNotFound,
            /// <summary>
            /// La règle d'échéance n'est pas renseignée sur le contrat[Identifiant:{0}]
            /// </summary>
            ContractMaturityRuleNotFound,
            /// <summary>
            /// Le contrat n'autorise pas la création automatique d'actif
            /// </summary>
            ContractNotAutoCreateAsset,

            StrikeIncrementIncorrect,
            StrikePxRangeIncorrect,

            MaturityRuleNotFound,
            /// <summary>
            /// Format de l'échéance incorrect
            /// </summary>
            MaturityFormatIncorrect,
            /// <summary>
            /// Echéance incompatible avec le cycle en vigueur
            /// </summary>
            MaturityPeriodIncorrect,
            /// <summary>
            /// Cycle non vérifié
            /// </summary>
            MaturityPeriodNoControl,
            /// <summary>
            /// L'échéance (obligatoirement au format YYYYMMDD) est un jour férié
            /// </summary>
            MaturityIsHoliday,
            /// <summary>
            /// 
            /// </summary>
            NewAsset,
            None,
        }

        /// <summary>
        /// 
        /// </summary>
        private enum AddAssetMessageEnum
        {
            MaturityDateNotSet,
            MaturityLastTrdDayNotSet,
            MaturityDelivryDateNotSet,
        }


        #region Member
        private readonly string CS;
        private readonly IDbTransaction DbTransaction;
        /// <summary>
        /// Représente les Maturity Rules associées au contrat dérivé par ordre de priorité et enabled en date business  <seealso cref="_dtBusiness"/> 
        /// </summary>
        private readonly DataDCMR[] _DCMR;

        /// <summary>
        /// Représente le contrat dérivé
        /// </summary>
        private readonly SQL_DerivativeContract _sqlDerivativeContract;

        /// <summary>
        /// Représente le marché du contrat dérivé
        /// </summary>
        private readonly DataMarket _dataMarket;


        /// <summary>
        /// Représente la date business, date qui sera utilisé pour calculer le DTENABLED
        /// </summary>
        private readonly DateTime _dtBusiness;
        /// <summary>
        /// Représente le produit de base
        /// </summary>
        private readonly IProductBase _productBase;
        /// <summary>
        /// Représente les caractéristiques de l'asset
        /// </summary>
        private readonly IFixInstrument _fixInstrument;
        /// <summary>
        /// Indicateur de validité des caractéristiques de l'asset
        /// </summary>
        private bool _isCharacteristicsValid;


        /// <summary>
        /// Indicateur d'échéance trimestrielle
        /// </summary>
        private bool _isQuaterlyMaturity;



        /// <summary>
        /// MaturityRule en adéquation avec l'échéance (IFixInstrument.MaturityMonthYear)
        /// <para>Représente la règle de maturité Main si le DC n'est associé qu'à 1 seul règle de maturité</para>
        /// </summary>
        private SQL_MaturityRuleBase _sqlMaturityRuleCompliant;
        #endregion Members

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pfixInstrument"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pDtBusiness"></param>
        public AssetETDBuilder(string pCS, IDbTransaction pDbTransaction, IFixInstrument pfixInstrument, IProductBase pProductBase, DateTime pDtBusiness)
        {
            CS = pCS;
            DbTransaction = pDbTransaction;
            _fixInstrument = pfixInstrument;

            _productBase = pProductBase;
            _dtBusiness = pDtBusiness;

            _isCharacteristicsValid = false;

            _isQuaterlyMaturity = false;

            _DCMR = new DataDCMR[0];
            _dataMarket = new DataMarket();

            _sqlDerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(CS, DbTransaction, _fixInstrument.SecurityExchange, _fixInstrument.Symbol, _dtBusiness);
            if (null != _sqlDerivativeContract)
            {
                _DCMR = new DataDCMREnabled(CS, DbTransaction, _dtBusiness).GetDCMR(_sqlDerivativeContract.Id).ToArray();
                _dataMarket = DataMarketEnabledHelper.GetDataMarket(CS, DbTransaction, _dtBusiness, _sqlDerivativeContract.IdMarket);
            }
        }
        #endregion

        #region Method

        /// <summary>
        /// Insertion d'un asset ETD dans la table ASSET_ETD
        /// <para>NB: DtEnabled = DtBusiness - 1 Week</para>
        /// </summary>
        /// <param name="pIDDerivativeAttrib"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pDtIns"></param>
        /// <param name="pDtEnabled">date d'activation associée à l'asset créé</param>
        /// <param name="pContratMultipler">null accepté</param>
        /// <param name="isAssetAlreadyExist"></param>
        /// FI 20170320 [22985] chgt de signature add paramètre pContractMultiplier 
        /// EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        /// EG 20180307 [23769] Gestion dbTransaction
        /// FI 20180316 [23769] Modify
        /// PL 20211026 Déplacé de AssetTools (public static) dans AssetETDBuilder (private)
        private void InsertAssetETD2(int pIDDerivativeAttrib, int pIdAIns, DateTime pDtIns, DateTime pDtEnabled, Nullable<Decimal> pContratMultipler, out Boolean isAssetAlreadyExist)
        {

            isAssetAlreadyExist = false;

            Nullable<decimal> strikePrice = null;
            Nullable<PutOrCallEnum> putCall = null;


            #region OPTION
            if (_sqlDerivativeContract.CfiCodeCategory == CfiCodeCategoryEnum.Option)
            {
                if ((!_fixInstrument.PutOrCallSpecified)
                    || ((!_fixInstrument.StrikePriceSpecified) || (_fixInstrument.StrikePrice == null)))
                {
                    throw new InvalidOperationException(String.Format("Error: InsertAssetETD() - Option missing characteristics! [PutCall:{0} - Strike:{1}]",
                        (_fixInstrument.PutOrCallSpecified ? _fixInstrument.PutOrCall.ToString() : "N/A"),
                        (_fixInstrument.StrikePriceSpecified ? _fixInstrument.StrikePrice.ToString() : "N/A")));
                }

                strikePrice = _fixInstrument.StrikePrice.DecValue;
                putCall = _fixInstrument.PutOrCall;
            }
            #endregion OPTION

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), Cst.AUTOMATIC_COMPUTE);
            dp.Add(new DataParameter(CS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), Cst.AUTOMATIC_COMPUTE);
            dp.Add(new DataParameter(CS, "AII", DbType.AnsiString, 47), Cst.AUTOMATIC_COMPUTE);
            dp.Add(new DataParameter(CS, "IDDERIVATIVEATTRIB", DbType.Int32), pIDDerivativeAttrib);
            dp.Add(new DataParameter(CS, "STRIKE", DbType.Decimal), strikePrice);
            dp.Add(new DataParameter(CS, "PUTCALL", DbType.AnsiString, 1), putCall.HasValue ? ReflectionTools.ConvertEnumToString<PutOrCallEnum>(putCall.Value) : Convert.DBNull);
            dp.Add(new DataParameter(CS, "ISSECURITYSTATUS", DbType.Boolean), true);
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTENABLED), pDtEnabled); // FI 20201006 [XXXXX] Appel à GetDataParamter
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDAINS), pIdAIns);
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTINS), pDtIns);
            dp.Add(new DataParameter(CS, "CONTRACTMULTIPLIER", DbType.Decimal), pContratMultipler ?? Convert.DBNull);

            StrBuilder sqlquery = new StrBuilder(SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ASSET_ETD.ToString());
            sqlquery += "(IDENTIFIER,DISPLAYNAME,AII,IDDERIVATIVEATTRIB,STRIKEPRICE,PUTCALL,CONTRACTMULTIPLIER,ISSECURITYSTATUS,DTENABLED,IDAINS,DTINS)" + Cst.CrLf;
            sqlquery += "values (@IDENTIFIER,@DISPLAYNAME,@AII,@IDDERIVATIVEATTRIB,@STRIKE,@PUTCALL,@CONTRACTMULTIPLIER,@ISSECURITYSTATUS,@DTENABLED,@IDAINS,@DTINS)";

            // FI 20180927 [24202] Si l'asset existe déjà, (si DuplicateKey, Spheres® considère que la création est ok)
            QueryParameters qryParameters = new QueryParameters(CS, sqlquery.ToString(), dp);
            try
            {
                DataHelper.ExecuteNonQuery(CS, DbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                // FI 20211203 [XXXXX] usage de la méthode IsDuplicateKeyError
                if (
                    DataHelper.IsDuplicateKeyError(CS, ex)
                    &&
                    ((null == DbTransaction) || (null != DbTransaction && DataHelper.IsTransactionValid(DbTransaction)))
                   )
                {
                    //On continue (Rq.: sur SQLSERVER la transaction est non valide. Sur Oracle elle est valide)
                    isAssetAlreadyExist = true;
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                // FI 20180316 [23769] Remove in cache post insert
                // FI 20180920 [24183] => paramètre pIsWithDependencies valorisée à true 
                DataHelper.queryCache.Remove(Cst.OTCml_TBL.ASSET_ETD.ToString(), CS, true, false);
                DataEnabledHelper.ClearCache(CS, Cst.OTCml_TBL.ASSET_ETD);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pVR_ETD"></param>
        /// <param name="pIsValid"></param>
        /// <param name="pNewValidationMsg"></param>
        /// <param name="opValidationMsg"></param>
        /// <returns></returns>
        private static bool CheckValidationMsg(Cst.CheckModeEnum pVR_ETD, bool pIsValid, CheckMessageEnum pNewValidationMsg, ref CheckMessageEnum opValidationMsg)
        {
            bool isValid = pIsValid;

            if (false == isValid)
            {
                switch (pVR_ETD)
                {
                    case Cst.CheckModeEnum.Error:
                        opValidationMsg = pNewValidationMsg;
                        break;
                    case Cst.CheckModeEnum.Warning:
                        if (opValidationMsg == CheckMessageEnum.None) // Spheres® ne retourne que le premier Warning
                        {
                            opValidationMsg = pNewValidationMsg;
                        }
                        isValid = true;
                        break;
                    default:
                        opValidationMsg = CheckMessageEnum.None;
                        isValid = true;
                        break;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Insert la Maturity si elle n'existe pas, sinon Update la maturity lorsqu'elle ne dispose pas d'une maturityDate. 
        /// Puis, insère le DerivativeAttrib s'il n'existe pas.
        /// </summary>
        /// <param name="idA"></param>
        /// <param name="dtSys"></param>
        /// <param name="dtEnabled">date d'activation associée à la maturité et/ou à l'échéance ouverte (lorsqu'il y a création)</param>
        /// <param name="opVRDateMsg">OUT: Retourne un message constitué de "warning" (fonction des règles de validation sur les dates)</param>
        /// <param name="opInfoMsg">OUT: Retourne un resumé des actions menées</param>
        /// <returns>idDerivativeAttrib</returns>
        /// FI 20100429 FI Add paramètre pIdA 
        /// RD 20110128 Nouvel algorithme de calcul des dates
        /// RD 20110419 [17414] Offset for the closing date
        /// PL 20130515 Add pDtBusiness
        /// PL 20130604 use pDtBusiness comme date de référence pour la notion Enabled/Disabled
        /// FI 20131126 [19271] Gestion de la maturity date présente dans pFixInstrument
        /// FI 20131230 [19337] Modifications diverses
        /// PL 20170712 Add opDtEnabled
        /// PL 20211026 Refactoring important et mis een commentaire du [19271] 
        /// PL 20211026 Méthode devient NON STATIC
        private int InsertMaturityAndDerivativeAttrib2(int idA, DateTime dtSys, DateTime dtEnabled, out string opVRDateMsg, out string opInfoMsg)
        {
            if (!_isCharacteristicsValid)
            {
                //Il convient de faire appel à CheckCharacteristicsBeforeCreateAssetETD() avant de faire appel à cette méthode.
                throw new InvalidOperationException("Unauthorized procedure call, characteristics are unchecked or not valid");
            }

            #region Initialisation des variables
            opInfoMsg = string.Empty;
            opVRDateMsg = string.Empty;

            int idMaturity = 0;
            int idDerivativeAttrib = 0;
            bool isPeriodicDelivery = false;


            DateTime dtMaturityDate = DateTime.MinValue;
            DateTime dtMaturityDateSys = DateTime.MinValue;
            DateTime dtRolledDate = DateTime.MinValue;
            DateTime dtMaturityLastTrdDay = DateTime.MinValue;
            DateTime dtMaturityDeliveryDate = DateTime.MinValue;
            MaturityPeriodicDeliveryCharacteristics mpdc = new MaturityPeriodicDeliveryCharacteristics();
            #endregion

            #region Chargement du DerivativeContract, de la MaturityRule et de la Maturity

            // FI 20211112 [XXXXX] Mise en place d'une exception avec message claire si DC non chargé
            if (_sqlDerivativeContract == null)
                throw new NullReferenceException($"var sqlDerivativeContract is null. Derivative Contract (Exchange:{_fixInstrument.SecurityExchange}, Symbol:{_fixInstrument.Symbol}) not loaded.");


            SQL_Maturity sqlMaturity = new SQL_Maturity(CS, _fixInstrument.MaturityMonthYear, _dtBusiness)
            {
                DbTransaction = DbTransaction,
                //PL 20131119 [TRIM 19164] sqlMaturityRule.Id instead of sqlDerivativeContract.IdMaturityRule
                IdMaturityRuleIn = _sqlMaturityRuleCompliant.Id
            };

            bool isToCalc_MaturityDate = true;
            bool isToCalc_LastTrdDay = true;
            bool isToCalc_DeliveryDate = true;

            if (sqlMaturity.LoadTable(new string[] { "IDMATURITY", "MATURITYDATE", "MATURITYDATESYS", "LASTTRADINGDAY", "DELIVERYDATE", "FIRSTDELIVERYDATE", "LASTDELIVERYDATE", "FIRSTDLVSETTLTDATE", "LASTDLVSETTLTDATE" }))
            {
                idMaturity = sqlMaturity.Id;

                dtMaturityDate = sqlMaturity.MaturityDate;
                dtMaturityDateSys = sqlMaturity.MaturityDateSys;
                dtMaturityLastTrdDay = sqlMaturity.LastTradingDay;
                dtMaturityDeliveryDate = sqlMaturity.DelivryDate;

                mpdc.Initialisation(sqlMaturity);

                //PL 20211026 Add test on dtMaturityDateSys
                //isToCalc_MaturityDate = DtFunc.IsDateTimeEmpty(dtMaturityDate);
                isToCalc_MaturityDate = DtFunc.IsDateTimeEmpty(dtMaturityDate) || DtFunc.IsDateTimeEmpty(dtMaturityDateSys);
                isToCalc_LastTrdDay = DtFunc.IsDateTimeEmpty(dtMaturityLastTrdDay);
                isToCalc_DeliveryDate = DtFunc.IsDateTimeEmpty(dtMaturityDeliveryDate) && mpdc.IsExistDateEmpty();
            }
            #endregion

            Boolean isToCalc = isToCalc_MaturityDate || isToCalc_LastTrdDay || isToCalc_DeliveryDate;

            if (isToCalc)
            {
                CalcMaturityRuleDate calc = new CalcMaturityRuleDate(CS, _productBase, (_dataMarket.IdM, _dataMarket.IdBC),new MaturityRule(_sqlMaturityRuleCompliant));
                
                #region Calcul: MaturityDate, LastTradingDay, DeliveryDate

                if (isToCalc_MaturityDate)
                {
                    if (_fixInstrument.MaturityDateSpecified)
                    {
                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        // Exploitation de la date présente dans l'objet FixInstrument.MaturityDate (FIX Tag 541 / MatDt)
                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        #region TRIM 19271
                        // -------------------------------------------------------------------------------------------------------------------------------------
                        // FI 20131126 [19271] 
                        // Nous avions imaginé que depuis l'importation des trades, la date de de maturité de l'asset serait présente dans le fichier en entrée.
                        // Cette date serait alors utilisée pour renseigner la date de maturité de l'échéance (Table MATURITY).
                        // Le code développé pour cette fonctionnalité est conservé et archivé sur la version VSS NV.
                        // Chez XI, il  n'y a finalement jamais la date de maturité en entrée. Cette fonctionalité ne sera donc jamais utilisée
                        //
                        // PL 20211026 Dans le flux issu de la Gateway Eurex FIXML la date de maturité est parfois valorisée.
                        //             Toutefois je ne sais si cette date est bien remontée dans fixInstrument.MaturityDate.
                        // -------------------------------------------------------------------------------------------------------------------------------------
                        #endregion
                        dtMaturityDateSys = _fixInstrument.MaturityDate.DateValue;
                        dtMaturityDate = dtMaturityDateSys;
                    }
                    else
                    {
                        /// FI 20220705 [XXXXX] Appel systématique à calc.Calc_MaturityDate
                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        // Format YearMonthOnly (YYYYMM)   - Ce format inclut la possibilité d'échéances Monthly ou Weekly (théoriquement jamais Daily)
                        // Format YearMonthWeek (YYYYMMwX) - Ce format inclut la possibilité d'échéances Weekly (théoriquement uniquement Weekly)
                        // Format YearMonthDay  (YYYYMMDD) - Ce format inclut la possibilité d'échéances Monthly, Weekly ou Daily

                        (DateTime MaturityDateSys, DateTime MaturityDate) = calc.Calc_MaturityDate(_fixInstrument.MaturityMonthYear, out dtRolledDate);

                        dtMaturityDateSys = MaturityDateSys;
                        dtMaturityDate = MaturityDate;
                    }
                    //*****************************************************************************************************************************************
                    //WARNING: Code "minimal" pour gérer au "mieux" le CBOE
                    //*****************************************************************************************************************************************
                    if (_isQuaterlyMaturity) //Valorisé par CheckMaturityOnYearMonthDayFormat dans le cadre d'un format YearMonthDay (YYYYMMDD)
                    {
                        //Echéance Quaterly --> Le Last Trading Day est identique à la Maturity Date
                        dtMaturityLastTrdDay = dtMaturityDateSys;
                    }
                    //*****************************************************************************************************************************************
                }

                #region Calcul: Delivery Date (dtMaturityDeliveryDate ou dtMaturityFirstDeliveryDate/dtMaturityLastDeliveryDate)
                if (isToCalc_DeliveryDate && DtFunc.IsDateTimeFilled(dtMaturityDateSys))
                {
                    if (_sqlMaturityRuleCompliant.IsNoPeriodicDelivery)
                    {
                        // MaturityDeliveryDate est calculée en appliquant un décalage sur MaturityDate:
                        // 1- Si la MaturityDate est soit renseignée, soit bien calculée à l'étape précédente
                        // 2- Si un décalage est spécifié pour la Date réglt. Liv. (MaturityDeliveryDateOffsetMultiplier, MaturityDeliveryDateOffsetPeriod et MaturityDeliveryDateOffsetDaytype)
                        // 3- En utilisant le BC spécifié sur MARKET
                        isPeriodicDelivery = false;
                        dtMaturityDeliveryDate = calc.Calc_MaturityDeliveryDate((dtMaturityDateSys, dtMaturityDate));
                    }
                    else if (_sqlMaturityRuleCompliant.IsPeriodicDelivery)
                    {
                        isPeriodicDelivery = true;
                        mpdc = calc.Calc_MaturityPeriodicDeliveryDates(_fixInstrument.MaturityMonthYear);
                    }
                }
                #endregion

                #region Calcul: Last Trading Day (dtMaturityLastTrdDay)
                //--------------------------------------------------------------------------------------------------------------------------------------------------
                //PL 20211019 Concernant le commentaire ci-dessous sur la LTD, on trouve effectivement plus haut un forçage, mais pour les Quaterly (Pas Weekly) !!!
                //--------------------------------------------------------------------------------------------------------------------------------------------------
                if (isToCalc_LastTrdDay
                    && DtFunc.IsDateTimeEmpty(dtMaturityLastTrdDay) //WARNING: Cette date est parfois forcée, voir plus haut. (ex. Weekly on CBOE)
                    && DtFunc.IsDateTimeFilled(dtMaturityDateSys))
                {
                    
                    dtMaturityLastTrdDay = calc.Calc_MaturityLastTradingDay(dtMaturityDateSys, dtRolledDate);

                    if (DtFunc.IsDateTimeEmpty(dtMaturityLastTrdDay) && DtFunc.IsDateTimeFilled(mpdc.dates.dtFirstDlvSettlt))
                    {
                        //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        //PL 20170221 WARNING: Afin de répondre au besoin des ETD sur ENERGY (ex. PEGAS, EEX) pour lesquels le prix utilisé pour le
                        //                     calcul des cash-flows de livraison est le prix observé le 1er jour de règlement.          
                        //                     On met ici le LTD égal à cette date et on peut ainsi paramétrer la lecture du prix de clôture
                        //                     en date LTD, afin de répondre au besoin. (celà en attendant une évolution du référentiel MR).
                        //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                        dtMaturityLastTrdDay = mpdc.dates.dtFirstDlvSettlt;
                        //-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    }
                }
                #endregion

                #endregion
            }

            #region Validation Rules: VRMaturityDate, VRLastTradingDay, VRDeliveryDate
            if (DtFunc.IsDateTimeEmpty(dtMaturityDate))
                CheckVR_ETD(_sqlDerivativeContract.VRMaturityDate, AddAssetMessageEnum.MaturityDateNotSet, ref opVRDateMsg);
            if (DtFunc.IsDateTimeEmpty(dtMaturityLastTrdDay))
                CheckVR_ETD(_sqlDerivativeContract.VRLastTradingDay, AddAssetMessageEnum.MaturityLastTrdDayNotSet, ref opVRDateMsg);
            if (DtFunc.IsDateTimeEmpty(dtMaturityDeliveryDate) && mpdc.IsExistDateEmpty())
                CheckVR_ETD(_sqlDerivativeContract.VRDelivryDate, AddAssetMessageEnum.MaturityDelivryDateNotSet, ref opVRDateMsg);
            #endregion

            if (isToCalc)
            {
                if (isPeriodicDelivery)
                    mpdc.Initialisation(_sqlMaturityRuleCompliant);

                #region Maturity
                if (idMaturity == 0)
                {
                    #region Création d'une nouvelle échéance
                    //PL 20131119 [TRIM 19164] sqlMaturityRule.Id instead of sqlDerivativeContract.IdMaturityRule
                    int rowAffected = InsertMaturity2(dtMaturityDate, dtMaturityDateSys, dtMaturityLastTrdDay, dtMaturityDeliveryDate, mpdc, idA, dtSys, dtEnabled);

                    if (rowAffected > 0)
                        opInfoMsg += Cst.CrLf + Ressource.GetString2("Msg_ETD_MaturityCreated", _fixInstrument.MaturityMonthYear, _fixInstrument.Symbol);

                    //FI 20131230 [19337] selection de la colonne IDMATURITY uniquement
                    if (sqlMaturity.LoadTable(new string[] { "IDMATURITY" }))
                        idMaturity = sqlMaturity.Id;

                    if (idMaturity == 0)
                    {
                        // FI 20230130 [XXXXX] Test sans dtRefEnabled fin de mieux aiguiller l'uitlisateur 
                        SQL_Maturity sqlMaturityNoDtEnabled = new SQL_Maturity(CS, _fixInstrument.MaturityMonthYear);
                        if (sqlMaturityNoDtEnabled.LoadTable(new string[] { "IDMATURITY" }))
                            throw new Exception($"Maturity already exists but is not active on {DtFunc.DateTimeToString(sqlMaturity.DtRefForDtEnabled, DtFunc.FmtShortDate)}. Please change the activation period to use this maturity.\r\nMaturityRule: {LogTools.IdentifierAndId(_sqlMaturityRuleCompliant.Identifier, _sqlMaturityRuleCompliant.Id)}. Maturity: {_fixInstrument.MaturityMonthYear}");
                        else
                            throw new Exception($"Failed to create Maturity.\r\nMaturityRule: {LogTools.IdentifierAndId(_sqlMaturityRuleCompliant.Identifier, _sqlMaturityRuleCompliant.Id)}. Maturity: {_fixInstrument.MaturityMonthYear}");
                    }
                    #endregion
                }
                else
                {
                    #region Mise à jour d'une échéance existante
                    if (false == isToCalc_MaturityDate)
                        dtMaturityDate = DateTime.MinValue;          // Pour ne pas opérer de mise à jour de cette date
                    if (false == isToCalc_LastTrdDay)
                        dtMaturityLastTrdDay = DateTime.MinValue;    // Pour ne pas opérer de mise à jour de cette date
                    if (false == isToCalc_DeliveryDate)
                    {
                        dtMaturityDeliveryDate = DateTime.MinValue;  // Pour ne pas opérer de mise à jour de cette date
                        mpdc = null;                                 // Pour ne pas opérer de mise à jour de ces données 
                    }
                    else
                    {
                        if (isPeriodicDelivery)
                            dtMaturityDeliveryDate = DateTime.MinValue;
                        else
                            mpdc = null;
                    }
                    int rowAffected = UpdateMaturity(CS, DbTransaction, idMaturity, dtMaturityDate, dtMaturityDateSys, dtMaturityLastTrdDay, dtMaturityDeliveryDate, mpdc, idA, dtSys);

                    if (rowAffected > 0)
                    {
                        opInfoMsg += Cst.CrLf + Ressource.GetString2("Msg_ETD_MaturityDatesUpdated", _fixInstrument.MaturityMonthYear, _fixInstrument.Symbol);
                    }
                    #endregion
                }

                // FI 20230118 fait désormais dans UpdateMaturity et InsertMaturity2 (et donc pas répété ici) 
                //DataHelper.queryCache.Remove(Cst.OTCml_TBL.MATURITY.ToString(), CS, false, false);
                #endregion
            }

            if (idMaturity > 0)
            {
                #region DerivativeAttrib
                // FI 20210715 [25819] Usage de dtBusiness à la place de opDtEnabled dans l'appel à GetIdDerivativeAttrib
                // RD 20230120 [26242] Usage de dtBusiness à la place de opDtEnabled dans l'appel à GetIdDerivativeAttrib
                idDerivativeAttrib = ExchangeTradedDerivativeTools.GetIdDerivativeAttrib(CS, DbTransaction, idMaturity, _sqlDerivativeContract.Id, _dtBusiness, false);

                if (idDerivativeAttrib == 0)
                {
                    InsertDerivativeAttrib2(CS, DbTransaction, idMaturity, _sqlDerivativeContract.Id, idA, dtSys, dtEnabled);
                    // FI 20210715 [25819] Usage de dtBusiness à la place de opDtEnabled dans l'appel à GetIdDerivativeAttrib
                    // RD 20230120 [26242] Usage de dtBusiness à la place de opDtEnabled dans l'appel à GetIdDerivativeAttrib
                    idDerivativeAttrib = ExchangeTradedDerivativeTools.GetIdDerivativeAttrib(CS, DbTransaction, idMaturity, _sqlDerivativeContract.Id, _dtBusiness, false);
                }
                #endregion
            }

            opVRDateMsg = opVRDateMsg.Trim(Cst.CrLf.ToCharArray());
            opInfoMsg = opInfoMsg.Trim(Cst.CrLf.ToCharArray());

            return idDerivativeAttrib;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMpdc"></param>
        /// <param name="pDp"></param>
        /// <param name="opColList"></param>
        private static void InsertOrUpdatePeriodicDelivery(string pCS, MaturityPeriodicDeliveryCharacteristics pMpdc, DataParameters pDp, out string opColList)
        {
            opColList = ",FIRSTDELIVERYDATE,LASTDELIVERYDATE,FIRSTDLVSETTLTDATE,LASTDLVSETTLTDATE";
            opColList += ",PERIODMLTPDELIVERY,PERIODDELIVERY,DAYTYPEDELIVERY,ROLLCONVDELIVERY";
            opColList += ",DELIVERYTIMESTART,DELIVERYTIMEEND,DELIVERYTIMEZONE,ISAPPLYSUMMERTIME";
            opColList += ",PERIODMLTPDLVSETTLTOFFSET,PERIODDLVSETTLTOFFSET,DAYTYPEDLVSETTLTOFFSET,SETTLTOFHOLIDAYDLVCONVENTION";

            pDp.Add(new DataParameter(pCS, "FIRSTDELIVERYDATE", DbType.Date), pMpdc.dates.dtFirstDelivery);
            pDp.Add(new DataParameter(pCS, "LASTDELIVERYDATE", DbType.Date), pMpdc.dates.dtLastDelivery);
            pDp.Add(new DataParameter(pCS, "FIRSTDLVSETTLTDATE", DbType.Date), pMpdc.dates.dtFirstDlvSettlt);
            pDp.Add(new DataParameter(pCS, "LASTDLVSETTLTDATE", DbType.Date), pMpdc.dates.dtLastDlvSettlt);
            pDp.Add(new DataParameter(pCS, "PERIODMLTPDELIVERY", DbType.Int32), pMpdc.detail.deliveryDateMultiplier);
            pDp.Add(new DataParameter(pCS, "PERIODDELIVERY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.deliveryDatePeriod);
            pDp.Add(new DataParameter(pCS, "DAYTYPEDELIVERY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.deliveryDateDaytype);
            //CC/PL 20180315 Use RollConventionEnum.NONE if "null" (NB: on Sqlserver null is convert to string empty, but on Oracle null is convert to DBNULL)
            //pDp.Add(new DataParameter(pCS, "ROLLCONVDELIVERY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.deliveryDateRollConv);
            pDp.Add(new DataParameter(pCS, "ROLLCONVDELIVERY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), string.IsNullOrEmpty(pMpdc.detail.deliveryDateRollConv) ? RollConventionEnum.NONE.ToString() : pMpdc.detail.deliveryDateRollConv);
            pDp.Add(new DataParameter(pCS, "DELIVERYTIMESTART", DbType.AnsiString, SQLCst.UT_TIME_OPTIONAL_LEN), pMpdc.detail.deliveryDateTimeStart);
            pDp.Add(new DataParameter(pCS, "DELIVERYTIMEEND", DbType.AnsiString, SQLCst.UT_TIME_OPTIONAL_LEN), pMpdc.detail.deliveryDateTimeEnd);
            pDp.Add(new DataParameter(pCS, "DELIVERYTIMEZONE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.deliveryDateTimeZone);
            pDp.Add(new DataParameter(pCS, "ISAPPLYSUMMERTIME", DbType.Boolean), pMpdc.detail.deliveryDateApplySummerTime);
            pDp.Add(new DataParameter(pCS, "PERIODMLTPDLVSETTLTOFFSET", DbType.Int32), pMpdc.detail.dlvSettltDateOffsetMultiplier);
            pDp.Add(new DataParameter(pCS, "PERIODDLVSETTLTOFFSET", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.dlvSettltDateOffsetPeriod);
            pDp.Add(new DataParameter(pCS, "DAYTYPEDLVSETTLTOFFSET", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.dvlSettltDateOffsetDaytype);
            pDp.Add(new DataParameter(pCS, "SETTLTOFHOLIDAYDLVCONVENTION", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pMpdc.detail.dlvSettltHolidayConv);
        }

        /// <summary>
        /// Update Maturity 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pMaturityDate"></param>
        /// <param name="pMaturityDateSys"></param>
        /// <param name="pMaturityLastTrdDay"></param>
        /// <param name="pMaturityDelivryDate"></param>
        /// <param name="pMpdc"></param>
        /// <param name="pIdA"></param>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        //FI 20100429 FI Add paramètre pIdA
        private static int UpdateMaturity(string pCS, IDbTransaction pDbTransaction, int pIDMaturity,
            DateTime pMaturityDate, DateTime pMaturityDateSys, DateTime pMaturityLastTrdDay, DateTime pMaturityDelivryDate, MaturityPeriodicDeliveryCharacteristics pMpdc,
            int pIdA, DateTime pDtSys)
        {
            if (DtFunc.IsDateTimeFilled(pMaturityDate) || DtFunc.IsDateTimeFilled(pMaturityLastTrdDay) || DtFunc.IsDateTimeFilled(pMaturityDelivryDate) || (pMpdc != null))
            {
                DataParameters dp = new DataParameters();

                dp.Add(new DataParameter(pCS, "IDMATURITY", DbType.Int32), pIDMaturity);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), pIdA);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), pDtSys);

                StrBuilder sqlquery = new StrBuilder(SQLCst.UPDATE_DBO + Cst.OTCml_TBL.MATURITY.ToString()) + Cst.CrLf;
                sqlquery += "set DTUPD=@DTUPD, IDAUPD=@IDAUPD";

                if (DtFunc.IsDateTimeFilled(pMaturityDate))
                {
                    sqlquery += ",MATURITYDATE=@MATURITYDATE,MATURITYDATESYS=@MATURITYDATESYS";
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYDATE), pMaturityDate); // FI 20201006 [XXXXX] DbType.Date 
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYDATESYS), pMaturityDateSys); // FI 20201006 [XXXXX] DbType.Date 
                }
                if (DtFunc.IsDateTimeFilled(pMaturityLastTrdDay))
                {
                    sqlquery += ",LASTTRADINGDAY=@LASTTRADINGDAY";
                    dp.Add(new DataParameter(pCS, "LASTTRADINGDAY", DbType.Date), pMaturityLastTrdDay);
                }
                if (DtFunc.IsDateTimeFilled(pMaturityDelivryDate))
                {
                    sqlquery += ",DELIVERYDATE=@DELIVERYDATE";
                    dp.Add(new DataParameter(pCS, "DELIVERYDATE", DbType.Date), pMaturityDelivryDate);
                }
                if (pMpdc != null)
                {
                    InsertOrUpdatePeriodicDelivery(pCS, pMpdc, dp, out string colList);
                    string[] colList_Split = colList.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string col in colList_Split)
                        sqlquery += "," + col + "=@" + col;
                }

                SQLWhere sqlWhere = new SQLWhere();
                sqlWhere.Append("IDMATURITY=@IDMATURITY");

                QueryParameters qry = new QueryParameters(pCS, sqlquery.ToString() + Cst.CrLf + sqlWhere.ToString(), dp);
                int rowAffected = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                // FI 20230118 Add
                DataHelper.queryCache.Remove(Cst.OTCml_TBL.MATURITY.ToString(), pCS, false, false);
                DataEnabledHelper.ClearCache(pCS, Cst.OTCml_TBL.MATURITY);

                return rowAffected;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Insertion d'une échéance "disponible" (MATURITY)
        /// <para>NB: DtEnabled = DtBusiness - 1 Week</para>
        /// </summary>
        /// <param name="pMaturityDate"></param>
        /// <param name="pMaturityDateSys"></param>
        /// <param name="pMaturityLastTrdDay"></param>
        /// <param name="pMaturityDelivryDate"></param>
        /// <param name="pMpdc"></param>
        /// <param name="pIdA"></param>
        /// <param name="pDtsys"></param>
        /// <param name="pDtEnabled"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        private int InsertMaturity2(DateTime pMaturityDate, DateTime pMaturityDateSys, DateTime pMaturityLastTrdDay, DateTime pMaturityDelivryDate, MaturityPeriodicDeliveryCharacteristics pMpdc,
            int pIdA, DateTime pDtsys,
            DateTime pDtEnabled)
        {
            //PL 20170712 InsertMaturity2: pDtEnabled inclut pDtBusiness - 7 days
            //PL 20130924 Add pDtBusiness - 7 days
            //WARNING: Afin d'éviter les désagréments qui interdissaient sur un LateTrade J-1 d'utiliser un asset automatiquement créé sur un RegularTrade J,
            //         on créé dorénavant l'asset en Date Business - 1 semaine.
            //         See also: InsertAssetETD2(...), InsertDerivativeAttrib2(...)
            //pDtBusiness = pDtBusiness.AddDays(-7); 

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(CS, "IDMATURITYRULE", DbType.Int32), _sqlMaturityRuleCompliant.Id);
            dp.Add(new DataParameter(CS, "MATURITYMONTHYEAR", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), _fixInstrument.MaturityMonthYear);
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTENABLED), pDtEnabled);   // FI 20201006 [XXXXX] DbType.Date
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDAINS), pIdA);
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTINS), pDtsys);

            StrBuilder sqlquery = new StrBuilder(SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.MATURITY.ToString());
            sqlquery += "(IDMATURITYRULE,MATURITYMONTHYEAR";
            StrBuilder sqlqueryValues = new StrBuilder("values (@IDMATURITYRULE,@MATURITYMONTHYEAR");

            if (DtFunc.IsDateTimeFilled(pMaturityDate))
            {
                sqlquery += ",MATURITYDATE,MATURITYDATESYS";
                sqlqueryValues += ",@MATURITYDATE,@MATURITYDATESYS";
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.MATURITYDATE), pMaturityDate);
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.MATURITYDATESYS), pMaturityDateSys);
            }
            if (DtFunc.IsDateTimeFilled(pMaturityLastTrdDay))
            {
                sqlquery += ",LASTTRADINGDAY";
                sqlqueryValues += ",@LASTTRADINGDAY";
                dp.Add(new DataParameter(CS, "LASTTRADINGDAY", DbType.Date), pMaturityLastTrdDay);
            }
            if (DtFunc.IsDateTimeFilled(pMaturityDelivryDate))
            {
                sqlquery += ",DELIVERYDATE";
                sqlqueryValues += ",@DELIVERYDATE";
                dp.Add(new DataParameter(CS, "DELIVERYDATE", DbType.Date), pMaturityDelivryDate);
            }
            else if (!pMpdc.IsExistDateEmpty())
            {
                InsertOrUpdatePeriodicDelivery(CS, pMpdc, dp, out string colList);
                sqlquery += colList;
                sqlqueryValues += colList.Replace(",", ",@");
            }
            sqlquery += ",DTENABLED,IDAINS,DTINS)";
            sqlqueryValues += ",@DTENABLED,@IDAINS,@DTINS)";

            QueryParameters qry = new QueryParameters(CS, sqlquery.ToString() + Cst.CrLf + sqlqueryValues.ToString(), dp);
            int rowAffected = 0;
            try
            {
                rowAffected = DataHelper.ExecuteNonQuery(CS, DbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                if (
                    DataHelper.IsDuplicateKeyError(CS, ex)
                    &&
                    ((null == DbTransaction) || (null != DbTransaction && DataHelper.IsTransactionValid(DbTransaction)))
                   )
                {
                    //On continue (Rq.: sur SQLSERVER la transaction est non valide. Sur Oracle elle est valide)
                }
                else
                {
                    throw;
                }
            }

            // FI 20230118 Add
            DataHelper.queryCache.Remove(Cst.OTCml_TBL.MATURITY.ToString(), CS, false, false);
            DataEnabledHelper.ClearCache(CS, Cst.OTCml_TBL.MATURITY);
            return rowAffected;
        }

        /// <summary>
        /// Insertion d'une échéance ouverte (DERIVATIVEATTRIB)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCS"></param>
        /// <param name="pIdMaturity"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pIdA"></param>
        /// <param name="pDtEnabled"></param>
        /// <returns></returns>
        /// EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        /// FI 20180316 [23769] Modify
        private static int InsertDerivativeAttrib2(string pCS, IDbTransaction pDbTransaction, int pIdMaturity, int pIdDC, int pIdA, DateTime pDtSys, DateTime pDtEnabled)
        {
            //PL 20170712 InsertMaturity2: pDtEnabled inclut pDtBusiness - 7 days
            //PL 20130924 Add pDtBusiness - 7 days
            //WARNING: Afin d'éviter les désagréments qui interdissaient sur un LateTrade J-1 d'utiliser un asset automatiquement créé sur un RegularTrade J,
            //         on créé dorénavant l'asset en Date Business - 1 semaine.
            //         See also: InsertAssetETD2(...), InsertMaturity2(...)
            //pDtBusiness = pDtBusiness.AddDays(-7);

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDDC), pIdDC);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDMATURITY), pIdMaturity);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTENABLED), pDtEnabled); // FI 20200610 [XXXXX] DbType.Date (Call DataParameter.GetParameter)
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pIdA);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), pDtSys);

            /* FI 20211203 [XXXX] suppression de la clause where exists et ajout d'un try catch
            string sqlquery = StrFunc.AppendFormat(@"insert into dbo.DERIVATIVEATTRIB (IDMATURITY,IDDC,DTENABLED,IDAINS,DTINS)
select @IDMATURITY,@IDDC,@DTENABLED,@IDAINS,@DTINS {0}", SQLCst.FROM_DUAL.ToString());
            // FI 20200918 [XXXXX] le "where not exists" suivant est présent dans la requête. 
            // Avant il n'existait que sous Oracle®  
            sqlquery += StrFunc.AppendFormat(@"
where not exists(  
    select 1 
    from dbo.DERIVATIVEATTRIB da 
    where da.IDDC=@IDDC and da.IDMATURITY=@IDMATURITY and {0}
)", OTCmlHelper.GetSQLDataDtEnabled(pCS, "da", pDtEnabled));
            */
            int rowAffected = 0;
            string sqlquery = StrFunc.AppendFormat(@"insert into dbo.DERIVATIVEATTRIB(IDMATURITY,IDDC,DTENABLED,IDAINS,DTINS) values (@IDMATURITY,@IDDC,@DTENABLED,@IDAINS,@DTINS)");
            try
            {
                QueryParameters qryParameters = new QueryParameters(pCS, sqlquery.ToString(), dp);
                rowAffected = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                if (
                    DataHelper.IsDuplicateKeyError(pCS, ex)
                    &&
                    ((null == pDbTransaction) || (null != pDbTransaction && DataHelper.IsTransactionValid(pDbTransaction)))
                   )
                {
                    //On continue (Rq.: sur SQLSERVER la transaction est non valide. Sur Oracle elle est valide)
                }
                else
                {
                    throw;
                }
            }

            // FI 20180316 [23769] Remove in cache post insert
            DataHelper.queryCache.Remove(Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString(), pCS, false, false);
            DataEnabledHelper.ClearCache(pCS, Cst.OTCml_TBL.DERIVATIVEATTRIB);

            return rowAffected;
        }

        /// <summary>
        /// Contrôle que toutes les informations nécéssaires à la création d'un asset sont présentes et valides.
        /// </summary>
        /// <param name="pCategory"></param>
        /// <param name="pVRMsgEnum"></param>
        /// <returns></returns>
        /// Fi 20220613 [XXXXX] Add
        public bool CheckCharacteristicsBeforeCreateAssetETD2(CfiCodeCategoryEnum pCategory, out CheckMessageEnum pVRMsgEnum)
        {

            pVRMsgEnum = CheckMessageEnum.None;

            #region Contrôle IsAssetInfoFilled
            bool isValid = new FixInstrumentContainer(_fixInstrument).IsAssetInfoFilled(true, pCategory == CfiCodeCategoryEnum.Option);
            if (false == isValid)
                pVRMsgEnum = CheckMessageEnum.CharacteristicMissing;
            #endregion

            #region Contrôle du Derivative Contract 
            if (isValid)
            {
                if (null == _sqlDerivativeContract)
                    pVRMsgEnum = CheckMessageEnum.ContractNotFound;
                else if ((_sqlDerivativeContract.IdMaturityRule == 0))
                    pVRMsgEnum = CheckMessageEnum.ContractMaturityRuleNotFound;
                else if (!_sqlDerivativeContract.IsAutoCreateAsset)
                    pVRMsgEnum = CheckMessageEnum.ContractNotAutoCreateAsset;

                isValid = (pVRMsgEnum == CheckMessageEnum.None);
            }
            #endregion

            #region Derivative Contract Option: Contrôle du Strike
            if (isValid && (_sqlDerivativeContract.CfiCodeCategory == CfiCodeCategoryEnum.Option))
            {
                decimal strike = _fixInstrument.StrikePrice.DecValue;

                if (null != _sqlDerivativeContract.StrikeIncrement && _sqlDerivativeContract.StrikeIncrement != 0)
                {
                    #region Strike is Valid according to StrikeIncrement
                    decimal strikeIncrement = (decimal)_sqlDerivativeContract.StrikeIncrement;
                    decimal strikeTemp = strike;

                    if (null != _sqlDerivativeContract.StrikeMinValue)
                        strikeTemp -= (decimal)_sqlDerivativeContract.StrikeMinValue;

                    isValid = (decimal.Remainder(strikeTemp, strikeIncrement) == 0);
                    #endregion

                    isValid = CheckValidationMsg(_sqlDerivativeContract.VRStrikeIncrement, isValid, CheckMessageEnum.StrikeIncrementIncorrect, ref pVRMsgEnum);
                }
                if (isValid)
                {
                    #region Strike is Between StrikeMinValue and StrikeMaxValue
                    if (null != _sqlDerivativeContract.StrikeMinValue && strike < _sqlDerivativeContract.StrikeMinValue)
                        isValid = false;

                    if (null != _sqlDerivativeContract.StrikeMaxValue && strike > _sqlDerivativeContract.StrikeMaxValue)
                        isValid = false;
                    #endregion

                    isValid = CheckValidationMsg(_sqlDerivativeContract.VRStrikePxRange, isValid, CheckMessageEnum.StrikePxRangeIncorrect, ref pVRMsgEnum);
                }
            }
            #endregion

            #region Contrôle que le format de l'échéance (IFixInstrument.MaturityMonthYear), nécessairement dans l'un des format FIX ici, est au minimum autorisé par une des MR permises sur le DC
            Cst.MaturityMonthYearFmtEnum maturityMonthYearFmt = default;
            if (isValid)
            {
                isValid = (GetDCMRDistinctFormat().Where(x => MaturityHelper.IsInputInFixFormat(_fixInstrument.MaturityMonthYear, x)).Count() > 0);
                if (isValid)
                {
                    MaturityHelper.IsInputInFixFormat(_fixInstrument.MaturityMonthYear, out maturityMonthYearFmt);
                }
                else
                {
                    pVRMsgEnum = CheckMessageEnum.MaturityFormatIncorrect;
                }
            }
            #endregion

            #region si échéance au format YYYYMMDD, Contrôle que l'échéance saisie n'est pas un jour fériée
            if (isValid && maturityMonthYearFmt == Cst.MaturityMonthYearFmtEnum.YearMonthDay)
            {
                // FI 20221118 [26178] Prise en compte uniquement des jours fériés de type ScheduledTradingDay
                isValid = (false == Tools.IsHoliday(CSTools.SetCacheOn(CS), GetDateMaturityMonthYear(), new string[] { _dataMarket.IdBC }, DayTypeEnum.ScheduledTradingDay));
                isValid = CheckValidationMsg(Cst.CheckModeEnum.Error, isValid, CheckMessageEnum.MaturityIsHoliday, ref pVRMsgEnum);
            }
            #endregion

            #region Contrôle de la compatibilité de l'échéance (IFixInstrument.MaturityMonthYear) avec le cycle des échéances autorisées
            if (isValid)
            {

                //PL 20131112 [TRIM 19164] Utilisation des données MM/YYYY de l'échéance, pour constituer une date 01/MM/YYYY utilisée comme date de référence pour la notion Enabled/Disabled
                string dtRef = _fixInstrument.MaturityMonthYear.Substring(0, 6) + "01";
                if (false == StrFunc.IsDate(dtRef, DtFunc.FmtDateyyyyMMdd, null, out DateTime dtRefForDtEnabled))
                    throw new InvalidOperationException($"{dtRef} is not a yyyyMMdd date");

                //idMR : Liste des MR(s) qui respectent le format de l'échéance (IFixInstrument.MaturityMonthYear)  
                IEnumerable<int> idMR = from item in _DCMR.Where(x => x.MaturityFormat == maturityMonthYearFmt) select item.IdMR;

                // Cas 1 : il existe uniquement 1 MR sur le DC (Comportement identique à v10)
                // => Spheres® vérifie(*) que l'échéance (IFixInstrument.MaturityMonthYear) est compatible avec la MR. 
                // (*) le contrôle prend en considération la Validation Rule _sqlDerivativeContract.VRMaturityDate
                //
                // Case 2 : il existe de 2 à n  MR (Nouveau comportement à partir de la v12)
                // => Spheres retient la première MR compatible avec l'échéance (IFixInstrument.MaturityMonthYear), sans traitement des éventuelles MR suivantes. 
                // Si aucune MR n'est compatible(**), retourne l'erreur rencontrée sur la 1er MR (la MR prioritaire)
                // (**) le contrôle est stricte et ne prend pas en considération la Validation Rule _sqlDerivativeContract.VRMaturityDate

                if (idMR.Count() == 1)
                {
                    // Cas 1
                    isValid = IsMaturityMonthYearCompliantWithMR(idMR.First(), dtRefForDtEnabled, _sqlDerivativeContract.VRMaturityDate, out CheckMessageEnum VRMsgMREnum);
                    if ((pVRMsgEnum == CheckMessageEnum.None) && (VRMsgMREnum != CheckMessageEnum.None))
                        pVRMsgEnum = VRMsgMREnum;

                    if (isValid)
                        SetMaturityCompliant(idMR.First(), dtRefForDtEnabled);
                }
                else
                {
                    // Cas 2
                    Boolean isMaturityMonthYearCompliant = false;
                    CheckMessageEnum firstVRMsgMREnum = CheckMessageEnum.None;
                    foreach (int itemMR in idMR)
                    {
                        isMaturityMonthYearCompliant = IsMaturityMonthYearCompliantWithMR(itemMR, dtRefForDtEnabled, Cst.CheckModeEnum.Error, out CheckMessageEnum VRMsgMREnum);

                        if (firstVRMsgMREnum == CheckMessageEnum.None && VRMsgMREnum != CheckMessageEnum.None)
                            firstVRMsgMREnum = VRMsgMREnum;

                        if (isMaturityMonthYearCompliant)
                        {
                            SetMaturityCompliant(itemMR, dtRefForDtEnabled);
                            break;
                        }
                    }

                    isValid = isMaturityMonthYearCompliant;
                    if (isValid)
                    {
                        // FI 20220812 [XXXXX] petite correction
                        // FI 20220817 [XXXXX] nouvelle correction (la correction du 20220812 était incorrecte)
                        // => si valid, ilne faut pas toucher à pVRMsgEnum qui pourrait par ailleurs être renseigné par des contrôles antérieurs à "Contrôle de la compatibilité de l'échéance (IFixInstrument.MaturityMonthYear) avec le cycle des échéances autorisées"  

                        // pVRMsgEnum = CheckMessageEnum.None;
                        // if ((pVRMsgEnum == CheckMessageEnum.None) && (firstVRMsgMREnum != CheckMessageEnum.None))
                        //    pVRMsgEnum = firstVRMsgMREnum;
                    }
                    else
                    {
                        pVRMsgEnum = firstVRMsgMREnum;

                        // FI 20230217 [XXXXX][WI560] 
                        // Afin de gérer une échéance atypique, Spheres® recherche si l'échéance est existe malgré tout (parce que par exemple saisie manuellement) si tel est le cas elle est acceptée
                        // Cette échéance doit nécesairement être rattachée à une des MRs rattachées au DC et active 
                        if (_sqlDerivativeContract.VRMaturityDate == Cst.CheckModeEnum.None || _sqlDerivativeContract.VRMaturityDate == Cst.CheckModeEnum.Warning)
                        {
                            IEnumerable<(int idMaturityRule, String maturityMonthYear)> maturity = LoadMaturity(CSTools.SetCacheOn(this.CS), idMR, dtRefForDtEnabled);
                            IEnumerable<(int idMaturityRule, String maturityMonthYear)> maturityFind = maturity.Where(x => x.maturityMonthYear == _fixInstrument.MaturityMonthYear);
                            if (maturityFind.Count() > 0)
                            {
                                isValid = true;
                                int idMaturiyRuleCompliant = (from item in maturityFind
                                                              select new
                                                              {
                                                                  idMaturiyRule = item.idMaturityRule,
                                                                  order = _DCMR.Where(x => x.IdMR == item.idMaturityRule).First().SequenceNumber
                                                              }).OrderBy(x => x.order).FirstOrDefault().idMaturiyRule;

                                SetMaturityCompliant(idMaturiyRuleCompliant, dtRefForDtEnabled);

                                if (_sqlDerivativeContract.VRMaturityDate == Cst.CheckModeEnum.Warning)
                                    pVRMsgEnum = CheckMessageEnum.MaturityPeriodIncorrect;
                                else
                                    pVRMsgEnum = CheckMessageEnum.None;
                            }
                        }
                    }
                }
            }
            #endregion

            _isCharacteristicsValid = isValid;

            return isValid;
        }


        /// <summary>
        /// Contrôle de la compatibilité de l'échéance (IFixInstrument.MaturityMonthYear) avec le cycle des échéances autorisées sur la MR <paramref name="pIdMR"/>
        /// </summary>
        /// <param name="pIdMR">Représente la MR</param>
        /// <param name="pCheckMode"></param>
        /// <param name="opVRMessage"></param>
        /// <returns></returns>
        /// FI 20220613 [XXXXX] Add
        public Boolean IsMaturityMonthYearCompliantWithMR(int pIdMR, DateTime pDtRefForDtEnabled, Cst.CheckModeEnum pCheckMode, out CheckMessageEnum opVRMessage)
        {
            if (pIdMR == 0)
                throw new ArgumentException("Value:0 not allowed", nameof(pIdMR));

            if (null == _sqlDerivativeContract)
                throw new InvalidOperationException("_sqlDerivativeContract is null");

            if (_dataMarket.IdM ==0 )
                throw new InvalidOperationException("Market is unknown");

            Boolean ret = true;
            opVRMessage = CheckMessageEnum.None;

            SQL_MaturityRuleActive sqlItemMR = GetSQLMaturityRuleActive(pIdMR, pDtRefForDtEnabled);

            switch (sqlItemMR.MaturityFormatEnum)
            {
                case Cst.MaturityMonthYearFmtEnum.YearMonthDay:
                case Cst.MaturityMonthYearFmtEnum.YearMonthOnly:
                    #region YearMonthOnly, YearMonthDay
                    //----------------------------------------------------------------------------------------------------------
                    //Warning: Pour l'instant, on n'exploite pas la valeur de: sqlMaturityRule.MaturityOpenSimultaneously (TODO)
                    //----------------------------------------------------------------------------------------------------------
                    ret = StrFunc.IsFilled(sqlItemMR.MaturityMMYRule);
                    ret = CheckValidationMsg(pCheckMode, ret, CheckMessageEnum.MaturityPeriodNoControl, ref opVRMessage);

                    if (ret && StrFunc.IsFilled(sqlItemMR.MaturityMMYRule))
                    {
                        if (sqlItemMR.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthOnly)
                        {
                            string MM = _fixInstrument.MaturityMonthYear.Substring(4, 2);
                            //On vérifie si le mois de l'échéance saisie est un mois autorisé par la règle en vigueur.
                            ret = (sqlItemMR.MaturityMMYRule.IndexOf(StrFunc.GetMaturityLetter(MM)) >= 0);
                            //TODO Il faudrait vérifier si le couple MM/YYYY est autorisé, mais en l'état le référentiel Spheres ne permet pas ce paramétrage suffisament finement.
                            //     Ex. Le 20/10/2021 l'échéance H22 (Juin 2022) est ouverte, mais l'échéance H23 (Juin 2023) ne l'est pas encore.
                        }
                        else if (sqlItemMR.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay)
                        {
                            if (sqlItemMR.IsMaturityMMYRule_Multiple || sqlItemMR.IsMaturityMMYRule_Monthly)
                            {
                                ret = CheckMaturityMonthYearOnMultipleMonthly(sqlItemMR);
                            }
                            else if (sqlItemMR.IsMaturityMMYRule_Weekly) // FI 20220624 [XXXXX] Gestion des frequences Weekly
                            {
                                ret = CheckMaturityMonthYearOnWeekly(sqlItemMR, pDtRefForDtEnabled);
                            }
                            else
                            {
                                ret = true;
                            }
                        }
                        else
                        {
                            throw new NotImplementedException($"Format :{sqlItemMR.MaturityFormatEnum} is not implemented");
                        }
                    }
                    #endregion
                    break;

                case Cst.MaturityMonthYearFmtEnum.YearMonthWeek:
                    #region YearMonthWeek
                    if (_fixInstrument.MaturityMonthYear.EndsWith("w5"))
                    {
                        //Vérification d'existance d'une 5ème semaine sur le mois saisie
                        _ = _fixInstrument.MaturityMonthYear.Substring(4, 2);
                        bool isExistFifthWeek = true; //TODO
                        ret = isExistFifthWeek;
                    }
                    #endregion
                    break;
            }

            ret = CheckValidationMsg(pCheckMode, ret, CheckMessageEnum.MaturityPeriodIncorrect, ref opVRMessage);

            return ret;
        }

        /// <summary>
        ///  Sur les DC qui autorisent n MaturityRule, peut retourner l'échéance ouverte équivalente(*) à l'échéance présente dans fixInstrument.MaturityMonthYear
        ///  (*) équivalente signifie => même date d'échéance et de format différent ( associé à une autre MR)
        /// </summary>
        /// <returns></returns>
        /// FI 20220613 [XXXXX] Add
        private (int idDerivativeAttrib, string maturityMonthYear) FindIdenticalDerivativeAttrib()
        {

            (int idDerivativeAttrib, string maturityMonthYear) ret = (0, string.Empty);

            if (_DCMR.Count() > 1)
            {
                DateTime dtMaturityDateSys = DateTime.MinValue;
                Cst.MaturityMonthYearFmtEnum fmtMaturityMonthYear = default;
                CalcMaturityRuleDate calc = new CalcMaturityRuleDate(CS, _productBase, (_dataMarket.IdM, _dataMarket.IdBC), new MaturityRule(_sqlMaturityRuleCompliant));

                bool isCheckExistingDerivativeAttrib;
                switch (_sqlMaturityRuleCompliant.MaturityFormatEnum)
                {
                    case Cst.MaturityMonthYearFmtEnum.YearMonthOnly:
                    case Cst.MaturityMonthYearFmtEnum.YearMonthWeek:
                        isCheckExistingDerivativeAttrib = GetDCMRDistinctFormat().Contains(Cst.MaturityMonthYearFmtEnum.YearMonthDay);
                        if (isCheckExistingDerivativeAttrib)
                        {
                            dtMaturityDateSys = calc.Calc_MaturityDate( _fixInstrument.MaturityMonthYear, out _).MaturityDateSys;
                            fmtMaturityMonthYear = Cst.MaturityMonthYearFmtEnum.YearMonthDay;
                        }
                        break;
                    case Cst.MaturityMonthYearFmtEnum.YearMonthDay:
                        isCheckExistingDerivativeAttrib = (_sqlMaturityRuleCompliant.FrequencyMaturity == Cst.FrequencyMonthly)
                                                            && GetDCMRDistinctFormat().Contains(Cst.MaturityMonthYearFmtEnum.YearMonthOnly);

                        if (isCheckExistingDerivativeAttrib)
                        {
                            dtMaturityDateSys = calc.Calc_MaturityDate(_fixInstrument.MaturityMonthYear, out _).MaturityDateSys;
                            fmtMaturityMonthYear = Cst.MaturityMonthYearFmtEnum.YearMonthOnly;
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Format (Id:{_sqlMaturityRuleCompliant.MaturityFormatEnum}) is not implemented");
                }


                if (isCheckExistingDerivativeAttrib && DtFunc.IsDateTimeFilled(dtMaturityDateSys))
                {
                    // FI 20230213 [XXXXX] Spheres® ne prend en considération que les MRs enabled nécessairement rattachées aux DC
                    int[] idMR = (from item in _DCMR.Where(x => x.MaturityFormat == fmtMaturityMonthYear)
                                  select item.IdMR).ToArray();

                    string query = $@"select da.IDDERIVATIVEATTRIB, ma.MATURITYMONTHYEAR
from dbo.DERIVATIVEATTRIB da
inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC=da.IDDC and dc.IDDC=@IDDC
inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY and ma.MATURITYDATESYS=@MATURITYDATESYS and {OTCmlHelper.GetSQLDataDtEnabled(CS, "ma", _dtBusiness)}
inner join dbo.MATURITYRULE mr on mr.IDMATURITYRULE=ma.IDMATURITYRULE and mr.MMYFMT=@MMYFMT and {DataHelper.SQLColumnIn(CS, "mr.IDMATURITYRULE", idMR, TypeData.TypeDataEnum.integer)}";

                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDDC), _sqlDerivativeContract.Id);
                    dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.MATURITYDATESYS), dtMaturityDateSys);
                    dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.MMYFMT), ReflectionTools.ConvertEnumToString<Cst.MaturityMonthYearFmtEnum>(fmtMaturityMonthYear));


                    QueryParameters qryParameters = new QueryParameters(CS, query, dp);
                    using (IDataReader dr = DataHelper.ExecuteReader(CS, DbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            ret.idDerivativeAttrib = Convert.ToInt32(dr["IDDERIVATIVEATTRIB"]);
                            ret.maturityMonthYear = Convert.ToString(dr["MATURITYMONTHYEAR"]);
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Alimentation de _sqlMaturityRuleCompliant
        /// </summary>
        /// <param name="idMR"></param>
        /// <param name="dtRefForDtEnabled"></param>
        private void SetMaturityCompliant(int idMR, DateTime dtRefForDtEnabled)
        {
            _sqlMaturityRuleCompliant = GetSQLMaturityRuleActive(idMR, dtRefForDtEnabled);
        }

        /// <summary>
        /// Insertion d'un nouvel asset ETD, puis Load de celui-ci. 
        /// Précédé, s'il y a lieu de l'Insertion (ou mise à jour) de la Maturity associée et du DerivativeAttrib.
        /// <para>NB: Il est nécessaire de faire appel à <see cref="CheckCharacteristicsBeforeCreateAssetETD2(CfiCodeCategoryEnum, out CheckMessageEnum)"/> avant de faire appel à cette méthode.</para>
        /// </summary>
        /// <param name="idA"></param>
        /// <param name="dtSys"></param>
        /// <param name="opVRDateMsg">OUT: Retourne un message constitué de "warning" (fonction des règles de validation sur les dates)</param>
        /// <param name="opInfoMsg">OUT: Retourne un resumé des actions menées</param>
        /// <param name="isAssetAlreadyExist">OUT : Retourne true si l'asset existe déjà</param>
        /// <exception cref="NullReferenceException">Lorsque l'asset n'est pas loaded</exception>
        /// <exception cref="InvalidOperationException">Lorsque l'appel de <see cref="CheckCharacteristicsBeforeCreateAssetETD2(CfiCodeCategoryEnum, out CheckMessageEnum)"/> n'a pas été efffectué</exception>
        /// <returns>L'asset ETD créé</returns>
        /// FI 20170320 [22985] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        /// FI 20180927 [24202] Add parameter opIsAssetAlreadyExist
        public SQL_AssetETD InsertAndLoadAssetETD(int idA, DateTime dtSys, out string opVRDateMsg, out string opInfoMsg, out Boolean isAssetAlreadyExist)
        {
            if (!_isCharacteristicsValid)
            {
                //Il convient de faire appel à CheckCharacteristicsBeforeCreateAssetETD() avant de faire appel à cette méthode.
                throw new InvalidOperationException("Unauthorized procedure call, characteristics are unchecked or not valid");
            }
            SQL_AssetETD sql_AssetETD = null;
            opVRDateMsg = string.Empty;
            opInfoMsg = string.Empty;

            isAssetAlreadyExist = false;

            (int idDerivativeAttrib, string maturityMonthYear) findIdenticalDerivativeAttrib = FindIdenticalDerivativeAttrib();

            Boolean isFinDerivativeAttrib = (findIdenticalDerivativeAttrib.idDerivativeAttrib > 0);

            if (isFinDerivativeAttrib)
            {
                IFixInstrument fixInstrumentClone = (IFixInstrument)ReflectionTools.Clone(_fixInstrument, ReflectionTools.CloneStyle.CloneField);
                fixInstrumentClone.MaturityMonthYear = findIdenticalDerivativeAttrib.maturityMonthYear;

                sql_AssetETD = AssetTools.LoadAssetETD(CSTools.SetCacheOn(CS, 1, null), DbTransaction, _sqlDerivativeContract.IdI, findIdenticalDerivativeAttrib.idDerivativeAttrib, _sqlDerivativeContract.CfiCodeCategory, fixInstrumentClone, _dtBusiness, out _);

                isAssetAlreadyExist = (null != sql_AssetETD);
            }

            if (false == isAssetAlreadyExist)
            {
                // FI 20230214 [XXXXX] call GetDatEnabledForInsert
                DateTime dtEnabled = GetDatEnabledForInsert();

                int idDerivativeAttrib;

                if (isFinDerivativeAttrib)
                {
                    idDerivativeAttrib = findIdenticalDerivativeAttrib.idDerivativeAttrib;
                    //Lorsque l'échéance est déjà ouverte, remplacement de l'échéance par celle présente sur l'échéance ouverte (nécessaire puisqu'il y a appel à AssetTools.LoadAssetETD)
                    _fixInstrument.MaturityMonthYear = findIdenticalDerivativeAttrib.maturityMonthYear;
                }
                else
                {
                    idDerivativeAttrib = InsertMaturityAndDerivativeAttrib2(idA, dtSys, dtEnabled, out opVRDateMsg, out opInfoMsg);
                }

                if (idDerivativeAttrib <= 0)
                {
                    throw new InvalidOperationException("Error on insert DerivativeAttrib, idDerivativeAttrib equal 0");
                }

                // FI 20170320 [22985] Appel ExchangeTradedDerivativeTools.CalcContractMultiplier uniquement s'il n'existe pas de CONTRACTMULTIPLIER sur DERIVATIVEATTRIB 
                // pour être en phase avec le cacul des frais (voir méthode  ExchangeTradedDerivativeContainer.FindDefaultContractMultiplier)
                // Si la méthode retourne une valeur cette dernière alimente ASSET_ETD.CONTRACTMULTIPLIER
                Nullable<Decimal> contractMultiplier = null;
                if (null == ExchangeTradedDerivativeTools.GetDerivativeAttribContractMultiplier(CS, DbTransaction, idDerivativeAttrib))
                {
                    contractMultiplier = ExchangeTradedDerivativeTools.CalcContractMultiplier(CS, _fixInstrument, _dtBusiness);
                }

                // Insertion de l'asset dans la table ASSET_ETD
                InsertAssetETD2(idDerivativeAttrib, idA, dtSys, dtEnabled, contractMultiplier, out isAssetAlreadyExist);

                // PL 20211026 Add SetCacheOn
                // NB: on met en cache le résultat de la query uniquement si celle-ci retourne au moins une ligne (donc un Asset). Aucune mise en cache si aucun Asset retournée.
                sql_AssetETD = AssetTools.LoadAssetETD(CSTools.SetCacheOn(CS, 1, null), DbTransaction, _sqlDerivativeContract.IdI, idDerivativeAttrib, _sqlDerivativeContract.CfiCodeCategory, _fixInstrument, _dtBusiness, out string errMsg);
                if (null == sql_AssetETD)
                {
                    //WARNING: Etrangement quand on avance pas à pas on arrive sur ce throw, mais il semble, à juste titre, ne pas se déclencher (PL 20130722)
                    string msg = @"Error: Asset not loaded!" + Cst.CrLf;
                    msg += String.Format(@"- Load information: {0}", errMsg) + Cst.CrLf;
                    throw new NullReferenceException(msg);
                }
            }

            return sql_AssetETD;
        }

        /// <summary>
        ///  Retourne la date d'activation qu'il faut appliquer à toute nouvelle insertion d'échéances ouvertes (tables: MATURITY, DERIVATIVEATTRIB) et asset ETD (table: ASSET_ETD)
        ///  <para>WARNING: Afin d'éviter les désagréments qui interdissaient sur un LateTrade J-1 d'utiliser un asset automatiquement créé sur un RegularTrade J,on créé dorénavant l'asset en Date Business moins 1 semaine, Excepté, lorsque cette création résulte d'une C.A., pur cela, on se retient le max avec la date d'activation en vigueur sur le DC parent de l'asset. </para>
        /// </summary>
        /// <returns></returns>
        /// FI 20230214 [XXXXX] Add Method
        private DateTime GetDatEnabledForInsert()
        {
            if (null == _sqlDerivativeContract)
                throw new InvalidOperationException("_sqlDerivativeContract is null");

            DateTime ret;

            DateTime dtEnabledDefault = _dtBusiness.AddDays(-7);

            DateTime dtEnabledDC = Convert.ToDateTime(_sqlDerivativeContract.GetFirstRowColumnValue("DTENABLED"));
            if (DateTime.Compare(dtEnabledDefault, dtEnabledDC) <= 0)
                ret = dtEnabledDC;
            else
                ret = dtEnabledDefault;

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pVR_ETD"></param>
        /// <param name="pMsgEnum"></param>
        /// <param name="pMsg"></param>
        /// <exception cref="Exception"></exception>
        private static void CheckVR_ETD(Cst.CheckModeEnum pVR_ETD, AddAssetMessageEnum pMsgEnum, ref string pMsg)
        {
            string tempMsg = GetResMessage(pMsgEnum);
            //
            switch (pVR_ETD)
            {
                case Cst.CheckModeEnum.Error:
                    throw new Exception(tempMsg);
                case Cst.CheckModeEnum.Warning:
                    if (StrFunc.IsFilled(pMsg))
                        pMsg += Cst.CrLf;
                    //
                    pMsg += tempMsg;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCheckMessage"></param>
        /// <returns></returns>
        private static string GetResMessage(AddAssetMessageEnum pCheckMessage, params string[] pArg)
        {
            if (ArrFunc.IsEmpty(pArg))
                return Ressource.GetString("Msg_ETD_" + pCheckMessage);
            else
                return Ressource.GetString2("Msg_ETD_" + pCheckMessage, pArg);
        }

        /// <summary>
        /// Contrôle de la compatibilité de l'échéance (IFixInstrument.MaturityMonthYear) avec la MR <paramref name="sqlItemMR"/>. 
        /// <para>Le format de l'échéance est YYYYMMDD. Le cycle de la MR est Monthly ou Multiple.</para>
        /// </summary>
        /// <param name="sqlItemMR"></param>
        /// <exception cref="ArgumentException">Lorsque la MR n'est pas Multiple ou Monthly ou format différent de YYYYMMDD</exception> 
        /// <returns></returns>
        private bool CheckMaturityMonthYearOnMultipleMonthly(SQL_MaturityRuleBase sqlItemMR)
        {

            if (false == (sqlItemMR.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay))
                throw new ArgumentException($"{nameof(sqlItemMR)}. Format:{sqlItemMR.MaturityFormatEnum} invalid.");

            if (false == (sqlItemMR.IsMaturityMMYRule_Multiple || sqlItemMR.IsMaturityMMYRule_Monthly))
                throw new ArgumentException($"{nameof(sqlItemMR)}. Cycle:{sqlItemMR.MaturityMMYRule} invalid.");


            bool ret = false;

            DateTime dtMaturityMonthYear = GetDateMaturityMonthYear();
            string MM = dtMaturityMonthYear.ToString("MM");

            //--------------------------------------------------------
            //Vérification du mois
            //--------------------------------------------------------
            //Si Monthly ou, le cas échéant, Quaterly, on commence par vérifier si le mois de l'échéance saisie est un mois autorisé par la règle en vigueur.
            //m --> month. ex.: m=FGJKNQVX|q=HMUZ
            string maturityRule = sqlItemMR.IsMaturityMMYRule_Monthly ? sqlItemMR.MaturityMMYRule : sqlItemMR.MaturityMMYRule_Multiple_Monthly;
            if (!String.IsNullOrEmpty(maturityRule))
            {
                //On vérifie si le mois de l'échéance saisie est un mois autorisé par la règle en vigueur.
                ret = (maturityRule.IndexOf(StrFunc.GetMaturityLetter(MM)) >= 0);
                //TODO Il faudrait vérifier si le couple MM/YYYY est autorisé, mais en l'état le référentiel Spheres ne permet pas ce paramétrage suffisament finement.
                //     Ex. Le 20/10/2021 l'échéance H22 (Juin 2022) est ouverte, mais l'échéance H23 (Juin 2023) ne l'est pas encore
            }
            if ((!ret) && sqlItemMR.IsMaturityMMYRule_Multiple)
            {
                maturityRule = sqlItemMR.MaturityMMYRule_Multiple_Quaterly;
                if (!String.IsNullOrEmpty(maturityRule))
                {
                    //On vérifie si le mois de l'échéance saisie est un mois autorisé par la règle en vigueur.
                    ret = (maturityRule.IndexOf(StrFunc.GetMaturityLetter(MM)) >= 0);
                    //TODO Il faudrait vérifier si le couple MM/YYYY est autorisé, mais en l'état le référentiel Spheres ne permet pas ce paramétrage suffisament finement.
                    //     Ex. Le 20/10/2021 l'échéance H22 (Juin 2022) est ouverte, mais l'échéance H23 (Juin 2023) ne l'est pas encore
                }
            }

            if (ret)
            {
                //Si le mois de l'échéance saisie est un mois autorisé par la règle en vigueur, on poursuit la vérification...

                #region 1- Si la règle autorise des échéances MONTHLY, on contrôle si le nom de l'échéance saisie est compatible avec paramétrage en vigueur (ex.: CBOE - 3ème samedi)
                if (sqlItemMR.IsMaturityMMYRule_Multiple_MonthlyInclude || sqlItemMR.IsMaturityMMYRule_Monthly)
                {
                    //Step 1: On considère uniquement YYYYMM du nom de l'échéance saisie pour calculer une date d'échéance à partir du paramétrage en vigueur.
                    MaturityRule matRule = new MaturityRule(sqlItemMR)
                    {
                        //Forçage à YearMonthOnly pour simuler un calcul d'une date à partir d'une échéance au format YYYYMMD
                        MaturityFormatEnum = Cst.MaturityMonthYearFmtEnum.YearMonthOnly
                    };

                    CalcMaturityRuleDate calc = new CalcMaturityRuleDate(CS, _productBase, (_dataMarket.IdM, _dataMarket.IdBC), matRule);
                    DateTime dtCalcMaturity = calc.Calc_MaturityDate(GetMaturityYearMonthOnlyForCheck(sqlItemMR), out DateTime rolledDate).MaturityDateSys;
                    
                    // FI 20230126 [XXXXX] Prise en compte de MaturityRelativeTo
                    ret = false;
                    if (DtFunc.IsDateTimeFilled(dtCalcMaturity))
                    {
                        if (sqlItemMR.IsMaturityMMYRule_Monthly)
                        {
                            Cst.MaturityRelativeTo MaturityRelativeTo = matRule.MaturityRelativeTo ?? Cst.MaturityRelativeTo.EXP;

                            switch (MaturityRelativeTo)
                            {
                                case Cst.MaturityRelativeTo.EXP:
                                    ret = (dtCalcMaturity == dtMaturityMonthYear);
                                    break;
                                case Cst.MaturityRelativeTo.LTD:
                                    DateTime dtCalcLTD = calc.Calc_MaturityLastTradingDay(dtCalcMaturity, rolledDate);
                                    ret = (dtCalcLTD == dtMaturityMonthYear);
                                    break;
                            }
                        }
                        else if (sqlItemMR.IsMaturityMMYRule_Multiple_MonthlyInclude)
                        {
                            //Step 2: On compare cette date d'échéance calculée au nom de l'échéance. Si elles sont identiques, le nom de l'échéance est donc une échéance Monthly autorisée.  
                            ret = (dtCalcMaturity == dtMaturityMonthYear);
                        }
                        else
                        {
                            throw new NotImplementedException($"Invalide MonthlyRule : {sqlItemMR.Identifier}");
                        }
                    }
                }
                #endregion

                #region 2- Sinon, si la règle autorise des échéances QUATERLY, on contrôle si le nom de l'échéance saisie est compatible avec paramétrage en vigueur (ex.: CBOE - dernier jour du trimestre)
                if ((!ret) && sqlItemMR.IsMaturityMMYRule_Multiple_QuaterlyInclude)
                {
                    //Step 3: On considère uniquement YYYYMM de l'échéance pour calculer une date d'échéance à partir d'un paramétrage fictif de type EOM,
                    //        car on ne considère pour l'instant, et en dur, que des dates d'échéances "Last Business Day" (lbd) pour les échéances Quaterly.
                    //        Ce qui répond au besoin du CBOE, marché pour lequel on a réaliser ce chantier en urgence.
                    int month = dtMaturityMonthYear.Month;
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    //WARNING: TODO
                    // - Il faudrait exploiter ici les mois paramétrés (ex. "q_lbd=HMUZ")
                    // - Etonnament je découvre aujourd'hui (PL 202110121) que l'on considère en dur comme mois de trimestre possible les mois HMUZ
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    if ((month == 3) || (month == 6) || (month == 9) || (month == 12))
                    {
                        MaturityRule matRule = new MaturityRule
                        {
                            MaturityFormatEnum = Cst.MaturityMonthYearFmtEnum.YearMonthOnly,
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            //WARNING: TODO
                            // - Il faudrait exploiter ici la présence/absence du mot clé "lbd" (last business day) (ex. "q_lbd=HMUZ")
                            //   Pour l'instant on considère qu'il y a tjs "lbd" en utilisant donc en dur pour le calcul le couple EOM/PRECEDING
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            MaturityRollConv = RollConventionEnum.EOM.ToString(),
                            MaturityRollConvBusinessDayConv = BusinessDayConventionEnum.PRECEDING
                        };

                        CalcMaturityRuleDate calc = new CalcMaturityRuleDate(CS, _productBase, (_dataMarket.IdM, _dataMarket.IdBC), matRule);
                        DateTime dtCalcMaturity = calc.Calc_MaturityDate(_fixInstrument.MaturityMonthYear.Substring(0, 6), out _).MaturityDateSys;

                        //Step 4: On compare cette date d'échéance calculée à l'échéance. Si elles sont identiques, l'échéance est donc une échéance Quaterly autorisée.  
                        ret = (DtFunc.IsDateTimeFilled(dtCalcMaturity) && (dtCalcMaturity == dtMaturityMonthYear));
                        if (ret)
                        {
                            _isQuaterlyMaturity = true;
                        }
                    }
                }
                #endregion

                #region 3- Sinon, si la règle autorise des échéances WEEKLY, on contrôle si le nom de l'échéance saisie est compatible avec paramétrage en vigueur (ex.: CBOE - vendredi, sauf 3ème vendredi)
                if ((!ret) && sqlItemMR.IsMaturityMMYRule_Multiple_WeeklyInclude)
                {
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    //WARNING: TODO
                    // - Il faudrait traiter les cas différent de "w_fri"
                    // - Il faudra traiter le cas d'un vendredi férié où dans ce cas la date sera sans doute un jeudi
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    //Weekly - FRIDAY
                    if (sqlItemMR.MaturityMMYRule_Multiple_Weekly.StartsWith("w_fri") && (dtMaturityMonthYear.DayOfWeek == DayOfWeek.Friday))
                    {
                        //Récupération de la RollConv paramétrée pour les échéances Monthly, afin de ne pas autoriser d'échéance Weekly ce même jour.  
                        //NB: ce test n'est pas indispensable, car on devrait dans ce cas avoir déjà considéré cette échéance Monthly plus haut (Step 1)
                        RollConventionEnum rollConvOfMonthlyMaturity = StringToEnum.RollConvention(sqlItemMR.MaturityRollConv);

                        switch (rollConvOfMonthlyMaturity)
                        {
                            case RollConventionEnum.FIRSTFRI:
                                //Contrôle si cette échéance hebdomadaire est différent du 1er vendredi dans le cas où l'échéance mensuelle est elle le 1er vendredi du mois
                                ret = (dtMaturityMonthYear.Day > 7);
                                break;
                            case RollConventionEnum.SECONDFRI:
                                //Contrôle si cette échéance hebdomadaire est différent du 2nd vendredi dans le cas où l'échéance mensuelle est elle le 2nd vendredi du mois
                                ret = (dtMaturityMonthYear.Day < 8) || (dtMaturityMonthYear.Day > 14);
                                break;
                            case RollConventionEnum.THIRDFRI:
                                //Contrôle si cette échéance hebdomadaire est différent du 3ième vendredi dans le cas où l'échéance mensuelle est elle le 3ième vendredi du mois
                                ret = (dtMaturityMonthYear.Day < 15) || (dtMaturityMonthYear.Day > 21);
                                break;
                            case RollConventionEnum.FOURTHFRI:
                                //Contrôle si cette échéance hebdomadaire est différent du 4ième vendredi dans le cas où l'échéance mensuelle est elle le 4ième vendredi du mois
                                ret = (dtMaturityMonthYear.Day < 22) || (dtMaturityMonthYear.Day > 28);
                                break;
                            case RollConventionEnum.FIFTHFRI:
                                //Contrôle si cette échéance hebdomadaire est différent du 5ième vendredi dans le cas où l'échéance mensuelle est elle le 5ième vendredi du mois
                                ret = (dtMaturityMonthYear.Day < 29);
                                break;
                        }
                    }
                }
                #endregion
            }
            return ret;
        }

        /// <summary>
        /// Contrôle de la compatibilité de l'échéance (IFixInstrument.MaturityMonthYear) avec la MR <paramref name="sqlItemMR"/>.
        /// <para>Le format de l'échéance est YYYYMMDD. Le cycle de la MR est weekly</para>.
        /// </summary>
        /// <param name="sqlItemMR">MR nécessairement au format YYYYMMDD et de cycle weekly</param>
        /// <param name="dtRefForDtEnabled"></param>
        /// <exception cref="ArgumentException">Lorsque la MR n'est pas weekly ou format différent de YYYYMMDD</exception> 
        /// <returns></returns>
        private Boolean CheckMaturityMonthYearOnWeekly(SQL_MaturityRuleBase sqlItemMR, DateTime dtRefForDtEnabled)
        {
            if (false == (sqlItemMR.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay))
                throw new ArgumentException($"{nameof(sqlItemMR)}. Format:{sqlItemMR.MaturityFormatEnum} invalid.");

            if (false == sqlItemMR.IsMaturityMMYRule_Weekly)
                throw new ArgumentException($"{nameof(sqlItemMR)}. Cycle:{sqlItemMR.MaturityMMYRule} invalid.");

            Boolean ret = CheckMaturityMonthYearOnWeeklyBasic(sqlItemMR);
            if (ret)
            {

                // Vérification qu'il n'existe pas une MR plus "proche" que sqlItemMR. S'il existe une MR plus "proche" la fonction retourne false  
                // cas1
                // Exemple MR (sqlItemMR) avec Cycle W124, RollConvention le Lundi, BusinessDayConvention Following et lundi 04/07/2022 férié et échéance 20220705 (jour non férié)
                // --> s'il existe une MR2 avec Cycle W124, RollConvention le Mardi => la fonction retourne false (MR2 est plus proche que sqlItemMR)
                //
                // cas2
                // Exemple MR (sqlItemMR) avec Cycle W124, RollConvention le Lundi, BusinessDayConvention Following et lundi 04/07/2022, mardi 05/07/2022, mercredi 06/07/2022 fériés, vendredi 08/07/2022  et échéance 20220707 (jour non férié)
                // --> s'il existe une MR2 avec Cycle W124, RollConvention le jeudi => la fonction retourne false (MR2 est plus proche que sqlItemMR)
                // --> sinon s'il existe une MR3 avec Cycle W124, RollConvention le vendredi, BusinessDayConvention Preceding => la fonction retourne

                if (RollConventionToDayOfWeek(StringToEnum.RollConvention(sqlItemMR.MaturityRollConv), out Nullable<DayOfWeek> dayOfWeek))
                {

                    DateTime dtMaturityMonthYear = GetDateMaturityMonthYear();

                    //On ne considère uniquement les MR dont le n° de squence est > au numéro de sequence de sqlItemMR (Les MRs étant traitées par ordre de priorité) 
                    int sqlMRItemSequenceNumber = (from item in _DCMR.Where(x => x.IdMR == sqlItemMR.Id) select item.SequenceNumber).First();


                    IEnumerable<SQL_MaturityRuleActive> sqlMROther = (from item in _DCMR.Where(x => x.SequenceNumber > sqlMRItemSequenceNumber && x.MaturityFormat == Cst.MaturityMonthYearFmtEnum.YearMonthDay)
                                                                      select GetSQLMaturityRuleActive(item.IdMR, dtRefForDtEnabled)).Where(y =>
                                                                                  y.IsMaturityMMYRule_Weekly &&
                                                                                  CheckMaturityMonthYearOnWeeklyBasic(y));


                    if (sqlMROther.Count() > 0)
                    {
                        foreach (SQL_MaturityRuleActive item in sqlMROther)
                        {
                            if (RollConventionToDayOfWeek(StringToEnum.RollConvention(item.MaturityRollConv), out Nullable<DayOfWeek> dayOfWeekItemOther))
                            {
                                if (Math.Abs(dayOfWeekItemOther.Value - dtMaturityMonthYear.DayOfWeek) < Math.Abs(dayOfWeek.Value - dtMaturityMonthYear.DayOfWeek))
                                {
                                    ret = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Contrôle basique de la compatibilité de l'échéance (IFixInstrument.MaturityMonthYear) avec la MR <paramref name="sqlItemMR"/> (dont le cycle est nécessairement weekly)
        /// <para>Exemple  : MR avec Cycle W124, RollConvention le Lundi, BusinessDayConvention Following => Retourne true si l'échéance est 20220704 (1er lundi de juillet 2022)</para>
        /// <para>Exemple2 : MR avec Cycle W124, RollConvention le Lundi, BusinessDayConvention Following => Retourne true si l'échéance est 20220705 et si le lundi 04/07/2022 est férié</para>
        /// </summary>
        /// <param name="sqlItemMR"></param>
        /// <returns></returns>
        private Boolean CheckMaturityMonthYearOnWeeklyBasic(SQL_MaturityRuleBase sqlItemMR)
        {
            Nullable<DayOfWeek> dayOfWeek = null;

            Boolean ret = sqlItemMR.MaturityRollConvEnum.HasValue &&
                RollConventionToDayOfWeek(StringToEnum.RollConvention(sqlItemMR.MaturityRollConv), out dayOfWeek);

            if (ret)
            {
                string DDD = ReflectionTools.ConvertEnumToString<DayOfWeekEnum>((DayOfWeekEnum)System.Enum.Parse(typeof(DayOfWeekEnum), dayOfWeek.ToString()));

                DateTime dtMaturityMonthYear = GetDateMaturityMonthYear();

                int[] availableWeekNumber = GetAvailableWeekNumber(sqlItemMR);
                if (ArrFunc.IsEmpty(availableWeekNumber))
                    availableWeekNumber = new int[] { 1, 2, 3, 4, 5 };

                string MaturityRollConv;
                string ordinalIndicator;
                foreach (int item in availableWeekNumber)
                {
                    ordinalIndicator = string.Empty;
                    switch (item)
                    {
                        case 1:
                            ordinalIndicator = "ST";
                            break;
                        case 2:
                            ordinalIndicator = "ND";
                            break;
                        case 3:
                            ordinalIndicator = "RD";
                            break;
                        case 4:
                        case 5:
                            ordinalIndicator = "TH";
                            break;
                    }

                    MaturityRollConv = String.Empty;
                    if (StrFunc.IsFilled(ordinalIndicator))
                        MaturityRollConv = $"{item}{ordinalIndicator}{DDD}";

                    if (StrFunc.IsEmpty(MaturityRollConv))
                        throw new InvalidOperationException("MaturityRollConv is empty");

                    //Step 1: On considère uniquement YYYYMM du nom de l'échéance saisie pour calculer une date d'échéance à partir du paramétrage en vigueur.
                    MaturityRule matRule = new MaturityRule(sqlItemMR)
                    {
                        //Astuce: forçage à YearMonthOnly pour simuler un calcul d'une date à partir d'une échéance au format YYYYMMD
                        MaturityFormatEnum = Cst.MaturityMonthYearFmtEnum.YearMonthOnly,
                        MaturityRollConv = MaturityRollConv
                    };

                    CalcMaturityRuleDate calc = new CalcMaturityRuleDate(CS, _productBase, (_dataMarket.IdM, _dataMarket.IdBC), matRule);
                    DateTime dtCalc = calc.Calc_MaturityDate(GetMaturityYearMonthOnlyForCheck(sqlItemMR), out DateTime rolledDate).MaturityDateSys;

                    // FI 20230126 [XXXXX] Prise en compte de MaturityRelativeTo
                    ret = false;
                    if (DtFunc.IsDateTimeFilled(dtCalc))
                    {
                        Cst.MaturityRelativeTo MaturityRelativeTo = matRule.MaturityRelativeTo ?? Cst.MaturityRelativeTo.EXP;

                        switch (MaturityRelativeTo)
                        {
                            case Cst.MaturityRelativeTo.EXP:
                                ret = (dtCalc == dtMaturityMonthYear);
                                break;
                            case Cst.MaturityRelativeTo.LTD:
                                dtCalc = calc.Calc_MaturityLastTradingDay(dtCalc, rolledDate);
                                ret = (dtCalc == dtMaturityMonthYear);
                                break;
                        }
                    }

                    if (ret || dtCalc.CompareTo(dtMaturityMonthYear) > 0)
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les semaines autorisées sur <paramref name="sqlItemMR"/> nécessairement de cycle Weekly
        /// <para>Retourne un tableau vide si toutes les semaines sont autorisées</para>
        /// </summary>
        /// <param name="sqlItemMR"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        private static int[] GetAvailableWeekNumber(SQL_MaturityRuleBase sqlItemMR)
        {
            if (null == sqlItemMR)
                throw new ArgumentNullException(nameof(sqlItemMR));

            if (false == sqlItemMR.IsMaturityMMYRule_Weekly)
                throw new ArgumentException($"{nameof(sqlItemMR)} must be Weekly");

            Regex regex = new Regex("^w([0-5]*)$");
            if (false == regex.IsMatch(sqlItemMR.MaturityMMYRule))
                throw new NotSupportedException($"Rule: {sqlItemMR.MaturityMMYRule} is not supported");

            // availableWeekNumber contient semaines autorisées sous forme de tableau de int
            // Exemple W124 retourne le tableau de int [1,2,4]
            int[] availableWeekNumber = ArrFunc.Map<Char, int>(regex.Matches(sqlItemMR.MaturityMMYRule)[0].Groups[1].Value.ToCharArray(), (x) => { return Convert.ToInt32(x.ToString()); });

            return availableWeekNumber;
        }

        /// <summary>
        /// Convertie <paramref name="maturityRollConv"/> en <see cref="System.DayOfWeek"/> si possible (les samedis et dimanches sont exclus)
        /// <para>Si la convertion n'est pas effectuée retourne false (<paramref name="dayOfWeek"/> retourne null)</para>
        /// </summary>
        /// <param name="maturityRollConv"></param>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        private static Boolean RollConventionToDayOfWeek(RollConventionEnum maturityRollConv, out Nullable<DayOfWeek> dayOfWeek)
        {
            dayOfWeek = null;
            bool ret;

            switch (maturityRollConv)
            {
                case RollConventionEnum.MON:
                    dayOfWeek = DayOfWeek.Monday;
                    ret = true;
                    break;
                case RollConventionEnum.TUE:
                    dayOfWeek = DayOfWeek.Tuesday;
                    ret = true;
                    break;
                case RollConventionEnum.WED:
                    dayOfWeek = DayOfWeek.Wednesday;
                    ret = true;
                    break;
                case RollConventionEnum.THU:
                    dayOfWeek = DayOfWeek.Thursday;
                    ret = true;
                    break;
                case RollConventionEnum.FRI:
                    dayOfWeek = DayOfWeek.Friday;
                    ret = true;
                    break;
                default:
                    ret = false;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Retourne les différents formats autorisés par les MR(s) rattachées au DC
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Cst.MaturityMonthYearFmtEnum> GetDCMRDistinctFormat()
        {
            return (from item in _DCMR select item.MaturityFormat).Distinct();
        }

        /// <summary>
        /// Recherche la règle de maturité active en <paramref name="pDtRefForDataEnabled"/>
        /// </summary>
        /// <param name="pIdMR"></param>
        /// <param name="pDtRefForDtEnabled"></param>
        /// <exception cref="InvalidOperationException">S'il n'exist pas de MR active</exception>
        /// <returns></returns>
        private SQL_MaturityRuleActive GetSQLMaturityRuleActive(int pIdMR, DateTime pDtRefForDtEnabled)
        {
            SQL_MaturityRuleActive sqlItemMR = new SQL_MaturityRuleActive(CSTools.SetCacheOn(CS), pIdMR, pDtRefForDtEnabled)
            {
                DbTransaction = DbTransaction
            };

            if (false == sqlItemMR.IsFound)
                throw new InvalidOperationException($"Enabled Maturity Rule not find (Id: {pIdMR}, Date: {DtFunc.DateTimeToStringDateISO(pDtRefForDtEnabled)})");

            return sqlItemMR;
        }

        /// <summary>
        /// Retourne l'échéance échéance _fixInstrument.MaturityMonthYear (nécessairement au format YYYYMMDD) sous forme de date
        /// </summary>
        /// <exception cref="InvalidOperationException">si YYYYMMDD ne forme pas une date valide</exception>
        /// <returns></returns>
        private DateTime GetDateMaturityMonthYear()
        {
            if (false == StrFunc.IsDate(_fixInstrument.MaturityMonthYear, DtFunc.FmtDateyyyyMMdd, null, out DateTime ret))
                throw new InvalidOperationException($"{_fixInstrument.MaturityMonthYear} is not a yyyyMMdd date");
            return ret;
        }

        /// <summary>
        /// Recherche des maturités actives en <paramref name="dtEnabled"/> et associes à la liste de MR <paramref name="idMR"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="idMR"></param>
        /// <param name="dtEnabled"></param>
        /// <returns></returns>
        /// FI 20230217 [XXXXX][WI560] add Method
        private static IEnumerable<(int idMaturityRule, String maturityMonthYear)> LoadMaturity(string cs, IEnumerable<int> idMR, DateTime dtEnabled)
        {
            if (idMR == null)
                throw new ArgumentNullException(nameof(idMR));

            if (idMR.Count() == 0)
                throw new ArgumentException("idMR is Empty", nameof(idMR));

            string query = $@"select mat.IDMATURITYRULE, mat.MATURITYMONTHYEAR
from dbo.MATURITY mat
where 
({DataHelper.SQLColumnIn(cs, "mat.IDMATURITYRULE", idMR.ToArray(), TypeData.TypeDataEnum.integer)})
and
({OTCmlHelper.GetSQLDataDtEnabled(cs, "mat", dtEnabled)})";


            List<MapDataReaderRow> row = new List<MapDataReaderRow>();
            QueryParameters qry = new QueryParameters(cs, query, new DataParameters());

            using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                row = DataReaderExtension.DataReaderMapToList(dr);
            }

            IEnumerable<(int idMaturityRule, String maturityMonthYear)> ret = from item in row
                                                                              select (Convert.ToInt32(item["IDMATURITYRULE"].Value), Convert.ToString(item["MATURITYMONTHYEAR"].Value));


            return ret;
        }


        /// <summary>
        /// Retourne une échéance au format YYYYMM afin de contrôler l'échéance <see cref="_fixInstrument.MaturityMonthYear"/> dont le format est nécessairement YYYYMMDD
        /// </summary>
        /// <param name="sqlItemMR"></param>
        /// <returns></returns>
        private string GetMaturityYearMonthOnlyForCheck(SQL_MaturityRuleBase sqlItemMR)
        {
            if (false == (sqlItemMR.MaturityFormatEnum == Cst.MaturityMonthYearFmtEnum.YearMonthDay))
                throw new ArgumentException($"{nameof(sqlItemMR)}. Format:{sqlItemMR.MaturityFormatEnum} invalid.");

            DateTime dtMaturityMonthYear = GetDateMaturityMonthYear();

            if (MaturityRuleHelper.IsMonthOffset((sqlItemMR.MaturityRollConv, sqlItemMR.MaturityMonthReference)))
                dtMaturityMonthYear = MaturityRuleHelper.ApplyMonthOffsetReverse(dtMaturityMonthYear, (sqlItemMR.MaturityRollConv, sqlItemMR.MaturityMonthReference));


            CalcMaturityRuleDate calc = new CalcMaturityRuleDate(CS, _productBase, (_dataMarket.IdM, _dataMarket.IdBC), new MaturityRule(sqlItemMR));
            dtMaturityMonthYear = calc.Calc_Offset(dtMaturityMonthYear, true);


            return dtMaturityMonthYear.ToString("yyyyMM");
        }
        #endregion
    }
}
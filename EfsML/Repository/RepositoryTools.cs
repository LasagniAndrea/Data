using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EfsML.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class RepositoryTools
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pEnumsCode"></param>
        /// <param name="pEnumValue"></param>
        /// FI 20170206 [21916] Modify
        public static void AddEnumRepository(string pCs, IRepository pRepository, string pEnumsCode, string pEnumValue)
        {

            if (StrFunc.IsFilled(pEnumsCode) && StrFunc.IsFilled(pEnumValue))
            {
                string extCodeEnums = string.Empty;
                string extValueEnum = string.Empty;
                string extAttrbEnum = string.Empty;
                //
                EnumTools.GetEnumInfos(pCs, pEnumsCode, pEnumValue, ref extCodeEnums, ref extValueEnum, ref extAttrbEnum);
                //
                string enumsHref = "ENUMS.CODE." + pEnumsCode;
                string enumHref = "ENUM.VALUE." + pEnumValue;
                //
                IEnumsRepository enumsRepository = GetEnumsFromRepository(pRepository.Enums, enumsHref);
                //
                if (null == enumsRepository)
                {
                    #region Create new Enums
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.Enums))
                        pos = pRepository.Enums.Length;
                    //
                    ReflectionTools.AddItemInArray(pRepository, "enums", pos);
                    enumsRepository = pRepository.Enums[pos];
                    enumsRepository.Id = enumsHref;
                    enumsRepository.Code = pEnumsCode;
                    enumsRepository.ExtCode = extCodeEnums;
                    #endregion
                }
                //
                if (false == IsEnumExist(enumsRepository.EnumsDet, enumHref))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(enumsRepository.EnumsDet))
                        pos = enumsRepository.EnumsDet.Length;
                    //
                    ReflectionTools.AddItemInArray(enumsRepository, "enumsDet", pos);
                    IEnumRepository enumRepository = enumsRepository.EnumsDet[pos];
                    //
                    enumRepository.Id = enumHref;
                    enumRepository.Value = pEnumValue;
                    enumRepository.ExtValue = extValueEnum;
                    // FI 20170206 [21916] alimentation de extAttrbSpecified
                    enumRepository.ExtAttrbSpecified = StrFunc.IsFilled(extAttrbEnum);
                    if (enumRepository.ExtAttrbSpecified)
                    enumRepository.ExtAttrb = extAttrbEnum;
                }
            }
        }
        /// <summary>
        /// Ajoute le tzdbId (NodaTime.MapZone) dans EnumRepository
        /// </summary>
        /// EG 20170929 [23450][22374] New (Remplace TimeZoneLocation Enum)
        public static void AddMapZoneRepository(IRepository pRepository, string pTzdbId)
        {

            if (StrFunc.IsFilled(pTzdbId))
            {
                string enumsHref = "ENUMS.CODE.TimezoneLocation";
                string enumHref = "ENUM.VALUE." + pTzdbId;

                IEnumsRepository enumsRepository = GetEnumsFromRepository(pRepository.Enums, enumsHref);

                if (null == enumsRepository)
                {
                    #region Create new Enums
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.Enums))
                        pos = pRepository.Enums.Length;
                    //
                    ReflectionTools.AddItemInArray(pRepository, "enums", pos);
                    enumsRepository = pRepository.Enums[pos];
                    enumsRepository.Id = enumsHref;
                    enumsRepository.Code = pTzdbId;
                    enumsRepository.ExtCode = enumsHref;
                    #endregion
                }

                if (false == IsEnumExist(enumsRepository.EnumsDet, enumHref))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(enumsRepository.EnumsDet))
                        pos = enumsRepository.EnumsDet.Length;

                    ReflectionTools.AddItemInArray(enumsRepository, "enumsDet", pos);
                    IEnumRepository enumRepository = enumsRepository.EnumsDet[pos];

                    enumRepository.Id = enumHref;
                    enumRepository.Value = pTzdbId;
                    enumRepository.ExtValue = enumHref;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumsRepository"></param>
        /// <param name="pHref"></param>
        /// <returns></returns>
        private static IEnumsRepository GetEnumsFromRepository(IEnumsRepository[] pEnumsRepository, string pHref)
        {
            IEnumsRepository ret = null;
            //
            if (ArrFunc.IsFilled(pEnumsRepository))
            {
                foreach (IEnumsRepository item in pEnumsRepository)
                {
                    if (item.Id == pHref)
                    {
                        ret = item;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExtendDetRepository"></param>
        /// <param name="pHref"></param>
        /// <returns></returns>
        private static bool IsEnumExist(IEnumRepository[] pExtendDetRepository, string pHref)
        {
            bool ret = false;
            //
            if (ArrFunc.IsFilled(pExtendDetRepository))
            {
                foreach (IEnumRepository item in pExtendDetRepository)
                {
                    if (item.Id == pHref)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }


        /// <summary>
        /// Ajoute dans {pRepository} le book {pIdB}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pBookId"></param>
        /// FI 20150306 [XXXXX] Modify
        public static void AddBookRepository(string pCs, IRepository pRepository, int pIdB)
        {
            if (pIdB > 0)
            {
                SQL_Book sql_Book = new SQL_Book(pCs, pIdB);
                //
                if (sql_Book.IsFound)
                {
                    string href = "BOOK.IDB." + sql_Book.Id.ToString();
                    //
                    if (false == IsRepositoryExist(pRepository.Book, href))
                    {
                        int pos = 0;
                        if (ArrFunc.IsFilled(pRepository.Book))
                            pos = pRepository.Book.Length;

                        ReflectionTools.AddItemInArray(pRepository, "book", pos);
                        IBookRepository item = pRepository.Book[pos];

                        item.OTCmlId = sql_Book.Id;
                        item.Id = href;
                        item.Identifier = sql_Book.Identifier;
                        item.Displayname = sql_Book.DisplayName;

                        item.Description = sql_Book.Description;
                        item.DescriptionSpecified = StrFunc.IsFilled(sql_Book.Description);

                        item.Extllink = sql_Book.ExtlLink;
                        item.ExtllinkSpecified = StrFunc.IsFilled(sql_Book.ExtlLink);

                        //FI 20150306 [XXXXX] Aliemntation de l'acteur propriétaire
                        //FI 20150310 [XXXXX] Suppression de l'alimentation de item.owner.OTCmlId
                        //item.owner.OTCmlId = sql_Book.IdA;
                        item.Owner.HRef = "ACTOR.IDA." + sql_Book.IdA;
                        AddActorRepository(pCs, pRepository, sql_Book.IdA);

                        item.IdC = sql_Book.IdC;
                        item.IdCSpecified = StrFunc.IsFilled(sql_Book.IdC);
                    }
                }
            }
        }

        /// <summary>
        /// Alimenation de {pRepository} avec l'instrument {pIdI}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pIdI"></param>
        /// <returns></returns>
        /// FI 20150218 [20275] Add
        public static void AddInstrumentRepository(string pCs, IRepository pRepository, int pIdI)
        {
            if (pIdI > 0)
            {
                SQL_Instrument sql = new SQL_Instrument(pCs, pIdI);
                if (sql.IsFound)
                    AddInstrumentRepository(pRepository, sql);
            }
        }
        /// <summary>
        /// Alimenation de {pRepository} avec l'instrument {pSql_Instr}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pSql_Instr"></param>
        /// <returns></returns>
        /// FI 20150218 [20275] Add
        public static void AddInstrumentRepository(IRepository pRepository, SQL_Instrument pSql_Instr)
        {

            string href_pref = "INSTRUMENT.IDI.";

            if (pSql_Instr.IsLoaded)
            {
                string href = href_pref + pSql_Instr.Id.ToString();
                if (false == IsRepositoryExist(pRepository.Instrument, href))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.Instrument))
                        pos = pRepository.Instrument.Length;

                    ReflectionTools.AddItemInArray(pRepository, "instrument", pos);
                    IInstrumentRepository item = pRepository.Instrument[pos];

                    item.OTCmlId = pSql_Instr.Id;
                    item.Id = href;
                    item.Identifier = pSql_Instr.Identifier;
                    item.Displayname = pSql_Instr.DisplayName;
                    item.Description = pSql_Instr.Description;
                    item.DescriptionSpecified = StrFunc.IsFilled(pSql_Instr.Description);
                    item.Extllink = pSql_Instr.ExtlLink;
                    item.ExtllinkSpecified = StrFunc.IsFilled(pSql_Instr.ExtlLink);

                    item.GProduct = pSql_Instr.GProduct;
                    item.Product = pSql_Instr.Product_Identifier;
                    item.IsMargining = pSql_Instr.IsMargining;
                    item.IsFunding = pSql_Instr.IsFunding; 
                }
            }
        }

        /// <summary>
        /// Alimentation de {pRepository} avec l'acteur {pIdA}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pIdI"></param>
        /// <returns></returns>
        /// FI 20150306 [XXXXX] Add
        public static void AddActorRepository(string pCs, IRepository pRepository, int pIdA)
        {
            if (pIdA > 0)
            {
                SQL_Actor sql = new SQL_Actor(pCs, pIdA);
                if (sql.IsFound)
                    AddActorRepository(pRepository, sql);
            }
        }
        /// <summary>
        /// Alimentation de {pRepository} avec l'instrument {pSql_Instr}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pSql_Instr"></param>
        /// <returns></returns>
        /// FI 20150306 [XXXXX] Add
        /// FI 20160530 [21885] Modify
        public static void AddActorRepository(IRepository pRepository, SQL_Actor pSql_Actor)
        {

            string href_pref = "ACTOR.IDA.";

            if (pSql_Actor.IsLoaded)
            {
                string href = href_pref + pSql_Actor.Id.ToString();
                if (false == IsRepositoryExist(pRepository.Actor, href))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.Actor))
                        pos = pRepository.Actor.Length;

                    ReflectionTools.AddItemInArray(pRepository, "actor", pos);
                    IActorRepository item = pRepository.Actor[pos];

                    item.OTCmlId = pSql_Actor.Id;
                    item.Id = href;
                    item.Identifier = pSql_Actor.Identifier;
                    item.Displayname = pSql_Actor.DisplayName;
                    item.Description = pSql_Actor.Description;
                    item.DescriptionSpecified = StrFunc.IsFilled(pSql_Actor.Description);
                    item.Extllink = pSql_Actor.ExtlLink;
                    item.ExtllinkSpecified = StrFunc.IsFilled(pSql_Actor.ExtlLink);
                    // FI 20160530 [21885] add code MIC
                    item.ISO10383_ALPHA4 = pSql_Actor.ISO10383_ALPHA4;
                    item.ISO10383_ALPHA4Specified = StrFunc.IsFilled(pSql_Actor.ISO10383_ALPHA4);
                    // FI 20190515 [23912] Add
                    item.ISO17442 = pSql_Actor.ISO17442;
                    item.ISO17442Specified = StrFunc.IsFilled(pSql_Actor.ISO17442);
                }
            }
        }


        /// <summary>
        /// Ajoute le marché du référentiel Spheres® {pIdM}  dans {pRepository} s'il n'y est pas présent 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRepository"></param>
        /// <param name="pExchange"></param>
        /// FI 20140820 [20275] Add Method 
        public static void AddMarketRepository(string pCS, IRepository pRepository, int pIdM)
        {
            if (pIdM > 0)
            {
                SQL_Market sqlMarket = new SQL_Market(pCS, pIdM, SQL_Table.ScanDataDtEnabledEnum.No);
                if (sqlMarket.LoadTable())
                    AddMarketRepository(pRepository, sqlMarket);
            }
        }

        /// <summary>
        /// Ajoute le marché du référentiel Spheres® {pSql_Market}  dans {pRepository} s'il n'y est pas présent 
        /// </summary>
        /// <param name="pRepository"></param>
        /// <param name="pSqlMarket"></param>
        // FI 20120803 [18009]
        // RD 20140627 [20147] Tous les flux XML véhiculent FIXML_SECURITYEXCHANGE du marché, il faut donc rajouter cette info dans MarketRepository
        // PL 20181001 [24211] RICCODE/BBGCODE
        public static void AddMarketRepository(IRepository pRepository, SQL_Market pSql_Market)
        {
            SQL_Market sql_Market = pSql_Market;

            if (sql_Market.IsLoaded)
            {
                string href = "MARKET.IDM." + sql_Market.Id.ToString();
                if (false == IsRepositoryExist(pRepository.Market, href))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.Market))
                        pos = pRepository.Market.Length;
                    //
                    ReflectionTools.AddItemInArray(pRepository, "market", pos);
                    IMarketRepository item = pRepository.Market[pos];
                    //
                    item.OTCmlId = sql_Market.Id;
                    item.Id = href;
                    item.Identifier = sql_Market.Identifier;
                    item.Displayname = sql_Market.DisplayName;
                    //
                    item.Description = sql_Market.Description;
                    item.DescriptionSpecified = StrFunc.IsFilled(sql_Market.Description);
                    //
                    item.Extllink = sql_Market.ExtlLink;
                    item.ExtllinkSpecified = StrFunc.IsFilled(sql_Market.ExtlLink);
                    //
                    item.ISO10383_ALPHA4 = sql_Market.ISO10383_ALPHA4;
                    item.ISO10383_ALPHA4Specified = StrFunc.IsFilled(sql_Market.ISO10383_ALPHA4);
                    //
                    item.Acronym = sql_Market.Acronym;
                    item.AcronymSpecified = StrFunc.IsFilled(sql_Market.Acronym);
                    //
                    item.City = sql_Market.City;
                    item.CitySpecified = StrFunc.IsFilled(sql_Market.City);
                    //
                    item.ExchangeSymbol = sql_Market.ExchangeSymbol;
                    item.ExchangeSymbolSpecified = StrFunc.IsFilled(sql_Market.ExchangeSymbol);
                    //
                    item.ShortIdentifier = sql_Market.ShortIdentifier;
                    item.ShortIdentifierSpecified = StrFunc.IsFilled(sql_Market.ShortIdentifier);
                    //
                    item.Fixml_SecurityExchange = sql_Market.FIXML_SecurityExchange;
                    item.Fixml_SecurityExchangeSpecified = StrFunc.IsFilled(sql_Market.FIXML_SecurityExchange);
                    //
                    item.RICCode = sql_Market.RICCode;
                    item.RICCodeSpecified = StrFunc.IsFilled(sql_Market.RICCode);
                    //
                    item.BBGCode = sql_Market.BBGCode;
                    item.BBGCodeSpecified = StrFunc.IsFilled(sql_Market.BBGCode);

                    item.ETDIdentifierFormat = sql_Market.ETDIdentifierFormat;
                    item.ETDIdentifierFormatSpecified = StrFunc.IsFilled(sql_Market.ETDIdentifierFormat);

                }
            }
        }

        /// <summary>
        /// Alimente {pRepository} avec l'asset {pAsset} s'il est non présent
        /// <para>○ Retourne l'élément ICommonRepository s'il a été ajouté</para>
        /// <para>○ Retourne null si l'asset n'a pas été ajouté</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pAsset">couple {Cst.UnderlyingAsset,Id} qui représent l'asset. Id doit être sup à zéro</param>
        // FI 20140820 [20275] Add Method 
        // EG 20150222 Add SQL_AssetFxRate
        // FI 20151019 [21317] Modify
        // FI 20160530 [21885] Modify
        // FI 20161214 [21916] Modify
        // FI 20170116 [21916] Modify
        public static IAssetRepository AddAssetRepository(string pCs, IRepository pRepository, Pair<Cst.UnderlyingAsset, int> pAsset)
        {
            if (pAsset.Second == 0)
                throw new ArgumentException("Asset is not valid (OTCmlId==0)");

            // FI 20170116 [21916] Utilisation de la méthode AssetTools.NewSQLAsset
            SQL_AssetBase sql_Asset = AssetTools.NewSQLAsset(pCs, pAsset.First, pAsset.Second);

            if (false == sql_Asset.LoadTable())
                throw new Exception(StrFunc.AppendFormat("Asset (assetCategoty:{0},id:{1}) is not loaded", pAsset.First.ToString(), pAsset.Second.ToString()));

            return AddAssetRepository(pCs, pRepository, sql_Asset);
        }

        /// <summary>
        /// Alimente {pRepository} avec l'asset {sql_Asset} s'il est non présent
        /// <para>○ Retourne l'élément ICommonRepository s'il a été ajouté</para>
        /// <para>○ Retourne null si l'asset n'a pas été ajouté</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="sql_Asset"></param>
        // FI 20140820 [20275] Add Method 
        // FI 20150218 [20275] Modify
        // FI 20150315 [XXPOC] Modify
        // FI 20150513 [XXXXX] Modify
        // FI 20150522 [20275] Modify
        // FI 20151019 [21317] Modify
        // FI 20160126 [21825] Modify
        // FI 20160530 [21885] Modify
        // FI 20161214 [21916] Modify
        // FI 20170116 [21916] Modify
        // FI 20170206 [21916] Modify
        // PL 20181001 [24213] RICCODE/BBGCODE
        public static IAssetRepository AddAssetRepository(string pCs, IRepository pRepository, SQL_AssetBase sql_Asset)
        {
            IAssetRepository ret = null;

            if (false == (sql_Asset.IsLoaded))
                throw new ArgumentException("sql_asset is not loaded");
            IAssetRepository[] assetRepository;
            string element;
            string key;
            if (sql_Asset is SQL_AssetEquity)
            {
                assetRepository = pRepository.AssetEquity;
                element = "assetEquity";
                key = "ASSET_EQUITY.IDASSET";
            }
            else if (sql_Asset is SQL_AssetIndex)
            {
                assetRepository = pRepository.AssetIndex;
                element = "assetIndex";
                key = "ASSET_INDEX.IDASSET";
            }
            else if (sql_Asset is SQL_AssetRateIndex)
            {
                assetRepository = pRepository.AssetIndex;
                element = "assetRateIndex";
                key = "RATEINDEX.IDRX"; // DOMMAGE => ASSET_RATEINDEX.IDASSET autait-été préférable (Conserver pour cause de compatibilité ascendante)
            }
            else if (sql_Asset is SQL_AssetETD)
            {
                assetRepository = pRepository.AssetETD;
                element = "assetETD";
                key = "ASSET_ETD.IDASSET";
            }
            else if (sql_Asset is SQL_AssetFxRate)
            {
                assetRepository = pRepository.AssetFxRate;
                element = "assetFxRate";
                key = "ASSET_FXRATE.IDASSET";
            }
            else if (sql_Asset is SQL_AssetDebtSecurity) // FI 20151019 [21317] 
            {
                assetRepository = pRepository.AssetDebtSecurity;
                element = "assetDebtSecurity";
                key = "ASSET_DEBTSECURITY.IDASSET";
            }
            else if (sql_Asset is SQL_AssetCash) // FI 20160530 [21885] Add
            {
                assetRepository = pRepository.AssetCash;
                element = "assetCash";
                key = "ASSET_CASH.IDASSET";
            }
            else if (sql_Asset is SQL_AssetCommodity) // FI 20160530 [21885] Add
            {
                assetRepository = pRepository.AssetCommodity;
                element = "assetCommodity";
                key = "ASSET_COMMODITY.IDASSET";
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemeted", sql_Asset.GetType().ToString()));

            string href = StrFunc.AppendFormat("{0}.{1}", key, sql_Asset.Id.ToString());
            if ((assetRepository == null) || false == IsRepositoryExist(assetRepository, href))
            {
                int pos = 0;
                if (ArrFunc.IsFilled(assetRepository))
                    pos = assetRepository.Length;

                ReflectionTools.AddItemInArray(pRepository, element, pos);
                assetRepository = (IAssetRepository[])ReflectionTools.GetElementByName(pRepository, element);

                ret = assetRepository[pos];

                ret.OTCmlId = sql_Asset.Id;
                ret.Id = href;
                ret.Identifier = sql_Asset.Identifier;
                ret.Displayname = sql_Asset.DisplayName;
                ret.Description = sql_Asset.Description;
                ret.DescriptionSpecified = StrFunc.IsFilled(sql_Asset.Description);
                ret.Extllink = sql_Asset.ExtlLink;
                ret.ExtllinkSpecified = StrFunc.IsFilled(sql_Asset.ExtlLink);

                // FI 20150513 [XXXXX] Alimentation de asset
                ret.AltIdentifier = BuildAssetAltIdentifier(sql_Asset, element, false);
                ret.AltIdentifierSpecified = StrFunc.IsFilled(ret.AltIdentifier);


                // FI 20150513 [XXXXX] Alimentation de IdM
                ret.IdMSpecified = sql_Asset.IdM > 0;
                if (ret.IdMSpecified)
                    ret.IdM = sql_Asset.IdM;

                // FI 20150513 [XXXXX] Alimentation de IdC
                ret.IdCSpecified = StrFunc.IsFilled(sql_Asset.IdC);
                if (ret.IdCSpecified)
                    ret.IdC = sql_Asset.IdC;

                switch (element)
                {
                    case "assetEquity":
                        IAssetEquityRepository assetEquityRepository = (IAssetEquityRepository)ret;
                        SQL_AssetEquity sqlAssetEquity = (SQL_AssetEquity)sql_Asset;

                        assetEquityRepository.AssetSymbolSpecified = StrFunc.IsFilled(sqlAssetEquity.AssetSymbol);
                        if (assetEquityRepository.AssetSymbolSpecified)
                            assetEquityRepository.AssetSymbol = sqlAssetEquity.AssetSymbol;

                        assetEquityRepository.BBGCodeSpecified = StrFunc.IsFilled(sqlAssetEquity.BBGCode);
                        if (assetEquityRepository.BBGCodeSpecified)
                            assetEquityRepository.BBGCode = sqlAssetEquity.BBGCode;

                        assetEquityRepository.RICCodeSpecified = StrFunc.IsFilled(sqlAssetEquity.RICCode);
                        if (assetEquityRepository.RICCodeSpecified)
                            assetEquityRepository.RICCode = sqlAssetEquity.RICCode;

                        assetEquityRepository.ISINCodeSpecified = StrFunc.IsFilled(sqlAssetEquity.ISINCode);
                        if (assetEquityRepository.ISINCodeSpecified)
                            assetEquityRepository.ISINCode = sqlAssetEquity.ISINCode;

                        assetEquityRepository.CFICodeSpecified = StrFunc.IsFilled(sqlAssetEquity.CFICode);
                        if (assetEquityRepository.CFICodeSpecified)
                            assetEquityRepository.CFICode = sqlAssetEquity.CFICode;
                        break;

                    case "assetDebtSecurity": // FI 20151019 [21317] add 
                        IAssetDebtSecurityRepository assetDebtSecurityRepository = (IAssetDebtSecurityRepository)ret;
                        SQL_AssetDebtSecurity sqlAssetDebtSecurity = (SQL_AssetDebtSecurity)sql_Asset;

                        assetDebtSecurityRepository.BBGCodeSpecified = StrFunc.IsFilled(sqlAssetDebtSecurity.BBGCode);
                        if (assetDebtSecurityRepository.BBGCodeSpecified)
                            assetDebtSecurityRepository.BBGCode = sqlAssetDebtSecurity.BBGCode;

                        assetDebtSecurityRepository.RICCodeSpecified = StrFunc.IsFilled(sqlAssetDebtSecurity.RICCode);
                        if (assetDebtSecurityRepository.RICCodeSpecified)
                            assetDebtSecurityRepository.RICCode = sqlAssetDebtSecurity.RICCode;

                        assetDebtSecurityRepository.ISINCodeSpecified = StrFunc.IsFilled(sqlAssetDebtSecurity.ISINCode);
                        if (assetDebtSecurityRepository.ISINCodeSpecified)
                            assetDebtSecurityRepository.ISINCode = sqlAssetDebtSecurity.ISINCode;

                        assetDebtSecurityRepository.SEDOLCodeSpecified = StrFunc.IsFilled(sqlAssetDebtSecurity.SEDOLCode);
                        if (assetDebtSecurityRepository.SEDOLCodeSpecified)
                            assetDebtSecurityRepository.SEDOLCode = sqlAssetDebtSecurity.SEDOLCode;

                        assetDebtSecurityRepository.ParValueSpecified = (null != sqlAssetDebtSecurity.ParValue);
                        if (assetDebtSecurityRepository.ParValueSpecified)
                            assetDebtSecurityRepository.ParValue = new EfsML.Notification.ReportAmount()
                            {
                                currency = sqlAssetDebtSecurity.ParValueIdC,
                                amount = sqlAssetDebtSecurity.ParValue.Value
                            };

                        break;

                    case "assetETD":
                        IAssetETDRepository assetETDRepository = (IAssetETDRepository)ret;
                        SQL_AssetETD AssetETD = (SQL_AssetETD)sql_Asset;

                        assetETDRepository.IdDC = AssetETD.IdDerivativeContract;

                        assetETDRepository.AssetSymbol = AssetETD.AssetSymbol;
                        assetETDRepository.AssetSymbolSpecified = StrFunc.IsFilled(AssetETD.AssetSymbol);

                        assetETDRepository.ISINCode = AssetETD.ISINCode;
                        assetETDRepository.ISINCodeSpecified = StrFunc.IsFilled(AssetETD.ISINCode);

                        assetETDRepository.RICCode = AssetETD.RICCode;
                        assetETDRepository.RICCodeSpecified = StrFunc.IsFilled(AssetETD.RICCode);

                        assetETDRepository.BBGCode = AssetETD.BBGCode;
                        assetETDRepository.BBGCodeSpecified = StrFunc.IsFilled(AssetETD.BBGCode);

                        assetETDRepository.CFICode = AssetETD.CFICode;
                        assetETDRepository.CFICodeSpecified = StrFunc.IsFilled(AssetETD.CFICode);

                        assetETDRepository.Aii = AssetETD.AII;
                        assetETDRepository.AiiSpecified = StrFunc.IsFilled(AssetETD.AII);

                        // FI 20160126 [21825] Lecture de la property OriginalContractMultiplier  
                        assetETDRepository.ContractMultiplier = AssetETD.OriginalContractMultiplier;
                        assetETDRepository.ContractMultiplierSpecified = (0 < AssetETD.OriginalContractMultiplier);

                        // FI 20150218 [20275] alimentation de price et fmtprice pour le strike
                        assetETDRepository.StrikePriceSpecified = (0 < AssetETD.StrikePrice);
                        if (assetETDRepository.StrikePriceSpecified)
                        {
                            SQL_DerivativeContract sql_DerivativeContract = new SQL_DerivativeContract(pCs, AssetETD.IdDerivativeContract);
                            if (false == sql_DerivativeContract.LoadTable(new string[] { "STRIKENUMERICBASE", "INSTRUMENTDEN", "STRIKEMULTIPLIER", "STRIKEFORMATSTYLE" }))
                                throw new NotSupportedException(StrFunc.AppendFormat("Derivative contrat (id:{0}) doesn't exist", AssetETD.IdDerivativeContract.ToString()));

                            assetETDRepository.StrikePrice = new RepositoryPrice()
                            {
                                price = AssetETD.StrikePrice,
                                fmtprice = new EFS_Decimal(AssetETD.StrikePrice).ToConvertedFractionalPartString(
                                                        sql_DerivativeContract.StrikeNumericBase, sql_DerivativeContract.InstrumentDen,
                                                        sql_DerivativeContract.StrikeMultiplier, sql_DerivativeContract.StrikeFormatStyle)

                            };
                        }

                        assetETDRepository.PutCall = AssetETD.PutCall;
                        assetETDRepository.PutCallSpecified = StrFunc.IsFilled(AssetETD.PutCall);

                        assetETDRepository.FactorSpecified = AssetETD.Factor.HasValue;
                        if (assetETDRepository.FactorSpecified)
                            assetETDRepository.Factor = AssetETD.Factor.Value;

                        assetETDRepository.MaturityDateSpecified = DtFunc.IsDateTimeFilled(AssetETD.Maturity_MaturityDate);
                        if (assetETDRepository.MaturityDateSpecified)
                            assetETDRepository.MaturityDate = AssetETD.Maturity_MaturityDate;

                        assetETDRepository.MaturityMonthYearSpecified = StrFunc.IsFilled(AssetETD.Maturity_MaturityMonthYear);
                        if (assetETDRepository.MaturityMonthYearSpecified)
                            assetETDRepository.MaturityMonthYear = AssetETD.Maturity_MaturityMonthYear;

                        break;

                    case "assetIndex":
                        IAssetIndexRepository assetIndexRepository = (IAssetIndexRepository)ret;
                        SQL_AssetIndex sqlAssetIndex = (SQL_AssetIndex)sql_Asset;

                        assetIndexRepository.AssetSymbolSpecified = StrFunc.IsFilled(sqlAssetIndex.AssetSymbol);
                        if (assetIndexRepository.AssetSymbolSpecified)
                            assetIndexRepository.AssetSymbol = sqlAssetIndex.AssetSymbol;

                        assetIndexRepository.BBGCodeSpecified = StrFunc.IsFilled(sqlAssetIndex.BBGCode);
                        if (assetIndexRepository.BBGCodeSpecified)
                            assetIndexRepository.BBGCode = sqlAssetIndex.BBGCode;

                        assetIndexRepository.RICCodeSpecified = StrFunc.IsFilled(sqlAssetIndex.RICCode);
                        if (assetIndexRepository.RICCodeSpecified)
                            assetIndexRepository.RICCode = sqlAssetIndex.RICCode;
                        break;

                    case "assetRateIndex":
                        IAssetRateIndexRepository assetRateIndexRepository = (IAssetRateIndexRepository)ret;
                        SQL_AssetRateIndex sql_AssetRateIndex = (SQL_AssetRateIndex)sql_Asset;

                        assetRateIndexRepository.InformationSource = assetRateIndexRepository.CreateInformationSource();
                        assetRateIndexRepository.InformationSource.RateSource.Value = sql_AssetRateIndex.Idx_SrcSupplierId;
                        assetRateIndexRepository.InformationSourceSpecified = StrFunc.IsFilled(assetRateIndexRepository.InformationSource.RateSource.Value);

                        assetRateIndexRepository.InformationSource.CreateRateSourcePage(sql_AssetRateIndex.Idx_SrcSupplierPageId);
                        assetRateIndexRepository.InformationSource.RateSourcePageSpecified = StrFunc.IsFilled(assetRateIndexRepository.InformationSource.RateSourcePage.Value);

                        assetRateIndexRepository.InformationSource.RateSourcePageHeading = sql_AssetRateIndex.Idx_SrcSupplierHeadId;
                        assetRateIndexRepository.InformationSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(assetRateIndexRepository.InformationSource.RateSourcePageHeading);

                        assetRateIndexRepository.RateType = sql_AssetRateIndex.Idx_RateType;
                        assetRateIndexRepository.RateTypeSpecified = StrFunc.IsFilled(assetRateIndexRepository.RateType);

                        assetRateIndexRepository.CalculationRule = sql_AssetRateIndex.Idx_CalculationRule;
                        assetRateIndexRepository.CalculationRuleSpecified = StrFunc.IsFilled(assetRateIndexRepository.CalculationRule);

                        break;
                    case "assetFxRate":
                        IAssetFxRateRepository assetFxRateRepository = (IAssetFxRateRepository)ret;
                        SQL_AssetFxRate sql_AssetFxRate = (SQL_AssetFxRate)sql_Asset;

                        // FI 20150315 [XXPOC] add primaryRateSrc
                        assetFxRateRepository.PrimaryRateSrc.RateSource.Value = sql_AssetFxRate.PrimaryRateSrc;
                        assetFxRateRepository.PrimaryRateSrc.RateSource.Scheme = null;
                        assetFxRateRepository.PrimaryRateSrc.RateSourcePageSpecified = StrFunc.IsFilled(sql_AssetFxRate.PrimaryRateSrcPage);
                        assetFxRateRepository.PrimaryRateSrc.RateSourcePageHeading = sql_AssetFxRate.PrimaryRateSrcHead;
                        assetFxRateRepository.PrimaryRateSrc.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_AssetFxRate.PrimaryRateSrcHead);

                        // FI 20150315 [XXPOC] add QuotedCurrencyPair
                        assetFxRateRepository.QuotedCurrencyPair.Currency1 = sql_AssetFxRate.QCP_Cur1;
                        assetFxRateRepository.QuotedCurrencyPair.Currency1Scheme = null;
                        assetFxRateRepository.QuotedCurrencyPair.Currency2 = sql_AssetFxRate.QCP_Cur2;
                        assetFxRateRepository.QuotedCurrencyPair.Currency2Scheme = null;
                        assetFxRateRepository.QuotedCurrencyPair.QuoteBasis = sql_AssetFxRate.QCP_QuoteBasisEnum;

                        // FI 20150315 [XXPOC]
                        assetFxRateRepository.FixingTime.HourMinuteTime.TimeValue = sql_AssetFxRate.TimeRateSrc;
                        assetFxRateRepository.FixingTime.BusinessCenter.Value = sql_AssetFxRate.IdBC_RateSrc;
                        assetFxRateRepository.FixingTime.BusinessCenter.BusinessCenterScheme = null;

                        IBusinessCenter businessCenter = assetFxRateRepository.FixingTime.BusinessCenter;
                        AddBusinessCenterRepository(pCs, pRepository, businessCenter.Value);
                        break;

                    case "assetCash":
                        //NOTHING
                        break;

                    case "assetCommodity":
                        IAssetCommodityRepository assetCommodityRepository = (IAssetCommodityRepository)ret;
                        SQL_AssetCommodity sql_AssetCommodity = (SQL_AssetCommodity)sql_Asset;
                        
                        sql_AssetCommodity.LoadTable(new string[]{"IDASSET", "IDCC"}); 
                        Nullable<Int32> idCC = sql_AssetCommodity.IdCC;
                        if (idCC.HasValue)
                            sql_AssetCommodity = new SQL_AssetCommodityContract(pCs, sql_AssetCommodity.IdAsset );

                        assetCommodityRepository.AssetSymbolSpecified = StrFunc.IsFilled(sql_AssetCommodity.AssetSymbol);
                        if (assetCommodityRepository.AssetSymbolSpecified)
                            assetCommodityRepository.AssetSymbol = sql_AssetCommodity.AssetSymbol;

                        if (sql_AssetCommodity.GetType().Equals(typeof(SQL_AssetCommodityContract)))
                        {
                            SQL_AssetCommodityContract sql_AssetCommodityContract = sql_AssetCommodity as SQL_AssetCommodityContract;

                            assetCommodityRepository.ContractSymbolSpecified = StrFunc.IsFilled(sql_AssetCommodityContract.CommodityContract_ContractSymbol);
                            if (assetCommodityRepository.ContractSymbolSpecified)
                                assetCommodityRepository.ContractSymbol = sql_AssetCommodityContract.CommodityContract_ContractSymbol;

                            assetCommodityRepository.ExchContractSymbolSpecified = StrFunc.IsFilled(sql_AssetCommodityContract.CommodityContract_ExchContractSymbol);
                            if (assetCommodityRepository.ExchContractSymbolSpecified)
                                assetCommodityRepository.ExchContractSymbol = sql_AssetCommodityContract.CommodityContract_ExchContractSymbol;

                            assetCommodityRepository.QtyUnitSpecified = StrFunc.IsFilled(sql_AssetCommodityContract.CommodityContract_UnitOfPrice);
                            if (assetCommodityRepository.QtyUnitSpecified)
                                assetCommodityRepository.QtyUnit = sql_AssetCommodityContract.CommodityContract_UnitOfPrice;

                            assetCommodityRepository.DeliveryPointSpecified = StrFunc.IsFilled(sql_AssetCommodityContract.CommodityContract_DeliveryPoint);
                            if (assetCommodityRepository.DeliveryPointSpecified)
                            {
                                assetCommodityRepository.DeliveryPoint = sql_AssetCommodityContract.CommodityContract_DeliveryPoint;
                                // FI 20170206 [21916] add Enum information
                                AddEnumRepository(pCs, pRepository, "DeliveryPointECC", assetCommodityRepository.DeliveryPoint);
                            }

                            assetCommodityRepository.DeliveryTimezoneSpecified = StrFunc.IsFilled(sql_AssetCommodityContract.CommodityContract_TimeZone);
                            if (assetCommodityRepository.DeliveryTimezoneSpecified)
                            {
                                assetCommodityRepository.DeliveryTimezone = sql_AssetCommodityContract.CommodityContract_TimeZone;
                                // FI 20170206 [21916] add Enum information
                                // EG 20170929 [22374][23450] upd TimeZoneLocation enum n'existe plus (Nodatime is used)
                                //AddEnumRepository(pCs, pRepository, "TimezoneLocation", assetCommodityRepository.deliveryTimezone);
                                AddMapZoneRepository(pRepository, assetCommodityRepository.DeliveryTimezone);
                            }

                            assetCommodityRepository.DurationSpecified = StrFunc.IsFilled(sql_AssetCommodityContract.CommodityContract_Duration);
                            if (assetCommodityRepository.DurationSpecified)
                            {
                                assetCommodityRepository.Duration = sql_AssetCommodityContract.CommodityContract_Duration;
                                // FI 20170206 [21916] add Enum information
                                AddEnumRepository(pCs, pRepository, "SettlementPeriodDurationEnum", assetCommodityRepository.Duration);
                            }

                            assetCommodityRepository.QtyScaleSpecified = true;
                            if (assetCommodityRepository.QtyScaleSpecified)
                                assetCommodityRepository.QtyScale = sql_AssetCommodityContract.CommodityContract_QtyScale;
                        }
                        
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", element));
                }
            }
            return ret;
        }
        
        /// <summary>
        /// Construit une alternative identifier pour l'asset
        /// </summary>
        /// <param name="sqlAsset"></param>
        /// <param name="element">Type d'asset</param>
        /// <param name="pIsAddCodeIsin">Avec présence du code ISIN</param>
        /// <returns></returns>
        /// FI 20150513 [XXXXX] Add (code c# issu du template UKDisplay_GetUnderlyerDescription)
        /// FI 20150708 [XXXXX] Modify
        /// FI 20150915 [21315] Modify
        /// FI 20151019 [21317] Modify   
        private static string BuildAssetAltIdentifier(SQL_AssetBase sqlAsset, string element, Boolean pisAddCodeIsin)
        {
            string @default = StrFunc.IsFilled(sqlAsset.Description) ? sqlAsset.Description : sqlAsset.Identifier;

            string isoMarket = sqlAsset.Market_ISO10383_ALPHA4;
            string idC = sqlAsset.IdC;
            string isincode = string.Empty;
            switch (element)
            {
                case "assetEquity":
                    isincode = ((SQL_AssetEquity)sqlAsset).ISINCode;
                    break;
                case "assetETD":
                    isincode = ((SQL_AssetETD)sqlAsset).ISINCode;
                    break;
                case "assetDebtSecurity":
                    isincode = ((SQL_AssetDebtSecurity)sqlAsset).ISINCode;
                    break;
            }

            string asset = @default;
            switch (element)
            {
                case "assetEquity":
                case "assetETD":
                    string tmp = StrFunc.AppendFormat("- {0}", isoMarket);
                    // Supression du "- codeISO" du marché s'il existe en fin de chaîne
                    if (asset.EndsWith(tmp))
                        asset = StrFunc.Before(asset, tmp).TrimEnd();

                    // Supression du "(code ISIN)" de l'asset s'il existe en fin de chaîne
                    tmp = StrFunc.AppendFormat("({0})", isincode);
                    if (asset.EndsWith(tmp))
                        asset = StrFunc.Before(asset, tmp).TrimEnd();

                    // Supression du "- Devise" s'il existe en fin de chaîne
                    tmp = StrFunc.AppendFormat("- {0}", idC);
                    if (asset.EndsWith(tmp))
                        asset = StrFunc.Before(asset, tmp).TrimEnd();

                    // Spheres® retient ce qui existe après le code ISIN lorsque l'asset commence par le CODEISIN
                    // FI 20150708 [XXXXX] 
                    if (StrFunc.IsFilled(isincode) && asset.StartsWith(isincode))
                        asset = StrFunc.After(asset, isincode, OccurenceEnum.First); // FI 20150915 [21315] add parameter First
                    break;

                case "assetDebtSecurity": // FI 20151019 [21317] gestion des titres de rémunération
                    // Supression du code ISIN de l'asset s'il existe en fin de chaîne
                    if (StrFunc.IsFilled(isincode) && asset.EndsWith(StrFunc.AppendFormat(" {0}", isincode)))
                        asset = StrFunc.Before(asset, isincode, OccurenceEnum.Last).TrimEnd(); // FI 20150915 [21315] add parameter First

                    // Supression du " Devise" s'il existe en fin de chaîne
                    tmp = StrFunc.AppendFormat(" {0}", idC);
                    if (asset.EndsWith(tmp))
                        asset = StrFunc.Before(asset, tmp).TrimEnd();

                    break;
            }

            string ret = @default;
            if (StrFunc.IsFilled(asset))
                ret = asset.Trim();

            if (pisAddCodeIsin)
            {
                if (StrFunc.IsFilled(isincode) && (false == ret.Contains(isincode)))
                    ret = StrFunc.AppendFormat("{0} - {1}", ret, isincode);
            }

            return ret;
        }


        /// <summary>
        /// Ajoute dans {pRepository}  le contrat derivé du référentiel Spheres® {pSql_DerivativeContract} s'il n'y est pas présent 
        /// <para>○ Retourne l'élément IDerivativeContractRepository s'il a été ajouté</para>
        /// <para>○ Retourne null si le contrat derivé n'a pas été ajouté</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pSql_DerivativeContract"></param>
        // FI 20150218 [20275] Modify
        // PL 20181001 [24212] RICCODE/BBGCODE
        public static IDerivativeContractRepository AddDerivativeContractRepository(string pCs, IRepository pRepository, SQL_DerivativeContract pSql_DerivativeContract)
        {
            IDerivativeContractRepository ret = null;

            string href_pref = "DERIVATIVECONTRACT.IDDC.";

            if (pSql_DerivativeContract.IsLoaded)
            {
                string href = href_pref + pSql_DerivativeContract.Id.ToString();
                if (false == IsRepositoryExist(pRepository.DerivativeContract, href))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.DerivativeContract))
                        pos = pRepository.DerivativeContract.Length;
                    //
                    ReflectionTools.AddItemInArray(pRepository, "derivativeContract", pos);
                    ret = pRepository.DerivativeContract[pos];
                    //
                    ret.OTCmlId = pSql_DerivativeContract.Id;
                    ret.Id = href;
                    ret.Identifier = pSql_DerivativeContract.Identifier;
                    ret.Displayname = pSql_DerivativeContract.DisplayName;
                    ret.Description = pSql_DerivativeContract.Description;
                    ret.DescriptionSpecified = StrFunc.IsFilled(pSql_DerivativeContract.Description);
                    ret.Extllink = pSql_DerivativeContract.ExtlLink;
                    ret.ExtllinkSpecified = StrFunc.IsFilled(pSql_DerivativeContract.ExtlLink);

                    ret.IdM = pSql_DerivativeContract.IdMarket;
                    ret.IdMSpecified = (pSql_DerivativeContract.IdMarket > 0);
                    //
                    ret.AssetCategory = pSql_DerivativeContract.AssetCategory;
                    ret.AssetCategorySpecified = StrFunc.IsFilled(pSql_DerivativeContract.AssetCategory);
                    //
                    ret.IdC_Price = pSql_DerivativeContract.PriceCurrency;
                    ret.IdC_PriceSpecified = StrFunc.IsFilled(pSql_DerivativeContract.PriceCurrency);
                    //
                    ret.IdC_Nominal = pSql_DerivativeContract.NominalCurrency;
                    ret.IdC_NominalSpecified = StrFunc.IsFilled(pSql_DerivativeContract.NominalCurrency);
                    //
                    ret.Category = pSql_DerivativeContract.Category;
                    ret.CategorySpecified = StrFunc.IsFilled(pSql_DerivativeContract.Category);
                    //
                    ret.ExerciseStyle = pSql_DerivativeContract.ExerciseStyle;
                    ret.ExerciseStyleSpecified = StrFunc.IsFilled(pSql_DerivativeContract.ExerciseStyle);
                    //
                    ret.ContractSymbol = pSql_DerivativeContract.ContractSymbol;
                    ret.ContractSymbolSpecified = StrFunc.IsFilled(pSql_DerivativeContract.ContractSymbol);
                    //
                    ret.FutValuationMethod = pSql_DerivativeContract.FutValuationMethod;
                    ret.FutValuationMethodSpecified = StrFunc.IsFilled(pSql_DerivativeContract.FutValuationMethod);
                    //
                    ret.SettltMethod = pSql_DerivativeContract.SettlementMethod;
                    ret.SettltMethodSpecified = StrFunc.IsFilled(pSql_DerivativeContract.SettlementMethod);
                    //
                    ret.RICCode = pSql_DerivativeContract.RICCode;
                    ret.RICCodeSpecified = StrFunc.IsFilled(pSql_DerivativeContract.RICCode);
                    //
                    ret.BBGCode = pSql_DerivativeContract.BBGCode;
                    ret.BBGCodeSpecified = StrFunc.IsFilled(pSql_DerivativeContract.BBGCode);
                    //
                    if (pSql_DerivativeContract.ContractMultiplier.HasValue)
                        ret.ContractMultiplier = pSql_DerivativeContract.ContractMultiplier.Value;
                    ret.ContractMultiplierSpecified = (null != pSql_DerivativeContract.ContractMultiplier);
                    //
                    ret.FactorSpecified = (pSql_DerivativeContract.Factor.HasValue);
                    if (ret.FactorSpecified)
                        ret.Factor = pSql_DerivativeContract.Factor.Value;
                    //
                    ret.InstrumentDen = pSql_DerivativeContract.InstrumentDen;

                    // FI 20150218 [20275] alimentation de priceFmtStyle
                    ret.PriceFmtStyleSpecified = true;
                    if (ret.PriceFmtStyleSpecified)
                        ret.PriceFmtStyle = pSql_DerivativeContract.PriceFormatStyle.ToString();

                    // FI 20150218 [20275] alimentation de strikeFmtStyle
                    ret.StrikeFmtStyleSpecified = (ret.Category == "O");
                    if (ret.StrikeFmtStyleSpecified)
                        ret.StrikeFmtStyle = pSql_DerivativeContract.StrikeFormatStyle.ToString();

                    if (System.Enum.IsDefined(typeof(Cst.UnderlyingAsset), pSql_DerivativeContract.AssetCategory))
                    {
                        Cst.UnderlyingAsset underlyingAsset = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), pSql_DerivativeContract.AssetCategory);
                        switch (underlyingAsset)
                        {
                            case Cst.UnderlyingAsset.Future:
                                if (pSql_DerivativeContract.IdDcUnl > 0)
                                {
                                    SQL_DerivativeContract sqlDrvContractFuture = new SQL_DerivativeContract(pCs, pSql_DerivativeContract.IdDcUnl);
                                    AddDerivativeContractRepository(pCs, pRepository, sqlDrvContractFuture);
                                    //
                                    ret.IdDC_Unl = pSql_DerivativeContract.IdDcUnl.ToString();
                                    ret.IdDC_UnlSpecified = true;
                                }
                                break;
                            default:
                                if (pSql_DerivativeContract.IdAssetUnl > 0)
                                {
                                    ret.IdAsset_Unl = pSql_DerivativeContract.IdAssetUnl.ToString();
                                    ret.IdAsset_UnlSpecified = true;

                                    RepositoryTools.AddAssetRepository(pCs, pRepository, new Pair<Cst.UnderlyingAsset, int>(underlyingAsset, pSql_DerivativeContract.IdAssetUnl));
                                }
                                break;
                        }
                    }

                    // FI 20220906 [XXXXX] Alimentation de ret.extlDesc à partir de Description (en priorité) ou DisplayName 
                    if (StrFunc.IsFilled(pSql_DerivativeContract.Description))
                    {
                        ret.ExtlDesc = RetrieveDataBetweenParentheses(pSql_DerivativeContract.Description);
                        ret.ExtlDescSpecified = StrFunc.IsFilled(ret.ExtlDesc);
                    }

                    if (false == ret.ExtlDescSpecified && StrFunc.IsFilled(pSql_DerivativeContract.DisplayName))
                    {
                        ret.ExtlDesc = RetrieveDataBetweenParentheses(pSql_DerivativeContract.DisplayName);
                        ret.ExtlDescSpecified = StrFunc.IsFilled(ret.ExtlDesc);
                    }

                    // FI 20220908 [XXXXX] Alimentation de attrib  
                    ret.Attrib = pSql_DerivativeContract.Attribute;
                    ret.AttribSpecified = StrFunc.IsFilled(ret.Attrib);

                    ret.ETDIdentifierFormat = pSql_DerivativeContract.ETDIdentifierFormat;
                    ret.ETDIdentifierFormatSpecified = StrFunc.IsFilled(pSql_DerivativeContract.ETDIdentifierFormat);

                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne la string à l'intérieur des parenthèses si présentes dans <paramref name="input"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string RetrieveDataBetweenParentheses(string input)
        {
            /*
             Exemples 
                input: OG Opt (GOLD OPTIONS) Phys Am
                => GOLD OPTIONS

                input: VI1 Opt (Veolia Environnement (100)) Phys Am	
                => Veolia Environnement (100)	

                input: APH Opt (AMPHENOL CORP (NEW) CL-A) Phys Am	
                => AMPHENOL CORP (NEW) CL-A	
                
                input : UN (UNILEVER) (UNILEVER PLC) Std/Opt Phys AM	
                => UNILEVER	
                (Remarque: Spheres® ne considère que les premières parenthèes)

                input: UDR Opt (UNITED DOMINION RLTY TR INC(MARYLAN) Phys Am	
                => UNITED DOMINION RLTY TR INC(MARYLAN) Phys A
                (Remarque: Tolérance lorsque input est incorrectement formaté. Spheres® considère tout après la 1er parenthèse ouvrante)

              */

            string ret = string.Empty;

            if (input.Contains("("))
            {
                char[] inputArray = input.ToCharArray();

                int nbOpen = 0;
                int nbClose = 0;
                List<char> lstRet = new List<char>();
                Boolean found = false;
                int i = -1;

                while ((false == found) & (i < inputArray.Count() - 2))
                {
                    i++;

                    if (nbOpen > 0)
                        lstRet.Add(inputArray[i]);

                    if (inputArray[i] == '(')
                        nbOpen++;

                    if (inputArray[i] == ')')
                        nbClose++;

                    found = (nbOpen > 0 & (nbOpen == nbClose));
                    if (found)
                        lstRet.RemoveAt(lstRet.Count() - 1);
                };

                if (found || lstRet.Count() > 0) // lstRet.Count() > 0 afin de gérer le cas où input est incorrectement formaté
                {
                    ret = string.Concat(lstRet);
                }
            }

            return ret;

        }


        /// <summary>
        /// Methode de test qu'il faut exécuter à chaque modification de la méthode afin de vérifier qu'il n'y a pas de dégradation
        /// </summary>
        /// <param name="CS"></param>
        public static void TestRetrieveDataBetweenParentheses(string CS)
        {
            string SQLSelect = "select DESCRIPTION from dbo.DERIVATIVECONTRACT where DESCRIPTION like '%(%'";

            using (IDataReader dataReader = DataHelper.ExecuteReader(CS, CommandType.Text, SQLSelect))
            {
                while (dataReader.Read())
                {
                    string before = dataReader["DESCRIPTION"].ToString();
                    string after = RetrieveDataBetweenParentheses(before);

                    System.Diagnostics.Debug.WriteLine($"{before}=>{after}");
                }
            }
        }



        /// <summary>
        /// Recherche le contrat derivé dans le référentiel Spheres® et l'ajoute dans {pRepository} s'il n'y est pas présent 
        /// <para>○ Retourne l'élément IDerivativeContractRepository s'il a été ajouté</para>
        /// <para>○ Retourne null si le contrat derivé n'a pas été ajouté</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pExchange">Marché auquel appartient le Derivative Contract(selon la règle SecurityExchange_t)</param>
        /// <param name="pSymbol">Symbol du Derivative Contract (selon la règle Symbol_t)</param>
        /// FI 20140903 [20275] Modify add paramter pDtRefForDtEnabled
        public static IDerivativeContractRepository AddDerivativeContractRepository(string pCs, IRepository pRepository, string pExchange, string pSymbol, DateTime pDtRefForDtEnabled)
        {
            IDerivativeContractRepository ret = null;

            // FI 20140903 [20275] Utilisation de pDtRefForDtEnabled (sur 1 même marché, suite à une CA il peut y avoir le même symbol por 2 IDDC)
            SQL_DerivativeContract sql_DerivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(pCs, null, pExchange, pSymbol, SQL_Table.ScanDataDtEnabledEnum.Yes, pDtRefForDtEnabled);
            if (null != sql_DerivativeContract)
                ret = Repository.RepositoryTools.AddDerivativeContractRepository(pCs, pRepository, sql_DerivativeContract);

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRepository"></param>
        /// <param name="pHRef"></param>
        /// <returns></returns>
        /// FI 20180818 [20275] Add Method
        public static bool IsRepositoryExist(ICommonRepository[] pRepository, string pHRef)
        {
            bool ret = false;

            if (ArrFunc.IsFilled(pRepository))
            {
                foreach (ICommonRepository item in pRepository)
                {
                    if (item.Id == pHRef)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// Chargement du référentiel INVOICINGRULES
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pInvoice"></param>
        public static void AddInvoicingRulesRepository(string pCs, IRepository pRepository, IInvoice pInvoice)
        {
            InvoicingScopeContainer scope = new InvoicingScopeContainer(pInvoice.Scope);
            scope.LoadSQLInvoicingRules(pCs);
            string href = "SCOPE.IDINVOICINGRULES." + pInvoice.Scope.OtcmlId;
            if ((null != scope.SqlInvoicingRules) && (false == IsRepositoryExist(pRepository.InvoicingScope, href)))
            {
                int pos = 0;
                if (ArrFunc.IsFilled(pRepository.InvoicingScope))
                    pos = pRepository.InvoicingScope.Length;
                ReflectionTools.AddItemInArray(pRepository, "invoicingScope", pos);
                IInvoicingScopeRepository repository = pRepository.InvoicingScope[pos];
                repository.OTCmlId = pInvoice.Scope.OTCmlId;
                repository.Id = href;
                repository.Identifier = scope.SqlInvoicingRules.Identifier;
                repository.Displayname = scope.SqlInvoicingRules.DisplayName;
                //
                repository.Description = scope.SqlInvoicingRules.Description;
                repository.DescriptionSpecified = StrFunc.IsFilled(scope.SqlInvoicingRules.Description);
                //
                repository.Extllink = scope.SqlInvoicingRules.ExtlLink;
                repository.ExtllinkSpecified = StrFunc.IsFilled(scope.SqlInvoicingRules.ExtlLink);
                //
                repository.EventType = scope.SqlInvoicingRules.EventType;
                repository.EventTypeSpecified = StrFunc.IsFilled(scope.SqlInvoicingRules.EventType);
                //
                repository.IdC_Fee = scope.SqlInvoicingRules.IdC_Fee;
                repository.IdC_FeeSpecified = StrFunc.IsFilled(scope.SqlInvoicingRules.IdC_Fee);
                //
                repository.PaymentType = scope.SqlInvoicingRules.PaymentType;
                repository.PaymentTypeSpecified = StrFunc.IsFilled(scope.SqlInvoicingRules.PaymentType);

            }
        }


        /// <summary>
        /// Ajout dans l'élément {pRepository}, les informations concernant la devise {pCurrency}
        /// <para>Lorsqu'une devise de référence est spécifié, l'élément contient le fixing vis à vis de devise de référence</para>
        /// <para></para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pRepository"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pCurrencyReference"></param>
        /// <param name="pDateFixing"></param>
        public static void AddCurrencyRepository(string pCs, IProductBase pProductBase, IRepository pRepository,
            string pCurrency, ICurrency pCurrencyReference, Nullable<DateTime> pDateFixing)
        {
            ICurrency currency = pProductBase.CreateCurrency(pCurrency);
            AddCurrencyRepository(pCs, pProductBase, pRepository, currency, pCurrencyReference, pDateFixing);
        }
        /// <summary>
        /// Ajout dans l'élément {pRepository}, les informations concernant la devise {pCurrency}
        /// <para>Lorsqu'une devise de référence est spécifié, l'élément contient le fixing vis à vis de devise de référence</para>
        /// <para></para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pRepository"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pCurrencyReference"></param>
        /// <param name="pDateFixing"></param>
        public static void AddCurrencyRepository(string pCs, IProductBase pProductBase, IRepository pRepository,
            ICurrency pCurrency, ICurrency pCurrencyReference, Nullable<DateTime> pDateFixing)
        {
            SQL_Currency sql_Currency = new SQL_Currency(pCs, SQL_Currency.IDType.Iso4217, pCurrency.Value);
            if (false == sql_Currency.IsLoaded)
                throw new Exception(StrFunc.AppendFormat("Currency [Iso4217:{0}", pCurrency.Value));

            string id = "CURRENCY.ISO4217_ALPHA3." + sql_Currency.Iso4217;

            // RD 20131015 [19067] un taux de change par jour

            if (false == IsCurrencyExist(pRepository.Currency, id, out ICurrencyRepository currencyRepository))
            {
                int pos = 0;
                if (ArrFunc.IsFilled(pRepository.Currency))
                    pos = pRepository.Currency.Length;

                ReflectionTools.AddItemInArray(pRepository, "currency", pos);
                currencyRepository = pRepository.Currency[pos];

                SetCurrencyRepository(pCs, pProductBase, currencyRepository, id, sql_Currency, pCurrencyReference, pDateFixing);
            }
            else
            {
                if (false == IsFxRateExist(currencyRepository, pDateFixing))
                {
                    SetFxRateRepository(pCs, pProductBase, currencyRepository, sql_Currency, pCurrencyReference, pDateFixing);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pCurrency"></param>
        /// <param name="psql_Currency"></param>
        /// <param name="pCurrencyReference"></param>
        /// <param name="pDateFixing"></param>
        // EG 20240216 [WI850][26600] Ajout Request Date pour édition des confirmation sur facture Migration MAREX
        private static void SetFxRateRepository(string pCS, IProductBase pProductBase,
                ICurrencyRepository pCurrency, SQL_Currency psql_Currency,
                ICurrency pCurrencyReference, Nullable<DateTime> pDateFixing)
        {
            if ((null != pCurrencyReference && pDateFixing.HasValue)
                && (psql_Currency.Iso4217 != pCurrencyReference.Value)
                && (pDateFixing.HasValue))
            {
                KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                {
                    IdC1 = psql_Currency.Iso4217,
                    IdC2 = pCurrencyReference.Value
                };
                keyAssetFXRate.SetQuoteBasis(true);

                KeyQuote keyQuote = new KeyQuote(pCS, pDateFixing.Value);
                SQL_Quote quote = new SQL_Quote(pCS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, pProductBase,
                    keyQuote, keyAssetFXRate);

                if (quote.IsLoaded)
                {
                    pCurrency.FxrateSpecified = true;

                    int pos = 0;
                    if (ArrFunc.IsFilled(pCurrency.Fxrate))
                        pos = pCurrency.Fxrate.Length;

                    ReflectionTools.AddItemInArray(pCurrency, "fxrate", pos);
                    IFxRateRepository currentFxRate = pCurrency.Fxrate[pos];
                    currentFxRate.RequestDate = pDateFixing.Value;
                    currentFxRate.FixingDate = new EFS_Date
                    {
                        DateValue = quote.Time
                    };

                    currentFxRate.Rate.DecValue = quote.QuoteValue;
                    currentFxRate.QuotedCurrencyPair.Currency1 = keyAssetFXRate.IdC1;
                    currentFxRate.QuotedCurrencyPair.Currency1Scheme = null;

                    currentFxRate.QuotedCurrencyPair.Currency2 = keyAssetFXRate.IdC2;
                    currentFxRate.QuotedCurrencyPair.Currency2Scheme = null;

                    currentFxRate.QuotedCurrencyPair.QuoteBasis = keyAssetFXRate.QuoteBasis;
                }
            }
        }

        /// <summary>
        ///  Alimente {pCurrency} à partir de {sql_Currency}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pId"></param>
        /// <param name="pCurrency"></param>
        /// <param name="psql_Currency"></param>
        /// <param name="pCurrencyReference"></param>
        /// <param name="pDateFixing"></param>
        private static void SetCurrencyRepository(string pCS, IProductBase pProductBase,
            ICurrencyRepository pCurrency, string pId,
            SQL_Currency psql_Currency, ICurrency pCurrencyReference, Nullable<DateTime> pDateFixing)
        {

            pCurrency.Id = pId;
            pCurrency.Identifier = psql_Currency.IdC;
            pCurrency.Displayname = psql_Currency.DisplayName;

            pCurrency.Description = psql_Currency.Description;
            pCurrency.DescriptionSpecified = StrFunc.IsFilled(pCurrency.Description);

            pCurrency.Extllink = Convert.ToString(psql_Currency.GetFirstRowColumnValue("EXTLLINK"));
            pCurrency.ExtllinkSpecified = StrFunc.IsFilled(pCurrency.Extllink);

            pCurrency.Symbol = psql_Currency.Symbol;
            pCurrency.SymbolSpecified = StrFunc.IsFilled(pCurrency.Symbol);

            pCurrency.Symbolalign = psql_Currency.SymbolAlign;
            pCurrency.SymbolalignSpecified = StrFunc.IsFilled(pCurrency.Symbolalign);

            pCurrency.ISO4217_num3 = psql_Currency.Iso4217_Num3;
            pCurrency.ISO4217_num3Specified = StrFunc.IsFilled(pCurrency.ISO4217_num3);

            pCurrency.Factor = psql_Currency.Factor;
            pCurrency.FactorSpecified = true;

            pCurrency.Rounddir = psql_Currency.RoundingDirectionFpML;
            pCurrency.RounddirSpecified = true;

            pCurrency.Roundprec = psql_Currency.RoundPrec;
            pCurrency.RoundprecSpecified = true;

            // RD 20131015 [19067] un taux de change par jour
            SetFxRateRepository(pCS, pProductBase, pCurrency, psql_Currency, pCurrencyReference, pDateFixing);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrencyRepository"></param>
        /// <param name="pHref"></param>
        /// <returns></returns>
        private static bool IsCurrencyExist(ICurrencyRepository[] pCurrencyRepository, string pHref, out ICurrencyRepository pCurrency)
        {
            bool ret = false;
            pCurrency = null;

            if (ArrFunc.IsFilled(pCurrencyRepository))
            {
                foreach (ICurrencyRepository item in pCurrencyRepository)
                {
                    if (item.Id == pHref)
                    {
                        ret = true;
                        pCurrency = item;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrencyRepository"></param>
        /// <param name="pDateFixing"></param>
        /// <returns></returns>
        private static bool IsFxRateExist(ICurrencyRepository pCurrencyRepository, Nullable<DateTime> pDateFixing)
        {
            bool ret = false;

            if (pDateFixing.HasValue && ArrFunc.IsFilled(pCurrencyRepository.Fxrate))
            {
                foreach (IFxRateRepository item in pCurrencyRepository.Fxrate)
                {
                    //if (item.FixingDate.DateValue == (DateTime)pDateFixing)
                    if (item.RequestDate == pDateFixing.Value)
                    {
                        ret = true;
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pTradeExtends"></param>
        public static void AddExtendRepository(string pCS, IRepository pRepository, ITradeExtends pTradeExtends)
        {
            foreach (ITradeExtend tradeExtend in pTradeExtends.TradeExtend)
            {
                SQL_DefineExtendDet sqlDefineExtendDet = new SQL_DefineExtendDet(pCS, tradeExtend.OTCmlId);
                sqlDefineExtendDet.LoadTable(new string[] { "IDDEFINEEXTENDDET,IDDEFINEEXTEND,IDENTIFIER,DISPLAYNAME,DESCRIPTION,DATATYPE,EXTLLINK" });
                //
                if (sqlDefineExtendDet.IsLoaded)
                {
                    int idDefineExtendDet = sqlDefineExtendDet.Id;
                    int idDefineExtend = sqlDefineExtendDet.IdDefineExtend;
                    //
                    string extendHref = "DEFINEEXTEND.ID." + idDefineExtend;
                    string extendDetHref = "DEFINEEXTENDDET.ID." + idDefineExtendDet;
                    //
                    tradeExtend.HRef = extendDetHref;
                    tradeExtend.HRefSpecified = StrFunc.IsFilled(extendDetHref);
                    //
                    IExtendRepository extendRepository = GetExtendFromRepository(pRepository.Extend, extendHref);
                    if (null == extendRepository)
                    {
                        #region Create new Extend
                        int pos = 0;
                        if (ArrFunc.IsFilled(pRepository.Extend))
                            pos = pRepository.Extend.Length;
                        //
                        ReflectionTools.AddItemInArray(pRepository, "extend", pos);
                        extendRepository = pRepository.Extend[pos];
                        extendRepository.OTCmlId = idDefineExtend;
                        extendRepository.Id = extendHref;
                        extendRepository.ExtendDetSpecified = true;
                        //
                        SQL_DefineExtend sqlDefineExtend = new SQL_DefineExtend(pCS, idDefineExtend);
                        sqlDefineExtend.LoadTable(new string[] { "IDDEFINEEXTEND,IDENTIFIER,DISPLAYNAME,DESCRIPTION,EXTLLINK" });
                        //
                        if (sqlDefineExtendDet.IsLoaded)
                        {
                            extendRepository.Identifier = sqlDefineExtend.Identifier;
                            extendRepository.Displayname = sqlDefineExtend.DisplayName;
                            extendRepository.Description = sqlDefineExtend.Description;
                            extendRepository.DescriptionSpecified = StrFunc.IsFilled(extendRepository.Description);
                            extendRepository.Extllink = sqlDefineExtend.ExtlLink;
                            extendRepository.ExtllinkSpecified = StrFunc.IsFilled(extendRepository.Extllink);
                        }
                        #endregion
                    }
                    //
                    if (false == IsExtendDetExist(extendRepository.ExtendDet, extendDetHref))
                    {
                        int pos = 0;
                        if (ArrFunc.IsFilled(extendRepository.ExtendDet))
                            pos = extendRepository.ExtendDet.Length;
                        //
                        ReflectionTools.AddItemInArray(extendRepository, "extendDet", pos);
                        IExtendDetRepository extendDetRepository = extendRepository.ExtendDet[pos];
                        //
                        extendDetRepository.OTCmlId = idDefineExtendDet;
                        extendDetRepository.Id = extendDetHref;
                        //
                        extendDetRepository.Identifier = sqlDefineExtendDet.Identifier;
                        extendDetRepository.Displayname = sqlDefineExtendDet.DisplayName;
                        extendDetRepository.Description = sqlDefineExtendDet.Description;
                        extendDetRepository.DescriptionSpecified = StrFunc.IsFilled(extendDetRepository.Description);
                        extendDetRepository.Extllink = sqlDefineExtendDet.ExtlLink;
                        extendDetRepository.ExtllinkSpecified = StrFunc.IsFilled(extendDetRepository.Extllink);
                        //
                        extendDetRepository.DataType = sqlDefineExtendDet.DataType;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExtendRepository"></param>
        /// <param name="pHref"></param>
        /// <returns></returns>
        private static IExtendRepository GetExtendFromRepository(IExtendRepository[] pExtendRepository, string pHref)
        {
            IExtendRepository ret = null;
            //
            if (ArrFunc.IsFilled(pExtendRepository))
            {
                foreach (IExtendRepository item in pExtendRepository)
                {
                    if (item.Id == pHref)
                    {
                        ret = item;
                        break;
                    }
                }
            }
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExtendDetRepository"></param>
        /// <param name="pHref"></param>
        /// <returns></returns>
        private static bool IsExtendDetExist(IExtendDetRepository[] pExtendDetRepository, string pHref)
        {
            bool ret = false;
            if (ArrFunc.IsFilled(pExtendDetRepository))
            {
                foreach (IExtendDetRepository item in pExtendDetRepository)
                {
                    if (item.Id == pHref)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// Alimentation de {pRepository} avec le businessCenter {pIdBC}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pIdBC"></param>
        /// <returns></returns>
        /// FI 20150403 [XXPOC] Add 
        public static void AddBusinessCenterRepository(string pCs, IRepository pRepository, string pIdBC)
        {
            if (StrFunc.IsFilled(pIdBC))
            {
                SQL_BusinessCenter sql = new SQL_BusinessCenter(pCs, pIdBC);
                if (sql.IsFound)
                    AddBusinessCenterRepository(pRepository, sql);
            }
        }


        /// <summary>
        /// Alimenation de {pRepository} avec l'instrument {pSql_Instr}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pRepository"></param>
        /// <param name="pSql_Instr"></param>
        /// <returns></returns>
        /// FI 20150403 [XXPOC] Add 
        public static void AddBusinessCenterRepository(IRepository pRepository, SQL_BusinessCenter pSql_bc)
        {
            string href_pref = "BUSINESSCENTER.IDBC.";

            if (pSql_bc.IsLoaded)
            {
                string href = href_pref + pSql_bc.IdBC;
                if (false == IsRepositoryExist(pRepository.BusinessCenter, href))
                {
                    int pos = 0;
                    if (ArrFunc.IsFilled(pRepository.BusinessCenter))
                        pos = pRepository.BusinessCenter.Length;

                    ReflectionTools.AddItemInArray(pRepository, "businessCenter", pos);
                    IBusinessCenterRepository item = pRepository.BusinessCenter[pos];

                    item.Id = href;
                    item.Identifier = pSql_bc.IdBC;
                    item.Displayname = pSql_bc.DisPlayname;

                    item.DescriptionSpecified = StrFunc.IsFilled(pSql_bc.Description);
                    if (item.DescriptionSpecified)
                        item.Description = pSql_bc.Description;

                    item.ExtllinkSpecified = StrFunc.IsFilled(pSql_bc.ExtlLink);
                    if (item.ExtllinkSpecified)
                        item.Extllink = pSql_bc.ExtlLink;
                }
            }
        }




        /// <summary>
        ///  Retourne l'asset repositoty de l'asset {pAsset}
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] Add Method
        /// FI 20151019 [21317] Modify 
        /// FI 20160530 [21885] Modify 
        public static IAssetRepository LoadAssetRepository(IRepository pRepository, Pair<int, Cst.UnderlyingAsset> pAsset)
        {
            if (null == pRepository)
                throw new ArgumentNullException("arg (name:pRepository) is null");

            if (null == pAsset)
                throw new ArgumentNullException("arg (name:asset) is null");

            IAssetRepository ret = null;

            switch (pAsset.Second)
            {
                case Cst.UnderlyingAsset.RateIndex:
                    if (pRepository.AssetRateIndexSpecified)
                        ret = pRepository.AssetRateIndex.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;
                case Cst.UnderlyingAsset.ExchangeTradedContract:
                    if (pRepository.AssetETDSpecified)
                        ret = pRepository.AssetETD.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;
                case Cst.UnderlyingAsset.FxRateAsset:
                    if (pRepository.AssetFxRateSpecified)
                        ret = pRepository.AssetFxRate.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;
                case Cst.UnderlyingAsset.Index:
                    if (pRepository.AssetIndexSpecified)
                        ret = pRepository.AssetIndex.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;
                case Cst.UnderlyingAsset.EquityAsset:
                    if (pRepository.AssetEquitySpecified)
                        ret = pRepository.AssetEquity.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;
                case Cst.UnderlyingAsset.Bond:
                    if (pRepository.AssetDebtSecuritySpecified)
                        ret = pRepository.AssetDebtSecurity.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;
                case Cst.UnderlyingAsset.Cash: // FI 20160530 [21885] add
                    if (pRepository.AssetCashSpecified)
                        ret = pRepository.AssetCash.Where(x => x.OTCmlId == pAsset.First).FirstOrDefault();
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("UnderlyingAsset (cat:{0}) is not implemented", pAsset.Second));
            }

            return ret;
        }

        /// <summary>
        ///  Retourne l'enum repository 
        /// </summary>
        /// <param name="pRepository"></param>
        /// <param name="pCode"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        public static IEnumRepository LoadEnumValue(IRepository pRepository, string pCode, string pValue)
        {
            IEnumRepository ret = null;

            IEnumsRepository code = null;
            if (pRepository.EnumsSpecified)
                code = pRepository.Enums.Where(x => x.Code == pCode).FirstOrDefault();

            if (code.EnumsDetSpecified)
                ret = code.EnumsDet.Where(x => x.Value == pValue).FirstOrDefault();

            return ret;
        }

    
    }




    /// <summary>
    ///  Représente un prix et son formatage selon les règles de formatage spécifiques aux éditions
    /// </summary>
    /// FI 20150218 [20275] add Class
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class RepositoryPrice
    {
        /// <summary>
        /// Représente le prix formaté
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("fmtPrice")]
        public string fmtprice;

        /// <summary>
        ///  valeur du prix
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public decimal price;

    }


}
using System;
using System.Data;
using ThreadTasks = System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
//
using EfsML.Enum;
//
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    ///  
    /// </summary>
    internal enum MarketColumnIdent
    {
        EXCHANGEACRONYM
    }

    /// <summary>
    /// Représente l'ensemble des assets ETD dont les cours sont requis
    /// </summary>
    /// PM 20180219 [23824] New
    internal class MarketDataAssetETDToImport : MarketDataAssetETD
    {
        #region Members
        /// <summary>
        /// Représente le(s) marché(s).
        /// <para>Généralement 1 marché mais pas toujours (exemple sur EUREX CLEARING AG avec 2 marchés => EUREX FRANKFURT (XEUR) et XHEX HELSINKI(XEUR))</para>
        /// </summary>
        private readonly Tuple<MarketColumnIdent, string> m_Market;

        /// <summary>
        /// Identifiant du Css de rattachement des marchés des assets concernés par l'importation
        /// </summary>
        // PM 20240122 [WI822] New
        private readonly string m_CssIdentifier;
        #endregion Members

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMarket"></param>
        public MarketDataAssetETDToImport(Tuple<MarketColumnIdent, string> pMarket) :
            base()
        {
            m_Market = pMarket;
        }

        /// <summary>
        /// Constructeur avec Identifiant du Css
        /// </summary>
        /// <param name="pCssIdentifier">Identifiant du Css</param>
        // PM 20240122 [WI822] New
        public MarketDataAssetETDToImport(string pCssIdentifier)
        {
            m_CssIdentifier = pCssIdentifier;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Chargement des assets dont les cours sont requis
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        // PM 20240122 [WI822] Rename
        public void LoadAssetByMarket(string pCs, DateTime pDtBusiness)
        {
            QueryParameters queryAsset = MarketDataAssetETDToImportReader.QueryByMarket(pCs, m_Market, pDtBusiness);

            using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, queryAsset.Query.ToString(), queryAsset.Parameters.GetArrayDbParameter()))
            {
                base.LoadAsset(dr.DataReaderEnumerator<MarketAssetETDToImport, MarketDataAssetETDToImportReader>());
            }
        }

        /// <summary>
        /// Chargement des assets dont le cours est requis
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        // PM 20240122 [WI822] Rename
        public async ThreadTasks.Task LoadAssetByMarketAsync(string pCs, DateTime pDtBusiness)
        {
            await ThreadTasks.Task.Run(() =>
            {
                LoadAssetByMarket(pCs, pDtBusiness);
            });
        }

        /// <summary>
        /// Chargement des assets dont les cours sont requis
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        // PM 20240122 [WI822] New
        public void LoadAssetByCss(string pCs, DateTime pDtBusiness)
        {
            QueryParameters queryAsset = MarketDataAssetETDToImportReader.QueryByCss(pCs, m_CssIdentifier, pDtBusiness);

            using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, queryAsset.Query.ToString(), queryAsset.Parameters.GetArrayDbParameter()))
            {
                base.LoadAsset(dr.DataReaderEnumerator<MarketAssetETDToImport, MarketDataAssetETDToImportReader>());
            }
        }

        /// <summary>
        /// Chargement des assets dont le cours est requis
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        // PM 20240122 [WI822] New
        public async ThreadTasks.Task LoadAssetByCssAsync(string pCs, DateTime pDtBusiness)
        {
            await ThreadTasks.Task.Run(() =>
            {
                LoadAssetByCss(pCs, pDtBusiness);
            });
        }

        /// <summary>
        /// Recherche d'un asset
        /// </summary>
        /// <param name="pSettings"></param>
        /// <param name="pRequest">critères de recherche</param>
        /// <returns>null si asset non trouvé</returns>
        public new MarketAssetETDToImport GetAsset(MarketAssetETDRequestSettings pSettings, MarketAssetETDRequest pRequest)
        {
            return base.GetAsset(pSettings, pRequest) as MarketAssetETDToImport;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de lecture des assets dont les cours sont requis
    /// </summary>
    /// PM 20180219 [23824] New
    internal class MarketDataAssetETDToImportReader : IReaderRow
    {
        #region Methods
        /// <summary>
        /// Requête de selection des assets d'un marché
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pMarket"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        /// PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice : Ajout IDASSET_UNL, MATURITYDATE
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        /// FI 20220321 [XXXXX] Refactoring Query
        // PM 20240122 [WI822] Rename and add ISINCODE, CONTRACTMULTIPLIER & IDC_PRICE
        public static QueryParameters QueryByMarket(string pCs, Tuple<MarketColumnIdent, string> pMarket, DateTime pDtBusiness)
        {
            string sqlQuery = $@"select asset.IDASSET, asset.IDENTIFIER, asset.ISINCODE, dc.IDDC, dc.IDM,
            dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.CONTRACTTYPE, dc.EXERCISESTYLE, dc.SETTLTMETHOD, dc.IDC_PRICE,
            ma.MATURITYMONTHYEAR, isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATE,
            asset.PUTCALL, asset.STRIKEPRICE,            
            dc.ASSETCATEGORY, dc.IDASSET_UNL, da.IDASSET as DA_IDASSET_UNL, 
            case when asset.CONTRACTMULTIPLIER is null then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end) else asset.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.ASSET_ETD asset
            inner join dbo.VW_ALLTRADEINSTRUMENT trdinst on (trdinst.IDASSET = asset.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            where (mk.{pMarket.Item1} = '{pMarket.Item2}') and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE)) 
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})";

            sqlQuery += $@" union 
            select asset_unl.IDASSET, asset_unl.IDENTIFIER, asset_unl.ISINCODE, dc_unl.IDDC, dc_unl.IDM,
            dc_unl.CONTRACTSYMBOL, dc_unl.CONTRACTATTRIBUTE, dc_unl.CATEGORY, dc_unl.CONTRACTTYPE, dc_unl.EXERCISESTYLE, dc_unl.SETTLTMETHOD, dc_unl.IDC_PRICE,
            ma_unl.MATURITYMONTHYEAR, isnull(ma_unl.MATURITYDATESYS, ma_unl.MATURITYDATE) as MATURITYDATE,
            asset_unl.PUTCALL, asset_unl.STRIKEPRICE,
            dc_unl.ASSETCATEGORY, dc_unl.IDASSET_UNL, da_unl.IDASSET as DA_IDASSET_UNL,
            case when asset_unl.CONTRACTMULTIPLIER is null then (case when da_unl.CONTRACTMULTIPLIER is null then dc_unl.CONTRACTMULTIPLIER else da_unl.CONTRACTMULTIPLIER end) else asset_unl.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.DERIVATIVEATTRIB da
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.VW_ALLTRADEINSTRUMENT trdinst on (trdinst.IDASSET = asset.IDASSET)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.ASSET_ETD asset_unl on (asset_unl.IDASSET = da.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da_unl on (da_unl.IDDERIVATIVEATTRIB = asset_unl.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma_unl on (ma_unl.IDMATURITY = da_unl.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc_unl on (dc_unl.IDDC = da_unl.IDDC)
            inner join dbo.MARKET mk_unl on (mk_unl.IDM = dc.IDM)
            where (mk_unl.{pMarket.Item1} = '{pMarket.Item2}') and (ma_unl.DELIVERYDATE is null or ma_unl.DELIVERYDATE >= @DTFILE) 
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc_unl", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk_unl", "DTFILE")})";

            sqlQuery += $@" union 
            select asset.IDASSET, asset.IDENTIFIER, asset.ISINCODE, dc.IDDC, dc.IDM,
            dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.CONTRACTTYPE, dc.EXERCISESTYLE, dc.SETTLTMETHOD, dc.IDC_PRICE,
            ma.MATURITYMONTHYEAR, isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATE,
            asset.PUTCALL, asset.STRIKEPRICE,
            dc.ASSETCATEGORY, dc.IDASSET_UNL, da.IDASSET as DA_IDASSET_UNL, 
            case when asset.CONTRACTMULTIPLIER is null then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end) else asset.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.ASSET_ETD asset
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            where (mk.{pMarket.Item1} = '{pMarket.Item2}') and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE))
            and (dc.ISMANDATORYIMPORTQUOTE = 1)
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE), pDtBusiness);

            QueryParameters qryParameters = new QueryParameters(pCs, sqlQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Requête de selection des assets d'une chambre
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCssIdentifier"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        // PM 20240122 [WI822] New
        public static QueryParameters QueryByCss(string pCs, string pCssIdentifier, DateTime pDtBusiness)
        {
            string sqlQuery = $@"select asset.IDASSET, asset.IDENTIFIER, asset.ISINCODE, dc.IDDC, dc.IDM,
            dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.CONTRACTTYPE, dc.EXERCISESTYLE, dc.SETTLTMETHOD, dc.IDC_PRICE,
            ma.MATURITYMONTHYEAR, isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATE,
            asset.PUTCALL, asset.STRIKEPRICE,            
            dc.ASSETCATEGORY, dc.IDASSET_UNL, da.IDASSET as DA_IDASSET_UNL,
            case when asset.CONTRACTMULTIPLIER is null then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end) else asset.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.ASSET_ETD asset
            inner join dbo.VW_ALLTRADEINSTRUMENT trdinst on (trdinst.IDASSET = asset.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            inner join dbo.ACTOR a on (a.IDA = mk.IDA)
            where (a.IDENTIFIER = '{pCssIdentifier}') and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE)) 
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "a", "DTFILE")})";

            sqlQuery += $@"
            union 
            select asset_unl.IDASSET, asset_unl.IDENTIFIER, asset_unl.ISINCODE, dc_unl.IDDC, dc_unl.IDM,
            dc_unl.CONTRACTSYMBOL, dc_unl.CONTRACTATTRIBUTE, dc_unl.CATEGORY, dc_unl.CONTRACTTYPE, dc_unl.EXERCISESTYLE, dc_unl.SETTLTMETHOD, dc_unl.IDC_PRICE,
            ma_unl.MATURITYMONTHYEAR, isnull(ma_unl.MATURITYDATESYS, ma_unl.MATURITYDATE) as MATURITYDATE,
            asset_unl.PUTCALL, asset_unl.STRIKEPRICE,
            dc_unl.ASSETCATEGORY, dc_unl.IDASSET_UNL, da_unl.IDASSET as DA_IDASSET_UNL,
            case when asset_unl.CONTRACTMULTIPLIER is null then (case when da_unl.CONTRACTMULTIPLIER is null then dc_unl.CONTRACTMULTIPLIER else da_unl.CONTRACTMULTIPLIER end) else asset_unl.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.DERIVATIVEATTRIB da
            inner join dbo.ASSET_ETD asset on (asset.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            inner join dbo.VW_ALLTRADEINSTRUMENT trdinst on (trdinst.IDASSET = asset.IDASSET)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.ASSET_ETD asset_unl on (asset_unl.IDASSET = da.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da_unl on (da_unl.IDDERIVATIVEATTRIB = asset_unl.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma_unl on (ma_unl.IDMATURITY = da_unl.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc_unl on (dc_unl.IDDC = da_unl.IDDC)
            inner join dbo.MARKET mk_unl on (mk_unl.IDM = dc.IDM)
            inner join dbo.ACTOR a on (a.IDA = mk_unl.IDA)
            where (a.IDENTIFIER = '{pCssIdentifier}') and (ma_unl.DELIVERYDATE is null or ma_unl.DELIVERYDATE >= @DTFILE) 
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc_unl", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk_unl", "DTFILE")})";

            sqlQuery += $@"
            union 
            select asset.IDASSET, asset.IDENTIFIER, asset.ISINCODE, dc.IDDC, dc.IDM,
            dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CATEGORY, dc.CONTRACTTYPE, dc.EXERCISESTYLE, dc.SETTLTMETHOD, dc.IDC_PRICE,
            ma.MATURITYMONTHYEAR, isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATE,
            asset.PUTCALL, asset.STRIKEPRICE,
            dc.ASSETCATEGORY, dc.IDASSET_UNL, da.IDASSET as DA_IDASSET_UNL, 
            case when asset.CONTRACTMULTIPLIER is null then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end) else asset.CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.ASSET_ETD asset
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            inner join dbo.ACTOR a on (a.IDA = mk.IDA)
            where (a.IDENTIFIER = '{pCssIdentifier}') and ((ma.DELIVERYDATE is null) or (ma.DELIVERYDATE >= @DTFILE))
            and (dc.ISMANDATORYIMPORTQUOTE = 1)
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
            and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.DTFILE), pDtBusiness);

            QueryParameters qryParameters = new QueryParameters(pCs, sqlQuery, dataParameters);

            return qryParameters;
        }
        #endregion Methods

        #region IReaderRow
        /// <summary>
        /// Data Reader permettant de lire les enregistrements
        /// </summary>
        public IDataReader Reader { get; set; }
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'un objet
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public object GetRowData()
        {
            MarketAssetETDToImport ret = default;
            if (null != Reader)
            {
                // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice : Ajout IDASSET_UNL, MATURITYDATESYS, MATURITYDATE
                // PM 20240122 [WI822] Ajout ISINCODE, CONTRACTMULTIPLIER et IDC_PRICE
                ret = new MarketAssetETDToImport()
                {
                    IdAsset = Convert.ToInt32(Reader["IDASSET"]),
                    AssetIdentifier = Reader["IDENTIFIER"].ToString(),
                    IdDC = Convert.ToInt32(Reader["IDDC"]),
                    IdM = Convert.ToInt32(Reader["IDM"]),
                    ContractSymbol = Reader["CONTRACTSYMBOL"].ToString(),
                    ContractCategory = Reader["CATEGORY"].ToString(),
                    ContractAttribute = (Convert.IsDBNull(Reader["CONTRACTATTRIBUTE"]) ? "0" : Reader["CONTRACTATTRIBUTE"].ToString()),
                    ContractType = ReflectionTools.ConvertStringToEnum<DerivativeContractTypeEnum>(Reader["CONTRACTTYPE"].ToString()),
                    ExerciseStyle = ReflectionTools.ConvertStringToEnumOrNullable<DerivativeExerciseStyleEnum>(Convert.IsDBNull(Reader["EXERCISESTYLE"]) ? string.Empty : Reader["EXERCISESTYLE"].ToString()),
                    SettlementMethod = ReflectionTools.ConvertStringToEnumOrDefault<SettlMethodEnum>((Convert.IsDBNull(Reader["SETTLTMETHOD"]) ? null : Reader["SETTLTMETHOD"].ToString()), SettlMethodEnum.CashSettlement),
                    MaturityMonthYear = Reader["MATURITYMONTHYEAR"].ToString(),
                    MaturityDate = Convert.IsDBNull(Reader["MATURITYDATE"]) ? default : Convert.ToDateTime(Reader["MATURITYDATE"]),
                    PutCall = ReflectionTools.ConvertStringToEnumOrNullable<PutOrCallEnum>(Convert.IsDBNull(Reader["PUTCALL"]) ? null : Reader["PUTCALL"].ToString()),
                    StrikePrice = (Convert.IsDBNull(Reader["STRIKEPRICE"]) ? 0 : Convert.ToDecimal(Reader["STRIKEPRICE"])),
                    UnderlyingAssetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString()),
                    IdAssetUnl = (Convert.IsDBNull(Reader["IDASSET_UNL"]) ? 0 : Convert.ToInt32(Reader["IDASSET_UNL"])),
                    IdAssetUnlFuture = (Convert.IsDBNull(Reader["DA_IDASSET_UNL"]) ? 0 : Convert.ToInt32(Reader["DA_IDASSET_UNL"])),
                    ISINCode = Reader["ISINCODE"].ToString(),
                    ContractMultiplier = (Convert.IsDBNull(Reader["CONTRACTMULTIPLIER"]) ? 0 : Convert.ToDecimal(Reader["CONTRACTMULTIPLIER"])),
                    PriceCurrency = Reader["IDC_PRICE"].ToString(),
                };
            }
            return ret;
        }
        #endregion IReaderRow
    }
}

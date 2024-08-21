using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ThreadTasks = System.Threading.Tasks;
using EFS.ACommon;
using EfsML;
using FpML.Enum;
using EFS.ApplicationBlocks.Data;
using EfsML.Enum;
using EFS.Common;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    internal delegate void QuoteEventHandler(object source, QuoteEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    internal class QuoteEventArgs : EventArgs
    {
        public DataRow DataRow { get; private set; }

        /// <summary>
        ///     Initialises a new instance of the <i>FonetEventArgs</i> class.
        /// </summary>
        public QuoteEventArgs(DataRow dataRow)
        {
            this.DataRow = dataRow;
        }
    }

    /// <summary>
    /// Gestion des cotations 
    /// </summary>
    internal class MarketAssetQuote
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<Cst.OTCml_TBL, AssetQuote> _assetQuote;

        private readonly string _idMarketEnv;
        private readonly string _idValScenario;

        private readonly DateTime _dtStart;
        private readonly int _idA;


        /// <summary>
        /// Dictionnaire des cotations gérées pat type (la clé du dictionnaire est la table exemple QUOTE_ETD_H, QUOTE_EQUITY_H, etc..)
        /// </summary>
        public Dictionary<Cst.OTCml_TBL, AssetQuote> AssetQuote
        {
            get { return _assetQuote; }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idMarketEnv"></param>
        /// <param name="idValScenario"></param>
        /// <param name="idA"></param>
        /// <param name="dtStart"></param>
        public MarketAssetQuote(string idMarketEnv, string idValScenario, DateTime dtStart, int idA)
        {
            _assetQuote = new Dictionary<Cst.OTCml_TBL, AssetQuote>();

            _idMarketEnv = idMarketEnv;
            _idValScenario = idValScenario;
            _dtStart = dtStart;
            _idA = idA;
        }


        /// <summary>
        /// Chargement des cotations existantes
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="marketDataAssetETDToImport">assets ETD dont le cours est requis</param>
        /// <param name="dtBusiness"></param>
        /// <param name="isOnlyETD">si true ne charge pas les cotations des ss-jacent de type autre que ETD</param>
        /// <returns></returns>
        /// FI 20221108 [26167] Ajout de isOnlyETD
        public async ThreadTasks.Task LoadQuoteAsync(string cs, MarketDataAssetETDToImport marketDataAssetETDToImport, DateTime dtBusiness, Boolean isOnlyETD)
        {

            _assetQuote.Clear();

            List<Cst.OTCml_TBL> lstTable = new List<Cst.OTCml_TBL>{
                Cst.OTCml_TBL.QUOTE_ETD_H
            };

            if (false == isOnlyETD)
            {
                lstTable.AddRange((from item in marketDataAssetETDToImport.AllAsset.Cast<MarketAssetETDToImport>().Where(x => StrFunc.IsFilled(x.UnderlyingAssetCategory) && x.IdAssetUnl > 0)
                                   select GetTable(Cst.ConvertToUnderlyingAsset(item.UnderlyingAssetCategory))).Where(x => x != Cst.OTCml_TBL.QUOTE_ETD_H).Distinct());

            }

            List<ThreadTasks.Task> lstTask = new List<ThreadTasks.Task>();
            foreach (Cst.OTCml_TBL item in lstTable)
            {
                _assetQuote.Add(item, new AssetQuote(cs, item, _idMarketEnv, _idValScenario, _dtStart, _idA));
                await _assetQuote[item].LoadQuoteAsync(cs, dtBusiness);
            }
        }

        /// <summary>
        /// Ajoute/modifie une cotation en mémoire
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="idAsset"></param>
        /// <param name="idM"></param>
        /// <param name="quoteTime"></param>
        /// <param name="quoteSide"></param>
        /// <param name="price"></param>
        /// <param name="currency"></param>
        public void InsertUpdate_QUOTE_XXX_H(Cst.UnderlyingAsset assetType, int idAsset, int idM, DateTime quoteTime, QuotationSideEnum quoteSide, decimal price, string currency)
        {
            Cst.OTCml_TBL key = GetTable(assetType);
            if (false == _assetQuote.ContainsKey(key))
                throw new InvalidOperationException($"Key: {key} doesn't exist. Quotation of {assetType} have not been loaded.");

            _assetQuote[key].InsertUpdate_QUOTE_XXX_H(idAsset, idM, quoteTime, quoteSide, price, currency);
        }

        /// <summary>
        /// Ajoute/modifie une cotation ETD en mémoire
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDelta"></param>
        // PM 20240122 [WI822] pPrice devient Nullable afin de pouvoir ne renseigner que pDelta
        public void InsertUpdate_QUOTE_ETD_H(int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, Nullable<decimal> pPrice, string pCurrency, Nullable<decimal> pDelta)
        {
            _assetQuote[Cst.OTCml_TBL.QUOTE_ETD_H].InsertUpdate_QUOTE_ETD_H(pIdAsset, pIdM, pQuoteTime, pQuoteSide, pPrice, pCurrency, pDelta);
        }

        /// <summary>
        /// Retourne la table de stockage pour le type d'asset <paramref name="assetType"/>
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns></returns>
        private static Cst.OTCml_TBL GetTable(Cst.UnderlyingAsset assetType)
        {
            return AssetTools.ConvertQuoteEnumToQuoteTbl(AssetTools.ConvertUnderlyingAssetToQuoteEnum(assetType));
        }

        /// <summary>
        ///  Mis à jour en base de données des modifications appliquées sur les cotations
        /// </summary>
        /// <returns></returns>
        public async ThreadTasks.Task UpdateDatabaseAsync()
        {
            foreach (Cst.OTCml_TBL key in _assetQuote.Keys)
            {
                await _assetQuote[key].UpdateDatabaseAsync();
            }
        }
        /// <summary>
        /// Retoune le nombre de cotations ajoutées/modifiées (tous assets confondus)
        /// </summary>
        /// <returns></returns>
        public int ChangesCount()
        {
            return (from item in _assetQuote.Keys
                    select _assetQuote[item].ChangesCount()).Sum();
        }
    }

    /// <summary>
    /// Représente un ensemble de cotation d'asset de même type  
    /// </summary>
    internal class AssetQuote : DataTableToUpdateBase
    {
        public event QuoteEventHandler OnSourceNotOverridable;
        public event QuoteEventHandler OnQuoteModified;
        public event QuoteEventHandler OnQuoteAdded;

        private List<String> _lstNotOverridableSource;

        private readonly DateTime _dtStart;
        private readonly int _idA;
        private readonly string _idMarketEnv;
        private readonly string _idValScenario;

        /// <summary>
        /// Représente la table de stockage des cotations (Exemple QUOTE_INDEX_H)
        /// </summary>
        public string TableName { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="table"></param>
        /// <param name="idMarketEnv"></param>
        /// <param name="idValScenario"></param>
        /// <param name="dtStart"></param>
        /// <param name="idA"></param>
        public AssetQuote(string cs, Cst.OTCml_TBL table, string idMarketEnv, string idValScenario, DateTime dtStart, int idA) : base()
        {
            TableName = table.ToString();
            _dtStart = dtStart;
            _idA = idA;
            _idMarketEnv = idMarketEnv;
            _idValScenario = idValScenario;
            LoadNotOverridableSource(cs);
        }


        /// <summary>
        /// Chargement de toutes les cotations en date <paramref name="dtBusiness"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dtBusiness"></param>
        /// <returns></returns>
        public async ThreadTasks.Task LoadQuoteAsync(string cs, DateTime dtBusiness)
        {
            // FI 20221108 [XXXXX] Modification de la query pour la rendre plus facile à lire
            string query = $@"select * from dbo.{TableName}
               where (IDMARKETENV = @IDMARKETENV)
                 and (IDVALSCENARIO = @IDVALSCENARIO)
                 and (TIME between @TIME1 and @TIME2)
                 and (IDBC is null)
                 and (CASHFLOWTYPE is null)";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(cs, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _idMarketEnv);
            dataParameters.Add(new DataParameter(cs, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _idValScenario);
            dataParameters.Add(new DataParameter(cs, "TIME1", DbType.DateTime), dtBusiness);
            dataParameters.Add(new DataParameter(cs, "TIME2", DbType.DateTime), dtBusiness.AddDays(1).AddSeconds(-1));

            QueryParameters qryParameters = new QueryParameters(cs, query, dataParameters);

            await base.LoadAsync(cs, TableName, qryParameters, qryParameters.GetQueryReplaceParameters());
        }


        /// <summary>
        /// Chargement de toutes les cotations d'un asset en date <paramref name="dtBusiness"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dtBusiness"></param>
        /// <param name="idAsset"></param>
        /// <returns></returns>
        /// FI 20221108 [26167] add
        public void LoadQuoteAsset(string cs, DateTime dtBusiness, int idAsset)
        {
            string query = $@"select * from dbo.{TableName}
               where (IDMARKETENV = @IDMARKETENV)
                 and (IDVALSCENARIO = @IDVALSCENARIO)
                 and (TIME between @TIME1 and @TIME2)
                 and (IDBC is null)
                 and (CASHFLOWTYPE is null)
                 and (IDASSET = @IDASSET)";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(cs, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _idMarketEnv);
            dataParameters.Add(new DataParameter(cs, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), _idValScenario);
            dataParameters.Add(new DataParameter(cs, "TIME1", DbType.DateTime), dtBusiness);
            dataParameters.Add(new DataParameter(cs, "TIME2", DbType.DateTime), dtBusiness.AddDays(1).AddSeconds(-1));
            dataParameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDASSET), idAsset);

            QueryParameters qryParameters = new QueryParameters(cs, query, dataParameters);

            base.Load(cs, TableName, qryParameters, qryParameters.GetQueryReplaceParameters());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        public void InsertUpdate_QUOTE_XXX_H(int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, decimal pPrice, string pCurrency)
        {
            InsertUpdate_QUOTE_H(pIdAsset, pIdM, pQuoteTime, pQuoteSide, pPrice, pCurrency, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDelta"></param>
        // PM 20240122 [WI822] pPrice devient Nullable afin de pouvoir ne renseigner que pDelta
        public void InsertUpdate_QUOTE_ETD_H(int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, Nullable<decimal> pPrice, string pCurrency, Nullable<decimal> pDelta)
        {
            InsertUpdate_QUOTE_H(pIdAsset, pIdM, pQuoteTime, pQuoteSide, pPrice, pCurrency, pDelta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDelta"></param>
        // PM 20240122 [WI822] pPrice devient Nullable afin de pouvoir ne renseigner que pDelta
        private void InsertUpdate_QUOTE_H(int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, Nullable<decimal> pPrice, string pCurrency, Nullable<decimal> pDelta)
        {
            DataRow dr = Find(pIdAsset, pQuoteTime, pQuoteSide);

            bool isOk = false;

            if (dr == null)
            {
                isOk = true;
                dr = DataTable.NewRow();
                DataTable.Rows.Add(dr);
                SetDataRowIns(dr);

                if ((TableName == Cst.OTCml_TBL.QUOTE_ETD_H.ToString()) && pDelta.HasValue && (false == pPrice.HasValue))
                {
                    // Cas ou l'on insert un delta dans QUOTE_ETD mais que la cotation n'est pas présente
                    pPrice = 0;
                }
            }
            else if (IsQuoteUpdatable(dr))
            {
                isOk = ((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                  || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                  || (Convert.IsDBNull(dr["IDBC"]) == false)
                  || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                  || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                  || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != _idMarketEnv))
                  || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != _idValScenario))
                  || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                  || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != pQuoteSide.ToString()))
                  || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                  || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                  || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                  || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                  || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != pQuoteTime));

                if (pPrice.HasValue)
                {
                    isOk = isOk || ((Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice.Value));
                }

                if (TableName == Cst.OTCml_TBL.QUOTE_ETD_H.ToString() && pDelta.HasValue)
                {
                    isOk = isOk || ((Convert.IsDBNull(dr["DELTA"]) == true) || (Convert.ToDecimal(dr["DELTA"]) != pDelta.Value));
                }

                if (isOk)
                {
                    SetDataRowUpd(dr);
                }
            }

            if (isOk)
            {
                dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                dr["IDASSET"] = pIdAsset;
                dr["IDBC"] = DBNull.Value; //Non présent
                dr["IDC"] = pCurrency;
                if (pIdM > 0)
                {
                    dr["IDM"] = pIdM;
                }
                dr["IDMARKETENV"] = _idMarketEnv;
                dr["IDVALSCENARIO"] = _idValScenario;
                dr["ISENABLED"] = true;
                dr["QUOTESIDE"] = pQuoteSide.ToString();
                dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString();
                dr["QUOTEUNIT"] = "Price";
                dr["SOURCE"] = "ClearingOrganization";
                dr["SPREADVALUE"] = 0;
                dr["TIME"] = pQuoteTime;
                if (pPrice.HasValue)
                {
                    dr["VALUE"] = pPrice.Value;
                }
                if (TableName == Cst.OTCml_TBL.QUOTE_ETD_H.ToString() && pDelta.HasValue)
                {
                    dr["DELTA"] = pDelta.Value;
                }

                if (dr.RowState == DataRowState.Added)
                {
                    OnQuoteAdded?.Invoke(this, new QuoteEventArgs(dr));
                }
                else if (dr.RowState == DataRowState.Modified)
                {
                    OnQuoteModified?.Invoke(this, new QuoteEventArgs(dr));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <returns></returns>
        private DataRow Find(int pIdAsset, DateTime pQuoteTime, QuotationSideEnum pQuoteSide)
        {
            return GetRows().Where(x => Convert.ToInt32(x["IDASSET"]) == pIdAsset && Convert.ToDateTime(x["TIME"]) == pQuoteTime && x["QUOTESIDE"].ToString() == pQuoteSide.ToString()).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        private void SetDataRowIns(DataRow dr)
        {
            if (DtFunc.IsDateTimeEmpty(_dtStart))
                throw new InvalidOperationException("dtStart is empty");

            dr["DTINS"] = _dtStart;
            dr["IDAINS"] = _idA;
        }

        /// <summary>
        /// Alimente les colonnes DTUPD et IDAUPD du datarow {pRow}
        /// </summary>
        /// <param name="dr"></param>
        private void SetDataRowUpd(DataRow dr)
        {
            if (DtFunc.IsDateTimeEmpty(_dtStart))
                throw new InvalidOperationException("dtStart is empty");

            dr["DTUPD"] = _dtStart;
            dr["IDAUPD"] = _idA;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private Boolean IsQuoteUpdatable(DataRow dr)
        {

            Boolean isOk = (dr.RowState == DataRowState.Unchanged); // Spheres® ne change pas une cotation déjà ajoutée/modifiée en mémoire
            if (isOk)
            {
                DateTime dtSys = Convert.IsDBNull(dr["DTUPD"]) ? Convert.ToDateTime(dr["DTINS"]) : Convert.ToDateTime(dr["DTUPD"]);
                if (dtSys == _dtStart)
                {
                    /// Spheres® ne change pas une cotation déjà validée (cad ajoutée/modifiée en base de données) par l'import courant (test sur les dates pour vérifier qu'on est bien sur l'import courant). 
                    /// pour information, lorsque la base est mise à jour le status des lignes est Unchanged <seealso cref="DataTableToUpdateBase.AcceptChanges"/>   
                    isOk = false;
                }
            }

            if (isOk && Convert.ToInt32(dr["IDQUOTE_H"]) > 0 && StrFunc.IsFilled(dr["SOURCE"].ToString()))
            {
                isOk = (false == _lstNotOverridableSource.Contains(dr["SOURCE"].ToString()));
                if (!isOk && null != OnSourceNotOverridable)
                    OnSourceNotOverridable(this, new QuoteEventArgs(dr));
            }

            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadNotOverridableSource(string cs)
        {
            /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            ExtendEnums extendEnums = ExtendEnumsTools.ListEnumsSchemes;
            if (null == extendEnums)
                throw new InvalidOperationException("Enums are not loaded");

            ExtendEnum extendEnum = ExtendEnumsTools.ListEnumsSchemes["InformationProvider"];
            if (null == extendEnum)
                throw new InvalidOperationException("Enum InformationProvider not found");
            */

            ExtendEnum[] extendEnums =  new DataEnabledEnum(cs).GetData();
            if (null == extendEnums)
                throw new InvalidOperationException("Enums are not loaded");

            ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(cs, "InformationProvider");
            if (null == extendEnum)
                throw new InvalidOperationException("Enum InformationProvider not found");



            _lstNotOverridableSource = (from item in extendEnum.item.Where(x => StrFunc.IsFilled(x.CustomValue) && x.CustomValue.Contains("NOTOVERRIDABLE"))
                                       select item.Value).ToList();
        
        }
    }
}
 

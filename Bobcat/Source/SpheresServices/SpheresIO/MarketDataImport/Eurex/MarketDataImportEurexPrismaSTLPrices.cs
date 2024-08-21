using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EfsML;
using EfsML.Enum;
using FixML.Enum;
using FpML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ThreadTasks = System.Threading.Tasks;


namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Permet l'importation des fichiers STL Eurex PRISMA (Importation des cours, Mise à jour des DC)
    /// </summary>
    // FI 20220325 [XXXXX] Add
    internal class MarketDataImportEurexPrismaSTLPrices : MarketDataImportEurexPrismaBase
    {
        #region Members
        /// <summary>
        /// Gestion des assets ETD (avec leur ss-jacent) dont les cours sont requis
        /// </summary>
        MarketDataAssetETDToImport _assetETDToImport;

        /// <summary>
        /// Paramètres de recherche de l'asset avec la Date de l'échéance
        /// </summary>
        /// PM 20220107 [XXXXX] Ajout
        MarketAssetETDRequestSettings _assetETDRequestSettingsDate;

        /// <summary>
        /// Paramètres de recherche de l'asset avec le Nom de l'échéance
        /// </summary>
        /// PM 20220107 [XXXXX] Renommage
        MarketAssetETDRequestSettings _assetETDRequestSettingsMonthYear;

        /// <summary>
        /// Gestion des cotations des assets
        /// </summary>
        MarketAssetQuote _assetQuote;

        /// <summary>
        /// Gestion des Contract Multiplier sur les DC
        /// </summary>
        MarketDerivativeContractCM _dcCM;
        #endregion Members

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="pDataName"></param>
        /// <param name="pDataStyle"></param>
        public MarketDataImportEurexPrismaSTLPrices(Task pTask, string pDataName, string pDataStyle) :
            base(pTask, pDataName, pDataStyle)
        {
            // PM 20220107 [XXXXX] Ajout deuxième MarketAssetETDRequestSettings pour recherche par date d'échéance
            _assetETDRequestSettingsDate = PrismaTools.GetAssetETDRequestSettings(AssetETDRequestMaturityMode.MaturityDate);
            _assetETDRequestSettingsMonthYear = PrismaTools.GetAssetETDRequestSettings(AssetETDRequestMaturityMode.MaturityMonthYear);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        ///  Importation des cours, Mise à jour des DC 
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        public int InputSettlementPriceFile(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);

                // PM 20151124 [20124] Initialise les membres m_IdMarketEnv & m_IdValScenario
                InitDefaultMembers();

                Tuple<MarketColumnIdent, string> marketIdent = new Tuple<MarketColumnIdent, string>(MarketColumnIdent.EXCHANGEACRONYM, ExchangeAcronym);

                _assetQuote = new MarketAssetQuote(m_IdMarketEnv, m_IdValScenario,  dtStart, task.UserId);

                // Lecture des assets dont les cours sont requis
                _assetETDToImport = new MarketDataAssetETDToImport(marketIdent);
                ThreadTasks.Task taskLoadAsset = _assetETDToImport.LoadAssetByMarketAsync(Cs, m_dtFile).ContinueWith((r) =>
                {
                    // Lecture des cotations
                    _assetQuote.LoadQuoteAsync(Cs, _assetETDToImport, m_dtFile, true).Wait();

                    foreach (Cst.OTCml_TBL key in _assetQuote.AssetQuote.Keys)
                    {
                        _assetQuote.AssetQuote[key].OnSourceNotOverridable += new QuoteEventHandler(OnQuoteSourceNotOverridable);
                        _assetQuote.AssetQuote[key].OnQuoteAdded += new QuoteEventHandler(OnQuoteUpdated);
                        _assetQuote.AssetQuote[key].OnQuoteModified += new QuoteEventHandler(OnQuoteUpdated);
                    }
                });

                // PM 20151209 [21653] Lecture des DC sans Contract Multiplier
                _dcCM = new MarketDerivativeContractCM(marketIdent, dtStart, task.UserId);
                ThreadTasks.Task taskLoadDc = _dcCM.LoadDCByMarketAsync(Cs, m_dtFile);

                ThreadTasks.Task.WaitAll(taskLoadAsset, taskLoadDc);


                bool isDCToUpdate = false;
                bool isDCFlexToUpdate = false;

                Boolean isProductOk = false;
                Boolean isProductFlexOk = false; // PM 20151214 [21643] Add isProductFlexOk

                IOTaskDetInOutFileRow parsingRowProduct = null;
                string currency = string.Empty;
                decimal productTickSize = 0;
                decimal productTickValue = 0;
                string productID = null;

                IOTaskDetInOutFileRow parsingRowExpiry = null;
                bool isNewExpiry = false;
                bool isUnlPriceForIndexOptionDone = false;
                MarketAssetETDToImport assetETDForIndexOption = default;

                // Pour l'instant non utlisée
                StringBuilder sqlquery = new StringBuilder();

                List<ThreadTasks.Task> lstTask = new List<ThreadTasks.Task>();
                List<String> lstSymbolPrisma = new List<string>();

                int lineNumber = 0;
                int guard = 99999999;
                while (++lineNumber < guard)
                {
                    try
                    {
                        // PM 20180219 [23824] IOTools => IOCommonTools
                        //string currentLine = IOTools.StreamReader.ReadLine();
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        AuditLine(lineNumber, currentLine, guard);
                        if (currentLine == null)
                        {
                            // Lorsqu'il existe plusieurs fichiers chez Eurex, seul le dernier se termine par *EOF*
                            // => Ce cas permet de gérer les fichiers où il n'existe pas de *EOF*
                            if ((null != sqlquery) && sqlquery.Length > 0)
                            {
                                DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlquery.ToString());
                                sqlquery = new StringBuilder();
                            }
                            break;
                        }

                        #region Process Ligne par ligne
                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            #region Product
                            case "P"://Product
                                //A chaque nouveau produit, Exécution du SQL non encore exécuté 
                                if ((null != sqlquery) && sqlquery.Length > 0)
                                {
                                    DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlquery.ToString());
                                    sqlquery = new StringBuilder();
                                }
                                parsingRowProduct = null;
                                LoadLine(currentLine, ref parsingRowProduct);

                                productID = parsingRowProduct.GetRowDataValue(Cs, "PRODUCT ID");
                                // PM 20151124 [20124] Devise pour l'import des cotations
                                currency = parsingRowProduct.GetRowDataValue(Cs, "Currency");
                                // PM 20151124 [20124] Tick Size et Tick Value pour l'import du Contract Multiplier
                                productTickSize = DecFunc.DecValueFromInvariantCulture(parsingRowProduct.GetRowDataValue(Cs, "Tick Size"));
                                productTickValue = DecFunc.DecValueFromInvariantCulture(parsingRowProduct.GetRowDataValue(Cs, "Tick Value"));

                                Logger.Log(new LoggerData(LogLevelEnum.Debug, $"Current product: {productID}", 3));

                                isProductOk = _assetETDToImport.CtrSym.Contains(productID);

                                // PM 20151209 [21653] Si le DC ne doit pas est importé, si son CM est absent alors le mettre à jour quand même
                                isDCToUpdate = false;
                                isDCFlexToUpdate = false;
                                if (isProductOk == false)
                                    isDCToUpdate = IsDCToUpdate(productID, DerivativeContractTypeEnum.STD);
                                break;
                            #endregion Product

                            #region Expiry
                            case "E"://Expiry
                                isNewExpiry = true;

                                isUnlPriceForIndexOptionDone = false;
                                assetETDForIndexOption = default;

                                parsingRowExpiry = null;
                                LoadLine(currentLine, ref parsingRowExpiry);
                                break;
                            #endregion Expiry

                            #region Serie
                            case "S"://Serie

                                IOTaskDetInOutFileRow parsingRowSerie = null;
                                LoadLine(currentLine, ref parsingRowSerie);

                                // PM 20221014 [XXXXX] Add assetRequestContractMonthYear
                                MarketAssetETDRequest assetRequestContractMonthYear = GetAssetRequestContractMonthYear(Cs, parsingRowProduct, parsingRowExpiry, parsingRowSerie);
                                // PM 20221014 [XXXXX] Add assetRequestContractDate
                                MarketAssetETDRequest assetRequestContractDate = GetAssetRequestContractDate(Cs, parsingRowProduct, parsingRowExpiry, parsingRowSerie);
                                // Attention: assetRequestNotForMaturityUse doit être utilisé uniquement pour accéder qu'aux données ne concernant pas l'échéance
                                MarketAssetETDRequest assetRequestNotForMaturityUse = assetRequestContractDate;

                                if (false == lstSymbolPrisma.Contains(assetRequestNotForMaturityUse.ContractSymbol))
                                {
                                    lstSymbolPrisma.Add(assetRequestNotForMaturityUse.ContractSymbol);
                                }

                                Boolean isFlexible = (assetRequestNotForMaturityUse.ContractType == DerivativeContractTypeEnum.FLEX);
                                if (isFlexible)
                                {
                                    isProductFlexOk = _assetETDToImport.CtrSym.Contains(assetRequestNotForMaturityUse.ContractSymbol);
                                    // PM 20151209 [21653] Lorsque le contract ne doit pas être importé, vérification si le contract flex est sans CM pour le mettre à jour
                                    if (isProductFlexOk == false)
                                        isDCFlexToUpdate = IsDCToUpdate(assetRequestNotForMaturityUse.ContractSymbol, DerivativeContractTypeEnum.FLEX);
                                }

                                if (isProductOk || isProductFlexOk)
                                {
                                    //  PM 20221014 [XXXXX] Passage de 2 paramètres MarketAssetETDRequest : 1er à partir de ContractDate, 2ème à partir de ContractMonth/ContractYear
                                    Boolean isAssetFound = IsSerieToImport(assetRequestContractDate, assetRequestContractMonthYear, out MarketAssetETDToImport assetETDFound);

                                    string stlPriceValue = parsingRowSerie.GetRowDataValue(Cs, "Settlement Price");

                                    // Importation du cours OfficialSettlement d'un indice s'il existe une option sur indice à échéance
                                    if (StrFunc.IsFilled(stlPriceValue) &&
                                        (assetRequestNotForMaturityUse.ContractCategory == "O") && (assetRequestNotForMaturityUse.SettlementMethod == SettlMethodEnum.CashSettlement)
                                        && (false == isUnlPriceForIndexOptionDone) && (assetETDForIndexOption == default(MarketAssetETDToImport)))
                                    {
                                        if (isAssetFound && (assetETDFound.UnderlyingAssetCategory == Cst.UnderlyingAsset.Index.ToString()))
                                        {
                                            assetETDForIndexOption = assetETDFound;
                                        }
                                        else
                                        {
                                            // Lorsque l'asset n'est pas trouvé, recherche d'un asset négocié Option sur Indice en CashSettlement sur le même Contrat et même Echeance 
                                            assetETDForIndexOption = GetAssetETDForIndexOption(assetRequestContractDate);
                                            if (assetETDForIndexOption == default(MarketAssetETDToImport))
                                            {
                                                // PM 20221024 [XXXXX] Asset non trouvé avec échéance YYYYMMDD, nouvelle recherche avec échéance YYYYMM
                                                assetETDForIndexOption = GetAssetETDForIndexOption(assetRequestContractMonthYear);
                                            }
                                        }

                                        if ((assetETDForIndexOption != default(MarketAssetETDToImport)) && (assetETDForIndexOption.IdAssetUnl > 0))
                                        {
                                            Boolean isExpiryDate = (assetETDForIndexOption.MaturityDate == m_dtFile);

                                            // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement du ssjacent d'une Options sur Indice à échéance
                                            if (isExpiryDate)
                                            {
                                                decimal stlPrice = DecFunc.DecValueFromInvariantCulture(stlPriceValue);
                                                //WARNING: De nombreux prix sont dans le fichier avec une valeur "0.01" ou vide. 
                                                //         Ces prix, a priori non significatif, son ignoré.
                                                if (stlPrice * 100 > 1)
                                                {
                                                    decimal unlStlPrice;
                                                    // usage de assetRequest puisque l'asset en cours n'est pas forcément un asset présent dans Spheres
                                                    if (assetRequestNotForMaturityUse.PutCall == PutOrCallEnum.Put)
                                                        unlStlPrice = assetRequestNotForMaturityUse.StrikePrice - stlPrice;
                                                    else
                                                        unlStlPrice = assetRequestNotForMaturityUse.StrikePrice + stlPrice;

                                                    // Mise à jour du cours OfficialSettlement sur l'Indice
                                                    lstTask.Add(AddAssetPriceAsync(Cst.UnderlyingAsset.Index, assetETDForIndexOption.IdAssetUnl, m_dtFile, QuotationSideEnum.OfficialSettlement, unlStlPrice, currency));

                                                    isUnlPriceForIndexOptionDone = true;
                                                }
                                            }
                                        }
                                    }


                                    // Mise à jour du DC
                                    if (isAssetFound)
                                    {
                                        // Importation du cours OfficialClose d'un asset ETD. 
                                        if (StrFunc.IsFilled(stlPriceValue)) // prix non présent sur les assets à échéance 
                                            lstTask.Add(AddAssetETDPriceAsync(assetETDFound, stlPriceValue, parsingRowSerie.GetRowDataValue(Cs, "Delta"), currency));

                                        // PM 20151124 [20124] Insertion/Mise à jour des cotations des sous-jacents
                                        if (isNewExpiry)
                                        {
                                            // PM 20161019 [22174] Prisma 5.0 : "Underlying close Price" peut ne pas être renseigné
                                            //decimal unlClosePrice = DecFunc.DecValueFromInvariantCulture(parsingRowExpiry.GetRowDataValue(Cs, "Underlying close Price"));
                                            string unlClosePrice = parsingRowExpiry.GetRowDataValue(Cs, "Underlying close Price");
                                            if (StrFunc.IsFilled(unlClosePrice))
                                                lstTask.Add(AddUnderlyingAssetPriceAsync(assetETDFound, unlClosePrice, currency));

                                            isNewExpiry = false;
                                        }

                                        // PM 20151124 [20124] Mise à jour du Contract Multiplier
                                        decimal tradingUnit = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(Cs, "Trading Unit"));
                                        if ((productTickSize != 0) && (productTickValue != 0) && (tradingUnit != 0))
                                        {
                                            // PM 20151124 [20124] Series Version Number et Trading Unit pour mise à jour du Contract Multiplier
                                            lstTask.Add(UpdateDCCMAsync(assetETDFound, productTickValue, productTickSize, tradingUnit));
                                            if (IntFunc.IntValue(assetETDFound.ContractAttribute) > 0)
                                                lstTask.Add(UpdateAssetETDAsync(assetETDFound, productTickValue, productTickSize, tradingUnit));
                                        }
                                    }
                                }
                                else if (isDCToUpdate || isDCFlexToUpdate) // PM 20151209 [21653] Mise à jour des DC sans CM
                                {
                                    int serieVerionNumber = IntFunc.IntValue(assetRequestNotForMaturityUse.ContractAttribute);
                                    if (serieVerionNumber == 0)
                                    {
                                        decimal tradingUnit = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(Cs, "Trading Unit"));
                                        if ((productTickSize != 0) && (productTickValue != 0) && (tradingUnit != 0))
                                        {
                                            IDerivativeContractIdent[] dc = GetInDCCM(assetRequestNotForMaturityUse);
                                            if (ArrFunc.IsFilled(dc))
                                            {
                                                foreach (IDerivativeContractIdent item in dc)
                                                    lstTask.Add(UpdateDCCMAsync(item, productTickValue, productTickSize, tradingUnit));
                                            }
                                            else
                                            {
                                                Logger.Log(new LoggerData(LogLevelEnum.Info, $"Update of contract Multiplier not done.{Cst.CrLf}Derivative contract not found. {GetDerivativeContractRequestLogInfo(assetRequestNotForMaturityUse)}", 3));
                                            }

                                            if (isFlexible)
                                            {
                                                isDCFlexToUpdate = false;
                                            }
                                            else
                                            {
                                                isDCToUpdate = false;
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion Serie

                            case "*EOF*":
                                if ((null != sqlquery) && sqlquery.Length > 0)
                                {
                                    DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlquery.ToString());
                                    sqlquery = new StringBuilder();
                                }
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row: {0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                lstTask.Add(UpdateDatabaseDCCMAsync());
                lstTask.Add(_assetQuote.UpdateDatabaseAsync());

                ThreadTasks.Task.WaitAll(lstTask.ToArray());

                LogDCNotInPRISMA(lstSymbolPrisma);

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                if (null != _assetQuote)
                {
                    foreach (Cst.OTCml_TBL key in _assetQuote.AssetQuote.Keys)
                    {
                        _assetQuote.AssetQuote[key].OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
                        _assetQuote.AssetQuote[key].OnQuoteAdded -= OnQuoteUpdated;
                        _assetQuote.AssetQuote[key].OnQuoteModified -= OnQuoteUpdated;

                    }
                }

                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Mis à jour en base de données des modifications appliquées sur les DC 
        ///  <para>Sollicitation de SpheresQuotationHandling sur les contrats tels que version ==0</para>
        /// </summary>
        /// <returns></returns>
        private async ThreadTasks.Task UpdateDatabaseDCCMAsync()
        {
            await _dcCM.UpdateDatabaseAsync().ContinueWith((r) =>
                ThreadTasks.Task.Run(() =>
                {
                    List<int> lstDC = (from item in r.Result
                                       join idDC in _assetETDToImport.AllAsset.Cast<MarketAssetETDToImport>().Where(y => y.ContractAttribute == "0").Select((x) => { return x.IdDC; }) on Convert.ToInt32(item) equals idDC
                                       select idDC).Distinct().ToList();

                    foreach (int idDc in lstDC)
                    {
                        DataRow row = _dcCM.GetRows().Where(x => Convert.ToInt32(x["IDDC"]) == idDc).FirstOrDefault();
                        // PM 20240122 [WI822] Modification passage des paramètres
                        SendQuotationHandlingContractMultiplierModified((Cst.OTCml_TBL.DERIVATIVECONTRACT, Convert.ToInt32(row["IDDC"])), Convert.ToDecimal(row["CONTRACTMULTIPLIER"]));
                    }
                }
            ));
        }

        /// <summary>
        ///  Mis à jour en mémoire du cours de l'asset ETD <paramref name="asset"/>. Mise à jour Asynchrone de la base de donnée dès 100 cotations ajoutées/modifiées (tous assets confondus)
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="stlPrice"></param>
        /// <param name="delta"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private async ThreadTasks.Task AddAssetETDPriceAsync(MarketAssetETDToImport asset, string stlPrice, string delta, string currency)
        {
            decimal stlPricedec = DecFunc.DecValueFromInvariantCulture(stlPrice);
            Nullable<decimal> deltadec = null;
            if (StrFunc.IsFilled(delta))
                deltadec = DecFunc.DecValueFromInvariantCulture(delta);

            _assetQuote.InsertUpdate_QUOTE_ETD_H(asset.IdAsset, asset.IdM, m_dtFile, QuotationSideEnum.OfficialClose, stlPricedec, currency, deltadec);

            if (_assetQuote.ChangesCount() == 100)
                await _assetQuote.UpdateDatabaseAsync();

        }

        /// <summary>
        /// Mis à jour en mémoire du cours de l'asset <paramref name="idAsset"/>. Mise à jour Asynchrone de la base de donnée dès 100 cotations ajoutées/modifiées (tous assets confondus)
        /// </summary>
        /// <param name="assetCategory"></param>
        /// <param name="idAsset"></param>
        /// <param name="quoteTime"></param>
        /// <param name="quoteSide"></param>
        /// <param name="price"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        /// FI 20221108 [26167] si assetCategory != ExchangeTradedContract => Mise à jour de la base immediate
        private async ThreadTasks.Task AddAssetPriceAsync(Cst.UnderlyingAsset assetCategory, int idAsset, DateTime quoteTime, QuotationSideEnum quoteSide, decimal price, string currency)
        {
            if (assetCategory == Cst.UnderlyingAsset.ExchangeTradedContract)
            {
                _assetQuote.InsertUpdate_QUOTE_XXX_H(assetCategory, idAsset, 0, quoteTime, quoteSide, price, currency);

                if (_assetQuote.ChangesCount() == 100)
                    await _assetQuote.UpdateDatabaseAsync();
            }
            else
            {
                AddAssetPriceSynchrone(assetCategory, idAsset, quoteSide, price, currency);
            }
        }

        /// <summary>
        ///  Mis à jour en mémoire du cours du ss-jacent de l'asset ETD <paramref name="asset"/>. Mise à jour Asynchrone de la base de donnée dès 100 cotations ajoutées/modifiées (tous assets confondus)
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="underlyingPrice"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        /// FI 20221108 [26167] si assetCategory du ssjacent != ExchangeTradedContract => Mise à jour de la base immediate
        private async ThreadTasks.Task AddUnderlyingAssetPriceAsync(MarketAssetETDToImport asset, string underlyingPrice, string currency)
        {
            Cst.UnderlyingAsset underlyingAssetCategory = Cst.ConvertToUnderlyingAsset(asset.UnderlyingAssetCategory);
            int idAssetUnl = (underlyingAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ? asset.IdAssetUnlFuture : asset.IdAssetUnl;
            if (idAssetUnl > 0)
            {
                if (underlyingAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract)
                {
                    _assetQuote.InsertUpdate_QUOTE_XXX_H(underlyingAssetCategory, idAssetUnl, 0, m_dtFile, QuotationSideEnum.OfficialClose, DecFunc.DecValueFromInvariantCulture(underlyingPrice), currency);

                    if (_assetQuote.ChangesCount() == 100)
                        await _assetQuote.UpdateDatabaseAsync();
                }
                else
                {
                    AddAssetPriceSynchrone(underlyingAssetCategory, idAssetUnl, QuotationSideEnum.OfficialClose, DecFunc.DecValueFromInvariantCulture(underlyingPrice), currency);
                }
            }
        }

        /// <summary>
        ///  Mise à jour du contractMultiplier sur l'asset ETD <paramref name="asset"/>
        ///  <para>Sollicitation de SpheresQuotationHandling </para>
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="productTickValue"></param>
        /// <param name="productTickSize"></param>
        /// <param name="tradingUnit"></param>
        /// <returns></returns>
        private async ThreadTasks.Task UpdateAssetETDAsync(MarketAssetETDToImport asset, decimal productTickValue, decimal productTickSize, decimal tradingUnit)
        {
            await ThreadTasks.Task.Run(() =>
            {
                decimal contractMultiplier = CalcContractMultiplier(productTickValue, productTickSize, tradingUnit);
                QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETDMultiplier(Cs, asset.IdAsset, contractMultiplier, task.Process.UserId, dtStart);
                int nbrowsAsset = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateAsset.Query, qryUpdateAsset.Parameters.GetArrayDbParameter());
                if (nbrowsAsset > 0)
                {
                    // Quote Handling pour l'asset
                    // PM 20240122 [WI822] Modification passage des paramètres
                    SendQuotationHandlingContractMultiplierModified((Cst.OTCml_TBL.ASSET_ETD, asset.IdAsset), contractMultiplier);
                }
            });
        }

        /// <summary>
        ///  Mise à jour en mémoire du contractMultiplier sur le DC <paramref name="dc"/>. Mise à jour Asynchrone de la base de donnée dès 20 Modifications appliquées sur les DC
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="productTickValue"></param>
        /// <param name="productTickSize"></param>
        /// <param name="tradingUnit"></param>
        private async ThreadTasks.Task UpdateDCCMAsync(IDerivativeContractIdent dc, decimal productTickValue, decimal productTickSize, decimal tradingUnit)
        {
            decimal minPriceIncr = productTickSize;
            if (StrFunc.IsFilled(dc.ContractAttribute) && IntFunc.IntValue2(dc.ContractAttribute) > 0)
            {
                _dcCM.UpdateDC(dc.IdDC, minPriceIncr);
            }
            else
            {
                decimal contractMultiplier = CalcContractMultiplier(productTickValue, productTickSize, tradingUnit);
                decimal minPriceAmount = productTickValue * tradingUnit;
                _dcCM.UpdateDC(dc.IdDC, contractMultiplier, minPriceAmount, minPriceIncr);
            }

            if (_dcCM.ChangesCount() > 20)
                await UpdateDatabaseDCCMAsync();
        }

        /// <summary>
        ///  Retourne les DCs Spheres qui matchent avec l'asset Eurex
        ///  <para>Next Generation ETD Contracts: Il pourrait éventuellement avoir plusieurs DC Spheres® pour 1 Product Eurex. Cas des contrats Sub-Monthly. Exemple: Le contrat Spheres ODX1 pourrait toujours etre actif alors qu'il n'existe que ODAX sous Eurex</para>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private IDerivativeContractIdent[] GetInDCCM(MarketAssetETDRequest request)
        {
            // Normalement il ne devrait y avoir qu'1 DC mais le code prévoit la possibilité de plusieurs DC comme l'indique le summary
            DerivativeContractIdent[] dc = _dcCM.DerivativeContract.Where(x =>
                    (x.ContractSymbol == request.ContractSymbol) &&
                    (x.ContractType == request.ContractType) &&
                    (x.ContractCategory == request.ContractCategory) &&
                    ((StrFunc.IsFilled(x.ContractAttribute) && x.ContractAttribute == request.ContractAttribute) || (StrFunc.IsEmpty(x.ContractAttribute) && request.ContractAttribute == "0")) &&
                    (x.SettlementMethod == request.SettlementMethod) &&
                    (x.ExerciseStyle == request.ExerciseStyle)
                    ).ToArray();
            return dc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// PM 20181211 [24389][24383] Ajout pour Gestion du cours OfficialSettlement sur les Options sur Indice
        private MarketAssetETDToImport GetAssetETDForIndexOption(MarketAssetETDRequest request)
        {
            MarketAssetETDToImport ret = default;

            if (_assetETDToImport.AssetByCtrSym.TryGetValue(request.ContractSymbol, out List<IAssetETDIdent> assetList))
            {
                ret = assetList.Cast<MarketAssetETDToImport>().FirstOrDefault(a =>

                    a.ContractAttribute == request.ContractAttribute &&
                    a.SettlementMethod == request.SettlementMethod &&
                    a.MaturityMonthYear == request.MaturityMonthYear &&
                    a.ContractCategory == "O" &&
                    a.UnderlyingAssetCategory == Cst.UnderlyingAsset.Index.ToString()
                );
            }

            return ret;
        }

        /// <summary>
        ///  Retourne true si le serie est à importer
        /// </summary>
        /// <param name="requestContractDate"></param>
        /// <param name="requestContractMonthYear"></param>
        /// <param name="oAssetFound">Asset Spheres trouvé</param> 
        /// <returns></returns>
        // PM 20220701 [XXXXX] Ajout recherche par date puis par nom d'échéance
        // PM 20221014 Ajout deuxième paramètre MarketAssetETDRequest, Recherche par Nom d'échéance/ContractDate, puis Date d(échéance/ContractDate et enfin Nom d'échéance/ContractMonthYear
        private Boolean IsSerieToImport(MarketAssetETDRequest requestContractDate, MarketAssetETDRequest requestContractMonthYear, out MarketAssetETDToImport oAssetFound)
        {
            // Recherche de l'asset en premier grace au nom de l'échéance au format YYYYMMDD (à partir de ContractDate)
            oAssetFound = _assetETDToImport.GetAsset(_assetETDRequestSettingsMonthYear, requestContractDate);

            if (null == oAssetFound)
            {
                // Asset non trouvé : nouvelle tentative avec la date d'échéance (à partir de ContractDate)
                oAssetFound = _assetETDToImport.GetAsset(_assetETDRequestSettingsDate, requestContractDate);

                if (null == oAssetFound)
                {
                    // RD 20230403 [26332] Utiliser la recherche format court que pour les Monthly               
                    if (requestContractDate.FrequencyMaturityEnum == ContractFrequencyEnum.Month)
                        // Asset non trouvé : nouvelle tentative avec le nom de l'échéance avec l'ancien format (format court)
                        oAssetFound = _assetETDToImport.GetAsset(_assetETDRequestSettingsMonthYear, requestContractMonthYear);

                    if (null == oAssetFound)
                    {
                        oAssetFound = new MarketAssetETDToImport();
                    }
                }
            }

            return (oAssetFound.IdAsset > 0);
        }

        ///// <summary>
        ///// Sollicitation de SpheresQuotationHandling
        ///// </summary>
        ///// <param name="source">Représente un ASSET_ETD ou un DC</param>
        ///// <param name="contractMultiplier"></param>
        //// PM 20240122 [WI822] Déplacée dans MarketDataImportBase.cs et modification des paramètres
        //private void SendQuotationHandlingContractMultiplierModified(Pair<Cst.OTCml_TBL, int> source, decimal contractMultiplier)
        //{
        //    Quote_ETDAsset quoteETD;
        //    switch (source.First)
        //    {
        //        case Cst.OTCml_TBL.DERIVATIVECONTRACT:
        //            // Quote Handling pour le contract
        //            quoteETD = new Quote_ETDAsset
        //            {
        //                QuoteTable = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(),
        //                idDC = source.Second,
        //                idDCSpecified = true
        //            };
        //            break;
        //        case Cst.OTCml_TBL.ASSET_ETD:
        //            // Quote Handling pour l'asset
        //            quoteETD = new Quote_ETDAsset
        //            {
        //                QuoteTable = Cst.OTCml_TBL.ASSET_ETD.ToString(),
        //                idAsset = source.Second
        //            };
        //            break;
        //        default:
        //            throw new NotSupportedException($"Table: {source.First} not supported");
        //    }

        //    quoteETD.action = DataRowState.Modified.ToString();
        //    quoteETD.contractMultiplier = contractMultiplier;
        //    quoteETD.contractMultiplierSpecified = true;
        //    quoteETD.timeSpecified = true; //FI 20170306 [22225] add
        //    quoteETD.time = m_dtFile;
        //    quoteETD.isCashFlowsVal = true;
        //    quoteETD.isCashFlowsValSpecified = true;

        //    MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = Cs };
            
        //    QuotationHandlingMQueue qHMQueue = new QuotationHandlingMQueue(quoteETD, mQueueAttributes);

        //    IOTools.SendMQueue(task, qHMQueue, Cst.ProcessTypeEnum.QUOTHANDLING);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productTickValue"></param>
        /// <param name="productTickSize"></param>
        /// <param name="tradingUnit"></param>
        /// <returns></returns>
        private static Decimal CalcContractMultiplier(decimal productTickValue, decimal productTickSize, decimal tradingUnit)
        {
            return productTickValue / productTickSize * tradingUnit;
        }

        /// <summary>
        ///  Retourne true s'il existe dans Spheres un DC de version 0 et sans CM pour le contrat/produit <paramref name="contractSymbol"/>, <paramref name="contractType"/>
        /// </summary>
        /// <param name="contractSymbol"></param>
        /// <returns></returns>
        private Boolean IsDCToUpdate(string contractSymbol, DerivativeContractTypeEnum contractType)
        {
            return _dcCM.DerivativeContract.Exists(x => x.ContractSymbol == contractSymbol && x.ContractType == contractType && IntFunc.IntValue(x.ContractAttribute) == 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetRequest"></param>
        /// <returns></returns>
        private static string GetDerivativeContractRequestLogInfo(MarketAssetETDRequest assetRequest)
        {
            string ret = $"Category: <b>{assetRequest.ContractCategory}</b>, Symbol: <b>{assetRequest.ContractSymbol}</b>, Type: <b>{assetRequest.ContractType}</b>, Version: <b>{assetRequest.ContractAttribute}</b>, SettlementMethod: <b>{assetRequest.SettlementMethod}</b>";
            if (assetRequest.ContractCategory == "O")
                ret += $", ExerciseStyle: <b>{assetRequest.ExerciseStyle}</b>";
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        private void LogDCNotInPRISMA(List<string> lstSymbolPrisma)
        {
            IEnumerable<DataRow> lstRowSymbol = from item in _dcCM.GetRows()
                                                join symbol in lstSymbolPrisma on item["CONTRACTSYMBOL"].ToString() equals symbol
                                                select item;

            IEnumerable<DataRow> dr = _dcCM.GetRows().Except(lstRowSymbol);
            foreach (DataRow item in dr)
            {
                Logger.Log(new LoggerData(LogLevelEnum.Info, $"Derivative Contract doesn't exist in PRISMA.{Cst.CrLf}Identifier: <b>{LogTools.IdentifierAndId(item["IDENTIFIER"].ToString(), Convert.ToInt32(item["IDDC"]))}</b>", 3));
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="row"></param>
        //// PM 20240122 [WI822] Déplacée dans MarketDataImportBase
        //private void OnQuoteSourceNotOverridable(object source, QuoteEventArgs e)
        //{
        //    ArrayList message = new ArrayList();
        //    string msgDet = " (source:<b>" + e.DataRow["SOURCE"] + "</b>, rule:NOTOVERRIDABLE)";
        //    message.Insert(0, msgDet);
        //    message.Insert(0, "LOG-06073");
        //    ProcessMapping.LogLogInfo(task.SetErrorWarning, null, message);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="e"></param>
        //// PM 20240122 [WI822] Déplacée dans MarketDataImportBase
        //private void OnQuoteUpdated(object source, QuoteEventArgs e)
        //{
        //    DataRow row = e.DataRow;

        //    string table = $"<b>{e.DataRow.Table.TableName}</b>";
        //    string idAsset = $"<b>{e.DataRow["IDASSET"]}</b> ";
        //    string quoteSide = $"<b>{e.DataRow["QUOTESIDE"]}</b>";
        //    string time = $"<b>{DtFunc.DateTimeToString(Convert.ToDateTime(e.DataRow["TIME"]), DtFunc.FmtISODateTime)}</b>";
        //    string quoteTiming = $"<b>{e.DataRow["QUOTETIMING"]}</b>";
        //    string value = $"<b>{StrFunc.FmtDecimalToCurrentCulture(Convert.ToDecimal(e.DataRow["VALUE"]))}</b>";

        //    string info = string.Empty;

        //    if (row.RowState == DataRowState.Added)
        //        info = $"Quotation Added => Table: {table}, IdAsset: {idAsset}, QuoteSide: {quoteSide}, Time: {time}, QuoteTiming: {quoteTiming}, Value: {value}";
        //    else if (row.RowState == DataRowState.Modified)
        //        info = $"Quotation Modified => IdQuote: <b>{e.DataRow["IDQUOTE_H"]}</b>, Table: {table}, IdAsset: {idAsset}, QuoteSide: {quoteSide}, Time: {time}, QuoteTiming: {quoteTiming}, Value: {value}";

        //    Logger.Log(new LoggerData(LogLevelEnum.Debug, info, 3));
        //}

        /// <summary>
        ///  Ajout/Modification d'un prix avec mise à jour synchrone de la base de donnée
        ///  <para>Méthode adaptée pour la mise à jour de prix de ss-jacent de type EQUITY, INDEX, etc qui peuvent être mis en jour en parallèle par d'autres importations de prix</para>
        /// </summary>
        /// <param name="assetCategory"></param>
        /// <param name="idAsset"></param>
        /// <param name="price"></param>
        /// <param name="currency"></param>
        /// FI 20221108 [26167] Add
        private void AddAssetPriceSynchrone(Cst.UnderlyingAsset assetCategory, int idAsset, QuotationSideEnum quoteSide, decimal price, string currency)
        {
            AssetQuote assetQuote = new AssetQuote(task.Cs, AssetTools.ConvertQuoteEnumToQuoteTbl(AssetTools.ConvertUnderlyingAssetToQuoteEnum(assetCategory)), m_IdMarketEnv, m_IdValScenario, dtStart, task.UserId); ; ; ; ;
            
            assetQuote.OnSourceNotOverridable += OnQuoteSourceNotOverridable;
            assetQuote.OnQuoteAdded += OnQuoteUpdated;
            assetQuote.OnQuoteModified += OnQuoteUpdated;

            assetQuote.LoadQuoteAsset(Cs,m_dtFile, idAsset);
            assetQuote.InsertUpdate_QUOTE_XXX_H(idAsset, 0, m_dtFile, quoteSide, price, currency);
            assetQuote.UpdateDatabase();

            assetQuote.OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
            assetQuote.OnQuoteAdded -= OnQuoteUpdated;
            assetQuote.OnQuoteModified -= OnQuoteUpdated;
        }

        #endregion Methods
    }
}

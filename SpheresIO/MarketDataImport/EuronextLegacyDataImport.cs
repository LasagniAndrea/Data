using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using ThreadingTasks = System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.Enum;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Importation des fichiers de prix et référentiels d'Euronext Legacy
    /// </summary>
    /// PM 20240122 [WI822] New
    internal class EuronextLegacyDataImport : MarketDataImportBase
    {
        #region private class
        /// <summary>
        /// Sous-jacents des dc pour l'importation de leur cours (hors Future)
        /// </summary>
        private sealed class EuronextUnderlying
        {
            /// <summary>
            /// Catégorie de l'Asset sous-jacent
            /// </summary>
            public Cst.UnderlyingAsset UnderlyingAssetCategory { get; set; }
            /// <summary>
            /// Id interne de l'asset sous-jacent
            /// </summary>
            public int IdAssetUnl { get; set; }
            /// <summary>
            /// Code ISIN de l'asset sous-jacent
            /// </summary>
            public string ISINCode { get; set; }
        }

        /// <summary>
        /// Classe de lecture des sous-jacents des dc pour l'importation de leur cours
        /// </summary>
        private sealed class EuronextUnderlyingImportReader : IReaderRow
        {
            #region Methods
            /// <summary>
            /// Requête de selection des assets sous-jacent d'une chambre
            /// </summary>
            /// <param name="pCs"></param>
            /// <param name="pCssIdentifier"></param>
            /// <param name="pDtBusiness"></param>
            /// <returns></returns>
            public static QueryParameters QueryByCss(string pCs, string pCssIdentifier, DateTime pDtBusiness)
            {
                string sqlQuery =
                    $@"select distinct asset.ASSETCATEGORY, asset.IDASSET, asset.ISINCODE
                     from VW_ASSET asset
                    inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = asset.IDASSET) and (dc.ASSETCATEGORY = asset.ASSETCATEGORY)
                    inner join MARKET mk on (mk.IDM = dc.IDM)
                    inner join ACTOR a on (a.IDA = mk.IDA) and (a.IDENTIFIER = '{pCssIdentifier}')
                    where (asset.ISINCODE is not null)
                      and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "dc", "DTFILE")})
                      and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "mk", "DTFILE")})
                      and ({OTCmlHelper.GetSQLDataDtEnabled(pCs, "a", "DTFILE")})";

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
                EuronextUnderlying ret = default;
                if (null != Reader)
                {
                    ret = new EuronextUnderlying()
                    {
                        IdAssetUnl = Convert.ToInt32(Reader["IDASSET"]),
                        UnderlyingAssetCategory = Cst.ConvertToUnderlyingAsset(Reader["ASSETCATEGORY"].ToString()),
                        ISINCode = Reader["ISINCODE"].ToString(),
                    };
                }
                return ret;
            }
            #endregion IReaderRow
        }
        #endregion private class

        #region Members
        /// <summary>
        /// Semaphore de traitement d'un seul fichier des cours à la fois
        /// </summary>
        private static SemaphoreSlim m_LoadOnePriceFileAtATime = new SemaphoreSlim(1, 1);
        
        /// <summary>
        /// Indique s'il faut charger les données (DC, asset) à gérer
        /// </summary>
        private static bool m_IsToLoadAsset = true;

        /// <summary>
        /// Date du dernier fichier importé
        /// </summary>
        private static DateTime m_LastDtFile = DateTime.MinValue;

        /// <summary>
        /// Identifier du Css auquel sont rattaché les marchés des assets pour lesquels importer les données
        /// </summary>
        private static string m_CssIdentifier = default;

        /// <summary>
        /// Gestion des assets ETD (avec leur ss-jacent) dont les cours sont requis
        /// </summary>
        private static MarketDataAssetETDToImport m_AssetETDToImport;

        /// <summary>
        /// Gestion des cotations des assets pour les closing prices
        /// </summary>
        private static MarketAssetQuote m_AssetQuoteClosing;

        /// <summary>
        /// Gestion des cotations des assets pour les settlment prices
        /// </summary>
        private static MarketAssetQuote m_AssetQuoteSettlement;

        /// <summary>
        /// Gestion des cotations des assets pour les deltas
        /// </summary>
        private static MarketAssetQuote m_AssetQuoteDelta;

        /// <summary>
        /// Gestion des Contract Multiplier sur les DC
        /// </summary>
        private static MarketDerivativeContractCM m_DcMult;
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pTask">Tâche IO</param>
        /// <param name="pDataName">Représente le source de donnée</param>
        /// <param name="pDataStyle">Représente le type de donnée en entrée</param>
        public EuronextLegacyDataImport(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle, true, false, AssetFindMaturityEnum.MATURITYDATE, true, false, true, true)
        {
            dtStart = OTCmlHelper.GetDateSysUTC(Cs);

            // Lecture de la date du fichier (ou sinon du paramètre DTBUSINESS)
            if (pDataName != default(string))
            {
                string fileName = Path.GetFileName(pDataName);
                string fileDate = fileName.Substring(0, 8);

                m_dtFile = new DtFunc().StringyyyyMMddToDateTime(fileDate); // Nom du fichier de la forme "yyyyMMdd_rf99.csv"
                if ((DateTime.MinValue == m_dtFile) && (task.IoTask.IsParametersSpecified && task.IoTask.ExistTaskParam("DTBUSINESS")))
                {
                    fileDate = task.IoTask.parameters["DTBUSINESS"];
                    if (false == DateTime.TryParse(fileDate, out m_dtFile))
                    {
                        m_dtFile = DateTime.Today;
                    }
                }
            }
            else
            {
                m_dtFile = DateTime.Today;
            }

            // Forcer le chargement des données en cas de nouvelle date
            m_IsToLoadAsset = (m_dtFile != m_LastDtFile);
            m_LastDtFile = m_dtFile;

            // Initialisation des données pour le premier element de la tâche
            string inputId = pTask.IoTask.taskDet.input.id;
            DataRow firstElement = pTask.Element.FirstOrDefault();
            DataRow lastElement = pTask.Element.LastOrDefault();
            if (m_IsToLoadAsset || (firstElement != default(DataRow)) && (firstElement["IDIOELEMENT"].ToString() == inputId))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Initialisation importation task:{0}", pTask.IoTask.displayname), 3));

                m_AssetETDToImport = default(MarketDataAssetETDToImport);
                m_AssetQuoteClosing = default(MarketAssetQuote);
                m_DcMult = default(MarketDerivativeContractCM);

                // Lecture des assets à gérer
                LoadAsset();

                // Les données sont chargées
                m_IsToLoadAsset = false;
            }

            // Remetre l'indicateur qu'il faut charger les données des assets après le dernier élément
            if ((lastElement != default(DataRow)) && (lastElement["IDIOELEMENT"].ToString() == inputId))
            {
                m_IsToLoadAsset = true;
            }
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Lecture du paramètre CSS de la tâche IO
        /// </summary>
        private void GetCssIdentifier()
        {
            if (task.IoTask.IsParametersSpecified && task.IoTask.ExistTaskParam("CSS"))
            {
                m_CssIdentifier = task.IoTask.parameters["CSS"];
                if (StrFunc.IsEmpty(m_CssIdentifier))
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "<b>The task's CSS parameter is not populated</b>");
                }
            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "<b>The task's CSS parameter is missing</b>");
            }
        }

        /// <summary>
        /// Lecture des assets et cotations à gérer
        /// </summary>
        private void LoadAsset()
        {
            GetCssIdentifier();

            // Initialisation des membres m_IdMarketEnv & m_IdValScenario
            InitDefaultMembers();

            // Lecture des assets dont les cours sont requis
            m_AssetETDToImport = new MarketDataAssetETDToImport(m_CssIdentifier);

            ThreadingTasks.Task taskLoadAsset = m_AssetETDToImport.LoadAssetByCssAsync(Cs, m_dtFile);

            // Lecture des DC pour mise à jour du Contract Multiplier
            m_DcMult = new MarketDerivativeContractCM(m_CssIdentifier, dtStart, task.UserId);
            ThreadingTasks.Task taskLoadDc = m_DcMult.LoadDCByCssAsync(Cs, m_dtFile);

            // Attente fin des taches de lecture
            ThreadingTasks.Task.WaitAll(taskLoadAsset, taskLoadDc);
        }

        /// <summary>
        /// Lecture des cotations à gérer
        /// </summary>
        private void LoadQuoteClosing()
        {
            // Initialisation des membres m_IdMarketEnv & m_IdValScenario
            InitDefaultMembers();

            // Closing prices à gérer
            m_AssetQuoteClosing = new MarketAssetQuote(m_IdMarketEnv, m_IdValScenario, dtStart, task.UserId);

            // Lecture des cotations pour les closing prices
            ThreadingTasks.Task taskLoadCotations = m_AssetQuoteClosing.LoadQuoteAsync(Cs, m_AssetETDToImport, m_dtFile, true);

            // Attente fin de la tache de lecture
            taskLoadCotations.Wait();
        }

        /// <summary>
        /// Lecture des cotations à l'échéance à gérer
        /// </summary>
        private void LoadQuoteExpiry()
        {
            // Initialisation des membres m_IdMarketEnv & m_IdValScenario
            InitDefaultMembers();

            // Closing prices à gérer
            m_AssetQuoteClosing = new MarketAssetQuote(m_IdMarketEnv, m_IdValScenario, dtStart, task.UserId);

            // Settlement prices à gérer
            m_AssetQuoteSettlement = new MarketAssetQuote(m_IdMarketEnv, m_IdValScenario, dtStart, task.UserId);

            // Lecture des cotations pour les closing prices
            ThreadingTasks.Task taskLoadCotations = m_AssetQuoteClosing.LoadQuoteAsync(Cs, m_AssetETDToImport, m_dtFile, true).ContinueWith((r) =>
            {
                    // Lecture des cotations pour les settlement prices
                    m_AssetQuoteSettlement.LoadQuoteAsync(Cs, m_AssetETDToImport, m_dtFile, true).Wait();
            });

            // Attente fin des taches de lecture
            taskLoadCotations.Wait();
        }

        /// <summary>
        /// Lecture des cotations à gérer pour les delta
        /// </summary>
        private void LoadQuoteDelta()
        {
            // Initialisation des membres m_IdMarketEnv & m_IdValScenario
            InitDefaultMembers();

            // Delta à gérer
            m_AssetQuoteDelta = new MarketAssetQuote(m_IdMarketEnv, m_IdValScenario, dtStart, task.UserId);

            // Lecture des cotations pour les deltas
            ThreadingTasks.Task taskLoadCotations = m_AssetQuoteDelta.LoadQuoteAsync(Cs, m_AssetETDToImport, m_dtFile, true);

            // Attente fin de la tache de lecture
            taskLoadCotations.Wait();
        }

        /// <summary>
        ///  Importation des cours, Mise à jour des DC 
        /// </summary>
        /// <param name="pInputSourceDataStyle">Source du flux en entrée</param>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="pNbOmitRowStart">Nombre de lignes à ignorer en début de fichier</param>
        /// <returns></returns>
        public int InputReferentialAndClosingPricesFile(Cst.InputSourceDataStyle pInputSourceDataStyle, InputParsing pInputParsing, int pNbOmitRowStart)
        {
            int lineNumber = 0;
            int guard = 99999999;
            IOTaskDetInOutFileRow parsingRow = default(IOTaskDetInOutFileRow);
            List<ThreadingTasks.Task> lstTask = new List<ThreadingTasks.Task>();

            m_LoadOnePriceFileAtATime.Wait();
            try
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Start importation:{0}", GetFileDesciption()), 3));

                LoadQuoteClosing();

                foreach (Cst.OTCml_TBL key in m_AssetQuoteClosing.AssetQuote.Keys)
                {
                    m_AssetQuoteClosing.AssetQuote[key].OnSourceNotOverridable += new QuoteEventHandler(OnQuoteSourceNotOverridable);
                    m_AssetQuoteClosing.AssetQuote[key].OnQuoteAdded += new QuoteEventHandler(OnQuoteUpdated);
                    m_AssetQuoteClosing.AssetQuote[key].OnQuoteModified += new QuoteEventHandler(OnQuoteUpdated);
                }

                InitDirectImport(pInputParsing);

                OpenInputFileName();
                while (lineNumber++ < guard)
                {
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == default(string))
                    {
                        break;
                    }
                    else if ((lineNumber > pNbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        parsingRow = default(IOTaskDetInOutFileRow);
                        LoadLine(currentLine, ref parsingRow);

                        string contractCategory = parsingRow.GetRowDataValue(Cs, "asset_type");
                        if ((contractCategory == "F") || (contractCategory == "O"))
                        {
                            string contractSymbol = parsingRow.GetRowDataValue(Cs, "symbol_code");

                            bool isContractOk = m_AssetETDToImport.CtrSym.Contains(contractSymbol);

                            // Si le DC n'est pas à importé, mais que son CM est absent alors le mettre à jour quand même
                            bool isDCMultToUpdate = false;
                            if (isContractOk == false)
                            {
                                isDCMultToUpdate = m_DcMult.DerivativeContract.Exists(x => x.ContractSymbol == contractSymbol);
                            }

                            if (isContractOk || isDCMultToUpdate)
                            {
                                MarketAssetETDRequestSettings settings = EuronextVarTools.GetAssetETDRequestSettings(true);
                                MarketAssetETDRequest assetRequest = GetAssetRequest(Cs, parsingRow);

                                if (isContractOk)
                                {
                                    // Recherche de l'asset via le Code Isin
                                    MarketAssetETDToImport assetETD = m_AssetETDToImport.GetAsset(settings, assetRequest);

                                    if (default(MarketAssetETDToImport) == assetETD)
                                    {
                                        // Asset non trouvé avec le code ISIN, recherche de l'asset via ses caractéristiques
                                        settings = EuronextVarTools.GetAssetETDRequestSettings(false, false, true);

                                        // Rechercher de l'asset via ses caractéristiques
                                        assetETD = m_AssetETDToImport.GetAsset(settings, assetRequest);

                                        // L'asset est trouvé et doit être mis à jour avec son code ISIN
                                        if (default(MarketAssetETDToImport) != assetETD)
                                        {
                                            // Mise à jour du code ISIN de l'Asset
                                            lstTask.Add(UpdateAssetETDCodeISINAsync(assetETD, assetRequest.ISINCode));
                                        }
                                    }

                                    if (default(MarketAssetETDToImport) != assetETD)
                                    {
                                        // Asset trouvé
                                        string priceValue = parsingRow.GetRowDataValue(Cs, "price");
                                        string currency = parsingRow.GetRowDataValue(Cs, "instr_curcy");
                                        if (StrFunc.IsFilled(priceValue))
                                        {
                                            // Mise à jour du cours de clôture
                                            lstTask.Add(AddQuoteETDPriceAsync(assetETD, QuotationSideEnum.OfficialClose, priceValue, currency));
                                        }

                                        // Gestion Contract Multiplier
                                        string multiplier = parsingRow.GetRowDataValue(Cs, "mult");
                                        if (StrFunc.IsFilled(multiplier))
                                        {
                                            decimal decMultiplier = DecFunc.DecValueFromInvariantCulture(multiplier);
                                            IDerivativeContractIdent dcForMult = GetInDCCM(assetRequest);
                                            if (dcForMult != default(IDerivativeContractIdent))
                                            {
                                                DataRow dr = m_DcMult.GetRows().FirstOrDefault(x => Convert.ToInt32(x["IDDC"]) == dcForMult.IdDC);
                                                if (dr != default(DataRow))
                                                {
                                                    if (Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]))
                                                    {
                                                        // Mise à jour du ContractMultiplier sur le DC que si celui-ci est null
                                                        lstTask.Add(UpdateDCCMAsync(dcForMult, decMultiplier));
                                                    }
                                                    else if (decMultiplier != Convert.ToDecimal(dr["CONTRACTMULTIPLIER"]))
                                                    {
                                                        // Mise à jour du ContractMultiplier sur les Asset que si différent de celui du DC
                                                        lstTask.Add(UpdateAssetETDMultiplierAsync(assetETD, decMultiplier));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Logger.Log(new LoggerData(LogLevelEnum.Info, $"Update of contract Multiplier not done.{Cst.CrLf}Derivative contract not found. {GetDerivativeContractRequestLogInfo(assetRequest)}", 3));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Gestion Contract Multiplier pour DC sans cours importé pour un asset
                                    string multiplier = parsingRow.GetRowDataValue(Cs, "mult");
                                    if (StrFunc.IsFilled(multiplier))
                                    {
                                        decimal decMultiplier = DecFunc.DecValueFromInvariantCulture(multiplier);
                                        IDerivativeContractIdent dcForMult = GetInDCCM(assetRequest);
                                        if (dcForMult != default(IDerivativeContractIdent))
                                        {
                                            DataRow dr = m_DcMult.GetRows().FirstOrDefault(x => Convert.ToInt32(x["IDDC"]) == dcForMult.IdDC);
                                            if ((dr != default(DataRow)) && Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]))
                                            {
                                                // Mise à jour du ContractMultiplier sur le DC que si celui-ci est null
                                                lstTask.Add(UpdateDCCMAsync(dcForMult, decMultiplier));
                                            }
                                        }
                                        else
                                        {
                                            Logger.Log(new LoggerData(LogLevelEnum.Info, $"Update of contract Multiplier not done.{Cst.CrLf}Derivative contract not found. {GetDerivativeContractRequestLogInfo(assetRequest)}", 3));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                lstTask.Add(UpdateDatabaseDCCMAsync());
                lstTask.Add(m_AssetQuoteClosing.UpdateDatabaseAsync());

                // Attendre la fin de toutes les tâches
                ThreadingTasks.Task.WaitAll(lstTask.ToArray());
            }
            catch { throw; }
            finally
            {
                CloseAllFiles();

                if (default(MarketAssetQuote) != m_AssetQuoteClosing)
                {
                    foreach (Cst.OTCml_TBL key in m_AssetQuoteClosing.AssetQuote.Keys)
                    {
                        m_AssetQuoteClosing.AssetQuote[key].OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
                        m_AssetQuoteClosing.AssetQuote[key].OnQuoteAdded -= OnQuoteUpdated;
                        m_AssetQuoteClosing.AssetQuote[key].OnQuoteModified -= OnQuoteUpdated;
                    }
                }

                m_LoadOnePriceFileAtATime.Release();

                Logger.Log(new LoggerData(LogLevelEnum.Debug, "End importation", 3));
            }
            return lineNumber;
        }

        /// <summary>
        ///  Importation des cours de settlement
        /// </summary>
        /// <param name="pInputSourceDataStyle">Source du flux en entrée</param>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="pNbOmitRowStart">Nombre de lignes à ignorer en début de fichier</param>
        /// <returns></returns>
        public int InputExpiryPricesFile(Cst.InputSourceDataStyle pInputSourceDataStyle, InputParsing pInputParsing, int pNbOmitRowStart)
        {
            int lineNumber = 0;
            int guard = 99999999;
            IOTaskDetInOutFileRow parsingRow = default(IOTaskDetInOutFileRow);
            List<ThreadingTasks.Task> lstTask = new List<ThreadingTasks.Task>();

            m_LoadOnePriceFileAtATime.Wait();
            try
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Start importation:{0}", GetFileDesciption()), 3));

                LoadQuoteExpiry();

                foreach (Cst.OTCml_TBL key in m_AssetQuoteSettlement.AssetQuote.Keys)
                {
                    m_AssetQuoteSettlement.AssetQuote[key].OnSourceNotOverridable += new QuoteEventHandler(OnQuoteSourceNotOverridable);
                    m_AssetQuoteSettlement.AssetQuote[key].OnQuoteAdded += new QuoteEventHandler(OnQuoteUpdated);
                    m_AssetQuoteSettlement.AssetQuote[key].OnQuoteModified += new QuoteEventHandler(OnQuoteUpdated);
                }

                foreach (Cst.OTCml_TBL key in m_AssetQuoteClosing.AssetQuote.Keys)
                {
                    m_AssetQuoteClosing.AssetQuote[key].OnSourceNotOverridable += new QuoteEventHandler(OnQuoteSourceNotOverridable);
                    m_AssetQuoteClosing.AssetQuote[key].OnQuoteAdded += new QuoteEventHandler(OnQuoteUpdated);
                    m_AssetQuoteClosing.AssetQuote[key].OnQuoteModified += new QuoteEventHandler(OnQuoteUpdated);
                }

                InitDirectImport(pInputParsing);

                OpenInputFileName();
                while (lineNumber++ < guard)
                {
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == default(string))
                    {
                        break;
                    }
                    else if ((lineNumber > pNbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        parsingRow = default(IOTaskDetInOutFileRow);
                        LoadLine(currentLine, ref parsingRow);

                        MarketAssetETDRequestSettings settings = EuronextVarTools.GetAssetETDRequestSettings(true, true);
                        MarketAssetETDRequest assetRequest = new MarketAssetETDRequest()
                        {
                            ISINCode = parsingRow.GetRowDataValue(Cs, "instr_id"),
                            PriceCurrency = parsingRow.GetRowDataValue(Cs, "instr_curcy"),
                        };

                        // Recherche de l'asset via le Code Isin
                        MarketAssetETDToImport assetETD = m_AssetETDToImport.GetAsset(settings, assetRequest);

                        if (default(MarketAssetETDToImport) != assetETD)
                        {
                            string stlPrice = parsingRow.GetRowDataValue(Cs, "price");
                            string unlPrice = parsingRow.GetRowDataValue(Cs, "und_price");
                            if (StrFunc.IsFilled(stlPrice))
                            {
                                // Mise à jour du cours de clôture
                                lstTask.Add(AddQuoteETDPriceAsync(assetETD, QuotationSideEnum.OfficialSettlement, stlPrice, assetRequest.PriceCurrency));
                            }
                            if (StrFunc.IsFilled(unlPrice))
                            {
                                // Mise à jour du cours de clôture du sous-jacent
                                lstTask.Add(AddUnderlyingAssetPriceAsync(assetETD, unlPrice, assetRequest.PriceCurrency));
                            }
                        }
                    }
                }
                lstTask.Add(m_AssetQuoteClosing.UpdateDatabaseAsync());
                lstTask.Add(m_AssetQuoteSettlement.UpdateDatabaseAsync());

                ThreadingTasks.Task.WaitAll(lstTask.ToArray());
            }
            catch { throw; }
            finally
            {
                CloseAllFiles();

                if (default(MarketAssetQuote) != m_AssetQuoteClosing)
                {
                    foreach (Cst.OTCml_TBL key in m_AssetQuoteClosing.AssetQuote.Keys)
                    {
                        m_AssetQuoteClosing.AssetQuote[key].OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
                        m_AssetQuoteClosing.AssetQuote[key].OnQuoteAdded -= OnQuoteUpdated;
                        m_AssetQuoteClosing.AssetQuote[key].OnQuoteModified -= OnQuoteUpdated;
                    }
                }
                if (default(MarketAssetQuote) != m_AssetQuoteSettlement)
                {
                    foreach (Cst.OTCml_TBL key in m_AssetQuoteSettlement.AssetQuote.Keys)
                    {
                        m_AssetQuoteSettlement.AssetQuote[key].OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
                        m_AssetQuoteSettlement.AssetQuote[key].OnQuoteAdded -= OnQuoteUpdated;
                        m_AssetQuoteSettlement.AssetQuote[key].OnQuoteModified -= OnQuoteUpdated;
                    }
                }

                m_LoadOnePriceFileAtATime.Release();

                Logger.Log(new LoggerData(LogLevelEnum.Debug, "End importation", 3));
            }
            return lineNumber;
        }

        /// <summary>
        ///  Importation des cours des Stock indices
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="pNbOmitRowStart">Nombre de lignes à ignorer en début de fichier</param>
        /// <returns></returns>
        public int InputStockIndicesValuesFile(InputParsing pInputParsing, int pNbOmitRowStart)
        {
            int lineNumber = 0;
            int guard = 99999999;
            IOTaskDetInOutFileRow parsingRow = default(IOTaskDetInOutFileRow);
            try
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Start importation:{0}", GetFileDesciption()), 3));

                // Initialisation de m_CssIdentifier
                GetCssIdentifier();

                // Initialisation des membres m_IdMarketEnv & m_IdValScenario
                InitDefaultMembers();

                // Lecture des sous-jacent
                List<EuronextUnderlying> lstUnderlying = LoadUnderlying();

                if ((lstUnderlying != default(List<EuronextUnderlying>)) && (lstUnderlying.Count() > 0))
                {
                    InitDirectImport(pInputParsing);

                    OpenInputFileName();
                    while (lineNumber++ < guard)
                    {
                        string currentLine = IOCommonTools.StreamReader.ReadLine();
                        if (currentLine == default(string))
                        {
                            break;
                        }
                        else if ((lineNumber > pNbOmitRowStart) && StrFunc.IsFilled(currentLine))
                        {
                            parsingRow = default(IOTaskDetInOutFileRow);
                            LoadLine(currentLine, ref parsingRow);

                            string ISINCode = parsingRow.GetRowDataValue(Cs, "index_id");
                            var foundUnderlying = lstUnderlying.Where(u => u.ISINCode == ISINCode);
                            if (foundUnderlying.Count() > 0)
                            {
                                string currency = parsingRow.GetRowDataValue(Cs, "index_curcy");
                                string unlPrice = parsingRow.GetRowDataValue(Cs, "value");
                                if (StrFunc.IsFilled(unlPrice))
                                {
                                    decimal decUnlPrice = DecFunc.DecValueFromInvariantCulture(unlPrice);
                                    foreach (EuronextUnderlying unl in foundUnderlying)
                                    {
                                        AddUnderlyingAssetPrice(unl, decUnlPrice, currency);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { throw; }
            finally
            {
                CloseAllFiles();

                Logger.Log(new LoggerData(LogLevelEnum.Debug, "End importation", 3));
            }
            return lineNumber;
        }

        /// <summary>
        ///  Importation des Deltas 
        /// </summary>
        /// <param name="pInputSourceDataStyle">Source du flux en entrée</param>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="pNbOmitRowStart">Nombre de lignes à ignorer en début de fichier</param>
        /// <returns></returns>
        public int InputOptionsDeltasFile(Cst.InputSourceDataStyle pInputSourceDataStyle, InputParsing pInputParsing, int pNbOmitRowStart)
        {
            int lineNumber = 0;
            int guard = 99999999;
            IOTaskDetInOutFileRow parsingRow = default(IOTaskDetInOutFileRow);
            List<ThreadingTasks.Task> lstTask = new List<ThreadingTasks.Task>();

            m_LoadOnePriceFileAtATime.Wait();
            try
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Start importation:{0}", GetFileDesciption()), 3));

                // Lecture des cotations pour les deltas
                LoadQuoteDelta();

                foreach (Cst.OTCml_TBL key in m_AssetQuoteDelta.AssetQuote.Keys)
                {
                    m_AssetQuoteDelta.AssetQuote[key].OnSourceNotOverridable += new QuoteEventHandler(OnQuoteSourceNotOverridable);
                    m_AssetQuoteDelta.AssetQuote[key].OnQuoteAdded += new QuoteEventHandler(OnQuoteUpdated);
                    m_AssetQuoteDelta.AssetQuote[key].OnQuoteModified += new QuoteEventHandler(OnQuoteUpdated);
                }

                InitDirectImport(pInputParsing);

                OpenInputFileName();
                while (lineNumber++ < guard)
                {
                    string currentLine = IOCommonTools.StreamReader.ReadLine();
                    if (currentLine == default(string))
                    {
                        break;
                    }
                    else if ((lineNumber > pNbOmitRowStart) && StrFunc.IsFilled(currentLine))
                    {
                        parsingRow = default(IOTaskDetInOutFileRow);
                        LoadLine(currentLine, ref parsingRow);

                        MarketAssetETDRequestSettings settings = EuronextVarTools.GetAssetETDRequestSettings(true, true);
                        MarketAssetETDRequest assetRequest = new MarketAssetETDRequest()
                        {
                            ISINCode = parsingRow.GetRowDataValue(Cs, "instr_id"),
                            PriceCurrency = parsingRow.GetRowDataValue(Cs, "instr_curcy")
                        };

                        // Recherche de l'asset via le Code Isin
                        MarketAssetETDToImport assetETD = m_AssetETDToImport.GetAsset(settings, assetRequest);

                        if (default(MarketAssetETDToImport) != assetETD)
                        {
                            string delta = parsingRow.GetRowDataValue(Cs, "delta");
                            if (StrFunc.IsFilled(delta))
                            {
                                // Mise à jour du delta
                                lstTask.Add(AddQuoteETDDeltaAsync(assetETD, delta, assetRequest.PriceCurrency));
                            }
                        }
                    }
                }
                lstTask.Add(m_AssetQuoteDelta.UpdateDatabaseAsync());

                ThreadingTasks.Task.WaitAll(lstTask.ToArray());
            }
            catch { throw; }
            finally
            {
                CloseAllFiles();

                if (default(MarketAssetQuote) != m_AssetQuoteDelta)
                {
                    foreach (Cst.OTCml_TBL key in m_AssetQuoteDelta.AssetQuote.Keys)
                    {
                        m_AssetQuoteDelta.AssetQuote[key].OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
                        m_AssetQuoteDelta.AssetQuote[key].OnQuoteAdded -= OnQuoteUpdated;
                        m_AssetQuoteDelta.AssetQuote[key].OnQuoteModified -= OnQuoteUpdated;
                    }
                }

                m_LoadOnePriceFileAtATime.Release();

                Logger.Log(new LoggerData(LogLevelEnum.Debug, "End importation", 3));
            }
            return lineNumber;
        }

        /// <summary>
        /// Construction des données de la requête de recherche d'un asset
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pPparsingRow"></param>
        /// <returns></returns>
        private MarketAssetETDRequest GetAssetRequest(string pCs, IOTaskDetInOutFileRow pPparsingRow)
        {
            string maturityDate = pPparsingRow.GetRowDataValue(pCs, "mat_dt");
            DateTime dtMatDate = new DtFunc().StringyyyyMMddToDateTime(maturityDate);
            string contractCategory = pPparsingRow.GetRowDataValue(pCs, "asset_type");
            string optionType = pPparsingRow.GetRowDataValue(pCs, "option_type");
            PutOrCallEnum? putCallEnum;

            if ((contractCategory != "F") && (contractCategory != "O"))
            {
                contractCategory = default(string);
            }

            switch (optionType)
            {
                case "C":
                    putCallEnum = PutOrCallEnum.Call;
                    break;
                case "P":
                    putCallEnum = PutOrCallEnum.Put;
                    break;
                default:
                    putCallEnum = null;
                    break;
            }
                
            MarketAssetETDRequest request = new MarketAssetETDRequest()
            {
                ISINCode = pPparsingRow.GetRowDataValue(pCs, "instr_id"),
                PriceCurrency = pPparsingRow.GetRowDataValue(pCs, "settl_curcy"),
                ContractSymbol = pPparsingRow.GetRowDataValue(pCs, "symbol_code"),
                ContractType = DerivativeContractTypeEnum.STD,
                // ContractAttribute = n/a,
                ContractCategory = contractCategory,
                SettlementMethod = (pPparsingRow.GetRowDataValue(pCs, "settl_type") == "P") ? SettlMethodEnum.PhysicalSettlement : SettlMethodEnum.CashSettlement,
                // ExerciseStyle = n/a,
                PutCall = putCallEnum,
                StrikePrice = DecFunc.DecValue(pPparsingRow.GetRowDataValue(pCs, "strike")),
                MaturityDate = dtMatDate,
                ContractMultiplier = DecFunc.DecValue(pPparsingRow.GetRowDataValue(pCs, "mult")),
            };
            return request;
        }

        /// <summary>
        /// Retourne le DC Spheres qui matche avec l'asset
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private IDerivativeContractIdent GetInDCCM(MarketAssetETDRequest request)
        {
            DerivativeContractIdent dc = m_DcMult.DerivativeContract.Where(x =>
                    (x.ContractSymbol == request.ContractSymbol) &&
                    (x.ContractType == request.ContractType) &&
                    (x.ContractCategory == request.ContractCategory) &&
                    (x.SettlementMethod == request.SettlementMethod)
                    ).FirstOrDefault();
            return dc;
        }

        /// <summary>
        ///  Mis à jour en mémoire du cours de l'asset ETD <paramref name="pAsset"/>.
        ///  Mise à jour Asynchrone de la base de donnée dès 100 cotations ajoutées/modifiées (tous assets confondus)
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        private async ThreadingTasks.Task AddQuoteETDPriceAsync(MarketAssetETDToImport pAsset, QuotationSideEnum pQuoteSide, string pPrice, string pCurrency)
        {
            decimal priceDec = DecFunc.DecValueFromInvariantCulture(pPrice);

            switch (pQuoteSide)
            {
                case QuotationSideEnum.OfficialClose:
                    m_AssetQuoteClosing.InsertUpdate_QUOTE_ETD_H(pAsset.IdAsset, pAsset.IdM, m_dtFile, pQuoteSide, priceDec, pCurrency, null);

                    if (m_AssetQuoteClosing.ChangesCount() == 100)
                    {
                        await m_AssetQuoteClosing.UpdateDatabaseAsync();
                    }
                    break;
                case QuotationSideEnum.OfficialSettlement:
                    m_AssetQuoteSettlement.InsertUpdate_QUOTE_ETD_H(pAsset.IdAsset, pAsset.IdM, m_dtFile, pQuoteSide, priceDec, pCurrency, null);

                    if (m_AssetQuoteSettlement.ChangesCount() == 100)
                    {
                        await m_AssetQuoteSettlement.UpdateDatabaseAsync();
                    }
                    break;
            }
        }

        /// <summary>
        ///  Mis à jour en mémoire du delta de l'asset ETD <paramref name="pAsset"/>.
        ///  Mise à jour Asynchrone de la base de donnée dès 100 deltas ajoutées/modifiées (tous assets confondus)
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pDelta"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        private async ThreadingTasks.Task AddQuoteETDDeltaAsync(MarketAssetETDToImport pAsset, string pDelta, string pCurrency)
        {
            decimal deltaDec = DecFunc.DecValueFromInvariantCulture(pDelta);

            m_AssetQuoteDelta.InsertUpdate_QUOTE_ETD_H(pAsset.IdAsset, pAsset.IdM, m_dtFile, QuotationSideEnum.OfficialClose, null, pCurrency, deltaDec);

            if (m_AssetQuoteDelta.ChangesCount() == 100)
            {
                await m_AssetQuoteDelta.UpdateDatabaseAsync();
            }
        }

        /// <summary>
        ///  Mise à jour en mémoire du contractMultiplier sur le DC <paramref name="pDc"/>
        ///  Mise à jour Asynchrone de la base de donnée dès 20 modifications appliquées sur les DC
        /// </summary>
        /// <param name="pDc"></param>
        /// <param name="pMultiplier"></param>
        private async ThreadingTasks.Task UpdateDCCMAsync(IDerivativeContractIdent pDc, decimal pMultiplier)
        {
            m_DcMult.UpdateDCMultiplier(pDc.IdDC, pMultiplier);
            
            if (m_DcMult.ChangesCount() > 20)
            {
                await UpdateDatabaseDCCMAsync();
            }
        }

        /// <summary>
        /// Mis à jour en base de données des modifications appliquées sur les DC 
        /// <para>Sollicitation de SpheresQuotationHandling</para>
        /// </summary>
        /// <returns></returns>
        private async ThreadingTasks.Task UpdateDatabaseDCCMAsync()
        {
            await m_DcMult.UpdateDatabaseAsync().ContinueWith((r) =>
                ThreadingTasks.Task.Run(() =>
                {
                    List<int> lstDC = (from item in r.Result
                                       join idDC in m_AssetETDToImport.AllAsset.Cast<MarketAssetETDToImport>().Select((x) => { return x.IdDC; }) on Convert.ToInt32(item) equals idDC
                                       select idDC).Distinct().ToList();

                    foreach (int idDc in lstDC)
                    {
                        DataRow row = m_DcMult.GetRows().Where(x => Convert.ToInt32(x["IDDC"]) == idDc).FirstOrDefault();
                        if (row != default(DataRow))
                        {
                            SendQuotationHandlingContractMultiplierModified((Cst.OTCml_TBL.DERIVATIVECONTRACT, Convert.ToInt32(row["IDDC"])), Convert.ToDecimal(row["CONTRACTMULTIPLIER"]));
                        }
                    }
                }
            ));
        }

        /// <summary>
        /// Mise à jour du contractMultiplier sur l'asset ETD <paramref name="pAsset"/>
        /// <para>Sollicitation de SpheresQuotationHandling</para>
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pMultiplier"></param>
        /// <returns></returns>
        private async ThreadingTasks.Task UpdateAssetETDMultiplierAsync(MarketAssetETDToImport pAsset, decimal pMultiplier)
        {
            await ThreadingTasks.Task.Run(() =>
            {
                QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETDMultiplier(Cs, pAsset.IdAsset, pMultiplier, task.Process.UserId, dtStart);
                int nbrowsAsset = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateAsset.Query, qryUpdateAsset.Parameters.GetArrayDbParameter());
                if (nbrowsAsset > 0)
                {
                    // Quote Handling pour l'asset
                    SendQuotationHandlingContractMultiplierModified((Cst.OTCml_TBL.ASSET_ETD, pAsset.IdAsset), pMultiplier);
                }
            });
        }

        /// <summary>
        /// Mise à jour du code ISIN sur l'asset ETD <paramref name="pAsset"/>
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pCodeISIN"></param>
        /// <returns></returns>
        private async ThreadingTasks.Task UpdateAssetETDCodeISINAsync(MarketAssetETDToImport pAsset, string pCodeISIN)
        {
            await ThreadingTasks.Task.Run(() =>
            {
                QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETDCodeISIN(Cs, pAsset.IdAsset, pCodeISIN, task.Process.UserId, dtStart);
                int nbrowsAsset = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateAsset.Query, qryUpdateAsset.Parameters.GetArrayDbParameter());
            });
        }

        /// <summary>
        ///  Mis à jour du cours du ss-jacent de l'asset ETD
        /// </summary>
        /// <param name="pUnderlyingAsset"></param>
        /// <param name="pUunderlyingPrice"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        private void AddUnderlyingAssetPrice(EuronextUnderlying pUnderlyingAsset, decimal pUunderlyingPrice, string pCurrency)
        {
            if (pUnderlyingAsset != default(EuronextUnderlying))
            {
                AddUnderlyingAssetPrice(pUnderlyingAsset.UnderlyingAssetCategory, pUnderlyingAsset.IdAssetUnl, QuotationSideEnum.OfficialClose, pUunderlyingPrice, pCurrency);
            }
        }

        /// <summary>
        ///  Ajout/Modification d'un cours avec mise à jour synchrone de la base de donnée
        ///  <para>Méthode adaptée pour la mise à jour de prix de ss-jacent de type EQUITY, INDEX, etc qui peuvent être mis en jour en parallèle par d'autres importations de prix</para>
        /// </summary>
        /// <param name="pUnlAssetCategory"></param>
        /// <param name="pIdUnlAsset"></param>
        /// <param name="pUnlPrice"></param>
        /// <param name="pCurrency"></param>
        private void AddUnderlyingAssetPrice(Cst.UnderlyingAsset pUnlAssetCategory, int pIdUnlAsset, QuotationSideEnum pQuoteSide, decimal pUnlPrice, string pCurrency)
        {
            AssetQuote assetQuote = new AssetQuote( task.Cs , AssetTools.ConvertQuoteEnumToQuoteTbl(AssetTools.ConvertUnderlyingAssetToQuoteEnum(pUnlAssetCategory)), m_IdMarketEnv, m_IdValScenario, dtStart, task.UserId);

            assetQuote.OnSourceNotOverridable += OnQuoteSourceNotOverridable;
            assetQuote.OnQuoteAdded += OnQuoteUpdated;
            assetQuote.OnQuoteModified += OnQuoteUpdated;

            assetQuote.LoadQuoteAsset(Cs, m_dtFile, pIdUnlAsset);
            assetQuote.InsertUpdate_QUOTE_XXX_H(pIdUnlAsset, 0, m_dtFile, QuotationSideEnum.OfficialClose, pUnlPrice, pCurrency);
            assetQuote.UpdateDatabase();

            assetQuote.OnSourceNotOverridable -= OnQuoteSourceNotOverridable;
            assetQuote.OnQuoteAdded -= OnQuoteUpdated;
            assetQuote.OnQuoteModified -= OnQuoteUpdated;
        }

        /// <summary>
        ///  Mise à jour du cours du ss-jacent de l'asset ETD <paramref name="asset"/>.
        ///  Mise à jour Asynchrone de la base de donnée dès 100 cotations ajoutées/modifiées pour les Futures
        ///  Mise à jour de la base immediate pour assetCategory du sousjacent diférent de Futures 
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pUnlPrice"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        private async ThreadingTasks.Task AddUnderlyingAssetPriceAsync(MarketAssetETDToImport pAsset, string pUnlPrice, string pCurrency)
        {
            Cst.UnderlyingAsset underlyingAssetCategory = Cst.ConvertToUnderlyingAsset(pAsset.UnderlyingAssetCategory);
            bool isUnlFuture = ((underlyingAssetCategory == Cst.UnderlyingAsset.Future) || (underlyingAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract));
            int idAssetUnl = (isUnlFuture ? pAsset.IdAssetUnlFuture : pAsset.IdAssetUnl);
            if (idAssetUnl > 0)
            {
                if (isUnlFuture)
                {
                    m_AssetQuoteClosing.InsertUpdate_QUOTE_XXX_H(underlyingAssetCategory, idAssetUnl, 0, m_dtFile, QuotationSideEnum.OfficialClose, DecFunc.DecValueFromInvariantCulture(pUnlPrice), pCurrency);

                    if (m_AssetQuoteClosing.ChangesCount() == 100)
                    {
                        await m_AssetQuoteClosing.UpdateDatabaseAsync();
                    }
                }
                else
                {
                    AddUnderlyingAssetPrice(underlyingAssetCategory, idAssetUnl, QuotationSideEnum.OfficialClose, DecFunc.DecValueFromInvariantCulture(pUnlPrice), pCurrency);
                }
            }
        }

        /// <summary>
        /// Lecture des sous-jacent (hors future)
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pCssIdentifier"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        private List<EuronextUnderlying> LoadUnderlying()
        {
            List<EuronextUnderlying> lstUnderlying = new List<EuronextUnderlying>();

            QueryParameters querySql = EuronextUnderlyingImportReader.QueryByCss(Cs, m_CssIdentifier, m_dtFile);

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, querySql.Query.ToString(), querySql.Parameters.GetArrayDbParameter()))
            {
                lstUnderlying = ((IEnumerable<EuronextUnderlying>)dr.DataReaderEnumerator<EuronextUnderlying, EuronextUnderlyingImportReader>()).ToList();
            }

            return lstUnderlying;
        }
        
        /// <summary>
        /// Log des info de la requête de recherche d'un asset
        /// </summary>
        /// <param name="pAssetRequest"></param>
        /// <returns></returns>
        private static string GetDerivativeContractRequestLogInfo(MarketAssetETDRequest pAssetRequest)
        {
            string ret = $"Category: <b>{pAssetRequest.ContractCategory}</b>, Symbol: <b>{pAssetRequest.ContractSymbol}</b>, Type: <b>{pAssetRequest.ContractType}</b>, SettlementMethod: <b>{pAssetRequest.SettlementMethod}</b>";
            return ret;
        }
        #endregion Methods
    }
}

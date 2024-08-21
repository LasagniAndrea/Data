using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
//
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
using EFS.SpheresIO.Properties;
//
using EfsML;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.v30.Fix;
//
using FixML.Enum;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;



namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Configuration de l'importation des fichiers PRISMA dans IMPRISMA_H
    /// </summary>
    [Obsolete("l'importation dans les tables PRISMA_H n'est plus nécessaire")]
    internal class PrismaInputFileConfig
    {
        /// <summary>
        /// Taille Max par défaut des requêtes SQL
        /// </summary>
//        private const int defaultSQLLimit = 60000;
        private const int defaultSQLLimit = 60000;

        /// <summary>
        /// Obtient le nombre max de produit à importer
        /// <para>L'importation s'arrête dès que ce nombre est atteint</para>
        /// </summary>
        public int NbProductMax
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la liste restrictive des produits à importer
        /// <para>L'importation s'arrête dès que les produits de la liste sont importés</para>
        /// </summary>
        public ArrayList RestrictProduct
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définie la directive qui active la recherche du produit dans Spheres®
        /// <para>Si true seuls les produits négociés sont importés</para>
        /// </summary>
        public Boolean IsSearchProduct
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient ou définie la directive qui active la recherche de la serie dans Spheres®
        /// <para>Si true seuls les séries négociés sont importés</para>
        /// </summary>
        public Boolean IsSearchSerie
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient ou définit la taille max des ordres SQL.Dès qu'un ordre SQL dépasse cette limite il est exécuté 
        /// <para>Définir -1 pour exécuter les requêtes 1 à 1</para>
        /// </summary>
        public int SqlLimitSize
        {
            get;
            set;
        }

        #region constructor
        /// <summary>
        /// Configuration par Défaut
        /// <para>L'importation de charge uniquement les produits et séries existants dans Spheres®</para>
        /// <para>La taille des requêtes ne dépasse pas la taille définie ds defaultSQLLimit</para>
        /// </summary>
        public PrismaInputFileConfig()
        {
            IsSearchProduct = true;
            IsSearchSerie = true;
            RestrictProduct = new ArrayList();
            NbProductMax = -1;
            SqlLimitSize = defaultSQLLimit;
        }

        /// <summary>
        /// Importation des produits de la liste
        /// <para>Les produits sont importés uniquement s'ils existent dans Speheres®</para>
        /// <para>La taille des requêtes ne dépasse pas la taille définie ds defaultSQLLimit</para>
        /// </summary>
        /// <param name="pRestrictProduct"></param>
        public PrismaInputFileConfig(ICollection pRestrictProduct)
            : this()
        {
            RestrictProduct = new ArrayList(pRestrictProduct);
            NbProductMax = ArrFunc.Count(pRestrictProduct);
        }

        /// <summary>
        /// Importation des {pNbProduct} premiers product du fichier
        /// <para>Les produits sont importés uniquement s'ils existent dans Speheres®</para>
        /// <para>La taille des requêtes ne dépasse pas la taille définie ds defaultSQLLimit</para>
        /// </summary>
        /// <param name="pNbProduct"></param>
        public PrismaInputFileConfig(int pNbProduct)
            : this()
        {
            NbProductMax = pNbProduct;
        }
        #endregion
    }

    /// <summary>
    /// Permet l'importation des fichiers Eurex PRISMA dans IMPRISMA_H et l'Importation des cours, Mise à jour des DC
    /// Pour l'mportation des cours, Mise à jour des DC, etc il convient d'utiliser <seealso cref="MarketDataImportEurexPrismaSTLPrices"/>
    /// </summary>
    [Obsolete("l'importation dans les tables PRISMA_H n'est plus nécessaire. Pour l'importation des cours, utiliser la classe MarketDataImportEurexPrismaSTLPrices")]
    internal class MarketDataImportEurexPrisma : MarketDataImportEurexPrismaBase
    {
        #region members
        /// <summary>
        /// Identifiant non significatif d'une date de traitement (voir table IMPRISMA_H)
        /// </summary>
        private int idIMPRISMA_H;

        // PM 20151216 [21662] Ajout IsEurosysSoftware
        protected bool IsEurosysSoftware { get; private set; } // Indique si l'import est à destination d'Eurosys Futures ou non
        // PM 20180208 [CHEETAH] Ajout pour gestion asynchrone
        private static SemaphoreSlim m_PrismaSemaphore;
        // PM 20180208 [CHEETAH] Gestion asynchrone
        private static bool m_IsAsync;
        // PM 20180219 [23824] Ajout
        protected MarketDataAssetETDToImport _assetETDToImport;
        // FI 20220321 [XXXXX] Add
        protected MarketAssetETDRequestSettings _assetETDRequestSettings;
        #endregion members

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pTask">Tâche IO</param>
        /// <param name="pDataName">Représente le source de donnée</param>
        /// <param name="pDataStyle">Représente le type de donnée en entrée</param>
        public MarketDataImportEurexPrisma(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle)
        {
            // PM 20151216 [21662] Ajout gestion IsEurosysSoftware
            if ((pTask != default(Task)) && pTask.IoTask.IsParametersSpecified && pTask.IoTask.ExistTaskParam("SOFTWARE"))
            {
                string software = pTask.IoTask.parameters["SOFTWARE"];
                IsEurosysSoftware = (software == Software.SOFTWARE_EurosysFutures);
            }

            // PM 20180208 [CHEETAH] Ajout pour gestion asynchrone
            m_IsAsync = (Settings.Default.AsyncLevel > 0);
            if (m_IsAsync)
            {
                m_PrismaSemaphore = new SemaphoreSlim(Settings.Default.AsyncLevel);
            }

            // PM 20180219 [23824] Ajout
            _assetETDToImport = new MarketDataAssetETDToImport(new Tuple<MarketColumnIdent, string>(MarketColumnIdent.EXCHANGEACRONYM, ExchangeAcronym));

            // PM 20220107 [XXXXX] Ajout paramètre AssetETDRequestMaturityMode.MaturityMonthYear pour rétrocompatibilité
            _assetETDRequestSettings = PrismaTools.GetAssetETDRequestSettings(AssetETDRequestMaturityMode.MaturityMonthYear);

        }
        #endregion Constructor

        #region Methods

        /// <summary>
        ///  Importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>

        /// <summary>
        ///  Importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        /// FI 20140618 [19911] Corrections diverses pour gérer les flex
        /// FI 20151123 [21576] modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputTheoreticalPriceFile(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                // PM 20180208 [CHEETAH] Liste des tâches lancées en asynchrone
                List<System.Threading.Tasks.Task> executeTask = new List<System.Threading.Tasks.Task>();

                StartPrismaImport(pInputParsing);

                PrismaInputFileConfig config = InitTheoreticalPriceFileConfig();
                // Delete dans 2 cas de figure
                // 1er cas : le fichier se termine par 0001_dddd
                // Ceci afin de gérer le cas où Eurex génère 2 fichiers  (le 1er se termine par 0001_0002 et le 2nd par 0001_0002)
                // => Il faudra importer manuellement le 2nd fichier, il n'y aura pas de delete qui supprimerait alors les infos du 1er fichier
                // 2nd cas : le fichier se termine par 9999_9999
                // Ceci afin de gérer le cas où spheres concatène les n fichiers présents (voir ticket 19081)
                bool isDelete = false;
                string extention = Path.GetExtension(dataName);
                // PM 20170524 [22834][23078] Gestion 99_99 en plus de 9999_9999
                //Regex regEx = new Regex(@"0001_\d{4}" + extention + "$", RegexOptions.IgnoreCase);
                // FI 20151123 [21576] Modify
                //if ((regEx.IsMatch(dataName)) || dataName.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention)))
                Regex regEx = new Regex(@"(0001_\d{4})|(01_\d{2})" + extention + "$", RegexOptions.IgnoreCase);
                if ((regEx.IsMatch(dataName))
                    || dataName.EndsWith(StrFunc.AppendFormat("9999_9999{0}", extention))
                    || dataName.EndsWith(StrFunc.AppendFormat("99_99{0}", extention)))
                {
                    isDelete = true;
                }

                if (isDelete)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                    Delete_TheoreticalPriceFile();
                }

                Boolean isProductOk = false;
                Boolean isSerieOk = false;
                Boolean isProductIDInTrade = false;
                int idAsset = 0;
                ArrayList alProductOk = new ArrayList();

                StringBuilder sqlSP = new StringBuilder();
                StringBuilder sqlCE = new StringBuilder();
                StringBuilder sqlVAR = new StringBuilder();

                DataRow dataRowProduct = null;
                DataRow dataRowSerie = null;
                DataRow dataRowLGS = null;
                DataRow dataRowFxRate = null;
                DataRow dataRowLGSOfSerie = null;
                DataRow dataRowRMSOfLGS = null;
                DataRow dataRowRMSOfLGSOfSerie = null;

                IOTaskDetInOutFileRow parsingRowProduct = null;
                IOTaskDetInOutFileRow parsingRowExpiry = null;
                IOTaskDetInOutFileRow parsingRowSerie = null;
                IOTaskDetInOutFileRow parsingRowNeutralScenario = null;
                IOTaskDetInOutFileRow parsingRowLGS = null;
                IOTaskDetInOutFileRow parsingRowRMS = null;
                IOTaskDetInOutFileRow parsingRowLH = null;
                IOTaskDetInOutFileRow parsingRowFxSet = null;

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

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
                            // PM 20180208 [CHEETAH] Gestion asynchrone
                            if (m_IsAsync)
                            {
                                executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(Cs, sqlSP, sqlCE, sqlVAR));
                            }
                            else
                            {
                                ExecuteQueryTheoriticalPriceFile(Cs, sqlSP, sqlCE, sqlVAR);
                            }
                            sqlSP = new StringBuilder();
                            sqlCE = new StringBuilder();
                            sqlVAR = new StringBuilder();
                            break;
                        }

                        #region Process Ligne par ligne
                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            case "P"://Product
                                //A chaque nouveau produit, Exécution du SQL non encore exécuté
                                // PM 20180208 [CHEETAH] Gestion asynchrone
                                if (m_IsAsync)
                                {
                                    executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(Cs, sqlSP, sqlCE, sqlVAR));
                                }
                                else
                                {
                                    ExecuteQueryTheoriticalPriceFile(Cs, sqlSP, sqlCE, sqlVAR);
                                }
                                sqlSP = new StringBuilder();
                                sqlCE = new StringBuilder();
                                sqlVAR = new StringBuilder();

                                parsingRowProduct = null;
                                dataRowProduct = null;
                                LoadLine(currentLine, ref parsingRowProduct);

                                string productID = parsingRowProduct.GetRowDataValue(Cs, "PRODUCT ID");
                                isProductOk = IsProductToImport(config, productID, alProductOk);
                                if (isProductOk)
                                {
                                    alProductOk.Add(productID);
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for product:{0}", productID), 3));

                                    if (config.IsSearchProduct)
                                    {
                                        isProductIDInTrade = IsExistDcInTrade(productID);
                                    }
                                }
                                break;

                            case "E"://Expiry
                                parsingRowExpiry = null;
                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                // => Toujours parser les Expiry
                                //if (isProductOk)
                                //{
                                LoadLine(currentLine, ref parsingRowExpiry);
                                //}
                                break;

                            case "S"://Serie
                                parsingRowSerie = null;
                                isSerieOk = false;
                                idAsset = 0;

                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                // => Vérifier le Product Flex
                                LoadLine(currentLine, ref parsingRowSerie);
                                if (false == isProductOk)
                                {
                                    Boolean isFlexible = BoolFunc.IsTrue(parsingRowSerie.GetRowDataValue(Cs, "Flex Series Flag"));
                                    if (isFlexible)
                                    {
                                        string flexProductID = parsingRowSerie.GetRowDataValue(Cs, "Flex Product ID");
                                        isProductOk = IsProductToImport(config, flexProductID, alProductOk);
                                        if (isProductOk)
                                        {
                                            if (config.IsSearchProduct)
                                            {
                                                // Vérifier qu'il existe des trade sur le contrat Flex 
                                                isProductOk = IsExistDcInTrade(flexProductID);
                                            }
                                            if (isProductOk)
                                            {
                                                alProductOk.Add(flexProductID);

                                                
                                                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for flexible product:{0}", flexProductID), 3));
                                            }
                                        }
                                    }
                                }

                                if (isProductOk)
                                {
                                    // PM 20150804 [21236] Pb d'un Flex sans trade sur le contrat standard
                                    //LoadLine(currentLine, ref parsingRowSerie);

                                    if (config.IsSearchProduct)
                                    {
                                        Boolean isStandard = BoolFunc.IsFalse(parsingRowSerie.GetRowDataValue(Cs, "Flex Series Flag"));
                                        if (isStandard)
                                        {
                                            // pour une série sur un produit standard, Spheres® vérifie que la série a été négociée uniquement si le contrat a déjà été négocié
                                            isProductOk = isProductIDInTrade;
                                        }
                                    }
                                    if (isProductOk)
                                    {
                                        // PM 20180219 [23824] Modification de IsSerieToImport 
                                        //isSerieOk = IsSerieToImport(config, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out idAsset);
                                        // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                        //isSerieOk = IsSerieToImport(config, false, parsingRowProduct, parsingRowSerie, parsingRowExpiry, out idAsset);
                                        MarketAssetETDRequest request = GetAssetRequestContractMonthYear(Cs, parsingRowProduct, parsingRowExpiry, parsingRowSerie);

                                        isSerieOk = IsSerieToImport(config, false, request, out MarketAssetETDToImport assetData);
                                        idAsset = assetData.IdAsset;
                                    }
                                }
                                break;
                            case "N": //NEUTRAL SCENARIO
                                // Alimentation de la table IMPRISMA_P,IMPRISMA_E,IMPRISA_S

                                parsingRowNeutralScenario = null;
                                dataRowSerie = null;
                                dataRowLGS = null; // Release 2 : les lignes LGS arrivent après la Ligne N, cette initialisation est donc ok 
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowNeutralScenario);

                                    dataRowProduct = this.LoadRowProduct(parsingRowProduct);
                                    int idProduct = Convert.ToInt32(dataRowProduct["IDIMPRISMAP_H"]);
                                    string productId = dataRowProduct["PRODUCTID"].ToString();

                                    DataRow rowExpiry = this.LoadRowProductExpiration(idProduct, productId, parsingRowExpiry);
                                    int idExpiry = Convert.ToInt32(rowExpiry["IDIMPRISMAE_H"]);

                                    dataRowSerie = LoadRowSerie(idExpiry, idAsset, parsingRowSerie, parsingRowNeutralScenario);

                                    //Release 1, une serie est associée à un et un seul Liquidation Group Split.
                                    //Ici Spheres® ajoute le Liquidation Group Split s'il n'existe pas
                                    string liquidationGroupSplit = parsingRowSerie.GetRowDataValue(Cs, "Liquidation Group Split");
                                    if (StrFunc.IsFilled(liquidationGroupSplit))
                                    {
                                        int idLiquidationGroup = Convert.ToInt32(dataRowProduct["IDIMPRISMALG_H"]);
                                        dataRowLGS = LoadRowLiquidationGroupSplit(idLiquidationGroup, liquidationGroupSplit, null);
                                    }
                                }
                                break;
                            case "LGS": //LGS existe uniquement en release 2
                                parsingRowLGS = null;
                                dataRowLGS = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowLGS);
                                    int idLiquidationGroup = Convert.ToInt32(dataRowProduct["IDIMPRISMALG_H"]);
                                    dataRowLGS = LoadRowLiquidationGroupSplit(idLiquidationGroup, parsingRowLGS, null);
                                }
                                break;
                            case "RMS":
                                parsingRowRMS = null;
                                dataRowRMSOfLGS = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowRMS);
                                    DataRow rowRMS = LoadRowRiskMeasuresSet(parsingRowRMS);

                                    int idLiquidationGroupSplit = Convert.ToInt32(dataRowLGS["IDIMPRISMALGS_H"]);
                                    int idRiskMeasureSet = Convert.ToInt32(rowRMS["IDIMPRISMARMS_H"]);
                                    dataRowRMSOfLGS = LoadRiskMeasuresSetOfLiquidationGroupSplit(idLiquidationGroupSplit, idRiskMeasureSet, parsingRowRMS);
                                }
                                break;
                            case "LH": //LIQUIDATION HORIZON 
                                parsingRowLH = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowLH);
                                }
                                break;
                            case "FX": //FX SET
                                parsingRowFxSet = null;
                                dataRowFxRate = null;
                                if (isSerieOk)
                                {
                                    LoadLine(currentLine, ref parsingRowFxSet);
                                    dataRowFxRate = LoadRowFX(parsingRowFxSet);
                                }
                                break;
                            case "SP": //SCENARIO PRICES
                                dataRowLGSOfSerie = null;
                                dataRowRMSOfLGSOfSerie = null;
                                if (isSerieOk)
                                {
                                    IOTaskDetInOutFileRow parsingRowScenariosPrices = null;
                                    LoadLine(currentLine, ref parsingRowScenariosPrices);

                                    //Liquidation Group Split of serie
                                    int idSerie = Convert.ToInt32(dataRowSerie["IDIMPRISMAS_H"]);
                                    int idLiquidationGroupSplit = Convert.ToInt32(dataRowLGS["IDIMPRISMALGS_H"]);
                                    dataRowLGSOfSerie = LoadRowLiquidationGroupSplitOfSerie(idSerie, idLiquidationGroupSplit, parsingRowLH, parsingRowLGS);

                                    //RSM of LGS of Serie
                                    int idRowRMSOfLGS = Convert.ToInt32(dataRowRMSOfLGS["IDIMPRISMARMSLGS_H"]);
                                    int IdFxRate = Convert.ToInt32(dataRowFxRate["IDIMPRISMAFX_H"]);
                                    // PM 20161019 [22174] Prisma 5.0 : Ajout pParsingRowLH
                                    //dataRowRMSOfLGSOfSerie = LoadRowRiskMeasuresSetOfLiquidationGroupSplitOfSerie(idSerie, idRowRMSOfLGS, IdFxRate);
                                    dataRowRMSOfLGSOfSerie = LoadRowRiskMeasuresSetOfLiquidationGroupSplitOfSerie(idSerie, idRowRMSOfLGS, IdFxRate, parsingRowLH);

                                    //Senarios Prices
                                    // PM 20161019 [22174] Prisma 5.0 : Ne plus prendre le liquidationHorizon dans "dataRowLGSOfSerie" mais dans "dataRowRMSOfLGSOfSerie"
                                    //int liquidationHorizon = Convert.ToInt32(dataRowLGSOfSerie["LH"]);
                                    int liquidationHorizon = Convert.ToInt32(dataRowRMSOfLGSOfSerie["LH"]);
                                    int idRMSOfLGSOfSerie = Convert.ToInt32(dataRowRMSOfLGSOfSerie["IDIMPRISMARMSLGSS_H"]);

                                    // PM 20161019 [22174] Prisma 5.0 : Ajout de Neutral Price pour les valeurs de SP qui ne sont pas renseignées
                                    decimal neutralPrice = Convert.ToDecimal(dataRowSerie["NPRICE"]);

                                    if (sqlSP.Length > 0)
                                    {
                                        sqlSP.Append(SQLCst.UNIONALL + Cst.CrLf);
                                    }
                                    // PM 20161019 [22174] Prisma 5.0 : Ajout de neutralPrice
                                    //sqlSP.Append(BuildSelectSPValues2(Cs, idRMSOfLGSOfSerie, parsingRowScenariosPrices, liquidationHorizon));
                                    sqlSP.Append(BuildSelectSPValues2(Cs, idRMSOfLGSOfSerie, parsingRowScenariosPrices, liquidationHorizon, neutralPrice));

                                    if (sqlSP.Length >= config.SqlLimitSize)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        if (m_IsAsync)
                                        {
                                            executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(Cs, sqlSP, null, null));
                                        }
                                        else
                                        {
                                            ExecuteQueryTheoriticalPriceFile(Cs, sqlSP, null, null);
                                        }
                                        sqlSP = new StringBuilder();
                                    }
                                }
                                break;
                            case "CE": //COMPRESSION ERROR
                                if (isSerieOk)
                                {
                                    IOTaskDetInOutFileRow parsiongRowCompressionError = null;
                                    LoadLine(currentLine, ref parsiongRowCompressionError);

                                    int liquidationHorizon = Convert.ToInt32(dataRowLGSOfSerie["LH"]);
                                    int idRMSOfLGSOfSerie = Convert.ToInt32(dataRowRMSOfLGSOfSerie["IDIMPRISMARMSLGSS_H"]);

                                    if (sqlCE.Length > 0)
                                        sqlCE.Append(SQLCst.UNIONALL + Cst.CrLf);
                                    sqlCE.Append(BuildSelectCEValues(Cs, idRMSOfLGSOfSerie, parsiongRowCompressionError, liquidationHorizon));

                                    if (sqlCE.Length >= config.SqlLimitSize)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        if (m_IsAsync)
                                        {
                                            executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(Cs, null, sqlCE, null));
                                        }
                                        else
                                        {
                                            ExecuteQueryTheoriticalPriceFile(Cs, null, sqlCE, null);
                                        }
                                        sqlCE = new StringBuilder();
                                    }
                                }
                                break;
                            case "IVAR": //INSTRUMENT VAR
                            case "AIVAR": //ADDITIONAL INSTRUMENT VAR
                                if (isSerieOk)
                                {
                                    IOTaskDetInOutFileRow parsiongRowVar = null;
                                    LoadLine(currentLine, ref parsiongRowVar);
                                    int idRMSOfLGSOfSerie = Convert.ToInt32(dataRowRMSOfLGSOfSerie["IDIMPRISMARMSLGSS_H"]);

                                    if (sqlVAR.Length > 0)
                                        sqlVAR.Append(SQLCst.UNIONALL + Cst.CrLf);
                                    sqlVAR.Append(BuildSelectVaRValues(Cs, idRMSOfLGSOfSerie, parsiongRowVar));

                                    if (sqlVAR.Length >= config.SqlLimitSize)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        if (m_IsAsync)
                                        {
                                            executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(Cs, null, null, sqlVAR));
                                        }
                                        else
                                        {
                                            ExecuteQueryTheoriticalPriceFile(Cs, null, null, sqlVAR);
                                        }
                                        sqlVAR = new StringBuilder();
                                    }
                                }
                                break;
                            case "*EOF*":
                                if (m_IsAsync)
                                {
                                    // PM 20180208 [CHEETAH] Gestion asynchrone
                                    executeTask.Add(ExecuteQueryTheoriticalPriceFileAsync(Cs, sqlSP, sqlCE, sqlVAR));
                                }
                                else
                                {
                                    ExecuteQueryTheoriticalPriceFile(Cs, sqlSP, sqlCE, sqlVAR);
                                }
                                //PM 20140612 [19911] Ajout mise à null des variables de données SQL à insérer pour ne pas tenter des les insérer de nouveau lorsque currentline vaut null
                                sqlSP = null;
                                sqlCE = null;
                                sqlVAR = null;
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                // PM 20180208 [CHEETAH] Gestion asynchrone : attente fin des tâches
                if (m_IsAsync)
                {
                    try
                    {
                        System.Threading.Tasks.Task.WaitAll(executeTask.Where(t => t != default(System.Threading.Tasks.Task)).ToArray());
                        executeTask = default;
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten();
                    }
                }

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier Risk Measure Configuration ou Importation du fichier Risk Measure Aggregation
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        public int InputRiskMeasure(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);

                DataRow dataRowLiquidationGroup = null;
                DataRow dataRowLiquidationGroupSplit = null;

                IOTaskDetInOutFileRow parsiongRowLiquidationGroup = null;
                IOTaskDetInOutFileRow parsiongRowLiquidationGroupSplit = null;

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
                            break;

                        #region Process Ligne par ligne
                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            case "LG"://Liquidation Group
                                dataRowLiquidationGroup = null;
                                parsiongRowLiquidationGroup = null;
                                LoadLine(currentLine, ref parsiongRowLiquidationGroup);
                                dataRowLiquidationGroup = LoadRowLiquidationGroup(parsiongRowLiquidationGroup);
                                break;

                            case "LGS": //LIQUIDATION GROUP SPLIT (présent uniquement en release 2)
                                parsiongRowLiquidationGroupSplit = null;
                                LoadLine(currentLine, ref parsiongRowLiquidationGroupSplit);
                                dataRowLiquidationGroupSplit = LoadRowLiquidationGroupSplit(
                                                    Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]), parsiongRowLiquidationGroupSplit, null);
                                break;

                            case "RM"://RISK MEASURE (présent uniquement sur le fichier Risk Measure Aggregation)
                                IOTaskDetInOutFileRow parsingRowRiskMeasures = null;
                                LoadLine(currentLine, ref parsingRowRiskMeasures);
                                if (parsiongRowLiquidationGroupSplit == null) // se produit sur un fichier Release 1 (pas de record LGS)
                                {
                                    //Release 1, les RM sont associés au Liquidation Group 
                                    //Release 2, les RM sont associés au Liquidation Group Split
                                    //Release 1,
                                    //Il est nécessaire de connaitre au préalable les Liquidation Group Split déclarés ds Theoretical prices and instrument uniquement sur les record de type S
                                    int idLiquidationGroup = Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]);
                                    //FI 20140225 [17861] Utilisation de la méthode GetLGSOfLG                                    
                                    //string identifierLiquidationGroup = dataRowLiquidationGroup["IDENTIFIER"].ToString() ;
                                    //Il peut ne pas y avoir de Liquidation Group Split lorsque Spheres intègre partiellement le fichier Theoretical prices and instrument (cas par défaut)
                                    //List<Pair<Int32, string>> liquidationGroupSplit = CheckLGSOfLG(idLiquidationGroup, identifierLiquidationGroup);
                                    List<Pair<Int32, string>> liquidationGroupSplit = GetLGSOfLG(Cs, idLiquidationGroup);
                                    for (int i = 0; i < ArrFunc.Count(liquidationGroupSplit); i++)
                                        LoadRowLiquidationGroupSplit(idLiquidationGroup, liquidationGroupSplit[i].Second, parsingRowRiskMeasures);
                                }
                                else
                                {
                                    //Release 2
                                    int idLiquidationGroup = Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]);
                                    LoadRowLiquidationGroupSplit(idLiquidationGroup, parsiongRowLiquidationGroupSplit, parsingRowRiskMeasures);
                                }
                                break;

                            case "RMS": //RISK MEASURE SET
                                IOTaskDetInOutFileRow parsingRowRiskMeasureSet = null;
                                LoadLine(currentLine, ref parsingRowRiskMeasureSet);

                                DataRow rowRMS = LoadRowRiskMeasuresSet(parsingRowRiskMeasureSet);
                                int idRiskMeasureSet = Convert.ToInt32(rowRMS["IDIMPRISMARMS_H"]);
                                if (parsiongRowLiquidationGroupSplit == null) // se produit sur un fichier Release 1 (pas de record LGS)
                                {
                                    //Release 1, les RMS sont associés au Liquidation Group 
                                    //Release 2, les RMS sont associés au Liquidation Group Split
                                    //
                                    //Release 1,
                                    //Il est nécessaire de connaitre au préalable les Liquidation Group Split déclarés ds Theoretical prices and instrument uniquement sur les record de type S
                                    int idLiquidationGroup = Convert.ToInt32(dataRowLiquidationGroup["IDIMPRISMALG_H"]);

                                    //FI 20140225 [17861] Utilisation de la méthode GetLGSOfLG
                                    //Il peut ne pas y avoir de Liquidation Group Split lorsque Spheres intègre partiellement le fichier Theoretical prices and instrument (cas par défaut)
                                    //string identifierLiquidationGroup = dataRowLiquidationGroup["IDENTIFIER"].ToString();
                                    //List<Pair<Int32, string>> liquidationGroupSplit = CheckLGSOfLG(idLiquidationGroup, identifierLiquidationGroup);
                                    List<Pair<Int32, string>> liquidationGroupSplit = GetLGSOfLG(Cs, idLiquidationGroup);
                                    for (int i = 0; i < ArrFunc.Count(liquidationGroupSplit); i++)
                                        LoadRiskMeasuresSetOfLiquidationGroupSplit(liquidationGroupSplit[i].First, idRiskMeasureSet, parsingRowRiskMeasureSet);
                                }
                                else
                                {
                                    //Release 2
                                    int idLiquidationGroupSplit = Convert.ToInt32(dataRowLiquidationGroupSplit["IDIMPRISMALGS_H"]);
                                    LoadRiskMeasuresSetOfLiquidationGroupSplit(idLiquidationGroupSplit, idRiskMeasureSet, parsingRowRiskMeasureSet);
                                }
                                break;

                            case "*EOF*":
                                break;

                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier Liquidity Factors Configuration
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputLiquidityFactors(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                Delete_LiquidityFactorsFile();

                StringBuilder sql = new StringBuilder();

                string lastLiquidityClass = string.Empty;
                DataRow dataRowLiquidityClass = null;

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
                            break;

                        #region Process Ligne par ligne
                        string value = GetFirstElement(currentLine);
                        if (value != "*EOF*")
                        {
                            IOTaskDetInOutFileRow parsingRow = null;
                            this.LoadLine(currentLine, ref parsingRow);

                            string liquidityClass = parsingRow.GetRowDataValue(Cs, "Liquidity Class");
                            if (lastLiquidityClass != liquidityClass)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add values for liquidity class:{0}", liquidityClass), 3));

                                dataRowLiquidityClass = LoadRowLiquidityClass(liquidityClass);
                                lastLiquidityClass = liquidityClass;
                            }

                            int idLiquidityClass = Convert.ToInt32(dataRowLiquidityClass["IDIMPRISMALIQCLASS_H"]);

                            if (sql.Length > 0)
                                sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                            sql.Append(BuildSelectLiquidityFactors(Cs, idLiquidityClass, parsingRow));
                        }
                        else
                        {
                            ExecuteQueryLiquidityFactors(Cs, sql);
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier Market Capacities Configuration
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        /// FI 20140617 [19911] utilisation de la variable isElementTheoInstExecuted pour s'appuyer sur l'importation du fichier TheoInst
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputMarketCapacities(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);
                PrismaInputFileConfig config = InitMarketCapacitiesFileConfig();

                // PM 20170524 [22834][23078] Recherche uniquement de "RISKDATA - TheoInst OI-serie"
                //Boolean isElementTheoInstExecuted = task.ExistElement("EUREX - RISKDATA - TheoInst OI-serie");
                Boolean isElementTheoInstExecuted = task.ExistElementContains("RISKDATA - TheoInst OI-serie");

                // Récupération de la liste des assets importés via l'importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
                List<String> lstProductId = new List<String>();
                if (isElementTheoInstExecuted)
                    lstProductId = LoadProductIdfromPrismaProduct();

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                Delete_MarketCapacitiesFile();

                string lastProductId = string.Empty;
                Boolean isProductOk = false;
                ArrayList alProductOk = new ArrayList();
                StringBuilder sql = new StringBuilder();

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
                            break;

                        #region Process Ligne par ligne
                        string value = GetFirstElement(currentLine);
                        if (value != "*EOF*")
                        {
                            IOTaskDetInOutFileRow parsingRow = null;
                            this.LoadLine(currentLine, ref parsingRow);

                            // Insertion des données à chaque nouveau produit
                            string productID = parsingRow.GetRowDataValue(Cs, "Product ID");
                            if ((productID != lastProductId))
                            {
                                lastProductId = productID;

                                isProductOk = IsProductToImport(config, productID, alProductOk);
                                if (isElementTheoInstExecuted && isProductOk)
                                    isProductOk = lstProductId.Contains(productID);

                                if (isProductOk)
                                {
                                    alProductOk.Add(productID);

                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add values for product:{0}", productID), 3));
                                }
                                //A chaque nouveau produit, Exécution du SQL non encore exécuté 
                                if (sql.Length > 0)
                                {
                                    ExecuteQueryMarketCapacities(Cs, sql);
                                    sql = new StringBuilder();
                                }
                            }
                            if (isProductOk)
                            {
                                if (sql.Length > 0)
                                    sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                                sql.Append(BuildSelectMarketCapacities(Cs, idIMPRISMA_H, parsingRow));
                                if (sql.Length >= config.SqlLimitSize)
                                {
                                    ExecuteQueryMarketCapacities(Cs, sql);
                                    sql = new StringBuilder();
                                }
                            }
                        }
                        else
                        {
                            if (sql.Length > 0)
                            {
                                ExecuteQueryMarketCapacities(Cs, sql);
                            }
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        ///  Importation du fichier foreign Exchange rates
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int InputFxExchangeRates(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                // PM 20180208 [CHEETAH] Liste des tâches lancées en asynchrone
                List<System.Threading.Tasks.Task> executeTask = new List<System.Threading.Tasks.Task>();

                StartPrismaImport(pInputParsing);
                PrismaInputFileConfig config = InitFxExchangeRateFileConfig();

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Removal of existing values ...", 3));

                Delete_FxExchangeRatesFile();

                IOTaskDetInOutFileRow parsingRowFxSet = null;
                IOTaskDetInOutFileRow parsingRowCurrencyPair = null;

                DataRow dataRowFxSet = null;
                DataRow dataRowFxPair = null;
                StringBuilder sql = new StringBuilder();

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
                            break;

                        string Record_Type = GetRecordType(currentLine);
                        switch (Record_Type)
                        {
                            case "FX": //FX SET
                                parsingRowFxSet = null;
                                dataRowFxSet = null;
                                LoadLine(currentLine, ref parsingRowFxSet);
                                dataRowFxSet = LoadRowFX(parsingRowFxSet);
                                break;

                            case "P"://Currency Pair
                                parsingRowCurrencyPair = null;
                                LoadLine(currentLine, ref parsingRowCurrencyPair);
                                break;

                            case "C"://Currency Exchange Rate
                                IOTaskDetInOutFileRow parsingRowCurrentExchangeRate = null;
                                LoadLine(currentLine, ref parsingRowCurrentExchangeRate);

                                int idPrismaFX = Convert.ToInt32(dataRowFxSet["IDIMPRISMAFX_H"]);
                                dataRowFxPair = LoadRowFxPair(idPrismaFX, parsingRowCurrencyPair, parsingRowCurrentExchangeRate);
                                break;

                            case "RMS"://Risk Measure Set and Exchange Rate Scenarios
                                IOTaskDetInOutFileRow parsingRowCurrentRiskMeasureSet = null;
                                LoadLine(currentLine, ref parsingRowCurrentRiskMeasureSet);

                                string RMSIdentifier = parsingRowCurrentRiskMeasureSet.GetRowDataValue(Cs, "Risk Measure Set");
                                DataRow dataRowRMS = LoadRowRiskMeasuresSet(parsingRowCurrentRiskMeasureSet);
                                int idRMS = Convert.ToInt32(dataRowRMS["IDIMPRISMARMS_H"]);
                                int idFxPair = Convert.ToInt32(dataRowFxPair["IDIMPRISMAFXPAIR_H"]);

                                if (sql.Length > 0)
                                    sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                                sql.Append(BuildSelectFXRatesValues(Cs, idFxPair, idRMS, parsingRowCurrentRiskMeasureSet));
                                if (sql.Length >= config.SqlLimitSize)
                                {
                                    if (m_IsAsync)
                                    {
                                        // PM 20180208 [CHEETAH] Gestion asynchrone
                                        executeTask.Add(ExecuteQueryFxRateAsync(Cs, sql));
                                    }
                                    else
                                    {
                                        ExecuteQueryFxRate(Cs, sql);
                                    }
                                    sql = new StringBuilder();
                                }
                                break;

                            case "*EOF*":
                                if (m_IsAsync)
                                {
                                    // PM 20180208 [CHEETAH] Gestion asynchrone
                                    executeTask.Add(ExecuteQueryFxRateAsync(Cs, sql));
                                }
                                else
                                {
                                    ExecuteQueryFxRate(Cs, sql);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                // PM 20180208 [CHEETAH] Gestion asynchrone : attente fin des tâches
                if (m_IsAsync)
                {
                    try
                    {
                        System.Threading.Tasks.Task.WaitAll(executeTask.Where(t => t != default(System.Threading.Tasks.Task)).ToArray());
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten();
                    }
                }

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
        /// </summary>
        /// <returns></returns>
        private PrismaInputFileConfig InitTheoreticalPriceFileConfig()
        {
            string productList = GetAppSetting("InputTheoreticalPriceFile", "productList");
            string nbProduct = GetAppSetting("InputTheoreticalPriceFile", "nbProduct");

            PrismaInputFileConfig config;
            if (StrFunc.IsFilled(productList))
            {
                string[] productArray = productList.Split(';');
                config = new PrismaInputFileConfig(productArray);
            }
            else if (StrFunc.IsFilled(nbProduct))
            {
                config = new PrismaInputFileConfig(IntFunc.IntValue(nbProduct));
            }
            else
            {
                config = new PrismaInputFileConfig();
            }

            string searchProduct = GetAppSetting("InputTheoreticalPriceFile", "isSearchProduct");
            if (StrFunc.IsFilled(searchProduct))
            {
                config.IsSearchProduct = BoolFunc.IsTrue(searchProduct);
            }

            string searchSerie = GetAppSetting("InputTheoreticalPriceFile", "isSearchSerie");
            if (StrFunc.IsFilled(searchSerie))
            {
                config.IsSearchSerie = BoolFunc.IsTrue(searchSerie);
            }

            string sqlLimitSize = GetAppSetting("InputTheoreticalPriceFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.SqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier Market capacities
        /// </summary>
        /// <returns></returns>
        private PrismaInputFileConfig InitMarketCapacitiesFileConfig()
        {
            string productList = GetAppSetting("InputMarketCapacitiesFile", "productList");
            string nbProduct = GetAppSetting("InputMarketCapacitiesFile", "nbProduct");

            PrismaInputFileConfig config;
            if (StrFunc.IsFilled(productList))
            {
                string[] productArray = productList.Split(';');
                config = new PrismaInputFileConfig(productArray);
            }
            else if (StrFunc.IsFilled(nbProduct))
            {
                config = new PrismaInputFileConfig(IntFunc.IntValue(nbProduct));
            }
            else
            {
                config = new PrismaInputFileConfig();
            }

            string searchProduct = GetAppSetting("InputMarketCapacitiesFile", "isSearchProduct");
            if (StrFunc.IsFilled(searchProduct))
            {
                config.IsSearchProduct = BoolFunc.IsTrue(searchProduct);
            }

            string sqlLimitSize = GetAppSetting("InputMarketCapacitiesFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.SqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier foreign Exchange rate
        /// </summary>
        /// <returns></returns>
        private PrismaInputFileConfig InitFxExchangeRateFileConfig()
        {
            PrismaInputFileConfig config = new PrismaInputFileConfig();

            string sqlLimitSize = GetAppSetting("InputFxExchangeRateFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.SqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Initialise la configuration de l'importation du fichier Settlement prices
        /// </summary>
        /// <returns></returns>
        ///  FI 20140617 [19911] add methode
        private PrismaInputFileConfig InitSettlementPriceFileConfig()
        {
            string productList = GetAppSetting("InputSettlementPriceFile", "productList");
            string nbProduct = GetAppSetting("InputSettlementPriceFile", "nbProduct");

            PrismaInputFileConfig config;
            if (StrFunc.IsFilled(productList))
            {
                string[] productArray = productList.Split(';');
                config = new PrismaInputFileConfig(productArray);
            }
            else if (StrFunc.IsFilled(nbProduct))
            {
                config = new PrismaInputFileConfig(IntFunc.IntValue(nbProduct));
            }
            else
            {
                config = new PrismaInputFileConfig();
            }

            string searchProduct = GetAppSetting("InputSettlementPriceFile", "isSearchProduct");
            if (StrFunc.IsFilled(searchProduct))
            {
                config.IsSearchProduct = BoolFunc.IsTrue(searchProduct);
            }

            string searchSerie = GetAppSetting("InputSettlementPriceFile", "isSearchSerie");
            if (StrFunc.IsFilled(searchSerie))
            {
                config.IsSearchSerie = BoolFunc.IsTrue(searchSerie);
            }

            string sqlLimitSize = GetAppSetting("InputSettlementPriceFile", "sqlLimitSize");
            if (StrFunc.IsFilled(sqlLimitSize))
            {
                int isqlLimitSize = IntFunc.IntValue(sqlLimitSize);
                config.SqlLimitSize = isqlLimitSize;
            }

            return config;
        }

        /// <summary>
        /// Retourne le la clé présente dans le fichier de config
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private static string GetAppSetting(string prefix, string key)
        {
            string ret = SystemSettings.GetAppSettings(prefix + "_" + key);
            if (StrFunc.IsEmpty(ret))
                ret = SystemSettings.GetAppSettings("IMPORTPRISMA_");
            return ret;
        }

        /// <summary>
        ///  Retourne true si le produit est à importer
        ///  <para>Lecture du fichier de configuration</para>
        /// </summary>
        /// <param name="pConfig">configuration de l'importation</param>
        /// <param name="pProductID">Symbol du produit à importer</param>
        /// <param name="pAlProduct">Liste des produits déjà importés</param>
        /// <returns></returns>
        /// FI 20140618 [19911] Modification de comportement, cette méthode ne teste plus l'existence de trade négocié sur le produit 
        private Boolean IsProductToImport(PrismaInputFileConfig pConfig, string pProductID, ArrayList pAlProduct)
        {
            bool ret = true;
            if ((pConfig.NbProductMax > 0) || (pConfig.RestrictProduct.Count > 0))
            {
                if (pConfig.RestrictProduct.Count > 0)
                    ret = pConfig.RestrictProduct.Contains(pProductID);

                if (ret && pConfig.NbProductMax > 0)
                    ret = (ArrFunc.Count(pAlProduct) < pConfig.NbProductMax);
            }
            //
            // FI 20140618 [19911] Mise en commentaire
            //if (ret == true && pConfig.isSearchProduct)
            //{
            //    // Pour des raisons de perfs => Spheres® ne teste pas la présence du DC dans le référentiel mais uniqement dans les trades
            //    //ret = IsExistDerivativeContract(pProductID);
            //    //if (ret)
            //    ret = IsExistDcInTrade(pProductID);
            //}

            return ret;
        }

        /// <summary>
        ///  Retourne true si le serie est à importer
        /// </summary>
        /// <param name="config">configuration de l'importation</param>
        /// <param name="request"></param>
        /// <param name="isImportPriceOnly">Indicateur d'import de cours uniquement</param>
        /// <param name="oAssetFound">Asset Spheres trouvé</param> 
        /// <returns></returns>
        private Boolean IsSerieToImport(PrismaInputFileConfig config, bool isImportPriceOnly, MarketAssetETDRequest request, out MarketAssetETDToImport oAssetFound)
        {
            oAssetFound = null;

            if (config.IsSearchSerie)
            {
                if (isImportPriceOnly)
                {
                    oAssetFound = _assetETDToImport.GetAsset(_assetETDRequestSettings, request);
                }
                else
                {
                    GetIdAssetETD(request, out oAssetFound);
                }
            }

            if (null == oAssetFound)
                oAssetFound = new MarketAssetETDToImport();

            return (oAssetFound.IdAsset > 0);
        }



        /// <summary>
        /// Supprime toutes les informations issues du fichier theoritical prices and instrument configuration 
        /// </summary>
        private int Delete_TheoreticalPriceFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

            string sqlQuery = @"delete from dbo.IMPRISMAP_H where (IDIMPRISMA_H = @ID)";

            QueryParameters qryParameters = new QueryParameters(Cs, sqlQuery, dataParameters);

            int nRowDeleted = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier liquidity factors
        /// </summary>
        private int Delete_LiquidityFactorsFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

            string sqlQuery = @"
delete from dbo.IMPRISMALIQFACT_H 
where IDIMPRISMALIQCLASS_H 
in 
(
    select IDIMPRISMALIQCLASS_H from dbo.IMPRISMALIQCLASS_H
    where (IDIMPRISMA_H=@ID)
)";

            QueryParameters qryParameters = new QueryParameters(Cs, sqlQuery, dataParameters);

            int nRowDeleted = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier market capacities
        /// </summary>
        private int Delete_MarketCapacitiesFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);

            string sqlQuery = @"delete from dbo.IMPRISMAMKTCAPA_H where IDIMPRISMA_H=@ID";

            QueryParameters qryParameters = new QueryParameters(Cs, sqlQuery, dataParameters);

            int nRowDeleted = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Supprime toutes les informations issues du fichier Foreign Exchange Rates
        /// </summary>
        private int Delete_FxExchangeRatesFile()
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.ID), this.idIMPRISMA_H);
            string sqlQuery = @"delete from dbo.IMPRISMAFXRMS_H 
            where IDIMPRISMAFXPAIR_H in
            ( select fxpair.IDIMPRISMAFXPAIR_H 
              from dbo.IMPRISMAFXPAIR_H fxpair
              inner join dbo.IMPRISMAFX_H fx on fx.IDIMPRISMAFX_H = fxpair.IDIMPRISMAFX_H
              where (fx.IDIMPRISMA_H = @ID)
            )";
            QueryParameters qryParameters = new QueryParameters(Cs, sqlQuery, dataParameters);
            _ = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            sqlQuery = @"delete from dbo.IMPRISMAFXPAIR_H
            where IDIMPRISMAFX_H in
            ( select IDIMPRISMAFX_H 
              from dbo.IMPRISMAFX_H 
              where (IDIMPRISMA_H = @ID))";

            qryParameters = new QueryParameters(Cs, sqlQuery, dataParameters);
            int nRowDeleted = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return nRowDeleted;
        }

        /// <summary>
        /// Retourne true si le symbol existe dans Spheres®
        /// <para>Rq: Sous Spheres® plusieurs Contrat derivé peuvent exister pour un même symbol (dans ce cas, la version sera différente (DERIVATIVECONTRACT.CONTRACTATTRIBUTE)</para>
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        /// PM 20170531 [22834] Méthode non utilisée
        //private Boolean IsExistDerivativeContract(string pContractSymbol)
        //{
        //    m_QueryExistDC.parameters["CONTRACTSYMBOL"].Value = pContractSymbol;

        //    object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(Cs), CommandType.Text, m_QueryExistDC.query, m_QueryExistDC.parameters.GetArrayDbParameter());
        //    bool ret = (obj != null);

        //    return ret;
        //}

        /// <summary>
        /// Retourne true si le symbol est négocié dans Spheres®
        /// <para>Rq: Sous Spheres® plusieurs Contrat derivé peuvent exister pour un même symbol (dans ce cas, la version sera différente (DERIVATIVECONTRACT.CONTRACTATTRIBUTE)</para>
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        // PM 20141022 [9700] Eurex Prisma for Eurosys Futures : La méthode devient "virtual"
        protected virtual Boolean IsExistDcInTrade(string pContractSymbol)
        {
            // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
            //return base.IsExistDcInTrade(CSTools.SetCacheOn(Cs), m_dtFile, EUREX_EXCHANGEACRONYM, pContractSymbol);
            return base.IsExistDcInTrade(CSTools.SetCacheOn(Cs), m_dtFile, ExchangeAcronym, pContractSymbol);
        }



        /// <summary>
        ///  Retourne l'enregistrement dans IMPRISMA_H qui correspond à la date de traitement
        ///  <para>Alimente au passage le membre idIMPRISMA_H</para>
        ///  <para>Alimente la table s'il n'existe aucun enregistrement en date de traitement</para>
        /// </summary>
        private DataRow LoadRowIMPRISMA_H()
        {
            // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
            //DataTable dt = GetDataTable_IMPRISMA_H(Cs, m_dtFile, out queryDataAdapter);
            DataTable dt = GetDataTable_IMPRISMA_H(Cs, m_dtFile, ExchangeAcronym, out string queryDataAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["DTBUSINESS"] = m_dtFile;
                // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
                dr["EXCHANGEACRONYM"] = ExchangeAcronym;

                SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryDataAdapter, dt);
                // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
                dt = GetDataTable_IMPRISMA_H(Cs, m_dtFile, ExchangeAcronym, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMA_H is not loaded for date:{0}", DtFunc.DateTimeToStringISO(m_dtFile)));
            }

            this.idIMPRISMA_H = Convert.ToInt32(dt.Rows[0]["IDIMPRISMA_H"]);

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate">Date de traitement</param>
        /// <param name="pExchangeAcronym">ExchangeAcronym du fichier en cours d'importation</param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        /// PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
        //private static DataTable GetDataTable_IMPRISMA_H(string pCS, DateTime pDate, out string pQuery)
        private static DataTable GetDataTable_IMPRISMA_H(string pCS, DateTime pDate, string pExchangeAcronym, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE), pDate);
            // PM 20170531 [22834] Ajout gestion EXCHANGEACRONYM
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXCHANGEACRONYM), pExchangeAcronym);

            //string sqlQuery = "select * from dbo.IMPRISMA_H where DTBUSINESS = @DTFILE";
            string sqlQuery = "select * from dbo.IMPRISMA_H where (DTBUSINESS = @DTFILE) and (EXCHANGEACRONYM = @EXCHANGEACRONYM)";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAP_H qui correspond au produit {pRowProduct}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAP_H s'il n'existe pas</para>
        /// <para>Alimente la table IMPRISMALG_H et IMPRISMALIQCLASS_H si nécessaire</para>
        /// </summary>
        /// <param name="pParsingRowProduct">Représente le parsing de la ligne Product du produit</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        /// FI 20140617 [19911] la colonne DTINS n'existe plus
        private DataRow LoadRowProduct(IOTaskDetInOutFileRow pParsingRowProduct)
        {
            string productId = pParsingRowProduct.GetRowDataValue(Cs, "Product ID");
            DataTable dt = GetTableProduct(CSTools.SetCacheOn(Cs, 1, null), productId, idIMPRISMA_H, out string queryAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();

                dr["IDIMPRISMA_H"] = idIMPRISMA_H;

                dr["PRODUCTID"] = productId;
                dr["TICKSIZE"] = DecFunc.DecValueFromInvariantCulture(pParsingRowProduct.GetRowDataValue(Cs, "Tick Size"));
                dr["TICKVALUE"] = DecFunc.DecValueFromInvariantCulture(pParsingRowProduct.GetRowDataValue(Cs, "Tick Value"));
                dr["IDC"] = pParsingRowProduct.GetRowDataValue(Cs, "Currency");

                string liquidityClass = pParsingRowProduct.GetRowDataValue(Cs, "Liquidity Class");
                DataRow drLiquidityClass = LoadRowLiquidityClass(liquidityClass);
                dr["IDIMPRISMALIQCLASS_H"] = drLiquidityClass["IDIMPRISMALIQCLASS_H"];

                string liquidationGroup = pParsingRowProduct.GetRowDataValue(Cs, "Liquidation Group");
                DataRow drLiquidationGroup = LoadRowLiquidationGroup(liquidationGroup);
                dr["IDIMPRISMALG_H"] = drLiquidationGroup["IDIMPRISMALG_H"];

                dr["MARGINSTYLE"] = pParsingRowProduct.GetRowDataValue(Cs, "Margin Style");
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableProduct(CSTools.SetCacheOn(Cs, 1, null), productId, idIMPRISMA_H, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAP_H is not loaded for Product:{0}", productId));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProductId"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableProduct(string pCS, string pProductId, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "PRODUCTID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), pProductId);

            string sqlQuery = "select * from dbo.IMPRISMAP_H where IDIMPRISMA_H=@IDIMPRISMA_H and PRODUCTID=@PRODUCTID";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALG_H qui correspond au groupe de liquidation {pLiquidationGroup}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALG_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pLiquidationGroup">représente l'identifier du groupe de liquidation</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidationGroup(string pLiquidationGroup)
        {
            IOTaskDetInOutFileRow row = new IOTaskDetInOutFileRow
            {
                data = new IOTaskDetInOutFileRowData[1] 
                {
                    new IOTaskDetInOutFileRowData()
                    {
                        name = "Liquidation Group",
                        value = pLiquidationGroup,
                        datatype = TypeData.TypeDataEnum.@string.ToString(),
                        datatypeSpecified = true
                    }
                }
            };
            return LoadRowLiquidationGroup(row);
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALG_H qui correspond au groupe de liquidation {pRowLiquidationGroup}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALG_H s'il n'existe pas</para>
        /// <para>Mise à jour lorsque le fichier est "Eurex Risk Measure configuration, "</para>
        /// </summary>
        /// <param name="pRowLiquidationGroup">Représente le parsing d'une ligne de type LiquidationGroup</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        /// FI 20140617 [19911] la colonne DTINS n'existe plus
        private DataRow LoadRowLiquidationGroup(IOTaskDetInOutFileRow pRowLiquidationGroup)
        {
            string liquidationGroup = pRowLiquidationGroup.GetRowDataValue(Cs, "Liquidation Group");

            DataTable dt = GetTableLiquidationGroup(CSTools.SetCacheOn(Cs, 1, null), liquidationGroup, idIMPRISMA_H, out string queryDataAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDENTIFIER"] = liquidationGroup;
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                switch (InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        dr["CURRENCYTYPEFLAG"] = pRowLiquidationGroup.GetRowDataValue(Cs, "Currency Type Flag");
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        dr["CURRENCYTYPEFLAG"] = "P"; // P Comme PRODUCT (valeur par défaut sur la table)
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryDataAdapter, dt);
                dt = GetTableLiquidationGroup(CSTools.SetCacheOn(Cs, 1, null), liquidationGroup, idIMPRISMA_H, out _);
            }
            else if (dt.Rows.Count == 1)
            {
                switch (InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        DataRow dr = dt.Rows[0];
                        dr["CURRENCYTYPEFLAG"] = pRowLiquidationGroup.GetRowDataValue(Cs, "Currency Type Flag");
                        //SetDataRowUpd(dr);

                        DataHelper.ExecuteDataAdapter(Cs, queryDataAdapter, dt);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        //NOTHING TO DO
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
            }
            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMLAG_H is not loaded for Liquidation Group:{0}", liquidationGroup));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// Retourne un DataTable
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLiquidationGroup"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidationGroup(string pCS, string pLiquidationGroup, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLiquidationGroup);

            string sqlQuery = "select * from dbo.IMPRISMALG_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALGS_H qui correspond à un groupe de liquidation Split {pLiquidationGroupSplit}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALGS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdLiquidationGroup">identifiant non significatif dun groupe de liquidation</param>
        /// <param name="pLiquidationGroupSplit">identifiant d'une groupe de liquidation Split</param>
        /// <param name="pParsingRowRiskMethod">Représente le parsing d'une ligne de type RM (risk Method), ne doit être valorisé uniquement sur le fichier Risk Measure Aggregation</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidationGroupSplit(int pIdLiquidationGroup, string pLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRowRiskMethod)
        {
            IOTaskDetInOutFileRow parsingRow = new IOTaskDetInOutFileRow
            {
                data = new IOTaskDetInOutFileRowData[1]
                {
                    new IOTaskDetInOutFileRowData()
                    {
                        name = "Liquidation Group Split",
                        value = pLiquidationGroupSplit,
                        datatype = TypeData.TypeDataEnum.@string.ToString(),
                        datatypeSpecified = true
                    }
                }
            };
            return LoadRowLiquidationGroupSplit(pIdLiquidationGroup, parsingRow, pParsingRowRiskMethod);
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALGS_H qui correspond au liquidation Group Split {pLiquidationGroupSplit}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALGS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdLiquidationGroup">Représente le parsing d'une ligne de type LG (Liquidation Group)</param>
        /// <param name="pParsingRowLiquidationGroupSplit">Représente le parsing d'une ligne de type LGS (Release 2 uniquement)</param>
        /// <param name="pParsingRowRiskMethod">Représente le parsing d'une ligne de type RM (risk Method), ne doit être valorisé uniquement sur le fichier Risk Measure Aggregation</param>
        /// <returns></returns>
        /// FI 20140617 [19911] la colonne DTINS n'existe plus
        private DataRow LoadRowLiquidationGroupSplit(int pIdLiquidationGroup, IOTaskDetInOutFileRow pParsingRowLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRowRiskMethod)
        {
            switch (InputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    //PM 20140608 [19911] Ne pas générer d'exeption car sur la ligne LGS en release 2 pParsingRowRiskMethod == null
                    //if (pParsingRowRiskMethod == null)
                    //    throw new ArgumentException(StrFunc.AppendFormat("Arg :{0} is null", "pRowRiskMethod"));
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    //NOTHING TO DO
                    break;
                default:
                    throw new Exception(StrFunc.AppendFormat("File :{0} is not implemented", InputSourceDataStyle.ToString()));
            }

            string liquidationGroupSplit = pParsingRowLiquidationGroupSplit.GetRowDataValue(Cs, "Liquidation Group Split");
            DataTable dt = GetTableLiquidationGroupSplit(CSTools.SetCacheOn(Cs, 1, null), pIdLiquidationGroup, liquidationGroupSplit, out string queryDataAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDENTIFIER"] = liquidationGroupSplit;
                dr["IDIMPRISMALG_H"] = pIdLiquidationGroup;
                switch (InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        //PM 20140608 [19911] Uniquement lorsque pParsingRowRiskMethod != null
                        if (pParsingRowRiskMethod != null)
                        {
                            dr["RISKMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(Cs, "Risk Method ID");
                            dr["AGGREGATIONMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(Cs, "Aggregation Method");
                        }
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        dr["RISKMETHOD"] = Convert.DBNull;
                        dr["AGGREGATIONMETHOD"] = Convert.DBNull;
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
                //FI 20140617 [19911] 
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryDataAdapter, dt);
                dt = GetTableLiquidationGroupSplit(CSTools.SetCacheOn(Cs, 1, null), pIdLiquidationGroup, liquidationGroupSplit, out _);
            }
            else if (dt.Rows.Count == 1)
            {
                switch (InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        //PM 20140608 [19911] Uniquement lorsque pParsingRowRiskMethod != null
                        if (pParsingRowRiskMethod != null)
                        {
                            DataRow dr = dt.Rows[0];
                            //PM 20140608 [19911] Add RISKMETHOD
                            dr["RISKMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(Cs, "Risk Method ID");
                            dr["AGGREGATIONMETHOD"] = pParsingRowRiskMethod.GetRowDataValue(Cs, "Aggregation Method");
                            //SetDataRowUpd(dr);

                            DataHelper.ExecuteDataAdapter(Cs, queryDataAdapter, dt);
                            DataHelper.queryCache.Remove("IMPRISMALGS_H", Cs);
                        }
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        //NOTHING TODO
                        break;
                    default:
                        throw new Exception(StrFunc.AppendFormat("{0} is not implemented", dataStyle));
                }
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMLALGS_H is not loaded for Liquidation Group Split:{0}", liquidationGroupSplit));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// Lecture de la table IMPRISMALGS_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLiquidationGroup"></param>
        /// <param name="pLiquidationGroupSplit"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidationGroupSplit(string pCS, int pIdLiquidationGroup, string pLiquidationGroupSplit, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALG_H", DbType.Int32), pIdLiquidationGroup);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLiquidationGroupSplit);

            string sqlQuery = "select * from dbo.IMPRISMALGS_H where IDIMPRISMALG_H=@IDIMPRISMALG_H and IDENTIFIER=@IDENTIFIER";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALIQCLASS_H qui correspond à la classe de liquidation  {pLiquidityClass}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAPLIQCLASS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pLiquidityClass">Représente le parsing d'une ligne de type LiquidationClass</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidityClass(string pLiquidityClass)
        {
            IOTaskDetInOutFileRow row = new IOTaskDetInOutFileRow
            {
                data = new IOTaskDetInOutFileRowData[1]
                {
                    new IOTaskDetInOutFileRowData()
                    {
                        name = "Liquidity Class",
                        value = pLiquidityClass,
                        datatype = TypeData.TypeDataEnum.@string.ToString(),
                        datatypeSpecified = true
                    }
                }
            };
            return LoadRowLiquidityClass(row);
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMALIQCLASS_H qui correspond à la classe de liquidation  {pLiquidityClass}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMALIQCLASS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParsingRowLiquidityClass">Représente une classe de liquidation </param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowLiquidityClass(IOTaskDetInOutFileRow pParsingRowLiquidityClass)
        {
            string liquidityClass = pParsingRowLiquidityClass.GetRowDataValue(Cs, "Liquidity Class");

            DataTable dt = GetTableLiquidityClass(CSTools.SetCacheOn(Cs, 1, null), liquidityClass, idIMPRISMA_H, out string queryAdapter);

            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDENTIFIER"] = liquidityClass;
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableLiquidityClass(CSTools.SetCacheOn(Cs, 1, null), liquidityClass, idIMPRISMA_H, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMALIQCLASS_H is not loaded for {0}", liquidityClass));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLiquidityClass"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidityClass(string pCS, string pLiquidityClass, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pLiquidityClass);

            string sqlQuery = "select * from dbo.IMPRISMALIQCLASS_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAE_H qui correspond à l'expiration {pRowExpiration} associée au produit {pProductId}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAE_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdProduct">id non significatif du produit</param>
        /// <param name="pProductId">symbol du produit</param>
        /// <param name="pParsingRowExpiration">Représente le parsing d'une ligne de type E</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowProductExpiration(int pIdProduct, string pProductId, IOTaskDetInOutFileRow pParsingRowExpiration)
        {
            //PM 20140612 [19911] En Release 2.0, l'échéance n'est pas forcement la date d'échéance : utilisation de Contract Year/Month au lieu de Expiration Year/Month
            //string year = pParsingRowExpiration.GetRowDataValue(Cs, "Expiration Year");
            //string month = pParsingRowExpiration.GetRowDataValue(Cs, "Expiration Month");
            string year = pParsingRowExpiration.GetRowDataValue(Cs, "Contract Year");
            string month = pParsingRowExpiration.GetRowDataValue(Cs, "Contract Month");
            string day = pParsingRowExpiration.GetRowDataValue(Cs, "Expiration Day");
            string daysToExpiry = pParsingRowExpiration.GetRowDataValue(Cs, "Days to Expiry");

            // PM 20161019 [22174] Prisma 5.0 : Attention Contract Year et Contract Month ne sont pas forcement alimentés pour les contrat Flex
            if (StrFunc.IsEmpty(year))
            {
                year = pParsingRowExpiration.GetRowDataValue(Cs, "Expiration Year");
            }
            if (StrFunc.IsEmpty(month))
            {
                month = pParsingRowExpiration.GetRowDataValue(Cs, "Expiration Month");
            }

            int iYear = IntFunc.IntValue(year);
            int iMonth = IntFunc.IntValue(month);
            int iDay = IntFunc.IntValue(day);
            int iDaysToExpiry = IntFunc.IntValue(daysToExpiry);

            DataTable dt = GetTableExpiration(CSTools.SetCacheOn(Cs, 1, null), pIdProduct, iYear, iMonth, iDay, out string queyAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAP_H"] = pIdProduct;
                dr["YEAR"] = iYear;
                dr["MONTH"] = iMonth;
                dr["DAY"] = iDay;
                dr["DAYTOEXPIRY"] = iDaysToExpiry;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queyAdapter, dt);
                dt = GetTableExpiration(CSTools.SetCacheOn(Cs, 1, null), pIdProduct, iYear, iMonth, iDay, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAE_H is not loaded for Product:{0} Year:{1} Month:{2} Day:{3}", pProductId, year, month, day));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdProduct"></param>
        /// <param name="pYear"></param>
        /// <param name="pMonth"></param>
        /// <param name="pDay"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableExpiration(string pCS, int pIdProduct, int pYear, int pMonth, int pDay, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAP_H", DbType.Int32), pIdProduct);
            dataParameters.Add(new DataParameter(pCS, "YEAR", DbType.Int32), pYear);
            dataParameters.Add(new DataParameter(pCS, "MONTH", DbType.Int32), pMonth);
            dataParameters.Add(new DataParameter(pCS, "DAY", DbType.Int32), pDay);

            string sqlQuery = "select * from dbo.IMPRISMAE_H where IDIMPRISMAP_H=@IDIMPRISMAP_H and YEAR=@YEAR and MONTH=@MONTH and DAY=@DAY";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);

            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAS_H qui correspond à une serie
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdExpiration">Id non significatif de l'échéance de la série</param>
        /// <param name="pIdAsset">Id non significatif de l'asset dans Spheres</param>
        /// <param name="pParsingRowSerie">Représente le parsing d'une ligne de type S</param>
        /// <param name="pParsingRowNeutralScenario">Représente le parsing d'une ligne de type N (contient le prix de l'asset)</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowSerie(int pIdExpiration, int pIdAsset, IOTaskDetInOutFileRow pParsingRowSerie, IOTaskDetInOutFileRow pParsingRowNeutralScenario)
        {
            // ROW SERIE
            string callPut = pParsingRowSerie.GetRowDataValue(Cs, "Call Put Flag");
            string strike = pParsingRowSerie.GetRowDataValue(Cs, "Exercise Price");
            string version = pParsingRowSerie.GetRowDataValue(Cs, "Series Version Number");
            string timeToExpiryBucketID = pParsingRowSerie.GetRowDataValue(Cs, "Time-To-Expiry Bucket ID");
            string moneynessBucketID = pParsingRowSerie.GetRowDataValue(Cs, "Moneyness Bucket ID");
            string riskBucket = pParsingRowSerie.GetRowDataValue(Cs, "Risk Bucket");
            string status = pParsingRowSerie.GetRowDataValue(Cs, "Series Status");
            string tradingUnit = pParsingRowSerie.GetRowDataValue(Cs, "Trading Unit");
            string vega = pParsingRowSerie.GetRowDataValue(Cs, "Option Vega");
            string volatility = pParsingRowSerie.GetRowDataValue(Cs, "Implied Volatility");
            string interestRate = pParsingRowSerie.GetRowDataValue(Cs, "Interest Rate");
            string flexProductId = pParsingRowSerie.GetRowDataValue(Cs, "Flex Product ID");
            string settlementType = pParsingRowSerie.GetRowDataValue(Cs, "Settlement Type");
            string seriesExerciseStyleFlag = pParsingRowSerie.GetRowDataValue(Cs, "Series Exercise Style Flag");
            string flexSeriesFlag = pParsingRowSerie.GetRowDataValue(Cs, "Flex Series Flag");

            Boolean isFlex = BoolFunc.IsTrue(flexSeriesFlag);
            decimal dStrike = Decimal.Zero;
            if (StrFunc.IsFilled(callPut))
                dStrike = DecFunc.DecValueFromInvariantCulture(strike);
            decimal dTradingUnit = DecFunc.DecValueFromInvariantCulture(tradingUnit);

            decimal dVega = DecFunc.DecValueFromInvariantCulture(vega);
            decimal dVolatility = DecFunc.DecValueFromInvariantCulture(volatility);
            decimal dInterestRate = DecFunc.DecValueFromInvariantCulture(interestRate);

            // ROW NEUTRAL SCENARIO
            string price = pParsingRowNeutralScenario.GetRowDataValue(Cs, "Theorical price for neutral scenario");
            decimal dPrice = DecFunc.DecValueFromInvariantCulture(price);

            DataTable dt = GetTableSeries(CSTools.SetCacheOn(Cs, 1, null), pIdExpiration, callPut, dStrike, version, settlementType, seriesExerciseStyleFlag, isFlex, out string queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAE_H"] = pIdExpiration;
                dr["PUTCALL"] = StrFunc.IsFilled(callPut) ? callPut : Convert.DBNull;
                dr["STRIKEPRICE"] = StrFunc.IsFilled(callPut) ? dStrike : Convert.DBNull;
                dr["VERSION"] = version;
                dr["IDASSET"] = pIdAsset;
                dr["ASSETCATEGORY"] = Cst.UnderlyingAsset.ExchangeTradedContract.ToString();
                dr["NPRICE"] = dPrice;
                dr["TTEBUCKETID"] = StrFunc.IsFilled(timeToExpiryBucketID) ? timeToExpiryBucketID : Convert.DBNull;
                dr["MONEYNESSBUCKETID"] = StrFunc.IsFilled(moneynessBucketID) ? moneynessBucketID : Convert.DBNull;
                dr["RISKBUCKET"] = StrFunc.IsFilled(riskBucket) ? riskBucket : Convert.DBNull;
                dr["STATUS"] = status;
                dr["TU"] = dTradingUnit;
                dr["DELTA"] = Convert.DBNull;
                dr["VEGA"] = dVega;
                dr["VOLATILITY"] = dVolatility;
                dr["INTERESTRATE"] = dInterestRate;
                dr["FLEXPRODUCTID"] = StrFunc.IsFilled(flexProductId) ? flexProductId : Convert.DBNull;
                dr["SETTLTMETHOD"] = settlementType;
                dr["EXERCISESTYLE"] = StrFunc.IsFilled(seriesExerciseStyleFlag) ? seriesExerciseStyleFlag : Convert.DBNull;
                //dr["ISFLEX"] = isFlex;
                dr["ISFLEX"] = OTCmlHelper.GetADONetBoolValue(Cs, isFlex.ToString());  //OTCmlHelper.GetADONetBoolValue for oracle  
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableSeries(CSTools.SetCacheOn(Cs, 1, null), pIdExpiration, callPut, dStrike, version, settlementType, seriesExerciseStyleFlag, isFlex, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAS_H is not loaded for asset (IdAsset):{0}", pIdAsset));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdExpiration"></param>
        /// <param name="pPutCall"></param>
        /// <param name="pStrikePrice"></param>
        /// <param name="pVersion"></param>
        /// <param name="pSettlementType"></param>
        /// <param name="pExerciseStyle"></param>
        /// <param name="pIsFlex"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableSeries(string pCS, int pIdExpiration, string pPutCall, Decimal pStrikePrice, String pVersion, string pSettlementType, string pExerciseStyle, bool pIsFlex, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAE_H", DbType.Int32), pIdExpiration);
            dataParameters.Add(new DataParameter(pCS, "VERSION", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pVersion);
            dataParameters.Add(new DataParameter(pCS, "ISFLEX", DbType.Boolean), pIsFlex);
            dataParameters.Add(new DataParameter(pCS, "SETTLTMETHOD", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pSettlementType);

            if (StrFunc.IsFilled(pPutCall))
            {
                dataParameters.Add(new DataParameter(pCS, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pPutCall);
                dataParameters.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal), pStrikePrice);
                dataParameters.Add(new DataParameter(pCS, "EXERCISESTYLE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pExerciseStyle);
            }

            string sqlQuery = @"
select * from dbo.IMPRISMAS_H 
where (IDIMPRISMAE_H=@IDIMPRISMAE_H )
and (VERSION=@VERSION) and (ISFLEX=@ISFLEX) and (SETTLTMETHOD=@SETTLTMETHOD)
and (PUTCALL=@PUTCALL) and (STRIKEPRICE=@STRIKEPRICE) and (EXERCISESTYLE=@EXERCISESTYLE)";
            if (StrFunc.IsEmpty(pPutCall))
            {
                sqlQuery = sqlQuery.Replace("PUTCALL=@PUTCALL", "PUTCALL is null");
                sqlQuery = sqlQuery.Replace("STRIKEPRICE=@STRIKEPRICE", "STRIKEPRICE is null");
                sqlQuery = sqlQuery.Replace("EXERCISESTYLE=@EXERCISESTYLE", "EXERCISESTYLE is null");
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne un enregistrement dans IMPRISMALGSS_H qui correspond au LGS of serie
        /// <para>Alimente IMPRISMALGSS_H si l'enregistrement n'existe pas</para>
        /// </summary>
        /// <param name="pIdSerie">id non significatif d'une serie</param>
        /// <param name="pIdLiquidationGroupSplit">id non significatif d'un Liquidation Group Split</param>
        /// <param name="pParsingRowLH">Représente le parsing d'une ligne type LH</param>
        /// <param name="pParsingRowLiquidationGroupSplit">Représente le parsing d'une ligne type LGS (nécessairement null si Release 1)</param>
        /// <returns></returns>
        private DataRow LoadRowLiquidationGroupSplitOfSerie(int pIdSerie, int pIdLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRowLH, IOTaskDetInOutFileRow pParsingRowLiquidationGroupSplit)
        {
            string liquidationHorizon = pParsingRowLH.GetRowDataValue(Cs, "Liquidation Horizon");
            int iLiquidationHorizon = Convert.ToInt32(liquidationHorizon);

            string defaultLGSIndicator = "Y";
            if (null != pParsingRowLiquidationGroupSplit)
                defaultLGSIndicator = pParsingRowLiquidationGroupSplit.GetRowDataValue(Cs, "Default LGS Indicator");

            DataTable dt = GetTableLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(Cs, 1, null), pIdSerie, pIdLiquidationGroupSplit, out string queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAS_H"] = pIdSerie;
                dr["IDIMPRISMALGS_H"] = pIdLiquidationGroupSplit;
                dr["LH"] = iLiquidationHorizon;
                dr["ISDEFAULT"] = OTCmlHelper.GetADONetBoolValue(Cs, defaultLGSIndicator);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(Cs, 1, null), pIdSerie, pIdLiquidationGroupSplit, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMALGSS_H is not loaded for serie (IdSerie):{0} and liquidation group Split:{1}", pIdSerie, pIdLiquidationGroupSplit));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdSerie"></param>
        /// <param name="pIdLiquidationGroupSplit"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableLiquidationGroupSplitOfSerie(string pCS, int pIdSerie, int pIdLiquidationGroupSplit, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAS_H", DbType.Int32), pIdSerie);
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALGS_H", DbType.Int32), pIdLiquidationGroupSplit);

            string sqlQuery = "select * from dbo.IMPRISMALGSS_H where IDIMPRISMAS_H=@IDIMPRISMAS_H and IDIMPRISMALGS_H=@IDIMPRISMALGS_H";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMARMS_H qui correspond au Risk Measure Set {pRowRMS}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMARMS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowRiskMeasuresSet(IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string identifier;
            switch (InputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                    identifier = pParsingRowRMS.GetRowDataValue(Cs, "Risk Measure Set");
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    identifier = pParsingRowRMS.GetRowDataValue(Cs, "Risk Measure Set ID");
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("dataStyle :{0} is not implemented", dataStyle));
            }

            DataTable dt = GetTableRiskMeasuresSet(CSTools.SetCacheOn(Cs, 1, null), identifier, idIMPRISMA_H, out string queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                dr["IDENTIFIER"] = identifier;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableRiskMeasuresSet(CSTools.SetCacheOn(Cs, 1, null), identifier, idIMPRISMA_H, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMARMS_H is not loaded for identifier:{0}", identifier));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableRiskMeasuresSet(string pCS, string pIdentifier, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);

            string sqlQuery = "select * from dbo.IMPRISMARMS_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        ///  Retourne l'enregsitrement dans IMPRISMARMSLGS_H qui correspond au RMS {pIdRiskMeasureSet} rattaché au LGS {pIdLiquidationGroupSplit}
        ///  <para>Alimente IMPRISMARMSLGS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdLiquidationGroupSplit">Id non significatif d'un LGS</param>
        /// <param name="pIdRiskMeasureSet">Id non significatif d'un RMS (Risk Measure Set)</param>
        /// <param name="pParsingRowRMS">Parsing d'une ligne de type RMS</param>
        /// <returns></returns>
        private DataRow LoadRiskMeasuresSetOfLiquidationGroupSplit(int pIdLiquidationGroupSplit, int pIdRiskMeasureSet, IOTaskDetInOutFileRow pParsingRowRMS)
        {
            DataTable dt = GetTableRiskMeasuresSetOfLiquidationGroupSplit(CSTools.SetCacheOn(Cs, 1, null), pIdLiquidationGroupSplit, pIdRiskMeasureSet, out string queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMALGS_H"] = pIdLiquidationGroupSplit;
                dr["IDIMPRISMARMS_H"] = pIdRiskMeasureSet;
                dr["ISUSEROBUSTNESS"] = OTCmlHelper.GetADONetBoolValue(Cs, "false"); // Valeur par défaut de la table , nécessaire pour Oracle 
                dr["ISCORRELATIONBREAK"] = OTCmlHelper.GetADONetBoolValue(Cs, "false"); // Valeur par défaut de la table , nécessaire pour Oracle 
                dr["ISLIQUIDCOMPONENT"] = OTCmlHelper.GetADONetBoolValue(Cs, "false"); // Valeur par défaut de la table, nécessaire pour Oracle 
                SetDataRowRMSLGS(Cs, dr, pParsingRowRMS, InputSourceDataStyle);
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableRiskMeasuresSetOfLiquidationGroupSplit(CSTools.SetCacheOn(Cs, 1, null), pIdLiquidationGroupSplit, pIdRiskMeasureSet, out _);
            }
            else if (Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE != InputSourceDataStyle)
            {
                DataRow dr = dt.Rows[0];
                SetDataRowRMSLGS(Cs, dr, pParsingRowRMS, InputSourceDataStyle);
                //SetDataRowUpd(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                DataHelper.queryCache.Remove("IMPRISMARMSLGS_H", Cs);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMARMSLGS_H is not loaded for LGS:{0} and RMS:{0}", pIdLiquidationGroupSplit, pIdRiskMeasureSet));
            }

            return dt.Rows[0];
        }

        /// <summary>
        ///  Aliemnte le datarow {pDataRow} avec les informations présentes dans une ligne RMS
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="pDataRow">DataRow mis à jour</param>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS</param>
        /// <param name="pDataStyle">Type de fichier source</param>
        private static void SetDataRowRMSLGS(string cs, DataRow pDataRow, IOTaskDetInOutFileRow pParsingRowRMS, Cst.InputSourceDataStyle pDataStyle)
        {
            switch (pDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    string historicalStessed = pParsingRowRMS.GetRowDataValue(cs, "Historical / Stressed");
                    string riskMeasure = pParsingRowRMS.GetRowDataValue(cs, "Risk Measure");
                    string anchorConfidenceLevel = pParsingRowRMS.GetRowDataValue(cs, "Anchor Confidence Level");
                    string robustness = pParsingRowRMS.GetRowDataValue(cs, "Robustness");
                    string scallingFactor = pParsingRowRMS.GetRowDataValue(cs, "Scaling Factor");
                    string correlationBreakFlag = pParsingRowRMS.GetRowDataValue(cs, "Correlation Break Flag");
                    string movingSubWindow = pParsingRowRMS.GetRowDataValue(cs, "Moving Sub-Window");
                    string confidenceLevelCorrelationBreak = pParsingRowRMS.GetRowDataValue(cs, "Confidence Level Correlation Break");
                    string cap = pParsingRowRMS.GetRowDataValue(cs, "Cap");
                    string floor = pParsingRowRMS.GetRowDataValue(cs, "Floor");
                    string multiplier = pParsingRowRMS.GetRowDataValue(cs, "Multiplier");
                    string liquidityRiskComponent = pParsingRowRMS.GetRowDataValue(cs, "Liquidity Risk Component");
                    string confidenceLevelDiversificationFactor = pParsingRowRMS.GetRowDataValue(cs, "Confidence Level Diversification Factor");
                    string alphaFloor = pParsingRowRMS.GetRowDataValue(cs, "Alpha Floor");
                    // PM 20161019 [22174] Prisma 5.0 : Ajout nbWorstScenario
                    string nbWorstScenario = pParsingRowRMS.GetRowDataValue(cs, "NoWorstScenarios");

                    pDataRow["HISTORICALSTRESSED"] = historicalStessed;
                    pDataRow["RISKMEASURE"] = riskMeasure;
                    pDataRow["CONFIDENCELEVEL"] = Convert.ToDecimal(anchorConfidenceLevel);
                    //Robustness
                    //pDataRow["ISUSEROBUSTNESS"] = BoolFunc.IsTrue(robustness) 
                    pDataRow["ISUSEROBUSTNESS"] = OTCmlHelper.GetADONetBoolValue(cs, robustness); //OTCmlHelper.GetADONetBoolValue for oracle  
                    pDataRow["SCALINGFACTOR"] = Convert.ToDecimal(scallingFactor);
                    // Correlation Break
                    //pDataRow["ISCORRELATIONBREAK"] = BoolFunc.IsTrue(correlationBreakFlag);
                    pDataRow["ISCORRELATIONBREAK"] = OTCmlHelper.GetADONetBoolValue(cs, correlationBreakFlag); //OTCmlHelper.GetADONetBoolValue for oracle  
                    if (StrFunc.IsFilled(movingSubWindow))
                    {
                        pDataRow["CBSUBWINDOW"] = Convert.ToInt32(movingSubWindow);
                    }
                    if (StrFunc.IsFilled(confidenceLevelCorrelationBreak))
                    {
                        pDataRow["CBCONFIDENCELEVEL"] = Convert.ToDecimal(confidenceLevelCorrelationBreak);
                    }
                    if (StrFunc.IsFilled(cap))
                    {
                        pDataRow["CBCAP"] = Convert.ToDecimal(cap);
                    }
                    if (StrFunc.IsFilled(floor))
                    {
                        pDataRow["CBFLOOR"] = Convert.ToDecimal(floor);
                    }
                    if (StrFunc.IsFilled(multiplier))
                    {
                        pDataRow["CBMULTIPLIER"] = Convert.ToDecimal(multiplier);
                    }
                    // Liquidity Risk
                    //pDataRow["ISLIQUIDCOMPONENT"] = BoolFunc.IsTrue(liquidityRiskComponent);
                    pDataRow["ISLIQUIDCOMPONENT"] = OTCmlHelper.GetADONetBoolValue(cs, liquidityRiskComponent); //OTCmlHelper.GetADONetBoolValue for oracle  
                    if (StrFunc.IsFilled(confidenceLevelDiversificationFactor))
                    {
                        pDataRow["DFACONFIDENCELEVEL"] = Convert.ToDecimal(confidenceLevelDiversificationFactor);
                    }
                    if (StrFunc.IsFilled(alphaFloor))
                    {
                        pDataRow["DFAFLOOR"] = Convert.ToDecimal(alphaFloor);
                    }
                    // PM 20161019 [22174] Prisma 5.0 : Ajout nbWorstScenario
                    if (StrFunc.IsFilled(nbWorstScenario))
                    {
                        pDataRow["NBWORSTSCENARIO"] = Convert.ToInt32(nbWorstScenario);
                    }
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    string weightingFactor = pParsingRowRMS.GetRowDataValue(cs, "Weighting Factor");
                    string aggregationMethod = pParsingRowRMS.GetRowDataValue(cs, "Aggregation Method");

                    pDataRow["WEIGHTINGFACTOR"] = Convert.ToDecimal(weightingFactor);
                    pDataRow["AGGREGATIONMETHOD"] = Convert.ToString(aggregationMethod);
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    // NOTHING TO DO
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Data Style:{0} is not implemented", pDataStyle));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdLiquidationGroupSplit"></param>
        /// <param name="pIdRiskMeasureSet"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableRiskMeasuresSetOfLiquidationGroupSplit(string pCS, int pIdLiquidationGroupSplit, int pIdRiskMeasureSet, out string pQuery)
        {

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALGS_H", DbType.Int32), pIdLiquidationGroupSplit);
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMARMS_H", DbType.Int32), pIdRiskMeasureSet);

            string sqlQuery = "select * from dbo.IMPRISMARMSLGS_H where IDIMPRISMALGS_H=@IDIMPRISMALGS_H and IDIMPRISMARMS_H=@IDIMPRISMARMS_H";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAFX_H qui correspond au FX Set {pRowFX}
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAFX_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pParsingRowFX">Représente le parsing d'une ligne de type FX</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowFX(IOTaskDetInOutFileRow pParsingRowFX)
        {
            string identifier = pParsingRowFX.GetRowDataValue(Cs, "FX Set");

            DataTable dt = GetTableFX(CSTools.SetCacheOn(Cs, 1, null), identifier, idIMPRISMA_H, out string queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMA_H"] = idIMPRISMA_H;
                dr["IDENTIFIER"] = identifier;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableFX(CSTools.SetCacheOn(Cs, 1, null), identifier, idIMPRISMA_H, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAFX_H is not loaded for identifier:{0}", identifier));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// Lecture de la table IMPRISMAFX_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdIMPRISMA_H"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableFX(string pCS, string pIdentifier, int pIdIMPRISMA_H, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMA_H", DbType.Int32), pIdIMPRISMA_H);
            dataParameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);

            string sqlQuery = "select * from dbo.IMPRISMAFX_H where IDIMPRISMA_H=@IDIMPRISMA_H and IDENTIFIER=@IDENTIFIER";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregistrement dans IMPRISMAFXPAIR_H qui correspond au FX Set {pRowFX} et au currencyPair
        /// <para>Ajoute l'enregistrement ds la table IMPRISMAFXPAIR_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdPrismaFx_H">Identifiant non significatif du Fx Set</param>
        /// <param name="pFxSetIdentifier">Identifiant significatif du Fx Set</param>
        /// <param name="pParsingRowFxPair">Représente le parsing d'une ligne de type P (Currency Pair)</param>
        /// <param name="pParsingRowCurrentExchangeRate">Représente le parsing d'une ligne de type C (Current Exchange Rate)</param>
        /// <exception cref="Exception s'il n'existe pas d'enregistrement"></exception>
        /// <returns></returns>
        private DataRow LoadRowFxPair(int pIdPrismaFx_H, IOTaskDetInOutFileRow pParsingRowFxPair, IOTaskDetInOutFileRow pParsingRowCurrentExchangeRate)
        {
            string currencyPair = pParsingRowFxPair.GetRowDataValue(Cs, "Currency pair");
            string currentExchangeRate = pParsingRowCurrentExchangeRate.GetRowDataValue(Cs, "Current Exchange Rate");

            decimal dCurrentExchangeRate = DecFunc.DecValueFromInvariantCulture(currentExchangeRate);

            DataTable dt = GetTableFxPair(Cs, pIdPrismaFx_H, currencyPair, out string queryAdapter);
            if (dt.Rows.Count == 0)
            {
                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAFX_H"] = pIdPrismaFx_H;
                dr["CURRENCYPAIR"] = currencyPair;
                dr["EXCHANGERATE"] = dCurrentExchangeRate;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableFxPair(Cs, pIdPrismaFx_H, currencyPair, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMAFXPAIR_H is not loaded for Fx Set:{0], Currency pair:{1}", currencyPair));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMPRISMAFX_H"></param>
        /// <param name="pCurrencyPair"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableFxPair(string pCS, int pIdIMPRISMAFX_H, string pCurrencyPair, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAFX_H", DbType.Int32), pIdIMPRISMAFX_H);
            dataParameters.Add(new DataParameter(pCS, "CURRENCYPAIR", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pCurrencyPair);

            string sqlQuery = "select * from dbo.IMPRISMAFXPAIR_H where IDIMPRISMAFX_H=@IDIMPRISMAFX_H and CURRENCYPAIR=@CURRENCYPAIR";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        /// Retourne l'enregsitrement dans IMPRISMARMSLGSS_H (RMS of LGS of Serie)
        /// <para>Alimente la table IMPRISMARMSLGSS_H s'il n'existe pas</para>
        /// </summary>
        /// <param name="pIdSerie">Représente une serie</param>
        /// <param name="pIdRiskMeasureSetOfLiquidationGroupSplit"></param>
        /// <param name="pIdFxRate">Représente les cours de change appliqués au couple serie/RMSofLGS</param>
        /// <param name="pParsingRowLH">Représente le parsing d'une ligne type LH</param>
        /// <returns></returns>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout pParsingRowLH
        private DataRow LoadRowRiskMeasuresSetOfLiquidationGroupSplitOfSerie(int pIdSerie, int pIdRiskMeasureSetOfLiquidationGroupSplit, int pIdFxRate, IOTaskDetInOutFileRow pParsingRowLH)
        {
            DataTable dt = GetTableRiskMeasuresSetOfLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(Cs, 1, null), pIdSerie, pIdRiskMeasureSetOfLiquidationGroupSplit, out string queryAdapter);
            // PM 20161019 [22174] Prisma 5.0 : Ajout Liquidation Horizon
            //if (dt.Rows.Count == 0)
            if ((0 == dt.Rows.Count) && (default != pParsingRowLH))
            {
                string liquidationHorizon = pParsingRowLH.GetRowDataValue(Cs, "Liquidation Horizon");
                int iLiquidationHorizon = Convert.ToInt32(liquidationHorizon);

                DataRow dr = dt.NewRow();
                dr["IDIMPRISMAS_H"] = pIdSerie;
                dr["IDIMPRISMARMSLGS_H"] = pIdRiskMeasureSetOfLiquidationGroupSplit;
                dr["IDIMPRISMAFX_H"] = pIdFxRate;
                dr["LH"] = iLiquidationHorizon;
                //FI 20140617 [19911] mise en commentaire
                //SetDataRowIns(dr);
                dt.Rows.Add(dr);

                DataHelper.ExecuteDataAdapter(Cs, queryAdapter, dt);
                dt = GetTableRiskMeasuresSetOfLiquidationGroupSplitOfSerie(CSTools.SetCacheOn(Cs, 1, null), pIdSerie, pIdRiskMeasureSetOfLiquidationGroupSplit, out _);
            }

            if (dt.Rows.Count == 0)
            {
                throw new Exception(StrFunc.AppendFormat("IMPRISMARMSLGSS_H is not loaded for identifier"));
            }

            return dt.Rows[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdSerie"></param>
        /// <param name="pIdRiskMeasureSetOfLiquidationGroupSplit"></param>
        /// <param name="pQuery"></param>
        /// <returns></returns>
        private static DataTable GetTableRiskMeasuresSetOfLiquidationGroupSplitOfSerie(string pCS, int pIdSerie, int pIdRiskMeasureSetOfLiquidationGroupSplit, out string pQuery)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMAS_H", DbType.Int32), pIdSerie);
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMARMSLGS_H", DbType.Int32), pIdRiskMeasureSetOfLiquidationGroupSplit);

            string sqlQuery = "select * from dbo.IMPRISMARMSLGSS_H where IDIMPRISMAS_H=@IDIMPRISMAS_H and IDIMPRISMARMSLGS_H=@IDIMPRISMARMSLGS_H";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            pQuery = qryParameters.GetQueryReplaceParameters();

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return dt;
        }

        /// <summary>
        ///  Insertion des prix des n scenarios d'une serie pour un RMS
        /// </summary>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="pLH">Liquidation Horizon</param>
        private void AddSPValues(IOTaskDetInOutFileRow pParsingRowSP, int pIdRMSLGSS, int pLH)
        {
            string sql = BuildInsertSPValues(Cs, pIdRMSLGSS, pParsingRowSP, pLH);
            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParsingRowSP"></param>
        /// <param name="pIdRMSLGSS"></param>
        /// <param name="pLH"></param>
        /// FI 20140430 [XXXXX] New methode
        private void AddSPValues2(IOTaskDetInOutFileRow pParsingRowSP, int pIdRMSLGSS, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(Cs, "Scenarios Prices").Split(';');

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQueryInsertSPValues(Cs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.Parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                    DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                i += pLH;
            }
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildInsertSPValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            StrBuilder finalQuery = new StrBuilder();

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQueryInsertSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.Parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                    finalQuery += qry.GetQueryReplaceParameters(false);
                i += pLH;
            }

            return finalQuery.ToString();
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildSelectSPValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            StringBuilder finalQuery = new StringBuilder();

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQuerySelectSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.Parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                {
                    if (finalQuery.Length > 0)
                        finalQuery.Append(SQLCst.UNIONALL + Cst.CrLf);
                    finalQuery.Append(qry.GetQueryReplaceParameters(false));
                }
                i += pLH;
            }

            finalQuery.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMASPS_H(IDIMPRISMARMSLGSS_H, SCENARIONO, PRICE1, PRICE2, PRICE3, PRICE4, PRICE5)" + Cst.CrLf);
            finalQuery.Append(";");

            return finalQuery.ToString();
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <param name="pNeutralPrice">Neutral Price</param>
        /// <returns></returns>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout de pNeutralPrice
        private static string BuildSelectSPValues2(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH, decimal pNeutralPrice)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            StringBuilder finalQuery = new StringBuilder();

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQuerySelectSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        // PM 20161019 [22174] Prisma 5.0 : Prendre le Neutral Price lorsque la valeur de SP n'est pas renseignée
                        //qry.parameters[col].Value = Convert.ToDecimal(spValues[i + j]); 
                        if (StrFunc.IsEmpty(spValues[i + j]))
                        {
                            qry.Parameters[col].Value = pNeutralPrice;
                        }
                        else
                        {
                            qry.Parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                        }
                    }
                }
                if (isAddQuery)
                {
                    if (finalQuery.Length > 0)
                    {
                        finalQuery.Append(SQLCst.UNIONALL + Cst.CrLf);
                    }
                    finalQuery.Append(qry.GetQueryReplaceParameters(false));
                }
                i += pLH;
            }

            return finalQuery.ToString();
        }

        /// <summary>
        ///  Retourne un script SQL constitué de n insert qui alimente la table des prix des series
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowSP">Représente le parsing d'une ligne de type SP</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private QueryParameters BuildInsertSPValues2(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowSP, int pLH)
        {
            string[] spValues = pParsingRowSP.GetRowDataValue(pCs, "Scenarios Prices").Split(';');

            QueryParameters ret = null;

            int scenarionNb = 0;
            int i = 0;
            Boolean isOk = true;
            while (isOk)
            {
                scenarionNb++;
                Boolean isAddQuery = false;
                QueryParameters qry = GetQueryInsertSPValues(pCs, pIdRMSLGSS, scenarionNb);
                for (int j = 0; j < pLH; j++)
                {
                    isOk = ((i + j) < ArrFunc.Count(spValues));
                    if (isOk)
                    {
                        isAddQuery = true;
                        string col = "PRICE" + (j + 1).ToString();
                        qry.Parameters[col].Value = Convert.ToDecimal(spValues[i + j]);
                    }
                }
                if (isAddQuery)
                {
                    ret = qry;
                }
                i += pLH;
            }

            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMASPS_H (prix des series pour les différents scenarios) 
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="pScenarionNb">Numéro de scenario</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertSPValues(string pCs, int pIdRMSLGSS, int pScenarionNb)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            for (int i = 1; i <= 5; i++)
            {
                dp.Add(new DataParameter(pCs, "PRICE" + i.ToString(), DbType.Decimal), Convert.DBNull);
            }
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), pScenarionNb);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMASPS_H") + Cst.CrLf;
            sql += "(IDIMPRISMARMSLGSS_H, SCENARIONO, PRICE1, PRICE2, PRICE3, PRICE4, PRICE5)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMARMSLGSS_H, @SCENARIONO,@PRICE1,@PRICE2,@PRICE3,@PRICE4,@PRICE5);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMASPS_H (prix des series pour les différents scenarios) 
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="pScenarionNb">Numéro de scenario</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectSPValues(string pCs, int pIdRMSLGSS, int pScenarionNb)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            for (int i = 1; i <= 5; i++)
            {
                dp.Add(new DataParameter(pCs, "PRICE" + i.ToString(), DbType.Decimal), Convert.DBNull);
            }
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), pScenarionNb);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMARMSLGSS_H,@SCENARIONO,@PRICE1,@PRICE2,@PRICE3,@PRICE4,@PRICE5" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Insertion des compressions Error associé à une devise
        /// </summary>
        /// <param name="pParsingRowCE">Représente le parsing d'une ligne de type CE</param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="pLH">Liquidation Horizon</param>
        private void AddCEValues(IOTaskDetInOutFileRow pParsingRowCE, int pIdRMSLGSS, int pLH)
        {
            string sql = BuildInsertCEValues(Cs, pIdRMSLGSS, pParsingRowCE, pLH);
            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sql);
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowCE">Représente le parsing d'une ligne de type CE</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildInsertCEValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowCE, int pLH)
        {
            string[] spValues = pParsingRowCE.GetRowDataValue(pCs, "Compression Error/Currency").Split(';');
            string idC = spValues[ArrFunc.Count(spValues) - 1];

            QueryParameters qry = GetQueryInsertCEValues(pCs, pIdRMSLGSS);
            for (int j = 0; j < pLH; j++)
            {
                string col = "CE" + (j + 1).ToString();
                qry.Parameters[col].Value = Convert.ToDecimal(spValues[j]);
            }
            qry.Parameters["IDC"].Value = idC;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowCE">Représente le parsing d'une ligne de type CE</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildSelectCEValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowCE, int pLH)
        {
            string[] spValues = pParsingRowCE.GetRowDataValue(pCs, "Compression Error/Currency").Split(';');
            string idC = spValues[ArrFunc.Count(spValues) - 1];

            QueryParameters qry = GetQuerySelectCEValues(pCs, pIdRMSLGSS);
            for (int j = 0; j < pLH; j++)
            {
                string col = "CE" + (j + 1).ToString();
                qry.Parameters[col].Value = Convert.ToDecimal(spValues[j]);
            }
            qry.Parameters["IDC"].Value = idC;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMACES_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertCEValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            for (int i = 1; i <= 5; i++)
                dp.Add(new DataParameter(pCs, "CE" + i.ToString(), DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMACES_H") + Cst.CrLf;
            sql += "(IDIMPRISMARMSLGSS_H, IDC, CE1, CE2, CE3, CE4, CE5)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMARMSLGSS_H, @IDC,@CE1,@CE2,@CE3,@CE4,@CE5);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMACES_H  
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectCEValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            for (int i = 1; i <= 5; i++)
                dp.Add(new DataParameter(pCs, "CE" + i.ToString(), DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMARMSLGSS_H,@IDC,@CE1,@CE2,@CE3,@CE4,@CE5" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// </summary>
        /// <param name="pParsingRowVar">Représente le parsing d'une ligne de type VAR ou IVAR</param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        private void AddVaRValues(IOTaskDetInOutFileRow pParsingRowVar, int pIdRMSLGSS)
        {
            string sql = BuildInsertVaRValues(Cs, pIdRMSLGSS, pParsingRowVar);
            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sql);
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMAVARS_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowVaR">Représente le parsing d'une ligne de type VAR ou IVAR</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildInsertVaRValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowVaR)
        {
            string IVAR = pParsingRowVaR.GetRowDataValue(pCs, "IVAR");
            string AIVAR = pParsingRowVaR.GetRowDataValue(pCs, "AIVAR");
            string varType = StrFunc.IsFilled(IVAR) ? IVAR : AIVAR;

            string idC = pParsingRowVaR.GetRowDataValue(pCs, "Currency");
            string longShortIndicator = pParsingRowVaR.GetRowDataValue(pCs, "Long Short Indicator");

            string amount = string.Empty;
            if (varType == "IVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Instrument VaR");
            else if (varType == "AIVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Additional Instrument VaR");
            decimal dAmount = DecFunc.DecValueFromInvariantCulture(amount);

            QueryParameters qry = GetQueryInsertVaRValues(pCs, pIdRMSLGSS);
            qry.Parameters["IDC"].Value = idC;
            qry.Parameters["SHORTLONG"].Value = longShortIndicator;
            qry.Parameters["VARTYPE"].Value = varType;
            qry.Parameters["VARAMOUNT"].Value = dAmount;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMAVARS_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowVaR">Représente le parsing d'une ligne de type VAR ou IVAR</param>
        /// <param name="pLH">Liquidation Horizon</param>
        /// <returns></returns>
        private static string BuildSelectVaRValues(string pCs, int pIdRMSLGSS, IOTaskDetInOutFileRow pParsingRowVaR)
        {
            string IVAR = pParsingRowVaR.GetRowDataValue(pCs, "IVAR");
            string AIVAR = pParsingRowVaR.GetRowDataValue(pCs, "AIVAR");
            string varType = StrFunc.IsFilled(IVAR) ? IVAR : AIVAR;

            string idC = pParsingRowVaR.GetRowDataValue(pCs, "Currency");
            string longShortIndicator = pParsingRowVaR.GetRowDataValue(pCs, "Long Short Indicator");

            string amount = string.Empty;
            if (varType == "IVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Instrument VaR");
            else if (varType == "AIVAR")
                amount = pParsingRowVaR.GetRowDataValue(pCs, "Additional Instrument VaR");
            decimal dAmount = DecFunc.DecValueFromInvariantCulture(amount);

            QueryParameters qry = GetQuerySelectVaRValues(pCs, pIdRMSLGSS);
            qry.Parameters["IDC"].Value = idC;
            qry.Parameters["SHORTLONG"].Value = longShortIndicator;
            qry.Parameters["VARTYPE"].Value = varType;
            qry.Parameters["VARAMOUNT"].Value = dAmount;

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAVARS_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertVaRValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARAMOUNT", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SHORTLONG", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMAVARS_H") + Cst.CrLf;
            sql += "(IDIMPRISMARMSLGSS_H, IDC, SHORTLONG, VARTYPE, VARAMOUNT)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMARMSLGSS_H, @IDC,@SHORTLONG,@VARTYPE,@VARAMOUNT);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAVARS_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">RMS of LGS of Serie</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectVaRValues(string pCs, int pIdRMSLGSS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMSLGSS_H", DbType.Int32), pIdRMSLGSS);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDC), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARAMOUNT", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SHORTLONG", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "VARTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT) + Cst.CrLf;
            sql += "@IDIMPRISMARMSLGSS_H,@IDC,@SHORTLONG,@VARTYPE,@VARAMOUNT" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMALIQFACT_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de la classe de Liquidation</param>
        /// <param name="pParsingRowLiquidityFactor">Représente le parsing d'une ligne du fichier Liquidity Factors Configuration</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static string BuildInsertLiquidityFactors(string pCs, int pIdLiquidityClass, IOTaskDetInOutFileRow pParsingRowLiquidityFactor)
        {
            string minPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Minimum Percentage Threshold");
            string maxPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Maximum Percentage Threshold");
            string liquidityFactorMinThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Minimum Threshold");
            string liquidityFactorMaxThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Maximum Threshold");

            QueryParameters qry = GetQueryInsertLiquidityFactorsValues(pCs, pIdLiquidityClass);
            qry.Parameters["PCTMINTHRESHOLD"].Value = Convert.ToDecimal(minPercentThreshold);
            if (StrFunc.IsFilled(maxPercentThreshold))
            {
                qry.Parameters["PCTMAXTHRESHOLD"].Value = Convert.ToDecimal(maxPercentThreshold);
            }
            qry.Parameters["MINTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMinThreshold);
            if (StrFunc.IsFilled(liquidityFactorMaxThreshold))
            {
                qry.Parameters["MAXTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMaxThreshold);
            }

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMALIQFACT_H
        ///  <para>Requête de type select </para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdRMSLGSS">id non significatif de la classe de Liquidation</param>
        /// <param name="pParsingRowLiquidityFactor">Représente le parsing d'une ligne du fichier Liquidity Factors Configuration</param>
        /// <returns></returns>
        private static string BuildSelectLiquidityFactors(string pCs, int pIdLiquidityClass, IOTaskDetInOutFileRow pParsingRowLiquidityFactor)
        {
            string minPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Minimum Percentage Threshold");
            string maxPercentThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Maximum Percentage Threshold");
            string liquidityFactorMinThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Minimum Threshold");
            string liquidityFactorMaxThreshold = pParsingRowLiquidityFactor.GetRowDataValue(pCs, "Liquidity Factor Maximum Threshold");

            QueryParameters qry = GetQuerySelectLiquidityFactorsValues(pCs, pIdLiquidityClass);
            qry.Parameters["PCTMINTHRESHOLD"].Value = Convert.ToDecimal(minPercentThreshold);
            if (StrFunc.IsFilled(maxPercentThreshold))
            {
                qry.Parameters["PCTMAXTHRESHOLD"].Value = Convert.ToDecimal(maxPercentThreshold);
            }
            qry.Parameters["MINTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMinThreshold);
            if (StrFunc.IsFilled(liquidityFactorMaxThreshold))
            {
                qry.Parameters["MAXTHRESHOLDFACTOR"].Value = Convert.ToDecimal(liquidityFactorMaxThreshold);
            }

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table IMPRISMALIQFACT_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdLiquidityClass"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertLiquidityFactorsValues(string pCs, int pIdLiquidityClass)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMALIQCLASS_H", DbType.Int32), pIdLiquidityClass);
            dp.Add(new DataParameter(pCs, "PCTMINTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PCTMAXTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MINTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MAXTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMALIQFACT_H") + Cst.CrLf;
            sql += "(IDIMPRISMALIQCLASS_H, PCTMINTHRESHOLD, PCTMAXTHRESHOLD, MINTHRESHOLDFACTOR, MAXTHRESHOLDFACTOR)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMALIQCLASS_H, @PCTMINTHRESHOLD, @PCTMAXTHRESHOLD, @MINTHRESHOLDFACTOR, @MAXTHRESHOLDFACTOR);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table IMPRISMALIQFACT_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdLiquidityClass"></param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectLiquidityFactorsValues(string pCs, int pIdLiquidityClass)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMALIQCLASS_H", DbType.Int32), pIdLiquidityClass);
            dp.Add(new DataParameter(pCs, "PCTMINTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PCTMAXTHRESHOLD", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MINTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MAXTHRESHOLDFACTOR", DbType.Decimal), Convert.DBNull);

            //StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMALIQFACT_H") + Cst.CrLf;
            //sql += "(IDIMPRISMALIQCLASS_H, PCTMINTHRESHOLD, PCTMAXTHRESHOLD, MINTHRESHOLDFACTOR, MAXTHRESHOLDFACTOR)" + Cst.CrLf;
            //sql += "Values(@IDIMPRISMALIQCLASS_H, @PCTMINTHRESHOLD, @PCTMAXTHRESHOLD, @MINTHRESHOLDFACTOR, @MAXTHRESHOLDFACTOR, @DTINS, @IDAINS);" + Cst.CrLf;

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMALIQCLASS_H,@PCTMINTHRESHOLD,@PCTMAXTHRESHOLD,@MINTHRESHOLDFACTOR,@MAXTHRESHOLDFACTOR" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);
            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMAMKTCAPA_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <param name="pParsingRowMarketCapacities">Représente le parsing d'une ligne du fichier Market capacities Configuration</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static string BuildInsertMarketCapacities(string pCs, int pIdPrismaH, IOTaskDetInOutFileRow pParsingRowMarketCapacities)
        {
            string productLine = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product Line");
            string productId = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product ID");
            string unlIsin = pParsingRowMarketCapacities.GetRowDataValue(pCs, "U/L_ISIN");
            string putCallFlag = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Put Call Flag");
            string timeToExpiryBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Time-To-Expiry Bucket ID");
            string moneynessBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Moneyness Bucket ID");
            string marketCapacity = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Market Capacity");
            string liquidityPremium = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Liquidity Premium");

            QueryParameters qry = GetQueryInsertMarketCapacitiesValues(pCs, pIdPrismaH);
            qry.Parameters["PRODUCTLINE"].Value = productLine;
            qry.Parameters["PRODUCTID"].Value = productId;
            qry.Parameters["ISINCODEUNL"].Value = unlIsin;
            if (StrFunc.IsFilled(putCallFlag))
                qry.Parameters["PUTCALL"].Value = putCallFlag;
            qry.Parameters["TTEBUCKETID"].Value = timeToExpiryBucketID;
            if (StrFunc.IsFilled(moneynessBucketID))
                qry.Parameters["MONEYNESSBUCKETID"].Value = moneynessBucketID;
            qry.Parameters["MARKETCAPACITY"].Value = Convert.ToDecimal(marketCapacity);
            qry.Parameters["LIQUIDITYPREMIUM"].Value = Convert.ToDecimal(liquidityPremium);

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL d'ajout d'un enregistrement de la table IMPRISMAMKTCAPA_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <param name="pParsingRowMarketCapacities">Représente le parsing d'une ligne du fichier Market capacities Configuration</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static string BuildSelectMarketCapacities(string pCs, int pIdPrismaH, IOTaskDetInOutFileRow pParsingRowMarketCapacities)
        {
            string productLine = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product Line");
            string productId = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Product ID");
            string unlIsin = pParsingRowMarketCapacities.GetRowDataValue(pCs, "U/L_ISIN");
            string putCallFlag = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Put Call Flag");
            string timeToExpiryBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Time-To-Expiry Bucket ID");
            string moneynessBucketID = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Moneyness Bucket ID");
            string marketCapacity = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Market Capacity");
            string liquidityPremium = pParsingRowMarketCapacities.GetRowDataValue(pCs, "Liquidity Premium");

            QueryParameters qry = GetQuerySelectMarketCapacitiesValues(pCs, pIdPrismaH);
            qry.Parameters["PRODUCTLINE"].Value = productLine;
            qry.Parameters["PRODUCTID"].Value = productId;
            qry.Parameters["ISINCODEUNL"].Value = unlIsin;
            if (StrFunc.IsFilled(putCallFlag))
                qry.Parameters["PUTCALL"].Value = putCallFlag;
            qry.Parameters["TTEBUCKETID"].Value = timeToExpiryBucketID;
            if (StrFunc.IsFilled(moneynessBucketID))
                qry.Parameters["MONEYNESSBUCKETID"].Value = moneynessBucketID;
            qry.Parameters["MARKETCAPACITY"].Value = Convert.ToDecimal(marketCapacity);
            qry.Parameters["LIQUIDITYPREMIUM"].Value = Convert.ToDecimal(liquidityPremium);

            string ret = qry.GetQueryReplaceParameters(false);
            return ret;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table de la table IMPRISMAMKTCAPA_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertMarketCapacitiesValues(string pCs, int pIdPrismaH)
        {
            DataParameters dp = new DataParameters();

            dp.Add(new DataParameter(pCs, "IDIMPRISMA_H", DbType.Int32), pIdPrismaH);
            dp.Add(new DataParameter(pCs, "PRODUCTLINE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PRODUCTID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "ISINCODEUNL", DbType.AnsiString, SQLCst.UT_ISINCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "TTEBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MONEYNESSBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MARKETCAPACITY", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "LIQUIDITYPREMIUM", DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMAMKTCAPA_H") + Cst.CrLf;
            sql += "(IDIMPRISMA_H, PRODUCTLINE, PRODUCTID, ISINCODEUNL, PUTCALL, TTEBUCKETID, MONEYNESSBUCKETID, MARKETCAPACITY, LIQUIDITYPREMIUM)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMA_H, @PRODUCTLINE, @PRODUCTID, @ISINCODEUNL, @PUTCALL, @TTEBUCKETID, @MONEYNESSBUCKETID, @MARKETCAPACITY, @LIQUIDITYPREMIUM);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne une query de type Insert pour alimentation de la table de la table IMPRISMAMKTCAPA_H  
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdPrismaH"></param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectMarketCapacitiesValues(string pCs, int pIdPrismaH)
        {
            DataParameters dp = new DataParameters();

            dp.Add(new DataParameter(pCs, "IDIMPRISMA_H", DbType.Int32), pIdPrismaH);
            dp.Add(new DataParameter(pCs, "PRODUCTLINE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PRODUCTID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "ISINCODEUNL", DbType.AnsiString, SQLCst.UT_ISINCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "TTEBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MONEYNESSBUCKETID", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "MARKETCAPACITY", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "LIQUIDITYPREMIUM", DbType.Decimal), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMA_H,@PRODUCTLINE,@PRODUCTID,@ISINCODEUNL,@PUTCALL,@TTEBUCKETID,@MONEYNESSBUCKETID,@MARKETCAPACITY,@LIQUIDITYPREMIUM" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);
            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        ///  Insertion des FX Rates 
        /// </summary>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS</param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">id non significatif de RMS</param>
        private void AddFXRatesValues(IOTaskDetInOutFileRow pParsingRowRMS, int pIdFxPair, int pIdRMS)
        {
            string sql = BuildInsertFXRatesValues(Cs, pIdFxPair, pIdRMS, pParsingRowRMS);
            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sql);
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">id non significatif de RMS</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS du fichier Foreign Exchante rates configuration</param>
        /// <returns></returns>
        private static string BuildInsertFXRatesValues(string pCs, int pIdFxPair, int pIdRMS, IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string[] values = pParsingRowRMS.GetRowDataValue(pCs, "Exchange Rate for scenarios").Split(';');

            StrBuilder sql = new StrBuilder();
            for (int i = 0; i < ArrFunc.Count(values); i++)
            {
                QueryParameters qry = GetQueryInsertFXRatesValues(pCs, pIdFxPair, pIdRMS);
                qry.Parameters["VALUE"].Value = DecFunc.DecValueFromInvariantCulture(values[i]);
                qry.Parameters["SCENARIONO"].Value = i + 1;

                sql += qry.GetQueryReplaceParameters(false);
            }
            string ret = sql.ToString();
            return ret;
        }

        /// <summary>
        ///  Retourne un script SQL constitué de 1 insert d'alimenation de la table IMPRISMACES_H
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">id non significatif de RMS</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pParsingRowRMS">Représente le parsing d'une ligne de type RMS du fichier Foreign Exchante rates configuration</param>
        /// <returns></returns>
        private static string BuildSelectFXRatesValues(string pCs, int pIdFxPair, int pIdRMS, IOTaskDetInOutFileRow pParsingRowRMS)
        {
            string[] values = pParsingRowRMS.GetRowDataValue(pCs, "Exchange Rate for scenarios").Split(';');

            StrBuilder sql = new StrBuilder();
            for (int i = 0; i < ArrFunc.Count(values); i++)
            {
                QueryParameters qry = GetQuerySelectFXRatesValues(pCs, pIdFxPair, pIdRMS);
                qry.Parameters["VALUE"].Value = DecFunc.DecValueFromInvariantCulture(values[i]);
                qry.Parameters["SCENARIONO"].Value = i + 1;
                if (sql.Length > 0)
                    sql.Append(SQLCst.UNIONALL + Cst.CrLf);
                sql += qry.GetQueryReplaceParameters(false);
            }
            return sql.ToString();
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAFXRMS_H  
        /// <para>Requête de type INSERT INTO</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">Id RMS</param>
        /// <param name="dtIns"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryInsertFXRatesValues(string pCs, int pIdFxPair, int pIdRMS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMAFXPAIR_H", DbType.Int32), pIdFxPair);
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMS_H", DbType.Int32), pIdRMS);
            dp.Add(new DataParameter(pCs, "VALUE", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.INSERT_INTO_DBO + "IMPRISMAFXRMS_H") + Cst.CrLf;
            sql += "(IDIMPRISMAFXPAIR_H, IDIMPRISMARMS_H, SCENARIONO, VALUE)" + Cst.CrLf;
            sql += "Values(@IDIMPRISMAFXPAIR_H, @IDIMPRISMARMS_H, @SCENARIONO,@VALUE);" + Cst.CrLf;

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }

        /// <summary>
        /// Retourne la requête élémentaire d'alimentation de la table IMPRISMAFXRMS_H  
        /// <para>Requête de type SELECT</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdFxPair">Id non significatif qui représente un couple de devise</param>
        /// <param name="pIdRMS">Id RMS</param>
        /// <returns></returns>
        private static QueryParameters GetQuerySelectFXRatesValues(string pCs, int pIdFxPair, int pIdRMS)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCs, "IDIMPRISMAFXPAIR_H", DbType.Int32), pIdFxPair);
            dp.Add(new DataParameter(pCs, "IDIMPRISMARMS_H", DbType.Int32), pIdRMS);
            dp.Add(new DataParameter(pCs, "VALUE", DbType.Decimal), Convert.DBNull);
            dp.Add(new DataParameter(pCs, "SCENARIONO", DbType.Int32), Convert.DBNull);

            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "@IDIMPRISMAFXPAIR_H,@IDIMPRISMARMS_H,@SCENARIONO,@VALUE" + Cst.CrLf;
            sql += DataHelper.SQLFromDual(pCs);

            QueryParameters qry = new QueryParameters(pCs, sql.ToString(), dp);
            return qry;
        }



        /// <summary>
        ///  Rerourne les groupes de liquidation split d'un groupe de liquidation
        /// </summary>
        /// <param name="pIdLiquidationGroup">Id non significatif du groupe le liquidation</param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private static List<Pair<Int32, string>> GetLGSOfLG(string pCS, int pIdLiquidationGroup)
        {
            List<Pair<Int32, string>> ret = new List<Pair<int, string>>();

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMPRISMALG_H", DbType.Int32), pIdLiquidationGroup);

            string sqlQuery = @"select lgs.IDIMPRISMALGS_H, lgs.IDENTIFIER 
  from dbo.IMPRISMALGS_H lgs
            inner join dbo.IMPRISMALG_H lg on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H) and (lg.IDIMPRISMALG_H = @IDIMPRISMALG_H)";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, dataParameters);
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    int idLGS = Convert.ToInt32(dr.GetValue(0));
                    string identifier = Convert.ToString(dr.GetValue(1));
                    ret.Add(new Pair<Int32, string>(idLGS, identifier));
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne les groupes de liquidation split d'un groupe de liquidation
        ///  <para>Cette méthode ne doit être utilisée que lors de l'importation des fichier de la Release 1</para>
        /// </summary>
        /// <param name="pIdLiquidationGroup"></param>
        /// <param name="pIdentifierLiquidationGroup"></param>
        /// <exception cref="Exception s'il n'existe pas aucun groupe de liquidation split"></exception>
        /// <returns></returns>
        private List<Pair<Int32, string>> CheckLGSOfLG(int pIdLiquidationGroup, string pIdentifierLiquidationGroup)
        {
            List<Pair<Int32, string>> ret = GetLGSOfLG(Cs, pIdLiquidationGroup);
            if (ArrFunc.IsEmpty(ret))
            {
                string file1 = "Theoretical prices and instrument";
                string file2;
                switch (InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        file2 = "Risk Measure Configutation";
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        file2 = "Risk Measure Aggregation Configutation";
                        break;
                    default:
                        throw new Exception($"File :{InputSourceDataStyle} is not implemented");
                }
                throw new Exception($"No Liquidation Group Split Found for Liquidation Group {pIdentifierLiquidationGroup}(id:{pIdLiquidationGroup}), please load file :{file1} before file :{file2}");
            }
            return ret;
        }

        /// <summary>
        /// Methode utilisée tester la méthode BuildInsertSPValues
        /// </summary>
        public static void TestBuildInsertSPValues()
        {
            string cs = @"Data Source=SVR-MSS-2\SVRMSS2;Initial Catalog=RD_PRISMA;Persist Security Info=False;User Id=sa;Password=efs98*;Workstation Id=127.0.0.1;Packet Size=4096";
            //cs = @"Data Source=DB02V112;User ID=RD_SPHERES_TST260;Password=efs";

            int id = 3;

            IOTaskDetInOutFileRow row = new IOTaskDetInOutFileRow
            {
                data = new IOTaskDetInOutFileRowData[2]
                {
                    new IOTaskDetInOutFileRowData()
                    {
                        name = "toto",
                        value = "TOTO"
                    },
                    new IOTaskDetInOutFileRowData()
                    {
                        name = "Scenarios Prices",
                        value = "1;2;3;4;5;6;7;8;9;10"
                    }
                }
            };


            int LH = 4;

            string SQL = BuildInsertSPValues(cs, id, row, LH);

            System.Diagnostics.Debug.WriteLine(SQL);
        }



        /// <summary>
        ///  Insertion des données dans tables SQL IMPRISMASPS_H, IMPRISMACES_H, IMPRISMAVARS_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sqlSP"></param>
        /// <param name="sqlCE"></param>
        /// <param name="sqlVAR"></param>
        private static void ExecuteQueryTheoriticalPriceFile(string pCS, StringBuilder sqlSP, StringBuilder sqlCE, StringBuilder sqlVAR)
        {
            if ((null != sqlSP) && sqlSP.Length > 0)
            {
                sqlSP.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMASPS_H(IDIMPRISMARMSLGSS_H, SCENARIONO, PRICE1, PRICE2, PRICE3, PRICE4, PRICE5)" + Cst.CrLf);
                sqlSP.Append(";");

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlSP.ToString());
            }

            if ((null != sqlCE) && sqlCE.Length > 0)
            {
                sqlCE.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMACES_H(IDIMPRISMARMSLGSS_H, IDC, CE1, CE2, CE3, CE4, CE5)");
                sqlCE.Append(";");

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlCE.ToString());
            }

            if ((null != sqlVAR) && sqlVAR.Length > 0)
            {
                sqlVAR.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMAVARS_H(IDIMPRISMARMSLGSS_H, IDC, SHORTLONG, VARTYPE, VARAMOUNT)");
                sqlVAR.Append(";");

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sqlVAR.ToString());
            }
        }

        /// <summary>
        /// Insertion des données dans tables SQL IMPRISMASPS_H, IMPRISMACES_H, IMPRISMAVARS_H en Asynchrone
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlSP"></param>
        /// <param name="pSqlCE"></param>
        /// <param name="pSqlVAR"></param>
        /// <returns></returns>
        // PM 20180208 [CHEETAH] New
        private static async System.Threading.Tasks.Task ExecuteQueryTheoriticalPriceFileAsync(string pCS, StringBuilder pSqlSP, StringBuilder pSqlCE, StringBuilder pSqlVAR)
        {
            if (((null != pSqlSP) && pSqlSP.Length > 0) || ((null != pSqlCE) && pSqlCE.Length > 0) || ((null != pSqlVAR) && pSqlVAR.Length > 0))
            {
                try
                {
                    m_PrismaSemaphore.Wait();
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        ExecuteQueryTheoriticalPriceFile(pCS, pSqlSP, pSqlCE, pSqlVAR);
                    });
                }
                catch { throw; }
                finally
                {
                    m_PrismaSemaphore.Release();
                }
            }
        }

        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMAMKTCAPA_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sqlMC"></param>
        private static void ExecuteQueryMarketCapacities(string pCS, StringBuilder sql)
        {
            if ((null != sql) && sql.Length > 0)
            {
                sql.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMAMKTCAPA_H(IDIMPRISMA_H, PRODUCTLINE, PRODUCTID, ISINCODEUNL, PUTCALL, TTEBUCKETID, MONEYNESSBUCKETID, MARKETCAPACITY, LIQUIDITYPREMIUM)");
                sql.Append(";");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sql.ToString());
            }
        }

        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMALIQFACT_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sql"></param>
        private static void ExecuteQueryLiquidityFactors(string pCS, StringBuilder sql)
        {
            if ((null != sql) && sql.Length > 0)
            {
                sql.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMALIQFACT_H(IDIMPRISMALIQCLASS_H, PCTMINTHRESHOLD, PCTMAXTHRESHOLD, MINTHRESHOLDFACTOR, MAXTHRESHOLDFACTOR)" + Cst.CrLf);
                sql.Append(";");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sql.ToString());
            }
        }
        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMAFXRMS_H
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="sql"></param>
        private static void ExecuteQueryFxRate(string pCS, StringBuilder sql)
        {
            if ((null != sql) && sql.Length > 0)
            {
                sql.Insert(0, SQLCst.INSERT_INTO_DBO + "IMPRISMAFXRMS_H(IDIMPRISMAFXPAIR_H, IDIMPRISMARMS_H, SCENARIONO, VALUE)");
                sql.Append(";");
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, sql.ToString());
            }
        }

        /// <summary>
        ///  Insertion des données dans table SQL IMPRISMAFXRMS_H en Asynchrone
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSql"></param>
        // PM 20180208 [CHEETAH] New
        private static async System.Threading.Tasks.Task ExecuteQueryFxRateAsync(string pCS, StringBuilder pSql)
        {
            if ((null != pSql) && pSql.Length > 0)
            {
                try
                {
                    m_PrismaSemaphore.Wait();
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        ExecuteQueryFxRate(pCS, pSql);
                    });
                }
                catch { throw; }
                finally
                {
                    m_PrismaSemaphore.Release();
                }
            }
        }

        /// <summary>
        ///  Retourne les assets présents dans la table IMPRISMAS_H
        ///  <para>Pour rapperl cette table est alimentée uniquement par l'importation du fichier Theoretical prices and instrument configuration</para>
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <returns></returns>
        /// FI 20140617 [19911] add method 
        // EG 20180426 Analyse du code Correction [CA2202]
        private List<int> LoadSpheresAssetFromPrismaSerie()
        {
            List<int> ret = new List<int>(); ;

            string query = @"select IDASSET from IMPRISMAS_H s
inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
inner join dbo.IMPRISMA_H main on (main.IDIMPRISMA_H = p.IDIMPRISMA_H) and (main.IDIMPRISMA_H = @IDIMPRISMA_H)";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(Cs, "IDIMPRISMA_H", DbType.Int32), idIMPRISMA_H);

            QueryParameters qry = new QueryParameters(Cs, query, dp);
            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    ret.Add(Convert.ToInt32(dr[0]));
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les PRODUCTID présents dans la table IMPRISMA_P
        /// </summary>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private List<string> LoadProductIdfromPrismaProduct()
        {
            List<string> ret = new List<string>(); ;

            string query = @"select PRODUCTID 
from dbo.IMPRISMAP_H p
inner join dbo.IMPRISMA_H main on (main.IDIMPRISMA_H = p.IDIMPRISMA_H) and (main.IDIMPRISMA_H = @IDIMPRISMA_H)";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(Cs, "IDIMPRISMA_H", DbType.Int32), idIMPRISMA_H);

            QueryParameters qry = new QueryParameters(Cs, query, dp);
            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    ret.Add(Convert.ToString(dr[0]));
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idAsset"></param>
        /// <param name="idExpiry"></param>
        /// <param name="pvPrice"></param>
        /// <param name="stlPrice"></param>
        /// <param name="unlPriceOffset"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryUpdPriceOfSeries(string cs, int idAsset, int idExpiry, Decimal pvPrice, Decimal stlPrice, Nullable<Decimal> unlPriceOffset)
        {
            string sql = @"update dbo.IMPRISMAS_H 
set PVPRICE = @PVPRICE, STLPRICE = @STLPRICE, UNLPRICEOFFSET = @UNLPRICEOFFSET
where (IDIMPRISMAE_H=@IDEXPIRY) and (IDASSET=@IDASSET);";
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(cs, "PVPRICE", DbType.Decimal), pvPrice);
            dp.Add(new DataParameter(cs, "STLPRICE", DbType.Decimal), stlPrice);
            dp.Add(new DataParameter(cs, "UNLPRICEOFFSET", DbType.Decimal), unlPriceOffset ?? Convert.DBNull);
            dp.Add(new DataParameter(cs, "IDEXPIRY", DbType.Int32), idExpiry);
            dp.Add(new DataParameter(cs, "IDASSET", DbType.Int32), idAsset);
            QueryParameters qry = new QueryParameters(cs, sql, dp);

            return qry;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour du CONTRACTMULTIPLIER de la table ASSET_ETD
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        /// PM 20160215 [21491] Déplacement de la méthode dans la classe de base
        //        private QueryParameters GetQueryUpdASSET_ETD(int pIdAsset, decimal pContractMultiplier)
        //        {
        //            string sql = @"update dbo.ASSET_ETD 
        //                              set CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER,
        //                                  IDAUPD = @IDAUPD,
        //                                  DTUPD = @DTUPD
        //                              where (IDASSET=@IDASSET)
        //                                and ((CONTRACTMULTIPLIER is null) or (CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
        //                                and exists (select 1 from dbo.DERIVATIVECONTRACT dc
        //                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
        //                                             where (dc.ISAUTOSETTING = 1)
        //                                               and (da.IDDERIVATIVEATTRIB = ASSET_ETD.IDDERIVATIVEATTRIB))";

        //            DataParameters dp = new DataParameters();
        //            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER), pContractMultiplier);
        //            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDAUPD), task.UserId);
        //            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTUPD), dtStart);
        //            dp.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);
        //            QueryParameters qry = new QueryParameters(cs, sql, dp);

        //            return qry;
        //        }


        /// <summary>
        ///  Importation du fichier settlement prices
        ///  <para>alimentation de la table IMPRISMAS_H et alimentation des tables de cotations</para>
        ///  <para>cette importation doit se dérouler nécessairement après l'importation du fichier Theoretical prices and instrument</para>
        /// </summary>
        /// <param name="pInputParsing">Parsings de l'importation</param>
        /// <param name="opNbParsingIgnoredLines">Nb de ligne ignorées par le parsing</param>
        /// <returns></returns>
        /// 
        public int InputSettlementPriceFile(InputParsing pInputParsing, ref int opNbParsingIgnoredLines)
        {
            try
            {
                StartPrismaImport(pInputParsing);
                PrismaInputFileConfig config = InitSettlementPriceFileConfig();
                //L'élément "EUREX - RISKDATA - TheoInst OI-serie" pourrait éventuellement ne pas exister si un jour Spheres® est utilisé uniquement pour récupérer les cotations
                //Si l'élément existe, Spheres® considère qu'il est nécessairemenent exécuté avant l'importation du fichier settlement prices
                // PM 20170524 [22834][23078] Recherche uniquement de "RISKDATA - TheoInst OI-serie"
                //Boolean isElementTheoInstExecuted = task.ExistElement("EUREX - RISKDATA - TheoInst OI-serie");
                Boolean isElementTheoInstExecuted = task.ExistElementContains("RISKDATA - TheoInst OI-serie");

                if (false == IsImportPriceOnly)
                {
                    // l'élément d'importation du fichier TheoInst doit déjà être exécutée (puisque cette fonction effectue des update de la table IMPRISMAS_H)
                    if (false == isElementTheoInstExecuted)
                    {
                        // PM 20170524 [22834][23078] Modif du message
                        //throw new InvalidOperationException(StrFunc.AppendFormat("the execution of element(id:{0])is required", "EUREX - RISKDATA - TheoInst OI-serie"));
                        throw new InvalidOperationException("The execution of element: \"* - RISKDATA - TheoInst OI-serie\" is required");
                    }
                }

                // Récupération de la liste des assets importés via l'importation du fichier Theoretical prices and instrument (TheoInst OI-serie)
                List<int> lstIdAsset = new List<int>();
                List<String> lstProductId = new List<String>();
                // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                //if (isElementTheoInstExecuted && (false == isImportPriceOnly))
                //{
                //    lstIdAsset = LoadSpheresAssetFromPrismaSerie();
                //    lstProductId = LoadProductIdfromPrismaProduct();
                //}
                if (IsImportPriceOnly)
                {
                    _assetETDToImport.LoadAssetByMarket(Cs, m_dtFile);
                    lstProductId = _assetETDToImport.CtrSym.ToList();
                    lstIdAsset = _assetETDToImport.AllAsset.Select(x => x.IdAsset).ToList();
                }
                else if (isElementTheoInstExecuted)
                {
                    lstIdAsset = LoadSpheresAssetFromPrismaSerie();
                    lstProductId = LoadProductIdfromPrismaProduct();
                }

                Boolean isProductOk = false;
                Boolean isExpiryOk = false;
                Boolean isProductFlexOk = false; // PM 20151214 [21643] Add isProductFlexOk
                ArrayList alProductOk = new ArrayList();
                Boolean isProductIDInTrade = false;

                StringBuilder sqlUpd = new StringBuilder();

                IOTaskDetInOutFileRow parsingRowProduct = null;
                IOTaskDetInOutFileRow parsingRowExpiry = null;

                // PM 20151124 [20124] Initialise les membres m_IdMarketEnv & m_IdValScenario
                InitDefaultMembers();

                // Lecture de l'IDM du marché pour l'import des cotations
                // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
                //int idm = GetIdmFromExchangeAcronym(EUREX_EXCHANGEACRONYM);
                int idm = GetIdmFromExchangeAcronym(ExchangeAcronym);
                string currency = string.Empty;
                decimal productTickSize = 0;
                decimal productTickValue = 0;
                string productID = null;
                List<Pair<string, int>> updatedDC = new List<Pair<string, int>>();
                bool isNewExpiry = false;

                //FI 20181126 [24308] Alimentation du paramètre pQuoteSide
                QueryParameters qryParamQuoteETD = GetQueryParameters_QUOTE_XXX_H(Cs, Cst.OTCml_TBL.QUOTE_ETD_H, m_IdMarketEnv, m_IdValScenario, m_dtFile, QuotationSideEnum.OfficialClose);

                // PM 20151209 [21653] Lecture des DC sans Contract Multiplier
                QueryParameters queryDCMutliplier = GetQuerySelectDERIVATIVECONTRACT(task.Cs, idm, m_dtFile);
                DataTable dtDC = DataHelper.ExecuteDataTable(task.Cs, queryDCMutliplier.Query.ToString(), queryDCMutliplier.Parameters.GetArrayDbParameter());
                List<DataRow> dataRowDCWithoutCM = dtDC.AsEnumerable().ToList(); // 
                List<Pair<string, string>> dcWithoutCM = dataRowDCWithoutCM.Select(dr => new Pair<string, string>(dr.Field<string>("CONTRACTSYMBOL"), dr.Field<string>("CONTRACTTYPE"))).ToList();
                bool isDCToUpdated = false;
                bool isDCFlexToUpdated = false;
                bool isCheckForFlex = dcWithoutCM.Any(dc => dc.Second == DerivativeContractTypeEnum.FLEX.ToString()); // Existe-t-il des Flex sans Contract Multiplier

                bool isUnlPriceForIndexOptionDone = false;
                MarketAssetETDToImport assetETDForIndexOption = default;

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Add values ...", 3));

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
                            if ((null != sqlUpd) && sqlUpd.Length > 0)
                            {
                                DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlUpd.ToString());
                                sqlUpd = new StringBuilder();
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
                                if ((null != sqlUpd) && sqlUpd.Length > 0)
                                {
                                    DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlUpd.ToString());
                                    sqlUpd = new StringBuilder();
                                }
                                parsingRowProduct = null;

                                LoadLine(currentLine, ref parsingRowProduct);

                                // PM 20151124 [20124] Devise pour l'import des cotations
                                currency = parsingRowProduct.GetRowDataValue(Cs, "Currency");
                                // PM 20151124 [20124] Tick Size et Tick Value pour l'import du Contract Multiplier
                                productTickSize = DecFunc.DecValueFromInvariantCulture(parsingRowProduct.GetRowDataValue(Cs, "Tick Size"));
                                productTickValue = DecFunc.DecValueFromInvariantCulture(parsingRowProduct.GetRowDataValue(Cs, "Tick Value"));

                                // PM 20151124 [20124] Garder le productID pour l'utiliser au niveau de la série
                                //string productID = parsingRowProduct.GetRowDataValue(Cs, "PRODUCT ID");
                                productID = parsingRowProduct.GetRowDataValue(Cs, "PRODUCT ID");

                                isProductOk = IsProductToImport(config, productID, alProductOk);
                                if (isProductOk)
                                {
                                    alProductOk.Add(productID);
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for product:{0}", productID), 3));

                                    // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                                    //if (isImportPriceOnly)
                                    //{
                                    //    isProductIDInTrade = IsExistDcInTrade(productID);
                                    //}
                                    //else
                                    //{
                                    //    isProductOk = lstProductId.Contains(productID);
                                    //}
                                    isProductOk = lstProductId.Contains(productID);
                                }
                                // PM 20151209 [21653] Si le DC ne doit pas est importé, si son CM est absent alors le mettre à jour quand même
                                isDCToUpdated = false;
                                isDCFlexToUpdated = false;
                                if ((isProductOk == false) && (IsImportPriceOnly == false))
                                {
                                    isDCToUpdated = dcWithoutCM.Any(dc => dc.First == productID);
                                }
                                break;
                            #endregion Product

                            #region Expiry
                            case "E"://Expiry
                                parsingRowExpiry = null;
                                // RD 20150404 [23043] Pour gérer le cas des Maturity avec date d'échéance à la date du fichier (ligne inéxistante dans le fichier Theo)
                                isExpiryOk = false;
                                isUnlPriceForIndexOptionDone = false;
                                assetETDForIndexOption = default;
                                //
                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                //if (isProductOk)
                                if (isProductOk || IsImportPriceOnly)
                                {
                                    // RD 20150404 [23043] Ici, on est dans le cas où le DC avec des échéances dans le fichier Theo
                                    isExpiryOk = true;
                                    LoadLine(currentLine, ref parsingRowExpiry);
                                    // PM 20151124 [20124] Nouvelle maturity dont importer le prix du sous-jacent
                                    isNewExpiry = true;
                                }
                                else
                                {
                                    // RD 20150404 [23043] Ici, on est dans le cas où le DC sans échéances dans le fichier Theo, il faut donc vérifier si Maturity avec date d'échéance à la date du fichier
                                    bool tmpIsProductOk = IsProductToImport(config, productID, alProductOk);
                                    IOTaskDetInOutFileRow tmpParsingRowExpiry = null;

                                    if (tmpIsProductOk)
                                    {
                                        LoadLine(currentLine, ref tmpParsingRowExpiry);

                                        string year = tmpParsingRowExpiry.GetRowDataValue(Cs, "Expiration Year");
                                        string month = tmpParsingRowExpiry.GetRowDataValue(Cs, "Expiration Month");
                                        string day = tmpParsingRowExpiry.GetRowDataValue(Cs, "Expiration Day");

                                        year = "20" + (StrFunc.IsEmpty(year) ? "00" : year);
                                        month = StrFunc.IsEmpty(month) ? "00" : month.PadLeft(2, '0');
                                        day = StrFunc.IsEmpty(day) ? "00" : day.PadLeft(2, '0');

                                        string maturityDate = year + month + day;
                                        DateTime dtMaturityDate = new DtFunc().StringyyyyMMddToDateTime(maturityDate);
                                        // Le jour de l'échéance
                                        tmpIsProductOk = (dtMaturityDate.CompareTo(m_dtFile) == 0);
                                    }

                                    if (tmpIsProductOk)
                                    {
                                        isExpiryOk = true;
                                        parsingRowExpiry = tmpParsingRowExpiry;
                                        isNewExpiry = true;
                                    }
                                }
                                break;
                            #endregion Expiry

                            #region Serie
                            case "S"://Serie
                                IOTaskDetInOutFileRow parsingRowSerie = null;

                                // PM 20150804 [21236] Pb d'un trade Flex sans trade sur le contrat standard
                                // => Vérifier le Product Flex
                                // PM 20151124 [20124] Toujours lire ligne serie du fichier et si contrat flexible lorsque isImportPriceOnly || isProductOk
                                //if (isImportPriceOnly)
                                Boolean isFlexible = false;
                                string flexProductID = null;
                                // PM 20151209 [21653] Ajout lecture de la série pour contract flex (ou standard) sans CM
                                //if (isImportPriceOnly || isProductOk)
                                // RD 20150404 [23043] Pour gérer le cas des Maturity avec date d'échéance à la date du fichier (ligne inéxistante dans le fichier Theo)
                                //if (isImportPriceOnly || isProductOk || isDCToUpdated || isCheckForFlex)
                                if (IsImportPriceOnly || isProductOk || isDCToUpdated || isCheckForFlex || isExpiryOk)
                                {
                                    LoadLine(currentLine, ref parsingRowSerie);
                                    isFlexible = BoolFunc.IsTrue(parsingRowSerie.GetRowDataValue(Cs, "Flex Series Flag"));
                                    if (IsImportPriceOnly)
                                    {
                                        if (isFlexible)
                                        {
                                            flexProductID = parsingRowSerie.GetRowDataValue(Cs, "Flex Product ID");
                                            isProductFlexOk = IsProductToImport(config, flexProductID, alProductOk);
                                            if (isProductFlexOk)
                                            {
                                                if (config.IsSearchProduct)
                                                {
                                                    // Vérifier qu'il existe des trade sur le contrat Flex 
                                                    // PM 20180219 [23824] Modification de la gestion de isImportPriceOnly
                                                    //isProductFlexOk = IsExistDcInTrade(flexProductID);
                                                    isProductFlexOk = lstProductId.Contains(flexProductID);
                                                }
                                                if (isProductFlexOk)
                                                {
                                                    alProductOk.Add(flexProductID);

                                                    
                                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Add parameters values for flexible product:{0}", flexProductID), 3));
                                                }
                                            }
                                            // PM 20151209 [21653] Lorsque le contract ne doit pas être importé, vérification si le contract flex est sans CM pour le mettre à jour
                                            if (isProductFlexOk == false)
                                            {
                                                isDCFlexToUpdated = dcWithoutCM.Any(dc => (dc.First == flexProductID) && (dc.Second == DerivativeContractTypeEnum.FLEX.ToString()));
                                            }
                                        }
                                        // PM 20151124 [20124] Refactoring du test plus bas
                                        else
                                        {
                                            // pour une série sur un produit standard, Spheres® vérifie que la série a été négociée uniquement si le contrat a déjà été négocié
                                            isProductOk = isProductIDInTrade;
                                        }
                                    }
                                }

                                // RD 20150404 [23043] Pour gérer le cas des Maturity avec date d'échéance à la date du fichier (ligne inéxistante dans le fichier Theo)
                                //if (isProductOk || isProductFlexOk)
                                if (isProductOk || isProductFlexOk || isExpiryOk)
                                {
                                    MarketAssetETDRequest assetRequest = GetAssetRequestContractMonthYear(Cs, parsingRowProduct, parsingRowExpiry, parsingRowSerie);

                                    Boolean isAssetFound = IsSerieToImport(config, IsImportPriceOnly, assetRequest, out MarketAssetETDToImport assetETDFound);
                                    int idAsset = assetETDFound.IdAsset;

                                    string stlPriceValue = parsingRowSerie.GetRowDataValue(Cs, "Settlement Price");

                                    // Vérifier si l'on doit importer le cours de l'indice d'une option sur indice
                                    if (StrFunc.IsFilled(stlPriceValue) &&
                                        (assetRequest.ContractCategory == "O") && (assetRequest.SettlementMethod == SettlMethodEnum.CashSettlement)
                                        && (false == isUnlPriceForIndexOptionDone) && (assetETDForIndexOption == default(MarketAssetETDToImport)))
                                    {
                                        if ((idAsset != 0) && (assetETDFound.UnderlyingAssetCategory == Cst.UnderlyingAsset.Index.ToString()))
                                            assetETDForIndexOption = assetETDFound;
                                        else
                                            // Lorsque l'asset n'est pas trouvé, recherche d'un asset négocié Option sur Indice en CashSettlement sur le même Contrat et même Echeance 
                                            assetETDForIndexOption = GetAssetETDForIndexOption(assetRequest);

                                        if ((assetETDForIndexOption != default(MarketAssetETDToImport)) && (assetETDForIndexOption.IdAssetUnl > 0))
                                        {
                                            Boolean isExpiryDate = (assetETDForIndexOption.MaturityDate == m_dtFile);

                                            // PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
                                            if (isExpiryDate)
                                            {
                                                decimal stlPrice = DecFunc.DecValueFromInvariantCulture(stlPriceValue);
                                                //WARNING: De nombreux prix sont dans le fichier avec une valeur "0.01" ou vide. 
                                                //         Ces prix, a priori non significatif, son ignoré.
                                                if (stlPrice * 100 > 1)
                                                {
                                                    decimal unlStlPrice;
                                                    // usage de assetRequest puisque l'asset en cours n'est pas forcément un asset présent dans Spheres
                                                    if (assetRequest.PutCall == PutOrCallEnum.Put)
                                                        unlStlPrice = assetRequest.StrikePrice - stlPrice;
                                                    else
                                                        unlStlPrice = assetRequest.StrikePrice + stlPrice;

                                                    // Mise à jour du cours OfficialSettlement des Options sur Indice
                                                    Cst.OTCml_TBL quoteTable = AssetTools.ConvertQuoteEnumToQuoteTbl(QuoteEnum.INDEX);
                                                    QueryParameters qryParamQuote = GetQueryParameters_QUOTE_XXX_H(Cs, quoteTable, m_IdMarketEnv, m_IdValScenario, m_dtFile, QuotationSideEnum.OfficialSettlement);
                                                    InsertUpdate_QUOTE_XXX_H(qryParamQuote, assetETDForIndexOption.IdAssetUnl, 0, m_dtFile, QuotationSideEnum.OfficialSettlement, unlStlPrice, currency);
                                                    isUnlPriceForIndexOptionDone = true;
                                                }
                                            }
                                        }
                                    }

                                    if (StrFunc.IsFilled(stlPriceValue) && isAssetFound)
                                    {
                                        // PM 20181001 [XXXXX] Import file STLCF : vérifier que les cours est présent
                                        //decimal stlPrice = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(Cs, "Settlement Price"));

                                        decimal stlPrice = DecFunc.DecValueFromInvariantCulture(stlPriceValue);

                                        // PM 20151124 [20124] Series Version Number et Trading Unit pour mise à jour du Contract Multiplier
                                        int serieVerionNumber = IntFunc.IntValue(parsingRowSerie.GetRowDataValue(Cs, "Series Version Number"));
                                        decimal tradingUnit = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(Cs, "Trading Unit"));

                                        // RD 20150404 / FI 20150404 si (pConfig.isSearchSerie = false) alors Idasset est à 0
                                        if (idAsset == 0)
                                            throw new NotImplementedException(StrFunc.AppendFormat("Record type {0}, asset not found", Record_Type));

                                        if (false == IsImportPriceOnly)
                                        {
                                            if (lstIdAsset.Contains(idAsset))
                                            {
                                                DataRow dataRowProduct = this.LoadRowProduct(parsingRowProduct);
                                                int idProduct = Convert.ToInt32(dataRowProduct["IDIMPRISMAP_H"]);
                                                string productId = dataRowProduct["PRODUCTID"].ToString();

                                                DataRow rowExpiry = this.LoadRowProductExpiration(idProduct, productId, parsingRowExpiry);
                                                int idExpiry = Convert.ToInt32(rowExpiry["IDIMPRISMAE_H"]);

                                                decimal pvPrice = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(Cs, "PV Reference Price"));
                                                Nullable<decimal> unlPriceOffset = null;
                                                string sUnlPriceOffset = parsingRowSerie.GetRowDataValue(Cs, "Underlying price offset");
                                                if (StrFunc.IsFilled(sUnlPriceOffset))
                                                {
                                                    unlPriceOffset = DecFunc.DecValueFromInvariantCulture(sUnlPriceOffset);
                                                }

                                                QueryParameters qryUpdatePrice = GetQueryUpdPriceOfSeries(Cs, idAsset, idExpiry, pvPrice, stlPrice, unlPriceOffset);

                                                if (sqlUpd.Length > 0)
                                                {
                                                    sqlUpd.Append(Cst.CrLf);
                                                }
                                                sqlUpd.Append(qryUpdatePrice.GetQueryReplaceParameters(false));
                                                if (sqlUpd.Length >= config.SqlLimitSize)
                                                {
                                                    DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlUpd.ToString());
                                                    sqlUpd = new StringBuilder();
                                                }
                                            }
                                            else
                                            {
                                                
                                                Logger.Log(new LoggerData(LogLevelEnum.Debug, string.Format("Asset (identifier:{0})(id:{1}) does not exist in file TheoInst", assetETDFound.AssetIdentifier, idAsset), 3));
                                            }
                                        }

                                        // PM 20151216 [21662] Ne rien faire quand il s'agit de l'importation Eurosys
                                        if (false == IsEurosysSoftware)
                                        {
                                            // PM 20161019 [22174] Prisma 5.0 : Ajout du Delta
                                            //// PM 20151124 [20124] Insertion/Mise à jour des cotations
                                            //InsertUpdate_QUOTE_XXX_H(qryParamQuoteETD, idAsset, idm, m_dtFile, QuotationSideEnum.OfficialClose, stlPrice, currency);
                                            string sDelta = parsingRowSerie.GetRowDataValue(Cs, "Delta");
                                            if (StrFunc.IsFilled(sDelta))
                                            {
                                                decimal delta = DecFunc.DecValueFromInvariantCulture(sDelta);
                                                InsertUpdate_QUOTE_ETD_H(qryParamQuoteETD, idAsset, idm, m_dtFile, QuotationSideEnum.OfficialClose, stlPrice, currency, delta);
                                            }
                                            else
                                            {
                                                // PM 20151124 [20124] Insertion/Mise à jour des cotations
                                                InsertUpdate_QUOTE_XXX_H(qryParamQuoteETD, idAsset, idm, m_dtFile, QuotationSideEnum.OfficialClose, stlPrice, currency);
                                            }

                                            // PM 20151124 [20124] Insertion/Mise à jour des cotations des sous-jacents
                                            if (isNewExpiry)
                                            {
                                                // PM 20161019 [22174] Prisma 5.0 : "Underlying close Price" peu ne pas être renseigné
                                                //decimal unlClosePrice = DecFunc.DecValueFromInvariantCulture(parsingRowExpiry.GetRowDataValue(Cs, "Underlying close Price"));
                                                string sUnlClosePrice = parsingRowExpiry.GetRowDataValue(Cs, "Underlying close Price");
                                                if (StrFunc.IsFilled(sUnlClosePrice))
                                                {
                                                    decimal unlClosePrice = DecFunc.DecValueFromInvariantCulture(sUnlClosePrice);
                                                    InsertUpdateUnderliyngPrice(assetETDFound, unlClosePrice, currency);
                                                }
                                                isNewExpiry = false;
                                            }

                                            // PM 20151124 [20124] Mise à jour du Contract Mutliplier
                                            if ((productTickSize != 0) && (productTickValue != 0) && (tradingUnit != 0))
                                            {
                                                string contractSymbol = isFlexible ? flexProductID : productID;
                                                bool isContractUpdated = updatedDC.Exists(p => p.First == contractSymbol && p.Second == serieVerionNumber);
                                                //
                                                if ((isContractUpdated == false) || (serieVerionNumber > 0))
                                                {
                                                    decimal contractMultiplier = productTickValue / productTickSize * tradingUnit;
                                                    int nbrowsContract = 0;
                                                    int nbrowsAsset = 0;
                                                    Quote_ETDAsset quoteETD = null;

                                                    // Mise à jour du Contract Mutliplier sur le DC
                                                    if (isContractUpdated == false)
                                                    {
                                                        QueryParameters qryUpdateDC;
                                                        if (serieVerionNumber > 0)
                                                        {
                                                            qryUpdateDC = GetQueryUpdDERIVATIVECONTRACTLight(idAsset, productTickSize);
                                                        }
                                                        else
                                                        {
                                                            qryUpdateDC = GetQueryUpdDERIVATIVECONTRACT(idAsset, contractMultiplier, productTickValue * tradingUnit, productTickSize);
                                                        }
                                                        nbrowsContract = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateDC.Query, qryUpdateDC.Parameters.GetArrayDbParameter());
                                                        updatedDC.Add(new Pair<string, int>(contractSymbol, serieVerionNumber));
                                                    }

                                                    // Mise à jour du Contract Mutliplier sur l'asset
                                                    if (serieVerionNumber > 0)
                                                    {
                                                        // PM 20160215 [21491] Déplacement de la méthode dans la classe de base
                                                        //QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETD(idAsset, contractMultiplier);
                                                        QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETDMultiplier(Cs, idAsset, contractMultiplier, task.Process.UserId, dtStart);
                                                        nbrowsAsset = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateAsset.Query, qryUpdateAsset.Parameters.GetArrayDbParameter());
                                                    }

                                                    // Test si le CM du DC a été modifié
                                                    if ((nbrowsContract > 0) && (serieVerionNumber == 0))
                                                    {
                                                        // Quote Handling pour le contract
                                                        quoteETD = new Quote_ETDAsset
                                                        {
                                                            QuoteTable = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(),
                                                            idDC = assetETDFound.IdDC,
                                                            idDCSpecified = true
                                                        };
                                                    }
                                                    // Test si l'asset a été modifié
                                                    else if (nbrowsAsset > 0)
                                                    {
                                                        // Quote Handling pour l'asset
                                                        quoteETD = new Quote_ETDAsset
                                                        {
                                                            QuoteTable = Cst.OTCml_TBL.ASSET_ETD.ToString(),
                                                            idAsset = idAsset
                                                        };
                                                    }
                                                    // Test si besoin d'envoyer à QuotationHandling
                                                    if (quoteETD != null)
                                                    {
                                                        quoteETD.action = DataRowState.Modified.ToString();
                                                        quoteETD.contractMultiplier = contractMultiplier;
                                                        quoteETD.contractMultiplierSpecified = true;
                                                        quoteETD.timeSpecified = true; //FI 20170306 [22225] add
                                                        quoteETD.time = m_dtFile;
                                                        quoteETD.isCashFlowsVal = true;
                                                        quoteETD.isCashFlowsValSpecified = true;
                                                        //
                                                        MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = Cs };
                                                        QuotationHandlingMQueue qHMQueue = new QuotationHandlingMQueue(quoteETD, mQueueAttributes);
                                                        //
                                                        IOTools.SendMQueue(task, qHMQueue, Cst.ProcessTypeEnum.QUOTHANDLING);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (isDCToUpdated || isDCFlexToUpdated) // PM 20151209 [21653] Mise à jour des DC sans CM
                                {
                                    int serieVerionNumber = IntFunc.IntValue(parsingRowSerie.GetRowDataValue(Cs, "Series Version Number"));
                                    if (serieVerionNumber == 0)
                                    {
                                        decimal tradingUnit = DecFunc.DecValueFromInvariantCulture(parsingRowSerie.GetRowDataValue(Cs, "Trading Unit"));
                                        if ((productTickSize != 0) && (productTickValue != 0) && (tradingUnit != 0))
                                        {
                                            string contractSymbol = isFlexible ? flexProductID : productID;
                                            bool isContractUpdated = updatedDC.Exists(p => p.First == contractSymbol && p.Second == serieVerionNumber);
                                            if (isContractUpdated == false)
                                            {
                                                DataRow dcToUptade = dataRowDCWithoutCM.FirstOrDefault(dr => dr.Field<string>("CONTRACTSYMBOL") == contractSymbol);
                                                if (dcToUptade != default(DataRow))
                                                {
                                                    decimal contractMultiplier = productTickValue / productTickSize * tradingUnit;
                                                    dcToUptade["CONTRACTMULTIPLIER"] = contractMultiplier;
                                                    dcToUptade["MINPRICEINCRAMOUNT"] = productTickValue * tradingUnit;
                                                    dcToUptade["MINPRICEINCR"] = productTickSize;
                                                    dcToUptade["IDAUPD"] = task.Process.UserId;
                                                    dcToUptade["DTUPD"] = dtStart;
                                                    updatedDC.Add(new Pair<string, int>(contractSymbol, serieVerionNumber));
                                                }
                                            }
                                            if (isFlexible)
                                            {
                                                isDCFlexToUpdated = false;
                                            }
                                            else
                                            {
                                                isDCToUpdated = false;
                                            }
                                        }
                                    }
                                }
                                break;
                            #endregion Serie

                            case "*EOF*":
                                if ((null != sqlUpd) && sqlUpd.Length > 0)
                                {
                                    DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlUpd.ToString());
                                    sqlUpd = new StringBuilder();
                                }
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Record type:{0} is not implemented", Record_Type));
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string msg = StrFunc.AppendFormat("An error occured on row:{0}", lineNumber);
                        throw new Exception(msg, ex);
                    }
                }
                // PM 20151209 [21653] Mise à jour des DC sans Contract Multiplier
                DataHelper.ExecuteDataAdapter(Cs, queryDCMutliplier.GetQueryReplaceParameters(), dtDC);

                opNbParsingIgnoredLines = nbParsingIgnoredLines;

                return lineNumber - 1;
            }
            catch { throw; }
            finally
            {
                EndPrismaImport();
            }
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
        /// 
        /// </summary>
        /// <param name="idAsset"></param>
        /// <returns></returns>
        protected MarketAssetETDToImport GetEurexAssetETDToImport(int idAsset)
        {

            SQL_AssetETD sqlAsset = new SQL_AssetETD(Cs, idAsset);
            sqlAsset.LoadTable(new string[] { "IDASSET", $"{sqlAsset.SQLObject}.IDENTIFIER", "IDDC",$"{sqlAsset.SQLObject}.IDM",
                "CONTRACTSYMBOL","CONTRACTATTRIBUTE", "CATEGORY","CONTRACTTYPE",
                "SETTLTMETHOD","EXERCISESTYLE", "STRIKEPRICE","PUTCALL","MATURITYMONTHYEAR","MATURITYDATE","MATURITYDATESYS",
                "ASSETCATEGORY", "IDASSET_UNL", "DA_IDASSET" });

            MarketAssetETDToImport ret = new MarketAssetETDToImport
            {
                IdAsset = sqlAsset.IdAsset,
                AssetIdentifier = sqlAsset.Identifier,
                IdDC = sqlAsset.IdDerivativeContract,
                IdM = sqlAsset.IdM,
                ContractSymbol = sqlAsset.DrvContract_Symbol,
                ContractAttribute = sqlAsset.DrvContract_Attribute,
                ContractCategory = sqlAsset.DrvContract_Category,
                ContractType = sqlAsset.DrvContract_ContractType,
                SettlementMethod = sqlAsset.SettlementMethod,
                ExerciseStyle = sqlAsset.ExerciseStyle,
                StrikePrice = sqlAsset.StrikePrice,
                PutCall = sqlAsset.PutCallEnum,
                MaturityMonthYear = sqlAsset.Maturity_MaturityMonthYear,
                MaturityDate = sqlAsset.Maturity_MaturityDateSys,
                UnderlyingAssetCategory = sqlAsset.DrvContract_AssetCategorie,
                IdAssetUnl = sqlAsset.DrvContract_IdAssetUnl,
                IdAssetUnlFuture = sqlAsset.DrvAttrib_IdAssetUnl
            };

            return ret;
        }

        /// <summary>
        /// Retourne l'Id non significatif de la serie dans Spheres®
        /// <para>Retourne 0 si la serie n'existe pas ds Spheres® ou si la série n'a jamais été négociée</para>
        /// </summary>
        /// <param name="pRequest">Element de recherche d'un asset</param>
        /// <param name="pAssetData">Retourne l'asset ETD de Spheres</param>
        /// <returns></returns>
        /// FI 20140618 [19911] lecture de l'élément Flex Product ID
        /// PM 20141022 [9700] Eurex Prisma for Eurosys Futures : La méthode devient "protected virtual"
        /// EG 20180426 Analyse du code Correction [CA2202]
        /// PM 20181211 [24389][24383] Gestion du cours OfficialSettlement sur les Options sur Indice
        protected virtual int GetIdAssetETD(MarketAssetETDRequest pRequest, out MarketAssetETDToImport pAssetData)
        {

            bool isUseDcContractMultiplier = false;

            QueryParameters queryParameters = null;
            DataParameters dp = null;
            if (pRequest.ContractCategory == "O")
            {
                queryParameters = QueryExistAssetOptionInTrades(Cs, m_MaturityType, _assetETDRequestSettings.IsWithSettlementMethod, _assetETDRequestSettings.IsWithExerciseStyle, isUseDcContractMultiplier, _assetETDRequestSettings.IsWithContractAttrib);
                dp = queryParameters.Parameters;

                if (_assetETDRequestSettings.IsWithExerciseStyle)
                    dp["EXERCISESTYLE"].Value = ReflectionTools.ConvertEnumToString<DerivativeExerciseStyleEnum>(pRequest.ExerciseStyle.Value);
                dp["PUTCALL"].Value = ReflectionTools.ConvertEnumToString<PutOrCallEnum>(pRequest.PutCall.Value);
                dp["STRIKEPRICE"].Value = pRequest.StrikePrice;
            }
            else if (pRequest.ContractCategory == "F")
            {
                queryParameters = QueryExistAssetFutureInTrades_or_UnderlyingOptionInTrades(Cs, m_MaturityType, _assetETDRequestSettings.IsWithSettlementMethod, isUseDcContractMultiplier, _assetETDRequestSettings.IsWithContractAttrib);
                dp = queryParameters.Parameters;
            }
            dp["DTFILE"].Value = m_dtFile;
            // PM 20170531 [22834] Remplacement de EUREX_EXCHANGEACRONYM par ExchangeAcronym
            //dp["EXCHANGEACRONYM"].Value = EUREX_EXCHANGEACRONYM;
            dp["EXCHANGEACRONYM"].Value = ExchangeAcronym;
            dp["CONTRACTSYMBOL"].Value = pRequest.ContractSymbol;
            dp["CATEGORY"].Value = pRequest.ContractCategory;
            dp["MATURITYMONTHYEAR"].Value = pRequest.MaturityMonthYear;
            if (_assetETDRequestSettings.IsWithSettlementMethod)
                dp["SETTLTMETHOD"].Value = ReflectionTools.ConvertEnumToString<SettlMethodEnum>(pRequest.SettlementMethod);
            if (_assetETDRequestSettings.IsWithContractAttrib)
                dp["CONTRACTATTRIBUTE"].Value = pRequest.ContractAttribute;

            // PM 20180219 [23824] Utilisation de ExecuteReader à la place de ExecuteDataTable car pas besoin de cache (sinon OutOfMemory)
            //using (IDataReader dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(Cs), queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            int idAsset = 0;
            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    idAsset = IntFunc.IntValue(dr["IDASSET"].ToString());
            }

            pAssetData = new MarketAssetETDToImport();
            if (idAsset > 0)
                pAssetData = GetEurexAssetETDToImport(idAsset);

            return pAssetData.IdAsset;
        }

        /// <summary>
        /// Selection des DC (version 0 ) dont le CM est null en vue de sa mise à jour
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdM"></param>
        /// <param name="pDtFile"></param>
        /// <returns></returns>
        /// PM 20151209 [21653] New
        private static QueryParameters GetQuerySelectDERIVATIVECONTRACT(string pCS, int pIdM, DateTime pDtFile)
        {
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + "dc.IDDC, dc.CONTRACTSYMBOL, dc.CONTRACTATTRIBUTE, dc.CONTRACTTYPE, dc.CONTRACTMULTIPLIER, dc.MINPRICEINCRAMOUNT, dc.MINPRICEINCR, dc.IDAUPD, dc.DTUPD" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + Cst.CrLf;
            query += SQLCst.WHERE + "(ISAUTOSETTING = 1)" + Cst.CrLf;
            query += SQLCst.AND + "(dc.IDM = @IDM)" + Cst.CrLf;
            query += SQLCst.AND + "(dc.CONTRACTMULTIPLIER is null)" + Cst.CrLf;
            query += SQLCst.AND + "((dc.CONTRACTATTRIBUTE is null) or (dc.CONTRACTATTRIBUTE = '0'))" + Cst.CrLf;
            query += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "dc", "DTFILE") + ")" + Cst.CrLf;

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), pIdM);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTFILE), pDtFile);

            QueryParameters qryParameters = new QueryParameters(pCS, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour des CONTRACTMULTIPLIER, MINPRICEINCR et MINPRICEINCRAMOUNT de la table DERIVATIVECONTRACT
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pMinPriceAmount"></param>
        /// <param name="pMinPriceIncr"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        private QueryParameters GetQueryUpdDERIVATIVECONTRACT(int pIdAsset, decimal pContractMultiplier, decimal pMinPriceAmount, decimal pMinPriceIncr)
        {
            string sql = @"update dbo.DERIVATIVECONTRACT 
                              set CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER,
                                  MINPRICEINCRAMOUNT = @MINPRICEINCRAMOUNT,
                                  MINPRICEINCR = @MINPRICEINCR,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (ISAUTOSETTING = 1)
                                and (((CONTRACTMULTIPLIER is null) or (CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
                                  or ((MINPRICEINCR is null) or (MINPRICEINCR != @MINPRICEINCR))
                                  or ((MINPRICEINCRAMOUNT is null) or (MINPRICEINCRAMOUNT != @MINPRICEINCRAMOUNT)))
                                and exists (select 1 from dbo.ASSET_ETD a
                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                                             where (a.IDASSET=@IDASSET)
                                               and (da.IDDC = DERIVATIVECONTRACT.IDDC))";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.CONTRACTMULTIPLIER), pContractMultiplier);
            dp.Add(new DataParameter(Cs, "MINPRICEINCRAMOUNT", DbType.Decimal), pMinPriceAmount);
            dp.Add(new DataParameter(Cs, "MINPRICEINCR", DbType.Decimal), pMinPriceIncr);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAUPD), task.Process.UserId);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTUPD), dtStart);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);
            QueryParameters qry = new QueryParameters(Cs, sql, dp);

            return qry;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour du MINPRICEINCR de la table DERIVATIVECONTRACT
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pMinPriceIncr"></param>
        /// <returns></returns>
        /// PM 20151124 [20124] New
        private QueryParameters GetQueryUpdDERIVATIVECONTRACTLight(int pIdAsset, decimal pMinPriceIncr)
        {
            string sql = @"update dbo.DERIVATIVECONTRACT 
                              set MINPRICEINCR = @MINPRICEINCR,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (ISAUTOSETTING = 1)
                                and ((MINPRICEINCR is null) or (MINPRICEINCR != @MINPRICEINCR))
                                and exists (select 1 from dbo.ASSET_ETD a
                                             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                                             where (a.IDASSET=@IDASSET)
                                               and (da.IDDC = DERIVATIVECONTRACT.IDDC))";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(Cs, "MINPRICEINCR", DbType.Decimal), pMinPriceIncr);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDAUPD), task.Process.UserId);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTUPD), dtStart);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDASSET), pIdAsset);
            QueryParameters qry = new QueryParameters(Cs, sql, dp);

            return qry;
        }

        /// <summary>
        /// Initialisation à effectuer lors de l'importation des fichiers PRISMA
        /// <para>Démarrage de l'audit, Ouverture du fichier, Récupération de la date de fichier, Chargement de la table IMPRISMA_H</para>
        /// </summary>
        /// <param name="pInputParsing"></param>
        protected override void StartPrismaImport(InputParsing pInputParsing)
        {
            base.StartPrismaImport(pInputParsing);
            LoadRowIMPRISMA_H();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void EndPrismaImport()
        {
            base.EndPrismaImport();
            DataHelper.queryCache.Remove("IMPRISMA_H", task.Cs, true);
        }


        /// <summary>
        /// Mise à jour ou insertion d'une cotation
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// PM 20151124 [20124] New
        private void InsertUpdateUnderliyngPrice(MarketAssetETDToImport pAsset, decimal pPrice, string pCurrency)
        {
            // PM 20161019 [22174] Prisma 5.0 : Ajout vérification que pSqlAsset.DrvContract_AssetCategorie est renseigné
            //if ((pSqlAsset != null) && (pSqlAsset.IsLoaded))
            Cst.UnderlyingAsset underlyingAssetCategory = Cst.ConvertToUnderlyingAsset(pAsset.UnderlyingAssetCategory);
            int idAssetUnl = (underlyingAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) ? pAsset.IdAssetUnlFuture : pAsset.IdAssetUnl;
            if (idAssetUnl > 0)
            {
                QuoteEnum quoteCategory = AssetTools.ConvertUnderlyingAssetToQuoteEnum(underlyingAssetCategory);
                Cst.OTCml_TBL quoteTable = AssetTools.ConvertQuoteEnumToQuoteTbl(quoteCategory);

                //FI 20181126 [24308] Add Alimentation du paramètre pQuoteSide
                QueryParameters qryParamQuote = GetQueryParameters_QUOTE_XXX_H(Cs, quoteTable, m_IdMarketEnv, m_IdValScenario, m_dtFile, QuotationSideEnum.OfficialClose);

                InsertUpdate_QUOTE_XXX_H(qryParamQuote, idAssetUnl, 0, m_dtFile, QuotationSideEnum.OfficialClose, pPrice, pCurrency);
            }
        }

        /// <summary>
        /// Insertion ou mise à jour d'une cotation dans une table QUOTE_XXX_H
        /// </summary>
        /// <param name="pQryParameters">QueryParameters correspondant à un select * sur une table QUOTE_XXX_H</param>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// PM 20151124 [20124] New
        private void InsertUpdate_QUOTE_XXX_H(QueryParameters pQryParameters, int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, decimal pPrice, string pCurrency)
        {

            pQryParameters.Parameters["IDASSET"].Value = pIdAsset;
            DataTable dt = DataHelper.ExecuteDataTable(Cs, pQryParameters.Query, pQryParameters.Parameters.GetArrayDbParameter());

            DataRow dr;
            bool setValues = false;
            if (dt.Rows.Count == 0)
            {
                dr = dt.NewRow();
                SetDataRowIns(dr);
                dt.Rows.Add(dr);
                setValues = true;
            }
            else
            {
                dr = dt.Rows[0];
                // PM 20160219 [21924] Ajout vérification de ENUM.CUSTOMVALUE
                string quoteSource = dr["SOURCE"].ToString();
                if (!IsQuoteOverridable(CSTools.SetCacheOn(Cs), quoteSource))
                {
                    ArrayList message = new ArrayList();
                    string msgDet = " (source:<b>" + quoteSource + "</b>, rule:NOTOVERRIDABLE)";
                    message.Insert(0, msgDet);
                    message.Insert(0, "LOG-06073");
                    ProcessMapping.LogLogInfo(task.SetErrorWarning, null, message);
                }
                else if (((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                    || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                    || (Convert.IsDBNull(dr["IDBC"]) == false)
                    || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                    || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                    || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != m_IdMarketEnv))
                    || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != m_IdValScenario))
                    || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                    || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != pQuoteSide.ToString()))
                    || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                    || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                    || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                    || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                    || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != pQuoteTime))
                    || ((Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice)))
                {
                    SetDataRowUpd(dr);
                    setValues = true;
                }
            }

            if (setValues)
            {
                dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                dr["IDASSET"] = pIdAsset;
                dr["IDBC"] = DBNull.Value; //Non présent
                dr["IDC"] = pCurrency;
                if (pIdM > 0)
                    dr["IDM"] = pIdM;
                dr["IDMARKETENV"] = m_IdMarketEnv;
                dr["IDVALSCENARIO"] = m_IdValScenario;
                dr["ISENABLED"] = true;
                dr["QUOTESIDE"] = pQuoteSide.ToString();
                dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString();
                dr["QUOTEUNIT"] = "Price";
                dr["SOURCE"] = "ClearingOrganization";
                dr["SPREADVALUE"] = 0;
                dr["TIME"] = pQuoteTime;
                dr["VALUE"] = pPrice;

                DataHelper.ExecuteDataAdapter(Cs, pQryParameters.GetQueryReplaceParameters(), dt);
            }
        }

        /// <summary>
        /// Insertion ou mise à jour d'une cotation dans la table QUOTE_ETD_H, avec valeur du Delta
        /// </summary>
        /// <param name="pQryParameters">QueryParameters correspondant à un select * sur une table QUOTE_ETD_H</param>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDelta"></param>
        /// PM 20161019 [22174] Prisma 5.0 : New
        private void InsertUpdate_QUOTE_ETD_H(QueryParameters pQryParameters, int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, decimal pPrice, string pCurrency, decimal pDelta)
        {

            pQryParameters.Parameters["IDASSET"].Value = pIdAsset;
            DataTable dt = DataHelper.ExecuteDataTable(Cs, pQryParameters.Query, pQryParameters.Parameters.GetArrayDbParameter());

            DataRow dr;
            bool setValues = false;
            if (dt.Rows.Count == 0)
            {
                dr = dt.NewRow();
                SetDataRowIns(dr);
                dt.Rows.Add(dr);
                setValues = true;
            }
            else
            {
                dr = dt.Rows[0];
                // PM 20160219 [21924] Ajout vérification de ENUM.CUSTOMVALUE
                string quoteSource = dr["SOURCE"].ToString();
                if (!IsQuoteOverridable(CSTools.SetCacheOn(Cs), quoteSource))
                {
                    ArrayList message = new ArrayList();
                    string msgDet = " (source:<b>" + quoteSource + "</b>, rule:NOTOVERRIDABLE)";
                    message.Insert(0, msgDet);
                    message.Insert(0, "LOG-06073");
                    ProcessMapping.LogLogInfo(task.SetErrorWarning, null, message);
                }
                else if (((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                    || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                    || (Convert.IsDBNull(dr["IDBC"]) == false)
                    || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                    || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                    || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != m_IdMarketEnv))
                    || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != m_IdValScenario))
                    || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                    || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != pQuoteSide.ToString()))
                    || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                    || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                    || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                    || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                    || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != pQuoteTime))
                    || ((Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice))
                    || ((Convert.IsDBNull(dr["DELTA"]) == true) || ((Convert.IsDBNull(dr["DELTA"]) == false) && (Convert.ToDecimal(dr["DELTA"]) != pDelta))))
                {
                    SetDataRowUpd(dr);
                    setValues = true;
                }
            }
            if (setValues)
            {
                dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                dr["IDASSET"] = pIdAsset;
                dr["IDBC"] = DBNull.Value; //Non présent
                dr["IDC"] = pCurrency;
                if (pIdM > 0)
                    dr["IDM"] = pIdM;
                dr["IDMARKETENV"] = m_IdMarketEnv;
                dr["IDVALSCENARIO"] = m_IdValScenario;
                dr["ISENABLED"] = true;
                dr["QUOTESIDE"] = pQuoteSide.ToString();
                dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString();
                dr["QUOTEUNIT"] = "Price";
                dr["SOURCE"] = "ClearingOrganization";
                dr["SPREADVALUE"] = 0;
                dr["TIME"] = pQuoteTime;
                dr["VALUE"] = pPrice;
                dr["DELTA"] = pDelta;

                DataHelper.ExecuteDataAdapter(Cs, pQryParameters.GetQueryReplaceParameters(), dt);
            }

        }

        #endregion Methods
    }
}

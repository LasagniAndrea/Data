using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.EntityMarket;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.EOD;
using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.External
{
    /// <summary>
    /// Class représentant une position
    /// </summary>
    [DataContract(Name = DataHelper<ExternalPosition>.DATASETROWNAME, Namespace = DataHelper<ExternalPosition>.DATASETNAMESPACE)]
    public class ExternalPosition
    {
        #region Members
        private int m_IdA;
        private string m_ActorIdentifier;
        private int m_IdAsset;
        private int m_IdT;
        private string m_Side;
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private decimal m_Quantity;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Id de l'acteur en position
        /// </summary>
        [DataMember(Name = "IDA", Order = 1)]
        public int IdA
        {
            get { return m_IdA; }
            set { m_IdA = value; }
        }

        /// <summary>
        /// Id du book en position
        /// </summary>
        public int IdB
        {
            get { return m_IdA; }
            set { m_IdA = value; }
        }

        /// <summary>
        /// Identifier de l'acteur en position
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string ActorIdentifier
        {
            get { return m_ActorIdentifier; }
            set { m_ActorIdentifier = value; }
        }

        /// <summary>
        /// Identifier du book en position
        /// </summary>
        public string BookIdentifier
        {
            get { return m_ActorIdentifier; }
            set { m_ActorIdentifier = value; }
        }
        
        /// <summary>
        /// Id de l'asset en position
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 3)]
        public int IdAsset
        {
            get { return m_IdAsset; }
            set { m_IdAsset = value; }
        }

        /// <summary>
        /// Id du trade en position
        /// </summary>
        [DataMember(Name = "IDT", Order = 4)]
        public int IdT
        {
            get { return m_IdT; }
            set { m_IdT = value; }
        }

        /// <summary>
        /// Sens (buyer = "1" seller = "2")
        /// </summary>
        [DataMember(Name = "SIDE", Order = 5)]
        public string Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }

        /// <summary>
        /// Quantité en position
        /// </summary>
        [DataMember(Name = "QTY", Order = 6)]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        {
            get { return m_Quantity; }
            set { m_Quantity = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Class représentant les informations sur un Asset
    /// </summary>
    [DataContract(Name = DataHelper<AssetETDInfo>.DATASETROWNAME, Namespace = DataHelper<AssetETDInfo>.DATASETNAMESPACE)]
    public class AssetETDInfo
    {
        #region Members
        private int m_IdAsset;
        private string m_MarketISO10383_ALPHA4;
        private string m_Category;
        private string m_ContractSymbol;
        private string m_ContractAttribute;
        private string m_MaturityMonthYear;
        private string m_PutCall;
        private decimal m_StrikePrice;
        // PM 20180918 [XXXXX] Ajout m_SettlmentMethod et m_ExerciseStyle suite test Prisma Eurosys
        private string m_SettlmentMethod;
        private string m_ExerciseStyle;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Id de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset
        {
            get { return m_IdAsset; }
            set { m_IdAsset = value; }
        }

        /// <summary>
        /// Code Iso du marché
        /// </summary>
        [DataMember(Name = "ISO10383_ALPHA4", Order = 2)]
        public string MarketISO10383_ALPHA4
        {
            get { return m_MarketISO10383_ALPHA4; }
            set { m_MarketISO10383_ALPHA4 = value; }
        }

        /// <summary>
        /// Catégorie du contrat
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 3)]
        public string Category
        {
            get { return m_Category; }
            set { m_Category = value; }
        }

        /// <summary>
        /// Symbol du contrat
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 4)]
        public string ContractSymbol
        {
            get { return m_ContractSymbol; }
            set { m_ContractSymbol = value; }
        }

        /// <summary>
        /// Version du contrat
        /// </summary>
        [DataMember(Name = "CONTRACTATTRIBUTE", Order = 5)]
        public string ContractAttribute
        {
            get { return m_ContractAttribute; }
            set { m_ContractAttribute = value; }
        }

        /// <summary>
        /// Echéance
        /// </summary>
        [DataMember(Name = "MATURITYMONTHYEAR", Order = 6)]
        public string MaturityMonthYear
        {
            get { return m_MaturityMonthYear; }
            set { m_MaturityMonthYear = value; }
        }

        /// <summary>
        /// Put / Call
        /// </summary>
        [DataMember(Name = "PUTCALL", Order = 7)]
        public string PutCall
        {
            get { return m_PutCall; }
            set { m_PutCall = value; }
        }

        /// <summary>
        /// Strike
        /// </summary>
        [DataMember(Name = "STRIKEPRICE", Order = 8)]
        public decimal StrikePrice
        {
            get { return m_StrikePrice; }
            set { m_StrikePrice = value; }
        }

        /// <summary>
        /// Settlment Method
        /// </summary>
        // PM 20180918 [XXXXX] Ajout suite test Prisma Eurosys
        [DataMember(Name = "SETTLTMETHOD", Order = 9)]
        public string SettlmentMethod
        {
            get { return m_SettlmentMethod; }
            set { m_SettlmentMethod = value; }
        }
        
        /// <summary>
        /// Exercise Style
        /// </summary>
        // PM 20180918 [XXXXX] Ajout suite test Prisma Eurosys
        [DataMember(Name = "EXERCISESTYLE", Order = 10)]
        public string ExerciseStyle
        {
            get { return m_ExerciseStyle; }
            set { m_ExerciseStyle = value; }
        }
        
        /// <summary>
        /// Identifier du marché
        /// </summary>
        public string MarketIdentifier
        {
            get { return m_MarketISO10383_ALPHA4; }
            set { m_MarketISO10383_ALPHA4 = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Class contenant un résultat de calcul de déposit
    /// </summary>
    public class ExternalInitialMarginResult
    {
        /// <summary>
        /// Id interne de l'acteur
        /// </summary>
        public int IdA;
        /// <summary>
        /// Identifiant de l'acteur
        /// </summary>
        public string ActorIdentifier;
        /// <summary>
        /// Ensemble des montatns de déposit
        /// </summary>
        public List<Money> Amounts;
        /// <summary>
        /// Type de méthode de calcul du déposit
        /// </summary>
        public InitialMarginMethodEnum CalculationMethodType;
        /// <summary>
        /// Journal XML du calcul du déposit
        /// </summary>
        public string XmlLogFile;
        /// <summary>
        /// Nom des fichiers des états de contrôle du déposit
        /// </summary>
        public Dictionary<string, string> ReportFile = new Dictionary<string,string>();
    }

    /// <summary>
    /// Class de traitement du calcul de déposit (Prisma à partir de la position d'Eurosys)
    /// </summary>
    public class RiskPerformanceExternal
    {
        #region Members
        // Données statiques
        // PM 20180918 [XXXXX] Modification chemin suite test Prisma Eurosys
        //private static Dictionary<string, string> m_ReportXSLTFile = new Dictionary<string, string>() { { "RP-CP046", @"\OTCml\XSL_files\MarginRequirementReport\PRISMA_EUREX_CP046.xslt" } };
        private readonly static Dictionary<string, string> m_ReportXSLTFile = new Dictionary<string, string>() { { "RP-CP046", @"\GUIOutput\MarginRequirementReport\PRISMA_EUREX_CP046.xslt" } };
        //
        // Données reçues
        private readonly string m_ExternalSchemaPrefix = null;
        private string m_ReportPath = null;
        private readonly RiskPerformanceProcess m_Process = null;
        // PM 20170313 [22833] PositionsRepository remplacé par RiskRepository
        //private PositionsRepository m_PositionsRepository = null;
        private RiskRepository m_PositionsRepository = null;
        private RiskMethodFactory m_MethodFactory = null;
        //
        // Données internes
        private List<ExternalPosition> m_Position = default;
        private Dictionary<int, AssetETDInfo> m_AssetInfo = default;
        private Dictionary<int, string> m_ActorRepository = new Dictionary<int, string>();
        private List<ExternalInitialMarginResult> m_InitialMarginResult = default;
        #endregion Members
        #region Accessors
        private string CS { get { return ((m_Process != null) ? m_Process.Cs : string.Empty); } }
        private string SessionId { get { return (((m_Process != null) && (m_Process.Session != null)) ? m_Process.Session.SessionId : string.Empty); } }
        private string WorkingFolder { get { return (((m_Process != null) && (m_Process.AppInstance != null)) ? m_Process.AppInstance.AppWorkingFolder : string.Empty); } }
        // PM 20180918 [XXXXX] Ajout RootFolder suite test Prisma Eurosys
        private string RootFolder { get { return (((m_Process != null) && (m_Process.AppInstance != null)) ? m_Process.AppInstance.AppRootFolder : string.Empty); } }
        private DateTime DtBusiness { get { return ((m_Process != null) ? m_Process.ProcessInfo.DtBusiness : default); } }
        private SettlSessIDEnum Timing { get { return ((m_Process != null) ? m_Process.ProcessInfo.Timing : SettlSessIDEnum.EndOfDay); } }
        #endregion Accessors
        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pProcess"></param>
        public RiskPerformanceExternal(RiskPerformanceProcess pProcess, string pExternalSchemaPrefix)
        {
            m_Process = pProcess;
            if (StrFunc.IsEmpty(pExternalSchemaPrefix))
            {
                m_ExternalSchemaPrefix = "";
            }
            else
            {
                m_ExternalSchemaPrefix = pExternalSchemaPrefix;
            }
        }
        #endregion Constructor
        #region Methods
        /// <summary>
        /// Alimentation de la position
        /// </summary>
        /// <param name="pPositionsRepository"></param>
        /// <param name="pBuildTableId"></param>
        // PM 20170313 [22833] PositionsRepository remplacé par RiskRepository
        //public void BuildPositionsRepository(PositionsRepository pPositionsRepository)
        // PM 20180926 [XXXXX] Prisma v8 : correction pour utilisation de IMASSET_ETD_{BuildTableId}_W
        //public void BuildPositionsRepository(RiskRepository pPositionsRepository)
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void BuildPositionsRepository(RiskRepository pPositionsRepository, string pBuildTableId)
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1045), 1));

            //
            if (pPositionsRepository != default(RiskRepository))
            {
                m_PositionsRepository = pPositionsRepository;
                pPositionsRepository.Clear();
                // PM 20170313 [22833] SettlSessID non utilisé
                //pPositionsRepository.SettlSessID = m_Process.ProcessInfo.Timing;
                // Charger la position dans un dictionnaire temporaire
                m_Position = LoadPosition();
                Dictionary<PosRiskMarginKey, RiskMarginPosition> positionsDictionary = BuildPosition();
                // Ajouter le contenu du dictionnaire temporaire dans celui réel des positions
                foreach (KeyValuePair<PosRiskMarginKey, RiskMarginPosition> keyValuePair in positionsDictionary)
                {
                    // PM 20170313 [22833] PositionsRepository remplacé par RiskRepository
                    //pPositionsRepository.Positions.Add(keyValuePair.Key, keyValuePair.Value);
                    pPositionsRepository.EvaluationData.AddPosition(keyValuePair.Key, keyValuePair.Value);
                }
                // Sauvegarder les id asset de tous les assets en position
                InsertImAssetEtd();
                // Construire le dictionnaire des identifier des acteurs pour le journal du traitement
                m_ActorRepository = (
                    from pos in m_Position
                    group pos by new { pos.IdA, pos.ActorIdentifier } into actorPos
                    select actorPos.Key).ToDictionary(a => a.IdA, a => a.ActorIdentifier);
                // Charger le dictionnaire des informations sur les Asset
                //m_AssetInfo = LoadAssetInfo();
                m_AssetInfo = LoadAssetInfo(pBuildTableId);
                //
                // Alimentation de AssetETDCache
                // PM 20180918 [XXXXX] Ajout suite test Prisma Eurosys
                pPositionsRepository.EvaluationData.PositionETD.SetAssetETDCacheExternal( BuildAssetETDCache() );
            }
            //
            if (m_Process.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<RiskRepository>(pPositionsRepository, new SysMsgCode(SysCodeEnum.LOG, 1067), 
                    m_Process.AppInstance.AppRootFolder, "PositionsRepository.xml", null, m_Process.ProcessState.AddCriticalException);
            }
        }

        /// <summary>
        /// Insertion des différents IdAsset composant la position dans la table IMASSET_ETD
        /// </summary>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        private void InsertImAssetEtd()
        {
            if (m_Position != default(List<ExternalPosition>))
            {
                string queryInsert = DataContractHelper.GetQuery(DataContractResultSets.INSERTIMASSETETD);
                CommandType queryType = DataContractHelper.GetType(DataContractResultSets.INSERTIMASSETETD);
                Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();
                // Selection des différents IdAsset
                IEnumerable<int> distinctIdAsset = (
                    from pos in m_Position
                    select pos.IdAsset
                    ).Distinct();
                //
                // PM 20180926 [XXXXX] IDDC est non null dans IMASSET_ETD_{BuildTableId}_W
                //int? iddc = null;
                int iddc = 0;
                dbParameterValues.Add("IDDC", iddc);
                using (IDbConnection connection = DataHelper.OpenConnection(CS))
                {
                    foreach (int idAsset in distinctIdAsset)
                    {
                        dbParameterValues.Add("IDASSET", idAsset);

                        DataHelper.ExecuteNonQuery(connection, queryType, queryInsert,
                            DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTIMASSETETD, dbParameterValues));

                        dbParameterValues.Remove("IDASSET");
                    }
                }
            }
        }

        /// <summary>
        /// Construction de AssetETDCache à partir de l'ensemble des infos sur les assets
        /// </summary>
        // PM 20180918 [XXXXX] Nouvelle méthode suite test Prisma Eurosys
        public Dictionary<int,SQL_AssetETD> BuildAssetETDCache()
        {
            Dictionary<int, SQL_AssetETD> assetETD = new Dictionary<int, SQL_AssetETD>();
            //
            SQL_AssetETD sqlAssetForDt = new SQL_AssetETD(m_Process.Cs, -1);
            sqlAssetForDt.LoadTable();
            //
            foreach (var assetInfo in m_AssetInfo)
            {
                if (!assetETD.ContainsKey(assetInfo.Key))
                {
                    DataTable newDt = sqlAssetForDt.Dt.Clone();
                    DataRow newRow = newDt.NewRow();
                    //
                    //sqlAsset.IdAsset
                    newRow["IDASSET"] = assetInfo.Key.ToString();
                    //sqlAsset.DrvContract_Symbol
                    newRow["CONTRACTSYMBOL"] = assetInfo.Value.ContractSymbol;
                    //sqlAsset.DrvContract_Attribute
                    newRow["CONTRACTATTRIBUTE"] = assetInfo.Value.ContractAttribute;
                    //sqlAsset.SettlementMethod
                    newRow["SETTLTMETHOD"] = assetInfo.Value.SettlmentMethod;
                    //sqlAsset.Maturity_MaturityMonthYear
                    newRow["MATURITYMONTHYEAR"] = assetInfo.Value.MaturityMonthYear;
                    //sqlAsset.DrvContract_ContractType
                    newRow["CONTRACTTYPE"] = "STD";
                    //sqlAsset.DrvContract_Category
                    newRow["CATEGORY"] = assetInfo.Value.Category;
                    //sqlAsset.PutCall
                    newRow["PUTCALL"] = assetInfo.Value.PutCall;
                    //sqlAsset.StrikePrice
                    newRow["STRIKEPRICE"] = assetInfo.Value.StrikePrice.ToString();
                    //sqlAsset.ExerciseStyle
                    newRow["EXERCISESTYLE"] = assetInfo.Value.ExerciseStyle;
                    //
                    newDt.Rows.Add(newRow);
                    //
                    SQL_AssetETD newSqlAsset = new SQL_AssetETD(newDt);
                    //
                    assetETD.Add(assetInfo.Key, newSqlAsset);
                }
            }
            return assetETD;
        }

        /// <summary>
        /// Construire un dictionnaire de positions (par asset, side et book dealer and clearer) a partir de la position brute
        /// </summary>
        private Dictionary<PosRiskMarginKey, RiskMarginPosition> BuildPosition()
        {
            // Dictionnaire temporaire des positions
            Dictionary<PosRiskMarginKey, RiskMarginPosition> positions = null;

            if (m_Position != default(List<ExternalPosition>))
            {
                var groupedPosition =
                    from pos in m_Position
                    group pos by new { pos.IdAsset, pos.Side, pos.IdB, pos.IdA } into groupedPos
                    select groupedPos;

                // Transformer la collection de positions regroupées en dictionnaire de position
                positions = groupedPosition.ToDictionary(
                    // Définir la clé de position
                    pos => new PosRiskMarginKey
                    {
                        idI = 0,
                        idAsset = pos.Key.IdAsset,
                        Side = pos.Key.Side,
                        idA_Dealer = pos.Key.IdA,
                        idB_Dealer = pos.Key.IdB,
                        idA_EntityDealer = 0,
                        idA_EntityClearer = 0,
                        idA_Clearer = 0,
                        idB_Clearer = 0,
                    },
                    //Définir l'objet position
                    pos => new RiskMarginPosition
                    {
                        TradeIds = (from trade in pos select trade.IdT).Distinct().ToArray(),
                        Quantity = (from trade in pos select trade.Quantity).Sum(),
                    },
                    // Affecter un comparer personnalisé pour la clé de position
                    new PosRiskMarginKeyComparer());
            }
            return positions;
        }

        /// <summary>
        /// Lecture de la position brute
        /// </summary>
        /// <returns></returns>
        private List<ExternalPosition> LoadPosition()
        {
            List<ExternalPosition> positions = default;
            using (IDbConnection connection = DataHelper.OpenConnection(CS))
            {
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness);
                //
                string sqlRequest = "";
                sqlRequest += "select po.IDA," + Cst.CrLf;
                sqlRequest += "       po.IDENTIFIER," + Cst.CrLf;
                sqlRequest += "       po.IDASSET," + Cst.CrLf;
                sqlRequest += "       po.IDT," + Cst.CrLf;
                sqlRequest += "       po.SIDE," + Cst.CrLf;
                sqlRequest += "       po.QTY" + Cst.CrLf;
                sqlRequest += "  from " + m_ExternalSchemaPrefix + "EVW_EUROSYS_IM_POSITION po" + Cst.CrLf;
                sqlRequest += " where po.DTBUSINESS = @DTBUSINESS";
                QueryParameters qp = new QueryParameters(CS, sqlRequest, dp);
                //
                positions = DataHelper<ExternalPosition>.ExecuteDataSet(
                    connection,
                    CommandType.Text,
                    sqlRequest,
                    qp.GetArrayDbParameterHint());
            }
            return positions;
        }

        /// <summary>
        /// Lecture des informations sur les Asset
        /// </summary>
        /// <param name="pBuildTableId"></param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        // PM 20180918 [XXXXX] Ajout SETTLTMETHOD et EXERCISESTYLE
        // PM 20180926 [XXXXX] Prisma v8 : correction pour utilisation de IMASSET_ETD_{BuildTableId}_W
        //private Dictionary<int, AssetETDInfo> LoadAssetInfo()
        private Dictionary<int, AssetETDInfo> LoadAssetInfo(string pBuildTableId)
        {
            Dictionary<int, AssetETDInfo> assetInfo = default;
            using (IDbConnection connection = DataHelper.OpenConnection(CS))
            {
                //DataParameters dp = new DataParameters();
                //dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.SESSIONID), SessionId);
                //
                // PM 20180926 [XXXXX] Correction : ne pas prendre "SessionId" mais "pBuildTableId"
                string sqlRequest = @"select asset.IDASSET, asset.ISO10383_ALPHA4, asset.CATEGORY,
                asset.CONTRACTSYMBOL, asset.CONTRACTATTRIBUTE, asset.MATURITYMONTHYEAR, asset.PUTCALL, asset.STRIKEPRICE,
                asset.SETTLTMETHOD, asset.EXERCISESTYLE
                from " + m_ExternalSchemaPrefix + @"EVW_EUROSYS_ASSET_ETD asset
                inner join dbo." + StrFunc.AppendFormat("IMASSET_ETD_{0}_W", pBuildTableId).ToUpper() + " im on (im.IDASSET = asset.IDASSET)" + Cst.CrLf;
                QueryParameters qp = new QueryParameters(CS, sqlRequest, null);
                // PM 20180926 [XXXXX] Correction : pas de parameters
                //List<AssetETDInfo> asset = DataHelper<AssetETDInfo>.ExecuteDataSet(connection, CommandType.Text, qp.query, qp.GetArrayDbParameterHint());
                List<AssetETDInfo> asset = DataHelper<AssetETDInfo>.ExecuteDataSet(connection, CommandType.Text, qp.Query);
                if (asset != default(List<AssetETDInfo>))
                {
                    assetInfo = asset.ToDictionary(a => a.IdAsset, a => a);
                }
                else
                {
                    assetInfo = new Dictionary<int, AssetETDInfo>();
                }
            }
            return assetInfo;
        }

        /// <summary>
        /// Initialisation des déposits à calculer et chargement des paramètres de calcul
        /// </summary>
        /// <param name="pMethodFactory"></param>
        /// <param name="pImRequestDiagnostics"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void InitializeDeposits(RiskMethodFactory pMethodFactory, IMRequestDiagnostics pImRequestDiagnostics)
        {
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1081), 1));

            //
            m_MethodFactory = pMethodFactory;

            // Création des paramètres fictifs sur les marchés
            // PM 20180918 [XXXXX] Suite test Prisma Eurosys : Ajout lecture des paramètres MARKET et CSS
            //EntityMarketWithCSS[] entityMarket = new EntityMarketWithCSS[] { new EntityMarketWithCSS { MarketId = 0, MarketCrossMarginActivated = false, CssCurrency = "EUR" }};
            IEnumerable<EntityMarketWithCSS> entityMarket =
                from marketCss in m_Process.MarketsCollectionFromEntity.Values
                    where marketCss.Market == "XEUR"
                    select marketCss;

            // PM 20180918 [XXXXX] Recherche Id du CSS pour utilisation de celui-ci (au lieu de 0 dans les versions précédantes)
            int idCss = entityMarket.FirstOrDefault().CssId;

            // Construction de la méthode de calcul
            // PM 20150423 [20575] Ajout pDtMarket = pDtBusiness
            // PM 20160404 [22116] BaseMethod remplacé par RiskMethodSet
            //BaseMethod method = m_MethodFactory.BuildMethod(0, m_Process.ProcessInfo.DtBusiness, m_Process.ProcessInfo.DtBusiness, 0, m_Process.ProcessInfo.Timing, InitialMarginMethodEnum.EUREX_PRISMA, m_Process.appInstance.SessionId, pImRequestDiagnostics, m_Process.ProcessInfo.TimeBusiness);
            // PM 20180219 [23824] Modification des paramètres
            //RiskMethodSet methodSet = m_MethodFactory.BuildMethodSetExternal(m_Process.Cs, 0, m_Process.ProcessInfo.DtBusiness, m_Process.ProcessInfo.DtBusiness, 0,
            // InitialMarginMethodEnum.EUREX_PRISMA, m_Process.ProcessInfo.Timing, m_Process.ProcessInfo.TimeBusiness, m_Process.appInstance.SessionId,
            // pImRequestDiagnostics, m_PositionsRepository.EvaluationData.AssetETDCache, entityMarket);
            // PM 20180918 [XXXXX] Suite test Prisma Eurosys : Suppression de AssetETDCache et ajout de m_PositionsRepository. AssetETDCache sera construit à partir de m_PositionsRepository
            //RiskMethodSet methodSet = m_MethodFactory.BuildMethodSetExternal(m_Process.ProcessInfo, 0, InitialMarginMethodEnum.EUREX_PRISMA, pImRequestDiagnostics,
            // m_PositionsRepository.EvaluationData.AssetETDCache, entityMarket);
            RiskMethodSet methodSet = m_MethodFactory.BuildMethodSetExternal(m_Process.ProcessInfo, idCss, InitialMarginMethodEnum.EUREX_PRISMA, pImRequestDiagnostics,
             m_PositionsRepository, entityMarket);

            // Ajout des paramètres fictifs sur les marchés
            // PM 20160404 [22116] Déplacé plus haut
            //EntityMarketWithCSS[] entityMarket = new EntityMarketWithCSS[] { new EntityMarketWithCSS { MarketId = 0, MarketCrossMarginActivated = false, CssCurrency = "EUR" }};
            //method.BuildMarketParameters(entityMarket);

            // Ajout des paramètres fictifs sur les MRO
            // PM 20150930 [21134] Add ScanRiskOffsetCapPrct
            // PM 20170106 [22633] Add IsInitialMarginForExeAssPosition
            // PM 20170313 [22833] Refactoring : PositionsRepository remplacé par RiskRepository
            //IEnumerable<MarginReqOfficeParameter> mroParams =
            //    from pos in m_PositionsRepository.Positions
            //    group pos by pos.Key.idA_Dealer into disctinctActor
            //    select new MarginReqOfficeParameter
            //    {
            //        ActorId = disctinctActor.Key,
            //        BookId = 0,
            //        CssId = 0,
            //        MarketId = 0,
            //        StockCoverageType = PosStockCoverEnum.None,
            //        WeightingRatio = 1,
            //        SpanAccountType = SpanAccountType.Member,
            //        SpanMaintenanceAmountIndicator = true,
            //        ScanRiskOffsetCapPrct = 0,
            //        IsInitialMarginForExeAssPosition = true,
            //    };
            IEnumerable<MarginReqOfficeParameter> mroParams =
                from idaDealer in m_PositionsRepository.EvaluationData.GetDealersIdA()
                select new MarginReqOfficeParameter
                {
                    ActorId = idaDealer,
                    BookId = 0,
                    CssId = 0,
                    MarketId = 0,
                    StockCoverageType = PosStockCoverEnum.None,
                    WeightingRatio = 1,
                    SpanAccountType = SpanAccountType.Member,
                    SpanMaintenanceAmountIndicator = true,
                    ScanRiskOffsetCapPrct = 0,
                    IsInitialMarginForExeAssPosition = true,
                };

            // PM 20160404 [22116] BaseMethod remplacé par RiskMethodSet
            //method.MarginReqOfficeParameters = (
            Dictionary<int, IEnumerable<MarginReqOfficeParameter>> marginReqOfficeParameters = (
                from param in mroParams
                select new { key = param.ActorId, values = new List<MarginReqOfficeParameter> { param } }
                ).ToDictionary( mro => mro.key, mro => mro.values.AsEnumerable() );
            //
            methodSet.MarginReqOfficeParameters = marginReqOfficeParameters;

            // Initialisation des déposits à calculer par la méthode
            // PM 20160404 [22116] BaseMethod remplacé par RiskMethodSet
            //InitializeMethodDeposits(method);
            InitializeMethodDeposits(methodSet);

            // Chargement des paramètres spécifique de la méthode
            // PM 20160404 [22116] Déplacé plus haut dans BuildMethodSetExternal
            //method.LoadParameters(m_Process.Cs, m_PositionsRepository.AssetETDCache);
        }

        /// <summary>
        /// Initialisation des déposits à calculer par la méthode
        /// </summary>
        /// <param name="pMethodSet"></param>
        /// PM 20160404 [22116] BaseMethod remplacé par RiskMethodSet
        //private void InitializeMethodDeposits(BaseMethod pMethod)
        private void InitializeMethodDeposits(RiskMethodSet pMethodSet)
        {
            pMethodSet.Deposits.Clear();

            // Regrouper les positions par Acteur/Book
            // PM 20170313 [22833] Refactoring : PositionsRepository remplacé par RiskRepository
            //Dictionary<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> posByActorBook = (
            //    from positions in m_PositionsRepository.Positions
            //    group positions by new { positions.Key.idA_Dealer, positions.Key.idB_Dealer } into posActorBook
            //    select new
            //    {
            //        Key = new Pair<int, int>(posActorBook.Key.idA_Dealer,posActorBook.Key.idB_Dealer),
            //        Positions = from pos in posActorBook
            //                    select new Pair<PosRiskMarginKey, RiskMarginPosition>
            //                    {
            //                        First = pos.Key,
            //                        Second = pos.Value
            //                    },
            //    }
            //    ).ToDictionary( po => po.Key, po => po.Positions.ToList());
            Dictionary<RiskActorBook, RiskData> posByActorBook = RiskData.AggregateByActorBook(new RiskData[] { m_PositionsRepository.EvaluationData });

            // Créer une liste de RiskElement pour chaque Acteur/Book
            List<RiskElement> riskElementList = new List<RiskElement>();
            foreach (var actorBookPos in posByActorBook)
            {
                // PM 20170313 [22833] Refactoring : changement de type de posByActorBook
                //RiskElement actorRiskElement =
                //    RoleMarginReqOfficeAttribute.CreateRootElement(actorBookPos.Key.First, actorBookPos.Key.Second,
                //        RiskElementEvaluation.SumPosition, RiskElementClass.DEPOSIT);

                //actorRiskElement.Positions.Add(actorBookPos.Key, actorBookPos.Value);

                RiskElement actorRiskElement =
                    RoleMarginReqOfficeAttribute.CreateRootElement(actorBookPos.Key.IdA, actorBookPos.Key.IdB,
                        RiskElementEvaluation.SumPosition, RiskElementClass.DEPOSIT);

                actorRiskElement.RiskDataActorBook.Add(actorBookPos.Key, actorBookPos.Value);

                riskElementList.Add(actorRiskElement);
            }

            // Créer un Deposit pour chaque RiskElement
            var depositInputDatas =
               from rootElement in riskElementList
               select new
               {
                   Root = rootElement,
                   IsGrossMargining = false,
                   //ActorsBooksInPosition = rootElement.Positions.Keys.Distinct(new PairComparer<int, int>()),
                   ActorsBooksInPosition = rootElement.RiskDataActorBook.Keys.Distinct(new RiskActorBookComparer()),
                   RiskElements = new List<RiskElement>() { rootElement },
                   Result = default(TradeRisk),
                   HierarchyClass = DepositHierarchyClass.ENTITY,
                   WeightingRatio = 1,
                   Ancestors = new List<Pair<int, int>>()
               };

            foreach (var inputDatas in depositInputDatas)
            {
                Deposit deposit = new Deposit(
                    inputDatas.Root,
                    inputDatas.IsGrossMargining,
                    inputDatas.RiskElements,
                    inputDatas.ActorsBooksInPosition,
                    inputDatas.HierarchyClass,
                    inputDatas.Result,
                    inputDatas.WeightingRatio,
                    inputDatas.Ancestors);

                Pair<int, int> pairToAdd = new Pair<int, int>(inputDatas.Root.ActorId, inputDatas.Root.AffectedBookId);

                if (!pMethodSet.Deposits.ContainsKey(pairToAdd))
                {
                    pMethodSet.Deposits.Add(
                        pairToAdd,
                        deposit);
                }
            }

            // PM 20160404 [22116] Initialiser les déposit de chaque méthode (pour compatibilité avec existant)
            foreach (BaseMethod method in pMethodSet.Methods.Values)
            {
                method.Deposits = pMethodSet.Deposits;
            }
        }

        /// <summary>
        /// Evaluation des déposits
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel EvaluateDeposits()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1082), 1));

            foreach (RiskMethodSet methSet in m_MethodFactory.MethodSet.Values)
            {
                if (IRQTools.IsIRQRequested(m_Process, m_Process.IRQNamedSystemSemaphore, ref ret))
                {
                    break;
                }
                ret = methSet.Evaluate(m_Process);
            }
            return ret;
        }

        /// <summary>
        /// Ecriture des résultats
        /// </summary>
        /// <param name="pLogsRepository"></param>
        /// <param name="pEurosysReportPath"></param>
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel CreateResults(CalculationSheetRepository pLogsRepository, string pEurosysReportPath)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1083), 1));

            //
            if (StrFunc.IsEmpty(pEurosysReportPath))
            {
                m_ReportPath = "C:";
            }
            else
            {
                if (pEurosysReportPath.EndsWith(@"\"))
                {
                    m_ReportPath = pEurosysReportPath.Remove(pEurosysReportPath.Length - 1);
                }
                else
                {
                    m_ReportPath = pEurosysReportPath;
                }
            }

            // Ensemble des résultats
            var resultDatas =
                from methodSet in m_MethodFactory.MethodSet.Values
                from deposit in methodSet.Deposits
                select new
                {
                    IdA = deposit.Key.First,
                    deposit.Value.Amounts,
                    CalculationMethodType = methodSet.MethodsType,
                    RiskElements = deposit.Value.Factors,
                    deposit.Value.MarginCalculationMethods,
                };

            // Données pour l'écriture sur la base de données
            m_InitialMarginResult = new List<ExternalInitialMarginResult>();

            // Construction du log détaillé de chaque deposit
            foreach (var deposit in resultDatas)
            {
                if (IRQTools.IsIRQRequested(m_Process, m_Process.IRQNamedSystemSemaphore, ref ret))
                    break;

                // Nouveau résultats de calcul à sauvegarder
                ExternalInitialMarginResult imResult = new ExternalInitialMarginResult()
                {
                    IdA = deposit.IdA,
                    Amounts = deposit.Amounts,
                    // PM 20160404 [22116] CalculationMethodType étant maintenant un tableau, prendre ici le premier élément
                    //CalculationMethodType = deposit.CalculationMethodType,
                    CalculationMethodType = deposit.CalculationMethodType[0],
                };
                m_ActorRepository.TryGetValue(imResult.IdA, out imResult.ActorIdentifier);

                // Document du détail du log
                MarginDetailsDocument calculationsheet = CreateMarginDetailsDocument(
                    imResult,
                    pLogsRepository,
                    deposit.RiskElements,
                    deposit.MarginCalculationMethods[0]);

                // Ecriture des fichiers reports et log
                WriteReports(imResult, calculationsheet);

                // Ajout du résultat à ceux à écrire en base
                m_InitialMarginResult.Add(imResult);
            }

            if ((Cst.ErrLevel.IRQ_EXECUTED != ret) && (false == IRQTools.IsIRQRequested(m_Process, m_Process.IRQNamedSystemSemaphore, ref ret)))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1084), 1));

                // Ecriture des résultats dans la base de données
                WriteResults();
            }
            return ret;
        }

        /// <summary>
        /// Creation du document contenant le détail du calcul
        /// </summary>
        /// <param name="pImResult"></param>
        /// <param name="pLogsRepository"></param>
        /// <param name="pRiskElements"></param>
        /// <param name="pMethodType"></param>
        /// <param name="pMarginCalculationMethod"></param>
        /// <returns></returns>
        private MarginDetailsDocument CreateMarginDetailsDocument(
            ExternalInitialMarginResult pImResult,
            CalculationSheetRepository pLogsRepository,
            List<RiskElement> pRiskElements,
            IMarginCalculationMethodCommunicationObject pMarginCalculationMethod)
        {
            // Document du détail du log
            MarginDetailsDocument calculationsheet = new MarginDetailsDocument();
            FpML.v44.Doc.Trade tradeInfo = new FpML.v44.Doc.Trade();
            calculationsheet.trade = new FpML.v44.Doc.Trade[] { tradeInfo };
            tradeInfo.tradeHeader.tradeDate.DateValue = DtBusiness;

            // Clé composée uniquement de Id Acteur et Id Book
            MarginDetailsDocumentKey key = new MarginDetailsDocumentKey(pImResult.IdA, pImResult.IdA, 0);
            
            // Ajout du document dans le dictionnnaire des logs
            pLogsRepository.CalculationSheets.Add(key, calculationsheet);

            // Gestion de l'élément MarginRequirementOffice
            MarginRequirementOffice office = calculationsheet.marginRequirementOffice;
            office.href = pImResult.ActorIdentifier;
            office.payerPartyReference = new PartyReference(pImResult.ActorIdentifier);
            office.bookId = new EfsML.v30.Doc.BookId
            {
                bookName = pImResult.ActorIdentifier,
                bookNameSpecified = true
            };
            office.isGross = false;
            office.weightingRatio = 1;
            office.marginAmounts = pImResult.Amounts.ToArray();

            // Ajout de l'élément contenant le détail du calcul (en net: pIsGrossMargining = false)
            office.marginCalculation = new MarginCalculation(false);

            // Ajout des montants de déposit
            NetMargin netMargin = calculationsheet.marginRequirementOffice.marginCalculation.netMargin;
            netMargin.marginAmounts = office.marginAmounts;

            // Affectation des dictionnaires nécéssaire à la construction du log
            pLogsRepository.ActorRepository = m_ActorRepository;
            pLogsRepository.AssetInfo = m_AssetInfo;

            // Construction du log détailé par type de méthode
            // PM 20200910 [25481] Suppresion paramètre pMethodType
            //netMargin.marginCalculationMethod = pLogsRepository.MarginCalculationMethodFactory(pImResult.CalculationMethodType, pMarginCalculationMethod);
            netMargin.marginCalculationMethod = pLogsRepository.MarginCalculationMethodFactory(pMarginCalculationMethod);
            // PM 20151116 [21561] Intégré dans MarginCalculationMethodFactory
            //netMargin.marginCalculationMethod.marginAmounts = pMarginCalculationMethod.MarginAmounts.Cast<Money>().ToArray();

            // Ajout de la position
            // PM 20170313 [22833] Gestion position et tradevalue
            //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = PositionsExtractor.GetPositions(pRiskElements);
            RiskData RiskData = RiskElement.AggregateRiskData(pRiskElements);
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = RiskData.GetPosition();

            netMargin.positions = pLogsRepository.BuildExternalPositionReport(DtBusiness, Timing, positions);
            //
            return calculationsheet;
        }

        /// <summary>
        /// Ecriture des fichiers de log et reports
        /// </summary>
        /// <param name="pImResult"></param>
        /// <param name="pDetailsDocument"></param>
        private void WriteReports(ExternalInitialMarginResult pImResult, MarginDetailsDocument pDetailsDocument)
        {
            // Génération du log XML
            StringBuilder xmlData = RiskPerformanceSerializationHelper.DumpObjectToString<MarginDetailsDocument>
                (pDetailsDocument, m_Process.ProcessState.SetErrorWarning, m_Process.ProcessState.AddCriticalException);
            
            string xmlText = xmlData.ToString();

            string fileName = String.Format("XML_MR-{0}", pImResult.IdA);
            fileName = FileTools.GetUniqueName(fileName, pImResult.ActorIdentifier);
            pImResult.XmlLogFile = m_ReportPath + @"\" + fileName + ".xml";
            FileTools.WriteStringToFile(xmlText, pImResult.XmlLogFile);

            // Génération des reports
            string temporarypath = m_Process.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True);
            foreach (KeyValuePair<string, string> reportXSLT in m_ReportXSLTFile)
            {
                // Transformation Xsl
                // PM 20180918 [XXXXX] Utilisation de RootFolder à la place de WorkingFolder suite test Prisma Eurosys
                //string xslFile = WorkingFolder + reportXSLT.Value;
                string xslFile = RootFolder + reportXSLT.Value;
                string result = XSLTTools.TransformXml(new StringBuilder(xmlText), xslFile, null, null);

                // Transformation Fop
                // EG 20160404 Migration vs2013
                //byte[] binaryResult = FopEngine_V2.TransformToByte(CSTools.SetCacheOn(CS), result, temporarypath);
                byte[] binaryResult = FopEngine.TransformToByte(CSTools.SetCacheOn(CS), result, temporarypath);

                // Ecriture du fichier 
                fileName = reportXSLT.Key + String.Format("_MR-{0}", pImResult.IdA);
                fileName = FileTools.GetUniqueName(fileName, pImResult.ActorIdentifier);
                string fileFullName = m_ReportPath + @"\" + fileName + ".pdf";
                FileTools.WriteBytesToFile(binaryResult, fileFullName, FileTools.WriteFileOverrideMode.Override);
                //
                if (pImResult.ReportFile != default(Dictionary<string, string>))
                {
                    pImResult.ReportFile.Add(reportXSLT.Key, fileFullName);
                }
            }
        }

        /// <summary>
        /// Ecriture des résultats sur la base de données
        /// </summary>
        private void WriteResults()
        {
            const string resultTableName = "DEPOSIT_SPHERES";
            DataTable resultTable = null;
            string sqlSelectQuery = "select * from " + m_ExternalSchemaPrefix + resultTableName + " where 0 = 1";
            //
            try
            {
                string sqlDeleteQuery = "delete from " + m_ExternalSchemaPrefix + resultTableName + " where (DATE_TRAIT = @DTBUSINESS)";
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness);
                QueryParameters qp = new QueryParameters(CS, sqlDeleteQuery, dp);
                //
                DataHelper.ExecuteNonQuery(CS, CommandType.Text, qp.Query, qp.GetArrayDbParameterHint());
                //
                DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlSelectQuery);
                resultTable = ds.Tables[0];
                resultTable.TableName = resultTableName;

                foreach (ExternalInitialMarginResult deposit in m_InitialMarginResult)
                {
                    foreach (Money amount in deposit.Amounts)
                    {
                        DataRow dr = resultTable.NewRow();
                        dr.BeginEdit();
                        dr["DATE_TRAIT"] = DtBusiness;
                        dr["IDA"] = deposit.IdA;
                        dr["IDC"] = amount.Currency;
                        dr["MONT_DEPOSIT"] = amount.Amount.DecValue;
                        dr["METH_CAL_DP"] = deposit.CalculationMethodType.ToString();
                        dr["XMLFILE"] = deposit.XmlLogFile;
                        if (deposit.ReportFile != default(Dictionary<string, string>))
                        {
                            // 3 reports max
                            int nb_reports = System.Math.Min(3, deposit.ReportFile.Count);
                            for (int i = 0; i < nb_reports; i += 1)
                            {
                                string columnReportName = String.Format("REPORT{0}_NAME", i + 1);
                                string columnReportPdfFile = String.Format("REPORT{0}_PDFFILE", i + 1);
                                KeyValuePair<string, string> report = deposit.ReportFile.ElementAt(i);
                                dr[columnReportName] = report.Key;
                                dr[columnReportPdfFile] = report.Value;
                            }
                        }
                        dr.EndEdit();
                        resultTable.Rows.Add(dr);
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                DataHelper.ExecuteDataAdapter(CS, sqlSelectQuery, resultTable);
            }
        }

        /// <summary>
        /// Lecture du paramétrage de MARKET et CSS pour l'Eurex
        /// </summary>
        /// <param name="pMarketsCollection">Collection qui sera alimentée</param>
        /// <returns></returns>
        // PM 20180918 [XXXXX] Nouvelle méthode suit test Prisma Eurosys
        public EntityMarketWithCSS[] BuildMarketsCSSExternal(MarketsDictionary pMarketsCollection)
        {
            EntityMarketWithCSS[] entityMarketArray = new EntityMarketWithCSS[] { new EntityMarketWithCSS { MarketId = 0, MarketCrossMarginActivated = false, CssCurrency = "EUR" } };
            if (pMarketsCollection != default(MarketsDictionary))
            {
                string sqlRequest = "";
                sqlRequest += "select m.IDENTIFIER            as MARKET," + Cst.CrLf;
                sqlRequest += "       m.IDM                   as MARKETID," + Cst.CrLf;
                sqlRequest += "       m.DISPLAYNAME           as MARKETNAME," + Cst.CrLf;
                sqlRequest += "       null                    as MARKET_DTEN," + Cst.CrLf;
                sqlRequest += "       null                    as MARKET_DTDIS," + Cst.CrLf;
                sqlRequest += "       m.ASSIGNMENTMETHOD      as MARKET_ASSMETH," + Cst.CrLf;
                sqlRequest += "       m.POSSTOCKCOVER         as MARKET_STOCKCOVERAGE," + Cst.CrLf;
                sqlRequest += "       m.ISIMINTRADAYEXEASS    as MARKET_EXEASS," + Cst.CrLf;
                sqlRequest += "       m.ISIMCROSSMARGINING    as MARKET_CROSSMARGIN," + Cst.CrLf;
                sqlRequest += "       m.IDBC                  as MARKET_BC," + Cst.CrLf;
                sqlRequest += "       0                       as EM_IDEM," + Cst.CrLf;
                sqlRequest += "       null                    as EM_DTBUSINESS," + Cst.CrLf;
                sqlRequest += "       null                    as DTENTITYNEXT," + Cst.CrLf;
                sqlRequest += "       null                    as DTMARKET," + Cst.CrLf;
                sqlRequest += "       0                       as EM_CROSSMARGIN," + Cst.CrLf;
                sqlRequest += "       a.IDENTIFIER            as CSS," + Cst.CrLf;
                sqlRequest += "       css.IDA                 as CSSID," + Cst.CrLf;
                sqlRequest += "       a.DISPLAYNAME           as CSSNAME," + Cst.CrLf;
                sqlRequest += "       null                    as CSS_DTEN," + Cst.CrLf;
                sqlRequest += "       null                    as CSS_DTDIS," + Cst.CrLf;
                sqlRequest += "       css.ASSIGNMENTMETHOD    as CSS_ASSMETH," + Cst.CrLf;
                sqlRequest += "       css.IDC                 as CSS_CURRENCY," + Cst.CrLf;
                sqlRequest += "       null                    as IDBCENTITY," + Cst.CrLf;
                sqlRequest += "       css.IDIOTASK_RISKDATA   as IDIOTASK_RISKDATA," + Cst.CrLf;
                sqlRequest += "       css.ISRISKDATAONHOLIDAY as ISRISKDATAONHOLIDAY" + Cst.CrLf;
                sqlRequest += "  from dbo.MARKET m" + Cst.CrLf;
                sqlRequest += " inner join dbo.ACTOR a on (a.IDA = m.IDA)" + Cst.CrLf;
                sqlRequest += " inner join dbo.CSS on (css.IDA = m.IDA)" + Cst.CrLf;
                sqlRequest += " where (m.ISO10383_ALPHA4 = 'XEUR') and (m.EXCHANGESYMBOL = 'XEUR')" + Cst.CrLf;

                using (IDbConnection connection = DataHelper.OpenConnection(CS))
                {
                    IEnumerable<EntityMarketWithCSS> entityMarkets = DataHelper<EntityMarketWithCSS>.ExecuteDataSet(connection, CommandType.Text, sqlRequest);

                    if (entityMarkets.Count() == 0)
                    {
                        entityMarkets = entityMarketArray;
                    }
                    else
                    {
                        entityMarketArray = entityMarkets.ToArray();
                    }
                    foreach (EntityMarketWithCSS entityMarket in entityMarkets)
                    {
                        string key = entityMarket.Market;
                        if (true != (String.IsNullOrEmpty(key) || pMarketsCollection.ContainsKey(key)))
                        {
                            entityMarket.DateBusiness = m_Process.ProcessInfo.DtBusiness;
                            entityMarket.DateBusinessNext = m_Process.ProcessInfo.DtBusiness.AddDays(1);
                            entityMarket.DateMarket = m_Process.ProcessInfo.DtBusiness;
                            entityMarket.ElementDisabledFrom = DateTime.MaxValue;
                            entityMarket.ElementEnabledFrom = DateTime.MinValue;
                            entityMarket.ForcedDateBusiness = m_Process.ProcessInfo.DtBusiness;
                            entityMarket.MarketDisabledFrom = DateTime.MaxValue;
                            entityMarket.MarketEnableFrom = DateTime.MinValue;
                            entityMarket.MarketCrossMarginActivated = false;
                            entityMarket.CssCurrency = "EUR";
                            //
                            pMarketsCollection.Add(key, entityMarket);
                        }
                    }
                }
            }
            return entityMarketArray;
        }
        #endregion Methods
    }
}

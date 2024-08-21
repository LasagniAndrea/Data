using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.DataContracts;
using EFS.SpheresRiskPerformance.EntityMarket;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;
//
using FixML.Enum;
//
namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Classe de constitution des données sur lesquelles portera l'évaluation du risque
    /// </summary>
    /// PM 20170313 [22833] New
    [XmlRoot(ElementName = "EvaluationDataDictionary")]
    // EG 20190114 Add detail to ProcessLog Refactoring
    public class RiskRepository
    {
        #region Members
        /// <summary>
        /// Données sur lesquelles portent le calcul de déposit
        /// </summary>
        private readonly RiskData m_EvaluationData;

        /// <summary>
        /// Niveau de détail actuel du log du processus courant
        /// </summary>
        protected LogLevelDetail m_LogDetailEnum;

        
        /// <summary>
        /// Add Exception Delegate
        /// </summary>
        protected AddCriticalException m_AddCriticalException;

        /// <summary>
        /// Timing de l'évaluation du deposit (EOD / Intraday)
        /// </summary>
        private SettlSessIDEnum m_SettlSessID;

        /// <summary>
        /// Tableau des Id des marchés à traiter lors du calcul.
        /// </summary>
        protected int[] m_Markets;

        /// <summary>
        /// Tableau des Id des marchés pour lequels les dénouements ne doivent pas être pris en compte lors d'un calcul IntraDay.
        /// Pour un calcul EOD, ce tableau reste vide
        /// </summary>
        private int[] m_MarketsWithoutIntraDayExeAssActivated;

        /// <summary>
        /// Tableau des Id des CSS à traiter lors du calcul.
        /// </summary>
        /// PM 20170602 [23212] Ajout m_CSSId
        private int[] m_CSSId;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Données sur lesquelles portent le calcul de déposit
        /// </summary>
        internal RiskData EvaluationData
        {
            get { return m_EvaluationData; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public RiskRepository()
        {
            m_EvaluationData = new RiskData();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Initialisation du Log
        /// </summary>
        /// <param name="pLogDetailEnum">Niveau de log</param>
        /// <param name="pAddException"></param>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        public void InitializeDeletage(LogLevelDetail pLogDetailEnum, AddCriticalException pAddException)
        {
            m_LogDetailEnum = pLogDetailEnum;
            m_AddCriticalException = pAddException;
        }

        /// <summary>
        /// Initialise les marchés pour lesquels effectuer le calcul
        /// </summary>
        /// <param name="pSettlSessID">EOD / IntraDay</param>
        /// <param name="pMarketsCollectionFromEntity">Marchés relatif à la chambre et l'entité pour lesquels le calcul a été demandé</param>
        public void InitializeMarkets(SettlSessIDEnum pSettlSessID, MarketsDictionary pMarketsCollectionFromEntity)
        {
            m_SettlSessID = pSettlSessID;

            // Construction des collections de Market Id relatif à la chambre 
            m_Markets =
                pSettlSessID == SettlSessIDEnum.EndOfDay ?
                    (from marketcss in pMarketsCollectionFromEntity select marketcss.Value.MarketId).ToArray()
                    :
                    (from marketcss in pMarketsCollectionFromEntity
                     where marketcss.Value.MarketIsIntradayExeAssActivated
                     select marketcss.Value.MarketId).ToArray();

            m_MarketsWithoutIntraDayExeAssActivated =
                pSettlSessID == SettlSessIDEnum.Intraday ?
                    (from marketcss in pMarketsCollectionFromEntity
                     where !marketcss.Value.MarketIsIntradayExeAssActivated
                     select marketcss.Value.MarketId).ToArray()
                    :
                    new int[0];

            // PM 20170602 [23212] Ajout alimentation de m_CSSId
            m_CSSId = pMarketsCollectionFromEntity.GetCSSInternalIDs().ToArray();
        }

        /// <summary>
        /// Build the net position collection by actor/book
        /// </summary>
        /// <param name="session">Instance courante</param>
        /// <param name="pCS">Connection string</param>
        /// <param name="pIdEntity">Id de l'entity, racine de la hierarchie d'acteur</param>
        /// <param name="pDtBusiness">business date courante, toutes les dates d'échéance des assets chargés doivent être antérieures à cette date</param>
        /// <param name="pDtBusinessNext">business date suivante</param>
        /// PM 20170808 [23371] Ajout pDtBusinessNext
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        // EG 20181119 PERF Correction post RC (Step 2)
        // PM 20221212 [XXXXX] Ajout gestion trades sans calcul de déposit
        public void BuildRepository(AppSession session, string pCS, int pIdEntity, DateTime pDtBusiness, DateTime pDtBusinessNext)
        {
            List<TradeAllocation> trades = new List<TradeAllocation>();
            List<TradeAllocation> physicalsettlements = new List<TradeAllocation>();
            List<TradeValue> tradeValues = new List<TradeValue>();
            List<TradeNoMargin> tradeNoMargin = new List<TradeNoMargin>();

            if (pIdEntity <= 0 || (ArrFunc.IsEmpty(m_Markets) && ArrFunc.IsEmpty(m_MarketsWithoutIntraDayExeAssActivated)))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, new ArgumentException("SYS-01021"));
            }

            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                if (false == ArrFunc.IsEmpty(m_Markets))
                {
                    trades = LoadTradesActions(session.AppInstance, pCS, connection, pIdEntity, pDtBusiness, pDtBusiness, m_Markets);
                }

                if (false == ArrFunc.IsEmpty(m_MarketsWithoutIntraDayExeAssActivated))
                {
                    trades.AddRange(LoadTradesActions(session.AppInstance, pCS, connection, pIdEntity, pDtBusiness.Date, pDtBusiness, m_MarketsWithoutIntraDayExeAssActivated));
                }

                if (false == ArrFunc.IsEmpty(m_Markets))
                {
                    physicalsettlements = LoadPhysicalSettlements(pCS, session.AppInstance, connection, pIdEntity, pDtBusiness, m_Markets);
                }

                if (false == ArrFunc.IsEmpty(m_MarketsWithoutIntraDayExeAssActivated))
                {
                    physicalsettlements.AddRange(LoadPhysicalSettlements(pCS, session.AppInstance, connection, pIdEntity, pDtBusiness.Date, m_MarketsWithoutIntraDayExeAssActivated));
                }
                trades.AddRange(physicalsettlements);

                IEnumerable<int> allMarkets = m_Markets.Union(m_MarketsWithoutIntraDayExeAssActivated);
                if (allMarkets.Count() > 0)
                {
                    // Utilisation de pDtBusinessNext et non de pDtBusiness afin de prendre les trades jusque la date business suivante (pour le calcul de la current exposure)
                    // PM 20190418 [24628] Ajout gestion acteur sans trade : ajout pDtBusiness
                    //tradeValues = LoadTradeValue(pAppInstance, connection, pIdEntity, allMarkets, pDtBusinessNext);
                    tradeValues = LoadTradeValue(session.AppInstance, connection, pIdEntity, allMarkets, pDtBusiness, pDtBusinessNext);

                    // PM 20221212 [XXXXX] Ajout chargement des trades sans calcul de déposit
                    tradeNoMargin = LoadTradeNoMargin(session.AppInstance, connection, pIdEntity, allMarkets, pDtBusiness);
                }
            }

            if (trades.Count > 0)
            {
                m_EvaluationData.PositionETD.BuildPosition(pIdEntity, trades);
                m_EvaluationData.PositionETD.BuildAssetETDCache(pCS);
                m_EvaluationData.PositionETD.InsertImAssetEtd(pCS,  session.BuildTableId());
            }

            if (tradeValues.Count > 0)
            {
                m_EvaluationData.TradeValueCOM.BuildTradeValueByDate(tradeValues, pDtBusiness, pDtBusinessNext);
                // PM 20200910 [25481] Ajout construction du cache d'Asset CommodityContract
                m_EvaluationData.TradeValueCOM.BuildAssetCache(pCS);
            }

            if (tradeNoMargin.Count > 0)
            {
                m_EvaluationData.TradeNoMargin.BuildTradeNoMargin(tradeNoMargin);
                m_EvaluationData.TradeNoMargin.BuildAssetCache(pCS);
            }
        }

        /// <summary>
        /// Effacer les données des objets en mémoire
        /// </summary>
        public void Clear()
        {
            m_EvaluationData.Clear();
        }

        /// <summary>
        /// Supprimer les données propres à la session
        /// </summary>
        /// <param name="pCS">Connection string</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée
        public void ClearSessionData(string pCS)
        {
            m_EvaluationData.ClearSessionData(pCS);
        }

        #region Private Methods
        /// <summary>
        /// Chargement des trades en positions
        /// </summary>
        /// <param name="pAppInstance"></param>
        /// <param name="pCS"></param>
        /// <param name="pConnection"></param>
        /// <param name="pId"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pInvariantDtBusiness"></param>
        /// <param name="pMarketsId"></param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        // EG 20190318 Upd tr.DTTIMESTAMP
        private List<TradeAllocation> LoadTradesActions(AppInstance pAppInstance, string pCS, IDbConnection pConnection, int pId, DateTime pDtBusiness, DateTime pInvariantDtBusiness, int[] pMarketsId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "DTBUSINESS", pDtBusiness },
                { "DTBUSINESSINVARIANT", pInvariantDtBusiness },
                { "IDENTITY", pId }
            };

            object[][] valuesMatrix =
                {
                    new object[] { GetQueryPositionActionBySide(pCS, EfsML.Enum.BuyerSellerEnum.BUYER, m_SettlSessID, true) },
                    new object[] { GetQueryPositionActionBySide(pCS, EfsML.Enum.BuyerSellerEnum.SELLER, m_SettlSessID, true) },
                    pMarketsId.Select(m => (object)m).ToArray(),
                    m_SettlSessID == SettlSessIDEnum.EndOfDay ? new object [] {""} : new object [] { "and tr.DTTIMESTAMP <= @DTBUSINESSINVARIANT" },
                };

            CommandType tradeAllocationsCommandType = DataContractHelper.GetType(DataContractResultSets.TRADEALLOCATIONSACTIONSWITH);

            string tradeAllocationsRequest = DataContractHelper.GetQuery(DataContractResultSets.TRADEALLOCATIONSACTIONSWITH, valuesMatrix);
            tradeAllocationsRequest = DataHelper<TradeAllocation>.IsNullTransform(pConnection, tradeAllocationsCommandType, tradeAllocationsRequest);

            List<TradeAllocation> trades = DataHelper<TradeAllocation>.ExecuteDataSet(pConnection, tradeAllocationsCommandType, tradeAllocationsRequest,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.TRADEALLOCATIONSACTIONSWITH, parameters));

            #region Log
            ////////////////////////////////////////////////////////////////
            if (m_LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                
                //SerializationHelper.DumpObjectToFile<List<TradeAllocation>>(trades, "LOG-01063", pAppInstance.AppRootFolder, "Trades.xml", null, m_Log);
                SerializationHelper.DumpObjectToFile<List<TradeAllocation>>(trades, new SysMsgCode(SysCodeEnum.LOG, 1063), pAppInstance.AppRootFolder, "Trades.xml", null,  m_AddCriticalException);
            }
            ////////////////////////////////////////////////////////////////
            #endregion Log

            return trades;
        }

        // EG 20180803 PERF New
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        private string GetQueryPositionActionBySide(string pCS, BuyerSellerEnum pBuyerSeller, SettlSessIDEnum pSettlSess, bool pIntraDayExeAssCtrlActivated)
        {
            string sqlSelect = @"select pad.IDT_{0} as IDT, sum(isnull(pad.QTY,0)) as QTY_{0}, 0 as QTY_{1}  
            from TRADEALLOC tr 
            inner join dbo.POSACTIONDET pad on (pad.IDT_{0} = tr.IDT)
            inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))" + Cst.CrLf;

            // 1 get the pos actions for a specific datetime...

            // 1.1 IntraDay including exe/ass for the given @DTPOS ( @DTPOS, with or without hours and minutes)
            if (pSettlSess == SettlSessIDEnum.Intraday && !pIntraDayExeAssCtrlActivated)
            {
                sqlSelect += @"where ({2} <= @DTBUSINESS)";
            }
            // 1.2 IntraDay excluding exe/ass for the given @DTPOS ( @DTPOS, with or without hours and minutes)
            else if (pSettlSess == SettlSessIDEnum.Intraday && pIntraDayExeAssCtrlActivated)
            {
                sqlSelect += @"inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)" + Cst.CrLf;
                sqlSelect += @"where 
                (pr.REQUESTTYPE not in ('ABN', 'NEX', 'NAS', 'ASS', 'EXE', 'MOF', 'AUTOABN', 'AUTOASS', 'AUTOEXE') and ({2} <= @DTBUSINESSINVARIANT))
                or 
                (pr.REQUESTTYPE in ('ABN', 'NEX', 'NAS', 'ASS', 'EXE', 'MOF', 'AUTOABN', 'AUTOASS', 'AUTOEXE') and ({2} <= @DTBUSINESS))";
            }
            // 1.3 EndOfDay
            else
            {
                // EG 20150107 Replace {1} by pa.DTBUSINESS
                sqlSelect += @"where (pa.DTBUSINESS <= @DTBUSINESS)";
            }

            sqlSelect = String.Format(sqlSelect + Cst.CrLf + "group by pad.IDT_{0}",
                (pBuyerSeller == BuyerSellerEnum.BUYER ? "BUY" : "SELL"),
                (pBuyerSeller == BuyerSellerEnum.BUYER ? "SELL" : "BUY"),
                DataHelper.SQLAddTime(pCS, "pa.DTBUSINESS", "pad.DTINS"));

            return sqlSelect;

        }

        /// <summary>
        /// Chargement des trades en positions en attente de livraison
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAppInstance"></param>
        /// <param name="pConnection"></param>
        /// <param name="pId"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pMarketsId"></param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        private List<TradeAllocation> LoadPhysicalSettlements(string pCS, AppInstance pAppInstance, IDbConnection pConnection, int pId, DateTime pDtBusiness, int[] pMarketsId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "DTBUSINESS", pDtBusiness },
                { "IDENTITY", pId }
            };

            object[][] valuesMatrix =
                {
                    pMarketsId.Select(m => (object)m).ToArray(),
                    m_SettlSessID == SettlSessIDEnum.EndOfDay ?
                        new object [] {""} :
                        new object [] { "and alloc.DTTRADE <= @DTBUSINESS" },
                    m_SettlSessID == SettlSessIDEnum.EndOfDay ?
                        new object [] {"and ecPhy.DTEVENT <= @DTBUSINESS"} :
                        new object [] { String.Format("and ({0} <= @DTBUSINESS)", DataHelper.SQLAddTime(pCS, "ecPhy.DTEVENT", "e.DTSTACTIVATION")) },
                };

            CommandType physicalSettlementsCommandType = DataContractHelper.GetType(DataContractResultSets.PHYSICALSETTLEMENTS);
            string physicalSettlementsRequest = DataContractHelper.GetQuery(DataContractResultSets.PHYSICALSETTLEMENTS, valuesMatrix);

            List<TradeAllocation> physicalsettlements = DataHelper<TradeAllocation>.ExecuteDataSet(
                pConnection,
                physicalSettlementsCommandType,
                physicalSettlementsRequest,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.PHYSICALSETTLEMENTS, parameters));

            // Livraison physique
            parameters.Add("MAXDELIVERYDAY", 30);

            CommandType physicalDeliveryCommandType = DataContractHelper.GetType(DataContractResultSets.PHYSICALDELIVERY);
            string physicalDeliveryRequest = DataContractHelper.GetQuery(DataContractResultSets.PHYSICALDELIVERY, valuesMatrix);

            List<TradeAllocation> physicalDelivery = DataHelper<TradeAllocation>.ExecuteDataSet(
                pConnection,
                physicalDeliveryCommandType,
                physicalDeliveryRequest,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.PHYSICALDELIVERY, parameters));

            #region Log
            ////////////////////////////////////////////////////////////////
            if (m_LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                
                //SerializationHelper.DumpObjectToFile<List<TradeAllocation>>(physicalsettlements, "LOG-01065", pAppInstance.AppRootFolder, "PhysicalSettlements.xml", null, m_Log);
                SerializationHelper.DumpObjectToFile<List<TradeAllocation>>(physicalsettlements,
                    new SysMsgCode(SysCodeEnum.LOG, 1065), pAppInstance.AppRootFolder, "PhysicalSettlements.xml", null,  m_AddCriticalException);
            }
            ////////////////////////////////////////////////////////////////
            #endregion Log

            return physicalsettlements.Concat(physicalDelivery).ToList();
        }

        /// <summary>
        /// Chargement des valeurs des trades
        /// </summary>
        /// <param name="pAppInstance"></param>
        /// <param name="pConnection"></param>
        /// <param name="pIdEntity"></param>
        /// <param name="pIdM"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtBusinessNext"></param>
        /// <returns></returns>
        // PM 20190418 [24628] Ajout gestion acteur sans trade
        //private List<TradeValue> LoadTradeValue(AppInstance pAppInstance, IDbConnection pConnection, int pIdEntity, IEnumerable<int> pIdM, DateTime pDtBusiness)
        private List<TradeValue> LoadTradeValue(AppInstance pAppInstance, IDbConnection pConnection, int pIdEntity, IEnumerable<int> pIdM, DateTime pDtBusiness, DateTime pDtBusinessNext)
        {
            // Date de début de recherche des données : dans un premier temps on prend en dur la date business moins 250 jours.
            // TODO : rendre dynamique la plage de date
            // 253 : 250 valeurs +3 au cas ou la date la plus éloignée serait un lundi
            // PM 20190206 [24482][24513] L'historique de valeur utilisé dans le calcul IMSM est de 250 jours Business, hors ici on limité l'historique à 253 jours calendaires
            // On prends maintenant l'historique sur une année calendaire
            //DateTime dtBusinessFirst = pDtBusiness.AddDays(-253);
            DateTime dtBusinessFirst = pDtBusiness.AddDays(-365);
            //
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
            {
                { "IDENTITY", pIdEntity },
                { "IDM", -1 },
                { "DTBUSINESS", pDtBusinessNext },
                { "DTBUSINESSFIRST", dtBusinessFirst }
            };

            // PM 20190418 [24628] Ajout gestion acteur sans trade
            Dictionary<string, object> dbParametersAgreement = new Dictionary<string, object>
            {
                { "IDENTITY", pIdEntity },
                { "IDM", -1 },
                { "DTBUSINESS", pDtBusiness }
            };

            List<TradeValue> tradeValues = new List<TradeValue>();
            foreach (int idM in pIdM)
            {
                dbParametersValue["IDM"] = idM;
                List<TradeValue> trades = LoadParametersMethod<TradeValue>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.TRADEVALUE);
                tradeValues.AddRange(trades);

                // PM 20190418 [24628] Ajout gestion acteur sans trade
                dbParametersAgreement["IDM"] = idM;
                trades = LoadParametersMethod<TradeValue>.LoadParameters(pConnection, dbParametersAgreement, DataContractResultSets.TRADEVALUEAGREEMENT);
                tradeValues.AddRange(trades);
            }

            #region Log
            ////////////////////////////////////////////////////////////////
            if (m_LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                
                //SerializationHelper.DumpObjectToFile<List<TradeValue>>(tradeValues, "LOG-01063", pAppInstance.AppRootFolder, "TradeValues.xml", null, m_Log);
                SerializationHelper.DumpObjectToFile<List<TradeValue>>(tradeValues, new SysMsgCode(SysCodeEnum.LOG, 1063),
                    pAppInstance.AppRootFolder, "TradeValues.xml", null,  m_AddCriticalException);
            }
            ////////////////////////////////////////////////////////////////
            #endregion Log

            return tradeValues;
        }

        /// <summary>
        /// Chargement des informations sur les trades sans calcul de déposit
        /// </summary>
        /// <param name="pAppInstance"></param>
        /// <param name="pConnection"></param>
        /// <param name="pIdEntity"></param>
        /// <param name="pIdM"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        // PM 20221212 [XXXXX] Ajout
        private List<TradeNoMargin> LoadTradeNoMargin(AppInstance pAppInstance, IDbConnection pConnection, int pIdEntity, IEnumerable<int> pIdM, DateTime pDtBusiness)
        {
            //
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>
            {
                { "IDENTITY", pIdEntity },
                { "IDM", -1 },
                { "DTBUSINESS", pDtBusiness }
            };

            List<TradeNoMargin> tradeNoMargin = new List<TradeNoMargin>();
            foreach (int idM in pIdM)
            {
                dbParametersValue["IDM"] = idM;
                List<TradeNoMargin> trades = LoadParametersMethod<TradeNoMargin>.LoadParameters(pConnection, dbParametersValue, DataContractResultSets.NOINITIALMARGINTRADE);
                tradeNoMargin.AddRange(trades);
            }

            #region Log
            ////////////////////////////////////////////////////////////////
            if (m_LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<List<TradeNoMargin>>(tradeNoMargin, new SysMsgCode(SysCodeEnum.LOG, 1063),
                    pAppInstance.AppRootFolder, "TradeNoMrgin.xml", null,  m_AddCriticalException);
            }
            ////////////////////////////////////////////////////////////////
            #endregion Log

            return tradeNoMargin;
        }
        #endregion Private Methods
        #endregion Methods
    }
}

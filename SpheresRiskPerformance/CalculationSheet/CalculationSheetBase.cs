using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.SpheresRiskPerformance.Properties;
using EFS.SpheresRiskPerformance.SpheresObjects;
using EFS.TradeInformation;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Doc;
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
using FixML.v50SP1;
using FixML.v50SP1.Enum;
//
using FpML.Interface;
using FpML.v44.Shared;
//
using Tz = EFS.TimeZone;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// key that identifies one single calculation sheet/result element
    /// </summary>
    public class MarginDetailsDocumentKey
    {
        /// <summary>
        /// Return an empty key, for serialization purpose only
        /// </summary>
        public MarginDetailsDocumentKey()
        { }

        /// <summary>
        /// Return a valid key
        /// </summary>
        /// <param name="pIdA">dealer</param>
        /// <param name="pIdB">book of the dealer</param>
        /// <param name="pIdCSS">clearing house</param>
        public MarginDetailsDocumentKey(int pIdA, int pIdB, int pIdCSS)
        {
            this.IdA = pIdA;

            this.IdB = pIdB;

            this.IdCSS = pIdCSS;
        }

        /// <summary>
        /// dealer
        /// </summary>
        public int IdA
        {
            get;
            set;
        }

        /// <summary>
        /// book of the dealer
        /// </summary>
        public int IdB
        {
            get;
            set;
        }

        /// <summary>
        /// clearing house
        /// </summary>
        public int IdCSS
        {
            get;
            set;
        }
    }

    /// <summary>
    /// IEqualityComparer implementation for the MarginDetailsDocumentKey class
    /// </summary>
    internal class MarginDetailsDocumentKeyComparer : IEqualityComparer<MarginDetailsDocumentKey>
    {

        #region IEqualityComparer<MarginDetailsDocumentKey> Membres

        /// <summary>
        /// Check the equality of two log keys
        /// </summary>
        /// <param name="x">first key log to be compared</param>
        /// <param name="y">second key log to be compared</param>
        /// <returns>true when the provided log keys are equal</returns>
        public bool Equals(MarginDetailsDocumentKey x, MarginDetailsDocumentKey y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return
                x.IdA.Equals(y.IdA) &&
                x.IdB.Equals(y.IdB) &&
                x.IdCSS.Equals(y.IdCSS);
        }

        /// <summary>
        /// Get the hashing code of the input log key
        /// </summary>
        /// <param name="obj">input log key we want ot compute the hashing code</param>
        /// <returns></returns>
        public int GetHashCode(MarginDetailsDocumentKey obj)
        {
            if (obj is null) return 0;

            int hashIdA = obj.IdA.GetHashCode();
            int hashIdB = obj.IdB.GetHashCode();
            int hashIdCSS = obj.IdCSS.GetHashCode();

            return hashIdA ^ hashIdB ^ hashIdCSS;
        }

        #endregion
    }

    /// <summary>
    /// CalculationSheet repository, containing the results and the calculation details of a deposit
    /// </summary>
    // EG 20190114 Add detail to ProcessLog Refactoring
    public sealed partial class CalculationSheetRepository
    {
        #region Members
        /// <summary>
        /// Connection String
        /// </summary>
        // PM 20200102 [XXXXX] New Log (m_Cs afin de remplacer ProcessLog.Cs)
        private string m_Cs;

        SerializableDictionary<MarginDetailsDocumentKey, MarginDetailsDocument> m_CalculationSheets
            = new SerializableDictionary<MarginDetailsDocumentKey, MarginDetailsDocument>(
                "DealerId_BookId_CSSId", "CalculationSheet", new MarginDetailsDocumentKeyComparer());

        /// <summary>
        /// Class helper to fill the calculation sheet with common trade elements (parties, markets repository, asset repository, etcetera...)
        /// </summary>
        DataDocumentContainer m_DataDocumentContainer = null;

        /// <summary>
        /// Actor sql elements cache used to build party elements
        /// </summary>
        readonly Dictionary<int, SQL_Actor> m_ActorCache = new Dictionary<int, SQL_Actor>();

        /// <summary>
        /// Actor sql elements cache
        /// </summary>
        readonly Dictionary<int, SQL_Book> m_BookCache = new Dictionary<int, SQL_Book>();

        

        /// <summary>
        /// Attaching delegate method
        /// </summary>
        AddAttachedDoc m_Attach = null;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        SetErrorWarning m_SetErrorWarning = null;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        AddCriticalException m_AddException = null;


        /// <summary>
        /// Main process informations
        /// </summary>
        RiskPerformanceProcessInfo m_ProcessInfo;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Results of the risk evaluation (including details). the dictionary key is composed by the pair {actor id, css id}. for each pair
        /// a single calculation sheet/result is provided. the calculation sheet/result can have multiple currencies.
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public SerializableDictionary<MarginDetailsDocumentKey, MarginDetailsDocument> CalculationSheets
        {
            get { return m_CalculationSheets; }
            set { m_CalculationSheets = value; }
        }

        /// <summary>
        /// Service info
        /// </summary>
        [XmlIgnore]
        public AppInstanceService AppInstance
        { get => AppSession.AppInstance as AppInstanceService; }


        /// <summary>
        /// Service info
        /// </summary>
        [XmlIgnore]
        public AppSession AppSession
        { get; set; }


        /// <summary>
        /// log level detail
        /// </summary>
        [XmlIgnore]
        public LogLevelDetail LogDetailEnum
        { get; set; }

        /// <summary>
        /// EFS licence
        /// </summary>
        [XmlIgnore]
        public EFS.Common.License License { get; set; }


        /// <summary>
        /// Asset ETD sql elements cache
        /// </summary>
        /// <remarks>this collection is defined externally</remarks>
        [XmlIgnore]
        internal Dictionary<int, SQL_AssetETD> AssetETDCache
        {
            get;
            set;
        }

        /// <summary>
        /// Main process informations
        /// </summary>
        internal RiskPerformanceProcessInfo ProcessInfo
        {
            get { return m_ProcessInfo; }
            set { m_ProcessInfo = value; }
        }


        /// <summary>
        /// Loggin delegate method, using datas array
        /// </summary>
        // EG 20180205 [23769] New
        internal AddAttachedDoc Attach
        {
            get { return m_Attach; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        internal SetErrorWarning SetErrorWarning
        {
            get { return m_SetErrorWarning; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        internal AddCriticalException AddException
        {
            get { return m_AddException; }
        }


/// Class helper to fill the calculation sheet with common trade elements
/// </summary>
//PM 20141216 [9700] Eurex Prisma for Eurosys Futures : Add DataDocumentContainer accessor
[XmlIgnore]
        internal DataDocumentContainer DataDocumentContainer
        {
            get { return m_DataDocumentContainer; }
            set { m_DataDocumentContainer = value; }
        }

        /// <summary>
        /// Connection String
        /// </summary>
        [XmlIgnore]
        public string Cs
        {
            get { return m_Cs; }
            set { m_Cs = value; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// New empty repository instance
        /// </summary>
        public CalculationSheetRepository() {}
        #endregion Constructor

        
        /// <summary>
        /// Add an empty calculation sheet object to the repository
        /// </summary>
        /// <param name="pCS">connection string to the DB</param>
        /// <param name="pIdActor">actor affected by the risk evaluation</param>
        /// <param name="pIdBook">book affected by the risk evaluation process</param>
        /// <param name="pIdCss">clearing house internal id</param>
        /// <param name="pIsGrossMargining">gross margin flag</param>
        /// <param name="pIdTechTrade">result Id of the risk evaluation</param>
        /// <param name="pTechTrade">risk evaluation result (AddCalculationSheet.pIdTechTrade)</param>
        /// <param name="pAmounts">deposit amounts list (all the currencies)</param>
        /// <param name="pWeightingRatio">book weighting ratio</param>
        /// <returns>true when the calculation sheet has been correctly added, 
        /// false when the calculation sheet can not be built because the trade is null</returns>
        // EG 20180205 [23769] Add  pIdentifierTechTrade
        public bool AddCalculationSheet(
            string pCS, int pIdActor, int pIdBook, int pIdCss, bool pIsGrossMargining,
            int pIdTechTrade, string pIdentifierTechTrade, ITrade pTechTrade, List<Money> pAmounts, decimal pWeightingRatio)
        {
            if (pTechTrade == null)
                return false;

            bool added = false;

            MarginDetailsDocumentKey key = new MarginDetailsDocumentKey(pIdActor, pIdBook, pIdCss);

            if (!CalculationSheets.ContainsKey(key))
            {
                MarginDetailsDocument calculationsheet = new MarginDetailsDocument(pIdTechTrade, pIdentifierTechTrade, pTechTrade, null);

                CalculationSheets.Add(key, calculationsheet);

                SimplePayment firstPayment = ((MarginRequirement)pTechTrade.Product).payment.FirstOrDefault();

                string payerHRef = firstPayment.payerPartyReference.href;
                string receiverHRef = firstPayment.receiverPartyReference.href;

                // Process margin req office, set main attributes
                SetMarginRequirement(
                    pCS, pIdActor, pIdBook, pIsGrossMargining, payerHRef, receiverHRef, pAmounts, pWeightingRatio,
                    calculationsheet.marginRequirementOffice);

                added = true;
            }

            return added;
        }

        /// <summary>
        /// Process a prevously added calculation sheet element, adding all the calculation sheet details, 
        /// which have been used by the risk evaluation process to compute the deposit result.
        /// </summary>
        /// <param name="pIdActor">actor affected by the risk evaluation process</param>
        /// <param name="pIdBook">book affected by the risk evaluation process</param>
        /// <param name="pIdCss">clearing house, clearer of the deposit</param>
        /// <param name="pDtBusiness">position business date</param>
        /// <param name="pCS">connection string to the DB</param>
        /// <param name="pTiming">user defined risk evaluation timing</param>
        /// <param name="pRiskElementClass">risk element type</param>
        /// <param name="pIdActorsBooksConstitutingPosition">all the actors/books constituting the position, including the current actor</param>
        /// <param name="pCalculationElements">all the positions (owned adn inherited) to evaluate the deposit of the current actor</param>
        /// <param name="pAmounts">deposit evaluated amounts (multiple currencies)</param>
        /// <param name="pMarginCalculationMethods">data container parameters used to build the MarginCalculatioMethod node of the report</param>
        /// <returns>true when the calculation sheet is well processed, 
        /// false if the calculation sheet has been already processed</returns>
        /// PM 20160404 [22116] Plusieurs InitialMarginMethodEnum possible : pMethodType devient un array
        /// PM 20170313 [22833] Changement de type de pIdActorsBooksConstitutingPosition : Pair<int, int>[] => RiskActorBook[]
        //public bool ProcessCalculationSheet(string pCS,
        //    int pIdActor, int pIdBook, int pIdCss,
        //    DateTime pDtBusiness, SettlSessIDEnum pTiming, bool pIsGrossMargining, RiskElementClass pRiskElementClass,
        //    Pair<int, int>[] pIdActorsBooksConstitutingPosition, IEnumerable<RiskElement> pCalculationElements, List<Money> pAmounts,
        //    InitialMarginMethodEnum[] pMethodType, IMarginCalculationMethodCommunicationObject[] pMarginCalculationMethods)
        public bool ProcessCalculationSheet(string pCS, int pIdActor, int pIdBook, int pIdCss, DateTime pDtBusiness, SettlSessIDEnum pTiming,  
            RiskActorBook[] pIdActorsBooksConstitutingPosition, IEnumerable<RiskElement> pCalculationElements, List<Money> pAmounts,
            IMarginCalculationMethodCommunicationObject[] pMarginCalculationMethods)
        {
            bool process = false;

            MarginDetailsDocumentKey key = new MarginDetailsDocumentKey(pIdActor, pIdBook, pIdCss);

            if (CalculationSheets.ContainsKey(key))
            {
                MarginDetailsDocument log = CalculationSheets[key];

                if (!log.Processed)
                {
                    // 1. Create the DataDocument helper to add parties  to the risk log

                    m_DataDocumentContainer = new DataDocumentContainer(log);

                    // 2. Add the parties

                    // PM 20170313 [22833] Changement de type de pIdActorsBooksConstitutingPosition : Pair<int, int>[] => RiskActorBook[]
                    //int[] pIdActorsConstitutingPosition = (
                    //    from actorId in pIdActorsBooksConstitutingPosition
                    //    select actorId.First
                    //    ).Distinct().ToArray();
                    int[] pIdActorsConstitutingPosition = (
                        from actorId in pIdActorsBooksConstitutingPosition
                        select actorId.IdA
                        ).Distinct().ToArray();

                    AddParties(pCS, pIdActorsConstitutingPosition);

                    // 3. Positions and evaluation parameters

                    log.marginRequirementOffice.marginCalculation =
                        new MarginCalculation(
                            log.marginRequirementOffice.isGross);

                    if (log.marginRequirementOffice.isGross)
                    {
                        if (Settings.Default.PositionExtendedReport)
                        {
                            // PM 20170313 [22833] Gestion des nouveaux types pour la position
                            //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions =
                            //    PositionsExtractor.GetPositions(pCalculationElements, pIdActor);
                            IEnumerable<RiskData> allRiskData = RiskElement.GetRiskData(pCalculationElements, pIdActor);
                            RiskData riskdata = RiskData.Aggregate(allRiskData);
                            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = riskdata.PositionETD.GetPositionAsEnumerablePair();

                            log.marginRequirementOffice.marginCalculation.positions = BuildPositionReport(pDtBusiness, pTiming, false, positions, null);
                        }

                        SetGrossMargin(pIdActor, pIdCss, pIdActorsBooksConstitutingPosition, ref log.marginRequirementOffice.marginCalculation.grossMargin);
                    }
                    else
                    // IsNetMargining
                    {

                        // PM 20170313 [22833] Gestion position et tradevalue
                        //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = null;
                        RiskData riskdata = default;

                        if (Settings.Default.PositionExtendedReport)
                        {
                            // PM 20170313 [22833] Gestion position et tradevalue
                            //positions = PositionsExtractor.GetPositions(pCalculationElements);
                            riskdata = RiskElement.AggregateRiskData(pCalculationElements);
                        }

                        //SetNetMargin(
                        //    pCS, pIdActor, pDtBusiness, pTiming, positions, pAmounts,
                        //    pMethodType, pMarginCalculationMethods, log.marginRequirementOffice.marginCalculation.netMargin); 
                        SetNetMargin(pDtBusiness, pTiming, riskdata, pAmounts, pMarginCalculationMethods, log.marginRequirementOffice.marginCalculation.netMargin);
                    }

                    process = log.Processed = true;
                }
            }

            return process;
        }

        /// <summary>
        /// Reset the  collection
        /// </summary>
        public void Reset()
        {
            CalculationSheets.Clear();

            // Do not clear the actor cache.. m_ActorCache ... or m_BookCache ...
            //  those collections could persist until the repository will be collected from the GC.
        }

        /// <summary>
        /// Init log method
        /// </summary>
        /// <param name="pAttach">delegate reference to add attachement</param>
        /// <param name="pSetErrorWarning"></param>
        /// <param name="pAddException"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void InitDelegate(AddAttachedDoc pAttach, SetErrorWarning pSetErrorWarning, AddCriticalException pAddException)
        {
            this.m_Attach = pAttach;
            this.m_SetErrorWarning = pSetErrorWarning;
            this.m_AddException = pAddException;
        }

        private void AddParties(string pCS, int[] pIdActorsConstitutingPosition)
        {
            foreach (int idParty in pIdActorsConstitutingPosition)
            {
                SQL_Actor actor = GetSQLActorFromCache(pCS, idParty);

                m_DataDocumentContainer.AddParty(actor);
            }
        }

        // EG 20180205 [23769] Use function with lock
        private SQL_Actor GetSQLActorFromCache(string pCS, int pIdParty)
        {
            SQL_Actor actor = null;
            lock (m_ActorCache)
            {
                if (!m_ActorCache.ContainsKey(pIdParty))
                {
                    actor = new SQL_Actor(pCS, pIdParty);

                    m_ActorCache.Add(pIdParty, actor);
                }
                else
                {
                    actor = m_ActorCache[pIdParty];
                }
            }

            return actor;
        }

        // EG 20180205 [23769] Use function with lock
        private SQL_Book GetSQLBookFromCache(string pCS, int pIdBook)
        {
            SQL_Book book = null;
            lock (m_BookCache)
            {
                if (!m_BookCache.ContainsKey(pIdBook))
                {
                    book = new SQL_Book(pCS, pIdBook);

                    m_BookCache.Add(pIdBook, book);
                }
                else
                {
                    book = m_BookCache[pIdBook];
                }

            }
            return book;
        }

        private BookId GetBookId(string pCS, int pIdBookAffected)
        {

            BookId bookId = new BookId();

            if (pIdBookAffected > 0)
            {
                SQL_Book book = GetSQLBookFromCache(pCS, pIdBookAffected);

                bookId.Value = book.Identifier;
                bookId.OTCmlId = book.Id;
                bookId.bookName = book.DisplayName;
                bookId.bookNameSpecified = true;
                bookId.bookIdScheme = Cst.OTCml_BookIdScheme;
            }
            else
            {
                bookId.Value = Cst.NotAvailable;
            }

            return bookId;
        }

        private void SetMarginRequirement(string pCS,
            int pIdActor, int pIdBook, bool pIsGrossMargining, string pPayerHRef, string pReceiverHRef, 
            List<Money> pAmounts, decimal pWeightingRatio,
            MarginRequirementOffice office)
        {
            office.isGross = pIsGrossMargining;

            SQL_Actor dealer = GetSQLActorFromCache(pCS, pIdActor);

            office.href = dealer.Identifier;

            office.payerPartyReference = new PartyReference(pPayerHRef);

            _ = GetSQLActorFromCache(pCS, m_ProcessInfo.Entity);

            office.receiverPartyReference = new PartyReference(pReceiverHRef);

            int idBookAffected = pIdBook;

            office.bookId = GetBookId(pCS, idBookAffected);

            office.weightingRatio = pWeightingRatio;

            office.marginAmounts = pAmounts.ToArray();
        }

        private static void SetMarginRequirement(MarginRequirementOffice source, MarginRequirementOffice dest)
        {
            dest.bookId = source.bookId;
            dest.href = source.href;
            dest.isGross = source.isGross;
            dest.payerPartyReference = source.payerPartyReference;
            dest.receiverPartyReference = source.receiverPartyReference;
            dest.marginAmounts = source.marginAmounts;
            dest.weightingRatio = source.weightingRatio;
        }

        /// <summary>
        /// Initialize one child calculation sheet
        /// </summary>
        /// <param name="pIdActor">current actor, owning the in processing calculation sheet</param>
        /// <param name="pIdBook">current book</param>
        /// <param name="pIdActorsBooksConstitutingPosition">all the actors/books participating to the current actor position, 
        /// including the current actor</param>
        /// <param name="pIdCss">clearing house</param>
        /// <param name="pGrossMargin">gross margin element to update</param>
        /// PM 20170313 [22833] Changement de type de pIdActorsBooksConstitutingPosition : Pair<int, int>[] => RiskActorBook[]
        //private void SetGrossMargin( int pIdActor, int pIdBook, int pIdCss, Pair<int, int>[] pIdActorsBooksConstitutingPosition, ref MarginRequirementOffice[] pGrossMargin)
        private void SetGrossMargin(int pIdActor, int pIdCss, RiskActorBook[] pIdActorsBooksConstitutingPosition, ref MarginRequirementOffice[] pGrossMargin)
        {

            IEnumerable<MarginDetailsDocument> childLogs =
                from actorbookIdConstitutingPosition
                    in pIdActorsBooksConstitutingPosition
                //where actorbookIdConstitutingPosition.First != pIdActor
                where (actorbookIdConstitutingPosition.IdA != pIdActor)
                join log
                    in m_CalculationSheets
                        on
                        //new { IdA = actorbookIdConstitutingPosition.First, IdB = actorbookIdConstitutingPosition.Second, IdCSS = pIdCss } equals
                        new { actorbookIdConstitutingPosition.IdA, actorbookIdConstitutingPosition.IdB, IdCSS = pIdCss } equals
                        new { log.Key.IdA, log.Key.IdB, log.Key.IdCSS }
                select log.Value;

            pGrossMargin = new MarginRequirementOffice[childLogs.ToArray().Length];

            int idxMarginRequirementOffice = 0;

            foreach (MarginDetailsDocument childLog in childLogs)
            {
                MarginRequirementOffice marginRequirementOfficeCopy = new MarginRequirementOffice();

                SetMarginRequirement(childLog.marginRequirementOffice, marginRequirementOfficeCopy);

                pGrossMargin[idxMarginRequirementOffice] = marginRequirementOfficeCopy;

                idxMarginRequirementOffice++;
            }

        }

        /// <summary>
        /// Adding position to the net margin node
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTiming">user defined riskl evaluation timing</param>
        /// <param name="pIdActor">current actor, owning the in processing log</param>
        /// <param name="pDtBusiness">positions business date</param>
        /// <param name="pRiskData">Risk data to be added</param>
        /// <param name="pAmounts">deposit evaluated amounts (multiple currencies)</param>
        /// <param name="pMethodType">method type</param>
        /// <param name="pMarginCalculationMethods">data container parameters used to build the MarginCalculatioMethod node of the report</param>
        /// <param name="pNetMargin">net margin element to update, must be NOT null</param>
        /// PM 20160404 [22116] Plusieurs InitialMarginMethodEnum possible : pMethodType devient un array
        /// PM 20170313 [22833] Gestion position et tradevalue : positions => pRiskData
        //private void SetNetMargin(
        //    string pCS, int pIdActor, DateTime pDtBusiness, SettlSessIDEnum pTiming,
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions, List<Money> pAmounts,
        //    InitialMarginMethodEnum[] pMethodType, IMarginCalculationMethodCommunicationObject[] pMarginCalculationMethods, NetMargin pNetMargin)
        private void SetNetMargin(DateTime pDtBusiness, SettlSessIDEnum pTiming, RiskData pRiskData, List<Money> pAmounts,
            IMarginCalculationMethodCommunicationObject[] pMarginCalculationMethods, NetMargin pNetMargin)
        {
            pNetMargin.marginAmounts = pAmounts.ToArray();

            // PM 20160404 [22116] Gestion plusieurs méthodes
            // PM 20170313 [22833] Le log de chaque méthode n'est maintenant plus forcement alimenté (cas de pas de données à évaluer pour une des méthodes)
            //int methodNum;
            //for (methodNum = 0; methodNum < pMethodType.Length; methodNum += 1)
            // PM 20200910 [25481] Gestion plusieurs méthodes y compris sur commodity
            //int methodCount = System.Math.Min(pMethodType.Length, pMarginCalculationMethods.Count());
            //for (int methodNum = 0; methodNum < methodCount; methodNum += 1)
            //    {
            //    MarginCalculationMethod[] marginCalculation = MarginCalculationMethodFactory(pMethodType[methodNum], pMarginCalculationMethods[methodNum]);
            int methodCount = pMarginCalculationMethods.Count();
            for (int methodNum = 0; methodNum < methodCount; methodNum += 1)
            {
                MarginCalculationMethod[] marginCalculation = MarginCalculationMethodFactory(pMarginCalculationMethods[methodNum]);
                    // PM 20170313 [22833] Ajout test sur marginCalculation
                    if (marginCalculation != default(MarginCalculationMethod[]))
                {
                    if (pNetMargin.marginCalculationMethod != null)
                    {
                        pNetMargin.marginCalculationMethod = pNetMargin.marginCalculationMethod.Concat(marginCalculation).ToArray();
                    }
                    else
                    {
                        pNetMargin.marginCalculationMethod = marginCalculation;
                    }
                }
            }
            //pNetMargin.marginCalculationMethod = MarginCalculationMethodFactory(pMethodType, pMarginCalculationMethods[0]);
            //pNetMargin.marginCalculationMethod = MarginCalculationMethodFactory(pMethodType[0], pMarginCalculationMethods[0]);
            // PM 20151116 [21561] Intégré dans MarginCalculationMethodFactory
            //pNetMargin.marginCalculationMethod.marginAmounts = pMarginCalculationMethods[0].MarginAmounts.Cast<Money>().ToArray();

            // PM 20170313 [22833] Modification de la recherche du log pour la DeliveryMarginCalculationMethod
            //if (pMarginCalculationMethods[methodNum] is DeliveryMarginCalculationMethodCommunicationObject)
            //{
            //    pNetMargin.deliveryCalculationMethod =
            //        BuildDeliveryMarginCalculationMethod(pMarginCalculationMethods[methodNum] as DeliveryMarginCalculationMethodCommunicationObject);
            //    pNetMargin.deliveryCalculationMethod.marginAmounts = pMarginCalculationMethods[methodNum].MarginAmounts.Cast<Money>().ToArray();
            //}
            DeliveryMarginCalculationMethodCommunicationObject deliveryCalculationMethod = (DeliveryMarginCalculationMethodCommunicationObject)
                pMarginCalculationMethods.Where(m => m is DeliveryMarginCalculationMethodCommunicationObject).FirstOrDefault();
            if (deliveryCalculationMethod != default(DeliveryMarginCalculationMethodCommunicationObject))
            {
                pNetMargin.deliveryCalculationMethod = BuildDeliveryMarginCalculationMethod(deliveryCalculationMethod);
                pNetMargin.deliveryCalculationMethod.marginAmounts = deliveryCalculationMethod.MarginAmounts.Cast<Money>().ToArray();
            }
            if ((Settings.Default.PositionExtendedReport) && (default(RiskData) != pRiskData))
            {
                // PM 20170313 [22833] Ajout lecture de la position
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = pRiskData.GetPosition();
                pNetMargin.positions = BuildPositionReport(pDtBusiness, pTiming, false, positions, null);
            }
        }

        /// <summary>
        /// Alimente la partie instrmt de {positionReport} 
        /// </summary>
        /// <param name="keyPos"></param>
        /// <param name="positionReport"></param>
        /// FI 20130317 [17861] Modification de la signature, Alimentation de positionReport.ContractType
        private void SetInstrumentBlock(PosRiskMarginKey keyPos, FixPositionReport positionReport)
        {
            SQL_AssetETD asset = this.AssetETDCache[keyPos.idAsset];

            positionReport.ContractTypeSpecified = true;
            positionReport.ContractType = asset.DrvContract_ContractType;

            InstrumentBlock instrmt = positionReport.Instrmt;

            // PM 20161019 [22174] Ne renseigner les AID que si réellement présent
            //instrmt.AID = new SecAltIDGrp_Block[] { new SecAltIDGrp_Block(), new SecAltIDGrp_Block() };

            //instrmt.AID[0].AltIDSrc = SecurityIDSourceEnum.ISIN;
            //instrmt.AID[0].AltIDSrcSpecified = true;
            //instrmt.AID[0].AltID = asset.ISINCode;
            //instrmt.AID[0].AltIDSpecified = true;

            //instrmt.AID[1].AltIDSrc = SecurityIDSourceEnum.ExchangeSymbol;
            //instrmt.AID[1].AltIDSrcSpecified = true;
            //instrmt.AID[1].AltID = asset.AssetSymbol;
            //instrmt.AID[1].AltIDSpecified = true;
            List<SecAltIDGrp_Block> instrmtAIDList = new List<SecAltIDGrp_Block>();
            if (StrFunc.IsFilled(asset.ISINCode))
            {
                SecAltIDGrp_Block instrmtAID = new SecAltIDGrp_Block
                {
                    AltIDSrc = SecurityIDSourceEnum.ISIN,
                    AltIDSrcSpecified = true,
                    AltID = asset.ISINCode,
                    AltIDSpecified = true
                };
                //
                instrmtAIDList.Add(instrmtAID);
            }
            if (StrFunc.IsFilled(asset.AssetSymbol))
            {
                SecAltIDGrp_Block instrmtAID = new SecAltIDGrp_Block
                {
                    AltIDSrc = SecurityIDSourceEnum.ExchangeSymbol,
                    AltIDSrcSpecified = true,
                    AltID = asset.AssetSymbol,
                    AltIDSpecified = true
                };
                //
                instrmtAIDList.Add(instrmtAID);
            }
            instrmt.AID = instrmtAIDList.ToArray();

            instrmt.Src = SecurityIDSourceEnum.Proprietary;
            instrmt.SrcSpecified = true;

            instrmt.MMY = asset.Maturity_MaturityMonthYear;
            instrmt.MMYSpecified = true;

            instrmt.Sym = asset.DrvContract_Symbol;
            instrmt.SymSpecified = true;

            if (StrFunc.IsFilled(asset.DrvContract_Attribute))
            {
                instrmt.OptAt = asset.DrvContract_Attribute;
                instrmt.OptAtSpecified = true;
            }

            instrmt.ID = asset.Identifier;
            instrmt.IDSpecified = true;

            instrmt.Exch = asset.Market_Identifier;
            instrmt.ExchSpecified = true;

            if (asset.PutCall != String.Empty)
            {
                instrmt.StrkPx = asset.StrikePrice;
                instrmt.StrkPxSpecified = true;

                instrmt.PutCall = (FixML.Enum.PutOrCallEnum)System.Enum.Parse(typeof(FixML.Enum.PutOrCallEnum), asset.PutCall);
                instrmt.PutCallSpecified = true;
            }

            // PM 20170515 [] Ajout maturity date
            if (asset.Maturity_MaturityDate != default)
            {
                instrmt.MatDt = DtFunc.DateTimeToStringyyyyMMdd(asset.Maturity_MaturityDate);
                instrmt.MatDtSpecified = true;
            }
        }

        private void SetParty_Block(string pCS, int pIdActor, Parties_Block pPty)
        {
            SQL_Actor actor = GetSQLActorFromCache(pCS, pIdActor);

            pPty.ID = actor.Identifier;
            pPty.IDSpecified = true;
            pPty.R = PartyRoleEnum.BuyerSellerReceiverDeliverer;
            pPty.RSpecified = true;
            pPty.Src = PartyIDSourceEnum.PropCode;
            pPty.SrcSpecified = true;
        }

        /// <summary>
        /// Build positions report
        /// </summary>
        /// <param name="pTiming">current risk evaluation timing</param>
        /// <param name="pHideInstrument">when true, just the quantity node will be serialized into the position report</param>
        /// <param name="pDtBusiness">current business date</param>
        /// <param name="pPositions">positions to be serialized</param>
        /// <param name="pStocksCoverage">all the stocks used to covered the given positions set</param>
        /// <param name="pShortOptionCompensationCommObjs">
        /// short options quantities, not empty when the short option compensation (cross margining for a margin class) has been used
        /// </param>
        private FixPositionReport[] BuildPositionReport(DateTime pDtBusiness, SettlSessIDEnum pTiming, bool pHideInstrument,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions,
            IEnumerable<StockCoverageCommunicationObject> pStocksCoverage,
            IEnumerable<ShortOptionCompensationCommunicationObject> pShortOptionCompensationCommObjs)
        {
            FixPositionReport[] positionReports = new FixPositionReport[pPositions.Count()];

            int idxPosition = 0;

            // PM 20161019 [22174] Ajout tri des positions
            // RD 20161121 [22624] Ajout test AssetETDCache.ContainsKey(p.First.idAsset)
            //IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> sortedPositions = pPositions
            //    .OrderBy(p => AssetETDCache[p.First.idAsset].StrikePrice)
            //    .OrderBy(p => AssetETDCache[p.First.idAsset].PutCall)
            //    .OrderBy(p => AssetETDCache[p.First.idAsset].Maturity_MaturityMonthYear)
            //    .OrderBy(p => AssetETDCache[p.First.idAsset].DrvContract_Symbol)
            //    .OrderBy(p => AssetETDCache[p.First.idAsset].Market_Identifier);

            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> sortedPositions = pPositions
                .OrderBy(p => (AssetETDCache.ContainsKey(p.First.idAsset) ? AssetETDCache[p.First.idAsset].StrikePrice : 0))
                .OrderBy(p => (AssetETDCache.ContainsKey(p.First.idAsset) ? AssetETDCache[p.First.idAsset].PutCall : string.Empty))
                .OrderBy(p => (AssetETDCache.ContainsKey(p.First.idAsset) ? AssetETDCache[p.First.idAsset].Maturity_MaturityMonthYear : string.Empty))
                .OrderBy(p => (AssetETDCache.ContainsKey(p.First.idAsset) ? AssetETDCache[p.First.idAsset].DrvContract_Symbol : string.Empty))
                .OrderBy(p => (AssetETDCache.ContainsKey(p.First.idAsset) ? AssetETDCache[p.First.idAsset].Market_Identifier : string.Empty));

            //foreach (Pair<PosRiskMarginKey, RiskMarginPosition> position in pPositions)
            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> position in sortedPositions)
            {

                PosRiskMarginKey keyPos = position.First;
                RiskMarginPosition valuePos = position.Second;

                FixPositionReport reportPosition = new FixPositionReport();

                if (pDtBusiness > DateTime.MinValue)
                {
                    reportPosition.BizDt = pDtBusiness;
                    reportPosition.BizDtSpecified = true;

                    // FI 20171025 [23533] Appel à ConvertEnumToString
                    //reportPosition.SetSesID = ReflectionTools.GetCustomAttributeValue<XmlEnumAttribute>
                    //    (typeof(SettlSessIDEnum), Enum.GetName(typeof(SettlSessIDEnum), pTiming), true);
                    reportPosition.SetSesID =
                        ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pTiming);

                    reportPosition.SetSesIDSpecified = true;
                }

                if (keyPos.idB_Dealer > 0)
                {
                    
                    //SQL_Book book = GetSQLBookFromCache(this.ProcessLog.Cs, keyPos.idB_Dealer);
                    SQL_Book book = GetSQLBookFromCache(m_Cs, keyPos.idB_Dealer);
                    reportPosition.Acct = book.Identifier;
                    reportPosition.AcctSpecified = true;
                }

                if (keyPos.idA_Dealer > 0)
                {
                    
                    //SetParty_Block(this.ProcessLog.Cs, keyPos.idA_Dealer, reportPosition.Pty);
                    SetParty_Block(m_Cs, keyPos.idA_Dealer, reportPosition.Pty);
                    reportPosition.PtySpecified = true;
                }

                if (!pHideInstrument)
                {
                    SetInstrumentBlock(keyPos, reportPosition);
                    reportPosition.InstrmtSpecified = true;
                }

                // PM 20130325 Ajout gestion de plusieurs positions action couvrant une position dérivée
                //StockCoverageCommunicationObject stockCoverage =
                //    pStocksCoverage != null ?
                //        pStocksCoverage.FirstOrDefault(key => key.AssetId == position.First.idAsset)
                //        :
                //        null;
                StockCoverageCommunicationObject stockCoverage = null;
                if (pStocksCoverage != null)
                {
                    stockCoverage = (
                        from stocks in pStocksCoverage
                        where stocks.AssetId == position.First.idAsset
                        group stocks by new { stocks.AssetId, stocks.ContractId } into stocksCov
                        select new StockCoverageCommunicationObject
                        {
                            AssetId = stocksCov.Key.AssetId,
                            ContractId = stocksCov.Key.ContractId,
                            Order = stocksCov.FirstOrDefault().Order,
                            Quantity = stocksCov.Sum(s=>s.Quantity),
                        }).FirstOrDefault();
                }

                ShortOptionCompensationCommunicationObject shortOptionCompensation =
                    pShortOptionCompensationCommObjs?.FirstOrDefault(key => key.AssetId == position.First.idAsset);

                SetQuantity(keyPos, valuePos, stockCoverage, shortOptionCompensation, reportPosition);

                positionReports[idxPosition] = reportPosition;

                idxPosition++;
            }

            return positionReports;
        }

        /// <summary>
        /// Build positions report
        /// </summary>
        /// <param name="pTiming">current risk evaluation timing</param>
        /// <param name="pHideInstrument">when true, just the quantity node will be serialized into the position report</param>
        /// <param name="pDtBusiness">current business date</param>
        /// <param name="pPositions">positions to be serialized</param>
        /// <param name="pStocksCoverage">all the stocks used to covered the given positions set</param>
        private FixPositionReport[] BuildPositionReport(DateTime pDtBusiness, SettlSessIDEnum pTiming, bool pHideInstrument,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions,
            IEnumerable<StockCoverageCommunicationObject> pStocksCoverage)
        {
            //PM 20141218 [9700] Eurex Prisma for Eurosys Futures : Gestion différente de la construction du report de la position
            if (m_ProcessInfo.SoftwareRequester == Software.SOFTWARE_EurosysFutures)
            {
                return BuildExternalPositionReport(pDtBusiness, pTiming, pPositions);
            }
            else
            {
                return BuildPositionReport(pDtBusiness, pTiming, pHideInstrument, pPositions, pStocksCoverage, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKeyPos"></param>
        /// <param name="pValuePos"></param>
        /// <param name="pStockCoverage"></param>
        /// <param name="pShortOptionCompensation"></param>
        /// <param name="reportPosition"></param>
        //PM 20141218 [9700] Eurex Prisma for Eurosys Futures : la méthode SetQuantity passe de private à public
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void SetQuantity(
            PosRiskMarginKey pKeyPos, RiskMarginPosition pValuePos, 
            StockCoverageCommunicationObject pStockCoverage, 
            ShortOptionCompensationCommunicationObject pShortOptionCompensation,
            FixPositionReport reportPosition)
        {
            
            if (pKeyPos.FixSide == SideEnum.Buy)
            {
                if (pValuePos.Quantity > 0)
                {
                    reportPosition.Qty = new FixQuantity
                    {
                        Long = pValuePos.Quantity
                    };
                }

                if (pStockCoverage != null)
                {
                    reportPosition.CoveredQty = new FixQuantity
                    {
                        Typ = PosType.PA,
                        Long = pStockCoverage.Quantity,
                        DescrTyp = String.Format("StockOrder:{0}", pStockCoverage.Order)
                    };
                }

            }
            else if (pKeyPos.FixSide == SideEnum.Sell)
            {
                if (pValuePos.Quantity > 0)
                {
                    reportPosition.Qty = new FixQuantity
                    {
                        Short = pValuePos.Quantity
                    };
                }

                if (pStockCoverage != null)
                {
                    reportPosition.CoveredQty = new FixQuantity
                    {
                        Typ = PosType.PA,
                        Short = pStockCoverage.Quantity,
                        DescrTyp = String.Format("StockOrder:{0}", pStockCoverage.Order)
                    };
                }

                // short option compensation can be not null just for side 2, short
                if (pShortOptionCompensation != null)
                {
                    reportPosition.CompensationQty = new FixQuantity
                    {
                        Typ = PosType.XM,
                        Short = pShortOptionCompensation.Quantity
                    };
                }

            }
            else
            {
                string message = String.Format(Ressource.GetString("RiskPerformance_WARNINGWrongFixSide"), pKeyPos.idA_Dealer, reportPosition.RptID);
                Logger.Log(new LoggerData(LogLevelEnum.Error, message));
            }

            // PM 20120305 
            // MF 20120326 : log exercised/assigned/in delivery quantity, when the ExeAssQuantity is different than 0
            // PM 20130910 [17949] Livraison : Ajout pValuePos.DeliveryQuantity
            if ((pValuePos.ExeAssQuantity != 0) || (pValuePos.DeliveryQuantity !=0))
            {
                // PM 20120305 : Ajout gestion des MOF
                if (AssetETDCache.TryGetValue(pKeyPos.idAsset, out SQL_AssetETD asset))
                {
                    reportPosition.ExeAssQty = new FixQuantity();

                    if (asset.DrvContract_IsFuture)
                    {
                        reportPosition.ExeAssQty.Typ = PosType.DN;
                    }
                    else
                    {
                        if (pValuePos.ExeAssQuantity > 0)
                        {
                            reportPosition.ExeAssQty.Typ = PosType.AS;
                        }
                        else
                        {
                            reportPosition.ExeAssQty.Typ = PosType.EX;
                        }
                    }
                    // PM 20130910 [17949] Livraison : Ajout pValuePos.DeliveryQuantity
                    if (pValuePos.DeliveryQuantity > 0)
                    {
                        reportPosition.ExeAssQty.Long = System.Math.Abs(pValuePos.DeliveryQuantity);
                    }
                    else if (pValuePos.DeliveryQuantity < 0)
                    {
                        reportPosition.ExeAssQty.Short = System.Math.Abs(pValuePos.DeliveryQuantity);
                    }
                    else
                    {
                        reportPosition.ExeAssQty.Long = System.Math.Abs(pValuePos.ExeAssQuantity);
                    }

                    reportPosition.ExeAssQty.QuantityDate = pValuePos.DeliveryDate ?? default;
                }
            }

        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private DeliveryMarginCalculationMethod BuildDeliveryMarginCalculationMethod(DeliveryMarginCalculationMethodCommunicationObject pMethodCommObj)
        {
            DeliveryMarginCalculationMethod method = new DeliveryMarginCalculationMethod
            {
                Name = "Delivery"
            };

            // PM 20181002 [XXXXX] Ajout vérification des données reçues en paramètres
            if ((pMethodCommObj != default(DeliveryMarginCalculationMethodCommunicationObject)) && (pMethodCommObj.Parameters != default(IRiskParameterCommunicationObject[])))
            {
                method.Parameters = new DeliveryParameter[pMethodCommObj.Parameters.Length];

                int idxParameter = 0;

                foreach (IRiskParameterCommunicationObject parameterCommObj in pMethodCommObj.Parameters)
                {
                    DeliveryParameterCommunicationObject deliveryParameterCommObj =
                        (DeliveryParameterCommunicationObject)parameterCommObj;

                    DeliveryParameter deliveryParameter = new DeliveryParameter();

                    method.Parameters[idxParameter] = deliveryParameter;
                    deliveryParameter.OTCmlId = deliveryParameterCommObj.AssetId;
                    deliveryParameter.Identifier = this.AssetETDCache[deliveryParameterCommObj.AssetId].Identifier;
                    deliveryParameter.ContractSymbol = this.AssetETDCache[deliveryParameterCommObj.AssetId].DrvContract_Symbol;

                    deliveryParameter.marginExpressionType =
                        System.Enum.GetName(typeof(ExpressionType), deliveryParameterCommObj.ExpressionType);
                    // PM 20130911 [17949] Livraison
                    deliveryParameter.deliveryStep =
                        System.Enum.GetName(typeof(InitialMarginDeliveryStepEnum), deliveryParameterCommObj.DeliveryStep);

                    deliveryParameter.amount = deliveryParameterCommObj.DeliveryValue;

                    if (deliveryParameterCommObj.ExpressionType == ExpressionType.Percentage)
                    {
                        deliveryParameter.contractSize = deliveryParameterCommObj.Size;
                        deliveryParameter.contractSizeSpecified = true;

                        if (!deliveryParameterCommObj.Missing)
                        {
                            deliveryParameter.quote = deliveryParameterCommObj.Quote;
                            deliveryParameter.quoteSpecified = true;
                        }
                        else
                        {
                            deliveryParameter.quoteMissing = true;
                            deliveryParameter.quoteMissingSpecified = true;

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            Logger.Log(new LoggerData(LogLevelEnum.Error,
                                (deliveryParameterCommObj.UnderlyerCategory.Value == Cst.UnderlyingAsset.Future) ? deliveryParameterCommObj.ErrorCodeFuture : deliveryParameterCommObj.ErrorCode,
                                0,
                                new LogParam(this.AssetETDCache[deliveryParameter.OTCmlId].DrvContract_AssetCategorie),
                                new LogParam(String.Format(
                                    "Asset: {0} - Timing: {1} - ",
                                    deliveryParameter.Identifier,
                                    deliveryParameterCommObj.QuoteTiming)),
                                new LogParam(String.Format(
                                    "Time: {0} - Adj. Time: {1} - ",
                                    deliveryParameterCommObj.SettlementDate.Value, 
                                    deliveryParameterCommObj.AdjustedTime.Value)),
                                new LogParam(String.Format(
                                    "Env: {0} - Scen: {1} - Side: {2}",
                                    deliveryParameterCommObj.IdMarketEnv,
                                    deliveryParameterCommObj.IdValScenario,
                                    deliveryParameterCommObj.QuoteSide)),
                                // RD 20140414 [19815] Utiliser le Cst.OTCml_TradeIdScheme pour récupérer l'id du trade, et non pas le premier Id trouvé qui peut être un UTI, un id du trader, ....
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));

                            if (null != deliveryParameterCommObj.SystemMsgInfo)
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, deliveryParameterCommObj.SystemMsgInfo.SysMsgCode, 0, deliveryParameterCommObj.SystemMsgInfo.LogParamDatas));
                            }
                        }
                    }
                    else
                    {
                        deliveryParameter.contractSizeSpecified = false;
                    }

                    deliveryParameter.marginAmount = MarginMoney.FromMoney(deliveryParameterCommObj.MarginAmount);

                    deliveryParameter.positions = BuildPositionReport
                        (DateTime.MinValue, SettlSessIDEnum.EndOfDay, false, deliveryParameterCommObj.Positions, null);

                    idxParameter++;
                }
            }
            return method;
        }
        /// <summary>
        /// Construction du log détail par type de méthode
        /// </summary>
        /// <param name="pCalculationMethod"></param>
        /// <returns></returns>
        // PM 20141216 [9700] Eurex Prisma for Eurosys Futures : la méthode MarginCalculationMethodFactory passe de private à public
        // PM 20151116 [21561] Retourne maintenant un tableau
        //public MarginCalculationMethod MarginCalculationMethodFactory(InitialMarginMethodEnum pMethodType, IMarginCalculationMethodCommunicationObject pCalculationMethod)
        // PM 20200910 [25481] Gestion plusieurs méthodes y compris sur commodity: suppresion paramètre pMethodType
        //public MarginCalculationMethod[] MarginCalculationMethodFactory(InitialMarginMethodEnum pMethodType, IMarginCalculationMethodCommunicationObject pCalculationMethod)
        public MarginCalculationMethod[] MarginCalculationMethodFactory(IMarginCalculationMethodCommunicationObject pCalculationMethod)
        {
            // PM 20151116 [21561] Ajout calculationMethods et subCalculationMethod
            MarginCalculationMethod[] calculationMethods = null;
            // PM 20170313 [22833] Ajout test sur pCalculationMethod
            if (pCalculationMethod != default)
            {
                MarginCalculationMethod calculationMethod = default;
                MarginCalculationMethod subCalculationMethod = null;

                // PM 20170313 [22833] Ajout test sur le type de l'objet pCalculationMethod
                //switch (pMethodType)
                switch (pCalculationMethod.MarginMethodType)
                {
                    case InitialMarginMethodEnum.Custom:
                        if (pCalculationMethod is CustomMarginCalculationMethodCommunicationObject custom)
                        {
                            calculationMethod = BuildCustomMarginCalculationMethod(custom);
                        }
                        break;
                    case InitialMarginMethodEnum.TIMS_IDEM:
                        if (pCalculationMethod is TimsIdemMarginCalculationMethodCommunicationObject timsidem)
                        {
                            calculationMethod = BuildTimsIdemMarginCalculationMethod(timsidem);
                        }
                        break;
                    case InitialMarginMethodEnum.TIMS_EUREX:
                        if (pCalculationMethod is TimsEurexMarginCalculationMethodCommunicationObject timseurex)
                        {
                            calculationMethod = BuildTimsEurexMarginCalculationMethod(timseurex);
                        }
                        break;
                    case InitialMarginMethodEnum.SPAN_C21:
                    case InitialMarginMethodEnum.SPAN_CME:
                    case InitialMarginMethodEnum.London_SPAN:
                        if (pCalculationMethod is SpanMarginCalcMethCom span)
                        {
                            calculationMethod = BuildSpanMarginCalculationMethod(span);
                        }
                        break;
                    case InitialMarginMethodEnum.CBOE_Margin:
                        if (pCalculationMethod is CboeMarginCalcMethCom cboe)
                        {
                            calculationMethod = BuildCBOEMarginCalculationMethod(cboe);
                        }
                        break;
                    case InitialMarginMethodEnum.MEFFCOM2:
                        if (pCalculationMethod is MeffCalcMethCom meff)
                        {
                            calculationMethod = BuildMEFFCOM2MarginCalculationMethod(meff);
                        }
                        break;
                    case InitialMarginMethodEnum.OMX_RCAR:
                        if (pCalculationMethod is OMXCalcMethCom omx)
                        {
                            calculationMethod = BuildOMXMarginCalculationMethod(omx);
                        }
                        break;
                    case InitialMarginMethodEnum.EUREX_PRISMA:
                        // PM 20151116 [21561] Ajout du log RBM
                        //calculationMethod = BuildPrismaMarginCalculationMethod((PrismaCalcMethCom)pCalculationMethod);
                        if (pCalculationMethod is PrismaCalcMethCom prisma)
                        {
                            calculationMethod = BuildPrismaMarginCalculationMethod(prisma);
                            // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
                            //if (prismaCalcMethCom.RbmMethComObj != default(TimsEurexMarginCalculationMethodCommunicationObject))
                            //{
                            //    subCalculationMethod = BuildTimsEurexMarginCalculationMethod(prismaCalcMethCom.RbmMethComObj);
                            //}
                        }
                        break;
                    // PM 20170313 [22833] Ajout du log de la méthode IMSM
                    case InitialMarginMethodEnum.IMSM:
                        if (pCalculationMethod is IMSMCalcMethCom imsm)
                        {
                            calculationMethod = BuildIMSMCalculationMethod(imsm);
                        }
                        break;
                    // PM 20221212 [XXXXX] Nouvelle méthode NOMARGIN
                    case InitialMarginMethodEnum.NOMARGIN:
                        if (pCalculationMethod is NoMarginCalcMethCom nomargin)
                        {
                            calculationMethod = BuildNoMarginCalculationMethod(nomargin);
                        }
                        break;
                    // PM 20190222 [24326] Nouvelle méthode NOMX_CFM
                    case InitialMarginMethodEnum.NOMX_CFM:
                        if (pCalculationMethod is NOMXCFMCalcMethCom nomx)
                        {
                            calculationMethod = BuildNOMXCFMCalculationMethod(nomx);
                        }
                        break;
                    // PM 20210518 [XXXXX] Ajout méthode SPAN 2, nouvelle méthode de CME Clearing
                    case InitialMarginMethodEnum.SPAN_2_CORE:
                    case InitialMarginMethodEnum.SPAN_2_SOFTWARE:
                        if (pCalculationMethod is SPAN2CalcMethCom span2)
                        {
                            calculationMethod = BuildSpan2MarginCalculationMethod(span2);
                        }
                        break;
                    // PM 20230612 [26091][WI390] Ajout méthode Euronext Var Based
                    // PM 20240423 [XXXXX] Ajout EURONEXT_LEGACY_VAR
                    case InitialMarginMethodEnum.EURONEXT_VAR_BASED:
                    case InitialMarginMethodEnum.EURONEXT_LEGACY_VAR:
                        if (pCalculationMethod is EuronextVarCalcMethCom euronextvar)
                        {
                            calculationMethod = BuildEuronextVarCalculationMethod(euronextvar);
                        }
                        break;
                    case InitialMarginMethodEnum.Delivery:
                        // Cas traité en dehors de la méthode
                        break;
                    default:
                        //throw new NotSupportedException(String.Format(Ressource.GetString("RiskPerformance_ERRUnknownMethod"), pMethodType));
                        throw new NotSupportedException(String.Format(Ressource.GetString("RiskPerformance_ERRUnknownMethod"), pCalculationMethod.MarginMethodType));
                }

                // PM 20151116 [21561] Changement de valeur de retour
                //return calculationMethod;
                if (calculationMethod != default(MarginCalculationMethod))
                {
                    calculationMethod.Version = pCalculationMethod.MethodVersion;
                    calculationMethod.VersionSpecified = true;

                    calculationMethod.marginAmounts = pCalculationMethod.MarginAmounts.Cast<Money>().ToArray();
                    if (subCalculationMethod != default(MarginCalculationMethod))
                    {
                        calculationMethods = new MarginCalculationMethod[] { calculationMethod, subCalculationMethod };
                    }
                    else
                    {
                        calculationMethods = new MarginCalculationMethod[] { calculationMethod };
                    }
                }
            }
            return calculationMethods;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataDoc"></param>
        /// <param name="pSqlActor"></param>
        /// <param name="pSqlBook"></param>
        /// <param name="pSqlEntity"></param>
        /// <param name="pSqlCss"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pMoney"></param>
        /// <param name="pRiskEvaluationTiming"></param>
        /// <param name="pDepositClearer"></param>
        /// <param name="pInitialMarginMethod">Méthode de calcul utilisée</param>
        /// <param name="pIsGrossMargining">Calcul en brut ou en net</param>
        /// PM 20140317 [17861] Ajout des paramètres pInitialMarginMethod et pIsGrossMargining (permet d'avoir des infos sur le calcul)
        /// PM 20160404 [22116] Plusieurs InitialMarginMethodEnum possible : pInitialMarginMethod devient un array
        /// FI 20160613 [22256] Add parameter pUnderlyingStock
        // EG 20171109 [23509] Upp Gestion DTORDERENTERED
        // EG 20180205 [23769] Public
        public void SetDataDocument(
            DataDocumentContainer pDataDoc, SQL_Actor pSqlActor, SQL_Book pSqlBook, SQL_Actor pSqlEntity, SQL_Actor pSqlCss,
            DateTime pDtBusiness, List<Money> pMoney, SettlSessIDEnum pRiskEvaluationTiming, bool pDepositClearer,
            InitialMarginMethodEnum[] pInitialMarginMethod, bool pIsGrossMargining, IEnumerable<UnderlyingStock> pUnderlyingStock)
        {

            // On recupère ORDERENTERED éventuel du trade si déjà créé
            Nullable<DateTimeOffset> dtOrderEntered = pDataDoc.GetOrderEnteredDateTimeOffset();

            //Suppression des parties
            pDataDoc.RemoveParty();

            //Ajout des parties
            IParty partyActor = pDataDoc.AddParty(pSqlActor);
            IPartyTradeIdentifier partyActorTradeIdentifier = pDataDoc.AddPartyTradeIndentifier(partyActor.Id);
            Tools.SetBookId(partyActorTradeIdentifier.BookId, pSqlBook);
            partyActorTradeIdentifier.BookIdSpecified = true;

            IParty partyEntity = pDataDoc.AddParty(pSqlEntity);
            if (null != partyEntity)
            {
                IPartyTradeInformation partyTradeInformationEntity = pDataDoc.AddPartyTradeInformation(partyEntity.Id);
                if (null != partyTradeInformationEntity)
                {
                    partyTradeInformationEntity.Timestamps = pDataDoc.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();
                    if (false == dtOrderEntered.HasValue)
                    {
                        DateTime dtSys = OTCmlHelper.GetDateSys(pSqlActor.CS);
                        dtOrderEntered = Tz.Tools.FromTimeZone(dtSys, Tz.Tools.UniversalTimeZone);
                    }
                    partyTradeInformationEntity.Timestamps.OrderEntered = Tz.Tools.ToString(dtOrderEntered);
                    partyTradeInformationEntity.Timestamps.OrderEnteredSpecified = true;
                    partyTradeInformationEntity.TimestampsSpecified = true;
                }
            }
            //
            //Chambre
            _ = pDataDoc.AddParty(pSqlCss);

            MarginRequirementContainer marginRequirement = new MarginRequirementContainer((IMarginRequirement)pDataDoc.CurrentProduct.Product);
            IProductBase productBase = (IProductBase)marginRequirement.MarginRequirement;

            // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness
            pDataDoc.SetClearedDate(pDtBusiness);
            pDataDoc.TradeHeader.TradeDate.Value = pDataDoc.TradeHeader.ClearedDate.Value;

            marginRequirement.Payment = null;
            marginRequirement.MarginRequirementOfficePartyReference = productBase.CreatePartyReference(pSqlActor.XmlId);
            marginRequirement.EntityPartyReference = productBase.CreatePartyReference(pSqlEntity.XmlId);
            marginRequirement.ClearingOrganizationPartyReference = productBase.CreatePartyReference(pSqlCss.XmlId);
            marginRequirement.Timing = pRiskEvaluationTiming;
            //PM 20140317 [17861] Gestion des paramètres pInitialMarginMethod et pIsGrossMargining
            marginRequirement.InitialMarginMethodSpecified = (false == pIsGrossMargining);
            marginRequirement.IsGrossMargin.BoolValue = pIsGrossMargining;
            marginRequirement.IsGrossMarginSpecified = true;
            if (marginRequirement.InitialMarginMethodSpecified)
            {
                // PM 20160404 [22116] initialMarginMethod devient un array
                marginRequirement.InitialMarginMethod = pInitialMarginMethod;
            }
            // FI 20160613 [22256] Alimentation de underlyingStockSpecified 
            marginRequirement.UnderlyingStockSpecified = (null != pUnderlyingStock) && (pUnderlyingStock.Count() > 0);
            if (marginRequirement.UnderlyingStockSpecified)
                marginRequirement.UnderlyingStock = pUnderlyingStock.ToArray();
            foreach (IMoney money in pMoney)
            {
                ISimplePayment simplePayment = productBase.CreateSimplePayment();

                if (!pDepositClearer)
                {
                    simplePayment.PayerPartyReference.HRef = partyActor.Id;
                    simplePayment.ReceiverPartyReference.HRef = partyEntity.Id;
                }
                else
                {
                    simplePayment.PayerPartyReference.HRef = partyEntity.Id;
                    simplePayment.ReceiverPartyReference.HRef = partyActor.Id;
                }
                simplePayment.PaymentAmount.Amount.DecValue =
                    // un crédit n'est pas à considérer
                    money.Amount.DecValue > 0 ?
                        money.Amount.DecValue
                        :
                        0;
                simplePayment.PaymentAmount.Currency = money.Currency;
                simplePayment.PaymentDate.AdjustableDateSpecified = true;
                simplePayment.PaymentDate.AdjustableDate = productBase.CreateAdjustableDate(pDtBusiness, FpML.Enum.BusinessDayConventionEnum.NotApplicable, null);
                //
                marginRequirement.AddPayment(simplePayment);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMode"></param>
        /// <returns></returns>
        internal static Cst.StatusEnvironment RiskEvaluationModeToStatusEnvironment(RiskEvaluationMode pMode)
        {
            Cst.StatusEnvironment ret = Cst.StatusEnvironment.REGULAR;
            if (pMode == RiskEvaluationMode.Normal)
                ret = Cst.StatusEnvironment.REGULAR;
            else if (pMode == RiskEvaluationMode.Simulation)
                ret = Cst.StatusEnvironment.SIMUL;
            return ret;
        }

    }

    /// <summary>
    /// Helper class used to load/write the calculation sheet (format XML) relative to a risk revaluation result 
    /// </summary>
    /// <remarks>
    /// <list type="">
    /// <listheader>Spheres DB affected objects</listheader>
    /// <item>table MARGINTRACK</item>
    /// <item>table MARGINDETTRACK</item>
    /// </list>
    /// </remarks>
    // EG 20190114 Add detail to ProcessLog Refactoring
    internal sealed class CalculationSheetWriter
    {

        DataTable m_tableTradeMarginTrack = null;

        DataTable m_tableMarginTrack = null;

        bool m_Initialized = false;

        /// <summary>
        /// Get the initialization state of the destination objects
        /// </summary>
        public bool Initialized
        {
            get { return m_Initialized; }
            set { m_Initialized = value; }
        }

        /// <summary>
        /// Service info
        /// </summary>
        [XmlIgnore]
        public AppInstanceService AppInstance
        { get => AppSession.AppInstance as AppInstanceService; }

        /// <summary>
        /// Service info
        /// </summary>
        [XmlIgnore]
        public AppSession  AppSession
        { get; set; }

        /// <summary>
        /// header process
        /// </summary>
         // PM 20200102 [XXXXX] New Log : supprimé
        //ProcessLogHeader m_ProcessHeader = null;

        /// <summary>
        /// Id de la table PROCESS_L
        /// </summary>
        // PM 20200102 [XXXXX] New Log : ajouté
        int m_IdProcessL;

        /// <summary>
        /// Connection string
        /// </summary>
        string m_CS = String.Empty;

        
        
        /// <summary>
        /// 
        /// </summary>
        SetErrorWarning m_SetErrorWarning = null;
        
        /// <summary>
        /// 
        /// </summary>
        AddCriticalException m_AddException = null;



        /// <summary>
        /// Time elapsed form the beginning
        /// </summary>
        string m_ElapsedTime;

        /// <summary>
        /// Create a new Log Writer
        /// </summary>
        public CalculationSheetWriter()
        {

        }

        /// <summary>
        /// Initialize the destination objects structure 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdProcessL"></param>
        /// <param name="pElapsedTime"></param>
        /// PM 20200102 [XXXXX] New Log : remplacement de ProcessLogHeader par pIdProcessL
        //public void Init(string pCS, ProcessLogHeader pProcessHeader, string pElapsedTime)
        public void Init(string pCS, int pIdProcessL, string pElapsedTime)
        {
            m_CS = pCS;
            
            CreateStructureMarginTrack();

            CreateStructureTradeMarginTrack();

            //m_ProcessHeader = pProcessHeader;
            m_IdProcessL = pIdProcessL;

            m_ElapsedTime = pElapsedTime;

            Initialized = true;
        }

        /// <summary>
        /// Inject the logs repository elements into the database
        /// </summary>
        /// <param name="opCount">output parameter, returns the number of injected elements</param>
        /// <param name="opIdMarginTrack">output parameter, returns the Id of the new element of the MARGINTRACK table</param>
        /// <param name="pRepository">the repository containinf the log elements to be injected</param>
        /// <returns>true when the injection did not raise any errors</returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public bool Write(CalculationSheetRepository pRepository, out int opCount, out int opIdMarginTrack)
        {
            bool res = false;

            opCount = 0;
            opIdMarginTrack = 0;

            try
            {
                
                IEnumerable<MarginDetailsDocument> calculationsheetsToWrite =
                    from log
                        in pRepository.CalculationSheets.Values
                    where log.Processed == true
                    select log;

                opIdMarginTrack = AddMarginTrackElement(pRepository.ProcessInfo, calculationsheetsToWrite.Count());

                opCount = 0;
 
                foreach (MarginDetailsDocument calculationsheet in calculationsheetsToWrite)
                {

                    switch (DataHelper.GetDbSvrType(this.m_CS))
                    {
                        case DbSvrType.dbORA:
                            {
                                System.Xml.XmlDocument xmlTrade = new System.Xml.XmlDocument();

                                Dictionary<string, object> parameters = CreateMarginDetTrackParameters(opIdMarginTrack, calculationsheet, xmlTrade);

                                string queryInsert = DataContractHelper.GetQuery(DataContractResultSets.INSERTTRADEMARGINTRACK);

                                opCount += DataHelper.ExecuteNonQueryXmlForOracle(
                                    DbSvrType.dbORA,
                                    this.m_CS,
                                    queryInsert,
                                    xmlTrade,
                                    "TRADEXML",
                                    DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTTRADEMARGINTRACK, parameters));

                                xmlTrade = null;
                            }

                            break;

                        case DbSvrType.dbSQL:
                        default:
                            {
                                DataRow newRow = CreateMarginDetTrackElement(opIdMarginTrack, calculationsheet);

                                m_tableTradeMarginTrack.Rows.Add(newRow);

                                // PM 20160922 Pour VS2013
                                //opCount +=
                                //    DataHelper.ExecuteDataAdapter
                                //    (this.m_CS, "select * from dbo.TRADEMARGINTRACK where 1 = 0",
                                //    m_tableTradeMarginTrack);
                                string querySelect = DataContractHelper.GetQuery(DataContractResultSets.SELECTTRADEMARGINTRACK);
                                opCount += DataHelper.ExecuteDataAdapter(this.m_CS, querySelect, m_tableTradeMarginTrack);                                

                                m_tableTradeMarginTrack.Clear();
                            }

                            break;
                    }

                }

                res = true;
                   
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                SpheresException2 otcmlEx = SpheresExceptionParser.GetSpheresException(Ressource.GetString("RiskPerformance_ERRNullTradeResultOrCalculationSheetExisting"), ex);
                // FI 20200623 [XXXXX] AddException
                m_AddException(otcmlEx);

                
                
                Logger.Log(new LoggerData(otcmlEx));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1018),0));
            }
            m_tableTradeMarginTrack.Rows.Clear();
            return res;
        }

        /// <summary>
        /// Init delegate method
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        /// <param name="pAddException"^></param>
        /// EG 20190114 Add detail to ProcessLog Refactoring
        public void InitDelegate(SetErrorWarning pSetErrorWarning, AddCriticalException pAddException)
        {
            m_SetErrorWarning = pSetErrorWarning;
            m_AddException = pAddException;
        }

        private void CreateStructureMarginTrack()
        {
            m_tableMarginTrack = DataHelper<MARGINTRACK>.BuildTableStructure();
        }

        private void CreateStructureTradeMarginTrack()
        {
            // PM 20160922 VS2013
            //m_tableTradeMarginTrack = DataHelper<TRADEMARGINTRACK>.BuildTableStructure();
            string sqlQuerySelect = DataContractHelper.GetQuery(DataContractResultSets.SELECTTRADEMARGINTRACK);
            DataSet ds = DataHelper.ExecuteDataset(m_CS, CommandType.Text, sqlQuerySelect);
            m_tableTradeMarginTrack = ds.Tables[0];

            //if (DataHelper.GetDbSvrType(this.m_CS) == DbSvrType.dbORA)
            //{
            //    DataColumn tradeXml = m_tableTradeMarginTrack.Columns["TRADEXML"];

            //    tradeXml.MaxLength = -1;

            //    tradeXml.ExtendedProperties.Add("OraDbType", 127);
            //    tradeXml.ExtendedProperties.Add("BaseColumn", "TRADEXML");
            //}
        }

        // EG 20210916 [XXXXX] Gestion de l'alimentation de MARGINTRACK avec UPDATE/INSERT
        // en fonction du contrôle d'existence IDPROCESS_L dans la table.
        private int AddMarginTrackElement(RiskPerformanceProcessInfo pProcessInfo, int pEvaluatedElements)
        {
            if (0 == GetIdMarginTrack(false))
            {
                DataRow newRow = m_tableMarginTrack.NewRow();
                // PM 20200102 [XXXXX] New Log : remplacement de m_ProcessHeader.IdProcess par m_IdProcessL
                newRow["IDPROCESS_L"] = m_IdProcessL;
                newRow["IDA_ENTITY"] = pProcessInfo.Entity;
                newRow["IDA_CSS"] = pProcessInfo.CssId;
                newRow["DTBUSINESS"] = pProcessInfo.DtBusiness;
                newRow["TIMING"] = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pProcessInfo.Timing);
                newRow["MESSAGE"] = String.Format(Ressource.GetString("RiskPerformance_LogProcessStatistics"), m_ElapsedTime, pEvaluatedElements);
                // FI 20200820 [25468] Dates systemes en UTC
                newRow["DTINS"] = OTCmlHelper.GetDateSysUTC(m_CS);
                newRow["IDAINS"] = this.AppSession.IdA;
                newRow["EXTLLINK"] = null;
                m_tableMarginTrack.Rows.Add(newRow);

                DataHelper.ExecuteDataAdapter(this.m_CS, "select * from dbo.MARGINTRACK where 1 = 0", m_tableMarginTrack);

                m_tableMarginTrack.Rows.Clear();
            }
            else
            {
                string sqlUpdate = @"update dbo.MARGINTRACK set MESSAGE = @MESSAGE, DTINS = @DTINS where (IDPROCESS_L = @IDPROCESS_L)";

                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(m_CS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(m_CS));
                parameters.Add(new DataParameter(m_CS, "MESSAGE", DbType.AnsiString, 128),
                    String.Format(Ressource.GetString("RiskPerformance_LogProcessStatistics"), m_ElapsedTime, pEvaluatedElements));
                parameters.Add(new DataParameter(m_CS, "IDPROCESS_L", DbType.Int32), m_IdProcessL);

                QueryParameters qry = new QueryParameters(m_CS, sqlUpdate, parameters);
                DataHelper.ExecuteNonQuery(m_CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

            }
            // retrieving last IDMARGINTRACK (relative to the current process)
            return GetIdMarginTrack(true);
        }

        // EG 20210916 [XXXXX] Nouveau paramètre pIsCatchException
        // -> lié à la gestion de l'alimentation de MARGINTRACK avec UPDATE/INSERT en fonction du contrôle d'existence IDPROCESS_L dans la table.
        private int GetIdMarginTrack(bool pIsCatchException)
        {
            int idMarginTrack = 0;

            using (IDbConnection connection = DataHelper.OpenConnection(m_CS))
            {

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                     // PM 20200102 [XXXXX] New Log : remplacement de m_ProcessHeader.IdProcess par m_IdProcessL
                    { "IDPROCESS_L", m_IdProcessL }
                };

                List<MARGINTRACK> elems = DataHelper<MARGINTRACK>.ExecuteDataSet(
                    connection,
                    DataContractHelper.GetType(DataContractResultSets.PROCESSMARGINTRACK),
                    DataContractHelper.GetQuery(DataContractResultSets.PROCESSMARGINTRACK),
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.PROCESSMARGINTRACK, parameters));

                if (1 == elems.Count)
                    idMarginTrack = elems[0].IdMarginTrack;
                else if (pIsCatchException)
                    throw new NotSupportedException(String.Format(@"{0} MARGINTRACK element(s) found for the current process. Writing log process aborted.", elems.Count));
            }

            return idMarginTrack;
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private DataRow CreateMarginDetTrackElement(int pIdMarginTrack, MarginDetailsDocument pDocument)
        {
            DataRow newRow = m_tableTradeMarginTrack.NewRow();

            newRow["IDMARGINTRACK"] = pIdMarginTrack;
            newRow["IDT"] = pDocument.IdTechTrade;
            // RD 20130902 [18921] Utilisation de la date du SGBD
            // FI 20200820 [25468] Dates systemes en UTC
            newRow["DTINS"] = OTCmlHelper.GetDateSysUTC(m_CS);
            newRow["IDAINS"] = this.AppSession.IdA;

            if (Settings.Default.UseOriginalCalculationSheetSerialization)
            {
                

                StringBuilder xmlstream = RiskPerformanceSerializationHelper.DumpObjectToString<MarginDetailsDocument>(pDocument,  m_SetErrorWarning, m_AddException);
                newRow["TRADEXML"] = xmlstream.ToString();
            }
            else
            {
                string xmlFile = RiskPerformanceSerializationHelper.DumpObjectToFile<MarginDetailsDocument>(
                    pDocument, String.Format("Log_{1}_Track{0}.xml", pDocument.party[0].id, pIdMarginTrack));
                
                byte[] data = FileTools.ReadFileToBytes(xmlFile);
                FileTools.FileDelete2(xmlFile);
                System.Text.Encoding encoding = new System.Text.UnicodeEncoding();
                // EG 20160404 Migration vs2013
                //newRow["TRADEXML"] = encoding.GetString(data);
                Byte[] byteOrderMark = encoding.GetPreamble();
                int substract = byteOrderMark.Length;
                // PM 20160922 Pour VS2013
                //newRow["TRADEXML"] = XElement.Parse(encoding.GetString(data, substract, data.Length - substract));
                newRow["TRADEXML"] = encoding.GetString(data, substract, data.Length - substract);
            }

            return newRow;
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private Dictionary<string, object> CreateMarginDetTrackParameters(int pIdMarginTrack, MarginDetailsDocument pDocument, System.Xml.XmlDocument pXmlTrade)
        {

            Dictionary<string, object> dbParameterValues = new Dictionary<string, object>
            {
                { "IDT", pDocument.IdTechTrade },
                { "IDMARGINTRACK", pIdMarginTrack },

                // RD 20130902 [18921] Utilisation de la date du SGBD
                // FI 20200820 [25468] Dates systèmes en UTC
                { "DTINS", OTCmlHelper.GetDateSysUTC(m_CS) },
                { "IDAINS", this.AppSession.IdA }
            };

            string xmlFile = RiskPerformanceSerializationHelper.DumpObjectToFile<MarginDetailsDocument>(pDocument, String.Format("Log_{1}_Track{0}.xml", pDocument.party[0].id, pIdMarginTrack));

            pXmlTrade.Load(xmlFile);

            FileTools.FileDelete2(xmlFile);
            
            return dbParameterValues;
        }
    }
}
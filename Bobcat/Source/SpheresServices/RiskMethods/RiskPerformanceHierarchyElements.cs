using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Spheres.Hierarchies;
using EFS.SpheresRiskPerformance.DataContracts;
using EFS.SpheresRiskPerformance.Enum;
//
using FixML.Enum;

namespace EFS.SpheresRiskPerformance.Hierarchies
{
    /// <summary>
    /// Extended RoleAttribute class containing all the parameters for the  MARGINREQOFFICE role
    /// </summary>
    [XmlRoot(ElementName = "RoleMarginReqOfficeSpecificParameters")]
    public class RoleMarginReqOfficeAttribute : RoleAttribute, IDataContractEnabled
    {
        private List<MarginReqOfficeBooksAndParameters> m_BooksWithParameters =
            new List<MarginReqOfficeBooksAndParameters>();

        /// <summary>
        /// Books list including main risk parameters (IMR book, gross/net calculation mode)
        /// </summary>
        [XmlArray(ElementName = "BooksWithParameters")]
        [XmlArrayItem(ElementName = "Book")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<MarginReqOfficeBooksAndParameters> BooksWithParameters
        {
            get { return m_BooksWithParameters; }
            set { m_BooksWithParameters = value; }
        }

        /// <summary>
        /// List of supplementary risk parameters per clearing house
        /// </summary>
        [XmlArray(ElementName = "ClearingOrgParameters")]
        [XmlArrayItem(ElementName = "Parameter")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<ClearingOrgParameter> ClearingOrgParameters
        {
            get;
            set;
        }

        /// <summary>
        /// List of supplementary risk parameters per market
        /// </summary>
        [XmlArray(ElementName = "EquityMarketParameters")]
        [XmlArrayItem(ElementName = "Parameter")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<EquityMarketParameter> EquityMarketParameters
        {
            get;
            set;
        }

        private int m_IMRBookId;

        /// <summary>
        /// IMR Flag
        /// </summary>
        [XmlAttribute(AttributeName = "imrbook")]
        public int IMRBookId
        {
            get { return m_IMRBookId; }
            set { m_IMRBookId = value; }
        }

        private List<TradeRisk> m_Results = new List<TradeRisk>();

        /// <summary>
        /// Existing previous risk evaluation results
        /// </summary>
        [XmlArray]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<TradeRisk> Results
        {
            get { return m_Results; }
            set { m_Results = value; }
        }

        private bool m_AffectAllBooks;

        /// <summary>
        /// Flag indicating when all the actor books more than the IMR book will be affected by the deposit evaluation
        /// </summary>
        [XmlAttribute(AttributeName = "isonallbooks")]
        public bool IsOnAllBooks
        {
            get { return m_AffectAllBooks; }
            set { m_AffectAllBooks = value; }
        }

        private bool m_IsGrossMargining;

        /// <summary>
        /// Position evaluation mode
        /// </summary>
        [XmlAttribute(AttributeName = "grossmargining")]
        public bool IsGrossMargining
        {
            get { return m_IsGrossMargining; }
            set { m_IsGrossMargining = value; }
        }

        /// <summary>
        /// Multiplier ratio affecting the final deposit amount
        /// </summary>
        /// <remarks>primary data source: MarginReqOfficeBooksAndParameters class</remarks>
        [XmlAttribute(AttributeName = "wratio")]
        public decimal WeightingRatio
        {
            get;

            set;
        }

        // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskDataActorBook
        //SerializableDictionary<int, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> m_Positions =
        //    new SerializableDictionary<int, List<Pair<PosRiskMarginKey, RiskMarginPosition>>>("ActorId", "PositionsByActor");
        private SerializableDictionary<int, RiskData> m_MRORiskData = new SerializableDictionary<int, RiskData>("ActorId", "RiskDataByActor");

        /// <summary>
        /// Positions set
        /// </summary>
        /// <remarks>
        /// collection containing all the positions owned by the current MARGINREQOFFICE actor 
        ///     and by its depending actors not having role MARGINREQOFFICE
        /// </remarks>
        [ReadOnly(true)]
        [Browsable(false)]
        public SerializableDictionary<int, RiskData> MRORiskData
        {
            get { return m_MRORiskData; }
            set { m_MRORiskData = value; }
        }

        private List<RiskElement> m_RootElements = new List<RiskElement>();

        /// <summary>
        /// Number of deposits to generate for the attribute owner 
        /// (including the positions set either of the attribute owner either of the transversal actors)
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public List<RiskElement> RootElements
        {
            get { return m_RootElements; }
            set { m_RootElements = value; }
        }

        private SerializableDictionary<Pair<int, int>, RiskElement> m_InheritedElements =
            new SerializableDictionary<Pair<int, int>, RiskElement>("ActorId_AffectedBookId", "Risks", new PairComparer<int, int>());

        /// <summary>
        /// Inherited elements, inherited from all the descendants of the current attribute owner 
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public SerializableDictionary<Pair<int, int>, RiskElement> InheritedElements
        {
            get { return m_InheritedElements; }
            set { m_InheritedElements = value; }
        }

        private readonly DateTime m_RoleEnabledFrom;

        private readonly DateTime m_RoleDisabledFrom;

        #region IDataContractEnabled Membres

        /// <summary>
        /// activation date of the the attribute, 
        ///  calculated either on the current BooksWithParameters activations either on the role activation date
        /// </summary>
        [XmlIgnore]
        public DateTime ElementEnabledFrom
        {
            get
            {
                IEnumerable<DateTime> enabledFrom =
                    new[] { this.m_RoleEnabledFrom }.Union(from bookMargin in BooksWithParameters select bookMargin.ElementEnabledFrom);

                return enabledFrom.Max();
            }
        }

        /// <summary>
        ///  deactivation date of the attribute, 
        ///  calculated either on the current BooksWithParameters deactivations either on the role deactivation date
        /// </summary>
        [XmlIgnore]
        public DateTime ElementDisabledFrom
        {
            get
            {
                IEnumerable<DateTime> disabledFrom =
                    new[] { this.m_RoleDisabledFrom }.Union(from bookMargin in BooksWithParameters select bookMargin.ElementDisabledFrom);

                return disabledFrom.Min();
            }
        }

        #endregion

        /// <summary>
        /// Empty attribute
        /// </summary>
        /// RD 20170420 [23092] Constructeur à supprimer
        // PM 20200129 Non, ne pas supprimer car constructeur necessaire à la serialisation en mode log Full 
        public RoleMarginReqOfficeAttribute()
        {
            this.ClearingOrgParameters = new List<ClearingOrgParameter>();

            this.EquityMarketParameters = new List<EquityMarketParameter>();
        }

        /// <summary>
        /// Empty identified attribute
        /// </summary>
        /// <param name="pIdNode">internal id of the actor owning the attribute</param>
        /// <param name="pIdParentNode">optional father node relative to the relationship base of the current attribute</param>
        /// <param name="pRoleEnabledFrom">Activation date of the role related to the attribute</param>
        /// <param name="pRoleDisabledFrom">Deactivation date of the role to the attribute</param>
        // RD 20170420 [23092] Modify        
        public RoleMarginReqOfficeAttribute(int pIdNode, int pIdParentNode, DateTime pRoleEnabledFrom, DateTime pRoleDisabledFrom) :
            base(pIdNode, pIdParentNode)
        {
            this.ClearingOrgParameters = new List<ClearingOrgParameter>();

            this.EquityMarketParameters = new List<EquityMarketParameter>();

            //this.IdNode = pIdNode;

            //this.IdParentNode = pIdParentNode;

            this.m_RoleEnabledFrom = pRoleEnabledFrom;

            this.m_RoleDisabledFrom = pRoleDisabledFrom;
        }

        #region ISpheresNodeAttribute

        /// <summary>
        /// Load the attribute datas
        /// </summary>
        /// <param name="pConnection">an open connection</param>
        /// <returns></returns>
        // RD 20170420 [23092] Modify        
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override bool BuildAttribute(IDbConnection pConnection)
        {
            //base.Built = base.BuildAttribute(pConnection);
            ResetAttribute();

            base.Built = false;

            string roleAttributeIdentifier = RolesCollection.ROLEMARGINREQOFFICE;

            Dictionary<string, object> parameterValues = new Dictionary<string, object>
            {
                { "IDA", this.IdNode },
                { "ROLE", roleAttributeIdentifier }
            };

            this.LoadSupplementaryRiskParameter(pConnection, parameterValues);


            BooksWithParameters = DataHelper<MarginReqOfficeBooksAndParameters>.ExecuteDataSet(
                pConnection,
                DataContractHelper.GetType(DataContractResultSets.MARGINREQOFFICEBOOKSANDPARAMETERS),
                DataContractHelper.GetQuery(DataContractResultSets.MARGINREQOFFICEBOOKSANDPARAMETERS),
                DataContractHelper.GetDbDataParameters(DataContractResultSets.MARGINREQOFFICEBOOKSANDPARAMETERS, parameterValues));

            if (BooksWithParameters != null)
            {
                BuildBooks();
                this.Built = SetMarginProperties();
            }

            if (!this.Built )
            {
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 1017), 0,
                    new LogParam(roleAttributeIdentifier),
                    new LogParam(IdNode)));
            }
            return this.Built;
        }


        /// <summary>
        /// Reset the attribute collection
        /// </summary>
        public override void ResetAttribute()
        {
            base.ResetAttribute();

            this.BooksWithParameters.Clear();

            // RD 20170420 [23092] Déplacement dans la classe "RoleAttribute"
            //this.Books.Clear();

            // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskRepository
            //this.Positions.Clear();
            MRORiskData.Clear();

            this.RootElements.Clear();

            this.InheritedElements.Clear();

            this.Results.Clear();
        }

        #endregion

        #region ICloneable Membres

        /// <summary>
        /// Build a new copy of the current hierarchy instance
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return SerializationHelper.Clone<RoleMarginReqOfficeAttribute>(this);
        }

        #endregion

        /// <summary>
        /// Fill the internal Results list (<seealso cref="Results"/>), using the risk margin trades found in the database
        /// </summary>
        /// <param name="pAllTradeRisk">All the Results present in the database</param>
        /// <returns>how many risk trades have been found</returns>
        // PM 20131010 [19046] Added pour optimiser le chargement des trades deposit existant
        // EG 20190114 Add detail to ProcessLog Refactoring
        public int LoadPreviousTradeRisks(List<TradeRisk> pAllTradeRisk)
        {
            this.Results.Clear();
            //
            var result = from tradeRisk in pAllTradeRisk
                         where tradeRisk.ActorId == IdNode
                         join book in this.Books on tradeRisk.BookId equals book.Id
                         select new
                         {
                             Results = tradeRisk,
                             Books = book,
                         };

            if (result.Count() > 0)
            {
                this.Results.AddRange(result.Select(elem => elem.Results).ToList());

                var log = from r in result
                          group r by r.Results.DateBusiness into resultDtBusiness
                          select new
                          {
                              DateBusiness = resultDtBusiness.Key,
                              Results = resultDtBusiness.Select(e => e.Results),
                              Books = resultDtBusiness.Select(e => e.Books),
                          };

                foreach (var dtResult in log)
                {
                    string tradeIdentifiers = result.Select(elem => elem.Results.Identifier).Aggregate((total, next) => String.Format("{0} {1}", total, next));
                    string bookNames = result.Select(elem => elem.Books.Identifier).Aggregate((total, next) => String.Format("{0} {1}", total, next));
                    string bookIds = result.Select(elem => elem.Books.Id.ToString("000000")).Aggregate((total, next) => String.Format("{0} {1}", total, next));
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1044), 4,
                        new LogParam(result.Count()),
                        new LogParam(tradeIdentifiers),
                        new LogParam(bookNames),
                        new LogParam(bookIds),
                        new LogParam(dtResult.DateBusiness.ToShortDateString())));
                }
            }
            return this.Results.Count;
        }

        /// <summary>
        /// Select all the actors/books contributintg to constitute the global position of the actor owning the attribute
        /// </summary>
        /// <param name="pRiskElementClass">kind of deposit we are interested to evaluate</param>
        /// <param name="pIdBook">affected book of the deposit, needed only when ADDITIONALDEPOSIT are requested</param>
        /// <returns>the array of actor/book pairs</returns>
        /// PM 20170313 [22833] Changement de type du tableau retourné : Pair<int, int>[] => RiskActorBook[]
        //internal Pair<int, int>[] GetPairsActorBookConstitutingPosition(RiskElementClass pRiskElementClass, int pIdBook)
        public RiskActorBook[] GetPairsActorBookConstitutingPosition(RiskElementClass pRiskElementClass, int pIdBook)
        {
            IEnumerable<RiskElement> riskElements = GetCalculationElements(pRiskElementClass, pIdBook);
            //Pair<int, int>[] actorBookPairs = PositionsExtractor.GetPairsActorBookConstitutingPosition(riskElements, this.IsGrossMargining);
            RiskActorBook[] actorBookPairs = RiskElement.GetPairsActorBookConstitutingPosition(riskElements, this.IsGrossMargining);
            return actorBookPairs;
        }

        /// <summary>
        /// Get all the risk elements constituting the global position of the actor
        /// </summary>
        /// <param name="pRiskElementClass">kind of deposit we are interested to evaluate</param>
        /// <param name="pIdBook">affected book of the deposit, needed only when ADDITIONALDEPOSIT are requsted</param>
        /// <returns>the risk elements list</returns>
        public IEnumerable<RiskElement> GetCalculationElements(RiskElementClass pRiskElementClass, int pIdBook)
        {
            IEnumerable<RiskElement> calculationElements = null;

            switch (pRiskElementClass)
            {
                case RiskElementClass.DEPOSIT:
                    calculationElements = (
                        // L'actor principal de calcul du déposit
                        from riskElement in RootElements
                        where (riskElement.RiskElementClass == RiskElementClass.DEPOSIT)
                        select riskElement)
                        .Concat(
                        // Select all the actors sending their elements to the actor owning the attribute 
                        from riskElement in InheritedElements.Values
                        where (riskElement.RiskElementClass == RiskElementClass.ELEMENT)
                        select riskElement
                        );
                    break;
                case RiskElementClass.ADDITIONALDEPOSIT:
                    calculationElements = (
                        // Prendre les acteurs permettant de constituer la position
                        from riskElement in RootElements
                        where ((riskElement.RiskElementClass == RiskElementClass.ADDITIONALDEPOSIT)
                            && (riskElement.AffectedBookId == pIdBook))
                        select riskElement);
                    break;
                default:
                    throw new NotSupportedException(
                        String.Format(Ressource.GetString("RiskPerformance_ERRNotSupportedRiskElementType"), this.IdNode, pRiskElementClass));
            }
            return calculationElements;
        }

        /// <summary>
        /// Get the previous trade risk realtive to the current 
        /// </summary>
        /// <param name="pCss">wanted clearing house (internal id)</param>
        /// <param name="pIdB">wanted book</param>
        /// <param name="pTiming">wanted timing</param>
        /// <returns></returns>
        public TradeRisk GetPreviousTradeRisk(int pCss, int pIdB, SettlSessIDEnum pTiming)
        {
            TradeRisk previousRisk = (
                from trade in this.Results
                where ((trade.CssId == pCss)
                     && (trade.BookId == pIdB)
                     && (trade.Timing == pTiming))
                select trade).FirstOrDefault();
            return previousRisk;
        }

        /// <summary>
        /// Build a number of risk elements, corresponding to the number of deposits we need to generate for the attribute owner
        /// </summary>
        public void BuildRootElements()
        {
            // Build the collection if it is empty
            if (this.RootElements.Count <= 0)
            {
                // 1. Build the collection of the IMR elements

                IEnumerable<RiskElement> riskElements;

                if (this.IsGrossMargining)
                {
                    riskElements = BuildRiskElements(true, false, RiskElementClass.DEPOSIT);
                }
                else
                {
                    riskElements = BuildRiskElements(false, true, RiskElementClass.DEPOSIT);
                }

                this.RootElements.AddRange(riskElements);

                // 2. Build the additional deposits, when the IsOnAllBooks is true

                if (this.IsOnAllBooks)
                {

                    // build the actor books collection, using the books having positions
                    // PM 20170313 [22833] Ne plus utiliser AggregatePositionsByActorBook() et changement de types
                    //Dictionary<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> currentActorBooksPositions =
                    //    (from positionsGroup in this.AggregatePositionsByActorBook()
                    //    where positionsGroup.Key.First == this.IdNode
                    //    select positionsGroup).ToDictionary(elem => elem.Key, elem => elem.Value, new PairComparer<int,int>());

                    // RD 20170512 [23092] Déterminer la position par rapport à liste des Books sur le noeud MRO
                    //Dictionary<RiskActorBook, RiskData> currentActorBooksPositions =
                    //   (from positionsGroup in RiskData.AggregateByActorBook(m_MRORiskData.Values)
                    //    where positionsGroup.Key.IdA == this.IdNode
                    //    select positionsGroup).ToDictionary(elem => elem.Key, elem => elem.Value, new RiskActorBookComparer());
                    Dictionary<RiskActorBook, RiskData> currentActorBooksPositions =
                        (from positionsGroup in RiskData.AggregateByActorBook(m_MRORiskData.Values)
                         from booksWithParameters in this.BooksWithParameters
                         where positionsGroup.Key.IdB == booksWithParameters.BookId
                         select positionsGroup).ToDictionary(elem => elem.Key, elem => elem.Value, new RiskActorBookComparer());

                    // filter the actor books collection removing the elements corresponding to the previously built risk elements
                    // PM 20170313 [22833] Changement de types
                    //IEnumerable<Pair<int, int>> validActorBooksKeys =
                    //    (from keyActorPositions in currentActorBooksPositions select keyActorPositions.Key)
                    //    .Except
                    //        ((from positionsbyBook in currentActorBooksPositions select positionsbyBook.Key)
                    //        .Intersect
                    //        (from rootElement in this.RootElements select 
                    //             new Pair<int, int> { First = rootElement.ActorId, Second = rootElement.AffectedBookId }, 
                    //             new PairComparer<int,int>()));
                    IEnumerable<RiskActorBook> validActorBooksKeys =
                        (from keyActorPositions in currentActorBooksPositions select keyActorPositions.Key)
                        .Except
                            ((from positionsbyBook in currentActorBooksPositions select positionsbyBook.Key)
                            .Intersect
                            (from rootElement in this.RootElements
                             select
                                 new RiskActorBook(rootElement.ActorId, rootElement.AffectedBookId),
                                 new RiskActorBookComparer()));

                    // PM 20170313 [22833] Changement de types
                    //foreach (Pair<int, int> actorBookKey in validActorBooksKeys)
                    foreach (RiskActorBook actorBookKey in validActorBooksKeys)
                    {
                        RiskElement actorRiskElement = null;

                        // PM 20170313 [22833] Changement de types
                        //actorRiskElement = CreateRootElement(actorBookKey.First, actorBookKey.Second,
                        //        // an additional deposit (or elementary) is to consider as Net (SumPosition) ever
                        //        RiskElementEvaluation.SumPosition, RiskElementClass.ADDITIONALDEPOSIT);
                        // RD 20170512 [23092] Le déposit est à calculer sur le noeud MRO et non pas sur le Dealer
                        actorRiskElement = CreateRootElement(this.IdNode, actorBookKey.IdB,
                                // an additional deposit (or elementary) is to consider as Net (SumPosition) ever
                                RiskElementEvaluation.SumPosition, RiskElementClass.ADDITIONALDEPOSIT);

                        // PM 20170313 [22833] Remplacement de Positions par RiskDataActorBook
                        //actorRiskElement.Positions.Add(actorBookKey, currentActorBooksPositions[actorBookKey]);
                        actorRiskElement.RiskDataActorBook.Add(actorBookKey, currentActorBooksPositions[actorBookKey]);

                        this.RootElements.Add(actorRiskElement);
                    }

                } // end if this.IsOnAllBooks

                // PM 20131010 [19046] Added
                // Ajout en tant que RootElements, les Acteurs/Books pour lesquels existent déjà un Trade MR mais qui ne sont plus en position
                // PM 20170313 [22833] Changement de types
                //IEnumerable<Pair<int, int>> validResultKeys =
                //        (from result in this.Results select new Pair<int, int> { First = result.ActorId, Second = result.BookId })
                //        .Except
                //            (from rootElement in this.RootElements
                //             select
                //                 new Pair<int, int> { First = rootElement.ActorId, Second = rootElement.AffectedBookId },
                //                 new PairComparer<int, int>());
                IEnumerable<RiskActorBook> validResultKeys =
                        (from result in this.Results select new RiskActorBook(result.ActorId, result.BookId))
                        .Except
                            (from rootElement in this.RootElements
                             select
                                 new RiskActorBook(rootElement.ActorId, rootElement.AffectedBookId),
                                 new RiskActorBookComparer());

                // PM 20170313 [22833] Changement de types
                //foreach (Pair<int, int> actorBookKey in validResultKeys)
                foreach (RiskActorBook actorBookKey in validResultKeys)
                {
                    // PM 20170313 [22833] Changement de types
                    //RiskElement actorRiskElement =  CreateRootElement(actorBookKey.First, actorBookKey.Second, RiskElementEvaluation.SumPosition, RiskElementClass.ADDITIONALDEPOSIT);
                    RiskElement actorRiskElement = CreateRootElement(actorBookKey.IdA, actorBookKey.IdB, RiskElementEvaluation.SumPosition, RiskElementClass.ADDITIONALDEPOSIT);

                    this.RootElements.Add(actorRiskElement);
                }

            } // edn if Deposits.Count > 0
        }

        /// <summary>
        /// Get the risk elements according to the input flags and the internal IMR book value
        /// </summary>
        /// <param name="pBuildGrossElements">build gross elements flag</param>
        /// <param name="pBuildNetElements">build net elements flag</param>
        /// <param name="pRiskElementClass">risk element type of the built elements</param>
        public IEnumerable<RiskElement> BuildRiskElements(bool pBuildGrossElements, bool pBuildNetElements, RiskElementClass pRiskElementClass)
        {

            // 1. Aggregate by Actor/Book 
            // PM 20170313 [22833] Ne plus utiliser AggregatePositionsByActorBook() et changement de types
            //Dictionary<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> riskDataElements = this.AggregatePositionsByActorBook();
            Dictionary<RiskActorBook, RiskData> riskDataElements = RiskData.AggregateByActorBook(m_MRORiskData.Values);

            List<RiskElement> riskElements = new List<RiskElement>();

            RiskElement actorRiskNetElement = null;
            RiskElement actorRiskGrossElement = null;

            // 2. for each group of positions build the needed deposits
            // PM 20170313 [22833] Changement de types
            //foreach (KeyValuePair<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> riskDataByActor in riskDataElements)
            foreach (KeyValuePair<RiskActorBook, RiskData> riskDataByActor in riskDataElements)
            {
                // 2.1 in case of the IMR book is specified on create one couple (NET, GROSS) of deposit only
                if (this.IMRBookId > 0)
                {
                    if (actorRiskNetElement == null)
                    {
                        actorRiskNetElement = CreateRootElement(this.IdNode, this.IMRBookId,
                            RiskElementEvaluation.SumPosition, pRiskElementClass);
                    }

                    if (actorRiskGrossElement == null)
                    {
                        actorRiskGrossElement = CreateRootElement(this.IdNode, this.IMRBookId,
                            RiskElementEvaluation.SumDeposit, pRiskElementClass);
                    }
                }
                //else
                // 2.1 in case of the IMR book is NOT specified we create a couple of deposit for any couple actor/book
                // if (this.IMRBookId <= 0)
                else if (pRiskElementClass == RiskElementClass.ELEMENT)
                {
                    // PM 20170313 [22833] Changement de types
                    //actorRiskNetElement =
                    //    CreateRootElement(riskDataByActor.Key.First, riskDataByActor.Key.Second,
                    //    RiskElementEvaluation.SumPosition, pRiskElementClass);

                    //actorRiskGrossElement =
                    //    CreateRootElement(riskDataByActor.Key.First, riskDataByActor.Key.Second,
                    //    RiskElementEvaluation.SumDeposit, pRiskElementClass);

                    actorRiskNetElement = CreateRootElement(riskDataByActor.Key.IdA, riskDataByActor.Key.IdB, RiskElementEvaluation.SumPosition, pRiskElementClass);

                    actorRiskGrossElement = CreateRootElement(riskDataByActor.Key.IdA, riskDataByActor.Key.IdB, RiskElementEvaluation.SumDeposit, pRiskElementClass);
                }

                // 3. adding to the collection just the item of the type demanded by the higher elements (according to the input flags)

                if (pBuildNetElements && actorRiskNetElement != null)
                {
                    if (!riskElements.Contains(actorRiskNetElement))
                    {
                        riskElements.Add(actorRiskNetElement);
                    }

                    // PM 20170313 [22833] Remplacement de Positions par RiskDataActorBook
                    //actorRiskNetElement.Positions.Add(riskDataByActor.Key, riskDataByActor.Value);
                    actorRiskNetElement.RiskDataActorBook.Add(riskDataByActor.Key, riskDataByActor.Value);
                }

                if (pBuildGrossElements && actorRiskGrossElement != null)
                {
                    if (!riskElements.Contains(actorRiskGrossElement))
                    {
                        riskElements.Add(actorRiskGrossElement);
                    }

                    // PM 20170313 [22833] Remplacement de Positions par RiskDataActorBook
                    //actorRiskGrossElement.Positions.Add(riskDataByActor.Key, riskDataByActor.Value);
                    actorRiskGrossElement.RiskDataActorBook.Add(riskDataByActor.Key, riskDataByActor.Value);
                }
            }

            // 2 in case of no riskElements has been generated and we have an MRO book specified, 
            //  we build at least the deposit relative to the IMR book (maybe we can attend to inherit some positions from the calculation
            //  hierarchy)

            if (riskElements.Count <= 0 && this.IMRBookId > 0)
            {
                if (pBuildNetElements)
                {
                    riskElements.Add(CreateRootElement(this.IdNode, this.IMRBookId, RiskElementEvaluation.SumPosition, pRiskElementClass));
                }

                if (pBuildGrossElements)
                {
                    riskElements.Add(CreateRootElement(this.IdNode, this.IMRBookId, RiskElementEvaluation.SumDeposit, pRiskElementClass));
                }
            }

            return riskElements;
        }

        /// <summary>
        /// Remove from the risk elements collection all the elements already existing 
        /// in the inherited risk elements collection of the attribute
        /// </summary>
        /// <param name="pAffectedRiskElements">affected list</param>
        /// <param name="pControlRiskElements">control list, 
        /// any element existant either in the control collection either in the the affected one 
        /// has to be deleted from the affected collection</param>
        /// <param name="pLog">Log event in case of duplicate suppression, if null no evenement will be logged</param>
        /// PM 20170313 [22833] Changement de type, Positions devenu RiskDataActorBook
        // EG 20190114 Add detail to ProcessLog Refactoring
        public static void RemoveDuplicateRiskElements(IEnumerable<RiskElement> pAffectedRiskElements, IEnumerable<RiskElement> pControlRiskElements,   SetErrorWarning pSetErrorWarning)
        {
            IEnumerable<RiskActorBook> duplicateItems = ((
                from element in pControlRiskElements
                from key in element.RiskDataActorBook.Keys
                select key)
                .Distinct(new RiskActorBookComparer()))
            .Intersect((
                from element in pAffectedRiskElements
                from key in element.RiskDataActorBook.Keys
                select key)
                .Distinct(new RiskActorBookComparer()),
                new RiskActorBookComparer());

            //foreach (Pair<int, int> duplicateItem in duplicateItems)
            foreach (RiskActorBook duplicateItem in duplicateItems)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pSetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1026), 0,
                    new LogParam(Convert.ToString(duplicateItem.IdA)),
                    new LogParam(Convert.ToString(duplicateItem.IdB))));

                foreach (RiskElement element in pAffectedRiskElements)
                {
                    //bool removed = element.Positions.Remove(duplicateItem);
                    bool removed = element.RiskDataActorBook.Remove(duplicateItem);

                    if (!removed)
                    {
                        throw new NotSupportedException(Ressource.GetString("RiskPerformance_ERRDuplicateSuppression"));
                    }
                }
            }
        }

        // RD 20170420 [23092] Rendre la méthode Override car elle est nuvellement définie dans la classe de base        
        public override void BuildBooks()
        {
            foreach (MarginReqOfficeBooksAndParameters book in BooksWithParameters)
            {
                BookNode node = new BookNode(book.Book, book.BookId, book.BookName, (book.BookId == book.IMRBookId));

                this.Books.Add(node);
            }
        }

        private bool SetMarginProperties()
        {
            bool set = false;

            MarginReqOfficeBooksAndParameters firstBook = BooksWithParameters.FirstOrDefault();

            if (firstBook != null)
            {
                set = true;

                IMRBookId = firstBook.IMRBookId;

                IsOnAllBooks =
                    firstBook.AffectAllBooks == EfsML.Enum.GlobalElementaryEnum.Elementary ||
                    firstBook.AffectAllBooks == EfsML.Enum.GlobalElementaryEnum.Full;

                IsGrossMargining = firstBook.IsGrossMargining;

                WeightingRatio = firstBook.WeightingRatio;
            }

            return set;
        }

        /// <summary>
        /// Load supplementary risk parameters that are valid just for specific markets and clearing houses
        /// </summary>
        /// <param name="pParameterValues">
        /// requested parameters in order to load the supplementary risk parameter, a valid IDA is requested
        /// </param>
        /// <param name="pConnection">current connection</param>
        /// <returns>true when the attribute has been built in a right way</returns>
        private bool LoadSupplementaryRiskParameter(IDbConnection pConnection, Dictionary<string, object> pParameterValues)
        {
            if (this.IdNode <= 0)
            {
                //the attribute has not been not correctly initialized, 
                //create the attribute passing a valid node Id (IdNode > 0 and corresponding to an existing Spheres element)
                return false;
            }

            // get clearing parameters

            CommandType type = DataContractHelper.GetType(DataContractResultSets.CLEARINGORGPARAMETER);
            string request = DataContractHelper.GetQuery(DataContractResultSets.CLEARINGORGPARAMETER);

            this.ClearingOrgParameters = DataHelper<ClearingOrgParameter>.ExecuteDataSet(
                pConnection, type, request,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.CLEARINGORGPARAMETER, pParameterValues));

            // get market parameters

            type = DataContractHelper.GetType(DataContractResultSets.EQUITYMARKETPARAMETER);
            request = DataContractHelper.GetQuery(DataContractResultSets.EQUITYMARKETPARAMETER);

            this.EquityMarketParameters = DataHelper<EquityMarketParameter>.ExecuteDataSet(
                pConnection, type, request,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.EQUITYMARKETPARAMETER, pParameterValues));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <param name="pFunction"></param>
        /// <param name="pRiskElementClass"></param>
        /// <returns></returns>
        //PM 20141216 [9700] Eurex Prisma for Eurosys Futures : La méthode devient public
        public static RiskElement CreateRootElement(
            int pIdA, int pIdB, RiskElementEvaluation pFunction, RiskElementClass pRiskElementClass)
        {
            RiskElement actorRiskElement = new RiskElement
            {
                ActorId = pIdA,
                AffectedBookId = pIdB,
                RiskElementEvaluation = pFunction,
                RiskElementClass = pRiskElementClass
            };

            return actorRiskElement;
        }
    }

    /// <summary>
    /// Class representing a risk evaluation result already computed
    /// </summary>
    [XmlRoot(ElementName = "TradeRisk")]
    public class TradeRisk : ISpheresNodeLeaf
    {
        string m_Trade;
        int m_IdTrade;
        string m_TradeName;
        DateTime m_DateBusiness;

        /// <summary>
        /// Business Date
        /// </summary>
        [XmlAttribute(AttributeName = "dateBusiness")]
        public DateTime DateBusiness
        {
            get { return m_DateBusiness; }
            set { m_DateBusiness = value; }
        }

        SettlSessIDEnum m_Timing;

        /// <summary>
        /// Timing mode
        /// </summary>
        [XmlAttribute(AttributeName = "timing")]
        public SettlSessIDEnum Timing
        {
            get { return m_Timing; }
            set { m_Timing = value; }
        }

        int m_CssId;

        /// <summary>
        /// Clearing house internal id
        /// </summary>
        [XmlAttribute(AttributeName = "cssid")]
        public int CssId
        {
            get { return m_CssId; }
            set { m_CssId = value; }
        }

        int m_ActorId;

        /// <summary>
        /// Actor internal id
        /// </summary>
        [XmlAttribute(AttributeName = "actorid")]
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }


        int m_BookId;

        /// <summary>
        /// Book internal id
        /// </summary>
        [XmlAttribute(AttributeName = "bookid")]
        public int BookId
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        // PM 20131217 [19365] Add m_IsClearer
        bool m_IsClearer;

        /// <summary>
        /// Margin Requirement for Clearer or not
        /// </summary>
        [XmlAttribute(AttributeName = "isClearer")]
        public bool IsClearer
        {
            get { return m_IsClearer; }
            set { m_IsClearer = value; }
        }

        RiskRevaluationMode m_ReEvaluation;

        /// <summary>
        /// Flag to force a new evaluation of a previous risk amount
        /// </summary>
        [XmlAttribute(AttributeName = "update")]
        public RiskRevaluationMode ReEvaluation
        {
            get { return m_ReEvaluation; }
            set { m_ReEvaluation = value; }
        }

        /// <summary>
        /// Get an empty result
        /// </summary>
        public TradeRisk() { }

        /// <summary>
        /// Get a result
        /// </summary>
        /// <param name="pTrade">trade identifier</param>
        /// <param name="pTradeId">trade internal id</param>
        /// <param name="pTradeName">trade display name</param>
        /// <param name="pDateBusiness">date business for the current entity/clearing house</param>
        /// <param name="pIdACss">clearing house internal id</param>
        /// <param name="pIdA">Actor internal id</param>
        /// <param name="pIdB">book internal id</param>
        /// <param name="pTiming">string representation for the timing of the created risk result</param>
        /// <param name="pIsClearer">Indique s'il s'agit d'un trade risk pour un clearer ou non</param>
        // PM 20131217 [19365] Add pIsClearer
        public TradeRisk(
            string pTrade, int pTradeId, string pTradeName,
            DateTime pDateBusiness, int pIdACss, int pIdA, int pIdB, string pTiming, bool pIsClearer)
        {
            this.Identifier = pTrade;
            this.Id = pTradeId;
            this.DisplayName = pTradeName;
            this.m_DateBusiness = pDateBusiness;
            this.m_CssId = pIdACss;
            this.m_ActorId = pIdA;
            this.m_BookId = pIdB;
            // PM 20131217 [19365] Add pIsClearer
            this.m_IsClearer = pIsClearer;

            string timingName = ReflectionTools.GetMemberNameByXmlEnumAttribute(typeof(SettlSessIDEnum), pTiming, true);
            if (System.Enum.IsDefined(typeof(SettlSessIDEnum), timingName))
            {
                this.m_Timing = (SettlSessIDEnum)System.Enum.Parse(typeof(SettlSessIDEnum), timingName);
            }
            else if (System.Enum.IsDefined(typeof(SettlSessIDEnum), pTiming))
            {
                this.m_Timing = (SettlSessIDEnum)System.Enum.Parse(typeof(SettlSessIDEnum), pTiming);
            }
        }

        #region ISpheresNodeLeaf Membres

        /// <summary>
        /// Trade identifier
        /// </summary>
        [XmlAttribute(AttributeName = "trade")]
        public string Identifier
        {
            get { return m_Trade; }
            set { m_Trade = value; }
        }

        /// <summary>
        /// Trade internal id
        /// </summary>
        [XmlAttribute(AttributeName = "tradeid")]
        public int Id
        {
            get { return m_IdTrade; }
            set { m_IdTrade = value; }
        }

        /// <summary>
        /// Trade display name
        /// </summary>
        [XmlElement(ElementName = "TradeName")]
        public string DisplayName
        {
            get { return m_TradeName; }
            set { m_TradeName = value; }
        }

        #endregion

    }

    /// <summary>
    /// Classe représentant un élément de calcul de risque (
    /// Class representing a risk calculation element (evaluated as positions set or deposit), 
    /// this element can be originated by a MARGINREQOFFICE actor.
    /// </summary>
    [XmlRoot(ElementName = "RiskElement")]
    public class RiskElement
    {
        int m_ActorId;

        /// <summary>
        /// Id de l'acteur propriétaire du book de la position
        /// </summary>
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        int m_AffectedBookId;

        /// <summary>
        /// Id du book de la position
        /// </summary>
        public int AffectedBookId
        {
            get { return m_AffectedBookId; }
            set { m_AffectedBookId = value; }
        }

        RiskElementEvaluation m_RiskElementEvaluation;

        /// <summary>
        /// Stratégie d'évaluation de la position inclus dans l'élément
        /// </summary>
        [XmlAttribute(AttributeName = "evaluationtype")]
        public RiskElementEvaluation RiskElementEvaluation
        {
            get { return m_RiskElementEvaluation; }
            set { m_RiskElementEvaluation = value; }
        }

        /// <summary>
        /// Type d'élément de calcul de risque
        /// </summary>
        [XmlAttribute(AttributeName = "elementclass")]
        public RiskElementClass RiskElementClass
        {
            get;
            set;
        }

        // PM 20170313 [22833] m_Positions remplacé par m_RiskData
        //SerializableDictionary<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> m_Positions =
        //    new SerializableDictionary<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>>(
        //        "ActorId_BookId", "Positions", new PairComparer<int,int>());

        ///// <summary>
        ///// Position de l'élément groupée par actor/book.
        ///// </summary>
        //public SerializableDictionary<Pair<int, int>, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> Positions
        //{
        //    get { return m_Positions; }
        //    set { m_Positions = value; }
        //}

        SerializableDictionary<RiskActorBook, RiskData> m_RiskDataActorBook = new SerializableDictionary<RiskActorBook, RiskData>("ActorId_BookId", "RiskDataActorBook", new RiskActorBookComparer());

        /// <summary>
        /// Données de calcul de risque de l'élément groupée par actor/book.
        /// </summary>
        public SerializableDictionary<RiskActorBook, RiskData> RiskDataActorBook
        {
            get { return m_RiskDataActorBook; }
            set { m_RiskDataActorBook = value; }
        }

        #region Methods
        /// <summary>
        /// Obtient les RiskData du RiskElement
        /// </summary>
        /// <returns></returns>
        /// PM 20170313 [22833] New (remplace PositionsExtractor.GetPositionsByBook() )
        public IEnumerable<RiskData> GetRiskData()
        {
            return m_RiskDataActorBook.Values;
        }

        /// <summary>
        /// Obtient tous les RiskData d'une collection de RiskElement
        /// </summary>
        /// <param name="pRiskElements"></param>
        /// <returns>Une collection de RiskData</returns>
        /// PM 20170313 [22833] New
        public static IEnumerable<RiskData> GetRiskData(IEnumerable<RiskElement> pRiskElements)
        {
            IEnumerable<RiskData> allRiskData;
            if (pRiskElements != default(IEnumerable<RiskElement>))
            {
                allRiskData =
                    from elements in pRiskElements
                    from data in elements.RiskDataActorBook
                    select data.Value;
            }
            else
            {
                allRiskData = new List<RiskData>();
            }
            return allRiskData;
        }

        /// <summary>
        /// Obtient tous les RiskData d'un actor à partir d'une collection de RiskElement
        /// </summary>
        /// <param name="pRiskElements"></param>
        /// <param name="pIdActor"></param>
        /// <returns>Une collection de RiskData</returns>
        /// PM 20170313 [22833] New
        public static IEnumerable<RiskData> GetRiskData(IEnumerable<RiskElement> pRiskElements, int pIdActor)
        {
            IEnumerable<RiskData> allRiskData;
            if (pRiskElements != default(IEnumerable<RiskElement>))
            {
                allRiskData =
                    from elements in pRiskElements
                    where elements.ActorId == pIdActor
                    from data in elements.RiskDataActorBook
                    select data.Value;
            }
            else
            {
                allRiskData = new List<RiskData>();
            }
            return allRiskData;
        }

        /// <summary>
        /// Regroupe les données de risque d'une collection de RiskElement
        /// </summary>
        /// <param name="pRiskElement">Un ensemble de RiskElement</param>
        /// <returns>Un nouvel objet représentant l'ensemble des données de risque</returns>
        /// PM 20170313 [22833] New
        public static RiskData AggregateRiskData(IEnumerable<RiskElement> pRiskElement)
        {
            IEnumerable<RiskData> allRiskData = RiskElement.GetRiskData(pRiskElement);
            RiskData riskdata = RiskData.Aggregate(allRiskData);
            return riskdata;
        }

        /// <summary>
        /// Get the list of all the actors/books inside the risk elements list
        /// </summary>
        /// <param name="pRiskElements">a risk elements collection, pass only the RiskElement objects you are interested to investigate to</param>
        /// <param name="pIsGrossMargining">true when we want to avoid to search internal actor/book 
        /// pairs inside the RiskElement objects class type ELEMENT, because we are evaluating a gross deposit</param>
        /// <returns>an actor/book pairs vector</returns>
        /// PM 20170313 [22833] Changement de type du tableau retourné : Pair<int, int>[] => RiskActorBook[]
        //static public Pair<int, int>[] GetPairsActorBookConstitutingPosition(IEnumerable<RiskElement> pRiskElements, bool pIsGrossMargining)
        static public RiskActorBook[] GetPairsActorBookConstitutingPosition(IEnumerable<RiskElement> pRiskElements, bool pIsGrossMargining)
        {
            //return
            //    (from riskElement in pRiskElements
            //     select new Pair<int, int>(riskElement.ActorId, riskElement.AffectedBookId))
            //    .Union
            //    (from riskElement in pRiskElements
            //     where
            //         // UNDONE 20111116 MF pour un deposit Net il faudrait renseigner le filtre "&& !pIsGrossMargining" à l'image de ce que 
            //         // l'on a mis plus bas pour les objets ELEMENT, pour les deposits elementaires (additional deposit) 
            //         // ne devrait changer rien que l'on le mette ou pas
            //     riskElement.RiskElementClass == RiskElementClass.DEPOSIT
            //     || riskElement.RiskElementClass == RiskElementClass.ADDITIONALDEPOSIT
            //     || (riskElement.RiskElementClass == RiskElementClass.ELEMENT && !pIsGrossMargining)
            //     from actorBookPair in riskElement.Positions.Keys
            //     select actorBookPair)
            //    .Distinct(new PairComparer<int, int>())
            //    .ToArray();

            RiskActorBook[] actorBook;
            if (pRiskElements != default(IEnumerable<RiskElement>))
            {
                actorBook = (
                    from riskElement in pRiskElements
                    select new RiskActorBook(riskElement.ActorId, riskElement.AffectedBookId)
                    ).Union(
                    from riskElement in pRiskElements
                    where
                        // UNDONE 20111116 MF pour un deposit Net il faudrait renseigner le filtre "&& !pIsGrossMargining" à l'image de ce que
                        // l'on a mis plus bas pour les objets ELEMENT, pour les deposits elementaires (additional deposit)
                        // ne devrait changer rien que l'on le mette ou pas
                        (riskElement.RiskElementClass == RiskElementClass.DEPOSIT)
                        || (riskElement.RiskElementClass == RiskElementClass.ADDITIONALDEPOSIT)
                        || ((riskElement.RiskElementClass == RiskElementClass.ELEMENT) && !pIsGrossMargining)
                    from actorBookPair in riskElement.RiskDataActorBook.Keys
                    select actorBookPair)
                    .Distinct(new RiskActorBookComparer())
                    .ToArray();
            }
            else
            {
                actorBook = new RiskActorBook[0];
            }
            return actorBook;
        }
        #endregion
    }
}

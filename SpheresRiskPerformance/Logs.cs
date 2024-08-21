using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

using EFS.Common;
using EFS.ParseException;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.SpheresRiskPerformance.SpheresObjects;
using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.Common.Log;
using EFS.TradeInformation;
using EFS.Actor;

using EfsML.Business;
using EfsML.Interface;
using EfsML.v30.MarginRequirement;
using EfsML.v30.Doc;

using FpML.Interface;
using FpML.v44.Shared;

using FixML.v50SP1;
using FixML.v50SP1.Enum;

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
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) ||
                Object.ReferenceEquals(y, null))
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
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashIdA = obj.IdA.GetHashCode();
            int hashIdB = obj.IdB.GetHashCode();
            int hashIdCSS = obj.IdCSS.GetHashCode();

            return hashIdA ^ hashIdB ^ hashIdCSS;
        }

        #endregion
    }


    /// <summary>
    /// Repository containing the results of the risk evaluation (including details)
    /// </summary>
    public sealed class CalculationSheetRepository 
    {

        SerializableDictionary<MarginDetailsDocumentKey, MarginDetailsDocument> m_CalculationSheets
            = new SerializableDictionary<MarginDetailsDocumentKey, MarginDetailsDocument>(
                "DealerId_BookId_CSSId", "CalculationSheet", new MarginDetailsDocumentKeyComparer());

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
        { get; set; }

        /// <summary>
        /// log level detail
        /// </summary>
        [XmlIgnore]
        public ErrorManager.DetailEnum LogDetailEnum
        { get; set; }

        /// <summary>
        /// EFS licence
        /// </summary>
        [XmlIgnore]
        public EFS.EFSTools.License License { get; set; }
        
        /// <summary>
        /// Process Log
        /// </summary>
        [XmlIgnore]
        public ProcessLog ProcessLog { get; set; }

        /// <summary>
        /// Class helper to fill the calculation sheet with common trade elements (parties, markets repository, asset repository, etcetera...)
        /// </summary>
        DataDocumentContainer m_DataDocumentContainer = null;

        /// <summary>
        /// Actor sql elements cache used to build party elements
        /// </summary>
        Dictionary<int, SQL_Actor> m_ActorCache = new Dictionary<int, SQL_Actor>();

        /// <summary>
        /// Actor sql elements cache
        /// </summary>
        Dictionary<int, SQL_Book> m_BookCache = new Dictionary<int, SQL_Book>();

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
        /// Loggin delegate method
        /// </summary>
        LogAddDetail m_Log = null;

        /// <summary>
        /// Attaching delegate method
        /// </summary>
        AddAttachedDoc m_Attach = null;

        /// <summary>
        /// Main process informations
        /// </summary>
        RiskPerformanceProcessInfo m_ProcessInfo;

        /// <summary>
        /// Main process informations
        /// </summary>
        internal RiskPerformanceProcessInfo ProcessInfo
        {
            get { return m_ProcessInfo; }
            set { m_ProcessInfo = value; }
        }

        /// <summary>
        /// New empty repository instance
        /// </summary>
        public CalculationSheetRepository()
        {
            
        }

        /// <summary>
        /// Return null when trade is not created (ex: previousResult.ReEvaluation == RiskRevaluationMode.DoNotEvaluate) 
        /// 
        /// </summary>
        /// <remarks>Create a new risk evaluation result object</remarks>
        /// <returns>the technical trade representing the risk evaluation result</returns>
        public ITrade CreateTradeResult(string pCS,
            int pIdACSS, int pIdA, int pIdB, DateTime pDtBusiness, TradeRisk pPreviousResult, List<Money> pMoney, 
            out int opIdt)
        {
            //
            ITrade ret = null;
            opIdt = 0;
            //
            SQL_Instrument sqlInstrument = 
                new SQL_Instrument(
                    CSTools.SetCacheOn(pCS), Cst.ProductMarginRequirement, SQL_Table.RestrictEnum.No, 
                    SQL_Table.ScanDataDtEnabledEnum.No, this.AppInstance.SessionId, true);

            bool findInstr = sqlInstrument.LoadTable(new string[] { "IDI,IDENTIFIER" });
            if (false == findInstr)
                throw new Exception(StrFunc.AppendFormat("Instrument {0} not found", Cst.ProductMarginRequirement));
            //
            Nullable<Cst.Capture.ModeEnum> captureMode = Cst.Capture.ModeEnum.New;
            if (pPreviousResult != null)
            {
                if (pPreviousResult.ReEvaluation == RiskRevaluationMode.EvaluateWithUpdate)
                    captureMode = Cst.Capture.ModeEnum.Update;
                else if (pPreviousResult.ReEvaluation == RiskRevaluationMode.NewEvaluation)
                    captureMode = Cst.Capture.ModeEnum.New;
                else if (pPreviousResult.ReEvaluation == RiskRevaluationMode.DoNotEvaluate)
                    captureMode = null;
            }
            //
            if ((captureMode == Cst.Capture.ModeEnum.New || captureMode == Cst.Capture.ModeEnum.Update))
            {
                //
                int idTSource = 0;
                string screenName = string.Empty;
                //
                if (captureMode == Cst.Capture.ModeEnum.Update)
                {
                    idTSource = pPreviousResult.Id;
                }
                else
                {
                    SearchInstrumentGUI searchInstrumentGUI = new SearchInstrumentGUI(pCS, sqlInstrument.Id);
                    StringData[] data = searchInstrumentGUI.GetDefault(false);
                    //
                    if (ArrFunc.IsEmpty(data))
                        throw new Exception(StrFunc.AppendFormat("Screen or template not found for Instrument {0}", sqlInstrument.Identifier));
                    //
                    screenName = ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value;
                    string templateIdentifier = ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value;
                    //
                    idTSource = TradeRDBMSTools.GetTradeIdT(pCS, templateIdentifier);
                }
                //
                if (idTSource == 0)
                    throw new Exception("Trade Source not found");
                //
                #region Create new trade ou update existing trade
                //
                string idMenu = Cst.IdMenu.InputTrade; // UNDONE FI 20110501 Quel est le menu ?? pour l'instant C'est trade
                User user = new User(this.AppInstance.IdA, null, RoleActor.SYSADMIN.ToString());
                CaptureSessionInfo sessionInfo = new CaptureSessionInfo(user, this.AppInstance, License, ProcessLog);
                InputUser inputUser = new InputUser(pCS, idMenu, user);
                //
                TradeRiskCaptureGen captureGen = new TradeRiskCaptureGen(pCS);
                bool isFound = 
                    captureGen.Load(
                    idTSource.ToString(), SQL_TableWithID.IDType.Id, (Cst.Capture.ModeEnum)captureMode, this.AppInstance.SessionId, true, false);

                if (false == isFound)
                    throw new Exception(StrFunc.AppendFormat("<b>trade [idT:{0}] not found</b>", idTSource));

                captureGen.InitBeforeCaptureMode(inputUser, sessionInfo);
                //
                SQL_Actor sqlActor = new SQL_Actor(CSTools.SetCacheOn(pCS), pIdA);
                sqlActor.LoadTable();
                //
                SQL_Book sqlBook = new SQL_Book(CSTools.SetCacheOn(pCS), pIdB);
                sqlBook.LoadTable();
                //
                SQL_Actor sqlActorCSS = new SQL_Actor(CSTools.SetCacheOn(pCS), pIdACSS);
                sqlActorCSS.LoadTable();
                //
                SQL_Actor sqlActorEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), this.m_ProcessInfo.Entity);

                sqlActorEntity.LoadTable();
                //
                SetDataDocument(captureGen.TradeCommonInput.DataDocument, sqlActor, sqlBook, sqlActorEntity, sqlActorCSS, pDtBusiness, pMoney, this.m_ProcessInfo.Timing);
                //
                if (captureMode == Cst.Capture.ModeEnum.Update)
                    screenName = captureGen.TradeCommonInput.SQLLastTradeLog.ScreenName;
                //
                if (captureMode == Cst.Capture.ModeEnum.New)
                {
                    //En création: Spheres® ecrase systématiquement le StatusEnvironment issu du template par REGULAR
                    captureGen.TradeCommonInput.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
                }
                //
                //Alimentation des partyNotifications (pas de messagerie)
                captureGen.TradeCommonInput.TradeNotification.SetSetConfirmation(false);
                //Alimentation des partyNotifications (pas de messagerie)
                captureGen.TradeCommonInput.StEnvironnement = 
                    CalculationSheetRepository.RiskEvaluationModeToStatusEnvironment(this.m_ProcessInfo.Mode).ToString();
                //
                TradeRecordSettings recordSettings = new TradeRecordSettings();
                recordSettings.displayName = StrFunc.AppendFormat("Deposit [{0}/{1}/{2}]", DtFunc.DateTimeToStringDateISO(pDtBusiness), sqlActor.Identifier, sqlActorCSS.Identifier);
                recordSettings.description = recordSettings.displayName;
                recordSettings.extLink = string.Empty;
                recordSettings.idScreen = screenName;
                //
                recordSettings.isUpdateOnly_TradeStream = false;
                recordSettings.isCalcIdRequest = true;
                recordSettings.isGetNewIdForIdentifier = true;
                recordSettings.isCheckValidationRules = true;
                recordSettings.isCheckValidationXSD = false;
                //
                TradeCommonCaptureGen.ErrorLevel lRet =
                RecordTrade(pCS, captureGen, recordSettings, sessionInfo, (Cst.Capture.ModeEnum)captureMode);
                //
                if (lRet != TradeCommonCaptureGen.ErrorLevel.SUCCESS)
                {
                    string methodName = MethodInfo.GetCurrentMethod().Name;
                    
                    //
                    string msg = string.Empty;
                    if (captureMode == Cst.Capture.ModeEnum.New)
                        msg =
                            StrFunc.AppendFormat("Error[{0}] when creating Margin Requirement trade for Margin Requirement Office {1} and clearing organization {2}",
                            lRet.ToString(), sqlActor.Identifier, sqlActorCSS.Identifier);
                    else
                        msg = StrFunc.AppendFormat("Error[{0}] when modifying Margin Requirement trade {1} of Margin Requirement Office {2} and clearing organization {3}",
                            lRet.ToString(), pPreviousResult.Identifier, sqlActor.Identifier, sqlActorCSS.Identifier);
                    //
                    throw new OTCmlException(methodName, msg);
                }
                //
                opIdt = captureGen.TradeCommonInput.identification.OTCmlId;
                ret = captureGen.TradeCommonInput.DataDocument.currentTrade;
                #endregion
            }
            //
            return ret;
        }

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
        /// <returns>true when the calculation sheet has been correctly added, 
        /// false when the calculation sheet can not be built because the trade is null</returns>
        public bool AddCalculationSheet(
            string pCS, int pIdActor, int pIdBook, int pIdCss, bool pIsGrossMargining, 
            int pIdTechTrade, ITrade pTechTrade, List<Money> pAmounts)
        {
            if (pTechTrade == null)
                return false;

            bool added = false;

            MarginDetailsDocumentKey key = new MarginDetailsDocumentKey(pIdActor, pIdBook, pIdCss);

            if (!CalculationSheets.ContainsKey(key))
            {
                MarginDetailsDocument calculationsheet = new MarginDetailsDocument(pIdTechTrade, pTechTrade, null);

                CalculationSheets.Add(key, calculationsheet);

                // Process margin req office, set main attributes
                SetMarginRequirement(pCS, pIdActor, pIdBook, pIsGrossMargining, pAmounts, calculationsheet.marginRequirementOffice);

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
        /// <param name="pIsGrossMargining">gross margin flag</param>
        /// <param name="pRiskElementClass">risk element type</param>
        /// <param name="pIdActorsBooksConstitutingPosition">all the actors/books constituting the position, including the current actor</param>
        /// <param name="pCalculationElements">all the positions (owned adn inherited) to evaluate the deposit of the current actor</param>
        /// <param name="pAmounts">deposit evaluated amounts (multiple currencies)</param>
        /// <param name="pMethodType">method type</param>
        /// <param name="pMarginCalculationMethod">data container parameters used to build the MarginCalculatioMethod node of the report</param>
        /// <returns>true when the calculation sheet is well processed, 
        /// false if the calculation sheet has been already processed</returns>
        public bool ProcessCalculationSheet(string pCS,
            int pIdActor, int pIdBook, int pIdCss, 
            DateTime pDtBusiness, RiskEvaluationTiming pTiming, bool pIsGrossMargining, RiskElementClass pRiskElementClass, 
            Pair<int,int>[] pIdActorsBooksConstitutingPosition, IEnumerable<RiskElement> pCalculationElements, List<Money> pAmounts,
            RiskMethodType pMethodType, ICalculationSheetMethod pMarginCalculationMethod)
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

                    int[] pIdActorsConstitutingPosition = (
                        from actorId in pIdActorsBooksConstitutingPosition
                        select actorId.First
                        ).Distinct().ToArray();

                    AddParties(pCS, pIdActorsConstitutingPosition);

                    // 3. Positions and evaluation parameters

                    log.marginRequirementOffice.marginCalculation =
                        new MarginCalculation(
                            log.marginRequirementOffice.isGross);

                    if (log.marginRequirementOffice.isGross)
                    {
                        
                        IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions = 
                            PositionsExtractor.GetPositions(pCalculationElements, pIdActor);
                           
                        log.marginRequirementOffice.marginCalculation.positions =
                            BuildPositionReport(pCS, pDtBusiness, pTiming, positions);

                        SetGrossMargin(pIdActor, pIdBook, pIdCss, pIdActorsBooksConstitutingPosition, 
                            ref log.marginRequirementOffice.marginCalculation.grossMargin);
                    }
                    else 
                        // IsNetMargining
                    {
                        IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions =
                            PositionsExtractor.GetPositions(pCalculationElements);

                        SetNetMargin(
                            pCS, pIdActor, pDtBusiness, pTiming, positions, pAmounts,
                            pMethodType, pMarginCalculationMethod, log.marginRequirementOffice.marginCalculation.netMargin);
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
        /// <param name="pLog">delegate reference to add log</param>
        /// <param name="pAttach">delegate reference to add attachement</param>
        public void InitLogAttachDelegates(LogAddDetail pLog, AddAttachedDoc pAttach)
        {
            this.m_Log = pLog;

            this.m_Attach = pAttach;
        }

        private void AddParties(string pCS, int[] idActorsConstitutingPosition)
        {
            foreach (int idParty in idActorsConstitutingPosition)
            {
                SQL_Actor actor = GetSQLActorFromCache(pCS, idParty);

                m_DataDocumentContainer.AddParty(actor);
            }
        }

        private SQL_Actor GetSQLActorFromCache(string pCS, int idParty)
        {
            SQL_Actor actor = null;

            if (!m_ActorCache.ContainsKey(idParty))
            {
                actor = new SQL_Actor(pCS, idParty);

                m_ActorCache.Add(idParty, actor);
            }
            else
            {
                actor = m_ActorCache[idParty];
            }

            return actor;
        }

        private SQL_Book GetSQLBookFromCache(string pCS, int idBook)
        {
            SQL_Book book = null;

            if (!m_BookCache.ContainsKey(idBook))
            {
                book = new SQL_Book(pCS, idBook);

                m_BookCache.Add(idBook, book);
            }
            else
            {
                book = m_BookCache[idBook];
            }

            return book;
        }

        private BookId GetBookId(string pCS, int idBookAffected)
        {
            
            BookId bookId = new BookId();

            if (idBookAffected > 0)
            {
                SQL_Book book = GetSQLBookFromCache(pCS, idBookAffected);

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
            int pIdActor, int pIdBook, bool pIsGrossMargining, List<Money> pAmounts, MarginRequirementOffice office)
        {
            office.isGross = pIsGrossMargining;

            SQL_Actor dealer = GetSQLActorFromCache(pCS, pIdActor);

            office.href = dealer.Identifier;

            office.payerPartyReference = new PartyReference(m_ActorCache[pIdActor].Identifier);

            SQL_Actor entity = GetSQLActorFromCache(pCS, m_ProcessInfo.Entity);

            office.receiverPartyReference = new PartyReference(entity.Identifier);

            int idBookAffected = pIdBook;

            office.bookId = GetBookId(pCS, idBookAffected);

            // TODO 20110502 MF Encore à tirer de la base ? demande à Philippe...
            office.weightingRatio = 0;

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
        private void SetGrossMargin(
            int pIdActor, int pIdBook, int pIdCss, Pair<int,int>[] pIdActorsBooksConstitutingPosition, ref MarginRequirementOffice[] pGrossMargin)
        {
            
            IEnumerable<MarginDetailsDocument> childLogs =
                from actorbookIdConstitutingPosition
                    in pIdActorsBooksConstitutingPosition
                where actorbookIdConstitutingPosition.First != pIdActor
                join log
                    in m_CalculationSheets
                        on
                        new { IdA = actorbookIdConstitutingPosition.First, IdB = actorbookIdConstitutingPosition.Second, IdCSS = pIdCss } equals
                        new { IdA = log.Key.IdA, IdB = log.Key.IdB, IdCSS = log.Key.IdCSS }
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
        /// <param name="positions">positions to be added</param>
        /// <param name="pAmounts">deposit evaluated amounts (multiple currencies)</param>
        /// <param name="pMethodType">method type</param>
        /// <param name="pMarginCalculationMethod">data container parameters used to build the MarginCalculatioMethod node of the report</param>
        /// <param name="pNetMargin">net margin element to update, must be NOT null</param>
        private void SetNetMargin(
            string pCS, int pIdActor, DateTime pDtBusiness, RiskEvaluationTiming pTiming,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions, List<Money> pAmounts,
            RiskMethodType pMethodType, ICalculationSheetMethod pMarginCalculationMethod, NetMargin pNetMargin)
        {
            pNetMargin.marginAmounts = pAmounts.ToArray();

            FixPositionReport[] positionReports = BuildPositionReport(pCS, pDtBusiness, pTiming, positions);

            pNetMargin.marginCalculationMethod = CalculationMethodFactory(pMethodType, pMarginCalculationMethod);

            pNetMargin.positions = positionReports;
        }

        private void SetInstrumentBlock(string pCS, PosRiskMarginKey keyPos, InstrumentBlock pInstrmt)
        {
            SQL_AssetETD asset = this.AssetETDCache[keyPos.idAsset];

            pInstrmt.AID = new SecAltIDGrp_Block[] { new SecAltIDGrp_Block(), new SecAltIDGrp_Block() };

            pInstrmt.AID[0].AltIDSrc = SecurityIDSourceEnum.ISIN;
            pInstrmt.AID[0].AltIDSrcSpecified = true;
            pInstrmt.AID[0].AltID = asset.ISINCode;
            pInstrmt.AID[0].AltIDSpecified = true;

            pInstrmt.AID[1].AltIDSrc = SecurityIDSourceEnum.ExchangeSymbol;
            pInstrmt.AID[1].AltIDSrcSpecified = true;
            pInstrmt.AID[1].AltID = asset.AssetSymbol;
            pInstrmt.AID[1].AltIDSpecified = true;

            pInstrmt.Src = SecurityIDSourceEnum.Proprietary;
            pInstrmt.SrcSpecified = true;

            pInstrmt.MMY = asset.MaturityMonthYear;
            pInstrmt.MMYSpecified = true;

            pInstrmt.Sym = asset.ContractSymbol;
            pInstrmt.SymSpecified = true;

            pInstrmt.ID = Convert.ToString(asset.IdInstrument);
            pInstrmt.IDSpecified = true;

            pInstrmt.Exch = asset.Market_Identifier;
            pInstrmt.ExchSpecified = true;

            if (asset.putCall_In.HasValue)
            {
                pInstrmt.StrkPx = asset.StrikePrice;
                pInstrmt.StrkPxSpecified = true;

                pInstrmt.PutCall = asset.putCall_In.Value;
                pInstrmt.PutCallSpecified = true;
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
        /// <param name="pCS"></param>
        /// <param name="pTiming">user defined riskl evaluation timing</param>
        /// <param name="pDtBusiness">positions business date</param>
        /// <param name="pPositions">positions to be added</param>
        private FixPositionReport[] BuildPositionReport(string pCS, DateTime pDtBusiness, RiskEvaluationTiming pTiming,
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            FixPositionReport[] positionReports = new FixPositionReport[pPositions.Count()];

            int idxPosition = 0;

            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> position in pPositions)
            {

                PosRiskMarginKey keyPos = position.First;
                RiskMarginPosition valuePos = position.Second;

                FixPositionReport reportPosition = new FixPositionReport();

                reportPosition.RptID = String.Format("Pos{0}", idxPosition + 1);

                if (pDtBusiness > DateTime.MinValue)
                {
                    reportPosition.BizDt = pDtBusiness;
                    reportPosition.BizDtSpecified = true;
                }

                reportPosition.SetSesID = Enum.GetName(typeof(RiskEvaluationTiming), pTiming);

                if (keyPos.idB_Dealer > 0)
                {
                    SQL_Book book = GetSQLBookFromCache(pCS, keyPos.idB_Dealer);
                    reportPosition.Acct = book.Identifier;
                }

                if (keyPos.idA_Dealer > 0)
                {
                    SetParty_Block(pCS, keyPos.idA_Dealer, reportPosition.Pty);
                }

                SetInstrumentBlock(pCS, keyPos, reportPosition.Instrmt);

                // TODO MF 20111005 il est pas possible faire le parsing de l'énumération à partir de SideEnum, il faudrait 
                //  utiliser la base hex avec Borrow financing "G" 0 de l'énum...

                if (keyPos.side == "1")
                {
                    reportPosition.Qty.Long = valuePos.Quantity;
                }
                else if (keyPos.side == "2")
                {
                    reportPosition.Qty.Short = valuePos.Quantity;
                }
                else
                {
                    string message = String.Format(Ressource.GetString("RiskPerformance_WARNINGWrongFixSide"), keyPos.idA_Dealer, reportPosition.RptID);

                    m_Log.Invoke(LevelStatusTools.StatusEnum.ERROR, ErrorManager.DetailEnum.NONE, message, false, null);
                }

                positionReports[idxPosition] = reportPosition;

                idxPosition++;
            }

            return positionReports;
        }

        private MarginCalculationMethod CalculationMethodFactory(RiskMethodType pMethodType, ICalculationSheetMethod pCalculationMethod)
        {
            MarginCalculationMethod calculationMethod = null;

            switch (pMethodType)
            {
                case RiskMethodType.STANDARD:

                    calculationMethod = BuildStdMarginCalculationMethod((StdCalculationSheetMethod)pCalculationMethod);

                    break;

                default:

                    throw new NotSupportedException(String.Format(Ressource.GetString("RiskPerformance_ERRUnknownMethod"), pMethodType));
            }

            return calculationMethod;
        }

        private StdMarginCalculationMethod BuildStdMarginCalculationMethod(StdCalculationSheetMethod pCalculationMethod)
        {
            StdMarginCalculationMethod standardmethod = new StdMarginCalculationMethod();

            standardmethod.name = "Standard";

            standardmethod.parameters = new StdContractParameter[pCalculationMethod.Parameters.Length];

            int idxParameter = 0;

            foreach (ICalculationSheetRiskParameter contractParameter in pCalculationMethod.Parameters)
            {
                StdCalculationSheetRiskContractParameter calculationSheetContractParameter = 
                    (StdCalculationSheetRiskContractParameter)contractParameter;

                StdContractParameter stdContractParameter = new StdContractParameter();

                standardmethod.parameters[idxParameter] = stdContractParameter;

                stdContractParameter.contract.OTCmlId = calculationSheetContractParameter.ContractId;
                stdContractParameter.contract.Id = calculationSheetContractParameter.Identifier;
                stdContractParameter.marginExpressionType = 
                    Enum.GetName(typeof(ExpressionTypeStandardMethod), calculationSheetContractParameter.ExpressionType);

                if (calculationSheetContractParameter.ExpressionType == ExpressionTypeStandardMethod.Percentage)
                {
                    stdContractParameter.quote = calculationSheetContractParameter.Quote;
                    stdContractParameter.quoteSpecified = true;

                    stdContractParameter.multiplier = calculationSheetContractParameter.Multiplier;
                    stdContractParameter.multiplierSpecified = true;
                }

                stdContractParameter.contract.identifier = calculationSheetContractParameter.Identifier;

                stdContractParameter.marginAmount = (Money)calculationSheetContractParameter.MarginAmount;

                stdContractParameter.positions = BuildPositionReport
                    (null, DateTime.MinValue, RiskEvaluationTiming.EndOfDay, calculationSheetContractParameter.Positions);

                stdContractParameter.parameters = new StdAmountParameter[calculationSheetContractParameter.Parameters.Length];

                int idxAmount = 0;

                foreach (ICalculationSheetRiskParameter parameterAmount in calculationSheetContractParameter.Parameters)
                {
                    StdCalculationSheetRiskAmountParameter calculationSheetAmountParameter =
                        (StdCalculationSheetRiskAmountParameter)parameterAmount;
                    
                    StdAmountParameter stdAmountParameter = new StdAmountParameter();

                    stdContractParameter.parameters[idxAmount] = stdAmountParameter;

                    stdAmountParameter.amount = calculationSheetAmountParameter.RiskValue;
                    stdAmountParameter.type = calculationSheetAmountParameter.Type;

                    stdAmountParameter.positions =
                        BuildPositionReport(null, DateTime.MinValue, RiskEvaluationTiming.EndOfDay, parameterAmount.Positions);
                    
                    stdAmountParameter.marginAmount = (Money)parameterAmount.MarginAmount;

                    idxAmount++;
                }

                idxParameter++;
            }

            return standardmethod;
        }

        /// <summary>
        /// Record trade in database
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCaptureGen"></param>
        /// <param name="pRecordSettings"></param>
        /// <param name="pCaptureSession"></param>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        private TradeCommonCaptureGen.ErrorLevel RecordTrade(
            string pCS,
            TradeRiskCaptureGen pCaptureGen,
            TradeRecordSettings pRecordSettings,
            CaptureSessionInfo pCaptureSession,
            Cst.Capture.ModeEnum pCaptureMode)
        {
            //
            try
            {
                TradeCommonCaptureGen.ErrorLevel lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
                //
                string methodName = MethodInfo.GetCurrentMethod().Name;
                string IdMenu = Cst.IdMenu.InputTrade; // UNDONE FI 20110501 Quel est le menu ?? pour l'instant C'est trade
                //
                int idRequest_L = 0;
                string identifier = string.Empty;
                string[] identifierUnderlying = null;
                int[] idUnderlying = null;
                //
                ProcessLog processLog = null;
                TradeCommonCaptureGenException errExc = null;
                try
                {
                    int idT = 0;
                    pCaptureGen.CheckAndRecord(null, IdMenu, pCaptureMode, pCaptureSession, pRecordSettings,
                        ref identifier, ref idT,
                        ref identifierUnderlying, ref idUnderlying,
                        ref idRequest_L, out processLog);
                    //
                    //FI 20100630 Mise à jour de TradeCommonInput.identification puisque utilisé par la suite
                    //L'idéal aurait été de faire appel à la méthode TradeCommonCaptureGen.Load() même cette méthode n'existe pas en mode transactionnel
                    SpheresIdentification identification = TradeRDBMSTools.GetTradeIdentification(pCS, null, idT);
                    pCaptureGen.TradeCommonInput.identification = identification;
                }
                catch (TradeCommonCaptureGenException ex)
                {
                    //Erreur reconnue
                    errExc = ex;
                    lRet = errExc.ErrLevel;
                }
                catch (Exception ex) { throw ex; }//Error non gérée
                //
                //Si une exception se produit après l'enregistrement du trade => cela veut dire que le trade est correctement rentré en base, on est en succès
                if ((null != errExc) && TradeCaptureGen.IsRecordInSuccess(errExc.ErrLevel))
                    lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
                //
                #region Ecrire du fichier XML ds temporary et dans attached doc si erreur ou log en mode full
                if ((TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet) || this.LogDetailEnum == ErrorManager.DetailEnum.FULL)
                {
                    bool isError = (TradeCommonCaptureGen.ErrorLevel.SUCCESS != lRet);
                    string filename = string.Empty;
                    string folder = string.Empty;
                    try
                    {
                        //
                        try
                        {
                            pCaptureGen.WriteTradeXMLOnTemporary(this.AppInstance, out folder, out filename);
                        }
                        catch (Exception ex)
                        {
                            throw new OTCmlException(methodName, "<b>ERROR on writting tradeXML on temporary folder</b>", ex);
                        }
                        //  
                        try
                        {
                            byte[] data = FileTools.ReadFileToBytes(folder + @"\" + filename);
                            m_Attach.Invoke(pCS, data, filename, "xml");
                        }
                        catch (Exception ex)
                        {
                            throw new OTCmlException(methodName, "<b>ERROR on writting tradeXML in AttachedDoc table</b>", ex);
                        }

                        //
                        // Ce message n'apparaît que en mode complet si succes et systematiquement si erreur

                        if (isError)
                        {
                            string message = 
                                String.Format("<b>Trade not conform</b>, for more details please see xml File {0} in the attached documents of the process log", 
                                    filename);

                            m_Log.Invoke(LevelStatusTools.StatusEnum.ERROR, ErrorManager.DetailEnum.NONE, message, false, null);
                        }
                        else
                        {

                            string message =
                                String.Format("Trade conform , see xml File {0} in attached doc", filename);

                            m_Log.Invoke(LevelStatusTools.StatusEnum.NA, ErrorManager.DetailEnum.NONE, message, false, null);
                        }
                    }
                    catch (OTCmlException ex)
                    {
                        m_Log.Invoke(LevelStatusTools.StatusEnum.ERROR, ErrorManager.DetailEnum.NONE, String.Empty, false, ex);
                    }
                }
                #endregion
                //
                return lRet;
            }
            catch (Exception ex) { throw ex; }
        }

        private static void SetDataDocument(
            DataDocumentContainer pDataDoc, SQL_Actor pSqlActor, SQL_Book pSqlBook, SQL_Actor pSqlEntity, SQL_Actor pSqlCss, 
            DateTime pDtBusiness, List<Money> pMoney, RiskEvaluationTiming pRiskEvaluationTiming)
        {

            //Suppression des parties
            pDataDoc.RemoveParty();

            //Ajout des parties
            IParty partyActor = pDataDoc.AddParty(pSqlActor);
            IPartyTradeIdentifier partyActorTradeIdentifier = pDataDoc.AddPartyTradeIndentifier(partyActor.id);
            Tools.SetBookId(partyActorTradeIdentifier.bookId, pSqlBook);
            partyActorTradeIdentifier.bookIdSpecified = true;
            //
            IParty partyEntity = pDataDoc.AddParty(pSqlEntity);
            //
            //Chambre
            IParty partyCss = pDataDoc.AddParty(pSqlCss);
            //
            ITradeHeader tradeHearder = pDataDoc.tradeHeader;
            tradeHearder.tradeDate.Value = DtFunc.DateTimeToString(pDtBusiness, DtFunc.FmtISODate);
            //
            MarginRequirementContainer marginRequirement = new MarginRequirementContainer((IMarginRequirement)pDataDoc.currentProduct.product);
            IProductBase productBase = (IProductBase)marginRequirement.marginRequirement;
            //
            marginRequirement.payment = null;
            marginRequirement.marginRequirementOfficePartyReference = productBase.CreatePartyReference(pSqlActor.xmlId);
            marginRequirement.entityPartyReference = productBase.CreatePartyReference(pSqlEntity.xmlId);  
            marginRequirement.clearingOrganizationPartyReference = productBase.CreatePartyReference(pSqlCss.xmlId);
            marginRequirement.timing = productBase.CreateMarginTiming();
            marginRequirement.timing.Value = pRiskEvaluationTiming.ToString();
            //
            foreach (IMoney money in pMoney)
            {
                ISimplePayment simplePayment = productBase.CreateSimplePayment();
                //
                bool isDealer = true;  // UNDONE FI 20110503 le MRO est-il dealer ? où trouver l'info ?
                if (isDealer)
                {
                    simplePayment.payerPartyReference.hRef = partyActor.id;
                    simplePayment.receiverPartyReference.hRef = partyEntity.id;
                }
                else
                {
                    simplePayment.payerPartyReference.hRef = partyEntity.id;
                    simplePayment.receiverPartyReference.hRef = partyActor.id;
                }
                simplePayment.paymentAmount.amount.DecValue = money.amount.DecValue;
                simplePayment.paymentAmount.currency = money.currency;
                simplePayment.paymentDate.adjustableDateSpecified = true;
                simplePayment.paymentDate.adjustableDate = productBase.CreateAdjustableDate(pDtBusiness, FpML.Enum.BusinessDayConventionEnum.NotApplicable, null);
                //
                marginRequirement.AddPayment(simplePayment);
            }
        }

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
    internal sealed class CalculationSheetWriter
    {

        DataTable m_tableMarginDetTrack = null;

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
        /// header process
        /// </summary>
        ProcessLogHeader m_ProcessHeader = null;

        /// <summary>
        /// Connection string
        /// </summary>
        string m_CS = String.Empty;

        /// <summary>
        /// Loggin delegate method
        /// </summary>
        LogAddDetail m_Log = null;

        /// <summary>
        /// Create a new Log Writer
        /// </summary>
        public CalculationSheetWriter()
        {

        }

        /// <summary>
        /// Initialize the destination objects structure 
        /// </summary>
        public void Init(string pCS, ProcessLogHeader pProcessHeader)
        {
            CreateStructureMarginTrack();

            CreateStructureMarginDetTrack();

            m_ProcessHeader = pProcessHeader;

            m_CS = pCS;

            Initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>how many elements have been written</returns>
        public bool Write(CalculationSheetRepository pRepository, out int opCount, out int opIdMarginTrack)
        {
            bool res = false;

            opCount = 0;
            opIdMarginTrack = 0;

            try
            {

                opIdMarginTrack = AddMarginTrackElement();

                IEnumerable<MarginDetailsDocument> calculationsheetsToWrite =
                    from log
                        in pRepository.CalculationSheets.Values
                    where log.Processed == true
                    select log;

                foreach (MarginDetailsDocument calculationsheet in calculationsheetsToWrite)
                {
                    DataRow newRow = CreateMarginDetTrackElement(opIdMarginTrack, calculationsheet);

                    m_tableMarginDetTrack.Rows.Add(newRow);
                }

                opCount = DataHelper.ExecuteDataAdapter(this.m_CS, "select * from dbo.MARGINDETTRACK where 1 = 0", m_tableMarginDetTrack);

                res = true;

            }
            catch (Exception ex)
            {
                OTCmlException otcmlEx =
                    OTCmlExceptionParser.GetOTCmlException(
                    Ressource.GetString("RiskPerformance_WARNINGNullTradeResultOrCalculationSheetExisting"), ex);

                m_Log.Invoke(
                    LevelStatusTools.StatusEnum.ERROR, ErrorManager.DetailEnum.NONE,
                    Ressource.GetString("RiskPerformance_ExceptionCatched"),
                    false, otcmlEx);
            }
            finally
            {
                m_tableMarginDetTrack.Rows.Clear();
            }

            return res;
        }

        /// <summary>
        /// Init log method
        /// </summary>
        /// <param name="pLog">delegate reference</param>
        public void InitLogAddDetailDelegate(LogAddDetail pLog)
        {
            this.m_Log = pLog;
        }

        private void CreateStructureMarginTrack()
        {
            m_tableMarginTrack = DataHelper<MARGINTRACK>.BuildTableStructure();
        }

        private void CreateStructureMarginDetTrack()
        {
            m_tableMarginDetTrack = DataHelper<MARGINDETTRACK>.BuildTableStructure();
        }

        private int AddMarginTrackElement()
        {
            DataRow newRow = m_tableMarginTrack.NewRow();

            newRow["IDPROCESS_L"] = m_ProcessHeader.IdProcess;

            m_tableMarginTrack.Rows.Add(newRow);

            DataHelper.ExecuteDataAdapter(this.m_CS, "select * from dbo.MARGINTRACK where 1 = 0", m_tableMarginTrack);

            m_tableMarginTrack.Rows.Clear();

            // retrieving last IDMARGINTRACK (relative to the current process)

            return GetIdMarginTrack();
        }

        private int GetIdMarginTrack()
        {
            int idMarginTrack = 0;

            using (IDbConnection connection = DataHelper.OpenConnection(m_CS))
            {

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("IDPROCESS_L", m_ProcessHeader.IdProcess);

                List<MARGINTRACK> elems = DataHelper<MARGINTRACK>.ExecuteDataSet(
                    connection,
                    DataContractHelper.GetType(DataContractResultSets.PROCESSMARGINTRACK),
                    DataContractHelper.GetQuery(DataContractResultSets.PROCESSMARGINTRACK),
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.PROCESSMARGINTRACK, parameters));

                if (elems.Count != 1)
                {
                    throw new NotSupportedException(
                        String.Format(@"
                            {0} MARGINTRACK element(s) found for the current process. Writing log process aborted.", elems.Count));
                }

                idMarginTrack = (int)elems[0].IdMarginTrack;
            }

            return idMarginTrack;
        }

        private DataRow CreateMarginDetTrackElement(int pIdMarginTrack, MarginDetailsDocument pLog)
        {
            DataRow newRow = m_tableMarginDetTrack.NewRow();

            //byte[] objectArray = SerializationHelper.DumpObjectToByteArray<MarginDetailsDocument>(pLog, this.m_Log);

            StringBuilder xmlstream = SerializationHelper.DumpObjectToString<MarginDetailsDocument>(pLog, this.m_Log);

            newRow["IDMARGINTRACK"] = pIdMarginTrack;
            newRow["IDT"] = pLog.IdTechTrade;
            newRow["MARGINXML"] = xmlstream;

            return newRow;
        }

    }


    public interface ICalculationSheetRiskParameter 
    {
      
        IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        ICalculationSheetRiskParameter[] Parameters { get; set; }

        IMoney MarginAmount { get; set; }
    }

    public interface ICalculationSheetMethod  
    {
        ICalculationSheetRiskParameter[] Parameters { get; set; }
    }

    public class StdCalculationSheetMethod : ICalculationSheetMethod
    {
        #region ICalculationSheetMethod Membres

        public ICalculationSheetRiskParameter[] Parameters
        {
            get;

            set;
        }

        #endregion
    }

    public class StdCalculationSheetRiskAmountParameter : ICalculationSheetRiskParameter
    {
        public decimal RiskValue
        {
            get;

            set;
        }

        public string Type
        {
            get;

            set;
        }

        #region ICalculationSheetRiskParameter Membres

        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        public ICalculationSheetRiskParameter[] Parameters
        {
            get;

            set;
        }

        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion
    }

    public class StdCalculationSheetRiskContractParameter : ICalculationSheetRiskParameter
    {

        public int ContractId
        {
            get;

            set;
        }

        public string Identifier
        {
            get;

            set;
        }

        public ExpressionTypeStandardMethod ExpressionType
        {
            get;

            set;
        }

        public decimal Quote
        {
            get;

            set;
        }

        public decimal Multiplier
        {
            get;

            set;
        }


        #region ICalculationSheetRiskParameter Membres

        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        public ICalculationSheetRiskParameter[] Parameters
        {
            get;

            set;
        }

        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion
    }

}
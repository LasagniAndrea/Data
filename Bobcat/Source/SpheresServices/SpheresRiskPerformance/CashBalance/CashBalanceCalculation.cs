using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.Properties;
using EFS.TradeInformation;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.CashBalance;
using EfsML.v30.Fix;
//
using FixML.Enum;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    
    /// <summary>
    /// 
    /// </summary>
    /// EG 20131216 [19342] New
    public class CBTemplateInfo
    {
        #region Members
        public string IdMenu { get; set; }
        public User User { get; set; }
        public CaptureSessionInfo SessionInfo { get; set; }
        public InputUser InputUser { get; set; }
        public int IdTTemplate { get; set; }
        /// <summary>
        ///  Obtient l'écran par défaut associé au template
        /// </summary>
        /// FI 20131223 [19389] add property
        public string ScreenTemplate { get; private set; }
        public int IdTSource { get; set; }
        public string ScreenName { get; set; }
        public TradeRiskCaptureGen CaptureGen { get; set; }
        #endregion Members
        #region Constructor
        public CBTemplateInfo(ProcessBase pProcess)
        {
            //PL 20140930
            //idMenu = IdMenu.GetIdMenu(IdMenu.Menu.IntputTradeRisk);
            IdMenu = EFS.ACommon.IdMenu.GetIdMenu(EFS.ACommon.IdMenu.Menu.InputTradeRisk_CashBalance);

            //CaptureSessionInfo
            SessionInfo = new CaptureSessionInfo
            {
                user = new User(pProcess.Session.IdA, null, RoleActor.SYSADMIN),
                session = pProcess.Session,
                licence = pProcess.License,
                idProcess_L = pProcess.IdProcess,
                idTracker_L = pProcess.Tracker.IdTRK_L
            };

            InputUser = new InputUser(IdMenu, User);
            InputUser.InitializeFromMenu(CSTools.SetCacheOn(pProcess.Cs));
            InputUser.InitCaptureMode();

            
            //SQL_Instrument sqlInstrument = new SQL_Instrument(sessionInfo.processLog.Cs, Cst.ProductCashBalance, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty);
            SQL_Instrument sqlInstrument = new SQL_Instrument(pProcess.Cs, Cst.ProductCashBalance, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty);
            if (false == sqlInstrument.LoadTable(new string[] { "IDI,IDENTIFIER" }))
                throw new NotSupportedException(StrFunc.AppendFormat("Instrument {0} not found", Cst.ProductCashBalance));

            SearchInstrumentGUI searchInstrumentGUI = new SearchInstrumentGUI(sqlInstrument.Id);
            
            //StringData[] data = searchInstrumentGUI.GetDefault(sessionInfo.processLog.Cs, false);
            StringData[] data = searchInstrumentGUI.GetDefault(pProcess.Cs, false);
            if (ArrFunc.IsEmpty(data))
                throw new NotSupportedException(StrFunc.AppendFormat("Screen or template not found for Instrument {0}", sqlInstrument.Identifier));
            ScreenName = ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value;

            // FI 20131223 [19389]
            ScreenTemplate = ScreenName;

            string templateIdentifier = ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value;
            
            //idTTemplate = TradeRDBMSTools.GetTradeIdT(sessionInfo.processLog.Cs, templateIdentifier);
            IdTTemplate = TradeRDBMSTools.GetTradeIdT(pProcess.Cs, templateIdentifier);
            if (0 == IdTTemplate)
                throw new NotSupportedException("Trade Source not found");

            CaptureGen = new TradeRiskCaptureGen();
            //FI 20131223 [19389] appel à CSTools.SetCacheOn
            
            //if (false == captureGen.Load(CSTools.SetCacheOn(sessionInfo.processLog.Cs), null, idTTemplate.ToString(), SQL_TableWithID.IDType.Id, inputUser.CaptureMode, user, sessionInfo.appInstance.SessionId, false))
            if (false == CaptureGen.Load(CSTools.SetCacheOn(pProcess.Cs), null, IdTTemplate.ToString(), SQL_TableWithID.IDType.Id, InputUser.CaptureMode, User, SessionInfo.session.SessionId, false))
            {
                throw new NotSupportedException(StrFunc.AppendFormat("<b>Trade [idT:{0}] not found</b>", IdTTemplate));
            }
        }
        #endregion Constructor
        #region Methods
        public void Complete(string pCS, int pIdTSource)
        {
            if (0 < pIdTSource)
            {
                IdTSource = pIdTSource;
                InputUser.CaptureMode = Cst.Capture.ModeEnum.Update;
                if (false == CaptureGen.Load(pCS, null, IdTSource.ToString(), SQL_TableWithID.IDType.Id, InputUser.CaptureMode, User, SessionInfo.session.SessionId, false))
                    throw new NotSupportedException(StrFunc.AppendFormat("<b>Trade [idT:{0}] not found</b>", IdTSource));
                ScreenName = CaptureGen.TradeCommonInput.SQLLastTradeLog.ScreenName;
            }
            else
            {
                // FI 20131223 [19389] valorisation de captureMode, Appel à captureGen.Load et alimentation de screenName
                InputUser.CaptureMode = Cst.Capture.ModeEnum.New;

                // Chgt du template si le trade le précédent appel à Complete était une modification de CashBalance déjà existant
                if (CaptureGen.TradeCommonInput.SQLTrade.IdT != IdTTemplate)
                    CaptureGen.Load(CSTools.SetCacheOn(pCS), null, IdTTemplate.ToString(), SQL_TableWithID.IDType.Id, InputUser.CaptureMode, User, SessionInfo.session.SessionId, false);

                ScreenName = ScreenTemplate;

                //En création: Spheres® ecrase systématiquement le StatusEnvironment issu du template par REGULAR
                CaptureGen.TradeCommonInput.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
            }
            // EG 20140217 Réinitialisation de TradeLink
            CaptureGen.TradeCommonInput.TradeLink = new List<TradeLink.TradeLink>();
            CaptureGen.InitBeforeCaptureMode(pCS, null, InputUser, SessionInfo);
        }
        #endregion Methods
    }
    /// <summary>
    /// Représente un couple (CBO/MRO, Book) et tous les montants, qui seront injectés dans le Trade Cash-Balance final
    /// </summary>
    /// PM 20140905 [20066][20185] Gestion méthode UK (ETD & CFD) Add CashFlowRMG_FlowCur, CashFloUMG_FlowCur, CashFlowFDA_FlowCur, CashFlowAll_FlowCur
    /// PM 20140908 [20066][20185] Gestion méthode UK (ETD & CFD) Rename OtherFlowLOV_FlowCur to CashFlowLOV_FlowCur
    [XmlRoot(ElementName = "Trade")]
    // EG 20180205 [23769] Partial
    // EG 20190114 Add detail to ProcessLog Refactoring
    public partial class CBTradeInfo
    {
        #region members
        /// <summary>
        /// 
        /// </summary>
        private SQL_Actor _sqlEntity;
        /// <summary>
        /// 
        /// </summary>
        private SQL_Actor _sqlActor;
        /// <summary>
        /// 
        /// </summary>
        private SQL_Book _sqlBook;

        /// <summary>
        /// Devise comptable
        /// <para>- soit la devise comptable de l'entité si elle existe</para>
        /// <para>- ou bien la devise "EUR" par défaut</para>
        /// </summary>
        private string _accountingCurrency;

        

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "ida_Entity")]
        public int Ida_Entity;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "identifier_Entity")]
        public string Identifier_Entity;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "dtBusiness")]
        public DateTime DtBusiness;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "dtPrevBusiness")]
        public DateTime DtBusinessPrev;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "idtPrev")]
        public int PrevCashBalanceIdt;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "identifierTradePrev")]
        public string PrevCashBalanceIdentifier;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "timing")]
        public SettlSessIDEnum Timing;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "partyTradeInfo")]
        public CBPartyTradeInfo PartyTradeInfo;

        /// <summary>
        /// Collection des différents trades de marché sources des différents flux de Cash-Flow de statut valid
        /// </summary>
        [XmlArray(ElementName = "tradesCashFlow")]
        [XmlArrayItemAttribute("trade")]
        public List<Pair<int, string>> TradesCashFlowSource;

        /// <summary>
        /// Collection des différents trades de marché sources des différents flux de Cash-Flow (statut valid et unvalid)
        /// <para>Les trades de statut Valid rentrent dans le solde (Trade CB)</para>
        /// <para>Les trades de statut Unvalid ne rentrent pas dans le solde (Trade CB)</para>
        /// <para>Cette collection contient uniquement les trades du jour</para>
        /// </summary>
        /// FI 20170208 [22151][22152] Add
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public List<Pair<int, string>> TradesCashFlowSourceAllStatus;

        /// <summary>
        /// Collection des différents trades marginRequirement sources des différents flux de l'encours déposit
        /// </summary>
        [XmlArray(ElementName = "tradesDeposit")]
        [XmlArrayItemAttribute("trade")]
        public List<Pair<int, string>> TradesDepositSource;

        /// <summary>
        /// Collection des différents trades bulletPayment sources des différents flux de des mouvements éspèces
        /// </summary>
        [XmlArray(ElementName = "tradesCashPayment")]
        [XmlArrayItemAttribute("trade")]
        public List<Pair<int, string>> TradesCashPaymentSource;

        #region CBFlows Cash Flow
        /// <summary>
        /// Cash-Flow: Borrowing en devise d'origine
        /// </summary>
        ///PM 20150323 [POC] Add CashFlowBWA_FlowCur
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName ="borrowings")]
        public CBFlows CashFlowBWA_FlowCur;

        /// <summary>
        /// Cash-Flow: Equalisation Payment en devise d'origine
        /// </summary>
        /// PM 20170911 [23408] Ajout CashFlowEQP_FlowCur
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "equalisationPayments")]
        public CBFlows CashFlowEQP_FlowCur;

        /// <summary>
        /// Cash-Flow: Funding en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "fundings")]
        public CBFlows CashFlowFDA_FlowCur;

        /// <summary>
        /// Cash-Flow: Liquidative Option Value en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "liquidativeOptionValues")]
        public CBFlows CashFlowLOV_FlowCur;

        /// <summary>
        /// Cash-Flow: Market Value en devise d'origine
        /// </summary>
        /// PM 20150616 [21124] Add CashFlowMKV_FlowCur
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "marketValues")]
        public CBFlows CashFlowMKV_FlowCur;

        /// <summary>
        /// Cash-Flow: Tous les frais et taxes en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "fees")]
        public CBFlows CashFlowOPP_FlowCur;

        /// <summary>
        /// Cash-Flow: Primes en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "premiums")]
        public CBFlows CashFlowPRM_FlowCur;

        /// <summary>
        /// Cash-Flow: Realized Margin en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "realizedMargins")]
        public CBFlows CashFlowRMG_FlowCur;

        /// <summary>
        /// Cash-Flow: Cash Settlement en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction bug dans "DumpObjectToFile" par ajout ou correction d'attribut de sérialization
        [XmlElement(ElementName = "cashsettlements")]
        public CBFlows CashFlowSCU_FlowCur;

        /// <summary>
        /// Cash-Flow: Safe Keeping Payment en devise d'origine
        /// </summary>
        /// PM 20150709 [21103] Add CashFlowSKP_FlowCur
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "safeKeepingPayments")]
        public CBFlows CashFlowSKP_FlowCur;

        /// <summary>
        /// Cash-Flow: Unrealized Margin
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "unrealizedMargins")]
        public CBFlows CashFlowUMG_FlowCur;

        /// <summary>
        /// Cash-Flow: Unsettled Transaction
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "unsettledTransactions")]
        public CBFlows CashFlowUST_FlowCur;

        /// <summary>
        /// Cash-Flow: liste des Marges en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "variationMargins")]
        public CBFlows CashFlowVMG_FlowCur;

        /// <summary>
        /// Cash-Flow: Tous les flux
        /// <para>Utile pour avoir dans un seul ensemble tous les assets et toutes les devises de tous les flux</para>
        /// </summary>
        [XmlIgnore]
        public CBFlows CashFlowAll_FlowCur;
        #endregion CBFlows Cash Flow

        /// <summary>
        /// Versements, retraits en devise d'origine et en date STL du jour
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "stlPayments")]
        public CBFlows PaymentStl_FlowCur;

        /// <summary>
        /// Versements, retraits en devise d'origine et en date STL à venir
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlElement(ElementName = "stlForwardPayments")]
        public CBFlows ForwardPaymentStl_FlowCur;

        /// <summary>
        /// Versements, retraits en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "payments")]
        [XmlArrayItemAttribute("payment")]
        public List<Money> Payment_FlowCur;

        /// <summary>
        /// Soldes "Précédent" en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "previousCashsBalance")]
        [XmlArrayItemAttribute("previousCashBalance")]
        public List<Money> PrevCashBalance_FlowCur;

        /// <summary>
        /// Défaut Déposit "Précédent" en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlArray(ElementName = "previousUncoveredMarginRequirements")]
        [XmlArrayItemAttribute("marginRequirement")]
        public List<Money> PrevDefectDeposit_FlowCur;

        /// <summary>
        /// Déposits "Précédent" en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlArray(ElementName = "previousMarginRequirements")]
        [XmlArrayItemAttribute("marginRequirement")]
        public List<Money> PrevDeposit_FlowCur;

        /// <summary>
        /// Espèces disponibles "Précédent" en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "previousCashsAvailable")]
        [XmlArrayItemAttribute("cashAvailable")]
        public List<Money> PrevCashAvailable_FlowCur;

        /// <summary>
        /// Espèces utilisées "Précédent" en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "previousCashsUsed")]
        [XmlArrayItemAttribute("cashUsed")]
        public List<Money> PrevCashUsed_FlowCur;

        /// <summary>
        /// Collaterals disponibles "Précédent" en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "previousColateralsAvailable")]
        [XmlArrayItemAttribute("colateralAvailable")]
        public List<Money> PrevCollatAvailable_FlowCur;

        /// <summary>
        /// Collaterals utilisés "Précédent" en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "previousColateralsUsed")]
        [XmlArrayItemAttribute("colateralUsed")]
        public List<Money> PrevCollatUsed_FlowCur;

        /// <summary>
        /// Tous les Deposits en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlArray(ElementName = "marginRequirements")]
        [XmlArrayItemAttribute("marginRequirement")]
        public List<CBDetDeposit> FlowDeposit;

        /// <summary>
        /// Tous les Montants Globaux de Deposits restant à couvrir en devise d'origine
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlArray(ElementName = "defectMarginRequirements")]
        [XmlArrayItemAttribute("defectMarginRequirement")]
        public List<Money> GlobalDefectDeposits_FlowCur;

        /// <summary>
        /// Le Montant Global de Deposits restant à couvrir en devise de contrevaleur du CBO
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("defectMarginRequierement")]
        public Money GlobalDefectDeposits_CTRValCur;

        /// <summary>
        /// Espèces disponibles en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "cashsAvailable")]
        [XmlArrayItemAttribute("cashAvailable")]
        public List<Money> CashAvailable_FlowCur;

        /// <summary>
        /// Les Contrevaleurs de toutes les espèces disponibles qui ont une devise d'origine différente de la devise de contrevaleur du CBO (CBO.EXCHAGEIDC)
        /// <para>- La Contrevaleur est calculée dans la devise de contrevaleur du CBO</para>
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlArray(ElementName = "exchangeCashsAvailable")]
        [XmlArrayItemAttribute("cashAvailable")]
        public List<CBExAmount> CashAvailable_ExCTRValCur;

        /// <summary>
        /// La somme de toutes les espèces disponibles confondues dans la devise de contrevaleur du CBO
        /// <para>- Donc les espèces disponibles déjà en devise de contrevaleur du CBO,</para>
        /// <para>- Plus la contrevaleur des espèces disponibles qui ont une devise d'origine différente de la devise de contrevaleur du CBO</para>
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("cashAvailable")]
        public Money CashAvailable_CTRValCur;

        /// <summary>
        /// Espèces en devise d'origine, utilisées pour la couverture du déposit
        /// </summary>
        [XmlArray(ElementName = "cashsUsed")]
        [XmlArrayItemAttribute("cashUsed")]
        public List<Money> CashUsed_FlowCur;

        /// <summary>
        /// Espèces en devise de contrevaleur du CBO, utilisées pour la couverture du déposit
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("cashUsed")]
        public Money CashUsed_CTRValCur;

        /// <summary>
        /// Détails des Collaterals disponibles en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "colateralsAvailable")]
        [XmlArrayItemAttribute("colateralAvailableDetail")]
        public List<CBDetCollateral> FlowCollateral;

        /// <summary>
        /// Tous les Appels/Restitution en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "marginsCall")]
        [XmlArrayItemAttribute("marginCall")]
        public List<Money> MarginCall_FlowCur;

        /// <summary>
        /// Appels/Restitution en devise de contrevaleur du CBO
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("marginCall")]
        public Money MarginCall_CTRValCur;

        /// <summary>
        /// Soldes en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "cashsBalance")]
        [XmlArrayItemAttribute("cashBalance")]
        public List<Money> CashBalance_FlowCur;

        /// <summary>
        /// Liste des devises avec leurs priorités
        /// </summary>
        [XmlArray(ElementName = "currencyPriorities")]
        [XmlArrayItemAttribute("currencyPriority")]
        public List<CBCollateralCurrencyPriority> CurrencyPriority;

        /// <summary>
        /// Equity Balance en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "equityBalances")]
        [XmlArrayItemAttribute("equityBalance")]
        public List<Money> EquityBalance_FlowCur;

        /// <summary>
        /// Equity Balance With Forward Cash en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "equityBalancesWithForwardCash")]
        [XmlArrayItemAttribute("equityBalanceWithForwardCash")]
        public List<Money> EquityBalanceForwardCash_FlowCur;

        /// <summary>
        /// Total Account Value en devise d'origine
        /// </summary>
        /// PM 20150616 [21124] Add TotalAccountValue_FlowCur
        [XmlArray(ElementName = "totalAccountValues")]
        [XmlArrayItemAttribute("totalAccountValue")]
        public List<Money> TotalAccountValue_FlowCur;

        /// <summary>
        /// Excess / Deficit en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "excessDeficits")]
        [XmlArrayItemAttribute("excessDeficit")]
        public List<Money> ExcessDeficit_FlowCur;

        /// <summary>
        /// Excess / Deficit With Forward Cash en devise d'origine
        /// </summary>
        [XmlArray(ElementName = "excessDeficitsWithForwardCash")]
        [XmlArrayItemAttribute("excessDeficitWithForwardCash")]
        public List<Money> ExcessDeficitForwardCash_FlowCur;

        /// <summary>
        /// Ensemble des montants contrevalorisé (dans le cas d'un cash balance en méthode 2 (CSBUK))
        /// </summary>
        public CBCtrValTradeInfo CtrValTradeInfo;

        /// <summary>
        ///  Etats des process EndOfDay 
        /// </summary>
        /// FI 20161027 [22151] Add
        public EndOfDayStatus endOfDayStatus;

        #endregion members

        #region accessors
        /// <summary>
        /// Le book de l'acteur du Trade qui va porter les montants
        /// </summary>
        [XmlAttribute(AttributeName = "idb")]
        public int Idb { get { return PartyTradeInfo.Idb; } }

        /// <summary>
        /// L'acteur (party) du Trade, vis-à-vis de L'entity
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int Ida { get { return PartyTradeInfo.Ida; } }

        /// <summary>
        /// Actor CBO avec Methode de calcul des Appel/Rest. = MGCCOLLATCUR (Déposit et couverture en devise)
        /// <para>(équivaut au Calcul MonoDevise dans Eurosys)</para>
        /// </summary>
        [XmlAttribute(AttributeName = "isWithMGCCollatCur")]
        public bool IsCBOWithMGCCollatCur { get { return (PartyTradeInfo.ActorCBO.IsCBOWithMGCCollatCur); } }
         
        /// <summary>
        /// Actor CBO avec Methode de calcul des Appel/Rest. = MGCCTRVAL (Déposit et couverture en contrevaleur)
        /// <para>(équivaut au Calcul MultiDevise dans Eurosys)</para>
        /// </summary>
        [XmlAttribute(AttributeName = "isWithMGCCTRVal")]
        public bool IsCBOWithMGCCTRVal { get { return (PartyTradeInfo.ActorCBO.IsCBOWithMGCCTRVal); } }

        /// <summary>
        /// Actor CBO avec Methode de calcul des Appel/Rest. = MGCCOLLATCTRVAL (Déposit en devise et couverture en contrevaleur)
        /// </summary>
        [XmlAttribute(AttributeName = "isWithMGCCollatCTRVal")]
        public bool IsCBOWithMGCCollatCTRVal { get { return (PartyTradeInfo.ActorCBO.IsCBOWithMGCCollatCTRVal); } }

        /// <summary>
        /// Devise à utiliser dans le cas ou la méthode de calcul des Appels/Restitutions est:
        /// <para>- MGCCTRVAL (Déposit et couverture en contrevaleur): utilisée comme devise de contrevaleur</para>
        /// <para>- MGCCOLLATCTRVAL (Déposit en devise et couverture en contrevaleur): utilisée comme devise pivot pour le calcul des contresvaleurs</para>
        /// </summary>
        [XmlAttribute(AttributeName = "exchangeIDC")]
        public string ExchangeIDC { get { return PartyTradeInfo.ActorCBO.BusinessAttribute.ExchangeIDC; } }

        /// <summary>
        /// Actor CBO avec IsUseAvailableCash=True
        /// </summary>
        [XmlAttribute(AttributeName = "isWithUseAvailableCash")]
        public bool IsCBOWithUseAvailableCash { get { return (PartyTradeInfo.ActorCBO.IsCBOWithUseAvailableCash); } }

        /// <summary>
        /// True: Actor CBO avec gestion des soldes, afin que chaque solde précédent soit pris en compte lors du calcul d'un nouveau solde. 
        /// <para>False: Actor CBO sans gestion des soldes. Dans ce cas, chaque calcul d'un nouveau solde considèrera un solde précédent à zéro.</para>
        /// </summary>
        [XmlAttribute(AttributeName = "isWithManagementBalance")]
        public bool IsCBOWithManagementBalance { get { return (PartyTradeInfo.ActorCBO.BusinessAttribute.IsManagementBalance); } }

        /// <summary>
        /// Retourne True si l'acteur est Clearer
        /// <para>Considérer le MRO, si le Trade porte sur le MRO
        /// sinon considérer le CBO</para>
        /// </summary>
        [XmlAttribute(AttributeName = "isClearer")]
        public bool IsClearer
        {
            get
            {
                if (PartyTradeInfo.ActorMRO != null)
                {
                    return PartyTradeInfo.ActorMRO.IsClearer;
                }
                else
                {
                    return PartyTradeInfo.ActorCBO.IsClearer;
                }
            }
        }

        /// <summary>
        /// Retourne True si l'acteur n'est pas un Clearer (Client, ...)
        /// <para>Considérer le MRO, si le Trade porte sur le MRO
        /// sinon considérer le CBO</para>
        /// </summary>
        [XmlIgnore]
        public bool IsNotClearer { get { return (IsClearer == false); } }

        /// <summary>
        /// Obtient la liste des devises présentes sur les différents flux
        /// </summary>
        /// FI 20120725 [18009] modifications diverses 
        ///PM 20140912 [20066][20185] Gestion méthode UK
        [XmlIgnore]
        public List<string> Currencies
        {
            get
            {
                List<string> distinctCurrency =
                    CashFlowOPP_FlowCur.DistinctCurrency()
                    .Concat(CashFlowPRM_FlowCur.DistinctCurrency())
                    .Concat(CashFlowVMG_FlowCur.DistinctCurrency())
                    .Concat(CashFlowAll_FlowCur.DistinctCurrency())
                    .Concat(PaymentStl_FlowCur.DistinctCurrency())
                    .Concat(ForwardPaymentStl_FlowCur.DistinctCurrency())
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(Payment_FlowCur))
                    .Concat(from collateral in FlowCollateral
                           where collateral.IsAllowed_AtLeastOneCSS
                           from money in collateral.CurrencyAmount
                           where StrFunc.IsFilled(money.Currency)
                           select money.Currency)
                    .Concat(from deposit in FlowDeposit
                           from money in deposit.CurrencyAmount
                           where StrFunc.IsFilled(money.Currency)
                           select money.Currency)
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevCashBalance_FlowCur))
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevDefectDeposit_FlowCur))
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevDeposit_FlowCur))
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevCashAvailable_FlowCur))
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevCashUsed_FlowCur))
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevCollatAvailable_FlowCur))
                    .Concat(CBFlowTools.MoneyListDistinctCurrency(PrevCollatUsed_FlowCur))
                    .Distinct().ToList();
                //
                return distinctCurrency;
            }
        }

        [XmlIgnore]
        public SQL_Actor SqlEntity
        {
            get { return _sqlEntity; }
        }

        [XmlIgnore]
        public SQL_Actor SqlActor
        {
            get { return _sqlActor; }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public SQL_Book SqlBook
        {
            get { return _sqlBook; }
        }

        /// <summary>
        /// Returns true if there is at least one not zero flow 
        /// </summary>
        /// FI 20120913 [18125] new property
        [XmlIgnore]
        public bool IsAmountFilled
        {
            get
            {
                bool ret = false;
                if (ArrFunc.IsFilled(Currencies))
                {
                    foreach (string currency in Currencies)
                    {
                        if (IsAmountCurrencyFilled(currency))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// Méthode de calcul du Cash Balance
        /// </summary>
        ///PM 20140910 [20066][20185] Add CbCalcMethod
        [XmlAttribute(AttributeName = "cbCalcMethod")]
        public CashBalanceCalculationMethodEnum CbCalcMethod
        {
            get { return (PartyTradeInfo.ActorCBO.BusinessAttribute.CbCalcMethod); }
        }

        /// <summary>
        /// Devise de contrevaleur à utiliser dans le cas d'un calcul du cash balance par la méthode 2 (Méthode CSBUK)
        /// </summary>
        [XmlAttribute(AttributeName = "cbIDC")]
        public string CashBalanceIDC
        {
            get { return PartyTradeInfo.ActorCBO.BusinessAttribute.CbIDC; }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200623 [XXXXX] Add
        [XmlIgnoreAttribute()]
        public SetErrorWarning SetErrorWarning
        {
            get;
            private set;
        }


        #endregion accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CBTradeInfo() { }

        /// <summary>
        /// Initialise les informations à injecter dans le Trade Cash-Balance, 
        /// et les différents montants non calculés du Trade, à partir, soit du MRO soit du CBO
        /// </summary>
        /// <param name="pIda_Entity"></param>
        /// <param name="pIdentifier_Entity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtBusinessPrev"></param>
        /// <param name="pTiming"></param>
        /// <param name="pPartyTradeInfo"></param>
        /// FI 20170208 [22151][22152] Modify
        public CBTradeInfo(int pIda_Entity, string pIdentifier_Entity, DateTime pDtBusiness, DateTime pDtBusinessPrev,
            SettlSessIDEnum pTiming, CBPartyTradeInfo pPartyTradeInfo)
        {
            Ida_Entity = pIda_Entity;
            Identifier_Entity = pIdentifier_Entity;
            DtBusiness = pDtBusiness;
            DtBusinessPrev = pDtBusinessPrev;
            Timing = pTiming;
            //
            PartyTradeInfo = pPartyTradeInfo;
            //
            PrevCashBalanceIdt = 0;
            PrevCashBalanceIdentifier = string.Empty;
            //
            TradesCashFlowSource = new List<Pair<int, string>>();
            // FI 20170208 [22151][22152] new  TradesCashFlowSourceAllStatus
            TradesCashFlowSourceAllStatus = new List<Pair<int, string>>();
            TradesDepositSource = new List<Pair<int, string>>();
            TradesCashPaymentSource = new List<Pair<int, string>>();
            //
            CashFlowBWA_FlowCur = null;
            // PM 20170911 [23408] Add CashFlowEQP_FlowCur
            CashFlowEQP_FlowCur = null;
            CashFlowFDA_FlowCur = null;
            CashFlowLOV_FlowCur = null;
            CashFlowMKV_FlowCur = null;
            CashFlowOPP_FlowCur = null;
            CashFlowPRM_FlowCur = null;
            CashFlowRMG_FlowCur = null;
            CashFlowSCU_FlowCur = null;
            // PM 20150709 [21103] Add CashFlowSKP_FlowCur
            CashFlowSKP_FlowCur = null;
            CashFlowUMG_FlowCur = null;
            CashFlowUST_FlowCur = null;
            CashFlowVMG_FlowCur = null;
            //
            CashFlowAll_FlowCur = null;
            //
            PaymentStl_FlowCur = null;
            ForwardPaymentStl_FlowCur = null;
            //
            Payment_FlowCur = null;
            //
            PrevCashBalance_FlowCur = null;
            PrevDefectDeposit_FlowCur = null;
            PrevDeposit_FlowCur = null;
            PrevCashAvailable_FlowCur = null;
            PrevCashUsed_FlowCur = null;
            PrevCollatAvailable_FlowCur = null;
            PrevCollatUsed_FlowCur = null;
            //
            FlowDeposit = null;
            //
            GlobalDefectDeposits_FlowCur = null;
            GlobalDefectDeposits_CTRValCur = null;
            //
            CashAvailable_FlowCur = null;
            CashAvailable_ExCTRValCur = null;
            CashAvailable_CTRValCur = null;
            CashUsed_FlowCur = null;
            CashUsed_CTRValCur = null;
            //
            FlowCollateral = null;
            //
            MarginCall_FlowCur = null;
            MarginCall_CTRValCur = null;
            CashBalance_FlowCur = null;
            //
            EquityBalance_FlowCur = null;
            EquityBalanceForwardCash_FlowCur = null;
            TotalAccountValue_FlowCur = null;
            ExcessDeficit_FlowCur = null;
            ExcessDeficitForwardCash_FlowCur = null;

            endOfDayStatus = null;
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// Attribue à chaque couverture 1 à n instructions de couverture
        /// <para>Si la couverture s'applique uniquement à une chambre de compensation (collateral.Ida_CssSpecified), alors seule 1 instructions est retenues</para>
        /// <para>sinon 1 instruction par chambre</para>
        /// <para>Mise en place d'une instruction par défaut si aucune instruction n'est paramétrée pour le couple Acteur/entité</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsClearer">si pIsClearer l'entité est l'acteur déposant, sinon est dépositaire</param>
        /// <param name="pIda">Acteur CBO/MRO</param>
        /// <param name="pIda_Entity">Entité</param>
        /// <param name="pRoleActor">rôles associés à l'acteur CBO/MRO</param>
        /// <param name="pCollaterals">Liste des couvertures</param>
        /// FI 20160530 [21885] Modify (static Method)
        // EG 20180205 [23769] Add dbTransaction  
        private static void CheckCollateralEnv(string pCS, IDbTransaction pDbTransaction, Boolean pIsClearer,
            int pIda, int pIda_Entity, List<RoleActor> pRoleActor, List<CBDetCollateral> pCollaterals)
        {
            if (ArrFunc.IsFilled(pCollaterals))
            {
                //Acteur Déposant 
                int ida_Pay = (pIsClearer ? pIda_Entity : pIda);
                int ida_Rec = (pIsClearer ? pIda : pIda_Entity);
                //
                RoleActor[] roles_Pay = (pIsClearer ? new[] { RoleActor.ENTITY } : pRoleActor.ToArray());
                RoleActor[] roles_rec = (pIsClearer ? pRoleActor.ToArray() : new[] { RoleActor.ENTITY });
                //
                //Chargement et tri des instructions de couverture relatives aux acteurs déposants/depositaires 
                CBCollateralEnvs collateralEns = new CBCollateralEnvs();
                collateralEns.LoadCollateralEnv(pCS, pDbTransaction, ida_Pay, roles_Pay, ida_Rec, roles_rec);
                collateralEns.Sort();
                //
                //Alimenation de chaque collateral avec un array d'instructions retenues
                foreach (CBDetCollateral collateral in pCollaterals)
                {
                    if (ArrFunc.IsEmpty(collateralEns.collateralEnv))
                    {
                        //Ne rien faire, collateral.collateralEnv est alimenté avec la valeur par défaut
                    }
                    else if (collateral.Ida_CssSpecified)
                    {
                        //Lorsque la couverture s'applique sur 1 chambre, Spheres® récupère uniquement 1 instruction (la + prioritaire)
                        List<CBCollateralEnv> lstResult =
                            (from collateralEnvItem in new List<CBCollateralEnv>(collateralEns.collateralEnv)
                             where (
                                    ((false == collateralEnvItem.CssId.HasValue) ||
                                    collateralEnvItem.CssId == collateral.Ida_Css)
                                    &&
                                    (collateralEnvItem.AssetCategory.Value.ToString() == collateral.AssetCategory)
                                    &&
                                    ((false == collateralEnvItem.IdAsset.HasValue) ||
                                        collateralEnvItem.IdAsset == collateral.IdAsset)
                                    )
                             orderby collateralEnvItem.Weight
                             select collateralEnvItem).ToList();
                        //
                        if (ArrFunc.IsFilled(lstResult))
                        {
                            collateral.collateralEnv = new List<CBCollateralEnv>
                            {
                                lstResult.First()
                            };
                        }
                    }
                    else if (false == collateral.Ida_CssSpecified)
                    {
                        //Lorsque la couverture s'applique sur n chambres, Spheres® récupère les n instructions prioritaires (1 par chambre)
                        //
                        //Liste des chambres présentes ds les instructions de couverture
                        List<Nullable<int>> css =
                            (from collateralEnvItem in new List<CBCollateralEnv>(collateralEns.collateralEnv)
                             where (
                                    (collateralEnvItem.AssetCategory.Value.ToString() == collateral.AssetCategory)
                                    &&
                                    ((false == collateralEnvItem.IdAsset.HasValue) ||
                                     (collateralEnvItem.IdAsset == collateral.IdAsset))
                                   )
                             select collateralEnvItem.CssId).ToList();
                        //
                        //Liste des différentes chambres présentes ds les instructions de couverture
                        if (ArrFunc.IsFilled(css))
                            css = (from cssitem in css select cssitem).Distinct().ToList();
                        //
                        if (ArrFunc.IsFilled(css))
                        {
                            foreach (Nullable<int> cssItem in css)
                            {
                                //Recherche des instructions spécifiques à la chambre 
                                List<CBCollateralEnv> lstResult =
                                    (from collateralEnvItem in new List<CBCollateralEnv>(collateralEns.collateralEnv)
                                     where (
                                            (collateralEnvItem.CssId == cssItem)
                                            &&
                                            (collateralEnvItem.AssetCategory.Value.ToString() == collateral.AssetCategory)
                                            &&
                                            ((false == collateralEnvItem.IdAsset.HasValue) ||
                                                collateralEnvItem.IdAsset == collateral.IdAsset)
                                            )
                                     orderby collateralEnvItem.Weight
                                     select collateralEnvItem).ToList();
                                //
                                //Ajour de l'instruction la plus prioritaire spécifique à la chambre
                                if (ArrFunc.IsFilled(lstResult))
                                {
                                    if (null == collateral.collateralEnv)
                                        collateral.collateralEnv = new List<CBCollateralEnv>();
                                    collateral.collateralEnv.Add(lstResult.First());
                                }
                            }
                        }
                    }
                    //
                    if (ArrFunc.IsEmpty(collateral.collateralEnv))
                    {
                        //collateral.collateralEnv est alimenté avec la valeur par défaut
                        Nullable<Cst.UnderlyingAsset> collateralAssetCategory =
                            (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), collateral.AssetCategory);

                        //FI/PL 20170308 [22939]
                        CBCollateralEnv defaultcollateralEnv = CBCollateralEnv.DefaultCollateralEnv((Nullable<int>)ida_Pay,
                            (Nullable<int>)ida_Rec, collateralAssetCategory, (Nullable<int>)collateral.IdAsset);

                        collateral.collateralEnv = new List<CBCollateralEnv>
                        {
                            defaultcollateralEnv
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Calcul du montant global des espèces disponibles, avec la formule suivante:
        /// <para>Espèces disponibles = Solde Précédent + Cash-Flows du jour + Versement du jour</para>
        /// <para> + Espèces utilisées en garantie précédentes + Défaut de couverture précédent</para>
        /// </summary>
        /// <returns></returns>
        private List<Money> CalcAvailableCash()
        {
            // Solde Précédent si Gestion des Soldes
            var prevCashBalance_FlowCur = (from money in PrevCashBalance_FlowCur
                                           where IsCBOWithManagementBalance
                                           select money);
            // Cash-Flows du jour - Marges
            var dailyVMG_FlowCur = CashFlowVMG_FlowCur.AllCurrencyAmount();

            // Cash-Flows du jour - Primes
            var dailyPRM_FlowCur = CashFlowPRM_FlowCur.AllCurrencyAmount();

            // Cash-Flows du jour - Cash-Settlement
            var dailySCU_FlowCur = CashFlowSCU_FlowCur.AllCurrencyAmount();

            // Cash-Flows du jour - Frais HT et Taxes
            var dailyOPP_FlowCur = CashFlowOPP_FlowCur.AllCurrencyAmount();

            // Cash-Flows du jour - SafeKeepingPayment
            // PM 20150709 [21103] Add CashFlowSKP_FlowCur
            var dailySKP_FlowCur = CashFlowSKP_FlowCur.AllCurrencyAmount();

            // Cash-Flows du jour - Equalisation Payment
            // PM 20170911 [23408] Add CashFlowEQP_FlowCur
            var dailyEQP_FlowCur = CashFlowEQP_FlowCur.AllCurrencyAmount();

            // Versement (Multiplié par -1 car le montant est payé par le Client à l'entité)
            var payment_FlowCur = (from money in Payment_FlowCur
                                   select new Money(money.Amount.DecValue * -1, money.Currency));

            // Espèces utilisées en garantie précédentes
            //var prevDefaul_FlowCur = (from money in PrevCashUsed_FlowCur select money);
            var prevDefaul_FlowCur = PrevCashUsed_FlowCur;

            // Défaut de couverture précédent (Multiplié par -1 car le montant est négatif)
            var prevDefectDeposit_FlowCur = (from money in PrevDefectDeposit_FlowCur
                                             select new Money(money.Amount.DecValue * -1, money.Currency));

            // PM 20150709 [21103] Add dailySKP_FlowCur
            // PM 20170911 [23408] Add dailyEQP_FlowCur
            List<Money> ret =
              (from money in
                   // Cumuler les différents montants
                   (from moneyByIDC in
                        (prevCashBalance_FlowCur
                        .Concat(dailyEQP_FlowCur)
                        .Concat(dailyOPP_FlowCur)
                        .Concat(dailyPRM_FlowCur)
                        .Concat(dailySCU_FlowCur)
                        .Concat(dailySKP_FlowCur)
                        .Concat(dailyVMG_FlowCur)
                        .Concat(payment_FlowCur)
                        .Concat(prevDefaul_FlowCur)
                        .Concat(prevDefectDeposit_FlowCur)).GroupBy(money => money.Currency)
                    select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key))
               // Ne considérer que les montants positif (espèces créditrices) pour les clients
               // et les montants négatifs (espèces créditrices de point de vue de l'entité) pour les clearers
               where (IsClearer && money.amount.DecValue < 0) ||
                     (IsNotClearer && money.amount.DecValue > 0)
               select money).ToList();

            return ret;
        }

        /// <summary>
        /// Retourne la liste des cashFlows cumulées par devise (VMG+PRM+SCU+OPP)
        /// </summary>
        /// <returns></returns>
        public List<Money> CalcCashFlows()
        {
            // Construire l'ensemble des différents montants
            // PM 20150709 [21103] Add CashFlowSKP_FlowCur
            // PM 20170911 [23408] Add CashFlowEQP_FlowCur
            IEnumerable<Money> allCashFlows =
                CashFlowOPP_FlowCur.AllCurrencyAmount()
                .Concat(CashFlowEQP_FlowCur.AllCurrencyAmount())
                .Concat(CashFlowPRM_FlowCur.AllCurrencyAmount())
                .Concat(CashFlowSCU_FlowCur.AllCurrencyAmount())
                .Concat(CashFlowSKP_FlowCur.AllCurrencyAmount())
                .Concat(CashFlowVMG_FlowCur.AllCurrencyAmount());
            // Cumuler les montants par devise
            List<Money> ret = CBFlowTools.CalcSumByIdC(allCashFlows);
            return ret;
        }

        /// <summary>
        /// Retourne la liste des deposits cumulés par devise
        /// </summary>
        /// <returns></returns>
        public List<Money> CalcDeposit_FlowCur()
        {
            List<Money> ret =
                (from moneyByIDC in
                     (from deposit in FlowDeposit
                      from money in deposit.CurrencyAmount
                      select money).GroupBy(money => money.Currency)
                 select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CBExAmount> CalcDeposit_ExCTRValCur()
        {
            List<CBExAmount> ret = null;
            if (IsCBOWithMGCCTRVal)
            {
                ret =
                    (from cbExAmountByIDC in
                         (from deposit in FlowDeposit
                          from cbExAmount in deposit.Deposit_ExCTRValCur
                          select cbExAmount).GroupBy(cbExAmount => cbExAmount.FlowCurrency)
                     select new CBExAmount(cbExAmountByIDC.Key,
                     new Money((from cbExAmount in cbExAmountByIDC select cbExAmount.CurrencyAmount.amount.DecValue).Sum(),
                     (from cbExAmount in cbExAmountByIDC select cbExAmount.CurrencyAmount.Currency).First()),
                     (from cbExAmount in cbExAmountByIDC
                      from quote in cbExAmount.Quote
                      select quote).ToList())).ToList();
            }
            return ret;
        }

        /// <summary>
        /// Aliemente les SQL_TABLE associés aux éléments Ida, Idb, Ida_Entity
        /// </summary>
        // RD 20160112 [21748] Modify                        
        // EG 20180205 [23769] Add dbTransaction  
        public void SetSqlActorBook(string pCS, IDbTransaction pDbTransaction)
        {
            _sqlActor = new SQL_Actor(pCS, Ida)
            {
                DbTransaction = pDbTransaction
            };
            _sqlActor.LoadTable();

            _sqlBook = new SQL_Book(pCS, Idb)
            {
                DbTransaction = pDbTransaction
            };
            _sqlBook.LoadTable();

            _sqlEntity = new SQL_Actor(pCS, Ida_Entity)
            {
                DbTransaction = pDbTransaction,
                WithInfoEntity = true
            };

            // RD 20160112 [21748] Use accounting currency as cross currency            
            _accountingCurrency = "EUR";
            if (_sqlEntity.IsLoaded)
            {
                if (_sqlEntity.IsEntityExist && (StrFunc.IsFilled(_sqlEntity.IdCAccount)))
                    _accountingCurrency = _sqlEntity.IdCAccount;
            }
        }

        /// <summary>
        /// Returns true if there is at least one not zero flow in the currency {pCurrency} 
        /// </summary>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        /// FI 20120913 [18125] Creation
        public bool IsAmountCurrencyFilled(string pCurrency)
        {
            bool isFilledPrevCashAvailable = (ArrFunc.IsFilled(PrevCashAvailable_FlowCur) &&
                (null != PrevCashAvailable_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            bool isFilledPrevCashBalance = (ArrFunc.IsFilled(PrevCashBalance_FlowCur) &&
                (null != PrevCashBalance_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            bool isFilledPrevCashUsed = (ArrFunc.IsFilled(PrevCashUsed_FlowCur) &&
                (null != PrevCashUsed_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            bool isFilledPrevCollatAvailable = (ArrFunc.IsFilled(PrevCollatAvailable_FlowCur) &&
                (null != PrevCollatAvailable_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            bool isFilledPrevCollatUsed = (ArrFunc.IsFilled(PrevCollatUsed_FlowCur) &&
                (null != PrevCollatUsed_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            bool isFilledPrevDefectDeposit = (ArrFunc.IsFilled(PrevDefectDeposit_FlowCur) &&
                (null != PrevDefectDeposit_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));

            bool isFilledPrevDeposit = (ArrFunc.IsFilled(PrevDeposit_FlowCur) &&
                (null != PrevDeposit_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            //
            // PM 20150709 [21103] Add CashFlowSKP_FlowCur
            bool isFilledCashFlow =
                ((CashFlowOPP_FlowCur != null) && CashFlowOPP_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowPRM_FlowCur != null) && CashFlowPRM_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowSCU_FlowCur != null) && CashFlowSCU_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowSKP_FlowCur != null) && CashFlowSKP_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowVMG_FlowCur != null) && CashFlowVMG_FlowCur.IsFilledCurrencyFlows(pCurrency));
            //
            // PM 20170911 [23408] Add CashFlowEQP_FlowCur
            bool isFilledOtherFlow =
                ((CashFlowBWA_FlowCur != null) && CashFlowBWA_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowEQP_FlowCur != null) && CashFlowEQP_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowFDA_FlowCur != null) && CashFlowFDA_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowLOV_FlowCur != null) && CashFlowLOV_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowMKV_FlowCur != null) && CashFlowMKV_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowRMG_FlowCur != null) && CashFlowRMG_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowUMG_FlowCur != null) && CashFlowUMG_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((CashFlowUST_FlowCur != null) && CashFlowUST_FlowCur.IsFilledCurrencyFlows(pCurrency));
            //
            bool isFilledPaymentStl_FlowCur =
                ((PaymentStl_FlowCur != null) && PaymentStl_FlowCur.IsFilledCurrencyFlows(pCurrency)) ||
                ((ForwardPaymentStl_FlowCur != null) && ForwardPaymentStl_FlowCur.IsFilledCurrencyFlows(pCurrency));
            //
            bool isFilledPayment_FlowCur = (ArrFunc.IsFilled(Payment_FlowCur) &&
                (null != Payment_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            //
            bool isFilledCollatAvailable = (ArrFunc.IsFilled(FlowCollateral) && (null !=
                (from collateral in FlowCollateral
                 where collateral.IsAllowed_AtLeastOneCSS
                 from money in collateral.CurrencyAmount
                 select money).ToList().Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            //
            bool isFilledDeposit = (ArrFunc.IsFilled(FlowDeposit) && (null !=
                (from deposit in FlowDeposit
                 from money in deposit.CurrencyAmount
                 select money).ToList().Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            //
            bool isFilledMarginCall = ((ArrFunc.IsFilled(MarginCall_FlowCur) &&
                (null != MarginCall_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency))));
            //
            bool isFilledCashBalance = (ArrFunc.IsFilled(CashBalance_FlowCur) &&
                (null != CashBalance_FlowCur.Find(money => money.Amount.DecValue != 0 && money.Currency == pCurrency)));
            //
            return (isFilledCashFlow || isFilledOtherFlow || isFilledPaymentStl_FlowCur || isFilledPayment_FlowCur || isFilledDeposit || isFilledCollatAvailable ||
                isFilledMarginCall || isFilledCashBalance || isFilledPrevCashAvailable || isFilledPrevCashBalance ||
                isFilledPrevCashUsed || isFilledPrevCollatAvailable || isFilledPrevCollatUsed || isFilledPrevDefectDeposit ||
                isFilledPrevDeposit);
        }

        /// <summary>
        /// Retourne la liste des PaymentType présents sur les frais pour la devise {pIdC}
        /// </summary>
        /// PM 20150709 [21103] Obsolete : remplacé par CBFlow.GetPaymentType
        //public List<string> GetFeesPaymentTypes(string pIdC)
        //{
        //    return (
        //        (from fee in CashFlowOPP_FlowCur.CashFlows
        //         from money in fee.CurrencyAmount
        //         where money.Currency == pIdC
        //         select fee.PaymentType).Distinct().ToList());
        //}

        /// <summary>
        /// Init Delegate method
        /// </summary>
        /// <param name="pLog">delegate reference</param>
        /// <param name="pSetErrorWarning"></param>
        public void InitDelegate( SetErrorWarning pSetErrorWarning)
        {
            SetErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Initialise les différents montants non calculés du trade, selon le cas:
        /// <para>Soit à partir des montants se trouvant sur le MRO</para>
        /// <para>Soit à partir des montants se trouvant sur le CBO</para>
        /// <param name="pDtBusiness">Date business of the posted request</param>
        /// </summary>   
        /// FI 20170208 [22151][22152] Modify
        /// FI 20170316 [22950] Modify (Nouvelle signature add parameter pDtBusiness)
        public void InitializeAmounts(DateTime pDtBusiness)
        {
            List<int> listInt = null;
            List<string> listStr = null;
            List<Pair<int, string>> listIntString = null;
            #region Flows amounts
            //            
            // 1- On est sur le Book spécifié sur un MRO « avec Book » 
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO) 
            //  * Considérer les sommes des montants des différents flux sur tous les Books du MRO lui même
            //
            // 2- On est sur le Book spécifié sur un CBO « avec Book » 
            //  * Considérer les sommes des montants des différents flux du CBO et ceux sur tous les MRO enfants de premier niveau
            //
            // 3- On est sur chacun des Books d'un MRO « sans Book » 
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer les sommes des montants des différents flux sur le Book en cours
            //
            if (PartyTradeInfo.IsMROBook)
            {
                #region Book MRO
                // FI 20170208 [22151][22152] flux Valid uniquement
                CashFlowBWA_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.BWA);
                // PM 20170911 [23408] Add Equalisation Payment
                CashFlowEQP_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.EQP);
                CashFlowFDA_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.FDA);
                CashFlowLOV_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.LOV);
                CashFlowMKV_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.MKV);
                CashFlowOPP_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.OPP);
                CashFlowPRM_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.PRM);
                CashFlowRMG_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.RMG);
                CashFlowSCU_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.SCU);
                // PM 20150709 [21103] Add SafeKeepingPayment
                CashFlowSKP_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.SKP);
                CashFlowUMG_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.UMG);
                CashFlowUST_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.UST);
                CashFlowVMG_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.VMG);
                CashFlowAll_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows);

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     where source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                     || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();
                TradesCashFlowSource.AddRange(listIntString);


                // FI 20170208 [22151][22152] Alimentation de TradesCashFlowSourceAllStatus (trade Valid et unValid)
                // FI 20170316 [22950] TradesCashFlowSourceAllStatus contient uniquement les trades tels que dtBusiness = date de traitement 
                TradesCashFlowSourceAllStatus.AddRange(
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     where source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                     || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                     from trade in source.Trades.Where(x => x.dtBusinessSpecified && x.dtBusiness == pDtBusiness)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList());

                #endregion Book MRO
            }
            else if (PartyTradeInfo.IsCBOBook)
            {
                #region Book CBO
                // FI 20170208 [22151][22152] flux Valid uniquement
                CashFlowBWA_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.BWA);
                CashFlowBWA_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.BWA, StatusEnum.Valid);

                // PM 20170911 [23408] Add Equalisation Payment
                CashFlowEQP_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.EQP);
                CashFlowEQP_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.EQP, StatusEnum.Valid);

                CashFlowFDA_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.FDA);
                CashFlowFDA_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.FDA, StatusEnum.Valid);

                CashFlowLOV_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.LOV);
                CashFlowLOV_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.LOV, StatusEnum.Valid);

                CashFlowMKV_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.MKV);
                CashFlowMKV_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.MKV, StatusEnum.Valid);

                CashFlowOPP_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.OPP);
                CashFlowOPP_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.OPP, StatusEnum.Valid);

                CashFlowPRM_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.PRM);
                CashFlowPRM_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.PRM, StatusEnum.Valid);

                CashFlowRMG_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.RMG);
                CashFlowRMG_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.RMG, StatusEnum.Valid);

                CashFlowSCU_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.SCU);
                CashFlowSCU_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.SCU, StatusEnum.Valid);

                // PM 20150709 [21103] Add SafeKeepingPayment
                CashFlowSKP_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.SKP);
                CashFlowSKP_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.SKP, StatusEnum.Valid);

                CashFlowUMG_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.UMG);
                CashFlowUMG_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.UMG, StatusEnum.Valid);

                CashFlowUST_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.UST);
                CashFlowUST_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.UST, StatusEnum.Valid);

                CashFlowVMG_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows, FlowSubTypeEnum.VMG);
                CashFlowVMG_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, FlowSubTypeEnum.VMG, StatusEnum.Valid);

                CashFlowAll_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows);
                CashFlowAll_FlowCur.AddChildFlows(PartyTradeInfo.FirstMROChilds, FlowTypeEnum.CashFlows, StatusEnum.Valid);

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorCBO.TradesSource
                     where source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                     || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier))
                     .Concat(from actor in PartyTradeInfo.FirstMROChilds
                             from source in actor.TradesSource
                             where source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                             || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                             from trade in source.GetTrades(StatusEnum.Valid)
                             select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();
                TradesCashFlowSource.AddRange(listIntString);

                // FI 20170208 [22151][22152] Alimentation de TradesCashFlowSourceAllStatus (trade Valid et unValid)
                // FI 20170316 [22950] TradesCashFlowSourceAllStatus contient uniquement les trades tels que dtBusiness = date de traitement
                TradesCashFlowSourceAllStatus.AddRange(
                    (from source in PartyTradeInfo.ActorCBO.TradesSource
                     where source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                     || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                     from trade in source.Trades.Where(x => x.dtBusinessSpecified && x.dtBusiness == pDtBusiness)
                     select new Pair<int, string>(trade.IdT, trade.Identifier))
                     .Concat(from actor in PartyTradeInfo.FirstMROChilds
                             from source in actor.TradesSource
                             where source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                             || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows)
                             from trade in source.Trades.Where(x => x.dtBusinessSpecified && x.dtBusiness == pDtBusiness)
                             select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int,string>()).ToList());

                #endregion Book CBO
            }
            else
            {
                #region All Book
                // FI 20170208 [22151][22152] flux Valid uniquement
                CashFlowBWA_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.BWA);
                // PM 20170911 [23408] Add Equalisation Payment
                CashFlowEQP_FlowCur = new CBFlows(PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.EQP); 
                CashFlowFDA_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.FDA);
                CashFlowLOV_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.LOV);
                CashFlowMKV_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.MKV);
                CashFlowOPP_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.OPP);
                CashFlowPRM_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.PRM);
                CashFlowRMG_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.RMG);
                CashFlowSCU_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.SCU);
                // PM 20150709 [21103] Add SafeKeepingPayment
                CashFlowSKP_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.SKP);
                CashFlowUMG_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.UMG);
                CashFlowUST_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.UST);
                CashFlowVMG_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), PartyTradeInfo.Idb, FlowTypeEnum.CashFlows, FlowSubTypeEnum.VMG);
                CashFlowAll_FlowCur = new CBFlows(PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid), FlowTypeEnum.CashFlows);

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     where (source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                     || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows))
                     && source.IDB == PartyTradeInfo.Idb
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();
                TradesCashFlowSource.AddRange(listIntString);

                // FI 20170208 [22151][22152] Alimentation de TradesCashFlowSourceAllStatus (trade Valid et unValid)
                // FI 20170316 [22950] TradesCashFlowSourceAllStatus contient uniquement les trades tels que dtBusiness = date de traitement
                TradesCashFlowSourceAllStatus.AddRange(
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     where (source.FlowTypes.Contains(FlowTypeEnum.CashFlows)
                     || source.FlowTypes.Contains(FlowTypeEnum.OtherFlows))
                     && source.IDB == PartyTradeInfo.Idb
                     from trade in source.Trades.Where(x => x.dtBusinessSpecified && x.dtBusiness == pDtBusiness)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList());

                #endregion All Book
            }
            #endregion
            #region Deposits amounts
            //
            // 1- On est sur le Book spécifié sur un MRO « avec Book »  
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer les montants de Deposit sur le Book spécifié sur le MRO
            //
            // 2- On est sur le Book spécifié sur un CBO « avec Book » 
            //
            //  a- Le CBO est lui même MRO
            //      - MRO "avec book":
            //          * Considérer uniquement le Deposit sur le Book spécifié sur le MRO
            //
            //      - MRO "sans book"
            //          * Considérer la somme de tous les Deposits sur tous les Books du MRO
            //
            //  b- Le CBO n'est pas lui même MRO, 
            //      alors considérer la somme de les Déposits sur tous les MRO enfants de premier niveau:
            //
            //      - MRO enfant "avec book":
            //          * Considérer uniquement le Deposit sur le Book spécifié sur le MRO enfant
            //
            //      - MRO enfant "sans book"
            //          * Considérer la somme de tous les Deposits sur tous les Books du MRO enfant
            //
            // 3- On est sur chacun des Books d'un MRO « sans Book » 
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO) 
            //  * Considérer les montants de Deposit sur le Book en cours
            //
            if (PartyTradeInfo.IsMROBook)
            {
                // FI 20170208 [22151][22152] flux Valid uniquement
                FlowDeposit =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.Deposit && flow.IDB == PartyTradeInfo.Idb
                     select (CBDetDeposit)flow).ToList();

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     where source.FlowTypes.Contains(FlowTypeEnum.Deposit) && source.IDB == PartyTradeInfo.Idb
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                TradesDepositSource.AddRange(listIntString);
            }
            else if (PartyTradeInfo.IsCBOBook)
            {
                if (PartyTradeInfo.ActorCBO.IsMRO)
                {
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    FlowDeposit =
                        ((from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                          where PartyTradeInfo.ActorCBO.IsMROGlobalScopWithBook
                          && flow.Type == FlowTypeEnum.Deposit
                          && flow.IDB == PartyTradeInfo.ActorCBO.BusinessAttribute.IdB_MRO
                          select (CBDetDeposit)flow)
                        .Union(from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                               where PartyTradeInfo.ActorCBO.IsMRONotGlobalScopOrWithoutBook
                               && flow.Type == FlowTypeEnum.Deposit
                               select (CBDetDeposit)flow)).ToList();

                    // FI 20170208 [22151][22152] trade Valid uniquement
                    listIntString =
                        (from source in PartyTradeInfo.ActorCBO.TradesSource
                         where PartyTradeInfo.ActorCBO.IsMROGlobalScopWithBook
                          && source.FlowTypes.Contains(FlowTypeEnum.Deposit)
                          && source.IDB == PartyTradeInfo.ActorCBO.BusinessAttribute.IdB_MRO
                         from trade in source.GetTrades(StatusEnum.Valid)
                         select new Pair<int, string>(trade.IdT, trade.Identifier))
                         .Concat(from source in PartyTradeInfo.ActorCBO.TradesSource
                                 where PartyTradeInfo.ActorCBO.IsMRONotGlobalScopOrWithoutBook
                                  && source.FlowTypes.Contains(FlowTypeEnum.Deposit)
                                 from trade in source.GetTrades(StatusEnum.Valid)
                                 select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();
                    TradesDepositSource.AddRange(listIntString);
                }
                else
                {
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    FlowDeposit =
                        ((from actor in PartyTradeInfo.FirstMROChilds
                          where actor.IsMROGlobalScopWithBook
                          from flow in actor.GetFlows(StatusEnum.Valid)
                          where flow.Type == FlowTypeEnum.Deposit && flow.IDB == actor.BusinessAttribute.IdB_MRO
                          select (CBDetDeposit)flow)
                        .Union(from actor in PartyTradeInfo.FirstMROChilds
                               where actor.IsMRONotGlobalScopOrWithoutBook
                               from flow in actor.GetFlows(StatusEnum.Valid)
                               where flow.Type == FlowTypeEnum.Deposit
                               select (CBDetDeposit)flow)).ToList();

                    // FI 20170208 [22151][22152] trade Valid uniquement
                    listIntString =
                        (from actor in PartyTradeInfo.FirstMROChilds
                         where actor.IsMROGlobalScopWithBook
                         from source in actor.TradesSource
                         where source.FlowTypes.Contains(FlowTypeEnum.Deposit)
                          && source.IDB == actor.BusinessAttribute.IdB_MRO
                         from trade in source.GetTrades(StatusEnum.Valid)
                         select new Pair<int, string>(trade.IdT, trade.Identifier))
                         .Concat(from actor in PartyTradeInfo.FirstMROChilds
                                 where actor.IsMRONotGlobalScopOrWithoutBook
                                 from source in actor.TradesSource
                                 where source.FlowTypes.Contains(FlowTypeEnum.Deposit)
                                 from trade in source.GetTrades(StatusEnum.Valid)
                                 select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                    TradesDepositSource.AddRange(listIntString);
                }
            }
            else
            {
                // FI 20170208 [22151][22152] flux Valid uniquement
                FlowDeposit =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.Deposit && flow.IDB == PartyTradeInfo.Idb
                     select (CBDetDeposit)flow).ToList();

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     where source.FlowTypes.Contains(FlowTypeEnum.Deposit) && source.IDB == PartyTradeInfo.Idb
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                TradesDepositSource.AddRange(listIntString);
            }
            #endregion
            #region CashAndCollateral amounts
            //
            // 1- On est sur le Book spécifié sur un MRO « avec Book »  
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer les sommes des montants Versements/Collaterals sur tous les Books du MRO lui même
            //
            // 2- On est sur le Book spécifié sur un CBO « avec Book » 
            //
            //  a- CashAndCollatera = CBO
            //      * Considérer les Versements/Collaterals du Book spécifié sur le CBO
            //
            //  b- CashAndCollatera = MROChild, et le CBO est lui même MRO
            //      * Considérer les Versement/Collateral du Book spécifié sur le CBO
            //
            //  c- CashAndCollatera = MROChild, et le CBO n'est pas lui même MRO, 
            //      alors considérer la somme de les Versements/Collaterals sur tous les MRO enfants de premier niveau:
            //  
            //      - MRO enfant "avec book":
            //          * Considérer les Versements/Collaterals uniquement sur le Book spécifié sur le MRO enfant
            //
            //      - MRO enfant "sans book"
            //          * Considérer la somme de tous les Versements/Collaterals sur tous les Books du MRO enfant
            //
            //  d- CashAndCollatera = CBOChild, 
            //      alors considérer la somme de les Versements/Collaterals sur tous les CBO enfants de premier niveau:
            //  
            //      - CBO enfant "avec book":
            //          * Considérer les Versements/Collaterals uniquement sur le Book spécifié sur le CBO enfant
            //
            //      - CBO enfant "sans book"
            //          * Considérer la somme de tous les Versements/Collaterals sur tous les Books du CBO enfant
            //
            // 3- On est sur chacun des Books d'un MRO « sans Book » 
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO) 
            //  * Considérer les sommes des montants Versements/Collaterals sur le Book en cours
            //
            if (PartyTradeInfo.IsMROBook)
            {

                // PM 20140911 [20066][20185] Add PaymentStl_FlowCur & ForwardPaymentStl_FlowCur
                // FI 20170208 [22151][22152] flux Valid uniquement
                IEnumerable<CBDetStlPayment> payStl =
                    from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                    where (flow.Type == FlowTypeEnum.SettlementPayment)
                       && (flow.IDB == PartyTradeInfo.Idb)
                    select (CBDetStlPayment)flow;
                PaymentStl_FlowCur = new CBFlows(payStl, false);
                ForwardPaymentStl_FlowCur = new CBFlows(payStl, true);

                // FI 20170208 [22151][22152] flux Valid uniquement
                Payment_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.Payment && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                FlowCollateral =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.Collateral && flow.IDB == PartyTradeInfo.Idb
                     select (CBDetCollateral)flow).ToList();

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                     //where (source.FlowTypes.Contains(FlowTypeEnum.Payment)) && source.IDB == PartyTradeInfo.Idb
                     where (source.FlowTypes.Contains(FlowTypeEnum.Payment) || source.FlowTypes.Contains(FlowTypeEnum.SettlementPayment))
                     && source.IDB == PartyTradeInfo.Idb
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                TradesCashPaymentSource.AddRange(listIntString);
            }
            else if (PartyTradeInfo.IsCBOBook)
            {
                if ((PartyTradeInfo.ActorCBO.BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.CBO) ||
                    (PartyTradeInfo.ActorCBO.BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.MROChild &&
                    PartyTradeInfo.ActorCBO.IsMRO))
                {
                    //PM 20140911 [20066][20185] Add PaymentStl_FlowCur & ForwardPaymentStl_FlowCur
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    IEnumerable<CBDetStlPayment> payStl =
                        from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                        where (flow.Type == FlowTypeEnum.SettlementPayment)
                           && (flow.IDB == PartyTradeInfo.ActorCBO.BusinessAttribute.IdB_CBO)
                        select (CBDetStlPayment)flow;
                    PaymentStl_FlowCur = new CBFlows(payStl, false);
                    ForwardPaymentStl_FlowCur = new CBFlows(payStl, true);

                    // FI 20170208 [22151][22152] flux Valid uniquement
                    Payment_FlowCur =
                        (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.Payment
                         && flow.IDB == PartyTradeInfo.ActorCBO.BusinessAttribute.IdB_CBO
                         from money in flow.CurrencyAmount
                         select money).ToList();
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    FlowCollateral =
                        (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.Collateral
                         && flow.IDB == PartyTradeInfo.ActorCBO.BusinessAttribute.IdB_CBO
                         select (CBDetCollateral)flow).ToList();

                    // FI 20170208 [22151][22152] trade Valid uniquement
                    listIntString =
                        (from source in PartyTradeInfo.ActorCBO.TradesSource
                         //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                         //where (source.FlowTypes.Contains(FlowTypeEnum.Payment))
                         where (source.FlowTypes.Contains(FlowTypeEnum.Payment) || source.FlowTypes.Contains(FlowTypeEnum.SettlementPayment))
                         && source.IDB == PartyTradeInfo.ActorCBO.BusinessAttribute.IdB_CBO
                         from trade in source.GetTrades(StatusEnum.Valid)
                         select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                    TradesCashPaymentSource.AddRange(listIntString);
                }
                else if (PartyTradeInfo.ActorCBO.BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.MROChild)
                {
                    //PM 20140911 [20066][20185] Add PaymentStl_FlowCur & ForwardPaymentStl_FlowCur
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    IEnumerable<CBDetStlPayment> payStl = (
                        from actor in PartyTradeInfo.FirstMROChilds
                        where actor.IsMROGlobalScopWithBook
                        from flow in actor.GetFlows(StatusEnum.Valid)
                        where (flow.Type == FlowTypeEnum.Payment) && (flow.IDB == actor.BusinessAttribute.IdB_MRO)
                        select (CBDetStlPayment)flow
                        ).Concat(
                        from actor in PartyTradeInfo.FirstMROChilds
                        where actor.IsMRONotGlobalScopOrWithoutBook
                        from flow in actor.GetFlows(StatusEnum.Valid)
                        where (flow.Type == FlowTypeEnum.Payment)
                        select (CBDetStlPayment)flow);
                    PaymentStl_FlowCur = new CBFlows(payStl, false);
                    ForwardPaymentStl_FlowCur = new CBFlows(payStl, true);

                    // FI 20170208 [22151][22152] flux Valid uniquement
                    Payment_FlowCur =
                        (from moneyByIDC in
                             (from flow in
                                  ((from actor in PartyTradeInfo.FirstMROChilds
                                    where actor.IsMROGlobalScopWithBook
                                    from flow in actor.GetFlows(StatusEnum.Valid)
                                    where flow.Type == FlowTypeEnum.Payment && flow.IDB == actor.BusinessAttribute.IdB_MRO
                                    select flow)
                                  .Union(from actor in PartyTradeInfo.FirstMROChilds
                                         where actor.IsMRONotGlobalScopOrWithoutBook
                                         from flow in actor.GetFlows(StatusEnum.Valid)
                                         where flow.Type == FlowTypeEnum.Payment
                                         select flow))
                              from money in flow.CurrencyAmount
                              select money).GroupBy(money => money.Currency)
                         select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();

                    // FI 20170208 [22151][22152] flux Valid uniquement
                    FlowCollateral =
                        (from actor in PartyTradeInfo.FirstMROChilds
                         where actor.IsMROGlobalScopWithBook
                         from flow in actor.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.Collateral && flow.IDB == actor.BusinessAttribute.IdB_MRO
                         select (CBDetCollateral)flow)
                        .Union(from actor in PartyTradeInfo.FirstMROChilds
                               where actor.IsMRONotGlobalScopOrWithoutBook
                               from flow in actor.GetFlows(StatusEnum.Valid)
                               where flow.Type == FlowTypeEnum.Collateral
                               select (CBDetCollateral)flow).ToList();

                    // FI 20170208 [22151][22152] trade Valid uniquement
                    listIntString =
                        (from actor in PartyTradeInfo.FirstMROChilds
                         where actor.IsMROGlobalScopWithBook
                         from source in actor.TradesSource
                         //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                         //where (source.FlowTypes.Contains(FlowTypeEnum.Payment)) && source.IDB == actor.BusinessAttribute.IdB_MRO
                         where (source.FlowTypes.Contains(FlowTypeEnum.Payment) || source.FlowTypes.Contains(FlowTypeEnum.SettlementPayment))
                         && source.IDB == actor.BusinessAttribute.IdB_MRO
                         from trade in source.GetTrades(StatusEnum.Valid)
                         select new Pair<int, string>(trade.IdT, trade.Identifier))
                         .Concat(from actor in PartyTradeInfo.FirstMROChilds
                                 where actor.IsMRONotGlobalScopOrWithoutBook
                                 from source in actor.TradesSource
                                 where (source.FlowTypes.Contains(FlowTypeEnum.Payment))
                                 from trade in source.GetTrades(StatusEnum.Valid)
                                 select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                    TradesCashPaymentSource.AddRange(listIntString);
                }
                else if (PartyTradeInfo.ActorCBO.BusinessAttribute.CashAndCollateral == CashAndCollateralLocalizationEnum.CBOChild)
                {
                    List<CBActorNode> firstCBOChilds = CBTools.FindFirst(PartyTradeInfo.ActorCBO.ChildActors, RoleActor.CSHBALANCEOFFICE);

                    //PM 20140911 [20066][20185] Add PaymentStl_FlowCur & ForwardPaymentStl_FlowCur
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    IEnumerable<CBDetStlPayment> payStl = (
                        from actor in firstCBOChilds
                        where actor.IsCBOGlobalScopWithBook
                        from flow in actor.GetFlows(StatusEnum.Valid)
                        where (flow.Type == FlowTypeEnum.SettlementPayment) && (flow.IDB == actor.BusinessAttribute.IdB_CBO)
                        select (CBDetStlPayment)flow
                        ).Concat(
                        from actor in firstCBOChilds
                        where actor.IsCBONotGlobalScopOrWithoutBook
                        from flow in actor.GetFlows(StatusEnum.Valid)
                        where (flow.Type == FlowTypeEnum.SettlementPayment)
                        select (CBDetStlPayment)flow);
                    PaymentStl_FlowCur = new CBFlows(payStl, false);
                    ForwardPaymentStl_FlowCur = new CBFlows(payStl, true);

                    // FI 20170208 [22151][22152] flux Valid uniquement
                    Payment_FlowCur =
                        (from moneyByIDC in
                             (from flow in
                                  ((from actor in firstCBOChilds
                                    where actor.IsCBOGlobalScopWithBook
                                    from flow in actor.GetFlows(StatusEnum.Valid)
                                    where flow.Type == FlowTypeEnum.Payment && flow.IDB == actor.BusinessAttribute.IdB_CBO
                                    select flow)
                                  .Union(from actor in firstCBOChilds
                                         where actor.IsCBONotGlobalScopOrWithoutBook
                                         from flow in actor.GetFlows(StatusEnum.Valid)
                                         where flow.Type == FlowTypeEnum.Payment
                                         select flow))
                              from money in flow.CurrencyAmount
                              select money).GroupBy(money => money.Currency)
                         select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();

                    // FI 20170208 [22151][22152] flux Valid uniquement
                    FlowCollateral =
                        (from actor in firstCBOChilds
                         where actor.IsCBOGlobalScopWithBook
                         from flow in actor.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.Collateral && flow.IDB == actor.BusinessAttribute.IdB_CBO
                         select (CBDetCollateral)flow)
                        .Union(from actor in firstCBOChilds
                               where actor.IsCBONotGlobalScopOrWithoutBook
                               from flow in actor.GetFlows(StatusEnum.Valid)
                               where flow.Type == FlowTypeEnum.Collateral
                               select (CBDetCollateral)flow).ToList();

                    // FI 20170208 [22151][22152] trade Valid uniquement
                    listIntString =
                        (from actor in firstCBOChilds
                         where actor.IsCBOGlobalScopWithBook
                         from source in actor.TradesSource
                         //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                         //where (source.FlowTypes.Contains(FlowTypeEnum.Payment)) && source.IDB == actor.BusinessAttribute.IdB_CBO
                         where (source.FlowTypes.Contains(FlowTypeEnum.Payment) || source.FlowTypes.Contains(FlowTypeEnum.SettlementPayment))
                         && source.IDB == actor.BusinessAttribute.IdB_CBO
                         from trade in source.GetTrades(StatusEnum.Valid)
                         select new Pair<int, string>(trade.IdT, trade.Identifier))
                         .Concat(from actor in firstCBOChilds
                                 where actor.IsCBONotGlobalScopOrWithoutBook
                                 from source in actor.TradesSource
                                 //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                                 //where (source.FlowTypes.Contains(FlowTypeEnum.Payment))
                                 where (source.FlowTypes.Contains(FlowTypeEnum.Payment) || source.FlowTypes.Contains(FlowTypeEnum.SettlementPayment))
                                 from trade in source.GetTrades(StatusEnum.Valid)
                                 select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();
                    TradesCashPaymentSource.AddRange(listIntString);
                }
            }
            else
            {
                //PM 20140911 [20066][20185] Add PaymentStl_FlowCur & ForwardPaymentStl_FlowCur
                // FI 20170208 [22151][22152] flux Valid uniquement
                IEnumerable<CBDetStlPayment> payStl =
                    from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                    where (flow.Type == FlowTypeEnum.SettlementPayment)
                       && (flow.IDB == PartyTradeInfo.Idb)
                    select (CBDetStlPayment)flow;
                PaymentStl_FlowCur = new CBFlows(payStl, false);
                ForwardPaymentStl_FlowCur = new CBFlows(payStl, true);

                // FI 20170208 [22151][22152] flux Valid uniquement
                Payment_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.Payment && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                FlowCollateral =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.Collateral && flow.IDB == PartyTradeInfo.Idb
                     select (CBDetCollateral)flow).ToList();

                // FI 20170208 [22151][22152] trade Valid uniquement
                listIntString =
                    (from source in PartyTradeInfo.ActorMRO.TradesSource
                     //PM 20140916 [20066][20185] Add FlowTypeEnum.SettlementPayment
                     //where (source.FlowTypes.Contains(FlowTypeEnum.Payment)) && source.IDB == PartyTradeInfo.Idb
                     where (source.FlowTypes.Contains(FlowTypeEnum.Payment) || source.FlowTypes.Contains(FlowTypeEnum.SettlementPayment))
                     && source.IDB == PartyTradeInfo.Idb
                     from trade in source.GetTrades(StatusEnum.Valid)
                     select new Pair<int, string>(trade.IdT, trade.Identifier)).Distinct(new PairComparer<int, string>()).ToList();

                TradesCashPaymentSource.AddRange(listIntString);
            }
            #endregion
            #region Solde Précédent
            //
            // Si la gestion des soldes est activée sur le CBO, alors:
            //
            // 1- On est sur le Book spécifié sur un MRO « avec Book »  
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer le Solde Précédent sur le Book spécifié sur le MRO
            //
            // 2- On est sur le Book spécifié sur un CBO « avec Book » 
            //  * Considérer uniquement le Solde Précédent sur le Book spécifié sur le CBO
            //
            // 3- On est sur chacun des Books d'un MRO « sans Book »  
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer le Solde Précédent sur le Book en cours
            //
            if (PartyTradeInfo.ActorCBO.BusinessAttribute.IsManagementBalance)
            {
                if (PartyTradeInfo.IsMROBook)
                {
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    PrevCashBalance_FlowCur =
                        (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.LastCashBalance
                         && flow.FlowSubType == FlowSubTypeEnum.CSB
                         && flow.IDB == PartyTradeInfo.Idb
                         from money in flow.CurrencyAmount
                         select money).ToList();
                }
                else if (PartyTradeInfo.IsCBOBook)
                {
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    PrevCashBalance_FlowCur =
                        (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.LastCashBalance
                         && flow.FlowSubType == FlowSubTypeEnum.CSB
                         && flow.IDB == PartyTradeInfo.Idb
                         from money in flow.CurrencyAmount
                         select money).ToList();
                }
                else
                {
                    // FI 20170208 [22151][22152] flux Valid uniquement
                    PrevCashBalance_FlowCur =
                        (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                         where flow.Type == FlowTypeEnum.LastCashBalance
                         && flow.FlowSubType == FlowSubTypeEnum.CSB
                         && flow.IDB == PartyTradeInfo.Idb
                         from money in flow.CurrencyAmount
                         select money).ToList();
                }
            }
            else
                PrevCashBalance_FlowCur = new List<Money>();
            #endregion
            #region Defaut Déposit Précédent
            //
            // 1- On est sur le Book spécifié sur un MRO « avec Book »   
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer le Defaut Déposit Précédent sur le Book spécifié sur le MRO
            //
            // 2- On est sur le Book spécifié sur un CBO « avec Book »
            //  * Considérer uniquement le Defaut Déposit Précédent sur le Book spécifié sur le CBO
            //
            // 3- On est sur chacun des Books d'un MRO « sans Book »  
            //  (Le MRO peut être le CBO lui-même ou bien un MRO enfant du CBO)
            //  * Considérer le Defaut Déposit Précédent sur le Book en cours
            //                
            if (PartyTradeInfo.IsMROBook)
            {
                // FI 20170208 [22151][22152] flux Valid uniquement
                listInt =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance && flow.IDB == PartyTradeInfo.Idb
                     select ((CBDetLastFlow)flow).Idt).ToList();

                if (ArrFunc.IsFilled(listInt))
                    PrevCashBalanceIdt = listInt.First();

                listStr =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance && flow.IDB == PartyTradeInfo.Idb
                     select ((CBDetLastFlow)flow).Identifier_t).ToList();

                if (ArrFunc.IsFilled(listStr))
                    PrevCashBalanceIdentifier = listStr.First();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevDefectDeposit_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.UMR
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevDeposit_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.MGR
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCashAvailable_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CSA
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCashUsed_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CSU
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCollatAvailable_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CLA
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCollatUsed_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CLU
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();
            }
            else if (PartyTradeInfo.IsCBOBook)
            {
                // FI 20170208 [22151][22152] flux Valid uniquement
                listInt =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance && flow.IDB == PartyTradeInfo.Idb
                     select ((CBDetLastFlow)flow).Idt).ToList();

                if (ArrFunc.IsFilled(listInt))
                    PrevCashBalanceIdt = listInt.First();

                listStr =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance && flow.IDB == PartyTradeInfo.Idb
                     select ((CBDetLastFlow)flow).Identifier_t).ToList();

                if (ArrFunc.IsFilled(listStr))
                    PrevCashBalanceIdentifier = listStr.First();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevDefectDeposit_FlowCur =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.UMR
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevDeposit_FlowCur =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.MGR
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCashAvailable_FlowCur =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CSA
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCashUsed_FlowCur =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CSU
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCollatAvailable_FlowCur =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CLA
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCollatUsed_FlowCur =
                    (from flow in PartyTradeInfo.ActorCBO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CLU
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();
            }
            else
            {
                // FI 20170208 [22151][22152] flux Valid uniquement
                listInt =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance && flow.IDB == PartyTradeInfo.Idb
                     select ((CBDetLastFlow)flow).Idt).ToList();

                if (ArrFunc.IsFilled(listInt))
                    PrevCashBalanceIdt = listInt.First();

                // FI 20170208 [22151][22152] flux Valid uniquement
                listStr =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance && flow.IDB == PartyTradeInfo.Idb
                     select ((CBDetLastFlow)flow).Identifier_t).ToList();

                if (ArrFunc.IsFilled(listStr))
                    PrevCashBalanceIdentifier = listStr.First();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevDefectDeposit_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.UMR
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevDeposit_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.MGR
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCashAvailable_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CSA
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCashUsed_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CSU
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCollatAvailable_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CLA
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();

                // FI 20170208 [22151][22152] flux Valid uniquement
                PrevCollatUsed_FlowCur =
                    (from flow in PartyTradeInfo.ActorMRO.GetFlows(StatusEnum.Valid)
                     where flow.Type == FlowTypeEnum.LastCashBalance
                     && flow.FlowSubType == FlowSubTypeEnum.CLU
                     && flow.IDB == PartyTradeInfo.Idb
                     from money in flow.CurrencyAmount
                     select money).ToList();
            }
            #endregion
        }


        /// <summary>
        /// Affecter les priorités aux Collatéraux
        /// <para>- Si le référentiel COLLATERALPRIORITY est renseigné, alors l'exploiter</para>
        /// <para>- Sinon adopter le fonctionnement par défaut: D'abord les Actifs, Ensuite les Espèces disponibles</para>
        /// </summary>
        private void SetCollateralPriority()
        {
            if (ArrFunc.Count(this.PartyTradeInfo.ActorCBO.BusinessAttribute.CollateralPriority) > 0)
            {
                // Le référentiel COLLATERALPRIORITY est renseigné
                int maxPriority =
                    (from priority in this.PartyTradeInfo.ActorCBO.BusinessAttribute.CollateralPriority
                     select priority.PriorityRank).Max() + 1;

                (from collateral in FlowCollateral select collateral).ToList()
                    .ForEach(collateral => collateral.CollateralPriority = maxPriority);

                foreach (CBDetCollateral collateral in FlowCollateral)
                {
                    List<int> listPriorities =
                        (from priority in this.PartyTradeInfo.ActorCBO.BusinessAttribute.CollateralPriority
                         where priority.CollateralCategory == collateral.CollateralCategory
                         && (
                         (priority.CollateralCategory == CollateralCategoryEnum.AvailableCash)
                         || (StrFunc.IsEmpty(priority.AssetCategory))
                         || (
                         (priority.AssetCategory == collateral.AssetCategory)
                         && ((priority.IdAsset == 0) || (priority.IdAsset == collateral.IdAsset))
                         ))
                         select priority.PriorityRank).ToList();

                    if (ArrFunc.IsFilled(listPriorities))
                        collateral.CollateralPriority = listPriorities.First();
                }
            }
            else
            {
                // Le référentiel COLLATERALPRIORITY est NON renseigné, donc adopter un fonctionnement par défaut:
                // - D'abord les Actifs
                (from collateral in FlowCollateral
                 where collateral.CollateralCategory == CollateralCategoryEnum.Assets
                 select collateral).ToList().ForEach(collateral => collateral.CollateralPriority = 0);

                // - Ensuite les Espèces disponibles
                (from collateral in FlowCollateral
                 where collateral.CollateralCategory == CollateralCategoryEnum.AvailableCash
                 select collateral).ToList().ForEach(collateral => collateral.CollateralPriority = 1);
            }
        }

        /// <summary>
        /// Affecter la liste des collatéraux correspondants au déposit
        /// <para>- Constituer la liste des collatéraux selon la Chambre de compensation du déposit</para>
        /// <para>- Affecter pour chaque Collateral, l'Instruction qui Autorise/Interdit le Collateral pour le Déposit</para>
        /// <para>- Filtrer que les Collaterals autorisés par les instructions</para>
        /// <para>- Trier en fonction des Instructions de tri (référentiel COLLATERALPRIORITY)et en fonction du Poids de l'Instruction qui Autorise/Interdit le Collateral</para>
        /// <para>- Vérification si tous les Collatterals autorisés, sont bien valorisés, à la date de compensation en cours</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDeposit"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetCollateralForDeposit(string pCS, IDbTransaction pDbTransaction, CBDetDeposit pDeposit)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            // 1 - Constituer la liste des Collaterals qui matchent avec la Chambre de compensation du déposit
            pDeposit.CSS_Collaterals =
                (from collateral in FlowCollateral // FI 20160510 => FlowCollateral contient les espèces disponibles)
                 where ((collateral.Ida_CssSpecified == false) || collateral.Ida_Css == pDeposit.Ida_Css)
                 select collateral).ToList();

            // 2 - Affecter pour chaque Collateral, l'instruction qui autorise/interdit le Collateral pour le Déposit
            foreach (CBDetCollateral collateral in pDeposit.CSS_Collaterals)
            {
                List<CBCollateralEnv> listIntsruction =
                    (from instruction in collateral.collateralEnv
                     where (instruction.IsCssIdSpecified == false) || instruction.CssId == pDeposit.Ida_Css
                     orderby instruction.Weight
                     select instruction).ToList();
                //
                if (ArrFunc.IsFilled(listIntsruction))
                    collateral.Instruction_CurrentCSS = listIntsruction.First();
                else
                    collateral.Instruction_CurrentCSS = null;
            }

            // 3- Filtrer que les Collaterals autorisés par les Intsructions, et trier par:
            //      - D'abord en fonction des Instructions de tri (référentiel COLLATERALPRIORITY)
            //      - Ensuite, en fonction du Poids de l'Instruction qui Autorise/Interdit le Collateral
            pDeposit.CSS_Collaterals =
                (from collateral in pDeposit.CSS_Collaterals
                 where collateral.IsAllowed_CurrentCSS
                 orderby collateral.CollateralPriority, collateral.Weight_CurrentCSS
                 select collateral).ToList();

            // 4 - Vérification si tous les Collatterals autorisés, sont bien valorisés, à la date de compensation en cours
            //
            // [TODO RD: Faire évoluer le code pour prendre la dérnière cotation connue pour valoriser le Collateral, 
            // dans le cas où il ne l'est pas, pour la date de compensation en cours]

            if (ArrFunc.IsFilled(pDeposit.CSS_Collaterals))
            {
                CBDetCollateral notValorizedCollateral =
                    pDeposit.CSS_Collaterals.Find(collateral => collateral.IsCollatValorised == false);
                //
                if (notValorizedCollateral != null)
                {
                    string depositor = (IsClearer ? Identifier_Entity : PartyTradeInfo.Identifier_a);
                    depositor += (IsClearer ? string.Empty : " - " + PartyTradeInfo.Identifier_b);
                    string depositary = (IsClearer ? PartyTradeInfo.Identifier_a : Identifier_Entity);
                    depositary += (IsClearer ? " - " + PartyTradeInfo.Identifier_b : string.Empty);
                    //
                    string css = string.Empty;
                    //
                    if (notValorizedCollateral.Ida_CssSpecified)
                    {
                        SQL_Actor sql_css = new SQL_Actor(CSTools.SetCacheOn(pCS), notValorizedCollateral.Ida_Css)
                        {
                            DbTransaction = pDbTransaction
                        };
                        if (sql_css.IsLoaded)
                            css = sql_css.Identifier;
                    }
                    else
                        css = "&lt;" + Ressource.GetString("All") + "&gt;";

                    string asset = string.Empty;

                    SQL_AssetBase sql_asset = null;
                    Cst.UnderlyingAsset assetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), notValorizedCollateral.AssetCategory);

                    switch (assetCategory)
                    {
                        case Cst.UnderlyingAsset.EquityAsset:
                            sql_asset = new SQL_AssetEquity(CSTools.SetCacheOn(pCS), notValorizedCollateral.IdAsset);
                            break;
                        case Cst.UnderlyingAsset.Cash:
                            sql_asset = new SQL_AssetCash(CSTools.SetCacheOn(pCS), notValorizedCollateral.IdAsset);
                            break;
                        case Cst.UnderlyingAsset.Bond:
                            sql_asset = new SQL_AssetDebtSecurity(CSTools.SetCacheOn(pCS), notValorizedCollateral.IdAsset);
                            break;
                    }
                    sql_asset.DbTransaction = pDbTransaction;

                    if (sql_asset.IsLoaded)
                        asset = sql_asset.Identifier + " [id:" + notValorizedCollateral.IdAsset.ToString() + "] [" + assetCategory.ToString() + "]";
                    
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessStateTools.StatusEnum status = ProcessStateTools.StatusErrorEnum;
                    SetErrorWarning.Invoke(status);

                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4021), 3,
                        new LogParam(depositor),
                        new LogParam(depositary),
                        new LogParam(css),
                        new LogParam(asset),
                        new LogParam(DtFunc.DateTimeToStringDateISO(DtBusiness))));

                    ret = Cst.ErrLevel.FAILURE;
                }
            }

            return ret;
        }

        /// <summary>
        /// Charger la liste des devises de tous les montants et leurs affecter les priorités
        /// <para>- Si le référentiel MGCCURPRIORITY est renseigné, alors l'exploiter</para>
        /// <para>- Sinon adopter le fonctionnement par défaut: l’ordre Alphabétique du code de la devise</para>
        /// </summary>
        /// <returns></returns>
        public void SetCurrencyPriority()
        {
            List<string> currencies = GetCurrencyFilledAmount();

            // Charger la liste des devises 
            CurrencyPriority =
                (from currency in currencies
                 select new CBCollateralCurrencyPriority(currency))
                .Distinct().ToList();

            // Affecter les priorités aux devises
            // FL/PL 20130912 [18865]: Pour gérer les priorités, on applique l’astuce suivante:
            //
            //   -	On initialise  l’objet Priority avec toutes les devises nécessaire au cash balance du CBO en cours de traitement 
            //       avec la valeur maximal de l’ordre de priorité +1 paramétré dans le référentielde priorités de couverture des devises
            //       (Table : MGCCURPRIORITY)
            //
            //   -  On affecte les priorités respectives pour les devises contenues dans le référentiel MGCCURPRIORITY.
            //
            //      Exemple de contenues de cet Objet dans le Cas :
            //        - Ou l’on doit traiter un Cash Balance sur les Devise EUR, GBP, JPY, USD
            //        - Avec un ordre de priorité suivant paramétré dans le référentiel MGCCURPRIORITY
            //          (Devise : EUR => Priorité 1, USD => Priorité 2)
            //
            //              Currency	PriorityRank		isExist PriorityRank
            //              EUR		    1			        True
            //              USD		    2			        True
            //              GBP		    3			        False	
            //              JPY		    3			        False
            //
            //   -	Ensuite il suffit d’appliquer un tri sur les données PriorityRank et Currency et le tour est joué.
            if (ArrFunc.Count(this.PartyTradeInfo.ActorCBO.BusinessAttribute.CollateralCurrencyPriority) > 0)
            {
                // Le référentiel MGCCURPRIORITY est renseigné
                // -------------------------------------------

                //Recherche de la valeur max paramétrée dans le référentiel MGCCURPRIORITY
                int maxPriority =
                    (from priority in this.PartyTradeInfo.ActorCBO.BusinessAttribute.CollateralCurrencyPriority
                     select priority.PriorityRank).Max();

                //Mise à jour de toutes les valeurs de PriorityRank avec la valeur max paramétrée + 1
                (from currency in CurrencyPriority select currency).ToList()
                    .ForEach(currency => currency.PriorityRank = (maxPriority + 1));

                //Affection de la la valeur paramétrée dans le référentiel MGCCURPRIORITY sur les devises correspondantes
                foreach (CBCollateralCurrencyPriority priority in
                    this.PartyTradeInfo.ActorCBO.BusinessAttribute.CollateralCurrencyPriority)
                {
                    CBCollateralCurrencyPriority currency = CurrencyPriority.Find(cu => cu.Currency == priority.Currency);
                    // EG 20131114 Ajout {}
                    if (currency != null)
                    {
                        currency.PriorityRank = priority.PriorityRank;
                        currency.isExistPriorityRank = true;
                    }
                }
            }

            // Dans le cas où il n’y a aucun paramétrage pour les Priorités de couverture des devises,
            // c'est l’ordre Alphabétique du code de la devise qui est pris en compte.
            //  le tri est sur les données PriorityRank et Currency
            CurrencyPriority = CurrencyPriority
                .OrderBy(currency => currency.PriorityRank)
                .ThenBy(currency => currency.Currency).ToList();
        }

        /// <summary>
        /// Calculer le défaut de Déposit, selon la méthode de calcul des Appel/Rest. de Déposit
        /// <para>Couvrir Déposit par Déposit, donc chambre par chambre, pour calculer à la fin:</para>
        /// <para>- un montant Défaut de déposit pour chaque Déposit</para>
        /// <para>- un montant Espèces utilisées</para>
        /// <para>- un montant Collateral utilisés pour chaque Collateral</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pFlowDeposit"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pIsExistPriorityRank"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        private Cst.ErrLevel CalcDefectDepositForMGCMethod(string pCS, IDbTransaction pDbTransaction, List<CBDetDeposit> pFlowDeposit, string pCurrency)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            pFlowDeposit = pFlowDeposit
                .OrderBy(deposit => deposit.DtSysDeposit).ToList();

            foreach (CBDetDeposit deposit in pFlowDeposit)
            {
                List<Money> prevStepDefectDeposits_MGCCollatCur = null;
                Money usedGross_MGCCollatCTRVal = null;

                decimal currentDefectDeposits_MGCCTRVal = 0;
                decimal prevStepDefectDeposits_MGCCTRVal = 0;

                decimal currentDefectDeposits_MGCCollatCTRVal = 0;
                decimal prevStepDefectDeposits_MGCCollatCTRVal = 0;

                // 1 - Mettre de coté le Déposit (donc le premier défaut de Déposit)
                if (IsCBOWithMGCCollatCur)
                {
                    // MGCCollatCur: Devise par devise, donc on traite une liste de devise 
                    deposit.DefectDeposits_FlowCur = new List<Money>(deposit.CurrencyAmount);
                }
                else if (IsCBOWithMGCCTRVal)
                {
                    // Calcul des contrevaleurs des Déposits
                    ProcessCounterValue(CSTools.SetCacheOn(pCS), pDbTransaction, ExchangeIDC, deposit.CurrencyAmount,
                        out deposit.Deposit_ExCTRValCur, out deposit.Deposit_MGCCTRVal);

                    // MGCCTRVal: Toutes les devises ramenées dans la devise de Contrevaleur et le tout cumulé
                    currentDefectDeposits_MGCCTRVal = deposit.Deposit_MGCCTRVal.Amount.DecValue;
                }
                else if (IsCBOWithMGCCollatCTRVal)
                {
                    // MGCCollatCTRVal: Une devise à la fois
                    currentDefectDeposits_MGCCollatCTRVal = deposit.CurrencyAmount.Find(amount => amount.Currency == pCurrency).amount.DecValue;
                }

                // 2 - Affecter la liste des collatéraux correspondants au déposit (Chambre)
                if (deposit.CSS_Collaterals == null)
                    ret = SetCollateralForDeposit(pCS, pDbTransaction, deposit);

                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    // 3 - Couverture du Déposit par les Collateral autorisés
                    foreach (CBDetCollateral collat in deposit.CSS_Collaterals)
                    {
                        // 1 - Couvrir avec les montants du Collateral:
                        //
                        // Defaut de Déposit (Négatif car payé par le Client à l'entité) = 
                        //          Defaut de Déposit (Négatif car payé par le Client à l'entité) - 
                        //          Montants du Collateral Restant(Négatif car payé par le Client à l'entité)
                        // 
                        if (IsCBOWithMGCCollatCur)
                        {
                            #region IsCBOWithMGCCollatCur
                            prevStepDefectDeposits_MGCCollatCur = deposit.DefectDeposits_FlowCur;

                            deposit.DefectDeposits_FlowCur =
                                (from money in
                                     (from moneyByIDC in
                                          (from money in prevStepDefectDeposits_MGCCollatCur
                                           select money).GroupBy(money => money.Currency)
                                      select new Money(
                                          // Defaut de Déposit
                                          (from money in moneyByIDC select money.amount.DecValue).Sum()
                                          -
                                          (
                                          // Montant du Collateral Disponible après abattement
                                              (from money in collat.CurrencyAmount
                                               where money.Currency == moneyByIDC.Key
                                               select CBTools.ApplyHaircut(money.Amount.DecValue, collat.Haircut_CurrentCSS)).Sum()
                                              -
                                          // Montant du Collateral déjà Utilisé après abattement
                                              (from money in collat.CollatUsedGross_FlowCur
                                               where money.Currency == moneyByIDC.Key
                                               select CBTools.ApplyHaircut(money.Amount.DecValue, collat.Haircut_CurrentCSS)).Sum()
                                          )
                                      , moneyByIDC.Key)
                                     )
                                 // Ne considérer que les montants positifs ou à zéro pour les Clearers
                                 // et les montants négatifs ou à zéro pour les clients (déposit restant à couvrir)
                                 where (IsClearer && money.amount.DecValue >= 0) || (IsNotClearer && money.amount.DecValue <= 0)
                                 select money).ToList();
                            #endregion
                        }
                        else if (IsCBOWithMGCCTRVal)
                        {
                            #region IsCBOWithMGCCTRVal
                            prevStepDefectDeposits_MGCCTRVal = currentDefectDeposits_MGCCTRVal;
                            currentDefectDeposits_MGCCTRVal = prevStepDefectDeposits_MGCCTRVal -
                                (CBTools.ApplyHaircut(collat.Amount_MGCCTRVal.AvailableGross.Amount.DecValue, collat.Haircut_CurrentCSS) -
                                CBTools.ApplyHaircut(collat.Amount_MGCCTRVal.UsedGross.Amount.DecValue, collat.Haircut_CurrentCSS));
                            //
                            // Ne considérer que les montants positif ou à zéro pour les Clearers
                            // et les montants négatifs ou à zéro pour les clients (déposit restant à couvrir)
                            if (IsClearer)
                                currentDefectDeposits_MGCCTRVal = System.Math.Max(0, currentDefectDeposits_MGCCTRVal);
                            else
                                currentDefectDeposits_MGCCTRVal = System.Math.Min(0, currentDefectDeposits_MGCCTRVal);
                            #endregion
                        }
                        else if (IsCBOWithMGCCollatCTRVal)
                        {
                            #region IsCBOWithMGCCollatCTRVal
                            Money availableGross = null;

                            prevStepDefectDeposits_MGCCollatCTRVal = currentDefectDeposits_MGCCollatCTRVal;

                            if (collat.CollateralCategory == CollateralCategoryEnum.Assets)
                            {
                                availableGross = collat.Amount_MGCCollatCTRVal.AvailableGross.Find(money => money.Currency == pCurrency);
                                usedGross_MGCCollatCTRVal = collat.Amount_MGCCollatCTRVal.UsedGross.Find(money => money.Currency == pCurrency);

                                if (availableGross == null)
                                    availableGross = new Money(0, pCurrency);

                                if (usedGross_MGCCollatCTRVal == null)
                                    usedGross_MGCCollatCTRVal = new Money(0, pCurrency);
                            }
                            else
                            {
                                // Pour le cas des cash disponibles, 
                                availableGross = collat.CurrencyAmount.Find(money => money.Currency == pCurrency);
                                if (availableGross == null)
                                    availableGross = new Money(0, pCurrency);

                                usedGross_MGCCollatCTRVal = collat.CollatUsedGross_FlowCur.Find(money => money.Currency == pCurrency);
                                if (usedGross_MGCCollatCTRVal == null)
                                    usedGross_MGCCollatCTRVal = new Money(0, pCurrency);
                            }

                            currentDefectDeposits_MGCCollatCTRVal = prevStepDefectDeposits_MGCCollatCTRVal -
                                (CBTools.ApplyHaircut(availableGross.Amount.DecValue, collat.Haircut_CurrentCSS) -
                                CBTools.ApplyHaircut(usedGross_MGCCollatCTRVal.Amount.DecValue, collat.Haircut_CurrentCSS));

                            // Ne considérer que les montants positif ou à zéro pour les Clearers
                            // et les montants négatifs ou à zéro pour les clients (déposit restant à couvrir)
                            if (IsClearer)
                                currentDefectDeposits_MGCCollatCTRVal = System.Math.Max(0, currentDefectDeposits_MGCCollatCTRVal);
                            else
                                currentDefectDeposits_MGCCollatCTRVal = System.Math.Min(0, currentDefectDeposits_MGCCollatCTRVal);
                            #endregion
                        }

                        // 2 - Les montants du Collateral utilisés:
                        //      Montants utilisés (Négatif car payé par le Client à l'entité) =  
                        //          Defaut de Déposit AVANT couverture avec le Collateral (Négatif car payé par le Client à l'entité) -
                        //          Defaut de Déposit APRES couverture avec le Collateral (Négatif car payé par le Client à l'entité)

                        if (IsCBOWithMGCCollatCur)
                        {
                            collat.CollatUsed_FlowCur =
                                // Montant du Collateral déjà Utilisé
                                (from money in collat.CollatUsed_FlowCur select money)
                                .Union
                                (from moneyByIDC in
                                     (from money in prevStepDefectDeposits_MGCCollatCur select money).GroupBy(money => money.Currency)
                                 select new Money(
                                     // Defaut de Déposit AVANT couverture avec le Collateral
                                 (from money in moneyByIDC select money.amount.DecValue).Sum()
                                 -
                                     // Defaut de Déposit APRES couverture avec le Collateral
                                 (from money in deposit.DefectDeposits_FlowCur
                                  where money.Currency == moneyByIDC.Key
                                  select money.amount.DecValue).Sum()
                                 , moneyByIDC.Key)).ToList();

                            collat.CollatUsedGross_FlowCur =
                                // Montant du Collateral déjà Utilisé (sans Abattement)
                                (from money in collat.CollatUsedGross_FlowCur select money)
                                .Union
                                (from moneyByIDC in
                                     (from money in prevStepDefectDeposits_MGCCollatCur select money).GroupBy(money => money.Currency)
                                 select new Money(
                                     // Defaut de Déposit AVANT couverture avec le Collateral (sans Abattement)
                                 (from money in moneyByIDC select CBTools.ApplyRise(money.amount.DecValue, collat.Haircut_CurrentCSS)).Sum()
                                 -
                                     // Defaut de Déposit APRES couverture avec le Collateral (sans Abattement)
                                 (from money in deposit.DefectDeposits_FlowCur
                                  where money.Currency == moneyByIDC.Key
                                  select CBTools.ApplyRise(money.amount.DecValue, collat.Haircut_CurrentCSS)).Sum()
                                 , moneyByIDC.Key)).ToList();
                        }
                        else if (IsCBOWithMGCCTRVal)
                        {
                            collat.Amount_MGCCTRVal.Used = new Money(
                                // Montant du Collateral déjà Utilisé
                                collat.Amount_MGCCTRVal.Used.Amount.DecValue +
                                // Defaut de Déposit AVANT couverture avec le Collateral
                                // - Defaut de Déposit APRES couverture avec le Collateral
                                (prevStepDefectDeposits_MGCCTRVal - currentDefectDeposits_MGCCTRVal), ExchangeIDC);

                            collat.Amount_MGCCTRVal.UsedGross = new Money(
                                // Montant du Collateral déjà Utilisé (sans Abattement)
                                collat.Amount_MGCCTRVal.UsedGross.Amount.DecValue +
                                // Defaut de Déposit AVANT couverture avec le Collateral (sans Abattement)
                                // - // Defaut de Déposit APRES couverture avec le Collateral (sans Abattement)
                                (CBTools.ApplyRise(prevStepDefectDeposits_MGCCTRVal, collat.Haircut_CurrentCSS) -
                                CBTools.ApplyRise(currentDefectDeposits_MGCCTRVal, collat.Haircut_CurrentCSS)), ExchangeIDC);
                        }
                        else if (IsCBOWithMGCCollatCTRVal)
                        {
                            Money used_MGCCollatCTRVal;

                            usedGross_MGCCollatCTRVal = new Money(
                                // Montant du Collateral déjà Utilisé (sans Abattement)
                                usedGross_MGCCollatCTRVal.Amount.DecValue +
                                // Defaut de Déposit AVANT couverture avec le Collateral (sans Abattement)
                                // - // Defaut de Déposit APRES couverture avec le Collateral (sans Abattement)
                                (CBTools.ApplyRise(prevStepDefectDeposits_MGCCollatCTRVal, collat.Haircut_CurrentCSS) -
                                CBTools.ApplyRise(currentDefectDeposits_MGCCollatCTRVal, collat.Haircut_CurrentCSS)), pCurrency);

                            if (collat.CollateralCategory == CollateralCategoryEnum.Assets)
                            {
                                used_MGCCollatCTRVal = collat.Amount_MGCCollatCTRVal.Used.Find(money => money.Currency == pCurrency);
                                if (used_MGCCollatCTRVal == null)
                                    used_MGCCollatCTRVal = new Money(0, pCurrency);

                                if (collat.Amount_MGCCollatCTRVal.UsedGross.Exists(money => money.Currency == pCurrency))
                                {
                                    collat.Amount_MGCCollatCTRVal.UsedGross.Find(money => money.Currency == pCurrency)
                                        .Amount.DecValue = usedGross_MGCCollatCTRVal.Amount.DecValue;
                                }
                                else
                                    collat.Amount_MGCCollatCTRVal.UsedGross.Add(usedGross_MGCCollatCTRVal);
                            }
                            else
                            {
                                used_MGCCollatCTRVal = collat.CollatUsed_FlowCur.Find(money => money.Currency == pCurrency);
                                if (used_MGCCollatCTRVal == null)
                                    used_MGCCollatCTRVal = new Money(0, pCurrency);

                                if (collat.CollatUsedGross_FlowCur.Exists(money => money.Currency == pCurrency))
                                {
                                    collat.CollatUsedGross_FlowCur.Find(money => money.Currency == pCurrency)
                                        .Amount.DecValue = usedGross_MGCCollatCTRVal.Amount.DecValue;
                                }
                                else
                                    collat.CollatUsedGross_FlowCur.Add(usedGross_MGCCollatCTRVal);
                            }

                            used_MGCCollatCTRVal = new Money(
                                // Montant du Collateral déjà Utilisé
                                used_MGCCollatCTRVal.Amount.DecValue +
                                // Defaut de Déposit AVANT couverture avec le Collateral
                                // - Defaut de Déposit APRES couverture avec le Collateral
                                (prevStepDefectDeposits_MGCCollatCTRVal - currentDefectDeposits_MGCCollatCTRVal), pCurrency);

                            if (collat.CollateralCategory == CollateralCategoryEnum.Assets)
                            {
                                if (collat.Amount_MGCCollatCTRVal.Used.Exists(money => money.Currency == pCurrency))
                                {
                                    collat.Amount_MGCCollatCTRVal.Used.Find(money => money.Currency == pCurrency)
                                        .Amount.DecValue = used_MGCCollatCTRVal.Amount.DecValue;
                                }
                                else
                                    collat.Amount_MGCCollatCTRVal.Used.Add(used_MGCCollatCTRVal);
                            }
                            else
                            {
                                if (collat.CollatUsed_FlowCur.Exists(money => money.Currency == pCurrency))
                                {
                                    collat.CollatUsed_FlowCur.Find(money => money.Currency == pCurrency)
                                        .Amount.DecValue = used_MGCCollatCTRVal.Amount.DecValue;
                                }
                                else
                                    collat.CollatUsed_FlowCur.Add(used_MGCCollatCTRVal);
                            }
                        }
                    }

                    if (IsCBOWithMGCCTRVal)
                        deposit.DefectDeposits_MGCCTRVal = new Money(currentDefectDeposits_MGCCTRVal, ExchangeIDC);
                    else if (IsCBOWithMGCCollatCTRVal)
                    {
                        // c'est la même liste de Défaut de Déposit que la méthode MGCCollatCur
                        if (deposit.DefectDeposits_FlowCur.Exists(money => money.Currency == pCurrency))
                        {
                            deposit.DefectDeposits_FlowCur.Find(money => money.Currency == pCurrency)
                                .Amount.DecValue = currentDefectDeposits_MGCCollatCTRVal;
                        }
                        else
                            deposit.DefectDeposits_FlowCur.Add(new Money(currentDefectDeposits_MGCCollatCTRVal, pCurrency));
                    }
                }
                else
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Calculer les différents montants, avec éventuellement la contrevaleur
        /// <para>Espèces disponibles (Solde créditeur en garantie disponible)</para>
        /// <para>Encours déposit</para>
        /// <para>Collatéral et garanties bancaires</para>
        /// <para>Les espèces disponibles utilisées (Solde créditeur en garantie disponible utilisé)</para>
        /// <para>Appel/Rest de déposit</para>
        /// <para>Soldes espèces</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20120725 [18009] modifications diverses
        ///PM 20140910 [20066][20185] Add processing UK method
        // EG 20180205 [23769] Add dbTransaction  
        public Cst.ErrLevel ProcessAmounts(string pCS, IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            // FI 20200722 [XXXXX] Appel déplacé de nouveau
            // PM 20140925 [20066][20185] Deplacé, était initialement dans CashBalanceProcess.CreateAndRecordTrade()
            //SetSqlActorBook(CSTools.SetCacheOn(pCS), pDbTransaction);

            // ----------------------------------------------
            // Collaterals (Collateral et garanties bancaires) (1)
            // ----------------------------------------------

            // 1 - Trier les Collaterals en fonction de: Date compensation, Catégorie d'actif et Actif 
            FlowCollateral = FlowCollateral
                .OrderBy(collateral => collateral.DtBusinessCollateral)
                .ThenBy(collateral => collateral.AssetCategory)
                .ThenBy(collateral => collateral.IdAsset).ToList();

            // 2 - Vérification des instructions "Actifs en couverture" (Référentiel: COLLATERALENV)
            //      et chaque Collateral sera enrichi avec la liste des Intsructions correspondantes.
            CheckCollateralEnv(pCS, pDbTransaction, IsClearer,  PartyTradeInfo.Ida, Ida_Entity, CBTools.GetRoles(PartyTradeInfo.Roles), FlowCollateral);

            // -------------------
            // Currency priority
            // -------------------
            SetCurrencyPriority();

            //PM 20140912 [20066][20185] Ajout gestion méthode UK
            switch (CbCalcMethod)
            {
                case CashBalanceCalculationMethodEnum.CSBUK:
                    ret = ProcessMethodUK(pCS, pDbTransaction);
                    break;
                case CashBalanceCalculationMethodEnum.CSBDEFAULT:
                default:

                    // ------------------------------------------------
                    // Espèces diponibles (Solde créditeur en garantie)
                    // ------------------------------------------------

                    // RD 20121220 / FL 20121220 / [18320] Correction du calcul du Solde
                    // - Ne pas tenir compte du paramétre UseAvailableCash (réferentiel « Garanties/Soldes »)
                    // - Quelque soit la méthode de calcul du Solde, on doit toujours prendre en compte les espèces disponibles
                    // pour couvrir les dépots de garantie (Deposit)

                    // 1 - Créer un nouveau Collateral représentant les espèces disponibles, 
                    //      pour utilisation dans les instructions de Tri
                    CBDetCollateral availableCash = new CBDetCollateral(PartyTradeInfo.ActorCBO.Ida, StatusEnum.Valid);
                    FlowCollateral.Add(availableCash);

                    // 2 - Calculer le montant global des Espèces Disponibles, avec la formule suivante:
                    //      Espèces disponibles = Solde Précédent + Cash-Flows du jour + Versement du jour
                    //
                    // FI 20120725 [18009] appel de la méthode CalcAvailableCash
                    availableCash.CurrencyAmount = CalcAvailableCash();

                    // 3 - Inversion du sens des montants des espèces disponibles, 
                    //      car ce n'est pas le même raisonnement que sur les Collaterals
                    //      un Collateral est payé par le Client à l'entité, 
                    //      les Espèces disponibles, donc créditrices, sont reçues par le Client
                    (from money in availableCash.CurrencyAmount select money).ToList()
                        .ForEach(money => money.Amount.DecValue = -1 * money.Amount.DecValue);

                    // ----------------------------------------------------
                    // Collaterals (Collateral et garanties bancaires) (2)
                    // ----------------------------------------------------

                    // 1 - Calcul des contrevaleurs des montants de Collaterals
                    if (IsCBOWithMGCCTRVal)
                    {
                        foreach (CBDetCollateral collateral in FlowCollateral)
                        {
                            ProcessCounterValue(CSTools.SetCacheOn(pCS), pDbTransaction, ExchangeIDC, collateral.CurrencyAmount,
                                out collateral.Amount_MGCCTRVal.AvailableGross_Ex,
                                out collateral.Amount_MGCCTRVal.AvailableGross);
                        }
                    }

                    // 2 - Affecter les priorités aux Collaterals
                    SetCollateralPriority();

                    // -------------------
                    // Currency priority
                    // -------------------
                    // PM 20140912 [20066][20185] Deplacé en début de process
                    //SetCurrencyPriority();

                    // -------------------------------------------
                    // Calculs des montants unitaires par Déposit
                    // -------------------------------------------
                    // Couvrir Déposit par Déposit, donc chambre par chambre, pour calculer à la fin:
                    // - un montant Défaut de déposit pour chaque Déposit
                    // - un montant Espèces utilisées
                    // - un montant Collateral utilisés pour chaque Collateral

                    if (IsCBOWithMGCCollatCur || IsCBOWithMGCCTRVal)
                    {
                        ret = CalcDefectDepositForMGCMethod(pCS, pDbTransaction, FlowDeposit, string.Empty);
                    }
                    else
                    {
                        foreach (CBCollateralCurrencyPriority currentCurrency in CurrencyPriority)
                        {
                            foreach (CBDetCollateral collateral in
                                FlowCollateral.Where(collateral => (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)))
                            {
                                // Constituent des collateraux disponibles
                                CBCollateralConstituent collatConstituent = new CBCollateralConstituent(currentCurrency.Currency);
                                if (collateral.Amount_MGCCollatCTRVal.Used.Count > 0)
                                    collatConstituent.AlreadyUsed.AddRange(collateral.Amount_MGCCollatCTRVal.Used);
                                collateral.Amount_MGCCollatCTRVal.AvailableConstituent.Add(collatConstituent);

                                // Calcul des montants de collatéraux déposées convertis dans la devise en cours. 
                                // RD 20160112 [21748] Use accounting currency as cross currency
                                ProcessCounterValue(CSTools.SetCacheOn(pCS), pDbTransaction, currentCurrency.Currency, collateral.CurrencyAmount,
                                    out List<CBExAmount> exAmount, out Money firstAvailableGross);

                                collateral.Amount_MGCCollatCTRVal.AvailableGross_Ex.AddRange(exAmount);

                                // Calcul des montants de collatéraux utilisées sur les devises précédentes convertis dans la devise en cours.
                                // RD 20160112 [21748] Use accounting currency as cross currency
                                ProcessCounterValue(CSTools.SetCacheOn(pCS), pDbTransaction, currentCurrency.Currency, collateral.Amount_MGCCollatCTRVal.UsedGross,
                                    out exAmount, out Money alreadyUsedGross);

                                collateral.Amount_MGCCollatCTRVal.AvailableGross_Ex.AddRange(exAmount);

                                // FL/PL 20130912 [18865]: Calcul du montant des collatéraux disponibles pour la devise en cours.
                                //  Ce montant est égal à la somme des montants de collatéraux déposées convertis dans la devise en cours 
                                //  moins la somme des collatéraux utilisées sur les devises précédentes convertis dans la devise en cours.
                                //
                                //  Attention !! Dans le cas ou la devise considérée n’est pas paramétrée pour le CBO en cours dans le référentiel 
                                //  de priorités de couverture des devises (Table : MGCCURPRIORITY) le montant des collatéraux disponible sera égale à zéro.
                                //
                                //  Ps. pour savoir si une devise pour un CBO donnée est paramétrée dans MGCCURPRIORITY, on vérifie la valeur de « currentCurrency.isExistPriorityRank ».
                                Money money = null;
                                if (currentCurrency.isExistPriorityRank)
                                {
                                    money = new Money(firstAvailableGross.Amount.DecValue - alreadyUsedGross.Amount.DecValue, currentCurrency.Currency);
                                }
                                else
                                {
                                    money = new Money(0, currentCurrency.Currency);
                                }
                                collateral.Amount_MGCCollatCTRVal.AvailableGross.Add(money);
                            }

                            List<CBDetDeposit> currentCurrencyDeposits =
                                (from deposit in FlowDeposit
                                 from money in deposit.CurrencyAmount
                                 where money.Currency == currentCurrency.Currency
                                 select deposit).ToList();

                            ret = CalcDefectDepositForMGCMethod(pCS, pDbTransaction, currentCurrencyDeposits, currentCurrency.Currency);
                        }
                    }

                    if (ret == Cst.ErrLevel.SUCCESS)
                    {
                        // ----------------------------
                        // Calcul des Montants Globaux
                        // ----------------------------

                        if (IsCBOWithMGCCollatCur || IsCBOWithMGCCollatCTRVal)
                        {
                            #region IsCBOWithMGCCollatCur || IsCBOWithMGCCollatCTRVal
                            CashAvailable_FlowCur =
                                (from collateral in FlowCollateral
                                 where collateral.CollateralCategory == CollateralCategoryEnum.AvailableCash
                                 from money in collateral.CurrencyAmount
                                 select new Money(money.Amount.DecValue * -1, money.Currency)).ToList();

                            CashUsed_FlowCur =
                                (from moneyByIDC in
                                     (from collateral in FlowCollateral
                                      where collateral.CollateralCategory == CollateralCategoryEnum.AvailableCash
                                      from money in collateral.CollatUsed_FlowCur
                                      select new Money(money.Amount.DecValue * -1, money.Currency)
                                     ).GroupBy(money => money.Currency)
                                 select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();

                            GlobalDefectDeposits_FlowCur =
                                (from moneyByIDC in
                                     (from deposit in FlowDeposit
                                      from money in deposit.DefectDeposits_FlowCur
                                      select money
                                     ).GroupBy(money => money.Currency)
                                 select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();
                            #endregion IsCBOWithMGCCollatCur || IsCBOWithMGCCollatCTRVal
                        }
                        else if (IsCBOWithMGCCTRVal)
                        {
                            #region IsCBOWithMGCCTRVal
                            CashAvailable_CTRValCur =
                                (from collateral in FlowCollateral
                                 where collateral.CollateralCategory == CollateralCategoryEnum.AvailableCash
                                 select new Money(collateral.Amount_MGCCTRVal.AvailableGross.Amount.DecValue * -1,
                                 collateral.Amount_MGCCTRVal.AvailableGross.Currency)
                                ).First();

                            CashAvailable_ExCTRValCur =
                                (from collateral in FlowCollateral
                                 where collateral.CollateralCategory == CollateralCategoryEnum.AvailableCash
                                 from exAmount in collateral.Amount_MGCCTRVal.AvailableGross_Ex
                                 select exAmount
                                ).ToList();

                            (from exAmount in CashAvailable_ExCTRValCur
                             select exAmount.CurrencyAmount).ToList()
                            .ForEach(money => money.Amount.DecValue = -1 * money.Amount.DecValue);

                            CashUsed_CTRValCur =
                                (from collateral in FlowCollateral
                                 where collateral.CollateralCategory == CollateralCategoryEnum.AvailableCash
                                 select new Money(collateral.Amount_MGCCTRVal.Used.Amount.DecValue * -1, collateral.Amount_MGCCTRVal.AvailableGross.Currency)
                                ).First();

                            GlobalDefectDeposits_CTRValCur =
                                new Money((from deposit in FlowDeposit select deposit.DefectDeposits_MGCCTRVal.amount.DecValue).Sum(), ExchangeIDC);
                            #endregion IsCBOWithMGCCTRVal
                        }

                        // ----------------------------------------------
                        // Collaterals (Collateral et garanties bancaires) (3)
                        // ----------------------------------------------
                        #region
                        //// Appliquer l'Abattement le plus défavorable au montant de Collateral non utilisé
                        //// 
                        ////  Montant Collateral net disponible =
                        ////      Abattement de (Montant Collateral brut disponible - Montant brut utilisé) +
                        ////      Montant Collateral net utilisé
                        ////
                        //foreach (CBDetCollateral collateral in
                        //    FlowCollateral.Where(collateral => (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)))
                        //{
                        //    // Pour toutes les méthodes, on calcul les montants des collatéraux nets (après abattement) en devises d'origines
                        //    collateral.CollatAvailable_FlowCur =
                        //        (from moneyByIDC in
                        //             (from money in collateral.CurrencyAmount select money).GroupBy(money => money.Currency)
                        //         select new Money(
                        //             // Le montant net non utilisé
                        //         CBTools.ApplyHaircut(
                        //             // Le montant brut disponible
                        //         (from money in moneyByIDC select money.amount.DecValue).Sum()
                        //         -
                        //             // Le montant brut utilisé
                        //         (from money in collateral.CollatUsedGross_FlowCur
                        //          where money.Currency == moneyByIDC.Key
                        //          select money.amount.DecValue).Sum()
                        //         ,
                        //         collateral.HaircutWorst)
                        //         +
                        //             // Le montant net utilisé
                        //         (from money in collateral.CollatUsed_FlowCur
                        //          where money.Currency == moneyByIDC.Key
                        //          select money.amount.DecValue).Sum(),
                        //         moneyByIDC.Key)).ToList();

                        //    if (IsCBOWithMGCCTRVal)
                        //    {
                        //        collateral.Amount_MGCCTRVal.Available = new Money(
                        //            // Le montant net non utilisé
                        //            //      = Abattement de (Le montant brut disponible - Le montant brut utilisé)
                        //            CBTools.ApplyHaircut(
                        //                collateral.Amount_MGCCTRVal.AvailableGross.amount.DecValue - collateral.Amount_MGCCTRVal.UsedGross.amount.DecValue,
                        //                collateral.HaircutWorst)
                        //            +
                        //            // Le montant net utilisé
                        //            collateral.Amount_MGCCTRVal.Used.amount.DecValue,
                        //            ExchangeIDC);
                        //    }
                        //    else if (IsCBOWithMGCCollatCTRVal)
                        //    {
                        //        collateral.Amount_MGCCollatCTRVal.Available =
                        //            (from moneyByIDC in
                        //                 (from money in collateral.Amount_MGCCollatCTRVal.AvailableGross select money).GroupBy(money => money.Currency)
                        //             select new Money(
                        //                 // Le montant net non utilisé
                        //             CBTools.ApplyHaircut(
                        //                 // Le montant brut disponible
                        //             (from money in moneyByIDC select money.amount.DecValue).Sum()
                        //             -
                        //                 // Le montant brut utilisé
                        //             (from money in collateral.Amount_MGCCollatCTRVal.UsedGross
                        //              where money.Currency == moneyByIDC.Key
                        //              select money.amount.DecValue).Sum()
                        //             ,
                        //             collateral.HaircutWorst)
                        //             +
                        //                 // Le montant net utilisé
                        //             (from money in collateral.Amount_MGCCollatCTRVal.Used
                        //              where money.Currency == moneyByIDC.Key
                        //              select money.amount.DecValue).Sum(),
                        //             moneyByIDC.Key)).ToList();
                        //    }
                        //}
                        #endregion

                        // RD 20131011 [19035] Calculer un Montant Collateral disponible BRUT, c'est à dire sans abattement.
                        // Montant Collateral brut disponible (sans abattement)
                        foreach (CBDetCollateral collateral in
                            FlowCollateral.Where(collateral => (collateral.CollateralCategory != CollateralCategoryEnum.AvailableCash)))
                        {
                            // Pour toutes les méthodes, on calcul les montants des collatéraux BRUT (sans abattement) en devises d'origines
                            collateral.CollatAvailable_FlowCur =
                                (from moneyByIDC in
                                     (from money in collateral.CurrencyAmount select money).GroupBy(money => money.Currency)
                                 select new Money(
                                 (from money in moneyByIDC select money.amount.DecValue).Sum(),
                                 moneyByIDC.Key)).ToList();

                            if (IsCBOWithMGCCTRVal)
                            {
                                collateral.Amount_MGCCTRVal.Available = new Money(
                                    // Le montant brut disponible
                                    collateral.Amount_MGCCTRVal.AvailableGross.amount.DecValue,
                                    ExchangeIDC);
                            }
                            else if (IsCBOWithMGCCollatCTRVal)
                            {
                                collateral.Amount_MGCCollatCTRVal.Available =
                                    (from moneyByIDC in
                                         (from money in collateral.Amount_MGCCollatCTRVal.AvailableGross select money).GroupBy(money => money.Currency)
                                     select new Money(
                                         // Le montant brut disponible
                                     (from money in moneyByIDC select money.amount.DecValue).Sum(),
                                     moneyByIDC.Key)).ToList();
                            }
                        }

                        //-------------------------------------------------------------------------------------------------------
                        // RD 20121220 / FL 20121220 / [18320] Correction de la formule de calcul de Appel/Restitution de Déposit
                        //
                        // Appel/Restitution de Déposit = 
                        //  (Défaut de couverture Précédent + Solde créditeur en garantie utilisées Précédent) 
                        //  -  (Défaut de couverture jour + Solde créditeur en garantie utilisées jour)

                        // Appel: Négatif car payé par le Client à l'entité
                        // Restitution: Positif car payé par l'entité au Client
                        //-------------------------------------------------------------------------------------------------------

                        if (IsCBOWithMGCCollatCur || IsCBOWithMGCCollatCTRVal)
                        {
                            // Défaut de couverture Précédent ( multiplier par -1, car le montant est déjà négatif)
                            var prevDefectDeposit_FlowCur = (from money in PrevDefectDeposit_FlowCur
                                                             select new Money(money.Amount.DecValue * -1, money.Currency));

                            // Solde créditeur en garantie utilisées Précédent
                            var prevCashUsed_FlowCur = (from money in PrevCashUsed_FlowCur
                                                        select new Money(money.Amount.DecValue, money.Currency));

                            // Défaut de couverture jour (c'est un montant déjà négatif)
                            var dailyDefectDeposits_FlowCur = (from money in GlobalDefectDeposits_FlowCur select money);

                            // Solde créditeur en garantie utilisées jour (montant positif, donc multiplier par -1)
                            var dailyCashUsed_FlowCur = (from money in CashUsed_FlowCur
                                                         select new Money(money.Amount.DecValue * -1, money.Currency));

                            MarginCall_FlowCur =
                                (from moneyByIDC in
                                     (prevDefectDeposit_FlowCur
                                     .Concat(prevCashUsed_FlowCur)
                                     .Concat(dailyDefectDeposits_FlowCur)
                                     .Concat(dailyCashUsed_FlowCur)
                                     ).GroupBy(money => money.Currency)
                                 select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();
                        }
                        else if (IsCBOWithMGCCTRVal)
                        {
                            // RD 20121220 / FL 20121220 / [18320] Calcul en MultiDevise à revoir pour respecter la nouvelle formule

                            MarginCall_FlowCur = new List<Money>(); // Liste vide
                            //
                            if (ArrFunc.IsFilled(PrevDefectDeposit_FlowCur) &&
                                (PrevDefectDeposit_FlowCur.First() != null))
                            {
                                MarginCall_CTRValCur =
                                    new Money(GlobalDefectDeposits_CTRValCur.Amount.DecValue - PrevDefectDeposit_FlowCur.First().Amount.DecValue,
                                        ExchangeIDC);
                            }
                            else
                            {
                                MarginCall_CTRValCur = GlobalDefectDeposits_CTRValCur;
                            }
                        }
                        //
                        List<Money> marginCallToUse = null;
                        //
                        if (IsCBOWithMGCCollatCur || IsCBOWithMGCCollatCTRVal)
                            marginCallToUse = MarginCall_FlowCur;
                        else
                            marginCallToUse = new List<Money> { MarginCall_CTRValCur };

                        //------------------------------------------------------------------
                        // Solde = 
                        //  + Solde Précédent 
                        //  + Cash-Flows du jour
                        //  - Versement du jour (Négatif car payé par le Client à l'entité) 
                        //  + Appel/Restitution
                        //------------------------------------------------------------------

                        CashBalance_FlowCur =
                            (from moneyByIDC in
                                 (
                                     // Solde Précédent si Gestion des Soldes
                                 (from money in PrevCashBalance_FlowCur
                                  where IsCBOWithManagementBalance
                                  select money)
                                 // Cash-Flows du jour
                                     //Prise en considération des montants HT et des taxes
                                 .Concat(CashFlowOPP_FlowCur.AllCurrencyAmount())
                                 .Concat(CashFlowPRM_FlowCur.AllCurrencyAmount())
                                 .Concat(CashFlowSCU_FlowCur.AllCurrencyAmount())
                                     // PM 20150709 [21103] Add CashFlowSKP_FlowCur
                                 .Concat(CashFlowSKP_FlowCur.AllCurrencyAmount())
                                 .Concat(CashFlowVMG_FlowCur.AllCurrencyAmount())
                                     // PM 20170911 [23408] Add CashFlowEQP_FlowCur
                                 .Concat(CashFlowEQP_FlowCur.AllCurrencyAmount())
                                    //  Versement (Multiplié par -1 car le montant est payé par le Client à l'entité)
                                 .Concat(from money in Payment_FlowCur
                                        select new Money(money.Amount.DecValue * -1, money.Currency))
                                 // Appel/Restitution
                                 .Concat(marginCallToUse)
                                 ).GroupBy(money => money.Currency)
                             select new Money((from money in moneyByIDC select money.amount.DecValue).Sum(), moneyByIDC.Key)).ToList();
                    }
                    break;
            }
            return ret;
        }

        #region ProcessSumCounterValue
        /// <summary>
        /// Realise la somme des montants des flux {pFlowCur} par devise, et retourne la contrevaleur de cette somme.
        /// La devise de contrevaleur est lu dans {pCtrValTradeInfo} et les cotations utilisées sont ajoutées dans {pCtrValTradeInfo}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCtrValTradeInfo"></param>
        /// <param name="pFlowCur"></param>
        /// <returns></returns>
        /// PM 20140917 [20066][20185] New
        // EG 20180205 [23769] Add dbTransaction  
        private Money ProcessSumCounterValue(string pCS, IDbTransaction pDbTransaction, CBCtrValTradeInfo pCtrValTradeInfo, CBFlows pFlowCur)
        {
            Money ret = null;
            if ((null != pCtrValTradeInfo) && (null != pFlowCur))
            {
                ret = new Money(0, pCtrValTradeInfo.Currency);
                if (null != pFlowCur.Flows)
                {
                    // Calcul de la somme des montants par devise
                    List<Money> sumMoneyIdc = pFlowCur.CalcSumByIdC();
                    // Calcul de la contrevaleur
                    ret = ProcessSumCounterValue(pCS, pDbTransaction, pCtrValTradeInfo, sumMoneyIdc);
                }
            }
            return ret;
        }
        /// <summary>
        /// Realise la somme des montants des flux {pFlowCur} par devise, et retourne la contrevaleur de cette somme.
        /// La devise de contrevaleur est lu dans {pCtrValTradeInfo} et les cotations utilisées sont ajoutées dans {pCtrValTradeInfo}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCtrValTradeInfo"></param>
        /// <param name="pFlowCur"></param>
        /// <returns></returns>
        /// PM 20140917 [20066][20185] New
        // EG 20180205 [23769] Add dbTransaction  
        private Money ProcessSumCounterValue(string pCS, IDbTransaction pDbTransaction, CBCtrValTradeInfo pCtrValTradeInfo, IEnumerable<Money> pFlowCur)
        {
            Money ret = null;
            if ((null != pCtrValTradeInfo) && (null != pFlowCur))
            {
                // Calcul de la somme des montants par devise
                List<Money> sumMoneyIdc =
                    (
                    from money in pFlowCur
                    group money by money.Currency
                        into moneyIdc
                    select new Money(moneyIdc.Sum(m => m.Amount.DecValue), moneyIdc.Key)
                    ).ToList();
                // Calcul de la contrevaleur
                ProcessCounterValue(pCS, pDbTransaction, pCtrValTradeInfo.Currency, sumMoneyIdc, out List<CBExAmount> exAmount, out ret);
                // Sauvegarde des cotations
                if (exAmount != null)
                {
                    IEnumerable<CBQuote> allQuote =
                        from amount in exAmount
                        where (null != amount)
                        from quote in amount.Quote
                        select quote;
                    //
                    pCtrValTradeInfo.Quote = allQuote.Union(pCtrValTradeInfo.Quote, new CBQuoteComparer()).ToList();
                }
            }
            return ret;
        }
        #endregion
        #region ProcessDetailCounterValue
        /// <summary>
        /// Calcul la contrevaleur de chaqu'un des flux de {pFlowCur} dans la devise de contrevaleur lu dans {pCtrValTradeInfo},
        /// et retourne {pFlowCur} avec les flux en contrevaleur mis à jour.
        /// Les cotations utilisées sont ajoutées dans {pCtrValTradeInfo}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCtrValTradeInfo"></param>
        /// <param name="pFlowCur"></param>
        /// <returns></returns>
        /// PM 20140918 [20066][20185] New
        // EG 20180205 [23769] Add dbTransaction  
        private CBFlows ProcessDetailCounterValue(string pCS, IDbTransaction pDbTransaction, CBCtrValTradeInfo pCtrValTradeInfo, CBFlows pFlowCur)
        {
            if ((null != pCtrValTradeInfo) && (null != pFlowCur))
            {
                List<CBDetFlow> flows = pFlowCur.Flows.ToList();
                foreach (CBDetFlow flow in flows)
                {
                    flow.CtrValAmount = ProcessDetailCounterValue(pCS, pDbTransaction, pCtrValTradeInfo, flow.CurrencyAmount);
                }
            }
            return pFlowCur;
        }
        /// <summary>
        /// Calcul la contrevaleur de chaque montant de l'ensemble {pFlowCur} dans la devise de contrevaleur lu dans {pCtrValTradeInfo},
        /// et retourne une liste de ces contrevaleurs.
        /// Les cotations utilisées sont ajoutées dans {pCtrValTradeInfo}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCtrValTradeInfo"></param>
        /// <param name="pFlowCur"></param>
        /// <returns></returns>
        /// PM 20140918 [20066][20185] New
        // EG 20180205 [23769] Add dbTransaction  
        private List<Money> ProcessDetailCounterValue(string pCS, IDbTransaction pDbTransaction, CBCtrValTradeInfo pCtrValTradeInfo, IEnumerable<Money> pFlowCur)
        {
            List<Money> ret = null;
            if ((null != pCtrValTradeInfo) && (null != pFlowCur))
            {
                IEnumerable<Money> inCurrencyAmount = pFlowCur.Where(m => m.Currency == pCtrValTradeInfo.Currency);
                List<Money> toConvertAmount = pFlowCur.Where(m => m.Currency != pCtrValTradeInfo.Currency).ToList();

                // Calcul de la contrevaleur
                ProcessCounterValue(pCS, pDbTransaction, pCtrValTradeInfo.Currency, toConvertAmount, out List<CBExAmount> exAmount, out Money sumExAmount);
                // Sauvegarde des cotations
                if (exAmount != null)
                {
                    IEnumerable<CBQuote> allQuote =
                        from amount in exAmount
                        where (null != amount)
                        from quote in amount.Quote
                        select quote;
                    //
                    pCtrValTradeInfo.Quote = allQuote.Union(pCtrValTradeInfo.Quote, new CBQuoteComparer()).ToList();
                }
                else
                {
                    exAmount = new List<CBExAmount>();
                }
                // Concat des montants déjà en devise de contrevaleur et de ceux convertis
                ret = inCurrencyAmount.Concat(
                    from amount in exAmount
                    where (null != amount)
                    select amount.CurrencyAmount
                    ).ToList();

                // RD 20171107 [23541] Add
                if (ret == null || ret == default || ArrFunc.IsEmpty(ret))
                {
                    Money zero = new Money(0, pCtrValTradeInfo.Currency);
                    ret = new List<Money>
                    {
                        zero
                    };
                }
            }
            return ret;
        }
        #endregion

        /// <summary>
        /// Calcul la contrevaleur de chaque montant de la liste {pMoney} dans la devise {pIdCExchange}
        /// <para>en plus de la somme de de toutes les contrevaleurs</para>
        /// <para>NB: Le cas échéant, une devise pivot est utilisée:</para>
        /// <para>- soit la devise comptable de l'entité si elle existe</para>
        /// <para>- ou bien la devise "EUR" par défaut</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdCExchange"></param>
        /// <param name="pMoney"></param>
        /// <param name="pExAmount"></param>
        /// <param name="pMoney_ExSUM"></param>
        // EG 20180205 [23769] Add dbTransaction  
        private void ProcessCounterValue(string pCS, IDbTransaction pDbTransaction, string pIdCExchange, List<Money> pMoney,
            out List<CBExAmount> pExAmount, out Money pMoney_ExSUM)
        {
            // RD 20160112 [21748] Use accounting currency as cross currency
            ProcessCounterValue(pCS, pDbTransaction, pIdCExchange, _accountingCurrency, pMoney, out pExAmount, out pMoney_ExSUM);
        }

        ///// <summary>
        ///// Calcul la contrevaleur de chaque montant de la liste {pMoney} dans la devise {pIdCExchange}
        ///// <para>en plus de la somme de de toutes les contrevaleurs</para>
        ///// <para>NB: Si une devise pivot {pIdCPivot} est spécifiée, alors elle sera utilisée le cas échéant</para>
        ///// </summary>
        ///// <param name="pCS"></param>
        ///// <param name="pIdCExchange"></param>
        ///// <param name="pMoney"></param>
        ///// <param name="pExAmount"></param>
        ///// <param name="pMoney"></param>
        //private void ProcessCounterValue(string pCS, string pIdCExchange, string pIdCPivot, List<Money> pMoney,
        //    out Money pMoney_ExSUM)
        //{
        //    List<CBExAmount> exAmount;

        //    ProcessCounterValue(pCS, pIdCExchange, pIdCPivot, pMoney, out exAmount, out pMoney_ExSUM);
        //}

        /// <summary>
        /// Calcul la contrevaleur de chaque montant de la liste {pMoney} dans la devise {pIdCExchange}
        /// <para>en plus de la somme de de toutes les contrevaleurs</para>
        /// <para>NB: Si une devise pivot {pIdCPivot} est spécifiée, alors elle sera utilisée le cas échéant</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdCExchange"></param>
        /// <param name="pIdCPivot"></param>
        /// <param name="pMoney"></param>
        /// <param name="pExAmount"></param>
        /// <param name="pMoney_ExSUM"></param>
        // EG 20180205 [23769] Add dbTransaction  
        private void ProcessCounterValue(string pCS, IDbTransaction pDbTransaction, string pIdCExchange, string pIdCPivot, List<Money> pMoney,
            out List<CBExAmount> pExAmount, out Money pMoney_ExSUM)
        {
            pExAmount = new List<CBExAmount>();
            List<CBQuote> quote = new List<CBQuote>();

            IEnumerable<Money> fxMoney =
                from money in pMoney
                where (money.Amount.DecValue != 0) && (money.Currency != pIdCExchange)
                select money;

            foreach (Money money in fxMoney)
            {
                if (money.Currency != pIdCExchange)
                {
                    decimal amount = money.Amount.DecValue;
                    quote = ReadQuote_FXRate(pCS, pDbTransaction, money.Currency, pIdCExchange, pIdCPivot, ref amount);
                    pExAmount.Add(new CBExAmount(money.Currency, new Money(amount, pIdCExchange), quote));
                }
            }

            IEnumerable<Money> moneyCTRValCur =
                (from money in pMoney
                 where money.Currency == pIdCExchange
                 select money)
                .Union
                (from fxAmount in pExAmount
                 select fxAmount.CurrencyAmount);
            //
            pMoney_ExSUM = new Money(moneyCTRValCur.Sum(money => money.Amount.DecValue), pIdCExchange);
        }

        /// <summary>
        /// Charge le taux de change de la devise {pIdC1} en devise {pIdC2}, et calcul la contre valeur du montant {pAmount}
        /// <para>NB: Si une devise pivot {pIdCPivot} est spécifiée, alors elle sera utilisée la cas échéant</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdC1"></param>
        /// <param name="pIdC2"></param>
        /// <param name="pIdCPivot"></param>
        /// <param name="pAmount"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        private List<CBQuote> ReadQuote_FXRate(string pCS, IDbTransaction pDbTransaction, string pIdC1, string pIdC2, string pIdCPivot, ref decimal pAmount)
        {
            // [TODO RD: Optimiser pour ne pas charger la cotation plusieurs fois]
            ProcessState processState = null;
            string message = string.Empty;

            List<CBQuote> retCBQuote = new List<CBQuote>();
            QuoteBasisEnum quoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
            decimal exchangeRate = 0;

            if (pIdC1 != pIdC2)
            {

                //Recherche prix OfficialClose/close
                KeyQuote keyQuote = new KeyQuote(pCS, pDbTransaction, DtBusiness, null, null, QuotationSideEnum.OfficialClose, QuoteTimingEnum.Close);

                KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                {
                    IdC1 = pIdC1,
                    IdC2 = pIdC2
                };
                keyAssetFXRate.SetQuoteBasis(true);

                SQL_Quote quote = new SQL_Quote(pCS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled,
                                                            new ExchangeTradedDerivative(), keyQuote, keyAssetFXRate)
                {
                    DbTransaction = pDbTransaction
                };

                bool isLoaded = quote.IsLoaded;
                bool ret = isLoaded;

                if (isLoaded)
                {
                    ret = isLoaded && (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);

                    if (ret)
                    {
                        exchangeRate = quote.QuoteValue;
                        quoteBasis = keyAssetFXRate.QuoteBasis;

                        FpML.v44.Shared.FxRate retRate = new FpML.v44.Shared.FxRate
                        {
                            quotedCurrencyPair = new QuotedCurrencyPair(pIdC1, pIdC2, quoteBasis),
                            rate = new EFS_Decimal(exchangeRate)
                        };

                        retCBQuote.Add(new CBQuote(retRate, quote.IdQuote));
                    }
                }

                if (ret == false)
                {
                    // EG 20180411 [23769] Set & Use dbTransaction
                    ProcessStateTools.StatusEnum status = (keyQuote.Time <= OTCmlHelper.GetDateBusiness(pCS, pDbTransaction)) ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusWarningEnum;
                    processState = new ProcessState(status, Cst.ErrLevel.QUOTENOTFOUND);

                    if (((KeyAssetFxRate)quote.KeyAssetIN).QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
                        message += keyAssetFXRate.IdC2 + "./" + keyAssetFXRate.IdC1;
                    else
                        message += keyAssetFXRate.IdC1 + "./" + keyAssetFXRate.IdC2;

                    message += " " + quote.QuoteValueMessage + Cst.CrLf2;
                    message += "Fixing details:" + Cst.CrLf;
                    message += "- Basis: " + keyAssetFXRate.QuoteBasis.ToString() + Cst.CrLf;
                    message += "- Type: " + (keyQuote.QuoteSide.HasValue ? keyQuote.QuoteSide.ToString() : "{null}") + Cst.CrLf;
                    message += "- QuoteTiming: " + (keyQuote.QuoteTiming.HasValue ? keyQuote.QuoteTiming.ToString() : "{null}") + Cst.CrLf;
                    message += "- Time: " + DtFunc.DateTimeToStringISO(keyQuote.Time) + Cst.CrLf;
                    message += "- Market Environment: " + (StrFunc.IsFilled(keyQuote.IdMarketEnv) ? keyQuote.IdMarketEnv.ToString() : "{null}") + Cst.CrLf;
                    message += "- Val. Scenario: " + (StrFunc.IsFilled(keyQuote.IdValScenario) ? keyQuote.IdValScenario.ToString() : "{null}") + Cst.CrLf;
                }

                if (isLoaded == false && StrFunc.IsFilled(pIdCPivot))
                {
                    // Tentative de recherche du fixing en passant par la devise pivot                    
                    if (quote.QuoteValueCodeReturn == Cst.ErrLevel.QUOTENOTFOUND)
                    {

                        ret = quote.GetQuoteByPivotCurrency(pIdCPivot, out KeyQuoteFxRate quote_IdC1vsPivot, out KeyQuoteFxRate quote_IdC2vsPivot);

                        if (ret)
                        {
                            exchangeRate = quote.QuoteValue;
                            quoteBasis = keyAssetFXRate.QuoteBasis;

                            FpML.v44.Shared.FxRate retRate = new FpML.v44.Shared.FxRate
                            {
                                quotedCurrencyPair = new QuotedCurrencyPair(pIdC1, pIdCPivot, quote_IdC1vsPivot.QuoteBasis),
                                rate = new EFS_Decimal(quote_IdC1vsPivot.QuoteValue)
                            };

                            retCBQuote.Add(new CBQuote(retRate, quote_IdC1vsPivot.IdQuote));

                            retRate = new FpML.v44.Shared.FxRate
                            {
                                quotedCurrencyPair = new QuotedCurrencyPair(pIdC2, pIdCPivot, quote_IdC2vsPivot.QuoteBasis),
                                rate = new EFS_Decimal(quote_IdC2vsPivot.QuoteValue)
                            };

                            retCBQuote.Add(new CBQuote(retRate, quote_IdC2vsPivot.IdQuote));

                            // PM 20160118 [] Ajout Spot rate calculé
                            retRate = new FpML.v44.Shared.FxRate
                            {
                                quotedCurrencyPair = new QuotedCurrencyPair(pIdC1, pIdC2, quoteBasis),
                                rate = new EFS_Decimal(exchangeRate)
                            };

                            retCBQuote.Add(new CBQuote(retRate, 0));
                        }
                    }
                }

                if (ret)
                {
                    EFS_Cash cash = new EFS_Cash(pCS, pDbTransaction, pIdC1, pIdC2, pAmount, exchangeRate, quoteBasis);
                    pAmount = cash.ExchangeAmountRounded;
                }
                else
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, message, processState);
            }

            return retCBQuote;
        }

        /// <summary>
        /// return currencies for which there is amount Filled (!=0)
        /// </summary>
        /// <returns></returns>
        /// FI 20120913 [18125] Creation
        /// RD 20130621 Réfactoring
        public List<string> GetCurrencyFilledAmount()
        {
            List<string> ret = new List<string>();

            if (ArrFunc.IsFilled(Currencies))
            {
                foreach (string currency in Currencies)
                {
                    if (IsAmountCurrencyFilled(currency))
                        ret.Add(currency);
                }
            }

            if (ret.Count == 0)
                ret.Add(string.Empty);

            return ret;
        }

        /// <summary>
        /// Calculer des différents montants du Cash Balance avec la méthode UK
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel ProcessMethodUK(string pCS, IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region Collateral Available
            if ((null != FlowCollateral) && (FlowCollateral.Count > 0))
            {
                // Parcours des collaterals
                foreach (CBDetCollateral collateral in FlowCollateral)
                {
                    #region Recherche et application de l'Haircut
                    decimal haircut = 0;
                    //
                    // PM 20150901 HaircutForced devient nullable
                    // Si HaircutForced est renseigné, prendre cet haircut
                    if (collateral.HaircutForced.HasValue)
                    {
                        haircut = collateral.HaircutForced.Value;
                    }
                    // Sinon rechercher l'haircut dans les instructions
                    else if (collateral.collateralEnv != null)
                    {
                        // Normalement collateral.collateralEnv contient à minima un élément avec des valeurs par défaut (dont un haircut à null donnant 0 avec HaircutValue() ).
                        //
                        // Rechercher les différents Haircut possible dans les instructions
                        IEnumerable<decimal> distinctHaircut = (
                            from colEnv in collateral.collateralEnv
                            where colEnv.IsCollateralAllowed
                            select colEnv.HaircutValue()).Distinct();
                        //
                        // Vérifier que le Haircut est toujours le même
                        if (distinctHaircut.Count() > 1)
                        {
                            // Problème, plusieurs Haircut sont applicables
                            // On ne devrait pas être dans ce cas
                            haircut = 0;
                            string hclist = distinctHaircut.Select(hc => hc.ToString()).Aggregate((a, b) => a + "; " + b);
                            
                            // FI 20200623 [XXXXX] SetErrorWarning
                            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusWarningEnum;
                            SetErrorWarning.Invoke(ProcessStateTools.StatusWarningEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 4004), 3,
                                new LogParam(collateral.AssetCategory),
                                new LogParam(collateral.IdAsset),
                                new LogParam(hclist)));

                            //ret = Cst.ErrLevel.MULTIDATAFOUND;
                        }
                        else
                        {
                            // Prendre l'haircut des instructions (0 si jamais il n'y avait aucune instruction)
                            haircut = distinctHaircut.FirstOrDefault();
                        }
                    }
                    // Interdire un haircut > 100 qui donnerait un montant négatif
                    haircut = System.Math.Min(100, haircut);
                    //
                    //Appliquer l'Haircut
                    collateral.CollatAvailable_FlowCur =
                        (from money in collateral.CurrencyAmount
                         group money by money.Currency
                             into moneyByIDC
                             select new Money(
                                 moneyByIDC.Sum(m => CBTools.ApplyHaircut(m.amount.DecValue, haircut)),
                                 moneyByIDC.Key)
                        ).ToList();
                    
                    
                    
                    #endregion
                }
            }
            #endregion Collateral Available
            #region Solde
            //------------------------------------------------------------------
            // Solde = 
            //  + Previous Cash Balance
            //  + Premium
            //  + Realized Margin
            //  + Cash Settlement
            //  + Fees and Tax
            //  + Safe Keeping Payment
            //  + Funding
            //  + Borrowing
            //  + Cash Deposit
            //  - Cash Withdrawal
            //  + Equalisation Payment
            //------------------------------------------------------------------
            IEnumerable<Money> previousCashBalance = IsCBOWithManagementBalance ? PrevCashBalance_FlowCur : new List<Money>();
            IEnumerable<Money> premium = CashFlowPRM_FlowCur.AllCurrencyAmount();
            IEnumerable<Money> cashSettlement = CashFlowSCU_FlowCur.AllCurrencyAmount();
            IEnumerable<Money> feesTax = CashFlowOPP_FlowCur.AllCurrencyAmount();
            // PM 20150709 [21103] Add CashFlowSKP_FlowCur
            IEnumerable<Money> safeKeepingPayment = CashFlowSKP_FlowCur.AllCurrencyAmount();
            IEnumerable<Money> funding = CashFlowFDA_FlowCur.AllCurrencyAmount();
            IEnumerable<Money> borrowing = CashFlowBWA_FlowCur.AllCurrencyAmount();
            // PM 20141209 : Ne pas prendre les RMG sur Option Premium Style
            IEnumerable<Money> realizedMargin =
                from flow in CashFlowRMG_FlowCur.CashFlows
                where (flow != null) && (flow.CurrencyAmount != null )
                && ((false == flow.Category.HasValue) // OTC
                    || (flow.Category.HasValue // ETD
                        && ((flow.Category.Value != CfiCodeCategoryEnum.Option) // Future
                            || (flow.FutValuationMethod.HasValue && (flow.FutValuationMethod.Value != FuturesValuationMethodEnum.PremiumStyle))  // Option Mark to Market
                        )))
                from money in flow.CurrencyAmount
                select money; 
            // Inversé le signe des Cash payment pour les calculs
            IEnumerable<Money> cashPayment = PaymentStl_FlowCur.AllCurrencyOppositeAmount();
            // PM 20170911 [23408] Add CashFlowEQP_FlowCur
            IEnumerable<Money> equalisationPayment = CashFlowEQP_FlowCur.AllCurrencyAmount();
            //
            IEnumerable<Money> allMoneyForCSB = 
                // Previous Cash Balance si Gestion des Soldes
                previousCashBalance
                // Premium
                .Concat(premium)
                // Realized Margin
                .Concat(realizedMargin)
                // Cash Settlement
                .Concat(cashSettlement)
                // Fees and Tax
                .Concat(feesTax)
                // PM 20150709 [21103] Add SafeKeepingPayment
                .Concat(safeKeepingPayment)
                // Funding
                .Concat(funding)
                // Borrowing
                .Concat(borrowing)
                // Cash Deposit & Cash Withdrawal
                .Concat(cashPayment)
                // PM 20170911 [23408] Add Equalisation Payment
                .Concat(equalisationPayment);
            //
            // Cumul par devise
            CashBalance_FlowCur = CBFlowTools.CalcSumByIdC(allMoneyForCSB);
            #endregion
            #region Equity Balance
            //------------------------------------------------------------------
            // Equity Balance = 
            //  + Cash Balance
            //  + Unrealized Margin (Future & Option Mark-To-Market)
            //  + Unsettled Cash
            //------------------------------------------------------------------
            IEnumerable<Money> unrealizedMargin = CashFlowUMG_FlowCur.AllCurrencyAmount();
            // PM 20141208 : Ne pas prendre les UMG sur Option Premium Style
            IEnumerable<Money> openTradeEquity =
                from flow in CashFlowUMG_FlowCur.CashFlows
                where (flow != null) && (flow.CurrencyAmount != null )
                && ((false == flow.Category.HasValue) // OTC
                    || (flow.Category.HasValue // ETD
                        && ((flow.Category.Value != CfiCodeCategoryEnum.Option) // Future
                            || (flow.FutValuationMethod.HasValue && (flow.FutValuationMethod.Value != FuturesValuationMethodEnum.PremiumStyle))  // Option Mark to Market
                        )))
                from money in flow.CurrencyAmount
                select money; 
            //
            IEnumerable<Money> unsettledCash = CashFlowUST_FlowCur.AllCurrencyAmount();
            //
            IEnumerable<Money> allMoneyForE_B =
                // Cash Balance
                CashBalance_FlowCur
                // Open Trade Equity
                .Concat(openTradeEquity)
                // Unsettled Cash
                .Concat(unsettledCash);
            //
            // Cumul par devise
            EquityBalance_FlowCur = CBFlowTools.CalcSumByIdC(allMoneyForE_B);
            #endregion
            #region Equity Balance With Forward Cash
            //------------------------------------------------------------------
            // Equity Balance = Equity Balance + Forward Cash Payment
            //------------------------------------------------------------------
            // Forward Cash
            IEnumerable<Money> cashForward = ForwardPaymentStl_FlowCur.AllCurrencyOppositeAmount();
            //
            IEnumerable<Money> allMoneyForEBF =
                // Equity Balance
                EquityBalance_FlowCur
                // Forward Cash Payment
                .Concat(cashForward);
            //
            // Cumul par devise
            EquityBalanceForwardCash_FlowCur = CBFlowTools.CalcSumByIdC(allMoneyForEBF);
            #endregion
            #region Option Value
            // OptionValue uniquement pour Option PremiumStyle
            // PM 20150409 [POC] Ajout des LOV des options OTC
            //IEnumerable<CBDetFlow> CashFlowLOV_PremiumStyle =
            //    from flow in CashFlowLOV_FlowCur.CashFlows
            //    where flow.FutValuationMethodSpecified && (flow.FutValuationMethod == FuturesValuationMethodEnum.PremiumStyle)
            //    select (CBDetFlow)flow;
            IEnumerable<CBDetFlow> CashFlowLOV_PremiumStyle =
                from flow in CashFlowLOV_FlowCur.CashFlows
                where (flow.FutValuationMethodSpecified && (flow.FutValuationMethod == FuturesValuationMethodEnum.PremiumStyle))
                 || (false == flow.CategorySpecified) // OTC
                select (CBDetFlow)flow;
            //
            CashFlowLOV_FlowCur = new CBFlows(CashFlowLOV_PremiumStyle);
            #endregion
            #region Total Account Value
            //PM 20150616 [21124] add Total Account Value
            //------------------------------------------------------------------
            // Total Account Value = Equity Balance + Market Value + Net Option Value
            //------------------------------------------------------------------
            IEnumerable<Money> marketValue = CashFlowMKV_FlowCur.AllCurrencyAmount();
            IEnumerable<Money> netOptionValue = CashFlowLOV_FlowCur.AllCurrencyAmount();
            IEnumerable<Money> allMoneyForTotalAccountValue =
                // Equity Balance
                EquityBalance_FlowCur
                // Market Value
                .Concat(marketValue)
                //  Net Option Value
                .Concat(netOptionValue);
            //
            // Cumul par devise
            TotalAccountValue_FlowCur = CBFlowTools.CalcSumByIdC(allMoneyForTotalAccountValue);
            #endregion
            #region Excess / Deficit
            //------------------------------------------------------------------
            // Excess / Deficit = Equity Balance - Margin Requirement + Collateral
            //------------------------------------------------------------------
            IEnumerable<Money> marginRequirement = from flow in FlowDeposit
                                                   where (flow != null) && (flow.CurrencyAmount != null)
                                                   from money in flow.CurrencyAmount
                                                   select money;
            //
            IEnumerable<Money> collateralAvailable = from flow in FlowCollateral
                                                     where (flow != null) && (flow.CurrencyAmount != null)
                                                     from money in flow.CollatAvailable_FlowCur
                                                     select new Money(money.Amount.DecValue * -1, money.Currency);
            //
            IEnumerable<Money> allMoneyForE_D =
                // Equity Balance
                EquityBalance_FlowCur
                // Margin Requirement
                .Concat(marginRequirement)
                // Collateral Available
                .Concat(collateralAvailable);
            //
            // Cumul par devise
            ExcessDeficit_FlowCur = CBFlowTools.CalcSumByIdC(allMoneyForE_D);
            #endregion
            #region Excess / Deficit With Forward Cash
            //------------------------------------------------------------------
            // Excess / Deficit = Excess / Deficit + Forward Cash Payment
            //------------------------------------------------------------------
            IEnumerable<Money> allMoneyForEDF =
                // Excess / Deficit
                ExcessDeficit_FlowCur
                // Forward Cash Payment
                .Concat(cashForward);
            //
            // Cumul par devise
            ExcessDeficitForwardCash_FlowCur = CBFlowTools.CalcSumByIdC(allMoneyForEDF);
            #endregion
            #region Cash Available
            // Pour info : non utilisé par la méthode UK
            IEnumerable<Money> variationMargin = CashFlowVMG_FlowCur.AllCurrencyAmount();
            // PM 20150709 [21103] Add SafeKeepingPayment
            IEnumerable<Money> cashFlows = variationMargin.Concat(premium.Concat(cashSettlement.Concat(feesTax.Concat(safeKeepingPayment))));
            IEnumerable<Money> cashAvailable = cashFlows.Concat(previousCashBalance.Concat(cashPayment));
            // Cumul par devise
            CashAvailable_FlowCur = CBFlowTools.CalcSumByIdC(cashAvailable);
            #endregion

            #region ContreValeur
            CtrValTradeInfo = new CBCtrValTradeInfo();
            if (StrFunc.IsFilled(CashBalanceIDC))
            {
                #region Lecture des montants par devise
                //PM 20150320 [POC] Add cashSettlementOptionPremium, cashSettlementOptionMarkToMarket, cashSettlementOther
                IEnumerable<Money> cashSettlementOptionPremium = CashFlowSCU_FlowCur.AllCurrencyETDOptionAmount(FuturesValuationMethodEnum.PremiumStyle);
                IEnumerable<Money> cashSettlementOptionMarkToMarket = CashFlowSCU_FlowCur.AllCurrencyETDOptionAmount(FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
                IEnumerable<Money> cashSettlementOther = CashFlowSCU_FlowCur.AllCurrencyOTCAmount();
                //PM 20141030 Inverser le signe des CashPayment
                IEnumerable<Money> cashPaymentDeposit = cashPayment.Where(m => m.Amount.DecValue > 0).Select(m => new Money(-1 * m.Amount.DecValue, m.Currency));
                IEnumerable<Money> cashPaymentWithdrawal = cashPayment.Where(m => m.Amount.DecValue < 0).Select(m => new Money(-1 * m.Amount.DecValue, m.Currency));
                // Reprendre le CashPayment avec le signe initial pour la génération des événements
                cashPayment = PaymentStl_FlowCur.AllCurrencyAmount();
                //PM 20141030 Inverser le signe des CashForwardPayment
                IEnumerable<Money> cashForwardDeposit = cashForward.Where(m => m.Amount.DecValue > 0).Select(m => new Money(-1 * m.Amount.DecValue, m.Currency));
                IEnumerable<Money> cashForwardWithdrawal = cashForward.Where(m => m.Amount.DecValue < 0).Select(m => new Money(-1 * m.Amount.DecValue, m.Currency));
                // Reprendre le CashForwardPayment avec le signe initial pour la génération des événements
                cashForward = ForwardPaymentStl_FlowCur.AllCurrencyAmount();
                IEnumerable<Money> longOptionValue = CashFlowLOV_FlowCur.AllCurrencyAmountBySide(SideEnum.Buy);
                IEnumerable<Money> shortOptionValue = CashFlowLOV_FlowCur.AllCurrencyAmountBySide(SideEnum.Sell);
                #region realizedMargin
                // Reprendre tous les RMG
                realizedMargin = CashFlowRMG_FlowCur.AllCurrencyAmount();
                IEnumerable<Money> realizedMarginFuture = CashFlowRMG_FlowCur.AllCurrencyETDFutureAmount();
                IEnumerable<Money> realizedMarginOptionPremium = CashFlowRMG_FlowCur.AllCurrencyETDOptionAmount(FuturesValuationMethodEnum.PremiumStyle);
                IEnumerable<Money> realizedMarginOptionMarkToMarket = CashFlowRMG_FlowCur.AllCurrencyETDOptionAmount(FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
                //PM 20150320 [POC] Add realizedMarginOther
                IEnumerable<Money> realizedMarginOther = CashFlowRMG_FlowCur.AllCurrencyOTCAmount();
                #endregion realizedMargin
                #region unrealizedMargin
                IEnumerable<Money> unrealizedMarginFuture = CashFlowUMG_FlowCur.AllCurrencyETDFutureAmount();
                IEnumerable<Money> unrealizedMarginOptionPremium = CashFlowUMG_FlowCur.AllCurrencyETDOptionAmount(FuturesValuationMethodEnum.PremiumStyle);
                IEnumerable<Money> unrealizedMarginOptionMarkToMarket = CashFlowUMG_FlowCur.AllCurrencyETDOptionAmount(FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
                //PM 20150320 [POC] Add unrealizedMarginOther
                IEnumerable<Money> unrealizedMarginOther = CashFlowUMG_FlowCur.AllCurrencyOTCAmount();
                #endregion unrealizedMargin
                IEnumerable<Money> collateralAvailableForCtrVal =
                    from flow in FlowCollateral
                    where (flow != null) && (flow.CurrencyAmount != null)
                    from money in flow.CollatAvailable_FlowCur
                    select money;
                #endregion
                #region Calcul des contrevaleurs
                string csCacheOn = CSTools.SetCacheOn(pCS);
                CtrValTradeInfo.Currency = CashBalanceIDC;
                CtrValTradeInfo.PreviousCashBalance = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, previousCashBalance);
                CtrValTradeInfo.Premium = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, premium);
                // PM 20150616 [21124] Remplacé par le détail
                //CtrValTradeInfo.RealizedMargin = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, realizedMargin);
                //CtrValTradeInfo.RealizedMarginFuture = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, realizedMarginFuture);
                //CtrValTradeInfo.RealizedMarginOptionPremium = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, realizedMarginOptionPremium);
                //CtrValTradeInfo.RealizedMarginOptionMarkToMarket = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, realizedMarginOptionMarkToMarket);
                ////PM 20150320 [POC] Add RealizedMarginOther
                //CtrValTradeInfo.RealizedMarginOther = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, realizedMarginOther);
                CtrValTradeInfo.CashSettlement = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashSettlement);
                //PM 20150320 [POC] Add CashSettlementOptionPremium, CashSettlementOptionMarkToMarket, CashSettlementOther
                CtrValTradeInfo.CashSettlementOptionPremium = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashSettlementOptionPremium);
                CtrValTradeInfo.CashSettlementOptionMarkToMarket = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashSettlementOptionMarkToMarket);
                CtrValTradeInfo.CashSettlementOther = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashSettlementOther);
                // Remplacé par CtrValTradeInfo.CBFlowsFeesAndTax
                //CtrValTradeInfo.Fee = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, feesTax);
                if ((funding != null) && (funding.Count() > 0))
                {
                    CtrValTradeInfo.Funding = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, funding);
                }
                //PM 20150323 [POC] Add Borrowing
                if ((borrowing != null) && (borrowing.Count() > 0))
                {
                    CtrValTradeInfo.Borrowing = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, borrowing);
                }
                //PM 20150319 [POC] Add Unsettled Cash
                // PM 20150616 [21124] Remplacé par le détail
                //CtrValTradeInfo.UnsettledCash = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, unsettledCash);
                CtrValTradeInfo.CashPayment = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashPayment);
                CtrValTradeInfo.CashPaymentDeposit = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashPaymentDeposit);
                CtrValTradeInfo.CashPaymentWithdrawal = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashPaymentWithdrawal);
                CtrValTradeInfo.CashBalance = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, CashBalance_FlowCur);
                // PM 20150616 [21124] Remplacé par le détail
                //CtrValTradeInfo.UnrealizedMargin = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, unrealizedMargin);
                //CtrValTradeInfo.UnrealizedMarginFuture = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, unrealizedMarginFuture);
                //CtrValTradeInfo.UnrealizedMarginOptionPremium = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, unrealizedMarginOptionPremium);
                //CtrValTradeInfo.UnrealizedMarginOptionMarkToMarket = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, unrealizedMarginOptionMarkToMarket);
                ////PM 20150320 [POC] Add UnrealizedMarginOther
                //CtrValTradeInfo.UnrealizedMarginOther = ProcessSumCounterValue(csCacheOn, CtrValTradeInfo, unrealizedMarginOther);
                if (((netOptionValue != null) && (netOptionValue.Count() > 0))
                    || ((longOptionValue != null) && (longOptionValue.Count() > 0))
                    || ((shortOptionValue != null) && (shortOptionValue.Count() > 0)))
                {
                    CtrValTradeInfo.OptionValue = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, netOptionValue);
                    CtrValTradeInfo.LongOptionValue = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, longOptionValue);
                    CtrValTradeInfo.ShortOptionValue = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, shortOptionValue);
                }
                //PM 20150616 [21124] add MarketValue
                if ((marketValue != null) && (marketValue.Count() > 0))
                {
                    CtrValTradeInfo.MarketValue = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, marketValue);
                }
                CtrValTradeInfo.EquityBalance = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, EquityBalance_FlowCur);
                CtrValTradeInfo.CashForward = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashForward);
                CtrValTradeInfo.CashForwardDeposit = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashForwardDeposit);
                CtrValTradeInfo.CashForwardWithdrawal = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashForwardWithdrawal);
                CtrValTradeInfo.EquityBalanceWithForwardCash = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, EquityBalanceForwardCash_FlowCur);
                CtrValTradeInfo.MarginRequirement = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, marginRequirement);
                CtrValTradeInfo.CollateralAvailable = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, collateralAvailableForCtrVal);
                //PM 20150616 [21124] add TotalAccountValue
                CtrValTradeInfo.TotalAccountValue = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, TotalAccountValue_FlowCur);
                CtrValTradeInfo.ExcessDeficit = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, ExcessDeficit_FlowCur);
                CtrValTradeInfo.ExcessDeficitWithForwardCash = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, ExcessDeficitForwardCash_FlowCur);
                // Pour info : non utilisé par la méthode UK
                CtrValTradeInfo.VariationMargin = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, variationMargin);
                CtrValTradeInfo.CashFlows = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, cashFlows);
                CtrValTradeInfo.CashAvailable = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, CashAvailable_FlowCur);
                // PM 20170911 [23408] Add EqualisationPayment
                if ((CashFlowEQP_FlowCur != null) && (CashFlowEQP_FlowCur.Flows.Count() > 0))
                {
                    CtrValTradeInfo.EqualisationPayment = ProcessSumCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, CashFlowEQP_FlowCur);
                }
                else
                {
                    CtrValTradeInfo.EqualisationPayment = default;
                }
                // PM 20141218 Ajout détail des Fees et Tax
                CtrValTradeInfo.CBFlowsFeesAndTax = ProcessDetailCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, CashFlowOPP_FlowCur);
                // PM 20150709 [21103] Add CBFlowsSafeKeepingPayment
                CtrValTradeInfo.CBFlowsSafeKeepingPayment = ProcessDetailCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, CashFlowSKP_FlowCur);
                // PM 20150616 [21124] Add détail for CBFlowsRealizedMargin, CBFlowsUnrealizedMargin, CBFlowsUnsettledCash
                CtrValTradeInfo.CBFlowsRealizedMargin = ProcessDetailCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, this.CashFlowRMG_FlowCur);
                CtrValTradeInfo.CBFlowsUnrealizedMargin = ProcessDetailCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, this.CashFlowUMG_FlowCur);
                CtrValTradeInfo.CBFlowsUnsettledCash = ProcessDetailCounterValue(csCacheOn, pDbTransaction, CtrValTradeInfo, this.CashFlowUST_FlowCur);
                #endregion
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusErrorEnum;
                SetErrorWarning.Invoke(status);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4005), 3, new LogParam(LogTools.IdentifierAndId(SqlActor.Identifier, SqlActor.Id))));

                ret = Cst.ErrLevel.DATANOTFOUND;
            }
            #endregion
            return ret;
        }
        
        /// <summary>
        /// Arrondi de tous les flux chargés (ils sont déjà cumulés par Actor/Book/DC(ou Asset)/PaymentType(pour les OPP))
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCBCache"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public void RoundDetailledAmounts(string pCS, IDbTransaction pDbTransaction, CBCache pCBCache)
        {
            if (Settings.Default.IsRoundedCashBalance)
            {
                // BWA
                CashFlowBWA_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // EQP
                // PM 20170911 [23408]  Add CashFlowEQP_FlowCur
                CashFlowEQP_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // FDA
                CashFlowFDA_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // LOV
                CashFlowLOV_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // MKV
                CashFlowMKV_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // OPP
                CashFlowOPP_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // PRM
                CashFlowPRM_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // RMG
                CashFlowRMG_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // SCU
                CashFlowSCU_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // SKP
                // PM 20150709 [21103] Add CashFlowSKP_FlowCur
                CashFlowSKP_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // UMG
                CashFlowUMG_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // UST
                CashFlowUST_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                // VMG
                CashFlowVMG_FlowCur.RoundAmounts(pCS, pDbTransaction, pCBCache);
                //
                // MGR
                FlowDeposit = CBFlowTools.RoundAmount(pCS, pDbTransaction, pCBCache, FlowDeposit);
                //
                // Collateral
                FlowCollateral = CBFlowTools.RoundAmount(pCS, pDbTransaction, pCBCache, FlowCollateral);
                //
                // Previous Cash Balance
                PrevCashBalance_FlowCur = CBFlowTools.RoundAmount(pCS, pDbTransaction, pCBCache, PrevCashBalance_FlowCur);
            }
        }

        #endregion
    }

    /// <summary>
    /// Flow to be used to calculate Cash Balance
    /// </summary>
    ///PM 20140911 [20066][20185] Add SettlementPayment
    public enum FlowTypeEnum
    {
        /// <summary>
        /// Flux des trades allocation non fongible
        /// </summary>
        // PM 20170213 [21916] Ajout pour Commodity Spot
        AllocNotFungibleFlows,
        /// <summary>
        /// All Daily Cash-Flows (Variation Margin, Daily Premium, Daily Cash Settlement, Daily Other pary Payment (Fees))
        /// </summary>
        CashFlows,
        /// <summary>
        /// Daily Collaterals
        /// </summary>
        Collateral,
        /// <summary>
        /// Daily Deposit
        /// </summary>
        Deposit,
        /// <summary>
        /// Last Cash Balance Flows
        /// </summary>
        LastCashBalance,
        /// <summary>
        /// Other Daily flows (Liquidative Option Value, Realized Margin, Unrealized Margin)
        /// </summary>
        OtherFlows,
        /// <summary>
        /// Daily Payments 
        /// </summary>
        Payment,
        /// <summary>
        /// Payments regarding settlement date
        /// </summary>
        SettlementPayment,
        /// <summary>
        /// All Daily Flows on Trades (OTC Trades and Not in position Trades)
        /// </summary>
        //PM 20150318 [POC] Add TradeFlows
        TradeFlows,
        /// <summary>
        /// Current Cash Balance Flows
        /// </summary>
        // PM 20190909 [24826][24915] Add CurrentCashBalance
        CurrentCashBalance,
        /// <summary>
        /// Other
        /// </summary>
        None,
    }

    /// <summary>
    /// Flow Sub type to be used to calculate Cash Balance
    /// </summary>
    /// FI 20170217 [22862] Modify
    public enum FlowSubTypeEnum
    {
        /// <summary>
        /// Amount
        /// </summary>
        AMT,
        /// <summary>
        /// Borrowing
        /// </summary>
        BWA,
        /// <summary>
        /// Collateral Available
        /// </summary>
        CLA,
        /// <summary>
        /// Collateral Used
        /// </summary>
        CLU,
        /// <summary>
        /// Cash Available
        /// </summary>
        CSA,
        /// <summary>
        /// Cash Balance Amount
        /// </summary>
        CSB,
        /// <summary>
        /// Cash Used
        /// </summary>
        CSU,
        /// <summary>
        /// Currency 1
        /// </summary>
        CU1,
        /// <summary>
        /// Currency 2
        /// </summary>
        CU2,
        /// <summary>
        /// Delivery Amount
        /// </summary>
        /// FI 20170217 [22862] Add
        DVA,
        /// <summary>
        /// Equalisation Payment
        /// </summary>
        /// PM 20170911 [23408] Ajout Equalisation Payment
        EQP,
        /// <summary>
        /// Funding
        /// </summary>
        ///PM 20140908 [20066][20185] Add FDA
        FDA,
        /// <summary>
        /// Gross Amount
        /// </summary>
        ///PM 20150318 [POC] Add GAM
        GAM,
        /// <summary>
        /// Interest
        /// </summary>
        ///PM 20151208 [21317] Ajout INT/INT sur la famille de product DSE
        INT,
        /// <summary>
        /// Liquidative Option Value
        /// </summary>
        LOV,
        /// <summary>
        /// Margin Requierement
        /// </summary>
        MGR,
        /// <summary>
        /// Market Value
        /// </summary>
        MKV,
        /// <summary>
        /// Other pary Payment (Fees)
        /// </summary>
        OPP,
        /// <summary>
        /// Premium
        /// </summary>
        PRM,
        /// <summary>
        /// Realized Margin
        /// </summary>
        ///PM 20140908 [20066][20185] Add RMG
        RMG,
        /// <summary>
        /// Cash Settlement
        /// </summary>
        SCU,
        /// <summary>
        /// Safe Keeping Payment
        /// </summary>
        /// PM 20150709 [21103] Add SKP
        SKP,
        /// <summary>
        /// Unrealized Margin
        /// </summary>
        ///PM 20140908 [20066][20185] Add UMG
        UMG,
        /// <summary>
        /// Uncovered Margin Requierement
        /// </summary>
        UMR,
        /// <summary>
        /// Unsettled
        /// </summary>
        ///PM 20150324 [POC] Add Unsettled
        UST,
        /// <summary>
        /// Variation Margin
        /// </summary>
        VMG,
        /// <summary>
        /// Other
        /// </summary>
        None,
    }
    
    /// <summary>
    /// status pouvant être attribué sur les flux et les trades  
    /// <para>Seuls les flux, trades valides sont pris en compte dans le solde (Trade CB)</para>
    /// </summary>
    /// FI 20170208 [22151][22152] Add
    public enum StatusEnum
    {
        /// <summary>
        /// 
        /// </summary>
        Valid,
        /// <summary>
        /// 
        /// </summary>
        Unvalid
    }



    /// <summary>
    /// Méthodes de gestion de la hiérarchie
    /// </summary>
    public static class CBTools
    {
        /// <summary>
        /// Recherche la liste des premiers descendants (en commençant par l'acteur lui même) avec le le rôle {pRole}, dans la liste
        /// des acteurs {pActors}
        /// </summary>
        /// <param name="pActors"></param>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static List<CBActorNode> FindFirst(List<CBActorNode> pActors, RoleActor pRole)
        {
            List<CBActorNode> ret = new List<CBActorNode>();
            //
            foreach (CBActorNode actor in pActors)
            {
                if (actor.IsExistRole(pRole))
                {
                    ret.Add(actor);
                }
                else
                {
                    ret.AddRange(FindFirst(actor.ChildActors, pRole));
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRoles"></param>
        /// <returns></returns>
        public static List<RoleActor> GetRoles(List<ActorRole> pRoles)
        {
            List<RoleActor> retNodes = new List<RoleActor>();
            //
            foreach (ActorRole role in pRoles)
                retNodes.Add(role.Role);
            //
            return retNodes;
        }

        /// <summary>
        /// Appliquer l'abattement {pHaircut} en poucentage au montant {pAmount}
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pHaircut"></param>
        /// <returns></returns>
        public static decimal ApplyHaircut(decimal pAmount, decimal pHaircut)
        {
            return pAmount - (pAmount * pHaircut / 100);
        }

        /// <summary>
        /// Appliquer la majoration {pRise} en poucentage au montant {pAmount}, 
        /// pour revenir au même montant avant abattement
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pRise"></param>
        /// <returns></returns>
        public static decimal ApplyRise(decimal pAmount, decimal pRise)
        {
            if (pRise == 100)
                return 0;
            else
                return (pAmount * 1 / ((100 - pRise) / 100));
        }
    }

    /// <summary>
    /// Définition des priorités d'utilisation des Espèces, Titres et Actions lors du calcul de la couverture des appels de déposits.
    /// </summary>
    [XmlRoot(ElementName = "CollateralPriority")]
    public class CBCollateralPriority
    {
        /// <summary>
        /// L'acteur CBO
        /// </summary>
        [XmlAttribute(AttributeName = "ida_CBO")]
        public int Ida_CBO;

        /// <summary>
        /// Ordre de priorité
        /// </summary>
        [XmlAttribute(AttributeName = "priorityRank")]
        public int PriorityRank;

        /// <summary>
        /// Type
        /// </summary>
        [XmlAttribute(AttributeName = "collateralCategory")]
        public CollateralCategoryEnum CollateralCategory;

        /// <summary>
        /// Catégorie d'actif
        /// </summary>
        [XmlAttribute(AttributeName = "assetCategory")]
        public string AssetCategory;

        /// <summary>
        /// Actif
        /// </summary>
        [XmlAttribute(AttributeName = "idAsset")]
        public int IdAsset;

        public CBCollateralPriority() { }
        public CBCollateralPriority(int pIda_CBO, int pPriorityRank,
            CollateralCategoryEnum pCollateralCategory, string pAssetCategory, int pIdAsset)
        {
            Ida_CBO = pIda_CBO;
            //
            PriorityRank = pPriorityRank;
            CollateralCategory = pCollateralCategory;
            AssetCategory = pAssetCategory;
            IdAsset = pIdAsset;
        }
    }

    /// <summary>
    /// Définition des priorités de couverture des devises.
    /// </summary>
    [XmlRoot(ElementName = "CollateralCurrencyPriority")]
    public class CBCollateralCurrencyPriority
    {
        /// <summary>
        /// L'acteur CBO
        /// </summary>
        [XmlAttribute(AttributeName = "ida_CBO")]
        public int Ida_CBO;

        /// <summary>
        /// Ordre de priorité
        /// </summary>
        [XmlAttribute(AttributeName = "priorityRank")]
        public int PriorityRank;

        // FL/PL 20130912 [18865]: Rajout du paramètre isExistPriorityRank
        /// <summary>
        /// Indicateur d'existance de paramétrage d'un Ordre de priorité
        /// </summary>
        [XmlAttribute(AttributeName = "isExistPriorityRank")]
        public bool isExistPriorityRank;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "currency")]
        public string Currency;

        public CBCollateralCurrencyPriority() { }
        public CBCollateralCurrencyPriority(string pCurrency)
            : this(0, -1, pCurrency) { }
        public CBCollateralCurrencyPriority(int pIda_CBO, int pPriorityRank, string pCurrency)
        {
            Ida_CBO = pIda_CBO;
            //
            PriorityRank = pPriorityRank;
            Currency = pCurrency;
        }
    }

    /// <summary>
    /// Constitution de des Collatéraux disponibles pour chaque devise.
    /// </summary>
    [XmlRoot(ElementName = "CollateralConstituent")]
    public class CBCollateralConstituent
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "currency")]
        public string Currency;

        /// <summary>
        /// Représente les collatéraux déjà utilisés dans les devise précédentes
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("collateralAlreadyUsed", Order = 1)]
        public List<Money> AlreadyUsed;

        public CBCollateralConstituent() { }
        public CBCollateralConstituent(string pCurrency)
            : this(pCurrency, new List<Money>()) { }
        public CBCollateralConstituent(string pCurrency, List<Money> pAlreadyUsed)
        {
            Currency = pCurrency;
            AlreadyUsed = pAlreadyUsed;
        }
    }

    /// <summary>
    /// Classe portant les détails de chaque flux manipulé dans le Cash-Balance.
    /// </summary>    
    /// PM 20140917 [20066][20185] Add m_CtrValAmount
    /// PM 20150616 [21124] Add m_DtValue
    /// FI 20170208 [22151][22152] Modify
    /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
    [XmlRoot(ElementName = "FlowDetail")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CBDetCashFlows))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CBDetLastFlow))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CBDetStlPayment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CBDetDeposit))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CBDetCollateral))]
    public class CBDetFlow : IComparable
    {
        #region members
        protected bool m_IsLoaded;
        protected int m_IDB;
        protected int m_IDA;
        protected FlowTypeEnum m_FlowType;
        protected FlowSubTypeEnum m_FlowSubType;
        //
        protected DateTime m_DtMarketPrev;
        protected List<Money> m_CurrencyAmount;
        //
        protected List<Money> m_CtrValAmount;
        //
        /// <summary>
        /// Date valeur des flux
        /// </summary>
        protected DateTime m_DtValue;

        /// <summary>
        /// Statut du flux
        /// </summary>
        /// FI 20170208 [22151][22152] Add
        protected StatusEnum m_status;

        /// <summary>
        /// True si ce type de montant est complétement chargé à partir de la BD
        /// <para>Pour indiquer qu'un flux est complétement chargé à partir de la DB</para>
        /// <para>Car pendant le chargement de la DB, si on charge deux flux équivalents, leurs montants seront cumulés</para>
        /// <para>Voir la méthode Add(Money pCurrencyAmount)</para>        
        /// </summary>
        [XmlIgnore]
        public bool IsLoaded
        {
            get { return m_IsLoaded; }
            set { m_IsLoaded = value; }
        }
        /// <summary>
        /// Book portant le montant
        /// </summary>
        [XmlAttribute(AttributeName = "idb")]
        public int IDB
        {
            get { return m_IDB; }
            set { m_IDB = value; }
        }
        /// <summary>
        /// Acteur portant le montant
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int IDA
        {
            get { return m_IDA; }
            set { m_IDA = value; }
        }
        [XmlIgnore]
        public bool Ida_CssSpecified;
        /// <summary>
        /// L'Ida de la chambre de compensation d'un montant de Déposit ou d'un Collatéral
        /// </summary>
        [XmlAttribute(AttributeName = "ida_css")]
        public int Ida_Css;

        /// <summary>
        /// Type du flux
        /// </summary>
        [XmlAttribute(AttributeName = "type")]
        public FlowTypeEnum Type
        {
            get { return m_FlowType; }
            set { m_FlowType = value; }
        }
        /// <summary>
        /// Sous Type du montant
        /// </summary>
        [XmlAttribute(AttributeName = "subType")]
        public FlowSubTypeEnum FlowSubType
        {
            get { return m_FlowSubType; }
            set { m_FlowSubType = value; }
        }

        /// <summary>
        /// Liste des montants par devise
        /// </summary>
        [XmlArray(ElementName = "currencyAmounts")]
        [XmlArrayItemAttribute("currencyAmount")]
        public List<Money> CurrencyAmount
        {
            get { return m_CurrencyAmount; }
            set { m_CurrencyAmount = value; }
        }

        /// <summary>
        /// Liste des montants en contrevaleur
        /// </summary>
        [XmlArray(ElementName = "ctrValAmounts")]
        [XmlArrayItemAttribute("ctrValAmount")]
        public List<Money> CtrValAmount
        {
            get { return m_CtrValAmount; }
            set { m_CtrValAmount = value; }
        }

        /// <summary>
        /// Date valeur des flux
        /// </summary>
        [XmlAttribute(AttributeName = "dtValue")]
        public DateTime DtValue
        {
            get { return m_DtValue; }
            set { m_DtValue = value; }
        }

        /// <summary>
        /// status du flux
        /// </summary>
        /// FI 20170208 [22151][22152] Add
        [XmlAttribute(AttributeName = "status")]
        public StatusEnum Status
        {
            get { return m_status; }
            set { m_status = value; }
        }
        #endregion
        #region constructors
        /// <summary>
        /// Constructueur
        /// </summary>
        public CBDetFlow() { }
        /// <summary>
        /// Constructueur
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pFlowType"></param>
        /// <param name="pFlowSubType"></param>
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetFlow(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowTypeEnum pFlowType,
            FlowSubTypeEnum pFlowSubType, DateTime pDtValue, StatusEnum pStatus)
        {
            m_IsLoaded = false;
            //
            m_IDB = pIdb;
            m_IDA = pIda;
            m_FlowType = pFlowType;
            m_FlowSubType = pFlowSubType;
            //            
            m_CurrencyAmount = new List<Money>
            {
                new Money(pAmount, pCurrency)
            };
            //
            m_DtValue = pDtValue;
            m_status = pStatus;
        }
        #endregion
        #region methods
        /// <summary>
        /// Comparer le Flux courant à {pObj}, selon les critères suivants et dans l'ordre:
        /// <para>IDA</para>
        /// <para>IDB</para>
        /// <para>AmountType</para>
        /// <para>AmountSubType, s'il existe sur l'un des deux</para>
        /// <para>Ida_Css</para>
        /// <para>Status</para>
        /// </summary>
        /// <param name="pObj">L'objet à comparer avec le CBAmount courant</param>
        /// <returns>
        /// <para> 0 : égalité</para>
        /// <para>-1 : différence</para>
        /// </returns>
        /// FI 20170208 [22151][22152] Modify
        public virtual int CompareTo(object pObj)
        {
            if (pObj is CBDetFlow flow)
            {
                int ret = 0;
                //
                if (flow.IDA != IDA)
                    ret = -1;
                else if (flow.IDB != IDB)
                    ret = -1;
                else if (flow.Type != Type)
                    ret = -1;
                else if (flow.FlowSubType != FlowSubType)
                    ret = -1;
                else if ((flow.Ida_CssSpecified || this.Ida_CssSpecified) &&
                    (flow.Ida_Css != Ida_Css))
                {
                    ret = -1;
                }
                else if (flow.Status != Status) // FI 20170208 [22151][22152] add
                    ret = -1;

                return ret;
            }

            throw new ArgumentException("object is not a CBFlow");
        }
        /// <summary>
        /// Ajoute un montant (couple: nombre, Devise) s'il n'existe pas dans la liste
        /// <para>En phase de chargement de la BD: Cumule le montant s'il existe un montant de la même devise</para>
        /// </summary>
        /// <param name="pCurrencyAmount"></param>
        public void Add(Money pCurrencyAmount)
        {
            Money money = this.CurrencyAmount.Find(match => match.Currency == pCurrencyAmount.Currency);
            //
            if (null == money)
                this.CurrencyAmount.Add(pCurrencyAmount);
            else if (this.IsLoaded == false)
                money.Amount.DecValue += pCurrencyAmount.Amount.DecValue;
        }
        #endregion
    }

    /// <summary>
    /// Classe portant la liste des Trades pour un type de flux
    /// </summary>
    /// FI 20170208 [22151][22152] Modify
    [XmlRoot(ElementName = "FlowTrades")]
    public class CBFlowTradesSource
    {
        #region members
        private int m_IDB;
        private int m_IDA;

        // FI 20170208 [22151][22152] est de type CBTrade
        //private List<Pair<int, string>> m_Trades;
        private List<CBTrade>  m_Trades;
        private List<FlowTypeEnum> m_FlowTypes;
        #endregion
        #region accessors
        /// <summary>
        /// Book apparaissant sur le trade
        /// </summary>
        [XmlAttribute(AttributeName = "idb")]
        public int IDB
        {
            get { return m_IDB; }
            set { m_IDB = value; }
        }
        /// <summary>
        /// C'est selon le cas:
        /// <para>- exchangeTradedDerivative: C'est le propriétaire du Book</para>
        /// <para>- marginRequirement: C'est le MRO</para>
        /// <para>- bulletPayment: C'est le MRO/CBO qui paye/reçoit le montant du mouvement éspèces</para>
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int IDA
        {
            get { return m_IDA; }
            set { m_IDA = value; }
        }
        /// <summary>
        /// La liste des trades 
        /// </summary>
        /// FI 20170208 [22151][22152] trades est de type List de CBTrade
        //[XmlArray(ElementName = "trades")]
        //[XmlArrayItemAttribute("idt")]
        //public List<Pair<int, string>> Trades
        //{
        //    get { return m_Trades; }
        //    set { m_Trades = value; }
        //}
        [XmlArray(ElementName = "trades")]
        [XmlArrayItemAttribute("trade")]
        public List<CBTrade> Trades
        {
            get { return m_Trades; }
            set { m_Trades = value; }
        }

        /// <summary>
        /// Les flux concernés
        /// </summary>
        [XmlArray(ElementName = "types")]
        [XmlArrayItemAttribute("type")]
        public List<FlowTypeEnum> FlowTypes
        {
            get { return m_FlowTypes; }
            set { m_FlowTypes = value; }
        }
        #endregion
        #region constructors
        public CBFlowTradesSource() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pFlowTypes"></param>
        /// FI 20170208 [22151][22152] Mofiy
        public CBFlowTradesSource(int pIdb, int pIda, List<FlowTypeEnum> pFlowTypes)
        {
            m_IDB = pIdb;
            m_IDA = pIda;
            m_FlowTypes = pFlowTypes;
            // FI 20170208 [22151][22152]
            //m_Trades = new List<Pair<int, string>>();
            m_Trades = new List<CBTrade>(); 
        }

        /// <summary>
        ///  Retourne les trades avec status = {pStatus}
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        /// FI 20170208 [22151][22152] Add
        public IEnumerable<CBTrade> GetTrades(StatusEnum pStatus)
        {
            return this.Trades.Where(x => x.Status == pStatus);
        }

        #endregion
    }

    /// <summary>
    /// Classe portant les détails de chaque flux de Cash-Flows.
    /// </summary>    
    /// FI 20120724 [18009] Add m_idDC
    /// PM 20140903 [20066][20185] Gestion méthode UK (ETD & CFD) Add m_Category, m_FutValuationMethod, m_IdAsset, m_AssetCategory
    /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
    [XmlRoot(ElementName = "CashFlowDetail")]
    public class CBDetCashFlows : CBDetFlow
    {
        #region private members
        private string m_PaymentType = string.Empty;
        private int m_IdTax = 0;
        private int m_IdTaxDet = 0;
        private string m_TaxCountry = string.Empty;
        private string m_TaxType = string.Empty;
        private decimal m_TaxRate = 0;
        private int m_IdDC = 0;
        private CfiCodeCategoryEnum? m_Category = default;
        private FuturesValuationMethodEnum? m_FutValuationMethod = default;
        private int m_IdAsset = 0;
        //PM 20150324 [POC] m_AssetCategory devient nullable
        private Cst.UnderlyingAsset? m_AssetCategory;
        private SideEnum? m_Side = default;
        #endregion private members
        #region public accessors
        /// <summary>
        /// PaymentType des Frais
        /// </summary>
        [XmlAttribute(AttributeName = "paymentType")]
        public string PaymentType
        {
            get { return m_PaymentType; }
            set { m_PaymentType = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool PaymentTypeSpecified;
        /// <summary>
        /// IdTax source pour un montant de tax
        /// </summary>
        [XmlAttribute(AttributeName = "idTax")]
        public int IdTax
        {
            get { return m_IdTax; }
            set { m_IdTax = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool IdTaxSpecified;
        /// <summary>
        /// IdTaxDet source pour un montant de tax
        /// </summary>
        [XmlAttribute(AttributeName = "idTaxDet")]
        public int IdTaxDet
        {
            get { return m_IdTaxDet; }
            set { m_IdTaxDet = value; }
        }
        /// <summary>
        /// Country source pour un montant de tax
        /// </summary>
        [XmlAttribute(AttributeName = "taxCountry")]
        public string TaxCountry
        {
            get { return m_TaxCountry; }
            set { m_TaxCountry = value; }
        }
        /// <summary>
        /// Type source pour un montant de tax
        /// </summary>
        [XmlAttribute(AttributeName = "taxType")]
        public string TaxType
        {
            get { return m_TaxType; }
            set { m_TaxType = value; }
        }
        /// <summary>
        /// Taux source pour un montant de tax
        /// </summary>
        [XmlAttribute(AttributeName = "taxRate")]
        public decimal TaxRate
        {
            get { return m_TaxRate; }
            set { m_TaxRate = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool IdDCSpecified
        {
            get { return (m_IdDC > 0); }
        }
        /// <summary>
        /// IdDC montant par Derivative Contrat
        /// </summary>
        [XmlAttribute(AttributeName = "idDC")]
        public int IdDC
        {
            get { return m_IdDC; }
            set { m_IdDC = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool CategorySpecified
        {
            get { return m_Category.HasValue; }
        }
        /// <summary>
        /// Categorie de DC
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public CfiCodeCategoryEnum? Category
        {
            get { return m_Category; }
            set { m_Category = value; }
        }
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlAttribute(AttributeName = "category")]
        public string CategoryToXML
        {
            get { return m_Category.HasValue?m_Category.Value.ToString():null; }
            set { m_Category = ReflectionTools.ConvertStringToEnumOrNullable<CfiCodeCategoryEnum>(value); }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool FutValuationMethodSpecified
        {
            get { return m_FutValuationMethod.HasValue; }
        }
        /// <summary>
        /// Méthode de valorisation d'un DC
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public FuturesValuationMethodEnum? FutValuationMethod
        {
            get { return m_FutValuationMethod; }
            set { m_FutValuationMethod = value; }
        }
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlAttribute(AttributeName = "futValuationMethod")]
        public string FutValuationMethodXML
        {
            get { return m_FutValuationMethod.HasValue ? m_FutValuationMethod.Value.ToString() : null; }
            set { m_FutValuationMethod = ReflectionTools.ConvertStringToEnumOrNullable<FuturesValuationMethodEnum>(value); }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool IdAssetSpecified
        {
            get { return (IdAsset > 0); }
        }
        /// <summary>
        /// IdAsset pour montant par Asset
        /// </summary>
        [XmlAttribute(AttributeName = "idAsset")]
        public int IdAsset
        {
            get { return m_IdAsset; }
            set { m_IdAsset = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool SideSpecified
        {
            get { return m_Side.HasValue; }
        }
        /// <summary>
        /// Sens des trades sur lesquels porte le flux
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        public SideEnum? Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlAttribute(AttributeName = "side")]
        public string SideXML
        {
            get { return m_Side.HasValue ? m_Side.Value.ToString() : null; }
            set { m_Side = ReflectionTools.ConvertStringToEnumOrNullable<SideEnum>(value); }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        //PM 20150324 [POC] Add AssetCategorySpecified
        public bool AssetCategorySpecified
        {
            get { return m_AssetCategory.HasValue; }
        }
        /// <summary>
        /// Categorie d'Asset
        /// </summary>
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnore]
        //PM 20150324 [POC] AssetCategory devient nullable
        public Cst.UnderlyingAsset? AssetCategory
        {
            get { return m_AssetCategory; }
            set { m_AssetCategory = value; }
        }
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlAttribute(AttributeName = "assetCategory")]
        public string AssetCategoryXML
        {
            get { return m_AssetCategory.HasValue ? m_AssetCategory.Value.ToString() : null; }
            set { m_AssetCategory = ReflectionTools.ConvertStringToEnumOrNullable<Cst.UnderlyingAsset>(value); }
        }
        #endregion public accessors
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        public CBDetCashFlows() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pAmountSubType"></param>
        /// <param name="pDtValue"></param>
        /// <param name="pCategory"></param>
        /// <param name="pFutValuationMethod"></param>
        /// <param name="pAssetCategory"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pSide"></param>
        /// PM 20150324 [POC] pAssetCategory devient nullable
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetCashFlows(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowSubTypeEnum pAmountSubType, DateTime pDtValue,
            Cst.UnderlyingAsset? pAssetCategory, CfiCodeCategoryEnum? pCategory, FuturesValuationMethodEnum? pFutValuationMethod,
            int pIdDC, int pIdAsset, SideEnum? pSide, StatusEnum pStatus )
            : this(pIdb, pIda, pAmount, pCurrency, pAmountSubType, pDtValue, string.Empty, 0, 0, string.Empty, string.Empty, 0, pAssetCategory, pCategory, pFutValuationMethod, pIdDC, pIdAsset, pSide, pStatus) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pAmountSubType"></param>
        /// <param name="pDtValue"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pIdTax"></param>
        /// <param name="pIdTaxDet"></param>
        /// <param name="pTaxCountry"></param>
        /// <param name="pTaxType"></param>
        /// <param name="pTaxRate"></param>
        /// <param name="pAssetCategory"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pIdAsset"></param>
        /// PM 20150324 [POC] pAssetCategory devient nullable
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetCashFlows(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowSubTypeEnum pAmountSubType, DateTime pDtValue,
            string pPaymentType, int pIdTax, int pIdTaxDet, string pTaxCountry, string pTaxType, decimal pTaxRate,
            Cst.UnderlyingAsset? pAssetCategory, int pIdDC, int pIdAsset, StatusEnum pStatus)
            : this(pIdb, pIda, pAmount, pCurrency, pAmountSubType, pDtValue, pPaymentType, pIdTax, pIdTaxDet, pTaxCountry, pTaxType, pTaxRate, pAssetCategory, default, default, pIdDC, pIdAsset, default, pStatus)
        {
        }
        
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pAmountSubType"></param>
        /// <param name="pDtValue"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pIdTax"></param>
        /// <param name="pIdTaxDet"></param>
        /// <param name="pTaxCountry"></param>
        /// <param name="pTaxType"></param>
        /// <param name="pTaxRate"></param>
        /// <param name="pAssetCategory"></param>
        /// <param name="pCategory"></param>
        /// <param name="pFutValuationMethod"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pSide"></param>
        /// <param name="pStatus"></param>
        /// PM 20150324 [POC] pAssetCategory devient nullable
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetCashFlows(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowSubTypeEnum pAmountSubType, DateTime pDtValue,
            string pPaymentType, int pIdTax, int pIdTaxDet, string pTaxCountry, string pTaxType, decimal pTaxRate,
            Cst.UnderlyingAsset? pAssetCategory, CfiCodeCategoryEnum? pCategory, FuturesValuationMethodEnum? pFutValuationMethod,
            int pIdDC, int pIdAsset, SideEnum? pSide, StatusEnum pStatus )
            : base(pIdb, pIda, pAmount, pCurrency, FlowTypeEnum.CashFlows, pAmountSubType, pDtValue, pStatus)
        {
            m_PaymentType = pPaymentType;
            PaymentTypeSpecified = StrFunc.IsFilled(m_PaymentType);
            //
            m_IdTax = pIdTax;
            IdTaxSpecified = (m_IdTax > 0);
            //
            m_IdTaxDet = pIdTaxDet;
            m_TaxCountry = pTaxCountry;
            m_TaxType = pTaxType;
            m_TaxRate = pTaxRate;

            m_IdDC = pIdDC;
            //
            m_AssetCategory = pAssetCategory;
            m_Category = pCategory;
            m_FutValuationMethod = pFutValuationMethod;
            m_IdAsset = pIdAsset;
            m_Side = pSide;
        }
        #endregion
        #region methods
        /// <summary>
        /// Comparer le CBAmount courant à {pObj}, selon les critères suivants et dans l'ordre:
        /// <para>IDA</para>
        /// <para>IDB</para>
        /// <para>AmountType</para>
        /// <para>AmountSubType, s'il existe sur l'un des deux</para>
        /// </summary>
        /// <param name="pObj">L'objet à comparer avec le CBAmount courant</param>
        /// <returns>
        /// <para> 0 : égalité</para>
        /// <para>-1 : différence</para>
        /// </returns>
        /// FI 20120724 [18009] gestion de m_idDC 
        /// PM 20140903 [20066][20185] Gestion AssetCategory, Category, FutValuationMethod, IdAsset, Side
        public override int CompareTo(object pObj)
        {
            int ret = base.CompareTo(pObj);
            //
            if (ret != -1)
            {
                if (pObj is CBDetCashFlows amount)
                {
                    if ((amount.PaymentTypeSpecified || this.PaymentTypeSpecified) &&
                        (amount.PaymentType != PaymentType))
                    {
                        ret = -1;
                    }
                    else if ((amount.IdTaxSpecified || this.IdTaxSpecified) &&
                        ((amount.IdTax != IdTax) ||
                        (amount.IdTaxDet != IdTaxDet) ||
                        (amount.TaxCountry != TaxCountry) ||
                        (amount.TaxType != TaxType) ||
                        (amount.TaxRate != TaxRate)))
                    {
                        ret = -1;
                    }
                    //PM 20150324 [POC] AssetCategory devient nullable
                    //else if (amount.AssetCategory != AssetCategory))
                    else if ((amount.AssetCategorySpecified || this.AssetCategorySpecified) && (amount.AssetCategory != AssetCategory))
                    {
                        ret = -1;
                    }
                    else if ((amount.CategorySpecified || this.CategorySpecified) &&
                        (amount.Category != Category))
                    {
                        ret = -1;
                    }
                    else if ((amount.FutValuationMethodSpecified || this.FutValuationMethodSpecified) &&
                        (amount.FutValuationMethod != FutValuationMethod))
                    {
                        ret = -1;
                    }
                    else if ((amount.IdDCSpecified || this.IdDCSpecified) &&
                        (amount.IdDC != IdDC))
                    {
                        ret = -1;
                    }
                    else if ((amount.IdAssetSpecified || this.IdAssetSpecified) &&
                        (amount.IdAsset != IdAsset))
                    {
                        ret = -1;
                    }
                    else if ((amount.SideSpecified || this.SideSpecified) &&
                        (amount.Side != Side))
                    {
                        ret = -1;
                    }
                }
                else
                {
                    ret = -1;
                }
            }
            //
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// Classe portant les détails de chaque Payment en date STL.
    /// </summary>
    [XmlRoot(ElementName = "StlPayment")]
    public class CBDetStlPayment : CBDetFlow
    {
        #region private members
        private bool m_IsForward = false;
        private readonly bool m_IsDeposit = true;
        private string m_PaymentType = null;
        #endregion private members
        #region public accessors
        /// <summary>
        /// Is Forward Payment
        /// </summary>
        [XmlAttribute(AttributeName = "isForward")]
        public bool IsForward
        {
            get { return m_IsForward; }
            set { m_IsForward = value; }
        }
        /// <summary>
        /// Is Deposit Payment
        /// </summary>
        [XmlAttribute(AttributeName = "isDeposit")]
        public bool IsDeposit
        {
            get { return m_IsDeposit; }
        }
        /// <summary>
        /// Is Withdrawal Payment
        /// </summary>
        [XmlAttribute(AttributeName = "isWithdrawal")]
        public bool IsWithdrawal
        {
            get { return !IsDeposit; }
        }
        /// <summary>
        /// PaymentType
        /// </summary>
        [XmlAttribute(AttributeName = "paymentType")]
        public string PaymentType
        {
            get { return m_PaymentType; }
            set { m_PaymentType = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool PaymentTypeSpecified;
        #endregion public accessors
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        public CBDetStlPayment() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pAmountSubType"></param>
        /// <param name="pDtValue"></param>
        /// <param name="pIsForward"></param>
        /// <param name="pStatus"></param>
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetStlPayment(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowSubTypeEnum pAmountSubType, DateTime pDtValue, bool pIsForward, StatusEnum pStatus)
            : this(pIdb, pIda, pAmount, pCurrency, pAmountSubType, pDtValue, pIsForward, null, pStatus) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pAmountSubType"></param>
        /// <param name="pDtValue"></param>
        /// <param name="pIsForward"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pStatus"></param>
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetStlPayment(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowSubTypeEnum pAmountSubType, DateTime pDtValue,
            bool pIsForward, string pPaymentType, StatusEnum pStatus)
            : base(pIdb, pIda, pAmount, pCurrency, FlowTypeEnum.SettlementPayment, pAmountSubType, pDtValue, pStatus)
        {
            m_IsForward = pIsForward;
            m_IsDeposit = (pAmount < 0);
            m_PaymentType = pPaymentType;
            PaymentTypeSpecified = StrFunc.IsFilled(m_PaymentType);
        }
        #endregion
        #region methods
        /// <summary>
        /// Comparer le CBAmount courant à {pObj}, selon les critères suivants et dans l'ordre:
        /// <para>IDA</para>
        /// <para>IDB</para>
        /// <para>AmountType</para>
        /// <para>AmountSubType, s'il existe sur l'un des deux</para>
        /// </summary>
        /// <param name="pObj">L'objet à comparer avec le CBAmount courant</param>
        /// <returns>
        /// <para> 0 : égalité</para>
        /// <para>-1 : différence</para>
        /// </returns>
        public override int CompareTo(object pObj)
        {
            int ret = base.CompareTo(pObj);
            //
            if (ret != -1)
            {
                if (pObj is CBDetStlPayment amount)
                {
                    if ((amount.PaymentTypeSpecified || this.PaymentTypeSpecified) &&
                        (amount.PaymentType != PaymentType))
                    {
                        ret = -1;
                    }
                    else if (amount.IsForward != IsForward)
                    {
                        ret = -1;
                    }
                    else if (amount.IsDeposit != IsDeposit)
                    {
                        ret = -1;
                    }
                }
                else
                {
                    ret = -1;
                }
            }
            return ret;
        }
        #endregion
    }

    /// <summary>
    /// Classe portant les détails de chaque flux de déposit.
    /// </summary>    
    [XmlRoot(ElementName = "Deposit")]
    public class CBDetDeposit : CBDetFlow
    {
        /// <summary>
        /// Date systeme d'insertion du Déposit
        /// </summary>
        [XmlAttribute(AttributeName = "dtSysDeposit")]
        // PM 20150616 [21124] utilisation du membre m_DtValue à la place de DtSysDeposit
        //public DateTime DtSysDeposit;
        public DateTime DtSysDeposit
        {
            get { return m_DtValue; }
            set { m_DtValue = value; }
        }

        /// <summary>
        /// Les Contrevaleurs de tous les Deposits qui ont une devise d'origine différente de la devise de contrevaleur du CBO (CBO.EXCHAGEIDC)
        /// <para>- La Contrevaleur est calculée dans la devise de contrevaleur du CBO</para>
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlArray(ElementName = "exchangeMarginRequierements")]
        [XmlArrayItemAttribute("marginRequierement")]
        public List<CBExAmount> Deposit_ExCTRValCur;
        /// <summary>
        /// La somme de tous les Déposits confondus dans la devise de contrevaleur du CBO
        /// <para>- Donc les Déposits déjà en devise de contrevaleur du CBO,</para>
        /// <para>- Plus la contrevaleur des Deposits qui ont une devise d'origine différente de la devise de contrevaleur du CBO</para>
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("marginRequierement")]
        public Money Deposit_MGCCTRVal;

        /// <summary>
        /// Tous les Deposits restant à couvrir en devise d'origine
        /// <para>- Uniquement si le type de contrevaleur du CBO est égale à FlowCurrency</para>
        /// </summary>
        [XmlArray(ElementName = "defectMarginRequierements")]
        [XmlArrayItemAttribute("defectMarginRequierement")]
        public List<Money> DefectDeposits_FlowCur;

        /// <summary>
        /// Deposits restant à couvrir en devise de contrevaleur du CBO
        /// <para>- Uniquement si la Méthode de calcul des Appels/Restitutions est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("defectMarginRequierement")]
        public Money DefectDeposits_MGCCTRVal;

        /// <summary>
        /// Collaterals autorisés par les instructions, et triés :
        ///      - D'abord en fonction des Instructions de tri (référentiel COLLATERALPRIORITY)
        ///      - Ensuite, en fonction du Poids de l'Instruction qui Autorise le Collateral
        /// </summary>
        [XmlArray(ElementName = "css_Collaterals")]
        [XmlArrayItemAttribute("css_Collateral")]
        public List<CBDetCollateral> CSS_Collaterals;

        public CBDetDeposit() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDtSysDeposit"></param>
        /// <param name="pIda_Css"></param>
        /// <param name="pStatus"></param>
        /// FI 20170208 [22151][22152] add pStatus
        public CBDetDeposit(int pIdb, int pIda, decimal pAmount, string pCurrency, DateTime pDtSysDeposit, int pIda_Css, StatusEnum pStatus)
            : base(pIdb, pIda, pAmount, pCurrency, FlowTypeEnum.Deposit, FlowSubTypeEnum.None, pDtSysDeposit, pStatus)
        {
            // PM 20150616 [21124] DtBusinessCollateral déjà initialisé dans la class de base grace à DtValue
            //DtSysDeposit = pDtSysDeposit;
            Ida_Css = pIda_Css;
            Ida_CssSpecified = (Ida_Css > 0);

            DefectDeposits_FlowCur = new List<Money>();

            Deposit_ExCTRValCur = null;
            Deposit_MGCCTRVal = null;
            DefectDeposits_MGCCTRVal = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CollatAmount_MGCCTRVal
    {
        /// <summary>
        /// Les Contrevaleurs de tout les montants brut (sans abattement) des Collaterals disponibles,
        /// qui ont une devise d'origine différente de la devise de contrevaleur
        /// </summary>        
        [XmlArray(ElementName = "exchangeColateralsAvailable")]
        [XmlArrayItemAttribute("colateralAvailable")]
        public List<CBExAmount> AvailableGross_Ex;

        /// <summary>
        /// La somme de tous les montants net (après abattement) des Collaterals disponibles confondus,
        /// dans la devise de contrevaleur
        /// <para>- Donc les Collaterals disponibles déjà en devise de contrevaleur,</para>
        /// <para>- Plus la contrevaleur des Collaterals disponibles qui ont une devise d'origine différente de la devise de contrevaleur</para>
        /// </summary>
        [XmlElementAttribute("colateralAvailable")]
        public Money Available;

        /// <summary>
        /// La somme de tous les montants brut (sans abattement) des Collaterals disponibles confondus,
        /// dans la devise de contrevaleur
        /// <para>- Donc les Collaterals disponibles déjà en devise de contrevaleur</para>
        /// <para>- Plus la contrevaleur des Collaterals disponibles qui ont une devise d'origine différente de la devise de contrevaleur</para>
        /// </summary>
        [XmlElementAttribute("colateralAvailableGross")]
        public Money AvailableGross;

        /// <summary>
        /// Collaterals net (après abattement) en devise de contrevaleur, utilisées pour la couverture du déposit
        /// </summary>
        [XmlElementAttribute("colateralUsed")]
        public Money Used;

        /// <summary>
        /// Collaterals brut (sans abattement) en devise de contrevaleur, utilisées pour la couverture du déposit
        /// </summary>
        [XmlElementAttribute("colateralUsedGross")]
        public Money UsedGross;

        public CollatAmount_MGCCTRVal(string pCurrency)
        {
            AvailableGross_Ex = new List<CBExAmount>();
            Available = new Money(0, pCurrency);
            AvailableGross = new Money(0, pCurrency);
            Used = new Money(0, pCurrency);
            UsedGross = new Money(0, pCurrency);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CollatAmount_MGCCollatCTRVal
    {
        /// <summary>
        /// Les Contrevaleurs de tout les montants brut (sans abattement) des Collaterals disponibles,
        /// qui ont une devise d'origine différente de la devise de contrevaleur
        /// </summary>        
        [XmlArray(ElementName = "exchangeColateralsAvailable")]
        [XmlArrayItemAttribute("colateralAvailable")]
        public List<CBExAmount> AvailableGross_Ex;

        /// <summary>
        /// La somme de tous les montants net (après abattement) des Collaterals disponibles confondus,
        /// dans la devise de contrevaleur
        /// <para>- Donc les Collaterals disponibles déjà en devise de contrevaleur,</para>
        /// <para>- Plus la contrevaleur des Collaterals disponibles qui ont une devise d'origine différente de la devise de contrevaleur</para>
        /// </summary>
        [XmlElementAttribute("colateralAvailable")]
        public List<Money> Available;

        /// <summary>
        /// La somme de tous les montants brut (sans abattement) des Collaterals disponibles confondus,
        /// dans la devise de contrevaleur
        /// <para>- Donc les Collaterals disponibles déjà en devise de contrevaleur</para>
        /// <para>- Plus la contrevaleur des Collaterals disponibles qui ont une devise d'origine différente de la devise de contrevaleur</para>
        /// </summary>
        [XmlElementAttribute("colateralAvailableGross")]
        public List<Money> AvailableGross;

        /// <summary>
        /// Constituent des collatéraux disponibles
        /// </summary>
        [XmlArray(ElementName = "collateralConstituents")]
        [XmlArrayItemAttribute("collateralConstituent")]
        public List<CBCollateralConstituent> AvailableConstituent;

        /// <summary>
        /// Collaterals net (après abattement) en devise de contrevaleur, utilisées pour la couverture du déposit
        /// </summary>
        [XmlElementAttribute("colateralUsed")]
        public List<Money> Used;

        /// <summary>
        /// Collaterals brut (sans abattement) en devise de contrevaleur, utilisées pour la couverture du déposit
        /// </summary>
        [XmlElementAttribute("colateralUsedGross")]
        public List<Money> UsedGross;

        public CollatAmount_MGCCollatCTRVal(string pCurrency)
        {
            AvailableGross_Ex = new List<CBExAmount>();
            Available = new List<Money>();
            AvailableGross = new List<Money>();
            AvailableConstituent = new List<CBCollateralConstituent>();
            Used = new List<Money>();
            UsedGross = new List<Money>();
        }
    }

    /// <summary>
    /// Classe portant les détails de chaque flux Collateral.
    /// </summary> 
    /// FI 20160530 [21885] Modify
    [XmlRoot(ElementName = "Collateral")]
    public class CBDetCollateral : CBDetFlow
    {
        /// <summary>
        /// Date compensation (Date Valeur) d'un Collateral
        /// </summary>
        [XmlAttribute(AttributeName = "dtBusinessCollateral")]
        // PM 20150616 [21124] utilisation du membre m_DtValue à la place de DtBusinessCollateral
        //public DateTime DtBusinessCollateral;
        public DateTime DtBusinessCollateral
        {
            get { return m_DtValue; }
            set { m_DtValue = value; }
        }
        /// <summary>
        /// Indique si un Collateral est valorisé ou non
        /// </summary>
        [XmlAttribute(AttributeName = "isCollatValorised")]
        public bool IsCollatValorised;

        /// <summary>
        /// La Catégorie d'un Collatéral
        /// </summary>
        [XmlAttribute(AttributeName = "collateralCategory")]
        public CollateralCategoryEnum CollateralCategory;

        /// <summary>
        /// Si AssetCategory est spécifié, alors il s'agit bien d'un Montant de dépôt de garantie
        /// </summary>
        [XmlIgnore]
        public bool AssetCategorySpecified;

        /// <summary>
        /// La Catégorie d'actif d'un Collatéral
        /// </summary>
        [XmlAttribute(AttributeName = "assetCategory")]
        public string AssetCategory;

        /// <summary>
        /// L'Actif d'un Collatéral
        /// </summary>
        [XmlAttribute(AttributeName = "idAsset")]
        public int IdAsset;

        /// <summary>
        /// L'Abattement forcé d'un Collateral, saisi dans le référentiel POSCOLLATERAL
        /// </summary>
        /// PM 20150901 HaircutForced devient nullable
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlAttribute(AttributeName = "haircutForced")]
        public decimal HaircutForcedXML
        {
            get { return HaircutForced ?? 0; }
        }
        [XmlIgnore()]
        public decimal? HaircutForced;

        /// <summary>
        /// L'ordre de priorité d'un Collatéral
        /// </summary>
        [XmlAttribute(AttributeName = "collateralPriority")]
        public int CollateralPriority;

        /// <summary>
        /// L'Abattement le plus défavorable d'un Collateral
        /// </summary>
        [XmlAttribute(AttributeName = "haircutWorst")]
        public decimal HaircutWorst
        {
            get
            {
                decimal haircut = 0;
                //
                // PM 20150901 HaircutForced devient nullable
                //if (HaircutForced > 0)
                //    haircut = HaircutForced;
                if (HaircutForced.HasValue)
                {
                    haircut = HaircutForced.Value;
                }
                else
                {
                    if (ArrFunc.IsFilled(collateralEnv))
                    {
                        haircut =
                            (from instruction in collateralEnv
                             where instruction.IsCollateralAllowed
                             select instruction.HaircutValue()
                            ).Max();
                    }
                }
                //
                haircut = System.Math.Max(0, haircut);
                haircut = System.Math.Min(100, haircut);
                //
                return haircut;
            }
        }

        /// <summary>
        /// Indique si un Collateral est autorisé pour au moins une Chambre, 
        /// selon le paramétrage du référentiel COLLATERALENV
        /// </summary>
        [XmlAttribute(AttributeName = "isCollatAllowed_AtLeastOneCSS")]
        public bool IsAllowed_AtLeastOneCSS
        {
            get
            {
                bool isCollatAllowed = false;
                //
                if (ArrFunc.IsFilled(collateralEnv))
                    isCollatAllowed = (null != collateralEnv.Find(instruction => instruction.IsCollateralAllowed));
                else if (CollateralCategory == CollateralCategoryEnum.AvailableCash)
                    isCollatAllowed = true;
                //
                return isCollatAllowed;
            }
        }
        /// <summary>
        /// Indique si un Collateral est autorisé ou non, selon le paramétrage du référentiel COLLATERALENV
        /// selon les instructions de la Chambre en cours.
        /// </summary>
        [XmlAttribute(AttributeName = "isCollatAllowed_CurrentCSS")]
        public bool IsAllowed_CurrentCSS
        {
            get
            {
                bool isCollatAllowed = false;
                //
                if (Instruction_CurrentCSS != null)
                    isCollatAllowed = Instruction_CurrentCSS.IsCollateralAllowed;
                else if (CollateralCategory == CollateralCategoryEnum.AvailableCash)
                    isCollatAllowed = true;
                //
                return isCollatAllowed;
            }
        }

        /// <summary>
        /// Indique le poids (priorité) du Collateral, selon l'instruction qui l'autorise pour la Chambre en cours.
        /// </summary>
        [XmlAttribute(AttributeName = "weight_CurrentCSS")]
        public int Weight_CurrentCSS
        {
            get
            {
                int weight = 0;
                //
                if (Instruction_CurrentCSS != null)
                    weight = Instruction_CurrentCSS.Weight;
                //
                return weight;
            }
        }

        /// <summary>
        /// Le pourcentage de l'Abattement d'un Collateral:
        /// <para>Considérer l'Abattement forcé, s'il est spécifié.</para>
        /// <para>Sinon considérer l'Abattement initial.</para>
        /// <para>Si l'abattement considéré est &lt; 0, alors prendre 0</para>
        /// <para>Si l'abattement considéré est &gt; 100, alors prendre 100</para>
        /// </summary>
        [XmlAttribute(AttributeName = "haircut")]
        public decimal Haircut_CurrentCSS
        {
            get
            {
                decimal haircut = 0;
                //
                // PM 20150901 HaircutForced devient nullable
                //if (HaircutForced > 0)
                //    haircut = HaircutForced;
                if (HaircutForced.HasValue)
                {
                    haircut = HaircutForced.Value;
                }
                else
                {
                    if (Instruction_CurrentCSS != null)
                        haircut = Instruction_CurrentCSS.HaircutValue();
                }
                //
                haircut = System.Math.Max(0, haircut);
                haircut = System.Math.Min(100, haircut);
                //
                return haircut;
            }
        }

        /// <summary>
        ///<para>METHODE DEFAULT => Collaterals gross (avant abattement) disponibles en devise d'origine</para>
        ///<para>METHODE UK => Collaterals net (après abattement) disponibles en devise d'origine</para>
        /// </summary>
        [XmlArray(ElementName = "colateralsAvailable")]
        [XmlArrayItemAttribute("colateralAvailable")]
        public List<Money> CollatAvailable_FlowCur;

        /// <summary>
        /// Collaterals net (après abattement) en devise d'origine, utilisées pour la couverture du déposit
        /// </summary>
        [XmlArray(ElementName = "colateralsUsed")]
        [XmlArrayItemAttribute("colateralUsed")]
        public List<Money> CollatUsed_FlowCur;
        /// <summary>
        /// Collaterals brut (sans abattement) en devise d'origine, utilisées pour la couverture du déposit
        /// </summary>
        [XmlArray(ElementName = "colateralsUsedGross")]
        [XmlArrayItemAttribute("colateralUsedGross")]
        public List<Money> CollatUsedGross_FlowCur;

        /// <summary>
        /// Les différents montants intermédiaires
        /// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("mgcCollatCur")]
        public CollatAmount_MGCCTRVal Amount_MGCCTRVal;

        /// <summary>
        /// Les différents montants intermédiaires
        /// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCOLLATCTRVAL</para>
        /// </summary>
        [XmlElementAttribute("mgcCollatCTRVal")]
        public CollatAmount_MGCCollatCTRVal Amount_MGCCollatCTRVal;

        ///// <summary>
        ///// Les Contrevaleurs de tout les montants brut (sans abattement) des Collaterals disponibles,
        ///// qui ont une devise d'origine différente de la devise de contrevaleur du CBO (CBO.EXCHAGEIDC)
        ///// <para>- La Contrevaleur est calculée dans la devise de contrevaleur du CBO</para>
        ///// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCTRVAL</para>
        ///// </summary>        
        //[XmlArray(ElementName = "exchangeColateralsAvailable")]
        //[XmlArrayItemAttribute("colateralAvailable")]
        //public List<CBExAmount> CollatAvailableGross_ExMGCCTRVAL;
        ///// <summary>
        ///// La somme de tous les montants net (après abattement) des Collaterals disponibles confondus,
        ///// dans la devise de contrevaleur du CBO
        ///// <para>- Donc les Collaterals disponibles déjà en devise de contrevaleur du CBO,</para>
        ///// <para>- Plus la contrevaleur des Collaterals disponibles 
        ///// qui ont une devise d'origine différente de la devise de contrevaleur du CBO</para>
        ///// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCTRVAL</para>
        ///// </summary>
        //[XmlElementAttribute("colateralAvailable")]
        //public Money CollatAvailable_MGCCTRVAL;
        ///// <summary>
        ///// La somme de tous les montants brut (sans abattement) des Collaterals disponibles confondus,
        ///// dans la devise de contrevaleur du CBO
        ///// <para>- Donc les Collaterals disponibles déjà en devise de contrevaleur du CBO,</para>
        ///// <para>- Plus la contrevaleur des Collaterals disponibles 
        ///// qui ont une devise d'origine différente de la devise de contrevaleur du CBO</para>
        ///// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCTRVAL</para>
        ///// </summary>
        //[XmlElementAttribute("colateralAvailableGross")]
        //public Money CollatAvailableGross_MGCCTRVAL;
        ///// <summary>
        ///// Collaterals net (après abattement) en devise de contrevaleur du CBO, utilisées pour la couverture du déposit
        ///// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCTRVAL</para>
        ///// </summary>
        //[XmlElementAttribute("colateralUsed")]
        //public Money CollatUsed_MGCCTRVAL;
        ///// <summary>
        ///// Collaterals brut (sans abattement) en devise de contrevaleur du CBO, utilisées pour la couverture du déposit
        ///// <para>- Uniquement si la méthode de calcul des Appel/Rest. du CBO est MGCCTRVAL</para>
        ///// </summary>
        //[XmlElementAttribute("colateralUsedGross")]
        //public Money CollatUsedGross_MGCCTRVAL;

        /// <summary>
        /// Obtient ou définit les instructions  qui matchent avec le collateral (garantie/couverture)
        /// <para>Si la couverture s'applique à 1 chambre uniquement => 1 seule instruction</para>
        /// <para>Sinon 1 instruction par chambre de compensation + 1 instruction toute chambre (lorsqu'il existe une instruction sans chambre)</para>
        /// </summary>
        [XmlArray(ElementName = "collateralEnv")]
        [XmlArrayItemAttribute("collateralEnv")]
        public List<CBCollateralEnv> collateralEnv;

        /// <summary>
        /// Obtient ou Défnit l'instruction retenue vis à vis de la chambre courante
        /// </summary>
        [XmlElementAttribute("collateralEnvMaster")]
        public CBCollateralEnv Instruction_CurrentCSS;

        /// <summary>
        ///   Obtient ou Définit la quantité de titre 
        /// </summary>
        /// FI 20160530 [21885] Add
        /// EG 20170127 Qty Long To Decimal
        [XmlElementAttribute("quantity")]
        public decimal? Qty;

        /// <summary>
        /// Id non significatif du dépôt de garantie
        /// </summary>
        /// FI 20160530 [21885] Add
        [XmlElementAttribute("idPoscollateral")]
        public int idPoscollateral;

        /// <summary>
        /// Id non significatif de la valo du dépôt de garantie
        /// </summary>
        /// FI 20160530 [21885] Add
        [XmlElementAttribute("idPoscollateralVal")]
        public int idPoscollateralVal;



        /// <summary>
        /// Nouvelle instace d'un collatera
        /// </summary>
        public CBDetCollateral() : base() { }
        /// <summary>
        ///  Nouvelle instace d'un collateral de type AvailableCash (Espèces disponibles)
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pStatus"></param>
        /// FI 20170208 [22151][22152] Add pStatus
        public CBDetCollateral(int pIda, StatusEnum pStatus)
            : base(0, pIda, 0, string.Empty, FlowTypeEnum.None, FlowSubTypeEnum.None, DateTime.MinValue, pStatus)
        {
            CollateralCategory = CollateralCategoryEnum.AvailableCash;
            IsCollatValorised = true;

            InitializeAmount(string.Empty);

            collateralEnv = new List<CBCollateralEnv>
            {
                CBCollateralEnv.DefaultCollateralEnv()
            };
        }

        /// <summary>
        ///  Nouvelle instace d'un collateral de type Assets (Dépôts de garantie (asset de type Bond, Equity, etc..))
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pDtBusinessCollateral"></param>
        /// <param name="pIda_Css"></param>
        /// <param name="pIsValorised"></param>
        /// <param name="pQty"></param>
        /// <param name="pAssetCategory"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pHaircutForced"></param>
        /// <param name="pStatus"></param>
        /// PM 20150901 pHaircutForced devient nullable
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// FI 20160530 [21885] Modify
        /// EG 20170127 Qty Long To Decimal
        /// FI 20170208 [22151][22152] add pStatus
        public CBDetCollateral(int pIdb, int pIda, decimal pAmount, string pCurrency,
            DateTime pDtBusinessCollateral, int pIda_Css, bool pIsValorised, decimal? pQty, string pAssetCategory, int pIdAsset,
            decimal? pHaircutForced, StatusEnum pStatus)
            : base(pIdb, pIda, pAmount, pCurrency, FlowTypeEnum.Collateral, FlowSubTypeEnum.None, pDtBusinessCollateral, pStatus)
        {
            CollateralCategory = CollateralCategoryEnum.Assets;

            // PM 20150616 [21124] DtBusinessCollateral déjà initialisé dans la class de base grace à DtValue
            // DtBusinessCollateral = pDtBusinessCollateral;
            Ida_Css = pIda_Css;
            Ida_CssSpecified = (Ida_Css > 0);

            IsCollatValorised = pIsValorised;
            // FI 20160530 [21885] add Qty
            Qty = pQty;

            AssetCategory = pAssetCategory;
            // FI 20160530 [21885] add 
            AssetCategorySpecified = true;
            IdAsset = pIdAsset;

            HaircutForced = pHaircutForced;

            InitializeAmount(pCurrency);
        }

        private void InitializeAmount(string pCurrency)
        {
            Amount_MGCCTRVal = new CollatAmount_MGCCTRVal(pCurrency);
            Amount_MGCCollatCTRVal = new CollatAmount_MGCCollatCTRVal(pCurrency);

            CollatAvailable_FlowCur = new List<Money>();
            CollatUsed_FlowCur = new List<Money>();
            CollatUsedGross_FlowCur = new List<Money>();
        }

        /// <summary>
        /// Comparer le CBAmount courant à {pObj}, selon les critères suivants et dans l'ordre:
        /// <para>IDA</para>
        /// <para>IDB</para>
        /// <para>AmountType</para>
        /// <para>AmountSubType, s'il existe sur l'un des deux</para>
        /// </summary>
        /// <param name="pObj">L'objet à comparer avec le CBAmount courant</param>
        /// <returns>
        /// <para> 0 : égalité</para>
        /// <para>-1 : différence</para>
        /// </returns>
        public override int CompareTo(object pObj)
        {
            int ret = base.CompareTo(pObj);
            if (ret != -1)
            {
                // Pour considérer toujours unitairement tous les montants de Collateral
                ret = -1;
            }
            return ret;
        }
    }

    /// <summary>
    /// Classe portant les détails de chaque flux du dérnier Cash-Balance.
    /// </summary>    
    [XmlRoot(ElementName = "FlowDetail")]
    public class CBDetLastFlow : CBDetFlow
    {
        // EG 20160404 Migration vs2013
        //DateTime m_DtMarketPrev;

        /// <summary>
        /// L'Id du Trade du montant
        /// </summary>
        [XmlAttribute(AttributeName = "idt")]
        public int Idt;

        /// <summary>
        /// L'Identifier du Trade du montant
        /// </summary>
        [XmlAttribute(AttributeName = "identifierTrade")]
        public string Identifier_t;

        /// <summary>
        /// Date compensation Précédent,facultatif, utilisé pour le cas des Cash-Balance veille
        /// </summary>
        [XmlAttribute(AttributeName = "dtBusinessPrev")]
        public DateTime DtMarketPrev
        {
            get { return m_DtMarketPrev; }
            set { m_DtMarketPrev = value; }
        }
        [XmlIgnore]
        public bool DtMarketPrevSpecified;

        //
        public CBDetLastFlow() { }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdb"></param>
        /// <param name="pIda"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pAmountSubType"></param>
        /// <param name="pDtValue"></param>
        /// <param name="pIdt"></param>
        /// <param name="pIdentifier_t"></param>
        /// <param name="pStatus"></param>
        /// PM 20150616 [21124] Add pDtValue
        /// FI 20170208 [22151][22152] add pStatus
        public CBDetLastFlow(int pIdb, int pIda, decimal pAmount, string pCurrency, FlowSubTypeEnum pAmountSubType, DateTime pDtValue,
            int pIdt, string pIdentifier_t, StatusEnum pStatus)
            : base(pIdb, pIda, pAmount, pCurrency, FlowTypeEnum.LastCashBalance, pAmountSubType, pDtValue, pStatus)
        {
            Idt = pIdt;
            Identifier_t = pIdentifier_t;
        }
        /// <summary>
        /// Comparer le CBLastFlow courant à {pObj}, selon les critères suivants et dans l'ordre:
        /// <para>IDA</para>
        /// <para>IDB</para>
        /// <para>AmountType</para>
        /// <para>AmountSubType, s'il existe sur l'un des deux</para>
        /// </summary>
        /// <param name="pObj">L'objet à comparer avec le CBAmount courant</param>
        /// <returns>
        /// <para> 0 : égalité</para>
        /// <para>-1 : différence</para>
        /// </returns>
        // EG 20160404 Migration vs2013
        //public virtual int CompareTo(object pObj)
        public override int CompareTo(object pObj)
        {
            int ret = base.CompareTo(pObj);

            if (ret != -1)
            {
                if (pObj is CBDetLastFlow amount)
                {
                    if ((amount.DtMarketPrevSpecified || this.DtMarketPrevSpecified) &&
                        (amount.DtMarketPrev != DtMarketPrev))
                    {
                        ret = -1;
                    }
                }
                else
                    ret = -1;
            }

            return ret;
        }
    }

    /// <summary>
    /// Représente une cotation composée:
    /// <para>Du taux utilisé</para>
    /// <para>De l’Id de la cotation retenue</para>
    /// </summary>
    [XmlRoot(ElementName = "quote")]
    public class CBQuote : IComparable
    {
        /// <summary>
        /// Le taux utilisé
        /// </summary>
        [XmlElementAttribute("rate")]
        public FpML.v44.Shared.FxRate Rate;

        /// <summary>
        /// l’Id de la cotation retenue, 0 si taux calculé
        /// </summary>
        [XmlElementAttribute("idQuote")]
        public int IdQuote;

        public CBQuote() { }
        public CBQuote(FpML.v44.Shared.FxRate pRate, int pIdQuote)
        {
            Rate = pRate;
            IdQuote = pIdQuote;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public int CompareTo(object pObj)
        {
            if (pObj is CBQuote quote)
            {
                int ret = 0;
                if (quote.IdQuote != IdQuote)
                    ret = -1;
                else if (quote.Rate.rate.DecValue != Rate.rate.DecValue)
                    ret = -1;
                return ret;
            }
            throw new ArgumentException("object is not a CBQuote");
        }
    }

    /// <summary>
    /// IEqualityComparer implementation for the CBQuote class
    /// </summary>
    class CBQuoteComparer : IEqualityComparer<CBQuote>
    {
        /// <summary>
        /// Check the equality of two pair objects
        /// </summary>
        /// <param name="x">first pair to to be compared</param>
        /// <param name="y">second pair to be compared</param>
        /// <returns>true when the provided pairs are equals</returns>
        public bool Equals(CBQuote x, CBQuote y)
        {

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            //Check whether the CBQuote' properties are equal.
            return (x.CompareTo(y) == 0);
        }
        /// <summary>
        /// Get the hashing code of the input CBQuote
        /// </summary>
        /// <param name="cbQuote">input CBQuote object we want to compute the hashing code</param>
        /// <returns>the hashing code of the provided CBQuote</returns>
        public int GetHashCode(CBQuote cbQuote)
        {
            //Check whether the object is null
            if (cbQuote is null) return 0;

            int hashCode1 = cbQuote.IdQuote.GetHashCode();
            //int hashCode2 = cbQuote.Rate.quotedCurrencyPair.currency1.Value.GetHashCode();
            //int hashCode3 = cbQuote.Rate.quotedCurrencyPair.currency2.Value.GetHashCode();
            //int hashCode4 = cbQuote.Rate.quotedCurrencyPair.quoteBasis.ToString().GetHashCode();
            int hashCode5 = cbQuote.Rate.rate.DecValue.GetHashCode();

            //Calculate the hash code for the CBQuote.
            return hashCode1/* ^ hashCode2 ^ hashCode3 ^ hashCode4*/ ^ hashCode5;
        }
    }

    /// <summary>
    /// Représente un montant contre-valorisé et le taux utilisé
    /// </summary>
    [XmlRoot(ElementName = "Exchange")]
    public class CBExAmount
    {
        /// <summary>
        /// la devise du flux d'origine
        /// </summary>
        [XmlAttribute(AttributeName = "fcu")]
        public string FlowCurrency;

        /// <summary>
        /// Montant contre-valorisé
        /// </summary>
        [XmlElementAttribute("amount")]
        public Money CurrencyAmount;

        /// <summary>
        /// Les taux utilisés
        /// </summary>
        [XmlArray(ElementName = "rates")]
        [XmlArrayItemAttribute("rate")]
        public List<CBQuote> Quote;

        /// <summary>
        /// 
        /// </summary>
        public CBExAmount() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFlowCurrency"></param>
        /// <param name="pCurrencyAmount"></param>
        /// <param name="pQuote"></param>
        public CBExAmount(string pFlowCurrency, Money pCurrencyAmount, List<CBQuote> pQuote)
        {
            FlowCurrency = pFlowCurrency;
            CurrencyAmount = pCurrencyAmount;
            Quote = pQuote;
        }
    }

    /// <summary>
    /// Représente le couple (Actor,Book) vis à vis duquel le trade Cash-Blance sera généré
    /// </summary>    
    [XmlRoot(ElementName = "PartyTradeInfo")]
    public class CBPartyTradeInfo
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "ida")]
        public int Ida;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "identifier_a")]
        public string Identifier_a;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray(ElementName = "roles")]
        [XmlArrayItemAttribute("role")]
        public List<ActorRole> Roles;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "idb")]
        public int Idb;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "identifier_b")]
        public string Identifier_b;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "cbo")]
        public CBActorNode ActorCBO;

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "isCBOBook")]
        public bool IsCBOBook;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray(ElementName = "firstMROChilds")]
        [XmlArrayItemAttribute("firstMROChild")]
        public List<CBActorNode> FirstMROChilds;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "mro")]
        public CBActorNode ActorMRO;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "isMROBook")]
        public bool IsMROBook;
        #endregion Members

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CBPartyTradeInfo() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIdentifier_a"></param>
        /// <param name="pRoles"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdentifier_b"></param>
        /// <param name="pActorCBO"></param>
        /// <param name="pFirstMROChilds"></param>
        public CBPartyTradeInfo(int pIda, string pIdentifier_a, List<ActorRole> pRoles,
            int pIdb, string pIdentifier_b, CBActorNode pActorCBO, List<CBActorNode> pFirstMROChilds)
            : this(pIda, pIdentifier_a, pRoles, pIdb, pIdentifier_b, pActorCBO, true, pFirstMROChilds, null, false) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIdentifier_a"></param>
        /// <param name="pRoles"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdentifier_b"></param>
        /// <param name="pActorCBO"></param>
        /// <param name="pActorMRO"></param>
        public CBPartyTradeInfo(int pIda, string pIdentifier_a, List<ActorRole> pRoles, int pIdb, string pIdentifier_b,
            CBActorNode pActorCBO, CBActorNode pActorMRO)
            : this(pIda, pIdentifier_a, pRoles, pIdb, pIdentifier_b, pActorCBO, false, null, pActorMRO, true) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIdentifier_a"></param>
        /// <param name="pRoles"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdentifier_b"></param>
        /// <param name="pActorCBO"></param>
        /// <param name="pActorMRO"></param>
        /// <param name="pIsMROBook"></param>
        public CBPartyTradeInfo(int pIda, string pIdentifier_a, List<ActorRole> pRoles, int pIdb, string pIdentifier_b,
            CBActorNode pActorCBO, CBActorNode pActorMRO, bool pIsMROBook)
            : this(pIda, pIdentifier_a, pRoles, pIdb, pIdentifier_b, pActorCBO, false, null, pActorMRO, pIsMROBook) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIdentifier_a"></param>
        /// <param name="pRoles"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdentifier_b"></param>
        /// <param name="pActorCBO"></param>
        /// <param name="pIsCBOBook"></param>
        /// <param name="pFirstMROChilds"></param>
        /// <param name="pActorMRO"></param>
        /// <param name="pIsMROBook"></param>
        public CBPartyTradeInfo(int pIda, string pIdentifier_a, List<ActorRole> pRoles, int pIdb, string pIdentifier_b,
            CBActorNode pActorCBO, bool pIsCBOBook, List<CBActorNode> pFirstMROChilds, CBActorNode pActorMRO, bool pIsMROBook)
        {
            Ida = pIda;
            Identifier_a = pIdentifier_a;
            Roles = pRoles;
            Idb = pIdb;
            Identifier_b = pIdentifier_b;
            //
            ActorCBO = pActorCBO;
            IsCBOBook = pIsCBOBook;
            FirstMROChilds = pFirstMROChilds;
            //
            ActorMRO = pActorMRO;
            IsMROBook = pIsMROBook;
        }

        #endregion constructor
    }

    

    /// <summary>
    /// Représente plusieurs instructions de couverture
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("COLLATERALENVS", Namespace = "", IsNullable = true)]
    public class CBCollateralEnvs : IComparer
    {
        /// <summary>
        /// Array des instructions de couverture
        /// </summary>
        [System.Xml.Serialization.XmlElement("COLLATERALENV", Namespace = "", IsNullable = true)]
        public CBCollateralEnv[] collateralEnv;

        #region Method
        /// <summary>
        /// Chargement des instructions de couverture pour un déposant, un dépositaire
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        public void LoadCollateralEnv(string pCS, IDbTransaction pDbTransaction, int pIdaPayer, RoleActor[] pRolePayer, int pIdaReceiver, RoleActor[] pRoleReceiver)
        {

            StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
            sqlQuery += "IDCOLLATERALENV," + Cst.CrLf;
            sqlQuery += "ASSETCATEGORY,IDASSET," + Cst.CrLf;
            sqlQuery += "PAYTYPEPARTY as PAYTYPEPARTY,IDPAY as PAYIDA,IDROLEACTOR_PAY as PAYROLEACTOR," + Cst.CrLf;
            sqlQuery += "RECTYPEPARTY as RECTYPEPARTY,IDREC as RECIDA,IDROLEACTOR_REC as RECROLEACTOR," + Cst.CrLf;
            sqlQuery += "IDA_CSS," + Cst.CrLf;
            sqlQuery += "HAIRCUT," + Cst.CrLf;
            sqlQuery += "MAXQTY," + Cst.CrLf;
            sqlQuery += "DEFINITION,DOCUMENTATION," + Cst.CrLf;
            sqlQuery += "ISCOLLATALLOWED" + Cst.CrLf;
            //
            //Acteur Déposant
            SQLActorBookCriteria sqlActorBookCriteriaA = new SQLActorBookCriteria(pCS, pDbTransaction, pIdaPayer, 0, SQL_Table.ScanDataDtEnabledEnum.Yes)
            {
                ColumnTYPEPARTY = "PAYTYPEPARTY",
                ColumnIDPARTY = "IDPAY",
                ColumnIDROLE = "IDROLEACTOR_PAY",
                RoleActor = pRolePayer
            };
            string restrictActorA = sqlActorBookCriteriaA.GetSQLRestrictionAndSignature(pCS, pDbTransaction, "collatenv", RoleActorBookRestrict.COLLATERAL);
            //
            //
            //Acteur Dépositaire
            SQLActorBookCriteria sqlActorBookCriteriaB = new SQLActorBookCriteria(pCS, pDbTransaction, pIdaReceiver, 0, SQL_Table.ScanDataDtEnabledEnum.Yes)
            {
                ColumnTYPEPARTY = "RECTYPEPARTY",
                ColumnIDPARTY = "IDREC",
                ColumnIDROLE = "IDROLEACTOR_REC",
                RoleActor = pRoleReceiver
            };
            string restrictActorB = sqlActorBookCriteriaB.GetSQLRestrictionAndSignature(pCS, pDbTransaction, "collatenv", RoleActorBookRestrict.COLLATERAL);

            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.COLLATERALENV + " collatenv" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, "collatenv") + Cst.CrLf;
            sqlQuery += SQLCst.AND + "((" + restrictActorA + ")" + SQLCst.AND + "(" + restrictActorB + "))" + Cst.CrLf;

            QueryParameters qryParameter = new QueryParameters(pCS, sqlQuery.ToString(), null);

            CBCollateralEnvs tmp = LoadFromQuery(qryParameter, pDbTransaction);
            collateralEnv = tmp.collateralEnv;
        }

        /// <summary>
        /// Tri des instructions de couverture
        /// <para>Attribution d'un poids à chaque instruction, les instructions de poids le plus faible sont prioritaires</para>
        /// <para>l'instruction la plus prioritaire a pour poids "0",la suivante a pour poids "1", etc...  </para>
        /// <para>Voir la méthode Compare pour l'explication sur le tri</para>
        /// </summary>
        public void Sort()
        {
            if (ArrFunc.IsFilled(collateralEnv))
            {
                Array.Sort(collateralEnv, this);
                for (int i = 0; i < ArrFunc.Count(collateralEnv); i++)
                    collateralEnv[i].Weight = i;
            }
        }

        /// <summary>
        /// Retourne une CollateralEnvs chargé à partir d'une query 
        /// </summary>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        private static CBCollateralEnvs LoadFromQuery(QueryParameters QryParameters, IDbTransaction pDbTransaction)
        {
            DataSet dsResult;
            if (null == QryParameters.Parameters)
            {
                dsResult = DataHelper.ExecuteDataset(QryParameters.Cs, pDbTransaction, CommandType.Text, QryParameters.Query, null);
            }
            else
            {
                dsResult = DataHelper.ExecuteDataset(QryParameters.Cs, pDbTransaction, CommandType.Text, QryParameters.Query, QryParameters.Parameters.GetArrayDbParameter());
            }
            dsResult.DataSetName = "COLLATERALENVS";

            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "COLLATERALENV";

            string dsSerializerResult = new DatasetSerializer(dsResult).Serialize();

            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(CBCollateralEnvs), dsSerializerResult);
            CBCollateralEnvs ret = (CBCollateralEnvs)CacheSerializer.Deserialize(serializeInfo);
            return ret;
        }
        #endregion

        #region IComparer Membres
        /// <summary>
        /// Compare 2 éléments de type CollateralEnv
        /// <para>
        /// Retourne -1 si x est prioritaire vis à vis de y
        /// </para>
        /// <para>
        /// Retourne 1 si y est prioritaire vis à vis de x
        /// </para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <Remarks>
        /// On prend par convention l’ordre suivant pour les priorités (du plus prioritaire au moins prioritaire)
        /// <para>
        /// Déposant
        ///     Partie
        ///         Acteur
        ///         Groupe d'acteurs
        ///     Rôle
        /// 
        /// Dépositaire
        ///     Partie
        ///         Acteur
        ///         Groupe d'acteurs
        ///     Rôle
        /// 
        /// Chambre de compensation
        /// 
        /// Actif
        /// Catégorie d’actif
        ///
        /// Abattement (du plus petit au plus grand)
        /// </para>
        /// </Remarks>
        public int Compare(object x, object y)
        {
            CBCollateralEnv xCollateralEnv = x as CBCollateralEnv;
            CBCollateralEnv yCollateralEnv = y as CBCollateralEnv;
            if (x == null)
                throw new NotImplementedException(StrFunc.AppendFormat("type [{0}] is not implemented", x.GetType().ToString()));
            if (y == null)
                throw new NotImplementedException(StrFunc.AppendFormat("type [{0}] is not implemented", y.GetType().ToString()));

            int ret = ComparePayerReceiver(xCollateralEnv, yCollateralEnv, PayerReceiverEnum.Payer);

            if (ret == 0)
                ret = ComparePayerReceiver(xCollateralEnv, yCollateralEnv, PayerReceiverEnum.Receiver);

            if (ret == 0)
            {
                if ((xCollateralEnv.IsCssIdSpecified) && ((false == yCollateralEnv.IsCssIdSpecified)))
                    ret = -1;
                else if ((false == xCollateralEnv.IsCssIdSpecified) && ((yCollateralEnv.IsCssIdSpecified)))
                    ret = 1;
            }

            // RD 20111202 Actif non pris en compte
            if (ret == 0)
            {
                if (xCollateralEnv.IsIdAssetSpecified && ((false == yCollateralEnv.IsIdAssetSpecified)))
                {
                    ret = -1;
                }
                else if ((false == xCollateralEnv.IsIdAssetSpecified) && ((yCollateralEnv.IsIdAssetSpecified)))
                {
                    ret = 1;
                }
            }

            //En cas de 2 instructions de même finesse qui seraient contradictoires, l’une autorisant, l’autre non, 
            //je dirai de ne pas autorisé l’utilisation de l’actif (issu de PM)
            if (ret == 0)
            {
                if ((false == xCollateralEnv.IsCollateralAllowed) && yCollateralEnv.IsCollateralAllowed)
                {
                    ret = -1;
                }
                else if (xCollateralEnv.IsCollateralAllowed && (false == yCollateralEnv.IsCollateralAllowed))
                {
                    ret = 1;
                }
            }

            //Les directives avec le plus faible abattement sont prioriraires
            //L'abattement est pris en compte uniquement si les 2 instructions comparées autorisent la couverture
            if (ret == 0)
            {
                if (xCollateralEnv.IsCollateralAllowed && yCollateralEnv.IsCollateralAllowed)
                {
                    if (xCollateralEnv.HaircutValue() < yCollateralEnv.HaircutValue())
                        ret = -1;
                    else if (xCollateralEnv.HaircutValue() > yCollateralEnv.HaircutValue())
                        ret = 1;
                }
            }
            return ret;
        }
        /// <summary>
        /// Compare les éléments "party" associés 2 instruction de couverture
        /// <para>Retourne -1 si l'instruction {xCollateralEnv} est prioritaire vis à vis de l'instruction {yCollateralEnv}</para>
        /// </summary>
        /// <param name="xCollateralEnv"></param>
        /// <param name="yCollateralEnv"></param>
        /// <param name="pPayrec">Payer pour Déposant, Receiver pour Dépositaire</param>
        /// <returns></returns>
        private static int ComparePayerReceiver(CBCollateralEnv xCollateralEnv, CBCollateralEnv yCollateralEnv, PayerReceiverEnum pPayrec)
        {
            int ret = 0;

            if ((xCollateralEnv.IsPayerReceiverSpecified(pPayrec)) &&
                (false == yCollateralEnv.IsPayerReceiverSpecified(pPayrec)))
            {
                ret = -1;
            }
            else if ((false == xCollateralEnv.IsPayerReceiverSpecified(pPayrec)) &&
                     (yCollateralEnv.IsPayerReceiverSpecified(pPayrec)))
            {
                ret = 1;
            }
            //
            if (ret == 0)
            {
                if ((xCollateralEnv.PayTypeParty.Value == TypePartyEnum.Actor) &&
                            (yCollateralEnv.PayTypeParty.Value == TypePartyEnum.GrpActor))
                {
                    ret = -1;
                }
                else if ((xCollateralEnv.PayTypeParty.Value == TypePartyEnum.GrpActor) &&
                            (yCollateralEnv.PayTypeParty.Value == TypePartyEnum.Actor))
                {
                    ret = 1;
                }
            }
            //
            if (ret == 0)
            {
                if ((xCollateralEnv.IsPayerReceiverRoleSpecified(pPayrec)) &&
                     (false == yCollateralEnv.IsPayerReceiverRoleSpecified(pPayrec)))
                {
                    ret = -1;
                }
                else if ((yCollateralEnv.IsPayerReceiverRoleSpecified(pPayrec)) &&
                        (false == xCollateralEnv.IsPayerReceiverRoleSpecified(pPayrec)))
                {
                    ret = 1;
                }
            }
            return ret;
        }


        #endregion
    }

    /// <summary>
    /// Représente une instruction de couverture
    /// </summary>
    /// FI 20160530 [21885] Modify 
    public class CBCollateralEnv
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDCOLLATERALENV")]
        public int IdCollateralEnv
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ASSETCATEGORY")]
        public Nullable<Cst.UnderlyingAsset> AssetCategory
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDASSET")]
        public Nullable<int> IdAsset
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("PAYTYPEPARTY")]
        public Nullable<TypePartyEnum> PayTypeParty
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("PAYIDA")]
        public Nullable<int> PayIdParty
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("PAYROLEACTOR")]
        public Nullable<RoleActor> PayRole
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("RECTYPEPARTY")]
        public Nullable<TypePartyEnum> RecTypeParty
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("RECIDA")]
        public Nullable<int> RecIdParty
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("RECROLEACTOR")]
        public Nullable<RoleActor> RecRole
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDA_CSS")]
        public Nullable<int> CssId
        {

            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ISCOLLATALLOWED")]
        public Boolean IsCollateralAllowed
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("MAXQTY")]
        public Nullable<decimal> MaxQty
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("HAIRCUT")]
        public Nullable<decimal> Haircut
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DEFINITION")]
        public string Definition
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DOCUMENTATION")]
        public string Documentation
        {
            get;
            set;
        }

        /// <summary>
        /// Poids, l'instruction de poids le plus faible est prioritaire
        /// <para>Valeur nécessairement >0</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int Weight
        {
            get;
            set;
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsPayerReceiverSpecified(PayerReceiverEnum pPayerReceiver)
        {

            Boolean ret = false;
            if (pPayerReceiver == PayerReceiverEnum.Payer)
                ret = IsObjectSpecified(this.PayIdParty);
            else if (pPayerReceiver == PayerReceiverEnum.Receiver)
                ret = IsObjectSpecified(this.RecIdParty);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPayerReceiverRoleSpecified(PayerReceiverEnum pPayerReceiver)
        {
            Boolean ret = false;
            if (pPayerReceiver == PayerReceiverEnum.Payer)
                ret = IsObjectSpecified(this.PayRole);
            else if (pPayerReceiver == PayerReceiverEnum.Receiver)
                ret = IsObjectSpecified(this.RecRole);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPayerSpecified
        {
            get
            {
                return IsPayerReceiverSpecified(PayerReceiverEnum.Payer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPayerRoleSpecified
        {
            get
            {
                return IsPayerReceiverRoleSpecified(PayerReceiverEnum.Payer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReceiverSpecified
        {
            get
            {
                return IsPayerReceiverSpecified(PayerReceiverEnum.Receiver);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReceiverRoleSpecified
        {
            get
            {
                return IsPayerReceiverRoleSpecified(PayerReceiverEnum.Receiver);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCssIdSpecified
        {
            get
            {
                return IsObjectSpecified(this.CssId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIdAssetSpecified
        {
            get
            {
                return IsObjectSpecified(this.IdAsset);
            }
        }

        /// <summary>
        /// Retroune le Haircut
        /// <para>Retourne 0 si non rensigné</para>
        /// </summary>
        /// <returns></returns>
        public decimal HaircutValue()
        {
            decimal ret = decimal.Zero;
            if (IsObjectSpecified(this.Haircut))
                ret = Haircut.Value;
            return ret;
        }

        /// <summary>
        /// Retourne true si pObject!= null et s'il est renseigné
        /// <para>Exemple un integer est renseigné si sa valeur est >0</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <returns></returns>
        private static bool IsObjectSpecified(object pObject)
        {
            bool ret = (null != pObject);
            if (ret)
            {
                object value = pObject;
                //

                if (value.GetType().Equals(typeof(int)))
                {
                    ret = ((int)value) > 0;
                }
                else if (value.GetType().Equals(typeof(string)))
                {
                    ret = StrFunc.IsFilled((string)value);
                }
                else if (value.GetType().Equals(typeof(RoleActor)))
                {
                    ret = true;
                }
                else if (value.GetType().Equals(typeof(decimal)))
                {
                    ret = ((decimal)value) > 0;
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("type [{0}] is not implemented", value.GetType().ToString()));


            }
            return ret;
        }
        public static CBCollateralEnv DefaultCollateralEnv()
        {
            return DefaultCollateralEnv(null, null, null, null);
        }
        /// <summary>
        ///  Retourne l'instruction par défaut établie par Spheres® s'il n'existe aucune instruction paramétrée qui matche avec une couverture 
        ///  <para>Cette instruction stipule que la couverture est à utiliser sans abattement</para>
        ///  <para>Alimente documentation avec 'Default instruction set automatically' </para>
        /// </summary>
        /// <returns></returns>
        //FI/PL 20170308 [22939]
        public static CBCollateralEnv DefaultCollateralEnv(Nullable<int> pIda_Pay, Nullable<int> pIda_Rec,
            Nullable<Cst.UnderlyingAsset> pAssetCategory, Nullable<int> pIdAsset)
        {
            CBCollateralEnv ret = new CBCollateralEnv
            {
                PayIdParty = pIda_Pay,
                PayTypeParty = TypePartyEnum.Actor,
                RecIdParty = pIda_Rec,
                RecTypeParty = TypePartyEnum.Actor,
                //ret.cssId = null;
                AssetCategory = pAssetCategory,
                IdAsset = pIdAsset,

                Documentation = "Default instruction set automatically",

                MaxQty = null,
                Haircut = null,
                IsCollateralAllowed = true
            };

            return ret;
        }
    }

    /// <summary>
    /// Classe des montants en contrevaleur du cash balance
    /// </summary>
    ///PM 20140918 [20066][20185] New
    public class CBCtrValTradeInfo
    {
        #region members
        /// <summary>
        /// 
        /// </summary>
        public string Currency;
        /// <summary>
        /// 
        /// </summary>
        public List<CBQuote> Quote;

        /// <summary>
        /// 
        /// </summary>
        public Money PreviousCashBalance;
        /// <summary>
        /// 
        /// </summary>
        public Money Premium;
        // PM 20150616 [21124] Remplacé par le détail
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money RealizedMargin;
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money RealizedMarginFuture;
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money RealizedMarginOptionPremium;
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money RealizedMarginOptionMarkToMarket;
        ///// <summary>
        ///// 
        ///// </summary>
        ////PM 20150319 [POC] Add RealizedMarginOther
        //public Money RealizedMarginOther;
        /// <summary>
        /// 
        /// </summary>
        public Money CashSettlement;
        /// <summary>
        /// 
        /// </summary>
        //PM 20150319 [POC] Add CashSettlementOptionPremium
        public Money CashSettlementOptionPremium;
        /// <summary>
        /// 
        /// </summary>
        //PM 20150319 [POC] Add CashSettlementOptionMarkToMarket
        public Money CashSettlementOptionMarkToMarket;
        /// <summary>
        /// 
        /// </summary>
        //PM 20150319 [POC] Add CashSettlementOther
        public Money CashSettlementOther;
        // Remplacé par le détail CBFlowsFeesAndTax
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money Fee;
        /// <summary>
        /// 
        /// </summary>
        public Money Funding;
        /// <summary>
        /// Borrowing
        /// </summary>
        //PM 20150323 [POC] Add Borrowing
        public Money Borrowing;
        // PM 20150616 [21124] Remplacé par le détail
        ///// <summary>
        ///// 
        ///// </summary>
        ////PM 20150319 [POC] Add Unsettled Cash
        //public Money UnsettledCash;
        /// <summary>
        /// 
        /// </summary>
        public Money CashPayment;
        /// <summary>
        /// 
        /// </summary>
        public Money CashPaymentDeposit;
        /// <summary>
        /// 
        /// </summary>
        public Money CashPaymentWithdrawal;
        // PM 20150616 [21124] Remplacé par le détail
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money UnrealizedMargin;
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money UnrealizedMarginFuture;
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money UnrealizedMarginOptionPremium;
        ///// <summary>
        ///// 
        ///// </summary>
        //public Money UnrealizedMarginOptionMarkToMarket;
        ///// <summary>
        ///// 
        ///// </summary>
        ////PM 20150319 [POC] Add UnrealizedMarginOther
        //public Money UnrealizedMarginOther;
        /// <summary>
        /// 
        /// </summary>
        public Money OptionValue;
        /// <summary>
        /// 
        /// </summary>
        public Money LongOptionValue;
        /// <summary>
        /// 
        /// </summary>
        public Money ShortOptionValue;
        /// <summary>
        /// Market Value
        /// </summary>
        /// PM 20150616 [21124] add MarketValue
        public Money MarketValue;
        /// <summary>
        /// 
        /// </summary>
        public Money CashBalance;
        /// <summary>
        /// 
        /// </summary>
        public Money EquityBalance;
        /// <summary>
        /// 
        /// </summary>
        public Money CashForward;
        /// <summary>
        /// 
        /// </summary>
        public Money CashForwardDeposit;
        /// <summary>
        /// 
        /// </summary>
        public Money CashForwardWithdrawal;
        /// <summary>
        /// 
        /// </summary>
        public Money EquityBalanceWithForwardCash;
        /// <summary>
        /// 
        /// </summary>
        public Money MarginRequirement;
        /// <summary>
        /// 
        /// </summary>
        public Money CollateralAvailable;
        /// <summary>
        /// 
        /// </summary>
        public Money ExcessDeficit;
        /// <summary>
        /// 
        /// </summary>
        public Money ExcessDeficitWithForwardCash;
        /// <summary>
        /// 
        /// </summary>
        public Money VariationMargin;
        /// <summary>
        /// 
        /// </summary>
        public Money CashFlows;
        /// <summary>
        /// 
        /// </summary>
        public Money CashAvailable;
        /// <summary>
        /// Total Account Value
        /// </summary>
        /// PM 20150616 [21124] add TotalAccountValue
        public Money TotalAccountValue;

        /// <summary>
        /// Equalisation Payment
        /// </summary>
        /// PM 20170911 [23408] Add EqualisationPayment
        public Money EqualisationPayment;

        /// <summary>
        /// Cash-Flow: Realized Margin
        /// </summary>
        /// PM 20150616 [21124] Add Realized Margin detailled flows
        public CBFlows CBFlowsRealizedMargin;

        /// <summary>
        /// Cash-Flow: Unrealized Margin
        /// </summary>
        /// PM 20150616 [21124] Add Unrealized Margin detailled flows
        public CBFlows CBFlowsUnrealizedMargin;

        /// <summary>
        /// Cash-Flow: Unsettled Transaction
        /// </summary>
        /// PM 20150616 [21124] Add Unsettled Cash detailled flows
        public CBFlows CBFlowsUnsettledCash;

        /// <summary>
        /// Fees et Tax
        /// </summary>
        public CBFlows CBFlowsFeesAndTax;

        /// <summary>
        /// Safe Keeping Payment
        /// </summary>
        /// PM 20150709 [21103] Add CBFlowsSafeKeepingPayment
        public CBFlows CBFlowsSafeKeepingPayment;
        #endregion
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        public CBCtrValTradeInfo()
        {
            Quote = new List<CBQuote>();
        }
        #endregion
    }

    /// <summary>
    /// Classe de stockage et manipulation des ensembles de flux
    /// </summary>
    ///PM 20140908 [20066][20185] New
    public class CBFlows
    {
        #region members
        private IEnumerable<CBDetFlow> m_FlowList;
        #endregion
        #region accessors
        /// <summary>
        /// Ensemble des flux
        /// </summary>
        public IEnumerable<CBDetFlow> Flows
        {
            get
            {
                return m_FlowList ?? new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// Ensemble des Cash Flows
        /// </summary>
        public IEnumerable<CBDetCashFlows> CashFlows
        {
            get
            {
                IEnumerable<CBDetCashFlows> ret = new List<CBDetCashFlows>();
                if (m_FlowList != null)
                {
                    ret = from flow in m_FlowList
                          where (flow.Type == FlowTypeEnum.CashFlows)
                          select (CBDetCashFlows)flow;
                }
                return ret;
            }
        }
        /// <summary>
        /// Ensemble des Payment en date de Settlement
        /// </summary>
        public IEnumerable<CBDetStlPayment> SettlementPayment
        {
            get
            {
                IEnumerable<CBDetStlPayment> ret = new List<CBDetStlPayment>();
                if (m_FlowList != null)
                {
                    ret = from flow in m_FlowList
                          where (flow.Type == FlowTypeEnum.SettlementPayment)
                          select (CBDetStlPayment)flow;
                }
                return ret;
            }
        }
        #endregion
        #region constructors
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        public CBFlows(){}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetFlows"></param>
        public CBFlows(IEnumerable<CBDetFlow> pDetFlows)
        {
            if (pDetFlows != null)
            {
                m_FlowList = pDetFlows;
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetFlows"></param>
        public CBFlows(IEnumerable<CBDetCashFlows> pDetFlows)
        {
            if (pDetFlows != null)
            {
                m_FlowList = pDetFlows.Cast<CBDetFlow>();
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetFlows"></param>
        /// <param name="pType"></param>
        public CBFlows(IEnumerable<CBDetFlow> pDetFlows, FlowTypeEnum pType)
        {
            if (pDetFlows != null)
            {
                m_FlowList = from flow in pDetFlows
                             where (flow.Type == pType)
                             select flow;
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetFlows"></param>
        /// <param name="pType"></param>
        /// <param name="pSubType"></param>
        public CBFlows(IEnumerable<CBDetFlow> pDetFlows, FlowTypeEnum pType, FlowSubTypeEnum pSubType)
        {
            if (pDetFlows != null)
            {
                m_FlowList = from flow in pDetFlows
                    where (flow.Type == pType) && (flow.FlowSubType == pSubType)
                    select flow;
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetFlows"></param>
        /// <param name="pIdb"></param>
        /// <param name="pType"></param>
        public CBFlows(IEnumerable<CBDetFlow> pDetFlows, int pIdb, FlowTypeEnum pType)
        {
            if (pDetFlows != null)
            {
                m_FlowList = from flow in pDetFlows
                             where (flow.Type == pType) && (flow.IDB == pIdb)
                             select flow;
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetFlows"></param>
        /// <param name="pType"></param>
        /// <param name="pSubType"></param>
        /// <param name="pIdb"></param>
        public CBFlows(IEnumerable<CBDetFlow> pDetFlows, int pIdb, FlowTypeEnum pType, FlowSubTypeEnum pSubType)
        {
            if (pDetFlows != null)
            {
                m_FlowList = from flow in pDetFlows
                    where (flow.Type == pType) && (flow.FlowSubType == pSubType) && (flow.IDB == pIdb)
                    select flow;
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDetStlPayment"></param>
        /// <param name="pIsForward"></param>
        public CBFlows(IEnumerable<CBDetStlPayment> pDetStlPayment, bool pIsForward)
        {
            if (pDetStlPayment != null)
            {
                m_FlowList = from flow in pDetStlPayment
                             where (flow.IsForward == pIsForward)
                             select (CBDetFlow)flow;
            }
            else
            {
                m_FlowList = new List<CBDetFlow>();
            }
        }
        #endregion
        #region methods
        /// <summary>
        /// Concatène les flux des noeuds enfants
        /// </summary>
        /// <param name="pChilds"></param>
        public void AddChildFlows(List<CBActorNode> pChilds)
        {
            if (pChilds != null)
            {
                m_FlowList.Concat(from actor in pChilds
                                  from flow in actor.Flows
                                  select flow);
            }
        }
        /// <summary>
        /// Concatène les flux des noeuds enfants
        /// </summary>
        /// <param name="pChilds"></param>
        /// <param name="pType"></param>
        /// <param name="pStatus"></param>
        /// FI 20170208 [22151][22152] Add parameter pStatus
        public void AddChildFlows(List<CBActorNode> pChilds, FlowTypeEnum pType, StatusEnum pStatus)
        {
            if (pChilds != null)
            {
                m_FlowList.Concat(from actor in pChilds
                                  // FI 20170208 [22151][22152] call GetFlows
                                  //from flow in actor.Flows
                                  from flow in actor.GetFlows(pStatus)
                                  where (flow.Type == pType)
                                  select flow);
            }
        }
        /// <summary>
        /// Concatène les flux des noeuds enfants
        /// </summary>
        /// <param name="pChilds"></param>
        /// <param name="pType"></param>
        /// <param name="pSubType"></param>
        /// <param name="pStatus"></param>
        /// FI 20170208 [22151][22152] Add parameter pStatus
        public void AddChildFlows(List<CBActorNode> pChilds, FlowTypeEnum pType, FlowSubTypeEnum pSubType, StatusEnum pStatus)
        {
            if (pChilds != null)
            {
                m_FlowList.Concat(from actor in pChilds
                                  // FI 20170208 [22151][22152] call GetFlows
                                  //from flow in actor.Flows
                                  from flow in actor.GetFlows(pStatus) 
                                  where (flow.Type == pType) && (flow.FlowSubType == pSubType)
                                  select flow);
            }
        }
        /// <summary>
        /// True si les flux ne sont pas vide et contiennent des éléments ayant un montant différent de 0 pour une devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        public bool IsFilledCurrencyFlows(string pIdC)
        {
            bool ret = (m_FlowList != null) &&
                (from flow in m_FlowList
                 where (flow != null)
                 from money in flow.CurrencyAmount
                 select money).ToList().Exists(money => (money.Amount.DecValue != 0) && (money.Currency == pIdC));
            return ret;
        }
        /// <summary>
        /// True si les flux ne sont pas vide et contiennent des éléments ayant une contrevaleur
        /// </summary>
        /// <returns></returns>
        public bool IsFilledCounterValueFlows()
        {
            bool ret = (m_FlowList != null) &&
                ((from flow in m_FlowList
                  where (flow != null) && (flow.CtrValAmount != default)
                  select flow).Count() > 0);
            return ret;
        }
        /// <summary>
        /// Recherche l'ensemble des différentes devises des flux
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> DistinctCurrency()
        {
            IEnumerable<string> ret = new List<string>();
            if (m_FlowList != null)
            {
                ret = (from flow in m_FlowList
                       where (flow != null)
                       from money in flow.CurrencyAmount
                       where StrFunc.IsFilled(money.Currency)
                       select money.Currency).Distinct();
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des montants des flux
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyAmount()
        {
            IEnumerable<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret = from flow in m_FlowList
                      where (flow != null) && (flow.CurrencyAmount != null )
                      from money in flow.CurrencyAmount
                      select money;
            }
            return ret;
        }
        /// Retourne l'ensemble des montants opposés [* (-1)]des flux
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyOppositeAmount()
        {
            return AllCurrencyAmountByFactor(-1);
        }
        /// <summary>
        /// <summary>
        /// Retourne un nouvel ensemble des montants des flux dont chaque montant est multiplié par un facteur
        /// </summary>
        /// <param name="pFactor"></param>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyAmountByFactor(decimal pFactor)
        {
            IEnumerable<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret = from flow in m_FlowList
                      where (flow != null) && (flow.CurrencyAmount != null)
                      from money in flow.CurrencyAmount
                      select new Money(money.Amount.DecValue * pFactor, money.Currency);
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des montants des flux de Cash Flow Pour les Futures ETD
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyETDFutureAmount()
        {
            IEnumerable<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret = from flow in CashFlows
                      where (flow.Category.HasValue && (flow.Category.Value == CfiCodeCategoryEnum.Future))
                      from money in flow.CurrencyAmount
                      select money;
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des montants des flux de Cash Flow Pour les Options ETD
        /// </summary>
        /// <param name="pOptValMeth"></param>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyETDOptionAmount(FuturesValuationMethodEnum pOptValMeth)
        {
            IEnumerable<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret = from flow in CashFlows
                      where (flow.Category.HasValue && (flow.Category.Value == CfiCodeCategoryEnum.Option))
                      && (flow.FutValuationMethod.HasValue && (flow.FutValuationMethod.Value == pOptValMeth))
                      from money in flow.CurrencyAmount
                      select money;
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des montants des flux de Cash Flow OTC
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyOTCAmount()
        {
            IEnumerable<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret = from flow in CashFlows
                      where (flow.Category.HasValue == false)
                      from money in flow.CurrencyAmount
                      select money;
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des montants des flux en fonction de leur Side
        /// </summary>
        /// <param name="pSide"></param>
        /// <returns></returns>
        public IEnumerable<Money> AllCurrencyAmountBySide(SideEnum pSide)
        {
            IEnumerable<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret = from flow in m_FlowList
                      where (flow != null) && (flow.CurrencyAmount != null)
                      && (flow is CBDetCashFlows detCashFlows)
                      && (detCashFlows.Side.HasValue && (detCashFlows.Side.Value == pSide))
                      from money in flow.CurrencyAmount
                      select money;
            }
            return ret;
        }
        /// <summary>
        /// Calcul la somme des flux par devise
        /// </summary>
        /// <returns></returns>
        public List<Money> CalcSumByIdC()
        {
            List<Money> ret = new List<Money>();
            if (m_FlowList != null)
            {
                ret =
                    (
                    from flow in m_FlowList
                    where (flow.CurrencyAmount != null)
                    from money in flow.CurrencyAmount
                    where (money != null)
                    group money by money.Currency
                    into moneyIdc
                    select new Money(moneyIdc.Sum(m => m.Amount.DecValue), moneyIdc.Key)
                    ).ToList();
            }
            return ret;
        }
        /// <summary>
        /// Calcul la somme des flux pour une devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        public Money CalcSumMoneyIdC(string pIdC)
        {
            Money ret = new Money(0, pIdC);
            if (m_FlowList != null)
            {
                decimal amount = (from flow in m_FlowList
                                  where (flow != null)
                                  from money in flow.CurrencyAmount
                                  where (money != null) && (money.Currency == pIdC)
                                  select money.Amount.DecValue).Sum();
                ret.amount.DecValue = amount;
            }
            return ret;
        }
        /// <summary>
        /// Calcul la somme des flux positif pour une devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        public Money CalcSumPositiveMoneyIdC(string pIdC)
        {
            Money ret = new Money(0, pIdC);
            if (m_FlowList != null)
            {
                decimal amount = (from flow in m_FlowList
                                  where (flow != null)
                                  from money in flow.CurrencyAmount
                                  where (money != null) && (money.Currency == pIdC) && (money.Amount.DecValue > 0)
                                  select money.Amount.DecValue).Sum();
                ret.amount.DecValue = amount;
            }
            return ret;
        }
        /// <summary>
        /// Calcul la somme des flux negatif pour une devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        public Money CalcSumNegativeMoneyIdC(string pIdC)
        {
            Money ret = new Money(0, pIdC);
            if (m_FlowList != null)
            {
                decimal amount = (from flow in m_FlowList
                                  where (flow != null)
                                  from money in flow.CurrencyAmount
                                  where (money != null) && (money.Currency == pIdC) && (money.Amount.DecValue < 0)
                                  select money.Amount.DecValue).Sum();
                ret.amount.DecValue = amount;
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des flux existant dans la devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] New
        public IEnumerable<CBDetFlow> FlowsByCurrency(string pIdC)
        {
            IEnumerable<CBDetFlow> flowCurrency =
                        from flow in Flows
                        from money in flow.CurrencyAmount
                        where (money != null) && (money.Currency == pIdC)
                        select flow;
            return flowCurrency;
        }
        /// <summary>
        /// Retourne l'ensemble des flux existant dans la devise donnée
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] New
        public IEnumerable<CBDetCashFlows> CashFlowsByCurrency(string pIdC)
        {
            IEnumerable<CBDetCashFlows> flowCurrency =
                        from flow in CashFlows
                        from money in flow.CurrencyAmount
                        where (money != null) && (money.Currency == pIdC)
                        select flow;
            return flowCurrency;
        }
        /// <summary>
        /// Retourne la liste des PaymentType présents sur les CashFlows pour la devise {pIdC}
        /// </summary>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        /// PM 20150709 [21103] New
        public List<string> GetPaymentType(string pIdC)
        {
            IEnumerable<CBDetCashFlows> flowCurrency = CashFlowsByCurrency(pIdC);
            List<string> paymentType = (
                from flow in flowCurrency
                select flow.PaymentType).Distinct().ToList();
            return paymentType;
        }
        /// <summary>
        /// Calcul la somme des flux en contrevaleur
        /// </summary>
        /// <returns></returns>
        /// PM 20150616 [21124] New
        public Money CalcSumMoneyCtrVal()
        {
            Money ret = null;
            if (m_FlowList != null)
            {
                ret = (from flow in m_FlowList
                       where (flow != null) && (flow.CtrValAmount != default)
                       from money in flow.CtrValAmount
                       where (money != null) && (money.Currency != null)
                       group money by money.Currency into moneyCtrVal
                       select new Money(moneyCtrVal.Sum(m => m.Amount.DecValue), moneyCtrVal.Key)).FirstOrDefault();
            }
            return ret;
        }
        #region CalcSumAmount
        /// <summary>
        /// Calcul la somme des flux en devise pIdC ou en contrevaleur
        /// </summary>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] New
        public Money CalcSumAmount(string pIdC, bool pIsCtrVal)
        {
            Money ret = null;
            if (m_FlowList != null)
            {
                ret = (pIsCtrVal ? CalcSumMoneyCtrVal() : CalcSumMoneyIdC(pIdC));
            }
            return ret;
        }
        #endregion CalcSumAmount
        /// <summary>
        /// Arrondi les montants unitaires de chaque flux en fonction des informations de la devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public void RoundAmounts(string pCS, IDbTransaction pDbTransaction, CBCache pCache)
        {
            m_FlowList = CBFlowTools.RoundAmount(pCS, pDbTransaction, pCache, m_FlowList.ToList());
        }
        #endregion
    }
    /// <summary>
    /// Méthodes de manipulation des montants
    /// </summary>
    ///PM 20140908 [20066][20185] New
    public static class CBFlowTools
    {
        #region methods
        #region IsFilledMoneyList
        /// <summary>
        /// True si la liste de Money n'est pas vide et contient des éléments ayant un montant différent de 0
        /// </summary>
        /// <param name="pMoneyList"></param>
        /// <returns></returns>
        public static bool IsFilledMoneyList(List<Money> pMoneyList)
        {
            bool ret = ArrFunc.IsFilled(pMoneyList) && pMoneyList.Exists(money => (money.Amount.DecValue != 0));
            return ret;
        }
        /// <summary>
        /// True si la liste de Money n'est pas vide et contient des éléments ayant un montant différent de 0 pour une devise donnée
        /// </summary>
        /// <param name="pMoneyList"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        public static bool IsFilledMoneyList(List<Money> pMoneyList, string pCurrency)
        {
            bool ret = ArrFunc.IsFilled(pMoneyList) && pMoneyList.Exists(money => (money.Amount.DecValue != 0) && (money.Currency == pCurrency));
            return ret;
        }
        #endregion
        #region MoneyListDistinctCurrency
        /// <summary>
        /// Recherche l'ensemble des différentes Currencies d'une liste de Money
        /// </summary>
        /// <param name="pMoneyList"></param>
        /// <returns></returns>
        public static IEnumerable<string> MoneyListDistinctCurrency(List<Money> pMoneyList)
        {
            IEnumerable<string> ret = new List<string>();
            if (ArrFunc.IsFilled(pMoneyList))
            {
                ret = (from money in pMoneyList
                      where StrFunc.IsFilled(money.Currency)
                      select money.Currency).Distinct();
            }
            return ret;
        }
        #endregion
        #region IsFilledDetCashFlow
        /// <summary>
        /// True si la liste de flux détaillés n'est pas vide et contient des éléments ayant un montant différent de 0 pour une devise donnée
        /// </summary>
        /// <param name="pFlowList"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        public static bool IsFilledDetCashFlow(List<CBDetCashFlows> pFlowList, string pCurrency)
        {
            bool ret = ArrFunc.IsFilled(pFlowList) &&
                    (from flow in pFlowList
                     where (flow != null) && (flow.CurrencyAmount != null)
                     from money in flow.CurrencyAmount
                     select money).ToList().Exists(money => (money.Amount.DecValue != 0) && (money.Currency == pCurrency));
            return ret;
        }
        #endregion
        #region DetCashFlowDistinctCurrency
        /// <summary>
        /// Recherche l'ensemble des différentes Currencies d'une liste de flux
        /// </summary>
        /// <param name="pFlowList"></param>
        /// <returns></returns>
        public static IEnumerable<string> DetCashFlowDistinctCurrency(List<CBDetCashFlows> pFlowList)
        {
            IEnumerable<string> ret = new List<string>();
            if (ArrFunc.IsFilled(pFlowList))
            {
                ret = (from flow in pFlowList
                       where (flow != null) && (flow.CurrencyAmount != null)
                       from money in flow.CurrencyAmount
                       where (money != null) && StrFunc.IsFilled(money.Currency)
                       select money.Currency).Distinct();
            }
            return ret;
        }
        /// <summary>
        /// Retourne l'ensemble des montants (Money) d'une liste de flux
        /// </summary>
        /// <param name="pFlowList"></param>
        /// <returns></returns>
        public static IEnumerable<Money> AllCurrencyAmount(List<CBDetCashFlows> pFlowList)
        {
            IEnumerable<Money> ret = new List<Money>();
            if (ArrFunc.IsFilled(pFlowList))
            {
                ret = from flow in pFlowList
                      where (flow != null) && (flow.CurrencyAmount != null)
                      from money in flow.CurrencyAmount
                      select money;
            }
            return ret;
        }
        #endregion
        #region RoundAmount
        /// <summary>
        /// Arrondi un montant en fonction des informations de sa devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        /// <param name="pAmount"></param>
        /// <returns>Montant arrondi</returns>
        /// PM 20150204 [20772] Added
        /// FI 20150729 [XXXXX] Modify
        // EG 20180205 [23769] Add dbTransaction  
        public static Money RoundAmount(string pCS, IDbTransaction pDbTransaction, CBCache pCache, Money pAmount)
        {
            Money roundedAmount = pAmount;
            // S'il n'est pas indiqué dans le fichier de config de ne pas arrondir les montants dans les CashBalances
            if (Settings.Default.IsRoundedCashBalance)
            {
                if ((pAmount != default) && (pCache != default(CBCache)) && (StrFunc.IsFilled(pAmount.Currency)))
                {
                    CurrencyCashInfo curCashInfo = pCache.GetCurCashInfo(pAmount.Currency);
                    if (curCashInfo == default)
                    {
                        curCashInfo = pCache.AddCurCashInfo(pCS, pDbTransaction, pAmount.Currency);
                    }
                    // FI 20150729 [XXXXX] Mise en place du calcul des arrondis (ceci n'était plus appliqué suite à une dégradation)  
                    EFS_Cash cash = new EFS_Cash(pCS, pDbTransaction, pAmount.Amount.DecValue, curCashInfo);
                    roundedAmount = new Money(cash.AmountRounded, pAmount.Currency);
                }
            }
            return roundedAmount;
        }
        /// <summary>
        /// Arrondi un ensemble de montant en fonction des informations de leur devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        /// <param name="pAmountList"></param>
        /// <returns>Ensemble des montants arrondis</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        public static List<Money> RoundAmount(string pCS, IDbTransaction pDbTransaction, CBCache pCache, List<Money> pAmountList)
        {
            List<Money> roundedAmountList = pAmountList;
            if (pAmountList != default)
            {
                roundedAmountList = (
                    from amount in pAmountList
                    select RoundAmount(pCS, pDbTransaction, pCache, amount)
                    ).ToList();
            }
            return roundedAmountList;
        }
        /// <summary>
        /// Arrondi les montants d'un ensemble de flux en fonction des informations de leur devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        /// <param name="pFlows"></param>
        /// <returns>Ensemble de flux avec leurs montants arrondis</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        public static List<CBDetFlow> RoundAmount(string pCS, IDbTransaction pDbTransaction, CBCache pCache, List<CBDetFlow> pFlows)
        {
            List<CBDetFlow> roundedFlows = pFlows;
            if (pFlows != default(List<CBDetFlow>))
            {
                foreach (CBDetFlow flow in pFlows)
                {
                    flow.CurrencyAmount = RoundAmount(pCS, pDbTransaction, pCache, flow.CurrencyAmount);
                }
            }
            return roundedFlows;
        }
        /// <summary>
        /// Arrondi les montants d'un ensemble de flux en fonction des informations de leur devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        /// <param name="pFlows"></param>
        /// <returns>Ensemble de flux avec leurs montants arrondis</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        public static List<CBDetCashFlows> RoundAmount(string pCS, IDbTransaction pDbTransaction, CBCache pCache, List<CBDetCashFlows> pFlows)
        {
            List<CBDetCashFlows> roundedFlows = pFlows;
            if (pFlows != default(List<CBDetCashFlows>))
            {
                foreach (CBDetCashFlows flow in pFlows)
                {
                    flow.CurrencyAmount = RoundAmount(pCS, pDbTransaction, pCache, flow.CurrencyAmount);
                }
            }
            return roundedFlows;
        }
        /// <summary>
        /// Arrondi les montants d'un ensemble de flux en fonction des informations de leur devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        /// <param name="pFlows"></param>
        /// <returns>Ensemble de flux avec leurs montants arrondis</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        public static List<CBDetDeposit> RoundAmount(string pCS, IDbTransaction pDbTransaction, CBCache pCache, List<CBDetDeposit> pFlows)
        {
            List<CBDetDeposit> roundedFlows = pFlows;
            if (pFlows != default(List<CBDetDeposit>))
            {
                foreach (CBDetDeposit flow in pFlows)
                {
                    flow.CurrencyAmount = RoundAmount(pCS, pDbTransaction, pCache, flow.CurrencyAmount);
                }
            }
            return roundedFlows;
        }
        /// <summary>
        /// Arrondi les montants d'un ensemble de flux en fonction des informations de leur devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCache"></param>
        /// <param name="pFlows"></param>
        /// <returns>Ensemble de flux avec leurs montants arrondis</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        public static List<CBDetCollateral> RoundAmount(string pCS, IDbTransaction pDbTransaction, CBCache pCache, List<CBDetCollateral> pFlows)
        {
            List<CBDetCollateral> roundedFlows = pFlows;
            if (pFlows != default(List<CBDetCollateral>))
            {
                foreach (CBDetCollateral flow in pFlows)
                {
                    flow.CurrencyAmount = RoundAmount(pCS, pDbTransaction, pCache, flow.CurrencyAmount);
                }
            }
            return roundedFlows;
        }
        #endregion
        #region CalcSumByIdC
        /// <summary>
        /// Returne une liste de Money resultant de la somme de l'ensemble des Moneys reçus en paramètre cumulé par devise.
        /// </summary>
        /// <param name="pAmounts">Ensemble de Money</param>
        /// <returns></returns>
        public static List<Money> CalcSumByIdC(IEnumerable<Money> pAmounts)
        {
            List<Money> ret = new List<Money>();
            if (pAmounts != null)
            {
                ret = (
                    from money in pAmounts
                    where (money != null)
                    group money by money.Currency
                        into moneyIdc
                        select new Money(moneyIdc.Sum(m => m.Amount.DecValue), moneyIdc.Key)
                    ).ToList();
            }
            return ret;
        }
        #endregion CalcSumByIdC
        #region CalcSumAmount
        /// <summary>
        /// Calcul la somme des flux en devise pIdC ou en contrevaleur
        /// </summary>
        /// <param name="pFlows"></param>
        /// <param name="pIdC"></param>
        /// <param name="pIsCtrVal"></param>
        /// <returns></returns>
        /// PM 20150616 [21124] New
        public static Money CalcSumAmount(IEnumerable<CBDetCashFlows> pFlows, string pIdC, bool pIsCtrVal)
        {
            Money ret = null;
            if ((pFlows != default(IEnumerable<CBDetCashFlows>)) && (pFlows.Count() > 0))
            {
                CBFlows cbFlows = new CBFlows(pFlows);
                ret = cbFlows.CalcSumAmount(pIdC, pIsCtrVal);
            }
            return ret;
        }
        #endregion CalcSumAmount
        #endregion methods
    }
    /// <summary>
    /// Class de stockage des informations sur un DC ou un Asset
    /// </summary>
    ///PM 20140910 [20066][20185] New
    public class CBAssetInfo
    {
        #region members
        /// <summary>
        /// 
        /// </summary>
        public Cst.UnderlyingAsset AssetCategory;
        /// <summary>
        /// IDDC ou IDASSET
        /// </summary>
        public int OTCmlId;
        /// <summary>
        /// Identifier (symbol)
        /// </summary>
        public string Sym;
        /// <summary>
        /// Marché 
        /// </summary>
        public string Exch;
        #endregion

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        public CBAssetInfo() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetCategory"></param>
        /// <param name="pOTCmlId"></param>
        public CBAssetInfo(Cst.UnderlyingAsset pAssetCategory, int pOTCmlId)
        {
            AssetCategory = pAssetCategory;
            OTCmlId = pOTCmlId;
        }
        #endregion
    }
    /// <summary>
    /// Class de gestion des informations en cache
    /// </summary>
    ///PM 20140910 [20066][20185] New
    public class CBCache
    {
        #region members
        /// <summary>
        /// Dictionnaire permettant de mettre en cache les infos sur les DC ou Asset
        /// </summary>
        private Dictionary<KeyValuePair<Cst.UnderlyingAsset, int>, CBAssetInfo> m_CBAssetCache;
        /// <summary>
        ///  Dictionnaire des informations sur les devises
        /// </summary>
        /// PM 20150204 [20772] Added
        private Dictionary<string, CurrencyCashInfo> m_CurCashInfoCash;
        #endregion
        #region methods
        /// <summary>
        /// Ajoute dans le cache les informations sur un DC ou Asset
        /// si celles-ci n'y figure pas déjà
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAssetCategory">Type d'asset</param>
        /// <param name="pOTCmlId">Id du DC ou Asset à mettre en cache</param>
        /// <returns>Les infos les le DC ou Asset concerné (null si impossible d'ajouter le DC ou Asset)</returns>
        /// FI 20160530 [21885] New
        /// FI 20170130 [22767] Modify
        /// FI 20170116 [21916] Modify
        // EG 20180205 [23769] Add dbTransaction  
        public CBAssetInfo AddContractAssetInfo(string pCS, IDbTransaction pDbTransaction, Cst.UnderlyingAsset pAssetCategory, int pOTCmlId)
        {
            //if (null == m_CBAssetCache)
            //{
            //    m_CBAssetCache = new Dictionary<KeyValuePair<Cst.UnderlyingAsset, int>, CBAssetInfo>();
            //}
            //KeyValuePair<Cst.UnderlyingAsset, int> key = new KeyValuePair<Cst.UnderlyingAsset, int>(pAssetCategory, pOTCmlId);
            //if (false == m_CBAssetCache.TryGetValue(key, out cbAsset))
            CBAssetInfo cbAsset = GetContractAssetInfo(pAssetCategory, pOTCmlId);
            if (null == cbAsset)
            {
                cbAsset = new CBAssetInfo(pAssetCategory, pOTCmlId);
                if (pAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract)
                {
                    AssetTools.LoadDerivativeContratIdentifier(pCS, pDbTransaction, pOTCmlId, out cbAsset.Sym, out cbAsset.Exch);
                }
                else if (pOTCmlId > 0)
                {
                    SQL_AssetBase sqlAsset = AssetTools.NewSQLAsset(pCS, pAssetCategory, pOTCmlId);
                    sqlAsset.DbTransaction = pDbTransaction;
                    if ((null != sqlAsset) && sqlAsset.IsLoaded)
                    {
                        switch (pAssetCategory)
                        {
                            case Cst.UnderlyingAsset.Index:
                                SQL_AssetIndex assetIndex = sqlAsset as SQL_AssetIndex;
                                cbAsset.Sym = assetIndex.AssetSymbol;
                                cbAsset.Exch = assetIndex.Market_FIXML_SecurityExchange;
                                break;
                            case Cst.UnderlyingAsset.EquityAsset:
                                SQL_AssetEquity assetEquity = sqlAsset as SQL_AssetEquity;
                                cbAsset.Sym = assetEquity.AssetSymbol;
                                cbAsset.Exch = assetEquity.Market_FIXML_SecurityExchange;
                                break;
                            case Cst.UnderlyingAsset.FxRateAsset: // FI 20150313 [XXXXX] add case FxRateAsset
                                SQL_AssetFxRate assetfxRate = sqlAsset as SQL_AssetFxRate;
                                cbAsset.Sym = assetfxRate.ShortIdentifier;
                                cbAsset.Exch = assetfxRate.Market_FIXML_SecurityExchange;
                                break;
                            case Cst.UnderlyingAsset.Bond:  // PM 20151106 add case Bond
                                SQL_AssetDebtSecurity assetDebtSecurity = sqlAsset as SQL_AssetDebtSecurity;
                                cbAsset.Sym = assetDebtSecurity.Identifier;
                                cbAsset.Exch = assetDebtSecurity.Market_FIXML_SecurityExchange;
                                break;
                            case Cst.UnderlyingAsset.Cash: // FI 20160530 [21885] Modify
                                SQL_AssetCash assetCash = sqlAsset as SQL_AssetCash;
                                cbAsset.Sym = assetCash.Identifier;
                                cbAsset.Exch = assetCash.Market_FIXML_SecurityExchange;
                                break;
                            case Cst.UnderlyingAsset.Commodity:
                                // FI 20170116 [21916] Load a SQL_AssetCommodityContract
                                if (((SQL_AssetCommodity)sqlAsset).IdCC.HasValue) // FI 20170130 [22767] Chg de SQL_AssetCommodityContract 
                                {
                                    SQL_AssetCommodityContract assetCommodity = new SQL_AssetCommodityContract(pCS, sqlAsset.Id)
                                    {
                                        DbTransaction = pDbTransaction
                                    };
                                    assetCommodity.LoadTable(new string[] { "IDASSET", "CONTRACTSYMBOL"});
                                    
                                    cbAsset.Sym = assetCommodity.CommodityContract_ContractSymbol;
                                    cbAsset.Exch = assetCommodity.Market_FIXML_SecurityExchange;
                                }
                                else
                                {
#if DEBUG
                                    throw new NotImplementedException(StrFunc.AppendFormat("SQL_AssetCommodity:{0} is not implemented", sqlAsset.GetType().ToString().ToString()));
#endif
                                }

                                break;

                            default:
                                cbAsset = null;
                                // FI 20150313 [XXXXX] NotImplementedException en debug
                                // EG 20160404 Migration vs2013
#if DEBUG
                                throw new NotImplementedException(StrFunc.AppendFormat("asset category:{0} is not implemented", pAssetCategory.ToString()));
#else
                                break;
#endif
                        }
                    }
                }
                if (null != cbAsset)
                    AddContractAssetInfo(cbAsset);
            }
            return cbAsset;
        }
        /// <summary>
        /// Lecture des information sur un DC ou Asset si celles-ci se trouve en cache
        /// </summary>
        /// <param name="pAssetCategory"></param>
        /// <param name="pOTCmlId"></param>
        /// <returns>null si non présent dans le cache</returns>
        public CBAssetInfo GetContractAssetInfo(Cst.UnderlyingAsset pAssetCategory, int pOTCmlId)
        {
            CBAssetInfo cbAsset = null;
            if (null == m_CBAssetCache)
            {
                m_CBAssetCache = new Dictionary<KeyValuePair<Cst.UnderlyingAsset, int>, CBAssetInfo>();
            }
            else
            {
                KeyValuePair<Cst.UnderlyingAsset, int> key = new KeyValuePair<Cst.UnderlyingAsset, int>(pAssetCategory, pOTCmlId);
                m_CBAssetCache.TryGetValue(key, out cbAsset);
            }
            return cbAsset;
        }
        private void AddContractAssetInfo(CBAssetInfo pCbAsset)
        {
            if (null != m_CBAssetCache)
            {
                KeyValuePair<Cst.UnderlyingAsset, int> key = new KeyValuePair<Cst.UnderlyingAsset, int>(pCbAsset.AssetCategory, pCbAsset.OTCmlId);
                if (false == m_CBAssetCache.ContainsKey(key))
                    m_CBAssetCache.Add(key, pCbAsset);
            }
        }
        /// <summary>
        /// Charge dans le cache les infos de tous les DC/Asset d'un ensemble de flux
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCashFlows"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public void AddAssetInfoFromCashFlows(string pCS, IDbTransaction pDbTransaction, IEnumerable<CBDetCashFlows> pCashFlows)
        {
            var flowById = from flow in pCashFlows
                           //PM 20150324 [POC] AssetCategory devient nullable : ne prendre que les non null
                           where flow.AssetCategorySpecified
                           group flow by new { flow.AssetCategory, OTCmlId = ((flow.IdDC > 0) ? flow.IdDC : flow.IdAsset) }
                           into groupedFlow
                           select groupedFlow;
            //
            foreach (var id in flowById)
            {
                AddContractAssetInfo(pCS, pDbTransaction, id.Key.AssetCategory.Value, id.Key.OTCmlId);
            }
        }
        /// <summary>
        /// Ajoute dans le cache les informations sur une devise
        /// si celles-ci n'y figure pas déjà
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrency">Devise</param>
        /// <returns>Les infos sur la devise</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        public CurrencyCashInfo AddCurCashInfo(string pCS, IDbTransaction pDbTransaction, string pCurrency)
        {
            CurrencyCashInfo curCashInfo = GetCurCashInfo(pCurrency);
            if (null == curCashInfo)
            {
                curCashInfo = new CurrencyCashInfo(pCS, pDbTransaction, pCurrency);
                AddCurCashInfo(curCashInfo);
            }
            return curCashInfo;
        }
        /// <summary>
        /// Lecture des informations sur une devise si celles-ci se trouve en cache
        /// </summary>
        /// <param name="pCurrency">Devise</param>
        /// <returns>null si non présent dans le cache</returns>
        // PM 20150204 [20772] Added
        public CurrencyCashInfo GetCurCashInfo(string pCurrency)
        {
            CurrencyCashInfo curCashInfo = null;
            if (null == m_CurCashInfoCash)
                m_CurCashInfoCash = new Dictionary<string, CurrencyCashInfo>();
            else
                m_CurCashInfoCash.TryGetValue(pCurrency, out curCashInfo);
            return curCashInfo;
        }

        public void AddCurCashInfo(CurrencyCashInfo pCurrencyCashInfo)
        {
            if (null != m_CurCashInfoCash)
            {
                if (false == m_CurCashInfoCash.ContainsKey(pCurrencyCashInfo.Currency))
                    m_CurCashInfoCash.Add(pCurrencyCashInfo.Currency, pCurrencyCashInfo);
            }
        }
        /// <summary>
        /// Charge dans le cache les infos de toutes les devises d'un ensemble de flux
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCashFlows"></param>
        /// <returns>Le nombre de devise contenu dans l'ensemble de flux</returns>
        // PM 20150204 [20772] Added
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20190308 Upd Test money.Currency renseigné
        public int AddCurCashInfoFromCashFlows(string pCS, IDbTransaction pDbTransaction, IEnumerable<CBDetCashFlows> pCashFlows)
        {
            int count = 0;
            if (pCashFlows != default(IEnumerable<CBDetCashFlows>))
            {
                IEnumerable<string> allCurrencies = (
                    from flow in pCashFlows
                    where (flow != null) && (flow.CurrencyAmount != null)
                    from money in flow.CurrencyAmount
                    where StrFunc.IsFilled(money.Currency)
                    select money.Currency).Distinct();
                //
                foreach (string currency in allCurrencies)
                {
                    AddCurCashInfo(pCS, pDbTransaction, currency);
                }
                count = allCurrencies.Count();
            }
            return count;
        }

        /// <summary>
        /// Charge dans le cache les infos de tous les Assets d'un ensemble de collateral
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCollateral"></param>
        /// FI 20160530 [21885] Add
        // EG 20180205 [23769] Add dbTransaction  
        public void AddAssetInfoFromCollateral(string pCS, IDbTransaction pDbTransaction, IEnumerable<CBDetCollateral> pCollateral)
        {
            var flowById = from flow in pCollateral
                           where flow.AssetCategorySpecified
                           group flow by new
                           {
                               AssetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), flow.AssetCategory),
                               OTCmlId = flow.IdAsset
                           }
                               into groupedFlow
                               select groupedFlow;

            foreach (var id in flowById)
            {
                AddContractAssetInfo(pCS, pDbTransaction, id.Key.AssetCategory, id.Key.OTCmlId);
            }
        }



        #endregion
    }
}

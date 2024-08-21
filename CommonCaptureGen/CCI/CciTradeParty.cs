#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.MiFIDII_Extended;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Représente un party (Contrepartie ou broker)
    /// </summary>
    /// FI 20170928 [23452] Modify (Hérite de CciPartyBase)
    /// FI 20200504 [XXXXX] suppression implementation de IContainerCci
    public class CciTradeParty : CciPartyBase, IContainerCciFactory, IContainerCciSpecified, ICciPresentation
    {
        #region Enums
        /// <summary>
        /// Liste des éléments gérés
        /// </summary>
        /// FI 20140204 [19564] add partyTradeIdentifier_UTI 
        /// FI 20170928 [23452] Add CciGroupAttribute attributs
        // EG 20171113 [23509] Add FacilityHaschanged
        public enum CciEnum
        {
            side,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            actor,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            book,

            localClassDerv,
            iasClassDerv,
            hedgeClassDerv,
            hedgeFolder,
            hedgeFactor,
            fxClass,
            localClassNDrv,
            iasClassNDrv,
            hedgeClassNDrv,

            frontId,
            folder,
            [CciGroupAttribute(name = "NCM")]
            ISNCMINI,
            [CciGroupAttribute(name = "NCM")]
            ISNCMINT,
            [CciGroupAttribute(name = "NCM")]
            ISNCMFIN,
            initiatorReactor,
            /// <summary>
            /// Emetteur associé à l'identifiant du trade (this cci only exists on the screen) 
            /// </summary>
            [CciGroupAttribute(name = "UTI")]
            partyTradeIdentifier_UTI_issuer,
            /// <summary>
            /// identifiant du trade (this cci only exists on the screen)
            /// </summary>
            [CciGroupAttribute(name = "UTI")]
            partyTradeIdentifier_UTI_identifier,
            /// <summary>
            /// Représente la valeur stockée dans Spheres® [issuer+identifier] 
            /// </summary>
            [CciGroupAttribute(name = "UTI")]
            partyTradeIdentifier_UTI_value,


            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_executionWithinFirm,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_investmentDecisionWithinFirm,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_tradingCapacity,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_tradingWaiver1,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_shortSale,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_otcClassification1,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_isCommodityHedge,

            // FI 20170928 [23452] Add 
            [CciGroupAttribute(name = "MiFIR")]
            partyTradeInformation_isSecuritiesFinancing,
        }

        /// <summary>
        /// Type de party
        /// </summary>
        public enum PartyType
        {
            /// <summary>
            /// Broker au sein d'un trade
            /// </summary>
            broker,
            /// <summary>
            /// Contrepartie d'un trade 
            /// </summary>
            party,
        }

        /// <summary>
        /// Composants de l'UTI
        /// </summary>
        //FI 20140204 [19564] add UTIElement enum
        private enum UTIElement
        {
            /// <summary>
            /// Emetteur
            /// </summary>
            issuer,
            /// <summary>
            /// identifiant 
            /// </summary>
            tradeId
        }
        #endregion Enums

        #region Members
        private readonly string prefixParent;
        /// <summary>
        /// Représente le type de party
        /// </summary>
        public PartyType partyType;
        /// <summary>
        /// Représente les traders rattachés à la party
        /// </summary>
        public CciTrader[] cciTrader;
        /// <summary>
        /// Représente les sales rattachés à la party
        /// </summary>
        public CciTrader[] cciSales;
        /// <summary>
        /// Représente les brokers rattachés à la party
        /// </summary>
        public CciTradeParty[] cciBroker;
        /// <summary>
        /// Représente la partie à laquelle est rattaché le broker (alimenté uniquement cciTradeParty PartyType=broker) 
        /// </summary>
        public CciTradeParty cciBrokerParent;
        //
        private bool _isInitTradeSide;
        private bool _isInitFromClearingTemplate;
        private bool _isInitFromPartyTemplate;
        // FI 20130730 [18847] mise en commentaire de _isActorSYSTEMAuthorized 
        //private bool _isActorSYSTEMAuthorized;
        //
        private RoleActor[] _additionnalRole;
        #endregion Members

        #region Accessors

        /// <summary>
        /// Obtient true si l'instance gère un broker
        /// </summary>
        public bool IsPartyBroker
        {
            get { return (PartyType.broker == partyType); }
        }

        /// <summary>
        /// Obtient true si l'instance gère une contrepartie de trade
        /// </summary>
        public bool IsParty
        {
            get { return (PartyType.party == partyType); }
        }

        /// Obtient true s'il s'agit de la Party de gauche d'un trade administratif
        /// </summary>
        /// EG 20141020 [20442] New
        public bool IsInvoiceBeneficiary
        {
            get
            {
                bool ret = (partyType == PartyType.party);
                if (ret)
                {
                    ret = (cciTrade.TradeCommonInput.SQLProduct.IsAdministrativeProduct && number == 1);
                }
                return ret;
            }
        }

        /// <summary>
        /// Retourne true si Spheres® doit alimenter les brokers rattachés à la partie avec la hierarchie des entités
        /// </summary>
        /// FI 20240528 [WI946] Add
        private bool IsSetBrokerWithEntity()
        {
            bool ret = false;
            switch (partyType)
            {
                case PartyType.party:
                    //number=1 sur la 1ère partie (en principe à gauche sur un écran standard), il s'agit de la partie qui représente le Dealer sur une Allocation. 
                    if (cciTrade.TradeCommonInput.IsAllocation)
                        ret = (number == 1);
                    else
                        ret = cciTrade.TradeCommonInput.IsDeal;
                    break;
                case PartyType.broker:
                    ret = false;
                    break;
                default:
                    throw new InvalidOperationException($"{partyType} is not implemented");
            }

            return ret;
        }

        /// <summary>
        /// Obtient true s'il s'agit du Clearer sur une Allocation (Party de droite sur un écran Allocation standard) 
        /// </summary>
        /// PL 20231218 New name (instead of IsAllocationAndRightParty) and refactoring
        /// FI 20240124 [XXXXX] Nouvelle écriture
        public bool IsAllocationAndClearerParty
        {
            get
            {
                bool ret = false;
                switch (partyType)
                {
                    case PartyType.party:
                        //number=2 sur la 2nde partie (en principe à droite sur un écran standard), il s'agit de la partie qui représente le Clearer (Clearing House/Clearing Broker) sur une Allocation. 
                        ret = (cciTrade.TradeCommonInput.IsTradeFoundAndAllocation && number == 2);
                        break;
                    case PartyType.broker:
                        ret = false;
                        break;
                    default:
                        throw new InvalidOperationException($"{partyType} is not implemented");
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient le nombre de traders
        /// </summary>
        public int TraderLength
        {
            get { return ArrFunc.IsFilled(cciTrader) ? cciTrader.Length : 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int SalesLength
        {
            get { return ArrFunc.IsFilled(cciSales) ? cciSales.Length : 0; }
        }

        /// <summary>
        /// Obtient le nombre de traders
        /// </summary>
        public int BrokerLength
        {
            get { return ArrFunc.IsFilled(cciBroker) ? cciBroker.Length : 0; }
        }

        /// <summary>
        /// Obtient true si l'acteur est broker d'une contrepartie
        /// </summary>
        public bool IsBrokerOfParty
        {
            get { return (null != cciBrokerParent); }
        }

        /// <summary>
        /// Obtient ou définit l'indicateur qui active l'exploitation de PARTYTEMPLATE 
        /// </summary>
        public bool IsInitFromPartyTemplate
        {
            get
            {
                bool ret = _isInitFromPartyTemplate;
                if (ret)
                {
                    ret = (partyType == PartyType.party);
                    ret &= (false == cciTrade.DataDocument.CurrentProduct.IsDebtSecurity);
                    ret &= (false == cciTrade.DataDocument.CurrentProduct.IsMarginRequirement);
                    //
                    //Si allocation les alimenations depuis PARTYTEMPLATE ne sont pas effectuées que sur le DO
                    if (cciTrade.TradeCommonInput.IsAllocation)
                        ret &= (number == 1);
                }
                return ret;
            }
            set
            {
                _isInitFromPartyTemplate = value;
            }
        }

        /// <summary>
        /// Obtient ou définit l'indicateur qui active l'exploitation de CLEARINGTEMPLATE 
        /// </summary>
        public bool IsInitFromClearingTemplate
        {
            get
            {
                bool ret = _isInitFromClearingTemplate;

                if (ret)
                {
                    //les alimentations depuis CLEARINGTEMPLATE sont effectuées uniquement sur les  allocations côté chambre   
                    ret = (partyType == PartyType.party);
                    ret &= cciTrade.TradeCommonInput.IsAllocation;
                    ret &= (number == 2);
                }

                return ret;
            }
            set
            {
                _isInitFromClearingTemplate = value;
            }
        }

        /// <summary>
        /// Obtient ou définit l'alimentation de tradeSide
        /// </summary>
        public bool IsInitTradeSide
        {
            get
            {
                bool ret = _isInitTradeSide;
                if (ret)
                {
                    ret &= (false == cciTrade.DataDocument.CurrentProduct.IsDebtSecurity);
                    ret &= (false == cciTrade.DataDocument.CurrentProduct.IsMarginRequirement);
                }
                return ret;
            }
            set
            {
                _isInitTradeSide = value;
            }
        }

        /// <summary>
        /// Obtient ou définit si Acteur "SYSTEM" autorisé 
        /// <para>
        /// Exemples:
        /// <para>- sur un titre (produit debtSecurity), SYSTEM est la contrepartie du trade</para>
        /// <para>- sur un trade "Incomplet", si le Book n'existe pas, SYSTEM est utilisé comme contrepartie du trade</para>
        /// </para>
        /// </summary>
        public bool IsActorSYSTEMAuthorized
        {
            get
            {
                //bool ret = _isActorSYSTEMAuthorized;
                Boolean ret = true;
                //
                if (ret)
                {
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // L'acteur SYSTEM n'est autorisé que sur les titres et les trades en statut activation "Missing"
                    ret &= (cciTrade.DataDocument.CurrentProduct.IsDebtSecurity
                        // FI 20130730 [18847] usage de IsStActivation_Missing
                        //|| cciTrade.TradeCommonInput.TradeStatus.IsCurrentStActivation_Missing);
                        || cciTrade.TradeCommonInput.TradeStatus.IsStActivation_Missing);
                }
                //
                return ret;
            }
            // FI 20130730 [18847] mise en commentaire de la méthode set
            //set
            //{
            //    _isActorSYSTEMAuthorized = value;
            //}
        }

        /// <summary>
        /// Obtient ou définit des rôles supplémentaires nécessaires à la partie 
        /// <para>Ex sur les futures la partie 2 peut être css</para>
        /// </summary>
        public RoleActor[] AdditionnalRole
        {
            get { return _additionnalRole; }
            set { _additionnalRole = value; }
        }

        /// <summary>
        ///  Obtient true s'il doit exister au minimum 1 broker (Gestionnaire(s) ou autres intervenants)
        ///  <para>  </para>
        /// </summary>
        /// FI 20170906 [23401] Add
        public Boolean IsAddCciSytemForBroker
        {
            get
            {
                return (this.IsParty && (false == cciTrade.CurrentTrade.Product.ProductBase.IsDebtSecurity) &&
                            ((this.IsInitFromClearingTemplate) || ((false == this.IsAllocationAndClearerParty))));
            }
        }


        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="plNumber"></param>
        /// <param name="pType"></param>
        /// <param name="pPrefixParent"></param>
        /// FI 20170928 [23452] Modify (appel à base)
        public CciTradeParty(CciTradeCommonBase pTrade, int plNumber, PartyType pType, string pPrefixParent) :
            base(pTrade, plNumber, pPrefixParent + pType.ToString())
        {

            partyType = pType;
            prefixParent = pPrefixParent;

            //Activation de l'alimenation de tradeSide                   
            _isInitTradeSide = true;

            //Activation des initialisations depuis ClearingTemplate et partytTemplate
            _isInitFromPartyTemplate = true;
            _isInitFromClearingTemplate = true;

        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140204 [19564] Add partyTradeIdentifier_UTI_value
        /// FI 20170928 [23452] Add
        /// FI 20171201 [XXXXX] Modify
        public void AddCciSystem()
        {
            //
            // RD 20091228 [16809] Confirmation indicators for each party
            // RD 20100831 [] Si les Cci ne sont pas présents, donc par défaut ils seront à FALSE
            if ((PartyType.party == partyType))
            {
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.side), true, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(CcisBase, Cst.CHK + CciClientId(CciEnum.ISNCMINI), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(CcisBase, Cst.CHK + CciClientId(CciEnum.ISNCMINT), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(CcisBase, Cst.CHK + CciClientId(CciEnum.ISNCMFIN), false, TypeData.TypeDataEnum.@bool);
                // FI 20140204 [19564] add partyTradeIdentifier_UTI_XXXXXX
                CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.partyTradeIdentifier_UTI_value), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.partyTradeIdentifier_UTI_issuer), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.partyTradeIdentifier_UTI_identifier), false, TypeData.TypeDataEnum.@string);

                // FI 20170928 [23452] 
                IEnumerable<CciEnum> cciEnum = CciTools.GetCciEnum<CciEnum>("MiFIR");
                foreach (CciEnum item in cciEnum)
                    CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(item), false, TypeData.TypeDataEnum.@string);

                // FI 20171201 [XXXXX] code déplacé ici
                // FI 20170116 [21916] cas où les ccis acteur/book côté clearing ne seraient pas nécessairement présents 
                // Ceci est possible en mode import (ccis côté Clearing absents puisque inconnus)
                // Ceci est impossible via la saisie où tous les écrans ALLOCs possèdent le côté clearing
                // Si cela se produit Spheres® ajoute les ccis System suivant pour que l'alimentation de ces données puisse être effectuée via CLEARINGTEMPLATE
                if (IsInitFromClearingTemplate)
                {
                    CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciTradeParty.CciEnum.actor), true, TypeData.TypeDataEnum.@string); // Obligatoire
                    CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciTradeParty.CciEnum.book), false, TypeData.TypeDataEnum.@string);// Optionel
                    if (ArrFunc.Count(cciBroker) > 0)
                    {
                        CciTools.AddCciSystem(CcisBase, Cst.TXT + cciBroker[0].CciClientId(CciTradeParty.CciEnum.actor), false, TypeData.TypeDataEnum.@string); // Optionel
                        CciTools.AddCciSystem(CcisBase, Cst.TXT + cciBroker[0].CciClientId(CciTradeParty.CciEnum.book), false, TypeData.TypeDataEnum.@string); // Optionel
                    }
                }

                // FI 20171201 [XXXXX] code déplacé ici
                // FI 20170718 [23326] Modify
                // Ajout systématique d'un acteur gestionnaire 
                // Notamment nécessaire dans le cas suivant 
                //      Importation de trade, post-parsing :  
                //                 BuyerOrSellerNegociationBroker (BSNB) n'existe pas, 
                //                 BuyerOrSellerNegociationBrokerTrader (BSNBTR) existe
                // Le gestionnaire sera alimenté lors du dump du book=> son cci est généré ici 

                if (IsAddCciSytemForBroker && ArrFunc.Count(cciBroker) > 0)
                    CciTools.AddCciSystem(CcisBase, Cst.TXT + cciBroker[0].CciClientId(CciTradeParty.CciEnum.actor), false, TypeData.TypeDataEnum.@string); // Optionel
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRemoveAll"></param>
        /// FI 20170928 [23452] Modify 
        public override void CleanUp()
        {
            if (ArrFunc.IsFilled(cciTrader))
            {
                for (int i = 0; i < cciTrader.Length; i++)
                    cciTrader[i].CleanUp();
            }

            if (ArrFunc.IsFilled(cciSales))
            {
                for (int i = 0; i < cciSales.Length; i++)
                    cciSales[i].CleanUp();
            }

            if (ArrFunc.IsFilled(cciBroker))
            {
                for (int i = 0; i < cciBroker.Length; i++)
                    cciBroker[i].CleanUp();
            }

            // FI 20170928 [23452] Appel à base
            base.CleanUp();
        }
        /* FI 20200421 [XXXXX] Mise en commentaire
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140204 [19564] Gestion UTI
        /// EG 20170822 [23342] Add executionDateTime 
        /// EG 20171016 [23452] Upd
        public void Dump_ToDocument()
        {
            //string partyReferenceHref;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    IPartyTradeInformation partyTradeInformation = GetPartyTradeInformation();

                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.side:
                            #region side
                            //Notion inexistante en FpML
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion side
                            break;
                        // RD 20091228 [16809] Confirmation indicators for each party
                        case CciEnum.ISNCMINI:
                        case CciEnum.ISNCMINT:
                        case CciEnum.ISNCMFIN:
                            //Notion inexistante en FpML
                            if (PartyType.party == partyType)
                                cciTrade.cciTradeNotification.Dump_ToDocument(number - 1, keyEnum, data);
                            break;
                        case CciEnum.actor:
                            //FI 20120523 [recette 2.6SP2]
                            bool isIntercept = IntercepActor(cci, data);
                            if (false == isIntercept)
                                DumpParty_ToDocument(cci, data);

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;

                            break;
                        case CciEnum.book:
                            DumpBook_ToDocument(cci, data);
                            // FI 20180608 [XXXXX] Recette v7.2 
                            // =>  DumpTraderAndSale uniquement s'il n'y a pas d'erreur 
                            if (isInitFromPartyTemplate && (false == cci.HasError))
                            {
                                //FI 20081124 Initialiation from PARTYTEMPLATE 
                                //FI 20141119 [20505] Recherche d'un trader/des sales 
                                //Boolean isOk = CaptureTools.DumpTrader_ToDocument_FromBook(cciTrade.CSCacheOn, cciTrade.TradeCommonInput, GetPartyId());
                                Boolean isOk = CaptureTools.DumpTraderAndSales_ToDocument_FromBook(cciTrade.CSCacheOn, cciTrade.TradeCommonInput, GetPartyId());
                                if (isOk)
                                    ccis.IsToSynchronizeWithDocument = true;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.localClassDerv:
                            DumpLocalClassDerv_ToDocument(data);
                            break;
                        case CciEnum.localClassNDrv:
                            DumpLocalClassNDrv_ToDocument(data);
                            break;
                        case CciEnum.iasClassDerv:
                            DumpIASClassDerv_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.iasClassNDrv:
                            DumpIASClassNDrv_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.fxClass:
                            DumpFxClass_ToDocument(data);
                            break;
                        case CciEnum.hedgeClassDerv:
                            DumpHedgeClassDerv_ToDocument(data);
                            break;
                        case CciEnum.hedgeClassNDrv:
                            DumpHedgeClassNDrv_ToDocument(data);
                            break;
                        case CciEnum.hedgeFolder:
                            DumpHedgeFolder_ToDocument(cci, data);
                            break;
                        case CciEnum.hedgeFactor:
                            DumpHedgeFactor_ToDocument(cci, data);
                            break;
                        case CciEnum.frontId:
                            DumpFrontId_ToDocument(cci);
                            break;
                        case CciEnum.folder:
                            DumpFolderId_ToDocument(cci, data);
                            break;
                        case CciEnum.initiatorReactor:
                            if (partyType == PartyType.party)
                            {
                                DumpInitiatorReactor_ToDocument(cci, data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            }
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_issuer:
                        case CciEnum.partyTradeIdentifier_UTI_identifier:
                            //En modification sans génération des évènemenrs Spheres® ne fait pas appel aux méthode ProcessInitialize
                            //Petit cas particulier ici puisque partyTradeIdentifier_UTI_value est un champ calculé
                            if (Cst.Capture.IsModeUpdatePostEvts(ccis.CaptureMode))
                            {
                                SynchroCciUTI(cci);
                                DumpUTI_ToDocument(ccis[CciClientId(CciEnum.partyTradeIdentifier_UTI_value)]);
                            }
                            else
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_value:
                            DumpUTI_ToDocument(cci);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        case CciEnum.partyTradeInformation_executionWithinFirm:
                            Dump_ExecutionOrInvestmentWithinFirm_ToDocument(cci, data, RoleActor.EXECUTION);
                            break;
                        case CciEnum.partyTradeInformation_investmentDecisionWithinFirm:
                            Dump_ExecutionOrInvestmentWithinFirm_ToDocument(cci, data, RoleActor.INVESTDECISION);
                            break;
                        case CciEnum.partyTradeInformation_tradingCapacity:
                            if (null != partyTradeInformation)
                            {
                                SchemeData scheme = new SchemeData() { value = data, scheme = "http://www.fpml.org/coding-scheme/esma-mifir-trading-capacity" };
                                partyTradeInformation.category = new IScheme[] { scheme };
                                partyTradeInformation.categorySpecified = IsFilledScheme(partyTradeInformation.category);
                            }
                            break;
                        case CciEnum.partyTradeInformation_tradingWaiver1:
                            if (null != partyTradeInformation)
                            {
                                if (ArrFunc.IsEmpty(partyTradeInformation.tradingWaiver))
                                {
                                    partyTradeInformation.tradingWaiver = new IScheme[] { 
                                        cciTrade.DataDocument.currentProduct.productBase.CreateTradingWaiver() };
                                }
                                partyTradeInformation.tradingWaiver[0].Value = data;
                                partyTradeInformation.tradingWaiverSpecified = IsFilledScheme(partyTradeInformation.tradingWaiver);

                                //SchemeData scheme = new SchemeData() { value = data, scheme = "http://www.fpml.org/coding-scheme/esma-mifir-trading-waiver" };
                                //partyTradeInformation.tradingWaiver = new IScheme[] { scheme };
                                //partyTradeInformation.tradingWaiverSpecified = IsFilledScheme(partyTradeInformation.tradingWaiver);
                            }
                            break;

                        case CciEnum.partyTradeInformation_shortSale:
                            if (null != partyTradeInformation)
                            {
                                partyTradeInformation.shortSale.Value = data;
                                partyTradeInformation.shortSaleSpecified = StrFunc.IsFilled(data);
                            }
                            break;
                        case CciEnum.partyTradeInformation_otcClassification1:
                            if (null != partyTradeInformation)
                            {
                                if (ArrFunc.IsEmpty(partyTradeInformation.otcClassification))
                                {
                                    partyTradeInformation.otcClassification = new IScheme[] { 
                                        cciTrade.DataDocument.currentProduct.productBase.CreateOtcClassification() };
                                }
                                partyTradeInformation.otcClassification[0].Value = data;
                                partyTradeInformation.otcClassificationSpecified = IsFilledScheme(partyTradeInformation.otcClassification);

                                //SchemeData scheme = new SchemeData() { value = data, scheme = "http://www.fpml.org/coding-scheme/esma-mifir-otc-classification" };
                                //partyTradeInformation.otcClassification = new IScheme[] { scheme };
                                //partyTradeInformation.otcClassificationSpecified = IsFilledScheme(partyTradeInformation.otcClassification);

                            }
                            break;
                        case CciEnum.partyTradeInformation_isCommodityHedge:
                            if (null != partyTradeInformation)
                            {
                                partyTradeInformation.isCommodityHedgeSpecified = StrFunc.IsFilled(data);
                                if (partyTradeInformation.isCommodityHedgeSpecified)
                                    partyTradeInformation.isCommodityHedge = BoolFunc.IsTrue(data);
                            }
                            break;
                        case CciEnum.partyTradeInformation_isSecuritiesFinancing:
                            if (null != partyTradeInformation)
                            {
                                partyTradeInformation.isSecuritiesFinancingSpecified = StrFunc.IsFilled(data);
                                if (partyTradeInformation.isSecuritiesFinancingSpecified)
                                    partyTradeInformation.isSecuritiesFinancing = BoolFunc.IsTrue(data);
                            }
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            for (int i = 0; i < TraderLength; i++)
                cciTrader[i].Dump_ToDocument();
            for (int i = 0; i < SalesLength; i++)
                cciSales[i].Dump_ToDocument();
            //
            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].Dump_ToDocument();
            //
            IPartyTradeInformation partyTradeInfo = GetPartyTradeInformation();
            if (null != partyTradeInfo)
            {
                partyTradeInfo.traderSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.traderSpecified, cciTrader);
                partyTradeInfo.salesSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.salesSpecified, cciSales);
                partyTradeInfo.brokerPartyReferenceSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.brokerPartyReferenceSpecified, cciBroker);
            }

        }
        */
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {

            // 
            if (partyType == PartyType.party)
            {
                if (null != Cci(CciEnum.initiatorReactor) && (null == cciTrade.CurrentTrade.TradeIntention))
                    cciTrade.CurrentTrade.TradeIntention = cciTrade.DataDocument.CurrentProduct.ProductBase.CreateTradeIntention();
            }
            //           
            if (null != cciTrade.CurrentTrade.TradeIntention)
            {
                TradeIntentionContainer tradeIntention = new TradeIntentionContainer(cciTrade.CurrentTrade.TradeIntention);
                tradeIntention.ClearInitiator();
            }
            //
            InitializeTrader_FromCci();
            InitializeSales_FromCci();
            InitializeBroker_FromCci();
            //
            if (ArrFunc.IsFilled(cciBroker))
            {
                for (int i = 0; i < cciBroker.Length; i++)
                    cciBroker[i].Initialize_FromCci();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140204 [19564] Gestion UTI
        /// EG 20141020 [20442]
        /// FI 20170214 [23629] Modify
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    SQL_Table sql_Table = null;
                    IParty party = null;
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Ajouter la Party si le Trade est en statut Missing
                    IPartyTradeIdentifier partyTradeIdentifier = cciTrade.DataDocument.GetPartyTradeIdentifier(GetPartyId(Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing));
                    IPartyTradeInformation partyTradeInformation = cciTrade.DataDocument.GetPartyTradeInformation(GetPartyId(Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing));
                    string partyId = string.Empty;
                    string data = string.Empty;
                    bool isSetting = true;
                    bool isToValidate = false;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.side:
                            #region Side
                            //Notion inexistante en FpML
                            #endregion Side
                            break;
                        // RD 20091228 [16809] Confirmation indicators for each party
                        case CciEnum.ISNCMINI:
                        case CciEnum.ISNCMINT:
                        case CciEnum.ISNCMFIN:
                            #region ISNCMINI/ISNCMINT/ISNCMFIN
                            //Notion inexistante en FpML
                            //Notion inexistante en FpML
                            if (PartyType.party == partyType)
                                data = cciTrade.cciTradeNotification.Initialize_FromDocument(number - 1, keyEnum);
                            #endregion ISNCMINI
                            break;
                        case CciEnum.actor:
                            #region actor
                            if (IsPartyBroker)
                            {
                                #region Broker
                                if (IsBrokerOfParty)
                                {
                                    partyId = String.Empty;
                                    IPartyTradeInformation partyTradeInfo = cciBrokerParent.GetPartyTradeInformation();
                                    if ((null != partyTradeInfo) &&
                                        ArrFunc.IsFilled(partyTradeInfo.BrokerPartyReference) &&
                                        (partyTradeInfo.BrokerPartyReference.Count() >= number))
                                        partyId = partyTradeInfo.BrokerPartyReference[number - 1].HRef;
                                }
                                else
                                {
                                    if (cciTrade.CurrentTrade.BrokerPartyReferenceSpecified)
                                        partyId = cciTrade.CurrentTrade.BrokerPartyReference[number - 1].HRef;
                                }
                                if (StrFunc.IsFilled(partyId))
                                {
                                    party = cciTrade.DataDocument.GetParty(partyId);
                                    if (null != party)
                                    {
                                        SQL_Actor sql_Actor = new SQL_Actor(cciTrade.CSCacheOn, party.PartyId);
                                        // 
                                        // RD 20120216 [17322]
                                        // Pre-proposition de l'entitié du book, 
                                        // et de toutes les Entités parentes ( même si elles ne sont pas Broker)
                                        sql_Actor.SetRoleRange(GetRole());
                                        if (sql_Actor.IsLoaded)
                                        {
                                            data = sql_Actor.Identifier;
                                            sql_Table = (SQL_Table)sql_Actor;
                                        }
                                        else
                                        {
                                            data = party.PartyId;
                                        }
                                        // FI 20131205 [19304] Valorisation de isToValidate à true si le broker est inconnu
                                        // Cela permet d'afficher le broker en rouge en consultation, 
                                        isToValidate = (party.OTCmlId == 0);
                                        // FI 20131219 [19334] En mode saisie validation pour afficher une erreur lorsque l'acteur est désactivé.
                                        // RD 20200921 [25246] Ne pas écraser la valeur isToValidate dans l'instruction suivante
                                        if (Cst.Capture.IsModeInput(CcisBase.CaptureMode))
                                            isToValidate = isToValidate || (false == sql_Actor.IsEnabled);
                                    }
                                }
                                #endregion Broker
                            }
                            else if (IsParty)
                            {
                                // Affichage des parties ds l'ordre définie ds currentFpMLDataDocReader.party[]
                                // Test sur lastValue Pour gérer le cas où Ds le DOC il y a EntityOfUSer (Ds ce cas newValue = "XXX" et lastValue =EntityOfUSer 
                                #region Party
                                // Recherche des Ccis qui contiennent le payer et le Receiver
                                CustomCaptureInfo cciPartyPay = CcisBase[cciTrade.CciClientIdPayer];
                                CustomCaptureInfo cciPartyRec = CcisBase[cciTrade.CciClientIdReceiver];
                                //string lastValue = string.Empty;
                                //
                                IParty tradeparty = null;
                                int num = 0;
                                for (int i = 0; i < ArrFunc.Count(cciTrade.DataDocument.Party); i++)
                                {
                                    // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                                    if (
                                        (cciTrade.DataDocument.Party[i].Id == XMLTools.GetXmlId(cciPartyPay.LastValue)) ||
                                        (cciTrade.DataDocument.Party[i].Id == XMLTools.GetXmlId(cciPartyRec.LastValue))
                                        )
                                    {
                                        num++;
                                        if (num == number)
                                        {
                                            tradeparty = cciTrade.DataDocument.Party[i];
                                            if (null != tradeparty)
                                                break;
                                        }
                                    }
                                }
                                //Chargement du cii à partir des infos contenus ds currentFpMLDataDocReader.party[i]
                                if (null != tradeparty)
                                {
                                    // EG 20160404 Migration vs2013
                                    // #warning 20060613 En cas d'integration d'un trade externe (sans otcmlId) il faut que partyId soit alimenté avec un ACTOR.IDENTIFIER (Prevoir L'utilisation de la fonction ActorTools.GetIdA)
                                    SQL_Actor sql_Actor = null;
                                    if (StrFunc.IsFilled(tradeparty.OtcmlId) && tradeparty.OTCmlId > 0)
                                        sql_Actor = new SQL_Actor(cciTrade.CSCacheOn, tradeparty.OTCmlId);
                                    else if (StrFunc.IsFilled(tradeparty.PartyId))
                                        sql_Actor = new SQL_Actor(cciTrade.CSCacheOn, tradeparty.PartyId);
                                    //
                                    if (null != sql_Actor)
                                    {
                                        // RD 20120322 / Intégration de trade "Incomplet"
                                        // Pour pouvoir afficher l'acteur SYSTEM qui n'est autorisé que sur les titres ou les trades incomplets
                                        // Ici je rajoute le test sur le Statut d'activation, car dans le cadre de l'import, en mode update d'un trade incomplet existant, 
                                        // la propriété "isActorSYSTEMAuthorized" de CciTrad¨Party n'est pas renseignée lors du premier chargement du trade.
                                        //if ((false == (isActorSYSTEMAuthorized || ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing))
                                        //    || ("SYSTEM" != tradeparty.partyId.ToUpper()))

                                        //FI 20130730 [18847] l'acteur SYSTEM doit tjs être reconnu par Spheres® 
                                        //Ceci de manière à ce que l'acteur SYSTEM ne se retrouve pas affiché côté clearing à cause de la méthode  SetPartyInOrder
                                        if ("SYSTEM" != tradeparty.PartyId.ToUpper())
                                            sql_Actor.SetRoleRange(GetRole());

                                        /// EG 20141020 [20442] Facturation avec Beneficiaire = CSS|ENTITY
                                        // EG 20230526 [WI640] Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
                                        if (IsInvoiceBeneficiary)
                                        {
                                            List<RoleActor> lst = new List<RoleActor>
                                            {
                                                RoleActor.INVOICINGOFFICE,
                                                RoleActor.ENTITY,
                                                RoleActor.CSS
                                            };
                                            sql_Actor.SetRoleRange(lst.ToArray());
                                        }
                                    }
                                    //
                                    if ((null != sql_Actor) && sql_Actor.IsLoaded)
                                    {
                                        //Affectation .XmlId
                                        //Ne pas toucher => Permet d'afficher les trades même si le xmlId d'un acteur a changé (Si son code bic ou identifier a changé)
                                        sql_Actor.XmlId = tradeparty.Id;
                                        //
                                        data = sql_Actor.Identifier;
                                        sql_Table = (SQL_Table)sql_Actor;
                                        isToValidate = (tradeparty.OTCmlId == 0);
                                        // FI 20131219 [19334] En mode saisie validation pour afficher une erreur lorsque l'acteur est désactivé.
                                        // RD 20200921 [25246] Ne pas écraser la valeur isToValidate dans l'instruction suivante
                                        if (Cst.Capture.IsModeInput(CcisBase.CaptureMode))
                                            isToValidate = isToValidate || (false == sql_Actor.IsEnabled);
                                    }
                                    //else if (ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing)
                                    //{
                                    //    data = tradeparty.partyId;
                                    //    sql_Table = null;
                                    //    isToValidate = (tradeparty.OTCmlId == 0);
                                    //}
                                    else
                                    {
                                        if (StrFunc.IsFilled(tradeparty.Id))
                                        {
                                            isToValidate = (tradeparty.Id == TradeCommonCustomCaptureInfos.PartyUnknown);
                                            // 20090512 RD
                                            if (false == isToValidate)
                                                isToValidate = (tradeparty.Id.ToLower() == TradeCommonCustomCaptureInfos.PartyIssuer.ToLower());
                                        }
                                        //
                                        // RD 20200921 [25246] Afficher partyId (équivalent à l'Identifier de l'acteur) car Id est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                                        // EG 20201007 [25246] Correction Alimentation data
                                        data = tradeparty.Id;
                                        //data = StrFunc.IsFilled(tradeparty.partyId) ? tradeparty.partyId : tradeparty.id;
                                        if (StrFunc.IsFilled(tradeparty.PartyId))
                                        {
                                            party = cciTrade.DataDocument.GetParty(partyId, PartyInfoEnum.id);
                                            if (null != party) // party peut être null si saisi de string.empty sur le cciReceiver
                                                data = party.Id;
                                        }
                                    }
                                }
                                #endregion Party
                            }
                            #endregion actor
                            break;
                        case CciEnum.book:
                            #region book
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.BookIdSpecified))
                            {
                                SQL_Book sql_Book = null;
                                int idA = 0;
                                if (null != Cci(CciEnum.actor).Sql_Table)
                                    idA = ((SQL_Actor)Cci(CciEnum.actor).Sql_Table).Id;
                                //
                                if (StrFunc.IsFilled(partyTradeIdentifier.BookId.OtcmlId) && partyTradeIdentifier.BookId.OTCmlId > 0)
                                {
                                    sql_Book = new SQL_Book(cciTrade.CSCacheOn, partyTradeIdentifier.BookId.OTCmlId);
                                }
                                else
                                {
                                    //FI 20130701 [18798] add if (StrFunc.IsFilled(partyTradeIdentifier.bookId.Value))
                                    // Sinon Spheres® récupère le 1er book venu
                                    if (StrFunc.IsFilled(partyTradeIdentifier.BookId.Value))
                                    {
                                        //FDA/PL 20221117 Use (idA <= 0) instead of (idA > 0)
                                        if (idA <= 0)
                                            sql_Book = new SQL_Book(cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, partyTradeIdentifier.BookId.Value);
                                        else
                                            sql_Book = new SQL_Book(cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, partyTradeIdentifier.BookId.Value, SQL_Table.ScanDataDtEnabledEnum.No, idA);
                                    }
                                }

                                //new SQL_Book(cciTrade.cs, SQL_TableWithID.IDType.Id, partyTradeIdentifier.bookId.otcmlId);
                                //FI 20130701 [18798]  test sur (null != sql_Book)
                                if ((null != sql_Book) && sql_Book.IsLoaded)
                                {
                                    data = sql_Book.Identifier;
                                    sql_Table = (SQL_Table)sql_Book;
                                    isToValidate = (partyTradeIdentifier.BookId.OTCmlId == 0);
                                    // FI 20131219 [19334] En mode saisie validation pour afficher une erreur lorsque le book est désactivé.
                                    // RD 20200921 [25246] Ne pas écraser la valeur isToValidate dans l'instruction suivante
                                    if (Cst.Capture.IsModeInput(CcisBase.CaptureMode))
                                        isToValidate = isToValidate || (false == sql_Book.IsEnabled);
                                }
                                else if ((StrFunc.IsFilled(partyTradeIdentifier.BookId.Value)) &&
                                    (Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing))
                                {
                                    // RD 20120322 / Intégration de trade "Incomplet"
                                    // Pour pouvoir afficher le Book "Incorrecte" et uniquement sur les trades incomplets                                        
                                    data = partyTradeIdentifier.BookId.Value;
                                    sql_Table = null;
                                    isToValidate = (partyTradeIdentifier.BookId.OTCmlId == 0);
                                }
                                //FI 20130701 [18798] ce code n'a pas de sens car party vaut null
                                //else
                                //    data = party.partyId;
                            }
                            #endregion book
                            break;
                        case CciEnum.localClassDerv:
                            #region LocalClassDerv
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.LocalClassDervSpecified))
                                data = partyTradeIdentifier.LocalClassDerv.Value;
                            #endregion LocalClassDerv
                            break;
                        case CciEnum.localClassNDrv:
                            #region LocalClassNDrv
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.LocalClassNDrvSpecified))
                                data = partyTradeIdentifier.LocalClassNDrv.Value;
                            #endregion LocalClassNDrv
                            break;
                        case CciEnum.iasClassDerv:
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.IasClassDervSpecified))
                                data = partyTradeIdentifier.IasClassDerv.Value;
                            break;
                        case CciEnum.iasClassNDrv:
                            #region IASClassNDrv
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.IasClassNDrvSpecified))
                                data = partyTradeIdentifier.IasClassNDrv.Value;
                            #endregion IASClassNDrv
                            break;
                        case CciEnum.fxClass:
                            #region FxClass
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.FxClassSpecified))
                                data = partyTradeIdentifier.FxClass.Value;
                            #endregion FxClass
                            break;
                        case CciEnum.hedgeClassDerv:
                            #region HedgeClassDerv
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.HedgeClassDervSpecified))
                                data = partyTradeIdentifier.HedgeClassDerv.Value;
                            #endregion HedgeClassDerv
                            break;
                        case CciEnum.hedgeClassNDrv:
                            #region HedgeClassNDrv
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.HedgeClassNDrvSpecified))
                                data = partyTradeIdentifier.HedgeClassNDrv.Value;
                            #endregion HedgeClassNDrv
                            break;
                        case CciEnum.hedgeFolder:
                            #region HedgeFolder
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.LinkIdSpecified))
                            {
                                foreach (ILinkId linkId in partyTradeIdentifier.LinkId)
                                {
                                    if (Cst.OTCmL_hedgingFolderid == linkId.LinkIdScheme)
                                    {
                                        data = linkId.Value;
                                        break;
                                    }
                                }
                            }
                            #endregion HedgeFolder
                            break;
                        case CciEnum.hedgeFactor:
                            #region HedgeFactor
                            if ((null != partyTradeIdentifier) && (partyTradeIdentifier.LinkIdSpecified))
                            {
                                foreach (ILinkId linkId in partyTradeIdentifier.LinkId)
                                {
                                    if (Cst.OTCmL_hedgingFolderid == linkId.LinkIdScheme)
                                    {
                                        if (StrFunc.IsFilled(linkId.StrFactor))
                                            data = linkId.StrFactor;
                                        break;
                                    }
                                }
                            }
                            #endregion HedgeFactor
                            break;
                        case CciEnum.frontId:
                            #region TradeId
                            if ((null != partyTradeIdentifier) && (null != partyTradeIdentifier.TradeId))
                            {
                                foreach (ISchemeId tId in partyTradeIdentifier.TradeId)
                                {
                                    if (Cst.OTCml_FrontTradeIdScheme == tId.Scheme)
                                    {
                                        data = tId.Value;
                                        break;
                                    }
                                }
                            }
                            #endregion TradeId
                            break;
                        case CciEnum.folder:
                            #region PartyFolder
                            if (null != partyTradeIdentifier && partyTradeIdentifier.LinkIdSpecified)
                            {
                                //20080430 PL Correction de BUG avec ISchemeId ??? (a vour avec EG)
                                foreach (ILinkId linkId in partyTradeIdentifier.LinkId)
                                {
                                    if (Cst.OTCml_FolderIdScheme == linkId.LinkIdScheme)
                                    {
                                        data = linkId.Value;
                                        break;
                                    }
                                }
                            }
                            #endregion PartyFolder
                            break;
                        case CciEnum.initiatorReactor:
                            if (partyType == PartyType.party)
                            {
                                if (cciTrade.DataDocument.CurrentTrade.TradeIntentionSpecified)
                                {
                                    if (cciTrade.DataDocument.IsPartyInitiator(GetPartyId(true)))
                                        data = IntentionEnum.Initiator.ToString();
                                    else if (cciTrade.DataDocument.IsPartyReactor(GetPartyId(true)))
                                        data = IntentionEnum.Reactor.ToString();
                                }
                            }
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_value:
                            // FI 20140204 [19564] Gestion UTI
                            if ((null != partyTradeIdentifier) && (null != partyTradeIdentifier.TradeId))
                            {
                                IScheme scheme = Tools.GetScheme(partyTradeIdentifier.TradeId, Cst.OTCml_TradeIdUTISpheresScheme);
                                if (null != scheme)
                                    data = scheme.Value;
                            }
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_issuer:
                            // FI 20140204 [19564] Gestion UTI
                            if ((null != partyTradeIdentifier) && (null != partyTradeIdentifier.TradeId))
                                data = ExtractFromTradeIdUTI(partyTradeIdentifier, UTIElement.issuer);
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_identifier:
                            // FI 20140204 [19564] Gestion UTI
                            if ((null != partyTradeIdentifier) && (null != partyTradeIdentifier.TradeId))
                                data = ExtractFromTradeIdUTI(partyTradeIdentifier, UTIElement.tradeId);
                            break;

                        case CciEnum.partyTradeInformation_executionWithinFirm:
                            Initialize_ExecutionOrInvestmentWithinFirm_FromDocument(out data, out sql_Table, RoleActor.EXECUTION);
                            break;
                        case CciEnum.partyTradeInformation_investmentDecisionWithinFirm:
                            Initialize_ExecutionOrInvestmentWithinFirm_FromDocument(out data, out sql_Table, RoleActor.INVESTDECISION);
                            break;
                        case CciEnum.partyTradeInformation_tradingCapacity:
                            if (null != partyTradeInformation && partyTradeInformation.CategorySpecified)
                            {
                                data = (from item in partyTradeInformation.Category.Where(x => x.Scheme == "http://www.fpml.org/coding-scheme/esma-mifir-trading-capacity")
                                        select item.Value).FirstOrDefault();
                            }
                            break;
                        case CciEnum.partyTradeInformation_tradingWaiver1:
                            if (null != partyTradeInformation && partyTradeInformation.TradingWaiverSpecified)
                                data = partyTradeInformation.TradingWaiver[0].Value;
                            break;
                        case CciEnum.partyTradeInformation_shortSale:
                            if (null != partyTradeInformation && partyTradeInformation.ShortSaleSpecified)
                                data = partyTradeInformation.ShortSale.Value;
                            break;
                        case CciEnum.partyTradeInformation_otcClassification1:
                            if (null != partyTradeInformation && partyTradeInformation.OtcClassificationSpecified)
                                data = partyTradeInformation.OtcClassification[0].Value;
                            break;
                        case CciEnum.partyTradeInformation_isCommodityHedge:
                            if (null != partyTradeInformation && partyTradeInformation.IsCommodityHedgeSpecified)
                                data = partyTradeInformation.IsCommodityHedge.ToString().ToLower();
                            break;

                        case CciEnum.partyTradeInformation_isSecuritiesFinancing:
                            if (null != partyTradeInformation && partyTradeInformation.IsSecuritiesFinancingSpecified)
                                data = partyTradeInformation.IsSecuritiesFinancing.ToString().ToString().ToLower();
                            break;

                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                    {
                        CcisBase.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }

            // FI 20170214 [23629] appel direct Initialize_FromCci 
            /*
            //20080514 RD Ticket 16108
            // L'appel est fait ici à cause de sql_actor qui n'est pas encore connu dans this.InitializeTrader_FromCci()
            InitializeTrader_FromCci();
            InitializeSales_FromCci();
            InitializeBroker_FromCci();
            */
            Initialize_FromCci();

            for (int i = 0; i < TraderLength; i++)
            {
                if (null != cciTrader[i].Trader)
                    cciTrader[i].Initialize_FromDocument();
            }
            //
            for (int i = 0; i < SalesLength; i++)
            {
                if (null != cciSales[i].Trader)
                    cciSales[i].Initialize_FromDocument();
            }
            //
            for (int i = 0; i < BrokerLength; i++)
            {
                cciBroker[i].Initialize_FromDocument();
            }
            //
            if (partyType == PartyType.party)
                SetIsMandatoryOnClass();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }

        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        // EG 20100208 ProductContainer
        // FI 20140204 [19564] Gestion UTI
        // EG 20170919 [22374] Add ExecutionDateTime
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                    //
                    switch (key)
                    {
                        case CciEnum.side:
                            #region side
                            cciTrade.SynchronizePayerReceiverFromSide();
                            #endregion side
                            break;
                        case CciEnum.actor:
                            ProcessInitializeActor(pCci);
                            break;
                        case CciEnum.book:
                            ProcessInitializeBook(pCci);
                            break;
                        case CciEnum.iasClassDerv:
                            #region IASClassDerv
                            bool isMandatory = (pCci.NewValue == "HEDGING");
                            Cci(CciEnum.hedgeClassDerv).IsMandatory = isMandatory;
                            if (isMandatory)
                            {
                                //Préproposition de HedgeClassDerv depuis BOOK
                                string hedgeClassDerv = string.Empty;
                                SQL_Book sql_Book = (SQL_Book)Cci(CciEnum.book).Sql_Table;
                                if (null != sql_Book)
                                {
                                    if (Ccis.TradeCommonInput.SQLInstrument.IsIFRS)
                                        hedgeClassDerv = sql_Book.HedgeClassDerv;
                                }
                                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeClassDerv), hedgeClassDerv);
                            }
                            else
                                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeClassDerv), string.Empty);
                            #endregion IASClassDerv
                            break;
                        case CciEnum.iasClassNDrv:
                            #region IASClassNDrv
                            isMandatory = (pCci.NewValue == "HEDGING");
                            Cci(CciEnum.hedgeClassNDrv).IsMandatory = isMandatory;
                            if (isMandatory)
                            {
                                //Préproposition de HedgeClassNDrv depuis BOOK
                                string hedgeClassNDrv = string.Empty;
                                SQL_Book sql_Book = (SQL_Book)Cci(CciEnum.book).Sql_Table;
                                if (null != sql_Book)
                                {
                                    if (Ccis.TradeCommonInput.SQLInstrument.IsIFRS)
                                        hedgeClassNDrv = sql_Book.HedgeClassNDrv;
                                }
                                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeClassNDrv), hedgeClassNDrv);
                            }
                            else
                                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeClassNDrv), string.Empty);
                            #endregion IASClassNDrv
                            break;
                        case CciEnum.initiatorReactor:
                            #region initiatorReactor
                            if (partyType == PartyType.party)
                            {
                                bool isInit = false;
                                string clientIdCtr = prefixParent + partyType.ToString() + (number == 1 ? "2" : "1") + CustomObject.KEY_SEPARATOR + CciEnum.actor.ToString();
                                CustomCaptureInfo cci = CcisBase[clientIdCtr];
                                if (null != cci)
                                    isInit = StrFunc.IsFilled(cci.NewValue);
                                //
                                if (isInit)
                                {
                                    if (StrFunc.IsFilled(pCci.NewValue))
                                    {
                                        IntentionEnum intention = (IntentionEnum)Enum.Parse(typeof(IntentionEnum), pCci.NewValue);
                                        if (intention == IntentionEnum.Reactor)
                                        {
                                            clientIdCtr = prefixParent + partyType.ToString() + (number == 1 ? "2" : "1") + CustomObject.KEY_SEPARATOR + CciEnum.initiatorReactor.ToString();
                                            cci = CcisBase[clientIdCtr];
                                            if (null != cci)
                                                CcisBase.SetNewValue(clientIdCtr, IntentionEnum.Initiator.ToString());
                                        }
                                    }
                                }
                            }
                            #endregion
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_issuer:
                        case CciEnum.partyTradeIdentifier_UTI_identifier:
                            //FI 20140204 [19564] 
                            SynchroCciUTIValue(pCci);
                            break;
                    }
                }
            }

            for (int i = 0; i < TraderLength; i++)
                cciTrader[i].ProcessInitialize(pCci);

            for (int i = 0; i < SalesLength; i++)
                cciSales[i].ProcessInitialize(pCci);

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].ProcessInitialize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        public void RefreshCciEnabled()
        {

            bool isEnabled = this.IsSpecified;
            bool isFrontEnabled = isEnabled;
            bool isFolderEnabled = isEnabled;
            //
            if (IsBrokerOfParty)
            {
                int index = cciBrokerParent.number - 1;
                int indexVs = (index == 1) ? 0 : 1;
                if (cciTrade.cciParty[indexVs].BrokerLength > 0)
                {
                    for (int i = 0; i < cciTrade.cciParty[indexVs].BrokerLength; i++)
                    {
                        if (cciTrade.cciParty[indexVs].cciBroker[i].GetPartyId() == GetPartyId())
                        {
                            //Front
                            if (isFrontEnabled)
                            {
                                if (null != cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.frontId))
                                    isFrontEnabled = cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.frontId).IsEmptyValue;
                            }
                            //Folder
                            if (isFolderEnabled)
                            {
                                if (null != cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.folder))
                                    isFolderEnabled = cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.folder).IsEmptyValue;
                            }
                            //Trader A Faire plus Tard
                        }
                    }
                }
            }

            CcisBase.Set(CciClientId(CciEnum.side), "IsEnabled", isEnabled);

            // RD 20091228 [16809] Confirmation indicators for each party
            IEnumerable<CciEnum> cciEnumNCM = CciTools.GetCciEnum<CciEnum>("NCM");
            foreach (CciEnum item in cciEnumNCM)
                CcisBase.Set(CciClientId(item), "IsEnabled", isEnabled);

            CcisBase.Set(CciClientId(CciEnum.frontId), "IsEnabled", isFrontEnabled);
            CcisBase.Set(CciClientId(CciEnum.folder), "IsEnabled", isFolderEnabled);

            for (int i = 0; i < TraderLength; i++)
                cciTrader[i].RefreshCciEnabled();

            for (int i = 0; i < SalesLength; i++)
                cciSales[i].RefreshCciEnabled();

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {

            if (IsCci(CciEnum.actor, pCci) && (null != pCci.Sql_Table))
                pCci.Display = pCci.Sql_Table.FirstRow["DISPLAYNAME"].ToString();
            //
            if (IsCci(CciEnum.book, pCci) && (null != pCci.Sql_Table))
                pCci.Display = pCci.Sql_Table.FirstRow["FULLNAME"].ToString();

            for (int i = 0; i < this.TraderLength; i++)
                cciTrader[i].SetDisplay(pCci);
            //
            for (int i = 0; i < this.SalesLength; i++)
                cciSales[i].SetDisplay(pCci);
            //
            for (int i = 0; i < this.BrokerLength; i++)
                cciBroker[i].SetDisplay(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140204 [19564] Gestion UTI
        /// EG 20170822 [23342] Add executionDateTime 
        /// EG 20171016 [23452] Upd
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        public void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    IPartyTradeInformation partyTradeInformation = GetPartyTradeInformation();
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.side:
                            #region side
                            //Notion inexistante en FpML
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion side
                            break;
                        // RD 20091228 [16809] Confirmation indicators for each party
                        case CciEnum.ISNCMINI:
                        case CciEnum.ISNCMINT:
                        case CciEnum.ISNCMFIN:
                            //Notion inexistante en FpML
                            if (PartyType.party == partyType)
                                cciTrade.cciTradeNotification.Dump_ToDocument(number - 1, cciEnum, data);
                            break;
                        case CciEnum.actor:
                            //FI 20120523 [recette 2.6SP2]
                            bool isIntercept = IntercepActor(cci, data);
                            if (false == isIntercept)
                                DumpParty_ToDocument(cci, data);

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;

                            break;
                        case CciEnum.book:
                            DumpBook_ToDocument(cci, data);
                            // FI 20180608 [XXXXX] Recette v7.2 
                            // =>  DumpTraderAndSale uniquement s'il n'y a pas d'erreur 
                            if (IsInitFromPartyTemplate && (false == cci.HasError))
                            {
                                //FI 20081124 Initialiation from PARTYTEMPLATE 
                                //FI 20141119 [20505] Recherche d'un trader/des sales 
                                //Boolean isOk = CaptureTools.DumpTrader_ToDocument_FromBook(cciTrade.CSCacheOn, cciTrade.TradeCommonInput, GetPartyId());
                                Boolean isOk = CaptureTools.DumpTraderAndSales_ToDocument(cciTrade.CSCacheOn, cciTrade.TradeCommonInput, GetPartyId());
                                if (isOk)
                                {
                                    CcisBase.IsToSynchronizeWithDocument = true;
                                    // FI 20240206 [WI840] la propriété IsToSynchronizeWithDocument n'est pas utlisée par IO
                                    // Il faut charger les ccis existants trader et ccis existants sales avec ce qui est remonté depuis PartyTemplate 
                                    if (Ccis.IsModeIO)
                                    {
                                        InitializeTrader_FromCci();
                                        InitializeSales_FromCci();

                                        foreach (CciTrader item in this.cciTrader)
                                            item.Initialize_Document();
                                        foreach (CciTrader item in this.cciSales)
                                            item.Initialize_Document();
                                    }
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.localClassDerv:
                            DumpLocalClassDerv_ToDocument(data);
                            break;
                        case CciEnum.localClassNDrv:
                            DumpLocalClassNDrv_ToDocument(data);
                            break;
                        case CciEnum.iasClassDerv:
                            DumpIASClassDerv_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.iasClassNDrv:
                            DumpIASClassNDrv_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.fxClass:
                            DumpFxClass_ToDocument(data);
                            break;
                        case CciEnum.hedgeClassDerv:
                            DumpHedgeClassDerv_ToDocument(data);
                            break;
                        case CciEnum.hedgeClassNDrv:
                            DumpHedgeClassNDrv_ToDocument(data);
                            break;
                        case CciEnum.hedgeFolder:
                            DumpHedgeFolder_ToDocument(cci, data);
                            break;
                        case CciEnum.hedgeFactor:
                            DumpHedgeFactor_ToDocument(cci, data);
                            break;
                        case CciEnum.frontId:
                            DumpFrontId_ToDocument(cci);
                            break;
                        case CciEnum.folder:
                            DumpFolderId_ToDocument(cci, data);
                            break;
                        case CciEnum.initiatorReactor:
                            if (partyType == PartyType.party)
                            {
                                DumpInitiatorReactor_ToDocument(data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            }
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_issuer:
                        case CciEnum.partyTradeIdentifier_UTI_identifier:
                            //En modification sans génération des évènemenrs Spheres® ne fait pas appel aux méthode ProcessInitialize
                            //Petit cas particulier ici puisque partyTradeIdentifier_UTI_value est un champ calculé
                            if (Cst.Capture.IsModeUpdatePostEvts(CcisBase.CaptureMode))
                            {
                                SynchroCciUTIValue(cci);
                                DumpUTI_ToDocument(Cci(CciEnum.partyTradeIdentifier_UTI_value));
                            }
                            else
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.partyTradeIdentifier_UTI_value:
                            DumpUTI_ToDocument(cci);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        case CciEnum.partyTradeInformation_executionWithinFirm:
                            Dump_ExecutionOrInvestmentWithinFirm_ToDocument(cci, data, RoleActor.EXECUTION);
                            break;
                        case CciEnum.partyTradeInformation_investmentDecisionWithinFirm:
                            Dump_ExecutionOrInvestmentWithinFirm_ToDocument(cci, data, RoleActor.INVESTDECISION);
                            break;
                        case CciEnum.partyTradeInformation_tradingCapacity:
                            if (null != partyTradeInformation)
                            {
                                SchemeData scheme = new SchemeData() { value = data, scheme = "http://www.fpml.org/coding-scheme/esma-mifir-trading-capacity" };
                                partyTradeInformation.Category = new IScheme[] { scheme };
                                partyTradeInformation.CategorySpecified = IsFilledScheme(partyTradeInformation.Category);
                            }
                            break;
                        case CciEnum.partyTradeInformation_tradingWaiver1:
                            if (null != partyTradeInformation)
                            {
                                if (ArrFunc.IsEmpty(partyTradeInformation.TradingWaiver))
                                {
                                    partyTradeInformation.TradingWaiver = new IScheme[] {
                                        cciTrade.DataDocument.CurrentProduct.ProductBase.CreateTradingWaiver() };
                                }
                                partyTradeInformation.TradingWaiver[0].Value = data;
                                partyTradeInformation.TradingWaiverSpecified = IsFilledScheme(partyTradeInformation.TradingWaiver);

                                //SchemeData scheme = new SchemeData() { value = data, scheme = "http://www.fpml.org/coding-scheme/esma-mifir-trading-waiver" };
                                //partyTradeInformation.tradingWaiver = new IScheme[] { scheme };
                                //partyTradeInformation.tradingWaiverSpecified = IsFilledScheme(partyTradeInformation.tradingWaiver);
                            }
                            break;

                        case CciEnum.partyTradeInformation_shortSale:
                            if (null != partyTradeInformation)
                            {
                                partyTradeInformation.ShortSale.Value = data;
                                partyTradeInformation.ShortSaleSpecified = StrFunc.IsFilled(data);
                            }
                            break;
                        case CciEnum.partyTradeInformation_otcClassification1:
                            if (null != partyTradeInformation)
                            {
                                if (ArrFunc.IsEmpty(partyTradeInformation.OtcClassification))
                                {
                                    partyTradeInformation.OtcClassification = new IScheme[] {
                                        cciTrade.DataDocument.CurrentProduct.ProductBase.CreateOtcClassification() };
                                }
                                partyTradeInformation.OtcClassification[0].Value = data;
                                partyTradeInformation.OtcClassificationSpecified = IsFilledScheme(partyTradeInformation.OtcClassification);

                                //SchemeData scheme = new SchemeData() { value = data, scheme = "http://www.fpml.org/coding-scheme/esma-mifir-otc-classification" };
                                //partyTradeInformation.otcClassification = new IScheme[] { scheme };
                                //partyTradeInformation.otcClassificationSpecified = IsFilledScheme(partyTradeInformation.otcClassification);

                            }
                            break;
                        case CciEnum.partyTradeInformation_isCommodityHedge:
                            if (null != partyTradeInformation)
                            {
                                partyTradeInformation.IsCommodityHedgeSpecified = StrFunc.IsFilled(data);
                                if (partyTradeInformation.IsCommodityHedgeSpecified)
                                    partyTradeInformation.IsCommodityHedge = BoolFunc.IsTrue(data);
                            }
                            break;
                        case CciEnum.partyTradeInformation_isSecuritiesFinancing:
                            if (null != partyTradeInformation)
                            {
                                partyTradeInformation.IsSecuritiesFinancingSpecified = StrFunc.IsFilled(data);
                                if (partyTradeInformation.IsSecuritiesFinancingSpecified)
                                    partyTradeInformation.IsSecuritiesFinancing = BoolFunc.IsTrue(data);
                            }
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            
            for (int i = 0; i < TraderLength; i++)
                cciTrader[i].Dump_ToDocument();
            for (int i = 0; i < SalesLength; i++)
                cciSales[i].Dump_ToDocument();
            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].Dump_ToDocument();

            IPartyTradeInformation partyTradeInfo = GetPartyTradeInformation();
            if (null != partyTradeInfo)
            {
                partyTradeInfo.TraderSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.TraderSpecified, cciTrader);
                partyTradeInfo.SalesSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.SalesSpecified, cciSales);
                partyTradeInfo.BrokerPartyReferenceSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.BrokerPartyReferenceSpecified, cciBroker);
            }

        }

        #endregion IContainerCciFactory Members

        #region IContainerCciSpecified Members
        /// <summary>
        /// Obtient true si sql_Table du cci actor est renseigné
        /// </summary>
        public bool IsSpecified
        {
            get
            {
                CustomCaptureInfo cci = Cci(CciEnum.actor);
                return (null != cci) && (null != cci.Sql_Table);
            }
        }
        #endregion IContainerCciSpecified Members

        #region Methods
        /// <summary>
        /// Dump a Book into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        /// 20130325 call CciTools.IsBookValid and CciTools.SearchCounterPartyActorOfBook
        /// FI 20150630 [XXXXX] Modify (plusieurs petites corrections pour que Spheres® affiche éventuellement 'plusieurs books correspondent')
        private void DumpBook_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            SQL_Book sql_Book = null;
            bool isLoaded = false;
            bool isFound = false;

            int idA = 0;
            SQL_Actor sql_Actor = ((SQL_Actor)CcisBase[CciClientId(CciEnum.actor)].Sql_Table);
            if (null != sql_Actor)
                idA = sql_Actor.Id;

            if (StrFunc.IsFilled(pData))
                CciTools.IsBookValid(cciTrade.CSCacheOn, pData, out sql_Book, out isLoaded, out isFound, idA, CcisBase.User, CcisBase.SessionId);


            pCci.ErrorMsg = string.Empty;
            IPartyTradeIdentifier partyTradeIdentifier = GetPartyTradeIdentifier();
            if (isFound)
            {
                #region Book is found in Database
                if (null == sql_Actor)
                {
                    sql_Actor = CciTools.SearchCounterPartyActorOfBook(cciTrade.CSCacheOn, sql_Book, CcisBase.User, CcisBase.SessionId);
                    if ((sql_Actor != null) && StrFunc.IsFilled(sql_Actor.Identifier))
                        DumpParty_ToDocument(CcisBase[CciClientId(CciEnum.actor)], sql_Actor.Identifier);
                }

                //FI 20120620 [17919] ceci ne doit pas être fait sur la partie droite d'une allocation
                //cela doit être fait dans tous les autres cas de figure même si le broker existe déjà
                //FI 20131219 [19367] add test (null != sql_Actor)
                //PL 20231215 FDA 20231215 Use IsAllocationAndLeftParty instead of IsAllocationAndRightParty (for TRADITION and not considering the entity of the executing broker's book)
                //PL 20231215 FDA 20231215 Rename IsAllocationAndLeftParty to IsAllocationAndDealerParty
                //if ((false == IsAllocationAndRightParty) && (null != sql_Actor))
                // FI 20240528 [WI946] call IsSetBrokerWithEntity method
                if (IsSetBrokerWithEntity() && (null != sql_Actor))
                {
                    SetBrokerWithEntity(sql_Actor, sql_Book);
                }
                pCci.NewValue = sql_Book.Identifier;
                pCci.Sql_Table = sql_Book;
                if (!sql_Book.IsEnabled)
                    pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_BookDisabled"), pCci.NewValue);


                partyTradeIdentifier = PartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    Tools.SetBookId(partyTradeIdentifier.BookId, sql_Book);
                    partyTradeIdentifier.BookIdSpecified = true;
                }

                DumpTradeSide_ToDocument();
                #endregion Book is found in Database
            }
            else
            {
                #region Book is NOT found in Database
                pCci.Sql_Table = null;
                if (pCci.IsFilled || (pCci.IsEmpty && pCci.IsMandatory))
                {
                    if (isLoaded)
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_BookNotUnique"), pCci.NewValue);
                    else
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_BookNotFound"), pCci.NewValue);
                }
                //
                // RD 20120322 / Intégration de trade "Incomplet"
                // Ajouter la Party si le Trade est en statut Missing
                if ((null == partyTradeIdentifier) &&
                    Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing)
                {
                    // RD 20200921 [25246] Chercher avec XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                    partyTradeIdentifier = PartyTradeIdentifier(GetPartyId(true));
                }

                if (null != partyTradeIdentifier)
                {
                    partyTradeIdentifier.BookId.Value = pCci.NewValue;
                    partyTradeIdentifier.BookId.Scheme = Cst.OTCml_BookIdScheme;
                    partyTradeIdentifier.BookId.BookNameSpecified = false;
                    partyTradeIdentifier.BookId.BookName = null;
                    partyTradeIdentifier.BookId.OTCmlId = 0;
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Pour afficher le Book incorrcte pour le Trade "Incomplet"
                    partyTradeIdentifier.BookIdSpecified = Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing;
                }
                #endregion Book is NOT found in Database
            }

        }

        /// <summary>
        /// Dump a FolderId into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFolderId_ToDocument(CustomCaptureInfo pCci, string pData)
        {

            ILinkId linkId = null;
            //					
            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            //
            if (null != partyTradeIdentifier)
            {
                bool isFilled = StrFunc.IsFilled(pData);

                if (StrFunc.IsFilled(pCci.LastValue))
                {
                    if (isFilled)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCml_FolderIdScheme);
                    }
                    else
                    {
                        //Find LastValue and remove
                        #region Remove linkId
                        int arrCounter = 0;
                        //20080519 PL Correction de BUG avec ISchemeId 
                        foreach (ILinkId lId in partyTradeIdentifier.LinkId)
                        {
                            if (Cst.OTCml_FolderIdScheme == lId.LinkIdScheme)
                            {
                                ReflectionTools.RemoveItemInArray(partyTradeIdentifier, "linkId", arrCounter);
                                break;
                            }
                            arrCounter++;
                        }
                        #endregion Remove linkId
                    }
                }
                else if (isFilled)
                {
                    linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCml_FolderIdScheme);
                    if (null == linkId)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                        if (null == linkId)
                        {
                            ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                            linkId = partyTradeIdentifier.LinkId[partyTradeIdentifier.LinkId.Length - 1];
                        }
                    }
                }
                //
                partyTradeIdentifier.LinkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.LinkId);
                //
                if (isFilled)
                {
                    pCci.ErrorMsg = string.Empty;
                    linkId.Value = pData;
                    linkId.LinkIdScheme = Cst.OTCml_FolderIdScheme;
                    linkId.Id = null;
                }
                else
                {
                    pCci.ErrorMsg = (pCci.IsMandatory ? Ressource.GetString("Msg_FolderIdNotFilled") : string.Empty);
                }
            }

        }

        /// <summary>
        /// Dump a TradeId (Front Id) into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20140204 [19564] modification de la signature Method et appel à SetTradeId
        private void DumpFrontId_ToDocument(CustomCaptureInfo pCci)
        {
            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
                SetTradeId(partyTradeIdentifier, Cst.OTCml_FrontTradeIdScheme, pCci);
        }

        /// <summary>
        /// Dump a Fx Class into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFxClass_ToDocument(string pData)
        {

            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.FxClass.Value = pData;
                    partyTradeIdentifier.FxClass.Scheme = Cst.OTCmL_FxClassScheme;
                    partyTradeIdentifier.FxClassSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.FxClass.Value = string.Empty;
                    partyTradeIdentifier.FxClass.Scheme = string.Empty;
                    partyTradeIdentifier.FxClassSpecified = false;
                }
            }

        }

        /// <summary>
        /// Dump a Hedge Class Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeClassDerv_ToDocument(string pData)
        {

            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {

                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.HedgeClassDerv.Value = pData;
                    partyTradeIdentifier.HedgeClassDerv.Scheme = Cst.OTCmL_HedgeClassDervScheme;
                    partyTradeIdentifier.HedgeClassDervSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.HedgeClassDerv.Value = string.Empty;
                    partyTradeIdentifier.HedgeClassDerv.Scheme = string.Empty;
                    partyTradeIdentifier.HedgeClassDervSpecified = false;
                }
            }

        }

        /// <summary>
        /// Dump a Hedge Class No Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeClassNDrv_ToDocument(string pData)
        {

            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();

            if (null != partyTradeIdentifier)
            {
                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.HedgeClassNDrv.Value = pData;
                    partyTradeIdentifier.HedgeClassNDrv.Scheme = Cst.OTCmL_HedgeClassNDrvScheme;
                    partyTradeIdentifier.HedgeClassNDrvSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.HedgeClassNDrv.Value = string.Empty;
                    partyTradeIdentifier.HedgeClassNDrv.Scheme = string.Empty;
                    partyTradeIdentifier.HedgeClassNDrvSpecified = false;
                }
            }

        }

        /// <summary>
        /// Dump a Hedge Factor into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeFactor_ToDocument(CustomCaptureInfo pCci, string pData)
        {

            ILinkId linkId = null;
            //					
            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            //
            if (null != partyTradeIdentifier)
            {
                bool isFilled = StrFunc.IsFilled(pData);

                if (StrFunc.IsFilled(pCci.LastValue))
                {
                    if (isFilled)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                    }
                }
                else if (isFilled)
                {
                    linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                    if (null == linkId)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                        if (null == linkId)
                        {
                            ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                            linkId = partyTradeIdentifier.LinkId[partyTradeIdentifier.LinkId.Length - 1];
                        }
                    }
                }
                //
                partyTradeIdentifier.LinkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.LinkId);
                //
                if (isFilled)
                {
                    linkId.LinkIdScheme = Cst.OTCmL_hedgingFolderid;
                    linkId.StrFactor = pData;
                }
                else
                {
                    pCci.ErrorMsg = (pCci.IsMandatory ? Ressource.GetString("Msg_FolderIdNotFilled") : string.Empty);
                }
            }

        }

        /// <summary>
        /// Dump a Hedge Folder into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeFolder_ToDocument(CustomCaptureInfo pCci, string pData)
        {

            ILinkId linkId = null;
            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                bool isFilled = StrFunc.IsFilled(pData);
                if (StrFunc.IsFilled(pCci.LastValue))
                {
                    if (isFilled)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                    }
                    else
                    {
                        //Find LastValue and remove
                        #region Remove linkId
                        int arrCounter = 0;
                        foreach (ISchemeId lId in partyTradeIdentifier.LinkId)
                        {
                            if (Cst.OTCmL_hedgingFolderid == lId.Scheme)
                            {
                                ReflectionTools.RemoveItemInArray(partyTradeIdentifier, "linkId", arrCounter);
                                break;
                            }
                            arrCounter++;
                        }
                        #endregion Remove linkId
                    }
                }
                else if (isFilled)
                {
                    linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                    if (null == linkId)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                        if (null == linkId)
                        {
                            ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                            linkId = partyTradeIdentifier.LinkId[partyTradeIdentifier.LinkId.Length - 1];
                        }
                    }
                }
                //
                partyTradeIdentifier.LinkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.LinkId);
                //
                if (isFilled)
                {
                    pCci.ErrorMsg = string.Empty;
                    linkId.Value = pData;
                    linkId.LinkIdScheme = Cst.OTCmL_hedgingFolderid;
                    linkId.Id = null;
                    //linkId.factor = string.Empty;
                }
                else
                {
                    pCci.ErrorMsg = (pCci.IsMandatory ? Ressource.GetString("Msg_FolderIdNotFilled") : string.Empty);
                }
            }

        }

        /// <summary>
        /// Dump a IAS Class Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpIASClassDerv_ToDocument(string pData)
        {

            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.IasClassDerv.Value = pData;
                    partyTradeIdentifier.IasClassDerv.Scheme = Cst.OTCmL_IASClassDervScheme;
                    partyTradeIdentifier.IasClassDervSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.IasClassDerv.Value = string.Empty;
                    partyTradeIdentifier.IasClassDerv.Scheme = string.Empty;
                    partyTradeIdentifier.IasClassDervSpecified = false;
                }
            }

        }

        /// <summary>
        /// Dump a IAS Class No Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpIASClassNDrv_ToDocument(string pData)
        {

            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.IasClassNDrv.Value = pData;
                    partyTradeIdentifier.IasClassNDrv.Scheme = Cst.OTCmL_IASClassNDrvScheme;
                    partyTradeIdentifier.IasClassNDrvSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.IasClassNDrv.Value = string.Empty;
                    partyTradeIdentifier.IasClassNDrv.Scheme = string.Empty;
                    partyTradeIdentifier.IasClassNDrvSpecified = false;
                }
            }

        }

        /// <summary>
        /// Dump a Local Class Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpLocalClassDerv_ToDocument(string pData)
        {

            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.LocalClassDerv.Value = pData;
                    partyTradeIdentifier.LocalClassDerv.Scheme = Cst.OTCmL_LocalClassDervScheme;
                    partyTradeIdentifier.LocalClassDervSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.LocalClassDerv.Value = string.Empty;
                    partyTradeIdentifier.LocalClassDerv.Scheme = string.Empty;
                    partyTradeIdentifier.LocalClassDervSpecified = false;
                }
            }

        }

        /// <summary>
        /// Dump a Local Class No Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpLocalClassNDrv_ToDocument(string pData)
        {
            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                if (StrFunc.IsFilled(pData))
                {
                    partyTradeIdentifier.LocalClassNDrv.Value = pData;
                    partyTradeIdentifier.LocalClassNDrv.Scheme = Cst.OTCmL_LocalClassNDrvScheme;
                    partyTradeIdentifier.LocalClassNDrvSpecified = true;
                }
                else
                {
                    partyTradeIdentifier.LocalClassNDrv.Value = string.Empty;
                    partyTradeIdentifier.LocalClassNDrv.Scheme = string.Empty;
                    partyTradeIdentifier.LocalClassNDrvSpecified = false;
                }
            }
        }




        /// <summary>
        /// Dump a party (party, broker) into DataDocument
        /// </summary>
        /// <param name="pCci">cci de type partie</param>
        /// <param name="pData">contient la valeur saisie</param>
        /// EG 20100208 ProductContainer.isDebtSecurity
        /// FI 20130325 call CciTools.IsActorValid
        /// FI 20131205 [19304] saisie d'un trade incomplet dont la partie a pour identifier une valeur numérique 
        /// FI 20140401 [19793] Tuning, suppression de l'usage de la méthode ccis.contains
        /// FI 20170928 [23452] Modify
        /// FI 20171201 [XXXXX] Modify
        private void DumpParty_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            SQL_Actor sql_Actor = null;
            bool isLoaded = false;
            bool isFound = false;

            if (StrFunc.IsFilled(pData))
                CciTools.IsActorValid(cciTrade.CSCacheOn, pData, out sql_Actor, out isLoaded, out isFound, IsActorSYSTEMAuthorized, GetRole(), CcisBase.User, CcisBase.SessionId);
            //
            #region If Counterparty, then check if actor is equal to the other counterparty
            if ((PartyType.party == partyType))
            {
                string clientIdCtr = prefixParent + partyType.ToString() + (number == 1 ? "2" : "1") + CustomObject.KEY_SEPARATOR + CciEnum.actor.ToString();
                CustomCaptureInfo cci = CcisBase[clientIdCtr];
                if (null != cci)
                {
                    bool isReplaceByUnknown = false;
                    if (isFound)
                    {
                        if (StrFunc.IsFilled(cci.NewValue) && cci.NewValue.ToLower() == sql_Actor.Identifier.ToLower())
                            isReplaceByUnknown = true;
                    }
                    else
                    {
                        if (StrFunc.IsEmpty(pData) && cci.IsEmptyValue)
                            isReplaceByUnknown = true;
                    }
                    //
                    if (isReplaceByUnknown)
                    {
                        isLoaded = false;
                        isFound = false;
                        pData = TradeCommonCustomCaptureInfos.PartyUnknown;
                        pCci.NewValue = pData;
                    }
                }
            }
            #endregion If Counterparty, then check if actor is equal to the other counterparty
            //
            //string party_id = (isFound ? sql_Actor.XmlId : pCci.NewValue);
            // FI 20131205 [19304] Appel à XMLTools.GetXmlId
            //party_id est maintenant nécessairement conforme à W3C
            // La valeur ne peut être un valeur numérique par exemple
            string party_id = (isFound ? sql_Actor.XmlId : XMLTools.GetXmlId(pCci.NewValue));



            #region Remove Last Party if is not use
            /* FI 20180306 [23822] Mise en commentaire des modifications liées au ticket 20409
            // RD 20141022 [20409] C'est pour prendre en compte l'Entité rajoutée automatiquement en tant que Broker
            // (voir la méthode CciTradeParty.SetBrokerWithEntity)
            //bool isLastInUSe = ccis.IsValueUseInParty(pCci.LastValue);
            bool isLastInUSe = ccis.IsLastValueUseInParty(pCci);
            */
            /* FI 20180306 [23822] Appel à IsValueUseInCciParty puisque le cci tradeHeader_party1_broker1_actor existe nécessairement (voir property IsAddCciSytemForBroker) */
            bool isLastInUSe = Ccis.IsValueUseInCciParty(pCci.LastValue);

            //
            // 20090511 RD 
            // Attention : 
            // 1- Il ne faut pas supprimer l'emetteur de l'Asset Titre SOUS-JACENT pour un Trade:
            //      ici on parle du Trade transaction du genre : DebtSecurityTransaction, Repo ....
            // 2- Par contre il faudrait supprimer l'emetteur du titre sur un Asset Titre si la Party n'est pas utilisée
            //      ici on parle du référentiel Titre : DebtSecurity
            //if (false == isLastInUSe && (false == cciTrade.DataDocument.isDebtSecurity))
            //    isLastInUSe = cciTrade.DataDocument.IsPartyIssuer(pCci.LastValue);
            //
            // 20091002 FI use IsValueUseInIssuer à la place de cciTrade.DataDocument.IsPartyIssuer
            //
            // 20091012 RD 
            // Si la party est ISSUER, alors ne pas la supprimer dans la collection des Party
            // Par contre, nettoyer tout le reste : partyTradeIdentifier, partyTradeInformation, tradeSide
            //
            bool isPartyUsedAsIssuer = false;
            if (false == isLastInUSe && (false == cciTrade.Product.IsDebtSecurity))
            {
                isPartyUsedAsIssuer = Ccis.IsValueUseInIssuer(pCci.LastValue);
                isLastInUSe = isPartyUsedAsIssuer;
            }
            //
            // FI 20131205 [19304] Appel à XMLTools.GetXmlId
            string lastParty_id = XMLTools.GetXmlId(pCci.LastValue);
            if (null != pCci.LastSql_Table)
                lastParty_id = ((SQL_Actor)pCci.LastSql_Table).XmlId;

            // FI 20240315 [WI913] Call RemovePartyTradeSideReference
            if (IsParty && IsInitTradeSide) 
                cciTrade.DataDocument.RemovePartyTradeSideReference(lastParty_id);

            if (false == isLastInUSe || isPartyUsedAsIssuer)
            {
                if (isPartyUsedAsIssuer)
                    cciTrade.DataDocument.RemovePartyReference(lastParty_id);
                else
                    cciTrade.DataDocument.RemoveParty(lastParty_id);
                // Sur les écrans avec brokers indépendants (isBrokerOfParty = false)
                // On réinitialize l'écran, car le nombre de broker dans le datadocument doit être en phase avec les broker à l'écran (index && count identiques)  
                // Le 1er cciPartyBroker alimente BrokerPartyReference 0, le 2ème cciPartyBroker alimente BrokerPartyReference 1, etc....
                if (IsPartyBroker && (false == IsBrokerOfParty))
                    cciTrade.InitializeBroker_FromCci();
            }
            
            #endregion Remove Last Party if is not use
            //
            #region Add Party
            IParty party = null;
            if (StrFunc.IsFilled(pData))
                party = cciTrade.DataDocument.AddParty(party_id);
            #endregion Add Party
            //
            #region Add brokerPartyReference if is IsPartyBroker
            if (IsPartyBroker)
            {
                if (IsBrokerOfParty)
                {
                    // Lorsque l'on supprime un broker et que ce dernier est utilisé ailleurs (isLastInUSe= true)
                    // On vient mettre à blanc le pointeur existant dans partyTradeInfo du cciBrokerParent
                    // 20091012 RD Et ceci uniquement si le broker n'est PAS ISSUER
                    if (isLastInUSe && (false == isPartyUsedAsIssuer))
                    {
                        IPartyTradeInformation partyTradeInfo = cciTrade.DataDocument.GetPartyTradeInformation(cciBrokerParent.GetPartyId());
                        if ((null != partyTradeInfo) && ArrFunc.IsFilled(partyTradeInfo.BrokerPartyReference))
                            partyTradeInfo.BrokerPartyReference[number - 1].HRef = party_id;
                    }
                }
                else
                {
                    //use Number Alimentation de 
                    IReference partyRef = cciTrade.CurrentTrade.BrokerPartyReference[number - 1];
                    partyRef.HRef = party_id;
                    cciTrade.CurrentTrade.BrokerPartyReferenceSpecified = ArrFunc.IsFilled(cciTrade.CurrentTrade.BrokerPartyReference);
                }
            }
            #endregion Add brokerPartyReference if is IsPartyBroker
            //
            if (isFound)
            {
                #region Party is found in Database
                pCci.NewValue = sql_Actor.Identifier;
                pCci.Sql_Table = sql_Actor;
                pCci.ErrorMsg = string.Empty;
                // FI 20131219 [19367] errorMsg alimenté si l'acteur n'est pas actif
                if (!sql_Actor.IsEnabled)
                    pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorDisabled"), pCci.NewValue);
                //
                Tools.SetParty(party, sql_Actor, cciTrade.DataDocument.IsEfsMLversionUpperThenVersion2);
                // FI 20120913 [18122] appel de la méthode DataDocumentContainer.SetAdditionalInfoOnParty
                // afin d'ajouter des informations plus riche sur l'acteur émetteur ( ex le code ISO18773-1)
                if ((cciTrade.Product.IsDebtSecurity) && (number == 1))
                    cciTrade.DataDocument.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, party, false);

                //20081124 FI Initialize_FromCci associée à la contrepartie 
                //Cette dernière initialize les traders,sales, brokers en fonction des zones présentes sur l'écran 
                if (null != cciBrokerParent)
                {
                    cciBrokerParent.Initialize_FromCci();
                    // FI 20171201 [XXXXX] Call AddCciSystem
                    cciBrokerParent.AddCciSystem();
                }
                else
                {
                    Initialize_FromCci();
                    // FI 20171201 [XXXXX] Call AddCciSystem
                    AddCciSystem();
                }

                // FI 20131205 [19304] Appel à la méthode SetPartyTradeInformationAndPartyTradeIdentifier
                SetPartyTradeInformationAndPartyTradeIdentifier(GetPartyId(false));

                // FI 20170928 [23452]
                cciTrade.DataDocument.SetRelatedMarketVenue();

                DumpTradeSide_ToDocument();

                #region Enrichir la Party "Issuer"
                // 20090512 RD
                // Enrichir la Party "Issuer" de l'Asset titre
                //
                if (cciTrade.IsCci_Issuer(pCci))
                    cciTrade.DataDocument.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, party, false);
                //DataDocumentContainer.SetAdditionalInfoOnParty(cciTrade.CSCacheOn, party, false);
                #endregion
                #endregion Party is found in Database
            }
            else
            {
                #region Party is NOT found in Database
                pCci.ErrorMsg = string.Empty;
                pCci.Sql_Table = null;
                //					
                if (pCci.IsFilled)
                {
                    if (cciTrade.IsStEnvTemplate)
                    {
                        if (pCci.NewValue != Cst.FpML_EntityOfUserIdentifier)
                        {
                            pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorNotFound"), pCci.NewValue);
                        }
                    }
                    else
                    {
                        if (isLoaded)
                        {
                            pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorNotUnique"), pCci.NewValue);
                        }
                        else
                        {
                            pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorNotFound"), pCci.NewValue);
                        }
                    }
                }
                //
                if (null != party)
                {
                    party.Id = party_id;
                    party.OTCmlId = 0;
                    //FI 20131205 [19304]
                    //party.partyId = party_id;
                    party.PartyId = pData;
                    party.PartyName = string.Empty;
                }
                // RD 20120322 / Intégration de trade "Incomplet"
                // Alimenter PartyTradeIdentifier avec l'acteur incorrecte si le Trade est Incomplet
                if (Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing)
                {
                    /// FI 20131205 [19304] ajout ce code insclus dans le if
                    if (StrFunc.IsFilled(party_id))
                    {
                        //20081124 FI Initialize_FromCci associée à la contrepartie 
                        //Cette dernière initialize les traders,sales, brokers en fonction des zones présentes sur l'écran 
                        if (null != cciBrokerParent)
                            cciBrokerParent.Initialize_FromCci();
                        else
                            Initialize_FromCci();

                        SetPartyTradeInformationAndPartyTradeIdentifier(party_id);
                    }
                }
                #endregion Party is NOT found in Database
            }
            //
            if (partyType == PartyType.party)
                cciTrade.cciTradeNotification.TradeNotification.PartyNotification[number - 1].IdActor = GetActorIda();
        }

        /// <summary>
        /// Alimente les éléments tradeSide du dataDocument
        /// <para>Si PartyType.party => Alimente le tradeSide associé à la partie </para>
        /// <para>Si PartyType.party => Alimente le tradeSide associé à la partie </para>
        /// </summary>
        /// <param name="pParty"></param>
        // EG 20180205 [23769] Add dbTransaction
        private void DumpTradeSide_ToDocument()
        {

            if (_isInitTradeSide)
            {
                if (partyType == PartyType.party)
                {
                    IParty party = cciTrade.DataDocument.GetParty(GetPartyId());
                    if (null != party)
                        cciTrade.DataDocument.SetTradeSide(cciTrade.CSCacheOn, null, party);
                }
                else if (partyType == PartyType.broker)
                {
                    cciTrade.DumpTradeSide_ToDocument();
                }
                else
                {
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", partyType.ToString()));
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        private void DumpInitiatorReactor_ToDocument(string pData)
        {
            string partyId = GetPartyId(true);
            cciTrade.DataDocument.RemovePartyTradeIntentionReference(partyId);
            if (StrFunc.IsFilled(pData))
            {
                IntentionEnum intention = (IntentionEnum)Enum.Parse(typeof(IntentionEnum), pData);
                cciTrade.DataDocument.AddPartyTradeIntention(partyId, intention);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCalculationAgent()
        {
            CustomCaptureInfo cci = CcisBase[CciClientId(CciEnum.book.ToString())];
            if ((null != cci) && (null != cci.Sql_Table))
            {
                SQL_Book book = (SQL_Book)cci.Sql_Table;
                if ((null != book) && (0 < book.IdA_Entity))
                    return GetPartyId();
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCalculationAgentBC()
        {
            CustomCaptureInfo cci = CcisBase[CciClientId(CciTradeParty.CciEnum.book.ToString())];
            if ((null != cci) && (null != cci.Sql_Table))
            {
                SQL_Book book = (SQL_Book)cci.Sql_Table;
                if ((null != book) && (0 < book.IdA_Entity))
                {
                    SQL_Actor entity = new SQL_Actor(cciTrade.CSCacheOn, book.IdA_Entity)
                    {
                        WithInfoEntity = true
                    };
                    if (entity.IsLoaded)
                        return entity.IdBC;
                }
            }
            return null;
        }



        /// <summary>
        /// Retourne l'attribut id attribué à l'élément Party
        /// </summary>
        /// <param name="pIsGetNewValueWhenDataNoValid">
        /// <para>Uniquement lorsque la donnée du cci (newValue) est non valide</para>
        /// <para>Si true, retoune la donnée du cci (newValue)</para>
        /// <para>Si false, retoune string.Empty</para>
        /// </param>
        /// <returns></returns>
        /// FI 20170928 [23452] Modify
        public override string GetPartyId(bool pIsGetNewValueWhenDataNoValid)
        {
            string ret = string.Empty;

            CustomCaptureInfo cci = Cci(CciEnum.actor);

            if (null != cci)
            {
                // FI 20170928 [23452] Appel method Cci
                if (null != cci.Sql_Table)
                    ret = ((SQL_Actor)Cci(CciEnum.actor).Sql_Table).XmlId;
                // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                else if (pIsGetNewValueWhenDataNoValid && StrFunc.IsFilled(cci.NewValue))
                    //ret = cci.NewValue;
                    ret = XMLTools.GetXmlId(cci.NewValue);
            }

            return ret;

        }


        /// <summary>
        /// Retourne la liste des rôles acceptés pour la partie
        /// <para>Si CciTradeParty représente un broker alors les rôles acceptés sont BROKER ou ENTITY</para>
        /// <para>Si CciTradeParty représente une contrepartie alors les rôles acceptés sont ISSUER (si titre) ou COUNTERPARTY (si trade) ou </para>
        /// <para>La liste des rôles accptés sont enrichis des rôles présents dans additionnalRole</para>          
        /// </summary>
        /// <returns></returns>
        private RoleActor[] GetRole()
        {
            List<RoleActor> lst = new List<RoleActor>();

            if (IsPartyBroker)
            {
                // RD 20120216 [17322]
                // Pour cette Partie, on peut avoir deux rôles:
                // 1- Entity: à cause de la Pre-proposition de l'entitié du book, et de toutes les Entités parentes ( même si elles ne sont pas Broker)
                // 2- Broker: Saisie d'un Broker
                lst.Add(RoleActor.BROKER);
                lst.Add(RoleActor.ENTITY);
            }
            else if (IsParty)
            {
                if (this.cciTrade.Product.IsDebtSecurity)
                    lst.Add(RoleActor.ISSUER);
                // EG 20150907 [21317] New test (IMPORTANT : Dans le cas d'une alloc sur la partie Clearer|Custodian seul ce rôle est retenu)
                // FI 20200406 [XXXXX] Pas de rôle Contrepartie si isMarginRequirement
                else if (false == (IsAllocationAndClearerParty || cciTrade.Product.IsMarginRequirement))
                    lst.Add(RoleActor.COUNTERPARTY);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", partyType.ToString()));
            //
            if (ArrFunc.IsFilled(AdditionnalRole))
            {
                for (int i = 0; i < ArrFunc.Count(AdditionnalRole); i++)
                {
                    lst.Add(AdditionnalRole[i]);
                }
            }

            RoleActor[] ret = lst.ToArray();

            return ret;

        }

        /// <summary>
        /// Retourne l'id non significatif de l'acteur
        /// <para>Retourne -99 si l'acteur est inconnu</para>
        /// </summary>
        /// <returns></returns>
        public int GetActorIda()
        {
            int ret = -99;
            //
            CustomCaptureInfo cci = Cci(CciEnum.actor);
            if (null != cci)
            {
                SQL_Actor sql_Actor = (SQL_Actor)cci.Sql_Table;
                if (null != sql_Actor)
                    ret = sql_Actor.Id;
            }
            return ret;

        }

        /// <summary>
        /// Retourne l'id non significatif du book
        /// <para>Retourne -99 si le book est inconnu</para>
        /// </summary>
        /// <returns></returns>
        public int GetBookIdB()
        {
            int ret = -99;
            //
            CustomCaptureInfo cci = Cci(CciEnum.book);
            if (null != cci)
            {
                SQL_Book sql_book = (SQL_Book)Cci(CciEnum.book).Sql_Table;
                if (null != sql_book)
                    ret = sql_book.Id;
            }
            //
            return ret;

        }

        /// <summary>
        /// Alimente le cciSide selon que la partie soit acheteuse ou vendeuse sur le trade
        /// </summary>
        public void InitializePartySide()
        {

            string idPayer = string.Empty;
            string lastIdPayer = string.Empty;
            //
            if (null != CcisBase[cciTrade.CciClientIdPayer])
            {
                idPayer = CcisBase[cciTrade.CciClientIdPayer].NewValue;
                lastIdPayer = CcisBase[cciTrade.CciClientIdPayer].LastValue;
            }
            //
            CustomCaptureInfo cciSide = Cci(CciEnum.side);
            CustomCaptureInfo cciParty = Cci(CciEnum.actor);
            //
            if ((null != cciSide) && (null != cciParty))
            {
                SQL_Actor sql_Actor = (SQL_Actor)cciParty.Sql_Table;
                // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                if (((null != sql_Actor) && (XMLTools.GetXmlId(idPayer) == sql_Actor.XmlId)) ||                                //cas Normal
                      (lastIdPayer == cciParty.LastValue && lastIdPayer == Cst.FpML_EntityOfUserIdentifier) || //2eme Cas pour EntityOfUSer
                      (idPayer == cciParty.NewValue)                                                           //3eme Cas {unknown},
                    )
                    cciSide.NewValue = cciTrade.RetSidePayer;
                else
                    cciSide.NewValue = cciTrade.RetSideReceiver;

                #region Garde fou pour que les sens du DO et de la CTR soit tjs opposés
                if (this.number == 2)
                {
                    if (cciSide.NewValue == CcisBase[cciTrade.cciParty[0].CciClientId(CciTradeParty.CciEnum.side)].NewValue)
                    {
                        if (cciSide.NewValue == cciTrade.RetSideReceiver)
                            cciSide.NewValue = cciTrade.RetSidePayer;
                        else
                            cciSide.NewValue = cciTrade.RetSideReceiver;
                    }
                }
                #endregion Garde fou pour que les sens du DO et de la CTR soit tjs opposés
                //
                cciSide.LastValue = cciSide.NewValue;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140708 [20179] Modify: Gestion du mode IsModeMatch
        /// FI 20171201 [XXXXX] Modify
        private void InitializeTrader_FromCci()
        {

            bool bSavPartyTradeInformationSpecified;
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            bSavPartyTradeInformationSpecified = cciTrade.CurrentTrade.TradeHeader.PartyTradeInformationSpecified;

            while (isOk)
            {
                index += 1;
                CciTrader ccitraderCurrent = new CciTrader(this, index + 1, Prefix, CciTrader.TraderTypeEnum.trader);
                isOk = CcisBase.Contains(ccitraderCurrent.CciClientId(CciTrader.CciEnum.identifier));
                if (isOk)
                {
                    IPartyTradeInformation partyTradeInformation;
                    //Si mode action autre que RemoveReplace ou si mode consult
                    if ((Cst.Capture.IsModeAction(CcisBase.CaptureMode) &&
                        CcisBase.CaptureMode != Cst.Capture.ModeEnum.RemoveReplace &&
                        CcisBase.CaptureMode != Cst.Capture.ModeEnum.PositionTransfer)
                        || Cst.Capture.IsModeConsult(CcisBase.CaptureMode)
                        || Cst.Capture.IsModeMatch(CcisBase.CaptureMode))
                        partyTradeInformation = GetPartyTradeInformation();
                    else
                        partyTradeInformation = PartyTradeInformation();
                    //
                    if (null != partyTradeInformation)
                    {
                        bool bSavSpecified = partyTradeInformation.TraderSpecified;
                        if (ArrFunc.IsEmpty(partyTradeInformation.Trader) || (index == partyTradeInformation.Trader.Length))
                            ReflectionTools.AddItemInArray(partyTradeInformation, "trader", index);
                        //
                        ccitraderCurrent.Trader = partyTradeInformation.Trader[index];
                        partyTradeInformation.TraderSpecified = bSavSpecified;
                        // FI 20171201 [XXXXX] Ajout dans la liste uniquement si (null != partyTradeInformation)
                        lst.Add(ccitraderCurrent);
                    }
                    // FI 20171201 [XXXXX] Ajout dans la liste uniquement si (null != partyTradeInformation)
                    //lst.Add(ccitraderCurrent);
                }
            }
            //
            cciTrader = null;
            cciTrader = (CciTrader[])lst.ToArray(typeof(CciTrader));
            //
            cciTrade.CurrentTrade.TradeHeader.PartyTradeInformationSpecified = bSavPartyTradeInformationSpecified;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140708 [20179] Modify : gestion du mode IsModeMatch
        public void InitializeSales_FromCci()
        {

            bool bSavPartyTradeInformationSpecified;
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            bSavPartyTradeInformationSpecified = cciTrade.CurrentTrade.TradeHeader.PartyTradeInformationSpecified;

            while (isOk)
            {
                index += 1;
                CciTrader cciSalesCurrent = new CciTrader(this, index + 1, Prefix, CciTrader.TraderTypeEnum.sales);
                isOk = CcisBase.Contains(cciSalesCurrent.CciClientId(CciTrader.CciEnum.identifier));
                if (isOk)
                {
                    IPartyTradeInformation partyTradeInformation;
                    if ((Cst.Capture.IsModeAction(CcisBase.CaptureMode)
                        && CcisBase.CaptureMode != Cst.Capture.ModeEnum.RemoveReplace
                        && CcisBase.CaptureMode != Cst.Capture.ModeEnum.PositionTransfer)
                    || Cst.Capture.IsModeConsult(CcisBase.CaptureMode)
                    || Cst.Capture.IsModeMatch(CcisBase.CaptureMode))
                        partyTradeInformation = GetPartyTradeInformation();
                    else
                        partyTradeInformation = PartyTradeInformation();
                    //
                    if (null != partyTradeInformation)
                    {
                        bool bSavSpecified = partyTradeInformation.TraderSpecified;
                        if (ArrFunc.IsEmpty(partyTradeInformation.Sales) || (index == partyTradeInformation.Sales.Length))
                            ReflectionTools.AddItemInArray(partyTradeInformation, "sales", index);
                        //
                        cciSalesCurrent.Trader = partyTradeInformation.Sales[index];
                        partyTradeInformation.SalesSpecified = bSavSpecified;
                    }
                    lst.Add(cciSalesCurrent);
                }
            }
            //
            cciSales = null;
            cciSales = (CciTrader[])lst.ToArray(typeof(CciTrader));
            //
            cciTrade.CurrentTrade.TradeHeader.PartyTradeInformationSpecified = bSavPartyTradeInformationSpecified;

        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20140708 [20179] Modify: Gestion du mode IsModeMatch
        /// FI 20170116 [21916] Modify 
        /// FI 20170718 [23326] Modify
        /// FI 20170906 [23401] Modify
        /// FI 20171201 [XXXXX] Modify
        private void InitializeBroker_FromCci()
        {

            bool bSavPartyTradeInformationSpecified = cciTrade.CurrentTrade.TradeHeader.PartyTradeInformationSpecified;

            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciTradeParty ccibrokerCurrent = new CciTradeParty(cciTrade, index + 1, PartyType.broker, Prefix)
                {
                    cciBrokerParent = this
                };
                // FI 20170116 [21916] add cas (this.isInitFromClearingTemplate && index==0)
                // Permmet l'alimentation d'un broker de nego via CLEARINGTEMPLATE lorsque ce cci est absent (possible en mode importation)
                //isOk = ccis.Contains(ccibrokerCurrent.CciClientId(CciTradeParty.CciEnum.actor)) || (this.isInitFromClearingTemplate && index==0);

                // FI 20170718 [23326] ajout aussi d'1 broker systématiquement lorsque (false == this.IsAllocationAndRightParty)
                // car le cci acteur sera ajouté en tant que cci système
                //isOk = ccis.Contains(ccibrokerCurrent.CciClientId(CciTradeParty.CciEnum.actor)) ||
                //  (this.IsParty &&
                //          ((this.isInitFromClearingTemplate && index == 0) || ((false == this.IsAllocationAndRightParty) && index == 0))
                //  );

                // FI 20170906 [23401] Utilisation de la property IsAddCciSytemForBroker
                isOk = CcisBase.Contains(ccibrokerCurrent.CciClientId(CciTradeParty.CciEnum.actor)) || (IsAddCciSytemForBroker && index == 0);

                if (isOk)
                {
                    IPartyTradeInformation partyTradeInformation;
                    if ((Cst.Capture.IsModeAction(CcisBase.CaptureMode) &&
                        CcisBase.CaptureMode != Cst.Capture.ModeEnum.RemoveReplace &&
                        CcisBase.CaptureMode != Cst.Capture.ModeEnum.PositionTransfer)
                        || Cst.Capture.IsModeConsult(CcisBase.CaptureMode)
                        || Cst.Capture.IsModeMatch(CcisBase.CaptureMode))
                        partyTradeInformation = GetPartyTradeInformation();
                    else
                        partyTradeInformation = PartyTradeInformation();

                    if (null != partyTradeInformation)
                    {
                        bool bSavSpecified = partyTradeInformation.BrokerPartyReferenceSpecified;

                        if ((ArrFunc.IsEmpty(partyTradeInformation.BrokerPartyReference) || (index == partyTradeInformation.BrokerPartyReference.Length)))
                            ReflectionTools.AddItemInArray(partyTradeInformation, "brokerPartyReference", index);

                        partyTradeInformation.BrokerPartyReferenceSpecified = bSavSpecified;

                        // FI 20171201 [XXXXX] Ajout dans la liste uniquement si (null != partyTradeInformation)
                        lst.Add(ccibrokerCurrent);

                        // FI 20171208 [XXXXX] Spheres® ajoute juste au dessus un ccibrokerCurrent alors qu'éventuellement il n'existe pas le actor (Je le rajoute donc)
                        // Permet de corriger un bug rencontré en recette sur l'importation d'un trade où il n'existe aucune modification sur les parties
                        CciTools.AddCciSystem(CcisBase, Cst.TXT + ccibrokerCurrent.CciClientId(CciTradeParty.CciEnum.actor), false, TypeData.TypeDataEnum.@string);
                    }
                    // FI 20171201 [XXXXX] Ajout dans la liste uniquement si (null != partyTradeInformation)
                    //lst.Add(ccibrokerCurrent);
                }
            }

            cciBroker = null;
            cciBroker = (CciTradeParty[])lst.ToArray(typeof(CciTradeParty));

            cciTrade.CurrentTrade.TradeHeader.PartyTradeInformationSpecified = bSavPartyTradeInformationSpecified;

        }

        /// <summary>
        /// 
        /// </summary>
        private void SetIsMandatoryOnClass()
        {
            bool isIFRS = Ccis.TradeCommonInput.SQLInstrument.IsIFRS;

            CustomCaptureInfo cci = Cci(CciEnum.book);
            CustomCaptureInfo cciActor = Cci(CciEnum.actor);

            bool isMandatory = false;
            //
            if ((null != cci) && (null != cci.Sql_Table))
            {
                SQL_Book sql_Book = (SQL_Book)cci.Sql_Table;
                isMandatory = ((null != sql_Book) && (sql_Book.IdA_Entity > 0));
                //
                if (isMandatory)
                {
                    isMandatory = false;
                    if (null != cciActor.Sql_Table)
                    {
                        int ida = ((SQL_Actor)cciActor.Sql_Table).Id;
                        isMandatory = (false == ActorTools.IsActorClient(cciTrade.CSCacheOn, ida));
                    }
                }
            }

            //Derv
            CcisBase.Set(CciClientId(CciEnum.localClassDerv), "IsMandatory", isMandatory);
            //20070808 FI ticket 15636 => add isIFRS
            CcisBase.Set(CciClientId(CciEnum.iasClassDerv), "IsMandatory", isMandatory && isIFRS);
            //20070808 FI ticket 15636 => mise en commentaire, isMandatory est affecté en cascade par la mise à jour de iasClassDerv  
            //ccis.Set(CciClientId(CciEnum.hedgeClassDerv),"isMandatory",isMandatory); 
            //NDrv
            CcisBase.Set(CciClientId(CciEnum.localClassNDrv), "IsMandatory", isMandatory);
            //20070808 FI ticket 15636 => add isIFRS
            CcisBase.Set(CciClientId(CciEnum.iasClassNDrv), "IsMandatory", isMandatory && isIFRS);
            //20070808 FI ticket 15636 => mise en commentaire, isMandatory est affecté en cascade par la mise à jour de iasClassDerv  
            //ccis.Set(CciClientId(CciEnum.hedgeClassNDrv),"isMandatory",isMandatory); isMandatory est affecté en cascade par la mise à jour de iasClassNDrv
            //Fx
            CcisBase.Set(CciClientId(CciEnum.fxClass), "IsMandatory", isMandatory);

        }
        #endregion Methods

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20170718 [23326] Modify
        /// FI 20170928 [23452] Modify 
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.actor);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ACTOR);

            cci = Cci(CciEnum.book);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.BOOK);

            if ((IsParty) && ArrFunc.IsFilled(cciBroker))
            {
                foreach (CciTradeParty cciParty in cciBroker)
                {
                    cciParty.DumpSpecific_ToGUI(pPage);
                }
            }

            // FI 20170928 [23452] Add
            cci = Cci(CciEnum.partyTradeInformation_executionWithinFirm);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ACTOR);

            // FI 20170928 [23452] Add
            cci = Cci(CciEnum.partyTradeInformation_investmentDecisionWithinFirm);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ACTOR);

            for (int i = 0; i < ArrFunc.Count(cciTrader); i++)
                cciTrader[i].DumpSpecific_ToGUI(pPage);

            for (int i = 0; i < ArrFunc.Count(cciSales); i++)
                cciSales[i].DumpSpecific_ToGUI(pPage);
        }
        #endregion

        /// <summary>
        /// Retourne true lorsque la donnée saisie dans le CCI actor donne lieu à un traitement particulier
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120523 [recette 2.6SP2]
        private bool IntercepActor(CustomCaptureInfo pCci, string pData)
        {
            bool ret = false;

            if (IsAllocationAndClearerParty)
            {
                //Lorsque l'utilisateur saisi un acteur BROKER,CLEARER
                //Si l'entité est Membre de la chambre
                //Alors Spheres® positionne 
                // - dans la zone Clearing la chambre
                // - dans la zone broker associé, l'acteur saisi par l'utilsateur 

                // RD 20170518 [23162] Add test if (StrFunc.IsFilled(pData))
                //SQL_Actor sqlActor;
                //bool isFound;
                //bool isLoaded;
                //CciTools.IsActorValid(cciTrade.CSCacheOn, pData, out sqlActor, out isLoaded, out isFound, isActorSYSTEMAuthorized, GetRole(), ccis.User, ccis.SessionId);

                SQL_Actor sqlActor = null;
                bool isFound = false;
                if (StrFunc.IsFilled(pData))
                    CciTools.IsActorValid(cciTrade.CSCacheOn, pData, out sqlActor, out _, out isFound, IsActorSYSTEMAuthorized, GetRole(), CcisBase.User, CcisBase.SessionId);

                if (isFound)
                {
                    //PL 20140722 Add Test on isExchangeTradedDerivative
                    if (((ProductContainerBase)cciTrade.cciProduct.Product).IsExchangeTradedDerivative)
                    {
                        #region isExchangeTradedDerivative
                        if (ActorTools.IsActorWithRole(cciTrade.CSCacheOn, sqlActor.Id, RoleActor.BROKER))
                        {
                            int idAEntity = BookTools.GetEntityBook(cciTrade.CSCacheOn, cciTrade.cciParty[0].GetBookIdB());
                            if (idAEntity > 0)
                            {
                                ExchangeTradedContainer etd = new ExchangeTradedContainer((IExchangeTradedBase)cciTrade.cciProduct.Product.Product);
                                SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(cciTrade.CSCacheOn, null, etd.TradeCaptureReport.Instrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                if ((null != sqlMarket) && (sqlMarket.IdA > 0))
                                {
                                    if (CaptureTools.IsActorClearingMember(cciTrade.CSCacheOn, idAEntity, sqlMarket.Id, true))
                                    {
                                        //---------------------------------------------------------------------------------------------
                                        //L'entité est GCM/DCM sur le marché --> On préselectionne la Clearing House associée au marché.
                                        //---------------------------------------------------------------------------------------------
                                        ret = true;
                                        CcisBase.IsToSynchronizeWithDocument = true;

                                        pCci.Reset();
                                        SQL_Actor sqlCSS = new SQL_Actor(cciTrade.CSCacheOn, sqlMarket.IdA);
                                        sqlCSS.LoadTable(new string[] { "IDENTIFIER" });
                                        pCci.NewValue = sqlCSS.Identifier;
                                        DumpParty_ToDocument(pCci, pCci.NewValue);

                                        //L'acteur saisie est Broker (PL 20140722 je ne comprends pas !)
                                        IParty party = cciTrade.DataDocument.AddParty(sqlActor.XmlId);
                                        Tools.SetParty(party, sqlActor, cciTrade.DataDocument.IsEfsMLversionUpperThenVersion2);
                                        cciTrade.DataDocument.AddBroker(sqlActor.XmlId);
                                        //Le broker est rattaché à la Clearing House 
                                        IPartyTradeInformation tradeInfo = GetPartyTradeInformation();
                                        cciTrade.DataDocument.AddBrokerPartyReference(tradeInfo, sqlActor.XmlId);
                                    }
                                }
                            }
                        }
                        #endregion isExchangeTradedDerivative
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimente les brokers rattachés à la partie avec la hierarchie des entités 
        /// </summary>
        /// <param name="pSqlBook"></param>
        /// Pre-proposition de l'entitié du book, 
        /// et de toutes les Entités parentes ( même si elles ne sont pas Broker)
        /// FI 20140407 [XXXXX] modifs bug trouvé par la recette
        /// EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude (GetActors)
        private void SetBrokerWithEntity(SQL_Actor pSqlActor, SQL_Book pSqlBook)
        {
            // Charger la hiérarchie des Entity
            if (pSqlBook.IdA_Entity > 0)
            {
                SearchAncestorRole search = new SearchAncestorRole(cciTrade.CSCacheOn, null, pSqlBook.IdA_Entity, RoleActor.ENTITY);
                int[] ancestors = search.GetActors(new RoleActor[] { RoleActor.ENTITY }, null);
                //
                for (int i = 0; i < ArrFunc.Count(ancestors); i++)
                {
                    if (ancestors[i] > 0)
                    {
                        SQL_Actor sqlActorAncestor = new SQL_Actor(cciTrade.CSCacheOn, ancestors[i]);
                        sqlActorAncestor.LoadTable();
                        //
                        bool isAdd = true;
                        if (pSqlActor.Identifier == sqlActorAncestor.Identifier)
                        {
                            //FI 20121003 [18165] lorsque l'entité est déjà la partie 
                            //Spheres® peut ajouter l'entité en tant que (« Gestionnaire(s) et autre(s) intervenant(s) ») 
                            //Cela dépend du paramètre global EFSSOFTWARE.ISADDENTITYPARTYINBROKER
                            isAdd = CaptureTools.IsAddEntityPartyInBroker(cciTrade.CSCacheOn);
                        }
                        //
                        if (isAdd)
                        {
                            // FI 20121002 [18165] L'acteur Broker (« Gestionnaire(s) et autre(s) intervenant(s) ») est alimenté avec l'entité même si cette dernière est la contrepartie (dealer)
                            // Ceci est nécessaire pour HPC, Il existe des conditions où HPC est PartyA et OtherParty1 (voir FEEMATRIX => "FEES-FESX-0.3EUR/LOT")
                            bool isBrokerSetToCci = false;
                            //
                            // Ajouter l'Entity dans la collection de cciBroker existante
                            if (ArrFunc.Count(cciBroker) > i)
                            {
                                cciBroker[i].DumpParty_ToDocument(CcisBase[cciBroker[i].CciClientId(CciEnum.actor)], sqlActorAncestor.Identifier);
                                isBrokerSetToCci = true;
                            }
                            //
                            // Si l'Entity n'est pas rajoutée dans la collection de cciBroker, 
                            // alors la rajouter directement dans le DataDocument avec les 5 étapes suivantes:                                        
                            //if ((false == isBrokerExistInCci) && (false == isBrokerSetToCci))
                            if (false == isBrokerSetToCci)
                            {
                                // Pour recharger les cci
                                CcisBase.IsToSynchronizeWithDocument = true;
                                //
                                // 1- Ajouter l'Entity dans la collection Party du DataDocument si elle n'existe pas
                                IParty party = cciTrade.DataDocument.AddParty(sqlActorAncestor.XmlId);
                                Tools.SetParty(party, sqlActorAncestor, cciTrade.DataDocument.IsEfsMLversionUpperThenVersion2);
                                //
                                // 2- Ajouter un élément partyTradeIdentifier dans le DataDocument
                                PartyTradeIdentifier(sqlActorAncestor.XmlId);
                                //
                                // 3- Ajouter un élément partyTradeInformation dans le DataDocument
                                PartyTradeInformation(sqlActorAncestor.XmlId);
                                //  
                                // 4- Ajouter un élément brokerPartyReference dans le DataDocument
                                cciTrade.DataDocument.AddBroker(sqlActorAncestor.XmlId);
                                //
                                // 5- Associer cette Entity à la Party en cour comme étant brokerPartyReference
                                IPartyTradeInformation partyTradeInfo = cciTrade.DataDocument.GetPartyTradeInformation(pSqlActor.XmlId);
                                //FI 20140407 [XXXXX] CreatePartyTradeInformation si partyTradeInfo is null
                                if (null == partyTradeInfo)
                                    partyTradeInfo = PartyTradeInformation(pSqlActor.XmlId);
                                cciTrade.DataDocument.AddBrokerPartyReference(partyTradeInfo, sqlActorAncestor.XmlId);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        ///  Alimente le partyTradeInfo.brokerPartyReference de l'acteur parent avec le broker {pParty_Id}
        /// </summary>
        /// <param name="pPartyId"></param>
        /// FI 20131205 [19304] Add Method
        private void AddBrokerOfParty(string pPartyId)
        {
            if (false == (IsPartyBroker & IsBrokerOfParty))
                throw new NotSupportedException("AddBrokerOfParty method is valid on Party:PartyType.broker only");

            cciTrade.DataDocument.AddBroker(pPartyId);
            IPartyTradeInformation partyTradeInfo = cciTrade.DataDocument.GetPartyTradeInformation(cciBrokerParent.GetPartyId());
            partyTradeInfo.BrokerPartyReference[number - 1].HRef = pPartyId;
            //FI 20091123 [16751] => nécessaire à l'amentation de tradeSide, brokerPartyReferenceSpecified est par ailleurs aimenté à chaque post (voir CciTradeParty_Dump)
            partyTradeInfo.BrokerPartyReferenceSpecified = true;
        }

        /// <summary>
        ///  Alimentation de PartyTradeInformation et PartyTradeIdentifier
        /// </summary>
        /// <param name="pPartyId"></param>
        /// FI 20131205 [19304] Add Method
        private void SetPartyTradeInformationAndPartyTradeIdentifier(string pPartyId)
        {
            PartyTradeIdentifier(pPartyId);
            PartyTradeInformation(pPartyId);
            // l'alimentation de partyTradeInfo doit être effectué après Initialize_FromCci 
            //(cet dernière crée  partyTradeInfo s'il n'existe pas 
            if (IsPartyBroker && IsBrokerOfParty)
                AddBrokerOfParty(pPartyId);
        }

        /// <summary>
        /// Dump UTI 
        /// </summary>
        /// <param name="pCci">Représente la donnée présente dans le cci partyTradeIdentifier_UTI_value</param>
        /// FI 20140204 [19564] add method
        private void DumpUTI_ToDocument(CustomCaptureInfo pCci)
        {
            IPartyTradeIdentifier partyTradeIdentifier = PartyTradeIdentifier();
            if (null != partyTradeIdentifier)
            {
                SetTradeId(partyTradeIdentifier, Cst.OTCml_TradeIdUTISpheresScheme, pCci);
                // FI 20240425[26593]
                // When a UIT is input manualy, the source is null
                if (partyTradeIdentifier.TradeIdSpecified)
                {
                    ITradeId uti = partyTradeIdentifier.TradeId.Where(x => x.Scheme == Cst.OTCml_TradeIdUTISpheresScheme).FirstOrDefault();
                    if (null != uti)
                        uti.Source = null;
                }
            }
        }

        /// <summary>
        /// Alimente le tradeId présent dans {partyTradeIdentifier} dont le scheme est {pScheme} avec la valeur présente dans {pCci} 
        /// <para>Ajoute un tradeId si le scheme n'est pas présent et si la donnée présente dans {pCci} est renseignée</para>
        /// <para>Remplace le tradeId si le scheme est déjà présent et si la donnée présente dans {pCci} est renseignée</para>
        /// <para>Supprime le tradeId si le scheme est présent et si la donnée présente dans {pCci} est non renseignée</para>
        /// </summary>
        /// <param name="partyTradeIdentifier"></param>
        /// <param name="pScheme"></param>
        /// <param name="pCci"></param>
        /// FI 20140204 [19564] add method
        private static void SetTradeId(IPartyTradeIdentifier partyTradeIdentifier, string pScheme, CustomCaptureInfo pCci)
        {
            if (null == partyTradeIdentifier)
                throw new ArgumentNullException("Argument partyTradeIdentifier is null");

            if (null == pCci)
                throw new ArgumentNullException("Argument pCci is null");

            ISchemeId tradeId = null;
            if (StrFunc.IsFilled(pCci.LastValue))
            {
                if (StrFunc.IsFilled(pCci.NewValue))
                {
                    tradeId = partyTradeIdentifier.GetTradeIdFromScheme(pScheme);
                }
                else
                {
                    //Find LastValue and remove
                    #region Remove tradeId
                    int arrCounter = 0;
                    foreach (ISchemeId tId in partyTradeIdentifier.TradeId)
                    {
                        if (pScheme == tId.Scheme)
                        {
                            ReflectionTools.RemoveItemInArray(partyTradeIdentifier, partyTradeIdentifier.GetTradeIdMemberName(), arrCounter);
                            break;
                        }
                        arrCounter++;
                    }
                    #endregion Remove tradeId
                }
            }
            else if (StrFunc.IsFilled(pCci.NewValue))
            {
                tradeId = partyTradeIdentifier.GetTradeIdFromScheme(pScheme);
                if (null == tradeId)
                {
                    tradeId = partyTradeIdentifier.GetTradeIdWithNoScheme();
                    if (null == tradeId)
                    {
                        ReflectionTools.AddItemInArray(partyTradeIdentifier, partyTradeIdentifier.GetTradeIdMemberName(), 0);
                        tradeId = partyTradeIdentifier.TradeId[partyTradeIdentifier.TradeId.Length - 1];
                    }
                }
            }
            partyTradeIdentifier.TradeIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.TradeId);
            //
            if (StrFunc.IsFilled(pCci.NewValue))
            {
                partyTradeIdentifier.TradeIdSpecified = true;
                tradeId.Value = pCci.NewValue;
                tradeId.Scheme = pScheme;
                tradeId.Id = null;
            }
        }

        /// <summary>
        /// Récupère l'issuer et l'identifiant du trade à partir tradeId dédié au stockage des UTI
        /// </summary>
        /// <param name="partyTradeIdentifier"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        /// FI 20140204 [19564] add method
        /// FI 20240425 [26593] UTI/PUTI REFIT
        /// FI 20240425 [26593] From now on, when the UTI is external to Spheres®, it is considered that the first 20 characters represent the issuer.
        private string ExtractFromTradeIdUTI(IPartyTradeIdentifier partyTradeIdentifier, UTIElement element)
        {
            string ret = string.Empty;

            if ((null != partyTradeIdentifier) && (ArrFunc.IsFilled(partyTradeIdentifier.TradeId)))
            {
                ITradeId tIdUTI = partyTradeIdentifier.TradeId.Where(x => x.Scheme == Cst.OTCml_TradeIdUTISpheresScheme).FirstOrDefault();

                if ((null != tIdUTI)  && StrFunc.IsFilled(tIdUTI.Value))
                {
                    int lenIssuer = 20; // default Value
                    if (StrFunc.IsFilled(tIdUTI.Source)) // Source exists when UTI is calculated By Spheres® only. When then UTI is imported source is Empty. Spheres®  considers that the first 20 characters are the issuer (like REFIT)
                    {
                        if (tIdUTI.Source.EndsWith("REFIT"))
                            lenIssuer = 20;
                        else if (tIdUTI.Source == "EUREX_L2_C7_3")
                            lenIssuer = 11;
                        else
                            lenIssuer = 10;
                    }

                    switch (element)
                    {
                        case UTIElement.issuer:
                            if (tIdUTI.Value.Length >= lenIssuer)
                                ret = tIdUTI.Value.Substring(0, lenIssuer);
                            else
                                ret = tIdUTI.Value;
                            break;
                        case UTIElement.tradeId:
                            if (tIdUTI.Value.Length > lenIssuer)
                                ret = tIdUTI.Value.Substring(lenIssuer);
                            break;
                        default:
                            throw new NotImplementedException($"{element} is not implemented");
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// set cci partyTradeIdentifier_UTI_Value from ccis partyTradeIdentifier_UTI_issuer and partyTradeIdentifier_UTI_identifier
        /// </summary>
        /// FI 20140204 [19564] add method
        private void SynchroCciUTIValue(CustomCaptureInfo pCCI)
        {
            CustomCaptureInfo cciIssuer = Cci(CciEnum.partyTradeIdentifier_UTI_issuer);
            CustomCaptureInfo cciIdentifier = Cci(CciEnum.partyTradeIdentifier_UTI_identifier);
            CustomCaptureInfo cciValue = Cci(CciEnum.partyTradeIdentifier_UTI_value);

            if (null == cciIssuer)
                throw new ArgumentNullException(StrFunc.AppendFormat("cci: {0} is null", CciEnum.partyTradeIdentifier_UTI_issuer.ToString()));
            if (null == cciIdentifier)
                throw new ArgumentNullException(StrFunc.AppendFormat("cci: {0} is null", CciEnum.partyTradeIdentifier_UTI_identifier.ToString()));
            if (null == cciValue)
                throw new ArgumentNullException(StrFunc.AppendFormat("cci: {0} is null", CciEnum.partyTradeIdentifier_UTI_value.ToString()));


            if (IsCci(CciEnum.partyTradeIdentifier_UTI_issuer, pCCI) || IsCci(CciEnum.partyTradeIdentifier_UTI_identifier, pCCI))
            {
                if (cciIssuer.IsEmpty && cciIdentifier.IsEmpty)
                {
                    cciValue.NewValue = string.Empty;
                }
                else
                {
                    cciValue.NewValue = cciIssuer.NewValue + cciIdentifier.NewValue;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140204 [19564] add method
        /// FI 20170928 [23452] Modify
        private void ResetCciUTI()
        {
            IEnumerable<CciEnum> lst = CciTools.GetCciEnum<CciEnum>("UTI");
            foreach (CciEnum item in lst)
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add 
        private void ResetCciNCM()
        {
            bool defaultValue = true;
            if (IsAllocationAndClearerParty)
                defaultValue = false;

            IEnumerable<CciEnum> cciEnumNCM = CciTools.GetCciEnum<CciEnum>("NCM");
            foreach (CciEnum item in cciEnumNCM)
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset(defaultValue.ToString().ToLower());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCCI"></param>
        /// FI 20170928 [23452] Modify 
        private void Dump_ExecutionOrInvestmentWithinFirm_ToDocument(CustomCaptureInfo pCci, string pData, RoleActor pRole)
        {
            if (false == ((pRole == RoleActor.EXECUTION) || (pRole == RoleActor.INVESTDECISION)))
                throw new ArgumentException(StrFunc.AppendFormat("Role :{0} is not supported"), pRole.ToString());


            IParty party = GetParty();
            IPartyTradeInformation partyTradeInformation = GetPartyTradeInformation();

            if (null != party && party.OTCmlId > 0 && null != partyTradeInformation)
            {
                Nullable<int> idB = cciTrade.DataDocument.GetOTCmlId_Book(party.Id);
                Nullable<int> idAEntity = idB.HasValue ? BookTools.GetEntityBook(cciTrade.CSCacheOn, idB) : new Nullable<int>();


                if (null != pCci.LastSql_Table)
                {
                    if (((SQL_Actor)pCci.LastSql_Table).IsAlgo)
                    {
                        // Suppression de l'algorithme précédent portant le rôle {pRole}
                        cciTrade.DataDocument.RemoveAlgorithm(party, pRole);
                    }
                    else
                    {
                        // Suppression de la personne précédente portant le rôle {pRole}
                        cciTrade.DataDocument.RemoveRelatedPerson(party, pRole);

                        // Suppression de la personne précédente si elle n'est plus utilisée en tant que relatedPerson
                        string lastPersonId = ((SQL_Actor)pCci.LastSql_Table).XmlId;

                        IRelatedPerson person = (partyTradeInformation.RelatedPersonSpecified) ?
                            partyTradeInformation.RelatedPerson.Where(x => x.PersonReference.HRef == lastPersonId).FirstOrDefault() : null;

                        if (null == person)
                            cciTrade.DataDocument.RemovePerson(party, lastPersonId);
                    }
                }

                if (pCci.IsFilled)
                {
                    CciTools.IsActorValid(cciTrade.CSCacheOn, pData, out SQL_Actor sql_Actor, out bool isLoaded, out bool isFound, false, new RoleActor[] { pRole }, CcisBase.User, CcisBase.SessionId);

                    if (isFound)
                    {
                        #region Party found
                        isFound = false;

                        bool isClientUnderDecisionMaker = BookTools.IsCounterPartyClientUnderDecisionMaker(cciTrade.CSCacheOn, party.OTCmlId, idB);
                        bool IsClient = BookTools.IsCounterPartyClient(cciTrade.CSCacheOn, party.OTCmlId, idB);
                        switch (pRole)
                        {
                            case RoleActor.INVESTDECISION:
                                if (true)
                                {
                                    var lst = ActorTools.LoadSalesAlgo(cciTrade.CSCacheOn, party.OTCmlId, CcisBase.User, CcisBase.SessionId);
                                    var lst2 = ActorTools.LoadSalesAlgo(cciTrade.CSCacheOn, idAEntity, CcisBase.User, CcisBase.SessionId);
                                    isFound = ((lst.Concat(lst2)).Where(x => x.Item2 == sql_Actor.Identifier).Count() > 0);
                                }
                                break;
                            case RoleActor.EXECUTION:
                                if (isClientUnderDecisionMaker && idAEntity > 0)
                                {
                                    var lst = ActorTools.LoadTraderAlgo(cciTrade.CSCacheOn, idAEntity.Value, CcisBase.User, CcisBase.SessionId);
                                    isFound = (lst.Where(x => x.Item2 == sql_Actor.Identifier).Count() > 0);
                                }
                                else // client ou House
                                {
                                    if (idAEntity.HasValue)
                                    {
                                        var lst = ActorTools.LoadTraderAlgo(cciTrade.CSCacheOn, idAEntity.Value, CcisBase.User, CcisBase.SessionId);
                                        var lst2 = ActorTools.LoadTraderAlgo(cciTrade.CSCacheOn, party.OTCmlId, CcisBase.User, CcisBase.SessionId);
                                        isFound = ((lst.Concat(lst2)).Where(x => x.Item2 == sql_Actor.Identifier).Count() > 0);
                                    }
                                    else
                                    {
                                        var lst = ActorTools.LoadTraderAlgo(cciTrade.CSCacheOn, party.OTCmlId, CcisBase.User, CcisBase.SessionId);
                                        isFound = (lst.Where(x => x.Item2 == sql_Actor.Identifier).Count() > 0);
                                    }
                                }
                                break;
                        }

                        if (isFound)
                        {
                            pCci.NewValue = sql_Actor.Identifier;
                            pCci.Sql_Table = sql_Actor;
                            pCci.ErrorMsg = string.Empty;

                            if (sql_Actor.IsAlgo)
                                cciTrade.DataDocument.AddAlgorithm(party, sql_Actor, pRole);
                            else
                                cciTrade.DataDocument.AddRelatedPerson(party, sql_Actor, pRole);
                        }
                        else
                        {
                            pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorAlgoNotFound"), pCci.NewValue);
                        }
                        #endregion Party found
                    }
                    else
                    {
                        #region Party is NOT found in Database
                        pCci.ErrorMsg = string.Empty;
                        pCci.Sql_Table = null;
                        if (pCci.IsFilled)
                        {
                            if (isLoaded)
                                pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorAlgoNotUnique"), pCci.NewValue);
                            else
                                pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_ActorAlgoNotFound"), pCci.NewValue);
                        }
                        #endregion Party is NOT found in Database
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pSqlActor"></param>
        /// FI 20170928 [23452] Add
        private void Initialize_ExecutionOrInvestmentWithinFirm_FromDocument(out string pData, out SQL_Table pSQLTable, RoleActor pRoleActor)
        {
            pData = string.Empty;
            pSQLTable = null;

            IPartyTradeInformation partyTradeInformation = cciTrade.DataDocument.GetPartyTradeInformation(GetPartyId(Ccis.TradeCommonInput.TradeStatus.IsStActivation_Missing));
            if (null != partyTradeInformation)
            {
                IAlgorithm algo = (partyTradeInformation.AlgorithmSpecified) ?
                                partyTradeInformation.Algorithm.Where(x => x.Role.Value ==
                                RoleActorTools.ConvertToFpmL<AlgorithmRoleEnum>(pRoleActor).ToString()).FirstOrDefault() : null;

                IRelatedPerson relatedPerson = (partyTradeInformation.RelatedPersonSpecified) ?
                                partyTradeInformation.RelatedPerson.Where(x => x.Role.Value ==
                                RoleActorTools.ConvertToFpmL<PersonRoleEnum>(pRoleActor).ToString()).FirstOrDefault() : null;

                SQL_Actor sqlActor = null;
                if (null != algo)
                {
                    pData = algo.Name;
                    sqlActor = new SQL_Actor(cciTrade.CSCacheOn, algo.OTCmlId);
                }
                else if (null != relatedPerson)
                {
                    IPerson person = cciTrade.DataDocument.GetPerson(relatedPerson.PersonReference.HRef);
                    if (person.PersonIdSpecified)
                    {
                        IScheme scheme = person.PersonId.Where(x => x.Scheme == Cst.OTCml_ActorIdentifierScheme).FirstOrDefault();
                        if (null != scheme)
                            pData = scheme.Value;
                    }
                    else if (person.FirstNameSpecified)
                        pData = person.FirstName;
                    else if (person.SurnameSpecified)
                        pData = person.Surname;

                    sqlActor = new SQL_Actor(cciTrade.CSCacheOn, person.OTCmlId);
                }

                if ((null != sqlActor) && sqlActor.IsLoaded)
                {
                    pData = sqlActor.Identifier;
                    pSQLTable = sqlActor;
                }
            }
        }

        /// <summary>
        /// Retourne true si l'array de scheme est renseigné
        /// <para>Retourne true si au minimum un item est renseigné (value + scheme)</para>
        /// </summary>
        /// <param name="pScheme"></param>
        /// <returns></returns>
        private static Boolean IsFilledScheme(IScheme[] pScheme)
        {
            if (null == pScheme)
                throw new ArgumentNullException("pScheme is null");


            return (pScheme.Where(x => StrFunc.IsFilled(x.Value) && StrFunc.IsFilled(x.Scheme)).Count() > 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Add
        /// FI 20170928 [23452] Modify
        private void ResetCciMiFIR()
        {
            IEnumerable<CciEnum> cciEnum = CciTools.GetCciEnum<CciEnum>("MiFIR");
            foreach (CciEnum item in cciEnum)
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            }

        }


        /// <summary>
        ///  pré-proposition post saisie d'un actor
        /// </summary>
        /// <param name="pCCi"></param>
        /// FI 20170928 [23452] Add
        private void ProcessInitializeActor(CustomCaptureInfo pCci)
        {
            SQL_Actor sql_Actor = (SQL_Actor)pCci.Sql_Table;
            SQL_Actor sql_ActorLast = (SQL_Actor)pCci.LastSql_Table;
            string newValue = pCci.NewValue;
            string lastValue = pCci.LastValue;
            bool isActorValid = pCci.IsFilledValue && (null != sql_Actor);

            if (null != sql_Actor)
                newValue = sql_Actor.XmlId;

            if (null != sql_ActorLast)
                lastValue = sql_ActorLast.XmlId;

            if (partyType == PartyType.party)
                cciTrade.SynchronizePayerReceiver(lastValue, newValue);
            else
                cciTrade.SynchronizePayerReceiverOtherPartyPayment(lastValue, newValue);

            cciTrade.SetClientIdDefaultReceiverOtherPartyPayment();
            cciTrade.SynchronizeParty(pCci);


            CustomCaptureInfo bookCci = Cci(CciEnum.book);
            bool isResetBroker = true;
            //
            if (isActorValid)
            {
                bool isBookFilled = (null != bookCci) && (null != bookCci.Sql_Table); // si true, Il existe dejà un book
                bool isFindDefaultBook = (null != bookCci) && (false == isBookFilled);// si true, active la recherche du book par défaut
                //
                if (isBookFilled)
                {
                    int IdBook = ((SQL_Book)bookCci.Sql_Table).Id;
                    SQL_Book sqlbook = new SQL_Book(CSTools.SetCacheOn(cciTrade.CS), SQL_TableWithID.IDType.Id, IdBook.ToString(), SQL_Table.ScanDataDtEnabledEnum.Yes, sql_Actor.Id);
                    isFindDefaultBook = (false == sqlbook.IsFound);
                }
                //
                if (isFindDefaultBook)
                {
                    bool isOk = false;
                    //
                    bookCci.Reset();
                    //
                    if ((PartyType.party == partyType) && (IsInitFromPartyTemplate))
                    {
                        //20081124 FI Recherche du book en fonction du parametrage PartyTemplates
                        isOk = SetCciBookFromPartyTemplate();
                    }
                    //
                    //Si la recherche depuis PARTYTEMPALTE n'aboutie pas => on recherche l'unique book associé à la partie 
                    if (false == isOk)
                    {
                        SQL_Book sqlbook = new SQL_Book(cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, "%", SQL_Table.ScanDataDtEnabledEnum.Yes, sql_Actor.Id);
                        if ((sqlbook.IsLoaded) && (sqlbook.Dt.Rows.Count == 1))
                            bookCci.NewValue = sqlbook.Identifier;
                    }
                }
                //
                if (PartyType.party == partyType)
                {
                    //Purge du broker sauf si le book existe déjà (isBookFilled =true) et qu'il est valide(isFindDefaultBook= false)
                    //Dans ce cas le broker a peut-être été initialisé à partir du book, Il ne faut pas le supprimer
                    //Rappel la saisie du book peut pre-preposé en cascade l'acteur et le broker
                    isResetBroker = (false == (isBookFilled && (false == isFindDefaultBook)));
                    //
                    //20081124 FI Pour rentrer dans DumpBook afin de mettre à jour partyTradeIdentifier
                    //Pour initialiser trader, Sales depuis PartyTemplate
                    if (IsInitFromPartyTemplate)
                    {
                        if ((isBookFilled) && (bookCci.NewValue == bookCci.LastValue))
                            bookCci.LastValue = ".";
                    }
                }
            }
            // EG 20150907 [21317]
            //else if (StrFunc.IsEmpty(newValue) && (null != bookCci)) // 20091007 RD/ On efface le Book uniquement si on a effacé l'Acteur
            else if (null != bookCci) // 20091007 RD/ On efface le Book uniquement si on a effacé l'Acteur
            {
                bookCci.NewValue = string.Empty;
            }

            CustomCaptureInfo cci = Cci(CciEnum.folder);
            if (null != cci)
                cci.Reset();

            cci = Cci(CciEnum.frontId);
            if (null != cci)
                cci.Reset();

            // RD 20091228 [16809] Confirmation indicators for each party
            if (this.partyType == PartyType.party)
            {
                cci = Cci(CciEnum.localClassDerv);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.localClassNDrv);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.iasClassDerv);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.iasClassNDrv);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.fxClass);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.hedgeClassDerv);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.hedgeClassNDrv);
                if (null != cci)
                    cci.Reset();
                cci = Cci(CciEnum.hedgeFolder);
                if (null != cci)
                    cci.Reset();

                // FI 20140204 [19564] Gestion UTI
                ResetCciUTI();

                ResetCciNCM();

                cci = Cci(CciEnum.initiatorReactor);
                if (null != cci)
                    cci.Reset();

                ResetCciMiFIR();

                SetIsMandatoryOnClass();
            }

            for (int i = 0; i < TraderLength; i++)
                cciTrader[i].Clear();

            for (int i = 0; i < SalesLength; i++)
                cciSales[i].Clear();

            if (isResetBroker)
            {
                for (int i = 0; i < BrokerLength; i++)
                    cciBroker[i].Cci(CciEnum.actor).NewValue = string.Empty;
            }
        }

        /// <summary>
        ///  pré-proposition post saisie d'un book
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20170928 [23452] Add
        private void ProcessInitializeBook(CustomCaptureInfo pCci)
        {
            SQL_Book sql_Book = (SQL_Book)pCci.Sql_Table;
            if (this.partyType == PartyType.party)
            {
                #region  Préproposition de Class
                string localClassDerv, iasClassDerv, hedgeClassDerv, hedgeFolder, hedgeFactor;
                string localClassNDrv, iasClassNDrv, hedgeClassNDrv;
                string fxClass;
                localClassDerv = iasClassDerv = hedgeClassDerv = hedgeFolder = hedgeFactor = localClassNDrv = iasClassNDrv = hedgeClassNDrv = fxClass = string.Empty;
                bool isIFRS = Ccis.TradeCommonInput.SQLInstrument.IsIFRS;
                //
                if (null != sql_Book)
                {
                    if (Ccis.TradeCommonInput.SQLProduct.IsDerivative)
                    {
                        //Derivative
                        localClassDerv = sql_Book.LocalClassDerv;
                        if (isIFRS)
                        {
                            iasClassDerv = sql_Book.IASClassDerv;
                            hedgeClassDerv = sql_Book.HedgeClassDerv;
                        }
                    }
                    else if (Ccis.TradeCommonInput.SQLProduct.IsFxAndNotOption)
                    {
                        //Fx
                        fxClass = sql_Book.FxClass;
                    }
                    else if (false == Ccis.TradeCommonInput.SQLProduct.IsBulletPayment)
                    {
                        //No Derivative
                        localClassNDrv = sql_Book.LocalClassNDrv;
                        if (isIFRS)
                        {
                            iasClassNDrv = sql_Book.IASClassNDrv;
                            hedgeClassNDrv = sql_Book.HedgeClassNDrv;
                        }
                    }
                    //
                    //hedgeFolder = sql_Book.HedgeFolder;
                    //hedgeFactor = sql_Book.HedgeFactor;
                }
                //
                SetIsMandatoryOnClass();
                //
                CcisBase.SetNewValue(CciClientId(CciEnum.localClassDerv), localClassDerv);
                CcisBase.SetNewValue(CciClientId(CciEnum.iasClassDerv), iasClassDerv);
                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeClassDerv), hedgeClassDerv);
                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeFolder), hedgeFolder);
                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeFactor), hedgeFactor);
                CcisBase.SetNewValue(CciClientId(CciEnum.localClassNDrv), localClassNDrv);
                CcisBase.SetNewValue(CciClientId(CciEnum.iasClassNDrv), iasClassNDrv);
                CcisBase.SetNewValue(CciClientId(CciEnum.hedgeClassNDrv), hedgeClassNDrv);
                CcisBase.SetNewValue(CciClientId(CciEnum.fxClass), fxClass);

                ResetCciMiFIR();

                #endregion  Préproposition de Class
            }
        }

        /// <summary>
        /// Reset des Ccis suite à modification de la plateforme
        /// </summary>
        // EG 20171113 [23509] New
        public void ResetCciFacilityHasChanged()
        {
            List<CciEnum> lst = CciTools.GetCciEnum<CciEnum>("FacilityHaschanged").ToList();
            lst.ForEach(item =>
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            });
        }

        /// <summary>
        ///  Initialisation de bookCci.NewValue à partir des paramétrages existants sous PARTYTEMPLATE
        /// <para>Retourne true lorsque l'initialisation s'est réellement effectuée</para>
        /// </summary>
        /// FI 20230927 [XXXXX][WI714] Refactoring ce cette méthode
        private Boolean SetCciBookFromPartyTemplate()
        {
            bool isOk;

            // INSTRUMENT
            int idI = cciTrade.Product.IdI;

            // ACTOR
            int idA = GetActorIda();
            isOk = (idA > 0);

            // MARKET
            int idM = 0;
            if (isOk)
            {
                if (cciTrade.TradeCommonInput.SQLProduct.IsLSD || cciTrade.TradeCommonInput.SQLProduct.IsESE || cciTrade.TradeCommonInput.SQLProduct.IsCOMS)
                {
                    cciTrade.DataDocument.CurrentProduct.GetMarket(cciTrade.CSCacheOn, null, out SQL_Market sqlMarket);
                    if (null != sqlMarket)
                        idM = sqlMarket.Id;
                }
            }

            if (isOk)
            {
                PartyTemplate partyTemplateFind = PartyTemplates.FindBook(cciTrade.CSCacheOn, idA, idI, idM);
                isOk = (null != partyTemplateFind);
                if (isOk)
                {
                    if (partyTemplateFind.idB > 0)
                    {
                        CustomCaptureInfo bookCci = Cci(CciEnum.book);
                        if (null != bookCci)
                            bookCci.NewValue = partyTemplateFind.bookIdentifier;
                    }
                }
            }
            return isOk;
        }
    }
}
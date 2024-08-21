#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    ///  Tronc Commun (Facturation, Trades de marché, trade déposit , Reférentiel Titre)
    /// </summary>
    /// FI 20170928 [23452] Modify
    public abstract class CciTradeCommonBase : ITradeCci, IContainerCciFactory, IContainerCciPayerReceiver, IContainerCciGetInfoButton, IContainerCciQuoteBasis, ICciPresentation
    {
        #region Members
        public CciTradeHeader cciTradeHeader;
        public CciTradeParty[] cciParty;
        public CciTradeParty[] cciBroker;
        public CciProductBase cciProduct; //attention n'est pas alimentée dans les factures
        public CciTradeNotification cciTradeNotification;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient le facility
        /// </summary>
        /// EG 20171031 [23509] New
        /// FI 20190716 [XXXXX] Rename CciMarketParty => CciFacilityParty
        public virtual CciMarketParty CciFacilityParty
        {
            get { return null; }
        }

        /// <summary>
        /// Obtient la collection des ccis de TradeCommonInput
        /// </summary>
        public TradeCommonCustomCaptureInfos Ccis
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la ConnectionString avec les éventuels Spheres attributs
        /// </summary>
        public string CS
        {
            get { return Ccis.CS; }
        }

        /// <summary>
        /// Obtient la ConnectionString avec l'attribut SpheresCache à true
        ///<para>
        /// <remarks>Le cache ne sera effectif que s'il est activé (voir le fichier Config)</remarks> 
        ///</para>
        /// </summary>
        public string CSCacheOn
        {
            get { return CSTools.SetCacheOn(Ccis.CS); }
        }

        /// <summary>
        /// Obtient TradeCommonInput
        /// </summary>
        public TradeCommonInput TradeCommonInput
        {
            get { return Ccis.TradeCommonInput; }
        }

        /// <summary>
        /// Obtient DataDocument du TradeCommonInput
        /// </summary>
        public DataDocumentContainer DataDocument
        {
            get { return TradeCommonInput.DataDocument; }
        }


        /// <summary>
        /// Obtient ProductContainer du DataDocument.product
        /// </summary>
        public ProductContainer Product
        {
            get { return TradeCommonInput.DataDocument.CurrentProduct; }
        }


        /// <summary>
        /// Obtient le currentTrade du DataDocument
        /// </summary>
        public ITrade CurrentTrade
        {
            get { return DataDocument.CurrentTrade; }
        }


        /// <summary>
        /// Obtient le true si le trade est un Template
        /// </summary>
        public bool IsStEnvTemplate
        {
            get { return TradeCommonInput.TradeStatus.IsStEnvironment_Template; }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IInvoice Invoice
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IInvoiceSettlement InvoiceSettlement
        {
            get { return null; }
        }


        /// <summary>
        /// 
        /// </summary>
        public int BrokerLength
        {
            get { return ArrFunc.IsFilled(cciBroker) ? cciBroker.Length : 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int PartyLength
        {
            get { return cciParty.Length; }
        }



        /// <summary>
        /// 
        /// </summary>
        public virtual string PrefixHeader
        {
            get { return TradeCommonCustomCaptureInfos.CCst.Prefix_tradeAdminHeader; }
        }
        #endregion Accessors

        #region Constructors
        public CciTradeCommonBase(TradeCommonCustomCaptureInfos pCci)
        {
            Ccis = pCci;
            cciTradeNotification = new CciTradeNotification(Ccis.TradeCommonInput.TradeNotification);
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        /// FI 20170718 [23326] Modify
        /// FI 20170906 [23401] Modify
        /// FI 20170928 [23452] Modify
        public virtual void AddCciSystem()
        {
            if (null != cciProduct)
                cciProduct.AddCciSystem();

            cciTradeHeader.AddCciSystem();

            for (int i = 0; i < PartyLength; i++)
            {
                cciParty[i].AddCciSystem();
            }

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        public virtual void CleanUp()
        {
            if (null != cciProduct)
                cciProduct.CleanUp();

            for (int i = 0; i < PartyLength; i++)
                cciParty[i].CleanUp();

            cciTradeHeader.CleanUp();

            CleanUpBroker();

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        // EG 20171114 [23509] LE DumpPartiesAndBrokers_ToDocument est effectué en 1er
        public virtual void Dump_ToDocument()
        {
            DumpPartiesAndBrokers_ToDocument();

            if (null != cciProduct)
                cciProduct.Dump_ToDocument();

            cciTradeHeader.Dump_ToDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        public virtual int GetArrayElementDocumentCount(string pPrefix, string pParentClientId, int pParentOccurs)
        {

            int ret = -1;
            //
            //if (null != cciProduct)
            //    ret = cciProduct.GetArrayElementDocumentCount(pPrefix, pParentClientId, pParentOccurs);
            //
            if (ret == -1)
            {
                bool isPrefixTrader = (TradeCommonCustomCaptureInfos.CCst.Prefix_trader == pPrefix);
                bool isPrefixSales = (TradeCommonCustomCaptureInfos.CCst.Prefix_sales == pPrefix);
                bool isPrefixBroker = (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pPrefix);
                bool isPrefixBrokerOfParty = (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pPrefix &&
                                             (TradeCommonCustomCaptureInfos.CCst.Prefix_party == pParentClientId));
                //
                if (isPrefixBroker && (false == isPrefixBrokerOfParty))
                    ret = ArrFunc.Count(CurrentTrade.BrokerPartyReference);
                //
                if ((isPrefixTrader || isPrefixSales || isPrefixBrokerOfParty) && (0 < pParentOccurs))
                {
                    ret = 1;
                    string parentReferencehRef = string.Empty;
                    if (ArrFunc.IsFilled(CurrentTrade.TradeHeader.PartyTradeInformation))
                    {
                        if (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pParentClientId)
                            parentReferencehRef = GetBrokerReferencehRef(pParentClientId, pParentOccurs);
                        else if (TradeCommonCustomCaptureInfos.CCst.Prefix_party == pParentClientId)
                            parentReferencehRef = GetCounterpartyReferencehRef(pParentClientId, pParentOccurs);
                        //
                        if (StrFunc.IsFilled(parentReferencehRef))
                        {
                            IPartyTradeInformation partyTradeInformation = DataDocument.GetPartyTradeInformation(parentReferencehRef);
                            if (null != partyTradeInformation)
                            {
                                if (isPrefixTrader)
                                    ret = ArrFunc.Count(partyTradeInformation.Trader);
                                if (isPrefixSales)
                                    ret = ArrFunc.Count(partyTradeInformation.Sales);
                                if (isPrefixBrokerOfParty)
                                    ret = ArrFunc.Count(partyTradeInformation.BrokerPartyReference);
                            }
                        }
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        // EG 20171114 [23509] L'initialisation des parties est faite en 1er
        public virtual void Initialize_Document()
        {
            cciTradeHeader.Initialize_Document();

            for (int i = 0; i < PartyLength; i++)
                cciParty[i].Initialize_Document();

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].Initialize_Document();

            if (null != cciProduct)
                cciProduct.Initialize_Document();

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        public virtual void Initialize_FromCci()
        {
            if (null != cciProduct)
                cciProduct.Initialize_FromCci();
            //
            //La liste des produits pouvant variés en fonction de l'écran (vrai sur les strategies)
            //il convient de synchronizer membre le product du DataDocument
            DataDocument.SetCurrentProduct();

            cciTradeHeader.Initialize_FromCci();

            InitializeParties_FromCci();

            InitializeBroker_FromCci();

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        public virtual void Initialize_FromDocument()
        {
            if (null != cciProduct)
                cciProduct.Initialize_FromDocument();

            cciTradeHeader.Initialize_FromDocument();

            for (int i = 0; i < PartyLength; i++)
                cciParty[i].Initialize_FromDocument();

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].Initialize_FromDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            if (null != cciProduct)
                isOk = cciProduct.IsClientId_PayerOrReceiver(pCci);
            if (false == isOk)
                isOk = cciTradeHeader.IsClientId_PayerOrReceiver(pCci);

            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20170928 [23452] Modify
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (null != cciProduct)
                cciProduct.ProcessInitialize(pCci);

            cciTradeHeader.ProcessInitialize(pCci);

            for (int i = 0; i < PartyLength; i++)
                cciParty[i].ProcessInitialize(pCci);

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].ProcessInitialize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessExecute(CustomCaptureInfo pCci)
        {
            if (null != cciProduct)
                cciProduct.ProcessExecute(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            if (null != cciProduct)
                cciProduct.ProcessExecuteAfterSynchronize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170928 [23452] Modify
        public virtual void RefreshCciEnabled()
        {
            if (null != cciProduct)
                cciProduct.RefreshCciEnabled();

            cciTradeHeader.RefreshCciEnabled();

            for (int i = 0; i < PartyLength; i++)
                cciParty[i].RefreshCciEnabled();

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pOccurs"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        public virtual bool RemoveLastEmptyItemInDocumentArray(string pPrefix, int pOccurs, string pParentClientId, int pParentOccurs)
        {
            bool ret = false;
            //if (null != cciProduct)
            //    ret = cciProduct.RemoveLastEmptyItemInDocumentArray(pPrefix, pOccurs, pParentClientId, pParentOccurs);
            //
            if (false == ret)
            {
                //
                bool isPrefixTrader = (TradeCommonCustomCaptureInfos.CCst.Prefix_trader == pPrefix);
                bool isPrefixSales = (TradeCommonCustomCaptureInfos.CCst.Prefix_sales == pPrefix);
                bool isPrefixBroker = (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pPrefix);
                bool isPrefixBrokerOfParty = (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pPrefix &&
                                             (TradeCommonCustomCaptureInfos.CCst.Prefix_party == pParentClientId));

                //
                if (isPrefixBroker && (false == isPrefixBrokerOfParty))
                    ret = RemoveLastItemInBrokerPartyReferenceArray(PrefixHeader + CustomObject.KEY_SEPARATOR + pPrefix, true);

                if (isPrefixTrader || isPrefixSales || isPrefixBrokerOfParty)
                    ret = RemoveLastItemInPartyTradeInformationElementArray(pPrefix, pOccurs, pParentClientId, pParentOccurs);
            }
            //
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public virtual void RemoveLastItemInArray(string pPrefix)
        {
            RemoveLastItemInBrokerPartyReferenceArray(pPrefix);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20170928 [23452] Modify
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != cciProduct)
                cciProduct.SetDisplay(pCci);

            cciTradeHeader.SetDisplay(pCci);

            for (int i = 0; i < PartyLength; i++)
                cciParty[i].SetDisplay(pCci);

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].SetDisplay(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20170928 [23452] Modify
        public virtual void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            for (int i = 0; i < PartyLength; i++)
                cciParty[i].DumpSpecific_ToGUI(pPage);

            for (int i = 0; i < BrokerLength; i++)
                cciBroker[i].DumpSpecific_ToGUI(pPage);

            if (null != cciProduct)
                cciProduct.DumpSpecific_ToGUI(pPage);
        }
        #endregion IContainerCciFactory Members

        #region ITradeCci Members
        /// <summary>
        /// Obtient le cci qui gère la devise principale du trade
        /// </summary>
        public virtual string CciClientIdMainCurrency
        {
            get
            {
                string ret = string.Empty;
                if (null != cciProduct)
                    ret = cciProduct.CciClientIdMainCurrency;
                return ret;
            }
        }

        /// <summary>
        /// Obtient la devise par défaut (lecture du fichier Config)
        /// </summary>
        /// <returns></returns>
        public virtual string GetMainCurrency
        {
            get
            {
                return GetDefaultCurrency();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string RetSidePayer
        {
            get
            {
                string ret;
                if (null != cciProduct)
                    ret = cciProduct.RetSidePayer;
                else
                    ret = SideTools.RetBuySide();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string RetSideReceiver
        {
            get
            {
                string ret;
                if (null != cciProduct)
                    ret = cciProduct.RetSideReceiver;
                else
                    ret = SideTools.RetSellSide();
                return ret;
            }
        }
        #endregion ITradeCci Members

        #region IContainerCciGetInfoButton Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <returns></returns>
        public virtual bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            bool ret = false;
            if (null != cciProduct)
                ret = cciProduct.IsButtonMenu(pCci, ref pCo);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public virtual bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            bool ret = false;
            if (null != cciProduct)
                ret = cciProduct.SetButtonScreenBox(pCci, pCo, ref pIsObjSpecified, ref pIsEnabled);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public virtual bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool ret = false;
            //
            if (null != cciProduct)
                ret = cciProduct.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            //
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public virtual void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (null != cciProduct)
                cciProduct.SetButtonReferential(pCci, pCo);

            for (int i = 0; i < PartyLength; i++)
            {
                bool isOk = false;
                // FI 20200406 [XXXXX] Add particular condition for Partie
                if (cciParty[i].IsCci(CciTradeParty.CciEnum.actor, pCci))
                {
                    isOk = true;
                    if (cciParty[i].IsAllocationAndClearerParty)
                    {
                        if ((this.DataDocument.CurrentProduct.IsExchangeTradedDerivative) || (this.DataDocument.CurrentProduct.IsCommoditySpot))
                            pCo.Condition = "CSS|CLEARER";
                        else
                            pCo.Condition = "CUSTODIAN";
                    }
                    else if (this.DataDocument.CurrentProduct.IsMarginRequirement)// FI 20200512 [XXXXX] condition ENTITY|MARGINREQOFFICE
                        pCo.Condition = "ENTITY|MARGINREQOFFICE";
                }
                else if (cciParty[i].IsCci(CciTradeParty.CciEnum.book, pCci))
                {
                    isOk = true;
                    pCo.Fk = null;

                    if (cciParty[i].Cci(CciTradeParty.CciEnum.actor).Sql_Table is SQL_Actor sql_Actor)
                        pCo.Fk = sql_Actor.Id.ToString(); //=> uniquement les book de l'acteur

                    if (isOk)
                        break;
                }
                else if (ArrFunc.IsFilled(cciParty[i].cciBroker))
                {
                    for (int j = 0; j < cciParty[i].BrokerLength; j++)
                    {
                        if (cciParty[i].cciBroker[j].IsCci(CciTradeParty.CciEnum.actor, pCci))
                        {
                            isOk = true;
                            if (false == cciParty[i].IsAllocationAndClearerParty)
                                pCo.Condition = "BROKER|ENTITY"; // FI 20200513 [XXXXX] condition BROKER|ENTITY 
                        }
                        else if (cciParty[i].cciBroker[j].IsCci(CciTradeParty.CciEnum.book, pCci))
                        {
                            isOk = true;
                            pCo.Fk = null;
                            if (cciParty[i].cciBroker[j].Cci(CciTradeParty.CciEnum.actor).Sql_Table is SQL_Actor sql_Actor)
                                pCo.Fk = sql_Actor.Id.ToString(); //=> uniquement les book de l'acteur
                        }
                        if (isOk)
                            break;
                    }
                }
                if (isOk)
                    break;
            }
        }

        #endregion ITradeGetInfoButton Members

        #region IContainerCciPayerReceiver Members
        #region public virtual CciClientIdPayer
        public virtual string CciClientIdPayer
        {
            get
            {
                string ret = string.Empty;
                //
                if (null != cciProduct)
                    ret = cciProduct.CciClientIdPayer;
                //
                if (StrFunc.IsEmpty(ret))
                {
                    for (int i = 0; i < this.PartyLength; i++)
                    {
                        if (cciParty[i].Cci(CciTradeParty.CciEnum.side).NewValue == this.RetSidePayer)
                        {
                            ret = cciParty[i].CciClientId(CciTradeParty.CciEnum.actor);
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion CciClientIdPayer
        #region public virtual CciClientIdReceiver
        public virtual string CciClientIdReceiver
        {
            get
            {
                string ret = string.Empty;
                //
                if (null != cciProduct)
                    ret = cciProduct.CciClientIdReceiver;
                //
                if (StrFunc.IsEmpty(ret))
                {

                    for (int i = 0; i < this.PartyLength; i++)
                    {
                        if (cciParty[i].Cci(CciTradeParty.CciEnum.side).NewValue == this.RetSideReceiver)
                        {
                            ret = cciParty[i].CciClientId(CciTradeParty.CciEnum.actor);
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion CciClientIdReceiver
        #region public virtual SynchronizePayerReceiver
        public virtual void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            if (null != cciProduct)
                cciProduct.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion SynchronizePayerReceiver
        #region public virtual SynchronizeParty
        public virtual void SynchronizeParty(CustomCaptureInfo pCci)
        {
            if (null != cciProduct)
                cciProduct.SynchronizeParty(pCci);

        }
        #endregion SynchronizeParty
        #endregion IContainerCciPayerReceiver Members

        #region IContainerCciQuoteBasis
        #region public virtual GetCurrency1
        public virtual string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (null != cciProduct)
                ret = cciProduct.GetCurrency1(pCci);
            return ret;

        }
        #endregion GetCurrency1
        #region public virtual GetCurrency2
        public virtual string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (null != cciProduct)
                ret = cciProduct.GetCurrency2(pCci);
            return ret;

        }
        #endregion GetCurrency2
        #region public virtual IsClientId_QuoteBasis
        public virtual bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool ret = false;
            if (null != cciProduct)
                ret = cciProduct.IsClientId_QuoteBasis(pCci);
            return ret;

        }
        #endregion IsClientId_QuoteBasis
        #region public virtual GetBaseCurrency
        public virtual string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            if (null != cciProduct)
                ret = cciProduct.GetBaseCurrency(pCci);
            return ret;

        }
        #endregion GetBaseCurrency
        #endregion

        #endregion Interfaces

        #region Methods
        /// <summary>
        /// Ajoute une party {Unknown} dans le DataDocument
        /// </summary>
        public void AddPartyUnknown()
        {
            if (null == DataDocument.GetParty(TradeCommonCustomCaptureInfos.PartyUnknown))
            {
                ReflectionTools.AddItemInArray(DataDocument.DataDocument, "party", 0);
                IParty party = DataDocument.Party[DataDocument.Party.Length - 1];
                party.Id = TradeCommonCustomCaptureInfos.PartyUnknown;
                party.PartyId = TradeCommonCustomCaptureInfos.PartyUnknown;
            }
        }

        /// <summary>
        /// CleanUp des broker et mise à jour en casacde CurrentTrade.brokerPartyReference 
        /// </summary>
        private void CleanUpBroker()
        {
            if (ArrFunc.IsFilled(cciBroker))
            {
                for (int i = 0; i < cciBroker.Length; i++)
                    cciBroker[i].CleanUp();
            }

            if (ArrFunc.IsFilled(CurrentTrade.BrokerPartyReference))
            {
                for (int i = CurrentTrade.BrokerPartyReference.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(CurrentTrade.BrokerPartyReference[i].HRef))
                        ReflectionTools.RemoveItemInArray(CurrentTrade, "brokerPartyReference", i);
                }
            }
            CurrentTrade.BrokerPartyReferenceSpecified = ArrFunc.IsFilled(CurrentTrade.BrokerPartyReference);

        }

        /// <summary>
        /// Obtient l'identifiant du cci qui gère la party dont l'id vaut pId (cci.ClientId) 
        /// </summary>
        /// <param name="pXmlIdValue"></param>
        /// <returns></returns>
        public virtual string ClientIdFromXmlId(string pId)
        {
            string ret = string.Empty;
            for (int i = 0; i < PartyLength; i++)
            {
                string clientId = cciParty[i].CciClientId(CciTradeParty.CciEnum.actor);
                SQL_Actor sql_actor = (SQL_Actor)Ccis[clientId].Sql_Table;
                if (null != sql_actor)
                {
                    if (sql_actor.XmlId == pId)
                    {
                        ret = clientId;
                        break;
                    }
                }
            }
            if (StrFunc.IsFilled(ret))
            {
                for (int i = 0; i < BrokerLength; i++)
                {
                    string clientId = cciBroker[i].CciClientId(CciTradeParty.CciEnum.actor);
                    SQL_Actor sql_actor = (SQL_Actor)Ccis[clientId].Sql_Table;
                    if (null != sql_actor)
                    {
                        if (sql_actor.XmlId == pId)
                        {
                            ret = clientId;
                            break;
                        }
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// Dump des parties et des brokers
        /// </summary>
        public void DumpPartiesAndBrokers_ToDocument()
        {
            //Parties
            bool partyTradeInformationSpecified = false;
            for (int i = 0; i < PartyLength; i++)
            {
                cciParty[i].Dump_ToDocument();
                if (false == partyTradeInformationSpecified)
                    partyTradeInformationSpecified = CciTools.Dump_IsCciContainerArraySpecified(CurrentTrade.TradeHeader.PartyTradeInformationSpecified, cciParty[i].cciTrader);
                if (false == partyTradeInformationSpecified)
                    partyTradeInformationSpecified = CciTools.Dump_IsCciContainerArraySpecified(CurrentTrade.TradeHeader.PartyTradeInformationSpecified, cciParty[i].cciBroker);
            }
            CurrentTrade.TradeHeader.PartyTradeInformationSpecified = partyTradeInformationSpecified;

            //Brokers
            for (int i = 0; i < BrokerLength; i++)
            {
                cciBroker[i].Dump_ToDocument();
                if (false == partyTradeInformationSpecified)
                    partyTradeInformationSpecified = CciTools.Dump_IsCciContainerArraySpecified(CurrentTrade.TradeHeader.PartyTradeInformationSpecified, cciBroker[i].cciTrader);
            }
            CurrentTrade.BrokerPartyReferenceSpecified = CciTools.Dump_IsCciContainerArraySpecified(CurrentTrade.BrokerPartyReferenceSpecified, cciBroker);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        public virtual void DumpInvoicingScope_ToDocument(CustomCaptureInfo pCci, string pData)
        {

        }

        /// <summary>
        /// Retourne l'OTCmlId d'une contrepartie ou d'un broker
        /// </summary>
        /// <param name="pPartyType"></param>
        /// <param name="pIndex"></param>
        /// <returns></returns>
        public int GetActorIda(CciTradeParty.PartyType pPartyType, int pIndex)
        {
            int ret;
            if (CciTradeParty.PartyType.party == pPartyType)
                ret = cciParty[pIndex].GetActorIda();
            else if (CciTradeParty.PartyType.broker == pPartyType)
                ret = cciBroker[pIndex].GetActorIda();
            else
                throw new NotImplementedException("PartyType not defined");
            return ret;
        }

        /// <summary>
        ///  Retourne les OTCmlId des contreparties 
        /// </summary>
        /// <returns></returns>
        public int[] GetPartyActorIda()
        {
            int[] ret = null;
            ArrayList al = new ArrayList();
            for (int i = 0; i < PartyLength; i++)
            {
                int ida = GetActorIda(CciTradeParty.PartyType.party, i);
                if (ida > 0)
                    al.Add(ida);
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));
            //
            return ret;

        }

        /// <summary>
        ///  recherche du nème nème Boker DataDocument.party 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pOccurs"></param>
        /// <returns></returns>
        public string GetBrokerReferencehRef(string pClientId, int pOccurs)
        {

            string ret = string.Empty;
            if (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pClientId)
            {
                if (ArrFunc.IsFilled(DataDocument.Party))
                {
                    //20080723 PL Refactoring suite à bug sur GetPartyCount()
                    //int itemPos = GetPartyCount() + pOccurs - 1;
                    //if (itemPos < DataDocument.party.Length)
                    //    ret = DataDocument.party[itemPos].id;
                    int occurs = 0;
                    for (int i = 0; i < DataDocument.Party.Length; i++)
                    {
                        if (DataDocument.IsPartyBroker(DataDocument.Party[i]))
                        {
                            occurs++;
                            if (occurs == pOccurs)
                            {
                                ret = DataDocument.Party[i].Id;
                                break;
                            }
                        }
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phRef"></param>
        /// <returns></returns>
        public CustomCaptureInfo GetCciCounterparty(string phRef)
        {
            CustomCaptureInfo ret = null;
            for (int i = 0; i < PartyLength; i++)
            {
                if (cciParty[i].GetPartyId() == phRef)
                {
                    ret = cciParty[i].Cci(CciTradeParty.CciEnum.actor);
                    break;
                }
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phRef"></param>
        /// <returns></returns>
        public CustomCaptureInfo GetCciCounterpartyVs(string phRef)
        {

            CustomCaptureInfo ret = null;
            for (int i = 0; i < PartyLength; i++)
            {
                if (cciParty[i].GetPartyId() != phRef)
                {
                    ret = cciParty[i].Cci(CciTradeParty.CciEnum.actor);
                    break;
                }
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        // EG 20171025 [23509] Upd tradeDate
        public virtual string GetData(string pKey, CustomCaptureInfo pCci)
        {

            string ret = string.Empty;

            if (null != cciProduct)
                ret = cciProduct.GetData(pKey, pCci);

            if (StrFunc.IsEmpty(ret))
            {
                if (StrFunc.IsEmpty(pKey))
                {
                    if (cciTradeHeader.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string cliendId_Key = cciTradeHeader.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        if (System.Enum.IsDefined(typeof(CciTradeHeader.CciEnum), cliendId_Key))
                        {
                            CciTradeHeader.CciEnum cciEnum = (CciTradeHeader.CciEnum)System.Enum.Parse(typeof(CciTradeHeader.CciEnum), cliendId_Key);
                            switch (cciEnum)
                            {
                                case CciTradeHeader.CciEnum.tradeDate:
                                    pKey = DtFunc.TODAY;
                                    break;
                            }
                        }
                    }
                }

                if (StrFunc.IsEmpty(pKey))
                    pKey = "T";

                switch (pKey.ToUpper())
                {
                    case "T":
                        ret = Tz.Tools.DateToStringISO(cciTradeHeader.Cci(CciTradeHeader.CciEnum.tradeDate).NewValue);
                        break;
                    case DtFunc.TODAY:
                        ret = pKey;
                        break;
                }
            }

            return ret;

        }

        /// <summary>
        /// Retourne true si le cci représente l'émetteur
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public virtual bool IsCci_Issuer(CustomCaptureInfo pCci)
        {

            bool ret = false;
            if (null != cciProduct)
                ret = cciProduct.IsCci_Issuer(pCci);
            return ret;

        }

        /// <summary>
        /// Retourne true si le cci est un  élément d'un extend
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public virtual bool IsCci_Extends(CustomCaptureInfo pCci)
        {

            return false;

        }

        /// <summary>
        /// Fonction de replace des constantes 'spéciales' dynamiques du trade
        /// </summary>
        /// <param name="pInitialString">chaine dans laquelle remplacer les valeurs const</param>
        /// <returns>string: chaine mise à jour</returns>
        // EG 20171113 [23509] Upd Add TRD_FACILITY
        public virtual string ReplaceTradeDynamicConstantsWithValues(CustomCaptureInfo pCci, string pInitialString)
        {

            string ret = pInitialString;

            if (null != cciProduct)
                ret = cciProduct.ReplaceTradeDynamicConstantsWithValues(pCci, ret);

            // TRD_FACILITY
            CciMarketParty cciMarketParty = CciFacilityParty;
            if ((null != cciMarketParty) && ret.Contains(Cst.TRD_FACILITY))
            {
                // FI 20200116 [25141] lecture de FIXML_SecurityExchange car le cci.NewValue ne contient pas nécessairement l'information FIXML_SecurityExchange
                string facilityId = string.Empty;
                if (null != (cciMarketParty.Cci(CciMarketParty.CciEnum.identifier).Sql_Table))
                    facilityId = ((SQL_Market)cciMarketParty.Cci(CciMarketParty.CciEnum.identifier).Sql_Table).FIXML_SecurityExchange;

                ret = ret.Replace(Cst.TRD_FACILITY, facilityId);
            }

            // TRD_IDA_PARTY1
            if (ret.Contains(Cst.TRD_IDA_PARTY1))
            {
                if (ArrFunc.Count(cciParty) > 0)
                    ret = ret.Replace(Cst.TRD_IDA_PARTY1, cciParty[0].GetActorIda().ToString());
            }

            // TRD_IDA_PARTY2
            if (ret.Contains(Cst.TRD_IDA_PARTY2))
            {

                if (ArrFunc.Count(cciParty) > 0)
                    ret = ret.Replace(Cst.TRD_IDA_PARTY2, cciParty[1].GetActorIda().ToString());
            }
            //
            return ret;

        }

        /// <summary>
        /// Retourne Iparty.Id de la 1er party sous DataDocument.party qui a le rôle contrepartry
        /// </summary>
        /// <returns></returns>
        public string GetIdFirstPartyCounterparty()
        {

            string ret = string.Empty;
            if (ArrFunc.IsFilled(DataDocument.Party))
            {
                for (int i = 0; i < DataDocument.Party.Length; i++)
                {
                    if (ActorTools.IsActorWithRole(CSTools.SetCacheOn(CS), DataDocument.Party[i].OTCmlId, Actor.RoleActor.COUNTERPARTY))
                    {
                        ret = DataDocument.Party[i].Id;
                        break;
                    }
                }
            }
            return ret;

        }


        /// <summary>
        /// Retourne l'élément de l'array cciParty qui contient le cci
        /// <para>Retourne -1 si le cci n'appartient pas à l'array cciParty</para>
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public int GetIndexParty(string pClientId_WithoutPrefix)
        {
            int ret = -1;
            for (int i = 0; i < PartyLength; i++)
            {
                if (cciParty[i].IsCciOfContainer(pClientId_WithoutPrefix))
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }



        /// <summary>
        ///  Recherche de la nème Contreparty
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="pOccurs"></param>
        /// <returns></returns>
        /// 20081110 FI Cette fonction Remplace GetpartyReferencehRef afin de palier des pb d'affichage de trader lorsque l'array datadocument.Party étant dans l'ordre suivant
        ///BROKER (index=0), puis PARTYA (index=1) puis PARTYB (index=2) 
        /// avec GetpartyReferencehRef On s'attendait à avoir systématiquement PARTYA (index=0) puis PARTYB (index=1) puis BROKER (index=2) 
        public string GetCounterpartyReferencehRef(string pClientId, int pOccurs)
        {
            string ret = string.Empty;
            //
            if (TradeCommonCustomCaptureInfos.CCst.Prefix_party == pClientId)
            {
                int occurs = 0;
                if ((ArrFunc.IsFilled(DataDocument.Party)) && (pOccurs <= DataDocument.Party.Length))
                {
                    ActorRoleCollection roleActorCol = DataDocument.GetActorRole(CSTools.SetCacheOn(CS));
                    for (int i = 0; i < ArrFunc.Count(DataDocument.Party); i++)
                    {
                        if (roleActorCol.IsActorRole(DataDocument.Party[i].OTCmlId, RoleActor.COUNTERPARTY))
                        {
                            occurs++;
                            if (occurs == pOccurs)
                            {
                                ret = DataDocument.Party[i].Id;
                                break;
                            }
                        }
                    }
                }
            }
            //
            return ret;

        }

        /// <summary>
        /// Initialize cciBroker
        /// </summary>
        public void InitializeBroker_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool SaveSpecified = CurrentTrade.BrokerPartyReferenceSpecified;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciTradeParty broker = new CciTradeParty(this, index + 1, CciTradeParty.PartyType.broker, "tradeHeader" + CustomObject.KEY_SEPARATOR);
                isOk = (Ccis.Contains(broker.CciClientId(CciTradeParty.CciEnum.actor)));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(CurrentTrade.BrokerPartyReference) || (index == CurrentTrade.BrokerPartyReference.Length))
                        ReflectionTools.AddItemInArray(CurrentTrade, "brokerPartyReference", index);
                    lst.Add(broker);
                }
            }
            //
            cciBroker = null;
            cciBroker = (CciTradeParty[])lst.ToArray(typeof(CciTradeParty));
            for (int i = 0; i < this.BrokerLength; i++)
                cciBroker[i].Initialize_FromCci();
            //
            CurrentTrade.BrokerPartyReferenceSpecified = SaveSpecified;


        }

        /// <summary>
        /// Initialize cciParty de type Party
        /// </summary>
        /// FI 20161214 [21916] Modify 
        /// FI 20170116 [21916] Modify
        public void InitializeParties_FromCci()
        {

            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciTradeParty party = new CciTradeParty(this, index + 1, CciTradeParty.PartyType.party, "tradeHeader" + CustomObject.KEY_SEPARATOR);

                // FI 20170116 [21916] add condition party.isInitFromClearingTemplate
                // En mode importation le cci actor pourrait être absent (Alimentation auto via CLEARINGTEMPLATE)
                isOk = (Ccis.Contains(party.CciClientId(CciTradeParty.CciEnum.actor)) || party.IsInitFromClearingTemplate);
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(DataDocument.DataDocument.Party) || (index == DataDocument.DataDocument.Party.Length))
                        ReflectionTools.AddItemInArray(DataDocument.DataDocument, "party", index);

                    if (party.IsAllocationAndClearerParty)
                    {

                        if ((this.DataDocument.CurrentProduct.IsExchangeTradedDerivative) || (this.DataDocument.CurrentProduct.IsCommoditySpot))
                            party.AdditionnalRole = new RoleActor[] { RoleActor.CSS, RoleActor.CLEARER };
                        else
                            party.AdditionnalRole = new RoleActor[] { RoleActor.CUSTODIAN };
                    }

                    if (this.DataDocument.CurrentProduct.IsMarginRequirement)
                    {
                        // FI 20200512 [XXXXX] Role ENTITY ou MARGINREQOFFICE
                        party.AdditionnalRole = new RoleActor[] { RoleActor.ENTITY, RoleActor.MARGINREQOFFICE };
                    }

                    lst.Add(party);
                }
            }

            cciParty = null;
            cciParty = (CciTradeParty[])lst.ToArray(typeof(CciTradeParty));
            for (int i = 0; i < this.PartyLength; i++)
                cciParty[i].Initialize_FromCci();
        }






        /// <summary>
        /// Aliment les Cci side de chaque partie
        /// </summary>
        public void InitializePartySide()
        {
            for (int i = 0; i < PartyLength; i++)
                cciParty[i].InitializePartySide();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void Initialize(string pPrefix)
        {
            cciTradeHeader = new CciTradeHeader(this, CurrentTrade.TradeHeader, pPrefix);
            cciParty = new CciTradeParty[2];
            for (int i = 0; i < 2; i++)
                cciParty[i] = new CciTradeParty(this, i + 1, CciTradeParty.PartyType.party, pPrefix + CustomObject.KEY_SEPARATOR);
        }



        /// <summary>
        /// Retourne true si le Cci est l'élément pElt de l'un des item de l'array cciBroker 
        /// </summary>
        /// <param name="pElt"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCci_Broker(CciTradeParty.CciEnum pElt, CustomCaptureInfo pCci)
        {
            bool isOk = false;
            for (int i = 0; i < BrokerLength; i++)
            {
                isOk = cciBroker[i].IsCci(pElt, pCci);
                if (isOk)
                    break;
            }
            return isOk;

        }

        /// <summary>
        /// Retourne true si le Cci est l'élément pElt de l'un des item de l'array cciParty
        /// </summary>
        public bool IsCci_Party(CciTradeParty.CciEnum pElt, CustomCaptureInfo pCci)
        {
            bool isOk = cciParty[0].IsCci(pElt, pCci);
            if ((false == isOk) && (PartyLength > 1))
                isOk = cciParty[1].IsCci(pElt, pCci);
            return isOk;

        }

        /// <summary>
        /// Retourne true si un cci est un receiver ou payer de OtherPartyPayment
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public virtual bool IsClientId_OtherPartyPaymentPayerReceiver(CustomCaptureInfo pCci)
        {

            return false;

        }

        /// <summary>
        /// Alimente les ccis qui représentent des receivers de OtherPartyPayment avec une valeur par défaut
        /// </summary>
        public virtual void SetClientIdDefaultReceiverOtherPartyPayment()
        {
        }

        /// <summary>
        /// Modifie l'ordre des parties présentes dans le dataDocument
        /// <para>Dans l'ordre, cciTradeparty[0] puis cciTradeparty[1] puis les brokers puis les autres</para>
        /// </summary>
        /// FI 20170928 [23452] Modify
        /// FI 20170928 [23452] Modify (Virtual method)
        public virtual void SetPartyInOrder()
        {

            Hashtable ht = new Hashtable();
            ArrayList al = new ArrayList();


            bool isStTemplateOrMissing = TradeCommonInput.TradeStatus.IsStEnvironment_Template
                        || TradeCommonInput.TradeStatus.IsStActivation_Missing;
            //
            for (int i = 0; i < PartyLength; i++)
            {
                bool addParty = cciParty[i].IsSpecified;
                if (false == addParty)
                {
                    // 20090921 RD / Pour ne pas vérifier la validité de l'acteur pour les templates
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Ajouter la Party si le Trade est un Template ou bien en statut Missing
                    addParty = isStTemplateOrMissing;
                }
                //
                if (addParty)
                {
                    string id = cciParty[i].GetPartyId(isStTemplateOrMissing);
                    if (false == ht.ContainsKey(id) && (null != DataDocument.GetParty(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }
            //
            for (int i = 0; i < BrokerLength; i++)
            {
                bool addParty = cciBroker[i].IsSpecified;
                if (false == addParty)
                {
                    // 20090921 FI / Pour ne pas vérifier la validité de l'acteur pour les templates
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Ajouter la Party si le Trade est un Template ou bien en statut Missing
                    addParty = isStTemplateOrMissing;
                }
                //
                if (addParty)
                {
                    string id = cciBroker[i].GetPartyId(isStTemplateOrMissing);
                    if (false == ht.ContainsKey(id) && (null != DataDocument.GetParty(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }

            //Ajout des parties présente dans le doc (partie et broker)
            if (ArrFunc.IsFilled(DataDocument.Party))
            {
                for (int i = 0; i < DataDocument.Party.Length; i++)
                {
                    string id = DataDocument.Party[i].Id;
                    //20090414 PL Gestion du cas où id == null
                    if ((id != null) && (!ht.ContainsKey(id)))
                    {
                        ht.Add(id, id);
                        al.Add(DataDocument.GetParty(id));
                    }
                }
            }
            //
            DataDocument.Party = (IParty[])al.ToArray((DataDocument.DataDocument.GetTypeParty()));

        }

        /// <summary>
        /// Echange (Swap) tous les ccis déclarés payer et receiver
        /// </summary>
        public void SynchronizePayerReceiverFromSide()
        {

            SQL_Actor sql_Actor;
            string value1 = Ccis[cciParty[0].CciClientId(CciTradeParty.CciEnum.actor)].NewValue;
            string value2 = Ccis[cciParty[1].CciClientId(CciTradeParty.CciEnum.actor)].NewValue;
            sql_Actor = (SQL_Actor)Ccis[cciParty[0].CciClientId(CciTradeParty.CciEnum.actor)].Sql_Table;
            if (null != sql_Actor)
                value1 = sql_Actor.XmlId;
            sql_Actor = (SQL_Actor)Ccis[cciParty[1].CciClientId(CciTradeParty.CciEnum.actor)].Sql_Table;
            if (null != sql_Actor)
                value2 = sql_Actor.XmlId;

            SynchronizePayerReceiver(value1, "$*Synchronize*$");
            SynchronizePayerReceiver(value2, value1);
            SynchronizePayerReceiver("$*Synchronize*$", value2);

            InitializePartySide();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public virtual void SynchronizePayerReceiverOtherPartyPayment(string pLastValue, string pNewValue)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pOccurs"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        private bool RemoveLastItemInPartyTradeInformationElementArray(string pPrefix, int pOccurs, string pParentClientId, int pParentOccurs)
        {

            bool isOk = true;
            bool isPrefixTrader = (TradeCommonCustomCaptureInfos.CCst.Prefix_trader == pPrefix);
            bool isPrefixSales = (TradeCommonCustomCaptureInfos.CCst.Prefix_sales == pPrefix);
            bool isPrefixBroker = (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pPrefix);
            //
            if ((isPrefixTrader || isPrefixSales || isPrefixBroker) && (0 < pParentOccurs))
            {
                if (pOccurs == 1)
                    return false;
                string parentReferencehRef = string.Empty;
                if (ArrFunc.IsFilled(CurrentTrade.TradeHeader.PartyTradeInformation))
                {
                    if (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pParentClientId)
                        parentReferencehRef = GetBrokerReferencehRef(pParentClientId, pParentOccurs);
                    else if (TradeCommonCustomCaptureInfos.CCst.Prefix_party == pParentClientId)
                        parentReferencehRef = GetCounterpartyReferencehRef(pParentClientId, pParentOccurs);
                    //
                    IPartyTradeInformation partyTradeInformation = DataDocument.GetPartyTradeInformation(parentReferencehRef);
                    if (null != partyTradeInformation)
                    {
                        int elementCount = 0;
                        if (isPrefixTrader)
                            elementCount = ArrFunc.Count(partyTradeInformation.Trader);
                        else if (isPrefixSales)
                            elementCount = ArrFunc.Count(partyTradeInformation.Sales);
                        else if (isPrefixBroker)
                            elementCount = ArrFunc.Count(partyTradeInformation.BrokerPartyReference);
                        //
                        if ((elementCount > 1) && (elementCount == pOccurs))
                        {
                            bool isContinue;
                            if (isPrefixTrader)
                                isContinue = StrFunc.IsEmpty(partyTradeInformation.Trader[elementCount - 1].Identifier);
                            else if (isPrefixSales)
                                isContinue = StrFunc.IsEmpty(partyTradeInformation.Sales[elementCount - 1].Identifier);
                            else if (isPrefixBroker)
                                isContinue = StrFunc.IsEmpty(partyTradeInformation.BrokerPartyReference[elementCount - 1].HRef);
                            else
                                throw new NotImplementedException();
                            //
                            if (isContinue)
                            {
                                if (TradeCommonCustomCaptureInfos.CCst.Prefix_broker == pParentClientId)
                                {
                                    Ccis.RemoveCciOf(cciBroker[pParentOccurs - 1].cciTrader[elementCount - 1]);
                                    ReflectionTools.RemoveItemInArray(cciBroker[pParentOccurs - 1], "cciTrader", elementCount - 1);
                                    ReflectionTools.RemoveItemInArray(partyTradeInformation, "trader", elementCount - 1);
                                }
                                else if (TradeCommonCustomCaptureInfos.CCst.Prefix_party == pParentClientId)
                                {
                                    if (isPrefixTrader)
                                    {
                                        Ccis.RemoveCciOf(cciParty[pParentOccurs - 1].cciTrader[elementCount - 1]);
                                        ReflectionTools.RemoveItemInArray(cciParty[pParentOccurs - 1], "cciTrader", elementCount - 1);
                                        ReflectionTools.RemoveItemInArray(partyTradeInformation, "trader", elementCount - 1);
                                    }
                                    if (isPrefixSales)
                                    {
                                        Ccis.RemoveCciOf(cciParty[pParentOccurs - 1].cciSales[elementCount - 1]);
                                        ReflectionTools.RemoveItemInArray(cciParty[pParentOccurs - 1], "cciSales", elementCount - 1);
                                        ReflectionTools.RemoveItemInArray(partyTradeInformation, "sales", elementCount - 1);
                                    }
                                    if (isPrefixBroker)
                                    {
                                        Ccis.RemoveCciOf(cciParty[pParentOccurs - 1].cciBroker[elementCount - 1]);
                                        ReflectionTools.RemoveItemInArray(cciParty[pParentOccurs - 1], "cciBroker", elementCount - 1);
                                        ReflectionTools.RemoveItemInArray(partyTradeInformation, "brokerPartyReference", elementCount - 1);
                                    }
                                }
                                //

                            }
                            else
                                isOk = false;
                        }
                    }
                }
            }
            //
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <returns></returns>
        protected bool RemoveLastItemInBrokerPartyReferenceArray(string pPrefix)
        {
            return RemoveLastItemInBrokerPartyReferenceArray(pPrefix, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pIsEmpty"></param>
        /// <returns></returns>
        /// FI 20170928 [23452] Modify
        protected bool RemoveLastItemInBrokerPartyReferenceArray(string pPrefix, bool pIsEmpty)
        {
            bool isToRemove = true;
            if (pPrefix == PrefixHeader + CustomObject.KEY_SEPARATOR + CciTradeParty.PartyType.broker)
            {
                int posArray = this.BrokerLength - 1;

                if (pIsEmpty)
                    isToRemove = StrFunc.IsEmpty(CurrentTrade.BrokerPartyReference[posArray].HRef);
                if (isToRemove)
                {
                    //Mise en commentaire pour simplifier CleanUp
                    //en théorie Cet appel n'existe plus depuis l'appel à InitializeNodeObjectArray2
                    //cciBroker[posArray].CleanUp(true);
                    Ccis.RemoveCciOf(cciBroker[posArray]);
                    ReflectionTools.RemoveItemInArray(this, "cciBroker", posArray);
                    ReflectionTools.RemoveItemInArray(CurrentTrade, "brokerPartyReference", posArray);
                }
            }
            return isToRemove;

        }

        /// <summary>
        /// Lecture du fichier .Config => clef Spheres_ReferentialDefault_currency
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCurrency()
        {
            return ConfigurationManager.AppSettings["Spheres_ReferentialDefault_currency"];
        }

        /// <summary>
        /// Lecture du fichier .Config => clef Spheres_TradeInputDefault_ISINVOICING
        /// </summary>
        public static string GetDefaultISINVOICING()
        {
            return ConfigurationManager.AppSettings["Spheres_TradeInputDefault_ISINVOICING"];
        }

        /// <summary>
        ///  Alimente l'élément TradeSide du datadocument à partir de cciParty
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction
        public void DumpTradeSide_ToDocument()
        {
            for (int i = 0; i < PartyLength; i++)
            {
                if (cciParty[i].IsInitTradeSide)
                {
                    IParty party = DataDocument.GetParty(cciParty[i].GetPartyId());
                    if (null != party)
                        DataDocument.SetTradeSide(CSCacheOn, null, party);
                }
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class CciProductBase : ContainerCciBase, IContainerCciFactory, ITradeCci, IContainerCciPayerReceiver, IContainerCciGetInfoButton, IContainerCciQuoteBasis
    {

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public ProductContainer Product
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public CciTradeCommonBase CciTradeCommon
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public CciProductBase(CciTradeCommonBase pCciTradeCommon, IProduct pProduct, string pPrefix)
            : this(pCciTradeCommon, pProduct, pPrefix, -1) { }
        public CciProductBase(CciTradeCommonBase pCciTradeCommon, IProduct pProduct, string pPrefix, int pNumber) :
            base(pPrefix, pNumber, pCciTradeCommon.Ccis)
        {
            CciTradeCommon = pCciTradeCommon;

            // FI 20200124 [XXXXX] Call InitProduct
            InitProduct(pProduct);

            // FI 20200428 [XXXXX] InitDefaultFromSQLCookie uniquement si IsGetDefaultOnInitializeCci
            // Pour effectuer les lecture SQL de la table COOKIE uniquement lorsqu'il en est réellement nécessaire
            if (CciTradeCommon.Ccis.IsGetDefaultOnInitializeCci && (false == CciTradeCommon.Ccis.IsModeIO))
                InitDefaultFromSQLCookie();
        }
        #endregion

        #region IContainerCciFactory Members
        #region AddCciSystem
        public virtual void AddCciSystem()
        {

        }
        #endregion AddCciSystem
        #region CleanUp
        public virtual void CleanUp()
        {

        }
        #endregion CleanUp
        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        public virtual void Dump_ToDocument()
        {

        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize_Document()
        {


        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        virtual public void Initialize_FromCci()
        {

        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public virtual void Initialize_FromDocument()
        {

        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {

        }
        #endregion
        #region ProcessExecute
        public virtual void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        public virtual void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region RefreshCciEnabled
        public virtual void RefreshCciEnabled()
        {

        }
        #endregion RefreshCciEnabled
        #region SetDisplay
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {

        }
        #endregion
        #endregion IContainerCciFactory Members

        #region ITradeCci Members
        #region CciClientIdMainCurrency
        public virtual string CciClientIdMainCurrency
        {
            get { return string.Empty; }
        }
        #endregion CciClientIdMainCurrency
        #region GetMainCurrency
        public virtual string GetMainCurrency
        {
            get { return string.Empty; }
        }
        #endregion GetMainCurrency
        #region RetSidePayer
        public virtual string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region RetSideReceiver
        public virtual string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #endregion ITradeCci Members

        #region IContainerCciPayerReceiver Members
        /// <summary>
        /// Représente le payer du produit
        /// </summary>
        public virtual string CciClientIdPayer
        {
            get
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Représente le receiver du produit
        /// </summary>
        public virtual string CciClientIdReceiver
        {
            get
            {
                return string.Empty;
            }
        }
        #region SynchronizePayerReceiver
        public virtual void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
        }
        #endregion SynchronizePayerReceiver
        #region SynchronizeParty
        public virtual void SynchronizeParty(CustomCaptureInfo pCci)
        {
        }
        #endregion SynchronizePayerReceiver
        #endregion IContainerCciPayerReceiver Members

        #region IContainerCciGetInfoButton Members
        #region IsButtonMenu
        public virtual bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            return false;
        }
        #endregion IsButtonMenu
        #region SetButtonScreenBox
        public virtual bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public virtual bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #region SetButtonReferential
        public virtual void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

        }
        #endregion SetButtonReferential

        #endregion ITradeGetInfoButton Members

        #region IContainerCciQuoteBasis Members
        #region GetBaseCurrency
        public virtual string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return string.Empty;
        }
        #endregion GetBaseCurrency
        #region GetCurrency1
        public virtual string GetCurrency1(CustomCaptureInfo pCci)
        {
            return string.Empty;
        }
        #endregion GetCurrency1
        #region GetCurrency2
        public virtual string GetCurrency2(CustomCaptureInfo pCci)
        {
            return string.Empty;
        }
        #endregion GetCurrency2
        #region IsClientId_QuoteBasis
        public virtual bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_QuoteBasis
        #endregion IContainerCciQuoteBasis Members

        #region IsCci_Issuer
        public virtual bool IsCci_Issuer(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion

        #region GetData
        public virtual string GetData(string pKey, CustomCaptureInfo pCci)
        {
            return string.Empty;

        }
        #endregion GetData

        #region DumpSpecific_ToGUI
        public virtual void DumpSpecific_ToGUI(CciPageBase pPage)
        {
        }
        #endregion
        #region DumpInvoicingScope_ToDocument
        public virtual void DumpInvoicingScope_ToDocument(CustomCaptureInfo pCci, string pData)
        {
        }
        #endregion
        #region ReplaceTradeDynamicConstantsWithValues
        public virtual string ReplaceTradeDynamicConstantsWithValues(CustomCaptureInfo pCci, string pInitialString)
        {

            string ret = pInitialString;
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                // TRD_IDI_INSTRUMENTMASTER 
                if (true == ret.Contains(Cst.TRD_IDI_INSTRUMENTMASTER))
                {
                    Nullable<int> idI = Product.GetIdI();
                    if (null == idI)
                        idI = -1;
                    //if (null != masterIdI)
                    ret = ret.Replace(Cst.TRD_IDI_INSTRUMENTMASTER, idI.ToString());
                }
            }
            return ret;

        }
        #endregion

        #region SetProduct
        public virtual void SetProduct(IProduct pProduct)
        {
            // FI 20200124 [XXXXX] Call InitProduct
            InitProduct(pProduct);
        }
        #endregion

        /// <summary>
        /// /
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// EG 20171114 [23509] Sauvegarde de la plateforme dans les cookies
        /// EG 20221201 [25639] [WI484] Add Environmental
        private void InitDefaultFromSQLCookie()
        {
            // MF 20120515 Ticket 17777 session is null during trade import by the IO service
            // MF 20120518 Ticket 17784 product can be null
            if (Product != null)
            {
                string defaultFacility = string.Empty;
                Nullable<Cst.SQLCookieElement> facilityCookieElement = null;
                Nullable<Cst.SQLCookieElement> marketCookieElement = null;

                if (Product.IsExchangeTradedDerivative)
                {
                    facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility;
                    marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket;
                }
                else if (Product.IsEquitySecurityTransaction)
                {
                    facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_ESE;
                    marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_ESE;
                }
                else if (Product.IsCommoditySpot)
                {
                    ICommoditySpot _commoditySpot = (ICommoditySpot)Product.Product;
                    if (_commoditySpot.IsGas)
                    {
                        facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_COMS_gas;
                        marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_COMS_gas;
                    }
                    else if (_commoditySpot.IsEnvironmental)
                    {
                        facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_COMS_env;
                        marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_COMS_env;
                    }
                    else if (_commoditySpot.IsElectricity)
                    {
                        facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_COMS_elec;
                        marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_COMS_elec;
                    }
                }
                else if (Product.IsTradeMarket)
                {
                    facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_Other;
                }

                if (facilityCookieElement.HasValue)
                {
                    AspTools.ReadSQLCookie(facilityCookieElement.Value.ToString(), out defaultFacility);
                    if (StrFunc.IsFilled(defaultFacility))
                        CciTradeCommon.TradeCommonInput.InitDefault(CommonInput.DefaultEnum.facility, defaultFacility);
                }

                if (StrFunc.IsFilled(defaultFacility) && marketCookieElement.HasValue)
                {
                    AspTools.ReadSQLCookie(marketCookieElement.Value.ToString(), out string defaultMarket);
                    if (StrFunc.IsFilled(defaultMarket))
                        CciTradeCommon.TradeCommonInput.InitDefault(CommonInput.DefaultEnum.market, defaultMarket);
                }
            }

        }


        /// <summary>
        /// Initialisation de RptSide lorsque le produit contient un RptSide
        /// <para>Mise en place des dispositions pour un fonctionnement correct de la saisie light</para>
        /// <para>Mise en place de 2 TrdCapRptSideGrp si Execution ou Intermediation</para>
        /// </summary>
        /// FI 20161214 [21916] Add
        /// FI 20170116 [21916] Modify
        /// EG 20171114 [23509] Upd
        protected void InitializeRptSideElement()
        {

            RptSideProductContainer rptSideContainer = this.Product.RptSide();

            if (null == rptSideContainer)
                throw new NullReferenceException("rptSideContainer is null");

            /*  FI 20170116 [21916] l'élément se nomme "RptSide"   
            string rptSideElementName = "rptSide";
            if (this.Product.productBase.IsExchangeTradedDerivative)
                rptSideElementName = "RptSide";
             */

            IFixTrdCapRptSideGrp[] trdCapRptSideGrp = rptSideContainer.RptSide;

            if (CciTradeCommon.TradeCommonInput.IsAllocation)
            {
                //Si Allocation, il faut au minimum 1 TrdCapRptSideGrp
                bool isOk = (ArrFunc.Count(trdCapRptSideGrp) >= 1);
                if (false == isOk)
                {
                    int newPos = ArrFunc.Count(trdCapRptSideGrp);
                    ReflectionTools.AddItemInArray(rptSideContainer.Parent, "RptSide", newPos);
                    // EG 20171113 
                    rptSideContainer.RptSide[newPos].SideSpecified = true;
                }

                trdCapRptSideGrp = rptSideContainer.RptSide;

                //Spheres conserve le 1er TrdCapRptSideGrp => Tous les autres sont supprimés
                if (ArrFunc.Count(trdCapRptSideGrp) > 1)
                {
                    for (int i = ArrFunc.Count(trdCapRptSideGrp) - 1; i > 0; i--)
                    {
                        if (ArrFunc.Count(trdCapRptSideGrp[i].Parties) > 0)
                        {
                            for (int j = 0; j < ArrFunc.Count(trdCapRptSideGrp[i].Parties); j++)
                            {
                                IFixParty fixParty = trdCapRptSideGrp[i].Parties[j];
                                if (fixParty.PartyIdSpecified)
                                    CciTradeCommon.TradeCommonInput.DataDocument.RemoveParty(fixParty.PartyId.href);
                            }
                        }
                        ReflectionTools.RemoveItemInArray(rptSideContainer.Parent, "RptSide", i);
                    }
                }
            }
            else if (CciTradeCommon.TradeCommonInput.IsDeal)
            {
                //Si NON Allocation, il faut au minimum 2 TrdCapRptSideGrp (1 de sens Achat, 1 de sens Vente) 
                bool isOk = (ArrFunc.Count(trdCapRptSideGrp) >= 2);
                while (false == isOk)
                {
                    int newPos = ArrFunc.Count(rptSideContainer.RptSide);
                    ReflectionTools.AddItemInArray(rptSideContainer.Parent, "RptSide", newPos);
                    isOk = (ArrFunc.Count(rptSideContainer.RptSide) >= 2);
                }
                IFixTrdCapRptSideGrp trdCapRptSide = rptSideContainer.GetTrdCapRptSideGrp(SideEnum.Buy);
                if (null == trdCapRptSide)
                {
                    trdCapRptSide = rptSideContainer.GetTrdCapRptSideGrp(null);
                    trdCapRptSide.Side = SideEnum.Buy;
                    trdCapRptSide.SideSpecified = true;
                }
                //
                trdCapRptSide = rptSideContainer.GetTrdCapRptSideGrp(SideEnum.Sell);
                if (null == trdCapRptSide)
                {
                    trdCapRptSide = rptSideContainer.GetTrdCapRptSideGrp(null);
                    trdCapRptSide.Side = SideEnum.Sell;
                    trdCapRptSide.SideSpecified = true;
                }

                for (int k = 0; k < 2; k++)
                {
                    if (0 == k)
                        trdCapRptSide = rptSideContainer.GetTrdCapRptSideGrp(SideEnum.Buy);
                    else
                        trdCapRptSide = rptSideContainer.GetTrdCapRptSideGrp(SideEnum.Sell);

                    if (ArrFunc.IsFilled(trdCapRptSide.Parties))
                    {
                        for (int j = ArrFunc.Count(trdCapRptSide.Parties) - 1; j > 0; j--)
                        {
                            if (trdCapRptSide.Parties[j].PartyIdSpecified && trdCapRptSide.Parties[j].PartyRoleSpecified)
                            {
                                if (trdCapRptSide.Parties[j].PartyRole != PartyRoleEnum.BuyerSellerReceiverDeliverer)
                                {
                                    CciTradeCommon.TradeCommonInput.DataDocument.RemoveParty(trdCapRptSide.Parties[j].PartyId.href);
                                    ReflectionTools.RemoveItemInArray(trdCapRptSide, "Pty", j);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Alimente RptSide côté Acheteur (avec le contenu du CciClientIdPayer) ou côté vendeur (avec le contenu du CciClientIdReceiver)
        ///  <para>Définit l'acheteur/vendeur et son rôle</para>
        /// </summary>
        /// FI 20161214 [21916] Add
        /// FI 20170222 [XXXXX] Modify 
        protected void RptSideSetBuyerSeller(BuyerSellerEnum pBuyerSeller)
        {
            RptSideProductContainer rptSideContainer = this.Product.RptSide();
            if (null == rptSideContainer)
                throw new NullReferenceException("rptSideContainer is null");

            switch (pBuyerSeller)
            {
                case BuyerSellerEnum.BUYER:
                    CustomCaptureInfo cciPayer = CciTradeCommon.Ccis[CciClientIdPayer]; // on considère que le payer du produit est l'acheteur ds RptSide 
                    if (null != cciPayer)
                    {
                        // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                        string partyId = XMLTools.GetXmlId(cciPayer.NewValue);
                        IParty party = CciTradeCommon.DataDocument.GetParty(partyId, PartyInfoEnum.id);
                        if (null != party) // party peut être null si saisi de string.empty sur le cciPayer
                            partyId = party.Id;

                        if (rptSideContainer.IsOneSide)
                        {
                            if (CciTradeCommon.cciParty[0].GetPartyId(true) == partyId)
                            {
                                rptSideContainer.SetBuyerSeller(partyId, SideEnum.Buy, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                            }
                            else if (CciTradeCommon.cciParty[1].GetPartyId(true) == partyId)
                            {
                                PartyRoleEnum role = PartyRoleEnum.ClearingOrganization;
                                if (rptSideContainer.ProductBase.IsExchangeTradedDerivative || rptSideContainer.ProductBase.IsCommoditySpot)
                                {
                                    if (null != party)
                                    {
                                        if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, party.OTCmlId, RoleActor.CSS))
                                            role = PartyRoleEnum.ClearingOrganization;
                                        else if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, party.OTCmlId, RoleActor.CLEARER))
                                            role = PartyRoleEnum.ClearingFirm;
                                    }
                                }
                                else
                                {
                                    role = PartyRoleEnum.Custodian;
                                }
                                rptSideContainer.SetBuyerSeller(partyId, SideEnum.Buy, role);
                            }
                        }
                        else if (rptSideContainer.IsTwoSide)
                        {
                            rptSideContainer.SetBuyerSeller(partyId, SideEnum.Buy, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                        }
                    }

                    break;

                case BuyerSellerEnum.SELLER:
                    CustomCaptureInfo cciReceiver = CciTradeCommon.Ccis[CciClientIdReceiver]; // on considère que le receiver du produit est le vendeur ds RptSide 
                    if (null != cciReceiver)
                    {
                        // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                        string partyId = XMLTools.GetXmlId(cciReceiver.NewValue);
                        IParty party = CciTradeCommon.DataDocument.GetParty(partyId, PartyInfoEnum.id);                        
                        if (null != party) // party peut être null si saisi de string.empty sur le cciReceiver
                            partyId = party.Id;

                        if (rptSideContainer.IsOneSide)
                        {
                            if (CciTradeCommon.cciParty[0].GetPartyId(true) == partyId)
                            {
                                rptSideContainer.SetBuyerSeller(partyId, SideEnum.Sell, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                            }
                            else if (CciTradeCommon.cciParty[1].GetPartyId(true) == partyId)
                            {
                                PartyRoleEnum role = PartyRoleEnum.ClearingOrganization;
                                if (rptSideContainer.ProductBase.IsExchangeTradedDerivative || rptSideContainer.ProductBase.IsCommoditySpot)
                                {
                                    if (null != party)
                                    {
                                        if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, party.OTCmlId, RoleActor.CSS))
                                            role = PartyRoleEnum.ClearingOrganization;
                                        else if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, party.OTCmlId, RoleActor.CLEARER))
                                            role = PartyRoleEnum.ClearingFirm;
                                    }
                                }
                                else
                                {
                                    role = PartyRoleEnum.Custodian;
                                }
                                rptSideContainer.SetBuyerSeller(partyId, SideEnum.Sell, role);
                            }
                        }
                        else if (rptSideContainer.IsTwoSide)
                        {
                            rptSideContainer.SetBuyerSeller(partyId, SideEnum.Sell, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        ///  Affecte l'acteur clearing avec la chambre de compensation du marché si l'entité du dealer est membre de la chambre
        ///  <para></para>
        /// </summary>
        /// FI 20161214 [21916] Add
        protected void SetCssByEntityClearingMember()
        {
            string cs = CciTradeCommon.CSCacheOn;
            CustomCaptureInfosBase ccis = CciTradeCommon.Ccis;

            int idAEntity = BookTools.GetEntityBook(cs, CciTradeCommon.cciParty[0].GetBookIdB());

            CciTradeCommon.DataDocument.CurrentProduct.GetMarket(cs, null, out SQL_Market sqlMarket);

            if (((null != sqlMarket) && (sqlMarket.IdA > 0)) && (idAEntity > 0))
            {
                bool isEntityClearingMember = CaptureTools.IsActorClearingMember(cs, idAEntity, sqlMarket.Id, true);
                if (isEntityClearingMember)
                {
                    SQL_Actor sqlActor = new SQL_Actor(cs, sqlMarket.IdA, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, ccis.User, ccis.SessionId);
                    if (sqlActor.LoadTable(new string[] { "IDENTIFIER" }))
                    {
                        string clientId = CciTradeCommon.cciParty[1].CciClientId(CciTradeParty.CciEnum.actor);
                        CciTradeCommon.Ccis.SetNewValue(clientId, sqlActor.Identifier, false);
                    }
                }
            }
        }


        #region IsCciExchange
        // EG 20171031 [23509] New
        public virtual bool IsCciExchange(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsCciExchange
        #region IsCciExecutionDateTime
        // EG 20171031 [23509] New
        public virtual bool IsCciExecutionDateTime(CustomCaptureInfo pCci)
        {
            CciMarketParty cci = CciTradeCommon.CciFacilityParty;
            bool ret = null != cci && cci.IsCci(CciMarketParty.CciEnum.executionDateTime, pCci);
            return ret;
        }
        #endregion IsCciExecutionDateTime
        #region IsCciMarketFacility
        // EG 20171031 [23509] New
        public virtual bool IsCciMarketFacility(CustomCaptureInfo pCci)
        {
            CciMarketParty cci = CciTradeCommon.CciFacilityParty;
            bool ret = null != cci && cci.IsCci(CciMarketParty.CciEnum.identifier, pCci);
            return ret;
        }
        #endregion IsCciMarketFacility
        #region IsCciOrderEntered
        // EG 20171031 [23509] New
        public virtual bool IsCciOrderEntered(CustomCaptureInfo pCci)
        {
            CciMarketParty cci = CciTradeCommon.CciFacilityParty;
            bool ret = null != cci && cci.IsCci(CciMarketParty.CciEnum.orderEntered, pCci);
            return ret;
        }
        #endregion IsCciOrderEntered
        #region CciClearedDate
        // EG 20171031 [23509] New
        public virtual CustomCaptureInfo CciClearedDate
        {
            get
            {
                CustomCaptureInfo cci = null;
                CciMarketParty cciMarketParty = CciTradeCommon.CciFacilityParty;
                if (null != cciMarketParty)
                    cci = cciMarketParty.Cci(CciMarketParty.CciEnum.clearedDate);
                return cci;
            }
        }
        #endregion Cci_ClearedDate
        #region CciExchange
        /// <summary>
        /// Obtient le cci marché
        /// </summary>
        /// EG 20171113 [23509] New
        public virtual CustomCaptureInfo CciExchange
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// Obtient la colonne SQL qui alimente de cci marché
        /// </summary>
        /// FI 20200116 [25141] Add
        public virtual string CciExchangeColumn
        {
            get
            {
                return null;
            }
        }
        #endregion CciExchange

        #region CciExecutionDateTime
        // EG 20171031 [23509] New
        public virtual CustomCaptureInfo CciExecutionDateTime
        {
            get
            {
                CustomCaptureInfo cci = null;
                CciMarketParty cciMarketParty = CciTradeCommon.CciFacilityParty;
                if (null != cciMarketParty)
                    cci = cciMarketParty.Cci(CciMarketParty.CciEnum.executionDateTime);
                return cci;
            }
        }
        #endregion CciExecutionDateTime
        #region CciMarketFacility
        // EG 20171031 [23509] New
        public virtual CustomCaptureInfo CciMarketFacility
        {
            get
            {
                CustomCaptureInfo cci = null;
                CciMarketParty cciMarketParty = CciTradeCommon.CciFacilityParty;
                if (null != cciMarketParty)
                    cci = cciMarketParty.Cci(CciMarketParty.CciEnum.identifier);
                return cci;
            }
        }
        #endregion CciMarketFacility

        #region CciOrderEntered
        // EG 20171031 [23509] New
        public virtual CustomCaptureInfo CciOrderEntered
        {
            get
            {
                CustomCaptureInfo cci = null;
                CciMarketParty cciMarketParty = CciTradeCommon.CciFacilityParty;
                if (null != cciMarketParty)
                    cci = cciMarketParty.Cci(CciMarketParty.CciEnum.orderEntered);
                return cci;
            }
        }
        #endregion CciOrderEntered

        #region GetSQLMarket
        /// <summary>
        /// Retourne SQLMarket en cours
        /// </summary>
        /// <returns></returns>
        // RD 20201008 [25225] New
        private SQL_Market GetSQLMarket()
        {
            CustomCaptureInfo cciMarketFacility = CciMarketFacility;
            SQL_Market sqlMarket;
            if ((null != cciMarketFacility) && (null != cciMarketFacility.Sql_Table))
                sqlMarket = cciMarketFacility.Sql_Table as SQL_Market;
            else
                CciTradeCommon.Product.GetMarket(CciTradeCommon.CSCacheOn, null, out sqlMarket);

            return sqlMarket;
        }
        #endregion GetSQLMarket
        #region GetBCSMarket
        /// <summary>
        /// Retourne le BCS du Marché
        /// </summary>
        /// <returns></returns>
        // RD 20201008 [25225] New
        private EFS_BusinessCenters GetBCSMarket()
        {
            EFS_BusinessCenters efs_bc = null;
            SQL_Market sqlMarket = GetSQLMarket();

            if (null != sqlMarket)
            {
                IBusinessCenters bcs = CciTradeCommon.Product.ProductBase.LoadBusinessCenters(CciTradeCommon.CSCacheOn, null, null, null, new string[] { sqlMarket.Id.ToString() });
                efs_bc = new EFS_BusinessCenters(CciTradeCommon.CSCacheOn, null, bcs, CciTradeCommon.DataDocument);
            }

            return efs_bc;
        }
        #endregion GetBCSMarket
        #region GetCutOffMarket
        /// <summary>
        /// Recherche du CutOff sur le marché
        /// </summary>
        // EG 20171031 [23509] New
        private string GetCutOffMarket()
        {
            string cutOff = string.Empty;
            // RD 20201008 [25225] Use GetSQLMarket();
            //SQL_Market sqlMarket = null;
            //CustomCaptureInfo cciMarketFacility = CciMarketFacility;
            //if ((null != cciMarketFacility) && (null != cciMarketFacility.Sql_Table))
            //    sqlMarket = cciMarketFacility.Sql_Table as SQL_Market;
            //else
            //    CciTradeCommon.Product.GetMarket(CciTradeCommon.CSCacheOn, null, out sqlMarket);
            SQL_Market sqlMarket = GetSQLMarket();
            if (null != sqlMarket)
                cutOff = sqlMarket.CutOffTime(CciTradeCommon.TradeCommonInput.SQLProduct.Family);

            return cutOff;
        }
        #endregion GetCutOffMarket

        #region IdA_Custodian
        // EG 20171031 [23509] New
        public virtual Nullable<int> IdA_Custodian
        {
            get { return null; }
        }
        #endregion IdA_Custodian
        /* FI 20240307 [WI862] Mise en commentaire (déjà effectué via l'appel à ProductContainerBase.SynchronizeFromDataDocument)
        #region DumpBizDt_ToDocument
        /// <summary>
        /// Dump a clearedDate into DataDocument (FIXML => BizDt)
        /// </summary>
        // EG 20171031 [23509] New
        public virtual void DumpBizDt_ToDocument(string pData)
        {
        }
        #endregion DumpBizDt_ToDocument
        */

        #region InitializeDates
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// EG 20171031 [23509] New
        /// EG 20171113 [23509] Upd
        /// EG 20171115 [23509] Upd Cutoff
        /// FI 20180117 [23729] Modity
        /// FI 20180130 [23737] Modify
        /// FI 20180705 [24024] Modfy
        /// FI 20240307 [WI866] Trade Input (EXEC/INTERMEDIATION) : Pre-propostion de cleared date
        public virtual void InitializeDates(CustomCaptureInfo pCci)
        {
            // FI 20180705 [24024] InitializeDates uniquement si la donnée a changé et si le cii n'est pas en erreur 
            bool isOk = pCci.HasChanged && (false == pCci.HasError) &&
                (CciTradeCommon.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) || IsCciExchange(pCci) ||
                    IsCciExecutionDateTime(pCci) || IsCciOrderEntered(pCci) || IsCciMarketFacility(pCci));

            if (isOk)
            {
                //TimeZone
                string timezone = string.Empty;
                if (null != CciTradeCommon.CciFacilityParty)
                    timezone = CciTradeCommon.CciFacilityParty.GetTimeZoneValue();

                //dtCleared => Lecture de la date business saisie (clearedDate)
                Nullable<DateTime> dtCleared = null;
                CustomCaptureInfo cciClearedDate = CciClearedDate;
                if ((null != cciClearedDate) && StrFunc.IsFilled(cciClearedDate.NewValue))
                    dtCleared = new EFS_Date(cciClearedDate.NewValue).DateValue;

                //dtExecution => Lecture de la date d'exécution saisie (executionDateTime)
                Nullable<DateTimeOffset> dtExecution = null;
                CustomCaptureInfo cciExecutionDateTime = CciExecutionDateTime;
                if (null != cciExecutionDateTime)
                    dtExecution = Tz.Tools.ToDateTimeOffset(cciExecutionDateTime.NewValue);

                //dtOrderEntered => Lecture de la date d'entrée saisie (orderEntered)
                Nullable<DateTimeOffset> dtOrderEntered = null;
                CustomCaptureInfo cciOrderEntered = CciOrderEntered;
                if (null != cciOrderEntered)
                    dtOrderEntered = Tz.Tools.ToDateTimeOffset(cciOrderEntered.NewValue);


                bool isApplyCutOff = IsCciExecutionDateTime(pCci) && dtExecution.HasValue && dtCleared.HasValue;

                if (IsCciMarketFacility(pCci))
                {
                    #region La plateforme a changé
                    // -----------------------------------------------------------------------------------------------------------------
                    // La plateforme a été modifiée on réinitialise systématiquement : ORDERENTERED et EXECUTIONDATETIME
                    // -----------------------------------------------------------------------------------------------------------------
                    // FI 20200811 [XXXXX] usage de OTCmlHelper.GetDateBusiness puisque GetRDBMSDateSys est obsolete
                    //DateTime dtTmp;
                    //DateTime dt = OTCmlHelper.GetRDBMSDateSys(CciTradeCommon.CS, null, true, false, out dtTmp);
                    // FI 20200903 [XXXXX] Utilisation de OTCmlHelper.GetDateSysUTC
                    DateTime dtsys = OTCmlHelper.GetDateSysUTC(CciTradeCommon.CSCacheOn);
                    if (null != cciOrderEntered)
                    {
                        dtOrderEntered = Tz.Tools.FromTimeZone(dtsys, Tz.Tools.UniversalTimeZone);
                        cciOrderEntered.NewValue = Tz.Tools.ToString(dtOrderEntered);
                    }
                    if (null != cciExecutionDateTime)
                    {
                        dtExecution = Tz.Tools.FromTimeZone(dtsys, Tz.Tools.UniversalTimeZone);
                        cciExecutionDateTime.NewValue = Tz.Tools.ToString(dtExecution);
                    }

                    if (pCci.IsInputByUser)
                    {
                        ResetCciFacilityHasChanged();
                        // FI 20190716 [XXXXX] call InitCciMarket
                        InitCciMarketFromFacility();
                    }

                    #endregion La plateforme a changé
                }

                if (IsCciMarketFacility(pCci) || CciTradeCommon.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) || IsCciExchange(pCci))
                {
                    #region Le marché ou book du dealer a changé
                    // -----------------------------------------------------------------------------------------------------------------
                    // On calcule DTBUSINESS =  MAx(ENTITYMARKET.Date, EXECUTIONDATETIME.Date)
                    // 1. On applique le CUTOFF du marché pour ajuster DTBUSINESS 
                    // -----------------------------------------------------------------------------------------------------------------
                    if (null != cciClearedDate)
                    {
                        DateTime dtBusiness = DateTime.MinValue;
                        if (this.CciTradeCommon.TradeCommonInput.IsAllocation)
                        {
                            #region Depouil/Alloc
                            // Lecture de la date Business dans EntityMarket
                            int idAEntity = CciTradeCommon.DataDocument.GetFirstEntity(CciTradeCommon.CSCacheOn);
                            if (idAEntity > 0)
                            {
                                CciTradeCommon.Product.GetMarket(CciTradeCommon.CSCacheOn, null, out SQL_Market sqlMarket);
                                if (null != sqlMarket)
                                    dtBusiness = OTCmlHelper.GetDateBusiness(CciTradeCommon.CS, idAEntity, sqlMarket.Id, IdA_Custodian);
                            }

                            if (DateTime.MinValue != dtBusiness)
                            {
                                // FI 20180130 [23737] Modify
                                // Initialize of dtcleared only when isInitFromdtBusiness is true
                                bool isInitFromdtBusiness = (false == dtCleared.HasValue) || (dtBusiness.CompareTo(dtCleared.Value) > 0);
                                if (isInitFromdtBusiness)
                                {
                                    cciClearedDate.NewValue = DtFunc.DateTimeToStringDateISO(dtBusiness);
                                    dtCleared = new EFS_Date(cciClearedDate.NewValue).DateValue;
                                    isApplyCutOff = dtExecution.HasValue;
                                }
                            }
                            #endregion Depouil/Alloc
                        }
                        else if ((false == dtCleared.HasValue) && dtExecution.HasValue)
                        {
                            cciClearedDate.NewValue = DtFunc.DateTimeToStringDateISO(dtExecution.Value.Date);
                        }

                        // FI 20180117 [23729] Il ne faut pas effacer la date éventuellement déjà saisie. Elle sera utilisée pour alimenter ENTITYMARKET 
                        //else
                        //{
                        //    cciClearedDate.NewValue = dtCleared.Value ;
                        //}
                        
                    }
                    #endregion Le marché ou book du dealer a changé
                }

                // -----------------------------------------------------------------------------------------------------------------
                // C'est la date d'entrée qui a été saisie|modifiée
                // => On met éventuellement à jour la date d'exécution si non renseignée
                // -----------------------------------------------------------------------------------------------------------------
                if (IsCciOrderEntered(pCci) && dtOrderEntered.HasValue)
                {
                    if ((null != CciExecutionDateTime) && (false == dtExecution.HasValue))
                    {
                        Nullable<DateTimeOffset> dtOrderEnteredInTimeZone = Tz.Tools.FromTimeZone(dtOrderEntered, timezone);
                        cciExecutionDateTime.NewValue = Tz.Tools.ToString(dtOrderEnteredInTimeZone);
                    }
                }

                // -----------------------------------------------------------------------------------------------------------------
                // C'est la date d'exécution qui a été saisie|modifiée
                // => On met éventuellement à jour la ClearedDate si non renseignée ou inférieure à la date d'exécution
                // -----------------------------------------------------------------------------------------------------------------
                if (isApplyCutOff)
                {
                    // On applique le fuseau horaire sur la date d'exécution
                    Nullable<DateTimeOffset> dtExecutionInTimeZone = Tz.Tools.FromTimeZone(dtExecution, timezone);
                    if (dtExecutionInTimeZone.HasValue && (dtCleared.Value.Date <= dtExecutionInTimeZone.Value.Date))
                    {
                        dtCleared = ApplyCutOff(dtExecutionInTimeZone);
                        if (DtFunc.IsDateTimeFilled(dtCleared.Value))
                            cciClearedDate.NewValue = DtFunc.DateTimeToStringDateISO(dtCleared.Value);
                    }
                }
            }
        }
        #endregion InitializeDates

        /// <summary>
        /// Reset des Ccis suite à modification de la plateforme
        /// </summary>
        /// EG 20171113 [23509] New 
        public virtual void ResetCciFacilityHasChanged()
        {
            // FI 20200116 [25141] Appel à CciExchange.Reset()
            if (null != CciExchange)
                CciExchange.Reset();
        }
        /// <summary>
        ///  Initilisation du marché en fonction de la platefome
        /// </summary>
        /// FI 20190716 [XXXXX] Add Method
        /// FI 20200116 [25141] Refactoring 
        public void InitCciMarketFromFacility()
        {
            if (false == this.CciTradeCommon.Ccis.IsModeIO && (null != CciMarketFacility.Sql_Table) && (null != CciExchange))
            {
                string sql = string.Empty;

                // Cas ou le contrôle est une DDL
                if (StrFunc.IsFilled(CciExchange.ListRetrieval) && CciExchange.ListRetrieval.StartsWith("sql:"))
                {
                    string[] array = CciExchange.ListRetrieval.Split(":".ToCharArray(), 2);
                    if ((array.Length >= 2) && Enum.IsDefined(typeof(Cst.ListRetrievalEnum), array[0].ToUpper()))
                    {
                        Cst.ListRetrievalEnum listRetrieval = (Cst.ListRetrievalEnum)Enum.Parse(typeof(Cst.ListRetrievalEnum), array[0], true);
                        if (listRetrieval == Cst.ListRetrievalEnum.SQL)
                            sql = CciTradeCommon.ReplaceTradeDynamicConstantsWithValues(CciExchange, array[1]);
                    }
                }
                else
                {
                    // Cas ou le contrôle est une TEXTBOX autocomplete
                    string columnSQL = CciExchangeColumn;
                    if (StrFunc.IsEmpty(columnSQL))
                        throw new NullReferenceException("CciExchangeColumn is null");

                    SQLWhere where = new SQLWhere("m.FACILITY = '%%FACILITY%%'");
                    if (this.Product.IsExchangeTradedDerivative)
                        where.Append("m.ISTRADEDDERIVATIVE=1");
                    else if (this.Product.IsCommoditySpot)
                        where.Append("m.ISCOMMODITYSPOT=1");
                    else if (this.Product.IsEquitySecurityTransaction)
                        where.Append("m.ISEQUITYMARKET=1");
                    else
                        where.Append("m.ISTRADEDDERIVATIVE=0");
                    
                    where.Append(OTCmlHelper.GetSQLDataDtEnabled(CciTradeCommon.CSCacheOn, "m"));

                    sql = StrFunc.AppendFormat($@"select {columnSQL}
                    from dbo.VW_MARKET_FACILITY m 
                    {where}");

                    sql = CciTradeCommon.ReplaceTradeDynamicConstantsWithValues(CciMarketFacility, sql);
                }

                if (StrFunc.IsFilled(sql))
                {
                    using (IDataReader dr = DataHelper.ExecuteReader(CciTradeCommon.CSCacheOn, CommandType.Text, sql))
                    {
                        int count = 0;
                        string defaultMarket = string.Empty;
                        while (dr.Read())
                        {
                            if (count == 0)
                                defaultMarket = dr[0].ToString();

                            count++;
                            if (count > 1)
                                break;
                        }

                        if (count == 1)
                            CciExchange.NewValue = defaultMarket;
                    }
                }
            }
        }

        /// <summary>
        /// Application du Cutoff du marché
        /// </summary>
        /// <param name="pDtExecutionInTimeZone">Date d'exécution convertie dans le timezone de la plateforme</param>
        /// <param name="pDateToApplyCutoff">Date à modifier si dépassement du cutoff</param>
        /// EG 20171115 [23509] Upd
        public Nullable<DateTime> ApplyCutOff(Nullable<DateTimeOffset> pDtExecutionInTimeZone)
        {
            Nullable<DateTime> ret = pDtExecutionInTimeZone.Value.Date;
            // On récupère le CutOff
            TimeSpan cutOffSpan = CciTradeCommon.DataDocument.GetCutOffMarketValue(GetCutOffMarket());
            double executionMilliseconds = pDtExecutionInTimeZone.Value.TimeOfDay.TotalMilliseconds;
            // On récupère le nombre de ticks du jour de la date d'exécution et du cutoff
            // si dépassement alors on ajout une journée business à la date d'éxécution
            if (cutOffSpan.TotalMilliseconds <= executionMilliseconds)
                ret = CciTradeCommon.Product.GetNextBusinessDate(CciTradeCommon.CSCacheOn, pDtExecutionInTimeZone.Value.Date);
            else
            {
                // RD 20201008 [25225] Vérifier si la date est oeuvrée ou pas
                EFS_BusinessCenters efs_bc = GetBCSMarket();
                if (efs_bc!=null && efs_bc.businessCentersSpecified)
                {
                    if (efs_bc.IsHoliday(pDtExecutionInTimeZone.Value.Date, DayTypeEnum.ExchangeBusiness))
                        ret = CciTradeCommon.Product.GetNextBusinessDate(CciTradeCommon.CSCacheOn, pDtExecutionInTimeZone.Value.Date);
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne le CCI utilisé comme référence pour calculer les dates de paiements (GrossAmount, Opp, etc..)
        /// </summary>
        /// <returns></returns>
        public CustomCaptureInfo GetCCiReferenceForPaymentDate()
        {
            CustomCaptureInfo ret;
            if (CciTradeCommon.TradeCommonInput.IsAllocation)
            {
                // FI 20190625 [XXXXX] La date de référence est la date d'execution pour les EquitySecurityTransaction et isDebtSecurityTransaction
                if (CciTradeCommon.TradeCommonInput.Product.IsEquitySecurityTransaction ||
                    CciTradeCommon.TradeCommonInput.Product.IsDebtSecurityTransaction)
                    ret = CciExecutionDateTime;
                else
                    ret = CciClearedDate;
            }
            else
                ret = CciExecutionDateTime;

            return ret;
        }
        /// <summary>
        /// Retourne true si le {pCci} est le cci de référence pour calculer les dates de paiements (GrossAmount, Opp, etc..)
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCCiReferenceForInitPaymentDate(CustomCaptureInfo pCci)
        {
            Boolean ret = false;
            CustomCaptureInfo cci = GetCCiReferenceForPaymentDate();
            if (null != cci)
                ret = (cci.ClientId_WithoutPrefix == pCci.ClientId_WithoutPrefix);
            return ret;
        }

        /// <summary>
        /// Initialisation du membre _product
        /// </summary>
        /// <param name="pProduct"></param>
        /// FI 20200124 [XXXXX] Add Method
        private void InitProduct(IProduct pProduct)
        {
            Product = null;
            if (null != pProduct)
            {
                if (pProduct.ProductBase.IsStrategy)
                    Product = new StrategyContainer((IStrategy)pProduct, CciTradeCommon.DataDocument);
                else
                    Product = new ProductContainer(pProduct, CciTradeCommon.DataDocument);
            }
        }
    }
}

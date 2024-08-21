#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI.CCI;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Concerne référentiel titres et les trades de marché, les trades Déposit
    /// </summary>
    public abstract class CciTradeBase : CciTradeCommonBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        public CciExtends cciExtends;

        /// <summary>
        /// 
        /// </summary>
        public CciTradeRemove cciTradeRemove;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la collection ccis 
        /// </summary>
        public new TradeCustomCaptureInfos Ccis => (TradeCustomCaptureInfos)base.Ccis;

        public override string PrefixHeader
        {
            get { return TradeCommonCustomCaptureInfos.CCst.Prefix_tradeHeader; }
        }
        #endregion Accessors
        
        #region Constructors
        public CciTradeBase(TradeCustomCaptureInfos pCcis)
            : base((TradeCommonCustomCaptureInfos)pCcis)
        {

            if (TradeCommonInput.IsExtend())
                cciExtends = new CciExtends(this, CurrentTrade.Extends, TradeCustomCaptureInfos.CCst.Prefix_tradeExtends);
            
            if (null != TradeCommonInput.RemoveTrade)
                cciTradeRemove = new CciTradeRemove(this, TradeCommonInput.RemoveTrade, TradeCommonCustomCaptureInfos.CCst.Prefix_remove);
            
            
            //Initialize(PrefixHeader);
        }
        #endregion Constructors
        
        #region Interfaces
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            if (null != cciExtends)
                cciExtends.AddCciSystem();

            if (null != cciTradeRemove)
                cciTradeRemove.AddCciSystem();

        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {

            base.CleanUp();
            
            if (null != cciTradeRemove)
                cciTradeRemove.CleanUp();
            
            if (null != cciExtends)
            {
                cciExtends.CleanUp();
                CurrentTrade.ExtendsSpecified = (null != CurrentTrade.Extends) && (ArrFunc.IsFilled(CurrentTrade.Extends.TradeExtend) && (0 < CurrentTrade.Extends.TradeExtend.Length));
            }

            CleanUpTradeSide();

        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            
            if (null != cciTradeRemove)
                cciTradeRemove.RefreshCciEnabled();
            
            if (null != cciExtends)
                cciExtends.RefreshCciEnabled();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {
            base.Dump_ToDocument();

            if (null != cciExtends)
            {
                cciExtends.Dump_ToDocument();
                CurrentTrade.ExtendsSpecified = (null != CurrentTrade.Extends) && ArrFunc.IsFilled(CurrentTrade.Extends.TradeExtend);
            }

            if (null != cciTradeRemove)
                cciTradeRemove.Dump_ToDocument();

            if (Cst.Capture.IsModeInput(Ccis.CaptureMode) && Ccis.IsQueueEmpty)
                LastDump_ToDocument();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            base.Initialize_FromCci();

            if (null != cciExtends)
                cciExtends.Initialize_FromCci();

            if (null != cciTradeRemove)
                cciTradeRemove.Initialize_FromCci();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {

            base.Initialize_FromDocument();

            if (null != cciExtends)
                cciExtends.Initialize_FromDocument();

            //Postionnement du Sens (Side)
            InitializePartySide();

            if (null != cciTradeRemove)
                cciTradeRemove.Initialize_FromDocument();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return base.IsClientId_PayerOrReceiver(pCci);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsCci_Extends(CustomCaptureInfo pCci)
        {
            bool ret = false;
            if (null != cciExtends)
                ret = cciExtends.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
            return ret;

        }
        
        /// <summary>
        /// Fonction de replace des constantes 'spéciales' dynamiques du trade
        /// </summary>
        /// <param name="pInitialString">string: chaine dans laquelle remplacer les valeurs const</param>
        /// <returns>string: chaine mise à jour</returns>
        /// FI 20180122 [23734] Modify
        public override string ReplaceTradeDynamicConstantsWithValues(CustomCaptureInfo pCci, string pInitialString)
        {

            string ret = pInitialString;
            //
            if (StrFunc.IsFilled(ret))
            {
                // FI 20180514 [23734] [23950] en priorité faire le cciProduct.ReplaceTradeDynamicConstantsWithValues 
                // Sur les strtatégie %%ID_INSTRUMENT%% est remplacé par IDI de la Jambe à la quelle appartient {pCci} 
                // Si l'appel n'est pas effectué %%ID_INSTRUMENT%% est remplacé par L'IDI du produit STGexchangeTradedDerivative
                if (null != cciProduct)
                    ret = cciProduct.ReplaceTradeDynamicConstantsWithValues(pCci, ret);

                // 20090917 PM : Ajout tests de presence des variables dans la chaine avant des les évaluer
                // TRD_IDA_BUYER 
                if (ret.Contains(Cst.TRD_IDA_BUYER))
                {
                    int buyer_IDA = DataDocument.GetOTCmlId_Buyer();
                    //if (buyer_IDA > -1)
                    ret = ret.Replace(Cst.TRD_IDA_BUYER, buyer_IDA.ToString());
                }

                // TRD_IDA_SELLER 
                if (ret.Contains(Cst.TRD_IDA_SELLER))
                {
                    int seller_IDA = DataDocument.GetOTCmlId_Seller();
                    //if (seller_IDA > -1)
                    ret = ret.Replace(Cst.TRD_IDA_SELLER, seller_IDA.ToString());
                }

                // TRD_IDI_INSTRUMENTMASTER 
                if (ret.Contains(Cst.TRD_IDI_INSTRUMENTMASTER))
                {
                    Nullable<int> masterIdI = Product.GetIdI();
                    if (null == masterIdI)
                        masterIdI = -1;
                    ret = ret.Replace(Cst.TRD_IDI_INSTRUMENTMASTER, masterIdI.ToString());
                }

                // TRD_IDI_INSTRUMENTUNDERLYER 
                if (ret.Contains(Cst.TRD_IDI_INSTRUMENTUNDERLYER))
                {
                    Nullable<int> underlyingIdI = Product.GetUnderlyingAssetIdI();
                    if (null == underlyingIdI)
                        underlyingIdI = -1;
                    //if (null != underlyingIdI)
                    ret = ret.Replace(Cst.TRD_IDI_INSTRUMENTUNDERLYER, underlyingIdI.ToString());
                }

                // TRD_IDT_UNDERLYER 
                if (ret.Contains(Cst.TRD_ID_UNDERLYER))
                {
                    // EG 20150402 [POC] Add CS parameter to read FXRateAsset default for FX
                    Nullable<int> underlyingId = Product.GetUnderlyingAssetId(CS);
                    if (null == underlyingId)
                        underlyingId = -1;
                    //if (null != underlyingIdT)
                    ret = ret.Replace(Cst.TRD_ID_UNDERLYER, underlyingId.ToString());
                }

                // TRD_CURRENCY
                if (ret.Contains(Cst.TRD_CURRENCY))
                {
                    string currency_IDC = Product.GetMainCurrency(CS);
                    //if (StrFunc.IsFilled(currency_IDC))
                    ret = ret.Replace(Cst.TRD_CURRENCY, currency_IDC);
                }

                // TRD_CURRENCY1 
                if (ret.Contains(Cst.TRD_CURRENCY1))
                {
                    string currency1_IDC = Product.GetCurrency1(CS);
                    //if (StrFunc.IsFilled(currency1_IDC))
                    ret = ret.Replace(Cst.TRD_CURRENCY1, currency1_IDC);
                }

                // TRD_CURRENCY2 
                if (ret.Contains(Cst.TRD_CURRENCY2))
                {
                    string currency2_IDC = Product.GetCurrency2(CS);
                    //if (StrFunc.IsFilled(currency2_IDC))
                    ret = ret.Replace(Cst.TRD_CURRENCY2, currency2_IDC);
                }

                // TRD_MARKET
                if (ret.Contains(Cst.TRD_MARKET))
                {
                    string market = Product.GetMarket();
                    ret = ret.Replace(Cst.TRD_MARKET, market);
                }

                // TRD_DERIVATIVE_CONTRACT
                if (ret.Contains(Cst.TRD_DERIVATIVE_CONTRACT))
                {
                    string derivativeContract = Product.GetDerivativeContract();
                    ret = ret.Replace(Cst.TRD_DERIVATIVE_CONTRACT, derivativeContract);
                }

                ret = base.ReplaceTradeDynamicConstantsWithValues(pCci, ret);
            }
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);

            if (null != cciExtends)
                cciExtends.ProcessInitialize(pCci);

            if (null != cciTradeRemove)
                cciTradeRemove.ProcessInitialize(pCci);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {

            base.SetDisplay(pCci);
            
            if (null != cciExtends)
                cciExtends.SetDisplay(pCci);

            if (null != cciTradeRemove)
                cciTradeRemove.SetDisplay(pCci);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion IContainerCciFactory Members
        
        #region ITradeGetInfoButton Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            base.SetButtonReferential(pCci, pCo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return base.SetButtonZoom(pCci, pCo, ref  pIsSpecified, ref pIsEnabled);
        }
        #endregion ITradeGetInfoButton Members

        
        #region IContainerCciPayerReceiver Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            base.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion IContainerCciPayerReceiver Members

        #region IContainerCciQuoteBasis Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return  base.GetBaseCurrency(pCci);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            return base.GetCurrency1(pCci);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            return base.GetCurrency2(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return  base.IsClientId_QuoteBasis(pCci);
        }

        #endregion IContainerCciQuoteBasis Members
        #endregion Interfaces
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        private void CleanUpTradeSide()
        {
            if (ArrFunc.IsFilled(CurrentTrade.TradeSide))
            {
                for (int i = CurrentTrade.TradeSide.Length - 1; -1 < i; i--)
                {
                    if ((false == CaptureTools.IsDocumentElementValid(CurrentTrade.TradeSide[i].Creditor.Account.HRef))
                           &&
                        (false == CaptureTools.IsDocumentElementValid(CurrentTrade.TradeSide[i].Creditor.Party.HRef)))
                        ReflectionTools.RemoveItemInArray(CurrentTrade, "tradeSide", i);
                }
            }
            CurrentTrade.TradeSideSpecified = ArrFunc.IsFilled(CurrentTrade.TradeSide);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        public override int GetArrayElementDocumentCount(string pPrefix, string pParentClientId, int pParentOccurs)
        {
            int ret = base.GetArrayElementDocumentCount(pPrefix, pParentClientId, pParentOccurs);
            if (-1 == ret)
            {
                if (TradeCustomCaptureInfos.CCst.Prefix_otherPartyPayment == pPrefix)
                    ret = ArrFunc.Count(CurrentTrade.OtherPartyPayment);
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_OtherPartyPaymentPayerReceiver(CustomCaptureInfo pCci)
        {
            return base.IsClientId_OtherPartyPaymentPayerReceiver(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciOtherPartyPayment"></param>
        public void SetClientIdDefaultReceiverToOtherPartyPayment(CciPayment[] pCciOtherPartyPayment)
        {

            string ret = string.Empty;
            for (int j = 0; j < ArrFunc.Count(pCciOtherPartyPayment); j++)
            {
                bool isOk = false;
                // Brokers ??
                for (int i = 0; i < BrokerLength; i++)
                {
                    if (StrFunc.IsFilled(Ccis[cciBroker[i].CciClientId(CciTradeParty.CciEnum.actor)].NewValue))
                    {
                        if (Ccis[cciBroker[i].CciClientId(CciTradeParty.CciEnum.actor)].NewValue != Ccis[pCciOtherPartyPayment[j].CciClientId(CciPayment.CciEnumPayment.payer)].NewValue)
                        {
                            isOk = true;
                            ret = cciBroker[i].CciClientId(CciTradeParty.CciEnum.actor);
                            break;
                        }
                    }
                }
                //
                if (false == isOk)
                {
                    // actors ?? party2 after Party 1 =>  Glop il faudrait plutôt choisir vendeur ??
                    for (int i = ArrFunc.Count(cciParty) - 1; -1 < i; i--)
                    {
                        if (StrFunc.IsFilled(Ccis[cciParty[i].CciClientId(CciTradeParty.CciEnum.actor)].NewValue))
                        {
                            if (Ccis[cciParty[i].CciClientId(CciTradeParty.CciEnum.actor)].NewValue != Ccis[pCciOtherPartyPayment[j].CciClientId(CciPayment.CciEnumPayment.payer)].NewValue)
                            {
                                isOk = true;
                                ret = cciParty[i].CciClientId(CciTradeParty.CciEnum.actor);
                                break;
                            }
                        }
                    }
                }
                //
                if (isOk)
                {
                    pCciOtherPartyPayment[j].ClientIdDefaultReceiver = ret;
                }
            }

        }

        /// <summary>
        ///  Action à mener après le dernier Dump
        /// </summary>
        /// FI 20171003 [23464] Add Methode
        protected virtual void LastDump_ToDocument()
        {
            // Suppression des parties valides mais non utilisées
            // Exemple sur les ETD, la chambre de compensation est une partie
            // Si l'utilisateur change de Marché, la chambre de compensation associée à la précédente valeur du marché doit être supprimée  
            // FI 20100416 [16951] La purge des parties non utilisées ne doit être effectué uniquement après le dernier dump 
            DataDocument.RemovePartyNotUsed(CSCacheOn);
        }

        #endregion Methods
    }
}

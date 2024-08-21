#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciProvisionBase.
    /// </summary>
    public class CciProvisionBase : IContainerCci, IContainerCciFactory, IContainerCciGetInfoButton
    {
        #region Members
        public TradeCustomCaptureInfos ccis;
        protected CciTradeBase cciTrade;
        protected IProduct product;
        protected string prefix;
        #endregion Members

        #region Property
        #region ImplicitEarlyTerminationProvisionSpecified
        protected bool ImplicitEarlyTerminationProvisionSpecified
        {
            get { return product.ProductBase.ImplicitEarlyTerminationProvisionSpecified; }
        }
        #endregion ImplicitEarlyTerminationProvisionSpecified
        #region ImplicitProvisionSpecified
        protected bool ImplicitProvisionSpecified
        {
            get { return product.ProductBase.ImplicitProvisionSpecified; }
        }
        #endregion ImplicitProvisionSpecified
        #region ImplicitProvision
        protected IImplicitProvision ImplicitProvision
        {
            get { return (product.ProductBase.ImplicitProvision); }
            set { product.ProductBase.ImplicitProvision = value; }
        }
        #endregion ImplicitProvision
        #region ImplicitCancelableProvisionSpecified
        protected bool ImplicitCancelableProvisionSpecified
        {
            get { return product.ProductBase.ImplicitCancelableProvisionSpecified; }
            set { product.ProductBase.ImplicitCancelableProvisionSpecified = value; }
        }
        #endregion ImplicitCancelableProvisionSpecified
        #region ImplicitExtendibleProvisionSpecified
        protected bool ImplicitExtendibleProvisionSpecified
        {
            get { return product.ProductBase.ImplicitExtendibleProvisionSpecified; }
            set { product.ProductBase.ImplicitExtendibleProvisionSpecified = value; }
        }
        #endregion ImplicitExtendibleProvisionSpecified

        #region ImplicitMandatoryEarlyTerminationProvisionSpecified
        protected bool ImplicitMandatoryEarlyTerminationProvisionSpecified
        {
            get { return product.ProductBase.ImplicitMandatoryEarlyTerminationProvisionSpecified; }
            set { product.ProductBase.ImplicitMandatoryEarlyTerminationProvisionSpecified = value; }
        }
        #endregion ImplicitMandatoryEarlyTerminationProvisionSpecified
        #region ImplicitOptionalEarlyTerminationProvisionSpecified
        protected bool ImplicitOptionalEarlyTerminationProvisionSpecified
        {
            get { return product.ProductBase.ImplicitOptionalEarlyTerminationProvisionSpecified; }
            set { product.ProductBase.ImplicitOptionalEarlyTerminationProvisionSpecified = value; }
        }
        #endregion ImplicitOptionalEarlyTerminationProvisionSpecified

        #region CancelableProvisionSpecified
        protected bool CancelableProvisionSpecified
        {
            get { return product.ProductBase.CancelableProvisionSpecified; }
        }
        #endregion CancelableProvisionSpecified
        #region ExtendibleProvisionSpecified
        protected bool ExtendibleProvisionSpecified
        {
            get { return product.ProductBase.ExtendibleProvisionSpecified; }
        }
        #endregion ExtendibleProvisionSpecified

        #region EarlyTerminationProvisionSpecified
        protected bool EarlyTerminationProvisionSpecified
        {
            get { return product.ProductBase.EarlyTerminationProvisionSpecified; }
        }
        #endregion EarlyTerminationProvisionSpecified
        #region MandatoryEarlyTerminationProvisionSpecified
        protected bool MandatoryEarlyTerminationProvisionSpecified
        {
            get { return product.ProductBase.MandatoryEarlyTerminationProvisionSpecified; }
        }
        #endregion MandatoryEarlyTerminationProvisionSpecified
        #region OptionalEarlyTerminationProvisionSpecified
        protected bool OptionalEarlyTerminationProvisionSpecified
        {
            get { return product.ProductBase.OptionalEarlyTerminationProvisionSpecified; }
        }
        #endregion OptionalEarlyTerminationProvisionSpecified
        #endregion Property

        #region Constructor
        public CciProvisionBase(CciTradeBase pTrade) : this(pTrade, string.Empty) { }
        public CciProvisionBase(CciTradeBase pTrade, string pPrefix)
        {
            cciTrade = pTrade;
            ccis = (TradeCustomCaptureInfos) cciTrade.Ccis;
            product = cciTrade.CurrentTrade.Product;
            prefix = pPrefix;
            if (StrFunc.IsFilled(prefix))
                prefix += CustomObject.KEY_SEPARATOR;
        }
        #endregion Constructor

        #region IContainerCciFactory members
        #region AddCciSystem
        public virtual void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region Dump_ToDocument
        public virtual void Dump_ToDocument()
        {
        }
        #endregion Dump_ToDocument
        #region Initialize_FromCci
        public virtual void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, product);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public virtual void Initialize_FromDocument()
        {
        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region IsClientId_PayerOrReceiver
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region CleanUp
        public virtual void CleanUp()
        {
        }
        #endregion CleanUp
        #region SetDisplay
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion SetDisplay
        #region RefreshCciEnabled
        public virtual void RefreshCciEnabled()
        {
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public virtual void RemoveLastItemInArray(string pPrefix)
        {
        }
        #endregion RemoveLastItemInArray
        #region Initialize_Document
        public virtual void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion IContainerCciFactory members

        #region IContainerCci members
        #region CciClientId
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return ccis[CciClientId(pClientId_Key)];
        }
        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #endregion IContainerCci members

        #region IContainerCciGetInfoButton members
        #region SetButtonZoom
        public virtual bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

        }
        #endregion
        #endregion IContainerCciGetInfoButton members

        #region Methods
        #region GetBuyerPartyReference
        public string GetBuyerPartyReference()
        {
            //foreach (IParty party in ccis.TradeCaptureGen.TradeCommonInput.DataDocument.party)
            //{
            //    if (ccis.TradeCaptureGen.TradeCommonInput.DataDocument.IsPartyBuyer(party))
            //        return party.id;
            //}
            //return null;

            foreach (IParty party in ccis.TradeCommonInput.DataDocument.Party)
            {
                if (ccis.TradeCommonInput.DataDocument.IsPartyBuyer(party))
                    return party.Id;
            }
            return null;

        }
        #endregion GetBuyerPartyReference
        #region GetCalculationAgent
        public string GetCalculationAgent()
        {
            string calculationAgent = string.Empty;
            foreach (CciTradeParty party in cciTrade.cciParty)
            {
                calculationAgent = party.GetCalculationAgent();
                if (StrFunc.IsFilled(calculationAgent))
                    break;
            }
            return calculationAgent;
        }
        #endregion GetCalculationAgent
        #region GetCalculationAgentBC
        public string GetCalculationAgentBC()
        {
            string calculationAgentBC = string.Empty;
            foreach (CciTradeParty party in cciTrade.cciParty)
            {
                calculationAgentBC = party.GetCalculationAgentBC();
                if (StrFunc.IsFilled(calculationAgentBC))
                    break;
            }
            return calculationAgentBC;
        }
        #endregion GetCalculationAgentBC
        #region GetSellerPartyReference
        public string GetSellerPartyReference()
        {
            //foreach (IParty party in ccis.TradeCaptureGen.TradeCommonInput.DataDocument.party)
            //{
            //    if (ccis.TradeCaptureGen.TradeCommonInput.DataDocument.IsPartySeller(party))
            //        return party.id;
            //}

            foreach (IParty party in ccis.TradeCommonInput.DataDocument.Party)
            {
                if (ccis.TradeCommonInput.DataDocument.IsPartySeller(party))
                    return party.Id;
            }
            return null;
        }
        #endregion GetSellerPartyReference

        #endregion Members
    }
}

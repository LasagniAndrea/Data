#region Using Directives
using EFS.Common.Web;
using EFS.GUI.CCI;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciTradeAdminBase
    /// <summary>
    /// Description résumée de CciTradeAdminBase.
    /// </summary>
    public abstract class CciTradeAdminBase : CciTradeCommonBase 
    {
        #region Members
        public CciTradeAdminRemove cciTradeAdminRemove;
        #endregion Members

        #region Accessors
        #region PrefixHeader
        public override string PrefixHeader
        {
            get { return TradeCommonCustomCaptureInfos.CCst.Prefix_tradeAdminHeader; }
        }
        #endregion PrefixHeader
        #region ccis
        /// <summary>
        /// Obtient la collection ccis 
        /// </summary>
        public new TradeAdminCustomCaptureInfos Ccis
        {
            get { return (TradeAdminCustomCaptureInfos)base.Ccis; }
        }
        #endregion Ccis

        #region TradeCommonInput
        /// <summary>
        /// Obtient TradeCommonInput
        /// </summary>
        public TradeAdminInput TradeAdminInput
        {
            get { return (TradeAdminInput)TradeCommonInput; }
        }
        #endregion TradeCommonInput

        #endregion Accessors
        //
        #region Constructors
        public CciTradeAdminBase(TradeAdminCustomCaptureInfos pCcis)
            : base((TradeCommonCustomCaptureInfos)pCcis)
        {
            if (null != TradeCommonInput.RemoveTrade)
                cciTradeAdminRemove = new CciTradeAdminRemove(this, TradeCommonInput.RemoveTrade, TradeAdminInput.LinkedTradeAdminRemove, TradeCommonCustomCaptureInfos.CCst.Prefix_tradeAdminRemove);
            //Initialize(PrefixHeader);
        }
        #endregion Constructors
        //
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.CleanUp();
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public override void Dump_ToDocument()
        {
            base.Dump_ToDocument();
            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region Initialize_FromCci
        public override void Initialize_FromCci()
        {
            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.Initialize_FromCci();
            cciTradeHeader.Initialize_FromCci();
            for (int i = 0; i < cciParty.Length; i++)
                cciParty[i].Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public override void Initialize_FromDocument()
        {
            base.Initialize_FromDocument();
            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.Initialize_FromDocument();
            InitializePartySide();
        }
        #endregion
        #region IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return base.IsClientId_PayerOrReceiver(pCci);
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        // FI 20170928 [23452] Modify
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);

            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.ProcessInitialize(pCci);

            // FI 20170928 [23452] Fin appel à ccis.Finalize
            //ccis.Finalize(pCci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public override void RemoveLastItemInArray(string pPrefix)
        {
            base.RemoveLastItemInArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            if (null != cciTradeAdminRemove)
                cciTradeAdminRemove.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCciQuoteBasis Members
        #region GetBaseCurrency
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return string.Empty;
        }
        #endregion GetBaseCurrency
        //#region GetCurrency1
        //public virtual string GetCurrency1(CustomCaptureInfo pCci)
        //{
        //    return string.Empty;
        //}
        //#endregion GetCurrency1
        //#region GetCurrency2
        //public virtual string GetCurrency2(CustomCaptureInfo pCci)
        //{
        //    return string.Empty;
        //}
        //#endregion GetCurrency2
        //#region IsClientId_QuoteBasis
        //public virtual bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        //{

        //    return false;
        //}
        //#endregion IsClientId_QuoteBasis
        #endregion IContainerCciQuoteBasis Members
        #region ITradeGetInfoButton Members
        #region IsButtonMenu
        public override bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            return false;
        }
        #endregion IsButtonMenu
        #region SetButtonReferential
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #endregion ITradeGetInfoButton Members

        #endregion Interfaces
    }
    #endregion CciTradeAdminBase
}

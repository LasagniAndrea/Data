#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI.CCI;

using EfsML.Business;

using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciProductBulletPayment
    /// <summary>
    /// Description résumée de CciProductBulletPayment.
    /// </summary>
    // EG 20171109 [23509] Upd CciTradeBase remplace CciTrade
    public class CciProductBulletPayment : CciProductBase
    {
        #region Members
        
        private CciPayment _cciPayment;
        private IBulletPayment _bulletPayment;
        #endregion Members

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get
            {
                return base.CcisBase as TradeCustomCaptureInfos;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTradeBase CciTrade
        {
            get { return base.CciTradeCommon as CciTradeBase; }
        }
        #endregion

        #region constructor
        // EG 20171109 [23509] Upd CciTradeBase remplace CciTrade
        public CciProductBulletPayment(CciTradeBase pCciTrade, IBulletPayment pBulletPayment, string pPrefix)
            : this(pCciTrade, pBulletPayment, pPrefix, -1)
        { }
        public CciProductBulletPayment(CciTradeBase pCciTrade, IBulletPayment pBulletPayment, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pBulletPayment, pPrefix, pNumber)
        {
        }
        #endregion constructor
        //
        #region Membres de ITradeCci
        #region public override RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetSellSide(); } }
        #endregion
        #region public override RetSideReceiver
        public override string RetSideReceiver { get { return SideTools.RetBuySide(); } }
        #endregion
        #region public override GetMainCurrency
        public override string GetMainCurrency
        {
            get
            {
                return _bulletPayment.Payment.PaymentCurrency;

            }
        }
        #endregion GetMainCurrency
        #region public override CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get
            {
                return _cciPayment.CciClientId(CciPayment.CciEnumPayment.paymentAmount_currency);
            }
        }
        #endregion
        #endregion  Membres de ITrade
        //
        #region Membres de IContainerCciPayerReceiver
        #region public override CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return _cciPayment.CciClientIdPayer; }
        }
        #endregion
        #region public override CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return _cciPayment.CciClientIdReceiver; }
        }
        #endregion
        #region public override SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            _cciPayment.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion
        #endregion
        //
        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            _cciPayment.Initialize_FromCci();

        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            _cciPayment.AddCciSystem();
        }

        #endregion
        #region public override Initialize_FromDocument
        public override void Initialize_FromDocument()
        {
            _cciPayment.Initialize_FromDocument();
        }
        #endregion
        #region public override Dump_ToDocument
        public override void Dump_ToDocument()
        {
            _cciPayment.Dump_ToDocument();
            //
            if (StrFunc.IsEmpty(_cciPayment.Money.Id))
                _cciPayment.Money.Id = CciTrade.DataDocument.GenerateId(TradeCustomCaptureInfos.CCst.NOTIONAL_REFERENCE, false);

        }
        #endregion
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            _cciPayment.ProcessInitialize(pCci);
            
            if ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                CciTrade.InitializePartySide();
            
        }
        #endregion
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return _cciPayment.IsClientId_PayerOrReceiver(pCci);

        }
        #endregion
        #region public override CleanUp
        public override void CleanUp()
        {
            _cciPayment.CleanUp();

        }
        #endregion
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            _cciPayment.RefreshCciEnabled();
        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            _cciPayment.SetDisplay(pCci);
        }
        #endregion
        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            if (Cst.Capture.IsModeNew(CciTrade.Ccis.CaptureMode) && (false == CciTrade.Ccis.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(_bulletPayment.Payment.PayerPartyReference.HRef) && StrFunc.IsEmpty(_bulletPayment.Payment.ReceiverPartyReference.HRef))
                {
                    //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    //id = GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    _bulletPayment.Payment.PayerPartyReference.HRef = id;
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }
        }
        #endregion
        #endregion
        //
        #region Membres de IContainerCciGetInfoButton
        #region public override SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            #region buttons settlementInfo
            if (!isOk)
            {
                isOk = _cciPayment.IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                if (isOk)
                {
                    pCo.Element = "settlementInformation";
                    pCo.Object = "payment";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = _cciPayment.IsSettlementInfoSpecified;
                    pIsEnabled = _cciPayment.IsSettlementInstructionSpecified;
                }
            }
            #endregion buttons settlementInfo
            return isOk;
        }
        #endregion SetButtonZoom
        #endregion
        //
        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {

            bool bRet = false;
            //
            if (false == bRet)
                bRet = _cciPayment.IsClientId_QuoteBasis(pCci);
            //
            return bRet;
        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {

            return _cciPayment.GetCurrency1(pCci);
        }
        #endregion
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            return _cciPayment.GetCurrency2(pCci);
        }
        #endregion
        #region public override GetBaseCurrency
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return _cciPayment.GetBaseCurrency(pCci);
        }
        #endregion GetBaseCurrency
        #endregion
        //
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            _bulletPayment = (IBulletPayment)pProduct;
            
            IPayment payment = null;
            if (null != _bulletPayment)
                payment = _bulletPayment.Payment;
            
            _cciPayment = new CciPayment(CciTrade, -1, payment, CciPayment.PaymentTypeEnum.Payment,
                Prefix + TradeCustomCaptureInfos.CCst.Prefix_bulletPayment);
           
            base.SetProduct(pProduct);
        }
        #endregion
    }
    #endregion
}

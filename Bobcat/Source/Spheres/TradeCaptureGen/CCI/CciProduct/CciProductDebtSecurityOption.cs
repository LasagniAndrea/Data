using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace EFS.TradeInformation
{

    public class CciProductDebtSecurityOption : CciProductOptionBaseExtended
    {
        #region Members
        private IDebtSecurityOption _debtSecurityOption;
        private CciSecurityAsset _cciSecurityAsset;
        #endregion

        #region accessor
        public IDebtSecurityOption DebtSecurityOption
        {
            get { return _debtSecurityOption; }
        }
        #endregion

        #region public Enum
        public new enum CciEnum
        {
            #region BondOptionStrike
            [System.Xml.Serialization.XmlEnumAttribute("strike.price")]
            strike_price,
            [System.Xml.Serialization.XmlEnumAttribute("strike.referenceSwapCurve")]
            strike_referenceSwapCurve,
            [System.Xml.Serialization.XmlEnumAttribute("strike.price.strikeprice")]
            strike_price_strikePrice,
            [System.Xml.Serialization.XmlEnumAttribute("strike.price.strikePercentage")]
            strike_price_strikePercentage,
            [System.Xml.Serialization.XmlEnumAttribute("strike.price.currency")]
            strike_price_currency,
            #endregion BondOptionStrike

            unknown,
        }
        #endregion Enum

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pDebtSecurityOption"></param>
        /// <param name="pPrefix"></param>
        public CciProductDebtSecurityOption(CciTrade pCciTrade, IDebtSecurityOption pDebtSecurityOption, string pPrefix)
            : this(pCciTrade, pDebtSecurityOption, pPrefix, -1)        
        {}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pDebtSecurityOption"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pNumber"></param>
        public CciProductDebtSecurityOption(CciTrade pCciTrade, IDebtSecurityOption pDebtSecurityOption, string pPrefix, int pNumber)
            : base(pCciTrade, (IOptionBaseExtended)pDebtSecurityOption, pPrefix, pNumber)
        {
            
        }
        #endregion Constructor

        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, DebtSecurityOption);
            CciSecurityAsset cciSecurityAssetCurrent = new CciSecurityAsset(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_securityAsset, DebtSecurityOption.SecurityAsset);
            bool isOk = CcisBase.Contains(cciSecurityAssetCurrent.CciClientId(CciSecurityAsset.CciEnum.securityId));
            if (isOk)
            {
                _cciSecurityAsset = cciSecurityAssetCurrent;
                if (null == DebtSecurityOption.SecurityAsset)
                    DebtSecurityOption.SecurityAsset = CciTrade.CurrentTrade.Product.ProductBase.CreateSecurityAsset();
                _cciSecurityAsset.securityAsset = DebtSecurityOption.SecurityAsset;
            }
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.Initialize_FromCci();

            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        public override void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion

                    switch (cciEnum)
                    {
                        #region strike
                        case CciEnum.strike_price_strikePrice:
                            if (DebtSecurityOption.Strike.PriceSpecified && DebtSecurityOption.Strike.Price.PriceSpecified)
                                data = DebtSecurityOption.Strike.Price.Price.Value;
                            break;
                        case CciEnum.strike_price_strikePercentage:
                            if (DebtSecurityOption.Strike.PriceSpecified && DebtSecurityOption.Strike.Price.PercentageSpecified)
                                data = DebtSecurityOption.Strike.Price.Percentage.Value;
                            break;
                        //case CciEnum.strike_price_currency:
                        //    if (DebtSecurityOption.strike.priceSpecified && DebtSecurityOption.strike.price.currencySpecified)
                        //        data = DebtSecurityOption.strike.price.currency.Value;
                        //    break;
                        #endregion
                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }

            base.Initialize_FromDocument();
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.Initialize_FromDocument();

        }

        #endregion Initialize_FromDocument

        #region public override Dump_ToDocument
        public override void Dump_ToDocument()
        {

            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables


                    switch (cciEnum)
                    {
                        #region strike
                        case CciEnum.strike_price_strikePrice:
                            DebtSecurityOption.Strike.PriceSpecified = cci.IsFilledValue;
                            DebtSecurityOption.Strike.Price.PriceSpecified = cci.IsFilledValue;
                            DebtSecurityOption.Strike.Price.PercentageSpecified = !cci.IsFilledValue;
                            if (DebtSecurityOption.Strike.Price.PriceSpecified)
                                DebtSecurityOption.Strike.Price.Price.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.strike_price_strikePercentage:
                            DebtSecurityOption.Strike.PriceSpecified = cci.IsFilledValue;
                            DebtSecurityOption.Strike.Price.PercentageSpecified = cci.IsFilledValue;
                            DebtSecurityOption.Strike.Price.PriceSpecified = !cci.IsFilledValue;
                            if (DebtSecurityOption.Strike.Price.PercentageSpecified)
                                DebtSecurityOption.Strike.Price.Percentage.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        //case CciEnum.strike_price_currency:
                        //    DebtSecurityOption.strike.price.currencySpecified = cci.IsFilledValue;
                        //    if (DebtSecurityOption.strike.price.currencySpecified)
                        //        DebtSecurityOption.strike.price.currency.Value = data;
                        //    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                        //    break;
                        #endregion
                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            base.Dump_ToDocument();
            if (null != _cciSecurityAsset)
            {
                _cciSecurityAsset.Dump_ToDocument();
            }
            SetBond();

        }
        #endregion Dump_ToDocument
        private void SetBond()
        {
            if ((null != _cciSecurityAsset) && (null != _cciSecurityAsset.cciDebtSecurity) && (null != _cciSecurityAsset.cciDebtSecurity.debtSecurity))
            {
                IDebtSecurity debtSecurity = _cciSecurityAsset.cciDebtSecurity.debtSecurity;
                _debtSecurityOption.NotionalAmountSpecified = false;
                _debtSecurityOption.NotionalAmountReferenceSpecified = false;
                ISecurity security = debtSecurity.Security;
                _debtSecurityOption.Bond.OTCmlId = _debtSecurityOption.SecurityAssetOTCmlId;
                //if ((null != security) && (0 < _debtSecurityOption.bond.OTCmlId))
                if (null != security)
                {
                    _debtSecurityOption.NotionalAmountSpecified = true;
                    _debtSecurityOption.Bond.InstrumentId = security.InstrumentId;
                    _debtSecurityOption.Bond.ClearanceSystemSpecified = security.ClearanceSystemSpecified;
                    _debtSecurityOption.Bond.ClearanceSystem = security.ClearanceSystem;
                    _debtSecurityOption.Bond.CurrencySpecified = security.CurrencySpecified;
                    _debtSecurityOption.Bond.Currency = security.Currency;
                    _debtSecurityOption.Bond.CouponTypeSpecified = security.CouponTypeSpecified;
                    _debtSecurityOption.Bond.CouponType = security.CouponType;
                    _debtSecurityOption.Bond.CouponTypeSpecified = security.CouponTypeSpecified;
                    _debtSecurityOption.Bond.CouponType = security.CouponType;
                    _debtSecurityOption.Bond.CouponRateSpecified = false;
                    _debtSecurityOption.Bond.ExchangeIdSpecified = security.ExchangeIdSpecified;
                    _debtSecurityOption.Bond.ExchangeId = security.ExchangeId;
                    _debtSecurityOption.Bond.DefinitionSpecified = security.DefinitionSpecified;
                    _debtSecurityOption.Bond.Definition = security.Definition;
                    _debtSecurityOption.Bond.DescriptionSpecified = security.DescriptionSpecified;
                    _debtSecurityOption.Bond.Description = security.Description;
                    _debtSecurityOption.Bond.FaceAmountSpecified = security.FaceAmountSpecified;
                    _debtSecurityOption.Bond.FaceAmount = new EFS_Decimal(security.FaceAmount.Amount.DecValue);
                    _debtSecurityOption.Bond.ParValueSpecified = security.FaceAmountSpecified;

                    _debtSecurityOption.Bond.IssuerPartyReferenceSpecified = _cciSecurityAsset.securityAsset.IssuerReferenceSpecified;
                    if (_debtSecurityOption.Bond.IssuerPartyReferenceSpecified)
                        _debtSecurityOption.Bond.IssuerPartyReference = CciTrade.Product.ProductBase.CreatePartyReference(_cciSecurityAsset.securityAsset.IssuerReference.HRef);

                    decimal numberOfIssuedSecurities = 1;
                    if (security.NumberOfIssuedSecuritiesSpecified)
                        numberOfIssuedSecurities = security.NumberOfIssuedSecurities.DecValue;
                    _debtSecurityOption.Bond.ParValue = new EFS_Decimal(security.FaceAmount.Amount.DecValue / numberOfIssuedSecurities);
                    if (debtSecurity.Stream[0].CalculationPeriodDates.TerminationDateAdjustableSpecified)
                    {
                        string maturityDate = debtSecurity.Stream[0].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
                        _debtSecurityOption.Bond.MaturitySpecified = StrFunc.IsFilled(maturityDate);
                        if (_debtSecurityOption.Bond.MaturitySpecified)
                            _debtSecurityOption.Bond.Maturity = new EFS_Date(maturityDate);
                    }

                    DebtSecurityOption.EntitlementCurrencySpecified = debtSecurity.Security.FaceAmountSpecified;
                    if (debtSecurity.Security.FaceAmountSpecified)
                    {
                        DebtSecurityOption.EntitlementCurrency = debtSecurity.Security.FaceAmount.GetCurrency;
                        CcisBase.SetNewValue(Cci(CciProductOptionBaseExtended.CciEnum.optionEntitlement).ClientId_WithoutPrefix,
                            StrFunc.FmtDecimalToInvariantCulture(CalculOptionEntitlement()), false);
                        CcisBase.SetNewValue(Cci(CciProductOptionBaseExtended.CciEnum.notionalAmount_currency).ClientId_WithoutPrefix, DebtSecurityOption.EntitlementCurrency.Value, false);
                    }
                }
                else
                {
                    ClearDebtSecurity();
                }
            }
            else
            {
                ClearDebtSecurity();
            }
            if (DebtSecurityOption.SecurityAssetSpecified && DebtSecurityOption.SecurityAsset.DebtSecuritySpecified)
                CciTrade.Product.SetReceiverOnSecurityAsset();
        }
        private void ClearDebtSecurity()
        {
            //PL 20150720 Add if()
            if (Cci(CciProductOptionBaseExtended.CciEnum.optionEntitlement) != null) 
                CcisBase.SetNewValue(Cci(CciProductOptionBaseExtended.CciEnum.optionEntitlement).ClientId_WithoutPrefix, string.Empty, false);
            //PL 20150720 Add if()
            if (Cci(CciProductOptionBaseExtended.CciEnum.entitlementCurrency) != null)
                CcisBase.SetNewValue(Cci(CciProductOptionBaseExtended.CciEnum.entitlementCurrency).ClientId_WithoutPrefix, string.Empty, false);
        }
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    case CciEnum.strike_price_strikePrice:
                        CcisBase.SetNewValue(Cci(CciProductOptionBaseExtended.CciEnum.optionEntitlement).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(CalculOptionEntitlement()), false);
                        break;
                    case CciEnum.strike_price_currency:
                        CcisBase.SetNewValue(Cci(CciProductOptionBaseExtended.CciEnum.notionalAmount_currency).ClientId_WithoutPrefix, pCci.NewValue, false);
                        break;
                    default:
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        break;
                }
            }

            base.ProcessInitialize(pCci);
            //ccis.SetNewValue(Cci(CciEnum.strike_price_currency).ClientId_WithoutPrefix, cciTrade.GetMainCurrency, false);
            if (null != _cciSecurityAsset)
            {
                _cciSecurityAsset.ProcessInitialize(pCci);
            }

        }
        #endregion ProcessInitialize

        #region public override CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.CleanUp();
        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.SetDisplay(pCci);
        }
        #endregion
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
        }
        #endregion

        #region Membres de IContainerCciGetInfoButton
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        // EG 20160404 Migration vs2013
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            if (null != _cciSecurityAsset)
                isOk = _cciSecurityAsset.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);

            // EG 20170220 New
            if (!isOk)
                isOk = base.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);

            return isOk;

        }
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        // EG 20160404 Migration vs2013
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.SetButtonReferential(pCci, pCo);
            base.SetButtonReferential(pCci, pCo);
        }
        #endregion Membres de IContainerCciGetInfoButton

        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            if (null != _cciSecurityAsset)
                _cciSecurityAsset.Initialize_Document();
        }
        #endregion Initialize_Document
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return base.IsClientId_PayerOrReceiver(pCci);
        }
        #endregion
        #endregion

        

        #region IContainerCciPayerReceiver Membres
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            base.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion

        #region public int GetArrayElementDocumentCount
        public int GetArrayElementDocumentCount(string pPrefix)
        {
            int ret = -1;
            if (null != _cciSecurityAsset)
            {
                if (-1 == ret && null != _cciSecurityAsset.cciDebtSecurity)
                    ret = _cciSecurityAsset.cciDebtSecurity.GetArrayElementDocumentCount(pPrefix);
            }
            return ret;
        }
        #endregion

        #region Methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            _debtSecurityOption = (IDebtSecurityOption)pProduct;
            base.SetProduct(pProduct);
        }
        #endregion



        #region ICciPresentation Membres
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (null != _cciSecurityAsset)
            {
                //Si click sur isNewAsset => affichage du panel securityAsset
                //eventTarget => Control à l'origine du post
                bool isModeConsult = Cst.Capture.IsModeConsult(CcisBase.CaptureMode);
                string eventTarget = string.Empty + pPage.Request.Params["__EVENTTARGET"];
                //
                bool isNewAsset = _cciSecurityAsset.IsNewAsset;
                CustomCaptureInfo cciNewAsset = _cciSecurityAsset.Cci(CciSecurityAsset.CciEnum.isNewAsset);
                CustomCaptureInfo cciTemplate = _cciSecurityAsset.Cci(CciSecurityAsset.CciEnum.template);
                if (StrFunc.IsFilled(eventTarget))
                {
                    bool isDisplay = (null != cciNewAsset) && (cciNewAsset.ClientId == eventTarget);
                    isDisplay = isDisplay || ((null != cciTemplate) && (cciTemplate.ClientId == eventTarget));
                    if (isDisplay)
                    {
                        string id = Cst.IMG + _cciSecurityAsset.Prefix + "tblDebtSecurityAsset";
                        pPage.ShowLinkControl(id);
                    }
                }
                Control ctrl = pPage.FindControl(cciTemplate.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isNewAsset;
                ctrl = pPage.FindControl("LBL" + cciTemplate.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = isNewAsset;
                ctrl = pPage.FindControl(cciNewAsset.ClientId);
                if (null != ctrl)
                {
                    ctrl.Visible = (false == isModeConsult);
                    if (ctrl.Parent.Parent.GetType().Equals(typeof(TableRow)))
                        ctrl.Parent.Parent.Visible = ctrl.Visible;
                }
                ctrl = pPage.FindControl("LBL" + cciNewAsset.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = (false == isModeConsult);

                _cciSecurityAsset.DumpSpecific_ToGUI(pPage);
            }
        }
        #endregion

        #region CalculOptionEntitlement
        /// <summary>
        /// OptionEntitlement est exprimé par option
        /// </summary>
        /// <returns></returns>
        protected override decimal CalculOptionEntitlement()
        {
            decimal faceAmount = 1;
            if ((null != _debtSecurityOption.DebtSecurity) && _debtSecurityOption.DebtSecurity.Security.FaceAmountSpecified)
                faceAmount = _debtSecurityOption.DebtSecurity.Security.FaceAmount.Amount.DecValue;
            decimal notionalAmount = faceAmount; 
            if (_debtSecurityOption.NotionalAmountSpecified)
                notionalAmount = _debtSecurityOption.NotionalAmount.Amount.DecValue;
            decimal optionEntitlement = notionalAmount;
            return optionEntitlement;
        }
        #endregion CalculOptionEntitlement

        #endregion


    }
 
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// CciSecurity
    /// </summary>
    public class CciSecurity :  CciUnderlyingAsset, IContainerCciFactory,  IContainerCciGetInfoButton
    {
        #region Enums
        #region CciEnum
        public new enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("couponType")]
            couponType,
            [System.Xml.Serialization.XmlEnumAttribute("priceRateType")]
            priceRateType,
            [System.Xml.Serialization.XmlEnumAttribute("numberOfIssuedSecurities")]
            numberOfIssuedSecurities,
            [System.Xml.Serialization.XmlEnumAttribute("faceAmount.amount")]
            faceAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("faceAmount.currency")]
            faceAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("price.issuePricePercentage")]
            price_issuePricePercentage,
            [System.Xml.Serialization.XmlEnumAttribute("price.redemptionPricePercentage")]
            price_redemptionPricePercentage,
            [System.Xml.Serialization.XmlEnumAttribute("purpose")]
            purpose,
            [System.Xml.Serialization.XmlEnumAttribute("commercialPaper.program")]
            commercialPaper_program,
            [System.Xml.Serialization.XmlEnumAttribute("commercialPaper.regType")]
            commercialPaper_regType,
            [System.Xml.Serialization.XmlEnumAttribute("calculationRules")]
            calculationRules,
            [System.Xml.Serialization.XmlEnumAttribute("orderRules")]
            orderRules,
            [System.Xml.Serialization.XmlEnumAttribute("quoteRules")]
            quoteRules,
            [System.Xml.Serialization.XmlEnumAttribute("indicator")]
            indicator,
            [System.Xml.Serialization.XmlEnumAttribute("yield")]
            yield,
            [System.Xml.Serialization.XmlEnumAttribute("yieldType")]
            yieldType,
            unknown
        }
        #endregion CciEnum
        #endregion Enums

        #region Members
        private readonly CciTradeBase cciTrade;
        public ISecurity security;
        
        public CciLocalization cciLocalization;
        public CciClassification cciClassification;
        #endregion

        #region Accessors

        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pSecurity"></param>
        public CciSecurity(CciTradeBase pTrade, string pPrefix, ISecurity pSecurity)
            : base(pTrade, pPrefix, (IUnderlyingAsset)pSecurity)
        {
            cciTrade = pTrade;
            security = pSecurity;
        }
        #endregion constructor

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {

            base.AddCciSystem();
            //
            string clientId_WithoutPrefix;

            clientId_WithoutPrefix = CciClientId(CciEnum.calculationRules);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.orderRules);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.quoteRules);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.indicator);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.yield);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

            if (null != cciClassification)
                cciClassification.AddCciSystem();

            if (null != cciLocalization)
                cciLocalization.AddCciSystem();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, security);
            //
            base.Initialize_FromCci();
            //
            InitializeClassification_FromCci();
            //
            InitializeLocalization_FromCci();

        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        public override void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    SQL_Table sql_Table = null;
                    string data = string.Empty;
                    bool isSetting = true;
                    bool isToValidate = false;
                    #endregion Reset variables
                    //
                    switch (cciEnum)
                    {
                        case CciEnum.couponType:
                            if (security.CouponTypeSpecified)
                                data = security.CouponType.Value;
                            break;
                        case CciEnum.priceRateType:
                            if (security.PriceRateTypeSpecified)
                                data = security.PriceRateType.ToString();
                            break;
                        case CciEnum.numberOfIssuedSecurities:
                            if (security.NumberOfIssuedSecuritiesSpecified)
                                data = StrFunc.FmtAmountToInvariantCulture(security.NumberOfIssuedSecurities.DecValue.ToString());
                            break;
                        case CciEnum.faceAmount_amount:
                            if (security.FaceAmountSpecified)
                                data = security.FaceAmount.Amount.Value;
                            break;
                        case CciEnum.faceAmount_currency:
                            if (security.FaceAmountSpecified)
                                data = security.FaceAmount.Currency;
                            break;
                        case CciEnum.price_issuePricePercentage:
                            if (security.PriceSpecified)
                            {
                                if (security.Price.IssuePricePercentageSpecified)
                                    data = security.Price.IssuePricePercentage.Value;
                            }
                            break;
                        case CciEnum.price_redemptionPricePercentage:
                            if (security.PriceSpecified)
                            {
                                if (security.Price.RedemptionPricePercentageSpecified)
                                    data = security.Price.RedemptionPricePercentage.Value;
                            }
                            break;
                        case CciEnum.yieldType:
                            if (security.YieldSpecified && security.Yield.YieldTypeSpecified)
                                data = security.Yield.YieldType.ToString();
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
            //
            base.Initialize_FromDocument();
            //
            if (null != cciClassification)
                cciClassification.Initialize_FromDocument();
            //
            if (null != cciLocalization)
                cciLocalization.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);
            //
            if (null != cciClassification)
                cciClassification.ProcessInitialize(pCci);
            //
            if (null != cciLocalization)
                cciLocalization.ProcessInitialize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            base.ProcessExecute(pCci);
            //
            if (null != cciClassification)
                cciClassification.ProcessExecute(pCci);
            //
            if (null != cciLocalization)
                cciLocalization.ProcessExecute(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public new void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
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
                    string partyReferenceHref = string.Empty;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    
                    switch (cciEnum)
                    {
                        case CciEnum.couponType:
                            #region couponType
                            security.CouponTypeSpecified = StrFunc.IsFilled(data);
                            if (security.CouponTypeSpecified)
                                security.CouponType.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion
                            break;
                        case CciEnum.priceRateType:
                            #region priceRateType
                            security.PriceRateTypeSpecified = StrFunc.IsFilled(data);
                            if (security.PriceRateTypeSpecified)
                                security.PriceRateType = (PriceRateType3CodeEnum)Enum.Parse(typeof(PriceRateType3CodeEnum), data, true);
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion
                            break;
                        case CciEnum.numberOfIssuedSecurities:
                            #region numberOfIssuedSecurities
                            security.NumberOfIssuedSecuritiesSpecified = StrFunc.IsFilled(data);
                            if (security.NumberOfIssuedSecuritiesSpecified)
                                security.NumberOfIssuedSecurities.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion numberOfIssuedSecurities
                            break;
                        case CciEnum.faceAmount_amount:
                            #region faceAmount
                            security.FaceAmountSpecified = StrFunc.IsFilled(data);
                            if (security.FaceAmountSpecified)
                                security.FaceAmount.Amount.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion faceAmount
                            break;
                        case CciEnum.faceAmount_currency:
                            #region faceAmount_currency
                            security.FaceAmount.Currency = data;
                            #endregion faceAmount_currency
                            break;
                        case CciEnum.price_issuePricePercentage:
                            #region price_issuePricePercentage
                            security.Price.IssuePricePercentageSpecified = StrFunc.IsFilled(data);
                            security.Price.IssuePricePercentage.Value = data;
                            #endregion
                            break;
                        case CciEnum.price_redemptionPricePercentage:
                            #region price_redemptionPricePercentage
                            security.Price.RedemptionPricePercentageSpecified = StrFunc.IsFilled(data);
                            security.Price.RedemptionPricePercentage.Value = data;
                            #endregion
                            break;
                        case CciEnum.yieldType:
                            #region yieldType
                            security.YieldSpecified = StrFunc.IsFilled(data);
                            security.Yield.YieldTypeSpecified = StrFunc.IsFilled(data);
                            if (security.YieldSpecified && security.Yield.YieldTypeSpecified)
                                security.Yield.YieldType = (YieldTypeEnum)Enum.Parse(typeof(YieldTypeEnum), data, true);
                            #endregion yieldType
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
            //
            security.PriceSpecified =
                security.Price.IssuePricePercentageSpecified || security.Price.RedemptionPricePercentageSpecified || security.Price.RedemptionPriceFormulaSpecified;
            //
            base.Dump_ToDocument();
            //
            if (null != cciClassification)
            {
                cciClassification.Dump_ToDocument();
                security.ClassificationSpecified = cciClassification.IsSpecified;
            }
            //
            if (null != cciLocalization)
            {
                cciLocalization.Dump_ToDocument();
                security.LocalizationSpecified = cciLocalization.IsSpecified;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            //
            if (null != cciClassification)
                cciClassification.CleanUp();
            //
            if (null != cciLocalization)
                cciLocalization.CleanUp();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public override void RemoveLastItemInArray(string pPrefix)
        {
            base.RemoveLastItemInArray(pPrefix);
            //
            if (null != cciClassification)
                cciClassification.RemoveLastItemInArray(pPrefix);
            //
            if (null != cciLocalization)
                cciLocalization.RemoveLastItemInArray(pPrefix);

        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            //
            if (null != cciClassification)
                cciClassification.RefreshCciEnabled();
            //
            if (null != cciLocalization)
                cciLocalization.RefreshCciEnabled();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            //
            if (null != cciClassification)
                cciClassification.SetDisplay(pCci);
            //
            if (null != cciLocalization)
                cciLocalization.SetDisplay(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public new void SetEnabled(bool pIsEnabled)
        {
            base.SetEnabled(pIsEnabled);
            //
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            //
            if (null != cciClassification)
                cciClassification.SetEnabled(pIsEnabled);
            //
            if (null != cciLocalization)
                cciLocalization.SetEnabled(pIsEnabled);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            //
            if (null != cciClassification)
                cciClassification.Initialize_Document();
            //
            if (null != cciLocalization)
                cciLocalization.Initialize_Document();

        }
        #endregion

        

        #region IContainerCciGetInfoButton Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //            
            #region button calculationRules
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.calculationRules, pCci);
                if (isOk)
                {
                    pCo.Object = "security";
                    pCo.Element = "calculationRules";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = security.CalculationRulesSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button calculationRules
            //            
            #region button orderRules
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.orderRules, pCci);
                if (isOk)
                {
                    pCo.Object = "security";
                    pCo.Element = "orderRules";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = security.OrderRulesSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button orderRules
            //
            #region button quoteRules
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.quoteRules, pCci);
                if (isOk)
                {
                    pCo.Object = "security";
                    pCo.Element = "quoteRules";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = security.QuoteRulesSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button quoteRules
            //
            #region button indicator
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.indicator, pCci);
                if (isOk)
                {
                    pCo.Object = "security";
                    pCo.Element = "indicator";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = security.IndicatorSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button indicator
            //
            #region button yield
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.yield, pCci);
                if (isOk)
                {
                    pCo.Object = "security";
                    pCo.Element = "yield";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = security.YieldSpecified;
                    pIsEnabled = true;
                }
            }
            #endregion button yield
            //
            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

        }
        #endregion

        #region Method
        /// <summary>
        /// 
        /// </summary>
        private void InitializeLocalization_FromCci()
        {
            //FI 20121009 [18182] set cciLocalization = null
            cciLocalization = null;
            CciLocalization cciLocalizationCurrent = new CciLocalization(cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_localization, null);
            //
            bool isOk = CcisBase.Contains(cciLocalizationCurrent.CciClientId(CciLocalization.CciEnum.countryOfIssue));
            if (isOk)
            {
                if (null == security.Localization)
                    security.Localization = security.CreateLocalization();
                //
                cciLocalization = new CciLocalization(cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_localization, security.Localization);
                cciLocalization.Initialize_FromCci();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeClassification_FromCci()
        {
            //FI 20121009 [18182] set cciClassification = null
            cciClassification = null;
            CciClassification cciClassificationCurrent = new CciClassification(cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_classification, null);
            //
            bool isOk = CcisBase.Contains(cciClassificationCurrent.CciClientId(CciClassification.CciEnum.cfiCode));
            if (isOk)
            {
                if (null == security.Classification)
                    security.Classification = security.CreateClassification();
                //
                cciClassification = new CciClassification(cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_classification, security.Classification);
                cciClassification.Initialize_FromCci();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pIsEmpty"></param>
        /// <returns></returns>
        public bool RemoveLastItemInInstrumendId(string pPrefix, bool pIsEmpty)
        {

            bool isOk = true;
            //
            if (pPrefix == TradeCustomCaptureInfos.CCst.Prefix_instrumentId)
            {
                int posArray = InstrumentIdLenght - 1;
                bool isToRemove = true;
                if (pIsEmpty)
                    isToRemove = StrFunc.IsEmpty(security.InstrumentId[posArray].Scheme);
                //
                if (isToRemove)
                {
                    CcisBase.RemoveCciOf(cciSecurityIdSourceScheme[posArray]);
                    ReflectionTools.RemoveItemInArray(this, "cciSecurityIdSourceScheme", posArray);
                    ReflectionTools.RemoveItemInArray(this.security, "instrumentId", posArray);
                }
                else
                    isOk = false;
            }
            //
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            //
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
            //
            if (null != cciClassification)
                cciClassification.Clear();
            //
            if (null != cciLocalization)
                cciLocalization.Clear();

        }

        #endregion
    }
}

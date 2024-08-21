#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.DynamicData;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciReturnLeg.
    /// </summary>
    // EG 20231127 [WI754] Implementation Return Swap : Refactoring Code Analysis
    public class CciReturnLeg : ContainerCciBase, IContainerCciPayerReceiver, IContainerCciFactory, IContainerCciSpecified
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            //effectiveAdjustableDate
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            effectiveDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            effectiveDate_adjustableDate_dateAdjustments_bDC,
            //effectiveRelativeDate
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            effectiveDate_relativeDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateRelativeDate.periodMultiplier")]
            effectiveDate_relativeDate_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateRelativeDate.period")]
            effectiveDate_relativeDate_period,
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateRelativeDate.dayType")]
            effectiveDate_relativeDate_dayType,
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateRelativeDate.businessDayConvention")]
            effectiveDate_relativeDate_bDC,
            //TerminationAdjustableDate
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            terminationDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            terminationDate_adjustableDate_dateAdjustments_bDC,
            //TerminationRelativeDate
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            terminationDate_relativeDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateRelativeDate.periodMultiplier")]
            terminationDate_relativeDate_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateRelativeDate.period")]
            terminationDate_relativeDate_period,
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateRelativeDate.dayType")]
            terminationDate_relativeDate_dayType,
            [System.Xml.Serialization.XmlEnumAttribute("terminationDate.adjustableOrRelativeDateRelativeDate.businessDayConvention")]
            terminationDate_relativeDate_bDC,

            //returnSwapAmount
            [System.Xml.Serialization.XmlEnumAttribute("amount.legAmountReferenceAmount")]
            amount_referenceAmount,
            [System.Xml.Serialization.XmlEnumAttribute("amount.cashSettlement")]
            amount_cashSettlement,
            //returnSwapNotional
            [System.Xml.Serialization.XmlEnumAttribute("notional.returnSwapNotionalNotionalAmount.amount")]
            notional_notionalAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notional.returnSwapNotionalNotionalAmount.currency")]
            notional_notionalAmount_currency,

            [System.Xml.Serialization.XmlEnumAttribute("rateOfReturn.notionalReset")]
            notionalReset,

            // Bouton rateOfReturn.PaymentDates.PaymentDatesInterim
            [System.Xml.Serialization.XmlEnumAttribute("rateOfReturn.paymentDates.paymentDatesInterim")]
            priceInterim_paymentDates,
            // Bouton rateOfReturn.PaymentDates.PaymentDatesFinal
            [System.Xml.Serialization.XmlEnumAttribute("rateOfReturn.paymentDates.paymentDatesFinal")]
            priceFinal_paymentDates,

            [System.Xml.Serialization.XmlEnumAttribute("rateOfReturn.valuationPriceInterimSpecified")]
            priceInterim_specified,

            unknown,
        }
        public enum CciEnumReturn
        {
            //return
            [System.Xml.Serialization.XmlEnumAttribute("@return.returnType")]
            returnType,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions")]
            dividendConditions,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditionsSpecified")]
            dividendConditions_specified,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendReinvestment")]
            dividendConditions_reinvestment,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendEntitlement")]
            dividendConditions_entitlement,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendAmount")]
            dividendConditions_amount,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendPaymentDate.paymentDateDividendDateReference")]
            dividendConditions_paymentDate_dateReference,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendPeriod")]
            dividendConditions_period,
            unknown,
        }
        /// EG 20150302 Add underlyer_notionalBase_amount|underlyer_notionalBase_currency (CFD Forex)
        public enum CciEnumUnderlyer
        {
            //underlyer
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.underlyingAsset")]
            underlyer_underlyingAsset,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.openUnits")]
            underlyer_openUnits,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.notionalBase.amount")]
            underlyer_notionalBase_amount,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.notionalBase.currency")]
            underlyer_notionalBase_currency,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.dividendPayout.dividendPayoutRatio")]
            underlyer_dividendPayout_ratio,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.dividendPayout.dividendPayoutConditions")]
            underlyer_dividendPayout_Conditions,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.underlyingAsset.currency")]
            underlyer_currency,
            unknown,
        }
        public enum CciEnumFxFeature
        {
            // fxFeature
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature.referenceCurrency")]
            fxFeature_referenceCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature.fxFeatureQuanto")]
            fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis,
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature.fxFeatureQuanto")]
            fxFeature_quanto_fxRate_rate,
            unknown,
        }
        public enum CciEnumMarginRatio
        {
            // Margin Ratio
            [System.Xml.Serialization.XmlEnumAttribute("marginRatioSpecified")]
            marginRatio_specified,
            [System.Xml.Serialization.XmlEnumAttribute("marginRatio.currency")]
            marginRatio_currency,
            [System.Xml.Serialization.XmlEnumAttribute("marginRatio.amount")]
            marginRatio_amount,
            [System.Xml.Serialization.XmlEnumAttribute("marginRatio.priceExpression")]
            marginRatio_priceExpression,
            [System.Xml.Serialization.XmlEnumAttribute("marginRatio.spreadSchedule.initialValue")]
            marginRatio_spreadSchedule_initialValue,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private IReturnLeg _returnLeg;
        
        private readonly IReturnSwap _returnSwap;

        private readonly CciTrade _cciTrade;
        private CciReturnLegValuationPrice _cciInitialPrice;
        private CciReturnLegValuationPrice _cciValuationPriceInterim;
        private CciReturnLegValuationPrice _cciValuationPriceFinal;


        #endregion Members
        #region Accessors
        #region IdA_Custodian
        private Nullable<int> IdA_Custodian
        {
            get
            {
                return ((CciProductReturnSwap)_cciTrade.cciProduct).ReturnSwapContainer.IdA_Custodian;
            }
        }
        #endregion IdA_Custodian
        /// <summary>
        /// 
        /// </summary>
        public CciReturnLegValuationPrice CciInitialPrice
        {
            get { return _cciInitialPrice; }
            set { _cciInitialPrice = value; }
        }


        /// <summary>
        ///  Obtient true si marginRatio.priceExpression = PriceExpressionEnum.PercentageOfNotional
        /// </summary>
        public bool IsMarginRatioPercentageOfNotional
        {
            get
            {
                return (Cci(CciEnumMarginRatio.marginRatio_priceExpression).NewValue == PriceExpressionEnum.PercentageOfNotional.ToString());

            }
        }


        /// <summary>
        ///  Obtient true si marginRatio.priceExpression = PriceExpressionEnum.AbsoluteTerms
        /// </summary>
        public bool IsMarginRatioAbsoluteTerms
        {
            get
            {
                return (Cci(CciEnumMarginRatio.marginRatio_priceExpression).NewValue == PriceExpressionEnum.AbsoluteTerms.ToString());
            }
        }

        #region ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        #endregion ccis
        #region GetCurrency1
        public string GetCurrency1
        {
            get { return this.GetFxFeatureReferenceCurrency; }
        }
        #endregion GetCurrency1
        #region GetCurrency2
        public string GetCurrency2
        {
            get { return this.GetUnderlyingAssetCurrency; }
        }
        #endregion GetCurrency2
        #region GetFxFeatureReferenceCurrency
        public string GetFxFeatureReferenceCurrency
        {
            get
            {
                string ret = null;
                if (_returnLeg.FxFeatureSpecified)
                    ret = _returnLeg.FxFeature.ReferenceCurrency.Value;
                return ret;
            }
        }
        #endregion GetFxFeatureReferenceCurrency
        #region GetUnderlyingAssetCurrency
        // EG 20140526
        public string GetUnderlyingAssetCurrency
        {
            get
            {
                string ret = null;
                if (_returnLeg.Underlyer.UnderlyerSingleSpecified)
                {
                    if (_returnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset.CurrencySpecified)
                        ret = _returnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset.Currency.Value;
                }
                else if (_returnLeg.Underlyer.UnderlyerBasketSpecified)
                {
                    if (ArrFunc.IsFilled(_returnLeg.Underlyer.UnderlyerBasket.BasketConstituent))
                        ret = _returnLeg.Underlyer.UnderlyerBasket.BasketConstituent[0].UnderlyingAsset.Currency.Value;
                }
                return ret;
            }
        }
        #endregion GetUnderlyingAssetCurrency
        #region GetUnderlyingAsset
        /// EG 20150302 New
        public IUnderlyingAsset UnderlyingAsset
        {
            get
            {
                IUnderlyingAsset underlyingAsset = null;
                if (_returnLeg.Underlyer.UnderlyerSingleSpecified)
                {
                    underlyingAsset = _returnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset;
                }
                else if (_returnLeg.Underlyer.UnderlyerBasketSpecified)
                {
                    if (ArrFunc.IsFilled(_returnLeg.Underlyer.UnderlyerBasket.BasketConstituent))
                        underlyingAsset = _returnLeg.Underlyer.UnderlyerBasket.BasketConstituent[0].UnderlyingAsset;
                }
                return underlyingAsset;
            }
        }
        #endregion GetUnderlyingAsset
        #region CciValuationPriceInterim
        public CciReturnLegValuationPrice CciValuationPriceInterim
        {
            get
            {
                return _cciValuationPriceInterim;
            }
        }
        #endregion CciValuationPriceInterim
        #region ReturnLeg
        public IReturnLeg ReturnLeg
        {
            get { return _returnLeg; }
            set { _returnLeg = value; }
        }
        #endregion ReturnLeg
        #endregion Accessors
        #region Constructors
        // EG 20231024 [XXXXX] RTS / Corrections diverses : Passage paramètre pLegNumber
        public CciReturnLeg(CciTrade pCciTrade, int pLegNumber, IReturnSwap pReturnSwap, IReturnLeg pReturnLeg, string pPrefix)
            : base(pPrefix, pLegNumber, pCciTrade.Ccis)
        {
            _cciTrade = pCciTrade;
            _returnLeg = pReturnLeg;
            _returnSwap = pReturnSwap;
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciSpecified Members
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion IContainerCciSpecified Members
        

        #region Membres de ITradeGetInfoButton
        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (IsCci(CciEnumUnderlyer.underlyer_underlyingAsset, pCci))
            {
                bool isOk = true;
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;

                if (_returnLeg.Underlyer.UnderlyerSingleSpecified)
                {
                    switch (_returnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset.UnderlyerAssetCategory)
                    {
                        case Cst.UnderlyingAsset.EquityAsset:
                            pCo.Referential = "ASSET_EQUITY";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_EQUITY";
                            break;
                        case Cst.UnderlyingAsset.FxRateAsset:
                            pCo.Referential = "ASSET_FXRATE";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_FXRATE";
                            break;
                        case Cst.UnderlyingAsset.Index:
                            pCo.Referential = "ASSET_INDEX";
                            pCo.Title = "OTC_REF_DATA_UNDERASSET_INDEX";
                            break;
                        default:
                            isOk = false;
                            break;
                    }

                    if (isOk)
                    {
                        pCo.DynamicArgument = null;
                        pCo.ClientId = pCci.ClientId_WithoutPrefix;
                        pCo.SqlColumn = "IDENTIFIER";
                        pCo.Condition = "TRADE_INPUT";
                        pCo.Fk = null;

                        StringDynamicData market = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "MARKET", string.Empty);
                        StringDynamicData IdI = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "IDI", _cciTrade.Product.IdI.ToString());
                        if (StrFunc.IsFilled(market.value))
                        {
                            pCo.Condition = "TRADE_INPUT";
                            pCo.DynamicArgument = new string[2] { market.Serialize(), IdI.Serialize() };
                        }
                        else
                        {
                            pCo.Condition = "TRADE_INPUT2";
                            pCo.DynamicArgument = new string[1] { IdI.Serialize() };
                        }
                    }

                }

            }
        }
        #endregion SetButtonReferential
        #region public override SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.priceInterim_paymentDates, pCci);
                if (isOk)
                {
                    pCo.Element = "paymentDatesInterim";
                    pCo.Object = "paymentDates";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = _returnLeg.RateOfReturn.PaymentDates.PaymentDatesInterimSpecified;
                    pIsEnabled = _returnLeg.RateOfReturn.ValuationPriceInterimSpecified;
                }
                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.priceFinal_paymentDates, pCci);
                    if (isOk)
                    {

                        pCo.Element = "paymentDateFinal";
                        pCo.Object = "paymentDates";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = true;
                        pIsEnabled = true;
                    }
                }
                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnumReturn.dividendConditions, pCci);
                    if (isOk)
                    {

                        pCo.Element = "dividendConditions";
                        pCo.Object = "return";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _returnLeg.Return.DividendConditionsSpecified;
                        pIsEnabled = _returnLeg.Return.DividendConditionsSpecified;
                    }
                }
                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnumMarginRatio.marginRatio_spreadSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        pCo.Object = "marginRatio";
                        pCo.Element = "spreadSchedule";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _returnLeg.RateOfReturn.MarginRatio.SpreadScheduleSpecified && _returnLeg.RateOfReturn.MarginRatio.SpreadSchedule.StepSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            return isOk;
        }
        #endregion
        #endregion Membres de ITradeGetInfoButton

        #region IContainerCciPayerReceiver Members
        #region  CciClientIdPayer/receiver
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer); }
        }
        #endregion  CciClientIdPayer
        #region  CciClientIdReceiver
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver); }
        }
        #endregion CciClientIdReceiver
        #region SynchronizePayerReceiver
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion
        #endregion SynchronizePayerReceiver
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.payer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.receiver), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnumUnderlyer.underlyer_currency), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.terminationDate_adjustableDate_unadjustedDate), false, TypeData.TypeDataEnum.@date);
            // Si pas alimenter il sont renseigner par initial Price
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.notional_notionalAmount_amount), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.notional_notionalAmount_currency), false, TypeData.TypeDataEnum.@string);

            if (null != _cciInitialPrice)
                _cciInitialPrice.AddCciSystem();
            if (null != _cciValuationPriceInterim)
                _cciValuationPriceInterim.AddCciSystem();
            if (null != _cciValuationPriceFinal)
                _cciValuationPriceFinal.AddCciSystem();

            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.priceInterim_paymentDates), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.priceFinal_paymentDates), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnumReturn.dividendConditions), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnumReturn.dividendConditions_paymentDate_dateReference), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnumReturn.dividendConditions_period), false, TypeData.TypeDataEnum.@string);

            // FI 20140811 [XXXXX] add puisque utiliser par les properties IsMarginRatioPercentageOfNotional et IsMarginRatioAbsoluteTerms
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnumMarginRatio.marginRatio_priceExpression), false, TypeData.TypeDataEnum.@string);
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
            //Nothing pour l'instant	
        }
        #endregion CleanUp
        #region Dump_ToDocument
        /// EG 20150302 Use UnderlyingAsset accessor
        public void Dump_ToDocument()
        {
            string cliendId_Key;
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if ((cci.HasChanged) && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    cliendId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                    {
                        CciEnum _currEnum = (CciEnum)Enum.Parse(typeof(CciEnum), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumUnderlyer), cliendId_Key))
                    {
                        CciEnumUnderlyer _currEnum = (CciEnumUnderlyer)Enum.Parse(typeof(CciEnumUnderlyer), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumReturn), cliendId_Key))
                    {
                        CciEnumReturn _currEnum = (CciEnumReturn)Enum.Parse(typeof(CciEnumReturn), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumFxFeature), cliendId_Key))
                    {
                        CciEnumFxFeature _currEnum = (CciEnumFxFeature)Enum.Parse(typeof(CciEnumFxFeature), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumMarginRatio), cliendId_Key))
                    {
                        CciEnumMarginRatio _currEnum = (CciEnumMarginRatio)Enum.Parse(typeof(CciEnumMarginRatio), cliendId_Key);
                        Dump_ToDocument(cci, _currEnum);
                    }
                }
            }
            if (null != _cciInitialPrice)
                _cciInitialPrice.Dump_ToDocument();

            if (null != _cciValuationPriceInterim)
                _cciValuationPriceInterim.Dump_ToDocument();

            if (null != _cciValuationPriceFinal)
                _cciValuationPriceFinal.Dump_ToDocument();



        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnum pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
            switch (pEnum)
            {

                case CciEnum.payer:
                    #region payer
                    ((IReturnSwapLeg)_returnLeg).PayerPartyReference.HRef = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//synchronize Payer/receiver
                    #endregion payer
                    break;
                case CciEnum.receiver:
                    #region receiver
                    ((IReturnSwapLeg)_returnLeg).ReceiverPartyReference.HRef = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//synchronize Payer/receiver
                    #endregion receiver
                    break;
                case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                    #region EffectiveDate (AdjustableDate)
                    _returnLeg.EffectiveDate.AdjustableDateSpecified = pCci.IsFilledValue;
                    _returnLeg.EffectiveDate.RelativeDateSpecified = (!pCci.IsFilledValue);
                    _returnLeg.EffectiveDate.AdjustableDate.UnadjustedDate.Value = data;
                    #endregion EffectiveDate (AdjustableDate)
                    break;
                case CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC:
                    #region EffectiveDate BDC (AdjustableDate)
                    if (_returnLeg.EffectiveDate.AdjustableDateSpecified)
                    {
                        BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                        _returnLeg.EffectiveDate.AdjustableDate.DateAdjustments.BusinessDayConvention = bDCEnum;
                    }
                    DumpBDC(pEnum);
                    #endregion EffectiveDate BDC (AdjustableDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_dateRelativeTo:
                    #region EffectiveDate RelativeTo( RelativeDate)
                    _returnLeg.EffectiveDate.RelativeDateSpecified = pCci.IsFilledValue;
                    _returnLeg.EffectiveDate.AdjustableDateSpecified = (!pCci.IsFilledValue);
                    _returnLeg.EffectiveDate.RelativeDate.DateRelativeToValue = data;
                    #endregion EffectiveDate RelativeTo( RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_period:
                    #region EffectiveDate Period ( RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                    {
                        PeriodEnum periodEnum = StringToEnum.Period(data);
                        _returnLeg.EffectiveDate.RelativeDate.Period = periodEnum;
                    }
                    #endregion EffectiveDate Period ( RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_periodMultiplier:
                    #region EffectiveDate Multiplier( RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                        _returnLeg.EffectiveDate.RelativeDate.PeriodMultiplier.Value = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; // if period <> D => DayType = empty
                    #endregion EffectiveDate Multiplier( RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_bDC:
                    #region EffectiveDate BDC( RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                    {
                        if (pCci.IsFilledValue)
                        {
                            BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                            _returnLeg.EffectiveDate.RelativeDate.BusinessDayConvention = bDCEnum;
                        }
                        DumpBDC(pEnum);
                    }
                    #endregion EffectiveDate BDC( RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_dayType:
                    #region EffectiveDate DayType( RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                    {
                        _returnLeg.EffectiveDate.RelativeDate.DayTypeSpecified = pCci.IsFilledValue;
                        if (pCci.IsFilledValue)
                        {
                            DayTypeEnum dayTypeEnum = StringToEnum.DayType(data);
                            _returnLeg.EffectiveDate.RelativeDate.DayType = dayTypeEnum;
                        }
                    }
                    #endregion EffectiveDate DayType( RelativeDate)
                    break;
                case CciEnum.terminationDate_adjustableDate_unadjustedDate:
                    #region TerminationDate (AdjustableDate)
                    _returnLeg.TerminationDate.AdjustableDateSpecified = pCci.IsFilledValue;
                    _returnLeg.TerminationDate.RelativeDateSpecified = (!pCci.IsFilledValue);
                    _returnLeg.TerminationDate.AdjustableDate.UnadjustedDate.Value = data;
                    #endregion TerminationDate (AdjustableDate)
                    break;
                case CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC:
                    #region TerminationDate BDC (AdjustableDate)
                    if (_returnLeg.TerminationDate.AdjustableDateSpecified)
                    {
                        BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                        _returnLeg.TerminationDate.AdjustableDate.DateAdjustments.BusinessDayConvention = bDCEnum;
                        DumpBDC(pEnum);
                    }
                    #endregion TerminationDate BDC (AdjustableDate)
                    break;
                case CciEnum.terminationDate_relativeDate_dateRelativeTo:
                    #region TerminationDate RelativeTo (RelativeDate)
                    _returnLeg.TerminationDate.RelativeDateSpecified = pCci.IsFilledValue;
                    _returnLeg.TerminationDate.AdjustableDateSpecified = (!pCci.IsFilledValue);
                    _returnLeg.TerminationDate.RelativeDate.DateRelativeToValue = data;
                    #endregion TerminationDate RelativeTo (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_period:
                    #region TerminationDate Period (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                    {
                        PeriodEnum period = StringToEnum.Period(data);
                        _returnLeg.TerminationDate.RelativeDate.Period = period;
                    }
                    #endregion TerminationDate Period (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_periodMultiplier:
                    #region TerminationDate Multiplier (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                        _returnLeg.TerminationDate.RelativeDate.PeriodMultiplier.Value = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; // if period <> D => DayType = empty
                    #endregion TerminationDate Multiplier (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_bDC:
                    #region TerminationDate BDC (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                    {
                        if (pCci.IsFilledValue)
                        {
                            BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                            _returnLeg.TerminationDate.RelativeDate.BusinessDayConvention = bDCEnum;
                            DumpBDC(pEnum);
                        }
                    }
                    #endregion TerminationDate BDC (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_dayType:
                    #region TerminationDate DayType (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                    {
                        if (pCci.IsFilledValue)
                        {
                            DayTypeEnum dayTypeEnum = StringToEnum.DayType(data);
                            _returnLeg.TerminationDate.RelativeDate.DayType = dayTypeEnum;
                        }
                    }
                    #endregion TerminationDate DayType (RelativeDate)
                    break;

                case CciEnum.notional_notionalAmount_amount:
                    #region notionalAmount_amount
                    _returnLeg.Notional.NotionalAmount.Amount.Value = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    #endregion notionalAmount_amount
                    break;
                case CciEnum.notional_notionalAmount_currency:
                    #region notionalAmount_currency
                    _returnLeg.Notional.NotionalAmount.Currency = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    #endregion notionalAmount_currency
                    break;
                case CciEnum.amount_referenceAmount:
                    #region returnSwapAmount
                    _returnLeg.ReturnSwapAmount.ReferenceAmountSpecified = pCci.IsFilledValue;
                    if (_returnLeg.ReturnSwapAmount.ReferenceAmountSpecified)
                        _returnLeg.ReturnSwapAmount.ReferenceAmount.Value = data;
                    #endregion returnSwapAmount
                    break;
                case CciEnum.amount_cashSettlement:
                    #region returnSwapAmount CashSettlement
                    _returnLeg.ReturnSwapAmount.CashSettlement.Value = data;
                    #endregion returnSwapAmount CashSettlement
                    break;
                case CciEnum.notionalReset:
                    #region returnSwapAmount CashSettlement
                    _returnLeg.RateOfReturn.NotionalReset.Value = data;
                    #endregion returnSwapAmount CashSettlement
                    break;
                case CciEnum.priceInterim_specified:
                    #region returnLeg.rateOfReturn.valuationPriceInterimSpecified
                    _returnLeg.RateOfReturn.ValuationPriceInterimSpecified = pCci.IsFilledValue;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                    #endregion returnLeg.rateOfReturn.valuationPriceInterimSpecified
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                CcisBase.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pEnum"></param>
        /// FI 20140811 [XXXXX] Modify
        /// EG 20150302 underlyer_notionalBase setting (CFD Forex)
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumUnderlyer pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
            switch (pEnum)
            {
                case CciEnumUnderlyer.underlyer_underlyingAsset:
                    #region singleUnderlyer

                    if (false == _returnLeg.Underlyer.UnderlyerSingleSpecified &&
                        false == _returnLeg.Underlyer.UnderlyerBasketSpecified)
                        _returnLeg.Underlyer.UnderlyerSingleSpecified = true;

                    IUnderlyingAsset underlyingAsset = UnderlyingAsset;
                    SQL_AssetBase sql_Asset = null;
                    bool isLoaded = false;
                    pCci.ErrorMsg = string.Empty;
                    if (StrFunc.IsFilled(data))
                    {
                        //Tip: Utile pour trouver "TIM.MI" à partir de "IM MI" via "%IM%MI%"
                        for (int i = 0; i < 2; i++)
                        {
                            string dataToFind = data;
                            if (i == 1)
                                dataToFind = data.Replace(" ", "%") + "%";

                            switch (underlyingAsset.UnderlyerAssetCategory)
                            {
                                case Cst.UnderlyingAsset.EquityAsset:
                                    sql_Asset = new SQL_AssetEquity(_cciTrade.CSCacheOn, SQL_AssetBase.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    ((SQL_AssetEquity)sql_Asset).IdI_In = _cciTrade.Product.IdI; ;
                                    break;
                                case Cst.UnderlyingAsset.FxRateAsset:
                                    sql_Asset = new SQL_AssetFxRate(_cciTrade.CSCacheOn, SQL_AssetBase.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    break;

                                case Cst.UnderlyingAsset.Index:
                                    sql_Asset = new SQL_AssetIndex(_cciTrade.CSCacheOn, SQL_AssetBase.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    //FI 20140811 [XXXXX]  Mise en commentaire TODO voir plus tard l'usage ce cette donnée     
                                    //((SQL_AssetIndex)sql_Asset).idI_In = _cciTrade.Product.idI; 
                                    break;
                                case Cst.UnderlyingAsset.Bond:
                                case Cst.UnderlyingAsset.ConvertibleBond:
                                    sql_Asset = new SQL_AssetDebtSecurity(_cciTrade.CSCacheOn, SQL_AssetBase.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    //FI 20140811 [XXXXX]  Mise en commentaire voir plus tard l'usage ce cette donnée
                                    //((SQL_AssetDebtSecurity)sql_Asset).idI_In = _cciTrade.Product.idI; 
                                    break;
                            }
                            isLoaded = sql_Asset.IsLoaded && (sql_Asset.RowsCount == 1);
                            if (isLoaded)
                                break;
                        }
                        //
                        if (isLoaded)
                        {
                            pCci.NewValue = sql_Asset.Identifier;
                            pCci.Sql_Table = sql_Asset;

                            underlyingAsset.OTCmlId = sql_Asset.IdAsset;
                            underlyingAsset.InstrumentId = underlyingAsset.CreateInstrumentId(sql_Asset.Identifier);
                            underlyingAsset.ClearanceSystemSpecified = StrFunc.IsFilled(sql_Asset.ClearanceSystem);
                            if (underlyingAsset.ClearanceSystemSpecified)
                                underlyingAsset.ClearanceSystem.Value = sql_Asset.ClearanceSystem;
                            //description
                            underlyingAsset.DefinitionSpecified = false;
                            underlyingAsset.DescriptionSpecified = StrFunc.IsFilled(sql_Asset.Description);
                            if (underlyingAsset.DescriptionSpecified)
                                underlyingAsset.Description = new EFS_String(sql_Asset.Description);
                            //Currency
                            underlyingAsset.CurrencySpecified = StrFunc.IsFilled(sql_Asset.IdC);
                            if (underlyingAsset.CurrencySpecified)
                            {
                                underlyingAsset.Currency = underlyingAsset.CreateCurrency(sql_Asset.IdC);
                                if ((null != _cciInitialPrice) && _cciInitialPrice.IsPriceExpressionAbsoluteTerms)
                                    CcisBase.SetNewValue(_cciInitialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), sql_Asset.IdC);

                            }
                            //ExchangeId
                            underlyingAsset.ExchangeIdSpecified = StrFunc.IsFilled(sql_Asset.Market_FIXML_SecurityExchange);
                            if (underlyingAsset.ExchangeIdSpecified)
                            {
                                underlyingAsset.ExchangeId = underlyingAsset.CreateExchangeId(sql_Asset.Market_FIXML_SecurityExchange);
                                underlyingAsset.ExchangeId.OTCmlId = sql_Asset.IdM;
                            }

                            // FxRate
                            if (Cst.UnderlyingAsset.FxRateAsset == underlyingAsset.UnderlyerAssetCategory)
                            {
                                if (underlyingAsset is IFxRateAsset fxRateAsset)
                                {
                                    SQL_AssetFxRate sql_AssetFxRate = (SQL_AssetFxRate)sql_Asset;
                                    fxRateAsset.CreateQuotedCurrencyPair(sql_AssetFxRate.QCP_Cur1, sql_AssetFxRate.QCP_Cur2, sql_AssetFxRate.QCP_QuoteBasisEnum);
                                }
                            }
                        }
                        pCci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                    }

                    if (false == isLoaded)
                    {
                        pCci.Sql_Table = null;

                        underlyingAsset.OTCmlId = 0;
                        underlyingAsset.CreateInstrumentId(string.Empty);
                        underlyingAsset.ClearanceSystemSpecified = false;
                        underlyingAsset.DefinitionSpecified = false;
                        underlyingAsset.DescriptionSpecified = false;
                        underlyingAsset.CurrencySpecified = false;
                        underlyingAsset.ExchangeIdSpecified = false;
                    }
                    //
                    DumpBDC(CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC);
                    DumpBDC(CciEnum.effectiveDate_relativeDate_bDC);
                    DumpBDC(CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC);
                    DumpBDC(CciEnum.terminationDate_relativeDate_bDC);

                    if (null != _cciValuationPriceInterim)
                        _cciValuationPriceInterim.Dump_PeriodicDatesBDA();
                    if (null != _cciValuationPriceFinal)
                        _cciValuationPriceFinal.Dump_PeriodicDatesBDA();

                    if (false == CcisBase.IsModeIO)
                        Ccis.SetFundingAndMargin(_cciTrade.CSCacheOn);


                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                    #endregion singleUnderlyer
                    break;
                case CciEnumUnderlyer.underlyer_openUnits:
                    #region singleUnderlyer OpenUnits
                    _returnLeg.Underlyer.UnderlyerSingle.OpenUnitsSpecified = pCci.IsFilledValue;
                    _returnLeg.Underlyer.UnderlyerSingle.OpenUnits.Value = data;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    #endregion singleUnderlyer OpenUnits
                    break;
                case CciEnumUnderlyer.underlyer_notionalBase_amount:
                    _returnLeg.Underlyer.UnderlyerSingle.NotionalBaseSpecified = pCci.IsFilledValue;
                    _returnLeg.Underlyer.UnderlyerSingle.NotionalBase.Amount = new EFS_Decimal(data);
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    break;
                case CciEnumUnderlyer.underlyer_notionalBase_currency:
                    _returnLeg.Underlyer.UnderlyerSingle.NotionalBaseSpecified = pCci.IsFilledValue;
                    _returnLeg.Underlyer.UnderlyerSingle.NotionalBase.Currency = data;
                    break;
                case CciEnumUnderlyer.underlyer_dividendPayout_ratio:
                    #region singleUnderlyer DividendPayout
                    _returnLeg.Underlyer.UnderlyerSingle.DividendPayout.DividendPayoutRatioSpecified = pCci.IsFilledValue;

                    _returnLeg.Underlyer.UnderlyerSingle.DividendPayoutSpecified =
                        _returnLeg.Underlyer.UnderlyerSingle.DividendPayout.DividendPayoutRatioSpecified
                        ||
                        _returnLeg.Underlyer.UnderlyerSingle.DividendPayout.DividendPayoutConditionsSpecified;

                    _returnLeg.Underlyer.UnderlyerSingle.DividendPayout.DividendPayoutRatio.Value = data;
                    #endregion singleUnderlyer DividendPayout
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                CcisBase.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumReturn pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
            switch (pEnum)
            {
                case CciEnumReturn.returnType:
                    #region return_returnType
                    ReturnTypeEnum returnEnum = (ReturnTypeEnum)System.Enum.Parse(typeof(ReturnTypeEnum), data, true);
                    _returnLeg.Return.ReturnType = returnEnum;
                    #endregion return_returnType
                    break;
                case CciEnumReturn.dividendConditions_specified:
                    #region return_dividendConditions_specified
                    _returnLeg.Return.DividendConditionsSpecified = pCci.IsFilledValue;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                    #endregion return_dividendConditions_dividendEntitlement
                    break;
                case CciEnumReturn.dividendConditions_entitlement:
                    #region return_dividendConditions_dividendEntitlement
                    _returnLeg.Return.DividendConditions.DividendEntitlementSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                    {
                        DividendEntitlementEnum divEntitlementEnum = (DividendEntitlementEnum)System.Enum.Parse(typeof(DividendEntitlementEnum), data, true);
                        _returnLeg.Return.DividendConditions.DividendEntitlement = divEntitlementEnum;
                        _returnLeg.Return.DividendConditionsSpecified = true;
                    }
                    #endregion return_dividendConditions_dividendEntitlement
                    break;
                case CciEnumReturn.dividendConditions_period:
                    #region return_dividendConditions_dividendPeriod
                    _returnLeg.Return.DividendConditions.DividendPeriodSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                    {
                        DividendPeriodEnum divPeriodEnum = (DividendPeriodEnum)System.Enum.Parse(typeof(DividendPeriodEnum), data, true);
                        _returnLeg.Return.DividendConditions.DividendPeriod = divPeriodEnum;
                        _returnLeg.Return.DividendConditionsSpecified = true;
                    }
                    #endregion return_dividendConditions_dividendPeriod
                    break;
                case CciEnumReturn.dividendConditions_amount:
                    #region return_dividendConditions_dividendAmount
                    _returnLeg.Return.DividendConditions.DividendAmountSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                    {
                        DividendAmountTypeEnum divAmountEnum = (DividendAmountTypeEnum)System.Enum.Parse(typeof(DividendAmountTypeEnum), data, true);
                        _returnLeg.Return.DividendConditions.DividendAmount = divAmountEnum;
                        _returnLeg.Return.DividendConditionsSpecified = true;
                    }
                    #endregion return_dividendConditions_dividendAmount
                    break;
                case CciEnumReturn.dividendConditions_paymentDate_dateReference:
                    #region return_dividendConditions_dividendPaymentDate_dividendDateReference
                    _returnLeg.Return.DividendConditions.DividendPaymentDate.DividendDateReferenceSpecified = pCci.IsFilledValue;
                    if (pCci.IsFilledValue)
                    {
                        DividendDateReferenceEnum divDateReferenceEnum = (DividendDateReferenceEnum)System.Enum.Parse(typeof(DividendDateReferenceEnum), data, true);
                        _returnLeg.Return.DividendConditions.DividendPaymentDate.DividendDateReference = divDateReferenceEnum;
                        _returnLeg.Return.DividendConditions.DividendPaymentDateSpecified = true;
                        _returnLeg.Return.DividendConditionsSpecified = true;
                    }
                    #endregion return_dividendConditions_dividendPaymentDate_dividendDateReference
                    break;
                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                CcisBase.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumFxFeature pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
            switch (pEnum)
            {
                case CciEnumFxFeature.fxFeature_referenceCurrency:
                    #region fxFeature_referenceCurrency
                    _returnLeg.FxFeatureSpecified = pCci.IsFilledValue;
                    if (_returnLeg.FxFeatureSpecified)
                        _returnLeg.FxFeature.ReferenceCurrency.Value = data;
                    #endregion fxFeature_referenceCurrency
                    break;
                case CciEnumFxFeature.fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis:
                    #region fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                    _returnLeg.FxFeature.FxFeatureQuantoSpecified = pCci.IsFilledValue;
                    if (_returnLeg.FxFeature.FxFeatureQuantoSpecified && StrFunc.IsFilled(data))
                    {
                        QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                        _returnLeg.FxFeature.FxFeatureQuanto.FxRate[0].QuotedCurrencyPair.QuoteBasis = QbEnum;
                    }
                    #endregion fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                    break;
                case CciEnumFxFeature.fxFeature_quanto_fxRate_rate:
                    #region fxFeature_quanto_fxRate_rate
                    _returnLeg.FxFeature.FxFeatureQuanto.FxRate[0].Rate.Value = data;
                    #endregion fxFeature_quanto_fxRate_rate
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                CcisBase.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }
        private void Dump_ToDocument(CustomCaptureInfo pCci, CciEnumMarginRatio pEnum)
        {
            bool isSetting = true;
            string data = pCci.NewValue;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
            switch (pEnum)
            {
                case CciEnumMarginRatio.marginRatio_specified:
                    #region marginRatio_specified
                    _returnLeg.RateOfReturn.MarginRatioSpecified = pCci.IsFilledValue;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                    #endregion marginRatio_specified
                    break;
                case CciEnumMarginRatio.marginRatio_currency:
                    #region marginRatio_currency
                    _returnLeg.RateOfReturn.MarginRatio.CurrencySpecified = pCci.IsFilledValue;
                    if (_returnLeg.RateOfReturn.MarginRatio.CurrencySpecified)
                        _returnLeg.RateOfReturn.MarginRatio.Currency = ((IProductBase)_returnSwap).CreateCurrency(data);
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    #endregion marginRatio_currency
                    break;
                case CciEnumMarginRatio.marginRatio_amount:
                    #region marginRatio_amount
                    _returnLeg.RateOfReturn.MarginRatioSpecified = pCci.IsFilledValue;
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified)
                    {
                        _returnLeg.RateOfReturn.MarginRatio.Amount = new EFS_Decimal(data);
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    }
                    #endregion marginRatio_amount
                    break;
                case CciEnumMarginRatio.marginRatio_spreadSchedule_initialValue:
                    #region marginRatio_spread_initialValue
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified)
                    {
                        _returnLeg.RateOfReturn.MarginRatio.SpreadScheduleSpecified = pCci.IsFilledValue;
                        if (_returnLeg.RateOfReturn.MarginRatio.SpreadScheduleSpecified)
                        {
                            _returnLeg.RateOfReturn.MarginRatio.CreateSpreadMarginRatio(new EFS_Decimal(data).DecValue);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                        }
                    }
                    #endregion marginRatio_spread_initialValue
                    break;
                case CciEnumMarginRatio.marginRatio_priceExpression:
                    #region marginRatio_amount
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified)
                    {
                        PriceExpressionEnum priceExpression = (PriceExpressionEnum)System.Enum.Parse(typeof(PriceExpressionEnum), data, true);
                        _returnLeg.RateOfReturn.MarginRatio.PriceExpression = priceExpression;
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                    }
                    #endregion marginRatio_amount
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion default
            }
            if (isSetting)
                CcisBase.Finalize(pCci.ClientId_WithoutPrefix, processQueue);
        }

        #endregion Dump_ToDocument

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20140805 [XXXXX] Modify
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            System.Web.UI.Control control;

            control = pPage.FindControl(CciClientId(CciEnumReturn.dividendConditions) + "_tblDividends");
            if (null != control)
                control.Visible = _returnLeg.Return.DividendConditionsSpecified;

            if (null != _cciValuationPriceInterim)
            {
                control = pPage.FindControl(_cciValuationPriceInterim.CciClientId("tblPrice"));
                if (null != control)
                    control.Visible = _returnLeg.RateOfReturn.ValuationPriceInterimSpecified;
            }

            control = pPage.FindControl(Prefix + "marginRatio_tblMarginRatio");
            if (null != control)
                control.Visible = _returnLeg.RateOfReturn.MarginRatioSpecified;

            // FI 20140805 [XXXXX] add OpenFormReferential for asset Equity
            //pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_EQUITY);
            // EG 20140826 Autant faire les chose à fond et ne pas passer ASSET_EQUITY en dur !!!

            CustomCaptureInfo cci = Cci(CciEnumUnderlyer.underlyer_underlyingAsset);
            if (null != cci)
            {
                if (_returnLeg.Underlyer.UnderlyerSingleSpecified)
                {
                    switch (_returnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset.UnderlyerAssetCategory)
                    {
                        case Cst.UnderlyingAsset.EquityAsset:
                            pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_EQUITY);
                            break;
                        case Cst.UnderlyingAsset.FxRateAsset:
                            pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_FXRATE);
                            break;
                        case Cst.UnderlyingAsset.Index:
                            pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_INDEX);
                            break;
                    }
                }
            }

        }

        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        // EG 20140526
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _returnLeg);

            if (_returnLeg.Underlyer.UnderlyerSingleSpecified)
            {
                if (null == _returnLeg.Underlyer.UnderlyerSingle.OpenUnits)
                    _returnLeg.Underlyer.UnderlyerSingle.OpenUnits = new EFS_Decimal();
            }
            else if (_returnLeg.Underlyer.UnderlyerBasketSpecified)
            {
                if (null == _returnLeg.Underlyer.UnderlyerBasket.OpenUnits)
                    _returnLeg.Underlyer.UnderlyerBasket.OpenUnits = new EFS_Decimal();
            }

            if (null == _returnLeg.RateOfReturn)
                _returnLeg.RateOfReturn = _returnLeg.CreateRateOfReturn;

            _returnLeg.RateOfReturn.NotionalResetSpecified = true;
            if (null == _returnLeg.RateOfReturn.NotionalReset)
                _returnLeg.RateOfReturn.NotionalReset = new EFS_Boolean();

            // Initial Price
            CciReturnLegValuationPrice _initialPrice = new CciReturnLegValuationPrice(_cciTrade, this, null, Prefix + "initialPrice");
            if (CcisBase.Contains(_initialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_amount)))
            {
                if (null == _returnLeg.RateOfReturn.InitialPrice)
                    _returnLeg.RateOfReturn.InitialPrice = _returnLeg.RateOfReturn.CreateReturnLegValuationPrice;
                _initialPrice.ReturnLegValuationPrice = _returnLeg.RateOfReturn.InitialPrice;
                _cciInitialPrice = _initialPrice;
            }
            // Interim Price
            CciReturnLegValuationPrice _priceInterim = new CciReturnLegValuationPrice(_cciTrade, this, null, Prefix + "priceInterim");
            if (null == _returnLeg.RateOfReturn.ValuationPriceInterim)
                _returnLeg.RateOfReturn.ValuationPriceInterim = _returnLeg.RateOfReturn.CreateReturnLegValuationPrice;
            _priceInterim.ReturnLegValuationPrice = _returnLeg.RateOfReturn.ValuationPriceInterim;
            _cciValuationPriceInterim = _priceInterim;

            // Final Price
            CciReturnLegValuationPrice _priceFinal = new CciReturnLegValuationPrice(_cciTrade, this, null, Prefix + "priceFinal");
            if (CcisBase.Contains(_priceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_amount)) ||
                CcisBase.Contains(_priceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.determinationMethod)))
            {
                if (null == _returnLeg.RateOfReturn.ValuationPriceFinal)
                    _returnLeg.RateOfReturn.ValuationPriceFinal = _returnLeg.RateOfReturn.CreateReturnLegValuationPrice;
                _priceFinal.ReturnLegValuationPrice = _returnLeg.RateOfReturn.ValuationPriceFinal;
                _cciValuationPriceFinal = _priceFinal;
            }
            //
            if (CcisBase.Contains(CciClientId(CciEnumFxFeature.fxFeature_referenceCurrency)))
            {
                if (false == _returnLeg.FxFeatureSpecified)
                    _returnLeg.FxFeature = _returnLeg.CreateFxFeature;
                if (false == _returnLeg.FxFeature.FxFeatureQuantoSpecified)
                    _returnLeg.FxFeature.FxFeatureQuanto.FxRate = _returnLeg.FxFeature.FxFeatureQuanto.CreateFxRate;
            }
            if (null != _cciInitialPrice)
                _cciInitialPrice.Initialize_FromCci();
            if (null != _cciValuationPriceInterim)
                _cciValuationPriceInterim.Initialize_FromCci();
            if (null != _cciValuationPriceFinal)
                _cciValuationPriceFinal.Initialize_FromCci();

            if (null == _returnLeg.ReturnSwapAmount)
                _returnLeg.ReturnSwapAmount = _returnLeg.CreateReturnSwapAmount;

            if (null == _returnLeg.Return)
                _returnLeg.Return = _returnLeg.CreateReturn;
            if (null == _returnLeg.Return.DividendConditions)
                _returnLeg.Return.DividendConditions = _returnLeg.Return.CreateDividendConditions;

            if (null == ((IReturnSwapLeg)_returnLeg).ReceiverPartyReference)
                ((IReturnSwapLeg)_returnLeg).ReceiverPartyReference = ((IReturnSwapLeg)_returnLeg).CreateReference;
            if (null == ((IReturnSwapLeg)_returnLeg).PayerPartyReference)
                ((IReturnSwapLeg)_returnLeg).PayerPartyReference = ((IReturnSwapLeg)_returnLeg).CreateReference;

            if (null == _returnLeg.RateOfReturn.MarginRatio)
                _returnLeg.RateOfReturn.MarginRatio = _returnLeg.CreateMarginRatio;
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string cliendId_Key;
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (this.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    cliendId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                    {
                        CciEnum _currEnum = (CciEnum)Enum.Parse(typeof(CciEnum), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumUnderlyer), cliendId_Key))
                    {
                        CciEnumUnderlyer _currEnum = (CciEnumUnderlyer)Enum.Parse(typeof(CciEnumUnderlyer), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumReturn), cliendId_Key))
                    {
                        CciEnumReturn _currEnum = (CciEnumReturn)Enum.Parse(typeof(CciEnumReturn), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumFxFeature), cliendId_Key))
                    {
                        CciEnumFxFeature _currEnum = (CciEnumFxFeature)Enum.Parse(typeof(CciEnumFxFeature), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                    else if (System.Enum.IsDefined(typeof(CciEnumMarginRatio), cliendId_Key))
                    {
                        CciEnumMarginRatio _currEnum = (CciEnumMarginRatio)Enum.Parse(typeof(CciEnumMarginRatio), cliendId_Key);
                        Initialize_FromDocument(cci, _currEnum);
                    }
                }
            }
            if (null != _cciInitialPrice)
                _cciInitialPrice.Initialize_FromDocument();
            if (null != _cciValuationPriceInterim)
                _cciValuationPriceInterim.Initialize_FromDocument();
            if (null != _cciValuationPriceFinal)
                _cciValuationPriceFinal.Initialize_FromDocument();
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnum pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                case CciEnum.payer:
                    #region Payer
                    data = ((IReturnSwapLeg)_returnLeg).PayerPartyReference.HRef;
                    #endregion Payer
                    break;
                case CciEnum.receiver:
                    #region Receiver
                    data = ((IReturnSwapLeg)_returnLeg).ReceiverPartyReference.HRef;
                    #endregion Receiver
                    break;
                case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                    #region EffectiveDate (AdjustableDate)
                    if (_returnLeg.EffectiveDate.AdjustableDateSpecified)
                        data = _returnLeg.EffectiveDate.AdjustableDate.UnadjustedDate.Value;
                    #endregion EffectiveDate (AdjustableDate)
                    break;
                case CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC:
                    #region EffectiveDate BDC (AdjustableDate)
                    if (_returnLeg.EffectiveDate.AdjustableDateSpecified)
                        data = _returnLeg.EffectiveDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                    #endregion EffectiveDate BDC (AdjustableDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_dateRelativeTo:
                    #region EffectiveDate RelativeTo (RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                        data = _returnLeg.EffectiveDate.RelativeDate.DateRelativeToValue;
                    #endregion EffectiveDate RelativeTo (RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_period:
                    #region EffectiveDate Period (RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                        data = _returnLeg.EffectiveDate.RelativeDate.Period.ToString();
                    #endregion EffectiveDate Period (RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_periodMultiplier:
                    #region EffectiveDate Multiplier (RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                        data = _returnLeg.EffectiveDate.RelativeDate.PeriodMultiplier.Value;
                    #endregion EffectiveDate Multiplier (RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_bDC:
                    #region EffectiveDate BDC (RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                        data = _returnLeg.EffectiveDate.RelativeDate.BusinessDayConvention.ToString();
                    #endregion EffectiveDate BDC (RelativeDate)
                    break;
                case CciEnum.effectiveDate_relativeDate_dayType:
                    #region EffectiveDate DayType (RelativeDate)
                    if (_returnLeg.EffectiveDate.RelativeDateSpecified && _returnLeg.EffectiveDate.RelativeDate.DayTypeSpecified)
                        data = _returnLeg.EffectiveDate.RelativeDate.DayType.ToString();
                    #endregion EffectiveDate DayType (RelativeDate)
                    break;
                case CciEnum.terminationDate_adjustableDate_unadjustedDate:
                    #region TerminationDate (AdjustableDate)
                    if (_returnLeg.TerminationDate.AdjustableDateSpecified)
                        data = _returnLeg.TerminationDate.AdjustableDate.UnadjustedDate.Value;
                    #endregion TerminationDate (AdjustableDate)
                    break;
                case CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC:
                    #region TerminationDate BDC (AdjustableDate)
                    if (_returnLeg.TerminationDate.AdjustableDateSpecified)
                        data = _returnLeg.TerminationDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                    #endregion TerminationDate BDC (AdjustableDate)
                    break;
                case CciEnum.terminationDate_relativeDate_dateRelativeTo:
                    #region TerminationDate RelativeTo (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                        data = _returnLeg.TerminationDate.RelativeDate.DateRelativeToValue;
                    #endregion TerminationDate RelativeTo (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_period:
                    #region TerminationDate Period (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                        data = _returnLeg.TerminationDate.RelativeDate.Period.ToString();
                    #endregion TerminationDate Period (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_periodMultiplier:
                    #region TerminationDate Multiplier (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                        data = _returnLeg.TerminationDate.RelativeDate.PeriodMultiplier.Value;
                    #endregion TerminationDate Multiplier (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_bDC:
                    #region TerminationDate BDC (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified)
                        data = _returnLeg.TerminationDate.RelativeDate.BusinessDayConvention.ToString();
                    #endregion TerminationDate BDC (RelativeDate)
                    break;
                case CciEnum.terminationDate_relativeDate_dayType:
                    #region TerminationDate DayType (RelativeDate)
                    if (_returnLeg.TerminationDate.RelativeDateSpecified && _returnLeg.TerminationDate.RelativeDate.DayTypeSpecified)
                        data = _returnLeg.TerminationDate.RelativeDate.DayType.ToString();
                    #endregion TerminationDate DayType (RelativeDate)
                    break;
                case CciEnum.notional_notionalAmount_amount:
                    #region notionalAmount_amount
                    data = _returnLeg.Notional.NotionalAmount.Amount.Value;
                    #endregion notionalAmount_amount
                    break;
                case CciEnum.notional_notionalAmount_currency:
                    #region notionalAmount_currency
                    data = _returnLeg.Notional.NotionalAmount.Currency;
                    #endregion notionalAmount_currency
                    break;
                case CciEnum.amount_referenceAmount:
                    #region returnSwapAmount
                    if (_returnLeg.ReturnSwapAmount.ReferenceAmountSpecified)
                        data = _returnLeg.ReturnSwapAmount.ReferenceAmount.Value;
                    #endregion returnSwapAmount
                    break;
                case CciEnum.amount_cashSettlement:
                    #region returnSwapAmount CashSettlement
                    data = _returnLeg.ReturnSwapAmount.CashSettlement.Value.ToUpper();
                    #endregion returnSwapAmount CashSettlement
                    break;
                case CciEnum.notionalReset:
                    #region returnLeg.rateOfReturn.notionalReset
                    if (_returnLeg.RateOfReturn.NotionalResetSpecified)
                        data = _returnLeg.RateOfReturn.NotionalReset.Value.ToUpper();
                    #endregion returnSwapAmount CashSettlement
                    break;

                case CciEnum.priceInterim_specified:
                    #region returnLeg.rateOfReturn.valuationPriceInterimSpecified
                    data = _returnLeg.RateOfReturn.ValuationPriceInterimSpecified.ToString().ToLower();
                    #endregion returnLeg.rateOfReturn.valuationPriceInterimSpecified
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                CcisBase.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        /// EG 20150302 underlyer_notionalBase setting (CFD Forex)
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumUnderlyer pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                case CciEnumUnderlyer.underlyer_underlyingAsset:
                    #region singleUnderlyer
                    try
                    {
                        IUnderlyingAsset underlyingAsset = null;
                        if (false == _returnLeg.Underlyer.UnderlyerSingleSpecified &&
                            false == _returnLeg.Underlyer.UnderlyerBasketSpecified)
                            _returnLeg.Underlyer.UnderlyerSingleSpecified = true;

                        if (_returnLeg.Underlyer.UnderlyerSingleSpecified)
                            underlyingAsset = _returnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset;
                        else if (_returnLeg.Underlyer.UnderlyerBasketSpecified)
                            underlyingAsset = _returnLeg.Underlyer.UnderlyerBasket.BasketConstituent[0].UnderlyingAsset;

                        int idAsset = underlyingAsset.OTCmlId;
                        if (idAsset > 0)
                        {
                            SQL_AssetBase sql_Asset = null;
                            switch (underlyingAsset.UnderlyerAssetCategory)
                            {
                                case Cst.UnderlyingAsset.EquityAsset:
                                    sql_Asset = new SQL_AssetEquity(_cciTrade.CSCacheOn, idAsset);
                                    break;
                                case Cst.UnderlyingAsset.Index:
                                    sql_Asset = new SQL_AssetIndex(_cciTrade.CSCacheOn, idAsset);
                                    break;
                                case Cst.UnderlyingAsset.FxRateAsset:
                                    sql_Asset = new SQL_AssetFxRate(_cciTrade.CSCacheOn, idAsset);
                                    break;
                                case Cst.UnderlyingAsset.Bond:
                                case Cst.UnderlyingAsset.ConvertibleBond:
                                    sql_Asset = new SQL_AssetDebtSecurity(_cciTrade.CSCacheOn, idAsset);
                                    break;
                            }
                            if (sql_Asset.IsLoaded && (sql_Asset.RowsCount == 1))
                            {
                                sql_Table = sql_Asset;
                                data = sql_Asset.Identifier;
                            }
                        }
                    }
                    catch
                    {
                        pCci.Sql_Table = null;
                        data = string.Empty;
                    }
                    #endregion singleUnderlyer
                    break;
                case CciEnumUnderlyer.underlyer_currency:
                    #region singleUnderlyer currency
                    data = this.GetUnderlyingAssetCurrency;
                    #endregion singleUnderlyer currency
                    break;

                case CciEnumUnderlyer.underlyer_openUnits:
                    #region singleUnderlyer openUnits
                    if (_returnLeg.Underlyer.UnderlyerSingle.OpenUnitsSpecified)
                        data = _returnLeg.Underlyer.UnderlyerSingle.OpenUnits.Value;
                    #endregion singleUnderlyer openUnits
                    break;

                case CciEnumUnderlyer.underlyer_notionalBase_amount:
                    #region singleUnderlyer notionalBase
                    if (_returnLeg.Underlyer.UnderlyerSingle.NotionalBaseSpecified)
                        data = _returnLeg.Underlyer.UnderlyerSingle.NotionalBase.Amount.Value;
                    #endregion singleUnderlyer notionalBase
                    break;

                case CciEnumUnderlyer.underlyer_notionalBase_currency:
                    #region singleUnderlyer notionalBase currency
                    if (_returnLeg.Underlyer.UnderlyerSingle.NotionalBaseSpecified)
                        data = _returnLeg.Underlyer.UnderlyerSingle.NotionalBase.Currency;
                    #endregion singleUnderlyer notionalBase currency
                    break;

                case CciEnumUnderlyer.underlyer_dividendPayout_ratio:
                    #region singleUnderlyer DividentPayout
                    if (_returnLeg.Underlyer.UnderlyerSingle.DividendPayoutSpecified)
                        if (_returnLeg.Underlyer.UnderlyerSingle.DividendPayout.DividendPayoutRatioSpecified)
                            data = _returnLeg.Underlyer.UnderlyerSingle.DividendPayout.DividendPayoutRatio.Value;
                    #endregion singleUnderlyer DividentPayout
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                CcisBase.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumReturn pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                case CciEnumReturn.returnType:
                    #region return_returnType
                    data = _returnLeg.Return.ReturnType.ToString();
                    #endregion return_returnType
                    break;
                case CciEnumReturn.dividendConditions_specified:
                    #region return_dividendConditionsSpecified
                    data = _returnLeg.Return.DividendConditionsSpecified.ToString().ToLower();
                    #endregion return_dividendConditionsSpecified
                    break;
                case CciEnumReturn.dividendConditions_entitlement:
                    #region return_dividendConditions_dividendEntitlement
                    if (_returnLeg.Return.DividendConditionsSpecified)
                        if (_returnLeg.Return.DividendConditions.DividendEntitlementSpecified)
                            data = _returnLeg.Return.DividendConditions.DividendEntitlement.ToString();
                    #endregion return_dividendConditions_dividendEntitlement
                    break;
                case CciEnumReturn.dividendConditions_period:
                    #region return_dividendConditions_dividendPeriod
                    if (_returnLeg.Return.DividendConditionsSpecified)
                        if (_returnLeg.Return.DividendConditions.DividendPeriodSpecified)
                            data = _returnLeg.Return.DividendConditions.DividendPeriod.ToString();
                    #endregion return_dividendConditions_dividendPeriod
                    break;
                case CciEnumReturn.dividendConditions_amount:
                    #region return_dividendConditions_dividendAmount
                    if ((_returnLeg.Return.DividendConditionsSpecified) &&
                        (_returnLeg.Return.DividendConditions.DividendAmountSpecified))
                        data = _returnLeg.Return.DividendConditions.DividendAmount.ToString();
                    #endregion return_dividendConditions_dividendAmount
                    break;
                case CciEnumReturn.dividendConditions_paymentDate_dateReference:
                    #region return_dividendConditions_dividendPaymentDate_dividendDateReference
                    if ((_returnLeg.Return.DividendConditionsSpecified) &&
                         (_returnLeg.Return.DividendConditions.DividendPaymentDateSpecified))
                        if (_returnLeg.Return.DividendConditions.DividendPaymentDate.DividendDateReferenceSpecified)
                            data = _returnLeg.Return.DividendConditions.DividendPaymentDate.DividendDateReference.ToString();
                    #endregion return_dividendConditions_dividendPaymentDate_dividendDateReference
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                CcisBase.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumFxFeature pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                case CciEnumFxFeature.fxFeature_referenceCurrency:
                    #region fxFeature_referenceCurrency
                    if (_returnLeg.FxFeatureSpecified)
                        data = _returnLeg.FxFeature.ReferenceCurrency.Value;
                    #endregion fxFeature_referenceCurrency
                    break;
                case CciEnumFxFeature.fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis:
                    #region fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                    if (_returnLeg.FxFeatureSpecified && _returnLeg.FxFeature.FxFeatureQuantoSpecified)
                        data = _returnLeg.FxFeature.FxFeatureQuanto.FxRate[0].QuotedCurrencyPair.QuoteBasis.ToString();
                    #endregion fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                    break;
                case CciEnumFxFeature.fxFeature_quanto_fxRate_rate:
                    #region fxFeature_quanto_fxRate_rate
                    if (_returnLeg.FxFeatureSpecified && _returnLeg.FxFeature.FxFeatureQuantoSpecified)
                        data = _returnLeg.FxFeature.FxFeatureQuanto.FxRate[0].Rate.Value;
                    #endregion fxFeature_quanto_fxRate_rate
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                CcisBase.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        private void Initialize_FromDocument(CustomCaptureInfo pCci, CciEnumMarginRatio pEnum)
        {
            string data = string.Empty;
            bool isToValidate = false;
            bool isSetting = true;
            SQL_Table sql_Table = null;

            switch (pEnum)
            {
                case CciEnumMarginRatio.marginRatio_specified:
                    #region marginRatio_specified
                    data = _returnLeg.RateOfReturn.MarginRatioSpecified.ToString().ToLower();
                    #endregion marginRatio_specified
                    break;
                case CciEnumMarginRatio.marginRatio_currency:
                    #region marginRatio_currency
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified &&
                        _returnLeg.RateOfReturn.MarginRatio.CurrencySpecified)
                        data = _returnLeg.RateOfReturn.MarginRatio.Currency.Value;
                    #endregion marginRatio_currency
                    break;
                case CciEnumMarginRatio.marginRatio_amount:
                    #region marginRatio_amount
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified)
                        data = _returnLeg.RateOfReturn.MarginRatio.Amount.Value;
                    #endregion marginRatio_amount
                    break;
                case CciEnumMarginRatio.marginRatio_spreadSchedule_initialValue:
                    #region marginRatio_amount
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified &&
                        _returnLeg.RateOfReturn.MarginRatio.SpreadScheduleSpecified)
                        data = _returnLeg.RateOfReturn.MarginRatio.SpreadSchedule.InitialValue.Value;
                    #endregion marginRatio_amount
                    break;
                case CciEnumMarginRatio.marginRatio_priceExpression:
                    #region marginRatio_priceExpression
                    // EG 20231127 [WI754] Implementation Return Swap : Inclusion of SetMarginRatio inside the clause "if"
                    if (_returnLeg.RateOfReturn.MarginRatioSpecified)
                    {
                        data = _returnLeg.RateOfReturn.MarginRatio.PriceExpression.ToString();
                        SetMarginRatioRegEx();
                    }
                    #endregion marginRatio_priceExpression
                    break;

                #region default
                default:
                    isSetting = false;
                    break;
                #endregion
            }
            if (isSetting)
            {
                CcisBase.InitializeCci(pCci, sql_Table, data);
                if (isToValidate)
                    pCci.LastValue = ".";
            }
        }
        #endregion Initialize_FromDocument

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
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
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                    switch (key)
                    {
                        #region Buyer/Seller: Calcul des BCs
                        case CciEnum.payer:
                            CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                            break;
                        case CciEnum.receiver:
                            CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                            break;
                        #endregion
                        #region effectiveDate
                        case CciEnum.effectiveDate_relativeDate_periodMultiplier:
                            Ccis.ProcessInitialize_DayType(CciClientId(CciEnum.effectiveDate_relativeDate_dayType), _returnLeg.EffectiveDate.RelativeDate.GetOffset);
                            break;
                        #endregion effectiveDate
                        #region terminationDate
                        case CciEnum.terminationDate_relativeDate_periodMultiplier:
                            Ccis.ProcessInitialize_DayType(CciClientId(CciEnum.terminationDate_relativeDate_dayType), _returnLeg.TerminationDate.RelativeDate.GetOffset);
                            break;
                        #endregion terminationDate
                        #region Currency: Arrondi du notional et Calcul des BCs
                        case CciEnum.notional_notionalAmount_amount:
                        case CciEnum.notional_notionalAmount_currency:
                            Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.notional_notionalAmount_amount), _returnLeg.Notional.NotionalAmount, true);
                            break;
                        #endregion

                        default:
                            break;
                    }
                }
                else if (System.Enum.IsDefined(typeof(CciEnumUnderlyer), clientId_Key))
                {
                    CciEnumUnderlyer key = (CciEnumUnderlyer)System.Enum.Parse(typeof(CciEnumUnderlyer), clientId_Key);

                    switch (key)
                    {
                        #region underlyer_singleUnderlyer_underlyingAsset
                        case CciEnumUnderlyer.underlyer_underlyingAsset:
                            string curr = string.Empty;
                            //
                            if (null != Cci(CciEnumUnderlyer.underlyer_underlyingAsset).Sql_Table)
                            {
                                //Plutôt que lire sql_Table pour déterminer la devise => lecture du doc fpml
                                curr = this.GetUnderlyingAssetCurrency;
                                Cci(CciReturnLeg.CciEnumUnderlyer.underlyer_currency).NewValue = curr;

                                InitializeDateFromEntityMarket();

                                //PL 20140723 A finaliser... 
                                //FI 20140811 [XXXXX] En mode importation Spheres® calcule les le funding et le margin avant l'enregistrement du trade
                                //FI 20140925 [XXXXX] IsSetFundingAndMargin n'est plus utilisée
                                //this.IsSetFundingAndMargin =  (false == ccis.IsModeIO);

                            }

                            if (StrFunc.IsFilled(curr))
                            {

                                CcisBase.SetNewValue(CciClientId(CciEnum.notional_notionalAmount_currency), curr);

                                // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                                if ((null != _cciInitialPrice) && (_cciInitialPrice.IsCommissionFixedAmount))
                                    CcisBase.SetNewValue(_cciInitialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.commission_currency), curr, /*true*/ false);
                                //Mise a jour Devise price
                                if ((null != _cciInitialPrice) && (_cciInitialPrice.IsPriceExpressionAbsoluteTerms))
                                    CcisBase.SetNewValue(_cciInitialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), curr, /*true*/ false);
                                //Mise a jour de cciValuationPriceInterim (commission)
                                if ((null != _cciValuationPriceInterim) && (_cciValuationPriceInterim.IsCommissionFixedAmount))
                                    CcisBase.SetNewValue(_cciValuationPriceInterim.CciClientId(CciReturnLegValuationPrice.CciEnum.commission_currency), curr, /*true*/ false);
                                //Mise a jour Devise price
                                if ((null != _cciValuationPriceInterim) && (_cciValuationPriceInterim.IsPriceExpressionAbsoluteTerms))
                                    CcisBase.SetNewValue(_cciValuationPriceInterim.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), curr, /*true*/ false);
                                //Mise a cciValuationPriceFinal (commission)
                                if ((null != _cciValuationPriceFinal) && (_cciValuationPriceFinal.IsCommissionFixedAmount))
                                    CcisBase.SetNewValue(_cciValuationPriceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.commission_currency), curr, /*true*/ false);
                                //Mise a jour Devise price
                                if ((null != _cciValuationPriceFinal) && (_cciValuationPriceFinal.IsPriceExpressionAbsoluteTerms))
                                    CcisBase.SetNewValue(_cciValuationPriceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), curr, /*true*/ false);
                                // Mise à jour Devise Margin Ratio
                                if (IsMarginRatioAbsoluteTerms)
                                    CcisBase.SetNewValue(CciClientId(CciEnumMarginRatio.marginRatio_currency), curr, /*true*/ false);
                            }
                            if (null != _cciValuationPriceInterim)
                                _cciValuationPriceInterim.Dump_PeriodicDatesBDA();
                            break;
                        case CciEnumUnderlyer.underlyer_openUnits:
                            _cciInitialPrice.SetNotionalAmount();
                            break;
                        #endregion

                        default:
                            break;
                    }
                }
                else if (System.Enum.IsDefined(typeof(CciEnumMarginRatio), clientId_Key))
                {
                    CciEnumMarginRatio key = (CciEnumMarginRatio)System.Enum.Parse(typeof(CciEnumMarginRatio), clientId_Key);

                    switch (key)
                    {
                        case CciEnumMarginRatio.marginRatio_priceExpression:
                            if (_returnLeg.RateOfReturn.MarginRatioSpecified)
                            {
                                // Amount / Ratio
                                SetMarginRatioRegEx();

                                // Currency
                                if (this.IsMarginRatioAbsoluteTerms)
                                    CcisBase.SetNewValue(CciClientId(CciEnumMarginRatio.marginRatio_currency),
                                        CcisBase[_cciTrade.CciClientIdMainCurrency].NewValue);
                                else
                                    CcisBase.SetNewValue(CciClientId(CciEnumMarginRatio.marginRatio_currency), string.Empty);
                            }
                            else
                            {
                                CcisBase.SetNewValue(CciClientId(CciEnumMarginRatio.marginRatio_amount), string.Empty);
                                CcisBase.SetNewValue(CciClientId(CciEnumMarginRatio.marginRatio_currency), string.Empty);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            if (null != _cciInitialPrice)
                _cciInitialPrice.ProcessInitialize(pCci);
            if (null != _cciValuationPriceInterim)
                _cciValuationPriceInterim.ProcessInitialize(pCci);
            if (null != _cciValuationPriceFinal)
                _cciValuationPriceFinal.ProcessInitialize(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
        }

        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnumUnderlyer.underlyer_underlyingAsset, pCci))
            {
                pCci.Display = string.Empty;
                if (pCci.Sql_Table is SQL_AssetBase asset)
                    pCci.Display = asset.SetDisplay;
            }

            if (null != _cciInitialPrice)
                _cciInitialPrice.SetDisplay(pCci);
            if (null != _cciValuationPriceInterim)
                _cciValuationPriceInterim.SetDisplay(pCci);
            if (null != _cciValuationPriceFinal)
                _cciValuationPriceFinal.SetDisplay(pCci);

            if (IsCci(CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (_returnLeg.EffectiveDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLeg.EffectiveDate.AdjustableDate.DateAdjustments);
            }

            if (IsCci(CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (_returnLeg.EffectiveDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLeg.TerminationDate.AdjustableDate.DateAdjustments);
            }

            if (IsCci(CciEnum.effectiveDate_relativeDate_bDC, pCci))
            {
                if (_returnLeg.EffectiveDate.RelativeDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLeg.EffectiveDate.RelativeDate.GetAdjustments);
            }

        }
        #endregion SetDisplay

        #endregion IContainerCciFactory Members
        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return IsCci(CciEnumFxFeature.fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis, pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciBdc"></param>
        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        private void DumpBDC(CciEnum pCciBdc)
        {
            string clientIdAsset = CciClientId(CciEnumUnderlyer.underlyer_underlyingAsset);    // A reprendre si basket (non gere pur l'instant)
            _ = CciClientId(pCciBdc);
            CciBC cciBC = new CciBC(_cciTrade);
            string id = string.Empty;

            //cciBC.Add(clientIdEquity, CciBC.TypeReferentialInfo.Equity);
            cciBC.Add(clientIdAsset, CciBC.TypeReferentialInfo.Asset);

            IBusinessDayAdjustments bda = null;
            switch (pCciBdc)
            {
                case CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC:
                    id = TradeCustomCaptureInfos.CCst.EFFECTIVE_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.TerminationDate.AdjustableDate.DateAdjustments;
                    break;
                case CciEnum.effectiveDate_relativeDate_bDC:
                    id = TradeCustomCaptureInfos.CCst.EFFECTIVE_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.EffectiveDate.RelativeDate.GetAdjustments;
                    break;
                case CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC:
                    id = TradeCustomCaptureInfos.CCst.TERMINATION_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.TerminationDate.AdjustableDate.DateAdjustments;
                    break;
                case CciEnum.terminationDate_relativeDate_bDC:
                    id = TradeCustomCaptureInfos.CCst.TERMINATION_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.TerminationDate.RelativeDate.GetAdjustments;
                    break;
            }
            //
            Ccis.DumpBDC_ToDocument(bda, CciClientId(pCciBdc), CciClientId(id), cciBC);
        }

        /// <summary>
        /// Alimente le regEx de du cci CciEnum.marginRatio_amount en fonction de _returnLeg.rateOfReturn.marginRatio.priceExpression
        /// </summary>
        /// FI 20140811 [XXXXX] Modify
        private void SetMarginRatioRegEx()
        {
            //FI 20140811 Utilisation de la méthode ccis.Set car elle teste la présence du cci marginRatio_amount
            if (_returnLeg.RateOfReturn.MarginRatio.PriceExpression == PriceExpressionEnum.PercentageOfNotional)
                CcisBase.Set(CciClientId(CciEnumMarginRatio.marginRatio_amount), "Regex", EFSRegex.TypeRegex.RegexFixedRateExtend);
            else
                CcisBase.Set(CciClientId(CciEnumMarginRatio.marginRatio_amount), "Regex", EFSRegex.TypeRegex.RegexDecimalExtend);
        }

        /// <summary>
        ///  pré-proposition des dates de compensation et de transaction en fonction de ENTITYMARKET 
        /// </summary>
        /// EG 20171004 [23452] TradeDateTime
        /// EG 20171016 [23509] Upd
        private void InitializeDateFromEntityMarket()
        {
            int idAEntity = _cciTrade.DataDocument.GetFirstEntity(_cciTrade.CSCacheOn);
            if (0 < idAEntity)
            {
                _cciTrade.Product.GetMarket(_cciTrade.CSCacheOn, null, out SQL_Market sqlMarket);
                if (null != sqlMarket)
                {
                    DateTime dt = OTCmlHelper.GetDateBusinessCustodian(_cciTrade.CS, idAEntity, sqlMarket.Id, IdA_Custodian.Value);

                    if (DtFunc.IsDateTimeFilled(dt))
                    {

                        //FI 20180301 [23814] Utilisation de clearedDate du cciMarket[0]
                        //FI 20180607 [XXXXX] (réécriture du code en utilisant cciProduct.CciClearedDate et cciProduct.CciExecutionDateTime
                        //=> Utilisation de _cciTrade.cciProduct.CciClearedDate et  _cciTrade.cciProduct.CciExecutionDateTime
                        DateTime currentClearedDate = new EFS_Date(_cciTrade.cciProduct.CciClearedDate.NewValue).DateValue;
                        if (currentClearedDate < dt | (CcisBase.CaptureMode == Cst.Capture.ModeEnum.New))
                        {
                            _cciTrade.cciProduct.CciClearedDate.NewValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                            if (CcisBase.CaptureMode != Cst.Capture.ModeEnum.PositionTransfer)
                            {
                                _cciTrade.cciProduct.CciExecutionDateTime.NewValue =
                                // FI 20200904 [XXXXX] call OTCmlHelper.GetDateSys
                                //Tz.Tools.AddTimeToDateReturnString(dt, OTCmlHelper.GetDateBusiness(_cciTrade.CS));
                                Tz.Tools.AddTimeToDateReturnString(dt, OTCmlHelper.GetDateSys(_cciTrade.CS));
                            }
                        }
                    }
                }
            }
        }
        #endregion Methods
    }
}

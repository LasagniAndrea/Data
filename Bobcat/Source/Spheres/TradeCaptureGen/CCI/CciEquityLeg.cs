#region Using Directives
using System;
using System.Reflection;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.EFSTools;
using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;

using EfsML.Enum.Tools;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciReturnLeg.
    /// </summary>
    public class CciReturnLeg : IContainerCci, IContainerCciPayerReceiver, IContainerCciFactory, IContainerCciSpecified
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
            //underlyer
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.underlyingAsset")]
            underlyer_singleUnderlyer_underlyingAsset,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.openUnits")]
            underlyer_singleUnderlyer_openUnits,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.dividendPayout.dividendPayoutRatio")]
            underlyer_singleUnderlyer_dividendPayout_dividendPayoutRatio,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.dividendPayout.dividendPayoutConditions")]
            underlyer_singleUnderlyer_dividendPayout_Conditions,
            [System.Xml.Serialization.XmlEnumAttribute("underlyer.underlyerSingle.underlyingAsset.currency")]
            underlyer_singleUnderlyer_currency,
            //equityAmount
            [System.Xml.Serialization.XmlEnumAttribute("equityAmount.legAmountReferenceAmount")]
            equityAmount_referenceAmount,
            [System.Xml.Serialization.XmlEnumAttribute("equityAmount.cashSettlement")]
            equityAmount_cashSettlement,
            //notional
            [System.Xml.Serialization.XmlEnumAttribute("notional.equitySwapNotionalNotionalAmount.amount")]
            notional_notionalAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notional.equitySwapNotionalNotionalAmount.currency")]
            notional_notionalAmount_currency,
            //return
            [System.Xml.Serialization.XmlEnumAttribute("@return.returnType")]
            return_returnType,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendEntitlement")]
            return_dividendConditions_dividendEntitlement,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendAmount")]
            return_dividendConditions_dividendAmount,
            [System.Xml.Serialization.XmlEnumAttribute("@return.dividendConditions.dividendPaymentDate.paymentDateDividendDateReference")]
            return_dividendConditions_dividendPaymentDate_dividendDateReference,
            //
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature.referenceCurrency")]
            fxFeature_referenceCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature.fxFeatureQuanto")]
            fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis,
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature.fxFeatureQuanto")]
            fxFeature_quanto_fxRate_rate,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private IReturnLeg _returnLeg;
        private string _prefix;
        private IReturnSwap _returnSwap;        
        
        private CciTradeBase _cciTrade;
        private CciReturnLegValuationPrice cciInitialPrice;
        private CciReturnLegValuationPrice cciValuationPriceInterim;
        private CciReturnLegValuationPrice cciValuationPriceFinal;
        #endregion Members
        #region Accessors
        #region ccis
        public TradeCustomCaptureInfos ccis
        {
            get { return _cciTrade.ccis; }
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
                if (_returnLeg.fxFeatureSpecified)
                    ret = _returnLeg.fxFeature.referenceCurrency.Value;
                return ret;
            }
        }
        #endregion GetFxFeatureReferenceCurrency
        #region GetUnderlyingAssetCurrency
        public string GetUnderlyingAssetCurrency
        {
            get
            {
                string ret = null;
                #warning 20070525 PL Add Try/Catch pour palier à un bug... A revoir avec FI
                try
                {                   
                    if (_returnLeg.underlyer.underlyerSingleSpecified)
                        ret = _returnLeg.underlyer.underlyerSingle.underlyingAsset.currency.Value;
                    else
                        ret = _returnLeg.underlyer.underlyerBasket.basketConstituent[0].underlyingAsset.currency.Value;
                }
                catch { }
                return ret;
            }
        }
        #endregion GetUnderlyingAssetCurrency
        #region CciValuationPriceInterim
        public CciReturnLegValuationPrice CciValuationPriceInterim
        {
            get
            {
                return cciValuationPriceInterim;
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
        public CciReturnLeg(CciTradeBase pCciTrade, int pLegNumber, IReturnSwap pReturnSwap, IReturnLeg pEquityLeg, string pPrefix)
        {
            _cciTrade = pCciTrade;
            _returnLeg = pEquityLeg;
            _returnSwap = pReturnSwap;
            _prefix = pPrefix + pLegNumber.ToString() + CustomObject.KEY_SEPARATOR;
            //
            
            //cciInitialPrice = new CciEquityValuationPrice(pTrade, this, pEquityLeg.valuation.initialPrice, prefix + "valuation_initialPrice");    
            //cciValuationPriceInterim  = new CciEquityValuationPrice(pTrade, this, pEquityLeg.valuation.valuationPriceInterim , prefix + "valuation_valuationPriceInterim");
            //cciValuationPriceFinal = new CciEquityValuationPrice(pTrade, this, pEquityLeg.valuation.valuationPriceFinal, prefix + "valuation_valuationPriceFinal");
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciSpecified Members
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion IContainerCciSpecified Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(string pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            bool isOk = false;
            //FI 20110418 [17405] ccis.Contains en commentaire => Tuning
            //isOk = ccis.Contains(pClientId_WithoutPrefix);
            //isOk = isOk && (pClientId_WithoutPrefix.StartsWith(m_Prefix));	
            isOk =  (pClientId_WithoutPrefix.StartsWith(_prefix));
            return isOk;
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
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
            ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion
        #endregion SynchronizePayerReceiver
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
            if (false == ccis.Contains(CciClientId(CciEnum.payer)))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.payer), true, TypeData.TypeDataEnum.@string));

            if (false == ccis.Contains(CciClientId(CciEnum.receiver)))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.receiver), true, TypeData.TypeDataEnum.@string));

            if (false == ccis.Contains(CciClientId(CciEnum.underlyer_singleUnderlyer_currency)))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.underlyer_singleUnderlyer_currency), false, TypeData.TypeDataEnum.@string));

            // Si pas alimenter il sont renseigner par in itial Price
            if (false == ccis.Contains(CciClientId(CciEnum.notional_notionalAmount_amount)))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.notional_notionalAmount_amount), false, TypeData.TypeDataEnum.@string));
            if (false == ccis.Contains(CciClientId(CciEnum.notional_notionalAmount_currency)))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.notional_notionalAmount_currency), false, TypeData.TypeDataEnum.@string));

            // GlopFI  Ajout des Bconvention
            if (null != cciInitialPrice)
                cciInitialPrice.AddCciSystem();
            if (null != cciValuationPriceInterim)
                cciValuationPriceInterim.AddCciSystem();
            if (null != cciValuationPriceFinal)
                cciValuationPriceFinal.AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
            //Nothing pour l'instant	
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            try
            {
                bool isSetting;
                string data = string.Empty;
                Type tCciEnum = typeof(CciEnum);
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    CustomCaptureInfo cci = ccis[_prefix + enumName];
                    if ((cci != null) && (cci.HasChanged))
                    {
                        isSetting = true;
                        data = cci.newValue;
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.payer:
                                #region payer
                                ((IReturnSwapLeg)_returnLeg).payerPartyReference.hRef = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//synchronize Payer/receiver
                                #endregion payer
                                break;
                            case CciEnum.receiver:
                                #region receiver
                                ((IReturnSwapLeg)_returnLeg).receiverPartyReference.hRef = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//synchronize Payer/receiver
                                #endregion receiver
                                break;
                            case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                                #region EffectiveDate (AdjustableDate)
                                _returnLeg.effectiveDate.adjustableDateSpecified = cci.IsFilledValue;
                                _returnLeg.effectiveDate.relativeDateSpecified = (!cci.IsFilledValue);
                                _returnLeg.effectiveDate.adjustableDate.unadjustedDate.Value = data;
                                #endregion EffectiveDate (AdjustableDate)
                                break;
                            case CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC:
                                #region EffectiveDate BDC (AdjustableDate)
                                if (_returnLeg.effectiveDate.adjustableDateSpecified)
                                {
                                    BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                                    _returnLeg.effectiveDate.adjustableDate.dateAdjustments.businessDayConvention = bDCEnum;
                                }
                                DumpBDC(keyEnum);
                                #endregion EffectiveDate BDC (AdjustableDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_dateRelativeTo:
                                #region EffectiveDate RelativeTo( RelativeDate)
                                _returnLeg.effectiveDate.relativeDateSpecified = cci.IsFilledValue;
                                _returnLeg.effectiveDate.adjustableDateSpecified = (!cci.IsFilledValue);
                                _returnLeg.effectiveDate.relativeDate.DateRelativeToValue = data;
                                #endregion EffectiveDate RelativeTo( RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_period:
                                #region EffectiveDate Period ( RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                {
                                    PeriodEnum periodEnum = StringToEnum.Period(data);
                                    _returnLeg.effectiveDate.relativeDate.period = periodEnum;
                                }
                                #endregion EffectiveDate Period ( RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_periodMultiplier:
                                #region EffectiveDate Multiplier( RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                    _returnLeg.effectiveDate.relativeDate.periodMultiplier.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; // if period <> D => DayType = empty
                                #endregion EffectiveDate Multiplier( RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_bDC:
                                #region EffectiveDate BDC( RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                {
                                    if (cci.IsFilledValue)
                                    {
                                        BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                                        _returnLeg.effectiveDate.relativeDate.businessDayConvention = bDCEnum;
                                    }
                                    DumpBDC(keyEnum);
                                }
                                #endregion EffectiveDate BDC( RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_dayType:
                                #region EffectiveDate DayType( RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                {
                                    _returnLeg.effectiveDate.relativeDate.dayTypeSpecified = cci.IsFilledValue;
                                    if (cci.IsFilledValue)
                                    {
                                        DayTypeEnum dayTypeEnum = StringToEnum.DayType(data);
                                        _returnLeg.effectiveDate.relativeDate.dayType = dayTypeEnum;
                                    }
                                }
                                #endregion EffectiveDate DayType( RelativeDate)
                                break;
                            case CciEnum.terminationDate_adjustableDate_unadjustedDate:
                                #region TerminationDate (AdjustableDate)
                                _returnLeg.terminationDate.adjustableDateSpecified = cci.IsFilledValue;
                                _returnLeg.terminationDate.relativeDateSpecified = (!cci.IsFilledValue);
                                _returnLeg.terminationDate.adjustableDate.unadjustedDate.Value = data;
                                #endregion TerminationDate (AdjustableDate)
                                break;
                            case CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC:
                                #region TerminationDate BDC (AdjustableDate)
                                if (_returnLeg.terminationDate.adjustableDateSpecified)
                                {
                                    BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                                    _returnLeg.terminationDate.adjustableDate.dateAdjustments.businessDayConvention = bDCEnum;
                                    DumpBDC(keyEnum);
                                }
                                #endregion TerminationDate BDC (AdjustableDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_dateRelativeTo:
                                #region TerminationDate RelativeTo (RelativeDate)
                                _returnLeg.terminationDate.relativeDateSpecified = cci.IsFilledValue;
                                _returnLeg.terminationDate.adjustableDateSpecified = (!cci.IsFilledValue);
                                _returnLeg.terminationDate.relativeDate.DateRelativeToValue = data;
                                #endregion TerminationDate RelativeTo (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_period:
                                #region TerminationDate Period (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                {
                                    PeriodEnum period = StringToEnum.Period(data);
                                    _returnLeg.terminationDate.relativeDate.period = period;
                                }
                                #endregion TerminationDate Period (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_periodMultiplier:
                                #region TerminationDate Multiplier (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                    _returnLeg.terminationDate.relativeDate.periodMultiplier.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; // if period <> D => DayType = empty
                                #endregion TerminationDate Multiplier (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_bDC:
                                #region TerminationDate BDC (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                {
                                    if (cci.IsFilledValue)
                                    {
                                        BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                                        _returnLeg.terminationDate.relativeDate.businessDayConvention = bDCEnum;
                                        DumpBDC(keyEnum);
                                    }
                                }
                                #endregion TerminationDate BDC (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_dayType:
                                #region TerminationDate DayType (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                {
                                    if (cci.IsFilledValue)
                                    {
                                        DayTypeEnum dayTypeEnum = StringToEnum.DayType(data);
                                        _returnLeg.terminationDate.relativeDate.dayType = dayTypeEnum;
                                    }
                                }
                                #endregion TerminationDate DayType (RelativeDate)
                                break;
                            case CciEnum.underlyer_singleUnderlyer_underlyingAsset:
                                #region singleUnderlyer
                                //IEquityAsset equityAsset = null;
                                IUnderlyingAsset underlyingAsset = null;
                                //
                                if (false == _returnLeg.underlyer.underlyerSingleSpecified &&
                                    false == _returnLeg.underlyer.underlyerBasketSpecified)
                                    _returnLeg.underlyer.underlyerSingleSpecified = true;
                                //
                                if (_returnLeg.underlyer.underlyerSingleSpecified)
                                    underlyingAsset = /*(IEquityAsset)*/_returnLeg.underlyer.underlyerSingle.underlyingAsset;
                                else if (_returnLeg.underlyer.underlyerBasketSpecified)
                                    underlyingAsset = /*(IEquityAsset)*/_returnLeg.underlyer.underlyerBasket.basketConstituent[0].underlyingAsset;
                                //                            
                                SQL_AssetEquity sql_AssetEquity = null;
                                bool isLoaded = false;
                                cci.errorMsg = string.Empty;
                                if (StrFunc.IsFilled(data))
                                {
                                    //Tip: Utile pour trouver "TIM.MI" à partir de "IM MI" via "%IM%MI%"
                                    for (int i = 0; i < 2; i++)
                                    {
                                        string dataToFind = data;
                                        if (i == 1)
                                            dataToFind = data.Replace(" ", "%") + "%";
                                        sql_AssetEquity = new SQL_AssetEquity(_cciTrade.CSCacheOn, SQL_AssetEquity.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                        isLoaded = sql_AssetEquity.IsLoaded && (sql_AssetEquity.RowsCount == 1);
                                        if (isLoaded)
                                            break;
                                    }
                                    //
                                    if (isLoaded)
                                    {
                                        cci.newValue = sql_AssetEquity.Identifier;
                                        cci.sql_Table = sql_AssetEquity;

                                        underlyingAsset.OTCmlId = sql_AssetEquity.IdAsset;
                                        underlyingAsset.instrumentId = underlyingAsset.CreateInstrument(sql_AssetEquity.Identifier);
                                        //underlyingAsset.instrumentId[0].Value = sql_AssetEquity.Asset_Identifier;
                                        underlyingAsset.clearanceSystemSpecified = StrFunc.IsFilled(sql_AssetEquity.ClearanceSystem);
                                        if (underlyingAsset.clearanceSystemSpecified)
                                            underlyingAsset.clearanceSystem.Value = sql_AssetEquity.ClearanceSystem;
                                        //description
                                        underlyingAsset.definitionSpecified = false;
                                        underlyingAsset.descriptionSpecified = StrFunc.IsFilled(sql_AssetEquity.Description);
                                        if (underlyingAsset.descriptionSpecified)
                                            underlyingAsset.description = new EFS_String(sql_AssetEquity.Description);
                                        //Currency
                                        underlyingAsset.currencySpecified = StrFunc.IsFilled(sql_AssetEquity.IdC);
                                        if (underlyingAsset.currencySpecified)
                                            underlyingAsset.currency = underlyingAsset.CreateCurrency(sql_AssetEquity.IdC);
                                        //ExchangeId
                                        //PL 20130208 ISO-GLOP
                                        //underlyingAsset.exchangeIdSpecified = StrFunc.IsFilled(sql_AssetEquity.Market_Identifier);
                                        underlyingAsset.exchangeIdSpecified = StrFunc.IsFilled(sql_AssetEquity.Market_FIXML_SecurityExchange);
                                        if (underlyingAsset.exchangeIdSpecified)
                                        {
                                            //PL 20130208 ISO-GLOP
                                            //underlyingAsset.exchangeId = underlyingAsset.CreateExchangeId(sql_AssetEquity.Market_Identifier);
                                            underlyingAsset.exchangeId = underlyingAsset.CreateExchangeId(sql_AssetEquity.Market_FIXML_SecurityExchange);
                                            underlyingAsset.OTCmlId = sql_AssetEquity.IdM;
                                        }
                                    }
                                    //
                                    cci.errorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                                }
                                if (false == isLoaded)
                                {
                                    cci.sql_Table = null;

                                    underlyingAsset.OTCmlId = 0;
                                    underlyingAsset.CreateInstrument(string.Empty);
                                    //underlyingAsset.instrumentId[0].Value = string.Empty;
                                    underlyingAsset.clearanceSystemSpecified = false;
                                    underlyingAsset.definitionSpecified = false;
                                    underlyingAsset.descriptionSpecified = false;
                                    underlyingAsset.currencySpecified = false;
                                    underlyingAsset.exchangeIdSpecified = false;
                                    //equityAsset.relatedExchangeIdSpecified = false;
                                    //equityAsset.optionsExchangeIdSpecified = false;
                                }
                                //
                                DumpBDC(CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC);
                                DumpBDC(CciEnum.effectiveDate_relativeDate_bDC);
                                DumpBDC(CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC);
                                DumpBDC(CciEnum.terminationDate_relativeDate_bDC);
                                //
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer les BCs ??
                                #endregion singleUnderlyer
                                break;
                            case CciEnum.underlyer_singleUnderlyer_openUnits:
                                #region singleUnderlyer OpenUnits
                                _returnLeg.underlyer.underlyerSingle.openUnitsSpecified = cci.IsFilledValue;
                                _returnLeg.underlyer.underlyerSingle.openUnits.Value = data;
                                #endregion singleUnderlyer OpenUnits
                                break;
                            case CciEnum.underlyer_singleUnderlyer_dividendPayout_dividendPayoutRatio:
                                #region singleUnderlyer DividendPayout
                                _returnLeg.underlyer.underlyerSingle.dividendPayout.dividendPayoutRatioSpecified = cci.IsFilledValue;

                                _returnLeg.underlyer.underlyerSingle.dividendPayoutSpecified =
                                    _returnLeg.underlyer.underlyerSingle.dividendPayout.dividendPayoutRatioSpecified
                                    ||
                                    _returnLeg.underlyer.underlyerSingle.dividendPayout.dividendPayoutConditionsSpecified;

                                _returnLeg.underlyer.underlyerSingle.dividendPayout.dividendPayoutRatio.Value = data;
                                #endregion singleUnderlyer DividendPayout
                                break;
                            case CciEnum.notional_notionalAmount_amount:
                                #region notionalAmount_amount
                                _returnLeg.notional.notionalAmount.amount.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                #endregion notionalAmount_amount
                                break;
                            case CciEnum.notional_notionalAmount_currency:
                                #region notionalAmount_currency
                                _returnLeg.notional.notionalAmount.currency = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                #endregion notionalAmount_currency
                                break;
                            case CciEnum.equityAmount_referenceAmount:
                                #region equityAmount
                                _returnLeg.returnSwapAmount.referenceAmountSpecified = cci.IsFilledValue;
                                if (_returnLeg.returnSwapAmount.referenceAmountSpecified)
                                    _returnLeg.returnSwapAmount.referenceAmount.Value = data;
                                #endregion equityAmount
                                break;
                            case CciEnum.equityAmount_cashSettlement:
                                #region equityAmount CashSettlement
                                _returnLeg.returnSwapAmount.cashSettlement.Value = data;
                                #endregion equityAmount CashSettlement
                                break;
                            case CciEnum.return_returnType:
                                #region return_returnType
                                ReturnTypeEnum returnEnum = (ReturnTypeEnum)System.Enum.Parse(typeof(ReturnTypeEnum), data, true);
                                _returnLeg.@return.returnType = returnEnum;
                                #endregion return_returnType
                                break;
                            case CciEnum.return_dividendConditions_dividendEntitlement:
                                #region return_dividendConditions_dividendEntitlement
                                _returnLeg.@return.dividendConditions.dividendEntitlementSpecified = cci.IsFilledValue;
                                if (cci.IsFilledValue)
                                {
                                    DividendEntitlementEnum divEntitlementEnum = (DividendEntitlementEnum)System.Enum.Parse(typeof(DividendEntitlementEnum), data, true);
                                    _returnLeg.@return.dividendConditions.dividendEntitlement = divEntitlementEnum;
                                    _returnLeg.@return.dividendConditionsSpecified = true;
                                }
                                #endregion return_dividendConditions_dividendEntitlement
                                break;
                            case CciEnum.return_dividendConditions_dividendAmount:
                                #region return_dividendConditions_dividendAmount
                                _returnLeg.@return.dividendConditions.dividendAmountSpecified = cci.IsFilledValue;
                                if (cci.IsFilledValue)
                                {
                                    DividendAmountTypeEnum divAmountEnum = (DividendAmountTypeEnum)System.Enum.Parse(typeof(DividendAmountTypeEnum), data, true);
                                    _returnLeg.@return.dividendConditions.dividendAmount = divAmountEnum;
                                    _returnLeg.@return.dividendConditionsSpecified = true;
                                }
                                #endregion return_dividendConditions_dividendAmount
                                break;
                            case CciEnum.return_dividendConditions_dividendPaymentDate_dividendDateReference:
                                #region return_dividendConditions_dividendPaymentDate_dividendDateReference
                                _returnLeg.@return.dividendConditions.dividendPaymentDate.dividendDateReferenceSpecified = cci.IsFilledValue;
                                if (cci.IsFilledValue)
                                {
                                    DividendDateReferenceEnum divDateReferenceEnum = (DividendDateReferenceEnum)System.Enum.Parse(typeof(DividendDateReferenceEnum), data, true);
                                    _returnLeg.@return.dividendConditions.dividendPaymentDate.dividendDateReference = divDateReferenceEnum;
                                    _returnLeg.@return.dividendConditions.dividendPaymentDateSpecified = true;
                                    _returnLeg.@return.dividendConditionsSpecified = true;
                                }
                                #endregion return_dividendConditions_dividendPaymentDate_dividendDateReference
                                break;
                            
                            case CciEnum.fxFeature_referenceCurrency:
                                #region fxFeature_referenceCurrency
                                _returnLeg.fxFeatureSpecified = cci.IsFilledValue;
                                if (_returnLeg.fxFeatureSpecified)
                                    _returnLeg.fxFeature.referenceCurrency.Value = data;
                                #endregion fxFeature_referenceCurrency
                                break;
                            case CciEnum.fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis:
                                #region fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                                _returnLeg.fxFeature.fxFeatureQuantoSpecified = cci.IsFilledValue;
                                if ((_returnLeg.fxFeature.fxFeatureQuantoSpecified) && StrFunc.IsFilled(data))
                                {
                                    QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                                    _returnLeg.fxFeature.fxFeatureQuanto.fxRate[0].quotedCurrencyPair.quoteBasis = QbEnum;
                                }
                                #endregion fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                                break;
                            case CciEnum.fxFeature_quanto_fxRate_rate:
                                #region fxFeature_quanto_fxRate_rate
                                _returnLeg.fxFeature.fxFeatureQuanto.fxRate[0].rate.Value = data;
                                #endregion fxFeature_quanto_fxRate_rate
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
                if (null != cciInitialPrice)
                    cciInitialPrice.Dump_ToDocument();
                if (null != cciValuationPriceInterim)
                    cciValuationPriceInterim.Dump_ToDocument();
                if (null != cciValuationPriceFinal)
                    cciValuationPriceFinal.Dump_ToDocument();
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _returnLeg);
            //
            CciReturnLegValuationPrice valInitialPrice = new CciReturnLegValuationPrice(_cciTrade , this, _returnSwap , null, _prefix + "valuation_initialPrice");
            //
            if (null == _returnLeg.valuation)
                _returnLeg.valuation = _returnLeg.CreateValuation;
            //
            if (ccis.Contains(valInitialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_amount)))
            {
                if (null == _returnLeg.valuation.initialPrice)
                    _returnLeg.valuation.initialPrice = _returnLeg.valuation.CreateReturnLegValuationPrice;
                //
                valInitialPrice.ReturnLegValuationPrice = _returnLeg.valuation.initialPrice;
                //
                cciInitialPrice = valInitialPrice;
            }
            //
            CciReturnLegValuationPrice valPriceInterim = new CciReturnLegValuationPrice(_cciTrade, this, _returnSwap,  null, _prefix + "valuation_valuationPriceInterim");
            //
            if (ccis.Contains(valPriceInterim.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_amount)))
            {
                if (null == _returnLeg.valuation.valuationPriceInterim)
                    _returnLeg.valuation.valuationPriceInterim = _returnLeg.valuation.CreateReturnLegValuationPrice;
                //
                _returnLeg.valuation.valuationPriceInterimSpecified = true;
                //
                valPriceInterim.ReturnLegValuationPrice = _returnLeg.valuation.valuationPriceInterim;
                //
                cciValuationPriceInterim = valPriceInterim;
            }
            //
            CciReturnLegValuationPrice valPriceFinal = new CciReturnLegValuationPrice(_cciTrade, this,_returnSwap,  null, _prefix + "valuation_valuationPriceFinal");
            //
            if (ccis.Contains(valPriceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_amount)))
            {
                if (null == _returnLeg.valuation.valuationPriceInterim)
                    _returnLeg.valuation.valuationPriceFinal = _returnLeg.valuation.CreateReturnLegValuationPrice;
                //
                valPriceFinal.ReturnLegValuationPrice = _returnLeg.valuation.valuationPriceFinal;
                //
                cciValuationPriceFinal = valPriceFinal;
            }
            //
            if (ccis.Contains(CciClientId(CciEnum.fxFeature_referenceCurrency)))
            {
                if (false == _returnLeg.fxFeatureSpecified)
                    _returnLeg.fxFeature = _returnLeg.CreateFxFeature;
                if (false == _returnLeg.fxFeature.fxFeatureQuantoSpecified)
                    _returnLeg.fxFeature.fxFeatureQuanto.fxRate = _returnLeg.fxFeature.fxFeatureQuanto.CreateFxRate;
            }
            ////=> Ajout equityvaluationPriceInterim qu'ils existent des ccis ou non (la gestion s'appuie sur specified) 
            //if (false == equileg.valuation.valuationPriceInterimSpecified)
            //{
            //    cciValuationPriceInterim.ReturnLegValuationPrice = equileg.valuation.CreateReturnLegValuationPrice;
            //}
            //
            if (null != cciInitialPrice)
                cciInitialPrice.Initialize_FromCci();
            if (null != cciValuationPriceInterim)
                cciValuationPriceInterim.Initialize_FromCci();
            if (null != cciValuationPriceFinal)
                cciValuationPriceFinal.Initialize_FromCci();
            //
            if (null == _returnLeg.returnSwapAmount)
                _returnLeg.returnSwapAmount = _returnLeg.CreateReturnSwapAmount;
            if (null == _returnLeg.@return)
                _returnLeg.@return = _returnLeg.CreateReturn;
            if (null == ((IReturnSwapLeg)_returnLeg).receiverPartyReference)
                ((IReturnSwapLeg)_returnLeg).receiverPartyReference = ((IReturnSwapLeg)_returnLeg).CreateReference;
            if (null == ((IReturnSwapLeg)_returnLeg).payerPartyReference)
                ((IReturnSwapLeg)_returnLeg).payerPartyReference = ((IReturnSwapLeg)_returnLeg).CreateReference;
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            try
            {
                string data;
                bool isSetting;
                SQL_Table sql_Table;
                //
                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = ccis[_prefix + enumName];
                    if (cci != null)
                    {
                        #region Reset variables
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion Reset variables

                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.payer:
                                #region Payer
                                data = ((IReturnSwapLeg)_returnLeg).payerPartyReference.hRef;
                                #endregion Payer
                                break;
                            case CciEnum.receiver:
                                #region Receiver
                                data = ((IReturnSwapLeg)_returnLeg).receiverPartyReference.hRef;
                                #endregion Receiver
                                break;
                            case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                                #region EffectiveDate (AdjustableDate)
                                if (_returnLeg.effectiveDate.adjustableDateSpecified)
                                    data = _returnLeg.effectiveDate.adjustableDate.unadjustedDate.Value;
                                #endregion EffectiveDate (AdjustableDate)
                                break;
                            case CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC:
                                #region EffectiveDate BDC (AdjustableDate)
                                if (_returnLeg.effectiveDate.adjustableDateSpecified)
                                    data = _returnLeg.effectiveDate.adjustableDate.dateAdjustments.businessDayConvention.ToString();
                                #endregion EffectiveDate BDC (AdjustableDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_dateRelativeTo:
                                #region EffectiveDate RelativeTo (RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                    data = _returnLeg.effectiveDate.relativeDate.DateRelativeToValue;
                                #endregion EffectiveDate RelativeTo (RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_period:
                                #region EffectiveDate Period (RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                    data = _returnLeg.effectiveDate.relativeDate.period.ToString();
                                #endregion EffectiveDate Period (RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_periodMultiplier:
                                #region EffectiveDate Multiplier (RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                    data = _returnLeg.effectiveDate.relativeDate.periodMultiplier.Value;
                                #endregion EffectiveDate Multiplier (RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_bDC:
                                #region EffectiveDate BDC (RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                    data = _returnLeg.effectiveDate.relativeDate.businessDayConvention.ToString();
                                #endregion EffectiveDate BDC (RelativeDate)
                                break;
                            case CciEnum.effectiveDate_relativeDate_dayType:
                                #region EffectiveDate DayType (RelativeDate)
                                if (_returnLeg.effectiveDate.relativeDateSpecified)
                                    if (_returnLeg.effectiveDate.relativeDate.dayTypeSpecified)
                                        data = _returnLeg.effectiveDate.relativeDate.dayType.ToString();
                                #endregion EffectiveDate DayType (RelativeDate)
                                break;
                            case CciEnum.terminationDate_adjustableDate_unadjustedDate:
                                #region TerminationDate (AdjustableDate)
                                if (_returnLeg.terminationDate.adjustableDateSpecified)
                                    data = _returnLeg.terminationDate.adjustableDate.unadjustedDate.Value;
                                #endregion TerminationDate (AdjustableDate)
                                break;
                            case CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC:
                                #region TerminationDate BDC (AdjustableDate)
                                if (_returnLeg.terminationDate.adjustableDateSpecified)
                                    data = _returnLeg.terminationDate.adjustableDate.dateAdjustments.businessDayConvention.ToString();
                                #endregion TerminationDate BDC (AdjustableDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_dateRelativeTo:
                                #region TerminationDate RelativeTo (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                    data = _returnLeg.terminationDate.relativeDate.DateRelativeToValue;
                                #endregion TerminationDate RelativeTo (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_period:
                                #region TerminationDate Period (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                    data = _returnLeg.terminationDate.relativeDate.period.ToString();
                                #endregion TerminationDate Period (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_periodMultiplier:
                                #region TerminationDate Multiplier (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                    data = _returnLeg.terminationDate.relativeDate.periodMultiplier.Value;
                                #endregion TerminationDate Multiplier (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_bDC:
                                #region TerminationDate BDC (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                    data = _returnLeg.terminationDate.relativeDate.businessDayConvention.ToString();
                                #endregion TerminationDate BDC (RelativeDate)
                                break;
                            case CciEnum.terminationDate_relativeDate_dayType:
                                #region TerminationDate DayType (RelativeDate)
                                if (_returnLeg.terminationDate.relativeDateSpecified)
                                    if (_returnLeg.terminationDate.relativeDate.dayTypeSpecified)
                                        data = _returnLeg.terminationDate.relativeDate.dayType.ToString();
                                #endregion TerminationDate DayType (RelativeDate)
                                break;
                            case CciEnum.underlyer_singleUnderlyer_underlyingAsset:
                                #region singleUnderlyer
                                try
                                {
                                    IEquityAsset equityAsset = null;
                                    if (false == _returnLeg.underlyer.underlyerSingleSpecified &&
                                        false == _returnLeg.underlyer.underlyerBasketSpecified)
                                        _returnLeg.underlyer.underlyerSingleSpecified = true;

                                    if (_returnLeg.underlyer.underlyerSingleSpecified)
                                        equityAsset = (IEquityAsset)_returnLeg.underlyer.underlyerSingle.underlyingAsset;
                                    else if (_returnLeg.underlyer.underlyerBasketSpecified)
                                        equityAsset = (IEquityAsset)_returnLeg.underlyer.underlyerBasket.basketConstituent[0].underlyingAsset;

                                    int idAsset = equityAsset.OTCmlId;
                                    if (idAsset > 0)
                                    {
                                        SQL_AssetEquity sql_AssetEquity = new SQL_AssetEquity(_cciTrade.CSCacheOn, idAsset);
                                        if (sql_AssetEquity.IsLoaded && (sql_AssetEquity.RowsCount == 1))
                                        {
                                            cci.sql_Table = sql_AssetEquity;
                                            data = sql_AssetEquity.Identifier;
                                        }
                                    }
                                }
                                catch
                                {
                                    cci.sql_Table = null;
                                    data = string.Empty;
                                }
                                #endregion singleUnderlyer
                                break;
                            case CciEnum.underlyer_singleUnderlyer_currency:
                                #region singleUnderlyer currency
                                data = this.GetUnderlyingAssetCurrency;
                                #endregion singleUnderlyer currency
                                break;

                            case CciEnum.underlyer_singleUnderlyer_openUnits:
                                #region singleUnderlyer openUnits
                                data = _returnLeg.underlyer.underlyerSingle.openUnits.Value;
                                #endregion singleUnderlyer openUnits
                                break;

                            case CciEnum.underlyer_singleUnderlyer_dividendPayout_dividendPayoutRatio:
                                #region singleUnderlyer DividentPayout
                                if (_returnLeg.underlyer.underlyerSingle.dividendPayoutSpecified)
                                    if (_returnLeg.underlyer.underlyerSingle.dividendPayout.dividendPayoutRatioSpecified)
                                        data = _returnLeg.underlyer.underlyerSingle.dividendPayout.dividendPayoutRatio.Value;
                                #endregion singleUnderlyer DividentPayout
                                break;
                            case CciEnum.notional_notionalAmount_amount:
                                #region notionalAmount_amount
                                data = _returnLeg.notional.notionalAmount.amount.Value;
                                #endregion notionalAmount_amount
                                break;
                            case CciEnum.notional_notionalAmount_currency:
                                #region notionalAmount_currency
                                data = _returnLeg.notional.notionalAmount.currency;
                                #endregion notionalAmount_currency
                                break;
                            case CciEnum.equityAmount_referenceAmount:
                                #region equityAmount
                                if (_returnLeg.returnSwapAmount.referenceAmountSpecified)
                                    data = _returnLeg.returnSwapAmount.referenceAmount.Value;
                                #endregion equityAmount
                                break;
                            case CciEnum.equityAmount_cashSettlement:
                                #region equityAmount CashSettlement
                                data = _returnLeg.returnSwapAmount.cashSettlement.Value.ToUpper();
                                #endregion equityAmount CashSettlement
                                break;
                            case CciEnum.return_returnType:
                                #region return_returnType
                                data = _returnLeg.@return.returnType.ToString();
                                #endregion return_returnType
                                break;
                            case CciEnum.return_dividendConditions_dividendEntitlement:
                                #region return_dividendConditions_dividendEntitlement
                                if (_returnLeg.@return.dividendConditionsSpecified)
                                    if (_returnLeg.@return.dividendConditions.dividendEntitlementSpecified)
                                        data = _returnLeg.@return.dividendConditions.dividendEntitlement.ToString();
                                #endregion return_dividendConditions_dividendEntitlement
                                break;
                            case CciEnum.return_dividendConditions_dividendAmount:
                                #region return_dividendConditions_dividendAmount
                                if ((_returnLeg.@return.dividendConditionsSpecified) &&
                                    (_returnLeg.@return.dividendConditions.dividendAmountSpecified))
                                    data = _returnLeg.@return.dividendConditions.dividendAmount.ToString();
                                #endregion return_dividendConditions_dividendAmount
                                break;
                            case CciEnum.return_dividendConditions_dividendPaymentDate_dividendDateReference:
                                #region return_dividendConditions_dividendPaymentDate_dividendDateReference
                                if ((_returnLeg.@return.dividendConditionsSpecified) &&
                                     (_returnLeg.@return.dividendConditions.dividendPaymentDateSpecified))
                                    if (_returnLeg.@return.dividendConditions.dividendPaymentDate.dividendDateReferenceSpecified)
                                        data = _returnLeg.@return.dividendConditions.dividendPaymentDate.dividendDateReference.ToString();
                                #endregion return_dividendConditions_dividendPaymentDate_dividendDateReference
                                break;
                            case CciEnum.fxFeature_referenceCurrency:
                                #region fxFeature_referenceCurrency
                                if (_returnLeg.fxFeatureSpecified)
                                    data = _returnLeg.fxFeature.referenceCurrency.Value;
                                #endregion fxFeature_referenceCurrency
                                break;
                            case CciEnum.fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis:
                                #region fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                                if ((_returnLeg.fxFeatureSpecified) && (_returnLeg.fxFeature.fxFeatureQuantoSpecified))
                                    data = _returnLeg.fxFeature.fxFeatureQuanto.fxRate[0].quotedCurrencyPair.quoteBasis.ToString();
                                #endregion fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis
                                break;
                            case CciEnum.fxFeature_quanto_fxRate_rate:
                                #region fxFeature_quanto_fxRate_rate
                                if ((_returnLeg.fxFeatureSpecified) && (_returnLeg.fxFeature.fxFeatureQuantoSpecified))
                                    data = _returnLeg.fxFeature.fxFeatureQuanto.fxRate[0].rate.Value;
                                #endregion fxFeature_quanto_fxRate_rate
                                break;
                            default:
                                #region default
                                isSetting = false;
                                #endregion default
                                break;
                        }
                        if (isSetting)
                            ccis.InitializeCci(cci, sql_Table, data);
                    }
                }
                if (null != cciInitialPrice)
                    cciInitialPrice.Initialize_FromDocument();
                if (null != cciValuationPriceInterim)
                    cciValuationPriceInterim.Initialize_FromDocument();
                if (null != cciValuationPriceFinal)
                    cciValuationPriceFinal.Initialize_FromDocument();
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        #endregion IsClientId_PayerOrReceiver
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
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //		
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (key)
                {
                    #region Buyer/Seller: Calcul des BCs
                    case CciEnum.payer:
                        ccis.Synchronize(CciClientIdReceiver, pCci.newValue, pCci.lastValue);
                        break;
                    case CciEnum.receiver:
                        ccis.Synchronize(CciClientIdPayer, pCci.newValue, pCci.lastValue);
                        break;
                    #endregion
                    #region effectiveDate
                    case CciEnum.effectiveDate_relativeDate_periodMultiplier:
                        ccis.ProcessInitialize_DayType(CciClientId(CciEnum.effectiveDate_relativeDate_dayType), _returnLeg.effectiveDate.relativeDate.GetOffset);
                        break;
                    #endregion effectiveDate
                    #region terminationDate
                    case CciEnum.terminationDate_relativeDate_periodMultiplier:
                        ccis.ProcessInitialize_DayType(CciClientId(CciEnum.terminationDate_relativeDate_dayType), _returnLeg.terminationDate.relativeDate.GetOffset);
                        break;
                    #endregion terminationDate
                    #region underlyer_singleUnderlyer_underlyingAsset
                    case CciEnum.underlyer_singleUnderlyer_underlyingAsset:
                        string curr = string.Empty;
                        //
                        if (null != Cci(CciEnum.underlyer_singleUnderlyer_underlyingAsset).sql_Table)
                        {
                            // plutôt que lire sql_Table pour determiner la devise => lecture du doc fpml
                            curr = this.GetUnderlyingAssetCurrency;
                            Cci(CciReturnLeg.CciEnum.underlyer_singleUnderlyer_currency).newValue = curr;
                        }
                        //
                        if (StrFunc.IsFilled(curr))
                        {
                            ccis.SetNewValue(CciClientId(CciEnum.notional_notionalAmount_currency), curr);

                            if ((null != cciInitialPrice) && (cciInitialPrice.IsCommissionFixedAmount))
                                ccis.SetNewValue(cciInitialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.commission_currency), curr, true);
                            //Mise a jour Devise price
                            if ((null != cciInitialPrice) && (cciInitialPrice.IsPriceExpressionAbsoluteTerms))
                                ccis.SetNewValue(cciInitialPrice.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), curr, true);
                            //Mise a jour de cciValuationPriceInterim (commission)
                            if ((null != cciValuationPriceInterim) && (cciValuationPriceInterim.IsCommissionFixedAmount))
                                ccis.SetNewValue(cciValuationPriceInterim.CciClientId(CciReturnLegValuationPrice.CciEnum.commission_currency), curr, true);
                            //Mise a jour Devise price
                            if ((null != cciValuationPriceInterim) && (cciValuationPriceInterim.IsPriceExpressionAbsoluteTerms))
                                ccis.SetNewValue(cciValuationPriceInterim.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), curr, true);
                            //Mise a cciValuationPriceFinal (commission)
                            if ((null != cciValuationPriceFinal) && (cciValuationPriceFinal.IsCommissionFixedAmount))
                                ccis.SetNewValue(cciValuationPriceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.commission_currency), curr, true);
                            //Mise a jour Devise price
                            if ((null != cciValuationPriceFinal) && (cciValuationPriceFinal.IsPriceExpressionAbsoluteTerms))
                                ccis.SetNewValue(cciValuationPriceFinal.CciClientId(CciReturnLegValuationPrice.CciEnum.netPrice_currency), curr, true);
                        }
                        //
                        if (null != cciValuationPriceInterim)
                            cciValuationPriceInterim.Dump_PeriodicDatesBDA();
                        //
                        break;
                    #endregion
                    #region Currency: Arrondi du notional et Calcul des BCs
                    case CciEnum.notional_notionalAmount_amount:
                    case CciEnum.notional_notionalAmount_currency:
                        bool bFromAmount = (CciEnum.notional_notionalAmount_amount == key);
                        ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.notional_notionalAmount_amount), _returnLeg.notional.notionalAmount, bFromAmount);
                        break;
                    #endregion
                    #region default
                    default:

                        break;
                    #endregion default
                }
            }
            //
            if (null != cciInitialPrice)
                cciInitialPrice.ProcessInitialize(pCci);
            if (null != cciValuationPriceInterim)
                cciValuationPriceInterim.ProcessInitialize(pCci);
            if (null != cciValuationPriceFinal)
                cciValuationPriceFinal.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string pPrefix)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {

            if (null != cciInitialPrice)
                cciInitialPrice.SetDisplay(pCci);
            if (null != cciValuationPriceInterim)
                cciValuationPriceInterim.SetDisplay(pCci);
            if (null != cciValuationPriceFinal)
                cciValuationPriceFinal.SetDisplay(pCci);
            //
            if (IsCci(CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (_returnLeg.effectiveDate.adjustableDateSpecified)
                    ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLeg.effectiveDate.adjustableDate.dateAdjustments);
            }
            //
            if (IsCci(CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (_returnLeg.effectiveDate.adjustableDateSpecified)
                    ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLeg.terminationDate.adjustableDate.dateAdjustments);
            }
            //
            if (IsCci(CciEnum.effectiveDate_relativeDate_bDC, pCci))
            {
                if (_returnLeg.effectiveDate.relativeDateSpecified)
                    ccis.SetDisplayBusinessDayAdjustments(pCci, _returnLeg.effectiveDate.relativeDate.GetAdjustments);
            }
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #endregion Interfaces
        #region Methods
        #region IsClientId_QuoteBasis
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return IsCci(CciEnum.fxFeature_quanto_fxRate_quotedCurrencyPair_quoteBasis, pCci);
        }
        #endregion IsClientId_QuoteBasis
        #region DumpBDC
        private void DumpBDC(CciEnum pCciBdc)
        {
            string clientIdEquity = CciClientId(CciEnum.underlyer_singleUnderlyer_underlyingAsset);    // A reprendre si basket (non gere pur l'instant)
            string clientIdBdc = CciClientId(pCciBdc);
            CciBC cciBC = new CciBC(_cciTrade);
            string id = string.Empty;
            //
            cciBC.Add(clientIdEquity, CciBC.TypeReferentialInfo.Equity);
            //
            IBusinessDayAdjustments bda = null;
            switch (pCciBdc)
            {
                case CciEnum.effectiveDate_adjustableDate_dateAdjustments_bDC:
                    id = TradeCustomCaptureInfos.CCst.EFFECTIVE_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.terminationDate.adjustableDate.dateAdjustments;
                    break;
                case CciEnum.effectiveDate_relativeDate_bDC:
                    id = TradeCustomCaptureInfos.CCst.EFFECTIVE_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.effectiveDate.relativeDate.GetAdjustments;
                    break;
                case CciEnum.terminationDate_adjustableDate_dateAdjustments_bDC:
                    id = TradeCustomCaptureInfos.CCst.TERMINATION_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.terminationDate.adjustableDate.dateAdjustments;
                    break;
                case CciEnum.terminationDate_relativeDate_bDC:
                    id = TradeCustomCaptureInfos.CCst.TERMINATION_BUSINESS_CENTERS_REFERENCE;
                    bda = _returnLeg.terminationDate.relativeDate.GetAdjustments;
                    break;
            }
            //
            ccis.DumpBDC_ToDocument(bda, CciClientId(pCciBdc), CciClientId(id), cciBC);
        }
        #endregion DumpBDC
        #endregion Methods
    }
}

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Linq;


namespace EFS.TradeInformation
{

    public class CciProductEquityOption : CciProductEquityDerivativeLongFormBase
    {
        #region Members
        private CciEquityPremium _cciEquityPremium;
        #endregion
        
        #region accessor
        public IEquityOption EquityOption { get; private set; }
        #endregion
        
        #region public Enum
        public new enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference.hRef")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference.hRef")]
            seller,
            #endregion
            #region optionType
            [System.Xml.Serialization.XmlEnumAttribute("optionType")]
            optionType,
            #endregion
            #region equityEffectiveDate
            [System.Xml.Serialization.XmlEnumAttribute("equityEffectiveDate")]
            equityEffectiveDate,
            #endregion
            #region notional
            [System.Xml.Serialization.XmlEnumAttribute("notional.amount")]
            notional_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notional.currency")]
            notional_currency,
            #endregion

            #region methodOfAdjustment
            [System.Xml.Serialization.XmlEnumAttribute("methodOfAdjustment")]
            methodOfAdjustment,
            #endregion methodOfAdjustment

            #region strike
            [System.Xml.Serialization.XmlEnumAttribute("strike.strikePrice")]
            strike_strikePrice,
            [System.Xml.Serialization.XmlEnumAttribute("strike.strikePercentage")]
            strike_strikePercentage,
            [System.Xml.Serialization.XmlEnumAttribute("strike.strikeDeterminationDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            strike_strikeDeterminationDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("strike.strikeDeterminationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            strike_strikeDeterminationDate_adjustableDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("strike.currency")]
            strike_currency,
            #endregion
            #region spotPrice
            [System.Xml.Serialization.XmlEnumAttribute("spotPrice")]
            spotPrice,
            #endregion
            #region numberOfOptions
            [System.Xml.Serialization.XmlEnumAttribute("numberOfOptions")]
            numberOfOptions,
            #endregion
            #region optionEntitlement
            [System.Xml.Serialization.XmlEnumAttribute("optionEntitlement")]
            optionEntitlement,
            #endregion
            unknown,
        }
        #endregion Enum
        
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pEquityOption"></param>
        /// <param name="pPrefix"></param>
        public CciProductEquityOption(CciTrade pCciTrade, IEquityOption pEquityOption, string pPrefix)
            :this(pCciTrade,pEquityOption,pPrefix,-1)        
        {}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pEquityOption"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pNumber"></param>
        public CciProductEquityOption(CciTrade pCciTrade, IEquityOption pEquityOption, string pPrefix, int pNumber)
            : base(pCciTrade, (IEquityDerivativeLongFormBase)pEquityOption, pPrefix, pNumber)
        {
            
        }
        #endregion
        
        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, EquityOption);
            base.Initialize_FromCci();
            _cciEquityPremium.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
            _cciEquityPremium.AddCciSystem();
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
                    //
                    switch (cciEnum)
                    {
                        #region strike
                        case CciEnum.strike_strikePrice:
                            if (EquityOption.Strike.PriceSpecified)
                                data = EquityOption.Strike.Price.Value;
                            break;
                        case CciEnum.strike_strikePercentage:
                            if (EquityOption.Strike.PercentageSpecified)
                                data = EquityOption.Strike.Percentage.Value;
                            break;
                        case CciEnum.strike_strikeDeterminationDate_adjustableDate_unadjustedDate:
                            if (EquityOption.Strike.StrikeDeterminationDateSpecified
                                && EquityOption.Strike.StrikeDeterminationDate.AdjustableDateSpecified)
                                data = EquityOption.Strike.StrikeDeterminationDate.AdjustableDate.UnadjustedDate.Value;
                            break;
                        case CciEnum.strike_strikeDeterminationDate_adjustableDate_dateAdjustments_bDC:
                            if (EquityOption.Strike.StrikeDeterminationDateSpecified
                                && EquityOption.Strike.StrikeDeterminationDate.AdjustableDateSpecified)
                                data = EquityOption.Strike.StrikeDeterminationDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                            break;
                        case CciEnum.strike_currency:
                            if (EquityOption.Strike.CurrencySpecified)
                                data = EquityOption.Strike.Currency.Value;
                            break;
                        #endregion
                        #region spotPrice
                        case CciEnum.spotPrice:
                            if (EquityOption.SpotPriceSpecified)
                                data = EquityOption.SpotPrice.Value;
                            break;
                        #endregion
                        #region numberOfOptions
                        case CciEnum.numberOfOptions:
                            if (EquityOption.NumberOfOptionsSpecified)
                                data = EquityOption.NumberOfOptions.Value;
                            break;
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
            // 
            base.Initialize_FromDocument();
            //
            _cciEquityPremium.Initialize_FromDocument();
            //

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
                    //
                    switch (cciEnum)
                    {
                        #region strike
                        case CciEnum.strike_strikePrice:
                            EquityOption.Strike.PriceSpecified = cci.IsFilledValue;
                            EquityOption.Strike.PercentageSpecified = !cci.IsFilledValue;
                            if (EquityOption.Strike.PriceSpecified)
                                EquityOption.Strike.Price.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.strike_strikePercentage:
                            EquityOption.Strike.PercentageSpecified = cci.IsFilledValue;
                            EquityOption.Strike.PriceSpecified = !cci.IsFilledValue;
                            if (EquityOption.Strike.PercentageSpecified)
                                EquityOption.Strike.Percentage.Value = data;
                            break;
                        case CciEnum.strike_strikeDeterminationDate_adjustableDate_unadjustedDate:
                            EquityOption.Strike.StrikeDeterminationDateSpecified = cci.IsFilledValue;
                            EquityOption.Strike.StrikeDeterminationDate.AdjustableDateSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                EquityOption.Strike.StrikeDeterminationDate.AdjustableDate.UnadjustedDate.Value = data;
                            break;
                        case CciEnum.strike_strikeDeterminationDate_adjustableDate_dateAdjustments_bDC:
                            if (EquityOption.Strike.StrikeDeterminationDate.AdjustableDateSpecified)
                                EquityOption.Strike.StrikeDeterminationDate.AdjustableDate.DateAdjustments.BusinessDayConvention = StringToEnum.BusinessDayConvention(data);
                            break;
                        case CciEnum.strike_currency:
                            EquityOption.Strike.CurrencySpecified = cci.IsFilledValue;
                            if (EquityOption.Strike.CurrencySpecified)
                                EquityOption.Strike.Currency.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion
                        #region spotPrice
                        case CciEnum.spotPrice:
                            EquityOption.SpotPriceSpecified = cci.IsFilledValue;
                            if (EquityOption.SpotPriceSpecified)
                                EquityOption.SpotPrice.Value = data;
                            break;
                        #endregion
                        #region numberOfOptions
                        case CciEnum.numberOfOptions:
                            EquityOption.NumberOfOptionsSpecified = cci.IsFilledValue;
                            if (EquityOption.NumberOfOptionsSpecified)
                                EquityOption.NumberOfOptions.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
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
            //
            base.Dump_ToDocument();
            //
            _cciEquityPremium.Dump_ToDocument();

        }
        #endregion Dump_ToDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
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
                    case CciEnum.strike_strikePrice:
                    case CciEnum.numberOfOptions:
                        CcisBase.SetNewValue(Cci(CciEnum.notional_amount).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(RetCalcNotional()), false);
                        break;
                    case CciEnum.strike_currency:
                        CcisBase.SetNewValue(Cci(CciEnum.notional_currency).ClientId_WithoutPrefix, pCci.NewValue, false);
                        break;

                    #region Default
                    default:
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        break;
                        #endregion Default
                }
            }
            //
            base.ProcessInitialize(pCci);
            //
            _cciEquityPremium.ProcessInitialize(pCci);
            //
            if (CciSingleUnderlyer.IsCci(CciSingleUnderlyer.CciEnum.equity, pCci))
            {
                CcisBase.SetNewValue(Cci(CciEnum.strike_currency).ClientId_WithoutPrefix, CciTrade.GetMainCurrency, false);
                CcisBase.SetNewValue(CciEquityExerciseValuationSettlement.Cci(CciEquityExerciceValuationSettlement.CciEnum.settlementCurrency).ClientId_WithoutPrefix, CciTrade.GetMainCurrency, false);
                CcisBase.SetNewValue(_cciEquityPremium.Cci(CciEquityPremium.CciEnum.pricePerOption_currency).ClientId_WithoutPrefix, CciTrade.GetMainCurrency, false);
            }
            //
            if (_cciEquityPremium.IsCci(CciEquityPremium.CciEnum.pricePerOption_amount, pCci) || IsCci(CciEnum.numberOfOptions, pCci))
            {
                if ((EquityOption.EquityPremium.PricePerOptionSpecified))
                {
                    decimal nbOption = 1;
                    if (EquityOption.NumberOfOptionsSpecified)
                        nbOption = EquityOption.NumberOfOptions.DecValue;
                    decimal premiumAmount = nbOption * EquityOption.EquityPremium.PricePerOption.Amount.DecValue;
                    CcisBase.SetNewValue(_cciEquityPremium.Cci(CciEquityPremium.CciEnum.paymentAmount_amount).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(premiumAmount), false);
                }
            }

        }
        #endregion ProcessInitialize
        #region public override CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
            _cciEquityPremium.CleanUp();
        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            _cciEquityPremium.SetDisplay(pCci);
        }
        #endregion
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            _cciEquityPremium.RefreshCciEnabled();
        }
        #endregion
        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            _cciEquityPremium.Initialize_Document();
        }
        #endregion Initialize_Document
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = _cciEquityPremium.IsClientId_PayerOrReceiver(pCci);
            if (!isOk)
                isOk = base.IsClientId_PayerOrReceiver(pCci);
            return isOk;
        }
        #endregion
        #endregion
        
        #region IContainerCciPayerReceiver Membres
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            _cciEquityPremium.SynchronizePayerReceiver(pLastValue, pNewValue);
            base.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion
        
        #region Methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            EquityOption = (IEquityOption)pProduct;
            IEquityPremium premium = null;
            if (null != EquityOption)
                premium = EquityOption.EquityPremium;
            _cciEquityPremium = new CciEquityPremium(CciTrade, premium, Prefix + TradeCustomCaptureInfos.CCst.Prefix_equityPremium);
            base.SetProduct(pProduct);
        }
        #endregion
        //
        #region private RetCalcNotional
        private decimal RetCalcNotional()
        {
            decimal ret = decimal.Zero;
            decimal numberOfOptions = 1;
            if (EquityOption.NumberOfOptionsSpecified)
                numberOfOptions = EquityOption.NumberOfOptions.DecValue;

            decimal optionEntitlement = 1;
            if (null != EquityOption.OptionEntitlement)
                optionEntitlement = EquityOption.OptionEntitlement.DecValue;

            decimal strike = decimal.Zero;
            if (EquityOption.Strike.PriceSpecified)
                strike = EquityOption.Strike.Price.DecValue;
            if (strike > 0)
                ret = numberOfOptions * optionEntitlement * strike;
            return ret;
        }
        #endregion
        #endregion

    }

}

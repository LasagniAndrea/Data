using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Reflection;



namespace EFS.TradeInformation
{
    internal class CciEquityOption : CciEquityDerivativeLongFormBase, IContainerCciFactory, IContainerCci, IContainerCciPayerReceiver
    {
        #region Members
        private readonly IEquityOption _equityOption;
        private readonly CciEquityPremium cciEquityPremium;
        #endregion

        #region accessor
        public IEquityOption EquityOption
        {
            get { return _equityOption; }
        }
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
        public CciEquityOption(CciTrade pCciTrade, IEquityOption pEquityOption, string pPrefix)
            : base(pCciTrade, (IEquityDerivativeLongFormBase)pEquityOption, pPrefix)
        {
            _equityOption = pEquityOption;
            cciEquityPremium = new CciEquityPremium(cciTrade, pEquityOption.EquityPremium, prefix + TradeCustomCaptureInfos.CCst.Prefix_equityPremium);
        }
        #endregion

        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, EquityOption);
            base.Initialize_FromCci();
            cciEquityPremium.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
            cciEquityPremium.AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        public override void Initialize_FromDocument()
        {
            string data;
            bool isSetting;
            SQL_Table sql_Table;
            string cliendId_Key;

            foreach (CustomCaptureInfo cci in Ccis)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    cliendId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    data = string.Empty;
                    isSetting = true;
                    sql_Table = null;
                    #endregion
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    //
                    switch (key)
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
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
            
            base.Initialize_FromDocument();
            
            cciEquityPremium.Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region public override Dump_ToDocument
        public override void Dump_ToDocument()
        {
            try
            {
                bool isSetting = false;
                string data;
                string clientId_Key;
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;

                foreach (CustomCaptureInfo cci in Ccis)
                {
                    //On ne traite que les contrôle dont le contenu à changé
                    if ((cci.HasChanged) && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        #region Reset variables
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                        data = cci.NewValue;
                        isSetting = true;
                        #endregion Reset variables
                        //
                        CciEnum key = CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                            key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                        //
                        switch (key)
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
                            Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
                //
                base.Dump_ToDocument();
                //
                cciEquityPremium.Dump_ToDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            try
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
                            Ccis.SetNewValue(Cci(CciEnum.notional_amount).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(RetCalcNotional()), false);
                            break;
                        case CciEnum.strike_currency:
                            Ccis.SetNewValue(Cci(CciEnum.notional_currency).ClientId_WithoutPrefix, pCci.NewValue, false);
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
                cciEquityPremium.ProcessInitialize(pCci);
                //
                if (cciSingleUnderlyer.IsCci(CciSingleUnderlyer.CciEnum.equity, pCci))
                {
                    Ccis.SetNewValue(Cci(CciEnum.strike_currency).ClientId_WithoutPrefix, cciTrade.GetMainCurrency, false);
                    Ccis.SetNewValue(cciEquityExerciseValuationSettlement.Cci(CciEquityExerciceValuationSettlement.CciEnum.settlementCurrency).ClientId_WithoutPrefix, cciTrade.GetMainCurrency, false);
                    Ccis.SetNewValue(cciEquityPremium.Cci(CciEquityPremium.CciEnum.pricePerOption_currency).ClientId_WithoutPrefix, cciTrade.GetMainCurrency, false);
                }
                //
                if (cciEquityPremium.IsCci(CciEquityPremium.CciEnum.pricePerOption_amount, pCci) || IsCci(CciEnum.numberOfOptions,pCci ) )
                {
                    if ((EquityOption.EquityPremium.PricePerOptionSpecified))
                    {
                        decimal premiumAmount = decimal.Zero;
                        decimal nbOption = 1;
                        //
                        if (EquityOption.NumberOfOptionsSpecified)
                            nbOption = EquityOption.NumberOfOptions.DecValue;
                        premiumAmount = nbOption * EquityOption.EquityPremium.PricePerOption.Amount.DecValue;
                        //
                        Ccis.SetNewValue(cciEquityPremium.Cci(CciEquityPremium.CciEnum.paymentAmount_amount).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(premiumAmount),false);
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public new void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public new void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region public override CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
            cciEquityPremium.CleanUp();
        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            cciEquityPremium.SetDisplay(pCci);
        }
        #endregion
        #region public override RemoveLastItemInArray
        public override void RemoveLastItemInArray(string pPrefix)
        {
            base.RemoveLastItemInArray(pPrefix);
            cciEquityPremium.RemoveLastItemInArray(pPrefix);
        }
        #endregion
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            cciEquityPremium.RefreshCciEnabled();
        }
        #endregion
        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            cciEquityPremium.Initialize_Document();
        }
        #endregion Initialize_Document
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = cciEquityPremium.IsClientId_PayerOrReceiver(pCci);
            if (!isOk)
                isOk = base.IsClientId_PayerOrReceiver(pCci);
            return isOk;
        }
        #endregion


        #region public int GetArrayElementDocumentCount
        public int GetArrayElementDocumentCount(string _)
        {
            return -1;
        }
        #endregion
        #endregion

        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public new string CciClientId(string pClientId_Key)
        { 
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public  CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public new CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region IsCciOfContainer
        public new bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            bool isOk = Ccis.Contains(pClientId_WithoutPrefix);
            return isOk && (pClientId_WithoutPrefix.StartsWith(prefix));
        }
        #endregion
        #region CciContainerKe
        public new string  CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion

        #region IContainerCciPayerReceiver Membres
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            cciEquityPremium.SynchronizePayerReceiver(pLastValue, pNewValue);
            base.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion

        #region private RetCalcNotional
        private decimal RetCalcNotional()
        {
            try
            {
                decimal ret = decimal.Zero;
                //
                decimal numberOfOptions = 1;
                if (EquityOption.NumberOfOptionsSpecified)
                    numberOfOptions = EquityOption.NumberOfOptions.DecValue;
                //
                decimal optionEntitlement = 1;
                if (null != EquityOption.OptionEntitlement)
                    optionEntitlement = EquityOption.OptionEntitlement.DecValue;
                //
                decimal strike = decimal.Zero;
                if (EquityOption.Strike.PriceSpecified)
                    strike = EquityOption.Strike.Price.DecValue;
                //
                if (strike > 0)
                    ret = numberOfOptions * optionEntitlement * strike;
                //
                return ret;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion

    }



    

}

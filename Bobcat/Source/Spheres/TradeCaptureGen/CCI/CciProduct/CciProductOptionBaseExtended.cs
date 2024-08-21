using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;


namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>

    public abstract class CciProductOptionBaseExtended : CciProductOptionBase
    {
        private readonly string expirationDateId = "expirationDate";
        private readonly string expirationBusinessCentersId = "expirationBusinessCenter";

        #region Enums
        #region CciEnum
        public new enum CciEnum
        {
            #region OptionDenomination
            [System.Xml.Serialization.XmlEnumAttribute("optionEntitlement")]
            optionEntitlement,
            [System.Xml.Serialization.XmlEnumAttribute("entitlementCurrency")]
            entitlementCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("numberOfOptions")]
            numberOfOptions,
            #endregion OptionDenomination

            #region notional
            [System.Xml.Serialization.XmlEnumAttribute("notionalAmount.amount")]
            notionalAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notionalAmount.currency")]
            notionalAmount_currency,
            #endregion

            #region exerciseProcedure
            [System.Xml.Serialization.XmlEnumAttribute("exerciseProcedure")]
            exerciseProcedure,
            #endregion exerciseProcedure

            #region Settlement
            [System.Xml.Serialization.XmlEnumAttribute("settlementType")]
            settlementType,
            [System.Xml.Serialization.XmlEnumAttribute("settlementDate")]
            settlementDate,
            #endregion Settlement

            #region Feature
            // Bouton feature
            [System.Xml.Serialization.XmlEnumAttribute("featureSpecified")]
            feature_specified,
            [System.Xml.Serialization.XmlEnumAttribute("feature.asian")]
            feature_asian,
            [System.Xml.Serialization.XmlEnumAttribute("feature.barrier")]
            feature_barrier,
            [System.Xml.Serialization.XmlEnumAttribute("feature.knock")]
            feature_knock,
            [System.Xml.Serialization.XmlEnumAttribute("feature.fxFeature")]
            feature_fx,
            #endregion Feature

            unknown,
        }

        #endregion CciEnum

        #endregion Enums
        
        #region Members
        private IOptionBaseExtended _optionBaseExtended;
        private CciPremium _cciPremium;
        private CciExercise _cciExercise;
        #endregion members
        
        #region Constructors
        public CciProductOptionBaseExtended(CciTrade pCciTrade, IOptionBaseExtended pOptionBaseExtended, string pPrefix, int pNumber)
            : base(pCciTrade, (IOptionBase)pOptionBaseExtended, pPrefix, pNumber)
        {
            
        }
        #endregion Constructors
        
        #region Interfaces
        #region IContainerCciFactory Members
        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.numberOfOptions), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DSP + CciClientId(CciEnum.optionEntitlement), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.entitlementCurrency), false, TypeData.TypeDataEnum.@string);

            _cciPremium.AddCciSystem();
            _cciExercise.AddCciSystem();

            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.exerciseProcedure), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.settlementDate), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_asian), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_barrier), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_knock), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_fx), false, TypeData.TypeDataEnum.@string);
        }
        #endregion AddCciSystem
        #region public override CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
            _cciPremium.CleanUp();

            #region Exercise
            if (_cciExercise != null)
            {
                _cciExercise.CleanUp();
                _optionBaseExtended.EFS_Exercise = _cciExercise.Exercise;
            }
            _optionBaseExtended.AmericanExerciseSpecified = (CciExercise.ExerciseTypeEnum.american == _cciExercise.ExerciseType);
            _optionBaseExtended.BermudaExerciseSpecified = (CciExercise.ExerciseTypeEnum.bermuda == _cciExercise.ExerciseType);
            _optionBaseExtended.EuropeanExerciseSpecified = (CciExercise.ExerciseTypeEnum.european == _cciExercise.ExerciseType);
            #endregion Exercise
        }
        #endregion CleanUp
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
                        case CciEnum.numberOfOptions:
                            #region numberOfOptions
                            _optionBaseExtended.NumberOfOptionsSpecified = cci.IsFilledValue;
                            if (_optionBaseExtended.NumberOfOptionsSpecified)
                                _optionBaseExtended.NumberOfOptions.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion numberOfOptions
                            break;

                        case CciEnum.optionEntitlement:
                            #region OptionEntitlement
                            _optionBaseExtended.OptionEntitlement = new EFS_Decimal(data);
                            #endregion OptionEntitlement
                            break;

                        case CciEnum.entitlementCurrency:
                            #region OptionEntitlement
                            _optionBaseExtended.EntitlementCurrencySpecified = cci.IsFilledValue;
                            if (_optionBaseExtended.EntitlementCurrencySpecified)
                                _optionBaseExtended.EntitlementCurrency.Value = data;

                            #endregion OptionEntitlement
                            break;

                        case CciEnum.notionalAmount_amount:
                            #region notionalAmount
                            _optionBaseExtended.NotionalAmountReferenceSpecified = false;
                            _optionBaseExtended.NotionalAmountSpecified = cci.IsFilledValue;
                            if (_optionBaseExtended.NotionalAmountSpecified)
                                _optionBaseExtended.NotionalAmount.Amount = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion notionalAmount
                            break;

                        case CciEnum.notionalAmount_currency:
                            #region notionalAmountCurrency
                            _optionBaseExtended.NotionalAmount.Currency = data;
                            #endregion notionalAmountCurrency
                            break;

                        case CciEnum.settlementType:
                            #region SettlementType
                            _optionBaseExtended.SettlementTypeSpecified = cci.IsFilledValue;
                            if (_optionBaseExtended.SettlementTypeSpecified)
                                _optionBaseExtended.SettlementType = (SettlementTypeEnum)Enum.Parse(typeof(SettlementTypeEnum), data);
                            #endregion SettlementType
                            break;

                        case CciEnum.feature_specified:
                            #region featureSpecified
                            _optionBaseExtended.FeatureSpecified = cci.IsFilledValue;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion featureSpecified
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
            base.Dump_ToDocument();
            _cciPremium.Dump_ToDocument();
            if (_cciExercise != null)//PL 20150720 Add if()
            {
                _cciExercise.Dump_ToDocument();

                _optionBaseExtended.AmericanExerciseSpecified = (CciExercise.ExerciseTypeEnum.american == _cciExercise.ExerciseType);
                _optionBaseExtended.BermudaExerciseSpecified = (CciExercise.ExerciseTypeEnum.bermuda == _cciExercise.ExerciseType);
                _optionBaseExtended.EuropeanExerciseSpecified = (CciExercise.ExerciseTypeEnum.european == _cciExercise.ExerciseType);
            }

        }
        #endregion Dump_ToDocument
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            Control control = pPage.FindControl(Prefix + "tblOptionFeature");
            if (null != control)
                control.Visible = _optionBaseExtended.FeatureSpecified;

            base.DumpSpecific_ToGUI(pPage);
        }

        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            _cciPremium.Initialize_Document();
            _cciExercise.Initialize_Document();
        }
        #endregion Initialize_Document

        #region Membres de IContainerCciGetInfoButton
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
            bool isOk = false;
            if (null != _cciExercise)
                isOk = _cciExercise.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);

            if ((false == isOk) && this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.feature_asian, pCci);
                if (isOk)
                {
                    pCo.Element = "asian";
                    pCo.Object = "feature";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = _optionBaseExtended.FeatureSpecified && _optionBaseExtended.Feature.AsianSpecified;
                    pIsEnabled = _optionBaseExtended.FeatureSpecified;
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.feature_barrier, pCci);
                    if (isOk)
                    {
                        pCo.Element = "barrier";
                        pCo.Object = "feature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _optionBaseExtended.FeatureSpecified && _optionBaseExtended.Feature.BarrierSpecified;
                        pIsEnabled = _optionBaseExtended.FeatureSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.feature_fx, pCci);
                    if (isOk)
                    {
                        pCo.Element = "fxFeature";
                        pCo.Object = "feature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _optionBaseExtended.FeatureSpecified && _optionBaseExtended.Feature.FxFeatureSpecified;
                        pIsEnabled = _optionBaseExtended.FeatureSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.feature_knock, pCci);
                    if (isOk)
                    {
                        pCo.Element = "knock";
                        pCo.Object = "feature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _optionBaseExtended.FeatureSpecified && _optionBaseExtended.Feature.KnockSpecified;
                        pIsEnabled = _optionBaseExtended.FeatureSpecified;
                    }
                }


                if (!isOk)
                {
                    isOk = _cciExercise.IsCci(CciExercise.CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_unadjustedDate, pCci);
                    if (isOk)
                    {
                        pCo.Object = "";
                        pCo.Element = "bermudaExerciseDates";
                        pIsSpecified = (_optionBaseExtended.BermudaExerciseSpecified);
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.settlementDate, pCci);
                    if (isOk)
                    {
                        pCo.Object = "product";
                        pCo.Element = "settlementDate";
                        pIsSpecified = true;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.exerciseProcedure, pCci);
                    if (isOk)
                    {
                        pCo.Object = "product";
                        pCo.Element = "procedure";
                        pIsSpecified = true;
                        pIsEnabled = true;
                    }
                }

                /*
                if (!isOk)
                {
                    isOk = this.IsCci(CciEnum.calculationAgent, pCci);
                    if (isOk)
                    {
                        pCo.Object = "product";
                        pCo.Element = "calculationAgent";
                        pIsSpecified = _swaption.calculationAgentSpecified;
                        pIsEnabled = true;
                    }
                }
                */

                /*
                if (!isOk)
                {
                    isOk = this.IsCci(CciEnum.cashSettlement, pCci);
                    if (isOk)
                    {
                        pCo.Object = "product";
                        pCo.Element = "cashSettlement";
                        pIsSpecified = _swaption.cashSettlementSpecified;
                        pIsEnabled = true;
                    }
                }
                */
            }
            return isOk;

        }
        #endregion Membres de IContainerCciGetInfoButton

        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = _cciPremium.IsClientId_PayerOrReceiver(pCci);
            if (!isOk)
                isOk = base.IsClientId_PayerOrReceiver(pCci);
            return isOk;
        }
        #endregion
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, (IOptionBaseExtended)_optionBaseExtended);
            base.Initialize_FromCci();
            _cciPremium.Initialize_FromCci();
            InitializeExercise_FromCci();

            if ((null == _optionBaseExtended.ExerciseProcedure))
                _optionBaseExtended.ExerciseProcedure = CciTrade.CurrentTrade.Product.ProductBase.CreateExerciseProcedure();

            if ((null == _optionBaseExtended.SettlementDate))
            {
                _optionBaseExtended.SettlementDateSpecified = true;
                _optionBaseExtended.SettlementDate.AdjustableDateSpecified = false;
                _optionBaseExtended.SettlementDate.RelativeDateSpecified = true;
                _optionBaseExtended.SettlementDate.RelativeDate = CciTrade.CurrentTrade.Product.ProductBase.CreateRelativeDateOffset();
                IRelativeDateOffset settlementDate = _optionBaseExtended.SettlementDate.RelativeDate;
                settlementDate.Period = PeriodEnum.D;
                settlementDate.PeriodMultiplier = new EFS_Integer(0);
                settlementDate.BusinessCentersNoneSpecified = false;
                settlementDate.BusinessCentersReferenceSpecified = true;
                settlementDate.BusinessCentersDefineSpecified = false;
                settlementDate.BusinessCentersReference = CciTrade.CurrentTrade.Product.ProductBase.CreateBusinessCentersReference(expirationBusinessCentersId);
                settlementDate.BusinessDayConvention = BusinessDayConventionEnum.FOLLOWING;
                settlementDate.DayTypeSpecified = true;
                settlementDate.DayType = DayTypeEnum.Business;
                settlementDate.DateRelativeToValue = expirationDateId;
            }

            if (false == _optionBaseExtended.FeatureSpecified)
                _optionBaseExtended.Feature = _optionBaseExtended.CreateOptionFeature;

        }
        #endregion Initialize_FromCci
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.numberOfOptions:
                            if (_optionBaseExtended.NumberOfOptionsSpecified)
                                data = _optionBaseExtended.NumberOfOptions.Value;
                            break;

                        case CciEnum.notionalAmount_amount:
                            if (_optionBaseExtended.NotionalAmountSpecified)
                                data = _optionBaseExtended.NotionalAmount.Amount.Value;
                            break;

                        case CciEnum.notionalAmount_currency:
                            if (_optionBaseExtended.NotionalAmountSpecified)
                                data = _optionBaseExtended.NotionalAmount.Currency;
                            break;

                        case CciEnum.settlementType:
                            if (_optionBaseExtended.SettlementTypeSpecified)
                                data = _optionBaseExtended.SettlementType.ToString();
                            break;

                        case CciEnum.feature_specified:
                            #region featureSpecified
                            data = _optionBaseExtended.FeatureSpecified.ToString().ToLower();
                            #endregion featureSpecified
                            break;

                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            base.Initialize_FromDocument();
            _cciPremium.Initialize_FromDocument();
            _cciExercise.Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                switch (key)
                {
                    case CciEnum.numberOfOptions:
                        CcisBase.SetNewValue(Cci(CciEnum.optionEntitlement).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(CalculOptionEntitlement()), false);
                        //ccis.SetNewValue(_cciPremium.Cci(CciPremium.CciEnum.paymentAmount_amount).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(CalculOptionPremiumAmount()), false);
                        break;
                    case CciEnum.notionalAmount_amount:
                        if (StrFunc.IsFilled(pCci.NewValue))
                        {
                            pCci.ErrorMsg = string.Empty;
                            // EG 20160404 Migration vs2013
                            //ISecurityAsset securityAsset = null;
                            IDebtSecurityOption _debtSecurityOption = _optionBaseExtended as IDebtSecurityOption;
                            if (_debtSecurityOption.SecurityAssetSpecified &&
                                _debtSecurityOption.SecurityAsset.DebtSecuritySpecified)
                            {
                                decimal notionalAmount = 0;
                                if (DecFunc.IsDecimal(pCci.NewValue))
                                    notionalAmount = DecFunc.DecValue(pCci.NewValue);

                                if (0 < notionalAmount)
                                {
                                    // EG 20150608 [21091] parValue au lieu de faceAmount
                                    if (_debtSecurityOption.ParValueSpecified &&
                                        _debtSecurityOption.DebtSecurity.Security.FaceAmountSpecified)
                                    {
                                        if (0 < _debtSecurityOption.ParValue.DecValue)
                                        {
                                            // le NotionalAmount n'est pas un multiple du ParValue
                                            if (0 != decimal.Remainder(notionalAmount, _debtSecurityOption.ParValue.DecValue))
                                            {
                                                // FI 20190520 [XXXXX] usage de Value plutot que decValue pour ne pas perdre des decimales
                                                string sNom = StrFunc.FmtDecimalToCurrentCulture(_debtSecurityOption.ParValue.Value) + Cst.Space +
                                                    _debtSecurityOption.DebtSecurity.Security.FaceAmount.Currency;
                                                pCci.ErrorMsg = Ressource.GetString2("Msg_NotNominalMultiple", sNom);
                                                CcisBase.SetNewValue(Cci(CciEnum.optionEntitlement).ClientId_WithoutPrefix, string.Empty, false);
                                            }
                                            else
                                            {
                                                // FI 20190520 [XXXXX] Usage de  .ToString(NumberFormatInfo.InvariantInfo)
                                                decimal optionEntitlement = CalculOptionEntitlement();
                                                CcisBase.SetNewValue(Cci(CciEnum.optionEntitlement).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(optionEntitlement.ToString(NumberFormatInfo.InvariantInfo)), false);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    pCci.ErrorMsg = Ressource.GetString("Msg_NotPositiveAmount");
                                }
                            }
                        }
                        break;
                    #region Default
                    default:
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        break;
                        #endregion Default
                }
            }
            base.ProcessInitialize(pCci);

            //ccis.SetNewValue(_cciPremium.Cci(CciPremium.CciEnum.pricePerOption_currency).ClientId_WithoutPrefix, cciTrade.GetMainCurrency, false);
            //ccis.SetNewValue(_cciPremium.Cci(CciPremium.CciEnum.pricePerOption_currency).ClientId_WithoutPrefix, cciTrade.GetMainCurrency, false);
            _cciPremium.ProcessInitialize(pCci);

            PremiumAmountInitialize(pCci);
            /*
            if (_cciPremium.IsCci(CciPremium.CciEnum.pricePerOption_amount, pCci) || IsCci(CciEnum.numberOfOptions, pCci))
            {
                if (_optionBaseExtended.premiumSpecified && _optionBaseExtended.premium.pricePerOptionSpecified)
                {
                    decimal premiumAmount = decimal.Zero;
                    decimal nbOption = 1;
                    if (_optionBaseExtended.numberOfOptionsSpecified)
                        nbOption = _optionBaseExtended.numberOfOptions.DecValue;
                    premiumAmount = nbOption * _optionBaseExtended.premium.pricePerOption.amount.DecValue * _optionBaseExtended.optionEntitlement.DecValue;
                    ccis.SetNewValue(_cciPremium.Cci(CciPremium.CciEnum.paymentAmount_amount).ClientId_WithoutPrefix, StrFunc.FmtDecimalToInvariantCulture(premiumAmount), false);
                }
            }
            */
            _cciExercise.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
        #region PremiumAmountInitialize
        private void PremiumAmountInitialize(CustomCaptureInfo pCci)
        {
            if (_cciPremium.IsCci(CciPremium.CciEnum.valuationType, pCci))
                _cciPremium.SetCciPremiumValuationAmount(pCci);
            else if (_cciPremium.IsCci(CciPremium.CciEnum.valuationAmount, pCci) || IsCci(CciEnum.numberOfOptions, pCci) || IsCci(CciEnum.notionalAmount_amount, pCci))
            {
                CustomCaptureInfo cciValuationAmount = _cciPremium.Cci(CciPremium.CciEnum.valuationAmount);
                PremiumAmountCalculation(cciValuationAmount);
            }

        }
        #endregion PremiumAmountInitialize;

        #region PremiumAmountCalculation
        private void PremiumAmountCalculation(CustomCaptureInfo pCci)
        {
            decimal numberOfOptions = _optionBaseExtended.NumberOfOptions.DecValue;
            IMoney nominal = null;
            if (_optionBaseExtended.NotionalAmountSpecified)
                nominal = _optionBaseExtended.NotionalAmount;

            CustomCaptureInfo cciPaymentAmount = _cciPremium.Cci(CciPremium.CciEnum.paymentAmount_amount);
            CustomCaptureInfo cciPaymentAmountCurrency = _cciPremium.Cci(CciPremium.CciEnum.paymentAmount_currency);
            CustomCaptureInfo cciPercentageOfNotional = _cciPremium.Cci(CciPremium.CciEnum.percentageOfNotional);
            CustomCaptureInfo cciPricePerOptionAmount = _cciPremium.Cci(CciPremium.CciEnum.pricePerOption_amount);
            CustomCaptureInfo cciPricePerOptionCurrency = _cciPremium.Cci(CciPremium.CciEnum.pricePerOption_currency);

            decimal premiumAmount = 0;
            switch (_optionBaseExtended.Premium.ValuationType)
            {
                case PremiumAmountValuationTypeEnum.Cash:
                    #region Cash
                    if (DecFunc.IsDecimal(pCci.NewValue))
                        premiumAmount = DecFunc.DecValue(pCci.NewValue);

                    if (premiumAmount > 0)
                    {
                        cciPaymentAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(premiumAmount);
                        cciPaymentAmountCurrency.NewValue = nominal.Currency;
                        cciPercentageOfNotional.NewValue = string.Empty;
                        cciPricePerOptionAmount.NewValue = string.Empty;
                        // EG 20150609 [21091] Set to nominal.currency
                        cciPricePerOptionCurrency.NewValue = nominal.Currency;
                    }
                    else
                        cciPaymentAmount.ErrorMsg = Ressource.GetString("Msg_ErrorPremiumAmount_Cash");
                    #endregion Cash

                    break;
                case PremiumAmountValuationTypeEnum.PercentageOfNotional:
                    #region PercentageOfNotional
                    decimal percentageOfNotional = 0;
                    if (DecFunc.IsDecimal(pCci.NewValue))
                        percentageOfNotional = DecFunc.DecValue(pCci.NewValue);
                    premiumAmount = numberOfOptions * percentageOfNotional * nominal.Amount.DecValue;
                    if (premiumAmount > 0)
                    {
                        cciPaymentAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(premiumAmount);
                        cciPaymentAmountCurrency.NewValue = nominal.Currency;
                        cciPercentageOfNotional.NewValue = StrFunc.FmtDecimalToInvariantCulture(percentageOfNotional);
                        cciPricePerOptionAmount.NewValue = string.Empty;
                        // EG 20150609 [21091] Set to nominal.currency
                        cciPricePerOptionCurrency.NewValue = nominal.Currency;
                    }
                    else
                        cciPaymentAmount.ErrorMsg = Ressource.GetString("Msg_ErrorPremiumAmount_PercentageOfNotional");
                    #endregion PercentageOfNotional
                    break;
                case PremiumAmountValuationTypeEnum.PricePerOption:
                    #region PricePerOption
                    decimal pricePerOption = 0;
                    if (DecFunc.IsDecimal(pCci.NewValue))
                        pricePerOption = DecFunc.DecValue(pCci.NewValue);

                    premiumAmount = pricePerOption * numberOfOptions;
                    if (0 < premiumAmount)
                    {
                        cciPaymentAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(premiumAmount);
                        cciPaymentAmountCurrency.NewValue = nominal.Currency;
                        cciPercentageOfNotional.NewValue = string.Empty;
                        cciPricePerOptionAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(pricePerOption);
                        cciPricePerOptionCurrency.NewValue = nominal.Currency;
                    }
                    else if (0 < pricePerOption)
                        cciPaymentAmount.ErrorMsg = Ressource.GetString("Msg_ErrorPremiumAmount_PricePerOption");
                    #endregion PricePerOption
                    break;
            }
        }
        #endregion PremiumAmountCalculation
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            _cciPremium.RefreshCciEnabled();
            if (_cciExercise != null)//PL 20150720 Add if()
                _cciExercise.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.numberOfOptions, pCci))
            {
                pCci.Display = string.Empty;
                if ((null != _optionBaseExtended.EntitlementCurrency) && (null != _optionBaseExtended.OptionEntitlement))
                {
                    pCci.Display = _optionBaseExtended.EntitlementCurrency.Value + " " + _optionBaseExtended.OptionEntitlement.CultureValue +
                        " of nominal amount of the bonds per option.";
                }
            }
            base.SetDisplay(pCci);
            _cciPremium.SetDisplay(pCci);
            _cciExercise.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #endregion Interfaces
        //
        #region methods
        #region IContainerCciPayerReceiver Membres
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            _cciPremium.SynchronizePayerReceiver(pLastValue, pNewValue);
            base.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            _optionBaseExtended = (IOptionBaseExtended)pProduct;
            _cciPremium = new CciPremium(CciTrade, _optionBaseExtended.Premium, Prefix + TradeCustomCaptureInfos.CCst.Prefix_premium);
            base.SetProduct(pProduct);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private void InitializeExercise_FromCci()
        {

            bool saveAmericanSpecified = _optionBaseExtended.AmericanExerciseSpecified;
            bool saveBermudaSpecified = _optionBaseExtended.BermudaExerciseSpecified;
            bool saveEuropeanSpecified = _optionBaseExtended.EuropeanExerciseSpecified;
            //
            foreach (string exerciseType in Enum.GetNames(typeof(CciExercise.ExerciseTypeEnum)))
            {
                _cciExercise = new CciExercise(CciTrade, Prefix + exerciseType + "Exercise");
                if (((exerciseType == CciExercise.ExerciseTypeEnum.american.ToString()) &&
                    CcisBase.Contains(_cciExercise.CciClientId(CciExercise.CciEnumExercise.expirationTime_hourMinuteTime))) ||
                    ((exerciseType == CciExercise.ExerciseTypeEnum.bermuda.ToString()) &&
                    CcisBase.Contains(_cciExercise.CciClientId(CciExercise.CciEnumExercise.expirationTime_hourMinuteTime))) ||
                    ((exerciseType == CciExercise.ExerciseTypeEnum.european.ToString()) &&
                    CcisBase.Contains(_cciExercise.CciClientId(CciExercise.CciEnumExercise.expirationTime_hourMinuteTime))))
                {
                    _cciExercise.ExerciseType = (CciExercise.ExerciseTypeEnum)Enum.Parse(typeof(CciExercise.ExerciseTypeEnum), exerciseType);
                    _cciExercise.SetExercise(_optionBaseExtended as IProduct);
                    break;
                }
            }
            _cciExercise.Initialize_FromCci();
            _optionBaseExtended.AmericanExerciseSpecified = saveAmericanSpecified;
            _optionBaseExtended.BermudaExerciseSpecified = saveBermudaSpecified;
            _optionBaseExtended.EuropeanExerciseSpecified = saveEuropeanSpecified;
        }


        #region CalculOptionEntitlement
        protected virtual decimal CalculOptionEntitlement()
        {
            return 0;
        }
        #endregion CalculOptionEntitlement

        #endregion
    }

}

#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Interface;
#endregion Using Directives

namespace EfsML.EventMatrix
{
    #region Choice Matrix
    #region EFS_GroupLevelChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_GroupLevelChoiceType
    {
        interest,
        interestLeg,
        nominalPeriod,
        nominalPeriods,
        payment,
        paymentDates,
        premium,
        returnLeg,
        simplePayment,
        swaptionUnderlyer,
    }
    #endregion EFS_GroupLevelChoiceType
    #region EFS_AvgFixingChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_AvgFixingChoiceType
    {
        quotedFixing,
        settlementFixing,
    }
    #endregion EFS_AvgFixingChoiceType
    #region EFS_CalculationPeriodChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_CalculationPeriodChoiceType
    {
        resetDates,
        capFlooreds,
    }
    #endregion EFS_CalculationPeriodChoiceType
    #region EFS_CapFloorChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_CapFloorChoiceType
    {
        capFloorStream,
        premium,
        additionalPayment,
        earlyTerminationProvision,
        mandatoryEarlyTerminationProvision,
    }
    #endregion EFS_CapFloorChoiceType
    #region EFS_FxDigitalOptionChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_FxDigitalOptionChoiceType
    {
        barrier,
        exerciseDates,
        payout,
        trigger,
    }
    #endregion EFS_FxDigitalOptionChoiceType
    #region EFS_FxOptionChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    // EG 20180514 [23812] Report
    public enum EFS_FxOptionChoiceType
    {
        barrier,
        exerciseDates,
        exerciseProcedure,
        fxCurrencyAmount,
        linkedProductClosingAmounts,
        marginRatio,
        rebate,
    }
    #endregion EFS_FxOptionChoiceType
    #region EFS_EquityInitialValuationChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_EquityInitialValuationChoiceType
    {
        basket,
        nboption,
        notional,
        singleUnderlyer,
    }
    #endregion EFS_EquityInitialValuationChoiceType
    #region EFS_FxDeliverableLegChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_FxDeliverableLegChoiceType
    {
        exchangeCurrency,
        fwpDepreciableAmount,
        linkedProductClosingAmounts,
        marginRatio,
        settlementCurrency,
        sideRate,
    }
    #endregion EFS_FxDeliverableLegChoiceType
    #region EFS_FxOptionLegChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_FxOptionLegChoiceType
    {
        premium,
        fxOptionType,
    }
    #endregion EFS_FxOptionLegChoiceType
    #region EFS_LoanDepositChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_LoanDepositChoiceType
    {
        additionalPayment,
        cancelableProvision,
        earlyTerminationProvision,
        extendibleProvision,
        loanDepositStream,
        mandatoryEarlyTerminationProvision,
        stepUpProvision,
    }
    #endregion EFS_LoanDepositChoiceType
    #region EFS_PaymentChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_PaymentChoiceType
    {
        originalPayment,
        tax,
    }
    #endregion EFS_PaymentChoiceType
    #region EFS_SettlementCurrencyChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_SettlementCurrencyChoiceType
    {
        avgQuotedFixing,
        avgSettlementFixing,
        quotedFixing,
        settlementFixing,
    }
    #endregion EFS_SettlementCurrencyChoiceType
    #region EFS_StreamChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_StreamChoiceType
    {
        nominalPeriods,
        nominalPeriodsVariation,
        paymentDates,
    }
    #endregion EFS_StreamChoiceType
    #region EFS_SwapChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    public enum EFS_SwapChoiceType
    {
        additionalPayment,
        cancelableProvision,
        earlyTerminationProvision,
        extendibleProvision,
        swapStream,
        mandatoryEarlyTerminationProvision,
        stepUpProvision,
    }
    #endregion EFS_SwapChoiceType
    #region EFS_FxOptionProvisionChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    // EG 20180514 [23812] Report
    public enum EFS_FxOptionProvisionChoiceType
    {
        premium,
        fxOptionType,
        cancelableProvision,
        earlyTerminationProvision,
        extendibleProvision,
    }
    #endregion EFS_FxOptionProvisionChoiceType
    #region EFS_FxDigitalOptionProvisionChoiceType
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
    // EG 20180514 [23812] Report
    public enum EFS_FxDigitalOptionProvisionChoiceType
    {
        premium,
        digitalOptionType,
        cancelableProvision,
        earlyTerminationProvision,
        extendibleProvision,
    }
    #endregion EFS_FxDigitalOptionProvisionChoiceType

    #endregion Choice Matrix
    

    #region Product
    #region EFS_EventMatrixBulletPayment
    public class EFS_EventMatrixBulletPayment : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixGroupLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixBulletPayment() { productName = "BulletPayment"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixBulletPayment
    #region EFS_EventMatrixCapFloor
    public class EFS_EventMatrixCapFloor : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("capFloorStream", typeof(EFS_EventMatrixStream))]
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CapFloorChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixCapFloor() { productName = "CapFloor"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixCapFloor
    #region EFS_EventMatrixEquityOption
    public class EFS_EventMatrixEquityOption : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("equityOptionPremium", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("equityOptionType", typeof(EFS_EventMatrixEquityOptionType))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixEquityOption() { productName = "EquityOption"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixEquityOption
    #region EFS_EventMatrixFra
    public class EFS_EventMatrixFra : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixGroupLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFra() { productName = "Fra"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFra
    #region EFS_EventMatrixFxAverageRateOption
    // EG 20180514 [23812] Report
    public class EFS_EventMatrixFxAverageRateOption : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("fxOptionType", typeof(EFS_EventMatrixFxOptionType))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxOptionProvisionChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFxAverageRateOption() { productName = "FxAverageRateOption"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFxAverageRateOption
    #region EFS_EventMatrixFxBarrierOption
    // EG 20180514 [23812] Report
    public class EFS_EventMatrixFxBarrierOption : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("fxOptionType", typeof(EFS_EventMatrixFxOptionType))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxOptionProvisionChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFxBarrierOption() { productName = "FxBarrierOption"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFxBarrierOption
    #region EFS_EventMatrixFxDigitalOption
    // EG 20180514 [23812] Report
    public class EFS_EventMatrixFxDigitalOption : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("digitalOptionType", typeof(EFS_EventMatrixDigitalOptionType))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxDigitalOptionProvisionChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFxDigitalOption() { productName = "FxDigitalOption"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFxDigitalOption
    #region EFS_EventMatrixFxLeg
    public class EFS_EventMatrixFxLeg : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("deliverableLeg", typeof(EFS_EventMatrixFxDeliverableLeg))]
        [System.Xml.Serialization.XmlElementAttribute("nonDeliverableLeg", typeof(EFS_EventMatrixFxNonDeliverableLeg))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFxLeg() { productName = "FxLeg"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFxLeg
    #region EFS_EventMatrixFxSwap
    public class EFS_EventMatrixFxSwap : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("deliverableLeg", typeof(EFS_EventMatrixFxDeliverableLeg))]
        [System.Xml.Serialization.XmlElementAttribute("nonDeliverableLeg", typeof(EFS_EventMatrixFxNonDeliverableLeg))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFxSwap() { productName = "FxSwap"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFxLeg
    #region EFS_EventMatrixFxOptionLeg
    // EG 20150403 (POC] Add MarginRatio
    // EG 20180514 [23812] Report
    public class EFS_EventMatrixFxOptionLeg : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("fxOptionType", typeof(EFS_EventMatrixFxOptionType))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxOptionProvisionChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixFxOptionLeg() { productName = "FxOptionLeg"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixFxOptionLeg
    #region EFS_EventMatrixLoanDeposit
    public class EFS_EventMatrixLoanDeposit : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("loanDepositStream", typeof(EFS_EventMatrixStream))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("stepUpProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_LoanDepositChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixLoanDeposit() { productName = "LoanDeposit"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixLoanDeposit
    

    #region EFS_EventMatrixSwap
    public class EFS_EventMatrixSwap : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("swapStream", typeof(EFS_EventMatrixStream))]
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("stepUpProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SwapChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixSwap() { productName = "Swap"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixSwap
    #region EFS_EventMatrixSwapUnderlyer
    public class EFS_EventMatrixSwapUnderlyer : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("swapStream", typeof(EFS_EventMatrixStream))]
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("stepUpProvision", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SwapChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixSwapUnderlyer() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixSwap
    #region EFS_EventMatrixSwaption
    public class EFS_EventMatrixSwaption : EFS_EventMatrixProduct
    {
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixGroupLevel))]        
        public EFS_EventMatrixGroup[] group;

        #region Constructors
        public EFS_EventMatrixSwaption() { productName = "Swaption"; }
        #endregion Constructors
    }
    #endregion EFS_EventsParametersSwaption
    #region EFS_EventMatrixTermDeposit
    public class EFS_EventMatrixTermDeposit : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixGroupLevel))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixTermDeposit() { productName = "TermDeposit"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixTermDeposit
    #endregion Product

    #region EFS_EventMatrixAvgFixing
    public class EFS_EventMatrixAvgFixing : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("settlementFixing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("quotedFixing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_AvgFixingChoiceType[] itemsElementName;

        public EFS_EventMatrixAvgFixing() { }
    }
    #endregion EFS_EventMatrixAvgFixing

    #region EFS_EventMatrixCalculationPeriodDates
    public class EFS_EventMatrixCalculationPeriodDates : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("resetDates", typeof(EFS_EventMatrixResetDates))]
        [System.Xml.Serialization.XmlElementAttribute("capFlooreds", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixCalculationPeriodDates() { }
    }
    #endregion EFS_EventMatrixCalculationPeriodDates
    #region EFS_EventMatrixDigitalOptionType
    public class EFS_EventMatrixDigitalOptionType : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("payout", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("trigger", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("barrier", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxDigitalOptionChoiceType[] itemsElementName;

        public EFS_EventMatrixDigitalOptionType() { }
    }
    #endregion EFS_EventMatrixDigitalOptionType
    #region EFS_EventMatrixEquityBasketValuationDates
    public class EFS_EventMatrixEquityBasketValuationDates : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("underlyerValuationDates", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixEquityBasketValuationDates() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixEquityBasketValuationDates
    #region EFS_EventMatrixEquityInitialValuation
    public class EFS_EventMatrixEquityInitialValuation : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("nboption", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("notional", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("singleUnderlyer", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("basket", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EquityInitialValuationChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixEquityInitialValuation() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixEquityInitialValuation
    #region EFS_EventMatrixEquityOptionType
    public class EFS_EventMatrixEquityOptionType : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("initialValuation", typeof(EFS_EventMatrixEquityInitialValuation))]
        [System.Xml.Serialization.XmlElementAttribute("asianFeatures", typeof(EFS_EventMatrixEquityValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("barrierFeatures", typeof(EFS_EventMatrixEquityValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("knockFeatures", typeof(EFS_EventMatrixEquityValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixEquityValuationDates))]
        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_OptionBaseChoiceType[] itemsElementName;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixEquityOptionType() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixEquityOptionType
    #region EFS_EventMatrixEquityValuationDates
    public class EFS_EventMatrixEquityValuationDates : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("underlyerValuationDates", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("basketValuationDates", typeof(EFS_EventMatrixEquityBasketValuationDates))]
        public EFS_EventMatrixGroup[] group;
        #endregion Members
        #region Constructors
        public EFS_EventMatrixEquityValuationDates() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixEquityValuationDates
    #region EFS_EventMatrixExchangeCurrency
    public class EFS_EventMatrixExchangeCurrency : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("spotFixing", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixExchangeCurrency() { }
    }
    #endregion EFS_EventMatrixExchangeCurrency
    #region EFS_EventMatrixGroupLevel
    public class EFS_EventMatrixGroupLevel : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("interest", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("nominalPeriod", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("nominalPeriods", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("payment", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("simplePayment", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(EFS_EventMatrixPaymentDates))]
        [System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("swaptionUnderlyer", typeof(EFS_EventMatrixSwap))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_GroupLevelChoiceType[] itemsElementName;

        public EFS_EventMatrixGroupLevel() { }
    }
    #endregion EFS_EventMatrixGroupLevel
    #region EFS_EventMatrixFxDeliverableLeg
    // EG 20150403 (POC] Add MarginRatio|linkedProductClosingAmounts
    public class EFS_EventMatrixFxDeliverableLeg : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("exchangeCurrency", typeof(EFS_EventMatrixExchangeCurrency))]
        [System.Xml.Serialization.XmlElementAttribute("sideRate", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("fwpDepreciableAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marginRatio", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("linkedProductClosingAmounts", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxDeliverableLegChoiceType[] itemsElementName;

        public EFS_EventMatrixFxDeliverableLeg() { }
    }
    #endregion EFS_EventMatrixFxDeliverableLeg
    #region EFS_EventMatrixFxNonDeliverableLeg
    // EG 20150403 (POC] Add MarginRatio
    public class EFS_EventMatrixFxNonDeliverableLeg : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("exchangeCurrency", typeof(EFS_EventMatrixExchangeCurrency))]
        [System.Xml.Serialization.XmlElementAttribute("sideRate", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(EFS_EventMatrixSettlementCurrency))]
        [System.Xml.Serialization.XmlElementAttribute("fwpDepreciableAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marginRatio", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("linkedProductClosingAmounts", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxDeliverableLegChoiceType[] itemsElementName;

        public EFS_EventMatrixFxNonDeliverableLeg() { }
    }
    #endregion EFS_EventMatrixFxNonDeliverableLeg
    #region EFS_EventMatrixFxOptionType
    public class EFS_EventMatrixFxOptionType : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("fxCurrencyAmount", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("marginRatio", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("linkedProductClosingAmounts", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("barrier", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("rebate", typeof(EFS_EventMatrixPayment))]
        [System.Xml.Serialization.XmlElementAttribute("exerciseProcedure", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxOptionChoiceType[] itemsElementName;

        public EFS_EventMatrixFxOptionType() { }
    }
    #endregion EFS_EventMatrixFxOptionType
    #region EFS_EventMatrixPayment
    public class EFS_EventMatrixPayment : EFS_EventMatrixGroup
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("originalPayment", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("tax", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_PaymentChoiceType[] itemsElementName;

        #endregion Members
        #region Constructors
        public EFS_EventMatrixPayment() { }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixPayment
    #region EFS_EventMatrixPaymentDates
    public class EFS_EventMatrixPaymentDates : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriodDates", typeof(EFS_EventMatrixCalculationPeriodDates))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixPaymentDates() { }
    }
    #endregion EFS_EventMatrixPaymentDates
    #region EFS_EventMatrixResetDates
    public class EFS_EventMatrixResetDates : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("selfAverageDates", typeof(EFS_EventMatrixSelfAverageDates))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixResetDates() { }
    }
    #endregion EFS_EventMatrixResetDates
    #region EFS_EventMatrixSelfAverageDates
    public class EFS_EventMatrixSelfAverageDates : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("selfResetDates", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixSelfAverageDates() { }
    }
    #endregion EFS_EventMatrixSelfAverageDates
    #region EFS_EventMatrixSettlementCurrency
    public class EFS_EventMatrixSettlementCurrency : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("avgSettlementFixing", typeof(EFS_EventMatrixAvgFixing))]
        [System.Xml.Serialization.XmlElementAttribute("settlementFixing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("avgQuotedFixing", typeof(EFS_EventMatrixAvgFixing))]
        [System.Xml.Serialization.XmlElementAttribute("quotedFixing", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SettlementCurrencyChoiceType[] itemsElementName;


        public EFS_EventMatrixSettlementCurrency() { }
    }
    #endregion EFS_EventMatrixSettlementCurrency
   

    #region EFS_EventMatrixStream
    public class EFS_EventMatrixStream : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("nominalPeriods", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("nominalPeriodsVariation", typeof(EFS_EventMatrixGroup))]
        [System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(EFS_EventMatrixPaymentDates))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("itemsElementName")]
        public EFS_EventMatrixGroup[] group;
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = false)]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_StreamChoiceType[] itemsElementName;


        public EFS_EventMatrixStream() { }
    }
    #endregion EFS_EventMatrixStream



}

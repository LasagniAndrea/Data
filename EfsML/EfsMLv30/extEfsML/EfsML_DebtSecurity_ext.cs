#region using directives
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EFS.EFSTools;

using EfsML.Enum;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.Shared;
using EfsML.v30.Ird;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Ird;
using FpML.v44.Shared;
#endregion using directives

namespace EfsML.v30.DebtSecurity
{
    #region AccruedInterestCalculationRules
    public partial class AccruedInterestCalculationRules : IAccruedInterestCalculationRules
    {
        #region Constructors
        public AccruedInterestCalculationRules()
        {
            rounding = new Rounding();
        }
        #endregion Constructors

        #region IAccruedInterestCalculationRules Members
        bool IAccruedInterestCalculationRules.calculationMethodSpecified
        {
            set { this.calculationMethodSpecified = value; }
            get { return this.calculationMethodSpecified;}
        }
        AccruedInterestCalculationMethodEnum IAccruedInterestCalculationRules.calculationMethod
        {
            set { this.calculationMethod = value; }
            get { return this.calculationMethod; }
        }
        bool IAccruedInterestCalculationRules.roundingSpecified
        {
            set { this.roundingSpecified = value; }
            get { return this.roundingSpecified; }
        }
        IRounding IAccruedInterestCalculationRules.rounding
        {
            set { this.rounding = (Rounding) value; }
            get { return this.rounding; }
        }
        bool IAccruedInterestCalculationRules.prorataDayCountFractionSpecified
        {
            set { this.prorataDayCountFractionSpecified = value; }
            get { return this.prorataDayCountFractionSpecified; }
        }
        DayCountFractionEnum IAccruedInterestCalculationRules.prorataDayCountFraction
        {
            set { this.prorataDayCountFraction = value; }
            get { return this.prorataDayCountFraction; }
        }
        #endregion IAccruedInterestCalculationRules Members
    }
    #endregion AccruedInterestCalculationRules

    #region Classification
    public partial class Classification : IClassification
    {
        #region Constructors
        public Classification()
        {
            debtSecurityClass = new Identification();
            symbol = new EFS_String();
            symbolSfx = new EFS_String();
        }
        #endregion Constructors

        #region IClassification Members
        bool IClassification.debtSecurityClassSpecified
        {
            set { this.debtSecurityClassSpecified = value; }
            get { return this.debtSecurityClassSpecified; }
        }
        IScheme IClassification.debtSecurityClass
        {
            set { this.debtSecurityClass = (Identification)value; }
            get { return (IScheme)this.debtSecurityClass; }
        }
        bool IClassification.cfiCodeSpecified
        {
            set { this.cfiCodeSpecified = value; }
            get { return this.cfiCodeSpecified; }
        }
        ICFIIdentifier IClassification.cfiCode
        {
            set { this.cfiCode = (CFIIdentifier) value; }
            get { return this.cfiCode; }
        }
        bool IClassification.productTypeCodeSpecified
        {
            set { this.productTypeCodeSpecified = value; }
            get { return this.productTypeCodeSpecified; }
        }
        ProductTypeCodeEnum IClassification.productTypeCode
        {
            set { this.productTypeCode = value; }
            get { return this.productTypeCode; }
        }
        bool IClassification.financialInstrumentProductTypeCodeSpecified
        {
            set { this.financialInstrumentProductTypeCodeSpecified = value; }
            get { return this.financialInstrumentProductTypeCodeSpecified; }
        }
        FinancialInstrumentProductTypeCodeEnum IClassification.financialInstrumentProductTypeCode
        {
            set { this.financialInstrumentProductTypeCode = value; }
            get { return this.financialInstrumentProductTypeCode; }
        }
        bool IClassification.symbolSpecified
        {
            set { this.symbolSpecified = value; }
            get { return this.symbolSpecified; }
        }
        EFS_String IClassification.symbol
        {
            set { this.symbol = value; }
            get { return this.symbol; }
        }
        bool IClassification.symbolSfxSpecified
        {
            set { this.symbolSfxSpecified = value; }
            get { return this.symbolSfxSpecified; }
        }
        EFS_String IClassification.symbolSfx
        {
            set { this.symbolSfx = value; }
            get { return this.symbolSfx; }
        }
        #endregion IClassification Members
    }
    #endregion Classification
    #region CFIIdentifier
    public partial class CFIIdentifier : ICFIIdentifier
    {
        #region ICFIIdentifier Members
        string ICFIIdentifier.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion ICFIIdentifier Members
    }
    #endregion CFIIdentifier
    #region CommercialPaper
    public partial class CommercialPaper : ICommercialPaper
    {
        #region Constructors
        public CommercialPaper()
        {
            regType = new EFS_Integer();
        }
        #endregion Constructors

        #region ICommercialPaper Members
        bool ICommercialPaper.programSpecified
        {
            set { this.programSpecified = value; }
            get { return this.programSpecified; }
        }
        CPProgramEnum ICommercialPaper.program
        {
            set { this.program = value; }
            get { return this.program; }
        }
        bool ICommercialPaper.regTypeSpecified
        {
            set { this.regTypeSpecified = value; }
            get { return this.regTypeSpecified; }
        }
        EFS_Integer ICommercialPaper.regType
        {
            set { this.regType = value; }
            get { return this.regType; }
        }
        #endregion ICommercialPaper Members
    }
    #endregion CommercialPaper

    #region DebtSecurity
    public partial class DebtSecurity : IProduct, IDebtSecurity
    {
        #region Accessors
        #region MinEffectiveDate
        public EFS_EventDate MinEffectiveDate
        {
            get
            {
                EFS_EventDate dtEffective = new EFS_EventDate();
                dtEffective.unadjustedDate = new EFS_Date();
                dtEffective.unadjustedDate.DateValue = DateTime.MinValue;
                dtEffective.adjustedDate = new EFS_Date();
                dtEffective.adjustedDate.DateValue = DateTime.MinValue;
                foreach (DebtSecurityStream stream in debtSecurityStream)
                {
                    if ((DateTime.MinValue == dtEffective.unadjustedDate.DateValue) ||
                        (0 < dtEffective.unadjustedDate.DateValue.CompareTo(stream.EffectiveDate.unadjustedDate.DateValue)))
                    {
                        dtEffective.unadjustedDate.DateValue = stream.EffectiveDate.unadjustedDate.DateValue;
                        dtEffective.adjustedDate.DateValue = stream.EffectiveDate.adjustedDate.DateValue;
                    }
                }
                return dtEffective;
            }
        }
        #endregion MinEffectiveDate
        #region MaxTerminationDate
        public EFS_EventDate MaxTerminationDate
        {
            get
            {
                EFS_EventDate dtTermination = new EFS_EventDate();
                dtTermination.unadjustedDate = new EFS_Date();
                dtTermination.unadjustedDate.DateValue = DateTime.MinValue;
                dtTermination.adjustedDate = new EFS_Date();
                dtTermination.adjustedDate.DateValue = DateTime.MinValue;
                foreach (IInterestRateStream stream in debtSecurityStream)
                {
                    if (0 < stream.TerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = stream.TerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = stream.TerminationDate.adjustedDate.DateValue;
                    }
                }
                return dtTermination;
            }
        }
        #endregion MaxTerminationDate
        #region Stream
        public DebtSecurityStream[] Stream
        {
            get { return debtSecurityStream; }
        }
        #endregion Stream
        #endregion Accessors
        #region Constructors
        public DebtSecurity()
        {
            security = new Security();
            debtSecurityStream = new DebtSecurityStream[1]{ new DebtSecurityStream() };
        }
        #endregion Constructors
        #region IProduct Members
        object IProduct.product { get { return this; } }
        IProductBase IProduct.productBase { get { return this; } }
        #endregion IProduct Members
        #region IDebtSecurity Members
        ISecurity IDebtSecurity.security
        {
            set { this.security = (Security)value; }
            get { return this.security; }
        }
        IDebtSecurityStream[] IDebtSecurity.stream
        {
            get { return this.debtSecurityStream; }
        }
        #endregion IDebtSecurity Members
    }
    #endregion DebtSecurity
    #region DebtSecurityStream
    public partial class DebtSecurityStream : IDebtSecurityStream
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_DebtSecurityAmounts efs_DebtSecurityAmounts;
        #endregion Members
        #region IDebtSecurityStream Members
        EFS_DebtSecurityAmounts IDebtSecurityStream.efs_DebtSecurityAmounts
        {
            set {efs_DebtSecurityAmounts = value;}
            get { return efs_DebtSecurityAmounts; }
        }
        IMoney IDebtSecurityStream.GetFirstParValueAmount
        {
            get 
            {
                Money parValueAmount = null;
                if (null != this.calculationPeriodDates.efs_CalculationPeriodDates)
                {
                    if (ArrFunc.IsFilled(this.calculationPeriodDates.efs_CalculationPeriodDates.nominalPeriods))
                    {
                        EFS_NominalPeriod nominalPeriod = this.calculationPeriodDates.efs_CalculationPeriodDates.nominalPeriods[0];
                        parValueAmount = new Money(nominalPeriod.PeriodAmount.DecValue, nominalPeriod.PeriodCurrency);
                    }
                    else if (ArrFunc.IsFilled(this.calculationPeriodDates.efs_CalculationPeriodDates.calculationPeriods))
                    {
                        EFS_CalculationPeriod calculationPeriod = this.calculationPeriodDates.efs_CalculationPeriodDates.calculationPeriods[0];
                        parValueAmount = new Money(calculationPeriod.PeriodAmount.DecValue, calculationPeriod.PeriodCurrency);
                    }
                }
                return parValueAmount; 
            }
        }
        #endregion IDebtSecurityStream Members
    }
    #endregion DebtSecurityStream

    #region FullCouponCalculationRules
    public partial class FullCouponCalculationRules : IFullCouponCalculationRules
    {
        #region Constructors
        public FullCouponCalculationRules()
        {
            unitCouponRounding = new Rounding();
            rounding = new Rounding();
        }
        #endregion Constructors

        #region IFullCouponCalculationRules Members
        bool IFullCouponCalculationRules.calculationMethodSpecified
        {
            set { this.calculationMethodSpecified = value; }
            get { return this.calculationMethodSpecified; }
        }
        FullCouponCalculationMethodEnum IFullCouponCalculationRules.calculationMethod
        {
            set { this.calculationMethod = value; }
            get { return this.calculationMethod; }
        }
        bool IFullCouponCalculationRules.unitCouponRoundingSpecified
        {
            set { this.unitCouponRoundingSpecified = value; }
            get { return this.unitCouponRoundingSpecified; }
        }
        IRounding IFullCouponCalculationRules.unitCouponRounding
        {
            set { this.unitCouponRounding = (Rounding) value; }
            get { return this.unitCouponRounding; }
        }
        bool IFullCouponCalculationRules.roundingSpecified
        {
            set { this.roundingSpecified = value; }
            get { return this.roundingSpecified; }
        }
        IRounding IFullCouponCalculationRules.rounding
        {
            set { this.rounding = (Rounding)value; }
            get { return this.rounding; }
        }
        #endregion IFullCouponCalculationRules Members
    }
    #endregion FullCouponCalculationRules

    #region Localization
    public partial class Localization : ILocalization
    {
        #region Constructors
        public Localization()
        {
            countryOfIssue = new Country();
            localeOfIssue = new Identification();
            stateOrProvinceOfIssue = new Identification();
        }
        #endregion Constructors

        #region ILocalization Members
        bool ILocalization.countryOfIssueSpecified
        {
            set { this.countryOfIssueSpecified = value; }
            get { return this.countryOfIssueSpecified; }
        }
        IScheme ILocalization.countryOfIssue
        {
            set { this.countryOfIssue = (Country)value; }
            get { return this.countryOfIssue; }
        }
        bool ILocalization.stateOrProvinceOfIssueSpecified
        {
            set { this.stateOrProvinceOfIssueSpecified = value; }
            get { return this.stateOrProvinceOfIssueSpecified; }
        }
        IScheme ILocalization.stateOrProvinceOfIssue
        {
            set { this.stateOrProvinceOfIssue = (Identification)value; }
            get { return (IScheme)this.stateOrProvinceOfIssue; }
        }
        bool ILocalization.localeOfIssueSpecified
        {
            set { this.localeOfIssueSpecified = value; }
            get { return this.localeOfIssueSpecified; }
        }
        IScheme ILocalization.localeOfIssue
        {
            set { this.localeOfIssue = (Identification)value; }
            get { return (IScheme)this.localeOfIssue; }
        }
        #endregion ILocalization Members
    }
    #endregion Localization

    #region PriceUnits
    public partial class PriceUnits : IPriceUnits
    {
        #region Accessors        
        #endregion Accessors

        #region Constructors
        public PriceUnits()
        {
            priceQuoteUnits = new PriceQuoteUnits();
            forced = new EFS_Boolean();
        }
        #endregion Constructors

        #region IPriceUnits Members
        bool IPriceUnits.forcedSpecified
        {
            set { this.forcedSpecified = value; }
            get { return this.forcedSpecified; }
        }
        bool IPriceUnits.forced
        {
            set { this.forced.BoolValue = value; }
            get { return this.forced.BoolValue; }
        }
        #endregion IPriceUnits Members

        #region IScheme Members
        string IScheme.scheme
        {
            set { this.priceQuoteUnits.priceQuoteUnitsScheme = value; }
            get { return this.priceQuoteUnits.priceQuoteUnitsScheme; }
        }
        string IScheme.Value
        {
            set { this.priceQuoteUnits.Value = value; }
            get { return this.priceQuoteUnits.Value; }
        }
        #endregion IScheme Members
    }
    #endregion PriceUnits

    #region Security
    public partial class Security : ISecurity
    {
        #region Constructors
        public Security()
        {
            classification = new Classification();
            couponType = new CouponType();
            localization = new Localization();
            instructionRegistryCountry = new Country();
            instructionRegistryReference = new PartyReference();
            instructionRegistryNone = new Empty();
            instructionRegistryNoneSpecified = true;
            guarantorPartyReference = new PartyReference();
            managerPartyReference = new PartyReference();
            seniority = new CreditSeniority();
            numberOfIssuedSecurities = new EFS_Integer();
            faceAmount = new Money();
            price = new SecurityPrice();
            commercialPaper = new CommercialPaper();
            calculationRules = new SecurityCalculationRules();
            orderRules = new SecurityOrderRules();
            quoteRules = new SecurityQuoteRules();
            indicator = new SecurityIndicator();
            yield = new SecurityYield();
        }
        #endregion Constructors

        #region ISecurity Members
        bool ISecurity.classificationSpecified
        {
            set { this.classificationSpecified = value; }
            get { return this.classificationSpecified; }
        }
        IClassification ISecurity.classification
        {
            set { this.classification = (Classification) value; }
            get { return this.classification; }
        }
        bool ISecurity.couponTypeSpecified
        {
            set { this.couponTypeSpecified = value; }
            get { return this.couponTypeSpecified; }
        }
        IScheme ISecurity.couponType
        {
            set { this.couponType = (CouponType) value; }
            get { return this.couponType; }
        }
        bool ISecurity.priceRateTypeSpecified
        {
            set { this.priceRateTypeSpecified = value; }
            get { return this.priceRateTypeSpecified; }
        }
        PriceRateTypeCodeEnum ISecurity.priceRateType
        {
            set { this.priceRateType = value; }
            get { return this.priceRateType; }
        }
        bool ISecurity.localizationSpecified
        {
            set { this.localizationSpecified = value; }
            get { return this.localizationSpecified; }
        }
        ILocalization ISecurity.localization
        {
            set { this.localization = (Localization)value; }
            get { return this.localization; }
        }
        bool ISecurity.instructionRegistryCountrySpecified
        {
            set { this.instructionRegistryCountrySpecified = value; }
            get { return this.instructionRegistryCountrySpecified; }
        }
        IScheme ISecurity.instructionRegistryCountry
        {
            set { this.instructionRegistryCountry = (Country) value; }
            get { return this.instructionRegistryCountry; }
        }
        bool ISecurity.instructionRegistryReferenceSpecified
        {
            set { this.instructionRegistryReferenceSpecified = value; }
            get { return this.instructionRegistryReferenceSpecified; }
        }
        IReference ISecurity.instructionRegistryReference
        {
            set { this.instructionRegistryReference = (PartyReference)value; }
            get { return this.instructionRegistryReference; }
        }
        bool ISecurity.instructionRegistryNoneSpecified
        {
            set { this.instructionRegistryNoneSpecified = value; }
            get { return this.instructionRegistryNoneSpecified; }
        }
        IEmpty ISecurity.instructionRegistryNone
        {
            set { this.instructionRegistryNone = (Empty)value; }
            get { return this.instructionRegistryNone; }
        }
        bool ISecurity.guarantorPartyReferenceSpecified
        {
            set { this.guarantorPartyReferenceSpecified = value; }
            get { return this.guarantorPartyReferenceSpecified; }
        }
        IReference ISecurity.guarantorPartyReference
        {
            set { this.guarantorPartyReference = (PartyReference)value; }
            get { return this.guarantorPartyReference; }
        }
        bool ISecurity.managerPartyReferenceSpecified
        {
            set { this.managerPartyReferenceSpecified = value; }
            get { return this.managerPartyReferenceSpecified; }
        }
        IReference ISecurity.managerPartyReference
        {
            set { this.managerPartyReference = (PartyReference) value; }
            get { return this.managerPartyReference; }
        }
        bool ISecurity.purposeSpecified
        {
            set { this.purposeSpecified = value; }
            get { return this.purposeSpecified; }
        }
        EFS_String ISecurity.purpose
        {
            set { this.purpose = value; }
            get { return this.purpose; }
        }
        bool ISecurity.senioritySpecified
        {
            set { this.senioritySpecified = value; }
            get { return this.senioritySpecified; }
        }
        IScheme ISecurity.seniority
        {
            set { this.seniority = (CreditSeniority)value; }
            get { return this.seniority; }
        }
        bool ISecurity.numberOfIssuedSecuritiesSpecified
        {
            set { this.numberOfIssuedSecuritiesSpecified = value; }
            get { return this.numberOfIssuedSecuritiesSpecified; }
        }
        EFS_Integer ISecurity.numberOfIssuedSecurities
        {
            set { this.numberOfIssuedSecurities = value; }
            get { return this.numberOfIssuedSecurities; }
        }
        bool ISecurity.faceAmountSpecified
        {
            set { this.faceAmountSpecified = value; }
            get { return this.faceAmountSpecified; }
        }
        IMoney ISecurity.faceAmount
        {
            set { this.faceAmount = (Money)value; }
            get { return this.faceAmount; }
        }
        bool ISecurity.priceSpecified
        {
            set { this.priceSpecified = value; }
            get { return this.priceSpecified; }
        }
        ISecurityPrice ISecurity.price
        {
            set { this.price = (SecurityPrice)value; }
            get { return this.price; }
        }
        bool ISecurity.commercialPaperSpecified
        {
            set { this.commercialPaperSpecified = value; }
            get { return this.commercialPaperSpecified; }
        }
        ICommercialPaper ISecurity.commercialPaper
        {
            set { this.commercialPaper = (CommercialPaper)value; }
            get { return this.commercialPaper; }
        }
        bool ISecurity.calculationRulesSpecified
        {
            set { this.calculationRulesSpecified = value; }
            get { return this.calculationRulesSpecified; }
        }
        ISecurityCalculationRules ISecurity.calculationRules
        {
            set { this.calculationRules = (SecurityCalculationRules)value; }
            get { return this.calculationRules; }
        }
        bool ISecurity.orderRulesSpecified
        {
            set { this.orderRulesSpecified = value; }
            get { return this.orderRulesSpecified; }
        }
        ISecurityOrderRules ISecurity.orderRules
        {
            set { this.orderRules = (SecurityOrderRules)value; }
            get { return this.orderRules; }
        }
        bool ISecurity.quoteRulesSpecified
        {
            set { this.quoteRulesSpecified = value; }
            get { return this.quoteRulesSpecified; }
        }
        ISecurityQuoteRules ISecurity.quoteRules
        {
            set { this.quoteRules = (SecurityQuoteRules)value; }
            get { return this.quoteRules; }
        }
        bool ISecurity.indicatorSpecified
        {
            set { this.indicatorSpecified = value; }
            get { return this.indicatorSpecified; }
        }
        ISecurityIndicator ISecurity.indicator
        {
            set { this.indicator = (SecurityIndicator)value; }
            get { return this.indicator; }
        }
        bool ISecurity.yieldSpecified
        {
            set { this.yieldSpecified = value; }
            get { return this.yieldSpecified; }
        }
        ISecurityYield ISecurity.yield
        {
            set { this.yield = (SecurityYield)value; }
            get { return this.yield; }
        }
        ILocalization ISecurity.CreateLocalization()
        {
            return new Localization();
        }
        IClassification ISecurity.CreateClassification()
        {
            return new Classification();
        }
        #endregion ISecurity Members

        

    }
    #endregion Security
    #region SecurityCalculationRules
    public partial class SecurityCalculationRules : ISecurityCalculationRules
    {
        #region Constructors
        public SecurityCalculationRules()
        {
            fullCouponCalculationRules = new FullCouponCalculationRules();
            accruedInterestCalculationRules = new AccruedInterestCalculationRules();
        }
        #endregion Constructors
        #region ISecurityCalculationRules Members
        bool ISecurityCalculationRules.fullCouponCalculationRulesSpecified
        {
            set { this.fullCouponCalculationRulesSpecified = value; }
            get { return this.fullCouponCalculationRulesSpecified; }
        }
        IFullCouponCalculationRules ISecurityCalculationRules.fullCouponCalculationRules
        {
            set { this.fullCouponCalculationRules = (FullCouponCalculationRules)value; }
            get { return this.fullCouponCalculationRules; }
        }
        bool ISecurityCalculationRules.accruedInterestCalculationRulesSpecified
        {
            set { this.accruedInterestCalculationRulesSpecified = value; }
            get { return this.accruedInterestCalculationRulesSpecified; }
        }
        IAccruedInterestCalculationRules ISecurityCalculationRules.accruedInterestCalculationRules
        {
            set { this.accruedInterestCalculationRules = (AccruedInterestCalculationRules)value; }
            get { return this.accruedInterestCalculationRules; }
        }
        #endregion ISecurityCalculationRules Members
    }
    #endregion SecurityCalculationRules
    #region SecurityIndicator
    public partial class SecurityIndicator : ISecurityIndicator
    {
        #region Constructors
        public SecurityIndicator()
        {
            certificated = new EFS_Boolean();
            dematerialised = new EFS_Boolean();
            fungible = new EFS_Boolean();
            immobilised = new EFS_Boolean();
            amortised = new EFS_Boolean();
            callProtection = new EFS_Boolean();
            callable = new EFS_Boolean();
            putable = new EFS_Boolean();
            convertible = new EFS_Boolean();
            escrowed = new EFS_Boolean();
            prefunded = new EFS_Boolean();
            paymentDirection = new EFS_Boolean();
            quoted = new EFS_Boolean();
        }
        #endregion Constructors

        #region ISecurityIndicator Membres

        bool ISecurityIndicator.certificatedSpecified
        {
            set { this.certificatedSpecified = value; }
            get { return this.certificatedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.certificated
        {
            set { this.certificated = value; }
            get { return this.certificated; }
        }
        bool ISecurityIndicator.dematerialisedSpecified
        {
            set { this.dematerialisedSpecified = value; }
            get { return this.dematerialisedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.dematerialised
        {
            set { this.dematerialised = value; }
            get { return this.dematerialised; }
        }
        bool ISecurityIndicator.fungibleSpecified
        {
            set { this.fungibleSpecified = value; }
            get { return this.fungibleSpecified; }
        }
        EFS_Boolean ISecurityIndicator.fungible
        {
            set { this.fungible = value; }
            get { return this.fungible; }
        }
        bool ISecurityIndicator.immobilisedSpecified
        {
            set { this.immobilisedSpecified = value; }
            get { return this.immobilisedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.immobilised
        {
            set { this.immobilised = value; }
            get { return this.immobilised; }
        }
        bool ISecurityIndicator.amortisedSpecified
        {
            set { this.amortisedSpecified = value; }
            get { return this.amortisedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.amortised
        {
            set { this.amortised = value; }
            get { return this.amortised; }
        }
        bool ISecurityIndicator.callProtectionSpecified
        {
            set { this.callProtectionSpecified = value; }
            get { return this.callProtectionSpecified; }
        }
        EFS_Boolean ISecurityIndicator.callProtection
        {
            set { this.callProtection = value; }
            get { return this.callProtection; }
        }
        bool ISecurityIndicator.callableSpecified
        {
            set { this.callableSpecified = value; }
            get { return this.callableSpecified; }
        }
        EFS_Boolean ISecurityIndicator.callable
        {
            set { this.callable = value; }
            get { return this.callable; }
        }
        bool ISecurityIndicator.putableSpecified
        {
            set { this.putableSpecified = value; }
            get { return this.putableSpecified; }
        }
        EFS_Boolean ISecurityIndicator.putable
        {
            set { this.putable = value; }
            get { return this.putable; }
        }
        bool ISecurityIndicator.convertibleSpecified
        {
            set { this.convertibleSpecified = value; }
            get { return this.convertibleSpecified; }
        }
        EFS_Boolean ISecurityIndicator.convertible
        {
            set { this.convertible = value; }
            get { return this.convertible; }
        }
        bool ISecurityIndicator.escrowedSpecified
        {
            set { this.escrowedSpecified = value; }
            get { return this.escrowedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.escrowed
        {
            set { this.escrowed = value; }
            get { return this.escrowed; }
        }
        bool ISecurityIndicator.prefundedSpecified
        {
            set { this.prefundedSpecified = value; }
            get { return this.prefundedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.prefunded
        {
            set { this.prefunded = value; }
            get { return this.prefunded; }
        }
        bool ISecurityIndicator.paymentDirectionSpecified
        {
            set { this.paymentDirectionSpecified = value; }
            get { return this.paymentDirectionSpecified; }
        }
        EFS_Boolean ISecurityIndicator.paymentDirection
        {
            set { this.paymentDirection = value; }
            get { return this.paymentDirection; }
        }
        bool ISecurityIndicator.quotedSpecified
        {
            set { this.quotedSpecified = value; }
            get { return this.quotedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.quoted
        {
            set { this.quoted = value; }
            get { return this.quoted; }
        }
        #endregion ISecurityIndicator Membres
    }
    #endregion SecurityIndicator
    #region SecurityOrderRules
    public partial class SecurityOrderRules : ISecurityOrderRules
    {
        #region Constructors
        public SecurityOrderRules()
        {
            priceUnits = new PriceUnits();
            accruedInterestIndicator = new EFS_Boolean();
            priceInPercentageRounding = new Rounding();
            priceInRateRounding = new Rounding();
            settlementDaysOffset = new Offset();
        }
        #endregion Constructors

        #region ISecurityOrderRules Members
        bool ISecurityOrderRules.priceUnitsSpecified
        {
            set { this.priceUnitsSpecified = value; }
            get { return this.priceUnitsSpecified; }
        }
        IPriceUnits ISecurityOrderRules.priceUnits
        {
            set { this.priceUnits = (PriceUnits) value; }
            get { return this.priceUnits; }
        }
        bool ISecurityOrderRules.accruedInterestIndicatorSpecified
        {
            set { this.accruedInterestIndicatorSpecified = value; }
            get { return this.accruedInterestIndicatorSpecified; }
        }
        EFS_Boolean ISecurityOrderRules.accruedInterestIndicator
        {
            set { this.accruedInterestIndicator = value; }
            get { return this.accruedInterestIndicator; }
        }
        bool ISecurityOrderRules.priceInPercentageRoundingSpecified
        {
            set { this.priceInPercentageRoundingSpecified = value; }
            get { return this.priceInPercentageRoundingSpecified; }
        }
        IRounding ISecurityOrderRules.priceInPercentageRounding
        {
            set { this.priceInPercentageRounding = (Rounding) value; }
            get { return this.priceInPercentageRounding; }
        }
        bool ISecurityOrderRules.priceInRateRoundingSpecified
        {
            set { this.priceInRateRoundingSpecified = value; }
            get { return this.priceInRateRoundingSpecified; }
        }
        IRounding ISecurityOrderRules.priceInRateRounding
        {
            set { this.priceInRateRounding = (Rounding)value; }
            get { return this.priceInRateRounding; }
        }
        bool ISecurityOrderRules.quantityTypeSpecified
        {
            set { this.quantityTypeSpecified = value; }
            get { return this.quantityTypeSpecified; }
        }
        OrderQuantityType3CodeEnum ISecurityOrderRules.quantityType
        {
            set { this.quantityType = value; }
            get { return this.quantityType; }
        }
        bool ISecurityOrderRules.settlementDaysOffsetSpecified
        {
            set { this.settlementDaysOffsetSpecified = value; }
            get { return this.settlementDaysOffsetSpecified; }
        }
        IOffset ISecurityOrderRules.settlementDaysOffset
        {
            set { this.settlementDaysOffset = (Offset)value; }
            get { return this.settlementDaysOffset; }
        }
        #endregion ISecurityOrderRules Members
    }
    #endregion SecurityOrderRules
    #region SecurityPrice
    public partial class SecurityPrice : ISecurityPrice
    {
        #region Constructors
        public SecurityPrice()
        {
            issuePricePercentage = new EFS_Decimal();
            redemptionPricePercentage = new EFS_Decimal();
            redemptionPriceFormula = new Formula();
            redemptionPriceNone = new Empty();
        }
        #endregion Constructors

        #region ISecurityPrice Members
        bool ISecurityPrice.issuePricePercentageSpecified
        {
            set { this.issuePricePercentageSpecified = value; }
            get { return this.issuePricePercentageSpecified; }
        }
        EFS_Decimal ISecurityPrice.issuePricePercentage
        {
            set { this.issuePricePercentage = value; }
            get { return this.issuePricePercentage; }
        }
        bool ISecurityPrice.redemptionPricePercentageSpecified
        {
            set { this.redemptionPricePercentageSpecified = value; }
            get { return this.redemptionPricePercentageSpecified; }
        }
        EFS_Decimal ISecurityPrice.redemptionPricePercentage
        {
            set { this.redemptionPricePercentage = value; }
            get { return this.redemptionPricePercentage; }
        }
        bool ISecurityPrice.redemptionPriceFormulaSpecified
        {
            set { this.redemptionPriceFormulaSpecified = value; }
            get { return this.redemptionPriceFormulaSpecified; }
        }
        IFormula ISecurityPrice.redemptionPriceFormula
        {
            set { this.redemptionPriceFormula = (Formula)value; }
            get { return this.redemptionPriceFormula; }
        }
        bool ISecurityPrice.redemptionPriceNoneSpecified
        {
            set { this.redemptionPriceNoneSpecified = value; }
            get { return this.redemptionPriceNoneSpecified; }
        }
        IEmpty ISecurityPrice.redemptionPriceNone
        {
            set { this.redemptionPriceNone = (Empty)value; }
            get { return this.redemptionPriceNone; }
        }
        #endregion ISecurityPrice Members
    }
    #endregion SecurityPrice
    #region SecurityQuoteRules
    public partial class SecurityQuoteRules : ISecurityQuoteRules
    {
        #region Constructors
        public SecurityQuoteRules()
        {
            quoteUnits = new PriceQuoteUnits();
            accruedInterestIndicator = new EFS_Boolean();
            quoteRounding = new Rounding();
        }
        #endregion Constructors

        #region ISecurityQuoteRules Members
        bool ISecurityQuoteRules.quoteUnitsSpecified
        {
            set { this.quoteUnitsSpecified = value; }
            get { return this.quoteUnitsSpecified; }
        }
        IScheme ISecurityQuoteRules.quoteUnits
        {
            set { this.quoteUnits =(PriceQuoteUnits) value; }
            get { return (IScheme)this.quoteUnits; }
        }
        bool ISecurityQuoteRules.accruedInterestIndicatorSpecified
        {
            set { this.accruedInterestIndicatorSpecified = value; }
            get { return this.accruedInterestIndicatorSpecified; }
        }
        EFS_Boolean ISecurityQuoteRules.accruedInterestIndicator
        {
            set { this.accruedInterestIndicator = value; }
            get { return this.accruedInterestIndicator; }
        }
        bool ISecurityQuoteRules.quoteRoundingSpecified
        {
            set { this.quoteRoundingSpecified = value; }
            get { return this.quoteRoundingSpecified; }
        }
        IRounding ISecurityQuoteRules.quoteRounding
        {
            set { this.quoteRounding =(Rounding) value; }
            get { return this.quoteRounding; }
        }
        #endregion ISecurityQuoteRules Members
    }
    #endregion SecurityQuoteRules
    #region SecurityYield
    public partial class SecurityYield : ISecurityYield
    {
        #region Constructors
        public SecurityYield()
        {
            yield = new EFS_Decimal();
            yieldCalculationDate = new EFS_Date();
        }
        #endregion Constructors
        #region ISecurityYield Members
        bool ISecurityYield.yieldTypeSpecified
        {
            set { this.yieldTypeSpecified = value; }
            get { return this.yieldTypeSpecified; }
        }
        YieldTypeEnum ISecurityYield.yieldType
        {
            set { this.yieldType = value; }
            get { return this.yieldType; }
        }
        bool ISecurityYield.yieldSpecified
        {
            set { this.yieldSpecified = value; }
            get { return this.yieldSpecified; }
        }
        EFS_Decimal ISecurityYield.yield
        {
            set { this.yield = value; }
            get { return this.yield; }
        }
        bool ISecurityYield.yieldCalculationDateSpecified
        {
            set { this.yieldCalculationDateSpecified = value; }
            get { return this.yieldCalculationDateSpecified; }
        }
        EFS_Date ISecurityYield.yieldCalculationDate
        {
            set { this.yieldCalculationDate = value; }
            get { return this.yieldCalculationDate; }
        }
        #endregion ISecurityYield Members
    }
    #endregion SecurityYield
}

#region using directives
using EFS.ACommon;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Shared;
using System;
using System.Data;
#endregion using directives

namespace EfsML.v30.Security.Shared
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
        bool IAccruedInterestCalculationRules.CalculationMethodSpecified
        {
            set { this.calculationMethodSpecified = value; }
            get { return this.calculationMethodSpecified; }
        }
        AccruedInterestCalculationMethodEnum IAccruedInterestCalculationRules.CalculationMethod
        {
            set { this.calculationMethod = value; }
            get { return this.calculationMethod; }
        }
        bool IAccruedInterestCalculationRules.RoundingSpecified
        {
            set { this.roundingSpecified = value; }
            get { return this.roundingSpecified; }
        }
        IRounding IAccruedInterestCalculationRules.Rounding
        {
            set { this.rounding = (Rounding)value; }
            get { return this.rounding; }
        }
        bool IAccruedInterestCalculationRules.ProrataDayCountFractionSpecified
        {
            set { this.prorataDayCountFractionSpecified = value; }
            get { return this.prorataDayCountFractionSpecified; }
        }
        DayCountFractionEnum IAccruedInterestCalculationRules.ProrataDayCountFraction
        {
            set { this.prorataDayCountFraction = value; }
            get { return this.prorataDayCountFraction; }
        }
        #endregion IAccruedInterestCalculationRules Members
    }
    #endregion AccruedInterestCalculationRules
    #region AdjustableOffset
    public partial class AdjustableOffset : IAdjustableOffset, IComparable
    {
        #region Constructors
        public AdjustableOffset()
        {
            businessCentersNoneSpecified = true;
            businessCentersNone = new Empty();
            businessCentersReference = new BusinessCentersReference();
            businessCentersDefine = new BusinessCenters();
        }
        #endregion Constructors

        #region IComparable Members
        #region CompareTo
        public int CompareTo(object pObj)
        {
            int ret = -1;
            if ((pObj is Interval interval) && (period == interval.period) && (periodMultiplier.IntValue == interval.periodMultiplier.IntValue))
                ret = 0;
            return ret;
        }
        #endregion CompareTo
        #endregion IComparable Members
        #region IOffset Members
        bool IOffset.DayTypeSpecified
        {
            set { this.dayTypeSpecified = value; }
            get { return this.dayTypeSpecified; }
        }
        DayTypeEnum IOffset.DayType
        {
            set { this.dayType = value; }
            get { return this.dayType; }
        }
        /// <summary>
        /// Retourne les business Centers associées au devises {pCurrencies}
        /// <para>Retourne null, s'il n'existe aucun business center actif</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCurrencies">Devise au format ISO4217_ALPHA3</param>
        /// <returns></returns>
        /// FI 20131118 [19118] Add Commentaire 
        // EG 20180307 [23769] Gestion dbTransaction
        IBusinessCenters IOffset.GetBusinessCentersCurrency(string pConnectionString, IDbTransaction pDbTransaction, params string[] pCurrencies)
        {
            return ((IOffset)this).GetBusinessCentersCurrency(pConnectionString, pDbTransaction, pCurrencies);
        }
        IBusinessDayAdjustments IOffset.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
        {
            return ((IOffset)this).CreateBusinessDayAdjustments(pBusinessDayConvention, pIdBC);
        }
        #endregion IOffset Members
        #region IInterval Membres
        PeriodEnum IInterval.Period
        {
            set { this.period = value; }
            get { return this.period; }
        }
        EFS_Integer IInterval.PeriodMultiplier
        {
            set { this.periodMultiplier = value; }
            get { return this.periodMultiplier; }
        }
        IInterval IInterval.GetInterval(int pMultiplier, PeriodEnum pPeriod) { return new Interval(pPeriod.ToString(), pMultiplier); }
        IRounding IInterval.GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision) { return new Rounding(pRoundingDirection, pPrecision); }
        int IInterval.CompareTo(object obj) { return this.CompareTo(obj); }
        #endregion IInterval Membres
        #region IAdjustableOffset Members
        bool IAdjustableOffset.BusinessCentersNoneSpecified
        {
            set { this.businessCentersNoneSpecified = value; }
            get { return this.businessCentersNoneSpecified; }
        }
        object IAdjustableOffset.BusinessCentersNone
        {
            set { this.businessCentersNone = (Empty)value; }
            get { return this.businessCentersNone; }
        }
        bool IAdjustableOffset.BusinessCentersDefineSpecified
        {
            set { this.businessCentersDefineSpecified = value; }
            get { return this.businessCentersDefineSpecified; }
        }
        IBusinessCenters IAdjustableOffset.BusinessCentersDefine
        {
            set { this.businessCentersDefine = (BusinessCenters)value; }
            get { return this.businessCentersDefine; }
        }
        bool IAdjustableOffset.BusinessCentersReferenceSpecified
        {
            set { this.businessCentersReferenceSpecified = value; }
            get { return this.businessCentersReferenceSpecified; }
        }
        IReference IAdjustableOffset.BusinessCentersReference
        {
            set { this.businessCentersReference = (BusinessCentersReference)value; }
            get { return this.businessCentersReference; }
        }
        string IAdjustableOffset.BusinessCentersReferenceValue
        {
            get
            {
                if (this.businessCentersReferenceSpecified)
                    return this.businessCentersReference.href;
                return string.Empty;
            }
        }
        #endregion IAdjustableOffset Members
    }
    #endregion AdjustableOffset

    #region CashStream
    public partial class CashStream : ICashStream
    {
        #region Constructors
        public CashStream():base()
        {
        }
        #endregion Constructors
    }
    #endregion CashStream
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
        bool IClassification.DebtSecurityClassSpecified
        {
            set { this.debtSecurityClassSpecified = value; }
            get { return this.debtSecurityClassSpecified; }
        }
        IScheme IClassification.DebtSecurityClass
        {
            set { this.debtSecurityClass = (Identification)value; }
            get { return (IScheme)this.debtSecurityClass; }
        }
        bool IClassification.CfiCodeSpecified
        {
            set { this.cfiCodeSpecified = value; }
            get { return this.cfiCodeSpecified; }
        }
        ICFIIdentifier IClassification.CfiCode
        {
            set { this.cfiCode = (CFIIdentifier)value; }
            get { return this.cfiCode; }
        }
        bool IClassification.ProductTypeCodeSpecified
        {
            set { this.productTypeCodeSpecified = value; }
            get { return this.productTypeCodeSpecified; }
        }
        ProductTypeCodeEnum IClassification.ProductTypeCode
        {
            set { this.productTypeCode = value; }
            get { return this.productTypeCode; }
        }
        bool IClassification.FinancialInstrumentProductTypeCodeSpecified
        {
            set { this.financialInstrumentProductTypeCodeSpecified = value; }
            get { return this.financialInstrumentProductTypeCodeSpecified; }
        }
        FinancialInstrumentProductTypeCodeEnum IClassification.FinancialInstrumentProductTypeCode
        {
            set { this.financialInstrumentProductTypeCode = value; }
            get { return this.financialInstrumentProductTypeCode; }
        }
        bool IClassification.SymbolSpecified
        {
            set { this.symbolSpecified = value; }
            get { return this.symbolSpecified; }
        }
        EFS_String IClassification.Symbol
        {
            set { this.symbol = value; }
            get { return this.symbol; }
        }
        bool IClassification.SymbolSfxSpecified
        {
            set { this.symbolSfxSpecified = value; }
            get { return this.symbolSfxSpecified; }
        }
        EFS_String IClassification.SymbolSfx
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
        bool ICommercialPaper.ProgramSpecified
        {
            set { this.programSpecified = value; }
            get { return this.programSpecified; }
        }
        CPProgramEnum ICommercialPaper.Program
        {
            set { this.program = value; }
            get { return this.program; }
        }
        bool ICommercialPaper.RegTypeSpecified
        {
            set { this.regTypeSpecified = value; }
            get { return this.regTypeSpecified; }
        }
        EFS_Integer ICommercialPaper.RegType
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
        #region IsDiscount
        public bool IsDiscount
        {
            get
            {
                bool isDiscount = false;
                if (security.priceRateTypeSpecified)
                    isDiscount = (security.priceRateType == PriceRateType3CodeEnum.DISC);
                return isDiscount;
            }
        }
        #endregion IsDiscount
        #region IsNotDiscount
        public bool IsNotDiscount
        {
            get { return (false == IsDiscount); }
        }
        #endregion IsNotDiscount
        #region IsPerpetual
        /// <summary>
        /// Le DebtSecurity est perpetuel ou pas
        /// </summary>
        // EG 20190823 [FIXEDINCOME] New
        public bool IsPerpetual
        {
            get
            {
                return debtSecurityType == DebtSecurityTypeEnum.Perpetual;
            }
        }
        #endregion IsPerpetual
        #region MinEffectiveDate
        public EFS_EventDate MinEffectiveDate
        {
            get
            {
                EFS_EventDate dtEffective = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (IInterestRateStream stream in debtSecurityStream)
                {
                    EFS_EventDate streamEffectiveDate = stream.EffectiveDate;
                    if ((DateTime.MinValue == dtEffective.unadjustedDate.DateValue) ||
                        (0 < dtEffective.unadjustedDate.DateValue.CompareTo(streamEffectiveDate.unadjustedDate.DateValue)))
                    {
                        dtEffective.unadjustedDate.DateValue = streamEffectiveDate.unadjustedDate.DateValue;
                        dtEffective.adjustedDate.DateValue = streamEffectiveDate.adjustedDate.DateValue;
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
                EFS_EventDate dtTermination = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (IInterestRateStream stream in debtSecurityStream)
                {
                    EFS_EventDate streamTerminationDate = stream.TerminationDate;
                    if (0 < streamTerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = streamTerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = streamTerminationDate.adjustedDate.DateValue;
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
        // EG 20190823 [FIXEDINCOME] New prevCouponDate
        public DebtSecurity()
        {
            prevCouponDate = new EFS_Date();
            security = new Security();
            debtSecurityStream = new DebtSecurityStream[1] { new DebtSecurityStream() };
        }
        #endregion Constructors
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members
        #region IDebtSecurity Members
        // EG 20190823 [FIXEDINCOME] New
        DebtSecurityTypeEnum IDebtSecurity.DebtSecurityType
        {
            set { this.debtSecurityType = value; }
            get { return this.debtSecurityType; }
        }
        // EG 20190823 [FIXEDINCOME] New
        bool IDebtSecurity.PrevCouponDateSpecified
        {
            set { this.prevCouponDateSpecified = value; }
            get { return this.prevCouponDateSpecified; }
        }
        // EG 20190823 [FIXEDINCOME] New
        EFS_Date IDebtSecurity.PrevCouponDate 
        {
            set { this.prevCouponDate = (EFS_Date)value; }
            get { return this.prevCouponDate; }
        }
        ISecurity IDebtSecurity.Security
        {
            set { this.security = (Security)value; }
            get { return this.security; }
        }
        IDebtSecurityStream[] IDebtSecurity.Stream
        {
            set { this.debtSecurityStream = (DebtSecurityStream[])value; }
            get { return this.debtSecurityStream; }
        }
        #endregion IDebtSecurity Members
    }
    #endregion DebtSecurity
    #region DebtSecurityStream
    public partial class DebtSecurityStream : IDebtSecurityStream
    {
        #region IDebtSecurityStream Members
        bool IDebtSecurityStream.SecurityExchangesSpecified
        {
            set { this.securityExchangesSpecified = value; }
            get { return this.securityExchangesSpecified; }
        }
        ISecurityExchanges IDebtSecurityStream.SecurityExchanges
        {
            get { return this.securityExchanges; }
        }
        bool IDebtSecurityStream.IsInitialSecurityExchange
        {
            get
            {
                bool isInitialExchange = false;
                if (securityExchangesSpecified)
                    isInitialExchange = securityExchanges.initialExchange.BoolValue;
                return (isInitialExchange);
            }
        }
        bool IDebtSecurityStream.IsIntermediateSecurityExchange
        {
            get
            {
                bool isIntermediateExchange = false;
                if (securityExchangesSpecified)
                    isIntermediateExchange = securityExchanges.intermediateExchange.BoolValue;
                return (isIntermediateExchange);
            }
        }
        bool IDebtSecurityStream.IsFinalSecurityExchange
        {
            get
            {
                bool isFinalExchange = false;
                if (securityExchangesSpecified)
                    isFinalExchange = securityExchanges.finalExchange.BoolValue;
                return (isFinalExchange);
            }
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
        /// <summary>
        /// Calcul de la prochaine tombée d'intérêts (Cas des DebtSecurity perpetual)
        /// </summary>
        // EG 20190823 [FIXEDINCOME] New
        ICalculationPeriodDates IDebtSecurityStream.CalcNextInterestPeriodDates(string pCs, DebtSecurityTransactionContainer pDebtSecurityTransactionContainer, DateTime pPreviousCouponDate)
        {
            ICalculationPeriodDates _calculationPeriodDates = null;
            IPaymentDates payDates = paymentDates;
            object objectToSearch = pDebtSecurityTransactionContainer.DataDocument.DataDocument.Item;

            string hRef;
            #region Get CalculationPeriodDate [CalculationPeriodDateReference and/or ResetDateReference]
            if (payDates.CalculationPeriodDatesReferenceSpecified)
            {
                hRef = payDates.CalculationPeriodDatesReference.HRef;
                _calculationPeriodDates = (ICalculationPeriodDates)ReflectionTools.GetObjectById(objectToSearch, hRef);
            }
            else if (payDates.ResetDatesReferenceSpecified)
            {
                hRef = payDates.ResetDatesReference.HRef;
                IResetDates resetDates = (IResetDates)ReflectionTools.GetObjectById(objectToSearch, hRef);
                if (resetDates != null)
                    hRef = resetDates.CalculationPeriodDatesReference.HRef;
                _calculationPeriodDates = (ICalculationPeriodDates)ReflectionTools.GetObjectById(objectToSearch, hRef);
            }
            #endregion Get CalculationPeriodDate [CalculationPeriodDateReference and/or ResetDateReference]

            if (null != _calculationPeriodDates)
            {
                #region CalculationPeriodDates
                _calculationPeriodDates.Efs_CalculationPeriodDates = new EFS_CalculationPeriodDates(pCs, pDebtSecurityTransactionContainer.DataDocument);
                _ = _calculationPeriodDates.Efs_CalculationPeriodDates.Calc2(CalPeriodEnum.FirstAndRegular, _calculationPeriodDates, null, pPreviousCouponDate);
                #endregion CalculationPeriodDates
                #region PaymentDates
                paymentDates.efs_PaymentDates = new EFS_PaymentDates(pCs, paymentDates, _calculationPeriodDates, resetDates, this, pDebtSecurityTransactionContainer.DataDocument);
                #endregion PaymentDates
                if (calculationPeriodDates.Id == _calculationPeriodDates.Id)
                    calculationPeriodDates.efs_CalculationPeriodDates = _calculationPeriodDates.Efs_CalculationPeriodDates;
            }
            return calculationPeriodDates;
        }
        /// <summary>
        /// Calcul des date ExDate et RecordDate (Cas DebtSecurity perpetual)
        /// </summary>
        // EG 20190823 [FIXEDINCOME] New
        void IDebtSecurityStream.SetRecordAndExDates(string pCS, DataDocumentContainer pDataDocument, DividendDateReferenceEnum pTypeDate, IRelativeDateOffset pRelativeDateOffset)
        {
            Tools.OffSetDateRelativeTo(pCS, pRelativeDateOffset, out DateTime[] offsetDates, pDataDocument);
            if (ArrFunc.IsFilled(offsetDates) && (offsetDates.Length == paymentDates.efs_PaymentDates.paymentDates.Length))
            {
                int i = 0;
                foreach (EFS_PaymentDate paymentDate in paymentDates.efs_PaymentDates.paymentDates)
                {
                    switch (pTypeDate)
                    {
                        case DividendDateReferenceEnum.ExDate:
                            paymentDate.exDateAdjustmentSpecified = true;
                            paymentDate.exDateAdjustment = new EFS_AdjustableDate(pCS, offsetDates[i], pRelativeDateOffset.GetAdjustments, pDataDocument);
                            break;
                        case DividendDateReferenceEnum.RecordDate:
                            paymentDate.recordDateAdjustmentSpecified = true;
                            paymentDate.recordDateAdjustment = new EFS_AdjustableDate(pCS, offsetDates[i], pRelativeDateOffset.GetAdjustments, pDataDocument);
                            break;
                        default:
                            break;
                    }
                    i++;
                }
            }
        }
        #endregion IDebtSecurityStream Members


        #region IInterestRateStream Members
        bool IInterestRateStream.IsInitialExchange
        {
            get 
            {
                bool isInitialExchange = false;
                if (principalExchangesSpecified)
                    isInitialExchange = principalExchanges.initialExchange.BoolValue;
                return (isInitialExchange);
            }
        }
        bool IInterestRateStream.IsIntermediateExchange
        {
            get
            {
                bool isIntermediateExchange = false;
                if (principalExchangesSpecified)
                    isIntermediateExchange = principalExchanges.intermediateExchange.BoolValue;
                return (isIntermediateExchange);
            }
        }
        bool IInterestRateStream.IsFinalExchange
        {
            get
            {
                bool isFinalExchange = false;
                if (principalExchangesSpecified)
                    isFinalExchange = principalExchanges.finalExchange.BoolValue;
                return (isFinalExchange);
            }
        }
        #endregion IInterestRateStream Members
    }
    #endregion DebtSecurityStream

    #region FullCouponCalculationRules
    // EG 20150907 [21317] Add recordDate|exDate
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
        bool IFullCouponCalculationRules.CalculationMethodSpecified
        {
            set { this.calculationMethodSpecified = value; }
            get { return this.calculationMethodSpecified; }
        }
        FullCouponCalculationMethodEnum IFullCouponCalculationRules.CalculationMethod
        {
            set { this.calculationMethod = value; }
            get { return this.calculationMethod; }
        }
        bool IFullCouponCalculationRules.UnitCouponRoundingSpecified
        {
            set { this.unitCouponRoundingSpecified = value; }
            get { return this.unitCouponRoundingSpecified; }
        }
        IRounding IFullCouponCalculationRules.UnitCouponRounding
        {
            set { this.unitCouponRounding = (Rounding)value; }
            get { return this.unitCouponRounding; }
        }
        bool IFullCouponCalculationRules.RoundingSpecified
        {
            set { this.roundingSpecified = value; }
            get { return this.roundingSpecified; }
        }
        IRounding IFullCouponCalculationRules.Rounding
        {
            set { this.rounding = (Rounding)value; }
            get { return this.rounding; }
        }
        // EG 20150907 [21317]
        bool IFullCouponCalculationRules.RecordDateSpecified
        {
            set { this.recordDateSpecified = value; }
            get { return this.recordDateSpecified; }
        }
        // EG 20150907 [21317]
        IRelativeDateOffset IFullCouponCalculationRules.RecordDate
        {
            set { this.recordDate = (RelativeDateOffset) value; }
            get { return this.recordDate; }
        }
        // EG 20150907 [21317]
        bool IFullCouponCalculationRules.ExDateSpecified
        {
            set { this.exDateSpecified = value; }
            get { return this.exDateSpecified; }
        }
        // EG 20150907 [21317]
        IRelativeDateOffset IFullCouponCalculationRules.ExDate
        {
            set { this.exDate = (RelativeDateOffset) value; }
            get { return this.exDate; }
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
        bool ILocalization.CountryOfIssueSpecified
        {
            set { this.countryOfIssueSpecified = value; }
            get { return this.countryOfIssueSpecified; }
        }
        IScheme ILocalization.CountryOfIssue
        {
            set { this.countryOfIssue = (Country)value; }
            get { return this.countryOfIssue; }
        }
        bool ILocalization.StateOrProvinceOfIssueSpecified
        {
            set { this.stateOrProvinceOfIssueSpecified = value; }
            get { return this.stateOrProvinceOfIssueSpecified; }
        }
        IScheme ILocalization.StateOrProvinceOfIssue
        {
            set { this.stateOrProvinceOfIssue = (Identification)value; }
            get { return (IScheme)this.stateOrProvinceOfIssue; }
        }
        bool ILocalization.LocaleOfIssueSpecified
        {
            set { this.localeOfIssueSpecified = value; }
            get { return this.localeOfIssueSpecified; }
        }
        IScheme ILocalization.LocaleOfIssue
        {
            set { this.localeOfIssue = (Identification)value; }
            get { return (IScheme)this.localeOfIssue; }
        }
        #endregion ILocalization Members
    }
    #endregion Localization

    #region Margin
    public partial class Margin : IMargin
    {
        #region IMargin Members
        MarginTypeEnum IMargin.MarginType
        {
            set { this.marginType = value; }
            get { return this.marginType; }
        }
        EFS_Decimal IMargin.MarginFactor
        {
            set { this.marginFactor = value; }
            get { return this.marginFactor; }
        }
        #endregion IMargin Members
    }
    #endregion Margin
    #region MidLifeEvent
    public abstract partial class MidLifeEvent : IMidLifeEvent
    {
        #region IMidLifeEvent Members
        IAdjustedDate IMidLifeEvent.EventDate
        {
            set { this.eventDate = (IdentifiedDate)value; }
            get { return this.eventDate; }
        }
        #endregion IMidLifeEvent Members
        #region IEvent Members
        ISchemeId[] IEvent.EventId
        {
            set { this.eventId = (EventId[])value; }
            get { return this.eventId; }
        }
        #endregion IEvent Members
    }
    #endregion MidLifeEvent

    #region OrderPrice
    public partial class OrderPrice : IOrderPrice
    {
        #region constructor
        public OrderPrice()
        {
            this.priceUnits = new PriceQuoteUnits();

        }
        #endregion

        #region IOrderPrice Membres

        IScheme IOrderPrice.PriceUnits
        {
            get { return this.priceUnits; }
            set { this.priceUnits = (PriceQuoteUnits)value; }
        }

        bool IOrderPrice.CleanPriceSpecified
        {
            get { return this.cleanPriceSpecified; }
            set { this.cleanPriceSpecified = value; }
        }

        EFS_Decimal IOrderPrice.CleanPrice
        {
            get { return this.cleanPrice; }
            set { this.cleanPrice = value; }
        }
        bool IOrderPrice.DirtyPriceSpecified
        {
            get { return this.dirtyPriceSpecified; }
            set { this.dirtyPriceSpecified = value; }
        }
        EFS_Decimal IOrderPrice.DirtyPrice
        {
            get { return this.dirtyPrice; }
            set { this.dirtyPrice = value; }
        }


        bool IOrderPrice.AccruedInterestRateSpecified
        {
            get { return this.accruedInterestRateSpecified; }
            set { this.accruedInterestRateSpecified = value; }
        }

        EFS_Decimal IOrderPrice.AccruedInterestRate
        {
            get { return this.accruedInterestRate; }
            set { this.accruedInterestRate = value; }
        }

        bool IOrderPrice.AccruedInterestAmountSpecified
        {
            get { return this.accruedInterestAmountSpecified; }
            set { this.accruedInterestAmountSpecified = value; }
        }

        IMoney IOrderPrice.AccruedInterestAmount
        {
            get { return this.accruedInterestAmount; }
            set { this.accruedInterestAmount = (Money)value; }
        }

        #endregion
    }
    #endregion
    #region OrderQuantity
    public partial class OrderQuantity : IOrderQuantity
    {
        #region constructor
        public OrderQuantity()
        {
            this.notionalAmount = new Money();
        }
        #endregion constructor

        #region IOrderQuantity Membres
        OrderQuantityType3CodeEnum IOrderQuantity.QuantityType
        {
            get { return this.quantityType; }
            set { quantityType = value; }
        }
        bool IOrderQuantity.NumberOfUnitsSpecified
        {
            get { return this.numberOfUnitsSpecified; }
            set { this.numberOfUnitsSpecified = value; }
        }
        EFS_Decimal IOrderQuantity.NumberOfUnits
        {
            get { return this.numberOfUnits; }
            set { this.numberOfUnits = value; }
        }
        IMoney IOrderQuantity.NotionalAmount
        {
            get { return this.notionalAmount; }
            set { this.notionalAmount = (Money)value; }
        }
        #endregion
    }
    #endregion OrderQuantity

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
        bool IPriceUnits.ForcedSpecified
        {
            set { this.forcedSpecified = value; }
            get { return this.forcedSpecified; }
        }
        bool IPriceUnits.Forced
        {
            set { this.forced.BoolValue = value; }
            get { return this.forced.BoolValue; }
        }
        #endregion IPriceUnits Members

        #region IScheme Members
        string IScheme.Scheme
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
    // EG 20190716 [VCL : New FixedIncome] Upd (GetRoundingAccruedRate, GetDayCountFractionForAccruedRate, GetPriceQuote, GetAssetMeasure
    public partial class Security : ISecurity
    {
        #region Constructors
        // EG 20171031 [23509] Upd
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
            // EG 20170127 Qty Long To Decimal
            numberOfIssuedSecurities = new EFS_Decimal();
            faceAmount = new Money();
            price = new SecurityPrice();
            commercialPaper = new CommercialPaper();
            calculationRules = new SecurityCalculationRules();
            orderRules = new SecurityOrderRules();
            quoteRules = new SecurityQuoteRules();
            indicator = new SecurityIndicator();
            yield = new SecurityYield();
            exchangeId = new ExchangeId();
        }
        #endregion Constructors

        #region ISecurity Members
        bool ISecurity.ClassificationSpecified
        {
            set { this.classificationSpecified = value; }
            get { return this.classificationSpecified; }
        }
        IClassification ISecurity.Classification
        {
            set { this.classification = (Classification)value; }
            get { return this.classification; }
        }
        bool ISecurity.CouponTypeSpecified
        {
            set { this.couponTypeSpecified = value; }
            get { return this.couponTypeSpecified; }
        }
        IScheme ISecurity.CouponType
        {
            set { this.couponType = (CouponType)value; }
            get { return this.couponType; }
        }
        bool ISecurity.PriceRateTypeSpecified
        {
            set { this.priceRateTypeSpecified = value; }
            get { return this.priceRateTypeSpecified; }
        }
        PriceRateType3CodeEnum ISecurity.PriceRateType
        {
            set { this.priceRateType = value; }
            get { return this.priceRateType; }
        }
        bool ISecurity.LocalizationSpecified
        {
            set { this.localizationSpecified = value; }
            get { return this.localizationSpecified; }
        }
        ILocalization ISecurity.Localization
        {
            set { this.localization = (Localization)value; }
            get { return this.localization; }
        }
        bool ISecurity.InstructionRegistryCountrySpecified
        {
            set { this.instructionRegistryCountrySpecified = value; }
            get { return this.instructionRegistryCountrySpecified; }
        }
        IScheme ISecurity.InstructionRegistryCountry
        {
            set { this.instructionRegistryCountry = (Country)value; }
            get { return this.instructionRegistryCountry; }
        }
        bool ISecurity.InstructionRegistryReferenceSpecified
        {
            set { this.instructionRegistryReferenceSpecified = value; }
            get { return this.instructionRegistryReferenceSpecified; }
        }
        IReference ISecurity.InstructionRegistryReference
        {
            set { this.instructionRegistryReference = (PartyReference)value; }
            get { return this.instructionRegistryReference; }
        }
        bool ISecurity.InstructionRegistryNoneSpecified
        {
            set { this.instructionRegistryNoneSpecified = value; }
            get { return this.instructionRegistryNoneSpecified; }
        }
        IEmpty ISecurity.InstructionRegistryNone
        {
            set { this.instructionRegistryNone = (Empty)value; }
            get { return this.instructionRegistryNone; }
        }
        bool ISecurity.GuarantorPartyReferenceSpecified
        {
            set { this.guarantorPartyReferenceSpecified = value; }
            get { return this.guarantorPartyReferenceSpecified; }
        }
        IReference ISecurity.GuarantorPartyReference
        {
            set { this.guarantorPartyReference = (PartyReference)value; }
            get { return this.guarantorPartyReference; }
        }
        bool ISecurity.ManagerPartyReferenceSpecified
        {
            set { this.managerPartyReferenceSpecified = value; }
            get { return this.managerPartyReferenceSpecified; }
        }
        IReference ISecurity.ManagerPartyReference
        {
            set { this.managerPartyReference = (PartyReference)value; }
            get { return this.managerPartyReference; }
        }
        bool ISecurity.PurposeSpecified
        {
            set { this.purposeSpecified = value; }
            get { return this.purposeSpecified; }
        }
        EFS_String ISecurity.Purpose
        {
            set { this.purpose = value; }
            get { return this.purpose; }
        }
        bool ISecurity.SenioritySpecified
        {
            set { this.senioritySpecified = value; }
            get { return this.senioritySpecified; }
        }
        IScheme ISecurity.Seniority
        {
            set { this.seniority = (CreditSeniority)value; }
            get { return this.seniority; }
        }
        bool ISecurity.NumberOfIssuedSecuritiesSpecified
        {
            set { this.numberOfIssuedSecuritiesSpecified = value; }
            get { return this.numberOfIssuedSecuritiesSpecified; }
        }
        // EG 20170127 Qty Long To Decimal
        EFS_Decimal ISecurity.NumberOfIssuedSecurities
        {
            set { this.numberOfIssuedSecurities = value; }
            get { return this.numberOfIssuedSecurities; }
        }
        bool ISecurity.FaceAmountSpecified
        {
            set { this.faceAmountSpecified = value; }
            get { return this.faceAmountSpecified; }
        }
        IMoney ISecurity.FaceAmount
        {
            set { this.faceAmount = (Money)value; }
            get { return this.faceAmount; }
        }
        bool ISecurity.PriceSpecified
        {
            set { this.priceSpecified = value; }
            get { return this.priceSpecified; }
        }
        ISecurityPrice ISecurity.Price
        {
            set { this.price = (SecurityPrice)value; }
            get { return this.price; }
        }
        bool ISecurity.CommercialPaperSpecified
        {
            set { this.commercialPaperSpecified = value; }
            get { return this.commercialPaperSpecified; }
        }
        ICommercialPaper ISecurity.CommercialPaper
        {
            set { this.commercialPaper = (CommercialPaper)value; }
            get { return this.commercialPaper; }
        }
        bool ISecurity.CalculationRulesSpecified
        {
            set { this.calculationRulesSpecified = value; }
            get { return this.calculationRulesSpecified; }
        }
        ISecurityCalculationRules ISecurity.CalculationRules
        {
            set { this.calculationRules = (SecurityCalculationRules)value; }
            get { return this.calculationRules; }
        }
        bool ISecurity.OrderRulesSpecified
        {
            set { this.orderRulesSpecified = value; }
            get { return this.orderRulesSpecified; }
        }
        ISecurityOrderRules ISecurity.OrderRules
        {
            set { this.orderRules = (SecurityOrderRules)value; }
            get { return this.orderRules; }
        }
        bool ISecurity.QuoteRulesSpecified
        {
            set { this.quoteRulesSpecified = value; }
            get { return this.quoteRulesSpecified; }
        }
        ISecurityQuoteRules ISecurity.QuoteRules
        {
            set { this.quoteRules = (SecurityQuoteRules)value; }
            get { return this.quoteRules; }
        }
        bool ISecurity.IndicatorSpecified
        {
            set { this.indicatorSpecified = value; }
            get { return this.indicatorSpecified; }
        }
        ISecurityIndicator ISecurity.Indicator
        {
            set { this.indicator = (SecurityIndicator)value; }
            get { return this.indicator; }
        }
        bool ISecurity.YieldSpecified
        {
            set { this.yieldSpecified = value; }
            get { return this.yieldSpecified; }
        }
        ISecurityYield ISecurity.Yield
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
        // EG 20190716 [VCL : New FixedIncome] New
        IRounding ISecurity.GetRoundingAccruedRate(RoundingDirectionEnum pDirection, int pPrecision)
        {
            Rounding ret = new Rounding(pDirection, pPrecision);
            if (calculationRulesSpecified && calculationRules.accruedInterestCalculationRulesSpecified)
            {
                if (calculationRules.accruedInterestCalculationRules.roundingSpecified)
                {
                    ret = calculationRules.accruedInterestCalculationRules.rounding;
                    ret.precision.IntValue = System.Math.Max(pPrecision, ret.precision.IntValue);
                }
            }
            return ret;
        }
        // EG 20190716 [VCL : New FixedIncome] New
        DayCountFractionEnum ISecurity.GetDayCountFractionForAccruedRate(DayCountFractionEnum pDayCountFraction)
        {
            DayCountFractionEnum ret = pDayCountFraction;
            if (calculationRulesSpecified && calculationRules.accruedInterestCalculationRulesSpecified)
            {
                if (calculationRules.accruedInterestCalculationRules.calculationMethodSpecified)
                {
                    if ((AccruedInterestCalculationMethodEnum.Prorata == calculationRules.accruedInterestCalculationRules.calculationMethod) &&
                        calculationRules.accruedInterestCalculationRules.prorataDayCountFractionSpecified)
                        ret = calculationRules.accruedInterestCalculationRules.prorataDayCountFraction;
                }
            }
            return ret;
        }
        // EG 20190716 [VCL : New FixedIncome] New
        Cst.PriceQuoteUnits ISecurity.GetPriceQuote(Cst.PriceQuoteUnits pPriceQuoteUnits)
        {
            Cst.PriceQuoteUnits ret = pPriceQuoteUnits;
            if (quoteRulesSpecified && quoteRules.quoteUnitsSpecified)
            {
                if (quoteRules.quoteUnitsSpecified)
                {
                    Nullable<Cst.PriceQuoteUnits> priceQuoteRulesUnit = ReflectionTools.ConvertStringToEnumOrNullable<Cst.PriceQuoteUnits>(quoteRules.quoteUnits.Value);
                    if (priceQuoteRulesUnit.HasValue)
                    {
                        switch (priceQuoteRulesUnit)
                        {
                            case Cst.PriceQuoteUnits.ParValueDecimal:
                                ret = Cst.PriceQuoteUnits.ParValueDecimal;
                                break;
                            case Cst.PriceQuoteUnits.Rate:
                                ret = Cst.PriceQuoteUnits.ParValueDecimal;
                                break;
                            case Cst.PriceQuoteUnits.Price:
                                ret = Cst.PriceQuoteUnits.Price;
                                break;
                        }
                    }
                }
            }
            return ret;
        }
        // EG 20190716 [VCL : New FixedIncome] New
        AssetMeasureEnum ISecurity.GetAssetMeasure(AssetMeasureEnum pAssetMeasure)
        {
            AssetMeasureEnum ret = pAssetMeasure;
            if (quoteRules.accruedInterestIndicatorSpecified && quoteRules.accruedInterestIndicator.BoolValue)
                ret = AssetMeasureEnum.DirtyPrice;
            return ret;
        }
        #endregion ISecurity Members
    }
    #endregion Security
    #region SecurityAsset
    /// EG 20150422 [20513] BANCAPERTA Del issuerSpecified
    /// EG 20150422 [20513] BANCAPERTA New issuerReference Mod issuer
    public partial class SecurityAsset : ISecurityAsset
    {
        #region Members
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool issuerSpecified;
        /// <summary>
        /// Contient la partie qui représente l'émetteur, issu du datadocument debtSecurity (saisie du référentiel Titre)
        /// l'émetteur est le payer du flux d'intérêts, l'acteur SYSTEM étant le receiver
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Party issuer;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        bool idTTemplateSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        int idTTemplate;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        bool isNewAssetSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        Boolean isNewAsset;
        //
        #endregion
        #region Constructor
        public SecurityAsset()
        {
            this.securityId = new EFS_String();
            this.securityName = new EFS_String();
            this.securityDescription = new EFS_String();
            this.securityIssueDate = new EFS_Date();
            this.debtSecurity = new DebtSecurity();
        }
        #endregion Constructor
        #region ISecurityAsset Members
        EFS_String ISecurityAsset.SecurityId
        {
            set { this.securityId = (EFS_String)value; }
            get { return this.securityId; }
        }
        //
        bool ISecurityAsset.SecurityNameSpecified
        {
            get { return this.securityNameSpecified; }
            set { this.securityNameSpecified = value; }
        }
        EFS_String ISecurityAsset.SecurityName
        {
            set { this.securityName = (EFS_String)value; }
            get { return this.securityName; }
        }
        //
        bool ISecurityAsset.SecurityDescriptionSpecified
        {
            get { return this.securityDescriptionSpecified; }
            set { this.securityDescriptionSpecified = value; }
        }
        EFS_String ISecurityAsset.SecurityDescription
        {
            set { this.securityDescription = (EFS_String)value; }
            get { return this.securityDescription; }
        }
        //
        bool ISecurityAsset.SecurityIssueDateSpecified
        {
            get { return this.securityIssueDateSpecified; }
            set { this.securityIssueDateSpecified = value; }
        }
        EFS_Date ISecurityAsset.SecurityIssueDate
        {
            set { this.securityIssueDate = (EFS_Date)value; }
            get { return this.securityIssueDate; }
        }
        //
        bool ISecurityAsset.DebtSecuritySpecified
        {
            get { return this.debtSecuritySpecified; }
            set { this.debtSecuritySpecified = value; }
        }
        IDebtSecurity ISecurityAsset.DebtSecurity
        {
            get { return this.debtSecurity; }
            set { this.debtSecurity = (DebtSecurity)value; }
        }
        //
        //bool ISecurityAsset.issuerSpecified
        //{
        //    get { return this.issuerSpecified; }
        //    set { this.issuerSpecified = value; }
        //}
        IParty ISecurityAsset.Issuer
        {
            get { return this.issuer; }
            set
            {
                this.issuer = (Party)value;
                this.issuerReferenceSpecified = (null != this.issuer);
                this.issuerReference = new PartyOrAccountReference();
                if (this.issuerReferenceSpecified)
                    this.issuerReference.href = this.issuer.id;
            }
        }
        bool ISecurityAsset.IssuerReferenceSpecified
        {
            get { return this.issuerReferenceSpecified; }
            set { this.issuerReferenceSpecified = value; }
        }
        IReference ISecurityAsset.IssuerReference
        {
            get { return this.issuerReference; }
            set { this.issuerReference = (PartyOrAccountReference)value; }
        }
        bool ISecurityAsset.IdTTemplateSpecified
        {
            get { return this.idTTemplateSpecified; }
            set { this.idTTemplateSpecified = value; }
        }
        int ISecurityAsset.IdTTemplate
        {
            get { return this.idTTemplate ; }
            set { this.idTTemplate = value; }
        }
        bool ISecurityAsset.IsNewAssetSpecified
        {
            get { return this.isNewAssetSpecified; }
            set { this.isNewAssetSpecified = value; }
        }
        bool ISecurityAsset.IsNewAsset
        {
            get { return this.isNewAsset; }
            set { this.isNewAsset = value; }
        }
        //
        string ISecurityAsset.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISecurityAsset.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion ISecurityAsset Members
    }
    #endregion SecurityAsset
    #region SecurityAssetReference
    public partial class SecurityAssetReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            get { return this.href; }
            set { this.href = value; }
        }
        #endregion IReference Members
    }
    #endregion SecurityAssetReference
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
        bool ISecurityCalculationRules.FullCouponCalculationRulesSpecified
        {
            set { this.fullCouponCalculationRulesSpecified = value; }
            get { return this.fullCouponCalculationRulesSpecified; }
        }
        IFullCouponCalculationRules ISecurityCalculationRules.FullCouponCalculationRules
        {
            set { this.fullCouponCalculationRules = (FullCouponCalculationRules)value; }
            get { return this.fullCouponCalculationRules; }
        }
        bool ISecurityCalculationRules.AccruedInterestCalculationRulesSpecified
        {
            set { this.accruedInterestCalculationRulesSpecified = value; }
            get { return this.accruedInterestCalculationRulesSpecified; }
        }
        IAccruedInterestCalculationRules ISecurityCalculationRules.AccruedInterestCalculationRules
        {
            set { this.accruedInterestCalculationRules = (AccruedInterestCalculationRules)value; }
            get { return this.accruedInterestCalculationRules; }
        }
        #endregion ISecurityCalculationRules Members
    }
    #endregion SecurityCalculationRules
    #region SecurityExchanges
    public partial class SecurityExchanges : ISecurityExchanges
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        #endregion Accessors
        #region ISecurityExchanges Members
        EFS_Boolean ISecurityExchanges.InitialExchange
        {
            set { this.initialExchange = value; }
            get { return this.initialExchange; }
        }
        EFS_Boolean ISecurityExchanges.FinalExchange
        {
            set { this.finalExchange = value; }
            get { return this.finalExchange; }
        }
        EFS_Boolean ISecurityExchanges.IntermediateExchange
        {
            set { this.intermediateExchange = value; }
            get { return this.intermediateExchange; }
        }
        #endregion ISecurityExchanges Members
    }
    #endregion ISecurityExchanges

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

        bool ISecurityIndicator.CertificatedSpecified
        {
            set { this.certificatedSpecified = value; }
            get { return this.certificatedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Certificated
        {
            set { this.certificated = value; }
            get { return this.certificated; }
        }
        bool ISecurityIndicator.DematerialisedSpecified
        {
            set { this.dematerialisedSpecified = value; }
            get { return this.dematerialisedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Dematerialised
        {
            set { this.dematerialised = value; }
            get { return this.dematerialised; }
        }
        bool ISecurityIndicator.FungibleSpecified
        {
            set { this.fungibleSpecified = value; }
            get { return this.fungibleSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Fungible
        {
            set { this.fungible = value; }
            get { return this.fungible; }
        }
        bool ISecurityIndicator.ImmobilisedSpecified
        {
            set { this.immobilisedSpecified = value; }
            get { return this.immobilisedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Immobilised
        {
            set { this.immobilised = value; }
            get { return this.immobilised; }
        }
        bool ISecurityIndicator.AmortisedSpecified
        {
            set { this.amortisedSpecified = value; }
            get { return this.amortisedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Amortised
        {
            set { this.amortised = value; }
            get { return this.amortised; }
        }
        bool ISecurityIndicator.CallProtectionSpecified
        {
            set { this.callProtectionSpecified = value; }
            get { return this.callProtectionSpecified; }
        }
        EFS_Boolean ISecurityIndicator.CallProtection
        {
            set { this.callProtection = value; }
            get { return this.callProtection; }
        }
        bool ISecurityIndicator.CallableSpecified
        {
            set { this.callableSpecified = value; }
            get { return this.callableSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Callable
        {
            set { this.callable = value; }
            get { return this.callable; }
        }
        bool ISecurityIndicator.PutableSpecified
        {
            set { this.putableSpecified = value; }
            get { return this.putableSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Putable
        {
            set { this.putable = value; }
            get { return this.putable; }
        }
        bool ISecurityIndicator.ConvertibleSpecified
        {
            set { this.convertibleSpecified = value; }
            get { return this.convertibleSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Convertible
        {
            set { this.convertible = value; }
            get { return this.convertible; }
        }
        bool ISecurityIndicator.EscrowedSpecified
        {
            set { this.escrowedSpecified = value; }
            get { return this.escrowedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Escrowed
        {
            set { this.escrowed = value; }
            get { return this.escrowed; }
        }
        bool ISecurityIndicator.PrefundedSpecified
        {
            set { this.prefundedSpecified = value; }
            get { return this.prefundedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Prefunded
        {
            set { this.prefunded = value; }
            get { return this.prefunded; }
        }
        bool ISecurityIndicator.PaymentDirectionSpecified
        {
            set { this.paymentDirectionSpecified = value; }
            get { return this.paymentDirectionSpecified; }
        }
        EFS_Boolean ISecurityIndicator.PaymentDirection
        {
            set { this.paymentDirection = value; }
            get { return this.paymentDirection; }
        }
        bool ISecurityIndicator.QuotedSpecified
        {
            set { this.quotedSpecified = value; }
            get { return this.quotedSpecified; }
        }
        EFS_Boolean ISecurityIndicator.Quoted
        {
            set { this.quoted = value; }
            get { return this.quoted; }
        }
        #endregion ISecurityIndicator Membres
    }
    #endregion SecurityIndicator
    #region SecurityLegReference
    public partial class SecurityLegReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            get { return this.href; }
            set { this.href = value; }
        }
        #endregion IReference Members
    }
    #endregion SecurityLegReference
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
        bool ISecurityOrderRules.PriceUnitsSpecified
        {
            set { this.priceUnitsSpecified = value; }
            get { return this.priceUnitsSpecified; }
        }
        IPriceUnits ISecurityOrderRules.PriceUnits
        {
            set { this.priceUnits = (PriceUnits)value; }
            get { return this.priceUnits; }
        }
        bool ISecurityOrderRules.AccruedInterestIndicatorSpecified
        {
            set { this.accruedInterestIndicatorSpecified = value; }
            get { return this.accruedInterestIndicatorSpecified; }
        }
        EFS_Boolean ISecurityOrderRules.AccruedInterestIndicator
        {
            set { this.accruedInterestIndicator = value; }
            get { return this.accruedInterestIndicator; }
        }
        bool ISecurityOrderRules.PriceInPercentageRoundingSpecified
        {
            set { this.priceInPercentageRoundingSpecified = value; }
            get { return this.priceInPercentageRoundingSpecified; }
        }
        IRounding ISecurityOrderRules.PriceInPercentageRounding
        {
            set { this.priceInPercentageRounding = (Rounding)value; }
            get { return this.priceInPercentageRounding; }
        }
        bool ISecurityOrderRules.PriceInRateRoundingSpecified
        {
            set { this.priceInRateRoundingSpecified = value; }
            get { return this.priceInRateRoundingSpecified; }
        }
        IRounding ISecurityOrderRules.PriceInRateRounding
        {
            set { this.priceInRateRounding = (Rounding)value; }
            get { return this.priceInRateRounding; }
        }
        bool ISecurityOrderRules.QuantityTypeSpecified
        {
            set { this.quantityTypeSpecified = value; }
            get { return this.quantityTypeSpecified; }
        }
        OrderQuantityType3CodeEnum ISecurityOrderRules.QuantityType
        {
            set { this.quantityType = value; }
            get { return this.quantityType; }
        }
        bool ISecurityOrderRules.SettlementDaysOffsetSpecified
        {
            set { this.settlementDaysOffsetSpecified = value; }
            get { return this.settlementDaysOffsetSpecified; }
        }
        IOffset ISecurityOrderRules.SettlementDaysOffset
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
        bool ISecurityPrice.IssuePricePercentageSpecified
        {
            set { this.issuePricePercentageSpecified = value; }
            get { return this.issuePricePercentageSpecified; }
        }
        EFS_Decimal ISecurityPrice.IssuePricePercentage
        {
            set { this.issuePricePercentage = value; }
            get { return this.issuePricePercentage; }
        }
        bool ISecurityPrice.RedemptionPricePercentageSpecified
        {
            set { this.redemptionPricePercentageSpecified = value; }
            get { return this.redemptionPricePercentageSpecified; }
        }
        EFS_Decimal ISecurityPrice.RedemptionPricePercentage
        {
            set { this.redemptionPricePercentage = value; }
            get { return this.redemptionPricePercentage; }
        }
        bool ISecurityPrice.RedemptionPriceFormulaSpecified
        {
            set { this.redemptionPriceFormulaSpecified = value; }
            get { return this.redemptionPriceFormulaSpecified; }
        }
        IFormula ISecurityPrice.RedemptionPriceFormula
        {
            set { this.redemptionPriceFormula = (Formula)value; }
            get { return this.redemptionPriceFormula; }
        }
        bool ISecurityPrice.RedemptionPriceNoneSpecified
        {
            set { this.redemptionPriceNoneSpecified = value; }
            get { return this.redemptionPriceNoneSpecified; }
        }
        IEmpty ISecurityPrice.RedemptionPriceNone
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
        bool ISecurityQuoteRules.QuoteUnitsSpecified
        {
            set { this.quoteUnitsSpecified = value; }
            get { return this.quoteUnitsSpecified; }
        }
        IScheme ISecurityQuoteRules.QuoteUnits
        {
            set { this.quoteUnits = (PriceQuoteUnits)value; }
            get { return (IScheme)this.quoteUnits; }
        }
        bool ISecurityQuoteRules.AccruedInterestIndicatorSpecified
        {
            set { this.accruedInterestIndicatorSpecified = value; }
            get { return this.accruedInterestIndicatorSpecified; }
        }
        EFS_Boolean ISecurityQuoteRules.AccruedInterestIndicator
        {
            set { this.accruedInterestIndicator = value; }
            get { return this.accruedInterestIndicator; }
        }
        bool ISecurityQuoteRules.QuoteRoundingSpecified
        {
            set { this.quoteRoundingSpecified = value; }
            get { return this.quoteRoundingSpecified; }
        }
        IRounding ISecurityQuoteRules.QuoteRounding
        {
            set { this.quoteRounding = (Rounding)value; }
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
        bool ISecurityYield.YieldTypeSpecified
        {
            set { this.yieldTypeSpecified = value; }
            get { return this.yieldTypeSpecified; }
        }
        YieldTypeEnum ISecurityYield.YieldType
        {
            set { this.yieldType = value; }
            get { return this.yieldType; }
        }
        bool ISecurityYield.YieldSpecified
        {
            set { this.yieldSpecified = value; }
            get { return this.yieldSpecified; }
        }
        EFS_Decimal ISecurityYield.Yield
        {
            set { this.yield = value; }
            get { return this.yield; }
        }
        bool ISecurityYield.YieldCalculationDateSpecified
        {
            set { this.yieldCalculationDateSpecified = value; }
            get { return this.yieldCalculationDateSpecified; }
        }
        EFS_Date ISecurityYield.YieldCalculationDate
        {
            set { this.yieldCalculationDate = value; }
            get { return this.yieldCalculationDate; }
        }
        #endregion ISecurityYield Members
    }
    #endregion SecurityYield
}

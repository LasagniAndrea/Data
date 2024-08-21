#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de CciStream. 
    /// </summary>
    /// FI 20190731 [XXXXX] CciStream herite de ContainerCciBase 
    public class CciStream : ContainerCciBase,  IContainerCciFactory,  IContainerCciPayerReceiver, IContainerCciSpecified, ICciPresentation 
    {
        #region private membres
        private string _prefixId; //prefix utilisé pour générer les id (les id doivent être uniques dans un document)   
        private readonly CciTradeBase cciTrade;
        private IInterestRateStream _irs;
        private readonly int number;
        private CciStub cciInitialStub;
        private CciStub cciFinalStub;
        private bool _isSwapReceiverAndPayer;
        #endregion private membres
        
        #region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,

            #region calculationPeriodDates
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.effectiveDate")]
            calculationPeriodDates_effectiveDate,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.effectiveDate.dateAdjustments.businessDayConvention")]
            calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC,
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.terminationDate")]
            calculationPeriodDates_terminationDate,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.terminationDate.dateAdjustments.businessDayConvention")]
            calculationPeriodDates_terminationDate_dateAdjustedDate_bDC,
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.calculationPeriodDatesAdjustments.businessDayConvention")]
            calculationPeriodDates_calculationPeriodDatesAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.firstPeriodStartDate")]
            calculationPeriodDates_firstPeriodStartDate,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.firstPeriodStartDate.dateAdjustments.businessDayConvention")]
            calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.firstRegularPeriodStartDate")]
            calculationPeriodDates_firstRegularPeriodStartDate,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.lastRegularPeriodEndDate")]
            calculationPeriodDates_lastRegularPeriodEndDate,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.calculationPeriodFrequency.periodFrequency")]
            calculationPeriodDates_calculationPeriodFrequency_periodFrequency,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.calculationPeriodFrequency.periodMultiplier")]
            calculationPeriodDates_calculationPeriodFrequency_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.calculationPeriodFrequency.period")]
            calculationPeriodDates_calculationPeriodFrequency_period,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodDates.calculationPeriodFrequency.rollConvention")]
            calculationPeriodDates_calculationPeriodFrequency_rollConvention,
            #endregion

            #region paymentDates
            [System.Xml.Serialization.XmlEnumAttribute("paymentDates.paymentDatesAdjustments.businessDayConvention")]
            paymentDates_paymentDatesAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDates.payRelativeTo")]
            paymentDates_payRelativeToReverse,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDates.firstPaymentDate")]
            paymentDates_firstPaymentDate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDates.paymentDaysOffset")]
            paymentDates_offset,
            #endregion

            #region knownAmountSchedule
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.initialValue")]
            calculationPeriodAmount_knownAmountSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.currency")]
            calculationPeriodAmount_knownAmountSchedule_currency,
            #endregion knownAmountSchedule
            //
            #region notionalStepSchedule
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.initialValue")]
            calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.currency")]
            calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepParameters")]
            calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters,
            #endregion notionalStepSchedule

            #region fxLinkedNotionalStepSchedule
            // 20080512 EG Add FxLinkedNotionalSchedule elements
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.constantNotionalScheduleReference")]
            fxLinkedNotionalSchedule_notionalScheduleReference,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.initialValue")]
            fxLinkedNotionalSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.currency")]
            fxLinkedNotionalSchedule_varyingNotionalCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.primaryRateSource.assetFxRateId")]
            fxLinkedNotionalSchedule_assetFxRate,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.primaryRateSource.rateSource")]
            fxLinkedNotionalSchedule_rateSource,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.primaryRateSource.rateSourcePage")]
            fxLinkedNotionalSchedule_rateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.primaryRateSource.rateSourcePageHeading")]
            fxLinkedNotionalSchedule_rateSourcePageHeading,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.secondaryRateSource.rateSource")]
            fxLinkedNotionalSchedule_secondaryRateSource,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.secondaryRateSource.rateSourcePage")]
            fxLinkedNotionalSchedule_secondaryRateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.secondaryRateSource.rateSourcePageHeading")]
            fxLinkedNotionalSchedule_secondaryRateSourcePageHeading,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.fixingTime.hourMinuteTime")]
            fxLinkedNotionalSchedule_fixingTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.fxSpotRateSource.fixingTime.businessCenter")]
            fxLinkedNotionalSchedule_fixingTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalFixingDates.dateRelativeTo")]
            fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalFixingDates.period")]
            fxLinkedNotionalSchedule_varyingNotionalFixingDates_period,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalFixingDates.periodMultiplier")]
            fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalFixingDates.businessDayConvention")]
            fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalFixingDates.dayType")]
            fxLinkedNotionalSchedule_varyingNotionalFixingDates_dayType,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalInterimExchangePaymentDates.dateRelativeTo")]
            fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalInterimExchangePaymentDates.period")]
            fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalInterimExchangePaymentDates.periodMultiplier")]
            fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalInterimExchangePaymentDates.businessDayConvention")]
            fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.varyingNotionalInterimExchangePaymentDates.dayType")]
            fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dayType,
            #endregion fxLinkedNotionalStepSchedule
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.dayCountFraction")]
            calculationPeriodAmount_calculation_dayCountFraction,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFixedRate")]
            calculationPeriodAmount_calculation_rate,
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFixedRate")]
            calculationPeriodAmount_calculation_fixedRateSchedule_initialValue,
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.floatingRateMultiplierSchedule.initialValue")]
            calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate.spreadSchedule.initialValue")]
            calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate")]
            calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate")]
            calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate")]
            calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRate")]
            calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue,
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateInflationRate")]
            calculationPeriodAmount_calculation_inflationRate,
            //
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateInflationRate.floatingRateMultiplierSchedule.initialValue")]
            calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateInflationRate.spreadSchedule.initialValue")]
            calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateInflationRate")]
            calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateInflationRate")]
            calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue,
            [System.Xml.Serialization.XmlEnumAttribute("calculationPeriodAmount.calculationPeriodAmountCalculation.rateInflationRate")]
            calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue,
            //
            [System.Xml.Serialization.XmlEnumAttribute("resetDates.ResetRelativeTo")]
            resetDates_resetRelativeTo,
            //
            [System.Xml.Serialization.XmlEnumAttribute("principalExchanges")]
            principalExchanges,
            unknown,

        }

        #endregion
        
        #region property
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        
        /// <summary>
        /// Retrourne true si le container cciInitialStub a été instancié
        /// </summary>
        public bool ExistInitialStub
        {
            get { return (null != cciInitialStub); }
        }
        
        /// <summary>
        /// Retrourne true si le container cciFinalStub a été instancié
        /// </summary>
        public bool ExistFinalStub
        {
            get { return (null != cciFinalStub); }
        }
        
        /// <summary>
        /// Retourne true si le Cci calculationPeriodAmount_calculation_rate a été affecté avec une donnée valide
        /// </summary>
        private bool IsFloatingRateValid
        {
            get
            {
                bool ret = false;
                if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_rate)))
                    ret = (null != Cci(CciEnum.calculationPeriodAmount_calculation_rate).Sql_Table);
                return ret;
            }
        }
        
        /// <summary>
        /// Retourne true si le Cci calculationPeriodAmount_calculation_inflationRate a été affecté avec une donnée valide
        /// </summary>
        private bool IsInflationRateValid
        {
            get
            {
                bool ret = false;
                if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRate)))
                    ret = (null != Cci(CciEnum.calculationPeriodAmount_calculation_inflationRate).Sql_Table);
                return ret;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < number)
                    ret = number.ToString();
                return ret;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private int PreviousNumber
        {
            get
            {
                int ret = 0;
                if (0 < number)
                    ret = number - 1;
                return ret;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private string PreviousNumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < PreviousNumber )
                    ret = PreviousNumber.ToString();
                return ret;
            }
        }
        
        /// <summary>
        /// Obtient ou définit l'InterestRateStream géré 
        /// </summary>
        public IInterestRateStream Irs
        {
            get { return _irs; }
            set { _irs = value; }
        }
        
        /// <summary>
        /// Ontient le taux Flottant de l'irs
        /// </summary>
        public IFloatingRate FloatingRate
        {
            get
            {
                IFloatingRate rate = null;
                if (IsFloatingRateSpecified)
                    rate = Irs.CalculationPeriodAmount.Calculation.RateFloatingRate;
                if (IsInflationRateSpecified)
                    rate = Irs.CalculationPeriodAmount.Calculation.RateInflationRate;
                return rate;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsCalculationSpecified
        {
            get
            {
                return Irs.CalculationPeriodAmount.CalculationSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFloatingRateSpecified
        {
            get
            {
                return IsCalculationSpecified && Irs.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsInflationRateSpecified
        {
            get
            {
                return IsCalculationSpecified && Irs.CalculationPeriodAmount.Calculation.RateInflationRateSpecified;

            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFxLinkedNotionalScheduleSpecified
        {
            get
            {
                return IsCalculationSpecified && Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFixedRateSpecified
        {
            get
            {
                //ne pas ecrire bret = (false == IsFloatingRateSpecified)
                // On ne test pas les mêmes elements FpML
                return IsCalculationSpecified && Irs.CalculationPeriodAmount.Calculation.RateFixedRateSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsCalculationNotionalSpecified
        {
            get
            {
                return IsCalculationSpecified && Irs.CalculationPeriodAmount.Calculation.NotionalSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsKnownAmountScheduleStepSpecified
        {
            get
            {
                return Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified &&
                       Irs.CalculationPeriodAmount.KnownAmountSchedule.StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsNotionalStepScheduleStepSpecified
        {
            get
            {
                return IsCalculationNotionalSpecified &&
                       Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsNotionalStepParametersSpecified
        {
            get
            {
                return IsCalculationNotionalSpecified &&
                       Irs.CalculationPeriodAmount.Calculation.Notional.StepParametersSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFixedRateScheduleSpecified
        {
            get
            {
                return (false == IsFloatingRateSpecified) && (false == IsInflationRateSpecified) &&
                        Irs.CalculationPeriodAmount.Calculation.RateFixedRate.StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsCapRateScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.CapRateScheduleSpecified && rate.CapRateSchedule[0].StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFloorRateScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.FloorRateScheduleSpecified && rate.FloorRateSchedule[0].StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFloatingRateMultiplierScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.FloatingRateMultiplierScheduleSpecified && rate.FloatingRateMultiplierSchedule.StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsSpreadScheduleSpecified
        {
            get
            {
                IFloatingRate rate = FloatingRate;
                return (null != rate) && rate.SpreadScheduleSpecified && rate.SpreadSchedule.StepSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsOffsetSpecified
        {
            get
            {
                return Irs.PaymentDates.PaymentDaysOffsetSpecified;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string PrefixId
        {
            get
            {
                return _prefixId;
            }
            set
            {
                _prefixId = value;
            }
        }

        /// <summary>
        /// Obtient ou définit si l'affectation du payer peut impacter le receiver et vis et versa
        /// <para>
        /// <example>
        /// <para>Exemple si isSwapReceiverAndPayer =true</para>
        /// <para>soit le payer A et Receiver B, si le payer devient B alors le receiver deviendra A</para>
        /// </example> 
        /// </para>
        /// </summary>
        public bool IsSwapReceiverAndPayer
        {
            get { return _isSwapReceiverAndPayer; }
            set { _isSwapReceiverAndPayer = value; }
        }
        #endregion property
        
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pStreamNumber"></param>
        /// <param name="pIrs"></param>
        /// FI 20190731 [XXXXX] Appel constructor de la classe de base
        public CciStream(CciTradeBase pTrade, string pPrefix, int pStreamNumber, IInterestRateStream pIrs)
            : base(pPrefix, pStreamNumber, pTrade.Ccis)
        {
            cciTrade = pTrade;
            number = pStreamNumber;  // Use property Number
            Irs = pIrs;
            _prefixId = string.Empty;
            _isSwapReceiverAndPayer = true;
        }
        #endregion Constructor
        
        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, Irs);

            if (null == Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments)
                Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments = Irs.CreateBusinessDayAdjustments();
            //
            if (null == Irs.ResetDates)
                Irs.ResetDates = Irs.CreateResetDates();
            if (null == Irs.ResetDates.ResetDatesAdjustments)
                Irs.ResetDates.ResetDatesAdjustments = Irs.CreateBusinessDayAdjustments();
            //
            if (null == Irs.ResetDates.FixingDates)
                Irs.ResetDates.FixingDates = Irs.CreateRelativeDateOffset();
            //				
            if (null == Irs.ResetDates.ResetFrequency)
                Irs.ResetDates.ResetFrequency = Irs.CreateResetFrequency();
            if (null == Irs.ResetDates.ResetFrequency.Interval.PeriodMultiplier)
                Irs.ResetDates.ResetFrequency.Interval.PeriodMultiplier = new EFS_Integer();
            //
            if (null == Irs.PaymentDates.PaymentFrequency)
                Irs.PaymentDates.PaymentFrequency = Irs.CreateInterval();
            if (null == Irs.PaymentDates.PaymentFrequency.PeriodMultiplier)
                Irs.PaymentDates.PaymentFrequency.PeriodMultiplier = new EFS_Integer();
            //
            if (null == Irs.PaymentDates.PaymentDaysOffset)
                Irs.PaymentDates.PaymentDaysOffset = Irs.CreateOffset();
            if (null == Irs.PaymentDates.PaymentDaysOffset.PeriodMultiplier)
                Irs.PaymentDates.PaymentDaysOffset.PeriodMultiplier = new EFS_Integer();
            IFloatingRate floatingRate;
            //
            // Initialisation par défaut, pour que lorsque que l'on saisie le taux que toutes les pré-propositions soient effectuées
            if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_rate))
                ||
                CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue))
                ||
                CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue))
                )
            {
                Irs.CalculationPeriodAmount.CalculationSpecified = true;
                //
                #region FloatingRate
                floatingRate = Irs.CalculationPeriodAmount.Calculation.RateFloatingRate;
                if (null == floatingRate.SpreadSchedule)
                    floatingRate.SpreadSchedule = floatingRate.CreateSpreadSchedule();
                //
                if (null == floatingRate.CapRateSchedule)
                {
                    if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue)))
                        floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(2);
                    else
                        floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(1);
                }
                else if (ArrFunc.Count(floatingRate.CapRateSchedule) == 1 && (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue))))
                {
                    IStrikeSchedule[] capRateSav = (IStrikeSchedule[])floatingRate.CapRateSchedule.Clone();
                    //
                    floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(2);
                    floatingRate.CapRateSchedule[0] = capRateSav[0];
                }
                //
                if (null == floatingRate.FloorRateSchedule)
                    floatingRate.FloorRateSchedule = floatingRate.CreateStrikeSchedule(1);
                #endregion FloatingRate
            }
            if ((CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRate)))
                ||
                (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue)))
                ||
                (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue)))
            )
            {
                #region InflationRate
                floatingRate = Irs.CalculationPeriodAmount.Calculation.RateInflationRate;
                if (null == floatingRate.SpreadSchedule)
                    floatingRate.SpreadSchedule = floatingRate.CreateSpreadSchedule();
                //
                if (null == floatingRate.CapRateSchedule)
                {
                    if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue)))
                        floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(2);
                    else
                        floatingRate.CapRateSchedule = floatingRate.CreateStrikeSchedule(1);
                }
                //
                if (null == floatingRate.FloorRateSchedule)
                    floatingRate.FloorRateSchedule = floatingRate.CreateStrikeSchedule(1);
                #endregion InflationRate
            }

            #region initial Stub
            CciStub cciStub;
            cciStub = new CciStub(cciTrade, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_initialStub, Irs);
            bool isOk = cciStub.IsCciMasterSpecified;
            //
            // 20090618 RD Code remplacé par la property CciStub.IsCciMasterSpecified 
            //                
            //isOk = ccis.Contains(cciStub.CciClientId(CciStub.CciEnum.rate));
            //isOk = isOk || ccis.Contains(cciStub.CciClientId(CciStub.CciEnum.floatingRate1));
            //isOk = isOk || ccis.Contains(cciStub.CciClientId(CciStub.CciEnum.stubAmount_amount));
            //
            if (isOk)
            {
                if (null == Irs.StubCalculationPeriodAmount)
                    Irs.StubCalculationPeriodAmount = Irs.CreateStubCalculationPeriodAmount();
                //
                if (StrFunc.IsEmpty(Irs.StubCalculationPeriodAmount.CalculationPeriodDatesReference.HRef))
                {
                    //20061129 PL Bug on stub rate on stream 2
                    //Irs.stubCalculationPeriodAmount.calculationPeriodDatesReference.href = Irs.calculationPeriodDates.id;  
                    Irs.StubCalculationPeriodAmount.CalculationPeriodDatesReference.HRef = GetCalculationPeriodDatesId();
                }
                //
                if (null == Irs.StubCalculationPeriodAmount.InitialStub)
                    Irs.StubCalculationPeriodAmount.InitialStub = Irs.StubCalculationPeriodAmount.CreateStub;
                //
                cciInitialStub = new CciStub(cciTrade, Irs.StubCalculationPeriodAmount.InitialStub, Prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_initialStub, Irs);
                cciInitialStub.Initialize_FromCci();
            }
            #endregion initial Stub

            #region finalStub
            cciStub = new CciStub(cciTrade, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_finalStub, Irs);
            isOk = cciStub.IsCciMasterSpecified;
            //
            // 20090618 RD Code remplacé par la property CciStub.IsCciMasterSpecified 
            //
            //isOk = ccis.Contains(cciStub.CciClientId(CciStub.CciEnum.rate));
            //isOk = isOk || ccis.Contains(cciStub.CciClientId(CciStub.CciEnum.floatingRate1));
            //isOk = isOk || ccis.Contains(cciStub.CciClientId(CciStub.CciEnum.stubAmount_amount));
            //
            if (isOk)
            {
                if (null == Irs.StubCalculationPeriodAmount)
                    Irs.StubCalculationPeriodAmount = Irs.CreateStubCalculationPeriodAmount();
                //
                if (StrFunc.IsEmpty(Irs.StubCalculationPeriodAmount.CalculationPeriodDatesReference.HRef))
                {
                    //20061129 PL Bug on stub rate on stream 2
                    //Irs.stubCalculationPeriodAmount.calculationPeriodDatesReference.href = Irs.calculationPeriodDates.id;
                    Irs.StubCalculationPeriodAmount.CalculationPeriodDatesReference.HRef = GetCalculationPeriodDatesId();
                }
                //
                if (null == Irs.StubCalculationPeriodAmount.FinalStub)
                    Irs.StubCalculationPeriodAmount.FinalStub = Irs.StubCalculationPeriodAmount.CreateStub;
                //
                cciFinalStub = new CciStub(cciTrade, Irs.StubCalculationPeriodAmount.FinalStub, Prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_finalStub, Irs);
                cciFinalStub.Initialize_FromCci();
            }
            #endregion finalStub

            // Initialisation par défaut, pour que lorsque que l'on saisie le taux du FxLinked (FxSpotRateSource)
            // toutes les pré-propositions soient effectuées (varying
            if (CcisBase.Contains(CciClientId(CciEnum.fxLinkedNotionalSchedule_assetFxRate)))
            {
                if (IsFxLinkedNotionalScheduleSpecified)
                {
                    IFxLinkedNotionalSchedule fxLinkedNotional = Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional;
                    if (null == fxLinkedNotional.VaryingNotionalFixingDates)
                        fxLinkedNotional.VaryingNotionalFixingDates = Irs.CreateRelativeDateOffset();
                    if (null == fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates)
                        fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates = Irs.CreateRelativeDateOffset();
                }
            }

        }
        
        /// <summary>
        /// Adding missing controls that are necessary for process intilialize
        /// (Payer et Receiver du stream)
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            ArrayList PayersReceivers = new ArrayList
            {
                CciClientIdPayer,
                CciClientIdReceiver
            };
            IEnumerator ListEnum = PayersReceivers.GetEnumerator();

            string clientId_WithoutPrefix;
            while (ListEnum.MoveNext())
            {
                clientId_WithoutPrefix = ListEnum.Current.ToString();
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            CcisBase[CciClientIdReceiver].IsMandatory = CcisBase[CciClientIdPayer].IsMandatory;

            //Nécessaire pour le recalcul de RollConvention
            clientId_WithoutPrefix = CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);

            clientId_WithoutPrefix = CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);

            if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodDates_firstPeriodStartDate)))
            {
                //Nécessaire pour le recalcul des BCs asscoiés au FirstPeriodeStartDate
                clientId_WithoutPrefix = CciClientId(CciEnum.calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC);
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }

            //used for cciTradeSwap.SynchronizeBDAFixedRate()
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.paymentDates_paymentDatesAdjustments_bDC), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue.ToString()), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.paymentDates_offset), false, TypeData.TypeDataEnum.@string);

            // si straddleRateSchedule spécifié alors ajout des cci capRateSchedule et floorRateSchedule
            if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue)))
            {
                CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue), true, TypeData.TypeDataEnum.@decimal);
                CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue), true, TypeData.TypeDataEnum.@decimal);
            }

            if (ExistInitialStub)
                cciInitialStub.AddCciSystem();
            if (ExistFinalStub)
                cciFinalStub.AddCciSystem();
        }
        
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    Boolean isSetting = true;
                    Boolean isToValidate = false;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    
                    switch (cciEnum)
                    {
                        #region Payer/Receiver
                        case CciEnum.payer:
                            #region Payer
                            data = Irs.PayerPartyReference.HRef;
                            #endregion Payer
                            break;
                        case CciEnum.receiver:
                            #region Receiver
                            data = Irs.ReceiverPartyReference.HRef;
                            #endregion Receiver
                            break;
                        #endregion Payer/Receiver

                        #region ResetDates
                        case CciEnum.resetDates_resetRelativeTo:
                            #region ResetRelativeTo
                            if (Irs.ResetDatesSpecified && Irs.ResetDates.ResetRelativeToSpecified)
                                data = Irs.ResetDates.ResetRelativeTo.ToString();
                            #endregion ResetRelativeTo
                            break;
                        #endregion ResetDates

                        #region CalculationPeriodDates
                        case CciEnum.calculationPeriodDates_effectiveDate:
                            #region EffectiveDate
                            if (Irs.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                                data = Irs.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value;
                            #endregion EffectiveDate
                            break;
                        case CciEnum.calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC:
                            #region EffectiveDate (BusinessDayConvention)
                            if (Irs.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                                data = Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessDayConvention.ToString();
                            #endregion EffectiveDate (BusinessDayConvention)
                            break;
                        case CciEnum.calculationPeriodDates_terminationDate:
                            #region TerminationDate
                            if (Irs.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                                data = Irs.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
                            #endregion TerminationDate
                            break;
                        case CciEnum.calculationPeriodDates_terminationDate_dateAdjustedDate_bDC:
                            #region TerminationDate (BusinessDayConvention)
                            if (Irs.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                                data = Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessDayConvention.ToString();
                            #endregion TerminationDate (BusinessDayConvention)
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC:
                            #region CalculationPeriodDates (BusinessDayConvention)
                            data = Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                            #endregion CalculationPeriodDates (BusinessDayConvention)
                            break;
                        case CciEnum.calculationPeriodDates_firstRegularPeriodStartDate:
                            #region FirstRegularPeriodStartDate
                            if (Irs.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                                data = Irs.CalculationPeriodDates.FirstRegularPeriodStartDate.Value;
                            #endregion FirstRegularPeriodStartDatebreak;
                            break;
                        case CciEnum.calculationPeriodDates_lastRegularPeriodEndDate:
                            #region LastRegularPeriodEndDate
                            if (Irs.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                                data = Irs.CalculationPeriodDates.LastRegularPeriodEndDate.Value;
                            #endregion LastRegularPeriodEndDate
                            break;
                        case CciEnum.calculationPeriodDates_firstPeriodStartDate:
                            #region FirstPeriodStartDate
                            if (Irs.CalculationPeriodDates.FirstPeriodStartDateSpecified)
                                data = Irs.CalculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.Value;
                            #endregion FirstPeriodStartDate
                            break;
                        case CciEnum.calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC:
                            #region FirstPeriodStartDate (BusinessDayConvention)
                            if (Irs.CalculationPeriodDates.FirstPeriodStartDateSpecified)
                                data = Irs.CalculationPeriodDates.FirstPeriodStartDate.DateAdjustments.BusinessDayConvention.ToString();
                            #endregion FirstPeriodStartDate (BusinessDayConvention)
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency:
                            #region CalculationFrequency (PeriodFrequency)
                            //
                            string periodMultiplier = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value;
                            string period = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period.ToString();
                            //
                            if (StrFunc.IsEmpty(periodMultiplier) && StrFunc.IsEmpty(period))
                            {
                                periodMultiplier = "0";
                                period = PeriodEnum.D.ToString();
                            }
                            else if (StrFunc.IsEmpty(periodMultiplier))
                                periodMultiplier = "1";
                            else if (StrFunc.IsEmpty(period))
                                period = PeriodEnum.M.ToString();
                            //
                            //20090514 RD/PL 
                            PeriodTofrequency.Translate(ref periodMultiplier, ref period);
                            //20091006 FI mise en commentaire Il ne  faut pas affecter le NewValue et laisser le LastValue à Empty
                            //car on rentre dans les procédure Dump et processinitaialzie (même lorsque l'on est en consultation)
                            //ccis.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), periodMultiplier);
                            //ccis.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period), period);
                            //
                            data = periodMultiplier + period;
                            #endregion CalculationFrequency (PeriodFrequency)
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier:
                            #region CalculationFrequency (Multiplier)
                            //20091006 FI mise en commentaire (voir les commentaires à la même date)
                            //if (false == ccis.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency)))
                            data = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value;
                            //else
                            //isSetting = false;
                            #endregion CalculationFrequency (Multiplier)
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_period:
                            #region CalculationFrequency (Period)
                            //20091006 FI mise en commentaire (voir les commentaires à la même date)
                            //if (false == ccis.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency)))
                            data = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period.ToString();
                            //else
                            //    isSetting = false;
                            #endregion CalculationFrequency (Period)
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_rollConvention:
                            #region CalculationFrequency (RollConvention)
                            data = Irs.CalculationPeriodDates.CalculationPeriodFrequency.RollConvention.ToString();
                            #endregion CalculationFrequency (RollConvention)
                            break;
                        #endregion CalculationPeriodDates

                        #region PaymentDates
                        case CciEnum.paymentDates_paymentDatesAdjustments_bDC:
                            #region BusinessDayConvention
                            data = Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
                            #endregion BusinessDayConvention
                            break;
                        case CciEnum.paymentDates_payRelativeToReverse:
                            #region PayRelativeToReverse
                            data = (PayRelativeToEnum.CalculationPeriodStartDate == Irs.PaymentDates.PayRelativeTo) ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                            #endregion PayRelativeToReverse
                            break;
                        case CciEnum.paymentDates_firstPaymentDate:
                            #region firstPaymentDate
                            if (Irs.PaymentDates.FirstPaymentDateSpecified)
                                data = Irs.PaymentDates.FirstPaymentDate.Value;
                            #endregion firstPaymentDate
                            break;
                        #endregion PaymentDates

                        #region KnownAmountSchedule
                        case CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                            #region InitialValue
                            if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                                data = Irs.CalculationPeriodAmount.KnownAmountSchedule.InitialValue.Value;
                            #endregion InitialValue
                            break;
                        case CciEnum.calculationPeriodAmount_knownAmountSchedule_currency:
                            #region Currency
                            if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                                data = Irs.CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;
                            #endregion Currency
                            break;
                        #endregion KnownAmountSchedule

                        #region NotionalStepSchedule
                        case CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                            #region InitialValue
                            if (Irs.CalculationPeriodAmount.CalculationSpecified &&
                                Irs.CalculationPeriodAmount.Calculation.NotionalSpecified)
                                data = Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.InitialValue.Value;
                            #endregion InitialValue
                            break;
                        case CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency:
                            #region Currency
                            if (Irs.CalculationPeriodAmount.CalculationSpecified &&
                                Irs.CalculationPeriodAmount.Calculation.NotionalSpecified)
                                data = Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.Currency.Value;
                            #endregion Currency
                            break;
                        #endregion NotionalStepSchedule

                        #region FxLinkedNotionalSchedule
                        case CciEnum.fxLinkedNotionalSchedule_notionalScheduleReference:
                        case CciEnum.fxLinkedNotionalSchedule_initialValue:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency:
                        case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                        case CciEnum.fxLinkedNotionalSchedule_rateSource:
                        case CciEnum.fxLinkedNotionalSchedule_rateSourcePage:
                        case CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading:
                        case CciEnum.fxLinkedNotionalSchedule_secondaryRateSource:
                        case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage:
                        case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading:
                        case CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime:
                        case CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_period:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dayType:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dayType:
                            if (Irs.CalculationPeriodAmount.CalculationSpecified && Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified)
                            {
                                IFxLinkedNotionalSchedule fxLinkedNotional = Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional;
                                switch (cciEnum)
                                {
                                    case CciEnum.fxLinkedNotionalSchedule_notionalScheduleReference:
                                        #region NotionalReference
                                        data = fxLinkedNotional.ConstantNotionalScheduleReference.HRef;
                                        break;
                                        #endregion NotionalReference
                                    case CciEnum.fxLinkedNotionalSchedule_initialValue:
                                        #region initialValue
                                        if (fxLinkedNotional.InitialValueSpecified)
                                            data = fxLinkedNotional.InitialValue.Value;
                                        break;
                                        #endregion initialValue
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency:
                                        #region Currency
                                        data = fxLinkedNotional.Currency;
                                        break;
                                        #endregion Currency
                                    case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                                    case CciEnum.fxLinkedNotionalSchedule_rateSource:
                                    case CciEnum.fxLinkedNotionalSchedule_rateSourcePage:
                                    case CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading:
                                    case CciEnum.fxLinkedNotionalSchedule_secondaryRateSource:
                                    case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage:
                                    case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading:
                                    case CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime:
                                    case CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter:
                                        #region FxSpotRateSource
                                        IFxSpotRateSource source = fxLinkedNotional.FxSpotRateSource;
                                        switch (cciEnum)
                                        {
                                            case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                                                #region AssetFxRate
                                                try
                                                {
                                                    int idAsset = source.PrimaryRateSource.OTCmlId;
                                                    if (0 < idAsset)
                                                    {
                                                        SQL_AssetFxRate sql_AssetFxRate = new SQL_AssetFxRate(cciTrade.CSCacheOn, idAsset);
                                                        if (sql_AssetFxRate.IsLoaded)
                                                        {
                                                            cci.Sql_Table = sql_AssetFxRate;
                                                            data = sql_AssetFxRate.Identifier;
                                                            source.PrimaryRateSource.SetAssetFxRateId(idAsset, data);
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    cci.Sql_Table = null;
                                                    data = string.Empty;
                                                }
                                                #endregion assetFxRate
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_rateSource:
                                                #region RateSource
                                                data = source.PrimaryRateSource.RateSource.Value;
                                                #endregion RateSource
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_rateSourcePage:
                                                #region RateSourcePage
                                                if (source.PrimaryRateSource.RateSourcePageSpecified)
                                                    data = source.PrimaryRateSource.RateSourcePage.Value;
                                                #endregion RateSourcePage
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading:
                                                #region RateSourcePageHeading
                                                if (source.PrimaryRateSource.RateSourcePageHeadingSpecified)
                                                    data = source.PrimaryRateSource.RateSourcePageHeading;
                                                #endregion RateSourcePageHeading
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_secondaryRateSource:
                                                #region Secondary RateSource
                                                if (source.SecondaryRateSourceSpecified)
                                                    data = source.SecondaryRateSource.RateSource.Value;
                                                #endregion Secondary RateSource
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage:
                                                #region Secondary RateSourcePage
                                                if (source.SecondaryRateSourceSpecified &&
                                                    source.SecondaryRateSource.RateSourcePageSpecified)
                                                    data = source.SecondaryRateSource.RateSourcePage.Value;
                                                #endregion Secondary RateSourcePage
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading:
                                                #region Secondary RateSourcePageHeading
                                                if (source.SecondaryRateSourceSpecified &&
                                                    source.SecondaryRateSource.RateSourcePageHeadingSpecified)
                                                    data = source.SecondaryRateSource.RateSourcePageHeading;
                                                #endregion Secondary RateSourcePageHeading
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime:
                                                #region FixingTime (HourMinuteTime)
                                                data = fxLinkedNotional.FxSpotRateSource.FixingTime.HourMinuteTime.Value;
                                                #endregion FixingTime (HourMinuteTime)
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter:
                                                #region FixingTime (BusinessCenter)
                                                data = fxLinkedNotional.FxSpotRateSource.FixingTime.BusinessCenter.Value;
                                                #endregion FixingTime (BusinessCenter)
                                                break;
                                            default:
                                                isSetting = false;
                                                break;
                                        }
                                        #endregion FxSpotRateSource
                                        break;
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_period:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dayType:
                                        #region VaryingNotionalFixingDates
                                        IRelativeDateOffset fixingOffset = fxLinkedNotional.VaryingNotionalFixingDates;
                                        switch (cciEnum)
                                        {
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo:
                                                #region DateRelativeTo
                                                data = fixingOffset.DateRelativeToValue;
                                                #endregion DateRelativeTo
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_period:
                                                #region Period
                                                data = fixingOffset.Period.ToString();
                                                #endregion Period
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier:
                                                #region PeriodMultiplier
                                                data = fixingOffset.PeriodMultiplier.Value;
                                                #endregion PeriodMultiplier
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC:
                                                #region BusinessDayConvention
                                                data = fixingOffset.BusinessDayConvention.ToString();
                                                #endregion BusinessDayConvention
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dayType:
                                                #region DayType
                                                if (fixingOffset.DayTypeSpecified)
                                                    data = fixingOffset.DayType.ToString();
                                                #endregion DayType
                                                break;
                                        }
                                        #endregion VaryingNotionalFixingDates
                                        break;
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dayType:
                                        #region VaryingNotionalInterimExchangePaymentDates
                                        IRelativeDateOffset paymentOffset = fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates;
                                        switch (cciEnum)
                                        {
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo:
                                                #region DateRelativeTo
                                                data = paymentOffset.DateRelativeToValue;
                                                #endregion DateRelativeTo
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period:
                                                #region Period
                                                data = paymentOffset.Period.ToString();
                                                #endregion Period
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier:
                                                #region PeriodMultiplier
                                                data = paymentOffset.PeriodMultiplier.Value;
                                                #endregion PeriodMultiplier
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC:
                                                #region BusinessDayConvention
                                                data = paymentOffset.BusinessDayConvention.ToString();
                                                #endregion BusinessDayConvention
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dayType:
                                                #region DayType
                                                if (paymentOffset.DayTypeSpecified)
                                                    data = paymentOffset.DayType.ToString();
                                                #endregion DayType
                                                break;
                                        }
                                        #endregion VaryingNotionalInterimExchangePaymentDates
                                        break;
                                    default:
                                        isSetting = false;
                                        break;
                                }
                            }
                            break;
                        #endregion FxLinkedNotionalSchedule

                        #region RateFixedRate / RateFloatingRate / RateInflationRate
                        case CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                            #region RateFixedRate
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                            {
                                ICalculation calculation = Irs.CalculationPeriodAmount.Calculation;
                                if (calculation.RateFixedRateSpecified)
                                    data = calculation.RateFixedRate.InitialValue.Value;
                            }
                            #endregion
                            break;

                        case CciEnum.calculationPeriodAmount_calculation_rate:
                            #region Rate
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                            {
                                ICalculation calculation = Irs.CalculationPeriodAmount.Calculation;
                                if (calculation.RateFixedRateSpecified)
                                {
                                    #region FixedRate
                                    data = calculation.RateFixedRate.InitialValue.Value;
                                    #endregion FixedRate
                                }
                                else if (calculation.RateFloatingRateSpecified)
                                {
                                    #region FloatingRate
                                    try
                                    {
                                        int idAsset = calculation.RateFloatingRate.FloatingRateIndex.OTCmlId;
                                        SQL_AssetRateIndex sql_RateIndex = null;
                                        if (idAsset > 0)
                                        {
                                            sql_RateIndex = new SQL_AssetRateIndex(cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                                        }
                                        else
                                        {
                                            if (StrFunc.IsFilled(calculation.RateFloatingRate.FloatingRateIndex.Value))
                                            {
                                                //20090619 PL TODO MultiSearch
                                                sql_RateIndex = new SQL_AssetRateIndex(cciTrade.CSCacheOn, SQL_AssetRateIndex.IDType.Asset_Identifier, calculation.RateFloatingRate.FloatingRateIndex.Value.Replace(" ", "%") + "%");
                                                //
                                                if (calculation.RateFloatingRate.IndexTenorSpecified)
                                                {
                                                    sql_RateIndex.Asset_PeriodMltp_In = calculation.RateFloatingRate.IndexTenor.PeriodMultiplier.IntValue;
                                                    sql_RateIndex.Asset_Period_In = calculation.RateFloatingRate.IndexTenor.Period.ToString();
                                                }
                                            }
                                        }
                                        //
                                        if ((null != sql_RateIndex) && sql_RateIndex.IsLoaded)
                                        {
                                            sql_Table = sql_RateIndex;
                                            data = sql_RateIndex.Identifier;
                                            isToValidate = (idAsset == 0);
                                        }
                                    }
                                    catch
                                    {
                                        sql_Table = null;
                                        data = string.Empty;
                                    }
                                    #endregion FloatingRate
                                }
                            }
                            #endregion Rate
                            break;

                        case CciEnum.calculationPeriodAmount_calculation_inflationRate:
                            #region InflationRate
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                            {
                                ICalculation calculation = Irs.CalculationPeriodAmount.Calculation;
                                if (calculation.RateInflationRateSpecified)
                                {
                                    try
                                    {
                                        int idAsset = calculation.RateInflationRate.FloatingRateIndex.OTCmlId;
                                        SQL_AssetIndex sql_Index = null;
                                        if (0 < idAsset)
                                            sql_Index = new SQL_AssetIndex(cciTrade.CSCacheOn, idAsset);
                                        else if (StrFunc.IsFilled(calculation.RateInflationRate.FloatingRateIndex.Value))
                                            sql_Index = new SQL_AssetIndex(cciTrade.CSCacheOn, SQL_AssetIndex.IDType.Identifier, calculation.RateInflationRate.FloatingRateIndex.Value.Replace(" ", "%") + "%");
                                        //
                                        if ((null != sql_Index) && sql_Index.IsLoaded)
                                        {
                                            sql_Table = sql_Index;
                                            data = sql_Index.Identifier;
                                            isToValidate = (0 == idAsset);
                                        }
                                    }
                                    catch
                                    {
                                        sql_Table = null;
                                        data = string.Empty;
                                    }
                                }
                            }
                            #endregion InflationRate
                            break;

                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue:
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                            {
                                ICalculation calculation = Irs.CalculationPeriodAmount.Calculation;
                                IFloatingRate floatingRate = null;
                                if (false == calculation.RateFixedRateSpecified)
                                {
                                    if (calculation.RateFloatingRateSpecified)
                                        floatingRate = calculation.RateFloatingRate;
                                    else if (calculation.RateInflationRateSpecified)
                                        floatingRate = calculation.RateInflationRate;
                                }
                                //
                                if (null != floatingRate)
                                {
                                    switch (cciEnum)
                                    {
                                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                            #region MultiplierSchedule
                                            if ((null != floatingRate) && floatingRate.FloatingRateMultiplierScheduleSpecified)
                                                data = floatingRate.FloatingRateMultiplierSchedule.InitialValue.Value;
                                            #endregion MultiplierSchedule
                                            break;
                                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue:
                                            #region SpreadSchedule
                                            if ((null != floatingRate) && floatingRate.SpreadScheduleSpecified)
                                                data = floatingRate.SpreadSchedule.InitialValue.Value;
                                            #endregion SpreadSchedule
                                            break;
                                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue:
                                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue:
                                            #region CapRate
                                            if ((null != floatingRate) && floatingRate.CapRateScheduleSpecified)
                                                data = floatingRate.CapRateSchedule[0].InitialValue.Value;
                                            #endregion CapRate
                                            break;
                                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue:
                                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue:
                                            #region CapRate
                                            if ((null != floatingRate) && floatingRate.CapRateScheduleSpecified)
                                                data = floatingRate.CapRateSchedule[1].InitialValue.Value;
                                            #endregion CapRate
                                            break;

                                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue:
                                            #region FloorRate
                                            if ((null != floatingRate) && floatingRate.FloorRateScheduleSpecified)
                                                data = floatingRate.FloorRateSchedule[0].InitialValue.Value;
                                            #endregion FloorRate
                                            break;
                                        default:
                                            isSetting = false;
                                            break;
                                    }
                                }
                            }
                            break;
                        #endregion RateFixedRate / RateFloatingRate / RateInflationRate

                        #region Others
                        case CciEnum.calculationPeriodAmount_calculation_dayCountFraction:
                            #region DayCountFraction
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                                data = Irs.CalculationPeriodAmount.Calculation.DayCountFraction.ToString();
                            else if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                            {
                                if (Irs.CalculationPeriodAmount.KnownAmountSchedule.DayCountFractionSpecified)
                                    data = Irs.CalculationPeriodAmount.KnownAmountSchedule.DayCountFraction.ToString();
                            }
                            #endregion DayCountFraction
                            break;

                        case CciEnum.principalExchanges:
                            #region principalExchanges
                            bool principalExchanges = (Irs.PrincipalExchangesSpecified &&
                                (Irs.PrincipalExchanges.InitialExchange.BoolValue ||
                                 Irs.PrincipalExchanges.IntermediateExchange.BoolValue ||
                                 Irs.PrincipalExchanges.FinalExchange.BoolValue));
                            data = principalExchanges ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                            #endregion principalExchanges
                            break;
                        #endregion Others

                        #region Default
                        default:
                            isSetting = false;
                            break;
                        #endregion Default
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
            if (this.ExistInitialStub)
                cciInitialStub.Initialize_FromDocument();
            if (this.ExistFinalStub)
                cciFinalStub.Initialize_FromDocument();
        }
        
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        // EG 20190415 [Migration BANCAPERTA]
        public void Dump_ToDocument()
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
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    ICalculation  calculation;
                    #endregion Reset variables


                    switch (cciEnum)
                    {
                        #region Payer/receiver
                        case CciEnum.payer:
                            #region Payer
                            Irs.PayerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            #endregion Payer
                            break;
                        case CciEnum.receiver:
                            #region Receiver
                            Irs.ReceiverPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            #endregion Receiver
                            break;
                        #endregion Payer/receiver
                        //
                        #region ResetDates
                        case CciEnum.resetDates_resetRelativeTo:
                            #region ResetRelativeto
                            Irs.ResetDatesSpecified = false;
                            Irs.ResetDates.ResetRelativeToSpecified = false;
                            if ((StrFunc.IsFilled(data)) && IsFloatingRateValid)
                            {
                                ResetRelativeToEnum resetRelativeToEnum = (ResetRelativeToEnum)System.Enum.Parse(typeof(ResetRelativeToEnum), data, true);
                                Irs.ResetDatesSpecified = true;
                                Irs.ResetDates.ResetRelativeToSpecified = true;
                                Irs.ResetDates.ResetRelativeTo = resetRelativeToEnum;
                            }
                            #endregion ResetRelativeto
                            break;
                        #endregion ResetDates
                        //
                        #region CalculationPeriodDates
                        case CciEnum.calculationPeriodDates_effectiveDate:
                            #region EffectiveDate
                            DumpCalculationPeriodDatesEffectiveDate();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer la rollConvention
                            #endregion EffectiveDate
                            break;
                        case CciEnum.calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC:
                            DumpCalculationPeriodDatesEffectiveDateBDA();
                            break;
                        case CciEnum.calculationPeriodDates_terminationDate:
                            #region TerminationDate
                            DumpCalculationPeriodDatesTerminationDate();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion TerminationDate
                            break;
                        case CciEnum.calculationPeriodDates_terminationDate_dateAdjustedDate_bDC:
                            #region TerminationDate (BusinessDayAdjustment)
                            DumpCalculationPeriodDatesTerminationDateBDA();
                            #endregion TerminationDate (BusinessDayAdjustment)
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC:
                            #region Calculation (BusinessDayAdjustment)
                            DumpCalculationBDA();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer PaymentDatesAdjusments
                            #endregion Calculation (BusinessDayAdjustment)
                            break;
                        case CciEnum.calculationPeriodDates_firstRegularPeriodStartDate:
                            #region FirstRegularPeriodStartDate
                            Irs.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodDates.FirstRegularPeriodStartDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer la rollConvention
                            #endregion FirstRegularPeriodStartDate
                            break;
                        case CciEnum.calculationPeriodDates_lastRegularPeriodEndDate:
                            #region LastRegularPeriodEndDate
                            Irs.CalculationPeriodDates.LastRegularPeriodEndDateSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodDates.LastRegularPeriodEndDate.Value = data;
                            #endregion LastRegularPeriodEndDate
                            break;
                        case CciEnum.calculationPeriodDates_firstPeriodStartDate:
                            #region FirstPeriodStartDate
                            Irs.CalculationPeriodDates.FirstPeriodStartDateSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer la rollConvention
                            #endregion FirstPeriodStartDate
                            break;
                        case CciEnum.calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC:
                            #region FirstPeriodStartDate (BusinessDayAdjustment)
                            DumpFirstPeriodStartDateBDA();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion FirstPeriodStartDate (BusinessDayAdjustment)
                            break;

                        #region CalculationFrequency
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency:
                            #region PeriodFrequency
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de mettre à jour Period et Multiplier
                            #endregion PeriodFrequency
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier:
                            #region PeriodMultiplier
                            Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer resetFrequency
                            #endregion PeriodMultiplier
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_period:
                            #region Period
                            if (StrFunc.IsFilled(data))
                            {
                                PeriodEnum periodEnum = StringToEnum.Period(data);
                                Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period = periodEnum;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer resetFrequency
                            #endregion Period
                            break;
                        case CciEnum.calculationPeriodDates_calculationPeriodFrequency_rollConvention:
                            #region Rollconvention
                            if (StrFunc.IsFilled(data))
                            {
                                RollConventionEnum rollConventionEnum = (RollConventionEnum)System.Enum.Parse(typeof(RollConventionEnum), data, true);
                                Irs.CalculationPeriodDates.CalculationPeriodFrequency.RollConvention = rollConventionEnum;
                            }
                            #endregion Rollconvention
                            break;
                        #endregion  CalculationFrequency
                        #endregion CalculationPeriodDates
                        //
                        #region PaymentDates
                        case CciEnum.paymentDates_paymentDatesAdjustments_bDC:
                            #region PaymentDatesAdjustments
                            DumpPaymentBDA();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion PaymentDatesAdjustments
                            break;
                        case CciEnum.paymentDates_payRelativeToReverse:
                            #region PayRelativeToReverse
                            if (cci.IsFilledValue)
                                Irs.PaymentDates.PayRelativeTo = PayRelativeToEnum.CalculationPeriodStartDate;
                            else
                                Irs.PaymentDates.PayRelativeTo = PayRelativeToEnum.CalculationPeriodEndDate;
                            break;
                            #endregion PayRelativeToReverse
                        case CciEnum.paymentDates_firstPaymentDate:
                            #region firstPaymentDate
                            Irs.PaymentDates.FirstPaymentDate.Value = data;
                            Irs.PaymentDates.FirstPaymentDateSpecified = StrFunc.IsFilled(data);
                            #endregion firstPaymentDate
                            break;
                        #endregion paymentDates
                        //
                        #region KnownAmountSchedule
                        case CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                            #region InitialValue
                            Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.CalculationSpecified = (false == Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified);
                            //							
                            if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                            {
                                Irs.CalculationPeriodAmount.KnownAmountSchedule.InitialValue.Value = data;
                                if (StrFunc.IsEmpty(Irs.CalculationPeriodAmount.KnownAmountSchedule.Id))
                                    Irs.CalculationPeriodAmount.KnownAmountSchedule.Id = GenerateId(TradeCustomCaptureInfos.CCst.INITIALVALUE_REFERENCE);
                                //
                                Irs.PaymentDates.CalculationPeriodDatesReference.HRef = GetCalculationPeriodDatesId();
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir NotionalStepScheduleInitialValue
                            #endregion InitialValue
                            break;
                        case CciEnum.calculationPeriodAmount_knownAmountSchedule_currency:
                            #region Currency
                            if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                                Irs.CalculationPeriodAmount.KnownAmountSchedule.Currency.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir NotionalStepScheduleInitialValue
                            #endregion Currency
                            break;
                        #endregion KnownAmountSchedule
                        //
                        #region NotionalStepSchedule
                        case CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                            #region InitialValue
                            Irs.CalculationPeriodAmount.CalculationSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified = (false == Irs.CalculationPeriodAmount.CalculationSpecified);
                            //
                            Irs.CalculationPeriodAmount.Calculation.NotionalSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified = (false == Irs.CalculationPeriodAmount.Calculation.NotionalSpecified);
                            if (Irs.CalculationPeriodAmount.Calculation.NotionalSpecified)
                            {
                                Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.InitialValue.Value = data;
                                if (StrFunc.IsEmpty(Irs.CalculationPeriodAmount.Calculation.Notional.Id))
                                    Irs.CalculationPeriodAmount.Calculation.Notional.Id = GenerateId(TradeCustomCaptureInfos.CCst.NOTIONALSCHEDULE_REFERENCE);
                                if (StrFunc.IsEmpty(Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.Id))
                                    Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.Id = GenerateId(TradeCustomCaptureInfos.CCst.INITIALVALUE_REFERENCE);
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir NotionalStepScheduleInitialValue
                            #endregion InitialValue
                            break;

                        case CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency:
                            #region Currency
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                                Irs.CalculationPeriodAmount.Calculation.Notional.StepSchedule.Currency.Value = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir NotionalStepScheduleInitialValue
                            #endregion Currency
                            break;
                        #endregion NotionalStepSchedule
                        //
                        #region FxLinkedNotionalSchedule
                        case CciEnum.fxLinkedNotionalSchedule_notionalScheduleReference:
                            #region NotionalReference
                            Irs.CalculationPeriodAmount.CalculationSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified = (false == Irs.CalculationPeriodAmount.CalculationSpecified);
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                                Irs.CalculationPeriodAmount.Calculation.NotionalSpecified = (false == Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified);
                            if (Irs.CalculationPeriodAmount.CalculationSpecified &&
                                Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified)
                            {
                                Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional.ConstantNotionalScheduleReference.HRef = data;
                            }
                            #endregion NotionalReference
                            break;
                        case CciEnum.fxLinkedNotionalSchedule_initialValue:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency:
                        case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                        case CciEnum.fxLinkedNotionalSchedule_rateSourcePage:
                        case CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading:
                        case CciEnum.fxLinkedNotionalSchedule_secondaryRateSource:
                        case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage:
                        case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading:
                        case CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime:
                        case CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_period:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dayType:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC:
                        case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dayType:
                            Irs.CalculationPeriodAmount.CalculationSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified = cci.IsFilledValue;
                            Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified = (false == Irs.CalculationPeriodAmount.CalculationSpecified);
                            if (Irs.CalculationPeriodAmount.CalculationSpecified)
                                Irs.CalculationPeriodAmount.Calculation.NotionalSpecified = (false == Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified);
                            if (Irs.CalculationPeriodAmount.CalculationSpecified &&
                                Irs.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified)
                            {
                                IFxLinkedNotionalSchedule fxLinkedNotional = Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional;
                                switch (cciEnum)
                                {
                                    case CciEnum.fxLinkedNotionalSchedule_initialValue:
                                        #region initialValue
                                        fxLinkedNotional.InitialValueSpecified = cci.IsFilledValue;
                                        if (fxLinkedNotional.InitialValueSpecified)
                                            fxLinkedNotional.InitialValue.Value = data;
                                        break;
                                        #endregion initialValue
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency:
                                        #region Currency
                                        fxLinkedNotional.Currency = data;
                                        Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional.ConstantNotionalScheduleReference.HRef = TradeCustomCaptureInfos.CCst.NOTIONALSCHEDULE_REFERENCE + PreviousNumberPrefix;
                                        break;
                                        #endregion Currency
                                    case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                                    case CciEnum.fxLinkedNotionalSchedule_rateSource:
                                    case CciEnum.fxLinkedNotionalSchedule_rateSourcePage:
                                    case CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading:
                                    case CciEnum.fxLinkedNotionalSchedule_secondaryRateSource:
                                    case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage:
                                    case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading:
                                    case CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime:
                                    case CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter:
                                        #region FxSpotRateSource
                                        IFxSpotRateSource source = fxLinkedNotional.FxSpotRateSource;
                                        switch (cciEnum)
                                        {

                                            case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                                                #region Asset_FxRate
                                                SQL_AssetFxRate sql_asset = null;
                                                bool isLoaded = false;
                                                cci.ErrorMsg = string.Empty;
                                                if (StrFunc.IsFilled(data))
                                                {
                                                    for (int i = 0; i < 2; i++)
                                                    {
                                                        string dataToFind = data;
                                                        if (i == 1)
                                                            dataToFind = data.Replace(" ", "%") + "%";
                                                        sql_asset = new SQL_AssetFxRate(cciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                                        isLoaded = sql_asset.IsLoaded && (sql_asset.RowsCount == 1);
                                                        if (isLoaded)
                                                            break;
                                                    }
                                                    //
                                                    if (isLoaded)
                                                    {
                                                        cci.NewValue = sql_asset.Identifier;
                                                        cci.Sql_Table = sql_asset;
                                                        //
                                                        #region fixingTime
                                                        source.FixingTime.HourMinuteTime.TimeValue = sql_asset.TimeRateSrc;
                                                        source.FixingTime.BusinessCenter.Value = sql_asset.IdBC_RateSrc;
                                                        #endregion fixingTime
                                                        #region primaryRateSource
                                                        source.PrimaryRateSource.OTCmlId = sql_asset.Id;
                                                        source.PrimaryRateSource.RateSource.Value = sql_asset.PrimaryRateSrc;
                                                        source.PrimaryRateSource.AssetFxRateId.Value = sql_asset.Identifier;
                                                        //
                                                        source.PrimaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcPage);
                                                        if (source.PrimaryRateSource.RateSourcePageSpecified)
                                                            source.PrimaryRateSource.CreateRateSourcePage(sql_asset.PrimaryRateSrcPage);
                                                        //
                                                        source.PrimaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.PrimaryRateSrcHead);
                                                        if (source.PrimaryRateSource.RateSourcePageHeadingSpecified)
                                                            source.PrimaryRateSource.RateSourcePageHeading = sql_asset.PrimaryRateSrcHead;
                                                        #endregion primaryRateSource
                                                        #region secondaryRateSource
                                                        source.SecondaryRateSourceSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrc);
                                                        if (source.SecondaryRateSourceSpecified)
                                                        {
                                                            if (null == source.SecondaryRateSource)
                                                                source.SecondaryRateSource = source.CreateInformationSource;
                                                            source.SecondaryRateSource.RateSource.Value = sql_asset.SecondaryRateSrc;
                                                            //
                                                            source.SecondaryRateSource.RateSourcePageSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcPage);
                                                            if (source.SecondaryRateSource.RateSourcePageSpecified)
                                                                source.SecondaryRateSource.CreateRateSourcePage(sql_asset.SecondaryRateSrcPage);
                                                            //
                                                            source.SecondaryRateSource.RateSourcePageHeadingSpecified = StrFunc.IsFilled(sql_asset.SecondaryRateSrcHead);
                                                            if (source.SecondaryRateSource.RateSourcePageHeadingSpecified)
                                                                source.SecondaryRateSource.RateSourcePageHeading = sql_asset.SecondaryRateSrcHead;
                                                        }
                                                        #endregion secondaryRateSource
                                                    }
                                                    //
                                                    cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                                                }
                                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de préproposer les BCs
                                                #endregion Asset_FxRate
                                                break;
                                            #region primaryRateSource
                                            case CciEnum.fxLinkedNotionalSchedule_rateSource:
                                                source.PrimaryRateSource.RateSource.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_rateSourcePage:
                                                source.PrimaryRateSource.RateSourcePageSpecified = cci.IsFilledValue;
                                                source.PrimaryRateSource.RateSourcePage.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading:
                                                source.PrimaryRateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                                                source.PrimaryRateSource.RateSourcePageHeading = data;
                                                break;
                                            #endregion primaryRateSource
                                            #region secondaryRateSource
                                            case CciEnum.fxLinkedNotionalSchedule_secondaryRateSource:
                                                source.SecondaryRateSourceSpecified = cci.IsFilledValue;
                                                source.SecondaryRateSource.RateSource.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage:
                                                source.SecondaryRateSource.RateSourcePageSpecified = cci.IsFilledValue;
                                                source.SecondaryRateSource.RateSourcePage.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading:
                                                source.SecondaryRateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                                                source.SecondaryRateSource.RateSourcePageHeading = data;
                                                break;
                                            #endregion secondaryRateSource
                                            #region fixingTime
                                            case CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime:
                                                source.FixingTime.HourMinuteTime.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter:
                                                source.FixingTime.BusinessCenter.Value = data;
                                                break;
                                            #endregion fixingTime
                                            default:
                                                isSetting = false;
                                                break;
                                        }
                                        #endregion FxSpotRateSource
                                        break;
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_period:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dayType:
                                        #region VaryingNotionalFixingDates
                                        IRelativeDateOffset fixingOffset = fxLinkedNotional.VaryingNotionalFixingDates;
                                        switch (cciEnum)
                                        {
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_dateRelativeTo:
                                                fixingOffset.DateRelativeToValue = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_period:
                                                fixingOffset.Period = StringToEnum.Period(data);
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_periodMultiplier:
                                                fixingOffset.PeriodMultiplier.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalFixingDates_bDC:
                                                break;
                                        }
                                        #endregion VaryingNotionalFixingDates
                                        break;
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC:
                                    case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dayType:
                                        #region VaryingNotionalInterimExchangePaymentDates
                                        IRelativeDateOffset paymentOffset = fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates;
                                        switch (cciEnum)
                                        {
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_dateRelativeTo:
                                                paymentOffset.DateRelativeToValue = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_period:
                                                paymentOffset.Period = StringToEnum.Period(data);
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_periodMultiplier:
                                                paymentOffset.PeriodMultiplier.Value = data;
                                                break;
                                            case CciEnum.fxLinkedNotionalSchedule_varyingNotionalInterimExchangePaymentDates_bDC:
                                                break;
                                        }
                                        #endregion VaryingNotionalInterimExchangePaymentDates
                                        break;
                                    default:
                                        isSetting = false;
                                        break;
                                }
                            }
                            break;
                        #endregion FxLinkedNotionalSchedule
                        //
                        #region calculationPeriodAmount_calculation_dayCountFraction
                        case CciEnum.calculationPeriodAmount_calculation_dayCountFraction:

                            if (StrFunc.IsFilled(data))
                            {
                                DayCountFractionEnum dcfEnum = (DayCountFractionEnum)System.Enum.Parse(typeof(DayCountFractionEnum), data, true);
                                if (Irs.CalculationPeriodAmount.CalculationSpecified)
                                {
                                    Irs.CalculationPeriodAmount.Calculation.DayCountFraction = dcfEnum;
                                }
                                else if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                                {
                                    Irs.CalculationPeriodAmount.KnownAmountSchedule.DayCountFraction = dcfEnum;
                                    Irs.CalculationPeriodAmount.KnownAmountSchedule.DayCountFractionSpecified = true;
                                }
                            }
                            // EG 20190415 [Migration BANCAPERTA]
                            else if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                            {
                                Irs.CalculationPeriodAmount.KnownAmountSchedule.DayCountFractionSpecified = false;
                            }
                            break;
                        #endregion DayCountFraction
                        //
                        #region FixedRate / Rate / InflationRate
                        case CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                            #region FixedRate
                            calculation = Irs.CalculationPeriodAmount.Calculation;
                            calculation.RateFixedRateSpecified = StrFunc.IsFilled(data);
                            if (calculation.RateFixedRateSpecified)
                            {
                                calculation.RateFloatingRateSpecified = false;
                                calculation.RateFixedRate.InitialValue.Value = data;
                            }
                            if (calculation.RateFixedRateSpecified)
                                Irs.CalculationPeriodAmount.CalculationSpecified = true;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer Frequency, BDC, DCF, ...
                            #endregion
                            break;
                        case CciEnum.calculationPeriodAmount_calculation_rate:
                            #region FixedRate / FloatingRate
                            calculation = Irs.CalculationPeriodAmount.Calculation;
                            Cci(CciEnum.calculationPeriodAmount_calculation_rate).Sql_Table = null;
                            Cci(CciEnum.calculationPeriodAmount_calculation_rate).ErrorMsg = string.Empty;
                            //
                            calculation.RateFixedRateSpecified = false;
                            calculation.RateFloatingRateSpecified = false;
                            calculation.RateInflationRateSpecified = false;
                            Irs.ResetDatesSpecified = false;

                            calculation.RateFixedRateSpecified = RateTools.IsFixedRate(data);
                            calculation.RateFloatingRateSpecified = RateTools.IsFloatingRate(data);

                            #region FixedRate
                            if (calculation.RateFixedRateSpecified)
                                calculation.RateFixedRate.InitialValue.Value = data;
                            #endregion FixedRate
                            //
                            #region  FloatingRate
                            if (calculation.RateFloatingRateSpecified)
                            {
                                IInterval tenorDefault = null;
                                try
                                {
                                    tenorDefault = (IInterval)Irs.CalculationPeriodDates.CalculationPeriodFrequency;
                                }
                                catch { tenorDefault = null; }
                                //								
                                Ccis.DumpFloatingRateIndex_ToDocument(CciClientId(CciEnum.calculationPeriodAmount_calculation_rate), tenorDefault,
                                    calculation.RateFloatingRate, calculation.RateFloatingRate.FloatingRateIndex, calculation.RateFloatingRate.IndexTenor);
                                //
                                Irs.ResetDatesSpecified = true;
                            }
                            #endregion FloatingRate
                            //
                            if ((calculation.RateFixedRateSpecified) || (calculation.RateFloatingRateSpecified))
                                Irs.CalculationPeriodAmount.CalculationSpecified = true;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer Frequency, BDC, DCF, ...
                            #endregion FixedRate / FloatingRate
                            break;
                        case CciEnum.calculationPeriodAmount_calculation_inflationRate:
                            #region Inflation Rate
                            calculation = Irs.CalculationPeriodAmount.Calculation;
                            Cci(CciEnum.calculationPeriodAmount_calculation_inflationRate).Sql_Table = null;
                            Cci(CciEnum.calculationPeriodAmount_calculation_inflationRate).ErrorMsg = string.Empty;
                            //
                            calculation.RateFixedRateSpecified = false;
                            calculation.RateFloatingRateSpecified = false;
                            calculation.RateInflationRateSpecified = true;
                            Ccis.Dump_Index_ToDocument(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRate), calculation.RateInflationRate.FloatingRateIndex);
                            Irs.ResetDatesSpecified = true;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer Frequency, BDC, DCF, ...
                            #endregion Inflation Rate
                            break;

                        #region FloatingRateCalculation
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                            DumpFloatingRateCalculation_ToDocument(cciEnum, Irs.CalculationPeriodAmount.Calculation.RateFloatingRate, data);
                            break;
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue:
                        case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue:
                            DumpFloatingRateCalculation_ToDocument(cciEnum, Irs.CalculationPeriodAmount.Calculation.RateInflationRate, data);
                            break;
                        case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion FloatingRateCalculation
                        #endregion Rate / InflationRate
                        //
                        case CciEnum.principalExchanges:
                            #region PrincipalExchanges
                            Irs.PrincipalExchangesSpecified = cci.IsFilledValue;
                            if (null == Irs.PrincipalExchanges.InitialExchange)
                                Irs.PrincipalExchanges.InitialExchange = new EFS_Boolean();
                            Irs.PrincipalExchanges.InitialExchange.BoolValue = Irs.PrincipalExchangesSpecified;
                            if (null == Irs.PrincipalExchanges.IntermediateExchange)
                                Irs.PrincipalExchanges.IntermediateExchange = new EFS_Boolean();
                            Irs.PrincipalExchanges.IntermediateExchange.BoolValue = Irs.PrincipalExchangesSpecified;
                            if (null == Irs.PrincipalExchanges.FinalExchange)
                                Irs.PrincipalExchanges.FinalExchange = new EFS_Boolean();
                            Irs.PrincipalExchanges.FinalExchange.BoolValue = Irs.PrincipalExchangesSpecified;
                            #endregion PrincipalExchanges
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
            //20061129 PL Bug on stub rate on stream 2
            GetCalculationPeriodDatesId();
            //
            //20061130 PL
            GetResetDatesId();
            //
            SynchronizeResetDatesFromCalculationPeriodDates();
            //
            if (ExistInitialStub)
                cciInitialStub.Dump_ToDocument();
            //
            if (ExistFinalStub)
                cciFinalStub.Dump_ToDocument();
            //
            if (ExistInitialStub)
                Irs.StubCalculationPeriodAmount.InitialStubSpecified = cciInitialStub.IsSpecified;
            //
            if (ExistFinalStub)
                Irs.StubCalculationPeriodAmount.FinalStubSpecified = cciFinalStub.IsSpecified;
            //
            if ((ExistInitialStub) || (ExistFinalStub))
                Irs.StubCalculationPeriodAmountSpecified = (Irs.StubCalculationPeriodAmount.InitialStubSpecified || Irs.StubCalculationPeriodAmount.FinalStubSpecified);
            //
        }
        
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                bool isEnabled;
                bool isFromAmount;
                bool isSynchro;
                //
                switch (elt)
                {
                    #region Payer/Receiver: Calcul des BCs
                    case CciEnum.payer:
                        isSynchro = IsSwapReceiverAndPayer;
                        if (isSynchro)
                        {
                            if ((false == pCci.IsMandatory))
                            {
                                if (StrFunc.IsEmpty(pCci.NewValue))
                                {
                                    isSynchro = false;
                                    Clear();
                                }
                                else
                                {
                                    if (StrFunc.IsEmpty(Cci(CciEnum.receiver).NewValue) && cciTrade.cciParty[0].IsSpecified && cciTrade.cciParty[1].IsSpecified)
                                    {
                                        for (int i = 0; i < cciTrade.PartyLength; i++)
                                        {
                                            string xmlId = ((SQL_Actor)cciTrade.cciParty[i].Cci(CciTradeParty.CciEnum.actor).Sql_Table).XmlId;
                                            //
                                            if (xmlId != pCci.NewValue)
                                            {
                                                Cci(CciEnum.receiver).NewValue = xmlId;
                                                isSynchro = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                SetEnabled(StrFunc.IsFilled(pCci.NewValue));
                            }
                        }
                        if (isSynchro)
                            CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue, true);
                        //
                        //------------------------------------------------------------------------------------------------------------
                        //PL 20100325 TRIM 16897 Plus de calul des BCs (donc des BDAs) lors de la modification du Payer/Receiver
                        //------------------------------------------------------------------------------------------------------------
                        //DumpCalculationBDA();
                        //DumpPaymentBDA();
                        //DumpFirstPeriodStartDateBDA();
                        //------------------------------------------------------------------------------------------------------------
                        break;
                    case CciEnum.receiver:
                        isSynchro = IsSwapReceiverAndPayer;
                        if (isSynchro)
                        {
                            if (false == pCci.IsMandatory)
                            {
                                if (StrFunc.IsEmpty(pCci.NewValue))
                                    isSynchro = false;
                            }
                        }
                        if (isSynchro)
                            CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue, true);
                        //
                        //------------------------------------------------------------------------------------------------------------
                        //PL 20100325 TRIM 16897 Plus de calul des BCs (donc des BDAs) lors de la modification du Payer/Receiver
                        //------------------------------------------------------------------------------------------------------------
                        //DumpCalculationBDA();
                        //DumpPaymentBDA();
                        //DumpFirstPeriodStartDateBDA();
                        //------------------------------------------------------------------------------------------------------------
                        break;
                    #endregion
                    #region EffectiveDate: Calcul de RollConvention
                    case CciEnum.calculationPeriodDates_effectiveDate:
                        if (false == this.cciTrade.Product.IsDebtSecurity)
                            SetRollConvention();
                        break;
                    case CciEnum.calculationPeriodDates_firstRegularPeriodStartDate:
                        SetRollConvention();
                        DumpFirstPeriodStartDateBDA();
                        break;
                    case CciEnum.calculationPeriodDates_firstPeriodStartDate:
                        if (false == this.cciTrade.Product.IsDebtSecurity)
                            SetRollConvention();
                        break;
                    case CciEnum.calculationPeriodDates_lastRegularPeriodEndDate:
                        // FI 20190731 [XXXXX] pré-proposition de la rollconvention sur les titres
                        if (this.cciTrade.Product.IsDebtSecurity)
                            SetRollConvention();
                        break;
                    case CciEnum.calculationPeriodDates_terminationDate:
                        // FI 20190731 [XXXXX] pré-proposition de la rollconvention sur les titres
                        if (this.cciTrade.Product.IsDebtSecurity)
                            SetRollConvention();
                        break;
                    #endregion
                    #region CalculationFrequencyPeriodFrequency
                    case CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency:
                        string periodMultiplier = string.Empty;
                        string period = string.Empty;
                        //
                        if (StrFunc.IsFilled(pCci.NewValue))
                        {
                            period = pCci.NewValue.Substring(pCci.NewValue.Length - 1, 1);
                            periodMultiplier = pCci.NewValue.Substring(0, pCci.NewValue.Length - 1);
                        }
                        //
                        //20090514 RD/PL 
                        PeriodTofrequency.Translate(ref periodMultiplier, ref period);
                        CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), periodMultiplier);
                        CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period), period);
                        break;
                    #endregion
                    #region CalculationFrequencyPeriodMultiplier, CalculationFrequencyPeriod: Calcul de RollConvention, ResetFrequency, PaymentFrequency
                    case CciEnum.calculationPeriodDates_calculationPeriodFrequency_period:
                    case CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier:
                        //
                        //if (CciEnum.calculationPeriodDates_calculationPeriodFrequency_period == elt)
                        //{
                        //    if (PeriodEnum.T == Irs.calculationPeriodDates.calculationPeriodFrequency.interval.period)
                        //    {
                        //        ccis.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), "1");
                        //        //
                        //        if (ccis.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency)))
                        //        {
                        //            string newPeriodFrequency = ccis.GetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency));
                        //            newPeriodFrequency = "1" + newPeriodFrequency.Substring(newPeriodFrequency.Length - 1, 1);
                        //            //
                        //            ccis.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency), newPeriodFrequency);
                        //        }
                        //    }
                        //}
                        //20071123 FI Ticket 15907
                        if (Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).HasChanged
                            && // Si new Value Vaut M ou Y alors que l'ancienne valeur différente de D ou Y
                            ((PeriodEnum.M.ToString() == Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).NewValue &&
                            PeriodEnum.Y.ToString() != Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).LastValue)
                            ||
                            (PeriodEnum.Y.ToString() == Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).NewValue &&
                            PeriodEnum.M.ToString() != Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).LastValue)
                            ||
                            // Si new Value Vaut autre que M ou Y alors que l'ancienne valeur égale à M ou D
                            (
                            (PeriodEnum.M.ToString() != Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).NewValue ||
                            PeriodEnum.Y.ToString() != Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).NewValue))
                            &&
                            (PeriodEnum.M.ToString() == Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).LastValue ||
                            PeriodEnum.Y.ToString() == Cci(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period).LastValue))
                            )
                        {
                            SetRollConvention();
                        }
                        //20090507 FI Appel à SetPaymentDates 
                        //En effet jusqu'à l'appel à SetPaymentDates n'était fait que post la saisie d'un taux
                        //or pour les titres précomptés le taux n'est pas renseigné
                        //SetPaymentDates fait des call à SetPaymentFrequency
                        //20090813 FI 16649 SetPaymentDates a été éclatée en plusieurs fonction
                        //appelle individuellement SetPaymentDatesId,SetPaymentDatesPayRelativeTo,SetPaymentFrequency
                        SetPaymentDatesId();
                        SetPaymentDatesPayRelativeTo();
                        SetPaymentFrequency();
                        SetResetFrequency();
                        //
                        // 20090602 Ticket 16497
                        // Forcer le Multiplier à 1 dés que Period = T
                        // Dans le cas ou Multiplier = 0, ne pas le forcer à 1 ( cas ZC = 0T)
                        //
                        if (CciEnum.calculationPeriodDates_calculationPeriodFrequency_period == elt)
                        {
                            if (PeriodEnum.T == Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period)
                            {
                                string newMultiplier = CcisBase.GetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier));
                                if (StrFunc.IsEmpty(newMultiplier) || newMultiplier.CompareTo("0") != 0)
                                {
                                    CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), "1");
                                    if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency)))
                                        CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency), "1" + PeriodEnum.T.ToString());
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Rate: Calcul de CalculationPeriodFrequency
                    case CciEnum.calculationPeriodAmount_calculation_rate:
                        isEnabled = IsFloatingRateValid;
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.resetDates_resetRelativeTo), "IsEnabled", isEnabled);

                        if (false == isEnabled)
                        {
                            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue), "NewValue", string.Empty);
                            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue), "NewValue", string.Empty);
                            CcisBase.Set(CciClientId(CciEnum.resetDates_resetRelativeTo), "NewValue", string.Empty);
                        }
                        
                        // Attention ne pas changer l'ordre des appels de proc
                        SetCalculationPeriodFrequency();
                        //SetRollConvention();
                        SetDayCountFraction();
                        SetResetDates();
                        SetCalculationPeriodDatesAdjusments();
                        SetPaymentDates();
                        // EG 20150824 DumpPaymentBDA est appelé dans SetPaymentDates pour IsFloatingRateSpecified|IsFixedRateSpecified|
                        // RD 20150814 [21254] Mettre à jour PaymentDateAjustement dans le cas du taux fixe
                        //if (Irs.calculationPeriodAmount.calculation.rateFixedRateSpecified)
                        //    DumpPaymentBDA();
                        break;
                    #endregion
                    #region InflationRate
                    case CciEnum.calculationPeriodAmount_calculation_inflationRate:
                        isEnabled = IsInflationRateValid;
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue), "IsEnabled", isEnabled);
                        CcisBase.Set(CciClientId(CciEnum.resetDates_resetRelativeTo), "IsEnabled", isEnabled);
                        //
                        if (false == isEnabled)
                        {
                            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue), "NewValue", string.Empty);
                            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue), "NewValue", string.Empty);
                            CcisBase.Set(CciClientId(CciEnum.resetDates_resetRelativeTo), "NewValue", string.Empty);
                        }
                        //						
                        break;
                    #endregion
                    #region CalculationPeriodDatesBusinessDayConvention: Calcul de PaymentBusinesDayConvention
                    case CciEnum.calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC:
                        //SetPaymentDatesAdjusments();
                        break;
                    #endregion
                    #region Currency: Arrondi du notional et Calcul des BCs
                    case CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                    case CciEnum.calculationPeriodAmount_knownAmountSchedule_currency:
                        if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                        {
                            IKnownAmountSchedule knownAmountSchedule = Irs.CalculationPeriodAmount.KnownAmountSchedule;
                            isFromAmount = IsCci(CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                            Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue), knownAmountSchedule.InitialValue, knownAmountSchedule.Currency.Value, isFromAmount);
                        }
                        //
                        if (Irs.CalculationPeriodAmount.KnownAmountScheduleSpecified)
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), "1");
                            CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period), PeriodEnum.T.ToString());
                            //
                            if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency)))
                                CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency), "1" + PeriodEnum.T.ToString());
                        }
                        //
                        break;
                    case CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                    case CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency:
                        if (Irs.CalculationPeriodAmount.CalculationSpecified && Irs.CalculationPeriodAmount.Calculation.NotionalSpecified)
                        {
                            isFromAmount = IsCci(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                            INotional notional = Irs.CalculationPeriodAmount.Calculation.Notional;
                            Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue), notional.StepSchedule.InitialValue, notional.StepSchedule.Currency.Value, isFromAmount);
                        }
                        //
                        if (IsCci(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency, pCci))
                        {
                            DumpCalculationBDA();
                            DumpPaymentBDA();
                            DumpFirstPeriodStartDateBDA();
                        }
                        break;
                    #endregion
                    #region fxLinkedNotionalSchedule_assetFxRate
                    case CciEnum.fxLinkedNotionalSchedule_assetFxRate:
                        if (null != pCci.Sql_Table)
                        {
                            SQL_AssetFxRate sql_AssetFxRate = (SQL_AssetFxRate)pCci.Sql_Table;
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_rateSource), sql_AssetFxRate.PrimaryRateSrc);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_rateSourcePage), sql_AssetFxRate.PrimaryRateSrcPage);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading), sql_AssetFxRate.PrimaryRateSrcHead);

                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_secondaryRateSource), sql_AssetFxRate.SecondaryRateSrc);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage), sql_AssetFxRate.SecondaryRateSrcPage);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading), sql_AssetFxRate.SecondaryRateSrcHead);

                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime), DtFunc.DateTimeToString(sql_AssetFxRate.TimeRateSrc, DtFunc.FmtISOTime));
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter), sql_AssetFxRate.IdBC_RateSrc);

                            SetVaryingNotionalFixingDates();
                            SetVaryingNotionalInterimExchangePaymentDates();
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_rateSource), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_rateSourcePage), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_rateSourcePageHeading), string.Empty);

                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_secondaryRateSource), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePage), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_secondaryRateSourcePageHeading), string.Empty);

                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_fixingTime_hourMinuteTime), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.fxLinkedNotionalSchedule_fixingTime_businessCenter), string.Empty);

                        }
                        break;
                    #endregion
                    #region calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue
                    case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_straddleRateSchedule_initialValue:
                        CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue), pCci.NewValue);
                        CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue), pCci.NewValue);
                        break;
                    #endregion
                    #region Default
                    default:

                        break;
                        #endregion Default
                }
            }
            //
            if (this.ExistInitialStub)
                cciInitialStub.ProcessInitialize(pCci);
            if (this.ExistFinalStub)
                cciFinalStub.ProcessInitialize(pCci);
            //

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
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

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
        public void CleanUp()
        {
            if (ExistInitialStub)
                cciInitialStub.CleanUp();
            //
            if (ExistFinalStub)
                cciFinalStub.CleanUp();
            //
            ICalculation calculation = Irs.CalculationPeriodAmount.Calculation;
            if (null != calculation)
            {
                IFloatingRate floatingRate;
                if (IsFloatingRateSpecified && (null != calculation.RateFloatingRate))
                {
                    floatingRate = calculation.RateFloatingRate;
                    //cap
                    if (null != floatingRate.CapRateSchedule)
                        floatingRate.CapRateScheduleSpecified = CaptureTools.IsDocumentElementValid(floatingRate.CapRateSchedule[0].InitialValue);
                    //Floor
                    if (null != floatingRate.FloorRateSchedule)
                        floatingRate.FloorRateScheduleSpecified = CaptureTools.IsDocumentElementValid(floatingRate.FloorRateSchedule[0].InitialValue);
                }
                if (IsInflationRateSpecified && (null != calculation.RateInflationRate))
                {
                    floatingRate = calculation.RateInflationRate;
                    //cap
                    if (null != floatingRate.CapRateSchedule)
                        floatingRate.CapRateScheduleSpecified = CaptureTools.IsDocumentElementValid(floatingRate.CapRateSchedule[0].InitialValue);
                    //Floor
                    if (null != floatingRate.FloorRateSchedule)
                        floatingRate.FloorRateScheduleSpecified = CaptureTools.IsDocumentElementValid(floatingRate.FloorRateSchedule[0].InitialValue);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (ExistInitialStub)
                cciInitialStub.SetDisplay(pCci);
            //
            if (ExistFinalStub)
                cciFinalStub.SetDisplay(pCci);
            //
            if (IsCci(CciEnum.calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments);
            //
            else if (IsCci(CciEnum.calculationPeriodDates_terminationDate_dateAdjustedDate_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments);
            //
            else if (IsCci(CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments);
            //
            else if (IsCci(CciEnum.paymentDates_paymentDatesAdjustments_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, Irs.PaymentDates.PaymentDatesAdjustments);
            //
            else if (IsCci(CciEnum.calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC, pCci))
            {
                if (Irs.CalculationPeriodDates.FirstPeriodStartDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, Irs.CalculationPeriodDates.FirstPeriodStartDate.DateAdjustments);
            }
            //
            else if (IsCci(CciEnum.resetDates_resetRelativeTo, pCci))
            {
                if (Irs.ResetDatesSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, Irs.ResetDates.FixingDates.GetAdjustments);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            if (ExistInitialStub)
                cciInitialStub.RefreshCciEnabled();
            //
            if (ExistFinalStub)
                cciFinalStub.RefreshCciEnabled();
            //
            bool isEnabled_FloatingRate = IsFloatingRateValid;
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue), "IsEnabled", isEnabled_FloatingRate);
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue), "IsEnabled", isEnabled_FloatingRate);
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue), "IsEnabled", isEnabled_FloatingRate);
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue), "IsEnabled", isEnabled_FloatingRate);

            bool isEnabled_InflationRate = IsInflationRateValid;
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue), "IsEnabled", isEnabled_InflationRate);
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue), "IsEnabled", isEnabled_InflationRate);
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue), "IsEnabled", isEnabled_InflationRate);
            CcisBase.Set(CciClientId(CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue), "IsEnabled", isEnabled_InflationRate);

            CcisBase.Set(CciClientId(CciEnum.resetDates_resetRelativeTo), "IsEnabled", isEnabled_FloatingRate || isEnabled_InflationRate);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string pPrefix)
        {
            if (ExistInitialStub)
                cciInitialStub.RemoveLastItemInArray(pPrefix);
            //
            if (ExistFinalStub)
                cciFinalStub.RemoveLastItemInArray(pPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }
        
        #endregion
        
        #region Membres de IContainerCciPayerReceiver
        #region  CciClientIdPayer/receiver
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer.ToString()); }
        }
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver.ToString()); }
        }
        #endregion
        #region SynchronizePayerReceiver
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue, true);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, true);
        }
        #endregion
        #endregion Membres de IContainerCciPayerReceiver
        
        
        
        #region Membres de IsSpecified
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion

        #region  Methodes
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);

        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(Boolean pIsEnabled)
        {
            CciTools.SetCciContainer(this, "CciEnum", "IsEnabled", pIsEnabled);
            //
            Cci(CciEnum.payer).IsEnabled = true;

        }
        
        /// <summary>
        /// 
        /// </summary>
        private void SetResetFrequency()
        {
            if (null == Irs.ResetDates.ResetFrequency)
                Irs.ResetDates.ResetFrequency = Irs.CreateResetFrequency();
            if (null == Irs.ResetDates.ResetFrequency.Interval.PeriodMultiplier)
                Irs.ResetDates.ResetFrequency.Interval.PeriodMultiplier = new EFS_Integer();

            Irs.ResetDates.ResetFrequency.Interval.PeriodMultiplier.Value = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value;
            Irs.ResetDates.ResetFrequency.Interval.Period = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period;

            Irs.ResetDates.ResetFrequency.WeeklyRollConventionSpecified = (PeriodEnum.W == Irs.ResetDates.ResetFrequency.Interval.Period);
        }
        
        /// <summary>
        /// aliemnte Irs.paymentDates.paymentFrequency avec ce qui existe ds Irs.calculationPeriodDates.calculationPeriodFrequency
        /// </summary>
        private void SetPaymentFrequency()
        {
            Irs.PaymentDates.PaymentFrequency.PeriodMultiplier.Value =
                Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.Value;
            //
            Irs.PaymentDates.PaymentFrequency.Period = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period;
        }
        
        /// <summary>
        /// Set PaymentDates à partir de calculationPeriodDates et en fonction des caractéristiques de l'asset de taux
        /// Payment Frequency de payment = calculation Perio Frequency 
        /// Set PaymentDatesAdjustments
        /// </summary>
        /// FI 20090508 Gestion des titres précompté
        /// PaymentDates est obligatoire mais sur les Zero coupon il n'y a pas de règlement
        /// Pour sur les Zero => payRelTo.CalculationPeriodStartDate
        /// 
        /// FI 20090813  16649
        /// SetPaymentDates est éclatées en mini function
        private void SetPaymentDates()
        {
            //
            SetPaymentDatesId();
            //
            //PaymentRelativeTo
            SetPaymentDatesPayRelativeTo();
            //	
            //PaymentFrequency
            SetPaymentFrequency();//Warning: also setting by Calculation Frequency
            //
            //PaymentDaysOffset
            SetPaymentDatesOffset();
            //
            //PaymentDatesAdjustments
            SetPaymentDatesAdjusments();//Warning: also setting by CalcPeriodDatesBDC

        }

        /// <summary>
        /// Alimente Irs.paymentDates.paymentDaysOffset avec l'offset présent sur l'asset de taux
        /// </summary>
        private void SetPaymentDatesOffset()
        {
            SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
            if (null != slqAssetRateIndex)
            {
                if (Irs.PaymentDates.PaymentDaysOffset == null)
                    Irs.PaymentDates.PaymentDaysOffset = Irs.CreateOffset();

                bool isSpecified = Ccis.DumpOffset_ToDocument(Irs.PaymentDates.PaymentDaysOffset, slqAssetRateIndex, "PAYMENT");
                Irs.PaymentDates.PaymentDaysOffsetSpecified = isSpecified;
            }
        }
        
        /// <summary>
        /// Alimente Irs.paymentDates
        /// </summary>
        private void SetPaymentDatesPayRelativeTo()
        {
            bool isZeroCoupon = Tools.IsPeriodZeroCoupon(Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval);
            SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
            //
            Irs.PaymentDates.CalculationPeriodDatesReferenceSpecified = false;
            Irs.PaymentDates.ResetDatesReferenceSpecified = false;
            //
            PayRelativeToEnum payRelTo = isZeroCoupon ? PayRelativeToEnum.CalculationPeriodStartDate : PayRelativeToEnum.CalculationPeriodEndDate;
            if ((null != slqAssetRateIndex) && StrFunc.IsFilled(slqAssetRateIndex.Idx_RelativeToPaymentDt))  
                payRelTo = (PayRelativeToEnum)System.Enum.Parse(typeof(PayRelativeToEnum), slqAssetRateIndex.Idx_RelativeToPaymentDt, true);
            //
            if (payRelTo == PayRelativeToEnum.CalculationPeriodEndDate || 
                payRelTo == PayRelativeToEnum.CalculationPeriodStartDate)
            {
                Irs.PaymentDates.CalculationPeriodDatesReferenceSpecified = true;
                //20061129 PL 
                //Irs.paymentDates.paymentDatesDateReferenceCalculationPeriodDatesReference.href = Irs.calculationPeriodDates.id;
                Irs.PaymentDates.CalculationPeriodDatesReference.HRef = GetCalculationPeriodDatesId();
            }
            else if (payRelTo == PayRelativeToEnum.ResetDate)
            {
                Irs.PaymentDates.ResetDatesReferenceSpecified = true;
                //20061130 PL
                //Irs.paymentDates.paymentDatesDateReferenceResetDatesReference.href  = Irs.resetDates.id;
                Irs.PaymentDates.ResetDatesReference.HRef = GetResetDatesId();
            }
            Irs.PaymentDates.PayRelativeTo = payRelTo;
        }
        
        /// <summary>
        /// alimente Irs.paymentDates si non renseigné
        /// </summary>
        private void SetPaymentDatesId()
        {
            if (StrFunc.IsEmpty(Irs.PaymentDates.Id))
                Irs.PaymentDates.Id = GenerateId(TradeCustomCaptureInfos.CCst.PAYMENT_DATES_REFERENCE);
        }

        /// <summary>
        /// Alimente Irs.resetDates si le stream est à taux flottant
        /// </summary>
        private void SetResetDates()
        {
            //
            ICalculation calculation = Irs.CalculationPeriodAmount.Calculation;
            //
            if (calculation.RateFloatingRateSpecified)
            {
                SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
                //
                //20061130 PL 
                //if (StrFunc.IsEmpty(Irs.resetDates.id))
                //	Irs.resetDates.id = CCst.RESET_DATES_REFERENCE + this.Number;
                GetResetDatesId();
                //
                //calculationPeriodDatesReference
                //20061129 PL 
                //Irs.resetDates.calculationPeriodDatesReference.href = Irs.calculationPeriodDates.id;
                Irs.ResetDates.CalculationPeriodDatesReference.HRef = GetCalculationPeriodDatesId();
                //
                //ResetRelativeTo
                if (null != slqAssetRateIndex)
                {
                    string resetRelativeTo = slqAssetRateIndex.Idx_RelativeToResetDt;
                    Irs.ResetDates.ResetRelativeToSpecified = false;
                    if (StrFunc.IsFilled(resetRelativeTo))
                    {
                        if (System.Enum.IsDefined(typeof(ResetRelativeToEnum), resetRelativeTo))
                        {
                            Irs.ResetDates.ResetRelativeToSpecified = true;
                            Irs.ResetDates.ResetRelativeTo = (ResetRelativeToEnum)System.Enum.Parse(typeof(ResetRelativeToEnum), resetRelativeTo, true);
                            //
                            if (CcisBase.Contains(CciClientId(CciEnum.resetDates_resetRelativeTo)))
                                CcisBase.SetNewValue(CciClientId(CciEnum.resetDates_resetRelativeTo), resetRelativeTo, false);
                        }
                    }
                }
                //                    
                //Fixing Dates
                Irs.ResetDates.InitialFixingDateSpecified = false;
                if (null != slqAssetRateIndex)
                {
                    //20061130 PL
                    //ccis.DumpRelativeDateOffset_ToDocument(Irs.resetDates.fixingDates, slqAssetRateIndex, Irs.resetDates.id, CCst.RESET_BUSINESS_CENTERS_REFERENCE + Number);
                    Ccis.DumpRelativeDateOffset_ToDocument(Irs.ResetDates.FixingDates, slqAssetRateIndex, GetResetDatesId(), TradeCustomCaptureInfos.CCst.RESET_BUSINESS_CENTERS_REFERENCE + NumberPrefix);
                }
                //      
                //RateCutOff Dates
                Irs.ResetDates.RateCutOffDaysOffsetSpecified = false;
                if (null != slqAssetRateIndex)
                {
                    if (null == Irs.ResetDates.RateCutOffDaysOffset)
                        Irs.ResetDates.RateCutOffDaysOffset = Irs.CreateOffset();
                    //
                    bool isSpecified = Ccis.DumpOffset_ToDocument(Irs.ResetDates.RateCutOffDaysOffset, slqAssetRateIndex, "RATECUTOFF");
                    Irs.ResetDates.RateCutOffDaysOffsetSpecified = isSpecified;
                }
                //
                //ResetFrequency
                SetResetFrequency();//Warning: also setting by Calculation Frequency
                //                
                if (null != slqAssetRateIndex)
                {
                    string weeklyRollConvention = slqAssetRateIndex.Asset_WeeklyRollConvResetDT;
                    if (System.Enum.IsDefined(typeof(WeeklyRollConventionEnum), weeklyRollConvention))
                    {
                        Irs.ResetDates.ResetFrequency.WeeklyRollConvention =
                            (WeeklyRollConventionEnum)System.Enum.Parse(typeof(WeeklyRollConventionEnum), weeklyRollConvention, true);
                    }
                }
            }
        }

        /// <summary>
        ///  Alimente CalculationPeriodDatesAdjustments en fonction de qui est patamétré sur le taux flottant 
        /// </summary>
        private void SetCalculationPeriodDatesAdjusments()
        {
            if (IsFloatingRateSpecified)
            {
                SetCalculationPeriodDatesBDC();
                DumpCalculationBDA();
            }

        }

        /// <summary>
        ///  Alimente PaymentDatesAdjustments en fonction de qui est patamétré sur le taux flottant 
        /// </summary>
        /// EG 20150824 [21254] Test sur IsFloatingRateSpecified || IsFixedRateSpecified
        private void SetPaymentDatesAdjusments()
        {
            //if (IsFloatingRateSpecified)
            //{
            //    SetPaymentDatesBDC();
            //    DumpPaymentBDA();
            //}
            
            SetPaymentDatesBDC();
            if (IsFloatingRateSpecified || IsFixedRateSpecified)
            {
                DumpPaymentBDA();
            }
        }
        
        /// <summary>
        /// Set RollConvention from EffectiveDate/FirsRegularStartDate and CalculationFrequencyPeriod
        /// </summary>
        private void SetRollConvention()
        {

            DateTime date = DateTime.MinValue;

            // FI 20190731 [XXXXX] pré-proposition de la rollconvention sur les titres
            if (this.cciTrade.Product.IsDebtSecurity)
            {
                //PL : remarque sur firstPeriodStartDate. Je ne suis pas retourné voir la définition FpML®, mais de mémoire elle doit indiquer que cette date est facultative et que quand elle existe elle est inférieure à effectiveDate
                //Sur la modélisation OTCml des Titres, de mémoire :
                //- j’ai détourné cette date pour la date à laquelle comment à courir les intérêts (ceux donc du 1er coupon).
                //- cette date est (quasiment) obligatoire, car nombreux sont les titres où les intérêts ne courent pas à partir de la date de valeur (effectiveDate)
                //- de plus la définition FpML® évoquée ci-dessus est violée puisque cette date est postérieure à la date de valeur (effectiveDate). Cela dit on est ici avec les Titres dans OTCml, plus dans FpML®.

                // Ordre de priorité sur les titres firstRegularPeriodStartDate, lastRegularPeriodEndDate, terminationDate
                if (Irs.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                    date = Irs.CalculationPeriodDates.FirstRegularPeriodStartDate.DateValue;
                else if (Irs.CalculationPeriodDates.LastRegularPeriodEndDateSpecified)
                    date = Irs.CalculationPeriodDates.LastRegularPeriodEndDate.DateValue;
                else if (Irs.CalculationPeriodDates.TerminationDateAdjustableSpecified)
                    date = Irs.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue;
            }
            else
            {
                //200911006 FI modification des priorité
                //on synchronise en fonction firstRegularPeriodStartDate
                //sinon on synchronise en fonction firstPeriodStartDate 
                //sinon on synchronise en fonction effectiveDateAdjustable 
                if (Irs.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                    date = Irs.CalculationPeriodDates.FirstRegularPeriodStartDate.DateValue;
                else if (Irs.CalculationPeriodDates.FirstPeriodStartDateSpecified)
                    date = Irs.CalculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.DateValue;
                else
                    date = Irs.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue;
            }

            if (DtFunc.IsDateTimeFilled(date))
            {
                string rollConvention = RollConventionEnum.NONE.ToString();
                PeriodEnum frequencyPeriod = Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval.Period;
                //Month or Year
                if ((PeriodEnum.M == frequencyPeriod) || (PeriodEnum.Y == frequencyPeriod))
                {
                    SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
                    //
                    if (null != slqAssetRateIndex && (StrFunc.IsFilled(slqAssetRateIndex.Asset_RollConvCalcDt)))
                    {
                        rollConvention = slqAssetRateIndex.Asset_RollConvCalcDt;
                        rollConvention = ((RollConventionEnum)System.Enum.Parse(typeof(RollConventionEnum), rollConvention, true)).ToString();
                    }
                    else
                    {
                        Calendar calendar = CultureInfo.CurrentCulture.Calendar;
                        if (calendar.GetDaysInMonth(date.Year, date.Month) == date.Day)
                            //Day is last month day --> EOM
                            rollConvention = RollConventionEnum.EOM.ToString();
                        else
                            rollConvention = "DAY" + date.Day.ToString();
                    }
                }

                string clientId = CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_rollConvention);
                if (CcisBase.Contains(clientId))
                {
                    CcisBase.SetNewValue(clientId, rollConvention);
                    CcisBase.Set(clientId, "IsEnabled", true);
                }

                else
                {
                    Irs.CalculationPeriodDates.CalculationPeriodFrequency.RollConvention =
                            (RollConventionEnum)System.Enum.Parse(typeof(RollConventionEnum), rollConvention, true);
                }
            }
        }
        
        /// <summary>
        /// Alimente les ccis qui expriment la fréquence (calculationPeriodDates_calculationPeriodFrequency) en fonction de l'asset
        /// <para>Uniquement lorsque la fréquence n'est déjà pas renseignée</para>
        /// </summary>
        /// EG 20150824 [21254]
        private void SetCalculationPeriodFrequency()
        {
            //ICalculation calculation = Irs.calculationPeriodAmount.calculation;
            //if (calculation.rateFloatingRateSpecified)
            if (IsFloatingRateSpecified)
            {
                //20091116 FI [16736] initialisation de la periode uniquement si elle est non renseignée
                bool initPerdiod = CcisBase[CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier)].IsEmptyValue;
                if (initPerdiod)
                {
                    if (IsFloatingRateSpecified)
                    {
                        SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
                        if (null != slqAssetRateIndex)
                        {
                            //20080620 PL/EPL Si RateIndex sans Tenor (ex.: EONIA) alors on considèrera "1 Terme"
                            string periodMltpCalcDt = slqAssetRateIndex.Asset_PeriodMltpCalcDt;
                            string periodCalcDt = slqAssetRateIndex.Asset_PeriodCalcDt;
                            if (StrFunc.IsEmpty(periodMltpCalcDt) || StrFunc.IsEmpty(periodCalcDt))
                            {
                                periodMltpCalcDt = 1.ToString();
                                periodCalcDt = FpML.Enum.PeriodEnum.T.ToString();
                            }
                            //
                            //20090514 RD/PL 
                            PeriodTofrequency.Translate(ref periodMltpCalcDt, ref periodCalcDt);
                            CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), periodMltpCalcDt);
                            CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_period), periodCalcDt);
                            //
                            if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency)))
                                CcisBase.SetNewValue(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency), periodMltpCalcDt + periodCalcDt);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Alimente le cci qui exprime le BDC des calculationPeriodDates (calculationPeriodDates_calculationPeriodDatesAdjustments_bDC)
        /// <para>Alimente l'élément FpML si le cci n'existe pas</para>
        /// <para>Alimentation uniquement si taux flottant</para>
        /// </summary>
        /// EG 20150824 [21254] IsFloatingRateSpecified
        private void SetCalculationPeriodDatesBDC()
        {
            //ICalculation calculation = Irs.calculationPeriodAmount.calculation;
            //if (calculation.rateFloatingRateSpecified && IsFloatingRateSpecified)
            if (IsFloatingRateSpecified)
            {
                SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
                //
                if (null != slqAssetRateIndex)
                {
                    if (CcisBase.Contains(CciClientId(CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC)))
                        Cci(CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC).NewValue = slqAssetRateIndex.Idx_BusinessDayConvention_CalcPeriod;
                    else
                        Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention = slqAssetRateIndex.FpML_Enum_BusinessDayConvention_CalcPeriod;
                }
            }
        }

        /// <summary>
        /// Alimente le cci paymentDates_paymentDatesAdjustments_bDC avec ce qui est paramétré sur le taux flottant
        /// <para>Alimente l'élément FpML si le cci n'existe pas</para>
        /// <para>Alimentation uniquement si taux flottant</para>
        /// </summary>
        /// EG 20150824 [21254] IsFloatingRateSpecified
        private void SetPaymentDatesBDC()
        {
            //ICalculation calculation = Irs.calculationPeriodAmount.calculation;
            //if (calculation.rateFloatingRateSpecified && IsFloatingRateSpecified)
            if (IsFloatingRateSpecified)
            {
                SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
                //
                if (null != slqAssetRateIndex)
                {
                    if (CcisBase.Contains(CciClientId(CciEnum.paymentDates_paymentDatesAdjustments_bDC)))
                        Cci(CciEnum.paymentDates_paymentDatesAdjustments_bDC).NewValue = slqAssetRateIndex.Idx_BusinessDayConvention_Payment;
                    else
                        Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention = slqAssetRateIndex.FpML_Enum_BusinessDayConvention_Payment;
                }
            }
        }
        
        /// <summary>
        /// Alimente le cci qui exprime le DayCountFraction
        /// <para>Alimentation uniquement si taux flottant</para>
        /// </summary>
        /// EG 20150824 [21254] IsFloatingRateSpecified
        private void SetDayCountFraction()
        {
            //ICalculation calculation = Irs.calculationPeriodAmount.calculation;
            //if (calculation.rateFloatingRateSpecified)
            if (IsFloatingRateSpecified)
            {
                Ccis.ProcessInitialize_DCF(CciClientId(CciEnum.calculationPeriodAmount_calculation_dayCountFraction),
                    CciClientId(CciEnum.calculationPeriodAmount_calculation_rate));
            }
        }

        /// <summary>
        /// Alimente Irs.calculationPeriodDates.calculationPeriodDatesAdjustments
        /// <para>Alimente Irs.calculationPeriodDates.effectiveDateAdjustable.dateAdjustments si le cci calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC n'existe pas</para>
        /// <para>Alimente Irs.calculationPeriodDates.terminationDate.dateAdjustments si le cci calculationPeriodDates_terminationDate_dateAdjustedDate_bDC n'existe pas</para>
        /// </summary>
        private void DumpCalculationBDA()
        {

            string clientId = CciClientId(CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.calculationPeriodAmount_knownAmountSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency), CciBC.TypeReferentialInfo.Currency }
            };

            //calculationPeriodDatesAdjustments
            Ccis.DumpBDC_ToDocument(Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments, clientId, GenerateId(TradeCustomCaptureInfos.CCst.PERIOD_BUSINESS_CENTERS_REFERENCE), cciBC);
            //
            if (false == Ccis.Contains(CciClientId(CciEnum.calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC)))
            {
                Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessDayConvention =
                    Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention;
                //
                Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessCentersDefineSpecified = false;
                if (false == Tools.IsBdcNoneOrNA(Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessDayConvention))
                {
                    Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessCentersReferenceSpecified = true;
                    Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessCentersReference.HRef =
                        Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefine.Id;
                }
                else
                {
                    Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessCentersReferenceSpecified = false;
                    Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments.BusinessCentersNoneSpecified = true;
                }
            }
            //
            if (false == Ccis.Contains(CciClientId(CciEnum.calculationPeriodDates_terminationDate_dateAdjustedDate_bDC)))
            {
                Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessDayConvention =
                    Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention;
                //
                Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessCentersDefineSpecified = false;
                if (false == Tools.IsBdcNoneOrNA(Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessDayConvention))
                {
                    Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessCentersReferenceSpecified = true;
                    Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessCentersReference.HRef = Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefine.Id;
                }
                else
                {
                    Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessCentersReferenceSpecified = false;
                    Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments.BusinessCentersNoneSpecified = true;
                }
            }
        }

        /// <summary>
        /// Alimente Irs.paymentDates.paymentDatesAdjustments
        /// </summary>
        private void DumpPaymentBDA()
        {
            string clientId = CciClientId(CciEnum.paymentDates_paymentDatesAdjustments_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.calculationPeriodAmount_knownAmountSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency), CciBC.TypeReferentialInfo.Currency }
            };
            Ccis.DumpBDC_ToDocument(Irs.PaymentDates.PaymentDatesAdjustments, clientId, GenerateId(TradeCustomCaptureInfos.CCst.PAYMENT_BUSINESS_CENTERS_REFERENCE), cciBC);
        }

        /// <summary>
        /// Alimente Irs.calculationPeriodDates.effectiveDateAdjustable.dateAdjustments
        /// </summary>
        private void DumpCalculationPeriodDatesEffectiveDateBDA()
        {
            string clientId = CciClientId(CciEnum.calculationPeriodDates_effectiveDate_dateAdjustedDate_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.calculationPeriodAmount_knownAmountSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency), CciBC.TypeReferentialInfo.Currency }
            };

            Ccis.DumpBDC_ToDocument(Irs.CalculationPeriodDates.EffectiveDateAdjustable.DateAdjustments, clientId, GenerateId(TradeCustomCaptureInfos.CCst.EFFECTIVE_BUSINESS_CENTERS_REFERENCE), cciBC);
        }

        /// <summary>
        /// Alimente Irs.calculationPeriodDates.effectiveDateAdjustable en fonction du cci calculationPeriodDates_effectiveDate
        /// </summary>
        private void DumpCalculationPeriodDatesEffectiveDate()
        {
            string data = Cci(CciEnum.calculationPeriodDates_effectiveDate).NewValue ;
            
            Irs.CalculationPeriodDates.EffectiveDateAdjustableSpecified = StrFunc.IsFilled(data);
            if (Irs.CalculationPeriodDates.EffectiveDateAdjustableSpecified)
            {
                Irs.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value = data;
                if (StrFunc.IsEmpty(Irs.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Id))
                    Irs.CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Id = GenerateId(TradeCustomCaptureInfos.CCst.EFFECTIVEDATE_REFERENCE);
            }

        }
        /// <summary>
        /// Alimente Irs.calculationPeriodDates.terminationDateAdjustable en fonction du cci calculationPeriodDates_terminationDate
        /// </summary>
        private void DumpCalculationPeriodDatesTerminationDate()
        {
            string data = Cci(CciEnum.calculationPeriodDates_terminationDate).NewValue;

            Irs.CalculationPeriodDates.TerminationDateAdjustableSpecified = StrFunc.IsFilled(data);
            if (Irs.CalculationPeriodDates.TerminationDateAdjustableSpecified)
            {
                Irs.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value = data;
                if (StrFunc.IsEmpty(Irs.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Id))
                    Irs.CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Id = GenerateId(TradeCustomCaptureInfos.CCst.TEMINATIONDATE_REFERENCE);
            }
        }

        /// <summary>
        /// Alimente Irs.calculationPeriodDates.terminationDateAdjustable.dateAdjustments
        /// </summary>
        private void DumpCalculationPeriodDatesTerminationDateBDA()
        {

            string clientId = CciClientId(CciEnum.calculationPeriodDates_terminationDate_dateAdjustedDate_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.calculationPeriodAmount_knownAmountSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                { CciClientId(CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency), CciBC.TypeReferentialInfo.Currency }
            };

            Ccis.DumpBDC_ToDocument(Irs.CalculationPeriodDates.TerminationDateAdjustable.DateAdjustments, clientId, GenerateId(TradeCustomCaptureInfos.CCst.TERMINATION_BUSINESS_CENTERS_REFERENCE), cciBC);
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void DumpFirstPeriodStartDateBDA()
        {
            string clientId = CciClientId(CciEnum.calculationPeriodDates_firstPeriodStartDate_dateAdjustedDate_bDC);

            if ((Irs.CalculationPeriodDates.FirstPeriodStartDateSpecified) && CcisBase.Contains(clientId))
            {
                CciBC cciBC = new CciBC(cciTrade)
                {
                    { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                    { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                    { CciClientId(CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                    { CciClientId(CciEnum.calculationPeriodAmount_knownAmountSchedule_currency), CciBC.TypeReferentialInfo.Currency },
                    { CciClientId(CciEnum.fxLinkedNotionalSchedule_varyingNotionalCurrency), CciBC.TypeReferentialInfo.Currency }
                };

                Ccis.DumpBDC_ToDocument(Irs.CalculationPeriodDates.FirstPeriodStartDate.DateAdjustments, clientId, GenerateId(TradeCustomCaptureInfos.CCst.FIRSTPERIODSTART_BUSINESS_CENTERS_REFERENCE), cciBC);
            }
        }
        
        /// <summary>
        /// Reourne l'asset de  taux du strem
        /// </summary>
        /// <returns></returns>
        /// Les ccistream peuvent ne pas avoir de calculationPeriodAmount_calculation_rate (Ex cciStreamGlobal) 
        /// Pour les initialisation à partir de l'asset il faut bien aller chercher l'info dans le document et ne pas s'appuyer sur le cii calculationPeriodAmount_calculation_rate
        private SQL_AssetRateIndex GetSqlAssetRateIndex()
        {
            SQL_AssetRateIndex ret = null;
            if (IsFloatingRateSpecified)
                ret = _irs.CalculationPeriodAmount.Calculation.RateFloatingRate.GetSqlAssetRateIndex(cciTrade.CSCacheOn);
            return ret;
        }

        /// <summary>
        /// Retourne l'id de Irs.calculationPeriodDates
        /// <para>Si l'id est vide, Spheres® en génère un</para>
        /// </summary>
        /// <returns></returns>
        private string GetCalculationPeriodDatesId()
        {
            string ret = GenerateId(TradeCustomCaptureInfos.CCst.CALCULATION_PERIOD_DATES_REFERENCE);
            if (StrFunc.IsEmpty(Irs.CalculationPeriodDates.Id))
                Irs.CalculationPeriodDates.Id = ret;

            ret = Irs.CalculationPeriodDates.Id;
            return ret;
        }
        
        /// <summary>
        /// Retourne l'id de Irs.resetDates
        /// <para>Si l'id est vide, Spheres® en génère un</para>
        /// </summary>
        /// <returns></returns>
        private string GetResetDatesId()
        {
            string ret = GenerateId(TradeCustomCaptureInfos.CCst.RESET_DATES_REFERENCE);
           
            if (StrFunc.IsEmpty(Irs.ResetDates.Id))
                Irs.ResetDates.Id = ret;
            
            ret = Irs.ResetDates.Id;
            
            return ret;
        }

        /// <summary>
        /// Alimente Irs.resetDates avec ce qui existe dans Irs.calculationPeriodDates lorsque le taux est un taux flottant
        /// </summary>
        private void SynchronizeResetDatesFromCalculationPeriodDates()
        {

            SQL_AssetRateIndex slqAssetRateIndex = GetSqlAssetRateIndex();
            //
            //ResetDatesAdjustments
            Irs.ResetDatesSpecified = false;
            if (null != slqAssetRateIndex)
            {
                Irs.ResetDatesSpecified = true;
                //
                //Bdc du reset = bdc du calculation period
                Irs.ResetDates.ResetDatesAdjustments.BusinessDayConvention =
                    Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention;
                //
                Irs.ResetDates.ResetDatesAdjustments.BusinessCentersNoneSpecified =
                    Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersNoneSpecified;
                //
                Irs.ResetDates.ResetDatesAdjustments.BusinessCentersReferenceSpecified = false;
                if (Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefineSpecified)
                {
                    Irs.ResetDates.ResetDatesAdjustments.BusinessCentersReferenceSpecified = true;
                    Irs.ResetDates.ResetDatesAdjustments.BusinessCentersReference.HRef =
                        Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersDefine.Efs_id.Value;
                }
                if (Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersReferenceSpecified)
                {
                    Irs.ResetDates.ResetDatesAdjustments.BusinessCentersReferenceSpecified = true;
                    Irs.ResetDates.ResetDatesAdjustments.BusinessCentersReference =
                        Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessCentersReference;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEltEnum"></param>
        /// <param name="pCciGlobal"></param>
        public void AddCci(CciStream.CciEnum pEltEnum, CustomCaptureInfo pCciGlobal)
        {
            if (!CcisBase.Contains(this.CciClientId(pEltEnum)))
            {
                CustomCaptureInfo cci = (CustomCaptureInfo)pCciGlobal.Clone(CustomCaptureInfo.CloneMode.CciAttribut);
                cci.ClientId = pCciGlobal.ClientId_Prefix + this.CciClientId(pEltEnum);
                CcisBase.Add(cci);
            }
        }

        /// <summary>
        /// Retourne l'id de Irs.paymentDates
        /// <para>Si l'id est vide, Spheres® en génère un </para>
        /// </summary>
        /// <returns></returns>
        private string GetPaymentDatesId()
        {
            string ret = GenerateId(TradeCustomCaptureInfos.CCst.PAYMENT_DATES_REFERENCE);
            if (StrFunc.IsEmpty(Irs.PaymentDates.Id))
                Irs.PaymentDates.Id = ret;

            ret = Irs.PaymentDates.Id;
            
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SQL_AssetFxRate GetSqlAssetFxRate()
        {
            SQL_AssetFxRate ret = null;
            if (IsFxLinkedNotionalScheduleSpecified)
                ret = _irs.CalculationPeriodAmount.Calculation.FxLinkedNotional.GetSqlAssetFxRate(cciTrade.CSCacheOn);
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKeyEnum"></param>
        /// <param name="pFloatingRate"></param>
        /// <param name="pData"></param>
        private void DumpFloatingRateCalculation_ToDocument(CciEnum pKeyEnum, IFloatingRate pFloatingRate, string pData)
        {
            bool isSpecified = StrFunc.IsFilled(pData);

            switch (pKeyEnum)
            {
                case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floatingRateMultiplierSchedule_initialValue:
                    #region MultiplierSchedule
                    isSpecified = isSpecified && (0 != DecFunc.DecValue(pData, CultureInfo.InvariantCulture));
                    pFloatingRate.FloatingRateMultiplierScheduleSpecified = isSpecified;
                    if (isSpecified)
                        pFloatingRate.FloatingRateMultiplierSchedule.InitialValue.Value = pData;
                    #endregion MultiplierSchedule
                    break;
                case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_spreadSchedule_initialValue:
                    #region SpreadSchedule
                    pFloatingRate.SpreadScheduleSpecified = isSpecified;
                    if (isSpecified)
                        pFloatingRate.SpreadSchedule.InitialValue.Value = pData;
                    #endregion SpreadSchedule
                    break;
                case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue:
                case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue:
                case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue:
                    #region CapRateSchedule_InitialValue
                    pFloatingRate.CapRateScheduleSpecified = isSpecified;
                    if (isSpecified)
                    {
                        IStrikeSchedule capRateSchedule;
                        if ((pKeyEnum == CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue) ||
                            (pKeyEnum == CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule_initialValue))
                        {
                            capRateSchedule = pFloatingRate.CapRateSchedule[0];
                        }
                        else if ((pKeyEnum == CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule2_initialValue) ||
                            (pKeyEnum == CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_capRateSchedule2_initialValue))
                        {
                            capRateSchedule = pFloatingRate.CapRateSchedule[1];
                        }
                        else
                        {
                            throw new NotImplementedException(StrFunc.AppendFormat("CCi {0} not implemented", pKeyEnum.ToString()));
                        }
                        //
                        capRateSchedule.InitialValue.Value = pData;
                        //=========================================
                        //20090825 PL TRIM 16494 NewModelCapFloor
                        //=========================================
                        //capRateSchedule.buyerSpecified = true;
                        //capRateSchedule.buyer = PayerReceiverEnum.Payer;
                        //capRateSchedule.sellerSpecified = true;
                        //capRateSchedule.seller = PayerReceiverEnum.Receiver;
                        if (!capRateSchedule.BuyerSpecified)
                        {
                            //Default FpML for compatiblity FpMl 1.0
                            capRateSchedule.BuyerSpecified = true;
                            capRateSchedule.Buyer = PayerReceiverEnum.Payer;
                        }
                        if (!capRateSchedule.SellerSpecified)
                        {
                            //Default FpML for compatiblity FpMl 1.0
                            capRateSchedule.SellerSpecified = true;
                            capRateSchedule.Seller = PayerReceiverEnum.Receiver;
                        }
                        //=========================================
                    }
                    #endregion CapRateSchedule_InitialValue
                    break;
                case CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                case CciEnum.calculationPeriodAmount_calculation_inflationRateCalculation_floorRateSchedule_initialValue:
                    #region FloorRateSchedule_InitialValue
                    pFloatingRate.FloorRateScheduleSpecified = isSpecified;
                    if (isSpecified)
                    {
                        IStrikeSchedule floorRateSchedule = pFloatingRate.FloorRateSchedule[0];
                        floorRateSchedule.InitialValue.Value = pData;
                        //=========================================
                        //20090825 PL TRIM 16494 NewModelCapFloor
                        //=========================================
                        //floorRateSchedule.buyerSpecified = true;
                        //floorRateSchedule.buyer = PayerReceiverEnum.Receiver;
                        //floorRateSchedule.sellerSpecified = true;
                        //floorRateSchedule.seller = PayerReceiverEnum.Payer;
                        if (!floorRateSchedule.BuyerSpecified)
                        {
                            //Default FpML for compatiblity FpMl 1.0
                            floorRateSchedule.BuyerSpecified = true;
                            floorRateSchedule.Buyer = PayerReceiverEnum.Receiver;
                        }
                        if (!floorRateSchedule.SellerSpecified)
                        {
                            //Default FpML for compatiblity FpMl 1.0
                            floorRateSchedule.SellerSpecified = true;
                            floorRateSchedule.Seller = PayerReceiverEnum.Payer;
                        }
                        //=========================================
                    }
                    #endregion FloorRateSchedule_InitialValue
                    break;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void SetVaryingNotionalFixingDates()
        {

            SQL_AssetFxRate sqlAssetFxRate = GetSqlAssetFxRate();
            //
            if (IsFxLinkedNotionalScheduleSpecified)
            {
                IFxLinkedNotionalSchedule fxLinkedNotional = Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional;
                IRelativeDateOffset varyingNotionalFixingDates = fxLinkedNotional.VaryingNotionalFixingDates;
                //
                IBusinessDayAdjustments bda;
                if (Irs.ResetDatesSpecified)
                {
                    varyingNotionalFixingDates.DateRelativeToValue = GetResetDatesId();
                    bda = Irs.ResetDates.ResetDatesAdjustments;
                }
                else
                {
                    varyingNotionalFixingDates.DateRelativeToValue = GetCalculationPeriodDatesId();
                    bda = Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments;
                }
                //
                if (null != sqlAssetFxRate)
                {
                    varyingNotionalFixingDates.Period = StringToEnum.Period(sqlAssetFxRate.PeriodSettlementTerm);
                    varyingNotionalFixingDates.PeriodMultiplier = new EFS_Integer(System.Math.Abs(sqlAssetFxRate.PeriodMultiplierSettlementTerm) * -1);
                    varyingNotionalFixingDates.DayTypeSpecified = true;
                    varyingNotionalFixingDates.DayType = DayTypeEnum.Business;
                    varyingNotionalFixingDates.BusinessDayConvention = BusinessDayConventionEnum.NONE;
                    //
                    varyingNotionalFixingDates.BusinessCentersNoneSpecified = bda.BusinessCentersNoneSpecified;
                    varyingNotionalFixingDates.BusinessCentersReferenceSpecified = bda.BusinessCentersReferenceSpecified || bda.BusinessCentersDefineSpecified;
                    varyingNotionalFixingDates.BusinessCentersDefineSpecified = false;
                    //
                    if (varyingNotionalFixingDates.BusinessCentersReferenceSpecified)
                    {
                        if (StrFunc.IsFilled(bda.BusinessCentersReference.HRef))
                            varyingNotionalFixingDates.BusinessCentersReference = bda.BusinessCentersReference;
                        else
                        {
                            if (bda.BusinessCentersDefineSpecified)
                                varyingNotionalFixingDates.BusinessCentersReference.HRef = bda.BusinessCentersDefine.Id;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void SetVaryingNotionalInterimExchangePaymentDates()
        {

            SQL_AssetFxRate sqlAssetFxRate = GetSqlAssetFxRate();
            //
            if (IsFxLinkedNotionalScheduleSpecified)
            {
                IFxLinkedNotionalSchedule fxLinkedNotional = Irs.CalculationPeriodAmount.Calculation.FxLinkedNotional;
                if (null == fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates)
                    fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates = Irs.CreateRelativeDateOffset();

                IRelativeDateOffset varyingNotionalInterimExchangePaymentDates = fxLinkedNotional.VaryingNotionalInterimExchangePaymentDates;

                varyingNotionalInterimExchangePaymentDates.DateRelativeToValue = GetPaymentDatesId();
                if (null != sqlAssetFxRate)
                {
                    varyingNotionalInterimExchangePaymentDates.GetInterval(0, PeriodEnum.D);
                    varyingNotionalInterimExchangePaymentDates.DayTypeSpecified = false;
                    varyingNotionalInterimExchangePaymentDates.BusinessDayConvention = BusinessDayConventionEnum.NONE;
                    varyingNotionalInterimExchangePaymentDates.BusinessCentersNoneSpecified = true;
                    varyingNotionalInterimExchangePaymentDates.BusinessCentersReferenceSpecified = false;
                    varyingNotionalInterimExchangePaymentDates.BusinessCentersDefineSpecified = false;
                }
            }

        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        private string GenerateId(string pKey)
        {
            if (StrFunc.IsFilled(PrefixId))
                pKey = StrFunc.FirstUpperCase(pKey); // pour respecter la norme hongroise

            string ret;
            if (number == 0)
            {
                //Cas d'un cciGlobal
                ret = cciTrade.DataDocument.GenerateId(PrefixId + pKey + "1", false);
            }
            else
            {
                ret = cciTrade.DataDocument.GenerateId(PrefixId + pKey + NumberPrefix, false);
            }
            return ret;

        }
        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = Cci(CciEnum.calculationPeriodAmount_calculation_rate);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_RATEINDEX);
        }
        #endregion
    }
}

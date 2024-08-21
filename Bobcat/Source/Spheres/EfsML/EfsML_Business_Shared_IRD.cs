#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_CalculationPeriod
    /// <revision>
    ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
    ///     <comment>Ticket 15889
    ///     Step dates: Unajusted versus Ajusted
    ///     Add public  member : BusinessDayAdjustments calculationPeriodDatesAdjustments;
    ///     Add private member : string                 m_Cs (ConnectionString)
    ///     </comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Member intervalFrequency (Use to calculate DCF in case of ACTISMA)
    ///     Used by EFS_DayCountFraction with an EFS_CalculationPeriod item (DayCountFraction Accessors)
    ///     </comment>
    /// </revision>
    public class EFS_CalculationPeriod : ICloneable
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        public EFS_AdjustablePeriod periodDates;
        public DayCountFractionEnum dayCountFractionEnum;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stubSpecified;
        public StubEnum stub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public RateTypeEnum stubRateType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_KnownAmountPeriod knownAmountPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CapFloored[] capFlooreds;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ResetDate[] resetDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool multiplierSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spreadSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal spread;
        // 20071029 EG Ticker 15889
        public IBusinessDayAdjustments calculationPeriodDatesAdjustments;
        // 20071106 EG Ticker 15859
        public IInterval intervalFrequency;
        #endregion Members
        #region Accessors
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date adjustedEndPeriod = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.adjustedDate.DateValue
                };
                return adjustedEndPeriod;

            }
        }

        #endregion AdjustedEndPeriod
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date adjustedStartPeriod = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.adjustedDate.DateValue
                };
                return adjustedStartPeriod;

            }
        }

        #endregion AdjustedStartPeriod
        #region DayCountFraction
        /// 20071106 EG Ticket 15859
        public EFS_DayCountFraction DayCountFraction(string pDayCountFraction)
        {

            if (!StrFunc.IsEmpty(pDayCountFraction))
            {
                DateTime startDate = periodDates.adjustableDate1.adjustedDate.DateValue;
                DateTime endDate = periodDates.adjustableDate2.adjustedDate.DateValue;
                DayCountFractionEnum dayCountFractionEnum = (DayCountFractionEnum)System.Enum.Parse(typeof(DayCountFractionEnum),
                    pDayCountFraction);
                return new EFS_DayCountFraction(startDate, endDate, dayCountFractionEnum, intervalFrequency);
            }
            return null;

        }
        #endregion DayCountFraction
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate2);

            }
        }

        #endregion EndPeriod
        #region Multiplier
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Multiplier
        {
            get
            {

                if (multiplierSpecified)
                    return multiplier;
                return null;

            }
        }
        #endregion Multiplier
        #region PeriodAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PeriodAmount
        {
            get
            {
                //20050919 PL/FDA Suite à modif de EFS_Decimal
                //this.knownAmountPeriod.amount.Value = (string) Tools.FormatValue(this.knownAmountPeriod.amount);
                if (null != this.knownAmountPeriod)
                    return this.knownAmountPeriod.amount;
                else
                    return null;


            }
        }
        #endregion PeriodAmount
        #region PeriodCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PeriodCurrency
        {
            get
            {

                if (null != this.knownAmountPeriod)
                    return this.knownAmountPeriod.currency;
                return null;

            }
        }
        #endregion PeriodCurrency
        #region Spread
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Spread
        {
            get
            {

                if (spreadSpecified)
                    return spread;
                return null;

            }
        }
        #endregion Spread
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate1);

            }
        }
        #endregion StartPeriod
        #region Stub
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StubEnum Stub
        {
            get
            {
                return this.stub;

            }
        }

        #endregion Stub
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                EFS_Date unadjustedEndPeriod = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return unadjustedEndPeriod;

            }
        }

        #endregion UnadjustedEndPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date unadjustedStartPeriod = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return unadjustedStartPeriod;

            }
        }

        #endregion UnadjustedStartPeriod
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CalculationPeriod(string pConnectionString, IInterval pIntervalPeriodFrequency, DataDocumentContainer pDataDocument)
        {
            // 20071029 EG Ticker 15889
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDates = new EFS_AdjustablePeriod();
            //stubSpecified = false;
            stub = StubEnum.None;
            dayCountFractionEnum = DayCountFractionEnum.ACT360;
            intervalFrequency = pIntervalPeriodFrequency;
        }
        #endregion Constructors
        #region Methods
        #region AssetRateIndex
        // EG 20140904 Add AssetCategory
        public object AssetRateIndex(object pCalculationPeriodAmount, string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {

            EFS_Asset asset = null;
            object rate = Rate(pCalculationPeriodAmount, pRateType, pRate, pRateInitialStub, pRateFinalStub);
            if (null != rate)
            {
                asset = new EFS_Asset
                {
                    idAsset = ((EFS_Integer)rate).IntValue,
                    assetCategory = Cst.UnderlyingAsset.RateIndex
                };
            }
            return asset;

        }
        #endregion AssetRateIndex
        #region EventType
        // EG 20230216 [26600] Correction EVT sur PER/FIX|FLO EVENTTYPE (Test ICalculationPeriodAmount)
        public string EventType(object pCalculationPeriodAmount, object pRate, object pRateInitialStub, object pRateFinalStub)
        {

            string eventType = "N/A";
            Type tObject = pCalculationPeriodAmount.GetType();
            if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.INotional) ||
                Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.ICalculationPeriodAmount) ||
                Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFxLinkedNotionalSchedule))
            {
                if ((StubEnum.Initial == stub) && (null != pRateInitialStub))
                    tObject = pRateInitialStub.GetType();
                else if ((StubEnum.Final == stub) && (null != pRateFinalStub))
                    tObject = pRateFinalStub.GetType();
                else
                    tObject = pRate.GetType();

                if (tObject.Equals(typeof(EFS_Decimal)) || Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.ISchedule))
                    eventType = EventTypeFunc.FixedRate;

                else if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFloatingRate) ||
                         Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFloatingRate, "FloatingRate[]") ||
                    Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFloatingRateCalculation) ||
                         Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IInflationRateCalculation))
                    eventType = EventTypeFunc.FloatingRate;
                // 20071015 EG StubAmount case
                else if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IMoney))
                    eventType = EventTypeFunc.KnownAmount;
            }
            else if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IAmountSchedule) ||
                     Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IKnownAmountSchedule))
                eventType = EventTypeFunc.KnownAmount;
            //
            return eventType;

        }
        #endregion EventType
        #region GetFixedRate
        // 20071029 EG Ticker 15889
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private EFS_Decimal GetFixedRate(object pRate)
        {

            EFS_Decimal rate = null;
            Type tObject = pRate.GetType();
            if (tObject.Equals(typeof(EFS_Decimal)))
                rate = (EFS_Decimal)pRate;
            else if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.ISchedule))
            {
                ISchedule schedule = (ISchedule)pRate;
                rate = new EFS_Decimal(schedule.InitialValue.DecValue);
                if (schedule.StepSpecified)
                {
                    DateTime dtStart = StartPeriod.adjustedDate.DateValue;
                    DateTime dtEnd = EndPeriod.adjustedDate.DateValue;
                    rate = new EFS_Decimal(Tools.GetStepValue(m_Cs, schedule, dtStart, dtEnd, 
                        (IBusinessDayAdjustments)calculationPeriodDatesAdjustments, m_DataDocument));
                }
            }
            return rate;

        }
        #endregion GetFixedRate
        #region GetFloatingRate
        private static EFS_Integer GetFloatingRate(RateTypeEnum pRateTypeEnum, object pRate)
        {
            IFloatingRate floatingRate = null;

            Type tObject = pRate.GetType();
            if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFloatingRate) ||
                Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFloatingRate, "FloatingRate[]") ||
                Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFloatingRateCalculation))
            {
                if (tObject.IsArray)
                {
                    Array rate = (Array)pRate;
                    if (RateTypeEnum.FloatingRate == pRateTypeEnum)
                        floatingRate = (IFloatingRate)rate.GetValue(0);
                    else if (1 < rate.Length)
                        floatingRate = (IFloatingRate)rate.GetValue(1);
                }
                else
                    floatingRate = (IFloatingRate)pRate;
            }

            if (null != floatingRate)
                return new EFS_Integer(floatingRate.FloatingRateIndex.OTCmlId);
            return null;
        }

        #endregion GetFloatingRate
        #region Rate
        public object Rate(object pCalculationPeriodAmount, string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {
            Type tObject;

            tObject = pCalculationPeriodAmount.GetType();
            if (Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.INotional) ||
                Tools.IsTypeOrInterfaceOf(tObject, InterfaceEnum.IFxLinkedNotionalSchedule))
            {
                RateTypeEnum rateTypeEnum = (RateTypeEnum)System.Enum.Parse(typeof(RateTypeEnum), pRateType, true);
                if (RateTypeEnum.FixedRate == rateTypeEnum)
                {
                    if ((StubEnum.Initial == stub) && (null != pRateInitialStub))
                        return GetFixedRate(pRateInitialStub);
                    else if ((StubEnum.Final == stub) && (null != pRateFinalStub))
                        return GetFixedRate(pRateFinalStub);
                    else
                        return GetFixedRate(pRate);
                }
                else if (RateTypeEnum.FloatingRate == rateTypeEnum || RateTypeEnum.FloatingRate2 == rateTypeEnum)
                {
                    if ((StubEnum.Initial == stub) && (null != pRateInitialStub))
                        return GetFloatingRate(rateTypeEnum, pRateInitialStub);
                    else if ((StubEnum.Final == stub) && (null != pRateFinalStub))
                        return GetFloatingRate(rateTypeEnum, pRateFinalStub);
                    else if (RateTypeEnum.FloatingRate == rateTypeEnum)
                        return GetFloatingRate(RateTypeEnum.FloatingRate, pRate);
                }
            }
            return null;

        }
        #endregion Rate
        #endregion Methods

        #region ICloneable Members
        #region Clone
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Clone()
        {
            // 20071029 EG Ticker 15889
            EFS_CalculationPeriod clone = new EFS_CalculationPeriod(this.m_Cs, intervalFrequency, m_DataDocument)
            {
                periodDates = (EFS_AdjustablePeriod)this.periodDates.Clone()
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
    }
    #endregion EFS_CalculationPeriod
    #region EFS_CalculationPeriodDates
    /// <revision>
    ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
    ///     <comment>Ticket 15889
    ///     Step dates: Unajusted versus Ajusted
    ///     Add public member : BusinessDayAdjustments calculationPeriodDatesAdjustments;
    ///     Indexor are updated
    ///     </comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Parameter 
    ///     pCalculationPeriodDates.calculationPeriodFrequency.IntervalCalculationPeriodFrequency to InsertArrayCalculationPeriod
    ///     Used by EFS_DayCountFraction in EFS_CalculationPeriod
    ///     </comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0.1</version><date>20071130</date><author>EG</author>
    ///     <comment>Ticket 15998
    ///     CalculNotionalStepParameters
    ///     </comment>
    /// </revision>
    public class EFS_CalculationPeriodDates
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        private readonly EFS_SettlementInfoEntity m_PreSettlementInfo;
        [System.Xml.Serialization.XmlArrayItemAttribute("calculationPeriod")]
        public EFS_CalculationPeriod[] calculationPeriods;
        [System.Xml.Serialization.XmlArrayItemAttribute("nominalPeriod")]
        public EFS_NominalPeriod[] nominalPeriods;
        [System.Xml.Serialization.XmlArrayItemAttribute("fxLinkedNominalPeriod")]
        public EFS_FxLinkedNominalPeriod[] fxLinkedNominalPeriods;
        public EFS_AdjustableDate effectiveDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstPeriodStartDateAdjustmentSpecified;
        public EFS_AdjustableDate firstPeriodStartDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool firstRegularPeriodStartDateAdjustmentSpecified;
        public EFS_AdjustableDate firstRegularPeriodStartDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lastRegularPeriodEndDateAdjustmentSpecified;
        public EFS_AdjustableDate lastRegularPeriodEndDateAdjustment;
        public EFS_AdjustableDate terminationDateAdjustment;
        public EFS_AdjustablePeriod regularPeriodDatesAdjustment;
        // 20071029 EG Ticker 15889
        public IBusinessDayAdjustments calculationPeriodDatesAdjustments;
        #region Stub Variables
        private readonly bool initialStubFloatingRateSpecified;
        private readonly bool initialStubFixedRateSpecified;
        private readonly bool initialStubAmountSpecified;
        private readonly bool finalStubFloatingRateSpecified;
        private readonly bool finalStubFixedRateSpecified;
        private readonly bool finalStubAmountSpecified;
        #endregion Stub Variables
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CalculationPeriodDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190823 [FIXEDINCOME] Upd (cas du Perpetual DebtSecurity) 
        // EG 20191011 [FIXEDINCOME] Upd (Correction déjà effectuée sur Caracal 20190926)
        public EFS_CalculationPeriodDates(string pConnectionString, ICalculationPeriodDates pCalculationPeriodDates, IInterestRateStream pStream, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            ICalculationPeriodAmount calculationPeriodAmount = pStream.CalculationPeriodAmount;
            IStubCalculationPeriodAmount stubCalculationPeriodAmount = pStream.StubCalculationPeriodAmount;
            // 20071029 EG Ticket 15889
            calculationPeriodDatesAdjustments = pCalculationPeriodDates.CalculationPeriodDatesAdjustments;

            // 20070823 EG Ticket : 15643
            #region PreSettlementInfo
            m_PreSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, pStream.StreamCurrency, pStream.PayerPartyReference.HRef,
                pStream.ReceiverPartyReference.HRef, m_DataDocument, calculationPeriodDatesAdjustments.DefaultOffsetPreSettlement);
            #endregion PreSettlementInfo


            //firstPeriodStartDateAdjustmentSpecified = false;
            //firstRegularPeriodStartDateAdjustmentSpecified = false;
            //lastRegularPeriodEndDateAdjustmentSpecified = false;

            #region Stub RateType (use to determine if Stub CalculationPeriods have Reset)
            if (pCalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
            {
                if ((null != stubCalculationPeriodAmount) && (stubCalculationPeriodAmount.InitialStubSpecified))
                {
                    initialStubAmountSpecified = stubCalculationPeriodAmount.InitialStub.StubTypeAmountSpecified;
                    initialStubFixedRateSpecified = stubCalculationPeriodAmount.InitialStub.StubTypeFixedRateSpecified;
                    initialStubFloatingRateSpecified = stubCalculationPeriodAmount.InitialStub.StubTypeFloatingRateSpecified;
                }
                else
                {
                    initialStubFixedRateSpecified = (false == pStream.ResetDatesSpecified);
                    initialStubFloatingRateSpecified = (pStream.ResetDatesSpecified);
                }
            }
            if (pCalculationPeriodDates.LastRegularPeriodEndDateSpecified)
            {
                if ((null != stubCalculationPeriodAmount) && (stubCalculationPeriodAmount.FinalStubSpecified))
                {
                    finalStubAmountSpecified = stubCalculationPeriodAmount.FinalStub.StubTypeAmountSpecified;
                    finalStubFixedRateSpecified = stubCalculationPeriodAmount.FinalStub.StubTypeFixedRateSpecified;
                    finalStubFloatingRateSpecified = stubCalculationPeriodAmount.FinalStub.StubTypeFloatingRateSpecified;
                }
                else
                {
                    finalStubFixedRateSpecified = (false == pStream.ResetDatesSpecified);
                    finalStubFloatingRateSpecified = (pStream.ResetDatesSpecified);
                }

            }
            Cst.ErrLevel ret;
            #endregion Stub RateType (use to determine if Stub CalculationPeriods have Reset)

            if (m_DataDocument.CurrentProduct.IsDebtSecurity)
            {
                IDebtSecurity debtSecurity = m_DataDocument.CurrentProduct.ProductBase as IDebtSecurity;
                Nullable<DateTime> prevCouponDate = null;
                if (debtSecurity.PrevCouponDateSpecified)
                    prevCouponDate = debtSecurity.PrevCouponDate.DateValue;
                ret = Calc2(
                    (debtSecurity.DebtSecurityType == DebtSecurityTypeEnum.Perpetual) ? CalPeriodEnum.FirstAndRegular : CalPeriodEnum.All,
                    pCalculationPeriodDates, null, prevCouponDate);

            }
            else
            {
                ret = Calc(pCalculationPeriodDates);
            }

            if (ret == Cst.ErrLevel.SUCCESS)
            {
                if (calculationPeriodAmount.CalculationSpecified && calculationPeriodAmount.Calculation.NotionalSpecified)
                {
                    ret = CreateNotionalPeriod();
                    if ((ret == Cst.ErrLevel.SUCCESS) && calculationPeriodAmount.Calculation.NotionalSpecified)
                        ret = CalcNotional(calculationPeriodAmount.Calculation, pCalculationPeriodDates.CalculationPeriodFrequency);

                    // 20071015 Eg StubAmount Case (virtual knownAmountperiod)
                    IMoney stubAmount;
                    if (initialStubAmountSpecified)
                    {
                        stubAmount = stubCalculationPeriodAmount.InitialStub.StubTypeAmount;
                        calculationPeriods[0].knownAmountPeriod = new EFS_KnownAmountPeriod(stubAmount.Amount.DecValue, stubAmount.Currency);
                    }
                    if (finalStubAmountSpecified)
                    {
                        stubAmount = stubCalculationPeriodAmount.FinalStub.StubTypeAmount;
                        calculationPeriods[calculationPeriods.Length - 1].knownAmountPeriod = new EFS_KnownAmountPeriod(stubAmount.Amount.DecValue, stubAmount.Currency);
                    }
                    // endcase
                }
                else if (calculationPeriodAmount.KnownAmountScheduleSpecified)
                {
                    ret = CreateKnownAmountPeriod(calculationPeriodAmount.KnownAmountSchedule, stubCalculationPeriodAmount);
                }
            }

            if ((ret == Cst.ErrLevel.SUCCESS) &&
                pStream.CalculationPeriodAmount.CalculationSpecified &&
                (pStream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified ||
                 pStream.CalculationPeriodAmount.Calculation.RateInflationRateSpecified))
            {
                // 20090826 EG Ticket : 16656
                _ = SetMultiplierAndSpreadValue(pStream.CalculationPeriodAmount.Calculation, stubCalculationPeriodAmount);
            }
        }
        #endregion Constructors
        #region Indexors
        // 20071029 EG Ticker 15889 Based on AdjustedDate
        public EFS_CalculationPeriod this[DateTime pDate]
        {
            get
            {

                for (int i = 0; i < calculationPeriods.Length; i++)
                {
                    if ((pDate == calculationPeriods[i].periodDates.adjustableDate1.adjustedDate.DateValue))
                        return calculationPeriods[i];
                }

                return null;
            }
        }
        #endregion Indexors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculate for a stream all unadjusted and adjusted calculation periods dates and 
        /// other related dates (the date adjustments being automatically calculated through the EFS_CalculationPeriodDates type).
        /// </summary>
        /// <param name="pCalculationPeriodDates">FpML Stream CalculationPeriodDates element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc(ICalculationPeriodDates pCalculationPeriodDates)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            StubEnum stub = StubEnum.None;
            RateTypeEnum stubRateType = RateTypeEnum.None;
            //
            ArrayList aCalculationPeriods = new ArrayList();
            EFS_AdjustableDate adjustableDate1 = new EFS_AdjustableDate(m_Cs, m_DataDocument);
            _ = new EFS_AdjustableDate(m_Cs, m_DataDocument);
            // 20071106 EG Ticket 15859
            IInterval intervalPeriodFrequency = pCalculationPeriodDates.CalculationPeriodFrequency.Interval;
            //

            #region Other Adjustable Dates
            // EffectiveDateAdjustment - TerminationDateAdjustment
            this.effectiveDateAdjustment = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.EffectiveDateAdjustable, m_DataDocument);
            this.terminationDateAdjustment = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.TerminationDateAdjustable, m_DataDocument);
            // FirstPeriodStartDateAdjustment
            this.firstPeriodStartDateAdjustmentSpecified = pCalculationPeriodDates.FirstPeriodStartDateSpecified;
            if (this.firstPeriodStartDateAdjustmentSpecified)
                this.firstPeriodStartDateAdjustment = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.FirstPeriodStartDate, m_DataDocument);
            // FirstRegularPeriodStartDateAdjustment / LastRegularPeriodEndDateAdjustment / RegularPeriodDatesAdjustment
            this.firstRegularPeriodStartDateAdjustmentSpecified = pCalculationPeriodDates.FirstRegularPeriodStartDateSpecified;
            this.regularPeriodDatesAdjustment = new EFS_AdjustablePeriod();
            if (this.firstRegularPeriodStartDateAdjustmentSpecified)
            {
                this.firstRegularPeriodStartDateAdjustment = new EFS_AdjustableDate(m_Cs,
                    pCalculationPeriodDates.FirstRegularPeriodStartDate.DateValue, pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);
                this.regularPeriodDatesAdjustment.adjustableDate1 = (EFS_AdjustableDate)this.firstRegularPeriodStartDateAdjustment.Clone();
            }
            else
                this.regularPeriodDatesAdjustment.adjustableDate1 = (EFS_AdjustableDate)this.effectiveDateAdjustment.Clone();
            // 
            this.lastRegularPeriodEndDateAdjustmentSpecified = pCalculationPeriodDates.LastRegularPeriodEndDateSpecified;
            if (this.lastRegularPeriodEndDateAdjustmentSpecified)
            {
                this.lastRegularPeriodEndDateAdjustment = new EFS_AdjustableDate(m_Cs,
                    pCalculationPeriodDates.LastRegularPeriodEndDate.DateValue, pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);
                this.regularPeriodDatesAdjustment.adjustableDate2 = (EFS_AdjustableDate)this.lastRegularPeriodEndDateAdjustment.Clone();
            }
            else
                this.regularPeriodDatesAdjustment.adjustableDate2 = (EFS_AdjustableDate)this.terminationDateAdjustment.Clone();
            #endregion Other Adjustable Dates
            #region Calculate All periods start and end dates
            #region Case First Period
            if (pCalculationPeriodDates.FirstPeriodStartDateSpecified)
                adjustableDate1.AdjustableDate = (IAdjustableDate)pCalculationPeriodDates.FirstPeriodStartDate.Clone();
            else
                adjustableDate1.AdjustableDate = (IAdjustableDate)pCalculationPeriodDates.EffectiveDateAdjustable.Clone();
            EFS_AdjustableDate adjustableDate2;
            if (pCalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
            {
                stub = StubEnum.Initial;
                #region StubRateType
                if (this.initialStubAmountSpecified)
                    stubRateType = RateTypeEnum.None;
                else if (this.initialStubFixedRateSpecified)
                    stubRateType = RateTypeEnum.FixedRate;
                else if (this.initialStubFloatingRateSpecified)
                    stubRateType = RateTypeEnum.FloatingRate;
                #endregion StubRateType

                adjustableDate2 = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.FirstRegularPeriodStartDate.DateValue,
                    pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);
            }
            else
            {
                adjustableDate2 = Tools.ApplyInterval(adjustableDate1,
                    pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue, pCalculationPeriodDates.CalculationPeriodFrequency.Interval);

                // 20070316 EG Add calculationPeriodDatesAdjustments to Date : Ticket 15378
                adjustableDate2.AdjustableDate.DateAdjustments = (IBusinessDayAdjustments)
                    pCalculationPeriodDates.CalculationPeriodDatesAdjustments.Clone();

                adjustableDate2 = Tools.ApplyRollConvention(adjustableDate2,
                    pCalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue, pCalculationPeriodDates.CalculationPeriodFrequency.RollConvention);
            }
            // Insert First Period
            // 20071106 EG Ticket 15859
            InsertArrayCalculationPeriod(ref aCalculationPeriods, adjustableDate1, adjustableDate2, stub, stubRateType, intervalPeriodFrequency);
            #endregion Case First Period
            #region Case Regular Period
            if (adjustableDate1.adjustableDateSpecified && adjustableDate2.adjustableDateSpecified)
            {
                int guard = 0;
                stub = StubEnum.None;
                stubRateType = RateTypeEnum.None;
                while (true)
                {
                    guard++;
                    if (guard == 999)
                    {
                        //Loop parapet
                        string msgException = "Calculation Period Dates exception:" + Cst.CrLf;
                        msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                        msgException += "Please, verify dates, periods, business day adjustment and roll convention on the trade";
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                    }
                    adjustableDate1 = (EFS_AdjustableDate)((EFS_CalculationPeriod)
                        aCalculationPeriods[aCalculationPeriods.Count - 1]).periodDates.adjustableDate2.Clone();

                    adjustableDate2 = Tools.ApplyInterval(adjustableDate1,
                        pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue,
                        pCalculationPeriodDates.CalculationPeriodFrequency.Interval);
                    adjustableDate2 = Tools.ApplyRollConvention(adjustableDate2,
                        pCalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue,
                        pCalculationPeriodDates.CalculationPeriodFrequency.RollConvention);

                    if (pCalculationPeriodDates.LastRegularPeriodEndDateSpecified &&
                        adjustableDate2.AdjustableDate.UnadjustedDate.DateValue >= pCalculationPeriodDates.LastRegularPeriodEndDate.DateValue)
                    {
                        adjustableDate2 = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.LastRegularPeriodEndDate.DateValue,
                            pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);

                        // Insert Last Regular Intermediary Period
                        // 20071106 EG Ticket 15859
                        InsertArrayCalculationPeriod(ref aCalculationPeriods, adjustableDate1, adjustableDate2, stub, stubRateType, intervalPeriodFrequency);
                        adjustableDate1 = (EFS_AdjustableDate)((EFS_CalculationPeriod)
                            aCalculationPeriods[aCalculationPeriods.Count - 1]).periodDates.adjustableDate2.Clone();
                        adjustableDate2 = new EFS_AdjustableDate(m_Cs, m_DataDocument);
                        #region StubRateType
                        stub = StubEnum.Final;
                        if (this.finalStubAmountSpecified)
                            stubRateType = RateTypeEnum.None;
                        else if (this.finalStubFixedRateSpecified)
                            stubRateType = RateTypeEnum.FixedRate;
                        else if (this.finalStubFloatingRateSpecified)
                            stubRateType = RateTypeEnum.FloatingRate;
                        #endregion StubRateType
                        ret = Cst.ErrLevel.SUCCESS;
                        break;
                    }
                    else if (adjustableDate2.AdjustableDate.UnadjustedDate.DateValue < pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue)
                    {
                        adjustableDate2.AdjustableDate.DateAdjustments = (IBusinessDayAdjustments)
                            pCalculationPeriodDates.CalculationPeriodDatesAdjustments.Clone();
                        // Insert Intermediary Period
                        // 20071106 EG Ticket 15859
                        InsertArrayCalculationPeriod(ref aCalculationPeriods, adjustableDate1, adjustableDate2, stub, stubRateType, intervalPeriodFrequency);
                        continue;
                    }
                    ret = Cst.ErrLevel.SUCCESS;
                    break;
                }
            }
            #endregion Case Regular Period
            #region Case Last Period
            if ((ret == Cst.ErrLevel.SUCCESS) &&
                (pCalculationPeriodDates.LastRegularPeriodEndDateSpecified ||
                (adjustableDate1.adjustedDate.DateValue < this.terminationDateAdjustment.adjustedDate.DateValue)))
            {
                adjustableDate2.AdjustableDate = (IAdjustableDate)pCalculationPeriodDates.TerminationDateAdjustable.Clone();
                // Insert Last Period
                // 20071106 EG Ticket 15859
                InsertArrayCalculationPeriod(ref aCalculationPeriods, adjustableDate1, adjustableDate2, stub, stubRateType, intervalPeriodFrequency);
            }
            #endregion Case Last Period
            #region Final Affectation
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                // CalculationPeriods
                if (aCalculationPeriods.Count > 0)
                    this.calculationPeriods = (EFS_CalculationPeriod[])aCalculationPeriods.ToArray(aCalculationPeriods[0].GetType());
            }
            #endregion Other Adjustable Dates
            #endregion Calculate All periods start and end dates

            return ret;
        }
        #endregion Calc
        #region CalcNotional
        /// <summary>
        /// Calculate and store the calculation periods notional amounts and variation amounts such as resulting from 
        /// the combination of the notionalStepSchedule and notionzalStepParameters elements
        /// </summary>
        /// <param name="pCalculation">Element defining the parameters used in the calculation of fixed or 
        /// floating rate calculation period amounts 
        /// </param>
        /// <param name="pCalculationPeriodFrequency">Stream CalculationPeriodDates Frequency element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        /// <remarks>The calculation periods in the calcPeriods parameter are supposed to be ordered by
        /// increasing period start dates.
        /// </remarks>
        private Cst.ErrLevel CalcNotional(ICalculation pCalculation, ICalculationPeriodFrequency pCalculationPeriodFrequency)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            #region Notional
            if (pCalculation.NotionalSpecified)
            {
                INotional notional = pCalculation.Notional;
                string currency = notional.StepSchedule.Currency.Value;
                #region Notional Step Schedule
                ret = CalcNotionalStepSchedule(notional.StepSchedule);
                #endregion Notional Step Schedule
                #region Notional Step Parameters
                if ((ret == Cst.ErrLevel.SUCCESS) && notional.StepParametersSpecified)
                    // Notional Step Parameters
                    ret = CalcNotionalStepParameters(notional.StepSchedule, notional.StepParameters,
                        pCalculationPeriodFrequency.Interval, pCalculationPeriodFrequency.RollConvention);
                #endregion Notional Step Parameters
                #region NominalPeriod Final treatment
                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    decimal MtPreviousAmount = notional.StepSchedule.InitialValue.DecValue;
                    decimal MtPreviousPreviousAmount = 0;
                    EFS_AdjustableDate startPeriod = null;
                    EFS_AdjustableDate firstPeriodStart = null;
                    ArrayList aNominalPeriods = new ArrayList();

                    foreach (EFS_NominalPeriod nominalPeriod in nominalPeriods)
                    {
                        if (MtPreviousAmount != nominalPeriod.periodAmount.DecValue)
                        {
                            if (null != startPeriod)
                            {
                                InsertArrayNominalPeriod(ref aNominalPeriods, startPeriod, nominalPeriod.periodDates.adjustableDate1, firstPeriodStart,
                                    MtPreviousAmount, currency, MtPreviousAmount - MtPreviousPreviousAmount);
                                startPeriod = nominalPeriod.periodDates.adjustableDate1;
                                firstPeriodStart = startPeriod;
                            }
                            MtPreviousPreviousAmount = MtPreviousAmount;
                            MtPreviousAmount = nominalPeriod.periodAmount.DecValue;
                        }
                        else if (null == startPeriod)
                        {
                            startPeriod = nominalPeriod.periodDates.adjustableDate1;
                            firstPeriodStart = startPeriod;
                            if (nominalPeriod.firstPeriodStartDateSpecified)
                                firstPeriodStart = nominalPeriod.firstPeriodStartDate;
                        }
                    }

                    #region Set NominalPeriod
                    // Last Period
                    if (0 != System.Math.Sign(MtPreviousAmount - MtPreviousPreviousAmount))
                        InsertArrayNominalPeriod(ref aNominalPeriods, startPeriod, terminationDateAdjustment, firstPeriodStart,
                            MtPreviousAmount, currency, MtPreviousAmount - MtPreviousPreviousAmount);
                    InsertArrayNominalPeriod(ref aNominalPeriods, startPeriod, terminationDateAdjustment, firstPeriodStart,
                        MtPreviousAmount, currency, MtPreviousAmount);

                    if (aNominalPeriods.Count > 0)
                        this.nominalPeriods = (EFS_NominalPeriod[])aNominalPeriods.ToArray(aNominalPeriods[0].GetType());
                    #endregion Set NominalPeriod

                }
                #endregion NominalPeriod Final treatment
            }
            #endregion Notional

            return ret;

        }
        #endregion CalcNotional
        #region CalcNotionalStepParameters
        /// <summary>
        /// Calculate and store the calculation periods notional variation amounts such as resulting from 
        /// the notionalStepParameters element.
        /// </summary>
        /// <param name="pNotionalStepSchedule">Stream NotionalStepSchedule element</param>
        /// <param name="pNotionalStepParameters">Stream NotionalStepParameters element</param>
        /// <param name="pCalculationPeriodFrequency">Stream CalculationPeriodDates Frequency element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        /// <remarks>The calculation periods in the calcPeriods parameter are supposed to be ordered by
        /// increasing period start dates.
        /// </remarks>
        /// <revision>
        ///     <version>1.2.0.1</version><author>EG</author><date>20071130</date>
        ///     <comment>Ticket 15998
        ///     StepDate control with AdjustedDate
        ///     VirtualLastStepDate used to include the LastStepDate for the application of the StepFrequency
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>2.4.0.0</version><author>PL</author><date>20100217</date>
        ///     <comment>Refactoring and Add Rounded
        ///     </comment>
        /// </revision>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcNotionalStepParameters(IAmountSchedule pNotionalStepSchedule, INotionalStepRule pNotionalStepParameters,
            IInterval pCalculationPeriodFrequency, RollConventionEnum pRollConvention)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            #region Validation Rules & Multiplier Calculation
            // 20071130 EG Ticker 15998
            if (false == pNotionalStepParameters.IsStepDateCalculated)
                pNotionalStepParameters.CalcAdjustableStepDate(m_Cs, terminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue, 
                    calculationPeriodDatesAdjustments, m_DataDocument);

            // 20071130 EG Ticker 15998
            if ((null == this[pNotionalStepParameters.Efs_FirstStepDate.AdjustedDate]) ||
                (null == this[pNotionalStepParameters.Efs_LastStepDate.AdjustedDate]))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Incorrect StepDate");

            Tools.CheckFrequency((IInterval)pNotionalStepParameters.StepFrequency, (IInterval)pCalculationPeriodFrequency, out int remainder);
            #endregion Validation Rules
            #region Process
            if (remainder == 0)
            {
                CurrencyCashInfo currencyCashInfo = null;
                // 20071130 EG Ticket 15998
                EFS_Period[] periods = Tools.ApplyInterval(pNotionalStepParameters.Efs_FirstStepDate.UnAdjustedDate,
                    pNotionalStepParameters.Efs_VirtualLastStepDate.UnAdjustedDate, pNotionalStepParameters.StepFrequency, pRollConvention);
                int j = 0;
                decimal MtPreviousAmount = 0;
                bool isEndStep = false;
                foreach (EFS_NominalPeriod nominalPeriod in nominalPeriods)
                {
                    if (isEndStep)
                    {
                        nominalPeriod.periodAmount.DecValue = MtPreviousAmount;
                    }
                    else
                    {
                        if (periods[j].date1 == nominalPeriod.periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue)
                        {
                            if (MtPreviousAmount != 0)
                                nominalPeriod.periodAmount.DecValue = MtPreviousAmount;
                            //
                            #region notionalStepAmount
                            if (pNotionalStepParameters.NotionalStepAmountSpecified)
                            {
                                nominalPeriod.periodAmount.DecValue += pNotionalStepParameters.NotionalStepAmount;
                            }
                            #endregion notionalStepAmount
                            #region notionalStepRate
                            else if (pNotionalStepParameters.NotionalStepRateSpecified)
                            {
                                if (pNotionalStepParameters.StepRelativeToSpecified)
                                {
                                    if (StepRelativeToEnum.Initial == pNotionalStepParameters.StepRelativeTo)
                                    {
                                        //-----------------------
                                        // StepRelativeTo INITIAL
                                        //-----------------------
                                        nominalPeriod.periodAmount.DecValue += pNotionalStepSchedule.InitialValue.DecValue * pNotionalStepParameters.NotionalStepRate;
                                    }
                                    else if (StepRelativeToEnum.Previous == pNotionalStepParameters.StepRelativeTo)
                                    {
                                        //------------------------
                                        // StepRelativeTo PREVIOUS
                                        //------------------------
                                        nominalPeriod.periodAmount.DecValue += nominalPeriod.periodAmount.DecValue * pNotionalStepParameters.NotionalStepRate;
                                    }
                                    //PL 20100217 Add Rounded (Use currencyCashInfo for tuning)
                                    if ((currencyCashInfo == null) && (StrFunc.IsFilled(pNotionalStepSchedule.Currency.Value)))
                                        currencyCashInfo = new CurrencyCashInfo(m_Cs, pNotionalStepSchedule.Currency.Value);
                                    if (currencyCashInfo != null)
                                    {
                                        EFS_Cash cash = new EFS_Cash(m_Cs, nominalPeriod.periodAmount.DecValue, currencyCashInfo);
                                        nominalPeriod.periodAmount.DecValue = cash.AmountRounded;
                                    }
                                }
                            }
                            #endregion notionalStepRate
                            //
                            MtPreviousAmount = nominalPeriod.periodAmount.DecValue;
                            j++;
                            isEndStep = (periods.Length < j + 1);
                        }
                        else if (0 != MtPreviousAmount)
                        {
                            nominalPeriod.periodAmount.DecValue = MtPreviousAmount;
                        }
                    }
                }
                ret = Cst.ErrLevel.SUCCESS;
            }
            #endregion Process

            return ret;
        }
        #endregion CalcNotionalStepParameters
        #region CalcNotionalStepSchedule
        /// <summary>
        /// Calculate and store the calculation periods notional amounts such as resulting from 
        /// the notionalStepSchedule element.
        /// </summary>
        /// <param name="pNotionalStepSchedule">Stream NotionalStepSchedule element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        /// <remarks>The calculation periods in the calcPeriods parameter are supposed to be ordered by
        /// increasing period start dates.
        /// </remarks>
        /// <revision>
        ///     <version>1.2.0</version><date>20070924</date><author>EG</author>
        ///     <EurosysSupport>N° 15764</EurosysSupport>
        ///     <comment>
        ///     Prise en compte des dates ajustées dans les notionalStep Schedule saisis
        ///		</comment>
        /// </revision>
        /// <revision>
        ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
        ///     <comment>Ticket 15889
        ///     Step dates: Unajusted versus Ajusted
        ///     </comment>
        /// </revision>
        /// 
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcNotionalStepSchedule(IAmountSchedule pNotionalStepSchedule)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region Validation Rules
            if (pNotionalStepSchedule.StepSpecified)
            {
                // 20071029 EG Ticker 15889
                if (false == pNotionalStepSchedule.IsStepCalculated)
                    pNotionalStepSchedule.CalcAdjustableSteps(m_Cs, calculationPeriodDatesAdjustments, m_DataDocument);

                foreach (EFS_Step step in pNotionalStepSchedule.Efs_Steps)
                {
                    if (null == this[step.AdjustedDate])
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Incorrect StepDate : " + step.UnAdjustedDate);
                }
            }
            #endregion Validation Rules
            #region Process
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                int i = 0;
                int j = 0;
                decimal mtStep = pNotionalStepSchedule.InitialValue.DecValue;
                decimal mtNextStep = mtStep;
                // 20071029 EG Ticker 15889
                DateTime dtNextStep = terminationDateAdjustment.AdjustedEventDate.DateValue;


                if (pNotionalStepSchedule.StepSpecified)
                {
                    // 20071029 EG Ticker 15889
                    dtNextStep = ((EFS_Step)pNotionalStepSchedule.Efs_Steps[0]).AdjustedDate;
                    mtNextStep = ((EFS_Step)pNotionalStepSchedule.Efs_Steps[0]).StepValue;
                }

                foreach (EFS_NominalPeriod nominalPeriod in nominalPeriods)
                {
                    // 20070924 EG Ticket 15764
                    // 20071029 EG Ticker 15889
                    bool isDtNextStepOk = dtNextStep <= nominalPeriod.periodDates.adjustableDate1.AdjustedEventDate.DateValue;
                    if (isDtNextStepOk)
                    {
                        mtStep = mtNextStep;
                        i++;
                        if (pNotionalStepSchedule.StepSpecified && (i < pNotionalStepSchedule.Efs_Steps.Length))
                        {
                            dtNextStep = ((EFS_Step)pNotionalStepSchedule.Efs_Steps[i]).AdjustedDate;
                            mtNextStep = ((EFS_Step)pNotionalStepSchedule.Efs_Steps[i]).StepValue;
                        }
                        else
                            dtNextStep = terminationDateAdjustment.AdjustedEventDate.DateValue;
                    }

                    nominalPeriod.periodAmount = new EFS_Decimal(mtStep);
                    j++;
                }
                ret = Cst.ErrLevel.SUCCESS;
            }
            #endregion Process

            return ret;
        }
        #endregion CalcNotionalStepSchedule
        #region CreateFxLinkedNotionalPeriod
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel CreateFxLinkedNotionalPeriod(IFxLinkedNotionalSchedule pFxLinkedNotionalSchedule, IInterestRateStream pStream)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            EFS_FxLinkedNotionalDate[] fxLinkedDates = pFxLinkedNotionalSchedule.Efs_FxLinkedNotionalDates.fxLinkedNotionalDates;
            string currency = pFxLinkedNotionalSchedule.Currency;
            string currencyNotionalScheduleReference = pFxLinkedNotionalSchedule.Efs_FxLinkedNotionalDates.currencyNotionalScheduleReference;
            this.fxLinkedNominalPeriods = new EFS_FxLinkedNominalPeriod[fxLinkedDates.Length];
            #region PreSettlementInfo


            EFS_SettlementInfoEntity preSettlementInfo = null;
            if (null != pStream)
            {
                IBusinessDayAdjustments bda = pStream.PaymentDates.PaymentDatesAdjustments;
                preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, pStream.StreamCurrency, pStream.PayerPartyReference.HRef, pStream.ReceiverPartyReference.HRef,
                    m_DataDocument, bda.DefaultOffsetPreSettlement);
            }
            #endregion PreSettlementInfo

            int i = 0;
            EFS_AdjustableDate adjustablePaymentDate = null;
            foreach (EFS_FxLinkedNotionalDate fxLinkedDate in fxLinkedDates)
            {
                if (0 == i)
                {
                    bool isInitialValueSpecified = pFxLinkedNotionalSchedule.InitialValueSpecified;
                    decimal amount = 0;
                    if (isInitialValueSpecified)
                        amount = pFxLinkedNotionalSchedule.InitialValue.DecValue;
                    this.fxLinkedNominalPeriods[i] = new EFS_FxLinkedNominalPeriod(m_Cs, fxLinkedDate.periodDatesAdjustment.adjustableDate1,
                        fxLinkedDate.periodDatesAdjustment.adjustableDate2,
                        fxLinkedDate.periodDatesAdjustment.adjustableDate1, fxLinkedDate.fixingDateAdjustment, terminationDateAdjustment,
                        amount, currency, amount, currencyNotionalScheduleReference, pFxLinkedNotionalSchedule.FxSpotRateSource,
                        preSettlementInfo, m_DataDocument)
                    {
                        fixingDateAdjustmentSpecified = (false == isInitialValueSpecified),
                        fxFixingSpecified = (false == isInitialValueSpecified)
                    };

                }
                else
                {
                    this.fxLinkedNominalPeriods[i] = new EFS_FxLinkedNominalPeriod(m_Cs, fxLinkedDate.periodDatesAdjustment.adjustableDate1,
                        fxLinkedDate.periodDatesAdjustment.adjustableDate2,
                        adjustablePaymentDate, fxLinkedDate.fixingDateAdjustment, terminationDateAdjustment,
                        0, currency, 0, currencyNotionalScheduleReference, pFxLinkedNotionalSchedule.FxSpotRateSource, preSettlementInfo, m_DataDocument)
                    {
                        fixingDateAdjustmentSpecified = true,
                        fxFixingSpecified = true
                    };
                }
                adjustablePaymentDate = fxLinkedDate.paymentDateAdjustment;
                i++;
            }

            return ret;
        }
        #endregion CreateFxLinkedNotionalPeriod
        #region CreateKnownAmountPeriod
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CreateKnownAmountPeriod(IAmountSchedule pKnownAmountSchedule, IStubCalculationPeriodAmount pStubCalculationPeriodAmount)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            foreach (EFS_CalculationPeriod period in this.calculationPeriods)
            {
                if (period.stubSpecified && (RateTypeEnum.None == period.stubRateType))
                {
                    IMoney stubAmount;

                    #region StubAmount case
                    if (StubEnum.Initial == period.stub)
                    {
                        stubAmount = pStubCalculationPeriodAmount.InitialStub.StubTypeAmount;
                        period.knownAmountPeriod = new EFS_KnownAmountPeriod(stubAmount.Amount.DecValue, stubAmount.Currency);
                    }
                    else if (StubEnum.Final == period.stub)
                    {
                        stubAmount = pStubCalculationPeriodAmount.FinalStub.StubTypeAmount;
                        period.knownAmountPeriod = new EFS_KnownAmountPeriod(stubAmount.Amount.DecValue, stubAmount.Currency);
                    }
                    #endregion StubAmount case
                }
                else
                {
                    #region Regular period
                    EFS_AdjustablePeriod adjustablePeriod = period.periodDates;
                    DateTime startDate = adjustablePeriod.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue;
                    DateTime endDate = adjustablePeriod.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;
                    // 20071029 EG Ticker 15889
                    decimal amount = Tools.GetStepValue(m_Cs, (ISchedule)pKnownAmountSchedule, startDate, endDate, 
                        (IBusinessDayAdjustments)calculationPeriodDatesAdjustments, m_DataDocument);
                    period.knownAmountPeriod = new EFS_KnownAmountPeriod(amount, pKnownAmountSchedule.Currency.Value);
                    #endregion Regular period
                }
            }

            return ret;
        }
        #endregion CalcNotionalStepSchedule
        #region CreateNotionalPeriod
        private Cst.ErrLevel CreateNotionalPeriod()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            int i = 0;
            this.nominalPeriods = new EFS_NominalPeriod[this.calculationPeriods.Length];
            foreach (EFS_CalculationPeriod calculationPeriod in this.calculationPeriods)
            {
                // 20090512 EG Test firstPeriodStartDateAdjustmentSpecified
                if ((0 == i) && this.firstPeriodStartDateAdjustmentSpecified)
                    this.nominalPeriods[i] = new EFS_NominalPeriod(calculationPeriod.periodDates, this.effectiveDateAdjustment);
                else
                    this.nominalPeriods[i] = new EFS_NominalPeriod(calculationPeriod.periodDates);
                i++;
            }

            return ret;
        }
        #endregion CreateNotionalPeriod
        #region Insert in calculationPeriod
        /// <revision>
        ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
        ///     <comment>Ticket 15859
        ///     Add new Parameter pIntervalFrequency
        ///     Used by EFS_DayCountFraction with an EFS_CalculationPeriod item
        ///     </comment>
        /// </revision>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void InsertArrayCalculationPeriod(ref ArrayList pCalculationPeriods, EFS_AdjustableDate pAdjustableDate1,
            EFS_AdjustableDate pAdjustableDate2, StubEnum pStub, RateTypeEnum pStubRateType, IInterval pIntervalPeriodFrequency)
        {

            EFS_CalculationPeriod calculationPeriod = new EFS_CalculationPeriod(m_Cs, pIntervalPeriodFrequency, m_DataDocument);
            calculationPeriod.periodDates.adjustableDate1 = (EFS_AdjustableDate)pAdjustableDate1.Clone();
            calculationPeriod.periodDates.adjustableDate2 = (EFS_AdjustableDate)pAdjustableDate2.Clone();
            calculationPeriod.stubSpecified = (pStub != StubEnum.None);
            calculationPeriod.stub = pStub;
            calculationPeriod.stubRateType = pStubRateType;
            calculationPeriod.intervalFrequency = pIntervalPeriodFrequency;
            // 20071029 EG Ticker 15889
            calculationPeriod.calculationPeriodDatesAdjustments = calculationPeriodDatesAdjustments;
            pCalculationPeriods.Add(calculationPeriod);

        }
        #endregion Insert in calculationPeriod
        #region SetCalculationPeriod
        /// <summary>
        ///  Création d'une période
        /// </summary>
        // EG 20190823 [FIXEDINCOME] New (DebtSecurity perpetual)
        private EFS_CalculationPeriod SetCalculationPeriod(EFS_AdjustableDate pAdjustableDate1, EFS_AdjustableDate pAdjustableDate2, 
            StubEnum pStub, RateTypeEnum pStubRateType, IInterval pIntervalPeriodFrequency)
        {
            EFS_CalculationPeriod calculationPeriod = new EFS_CalculationPeriod(m_Cs, pIntervalPeriodFrequency, m_DataDocument);
            calculationPeriod.periodDates.adjustableDate1 = (EFS_AdjustableDate)pAdjustableDate1.Clone();
            calculationPeriod.periodDates.adjustableDate2 = (EFS_AdjustableDate)pAdjustableDate2.Clone();
            calculationPeriod.stubSpecified = (pStub != StubEnum.None);
            calculationPeriod.stub = pStub;
            calculationPeriod.stubRateType = pStubRateType;
            calculationPeriod.intervalFrequency = pIntervalPeriodFrequency;
            calculationPeriod.calculationPeriodDatesAdjustments = calculationPeriodDatesAdjustments;
            return calculationPeriod;
        }
        #endregion SetCalculationPeriod
        #region Insert in nominalPeriod
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void InsertArrayNominalPeriod(ref ArrayList pNominalPeriods,
            EFS_AdjustableDate pStartPeriod, EFS_AdjustableDate pEndPeriod, EFS_AdjustableDate pFirstPeriodStartDate, decimal pAmount, string pCurrency, decimal pVariationAmount)
        {
            EFS_NominalPeriod nominalPeriod = new EFS_NominalPeriod(pStartPeriod, pEndPeriod, pAmount, pCurrency, pVariationAmount)
            {
                firstPeriodStartDateSpecified = (null != pFirstPeriodStartDate) && (0 == pNominalPeriods.Count)
            };
            if (nominalPeriod.firstPeriodStartDateSpecified)
                nominalPeriod.firstPeriodStartDate = pFirstPeriodStartDate;
            // 20070823 EG Ticket : 15643
            #region PreSettlementDate
            nominalPeriod.preSettlementSpecified = m_PreSettlementInfo.IsUsePreSettlement;
            if (nominalPeriod.preSettlementSpecified)
            {
                nominalPeriod.preSettlementStartPeriod = new EFS_PreSettlement(m_Cs, null, pStartPeriod.AdjustedEventDate, nominalPeriod.currency, m_PreSettlementInfo.OffsetPreSettlement, m_DataDocument);
                nominalPeriod.preSettlementEndPeriod = new EFS_PreSettlement(m_Cs, null, pEndPeriod.AdjustedEventDate, nominalPeriod.currency, m_PreSettlementInfo.OffsetPreSettlement, m_DataDocument);
            }
            #endregion PreSettlementDate
            pNominalPeriods.Add(nominalPeriod);

        }
        #endregion Insert in nominalPeriod
        #region SetMultiplierAndSpreadValue
        // 20090828 EG ticket : 16656
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel SetMultiplierAndSpreadValue(ICalculation pCalculation, IStubCalculationPeriodAmount pStubCalculationPeriodAmount)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            IFloatingRate floatingRate = null;
            if (pCalculation.RateFloatingRateSpecified)
                floatingRate = pCalculation.RateFloatingRate;
            else if (pCalculation.RateInflationRateSpecified)
                floatingRate = pCalculation.RateInflationRate;

            // 20090828 EG ticket : 16656 [START]
            bool isInitialStubSpecified = (null != pStubCalculationPeriodAmount) && (pStubCalculationPeriodAmount.InitialStubSpecified);
            if (isInitialStubSpecified)
                isInitialStubSpecified = (pStubCalculationPeriodAmount.InitialStub.StubTypeFixedRateSpecified || pStubCalculationPeriodAmount.InitialStub.StubTypeFloatingRateSpecified);
            bool isFinalStubSpecified = (null != pStubCalculationPeriodAmount) && (pStubCalculationPeriodAmount.FinalStubSpecified);
            if (isFinalStubSpecified)
                isFinalStubSpecified = (pStubCalculationPeriodAmount.FinalStub.StubTypeFixedRateSpecified || pStubCalculationPeriodAmount.FinalStub.StubTypeFloatingRateSpecified);
            for (int i = 0; i < this.calculationPeriods.Length; i++)
            {
                EFS_CalculationPeriod period = this.calculationPeriods[i];
                DateTime startDate = period.periodDates.adjustableDate1.adjustedDate.DateValue;
                DateTime endDate = period.periodDates.adjustableDate2.adjustedDate.DateValue;
                ISchedule spreadSchedule = null;
                ISchedule multiplierSchedule = null;

                IFloatingRate stubFloatingRate;
                if ((0 == i) && isInitialStubSpecified)
                {
                    #region InitialStub
                    if (pStubCalculationPeriodAmount.InitialStub.StubTypeFloatingRateSpecified)
                    {
                        stubFloatingRate = pStubCalculationPeriodAmount.InitialStub.StubTypeFloatingRate[0];
                        period.spreadSpecified = stubFloatingRate.SpreadScheduleSpecified;
                        if (period.spreadSpecified)
                            spreadSchedule = stubFloatingRate.SpreadSchedule;
                        period.multiplierSpecified = stubFloatingRate.FloatingRateMultiplierScheduleSpecified;
                        if (period.multiplierSpecified)
                            multiplierSchedule = stubFloatingRate.FloatingRateMultiplierSchedule;
                    }
                    else
                        continue;
                    #endregion InitialStub
                }
                else if (((this.calculationPeriods.Length - 1) == i) && isFinalStubSpecified)
                {
                    #region FinalStub
                    if (pStubCalculationPeriodAmount.FinalStub.StubTypeFloatingRateSpecified)
                    {
                        stubFloatingRate = pStubCalculationPeriodAmount.FinalStub.StubTypeFloatingRate[0];
                        period.spreadSpecified = stubFloatingRate.SpreadScheduleSpecified;
                        if (period.spreadSpecified)
                            spreadSchedule = stubFloatingRate.SpreadSchedule;
                        period.multiplierSpecified = stubFloatingRate.FloatingRateMultiplierScheduleSpecified;
                        if (period.multiplierSpecified)
                            multiplierSchedule = stubFloatingRate.FloatingRateMultiplierSchedule;
                    }
                    else
                        continue;
                    #endregion FinalStub
                }
                else
                {
                    #region Regular
                    period.spreadSpecified = floatingRate.SpreadScheduleSpecified;
                    if (period.spreadSpecified)
                        spreadSchedule = floatingRate.SpreadSchedule;

                    period.multiplierSpecified = floatingRate.FloatingRateMultiplierScheduleSpecified;
                    if (period.multiplierSpecified)
                        multiplierSchedule = floatingRate.FloatingRateMultiplierSchedule;
                    #endregion Regular
                }

                if (period.spreadSpecified)
                    period.spread = new EFS_Decimal(Tools.GetStepValue(m_Cs, spreadSchedule, startDate, endDate, 
                        period.calculationPeriodDatesAdjustments, m_DataDocument));
                if (period.multiplierSpecified)
                    period.multiplier = new EFS_Decimal(Tools.GetStepValue(m_Cs, multiplierSchedule, startDate, endDate, 
                        period.calculationPeriodDatesAdjustments, m_DataDocument));
            }
            return ret;
        }
        #endregion SetMultiplierAndSpreadValue
        #region Set
        /// <summary>
        /// Initialisation des membres pour calcul
        /// </summary>
        // EG 20190823 [FIXEDINCOME] New (DebtSecurity perpetual)
        protected void Set(ICalculationPeriodDates pCalculationPeriodDates)
        {
            // EffectiveDateAdjustment - TerminationDateAdjustment
            effectiveDateAdjustment = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.EffectiveDateAdjustable, m_DataDocument);
            terminationDateAdjustment = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.TerminationDateAdjustable, m_DataDocument);

            // FirstPeriodStartDateAdjustment
            firstPeriodStartDateAdjustmentSpecified = pCalculationPeriodDates.FirstPeriodStartDateSpecified;
            if (firstPeriodStartDateAdjustmentSpecified)
                firstPeriodStartDateAdjustment = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.FirstPeriodStartDate, m_DataDocument);

            // FirstRegularPeriodStartDateAdjustment / LastRegularPeriodEndDateAdjustment / RegularPeriodDatesAdjustment
            firstRegularPeriodStartDateAdjustmentSpecified = pCalculationPeriodDates.FirstRegularPeriodStartDateSpecified;
            regularPeriodDatesAdjustment = new EFS_AdjustablePeriod();
            if (firstRegularPeriodStartDateAdjustmentSpecified)
            {
                firstRegularPeriodStartDateAdjustment = new EFS_AdjustableDate(m_Cs,
                    pCalculationPeriodDates.FirstRegularPeriodStartDate.DateValue, pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);
                regularPeriodDatesAdjustment.adjustableDate1 = (EFS_AdjustableDate)this.firstRegularPeriodStartDateAdjustment.Clone();
            }
            else
                regularPeriodDatesAdjustment.adjustableDate1 = (EFS_AdjustableDate)this.effectiveDateAdjustment.Clone();

            lastRegularPeriodEndDateAdjustmentSpecified = pCalculationPeriodDates.LastRegularPeriodEndDateSpecified;
            if (lastRegularPeriodEndDateAdjustmentSpecified)
            {
                lastRegularPeriodEndDateAdjustment = new EFS_AdjustableDate(m_Cs,
                    pCalculationPeriodDates.LastRegularPeriodEndDate.DateValue, pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);
                regularPeriodDatesAdjustment.adjustableDate2 = (EFS_AdjustableDate)this.lastRegularPeriodEndDateAdjustment.Clone();
            }
            else
                regularPeriodDatesAdjustment.adjustableDate2 = (EFS_AdjustableDate)this.terminationDateAdjustment.Clone();
        }
        #endregion Set


        /// <summary>
        /// Calcul et Insertion de la 1ère période
        /// </summary>
        protected Pair<bool, EFS_CalculationPeriod> CalcFirstPeriod(ICalculationPeriodDates pCalculationPeriodDates, Nullable<DateTime> pPreviousCouponDate)
        {
            StubEnum stub = StubEnum.None;
            RateTypeEnum stubRateType = RateTypeEnum.None;

            EFS_AdjustableDate adjustableDate1 = new EFS_AdjustableDate(m_Cs, m_DataDocument);
            _ = new EFS_AdjustableDate(m_Cs, m_DataDocument);

            ICalculationPeriodFrequency frequency = pCalculationPeriodDates.CalculationPeriodFrequency;

            if (pCalculationPeriodDates.FirstPeriodStartDateSpecified)
                adjustableDate1.AdjustableDate = (IAdjustableDate)pCalculationPeriodDates.FirstPeriodStartDate.Clone();
            else
                adjustableDate1.AdjustableDate = (IAdjustableDate)pCalculationPeriodDates.EffectiveDateAdjustable.Clone();
            EFS_AdjustableDate adjustableDate2;
            if (pCalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
            {
                stub = StubEnum.Initial;
                #region StubRateType
                if (this.initialStubAmountSpecified)
                    stubRateType = RateTypeEnum.None;
                else if (this.initialStubFixedRateSpecified)
                    stubRateType = RateTypeEnum.FixedRate;
                else if (this.initialStubFloatingRateSpecified)
                    stubRateType = RateTypeEnum.FloatingRate;
                #endregion StubRateType

                adjustableDate2 = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.FirstRegularPeriodStartDate.DateValue,
                    pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);
            }
            else
            {
                adjustableDate2 = Tools.ApplyInterval(adjustableDate1, pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue, frequency.Interval);
                adjustableDate2.AdjustableDate.DateAdjustments = (IBusinessDayAdjustments)pCalculationPeriodDates.CalculationPeriodDatesAdjustments.Clone();
                adjustableDate2 = Tools.ApplyRollConvention(adjustableDate2, pCalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue, frequency.RollConvention);
            }

            Pair<bool, EFS_CalculationPeriod> calculationPeriod = new Pair<bool, EFS_CalculationPeriod>
            {
                First = (false == pPreviousCouponDate.HasValue) || (pPreviousCouponDate.Value <= adjustableDate1.adjustedDate.DateValue),
                Second = SetCalculationPeriod(adjustableDate1, adjustableDate2, stub, stubRateType, frequency.Interval)
            };
            return calculationPeriod;
        }
        protected List<Pair<bool,EFS_CalculationPeriod>> CalcRegularPeriod(ICalculationPeriodDates pCalculationPeriodDates,
            EFS_CalculationPeriod pPreviousCalculationPeriod, Nullable<DateTime> pPreviousCouponDate)
        {
            List<Pair<bool,EFS_CalculationPeriod>> lstCalculationPeriods = new List<Pair<bool,EFS_CalculationPeriod>>();

            ICalculationPeriodFrequency frequency = pCalculationPeriodDates.CalculationPeriodFrequency;
            _ = pPreviousCalculationPeriod.periodDates.adjustableDate1;
            EFS_AdjustableDate adjustableDate2 = pPreviousCalculationPeriod.periodDates.adjustableDate2;

            bool isPerpetual = (pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue.Date == DateTime.MaxValue.Date);

            if (adjustableDate2.adjustableDateSpecified)
            {
                int guard = 0;
                StubEnum stub = StubEnum.None;
                RateTypeEnum stubRateType = RateTypeEnum.None;
                while (true)
                {
                    guard++;
                    if (guard == 999)
                    {
                        //Loop parapet
                        string msgException = "Calculation Period Dates exception:" + Cst.CrLf;
                        msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                        msgException += "Please, verify dates, periods, business day adjustment and roll convention on the trade";
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                    }
                    EFS_AdjustableDate adjustableDate1 = adjustableDate2.Clone() as EFS_AdjustableDate;
                    adjustableDate2 = Tools.ApplyInterval(adjustableDate1, pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue, frequency.Interval);
                    adjustableDate2 = Tools.ApplyRollConvention(adjustableDate2,pCalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue, frequency.RollConvention);

                    if (pCalculationPeriodDates.LastRegularPeriodEndDateSpecified &&
                        adjustableDate2.AdjustableDate.UnadjustedDate.DateValue >= pCalculationPeriodDates.LastRegularPeriodEndDate.DateValue)
                    {
                        adjustableDate2 = new EFS_AdjustableDate(m_Cs, pCalculationPeriodDates.LastRegularPeriodEndDate.DateValue,
                            pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);

                        // Insert Last Regular Intermediary Period
                        Pair<bool, EFS_CalculationPeriod> calculationPeriod = new Pair<bool, EFS_CalculationPeriod>
                        {
                            First = true,
                            Second = SetCalculationPeriod(adjustableDate1, adjustableDate2, stub, stubRateType, frequency.Interval)
                        };
                        lstCalculationPeriods.Add(calculationPeriod);
                        break;
                    }
                    else if (adjustableDate2.AdjustableDate.UnadjustedDate.DateValue < pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue)
                    {
                        Pair<bool, EFS_CalculationPeriod> calculationPeriod = new Pair<bool, EFS_CalculationPeriod>
                        {
                            First = (false == pPreviousCouponDate.HasValue) || (pPreviousCouponDate.Value <= adjustableDate1.adjustedDate.DateValue),
                            Second = SetCalculationPeriod(adjustableDate1, adjustableDate2, stub, stubRateType, frequency.Interval)
                        };
                        lstCalculationPeriods.Add(calculationPeriod);
                        if ((false == isPerpetual) || (false == calculationPeriod.First))
                            continue;
                    }
                    break;
                }
            }
            return lstCalculationPeriods;
        }
        /// <summary>
        /// Calcul de la dernière periode
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Refactoring (cas du Perpetual DebtSecurity) 
        // EG 20191011 [FIXEDINCOME] Upd (adjustableDate1 et adjustableDate2 initialisés avec date2 de la dernière periode calculée (clonage) - Correction déjà effectuée sur Caracal 20190926)
        protected Pair<bool, EFS_CalculationPeriod> CalcLastPeriod(ICalculationPeriodDates pCalculationPeriodDates, EFS_CalculationPeriod pPreviousCalculationPeriod)
        {
            Pair<bool, EFS_CalculationPeriod> calculationPeriod = new Pair<bool, EFS_CalculationPeriod>(false, null);
            ICalculationPeriodFrequency frequency = pCalculationPeriodDates.CalculationPeriodFrequency;
            EFS_AdjustableDate adjustableDate1 = pPreviousCalculationPeriod.periodDates.adjustableDate2.Clone() as EFS_AdjustableDate;
            EFS_AdjustableDate adjustableDate2 = pPreviousCalculationPeriod.periodDates.adjustableDate2.Clone() as EFS_AdjustableDate; 

            StubEnum stub = StubEnum.None;
            RateTypeEnum stubRateType = RateTypeEnum.None;


            if (pCalculationPeriodDates.LastRegularPeriodEndDateSpecified || (adjustableDate1.adjustedDate.DateValue < this.terminationDateAdjustment.adjustedDate.DateValue))
            {
                calculationPeriod.First = true;
                adjustableDate2.AdjustableDate = pCalculationPeriodDates.TerminationDateAdjustable.Clone() as IAdjustableDate;
                calculationPeriod.Second = SetCalculationPeriod(adjustableDate1, adjustableDate2, stub, stubRateType, frequency.Interval);
            }
            return calculationPeriod;
        }
        public Cst.ErrLevel Calc2(CalPeriodEnum pCalcPeriodEnum, ICalculationPeriodDates pCalculationPeriodDates)
        {
            return Calc2(pCalcPeriodEnum, pCalculationPeriodDates, null, null);
        }
        public Cst.ErrLevel Calc2(CalPeriodEnum pCalcPeriodEnum, ICalculationPeriodDates pCalculationPeriodDates,
            EFS_CalculationPeriod pPreviousCalculationPeriod, Nullable<DateTime> pPreviousCouponDate)
        {
            List<Pair<bool, EFS_CalculationPeriod>> lstCalculationPeriods = new List<Pair<bool, EFS_CalculationPeriod>>();
            Set(pCalculationPeriodDates);

            switch (pCalcPeriodEnum)
            {
                case CalPeriodEnum.First:
                    lstCalculationPeriods.Add(CalcFirstPeriod(pCalculationPeriodDates, pPreviousCouponDate));
                    break;
                case CalPeriodEnum.Regular:
                    lstCalculationPeriods.AddRange(CalcRegularPeriod(pCalculationPeriodDates, pPreviousCalculationPeriod, pPreviousCouponDate));
                    break;
                case CalPeriodEnum.Last:
                    lstCalculationPeriods.Add(CalcLastPeriod(pCalculationPeriodDates, pPreviousCalculationPeriod));
                    break;
                case CalPeriodEnum.FirstAndRegular:
                    lstCalculationPeriods.Add(CalcFirstPeriod(pCalculationPeriodDates, pPreviousCouponDate));
                    lstCalculationPeriods.AddRange(CalcRegularPeriod(pCalculationPeriodDates, lstCalculationPeriods[lstCalculationPeriods.Count - 1].Second, pPreviousCouponDate));
                    break;
                case CalPeriodEnum.All:
                    lstCalculationPeriods.Add(CalcFirstPeriod(pCalculationPeriodDates, pPreviousCouponDate));
                    lstCalculationPeriods.AddRange(CalcRegularPeriod(pCalculationPeriodDates, lstCalculationPeriods[lstCalculationPeriods.Count - 1].Second, pPreviousCouponDate));
                    lstCalculationPeriods.Add(CalcLastPeriod(pCalculationPeriodDates, lstCalculationPeriods[lstCalculationPeriods.Count - 1].Second));
                    break;
            }

            calculationPeriods = (from item in lstCalculationPeriods.Where(item => (item != null) && item.First) select item.Second).ToArray();
            return Cst.ErrLevel.SUCCESS;
        }
        public Cst.ErrLevel OldCalc2(CalPeriodEnum pCalcPeriodEnum, ICalculationPeriodDates pCalculationPeriodDates,
            EFS_CalculationPeriod pPreviousCalculationPeriod, Nullable<DateTime> pPreviousCouponDate)
        {
            List<Pair<bool, EFS_CalculationPeriod>> lstCalculationPeriods = new List<Pair<bool, EFS_CalculationPeriod>>();
            Set(pCalculationPeriodDates);

            switch (pCalcPeriodEnum)
            {
                case CalPeriodEnum.First:
                    lstCalculationPeriods.Add(CalcFirstPeriod(pCalculationPeriodDates, pPreviousCouponDate));
                    break;
                case CalPeriodEnum.Regular:
                    lstCalculationPeriods.AddRange(CalcRegularPeriod(pCalculationPeriodDates, pPreviousCalculationPeriod, pPreviousCouponDate));
                    break;
                case CalPeriodEnum.Last:
                    lstCalculationPeriods.Add(CalcLastPeriod(pCalculationPeriodDates, pPreviousCalculationPeriod));
                    break;
                case CalPeriodEnum.FirstAndRegular:
                    lstCalculationPeriods.Add(CalcFirstPeriod(pCalculationPeriodDates, pPreviousCouponDate));
                    lstCalculationPeriods.AddRange(CalcRegularPeriod(pCalculationPeriodDates, lstCalculationPeriods[lstCalculationPeriods.Count - 1].Second, pPreviousCouponDate));
                    break;
                case CalPeriodEnum.All:
                    lstCalculationPeriods.Add(CalcFirstPeriod(pCalculationPeriodDates, pPreviousCouponDate));
                    lstCalculationPeriods.AddRange(CalcRegularPeriod(pCalculationPeriodDates, lstCalculationPeriods[lstCalculationPeriods.Count - 1].Second, pPreviousCouponDate));
                    lstCalculationPeriods.Add(CalcLastPeriod(pCalculationPeriodDates, lstCalculationPeriods[lstCalculationPeriods.Count - 1].Second));
                    break;
            }
            //lstCalculationPeriods.RemoveAll(item => item == null);
            //if (0 < lstCalculationPeriods.Count)
            //    calculationPeriods = lstCalculationPeriods.ToArray();
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Methods
    }
    #endregion EFS_CalculationPeriodDates
    #region EFS_CapFloored
    public class EFS_CapFloored
    {
        #region Members
        public EFS_AdjustablePeriod periodDates;
        public string eventType;
        public string payer;
        public string receiver;
        public ISchedule strikeSchedule;
        public EFS_Decimal strike;
        #endregion Members

        #region Constructors
        public EFS_CapFloored(string pEventType, string pPayer, string pReceiver, ISchedule pStrikeSchedule)
        {
            eventType = pEventType;
            payer = pPayer;
            receiver = pReceiver;
            strikeSchedule = pStrikeSchedule;
        }
        public EFS_CapFloored(string pEventType, string pPayer, string pReceiver)
        {
            eventType = pEventType;
            payer = pPayer;
            receiver = pReceiver;
        }
        #endregion Constructors
        #region Accessors
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date adjustedStartPeriod = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.adjustedDate.DateValue
                };
                return adjustedStartPeriod;

            }
        }
        #endregion AdjustedStartPeriod
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date adjustedEndPeriod = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.adjustedDate.DateValue
                };
                return adjustedEndPeriod;

            }
        }
        #endregion AdjustedEndPeriod
        #region CapFlooredPayer
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CapFlooredPayer
        {
            get
            {
                return payer;

            }
        }
        #endregion CapFlooredPayer
        #region CapFlooredReceiver
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CapFlooredReceiver
        {
            get
            {
                return receiver;

            }
        }
        #endregion CapFlooredReceiver
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate2);

            }
        }
        #endregion EndPeriod
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                return eventType;

            }
        }
        #endregion EventType
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate1);

            }
        }
        #endregion StartPeriod
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Strike
        {
            get
            {
                return strike;

            }
        }
        #endregion StartPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedEndPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedStartPeriod
        #endregion Accessors

        #region Methods
        #region SetPeriodDatesAndStrikeValue
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void SetPeriodDatesAndStrikeValue(string pConnectionString, EFS_CalculationPeriod pCalculationPeriod, DataDocumentContainer pDataDocument)
        {
            periodDates = (EFS_AdjustablePeriod)pCalculationPeriod.periodDates.Clone();
            strike = new EFS_Decimal(Tools.GetStepValue(pConnectionString, strikeSchedule,
                periodDates.adjustableDate1.adjustedDate.DateValue,
                periodDates.adjustableDate2.adjustedDate.DateValue,
                pCalculationPeriod.calculationPeriodDatesAdjustments, pDataDocument));

        }
        #endregion SetPeriodDatesAndStrikeValue
        #endregion Methods
    }
    #endregion EFS_CapFloored
    #region EFS_CapFlooreds
    public class EFS_CapFlooreds
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        private readonly ArrayList m_aCapFloored = new ArrayList();
        private readonly string m_Payer;
        private readonly string m_Receiver;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CapFlooreds(string pConnectionString, IReference pPayerPartyReference, IReference pReceiverPartyReference,
            IFloatingRate pFloatingRate, ICalculationPeriodDates pCalcPeriod, DataDocumentContainer pDataDocument)
        {
            EFS_CalculationPeriodDates calculationPeriodDates = pCalcPeriod.Efs_CalculationPeriodDates;
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            m_Payer = pPayerPartyReference.HRef;
            m_Receiver = pReceiverPartyReference.HRef;
            #region Cap
            if (pFloatingRate.CapRateScheduleSpecified)
                ReadCapFloored(pFloatingRate.CapRateSchedule, "CA");
            #endregion Cap
            #region Floor
            if (pFloatingRate.FloorRateScheduleSpecified)
                ReadCapFloored(pFloatingRate.FloorRateSchedule, "FL");
            #endregion Floor

            if (m_aCapFloored.Count > 0)
            {
                foreach (EFS_CalculationPeriod calculationPeriod in calculationPeriodDates.calculationPeriods)
                {
                    if ((StubEnum.None != calculationPeriod.stub) && (RateTypeEnum.FloatingRate != calculationPeriod.stubRateType))
                        continue;
                    calculationPeriod.capFlooreds = new EFS_CapFloored[m_aCapFloored.Count];
                    for (int i = 0; i < m_aCapFloored.Count; i++)
                    {
                        EFS_CapFloored tmpCapFloored = (EFS_CapFloored)m_aCapFloored[i];
                        EFS_CapFloored capFloored = new EFS_CapFloored(tmpCapFloored.eventType, tmpCapFloored.payer, tmpCapFloored.receiver, tmpCapFloored.strikeSchedule)
                        {
                            periodDates = (EFS_AdjustablePeriod)calculationPeriod.periodDates.Clone()
                        };
                        capFloored.SetPeriodDatesAndStrikeValue(m_Cs, calculationPeriod, m_DataDocument);
                        calculationPeriod.capFlooreds.SetValue(capFloored, i);

                    }
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region AddCapFloored
        public void AddCapFloored(string pEventType, string pPayer, string pReceiver, ISchedule pSchedule)
        {
            bool isOk = true;
            for (int i = 0; i < m_aCapFloored.Count; i++)
            {
                EFS_CapFloored capFloored = (EFS_CapFloored)m_aCapFloored[i];
                if (capFloored.eventType.ToUpper() == pEventType.ToUpper())
                {
                    isOk = false;
                    break;
                }
            }
            if (isOk)
                m_aCapFloored.Add(new EFS_CapFloored(pEventType, pPayer, pReceiver, pSchedule));
        }
        #endregion AddCapFloored
        #region ReadCapFloored
        private void ReadCapFloored(IStrikeSchedule[] pStrikeSchedule, string pEventType)
        {

            foreach (IStrikeSchedule strikeSchedule in pStrikeSchedule)
            {
                if (strikeSchedule.BuyerSpecified)
                {
                    if (PayerReceiverEnum.Payer == strikeSchedule.Buyer)
                        AddCapFloored(pEventType + "B", m_Receiver, m_Payer, strikeSchedule);
                    else
                        AddCapFloored(pEventType + "S", m_Payer, m_Receiver, strikeSchedule);
                }
                else if (strikeSchedule.SellerSpecified)
                {
                    if (PayerReceiverEnum.Payer == strikeSchedule.Seller)
                        AddCapFloored(pEventType + "S", m_Payer, m_Receiver, strikeSchedule);
                    else
                        AddCapFloored(pEventType + "B", m_Receiver, m_Payer, strikeSchedule);
                }
                if ((2 == m_aCapFloored.Count) || (4 == m_aCapFloored.Count))
                    break;
            }
        }
        #endregion ReadCapFloored
        #endregion Methods
    }
    #endregion EFS_CapFlooreds

    #region EFS_ExerciseDates
    public class EFS_ExerciseDates
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private readonly string m_Cs;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private readonly DataDocumentContainer m_DataDocument;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool americanDateAdjustementSpecified;
        public EFS_AdjustablePeriod americanDateAdjustement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bermudaDateAdjustementSpecified;
        public EFS_AdjustableDates bermudaDateAdjustement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool europeanDateAdjustementSpecified;
        public EFS_AdjustableDate europeanDateAdjustement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool relevantUnderlyingDateAdjustementSpecified;
        public EFS_AdjustableDates relevantUnderlyingDateAdjustement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashSettlementValuationDateAdjustementSpecified;
        public EFS_AdjustableDates cashSettlementValuationDateAdjustement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashSettlementPaymentDateAdjustementSpecified;
        public EFS_AdjustableDates cashSettlementPaymentDateAdjustement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool americanExerciseDatesEventsSpecified;
        public EFS_ExerciseDatesEvent[] americanExerciseDatesEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bermudaExerciseDatesEventsSpecified;
        public EFS_ExerciseDatesEvent[] bermudaExerciseDatesEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool europeanExerciseDatesEventsSpecified;
        public EFS_ExerciseDatesEvent[] europeanExerciseDatesEvents;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExerciseDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ExerciseDates(string pConnectionString, object pObjectDeclaringExercise, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            Calc(pObjectDeclaringExercise);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculation for an option trade all unadjusted and adjusted exercise dates
        /// </summary>
        /// <param name="pObjectDeclaringExercise">Parent of Exercise element
        /// (CancelableProvision, ExtendibleProvision, OptionalEarlyTermination, Swaption, StepUpProvision)
        /// </param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        public Cst.ErrLevel Calc(object pObjectDeclaringExercise)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.INITIALIZE_ERROR;

            #region Get pObjectDeclaringExercise.Exercise
            object exercise = null;
            Type tObjectDeclaringExercise = pObjectDeclaringExercise.GetType();
            PropertyInfo pty = tObjectDeclaringExercise.GetProperty("EFS_Exercise");
            if (null != pty)
                exercise = tObjectDeclaringExercise.InvokeMember(pty.Name, BindingFlags.GetProperty, null, pObjectDeclaringExercise, null);
            #endregion Get pObjectDeclaringExercise.Exercise

            #region Exercise determination
            if (null != exercise)
            {
                ExerciseStyleEnum exerciseStyle;

                //20080903 FI pObjectDeclaringExercise n'est pas uniquement un IProvision => il peut être un ISwaption
                if (Tools.IsTypeOrInterfaceOf(pObjectDeclaringExercise, InterfaceEnum.ISwaption))
                    exerciseStyle = ((ISwaption)pObjectDeclaringExercise).GetStyle;
                else if (Tools.IsTypeOrInterfaceOf(pObjectDeclaringExercise, InterfaceEnum.IProvision))
                    exerciseStyle = ((IProvision)pObjectDeclaringExercise).GetStyle;
                else
                    throw new NotImplementedException(tObjectDeclaringExercise.ToString());
                //
                americanDateAdjustementSpecified = (ExerciseStyleEnum.American == exerciseStyle);
                bermudaDateAdjustementSpecified = (ExerciseStyleEnum.Bermuda == exerciseStyle);
                europeanDateAdjustementSpecified = (ExerciseStyleEnum.European == exerciseStyle);
            }
            if (americanDateAdjustementSpecified)
                ret = CalcAmerican((IAmericanExercise)exercise);
            else if (bermudaDateAdjustementSpecified)
                ret = CalcBermuda((IBermudaExercise)exercise);
            else if (europeanDateAdjustementSpecified)
                ret = CalcEuropean((IEuropeanExercise)exercise);
            #endregion Exercise determination

            if (Cst.ErrLevel.SUCCESS == ret)
                ret = SetCashSettlementDates(pObjectDeclaringExercise);

            if (Cst.ErrLevel.SUCCESS == ret)
                ret = SetAdjustedDatesEvent();


            return ret;
        }
        #endregion Calc
        #region American
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcAmerican(IAmericanExercise pExercise)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;

            #region CommencementDate
            americanDateAdjustement = new EFS_AdjustablePeriod();
            if (pExercise.CommencementDate.AdjustableDateSpecified)
            {
                #region AdjustableDate
                americanDateAdjustement = new EFS_AdjustablePeriod
                {
                    adjustableDate1 = new EFS_AdjustableDate(m_Cs, pExercise.CommencementDate.AdjustableDate, m_DataDocument)
                };
                ret = Cst.ErrLevel.SUCCESS;
                #endregion AdjustableDate
            }
            else if (pExercise.CommencementDate.RelativeDateSpecified)
            {
                #region RelativeTo
                americanDateAdjustement = new EFS_AdjustablePeriod();
                ret = Tools.OffSetDateRelativeTo(m_Cs, pExercise.CommencementDate.RelativeDate, out DateTime[] offsetDate, m_DataDocument);
                if (ret == Cst.ErrLevel.SUCCESS)
                    americanDateAdjustement.adjustableDate1 = new EFS_AdjustableDate(m_Cs, offsetDate[0],
                        ((IRelativeDateOffset)pExercise.CommencementDate.RelativeDate).GetAdjustments, m_DataDocument);
                #endregion RelativeTo
            }
            #endregion CommencementDate
            #region ExpirationDate
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                if (pExercise.ExpirationDate.AdjustableDateSpecified)
                {
                    #region AdjustableDate
                    americanDateAdjustement.adjustableDate2 = new EFS_AdjustableDate(m_Cs, pExercise.ExpirationDate.AdjustableDate, m_DataDocument);
                    #endregion AdjustableDate
                }
                else if (pExercise.ExpirationDate.RelativeDateSpecified)
                {
                    #region RelativeTo
                    ret = Tools.OffSetDateRelativeTo(m_Cs, pExercise.ExpirationDate.RelativeDate, out DateTime[] offsetDate, m_DataDocument);
                    if (ret == Cst.ErrLevel.SUCCESS)
                        americanDateAdjustement.adjustableDate2 = new EFS_AdjustableDate(m_Cs, offsetDate[offsetDate.Length - 1],
                            ((IRelativeDateOffset)pExercise.ExpirationDate.RelativeDate).GetAdjustments, m_DataDocument);
                    #endregion RelativeTo
                }
            }
            #endregion ExpirationDate
            #region RelevantUnderlyingDates
            if (pExercise.RelevantUnderlyingDateSpecified)
                ret = SetRelevantUnderlyingDates(pExercise.RelevantUnderlyingDate);
            #endregion RelevantUnderlyingDates

            return ret;
        }
        #endregion American
        #region Bermuda
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcBermuda(IBermudaExercise pExercise)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;

            #region ExerciseDates
            bermudaDateAdjustement = new EFS_AdjustableDates();
            if (pExercise.BermudaExerciseDates.AdjustableDatesSpecified)
            {
                #region AdjustableDates
                IAdjustableDates adjustableDates = pExercise.BermudaExerciseDates.AdjustableDates;
                bermudaDateAdjustement.adjustableDates = new EFS_AdjustableDate[adjustableDates.UnadjustedDate.Length];
                for (int i = 0; i < adjustableDates.UnadjustedDate.Length; i++)
                {
                    bermudaDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, adjustableDates[i], 
                        adjustableDates.DateAdjustments, m_DataDocument);
                }
                #endregion AdjustableDates
                ret = Cst.ErrLevel.SUCCESS;
            }
            else if (pExercise.BermudaExerciseDates.RelativeDatesSpecified)
            {
                #region RelativeDates
                IRelativeDates relativeDates = pExercise.BermudaExerciseDates.RelativeDates;
                ret = Tools.OffSetDateRelativeTo(m_Cs, relativeDates, out DateTime[] offsetDates, m_DataDocument);
                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    bermudaDateAdjustement.adjustableDates = new EFS_AdjustableDate[offsetDates.Length];
                    for (int i = 0; i < offsetDates.Length; i++)
                    {
                        bermudaDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, offsetDates[i], 
                            relativeDates.GetAdjustments, m_DataDocument);
                    }
                }
                #endregion RelativeDates
            }
            #endregion ExerciseDates
            #region RelevantUnderlyingDates
            if (pExercise.RelevantUnderlyingDateSpecified)
                ret = SetRelevantUnderlyingDates(pExercise.RelevantUnderlyingDate);
            #endregion RelevantUnderlyingDates

            return ret;
        }
        #endregion Bermuda
        #region European
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel CalcEuropean(IEuropeanExercise pExercise)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;

            #region ExpirationDate
            if (pExercise.ExpirationDate.AdjustableDateSpecified)
            {
                #region AdjustableDate
                europeanDateAdjustement = new EFS_AdjustableDate(m_Cs, pExercise.ExpirationDate.AdjustableDate, m_DataDocument);
                ret = Cst.ErrLevel.SUCCESS;
                #endregion AdjustableDate
            }
            else if (pExercise.ExpirationDate.RelativeDateSpecified)
            {
                #region RelativeDate
                IRelativeDateOffset relativeDate = pExercise.ExpirationDate.RelativeDate;
                ret = Tools.OffSetDateRelativeTo(m_Cs, relativeDate, out DateTime offsetDate, m_DataDocument);
                if (ret == Cst.ErrLevel.SUCCESS)
                    europeanDateAdjustement = new EFS_AdjustableDate(m_Cs, offsetDate, relativeDate.GetAdjustments, m_DataDocument);
                #endregion RelativeDate
            }
            #endregion ExpirationDate
            #region RelevantUnderlyingDates
            if (pExercise.RelevantUnderlyingDateSpecified)
                ret = SetRelevantUnderlyingDates(pExercise.RelevantUnderlyingDate);
            #endregion RelevantUnderlyingDates

            return ret;
        }
        #endregion European

        #region SetAdjustedDatesEvent
        public Cst.ErrLevel SetAdjustedDatesEvent()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            int nbRelevantDates = 0;
            int nbCashSettlementValuationDates = 0;
            int nbCashSettlementPaymentDates = 0;
            ArrayList aExerciseDatesEvents = new ArrayList();
            if (relevantUnderlyingDateAdjustementSpecified)
                nbRelevantDates = relevantUnderlyingDateAdjustement.adjustableDates.Length;
            if (cashSettlementValuationDateAdjustementSpecified)
                nbCashSettlementValuationDates = cashSettlementValuationDateAdjustement.adjustableDates.Length;
            if (cashSettlementPaymentDateAdjustementSpecified)
                nbCashSettlementPaymentDates = cashSettlementPaymentDateAdjustement.adjustableDates.Length;
            
            int nbExerciseDates;
            EFS_ExerciseDatesEvent exerciseDatesEvent;
            DateTime dtExercise;
            DateTime dtRelevantDate;
            DateTime dtCashSettlementPaymentDate;
            DateTime dtCashSettlementValuationDate;
            if (bermudaDateAdjustementSpecified)
            {
                #region Bermuda
                nbExerciseDates = bermudaDateAdjustement.adjustableDates.Length;
                #region Loop into ExerciseDates
                for (int i = 0; i < nbExerciseDates; i++)
                {
                    dtExercise = bermudaDateAdjustement.adjustableDates[i].AdjustedEventDate.DateValue;
                    dtRelevantDate = dtExercise;
                    #region ExerciseDate
                    exerciseDatesEvent = new EFS_ExerciseDatesEvent
                    {
                        startExerciseDate = new EFS_Date
                        {
                            DateValue = dtExercise
                        },
                        endExerciseDate = new EFS_Date
                        {
                            DateValue = dtExercise
                        }
                    };
                    #endregion ExerciseDate
                    #region RelevantUnderlyingDate
                    if (relevantUnderlyingDateAdjustementSpecified)
                    {
                        if (nbExerciseDates == nbRelevantDates)
                        {
                            // Il y a une date de relevant pour une date d'exercice
                            dtRelevantDate = relevantUnderlyingDateAdjustement.adjustableDates[i].AdjustedEventDate.DateValue;
                            exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date
                            {
                                DateValue = dtRelevantDate
                            };
                        }
                        else
                        {
                            // Il y a une date de relevant pour n dates d'exercice
                            for (int j = 0; j < nbRelevantDates; j++)
                            {
                                dtRelevantDate = relevantUnderlyingDateAdjustement.adjustableDates[j].AdjustedEventDate.DateValue;
                                if (0 <= dtRelevantDate.CompareTo(dtExercise))
                                {
                                    exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date
                                    {
                                        DateValue = dtRelevantDate
                                    };
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date
                        {
                            DateValue = dtRelevantDate
                        };
                    }
                    exerciseDatesEvent.relevantUnderlyingDateSpecified = (null != exerciseDatesEvent.relevantUnderlyingDate);
                    #endregion RelevantUnderlyingDate
                    #region CashSettlementValuationDate
                    if (cashSettlementValuationDateAdjustementSpecified)
                    {
                        if (nbExerciseDates == nbCashSettlementValuationDates)
                        {
                            // Il y a une date de cashSettlementValuation pour une date d'exercice
                            dtCashSettlementValuationDate = cashSettlementValuationDateAdjustement.adjustableDates[i].AdjustedEventDate.DateValue;
                            exerciseDatesEvent.cashSettlementValuationDate = new EFS_Date
                            {
                                DateValue = dtCashSettlementValuationDate
                            };
                        }
                        else
                        {
                            // Il y a une date de cashSettlementValuation pour n dates d'exercice
                            for (int j = 0; j < nbCashSettlementValuationDates; j++)
                            {
                                dtCashSettlementValuationDate = cashSettlementValuationDateAdjustement.adjustableDates[j].AdjustedEventDate.DateValue;
                                if (0 <= dtCashSettlementValuationDate.CompareTo(dtExercise))
                                {
                                    exerciseDatesEvent.cashSettlementValuationDate = new EFS_Date
                                    {
                                        DateValue = dtCashSettlementValuationDate
                                    };
                                    break;
                                }
                            }
                        }
                    }
                    exerciseDatesEvent.cashSettlementValuationDateSpecified = (null != exerciseDatesEvent.cashSettlementValuationDate);
                    #endregion CashSettlementValuationDate
                    #region CashSettlementPaymentDate
                    if (cashSettlementPaymentDateAdjustementSpecified)
                    {
                        if (nbExerciseDates == nbCashSettlementPaymentDates)
                        {
                            // Il y a une date de cashSettlementPayment pour une date d'exercice
                            dtCashSettlementPaymentDate = cashSettlementPaymentDateAdjustement.adjustableDates[i].AdjustedEventDate.DateValue;
                            exerciseDatesEvent.cashSettlementPaymentDate = new EFS_Date
                            {
                                DateValue = dtCashSettlementPaymentDate
                            };
                        }
                        else
                        {
                            // Il y a une date de cashSettlementPayment pour n dates d'exercice
                            for (int j = 0; j < nbCashSettlementPaymentDates; j++)
                            {
                                dtCashSettlementPaymentDate = cashSettlementPaymentDateAdjustement.adjustableDates[j].AdjustedEventDate.DateValue;
                                if (0 <= dtCashSettlementPaymentDate.CompareTo(dtExercise))
                                {
                                    exerciseDatesEvent.cashSettlementPaymentDate = new EFS_Date
                                    {
                                        DateValue = dtCashSettlementPaymentDate
                                    };
                                    break;
                                }
                            }
                        }
                    }
                    exerciseDatesEvent.cashSettlementPaymentDateSpecified = (null != exerciseDatesEvent.cashSettlementPaymentDate);
                    #endregion CashSettlementPaymentDate

                    aExerciseDatesEvents.Add(exerciseDatesEvent);
                }
                #endregion Loop into ExerciseDates
                #region Save To exerciseDatesEvents
                if (0 < aExerciseDatesEvents.Count)
                    bermudaExerciseDatesEvents = (EFS_ExerciseDatesEvent[])aExerciseDatesEvents.ToArray(typeof(EFS_ExerciseDatesEvent));
                bermudaExerciseDatesEventsSpecified = (null != bermudaExerciseDatesEvents);
                #endregion Save To exerciseDatesEvents
                #endregion Bermuda
            }
            else if (europeanDateAdjustementSpecified)
            {
                #region European
                nbExerciseDates = 1;
                dtExercise = europeanDateAdjustement.AdjustedEventDate.DateValue;
                #region ExerciseDate
                exerciseDatesEvent = new EFS_ExerciseDatesEvent
                {
                    startExerciseDate = new EFS_Date
                    {
                        DateValue = dtExercise
                    },
                    endExerciseDate = new EFS_Date
                    {
                        DateValue = dtExercise
                    }
                };
                #endregion ExerciseDate
                #region RelevantUnderlyingDate
                if (relevantUnderlyingDateAdjustementSpecified)
                {
                    if (nbExerciseDates == nbRelevantDates)
                    {
                        exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date();
                        dtRelevantDate = relevantUnderlyingDateAdjustement.adjustableDates[0].AdjustedEventDate.DateValue;
                        exerciseDatesEvent.relevantUnderlyingDate.DateValue = dtRelevantDate;
                    }
                }
                exerciseDatesEvent.relevantUnderlyingDateSpecified = (null != exerciseDatesEvent.relevantUnderlyingDate);
                #endregion RelevantUnderlyingDate
                #region CashSettlementValuationDate
                if (cashSettlementValuationDateAdjustementSpecified)
                {
                    if (nbExerciseDates == nbCashSettlementValuationDates)
                    {
                        exerciseDatesEvent.cashSettlementValuationDate = new EFS_Date();
                        dtCashSettlementValuationDate = cashSettlementValuationDateAdjustement.adjustableDates[0].AdjustedEventDate.DateValue;
                        exerciseDatesEvent.cashSettlementValuationDate.DateValue = dtCashSettlementValuationDate;
                    }
                }
                exerciseDatesEvent.cashSettlementValuationDateSpecified = (null != exerciseDatesEvent.cashSettlementValuationDate);
                #endregion CashSettlementValuationDate
                #region CashSettlementPaymentDate
                if (cashSettlementPaymentDateAdjustementSpecified)
                {
                    if (nbExerciseDates == nbCashSettlementPaymentDates)
                    {
                        exerciseDatesEvent.cashSettlementPaymentDate = new EFS_Date();
                        dtCashSettlementPaymentDate = cashSettlementPaymentDateAdjustement.adjustableDates[0].AdjustedEventDate.DateValue;
                        exerciseDatesEvent.cashSettlementPaymentDate.DateValue = dtCashSettlementPaymentDate;
                    }
                }
                exerciseDatesEvent.cashSettlementPaymentDateSpecified = (null != exerciseDatesEvent.cashSettlementPaymentDate);
                #endregion CashSettlementPaymentDate

                aExerciseDatesEvents.Add(exerciseDatesEvent);
                #region Save To exerciseDatesEvents
                if (0 < aExerciseDatesEvents.Count)
                    europeanExerciseDatesEvents = (EFS_ExerciseDatesEvent[])aExerciseDatesEvents.ToArray(typeof(EFS_ExerciseDatesEvent));
                europeanExerciseDatesEventsSpecified = (null != europeanExerciseDatesEvents);
                #endregion Save To exerciseDatesEvents

                #endregion European
            }
            else if (americanDateAdjustementSpecified)
            {
                #region American
                DateTime dtStartExercise = americanDateAdjustement.AdjustedStartPeriod.DateValue;
                DateTime dtEndExercise = americanDateAdjustement.AdjustedEndPeriod.DateValue;
                #region ExerciseDate
                exerciseDatesEvent = new EFS_ExerciseDatesEvent
                {
                    startExerciseDate = new EFS_Date
                    {
                        DateValue = dtStartExercise
                    },
                    endExerciseDate = new EFS_Date
                    {
                        DateValue = dtEndExercise
                    }
                };
                #endregion ExerciseDate
                #region RelevantUnderlyingDate
                if (relevantUnderlyingDateAdjustementSpecified && (1 == nbRelevantDates))
                {
                    dtRelevantDate = relevantUnderlyingDateAdjustement.adjustableDates[0].AdjustedEventDate.DateValue;
                    exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date
                    {
                        DateValue = dtRelevantDate
                    };
                }
                exerciseDatesEvent.relevantUnderlyingDateSpecified = (null != exerciseDatesEvent.relevantUnderlyingDate);
                #endregion RelevantUnderlyingDate
                #region CashSettlementValuationDate
                if (cashSettlementValuationDateAdjustementSpecified && (1 == nbCashSettlementValuationDates))
                {
                    dtCashSettlementValuationDate = cashSettlementValuationDateAdjustement.adjustableDates[0].AdjustedEventDate.DateValue;
                    exerciseDatesEvent.cashSettlementValuationDate = new EFS_Date
                    {
                        DateValue = dtCashSettlementValuationDate
                    };
                }
                exerciseDatesEvent.cashSettlementValuationDateSpecified = (null != exerciseDatesEvent.cashSettlementValuationDate);
                #endregion CashSettlementValuationDate
                #region CashSettlementPaymentDate
                if (cashSettlementPaymentDateAdjustementSpecified && (1 == nbCashSettlementPaymentDates))
                {
                    dtCashSettlementPaymentDate = cashSettlementPaymentDateAdjustement.adjustableDates[0].AdjustedEventDate.DateValue;
                    exerciseDatesEvent.cashSettlementPaymentDate = new EFS_Date
                    {
                        DateValue = dtCashSettlementPaymentDate
                    };
                }
                exerciseDatesEvent.cashSettlementPaymentDateSpecified = (null != exerciseDatesEvent.cashSettlementPaymentDate);
                #endregion CashSettlementPaymentDate

                aExerciseDatesEvents.Add(exerciseDatesEvent);
                #region Save To exerciseDatesEvents
                if (0 < aExerciseDatesEvents.Count)
                    americanExerciseDatesEvents = (EFS_ExerciseDatesEvent[])aExerciseDatesEvents.ToArray(typeof(EFS_ExerciseDatesEvent));
                americanExerciseDatesEventsSpecified = (null != americanExerciseDatesEvents);
                #endregion Save To exerciseDatesEvents

                #endregion American
            }

            return ret;
        }
        #endregion SetAdjustedDatesEvent
        #region SetCashSettlementDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel SetCashSettlementDates(object pObjectDeclaringExercise)
        {
            Cst.ErrLevel ret;
            ICashSettlement cashSettlement;
            //20080903 FI pObjectDeclaringExercise n'est pas uniquement un IProvision => il peut être un ISwaption
            if (Tools.IsTypeOrInterfaceOf(pObjectDeclaringExercise, InterfaceEnum.ISwaption))
                cashSettlement = ((ISwaption)pObjectDeclaringExercise).CashSettlement;
            else if (Tools.IsTypeOrInterfaceOf(pObjectDeclaringExercise, InterfaceEnum.IProvision))
                cashSettlement = ((IProvision)pObjectDeclaringExercise).CashSettlement;
            else
                throw new NotImplementedException(pObjectDeclaringExercise.ToString());

            if (null != cashSettlement)
            {
                #region cashSettlementValuationDate
                cashSettlementValuationDateAdjustement = new EFS_AdjustableDates();
                ret = Tools.OffSetDateRelativeTo(m_Cs, cashSettlement.ValuationDate, out DateTime[] offsetDates, m_DataDocument);
                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    cashSettlementValuationDateAdjustement.adjustableDates = new EFS_AdjustableDate[offsetDates.Length];
                    for (int i = 0; i < offsetDates.Length; i++)
                    {
                        cashSettlementValuationDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, offsetDates[i],
                            cashSettlement.ValuationDate.GetAdjustments, m_DataDocument);
                    }
                    cashSettlementValuationDateAdjustementSpecified = true;
                }
                #endregion cashSettlementValuationDate

                #region cashSettlementPaymentDate
                if (cashSettlement.PaymentDateSpecified)
                {
                    ICashSettlementPaymentDate paymentDate = cashSettlement.PaymentDate;
                    cashSettlementPaymentDateAdjustement = new EFS_AdjustableDates();
                    if (paymentDate.AdjustableDatesSpecified)
                    {
                        #region AdjustableDates
                        IAdjustableDates adjustableDates = paymentDate.AdjustableDates;
                        object[] unadjustedDates = adjustableDates.UnadjustedDate;
                        cashSettlementPaymentDateAdjustement.adjustableDates = new EFS_AdjustableDate[unadjustedDates.Length];
                        for (int i = 0; i < unadjustedDates.Length; i++)
                        {
                            cashSettlementPaymentDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, adjustableDates[i],
                                adjustableDates.DateAdjustments, m_DataDocument);
                        }
                        cashSettlementPaymentDateAdjustementSpecified = true;
                        #endregion AdjustableDates
                    }
                    else if (paymentDate.BusinessDateRangeSpecified)
                    {
                        #region BusinessDateRange
                        //20071011 EG TO OR NOT TO DO 
                        cashSettlementPaymentDateAdjustementSpecified = false;
                        #endregion BusinessDateRange
                    }
                    else if (paymentDate.RelativeDateSpecified)
                    {
                        #region RelativeDateOffset
                        ret = Tools.OffSetDateRelativeTo(m_Cs, paymentDate.RelativeDate, out offsetDates, m_DataDocument);
                        if (ret == Cst.ErrLevel.SUCCESS)
                        {
                            cashSettlementPaymentDateAdjustement.adjustableDates = new EFS_AdjustableDate[offsetDates.Length];
                            for (int i = 0; i < offsetDates.Length; i++)
                            {
                                cashSettlementPaymentDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, offsetDates[i],
                                    paymentDate.RelativeDate.GetAdjustments, m_DataDocument);
                            }
                            cashSettlementPaymentDateAdjustementSpecified = true;
                        }
                        #endregion RelativeDateOffset
                    }
                }
                #endregion cashSettlementPaymentDate
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetCashSettlementDates
        #region SetRelevantUnderlyingDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel SetRelevantUnderlyingDates(IAdjustableOrRelativeDates pRelevantUnderlyingDate)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;

            relevantUnderlyingDateAdjustement = new EFS_AdjustableDates();
            if (pRelevantUnderlyingDate.AdjustableDatesSpecified)
            {
                #region AdjustableDates
                IAdjustableDates adjustableDates = pRelevantUnderlyingDate.AdjustableDates;
                relevantUnderlyingDateAdjustement.adjustableDates = new EFS_AdjustableDate[adjustableDates.UnadjustedDate.Length];
                for (int i = 0; i < adjustableDates.UnadjustedDate.Length; i++)
                {
                    relevantUnderlyingDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, adjustableDates[i], 
                        adjustableDates.DateAdjustments, m_DataDocument);
                }
                #endregion AdjustableDates
                ret = Cst.ErrLevel.SUCCESS;
            }
            else if (pRelevantUnderlyingDate.RelativeDatesSpecified)
            {

                #region RelativeDates
                IRelativeDates relativeDates = pRelevantUnderlyingDate.RelativeDates;
                ret = Tools.OffSetDateRelativeTo(m_Cs, relativeDates, out DateTime[] offsetDates, m_DataDocument);
                if (ret == Cst.ErrLevel.SUCCESS)
                {
                    relevantUnderlyingDateAdjustement.adjustableDates = new EFS_AdjustableDate[offsetDates.Length];
                    for (int i = 0; i < offsetDates.Length; i++)
                    {
                        relevantUnderlyingDateAdjustement.adjustableDates[i] = new EFS_AdjustableDate(m_Cs, offsetDates[i], relativeDates.GetAdjustments, m_DataDocument);
                    }
                }
                #endregion RelativeDates
            }
            relevantUnderlyingDateAdjustementSpecified = (null != relevantUnderlyingDateAdjustement);

            return ret;
        }
        #endregion SetRelevantUnderlyingDates

        #endregion Methods
    }
    #endregion EFS_ExerciseDatesBase

    #region EFS_FraDates
    public class EFS_FraDates
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        private Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public EFS_AdjustableDate paymentDateAdjustment;
        public EFS_AdjustableDate fixingDateAdjustment;
        public EFS_PreSettlement preSettlement;
        public bool preSettlementSpecified;
        #endregion Members
        #region Accessors
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {

                if (preSettlementSpecified)
                    return preSettlement.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FraDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FraDates(string pConnectionString, IAdjustableDate pPaymentDate, IRelativeDateOffset pFixingDateOffset, DateTime pAdjustedTerminationDate, IFra pProduct, DataDocumentContainer pDataDocument)
            :this(pConnectionString, pDataDocument)
        {
            Calc(pPaymentDate, pFixingDateOffset, pAdjustedTerminationDate, pProduct);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculate for a FRA trade all unadjusted and adjusted dates
        /// </summary>
        /// <param name="pPaymentDate">Fra PaymentDate element</param>
        /// <param name="pFixingDateOffset">Fra FixingDateOffset element</param>
        /// <param name="pAdjustedTerminationDate">Fra AdjustedTerminationDate element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void Calc(IAdjustableDate pPaymentDate, IRelativeDateOffset pFixingDateOffset, DateTime pAdjustedTerminationDate, IFra pProduct)
        {
            m_ErrLevel = Cst.ErrLevel.UNDEFINED;

            paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, pPaymentDate, m_DataDocument);
            IBusinessDayAdjustments bda = pFixingDateOffset.GetAdjustments;
            DateTime payDate = Tools.ApplyOffset(m_Cs, pPaymentDate.UnadjustedDate.DateValue, pAdjustedTerminationDate, pFixingDateOffset.GetOffset, bda, m_DataDocument);

            if (Convert.ToDateTime(null) != payDate)
            {
                fixingDateAdjustment = new EFS_AdjustableDate(m_Cs, payDate, bda, m_DataDocument);

                // 20070823 EG Ticket : 15643
                #region preSettlementPaymentDate
                EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, pProduct.Notional.Currency,
                    pProduct.BuyerPartyReference.HRef, pProduct.SellerPartyReference.HRef, m_DataDocument, bda.DefaultOffsetPreSettlement);
                preSettlementSpecified = (preSettlementInfo.IsUsePreSettlement);
                if (preSettlementSpecified)
                    preSettlement = new EFS_PreSettlement(m_Cs, null, paymentDateAdjustment.adjustedDate.DateValue, preSettlementInfo.Currency, preSettlementInfo.OffsetPreSettlement, m_DataDocument);
                #endregion preSettlementPaymentDate
                m_ErrLevel = Cst.ErrLevel.SUCCESS;
            }
            else
                m_ErrLevel = Cst.ErrLevel.ABORTED;

        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FraDates
    #region EFS_FxLinkedNotionalDate
    public class EFS_FxLinkedNotionalDate
    {
        private readonly string m_Cs;
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate paymentDateAdjustment;
        public EFS_AdjustableDate fixingDateAdjustment;

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLinkedNotionalDate(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, pDataDocument);
            fixingDateAdjustment = new EFS_AdjustableDate(m_Cs, pDataDocument);
        }
        #endregion Constructors
    }
    #endregion EFS_FxLinkedNotionalDate
    #region EFS_FxLinkedNotionalDates
    public class EFS_FxLinkedNotionalDates
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        [System.Xml.Serialization.XmlArrayItemAttribute("fxLinkedNotionalDate")]
        public EFS_FxLinkedNotionalDate[] fxLinkedNotionalDates;
        public string currencyNotionalScheduleReference;
        #endregion Members

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLinkedNotionalDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument; 
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLinkedNotionalDates(string pConnectionString, IFxLinkedNotionalSchedule pNotionalSchedule, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            Calc(pNotionalSchedule);
        }
        #endregion Constructors
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc(IFxLinkedNotionalSchedule pNotionalSchedule)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            IInterval paymentFrequency = null;
            object exchangeDates = null;
            IInterval fixingFrequency = null;

            #region Currency NotionalScheduleReference
            object notionalReference = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, pNotionalSchedule.ConstantNotionalScheduleReference.HRef);
            if ((null != notionalReference) && Tools.IsTypeOrInterfaceOf(notionalReference, InterfaceEnum.INotional))
                currencyNotionalScheduleReference = ((INotional)notionalReference).StepSchedule.Currency.Value;
            #endregion Currency NotionalScheduleReference

            #region The payment and fixing dates must be relative either to payment dates or to reset dates
            // Payment Frequency 
            object exchangeReference = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, pNotionalSchedule.VaryingNotionalInterimExchangePaymentDates.DateRelativeToValue);
            if (Tools.IsTypeOrInterfaceOf(exchangeReference, InterfaceEnum.IPaymentDates))
            {
                IPaymentDates reference = (IPaymentDates)exchangeReference;
                paymentFrequency = reference.PaymentFrequency;
                exchangeDates = reference.Efs_PaymentDates.paymentDates;
            }
            else if (Tools.IsTypeOrInterfaceOf(exchangeReference, InterfaceEnum.IResetDates))
            {
                IResetDates reference = (IResetDates)exchangeReference;
                paymentFrequency = reference.ResetFrequency.Interval;
                exchangeDates = reference.Efs_ResetDates.resetDates;
            }
            else if (Tools.IsTypeOrInterfaceOf(exchangeReference, InterfaceEnum.ICalculationPeriodDates))
            {
                ICalculationPeriodDates reference = (ICalculationPeriodDates)exchangeReference;
                paymentFrequency = reference.CalculationPeriodFrequency.Interval;
                exchangeDates = reference.Efs_CalculationPeriodDates.calculationPeriods;
            }
            else
                ret = Cst.ErrLevel.ABORTED;

            // Fixing Frequency 
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                object fixingReference = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, pNotionalSchedule.VaryingNotionalFixingDates.DateRelativeToValue);
                if (Tools.IsTypeOrInterfaceOf(fixingReference, InterfaceEnum.IPaymentDates))
                    fixingFrequency = ((IPaymentDates)fixingReference).PaymentFrequency;
                else if (Tools.IsTypeOrInterfaceOf(fixingReference, InterfaceEnum.IResetDates))
                    fixingFrequency = ((IResetDates)fixingReference).ResetFrequency.Interval;
                else if (Tools.IsTypeOrInterfaceOf(fixingReference, InterfaceEnum.ICalculationPeriodDates))
                    fixingFrequency = ((ICalculationPeriodDates)fixingReference).CalculationPeriodFrequency.Interval;
                else
                    ret = Cst.ErrLevel.ABORTED;
            }
            #endregion The payment and fixing dates must be relative either to payment dates or to reset dates
            #region The payment and fixingDates must be relative to dates sharing the same frequency
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                Tools.CheckFrequency(paymentFrequency, fixingFrequency, out _);
                if ((paymentFrequency.Period != fixingFrequency.Period) ||
                    (paymentFrequency.PeriodMultiplier.DecValue != fixingFrequency.PeriodMultiplier.DecValue))
                    ret = Cst.ErrLevel.ABORTED;
            }
            #endregion The payment and fixingDates must be relative to dates sharing the same frequency

            #region process
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                #region Loop for case payment relativeTo payment dates or reset dates
                Array aExchangeDates = exchangeDates as Array;
                fxLinkedNotionalDates = new EFS_FxLinkedNotionalDate[aExchangeDates.Length];
                for (int i = 0; i < aExchangeDates.Length; i++)
                {
                    object item = aExchangeDates.GetValue(i);
                    fxLinkedNotionalDates[i] = new EFS_FxLinkedNotionalDate(m_Cs, m_DataDocument);
                    if (exchangeDates.GetType().GetElementType().Equals(typeof(EFS_PaymentDate)))
                        fxLinkedNotionalDates[i].periodDatesAdjustment = ((EFS_PaymentDate)item).periodDatesAdjustment;
                    else if (exchangeDates.GetType().GetElementType().Equals(typeof(EFS_ResetDate)))
                        fxLinkedNotionalDates[i].periodDatesAdjustment = ((EFS_ResetDate)item).periodDatesAdjustment;
                    else if (exchangeDates.GetType().GetElementType().Equals(typeof(EFS_CalculationPeriod)))
                        fxLinkedNotionalDates[i].periodDatesAdjustment = ((EFS_CalculationPeriod)item).periodDates;
                }

                IRelativeDateOffset exchange = pNotionalSchedule.VaryingNotionalInterimExchangePaymentDates;
                IRelativeDateOffset fixing = pNotionalSchedule.VaryingNotionalFixingDates;
                if ((Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(m_Cs, exchange, out DateTime[] offsetExchangeDates, m_DataDocument)) &&
                    (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(m_Cs, fixing, out DateTime[] offsetFixingDates, m_DataDocument)))
                {
                    int i = 0;
                    foreach (EFS_FxLinkedNotionalDate fxLinkedNotionalDate in fxLinkedNotionalDates)
                    {
                        fxLinkedNotionalDate.paymentDateAdjustment =
                            new EFS_AdjustableDate(m_Cs, offsetExchangeDates[i], exchange.GetAdjustments, m_DataDocument);
                        fxLinkedNotionalDate.fixingDateAdjustment = new EFS_AdjustableDate(m_Cs, offsetFixingDates[i], fixing.GetAdjustments, m_DataDocument);
                        i++;
                    }
                }
                else
                    ret = Cst.ErrLevel.ABORTED;
                #endregion Loop for case payment relativeTo payment dates or reset dates
            }
            #endregion process

            return ret;
        }
        #endregion Calc
    }
    #endregion EFS_FxLinkedNotionalDates
    #region EFS_FxLinkedNominalPeriod
    public class EFS_FxLinkedNominalPeriod : ICloneable
    {
        #region Members
        private string m_Cs;
        private DataDocumentContainer m_DataDocument;
        public EFS_AdjustablePeriod periodDates;
        public EFS_AdjustableDate paymentDateAdjustment;
        public EFS_AdjustableDate fixingDateAdjustment;
        public bool fixingDateAdjustmentSpecified;
        public EFS_AdjustableDate terminationDateAdjustment;
        public string currency;
        public string currencyNotionalScheduleReference;
        public EFS_Decimal periodAmount;
        public EFS_Decimal periodVariationAmount;
        public EFS_FxFixing fxFixing;
        public bool fxFixingSpecified;
        public EFS_PreSettlement preSettlementPaymentDate;
        public EFS_PreSettlement preSettlementEndPeriod;
        public bool preSettlementSpecified;
        #endregion Members
        #region Constructors
        public EFS_FxLinkedNominalPeriod()
        {
            periodDates = new EFS_AdjustablePeriod();
            periodAmount = new EFS_Decimal();
            periodVariationAmount = new EFS_Decimal();
        }
        public EFS_FxLinkedNominalPeriod(EFS_AdjustablePeriod pPeriodDates, EFS_AdjustableDate pPaymentDate, EFS_AdjustableDate pFixingDate, EFS_AdjustableDate pTerminationDate)
        {
            periodDates = (EFS_AdjustablePeriod)pPeriodDates.Clone();
            paymentDateAdjustment = (EFS_AdjustableDate)pPaymentDate.Clone();
            fixingDateAdjustment = (EFS_AdjustableDate)pFixingDate.Clone();
            terminationDateAdjustment = (EFS_AdjustableDate)pTerminationDate.Clone();
        }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLinkedNominalPeriod(string pConnectionString, EFS_AdjustableDate pStartPeriod, EFS_AdjustableDate pEndPeriod,
            EFS_AdjustableDate pPaymentDate, EFS_AdjustableDate pFixingDate, EFS_AdjustableDate pTerminationDate,
            EFS_Decimal pPeriodAmount, string pCurrency, EFS_Decimal pPeriodVariationAmount, string pCurrencyNotionalScheduleReference,
            IFxSpotRateSource pFxSpotRateSource, EFS_SettlementInfoEntity pPreSettlementInfo, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDates = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)pStartPeriod.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)pEndPeriod.Clone()
            };
            paymentDateAdjustment = (EFS_AdjustableDate)pPaymentDate.Clone();
            fixingDateAdjustment = (EFS_AdjustableDate)pFixingDate.Clone();
            terminationDateAdjustment = (EFS_AdjustableDate)pTerminationDate.Clone();
            currency = pCurrency;
            currencyNotionalScheduleReference = pCurrencyNotionalScheduleReference;
            periodAmount = (EFS_Decimal)pPeriodAmount.Clone();
            periodVariationAmount = (EFS_Decimal)pPeriodVariationAmount.Clone();
            SetFxFixing(pFxSpotRateSource);
            SetPreSettlementDate(pPreSettlementInfo);
        }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLinkedNominalPeriod(string pConnectionString, EFS_AdjustableDate pStartPeriod, EFS_AdjustableDate pEndPeriod,
            EFS_AdjustableDate pPaymentDate, EFS_AdjustableDate pFixingDate, EFS_AdjustableDate pTerminationDate,
            decimal pPeriodAmount, string pCurrency, decimal pPeriodVariationAmount, string pCurrencyNotionalScheduleReference,
            IFxSpotRateSource pFxSpotRateSource, EFS_SettlementInfoEntity pPreSettlementInfo, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDates = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)pStartPeriod.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)pEndPeriod.Clone()
            };
            paymentDateAdjustment = (EFS_AdjustableDate)pPaymentDate.Clone();
            fixingDateAdjustment = (EFS_AdjustableDate)pFixingDate.Clone();
            terminationDateAdjustment = (EFS_AdjustableDate)pTerminationDate.Clone();
            currency = pCurrency;
            currencyNotionalScheduleReference = pCurrencyNotionalScheduleReference;
            periodAmount = new EFS_Decimal(pPeriodAmount);
            periodVariationAmount = new EFS_Decimal(pPeriodVariationAmount);
            SetFxFixing(pFxSpotRateSource);
            SetPreSettlementDate(pPreSettlementInfo);
        }
        #endregion Constructors

        #region ICloneable
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Clone()
        {
            EFS_FxLinkedNominalPeriod clone = new EFS_FxLinkedNominalPeriod
            {
                periodDates = (EFS_AdjustablePeriod)this.periodDates.Clone(),
                periodAmount = (EFS_Decimal)this.periodAmount.Clone(),
                currency = this.currency,
                currencyNotionalScheduleReference = this.currencyNotionalScheduleReference,
                periodVariationAmount = (EFS_Decimal)this.periodVariationAmount.Clone(),
                fxFixing = new EFS_FxFixing(fxFixing.eventType, fxFixing.fixing),
                m_Cs = this.m_Cs,
                m_DataDocument = this.m_DataDocument,
                preSettlementSpecified = this.preSettlementSpecified,
                preSettlementPaymentDate = (EFS_PreSettlement)this.preSettlementPaymentDate.Clone(),
                preSettlementEndPeriod = (EFS_PreSettlement)this.preSettlementEndPeriod.Clone()
            };
            clone.fxFixing.notionalAmount = (EFS_Decimal)this.periodVariationAmount.Clone();
            clone.fxFixing.referenceCurrency = this.currencyNotionalScheduleReference;
            return clone;
        }
        #endregion ICloneable

        #region Accessors
        #region AbsoluteVariationAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal AbsoluteVariationAmount
        {
            get
            {

                EFS_Decimal clone = null;
                if (null != this.periodVariationAmount)
                {
                    clone = (EFS_Decimal)this.periodVariationAmount.Clone();
                    clone.DecValue = System.Math.Abs(clone.DecValue);
                    //20050919 PL/FDA Suite à modif de EFS_Decimal
                    //clone.Value = (string) Tools.FormatValue(clone);
                }
                return clone;

            }
        }

        #endregion AbsoluteVariationAmount
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.adjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion AdjustedEndPeriod
        #region AdjustedFixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedFixingDate
        {
            get
            {

                if (fixingDateAdjustmentSpecified)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = fixingDateAdjustment.adjustedDate.DateValue
                    };
                    return dt;
                }
                return null;

            }
        }

        #endregion AdjustedFixingDate
        #region AdjustedPreSettlementEndDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementEndDate
        {
            get
            {

                if (preSettlementSpecified)
                    return preSettlementEndPeriod.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementEndDate
        #region AdjustedPreSettlementPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementPaymentDate
        {
            get
            {

                if (preSettlementSpecified)
                    return preSettlementPaymentDate.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = paymentDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion AdjustedPaymentDate
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.adjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion AdjustedStartPeriod
        #region AssetFxRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object AssetFxRate
        {
            get
            {
                if (fxFixingSpecified)
                    return Tools.GetAssetFxRate(fxFixing);
                else
                    return null;
            }
        }
        #endregion AssetFxRate
        #region FixingRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxFixing FixingRate
        {
            get
            {

                if (fxFixingSpecified)
                    return fxFixing;
                else
                    return null;

            }
        }
        #endregion FixingRate
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate2);

            }
        }

        #endregion EndPeriod
        #region NominalPartyReference
        public static string NominalPartyReference(string pType, string pPayer, string pReceiver, EFS_Decimal pVariation)
        {

            if (null != pVariation)
            {
                bool isAmort = (1 != System.Math.Sign(pVariation.DecValue));
                bool isPayer = (PayerReceiverEnum.Payer.ToString() == pType);
                bool isReceiver = (PayerReceiverEnum.Receiver.ToString() == pType);
                if ((isAmort && isPayer) || ((false == isAmort) && isReceiver))
                    return pPayer;
                else if ((isAmort && isReceiver) || ((false == isAmort) && isPayer))
                    return pReceiver;
            }
            return null;

        }
        #endregion NominalPartyReference
        #region PeriodAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PeriodAmount
        {
            get
            {

                return this.periodAmount;

            }
        }

        #endregion PeriodAmount
        #region PeriodCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PeriodCurrency
        {
            get
            {

                return this.currency;

            }
        }
        #endregion PeriodCurrency
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate1);

            }
        }

        #endregion StartPeriod
        #region TerminationPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TerminationPeriod
        {
            get
            {

                return new EFS_EventDate(terminationDateAdjustment);

            }
        }

        #endregion TerminationPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion UnadjustedEndPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion UnadjustedStartPeriod
        #region VariationAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal VariationAmount
        {
            get
            {
                //if (null != this.periodVariationAmount)
                //	this.periodVariationAmount.Value = (string) Tools.FormatValue(periodVariationAmount);
                //20050919 PL/FDA Suite à modif de EFS_Decimal
                return this.periodVariationAmount;

            }
        }

        #endregion VariationAmount
        #region GetEventCodeStart
        // 20071015 EG Ticket 15858
        public static string GetEventCodeStart(EFS_EventDate pEffectiveDate, EFS_EventDate pMinEffectiveDate)
        {

            if (0 == pMinEffectiveDate.unadjustedDate.DateValue.CompareTo(pEffectiveDate.unadjustedDate.DateValue))
                return EventCodeFunc.Start;
            else
                return EventCodeFunc.StartIntermediary;

        }
        #endregion GetEventCodeStart
        #region GetEventCodeTermination
        // 20071015 EG Ticket 15858
        public static string GetEventCodeTermination(EFS_EventDate pTerminationDate, EFS_EventDate pMaxTerminationDate)
        {

            if (0 == pMaxTerminationDate.unadjustedDate.DateValue.CompareTo(pTerminationDate.unadjustedDate.DateValue))
                return EventCodeFunc.Termination;
            else
                return EventCodeFunc.TerminationIntermediary;

        }
        #endregion GetEventCodeTermination
        #region GetDateRecognition
        // 20071015 EG Ticket 15858
        public EFS_Date GetDateRecognition(EFS_Date pAdjustedTradeDate, EFS_EventDate pEffectiveDate, EFS_EventDate pMinEffectiveDate)
        {

            if (EventCodeFunc.IsStartIntermediary(GetEventCodeStart(pEffectiveDate, pMinEffectiveDate)))
                return UnadjustedStartPeriod;
            else
                return pAdjustedTradeDate;

        }
        #endregion GetEventCodeTermination
        #endregion Accessors
        #region Methods
        #region SetFxFixing
        private void SetFxFixing(IFxSpotRateSource pFxSpotRateSource)
        {
            IMoney notionalAmountReference = pFxSpotRateSource.CreateMoney(this.periodVariationAmount.DecValue, this.currencyNotionalScheduleReference);
            IFxFixing fixing = pFxSpotRateSource.CreateFxFixing(currencyNotionalScheduleReference, currency, fixingDateAdjustment.adjustedDate.DateValue);
            fxFixing = new EFS_FxFixing(EventTypeFunc.FxRate, fixing, notionalAmountReference);
        }
        #endregion SetFxFixing
        #region SetPreSettlementDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetPreSettlementDate(EFS_SettlementInfoEntity pPreSettlementInfo)
        {
            #region PreSettlementDate
            preSettlementSpecified = (null != pPreSettlementInfo) && pPreSettlementInfo.IsUsePreSettlement;
            if (preSettlementSpecified)
            {
                preSettlementPaymentDate = new EFS_PreSettlement(m_Cs, null, paymentDateAdjustment.AdjustedEventDate, currency, currencyNotionalScheduleReference, pPreSettlementInfo.OffsetPreSettlement, m_DataDocument);
                preSettlementEndPeriod = new EFS_PreSettlement(m_Cs, null, periodDates.AdjustedEndPeriod, currency, currencyNotionalScheduleReference, pPreSettlementInfo.OffsetPreSettlement, m_DataDocument);
            }
            #endregion PreSettlementDate

        }
        #endregion SetPreSettlementDate
        #endregion Methods
    }
    #endregion EFS_AdjustablePeriodAndAmount

    #region EFS_KnownAmountPeriod
    public class EFS_KnownAmountPeriod
    {
        #region Members
        public EFS_Decimal amount;
        public string currency;
        #endregion Members
        #region Constructors
        public EFS_KnownAmountPeriod(decimal pAmount, string pCurrency)
        {
            amount = new EFS_Decimal(pAmount);
            currency = pCurrency;
        }
        #endregion Constructors
    }
    #endregion EFS_KnownAmountPeriod

    #region EFS_MandatoryEarlyTerminationDates
    public class EFS_MandatoryEarlyTerminationDates
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        public EFS_AdjustableDate terminationDate;
        public EFS_AdjustableDate cashSettlementPaymentDate;
        public EFS_AdjustableDate cashSettlementValuationDate;
        public EFS_ExerciseDates efs_ExerciseDates;
        #endregion Members
        #region Accessors
        #region MandatoryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate MandatoryDate
        {
            get
            {

                if (null != terminationDate)
                    return terminationDate.EventDate;
                else
                    return null;

            }
        }
        #endregion MandatoryDate
        #region AdjustedMandatoryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedMandatoryDate
        {
            get
            {

                if (null != terminationDate)
                    return terminationDate.AdjustedEventDate;
                else
                    return null;

            }
        }
        #endregion AdjustedMandatoryDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_MandatoryEarlyTerminationDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_MandatoryEarlyTerminationDates(string pConnectionString, IMandatoryEarlyTermination pMandatoryEarlyTermination, DataDocumentContainer pDataDocument)
            :this(pConnectionString, pDataDocument)
        {
            Calc(pMandatoryEarlyTermination);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculation for a swap with mandatory early termination provision the unadjusted and adjusted 
        /// termination and cash settlement dates
        /// </summary>
        /// <param name="pMandatoryEarlyTermination">Swap MandatoryEarlyTermination element</param>
        /// <returns>Integer ErrorLevel (-1 = Succes)</returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc(IMandatoryEarlyTermination pMandatoryEarlyTermination)
        {
            #region TerminationDate
            terminationDate = new EFS_AdjustableDate(m_Cs, pMandatoryEarlyTermination.MandatoryEarlyTerminationDate, m_DataDocument);
            #endregion TerminationDate
            #region CashSettlementPaymentDate
            ICashSettlement cash = pMandatoryEarlyTermination.CashSettlement;
            Cst.ErrLevel ret;
            DateTime offsetDate;
            if (cash.PaymentDateSpecified)
            {
                ICashSettlementPaymentDate cashPaymentDate = cash.PaymentDate;
                #region AdjustableDates
                if (cashPaymentDate.AdjustableDatesSpecified)
                {
                    cashSettlementPaymentDate = new EFS_AdjustableDate(m_Cs, cashPaymentDate.AdjustableDates[0],
                        cashPaymentDate.AdjustableDates.DateAdjustments, m_DataDocument);
                }
                #endregion AdjustableDates
                #region RelativeDate
                else if (cashPaymentDate.RelativeDateSpecified)
                {
                    ret = Tools.OffSetDateRelativeTo(m_Cs, cashPaymentDate.RelativeDate, out offsetDate, m_DataDocument);
                    if (ret == Cst.ErrLevel.SUCCESS)
                        cashSettlementPaymentDate = new EFS_AdjustableDate(m_Cs, offsetDate, cashPaymentDate.RelativeDate.GetAdjustments, m_DataDocument);
                }
                #endregion RelativeDate
            }
            #endregion CashSettlementPaymentDate
            #region CashSettlementValuationDate
            ret = Tools.OffSetDateRelativeTo(m_Cs, cash.ValuationDate, out offsetDate, m_DataDocument);
            if (ret == Cst.ErrLevel.SUCCESS)
                cashSettlementValuationDate = new EFS_AdjustableDate(m_Cs, offsetDate, cash.ValuationDate.GetAdjustments, m_DataDocument);
            #endregion CashSettlementValuationDate
            #region MandatoryEarlyTerminationAdjustedDates
            if ((ret == Cst.ErrLevel.SUCCESS) && (pMandatoryEarlyTermination.AdjustedDatesSpecified))
            {
                IMandatoryEarlyTerminationAdjustedDates dtAdjusted = pMandatoryEarlyTermination.AdjustedDates;
                terminationDate.adjustedDate.DateValue = dtAdjusted.AdjustedEarlyTerminationDate;
                cashSettlementPaymentDate.adjustedDate.DateValue = dtAdjusted.AdjustedCashSettlementPaymentDate;
                cashSettlementValuationDate.adjustedDate.DateValue = dtAdjusted.AdjustedCashSettlementValuationDate;
            }
            #endregion MandatoryEarlyTerminationAdjustedDates

            #region Virtual European Exercise
            efs_ExerciseDates = new EFS_ExerciseDates(m_Cs, m_DataDocument)
            {
                europeanDateAdjustementSpecified = true,
                europeanDateAdjustement = new EFS_AdjustableDate(m_Cs, terminationDate.AdjustableDate, m_DataDocument)
            };
            efs_ExerciseDates.SetCashSettlementDates(pMandatoryEarlyTermination);
            efs_ExerciseDates.SetAdjustedDatesEvent();
            #endregion Virtual European Exercise


            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_MandatoryEarlyTerminationDates

    #region EFS_NominalPeriod
    public class EFS_NominalPeriod : ICloneable
    {
        #region Members
        public EFS_AdjustablePeriod periodDates;
        public string currency;
        public EFS_Decimal periodAmount;
        public EFS_Decimal periodVariationAmount;
        public EFS_PreSettlement preSettlementStartPeriod;
        public EFS_PreSettlement preSettlementEndPeriod;
        public bool preSettlementSpecified;
        public bool firstPeriodStartDateSpecified;
        public EFS_AdjustableDate firstPeriodStartDate;
        #endregion Members
        #region Accessors
        #region AdjustedFirstStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedFirstStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date();
                if (firstPeriodStartDateSpecified)
                    dt.DateValue = firstPeriodStartDate.adjustedDate.DateValue;
                else
                    dt.DateValue = periodDates.adjustableDate1.adjustedDate.DateValue;
                return dt;

            }
        }
        #endregion AdjustedFirstStartPeriod
        #region FirstStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate FirstStartPeriod
        {
            get
            {

                if (firstPeriodStartDateSpecified)
                    return new EFS_EventDate(firstPeriodStartDate);
                else
                    return StartPeriod;

            }
        }
        #endregion FirstStartPeriod
        #region AdjustedPreSettlementStartDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementStartDate
        {
            get
            {

                if (preSettlementSpecified)
                    return preSettlementStartPeriod.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementStartDate
        #region AdjustedPreSettlementEndDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementEndDate
        {
            get
            {
                if (preSettlementSpecified)
                    return preSettlementEndPeriod.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementEndDate
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedStartPeriod
        #region DateStepStartRecognition
        // 20071119 EG (Min entre StartPeriod ajustée, non ajustée et presettlementStartDate)
        public EFS_Date DateStepStartRecognition
        {
            get
            {
                EFS_Date dtRecognition = new EFS_Date
                {
                    DateValue = StartPeriod.unadjustedDate.DateValue
                };
                if (StartPeriod.adjustedDate.DateValue < dtRecognition.DateValue)
                    dtRecognition.DateValue = StartPeriod.adjustedDate.DateValue;
                return dtRecognition;
            }
        }
        #endregion DateStepStartRecognition
        #region DateStepTerminationRecognition
        // 20071119 EG (Min entre EndPeriod ajustée, non ajustée et presettlementEndDate)
        public EFS_Date DateStepTerminationRecognition
        {
            get
            {
                EFS_Date dtRecognition = new EFS_Date
                {
                    DateValue = EndPeriod.unadjustedDate.DateValue
                };
                if (EndPeriod.adjustedDate.DateValue < dtRecognition.DateValue)
                    dtRecognition.DateValue = EndPeriod.adjustedDate.DateValue;
                return dtRecognition;
            }
        }
        #endregion DateStepTerminationRecognition
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedStartPeriod
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDates.adjustableDate1);

            }
        }
        #endregion StartPeriod
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedEndPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedEndPeriod
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {
                return new EFS_EventDate(periodDates.adjustableDate2);

            }
        }
        #endregion EndPeriod
        #region VariationAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal VariationAmount
        {
            get
            {
                return this.periodVariationAmount;

            }
        }
        #endregion VariationAmount
        #region PeriodCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PeriodCurrency
        {
            get
            {
                return this.currency;

            }
        }
        #endregion PeriodCurrency
        #region AbsoluteVariationAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal AbsoluteVariationAmount
        {
            get
            {

                EFS_Decimal clone = null;
                if (null != this.periodVariationAmount)
                {
                    clone = (EFS_Decimal)this.periodVariationAmount.Clone();
                    clone.DecValue = System.Math.Abs(clone.DecValue);
                    //20050919 PL/FDA Suite à modif de EFS_Decimal
                    //clone.Value    = (string) Tools.FormatValue(clone);
                }
                return clone;

            }
        }
        #endregion AbsoluteVariationAmount
        #region PeriodAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PeriodAmount
        {
            get
            {

                return this.periodAmount;

            }
        }
        #endregion PeriodAmount
        #region NominalPartyReference
        public static string NominalPartyReference(string pType, string pPayer, string pReceiver, EFS_Decimal pVariation)
        {

            if (null != pVariation)
            {
                bool isAmort = (1 != System.Math.Sign(pVariation.DecValue));
                bool isPayer = (PayerReceiverEnum.Payer.ToString() == pType);
                bool isReceiver = (PayerReceiverEnum.Receiver.ToString() == pType);
                if ((isAmort && isPayer) || ((false == isAmort) && isReceiver))
                    return pPayer;
                else if ((isAmort && isReceiver) || ((false == isAmort) && isPayer))
                    return pReceiver;
            }
            return null;

        }
        #endregion NominalPartyReference
        #region GetEventCodeStart
        // 20071015 EG Ticket 15858
        public static string GetEventCodeStart(EFS_EventDate pEffectiveDate, EFS_EventDate pMinEffectiveDate)
        {

            if (0 == pMinEffectiveDate.unadjustedDate.DateValue.CompareTo(pEffectiveDate.unadjustedDate.DateValue))
                return EventCodeFunc.Start;
            else
                return EventCodeFunc.StartIntermediary;

        }
        #endregion GetEventCodeStart
        #region GetEventCodeTermination
        // 20071015 EG Ticket 15858
        public static string GetEventCodeTermination(EFS_EventDate pTerminationDate, EFS_EventDate pMaxTerminationDate)
        {

            if (0 == pMaxTerminationDate.unadjustedDate.DateValue.CompareTo(pTerminationDate.unadjustedDate.DateValue))
                return EventCodeFunc.Termination;
            else
                return EventCodeFunc.TerminationIntermediary;

        }
        #endregion GetEventCodeTermination
        #region GetDateRecognition
        // 20071015 EG Ticket 15858
        public EFS_Date GetDateRecognition(EFS_Date pAdjustedTradeDate, EFS_EventDate pEffectiveDate, EFS_EventDate pMinEffectiveDate)
        {

            if (EventCodeFunc.IsStartIntermediary(GetEventCodeStart(pEffectiveDate, pMinEffectiveDate)))
                // 20071119 EG (Min entre StartPeriod ajustée, non ajustée et presettlementStartDate)
                return DateStepStartRecognition;
            else
                return pAdjustedTradeDate;

        }
        #endregion GetDateRecognition
        #endregion Accessors
        #region Constructors
        public EFS_NominalPeriod()
        {
            periodDates = new EFS_AdjustablePeriod();
            periodAmount = new EFS_Decimal();
            periodVariationAmount = new EFS_Decimal();
        }
        public EFS_NominalPeriod(EFS_AdjustablePeriod pPeriodDates, EFS_AdjustableDate pEffectiveDate)
            : this(pPeriodDates)
        {
            firstPeriodStartDateSpecified = true;
            firstPeriodStartDate = periodDates.adjustableDate1;
            periodDates.adjustableDate1 = (EFS_AdjustableDate)pEffectiveDate.Clone();
        }

        public EFS_NominalPeriod(EFS_AdjustablePeriod pPeriodDates)
        {
            periodDates = (EFS_AdjustablePeriod)pPeriodDates.Clone();
        }

        public EFS_NominalPeriod(EFS_AdjustableDate pStartPeriod, EFS_AdjustableDate pEndPeriod, EFS_Decimal pPeriodAmount, string pCurrency, EFS_Decimal pPeriodVariationAmount)
        {
            periodDates = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)pStartPeriod.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)pEndPeriod.Clone()
            };
            currency = pCurrency;
            periodAmount = (EFS_Decimal)pPeriodAmount.Clone();
            periodVariationAmount = (EFS_Decimal)pPeriodVariationAmount.Clone();
        }

        public EFS_NominalPeriod(EFS_AdjustableDate pStartPeriod, EFS_AdjustableDate pEndPeriod, decimal pPeriodAmount, string pCurrency, decimal pPeriodVariationAmount)
        {
            periodDates = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)pStartPeriod.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)pEndPeriod.Clone()
            };
            periodAmount = new EFS_Decimal(pPeriodAmount);
            currency = pCurrency;
            periodVariationAmount = new EFS_Decimal(pPeriodVariationAmount);
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_NominalPeriod(string pConnectionString, ITermDeposit pTermDeposit, DataDocumentContainer pDataDocument)
        {
            periodDates = new EFS_AdjustablePeriod();
            IBusinessDayAdjustments bda = ((IProduct)pTermDeposit).ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NotApplicable);
            periodDates.adjustableDate1 = new EFS_AdjustableDate(pConnectionString, pTermDeposit.StartDate.DateValue, bda, pDataDocument);
            periodDates.adjustableDate2 = new EFS_AdjustableDate(pConnectionString, pTermDeposit.MaturityDate.DateValue, bda, pDataDocument);
            periodAmount = new EFS_Decimal(pTermDeposit.Principal.Amount.DecValue);
            currency = pTermDeposit.Principal.Currency;

            #region PreSettlementInfo
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(pConnectionString, currency,
                                                            pTermDeposit.InitialPayerReference.HRef,
                                                            pTermDeposit.InitialReceiverReference.HRef,
                                                            pDataDocument,
                                                            periodDates.adjustableDate1.AdjustableDate.DateAdjustments.DefaultOffsetPreSettlement);
            #endregion PreSettlementInfo

            preSettlementSpecified = (null != preSettlementInfo) && (preSettlementInfo.IsUsePreSettlement);
            if (preSettlementSpecified)
            {
                preSettlementStartPeriod = new EFS_PreSettlement(pConnectionString, null, periodDates.adjustableDate1.AdjustedEventDate,
                                                  preSettlementInfo.Currency, preSettlementInfo.OffsetPreSettlement, pDataDocument);
                preSettlementEndPeriod = new EFS_PreSettlement(pConnectionString, null, periodDates.adjustableDate2.AdjustedEventDate,
                                                  preSettlementInfo.Currency, preSettlementInfo.OffsetPreSettlement, pDataDocument);
            }
        }
        #endregion Constructors
        #region ICloneable
        public object Clone()
        {
            EFS_NominalPeriod clone = new EFS_NominalPeriod
            {
                periodDates = (EFS_AdjustablePeriod)this.periodDates.Clone(),
                periodAmount = (EFS_Decimal)this.periodAmount.Clone(),
                currency = this.currency,
                periodVariationAmount = (EFS_Decimal)this.periodVariationAmount.Clone(),
                preSettlementSpecified = this.preSettlementSpecified,
                preSettlementStartPeriod = (EFS_PreSettlement)this.preSettlementStartPeriod.Clone(),
                preSettlementEndPeriod = (EFS_PreSettlement)this.preSettlementEndPeriod.Clone()
            };
            return clone;
        }
        #endregion ICloneable
    }
    #endregion EFS_NominalPeriod

    #region EFS_PaymentDate
    /// <revision>
    ///     <version>1.1.8</version><date>20070823</date><author>EG</author>
    ///     <comment>
    ///     Apply Rules to determine the date of Pre-Settlement events
    ///     </comment>
    /// </revision>
    public class EFS_PaymentDate
    {
        #region Members
        private readonly string m_Cs;
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate paymentDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rateCutOffDateSpecified;
        public IRequiredIdentifierDate rateCutOffDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stubSpecified;
        public StubEnum stub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CalculationPeriod[] calculationPeriods;
        public string currency;
        public EFS_PreSettlement preSettlement;
        public bool preSettlementSpecified;

        public bool exDateAdjustmentSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_AdjustableDate exDateAdjustment;

        public bool recordDateAdjustmentSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_AdjustableDate recordDateAdjustment;

        #endregion Members
        #region Accessors
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {
                // Case Reverse KNA
                if (0 <= DateTime.Compare(periodDatesAdjustment.adjustableDate2.adjustedDate.DateValue, AdjustedPaymentDate.DateValue))
                    return new EFS_Date(AdjustedPaymentDate.Value);

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.adjustedDate.DateValue
                };
                return dt;
            }
        }
        #endregion AdjustedEndPeriod
        #region AdjustedExDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedExDate
        {
            get
            {
                EFS_Date dt = null;
                if (exDateAdjustmentSpecified)
                    dt = new EFS_Date(exDateAdjustment.adjustedDate.Value);
                return dt;
            }
        }
        #endregion AdjustedExDate
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                return new EFS_Date(paymentDateAdjustment.adjustedDate.Value);

            }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {
                if (preSettlementSpecified)
                    return preSettlement.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        #region AdjustedRateCutOff
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedRateCutOff
        {
            get
            {
                if (rateCutOffDateSpecified)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = rateCutOffDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion AdjustedRateCutOff
        #region AdjustedRecordOrEndPeriodDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20190823 [FIXEDINCOME] New
        public EFS_Date AdjustedRecordOrEndPeriodDate
        {
            get
            {
                return recordDateAdjustmentSpecified ? AdjustedRecordDate : AdjustedEndPeriod;
            }
        }
        #endregion AdjustedRecordOrEndPeriodDate
        #region AdjustedRecordDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedRecordDate
        {
            get
            {
                EFS_Date dt = null;
                if (recordDateAdjustmentSpecified)
                    dt = new EFS_Date(recordDateAdjustment.adjustedDate.Value);
                return dt;
            }
        }
        #endregion AdjustedRecordDate

        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.adjustedDate.DateValue
                };
                return dt;
            }
        }
        #endregion AdjustedStartPeriod
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get
            {
                return currency;
            }
        }
        #endregion Currency
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {
                return new EFS_EventDate(periodDatesAdjustment.adjustableDate2);

            }
        }
        #endregion EndPeriod
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {
                return new EFS_EventDate(paymentDateAdjustment);
            }
        }
        #endregion PaymentDate
        #region PeriodCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PeriodCurrency
        {
            get
            {
                if (null != this.calculationPeriods && (0 < this.calculationPeriods.Length))
                    return this.calculationPeriods[0].PeriodCurrency;
                return null;

            }
        }
        #endregion PeriodCurrency
        #region RateCutOff
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate RateCutOff
        {
            get
            {
                if (rateCutOffDateSpecified)
                    return new EFS_EventDate(rateCutOffDate.DateValue, rateCutOffDate.DateValue);
                else
                    return null;

            }
        }
        #endregion RateCutOff
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {
                return new EFS_EventDate(periodDatesAdjustment.adjustableDate1);
            }
        }
        #endregion StartPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;
            }
        }
        #endregion UnadjustedEndPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedStartPeriod
        #region UnadjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedPaymentDate
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = paymentDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedPaymentDate
        #region GetEventCodeTermination
        // 20071015 EG Ticket 15858
        // EG 20190823 [FIXEDINCOME] Upd (DebtSecurity perpetual)
        public static string GetEventCodeTermination(EFS_EventDate pTerminationDate, EFS_EventDate pMaxTerminationDate, bool pIsPerpetual)
        {
            if (pIsPerpetual || (0 != pMaxTerminationDate.unadjustedDate.DateValue.CompareTo(pTerminationDate.unadjustedDate.DateValue)))
                return EventCodeFunc.Intermediary;
            else 
                return EventCodeFunc.Termination;
        }
        #endregion GetEventCodeTermination

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_PaymentDate(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, pDataDocument);
            //stubSpecified = false;
            stub = StubEnum.None;
        }
        #endregion Constructors
    }
    #endregion EFS_PaymentDate
    #region EFS_PaymentDates
    /// <revision>
    ///     <version>1.1.8</version><date>20070823</date><author>EG</author>
    ///     <comment>
    ///     Apply Rules to determine the date of Pre-Settlement events
    ///     </comment>
    /// </revision>
    public class EFS_PaymentDates
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        [System.Xml.Serialization.XmlArrayItemAttribute("paymentDate")]
        public EFS_PaymentDate[] paymentDates;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_PaymentDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_PaymentDates(string pConnectionString, IPaymentDates pPaymentDates, ICalculationPeriodDates pCalculationPeriodDates, IResetDates pResetDates, IInterestRateStream pStream, DataDocumentContainer pDataDocument)
            :this(pConnectionString, pDataDocument)
        {
            Calc(pPaymentDates, pCalculationPeriodDates, pResetDates, pStream);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculate for a stream the payment dates and the unadjusted and adjusted related payment periods
        /// (the date adjustments being automatically calculated through the EFS_PaymentDates type).
        /// </summary>
        /// <param name="pPaymentDates">Stream PaymentDates element</param>
        /// <param name="pCalculationPeriodDates">Stream CalculationPeriodDates element</param>
        /// <param name="pResetDates">Stream ResetDates element</param>
        /// <returns></returns>
        /// <remarks>The calculation periods in the EFS_CalculationPeriodDates are supposed to be ordered by
        /// increasing period start dates.
        /// </remarks>
        /// 
        /// <revision>
        ///     <version>1.1.5</version><date>20070405</date><author>EG</author>
        ///     <EurosysSupport>N° 15422</EurosysSupport>
        ///     <comment>
        ///     Modification des paramètres passés à la fonction InsertArrayPaymentDates.
        ///     Le paramètre pCalculationPeriodDates (CalculationPeriodDates) est passé dans sa totalité et remplace donc les paramètres
        ///     pCalculationPeriodDatesAdjustments (BusinessDayAdjustments) et pCalculationPeriodDates (EFS_CalculationPeriodDates)
        ///		</comment>
        /// </revision>
        // 20090514 EG Multiplier = 0 (Frequency1 = Frequency2 = 0T)
        public Cst.ErrLevel Calc(IPaymentDates pPaymentDates, ICalculationPeriodDates pCalculationPeriodDates, IResetDates pResetDates)
        {
            return Calc(pPaymentDates, pCalculationPeriodDates, pResetDates, null);
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190823 [FIXEDINCOME] Upd (DebtSecurity perpetual)
        private Cst.ErrLevel Calc(IPaymentDates pPaymentDates, ICalculationPeriodDates pCalculationPeriodDates, IResetDates pResetDates, IInterestRateStream pStream)
        {


            // 20070823 EG Ticket : 15643
            #region PreSettlementInfo

            EFS_SettlementInfoEntity preSettlementInfo = null;
            if (null != pStream)
            {
                IBusinessDayAdjustments bda = pCalculationPeriodDates.CalculationPeriodDatesAdjustments;
                preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, pStream.StreamCurrency, pStream.PayerPartyReference.HRef, pStream.ReceiverPartyReference.HRef, m_DataDocument, bda.DefaultOffsetPreSettlement);
            }


            #endregion PreSettlementInfo

            #region Validation Rules & Multiplier Calculation
            // the payment frequency must be a multiple of the calculation period frequency
            int multiplier = Tools.CheckFrequency(pPaymentDates.PaymentFrequency, pCalculationPeriodDates.CalculationPeriodFrequency.Interval, out int remainder);

            Cst.ErrLevel ret;
            #endregion Validation Rules & Multiplier Calculation
            #region Process
            if (remainder == 0)
            {
                ret = Cst.ErrLevel.SUCCESS;
                #region private variables
                ArrayList aPaymentDates = new ArrayList();
                PayRelativeToEnum relativeTo;
                EFS_CalculationPeriodDates calculationPeriodDates = pCalculationPeriodDates.Efs_CalculationPeriodDates;
                DateTime endDate = calculationPeriodDates.calculationPeriods[0].periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue;
                DateTime rateCutOffDate = Convert.ToDateTime(null);
                DateTime terminationDate = calculationPeriodDates.terminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue;
                #endregion private variables
                #region Evaluate Payment RelativeTo
                if ((pPaymentDates.PayRelativeTo == PayRelativeToEnum.CalculationPeriodStartDate) ||
                    (pPaymentDates.PayRelativeTo == PayRelativeToEnum.ResetDate &&
                    pResetDates.ResetRelativeTo == ResetRelativeToEnum.CalculationPeriodStartDate))
                    relativeTo = PayRelativeToEnum.CalculationPeriodStartDate;
                else
                    relativeTo = PayRelativeToEnum.CalculationPeriodEndDate;
                #endregion Evaluate  Payment RelativeTo

                int i = 0;

                #region Initial stub payment dates calculation (firstRegularPeriodStartDateAdjustment)
                bool isFirstRegularPeriodStartDateAdjustment = calculationPeriodDates.firstRegularPeriodStartDateAdjustmentSpecified &&
                    (endDate < calculationPeriodDates.firstRegularPeriodStartDateAdjustment.AdjustableDate.UnadjustedDate.DateValue);

                StubEnum stub;
                DateTime startDate;
                DateTime payDate;
                //if (calculationPeriodDates.firstRegularPeriodStartDateAdjustmentSpecified)
                if (isFirstRegularPeriodStartDateAdjustment)
                {
                    stub = StubEnum.Initial;
                    startDate = endDate;
                    endDate = calculationPeriodDates.firstRegularPeriodStartDateAdjustment.AdjustableDate.UnadjustedDate.DateValue;
                    if (pPaymentDates.FirstPaymentDateSpecified)
                        payDate = pPaymentDates.FirstPaymentDate.DateValue;
                    else if (relativeTo == PayRelativeToEnum.CalculationPeriodStartDate)
                        payDate = startDate;
                    else
                        payDate = endDate;

                    if (pPaymentDates.PaymentDaysOffsetSpecified)
                        payDate = Tools.ApplyOffset(m_Cs, payDate, terminationDate, pPaymentDates.PaymentDaysOffset, pPaymentDates.PaymentDatesAdjustments, m_DataDocument);
                    if ((null != pResetDates) && pResetDates.RateCutOffDaysOffsetSpecified)
                        rateCutOffDate = Tools.ApplyOffset(m_Cs, endDate, terminationDate, pResetDates.RateCutOffDaysOffset, pResetDates.ResetDatesAdjustments, m_DataDocument);

                    // Insert PaymentDates
                    InsertArrayPaymentDates(ref aPaymentDates, startDate, endDate,
                        pCalculationPeriodDates, payDate, pPaymentDates.PaymentDatesAdjustments, rateCutOffDate, stub, preSettlementInfo);

                    if (multiplier > 0)
                    {
                        //PL 20120829 Si 1 Term alors multiplier=0. Cette valeur 0 est utilisée plus bas pour identifier ce cas, on ne modifie donc pas "i".
                        i++;
                    }
                }
                stub = StubEnum.None;
                #endregion Initial stub payment dates calculation
                #region Other Payment Date Calculation
                int guard = 0;
                while (true)
                {
                    if (endDate == terminationDate)
                    {
                        ret = Cst.ErrLevel.SUCCESS;
                        break;
                    }
                    startDate = endDate;
                    i += multiplier;
                    if (i > calculationPeriodDates.calculationPeriods.Length)
                        break;

                    // 20090514 EG Multiplier = 0 (Frequency1 = Frequency2 = 0T) //PL 20120829 0T ou 1T ??
                    if (0 == i)
                        endDate = terminationDate;
                    else
                        endDate = calculationPeriodDates.calculationPeriods[i - 1].periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;

                    #region Calculate the unadjusted payment Date
                    if (calculationPeriodDates.lastRegularPeriodEndDateAdjustmentSpecified &&
                        endDate == calculationPeriodDates.lastRegularPeriodEndDateAdjustment.AdjustableDate.UnadjustedDate.DateValue &&
                        pPaymentDates.LastRegularPaymentDateSpecified)
                        payDate = pPaymentDates.LastRegularPaymentDate.DateValue;
                    else if (relativeTo == PayRelativeToEnum.CalculationPeriodStartDate)
                        payDate = startDate;
                    else
                        payDate = endDate;

                    if (pPaymentDates.PaymentDaysOffsetSpecified)
                        payDate = Tools.ApplyOffset(m_Cs, payDate, terminationDate, pPaymentDates.PaymentDaysOffset, pPaymentDates.PaymentDatesAdjustments, m_DataDocument);
                    if ((null != pResetDates) && pResetDates.RateCutOffDaysOffsetSpecified)
                        rateCutOffDate = Tools.ApplyOffset(m_Cs, endDate, terminationDate, pResetDates.RateCutOffDaysOffset, pResetDates.ResetDatesAdjustments, m_DataDocument);

                    // Insert PaymentDates
                    InsertArrayPaymentDates(ref aPaymentDates, startDate, endDate,
                        pCalculationPeriodDates, payDate, pPaymentDates.PaymentDatesAdjustments, rateCutOffDate, stub, preSettlementInfo);

                    if (calculationPeriodDates.lastRegularPeriodEndDateAdjustmentSpecified &&
                        endDate == calculationPeriodDates.lastRegularPeriodEndDateAdjustment.AdjustableDate.UnadjustedDate.DateValue &&
                        pPaymentDates.LastRegularPaymentDateSpecified)
                        stub = StubEnum.Final;
                    #endregion Calculate the unadjusted payment Date

                    #region guard
                    guard++;
                    if (guard == 999)
                    {
                        //Loop parapet
                        string msgException = "Payment Dates exception:" + Cst.CrLf;
                        msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                        msgException += "Please, verify dates, periods, business day adjustment and roll convention on the trade";
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                    }
                    #endregion
                }
                #endregion Other Payment Date Calculation
                #region PaymentDate Storage (arrayList to array)
                if ((ret == Cst.ErrLevel.SUCCESS) && (aPaymentDates.Count > 0))
                    this.paymentDates = (EFS_PaymentDate[])aPaymentDates.ToArray(aPaymentDates[0].GetType());
                #endregion PaymentDate Storage (arrayList to array)
            }
            else
            {
                #region Error
                string paymentFrequency = pPaymentDates.PaymentFrequency.PeriodMultiplier.ToString() +
                    pPaymentDates.PaymentFrequency.Period.ToString();
                string calculationPeriodFrequency = pCalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.ToString() +
                    pCalculationPeriodDates.CalculationPeriodFrequency.Interval.Period.ToString();

                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "ERR: PaymentFrequency [{0}] isn't a multiple of the calculationperiodfrequency. [{1}]", paymentFrequency, calculationPeriodFrequency);
                #endregion Error
            }
            #endregion Process

            return ret;
        }

        #endregion Calc
        #region Insert in paymentDates
        /// <revision>
        ///     <version>1.1.5</version><date>20070405</date><author>EG</author>
        ///     <EurosysSupport>N° 15422</EurosysSupport>
        ///     <comment>
        ///     1./ Modification des paramètres passés à la fonction.
        ///         Le paramètre pCalculationPeriodDates (CalculationPeriodDates) est passé dans sa totalité et remplace donc les paramètres
        ///         pCalculationPeriodDatesAdjustments (BusinessDayAdjustments) et pCalculationPeriodDates (EFS_CalculationPeriodDates)
        ///         
        ///     2./ Si la date de fin de payment = la date de termination du trade alors le calcul et l'ajustement 
        ///         de la date de fin de payment s'opère avec TerminationDateAdjustment sion pas de changement  le calcul s'opère avec
        ///         CalculationPeriodDatesAdjustments.
        ///		</comment>
        /// </revision>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void InsertArrayPaymentDates(ref ArrayList pPaymentDates, DateTime pStartDate, DateTime pEndDate,
            ICalculationPeriodDates pCalculationPeriodDates, DateTime pPayDate, IBusinessDayAdjustments pPaymentDatesAdjustments,
            DateTime pRateCutOffDate, StubEnum pStub, EFS_SettlementInfoEntity pPreSettlementInfo)
        {

            EFS_CalculationPeriodDates efs_CalculationPeriodDates = pCalculationPeriodDates.Efs_CalculationPeriodDates;
            EFS_PaymentDate efs_PaymentDate = new EFS_PaymentDate(m_Cs, m_DataDocument);
            efs_PaymentDate.periodDatesAdjustment.adjustableDate1 = new EFS_AdjustableDate(m_Cs, pStartDate, pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);

            if (0 == pEndDate.CompareTo(pCalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue))
                efs_PaymentDate.periodDatesAdjustment.adjustableDate2 =
                    new EFS_AdjustableDate(m_Cs, pEndDate, efs_CalculationPeriodDates.terminationDateAdjustment.AdjustableDate.DateAdjustments, m_DataDocument);
            else
                efs_PaymentDate.periodDatesAdjustment.adjustableDate2 =
                    new EFS_AdjustableDate(m_Cs, pEndDate, pCalculationPeriodDates.CalculationPeriodDatesAdjustments, m_DataDocument);

            efs_PaymentDate.paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, pPayDate, pPaymentDatesAdjustments, m_DataDocument);
            efs_PaymentDate.rateCutOffDateSpecified = (pRateCutOffDate != Convert.ToDateTime(null));
            efs_PaymentDate.rateCutOffDate = pCalculationPeriodDates.CreateRequiredIdentifierDate(pRateCutOffDate);
            efs_PaymentDate.stubSpecified = (pStub != StubEnum.None);
            efs_PaymentDate.stub = pStub;


            // 20070823 EG Ticket : 15643
            #region PreSettlementDate
            efs_PaymentDate.preSettlementSpecified = (null != pPreSettlementInfo) && pPreSettlementInfo.IsUsePreSettlement;
            if (efs_PaymentDate.preSettlementSpecified)
                efs_PaymentDate.preSettlement = new EFS_PreSettlement(m_Cs, null, efs_PaymentDate.paymentDateAdjustment.AdjustedEventDate,
                    pPreSettlementInfo.Currency, pPreSettlementInfo.OffsetPreSettlement, m_DataDocument);
            #endregion PreSettlementDate


            #region Store CalculationPeriods for Current PaymentDate
            ArrayList aCalculationPeriod = new ArrayList();
            foreach (EFS_CalculationPeriod calculationPeriod in efs_CalculationPeriodDates.calculationPeriods)
            {
                DateTime startPeriod = calculationPeriod.periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue;
                DateTime endPeriod = calculationPeriod.periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;
                if ((1 > pStartDate.CompareTo(startPeriod)) && (0 <= pEndDate.CompareTo(endPeriod)))
                {
                    aCalculationPeriod.Add(calculationPeriod);
                }
            }
            if (0 < aCalculationPeriod.Count)
                efs_PaymentDate.calculationPeriods = (EFS_CalculationPeriod[])aCalculationPeriod.ToArray(aCalculationPeriod[0].GetType());
            pPaymentDates.Add(efs_PaymentDate);
            #endregion Store CalculationPeriods for Current PaymentDate

        }
        #endregion Insert in PaymentDates
        #region SetCurrencyPayment
        public void SetCurrencyPayment(ICalculationPeriodDates pCalculationPeriodDates)
        {

            // 20070130 RD Un test rajouté du à l'Erreur dans le cas EFFECTIVEDATE = TERMINATIONDATE
            if (this.paymentDates != null)
            {
                EFS_CalculationPeriodDates calculationPeriodDates = pCalculationPeriodDates.Efs_CalculationPeriodDates;
                foreach (EFS_PaymentDate paymentDate in this.paymentDates)
                {
                    DateTime startPayment = paymentDate.UnadjustedStartPeriod.DateValue;
                    DateTime endPayment = paymentDate.UnadjustedEndPeriod.DateValue;
                    if (null != calculationPeriodDates.fxLinkedNominalPeriods)
                    {
                        foreach (EFS_FxLinkedNominalPeriod nominalPeriod in calculationPeriodDates.fxLinkedNominalPeriods)
                        {
                            DateTime startPeriod = nominalPeriod.UnadjustedStartPeriod.DateValue;
                            DateTime endPeriod = nominalPeriod.UnadjustedEndPeriod.DateValue;
                            if ((0 <= startPayment.CompareTo(startPeriod)) && (0 >= endPayment.CompareTo(endPeriod)))
                            {
                                paymentDate.currency = nominalPeriod.currency;
                                break;
                            }
                        }
                    }
                    else if (null != calculationPeriodDates.nominalPeriods)
                    {
                        foreach (EFS_NominalPeriod nominalPeriod in calculationPeriodDates.nominalPeriods)
                        {
                            DateTime startPeriod = nominalPeriod.UnadjustedStartPeriod.DateValue;
                            DateTime endPeriod = nominalPeriod.UnadjustedEndPeriod.DateValue;
                            if ((0 <= startPayment.CompareTo(startPeriod)) && (0 >= endPayment.CompareTo(endPeriod)))
                            {
                                paymentDate.currency = nominalPeriod.currency;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (EFS_CalculationPeriod calculationPeriod in paymentDate.calculationPeriods)
                        {
                            paymentDate.currency = calculationPeriod.knownAmountPeriod.currency;
                            break;
                        }
                    }
                }
            }

        }
        #endregion SetCurrencyPayment
        #endregion Methods
    }
    #endregion EFS_PaymentDates

    #region EFS_RateIndex
    public class EFS_RateIndex
    {
        #region Members
        public int idAsset;
        public int idRx;
        public string identifier;
        public IOffset selfAverage;
        public IOffset selfReset;
        public string idBC;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stubSpecified;
        public StubEnum stub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isSelfCompounding;
        public int idAssetBasis;
        public int idRxBasis;
        private readonly IBusinessCenters m_BusinessCenters;
        #endregion Members
        #region Accessors
        #region BusinessCenters
        public IBusinessCenters BusinessCenters
        {
            get { return m_BusinessCenters; }
        }
        #endregion BusinessCenters
        #endregion Accessors
        #region Constructors
        public EFS_RateIndex()
        { }
        public EFS_RateIndex(IResetDates pResetDates, int pIdAsset, int pIdRx, string pIdentifier, string pIdBC, bool pIsSelfCompounding, StubEnum pStub)
        {
            idAsset = pIdAsset;
            idRx = pIdRx;
            identifier = pIdentifier;
            idBC = pIdBC;
            isSelfCompounding = pIsSelfCompounding;
            stubSpecified = (pStub != StubEnum.None);
            stub = pStub;
            IOffset offset = pResetDates.ResetDatesAdjustments.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.Calendar);
            m_BusinessCenters = (IBusinessCenters)offset.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE, pIdBC).BusinessCentersDefine;
        }
        public EFS_RateIndex(IResetDates pResetDates, int pIdAsset, int pIdRx, string pIdentifier, bool pIsSelfCompounding,
            string pPeriodSelfAvg, int pPeriodMultiplierSelfAvg, string pDaytypeSelfAvg,
            string pPeriodSelfReset, int pPeriodMultiplierSelfReset, string pDaytypeSelfReset,
            string pIdBC, int pIdAssetBasis, int pIdRxBasis, StubEnum pStub)
        {
            idAsset = pIdAsset;
            idRx = pIdRx;
            identifier = pIdentifier;
            isSelfCompounding = pIsSelfCompounding;
            selfAverage = pResetDates.ResetDatesAdjustments.CreateOffset(StringToEnum.Period(pPeriodSelfAvg), pPeriodMultiplierSelfAvg, StringToEnum.DayType(pDaytypeSelfAvg));
            selfReset = pResetDates.ResetDatesAdjustments.CreateOffset(StringToEnum.Period(pPeriodSelfReset), pPeriodMultiplierSelfReset, StringToEnum.DayType(pDaytypeSelfReset));
            m_BusinessCenters = (IBusinessCenters)selfAverage.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE, pIdBC).BusinessCentersDefine;
            idBC = pIdBC;
            idAssetBasis = pIdAssetBasis;
            idRxBasis = pIdRxBasis;
            stubSpecified = (pStub != StubEnum.None);
            stub = pStub;
        }
        #endregion Constructors
    }
    #endregion EFS_RateIndex

    #region EFS_ResetDate
    public class EFS_ResetDate
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate resetDateAdjustment;
        public EFS_AdjustableDate fixingDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stubSpecified;
        public StubEnum stub;
        [System.Xml.Serialization.XmlArrayItemAttribute("selfAverageDate")]
        public EFS_SelfAverageDate[] selfAverageDates;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ResetDate(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            resetDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DataDocument);
            fixingDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DataDocument);
            //stubSpecified = false;
            stub = StubEnum.None;
        }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ResetDate(string pConnectionString, DateTime pStartDate, DateTime pEndDate,
            IBusinessDayAdjustments pCalculationPeriodDatesAdjustments,
            DateTime pResetDate, IBusinessDayAdjustments pResetDatesAdjustments,
            DateTime pFixingDate, IBusinessDayAdjustments pFixingDatesAdjustments, StubEnum pStub, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDatesAdjustment = new EFS_AdjustablePeriod
            {
                adjustableDate1 = new EFS_AdjustableDate(m_Cs, pStartDate, pCalculationPeriodDatesAdjustments, m_DataDocument),
                adjustableDate2 = new EFS_AdjustableDate(m_Cs, pEndDate, pCalculationPeriodDatesAdjustments, m_DataDocument)
            };
            resetDateAdjustment = new EFS_AdjustableDate(m_Cs, pResetDate, pResetDatesAdjustments, m_DataDocument);
            fixingDateAdjustment = new EFS_AdjustableDate(m_Cs, pFixingDate, pFixingDatesAdjustments, m_DataDocument);
            stubSpecified = (pStub != StubEnum.None);
            stub = pStub;
        }
        #endregion Constructors
        #region Accessors
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedEndPeriod
        #region AdjustedFixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedFixingDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = fixingDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedFixingDate
        #region AdjustedResetDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedResetDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = resetDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedResetDate
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedStartPeriod
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(periodDatesAdjustment.adjustableDate2);

            }
        }
        #endregion EndPeriod
        #region FixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate FixingDate
        {
            get
            {

                return new EFS_EventDate(fixingDateAdjustment);

            }
        }
        #endregion FixingDate
        #region ResetDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ResetDate
        {
            get
            {

                return new EFS_EventDate(resetDateAdjustment);

            }
        }
        #endregion ResetDate
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDatesAdjustment.adjustableDate1);

            }
        }
        #endregion StartPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedEndPeriod
        #region UnadjustedFixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedFixingDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = fixingDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedFixingDate
        #region UnadjustedResetDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedResetDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = resetDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedResetDate
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedStartPeriod
        #endregion Accessors
        #region Methods
        #region AssetRateIndex
        // EG 20140904 Add AssetCategory
        public object AssetRateIndex(object pCalculationPeriodAmount, StubEnum pStub, string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {
            EFS_Asset asset = null;
            object rate = Rate(pCalculationPeriodAmount, pStub, pRateType, pRate, pRateInitialStub, pRateFinalStub);
            if (null != rate)
            {
                asset = new EFS_Asset
                {
                    idAsset = ((EFS_Integer)rate).IntValue,
                    assetCategory = Cst.UnderlyingAsset.RateIndex,
                    time = fixingDateAdjustment.AdjustedEventDate.DateValue
                };
            }
            return asset;

        }
        #endregion AssetRateIndex
        #region Rate
        /// <revision>
        ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
        ///     <comment>Ticket 15889
        ///     Step dates: Unajusted versus Ajusted
        ///     Pass parameter m_Cs to EFS_CalculationPeriod
        ///     </comment>
        /// </revision>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public object Rate(object pCalculationPeriodAmount, StubEnum pStub, string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {

            EFS_CalculationPeriod calculationPeriod = new EFS_CalculationPeriod(m_Cs, null, m_DataDocument)
            {
                stubSpecified = (StubEnum.None != stub),
                stub = stub
            };
            return calculationPeriod.Rate(pCalculationPeriodAmount, pRateType, pRate, pRateInitialStub, pRateFinalStub);

        }
        #endregion Rate
        #endregion Methods
    }
    #endregion EFS_ResetDate
    #region EFS_ResetDates
    public class EFS_ResetDates
    {
        #region Members
        private readonly string m_Cs;
        [System.Xml.Serialization.XmlArrayItemAttribute("resetDate")]
        public EFS_ResetDate[] resetDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private EFS_RateIndex[] rateIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private readonly DataDocumentContainer m_DataDocument;

        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ResetDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ResetDates(string pConnectionString, IResetDates pResetDates, ICalculationPeriodDates pCalculationPeriodDates, int[] pIdAsset, DataDocumentContainer pDataDocument)
            :this(pConnectionString, pDataDocument)
        {
            InitEFS_RateIndex(pResetDates, pIdAsset);
            Calc(pResetDates, pCalculationPeriodDates);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// <summary>
        /// Calculate for a stream the unadjusted and adjusted reset periods, reset dates and fixing dates
        /// </summary>
        /// <param name="pResetDates">Stream ResetDates element</param>
        /// <param name="pCalculationPeriodDates">Stream CalculationPeriodDates element</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>The calculation periods in the EFS_CalculationPeriodDates are supposed to be ordered by increasing period start dates</para>
        /// <para>The date adjustments being automatically calculated through the EFS_ResetDates type</para>
        /// </remarks>
        /// <exception cref="SpheresException2 if incoherence is find during the calculation"></exception>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        public void Calc(IResetDates pResetDates, ICalculationPeriodDates pCalculationPeriodDates)
        {

            #region Validation Rules
            // the reset frequency must be a multiple of the calculation period frequency
            Tools.CheckFrequency(pCalculationPeriodDates.CalculationPeriodFrequency.Interval, pResetDates.ResetFrequency.Interval, out int remainder);
            #endregion Validation Rules
            //
            if (false == (remainder == 0))
            {
                string resetFrequency = pResetDates.ResetFrequency.Interval.PeriodMultiplier.ToString() +
                    pResetDates.ResetFrequency.Interval.Period.ToString();
                string calculationPeriodFrequency = pCalculationPeriodDates.CalculationPeriodFrequency.Interval.PeriodMultiplier.ToString() +
                    pCalculationPeriodDates.CalculationPeriodFrequency.Interval.Period.ToString();
                //
                throw new Exception(StrFunc.AppendFormat("CalculationPeriodFrequency [{0}] isn't a multiple of the ResetFrequency. [{1}]", calculationPeriodFrequency, resetFrequency));
            }
            #region Process
            #region private variables
            ArrayList aResetDates = new ArrayList();
            ArrayList aResetDatesByCalculationPeriods = new ArrayList();
            EFS_CalculationPeriodDates calculationPeriodDates = pCalculationPeriodDates.Efs_CalculationPeriodDates;
            DateTime endDate = calculationPeriodDates.calculationPeriods[0].periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue;
            DateTime terminationDate = calculationPeriodDates.terminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue;
            #endregion private variables
            #region ResetDate Calculation foreach calculationPeriod
            foreach (EFS_CalculationPeriod calculationPeriod in calculationPeriodDates.calculationPeriods)
            {
                DateTime startPeriod = calculationPeriod.periodDates.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue;
                DateTime endPeriod = calculationPeriod.periodDates.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;
                DateTime startDate = startPeriod;
                int guard = 0;
                while (true)
                {
                    if ((endDate == terminationDate) || (endDate == endPeriod))
                        break;
                    //
                    if ((StubEnum.None != calculationPeriod.stub) && (RateTypeEnum.FloatingRate != calculationPeriod.stubRateType))
                        break;
                    //

                    #region Calculate the unadjusted reset period dates
                    if (calculationPeriod.stubSpecified)
                    {
                        endDate = endPeriod;
                    }
                    else
                    {
                        endDate = Tools.ApplyInterval(startDate, endPeriod, pResetDates.ResetFrequency.Interval);
                        RollConventionEnum rollConventionEnum = pCalculationPeriodDates.CalculationPeriodFrequency.RollConvention;
                        EFS_RollConvention rollConvention = EFS_RollConvention.GetNewRollConvention(rollConventionEnum, endDate, default);
                        endDate = rollConvention.rolledDate;
                    }

                    DateTime resetDate;
                    #endregion Calculate the unadjusted reset period dates

                    #region Calculate the unadjusted reset date (start or end,roll convention)
                    if ((pResetDates.ResetRelativeToSpecified) &&
                        (pResetDates.ResetRelativeTo == ResetRelativeToEnum.CalculationPeriodStartDate))
                        resetDate = startDate;
                    else
                        resetDate = endDate;
                    if (pResetDates.ResetFrequency.WeeklyRollConventionSpecified && guard == 0)
                    {
                        resetDate = Tools.ApplyWeeklyRollConvention(resetDate, pResetDates.ResetFrequency.WeeklyRollConvention);
                    }
                    #endregion Calculate the unadjusted reset date (start or end,roll convention)

                    // 20071018 EG resetDate Ticket 15866 : fixingDates calculation relative to resetDates (adjusted)
                    #region Calculate the adjusted reset dates to determine the fixing date relative
                    EFS_AdjustableDate adjustableResetDate = new EFS_AdjustableDate(m_Cs, resetDate, pResetDates.ResetDatesAdjustments, m_DataDocument);
                    #endregion Calculate the adjusted reset dates to determine the fixing date relative

                    #region Calculate the unadjusted fixing dates (initial fixing, offset)
                    IBusinessDayAdjustments fixingDateBusinessDayAdjustments = null;
                    DateTime fixingDate;
                    if (guard == 0 && pResetDates.InitialFixingDateSpecified)
                    {
                        fixingDateBusinessDayAdjustments = pResetDates.InitialFixingDate.GetAdjustments;
                        // 20071018 EG Ticket 15866 : Ticket 15866 : resetDate is now adjusted
                        fixingDate = Tools.ApplyOffset(m_Cs, adjustableResetDate.adjustedDate.DateValue, terminationDate, pResetDates.InitialFixingDate.GetOffset, fixingDateBusinessDayAdjustments, m_DataDocument);
                    }
                    else
                    {
                        fixingDateBusinessDayAdjustments = pResetDates.FixingDates.GetAdjustments;
                        // 20071018 EG Ticket 15866 : resetDate is now adjusted
                        fixingDate = Tools.ApplyOffset(m_Cs, adjustableResetDate.adjustedDate.DateValue, terminationDate, pResetDates.FixingDates.GetOffset, fixingDateBusinessDayAdjustments, m_DataDocument);
                    }
                    #endregion Calculate the unadjusted fixing dates (initial fixing, offset)

                    EFS_ResetDate efs_ResetDate = new EFS_ResetDate(m_Cs, startDate, endDate,
                        pCalculationPeriodDates.CalculationPeriodDatesAdjustments,
                        resetDate, pResetDates.ResetDatesAdjustments, fixingDate, fixingDateBusinessDayAdjustments,
                        calculationPeriod.stub, m_DataDocument);

                    #region Call SelfAverageDate
                    int itemRate = 0;
                    if (StubEnum.Initial == calculationPeriod.stub)
                        itemRate = 1;
                    else if (StubEnum.Final == calculationPeriod.stub)
                        itemRate = 3;

                    if (null != rateIndex[itemRate] && ((EFS_RateIndex)rateIndex[itemRate]).isSelfCompounding)
                        efs_ResetDate.selfAverageDates = (EFS_SelfAverageDate[])CalcSelfAverage(startDate, endDate, pResetDates.ResetFrequency.Interval, rateIndex[itemRate], pResetDates.FixingDates);
                    #endregion Call SelfAverageDate

                    aResetDatesByCalculationPeriods.Add(efs_ResetDate);
                    aResetDates.Add(efs_ResetDate);
                    startDate = endDate;
                    //
                    guard++;
                    //FI 20100629 [17072] [Ajout du test sur endDate > terminationDate)
                    //En effet le traitement peut être excessivement long avant même d'arriver à 999
                    //Les utilisateurs n'ont alors plus qu'une solution => arrêter le service  
                    if (guard == 999 || endDate > terminationDate)
                    {
                        //Loop parapet
                        string msgException = "Reset Dates exception:" + Cst.CrLf;
                        msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                        msgException += "Please, verify dates, periods, business day adjustment and roll convention on the trade";
                        throw new Exception(msgException);
                    }
                }
                //Store ResetDates to efs_CalculationPeriodDates.calculationPeriods
                if (aResetDatesByCalculationPeriods.Count > 0)
                {
                    calculationPeriod.resetDates = (EFS_ResetDate[])
                        aResetDatesByCalculationPeriods.ToArray(aResetDatesByCalculationPeriods[0].GetType());
                }
                aResetDatesByCalculationPeriods.Clear();
            }
            #endregion Reset Date Calculation foreach calculationPeriod
            //
            //ResetDate Storage (arrayList to array)
            if (aResetDates.Count > 0)
                this.resetDates = (EFS_ResetDate[])aResetDates.ToArray(aResetDates[0].GetType());
            //
            #endregion Process

        }
        #endregion Calc
        #region CalcSelfAverage
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        private Array CalcSelfAverage(DateTime pStartDate, DateTime pEndDate, IInterval pIntervalReset, EFS_RateIndex pRateIndex, IRelativeDateOffset pLookBackOffset)
        {
            #region Validation Rules
            IInterval intervalSelfAvg = pRateIndex.selfAverage.GetInterval(pRateIndex.selfAverage.PeriodMultiplier.IntValue, pRateIndex.selfAverage.Period);
            //int multiplier = Tools.CheckFrequency(pIntervalReset, intervalSelfAvg, out remainder);
            Tools.CheckFrequency(pIntervalReset, intervalSelfAvg, out int remainder);
            #endregion Validation Rules
            #region Process
            if (remainder == 0)
            {
                #region private variables
                ArrayList aSelfAverageDates = new ArrayList();
                DateTime endDate = pStartDate;
                #endregion private variables

                #region SelfAverage Date Calculation
                int guard = 0;
                while (true)
                {
                    guard++;
                    if (guard == 999)
                        throw new Exception("ERR: SelfAverageDates Loop");

                    if (endDate == pEndDate)
                        break;

                    DateTime startDate = endDate;
                    endDate = Tools.ApplyOffset(m_Cs, startDate, pEndDate, pRateIndex.selfAverage, pRateIndex.BusinessCenters);
                    if (endDate > pEndDate)
                        endDate = pEndDate;
                    DateTime selfAverageDate = endDate;

                    EFS_SelfAverageDate efs_SelfAverageDate = new EFS_SelfAverageDate(m_Cs, startDate, endDate, selfAverageDate, pRateIndex, m_DataDocument)
                    {
                        selfResetDates = (EFS_SelfResetDate[])CalcSelfReset(startDate, endDate, pRateIndex, pLookBackOffset)
                    };

                    aSelfAverageDates.Add(efs_SelfAverageDate);
                }
                if (aSelfAverageDates.Count > 0)
                    return aSelfAverageDates.ToArray(aSelfAverageDates[0].GetType());
                #endregion SelfAverage Date Calculation
            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                    "ResetFrequency [{0}] isn't a multiple of the SelfAverage. [{1}]",
                    pIntervalReset.PeriodMultiplier.ToString() + pIntervalReset.Period.ToString(),
                    pRateIndex.selfAverage.PeriodMultiplier.ToString() + pRateIndex.selfAverage.Period.ToString());
            }
            return null;
            #endregion Process

        }
        #endregion CalcSelfAverage
        #region CalcSelfReset
        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        private Array CalcSelfReset(DateTime pStartDate, DateTime pEndDate, EFS_RateIndex pRateIndex, IRelativeDateOffset pLookBackOffset)
        {

            #region Validation Rules
            IInterval intervalSelfAvg = pRateIndex.selfAverage.GetInterval(pRateIndex.selfAverage.PeriodMultiplier.IntValue, pRateIndex.selfAverage.Period);
            IInterval intervalSelfReset = pRateIndex.selfReset.GetInterval(pRateIndex.selfReset.PeriodMultiplier.IntValue, pRateIndex.selfReset.Period);
            Tools.CheckFrequency(intervalSelfAvg, intervalSelfReset, out int remainder);
            #endregion Validation Rules
            #region Process
            if (remainder == 0)
            {
                #region private variables
                ArrayList aSelfResetDates = new ArrayList();
                DateTime endDate = pStartDate;
                #endregion private variables

                #region SelfReset Date Calculation
                int guard = 0;
                while (true)
                {
                    guard++;
                    if (guard == 999)
                    {
                        //Loop parapet
                        string msgException = "Self-Reset Dates exception:" + Cst.CrLf;
                        msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                        msgException += "Please, verify dates, periods, business day adjustment and roll convention on the trade";
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                    }

                    if (endDate == pEndDate) break;

                    DateTime startDate = endDate;
                    endDate = Tools.ApplyOffset(m_Cs, startDate, pEndDate, pRateIndex.selfReset, pRateIndex.BusinessCenters);
                    if (endDate > pEndDate)
                        endDate = pEndDate;

                    DateTime selfResetDate = startDate;
                    if (null != pLookBackOffset)
                        selfResetDate = Tools.ApplyOffset(m_Cs, selfResetDate, endDate, pLookBackOffset.GetOffset, pLookBackOffset.GetAdjustments, m_DataDocument);
                    aSelfResetDates.Add(new EFS_SelfResetDate(m_Cs, startDate, endDate, selfResetDate, pRateIndex));
                }
                if (aSelfResetDates.Count > 0)
                    return aSelfResetDates.ToArray(aSelfResetDates[0].GetType());
                #endregion SelfReset Date Calculation
            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                    "SelfAverage [{0}] isn't a multiple of the SelftReset. [{1}]",
                    pRateIndex.selfAverage.PeriodMultiplier.ToString() + pRateIndex.selfAverage.Period.ToString(),
                    pRateIndex.selfReset.PeriodMultiplier.ToString() + pRateIndex.selfReset.Period.ToString());
            }
            return null;

            #endregion Process
        }
        #endregion CalcSelfReset
        #region StoreIndexes
        private void InitEFS_RateIndex(IResetDates pResetDates, int[] pIdAsset)
        {
            SQL_AssetRateIndex sql_AssetRateIndex;
            if (0 < pIdAsset.Length)
            {
                rateIndex = new EFS_RateIndex[pIdAsset.Length];
                for (int i = 0; i < pIdAsset.Length; i++)
                {
                    if (0 != pIdAsset[i])
                    {
                        sql_AssetRateIndex = new SQL_AssetRateIndex(m_Cs, SQL_AssetRateIndex.IDType.IDASSET, pIdAsset[i])
                        {
                            WithInfoSelfCompounding = Cst.IndexSelfCompounding.CASHFLOW
                        };
                        if (sql_AssetRateIndex.IsLoaded)
                        {
                            string identifier = sql_AssetRateIndex.Identifier;
                            string idBC = sql_AssetRateIndex.Idx_IdBc;
                            DataRow dr = sql_AssetRateIndex.FirstRow;
                            int idRx = Convert.ToInt32(dr["Idx_IDRX"]);
                            if (sql_AssetRateIndex.Idx_IsSelfCompounding)
                            {
                                string periodSelfAvg = (dr["PERIODSELFAVG"] is DBNull) ?
                                    PeriodEnum.Y.ToString() : dr["PERIODSELFAVG"].ToString();
                                int periodMltpSelftAvg = (dr["PERIODMLTPSELFAVG"] is DBNull) ? 0 : Convert.ToInt32(dr["PERIODMLTPSELFAVG"]);
                                string dayTypeSelfAvg = (dr["DAYTYPESELFAVG"] is DBNull) ?
                                    DayTypeEnum.Calendar.ToString() : dr["DAYTYPESELFAVG"].ToString();
                                string periodSelfReset = (dr["PERIODSELFRESET"] is DBNull) ?
                                    PeriodEnum.Y.ToString() : dr["PERIODSELFRESET"].ToString();
                                int periodMltpSelfReset = (dr["PERIODMLTPSELFRESET"] is DBNull) ?
                                    0 : Convert.ToInt32(dr["PERIODMLTPSELFRESET"]);
                                string dayTypeSelfReset = (dr["DAYTYPESELFRESET"] is DBNull) ?
                                    DayTypeEnum.Calendar.ToString() : dr["DAYTYPESELFRESET"].ToString();

                                int idRxBasis = (dr["IDRX_BASIS"] is DBNull) ? 0 : Convert.ToInt32(dr["IDRX_BASIS"]);

                                SQL_AssetRateIndex sql_AssetRateIndexBasis = new SQL_AssetRateIndex(m_Cs, SQL_AssetRateIndex.IDType.IDRX, idRxBasis);
                                //SQL_RateIndex sql_RateIndex = new SQL_RateIndex(m_Cs, idRxBasis);
                                if (sql_AssetRateIndexBasis.IsLoaded)
                                {
                                    rateIndex[i] = new EFS_RateIndex(pResetDates, pIdAsset[i], idRx, identifier, true,
                                        periodSelfAvg, periodMltpSelftAvg, dayTypeSelfAvg,
                                        periodSelfReset, periodMltpSelfReset, dayTypeSelfReset,
                                        idBC, sql_AssetRateIndexBasis.IdAsset, sql_AssetRateIndexBasis.IdRX,
                                        (StubEnum)System.Enum.ToObject(typeof(StubEnum), i));
                                }
                            }
                            else
                                rateIndex[i] = new EFS_RateIndex(pResetDates, pIdAsset[i], idRx, identifier, idBC,
                                    sql_AssetRateIndex.Idx_IsSelfCompounding,
                                    (StubEnum)System.Enum.ToObject(typeof(StubEnum), i));
                        }
                    }
                }
            }

        }
        #endregion InitEFS_Indexes
        #endregion Methods
    }
    #endregion EFS_ResetDates

    #region EFS_SelfAverageDate
    public class EFS_SelfAverageDate
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate selfDateAdjustment;
        [System.Xml.Serialization.XmlArrayItemAttribute("selfResetDate")]
        public EFS_SelfResetDate[] selfResetDates;
        public int idAsset;
        public int idRx;
        public int idAssetBasis;
        public int idRxBasis;
        #endregion Members
        #region Accessors
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedEndPeriod
        #region AdjustedSelfDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedSelfDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = selfDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedSelfDate
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedStartPeriod
        #region AssetBasisRateIndex
        // EG 20140904 Add AssetCategory
        public object AssetBasisRateIndex
        {
            get
            {
                EFS_Asset asset = new EFS_Asset
                {
                    idAsset = idAssetBasis,
                    assetCategory = Cst.UnderlyingAsset.RateIndex
                };
                return asset;
            }
        }
        #endregion AssetBasisRateIndex
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(periodDatesAdjustment.adjustableDate2);

            }
        }
        #endregion EndPeriod
        #region SelfDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate SelfDate
        {
            get
            {

                return new EFS_EventDate(selfDateAdjustment);

            }
        }
        #endregion SelfDate
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDatesAdjustment.adjustableDate1);

            }
        }
        #endregion StartPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedEndPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedStartPeriod
        #region UnadjustedSelfDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedSelfDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = selfDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedSelfDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SelfAverageDate(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            selfDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DataDocument);
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SelfAverageDate(string pConnectionString, DateTime pStartDate, DateTime pEndDate, DateTime pSelfDate,
            EFS_RateIndex pRateIndex, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            IBusinessDayAdjustments bda = pRateIndex.selfAverage.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE);
            periodDatesAdjustment.adjustableDate1 = new EFS_AdjustableDate(m_Cs, pStartDate, bda, m_DataDocument);
            periodDatesAdjustment.adjustableDate2 = new EFS_AdjustableDate(m_Cs, pEndDate, bda, m_DataDocument);
            selfDateAdjustment = new EFS_AdjustableDate(m_Cs, pSelfDate, bda, m_DataDocument);
            idAsset = pRateIndex.idAsset;
            idRx = pRateIndex.idRx;
            idAssetBasis = pRateIndex.idAssetBasis;
            idRxBasis = pRateIndex.idRxBasis;
        }
        #endregion Constructors
    }
    #endregion EFS_SelfAverageDate
    #region EFS_SelfResetDate
    public class EFS_SelfResetDate
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate selfDateAdjustment;
        public int idAsset;
        public int idRx;
        #endregion Members
        #region Accessors
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.adjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion AdjustedEndPeriod
        #region AdjustedSelfDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedSelfDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = selfDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion AdjustedSelfDate
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.adjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion AdjustedStartPeriod
        #region AssetBasisRateIndex
        // EG 20140904 Add AssetCategory
        public object AssetBasisRateIndex
        {
            get
            {
                EFS_Asset asset = new EFS_Asset
                {
                    idAsset = idAsset,
                    assetCategory = Cst.UnderlyingAsset.RateIndex,
                    time = selfDateAdjustment.AdjustedEventDate.DateValue
                };

                return asset;
            }
        }
        #endregion AssetBasisRateIndex
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(periodDatesAdjustment.adjustableDate2);

            }
        }

        #endregion EndPeriod
        #region SelfDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate SelfDate
        {
            get
            {

                return new EFS_EventDate(selfDateAdjustment);

            }
        }
        #endregion SelfDate
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(periodDatesAdjustment.adjustableDate1);

            }
        }

        #endregion StartPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion UnadjustedEndPeriod
        #region UnadjustedSelfDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedSelfDate
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = selfDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion UnadjustedSelfDate
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = periodDatesAdjustment.adjustableDate1.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }

        #endregion UnadjustedStartPeriod
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SelfResetDate(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            selfDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DataDocument);
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SelfResetDate(string pConnectionString, DateTime pStartDate, DateTime pEndDate, DateTime pSelfDate, EFS_RateIndex pRateIndex)
        {
            m_Cs = pConnectionString;
            periodDatesAdjustment = new EFS_AdjustablePeriod();
            IBusinessDayAdjustments bda = pRateIndex.selfAverage.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE);
            periodDatesAdjustment.adjustableDate1 = new EFS_AdjustableDate(m_Cs, pStartDate, bda, m_DataDocument);
            periodDatesAdjustment.adjustableDate2 = new EFS_AdjustableDate(m_Cs, pEndDate, bda, m_DataDocument);
            selfDateAdjustment = new EFS_AdjustableDate(m_Cs, pSelfDate, bda, m_DataDocument);
            idAsset = pRateIndex.idAssetBasis;
            idRx = pRateIndex.idRxBasis;
        }
        #endregion Constructors
    }
    #endregion EFS_SelfResetDate
    #region EFS_SwaptionDates
    public class EFS_SwaptionDates
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;

        public EFS_AdjustableDate commencementDateAdjustment;
        public EFS_AdjustableDate expirationDateAdjustment;
        public EFS_Payment[] premiumDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ExerciseStyleEnum exerciseStyle;
        #endregion Members
        #region Accessors
        #region AdjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = commencementDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedStartPeriod
        #region UnadjustedStartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedStartPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = commencementDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedStartPeriod
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {

                return new EFS_EventDate(commencementDateAdjustment);

            }
        }
        #endregion StartPeriod
        #region AdjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = expirationDateAdjustment.adjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion AdjustedEndPeriod
        #region UnadjustedEndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedEndPeriod
        {
            get
            {

                EFS_Date dt = new EFS_Date
                {
                    DateValue = expirationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;

            }
        }
        #endregion UnadjustedEndPeriod
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get
            {

                return new EFS_EventDate(expirationDateAdjustment);

            }
        }
        #endregion EndPeriod
        #region ExerciseType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExerciseType
        {
            get
            {
                return exerciseStyle.ToString();

            }
        }
        #endregion ExerciseType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SwaptionDates(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_SwaptionDates(string pConnectionString, ISwaption pSwaption, DataDocumentContainer pDataDocument)
            :this(pConnectionString, pDataDocument)
        {
            Pair<EFS_AdjustableDate, EFS_AdjustableDate> exerciseDateRange = null;
            exerciseStyle = pSwaption.GetStyle;
            switch (exerciseStyle)
            {
                case ExerciseStyleEnum.American:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, pSwaption.ExerciseAmerican, m_DataDocument);
                    break;
                case ExerciseStyleEnum.Bermuda:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, pSwaption.ExerciseBermuda, m_DataDocument);
                    break;
                case ExerciseStyleEnum.European:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, pSwaption.ExerciseEuropean, m_DataDocument);
                    break;
            }
            commencementDateAdjustment = exerciseDateRange.First;
            expirationDateAdjustment = exerciseDateRange.Second;
        }
        #endregion Constructors
    }
    #endregion EFS_SwaptionDates
}

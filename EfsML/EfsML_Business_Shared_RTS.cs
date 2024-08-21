#region Using Directives
using System;
using System.Collections.Generic;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;

using EFS.GUI.Interface;

using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;

using FixML.Enum;
using FixML.Interface;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_InterestLeg
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis 
    public class EFS_InterestLeg : EFS_ReturnSwapLeg
    {
        #region Members
        public EFS_AdjustableDate initialStartDate;
        public bool initialStartDateSpecified;
        public EFS_AdjustableDate initialEndDate;
        public bool initialEndDateSpecified;
        public EFS_AdjustableDate finalStartDate;
        public bool finalStartDateSpecified;
        public EFS_AdjustableDate finalEndDate;
        public bool finalEndDateSpecified;
        public RateTypeEnum initialStubRateType;
        public bool initialStubRateTypeSpecified;
        public RateTypeEnum finalStubRateType;
        public bool finalStubRateTypeSpecified;

        public EFS_InterestLegPaymentDate[] paymentPeriods;

        private EFS_Asset m_Asset;
        private List<EFS_InterestLegPaymentDate> _lstPaymentPeriods;
        private List<EFS_InterestLegResetDate> _lstResetPeriods;
        // EG 20231127 [WI755] Implementation Return Swap : New
        public string paymentCurrency;
        // EG 20231127 [WI755] Implementation Return Swap : New
        public bool IsFunding { set; get; }
        #endregion Members
        #region Accessors
        #region Asset
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset Asset
        {
            get { return m_Asset; }
            set { m_Asset = value; }
        }
        #endregion Asset
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InterestLeg(string pCS, DataDocumentContainer pDataDocument) : base(pCS, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InterestLeg(string pCS, Pair<IInterestLeg, IInterestCalculation> pInterestLegInfo, DataDocumentContainer pDataDocument)
            : base(pCS, (IReturnSwapLeg)pInterestLegInfo.First, pInterestLegInfo.First.Notional, pDataDocument)
        {
            Calc(pInterestLegInfo);
        }
        #endregion Constructors
        #region Methods
        #region CalcDailyPeriod

        /// <summary>
        /// terminationDateAdjustment == effectiveDateAdjustment
        /// </summary>
        /// <param name="pInterestLegInfo"></param>
        /// <returns></returns>
        /// FI 20141215 [20570] Rename en CalcDailyPeriod
        public Cst.ErrLevel CalcDailyPeriod(Pair<IInterestLeg, IInterestCalculation> pInterestLegInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            
            IInterestLeg _leg = pInterestLegInfo.First;
            IAdjustableRelativeOrPeriodicDates2 _paymentDates = _leg.CalculationPeriodDates.PaymentDates;
            
            // Calcul de la périodes 
            #region EffectiveDate & TerminationDate
            EFS_AdjustableDate _daily = Tools.GetEFS_AdjustableDate(m_Cs, _leg.CalculationPeriodDates.EffectiveDate, m_DataDocument);
            effectiveDateAdjustment = _daily;
            terminationDateAdjustment = _daily;
            #endregion EffectiveDate & TerminationDate

            _lstPaymentPeriods = new List<EFS_InterestLegPaymentDate>();

            #region EFS_Asset
            if ((null != pInterestLegInfo.Second) && pInterestLegInfo.Second.SqlAssetSpecified)
            {
                SQL_AssetBase _asset = pInterestLegInfo.Second.SqlAsset;
                m_Asset = new EFS_Asset
                {
                    idAsset = _asset.Id,
                    idC = _asset.IdC,
                    IdMarket = _asset.IdM,
                    description = _asset.Description,
                    assetCategory = _asset.AssetCategory
                };
                if (_asset is SQL_AssetRateIndex)
                {
                    SQL_AssetRateIndex _assetRateIndex = _asset as SQL_AssetRateIndex;
                    m_Asset.idBC = _assetRateIndex.Idx_IdBc;
                }
            }
            #endregion EFS_Asset


            #region PaymentDates
            if (_paymentDates.RelativeDatesSpecified)
                ret = CalculationPeriod(_lstPaymentPeriods, _paymentDates.RelativeDates);
            else if (_paymentDates.PeriodicDatesSpecified)
                ret = CalculationPeriod(_lstPaymentPeriods, _paymentDates.PeriodicDates);
            #endregion PaymentDates

            #region InterestLegResetDates calculation
            // Calcul des périodes de reset
            if (Cst.ErrLevel.SUCCESS == ret)
                ret = CalcResetDates(pInterestLegInfo);
            #endregion InterestLegResetDates calculation

            if ((Cst.ErrLevel.SUCCESS == ret) && (0 < _lstPaymentPeriods.Count))
                paymentPeriods = _lstPaymentPeriods.ToArray();

            ret = SetMultiplierAndSpreadValue(pInterestLegInfo);

            return ret;
        }
        #endregion CalcDailyPeriod


        #region AddToCalculationPeriod
        protected override void AddToCalculationPeriod<T>(List<T> pList, EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate)
        {
            AddToCalculationPeriod(pList, pStartDate, pEndDate, StubEnum.None, RateTypeEnum.None);
        }
        protected override void AddToCalculationPeriod<T>(List<T> pList, EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate, StubEnum pStub, RateTypeEnum pStubRateType)
        {
            if (pList is List<EFS_InterestLegPaymentDate>)
            {
                if (!(pList is List<EFS_InterestLegPaymentDate> _lst))
                    _lst = new List<EFS_InterestLegPaymentDate>();
                _lst.Add(new EFS_InterestLegPaymentDate(pStartDate, pEndDate, pStub, pStubRateType));
            }
        }
        #endregion AddToCalculationPeriod
        #region AddToResetPeriod
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected void AddToResetPeriod(DateTime pStartDate, DateTime pEndDate, IBusinessDayAdjustments pCalculationPeriodBDA, 
            DateTime pResetDate, DateTime pFixingDate, IBusinessDayAdjustments pFixingBDA, StubEnum pStub)
        {
            if (null == _lstResetPeriods)
                _lstResetPeriods = new List<EFS_InterestLegResetDate>();
            _lstResetPeriods.Add(new EFS_InterestLegResetDate(m_Cs, pStartDate, pEndDate, pCalculationPeriodBDA, pResetDate, 
                pFixingDate, pFixingBDA, pStub, m_DataDocument));
        }
        #endregion AddToResetPeriod

        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20231024 [XXXXX] RTS / Corrections diverses : Test sur FixingRateSpecified|FloatingRateSpecified
        // EG 20231127 [WI755] Implementation Return Swap : Add paymentCurrency
        public Cst.ErrLevel Calc(Pair<IInterestLeg, IInterestCalculation> pInterestLegInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region EffectiveDate & TerminationDate
            IInterestLeg _leg = pInterestLegInfo.First;
            effectiveDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _leg.CalculationPeriodDates.EffectiveDate, m_DataDocument);
            terminationDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _leg.CalculationPeriodDates.TerminationDate, m_DataDocument);
            #endregion EffectiveDate & TerminationDate

            #region EFS_Asset
            if ((null != pInterestLegInfo.Second) && pInterestLegInfo.Second.SqlAssetSpecified)
            {
                SQL_AssetBase _asset = pInterestLegInfo.Second.SqlAsset;
                m_Asset = new EFS_Asset
                {
                    idAsset = _asset.Id,
                    idC = _asset.IdC,
                    IdMarket = _asset.IdM,
                    description = _asset.Description,
                    assetCategory = _asset.AssetCategory
                };

                if (_asset is SQL_AssetRateIndex)
                {
                    SQL_AssetRateIndex _assetRateIndex = _asset as SQL_AssetRateIndex;
                    m_Asset.idBC = _assetRateIndex.Idx_IdBc;
                }
            }
            #endregion EFS_Asset

            #region InterestLeg Dates calculation
            #region Stubs Dates determination
            if (_leg.StubCalculationPeriodSpecified)
            {
                #region Stub
                if (_leg.StubCalculationPeriod.InitialStubSpecified)
                    Stub(_leg.StubCalculationPeriod.InitialStub, StubEnum.Initial);

                if (_leg.StubCalculationPeriod.FinalStubSpecified)
                    Stub(_leg.StubCalculationPeriod.FinalStub, StubEnum.Final);
                #endregion Stub
            }
            #endregion Stubs Dates determination

            // Les périodes sur la jambes Interest sont calculées si : 
            // - CFD NON OPEN
            bool _isPeriodCalculated = (false == IsOpen);
            if (_isPeriodCalculated)
            {

                _lstPaymentPeriods = new List<EFS_InterestLegPaymentDate>();

                #region InitialStub Period
                if (initialStartDateSpecified)

                    AddToCalculationPeriod(_lstPaymentPeriods, 
                        new EFS_AdjustableDate(m_Cs, initialStartDate.AdjustableDate, m_DataDocument), 
                        new EFS_AdjustableDate(m_Cs, initialEndDate.AdjustableDate, m_DataDocument), StubEnum.Initial, initialStubRateType);
                #endregion InitialStub Period
                #region PaymentDates

                IAdjustableRelativeOrPeriodicDates2 _paymentDates = _leg.CalculationPeriodDates.PaymentDates;
                // Calcul des périodes 
                if (_paymentDates.AdjustableDatesSpecified)
                    ret = CalculationPeriod(_lstPaymentPeriods, _paymentDates.AdjustableDates);
                else if (_paymentDates.RelativeDatesSpecified)
                    ret = CalculationPeriod(_lstPaymentPeriods, _paymentDates.RelativeDates);
                else if (_paymentDates.PeriodicDatesSpecified)
                    ret = CalculationPeriod(_lstPaymentPeriods, _paymentDates.PeriodicDates);
                #endregion PaymentDates
                #region FinalStub Period
                if (finalStartDateSpecified)
                    AddToCalculationPeriod(_lstPaymentPeriods,
                        new EFS_AdjustableDate(m_Cs, finalStartDate.AdjustableDate, m_DataDocument), 
                        new EFS_AdjustableDate(m_Cs, finalEndDate.AdjustableDate, m_DataDocument), StubEnum.Final, finalStubRateType);
                #endregion FinalStub Period


                if ((Cst.ErrLevel.SUCCESS == ret) && pInterestLegInfo.Second.FloatingRateSpecified)
                    ret = CalcResetDates(pInterestLegInfo);

                if ((Cst.ErrLevel.SUCCESS == ret) && (0 < _lstPaymentPeriods.Count))
                    paymentPeriods = _lstPaymentPeriods.ToArray();

                if ((Cst.ErrLevel.SUCCESS == ret) && pInterestLegInfo.Second.FloatingRateSpecified)
                    ret = SetMultiplierAndSpreadValue(pInterestLegInfo);

                #endregion InterestLeg Dates calculation
            }

            // EG 20231127 [WI755] Implementation Return Swap : New
            paymentCurrency = _leg.InterestAmount.MainLegAmountCurrency;
            return ret;
        }
        #endregion Calc
        #region CalcResetDates
        /// <summary>
        /// Calcul des périodes de reset
        /// </summary>
        /// <param name="pInterestLegInfo"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel CalcResetDates(Pair<IInterestLeg, IInterestCalculation> pInterestLegInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            IInterestLeg _leg = pInterestLegInfo.First;
            IAdjustableRelativeOrPeriodicDates2 _paymentDates = _leg.CalculationPeriodDates.PaymentDates;
            IInterestLegResetDates _resetDates = _leg.CalculationPeriodDates.ResetDates;

            _lstPaymentPeriods.ForEach(_payment =>
            {
                EFS_AdjustableDate _adjustableDate1 = _payment.periodDates.adjustableDate1;
                EFS_AdjustableDate _adjustableDate2 = _payment.periodDates.adjustableDate2;
                DateTime _startPeriod = _adjustableDate1.AdjustableDate.UnadjustedDate.DateValue;
                DateTime _endPeriod = _adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;

                DateTime _fixingDate = Convert.ToDateTime(null);
                IBusinessDayAdjustments _fixingDateBusinessDayAdjustments = null;

                if (_resetDates.ResetRelativeToSpecified)
                {
                    #region ResetRelativeTo
                    if (ResetRelativeToEnum.CalculationPeriodStartDate == _resetDates.ResetRelativeTo)
                    {
                        GetFixingDate(_resetDates, _endPeriod, _adjustableDate1, out _fixingDate, out _fixingDateBusinessDayAdjustments, true);
                        AddToResetPeriod(_startPeriod, _endPeriod, _adjustableDate1.AdjustableDate.DateAdjustments, _startPeriod, _fixingDate, _fixingDateBusinessDayAdjustments, _payment.Stub);
                    }
                    else if (ResetRelativeToEnum.CalculationPeriodEndDate == _resetDates.ResetRelativeTo)
                    {
                        GetFixingDate(_resetDates, _endPeriod, _adjustableDate2, out _fixingDate, out _fixingDateBusinessDayAdjustments, true);
                        AddToResetPeriod(_startPeriod, _endPeriod, _adjustableDate2.AdjustableDate.DateAdjustments, _endPeriod, _fixingDate, _fixingDateBusinessDayAdjustments, _payment.Stub);
                    }
                    #endregion ResetRelativeTo
                }
                else if (_resetDates.ResetFrequencySpecified)
                {
                    #region ResetFrequency
                    IResetFrequency _resetFrequency = _resetDates.ResetFrequency;
                    IInterval _interval = _resetDates.ResetFrequency.Interval;
                    if (_paymentDates.PeriodicDatesSpecified)
                        _interval = _paymentDates.PeriodicDates.CalculationPeriodFrequency.Interval;
                    Tools.CheckFrequency(_interval, _resetFrequency.Interval, out int remainder);
                    if (false == (remainder == 0))
                    {
                        string resetFrequency = _resetFrequency.Interval.PeriodMultiplier.ToString() + _resetFrequency.Interval.Period.ToString();
                        string calculationPeriodFrequency = _interval.PeriodMultiplier.ToString() + _interval.Period.ToString();
                        throw new Exception(StrFunc.AppendFormat("CalculationPeriodFrequency [{0}] isn't a multiple of the ResetFrequency. [{1}]",
                            calculationPeriodFrequency, resetFrequency));
                    }

                    DateTime _startDate = _startPeriod;
                    DateTime _endDate = Convert.ToDateTime(null);
                    DateTime _resetDate = Convert.ToDateTime(null);
                    int guard = 0;
                    while (true)
                    {
                        if (_endDate == _endPeriod)
                            break;

                        #region Calculate the unadjusted reset period dates
                        _endDate = Tools.ApplyInterval(_startDate, _endPeriod, _resetFrequency.Interval);
                        #endregion Calculate the unadjusted reset period dates

                        #region Calculate the unadjusted reset date (start or end,roll convention)
                        _resetDate = _endDate;
                        if (_resetFrequency.WeeklyRollConventionSpecified && guard == 0)
                            _resetDate = Tools.ApplyWeeklyRollConvention(_resetDate, _resetFrequency.WeeklyRollConvention);
                        #endregion Calculate the unadjusted reset date (start or end,roll convention)

                        #region Calculate the unadjusted fixing dates (initial fixing, offset)
                        EFS_AdjustableDate _adjustableResetDate = new EFS_AdjustableDate(m_Cs, _resetDate, _adjustableDate2.AdjustableDate.DateAdjustments, m_DataDocument);
                        GetFixingDate(_resetDates, _endPeriod, _adjustableResetDate, out _fixingDate, out _fixingDateBusinessDayAdjustments, (guard == 0));
                        #endregion Calculate the unadjusted fixing dates (initial fixing, offset)

                        AddToResetPeriod(_startDate, _endDate, _adjustableDate2.AdjustableDate.DateAdjustments, _resetDate, _fixingDate, _fixingDateBusinessDayAdjustments, _payment.Stub);
                        _startDate = _endDate;

                        guard++;
                        if (guard == 9999)
                        {
                            ret = Cst.ErrLevel.ABORTED;
                            string msgException = "Interest Leg Reset Dates exception:" + Cst.CrLf;
                            msgException += "Incoherence during the calculation and the adjustment of dates !" + Cst.CrLf;
                            msgException += "Please, verify dates, periods, business day adjustment and roll convention on the trade";
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException);
                        }

                        //if (isOnlyNextCalculation)
                        //    break;
                        //else
                        continue;
                    }
                    #endregion ResetFrequency
                }

                if ((Cst.ErrLevel.SUCCESS == ret) && (0 < _lstResetPeriods.Count))
                {
                    _payment.resetDates = _lstResetPeriods.ToArray();
                    _lstResetPeriods.Clear();
                }

            });
            #endregion InterestLegResetDates calculation

            return ret;
        }

        #region GetFixingDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void GetFixingDate(IInterestLegResetDates pResetDates, DateTime pEndPeriod, EFS_AdjustableDate pResetDate, out DateTime pFixingDate, out IBusinessDayAdjustments pFixingBDA, bool pIsInitialFixingDate)
        {
            IBusinessDayAdjustments _fixingDateBDA = pResetDate.AdjustableDate.DateAdjustments;
            DateTime _fixingDate = pResetDate.adjustedDate.DateValue;
            if (pIsInitialFixingDate && pResetDates.InitialFixingDateSpecified)
            {
                _fixingDateBDA = pResetDates.InitialFixingDate.GetAdjustments;
                _fixingDate = Tools.ApplyOffset(m_Cs, pResetDate.adjustedDate.DateValue, pEndPeriod, pResetDates.InitialFixingDate.GetOffset, _fixingDateBDA, m_DataDocument);
            }
            else if (pResetDates.FixingDatesSpecified)
            {
                if (pResetDates.FixingDates.AdjustableDatesSpecified)
                {
                    // NON GERE
                }
                else if (pResetDates.FixingDates.RelativeDateOffsetSpecified)
                {
                    _fixingDate = Tools.ApplyOffset(m_Cs, pResetDate.adjustedDate.DateValue, pEndPeriod, pResetDates.FixingDates.RelativeDateOffset.GetOffset, _fixingDateBDA, m_DataDocument);
                }
            }
            pFixingDate = _fixingDate;
            pFixingBDA = _fixingDateBDA;
        }
        #endregion GetFixingDate

        #region SetMultiplierAndSpreadValue
        // 20090828 EG ticket : 16656
        // EG 20150309 POC - BERKELEY SpreadSchedule rempalce ISchedule
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel SetMultiplierAndSpreadValue(Pair<IInterestLeg, IInterestCalculation> pInterestLegInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (pInterestLegInfo.Second.FloatingRateSpecified)
            {
                IFloatingRate floatingRate = pInterestLegInfo.Second.FloatingRate;
                if (ArrFunc.IsFilled(paymentPeriods))
                {
                    for (int i = 0; i < paymentPeriods.Length; i++)
                    {
                        EFS_InterestLegPaymentDate _payment = paymentPeriods[i];
                        DateTime startDate = _payment.periodDates.adjustableDate1.adjustedDate.DateValue;
                        DateTime endDate = _payment.periodDates.adjustableDate2.adjustedDate.DateValue;
                        //ISchedule spreadSchedule = null;
                        ISpreadSchedule[] spreadSchedule = null;
                        ISchedule multiplierSchedule = null;

                        #region Regular
                        _payment.spreadSpecified = floatingRate.SpreadScheduleSpecified;
                        if (_payment.spreadSpecified)
                        {
                            //spreadSchedule = floatingRate.spreadSchedule;
                            spreadSchedule = floatingRate.LstSpreadSchedule;
                        }

                        _payment.multiplierSpecified = floatingRate.FloatingRateMultiplierScheduleSpecified;
                        if (_payment.multiplierSpecified)
                            multiplierSchedule = floatingRate.FloatingRateMultiplierSchedule;
                        #endregion Regular

                        if (pInterestLegInfo.First.StubCalculationPeriodSpecified)
                        {
                            IStubCalculationPeriod _stub = pInterestLegInfo.First.StubCalculationPeriod;
                            IFloatingRate stubFloatingRate;
                            if ((0 == i) && initialStartDateSpecified)
                            {
                                #region InitialStub
                                if (_stub.InitialStub.StubTypeFloatingRateSpecified)
                                {
                                    stubFloatingRate = _stub.InitialStub.StubTypeFloatingRate[0];
                                    _payment.spreadSpecified = stubFloatingRate.SpreadScheduleSpecified;
                                    // EG 20150309 POC - BERKELEY SpreadSchedule 
                                    if (_payment.spreadSpecified)
                                    {
                                        //spreadSchedule = stubFloatingRate.spreadSchedule;
                                        spreadSchedule = stubFloatingRate.LstSpreadSchedule;
                                    }
                                    _payment.multiplierSpecified = stubFloatingRate.FloatingRateMultiplierScheduleSpecified;
                                    if (_payment.multiplierSpecified)
                                        multiplierSchedule = stubFloatingRate.FloatingRateMultiplierSchedule;
                                }
                                else
                                    continue;
                                #endregion InitialStub
                            }
                            else if (((paymentPeriods.Length - 1) == i) && finalEndDateSpecified)
                            {
                                #region FinalStub
                                if (_stub.FinalStub.StubTypeFloatingRateSpecified)
                                {
                                    stubFloatingRate = _stub.FinalStub.StubTypeFloatingRate[0];
                                    _payment.spreadSpecified = stubFloatingRate.SpreadScheduleSpecified;
                                    // EG 20150309 POC - BERKELEY SpreadSchedule 
                                    if (_payment.spreadSpecified)
                                    {
                                        //spreadSchedule = stubFloatingRate.spreadSchedule;
                                        spreadSchedule = stubFloatingRate.LstSpreadSchedule;
                                    }

                                    _payment.multiplierSpecified = stubFloatingRate.FloatingRateMultiplierScheduleSpecified;
                                    if (_payment.multiplierSpecified)
                                        multiplierSchedule = stubFloatingRate.FloatingRateMultiplierSchedule;
                                }
                                else
                                    continue;
                                #endregion FinalStub
                            }
                        }
                        IBusinessDayAdjustments _bda = _payment.periodDates.adjustableDate2.AdjustableDate.DateAdjustments;
                        // EG 20150309 POC - BERKELEY SpreadSchedule (Boucle)
                        //if (_payment.spreadSpecified)
                        //    _payment.spread = new EFS_Decimal(Tools.GetStepValue(cs, spreadSchedule, startDate, endDate, _bda));
                        //
                        if (_payment.spreadSpecified)
                        {
                            decimal _spread = 0;
                            // EG 20150309 POC - BERKELEY on cumule les spreads
                            foreach (ISpreadSchedule item in spreadSchedule)
                            {
                                _spread += Tools.GetStepValue(m_Cs, item, startDate, endDate, _bda, m_DataDocument);
                            }
                            _payment.spread = new EFS_Decimal(_spread);
                        }

                        if (_payment.multiplierSpecified)
                            _payment.multiplier = new EFS_Decimal(Tools.GetStepValue(m_Cs, multiplierSchedule, startDate, endDate, _bda, m_DataDocument));
                    }
                }
            }
            return ret;
        }
        #endregion SetMultiplierAndSpreadValue

        #region Stub
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void Stub(IStub pStub, StubEnum pStubType)
        {
            if (StubEnum.Initial == pStubType)
            {
                #region InitialStubDate
                initialStartDateSpecified = pStub.StubStartDateSpecified;
                initialEndDateSpecified = pStub.StubEndDateSpecified;
                if (initialStartDateSpecified)
                    initialStartDate = Tools.GetEFS_AdjustableDate(m_Cs, pStub.StubStartDate, m_DataDocument);
                else
                    initialStartDate = (EFS_AdjustableDate)effectiveDateAdjustment.Clone();

                if (initialEndDateSpecified)
                    initialEndDate = Tools.GetEFS_AdjustableDate(m_Cs, pStub.StubEndDate, m_DataDocument);
                else
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StubException(pStubType, "End"));

                if (pStub.StubTypeAmountSpecified)
                    initialStubRateType = RateTypeEnum.None;
                else if (pStub.StubTypeFixedRateSpecified)
                    initialStubRateType = RateTypeEnum.FixedRate;
                else if (pStub.StubTypeFloatingRateSpecified)
                    initialStubRateType = RateTypeEnum.FloatingRate;
                #endregion InitialStubDate
            }
            else if (StubEnum.Final == pStubType)
            {
                #region FinalStubDate
                finalStartDateSpecified = pStub.StubStartDateSpecified;
                finalEndDateSpecified = pStub.StubEndDateSpecified;
                if (finalStartDateSpecified)
                    finalStartDate = Tools.GetEFS_AdjustableDate(m_Cs, pStub.StubStartDate, m_DataDocument);
                else
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StubException(pStubType, "Start"));

                if (finalEndDateSpecified)
                    finalEndDate = Tools.GetEFS_AdjustableDate(m_Cs, pStub.StubEndDate, m_DataDocument);
                else
                    finalEndDate = (EFS_AdjustableDate)terminationDateAdjustment.Clone();

                if (pStub.StubTypeAmountSpecified)
                    finalStubRateType = RateTypeEnum.None;
                else if (pStub.StubTypeFixedRateSpecified)
                    finalStubRateType = RateTypeEnum.FixedRate;
                else if (pStub.StubTypeFloatingRateSpecified)
                    finalStubRateType = RateTypeEnum.FloatingRate;

                #endregion FinalStubDate
            }

        }
        #endregion Stub
        #region StubException
        private static string StubException(StubEnum pStub, string pStubTypeDate)
        {
            string msgException = "Stub Calculation Period Dates exception:" + Cst.CrLf;
            msgException += "The " + pStub.ToString() + "stub " + pStubTypeDate + " date is not specified !" + Cst.CrLf;
            return msgException;
        }
        #endregion StubException

        #endregion Methods
    }
    #endregion EFS_InterestLeg
    #region EFS_InterestLegPaymentDate
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_InterestLegPaymentDate
    {
        #region Members
        public EFS_AdjustablePeriod periodDates;
        public EFS_InterestLegResetDate[] resetDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stubSpecified;
        public StubEnum stub;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public RateTypeEnum stubRateType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool multiplierSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spreadSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal spread;
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
                    DateValue = periodDates.adjustableDate2.adjustedDate.DateValue
                };
                return dt;
            }
        }
        #endregion AdjustedEndPeriod
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
        #region Stub
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public StubEnum Stub
        {
            get {return this.stub;}
        }
        #endregion Stub

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
        #region Constructors
        public EFS_InterestLegPaymentDate(EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate, StubEnum pStub, RateTypeEnum pStubRateType)
        {
            periodDates = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)pStartDate.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)pEndDate.Clone()
            };
            stubSpecified = (StubEnum.None != pStub);
            stub = pStub;
            stubRateType = pStubRateType;
        }
        #endregion Constructors
        #region Methods
        #region Rate
        // EG 20140904 Add AssetCategory
        // EG 20231024 [XXXXX] RTS / Corrections diverses : Ajout méthode appelée dans la génération des événement et jamais écrite jusqu'à ce jour
        public object Rate(string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {
            EFS_Decimal fixedRate = null;
            Nullable<RateTypeEnum> rateTypeEnum = null;
            if (System.Enum.IsDefined(typeof(RateTypeEnum), pRateType))
                rateTypeEnum = (RateTypeEnum)ReflectionTools.EnumParse(new RateTypeEnum(), pRateType);
            if (rateTypeEnum.HasValue && (rateTypeEnum.Value == RateTypeEnum.FixedRate))
            {
                Nullable<decimal> _fixedRate = null;
                if ((StubEnum.Initial == stub) && (null != pRateInitialStub) && (pRateInitialStub is EFS_Decimal))
                    _fixedRate = (pRateInitialStub as EFS_Decimal).DecValue;
                else if ((StubEnum.Final == stub) && (null != pRateFinalStub) && (pRateFinalStub is EFS_Decimal))
                    _fixedRate = (pRateFinalStub as EFS_Decimal).DecValue;
                else if (pRate is EFS_Decimal)
                    _fixedRate = (pRate as EFS_Decimal).DecValue;

                if (_fixedRate.HasValue)
                {
                    fixedRate = new EFS_Decimal
                    {
                        DecValue = _fixedRate.Value
                    };
                }
            }
            return fixedRate;
        }
        #endregion Rate

        #region AssetRateIndex
        // EG 20140904 Add AssetCategory
        // EG 20231024 [XXXXX] RTS / Corrections diverses : Test sur IFloatingRate + Setting pRateFinalStub
        public object AssetRateIndex(string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {
            EFS_Asset asset = null;
            Nullable<RateTypeEnum> rateTypeEnum = null;
            if (System.Enum.IsDefined(typeof(RateTypeEnum), pRateType))
                rateTypeEnum = (RateTypeEnum)ReflectionTools.EnumParse(new RateTypeEnum(), pRateType);
            if (rateTypeEnum.HasValue && (rateTypeEnum.Value == RateTypeEnum.FloatingRate))
            {
                IFloatingRate _floatingRate = null;
                if ((StubEnum.Initial == stub) && (null != pRateInitialStub) && (pRateInitialStub is IFloatingRate rate))
                    _floatingRate = rate;
                else if ((StubEnum.Final == stub) && (null != pRateFinalStub) && (pRateFinalStub is IFloatingRate rate1))
                    _floatingRate = rate1;
                else if ((RateTypeEnum.FloatingRate == rateTypeEnum) && (pRate is IFloatingRate rate2))
                    _floatingRate = rate2;

                if (null != _floatingRate)
                {
                    asset = new EFS_Asset
                    {
                        idAsset = _floatingRate.FloatingRateIndex.OTCmlId,
                        assetCategory = Cst.UnderlyingAsset.RateIndex
                    };
                }
            }
            return asset;
        }
        #endregion AssetRateIndex
        // EG 20231127 [WI755] Implementation Return Swap : New
        #region GetEventCodeTermination
        public static string GetEventCodeTermination(EFS_EventDate pTerminationDate, EFS_EventDate pMaxTerminationDate, bool pIsPerpetual)
        {
            if (pIsPerpetual || (0 != pMaxTerminationDate.unadjustedDate.DateValue.CompareTo(pTerminationDate.unadjustedDate.DateValue)))
                return EventCodeFunc.Intermediary;
            else
                return EventCodeFunc.Termination;
        }
        #endregion GetEventCodeTermination
        #endregion Methods
    }
    #endregion EFS_InterestLegPaymentDate
    #region EFS_InterestLegResetDate
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_InterestLegResetDate
    {
        #region Members
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate resetDateAdjustment;
        public EFS_AdjustableDate fixingDateAdjustment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stubSpecified;
        public StubEnum stub;
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
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_InterestLegResetDate(string pCS, DateTime pStartDate, DateTime pEndDate, IBusinessDayAdjustments pCalculationPeriodBDA, 
            DateTime pResetDate, DateTime pFixingDate, IBusinessDayAdjustments pFixingBDA, StubEnum pStub, DataDocumentContainer pDataDocument)
        {
            periodDatesAdjustment = new EFS_AdjustablePeriod
            {
                adjustableDate1 = new EFS_AdjustableDate(pCS, pStartDate, pCalculationPeriodBDA, pDataDocument),
                adjustableDate2 = new EFS_AdjustableDate(pCS, pEndDate, pCalculationPeriodBDA, pDataDocument)
            };
            resetDateAdjustment = new EFS_AdjustableDate(pCS, pResetDate, pCalculationPeriodBDA, pDataDocument);
            fixingDateAdjustment = new EFS_AdjustableDate(pCS, pFixingDate, pFixingBDA, pDataDocument);
            stubSpecified = (pStub != StubEnum.None);
            stub = pStub;
        }
        #endregion Constructors
        #region Methods
        #region Rate
        // EG 20231127 [WI755] Implementation Return Swap : New
        public object Rate(StubEnum pStub, string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {
            EFS_Decimal fixedRate = null;
            Nullable<RateTypeEnum> rateTypeEnum = null;
            if (System.Enum.IsDefined(typeof(RateTypeEnum), pRateType))
                rateTypeEnum = (RateTypeEnum)ReflectionTools.EnumParse(new RateTypeEnum(), pRateType);
            if (rateTypeEnum.HasValue && (rateTypeEnum.Value == RateTypeEnum.FixedRate))
            {
                Nullable<decimal> _fixedRate = null;
                if ((StubEnum.Initial == pStub) && (null != pRateInitialStub) && (pRateInitialStub is EFS_Decimal))
                    _fixedRate = (pRateInitialStub as EFS_Decimal).DecValue;
                else if ((StubEnum.Final == pStub) && (null != pRateFinalStub) && (pRateFinalStub is EFS_Decimal))
                    _fixedRate = (pRateFinalStub as EFS_Decimal).DecValue;
                else if (pRate is EFS_Decimal)
                    _fixedRate = (pRate as EFS_Decimal).DecValue;

                if (_fixedRate.HasValue)
                {
                    fixedRate = new EFS_Decimal
                    {
                        DecValue = _fixedRate.Value
                    };
                }
            }
            return fixedRate;
        }
        #endregion Rate
        #region AssetRateIndex
        // EG 20140904 Add AssetCategory
        // EG 20231024 [XXXXX] RTS / Corrections diverses : Test sur IFLoatingRate
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring
        public object AssetRateIndex(StubEnum pStub, string pRateType, object pRate, object pRateInitialStub, object pRateFinalStub)
        {
            EFS_Asset asset = null;
            Nullable<RateTypeEnum> rateTypeEnum = null;
            if (System.Enum.IsDefined(typeof(RateTypeEnum), pRateType))
                rateTypeEnum = (RateTypeEnum)ReflectionTools.EnumParse(new RateTypeEnum(), pRateType);
            if (rateTypeEnum.HasValue && (rateTypeEnum.Value == RateTypeEnum.FloatingRate))
            {
                IFloatingRate _floatingRate = null;
                if ((StubEnum.Initial == pStub) && (null != pRateInitialStub) && (pRate is IFloatingRate))
                    _floatingRate = (IFloatingRate)pRateInitialStub;
                else if ((StubEnum.Final == pStub) && (null != pRateFinalStub) && (pRate is IFloatingRate))
                    _floatingRate = (IFloatingRate)pRateInitialStub;
                else if ((RateTypeEnum.FloatingRate == rateTypeEnum) && (pRate is IFloatingRate rate))
                    _floatingRate = rate;

                if (null != _floatingRate)
                {
                    asset = new EFS_Asset
                    {
                        idAsset = _floatingRate.FloatingRateIndex.OTCmlId,
                        assetCategory = Cst.UnderlyingAsset.RateIndex
                    };
                }
            }
            return asset;
        }
        #endregion AssetRateIndex
        #endregion Methods
    }
    #endregion EFS_InterestLegResetDate

    #region EFS_PrincipalExchange
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_PrincipalExchange
    {
        #region Members
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney principalExchangeAmount;
        public EFS_AdjustableDate principalExchangeDate;
        #endregion Members
        #region Accessors
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
                if (null != principalExchangeDate)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = principalExchangeDate.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;
            }
        }
        #endregion AdjustedPaymentDate
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get
            {
                return principalExchangeAmount.Amount;
            }
        }
        #endregion Amount
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Currency
        {
            get
            {
                return principalExchangeAmount.Currency;
            }
        }
        #endregion Currency
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;
            }
        }
        #endregion PayerPartyReference
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {
                return new EFS_EventDate(principalExchangeDate);
            }
        }
        #endregion PaymentDate
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;
            }
        }
        #endregion ReceiverPartyReference
        #endregion Accessors
        #region Constructors
        public EFS_PrincipalExchange(IReference pPayerPartyReference, IReference pReceiverPartyReference, IMoney pPrincipalExchangeAmount, EFS_AdjustableDate pPrincipalExchangeDate)
        {
            payerPartyReference = pPayerPartyReference;
            receiverPartyReference = pReceiverPartyReference;
            principalExchangeAmount = pPrincipalExchangeAmount;
            principalExchangeDate = pPrincipalExchangeDate;
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion EFS_PrincipalExchange
    #region EFS_PrincipalExchangeFeatures
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_PrincipalExchangeFeatures
    {
        #region Members
        private readonly string m_Cs;
        private readonly ITradeDate m_TradeDate;
        private readonly DataDocumentContainer m_DataDocument;

        public EFS_PrincipalExchange[] initialPrincipalExchange;
        public bool initialPrincipalExchangeSpecified;
        public EFS_PrincipalExchange[] finalPrincipalExchange;
        public bool finalPrincipalExchangeSpecified;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_PrincipalExchangeFeatures(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            m_TradeDate = m_DataDocument.TradeHeader.TradeDate;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_PrincipalExchangeFeatures(string pConnectionString, IReturnSwap pReturnSwap, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            Calc(pReturnSwap);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc(IReturnSwap pReturnSwap)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            if (pReturnSwap.PrincipalExchangeFeaturesSpecified && pReturnSwap.PrincipalExchangeFeatures.DescriptionsSpecified)
            {
                IPrincipalExchanges principalExchange = pReturnSwap.PrincipalExchangeFeatures.PrincipalExchanges;
                initialPrincipalExchangeSpecified = principalExchange.InitialExchange.BoolValue;
                finalPrincipalExchangeSpecified = principalExchange.FinalExchange.BoolValue;
                List<EFS_PrincipalExchange> aInitialPrincipalExchange = new List<EFS_PrincipalExchange>();
                List<EFS_PrincipalExchange> aFinalPrincipalExchange = new List<EFS_PrincipalExchange>();

                IPrincipalExchangeDescriptions[] descriptions = pReturnSwap.PrincipalExchangeFeatures.Descriptions;
                foreach (IPrincipalExchangeDescriptions description in descriptions)
                {
                    IReference payerPartyReference = description.PayerPartyReference;
                    IReference receiverPartyReference = description.ReceiverPartyReference;
                    IPrincipalExchangeAmount principalExchangeAmount = description.PrincipalExchangeAmount;
                    IMoney money = null;
                    #region PrincipalExchangeAmount
                    if (principalExchangeAmount.RelativeToSpecified)
                    {
                        #region Relativeto
                        object exchangeAmount = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, principalExchangeAmount.RelativeTo.HRef);
                        if (null != exchangeAmount)
                        {
                            if (Tools.IsTypeOrInterfaceOf(exchangeAmount, InterfaceEnum.IMoney))
                                money = (IMoney)exchangeAmount;
                            else
                            {
                                // TO BE DEFINE
                            }
                        }
                        #endregion Relativeto
                    }
                    else if (principalExchangeAmount.DeterminationMethodSpecified)
                    {
                    }
                    else if (principalExchangeAmount.AmountSpecified)
                    {
                        money = (IMoney)principalExchangeAmount.Amount;
                    }
                    #endregion PrincipalExchangeAmount

                    #region PrincipalExchangeDate
                    EFS_AdjustableDate adjustablePrincipalExchangeDate = null;
                    //IAdjustableOrRelativeDate principalExchangeDate = description.principalExchangeDate;
                    if (description.PrincipalExchangeDate.AdjustableDateSpecified)
                    {
                        #region AdjustableDate
                        adjustablePrincipalExchangeDate = new EFS_AdjustableDate(m_Cs, description.PrincipalExchangeDate.AdjustableDate, m_DataDocument);
                        #endregion AdjustableDate
                    }
                    else if (description.PrincipalExchangeDate.RelativeDateSpecified)
                    {
                        #region RelativeDateOffset
                        DateTime offsetDate = DateTime.MinValue;
                        ret = Tools.OffSetDateRelativeTo(m_Cs, description.PrincipalExchangeDate.RelativeDate, out offsetDate, m_DataDocument);
                        adjustablePrincipalExchangeDate = new EFS_AdjustableDate(m_Cs, offsetDate, description.PrincipalExchangeDate.RelativeDate.GetAdjustments, m_DataDocument);
                        #endregion RelativeDateOffset
                    }
                    #endregion PrincipalExchangeDate
                    if (IsInitialPrincipalExchange(pReturnSwap, adjustablePrincipalExchangeDate))
                        aInitialPrincipalExchange.Add(new EFS_PrincipalExchange(payerPartyReference, receiverPartyReference, money, adjustablePrincipalExchangeDate));
                    else if (IsFinalPrincipalExchange(pReturnSwap, adjustablePrincipalExchangeDate))
                        aFinalPrincipalExchange.Add(new EFS_PrincipalExchange(payerPartyReference, receiverPartyReference, money, adjustablePrincipalExchangeDate));
                }

                if (0 < aInitialPrincipalExchange.Count)
                    initialPrincipalExchange = aInitialPrincipalExchange.ToArray();
                if (0 < aFinalPrincipalExchange.Count)
                    finalPrincipalExchange = aFinalPrincipalExchange.ToArray();
            }

            return ret;
        }
        #endregion Calc
        #region IsInitialPrincipalExchange
        private bool IsInitialPrincipalExchange(IReturnSwapBase pReturnSwap, EFS_AdjustableDate pAdjustableDate)
        {
            bool isInitialPrincipalExchange = false;

            DateTime principalExchangeDate = pAdjustableDate.adjustedDate.DateValue;
            if (m_TradeDate.DateValue == principalExchangeDate)
                isInitialPrincipalExchange = true;
            else
            {
                foreach (IReturnLeg returnLeg in pReturnSwap.ReturnLeg)
                {
                    if (null != returnLeg.Efs_ReturnLeg)
                    {
                        DateTime effectiveDate = returnLeg.Efs_ReturnLeg.effectiveDateAdjustment.adjustedDate.DateValue;
                        if (effectiveDate == principalExchangeDate)
                        {
                            isInitialPrincipalExchange = true;
                            break;
                        }
                    }
                }
            }

            return isInitialPrincipalExchange;
        }
        #endregion IsInitialPrincipalExchange
        #region IsFinalPrincipalExchange
        private static bool IsFinalPrincipalExchange(IReturnSwapBase pReturnSwap, EFS_AdjustableDate pAdjustableDate)
        {
            bool isFinalPrincipalExchange = false;

            DateTime principalExchangeDate = pAdjustableDate.adjustedDate.DateValue;
            foreach (IReturnLeg returnLeg in pReturnSwap.ReturnLeg)
            {
                if (null != returnLeg.Efs_ReturnLeg)
                {
                    DateTime terminationDate = returnLeg.Efs_ReturnLeg.terminationDateAdjustment.adjustedDate.DateValue;
                    if (terminationDate == principalExchangeDate)
                    {
                        isFinalPrincipalExchange = true;
                        break;
                    }
                }
            }

            return isFinalPrincipalExchange;
        }
        #endregion IsInitialPrincipalExchange
        #endregion Methods
    }
    #endregion EFS_PrincipalExchangeFeatures

    #region EFS_ReturnLeg
    /// EG 20150302 Add notionalBase|notionalBaseSpecified (CFD Forex)
    // EG 20231127 [WI755] Implementation Return Swap : Add properties
    public class EFS_ReturnLeg : EFS_ReturnSwapLegUnderlyer
    {
        #region Members
        public EFS_Commission initialFee;
        public bool initialFeeSpecified;
        public IReturnLegValuation rateOfReturn;
        public bool notionalBaseSpecified;
        public IMoney notionalBase;
        public string paymentCurrency;
        public EFS_ReturnLegValuationPeriod[] notionalPeriods;
        #endregion Members
        #region Accessors
        public bool IsMarginRatio { get { return rateOfReturn.MarginRatioSpecified; } }
        public bool IsNotionalReset { get { return rateOfReturn.NotionalResetSpecified && rateOfReturn.NotionalReset.BoolValue; } }
        public bool IsNotNotionalReset { get { return !IsNotionalReset; } }
        public bool IsMargining { set; get; }
        public EFS_Decimal InitialNotionalAmount { get { return NotionalAmount; } }
        public string InitialNotionalCurrency { get { return NotionalCurrency; } }
        #region MarginRatioAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20150309 POC - BERKELEY Add SpreadScheduleAmount
        public EFS_Decimal MarginRatioAmount
        {
            get
            {
                EFS_Decimal _amount = new EFS_Decimal();
                if (rateOfReturn.MarginRatioSpecified)
                {
                    _amount.DecValue = rateOfReturn.MarginRatio.Amount.DecValue;
                    if (rateOfReturn.MarginRatio.SpreadScheduleSpecified)
                        _amount.DecValue += rateOfReturn.MarginRatio.SpreadSchedule.InitialValue.DecValue;
                }
                return _amount;
            }
        }
        #endregion MarginRatioAmount
        #region MarginRatioUnitType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string MarginRatioUnitType
        {
            get
            {
                UnitTypeEnum _unitType = UnitTypeEnum.None;
                if (rateOfReturn.MarginRatioSpecified)
                {
                    switch (rateOfReturn.MarginRatio.PriceExpression)
                    {
                        case PriceExpressionEnum.AbsoluteTerms:
                            _unitType = UnitTypeEnum.Currency;
                            break;
                        case PriceExpressionEnum.PercentageOfNotional:
                            _unitType = UnitTypeEnum.Percentage;
                            break;
                    }
                }
                return _unitType.ToString();
            }

        }
        #endregion MarginRatioUnitType
        #region MarginRatioUnit
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string MarginRatioUnit
        {
            get
            {
                string _unit = string.Empty;
                if (rateOfReturn.MarginRatioSpecified &&
                    rateOfReturn.MarginRatio.CurrencySpecified &&
                    rateOfReturn.MarginRatio.PriceExpression == PriceExpressionEnum.AbsoluteTerms)
                    _unit = rateOfReturn.MarginRatio.Currency.Value;
                return _unit.ToString();
            }
        }
        #endregion MarginRatioUnit
        #region NotionalBaseAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150302 New (CFD Forex)
        public EFS_Decimal NotionalBaseAmount
        {
            get { return (notionalBaseSpecified ? notionalBase.Amount : null); }
        }
        #endregion NotionalBaseAmount
        #region NotionalBaseCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20150302 New (CFD Forex)
        public string NotionalBaseCurrency
        {
            get { return (notionalBaseSpecified ? notionalBase.Currency : null); }
        }
        #endregion NotionalBaseCurrency

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ReturnLeg(string pCS, DataDocumentContainer pDataDocument) : base(pCS, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20231127 [WI755] Implementation Return Swap : Add notionalPeriods
        public EFS_ReturnLeg(string pCS, Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLegInfo, DataDocumentContainer pDataDocument)
            : base(pCS, pReturnLegInfo, pDataDocument)
        {
            Calc(pReturnLegInfo);
            pReturnLegInfo.First.RateOfReturn.Efs_RateOfReturn = new EFS_RateOfReturn(pCS, pReturnLegInfo, pDataDocument);
            notionalPeriods = pReturnLegInfo.First.RateOfReturn.Efs_RateOfReturn.valuationPeriods;
        }
        #endregion Constructors
        #region Methods
        #region Calc
        /// EG 20150302 Add notionalBase|notionalBaseSpecified setting (CFD Forex)
        // EG 20231127 [WI755] Implementation Return Swap : Add paymentCurrency
        public Cst.ErrLevel Calc(Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLegInfo)
        {
            IReturnLeg _leg = pReturnLegInfo.First;
            rateOfReturn = _leg.RateOfReturn;
            #region InitialFees
            IReturnLegValuationPrice price = _leg.RateOfReturn.InitialPrice;
            initialFeeSpecified = price.CommissionSpecified;
            if (initialFeeSpecified)
                initialFee = new EFS_Commission(price, notional);
            #endregion InitialFees

            #region NotionalBase
            if (_leg.Underlyer.UnderlyerSingleSpecified)
            {
                notionalBaseSpecified = _leg.Underlyer.UnderlyerSingle.NotionalBaseSpecified;
                notionalBase = _leg.Underlyer.UnderlyerSingle.NotionalBase;
            }
            #endregion NotionalBase

            paymentCurrency = _leg.ReturnSwapAmount.MainLegAmountCurrency;

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_ReturnLeg
    #region EFS_RateOfReturn
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_RateOfReturn : EFS_ReturnSwapLeg
    {
        #region Members
        public EFS_ReturnLegValuationPeriod[] valuationPeriods;
        public bool initialMarginSpecified;
        public IMoney initialMargin;
        private List<EFS_ReturnLegValuationPeriod> _lstValuationPeriods;
        private List<EFS_ReturnLegValuationPeriod> _lstValuationInterimPeriods;
        private List<EFS_ReturnLegValuationPeriod> _lstValuationFinalPeriods;
        #endregion Members
        #region Accessors
        #region InitialMarginAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal InitialMarginAmount
        {
            get
            {
                EFS_Decimal _amount = new EFS_Decimal();
                if (initialMarginSpecified)
                    _amount.DecValue = initialMargin.Amount.DecValue;
                return _amount;
            }

        }
        #endregion InitialMarginAmount
        #region InitialMarginCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginCurrency
        {
            get
            {
                string _unit = string.Empty;
                if (initialMarginSpecified)
                    _unit = initialMargin.Currency;
                return _unit;
            }
        }
        #endregion InitialMarginCurrency
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_RateOfReturn(string pCS, Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLegInfo, DataDocumentContainer pDataDocument)
            : base(pCS, (IReturnSwapLeg)pReturnLegInfo.First, pReturnLegInfo.First.Notional, pDataDocument)
        {
            Calc(pReturnLegInfo);
        }
        #endregion Constructors
        #region Methods

        #region AddToCalculationPeriod
        protected override void AddToCalculationPeriod<T>(List<T> pList, EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate)
        {
            if (pList is List<EFS_ReturnLegValuationPeriod>)
            {
                if (!(pList is List<EFS_ReturnLegValuationPeriod> _lst))
                    _lst = new List<EFS_ReturnLegValuationPeriod>();
                _lst.Add(new EFS_ReturnLegValuationPeriod(pStartDate, pEndDate));
            }
        }
        #endregion AddToCalculationPeriod
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc(Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLegInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            IReturnLeg _leg = pReturnLegInfo.First;

            #region InitialMargin
            InitialMarginCalculation(pReturnLegInfo);
            #endregion InitialMargin

            #region EffectiveDate and Termination calculation
            effectiveDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _leg.EffectiveDate, m_DataDocument);
            terminationDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _leg.TerminationDate, m_DataDocument);
            #endregion EffectiveDate and Termination calculation

            _lstValuationPeriods = new List<EFS_ReturnLegValuationPeriod>();


            // Les périodes sur la jambes ReturnLeg sont calculées si : 
            // - CFD NON OPEN et PERIODICITE <> 1 JOUR
            bool _isPeriodCalculated = (false == IsOpen) && (false == _leg.IsDailyPeriod);
            if (_isPeriodCalculated)
            {
                #region PriceInterim
                // ValuationPriceInterim
                if (_leg.RateOfReturn.ValuationPriceInterimSpecified &&
                    _leg.RateOfReturn.ValuationPriceInterim.ValuationRulesSpecified)
                {
                    _lstValuationInterimPeriods = new List<EFS_ReturnLegValuationPeriod>();
                    ret = ValuationRulesCalculation(_lstValuationInterimPeriods, _leg.RateOfReturn.ValuationPriceInterim.ValuationRules);
                }
                // PaymentPriceInterim
                if (_leg.RateOfReturn.PaymentDates.PaymentDatesInterimSpecified)
                    PaymentPeriod(_lstValuationInterimPeriods, _leg.RateOfReturn.PaymentDates.PaymentDatesInterim);

                #endregion PriceInterim
                #region PriceFinal
                // ValuationPriceFinal
                if (false == IsOpen)
                {
                    if ((Cst.ErrLevel.SUCCESS == ret) && _leg.RateOfReturn.ValuationPriceFinal.ValuationRulesSpecified)
                    {
                        _lstValuationFinalPeriods = new List<EFS_ReturnLegValuationPeriod>();
                        ret = ValuationRulesCalculation(_lstValuationFinalPeriods, _leg.RateOfReturn.ValuationPriceFinal.ValuationRules);
                        // PaymentPriceFinal
                        PaymentPeriod(_lstValuationFinalPeriods, _leg.RateOfReturn.PaymentDates.PaymentDateFinal);
                    }
                }
                #endregion PriceFinal
            }
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                if ((null != _lstValuationInterimPeriods) && (0 < _lstValuationInterimPeriods.Count))
                    _lstValuationPeriods.AddRange(_lstValuationInterimPeriods);
                if ((null != _lstValuationFinalPeriods) && (0 < _lstValuationFinalPeriods.Count))
                {
                    _lstValuationFinalPeriods.ForEach(_vf =>
                        {
                            if (false == _lstValuationPeriods.Exists(_vp =>
                                (_vp.EndPeriod.unadjustedDate.DateValue == _vf.StartPeriod.unadjustedDate.DateValue &&
                                 _vp.EndPeriod.unadjustedDate.DateValue == _vf.EndPeriod.unadjustedDate.DateValue)))
                            {
                                _lstValuationPeriods.Add(_vf);
                            }
                        });
                }
                if (0 < _lstValuationPeriods.Count)
                    valuationPeriods = _lstValuationPeriods.ToArray();
            }
            return ret;
        }
        #endregion Calc
        #region InitialMarginCalculation
        public void InitialMarginCalculation(Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLegInfo)
        {
            #region InitialMargin
            // EG 20150309 POC - BERKELEY Add SpreadSchedule
            initialMarginSpecified = pReturnLegInfo.First.RateOfReturn.MarginRatioSpecified;
            if (initialMarginSpecified)
            {
                IMarginRatio _marginRatio = pReturnLegInfo.First.RateOfReturn.MarginRatio;
                switch (_marginRatio.PriceExpression)
                {
                    case PriceExpressionEnum.PercentageOfNotional:
                        IReturnSwapLeg _returnSwapLeg = (IReturnSwapLeg)pReturnLegInfo.First;
                        initialMargin = _returnSwapLeg.CreateMoney;
                        decimal amount = _marginRatio.Amount.DecValue;
                        if (_marginRatio.SpreadScheduleSpecified)
                            amount += _marginRatio.SpreadSchedule.InitialValue.DecValue;
                        initialMargin.Amount = new EFS_Decimal(amount * NotionalAmount.DecValue);
                        initialMargin.Currency = NotionalCurrency;
                        break;
                    case PriceExpressionEnum.AbsoluteTerms:
                        initialMargin = ((IReturnSwapLeg)pReturnLegInfo.First).CreateMoney;
                        initialMargin.Amount = new EFS_Decimal(_marginRatio.Amount.DecValue);
                        initialMargin.Currency = _marginRatio.CurrencySpecified?_marginRatio.Currency.Value:NotionalCurrency;
                        break;
                }
            }
            #endregion InitialMargin
        }
        #endregion InitialMarginCalculation
        #region ValuationRulesCalculation
        private Cst.ErrLevel ValuationRulesCalculation(List<EFS_ReturnLegValuationPeriod> pList, IEquityValuation pEquityValuation)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pEquityValuation.ValuationDateSpecified)
            {
                IAdjustableDateOrRelativeDateSequence _valuationDate = pEquityValuation.ValuationDate;
                if (_valuationDate.AdjustableDateSpecified)
                    ret = CalculationPeriod(pList,_valuationDate.AdjustableDate);
                else if (_valuationDate.RelativeDateSequenceSpecified)
                    ret = CalculationPeriod(pList, _valuationDate.RelativeDateSequence);
            }
            else if (pEquityValuation.ValuationDatesSpecified)
            {
                IAdjustableRelativeOrPeriodicDates _valuationDates = pEquityValuation.ValuationDates;
                if (_valuationDates.AdjustableDatesSpecified)
                    ret = CalculationPeriod(pList, _valuationDates.AdjustableDates);
                else if (_valuationDates.RelativeDateSequenceSpecified)
                    ret = CalculationPeriod(pList, _valuationDates.RelativeDateSequence);
                else if (_valuationDates.PeriodicDatesSpecified)
                    ret = CalculationPeriod(pList, _valuationDates.PeriodicDates);
            }
            return ret;
        }
        #endregion ValuationRulesCalculation


        #region CalculationPeriod
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected Cst.ErrLevel PaymentPeriod<T>(List<EFS_ReturnLegValuationPeriod> pValuationPeriods, T pElementDates)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (pElementDates is IAdjustableOrRelativeDates)
            {
                IAdjustableOrRelativeDates _elementDates = pElementDates as IAdjustableOrRelativeDates;
                if (_elementDates.AdjustableDatesSpecified)
                {
                }
                else if (_elementDates.RelativeDatesSpecified)
                {
                    pValuationPeriods.ForEach(valuation =>
                    {
                        DateTime _endPeriod = valuation.valuationPeriod.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;
                        if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(m_Cs, _elementDates.RelativeDates, _endPeriod, out DateTime[] offsetDates, m_DataDocument))
                        {
                            foreach (DateTime _offsetDate in offsetDates)
                            {
                                valuation.paymentDate = new EFS_AdjustableDate(m_Cs, _offsetDate, _elementDates.RelativeDates.GetAdjustments, m_DataDocument);
                            }
                        }
                    });
                }
            }
            else if (pElementDates is IAdjustableOrRelativeDate)
            {
                IAdjustableOrRelativeDate _elementDates = pElementDates as IAdjustableOrRelativeDate;
                if (_elementDates.AdjustableDateSpecified)
                {
                }
                else if (_elementDates.RelativeDateSpecified)
                {
                    pValuationPeriods.ForEach(valuation =>
                    {
                        DateTime _endPeriod = valuation.valuationPeriod.adjustableDate2.AdjustableDate.UnadjustedDate.DateValue;
                        if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(m_Cs, _elementDates.RelativeDate, _endPeriod, out DateTime[] offsetDates, m_DataDocument))
                        {
                            foreach (DateTime _offsetDate in offsetDates)
                            {
                                valuation.paymentDate = new EFS_AdjustableDate(m_Cs, _offsetDate, _elementDates.RelativeDate.GetAdjustments, m_DataDocument);
                            }
                        }
                    });
                }
            }
            return ret;
        }
        #endregion CalculationPeriod
        #endregion Methods
    }
    #endregion EFS_RateOfReturn
    #region EFS_ReturnLegValuationPeriod
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_ReturnLegValuationPeriod
    {
        #region Members
        public EFS_AdjustablePeriod valuationPeriod;
        public EFS_AdjustableDate paymentDate;
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
                    DateValue = valuationPeriod.adjustableDate2.adjustedDate.DateValue
                };
                return dt;
            }
        }
        #endregion AdjustedEndPeriod
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = paymentDate.adjustedDate.DateValue
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
                    DateValue = valuationPeriod.adjustableDate1.adjustedDate.DateValue
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
                return new EFS_EventDate(valuationPeriod.adjustableDate2);
            }
        }
        #endregion EndPeriod
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get
            {
                return new EFS_EventDate(valuationPeriod.adjustableDate1);
            }
        }
        #endregion StartPeriod
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {
                return new EFS_EventDate(paymentDate);
            }
        }
        #endregion PaymentDate
        #endregion Accessors
        #region Constructors
        public EFS_ReturnLegValuationPeriod(EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate) 
        {
            valuationPeriod = new EFS_AdjustablePeriod
            {
                adjustableDate1 = (EFS_AdjustableDate)pStartDate.Clone(),
                adjustableDate2 = (EFS_AdjustableDate)pEndDate.Clone()
            };
        }
        #endregion Constructors
    }
    #endregion EFS_ReturnLegValuationPeriod

    #region EFS_ReturnSwap
    // EG 20231127 [WI755] Implementation Return Swap : Add members
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_ReturnSwap
    {
        #region Members
        private readonly string m_Cs;
        protected Cst.ErrLevel m_ErrLevel = Cst.ErrLevel.UNDEFINED;
        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        // EG 20141029 New
        public IReference dealerPartyReference;
        // EG 20141029 New
        public IReference custodianPartyReference;
        public DateTime tradeDate;
        public DateTime clearingBusinessDate;
        public bool isPositionKeeping;
        public bool isMargining;
        public bool isFunding;
        public bool isFungible;
        #endregion Members

        #region Accessors
        #region AdjustedClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedClearingBusinessDate
        {
            get
            {
                EFS_Date adjustedDate = new EFS_Date
                {
                    DateValue = clearingBusinessDate.Date
                };
                return adjustedDate;
            }
        }
        #endregion AdjustedClearingBusinessDate
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string BuyerPartyReference
        {
            get
            {
                return buyerPartyReference.HRef;
            }
        }
        #endregion BuyerPartyReference
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return new EFS_EventDate(clearingBusinessDate.Date, clearingBusinessDate.Date); }
        }
        #endregion ClearingBusinessDate
        #region CustodianPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CustodianPartyReference
        {
            get
            {
                return custodianPartyReference.HRef;
            }
        }
        #endregion CustodianPartyReference
        #region DealerPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string DealerPartyReference
        {
            get
            {
                return dealerPartyReference.HRef;
            }
        }
        #endregion DealerPartyReference
        #region ErrLevel
        public Cst.ErrLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #region InitialMarginPayerPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                return (null != dealerPartyReference) ? dealerPartyReference.HRef : buyerPartyReference.HRef;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                return (null != custodianPartyReference) ? custodianPartyReference.HRef : buyerPartyReference.HRef;
            }
        }
        #endregion InitialMarginReceiverPartyReference
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SellerPartyReference
        {
            get
            {
                return sellerPartyReference.HRef;
            }
        }
        #endregion SellerPartyReference
        #endregion Accessors

        #region Constructors
        public EFS_ReturnSwap(string pConnectionString, IReturnSwap pReturnSwap, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            m_Cs = pConnectionString;
            Calc(pReturnSwap, pDataDocument, pStatusBusiness);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchangeTradedDerivative"></param>
        /// <param name="pDataDocument"></param>
        private void Calc(IReturnSwap pReturnSwap, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {

            ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(m_Cs, null, pReturnSwap, pDataDocument);
            _returnSwapContainer.InitRptSide(m_Cs, pStatusBusiness == Cst.StatusBusiness.ALLOC);

            // EG 20231127 [WI755] Implementation Return Swap : New
            _ = _returnSwapContainer.GetInstrument(CSTools.SetCacheOn(m_Cs), out SQL_Instrument _sql_Instrument);
            if (null != _sql_Instrument)
            {
                isMargining = _sql_Instrument.IsMargining;
                isFunding = _sql_Instrument.IsFunding;
                isFungible = _sql_Instrument.IsFungible;
            }


            // Buyer / Seller
            IFixParty buyer = _returnSwapContainer.GetBuyerSeller(SideEnum.Buy);
            IFixParty seller = _returnSwapContainer.GetBuyerSeller(SideEnum.Sell);
            buyerPartyReference = buyer.PartyId;
            sellerPartyReference = seller.PartyId;


            isPositionKeeping = false;
            if (pStatusBusiness == Cst.StatusBusiness.ALLOC)
            {
                isPositionKeeping = _returnSwapContainer.IsPosKeepingOnBookDealer(m_Cs);
                IFixParty _dealerParty = _returnSwapContainer.GetDealer();
                if (null != _dealerParty)
                    dealerPartyReference = _dealerParty.PartyId;
                IFixParty _custodianParty = _returnSwapContainer.GetClearerCustodian();
                if (null != _custodianParty)
                    custodianPartyReference = _custodianParty.PartyId;
            }

            // Trade, Business and Maturity Dates
            tradeDate = _returnSwapContainer.TradeDate;
            clearingBusinessDate = _returnSwapContainer.ClearingBusinessDate;
            m_ErrLevel = Cst.ErrLevel.SUCCESS;
        }

        #endregion Methods
    }
    #endregion EFS_ReturnSwap
    #region EFS_ReturnSwapLeg
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_ReturnSwapLeg
    {
        #region Members
        protected string m_Cs;
        protected DataDocumentContainer m_DataDocument;
        public EFS_AdjustableDate effectiveDateAdjustment;
        public EFS_AdjustableDate terminationDateAdjustment;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney notional;
        public string notionalId;
        public string notionalHRef;
        #endregion Members
        #region Accessors
        #region AdjustedEffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedEffectiveDate
        {
            get
            {
                if (null != effectiveDateAdjustment)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = effectiveDateAdjustment.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;
            }
        }
        #endregion AdjustedEffectiveDate
        #region AdjustedTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedTerminationDate
        {
            get
            {
                if (null != terminationDateAdjustment)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = terminationDateAdjustment.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;
            }
        }
        #endregion AdjustedTerminationDate
        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EffectiveDate
        {
            get
            {
                return new EFS_EventDate(effectiveDateAdjustment);
            }
        }
        #endregion EffectiveDate
        #region IsOpen
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsOpen
        {
            get
            {
                return (terminationDateAdjustment.adjustableDateSpecified &&
                    terminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue == DateTime.MaxValue.Date);
            }
        }
        #endregion EffectiveDate
        #region NotionalAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal NotionalAmount
        {
            get
            {
                return notional.Amount;
            }
        }
        #endregion NotionalAmount
        #region NotionalCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string NotionalCurrency
        {
            get { return notional.Currency; }
        }
        #endregion NotionalCurrency

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return payerPartyReference.HRef;
            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference.HRef;
            }
        }
        #endregion ReceiverPartyReference
        #region TerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate TerminationDate
        {
            get
            {
                return new EFS_EventDate(terminationDateAdjustment);
            }
        }
        #endregion TerminationDate

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ReturnSwapLeg(string pConnectionString, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
        }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ReturnSwapLeg(string pConnectionString, IReturnSwapLeg pReturnSwapLeg, IReturnSwapNotional pReturnSwapNotional, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pDataDocument)
        {
            InitMembers(pReturnSwapLeg, pReturnSwapNotional);
        }
        #endregion Constructors
        #region Methods
        #region AddToCalculationPeriod

        protected virtual void AddToCalculationPeriod<T>(List<T> pList, EFS_AdjustableDate pStartDate)
        {
            AddToCalculationPeriod(pList, pStartDate, pStartDate);
        }
        protected virtual void AddToCalculationPeriod<T>(List<T> pList, EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate) { }
        protected virtual void AddToCalculationPeriod<T>(List<T> pList, EFS_AdjustableDate pStartDate, EFS_AdjustableDate pEndDate, StubEnum pStub, RateTypeEnum pStubRateType) { }
        #endregion AddToValuationPeriod
        #region CalculationPeriod
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected Cst.ErrLevel CalculationPeriod<T1, T2>(List<T1> pList, T2 pElementDates)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pElementDates is IAdjustableDate)
            {
                #region AdjustableDate
                IAdjustableDate _elementDates = pElementDates as IAdjustableDate;
                EFS_AdjustableDate _adjustableDate = new EFS_AdjustableDate(m_Cs, _elementDates.UnadjustedDate.DateValue, 
                    _elementDates.DateAdjustments, m_DataDocument);
                AddToCalculationPeriod(pList, _adjustableDate);
                #endregion AdjustableDate
            }
            else if (pElementDates is IAdjustableDates)
            {
                #region AdjustableDates
                IAdjustableDates _elementDates = pElementDates as IAdjustableDates;
                foreach (IAdjustedDate _unadjustedDate in _elementDates.UnadjustedDate)
                {
                    EFS_AdjustableDate _adjustableDate = new EFS_AdjustableDate(m_Cs, _unadjustedDate.DateValue, 
                        _elementDates.DateAdjustments, m_DataDocument);
                    AddToCalculationPeriod(pList, _adjustableDate);
                }
                #endregion AdjustableDates
            }
            else if (pElementDates is IRelativeDates)
            {
                #region RelativeDates
                IRelativeDates _elementDates = pElementDates as IRelativeDates;
                string hRef = ((IRelativeDateOffset)_elementDates).DateRelativeToValue;
                object objRef = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, hRef);
                if (objRef is IAdjustableRelativeOrPeriodicDates)
                {
                    IAdjustableRelativeOrPeriodicDates _obj = objRef as IAdjustableRelativeOrPeriodicDates;
                    if (_obj.AdjustableDatesSpecified)
                        ret = CalculationPeriod(pList, _obj.AdjustableDates);
                    else if (_obj.RelativeDateSequenceSpecified)
                        ret = CalculationPeriod(pList, _obj.RelativeDateSequence);
                    else if (_obj.PeriodicDatesSpecified)
                        ret = CalculationPeriod(pList, _obj.PeriodicDates);
                }

                #endregion RelativeDates
            }
            else if (pElementDates is IRelativeDateSequence)
            {
                #region RelativeDateSequence
                IRelativeDateSequence _elementDates = pElementDates as IRelativeDateSequence;
                IBusinessDayAdjustments _bda = _elementDates.GetAdjustments;
                if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(m_Cs, _elementDates, out DateTime[] offsetDates, m_DataDocument))
                {
                    foreach (DateTime _offsetDate in offsetDates)
                    {
                        EFS_AdjustableDate _adjustableDate = new EFS_AdjustableDate(m_Cs, _offsetDate, _bda, m_DataDocument);
                        AddToCalculationPeriod(pList, _adjustableDate);
                    }
                }
                #endregion RelativeDateSequence
            }
            else if (pElementDates is IPeriodicDates)
            {
                #region PeriodicDates
                IPeriodicDates _elementDates = pElementDates as IPeriodicDates;
                ICalculationPeriodFrequency calculationPeriodFrequency = _elementDates.CalculationPeriodFrequency;

                EFS_AdjustableDate startDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _elementDates.CalculationStartDate, m_DataDocument);
                EFS_AdjustableDate endDateAdjustment = (EFS_AdjustableDate)terminationDateAdjustment.Clone();

                if (_elementDates.CalculationEndDateSpecified)
                {
                    EFS_AdjustableDate calculationEndDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _elementDates.CalculationEndDate, m_DataDocument);
                    if (calculationEndDateAdjustment.AdjustableDate.UnadjustedDate.DateValue < terminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue)
                        endDateAdjustment = calculationEndDateAdjustment;
                }

                _ = new EFS_AdjustableDate(m_Cs, m_DataDocument);
                _ = new EFS_AdjustableDate(m_Cs, m_DataDocument);
                EFS_AdjustableDate _adjustableDate2 = (EFS_AdjustableDate)startDateAdjustment.Clone();

                bool _isOneDayPeriod = (calculationPeriodFrequency.Interval.Period == PeriodEnum.D) && (calculationPeriodFrequency.Interval.PeriodMultiplier.IntValue == 1);
                bool _isOpenDailyPeriod = IsOpen && (calculationPeriodFrequency.Interval.Period == PeriodEnum.D);
                if (_adjustableDate2.adjustableDateSpecified && (false == _isOpenDailyPeriod))
                {
                    int guard = 0;
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
                        EFS_AdjustableDate _adjustableDate1 = (EFS_AdjustableDate)_adjustableDate2.Clone();
                        _adjustableDate2 = Tools.ApplyInterval(_adjustableDate1, endDateAdjustment.AdjustableDate.UnadjustedDate.DateValue, calculationPeriodFrequency.Interval);
                        _adjustableDate2 = Tools.ApplyRollConvention(_adjustableDate2, startDateAdjustment.AdjustableDate.UnadjustedDate.DateValue,
                                calculationPeriodFrequency.RollConvention);

                        if (_adjustableDate2.AdjustableDate.UnadjustedDate.DateValue >= endDateAdjustment.AdjustableDate.UnadjustedDate.DateValue)
                        {
                            _adjustableDate2 = new EFS_AdjustableDate(m_Cs, endDateAdjustment.AdjustableDate.UnadjustedDate.DateValue, 
                                _elementDates.CalculationPeriodDatesAdjustments, m_DataDocument);
                            AddToCalculationPeriod(pList, _adjustableDate1, _adjustableDate2);
                            break;
                        }
                        else
                        {
                            _adjustableDate2.AdjustableDate.DateAdjustments = (IBusinessDayAdjustments)_elementDates.CalculationPeriodDatesAdjustments.Clone();
                            AddToCalculationPeriod(pList, _adjustableDate1, _adjustableDate2);

                            if (_isOneDayPeriod)
                                _adjustableDate2.AdjustableDate.UnadjustedDate.DateValue = _adjustableDate2.adjustedDate.DateValue;

                            continue;
                        }
                    }
                }
                #endregion PeriodicDates
            }
            return ret;
        }
        #endregion CalculationPeriod
        #region InitMembers
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void InitMembers(IReturnSwapLeg pReturnSwapLeg, IReturnSwapNotional pReturnSwapNotional)
        {
            #region Payer / Receiver
            payerPartyReference = pReturnSwapLeg.PayerPartyReference;
            receiverPartyReference = pReturnSwapLeg.ReceiverPartyReference;
            #endregion Payer / Receiver

            #region NotionalAmount
            notionalId = pReturnSwapNotional.Id;
            if (pReturnSwapNotional.RelativeToSpecified)
            {
                Tools.AmountRelativeTo(m_Cs, pReturnSwapNotional, out notional, m_DataDocument);
                notionalHRef = pReturnSwapNotional.RelativeTo.HRef;
            }
            else if (pReturnSwapNotional.NotionalAmountSpecified)
                notional = pReturnSwapNotional.NotionalAmount.Clone();
            else if (pReturnSwapNotional.DeterminationMethodSpecified)
                notional = null; // TODO
            #endregion NotionalAmount
        }
        #endregion InitMembers
        #endregion Methods
    }
    #endregion EFS_ReturnSwapLeg
    #region EFS_ReturnSwapLegUnderlyer
    /// EG 20150302 Add EventNotionalBase|EventTypeInitialAmount (CFD Forex)
    // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
    public class EFS_ReturnSwapLegUnderlyer : EFS_ReturnSwapLeg
    {
        #region Members
        public EFS_Basket basket;
        public bool basketSpecified;
        public EFS_SingleUnderlyer singleUnderlyer;
        public bool singleUnderlyerSpecified;
        #endregion Members
        #region Accessors
        #region Asset
        public EFS_Asset Asset
        {
            get { return Underlyer.asset; }
        }
        #endregion Asset
        #region EventDateNotionalBase
        /// <summary>
        /// Retourne les dates START|END pour EVENT de type BCU
        /// </summary>
        /// <param name="pAmountType"></param>
        /// <returns></returns>
        public EFS_EventDate EventDateNotionalBase(EFS_EventDate pDefaultDate)
        {
            EFS_EventDate eventDate = new EFS_EventDate(pDefaultDate.unadjustedDate.DateValue, pDefaultDate.adjustedDate.DateValue);
            if (Asset.assetCategory.HasValue && (Asset.assetCategory.Value != Cst.UnderlyingAsset.FxRateAsset))
                eventDate = null;
            return eventDate;
        }
        #endregion EventDateInitialAmount
        #region EventTypeInitialAmount
        /// <summary>
        /// Retourne le type d'événement pour les événements IninitalValuation en fonction du type de sous-jacent du ReturnLeg
        /// </summary>
        /// <param name="pAmountType"></param>
        /// <returns></returns>
        public string EventTypeInitialAmount(string pAmountType)
        {
            string eventType = string.Empty;
            switch (pAmountType)
            {
                case "Notional":
                    eventType = EventTypeFunc.Nominal;
                    if (Asset.assetCategory.HasValue &&
                        (Asset.assetCategory.Value == Cst.UnderlyingAsset.FxRateAsset))
                        eventType = EventTypeFunc.QuotedCurrency;
                    break;
                case "NotionalBase":
                    eventType = EventTypeFunc.BaseCurrency;
                    break;
                case "Quantity":
                    eventType = EventTypeFunc.Quantity;
                    break;
                case "MarginRequirementRatio":
                    eventType = EventTypeFunc.MarginRequirementRatio;
                    break;

            }
            return eventType;
        }
        #endregion UnderlyerEventCode
        #region OpenUnits
        public EFS_Decimal OpenUnits
        {
            get { return Underlyer.OpenUnits; }
        }
        #endregion OpenUnits
        #region Underlyer
        public EFS_Underlyer Underlyer
        {
            get
            {
                EFS_Underlyer _underlyer = null;
                if (basketSpecified)
                    _underlyer = (EFS_Underlyer)basket;
                else if (singleUnderlyerSpecified)
                    _underlyer = (EFS_Underlyer)singleUnderlyer;
                return _underlyer;
            }
        }
        #endregion Underlyer
        #region UnderlyerEventCode
        public string UnderlyerEventCode
        {
            get
            {
                return Underlyer.UnderlyerEventCode;
            }
        }
        #endregion UnderlyerEventCode
        #region UnderlyerEventType
        public string UnderlyerEventType
        {
            get { return Underlyer.UnderlyerEventType; }
        }
        #endregion UnderlyerEventType
        #region Unit
        public string Unit
        {
            get { return Underlyer.Unit; }
        }
        #endregion Unit
        #region UnitType
        public string UnitType
        {
            get { return Underlyer.UnitType; }
        }
        #endregion UnitType
        #region ValuationEventCode
        public string ValuationEventCode
        {
            get
            {
                return Underlyer.ValuationEventCode;
            }
        }
        #endregion ValuationEventCode
        #region ValuationEventType
        public string ValuationEventType
        {
            get { return Underlyer.ValuationEventType; }
        }
        #endregion ValuationEventType
        #endregion Accessors
        #region Constructors
        // EG 20140904 Add AssetCategory
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ReturnSwapLegUnderlyer(string pCS, DataDocumentContainer pDataDocument) : base(pCS, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_ReturnSwapLegUnderlyer(string pCS, Pair<IReturnLeg, IReturnLegMainUnderlyer> pReturnLegInfo, DataDocumentContainer pDataDocument) //, bool pIsOnlyNextValuation)
            : base(pCS, (IReturnSwapLeg)pReturnLegInfo.First, pReturnLegInfo.First.Notional, pDataDocument) //, pIsOnlyNextValuation)
        {
            IReturnLeg _leg = pReturnLegInfo.First;

            #region EffectiveDate / TerminationDate
            effectiveDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _leg.EffectiveDate, m_DataDocument);
            terminationDateAdjustment = Tools.GetEFS_AdjustableDate(m_Cs, _leg.TerminationDate, m_DataDocument);
            #endregion EffectiveDate / TerminationDate
            #region Underlyer
            singleUnderlyerSpecified = _leg.Underlyer.UnderlyerSingleSpecified;
            basketSpecified = _leg.Underlyer.UnderlyerBasketSpecified;
            if (singleUnderlyerSpecified)
                singleUnderlyer = new EFS_SingleUnderlyer(pCS, _leg.PayerPartyReference, _leg.ReceiverPartyReference, _leg.Underlyer.UnderlyerSingle);
            if (basketSpecified)
                basket = new EFS_Basket(pCS, _leg.PayerPartyReference, _leg.ReceiverPartyReference, _leg.Underlyer.UnderlyerBasket);
            #endregion Underlyer
            #region EFS_Asset
            if ((null != pReturnLegInfo.Second) && pReturnLegInfo.Second.SqlAssetSpecified)
            {
                SQL_AssetBase _asset = pReturnLegInfo.Second.SqlAsset;
                EFS_Underlyer _unl = Underlyer;
                _unl.asset = new EFS_Asset
                {
                    idAsset = _asset.Id,
                    idC = _asset.IdC,
                    IdMarket = _asset.IdM,
                    IdMarketFIXML_SecurityExchange = _asset.Market_FIXML_SecurityExchange,
                    description = _asset.Description,
                    idBC = _asset.Market_IDBC,
                    assetCategory = _asset.AssetCategory
                };

                if (_asset is SQL_AssetEquity)
                {
                    SQL_AssetEquity _assetEquity = _asset as SQL_AssetEquity;
                    _unl.asset.assetSymbol = _assetEquity.AssetSymbol;
                    _unl.asset.isinCode = _assetEquity.ISINCode;
                }
                else if (_asset is SQL_AssetIndex)
                {
                    SQL_AssetIndex _assetIndex = _asset as SQL_AssetIndex;
                    _unl.asset.assetSymbol = _assetIndex.AssetSymbol;
                }
                else if (_asset is SQL_AssetFxRate)
                {
                    // TO DO
                }
            }
            #endregion EFS_Asset

        }
        #endregion Constructors
    }
    #endregion EFS_ReturnSwapLegUnderlyer

    #region EFS_VarianceLeg
    public class EFS_VarianceLeg : EFS_ReturnSwapLeg
    {
        #region Members
        #endregion Members
        #region Accessors
        #endregion Accessors

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_VarianceLeg(string pConnectionString, IVarianceLeg pVarianceLeg, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pDataDocument)
        {
            payerPartyReference = pVarianceLeg.PayerPartyReference;
            receiverPartyReference = pVarianceLeg.ReceiverPartyReference;
        }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }
    #endregion EFS_VarianceLeg
}

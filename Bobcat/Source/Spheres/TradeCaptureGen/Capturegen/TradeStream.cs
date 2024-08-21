#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// 
    /// </summary>
    public class TradeStreamTools
    {
        #region Methods
        /// <summary>
        /// Injecte un enregistrement dans TRADESTREAM à partir de IFxLeg
        /// </summary>
        public static void InsertStreamFxSingleLeg(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, int pStreamNo, IFxLeg pLeg)
        {
            TradeStreamInfo tradeStreamInfo = new TradeStreamInfo(pIdT, pInstrumentNo, pStreamNo);
            //
            IPayment payment1 = pLeg.ExchangedCurrency1;
            IPayment payment2 = pLeg.ExchangedCurrency2;
            //
            tradeStreamInfo.IdC1 = payment1.PaymentCurrency;
            tradeStreamInfo.IdC2 = payment2.PaymentCurrency;
            tradeStreamInfo.SetIdParty(pTradeCommonInput, payment1.PayerPartyReference.HRef, payment1.ReceiverPartyReference.HRef);
            tradeStreamInfo.Insert(pCS, pDbTransaction);

        }

        /// <summary>
        /// Injecte n enregistrements dans de TRADESTREAM à partir de IFxLeg[]
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pLegs"></param>
        public static void InsertStreamFxSingleLegs(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, ref int pStreamNo, IFxLeg[] pLegs)
        {
            int streamNo = pStreamNo;
            foreach (IFxLeg leg in pLegs)
            {
                // RD 20110523 Bug sur l'insertion du Stream 
                // Correction: remplacer streamNo par pInstrumentNo
                InsertStreamFxSingleLeg(pCS, pDbTransaction, pTradeCommonInput, pIdT, pInstrumentNo, streamNo, leg);
                streamNo++;
            }
            pStreamNo = streamNo;

        }

        /// <summary>
        /// Injecte un enregistrement dans TRADESTREAM à partir de IReturnSwapLeg
        /// </summary>
        public static void InsertStreamReturnSwapLeg(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, int pLegNo, IReturnSwapLeg pReturnSwapLeg)
        {
            TradeStreamInfo tradeStreamInfo = new TradeStreamInfo(pIdT, pInstrumentNo, pLegNo);
            tradeStreamInfo.SetIdParty(pTradeCommonInput, pReturnSwapLeg.PayerPartyReference.HRef, pReturnSwapLeg.ReceiverPartyReference.HRef);
            tradeStreamInfo.Insert(pCS, pDbTransaction);

        }

        /// <summary>
        /// Injecte n enregistrements dans de TRADESTREAM à partir de IReturnSwapLeg[]
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pLegs"></param>
        public static void InsertStreamReturnSwapLegs(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, ref int pLegNo, IReturnSwapLeg[] pReturnSwapLegs)
        {
            int legNo = pLegNo;
            foreach (IReturnSwapLeg leg in pReturnSwapLegs)
            {
                InsertStreamReturnSwapLeg(pCS, pDbTransaction, pTradeCommonInput, pIdT, pInstrumentNo, legNo, leg);
                legNo++;
            }
            pLegNo = legNo;

        }

        

        /// <summary>
        /// Injecte un enregistrement dans TRADESTREAM à partir de IPayment
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pPayment"></param>
        public static void InsertStreamPayment(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, int pStreamNo, IPayment pPayment)
        {
            InsertStreamShortForm(pCS, pDbTransaction, pTradeCommonInput, pIdT, pInstrumentNo, pStreamNo, pPayment.PayerPartyReference, pPayment.ReceiverPartyReference, pPayment.PaymentCurrency, null);

        }

        /// <summary>
        /// Injecte n enregistrements dans de TRADESTREAM à partir de IPayment[]
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pPayment"></param>
        public static void InsertStreamPayments(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, ref int pStreamNo, IPayment[] pPayment)
        {
            int streamNo = pStreamNo;
            foreach (IPayment payment in pPayment)
            {
                InsertStreamPayment(pCS, pDbTransaction, pTradeCommonInput, pIdT, pInstrumentNo, streamNo, payment);
                streamNo++;
            }
            pStreamNo = streamNo;

        }

        /// <summary>
        /// Injecte 1 enregistrement dans TRADESTREAM à partir de ISImplePayment
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pPayment"></param>
        public static void InsertStreamSimplePayment(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, int pStreamNo, ISimplePayment pPayment)
        {
            InsertStreamShortForm(pCS, pDbTransaction, pTradeCommonInput, pIdT, pInstrumentNo, pStreamNo, pPayment.PayerPartyReference, pPayment.ReceiverPartyReference, pPayment.PaymentAmount.Currency, null);

        }

        /// <summary>
        /// Injecte n enregistrements dans TRADESTREAM à partir de ISImplePayment[]
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pPayment"></param>
        public static void InsertStreamSimplePayments(string pCs, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, ref int pStreamNo, ISimplePayment[] pPayment)
        {
            int streamNo = pStreamNo;
            foreach (ISimplePayment payment in pPayment)
            {
                InsertStreamSimplePayment(pCs, pDbTransaction, pTradeCommonInput, pIdT, pInstrumentNo, streamNo, payment);
                streamNo++;
            }
            pStreamNo = streamNo;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pStreams"></param>
        /// <param name="pIsUpdateOnly_TradeStream"></param>
        /// 20081015 EG Insertion IDC2 = Devise de référence pour FxLinked
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static void InsertInterestRateStreams(string pCS, IDbTransaction pDbTransaction,
            TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, ref int pStreamNo, IInterestRateStream[] pStreams)
        {
            int streamNo = pStreamNo;
            TradeStreamInfo tradeStreamInfo = new TradeStreamInfo(pIdT, pInstrumentNo, streamNo);
            foreach (IInterestRateStream stream in pStreams)
            {
                string currencyNotionalScheduleReference = string.Empty;
                ICalculationPeriodAmount cpa = stream.CalculationPeriodAmount;
                if (cpa.CalculationSpecified && cpa.Calculation.FxLinkedNotionalSpecified)
                {
                    string hRef = cpa.Calculation.FxLinkedNotional.ConstantNotionalScheduleReference.HRef;
                    object notionalReference = ReflectionTools.GetObjectById(pStreams, hRef);
                    if ((null != notionalReference) && Tools.IsTypeOrInterfaceOf(notionalReference, InterfaceEnum.INotional))
                        currencyNotionalScheduleReference = ((INotional)notionalReference).StepSchedule.Currency.Value;
                }
                InsertInterestRateStream(pCS, pDbTransaction, pTradeCommonInput, tradeStreamInfo, streamNo, stream, currencyNotionalScheduleReference);
                streamNo++;
            }
            pStreamNo = streamNo;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="pTradeStreamInfo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pStream"></param>
        /// <param name="pIsUpdateOnly_TradeStream"></param>
        /// <param name="pIdC2"></param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static void InsertInterestRateStream(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, TradeStreamInfo pTradeStreamInfo, int pStreamNo, IInterestRateStream pStream, string pIdC2)
        {
            GetStreamStepInfo(pStream, pTradeStreamInfo);
            pTradeStreamInfo.StreamNo = pStreamNo;
            pTradeStreamInfo.IdC1 = pStream.StreamCurrency;
            #region FxLinked case (IDC2 = Reference currency for the fxlinked swap)
            if (StrFunc.IsFilled(pIdC2))
                pTradeStreamInfo.IdC2 = pIdC2;
            #endregion FxLinked case (IDC2 = Reference currency for the fxlinked swap)
            pTradeStreamInfo.SetIdParty(pTradeCommonInput, pStream.PayerPartyReference.HRef, pStream.ReceiverPartyReference.HRef);
            pTradeStreamInfo.Insert(pCS, pDbTransaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStream"></param>
        /// <param name="pTradeStreamInfo"></param>
        private static void GetStreamStepInfo(IInterestRateStream pStream, TradeStreamInfo pTradeStreamInfo)
        {

            #region isPrincipalExchange
            pTradeStreamInfo.IsPrincipalExchange = pStream.IsInitialExchange || pStream.IsIntermediateExchange || pStream.IsFinalExchange;
            #endregion isPrincipalExchange

            #region isNotionalStep / isMultiplierStep / isSpreadStep / isFixedRateStep / isStrikeStep / isAmountStep
            pTradeStreamInfo.IsNotionalStep = false;
            pTradeStreamInfo.IsMultiplierStep = false;
            pTradeStreamInfo.IsSpreadStep = false;
            pTradeStreamInfo.IsFixedRateStep = false;
            pTradeStreamInfo.IsStrikeStep = false;
            pTradeStreamInfo.IsAmountStep = false;
            if (pStream.CalculationPeriodAmount.CalculationSpecified)
            {
                #region isNotionalStep
                ICalculation calculation = pStream.CalculationPeriodAmount.Calculation;
                IFloatingRate rate = null;
                if (calculation.RateFloatingRateSpecified)
                    rate = calculation.RateFloatingRate;
                else if (calculation.RateInflationRateSpecified)
                    rate = calculation.RateInflationRate;

                if (calculation.NotionalSpecified)
                {
                    pTradeStreamInfo.IsNotionalStep = calculation.Notional.StepSchedule.StepSpecified;
                    pTradeStreamInfo.IsNotionalStep |= calculation.Notional.StepParametersSpecified;
                }
                #endregion isNotionalStep
                #region isMultiplierStep
                pTradeStreamInfo.IsMultiplierStep = (null != rate) &&
                                                    (rate.FloatingRateMultiplierScheduleSpecified) &&
                                                    (rate.FloatingRateMultiplierSchedule.StepSpecified);
                #endregion isMultiplierStep
                #region isSpreadStep
                pTradeStreamInfo.IsSpreadStep = (null != rate) && (rate.SpreadScheduleSpecified) && (rate.SpreadSchedule.StepSpecified);
                #endregion
                #region isFixedRateStep
                pTradeStreamInfo.IsFixedRateStep = calculation.RateFixedRateSpecified && calculation.RateFixedRate.StepSpecified;
                #endregion isFixedRateStep
                #region isStrikeStep
                if (null != rate)
                {
                    if (rate.CapRateScheduleSpecified)
                    {
                        #region capRateSchedule
                        foreach (IStrikeSchedule schedule in rate.CapRateSchedule)
                        {
                            pTradeStreamInfo.IsStrikeStep = schedule.StepSpecified;
                            if (pTradeStreamInfo.IsStrikeStep)
                                break;
                        }
                        #endregion capRateSchedule
                    }
                    if ((false == pTradeStreamInfo.IsStrikeStep) && rate.FloorRateScheduleSpecified)
                    {
                        #region floorRateSchedule
                        foreach (IStrikeSchedule schedule in calculation.RateFloatingRate.FloorRateSchedule)
                        {
                            pTradeStreamInfo.IsStrikeStep = schedule.StepSpecified;
                            if (pTradeStreamInfo.IsStrikeStep)
                                break;
                        }
                        #endregion floorRateSchedule
                    }
                }
                #endregion isStrikeStep
            }
            else if (pStream.CalculationPeriodAmount.KnownAmountScheduleSpecified)
            {
                #region isAmountStep
                pTradeStreamInfo.IsAmountStep = pStream.CalculationPeriodAmount.KnownAmountSchedule.StepSpecified;
                #endregion isAmountStep
            }
            #endregion isNotionalStep / isMultiplierStep / isSpreadStep / isFixedRateStep / isStrikeStep / isAmountStep

            ICalculationPeriodDates calculationPeriodDates = pStream.CalculationPeriodDates;
            ICalculationPeriodFrequency calculationPeriodFrequency = calculationPeriodDates.CalculationPeriodFrequency;
            IPaymentDates paymentDates = pStream.PaymentDates;
            IInterval paymentFrequency = paymentDates.PaymentFrequency;

            #region bdc_CalcPeriod / periodCalcDt / periodMltpCalcDt / rollConvCalcDt
            pTradeStreamInfo.Bdc_CalcPeriod = calculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
            pTradeStreamInfo.PeriodCalcDt = calculationPeriodFrequency.Interval.Period.ToString();
            pTradeStreamInfo.PeriodMltpCalcDt = calculationPeriodFrequency.Interval.PeriodMultiplier.IntValue;
            pTradeStreamInfo.RollConvCalcDt = EnumToString.RollConvention(calculationPeriodFrequency.RollConvention);
            #endregion bdc_CalcPeriod / periodCalcDt / periodMltpCalcDt / rollConvCalcDt

            #region bdc_Payment / periodPaymentDt / periodMltpPaymentDt
            pTradeStreamInfo.Bdc_Payment = paymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
            pTradeStreamInfo.PeriodPaymentDt = paymentFrequency.Period.ToString();
            pTradeStreamInfo.PeriodMltpPaymentDt = paymentFrequency.PeriodMultiplier.IntValue;
            #endregion bdc_Payment / periodPaymentDt / periodMltpPaymentDt

            #region firstRegularPeriodStartDate / lastRegularPeriodEndDate / firstPeriodStartDate
            pTradeStreamInfo.FirstRegularPeriodStartDate = DateTime.MinValue;
            pTradeStreamInfo.LastRegularPeriodEndDate = DateTime.MinValue;
            pTradeStreamInfo.FirstPeriodStartDate = DateTime.MinValue;
            if (calculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                pTradeStreamInfo.FirstRegularPeriodStartDate = calculationPeriodDates.FirstRegularPeriodStartDate.DateValue;
            if (calculationPeriodDates.LastRegularPeriodEndDateSpecified)
                pTradeStreamInfo.LastRegularPeriodEndDate = calculationPeriodDates.LastRegularPeriodEndDate.DateValue;
            if (calculationPeriodDates.FirstPeriodStartDateSpecified)
                pTradeStreamInfo.FirstPeriodStartDate = calculationPeriodDates.FirstPeriodStartDate.UnadjustedDate.DateValue; ;
            #endregion firstRegularPeriodStartDate / lastRegularPeriodEndDate / firstPeriodStartDate

        }

        /// <summary>
        /// Injecte un enregistrement dans TRADESTREAM à partir d'un couple payer/receiver et d'une devise
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeCommonInput"></param>
        /// <param name="pIdT"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="payerPartyReference"></param>
        /// <param name="receiverPartyReference"></param>
        /// <param name="IdC"></param>
        public static void InsertStreamShortForm(string pCS, IDbTransaction pDbTransaction, TradeCommonInput pTradeCommonInput, int pIdT, int pInstrumentNo, int pStreamNo,
                IReference payerPartyReference, IReference receiverPartyReference, string pIdC, string pIdC2)
        {

            TradeStreamInfoShortForm tradeStreamInfo = new TradeStreamInfoShortForm(pIdT, pInstrumentNo, pStreamNo)
            {
                IdC1 = pIdC,
                IdC2 = pIdC2
            };
            tradeStreamInfo.SetIdParty(pTradeCommonInput, payerPartyReference.HRef, receiverPartyReference.HRef);
            tradeStreamInfo.Insert(pCS, pDbTransaction);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class TradeStreamInfo : TradeStreamInfoShortForm
    {
        #region Members
        private string m_OptionType;
        private bool m_IsPrincipalExchange;
        private bool m_IsNotionalStep;
        private bool m_IsMultiplierStep;
        private bool m_IsSpreadStep;
        private bool m_IsFixedRateStep;
        private bool m_IsStrikeStep;
        private bool m_IsAmountStep;
        private string m_Bdc_Payment;
        private string m_Bdc_CalcPeriod;
        private string m_PeriodCalcDt;
        private string m_PeriodPaymentDt;
        private string m_RollConvCalcDt;
        private int m_PeriodMltpCalcDt;
        private int m_PeriodMltpPaymentDt;
        private DateTime m_FirstRegularPeriodStartDate;
        private DateTime m_LastRegularPeriodEndDate;
        private DateTime m_FirstPeriodStartDate;
        #endregion Members

        #region Accessors
        #region Bdc_CalcPeriod
        public string Bdc_CalcPeriod
        {
            set { m_Bdc_CalcPeriod = value; }
            get { return m_Bdc_CalcPeriod; }
        }
        #endregion Bdc_CalcPeriod
        #region Bdc_Payment
        public string Bdc_Payment
        {
            set { m_Bdc_Payment = value; }
            get { return m_Bdc_Payment; }
        }
        #endregion Bdc_Payment
        #region FirstPeriodStartDate
        public DateTime FirstPeriodStartDate
        {
            set { m_FirstPeriodStartDate = value; }
            get { return m_FirstPeriodStartDate; }
        }
        #endregion FirstPeriodStartDate
        #region FirstRegularPeriodStartDate
        public DateTime FirstRegularPeriodStartDate
        {
            set { m_FirstRegularPeriodStartDate = value; }
            get { return m_FirstRegularPeriodStartDate; }
        }
        #endregion FirstRegularPeriodStartDate
        #region IsAmountStep
        public bool IsAmountStep
        {
            set { m_IsAmountStep = value; }
            get { return m_IsAmountStep; }
        }
        #endregion IsAmountStep
        #region IsFixedRateStep
        public bool IsFixedRateStep
        {
            set { m_IsFixedRateStep = value; }
            get { return m_IsFixedRateStep; }
        }
        #endregion IsFixedRateStep
        #region IsMultiplierStep
        public bool IsMultiplierStep
        {
            set { m_IsMultiplierStep = value; }
            get { return m_IsMultiplierStep; }
        }
        #endregion IsMultiplierStep
        #region IsNotionalStep
        public bool IsNotionalStep
        {
            set { m_IsNotionalStep = value; }
            get { return m_IsNotionalStep; }
        }
        #endregion IsNotionalStep
        #region IsPrincipalExchange
        public bool IsPrincipalExchange
        {
            set { m_IsPrincipalExchange = value; }
            get { return m_IsPrincipalExchange; }
        }
        #endregion IsPrincipalExchange
        #region IsSpreadStep
        public bool IsSpreadStep
        {
            set { m_IsSpreadStep = value; }
            get { return m_IsSpreadStep; }
        }
        #endregion IsSpreadStep
        #region IsStrikeStep
        public bool IsStrikeStep
        {
            set { m_IsStrikeStep = value; }
            get { return m_IsStrikeStep; }
        }
        #endregion IsStrikeStep
        #region LastRegularPeriodEndDate
        public DateTime LastRegularPeriodEndDate
        {
            set { m_LastRegularPeriodEndDate = value; }
            get { return m_LastRegularPeriodEndDate; }
        }
        #endregion LastRegularPeriodEndDate
        #region OptionType
        public string OptionType
        {
            set { m_OptionType = value; }
            get { return m_OptionType; }
        }
        #endregion OptionType
        #region PeriodCalcDt
        public string PeriodCalcDt
        {
            set { m_PeriodCalcDt = value; }
            get { return m_PeriodCalcDt; }
        }
        #endregion PeriodCalcDt
        #region PeriodPaymentDt
        public string PeriodPaymentDt
        {
            set { m_PeriodPaymentDt = value; }
            get { return m_PeriodPaymentDt; }
        }
        #endregion PeriodPaymentDt
        #region PeriodMltpCalcDt
        public int PeriodMltpCalcDt
        {
            set { m_PeriodMltpCalcDt = value; }
            get { return m_PeriodMltpCalcDt; }
        }
        #endregion PeriodMltpCalcDt
        #region PeriodMltpPaymentDt
        public int PeriodMltpPaymentDt
        {
            set { m_PeriodMltpPaymentDt = value; }
            get { return m_PeriodMltpPaymentDt; }
        }
        #endregion PeriodMltpPaymentDt
        #region RollConvCalcDt
        public string RollConvCalcDt
        {
            set { m_RollConvCalcDt = value; }
            get { return m_RollConvCalcDt; }
        }
        #endregion RollConvCalcDt
        #endregion Accessors

        #region Constructors
        public TradeStreamInfo(int pIdT, int pInstrumentNo, int pStreamNo)
            : base(pIdT, pInstrumentNo, pStreamNo)
        {
        }
        #endregion Constructors

        #region Methods
        #region DBValue
        protected object DB_OptionType()
        {
            return StrFunc.IsFilled(m_OptionType) ? m_OptionType : Convert.DBNull;
        }
        protected object DB_Bdc_Payment()
        {
            return StrFunc.IsFilled(m_Bdc_Payment) ? m_Bdc_Payment : Convert.DBNull;
        }
        protected object DB_Bdc_CalcPeriod()
        {
            return StrFunc.IsFilled(m_Bdc_CalcPeriod) ? m_Bdc_CalcPeriod : Convert.DBNull;
        }
        protected object DB_PeriodCalcDt()
        {
            return StrFunc.IsFilled(m_PeriodCalcDt) ? m_PeriodCalcDt : Convert.DBNull;
        }
        protected object DB_PeriodPaymentDt()
        {
            return StrFunc.IsFilled(m_PeriodPaymentDt) ? m_PeriodPaymentDt : Convert.DBNull;
        }
        protected object DB_RollConvCalcDt()
        {
            return StrFunc.IsFilled(m_RollConvCalcDt) ? m_RollConvCalcDt : Convert.DBNull;
        }
        protected object DB_PeriodMltpCalcDt()
        {
            return (0 != m_PeriodMltpCalcDt) ? m_PeriodMltpCalcDt : Convert.DBNull;
        }
        protected object DB_PeriodMltpPaymentDt()
        {
            return (0 != m_PeriodMltpPaymentDt) ? m_PeriodMltpPaymentDt : Convert.DBNull;
        }
        protected object DB_FirstRegularPeriodStartDate()
        {
            return DataHelper.GetDBData(m_FirstRegularPeriodStartDate);
        }
        protected object DB_LastRegularPeriodEndDate()
        {
            return DataHelper.GetDBData(m_LastRegularPeriodEndDate);
        }
        protected object DB_FirstPeriodStartDate()
        {
            return DataHelper.GetDBData(m_FirstPeriodStartDate);
        }
        #endregion DBValue

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected override QueryParameters GetQueryUpdate(string pCS)
        {

            StrBuilder update = new StrBuilder();

            update += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRADESTREAM + SQLCst.SET + Cst.CrLf;
            update += "ISNOTIONALSTEP=@ISNOTIONALSTEP,ISMULTIPLIERSTEP=@ISMULTIPLIERSTEP,ISSPREADSTEP=@ISSPREADSTEP,";
            update += "ISFIXEDRATESTEP=@ISFIXEDRATESTEP,ISSTRIKESTEP=@ISSTRIKESTEP,ISAMOUNTSTEP=@ISAMOUNTSTEP,";
            update += "BDC_CALCPERIOD=@BDC_CALCPERIOD,PERIODMLTPCALCDT=@PERIODMLTPCALCDT,PERIODCALCDT=@PERIODCALCDT,ROLLCONVCALCDT=@ROLLCONVCALCDT,";
            update += "BDC_PAYMENT=@BDC_PAYMENT,PERIODMLTPPAYMENTDT=@PERIODMLTPPAYMENTDT,PERIODPAYMENTDT=@PERIODPAYMENTDT,";
            update += "DTFIRSTREGULPERIOD=@DTFIRSTREGULPERIOD,DTLASTREGULPERIOD=@DTLASTREGULPERIOD,DTFIRSTPERIOD=@DTFIRSTPERIOD" + Cst.CrLf;
            update += SQLCst.WHERE + "IDT=@IDT and INSTRUMENTNO=@INSTRUMENTNO and STREAMNO=@STREAMNO";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), m_IdT);
            dataParameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), m_InstrumentNo);
            dataParameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), m_StreamNo);

            dataParameters.Add(new DataParameter(pCS, "ISNOTIONALSTEP", DbType.Boolean), m_IsNotionalStep);
            dataParameters.Add(new DataParameter(pCS, "ISMULTIPLIERSTEP", DbType.Boolean), m_IsMultiplierStep);
            dataParameters.Add(new DataParameter(pCS, "ISSPREADSTEP", DbType.Boolean), m_IsSpreadStep);
            dataParameters.Add(new DataParameter(pCS, "ISFIXEDRATESTEP", DbType.Boolean), m_IsFixedRateStep);
            dataParameters.Add(new DataParameter(pCS, "ISSTRIKESTEP", DbType.Boolean), m_IsStrikeStep);
            dataParameters.Add(new DataParameter(pCS, "ISAMOUNTSTEP", DbType.Boolean), m_IsAmountStep);

            dataParameters.Add(new DataParameter(pCS, "BDC_CALCPERIOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_Bdc_CalcPeriod());
            dataParameters.Add(new DataParameter(pCS, "PERIODMLTPCALCDT", DbType.Int32), DB_PeriodMltpCalcDt());
            dataParameters.Add(new DataParameter(pCS, "PERIODCALCDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_PeriodCalcDt());
            dataParameters.Add(new DataParameter(pCS, "ROLLCONVCALCDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_RollConvCalcDt());

            dataParameters.Add(new DataParameter(pCS, "BDC_PAYMENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_Bdc_Payment());
            dataParameters.Add(new DataParameter(pCS, "PERIODMLTPPAYMENTDT", DbType.Int32), DB_PeriodMltpPaymentDt());
            dataParameters.Add(new DataParameter(pCS, "PERIODPAYMENTDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_PeriodPaymentDt());

            dataParameters.Add(new DataParameter(pCS, "DTFIRSTREGULPERIOD", DbType.Date), DB_FirstRegularPeriodStartDate()); // FI 20201006 [XXXXX] DbType.Date
            dataParameters.Add(new DataParameter(pCS, "DTLASTREGULPERIOD", DbType.Date), DB_LastRegularPeriodEndDate()); // FI 20201006 [XXXXX] DbType.Date
            dataParameters.Add(new DataParameter(pCS, "DTFIRSTPERIOD", DbType.Date), DB_FirstPeriodStartDate()); // FI 20201006 [XXXXX] DbType.Date


            QueryParameters qry = new QueryParameters(pCS, update.ToString(), dataParameters);

            return qry;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected override QueryParameters GetQueryInsert(string pCS)
        {

            StrBuilder insert = new StrBuilder();
            insert += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADESTREAM + Cst.CrLf;
            insert += "(IDT, INSTRUMENTNO, STREAMNO,";
            insert += "IDA_PAY, IDA_REC, IDC, IDC2, OPTIONTYPE,ISPRINCIPALEXCHANGE,";
            insert += "ISNOTIONALSTEP,ISMULTIPLIERSTEP,ISSPREADSTEP,ISFIXEDRATESTEP,ISSTRIKESTEP,ISAMOUNTSTEP,";
            insert += "BDC_CALCPERIOD,PERIODMLTPCALCDT,PERIODCALCDT,ROLLCONVCALCDT,";
            insert += "BDC_PAYMENT,PERIODMLTPPAYMENTDT,PERIODPAYMENTDT,";
            insert += "DTFIRSTREGULPERIOD,DTLASTREGULPERIOD,DTFIRSTPERIOD)" + Cst.CrLf;
            insert += " values " + Cst.CrLf;
            insert += " (@IDT, @INSTRUMENTNO, @STREAMNO, ";
            insert += " @IDA_PAY, @IDA_REC, @IDC, @IDC2, @OPTIONTYPE,@ISPRINCIPALEXCHANGE,";
            insert += " @ISNOTIONALSTEP,@ISMULTIPLIERSTEP,@ISSPREADSTEP,@ISFIXEDRATESTEP,@ISSTRIKESTEP,@ISAMOUNTSTEP,";
            insert += " @BDC_CALCPERIOD,@PERIODMLTPCALCDT,@PERIODCALCDT,@ROLLCONVCALCDT,";
            insert += " @BDC_PAYMENT,@PERIODMLTPPAYMENTDT,@PERIODPAYMENTDT,";
            insert += " @DTFIRSTREGULPERIOD,@DTLASTREGULPERIOD,@DTFIRSTPERIOD)";
            //
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), m_IdT);
            dataParameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), m_InstrumentNo);
            dataParameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), m_StreamNo);

            dataParameters.Add(new DataParameter(pCS, "IDA_PAY", DbType.Int32), DB_IdAPay());
            dataParameters.Add(new DataParameter(pCS, "IDA_REC", DbType.Int32), DB_IdARec());
            dataParameters.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC1());
            dataParameters.Add(new DataParameter(pCS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), DB_IdC2());
            dataParameters.Add(new DataParameter(pCS, "OPTIONTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_OptionType());
            dataParameters.Add(new DataParameter(pCS, "ISPRINCIPALEXCHANGE", DbType.Boolean), m_IsPrincipalExchange);

            dataParameters.Add(new DataParameter(pCS, "ISNOTIONALSTEP", DbType.Boolean), m_IsNotionalStep);
            dataParameters.Add(new DataParameter(pCS, "ISMULTIPLIERSTEP", DbType.Boolean), m_IsMultiplierStep);
            dataParameters.Add(new DataParameter(pCS, "ISSPREADSTEP", DbType.Boolean), m_IsSpreadStep);
            dataParameters.Add(new DataParameter(pCS, "ISFIXEDRATESTEP", DbType.Boolean), m_IsFixedRateStep);
            dataParameters.Add(new DataParameter(pCS, "ISSTRIKESTEP", DbType.Boolean), m_IsStrikeStep);
            dataParameters.Add(new DataParameter(pCS, "ISAMOUNTSTEP", DbType.Boolean), m_IsAmountStep);
            //
            dataParameters.Add(new DataParameter(pCS, "BDC_CALCPERIOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_Bdc_CalcPeriod());
            dataParameters.Add(new DataParameter(pCS, "PERIODMLTPCALCDT", DbType.Int32), DB_PeriodMltpCalcDt());
            dataParameters.Add(new DataParameter(pCS, "PERIODCALCDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_PeriodCalcDt());
            dataParameters.Add(new DataParameter(pCS, "ROLLCONVCALCDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_RollConvCalcDt());

            dataParameters.Add(new DataParameter(pCS, "BDC_PAYMENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_Bdc_Payment());
            dataParameters.Add(new DataParameter(pCS, "PERIODMLTPPAYMENTDT", DbType.Int32), DB_PeriodMltpPaymentDt());
            dataParameters.Add(new DataParameter(pCS, "PERIODPAYMENTDT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), DB_PeriodPaymentDt());

            dataParameters.Add(new DataParameter(pCS, "DTFIRSTREGULPERIOD", DbType.Date), DB_FirstRegularPeriodStartDate()); // FI 20201006 [XXXXX] DbType.Date
            dataParameters.Add(new DataParameter(pCS, "DTLASTREGULPERIOD", DbType.Date), DB_LastRegularPeriodEndDate()); // FI 20201006 [XXXXX] DbType.Date
            dataParameters.Add(new DataParameter(pCS, "DTFIRSTPERIOD", DbType.Date), DB_FirstPeriodStartDate()); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters qry = new QueryParameters(pCS, insert.ToString(), dataParameters);

            return qry;
        }
        #endregion Methods
    }

}

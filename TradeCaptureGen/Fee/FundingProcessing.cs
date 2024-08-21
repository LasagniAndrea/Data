using EFS.ACommon;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System.Data;

namespace EFS.TradeInformation
{
    public abstract class ProcessingBase
    {
        #region Members
        protected FeeRequest _scheduleRequest;

        /// <summary>
        /// Mode transactionnel
        /// </summary>
        protected IDbTransaction _dbTransaction;

        protected StrBuilder _auditMessage;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Mode transactionnel
        /// </summary>
        public IDbTransaction DbTransaction
        {
            get { return _dbTransaction; }
            set { _dbTransaction = value; }
        }

        /// <summary>
        /// Environment Spheres®
        /// </summary>
        public FeeRequest ScheduleRequest
        {
            get { return _scheduleRequest; }
        }
        #endregion Accessors

        #region Constructors
        public ProcessingBase(FeeRequest pSchedulequest)
        {
            _scheduleRequest = pSchedulequest;
        }
        #endregion Constructors
    }
    // EG 20150319 [POC] Add FundingBorrowingType
    public class FundingProcessing : ProcessingBase
    {
        #region Members
        private FundingResponse _fundingResponse;
        private readonly Cst.FundingType _fundingType;
        #endregion Members

        #region Accessors
        public FundingResponse FundingResponse
        {
            get { return _fundingResponse; }
        }
        public Cst.FundingType FundingType
        {
            get { return _fundingType; }
        }
        #endregion Accessors    

        #region Constructors
        public FundingProcessing(FeeRequest pSchedulequest)
            : base(pSchedulequest) { _fundingType = Cst.FundingType.Funding; }
        public FundingProcessing(Cst.FundingType pFundingType, FeeRequest pSchedulequest)
            : base(pSchedulequest) { _fundingType = pFundingType; }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Purge fundingResponse
        /// </summary>
        public void Reset()
        {
            _fundingResponse = null;
        }

        /// <summary>
        /// Check data and get the funding rate elements.
        /// </summary>
        public void Calc()
        {
            _fundingResponse = new FundingResponse(this);
            _fundingResponse.Calc();
            //TBD _fundingResponse.AuditMessage = _auditMessage.ToString();
        }

        /// <summary>
        /// Set funding rate elements into trade legs.
        /// </summary>
        /// <param name="pLegNumber"></param>
        public void SetFunding()
        {
            SetFunding(0);
        }
        /// <summary>
        /// Set funding rate elements into trade legs.
        /// </summary>
        /// <param name="pLegNumber"></param>
        public void SetFunding(int pLegNumber)
        {
            if (_fundingResponse.InterestCalculationSpecified)
            {
                IReturnSwap returnSwap = (IReturnSwap)_scheduleRequest.Product.Product;
                
                //Interest Leg
                if ((pLegNumber >= 0) && (returnSwap.InterestLegSpecified) && (returnSwap.InterestLeg.GetLength(0) > pLegNumber))
                {
                    IInterestCalculation trade_IC = returnSwap.InterestLeg[pLegNumber].InterestCalculation;
                    IInterestCalculation schedule_IC = _fundingResponse.InterestCalculation;

                    trade_IC.FloatingRateSpecified = schedule_IC.FloatingRateSpecified;
                    trade_IC.FixedRateSpecified = schedule_IC.FixedRateSpecified;
                    if (trade_IC.FloatingRateSpecified)
                    {
                        trade_IC.SqlAsset = schedule_IC.SqlAsset;
                        trade_IC.FloatingRate = schedule_IC.FloatingRate;

                        if (_fundingResponse.FixingDatesRDO.DayTypeSpecified)
                        {
                            IInterestLegResetDates trade_ILRD = returnSwap.InterestLeg[pLegNumber].CalculationPeriodDates.ResetDates;
                            trade_ILRD.ResetRelativeToSpecified = true;
                            trade_ILRD.ResetRelativeTo = ResetRelativeToEnum.CalculationPeriodStartDate;
                            trade_ILRD.FixingDatesSpecified = true;
                            trade_ILRD.FixingDates.AdjustableDatesSpecified = false;
                            trade_ILRD.FixingDates.RelativeDateOffsetSpecified = true;
                            //trade_ILRD.fixingDates.relativeDateOffset = _fundingResponse.FixingDatesRDO;
                            //A finaliser, si besoin (BC, ...)
                            trade_ILRD.FixingDates.RelativeDateOffset.DayTypeSpecified = _fundingResponse.FixingDatesRDO.DayTypeSpecified;
                            trade_ILRD.FixingDates.RelativeDateOffset.DayType = _fundingResponse.FixingDatesRDO.DayType;
                            trade_ILRD.FixingDates.RelativeDateOffset.Period = _fundingResponse.FixingDatesRDO.Period;
                            trade_ILRD.FixingDates.RelativeDateOffset.PeriodMultiplier = _fundingResponse.FixingDatesRDO.PeriodMultiplier;
                        }
                    }
                    if (trade_IC.FixedRateSpecified)
                    {
                        trade_IC.FixedRate = schedule_IC.FixedRate;
                    }
                    
                    trade_IC.DayCountFraction = schedule_IC.DayCountFraction;
                }
                
                //Return Leg
                if ((pLegNumber >= 0) && (returnSwap.ReturnLegSpecified) && (returnSwap.ReturnLeg.GetLength(0) > pLegNumber))
                {
                    IReturnLeg trade_RL = returnSwap.ReturnLeg[pLegNumber];

                    if (_fundingResponse.IsNotionalResetSpecified)
                    {
                        trade_RL.RateOfReturn.NotionalResetSpecified = true;
                        trade_RL.RateOfReturn.NotionalReset = new EFS.GUI.Interface.EFS_Boolean(_fundingResponse.IsNotionalReset);
                    }

                    if (_fundingResponse.InterestFrequencySpecified)
                    {
                        if (trade_RL.RateOfReturn.ValuationPriceInterimSpecified
                            && trade_RL.RateOfReturn.ValuationPriceInterim.ValuationRulesSpecified
                            && trade_RL.RateOfReturn.ValuationPriceInterim.ValuationRules.ValuationDatesSpecified
                            && trade_RL.RateOfReturn.ValuationPriceInterim.ValuationRules.ValuationDates.PeriodicDatesSpecified)
                        {
                            ICalculationPeriodFrequency cpf = trade_RL.RateOfReturn.ValuationPriceInterim.ValuationRules.ValuationDates.PeriodicDates.CalculationPeriodFrequency;
                            cpf.Period = _fundingResponse.InterestFrequency.Period;
                            cpf.PeriodMultiplier = _fundingResponse.InterestFrequency.PeriodMultiplier;
                        }
                    }
                }
            }
        }
        #endregion Methods
    }

    public class MarginProcessing : ProcessingBase
    {
        #region Members
        private MarginResponse _marginResponse;
        #endregion Members

        #region Accessors
        public MarginResponse MarginResponse
        {
            get { return _marginResponse; }
        }
        #endregion Accessors    

        #region Constructors
        public MarginProcessing(FeeRequest pSchedulequest)
            : base (pSchedulequest) { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Purge marginResponse
        /// </summary>
        public void Reset()
        {
            _marginResponse = null;
        }

        /// <summary>
        /// Check data and get the margin elements.
        /// </summary>
        public void Calc()
        {
            _marginResponse = new MarginResponse(this);
            _marginResponse.Calc();
            //TBD _marginResponse.AuditMessage = _auditMessage.ToString();
        }

        /// <summary>
        /// Set margin elements into trade returnleg.
        /// </summary>
        /// <param name="pLegNumber"></param>
        public void SetMargin()
        {
            SetMargin(0);
        }
        /// <summary>
        /// Set margin elements into trade returnleg.
        /// </summary>
        /// <param name="pLegNumber"></param>
        /// EG 20150306 [POC-BERKELEY] : EquitySecurityTransaction
        public void SetMargin(int pLegNumber)
        {
            if ( _marginResponse.MarginRatioAmountSpecified)
            {
                IMarginRatio marginRatio = null;

                if (_scheduleRequest.Product.Product.ProductBase.IsReturnSwap)
                {
                    IReturnSwap returnSwap = (IReturnSwap)_scheduleRequest.Product.Product;
                    //Return Leg
                    if ((pLegNumber >= 0) && (returnSwap.ReturnLegSpecified) && (returnSwap.ReturnLeg.GetLength(0) > pLegNumber))
                    {
                        IReturnLeg trade_RL = returnSwap.ReturnLeg[pLegNumber];

                        trade_RL.RateOfReturn.MarginRatioSpecified = true;
                        marginRatio = trade_RL.RateOfReturn.MarginRatio;
                        //if (trade_RL.notional.notionalAmountSpecified && (!string.IsNullOrEmpty(trade_RL.notional.notionalAmount.currency)))
                        //{
                        //    trade_RL.rateOfReturn.marginRatio.currencySpecified = true;
                        //    trade_RL.rateOfReturn.marginRatio.currency.Value = trade_RL.notional.notionalAmount.currency;
                        //}
                        //trade_RL.rateOfReturn.marginRatio.currencySpecified = false;

                        trade_RL.RateOfReturn.MarginRatio.SpreadScheduleSpecified = _marginResponse.SpreadMarginRatioAmountSpecified;
                        if (trade_RL.RateOfReturn.MarginRatio.SpreadScheduleSpecified)
                            trade_RL.RateOfReturn.MarginRatio.CreateSpreadMarginRatio(_marginResponse.SpreadMarginRatioAmount.Value);
                    }
                }
                else if (_scheduleRequest.Product.Product.ProductBase.IsEquitySecurityTransaction)
                {
                    IEquitySecurityTransaction eqs = (IEquitySecurityTransaction)_scheduleRequest.Product.Product;
                    eqs.MarginRatioSpecified = true;
                    marginRatio = eqs.MarginRatio;
                    if (eqs.MarginRatio != null)
                    {
                        eqs.MarginRatio.SpreadScheduleSpecified = _marginResponse.SpreadMarginRatioAmountSpecified;
                        if (eqs.MarginRatio.SpreadScheduleSpecified)
                            eqs.MarginRatio.CreateSpreadMarginRatio(_marginResponse.SpreadMarginRatioAmount.Value);
                    }
                }
                else if (_scheduleRequest.Product.Product.ProductBase.IsFxLeg)
                {
                    IFxLeg fxl = (IFxLeg)_scheduleRequest.Product.Product;
                    fxl.MarginRatioSpecified = true;
                    marginRatio = fxl.MarginRatio;
                }
                else if (_scheduleRequest.Product.Product.ProductBase.IsFxOptionLeg) 
                {
                    IFxOptionLeg fxol = (IFxOptionLeg)_scheduleRequest.Product.Product;
                    fxol.MarginRatioSpecified = true;
                    marginRatio = fxol.MarginRatio;
                }

                if (marginRatio != null)
                {
                    marginRatio.Amount.DecValue = _marginResponse.MarginRatioAmount.Value;
                    marginRatio.PriceExpression = PriceExpressionEnum.PercentageOfNotional;
                    marginRatio.CurrencySpecified = false;

                    marginRatio.CrossMarginRatioSpecified = _marginResponse.CrossMarginRatioAmountSpecified;
                    if (marginRatio.CrossMarginRatioSpecified)
                    {
                        marginRatio.CrossMarginRatio.Amount.DecValue = _marginResponse.CrossMarginRatioAmount.Value;
                        marginRatio.CrossMarginRatio.PriceExpression = PriceExpressionEnum.PercentageOfNotional;
                        marginRatio.CrossMarginRatio.CurrencySpecified = false;
                    }
                }
            }
        }
        #endregion Methods
    }
}
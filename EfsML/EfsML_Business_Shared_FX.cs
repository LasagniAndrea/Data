#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_AmericanDigitalOption
    /// <revision>
    ///     <version>1.1.5</version><date>20070404</date><author>EG</author>
    ///     <EurosysSupport>N° 15409</EurosysSupport>
    ///     <comment>
    ///     Ajout fxBarrier test sur DigitalOPtionType
    ///     Ajout IsAboveOrBelowIn et IsAboveOrBelowOut pour déterminer IsInTheMoney (Cas American DigitalBarrier option)
    ///		</comment>
    /// </revision>
    public class EFS_AmericanDigitalOption : EFS_DigitalOptionBase
    {
        #region Members
        public IFxAmericanTrigger[] trigger;
        public bool isBoundary;
        public bool isLimit;
        #endregion Members
        #region Accessors
        #region DigitalOptionType
        public string DigitalOptionType
        {
            get
            {
                string digitalOptionType = string.Empty;
                int nbTrigger = trigger.GetLength(0);
                if (1 == nbTrigger)
                {
                    if (TouchConditionEnum.Touch == trigger[0].TouchCondition)
                    {
                        digitalOptionType = EventTypeFunc.Touch;
                        if (fxBarrierSpecified)
                        {
                            FxBarrierTypeEnum fxBarrierType = fxBarrier[0].FxBarrierType;
                            if ((FxBarrierTypeEnum.Knockin == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockin == fxBarrierType))
                                digitalOptionType = EventTypeFunc.TouchKnockIn;
                            else if ((FxBarrierTypeEnum.Knockout == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockout == fxBarrierType))
                                digitalOptionType = EventTypeFunc.TouchKnockOut;
                        }
                    }
                    else if (TouchConditionEnum.Notouch == trigger[0].TouchCondition)
                    {
                        digitalOptionType = EventTypeFunc.NoTouch;
                        if (fxBarrierSpecified)
                        {
                            FxBarrierTypeEnum fxBarrierType = fxBarrier[0].FxBarrierType;
                            if ((FxBarrierTypeEnum.Knockin == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockin == fxBarrierType))
                                digitalOptionType = EventTypeFunc.NoTouchKnockIn;
                            else if ((FxBarrierTypeEnum.Knockout == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockout == fxBarrierType))
                                digitalOptionType = EventTypeFunc.NoTouchKnockOut;
                        }
                    }
                }
                else if (2 == nbTrigger)
                {
                    bool isNoTouch = (TouchConditionEnum.Notouch == trigger[0].TouchCondition) &&
                        (TouchConditionEnum.Notouch == trigger[1].TouchCondition);

                    bool isTouch = (TouchConditionEnum.Touch == trigger[0].TouchCondition) &&
                        (TouchConditionEnum.Touch == trigger[1].TouchCondition);

                    if (isNoTouch)
                    {
                        digitalOptionType = EventTypeFunc.DoubleNoTouch;
                        if (resurrectingSpecified)
                            digitalOptionType = EventTypeFunc.DoubleNoTouchResurrecting;
                        else if (extinguishingSpecified)
                            digitalOptionType = EventTypeFunc.DoubleNoTouchExtinguishing;
                    }
                    else if (isTouch)
                    {
                        digitalOptionType = EventTypeFunc.DoubleTouch;
                        if (isLimit && extinguishingSpecified)
                            digitalOptionType = EventTypeFunc.DoubleTouchLimitExtinguishing;
                        else if (isLimit && resurrectingSpecified)
                            digitalOptionType = EventTypeFunc.DoubleTouchLimitResurrecting;
                        else if (isBoundary)
                            digitalOptionType = EventTypeFunc.DoubleTouchBoundary;
                        else if (isLimit)
                            digitalOptionType = EventTypeFunc.DoubleTouchLimit;
                    }
                }
                return digitalOptionType;
            }
        }
        #endregion DigitalOptionType
        #endregion Accessors
        #region Constructors
        public EFS_AmericanDigitalOption(string pConnectionString, ITradeDate pTradeDate, IFxDigitalOption pFxDigitalOption)
            : base(pConnectionString, pTradeDate, pFxDigitalOption)
        {
            trigger = pFxDigitalOption.TypeTriggerAmerican;
            isBoundary = pFxDigitalOption.BoundarySpecified;
            isLimit = pFxDigitalOption.LimitSpecified;
        }
        #endregion Constructors
    }
    #endregion EFS_AmericanDigitalOption
    #region EFS_AverageFxFixing
    public class EFS_AverageFxFixing
    {
        #region Members
        public string eventType;
        public IQuotedCurrencyPair currencyPair;
        public EFS_FxFixing[] fixing;
        #endregion Members
        #region Accessors
        #region CurrencyPair
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IQuotedCurrencyPair CurrencyPair
        {
            get
            {
                return this.currencyPair;

            }
        }
        #endregion CurrencyPair
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                return this.eventType;

            }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        public EFS_AverageFxFixing(string pEventType, ArrayList pFixing)
        {
            eventType = pEventType;
            currencyPair = ((EFS_FxFixing)pFixing[0]).fixing.QuotedCurrencyPair;
            fixing = (EFS_FxFixing[])pFixing.ToArray(typeof(EFS_FxFixing));
        }
        #endregion Constructors
    }
    #endregion EFS_AverageFxFixing

    #region EFS_DeliverableLeg
    // EG 20150402 [POC] Add MarginRatio
    public class EFS_DeliverableLeg
    {
        #region Members
        public FxTypeEnum fxType;
        public EFS_FxLegPayment exchangedCurrency1;
        public bool sideRateCurrency1Specified;
        public EFS_SideRatePayment sideRateCurrency1;
        public EFS_FxLegPayment exchangedCurrency2;
        public bool sideRateCurrency2Specified;
        public EFS_SideRatePayment sideRateCurrency2;
        public EFS_FwpDepreciableAmount fwpDepreciableAmount;
        // EG 20150403 [POC]
        public bool marginRatioSpecified;
        public IMarginRatio marginRatio;
        public bool initialMarginSpecified;
        public IMoney initialMargin;
        #endregion Members
        #region Accessors
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {

                DateTime valueDate1 = exchangedCurrency1.valueDate;
                DateTime valueDate2 = exchangedCurrency2.valueDate;
                int ret = valueDate1.CompareTo(valueDate2);
                if (0 < ret)
                    return exchangedCurrency1.ValueDate;
                else
                    return exchangedCurrency2.ValueDate;

            }
        }
        #endregion ValueDate
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
        // EG 20150402 (POC] Add MarginRatio
        public EFS_DeliverableLeg(EFS_FxLegPayment pExchangedCurrency1, EFS_SideRatePayment pSideRateCurrency1,
            EFS_FxLegPayment pExchangedCurrency2, EFS_SideRatePayment pSideRateCurrency2, Pair<IMarginRatio, IMoney> pMargin)
        //IMarginRatio pMarginRatio, IMoney pInitialMargin)
        {
            fxType = FxTypeEnum.Unknown;
            exchangedCurrency1 = pExchangedCurrency1;
            sideRateCurrency1Specified = (null != pSideRateCurrency1);
            sideRateCurrency1 = pSideRateCurrency1;
            exchangedCurrency2 = pExchangedCurrency2;
            sideRateCurrency2Specified = (null != pSideRateCurrency2);
            sideRateCurrency2 = pSideRateCurrency2;

            marginRatioSpecified = (null != pMargin.First);
            marginRatio = pMargin.First;
            initialMarginSpecified = (null != pMargin.Second);
            initialMargin = pMargin.Second;

            if (exchangedCurrency1.exchangeRate.exchangeRate.FxFixingSpecified ||
                exchangedCurrency2.exchangeRate.exchangeRate.FxFixingSpecified)
                fxType = FxTypeEnum.DeliverableFxLeg_Fixing;

            bool isSpotSpecified = (exchangedCurrency1.exchangeRate.exchangeRate.SpotRateSpecified);
            bool isForwardPointSpecified = (exchangedCurrency1.exchangeRate.exchangeRate.ForwardPointsSpecified);
            if (isSpotSpecified || isForwardPointSpecified)
                fwpDepreciableAmount = new EFS_FwpDepreciableAmount(exchangedCurrency1, exchangedCurrency2);

        }
        #endregion Constructors
        #region Methods
        #region EventCode
        public string EventCode(EFS_EventDate pTradeDate)
        {
            DateTime tradeDate = pTradeDate.adjustedDate.DateValue;
            System.TimeSpan timeSpan = exchangedCurrency1.valueDate.Subtract(tradeDate);
            if (2 < timeSpan.Days)
                return EventCodeFunc.FxForward;
            else
                return EventCodeFunc.FxSpot;
        }
        #endregion EventCode
        #endregion Methods
    }
    #endregion EFS_DeliverableLeg
    #region EFS_DigitalOptionBase
    /// <revision>
    ///     <version>1.1.5</version><date>20070404</date><author>EG</author>
    ///     <EurosysSupport>N° 15409</EurosysSupport>
    ///     <comment>
    ///     Ajout fxBarrierSpecified et fxBarrier (provenance de EFS_EuropeanDigitalOption)
    ///     désormais communs à tout type d'option digitale
    ///		</comment>
    /// </revision>
    /// <revision>
    ///     <version>1.2.0</version><date>20071030</date><author>EG</author>
    ///     <EurosysSupport>N° XXXXX</EurosysSupport>
    ///     <comment>
    ///     Ajout exerciseDates pour homogénisation des options
    ///		</comment>
    /// </revision>
    public abstract class EFS_DigitalOptionBase
    {
        #region Members
        private readonly string m_Cs;
        public ITradeDate tradeDate;
        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        public IExpiryDateTime expiryDateTime;
        public EFS_Date valueDate;
        public IQuotedCurrencyPair quotedCurrencyPair;
        public bool spotRateSpecified;
        public EFS_Decimal spotRate;
        public EFS_FxOptionPayout triggerPayout;
        public ExerciseStyleEnum exerciseStyle;
        public bool resurrectingSpecified;
        public IPayoutPeriod resurrecting;
        public bool extinguishingSpecified;
        public IPayoutPeriod extinguishing;
        public bool fxBarrierSpecified;
        public IFxBarrier[] fxBarrier;
        public EFS_FxExerciseDates[] exerciseDates;
        #endregion Members
        #region Accessors
        #region AdjustedExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual EFS_Date AdjustedExpiryDate
        {
            get
            {
                return new EFS_Date(expiryDateTime.ExpiryDate.Value);
            }
        }
        #endregion AdjustedExpiryDate
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
        #region ExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpiryDate
        {
            get { return new EFS_EventDate(expiryDateTime.ExpiryDate.DateValue, expiryDateTime.ExpiryDate.DateValue); }
        }
        #endregion ExpiryDate
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
        #region SpotRate
        public EFS_Decimal SpotRate
        {
            get { return spotRate; }
        }
        #endregion SpotRate
        #endregion Accessors
        #region Constructors
        public EFS_DigitalOptionBase(string pConnectionString, ITradeDate pTradeDate, IFxDigitalOption pFxDigitalOption)
        {
            m_Cs = pConnectionString;
            tradeDate = pTradeDate;
            buyerPartyReference = pFxDigitalOption.BuyerPartyReference;
            sellerPartyReference = pFxDigitalOption.SellerPartyReference;
            expiryDateTime = pFxDigitalOption.ExpiryDateTime;
            valueDate = new EFS_Date
            {
                DateValue = pFxDigitalOption.ValueDate.DateValue
            };
            quotedCurrencyPair = pFxDigitalOption.QuotedCurrencyPair;
            triggerPayout = new EFS_FxOptionPayout(m_Cs, pFxDigitalOption.TriggerPayout);
            exerciseStyle = (pFxDigitalOption.TypeTriggerAmericanSpecified ? ExerciseStyleEnum.American : ExerciseStyleEnum.European);
            spotRateSpecified = pFxDigitalOption.SpotRateSpecified;
            spotRate = pFxDigitalOption.SpotRate;

            resurrectingSpecified = pFxDigitalOption.ResurrectingSpecified;
            if (resurrectingSpecified)
                resurrecting = pFxDigitalOption.Resurrecting;
            extinguishingSpecified = pFxDigitalOption.ExtinguishingSpecified;
            if (extinguishingSpecified)
                extinguishing = pFxDigitalOption.Extinguishing;

            fxBarrierSpecified = pFxDigitalOption.FxBarrierSpecified;
            if (fxBarrierSpecified)
                fxBarrier = pFxDigitalOption.FxBarrier;

            SetExerciseDates();
        }
        #endregion Constructors
        #region Methods
        #region SetExerciseDates
        private void SetExerciseDates()
        {
            DateTime startPeriod = tradeDate.DateValue;
            DateTime endPeriod = expiryDateTime.ExpiryDate.DateValue;
            //DateTime exerciseDate = endPeriod;
            if (ExerciseStyleEnum.American == exerciseStyle)
            {
                exerciseDates = new EFS_FxExerciseDates[1];
                exerciseDates[0] = new EFS_FxExerciseDates(EventTypeFunc.American, startPeriod, endPeriod, SettlementTypeEnum.Cash);
            }
            else if (ExerciseStyleEnum.European == exerciseStyle)
            {
                exerciseDates = new EFS_FxExerciseDates[1];
                exerciseDates[0] = new EFS_FxExerciseDates(EventTypeFunc.European, endPeriod, endPeriod, SettlementTypeEnum.Cash);
            }
        }
        #endregion SetExerciseDates
        #endregion Methods
    }
    #endregion EFS_DigitalOptionBase

    #region EFS_EuropeanDigitalOption
    /// <revision>
    ///     <version>1.1.5</version><date>20070404</date><author>EG</author>
    ///     <EurosysSupport>N° 15409</EurosysSupport>
    ///     <comment>
    ///     Déplacement fxBarrierSpecified et fxBarrier vers sa classe de base (EFS_DigitalOptionBase)
    ///     désormais communs à tout type d'option digitale
    ///		</comment>
    /// </revision>
    public class EFS_EuropeanDigitalOption : EFS_DigitalOptionBase
    {
        #region Members
        public IFxEuropeanTrigger[] trigger;
        public bool assetOrNothingSpecified;
        public IAssetOrNothing assetOrNothing;
        #endregion Members
        #region Accessors
        #region DigitalOptionType
        public string DigitalOptionType
        {
            get
            {
                string digitalOptionType = string.Empty;
                int nbTrigger = trigger.GetLength(0);
                if (1 == nbTrigger)
                {
                    if (TriggerConditionEnum.Above == trigger[0].TriggerCondition)
                    {
                        digitalOptionType = EventTypeFunc.Above;

                        if (assetOrNothingSpecified)
                        {
                            if (assetOrNothing.GapSpecified)
                                digitalOptionType = EventTypeFunc.AboveGap;
                            else
                                digitalOptionType = EventTypeFunc.AboveAssetOrNothing;
                        }
                        else if (fxBarrierSpecified)
                        {
                            FxBarrierTypeEnum fxBarrierType = fxBarrier[0].FxBarrierType;
                            if ((FxBarrierTypeEnum.Knockin == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockin == fxBarrierType))
                                digitalOptionType = EventTypeFunc.AboveKnockIn;
                            else if ((FxBarrierTypeEnum.Knockout == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockout == fxBarrierType))
                                digitalOptionType = EventTypeFunc.AboveKnockOut;
                        }
                        else if (resurrectingSpecified)
                            digitalOptionType = EventTypeFunc.AboveResurrecting;
                        else if (extinguishingSpecified)
                            digitalOptionType = EventTypeFunc.AboveExtinguishing;
                    }
                    else if (TriggerConditionEnum.Below == trigger[0].TriggerCondition)
                    {
                        digitalOptionType = EventTypeFunc.Below;

                        if (assetOrNothingSpecified)
                        {
                            if (assetOrNothing.GapSpecified)
                                digitalOptionType = EventTypeFunc.BelowGap;
                            else
                                digitalOptionType = EventTypeFunc.BelowAssetOrNothing;
                        }
                        else if (fxBarrierSpecified)
                        {
                            FxBarrierTypeEnum fxBarrierType = fxBarrier[0].FxBarrierType;
                            if ((FxBarrierTypeEnum.Knockin == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockin == fxBarrierType))
                                digitalOptionType = EventTypeFunc.BelowKnockIn;
                            else if ((FxBarrierTypeEnum.Knockout == fxBarrierType) || (FxBarrierTypeEnum.ReverseKnockout == fxBarrierType))
                                digitalOptionType = EventTypeFunc.BelowKnockOut;
                        }
                        else if (resurrectingSpecified)
                            digitalOptionType = EventTypeFunc.BelowResurrecting;
                        else if (extinguishingSpecified)
                            digitalOptionType = EventTypeFunc.BelowExtinguishing;
                    }
                }
                else if (2 == nbTrigger)
                {
                    digitalOptionType = EventTypeFunc.Range;
                }
                return digitalOptionType;
            }
        }
        #endregion DigitalOptionType
        #endregion Accessors
        #region Constructors
        public EFS_EuropeanDigitalOption(string pConnectionString, ITradeDate pTradeDate, IFxDigitalOption pFxDigitalOption)
            : base(pConnectionString, pTradeDate, pFxDigitalOption)
        {
            trigger = pFxDigitalOption.TypeTriggerEuropean;
            assetOrNothingSpecified = pFxDigitalOption.AssetOrNothingSpecified;
            if (assetOrNothingSpecified)
                assetOrNothing = pFxDigitalOption.AssetOrNothing;
        }
        #endregion Constructors
    }
    #endregion EFS_EuropeanDigitalOption

    #region EFS_FwpDepreciableAmount
    public class EFS_FwpDepreciableAmount
    {
        #region Members
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public EFS_EventDate valueDate;
        public EFS_Date preSettlementDate;
        public EFS_Decimal fwpRate;
        public IMoney fwpAmount;
        public EFS_ExchangeRate exchangeRate;
        #endregion Members
        #region Accessors
        #region ExchangeRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExchangeRate ExchangeRate
        {
            get
            {
                return this.exchangeRate;

            }
        }
        #endregion ExchangeRate
        #region FwpAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal FwpAmount
        {
            get
            {

                if (null != fwpAmount)
                    return fwpAmount.Amount;
                else
                    return null;

            }
        }

        #endregion FwpAmount
        #region FwpCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FwpCurrency
        {
            get
            {
                return fwpAmount.Currency;

            }
        }

        #endregion FwpCurrency
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
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {
                return valueDate;

            }
        }
        #endregion ValueDate
        #region AdjustedValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedValueDate
        {
            get { return valueDate.adjustedDate; }
        }
        #endregion AdjustedValueDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get { return preSettlementDate; }
        }
        #endregion AdjustedPreSettlementDate
        #endregion Accessors
        #region Constructors
        public EFS_FwpDepreciableAmount(EFS_FxLegPayment pFxLegPaymentCurrency1, EFS_FxLegPayment pFxLegPaymentCurrency2)
        {
            exchangeRate = pFxLegPaymentCurrency1.exchangeRate;
            IQuotedCurrencyPair qcp = exchangeRate.exchangeRate.QuotedCurrencyPair;
            if ((QuoteBasisEnum.Currency1PerCurrency2 == qcp.QuoteBasis) &&
                (qcp.Currency2 == pFxLegPaymentCurrency1.PaymentCurrency))
                Calc(pFxLegPaymentCurrency2, pFxLegPaymentCurrency1.PaymentCurrency);
            else if ((QuoteBasisEnum.Currency2PerCurrency1 == qcp.QuoteBasis) &&
                (qcp.Currency1 == pFxLegPaymentCurrency1.PaymentCurrency))
                Calc(pFxLegPaymentCurrency2, pFxLegPaymentCurrency1.PaymentCurrency);
            else
                Calc(pFxLegPaymentCurrency1, pFxLegPaymentCurrency2.PaymentCurrency);
        }
        public EFS_FwpDepreciableAmount(EFS_NonDeliverableForward pNonDeliverableForward)
        {
            exchangeRate = pNonDeliverableForward.exchangedCurrency.exchangeRate;
            string currency = pNonDeliverableForward.currency2.Value;
            if (pNonDeliverableForward.referenceCurrency.Value == currency)
                currency = pNonDeliverableForward.currency1.Value;

            Calc(pNonDeliverableForward.exchangedCurrency, currency);
        }
        public EFS_FwpDepreciableAmount(EFS_FxLegPayment pFxLegPayment)
        {
            exchangeRate = pFxLegPayment.exchangeRate;
            Calc(pFxLegPayment, pFxLegPayment.PaymentCurrency);
        }
        #endregion Constructors
        #region Methods
        private Cst.ErrLevel Calc(EFS_FxLegPayment pFxLegPayment, string pCurrency)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            valueDate = pFxLegPayment.ValueDate;
            preSettlementDate = pFxLegPayment.AdjustedPreSettlementDate;

            IQuotedCurrencyPair qcp = exchangeRate.exchangeRate.QuotedCurrencyPair;
            decimal forwardRate = exchangeRate.exchangeRate.Rate.DecValue;
            #region ForwardPoints rate
            fwpRate = new EFS_Decimal();
            if (false == exchangeRate.exchangeRate.SpotRateSpecified)
            {
                if (exchangeRate.exchangeRate.ForwardPointsSpecified)
                {
                    exchangeRate.exchangeRate.SpotRateSpecified = true;
                    exchangeRate.exchangeRate.SpotRate = new EFS_Decimal(forwardRate - exchangeRate.exchangeRate.ForwardPoints.DecValue);
                }
            }
            decimal spotRate = exchangeRate.exchangeRate.SpotRate.DecValue;
            if (qcp.Currency1 == pCurrency)
            {
                if (QuoteBasisEnum.Currency1PerCurrency2 == qcp.QuoteBasis)
                    qcp.QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                else
                    qcp.QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
            }
            #endregion ForwardPoints rate
            #region ForwardPoints amount
            EFS_Cash cash1 = new EFS_Cash(pFxLegPayment.CS, pFxLegPayment.paymentAmount.Currency, pCurrency, pFxLegPayment.paymentAmount.Amount.DecValue, forwardRate, qcp.QuoteBasis);
            EFS_Cash cash2 = new EFS_Cash(pFxLegPayment.CS, pFxLegPayment.paymentAmount.Currency, pCurrency, pFxLegPayment.paymentAmount.Amount.DecValue, spotRate, qcp.QuoteBasis);
            decimal amount = cash1.ExchangeAmountRounded - cash2.ExchangeAmountRounded;
            fwpAmount = exchangeRate.exchangeRate.CreateMoney(System.Math.Abs(amount), pCurrency);
            if (0 > amount)
            {
                payerPartyReference = pFxLegPayment.receiverPartyReference;
                receiverPartyReference = pFxLegPayment.payerPartyReference;
            }
            else
            {
                payerPartyReference = pFxLegPayment.payerPartyReference;
                receiverPartyReference = pFxLegPayment.receiverPartyReference;
            }
            #endregion ForwardPoints amount

            return ret;
        }
        #endregion Methods
    }
    #endregion EFS_FwpDepreciableAmount

    #region EFS_NonDeliverableLeg
    // EG 20150402 [POC] Add MarginRatio
    public class EFS_NonDeliverableLeg
    {
        #region Members
        public EFS_FxLegPayment exchangedCurrency1;
        public bool sideRateCurrency1Specified;
        public EFS_SideRatePayment sideRateCurrency1;

        public EFS_FxLegPayment exchangedCurrency2;
        public bool sideRateCurrency2Specified;
        public EFS_SideRatePayment sideRateCurrency2;

        public EFS_NonDeliverableForward nonDeliverableForwardCurrency;
        public EFS_FwpDepreciableAmount fwpDepreciableAmount;

        public bool marginRatioSpecified;
        public IMarginRatio marginRatio;
        public bool initialMarginSpecified;
        public IMoney initialMargin;

        #endregion Members
        #region Accessors
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return nonDeliverableForwardCurrency.PayerPartyReference;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return nonDeliverableForwardCurrency.ReceiverPartyReference;

            }
        }
        #endregion ReceiverPartyReference
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {

                DateTime valueDate1 = exchangedCurrency1.valueDate;
                DateTime valueDate2 = exchangedCurrency2.valueDate;
                int ret = valueDate1.CompareTo(valueDate2);
                if (0 < ret)
                    return exchangedCurrency1.ValueDate;
                else
                    return exchangedCurrency2.ValueDate;

            }
        }
        #endregion ValueDate
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
        // EG 20150402 (POC] Add MarginRatio
        public EFS_NonDeliverableLeg(string pConnectionString, EFS_FxLegPayment pExchangedCurrency1, EFS_SideRatePayment pSideRateCurrency1,
            EFS_FxLegPayment pExchangedCurrency2, EFS_SideRatePayment pSideRateCurrency2, IFxCashSettlement pNonDeliverableForward, Pair<IMarginRatio, IMoney> pMargin)
        {
            exchangedCurrency1 = pExchangedCurrency1;
            sideRateCurrency1Specified = (null != pSideRateCurrency1);
            sideRateCurrency1 = pSideRateCurrency1;

            exchangedCurrency2 = pExchangedCurrency2;
            sideRateCurrency2Specified = (null != pSideRateCurrency2);
            sideRateCurrency2 = pSideRateCurrency2;

            nonDeliverableForwardCurrency = new EFS_NonDeliverableForward(pConnectionString, pExchangedCurrency1, pExchangedCurrency2, pNonDeliverableForward);

            bool isSpotSpecified = (nonDeliverableForwardCurrency.exchangedCurrency.exchangeRate.exchangeRate.SpotRateSpecified);
            bool isForwardPointSpecified = (nonDeliverableForwardCurrency.exchangedCurrency.exchangeRate.exchangeRate.ForwardPointsSpecified);
            if (isSpotSpecified || isForwardPointSpecified)
                fwpDepreciableAmount = new EFS_FwpDepreciableAmount(nonDeliverableForwardCurrency);

            marginRatioSpecified = (null != pMargin.First);
            marginRatio = pMargin.First;
            initialMarginSpecified = (null != pMargin.Second);
            initialMargin = pMargin.Second;

        }
        #endregion Constructors
        #region Methods
        #region EventCode
        public string EventCode(EFS_EventDate pTradeDate)
        {
            DateTime tradeDate = pTradeDate.adjustedDate.DateValue;
            System.TimeSpan timeSpan = exchangedCurrency1.valueDate.Subtract(tradeDate);
            if (2 < timeSpan.Days)
                return EventCodeFunc.FxForward;
            else
                return EventCodeFunc.FxSpot;
        }
        #endregion EventCode
        #endregion Methods
    }
    #endregion EFS_NonDeliverableLeg
    #region EFS_NonDeliverableForward
    public class EFS_NonDeliverableForward
    {
        #region Members
        private readonly string m_Cs;
        public FxTypeEnum fxType;
        public ICurrency currency1;
        public ICurrency currency2;
        public ICurrency referenceCurrency;
        public ICurrency settlementCurrency;
        public EFS_FxLegPayment exchangedCurrency;
        public bool calculationAgentSettlementRateSpecified;
        public EFS_SettlementRate settlementRate;

        public bool avgQuotedFixingSpecified;
        public EFS_AverageFxFixing[] avgQuotedFixing;
        public bool quotedFixingSpecified;
        public EFS_FxFixing[] quotedFixing;

        public bool avgSettlementFixingSpecified;
        public EFS_AverageFxFixing avgSettlementFixing;
        public bool settlementFixingSpecified;
        public EFS_FxFixing settlementFixing;
        #endregion Members
        #region Accessors
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                if (null != exchangedCurrency.paymentDateAdjustment)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = exchangedCurrency.paymentDateAdjustment.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get { return exchangedCurrency.AdjustedPreSettlementDate; }
        }
        #endregion AdjustedPreSettlementDate
        #region IsCalculationAgentSettlementRate
        public bool IsCalculationAgentSettlementRate
        {
            get { return calculationAgentSettlementRateSpecified; }
        }
        #endregion IsCalculationAgentSettlementRate
        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return exchangedCurrency.payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return exchangedCurrency.receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference
        #region SettlementCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SettlementCurrency
        {
            get
            {
                return settlementCurrency.Value;

            }
        }
        #endregion SettlementCurrency
        #region SettlementRate
        public EFS_SettlementRate SettlementRate
        {
            get { return settlementRate; }
        }
        #endregion SettlementRate
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {
                return new EFS_EventDate(exchangedCurrency.valueDate, exchangedCurrency.valueDate);

            }
        }
        #endregion ValueDate
        #endregion Accessors
        #region Constructors
        public EFS_NonDeliverableForward(string pConnectionString, EFS_FxLegPayment pExchangeCurrency1, EFS_FxLegPayment pExchangeCurrency2, IFxCashSettlement pNonDeliverableForward)
        {

            #region Variables initialisation
            m_Cs = pConnectionString;
            fxType = FxTypeEnum.Unknown;
            settlementCurrency = pNonDeliverableForward.SettlementCurrency;
            currency1 = pExchangeCurrency1.paymentAmount.GetCurrency;
            currency2 = pExchangeCurrency2.paymentAmount.GetCurrency;
            calculationAgentSettlementRateSpecified = pNonDeliverableForward.CustomerSettlementRateSpecified || pNonDeliverableForward.CalculationAgentSettlementRateSpecified;
            #endregion Variables initialisation
            #region private Variables
            string ce1 = currency1.Value;
            string ce2 = currency2.Value;
            string cs = settlementCurrency.Value;
            bool isThirdCurrency = (cs != ce1) && (cs != ce2);
            Hashtable hashQuotedFixing = new Hashtable();
            Hashtable hashSettlementFixing = new Hashtable();
            #endregion private Variables
            #region Fixing identification by type (Quoted and/or Settlement)
            foreach (IFxFixing fixing in pNonDeliverableForward.Fixing)
            {
                string cf1 = fixing.QuotedCurrencyPair.Currency1;
                string cf2 = fixing.QuotedCurrencyPair.Currency2;
                #region Quotations fixing
                if (((ce1 == cf1) && (ce2 == cf2)) || ((ce1 == cf2) && (ce2 == cf1)))
                    RegroupFixing("C1C2", hashQuotedFixing, fixing);
                else if (((ce1 == cf1) && (ce2 != cf2)) || ((ce1 == cf2) && (ce2 != cf1)))
                    RegroupFixing("C1C3", hashQuotedFixing, fixing);
                else if (((ce2 == cf1) && (ce1 != cf2)) || ((ce2 == cf2) && (ce1 != cf1)))
                    RegroupFixing("C2C3", hashQuotedFixing, fixing);
                #endregion Quotations fixing
                #region Settlement fixing
                if (((cs == cf1) && (ce1 == cf2)) || ((cs == cf2) && (ce1 == cf1)))
                    RegroupFixing("C1CS", hashSettlementFixing, fixing);
                else if (((cs == cf1) && (ce2 == cf2)) || ((cs == cf2) && (ce2 == cf1)))
                    RegroupFixing("C2CS", hashSettlementFixing, fixing);
                else if ((cs == cf1) || (cs == cf2))
                    RegroupFixing("C3CS", hashSettlementFixing, fixing);
                #endregion Settlement fixing
            }
            #endregion Fixing identification by type (Quoted and/or Settlement)


            ArrayList aAverageFixing = new ArrayList();
            ArrayList aFixing = new ArrayList();
            ArrayList aAverageSettlementFixing = new ArrayList();
            ArrayList aSettlementFixing = new ArrayList();
            if (hashQuotedFixing.ContainsKey("C1C2"))
            {
                DispatchFixing(hashQuotedFixing, "C1C2", aAverageFixing, aFixing);
                if (isThirdCurrency)
                {
                    #region NDF_ThirdSettlementCurrency
                    fxType = FxTypeEnum.NDF_ThirdSettlementCurrency;
                    if (hashSettlementFixing.ContainsKey("C1CS"))
                    {
                        DispatchFixing(hashSettlementFixing, "C1CS", aAverageSettlementFixing, aSettlementFixing);
                        referenceCurrency = currency2;
                    }
                    if (hashSettlementFixing.ContainsKey("C2CS"))
                    {
                        DispatchFixing(hashSettlementFixing, "C2CS", aAverageSettlementFixing, aSettlementFixing);
                        referenceCurrency = currency1;
                    }
                    #endregion NDF_ThirdSettlementCurrency
                    if (null == referenceCurrency)
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "FxType error (Reference Currency undefined)");
                }
                else
                {
                    #region NDF
                    fxType = FxTypeEnum.NDF;
                    referenceCurrency = (cs == ce1) ? currency2 : currency1;
                    #endregion NDF
                }
            }
            else if (hashQuotedFixing.ContainsKey("C1C3") && hashQuotedFixing.ContainsKey("C2C3"))
            {
                DispatchFixing(hashQuotedFixing, "C1C3", aAverageFixing, aFixing);
                DispatchFixing(hashQuotedFixing, "C2C3", aAverageFixing, aFixing);
                if (isThirdCurrency)
                {
                    #region NDF_Bridge_ThirdSettlementCurrency
                    fxType = FxTypeEnum.NDF_Bridge_ThirdSettlementCurrency;
                    referenceCurrency = GetEmergentCurrency();
                    if ((ce2 == referenceCurrency.Value) && hashSettlementFixing.ContainsKey("C1CS"))
                        DispatchFixing(hashSettlementFixing, "C1CS", aAverageSettlementFixing, aSettlementFixing);
                    else if ((ce1 == referenceCurrency.Value) && hashSettlementFixing.ContainsKey("C2CS"))
                        DispatchFixing(hashSettlementFixing, "C2CS", aAverageSettlementFixing, aSettlementFixing);
                    else if (hashSettlementFixing.ContainsKey("C3CS"))
                        DispatchFixing(hashSettlementFixing, "C3CS", aAverageSettlementFixing, aSettlementFixing);
                    else
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "FxType error (Fixing incompatibilites)");
                    #endregion NDF_Bridge_ThirdSettlementCurrency
                }
                else if (hashSettlementFixing.ContainsKey("C3CS"))
                {
                    #region NDF_Bridge
                    fxType = FxTypeEnum.NDF_Bridge;
                    referenceCurrency = (cs == ce1) ? currency2 : currency1;
                    #endregion NDF_Bridge
                }
                else
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "FxType error (Fixing incompatibilites)");
            }
            else
            {
                #region Error
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "FxType error (Fixing incompatibilites)");
                #endregion Error
            }
            #region QuotedFixing Setting
            avgQuotedFixingSpecified = (0 < aAverageFixing.Count);
            if (avgQuotedFixingSpecified)
                avgQuotedFixing = (EFS_AverageFxFixing[])aAverageFixing.ToArray(typeof(EFS_AverageFxFixing));
            quotedFixingSpecified = (0 < aFixing.Count);
            if (quotedFixingSpecified)
                quotedFixing = (EFS_FxFixing[])aFixing.ToArray(typeof(EFS_FxFixing));
            #endregion QuotedFixing Setting
            #region SettlementFixing Setting
            avgSettlementFixingSpecified = (0 < aAverageSettlementFixing.Count);
            if (avgSettlementFixingSpecified)
                avgSettlementFixing = (EFS_AverageFxFixing)aAverageSettlementFixing[0];
            settlementFixingSpecified = (0 < aSettlementFixing.Count);
            if (settlementFixingSpecified)
                settlementFixing = (EFS_FxFixing)aSettlementFixing[0];
            #endregion SettlementFixing Setting

            #region ForwardRate/NotionalAmount
            EFS_Decimal notionalAmount = new EFS_Decimal();
            if (ce1 == referenceCurrency.Value)
            {
                exchangedCurrency = pExchangeCurrency1;
                notionalAmount = new EFS_Decimal(pExchangeCurrency2.paymentAmount.Amount.DecValue);
            }
            else if (ce2 == referenceCurrency.Value)
            {
                exchangedCurrency = pExchangeCurrency2;
                notionalAmount = new EFS_Decimal(pExchangeCurrency1.paymentAmount.Amount.DecValue);
            }
            settlementRate = new EFS_SettlementRate(fxType, notionalAmount, (ICurrency)referenceCurrency, exchangedCurrency.exchangeRate, (IMoney)exchangedCurrency.paymentAmount);
            #endregion ForwardRate/NotionalAmount

        }
        #endregion Constructors
        #region Methods
        #region RegroupFixing
        private void RegroupFixing(string pKey, Hashtable pHashFixing, IFxFixing pFixing)
        {
            EFS_FxFixing efs_FxFixing = new EFS_FxFixing(pKey, pFixing);
            if (pHashFixing.ContainsKey(pKey))
                ((ArrayList)pHashFixing[pKey]).Add(efs_FxFixing);
            else
            {
                ArrayList aFixing = new ArrayList
                {
                    efs_FxFixing
                };
                pHashFixing.Add(pKey, aFixing);
            }
        }
        #endregion RegroupFixing
        #region DispatchFixing
        private void DispatchFixing(Hashtable pHashFixing, string pCurrencyPair, ArrayList pAverageFixing, ArrayList paFixing)
        {
            string eventType = string.Empty;
            foreach (DictionaryEntry fixing in pHashFixing)
            {
                if (pCurrencyPair == fixing.Key.ToString())
                {
                    switch (pCurrencyPair)
                    {
                        case "C1C2":
                            if (calculationAgentSettlementRateSpecified)
                                eventType = EventTypeFunc.FxCalculationAgent;
                            else
                                eventType = EventTypeFunc.FxRate;
                            break;
                        case "C1C3":
                            eventType = EventTypeFunc.FxRate1;
                            break;
                        case "C2C3":
                            eventType = EventTypeFunc.FxRate2;
                            break;
                        case "C1CS":
                        case "C2CS":
                        case "C3CS":
                            eventType = EventTypeFunc.SettlementCurrency;
                            break;
                    }
                    EFS_FxFixing efs_FxFixing = null;
                    if (1 < ((ArrayList)fixing.Value).Count)
                    {
                        for (int i = 0; i < ((ArrayList)fixing.Value).Count; i++)
                        {
                            efs_FxFixing = (EFS_FxFixing)((ArrayList)fixing.Value)[i];
                            efs_FxFixing.eventType = eventType;
                        }
                        EFS_AverageFxFixing averageFxFixing = new EFS_AverageFxFixing(eventType, (ArrayList)fixing.Value);
                        pAverageFixing.Add(averageFxFixing);
                    }
                    else
                    {
                        efs_FxFixing = (EFS_FxFixing)((ArrayList)fixing.Value)[0];
                        efs_FxFixing.eventType = eventType;
                        paFixing.Add(((ArrayList)fixing.Value)[0]);
                    }
                }
            }
        }
        #endregion DispatchFixing
        #region GetEmergentCurrency
        private ICurrency GetEmergentCurrency()
        {
            ICurrency currency = currency2;
            SQL_Currency sqlCurrency1 = new SQL_Currency(m_Cs, currency1.Value);
            SQL_Currency sqlCurrency2 = new SQL_Currency(m_Cs, currency2.Value);
            if (sqlCurrency2.IsLoaded && sqlCurrency2.IsEmergingMarket)
                currency = currency2;
            else if (sqlCurrency1.IsLoaded && sqlCurrency1.IsEmergingMarket)
                currency = currency1;
            return currency;
        }
        #endregion GetEmergentCurrency
        #endregion Methods
    }
    #endregion EFS_NonDeliverableForward

    #region EFS_RateObservationDate
    public class EFS_RateObservationDate
    {
        #region Members
        public EFS_Date observationDate;
        public EFS_Decimal weightingFactor;
        public bool rateSpecified;
        public EFS_Decimal rate;
        public bool isSecondaryRateSource;
        #endregion Members
        #region Constructors
        public EFS_RateObservationDate(DateTime pObservationDate)
        {
            observationDate = new EFS_Date
            {
                DateValue = pObservationDate
            };
            weightingFactor = new EFS_Decimal(1);
        }
        public EFS_RateObservationDate(DateTime pObservationDate, decimal pWeightingFactor)
        {
            observationDate = new EFS_Date
            {
                DateValue = pObservationDate
            };
            weightingFactor = new EFS_Decimal(pWeightingFactor);
        }
        public EFS_RateObservationDate(DateTime pObservationDate, decimal pWeightingFactor, decimal pRate)
        {
            observationDate = new EFS_Date
            {
                DateValue = pObservationDate
            };
            weightingFactor = new EFS_Decimal(pWeightingFactor);
            rateSpecified = true;
            rate = new EFS_Decimal(pRate);
        }
        #endregion Constructors
    }
    #endregion EFS_RateObservationDate

    #region EFS_SettlementRate
    public class EFS_SettlementRate
    {
        #region Members
        public FxTypeEnum fxType;
        public string referenceCurrency;
        public EFS_Decimal notionalAmount;
        public EFS_Decimal forwardRate;
        public bool settlementRateSpecified;
        public EFS_Decimal settlementRate;
        public bool conversionRateSpecified;
        public EFS_Decimal conversionRate;
        public bool spotRateSpecified;
        public EFS_Decimal spotRate;
        #endregion Members
        #region Constructors
        public EFS_SettlementRate(FxTypeEnum pFxType, EFS_Decimal pNotionalAmount, ICurrency pReferenceCurrency,
            EFS_ExchangeRate pExchangeRate, IMoney pPaymentAmount)
        {
            fxType = pFxType;
            referenceCurrency = pReferenceCurrency.Value;
            notionalAmount = pNotionalAmount;
            #region ForwardRate / SpotRate
            IExchangeRate exchangeRate = pExchangeRate.exchangeRate;
            string currency1 = exchangeRate.QuotedCurrencyPair.Currency1;
            string currency2 = exchangeRate.QuotedCurrencyPair.Currency2;
            QuoteBasisEnum basis = exchangeRate.QuotedCurrencyPair.QuoteBasis;
            spotRateSpecified = exchangeRate.SpotRateSpecified;

            if (((QuoteBasisEnum.Currency1PerCurrency2 == basis) && (pPaymentAmount.Currency == currency1)) ||
                ((QuoteBasisEnum.Currency2PerCurrency1 == basis) && (pPaymentAmount.Currency == currency2)))
            {
                forwardRate = new EFS_Decimal(exchangeRate.Rate.DecValue);
                if (spotRateSpecified)
                    spotRate = new EFS_Decimal(exchangeRate.SpotRate.DecValue);
            }
            else if (((QuoteBasisEnum.Currency1PerCurrency2 == basis) && (pPaymentAmount.Currency == currency2)) ||
                ((QuoteBasisEnum.Currency2PerCurrency1 == basis) && (pPaymentAmount.Currency == currency1)))
            {
                forwardRate = new EFS_Decimal(1 / exchangeRate.Rate.DecValue);
                if (spotRateSpecified)
                    spotRate = new EFS_Decimal(1 / exchangeRate.SpotRate.DecValue);
            }
            #endregion ForwardRate / SpotRate
            #region SettlementRate
            //settlementRateSpecified = false;
            #endregion SettlementRate
            #region ConversionRate
            conversionRateSpecified = Tools.IsRegularNDF(pFxType) || Tools.IsNDF_Bridge(pFxType);
            if (conversionRateSpecified)
                conversionRate = new EFS_Decimal(1);
            #endregion ConversionRate
        }
        #endregion Constructors
    }
    #endregion EFS_SettlementRate
    #region EFS_SideRatePayment
    public class EFS_SideRatePayment
    {
        #region Members
        public ISideRate sideRate;
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney baseAmount;
        public DateTime valueDate;
        #endregion Members
        #region Accessors
        #region BaseAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal BaseAmount
        {
            get
            {

                if (null != baseAmount)
                    return baseAmount.Amount;
                else
                    return null;

            }
        }
        #endregion BaseAmount
        #region BaseCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string BaseCurrency
        {
            get
            {
                return baseAmount.Currency;

            }
        }
        #endregion BaseCurrency
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
        #region SideRate
        public ISideRate SideRate
        {
            get { return sideRate; }
        }
        #endregion SideRate
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {
                return new EFS_EventDate(valueDate, valueDate);

            }
        }
        #endregion ValueDate
        #endregion Accessors
        #region Constructors
        public EFS_SideRatePayment(ICurrency pBaseCurrency, ISideRate pSideRate, EFS_FxLegPayment pExchangeCurrency)
            : this(pBaseCurrency, pSideRate, pExchangeCurrency, false) { }
        public EFS_SideRatePayment(ICurrency pBaseCurrency, ISideRate pSideRate, EFS_FxLegPayment pExchangeCurrency, bool pIsBasisConverted)
        {
            Calc(pBaseCurrency, pSideRate, pExchangeCurrency, pIsBasisConverted);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        private Cst.ErrLevel Calc(ICurrency pBaseCurrency, ISideRate pSideRate, EFS_FxLegPayment pExchangeCurrency, bool pIsBasisConverted)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            sideRate = pSideRate;
            payerPartyReference = pExchangeCurrency.payerPartyReference;
            receiverPartyReference = pExchangeCurrency.receiverPartyReference;
            valueDate = pExchangeCurrency.valueDate;
            #region SideRateBasis conversion
            if (pIsBasisConverted)
            {
                if (SideRateBasisEnum.BaseCurrencyPerCurrency1 == sideRate.SideRateBasis)
                    sideRate.SideRateBasis = SideRateBasisEnum.BaseCurrencyPerCurrency2;
                else if (SideRateBasisEnum.BaseCurrencyPerCurrency2 == sideRate.SideRateBasis)
                    sideRate.SideRateBasis = SideRateBasisEnum.BaseCurrencyPerCurrency1;
                else if (SideRateBasisEnum.Currency1PerBaseCurrency == sideRate.SideRateBasis)
                    sideRate.SideRateBasis = SideRateBasisEnum.Currency2PerBaseCurrency;
                else if (SideRateBasisEnum.Currency2PerBaseCurrency == sideRate.SideRateBasis)
                    sideRate.SideRateBasis = SideRateBasisEnum.Currency1PerBaseCurrency;
            }
            #endregion SideRateBasis conversion
            #region Amount calculation
            decimal notional = pExchangeCurrency.PaymentAmount.DecValue;
            decimal calculatedAmount = 0;
            if ((SideRateBasisEnum.BaseCurrencyPerCurrency1 == sideRate.SideRateBasis) ||
                (SideRateBasisEnum.BaseCurrencyPerCurrency2 == sideRate.SideRateBasis))
                calculatedAmount = notional / sideRate.Rate.DecValue;
            else if ((SideRateBasisEnum.Currency1PerBaseCurrency == sideRate.SideRateBasis) ||
                (SideRateBasisEnum.Currency2PerBaseCurrency == sideRate.SideRateBasis))
                calculatedAmount = notional * sideRate.Rate.DecValue;

            baseAmount = sideRate.CreateMoney(calculatedAmount, pBaseCurrency.Value);
            #endregion Amount calculation

            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_SideRatePayment


    #region EFS_FxAverageRateObservationDates
    public class EFS_FxAverageRateObservationDates
    {
        #region Members
        private readonly bool observedRatesSpecified;
        private readonly IObservedRates[] observedRates;
        private readonly bool rateObservationDateSpecified;
        private readonly IFxAverageRateObservationDate[] rateObservationDates;
        private readonly bool rateObservationScheduleSpecified;
        private readonly IFxAverageRateObservationSchedule rateObservationSchedule;
        public EFS_RateObservationDate[] efs_RateObservationDates;
        #endregion Members
        #region Constructors
        public EFS_FxAverageRateObservationDates(IFxAverageRateOption pFxAverageRateOption) : this(pFxAverageRateOption, Convert.ToDateTime(null)) { }
        public EFS_FxAverageRateObservationDates(IFxAverageRateOption pFxAverageRateOption, DateTime pActionDate)
        {
            observedRatesSpecified = pFxAverageRateOption.ObservedRatesSpecified;
            rateObservationDateSpecified = pFxAverageRateOption.RateObservationDateSpecified;
            rateObservationScheduleSpecified = pFxAverageRateOption.RateObservationScheduleSpecified;

            if (observedRatesSpecified)
                observedRates = pFxAverageRateOption.ObservedRates;
            if (rateObservationDateSpecified)
                rateObservationDates = pFxAverageRateOption.RateObservationDate;
            if (rateObservationScheduleSpecified)
                rateObservationSchedule = pFxAverageRateOption.RateObservationSchedule;

            Calc(pActionDate);
        }
        #endregion Constructors

        #region Methods
        #region Calc
        private void Calc(DateTime pActionDate)
        {
            ArrayList aRateObservation = new ArrayList();
            bool isNoActionDate = DtFunc.IsDateTimeEmpty(pActionDate);
            if (rateObservationDateSpecified)
            {
                #region RateObservationDates
                foreach (IFxAverageRateObservationDate item in rateObservationDates)
                {
                    if (isNoActionDate || (0 <= pActionDate.CompareTo(item.ObservationDate)))
                        aRateObservation.Add(new EFS_RateObservationDate(item.ObservationDate, item.AverageRateWeightingFactor));
                }
                #endregion RateObservationDates
            }
            else if (rateObservationScheduleSpecified)
            {
                #region RateObservationSchedule
                if (isNoActionDate || (0 <= pActionDate.CompareTo(rateObservationSchedule.ObservationStartDate.DateValue)))
                {
                    aRateObservation.Add(new EFS_RateObservationDate(rateObservationSchedule.ObservationStartDate.DateValue));

                    EFS_Period[] periods = Tools.ApplyInterval(rateObservationSchedule.ObservationStartDate.DateValue,
                        rateObservationSchedule.ObservationEndDate.DateValue, rateObservationSchedule.CalculationPeriodFrequency.Interval,
                        rateObservationSchedule.CalculationPeriodFrequency.RollConvention);
                    foreach (EFS_Period item in periods)
                    {
                        if (isNoActionDate || (0 <= pActionDate.CompareTo(item.date2)))
                            aRateObservation.Add(new EFS_RateObservationDate(item.date2));
                    }
                }
                #endregion RateObservationSchedule
            }
            #region ObservedRates
            if (observedRatesSpecified)
            {
                foreach (IObservedRates item in observedRates)
                {
                    if (isNoActionDate || (0 <= pActionDate.CompareTo(item.ObservationDate)))
                        aRateObservation.Add(new EFS_RateObservationDate(item.ObservationDate, Convert.ToDecimal(1), item.ObservedRate));
                }
            }
            #endregion ObservedRates
            #region Alim efs_RateObservationDates
            efs_RateObservationDates = (EFS_RateObservationDate[])aRateObservation.ToArray(typeof(EFS_RateObservationDate));
            #endregion Alim efs_RateObservationDates
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxAverageRateObservationDates
    #region EFS_FxAverageRateOption
    public class EFS_FxAverageRateOption : EFS_FxOptionBase
    {
        #region Members
        public bool isAverageStrikeOption;
        #endregion Members
        #region Accessors
        #region EventCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventCode
        {
            get
            {
                string eventType = base.ExerciseTypeOption;
                if (EventTypeFunc.IsAmerican(eventType))
                    return EventCodeFunc.AmericanAverageOption;
                else if (EventTypeFunc.IsBermuda(eventType))
                    return EventCodeFunc.BermudaAverageOption;
                else if (EventTypeFunc.IsEuropean(eventType))
                    return EventCodeFunc.EuropeanAverageOption;
                return null;
            }
        }
        #endregion EventCode
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get { return isAverageStrikeOption ? EventTypeFunc.AverageStrike : EventTypeFunc.AverageRate; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        public EFS_FxAverageRateOption(string pConnectionString, ITradeDate pTradeDate, IFxAverageRateOption pFxAverageRateOption)
            : base(pConnectionString, pTradeDate, pFxAverageRateOption)
        {
            isAverageStrikeOption = pFxAverageRateOption.AverageStrikeOptionSpecified;
        }
        #endregion Constructors
    }
    #endregion EFS_FxAverageRateOption
    #region EFS_FxBarrierOption
    public class EFS_FxBarrierOption : EFS_FxOptionBase
    {
        #region Members
        public bool spotRateSpecified;
        public EFS_Decimal spotRate;
        public IFxBarrier[] fxBarrier;
        public bool fxRebateBarrierSpecified;
        public IFxBarrier fxRebateBarrier;
        public bool isFxCapBarrier;
        public bool isFxFloorBarrier;
        public bool triggerPayoutSpecified;
        public EFS_FxOptionPayout triggerPayout;
        #endregion Members
        #region Accessors
        #region EventCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventCode
        {
            get
            {
                string eventType = base.ExerciseTypeOption;
                if (EventTypeFunc.IsAmerican(eventType))
                    return EventCodeFunc.AmericanBarrierOption;
                else if (EventTypeFunc.IsBermuda(eventType))
                    return EventCodeFunc.BermudaBarrierOption;
                else if (EventTypeFunc.IsEuropean(eventType))
                    return EventCodeFunc.EuropeanBarrierOption;
                return null;
            }
        }
        #endregion EventCode
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                string eventType = EventCode;
                foreach (IFxBarrier barrier in fxBarrier)
                {
                    if (barrier.FxBarrierTypeSpecified)
                    {
                        if ((FxBarrierTypeEnum.Knockin == barrier.FxBarrierType) ||
                            (FxBarrierTypeEnum.ReverseKnockin == barrier.FxBarrierType))
                            eventType = EventTypeFunc.KnockIn;
                        else if ((FxBarrierTypeEnum.Knockout == barrier.FxBarrierType) ||
                            (FxBarrierTypeEnum.ReverseKnockout == barrier.FxBarrierType))
                            eventType = EventTypeFunc.KnockOut;
                        break;
                    }
                }
                return eventType;
            }
        }
        #endregion EventType
        #region IsFxCapBarrier
        public bool IsFxCapBarrier
        {
            get { return isFxCapBarrier; }
        }
        #endregion IsFxCapBarrier
        #region IsFxFloorBarrier
        public bool IsFxFloorBarrier
        {
            get { return isFxFloorBarrier; }
        }
        #endregion IsFxFloorBarrier
        #region SpotRate
        public EFS_Decimal SpotRate
        {
            get { return spotRate; }
        }
        #endregion SpotRate
        #endregion Accessors
        #region Constructors
        public EFS_FxBarrierOption(string pConnectionString, ITradeDate pTradeDate, IFxBarrierOption pFxBarrierOption)
            : base(pConnectionString, pTradeDate, pFxBarrierOption)
        {
            fxBarrier = pFxBarrierOption.FxBarrier;
            spotRateSpecified = pFxBarrierOption.SpotRateSpecified;
            if (spotRateSpecified)
                spotRate = pFxBarrierOption.SpotRate;
            fxRebateBarrierSpecified = pFxBarrierOption.FxRebateBarrierSpecified;
            if (fxRebateBarrierSpecified)
                fxRebateBarrier = pFxBarrierOption.FxRebateBarrier;
            if (pFxBarrierOption.CappedCallOrFlooredPutSpecified)
            {
                isFxCapBarrier = pFxBarrierOption.CappedCallOrFlooredPut.TypeFxCapBarrierSpecified;
                isFxFloorBarrier = pFxBarrierOption.CappedCallOrFlooredPut.TypeFxFloorBarrierSpecified;
            }
            triggerPayoutSpecified = pFxBarrierOption.TriggerPayoutSpecified;
            if (triggerPayoutSpecified)
                triggerPayout = new EFS_FxOptionPayout(m_Cs, pFxBarrierOption.TriggerPayout);
        }
        #endregion Constructors
    }
    #endregion EFS_FxBarrierOption
    #region EFS_FxDigitalOption
    public class EFS_FxDigitalOption
    {
        #region Members
        private readonly string m_Cs;
        public bool americanDigitalOptionSpecified;
        public EFS_AmericanDigitalOption americanDigitalOption;
        public bool europeanDigitalOptionSpecified;
        public EFS_EuropeanDigitalOption europeanDigitalOption;
        #endregion Members
        #region Constructors
        public EFS_FxDigitalOption(string pConnectionString, ITradeDate pTradeDate, IFxDigitalOption pFxDigitalOption)
        {
            m_Cs = pConnectionString;
            Calc(pTradeDate, pFxDigitalOption);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        private Cst.ErrLevel Calc(ITradeDate pTradeDate, IFxDigitalOption pFxDigitalOption)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            americanDigitalOptionSpecified = pFxDigitalOption.TypeTriggerAmericanSpecified;
            europeanDigitalOptionSpecified = pFxDigitalOption.TypeTriggerEuropeanSpecified;
            if (americanDigitalOptionSpecified)
                americanDigitalOption = new EFS_AmericanDigitalOption(m_Cs, pTradeDate, pFxDigitalOption);
            else if (europeanDigitalOptionSpecified)
                europeanDigitalOption = new EFS_EuropeanDigitalOption(m_Cs, pTradeDate, pFxDigitalOption);


            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxDigitalOption
    #region EFS_FxExerciseDates
    // EG 20091229 Add SettltMethodEnum (FIX) for ETD
    public class EFS_FxExerciseDates
    {
        #region Members
        public EfsML.Enum.EventTypeEnum code;
        public string settlementType;
        public EFS_Date startPeriod;
        public EFS_Date endPeriod;
        public EFS_Date exerciseDate;
        #endregion Members
        #region Accessors
        #region AdjustedExerciseDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedExerciseDate
        {
            get { return new EFS_Date(exerciseDate.Value); }
        }
        #endregion AdjustedExerciseDate
        #region EndPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate EndPeriod
        {
            get { return new EFS_EventDate(endPeriod.DateValue, endPeriod.DateValue); }
        }
        #endregion EndPeriod
        #region ExerciseCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExerciseCode
        {
            get { return code.ToString(); }
        }
        #endregion ExerciseCode
        #region ExerciseSettlementType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExerciseSettlementType
        {
            get { return settlementType; }
        }
        #endregion ExerciseSettlementType
        #region StartPeriod
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate StartPeriod
        {
            get { return new EFS_EventDate(startPeriod.DateValue, startPeriod.DateValue); }
        }
        #endregion StartPeriod
        #endregion Accessors
        #region Constructors
        public EFS_FxExerciseDates() { }
        // EG 20091229 Add Constructor with : SettlMethodEnum pSettlementType
        public EFS_FxExerciseDates(string pEventType, DateTime pStartPeriod, DateTime pEndPeriod, SettlMethodEnum pSettlMethod)
        {
            SettlementTypeEnum settlementType = SettlementTypeEnum.Cash;
            if (FixML.Enum.SettlMethodEnum.PhysicalSettlement == pSettlMethod)
                settlementType = SettlementTypeEnum.Physical;
            Calc(pEventType, pStartPeriod, pEndPeriod, pEndPeriod, settlementType);
        }
        public EFS_FxExerciseDates(string pEventType, DateTime pStartPeriod, DateTime pEndPeriod, SettlementTypeEnum pSettlementType)
        {
            Calc(pEventType, pStartPeriod, pEndPeriod, pEndPeriod, pSettlementType);
        }
        public EFS_FxExerciseDates(string pEventType, DateTime pStartPeriod, DateTime pEndPeriod, DateTime pExerciseDate, SettlementTypeEnum pSettlementType)
        {
            Calc(pEventType, pStartPeriod, pEndPeriod, pExerciseDate, pSettlementType);
        }
        #endregion Constructors
        #region Methods
        private void Calc(string pEventType, DateTime pStartPeriod, DateTime pEndPeriod, DateTime pExerciseDate, SettlementTypeEnum pSettlementType)
        {
            code = (EventTypeEnum)System.Enum.Parse(typeof(EventTypeEnum), pEventType, true);
            if (SettlementTypeEnum.Election == pSettlementType)
                settlementType = EventClassFunc.ElectionSettlement;
            else if (SettlementTypeEnum.Cash == pSettlementType)
                settlementType = EventClassFunc.CashSettlement;
            else if (SettlementTypeEnum.Physical == pSettlementType)
                settlementType = EventClassFunc.PhysicalSettlement;
            startPeriod = new EFS_Date
            {
                DateValue = pStartPeriod
            };
            endPeriod = new EFS_Date
            {
                DateValue = pEndPeriod
            };
            exerciseDate = new EFS_Date
            {
                DateValue = pExerciseDate
            };
        }
        #endregion Methods
    }
    #endregion EFS_FxExerciseDates
    #region EFS_FxExerciseProcedure
    public class EFS_FxExerciseProcedure
    {
        #region Members
        public bool isCashSettlement;
        public bool automaticExerciseSpecified;
        public IAutomaticExercise automaticExercise;
        public bool isFallBackExercise;
        public SettlementTypeEnum settlementType;
        #endregion Members
        #region Accessors
        #region ExerciseProcedureType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExerciseProcedureType
        {
            get
            {
                if (automaticExerciseSpecified)
                    return EventCodeFunc.AutomaticExercise;
                else if (isFallBackExercise)
                    return EventCodeFunc.FallbackExercise;
                else
                    return string.Empty;
            }
        }
        #endregion ExerciseProcedureType
        #region SettlementType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string SettlementType
        {
            get
            {
                string type = string.Empty;
                if (SettlementTypeEnum.Cash == settlementType)
                    type = EventTypeFunc.CashSettlement;
                else if (SettlementTypeEnum.Election == settlementType)
                    type = EventTypeFunc.ElectionSettlement;
                else if (SettlementTypeEnum.Physical == settlementType)
                    type = EventTypeFunc.PhysicalSettlement;
                return type;
            }
        }
        #endregion SettlementType
        #endregion Accessors
        #region Constructors
        public EFS_FxExerciseProcedure() { }
        public EFS_FxExerciseProcedure(IExerciseProcedure pExerciseProcedure, SettlementTypeEnum pSettlementType)
        {
            automaticExerciseSpecified = pExerciseProcedure.ExerciseProcedureAutomaticSpecified;
            if (automaticExerciseSpecified)
                automaticExercise = pExerciseProcedure.ExerciseProcedureAutomatic;
            else
                isFallBackExercise = pExerciseProcedure.ExerciseProcedureManual.FallbackExercise;
            settlementType = pSettlementType;
        }
        #endregion Constructors
        #region Methods
        #region AddOneDayToExpiryDate
        public static EFS_Date AddOneDayToExpiryDate(EFS_EventDate pExpiryDate)
        {
            DateTime dtExpiry = pExpiryDate.adjustedDate.DateValue.AddDays(1);
            return new EFS_Date(DtFunc.DateTimeToString(dtExpiry, DtFunc.FmtISODate));
        }
        #endregion AddOneDayToExpiryDate
        #endregion Methods
    }
    #endregion EFS_FxExerciseProcedure
    #region EFS_FxFixing
    public class EFS_FxFixing : ICloneable
    {
        #region Members
        public string eventType;
        public IFxFixing fixing;
        public bool referenceCurrencySpecified;
        public string referenceCurrency;
        public bool notionalAmountSpecified;
        public EFS_Decimal notionalAmount;
        #endregion Members
        #region Accessors
        #region AssetFxRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object AssetFxRate
        {
            get { return Tools.GetAssetFxRate(this); }
        }
        #endregion AssetFxRate
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get
            {
                return this.eventType;

            }
        }
        #endregion EventType
        #region FixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date FixingDate
        {
            get
            {
                return new EFS_Date(fixing.FixingDate.Value);

            }
        }
        #endregion FixingDate
        #region FixingRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FxFixing FixingRate
        {
            get
            {
                return this;

            }
        }
        #endregion FixingRate
        #endregion Accessors
        #region Constructors
        public EFS_FxFixing() { }
        public EFS_FxFixing(string pEventType, IFxFixing pFixing)
        {
            eventType = pEventType;
            fixing = pFixing;
        }
        public EFS_FxFixing(string pEventType, IFxFixing pFixing, IMoney pAmount)
        {
            eventType = pEventType;
            fixing = pFixing;
            if (null != pAmount)
            {
                referenceCurrencySpecified = StrFunc.IsFilled(pAmount.Currency);
                if (referenceCurrencySpecified)
                    referenceCurrency = pAmount.Currency;

                notionalAmountSpecified = (0 != pAmount.Amount.DecValue);
                if (notionalAmountSpecified)
                    notionalAmount = pAmount.Amount;
            }
        }
        #endregion Constructors

        #region ICloneable Members
        #region Clone
        public object Clone()
        {
            EFS_FxFixing clone = new EFS_FxFixing
            {
                eventType = eventType,
                fixing = (IFxFixing)fixing.Clone(),
                referenceCurrencySpecified = referenceCurrencySpecified,
                notionalAmountSpecified = notionalAmountSpecified
            };
            if (referenceCurrencySpecified)
                clone.referenceCurrency = referenceCurrency;
            if (notionalAmountSpecified)
                clone.notionalAmount = new EFS_Decimal(notionalAmount.DecValue);
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members

    }
    #endregion EFS_FxFixing
    #region EFS_FxLeg
    // EG 20150402 (POC] Add MarginRatio
    public class EFS_FxLeg
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        public bool deliverableLegSpecified;
        public EFS_DeliverableLeg deliverableLeg;
        public bool nonDeliverableLegSpecified;
        public EFS_NonDeliverableLeg nonDeliverableLeg;


        // EG 20150403 POC
        private Pair<IMarginRatio, IMoney> margin;
        public Pair<Pair<IReference, IReference>, Pair<IReference, IReference>> partyReference;
        #endregion Members
        #region Accessors
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {
                if (deliverableLegSpecified)
                    return deliverableLeg.ValueDate;
                else if (nonDeliverableLegSpecified)
                    return nonDeliverableLeg.ValueDate;
                else
                    return null;
            }
        }
        #endregion ValueDate
        #region CustodianPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CustodianPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if ((null != partyReference) && (null != partyReference.Second) && (null != partyReference.Second.Second))
                    hRef = partyReference.Second.Second.HRef;
                return hRef;

            }
        }
        #endregion CustodianPartyReference
        #region DealerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string DealerPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if ((null != partyReference) && (null != partyReference.Second) && (null != partyReference.Second.First))
                    hRef = partyReference.Second.First.HRef;
                return hRef;
            }
        }
        #endregion DealerPartyReference
        #region InitialMarginPayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                string hRef = DealerPartyReference;
                if (StrFunc.IsEmpty(hRef))
                    hRef = partyReference.First.First.HRef;
                return hRef;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                string hRef = CustodianPartyReference;
                if (StrFunc.IsEmpty(hRef))
                    hRef = partyReference.First.First.HRef;
                return hRef;
            }
        }
        #endregion InitialMarginReceiverPartyReference

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLeg(string pConnectionString, IFxLeg pFxLeg, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            Calc(pFxLeg, pDataDocument, pStatusBusiness);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel Calc(IFxLeg pFxLeg, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            FxLegContainer fxLegContainer = new FxLegContainer(pFxLeg, pDataDocument);
            fxLegContainer.InitRptSide(m_Cs, (pStatusBusiness == Cst.StatusBusiness.ALLOC));
            partyReference = fxLegContainer.GetPartyReference();

            #region ValueDate
            DateTime valueDate = Convert.ToDateTime(null);
            if (pFxLeg.FxDateValueDateSpecified)
                valueDate = pFxLeg.FxDateValueDate.DateValue;
            #endregion ValueDate
            #region ExchangeCurrency1
            if (pFxLeg.FxDateCurrency1ValueDateSpecified)
                valueDate = pFxLeg.FxDateCurrency1ValueDate.DateValue;
            EFS_FxLegPayment exchangedCurrency1 = new EFS_FxLegPayment(m_Cs, EFS_ExchangeCurrencyPosEnum.ExchangeCurrency1, pFxLeg, valueDate, m_DataDocument);
            #endregion ExchangeCurrency1
            #region ExchangeCurrency2
            if (pFxLeg.FxDateCurrency2ValueDateSpecified)
                valueDate = pFxLeg.FxDateCurrency2ValueDate.DateValue;
            EFS_FxLegPayment exchangedCurrency2 = new EFS_FxLegPayment(m_Cs, EFS_ExchangeCurrencyPosEnum.ExchangeCurrency2, pFxLeg, valueDate, m_DataDocument);
            #endregion ExchangeCurrency2
            #region SideRates
            EFS_SideRatePayment sideRateCurrency1 = null;
            EFS_SideRatePayment sideRateCurrency2 = null;

            if (pFxLeg.ExchangeRate.SideRatesSpecified)
            {
                ISideRates sideRates = pFxLeg.ExchangeRate.SideRates;
                ICurrency baseCurrency = sideRates.BaseCurrency;
                if (sideRates.Currency1SideRateSpecified)
                {
                    ISideRate currency1SideRate = sideRates.Currency1SideRate;
                    string currency1 = currency1SideRate.Currency;
                    if (currency1 == exchangedCurrency1.paymentAmount.Currency)
                        sideRateCurrency1 = new EFS_SideRatePayment(baseCurrency, currency1SideRate, exchangedCurrency1);
                    else if (currency1 == exchangedCurrency2.paymentAmount.Currency)
                        sideRateCurrency2 = new EFS_SideRatePayment(baseCurrency, currency1SideRate, exchangedCurrency2, true);
                }
                if (sideRates.Currency2SideRateSpecified)
                {
                    ISideRate currency2SideRate = sideRates.Currency2SideRate;
                    string currency2 = currency2SideRate.Currency;
                    if (currency2 == exchangedCurrency1.paymentAmount.Currency)
                        sideRateCurrency1 = new EFS_SideRatePayment(baseCurrency, currency2SideRate, exchangedCurrency1, true);
                    else if (currency2 == exchangedCurrency2.paymentAmount.Currency)
                        sideRateCurrency2 = new EFS_SideRatePayment(baseCurrency, currency2SideRate, exchangedCurrency2);
                }
            }
            #endregion SideRates

            #region InitialMargin

            margin = new Pair<IMarginRatio, IMoney>();
            if (fxLegContainer.MarginRatioSpecified)
            {
                margin.First = fxLegContainer.MarginRatio;
                IMoney amountCurrency2 = exchangedCurrency2.paymentAmount;
                IProductBase productBase = (IProductBase)fxLegContainer.DataDocument.CurrentProduct.ProductBase;
                switch (fxLegContainer.MarginRatio.PriceExpression)
                {
                    case PriceExpressionEnum.PercentageOfNotional:
                        margin.Second = productBase.CreateMoney();
                        margin.Second.Amount = new EFS_Decimal(margin.First.Amount.DecValue * amountCurrency2.Amount.DecValue);
                        margin.Second.Currency = amountCurrency2.Currency;
                        break;
                    case PriceExpressionEnum.AbsoluteTerms:
                        margin.Second = productBase.CreateMoney();
                        margin.Second.Amount = new EFS_Decimal(margin.First.Amount.DecValue);
                        margin.Second.Currency = margin.First.CurrencySpecified ? margin.First.Currency.Value : amountCurrency2.Currency;
                        break;
                }
            }
            #endregion InitialMargin
            #region Deliverable Or NonDeliverableForward
            nonDeliverableLegSpecified = pFxLeg.NonDeliverableForwardSpecified;
            deliverableLegSpecified = (false == nonDeliverableLegSpecified);
            // EG 20150402 (POC] Add MarginRatio
            if (deliverableLegSpecified)
                deliverableLeg = new EFS_DeliverableLeg(exchangedCurrency1, sideRateCurrency1, exchangedCurrency2, sideRateCurrency2, margin);
            else if (nonDeliverableLegSpecified)
                nonDeliverableLeg = new EFS_NonDeliverableLeg(m_Cs, exchangedCurrency1, sideRateCurrency1, exchangedCurrency2, sideRateCurrency2, pFxLeg.NonDeliverableForward, margin);
            #endregion Deliverable Or NonDeliverableForward

            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxLeg
    #region EFS_FxLegPayment
    /// <revision>
    ///     <version>1.1.8</version><date>20070823</date><author>EG</author>
    ///     <comment>
    ///     Apply Rules to determine the date of Pre-Settlement events
    ///     </comment>
    /// </revision>
    public class EFS_FxLegPayment
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney paymentAmount;
        public DateTime valueDate;
        public EFS_AdjustableDate paymentDateAdjustment;
        public EFS_ExchangeRate exchangeRate;
        public bool fixingSpecified;
        public EFS_FxFixing fixing;
        public EFS_PreSettlement preSettlement;
        #endregion Members
        #region Accessors
        #region CS
        public string CS { get { return m_Cs; } }
        #endregion CS
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {

                if (null != paymentDateAdjustment)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = paymentDateAdjustment.adjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }
        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {

                if (null != preSettlement)
                    return preSettlement.AdjustedPreSettlementDate;
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
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
        #region PaymentAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PaymentAmount
        {
            get
            {

                if (null != paymentAmount)
                    return paymentAmount.Amount;
                else
                    return null;

            }
        }

        #endregion PaymentAmount
        #region PaymentCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PaymentCurrency
        {
            get
            {
                return paymentAmount.Currency;

            }
        }
        #endregion PaymentCurrency
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
        #region UnadjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date UnadjustedPaymentDate
        {
            get
            {

                if (null != paymentDateAdjustment && paymentDateAdjustment.adjustableDateSpecified)
                {
                    EFS_Date dt = new EFS_Date
                    {
                        DateValue = paymentDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                    };
                    return dt;
                }
                else
                    return null;

            }
        }

        #endregion UnadjustedPaymentDate
        #region ValueDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ValueDate
        {
            get
            {
                return new EFS_EventDate(valueDate, valueDate);

            }
        }
        #endregion ValueDate
        #region ExchangeRate
        public EFS_ExchangeRate ExchangeRate
        {
            get { return exchangeRate; }
        }
        #endregion ExchangeRate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxLegPayment(string pConnectionString, EFS_ExchangeCurrencyPosEnum pExchangeCurrencyPos, IFxLeg pFxLeg, DateTime pValueDate, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            Calc(pExchangeCurrencyPos, pFxLeg, pValueDate);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        private Cst.ErrLevel Calc(EFS_ExchangeCurrencyPosEnum pExchangeCurrencyPos, IFxLeg pFxLeg, DateTime pValueDate)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            IPayment payment;
            IPayment payment_ctrval;
            bool isExchangeCurrency2 = false;
            if (EFS_ExchangeCurrencyPosEnum.ExchangeCurrency1 == pExchangeCurrencyPos)
            {
                payment = pFxLeg.ExchangedCurrency1;
                payment_ctrval = pFxLeg.ExchangedCurrency2;
            }
            else
            {
                isExchangeCurrency2 = true;
                payment = pFxLeg.ExchangedCurrency2;
                payment_ctrval = pFxLeg.ExchangedCurrency1;
            }
            payerPartyReference = payment.PayerPartyReference;
            receiverPartyReference = payment.ReceiverPartyReference;
            paymentAmount = payment.PaymentAmount;
            valueDate = pValueDate;

            if (payment.PaymentDateSpecified)
                paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, payment.PaymentDate, m_DataDocument);
            if (payment.AdjustedPaymentDateSpecified)
            {
                paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DataDocument)
                {
                    adjustedDate = payment.CreateAdjustedDate(payment.AdjustedPaymentDate)
                };
            }
            else if (DtFunc.IsDateTimeFilled(valueDate))
            {
                paymentDateAdjustment = new EFS_AdjustableDate(m_Cs, m_DataDocument)
                {
                    adjustedDate = payment.CreateAdjustedDate(valueDate)
                };
            }

            #region Currency Fixing determination (Spot à fixing)
            if (pFxLeg.ExchangeRate.FxFixingSpecified)
            {
                // The fixing is supported by this exchangerate when :
                // (the payment amount of this = 0 )
                // OR
                // (the payment amount of this <> 0 AND the payment amount of her payment_ctrval is <> 0 AND isExchangeCurrency2)
                fixingSpecified = ((0 == paymentAmount.Amount.DecValue) || (0 != payment_ctrval.PaymentAmount.Amount.DecValue && isExchangeCurrency2));
            }
            if (fixingSpecified)
            {
                exchangeRate = new EFS_ExchangeRate(pFxLeg.ExchangeRate, payment_ctrval.PaymentAmount);
                fixing = new EFS_FxFixing(EventTypeFunc.FxRate, pFxLeg.ExchangeRate.FxFixing, payment_ctrval.PaymentAmount);
            }
            else
                exchangeRate = new EFS_ExchangeRate(pFxLeg.ExchangeRate);
            #endregion Currency Fixing determination (Spot à fixing)

            #region Pre-Settlement Date
            //
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, payment.PayerPartyReference.HRef,
                payment.ReceiverPartyReference.HRef,m_DataDocument, paymentAmount.DefaultOffsetPreSettlement);
            if (fixingSpecified)
                preSettlementInfo.SetOffset(pFxLeg.ExchangeRate.FxFixing.PrimaryRateSource.OTCmlId);
            else
                preSettlementInfo.SetOffset(payment.PaymentAmount.Currency, payment_ctrval.PaymentAmount.Currency);
            preSettlement = new EFS_PreSettlement(m_Cs, null, paymentDateAdjustment.adjustedDate.DateValue, payment.PaymentAmount.Currency, payment_ctrval.PaymentAmount.Currency,
                preSettlementInfo.OffsetPreSettlement, preSettlementInfo.PreSettlementMethod, m_DataDocument);
            #endregion Pre-Settlement Date

            return ret;

        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxLegPayment

    #region EFS_FxOptionBase
    public abstract class EFS_FxOptionBase
    {
        #region Members
        protected string m_Cs;
        public ITradeDate tradeDate;
        public IReference buyerPartyReference;
        public IReference sellerPartyReference;
        public IExpiryDateTime expiryDateTime;
        public EFS_Date valueDate;
        public IMoney putCurrencyAmount;
        public IMoney callCurrencyAmount;
        public ExerciseStyleEnum exerciseStyle;
        public IFxStrikePrice strikePrice;
        public EFS_FxExerciseDates[] exerciseDates;
        public bool exerciseProcedureSpecified;
        public EFS_FxExerciseProcedure exerciseProcedure;
        public SettlementTypeEnum settlementType;

        // EG 20150403 [POC]
        public Pair<Pair<IReference, IReference>, Pair<IReference, IReference>> partyReference;
        public bool marginRatioSpecified;
        public IMarginRatio marginRatio;
        public bool initialMarginSpecified;
        public IMoney initialMargin;
        #endregion Members
        #region Accessors
        #region AdjustedExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedExpiryDate
        {
            get
            {
                EFS_Date adjustedExpiryDate = new EFS_Date
                {
                    DateValue = expiryDateTime.ExpiryDate.DateValue
                };

                return adjustedExpiryDate;
            }
        }
        #endregion AdjustedExpiryDate
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
        #region ExerciseTypeOption
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExerciseTypeOption
        {
            get
            {
                if (ExerciseStyleEnum.American == exerciseStyle)
                    return EventTypeFunc.American.ToString();
                else if (ExerciseStyleEnum.Bermuda == exerciseStyle)
                    return EventTypeFunc.Bermuda.ToString();
                else if (ExerciseStyleEnum.European == exerciseStyle)
                    return EventTypeFunc.European.ToString();
                return null;
            }
        }
        #endregion ExerciseTypeOption
        #region ExpiryDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpiryDate
        {
            get { return new EFS_EventDate(expiryDateTime.ExpiryDate.DateValue, expiryDateTime.ExpiryDate.DateValue); }
        }
        #endregion ExpiryDate
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
        #region StrikePrice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public IFxStrikePrice StrikePrice
        {
            get
            {
                return strikePrice;

            }
        }
        #endregion StrikePrice

        // EG 20150403 (POC]
        #region CustodianPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CustodianPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if ((null != partyReference) && (null != partyReference.Second) && (null != partyReference.Second.Second))
                    hRef = partyReference.Second.Second.HRef;
                return hRef;

            }
        }
        #endregion CustodianPartyReference
        #region DealerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string DealerPartyReference
        {
            get
            {
                string hRef = string.Empty;
                if ((null != partyReference) && (null != partyReference.Second) && (null != partyReference.Second.First))
                    hRef = partyReference.Second.First.HRef;
                return hRef;
            }
        }
        #endregion DealerPartyReference
        #region InitialMarginPayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                string hRef = DealerPartyReference;
                if (StrFunc.IsEmpty(hRef) && (null != partyReference))
                    hRef = partyReference.First.First.HRef;
                return hRef;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                string hRef = CustodianPartyReference;
                if (StrFunc.IsEmpty(hRef) && (null != partyReference))
                    hRef = partyReference.First.First.HRef;
                return hRef;
            }
        }
        #endregion InitialMarginReceiverPartyReference

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
        public EFS_FxOptionBase(string pConnectionString, ITradeDate pTradeDate, IFxOptionLeg pFxOptionLeg, DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness)
        {
            m_Cs = pConnectionString;
            tradeDate = pTradeDate;
            buyerPartyReference = pFxOptionLeg.BuyerPartyReference;
            sellerPartyReference = pFxOptionLeg.SellerPartyReference;
            expiryDateTime = pFxOptionLeg.ExpiryDateTime;
            valueDate = pFxOptionLeg.ValueDate;
            putCurrencyAmount = pFxOptionLeg.PutCurrencyAmount;
            callCurrencyAmount = pFxOptionLeg.CallCurrencyAmount;
            exerciseStyle = pFxOptionLeg.ExerciseStyle;
            strikePrice = pFxOptionLeg.FxStrikePrice;
            settlementType = pFxOptionLeg.CashSettlementTermsSpecified ? SettlementTypeEnum.Cash : SettlementTypeEnum.Physical;
            SetExerciseDates(pFxOptionLeg.BermudanExerciseDatesSpecified, pFxOptionLeg.BermudanExerciseDates, settlementType);
            SetExerciseProcedure(pFxOptionLeg.ProcedureSpecified, pFxOptionLeg.Procedure, settlementType);

            FxOptionLegContainer fxOptionLegContainer = new FxOptionLegContainer(pFxOptionLeg, pDataDocument);
            fxOptionLegContainer.InitRptSide(m_Cs, (pStatusBusiness == Cst.StatusBusiness.ALLOC));
            partyReference = fxOptionLegContainer.GetPartyReference();

            #region InitialMargin
            marginRatioSpecified = fxOptionLegContainer.MarginRatioSpecified;
            marginRatio = fxOptionLegContainer.MarginRatio;
            initialMarginSpecified = marginRatioSpecified;
            if (initialMarginSpecified)
            {
                IProductBase productBase = (IProductBase)fxOptionLegContainer.DataDocument.CurrentProduct.ProductBase;
                IMoney amountPremium = productBase.CreateMoney();
                if (pFxOptionLeg.FxOptionPremiumSpecified)
                {
                    if (ArrFunc.IsFilled(pFxOptionLeg.FxOptionPremium) && 
                        (null != pFxOptionLeg.FxOptionPremium[0].Efs_FxOptionPremium))
                    {
                        EFS_FxOptionPremium premium = pFxOptionLeg.FxOptionPremium[0].Efs_FxOptionPremium;
                        amountPremium.Amount.DecValue = premium.PremiumAmount.DecValue;
                        amountPremium.Currency = premium.PremiumCurrency;
                    }
                }
                switch (fxOptionLegContainer.MarginRatio.PriceExpression)
                {
                    case PriceExpressionEnum.PercentageOfNotional:
                        initialMargin = productBase.CreateMoney();
                        initialMargin.Amount = new EFS_Decimal(marginRatio.Amount.DecValue * amountPremium.Amount.DecValue);
                        initialMargin.Currency = amountPremium.Currency;
                        break;
                    case PriceExpressionEnum.AbsoluteTerms:
                        initialMargin = productBase.CreateMoney();
                        initialMargin.Amount = new EFS_Decimal(marginRatio.Amount.DecValue);
                        initialMargin.Currency = marginRatio.CurrencySpecified ? marginRatio.Currency.Value : amountPremium.Currency;
                        break;
                }
            }
            #endregion InitialMargin

        }
        public EFS_FxOptionBase(string pConnectionString, ITradeDate pTradeDate, IFxBarrierOption pFxBarrierOption)
        {
            m_Cs = pConnectionString;
            tradeDate = pTradeDate;
            buyerPartyReference = pFxBarrierOption.BuyerPartyReference;
            sellerPartyReference = pFxBarrierOption.SellerPartyReference;
            expiryDateTime = pFxBarrierOption.ExpiryDateTime;
            valueDate = pFxBarrierOption.ValueDate;
            putCurrencyAmount = pFxBarrierOption.PutCurrencyAmount;
            callCurrencyAmount = pFxBarrierOption.CallCurrencyAmount;
            exerciseStyle = pFxBarrierOption.ExerciseStyle;
            settlementType = pFxBarrierOption.CashSettlementTermsSpecified ? SettlementTypeEnum.Cash : SettlementTypeEnum.Physical;
            SetExerciseDates(pFxBarrierOption.BermudanExerciseDatesSpecified, pFxBarrierOption.BermudanExerciseDates, settlementType);
            SetExerciseProcedure(pFxBarrierOption.ProcedureSpecified, pFxBarrierOption.Procedure, settlementType);

        }
        public EFS_FxOptionBase(string pConnectionString, ITradeDate pTradeDate, IFxAverageRateOption pFxAverageRateOption)
        {
            m_Cs = pConnectionString;
            SettlementTypeEnum settlementType = SettlementTypeEnum.Cash;
            tradeDate = pTradeDate;
            buyerPartyReference = pFxAverageRateOption.BuyerPartyReference;
            sellerPartyReference = pFxAverageRateOption.SellerPartyReference;
            expiryDateTime = pFxAverageRateOption.ExpiryDateTime;
            valueDate = pFxAverageRateOption.ValueDate;
            putCurrencyAmount = pFxAverageRateOption.PutCurrencyAmount;
            callCurrencyAmount = pFxAverageRateOption.CallCurrencyAmount;
            exerciseStyle = pFxAverageRateOption.ExerciseStyle;
            settlementType = pFxAverageRateOption.AverageStrikeOptionSpecified ? pFxAverageRateOption.AverageStrikeOption.SettlementType : settlementType;
            SetExerciseDates(pFxAverageRateOption.BermudanExerciseDatesSpecified, pFxAverageRateOption.BermudanExerciseDates, settlementType);
            SetExerciseProcedure(pFxAverageRateOption.ProcedureSpecified, pFxAverageRateOption.Procedure, settlementType);
        }
        #endregion Constructors
        #region Methods
        #region SetExerciseDates
        private void SetExerciseDates(bool pIsBermuda, IDateList pBermudanDates, SettlementTypeEnum pSettlementType)
        {
            DateTime startPeriod = tradeDate.DateValue;
            DateTime endPeriod = expiryDateTime.ExpiryDate.DateValue;
            if (ExerciseStyleEnum.American == exerciseStyle)
            {
                exerciseDates = new EFS_FxExerciseDates[1];
                exerciseDates[0] = new EFS_FxExerciseDates(EventTypeFunc.American, startPeriod, endPeriod, pSettlementType);
            }
            else if (ExerciseStyleEnum.Bermuda == exerciseStyle && pIsBermuda)
            {
                exerciseDates = new EFS_FxExerciseDates[pBermudanDates.Date.Length];
                for (int i = 0; i < exerciseDates.Length; i++)
                {
                    DateTime exerciseDate = pBermudanDates[i];
                    exerciseDates[i] = new EFS_FxExerciseDates(EventTypeFunc.Bermuda, startPeriod, endPeriod, exerciseDate, pSettlementType);
                }
            }
            else if (ExerciseStyleEnum.European == exerciseStyle)
            {
                exerciseDates = new EFS_FxExerciseDates[1];
                exerciseDates[0] = new EFS_FxExerciseDates(EventTypeFunc.European, endPeriod, endPeriod, pSettlementType);
            }
        }
        #endregion SetExerciseDates
        #region SetExerciseProcedure
        private void SetExerciseProcedure(bool pIsExerciseProcedure, IExerciseProcedure pExerciseProcedure, SettlementTypeEnum pSettlementType)
        {
            if (pIsExerciseProcedure)
            {
                if (pExerciseProcedure.ExerciseProcedureAutomaticSpecified ||
                    pExerciseProcedure.ExerciseProcedureManual.FallbackExerciseSpecified)
                {
                    exerciseProcedureSpecified = pIsExerciseProcedure;
                    exerciseProcedure = new EFS_FxExerciseProcedure(pExerciseProcedure, pSettlementType);
                }
            }
        }
        #endregion SetExerciseProcedure
        #endregion Methods
    }
    #endregion EFS_FxOptionBase
    #region EFS_FxOptionPayout
    public class EFS_FxOptionPayout
    {
        #region Members
        private readonly string m_Cs;
        public IMoney payoutAmount;
        public bool exchangeRateSpecified;
        public EFS_ExchangeRate exchangeRate;

        public bool originalPaymentSpecified;
        public EFS_OriginalPayment originalPayment;
        #endregion Members
        #region Accessors
        #region ExchangeRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ExchangeRate ExchangeRate
        {
            get
            {
                return exchangeRate;

            }
        }
        #endregion ExchangeRate
        #region PayoutAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PayoutAmount
        {
            get
            {
                return payoutAmount.Amount;

            }
        }
        #endregion PayoutAmount
        #region PayoutCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayoutCurrency
        {
            get
            {
                return payoutAmount.Currency;

            }
        }
        #endregion PayoutCurrency
        #endregion Accessors
        #region Constructors
        public EFS_FxOptionPayout(string pConnectionString, IFxOptionPayout pFxOptionPayout)
        {
            m_Cs = pConnectionString;
            IMoney payoutAmount = pFxOptionPayout.CreateMoney(pFxOptionPayout.Amount.DecValue, pFxOptionPayout.Currency);
            Calc(payoutAmount, pFxOptionPayout);
            if (originalPaymentSpecified)
                originalPayment = new EFS_OriginalPayment(payoutAmount);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        private void Calc(IMoney pPayoutAmount, IFxOptionPayout pFxOptionPayout)
        {

            originalPaymentSpecified = pFxOptionPayout.CustomerSettlementPayoutSpecified;
            if (originalPaymentSpecified)
            {
                ICustomerSettlementPayment settlementPayout = pFxOptionPayout.CustomerSettlementPayout;
                exchangeRateSpecified = (null != settlementPayout.Rate);
                if (exchangeRateSpecified)
                    exchangeRate = new EFS_ExchangeRate(settlementPayout.Rate);

                if ((false == settlementPayout.AmountSpecified) || (0 == settlementPayout.Amount.DecValue))
                {
                    if (exchangeRateSpecified)
                    {
                        decimal rate = exchangeRate.exchangeRate.Rate.DecValue;
                        IQuotedCurrencyPair quote = exchangeRate.exchangeRate.QuotedCurrencyPair;
                        EFS_Cash cash = new EFS_Cash(m_Cs, pPayoutAmount, rate, quote);
                        payoutAmount = pFxOptionPayout.CreateMoney(cash.ExchangeAmountRounded, settlementPayout.Currency);
                    }
                }
                else
                {
                    payoutAmount = pFxOptionPayout.CreateMoney(settlementPayout.Amount.DecValue, settlementPayout.Currency);
                }
            }
            else
            {
                payoutAmount = pPayoutAmount;
            }

        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxOptionPayout
    #region EFS_FxOptionPremium
    /// <revision>
    ///     <version>1.1.8</version><date>20070823</date><author>EG</author>
    ///     <comment>
    ///     Apply Rules to determine the date of Pre-Settlement events
    ///     </comment>
    /// </revision>
    public class EFS_FxOptionPremium
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        public IReference payerPartyReference;
        public IReference receiverPartyReference;
        public IMoney premiumAmount;
        public EFS_EventDate expiryDate;
        public EFS_Date premiumSettlementDate;
        public bool exchangeRateSpecified;
        public EFS_ExchangeRate exchangeRate;
        public bool premiumQuoteSpecified;
        public EFS_FxPremiumQuote premiumQuote;
        public EFS_PreSettlement preSettlement;
        public bool preSettlementSpecified;

        public bool originalPaymentSpecified;
        public EFS_OriginalPayment originalPayment;
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
        #region AdjustedSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedSettlementDate
        {
            get
            {
                return premiumSettlementDate;

            }
        }
        #endregion AdjustedSettlementDate
        #region ExchangeRate
        public EFS_ExchangeRate ExchangeRate
        {
            get { return exchangeRate; }
        }
        #endregion ExchangeRate
        #region ExpirationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ExpirationDate
        {
            get
            {
                return expiryDate;

            }
        }
        #endregion ExpirationDate
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
        #region PremiumAmount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal PremiumAmount
        {
            get
            {
                return this.premiumAmount.Amount;

            }
        }
        #endregion PremiumAmount
        #region PremiumCurrency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PremiumCurrency
        {
            get
            {
                return this.premiumAmount.Currency;

            }
        }
        #endregion PremiumCurrency
        #region SettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate SettlementDate
        {
            get
            {
                return new EFS_EventDate(premiumSettlementDate.DateValue, premiumSettlementDate.DateValue);

            }
        }
        #endregion SettlementDate
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
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxOptionPremium(string pConnectionString, IExpiryDateTime pExpiryDateTime, IFxOptionPremium pFxOptionPremium,
            IMoney pCallCurrencyAmount, IMoney pPutCurrencyAmount, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            Calc(pFxOptionPremium, pExpiryDateTime, pCallCurrencyAmount, pPutCurrencyAmount);
            if (originalPaymentSpecified)
                originalPayment = new EFS_OriginalPayment(pFxOptionPremium.PremiumAmount, this);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void Calc(IFxOptionPremium pFxOptionPremium, IExpiryDateTime pExpiryDateTime, IMoney pCallCurrencyAmount, IMoney pPutCurrencyAmount)
        {

            payerPartyReference = pFxOptionPremium.PayerPartyReference;
            receiverPartyReference = pFxOptionPremium.ReceiverPartyReference;
            premiumSettlementDate = new EFS_Date(pFxOptionPremium.PremiumSettlementDate.Value);
            expiryDate = new EFS_EventDate(pExpiryDateTime.ExpiryDate.DateValue, pExpiryDateTime.ExpiryDate.DateValue);

            originalPaymentSpecified = pFxOptionPremium.CustomerSettlementPremiumSpecified;
            if (originalPaymentSpecified)
            {
                ICustomerSettlementPayment settlementPremium = pFxOptionPremium.CustomerSettlementPremium;
                exchangeRateSpecified = (null != settlementPremium.Rate);
                if (exchangeRateSpecified)
                    exchangeRate = new EFS_ExchangeRate(settlementPremium.Rate);

                if (settlementPremium.AmountSpecified)
                    premiumAmount = settlementPremium.GetMoney();
                else
                {
                    if (exchangeRateSpecified)
                    {
                        decimal rate = exchangeRate.exchangeRate.Rate.DecValue;
                        IQuotedCurrencyPair quote = exchangeRate.exchangeRate.QuotedCurrencyPair;
                        EFS_Cash cash = new EFS_Cash(m_Cs, pFxOptionPremium.PremiumAmount, rate, quote);
                        premiumAmount = pFxOptionPremium.CreateMoney(cash.ExchangeAmountRounded, settlementPremium.Currency);
                    }
                }
            }
            else
            {
                premiumQuoteSpecified = pFxOptionPremium.PremiumQuoteSpecified;
                if (premiumQuoteSpecified)
                    premiumQuote = new EFS_FxPremiumQuote(pFxOptionPremium.PremiumQuote, pCallCurrencyAmount, pPutCurrencyAmount);
                premiumAmount = (IMoney)pFxOptionPremium.PremiumAmount;
            }
            // 20070823 EG Ticket : 15643
            #region premiumPreSettlementDate
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, premiumAmount.Currency,
                payerPartyReference.HRef, receiverPartyReference.HRef, m_DataDocument, ((IMoney)premiumAmount).DefaultOffsetPreSettlement);
            preSettlementSpecified = (preSettlementInfo.IsUsePreSettlement);
            if (preSettlementSpecified)
                preSettlement = new EFS_PreSettlement(m_Cs, null, premiumSettlementDate.DateValue, premiumAmount.Currency, preSettlementInfo.OffsetPreSettlement, m_DataDocument);
            #endregion premiumPreSettlementDate

        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxOptionPremium
    #region EFS_FxPremiumQuote
    public class EFS_FxPremiumQuote
    {
        public IPremiumQuote premiumQuote;
        public string callCurrency;
        public string putCurrency;
        public IMoney amountReference;
        public bool amountReferenceSpecified;

        #region Constructors
        public EFS_FxPremiumQuote(IPremiumQuote pPremiumQuote, IMoney pCallCurrencyAmount, IMoney pPutCurrencyAmount)
        {
            premiumQuote = pPremiumQuote;
            callCurrency = pCallCurrencyAmount.Currency;
            putCurrency = pPutCurrencyAmount.Currency;
            switch (premiumQuote.PremiumQuoteBasis)
            {
                case PremiumQuoteBasisEnum.Explicit:
                    break;
                case PremiumQuoteBasisEnum.CallCurrencyPerPutCurrency:
                case PremiumQuoteBasisEnum.PercentageOfPutCurrencyAmount:
                    amountReference = pPutCurrencyAmount;
                    break;
                case PremiumQuoteBasisEnum.PutCurrencyPerCallCurrency:
                case PremiumQuoteBasisEnum.PercentageOfCallCurrencyAmount:
                    amountReference = pCallCurrencyAmount;
                    break;
            }
            amountReferenceSpecified = (null != amountReference);
        }
        #endregion Constructors
    }
    #endregion EFS_FxPremiumQuote
    #region EFS_FxPreSettlement
    public class EFS_FxPreSettlement
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument;
        private EFS_Offset m_PreSettlementDateAdjustment;
        #endregion Members
        #region Accessors
        #region PreSettlementDateAdjustment
        public EFS_Offset PreSettlementDateAdjustment
        {
            get { return m_PreSettlementDateAdjustment; }
        }
        #endregion PreSettlementDateAdjustment
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedPreSettlementDate
        {
            get
            {

                if ((null != m_PreSettlementDateAdjustment) && (0 < m_PreSettlementDateAdjustment.offsetDate.Length))
                {
                    string dt = DtFunc.DateTimeToString(m_PreSettlementDateAdjustment.offsetDate[0], DtFunc.FmtISODate);
                    return new EFS_Date(dt);
                }
                else
                    return null;

            }
        }
        #endregion AdjustedPreSettlementDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_FxPreSettlement(string pConnectionString, IProduct pProduct, DateTime pSourceDate, string pCurrency1, string pCurrency2, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            Calc(pSourceDate, pProduct, pCurrency1, pCurrency2);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private Cst.ErrLevel Calc(DateTime pSourceDate, IProduct pProduct, string pCurrency1, string pCurrency2)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;

            #region Pre-Settlement Date
            IOffset offset = pProduct.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.ExchangeBusiness);
            string[] currencies = new string[6];
            SQL_Currency ccy = new SQL_Currency(m_Cs, pCurrency1);
            if (ccy.LoadTable(new string[] { "IDC", "IDBC", "IDBC2", "IDBC3" }))
            {
                currencies[0] = ccy.IdBC;
                currencies[1] = ccy.IdBC2;
                currencies[2] = ccy.IdBC3;
            }
            if (StrFunc.IsFilled(pCurrency2))
            {
                ccy = new SQL_Currency(m_Cs, pCurrency2);
                if (ccy.LoadTable(new string[] { "IDC", "IDBC", "IDBC2", "IDBC3" }))
                {
                    currencies[3] = ccy.IdBC;
                    currencies[4] = ccy.IdBC2;
                    currencies[5] = ccy.IdBC3;
                }
            }

            IBusinessDayAdjustments bda = offset.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NONE, currencies);
            m_PreSettlementDateAdjustment = new EFS_Offset(m_Cs, offset, pSourceDate, bda, m_DataDocument);
            #endregion Pre-Settlement Date


            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_FxPreSettlement
    #region EFS_FxSimpleOption
    public class EFS_FxSimpleOption : EFS_FxOptionBase
    {
        #region Accessors
        #region EventCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventCode
        {
            get
            {
                string eventType = base.ExerciseTypeOption;
                if (EventTypeFunc.IsAmerican(eventType))
                    return EventCodeFunc.AmericanSimpleOption;
                else if (EventTypeFunc.IsBermuda(eventType))
                    return EventCodeFunc.BermudaSimpleOption;
                else if (EventTypeFunc.IsEuropean(eventType))
                    return EventCodeFunc.EuropeanSimpleOption;
                return null;
            }
        }
        #endregion EventCode
        #region EventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string EventType
        {
            get { return EventCode; }
        }
        #endregion EventType
        #endregion Accessors
        #region Constructors
        public EFS_FxSimpleOption(string pConnectionString, ITradeDate pTradeDate, IFxOptionLeg pFxOptionLeg,
            DataDocumentContainer pDataDocument, Cst.StatusBusiness pStatusBusiness) : base(pConnectionString, pTradeDate, pFxOptionLeg, pDataDocument, pStatusBusiness) { }
        #endregion Constructors
    }
    #endregion EFS_FxSimpleOption

    #region EFS_TriggerRate
    public class EFS_TriggerRate
    {
        #region Members
        public EFS_Decimal spotRate;
        public EFS_Decimal triggerRate;
        public IQuotedCurrencyPair currencyPair;
        #endregion Members
        #region Constructors
        public EFS_TriggerRate(decimal pTriggerRate, decimal pSpotRate, IQuotedCurrencyPair pQuotedCurrencyPair)
        {
            triggerRate = new EFS_Decimal(pTriggerRate);
            spotRate = new EFS_Decimal(pSpotRate);
            currencyPair = pQuotedCurrencyPair;
        }
        #endregion Constructors
    }
    #endregion EFS_TriggerRate

    #region EFS_TermDeposit
    public class EFS_TermDeposit
    {
        #region Members
        public EFS_NominalPeriod nominalPeriod;
        public EFS_TermDepositInterest interestPeriod;
        #endregion Members
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_TermDeposit(string pConnectionString, ITermDeposit pTermDeposit, DataDocumentContainer pDataDocument)
        {
            nominalPeriod = new EFS_NominalPeriod(pConnectionString, pTermDeposit, pDataDocument);
            interestPeriod = new EFS_TermDepositInterest(pConnectionString, pTermDeposit, pDataDocument);
        }
        #endregion Constructors
    }
    #endregion EFS_TermDeposit
    #region EFS_TermDepositInterest
    public class EFS_TermDepositInterest
    {
        #region Members
        private readonly string m_Cs;
        private readonly DataDocumentContainer m_DataDocument; 
        public EFS_AdjustablePeriod periodDatesAdjustment;
        public EFS_AdjustableDate paymentDateAdjustment;
        public IMoney interest;
        public EFS_Decimal fixedRate;
        public EFS_DayCountFraction dayCountFraction;
        public EFS_PreSettlement preSettlement;
        public bool preSettlementSpecified;
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
                return interest.Currency;

            }
        }
        #endregion Currency
        #region DayCountFraction
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_DayCountFraction DayCountFraction
        {
            get
            {
                return dayCountFraction;

            }
        }
        #endregion DayCountFraction
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
        #region FixedRate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal FixedRate
        {
            get
            {
                return fixedRate;

            }
        }
        #endregion FixedRate
        #region Interest
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Interest
        {
            get
            {
                return interest.Amount;

            }
        }
        #endregion Interest
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
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pTermDeposit"></param>
        /// FI 20140909 [20340] Modify
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_TermDepositInterest(string pConnectionString, ITermDeposit pTermDeposit, DataDocumentContainer pDataDocument)
        {
            m_Cs = pConnectionString;
            m_DataDocument = pDataDocument;
            IBusinessDayAdjustments bda = ((IProduct)pTermDeposit).ProductBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.NotApplicable);
            periodDatesAdjustment = new EFS_AdjustablePeriod
            {
                adjustableDate1 = new EFS_AdjustableDate(m_Cs, pTermDeposit.StartDate.DateValue, bda, m_DataDocument),
                adjustableDate2 = new EFS_AdjustableDate(m_Cs, pTermDeposit.MaturityDate.DateValue, bda, m_DataDocument)
            };
            paymentDateAdjustment = (EFS_AdjustableDate)periodDatesAdjustment.adjustableDate2.Clone();
            IInterval interval = (IInterval)bda.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.Business).GetInterval(0, PeriodEnum.D);

            dayCountFraction = new EFS_DayCountFraction(periodDatesAdjustment.adjustableDate1.adjustedDate.DateValue,
                                                        periodDatesAdjustment.adjustableDate2.adjustedDate.DateValue,
                                                        pTermDeposit.DayCountFraction, interval);

            // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
            //if (pTermDeposit.fixedRateSpecified)
            fixedRate = pTermDeposit.FixedRate;

            if (pTermDeposit.InterestSpecified)
            {
                interest = pTermDeposit.Interest;
            }
            else// if (pTermDeposit.fixedRateSpecified) // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
            {
                decimal amount = pTermDeposit.Principal.Amount.DecValue * pTermDeposit.FixedRate.DecValue * dayCountFraction.Factor;
                interest = ((IProductBase)pTermDeposit).CreateMoney(amount, pTermDeposit.Principal.Currency);
            }
            #region PreSettlementInfo
            //            
            EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_Cs, interest.Currency,
                pTermDeposit.InitialPayerReference.HRef, pTermDeposit.InitialReceiverReference.HRef, m_DataDocument, interest.DefaultOffsetPreSettlement);
            preSettlementSpecified = (null != preSettlementInfo) && preSettlementInfo.IsUsePreSettlement;
            if (preSettlementSpecified)
                preSettlement = new EFS_PreSettlement(m_Cs, null, paymentDateAdjustment.AdjustedEventDate, preSettlementInfo.Currency, preSettlementInfo.OffsetPreSettlement, m_DataDocument);
            #endregion PreSettlementInfo
        }
        #endregion Constructors
    }
    #endregion EFS_TermDepositInterest

}

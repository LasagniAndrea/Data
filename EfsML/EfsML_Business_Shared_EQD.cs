#region Using Directives
using System;
using System.Collections.Generic;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_EquityAutomaticExercise
    public class EFS_EquityAutomaticExercise : EFS_AutomaticExerciseBase
    {
        #region Members
        public bool electionDateSpecified;
        public EFS_AdjustableDate electionDate;
        #endregion Members
        #region Accessors
        #region AdjustedElectionDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Date AdjustedElectionDate
        {
            get
            {
                EFS_Date adjustedElectionDate = null;
                if (electionDateSpecified)
                {
                    adjustedElectionDate = new EFS_Date
                    {
                        DateValue = electionDate.adjustedDate.DateValue
                    };
                }
                return adjustedElectionDate;
            }
        }
        #endregion AdjustedElectionDate
        #endregion Accessors
        #region Constructors
        // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
        public EFS_EquityAutomaticExercise() { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityAutomaticExercise(string pConnectionString, IEquityExerciseValuationSettlement pEquityExercise, EFS_Date pExpiryDate, DataDocumentContainer pDataDocument)
            :base(pConnectionString, pExpiryDate, pDataDocument)
        {
            exerciseStyle = pEquityExercise.GetStyle;
            settlementType = pEquityExercise.SettlementType;
            settlementCurrencySpecified = true;
            settlementCurrency = pEquityExercise.SettlementCurrency.Value;
            expiryDate = pExpiryDate;
            settlementDateSpecified = pEquityExercise.SettlementDateSpecified;
            if (settlementDateSpecified)
                settlementDate = Tools.GetEFS_AdjustableDate(cs, pEquityExercise.SettlementDate, m_DataDocument);
            electionDateSpecified = pEquityExercise.ElectionDateSpecified;
            if (electionDateSpecified)
                electionDate = Tools.GetEFS_AdjustableDate(cs, pEquityExercise.ElectionDate, m_DataDocument);
        }
        #endregion Constructors
    }
    #endregion EFS_EquityAutomaticExercise

    #region EFS_EquityAveragingPeriod
    public class EFS_EquityAveragingPeriod : EFS_OptionFeaturesDates
    {
        #region Members
        protected string averagingCode;
        protected string averagingType;
        #endregion Members
        #region Accessors
        #region AveragingCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string AveragingCode
        {
            get { return averagingCode; }
        }
        #endregion AveragingCode
        #region AveragingType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string AveragingType
        {
            get { return averagingType; }
        }
        #endregion AveragingType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityAveragingPeriod(string pConnectionString, string pAveragingType, EFS_Underlyer pUnderlyer,
            IEquityAveragingPeriod pAveragingPeriod, IBusinessDayAdjustments pBusinessDayAdjustments, bool pIsLookBackMethod, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pUnderlyer, pAveragingPeriod.ScheduleSpecified, pAveragingPeriod.Schedule,
                  pAveragingPeriod.AveragingDateTimesSpecified, pAveragingPeriod.AveragingDateTimes, pBusinessDayAdjustments, pDataDocument)
        {
            averagingCode = EventCodeFunc.Asian;
            averagingType = pAveragingType;
            if (pIsLookBackMethod)
            {
                averagingCode = EventCodeFunc.LookBackOption;
                averagingType = (EventTypeFunc.IsAveragingIn(averagingType) ? EventTypeFunc.LookBackIn : EventTypeFunc.LookBackOut);
            }
        }
        #endregion Constructors
    }
    #endregion EFS_EquityAveragingPeriod

    #region EFS_EquityExercise
    public class EFS_EquityExercise : EFS_OptionExercise
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityExercise(string pCS, IEquityOption pEquityOption, EFS_Underlyer pUnderlyer, EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
            : base(pCS, pEquityOption.EquityExercise.GetStyle, pEquityOption.EquityExercise.SettlementType, true, pMarketScheduleTradingDay, pDataDocument)
        {
            IEquityExerciseValuationSettlement equityExercise = pEquityOption.EquityExercise;
            Pair<EFS_AdjustableDate, EFS_AdjustableDate> exerciseDateRange = null;
            IDateList lstDates = null;
            switch (exerciseStyle)
            {
                case ExerciseStyleEnum.American:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, equityExercise.EquityExerciseAmerican, m_DataDocument);
                    break;
                case ExerciseStyleEnum.Bermuda:
                    IEquityBermudaExercise bermudaExercise = equityExercise.EquityExerciseBermuda;
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, bermudaExercise, m_DataDocument);
                    lstDates = bermudaExercise.BermudaExerciseDates;
                    break;
                case ExerciseStyleEnum.European:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, equityExercise.EquityExerciseEuropean, m_DataDocument);
                    break;
            }
            commencementDate = exerciseDateRange.First;
            expirationDate = exerciseDateRange.Second;
            exerciseDates = new EFS_EquityExerciseDates[(null == lstDates) ? 1 : lstDates.Date.Length];
            exerciseDates[0] = new EFS_EquityExerciseDates(m_Cs, exerciseStyle, pUnderlyer, settlementType.Value, commencementDate, 
                expirationDate, lstDates, pMarketScheduleTradingDay, m_DataDocument);
        }
        #endregion Constructors
    }
    #endregion EFS_EquityExercise

    #region EFS_EquityExerciseDates
    public class EFS_EquityExerciseDates : EFS_OptionExerciseDates
    {
        #region Constructors
        public EFS_EquityExerciseDates() { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityExerciseDates(string pCS, ExerciseStyleEnum pExerciseStyle, EFS_Underlyer pUnderlyer, SettlementTypeEnum pSettlementType,
            EFS_AdjustableDate pCommencementDate, EFS_AdjustableDate pExpiryDate, 
            EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
            : this(pCS, pExerciseStyle, pUnderlyer, pSettlementType, pCommencementDate, pExpiryDate, null, pMarketScheduleTradingDay, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityExerciseDates(string pCS, ExerciseStyleEnum pExerciseStyle, EFS_Underlyer pUnderlyer, SettlementTypeEnum pSettlementType,
            EFS_AdjustableDate pCommencementDate, EFS_AdjustableDate pExpiryDate, IDateList pExerciseDates,
            EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
            : base(pCS, pExerciseStyle, pSettlementType, pCommencementDate, pExpiryDate, pMarketScheduleTradingDay, pDataDocument)
        {
            CalculExerciseValuationDates(pUnderlyer, pExerciseDates);
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion EFS_EquityExerciseDates



    #region EFS_EquityMarketScheduleTradingDay
    public class EFS_EquityMarketScheduleTradingDay : EFS_MarketScheduleTradingDayBase
    {
        #region Constructors
        public EFS_EquityMarketScheduleTradingDay(string pConnectionString, IEquityOption pEquityOption)
            :base (pConnectionString,(IProductBase)pEquityOption)
        {
            LstExchangeTraded = new List<IExchangeTraded>();
            if (pEquityOption.Underlyer.UnderlyerBasketSpecified)
            {
                foreach (IBasketConstituent constituent in pEquityOption.Underlyer.UnderlyerBasket.BasketConstituent)
                {
                    LstExchangeTraded.Add((IExchangeTraded)constituent.UnderlyingAsset);
                }
            }
            else if (pEquityOption.Underlyer.UnderlyerSingleSpecified)
            {
                LstExchangeTraded.Add((IExchangeTraded)pEquityOption.Underlyer.UnderlyerSingle.UnderlyingAsset);
            }

            Initialize();
        }
        #endregion Constructors
    }
    #endregion EFS_EquityMarketScheduleTradingDay

    #region EFS_EquityOptionNotional
    public class EFS_EquityOptionNotional : EFS_OptionNotionalBase
    {
        #region Accessors
        #endregion Accessors
        #region Constructors
        public EFS_EquityOptionNotional(IReference pPayerPartyReference, IReference pReceiverPartyReference, IMoney pNotional)
            :base (pPayerPartyReference,pReceiverPartyReference,pNotional)
        {
        }
        #endregion Constructors
    }
    #endregion EFS_EquityOptionNotional

    #region EFS_EquityOption
    // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
    public class EFS_EquityOption : EFS_EquityOptionBase
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityOption(string pConnectionString, IEquityOption pEquityOption, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pEquityOption, pDataDocument)
        {
            numberOfOptions = new EFS_Decimal(equityOption.NumberOfOptionsSpecified ? equityOption.NumberOfOptions.DecValue : 1);
            optionEntitlement = new EFS_Decimal(Math.Max(1, equityOption.OptionEntitlement.DecValue));

            notional = null;
            if (equityOption.NotionalSpecified)
            {
                notional = new EFS_EquityOptionNotional(NotionalPayerPartyReference, NotionalReceiverPartyReference, equityOption.Notional);
            }
            else if (strike.strikeAmountSpecified)
            {
                decimal result = numberOfOptions.DecValue * optionEntitlement.DecValue * strike.strikeAmount.Amount.DecValue;
                IMoney money = ((IProduct)equityOption).ProductBase.CreateMoney(result, strike.strikeAmount.Currency);
                notional = new EFS_EquityOptionNotional(NotionalPayerPartyReference, NotionalReceiverPartyReference, money);
            }
            //else
            //    throw new NotImplementedException("EquityNotional not implemented");

        }
        #endregion Constructors
    }
    #endregion EFS_EquityOption

    #region EFS_EquityOptionBase
    // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
    public class EFS_EquityOptionBase : EFS_OptionBase
    {
        #region Members
        protected IEquityOption equityOption;
        public bool singleUnderlyerSpecified;
        public bool underlyerBasketSpecified;
        public EFS_EquityExercise exercise;
        #endregion Members
        #region Accessors
        #region EventCode
        public override string EventCode
        {
            get
            {
                string eventCode = string.Empty;
                if (ExerciseStyleEnum.American == exercise.exerciseStyle)
                    eventCode = EventCodeFunc.AmericanEquityOption;
                else if (ExerciseStyleEnum.Bermuda == exercise.exerciseStyle)
                    eventCode = EventCodeFunc.BermudaEquityOption;
                else if (ExerciseStyleEnum.European == exercise.exerciseStyle)
                    eventCode = EventCodeFunc.EuropeanEquityOption;
                return eventCode;
            }
        }
        #endregion EquityOptionCode

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string PayerPartyReference
        {
            get
            {
                return underlyer.payerPartyReference.HRef;

            }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ReceiverPartyReference
        {
            get
            {
                return underlyer.receiverPartyReference.HRef;

            }
        }
        #endregion ReceiverPartyReference

        #region UnderlyerType
        public override string UnderlyerType
        {
            get
            {
                return (singleUnderlyerSpecified ? EventTypeFunc.SingleUnderlyer : EventTypeFunc.Basket);
            }
        }
        #endregion UnderlyerType

        #region UnderlyingAsset
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset UnderlyingAsset
        {
            get {return underlyer.asset;}
        }
        #endregion UnderlyingAsset

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
        public EFS_EquityOptionBase(string pConnectionString, IEquityOption pEquityOption, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pEquityOption.BuyerPartyReference, pEquityOption.SellerPartyReference, pEquityOption.OptionType, pDataDocument)
        {
            equityOption = pEquityOption;

            strike = new EFS_EquityStrike(m_Cs, equityOption, m_DataDocument);

            singleUnderlyerSpecified = equityOption.Underlyer.UnderlyerSingleSpecified;
            underlyerBasketSpecified = equityOption.Underlyer.UnderlyerBasketSpecified;
            if (singleUnderlyerSpecified)
                underlyer = new EFS_SingleUnderlyer(m_Cs, optionType, buyerPartyReference, sellerPartyReference, equityOption.Underlyer.UnderlyerSingle);
            else if (underlyerBasketSpecified)
                underlyer = new EFS_Basket(m_Cs, optionType, buyerPartyReference, sellerPartyReference, equityOption.Underlyer.UnderlyerBasket);

            marketScheduleTradingDay = new EFS_EquityMarketScheduleTradingDay(m_Cs, equityOption);
            exercise = new EFS_EquityExercise(m_Cs, equityOption, underlyer, marketScheduleTradingDay, m_DataDocument);

            effectiveDate = new EFS_Date();
            if (equityOption.EquityEffectiveDateSpecified)
                effectiveDate.DateValue = equityOption.EquityEffectiveDate.DateValue;
            else
                effectiveDate.DateValue = tradeDate.DateValue;
            expiryDate = new EFS_Date();
            if (null != exercise.expirationDate.adjustedDate)
                expiryDate.DateValue = exercise.expirationDate.adjustedDate.DateValue;

            automaticExerciseSpecified = equityOption.EquityExercise.AutomaticExerciseSpecified && equityOption.EquityExercise.AutomaticExercise;
            if (automaticExerciseSpecified)
                automaticExercise = new EFS_EquityAutomaticExercise(m_Cs, equityOption.EquityExercise, expiryDate, m_DataDocument);

            isWithFeatures = equityOption.FeatureSpecified;
            SetFeatures(equityOption.Feature);
        }
        #endregion Constructors
    }
    #endregion EFS_EquityOptionBase

    #region EFS_EquityOptionPremium
    public class EFS_EquityOptionPremium : EFS_OptionPremiumBase
    {
        #region Members
        // EG 20160404 Migration vs2013
        //public EFS_Date expiryDate;
        public bool swapPremiumSpecified;
        public EFS_Boolean swapPremium;
        #endregion Members
        #region Accessors
        #region ExpiryDate
        // EG 20160404 Migration vs2013
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public EFS_EventDate ExpiryDate
        //{
        //    get { return new EFS_EventDate(expiryDate.DateValue, expiryDate.DateValue); }
        //}
        #endregion ExpiryDate
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityOptionPremium(string pConnnectionString, IEquityOption pEquityOption, DataDocumentContainer pDataDocument)
            : base(pConnnectionString, (IProductBase)pEquityOption, (INbOptionsAndNotionalBase)pEquityOption, (IPremiumBase)pEquityOption.EquityPremium, pDataDocument)
        {
            expiryDate = pEquityOption.Efs_EquityOption.expiryDate;
            IEquityPremium equityPremium = pEquityOption.EquityPremium;
            settlementDateSpecified = equityPremium.PaymentDateSpecified;
            if (settlementDateSpecified)
            {
                settlementDate = new EFS_AdjustableDate(m_Cs, equityPremium.PaymentDate, m_DataDocument);
                SetPreSettlement();
            }

        }
        #endregion Constructors
    }
    #endregion EFS_EquityOptionPremium

    #region EFS_EquityStrike
    public class EFS_EquityStrike : EFS_OptionStrikeBase
    {
        #region Members
        public bool strikeDateSpecified;
        public EFS_AdjustableDate strikeDate;
        public string currency;
        #endregion Members
        #region Accessors
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_EquityStrike(string pCS, IEquityOption pEquityOption, DataDocumentContainer pDataDocument)
            : base(pCS, pDataDocument)
        {
            #region EquityStrike
            IEquityStrike equityStrike = pEquityOption.Strike;
            spotPriceSpecified = pEquityOption.SpotPriceSpecified;
            spotPrice = pEquityOption.SpotPrice;
            if (equityStrike.PriceSpecified)
            {
                strikePriceSpecified = equityStrike.PriceSpecified;
                strikePrice = equityStrike.Price;
            }
            else if (equityStrike.PercentageSpecified)
            {
                #region Percentage of the spotPrice
                strikePriceSpecified = spotPriceSpecified;
                strikeDateSpecified = equityStrike.StrikeDeterminationDateSpecified;
                if (strikePriceSpecified)
                    strikePrice = new EFS_Decimal(equityStrike.Percentage.DecValue * pEquityOption.SpotPrice.DecValue);
                else if (strikeDateSpecified)
                    strikeDate = Tools.GetEFS_AdjustableDate(cs, equityStrike.StrikeDeterminationDate, m_DataDocument);
                #endregion Percentage of the spotPrice
            }
            strikeAmountSpecified = equityStrike.CurrencySpecified && strikePriceSpecified;
            if (strikeAmountSpecified)
                strikeAmount = ((IProduct)pEquityOption).ProductBase.CreateMoney(strikePrice.DecValue, equityStrike.Currency.Value);
            #endregion EquityStrike
        }
        #endregion Constructors
    }
    #endregion EFS_EquityStrike


}

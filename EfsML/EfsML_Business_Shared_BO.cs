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
using System.Collections.Generic;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_BondOption
    /// <summary>
    /// Point d'entrée pour la génération des événements
    /// </summary>
    public class EFS_BondOption : EFS_BondOptionBase
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondOption(string pConnectionString, IDebtSecurityOption pDebtSecurityOption, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pDebtSecurityOption, pDataDocument)
        {
            numberOfOptions = new EFS_Decimal(debtSecurityOption.NumberOfOptionsSpecified ? debtSecurityOption.NumberOfOptions.DecValue : 1);
            optionEntitlement = new EFS_Decimal(Math.Max(1, debtSecurityOption.OptionEntitlement.DecValue));
            entitlementCurrencySpecified = debtSecurityOption.EntitlementCurrencySpecified;
            if (entitlementCurrencySpecified)
                entitlementCurrency = debtSecurityOption.EntitlementCurrency.Value;

            notional = null;
            if (debtSecurityOption.NotionalAmountSpecified)
            {
                notional = new EFS_BondOptionNotional(NotionalPayerPartyReference, NotionalReceiverPartyReference, debtSecurityOption.NotionalAmount);
            }
            else if (strike.strikeAmountSpecified)
            {
                decimal result = numberOfOptions.DecValue * optionEntitlement.DecValue * strike.strikeAmount.Amount.DecValue;
                IMoney money = ((IProduct)debtSecurityOption).ProductBase.CreateMoney(result, strike.strikeAmount.Currency);
                notional = new EFS_BondOptionNotional(NotionalPayerPartyReference, NotionalReceiverPartyReference, money);
            }
        }
        #endregion Constructors
    }
    #endregion EFS_BondOption

    #region EFS_BondOptionBase
    public abstract class EFS_BondOptionBase : EFS_OptionBase
    {
        #region Members
        protected IDebtSecurityOption debtSecurityOption;
        public bool entitlementCurrencySpecified;
        public string entitlementCurrency;
        public EFS_BondOptionExercise exercise;
        #endregion Members
        #region Accessors
        #region EventCode
        public override string EventCode
        {
            get
            {
                string eventCode = string.Empty;
                if (ExerciseStyleEnum.American == exercise.exerciseStyle)
                    eventCode = EventCodeFunc.AmericanBondOption;
                else if (ExerciseStyleEnum.Bermuda == exercise.exerciseStyle)
                    eventCode = EventCodeFunc.BermudaBondOption;
                else if (ExerciseStyleEnum.European == exercise.exerciseStyle)
                    eventCode = EventCodeFunc.EuropeanBondOption;
                return eventCode;
            }
        }
        #endregion EventCode
        #region EventType
        public override string EventType
        {
            get { return debtSecurityOption.OptionType == OptionTypeEnum.Call ? EventTypeFunc.Call : EventTypeFunc.Put; }
        }
        #endregion EventType
        #region ExerciseEventClass
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExerciseEventClass
        {
            get 
            {
                string eventClass = EventClassFunc.GroupLevel;
                if (debtSecurityOption.SettlementTypeSpecified)
                {
                    switch (debtSecurityOption.SettlementType)
                    {
                        case SettlementTypeEnum.Cash:
                            eventClass = EventClassFunc.CashSettlement;
                            break;
                        case SettlementTypeEnum.Physical:
                            eventClass = EventClassFunc.PhysicalSettlement;
                            break;
                        case SettlementTypeEnum.Election:
                            eventClass = EventClassFunc.ElectionSettlement;
                            break;
                    }
                }
                return eventClass; 
            }
        }
        #endregion ExerciseEventClass


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
        #region UnderlyingAsset
        public object UnderlyingAsset
        {
            get { return underlyer.asset; }
        }
        #endregion UnderlyingAsset

        #region UnderlyerType
        public override string UnderlyerType
        {
            get
            {
                return (debtSecurityOption.ConvertibleBondSpecified ? EventTypeFunc.ConvertibleBond : EventTypeFunc.Bond);
            }
        }
        #endregion UnderlyerType
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondOptionBase(string pConnectionString, IDebtSecurityOption pDebtSecurityOption, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pDebtSecurityOption.BuyerPartyReference, pDebtSecurityOption.SellerPartyReference, pDebtSecurityOption.OptionType, pDataDocument)
        {
            debtSecurityOption = pDebtSecurityOption;
            if (debtSecurityOption.SettlementTypeSpecified)
                strike = new EFS_BondOptionStrike(m_Cs, debtSecurityOption, m_DataDocument);

            Nullable<decimal> notionalAmount = null;
            if (debtSecurityOption.NotionalAmountSpecified)
                notionalAmount = debtSecurityOption.NotionalAmount.Amount.DecValue;
            Nullable<decimal> parAmount = null;
            if (debtSecurityOption.BondSpecified && debtSecurityOption.Bond.ParValueSpecified)
                parAmount = debtSecurityOption.Bond.ParValue.DecValue;
            else if (debtSecurityOption.ConvertibleBondSpecified && debtSecurityOption.ConvertibleBond.ParValueSpecified)
                parAmount = debtSecurityOption.ConvertibleBond.ParValue.DecValue;

            if (debtSecurityOption.SecurityAssetSpecified)
                underlyer = new EFS_BondUnderlyer(m_Cs, optionType, buyerPartyReference, sellerPartyReference, debtSecurityOption.SecurityAsset, notionalAmount, parAmount);
            else if (debtSecurityOption.BondSpecified)
                underlyer = new EFS_BondUnderlyer(m_Cs, optionType, buyerPartyReference, sellerPartyReference, debtSecurityOption.Bond, notionalAmount, parAmount);
            else if (debtSecurityOption.ConvertibleBondSpecified)
                underlyer = new EFS_BondUnderlyer(m_Cs, optionType, buyerPartyReference, sellerPartyReference, debtSecurityOption.ConvertibleBond, notionalAmount, parAmount);

            marketScheduleTradingDay = new EFS_BondMarketScheduleTradingDay(m_Cs, debtSecurityOption);
            exercise = new EFS_BondOptionExercise(m_Cs, debtSecurityOption, marketScheduleTradingDay, m_DataDocument);

            effectiveDate = new EFS_Date(tradeDate.Value);
            expiryDate = new EFS_Date(exercise.expirationDate.adjustedDate.Value);

            automaticExerciseSpecified = debtSecurityOption.ExerciseProcedure.ExerciseProcedureAutomaticSpecified;
            if (automaticExerciseSpecified)
                automaticExercise = new EFS_BondAutomaticExercise(m_Cs, debtSecurityOption, expiryDate, m_DataDocument);

            isWithFeatures = debtSecurityOption.FeatureSpecified;
            if (isWithFeatures)
                SetFeatures(debtSecurityOption.Feature);
        }
        #endregion Constructors
    }
    #endregion EFS_BondOptionBase

    #region EFS_BondAutomaticExercise
    // EG 20170220 [22842] Upd
    public class EFS_BondAutomaticExercise : EFS_AutomaticExerciseBase
    {
        #region Members
        public decimal thresholdRate;
        #endregion Members
        #region Constructors
        public EFS_BondAutomaticExercise() { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondAutomaticExercise(string pConnectionString, IOptionBaseExtended pOptionBaseExtended, EFS_Date pExpiryDate, DataDocumentContainer pDataDocument)
            : base(pConnectionString, pExpiryDate, pDataDocument)
        {
            exerciseStyle = pOptionBaseExtended.GetStyle;

            if (pOptionBaseExtended.SettlementTypeSpecified)
                settlementType = pOptionBaseExtended.SettlementType;

            settlementCurrencySpecified = pOptionBaseExtended.SettlementCurrencySpecified;
            if (settlementCurrencySpecified)
                settlementCurrency = pOptionBaseExtended.SettlementCurrency.Value;

            settlementDateSpecified = pOptionBaseExtended.SettlementDateSpecified;
            if (settlementDateSpecified)
                settlementDate = Tools.GetEFS_AdjustableDate(cs, pOptionBaseExtended.SettlementDate, m_DataDocument);
            thresholdRate = pOptionBaseExtended.ExerciseProcedure.ExerciseProcedureAutomatic.ThresholdRate;
        }
        #endregion Constructors
    }
    #endregion EFS_BondAutomaticExercise

    #region EFS_BondOptionExercise
    // EG 20170220 [22842] Upd
    public class EFS_BondOptionExercise : EFS_OptionExercise
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exerciseDatesEventsSpecified;
        public EFS_ExerciseDatesEvent[] exerciseDatesEvents;

        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondOptionExercise(string pCS, IDebtSecurityOption pDebtSecurityOption, 
            EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
            : base(pCS, pDebtSecurityOption.GetStyle, pDebtSecurityOption.SettlementType, pDebtSecurityOption.SettlementTypeSpecified, 
            pMarketScheduleTradingDay, pDataDocument)
        {

            Pair<EFS_AdjustableDate, EFS_AdjustableDate> exerciseDateRange = null;
            List<EFS_AdjustableDate> lstRelevantUnderlyingDates = new List<EFS_AdjustableDate>();

            switch (exerciseStyle)
            {
                case ExerciseStyleEnum.American:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, pDebtSecurityOption.AmericanExercise, m_DataDocument);
                    lstRelevantUnderlyingDates = ExerciseTools.GetExerciseRelevantUnderlyingDates(m_Cs, pDebtSecurityOption.AmericanExercise, m_DataDocument);
                    break;
                case ExerciseStyleEnum.Bermuda:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, pDebtSecurityOption.BermudaExercise, m_DataDocument);
                    lstRelevantUnderlyingDates = ExerciseTools.GetExerciseRelevantUnderlyingDates(m_Cs, pDebtSecurityOption.BermudaExercise, m_DataDocument);
                    break;
                case ExerciseStyleEnum.European:
                    exerciseDateRange = ExerciseTools.GetExerciseDateRange(m_Cs, pDebtSecurityOption.EuropeanExercise, m_DataDocument);
                    lstRelevantUnderlyingDates = ExerciseTools.GetExerciseRelevantUnderlyingDates(m_Cs, pDebtSecurityOption.EuropeanExercise, m_DataDocument);
                    break;
            }

            commencementDate = exerciseDateRange.First;
            expirationDate = exerciseDateRange.Second;
            SetAdjustedDatesEvent(pDebtSecurityOption, exerciseDateRange, lstRelevantUnderlyingDates);
        }
        #endregion Constructors

        #region Methods
        #region SetAdjustedDatesEvent
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        protected void SetAdjustedDatesEvent(IDebtSecurityOption pDebtSecurityOption, Pair<EFS_AdjustableDate, EFS_AdjustableDate> pExerciseDateRange, 
            List<EFS_AdjustableDate> pLstRelevantUnderlyingDates)
        {
            DateTime dtExerciseDate = DateTime.MinValue;
            DateTime dtRelevantDate = DateTime.MinValue;

            List<EFS_ExerciseDatesEvent> lstExerciseDatesEvents = new List<EFS_ExerciseDatesEvent>();
            EFS_ExerciseDatesEvent exerciseDatesEvent = null;
            int nbRelevantDates = pLstRelevantUnderlyingDates.Count;
            switch (exerciseStyle)
            {
                case ExerciseStyleEnum.Bermuda:

                    List<EFS_AdjustableDate> lstDates = ExerciseTools.GetBermudaExerciseDates(m_Cs, pDebtSecurityOption.BermudaExercise, m_DataDocument);
                    int nbExerciseDates = lstDates.Count;
                    int i = 0;
                    lstDates.ForEach(exerciseDate =>
                        {
                            dtExerciseDate = exerciseDate.AdjustedEventDate.DateValue;
                            dtRelevantDate = dtExerciseDate;

                            #region ExerciseDate
                            exerciseDatesEvent = new EFS_ExerciseDatesEvent
                            {
                                exerciseStyle = exerciseStyle,
                                startExerciseDate = new EFS_Date()
                                {
                                    DateValue = dtExerciseDate
                                },
                                endExerciseDate = new EFS_Date()
                                {
                                    DateValue = dtExerciseDate
                                }
                            };
                            #endregion ExerciseDate

                            #region RelevantUnderlyingDate
                            if (0 < nbRelevantDates)
                            {
                                if (nbExerciseDates == nbRelevantDates)
                                {
                                    // Il y a une date de relevant pour une date d'exercice
                                    dtRelevantDate = pLstRelevantUnderlyingDates[i].AdjustedEventDate.DateValue;
                                    exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date
                                    {
                                        DateValue = dtRelevantDate
                                    };
                                }
                                else
                                {
                                    // Il y a une date de relevant pour n dates d'exercice
                                    foreach (EFS_AdjustableDate relevant in pLstRelevantUnderlyingDates)
                                    {
                                        dtRelevantDate = relevant.AdjustedEventDate.DateValue;
                                        if (0 <= dtRelevantDate.CompareTo(dtExerciseDate))
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
                            exerciseDatesEvent.relevantUnderlyingDateSpecified = (null != exerciseDatesEvent.relevantUnderlyingDate);
                            lstExerciseDatesEvents.Add(exerciseDatesEvent);
                            #endregion RelevantUnderlyingDate
                            i++;
                        });
                        break;

                case ExerciseStyleEnum.American:
                case ExerciseStyleEnum.European:

                    #region ExerciseDate
                    exerciseDatesEvent = new EFS_ExerciseDatesEvent
                    {
                        exerciseStyle = exerciseStyle,
                        startExerciseDate = new EFS_Date()
                        { 
                            DateValue = pExerciseDateRange.First.AdjustedEventDate.DateValue
                        },
                        endExerciseDate = new EFS_Date()
                        {
                            DateValue = pExerciseDateRange.Second.AdjustedEventDate.DateValue
                        }
                    };
                    #endregion ExerciseDate

                    #region RelevantUnderlyingDate
                    if (1 == nbRelevantDates)
                    {
                        exerciseDatesEvent.relevantUnderlyingDate = new EFS_Date
                        {
                            DateValue = pLstRelevantUnderlyingDates[0].AdjustedEventDate.DateValue
                        };
                    }
                    exerciseDatesEvent.relevantUnderlyingDateSpecified = (null != exerciseDatesEvent.relevantUnderlyingDate);
                    #endregion RelevantUnderlyingDate

                    #region SettlementDate
                    if (pDebtSecurityOption.SettlementDateSpecified)
                    {
                        exerciseDatesEvent.cashSettlementPaymentDateSpecified = pDebtSecurityOption.SettlementDateSpecified;
                        EFS_AdjustableDate settlementDate = Tools.GetEFS_AdjustableDate(m_Cs, pDebtSecurityOption.SettlementDate, m_DataDocument);
                        exerciseDatesEvent.cashSettlementPaymentDate = new EFS_Date
                        {
                            DateValue = settlementDate.AdjustedEventDate.DateValue
                        };
                    }
                    #endregion SettlementDate

                    lstExerciseDatesEvents.Add(exerciseDatesEvent);

                    break;
            }
            exerciseDatesEventsSpecified = (0 < lstExerciseDatesEvents.Count);
            if (exerciseDatesEventsSpecified)
                exerciseDatesEvents = lstExerciseDatesEvents.ToArray();
        }
        #endregion SetAdjustedDatesEvent
        #endregion Methods
    }
    #endregion EFS_BondOptionExercise

    #region EFS_BondExerciseDates
    public class EFS_BondExerciseDates : EFS_OptionExerciseDates
    {
        #region Constructors
        public EFS_BondExerciseDates() { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondExerciseDates(string pCS, ExerciseStyleEnum pExerciseStyle, EFS_Underlyer pUnderlyer, SettlementTypeEnum pSettlementType,
            EFS_AdjustableDate pCommencementDate, EFS_AdjustableDate pExpiryDate,
            EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
            : this(pCS, pExerciseStyle, pUnderlyer, pSettlementType, pCommencementDate, pExpiryDate, null, pMarketScheduleTradingDay, pDataDocument) { }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondExerciseDates(string pCS, ExerciseStyleEnum pExerciseStyle, EFS_Underlyer pUnderlyer, SettlementTypeEnum pSettlementType,
            EFS_AdjustableDate pCommencementDate, EFS_AdjustableDate pExpiryDate, EFS_AdjustableDates pExerciseDates,
            EFS_MarketScheduleTradingDayBase pMarketScheduleTradingDay, DataDocumentContainer pDataDocument)
            : base(pCS, pExerciseStyle, pSettlementType, pCommencementDate, pExpiryDate, pMarketScheduleTradingDay, pDataDocument)
        {
            CalculExerciseValuationDates(pUnderlyer, pExerciseDates);
        }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion EFS_BondExerciseDates

    #region EFS_BondOptionNotional
    public class EFS_BondOptionNotional : EFS_OptionNotionalBase
    {
        #region Constructors
        public EFS_BondOptionNotional(IReference pPayerPartyReference, IReference pReceiverPartyReference, IMoney pNotionalAmount)
            :base(pPayerPartyReference, pReceiverPartyReference, pNotionalAmount)
        {
        }
        #endregion Constructors
    }
    #endregion EFS_BondOptionNotional

    #region EFS_BondOptionPremium
    public class EFS_BondOptionPremium : EFS_OptionPremiumBase
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondOptionPremium(string pCS, IDebtSecurityOption pDebtSecurityOption, DataDocumentContainer pDataDocument)
            : base(pCS, (IProductBase)pDebtSecurityOption, (INbOptionsAndNotionalBase)pDebtSecurityOption, (IPremiumBase)pDebtSecurityOption.Premium, pDataDocument)
        {
            expiryDate = pDebtSecurityOption.Efs_BondOption.expiryDate;
            IPremium premium = pDebtSecurityOption.Premium;
            if (premium.PaymentDate.AdjustableDateSpecified)
                settlementDate = new EFS_AdjustableDate(m_Cs, premium.PaymentDate.AdjustableDate, m_DataDocument);
            else if (premium.PaymentDate.RelativeDateSpecified)
            {
                IRelativeDateOffset relativeDate = premium.PaymentDate.RelativeDate;
                if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(m_Cs, relativeDate, out DateTime[] offsetDate, m_DataDocument))
                    settlementDate = new EFS_AdjustableDate(m_Cs, offsetDate[0], relativeDate.GetAdjustments, m_DataDocument);
            }
            SetPreSettlement();
        }
        #endregion Constructors
    }
    #endregion EFS_BondOptionPremium

    #region EFS_BondOptionStrike
    public class EFS_BondOptionStrike : EFS_OptionStrikeBase
    {
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_BondOptionStrike(string pCS, IDebtSecurityOption pDebtSecurityOption, DataDocumentContainer pDataDocument)
            : base(pCS, pDataDocument)
        {
            IBondOptionStrike bondOptionStrike = pDebtSecurityOption.Strike;
            if (bondOptionStrike.PriceSpecified)
            {
                strikePriceSpecified = bondOptionStrike.Price.PriceSpecified;
                strikePercentageSpecified = bondOptionStrike.Price.PercentageSpecified;
                if (strikePriceSpecified)
                {
                    strikePrice = bondOptionStrike.Price.Price;
                    strikeAmountSpecified = bondOptionStrike.Price.CurrencySpecified;
                    if (strikeAmountSpecified)
                        strikeAmount = ((IProduct)pDebtSecurityOption).ProductBase.CreateMoney(strikePrice.DecValue, 
                            bondOptionStrike.Price.Currency.Value);
                }
                else if (strikePercentageSpecified)
                {
                    strikePriceSpecified = spotPriceSpecified;
                    if (strikePriceSpecified)
                    {
                        #region Percentage of the spotPrice
                        strikePercentageSpecified = false;
                        strikePrice = new EFS_Decimal(bondOptionStrike.Price.Percentage.DecValue * spotPrice.DecValue); // ALERT spotPrice ??
                        #endregion Percentage of the spotPrice
                    }
                    else
                    {
                        strikePercentage = new EFS_Decimal(bondOptionStrike.Price.Percentage.DecValue);
                    }
                }
            }
            else if (bondOptionStrike.ReferenceSwapCurveSpecified)
            {
                // TO DO
            }
        }
        #endregion Constructors
    }
    #endregion EFS_BondStrike

    #region EFS_BondMarketScheduleTradingDay
    public class EFS_BondMarketScheduleTradingDay : EFS_MarketScheduleTradingDayBase
    {
        #region Constructors
        public EFS_BondMarketScheduleTradingDay(string pConnectionString, IDebtSecurityOption pDebtSecurityOption)
            :base (pConnectionString,(IProductBase)pDebtSecurityOption)
        {
            LstExchangeTraded = new List<IExchangeTraded>();
            if (pDebtSecurityOption.BondSpecified)
                LstExchangeTraded.Add((IExchangeTraded)pDebtSecurityOption.Bond);
            else if (pDebtSecurityOption.ConvertibleBondSpecified)
                LstExchangeTraded.Add((IExchangeTraded)pDebtSecurityOption.ConvertibleBond);

            Initialize();
        }
        #endregion Constructors
    }
    #endregion EFS_BondMarketScheduleTradingDay

    #region EFS_BondUnderlyer
    public class EFS_BondUnderlyer : EFS_Underlyer
    {
        #region Members
        private readonly Nullable<decimal> notionalAmount;
        private readonly Nullable<decimal> parAmount;
        #endregion Members
        #region Constructors
        public EFS_BondUnderlyer(string pCS, OptionTypeEnum pOptionType, IReference pBuyerPartyReference, IReference pSellerPartyReference,
            object pSecurityAsset, Nullable<decimal> pNotionalAmount, Nullable<decimal> pParAmount)
            : base(pCS, EventCodeFunc.Underlyer, EventCodeFunc.UnderlyerValuationDate)
        {
            notionalAmount = pNotionalAmount;
            parAmount = pParAmount;
            SetBuyerSellerPartyReference(pOptionType, pBuyerPartyReference, pSellerPartyReference);
            SetUnderlyingAsset(pCS, pSecurityAsset);
            SetOpenUnits(pSecurityAsset);
        }
        #endregion Constructors
        #region Methods
        #region SetOpenUnits
        protected override void SetOpenUnits<T>(T pSource)
        {
            unitValue = new EFS_Decimal(1);
            unitType = UnitTypeEnum.Qty;
            if (notionalAmount.HasValue && parAmount.HasValue)
                unitValue.DecValue = notionalAmount.Value / parAmount.Value;
            UpdateUnderlyingAsset(unitValue, unitType, unit);
        }
        #endregion SetOpenUnits
        #endregion Methods
    }
    #endregion EFS_BondUnderlyer

    #region EFS_BondUnderlyerValuationDates
    public class EFS_BondUnderlyerValuationDates : EFS_ValuationDates
    {
        #region Constructors
        public EFS_BondUnderlyerValuationDates(string pConnectionString, EFS_BondUnderlyer pBondUnderlyer, EFS_AdjustableDate pValuationDate)
            : base(pConnectionString, pBondUnderlyer, pValuationDate)
        {
        }
        #endregion Constructors
    }
    #endregion EFS_BondUnderlyerValuationDates

}

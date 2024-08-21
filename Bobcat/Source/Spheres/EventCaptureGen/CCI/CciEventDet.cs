#region Using Directives
using System;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Reflection;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web; 

using EFS.GUI;
using EFS.GUI.CCI; 
using EFS.GUI.Interface;



using EfsML;

using FpML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{
	#region CciEventDet
	public class CciEventDet : IContainerCciFactory, IContainerCci,IContainerCciQuoteBasis	
	{
		#region Enums
        #region CciEnum
        public enum CciEnum
		{
			// PaymentQuote
			paymentQuote_notionalReference,
			paymentQuote_pctRate,

			// PremiumQuote
			premiumQuote_notionalReference,
			premiumQuote_pctRate,
			premiumQuote_idCRef,
			premiumQuote_idCOne,
			premiumQuote_idCTwo,
			premiumQuote_quoteBasis,

			// CurrencyPair

			currencyPair_idCOne,
			currencyPair_idCTwo,
			currencyPair_quoteBasis,
			currencyPair_rate,

			// DayCountFraction
			dayCountFraction_name,
			dayCountFraction_num,
			dayCountFraction_den,
			dayCountFraction_totalOfYear,
			dayCountFraction_totalOfDay,
			dayCountFraction_rate,

			// ExchangeRate
			exchangeRate_idCOne,
			exchangeRate_idCTwo,
			exchangeRate_idCRef,
			exchangeRate_quoteBasis,		
			exchangeRate_rate,
			exchangeRate_spotRate,
			exchangeRate_forwardPoints,
			exchangeRate_notionalAmount,

			// ExchangeRatePremium
			exchangeRatePremium_idCOne,
			exchangeRatePremium_idCTwo,
			exchangeRatePremium_quoteBasis,		
			exchangeRatePremium_rate,
			exchangeRatePremium_notionalAmount,


			// StrikePrice
			strikePrice_quoteBasis,
			strikePrice_rate,

			// SideRate
			sideRate_idCOne,
			sideRate_idCTwo,
			sideRate_idCBase,
			sideRate_sideRatebasis,
			sideRate_rate,
			sideRate_spotRate,
			sideRate_forwardPoints,

			// SettlementRate
			settlementRate_fxType,
			settlementRate_idCRef,
			settlementRate_notionalAmount,
			settlementRate_rate,
			settlementRate_spotRate,
			settlementRate_strike,
			settlementRate_settlementRate,

			// FixingRate
			fixingRate_idCOne,
			fixingRate_idCTwo,
			fixingRate_quoteBasis,
			fixingRate_idBC,
			fixingRate_rate,
			fixingRate_dateFixing,
			fixingRate_timeFixing,

			// CapFloorSchedule
			capFloorSchedule_rate,
			capFloorSchedule_strike,

			// TriggerRate
			triggerRate_idCOne,
			triggerRate_idCTwo,
			triggerRate_quoteBasis,
			triggerRate_dateFixing,
			triggerRate_timeFixing,
			triggerRate_status,
			triggerRate_spotRate,
			triggerRate_rate,

			// Notes
            notes_dtAction_date,
            notes_dtAction_time,
			notes_note,
			notes_extlLink,

			// PricingFx
			pricingFx_idCOne,
			pricingFx_idCTwo,

			pricingFx_dcf,
			pricingFx_num,
			pricingFx_den,
			pricingFx_totalOfYear,
			pricingFx_totalOfDay,
			pricingFx_timeToExpiration,

			pricingFx_dcfTwo,
			pricingFx_numTwo,
			pricingFx_denTwo,
			pricingFx_totalOfYearTwo,
			pricingFx_totalOfDayTwo,
			pricingFx_timeToExpirationTwo,

			pricingFx_spotRate,
			pricingFx_forwardRate,
			pricingFx_interestRateOne,
			pricingFx_interestRateTwo,


			// PricingFxOption
			pricingFxOption_idCOne,
			pricingFxOption_idCTwo,
			pricingFxOption_strike,

			pricingFxOption_dcf,
			pricingFxOption_num,
			pricingFxOption_den,

			pricingFxOption_totalOfYear,
			pricingFxOption_totalOfDay,
			pricingFxOption_timeToExpiration,

			pricingFxOption_exchangeRate,
			pricingFxOption_interestRateOne,
			pricingFxOption_interestRateTwo,

			pricingFxOption_volatility,
			pricingFxOption_callPrice,
			pricingFxOption_callCharm,
			pricingFxOption_callDelta,
			pricingFxOption_callRhoOne,
			pricingFxOption_callRhoTwo,
			pricingFxOption_callTheta,

			pricingFxOption_putPrice,
			pricingFxOption_putCharm,
			pricingFxOption_putDelta,
			pricingFxOption_putRhoOne,
			pricingFxOption_putRhoTwo,
			pricingFxOption_putTheta,

			pricingFxOption_gamma,
			pricingFxOption_vega,
			pricingFxOption_color,
			pricingFxOption_speed,
			pricingFxOption_vanna,
			pricingFxOption_volga,

			pricingFxOption_screen,

            // Closing Fungible
            closing_contractMultiplier,
            closing_dailyQuantity,
            closing_price,
            closing_priceBase,
            closing_quoteTiming,
            closing_quotePrice,
            closing_quotePriceBase,
            closing_quotePriceYest,
            closing_quotePriceYestBase,
            closing_closingPrice,
            closing_closingPriceBase,
            closing_strikePrice,
            closing_factor,

			unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        public EventCustomCaptureInfos ccis;
        private EventDetails eventDetail;
        private readonly EventPricing eventPricing;
        private readonly string prefix;
        #endregion Members
        #region Accessors
        #region CS
        public string CS
		{
			get {return ccis.CS;}
		}
		#endregion CS
		
		#region EventDetail
		public EventDetails EventDetail
		{
			set {eventDetail = (EventDetails) value;}
			get {return eventDetail;}
        }
        #endregion EventDetail
        #region IsSpecified
        public bool IsSpecified
		{
			get
			{
				FieldInfo[] flds = eventDetail.GetType().GetFields();
				foreach (FieldInfo fld in flds)
				{
					if (fld.Name.EndsWith(Cst.FpML_SerializeKeySpecified))
					{
						if ((bool) fld.GetValue(eventDetail))
							return true;
					}
				}
				return false;
			}
		}

		#endregion IsSpecified
		#endregion Accessors
		#region Constructor
        public CciEventDet(CciEvent pCciEvent, EventDetails pEventDetails, EventPricing pEventPricing, string pPrefix)
		{
			prefix	        = pPrefix + CustomObject.KEY_SEPARATOR ;
			ccis            = pCciEvent.ccis;  	
			eventDetail     = pEventDetails;
			eventPricing    = pEventPricing;
		}
		#endregion Constructors

		#region Interface Methods
		#region IContainerCciFactory members
		#region AddCciSystem
		public void AddCciSystem()
		{
		}
		#endregion AddCciSystem
		#region Initialize_FromCci
		public void Initialize_FromCci()
		{
		}
		#endregion Initialize_FromCci
		#region Initialize_FromDocument
		public void Initialize_FromDocument()
		{ 
			
				string data;
				//string display;
				bool isSetting;
				SQL_Table sql_Table;

				Type tCciEnum = typeof(CciEnum);
				foreach (string enumName in Enum.GetNames(tCciEnum))
				{
					CustomCaptureInfo cci = ccis[prefix + enumName];
					if (cci != null)
					{
						#region Reset variables
						data      = string.Empty;
						//display   = string.Empty;
						isSetting = true;
						sql_Table = null;
						#endregion Reset variables
						//
						CciEnum keyEnum = (CciEnum ) System.Enum.Parse(typeof(CciEnum ), enumName);
						switch (keyEnum)
						{
							#region EventDet
							case CciEnum.settlementRate_fxType:
								#region FxType
								if (eventDetail.fxTypeSpecified)
									data = eventDetail.fxType;
								break;
								#endregion FxType
							case CciEnum.paymentQuote_notionalReference:
							case CciEnum.premiumQuote_notionalReference:
								#region NotionalReference
								if (eventDetail.notionalReferenceSpecified)
									data = eventDetail.notionalReference.Value;
								break;
								#endregion NotionalReference
							case CciEnum.premiumQuote_pctRate:
							case CciEnum.paymentQuote_pctRate:
								#region PctRate
								if (eventDetail.pctRateSpecified)
									data = eventDetail.pctRate.Value;
								break;
								#endregion PctRate
							case CciEnum.dayCountFraction_name:
								#region Value
								if (eventDetail.dcfSpecified)
									data = eventDetail.dcf;
								break;
								#endregion Value
							case CciEnum.dayCountFraction_num:
								#region Numerator
								if (eventDetail.dcfNumSpecified)
									data = eventDetail.dcfNum.Value;
								break;
								#endregion Numerator
							case CciEnum.dayCountFraction_den:
								#region Denominator
								if (eventDetail.dcfDenSpecified)
									data = eventDetail.dcfDen.Value;
								break;
								#endregion Denominator
							case CciEnum.dayCountFraction_totalOfYear:
								#region TotalOfYear
								if (eventDetail.totalOfYearSpecified)
									data = eventDetail.totalOfYear.Value;
								break;
								#endregion TotalOfYear
							case CciEnum.dayCountFraction_totalOfDay:
								#region TotalOfDay
								if (eventDetail.totalOfDaySpecified)
									data = eventDetail.totalOfDay.Value;
								break;
								#endregion TotalOfDay
							case CciEnum.sideRate_idCBase:
								#region IdCBase
								if (eventDetail.idCBaseSpecified)
									data = eventDetail.idCBase;
								break;
								#endregion IdCBase
							case CciEnum.sideRate_idCOne:
								#region IdC1
								if (eventDetail.idC1Specified)
									data = eventDetail.idC1;
								else if (eventDetail.idCBaseSpecified)
									data = eventDetail.idCBase;
								break;
								#endregion IdC1
							case CciEnum.premiumQuote_idCOne:
							case CciEnum.currencyPair_idCOne:
							case CciEnum.exchangeRate_idCOne:
							case CciEnum.fixingRate_idCOne:
							case CciEnum.exchangeRatePremium_idCOne:
							case CciEnum.triggerRate_idCOne:
								#region IdC1
								if (eventDetail.idC1Specified)
									data = eventDetail.idC1;
								break;
								#endregion IdC1
							case CciEnum.sideRate_idCTwo:
								#region IdC2
								if (eventDetail.idC2Specified)
									data = eventDetail.idC2;
								else if (eventDetail.idCBaseSpecified)
									data = eventDetail.idCBase;
								break;
								#endregion IdC2
							case CciEnum.premiumQuote_idCTwo:
							case CciEnum.currencyPair_idCTwo:
							case CciEnum.exchangeRate_idCTwo:
							case CciEnum.fixingRate_idCTwo:
							case CciEnum.exchangeRatePremium_idCTwo:
							case CciEnum.triggerRate_idCTwo:
								#region IdC2
								if (eventDetail.idC2Specified)
									data = eventDetail.idC2;
								break;
								#endregion IdC2
							case CciEnum.premiumQuote_idCRef:
							case CciEnum.settlementRate_idCRef:
								#region IdCRef
								if (eventDetail.idCRefSpecified)
									data = eventDetail.idCRef;
								break;
								#endregion IdCRef
							case CciEnum.sideRate_sideRatebasis:
								#region SideBasis
								if (eventDetail.sideRateBasisSpecified)
									data = eventDetail.sideRateBasis.ToString();
								break;
								#endregion SideBasis
							case CciEnum.strikePrice_quoteBasis:
								#region StrikeQuoteBasis
								if (eventDetail.strikeQuoteBasisSpecified)
									data = eventDetail.strikeQuoteBasis.ToString();
								break;
								#endregion StrikeQuoteBasis
							case CciEnum.premiumQuote_quoteBasis:
								#region PremiumQuoteBasis
								if (eventDetail.premiumQuoteBasisSpecified)
									data = eventDetail.premiumQuoteBasis.ToString();
								break;
								#endregion PremiumQuoteBasis
							case CciEnum.currencyPair_quoteBasis:
							case CciEnum.exchangeRate_quoteBasis:
							case CciEnum.fixingRate_quoteBasis:
							case CciEnum.exchangeRatePremium_quoteBasis:
							case CciEnum.triggerRate_quoteBasis:
								#region Basis
								if (eventDetail.quoteBasisSpecified)
									data = eventDetail.quoteBasis.ToString();
								break;
								#endregion Basis
							case CciEnum.dayCountFraction_rate:
							case CciEnum.currencyPair_rate:
							case CciEnum.exchangeRate_rate:
							case CciEnum.sideRate_rate:
							case CciEnum.settlementRate_rate:
							case CciEnum.fixingRate_rate:
							case CciEnum.capFloorSchedule_rate:
							case CciEnum.exchangeRatePremium_rate:
							case CciEnum.triggerRate_rate:
								#region Rate
								if (eventDetail.rateSpecified)
									data = eventDetail.rate.Value;
								break;
								#endregion Rate
							case CciEnum.exchangeRate_spotRate:
							case CciEnum.sideRate_spotRate:
							case CciEnum.settlementRate_spotRate:
							case CciEnum.triggerRate_spotRate:
								#region SpotRate
								if (eventDetail.spotRateSpecified)
									data = eventDetail.spotRate.Value;
								break;
								#endregion SpotRate
							case CciEnum.exchangeRate_forwardPoints:
							case CciEnum.sideRate_forwardPoints:
								#region ForwardPoints
								if (eventDetail.fwdPointsSpecified)
									data = eventDetail.fwdPoints.Value;
								break;
								#endregion ForwardPoints
							case CciEnum.capFloorSchedule_strike:
							case CciEnum.settlementRate_strike:
							case CciEnum.strikePrice_rate:
								#region Strike
								if (eventDetail.strikePriceSpecified)
									data = eventDetail.strikePrice.Value;
								break;
								#endregion Strike
							case CciEnum.exchangeRate_notionalAmount:
							case CciEnum.settlementRate_notionalAmount:
							case CciEnum.exchangeRatePremium_notionalAmount:
								#region NotionalAmount
								if (eventDetail.notionalAmountSpecified)
									data = eventDetail.notionalAmount.Value;
								break;
								#endregion NotionalAmount
							case CciEnum.settlementRate_settlementRate:
								#region SettlementRate
								if (eventDetail.settlementRateSpecified)
									data = eventDetail.settlementRate.Value;
								break;
								#endregion SettlementRate
							case CciEnum.fixingRate_dateFixing:
							case CciEnum.fixingRate_timeFixing:
							case CciEnum.triggerRate_dateFixing:
							case CciEnum.triggerRate_timeFixing:
								#region DateFixing
								if (eventDetail.dtFixingSpecified)
									data = eventDetail.dtFixing.Value;
								break;
								#endregion DateFixing
							case CciEnum.fixingRate_idBC:
								#region BusinessCenter
								if (eventDetail.idBCSpecified)
									data = eventDetail.idBC;
								break;
								#endregion BusinessCenter
							case CciEnum.triggerRate_status:
								#region Trigger status
								if (ccis.EventInput.CurrentEvent.idStTriggerSpecified)
                                    data = ccis.EventInput.CurrentEvent.idStTrigger;
								break;
								#endregion Trigger status
                            case CciEnum.notes_dtAction_date:
                            case CciEnum.notes_dtAction_time:
                                #region DtAction
                                if (eventDetail.dtActionSpecified)
                                    data = eventDetail.dtAction.Value;
                                break;
                                #endregion DtAction
                            case CciEnum.notes_note:
								#region Notes
								if (eventDetail.noteSpecified)
									data = eventDetail.note;
								break;
								#endregion Notes
							case CciEnum.notes_extlLink:
								#region ExtlLink
								if (eventDetail.extlLinkSpecified)
									data = eventDetail.extlLink;
								break;
								#endregion ExtlLink
                            case CciEnum.closing_contractMultiplier:
                                #region contractMultiplier
                                if (eventDetail.contractMultiplierSpecified)
                                    data = eventDetail.contractMultiplier.Value;
                                break;
                                #endregion contractMultiplier
                            case CciEnum.closing_dailyQuantity:
                                #region dailyQuantity
                                if (eventDetail.dailyQuantitySpecified)
                                    data = eventDetail.dailyQuantity.Value;
                                break;
                                #endregion dailyQuantity
                            case CciEnum.closing_price:
                                #region price
                                if (eventDetail.priceSpecified)
                                    data = eventDetail.price.Value;
                                break;
                                #endregion price
                            case CciEnum.closing_priceBase:
                                #region price100
                                if (eventDetail.price100Specified)
                                    data = eventDetail.price100.Value;
                                break;
                                #endregion price100
                            case CciEnum.closing_quotePrice:
                                #region quotePrice
                                if (eventDetail.quotePriceSpecified)
                                    data = eventDetail.quotePrice.Value;
                                break;
                                #endregion quotePrice
                            case CciEnum.closing_quotePriceBase:
                                #region quotePrice100
                                if (eventDetail.quotePrice100Specified)
                                    data = eventDetail.quotePrice100.Value;
                                break;
                                #endregion quotePrice100
                            case CciEnum.closing_quotePriceYest:
                                #region quotePriceYest
                                if (eventDetail.quotePriceYestSpecified)
                                    data = eventDetail.quotePriceYest.Value;
                                break;
                                #endregion quotePriceYest
                            case CciEnum.closing_quotePriceYestBase:
                                #region quotePriceYest100
                                if (eventDetail.quotePriceYest100Specified)
                                    data = eventDetail.quotePriceYest100.Value;
                                break;
                                #endregion quotePriceYest100
                            case CciEnum.closing_quoteTiming:
                                #region quoteTiming
                                if (eventDetail.quoteTimingSpecified)
                                    data = eventDetail.quoteTiming;
                                break;
                                #endregion quoteTiming
                            #endregion EventDet
                            #region EventPricing
                            case CciEnum.pricingFx_dcf:
							case CciEnum.pricingFxOption_dcf:
								#region Dcf
								if (eventPricing.dcfSpecified)
									data = eventPricing.dcf;
								break;
								#endregion Dcf
							case CciEnum.pricingFx_dcfTwo:
								#region Dcf2
								if (eventPricing.dcf2Specified)
									data = eventPricing.dcf2;
								break;
								#endregion Dcf2
							case CciEnum.pricingFx_num:
							case CciEnum.pricingFxOption_num:
								#region Numerator
								if (eventPricing.dcfNumSpecified)
									data = eventPricing.dcfNum.Value;
								break;
								#endregion Numerator
							case CciEnum.pricingFx_numTwo:
								#region Numerator2
								if (eventPricing.dcfNum2Specified)
									data = eventPricing.dcfNum2.Value;
								break;
								#endregion Numerator2
							case CciEnum.pricingFx_den:
							case CciEnum.pricingFxOption_den:
								#region Denominator
								if (eventPricing.dcfDenSpecified)
									data = eventPricing.dcfDen.Value;
								break;
								#endregion Denominator
							case CciEnum.pricingFx_denTwo:
								#region Denominator2
								if (eventPricing.dcfDen2Specified)
									data = eventPricing.dcfDen2.Value;
								break;
								#endregion Denominator2
							case CciEnum.pricingFx_totalOfYear:
							case CciEnum.pricingFxOption_totalOfYear:
								#region TotalOfYear
								if (eventPricing.totalOfYearSpecified)
									data = eventPricing.totalOfYear.Value;
								break;
								#endregion TotalOfYear
							case CciEnum.pricingFx_totalOfYearTwo:
								#region TotalOfYear2
								if (eventPricing.totalOfYear2Specified)
									data = eventPricing.totalOfYear2.Value;
								break;
								#endregion TotalOfYear2
							case CciEnum.pricingFx_totalOfDay:
							case CciEnum.pricingFxOption_totalOfDay:
								#region TotalOfDay
								if (eventPricing.totalOfDaySpecified)
									data = eventPricing.totalOfDay.Value;
								break;
								#endregion TotalOfDay
							case CciEnum.pricingFx_totalOfDayTwo:
								#region TotalOfDay2
								if (eventPricing.totalOfDay2Specified)
									data = eventPricing.totalOfDay2.Value;
								break;
								#endregion TotalOfDay2
							case CciEnum.pricingFx_timeToExpiration:
							case CciEnum.pricingFxOption_timeToExpiration:
								#region TimeToExpiration
								if (eventPricing.timeToExpirationSpecified)
									data = eventPricing.timeToExpiration.Value;
								break;
								#endregion TimeToExpiration
							case CciEnum.pricingFx_timeToExpirationTwo:
								#region TimeToExpiration2
								if (eventPricing.timeToExpiration2Specified)
									data = eventPricing.timeToExpiration2.Value;
								break;
								#endregion TimeToExpiration2
							case CciEnum.pricingFx_idCOne:
							case CciEnum.pricingFxOption_idCOne:
								#region IdC1
								if (eventPricing.idC1Specified)
									data = eventPricing.idC1;
								break;
								#endregion IdC1
							case CciEnum.pricingFx_idCTwo:
							case CciEnum.pricingFxOption_idCTwo:
								#region IdC2
								if (eventPricing.idC2Specified)
									data = eventPricing.idC2;
								break;
								#endregion IdC2
							case CciEnum.pricingFxOption_strike:
								#region Strike
								if (eventPricing.strikeSpecified)
									data = eventPricing.strike.Value;
								break;
								#endregion Strike
							case CciEnum.pricingFx_forwardRate:
							case CciEnum.pricingFxOption_exchangeRate:
								#region ExchangeRate
								if (eventPricing.exchangeRateSpecified)
									data = eventPricing.exchangeRate.Value;
								break;
								#endregion ExchangeRate
							case CciEnum.pricingFx_interestRateOne:
							case CciEnum.pricingFxOption_interestRateOne:
								#region InterestRate1
								if (eventPricing.interestRate1Specified)
									data = eventPricing.interestRate1.Value;
								break;
								#endregion InterestRate1
							case CciEnum.pricingFx_interestRateTwo:
							case CciEnum.pricingFxOption_interestRateTwo:
								#region InterestRate2
								if (eventPricing.interestRate2Specified)
									data = eventPricing.interestRate2.Value;
								break;
								#endregion InterestRate2
							case CciEnum.pricingFx_spotRate:
								#region SpotRate
								if (eventPricing.spotRateSpecified)
									data = eventPricing.spotRate.Value;
								break;
								#endregion SpotRate
							case CciEnum.pricingFxOption_volatility:
								#region Volatility
								if (eventPricing.volatilitySpecified)
									data = eventPricing.volatility.Value;
								break;
								#endregion Volatility
							case CciEnum.pricingFxOption_callPrice:
								#region CallPrice
								if (eventPricing.callPriceSpecified)
									data = eventPricing.callPrice.Value;
								break;
								#endregion CallPrice
							case CciEnum.pricingFxOption_callCharm:
								#region CallCharm
								if (eventPricing.callCharmSpecified)
									data = eventPricing.callCharm.Value;
								break;
								#endregion CallCharm
							case CciEnum.pricingFxOption_callDelta:
								#region CallDelta
								if (eventPricing.callDeltaSpecified)
									data = eventPricing.callDelta.Value;
								break;
								#endregion CallDelta
							case CciEnum.pricingFxOption_callRhoOne:
								#region CallRho1
								if (eventPricing.callRho1Specified)
									data = eventPricing.callRho1.Value;
								break;
								#endregion CallRho1
							case CciEnum.pricingFxOption_callRhoTwo:
								#region CallRho2
								if (eventPricing.callRho2Specified)
									data = eventPricing.callRho2.Value;
								break;
								#endregion CallRho2
							case CciEnum.pricingFxOption_callTheta:
								#region CallTheta
								if (eventPricing.callThetaSpecified)
									data = eventPricing.callTheta.Value;
								break;
								#endregion CallTheta
							case CciEnum.pricingFxOption_putPrice:
								#region PutPrice
								if (eventPricing.putPriceSpecified)
									data = eventPricing.putPrice.Value;
								break;
								#endregion PutPrice
							case CciEnum.pricingFxOption_putCharm:
								#region PutCharm
								if (eventPricing.putCharmSpecified)
									data = eventPricing.putCharm.Value;
								break;
								#endregion PutCharm
							case CciEnum.pricingFxOption_putDelta:
								#region PutDelta
								if (eventPricing.putDeltaSpecified)
									data = eventPricing.putDelta.Value;
								break;
								#endregion PutDelta
							case CciEnum.pricingFxOption_putRhoOne:
								#region PutRho1
								if (eventPricing.putRho1Specified)
									data = eventPricing.putRho1.Value;
								break;
								#endregion PutRho1
							case CciEnum.pricingFxOption_putRhoTwo:
								#region PutRho2
								if (eventPricing.putRho2Specified)
									data = eventPricing.putRho2.Value;
								break;
								#endregion PutRho2
							case CciEnum.pricingFxOption_putTheta:
								#region PutTheta
								if (eventPricing.putThetaSpecified)
									data = eventPricing.putTheta.Value;
								break;
								#endregion PutTheta

							case CciEnum.pricingFxOption_gamma:
								#region Gamma
								if (eventPricing.gammaSpecified)
									data = eventPricing.gamma.Value;
								break;
								#endregion Gamma
							case CciEnum.pricingFxOption_vega:
								#region Vega
								if (eventPricing.vegaSpecified)
									data = eventPricing.vega.Value;
								break;
								#endregion Vega
							case CciEnum.pricingFxOption_color:
								#region Color
								if (eventPricing.colorSpecified)
									data = eventPricing.color.Value;
								break;
								#endregion Color
							case CciEnum.pricingFxOption_speed:
								#region Speed
								if (eventPricing.speedSpecified)
									data = eventPricing.speed.Value;
								break;
								#endregion Speed
							case CciEnum.pricingFxOption_vanna:
								#region Vanna
								if (eventPricing.vannaSpecified)
									data = eventPricing.vanna.Value;
								break;
								#endregion Vanna
							case CciEnum.pricingFxOption_volga:
								#region Volga
								if (eventPricing.volgaSpecified)
									data = eventPricing.volga.Value;
								break;
								#endregion Volga
								#endregion EventPricing
							default:
								isSetting = false;
								break;
						}
						if (isSetting)
							ccis.InitializeCci(cci,sql_Table,data);
					}
				}
			
		}
		#endregion Initialize_FromDocument
		#region Dump_ToDocument
		public void Dump_ToDocument()
		{ 
			bool isSetting;
			string data;
			CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            
			Type tCciEnum = typeof(CciEnum);
			foreach (string enumName in Enum.GetNames(tCciEnum))
			{
				CustomCaptureInfo cci = ccis[prefix + enumName];
				if ((cci != null) && (cci.HasChanged))
				{
					#region Reset variables
					data                            = cci.NewValue;
					isSetting                       = true;
					processQueue                    = CustomCaptureInfosBase.ProcessQueueEnum.None;
					#endregion
					//
					CciEnum keyEnum = (CciEnum ) System.Enum.Parse(typeof(CciEnum ), enumName);
					switch (keyEnum)
					{
                        case CciEnum.notes_note:
                            #region Note
                            eventDetail.noteSpecified = StrFunc.IsFilled(data);
							eventDetail.note          = data;
                            #endregion Note
                            break;
						case CciEnum.notes_extlLink:
                            #region ExtlLink
                            eventDetail.extlLinkSpecified = StrFunc.IsFilled(data);
							eventDetail.extlLink          = data;
                            #endregion ExtlLink
                            break;
                        default:
							isSetting = false;
							break;
					}
					if (isSetting)
						ccis.Finalize(cci.ClientId_WithoutPrefix , processQueue );
				}
			}
		}
		#endregion Dump_ToDocument
		#region IsClientId_PayerOrReceiver
		public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
		{
			return false;
		}
		#endregion IsClientId_PayerOrReceiver
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region ProcessInitialize
		public void ProcessInitialize(CustomCaptureInfo pCci)
		{
			
		}
		#endregion ProcessInitialize
		#region GetDisplay
		public void  SetDisplay(CustomCaptureInfo pCci)
		{
		}
		#endregion
		#region CleanUp
		public void CleanUp()
		{
		}
		#endregion CleanUp
		#region RefreshCciEnabled
		public void RefreshCciEnabled()
		{
			
		}
		#region RemoveLastItemInArray
		public void RemoveLastItemInArray(string _)
		{
		}
		#endregion RemoveLastItemInArray
		#endregion IContainerCciFactory Members
		#region IContainerCci Members
		#region CciClientId
		public string  CciClientId(CciEnum  pEnumValue) 
		{
			return CciClientId(pEnumValue.ToString()); 
		}
		public string CciClientId(string pClientId_Key)
		{
			return prefix + pClientId_Key  ;
		}
		#endregion 
		#region Cci
		public CustomCaptureInfo Cci(CciEnum pEnum  )
		{
			return Cci(pEnum.ToString()); 
		}
		public CustomCaptureInfo Cci(string pClientId_Key)
		{
			return ccis[CciClientId(pClientId_Key)];
		}

		#endregion 
		#region IsCciOfContainer
		public bool IsCciOfContainer(string pClientId_WithoutPrefix)
		{
            return (pClientId_WithoutPrefix.StartsWith(prefix));
		}
		#endregion 
		#region CciContainerKey
		public string CciContainerKey(string pClientId_WithoutPrefix)
		{
			return pClientId_WithoutPrefix.Substring(prefix.Length);
		}
		#endregion 
		#region IsCci
		public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci )
		{
			return   (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix)  ; 
		}
		#endregion
		#endregion IContainerCci Members
		#region IContainerCciQuoteBasis members
		#region public IsClientId_QuoteBasis
		public  bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
		{
			bool isOk = IsCci(CciEnum.exchangeRate_quoteBasis,pCci);
			if (false == isOk)
				isOk = IsCci(CciEnum.fixingRate_quoteBasis,pCci);
			if (false == isOk)
				isOk = IsCci(CciEnum.sideRate_sideRatebasis,pCci);
			return isOk;
		}
		#endregion
		#region public GetCurrency1
		public string GetCurrency1(CustomCaptureInfo pCci)
		{
			string ret = string.Empty;
			if (IsCci(CciEnum.exchangeRate_quoteBasis,pCci))
				ret = Cci(CciEnum.exchangeRate_idCOne).NewValue;  
			else if (IsCci(CciEnum.fixingRate_quoteBasis,pCci))
				ret = Cci(CciEnum.fixingRate_idCOne).NewValue;  
			return ret ;
		}
		#endregion
		#region public GetCurrency2
		public  string GetCurrency2(CustomCaptureInfo pCci)
		{
			string ret = string.Empty;
			if (IsCci(CciEnum.exchangeRate_quoteBasis,pCci))
				ret = Cci(CciEnum.exchangeRate_idCTwo).NewValue;  
			else if (IsCci(CciEnum.fixingRate_quoteBasis,pCci))
				ret = Cci(CciEnum.fixingRate_idCTwo).NewValue;  
			return ret ;
		}
		#region public GetBaseCurrency
		public  string GetBaseCurrency(CustomCaptureInfo pCci)
		{
			string ret = string.Empty;
			if (IsCci(CciEnum.sideRate_sideRatebasis,pCci))
				ret = Cci(CciEnum.sideRate_idCBase).NewValue;  
			return ret ;
		}
		#endregion GetBaseCurrency
		#endregion
		#endregion IContainerCciQuoteBasis members
		#region Initialize_Document
		public void Initialize_Document()
		{
		}
		#endregion Initialize_Document
		#endregion Interface Methods
		#endregion
	}
	#endregion CciEventDet
}

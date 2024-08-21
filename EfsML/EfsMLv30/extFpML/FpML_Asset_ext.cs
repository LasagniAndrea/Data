#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.AssetDef;
using EfsML.v30.Security.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Data;
using System.Reflection;




#endregion using directives


namespace FpML.v44.Assetdef
{
    #region ActualPrice

    /// <summary>
    /// 
    /// </summary>
    /// EG 20140702 Upd Interface
    /// FI 20150129 [20748] Modify
    /// EG 20150306 [POC-BERKELEY] : Add accessors (Amount|UnitType|Unit) 
    public partial class ActualPrice : IActualPrice
    {
        #region Amount
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Decimal Amount
        {
            get { return amount; }

        }
        #endregion Amount
        #region UnitType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string UnitType
        {
            get
            {
                UnitTypeEnum unitType = UnitTypeEnum.None;
                if (priceExpression == PriceExpressionEnum.AbsoluteTerms)
                    unitType = UnitTypeEnum.Currency;
                else if (priceExpression == PriceExpressionEnum.PercentageOfNotional)
                    unitType = UnitTypeEnum.Percentage;
                return unitType.ToString();
            }

        }
        #endregion UnitType
        #region Unit
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Unit
        {
            get
            {
                string unit = string.Empty;
                if (currencySpecified && (priceExpression == PriceExpressionEnum.AbsoluteTerms))
                    unit = currency.Value;
                return unit;
            }
        }
        #endregion Unit

        #region IActualPrice Members
        EFS_Decimal IActualPrice.Amount
        {
            set { this.amount = value; }
            get { return this.amount; }
        }
        bool IActualPrice.CurrencySpecified
        {
            set { this.currencySpecified = value; }
            get { return this.currencySpecified; }
        }

        ICurrency IActualPrice.Currency
        {
            set { this.currency = (Currency)value; }
            get { return this.currency; }
        }
        PriceExpressionEnum IActualPrice.PriceExpression
        {
            set { this.priceExpression = value; }
            get { return this.priceExpression; }
        }
        #endregion IActualPrice Members
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150129 [20748] Add Constructor
        public ActualPrice()
        {
            this.amount = new EFS_Decimal();
            this.currency = new Currency();

        }
    }
	#endregion ActualPrice
	#region AssetPool
	public partial class AssetPool : IAssetPool
	{
		#region IAssetPool Members
		bool IAssetPool.VersionSpecified
		{
			set { this.versionSpecified = value; }
			get { return this.versionSpecified; }
		}
		EFS_NonNegativeInteger IAssetPool.Version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IAssetPool.EffectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IAssetPool.EffectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		EFS_Decimal IAssetPool.InitialFactor
		{
			set { this.initialFactor = value; }
			get { return this.initialFactor; }
		}
		bool IAssetPool.CurrentFactorSpecified
		{
			set { this.currentFactorSpecified = value; }
			get { return this.currentFactorSpecified; }
		}
		EFS_Decimal IAssetPool.CurrentFactor
		{
			set { this.currentFactor = value; }
			get { return this.currentFactor; }
		}
		#endregion IAssetPool Members
	}
	#endregion AssetPool
	#region AssetReference
	public partial class AssetReference : IReference
	{
		#region IReference Members
		string IReference.HRef
		{
			get {return this.href;}
			set{this.href=value;}
		}
		#endregion IReference Members
	}
	#endregion AssetReference

	#region Basket
    // EG 20140702 Upd Interface
    // EG 20140702 Add _sql_Asset
    /// EG 20150302 Add notionalBase|notionalBaseSpecified (CFD Forex)
    /// EG 20170510 [23153] Add SetAsset 
    public partial class Basket : IBasket, IReturnLegMainUnderlyer
	{
        // Variable de travail
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_AssetBase _sql_Asset;

		#region IOpenUnits Members
		bool IOpenUnits.OpenUnitsSpecified
		{
			set { this.openUnitsSpecified = value; }
			get { return this.openUnitsSpecified; }
		}
		EFS_Decimal IOpenUnits.OpenUnits
		{
			set { this.openUnits = value; }
			get { return this.openUnits; }
		}
		#endregion IOpenUnits Members
		#region IBasket Members
        bool IBasket.BasketCurrencySpecified
        {
            set { this.basketCurrencySpecified = value; }
            get { return this.basketCurrencySpecified; }
        }
        ICurrency IBasket.BasketCurrency
        {
            set { this.basketCurrency = (Currency)value; }
            get { return this.basketCurrency; }
        }
		IBasketConstituent[] IBasket.BasketConstituent { get { return this.basketConstituent; } }
		#endregion IBasket Members

        #region IReturnLegMainUnderlyer Membres
        bool IReturnLegMainUnderlyer.ExchangeIdSpecified
        {
            set { ; }
            get { return false; }
        }
        ISpheresIdScheme IReturnLegMainUnderlyer.ExchangeId
        {
            set { ; }
            get { return null; }
        }
        bool IReturnLegMainUnderlyer.InstrumentIdSpecified
        {
            get { return false; }
        }
        IScheme[] IReturnLegMainUnderlyer.InstrumentId
        {
            set { ; }
            get { return null; }
        }

        bool IReturnLegMainUnderlyer.CurrencySpecified
        {
            set { this.basketCurrencySpecified = value; }
            get { return this.basketCurrencySpecified; }
        }
        ICurrency IReturnLegMainUnderlyer.Currency
        {
            set { this.basketCurrency = (Currency)value; }
            get { return this.basketCurrency; }
        }
        bool IReturnLegMainUnderlyer.OpenUnitsSpecified
        {
            set { this.openUnitsSpecified = value; }
            get { return this.openUnitsSpecified; }
        }

        EFS_Decimal IReturnLegMainUnderlyer.OpenUnits
        {
            set { this.openUnits = value; }
            get { return this.openUnits; }
        }
        bool IReturnLegMainUnderlyer.SqlAssetSpecified
        {
            get { return (null != this._sql_Asset); }
        }
        SQL_AssetBase IReturnLegMainUnderlyer.SqlAsset
        {
            set { this._sql_Asset = value; }
            get { return this._sql_Asset; }
        }
        Nullable<int> IReturnLegMainUnderlyer.OTCmlId
        {
            set { ; }
            get { return null; }
        }
        Nullable<Cst.UnderlyingAsset> IReturnLegMainUnderlyer.UnderlyerAsset
        {
            get { return null; }
        }
        bool IReturnLegMainUnderlyer.NotionalBaseSpecified
        {
            get { return false; }
        }

        IMoney IReturnLegMainUnderlyer.NotionalBase
        {
            get { return null; }
        }
        // EG 20170510 [23153]
        void IReturnLegMainUnderlyer.SetAsset(string pCS, IDbTransaction pDbTransaction)
        {
        }
        #endregion

	}
	#endregion Basket
	#region BasketConstituent
	public partial class BasketConstituent : IEFS_Array, IBasketConstituent
	{
		#region Accessors
		#region id
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
		#endregion id
		#endregion Accessors

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent,
			ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IBasketConstituent Members
		bool IBasketConstituent.ConstituentWeightSpecified { get { return this.constituentWeightSpecified; } }
		bool IBasketConstituent.ConstituentWeightBasketAmountSpecified { get { return this.constituentWeight.constituentWeightBasketAmountSpecified; } }
		decimal IBasketConstituent.ConstituentWeightBasketAmountValue { get { return this.constituentWeight.constituentWeightBasketAmount.amount.DecValue; } }
		string IBasketConstituent.ConstituentWeightBasketAmountCurrency { get { return this.constituentWeight.constituentWeightBasketAmount.currency.Value; } }
		bool IBasketConstituent.ConstituentWeightBasketPercentageSpecified { get { return this.constituentWeight.constituentWeightBasketPercentageSpecified; } }
		decimal IBasketConstituent.ConstituentWeightBasketPercentage { get { return this.constituentWeight.constituentWeightBasketPercentage.DecValue; } }
		bool IBasketConstituent.ConstituentWeightOpenUnitsSpecified { get { return this.constituentWeight.constituentWeightOpenUnitsSpecified; } }
		decimal IBasketConstituent.ConstituentWeightOpenUnits { get { return this.constituentWeight.constituentWeightOpenUnits.DecValue; } }
		IUnderlyingAsset IBasketConstituent.UnderlyingAsset { get { return this.underlyingAsset; } }
		#endregion IBasketConstituent Members

	}
	#endregion BasketConstituent
	#region Bond
	public partial class Bond : IBond, IEFS_Array
	{
		#region Constructors
		public Bond()
		{
			issuerName = new EFS_String();
			issuerPartyReference = new PartyReference();
			maturity = new EFS_Date();
		}
		#endregion Constructors

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IBond Members
		bool IBond.IssuerNameSpecified
		{
			set { this.issuerNameSpecified = value; }
			get { return this.issuerNameSpecified; }
		}
		EFS_String IBond.IssuerName
		{
			set { this.issuerName = value; }
			get { return this.issuerName; }
		}
		bool IBond.IssuerPartyReferenceSpecified
		{
			set { this.issuerPartyReferenceSpecified = value; }
			get { return this.issuerPartyReferenceSpecified; }
		}
		IReference IBond.IssuerPartyReference
		{
			set { this.issuerPartyReference = (PartyReference)value; }
			get { return this.issuerPartyReference; }
		}
		bool IBond.SenioritySpecified
		{
			set { this.senioritySpecified = value; }
			get { return this.senioritySpecified; }
		}

		IScheme IBond.Seniority
		{
			set { this.seniority = (CreditSeniority)value; }
			get { return this.seniority; }
		}
		bool IBond.CouponTypeSpecified
		{
			set { this.couponTypeSpecified = value; }
			get { return this.couponTypeSpecified; }
		}
		IScheme IBond.CouponType
		{
			set { this.couponType = (CouponType)value; }
			get { return this.couponType; }
		}
		bool IBond.CouponRateSpecified
		{
			set { this.couponRateSpecified = value; }
			get { return this.couponRateSpecified; }
		}
		EFS_Decimal IBond.CouponRate
		{
			set { this.couponRate = value; }
			get { return this.couponRate; }
		}
		bool IBond.MaturitySpecified
		{
			set { this.maturitySpecified = value; }
			get { return this.maturitySpecified; }
		}
		EFS_Date IBond.Maturity
		{
			set { this.maturity = value; }
			get { return this.maturity; }
		}
		bool IBond.ParValueSpecified
		{
			set { this.parValueSpecified = value; }
			get { return this.parValueSpecified; }
		}
		EFS_Decimal IBond.ParValue
		{
			set { this.parValue = value; }
			get { return this.parValue; }
		}
		bool IBond.FaceAmountSpecified
		{
			set { this.faceAmountSpecified = value; }
			get { return this.faceAmountSpecified; }
		}
		EFS_Decimal IBond.FaceAmount
		{
			set { this.faceAmount = value; }
			get { return this.faceAmount; }
		}
		bool IBond.PaymentFrequencySpecified
		{
			set { this.paymentFrequencySpecified = value; }
			get { return this.paymentFrequencySpecified; }
		}
		IInterval IBond.PaymentFrequency
		{
			set { this.paymentFrequency = (Interval)value; }
			get { return this.paymentFrequency; }
		}
		bool IBond.DayCountFractionSpecified
		{
			set { this.dayCountFractionSpecified = value; }
			get { return this.dayCountFractionSpecified; }
		}
		DayCountFractionEnum IBond.DayCountFraction
		{
			set { this.dayCountFraction = value; }
			get { return this.dayCountFraction; }
		}
		string IBond.OtcmlId
		{
			set { this.otcmlId = value; }
			get { return this.otcmlId; }
		}
        // EG 20160404 Migration vs2013
        //int IBond.OTCmlId
        //{
        //    set { this.OTCmlId = value; }
        //    get { return this.OTCmlId; }
        //}
		#endregion IBond Members
	}
	#endregion Bond

	#region Commission
	public partial class Commission : ICommission
	{
		#region ICommission Members
		EFS_Decimal ICommission.Amount 
		{ 
			get { return this.commissionAmount; } 
		}
		CommissionDenominationEnum ICommission.CommissionDenomination 
		{ 
			set { this.commissionDenomination = value; } 
			get { return this.commissionDenomination; } 
		}
		bool ICommission.CurrencySpecified
		{
			get {return this.currencySpecified; }
		}
		string ICommission.Currency 
		{ 
			set { this.currency.Value = value; } 
			get { return this.currency.Value; } 
		}
		IMoney ICommission.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
		#endregion ICommission Members
	}
	#endregion Commission
	#region ConvertibleBond
	public partial class ConvertibleBond : IConvertibleBond, IEFS_Array
	{
		#region Constructor
		public ConvertibleBond()
		{
			redemptionDate = new EFS_Date();
		}
		#endregion Constructor

		#region IEFS_Array Members
		#region DisplayArray
        // EG 20160404 Migration vs2013
        //public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        //{
        //    return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        //}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IConvertibleBond Membres
		IUnderlyingAsset IConvertibleBond.UnderlyingEquity
		{
			set { this.underlyingEquity = (UnderlyingAsset)value; }
			get { return this.underlyingEquity; }
		}
		bool IConvertibleBond.RedemptionDateSpecified
		{
			set { this.redemptionDateSpecified = value; }
			get { return this.redemptionDateSpecified; }
		}
		EFS_Date IConvertibleBond.RedemptionDate
		{
			set { this.redemptionDate = value; }
			get { return this.redemptionDate; }
		}
		#endregion IConvertibleBond Membres
	}
	#endregion ConvertibleBond
	#region CouponType
	public partial class CouponType : ICloneable,IScheme
	{
		#region Constructors
		public CouponType()
		{
			couponTypeScheme = "http://www.fpml.org/coding-scheme/coupon-type-1-0";
		}
		#endregion Constructors
		#region ICloneable Members
		#region Clone
		public object Clone()
		{
            CouponType clone = new CouponType
            {
                couponTypeScheme = this.couponTypeScheme,
                Value = this.Value
            };
            return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.Scheme
		{
			set { this.couponTypeScheme = value; }
			get { return this.couponTypeScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion CouponType
	
	#region DividendPayout
	public partial class DividendPayout : IDividendPayout
	{
		#region Constructors
		public DividendPayout()
		{
			dividendPayoutRatio = new EFS_Decimal();
			dividendPayoutConditions = new EFS_MultiLineString();
		}
		#endregion Constructors

		#region IDividendPayout Members
		bool IDividendPayout.DividendPayoutRatioSpecified
		{
			set { this.dividendPayoutRatioSpecified = value; }
			get { return this.dividendPayoutRatioSpecified; }
		}
		EFS_Decimal IDividendPayout.DividendPayoutRatio
		{
			get { return this.dividendPayoutRatio; }
		}
		bool IDividendPayout.DividendPayoutConditionsSpecified
		{
			get { return this.dividendPayoutConditionsSpecified; }
		}
		#endregion IDividendPayout Members
	}
	#endregion DividendPayout

	#region EquityAsset
	public partial class EquityAsset : IEquityAsset,IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
	}
	#endregion EquityAsset
	#region ExchangeTraded
	public abstract partial class ExchangeTraded : IExchangeTraded
	{
		#region IExchangeTraded Members
		bool IExchangeTraded.RelatedExchangeIdSpecified
		{
			set { this.relatedExchangeIdSpecified = value; }
			get { return this.relatedExchangeIdSpecified; }
		}
		IScheme[] IExchangeTraded.RelatedExchangeId
		{
			set { this.relatedExchangeId = (ExchangeId[]) value; }
			get { return this.relatedExchangeId; }
		}
		bool IExchangeTraded.OptionsExchangeIdSpecified
		{
			set { this.optionsExchangeIdSpecified = value; }
			get { return this.optionsExchangeIdSpecified; }
		}

		IScheme[] IExchangeTraded.OptionsExchangeId
		{
			set { this.optionsExchangeId = (ExchangeId[])value; }
			get { return this.optionsExchangeId; }
		}
		IScheme IExchangeTraded.FirstOptionsExchangeId
		{
			set 
			{ 
				if (0 < this.optionsExchangeId.Length)
					this.optionsExchangeId[0] = (ExchangeId)value; 
			}
			get 
			{
				if (0 < this.optionsExchangeId.Length)
					return this.optionsExchangeId[0];
				return null;
			}
		}
		#endregion IExchangeTraded Members
	}
	#endregion ExchangeTraded
    #region ExchangeTradedCalculatedPrice
    // EG 20140702 New build FpML4.4
    public abstract partial class ExchangeTradedCalculatedPrice : IExchangeTradedCalculatedPrice
    {
        #region IExchangeTradedCalculatedPrice Members
        bool IExchangeTradedCalculatedPrice.ConstituentExchangeIdSpecified
        {
            set { this.constituentExchangeIdSpecified = value; }
            get { return this.constituentExchangeIdSpecified; }
        }
        IScheme[] IExchangeTradedCalculatedPrice.ConstituentExchangeId
        {
            set { this.constituentExchangeId = (ExchangeId[])value; }
            get { return this.constituentExchangeId; }
        }
        IScheme IExchangeTradedCalculatedPrice.FirstConstituentExchangeId
        {
            set
            {
                if (0 < this.constituentExchangeId.Length)
                    this.constituentExchangeId[0] = (ExchangeId)value;
            }
            get
            {
                if (0 < this.constituentExchangeId.Length)
                    return this.constituentExchangeId[0];
                return null;
            }
        }
        #endregion IExchangeTradedCalculatedPrice Members
    }
    #endregion ExchangeTradedCalculatedPrice

    #region Future
    public partial class Future : IFuture
	{
		#region Constructors
		public Future()
		{
			multiplier				= new EFS_Integer();
			futureContractReference	= new EFS_String();
			maturity				= new EFS_Date();
		}
		#endregion Constructors
		#region IFuture Members
		bool IFuture.MultiplierSpecified
		{
			set { this.multiplierSpecified = value; }
			get { return this.multiplierSpecified; }
		}
		EFS_Integer IFuture.Multiplier
		{
			set { this.multiplier = value; }
			get { return this.multiplier; }
		}
		bool IFuture.FutureContractReferenceSpecified
		{
			set { this.futureContractReferenceSpecified = value; }
			get { return this.futureContractReferenceSpecified; }
		}
		EFS_String IFuture.FutureContractReference
		{
			set { this.futureContractReference = value; }
			get { return this.futureContractReference; }
		}
		bool IFuture.MaturitySpecified
		{
			set { this.maturitySpecified = value; }
			get { return this.maturitySpecified; }
		}
		EFS_Date IFuture.Maturity
		{
			set { this.maturity = value; }
			get { return this.maturity; }
		}
		#endregion IFuture Members
	}
	#endregion Future

    #region FxRateAsset
    // EG 20140702 New 
    public partial class FxRateAsset : IFxRateAsset
    {
        #region IFxRateAsset Membres
        IQuotedCurrencyPair IFxRateAsset.QuotedCurrencyPair
        {
            set { this.quotedCurrencyPair = (QuotedCurrencyPair)value; }
            get { return this.quotedCurrencyPair; }
        }
        bool IFxRateAsset.RateSourceSpecified
        {
            set { this.rateSourceSpecified = value; }
            get { return this.rateSourceSpecified; }
        }
        IFxSpotRateSource IFxRateAsset.RateSource
        {
            set { this.rateSource = (FxSpotRateSource)value; }
            get { return this.rateSource; }
        }
        void IFxRateAsset.CreateQuotedCurrencyPair(string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis)
        {
            this.quotedCurrencyPair = new QuotedCurrencyPair(pCurrency1, pCurrency2, pQuoteBasis);
        }
        #endregion
    }
    #endregion FxRateAsset

    #region Index
    // EG 20140702 New 
    public partial class Index : IIndex, IEFS_Array
    {
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }
    #endregion Index


    #region PriceQuoteUnits
    public partial class PriceQuoteUnits : IScheme 
    {
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.priceQuoteUnitsScheme  = value; }
            get { return this.priceQuoteUnitsScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members

    }
    #endregion

	#region Price
	public partial class Price : IPrice
	{
		#region Constructors
		public Price()
		{
			priceNetPrice = new ActualPrice();
			priceAmountRelativeTo = new AmountReference();
			priceDeterminationMethod = new EFS_MultiLineString();
		}
		#endregion Constructors

		#region IPrice Members
		bool IPrice.CommissionSpecified
		{
			set { this.commissionSpecified = value; }
			get { return this.commissionSpecified; }
		}
		ICommission IPrice.Commission
		{
			get { return this.commission; }
		}
		bool IPrice.GrossPriceSpecified
		{
			get { return this.grossPriceSpecified; }
		}
		IActualPrice IPrice.GrossPrice
		{
			get { return this.grossPrice; }
		}
		bool IPrice.DeterminationMethodSpecified
		{
			set { this.priceDeterminationMethodSpecified = value; }
			get { return this.priceDeterminationMethodSpecified; }
		}
		string IPrice.DeterminationMethod
		{
			set { this.priceDeterminationMethod.Value = value; }
			get { return this.priceDeterminationMethod.Value; }
		}
		bool IPrice.AmountRelativeToSpecified
		{
			set { this.priceAmountRelativeToSpecified = value; }
			get { return this.priceAmountRelativeToSpecified; }
		}
		IReference IPrice.AmountRelativeTo
		{
			get { return this.priceAmountRelativeTo; }
		}
		bool IPrice.NetPriceSpecified
		{
			set { this.priceNetPriceSpecified = value; }
			get { return this.priceNetPriceSpecified; }
		}
		IActualPrice IPrice.NetPrice
		{
			set { this.priceNetPrice = (ActualPrice)value; }
			get { return this.priceNetPrice; }
		}
		bool IPrice.AccruedInterestPriceSpecified
		{
			get { return this.accruedInterestPriceSpecified; }
		}
		EFS_Decimal IPrice.AccruedInterestPrice
		{
			get { return this.accruedInterestPrice; }
		}
		#endregion IReturnLegValuationPrice Members
	}
	#endregion Price

	#region SingleUnderlyer
    // EG 20140702 Add Interface IReturnLegMainUnderlyer
    // EG 20140702 Add _sql_Asset
    /// EG 20150302 Add notionalBase constructor (CFD Forex)
    /// EG 20150302 Add notionalBase|notionalBaseSpecified on ISingleUnderlyer|IReturnLegMainUnderlyer (CFD Forex)
    public partial class SingleUnderlyer : ISingleUnderlyer, IReturnLegMainUnderlyer
	{
        // Variable de travail
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public SQL_AssetBase _sql_Asset;

		#region Constructors
        /// EG 20150302 
		public SingleUnderlyer()
		{
			cappedFlooredPrice = new UnderlyerCappedFlooredPrice();
            notionalBase = new Money();
		}
		#endregion Constructors
		#region IOpenUnits Members
		bool IOpenUnits.OpenUnitsSpecified
		{
			set { this.openUnitsSpecified = value; }
			get { return this.openUnitsSpecified; }
		}
		EFS_Decimal IOpenUnits.OpenUnits
		{
			set { this.openUnits = value; }
			get { return this.openUnits; }
		}
		#endregion IOpenUnits Members
		#region ISingleUnderlyer Members
		IUnderlyingAsset ISingleUnderlyer.UnderlyingAsset 
        {
            set { this.underlyingAsset = (UnderlyingAsset) value; }
            get { return this.underlyingAsset; } 
        }
		bool ISingleUnderlyer.DividendPayoutSpecified
		{
			set { this.dividendPayoutSpecified = value; }
			get { return this.dividendPayoutSpecified; }
		}
        IDividendPayout ISingleUnderlyer.DividendPayout
        {
            set { this.dividendPayout = (DividendPayout)value; }
            get { return this.dividendPayout; }
        }
        bool ISingleUnderlyer.NotionalBaseSpecified
        {
            set { this.notionalBaseSpecified = value; }
            get { return this.notionalBaseSpecified; }
        }
        IMoney ISingleUnderlyer.NotionalBase
        {
            set { this.notionalBase = (Money)value; }
            get { return this.notionalBase; }
        }
        #endregion ISingleUnderlyer Members

        #region IReturnLegMainUnderlyer Membres
        bool IReturnLegMainUnderlyer.ExchangeIdSpecified
        {
            set { this.underlyingAsset.exchangeIdSpecified = value; }
            get { return this.underlyingAsset.exchangeIdSpecified; }
        }
        ISpheresIdScheme IReturnLegMainUnderlyer.ExchangeId
        {
            set { this.underlyingAsset.exchangeId = (ExchangeId)value; }
            get { return this.underlyingAsset.exchangeId; }
        }
        bool IReturnLegMainUnderlyer.InstrumentIdSpecified
        {
            get { return ArrFunc.IsFilled(this.underlyingAsset.instrumentId); }
        }
        IScheme[] IReturnLegMainUnderlyer.InstrumentId
        {
            set { this.underlyingAsset.instrumentId = (InstrumentId[])value; }
            get { return this.underlyingAsset.instrumentId; }
        }
        bool IReturnLegMainUnderlyer.CurrencySpecified
        {
            set { this.underlyingAsset.currencySpecified = value; }
            get { return this.underlyingAsset.currencySpecified; }
        }
        ICurrency IReturnLegMainUnderlyer.Currency
        {
            set { this.underlyingAsset.currency = (Currency)value; }
            get { return this.underlyingAsset.currency; }
        }
        bool IReturnLegMainUnderlyer.OpenUnitsSpecified
        {
            set { this.openUnitsSpecified = value; }
            get { return this.openUnitsSpecified; }
        }

        EFS_Decimal IReturnLegMainUnderlyer.OpenUnits
        {
            set { this.openUnits = value; }
            get { return this.openUnits; }
        }
        bool IReturnLegMainUnderlyer.SqlAssetSpecified
        {
            get { return (null != this._sql_Asset); }
        }
        
        SQL_AssetBase IReturnLegMainUnderlyer.SqlAsset
        {
            set { this._sql_Asset = value; }
            get { return this._sql_Asset; }
        }
        Nullable<int> IReturnLegMainUnderlyer.OTCmlId
        {
            set {if (value.HasValue) this.underlyingAsset.OTCmlId = value.Value;}
            get {return this.underlyingAsset.OTCmlId;}
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140812 [XXXXX] Modify
        Nullable<Cst.UnderlyingAsset> IReturnLegMainUnderlyer.UnderlyerAsset
        {
            get
            {
                IUnderlyingAsset _underlyingAsset =  this.underlyingAsset as IUnderlyingAsset;
                Nullable<Cst.UnderlyingAsset> ret = _underlyingAsset.UnderlyerAssetCategory;
                return ret;
            }
        }
        bool IReturnLegMainUnderlyer.NotionalBaseSpecified
        {
            get { return this.notionalBaseSpecified; }
        }

        IMoney IReturnLegMainUnderlyer.NotionalBase
        {
            get { return this.notionalBase; }
        }
        // EG 20170510 [23153] New
        void IReturnLegMainUnderlyer.SetAsset(string pCS, IDbTransaction pDbTransaction)
        {
            if (0 != this.underlyingAsset.OTCmlId)
            {
                IUnderlyingAsset _underlyingAsset = this.underlyingAsset as IUnderlyingAsset;
                Nullable<Cst.UnderlyingAsset> assetCategory = _underlyingAsset.UnderlyerAssetCategory;
                if (assetCategory.HasValue)
                {
                    switch (assetCategory.Value)
                    {
                        case Cst.UnderlyingAsset.EquityAsset:
                            this._sql_Asset = new SQL_AssetEquity(pCS, this.underlyingAsset.OTCmlId);
                            break;
                        case Cst.UnderlyingAsset.FxRateAsset:
                            this._sql_Asset = new SQL_AssetFxRate(pCS, this.underlyingAsset.OTCmlId);
                            break;
                    }
                    if (null != this._sql_Asset)
                    {
                        if (null != pDbTransaction)
                            this._sql_Asset.DbTransaction = pDbTransaction;
                        this._sql_Asset.LoadTable();
                    }
                }
            }
        }
        #endregion
    }
	#endregion SingleUnderlyer

	#region Underlyer
	public partial class Underlyer : IUnderlyer
	{
		#region Constructors
		public Underlyer()
		{
			underlyerSingle = new SingleUnderlyer();
			underlyerBasket = new Basket();
		}
		#endregion Constructors

		#region IUnderlyer Members
		bool IUnderlyer.UnderlyerSingleSpecified 
		{ 
			set { this.underlyerSingleSpecified = value; } 
			get { return this.underlyerSingleSpecified; } 
		}
		ISingleUnderlyer IUnderlyer.UnderlyerSingle 
        {
            set { this.underlyerSingle = (SingleUnderlyer) value; } 
            get { return this.underlyerSingle; } 
        }
        bool IUnderlyer.UnderlyerBasketSpecified
        {
            get { return this.underlyerBasketSpecified; }
            set { this.underlyerBasketSpecified = value; }
        }
        IBasket IUnderlyer.UnderlyerBasket
        {
            get { return this.underlyerBasket; }
            set { underlyerBasket = (Basket)value; }
        }
		#endregion IUnderlyer Members
	}
	#endregion Underlyer
	#region UnderlyingAsset
    /// EG 20150302 Add FxRateAsset on accessor UnderlyerEventType (CFD Forex)
	public partial class UnderlyingAsset : IUnderlyingAsset
    {
		#region IUnderlyingAsset Members
		bool IUnderlyingAsset.ExchangeIdSpecified
		{
			set { this.exchangeIdSpecified = value; }
			get { return this.exchangeIdSpecified; }
		}
        ISpheresIdScheme IUnderlyingAsset.ExchangeId
		{
			set {this.exchangeId = (ExchangeId) value;}
			get {return this.exchangeId;}
		}
		bool IUnderlyingAsset.CurrencySpecified
		{
			set { this.currencySpecified = value; }
			get { return this.currencySpecified; }
		}
		ICurrency IUnderlyingAsset.Currency
		{
			set { this.currency = (Currency)value; }
			get { return this.currency; }
		}
		IScheme[] IUnderlyingAsset.InstrumentId
		{
            set { this.instrumentId = (InstrumentId[])value; }
            get {return this.instrumentId; }
		}
		bool IUnderlyingAsset.DescriptionSpecified
		{
			set { this.descriptionSpecified = value; }
			get { return this.descriptionSpecified; }
		}
		EFS_String IUnderlyingAsset.Description
		{
			set { this.description = value; }
			get { return this.description; }
		}
		bool IUnderlyingAsset.ClearanceSystemSpecified
		{
			set { this.clearanceSystemSpecified = value; }
			get { return this.clearanceSystemSpecified; }
		}
		IScheme IUnderlyingAsset.ClearanceSystem
		{
            set { this.clearanceSystem = (ClearanceSystem) value; }
            get { return this.clearanceSystem; }
		}
		bool IUnderlyingAsset.DefinitionSpecified
		{
			set { this.definitionSpecified = value; }
			get { return this.definitionSpecified; }
		}
		IReference IUnderlyingAsset.Definition
		{
            set { this.definition = (ProductReference) value; }
            get { return this.definition; }
		}
		ICurrency IUnderlyingAsset.CreateCurrency(string pIdC)
		{
			return new Currency(pIdC);
		}
		ISpheresIdScheme IUnderlyingAsset.CreateExchangeId(string pIdM)
		{
			return new ExchangeId(pIdM);
		}
        public IScheme[] CreateInstrumentId(string pId)
        {
            InstrumentId[] ret = new InstrumentId[] { new InstrumentId() };
            ret[0].Value = pId;
            //
            return ret;
        }
		int IUnderlyingAsset.OTCmlId
		{
			set {this.OTCmlId = value;}
			get {return this.OTCmlId;}
		}
        /// EG 20150302 Add FxRateAsset Test (CFD Forex)
		string IUnderlyingAsset.UnderlyerEventType
		{
			get
			{
				string eventType = EventTypeFunc.Underlyer;
				if (this.GetType().Equals(typeof(Bond)))
					eventType = EventTypeFunc.Bond;
                else if (this.GetType().Equals(typeof(DebtSecurity)))
                    eventType = EventTypeFunc.Bond;
				else if (this.GetType().Equals(typeof(ConvertibleBond)))
					eventType = EventTypeFunc.Bond;
				else if (this.GetType().Equals(typeof(EquityAsset)))
					eventType = EventTypeFunc.Share;
				else if (this.GetType().Equals(typeof(Index)))
					eventType = EventTypeFunc.Index;
                else if (this.GetType().Equals(typeof(FxRateAsset)))
                    eventType = EventTypeFunc.FxRate;
				return eventType;
			}
		}
        /// <summary>
        /// Obtient la catégorie de l'asset
        /// </summary>
        /// FI 20161214 [21916] Modify
        // EG 20180423 Analyse du code Correction [CA1065]
        Cst.UnderlyingAsset IUnderlyingAsset.UnderlyerAssetCategory
        {
            get
            {
                Cst.UnderlyingAsset ret = default;
                if (this is Bond)
                    ret = Cst.UnderlyingAsset.Bond;
                else if (this is Security)
                    ret = Cst.UnderlyingAsset.Bond;
                else if (this is ConvertibleBond)
                    ret = Cst.UnderlyingAsset.ConvertibleBond;
                else if (this is EquityAsset)
                    ret = Cst.UnderlyingAsset.EquityAsset;
                else if (this is Deposit)
                    ret = Cst.UnderlyingAsset.Deposit;
                else if (this is Future)
                    ret = Cst.UnderlyingAsset.Future;
                else if (this is FxRateAsset)
                    ret = Cst.UnderlyingAsset.FxRateAsset;
                else if (this is Index)
                    ret = Cst.UnderlyingAsset.Index;
                else if (this is MutualFund)
                    ret = Cst.UnderlyingAsset.MutualFund;
                else if (this is RateIndex)
                    ret = Cst.UnderlyingAsset.RateIndex;
                else if (this is SimpleCreditDefaultSwap)
                    ret = Cst.UnderlyingAsset.SimpleCreditDefaultSwap;
                else if (this is SimpleFra)
                    ret = Cst.UnderlyingAsset.SimpleFra;
                else if (this is SimpleIRSwap)
                    ret = Cst.UnderlyingAsset.SimpleIRSwap;
                else if (this is EfsML.v30.CommodityDerivative.CommodityAsset)
                    ret = Cst.UnderlyingAsset.Commodity;

#if DEBUG
                else
                    throw new InvalidOperationException(StrFunc.AppendFormat("{0} is not implemented", this.GetType().ToString()));
#endif


                return ret;
            }
        }
        
        #endregion IUnderlyingAsset Members
	}
    #endregion UnderlyingAsset
}

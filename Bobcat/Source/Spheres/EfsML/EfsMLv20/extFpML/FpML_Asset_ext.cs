#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;

using EFS.ACommon; 

using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI;

using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;  

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Eqs;
#endregion using directives

namespace FpML.v42.Asset
{
	#region ActualPrice
	public partial class ActualPrice : IActualPrice
	{
		#region IActualPrice Members
		EFS_Decimal IActualPrice.amount
		{
			set { this.amount = value; }
			get { return this.amount; }
		}
		bool IActualPrice.currencySpecified
		{
			set { this.currencySpecified = value; }
			get { return this.currencySpecified; }
		}

		ICurrency IActualPrice.currency
		{
			set { this.currency = (Currency)value; }
			get { return this.currency; }
		}
		PriceExpressionEnum IActualPrice.priceExpression
		{
			set { this.priceExpression = value; }
			get { return this.priceExpression; }
		}
		#endregion IActualPrice Members
	}
	#endregion ActualPrice

	#region Basket
    /// EG 20140702 Upd Interface
	public partial class Basket : IBasket
	{
		#region IOpenUnits Members
		bool IOpenUnits.openUnitsSpecified
		{
			set { this.openUnitsSpecified = value; }
			get { return this.openUnitsSpecified; }
		}
		EFS_Decimal IOpenUnits.openUnits
		{
			set { this.openUnits = value; }
			get { return this.openUnits; }
		}
		#endregion IOpenUnits Members
		#region IBasket Members
        bool IBasket.basketCurrencySpecified
        {
            set { ; }
            get { return false; }
        }
        ICurrency IBasket.basketCurrency
        {
            set { ; }
            get { return null; }
        }
		IBasketConstituent[] IBasket.basketConstituent {get { return this.basketConstituent; }}
		#endregion IBasket Members
	}
	#endregion Basket
	#region BasketConstituent
	public partial class BasketConstituent : IEFS_Array, IBasketConstituent
	{
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
		bool IBasketConstituent.constituentWeightSpecified {get { return this.constituentWeightSpecified; }}
		bool IBasketConstituent.constituentWeightBasketAmountSpecified {get { return this.constituentWeight.constituentWeightBasketAmountSpecified; }}
		decimal IBasketConstituent.constituentWeightBasketAmountValue {get { return this.constituentWeight.constituentWeightBasketAmount.amount.DecValue; }}
		string IBasketConstituent.constituentWeightBasketAmountCurrency {get { return this.constituentWeight.constituentWeightBasketAmount.currency.Value; }}
		bool IBasketConstituent.constituentWeightBasketPercentageSpecified {get { return this.constituentWeight.constituentWeightBasketPercentageSpecified; }}
		decimal IBasketConstituent.constituentWeightBasketPercentage {get { return this.constituentWeight.constituentWeightBasketPercentage.DecValue; }}
		bool IBasketConstituent.constituentWeightOpenUnitsSpecified	{get { return this.constituentWeight.constituentWeightOpenUnitsSpecified; }}
		decimal IBasketConstituent.constituentWeightOpenUnits { get { return this.constituentWeight.constituentWeightOpenUnits.DecValue; } }
		IUnderlyingAsset IBasketConstituent.underlyingAsset { get { return this.underlyingAsset; } }
		#endregion IBasketConstituent Members
	}
	#endregion BasketConstituent

	#region Commission
	public partial class Commission : ICommission
	{
		#region ICommission Members
		EFS_Decimal ICommission.amount 
		{
			get { return this.commissionAmount; }
		}
		CommissionDenominationEnum ICommission.commissionDenomination
		{
			set { this.commissionDenomination = value; }
			get { return this.commissionDenomination; }
		}
		bool ICommission.currencySpecified
		{
			get { return this.currencySpecified; }
		}
		string ICommission.currency 
		{ 
			set { this.currency.Value = value; } 
			get { return this.currency.Value; } 
		}
		IMoney ICommission.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount,pCurrency); }
		#endregion ICommission Members
	}
	#endregion Commission

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
		bool IDividendPayout.dividendPayoutRatioSpecified
		{
			set { this.dividendPayoutRatioSpecified = value;}
			get { return this.dividendPayoutRatioSpecified;}
		}
		EFS_Decimal IDividendPayout.dividendPayoutRatio
		{
			get { return this.dividendPayoutRatio;}
		}
		bool IDividendPayout.dividendPayoutConditionsSpecified
		{
			get { return this.dividendPayoutConditionsSpecified; }
		}
		#endregion IDividendPayout Members
	}
	#endregion DividendPayout

	#region EquityAsset
	public partial class EquityAsset : IEquityAsset
	{
		#region IExchangeTraded Members
		bool IExchangeTraded.relatedExchangeIdSpecified
		{
			set { this.relatedExchangeIdSpecified = value; }
			get { return this.relatedExchangeIdSpecified; }
		}
		IScheme[] IExchangeTraded.relatedExchangeId
		{
			set { this.relatedExchangeId = (ExchangeId[])value; }
			get { return this.relatedExchangeId; }
		}
		bool IExchangeTraded.optionsExchangeIdSpecified
		{
			set { this.optionsExchangeIdSpecified = value; }
			get { return this.optionsExchangeIdSpecified; }
		}

		IScheme[] IExchangeTraded.optionsExchangeId
		{
			set { ((IExchangeTraded)this).firstOptionsExchangeId = ((ExchangeId[])value)[0];}
			get { return new ExchangeId[1]{this.optionsExchangeId}; }
		}
		IScheme IExchangeTraded.firstOptionsExchangeId
		{
			set	{this.optionsExchangeId = (ExchangeId)value;}
			get	{return this.optionsExchangeId;}
		}
		#endregion IExchangeTraded Members
	}
	#endregion EquityAsset


	#region Price
	public partial class Price : IPrice
	{
		#region Constructors
		public Price()
		{
			priceNetPrice = new ActualPrice();
			priceAmountRelativeTo = new Reference();
			priceDeterminationMethod = new EFS_MultiLineString();
		}
		#endregion Constructors

		#region IPrice Members
		bool IPrice.commissionSpecified 
		{
			set { this.commissionSpecified = value; } 
			get { return this.commissionSpecified; } 
		}
		ICommission IPrice.commission 
		{
			get { return this.commission; } 
		}
		bool IPrice.grossPriceSpecified 
		{
			get { return this.grossPriceSpecified; } 
		}
		IActualPrice IPrice.grossPrice 
		{
			get { return this.grossPrice; } 
		}
		bool IPrice.determinationMethodSpecified
		{
			set {this.priceDeterminationMethodSpecified = value;}
			get {return this.priceDeterminationMethodSpecified;}
		}
		string IPrice.determinationMethod
		{
			set {this.priceDeterminationMethod.Value = value;}
			get {return this.priceDeterminationMethod.Value;}
		}
		bool IPrice.amountRelativeToSpecified
		{
			set {this.priceAmountRelativeToSpecified = value;}
			get {return this.priceAmountRelativeToSpecified;}
		}
		IReference IPrice.amountRelativeTo
		{
			get {return this.priceAmountRelativeTo;}
		}
		bool IPrice.netPriceSpecified
		{
			set {this.priceNetPriceSpecified = value; }
			get {return this.priceNetPriceSpecified; }
		}
		IActualPrice IPrice.netPrice
		{
			set {this.priceNetPrice = (ActualPrice)value;}
			get {return this.priceNetPrice;}
		}
		bool IPrice.accruedInterestPriceSpecified
		{
			get {return this.accruedInterestPriceSpecified; }
		}
		EFS_Decimal IPrice.accruedInterestPrice
		{
			get {return this.accruedInterestPrice; }
		}
		#endregion IReturnLegValuationPrice Members
	}
	#endregion Price

	#region SingleUnderlyer
    /// EG 20150302 CFD Forex Add notionalBase|notionalBaseSpecified
	public partial class SingleUnderlyer : ISingleUnderlyer
	{
		#region IOpenUnits Members
		bool IOpenUnits.openUnitsSpecified
		{
			set { this.openUnitsSpecified = value; }
			get { return this.openUnitsSpecified; }
		}
		EFS_Decimal IOpenUnits.openUnits
		{
			set { this.openUnits = value; }
			get { return this.openUnits; }
		}
		#endregion IOpenUnits Members
		#region ISingleUnderlyer Members
		IUnderlyingAsset ISingleUnderlyer.underlyingAsset 
        {
            set { this.underlyingAsset = (UnderlyingAsset)value; } 
            get { return this.underlyingAsset; }
        }
		bool ISingleUnderlyer.dividendPayoutSpecified
		{
			set { this.dividendPayoutSpecified = value; }
			get { return this.dividendPayoutSpecified; }
		}
		IDividendPayout ISingleUnderlyer.dividendPayout
		{
            set { this.dividendPayout = (DividendPayout)value; }
            get { return this.dividendPayout; }
		}
        bool ISingleUnderlyer.notionalBaseSpecified
        {
            set {}
            get { return false; }
        }
        IMoney ISingleUnderlyer.notionalBase
        {
            set {}
            get { return null; }
        }
		#endregion ISingleUnderlyer Members
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
		bool IUnderlyer.underlyerSingleSpecified 
		{
			set { this.underlyerSingleSpecified = value; }
			get { return this.underlyerSingleSpecified; }
		}
		ISingleUnderlyer IUnderlyer.underlyerSingle 
        {
            set { this.underlyerSingle = (SingleUnderlyer)value; }
            get { return this.underlyerSingle; } 
        }
        bool IUnderlyer.underlyerBasketSpecified
        {
            set { this.underlyerBasketSpecified = value; }
            get { return this.underlyerBasketSpecified; }
        }
        IBasket IUnderlyer.underlyerBasket
        {
            set { this.underlyerBasket = (Basket)value; }
            get { return this.underlyerBasket; }
        }
		#endregion IUnderlyer Members
	}
	#endregion Underlyer
	#region UnderlyingAsset
	public partial class UnderlyingAsset : IUnderlyingAsset
    {
		#region IUnderlyingAsset Members
		bool IUnderlyingAsset.exchangeIdSpecified
		{
			set { this.exchangeIdSpecified = value; }
			get { return this.exchangeIdSpecified; }
		}
		ISpheresIdScheme IUnderlyingAsset.exchangeId
		{
			set { this.exchangeId = (ExchangeId)value; }
			get { return this.exchangeId; }
		}
		bool IUnderlyingAsset.currencySpecified
		{
			set { this.currencySpecified = value; }
			get { return this.currencySpecified; }
		}
		ICurrency IUnderlyingAsset.currency
		{
			set { this.currency = (Currency)value; }
			get { return this.currency; }
		}
		IScheme[] IUnderlyingAsset.instrumentId
		{
            set { this.instrumentId = (InstrumentId[])value; }
            get { return this.instrumentId; }
		}
		bool IUnderlyingAsset.descriptionSpecified
		{
			set { this.descriptionSpecified = value; }
			get { return this.descriptionSpecified; }
		}
		EFS_String IUnderlyingAsset.description
		{
			set { this.description = value; }
			get { return this.description; }
		}
		bool IUnderlyingAsset.clearanceSystemSpecified
		{
			set { this.clearanceSystemSpecified = value; }
			get { return this.clearanceSystemSpecified; }
		}
		IScheme IUnderlyingAsset.clearanceSystem
		{
            set { this.clearanceSystem = (ClearanceSystem) value; }
            get { return this.clearanceSystem; }
		}
		bool IUnderlyingAsset.definitionSpecified
		{
			set { this.definitionSpecified = value; }
			get { return this.definitionSpecified; }
		}
		IReference IUnderlyingAsset.definition
		{
            set { this.definition = (ProductReference) value; }
            get { return this.definition; }
		}
		ICurrency IUnderlyingAsset.CreateCurrency(string pIdC)
		{
			return new Currency(pIdC);
		}
		ISpheresIdScheme  IUnderlyingAsset.CreateExchangeId(string pIdM)
		{
			return new ExchangeId(pIdM);
		}
        int IUnderlyingAsset.OTCmlId
        {
            set
            {
                this.OTCmlId = value;
            }
            get
            {
                return this.OTCmlId;
            }
        }
        string IUnderlyingAsset.UnderlyerEventType
		{
			get
			{
				return EventTypeFunc.Underlyer;
			}
		}

        /// <summary>
        /// Obtient la catégorie de l'asset
        /// </summary>
        /// FI 20140812 [XXXXX] add property
        // EG 20180423 Analyse du code Correction [CA1065]
        Cst.UnderlyingAsset IUnderlyingAsset.UnderlyerAssetCategory
        {
            get
            {
                Cst.UnderlyingAsset ret = default(Cst.UnderlyingAsset);
                if (this is Bond)
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
#if DEBUG
                else
                    throw new InvalidOperationException(StrFunc.AppendFormat("{0} is not implemented", this.GetType().ToString()));
#endif


                return ret;
            }
        }

        public IScheme[] CreateInstrumentId(string pId)
        {
            InstrumentId[] ret = new InstrumentId[] { new InstrumentId() };
            ret[0].Value = pId;
            //
            return ret;
        }
		#endregion IUnderlyingAsset Members

    }
    #endregion UnderlyingAsset
}

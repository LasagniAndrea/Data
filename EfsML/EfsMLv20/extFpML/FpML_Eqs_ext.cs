#region Using Directives
using System;
using System.Data;
using System.Collections;
using System.Reflection;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;

using EfsML.v20;
using EfsML.Interface;
using FpML.Interface;
using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Ird;
using FpML.v42.Shared;
using FpML.v42.Asset;
using FpML.v42.EqShared;

#endregion Using Directives
namespace FpML.v42.Eqs
{
	#region AdditionalPaymentAmount
	public partial class AdditionalPaymentAmount : IAdditionalPaymentAmount
	{
		#region IAdditionalPaymentAmount Members
		bool IAdditionalPaymentAmount.paymentAmountSpecified
		{
			set { this.paymentAmountSpecified = value; }
			get { return this.paymentAmountSpecified; }
		}
		IMoney IAdditionalPaymentAmount.paymentAmount
		{
			get { return this.paymentAmount; }
		}
		bool IAdditionalPaymentAmount.formulaSpecified
		{
			get { return this.formulaSpecified; }
		}
		#endregion IAdditionalPaymentAmount Members
	}
	#endregion AdditionalPaymentAmount

	#region AssetSwapLeg
    // EG 20140702 Upd Interface
	public partial class AssetSwapLeg : IReturnSwapLeg
	{
		#region Constructors
		public AssetSwapLeg()
		{
			payerPartyReference = new PartyReference();
			receiverPartyReference = new PartyReference();
		}
		#endregion Constructors

		#region IReturnSwapLeg Members
		IReference IReturnSwapLeg.payerPartyReference 
        { 
            set { this.payerPartyReference = (PartyReference)value; }
            get { return this.payerPartyReference; } 
        }
		IReference IReturnSwapLeg.receiverPartyReference 
        {
            set { this.receiverPartyReference = (PartyReference)value; }
            get { return this.receiverPartyReference; } 
        }
        IReference IReturnSwapLeg.CreateReference
        {
            get { return new PartyReference(); }
        }
        IMoney IReturnSwapLeg.CreateMoney
        {
            get { return new Money(); }
        }
        string IReturnSwapLeg.LegEventCode
        {
            get
            {
                return string.Empty;
            }
        }
        string IReturnSwapLeg.LegEventType
        {
            get
            {
                return string.Empty;
            }
        }
		#endregion IReturnSwapLeg Members
	}
	#endregion AssetSwapLeg

	#region EquityAmount
	public partial class EquityAmount : IReturnSwapAmount
	{
		#region IReturnSwapAmount Members
		EFS_Boolean IReturnSwapAmount.cashSettlement
		{
			get { return this.cashSettlement; }
		}
		bool IReturnSwapAmount.optionsExchangeDividendsSpecified
		{
			get { return this.optionsExchangeDividendsSpecified; }
		}
		EFS_Boolean IReturnSwapAmount.optionsExchangeDividends
		{
			get { return this.optionsExchangeDividends; }
		}
		bool IReturnSwapAmount.additionalDividendsSpecified
		{
			get { return this.additionalDividendsSpecified; }
		}
		EFS_Boolean IReturnSwapAmount.additionalDividends
		{
			get { return this.additionalDividends; }
		}
		#endregion IReturnSwapAmount Members
		#region ILegAmount Members
		bool ILegAmount.paymentCurrencySpecified
		{
			get { return this.paymentCurrencySpecified; }
		}
		IPaymentCurrency ILegAmount.paymentCurrency
		{
			get { return this.paymentCurrency; }
		}
		bool ILegAmount.referenceAmountSpecified
		{
			set { this.legAmountReferenceAmountSpecified = value; }
			get { return this.legAmountReferenceAmountSpecified; }
		}
		IScheme ILegAmount.referenceAmount
		{
			get { return this.legAmountReferenceAmount; }
		}
		bool ILegAmount.calculationDatesSpecified
		{
			get { return this.calculationDatesSpecified; }
		}
		IAdjustableRelativeOrPeriodicDates ILegAmount.calculationDates
		{
			get { return this.calculationDates; }
		}
		#endregion ILegAmount Members
	}
	#endregion EquityAmount
	#region EquityLeg
    // EG 20140702 Upd Interface
	public partial class EquityLeg : IEFS_Array, IReturnLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_ReturnLeg efs_ReturnLeg;
		#endregion Members
		#region Accessors
		#region AdjustedEffectiveDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedEffectiveDate
		{
			get
			{
				EFS_Date adjustedEffectiveDate = new EFS_Date();
				adjustedEffectiveDate.DateValue = EffectiveDateAdjustment.adjustedDate.DateValue;
				return adjustedEffectiveDate;
            }
		}
		#endregion AdjustedEffectiveDate
		#region EffectiveDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate EffectiveDate
		{
			get {return new EFS_EventDate(EffectiveDateAdjustment);}
		}

		#endregion EffectiveDate
		#region EffectiveDateAdjustment
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        private EFS_AdjustableDate EffectiveDateAdjustment
		{
			get {return efs_ReturnLeg.effectiveDateAdjustment; }
		}
		#endregion EffectiveDateAdjustment
		#region PayerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
		{
			get {return payerPartyReference.href; }
		}
		#endregion PayerPartyReference
		#region ReceiverPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
		{
			get {return receiverPartyReference.href; }
		}
		#endregion ReceiverPartyReference
		#region TerminationDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate TerminationDate
		{
			get {return new EFS_EventDate(TerminationDateAdjustment); }
		}
		#endregion TerminationDate
		#region TerminationDateAdjustment
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        private EFS_AdjustableDate TerminationDateAdjustment
		{
			get {return efs_ReturnLeg.terminationDateAdjustment; }
		}
		#endregion TerminationDateAdjustment
		#endregion Accessors

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IReturnLeg Members
        IAdjustableOrRelativeDate IReturnLeg.effectiveDate
        {
            set { this.effectiveDate = (AdjustableOrRelativeDate)value; } 
            get { return this.effectiveDate; }
        }
        IAdjustableOrRelativeDate IReturnLeg.terminationDate
        {
            set { this.terminationDate = (AdjustableOrRelativeDate)value; } 
            get { return this.terminationDate; }
        } 
		IReturnSwapAmount IReturnLeg.returnSwapAmount
		{
            set { this.equityAmount = (EquityAmount)value; }
            get { return this.equityAmount; }
		}
        IReturnLegValuation IReturnLeg.rateOfReturn 
        {
            set { this.valuation = (EquitySwapValuation)value; }
            get { return this.valuation; }
        }
        IReturnSwapNotional IReturnLeg.notional
        {
            get { return this.notional; }
        }
        IUnderlyer IReturnLeg.underlyer
        {
            get { return this.underlyer; }
        }
		EFS_ReturnLeg IReturnLeg.efs_ReturnLeg 
		{ 
			set { this.efs_ReturnLeg = value;}
            get { return this.efs_ReturnLeg; }
		}
		bool IReturnLeg.fxFeatureSpecified
		{
			set { this.fxFeatureSpecified = value; }
			get { return this.fxFeatureSpecified;}
		}
		IFxFeature IReturnLeg.fxFeature 
		{ 
			set { this.fxFeature = (FpML.v42.EqShared.FxFeature)value; }
			get { return this.fxFeature; } 
		}
		IReturn IReturnLeg.@return
		{
            set { this.@return = (Return)value; }
            get { return this.@return; }
		}
        IReturnLegValuation IReturnLeg.CreateRateOfReturn { get { return new EquitySwapValuation(); } }		
		IFxFeature IReturnLeg.CreateFxFeature { get { return new FpML.v42.EqShared.FxFeature(); } }
        IReturnSwapAmount IReturnLeg.CreateReturnSwapAmount
        {
            get
            {
                EquityAmount ret = new EquityAmount();
                ret.cashSettlement = new EFS_Boolean(false);
                //
                return ret;
            }
        }
        IReturn IReturnLeg.CreateReturn
        {
            get
            {
                Return ret = new Return();
                ret.returnType = ReturnTypeEnum.Price;
                //
                return ret;
            }
        }
        IMarginRatio IReturnLeg.CreateMarginRatio
        {
            get
            {
                return null;
            }
        }
        bool IReturnLeg.IsOpenDailyPeriod
        {
            get { return false; }
        }
        bool IReturnLeg.IsDailyPeriod
        {
            get { return false; }
        }
        #endregion IReturnLeg Members
	}
	#endregion EquityLeg
	#region EquityPaymentDates
	public partial class EquityPaymentDates : IReturnSwapPaymentDates
	{
		#region IReturnSwapPaymentDates Members
		IAdjustableOrRelativeDate IReturnSwapPaymentDates.paymentDateFinal { get { return this.equityPaymentDateFinal; } }
		bool IReturnSwapPaymentDates.paymentDatesInterimSpecified { get { return this.equityPaymentDatesInterimSpecified; } }
		IAdjustableOrRelativeDates IReturnSwapPaymentDates.paymentDatesInterim { get { return this.equityPaymentDatesInterim; } }
		#endregion IReturnSwapPaymentDates Members
	}
	#endregion EquityPaymentDates
	#region EquitySwap
    // EG 20140702 Upd Interface
	public partial class EquitySwap : IProduct, IReturnSwap
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

		#region IReturnSwap Members
        bool IReturnSwap.principalExchangeFeaturesSpecified
        {
            set { this.principalExchangeFeaturesSpecified = value; }
            get { return this.principalExchangeFeaturesSpecified; }
        }
        IPrincipalExchangeFeatures IReturnSwap.principalExchangeFeatures
        {
            set { this.principalExchangeFeatures = (PrincipalExchangeFeatures)value; }
            get { return this.principalExchangeFeatures; }
        }
		bool IReturnSwap.earlyTerminationSpecified 
		{
			set { this.earlyTerminationSpecified = value; }
			get { return this.earlyTerminationSpecified; } 
		}
		IReturnSwapEarlyTermination[] IReturnSwap.earlyTermination 
		{
			set { this.earlyTermination = (EquitySwapEarlyTerminationType[])value; }
			get { return this.earlyTermination; } 
		}
		bool IReturnSwap.extraordinaryEventsSpecified 
		{ 
			set { this.extraordinaryEventsSpecified = value; } 
			get { return this.extraordinaryEventsSpecified; } 
		}
		IExtraordinaryEvents IReturnSwap.extraordinaryEvents 
		{ 
			set { this.extraordinaryEvents = (ExtraordinaryEvents)value; } 
			get { return this.extraordinaryEvents; } 
		}
		bool IReturnSwap.additionalPaymentSpecified
		{
			set { this.additionalPaymentSpecified = value; }
			get { return this.additionalPaymentSpecified; }
		}
		IReturnSwapAdditionalPayment[] IReturnSwap.additionalPayment
		{
			set { this.additionalPayment = (EquitySwapAdditionalPayment[])value; }
			get { return this.additionalPayment; }
		}
		IExtraordinaryEvents IReturnSwap.CreateExtraordinaryEvents { get { return new ExtraordinaryEvents(); } }
		IReturnSwapEarlyTermination[] IReturnSwap.CreateEarlyTermination { get { return new EquitySwapEarlyTerminationType[1] { new EquitySwapEarlyTerminationType() }; } }
        EFS_ReturnSwap IReturnSwap.efs_ReturnSwap
        {
            set { ; }
            get { return null; }
        }
		#endregion IReturnSwap Members
	}
	#endregion EquitySwap
	#region EquitySwapAdditionalPayment
	public partial class EquitySwapAdditionalPayment : IEFS_Array, IReturnSwapAdditionalPayment
	{
		#region Constructors
		public EquitySwapAdditionalPayment()
		{
			additionalPaymentDate = new AdjustableOrRelativeDate();
			additionalPaymentAmount = new AdditionalPaymentAmount();
			payerPartyReference = new PartyReference();
			receiverPartyReference = new PartyReference();
			paymentType = new PaymentType();
		}
		#endregion Constructors

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Interface Methods
		#region IReturnSwapAdditionalPayment Members
		IReference IReturnSwapAdditionalPayment.payerPartyReference
		{
			get { return this.payerPartyReference;}
		}
		IReference IReturnSwapAdditionalPayment.receiverPartyReference
		{
			get { return this.receiverPartyReference; }
		}
		IAdjustableOrRelativeDate IReturnSwapAdditionalPayment.additionalPaymentDate
		{
			get { return this.additionalPaymentDate; }
		}
		bool IReturnSwapAdditionalPayment.paymentTypeSpecified
		{
			set { this.paymentTypeSpecified = value; }
			get { return this.paymentTypeSpecified; }
		}
		IScheme IReturnSwapAdditionalPayment.paymentType
		{
			set { this.paymentType = (PaymentType)value; }
			get { return this.paymentType; }
		}
		IScheme IReturnSwapAdditionalPayment.CreatePaymentType
		{
			get { return new PaymentType(); }
		}
		IAdditionalPaymentAmount IReturnSwapAdditionalPayment.additionalPaymentAmount
		{
			get { return this.additionalPaymentAmount; }
		}
		#endregion IReturnSwapAdditionalPayment Members

	}
	#endregion EquitySwapAdditionalPayment
	#region EquitySwapBase
    // EG 20140702 Upd Interface
	public partial class EquitySwapBase : IReturnSwapBase
	{
		#region Accessors
		#region BuyerPartyReference
		public string BuyerPartyReference
		{
			get
			{
				if (buyerPartyReferenceSpecified)
					return buyerPartyReference.href;
				else
					return null;
			}
		}
		#endregion BuyerPartyReference
		#region ReturnSwapType
		public string ReturnSwapType
		{
			get
			{
				string eventType = EventTypeFunc.Basket;
				foreach (EquityLeg leg in returnLeg)
				{
					if (leg.underlyer.underlyerSingleSpecified)
					{
						Type tUnderlyer = leg.underlyer.GetType();
						if (tUnderlyer.Equals(typeof(Index)))
							eventType = EventTypeFunc.Index;
						else
							eventType = EventTypeFunc.Share;
						break;
					}
				}
				return eventType;
			}
		}
		#endregion ReturnSwapType
		#region SellerPartyReference
		public string SellerPartyReference
		{
			get
			{
				if (sellerPartyReferenceSpecified)
					return sellerPartyReference.href;
				else
					return null;
			}
		}
		#endregion SellerPartyReference
		#endregion Accessors

		#region IReturnSwapBase Members
		bool IReturnSwapBase.buyerPartyReferenceSpecified 
		{
			set { this.buyerPartyReferenceSpecified = value; } 
			get { return this.buyerPartyReferenceSpecified; } 
		}
		IReference IReturnSwapBase.buyerPartyReference 
		{
			set { this.buyerPartyReference = (PartyReference)value; } 
			get { return this.buyerPartyReference; } 
		}
		bool IReturnSwapBase.sellerPartyReferenceSpecified 
		{
			set { this.sellerPartyReferenceSpecified = value; } 
			get { return this.sellerPartyReferenceSpecified;} 
		}
		IReference IReturnSwapBase.sellerPartyReference 
		{ 
			set { this.sellerPartyReference = (PartyReference)value; } 
			get { return this.sellerPartyReference; } 
		}
		bool IReturnSwapBase.returnLegSpecified 
        {
            set { this.returnLegSpecified = value; }
            get { return this.returnLegSpecified; } 
        }
		IReturnLeg[] IReturnSwapBase.returnLeg { get { return this.returnLeg; } }
		bool IReturnSwapBase.interestLegSpecified
		{
			set { this.interestLegSpecified = value; }
			get { return this.interestLegSpecified; }
		}
		IInterestLeg[] IReturnSwapBase.interestLeg
		{
			set { this.interestLeg = (InterestLeg[])value; }
			get { return this.interestLeg; }
		}
        IReference IReturnSwapBase.CreatePartyReference { get { return new PartyReference(); } }
        IReference IReturnSwapBase.CreatePartyOrTradeSideReference { get { return null; } }
        IReturnLeg IReturnSwapBase.CreateReturnLeg { get { return new EquityLeg(); } }
        bool IReturnSwapBase.returnSwapLegSpecified
        {
            get { return this.returnLegSpecified || this.interestLegSpecified; }
        }
        IReturnSwapLeg[] IReturnSwapBase.returnSwapLeg
        {
            get
            {
                AssetSwapLeg[] _assetSwapLeg = null;
                ArrayList aAssetSwapLeg = new ArrayList();
                if (this.returnLegSpecified)
                    aAssetSwapLeg.AddRange(this.returnLeg);
                if (this.interestLegSpecified)
                    aAssetSwapLeg.AddRange(this.interestLeg);
                if (0 < aAssetSwapLeg.Count)
                    _assetSwapLeg = (AssetSwapLeg[])aAssetSwapLeg.ToArray(typeof(AssetSwapLeg));
                return _assetSwapLeg;
            }
        }
        // FI 20170116 [21916] RptSide (R majuscule)
        FixML.Interface.IFixTrdCapRptSideGrp[] IReturnSwapBase.RptSide
        {
            set { ; }
            get { return null; }
        }
		#endregion IReturnSwapBase Members
	}
	#endregion EquitySwapBase
	#region EquitySwapEarlyTerminationType
	public partial class EquitySwapEarlyTerminationType : IEFS_Array,IReturnSwapEarlyTermination
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent,
			ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members

		#region IReturnSwapEarlyTermination Members
		IReference IReturnSwapEarlyTermination.partyReference
		{
			set { this.partyReference = (PartyReference)value;}
			get { return this.partyReference;}
		}
		IStartingDate IReturnSwapEarlyTermination.startingDate
		{
			set { this.startingDate = (StartingDate)value; }
			get { return this.startingDate; }
		}
		IStartingDate IReturnSwapEarlyTermination.CreateStartingDate {get {return new StartingDate();}}
		#endregion IReturnSwapEarlyTermination Members
	}
	#endregion EquitySwapEarlyTerminationType
	#region EquitySwapNotional
	public partial class EquitySwapNotional : IReturnSwapNotional
	{
		#region Accessors
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		public string id
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
		#endregion Accessors
		#region Constructors
		public EquitySwapNotional()
		{
			equitySwapNotionalDeterminationMethod = new EFS_MultiLineString();
			equitySwapNotionalNotionalAmount = new Money();
			equitySwapNotionalAmountRelativeTo = new Reference();
		}
		#endregion Constructors

		#region IReturnSwapNotional Members
		bool IReturnSwapNotional.determinationMethodSpecified {get { return this.equitySwapNotionalDeterminationMethodSpecified; }}
		EFS_MultiLineString IReturnSwapNotional.determinationMethod { get { return this.equitySwapNotionalDeterminationMethod; } }
		bool IReturnSwapNotional.notionalAmountSpecified {get { return this.equitySwapNotionalNotionalAmountSpecified; }}
		IMoney IReturnSwapNotional.notionalAmount { get { return this.equitySwapNotionalNotionalAmount;} }
		bool IReturnSwapNotional.relativeToSpecified {get { return this.equitySwapNotionalAmountRelativeToSpecified; }}
		IReference IReturnSwapNotional.relativeTo { get { return this.equitySwapNotionalAmountRelativeTo; } }
		string IReturnSwapNotional.id {get { return this.id; } }
		#endregion IReturnSwapNotional Members
	}
	#endregion EquitySwapNotional
	#region EquitySwapTransactionSupplement
	public partial class EquitySwapTransactionSupplement : IProduct
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion EquitySwapTransactionSupplement
	#region EquitySwapValuation
    // EG 20140702 Upd Interface
	public partial class EquitySwapValuation : IReturnLegValuation
	{
		#region IReturnLegValuation Members
		IReturnLegValuationPrice IReturnLegValuation.initialPrice 
        {
            set { this.initialPrice = (EquitySwapValuationPrice)value; }
            get { return this.initialPrice; } 
        }
		bool IReturnLegValuation.valuationPriceInterimSpecified
		{
			set { this.valuationPriceInterimSpecified = value; }
			get { return this.valuationPriceInterimSpecified; }
		}
		IReturnLegValuationPrice IReturnLegValuation.valuationPriceInterim
		{
            set { this.valuationPriceInterim = (EquitySwapValuationPrice)value; }
            get { return this.valuationPriceInterim; }
		}
		IReturnLegValuationPrice IReturnLegValuation.valuationPriceFinal
		{
            set { this.valuationPriceFinal = (EquitySwapValuationPrice)value; }
            get { return this.valuationPriceFinal; }
		}
		IReturnLegValuationPrice IReturnLegValuation.CreateReturnLegValuationPrice {get {return new EquitySwapValuationPrice();}}
        IReturnSwapPaymentDates IReturnLegValuation.paymentDates 
        {
            set { this.equityPaymentDates = (EquityPaymentDates) value; }
            get { return this.equityPaymentDates; } 
        }
        bool IReturnLegValuation.notionalResetSpecified
        {
            set { ; }
            get { return false; }
        }
        EFS_Boolean IReturnLegValuation.notionalReset
        {
            set { ; }
            get { return new EFS_Boolean(false); }
        }
        bool IReturnLegValuation.marginRatioSpecified
        {
            set { ; }
            get { return false; }
        }
        IMarginRatio IReturnLegValuation.marginRatio
        {
            set { ; }
            get { return null; }
        }
        EFS_RateOfReturn IReturnLegValuation.efs_RateOfReturn
        {
            set { ; }
            get { return null; }
        }
		#endregion IReturnLegValuation Members
	}
	#endregion EquitySwapValuation
	#region EquitySwapValuationPrice
    public partial class EquitySwapValuationPrice : IReturnLegValuationPrice
    {
        #region IReturnLegValuationPrice Members
        bool IReturnLegValuationPrice.valuationRulesSpecified
        {
            set { this.equityValuationSpecified = value; }
            get { return this.equityValuationSpecified; }
        }
        IEquityValuation IReturnLegValuationPrice.valuationRules
        {
            set { this.equityValuation = (EquityValuation)value; }
            get { return this.equityValuation; }
        }
        IEquityValuation IReturnLegValuationPrice.CreateValuationRules() { return new EquityValuation(); }
        IAdjustableRelativeOrPeriodicDates IReturnLegValuationPrice.CreateAdjustableRelativeOrPeriodicDates() { return new AdjustableRelativeOrPeriodicDates(); }
        IPeriodicDates IReturnLegValuationPrice.CreatePeriodicDates() { return new PeriodicDates(); }
        IActualPrice IReturnLegValuationPrice.CreatePrice() { return new ActualPrice(); }
        ICurrency IReturnLegValuationPrice.CreateCurrency() { return new Currency(); }
        #endregion IReturnLegValuationPrice Members

    }
	#endregion EquitySwapValuationPrice

	#region InterestCalculation
    // EG 20170510 [23153] Add SetAsset 
	public partial class InterestCalculation : IInterestCalculation
	{
		#region IInterestCalculation Members
		bool IInterestCalculation.compoundingSpecified
		{
			get { return false;}
		}
        ICompounding IInterestCalculation.compounding
        {
            get { return null; }
        }
        DayCountFractionEnum IInterestCalculation.dayCountFraction
		{
			set { this.dayCountFraction = value; }
			get { return this.dayCountFraction; }
		}
        // EG 20170510 [23153]
        void IInterestAccrualsMethod.SetAsset(string pCS, IDbTransaction pDbTransaction)
        {
        }
		#endregion IInterestCalculation Members

	}
	#endregion InterestCalculation

	#region InterestLeg
    // EG 20140702 Upd Interface
	public partial class InterestLeg : IEFS_Array, IInterestLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_InterestLeg efs_InterestLeg;
		#endregion Members
		
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members

		#region IInterestLeg Members
		IReturnSwapLeg IInterestLeg.returnSwapLeg { get { return (IReturnSwapLeg)this; } }
		IInterestCalculation IInterestLeg.interestCalculation 
		{ 
			set { this.interestCalculation = (InterestCalculation)value; } 
			get { return this.interestCalculation; } 
		}
		IInterestLegCalculationPeriodDates IInterestLeg.calculationPeriodDates 
		{
			set { this.interestLegCalculationPeriodDates = (InterestLegCalculationPeriodDates)value;}
			get { return this.interestLegCalculationPeriodDates;}
		}
		bool IInterestLeg.stubCalculationPeriodSpecified {get { return this.stubCalculationPeriodSpecified;}}
		IStubCalculationPeriod IInterestLeg.stubCalculationPeriod{get { return this.stubCalculationPeriod;}}
		ILegAmount IInterestLeg.interestAmount 
		{
			set { this.interestAmount = (LegAmount)value; }
			get { return this.interestAmount; } 
		}
        IAdjustableRelativeOrPeriodicDates2 IInterestLeg.paymentDates {get {return null;}
        }
		IReturnSwapNotional IInterestLeg.notional {get { return this.notional; }}
		IInterestCalculation IInterestLeg.CreateInterestCalculation { get { return new InterestCalculation(); } }
		IInterestLegCalculationPeriodDates IInterestLeg.CreateCalculationPeriodDates { get { return new InterestLegCalculationPeriodDates(); } }
		ILegAmount IInterestLeg.CreateLegAmount { get { return new LegAmount(); } }
        IInterestLegResetDates IInterestLeg.CreateResetDates { get { return new InterestLegResetDates(); } }
        IAdjustableRelativeOrPeriodicDates2 IInterestLeg.CreatePaymentDates
        {
            get
            {
                return null;
            }
        }
        IResetFrequency IInterestLeg.CreateResetFrequency { get { return new ResetFrequency(); } }
        IRelativeDateOffset IInterestLeg.CreateRelativeDateOffset { get { return new RelativeDateOffset(); } }
        EFS_InterestLeg IInterestLeg.efs_InterestLeg
        {
            set { this.efs_InterestLeg = value; }
            get { return this.efs_InterestLeg; }
        }
        bool IInterestLeg.IsPeriodRelativeToReturnLeg
        {
            get { return false; }
        }
		#endregion IInterestLeg Members


        string IReturnSwapLeg.LegEventCode
        {
            get
            {
                return string.Empty;
            }
        }
        string IReturnSwapLeg.LegEventType
        {
            get
            {
                return string.Empty;
            }
        }
	}
	#endregion InterestLeg
	#region InterestLegCalculationPeriodDates
    // EG 20140702 Upd Interface
	public partial class InterestLegCalculationPeriodDates : IInterestLegCalculationPeriodDates
	{
		#region IInterestLegCalculationPeriodDates Members
		IAdjustableOrRelativeDate IInterestLegCalculationPeriodDates.effectiveDate {get { return this.effectiveDate;}}
		IAdjustableOrRelativeDate IInterestLegCalculationPeriodDates.terminationDate { get { return this.terminationDate;} }
		IAdjustableRelativeOrPeriodicDates2 IInterestLegCalculationPeriodDates.paymentDates 
        {
            set { ; }
            get { return null; } 
        }
		IInterestLegResetDates IInterestLegCalculationPeriodDates.resetDates 
        {
            set { this.interestLegResetDates = (InterestLegResetDates)value; }
            get { return this.interestLegResetDates;}
        }
        string IInterestLegCalculationPeriodDates.id
        {
            set { this.id = value; }
            get { return this.id; }
        }
		#endregion IInterestLegCalculationPeriodDates Members
	}
	#endregion InterestLegCalculationPeriodDates
	#region InterestLegResetDates
    // EG 20140702 Upd Interface
	public partial class InterestLegResetDates : IInterestLegResetDates
	{
		#region Constructors
		public InterestLegResetDates()
		{
			resetDatesResetFrequency = new ResetFrequency();
		}
		#endregion Constructors

		#region IInterestLegResetDates Members
        IReference IInterestLegResetDates.calculationPeriodDatesReference
        {
            set { this.calculationPeriodDatesReference = (DateReference)value; }
            get { return this.calculationPeriodDatesReference; }
        }
        bool IInterestLegResetDates.resetRelativeToSpecified
        {
            set { this.resetDatesResetRelativeToSpecified = value; }
            get { return this.resetDatesResetRelativeToSpecified; }
        }
        ResetRelativeToEnum IInterestLegResetDates.resetRelativeTo
        {
            set { this.resetDatesResetRelativeTo = value; }
            get { return this.resetDatesResetRelativeTo; }
        }
        bool IInterestLegResetDates.resetFrequencySpecified
        {
            set { this.resetDatesResetFrequencySpecified = value; }
            get { return this.resetDatesResetFrequencySpecified; }
        }
        IResetFrequency IInterestLegResetDates.resetFrequency
        {
            set { this.resetDatesResetFrequency = (ResetFrequency)value; }
            get { return this.resetDatesResetFrequency; }
        }
        bool IInterestLegResetDates.initialFixingDateSpecified
        {
            set { ; }
            get { return false; }
        }
        IRelativeDateOffset IInterestLegResetDates.initialFixingDate
        {
            set { ; }
            get { return null; }
        }
        bool IInterestLegResetDates.fixingDatesSpecified
        {
            set { ; }
            get { return false; }
        }
        IAdjustableDatesOrRelativeDateOffset IInterestLegResetDates.fixingDates
        {
            set { ; }
            get { return null; }
        }

        #endregion IInterestLegResetDates Members
	}
	#endregion InterestLegResetDates
	#region LegAmount
	public partial class LegAmount : ILegAmount
	{
		#region Constructors
		public LegAmount()
		{
			legAmountFormula = new Formula();
			legAmountReferenceAmount = new ReferenceAmount();
			legAmountVariance = new Variance();
		}
		#endregion Constructors

		#region ILegAmount Members
		bool ILegAmount.paymentCurrencySpecified
		{
			get { return this.paymentCurrencySpecified; }
		}
		IPaymentCurrency ILegAmount.paymentCurrency
		{
			get { return this.paymentCurrency; }
		}
		bool ILegAmount.referenceAmountSpecified
		{
			set { this.legAmountReferenceAmountSpecified = value; }
			get { return this.legAmountReferenceAmountSpecified; }
		}
		IScheme ILegAmount.referenceAmount
		{
			get { return this.legAmountReferenceAmount; }
		}
		bool ILegAmount.calculationDatesSpecified
		{
			get { return this.calculationDatesSpecified; }
		}
		IAdjustableRelativeOrPeriodicDates ILegAmount.calculationDates
		{
			get { return this.calculationDates; }
		}
		#endregion ILegAmount Members
	}
	#endregion LegAmount

	#region PrincipalExchangeAmount
	public partial class PrincipalExchangeAmount : IPrincipalExchangeAmount
	{
		#region Constructors
		public PrincipalExchangeAmount()
		{
			exchangeAmountRelativeTo = new Reference();
			exchangeDeterminationMethod = new EFS_MultiLineString();
			exchangePrincipalAmount = new Money();
		}
		#endregion Constructors

		#region IPrincipalExchangeAmount Members
		bool IPrincipalExchangeAmount.relativeToSpecified { get { return this.exchangeAmountRelativeToSpecified; } }
		IReference IPrincipalExchangeAmount.relativeTo { get { return this.exchangeAmountRelativeTo; } }
		bool IPrincipalExchangeAmount.determinationMethodSpecified { get { return this.exchangeDeterminationMethodSpecified; } }
		EFS_MultiLineString IPrincipalExchangeAmount.determinationMethod { get { return this.exchangeDeterminationMethod; } }
		bool IPrincipalExchangeAmount.amountSpecified { get { return this.exchangePrincipalAmountSpecified; } }
		IMoney IPrincipalExchangeAmount.amount { get { return this.exchangePrincipalAmount; } }
		#endregion IPrincipalExchangeAmount Members
	}
	#endregion PrincipalExchangeAmount
	#region PrincipalExchangeDescriptions
	public partial class PrincipalExchangeDescriptions : IEFS_Array,IPrincipalExchangeDescriptions
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members

		#region IPrincipalExchangeDescriptions Members
		IReference IPrincipalExchangeDescriptions.payerPartyReference {get { return this.payerPartyReference;}}
		IReference IPrincipalExchangeDescriptions.receiverPartyReference {get { return this.receiverPartyReference;}}
		IPrincipalExchangeAmount IPrincipalExchangeDescriptions.principalExchangeAmount {get { return this.principalExchangeAmount;}}
		IAdjustableOrRelativeDate IPrincipalExchangeDescriptions.principalExchangeDate {get { return this.principalExchangeDate;} }
		#endregion IPrincipalExchangeDescriptions Members
	}
	#endregion PrincipalExchangeDescriptions
	#region PrincipalExchangeFeatures
	public partial class PrincipalExchangeFeatures : IPrincipalExchangeFeatures
	{
		#region IPrincipalExchangeFeatures Members
		bool IPrincipalExchangeFeatures.descriptionsSpecified {get { return this.principalExchangeDescriptionsSpecified; }}
		IPrincipalExchanges IPrincipalExchangeFeatures.principalExchanges { get { return this.principalExchanges; } }
		IPrincipalExchangeDescriptions[] IPrincipalExchangeFeatures.descriptions { get { return this.principalExchangeDescriptions; } }
		#endregion IPrincipalExchangeFeatures Members
	}
	#endregion PrincipalExchangeFeatures

	#region Return
	public partial class Return : IReturn
	{
		#region IReturn Members
		ReturnTypeEnum IReturn.returnType
		{
			set {this.returnType = value; }
			get {return this.returnType; }
		}
		bool IReturn.dividendConditionsSpecified
		{
			set { this.dividendConditionsSpecified = value; }
			get { return this.dividendConditionsSpecified; }
		}
		IDividendConditions IReturn.dividendConditions
		{
            set { this.dividendConditions = (DividendConditions)value; }
			get { return this.dividendConditions; }
		}
        IDividendConditions IReturn.CreateDividendConditions
        {
            get { return new DividendConditions(); }
        }
		#endregion IReturn Members	
	}
	#endregion Return


	#region StartingDate
	public partial class StartingDate : IStartingDate
	{
		#region Constructors
		public StartingDate()
		{
			startingDateAdjustableDate = new AdjustableDate();
			startingDateDateRelativeTo = new DateRelativeTo();
		}
		#endregion Constructors

		#region IStartingDate Members
		bool IStartingDate.relativeToSpecified
		{
			set { this.startingDateDateRelativeToSpecified = value; }
			get { return this.startingDateDateRelativeToSpecified; }
		}
		string IStartingDate.dateRelativeTo
		{
			set 
			{ 
				this.startingDateDateRelativeTo = new DateRelativeTo();
				this.startingDateDateRelativeTo.href = value;
			}
			get { return this.startingDateDateRelativeTo.href; }
		}
		bool IStartingDate.adjustableDateSpecified
		{
			get { return this.startingDateAdjustableDateSpecified; }
		}
		IAdjustableDate IStartingDate.adjustableDate
		{
			get { return this.startingDateAdjustableDate; }
		}
		#endregion IStartingDate Members
	}
	#endregion StartingDate
	#region StubCalculationPeriod
	public partial class StubCalculationPeriod : IStubCalculationPeriod
	{
		#region IStubCalculationPeriod Members
		bool IStubCalculationPeriod.initialStubSpecified {get { return this.initialStubSpecified;}}
		IStub IStubCalculationPeriod.initialStub {get { return this.initialStub;}}
		bool IStubCalculationPeriod.finalStubSpecified {get { return this.finalStubSpecified;}}
		IStub IStubCalculationPeriod.finalStub {get { return this.finalStub;} }
		#endregion IStubCalculationPeriod Members
	}
	#endregion StubCalculationPeriod

	#region VarianceLeg
	public partial class VarianceLeg : IEFS_Array, IVarianceLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_VarianceLeg efs_VarianceLeg;
		#endregion Members

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IReturnSwapLeg Members
		IReference IReturnSwapLeg.payerPartyReference 
        {
            set { this.payerPartyReference = (PartyReference)value; } 
            get { return this.payerPartyReference; } 
        }
		IReference IReturnSwapLeg.receiverPartyReference 
        {
            set { this.receiverPartyReference = (PartyReference)value; }
            get { return this.receiverPartyReference; } 
        }
		#endregion IReturnSwapLeg Members
	}
	#endregion VarianceLeg


}

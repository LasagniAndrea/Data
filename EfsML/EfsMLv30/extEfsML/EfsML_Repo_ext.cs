#region using directives
using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EFS.EFSTools;

using EfsML.Enum;
using EfsML.DynamicData;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.Doc;
using EfsML.v30.Shared;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Enum;
using FpML.v44.Shared;
#endregion using directives


namespace EfsML.v30.Repo
{
	#region AdjustableOffset
	public partial class AdjustableOffset : IAdjustableOffset,IComparable
	{
		#region Constructors
		public AdjustableOffset()
		{
			businessCentersNoneSpecified	= true;
			businessCentersNone				= new Empty();
			businessCentersReference		= new BusinessCentersReference();
			businessCentersDefine			= new BusinessCenters();
		}
		#endregion Constructors

		#region IComparable Members
		#region CompareTo
		public int CompareTo(object obj)
		{
			int ret = -1;
			//	
			if ((period == ((Interval)obj).period) && (periodMultiplier.IntValue == ((Interval)obj).periodMultiplier.IntValue))
				ret = 0;
			//
			return ret;
		}
		#endregion CompareTo
		#endregion IComparable Members
		#region IOffset Members
		bool IOffset.dayTypeSpecified
		{
			set {this.dayTypeSpecified = value;}
			get { return this.dayTypeSpecified; }
		}
		DayTypeEnum IOffset.dayType
		{
			set { this.dayType = value; }
			get { return this.dayType; }
		}
		IBusinessCenters IOffset.GetBusinessCentersCurrency(string pConnectionString, params string[] pCurrencies)
		{
			return ((IOffset)this).GetBusinessCentersCurrency(pConnectionString, pCurrencies);
		}
		IBusinessDayAdjustments IOffset.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, params string[] pIdBC)
		{
			return ((IOffset)this).CreateBusinessDayAdjustments(pBusinessDayConvention, pIdBC);
		}
		#endregion IOffset Members
		#region IInterval Membres
		PeriodEnum IInterval.period
		{
			set { this.period = value; }
			get { return this.period; }
		}
		EFS_Integer IInterval.periodMultiplier
		{
			set { this.periodMultiplier = value; }
			get { return this.periodMultiplier; }
		}
		IInterval IInterval.GetInterval(int pMultiplier, PeriodEnum pPeriod) { return new Interval(pPeriod.ToString(), pMultiplier); }
		IRounding IInterval.GetRounding(RoundingDirectionEnum pRoundingDirection, int pPrecision) { return new Rounding(pRoundingDirection, pPrecision); }
		int IInterval.CompareTo(object obj) { return this.CompareTo(obj); }
		#endregion IInterval Membres
		#region IAdjustableOffset Members
		bool IAdjustableOffset.businessCentersNoneSpecified
		{
			set { this.businessCentersNoneSpecified = value; }
			get { return this.businessCentersNoneSpecified; }
		}
		object IAdjustableOffset.businessCentersNone
		{
			set { this.businessCentersNone = (Empty)value; }
			get { return this.businessCentersNone; }
		}
		bool IAdjustableOffset.businessCentersDefineSpecified
		{
			set { this.businessCentersDefineSpecified = value; }
			get { return this.businessCentersDefineSpecified; }
		}
		IBusinessCenters IAdjustableOffset.businessCentersDefine
		{
			set { this.businessCentersDefine = (BusinessCenters)value; }
			get { return this.businessCentersDefine; }
		}
		bool IAdjustableOffset.businessCentersReferenceSpecified
		{
			set { this.businessCentersReferenceSpecified = value; }
			get { return this.businessCentersReferenceSpecified; }
		}
		IReference IAdjustableOffset.businessCentersReference
		{
			set { this.businessCentersReference = (BusinessCentersReference)value; }
			get { return this.businessCentersReference; }
		}
		string IAdjustableOffset.businessCentersReferenceValue
		{
			get
			{
				if (this.businessCentersReferenceSpecified)
					return this.businessCentersReference.href;
				return string.Empty;
			}
		}
		#endregion IAdjustableOffset Members
	}
	#endregion AdjustableOffset
	#region AtomicSettlementTransfer
	public partial class AtomicSettlementTransfer : IAtomicSettlementTransfer
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
		#region IAtomicSettlementTransfer Members
		bool IAtomicSettlementTransfer.suppressSpecified
		{
			set { this.suppressSpecified = value; }
			get { return this.suppressSpecified; }
		}
		bool IAtomicSettlementTransfer.suppress
		{
			set { this.suppress = new EFS_Boolean(value); }
			get { return this.suppress.BoolValue; }
		}
		#endregion IAtomicSettlementTransfer Members
	}
	#endregion AtomicSettlementTransfer
	#region Attribution
	public partial class Attribution : IAttribution
	{
		#region IAttribution Members
		IScheme IAttribution.type
		{
			set { this.type = (AttributionType)value; }
			get { return this.type;}
		}
		bool IAttribution.settlementAmountSpecified
		{
			set { this.settlementAmountSpecified = value; }
			get { return this.settlementAmountSpecified; }
		}
		EFS_Decimal IAttribution.settlementAmount
		{
			set { this.settlementAmount = value; }
			get { return this.settlementAmount; }
		}
		bool IAttribution.baseAmountSpecified
		{
			set { this.baseAmountSpecified = value; }
			get { return this.baseAmountSpecified; }
		}
		EFS_Decimal IAttribution.baseAmount
		{
			set { this.baseAmount = value; }
			get { return this.baseAmount; }
		}
		bool IAttribution.underlyingAmountSpecified
		{
			set { this.underlyingAmountSpecified = value; }
			get { return this.underlyingAmountSpecified; }
		}
		EFS_Decimal IAttribution.underlyingAmount
		{
			set { this.underlyingAmount = value; }
			get { return this.underlyingAmount; }
		}
		#endregion IAttribution Members
	}
	#endregion Attribution
	#region Attributions
	public partial class Attributions : IAttributions
	{
		#region IAttributions Members
		ICurrency IAttributions.settlementCurrency
		{
			set {this.settlementCurrency = (Currency)value;}
			get {return this.settlementCurrency;}
		}
		ICurrency IAttributions.baseCurrency
		{
			set { this.baseCurrency = (Currency)value; }
			get { return this.baseCurrency; }
		}
		bool IAttributions.underlyingCurrencySpecified
		{
			set { this.underlyingCurrencySpecified = value; }
			get { return this.underlyingCurrencySpecified; }
		}
		ICurrency IAttributions.underlyingCurrency
		{
			set { this.underlyingCurrency = (Currency)value; }
			get { return this.underlyingCurrency; }
		}
		bool IAttributions.attributionSpecified
		{
			set { this.attributionSpecified = value; }
			get { return this.attributionSpecified; }
		}
		IAttribution[] IAttributions.attribution
		{
			set { this.attribution = (Attribution[])value; }
			get { return this.attribution; }
		}
		#endregion IAttributions Members
	}
	#endregion Attributions
	#region AttributionType
	public partial class AttributionType : IScheme
	{
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.attributionTypeScheme; }
			set { this.attributionTypeScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion AttributionType

	#region BondCollateral
	public partial class BondCollateral : IBondCollateral 
	{
		#region IBondCollateral Members
		IMoney IBondCollateral.nominalAmount
		{
			set { this.nominalAmount = (Money)value; }
			get { return this.nominalAmount;}
		}
		bool IBondCollateral.cleanPriceSpecified
		{
			set { this.cleanPriceSpecified = value; }
			get { return this.cleanPriceSpecified; }
		}
		EFS_Decimal IBondCollateral.cleanPrice
		{
			set { this.cleanPrice = value; }
			get { return this.cleanPrice; }
		}
		bool IBondCollateral.accrualsSpecified
		{
			set { this.accrualsSpecified = value; }
			get { return this.accrualsSpecified; }
		}
		EFS_Decimal IBondCollateral.accruals
		{
			set { this.accruals = value; }
			get { return this.accruals; }
		}
		bool IBondCollateral.dirtyPriceSpecified
		{
			set { this.dirtyPriceSpecified = value; }
			get { return this.dirtyPriceSpecified; }
		}
		EFS_Decimal IBondCollateral.dirtyPrice
		{
			set { this.dirtyPrice = value; }
			get { return this.dirtyPrice; }
		}
		bool IBondCollateral.relativePriceSpecified
		{
			set { this.relativePriceSpecified = value; }
			get { return this.relativePriceSpecified; }
		}
		IRelativePrice IBondCollateral.relativePrice
		{
			set { this.relativePrice = (RelativePrice)value; }
			get { return this.relativePrice; }
		}
		bool IBondCollateral.yieldToMaturitySpecified
		{
			set { this.yieldToMaturitySpecified = value; }
			get { return this.yieldToMaturitySpecified; }
		}
		EFS_Decimal IBondCollateral.yieldToMaturity
		{
			set { this.yieldToMaturity = value; }
			get { return this.yieldToMaturity; }
		}
		bool IBondCollateral.inflationFactorSpecified
		{
			set { this.inflationFactorSpecified = value; }
			get { return this.inflationFactorSpecified; }
		}
		EFS_Decimal IBondCollateral.inflationFactor
		{
			set { this.inflationFactor = value; }
			get { return this.inflationFactor; }
		}
		bool IBondCollateral.interestStartDateSpecified
		{
			set { this.interestStartDateSpecified = value; }
			get { return this.interestStartDateSpecified; }
		}
		IAdjustableOrRelativeDate IBondCollateral.interestStartDate
		{
			set { this.interestStartDate = (AdjustableOrRelativeDate)value; }
			get { return this.interestStartDate; }
		}
		bool IBondCollateral.poolSpecified
		{
			set { this.poolSpecified = value; }
			get { return this.poolSpecified; }
		}
		IAssetPool IBondCollateral.pool
		{
			set { this.pool = (AssetPool)value; }
			get { return this.pool; }
		}
		#endregion IBondCollateral Members
	}
	#endregion BondCollateral

	#region CashRepricingEvent
	public partial class CashRepricingEvent : ICashRepricingEvent
	{
		#region ICashRepricingEvent Members
		bool ICashRepricingEvent.collateralSpecified
		{
			set { this.collateralSpecified = value; }
			get { return this.collateralSpecified; }
		}
		ICollateralValuation ICashRepricingEvent.collateral
		{
			set { this.collateral = (CollateralValuation)value; }
			get { return this.collateral; }
		}
		EFS_Boolean ICashRepricingEvent.combinedInterestPayout
		{
			set { this.combinedInterestPayout = value; }
			get { return this.combinedInterestPayout; }
		}
		bool ICashRepricingEvent.transferSpecified
		{
			set { this.transferSpecified = value; }
			get { return this.transferSpecified; }
		}
		ITransfer ICashRepricingEvent.transfer
		{
			set { this.transfer = (Transfer)value; }
			get { return this.transfer; }
		}
		#endregion ICashRepricingEvent Members
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
	}
	#endregion CashRepricingEvent
	#region CashTransfer
	public partial class CashTransfer : ICashTransfer
	{
		#region ICashTransfer Members
		IMoney ICashTransfer.transferAmount
		{
			set { this.transferAmount = (Money)value; }
			get { return this.transferAmount; }
		}
		IReference ICashTransfer.payerPartyReference
		{
			set { this.payerPartyReference = (PartyReference)value; }
			get { return this.payerPartyReference; }
		}
		IReference ICashTransfer.receiverPartyReference
		{
			set { this.receiverPartyReference = (PartyReference)value; }
			get { return this.receiverPartyReference; }
		}
		bool ICashTransfer.attributionsSpecified
		{
			set { this.attributionsSpecified = value; }
			get { return this.attributionsSpecified; }
		}
		IAttributions[] ICashTransfer.attributions
		{
			set { this.attributions = (Attributions[])value; }
			get { return this.attributions; }
		}
		#endregion ICashTransfer Members
		#region IAtomicSettlementTransfer Members
		bool IAtomicSettlementTransfer.suppressSpecified
		{
			set { this.suppressSpecified = value; }
			get { return this.suppressSpecified; }
		}
		bool IAtomicSettlementTransfer.suppress
		{
			set { this.suppress = new EFS_Boolean(value); }
			get { return this.suppress.BoolValue; }
		}
		#endregion IAtomicSettlementTransfer Members
	}
	#endregion CashTransfer
	#region CollateralSubstitutionEvent
	public partial class CollateralSubstitutionEvent : ICollateralSubstitutionEvent
	{
		#region ICollateralSubstitutionEvent Membres
		ICollateralValuation ICollateralSubstitutionEvent.previousCollateral
		{
			set { this.previousCollateral = (CollateralValuation)value; }
			get { return this.previousCollateral; }
		}
		ICollateralValuation ICollateralSubstitutionEvent.newCollateral
		{
			set { this.newCollateral = (CollateralValuation)value; }
			get { return this.newCollateral; }
		}
		bool ICollateralSubstitutionEvent.settlementTransferSpecified
		{
			set { this.settlementTransferSpecified = value; }
			get { return this.settlementTransferSpecified; }
		}
		ISettlementTransfer ICollateralSubstitutionEvent.settlementTransfer
		{
			set { this.settlementTransfer = (SettlementTransfer)value; }
			get { return this.settlementTransfer; }
		}
		#endregion ICollateralSubstitutionEvent Membres
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
	}
	#endregion CollateralSubstitutionEvent
	#region CollateralValuation
	public partial class CollateralValuation : ICollateralValuation , IEFS_Array
	{
		#region Constructors
		public CollateralValuation()
		{
			instrumentUsedBondCollateral = new BondCollateral();
			instrumentUsedUnitContract   = new UnitContract();
		}
		#endregion Constructors
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region ICollateralValuation Members
		bool ICollateralValuation.bondCollateralSpecified
		{
			set {this.instrumentUsedBondCollateralSpecified = value;}
			get {return this.instrumentUsedBondCollateralSpecified;}
		}
		IBondCollateral ICollateralValuation.bondCollateral
		{
			set { this.instrumentUsedBondCollateral = (BondCollateral)value; }
			get { return this.instrumentUsedBondCollateral; }
		}
		bool ICollateralValuation.unitContractSpecified
		{
			set { this.instrumentUsedUnitContractSpecified = value; }
			get { return this.instrumentUsedUnitContractSpecified; }
		}
		IUnitContract ICollateralValuation.unitContract
		{
			set { this.instrumentUsedUnitContract = (UnitContract)value; }
			get { return this.instrumentUsedUnitContract; }
		}
		IReference ICollateralValuation.assetReference
		{
			set { this.assetReference = (AssetReference)value; }
			get { return this.assetReference; }
		}
		#endregion ICollateralValuation Members
	}
	#endregion  : CollateralValuation
	#region CouponEvent
	public partial class CouponEvent : ICouponEvent
	{
		#region ICouponEvent Membres
		EFS_Decimal ICouponEvent.couponAmount
		{
			set { this.couponAmount = value; }
			get { return this.couponAmount; }
		}
		EFS_Decimal ICouponEvent.reinvestmentRate
		{
			set { this.reinvestmentRate = value; }
			get { return this.reinvestmentRate; }
		}
		IReference ICouponEvent.assetReference
		{
			set { this.assetReference = (AssetReference)value; }
			get { return this.assetReference; }
		}
		bool ICouponEvent.transferSpecified
		{
			set { this.transferSpecified = value; }
			get { return this.transferSpecified; }
		}
		EFS_Decimal ICouponEvent.transfer
		{
			set { this.transfer = value; }
			get { return this.transfer; }
		}
		#endregion ICouponEvent Membres
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
	}
	#endregion CouponEvent

	#region EventIdentification
	public partial class EventIdentification : ITradeComponentIdentification
	{
		#region Constructors
		public EventIdentification()
		{
			byId			= new EventId[] { };
			byVersionedId	= new VersionedEventId[] { };
			byReference		= new EventReference();
		}
		#endregion Constructors
		#region ITradeComponentIdentification Members
		bool ITradeComponentIdentification.idSpecified
		{
			set { this.byIdSpecified = value; }
			get { return this.byIdSpecified; }
		}
		IScheme[] ITradeComponentIdentification.id
		{
			set { this.byId = (EventId[])value; }
			get { return this.byId; }
		}
		bool ITradeComponentIdentification.versionedIdSpecified
		{
			set { this.byVersionedIdSpecified = value; }
			get { return this.byVersionedIdSpecified; }
		}
		IVersionedId[] ITradeComponentIdentification.versionedId
		{
			set { this.byVersionedId = (VersionedEventId[])value; }
			get { return this.byVersionedId; }
		}
		bool ITradeComponentIdentification.referenceSpecified
		{
			set { this.byReferenceSpecified = value; }
			get { return this.byReferenceSpecified; }
		}
		IReference ITradeComponentIdentification.reference
		{
			set { this.byReference = (EventReference)value; }
			get { return this.byReference; }
		}
		#endregion ITradeComponentIdentification Members
	}
	#endregion EventIdentification
	#region EventReference
	public partial class EventReference : IReference
	{
		#region IReference Members
		string IReference.hRef
		{
			get { return this.href; }
			set { this.href = value; }
		}
		#endregion IReference Members
	}
	#endregion EventReference

	#region ForwardRepoTransactionLeg
	public partial class ForwardRepoTransactionLeg : IForwardRepoTransactionLeg
	{
		#region IForwardRepoTransactionLeg Members
		bool IForwardRepoTransactionLeg.repoInterestSpecified
		{
			set { this.repoInterestSpecified = value; }
			get { return this.repoInterestSpecified; }
		}
		EFS_Decimal IForwardRepoTransactionLeg.repoInterest
		{
			set { this.repoInterest = value; }
			get { return this.repoInterest; }
		}
		#endregion IForwardRepoTransactionLeg Members
		#region IRepoTransactionLeg Members
		bool IRepoTransactionLeg.identifierIdSpecified
		{
			set { this.identifierIdSpecified = value; }
			get { return this.identifierIdSpecified; }
		}
		IScheme[] IRepoTransactionLeg.identifierId
		{
			set { this.identifierId = (RepoLegId[]) value; }
			get { return this.identifierId; }
		}
		bool IRepoTransactionLeg.identifierVersionedIdSpecified
		{
			set { this.identifierVersionedIdSpecified = value; }
			get { return this.identifierVersionedIdSpecified; }
		}
		IVersionedId[] IRepoTransactionLeg.identifierVersionedId
		{
			set { this.identifierVersionedId = (VersionedRepoLegId[])value; }
			get { return this.identifierVersionedId; }
		}
		bool IRepoTransactionLeg.identifierNoneSpecified
		{
			set { this.identifierNoneSpecified = value; }
			get { return this.identifierNoneSpecified; }
		}
		object IRepoTransactionLeg.identifierNone
		{
			set { this.identifierNone = (Empty)value; }
			get { return this.identifierNone; }
		}
		IReference IRepoTransactionLeg.buyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IRepoTransactionLeg.sellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		IAdjustableOrRelativeDate IRepoTransactionLeg.settlementDate
		{
			set { this.settlementDate = (AdjustableOrRelativeDate)value; }
			get { return this.settlementDate; }
		}
		bool IRepoTransactionLeg.settlementAmountSpecified
		{
			set { this.typeSettlementAmountSpecified = value; }
			get { return this.typeSettlementAmountSpecified; }
		}
		IMoney IRepoTransactionLeg.settlementAmount
		{
			set { this.typeSettlementAmount = (Money)value; }
			get { return this.typeSettlementAmount; }
		}
		bool IRepoTransactionLeg.settlementCurrencySpecified
		{
			set { this.typeSettlementCurrencySpecified = value; }
			get { return this.typeSettlementCurrencySpecified; }
		}
		ICurrency IRepoTransactionLeg.settlementCurrency
		{
			set { this.typeSettlementCurrency = (Currency)value; }
			get { return this.typeSettlementCurrency; }
		}
		bool IRepoTransactionLeg.collateralValuationSpecified
		{
			set { this.collateralValuationSpecified = value; }
			get { return this.collateralValuationSpecified; }
		}
		ICollateralValuation[] IRepoTransactionLeg.collateralValuation
		{
			set { this.collateralValuation = (CollateralValuation[])value; }
			get { return this.collateralValuation; }
		}
		#endregion IRepoTransactionLeg Members	
	}
	#endregion ForwardRepoTransactionLeg

	#region InterestPayoutEvent
	public partial class InterestPayoutEvent : IInterestPayoutEvent
	{
		#region IInterestPayoutEvent Members

		IMoney IInterestPayoutEvent.payment
		{
			set { this.payment = (Money)value; }
			get { return this.payment; }
		}
		bool IInterestPayoutEvent.transferSpecified
		{
			set { this.transferSpecified = value; }
			get { return this.transferSpecified; }
		}
		EFS_Decimal IInterestPayoutEvent.transfer
		{
			set { this.transfer = value; }
			get { return this.transfer; }
		}
		#endregion IInterestPayoutEvent Members
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
	}
	#endregion InterestPayoutEvent

	#region Margin
	public partial class Margin : IMargin
	{
		#region IMargin Members
		MarginTypeEnum IMargin.marginType
		{
			set { this.marginType = value; }
			get { return this.marginType; }
		}
		EFS_Decimal IMargin.marginFactor
		{
			set { this.marginFactor = value; }
			get { return this.marginFactor; }
		}
		#endregion IMargin Members
	}
	#endregion Margin
	#region MarkToMarketEvent
	public partial class MarkToMarketEvent : IMarkToMarketEvent
	{
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
		#region IMarkToMarketEvent Members
		ICollateralValuation IMarkToMarketEvent.collateral
		{
			set { this.collateral = (CollateralValuation)value; }
			get { return this.collateral; }
		}
		EFS_Boolean IMarkToMarketEvent.combinedInterestPayout
		{
			set { this.combinedInterestPayout = value; }
			get { return this.combinedInterestPayout; }
		}
		bool IMarkToMarketEvent.transferSpecified
		{
			set { this.transferSpecified = value; }
			get { return this.transferSpecified; }
		}
		ITransfer IMarkToMarketEvent.transfer
		{
			set { this.transfer = (Transfer)value; }
			get { return this.transfer; }
		}
		#endregion IMarkToMarketEvent Members
	}
	#endregion MarkToMarketEvent
	#region MidLifeEvent
	public abstract partial class MidLifeEvent : IMidLifeEvent
	{
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate;}
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
	}
	#endregion MidLifeEvent

	#region NetTradeIdentifier
	public partial class NetTradeIdentifier : INetTradeIdentifier
	{
		#region INetTradeIdentifier Members
		ITradeIdentifierList[] INetTradeIdentifier.originalTradeIdentifier
		{
			get {return this.originalTradeIdentifier;}
			set {this.originalTradeIdentifier = (TradeIdentifierList[])value;}
		}
		#endregion INetTradeIdentifier Members
		#region IPartyTradeIdentifier Members
		IReference IPartyTradeIdentifier.partyReference
		{
			set { this.partyReference = (PartyReference)value; }
			get { return this.partyReference; }
		}
		bool IPartyTradeIdentifier.bookIdSpecified
		{
			set { this.bookIdSpecified = value; }
			get { return this.bookIdSpecified; }
		}
		IBookId IPartyTradeIdentifier.bookId
		{
			get { return this.bookId; }
		}
		bool IPartyTradeIdentifier.linkIdSpecified
		{
			set { this.linkIdSpecified = value; }
			get { return this.linkIdSpecified; }
		}
		ILinkId[] IPartyTradeIdentifier.linkId
		{
			get { return this.linkId; }
		}
		bool IPartyTradeIdentifier.localClassDervSpecified
		{
			set { this.localClassDervSpecified = value; }
			get { return this.localClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.localClassDerv
		{
			set { this.localClassDerv = (LocalClassDerv)value; }
			get { return this.localClassDerv; }
		}
		bool IPartyTradeIdentifier.localClassNDrvSpecified
		{
			set { this.localClassNDrvSpecified = value; }
			get { return this.localClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.localClassNDrv
		{
			set { this.localClassNDrv = (LocalClassNDrv)value; }
			get { return this.localClassNDrv; }
		}
		bool IPartyTradeIdentifier.iasClassDervSpecified
		{
			set { this.iasClassDervSpecified = value; }
			get { return this.iasClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.iasClassDerv
		{
			set { this.iasClassDerv = (IASClassDerv)value; }
			get { return this.iasClassDerv; }
		}
		bool IPartyTradeIdentifier.iasClassNDrvSpecified
		{
			set { this.iasClassNDrvSpecified = value; }
			get { return this.iasClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.iasClassNDrv
		{
			set { this.iasClassNDrv = (IASClassNDrv)value; }
			get { return this.iasClassNDrv; }
		}
		bool IPartyTradeIdentifier.hedgeClassDervSpecified
		{
			set { this.hedgeClassDervSpecified = value; }
			get { return this.hedgeClassDervSpecified; }
		}
		IScheme IPartyTradeIdentifier.hedgeClassDerv
		{
			set { this.hedgeClassDerv = (HedgeClassDerv)value; }
			get { return this.hedgeClassDerv; }
		}
		bool IPartyTradeIdentifier.hedgeClassNDrvSpecified
		{
			set { this.hedgeClassNDrvSpecified = value; }
			get { return this.hedgeClassNDrvSpecified; }
		}
		IScheme IPartyTradeIdentifier.hedgeClassNDrv
		{
			set { this.hedgeClassNDrv = (HedgeClassNDrv)value; }
			get { return this.hedgeClassNDrv; }
		}
		bool IPartyTradeIdentifier.fxClassSpecified
		{
			set { this.fxClassSpecified = value; }
			get { return this.fxClassSpecified; }
		}
		IScheme IPartyTradeIdentifier.fxClass
		{
			set { this.fxClass = (FxClass)value; }
			get { return this.fxClass; }
		}
		//
		bool IPartyTradeIdentifier.tradeIdSpecified
		{
			set { this.tradeTradeIdSpecified = value; }
			get { return this.tradeTradeIdSpecified; }
		}
		ISchemeId[] IPartyTradeIdentifier.tradeId
		{
			get
			{
				return this.tradeTradeId;
			}
		}
		//
		bool IPartyTradeIdentifier.versionedTradeIdSpecified
		{
			set { this.tradeVersionedTradeIdSpecified = value; }
			get { return this.tradeVersionedTradeIdSpecified; }
		}
		IVersionedShemeId[] IPartyTradeIdentifier.versionedTradeId
		{
			get
			{
				return this.tradeVersionedTradeId;
			}
		}

		ILinkId IPartyTradeIdentifier.GetLinkIdFromScheme(string pScheme) { return this.GetLinkIdFromScheme(pScheme); }
		ILinkId IPartyTradeIdentifier.GetLinkIdWithNoScheme() { return this.GetLinkIdWithNoScheme(); }
		#endregion IPartyTradeIdentifier Members
		#region ITradeIdentifier Members
		string ITradeIdentifier.GetTradeIdMemberName() { return "tradeTradeId"; }
		ISchemeId ITradeIdentifier.GetTradeIdFromScheme(string pScheme) { return this.GetTradeIdFromScheme(pScheme); }
		ISchemeId ITradeIdentifier.GetTradeIdWithNoScheme() { return this.GetTradeIdWithNoScheme(); }
		void ITradeIdentifier.RemoveTradeIdFromScheme(string pScheme) { this.RemoveTradeIdFromScheme(pScheme); }
		#endregion ITradeIdentifier Members
	}
	#endregion NetTradeIdentifier

	#region PartySettlementTransferInformation
	public partial class PartySettlementTransferInformation : IPartySettlementTransferInformation
	{
		#region IPartySettlementTransferInformation Members
		IReference IPartySettlementTransferInformation.partyReference
		{
			set { this.partyReference = (PartyReference)value; }
			get { return this.partyReference; }
		}
		ISettlementTransferProcessingInformation IPartySettlementTransferInformation.processingInformation
		{
			set { this.processingInformation = (SettlementTransferProcessingInformation)value; }
			get { return this.processingInformation; }
		}
		#endregion IPartySettlementTransferInformation Members
	}
	#endregion PartySettlementTransferInformation

	#region RateChangeEvent
	public partial class RateChangeEvent : IRateEvent
	{
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
		#region IRateEvent Members
		EFS_Decimal IRateEvent.rate
		{
			set { this.rate = value; }
			get { return this.rate; }
		}
		#endregion IRateEvent Members
	}
	#endregion RateChangeEvent
	#region RateObservationEvent
	public partial class RateObservationEvent : IRateEvent
	{
		#region IMidLifeEvent Members
		IAdjustedDate IMidLifeEvent.eventDate
		{
			set { this.eventDate = (IdentifiedDate)value; }
			get { return this.eventDate; }
		}
		#endregion IMidLifeEvent Members
		#region IEvent Members
		ISchemeId[] IEvent.eventId
		{
			set { this.eventId = (EventId[])value; }
			get { return this.eventId; }
		}
		#endregion IEvent Members
		#region IRateEvent Members
		EFS_Decimal IRateEvent.rate
		{
			set { this.rate = value; }
			get { return this.rate; }
		}
		#endregion IRateEvent Members
	}
	#endregion RateObservationEvent
	#region RelativePrice
	public partial class RelativePrice : IRelativePrice
	{
		#region Constructors
		public RelativePrice()
		{
			typeBond			= new Bond();
			typeConvertibleBond	= new ConvertibleBond();
			typeEquity			= new EquityAsset();
		}
		#endregion Constructors
		#region IRelativePrice Members
		EFS_Decimal IRelativePrice.spread
		{
			set{ this.spread = value; }
			get { return this.spread; }
		}
		bool IRelativePrice.bondSpecified
		{
			set { this.typeBondSpecified = value; }
			get { return this.typeBondSpecified; }
		}
		IBond IRelativePrice.bond
		{
			set { this.typeBond = (Bond)value; }
			get { return this.typeBond; }
		}
		bool IRelativePrice.convertibleBondSpecified
		{
			set { this.typeConvertibleBondSpecified = value; }
			get { return this.typeConvertibleBondSpecified; }
		}
		IConvertibleBond IRelativePrice.convertibleBond
		{
			set { this.typeConvertibleBond = (ConvertibleBond)value; }
			get { return this.typeConvertibleBond; }
		}
		bool IRelativePrice.equitySpecified
		{
			set { this.typeEquitySpecified = value; }
			get { return this.typeEquitySpecified; }
		}
		IEquityAsset IRelativePrice.equity
		{
			set { this.typeEquity = (EquityAsset)value; }
			get { return this.typeEquity; }
		}
		#endregion IRelativePrice Members
	}
	#endregion BondOption
	#region Repo
	public partial class Repo : IProduct, IRepo,IDeclarativeProvision
	{
		#region Constructors
		public Repo()
		{
			rateFixed		= new Schedule();
			rateFloating	= new FloatingRateCalculation();
		}
		#endregion Constructors
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IRepo Members
		bool IRepo.rateFixedSpecified
		{
			set { this.rateFixedSpecified = value; }
			get { return this.rateFixedSpecified; }
		}
		ISchedule IRepo.rateFixed
		{
			set { this.rateFixed = (Schedule)value; }
			get { return this.rateFixed; }
		}
		bool IRepo.rateFloatingSpecified
		{
			set { this.rateFloatingSpecified = value; }
			get { return this.rateFloatingSpecified; }
		}
		IFloatingRateCalculation IRepo.rateFloating
		{
			set { this.rateFloating = (FloatingRateCalculation)value; }
			get { return this.rateFloating; }
		}
		DayCountFractionEnum IRepo.dayCountFraction
		{
			set { this.dayCountFraction = value; }
			get { return this.dayCountFraction; }
		}
		bool IRepo.noticePeriodSpecified
		{
			set { this.noticePeriodSpecified = value; }
			get { return this.noticePeriodSpecified; }
		}
		IAdjustableOffset IRepo.noticePeriod
		{
			set { this.noticePeriod = (AdjustableOffset)value; }
			get { return this.noticePeriod; }
		}
		RepoDurationEnum IRepo.duration
		{
			set { this.duration = value; }
			get { return this.duration; }
		}
		IMargin IRepo.margin
		{
			set { this.margin = (Margin)value; }
			get { return this.margin; }
		}
		IRepoTransactionLeg IRepo.spotLeg
		{
			set { this.spotLeg = (RepoTransactionLeg)value; }
			get { return this.spotLeg; }
		}
		bool IRepo.forwardLegSpecified
		{
			set { this.forwardLegSpecified = value; }
			get { return this.forwardLegSpecified; }
		}
		IForwardRepoTransactionLeg IRepo.forwardLeg
		{
			set { this.forwardLeg = (ForwardRepoTransactionLeg)value; }
			get { return this.forwardLeg; }
		}
		bool IRepo.midLifeEventpecified
		{
			set { this.midLifeEventpecified = value; }
			get { return this.midLifeEventpecified; }
		}
		IMidLifeEvent[] IRepo.midLifeEvent
		{
			set { this.midLifeEvent = (MidLifeEvent[])value; }
			get { return this.midLifeEvent; }
		}
		bool IRepo.settlementTransferSpecified
		{
			set { this.settlementTransferSpecified = value; }
			get { return this.settlementTransferSpecified; }
		}
		ISettlementTransfer IRepo.settlementTransfer
		{
			set { this.settlementTransfer = (SettlementTransfer)value; }
			get { return this.settlementTransfer; }
		}
		#endregion IRepo Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion Repo
	#region RepoCollateral
	public partial class RepoCollateral : IRepoCollateral, IEFS_Array
	{
		#region Constructors
		public RepoCollateral()
		{
			typeBond			= new Bond();
			typeConvertibleBond	= new ConvertibleBond();
			typeEquity			= new EquityAsset();
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
		#region IRepoCollateral Members
		bool IRepoCollateral.bondSpecified
		{
			set { this.typeBondSpecified = value; }
			get { return this.typeBondSpecified; }
		}
		IBond IRepoCollateral.bond
		{
			set { this.typeBond = (Bond)value; }
			get { return this.typeBond; }
		}
		bool IRepoCollateral.convertibleBondSpecified
		{
			set { this.typeConvertibleBondSpecified = value; }
			get { return this.typeConvertibleBondSpecified; }
		}
		IConvertibleBond IRepoCollateral.convertibleBond
		{
			set { this.typeConvertibleBond = (ConvertibleBond)value; }
			get { return this.typeConvertibleBond; }
		}
		bool IRepoCollateral.equitySpecified
		{
			set { this.typeEquitySpecified = value; }
			get { return this.typeEquitySpecified; }
		}
		IEquityAsset IRepoCollateral.equity
		{
			set { this.typeEquity = (EquityAsset)value; }
			get { return this.typeEquity; }
		}
		#endregion IRepoCollateral Members
	}
	#endregion RepoCollateral
	#region RepoLegId
	public partial class RepoLegId : ISchemeId, IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IScheme Membres
		string IScheme.scheme
		{
			set { this.repoLegIdScheme = value; }
			get { return this.repoLegIdScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Membres
		#region ISchemeId Membres
		string ISchemeId.id
		{
			set { this.id = value; }
			get { return this.id; }
		}
		#endregion ISchemeId Membres
	}
	#endregion RepoLegId
	#region RepoLegIdentification
	public partial class RepoLegIdentification : ITradeComponentIdentification
	{
		#region Constructors
		public RepoLegIdentification()
		{
			byId			= new RepoLegId[] { };
			byVersionedId	= new VersionedRepoLegId[] { };
			byReference		= new RepoTransactionLegReference();
		}
		#endregion Constructors
		#region ITradeComponentIdentification Members
		bool ITradeComponentIdentification.idSpecified
		{
			set { this.byIdSpecified=value; }
			get { return this.byIdSpecified; }
		}
		IScheme[] ITradeComponentIdentification.id
		{
			set { this.byId = (RepoLegId[])value; }
			get { return this.byId; }
		}
		bool ITradeComponentIdentification.versionedIdSpecified
		{
			set { this.byVersionedIdSpecified = value; }
			get { return this.byVersionedIdSpecified; }
		}
		IVersionedId[] ITradeComponentIdentification.versionedId
		{
			set { this.byVersionedId = (VersionedRepoLegId[])value; }
			get { return this.byVersionedId; }
		}
		bool ITradeComponentIdentification.referenceSpecified
		{
			set { this.byReferenceSpecified = value; }
			get { return this.byReferenceSpecified; }
		}
		IReference ITradeComponentIdentification.reference
		{
			set { this.byReference = (RepoTransactionLegReference)value; }
			get { return this.byReference; }
		}
		#endregion ITradeComponentIdentification Members
	}
	#endregion RepoLegIdentification
	#region RepoTransactionLeg
	public partial class RepoTransactionLeg : IRepoTransactionLeg
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
		public RepoTransactionLeg()
		{
			identifierId			= new RepoLegId[]{};
			identifierVersionedId	= new VersionedRepoLegId[]{};
			identifierNone			= new Empty();
			identifierNoneSpecified = true;

			typeSettlementAmount	= new Money();
			typeSettlementCurrency	= new Currency();
		}
		#endregion Constructors
		#region IRepoTransactionLeg Members
		bool IRepoTransactionLeg.identifierIdSpecified
		{
			set { this.identifierIdSpecified = value; }
			get { return this.identifierIdSpecified; }
		}
		IScheme[] IRepoTransactionLeg.identifierId
		{
			set { this.identifierId = (RepoLegId[]) value; }
			get { return this.identifierId; }
		}
		bool IRepoTransactionLeg.identifierVersionedIdSpecified
		{
			set { this.identifierVersionedIdSpecified = value; }
			get { return this.identifierVersionedIdSpecified; }
		}
		IVersionedId[] IRepoTransactionLeg.identifierVersionedId
		{
			set { this.identifierVersionedId = (VersionedRepoLegId[])value; }
			get { return this.identifierVersionedId; }
		}
		bool IRepoTransactionLeg.identifierNoneSpecified
		{
			set { this.identifierNoneSpecified = value; }
			get { return this.identifierNoneSpecified; }
		}
		object IRepoTransactionLeg.identifierNone
		{
			set { this.identifierNone = (Empty)value; }
			get { return this.identifierNone; }
		}
		IReference IRepoTransactionLeg.buyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IRepoTransactionLeg.sellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		IAdjustableOrRelativeDate IRepoTransactionLeg.settlementDate
		{
			set { this.settlementDate = (AdjustableOrRelativeDate)value; }
			get { return this.settlementDate; }
		}
		bool IRepoTransactionLeg.settlementAmountSpecified
		{
			set { this.typeSettlementAmountSpecified = value; }
			get { return this.typeSettlementAmountSpecified; }
		}
		IMoney IRepoTransactionLeg.settlementAmount
		{
			set { this.typeSettlementAmount = (Money)value; }
			get { return this.typeSettlementAmount; }
		}
		bool IRepoTransactionLeg.settlementCurrencySpecified
		{
			set { this.typeSettlementCurrencySpecified = value; }
			get { return this.typeSettlementCurrencySpecified; }
		}
		ICurrency IRepoTransactionLeg.settlementCurrency
		{
			set { this.typeSettlementCurrency = (Currency)value; }
			get { return this.typeSettlementCurrency; }
		}
		bool IRepoTransactionLeg.collateralValuationSpecified
		{
			set { this.collateralValuationSpecified = value; }
			get { return this.collateralValuationSpecified; }
		}
		ICollateralValuation[] IRepoTransactionLeg.collateralValuation
		{
			set { this.collateralValuation = (CollateralValuation[])value; }
			get { return this.collateralValuation; }
		}
		#endregion IRepoTransactionLeg Members
	}
	#endregion RepoTransactionLeg
	#region RepoTransactionLegReference
	public partial class RepoTransactionLegReference : IReference
	{
		#region IReference Members
		string IReference.hRef
		{
			get { return this.href; }
			set { this.href = value; }
		}
		#endregion IReference Members
	}
	#endregion RepoTransactionLegReference

	#region SecurityTransfer
	public partial class SecurityTransfer : ISecurityTransfer
	{
		#region ISecurityTransfer Membres
		EFS_Decimal ISecurityTransfer.quantity
		{
			set { this.quantity = value; }
			get { return this.quantity; }
		}
		IReference ISecurityTransfer.assetReference
		{
			set { this.assetReference = (AssetReference)value; }
			get { return this.assetReference; }
		}
		IReference ISecurityTransfer.delivererPartyReference
		{
			set { this.delivererPartyReference = (PartyReference)value; }
			get { return this.delivererPartyReference; }
		}
		IReference ISecurityTransfer.receiverPartyReference
		{
			set { this.receiverPartyReference = (PartyReference)value; }
			get { return this.receiverPartyReference; }
		}
		bool ISecurityTransfer.daylightIndicatorSpecified
		{
			set { this.daylightIndicatorSpecified = value; }
			get { return this.daylightIndicatorSpecified; }
		}
		EFS_Boolean ISecurityTransfer.daylightIndicator
		{
			set { this.daylightIndicator = value; }
			get { return this.daylightIndicator; }
		}
		#endregion ISecurityTransfer Membres
		#region IAtomicSettlementTransfer Members
		bool IAtomicSettlementTransfer.suppressSpecified
		{
			set { this.suppressSpecified = value; }
			get { return this.suppressSpecified; }
		}
		bool IAtomicSettlementTransfer.suppress
		{
			set { this.suppress = new EFS_Boolean(value); }
			get { return this.suppress.BoolValue; }
		}
		#endregion IAtomicSettlementTransfer Members
	}
	#endregion SecurityTransfer
	#region SettlementInstructionReference
	public partial class SettlementInstructionReference : IReference
	{
		#region IReference Members
		string IReference.hRef
		{
			get { return this.href; }
			set { this.href = value; }
		}
		#endregion IReference Members
	}
	#endregion SettlementInstructionReference
	#region SettlementTransfer
	public partial class SettlementTransfer : ISettlementTransfer
	{
		#region Constructors
		public SettlementTransfer():base(){}
		#endregion Constructors
		#region ISettlementTransfer Members
		bool ISettlementTransfer.transferInformationSpecified
		{
			set { this.transferInformationSpecified = value; }
			get { return this.transferInformationSpecified; }
		}
		IPartySettlementTransferInformation[] ISettlementTransfer.transferInformation
		{
			set { this.transferInformation = (PartySettlementTransferInformation[])value; }
			get { return this.transferInformation; }
		}
		ITransfer[] ISettlementTransfer.transfer
		{
			set { this.transfer = (Transfer[]) value; }
			get { return this.transfer; }
		}
		bool ISettlementTransfer.settlementInstructionSpecified
		{
			set { this.settlementInstructionSpecified = value; }
			get { return this.settlementInstructionSpecified; }
		}
		ISettlementInstruction[] ISettlementTransfer.settlementInstruction
		{
			set { this.settlementInstruction = (SettlementInstruction[])value; }
			get { return this.settlementInstruction; }
		}
		#endregion ISettlementTransfer Members
		#region ISettlementTransferIdentifierBase Membres
		bool ISettlementTransferIdentifierBase.identifierIdSpecified
		{
			set { this.identifierIdSpecified = value; }
			get { return this.identifierIdSpecified; }
		}
		IScheme[] ISettlementTransferIdentifierBase.identifierId
		{
			set { this.identifierId = (SettlementTransferId[])value; }
			get { return this.identifierId; }
		}
		bool ISettlementTransferIdentifierBase.identifierVersionedIdSpecified
		{
			set { this.identifierVersionedIdSpecified = value; }
			get { return this.identifierVersionedIdSpecified; }
		}
		IVersionedId[] ISettlementTransferIdentifierBase.identifierVersionedId
		{
			set { this.identifierVersionedId = (VersionedSettlementTransferId[])value; }
			get { return this.identifierVersionedId; }
		}
		bool ISettlementTransferIdentifierBase.typeNoneSpecified
		{
			set { this.typeNoneSpecified = value; }
			get { return this.typeNoneSpecified; }
		}
		IEmpty ISettlementTransferIdentifierBase.typeNone
		{
			set { this.typeNone = (Empty)value; }
			get { return this.typeNone; }
		}
		bool ISettlementTransferIdentifierBase.typeTypeSpecified
		{
			set { this.typeTypeSpecified = value; }
			get { return this.typeTypeSpecified; }
		}
		IScheme[] ISettlementTransferIdentifierBase.typeType
		{
			set { this.typeType = (SettlementTransferType[])value; }
			get { return this.typeType; }
		}
		bool ISettlementTransferIdentifierBase.typeVersionedTypeSpecified
		{
			set { this.typeVersionedTypeSpecified = value; }
			get { return this.typeVersionedTypeSpecified; }
		}
		IVersionedId[] ISettlementTransferIdentifierBase.typeVersionedType
		{
			set { this.typeVersionedType = (VersionedSettlementTransferType[])value; }
			get { return this.typeVersionedType; }
		}
		#endregion ISettlementTransferIdentifierBase Membres
	}
	#endregion SettlementTransfer
	#region SettlementTransferId
	public partial class SettlementTransferId : ISchemeId
	{
		#region IScheme Membres
		string IScheme.scheme
		{
			set { this.settlementTransferIdScheme = value; }
			get { return this.settlementTransferIdScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Membres
		#region ISchemeId Membres
		string ISchemeId.id
		{
			set { this.id = value; }
			get { return this.id; }
		}
		#endregion ISchemeId Membres
	}
	#endregion SettlementTransferId
	#region SettlementTransferIdentifier
	public partial class SettlementTransferIdentifier : ISettlementTransferIdentifier
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
		public SettlementTransferIdentifier() : base(){}
		#endregion Constructors
		#region ISettlementTransferIdentifierBase Membres
		bool ISettlementTransferIdentifierBase.identifierIdSpecified
		{
			set { this.identifierIdSpecified = value; }
			get { return this.identifierIdSpecified; }
		}
		IScheme[] ISettlementTransferIdentifierBase.identifierId
		{
			set { this.identifierId = (SettlementTransferId[])value; }
			get { return this.identifierId; }
		}
		bool ISettlementTransferIdentifierBase.identifierVersionedIdSpecified
		{
			set { this.identifierVersionedIdSpecified = value; }
			get { return this.identifierVersionedIdSpecified; }
		}
		IVersionedId[] ISettlementTransferIdentifierBase.identifierVersionedId
		{
			set { this.identifierVersionedId = (VersionedSettlementTransferId[])value; }
			get { return this.identifierVersionedId; }
		}
		bool ISettlementTransferIdentifierBase.typeNoneSpecified
		{
			set { this.typeNoneSpecified = value; }
			get { return this.typeNoneSpecified; }
		}
		IEmpty ISettlementTransferIdentifierBase.typeNone
		{
			set { this.typeNone = (Empty)value; }
			get { return this.typeNone; }
		}
		bool ISettlementTransferIdentifierBase.typeTypeSpecified
		{
			set { this.typeTypeSpecified = value; }
			get { return this.typeTypeSpecified; }
		}
		IScheme[] ISettlementTransferIdentifierBase.typeType
		{
			set { this.typeType = (SettlementTransferType[])value; }
			get { return this.typeType; }
		}
		bool ISettlementTransferIdentifierBase.typeVersionedTypeSpecified
		{
			set { this.typeVersionedTypeSpecified = value; }
			get { return this.typeVersionedTypeSpecified; }
		}
		IVersionedId[] ISettlementTransferIdentifierBase.typeVersionedType
		{
			set { this.typeVersionedType = (VersionedSettlementTransferType[])value; }
			get { return this.typeVersionedType; }
		}
		#endregion ISettlementTransferIdentifierBase Membres
	}
	#endregion SettlementTransferIdentifier
	#region SettlementTransferIdentifierBase
	public partial class SettlementTransferIdentifierBase : ISettlementTransferIdentifierBase
	{
		#region Constructors
		public SettlementTransferIdentifierBase()
		{
			identifierId = new SettlementTransferId[] { };
			identifierVersionedId = new VersionedSettlementTransferId[] { };

			typeNone = new Empty();
			typeType = new SettlementTransferType[] { };
			typeVersionedType = new VersionedSettlementTransferType[] { };
		}
		#endregion Constructors
		#region ISettlementTransferIdentifierBase Membres
		bool ISettlementTransferIdentifierBase.identifierIdSpecified
		{
			set {this.identifierIdSpecified = value;}
			get { return this.identifierIdSpecified; }
		}
		IScheme[] ISettlementTransferIdentifierBase.identifierId
		{
			set { this.identifierId = (SettlementTransferId[])value; }
			get { return this.identifierId; }
		}
		bool ISettlementTransferIdentifierBase.identifierVersionedIdSpecified
		{
			set { this.identifierVersionedIdSpecified = value; }
			get { return this.identifierVersionedIdSpecified; }
		}
		IVersionedId[] ISettlementTransferIdentifierBase.identifierVersionedId
		{
			set { this.identifierVersionedId = (VersionedSettlementTransferId[])value; }
			get { return this.identifierVersionedId; }
		}
		bool ISettlementTransferIdentifierBase.typeNoneSpecified
		{
			set { this.typeNoneSpecified = value; }
			get { return this.typeNoneSpecified; }
		}
		IEmpty ISettlementTransferIdentifierBase.typeNone
		{
			set { this.typeNone = (Empty)value; }
			get { return this.typeNone; }
		}
		bool ISettlementTransferIdentifierBase.typeTypeSpecified
		{
			set { this.typeTypeSpecified = value; }
			get { return this.typeTypeSpecified; }
		}
		IScheme[] ISettlementTransferIdentifierBase.typeType
		{
			set { this.typeType = (SettlementTransferType[])value; }
			get { return this.typeType; }
		}
		bool ISettlementTransferIdentifierBase.typeVersionedTypeSpecified
		{
			set { this.typeVersionedTypeSpecified = value; }
			get { return this.typeVersionedTypeSpecified; }
		}
		IVersionedId[] ISettlementTransferIdentifierBase.typeVersionedType
		{
			set { this.typeVersionedType = (VersionedSettlementTransferType[])value; }
			get { return this.typeVersionedType; }
		}
		#endregion ISettlementTransferIdentifierBase Membres
	}
	#endregion SettlementTransferIdentifierBase
	#region SettlementTransferProcessingInformation
	public partial class SettlementTransferProcessingInformation : ISettlementTransferProcessingInformation
	{
		#region ISettlementTransferProcessingInformation Members
		bool ISettlementTransferProcessingInformation.ownerSpecified
		{
			get {return this.ownerSpecified;}
			set {this.ownerSpecified = value;}
		}
		EFS_Boolean ISettlementTransferProcessingInformation.owner
		{
			get { return this.owner; }
			set { this.owner = value; }
		}
		#endregion ISettlementTransferProcessingInformation Members
	}
	#endregion SettlementTransferProcessingInformation
	#region SettlementTransferType
	public partial class SettlementTransferType : IScheme
	{
		#region IScheme Membres
		string IScheme.scheme
		{
			set { this.settlementTransferTypeScheme = value; }
			get { return this.settlementTransferTypeScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Membres
	}
	#endregion SettlementTransferType
	#region StreamId
	public partial class StreamId : IScheme
	{
		#region IScheme Membres
		string IScheme.scheme
		{
			set { this.streamId = value; }
			get { return this.streamId; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Membres
	}
	#endregion StreamId
	#region StreamIdentification
	public partial class StreamIdentification : ITradeComponentIdentification
	{
		#region Constructors
		public StreamIdentification()
		{
			byId			= new StreamId[] { };
			byVersionedId	= new VersionedStreamId[] { };
			byReference		= new StreamReference();
		}
		#endregion Constructors
		#region ITradeComponentIdentification Members
		bool ITradeComponentIdentification.idSpecified
		{
			set { this.byIdSpecified = value; }
			get { return this.byIdSpecified; }
		}
		IScheme[] ITradeComponentIdentification.id
		{
			set { this.byId = (StreamId[])value; }
			get { return this.byId; }
		}
		bool ITradeComponentIdentification.versionedIdSpecified
		{
			set { this.byVersionedIdSpecified = value; }
			get { return this.byVersionedIdSpecified; }
		}
		IVersionedId[] ITradeComponentIdentification.versionedId
		{
			set { this.byVersionedId = (VersionedStreamId[])value; }
			get { return this.byVersionedId; }
		}
		bool ITradeComponentIdentification.referenceSpecified
		{
			set { this.byReferenceSpecified = value; }
			get { return this.byReferenceSpecified; }
		}
		IReference ITradeComponentIdentification.reference
		{
			set { this.byReference = (StreamReference)value; }
			get { return this.byReference; }
		}
		#endregion ITradeComponentIdentification Members
	}
	#endregion StreamIdentification
	#region StreamReference
	public partial class StreamReference : IReference
	{
		#region IReference Members
		string IReference.hRef
		{
			get { return this.href; }
			set { this.href = value; }
		}
		#endregion IReference Members
	}
	#endregion StreamReference

	#region TradeAndComponentIdentifier
	public partial class TradeAndComponentIdentifier : ITradeAndComponentIdentifier
	{
		#region Constructors
		public TradeAndComponentIdentifier()
		{
		}
		#endregion Constructors

		#region ITradeAndComponentIdentifier Members
		ITradeComponentIdentifier ITradeAndComponentIdentifier.tradeComponentIdentifier
		{
			set {this.tradeComponentIdentifier = (TradeComponentIdentifier)value;}
			get { return this.tradeComponentIdentifier; }
		}
		#endregion ITradeAndComponentIdentifier Members
		#region ITradeIdentifier Members
		string ITradeIdentifier.GetTradeIdMemberName() { return this.GetTradeIdMemberName(); }
		ISchemeId ITradeIdentifier.GetTradeIdFromScheme(string pScheme) { return this.GetTradeIdFromScheme(pScheme); }
		ISchemeId ITradeIdentifier.GetTradeIdWithNoScheme() { return this.GetTradeIdWithNoScheme(); }
		#endregion ITradeIdentifier Members
	}
	#endregion TradeAndComponentIdentifier
	#region TradeComponentIdentifier
	public partial class TradeComponentIdentifier : ITradeComponentIdentifier
	{
		#region ITradeComponentIdentifier Members
		bool ITradeComponentIdentifier.identificationRepoLegSpecified
		{
			set { this.identificationRepoLegSpecified = value; }
			get { return this.identificationRepoLegSpecified; }
		}
		ITradeComponentIdentification ITradeComponentIdentifier.identificationRepoLeg
		{
			set { this.identificationRepoLeg = (RepoLegIdentification)value; }
			get { return this.identificationRepoLeg; }
		}
		bool ITradeComponentIdentifier.identificationEventSpecified
		{
			set { this.identificationEventSpecified = value; }
			get { return this.identificationEventSpecified; }
		}
		ITradeComponentIdentification ITradeComponentIdentifier.identificationEvent
		{
			set { this.identificationEvent = (EventIdentification)value; }
			get { return this.identificationEvent; }
		}
		bool ITradeComponentIdentifier.identificationStreamSpecified
		{
			set { this.identificationStreamSpecified = value; }
			get { return this.identificationStreamSpecified; }
		}
		ITradeComponentIdentification ITradeComponentIdentifier.identificationStream
		{
			set { this.identificationStream = (StreamIdentification)value; }
			get { return this.identificationStream; }
		}
		#endregion ITradeComponentIdentifier Members
	}
	#endregion TradeComponentIdentifier
	#region TradeIdentifierList
	public partial class TradeIdentifierList : ITradeIdentifierList
	{
		#region ITradeIdentifierList Members
		ITradeIdentifier[] ITradeIdentifierList.tradeIdentifier
		{
			get {return this.tradeIdentifier;}
			set {this.tradeIdentifier = (TradeIdentifier[])value;}
		}
		#endregion ITradeIdentifierList Members
	}
	#endregion TradeIdentifierList
	#region Transfer
	public partial class Transfer : ITransfer
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
		#region Constructor
		public Transfer()
		{
			transferId			= new TransferId[] { };
			transferVersionedId	= new VersionedTransferId[] { };
			transferNone		= new Empty();

			identifierComponent			= new TradeComponentIdentifier();
			identifierTradeAndComponent = new TradeAndComponentIdentifier[] { };
			identifierNetTrade			= new NetTradeIdentifier();
			identifierNone				= new Empty();
		}
		#endregion Constructors

		#region ITransfer Members
		bool ITransfer.transferIdSpecified
		{
			get { return this.transferIdSpecified; }
			set { this.transferIdSpecified = value; }
		}
		IScheme[] ITransfer.transferId
		{
			get { return this.transferId; }
			set { this.transferId = (TransferId[])value; }
		}
		bool ITransfer.transferVersionedIdSpecified
		{
			get { return this.transferVersionedIdSpecified; }
			set { this.transferVersionedIdSpecified = value; }
		}
		IVersionedId[] ITransfer.transferVersionedId
		{
			get { return this.transferVersionedId; }
			set { this.transferVersionedId = (VersionedTransferId[])value; }
		}
		bool ITransfer.transferNoneSpecified
		{
			get { return this.transferNoneSpecified; }
			set { this.transferNoneSpecified = value; }
		}
		IEmpty ITransfer.transferNone
		{
			get { return this.transferNone; }
			set { this.transferNone = (Empty)value; }
		}
		bool ITransfer.identifierComponentSpecified
		{
			get { return this.identifierComponentSpecified; }
			set { this.identifierComponentSpecified = value; }
		}
		ITradeComponentIdentifier ITransfer.identifierComponent
		{
			get { return this.identifierComponent; }
			set { this.identifierComponent = (TradeComponentIdentifier)value; }
		}
		bool ITransfer.identifierTradeAndComponentSpecified
		{
			get { return this.identifierTradeAndComponentSpecified; }
			set { this.identifierTradeAndComponentSpecified = value; }
		}
		ITradeAndComponentIdentifier[] ITransfer.identifierTradeAndComponent
		{
			get { return this.identifierTradeAndComponent; }
			set { this.identifierTradeAndComponent = (TradeAndComponentIdentifier[])value; }
		}
		bool ITransfer.identifierNetTradeSpecified
		{
			get { return this.identifierNetTradeSpecified; }
			set { this.identifierNetTradeSpecified = value; }
		}
		INetTradeIdentifier ITransfer.identifierNetTrade
		{
			get { return this.identifierNetTrade; }
			set { this.identifierNetTrade = (NetTradeIdentifier)value; }
		}
		bool ITransfer.identifierNoneSpecified
		{
			get { return this.identifierNoneSpecified; }
			set { this.identifierNoneSpecified = value; }
		}
		IEmpty ITransfer.identifierNone
		{
			get { return this.identifierNone; }
			set { this.identifierNone = (Empty)value; }
		}
		DeliveryMethodEnum ITransfer.deliveryMethod
		{
			get { return this.deliveryMethod; }
			set { this.deliveryMethod = value; }
		}
		IAdjustedDate ITransfer.transferDate
		{
			get { return this.transferDate; }
			set { this.transferDate = (IdentifiedDate)value; }
		}
		bool ITransfer.cashTransferSpecified
		{
			get { return this.cashTransferSpecified; }
			set { this.cashTransferSpecified = value; }
		}
		ICashTransfer ITransfer.cashTransfer
		{
			get { return this.cashTransfer; }
			set { this.cashTransfer = (CashTransfer)value; }
		}
		bool ITransfer.securityTransferSpecified
		{
			get { return this.securityTransferSpecified; }
			set { this.securityTransferSpecified = value; }
		}
		ISecurityTransfer ITransfer.securityTransfer
		{
			get { return this.securityTransfer; }
			set { this.securityTransfer = (SecurityTransfer)value; }
		}
		bool ITransfer.settlementInstructionReferenceSpecified
		{
			get { return this.settlementInstructionReferenceSpecified; }
			set { this.settlementInstructionReferenceSpecified = value; }
		}
		IReference ITransfer.settlementInstructionReference
		{
			get { return this.settlementInstructionReference; }
			set { this.settlementInstructionReference = (SettlementInstructionReference)value; }
		}
		#endregion ITransfer Members
	}
	#endregion Transfer
	#region TransferId
	public partial class TransferId : ISchemeId
	{
		#region IScheme Membres
		string IScheme.scheme
		{
			set { this.transferIdScheme = value; }
			get { return this.transferIdScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Membres
		#region ISchemeId Membres
		string ISchemeId.id
		{
			set { this.id = value; }
			get { return this.id; }
		}
		#endregion ISchemeId Membres
	}
	#endregion TransferId

	#region UnitContract
	public partial class UnitContract : IUnitContract
	{
		#region IUnitContract Members
		EFS_Decimal IUnitContract.numberOfUnits
		{
			set { this.numberOfUnits = value; }
			get { return this.numberOfUnits; }
		}
		IMoney IUnitContract.unitPrice
		{
			set { this.unitPrice = (Money)value; }
			get { return this.unitPrice; }
		}
		#endregion IUnitContract Members
	}
	#endregion UnitContract

	#region VersionedEventId
	public partial class VersionedEventId : IVersionedEventId,IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IVersionedEventId Members
		ISchemeId IVersionedEventId.id
		{
			set { this.id = (EventId)value; }
			get { return this.id; }
		}
		#endregion IVersionedEventId Members
		#region IVersionedId Members
		EFS_NonNegativeInteger IVersionedId.version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IVersionedId.effectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IVersionedId.effectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		#endregion IVersionedId Members
	}
	#endregion VersionedEventId
	#region VersionedRepoLegId
	public partial class VersionedRepoLegId : IVersionedRepoLegId,IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IVersionedRepoLegId Members
		IScheme IVersionedRepoLegId.id
		{
			set { this.id = (RepoLegId)value; }
			get { return this.id; }
		}
		#endregion IVersionedEventId Members
		#region IVersionedId Members
		EFS_NonNegativeInteger IVersionedId.version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IVersionedId.effectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IVersionedId.effectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		#endregion IVersionedId Members
	}
	#endregion VersionedRepoLegId
	#region VersionedSettlementTransferId
	public partial class VersionedSettlementTransferId : IVersionedSettlementTransferId
	{
		#region IVersionedSettlementTransferId Members
		IScheme IVersionedSettlementTransferId.id
		{
			set { this.id = (SettlementTransferId)value; }
			get { return this.id; }
		}
		#endregion IVersionedSettlementTransferId Members
		#region IVersionedId Members
		EFS_NonNegativeInteger IVersionedId.version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IVersionedId.effectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IVersionedId.effectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		#endregion IVersionedId Members
	}
	#endregion VersionedSettlementTransferId
	#region VersionedSettlementTransferType
	public partial class VersionedSettlementTransferType : IVersionedSettlementTransferType
	{
		#region IVersionedSettlementTransferType Members
		IScheme IVersionedSettlementTransferType.type
		{
			set { this.type = (SettlementTransferType)value; }
			get { return this.type; }
		}
		#endregion IVersionedSettlementTransferType Members
		#region IVersionedId Members
		EFS_NonNegativeInteger IVersionedId.version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IVersionedId.effectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IVersionedId.effectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		#endregion IVersionedId Members
	}
	#endregion VersionedSettlementTransferType
	#region VersionedStreamId
	public partial class VersionedStreamId : IVersionedStreamId
	{
		#region IVersionedStreamId Members
		IScheme IVersionedStreamId.id
		{
			set { this.id = (StreamId)value; }
			get { return this.id; }
		}
		#endregion IVersionedStreamId Members
		#region IVersionedId Members
		EFS_NonNegativeInteger IVersionedId.version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IVersionedId.effectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IVersionedId.effectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		#endregion IVersionedId Members
	}
	#endregion VersionedStreamId
	#region VersionedTransferId
	public partial class VersionedTransferId : IVersionedTransferId
	{
		#region IVersionedTransferId Members
		IScheme IVersionedTransferId.id
		{
			set { this.id = (TransferId)value; }
			get { return this.id; }
		}
		#endregion IVersionedTransferId Members
		#region IVersionedId Members
		EFS_NonNegativeInteger IVersionedId.version
		{
			set { this.version = value; }
			get { return this.version; }
		}
		bool IVersionedId.effectiveDateSpecified
		{
			set { this.effectiveDateSpecified = value; }
			get { return this.effectiveDateSpecified; }
		}
		IAdjustedDate IVersionedId.effectiveDate
		{
			set { this.effectiveDate = (IdentifiedDate)value; }
			get { return this.effectiveDate; }
		}
		#endregion IVersionedId Members
	}
	#endregion VersionedTransferId

}

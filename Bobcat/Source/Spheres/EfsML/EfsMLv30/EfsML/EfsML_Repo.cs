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
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class AdjustableOffset : Offset
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
		public EFS_RadioChoice businessCenters;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool businessCentersNoneSpecified;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Empty businessCentersNone;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public BusinessCenters businessCentersDefine;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
		public BusinessCentersReference businessCentersReference;
		#endregion Members
	}
	#endregion AdjustableOffset
	#region AtomicSettlementTransfer
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public abstract partial class AtomicSettlementTransfer
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool suppressSpecified;
		[System.Xml.Serialization.XmlElementAttribute("suppress", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Suppress")]
		public EFS_Boolean suppress;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion AtomicSettlementTransfer
	#region Attribution
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class Attribution : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
		public AttributionType type;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool settlementAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementAmount", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement amount")]
		public EFS_Decimal settlementAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool baseAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("baseAmount", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Base amount")]
		public EFS_Decimal baseAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool underlyingAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("underlyingAmount", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying amount")]
		public EFS_Decimal underlyingAmount;
		#endregion Members
	}
	#endregion Attribution
	#region Attributions
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class Attributions : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementCurrency",Order = 1)]
		public Currency settlementCurrency;
		[System.Xml.Serialization.XmlElementAttribute("baseCurrency",Order = 2)]
		public Currency baseCurrency;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool underlyingCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("underlyingCurrency", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying currency")]
		public Currency underlyingCurrency;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool attributionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("attribution",Order=4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Attributions")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Attribution", IsClonable = true, IsChild = true)]
		public Attribution[] attribution;
		#endregion Members
	}
	#endregion Attributions
	#region AttributionType
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class AttributionType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		public string attributionTypeScheme;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		public string Value;
		#endregion Members
	}
	#endregion AttributionType

	#region BondCollateral
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class BondCollateral : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("nominalAmount", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nominal amount", IsVisible = true)]
		public Money nominalAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cleanPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cleanPrice", Order = 2)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nominal amount")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clean price")]
		public EFS_Decimal cleanPrice;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool accrualsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruals", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accruals")]
		public EFS_Decimal accruals;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool dirtyPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dirtyPrice", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dirty price")]
		public EFS_Decimal dirtyPrice;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool relativePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativePrice", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Relative price")]
		public RelativePrice relativePrice;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool yieldToMaturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("yieldToMaturity", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Yield to maturity")]
		public EFS_Decimal yieldToMaturity;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool inflationFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("inflationFactor", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Inflation factor")]
		public EFS_Decimal inflationFactor;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool interestStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("interestStartDate", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Interest start date")]
		public AdjustableOrRelativeDate interestStartDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool poolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("pool", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asset pool")]
		public AssetPool pool;
		#endregion Members
	}
	#endregion BondCollateral

	#region CashRepricingEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class CashRepricingEvent : MidLifeEvent
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool collateralSpecified;
        [System.Xml.Serialization.XmlElementAttribute("collateral", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Collateral")]
		public CollateralValuation collateral;
        [System.Xml.Serialization.XmlElementAttribute("combinedInterestPayout", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Combined interest payout")]
		public EFS_Boolean combinedInterestPayout;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transfer", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transfer")]
		public Transfer transfer;
		#endregion Members
	}
	#endregion CashRepricingEvent
	#region CashTransfer
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class CashTransfer : AtomicSettlementTransfer
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("transferAmount", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End,Name = "Transfer amount")]
		public Money transferAmount;
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 2)]
		[ControlGUI(Name = "Payer", LblWidth = 120)]
		public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 3)]
		[ControlGUI(Name = "Receiver", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool attributionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("attributions", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Attributions")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Attributions", IsClonable = true, IsChild = true)]
		public Attributions[] attributions;
		#endregion Members
	}
	#endregion CashTransfer
	#region CollateralSubstitutionEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class CollateralSubstitutionEvent : MidLifeEvent
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("previousCollateral", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Previous collateral")]
		public CollateralValuation previousCollateral;
        [System.Xml.Serialization.XmlElementAttribute("newCollateral", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "New collateral")]
		public CollateralValuation newCollateral;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool settlementTransferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementTransfer", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement transfer")]
		public SettlementTransfer settlementTransfer;
		#endregion Members
	}
	#endregion CollateralSubstitutionEvent
	#region CollateralValuation
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class CollateralValuation
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type of collateral")]
		public EFS_RadioChoice instrumentUsed;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool instrumentUsedBondCollateralSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bondCollateral", typeof(BondCollateral), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public BondCollateral instrumentUsedBondCollateral;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool instrumentUsedUnitContractSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unitContract", typeof(UnitContract), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public UnitContract instrumentUsedUnitContract;
		[System.Xml.Serialization.XmlElementAttribute("assetReference", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Asset reference")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.AssetReference)]
		public AssetReference assetReference;
		#endregion Members
	}
	#endregion  : CollateralValuation
	#region CouponEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class CouponEvent : MidLifeEvent
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("couponAmount", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon amount")]
		public EFS_Decimal couponAmount;
        [System.Xml.Serialization.XmlElementAttribute("reinvestmentRate", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reinvestment rate")]
		public EFS_Decimal reinvestmentRate;
        [System.Xml.Serialization.XmlElementAttribute("assetReference", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asset reference")]
		public AssetReference assetReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transfer", Order = 4)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transfer")]
		public EFS_Decimal transfer;
		#endregion Members
	}
	#endregion CouponEvent

	#region EventIdentification
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class EventIdentification
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "by")]
		public EFS_RadioChoice by;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventId", typeof(EventId), Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EventId[] byId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byVersionedIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedEventId", typeof(VersionedEventId), Order = 2)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public VersionedEventId[] byVersionedId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventReference", typeof(EventReference), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EventReference byReference;
		#endregion Members
	}
	#endregion EventIdentification
	#region EventReference
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class EventReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
		public string href;
		#endregion Members
	}
	#endregion EventReference

	#region ForwardRepoTransactionLeg
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class ForwardRepoTransactionLeg : RepoTransactionLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool repoInterestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("repoInterest", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo interest")]
		public EFS_Decimal repoInterest;
		#endregion Members
	}
	#endregion ForwardRepoTransactionLeg

	#region InterestPayoutEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class InterestPayoutEvent : MidLifeEvent
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount")]
		public Money payment;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transfer", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transfer")]
		public EFS_Decimal transfer;
		#endregion Members
	}
	#endregion InterestPayoutEvent

	#region Margin
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class Margin : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("marginType", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Type")]
		public MarginTypeEnum marginType;
        [System.Xml.Serialization.XmlElementAttribute("marginFactor", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.None, Name = "Factor")]
		public EFS_Decimal marginFactor;
		#endregion Members
	}
	#endregion Margin
	#region MarkToMarketEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class MarkToMarketEvent : MidLifeEvent
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("collateral", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Collateral")]
		public CollateralValuation collateral;
        [System.Xml.Serialization.XmlElementAttribute("combinedInterestPayout", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Combined interest payout")]
		public EFS_Boolean combinedInterestPayout;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transfer", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transfer")]
		public Transfer transfer;
		#endregion Members
	}
	#endregion MarkToMarketEvent
	#region MidLifeEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CashRepricingEvent))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CollateralSubstitutionEvent))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(CouponEvent))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestPayoutEvent))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(MarkToMarketEvent))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(RateChangeEvent))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(RateObservationEvent))]
	public abstract partial class MidLifeEvent : FpML.v44.Doc.ToDefine.Event
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("eventDate", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event date")]
		public IdentifiedDate eventDate;
		#endregion Members
	}
	#endregion MidLifeEvent

	#region NetTradeIdentifier
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class NetTradeIdentifier : PartyTradeIdentifier
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("originalTradeIdentifier", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Identification of original trades.")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Identification", IsClonable = true, IsChild = true, MinItem = 2)]
		public TradeIdentifierList[] originalTradeIdentifier;
		#endregion Members
	}
	#endregion NetTradeIdentifier

	#region PartySettlementTransferInformation
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class PartySettlementTransferInformation
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party reference")]
		public PartyReference partyReference;
        [System.Xml.Serialization.XmlElementAttribute("processingInformation", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Processing information")]
		public SettlementTransferProcessingInformation processingInformation;
		#endregion Members
	}
	#endregion PartySettlementTransferInformation

	#region RateChangeEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RateChangeEvent : MidLifeEvent
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("rate", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Rate")]
		public EFS_Decimal rate;
		#endregion Members
	}
	#endregion RateChangeEvent
	#region RateObservationEvent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RateObservationEvent : MidLifeEvent
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("rate", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Rate")]
		public EFS_Decimal rate;
		#endregion Members
	}
	#endregion RateObservationEvent
	#region RelativePrice
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RelativePrice
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("spread", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Spread")]
		public EFS_Decimal spread;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike of the Bond Option")]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
		public EFS_RadioChoice type;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bond", IsVisible = true)]
		public Bond typeBond;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeConvertibleBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "ConvertibleBond", IsVisible = true)]
		public ConvertibleBond typeConvertibleBond;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeEquitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("equity", typeof(EquityAsset), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Equity", IsVisible = true)]
		public EquityAsset typeEquity;
		#endregion Members
	}
	#endregion RelativePrice
	#region Repo
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlRootAttribute("repo", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
	public partial class Repo : Product
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate information", IsVisible = false)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Type")]
		public EFS_RadioChoice rate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool rateFixedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixedRateSchedule", typeof(Schedule),Order=1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed", IsVisible = true)]
		public Schedule rateFixed;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool rateFloatingSpecified;
		[System.Xml.Serialization.XmlElementAttribute("floatingRateCalculation", typeof(FloatingRateCalculation), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating", IsVisible = true)]
		public FloatingRateCalculation rateFloating;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Day count fraction")]
		public DayCountFractionEnum dayCountFraction;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool noticePeriodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("noticePeriod", Order = 4)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate information")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notice period")]
		public AdjustableOffset noticePeriod;
		[System.Xml.Serialization.XmlElementAttribute("duration", Order = 5)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Duration")]
		public RepoDurationEnum duration;
		[System.Xml.Serialization.XmlElementAttribute("margin", Order = 6)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin")]
		public Margin margin;
		[System.Xml.Serialization.XmlElementAttribute("spotLeg", Order = 7)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Margin")]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot leg")]
		public RepoTransactionLeg spotLeg;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool forwardLegSpecified;
		[System.Xml.Serialization.XmlElementAttribute("forwardLeg", Order = 8)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot leg")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forward leg")]
		public ForwardRepoTransactionLeg forwardLeg;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool midLifeEventpecified;
		[System.Xml.Serialization.XmlElementAttribute("midLifeEvent", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Mid life events")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Event", IsClonable = true, IsChild = true)]
		public MidLifeEvent[] midLifeEvent;
		[System.Xml.Serialization.XmlElementAttribute("collateral", Order = 10)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Collaterals", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true,IsCopyPasteItem = true)]
		public RepoCollateral[] collaterals;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool settlementTransferSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementTransfer", Order = 13)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Settlement transfer")]
		public SettlementTransfer settlementTransfer;
		#endregion Members
	}
	#endregion Repo
	#region RepoCollateral
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	[System.Xml.Serialization.XmlRootAttribute("collateral", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
	public partial class RepoCollateral
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
		public EFS_RadioChoice type;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeBondSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond),Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bonds", IsVisible = true)]
		public Bond typeBond;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeConvertibleBondSpecified;
		[System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "ConvertibleBonds", IsVisible = true)]
		public ConvertibleBond typeConvertibleBond;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeEquitySpecified;
		[System.Xml.Serialization.XmlElementAttribute("equity", typeof(EquityAsset), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Equities", IsVisible = true)]
		public EquityAsset typeEquity;
		#endregion Members
	}
	#endregion RepoCollateral
	#region RepoLegId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RepoLegId : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_SchemeValue repoLegId;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		[ControlGUI(Name = "Scheme", Width = 350)]
		public string repoLegIdScheme;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		[ControlGUI(Name = "value", Width = 100)]
		public string Value;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[ControlGUI(Name = "Id", Width = 50)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		public string id;
		#endregion Members
	}
	#endregion RepoLegId
	#region RepoLegIdentification
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RepoLegIdentification
	{

		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "by")]
		public EFS_RadioChoice by;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("repoLegId", typeof(RepoLegId), Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public RepoLegId[] byId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byVersionedIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedRepoLegId", typeof(VersionedRepoLegId), Order = 2)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public VersionedRepoLegId[] byVersionedId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("repoLegReference", typeof(RepoTransactionLegReference), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public RepoTransactionLegReference byReference;
		#endregion Members
	}
	#endregion RepoLegIdentification
	#region RepoTransactionLeg
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RepoTransactionLeg : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Identification by")]
		public EFS_RadioChoice identifier;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("id", typeof(RepoLegId), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repo Leg id")]
		public RepoLegId[] identifierId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierVersionedIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedId", typeof(VersionedRepoLegId), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Versioned ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id",IsChild = true)]
		public VersionedRepoLegId[] identifierVersionedId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierNoneSpecified;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Empty identifierNone;

        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[ControlGUI(Name = "Buyer",LineFeed=MethodsGUI.LineFeedEnum.Before)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
		[ControlGUI(Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement informations", IsVisible = false, IsGroup = true)]
		public bool FillBalise;

        [System.Xml.Serialization.XmlElementAttribute("settlementDate", Order = 5)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
		public AdjustableOrRelativeDate settlementDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date")]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
		public EFS_RadioChoice typeSettlement;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeSettlementAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementAmount", typeof(Money), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money typeSettlementAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeSettlementCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(Currency), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Currency typeSettlementCurrency;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool collateralValuationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("collateralValuation", Order = 8)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement informations")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Collateral valuations")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "valuation", IsClonable = true,IsChild = true)]
		[BookMarkGUI(IsVisible = false)]
		public CollateralValuation[] collateralValuation;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;

		#endregion Members
	}
	#endregion RepoTransactionLeg
	#region RepoTransactionLegReference
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class RepoTransactionLegReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
		public string href;
		#endregion Members
	}
	#endregion RepoTransactionLegReference

	#region SecurityTransfer
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SecurityTransfer : AtomicSettlementTransfer
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("quantity", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Quantity")]
		public EFS_Decimal quantity;
		[System.Xml.Serialization.XmlElementAttribute("assetReference", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asset reference")]
		public AssetReference assetReference;
		[System.Xml.Serialization.XmlElementAttribute("delivererPartyReference", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Deliverer party reference")]
		public PartyReference delivererPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 4)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Receiver party reference")]
		public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool daylightIndicatorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("daylightIndicator", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Daylight indicator")]
		public EFS_Boolean daylightIndicator;
		#endregion Members
	}
	#endregion SecurityTransfer
	#region SettlementInstructionReference
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementInstructionReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
		public string href;
		#endregion Members
	}
	#endregion SettlementInstructionReference
	#region SettlementTransfer
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementTransfer : SettlementTransferIdentifierBase
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("transferInformation", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Party settlement transfer informations")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party settlement transfer information", IsClonable = true, IsChild = true)]
		public PartySettlementTransferInformation[] transferInformation;

        [System.Xml.Serialization.XmlElementAttribute("transfer", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Elementary transfers")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Elementary transfer", IsClonable = true, IsChild = true)]
		public Transfer[] transfer;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool settlementInstructionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInstruction", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement instructions")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement instruction", IsClonable = true, IsChild = true)]
		public SettlementInstruction[] settlementInstruction;
		#endregion Members
	}
	#endregion SettlementTransfer
	#region SettlementTransferId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementTransferId : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_SchemeValue settlementTransferId;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		[ControlGUI(Name = "Scheme", Width = 350)]
		public string settlementTransferIdScheme;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		[ControlGUI(Name = "value", Width = 100)]
		public string Value;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "Id", Width = 50)]
		public string id;
		#endregion Members
	}
	#endregion SettlementTransferId
	#region SettlementTransferIdentifier
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementTransferIdentifier : SettlementTransferIdentifierBase
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion SettlementTransferIdentifier
	#region SettlementTransferIdentifierBase
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementTransferIdentifierBase : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement transfer identifier")]
		public EFS_RadioChoice identifier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("id", typeof(SettlementTransferId), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		public SettlementTransferId[] identifierId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierVersionedIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedId", typeof(VersionedSettlementTransferId), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Versioned ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Versioned id")]
		public VersionedSettlementTransferId[] identifierVersionedId;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement transfer type")]
		public EFS_RadioChoice type;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeNoneSpecified;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Empty typeNone;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("type", typeof(SettlementTransferType), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Types", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "type")]
		public SettlementTransferType[] typeType;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeVersionedTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedType", typeof(VersionedSettlementTransferType), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Versioned types", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Versioned type")]
		public VersionedSettlementTransferType[] typeVersionedType;
		#endregion Members
	}
	#endregion SettlementTransferIdentifierBase
	#region SettlementTransferProcessingInformation
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementTransferProcessingInformation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool ownerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("owner", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Owner")]
		public EFS_Boolean owner;
		#endregion Members
	}
	#endregion SettlementTransferProcessingInformation
	#region SettlementTransferType
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class SettlementTransferType : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_SchemeValue settlementTransferType;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		[ControlGUI(Name = "Scheme", Width = 350)]
		public string settlementTransferTypeScheme;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		[ControlGUI(Name = "value", Width = 100)]
		public string Value;
		#endregion Members
	}
	#endregion SettlementTransferType
	#region StreamId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class StreamId : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_SchemeValue streamIdValue;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		[ControlGUI(Name = "Stream Id", Width = 350)]
		public string streamId;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		[ControlGUI(Name = "value", Width = 100)]
		public string Value;
		#endregion Members
	}
	#endregion StreamId
	#region StreamIdentification
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class StreamIdentification
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "by")]
		public EFS_RadioChoice by;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("streamId", typeof(StreamId), Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public StreamId[] byId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byVersionedIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedStreamId", typeof(VersionedStreamId), Order = 2)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public VersionedStreamId[] byVersionedId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool byReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("streamReference", typeof(StreamReference), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public StreamReference byReference;
		#endregion Members
	}
	#endregion StreamIdentification
	#region StreamReference
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class StreamReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
		public string href;
		#endregion Members
	}
	#endregion StreamReference

	#region TradeAndComponentIdentifier
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class TradeAndComponentIdentifier : TradeIdentifier
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("tradeComponentIdentifier", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trade component identifier")]
		public TradeComponentIdentifier tradeComponentIdentifier;
		#endregion Members
	}
	#endregion TradeAndComponentIdentifier
	#region TradeComponentIdentifier
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class TradeComponentIdentifier
	{

		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Identification")]
		public EFS_RadioChoice identification;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identificationRepoLegSpecified;
        [System.Xml.Serialization.XmlElementAttribute("repoLegIdentification", typeof(RepoLegIdentification), Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public RepoLegIdentification identificationRepoLeg;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identificationEventSpecified;
        [System.Xml.Serialization.XmlElementAttribute("eventIdentification", typeof(EventIdentification), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EventIdentification identificationEvent;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identificationStreamSpecified;
        [System.Xml.Serialization.XmlElementAttribute("streamIdentification", typeof(StreamIdentification), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public StreamIdentification identificationStream;
		#endregion Members
	}
	#endregion TradeComponentIdentifier
	#region TradeIdentifierList
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class TradeIdentifierList
	{

		#region Members
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trade identifiers")]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Identifier", IsClonable = true, IsChild = true, MinItem = 2)]
		public TradeIdentifier[] tradeIdentifier;
		#endregion Members
	}
	#endregion TradeIdentifierList
	#region Transfer
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class Transfer
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Transfer identifiers")]
		public EFS_RadioChoice transfer;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("id", typeof(TransferId), Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public TransferId[] transferId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferVersionedIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("versionedId", typeof(VersionedTransferId), Order = 2)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public VersionedTransferId[] transferVersionedId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool transferNoneSpecified;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Empty transferNone;


		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Identifiation")]
		public EFS_RadioChoice identifier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierComponentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeComponentIdentifier", typeof(TradeComponentIdentifier), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public TradeComponentIdentifier identifierComponent;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierTradeAndComponentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeAndComponentIdentifier", typeof(TradeAndComponentIdentifier), Order = 4)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids", IsVisible = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Id")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public TradeAndComponentIdentifier[] identifierTradeAndComponent;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierNetTradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netTradeIdentifier", typeof(NetTradeIdentifier), Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public NetTradeIdentifier identifierNetTrade;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool identifierNoneSpecified;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Empty identifierNone;

        [System.Xml.Serialization.XmlElementAttribute("deliveryMethod", Order = 6)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Delivery method")]
		public DeliveryMethodEnum deliveryMethod;

        [System.Xml.Serialization.XmlElementAttribute("transferDate", Order = 7)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Transfer date")]
		public IdentifiedDate transferDate;


		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cashTransferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashTransfer", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash transfer")]
		public CashTransfer cashTransfer;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool securityTransferSpecified;
        [System.Xml.Serialization.XmlElementAttribute("securityTransfer", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Security transfer")]
		public SecurityTransfer securityTransfer;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool settlementInstructionReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInstructionReference", Order = 10)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement instruction")]
		public SettlementInstructionReference settlementInstructionReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
		#endregion Members
	}
	#endregion Transfer
	#region TransferId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class TransferId : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
		public EFS_SchemeValue transferId;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
		[ControlGUI(Name = "Scheme", Width = 350)]
		public string transferIdScheme;
		[System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
		[ControlGUI(Name = "value", Width = 100)]
		public string Value;
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
		[ControlGUI(Name = "Id", Width = 50)]
		public string id;
		#endregion Members
	}
	#endregion TransferId

	#region UnitContract
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class UnitContract
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("numberOfUnits", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of units")]
		public EFS_Decimal numberOfUnits;
        [System.Xml.Serialization.XmlElementAttribute("unitPrice", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit price")]
		public Money unitPrice;
		#endregion Members
	}
	#endregion UnitContract

	#region VersionedEventId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class VersionedEventId : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("id", Order = 1)]
		public EventId id;
        [System.Xml.Serialization.XmlElementAttribute("version", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
		public EFS_NonNegativeInteger version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
		public IdentifiedDate effectiveDate;
		#endregion Members
	}
	#endregion VersionedEventId
	#region VersionedRepoLegId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class VersionedRepoLegId : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("id", Order = 1)]
		public RepoLegId id;
        [System.Xml.Serialization.XmlElementAttribute("version", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
		public EFS_NonNegativeInteger version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End,Name = "Effective Date")]
		public IdentifiedDate effectiveDate;
		#endregion Members
	}
	#endregion VersionedRepoLegId
	#region VersionedSettlementTransferId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class VersionedSettlementTransferId : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 1)]
		public SettlementTransferId id;
        [System.Xml.Serialization.XmlElementAttribute("version", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
		public EFS_NonNegativeInteger version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
		public IdentifiedDate effectiveDate;
		#endregion Members
	}
	#endregion VersionedSettlementTransferId
	#region VersionedSettlementTransferType
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class VersionedSettlementTransferType : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("type", Order = 1)]
		public SettlementTransferType type;
        [System.Xml.Serialization.XmlElementAttribute("version", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
		public EFS_NonNegativeInteger version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
		public IdentifiedDate effectiveDate;
		#endregion Members
	}
	#endregion VersionedSettlementTransferType
	#region VersionedStreamId	
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class VersionedStreamId : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("id", Order = 1)]
		public StreamId id;
        [System.Xml.Serialization.XmlElementAttribute("version", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
		public EFS_NonNegativeInteger version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
		public IdentifiedDate effectiveDate;
		#endregion Members
	}
	#endregion VersionedStreamId
	#region VersionedTransferId
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
	public partial class VersionedTransferId : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("id", Order = 1)]
		public TransferId id;
        [System.Xml.Serialization.XmlElementAttribute("version", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
		public EFS_NonNegativeInteger version;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
		public IdentifiedDate effectiveDate;
		#endregion Members
	}
	#endregion VersionedTransferId
}

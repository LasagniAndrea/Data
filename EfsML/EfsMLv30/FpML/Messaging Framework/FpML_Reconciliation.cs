#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.Enum;
using FpML.v44.Riskdef.ToDefine;
using FpML.v44.Doc;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Msg.ToDefine;
using FpML.v44.Reporting.ToDefine;
using FpML.v44.Shared;
using FpML.v44.ValuationResults.ToDefine;
#endregion using directives

namespace FpML.v44.Reconciliation
{
    #region TradeCashflowsStatus
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeCashflowsStatus : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string tradeCashflowsStatusScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public TradeCashflowsStatus()
        {
            tradeCashflowsStatusScheme = "http://www.fpml.org/coding-scheme/trade-cashflows-status-1-0";
        }
        #endregion Constructors
    }
    #endregion TradeCashflowsStatus
}

namespace FpML.v44.Reconciliation.ToDefine
{

    #region AllegedCashflow
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AllegedCashflow
    {
        public System.DateTime asOfDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool asOfDateSpecified;
        public TradeCashflowsId tradeCashflowsId;
        public TradeIdentifyingItems tradeIdentifyingItems;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("payment")]
        public PaymentMatching[] payment;
    }
    #endregion AllegedCashflow
    #region AssertedCashflow
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AssertedCashflow
    {
        public System.DateTime asOfDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool asOfDateSpecified;
        public TradeCashflowsId tradeCashflowsId;
        public TradeIdentifyingItems tradeIdentifyingItems;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("payment")]
        public PaymentMatching[] payment;
    }
    #endregion AssertedCashflow
    #region AssertedPosition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AssertedPosition
    {
        public PositionId positionId;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string version;
        public ReportingRoles reportingRoles;
        public PositionConstituent constituent;
        [System.Xml.Serialization.XmlElementAttribute("scheduledDate")]
        public ScheduledDate[] scheduledDate;
        [System.Xml.Serialization.XmlElementAttribute("valuation")]
        public AssetValuation[] valuation;
    }
    #endregion AssertedPosition

    #region CalculationDetails
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CalculationDetails
    {
        public GrossCashflow grossCashflow;
        [System.Xml.Serialization.XmlElementAttribute("observationElements")]
        public CashflowObservation[] observationElements;
        public CashflowCalculationElements calculationElements;
    }
    #endregion CalculationDetails
    #region CancelTradeCashflows
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CancelTradeCashflows : NotificationMessage
    {
        public TradeCashflowsId tradeCashflowsId;
        public TradeIdentifyingItems tradeIdentifyingItems;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("payment")]
        public PaymentMatching[] payment;
        public MatchId matchId;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion CancelTradeCashflows
    #region CashflowCalculationElements
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowCalculationElements
    {
        public UnderlyerReferenceUnits numberOfUnits;
        public CashflowNotional notional;
        [System.Xml.Serialization.XmlElementAttribute("underlyer")]
        public TradeUnderlyer[] underlyer;
        [System.Xml.Serialization.XmlElementAttribute("calculatedRate")]
        public CashflowFixing[] calculatedRate;
        [System.Xml.Serialization.XmlElementAttribute("calculationPeriod")]
        public CashflowCalculationPeriod[] calculationPeriod;
    }
    #endregion CashflowCalculationElements
    #region CashflowCalculationPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowCalculationPeriod
    {
        [System.Xml.Serialization.XmlElementAttribute("calculatedRateReference")]
        public CashflowFixingReference[] calculatedRateReference;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool adjustedStartDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool adjustedEndDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string numberOfDays;
        public StepReference fixedRateStepReference;
        public DayCountFraction dayCountFraction;
        public System.Decimal dayCountYearFraction;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dayCountYearFractionSpecified;
        public CompoundingMethodEnum compoundingMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool compoundingMethodSpecified;
        public System.Decimal accruedAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool accruedAmountSpecified;
    }
    #endregion CashflowCalculationPeriod
    #region CashflowFixing
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowFixing
    {
        [System.Xml.Serialization.XmlElementAttribute("observationReference")]
        public CashflowObservationReference[] observationReference;
        public System.Decimal calculatedValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool calculatedValueSpecified;
        public System.Decimal multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool multiplierSpecified;
        public System.Decimal spread;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("capValue")]
        public Strike[] capValue;
        [System.Xml.Serialization.XmlElementAttribute("floorValue")]
        public Strike[] floorValue;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion CashflowFixing
    #region CashflowFixingReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowFixingReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion CashflowFixingReference
    #region CashflowId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string cashflowIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion CashflowId
    #region CashflowNotional
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowNotional
    {
        [System.Xml.Serialization.XmlElementAttribute("currency", typeof(Currency))]
        [System.Xml.Serialization.XmlElementAttribute("units", typeof(string), DataType = "normalizedString")]
        public object Item;
        public System.Decimal amount;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion CashflowNotional
    #region CashflowObservation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowObservation
    {
        public TradeUnderlyerReference underlyerReference;
        public object Item;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime observationDate;
        public BasicQuotation observedValue;
        public System.Decimal weight;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool weightSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion CashflowObservation
    #region CashflowObservationReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CashflowObservationReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion CashflowObservationReference

    #region DefinePosition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DefinePosition : Position
    {
        public PositionReference forceMatch;
    }
    #endregion DefinePosition

    #region GrossCashflow
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class GrossCashflow
    {
        public CashflowId cashflowId;
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        public Money cashflowAmount;
        public CashflowType cashflowType;
    }
    #endregion GrossCashflow

    #region InitialPortfolioDefinition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class InitialPortfolioDefinition : PortfolioDefinition
    {
        public bool newPortfolioDefinition;
    }
    #endregion InitialPortfolioDefinition

    #region MatchId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MatchId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string matchIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion MatchId

    #region PaymentId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PaymentId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string paymentIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion PaymentId
    #region PaymentMatching
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PaymentMatching
    {
        public PaymentId identifier;
        public PartyOrAccountReference payerPartyReference;
        public PartyOrAccountReference receiverPartyReference;
        public Money paymentAmount;
        [System.Xml.Serialization.XmlElementAttribute("calculationDetails")]
        public CalculationDetails[] calculationDetails;
    }
    #endregion PaymentMatching
    #region PortfolioDefinition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InitialPortfolioDefinition))]
    public class PortfolioDefinition
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString")]
        public string portfolioName;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime asOfDate;
        public PartyReference definingParty;
        public PartyReference matchingParty;
    }
    #endregion PortfolioDefinition
    #region PositionMatchResult
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionMatchResult
    {
        public PositionMatchStatus status;
        [System.Xml.Serialization.XmlElementAttribute("allegedPosition", typeof(AssertedPosition))]
        [System.Xml.Serialization.XmlElementAttribute("assertedPosition", typeof(AssertedPosition))]
        [System.Xml.Serialization.XmlElementAttribute("proposedMatch", typeof(PositionProposedMatch))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;
    }
    #endregion PositionMatchResult
    #region PositionMatchStatus
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionMatchStatus
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/position-status-1-0")]
        public string positionStatusScheme = "http://www.fpml.org/coding-scheme/position-status-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion PositionMatchStatus
    #region PositionProposedMatch
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionProposedMatch
    {
        public PositionId positionId;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string version;
        public ReportingRoles reportingRoles;
        public PositionConstituent constituent;
        [System.Xml.Serialization.XmlElementAttribute("scheduledDate")]
        public ScheduledDate[] scheduledDate;
        [System.Xml.Serialization.XmlElementAttribute("valuation")]
        public AssetValuation[] valuation;
        public MatchId matchId;
        [System.Xml.Serialization.XmlElementAttribute("difference")]
        public TradeDifference[] difference;
    }
    #endregion PositionProposedMatch
    #region PositionReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionReference
    {
        public PositionId positionId;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string version;
    }
    #endregion PositionReference
    #region PositionsAcknowledged
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionsAcknowledged : ResponseMessage
    {
        public PortfolioDefinition portfolio;
        [System.Xml.Serialization.XmlElementAttribute("unprocessedPosition", typeof(UnprocessedPosition))]
        [System.Xml.Serialization.XmlElementAttribute("definedPosition", typeof(PositionReference))]
        [System.Xml.Serialization.XmlElementAttribute("removedPosition", typeof(PositionReference))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion PositionsAcknowledged
    #region PositionsAsserted
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionsAsserted : RequestMessage
    {
        public InitialPortfolioDefinition portfolio;
        public bool submissionsComplete;
        [System.Xml.Serialization.XmlElementAttribute("removePosition", typeof(PositionReference))]
        [System.Xml.Serialization.XmlElementAttribute("replaceAllPositions", typeof(Empty))]
        [System.Xml.Serialization.XmlElementAttribute("definePosition", typeof(DefinePosition))]
        public object[] Items;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion PositionsAsserted
    #region PositionsMatchResults
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionsMatchResults : ResponseMessage
    {
        public PortfolioDefinition portfolio;
        [System.Xml.Serialization.XmlElementAttribute("positionMatchResult")]
        public PositionMatchResult[] positionMatchResult;
        public bool matchCompleted;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion PositionsMatchResults

    #region RequestPortfolio
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestPortfolio : RequestMessage
    {
        public object asOfDate;
        [System.Xml.Serialization.XmlElementAttribute("requestedPositions", typeof(RequestedPositions))]
        [System.Xml.Serialization.XmlElementAttribute("portfolioName", typeof(string), DataType = "normalizedString")]
        public object Item;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestPortfolio

    #region StepReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class StepReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion StepReference

    #region TradeCashflowsAsserted
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeCashflowsAsserted : NotificationMessage
    {
        public System.DateTime asOfDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool asOfDateSpecified;
        public TradeCashflowsId tradeCashflowsId;
        public TradeIdentifyingItems tradeIdentifyingItems;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("payment")]
        public PaymentMatching[] payment;
        public MatchId matchId;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeCashflowsAsserted
    #region TradeCashflowsId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeCashflowsId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string tradeCashflowsIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion TradeCashflowsId
    #region TradeCashflowsMatchResult
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeCashflowsMatchResult : ResponseMessage
    {
        public TradeCashflowsStatus status;
        [System.Xml.Serialization.XmlElementAttribute("assertedCashflow", typeof(AssertedCashflow))]
        [System.Xml.Serialization.XmlElementAttribute("allegedCashflow", typeof(AllegedCashflow))]
        [System.Xml.Serialization.XmlElementAttribute("proposedMatch", typeof(TradeCashflowsProposedMatch))]
        public object[] Items;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeCashflowsMatchResult
    #region TradeCashflowsProposedMatch
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeCashflowsProposedMatch
    {
        public TradeCashflowsId tradeCashflowsId;
        public TradeIdentifyingItems tradeIdentifyingItems;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime adjustedPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("payment")]
        public PaymentMatching[] payment;
        public MatchId matchId;
        [System.Xml.Serialization.XmlElementAttribute("difference")]
        public TradeDifference[] difference;
    }
    #endregion TradeCashflowsProposedMatch
    #region TradeDetails
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeDetails
    {
        public IdentifiedDate tradeDate;
        public AdjustableDate2 effectiveDate;
        public AdjustableDate2 terminationDate;
        public ProductType productType;
        [System.Xml.Serialization.XmlElementAttribute("underlyer")]
        public TradeUnderlyer[] underlyer;
        [System.Xml.Serialization.XmlElementAttribute("notional")]
        public CashflowNotional[] notional;
    }
    #endregion TradeDetails
    #region TradeIdentifyingItems
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeIdentifyingItems
    {
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier")]
        public PartyTradeIdentifier[] partyTradeIdentifier;
        public TradeDetails tradeDetails;
    }
    #endregion TradeIdentifyingItems
    #region TradeUnderlyer
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeUnderlyer
    {
        [System.Xml.Serialization.XmlElementAttribute("referenceEntity", typeof(LegalEntity))]
        [System.Xml.Serialization.XmlElementAttribute("fixedRate", typeof(Schedule))]
        [System.Xml.Serialization.XmlElementAttribute("floatingRate", typeof(FloatingRate))]
        public object Item;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion TradeUnderlyer
    #region TradeUnderlyerReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeUnderlyerReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion TradeUnderlyerReference

    #region UnderlyerReferenceUnits
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class UnderlyerReferenceUnits
    {
        public TradeUnderlyerReference underlyerReference;
        public System.Decimal quantity;
    }
    #endregion UnderlyerReferenceUnits
    #region UnprocessedPosition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class UnprocessedPosition
    {
        public PositionId positionId;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string version;
        [System.Xml.Serialization.XmlElementAttribute("reason")]
        public Reason[] reason;
    }
    #endregion UnprocessedPosition
}

#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Allocation.ToDefine;
using FpML.v44.Confirmation.ToDefine;
using FpML.v44.ContractNotification.ToDefine;
using FpML.v44.CreditEvent.Notification.ToDefine;
using FpML.v44.Doc;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Matching.Status.ToDefine;
using FpML.v44.PreTrade.ToDefine;
using FpML.v44.PostTrade.ToDefine;
using FpML.v44.PostTrade.Confirmation.ToDefine;
using FpML.v44.PostTrade.Negotiation.ToDefine;
using FpML.v44.PostTrade.Execution.ToDefine;
using FpML.v44.Reconciliation.ToDefine;
using FpML.v44.Reporting.ToDefine;
using FpML.v44.Shared;
/*using FpML.v44.TradeExecution.ToDefine;*/
using FpML.v44.TradeRequest.ToDefine;
using FpML.v44.TradeNotification.ToDefine;

#endregion using directives


namespace FpML.v44.Msg.ToDefine
{
    #region AdditionalData
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AdditionalData
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string additionalDataScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion AdditionalData

    #region ConversationId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ConversationId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string conversationIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ConversationId

    #region Message
    // EG 20140702 New build FpML4.4 TradeAmended DEPRECATED
    // EG 20140702 New build FpML4.4 TradeCreated DEPRECATED
    // EG 20140702 New build FpML4.4 TradeCancelled DEPRECATED
    // EG 20140702 New build FpML4.4 new TradeExecution
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NotificationMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractIncreased))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractFullTermination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractReferenceMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IncreaseConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRejected))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMismatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeCashflows))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeUnmatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TerminationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationNotificationMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationAmended))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventNotification))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCashflowsAsserted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractPartialTermination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmendmentConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResponseMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AcceptQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAlreadyExpired))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeErrorResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyTerminated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNotFound))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAcceptanceConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsAcknowledged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationResponseMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentGranted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRefused))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadySubmitted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCashflowsMatchResult))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuoteResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteUpdated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsMatchResults))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsAsserted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestPortfolio))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAllocation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTerminationConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationRequestMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovateTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestNovationConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestIncreaseConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestValuationReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestPositionReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAmendmentConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeExecution))]
    public abstract class Message : Document { }
    #endregion Message
    #region MessageAddress
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MessageAddress
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/ext/iso9362")]
        //public string partyIdScheme = "http://www.fpml.org/ext/iso9362";
        //20090416 PL Use OTCml_ActorIdentifierScheme
        public string partyIdScheme = Cst.OTCml_ActorIdentifierScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion MessageAddress
    #region MessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NotificationMessageHeader))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResponseMessageHeader))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestMessageHeader))]
    public abstract class MessageHeader
    {
        public ConversationId conversationId;
        public MessageId messageId;
    }
    #endregion MessageHeader
    #region MessageId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MessageId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string messageIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion MessageId
    #region MessageRejected
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MessageRejected : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("reason")]
        public Reason[] reason;
        public AdditionalData additionalData;
    }
    #endregion MessageRejected

    #region NotificationMessage
    // EG 20140702 New build FpML4.4 TradeAmended DEPRECATED
    // EG 20140702 New build FpML4.4 TradeCreated DEPRECATED
    // EG 20140702 New build FpML4.4 TradeCancelled DEPRECATED
    // EG 20140702 New build FpML4.4 new TradeExecution
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractIncreased))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractFullTermination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractReferenceMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IncreaseConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRejected))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMismatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeCashflows))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeUnmatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TerminationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationNotificationMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationAmended))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventNotification))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCashflowsAsserted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractPartialTermination))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmendmentConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeExecution))]
    public abstract class NotificationMessage : Message
    {
        [System.Xml.Serialization.XmlElementAttribute("header", Order = 1)]
        public NotificationMessageHeader header;
        [System.Xml.Serialization.XmlElementAttribute("validation", Order = 2)]
        public Validation[] validation;
    }
    #endregion NotificationMessage
    #region NotificationMessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NotificationMessageHeader : MessageHeader
    {
        [System.Xml.Serialization.XmlElementAttribute("inReplyTo", Order = 1)]
        public MessageId inReplyTo;
        [System.Xml.Serialization.XmlElementAttribute("sentBy", Order = 2)]
        public MessageAddress sentBy;
        [System.Xml.Serialization.XmlElementAttribute("sendTo",Order=3)]
        public MessageAddress[] sendTo;
        [System.Xml.Serialization.XmlElementAttribute("copyTo", Order = 4)]
        public MessageAddress[] copyTo;
        [System.Xml.Serialization.XmlElementAttribute("creationTimestamp", Order = 5)]
        public System.DateTime creationTimestamp;
        [System.Xml.Serialization.XmlElementAttribute("expiryTimestamp", Order = 6)]
        public System.DateTime expiryTimestamp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expiryTimestampSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partyMessageInformation",Order = 7)]
        public PartyMessageInformation[] partyMessageInformation;
    }
    #endregion NotificationMessageHeader

    #region PartyMessageInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PartyMessageInformation
    {
        public PartyReference partyReference;
    }
    #endregion PartyMessageInformation
    #region ProblemLocation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ProblemLocation
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string locationType;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string problemLocationScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ProblemLocation

    #region Reason
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Reason
    {
        public ReasonCode reasonCode;
        public ProblemLocation location;
        public string description;
        public Validation validationRuleId;
        [System.Xml.Serialization.XmlElementAttribute("additionalData")]
        public AdditionalData[] additionalData;
    }
    #endregion Reason
    #region ReasonCode
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReasonCode
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/reason-code-1-0")]
        public string reasonCodeScheme = "http://www.fpml.org/coding-scheme/reason-code-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ReasonCode
    #region RequestMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsAsserted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestPortfolio))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAllocation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTerminationConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationRequestMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovateTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestNovationConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestIncreaseConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestValuationReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestPositionReport))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAmendmentConfirmation))]
    public abstract class RequestMessage : Message
    {
        public RequestMessageHeader header;
        [System.Xml.Serialization.XmlElementAttribute("validation")]
        public Validation[] validation;
    }
    #endregion RequestMessage
    #region RequestMessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestMessageHeader : MessageHeader
    {
        public MessageAddress sentBy;
        [System.Xml.Serialization.XmlElementAttribute("sendTo")]
        public MessageAddress[] sendTo;
        [System.Xml.Serialization.XmlElementAttribute("copyTo")]
        public MessageAddress[] copyTo;
        public System.DateTime creationTimestamp;
        public System.DateTime expiryTimestamp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expiryTimestampSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partyMessageInformation")]
        public PartyMessageInformation[] partyMessageInformation;
    }
    #endregion RequestMessageHeader
    #region RequestTradeStatus
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestTradeStatus : RequestMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public TradeIdentifier[] tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTradeStatus
    #region ResponseMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AcceptQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAlreadyExpired))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeErrorResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyTerminated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNotFound))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAcceptanceConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsAcknowledged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationResponseMessage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentGranted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRefused))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadySubmitted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCashflowsMatchResult))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuoteResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteUpdated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PositionsMatchResults))]
    public abstract class ResponseMessage : Message
    {
        public ResponseMessageHeader header;
        [System.Xml.Serialization.XmlElementAttribute("validation")]
        public Validation[] validation;
    }
    #endregion ResponseMessage
    #region ResponseMessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ResponseMessageHeader : MessageHeader
    {
        public MessageId inReplyTo;
        public MessageAddress sentBy;
        [System.Xml.Serialization.XmlElementAttribute("sendTo")]
        public MessageAddress[] sendTo;
        [System.Xml.Serialization.XmlElementAttribute("copyTo")]
        public MessageAddress[] copyTo;
        public System.DateTime creationTimestamp;
        public System.DateTime expiryTimestamp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expiryTimestampSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partyMessageInformation")]
        public PartyMessageInformation[] partyMessageInformation;
    }
    #endregion ResponseMessageHeader

    #region TradeAlreadyCancelled
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlreadyCancelled : TradeErrorResponse { }
    #endregion TradeAlreadyCancelled
    #region TradeAlreadySubmitted
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlreadySubmitted : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion TradeAlreadySubmitted
    #region TradeAlreadyTerminated
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlreadyTerminated : TradeErrorResponse { }
    #endregion TradeAlreadyTerminated
    #region TradeErrorResponse
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyTerminated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyCancelled))]
    public abstract class TradeErrorResponse : ResponseMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlArrayItemAttribute("partyTradeIdentifier", IsNullable = false)]
        public PartyTradeIdentifier[] tradeReference;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeErrorResponse
    #region TradeNotFound
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeNotFound : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public TradeIdentifier Item;
        public Party party;
    }
    #endregion TradeNotFound
    #region TradeStatus
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeStatus : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeStatusItem")]
        public TradeStatusItem[] tradeStatusItem;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeStatus
    #region TradeStatusItem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeStatusItem
    {
        public TradeIdentifier tradeIdentifier;
        public TradeStatusValue tradeStatusValue;
    }
    #endregion TradeStatusItem
    #region TradeStatusValue
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeStatusValue
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string tradeStatusScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion TradeStatusValue
}
